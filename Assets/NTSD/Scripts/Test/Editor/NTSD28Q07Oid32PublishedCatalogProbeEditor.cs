#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q07Oid32PublishedCatalogProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q07_Oid32PublishedCatalog.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-OID32-PUBLISHED-CATALOG-001";
        private static readonly int[] BoundaryOids = { 55, 32, 30, 30, 31, 31 };
        private static readonly int[] BoundaryPics = { 44, 64, 81, 91, 81, 91 };

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public long startedUtcTicks;
            public bool allBoundaryCells;
        }

        [Serializable]
        private sealed class BoundaryCell
        {
            public int oid;
            public int pic;
            public bool found;
            public string sourceSheetPath;
            public int textureWidth;
            public int textureHeight;
            public bool legacySpritePresent;
            public bool centralBindingValid;
            public bool nativeClampedSource;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string runId;
            public string scenePath;
            public bool sceneDirty;
            public string contentRoot;
            public string publishedVisualKey;
            public int observedTick;
            public int catalogCount;
            public bool oid32DataLoaded;
            public bool pic0Found;
            public string pic0SourceSheet;
            public int pic0TextureWidth;
            public int pic0TextureHeight;
            public bool pic0CentralBindingValid;
            public bool pic64Found;
            public string pic64SourceSheet;
            public int pic64TextureWidth;
            public int pic64TextureHeight;
            public bool pic64CentralBindingValid;
            public bool pic64LegacySpritePresent;
            public string pic64CentralBindingMode;
            public int nativeClampedCatalogEntryCount;
            public int nativeClampedUniqueSourceCount;
            public int nativeClampedUniqueTextureCount;
            public int nativeClampedInvalidCentralBindingCount;
            public BoundaryCell[] boundaryCells;
            public string scope;
        }

        static NTSD28Q07Oid32PublishedCatalogProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static bool ValidRunId(string runId)
        {
            if (string.IsNullOrEmpty(runId) || runId.Length > 80)
                return false;
            foreach (char value in runId)
            {
                if (!char.IsLetterOrDigit(value) && value != '-' && value != '_')
                    return false;
            }
            return true;
        }

        private static void Poll()
        {
            string path = ProjectPath(RequestPath);
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(path))
                return;

            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || (!request.requested && !request.running))
                return;

            string output = ValidRunId(request.runId)
                ? ProjectPath(Path.Combine(ResultRoot, request.runId + ".json"))
                : null;
            if (request.requested && !request.running)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (output == null || File.Exists(output) ||
                    scene.path != BattleScene || scene.isDirty)
                {
                    request.requested = false;
                    File.WriteAllText(path, JsonUtility.ToJson(request));
                    Debug.LogError("[Q07 OID32 Catalog Probe] Invalid request, output collision or unsaved Battle Scene.");
                    return;
                }
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(path, JsonUtility.ToJson(request));
                EditorApplication.EnterPlaymode();
                return;
            }

            if (!EditorApplication.isPlaying)
                return;
            var report = new Report
            {
                runId = request.runId,
                scenePath = SceneManager.GetActiveScene().path,
                sceneDirty = SceneManager.GetActiveScene().isDirty,
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
                scope = "Original saved Battle Scene formal prewarm SpriteCatalog read only; no entity draw or pixel proof."
            };
            try
            {
                if (DateTime.UtcNow - new DateTime(request.startedUtcTicks, DateTimeKind.Utc) >
                    TimeSpan.FromSeconds(180))
                    throw new TimeoutException("Formal Battle content prewarm did not become ready.");
                SimulationTickDriver driver = SimulationTickDriver.Instance;
                if (driver?.World == null || driver.CurrentTickIndex < 5 ||
                    !CharacterAnimtorManager.HasInstance || !GameDataManager.HasInstance)
                    return;

                CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
                report.observedTick = driver.CurrentTickIndex;
                report.publishedVisualKey = GameDataManager.Instance.PublishedVisualContentKey;
                report.catalogCount = manager.SpriteCatalog?.Count ?? 0;
                var nativeClampedPaths = new HashSet<string>(StringComparer.Ordinal);
                var nativeClampedTextures = new HashSet<Texture2D>();
                if (manager.SpriteCatalog != null)
                {
                    foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in
                             manager.SpriteCatalog.Entries)
                    {
                        BattleSpriteEntry entry = pair.Value;
                        if (entry == null || string.IsNullOrEmpty(entry.SourceSheetPath) ||
                            entry.SourceSheetPath.Replace('\\', '/').IndexOf(
                                "/Temp/NTSD28NativeClampCells/",
                                StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        report.nativeClampedCatalogEntryCount++;
                        nativeClampedPaths.Add(entry.SourceSheetPath);
                        if (entry.SharedTexture != null)
                            nativeClampedTextures.Add(entry.SharedTexture);
                        if (!entry.CentralBinding.IsValid)
                            report.nativeClampedInvalidCentralBindingCount++;
                    }
                }
                report.nativeClampedUniqueSourceCount = nativeClampedPaths.Count;
                report.nativeClampedUniqueTextureCount = nativeClampedTextures.Count;
                report.oid32DataLoaded = manager.GetCharacterData(32) != null;
                report.pic0Found = manager.TryGetSpriteEntry(32, 0,
                    out BattleSpriteEntry pic0);
                if (pic0 != null)
                {
                    report.pic0SourceSheet = pic0.SourceSheetPath;
                    report.pic0TextureWidth = pic0.SharedTexture?.width ?? 0;
                    report.pic0TextureHeight = pic0.SharedTexture?.height ?? 0;
                    report.pic0CentralBindingValid = pic0.CentralBinding.IsValid;
                }
                report.pic64Found = manager.TryGetSpriteEntry(32, 64,
                    out BattleSpriteEntry pic64);
                if (pic64 != null)
                {
                    report.pic64SourceSheet = pic64.SourceSheetPath;
                    report.pic64TextureWidth = pic64.SharedTexture?.width ?? 0;
                    report.pic64TextureHeight = pic64.SharedTexture?.height ?? 0;
                    report.pic64CentralBindingValid = pic64.CentralBinding.IsValid;
                    report.pic64LegacySpritePresent = pic64.LegacySprite != null;
                    report.pic64CentralBindingMode = pic64.CentralBinding.Mode.ToString();
                }
                if (report.scenePath != BattleScene || report.sceneDirty ||
                    report.contentRoot != FormalRoot ||
                    string.IsNullOrEmpty(report.publishedVisualKey) ||
                    report.catalogCount <= 0 || !report.oid32DataLoaded ||
                    !report.pic0Found || !report.pic0CentralBindingValid ||
                    string.IsNullOrEmpty(report.pic0SourceSheet) ||
                    !report.pic0SourceSheet.Replace('\\', '/').EndsWith(
                        "m/nin/hun.png", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Formal OID32 catalog control or Battle Scene precondition failed.");
                }
                if (request.allBoundaryCells)
                {
                    report.boundaryCells = new BoundaryCell[BoundaryOids.Length];
                    for (int index = 0; index < BoundaryOids.Length; index++)
                    {
                        var cell = new BoundaryCell
                        {
                            oid = BoundaryOids[index],
                            pic = BoundaryPics[index]
                        };
                        cell.found = manager.TryGetSpriteEntry(cell.oid, cell.pic,
                            out BattleSpriteEntry entry);
                        if (entry != null)
                        {
                            cell.sourceSheetPath = entry.SourceSheetPath;
                            cell.textureWidth = entry.SharedTexture?.width ?? 0;
                            cell.textureHeight = entry.SharedTexture?.height ?? 0;
                            cell.legacySpritePresent = entry.LegacySprite != null;
                            cell.centralBindingValid = entry.CentralBinding.IsValid;
                            cell.nativeClampedSource = !string.IsNullOrEmpty(cell.sourceSheetPath) &&
                                cell.sourceSheetPath.Replace('\\', '/').IndexOf(
                                    "/Temp/NTSD28NativeClampCells/",
                                    StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        report.boundaryCells[index] = cell;
                    }
                    report.scope = "Formal Battle prewarm six-key SpriteCatalog publication only; no entity draw or pixel proof.";
                    foreach (BoundaryCell cell in report.boundaryCells)
                    {
                        if (!cell.found || !cell.nativeClampedSource ||
                            cell.textureWidth != 79 || cell.textureHeight != 79 ||
                            !cell.legacySpritePresent || !cell.centralBindingValid)
                        {
                            throw new InvalidOperationException(
                                "Formal boundary-cell catalog first difference at OID" +
                                cell.oid + "/pic" + cell.pic + ".");
                        }
                    }
                }
                report.status = "OBSERVED";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }

            Directory.CreateDirectory(ProjectPath(ResultRoot));
            try
            {
                using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(report, true));
            }
            finally
            {
                request.running = false;
                File.WriteAllText(path, JsonUtility.ToJson(request));
                EditorApplication.ExitPlaymode();
            }
        }
    }
}
#endif
