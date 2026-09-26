#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07SasukeNeedleCameraProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_Q07_SasukeNeedleCamera.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-CAMERA-PIXEL-001";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const int CaptureWidth = 960;
        private static readonly List<LF2Entity> Entities = new List<LF2Entity>(32);
        private static readonly HashSet<int> ChildStableIds = new HashSet<int>();
        private static readonly Dictionary<int, int> ChildPics = new Dictionary<int, int>();
        private static DateTime startedUtc;
        private static Request activeRequest;
        private static Report report;
        private static Color32[] baselinePixels;
        private static SimulationTickDriver shutdownDriver;
        private static string shutdownRunId;
        private static bool running;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string error;
            public string scenePath;
            public string contentRoot;
            public int baselineTick = -1;
            public int childTick = -1;
            public int childCount;
            public int commandCount;
            public int matchedChildCommands;
            public string childCommandSummary;
            public string childPicSummary;
            public string sourceSheetPath;
            public int sourceVisualDataId;
            public int sourcePic;
            public int captureWidth;
            public int captureHeight;
            public int roiX;
            public int roiY;
            public int roiWidth;
            public int roiHeight;
            public int roiBaselineNonblackPixels;
            public int roiChangedPixels;
            public int roiChangedNonblackPixels;
            public string beforePngPath;
            public string afterPngPath;
            public bool cameraRestored = true;
        }

        [Serializable]
        private sealed class ShutdownWitness
        {
            public string runId;
            public string status;
            public string error;
            public string lifecycleState;
            public string reportStatus;
            public string completedStage;
            public int remainingWorldObjects = -1;
            public int remainingRuntimeSlots = -1;
            public int remainingPoolBorrowers = -1;
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (!running)
            {
                string path = ProjectPath(RequestPath);
                if (!File.Exists(path))
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
                if (request == null || !request.requested || !EditorApplication.isPlaying)
                    return;
                if (string.IsNullOrEmpty(request.runId) ||
                    !System.Linq.Enumerable.All(request.runId,
                        value => char.IsLetterOrDigit(value) || value == '-'))
                    return;
                string output = ProjectPath(ResultRoot + "/" + request.runId + ".json");
                string shutdownOutput = ProjectPath(ResultRoot + "/" +
                    request.runId + "-shutdown.json");
                if (File.Exists(output) || File.Exists(shutdownOutput))
                    return;
                request.requested = false;
                File.WriteAllText(path, JsonUtility.ToJson(request));
                activeRequest = request;
                report = new Report { runId = request.runId };
                baselinePixels = null;
                shutdownRunId = request.runId;
                shutdownDriver = null;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                startedUtc = DateTime.UtcNow;
                running = true;
            }

            try
            {
                Observe();
            }
            catch (Exception exception)
            {
                Finish("FAIL", exception.GetType().Name + ": " + exception.Message);
            }
        }

        private static void Observe()
        {
            if (!EditorApplication.isPlaying)
            {
                Finish("FAIL", "Play Mode ended before both camera frames were captured.");
                return;
            }
            if ((DateTime.UtcNow - startedUtc).TotalSeconds > 180)
            {
                Finish("FAIL", "Timed out waiting for natural OID440 birth and camera publication.");
                return;
            }
            Scene battle = SceneManager.GetSceneByName("NTSD_Battle");
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (!battle.IsValid() || !battle.isLoaded || world == null)
                return;
            shutdownDriver = driver;
            report.scenePath = battle.path;
            report.contentRoot = CharacterAnimtorManager.ConfiguredContentRoot;
            Require(report.scenePath == "Assets/NTSD/Scene/NTSD_Battle.unity" &&
                report.contentRoot == FormalRoot,
                "Original Battle Scene and formal content root are required.");

            Entities.Clear();
            world.GetAllEntities(Entities);
            int sasukeCount = 0;
            int childCount = 0;
            ChildStableIds.Clear();
            ChildPics.Clear();
            report.childPicSummary = string.Empty;
            BattleSpriteEntry source = null;
            foreach (LF2Entity entity in Entities)
            {
                if (entity == null || entity.Runtime == null)
                    continue;
                if (entity.ObjectId == 11)
                    sasukeCount++;
                if (entity.ObjectId != 440)
                    continue;
                childCount++;
                ChildStableIds.Add(entity.Runtime.StableId);
                if (entity.TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry) &&
                    entry != null && entry.Key.VisualDataId == 440 &&
                    entry.Key.EffectivePic == entity.GetRenderPicIndex() &&
                    entry.CentralBinding.IsValid &&
                    entry.SourceSheetPath.Replace('\\', '/').EndsWith(
                        "c/sasu/a/chi.png", StringComparison.OrdinalIgnoreCase))
                {
                    ChildPics.Add(entity.Runtime.StableId, entry.Key.EffectivePic);
                    report.childPicSummary += entity.Runtime.StableId + ":" +
                        entry.Key.EffectivePic + ";";
                    source = entry;
                }
            }
            if (sasukeCount == 0)
                return;

            int tick = driver.CurrentTickIndex;
            world.RenderDispatchAll(tick, true);
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
            if (!plan.IsValid || plan.IsStale ||
                plan.Owner != BattlePixelFrameOwner.Central ||
                plan.CapturedFrame?.CommandsMaterialized != true ||
                plan.Submission == null || plan.SimulationTick != tick)
                return;
            Camera camera = NTSDRenderSpace.WorldCamera;
            if (camera == null || !camera.enabled || !camera.gameObject.activeInHierarchy ||
                !BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    camera, CameraRenderType.Base, camera.cameraType, true,
                    out BattleCentralSubmission.BattleCentralSubmissionLease lease))
                return;
            using (lease)
            {
            }

            if (baselinePixels == null && childCount == 0)
            {
                baselinePixels = Capture(camera, "before");
                report.baselineTick = tick;
                return;
            }
            if (childCount < 4 || baselinePixels == null)
            {
                FinishIfChildWindowEnded(tick);
                return;
            }
            if (ChildPics.Count != 4)
            {
                FinishIfChildWindowEnded(tick);
                return;
            }
            Require(childCount == 4 && ChildStableIds.Count == 4 && source != null,
                "The natural Sasuke child set or formal chi.png binding is incomplete.");

            BattlePresentationFrame frame = plan.CapturedFrame;
            report.commandCount = frame.CommandCount;
            report.childCount = childCount;
            report.childTick = tick;
            report.sourceSheetPath = source.SourceSheetPath;
            report.sourceVisualDataId = source.Key.VisualDataId;
            report.sourcePic = source.Key.EffectivePic;
            report.matchedChildCommands = 0;
            report.childCommandSummary = string.Empty;
            int x0 = int.MaxValue;
            int y0 = int.MaxValue;
            int x1 = int.MinValue;
            int y1 = int.MinValue;
            int height = baselinePixels.Length / CaptureWidth;
            var projectedCenters = new HashSet<Vector2Int>();
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (!ChildPics.TryGetValue(command.StableId, out int childPic))
                    continue;
                if (command.Type != BattleRenderCommandType.Entity ||
                    command.VisualDataId != 440 || command.EffectivePic != childPic ||
                    !ChildStableIds.Contains(command.StableId))
                    continue;
                report.matchedChildCommands++;
                report.childCommandSummary += command.StableId + ":" +
                    command.RuntimeSlot + ":" + childPic + ":" +
                    command.Position + ";";
                float widthWorld = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                                   NTSDRenderSpace.BattleVisualScale;
                float heightWorld = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                                    NTSDRenderSpace.BattleVisualScale;
                float left = command.Position.x - command.Pivot.x * widthWorld;
                float bottom = command.Position.y - command.Pivot.y * heightWorld;
                Vector3 lower = camera.WorldToViewportPoint(
                    new Vector3(left, bottom, command.Position.z));
                Vector3 upper = camera.WorldToViewportPoint(new Vector3(
                    left + widthWorld, bottom + heightWorld, command.Position.z));
                int leftPixel = Mathf.Clamp(Mathf.FloorToInt(
                    Mathf.Min(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
                int rightPixel = Mathf.Clamp(Mathf.CeilToInt(
                    Mathf.Max(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
                int bottomPixel = Mathf.Clamp(Mathf.FloorToInt(
                    Mathf.Min(lower.y, upper.y) * height), 0, height);
                int topPixel = Mathf.Clamp(Mathf.CeilToInt(
                    Mathf.Max(lower.y, upper.y) * height), 0, height);
                x0 = Math.Min(x0, leftPixel);
                x1 = Math.Max(x1, rightPixel);
                y0 = Math.Min(y0, bottomPixel);
                y1 = Math.Max(y1, topPixel);
                projectedCenters.Add(new Vector2Int(
                    (leftPixel + rightPixel) / 2,
                    (bottomPixel + topPixel) / 2));
            }
            Require(report.matchedChildCommands == 4 && x1 > x0 && y1 > y0,
                "Four natural OID440 commands did not project into the camera.");
            if (projectedCenters.Count != 4)
            {
                FinishIfChildWindowEnded(tick);
                return;
            }
            int baselineNonblack = 0;
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                {
                    Color32 before = baselinePixels[y * CaptureWidth + x];
                    if (before.r != 0 || before.g != 0 || before.b != 0)
                        baselineNonblack++;
                }
            if (baselineNonblack != 0)
            {
                FinishIfChildWindowEnded(tick);
                return;
            }
            report.roiX = x0;
            report.roiY = y0;
            report.roiWidth = x1 - x0;
            report.roiHeight = y1 - y0;
            report.roiBaselineNonblackPixels = baselineNonblack;

            Color32[] after = Capture(camera, "after");
            Require(after.Length == baselinePixels.Length,
                "Camera dimensions changed between the two captures.");
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                {
                    int pixelIndex = y * CaptureWidth + x;
                    Color32 before = baselinePixels[pixelIndex];
                    Color32 current = after[pixelIndex];
                    bool changed = before.r != current.r || before.g != current.g ||
                                   before.b != current.b;
                    if (changed)
                    {
                        report.roiChangedPixels++;
                        if (current.r != 0 || current.g != 0 || current.b != 0)
                            report.roiChangedNonblackPixels++;
                    }
                }
            Finish(report.cameraRestored && report.roiChangedNonblackPixels > 0
                    ? "PASS_SCOPED_UNITY_CAMERA" : "PIXEL_OWNERSHIP_UNPROVEN",
                "Natural OID440 commands and central-only camera A/B captured; " +
                "other changing sprites may overlap the union ROI.");
        }

        private static Color32[] Capture(Camera camera, string phase)
        {
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            Require(report.captureHeight == 0 || report.captureHeight == height,
                "Camera aspect changed between captures.");
            report.captureWidth = CaptureWidth;
            report.captureHeight = height;
            var target = new RenderTexture(CaptureWidth, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            RenderTexture previousActive = RenderTexture.active;
            var saved = new CameraState(camera);
            Texture2D readback = null;
            try
            {
                target.Create();
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, height,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, height),
                    0, 0, false);
                readback.Apply(false, false);
                string relative = ResultRoot + "/" + activeRequest.runId +
                                  "-" + phase + ".png";
                string output = ProjectPath(relative);
                Require(!File.Exists(output), "Refusing to overwrite a camera PNG.");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, readback.EncodeToPNG());
                if (phase == "before")
                    report.beforePngPath = relative;
                else
                    report.afterPngPath = relative;
                return readback.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(camera);
                report.cameraRestored &= saved.Matches(camera) &&
                                         RenderTexture.active == previousActive;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void Finish(string status, string error)
        {
            report.status = status;
            report.error = error;
            string output = ProjectPath(ResultRoot + "/" + activeRequest.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!File.Exists(output))
                File.WriteAllText(output, JsonUtility.ToJson(report, true));
            running = false;
            baselinePixels = null;
            activeRequest = null;
        }

        private static void FinishIfChildWindowEnded(int tick)
        {
            if (tick >= 26)
                Finish("PIXEL_OWNERSHIP_UNPROVEN",
                    "No frame with four separate OID440 screen centers and a " +
                    "baseline-black child ROI before the natural child window ended.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode ||
                string.IsNullOrEmpty(shutdownRunId))
                return;

            var witness = new ShutdownWitness { runId = shutdownRunId };
            try
            {
                if (shutdownDriver == null)
                {
                    witness.status = "DRIVER_NOT_OBSERVED";
                }
                else
                {
                    witness.lifecycleState = shutdownDriver.LifecycleState.ToString();
                    if (shutdownDriver.LifecycleState !=
                        BattleRuntimeLifecycleState.Stopped)
                    {
                        witness.status = "NOT_STOPPED_AT_OBSERVER_CALLBACK";
                    }
                    else
                    {
                        BattleRuntimeShutdownReport shutdown =
                            shutdownDriver.ShutdownBattleRuntime();
                        witness.reportStatus = shutdown.Status.ToString();
                        witness.completedStage = shutdown.CompletedStage.ToString();
                        witness.remainingWorldObjects = shutdown.RemainingWorldObjects;
                        witness.remainingRuntimeSlots = shutdown.RemainingRuntimeSlots;
                        witness.remainingPoolBorrowers = shutdown.RemainingPoolBorrowers;
                        witness.status = shutdown.IsComplete &&
                            shutdown.CompletedStage ==
                                BattleRuntimeShutdownStage.RuntimeMapCleared &&
                            witness.remainingWorldObjects == 0 &&
                            witness.remainingRuntimeSlots == 0 &&
                            witness.remainingPoolBorrowers == 0
                                ? "PASS" : "FAIL";
                    }
                }
            }
            catch (Exception exception)
            {
                witness.status = "FAIL";
                witness.error = exception.GetType().Name + ": " +
                    exception.Message;
            }
            finally
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                shutdownDriver = null;
                shutdownRunId = null;
            }

            string path = ProjectPath(ResultRoot + "/" + witness.runId +
                                      "-shutdown.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew,
                       FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(witness, true));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static string ProjectPath(string relative)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));
        }

        private readonly struct CameraState
        {
            private readonly int cullingMask;
            private readonly CameraClearFlags clearFlags;
            private readonly Color backgroundColor;
            private readonly bool allowHdr;
            private readonly bool allowMsaa;
            private readonly RenderTexture targetTexture;

            public CameraState(Camera camera)
            {
                cullingMask = camera.cullingMask;
                clearFlags = camera.clearFlags;
                backgroundColor = camera.backgroundColor;
                allowHdr = camera.allowHDR;
                allowMsaa = camera.allowMSAA;
                targetTexture = camera.targetTexture;
            }

            public void Restore(Camera camera)
            {
                camera.targetTexture = targetTexture;
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = backgroundColor;
                camera.allowHDR = allowHdr;
                camera.allowMSAA = allowMsaa;
            }

            public bool Matches(Camera camera)
            {
                return camera.targetTexture == targetTexture &&
                       camera.cullingMask == cullingMask &&
                       camera.clearFlags == clearFlags &&
                       camera.backgroundColor == backgroundColor &&
                       camera.allowHDR == allowHdr &&
                       camera.allowMSAA == allowMsaa;
            }
        }
    }
}
#endif
