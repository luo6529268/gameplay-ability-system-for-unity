#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q09FormalPlatformDriverPlayProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q09_FormalPlatformDriver.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-FORMAL-PLATFORM-DRIVER-PLAY-001";
        private const string CameraResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-FORMAL-PLATFORM-SHADOW-CAMERA-WITNESS-001";
        private const int CaptureWidth = 1920;
        private static readonly Color32 Clear = new Color32(255, 255, 255, 255);

        private static int stableTick = -1;
        private static int stableUpdates;
        private static bool pauseCaptured;
        private static bool wasPaused;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public long startedUtcTicks;
            public bool captureCamera;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string runId;
            public string scope;
            public string scenePath;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public int startTick;
            public int endTick;
            public int sourceSlot;
            public int targetSlot;
            public int sourceActionBefore;
            public int sourceSnapshotBefore;
            public int sourceActionAfter;
            public int sourceSnapshotAfter;
            public int targetActionAfter;
            public int sourceXBefore;
            public int sourceYBefore;
            public int sourceZBefore;
            public int targetXBefore;
            public int targetYBefore;
            public int targetZBefore;
            public int targetXAfter;
            public int targetYAfter;
            public int targetZAfter;
            public int targetCollisionReferenceAfter;
            public int targetPlatformSlotAfter;
            public int targetShadowOffsetAfter;
            public bool centralPlanValid;
            public int centralPlanTick;
            public int targetShadowCommands;
            public int shadowCommandZ;
            public Vector3 shadowCommandPosition;
            public Vector3 expectedShadowPosition;
            public bool cameraRequested;
            public bool cameraStateRestored;
            public string cameraPixelStatus;
            public int captureWidth;
            public int captureHeight;
            public string baselineImage;
            public string postTickImage;
            public int shadowBoundsX;
            public int shadowBoundsY;
            public int shadowBoundsWidth;
            public int shadowBoundsHeight;
            public int exclusiveWhiteBaselinePixels;
            public int exclusiveChangedPixels;
            public int sampleX;
            public int sampleY;
            public string samplePostRgb;
            public int objectsBefore;
            public int objectsAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int borrowersBefore;
            public int borrowersAfter;
            public bool fixtureUnregistered;
        }

        static NTSD28Q09FormalPlatformDriverPlayProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            string path = ProjectPath(RequestPath);
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(path))
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

            if (request.requested && !request.running)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (!ValidRunId(request.runId) || scene.path != ScenePath ||
                    scene.isDirty || File.Exists(ResultPath(request.runId,
                        request.captureCamera)))
                {
                    Finish(request, new Report
                    {
                        status = "FAIL",
                        error = "Invalid run ID, output collision or unsaved original Battle Scene.",
                        scenePath = scene.path,
                    });
                    return;
                }
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(path, JsonUtility.ToJson(request));
                EditorApplication.EnterPlaymode();
                return;
            }

            if (!request.running || !EditorApplication.isPlaying)
                return;

            var report = new Report
            {
                runId = request.runId,
                scenePath = SceneManager.GetActiveScene().path,
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
                cameraRequested = request.captureCamera,
                cameraPixelStatus = request.captureCamera ? "PENDING" : "NOT_REQUESTED",
                scope = request.captureCamera
                    ? "Controlled formal OID56 frame130 + OID2, one complete production Driver tick, CentralOnly Shadow command and conservative original-camera pixel attribution; natural input, Legacy Play and root EXE graphics are not covered."
                    : "Controlled formal OID56 frame130 + OID2, one complete production Driver tick and CentralOnly Shadow command; natural input, camera pixels and root EXE graphics are not covered.",
            };
            try
            {
                Require(DateTime.UtcNow - new DateTime(request.startedUtcTicks,
                    DateTimeKind.Utc) < TimeSpan.FromMinutes(3),
                    "Formal Battle Scene Play timed out.");
                SimulationTickDriver driver = SimulationTickDriver.Instance;
                SimulationWorld world = driver?.World;
                if (world == null || driver.CurrentTickIndex < 5 ||
                    !world.IsBattleSnapshotBoundaryReady)
                    return;
                Require(SceneManager.GetActiveScene().path == ScenePath,
                    "Battle Scene changed during Play.");
                Require(report.contentRoot == FormalRoot,
                    "Formal content root is not selected.");
                Require(driver.PresentationBackendMode == BattlePresentationBackendMode.CentralOnly,
                    "CentralOnly backend is not selected.");
                Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                    "Dedicated worker failed before the controlled tick.");
                if (!pauseCaptured)
                {
                    wasPaused = driver.IsPaused;
                    driver.SetPaused(true);
                    pauseCaptured = true;
                    return;
                }
                if (!driver.IsPaused ||
                    driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                    return;
                if (stableTick != driver.CurrentTickIndex)
                {
                    stableTick = driver.CurrentTickIndex;
                    stableUpdates = 0;
                    return;
                }
                if (++stableUpdates < 4)
                    return;
                Run(request, driver, world, report);
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
                Finish(request, report);
            }
        }

        private static void Run(Request request, SimulationTickDriver driver,
            SimulationWorld world,
            Report report)
        {
            LF2Character source = null;
            LF2Character target = null;
            Camera camera = null;
            Color32[] baselinePixels = null;
            LF2ObjectPool pool = LF2ObjectPool.Instance;
            report.sceneHashBefore = HashFile(ProjectPath(ScenePath));
            report.objectsBefore = world.ObjectCount;
            report.slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.borrowersBefore = pool.ActiveObjectCountForAcceptance;
            try
            {
                var sourceData = world.RuntimeCharacterConfigs.Resolve(56);
                var targetData = world.RuntimeCharacterConfigs.Resolve(2);
                Require(sourceData?.characterData != null &&
                    targetData?.characterData != null,
                    "Formal OID56/OID2 runtime definitions are not available.");
                int sourceSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                int targetSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    sourceSlot + 1, 1000);
                Require(sourceSlot >= 50 && targetSlot > sourceSlot,
                    "Two free runtime slots are required.");
                if (request.captureCamera)
                {
                    camera = NTSDRenderSpace.WorldCamera;
                    Require(camera != null && camera.isActiveAndEnabled,
                        "Original Battle Scene world camera is unavailable.");
                    BattlePixelFramePlan baselinePlan =
                        BattleCentralRenderSystem.PrepareFrame(world);
                    Require(baselinePlan.IsValid && !baselinePlan.IsStale,
                        "Pre-fixture central baseline plan is unavailable.");
                    baselinePixels = CapturePixels(camera, request.runId,
                        "baseline", report);
                }
                report.sourceSlot = sourceSlot;
                report.targetSlot = targetSlot;
                int z = world.Runtime.Stage.ZMin + 50;
                source = Create(world, sourceData, 56, sourceSlot, 1,
                    200, 0, z, 130);
                target = Create(world, targetData, 2, targetSlot, 2,
                    205, -10, z, 0);
                source.Runtime.NativePreviousY104 = 0;
                target.Runtime.NativePreviousY104 = -10;
                report.sourceActionBefore = source.Frame.N;
                report.sourceSnapshotBefore = source.Runtime.PrevFrame2;
                report.sourceXBefore = source.Runtime.XInt;
                report.sourceYBefore = source.Runtime.YInt;
                report.sourceZBefore = source.Runtime.ZInt;
                report.targetXBefore = target.Runtime.XInt;
                report.targetYBefore = target.Runtime.YInt;
                report.targetZBefore = target.Runtime.ZInt;
                Require(source.Frame.D?.itrs != null &&
                    source.Frame.D.itrs.Exists(itr => itr.kind == 50),
                    "Selected formal current frame lacks kind50 ITR.");
                Require(source.Frame.N == 130 && source.Runtime.PrevFrame2 == 130,
                    "Formal source current/snapshot action was not fixed at frame130.");

                report.startTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true,
                    buildPresentation: true), "Production Driver rejected the tick.");
                report.endTick = driver.CurrentTickIndex;
                report.sourceActionAfter = source.Frame.N;
                report.sourceSnapshotAfter = source.Runtime.PrevFrame2;
                report.targetActionAfter = target.Frame.N;
                report.targetXAfter = target.Runtime.XInt;
                report.targetYAfter = target.Runtime.YInt;
                report.targetZAfter = target.Runtime.ZInt;
                report.targetCollisionReferenceAfter =
                    target.Runtime.CollisionYReference;
                report.targetPlatformSlotAfter =
                    target.Runtime.PlatformSourceSlotF4;
                report.targetShadowOffsetAfter =
                    target.Runtime.RenderShadowOffset10C;
                Require(report.endTick == report.startTick,
                    "Driver did not complete exactly one tick.");

                BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
                report.centralPlanValid = plan.IsValid && !plan.IsStale &&
                    plan.CapturedFrame?.CommandsMaterialized == true;
                report.centralPlanTick = plan.SimulationTick;
                BattlePresentationFrame frame = null;
                BattleRenderCommand shadowCommand = default;
                int shadowIndex = -1;
                if (report.centralPlanValid)
                {
                    frame = plan.CapturedFrame;
                    for (int index = 0; index < frame.CommandCount; index++)
                    {
                        BattleRenderCommand command = frame.GetCommand(index);
                        if (command.Type != BattleRenderCommandType.Shadow ||
                            command.RuntimeSlot != target.Runtime.SlotIndex)
                            continue;
                        report.targetShadowCommands++;
                        report.shadowCommandZ = command.ZInt;
                        report.shadowCommandPosition = command.Position;
                        shadowCommand = command;
                        shadowIndex = index;
                    }
                }
                report.expectedShadowPosition = NTSDRenderSpace
                    .CaptureViewportTransform().ScreenPixelToWorld(0,
                        unchecked(target.Runtime.ZInt +
                            target.Runtime.RenderShadowOffset10C), 0f);
                Require(report.targetPlatformSlotAfter == source.Runtime.SlotIndex &&
                    report.targetShadowOffsetAfter != 0,
                    "Formal operation30 did not arm the platform shadow carrier.");
                Require(report.centralPlanValid &&
                    report.centralPlanTick == report.endTick &&
                    report.targetShadowCommands == 1 &&
                    report.shadowCommandZ == target.Runtime.ZInt &&
                    Mathf.Abs(report.shadowCommandPosition.y -
                        report.expectedShadowPosition.y) < 0.0001f,
                    "Central Shadow command did not consume the nonzero carrier.");
                if (request.captureCamera)
                {
                    Color32[] postPixels = CapturePixels(camera, request.runId,
                        "post-tick", report);
                    CompareShadowPixels(camera, frame, shadowIndex,
                        shadowCommand, baselinePixels, postPixels, report);
                }
                report.status = !request.captureCamera ||
                    report.cameraPixelStatus == "PASS_EXCLUSIVE_SHADOW"
                    ? "PASS"
                    : "PARTIAL_PIXEL_OWNERSHIP_UNPROVEN";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }
            finally
            {
                if (target?.RegisteredWorldForSimulation == world)
                    world.Unregister(target);
                if (source?.RegisteredWorldForSimulation == world)
                    world.Unregister(source);
                report.fixtureUnregistered =
                    (target == null || target.RegisteredWorldForSimulation == null) &&
                    (source == null || source.RegisteredWorldForSimulation == null);
                report.objectsAfter = world.ObjectCount;
                report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                report.borrowersAfter = pool.ActiveObjectCountForAcceptance;
                report.sceneHashAfter = HashFile(ProjectPath(ScenePath));
                if (!report.fixtureUnregistered ||
                    report.objectsAfter != report.objectsBefore ||
                    report.slotsAfter != report.slotsBefore ||
                    report.borrowersAfter != report.borrowersBefore ||
                    report.sceneHashAfter != report.sceneHashBefore)
                {
                    report.status = "FAIL";
                    report.error += " Cleanup count or Scene SHA mismatch.";
                }
                driver.SetPaused(wasPaused);
                Finish(new Request
                {
                    runId = report.runId,
                    running = true,
                    captureCamera = request.captureCamera,
                }, report);
            }
        }

        private static Color32[] CapturePixels(Camera camera, string runId,
            string name, Report report)
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
            var saved = new CameraState(camera);
            RenderTexture previousActive = RenderTexture.active;
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
                readback = new Texture2D(CaptureWidth, height,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0, 0, CaptureWidth, height),
                    0, 0, false);
                readback.Apply(false, false);
                string relative = CameraResultRoot + "/" + runId + "-" +
                    name + ".png";
                string path = ProjectPath(relative);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, readback.EncodeToPNG());
                if (name == "baseline")
                    report.baselineImage = relative;
                else
                    report.postTickImage = relative;
                return readback.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(camera);
                report.cameraStateRestored =
                    (name == "baseline" || report.cameraStateRestored) &&
                    saved.Matches(camera);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void CompareShadowPixels(Camera camera,
            BattlePresentationFrame frame, int shadowIndex,
            BattleRenderCommand shadowCommand, Color32[] baseline,
            Color32[] post, Report report)
        {
            Require(frame != null && shadowIndex >= 0 && baseline != null &&
                post != null && baseline.Length == post.Length &&
                report.cameraStateRestored,
                "Camera capture or state restoration was incomplete.");
            RectInt shadow = ProjectCommandBounds(camera, shadowCommand,
                report.captureHeight);
            report.shadowBoundsX = shadow.x;
            report.shadowBoundsY = shadow.y;
            report.shadowBoundsWidth = shadow.width;
            report.shadowBoundsHeight = shadow.height;
            RectInt[] exclusions = new RectInt[frame.CommandCount - 1];
            int exclusionCount = 0;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                if (index == shadowIndex)
                    continue;
                RectInt bounds = ProjectCommandBounds(camera,
                    frame.GetCommand(index), report.captureHeight);
                exclusions[exclusionCount++] = RectFromLimits(
                    Mathf.Max(0, bounds.xMin - 2),
                    Mathf.Max(0, bounds.yMin - 2),
                    Mathf.Min(CaptureWidth, bounds.xMax + 2),
                    Mathf.Min(report.captureHeight, bounds.yMax + 2));
            }
            for (int y = shadow.yMin; y < shadow.yMax; y++)
            {
                for (int x = shadow.xMin; x < shadow.xMax; x++)
                {
                    var point = new Vector2Int(x, y);
                    bool excluded = false;
                    for (int index = 0; index < exclusionCount; index++)
                    {
                        if (!exclusions[index].Contains(point))
                            continue;
                        excluded = true;
                        break;
                    }
                    if (excluded)
                        continue;
                    int pixelIndex = y * CaptureWidth + x;
                    if (!Near(baseline[pixelIndex], Clear, 2))
                        continue;
                    report.exclusiveWhiteBaselinePixels++;
                    if (Near(post[pixelIndex], Clear, 2))
                        continue;
                    report.exclusiveChangedPixels++;
                    if (report.exclusiveChangedPixels == 1)
                    {
                        report.sampleX = x;
                        report.sampleY = y;
                        Color32 sample = post[pixelIndex];
                        report.samplePostRgb = sample.r + "," + sample.g + "," +
                            sample.b;
                    }
                }
            }
            report.cameraPixelStatus = report.exclusiveChangedPixels > 0
                ? "PASS_EXCLUSIVE_SHADOW"
                : "PIXEL_OWNERSHIP_UNPROVEN";
        }

        private static RectInt ProjectCommandBounds(Camera camera,
            BattleRenderCommand command, int height)
        {
            float width = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                NTSDRenderSpace.BattleVisualScale;
            float bodyHeight = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * width;
            float bottom = command.Position.y - command.Pivot.y * bodyHeight;
            Vector3 lower = camera.WorldToViewportPoint(
                new Vector3(left, bottom, command.Position.z));
            Vector3 upper = camera.WorldToViewportPoint(
                new Vector3(left + width, bottom + bodyHeight,
                    command.Position.z));
            return RectFromLimits(
                Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.x, upper.x) *
                    CaptureWidth), 0, CaptureWidth),
                Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.y, upper.y) *
                    height), 0, height),
                Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.x, upper.x) *
                    CaptureWidth), 0, CaptureWidth),
                Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.y, upper.y) *
                    height), 0, height));
        }

        private static RectInt RectFromLimits(int xMin, int yMin,
            int xMax, int yMax)
        {
            return new RectInt(xMin, yMin,
                Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));
        }

        private static bool Near(Color32 left, Color32 right, int tolerance)
        {
            return Math.Abs(left.r - right.r) <= tolerance &&
                   Math.Abs(left.g - right.g) <= tolerance &&
                   Math.Abs(left.b - right.b) <= tolerance;
        }

        private readonly struct CameraState
        {
            private readonly int cullingMask;
            private readonly CameraClearFlags clearFlags;
            private readonly Color background;
            private readonly bool hdr;
            private readonly bool msaa;
            private readonly RenderTexture target;

            public CameraState(Camera camera)
            {
                cullingMask = camera.cullingMask;
                clearFlags = camera.clearFlags;
                background = camera.backgroundColor;
                hdr = camera.allowHDR;
                msaa = camera.allowMSAA;
                target = camera.targetTexture;
            }

            public void Restore(Camera camera)
            {
                camera.targetTexture = target;
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = background;
                camera.allowHDR = hdr;
                camera.allowMSAA = msaa;
            }

            public bool Matches(Camera camera)
            {
                return camera.targetTexture == target &&
                       camera.cullingMask == cullingMask &&
                       camera.clearFlags == clearFlags &&
                       camera.backgroundColor == background &&
                       camera.allowHDR == hdr &&
                       camera.allowMSAA == msaa;
            }
        }

        private static LF2Character Create(SimulationWorld world,
            LF2CharacterDataWrapper data, int oid, int slot, int team,
            int x, int y, int z, int action)
        {
            var entity = new LF2Character();
            entity.ModuleInitialize();
            entity.ObjectId = oid;
            entity.Name = "Q09FormalPlatform" + oid;
            entity.FrameCache.Load(data);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.ImmediateFrame(action);
            entity.Frame.Prev = action;
            entity.Frame.Prev2 = action;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = action;
            entity.Initialize(500, 500);
            entity.AiControlled = false;
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private static void Finish(Request request, Report report)
        {
            if (ValidRunId(request.runId))
            {
                string output = ResultPath(request.runId,
                    request.captureCamera);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!File.Exists(output))
                    File.WriteAllText(output, JsonUtility.ToJson(report, true));
            }
            request.requested = false;
            request.running = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
            stableTick = -1;
            stableUpdates = 0;
            pauseCaptured = false;
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static bool ValidRunId(string runId)
        {
            if (string.IsNullOrEmpty(runId) || runId.Length > 80)
                return false;
            foreach (char value in runId)
                if (!char.IsLetterOrDigit(value) && value != '-' && value != '_')
                    return false;
            return true;
        }

        private static string ResultPath(string runId, bool captureCamera) =>
            ProjectPath((captureCamera ? CameraResultRoot : ResultRoot) +
                "/" + runId + ".json");

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string HashFile(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
#endif
