#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCentralDiagnosticWindow : EditorWindow
    {
        private const string DefaultOutputPath = "Temp/NTSD_BattleCentralDiagnostic.json";

        private int runtimeSlot;
        private BattleRenderCommandType commandType = BattleRenderCommandType.Entity;
        private string status = "Enter a runtime slot while a battle world is active.";
        private MessageType statusMessageType = MessageType.Info;
        private string lastJson = string.Empty;
        private Vector2 scrollPosition;

        [MenuItem("NTSD/Battle Rendering/Central Entity Diagnostic")]
        public static void Open()
        {
            GetWindow<BattleCentralDiagnosticWindow>("Central Render Diagnostic");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Current Battle Render Query", EditorStyles.boldLabel);
            runtimeSlot = EditorGUILayout.IntField("Runtime Slot", runtimeSlot);
            commandType = (BattleRenderCommandType)EditorGUILayout.EnumPopup("Command Type", commandType);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Query"))
                    QueryCurrentWorld();
                if (GUILayout.Button("Export JSON"))
                    ExportCurrentWorld();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(status, statusMessageType);
            EditorGUILayout.LabelField("Deterministic JSON", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(lastJson, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void QueryCurrentWorld()
        {
            try
            {
                lastJson = BattleCentralDiagnosticExporter.CaptureCurrentWorldJson(runtimeSlot, commandType);
                status = BuildSuccessStatus(lastJson);
                statusMessageType = MessageType.Info;
            }
            catch (Exception ex)
            {
                lastJson = string.Empty;
                status = ex.Message;
                statusMessageType = MessageType.Error;
                Debug.LogError($"[BattleCentralDiagnostic] Query failed: {ex.Message}");
            }
        }

        private void ExportCurrentWorld()
        {
            try
            {
                string json = BattleCentralDiagnosticExporter.CaptureCurrentWorldJson(runtimeSlot, commandType);
                string defaultAbsolutePath = BattleCentralDiagnosticExporter.ProjectPath(DefaultOutputPath);
                string selectedPath = EditorUtility.SaveFilePanel(
                    "Export Central Render Diagnostic",
                    Path.GetDirectoryName(defaultAbsolutePath),
                    Path.GetFileName(defaultAbsolutePath),
                    "json");
                if (string.IsNullOrEmpty(selectedPath))
                    return;

                BattleCentralDiagnosticExporter.WriteJson(selectedPath, json);
                lastJson = json;
                status = $"Exported {selectedPath}";
                statusMessageType = MessageType.Info;
                Debug.Log($"[BattleCentralDiagnostic] Exported: {selectedPath}");
            }
            catch (Exception ex)
            {
                status = ex.Message;
                statusMessageType = MessageType.Error;
                Debug.LogError($"[BattleCentralDiagnostic] Export failed: {ex.Message}");
            }
        }

        private static string BuildSuccessStatus(string json)
        {
            return json.Contains("\"renderingReport\":null")
                ? "Entity diagnostic captured. No rendering summary is available yet."
                : "Entity diagnostic and rendering summary captured.";
        }
    }

    internal static class BattleCentralDiagnosticExporter
    {
        internal const string Schema = "ntsd-central-render-diagnostic-v1";

        internal static string CaptureCurrentWorldJson(
            int runtimeSlot,
            BattleRenderCommandType commandType)
        {
            SimulationWorld world = SimulationTickDriver.Instance?.World;
            if (world == null)
            {
                throw new InvalidOperationException(
                    "No current SimulationWorld exists. Enter Play Mode and start a battle before querying.");
            }

            BattleCentralEntityDiagnostic entity =
                BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(world, runtimeSlot, commandType);
            if (entity.Reason == BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(runtimeSlot),
                    runtimeSlot,
                    $"Runtime slot {runtimeSlot} is not currently claimed in the active SimulationWorld.");
            }

            BattleRenderingDiagnosticReport report = BattleCentralRenderSystem.CaptureDiagnosticReport();
            return Serialize(runtimeSlot, commandType, entity, report);
        }

        internal static string Serialize(
            int requestedRuntimeSlot,
            BattleRenderCommandType requestedCommandType,
            in BattleCentralEntityDiagnostic entity,
            BattleRenderingDiagnosticReport report)
        {
            string entityJson = BattleCanonicalJson.Serialize(BuildEntityProjection(entity));
            string reportJson = report?.ToJson() ?? "null";
            var builder = new StringBuilder(entityJson.Length + reportJson.Length + 192);
            builder.Append("{\"schema\":\"");
            builder.Append(Schema);
            builder.Append("\",\"requestedRuntimeSlot\":");
            builder.Append(requestedRuntimeSlot.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"requestedCommandType\":");
            builder.Append(BattleCanonicalJson.Serialize(requestedCommandType.ToString()));
            builder.Append(",\"entityDiagnostic\":");
            builder.Append(entityJson);
            builder.Append(",\"renderingReportAvailable\":");
            builder.Append(report != null ? "true" : "false");
            builder.Append(",\"renderingReport\":");
            builder.Append(reportJson);
            builder.Append('}');
            return builder.ToString();
        }

        internal static void WriteJson(string outputPath, string json)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("An output path is required.", nameof(outputPath));
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            string fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ProjectPath("Temp"));
            File.WriteAllText(fullPath, json, new UTF8Encoding(false));
        }

        internal static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static Dictionary<string, object> BuildEntityProjection(
            in BattleCentralEntityDiagnostic entity)
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["atlasPageIndex"] = entity.AtlasPageIndex,
                ["atlasSlice"] = entity.AtlasSlice,
                ["bindingMode"] = entity.BindingMode.ToString(),
                ["chunkIndex"] = entity.ChunkIndex,
                ["color"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["a"] = entity.Color.a,
                    ["b"] = entity.Color.b,
                    ["g"] = entity.Color.g,
                    ["r"] = entity.Color.r,
                },
                ["commandIndex"] = entity.CommandIndex,
                ["commandType"] = entity.CommandType.ToString(),
                ["currentDatObjectId"] = entity.CurrentDatObjectId,
                ["effectivePic"] = entity.EffectivePic,
                ["frameId"] = entity.FrameId,
                ["entityVisible"] = entity.EntityVisible,
                ["flipX"] = entity.FlipX,
                ["flipY"] = entity.FlipY,
                ["handle"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["generation"] = entity.Handle.Generation,
                    ["isValid"] = entity.Handle.IsValid,
                    ["slot"] = entity.Handle.Slot,
                },
                ["hasCommand"] = entity.HasCommand,
                ["hasLogicalResourceKey"] = entity.HasLogicalResourceKey,
                ["hasResolvedResource"] = entity.HasResolvedResource,
                ["hasSnapshot"] = entity.HasSnapshot,
                ["localSequence"] = entity.LocalSequence,
                ["logicalResourceKey"] = BuildResourceKeyProjection(entity),
                ["normalizedUv"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["height"] = entity.NormalizedUv.height,
                    ["width"] = entity.NormalizedUv.width,
                    ["x"] = entity.NormalizedUv.x,
                    ["y"] = entity.NormalizedUv.y,
                },
                ["objectId"] = entity.ObjectId,
                ["pivot"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["x"] = entity.Pivot.x,
                    ["y"] = entity.Pivot.y,
                },
                ["position"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["x"] = entity.Position.x,
                    ["y"] = entity.Position.y,
                    ["z"] = entity.Position.z,
                },
                ["presentationBaseOrder"] = entity.PresentationBaseOrder,
                ["reason"] = entity.Reason.ToString(),
                ["segmentIndex"] = entity.SegmentIndex,
                ["shadowVisible"] = entity.ShadowVisible,
                ["sortOrder"] = entity.SortOrder,
                ["stableId"] = entity.StableId,
                ["submitted"] = entity.Submitted,
            };
        }

        private static object BuildResourceKeyProjection(in BattleCentralEntityDiagnostic entity)
        {
            if (!entity.HasLogicalResourceKey)
                return null;

            BattleVisualResourceKey key = entity.LogicalResourceKey;
            var result = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["kind"] = key.Kind.ToString(),
            };
            if (key.IsEntitySprite)
            {
                result["effectivePic"] = key.EntitySpriteKey.EffectivePic;
                result["visualDataId"] = key.EntitySpriteKey.VisualDataId;
            }
            else if (key.IsCommonSpark)
            {
                result["pic"] = key.CommonSparkPic;
            }
            else if (key.IsCommonWordGlyph)
            {
                result["charCode"] = key.CommonWordCharCode;
                result["sheetIndex"] = key.CommonWordSheetIndex;
            }
            return result;
        }
    }

    [InitializeOnLoad]
    internal static class BattleCentralDiagnosticRequestProcessor
    {
        private const string RequestFile = "Temp/NTSD_BattleCentralDiagnostic.request.json";
        private const string ResultFile = "Temp/NTSD_BattleCentralDiagnostic.result";
        private const string DefaultOutputFile = "Temp/NTSD_BattleCentralDiagnostic.json";

        private static bool requestInProgress;
        private static bool staleResultDeleteWarningLogged;
        private static readonly string RequestAbsolutePath =
            BattleCentralDiagnosticExporter.ProjectPath(RequestFile);
        private static readonly string ResultAbsolutePath =
            BattleCentralDiagnosticExporter.ProjectPath(ResultFile);

        static BattleCentralDiagnosticRequestProcessor()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (requestInProgress)
                return;

            if (!File.Exists(RequestAbsolutePath))
            {
                staleResultDeleteWarningLogged = false;
                return;
            }

            try
            {
                if (File.Exists(ResultAbsolutePath))
                    File.Delete(ResultAbsolutePath);
                staleResultDeleteWarningLogged = false;
            }
            catch (Exception ex)
            {
                if (!staleResultDeleteWarningLogged)
                {
                    Debug.LogWarning(
                        $"[BattleCentralDiagnostic] Failed to delete stale result: {ex.Message}");
                    staleResultDeleteWarningLogged = true;
                }
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            requestInProgress = true;
            try
            {
                ProcessRequest(RequestAbsolutePath);
            }
            finally
            {
                TryDeleteRequest(RequestAbsolutePath);
                requestInProgress = false;
            }
        }

        private static void ProcessRequest(string requestPath)
        {
            try
            {
                string requestJson = File.ReadAllText(requestPath, Encoding.UTF8);
                DiagnosticRequest request = JsonUtility.FromJson<DiagnosticRequest>(requestJson);
                if (request == null)
                    throw new InvalidOperationException("The central diagnostic request JSON is empty or invalid.");

                BattleRenderCommandType commandType = ParseCommandType(request.commandType);
                string outputPath = string.IsNullOrWhiteSpace(request.outputPath)
                    ? BattleCentralDiagnosticExporter.ProjectPath(DefaultOutputFile)
                    : BattleCentralDiagnosticExporter.ProjectPath(request.outputPath);
                string json = BattleCentralDiagnosticExporter.CaptureCurrentWorldJson(
                    request.runtimeSlot,
                    commandType);
                BattleCentralDiagnosticExporter.WriteJson(outputPath, json);
                WriteResult(ResultAbsolutePath, $"PASS{Environment.NewLine}{outputPath}");
                Debug.Log($"[BattleCentralDiagnostic] Request exported: {outputPath}");
            }
            catch (Exception ex)
            {
                WriteResult(ResultAbsolutePath, $"FAIL{Environment.NewLine}{ex}");
                Debug.LogError($"[BattleCentralDiagnostic] Request failed: {ex}");
            }
        }

        private static BattleRenderCommandType ParseCommandType(string value)
        {
            string candidate = string.IsNullOrWhiteSpace(value)
                ? nameof(BattleRenderCommandType.Entity)
                : value;
            if (!Enum.TryParse(candidate, true, out BattleRenderCommandType commandType) ||
                !Enum.IsDefined(typeof(BattleRenderCommandType), commandType))
            {
                throw new ArgumentException(
                    $"Unknown BattleRenderCommandType '{candidate}'.",
                    nameof(value));
            }
            return commandType;
        }

        private static void WriteResult(string resultPath, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath) ??
                                      BattleCentralDiagnosticExporter.ProjectPath("Temp"));
            File.WriteAllText(resultPath, content, new UTF8Encoding(false));
        }

        private static void TryDeleteRequest(string requestPath)
        {
            try
            {
                File.Delete(requestPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BattleCentralDiagnostic] Failed to delete request: {ex.Message}");
            }
        }

        [Serializable]
        private sealed class DiagnosticRequest
        {
            public int runtimeSlot = -1;
            public string commandType = nameof(BattleRenderCommandType.Entity);
            public string outputPath = DefaultOutputFile;
        }
    }
}
#endif
