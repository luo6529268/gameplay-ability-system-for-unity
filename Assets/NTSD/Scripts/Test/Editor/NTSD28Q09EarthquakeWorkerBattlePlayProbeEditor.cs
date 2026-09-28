#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
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
    internal static class NTSD28Q09EarthquakeWorkerBattlePlayProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_Q09_EarthquakeWorkerBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001";
        private const string InlineResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P13-INLINE-BACKGROUND-PLAY-001";
        private const string BattleScenePath =
            "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const int CaptureWidth = 1024;
        private const int CaptureHeight = 576;
        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(6);

        private enum Stage
        {
            WaitingForBattle,
            ReadyActive,
            WaitingActive,
            ReadyReset,
            WaitingReset,
        }

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public bool useInline;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string executionPath;
            public string status;
            public string error;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public int startTick;
            public int activeTick;
            public int resetTick;
            public int hanSlot = -1;
            public bool workerActiveAtStart;
            public bool workerActiveAfterReset;
            public string workerFailure;
            public int activeRuntimeOwner;
            public int activeRuntimeX;
            public int activeRuntimeY;
            public int activeFrameOwner;
            public int activeFrameX;
            public int activeFrameY;
            public int activeFrameTick;
            public int resetRuntimeOwner;
            public int resetRuntimeX;
            public int resetRuntimeY;
            public int resetFrameOwner;
            public int resetFrameX;
            public int resetFrameY;
            public int resetFrameTick;
            public string baselinePng;
            public string activePng;
            public string resetPng;
            public Vector3 mapPositionBefore;
            public Vector3 mapPositionAfter;
            public Vector3 mapBoundsCenterBefore;
            public Vector3 mapBoundsCenterAfter;
            public Vector3 cameraPositionBefore;
            public Vector3 cameraPositionAfter;
            public float cameraSizeBefore;
            public float cameraSizeAfter;
            public bool materialRestored = true;
            public bool cameraRestored = true;
            public bool stopped;
            public int borrowersAfter = -1;
        }

        private static Stage stage;
        private static DateTime startedUtc;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int editorUpdates;
        private static int completedUpdate;
        private static int expectedTick;
        private static Request request;
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character han;
        private static Camera camera;
        private static SpriteRenderer map;
        private static Material originalMaterial;

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
            {
                Reset();
                return;
            }

            Request current;
            try
            {
                current = JsonUtility.FromJson<Request>(
                    File.ReadAllText(requestFile));
            }
            catch (IOException)
            {
                return;
            }
            if (current == null || !current.requested)
            {
                Reset();
                return;
            }
            if (string.IsNullOrEmpty(current.runId) ||
                !current.runId.All(character =>
                    char.IsLetterOrDigit(character) || character == '-'))
            {
                Debug.LogError("Q09 worker probe rejected an invalid runId.");
                File.WriteAllText(requestFile, JsonUtility.ToJson(new Request
                {
                    requested = false,
                }));
                Reset();
                return;
            }
            if (startedUtc == default)
            {
                startedUtc = DateTime.UtcNow;
                request = current;
            }
            if (DateTime.UtcNow - startedUtc > Timeout)
            {
                FinishFailure(current, "Bounded startup/worker timeout.");
                return;
            }
            if (report == null && File.Exists(ProjectPath(
                    ResultRootFor(current) + "/" + current.runId + ".json")))
            {
                FinishFailure(current, "Refusing to overwrite a report.");
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                if (stage != Stage.WaitingForBattle || report != null)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (scene.path != BattleScenePath || scene.isDirty)
                {
                    FinishFailure(current,
                        "Requires the saved clean original Battle Scene.");
                    return;
                }
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            editorUpdates++;
            try
            {
                if (report == null)
                {
                    TryBegin(current);
                    return;
                }
                if (driver == null || world == null ||
                    (!request.useInline &&
                     driver.DedicatedSimulationWorkerFailureForDiagnostics != null))
                {
                    throw new InvalidOperationException(
                        "Worker or battle world failed: " +
                        driver?.DedicatedSimulationWorkerFailureForDiagnostics);
                }

                switch (stage)
                {
                    case Stage.ReadyActive:
                        TrySchedule(Stage.WaitingActive);
                        break;
                    case Stage.WaitingActive:
                        if (PublicationReady())
                            ObserveActive();
                        break;
                    case Stage.ReadyReset:
                        TrySchedule(Stage.WaitingReset);
                        break;
                    case Stage.WaitingReset:
                        if (PublicationReady())
                            ObserveReset();
                        break;
                }
            }
            catch (Exception error)
            {
                FinishFailure(current, error.ToString());
            }
        }

        private static void TryBegin(Request current)
        {
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5)
                return;
            if (driver.LifecycleState != BattleRuntimeLifecycleState.Running)
                return;
            if (!current.useInline &&
                driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
                throw new InvalidOperationException(
                    driver.DedicatedSimulationWorkerFailureForDiagnostics.ToString());
            if (!current.useInline &&
                !driver.DedicatedSimulationWorkerActiveForDiagnostics)
            {
                throw new InvalidOperationException(
                    "The original Battle Scene did not start the dedicated worker: " +
                    driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics);
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                stableUpdates = 0;
                return;
            }
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 4)
                return;

            report = new Report
            {
                runId = current.runId,
                executionPath = current.useInline ? "inline" : "dedicated-worker",
                status = "RUNNING",
                sceneHashBefore = HashFile(ProjectPath(BattleScenePath)),
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
                startTick = driver.CurrentTickIndex,
                workerActiveAtStart =
                    driver.DedicatedSimulationWorkerActiveForDiagnostics,
            };
            Require(SceneManager.GetActiveScene().path == BattleScenePath,
                "Active Scene changed during startup.");
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Formal battle content root is not selected.");
            Require(world.BattleGameModeId == 0,
                "The controlled formal Han case requires mode 0.");
            LF2CharacterDataWrapper config =
                world.RuntimeCharacterConfigs.Resolve(726);
            Require(config?.characterData != null,
                "Current formal Han definition is unavailable.");
            report.hanSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                50, 1000);
            Require(report.hanSlot >= 50,
                "No free runtime slot for controlled Han.");
            han = new LF2Character();
            han.ModuleInitialize();
            han.ObjectId = 726;
            han.Name = "Q09EarthquakeWorkerHan";
            han.FrameCache.Load(config);
            han.SetRequiredRuntimeSlot(report.hanSlot);
            world.Register(han);
            han.ImmediateFrame(0);
            han.Initialize(500, 500);
            han.AiControlled = false;
            han.Team = 3;
            han.RelationTeam = 3;
            han.Runtime.SetPosition(500, 0, 650);
            AppManager.SyncParticipantBirthPosition(han, 500, 650);

            camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.enabled &&
                    camera.gameObject.activeInHierarchy,
                "Saved Battle world camera is unavailable.");
            BattleBackgroundPlatformPresentation background =
                Resources.FindObjectsOfTypeAll<BattleBackgroundPlatformPresentation>()
                    .FirstOrDefault(candidate => candidate != null &&
                        !EditorUtility.IsPersistent(candidate) &&
                        candidate.gameObject.scene ==
                        SceneManager.GetActiveScene());
            Require(background != null,
                "Project Map background presentation is unavailable.");
            map = background.GetComponent<SpriteRenderer>();
            Require(map != null && map.sprite != null,
                "Project Map sprite is unavailable.");
            originalMaterial = map.sharedMaterial;
            report.mapPositionBefore = map.transform.position;
            report.mapBoundsCenterBefore = map.bounds.center;
            report.cameraPositionBefore = camera.transform.position;
            report.cameraSizeBefore = camera.orthographicSize;
            Require(world.Runtime.Earthquake.BackgroundOffsetX == 0 &&
                    world.Runtime.Earthquake.BackgroundOffsetY == 0,
                "Baseline background offset is not zero.");
            report.baselinePng = CaptureMapCamera("baseline");
            han.ImmediateFrame(150);
            stage = Stage.ReadyActive;
        }

        private static void TrySchedule(Stage waitingStage)
        {
            if (!request.useInline &&
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            int nextTick = driver.CurrentTickIndex + 1;
            if (request.useInline)
            {
                Require(driver.StepOneTick(ignorePaused: true,
                        buildPresentation: true),
                    "Inline Driver rejected complete tick " + nextTick);
            }
            else if (!driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                         buildPresentation: true))
            {
                string reason =
                    driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics;
                if (reason == "worker-is-not-ready-for-diagnostic-submission")
                    return;
                throw new InvalidOperationException(
                    "Worker rejected the diagnostic tick: " + reason);
            }
            expectedTick = nextTick;
            stage = waitingStage;
        }

        private static bool PublicationReady()
        {
            if ((!request.useInline &&
                 driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) ||
                driver.CurrentTickIndex < expectedTick)
                return false;
            BattlePresentationFrame frame =
                world.BattlePresentation?.PublishedFrame;
            if (frame == null || frame.TickIndex != expectedTick)
                return false;
            if (completedUpdate != editorUpdates - 1)
            {
                completedUpdate = editorUpdates;
                return false;
            }
            return true;
        }

        private static void ObserveActive()
        {
            NTSD28EarthquakeRuntimeState quake = world.Runtime.Earthquake;
            BattlePresentationFrame frame =
                world.BattlePresentation.PublishedFrame;
            report.activeTick = driver.CurrentTickIndex;
            report.activeRuntimeOwner = quake.OwnerSlot;
            report.activeRuntimeX = quake.BackgroundOffsetX;
            report.activeRuntimeY = quake.BackgroundOffsetY;
            report.activeFrameOwner = frame.EarthquakeOwnerSlot;
            report.activeFrameX = frame.EarthquakeBackgroundOffsetX;
            report.activeFrameY = frame.EarthquakeBackgroundOffsetY;
            report.activeFrameTick = frame.TickIndex;
            Require(report.activeTick == expectedTick &&
                    report.activeFrameTick == expectedTick &&
                    report.activeRuntimeOwner == report.hanSlot &&
                    report.activeRuntimeX == 2 &&
                    report.activeRuntimeY == 0 &&
                    report.activeFrameOwner == report.activeRuntimeOwner &&
                    report.activeFrameX == report.activeRuntimeX &&
                    report.activeFrameY == report.activeRuntimeY,
                "Active earthquake publication first difference.");
            report.activePng = CaptureMapCamera("active");
            han.ImmediateFrame(151);
            stage = Stage.ReadyReset;
        }

        private static void ObserveReset()
        {
            NTSD28EarthquakeRuntimeState quake = world.Runtime.Earthquake;
            BattlePresentationFrame frame =
                world.BattlePresentation.PublishedFrame;
            report.resetTick = driver.CurrentTickIndex;
            report.resetRuntimeOwner = quake.OwnerSlot;
            report.resetRuntimeX = quake.BackgroundOffsetX;
            report.resetRuntimeY = quake.BackgroundOffsetY;
            report.resetFrameOwner = frame.EarthquakeOwnerSlot;
            report.resetFrameX = frame.EarthquakeBackgroundOffsetX;
            report.resetFrameY = frame.EarthquakeBackgroundOffsetY;
            report.resetFrameTick = frame.TickIndex;
            Require(report.resetTick == expectedTick &&
                    report.resetFrameTick == expectedTick &&
                    report.resetRuntimeX == 0 && report.resetRuntimeY == 0 &&
                    report.resetFrameOwner == report.resetRuntimeOwner &&
                    report.resetFrameX == 0 && report.resetFrameY == 0,
                "Reset earthquake publication first difference.");
            report.resetPng = CaptureMapCamera("reset");
            report.workerActiveAfterReset =
                driver.DedicatedSimulationWorkerActiveForDiagnostics;
            Require(request.useInline || report.workerActiveAfterReset,
                "The worker stopped before reset publication.");
            report.status = request.useInline
                ? "PASS_INLINE_FRAME_CAPTURED_PIXEL_ANALYSIS_PENDING"
                : "PASS_WORKER_FRAME_CAPTURED_PIXEL_ANALYSIS_PENDING";
            Finish();
        }

        private static string CaptureMapCamera(string label)
        {
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(
                world);
            Require(plan.IsValid && !plan.IsStale &&
                    plan.SimulationTick == world.CurrentTickIndex,
                "Camera plan is unavailable for " + label);
            var target = new RenderTexture(CaptureWidth, CaptureHeight, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            int previousMask = camera.cullingMask;
            CameraClearFlags previousClear = camera.clearFlags;
            Color previousColor = camera.backgroundColor;
            bool previousHdr = camera.allowHDR;
            bool previousMsaa = camera.allowMSAA;
            Texture2D readback = null;
            try
            {
                target.Create();
                camera.targetTexture = target;
                camera.cullingMask = 1 << map.gameObject.layer;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.Render();
                report.materialRestored &= map.sharedMaterial == originalMaterial;
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, CaptureHeight,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight),
                    0, 0, false);
                readback.Apply(false, false);
                string relative = ResultRootFor(request) + "/" + request.runId +
                                  "-" + label + ".png";
                string output = ProjectPath(relative);
                Require(!File.Exists(output),
                    "Refusing to overwrite capture " + label);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, readback.EncodeToPNG());
                return relative;
            }
            finally
            {
                RenderTexture.active = previousActive;
                camera.targetTexture = previousTarget;
                camera.cullingMask = previousMask;
                camera.clearFlags = previousClear;
                camera.backgroundColor = previousColor;
                camera.allowHDR = previousHdr;
                camera.allowMSAA = previousMsaa;
                report.cameraRestored &=
                    camera.targetTexture == previousTarget &&
                    camera.cullingMask == previousMask &&
                    camera.clearFlags == previousClear &&
                    camera.backgroundColor == previousColor &&
                    camera.allowHDR == previousHdr &&
                    camera.allowMSAA == previousMsaa &&
                    RenderTexture.active == previousActive;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void FinishFailure(Request current, string message)
        {
            if (report == null)
            {
                report = new Report
                {
                    runId = current?.runId,
                    executionPath = current?.useInline == true
                        ? "inline" : "dedicated-worker",
                    status = "FAIL",
                    error = message,
                    sceneHashBefore = HashFile(ProjectPath(BattleScenePath)),
                };
            }
            else
            {
                report.status = "FAIL";
                report.error = message;
            }
            Finish();
        }

        private static void Finish()
        {
            if (report == null)
                return;
            try
            {
                if (map != null && camera != null)
                {
                    report.mapPositionAfter = map.transform.position;
                    report.mapBoundsCenterAfter = map.bounds.center;
                    report.cameraPositionAfter = camera.transform.position;
                    report.cameraSizeAfter = camera.orthographicSize;
                    Require(report.mapPositionBefore == report.mapPositionAfter &&
                            report.mapBoundsCenterBefore ==
                            report.mapBoundsCenterAfter &&
                            report.cameraPositionBefore ==
                            report.cameraPositionAfter &&
                            Mathf.Approximately(report.cameraSizeBefore,
                                report.cameraSizeAfter) &&
                            report.materialRestored && report.cameraRestored,
                        "Map or fixed-camera geometry/material changed.");
                }
                if (driver != null && EditorApplication.isPlaying)
                {
                    report.workerFailure =
                        driver.DedicatedSimulationWorkerFailureForDiagnostics
                            ?.ToString();
                    BattleRuntimeShutdownReport shutdown =
                        driver.ShutdownBattleRuntime();
                    bool mapCleared = true;
                    foreach (BattleBootstrap bootstrap in
                        Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null ||
                            EditorUtility.IsPersistent(bootstrap) ||
                            !bootstrap.gameObject.scene.IsValid())
                            continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    if (shutdown.RuntimeStagesCompleted)
                        shutdown =
                            driver.CompleteBattleRuntimeShutdownAfterMapCleanup(
                                mapCleared);
                    report.stopped = shutdown.IsComplete;
                    LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                    report.borrowersAfter = pool == null ? 0 :
                        pool.ActiveObjectCountForAcceptance +
                        pool.ActiveSpriteCountForAcceptance;
                    Require(report.stopped && report.borrowersAfter == 0,
                        "Ordered shutdown or pool borrower postcondition failed.");
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error += "\nCleanup: " + error;
            }
            report.sceneHashAfter = HashFile(ProjectPath(BattleScenePath));
            if (report.sceneHashBefore != report.sceneHashAfter)
            {
                report.status = "FAIL";
                report.error += "\nSaved Battle Scene hash changed.";
            }
            string requestFile = ProjectPath(RequestPath);
            File.WriteAllText(requestFile, JsonUtility.ToJson(new Request
            {
                requested = false,
                runId = report.runId,
                useInline = request?.useInline == true,
            }));
            string output = ProjectPath(ResultRootFor(request) + "/" +
                                        report.runId + ".json");
            if (!File.Exists(output))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllText(output, JsonUtility.ToJson(report, true));
            }
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlaying)
                    EditorApplication.ExitPlaymode();
            };
            Reset();
        }

        private static string ProjectPath(string relative)
        {
            return Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, relative));
        }

        private static string ResultRootFor(Request current)
        {
            return current?.useInline == true ? InlineResultRoot : ResultRoot;
        }

        private static string HashFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Reset()
        {
            stage = Stage.WaitingForBattle;
            startedUtc = default;
            stableTick = -1;
            stableUpdates = 0;
            editorUpdates = 0;
            completedUpdate = 0;
            expectedTick = 0;
            request = null;
            report = null;
            driver = null;
            world = null;
            han = null;
            camera = null;
            map = null;
            originalMaterial = null;
        }
    }
}
#endif
