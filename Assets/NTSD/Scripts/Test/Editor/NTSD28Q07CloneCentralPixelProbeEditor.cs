#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q07CloneCentralPixelProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_CloneCentralPixel.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-NARUTO-CLONE-CENTRAL-PIXEL-WITNESS-001";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string FormalFingerprint =
            "FD18D668B9D4EF0FAD4EE3D8056F98754049B3F25FB6927EC562C3F60B008147";
        private const int CaptureWidth = 960;
        private const int TimeoutTicks = 75;
        private static readonly Color32 ClearColor = new Color32(255, 255, 255, 255);
        private static readonly List<LF2Entity> Entities = new List<LF2Entity>(16);
        private static int startTick = -1;
        private static int visibleTick = -1;
        private static string activeRunId;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public string evidenceScope;
            public int visibleTick;
            public int planSimulationTick;
            public int commandStableId;
            public int commandRuntimeSlot;
            public int commandVisualDataId;
            public int commandEffectivePic;
            public int fullNonClearPixels;
            public int cloneBoundsNonClearPixels;
            public int cloneBoundsAreaPixels;
            public string imagePath;
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
            string requestFile = ProjectPath(RequestPath);
            if (!File.Exists(requestFile))
                return;
            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || !request.requested)
            {
                Reset();
                return;
            }
            if (string.IsNullOrEmpty(request.runId) ||
                !System.Linq.Enumerable.All(request.runId,
                    value => char.IsLetterOrDigit(value) || value == '-'))
            {
                Finish(request, false, "Invalid runId.", null);
                return;
            }
            activeRunId = request.runId;
            if (!EditorApplication.isPlaying)
            {
                if (startTick >= 0)
                    Finish(request, false, "Play ended before central pixel capture.", null);
                else if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (driver == null || world == null)
                return;
            if (startTick < 0)
            {
                startTick = driver.CurrentTickIndex;
            }
            if (GameConfig.Instance?.BattleContentRuntimeRoot != FormalRoot)
            {
                Finish(request, false, "Formal content root is not selected.", null);
                return;
            }
            CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
            if (manager?.PublishedLoganContentIdentity == null)
                return;
            if (manager.PublishedLoganContentIdentity.SemanticFingerprint != FormalFingerprint)
            {
                Finish(request, false, "Formal content fingerprint differs.", null);
                return;
            }
            if (driver.CurrentTickIndex > startTick + TimeoutTicks)
            {
                Finish(request, false, "Timed out before clone central pixel capture.", null);
                return;
            }

            Entities.Clear();
            world.GetAllEntities(Entities);
            foreach (LF2Entity entity in Entities)
                if (entity != null && entity.ObjectId == 33 &&
                    entity.Frame?.N == 242 && entity.GetRenderPicIndex() == 1)
                    visibleTick = driver.CurrentTickIndex;
            if (visibleTick < 0)
                return;

            BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            Camera camera = NTSDRenderSpace.WorldCamera;
            if (!plan.IsValid || !ReferenceEquals(plan.World, world) ||
                plan.Owner != BattlePixelFrameOwner.Central || plan.IsStale ||
                plan.Submission == null || plan.CapturedFrame == null ||
                !plan.CapturedFrame.CommandsMaterialized ||
                plan.SimulationTick < visibleTick || camera == null)
                return;
            if (!BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    camera, CameraRenderType.Base, camera.cameraType, true,
                    out BattleCentralSubmission.BattleCentralSubmissionLease lease))
                return;
            BattleRenderCommand cloneCommand = default;
            bool found = false;
            using (lease)
            {
                BattlePresentationFrame frame = plan.CapturedFrame;
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (command.Type != BattleRenderCommandType.Entity ||
                        command.VisualDataId != 33 || command.EffectivePic != 1)
                        continue;
                    cloneCommand = command;
                    found = true;
                    break;
                }
            }
            if (!found)
                return;
            try
            {
                Report report = Capture(camera, cloneCommand, plan.SimulationTick);
                Finish(request,
                    report.cloneBoundsNonClearPixels > 0,
                    report.cloneBoundsNonClearPixels > 0
                        ? "OID33 pic1 central command and projected non-clear camera pixels observed."
                        : "OID33 pic1 command exists but projected camera region is clear.",
                    report);
            }
            catch (Exception exception)
            {
                Finish(request, false, "Camera capture failed: " + exception, null);
            }
        }

        private static Report Capture(Camera camera, BattleRenderCommand command, int planTick)
        {
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            float widthWorld = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                               NTSDRenderSpace.BattleVisualScale;
            float heightWorld = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * widthWorld;
            float bottom = command.Position.y - command.Pivot.y * heightWorld;
            Vector3 lower = camera.WorldToViewportPoint(new Vector3(left, bottom, command.Position.z));
            Vector3 upper = camera.WorldToViewportPoint(new Vector3(
                left + widthWorld, bottom + heightWorld, command.Position.z));
            int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.y, upper.y) * height), 0, height);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.y, upper.y) * height), 0, height);

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
                camera.backgroundColor = Color.white;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, height, TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, height), 0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                int full = 0;
                int region = 0;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < CaptureWidth; x++)
                    {
                        Color32 pixel = pixels[y * CaptureWidth + x];
                        bool nonClear = pixel.r != ClearColor.r || pixel.g != ClearColor.g ||
                                        pixel.b != ClearColor.b || pixel.a != ClearColor.a;
                        if (!nonClear) continue;
                        full++;
                        if (x >= x0 && x < x1 && y >= y0 && y < y1)
                            region++;
                    }
                string imageRelativePath = ResultRoot + "/" + activeRunId + ".png";
                string imagePath = ProjectPath(imageRelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                File.WriteAllBytes(imagePath, readback.EncodeToPNG());
                return new Report
                {
                    evidenceScope = "OID33 central command and projected camera region, not EXE pixel parity",
                    visibleTick = visibleTick,
                    planSimulationTick = planTick,
                    commandStableId = command.StableId,
                    commandRuntimeSlot = command.RuntimeSlot,
                    commandVisualDataId = command.VisualDataId,
                    commandEffectivePic = command.EffectivePic,
                    fullNonClearPixels = full,
                    cloneBoundsNonClearPixels = region,
                    cloneBoundsAreaPixels = (x1 - x0) * (y1 - y0),
                    imagePath = imageRelativePath,
                };
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(camera);
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void Finish(Request request, bool passed, string message, Report report)
        {
            report = report ?? new Report();
            report.status = passed ? "PASS" : "FAIL";
            report.message = message;
            report.evidenceScope = report.evidenceScope ?? "Target Play observation only";
            string output = ProjectPath(ResultRoot + "/" + request.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!File.Exists(output))
                File.WriteAllText(output, JsonUtility.ToJson(report, true));
            request.requested = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
            Reset();
        }

        private static void Reset()
        {
            startTick = -1;
            visibleTick = -1;
            activeRunId = null;
            Entities.Clear();
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
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
        }
    }
}
#endif
