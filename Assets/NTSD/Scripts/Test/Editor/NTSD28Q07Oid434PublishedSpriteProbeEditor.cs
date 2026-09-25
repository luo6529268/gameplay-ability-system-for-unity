#if UNITY_EDITOR
using System;
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
    internal static class NTSD28Q07Oid434PublishedSpriteProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_Oid434PublishedSprite.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-OID434-PUBLISHED-SPRITE-001";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string ExpectedSheet = "c/nar/a/ras.png";

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public long startedUtcTicks;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string runId;
            public string scenePath;
            public bool sceneDirty;
            public int observedTick;
            public string contentRoot;
            public string publishedVisualKey;
            public bool spriteEntryFound;
            public int visualDataId;
            public int effectivePic;
            public string sourceSheetPath;
            public float pixelWidth;
            public float pixelHeight;
            public int textureWidth;
            public int textureHeight;
            public bool centralBindingValid;
            public bool legacySpritePresent;
            public string scope;
        }

        static NTSD28Q07Oid434PublishedSpriteProbeEditor()
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
                    Debug.LogError("[Q07 OID434 Sprite Probe] Invalid request, result collision or unsaved Battle Scene.");
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
                scope = "Original saved Battle Scene production formal prewarm SpriteCatalog entry only; no entity draw or pixel proof."
            };
            try
            {
                if (DateTime.UtcNow - new DateTime(request.startedUtcTicks, DateTimeKind.Utc) >
                    TimeSpan.FromSeconds(120))
                    throw new TimeoutException("Formal Battle content prewarm did not become ready.");
                SimulationTickDriver driver = SimulationTickDriver.Instance;
                if (driver?.World == null || driver.CurrentTickIndex < 5 ||
                    !CharacterAnimtorManager.HasInstance || !GameDataManager.HasInstance)
                    return;

                CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
                report.observedTick = driver.CurrentTickIndex;
                report.publishedVisualKey = GameDataManager.Instance.PublishedVisualContentKey;
                report.spriteEntryFound = manager.TryGetSpriteEntry(434, 36,
                    out BattleSpriteEntry entry);
                if (entry != null)
                {
                    report.visualDataId = entry.Key.VisualDataId;
                    report.effectivePic = entry.Key.EffectivePic;
                    report.sourceSheetPath = entry.SourceSheetPath;
                    report.pixelWidth = entry.PixelWidth;
                    report.pixelHeight = entry.PixelHeight;
                    report.textureWidth = entry.SharedTexture != null ? entry.SharedTexture.width : 0;
                    report.textureHeight = entry.SharedTexture != null ? entry.SharedTexture.height : 0;
                    report.centralBindingValid = entry.CentralBinding.IsValid;
                    report.legacySpritePresent = entry.LegacySprite != null;
                }
                string normalizedSheet = report.sourceSheetPath?.Replace('\\', '/');
                if (report.scenePath != BattleScene || report.sceneDirty ||
                    report.contentRoot != FormalRoot ||
                    string.IsNullOrEmpty(report.publishedVisualKey) ||
                    !report.spriteEntryFound || report.visualDataId != 434 ||
                    report.effectivePic != 36 ||
                    string.IsNullOrEmpty(normalizedSheet) ||
                    !normalizedSheet.EndsWith(ExpectedSheet, StringComparison.OrdinalIgnoreCase) ||
                    report.pixelWidth != 48f || report.pixelHeight != 48f ||
                    report.textureWidth <= 0 || report.textureHeight <= 0 ||
                    !report.centralBindingValid)
                {
                    throw new InvalidOperationException("Published OID434/pic36 does not match the formal ras.png entry.");
                }
                report.status = "PASS";
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
