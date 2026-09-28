#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
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
    [InitializeOnLoad]
    internal static class NTSD28Q09KarinState9997BattlePlayProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string GameConfigAssetPath =
            "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string RequestPath =
            "Temp/NTSD28_Q09_Karin9997_20260928_03.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001";
        private const string PixelResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P12-KARIN-LEGACY-GPU-PIXEL-001";
        private const int FixtureSlot = 8;
        private const int MaxTicks = 8;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public int sourceX;
            public int fixtureSlot;
            public bool checkLegacy;
            public bool legacyBoot;
            public bool captureLegacyPixels;
            public long startedUtcTicks;
        }

        [Serializable]
        private sealed class TickRecord
        {
            public int step;
            public int worldTick;
            public int actorAction;
            public int childSlot = -1;
            public int childAction = -1;
            public int childState = -1;
            public int childOwner = -1;
            public string childDir;
            public int publishedTick = -1;
            public bool childPublished;
            public bool bodyCommand;
        }

        [Serializable]
        private sealed class Report
        {
            public string status = "FAIL";
            public string error;
            public string runId;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public string backendMode;
            public bool configIsRuntimeClone;
            public bool logicOnlyMaterialization;
            public bool dedicatedWorkerActive;
            public int assetEtcMode = -1;
            public int catalogEtcMode = -1;
            public string assetModeFingerprint;
            public string catalogModeFingerprint;
            public float cameraAspect;
            public float cameraOrthographicSize;
            public float cameraWorldLeft;
            public float cameraWorldRight;
            public float presentationWorldLeft;
            public float presentationUnitsPerPixelX;
            public float cameraVisibleLeftPixels;
            public float cameraVisibleRightPixels;
            public bool hasWalkableBounds;
            public float walkableWorldLeft;
            public float walkableWorldWidth;
            public int sourceX;
            public double physicalX;
            public double physicalZ;
            public double sourceZ;
            public double viewXScale;
            public int fixtureSlot;
            public int fixtureOwner = -1;
            public int childSlot = -1;
            public int childOid = -1;
            public int childAction = -1;
            public int childState = -1;
            public int childOwner = -1;
            public string childDir;
            public int childPic = -1;
            public bool snapshotFlipX;
            public bool commandFlipX;
            public float commandX;
            public float commandY;
            public int commandTick = -1;
            public int commandCount;
            public bool commandSubmitted;
            public bool legacyObserved;
            public bool legacySpriteEnabled;
            public bool legacyFlipX;
            public float legacyX;
            public float legacyY;
            public int pixelCaptureWidth;
            public int pixelCaptureHeight;
            public int legacyBodyPixelCount;
            public int legacyBodyPixelMinX = -1;
            public int legacyBodyPixelMinY = -1;
            public int legacyBodyPixelMaxX = -1;
            public int legacyBodyPixelMaxY = -1;
            public int pixelTickBefore = -1;
            public int pixelTickAfter = -1;
            public bool pixelCameraRestored;
            public bool pixelBodyRestored;
            public string legacyBodyOnPng;
            public string legacyBodyOffPng;
            public float legacyDeltaX;
            public float legacyDeltaY;
            public float fallbackExpectedPivotX;
            public float fallbackObservedPivotX;
            public float fallbackPixelDifference;
            public bool backendRestored;
            public int objectsBefore;
            public int objectsAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int borrowersBefore;
            public int borrowersAfter;
            public bool fixtureReleased;
            public bool childReleased;
            public bool pauseRestored;
            public List<TickRecord> ticks = new List<TickRecord>();
        }

        private static Request request;
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character fixture;
        private static LF2Entity child;
        private static int phase;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int expectedTick = -1;
        private static int stepped;
        private static bool savedPaused;
        private static bool pauseCaptured;
        private static double deadline;
        private static int ActiveFixtureSlot => request?.fixtureSlot == 9 ? 9 : FixtureSlot;
        private static BattlePresentationBackendMode originalBackend;
        private static bool backendSwitched;

        static NTSD28Q09KarinState9997BattlePlayProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged -= RestoreProbeConfigAfterPlay;
            EditorApplication.playModeStateChanged += RestoreProbeConfigAfterPlay;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureOptInLegacyBeforeSceneLoad()
        {
            string path = ProjectPath(RequestPath);
            if (!File.Exists(path))
                return;
            Request pending;
            try
            {
                pending = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            }
            catch (Exception)
            {
                return;
            }
            if (pending == null || !pending.requested || !ValidRunId(pending.runId) ||
                File.Exists(ResultPath(pending.runId)))
                return;
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(
                GameConfigAssetPath);
            if (source == null)
                return;
            GameConfig current = GameConfig.Instance;
            if (current != null && current != source)
            {
                if (!IsProbeConfigClone(current, source))
                    return;
                SetGameConfigInstance(null);
                UnityEngine.Object.DestroyImmediate(current);
            }
            if (!pending.legacyBoot)
            {
                if (GameConfig.Instance == null)
                    GameConfig.Instance = source;
                return;
            }
            if (GameConfig.Instance == source)
                SetGameConfigInstance(null);
            GameConfig copy = UnityEngine.Object.Instantiate(source);
            copy.hideFlags = HideFlags.DontSave;
            copy.BattlePresentationBackendName =
                nameof(BattlePresentationBackendMode.LegacyOnly);
            GameConfig.Instance = copy;
        }

        private static void RestoreProbeConfigAfterPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(
                GameConfigAssetPath);
            GameConfig current = GameConfig.Instance;
            if (!IsProbeConfigClone(current, source))
                return;
            SetGameConfigInstance(null);
            GameConfig.Instance = source;
            UnityEngine.Object.DestroyImmediate(current);
        }

        private static bool IsProbeConfigClone(GameConfig current,
            GameConfig source)
        {
            return current != null && source != null &&
                   !AssetDatabase.Contains(current) &&
                   current.name == source.name + "(Clone)" &&
                   (current.hideFlags & HideFlags.DontSave) == HideFlags.DontSave &&
                   current.BattlePresentationBackendName ==
                       nameof(BattlePresentationBackendMode.LegacyOnly) &&
                   current.BattleContentRuntimeRoot == source.BattleContentRuntimeRoot;
        }

        private static void SetGameConfigInstance(GameConfig value)
        {
            FieldInfo field = typeof(GameConfig).GetField("_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(field != null, "GameConfig singleton backing field changed.");
            field.SetValue(null, value);
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(ProjectPath(RequestPath)))
                return;
            Request next;
            try
            {
                next = JsonUtility.FromJson<Request>(
                    File.ReadAllText(ProjectPath(RequestPath)));
            }
            catch (IOException)
            {
                return;
            }
            if (next == null || (!next.requested && !next.running))
                return;
            if (!ValidRunId(next.runId) || (next.sourceX != 20 && next.sourceX != 500) ||
                (next.fixtureSlot != 0 && next.fixtureSlot != 9) ||
                File.Exists(ResultPath(next.runId)))
                return;
            if (!EditorApplication.isPlaying)
            {
                if (!next.requested || next.running ||
                    EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (scene.path != ScenePath || scene.isDirty)
                {
                    WriteImmediateFailure(next, "A clean saved Battle Scene is required.");
                    return;
                }
                EditorApplication.EnterPlaymode();
                return;
            }

            if (request == null)
            {
                request = next;
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
                report = new Report
                {
                    runId = request.runId,
                    sourceX = request.sourceX,
                    fixtureSlot = ActiveFixtureSlot,
                };
                deadline = EditorApplication.timeSinceStartup + 180.0;
            }
            try
            {
                Require(EditorApplication.timeSinceStartup < deadline,
                    "Battle Play probe timed out.");
                if (phase == 0)
                    Prepare();
                else if (phase == 1)
                    WaitForPauseAndSpawn();
                else
                    StepAndObserve();
            }
            catch (Exception error)
            {
                report.error = error.ToString();
                Finish();
            }
        }

        private static void Prepare()
        {
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 ||
                !world.IsBattleSnapshotBoundaryReady)
                return;
            Require(SceneManager.GetActiveScene().path == ScenePath,
                "Battle Scene changed during Play.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == FormalRoot,
                "Formal content root is not selected.");
            BattlePresentationBackendMode expectedBackend = request.legacyBoot
                ? BattlePresentationBackendMode.LegacyOnly
                : BattlePresentationBackendMode.CentralOnly;
            report.backendMode = driver.PresentationBackendMode.ToString();
            report.configIsRuntimeClone = GameConfig.Instance != null &&
                !AssetDatabase.Contains(GameConfig.Instance);
            report.logicOnlyMaterialization = world.UsesLogicOnlyEntityMaterialization;
            report.dedicatedWorkerActive =
                driver.DedicatedSimulationWorkerActiveForDiagnostics;
            Require(driver.PresentationBackendMode == expectedBackend &&
                    world.BattlePresentation.Mode == expectedBackend,
                "Requested Battle presentation backend was not selected before preparation.");
            Require(!request.captureLegacyPixels ||
                    (request.checkLegacy && request.legacyBoot),
                "Legacy GPU capture requires pre-boot LegacyOnly mode.");
            if (request.legacyBoot)
                Require(report.configIsRuntimeClone &&
                        !report.logicOnlyMaterialization &&
                        !report.dedicatedWorkerActive,
                    "Legacy boot must use an in-memory config and the non-worker renderer path.");
            Require(CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                "Formal content publication is not ready.");
            Require(world.BattleGameModeId == 0, "Selected battle mode is not 0.");
            ProjectBattleModeConfig.Snapshot assetMode =
                ProjectBattleModeConfig.LoadDefault().Capture();
            ProjectBattleModeConfig.Snapshot catalogMode =
                world.RuntimeDataCatalog.ProjectModeSnapshot;
            Require(catalogMode != null, "Production Battle catalog has no project mode Snapshot.");
            report.assetEtcMode = assetMode.SelectedModeEtcMode;
            report.catalogEtcMode = catalogMode.SelectedModeEtcMode;
            report.assetModeFingerprint = assetMode.Fingerprint;
            report.catalogModeFingerprint = catalogMode.Fingerprint;
            Require(report.assetEtcMode == 1 && report.catalogEtcMode == 1 &&
                string.Equals(report.assetModeFingerprint,
                    report.catalogModeFingerprint, StringComparison.Ordinal),
                "Selected project etc-mode did not reach the production Battle catalog.");
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.orthographic,
                "An active orthographic Battle World camera is required.");
            NTSDRenderSpace.ViewportTransformSnapshot viewport =
                NTSDRenderSpace.CaptureViewportTransform();
            report.cameraAspect = camera.aspect;
            report.cameraOrthographicSize = camera.orthographicSize;
            report.cameraWorldLeft = camera.transform.position.x -
                camera.orthographicSize * camera.aspect;
            report.cameraWorldRight = camera.transform.position.x +
                camera.orthographicSize * camera.aspect;
            report.presentationWorldLeft = viewport.Left;
            report.presentationUnitsPerPixelX = viewport.UnitsPerPixelX;
            report.cameraVisibleLeftPixels =
                (report.cameraWorldLeft - viewport.Left) / viewport.UnitsPerPixelX;
            report.cameraVisibleRightPixels =
                (report.cameraWorldRight - viewport.Left) / viewport.UnitsPerPixelX;
            report.hasWalkableBounds = NTSDRenderSpace.TryGetStageWorldBounds(out Rect bounds);
            if (report.hasWalkableBounds)
            {
                report.walkableWorldLeft = bounds.xMin;
                report.walkableWorldWidth = bounds.width;
            }
            savedPaused = driver.IsPaused;
            pauseCaptured = true;
            driver.SetPaused(true);
            phase = 1;
        }

        private static void WaitForPauseAndSpawn()
        {
            Require(ReferenceEquals(world, driver.World), "Production World changed.");
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                "Dedicated worker failed.");
            if (!driver.IsPaused || driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 4)
                return;
            report.sceneHashBefore = HashFile(ProjectPath(ScenePath));
            report.objectsBefore = world.ObjectCount;
            report.slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.borrowersBefore = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            Require(world.FindEntityByRuntimeSlotForQuery(ActiveFixtureSlot) == null,
                "Karin fixture slot is occupied: " + ActiveFixtureSlot);
            LF2CharacterDataWrapper data = world.RuntimeCharacterConfigs.Resolve(77);
            Require(data?.characterData != null, "Formal Karin OID77 is unavailable.");
            fixture = new LF2Character();
            fixture.ModuleInitialize();
            fixture.ObjectId = 77;
            fixture.Name = "Q09KarinState9997";
            fixture.FrameCache.Load(data);
            fixture.SetRequiredRuntimeSlot(ActiveFixtureSlot);
            world.Register(fixture);
            fixture.ImmediateFrame(415);
            fixture.Initialize(500, 500);
            fixture.OwnerEntityIndex = ActiveFixtureSlot;
            fixture.AiControlled = false;
            fixture.Team = 1;
            fixture.RelationTeam = 1;
            fixture.SwitchDir("left");
            report.viewXScale = world.FixedViewRunDistanceScale;
            report.physicalX = request.sourceX * report.viewXScale;
            report.sourceZ = 350;
            report.physicalZ = Math.Max(world.Runtime.Stage.ZMin + 20,
                Math.Min(world.Runtime.Stage.ZMax - 20,
                    report.sourceZ * world.FixedViewRunVerticalDistanceScale));
            fixture.Runtime.SetPosition(report.physicalX, 0, report.physicalZ);
            fixture.Runtime.SetSourceRulePosition(request.sourceX, report.sourceZ);
            fixture.Runtime.SetVelocity(0, 0, 0);
            fixture.Runtime.SyncIntegerPosition();
            fixture.Runtime.SyncSourceRuleIntegerPosition();
            fixture.RefreshRuntimeSnapshot();
            report.fixtureOwner = fixture.Runtime.OwnerSlotIndex;
            Require(fixture.Frame.N == 415 && fixture.Runtime.Dir == "left" &&
                fixture.Runtime.SlotIndex == ActiveFixtureSlot &&
                report.fixtureOwner == ActiveFixtureSlot,
                "Controlled Karin initial frame/facing/slot/owner did not hold.");
            phase = 2;
        }

        private static void StepAndObserve()
        {
            Require(driver.IsPaused && ReferenceEquals(world, driver.World),
                "Paused production World changed.");
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                "Dedicated worker failed during the complete tick.");
            if (expectedTick < 0)
            {
                if (stepped >= MaxTicks)
                {
                    report.status = "CHILD_OR_COMMAND_NOT_OBSERVED";
                    report.error = "No OID314/state9997 body within eight full ticks.";
                    Finish();
                    return;
                }
                expectedTick = driver.CurrentTickIndex + 1;
                bool accepted = driver.DedicatedSimulationWorkerActiveForDiagnostics
                    ? driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(true)
                    : driver.StepOneTick(ignorePaused: true, buildPresentation: true);
                Require(accepted, "Production Driver rejected the diagnostic tick: " +
                    driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
                return;
            }
            if (driver.CurrentTickIndex < expectedTick ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            Require(driver.CurrentTickIndex == expectedTick,
                "Production Driver advanced more than one requested tick.");
            stepped++;
            TickRecord row = new TickRecord
            {
                step = stepped,
                worldTick = expectedTick,
                actorAction = fixture.Frame.N,
            };
            for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                if (candidate?.ObjectId != 314)
                    continue;
                child = candidate;
                row.childSlot = slot;
                row.childAction = child.Frame.N;
                row.childState = (int)(child.Frame.D?.state ?? -1);
                row.childOwner = child.Runtime.OwnerSlotIndex;
                row.childDir = child.Runtime.Dir;
                report.childSlot = row.childSlot;
                report.childOid = child.ObjectId;
                report.childAction = row.childAction;
                report.childState = row.childState;
                report.childOwner = row.childOwner;
                report.childDir = row.childDir;
                report.childPic = child.GetRenderPicIndex();
                break;
            }
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            row.publishedTick = published?.TickIndex ?? -1;
            if (child != null && published != null)
            {
                for (int rank = 0; rank < published.EntityCount; rank++)
                {
                    BattlePresentationEntitySnapshot entity = published.GetEntity(rank);
                    if (entity.RuntimeSlot != child.Runtime.SlotIndex ||
                        entity.ObjectId != 314)
                        continue;
                    row.childPublished = true;
                    report.snapshotFlipX = entity.FlipX;
                    if (ActiveFixtureSlot == 9)
                        report.fallbackExpectedPivotX =
                            ComputeOwner9FallbackPivotX(entity, child,
                                published.TickIndex);
                    break;
                }
                if (!request.legacyBoot)
                {
                    BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
                    BattlePresentationFrame commandFrame = plan.CapturedFrame;
                    if (plan.IsValid && !plan.IsStale && commandFrame != null &&
                        commandFrame.CommandsMaterialized)
                    {
                        for (int index = 0; index < commandFrame.CommandCount; index++)
                        {
                            BattleRenderCommand command = commandFrame.GetCommand(index);
                            if (command.RuntimeSlot != child.Runtime.SlotIndex ||
                                command.Type != BattleRenderCommandType.Entity)
                                continue;
                            row.bodyCommand = true;
                            report.childSlot = child.Runtime.SlotIndex;
                            report.childOid = child.ObjectId;
                            report.childAction = child.Frame.N;
                            report.childState = (int)(child.Frame.D?.state ?? -1);
                            report.childOwner = child.Runtime.OwnerSlotIndex;
                            report.childDir = child.Runtime.Dir;
                            report.childPic = child.GetRenderPicIndex();
                            report.commandFlipX = command.FlipX;
                            report.commandX = command.Position.x;
                            report.commandY = command.Position.y;
                            if (ActiveFixtureSlot == 9)
                            {
                                report.fallbackObservedPivotX =
                                    (command.Position.x - report.presentationWorldLeft) /
                                    report.presentationUnitsPerPixelX;
                                report.fallbackPixelDifference = Mathf.Abs(
                                    report.fallbackObservedPivotX -
                                    report.fallbackExpectedPivotX);
                            }
                            report.commandTick = commandFrame.TickIndex;
                            report.commandCount = commandFrame.CommandCount;
                            report.commandSubmitted = plan.Submission != null;
                            break;
                        }
                    }
                }
            }
            report.ticks.Add(row);
            if (request.legacyBoot && child != null && row.childState == 9997)
            {
                ObserveLegacyBody();
                Finish();
                return;
            }
            if (row.bodyCommand && report.childState == 9997)
            {
                if (request.checkLegacy)
                    ObserveLegacyBody();
                else
                    report.status = report.childOwner != ActiveFixtureSlot
                        ? "OWNER_FIRST_DIFFERENCE"
                        : ActiveFixtureSlot == 9
                            ? report.snapshotFlipX && report.commandFlipX &&
                              report.fallbackPixelDifference <= 0.75f
                                ? "FALLBACK_OWNER9_COMMAND_MATCH"
                                : "FALLBACK_OWNER9_COMMAND_DIFFERENCE"
                        : report.commandFlipX
                            ? "FIRST_DIFFERENCE_COMMAND_FACING"
                            : "NO_FACING_DIFFERENCE";
                Finish();
                return;
            }
            expectedTick = -1;
        }

        private static void ObserveLegacyBody()
        {
            Require(child?.Renderer != null,
                "Natural state9997 child has no Legacy renderer.");
            if (request.legacyBoot)
            {
                CaptureLegacyBody();
                if (request.captureLegacyPixels)
                    CaptureLegacyBodyPixels();
                if (ActiveFixtureSlot == 9)
                {
                    report.fallbackObservedPivotX =
                        (report.legacyX - report.presentationWorldLeft) /
                        report.presentationUnitsPerPixelX;
                    report.fallbackPixelDifference = Mathf.Abs(
                        report.fallbackObservedPivotX -
                        report.fallbackExpectedPivotX);
                }
                bool bodyObserved = report.childOwner == ActiveFixtureSlot &&
                                report.childDir == "left" &&
                                report.legacySpriteEnabled &&
                                (ActiveFixtureSlot == 9
                                    ? report.snapshotFlipX && report.legacyFlipX &&
                                      report.fallbackPixelDifference <= 0.75f
                                    : !report.legacyFlipX);
                report.status = request.captureLegacyPixels
                    ? bodyObserved && report.legacyBodyPixelCount > 0 &&
                      report.pixelTickBefore == report.pixelTickAfter &&
                      report.pixelCameraRestored && report.pixelBodyRestored
                        ? "LEGACY_GPU_BODY_PIXELS_OBSERVED"
                        : "LEGACY_GPU_BODY_PIXELS_MISSING"
                    : bodyObserved ? "LEGACY_BODY_OBSERVED" : "LEGACY_BODY_DIFFERENCE";
                return;
            }
            originalBackend = world.BattlePresentation.Mode;
            try
            {
                world.SetBattlePresentationBackend(
                    BattlePresentationBackendMode.LegacyOnly);
                backendSwitched = true;
                CaptureLegacyBody();
                report.legacyDeltaX = Mathf.Abs(report.legacyX - report.commandX);
                report.legacyDeltaY = Mathf.Abs(report.legacyY - report.commandY);
                report.status = report.childOwner == ActiveFixtureSlot &&
                                !report.commandFlipX && !report.legacyFlipX &&
                                report.legacySpriteEnabled &&
                                report.legacyDeltaX <= 0.001f &&
                                report.legacyDeltaY <= 0.001f
                    ? "LEGACY_BODY_MATCH"
                    : "LEGACY_BODY_DIFFERENCE";
            }
            finally
            {
                RestoreBackend();
            }
        }

        private static float ComputeOwner9FallbackPivotX(
            BattlePresentationEntitySnapshot entity, LF2Entity effect,
            int tickIndex)
        {
            float scale = NTSDRenderSpace.BattleVisualScale;
            float width = effect.GetSpriteWidthPxForRender() * scale;
            float centerX = entity.CenterX;
            int shakeX = entity.FrameDelay < 0 ? 6 * (tickIndex & 1) - 3 : 0;
            int screenX = entity.XInt + (int)entity.RenderOffsetX -
                          entity.CameraX + shakeX;
            float ordinaryPivot = entity.FlipX
                ? screenX + scale * (centerX -
                                     effect.GetSpriteWidthPxForRender() * 0.5f)
                : screenX + scale * (
                    effect.GetSpriteWidthPxForRender() * 0.5f - centerX);
            float maxLeft = Mathf.Max(report.cameraVisibleLeftPixels,
                report.cameraVisibleRightPixels - 1f - width);
            float left = Mathf.Clamp(ordinaryPivot - width * 0.5f,
                report.cameraVisibleLeftPixels, maxLeft);
            return left + width * 0.5f +
                   entity.HeldVisualAttachmentOffsetPixels.x +
                   entity.LocalOffsetPixels.x * scale;
        }

        private static void CaptureLegacyBody()
        {
            child.Renderer.ForceRefreshPresentation();
            FieldInfo bodyField = typeof(LF2ObjectRenderer).GetField(
                "_spriteRenderer", BindingFlags.Instance | BindingFlags.NonPublic);
            SpriteRenderer body = bodyField?.GetValue(child.Renderer) as SpriteRenderer;
            Require(body != null, "Natural child has no body SpriteRenderer.");
            Transform root = child.Renderer.transform.parent != null
                ? child.Renderer.transform.parent
                : child.Renderer.transform;
            report.legacyObserved = true;
            report.legacySpriteEnabled = body.enabled;
            report.legacyFlipX = body.flipX;
            report.legacyX = root.position.x;
            report.legacyY = root.position.y;
        }

        private static void CaptureLegacyBodyPixels()
        {
            const int width = 1280;
            const int height = 720;
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.isActiveAndEnabled,
                "An active Battle World camera is required for GPU capture.");
            SpriteRenderer body = child.Renderer.GetComponent<SpriteRenderer>();
            Require(body != null && body.enabled && body.sprite != null,
                "Natural Legacy body must have a visible Sprite before GPU capture.");

            RenderTexture originalTarget = camera.targetTexture;
            RenderTexture originalActive = RenderTexture.active;
            bool originalBodyEnabled = body.enabled;
            report.pixelCaptureWidth = width;
            report.pixelCaptureHeight = height;
            report.pixelTickBefore = world.CurrentTickIndex;
            var target = new RenderTexture(width, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            Texture2D readback = null;
            try
            {
                target.Create();
                readback = new Texture2D(width, height,
                    TextureFormat.RGBA32, false, true);
                camera.targetTexture = target;
                Color32[] withBody = CaptureLegacyCameraPixels(
                    camera, target, readback, "body-on",
                    out report.legacyBodyOnPng);
                body.enabled = false;
                Color32[] withoutBody = CaptureLegacyCameraPixels(
                    camera, target, readback, "body-off",
                    out report.legacyBodyOffPng);
                for (int index = 0; index < withBody.Length; index++)
                {
                    Color32 a = withBody[index];
                    Color32 b = withoutBody[index];
                    if (a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a)
                        continue;
                    int x = index % width;
                    int y = index / width;
                    if (report.legacyBodyPixelCount++ == 0)
                    {
                        report.legacyBodyPixelMinX = x;
                        report.legacyBodyPixelMinY = y;
                        report.legacyBodyPixelMaxX = x;
                        report.legacyBodyPixelMaxY = y;
                    }
                    else
                    {
                        report.legacyBodyPixelMinX = Mathf.Min(report.legacyBodyPixelMinX, x);
                        report.legacyBodyPixelMinY = Mathf.Min(report.legacyBodyPixelMinY, y);
                        report.legacyBodyPixelMaxX = Mathf.Max(report.legacyBodyPixelMaxX, x);
                        report.legacyBodyPixelMaxY = Mathf.Max(report.legacyBodyPixelMaxY, y);
                    }
                }
                report.pixelTickAfter = world.CurrentTickIndex;
            }
            finally
            {
                body.enabled = originalBodyEnabled;
                camera.targetTexture = originalTarget;
                RenderTexture.active = originalActive;
                report.pixelBodyRestored = body.enabled == originalBodyEnabled;
                report.pixelCameraRestored =
                    camera.targetTexture == originalTarget &&
                    RenderTexture.active == originalActive;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Color32[] CaptureLegacyCameraPixels(
            Camera camera, RenderTexture target, Texture2D readback,
            string suffix, out string relativePath)
        {
            relativePath = PixelResultRoot + "/" + request.runId +
                           "-" + suffix + ".png";
            string output = ProjectPath(relativePath);
            Require(!File.Exists(output), "Refusing to overwrite Legacy camera PNG.");
            camera.Render();
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0f, 0f, target.width, target.height),
                0, 0, false);
            readback.Apply(false, false);
            Color32[] pixels = readback.GetPixels32();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, readback.EncodeToPNG());
            return pixels;
        }

        private static void RestoreBackend()
        {
            if (!backendSwitched || world == null)
                return;
            world.SetBattlePresentationBackend(originalBackend);
            report.backendRestored = world.BattlePresentation.Mode == originalBackend;
            backendSwitched = false;
        }

        private static void Finish()
        {
            try
            {
                RestoreBackend();
                if (child?.Match == world && child.Runtime?.SlotIndex >= 0)
                    child.FreeEntityLikeExe();
                if (world != null)
                    world.FlushPendingDestroyForDiagnostics();
                report.childReleased = child == null || report.childSlot < 0 ||
                    world.FindEntityByRuntimeSlotForQuery(report.childSlot) != child;
                if (fixture?.RegisteredWorldForSimulation == world)
                    world.Unregister(fixture);
                report.fixtureReleased = fixture == null ||
                    fixture.RegisteredWorldForSimulation == null;
                if (world != null && report.sceneHashBefore != null)
                {
                    report.objectsAfter = world.ObjectCount;
                    report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                    report.borrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
                    report.sceneHashAfter = HashFile(ProjectPath(ScenePath));
                    if (report.sceneHashAfter != report.sceneHashBefore ||
                        report.objectsAfter != report.objectsBefore ||
                        report.slotsAfter != report.slotsBefore ||
                        report.borrowersAfter != report.borrowersBefore ||
                        !report.childReleased || !report.fixtureReleased)
                    {
                        report.status = "FAIL_CLEANUP";
                        report.error += " Owned entity or Scene cleanup mismatch.";
                    }
                }
                if (pauseCaptured && driver != null)
                {
                    driver.SetPaused(savedPaused);
                    report.pauseRestored = driver.IsPaused == savedPaused;
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL_CLEANUP";
                report.error += " Cleanup: " + error;
            }
            finally
            {
                string output = ResultPath(request.runId);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!File.Exists(output))
                    File.WriteAllText(output, JsonUtility.ToJson(report, true));
                request.requested = false;
                request.running = false;
                File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
                request = null;
                report = null;
                driver = null;
                world = null;
                fixture = null;
                child = null;
                phase = 0;
                stableTick = -1;
                stableUpdates = 0;
                expectedTick = -1;
                stepped = 0;
                pauseCaptured = false;
                backendSwitched = false;
                if (EditorApplication.isPlaying)
                    EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private static void WriteImmediateFailure(Request failed, string error)
        {
            string output = ResultPath(failed.runId);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!File.Exists(output))
                File.WriteAllText(output, JsonUtility.ToJson(new Report
                {
                    runId = failed.runId,
                    sourceX = failed.sourceX,
                    error = error,
                }, true));
            failed.requested = false;
            failed.running = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(failed));
        }

        private static bool ValidRunId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 80)
                return false;
            foreach (char ch in value)
                if (!char.IsLetterOrDigit(ch) && ch != '-' && ch != '_')
                    return false;
            return true;
        }

        private static string ResultPath(string runId) =>
            ProjectPath(ResultRoot + "/" + runId + ".json");

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
