#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class BattleCentralProductionWindowSceneProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string LegacyOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/corrected-02/";
        private static string OutputRoot => !string.IsNullOrEmpty(report?.outputRoot) ? report.outputRoot : LegacyOutputRoot;
        private const string SessionKey = "NTSD.Optimization.ProductionWindow.14";
        private const int WindowTicks = 96;
        private const int MaximumSamples = 2048;
        private static Report report;
        private static SimulationTickDriver driver;
        private static FrameSample[] samples;
        private static BattleCentralMaterializationReport baseline;
        private static Func<SimulationWorld, double> readAlpha;
        private static readonly BattleCentralSubmission[] observedSlots = new BattleCentralSubmission[2];
        private static bool cameraObservationOpen;
        private static long cameraAllocationStart;
        private static int beginPlanGeneration;
        private static int beginPlanSlot;
        private static int beginMeshIdentity;
        private static int beginSubmissionDrawCount;
        private static Keyboard dynamicKeyboard;
        private static LF2Character dynamicActor;
        private static DynamicEntityRecord[] dynamicEntities;
        private static int dynamicEntityCount;
        private static int dynamicStep;
        private static int dynamicLastInputTick;
        private static bool dynamicInputReleased;
        private const int MaximumDynamicEntities = 128;
        private const int MaximumTimingCommands = 512;
        private static BattleRenderCommand[] timingCommands;
        private static Vector3[] timingOffsets;
        private static int timingCommandCount;
        private static int timingPublicationTick = -1;
        private static long cameraBeginTimestamp;
        private static Func<double> readBuiltAlpha;
        private static Func<long> readPublicationTimestamp;
        private static Func<int> readPublicationVersion;
        private static Func<int> readMaterializedVersion;
        private static Func<bool> readFootEnabled;
        private static Func<Sprite> readFootSprite;

        [Serializable]
        private sealed class Report
        {
            public string status = "RUNNING";
            public string phase = "STARTUP";
            public string startedUtc;
            public string error;
            public int cycle;
            public string outputRoot;
            public bool runNextCycle = true;
            public bool dynamicInput;
            public bool sampleTiming;
            public int targetCameraFrames;
            public bool replayProductionCatalog;
            public string catalogReplayResult;
            public bool diagnoseFootCoverage;
            public string footCoverage;
            public FootAuthoringObservation footAuthoringBefore;
            public FootAuthoringObservation footAuthoringAfter;
            public int samePublicationPairs;
            public int interpolatedMovingCommandComparisons;
            public int timingCommandComparisons;
            public int cameraAlphaEnvelopeComparisons;
            public double maximumGeometryDeltaErrorPixels;
            public double maximumFirstObservedPublicationAgeMs;
            public string timingCoverage;
            public int windowTicks = WindowTicks;
            public int movingCameraSamples;
            public int snapshotEntityComparisons;
            public int newObservedEntities;
            public int retiredObservedEntities;
            public int inputSequenceStep;
            public string dynamicCoverage;
            public DynamicEntityRecord[] dynamicEntities;
            public int startTick;
            public int endTick;
            public int sampleCount;
            public int observedSubmissionSlots;
            public int configuredRenderFps;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public bool sceneClean;
            public bool orderedShutdown;
            public int remainingObjects = -1;
            public int remainingSlots = -1;
            public int remainingBorrowers = -1;
            public long cameraRenderEnvelopeAllocatedBytes;
            public int cameraRenderEnvelopeNonzeroFrames;
            public long observerAllocatedBytes;
            public bool collectionControlSupported;
            public bool playerLoopHardGateSupported;
            public MemorySnapshot memoryBefore;
            public MemorySnapshot memoryAfter;
            public FrameSample[] frames;
        }

        [Serializable]
        private sealed class FootAuthoringObservation
        {
            public string pipelineType;
            public int loadedPreviewCount;
            public int eligiblePreviewCount;
            public int activeEligiblePreviewCount;
            public bool authoringAvailable;
            public bool authoredEnabled;
            public string authoredSpritePath;
            public bool registeredMaterialAvailable;
            public bool runtimeEnabled;
            public bool runtimeSpriteAvailable;
            public string runtimeSpritePath;
            public int runtimeAnimationFrameCount;
            public float runtimeFrameDurationSeconds;
            public string gameConfigPath;
            public bool gameConfigSpriteAvailable;
            public string gameConfigSpritePath;
            public int gameConfigValidFrameCount;
            public string[] gameConfigAnimationFramePaths;
            public float gameConfigFrameDurationSeconds;
        }

        [Serializable]
        private struct MemorySnapshot
        {
            public long tickBytes;
            public long driverUpdateBytes;
            public long latePresentationBytes;
            public long playerLoopBytes;
            public int gen0Collections;
            public int gen1Collections;
            public int gen2Collections;
        }

        [Serializable]
        private struct FrameSample
        {
            public int unityFrame;
            public int logicTick;
            public int publicationTick;
            public int displayTick;
            public int generation;
            public int beginPlanGeneration;
            public int beginPlanSlot;
            public int beginMeshIdentity;
            public double alpha;
            public double builtAlpha;
            public double publicationAgeAtCameraBeginMs;
            public double publicationAgeAtCameraEndMs;
            public int queuePublicationVersion;
            public int materializedPublicationVersion;
            public int timingCommandComparisons;
            public int entities;
            public int commands;
            public int resolvedCommands;
            public int segments;
            public int chunks;
            public int capacityGrowth;
            public int vertexUploadCalls;
            public long uploadedVertexBytes;
            public int renderPassRecordedDraws;
            public int cameraExecutedDraws;
            public int activeFootMarkers;
            public int selfFootCommandCount;
            public bool runtimeFootEnabled;
            public bool runtimeFootSpriteAvailable;
            public int activeHealthBars;
            public int sourceTextureSegments;
            public int atlasPageSegments;
            public int textureArraySegments;
            public bool hasBoundCatalog;
            public int submissionSlot;
            public int meshIdentity;
            public int leaseCountAfterCamera;
            public long cameraRenderEnvelopeAllocatedBytes;
            public Vector3 firstEntityPosition;
            public int actorAction;
            public double actorSourceX;
            public double actorSourceZ;
            public int actorInputButtons;
            public int comparedEntities;
        }

        [Serializable]
        private struct DynamicEntityRecord
        {
            public int slot;
            public uint handleGeneration;
            public int stableId;
            public int objectId;
            public bool initial;
            public int firstObservedPublicationTick;
            public int firstVisiblePublicationTick;
            public int firstBodyCommandTick;
            public int lastObservedPublicationTick;
            public int firstAbsentPublicationTick;
            public bool present;
        }

        static BattleCentralProductionWindowSceneProbeEditor()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlay;
            RenderPipelineManager.beginCameraRendering += BeginCamera;
            RenderPipelineManager.endCameraRendering += EndCamera;
        }

        [MenuItem("NTSD/Validation/Optimization/Production Window Two Cycles")]
        private static void Start()
        {
            Require(report == null && string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "Another production-window probe is active.");
            RequireIdleOriginalScene();
            Require(!File.Exists(ResultPath(1)) && !File.Exists(ResultPath(2)),
                "Refuses to overwrite prior evidence.");
            report = new Report { cycle = 1, startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene() };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("NTSD/Validation/Optimization/Batch15 Production Before")]
        private static void StartBatch15Before() => StartBatch15("before", 1);

        [MenuItem("NTSD/Validation/Optimization/Batch15 Production After First")]
        private static void StartBatch15AfterFirst() => StartBatch15("after", 1);

        [MenuItem("NTSD/Validation/Optimization/Batch15 Production After Second")]
        private static void StartBatch15AfterSecond() => StartBatch15("after", 2);

        [MenuItem("NTSD/Validation/Optimization/Batch16 Dynamic Display")]
        private static void StartBatch16Dynamic()
        {
            Require(report == null && string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "Another production-window probe is active.");
            RequireIdleOriginalScene();
            const string root = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007/run-01/";
            Require(!File.Exists(root + "production-window-01.json") &&
                !File.Exists(root + "materialization-window-01.json"), "Refuses to overwrite batch16 evidence.");
            Directory.CreateDirectory(root);
            report = new Report
            {
                cycle = 1, outputRoot = root, runNextCycle = false, dynamicInput = true, windowTicks = 240,
                startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene(),
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("NTSD/Validation/Optimization/Batch18 Display Sample Timing")]
        private static void StartBatch18Timing()
        {
            Require(report == null && string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "Another production-window probe is active.");
            RequireIdleOriginalScene();
            const string root = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007/run-01/";
            Require(!File.Exists(root + "production-window-01.json") &&
                !File.Exists(root + "materialization-window-01.json"), "Refuses to overwrite batch18 evidence.");
            Directory.CreateDirectory(root);
            report = new Report
            {
                cycle = 1, outputRoot = root, runNextCycle = false, dynamicInput = true,
                sampleTiming = true, windowTicks = 240,
                startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene(),
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("NTSD/Validation/Optimization/Batch20 Production Catalog 1800 Cameras")]
        private static void StartBatch20Catalog()
        {
            Require(report == null && string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "Another production-window probe is active.");
            RequireIdleOriginalScene();
            const string root = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH20-PRODUCTION-CATALOG-20261007/run-01/";
            Require(!Directory.Exists(root), "Refuses to reuse batch20 evidence directory.");
            Directory.CreateDirectory(root);
            report = new Report
            {
                cycle = 1, outputRoot = root, runNextCycle = false, targetCameraFrames = 1800,
                replayProductionCatalog = true, startedUtc = DateTime.UtcNow.ToString("O"),
                sceneHashBefore = HashScene(),
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("NTSD/Validation/Optimization/Batch21 Foot Authoring Diagnosis 64 Cameras")]
        private static void StartBatch21FootDiagnosis()
        {
            Require(report == null && string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "Another production-window probe is active.");
            RequireIdleOriginalScene();
            const string root = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007/run-01/";
            Require(!Directory.Exists(root), "Refuses to reuse batch21 evidence directory.");
            Directory.CreateDirectory(root);
            report = new Report
            {
                cycle = 1, outputRoot = root, runNextCycle = false, targetCameraFrames = 64,
                diagnoseFootCoverage = true, startedUtc = DateTime.UtcNow.ToString("O"),
                sceneHashBefore = HashScene(),
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        private static void StartBatch15(string phase, int cycle)
        {
            Require(report == null && string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "Another production-window probe is active.");
            RequireIdleOriginalScene();
            string root = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/" + phase + "/";
            Require(!File.Exists(root + "production-window-" + cycle.ToString("00") + ".json") &&
                !File.Exists(root + "materialization-window-" + cycle.ToString("00") + ".json"),
                "Refuses to overwrite prior batch15 evidence.");
            if (cycle == 2)
            {
                Report first = JsonUtility.FromJson<Report>(File.ReadAllText(root + "production-window-01.json"));
                Require(first.status == "PASS" && first.sceneClean && first.orderedShutdown,
                    "Requires a clean, completed first cycle.");
            }
            Directory.CreateDirectory(root);
            report = new Report
            {
                cycle = cycle, outputRoot = root, runNextCycle = false,
                startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene(),
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            RestoreSession();
            if (report == null || report.phase == "SHUTDOWN_FAILED")
                return;
            try
            {
                Require((DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime()).TotalSeconds < 900,
                    "Production-window deadline exceeded.");
                Require(report.phase != "STARTUP" ||
                    (DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime()).TotalSeconds < 600,
                    "Original Scene startup deadline exceeded.");
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        Finish();
                    return;
                }
                if (!EditorApplication.isPlaying)
                    return;
                if (report.phase == "COMPLETE")
                {
                    CompleteWindow();
                    Exit();
                    return;
                }
                if (report.phase == "OBSERVING")
                {
                    if (report.dynamicInput)
                        ApplyDynamicInput();
                    return;
                }
                driver = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                    .FirstOrDefault(value => value != null && value.isActiveAndEnabled && !EditorUtility.IsPersistent(value));
                if (driver?.World == null || driver.CurrentTickIndex < 8 ||
                    !driver.ManagedMemoryBoundary.BattleWindowOpen)
                    return;
                Require(!driver.IsPaused, "Original scene must run naturally; no forced stepping.");
                BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
                if (!plan.UsesCentralPixels || plan.IsStale ||
                    !BattleCentralRenderSystem.Diagnostics.SubmittedPixelsLastFrame)
                    return;
                samples = new FrameSample[MaximumSamples];
                Array.Clear(observedSlots, 0, observedSlots.Length);
                readAlpha = (Func<SimulationWorld, double>)typeof(BattleCentralRenderSystem)
                    .GetMethod("LastResolvedDisplayAlphaForWorld", BindingFlags.Static | BindingFlags.NonPublic)
                    .CreateDelegate(typeof(Func<SimulationWorld, double>));
                baseline = BattleCentralRenderSystem.Diagnostics.CaptureMaterializationReport();
                report.memoryBefore = CaptureMemory(driver.ManagedMemoryBoundary);
                report.collectionControlSupported = driver.ManagedMemoryBoundary.ManagedCollectionControlSupported;
                report.playerLoopHardGateSupported = driver.ManagedMemoryBoundary.PlayerLoopEnvelopeHardGateSupported;
                report.configuredRenderFps = new SerializedObject(driver).FindProperty("battleRenderFps").intValue;
                report.startTick = driver.CurrentTickIndex;
                if (report.sampleTiming)
                {
                    timingCommands = new BattleRenderCommand[MaximumTimingCommands];
                    timingOffsets = new Vector3[MaximumTimingCommands];
                    timingCommandCount = 0;
                    timingPublicationTick = -1;
                    readBuiltAlpha = BindFieldReader<double>("lastBuiltDisplayAlpha");
                    readPublicationTimestamp = BindFieldReader<long>("pendingPublicationTimestamp");
                    readPublicationVersion = BindFieldReader<int>("pendingPublicationVersion");
                    readMaterializedVersion = BindFieldReader<int>("lastMaterializedPublicationVersion");
                }
                if (report.dynamicInput)
                {
                    Require(driver.World.TryResolveRosterInputEntity(0, out LF2Entity actor) &&
                        actor is LF2Character, "Requires current original roster player 0.");
                    dynamicActor = (LF2Character)actor;
                    dynamicKeyboard = Keyboard.current;
                    Require(dynamicKeyboard != null &&
                        (dynamicActor.Controller as CharacterInputModule)?.MoveAction?.enabled == true,
                        "Requires the original enabled Input Actions and Keyboard.");
                    dynamicEntities = new DynamicEntityRecord[MaximumDynamicEntities];
                    dynamicEntityCount = 0;
                    dynamicStep = 0;
                    dynamicLastInputTick = report.startTick;
                    dynamicInputReleased = false;
                    QueueDynamicInput(Key.S);
                }
                if (report.diagnoseFootCoverage)
                {
                    report.footAuthoringBefore = CaptureFootAuthoring();
                    readFootEnabled = BindFieldReader<bool>("runtimeFootMarkersEnabled");
                    readFootSprite = BindFieldReader<Sprite>("runtimeFootMarkerSprite");
                }
                report.phase = "OBSERVING";
                SaveSession();
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private static void BeginCamera(ScriptableRenderContext context, Camera camera)
        {
            if (report == null || report.phase != "OBSERVING" || driver?.World == null ||
                camera != NTSDRenderSpace.WorldCamera)
                return;
            long observerStart = GC.GetAllocatedBytesForCurrentThread();
            BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            beginPlanGeneration = plan.Generation;
            beginSubmissionDrawCount = BattleCentralRenderSystem.Diagnostics.SubmissionCount;
            beginPlanSlot = plan.Submission != null ? ObserveSlot(plan.Submission) : -1;
            beginMeshIdentity = plan.Submission?.Backend.ActiveChunkCount > 0
                ? plan.Submission.Backend.GetChunkMesh(0).GetInstanceID() : 0;
            report.observerAllocatedBytes += Math.Max(0, GC.GetAllocatedBytesForCurrentThread() - observerStart);
            cameraObservationOpen = true;
            if (report.sampleTiming)
                cameraBeginTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            cameraAllocationStart = GC.GetAllocatedBytesForCurrentThread();
        }

        private static void EndCamera(ScriptableRenderContext context, Camera camera)
        {
            if (!cameraObservationOpen || camera != NTSDRenderSpace.WorldCamera)
                return;
            long envelopeBytes = Math.Max(0, GC.GetAllocatedBytesForCurrentThread() - cameraAllocationStart);
            cameraObservationOpen = false;
            long observerStart = GC.GetAllocatedBytesForCurrentThread();
            try
            {
                // Alignment contract: NTSD-OPT-M03-PRODUCTION-WINDOW-014.
                // Read-only after the real camera; never force Build or treat CPU lease release as GPU completion.
                CaptureSample(envelopeBytes);
                report.observerAllocatedBytes += Math.Max(0, GC.GetAllocatedBytesForCurrentThread() - observerStart);
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.error = exception.ToString();
                report.phase = "COMPLETE";
            }
        }

        private static void CaptureSample(long envelopeBytes)
        {
            if (report.targetCameraFrames > 0 && report.sampleCount > 0 &&
                !BattleProductionCatalogReplayEditor.IsNextCameraFrame(samples[report.sampleCount - 1].unityFrame, Time.frameCount))
                return;
            Require(report.sampleCount < MaximumSamples, "Diagnostic sample capacity exceeded; no truncation.");
            SimulationWorld world = driver.World;
            BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            BattlePresentationFrame publication = world.BattlePresentation.PublishedFrame;
            BattlePresentationFrame captured = plan.CapturedFrame;
            Require(plan.UsesCentralPixels && !plan.IsStale && ReferenceEquals(plan.World, world),
                "Natural camera did not have a current central plan.");
            Require(publication != null && captured != null && !ReferenceEquals(publication, captured) &&
                !publication.CommandsMaterialized && !publication.PresentationOrderMaterialized &&
                captured.CommandsMaterialized && captured.PresentationOrderMaterialized,
                "Publication isolation/materialization contract failed.");
            Require(plan.SimulationTick == publication.TickIndex && plan.DisplayTick == captured.TickIndex,
                "Publication and display tick mismatch.");
            BattleDynamicMeshBackend backend = plan.Submission.Backend;
            BattleCentralBuildDiagnostics diagnostics = backend.Diagnostics;
            Require(diagnostics.CapacityGrowthCount == 0 && diagnostics.UnresolvedCommandCount == 0,
                "Production backend grew or failed resource resolution.");
            int priorEnd = 0;
            int sourceSegments = 0, atlasSegments = 0, arraySegments = 0;
            for (int index = 0; index < backend.SegmentCount; index++)
            {
                BattleCentralRenderSegment segment = backend.GetSegment(index);
                Require(segment.FirstCommandIndex >= priorEnd && segment.CommandCount > 0 &&
                    segment.FirstCommandIndex + segment.CommandCount <= captured.CommandCount &&
                    segment.Texture != null && segment.Material != null,
                    "Physical segment range/order/binding invalid.");
                priorEnd = segment.FirstCommandIndex + segment.CommandCount;
                if (segment.BindingMode == BattleSpriteCentralBindingMode.SourceTexture2D) sourceSegments++;
                if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasPageTexture2D) atlasSegments++;
                if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray) arraySegments++;
                Mesh mesh = backend.GetChunkMesh(segment.ChunkIndex);
                Require(mesh != null && mesh.GetVertexBufferStride(0) == 44, "Production vertex layout changed.");
                Bounds bounds = mesh.GetSubMesh(segment.SubMeshIndex).bounds;
                Require(IsFinite(bounds.center) && IsFinite(bounds.extents), "Non-finite production bounds.");
            }
            int slot = ObserveSlot(plan.Submission);
            Require(slot >= 0, "Unexpected third submission slot.");
            Vector3 firstEntityPosition = default;
            int selfFootCommands = 0;
            bool firstEntityFound = false;
            for (int index = 0; index < captured.CommandCount; index++)
            {
                BattleRenderCommand command = captured.GetCommand(index);
                if (command.Type == BattleRenderCommandType.Entity)
                {
                    if (!firstEntityFound)
                    {
                        firstEntityPosition = command.Position;
                        firstEntityFound = true;
                    }
                    if (report.diagnoseFootCoverage && command.ShowSelfFootMarker)
                        selfFootCommands++;
                    if (!report.diagnoseFootCoverage)
                        break;
                }
            }
            int draws = BattleCentralRenderSystem.Diagnostics.LastSubmissionDrawCount;
            Require(draws > 0 && BattleCentralRenderSystem.Diagnostics.SubmittedPixelsLastFrame,
                "Production RenderPass recorded no central draws.");
            int comparedEntities = report.dynamicInput ? CaptureDynamicContract(publication, captured) : 0;
            double builtAlpha = report.sampleTiming ? readBuiltAlpha() : readAlpha(world);
            long cameraEndTimestamp = report.sampleTiming ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
            long publicationTimestamp = report.sampleTiming ? readPublicationTimestamp() : 0;
            double ageBeginMs = report.sampleTiming
                ? (cameraBeginTimestamp - publicationTimestamp) * 1000.0 / System.Diagnostics.Stopwatch.Frequency : 0;
            double ageEndMs = report.sampleTiming
                ? (cameraEndTimestamp - publicationTimestamp) * 1000.0 / System.Diagnostics.Stopwatch.Frequency : 0;
            int timingComparisons = report.sampleTiming
                ? CaptureTimingContract(publication, captured, plan.Generation, builtAlpha, ageBeginMs, ageEndMs) : 0;
            if (report.dynamicInput && report.sampleCount > 0 &&
                (dynamicActor.Runtime.SourceRuleX != samples[report.sampleCount - 1].actorSourceX ||
                 dynamicActor.Runtime.SourceRuleZ != samples[report.sampleCount - 1].actorSourceZ))
                report.movingCameraSamples++;
            samples[report.sampleCount++] = new FrameSample
            {
                unityFrame = Time.frameCount, logicTick = driver.CurrentTickIndex,
                publicationTick = publication.TickIndex, displayTick = plan.DisplayTick, generation = plan.Generation,
                beginPlanGeneration = beginPlanGeneration, beginPlanSlot = beginPlanSlot, beginMeshIdentity = beginMeshIdentity,
                alpha = readAlpha(world), entities = captured.EntityCount, commands = captured.CommandCount,
                builtAlpha = builtAlpha, publicationAgeAtCameraBeginMs = ageBeginMs,
                publicationAgeAtCameraEndMs = ageEndMs,
                queuePublicationVersion = report.sampleTiming ? readPublicationVersion() : 0,
                materializedPublicationVersion = report.sampleTiming ? readMaterializedVersion() : 0,
                timingCommandComparisons = timingComparisons,
                resolvedCommands = diagnostics.ResolvedCommandCount, segments = backend.SegmentCount,
                chunks = backend.ActiveChunkCount, capacityGrowth = diagnostics.CapacityGrowthCount,
                vertexUploadCalls = diagnostics.VertexUploadCallCount, uploadedVertexBytes = diagnostics.UploadedVertexBytes,
                renderPassRecordedDraws = draws, submissionSlot = slot,
                cameraExecutedDraws = BattleCentralRenderSystem.Diagnostics.SubmissionCount - beginSubmissionDrawCount,
                activeFootMarkers = plan.Submission.FootMarkerBackend.ActiveMarkerCount,
                selfFootCommandCount = selfFootCommands,
                runtimeFootEnabled = report.diagnoseFootCoverage && readFootEnabled(),
                runtimeFootSpriteAvailable = report.diagnoseFootCoverage && readFootSprite() != null,
                activeHealthBars = plan.Submission.HealthBackend.ActiveBarCount,
                sourceTextureSegments = sourceSegments, atlasPageSegments = atlasSegments, textureArraySegments = arraySegments,
                hasBoundCatalog = captured.BoundCatalog != null && !ReferenceEquals(captured.BoundCatalog, BattleSpriteCatalog.Empty),
                meshIdentity = backend.ActiveChunkCount > 0 ? backend.GetChunkMesh(0).GetInstanceID() : 0,
                leaseCountAfterCamera = plan.Submission.ReadLeaseCount,
                cameraRenderEnvelopeAllocatedBytes = envelopeBytes, firstEntityPosition = firstEntityPosition,
                actorAction = dynamicActor?.Frame.N ?? -1,
                actorSourceX = dynamicActor?.Runtime.SourceRuleX ?? 0,
                actorSourceZ = dynamicActor?.Runtime.SourceRuleZ ?? 0,
                actorInputButtons = report.dynamicInput ? ReadPlayerButtons() : 0,
                comparedEntities = comparedEntities,
            };
            report.cameraRenderEnvelopeAllocatedBytes += envelopeBytes;
            if (envelopeBytes > 0)
                report.cameraRenderEnvelopeNonzeroFrames++;
            report.endTick = driver.CurrentTickIndex;
            if (BattleProductionCatalogReplayEditor.IsWindowComplete(report.targetCameraFrames, report.sampleCount,
                report.startTick, report.endTick, report.windowTicks, MaximumSamples))
                report.phase = "COMPLETE";
        }

        private static int ReadPlayerButtons()
        {
            FrameInputSet input = driver.LastAppliedFrameInput;
            if (input?.Players != null)
                for (int index = 0; index < input.Players.Count; index++)
                    if (input.Players[index].PlayerSlot == 0)
                        return (int)input.Players[index].Buttons;
            return 0;
        }

        internal static string ClassifyFootCoverage(bool authoringAvailable, bool runtimeEnabled,
            bool runtimeSpriteAvailable, int selfCommands, int activeMarkers)
        {
            if (selfCommands < 0 || activeMarkers < 0)
                throw new ArgumentOutOfRangeException(nameof(selfCommands), "Observed counts cannot be negative.");
            if (!authoringAvailable)
                return selfCommands > 0 ? "NO_LOADED_AUTHORING" : "NO_AUTHORING_AND_SELF_FLAG_UNOBSERVED";
            if (!runtimeEnabled)
                return "RUNTIME_DISABLED";
            if (!runtimeSpriteAvailable)
                return "RUNTIME_SPRITE_UNAVAILABLE";
            if (selfCommands == 0)
                return "SELF_FLAG_UNOBSERVED";
            return activeMarkers > 0 ? "ACTIVITY_OBSERVED_NOT_CERTIFIED" : "FOOT_BACKEND_EMPTY";
        }

        private static FootAuthoringObservation CaptureFootAuthoring()
        {
            var result = new FootAuthoringObservation
            {
                pipelineType = GraphicsSettings.currentRenderPipeline != null
                    ? GraphicsSettings.currentRenderPipeline.GetType().FullName : "BuiltIn",
                registeredMaterialAvailable = BattleCentralRenderSystem.RegisteredFeatureMaterialForAcceptance != null,
                runtimeEnabled = ReadCentralField<bool>("runtimeFootMarkersEnabled"),
                runtimeFrameDurationSeconds = ReadCentralField<float>("runtimeFootMarkerAnimationFrameDurationSeconds"),
            };
            Sprite runtimeSprite = ReadCentralField<Sprite>("runtimeFootMarkerSprite");
            result.runtimeSpriteAvailable = runtimeSprite != null;
            result.runtimeSpritePath = AssetDatabase.GetAssetPath(runtimeSprite);
            result.runtimeAnimationFrameCount = ReadCentralField<Sprite[]>("runtimeFootMarkerAnimationFrames")?.Length ?? 0;
            BattleCentralEditorPreview[] previews = Resources.FindObjectsOfTypeAll<BattleCentralEditorPreview>();
            result.loadedPreviewCount = previews.Length;
            foreach (BattleCentralEditorPreview preview in previews)
            {
                if (preview == null || preview.gameObject == null || !preview.gameObject.scene.IsValid() ||
                    !preview.gameObject.scene.isLoaded || (preview.hideFlags & HideFlags.HideInHierarchy) != 0)
                    continue;
                result.eligiblePreviewCount++;
                if (preview.isActiveAndEnabled)
                    result.activeEligiblePreviewCount++;
            }
            MethodInfo authoring = typeof(BattleCentralEditorPreview).GetMethod(
                "TryGetRuntimeFootMarkerAuthoringSettings", BindingFlags.Static | BindingFlags.NonPublic);
            Require(authoring != null, "Foot authoring diagnostic contract changed.");
            object[] args = { false, null, null, 0f, BattleFootMarkerStyle.Default };
            result.authoringAvailable = (bool)authoring.Invoke(null, args);
            result.authoredEnabled = (bool)args[0];
            result.authoredSpritePath = AssetDatabase.GetAssetPath(args[1] as Sprite);
            GameConfig config = GameConfig.Instance;
            result.gameConfigPath = AssetDatabase.GetAssetPath(config);
            result.gameConfigSpriteAvailable = config != null && config.FootMarkerSprite != null;
            result.gameConfigSpritePath = AssetDatabase.GetAssetPath(config != null ? config.FootMarkerSprite : null);
            Sprite[] frames = config != null ? config.FootMarkerAnimationFrames : null;
            result.gameConfigAnimationFramePaths = new string[frames?.Length ?? 0];
            for (int index = 0; index < result.gameConfigAnimationFramePaths.Length; index++)
            {
                result.gameConfigAnimationFramePaths[index] = AssetDatabase.GetAssetPath(frames[index]);
                if (frames[index] != null && frames[index].texture != null)
                    result.gameConfigValidFrameCount++;
            }
            result.gameConfigFrameDurationSeconds = config != null ? config.FootMarkerAnimationFrameDurationSeconds : 0f;
            return result;
        }

        private static T ReadCentralField<T>(string name)
        {
            FieldInfo field = typeof(BattleCentralRenderSystem).GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
            Require(field != null && field.FieldType == typeof(T), "Foot diagnostic field contract changed.");
            return (T)field.GetValue(null);
        }

        private static Func<T> BindFieldReader<T>(string name)
        {
            FieldInfo field = typeof(BattleCentralRenderSystem).GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
            Require(field != null && field.FieldType == typeof(T), "Timing diagnostic field contract changed.");
            return Expression.Lambda<Func<T>>(Expression.Field(null, field)).Compile();
        }

        private static int CaptureTimingContract(BattlePresentationFrame publication, BattlePresentationFrame captured,
            int generation, double builtAlpha, double ageBeginMs, double ageEndMs)
        {
            Require(captured.CommandCount <= MaximumTimingCommands, "Timing command capacity exceeded; whole observation rejected.");
            Require(readPublicationTimestamp() > 0 && ageBeginMs >= 0 && ageEndMs >= ageBeginMs &&
                builtAlpha >= 0 && builtAlpha <= 1, "Invalid publication timestamp/alpha interval.");
            Require(readPublicationVersion() == readMaterializedVersion(), "Camera retained an older queued publication.");
            double intervalMs = driver.World.BattlePresentationLogicIntervalSeconds * 1000.0;
            // CPU camera interval only; neither a read lease nor this callback proves GPU completion.
            if (generation != beginPlanGeneration && publication.PreviousMotionTickIndex >= 0 &&
                (long)publication.PreviousMotionTickIndex + 1 == publication.TickIndex &&
                report.configuredRenderFps > 30)
            {
                double lower = Math.Max(0, Math.Min(1, ageBeginMs / intervalMs));
                double upper = Math.Max(0, Math.Min(1, ageEndMs / intervalMs));
                Require(builtAlpha + 0.001 >= lower && builtAlpha <= upper + 0.001,
                    "Built alpha excludes queue-to-camera age.");
                report.cameraAlphaEnvelopeComparisons++;
            }
            bool samePublication = timingPublicationTick == publication.TickIndex;
            if (samePublication)
            {
                Require(timingCommandCount == captured.CommandCount, "Same-publication command count changed.");
                report.samePublicationPairs++;
            }
            else
            {
                report.maximumFirstObservedPublicationAgeMs = Math.Max(report.maximumFirstObservedPublicationAgeMs, ageEndMs);
            }
            int comparisons = 0;
            for (int index = 0; index < captured.CommandCount; index++)
            {
                BattleRenderCommand command = captured.GetCommand(index);
                Vector3 offset = IndependentOffset(publication, command, builtAlpha);
                if (samePublication)
                {
                    BattleRenderCommand prior = timingCommands[index];
                    Require(prior.Handle.Equals(command.Handle) && prior.Type == command.Type &&
                        prior.LocalSequence == command.LocalSequence && prior.SortOrder == command.SortOrder,
                        "Same-publication command identity/order changed.");
                    Vector3 expected = prior.Position + offset - timingOffsets[index];
                    double errorPixels = Math.Max(Math.Abs(command.Position.x - expected.x) / NTSDRenderSpace.UnitsPerPixelX,
                        Math.Abs(command.Position.y - expected.y) / NTSDRenderSpace.UnitsPerPixelY);
                    report.maximumGeometryDeltaErrorPixels = Math.Max(report.maximumGeometryDeltaErrorPixels, errorPixels);
                    Require(errorPixels <= 0.005 && Math.Abs(command.Position.z - prior.Position.z) <= 1e-6,
                        "Captured geometry does not follow the same-publication interpolated sample.");
                    if ((offset - timingOffsets[index]).sqrMagnitude > 1e-14f)
                        report.interpolatedMovingCommandComparisons++;
                    comparisons++;
                }
                timingCommands[index] = command;
                timingOffsets[index] = offset;
            }
            timingCommandCount = captured.CommandCount;
            timingPublicationTick = publication.TickIndex;
            report.timingCommandComparisons += comparisons;
            return comparisons;
        }

        private static Vector3 IndependentOffset(BattlePresentationFrame publication, in BattleRenderCommand command, double alpha)
        {
            if (alpha >= 1 || command.Type == BattleRenderCommandType.HitRecord)
                return default;
            for (int index = 0; index < publication.MotionStateCount; index++)
            {
                BattlePresentationMotionState current = publication.GetMotionState(index);
                if (!current.Handle.Equals(command.Handle))
                    continue;
                for (int priorIndex = 0; priorIndex < publication.PreviousMotionStateCount; priorIndex++)
                {
                    BattlePresentationMotionState previous = publication.GetPreviousMotionState(priorIndex);
                    if (!previous.Handle.Equals(current.Handle))
                        continue;
                    if (BattlePresentationMotionSampler.Sample(previous, current,
                        publication.PreviousMotionTickIndex, publication.TickIndex, alpha,
                        driver.World.FixedViewRunDistanceScale, driver.World.SpatialProjection.VerticalScale,
                        driver.World.FixedViewRunVerticalDistanceScale, out _) != BattlePresentationMotionSampleStatus.Sampled)
                        return default;
                    double dx = RoundedDelta(previous.PreciseX, current.PreciseX, alpha) * driver.World.FixedViewRunDistanceScale;
                    double dy = RoundedDelta(previous.PreciseY, current.PreciseY, alpha) * driver.World.SpatialProjection.VerticalScale;
                    double dz = RoundedDelta(previous.PreciseZ, current.PreciseZ, alpha) * driver.World.FixedViewRunVerticalDistanceScale;
                    bool ground = command.Type == BattleRenderCommandType.Shadow ||
                        (command.Type == BattleRenderCommandType.OverlayGlyph && command.MotionAnchor == BattlePresentationMotionAnchor.Ground);
                    return new Vector3((float)dx * NTSDRenderSpace.UnitsPerPixelX,
                        -(float)(dz + (ground ? 0 : dy)) * NTSDRenderSpace.UnitsPerPixelY, 0);
                }
            }
            return default;
        }

        private static double RoundedDelta(double previous, double current, double alpha)
        {
            return Math.Round(previous + (current - previous) * alpha, MidpointRounding.AwayFromZero) -
                Math.Round(current, MidpointRounding.AwayFromZero);
        }

        private static int CaptureDynamicContract(BattlePresentationFrame publication, BattlePresentationFrame captured)
        {
            // Observation is bounded and allocation-free; no sort, materialize, lease or World writes.
            Require(publication.EntityCount == captured.EntityCount, "Published/captured entity count differs.");
            for (int index = 0; index < dynamicEntityCount; index++)
                dynamicEntities[index].present = false;
            for (int index = 0; index < publication.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot source = publication.GetEntity(index);
                bool matched = false;
                for (int candidate = 0; candidate < captured.EntityCount; candidate++)
                {
                    BattlePresentationEntitySnapshot display = captured.GetEntity(candidate);
                    if (!source.Handle.Equals(display.Handle))
                        continue;
                    Require(source.StableId == display.StableId && source.FrameId == display.FrameId &&
                        source.EntityVisible == display.EntityVisible && source.ShadowVisible == display.ShadowVisible,
                        "Latest entity identity/frame/visibility differs in captured publication.");
                    matched = true;
                    break;
                }
                Require(matched, "Current publication entity is missing from captured frame.");
                int recordIndex = -1;
                for (int candidate = 0; candidate < dynamicEntityCount; candidate++)
                    if (dynamicEntities[candidate].slot == source.Handle.Slot &&
                        dynamicEntities[candidate].handleGeneration == source.Handle.Generation &&
                        dynamicEntities[candidate].stableId == source.StableId)
                    {
                        recordIndex = candidate;
                        break;
                    }
                if (recordIndex < 0)
                {
                    Require(dynamicEntityCount < MaximumDynamicEntities, "Dynamic identity capacity exceeded; no truncation.");
                    recordIndex = dynamicEntityCount++;
                    bool initial = report.sampleCount == 0;
                    dynamicEntities[recordIndex] = new DynamicEntityRecord
                    {
                        slot = source.Handle.Slot, handleGeneration = source.Handle.Generation,
                        stableId = source.StableId, objectId = source.ObjectId, initial = initial,
                        firstObservedPublicationTick = publication.TickIndex,
                        firstVisiblePublicationTick = -1, firstBodyCommandTick = -1,
                        firstAbsentPublicationTick = -1,
                    };
                    if (!initial)
                        report.newObservedEntities++;
                }
                ref DynamicEntityRecord record = ref dynamicEntities[recordIndex];
                record.present = true;
                record.lastObservedPublicationTick = publication.TickIndex;
                if (source.EntityVisible && record.firstVisiblePublicationTick < 0)
                    record.firstVisiblePublicationTick = publication.TickIndex;
            }
            for (int index = 0; index < captured.CommandCount; index++)
            {
                BattleRenderCommand command = captured.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Entity)
                    continue;
                bool found = false;
                for (int candidate = 0; candidate < dynamicEntityCount; candidate++)
                {
                    ref DynamicEntityRecord record = ref dynamicEntities[candidate];
                    if (record.slot != command.Handle.Slot || record.handleGeneration != command.Handle.Generation ||
                        record.stableId != command.StableId)
                        continue;
                    Require(record.present, "Retired publication entity remains in a body command.");
                    if (record.firstBodyCommandTick < 0)
                        record.firstBodyCommandTick = captured.TickIndex;
                    found = true;
                    break;
                }
                Require(found, "Body command has an identity absent from the current publication.");
            }
            for (int index = 0; index < dynamicEntityCount; index++)
            {
                ref DynamicEntityRecord record = ref dynamicEntities[index];
                if (!record.present && record.firstAbsentPublicationTick < 0)
                {
                    record.firstAbsentPublicationTick = publication.TickIndex;
                    report.retiredObservedEntities++;
                }
            }
            report.snapshotEntityComparisons += publication.EntityCount;
            return publication.EntityCount;
        }

        private static void ApplyDynamicInput()
        {
            Require(dynamicActor?.Runtime != null && !driver.IsPaused, "Dynamic actor/runtime stopped.");
            int tick = driver.CurrentTickIndex;
            int elapsed = tick - report.startTick;
            if (dynamicStep == 0 && elapsed >= 12)
            {
                QueueDynamicInput();
                dynamicStep = 1;
            }
            else if (dynamicStep == 1 && elapsed >= 24)
            {
                QueueDynamicInput(Key.L);
                dynamicStep = 2;
            }
            else if (dynamicStep == 2 && dynamicActor.Runtime.NativeInputProxy.ComboState[1] == 1)
            {
                QueueDynamicInput(dynamicActor.Runtime.IsFacingLeft ? Key.A : Key.D);
                dynamicStep = 3;
            }
            else if (dynamicStep == 3 && dynamicActor.Runtime.NativeInputProxy.ComboState[1] ==
                (dynamicActor.Runtime.IsFacingLeft ? 3 : 2))
            {
                QueueDynamicInput(Key.K);
                dynamicStep = 4;
            }
            else if (dynamicStep == 4 && (dynamicActor.Frame.N == 240 || dynamicActor.Frame.N == 241))
            {
                QueueDynamicInput();
                dynamicStep = 5;
            }
            else if (dynamicStep == 5 && dynamicActor.Frame.N == 253)
            {
                QueueDynamicInput(Key.J);
                dynamicStep = 6;
            }
            else if (dynamicStep == 6 && tick - dynamicLastInputTick >= 2)
            {
                QueueDynamicInput();
                dynamicStep = 7;
            }
            else if (dynamicStep >= 2 && dynamicStep <= 4 && tick - dynamicLastInputTick >= 30)
            {
                // Incomplete combo is coverage, not authority or a reason to force a frame/MP.
                QueueDynamicInput();
                dynamicStep = 8;
            }
            if (elapsed >= 160 && !dynamicInputReleased)
            {
                QueueDynamicInput();
                dynamicInputReleased = true;
            }
            report.inputSequenceStep = dynamicStep;
        }

        private static void QueueDynamicInput(params Key[] keys)
        {
            if (dynamicKeyboard != null)
                InputSystem.QueueStateEvent(dynamicKeyboard, new KeyboardState(keys));
            dynamicLastInputTick = driver?.CurrentTickIndex ?? -1;
        }

        private static void CompleteWindow()
        {
            report.frames = new FrameSample[report.sampleCount];
            if (samples != null)
                Array.Copy(samples, report.frames, report.sampleCount);
            if (report.status == "FAIL")
                return;
            BattleCentralMaterializationReport end = BattleCentralRenderSystem.Diagnostics.CaptureMaterializationReport();
            Require(end.TryCreateWindow(baseline, out BattleCentralMaterializationReport window, out string reason), reason);
            report.memoryAfter = CaptureMemory(driver.ManagedMemoryBoundary);
            if (report.dynamicInput)
            {
                Require(report.movingCameraSamples > 0, "No actual dynamic motion was observed.");
                report.dynamicEntities = new DynamicEntityRecord[dynamicEntityCount];
                Array.Copy(dynamicEntities, report.dynamicEntities, dynamicEntityCount);
                report.dynamicCoverage = report.newObservedEntities > 0 && report.retiredObservedEntities > 0
                    ? "MOTION_SPAWN_RETIRE_PUBLICATION_PASS_GPU_PIXELS_PENDING"
                    : "MOTION_PUBLICATION_PASS_SPAWN_OR_RETIRE_UNCOVERED";
            }
            if (report.sampleTiming)
            {
                Require(report.samePublicationPairs > 0 && report.interpolatedMovingCommandComparisons > 0 &&
                    report.cameraAlphaEnvelopeComparisons > 0, "No natural interpolated timing coverage.");
                report.timingCoverage = "SAME_PUBLICATION_GEOMETRY_AND_CPU_ALPHA_ENVELOPE_PASS_SCREEN_LATENCY_PENDING";
            }
            SaveNew(OutputRoot + "materialization-window-" + report.cycle.ToString("00") + ".json",
                window.ToJson());
            Require(report.observedSubmissionSlots >= 1 && report.observedSubmissionSlots <= 2 && report.sampleCount > 1,
                "No valid submission-slot observation window.");
            if (report.replayProductionCatalog)
            {
                Require(report.sampleCount == report.targetCameraFrames, "Distinct camera target not met.");
                for (int index = 0; index < report.frames.Length; index++)
                {
                    FrameSample sample = report.frames[index];
                    Require(sample.cameraExecutedDraws == sample.renderPassRecordedDraws &&
                        sample.activeFootMarkers > 0 && sample.activeHealthBars > 0 &&
                        sample.hasBoundCatalog && sample.leaseCountAfterCamera == 0,
                        "Production camera/catalog/auxiliary/CPU-lease coverage failed.");
                }
                report.catalogReplayResult = OutputRoot + "catalog-replay.json";
                BattleProductionCatalogReplayEditor.Run(BattleCentralRenderSystem.CurrentPixelFramePlan,
                    report.catalogReplayResult);
                Require(report.cameraRenderEnvelopeAllocatedBytes == 0 && report.observerAllocatedBytes == 0,
                    "Production camera/observer current-thread allocation was nonzero; evidence retained.");
            }
            if (report.diagnoseFootCoverage)
            {
                Require(report.sampleCount == report.targetCameraFrames, "Distinct diagnostic camera target not met.");
                report.footAuthoringAfter = CaptureFootAuthoring();
                int maximumSelfCommands = 0, maximumFootMarkers = 0;
                foreach (FrameSample sample in report.frames)
                {
                    Require(sample.cameraExecutedDraws == sample.renderPassRecordedDraws &&
                        sample.hasBoundCatalog && sample.leaseCountAfterCamera == 0,
                        "Diagnostic camera/catalog/CPU-lease observation failed.");
                    maximumSelfCommands = Math.Max(maximumSelfCommands, sample.selfFootCommandCount);
                    maximumFootMarkers = Math.Max(maximumFootMarkers, sample.activeFootMarkers);
                }
                report.footCoverage = ClassifyFootCoverage(report.footAuthoringAfter.authoringAvailable,
                    report.footAuthoringAfter.runtimeEnabled, report.footAuthoringAfter.runtimeSpriteAvailable,
                    maximumSelfCommands, maximumFootMarkers);
            }
            report.status = "PASS";
        }

        private static MemorySnapshot CaptureMemory(BattleManagedMemoryBoundary memory)
        {
            return new MemorySnapshot
            {
                tickBytes = memory.AllocatedBytes, driverUpdateBytes = memory.DriverUpdateAllocatedBytes,
                latePresentationBytes = memory.PresentationAllocatedBytes, playerLoopBytes = memory.PlayerLoopAllocatedBytes,
                gen0Collections = memory.Generation0Collections, gen1Collections = memory.Generation1Collections,
                gen2Collections = memory.Generation2Collections,
            };
        }

        private static int ObserveSlot(BattleCentralSubmission submission)
        {
            for (int index = 0; index < observedSlots.Length; index++)
            {
                if (ReferenceEquals(observedSlots[index], submission))
                    return index;
                if (observedSlots[index] == null)
                {
                    observedSlots[index] = submission;
                    report.observedSubmissionSlots++;
                    return index;
                }
            }
            return -1;
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static void Fail(Exception exception)
        {
            report.status = "FAIL";
            report.error = exception.ToString();
            Exit();
        }

        private static void Exit()
        {
            cameraObservationOpen = false;
            if (report.dynamicInput && dynamicKeyboard != null)
            {
                QueueDynamicInput();
                InputSystem.Update();
                dynamicKeyboard = null;
            }
            if (driver == null && EditorApplication.isPlaying)
            {
                driver = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                    .FirstOrDefault(value => value != null && value.isActiveAndEnabled && !EditorUtility.IsPersistent(value));
            }
            if (driver?.World != null)
            {
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap))
                            continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }
                report.remainingObjects = shutdown.RemainingWorldObjects;
                report.remainingSlots = shutdown.RemainingRuntimeSlots;
                report.remainingBorrowers = shutdown.RemainingPoolBorrowers;
                report.orderedShutdown = shutdown.IsComplete && driver.World == null;
                if (!report.orderedShutdown)
                {
                    report.status = "FAIL";
                    report.error += " Ordered shutdown failed: " + shutdown.FailureReason;
                    report.phase = "SHUTDOWN_FAILED";
                    SaveSession();
                    return;
                }
            }
            report.phase = "EXITING";
            SaveSession();
            EditorApplication.ExitPlaymode();
        }

        private static void OnPlay(PlayModeStateChange state)
        {
            RestoreSession();
            if (report == null)
                return;
            if (state == PlayModeStateChange.ExitingPlayMode && report.phase != "EXITING")
            {
                report.status = "FAIL";
                report.error = "Play stopped externally before the production-window probe completed.";
                report.phase = "EXITING";
                SaveSession();
            }
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
                Finish();
        }

        private static void Finish()
        {
            Scene scene = SceneManager.GetActiveScene();
            report.sceneHashAfter = HashScene();
            report.sceneClean = scene.path == ScenePath && !scene.isDirty && SceneManager.sceneCount == 1 &&
                report.sceneHashBefore == report.sceneHashAfter;
            if (!report.sceneClean)
            {
                report.status = "FAIL";
                report.error += " Saved Scene identity/dirty state changed.";
            }
            report.phase = "DONE";
            SaveNew(ResultPath(report.cycle), JsonUtility.ToJson(report, true));
            bool nextCycle = report.runNextCycle && report.status == "PASS" && report.cycle == 1;
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            samples = null;
            baseline = null;
            dynamicActor = null;
            dynamicKeyboard = null;
            dynamicEntities = null;
            dynamicEntityCount = 0;
            timingCommands = null;
            timingOffsets = null;
            timingCommandCount = 0;
            timingPublicationTick = -1;
            readBuiltAlpha = null;
            readPublicationTimestamp = null;
            readPublicationVersion = null;
            readMaterializedVersion = null;
            readFootEnabled = null;
            readFootSprite = null;
            cameraObservationOpen = false;
            Array.Clear(observedSlots, 0, observedSlots.Length);
            if (nextCycle)
                EditorApplication.delayCall += StartSecondCycle;
        }

        private static void StartSecondCycle()
        {
            RequireIdleOriginalScene();
            Require(!File.Exists(ResultPath(2)), "Second-cycle evidence exists.");
            report = new Report { cycle = 2, startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene() };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("NTSD/Validation/Optimization/Production Window Second Cycle When Idle")]
        private static void StartSecondCycleWhenIdle()
        {
            RequireIdleOriginalScene();
            RestoreSession();
            Require(report == null || (report.cycle == 2 && report.phase == "STARTUP"),
                "Refuses to interrupt an active observation or shutdown.");
            Require(File.Exists(ResultPath(1)) && !File.Exists(ResultPath(2)),
                "Requires first-cycle evidence and refuses to overwrite the second cycle.");
            Report first = JsonUtility.FromJson<Report>(File.ReadAllText(ResultPath(1)));
            Require(first.status == "PASS" && first.cycle == 1 && first.sceneClean && first.orderedShutdown,
                "First cycle must have passed with clean ordered shutdown.");
            StartSecondCycle();
        }

        private static void RequireIdleOriginalScene()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling &&
                !EditorApplication.isUpdating, "Editor must be idle.");
            MethodInfo isTestRunActive = typeof(UnityEditor.TestTools.TestRunner.Api.TestRunnerApi)
                .GetMethod("IsRunActive", BindingFlags.Static | BindingFlags.NonPublic);
            Require(isTestRunActive != null && !(bool)isTestRunActive.Invoke(null, null),
                "An actual Unity Test Runner run is still active; bridge metadata alone is insufficient.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == ScenePath && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires one saved original Battle Scene.");
        }

        private static void Require(bool condition, string reason)
        {
            if (!condition)
                throw new InvalidOperationException(reason);
        }

        private static string ResultPath(int cycle) => OutputRoot + "production-window-" + cycle.ToString("00") + ".json";

        private static void SaveNew(string path, string json)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string HashScene()
        {
            using var sha = SHA256.Create();
            using var file = File.OpenRead(ScenePath);
            return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", string.Empty);
        }

        private static void SaveSession() => SessionState.SetString(SessionKey, JsonUtility.ToJson(report));

        private static void RestoreSession()
        {
            if (report != null)
                return;
            string saved = SessionState.GetString(SessionKey, string.Empty);
            if (string.IsNullOrEmpty(saved))
                return;
            report = JsonUtility.FromJson<Report>(saved);
            if (report.phase == "OBSERVING" || report.phase == "COMPLETE")
            {
                report.status = "FAIL";
                report.error = "Unexpected domain reload interrupted a live observation window.";
                report.phase = "COMPLETE";
            }
        }
    }
}
#endif
