#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    internal static class NTSD28Q09HanEarthquakeBattlePlayProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_Q09_HanEarthquakeBattlePlay.request.json";
        private const string PhasePairRequestPath =
            "Temp/NTSD28_Q09_HanEarthquakePhasePair.request.json";
        private const string CandidateRequestPath =
            "Temp/NTSD28_Q07_D024_HanCandidateBranch.request.json";
        private const string FarCandidateRequestPath =
            "Temp/NTSD28_Q07_D024_HanFarCandidate.request.json";
        private const string MappedNearRequestPath =
            "Temp/NTSD28_Q07_D024_HanMappedNear.request.json";
        private const string MappedFarRequestPath =
            "Temp/NTSD28_Q07_D024_HanMappedFar.request.json";
        private const string InMapNearRequestPath =
            "Temp/NTSD28_Q07_D024_HanInMapNear.request.json";
        private const string InMapFarRequestPath =
            "Temp/NTSD28_Q07_D024_HanInMapFar.request.json";
        private const string StageEdgeRequestPath =
            "Temp/NTSD28_Q07_D024_ProjectStageEdge.request.json";
        private static readonly string[] MappedRequestPaths =
            { MappedNearRequestPath, MappedFarRequestPath,
              InMapNearRequestPath, InMapFarRequestPath };
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001";
        private const string CandidateResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-D024-HAN-CANDIDATE-BRANCH-001";
        private const string StageEdgeResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-D024-PROJECT-STAGE-EDGE-PLAY-001";
        private const string FormalContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const int CaptureWidth = 1024;
        private const int CaptureHeight = 576;
        private static DateTime startedUtc;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static bool running;
        private static string currentRequestFile;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public bool captureCandidateBranch;
            public int leeStartX;
            public bool sourceMappedPositions;
            public int sourceStartZ;
            public bool stageEdgeOnly;
        }

        [Serializable]
        private sealed class LegacyRequest
        {
            public bool requested;
            public string runId;
        }

        [Serializable]
        private sealed class CandidateRect
        {
            public int x1, y1, x2, y2;
        }

        [Serializable]
        private sealed class CandidateItrRect
        {
            public int kind, index;
            public CandidateRect rect;
        }

        [Serializable]
        private sealed class CandidateBranchEvidence
        {
            public string collectorMode, firstLimit;
            public int tick, collisionAction, hanSlot, leeSlot;
            public int hanSourceX, hanPhysicalX, leeSourceX, leePhysicalX;
            public int selectedCandidates, participantCount, spatialPairCount;
            public bool hanParticipantFound, leeParticipantFound, spatialPairPresent;
            public bool hanHasItrUnion, leeHasBodyUnion, unionOverlap;
            public bool hanExactAttackCache, leeExactBodyCache;
            public int kind3RectCount, kind3BodyOverlapCount;
            public bool brutePairAllowed, bruteAttackerCarrier;
            public bool bruteHasCollisionItr, bruteTargetHasBody;
            public bool bruteCoarsePass, bruteKind3ItrAllowed;
            public bool bruteKind3HitsTarget;
            public int bruteKind3BodyX;
            public string brutePredicateReadPhase, brutePostTickFirstFalsePredicate;
            public string bruteCollectionFirstFalsePredicate;
            public int bruteLiveCacheCount, bruteLiveHanCandidateCount;
            public bool bruteLiveCacheHasHan;
            public string checksumBeforeRead, checksumAfterRead;
            public CandidateRect hanItrUnion, leeBodyUnion;
            public List<CandidateItrRect> hanExactItrRects = new List<CandidateItrRect>();
            public List<CandidateRect> leeExactBodyRects = new List<CandidateRect>();
        }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int driverTick;
            public int inputPhase;
            public int hanAction;
            public int hanCollisionAction;
            public int hanSelectedCandidateCount;
            public int hanState;
            public int leeAction;
            public double hanX;
            public double hanZ;
            public double leeX;
            public double leeZ;
            public double hanRuleX;
            public double hanRuleZ;
            public double leeRuleX;
            public double leeRuleZ;
            public int hanCaughtSlot;
            public int leeCatcherSlot;
            public int runtimeOwner;
            public int runtimeX;
            public int runtimeY;
            public int frameOwner;
            public int frameX;
            public int frameY;
            public int frameTick;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string error;
            public string scenePath;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public int startTick;
            public int endTick;
            public int inputPhaseBeforePair;
            public int inputPhasePaired;
            public int hanSlot = -1;
            public int leeSlot = -1;
            public int leeStartX;
            public bool sourceMappedPositions;
            public int sourceStartZ;
            public double horizontalScale;
            public double depthScale;
            public int stageWidth;
            public int stageZMin;
            public int stageZMax;
            public double hanStartPhysicalX;
            public double hanStartPhysicalZ;
            public double leeStartPhysicalX;
            public double leeStartPhysicalZ;
            public double hanStartSourceX;
            public double hanStartSourceZ;
            public double leeStartSourceX;
            public double leeStartSourceZ;
            public bool walkableSnapshotAvailable;
            public bool hanStartWalkable;
            public bool leeStartWalkable;
            public int hanRosterSlot = -1;
            public int leeRosterSlot = -1;
            public int firstHan145 = -1;
            public int firstHan149 = -1;
            public int firstOffset2 = -1;
            public int firstReset = -1;
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
            public bool materialRestoredAfterCapture = true;
            public bool captureCameraRestored = true;
            public bool stopped;
            public int borrowersAfter = -1;
            public CandidateBranchEvidence candidateBranch;
            public CandidateBranchEvidence inCollectionCandidateBranch;
            public List<TickRow> ticks = new List<TickRow>(50);
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string requestFile = ProjectPath(RequestPath);
            string phasePairRequestFile = ProjectPath(PhasePairRequestPath);
            string candidateRequestFile = ProjectPath(CandidateRequestPath);
            string farCandidateRequestFile = ProjectPath(FarCandidateRequestPath);
            string mappedNearRequestFile = ProjectPath(MappedNearRequestPath);
            string mappedFarRequestFile = ProjectPath(MappedFarRequestPath);
            string inMapNearRequestFile = ProjectPath(InMapNearRequestPath);
            string inMapFarRequestFile = ProjectPath(InMapFarRequestPath);
            string stageEdgeRequestFile = ProjectPath(StageEdgeRequestPath);
            if (File.Exists(candidateRequestFile))
            {
                try
                {
                    Request candidate = JsonUtility.FromJson<Request>(
                        File.ReadAllText(candidateRequestFile));
                    if (candidate?.requested == true)
                        requestFile = candidateRequestFile;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (File.Exists(farCandidateRequestFile))
            {
                try
                {
                    Request farCandidate = JsonUtility.FromJson<Request>(
                        File.ReadAllText(farCandidateRequestFile));
                    if (farCandidate?.requested == true)
                        requestFile = farCandidateRequestFile;
                }
                catch (IOException)
                {
                    return;
                }
            }
            foreach (string mappedRequestPath in MappedRequestPaths)
            {
                string mappedRequestFile = ProjectPath(mappedRequestPath);
                if (!File.Exists(mappedRequestFile))
                    continue;
                try
                {
                    Request mappedRequest = JsonUtility.FromJson<Request>(
                        File.ReadAllText(mappedRequestFile));
                    if (mappedRequest?.requested == true)
                        requestFile = mappedRequestFile;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (File.Exists(stageEdgeRequestFile))
            {
                try
                {
                    Request edgeRequest = JsonUtility.FromJson<Request>(
                        File.ReadAllText(stageEdgeRequestFile));
                    if (edgeRequest?.requested == true)
                        requestFile = stageEdgeRequestFile;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (File.Exists(phasePairRequestFile))
            {
                try
                {
                    Request phasePairRequest = JsonUtility.FromJson<Request>(
                        File.ReadAllText(phasePairRequestFile));
                    if (phasePairRequest?.requested == true)
                        requestFile = phasePairRequestFile;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (!File.Exists(requestFile))
            {
                Reset();
                return;
            }

            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            }
            catch (IOException)
            {
                return;
            }
            currentRequestFile = requestFile;

            if (request == null || !request.requested)
            {
                Reset();
                return;
            }
            if (string.IsNullOrEmpty(request.runId) ||
                !request.runId.All(character => char.IsLetterOrDigit(character) ||
                                                character == '-'))
            {
                Finish(request, new Report { status = "FAIL", error = "Invalid runId." });
                return;
            }
            if ((requestFile == candidateRequestFile ||
                 requestFile == farCandidateRequestFile ||
                 requestFile == mappedNearRequestFile ||
                 requestFile == mappedFarRequestFile ||
                 requestFile == inMapNearRequestFile ||
                 requestFile == inMapFarRequestFile) &&
                !request.captureCandidateBranch)
            {
                request.captureCandidateBranch = true;
                Finish(request, new Report { status = "FAIL",
                    error = "Q07 request requires captureCandidateBranch=true." });
                return;
            }
            if (requestFile == farCandidateRequestFile && request.leeStartX != 580)
            {
                Finish(request, new Report { status = "FAIL",
                    error = "The far candidate request requires Lee X580." });
                return;
            }
            if ((requestFile == mappedNearRequestFile ||
                 requestFile == mappedFarRequestFile) &&
                (!request.sourceMappedPositions ||
                 (request.sourceStartZ != 0 && request.sourceStartZ != 650) ||
                 request.leeStartX !=
                 (requestFile == mappedNearRequestFile ? 520 : 580)))
            {
                Finish(request, new Report { status = "FAIL",
                    error = "Mapped request requires the paired source start." });
                return;
            }
            if ((requestFile == inMapNearRequestFile ||
                 requestFile == inMapFarRequestFile) &&
                (!request.sourceMappedPositions || request.sourceStartZ != 400 ||
                 request.leeStartX !=
                 (requestFile == inMapNearRequestFile ? 520 : 580)))
            {
                Finish(request, new Report { status = "FAIL",
                    error = "In-map request requires paired source Z400/X." });
                return;
            }
            if (requestFile == stageEdgeRequestFile &&
                (!request.stageEdgeOnly || request.captureCandidateBranch ||
                 !request.sourceMappedPositions ||
                 (request.sourceStartZ != 100 && request.sourceStartZ != 600) ||
                 request.leeStartX != 520))
            {
                Finish(request, new Report { status = "FAIL",
                    error = "Stage edge request requires mapped Z100/600, Lee X520 and one-tick mode." });
                return;
            }
            if (requestFile == phasePairRequestFile &&
                (request.captureCandidateBranch || request.stageEdgeOnly ||
                 !request.sourceMappedPositions || request.sourceStartZ != 400 ||
                 request.leeStartX != 520))
            {
                Finish(request, new Report { status = "FAIL",
                    error = "Phase-pair request requires source-mapped X520/Z400 natural capture." });
                return;
            }
            if (request.leeStartX != 0 && request.leeStartX != 520 &&
                request.leeStartX != 580)
            {
                Finish(request, new Report { status = "FAIL",
                    error = "Lee starting X must be the paired 520 or 580." });
                return;
            }
            if (File.Exists(ProjectPath(ResultRootFor(request) + "/" +
                                        request.runId + ".json")))
            {
                Finish(request, new Report { status = "FAIL",
                    error = "Refusing to overwrite an existing report." });
                return;
            }
            if (startedUtc == default)
                startedUtc = DateTime.UtcNow;
            if (DateTime.UtcNow - startedUtc > TimeSpan.FromMinutes(5))
            {
                Finish(request, new Report { status = "FAIL", error = "Startup timeout." });
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                Scene scene = SceneManager.GetActiveScene();
                if (scene.path != "Assets/NTSD/Scene/NTSD_Battle.unity" || scene.isDirty)
                {
                    Finish(request, new Report { status = "FAIL",
                        error = "Requires the saved clean original Battle Scene." });
                    return;
                }
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5)
                return;
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Finish(request, new Report { status = "FAIL", error =
                    driver.DedicatedSimulationWorkerFailureForDiagnostics.ToString() });
                EditorApplication.ExitPlaymode();
                return;
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

            running = true;
            WriteConsumedRequest(request);
            Run(request, driver, world);
        }

        private static void Run(Request request, SimulationTickDriver driver,
            SimulationWorld world)
        {
            BruteForceSceneQuery candidateQuery = null;
            Action previousCandidateHook = null;
            var report = new Report
            {
                runId = request.runId,
                status = "RUNNING",
                scenePath = SceneManager.GetActiveScene().path,
                sceneHashBefore = HashFile(ProjectPath(
                    "Assets/NTSD/Scene/NTSD_Battle.unity")),
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
                startTick = driver.CurrentTickIndex,
                leeStartX = request.leeStartX == 0 ? 520 : request.leeStartX,
                sourceMappedPositions = request.sourceMappedPositions,
                sourceStartZ = request.sourceStartZ == 0 ? 650 : request.sourceStartZ,
                horizontalScale = world.SpatialProjection.HorizontalScale,
                depthScale = world.SpatialProjection.DepthScale,
                stageWidth = world.Runtime.Stage.StageWidthPx,
                stageZMin = world.Runtime.Stage.ZMin,
                stageZMax = world.Runtime.Stage.ZMax,
            };
            try
            {
                Require(report.scenePath == "Assets/NTSD/Scene/NTSD_Battle.unity",
                    "Active scene changed.");
                Require(report.contentRoot == FormalContentRoot,
                    "Formal battle content root is not selected.");
                Require(world.BattleGameModeId == 0,
                    "This natural case requires battle mode 0.");
                Require(!world.OneTuInput,
                    "This natural case requires formal 2tu input cadence.");
                report.inputPhaseBeforePair = world.InputPhase;
                world.Runtime.Flow.InputPhase = 0;
                report.inputPhasePaired = world.InputPhase;

                LF2CharacterDataWrapper hanConfig =
                    world.RuntimeCharacterConfigs.Resolve(726);
                LF2CharacterDataWrapper leeConfig =
                    world.RuntimeCharacterConfigs.Resolve(7);
                Require(hanConfig?.characterData != null &&
                        leeConfig?.characterData != null,
                    "Current formal Han/Lee character definitions are unavailable.");

                report.hanSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                report.leeSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    report.hanSlot + 1, 1000);
                Require(report.hanSlot >= 50 && report.leeSlot > report.hanSlot,
                    "Two free runtime slots are required.");
                BattleSlotRuntimeState[] slots = world.Runtime.Roster.Slots;
                report.hanRosterSlot = Array.FindIndex(slots,
                    slot => slot == null || !slot.Active);
                report.leeRosterSlot = Array.FindIndex(slots,
                    report.hanRosterSlot + 1, slot => slot == null || !slot.Active);
                Require(report.hanRosterSlot >= 0 &&
                        report.leeRosterSlot > report.hanRosterSlot,
                    "Two free human roster slots are required.");

                LF2Character han = CreateCharacter(world, hanConfig, 726,
                    report.hanSlot, 3, 500, report.sourceStartZ,
                    request.sourceMappedPositions);
                LF2Character lee = CreateCharacter(world, leeConfig, 7,
                    report.leeSlot, 4, report.leeStartX, report.sourceStartZ,
                    request.sourceMappedPositions);
                report.hanStartPhysicalX = han.Runtime.X;
                report.hanStartPhysicalZ = han.Runtime.Z;
                report.leeStartPhysicalX = lee.Runtime.X;
                report.leeStartPhysicalZ = lee.Runtime.Z;
                report.hanStartSourceX = han.Runtime.SourceRuleX;
                report.hanStartSourceZ = han.Runtime.SourceRuleZ;
                report.leeStartSourceX = lee.Runtime.SourceRuleX;
                report.leeStartSourceZ = lee.Runtime.SourceRuleZ;
                bool hanWalkableSnapshotAvailable = world.TryIsGroundPixelWalkable(
                    report.hanStartPhysicalX, report.hanStartPhysicalZ,
                    out bool hanStartWalkable);
                bool leeWalkableSnapshotAvailable = world.TryIsGroundPixelWalkable(
                    report.leeStartPhysicalX, report.leeStartPhysicalZ,
                    out bool leeStartWalkable);
                report.walkableSnapshotAvailable = hanWalkableSnapshotAvailable &&
                    leeWalkableSnapshotAvailable;
                report.hanStartWalkable = hanStartWalkable;
                report.leeStartWalkable = leeStartWalkable;
                if (currentRequestFile == ProjectPath(InMapNearRequestPath) ||
                    currentRequestFile == ProjectPath(InMapFarRequestPath))
                {
                    Require(report.walkableSnapshotAvailable &&
                            report.hanStartWalkable && report.leeStartWalkable &&
                            report.hanStartPhysicalZ >= report.stageZMin &&
                            report.hanStartPhysicalZ <= report.stageZMax &&
                            report.leeStartPhysicalZ >= report.stageZMin &&
                            report.leeStartPhysicalZ <= report.stageZMax,
                        "Mapped source starts are outside the project walkable stage.");
                }
                if (request.sourceMappedPositions)
                {
                    BattleSpatialProjection projection = world.SpatialProjection;
                    Require(Math.Abs(report.hanStartPhysicalX -
                            projection.SourceToViewX(500, 0.0)) < 1e-9 &&
                            Math.Abs(report.leeStartPhysicalX -
                            projection.SourceToViewX(report.leeStartX, 0.0)) < 1e-9 &&
                            Math.Abs(report.hanStartPhysicalZ -
                            projection.SourceToViewZ(report.sourceStartZ, 0.0)) < 1e-9 &&
                            Math.Abs(report.leeStartPhysicalZ -
                            projection.SourceToViewZ(report.sourceStartZ, 0.0)) < 1e-9 &&
                            report.hanStartSourceX == 500 &&
                            report.leeStartSourceX == report.leeStartX &&
                            report.hanStartSourceZ == report.sourceStartZ &&
                            report.leeStartSourceZ == report.sourceStartZ,
                        "Mapped physical and original source starts diverged.");
                }
                slots[report.hanRosterSlot] = new BattleSlotRuntimeState
                {
                    Active = true, IsHuman = true, CharacterId = 726, Team = 3,
                    RuntimeSlotIndex = han.Runtime.SlotIndex,
                    StableId = han.Runtime.StableId,
                };
                slots[report.leeRosterSlot] = new BattleSlotRuntimeState
                {
                    Active = true, IsHuman = true, CharacterId = 7, Team = 4,
                    RuntimeSlotIndex = lee.Runtime.SlotIndex,
                    StableId = lee.Runtime.StableId,
                };

                Camera camera = NTSDRenderSpace.WorldCamera;
                Require(camera != null && camera.enabled &&
                        camera.gameObject.activeInHierarchy,
                    "Saved Battle world camera is unavailable.");
                BattleBackgroundPlatformPresentation presentation =
                    Resources.FindObjectsOfTypeAll<BattleBackgroundPlatformPresentation>()
                        .FirstOrDefault(candidate => candidate != null &&
                            !EditorUtility.IsPersistent(candidate) &&
                            candidate.gameObject.scene == SceneManager.GetActiveScene());
                Require(presentation != null,
                    "Project Map background presentation is unavailable.");
                SpriteRenderer map = presentation.GetComponent<SpriteRenderer>();
                Require(map != null && map.sprite != null,
                    "Project Map sprite is unavailable.");
                report.mapPositionBefore = map.transform.position;
                report.mapBoundsCenterBefore = map.bounds.center;
                report.cameraPositionBefore = camera.transform.position;
                report.cameraSizeBefore = camera.orthographicSize;
                Material mapMaterialBefore = map.sharedMaterial;

                if (request.captureCandidateBranch)
                {
                    candidateQuery = world.SceneQuery as BruteForceSceneQuery;
                    Require(candidateQuery != null,
                        "Q07 requires the active BruteForceSceneQuery.");
                    previousCandidateHook =
                        candidateQuery.BeforeCollisionCandidateStoreFinalCompareForSelfCheck;
                    Require(previousCandidateHook == null,
                        "Q07 cannot replace an existing candidate callback.");
                    candidateQuery.BeforeCollisionCandidateStoreFinalCompareForSelfCheck =
                        () =>
                        {
                            if (report.inCollectionCandidateBranch != null ||
                                han.Frame.Prev2 != 146)
                                return;
                            report.inCollectionCandidateBranch =
                                CaptureInCollectionCandidateBranch(
                                    candidateQuery, world, han, lee);
                        };
                }

                for (int relativeTick = 1; relativeTick <= 50; relativeTick++)
                {
                    int nextTick = driver.CurrentTickIndex + 1;
                    // Physical Attack maps to Jump and physical Jump maps to
                    // Defend in this project's frame-input contract.
                    SimulationInputButtons hanButtons = relativeTick <= 2
                        ? SimulationInputButtons.Jump
                        : relativeTick <= 4
                            ? SimulationInputButtons.Defend
                            : SimulationInputButtons.None;
                    var input = new FrameInputSet(nextTick, new[]
                    {
                        new SimulationPlayerInput(report.hanRosterSlot, hanButtons),
                        new SimulationPlayerInput(report.leeRosterSlot,
                            SimulationInputButtons.None),
                    });
                    Require(driver.StepOneTick(input, ignorePaused: true,
                        buildPresentation: true),
                        "Full Driver rejected relative tick " + relativeTick);

                    NTSD28EarthquakeRuntimeState quake = world.Runtime.Earthquake;
                    BattlePresentationFrame frame =
                        world.BattlePresentation?.PublishedFrame;
                    var row = new TickRow
                    {
                        relativeTick = relativeTick,
                        driverTick = driver.CurrentTickIndex,
                        inputPhase = world.InputPhase,
                        hanAction = han.Frame.N,
                        hanCollisionAction = han.Frame.Prev2,
                        hanSelectedCandidateCount = han.Runtime.HitCandidateCount,
                        hanState = han.GetState(),
                        leeAction = lee.Frame.N,
                        hanX = han.Runtime.X,
                        hanZ = han.Runtime.Z,
                        leeX = lee.Runtime.X,
                        leeZ = lee.Runtime.Z,
                        hanRuleX = han.Runtime.SourceRuleX,
                        hanRuleZ = han.Runtime.SourceRuleZ,
                        leeRuleX = lee.Runtime.SourceRuleX,
                        leeRuleZ = lee.Runtime.SourceRuleZ,
                        hanCaughtSlot = han.Runtime.CaughtSlotIndex,
                        leeCatcherSlot = lee.Runtime.CatcherSlotIndex,
                        runtimeOwner = quake.OwnerSlot,
                        runtimeX = quake.BackgroundOffsetX,
                        runtimeY = quake.BackgroundOffsetY,
                        frameOwner = frame?.EarthquakeOwnerSlot ?? -1,
                        frameX = frame?.EarthquakeBackgroundOffsetX ?? 0,
                        frameY = frame?.EarthquakeBackgroundOffsetY ?? 0,
                        frameTick = frame?.TickIndex ?? -1,
                    };
                    report.ticks.Add(row);
                    Require(row.frameTick == row.driverTick &&
                            row.frameOwner == row.runtimeOwner &&
                            row.frameX == row.runtimeX &&
                            row.frameY == row.runtimeY,
                        "Runtime/frozen-frame earthquake first difference at " +
                        relativeTick);
                    Require(row.inputPhase == (relativeTick & 1),
                        "The paired 2tu input phase diverged from the formal LFR.");

                    if (request.stageEdgeOnly)
                        break;

                    if (row.hanAction == 145 && report.firstHan145 < 0)
                        report.firstHan145 = relativeTick;
                    if (row.hanAction == 149 && report.firstHan149 < 0)
                        report.firstHan149 = relativeTick;
                    if (request.captureCandidateBranch &&
                        row.hanCollisionAction == 146)
                    {
                        report.candidateBranch = CaptureCandidateBranch(
                            world, han, lee, row);
                        break;
                    }
                    if (request.captureCandidateBranch)
                        continue;
                    if (relativeTick == 1)
                        report.baselinePng = CaptureMapCamera(
                            request.runId, "baseline", report, camera, map,
                            mapMaterialBefore, world);
                    if (row.runtimeX == 2 && row.runtimeY == 0 &&
                        report.firstOffset2 < 0)
                    {
                        report.firstOffset2 = relativeTick;
                        report.activePng = CaptureMapCamera(
                            request.runId, "active", report, camera, map,
                            mapMaterialBefore, world);
                    }
                    if (report.firstOffset2 > 0 && row.runtimeX == 0 &&
                        row.runtimeY == 0 && report.firstReset < 0)
                    {
                        report.firstReset = relativeTick;
                        report.resetPng = CaptureMapCamera(
                            request.runId, "reset", report, camera, map,
                            mapMaterialBefore, world);
                    }
                }

                report.endTick = driver.CurrentTickIndex;
                report.mapPositionAfter = map.transform.position;
                report.mapBoundsCenterAfter = map.bounds.center;
                report.cameraPositionAfter = camera.transform.position;
                report.cameraSizeAfter = camera.orthographicSize;
                Require(report.mapPositionBefore == report.mapPositionAfter &&
                        report.mapBoundsCenterBefore == report.mapBoundsCenterAfter &&
                        report.cameraPositionBefore == report.cameraPositionAfter &&
                        Mathf.Approximately(report.cameraSizeBefore,
                            report.cameraSizeAfter),
                    "Map geometry or fixed camera changed during the probe.");
                if (request.stageEdgeOnly)
                {
                    Require(report.ticks.Count == 1,
                        "Stage edge probe did not complete exactly one Driver tick.");
                    report.status = "OBSERVED_PROJECT_STAGE_EDGE";
                }
                else if (request.captureCandidateBranch)
                {
                    Require(report.candidateBranch != null,
                        "The first collision action146 was not observed.");
                    Require(report.inCollectionCandidateBranch != null,
                        "The action146 in-collection callback was not observed.");
                    report.status = string.IsNullOrEmpty(
                        report.candidateBranch.firstLimit)
                        ? "PASS_SCOPED_BRANCH_CAPTURE"
                        : "OBSERVED_CACHE_LIMIT";
                }
                else
                {
                    Require(report.firstHan145 > 0 && report.firstHan149 > 0 &&
                            report.firstOffset2 > 0 && report.firstReset > 0,
                        "Natural Han 145/149/earthquake/reset chain did not complete.");
                    Require(!string.IsNullOrEmpty(report.baselinePng) &&
                            !string.IsNullOrEmpty(report.activePng) &&
                            !string.IsNullOrEmpty(report.resetPng) &&
                            report.materialRestoredAfterCapture &&
                            report.captureCameraRestored,
                        "The project Map camera capture or cleanup is incomplete.");
                    report.status = "PASS_SCOPED_PLAY";
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }
            finally
            {
                if (candidateQuery != null)
                    candidateQuery.BeforeCollisionCandidateStoreFinalCompareForSelfCheck =
                        previousCandidateHook;
                try
                {
                    BattleRuntimeShutdownReport shutdown =
                        driver.ShutdownBattleRuntime();
                    bool mapCleared = true;
                    foreach (BattleBootstrap bootstrap in
                        Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap) ||
                            !bootstrap.gameObject.scene.IsValid())
                            continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    if (shutdown.RuntimeStagesCompleted)
                        shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(
                            mapCleared);
                    report.stopped = shutdown.IsComplete;
                    LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                    report.borrowersAfter = pool == null ? 0 :
                        pool.ActiveObjectCountForAcceptance +
                        pool.ActiveSpriteCountForAcceptance;
                    if (!report.stopped || report.borrowersAfter != 0)
                    {
                        report.status = "FAIL";
                        report.error += "\nOrdered shutdown or pool borrowers failed.";
                    }
                }
                catch (Exception cleanupError)
                {
                    report.status = "FAIL";
                    report.error += "\nCleanup: " + cleanupError;
                }
                report.sceneHashAfter = HashFile(ProjectPath(
                    "Assets/NTSD/Scene/NTSD_Battle.unity"));
                if (report.sceneHashAfter != report.sceneHashBefore)
                {
                    report.status = "FAIL";
                    report.error += "\nSaved Battle Scene hash changed.";
                }
                Finish(request, report);
                EditorApplication.delayCall += () =>
                {
                    if (EditorApplication.isPlaying)
                        EditorApplication.ExitPlaymode();
                };
            }
        }

        private static LF2Character CreateCharacter(SimulationWorld world,
            LF2CharacterDataWrapper config, int oid, int runtimeSlot,
            int team, int x, int z, bool sourceMappedPositions)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = oid;
            character.Name = "Q09HanQuakePlay" + oid;
            character.FrameCache.Load(config);
            character.SetRequiredRuntimeSlot(runtimeSlot);
            world.Register(character);
            character.ImmediateFrame(0);
            character.Initialize(500, 500);
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.AiControlled = false;
            character.Team = team;
            character.RelationTeam = team;
            character.OwnerEntityIndex = runtimeSlot;
            BattleSpatialProjection projection = world.SpatialProjection;
            character.Runtime.SetPosition(
                sourceMappedPositions ? projection.SourceToViewX(x, 0.0) : x,
                0,
                sourceMappedPositions ? projection.SourceToViewZ(z, 0.0) : z);
            AppManager.SyncParticipantBirthPosition(character, x, z);
            return character;
        }

        private static string CaptureMapCamera(string runId, string label,
            Report report, Camera camera, SpriteRenderer map,
            Material mapMaterialBefore, SimulationWorld world)
        {
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
            Require(plan.IsValid && !plan.IsStale &&
                    plan.SimulationTick == world.CurrentTickIndex,
                "Published camera plan is unavailable for " + label);
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
                report.materialRestoredAfterCapture &=
                    map.sharedMaterial == mapMaterialBefore;
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, CaptureHeight,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight),
                    0, 0, false);
                readback.Apply(false, false);
                string relativePath = ResultRoot + "/" + runId + "-" + label +
                                      ".png";
                string output = ProjectPath(relativePath);
                Require(!File.Exists(output),
                    "Refusing to overwrite project Map capture " + label);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, readback.EncodeToPNG());
                return relativePath;
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
                report.captureCameraRestored &=
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

        private static void Finish(Request request, Report report)
        {
            WriteConsumedRequest(request);
            if (request != null && !string.IsNullOrEmpty(request.runId) &&
                request.runId.All(character => char.IsLetterOrDigit(character) ||
                                          character == '-'))
            {
                string output = ProjectPath(ResultRootFor(request) + "/" + request.runId +
                                            ".json");
                if (!File.Exists(output))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(output));
                    File.WriteAllText(output, JsonUtility.ToJson(report, true));
                }
            }
            Reset();
        }

        private static string ResultRootFor(Request request) =>
            request?.stageEdgeOnly == true ? StageEdgeResultRoot :
            request?.captureCandidateBranch == true ? CandidateResultRoot : ResultRoot;

        private static void WriteConsumedRequest(Request request)
        {
            bool branch = request?.captureCandidateBranch == true;
            File.WriteAllText(currentRequestFile ?? ProjectPath(
                    branch ? CandidateRequestPath : RequestPath),
                branch
                    ? JsonUtility.ToJson(new Request
                    {
                        requested = false,
                        runId = request.runId,
                        captureCandidateBranch = true,
                        leeStartX = request.leeStartX,
                        sourceMappedPositions = request.sourceMappedPositions,
                        sourceStartZ = request.sourceStartZ,
                    })
                    : JsonUtility.ToJson(new LegacyRequest
                    {
                        requested = false,
                        runId = request?.runId,
                    }));
        }

        private static CandidateBranchEvidence CaptureCandidateBranch(
            SimulationWorld world, LF2Character han, LF2Character lee, TickRow row)
        {
            var evidence = new CandidateBranchEvidence
            {
                tick = row.driverTick,
                collisionAction = row.hanCollisionAction,
                hanSlot = han.Runtime.SlotIndex,
                leeSlot = lee.Runtime.SlotIndex,
                hanSourceX = (int)han.Runtime.SourceRuleX,
                hanPhysicalX = han.Runtime.XInt,
                leeSourceX = (int)lee.Runtime.SourceRuleX,
                leePhysicalX = lee.Runtime.XInt,
                selectedCandidates = han.Runtime.HitCandidateCount,
            };
            BruteForceSceneQuery query = world.SceneQuery as BruteForceSceneQuery;
            if (query == null)
            {
                evidence.firstLimit = "The active SceneQuery is not BruteForceSceneQuery.";
                return evidence;
            }
            evidence.collectorMode = query.LastFormalCollectorModeForDiagnostics.ToString();
            if (query.LastFormalCollectorModeForDiagnostics ==
                CollisionFormalCollectorMode.ForceBruteForce)
            {
                CaptureBruteCandidateBranch(query, world, han, lee, evidence);
                return evidence;
            }
            if (query.LastFormalCollectorModeForDiagnostics !=
                CollisionFormalCollectorMode.ForceRoleAware)
            {
                evidence.firstLimit = "The active collector did not use RoleAware caches.";
                return evidence;
            }

            object buffer = ReadMember(query, "_roleFormalParticipants");
            evidence.participantCount = (int)ReadMember(buffer, "Count");
            Array participants = ReadMember(buffer, "items") as Array;
            if (participants == null || evidence.participantCount > participants.Length)
            {
                evidence.firstLimit = "RoleAware participant buffer is unavailable.";
                return evidence;
            }

            int hanOrdinal = -1;
            int leeOrdinal = -1;
            for (int index = 0; index < evidence.participantCount; index++)
            {
                object participant = participants.GetValue(index);
                LF2Entity entity = ReadMember(participant, "Entity") as LF2Entity;
                if (ReferenceEquals(entity, han)) hanOrdinal = index;
                if (ReferenceEquals(entity, lee)) leeOrdinal = index;
            }
            evidence.hanParticipantFound = hanOrdinal >= 0;
            evidence.leeParticipantFound = leeOrdinal >= 0;
            if (!evidence.hanParticipantFound || !evidence.leeParticipantFound)
            {
                evidence.firstLimit = "The current RoleAware cache lacks Han or Lee.";
                return evidence;
            }

            var pairKeys = ReadMember(query, "_formalAuthorityPairKeys") as
                System.Collections.IList;
            if (pairKeys == null)
            {
                evidence.firstLimit = "The current spatial pair list is unavailable.";
                return evidence;
            }
            evidence.spatialPairCount = pairKeys.Count;
            long expectedPair = ((long)Math.Min(hanOrdinal, leeOrdinal) << 32) |
                                (uint)Math.Max(hanOrdinal, leeOrdinal);
            for (int index = 0; index < pairKeys.Count; index++)
            {
                if ((long)pairKeys[index] == expectedPair)
                    evidence.spatialPairPresent = true;
            }

            object hanParticipant = participants.GetValue(hanOrdinal);
            object leeParticipant = participants.GetValue(leeOrdinal);
            evidence.hanHasItrUnion = (bool)ReadMember(hanParticipant,
                "HasOrdinaryItrUnion");
            evidence.leeHasBodyUnion = (bool)ReadMember(leeParticipant,
                "HasBodyUnion");
            evidence.hanExactAttackCache = (bool)ReadMember(hanParticipant,
                "HasExactAttackCache");
            evidence.leeExactBodyCache = (bool)ReadMember(leeParticipant,
                "HasExactBodyCache");
            if (evidence.hanHasItrUnion)
                evidence.hanItrUnion = ReadRect(ReadMember(hanParticipant,
                    "OrdinaryItrUnionWorld"));
            if (evidence.leeHasBodyUnion)
                evidence.leeBodyUnion = ReadRect(ReadMember(leeParticipant,
                    "BodyUnionWorld"));
            evidence.unionOverlap = RectsOverlap(evidence.hanItrUnion,
                evidence.leeBodyUnion);

            if (!evidence.hanExactAttackCache || !evidence.leeExactBodyCache)
                return evidence;
            var itrRects = ReadMember(query, "_roleFormalExactItrRects") as
                System.Collections.IList;
            var bodyRects = ReadMember(query, "_roleFormalExactBodyRects") as
                System.Collections.IList;
            if (itrRects == null || bodyRects == null)
            {
                evidence.firstLimit = "The exact rectangle caches are unavailable.";
                return evidence;
            }
            int itrStart = (int)ReadMember(hanParticipant, "ExactItrRectOffset");
            int itrCount = (int)ReadMember(hanParticipant, "ExactItrRectCount");
            int bodyStart = (int)ReadMember(leeParticipant, "ExactBodyRectOffset");
            int bodyCount = (int)ReadMember(leeParticipant, "ExactBodyRectCount");
            if (itrStart < 0 || itrCount < 0 || itrStart + itrCount > itrRects.Count ||
                bodyStart < 0 || bodyCount < 0 || bodyStart + bodyCount > bodyRects.Count)
            {
                evidence.firstLimit = "The exact rectangle cache ranges are invalid.";
                return evidence;
            }
            for (int index = bodyStart; index < bodyStart + bodyCount; index++)
            {
                evidence.leeExactBodyRects.Add(ReadRect(ReadMember(
                    bodyRects[index], "WorldRect")));
            }
            for (int index = itrStart; index < itrStart + itrCount; index++)
            {
                object entry = itrRects[index];
                InteractionArea itr = ReadMember(entry, "Itr") as InteractionArea;
                var rect = new CandidateItrRect
                {
                    kind = itr?.kind ?? -1,
                    index = (int)ReadMember(entry, "ItrIndex"),
                    rect = ReadRect(ReadMember(entry, "WorldRect")),
                };
                evidence.hanExactItrRects.Add(rect);
                if (rect.kind != 3) continue;
                evidence.kind3RectCount++;
                foreach (CandidateRect body in evidence.leeExactBodyRects)
                {
                    if (RectsOverlap(rect.rect, body))
                        evidence.kind3BodyOverlapCount++;
                }
            }
            return evidence;
        }

        private static CandidateBranchEvidence CaptureInCollectionCandidateBranch(
            BruteForceSceneQuery query, SimulationWorld world,
            LF2Character han, LF2Character lee)
        {
            var evidence = new CandidateBranchEvidence
            {
                tick = world.CurrentTickIndex,
                collisionAction = han.Frame.Prev2,
                collectorMode = query.LastFormalCollectorModeForDiagnostics.ToString(),
                hanSlot = han.Runtime.SlotIndex,
                leeSlot = lee.Runtime.SlotIndex,
                hanSourceX = (int)han.Runtime.SourceRuleX,
                hanPhysicalX = han.Runtime.XInt,
                leeSourceX = (int)lee.Runtime.SourceRuleX,
                leePhysicalX = lee.Runtime.XInt,
            };
            if (query.LastFormalCollectorModeForDiagnostics !=
                CollisionFormalCollectorMode.ForceBruteForce)
            {
                evidence.firstLimit = "Action146 did not use the brute collector.";
                return evidence;
            }
            var cache = ReadMember(query, "_candidateCache") as
                System.Collections.IDictionary;
            if (cache == null)
            {
                evidence.firstLimit = "The live candidate cache is unavailable.";
                return evidence;
            }
            evidence.bruteLiveCacheCount = cache.Count;
            evidence.bruteLiveCacheHasHan = cache.Contains(han);
            if (evidence.bruteLiveCacheHasHan)
                evidence.bruteLiveHanCandidateCount =
                    (cache[han] as System.Collections.IList)?.Count ?? -1;
            CaptureBruteCandidateBranch(query, world, han, lee, evidence,
                afterTick: false);
            return evidence;
        }

        private static void CaptureBruteCandidateBranch(
            BruteForceSceneQuery query, SimulationWorld world,
            LF2Character han, LF2Character lee, CandidateBranchEvidence evidence,
            bool afterTick = true)
        {
            if (afterTick)
                evidence.checksumBeforeRead = world.CaptureParityFrameSnapshot(
                    evidence.tick).OverallChecksum;
            evidence.brutePairAllowed = (bool)InvokeQueryMethod(query,
                "CandidateCollectionPairAllowed",
                new[] { typeof(LF2Entity), typeof(LF2Entity) }, han, lee);
            LF2FrameData hanCurrent = (LF2FrameData)InvokeQueryMethod(query,
                "GetAuthoredCurrentFrame", new[] { typeof(LF2Entity) }, han);
            LF2FrameData leeCurrent = (LF2FrameData)InvokeQueryMethod(query,
                "GetAuthoredCurrentFrame", new[] { typeof(LF2Entity) }, lee);
            LF2FrameData hanCollision = han.GetCollisionFrameData();
            LF2FrameData leeCollision = lee.GetCollisionFrameData();
            evidence.bruteHasCollisionItr = hanCollision?.itrs?.Count > 0;
            evidence.bruteAttackerCarrier = (bool)InvokeQueryMethod(query,
                "IsCandidateAttackerCarrierForCurrentTick",
                new[] { typeof(LF2Entity) }, han);
            evidence.bruteTargetHasBody = (bool)InvokeQueryMethod(query,
                "HasAnyReleaseBody", new[] { typeof(LF2FrameData) },
                leeCollision);
            if (evidence.bruteHasCollisionItr && evidence.bruteTargetHasBody)
            {
                evidence.bruteCoarsePass = (bool)InvokeQueryMethod(query,
                    "PassesReleaseCoarsePrefilter",
                    new[] { typeof(LF2Entity), typeof(LF2FrameData),
                            typeof(LF2FrameData), typeof(LF2Entity),
                            typeof(LF2FrameData), typeof(LF2FrameData) },
                    han, hanCurrent, hanCollision, lee, leeCurrent, leeCollision);
                InteractionArea grab = hanCollision.itrs.FirstOrDefault(
                    itr => itr?.kind == 3);
                if (grab != null)
                {
                    evidence.kind3RectCount = 1;
                    evidence.bruteKind3ItrAllowed = (bool)InvokeQueryMethod(query,
                        "ItrAllowedForFormalCollection",
                        new[] { typeof(LF2Entity), typeof(LF2FrameData),
                                typeof(LF2FrameData), typeof(InteractionArea),
                                typeof(LF2Entity), typeof(LF2FrameData) },
                        han, hanCurrent, hanCollision, grab, lee, leeCurrent);
                    object[] hitArguments =
                    {
                        han, hanCollision, grab, lee, leeCollision, 0,
                    };
                    evidence.bruteKind3HitsTarget = (bool)InvokeQueryMethod(
                        query, "HitsTarget",
                        new[] { typeof(LF2Entity), typeof(LF2FrameData),
                                typeof(InteractionArea), typeof(LF2Entity),
                                typeof(LF2FrameData), typeof(int).MakeByRefType() },
                        hitArguments);
                    evidence.bruteKind3BodyX = (int)hitArguments[5];
                }
            }

            evidence.brutePredicateReadPhase = afterTick
                ? "AfterCandidateConsumption_PostTickReevaluation"
                : "AfterCandidateCollection_BeforeFinalCompare";
            string firstFalsePredicate = !evidence.brutePairAllowed
                ? "PairAllowed" : !evidence.bruteHasCollisionItr
                ? "CollisionItr" : !evidence.bruteAttackerCarrier
                ? "AttackerCarrier" : !evidence.bruteTargetHasBody
                ? "TargetBody" : !evidence.bruteCoarsePass
                ? "CoarsePrefilter" : evidence.kind3RectCount == 0
                ? "Kind3Missing" : !evidence.bruteKind3ItrAllowed
                ? "Kind3ItrAllowed" : !evidence.bruteKind3HitsTarget
                ? "Kind3HitsTarget" : "LaterOrUnobserved";
            if (afterTick)
            {
                evidence.brutePostTickFirstFalsePredicate = firstFalsePredicate;
                evidence.checksumAfterRead = world.CaptureParityFrameSnapshot(
                    evidence.tick).OverallChecksum;
                evidence.firstLimit = "Post-tick predicates cannot identify the in-collector first rejection; carrier/cache may already be cleared.";
                if (evidence.checksumBeforeRead != evidence.checksumAfterRead)
                    evidence.firstLimit = "Diagnostic predicate reads changed World checksum.";
            }
            else
            {
                evidence.bruteCollectionFirstFalsePredicate =
                    firstFalsePredicate;
                evidence.firstLimit = "After-collection predicate re-evaluation is not a per-return trace.";
            }
        }

        private static object InvokeQueryMethod(
            BruteForceSceneQuery query, string name, Type[] parameters,
            params object[] arguments)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                                       BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo method = typeof(BruteForceSceneQuery).GetMethod(
                name, flags, null, parameters, null);
            if (method == null)
                throw new InvalidOperationException("Missing diagnostic predicate " + name);
            return method.Invoke(method.IsStatic ? null : query, arguments);
        }

        private static object ReadMember(object target, string name)
        {
            if (target == null)
                throw new InvalidOperationException("Null diagnostic cache member " + name);
            const BindingFlags flags = BindingFlags.Instance |
                                       BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();
            FieldInfo field = type.GetField(name, flags);
            if (field != null) return field.GetValue(target);
            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null) return property.GetValue(target);
            throw new InvalidOperationException("Missing diagnostic cache member " +
                                                type.Name + "." + name);
        }

        private static CandidateRect ReadRect(object rectangle) => new CandidateRect
        {
            x1 = (int)ReadMember(rectangle, "X1"),
            y1 = (int)ReadMember(rectangle, "Y1"),
            x2 = (int)ReadMember(rectangle, "X2"),
            y2 = (int)ReadMember(rectangle, "Y2"),
        };

        private static bool RectsOverlap(CandidateRect a, CandidateRect b) =>
            a != null && b != null && a.x1 < b.x2 && a.x2 > b.x1 &&
            a.y1 < b.y2 && a.y2 > b.y1;

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, relativePath));
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
            running = false;
            startedUtc = default;
            stableTick = -1;
            stableUpdates = 0;
            currentRequestFile = null;
        }
    }
}
#endif
