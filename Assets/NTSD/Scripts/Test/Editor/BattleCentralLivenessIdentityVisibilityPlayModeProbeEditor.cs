#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Editor-only joint witness for central pending/generation/T+1 liveness,
    /// current-DAT OID223/224 shadow identity, and production visibility gates.
    /// Alignment contract: R8-RENDERLIVE-001.
    /// </summary>
    public static class BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run Central Liveness Identity Visibility Play Probe";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01G_R07B_CentralLivenessIdentityVisibility.result.json";
        private const string RequestRelativePath =
            "Temp/NTSD_R8_WP01G_R07B_CentralLivenessIdentityVisibility.request";
        private const int OidNoShadowA = 223;
        private const int OidNoShadowB = 224;
        private const int OidShadowControl = 203;
        private const int OidLateChild = 999;
        private const int ProducerObjectIdBase = 8700;
        private const int TimeoutEditorUpdates = 12000;
        private const int FixtureX = 560;
        private const int FixtureZ = 340;
        private const int FixtureSpacing = 48;

        private static readonly List<LF2Entity> BaselineEntities =
            new List<LF2Entity>(128);
        private static readonly List<LF2Entity> SnapshotBefore =
            new List<LF2Entity>(128);
        private static readonly List<LF2Entity> SnapshotAfter =
            new List<LF2Entity>(128);
        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(16);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPointFactory factory;
        private static LF2ObjectPool objectPool;
        private static CharacterAnimtorManager characterManager;
        private static ProbeOpointEntity directProducer;
        private static ProbeOpointEntity lateProducer;
        private static LF2Entity pendingEntity;
        private static LF2Entity oid223Entity;
        private static LF2Entity oid224Entity;
        private static LF2Entity controlEntity;
        private static LF2Entity lateChild;
        private static RuntimeEntityHandle pendingOldHandle;
        private static RuntimeEntityHandle lateChildHandle;
        private static RuntimeEntityHandle oid223Handle;
        private static RuntimeEntityHandle oid224Handle;
        private static RuntimeEntityHandle controlHandle;
        private static int controlExpectedCurrentDatOid;
        private static BattleStructuralWriterDiagnostics structuralBefore;
        private static ProbeReport report;
        private static ProbePhase phase;
        private static int editorUpdates;
        private static int expectedTick;
        private static int completionEditorUpdate;
        private static int pendingFixtureOid;
        private static int pendingFixtureFrame;
        private static int pauseStableTick;
        private static int pauseStableUpdates;
        private static int baselineObjectCount;
        private static int baselineClaimedSlots;
        private static int baselineObjectPoolActive;
        private static int baselineLogicPoolActive;
        private static int[] baselineKillStats;
        private static int[] baselineDamageStats;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;
        private static bool previousPaused;
        private static bool workerPath;
        private static bool pauseRequested;
        private static bool baselineCaptured;
        private static bool running;
        private static bool requestForcedDriverUnpause;

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || running)
            {
                return;
            }

            if (EditorApplication.isPaused)
            {
                EditorApplication.isPaused = false;
                return;
            }

            SimulationTickDriver currentDriver = SimulationTickDriver.Instance;
            if (currentDriver == null || currentDriver.World == null)
                return;
            if (currentDriver.IsPaused)
            {
                requestForcedDriverUnpause = true;
                currentDriver.SetPaused(false);
                return;
            }
            if (currentDriver.CurrentTickIndex < 5)
                return;

            string requestPath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                RequestRelativePath));
            if (!File.Exists(requestPath))
                return;

            File.Delete(requestPath);
            RunFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            bool restoreDriverPause = requestForcedDriverUnpause;
            StopObservation();
            ResetProbeState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            factory = LF2ObjectPointFactory.Instance;
            objectPool = LF2ObjectPool.Instance;
            characterManager = CharacterAnimtorManager.Instance;
            if (driver == null || world == null || factory == null ||
                objectPool == null || LF2ReferencePool.Instance == null ||
                GameDataManager.Instance == null || characterManager == null)
            {
                WriteImmediateFailure(
                    "The production driver, world, catalogs, factory, or pools are unavailable.");
                return;
            }
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly)
            {
                WriteImmediateFailure("R07B requires the protected CentralOnly backend.");
                return;
            }

            previousPaused = restoreDriverPause || driver.IsPaused;
            requestForcedDriverUnpause = false;
            workerPath = driver.DedicatedSimulationWorkerActiveForDiagnostics;
            report = new ProbeReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                workerPath = workerPath,
                pendingFixtureOid = pendingFixtureOid,
                pendingFixtureFrame = pendingFixtureFrame,
            };
            phase = ProbePhase.WaitingForSafeBoundary;
            running = true;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                Fail("Play Mode or the production world ended before completion.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > TimeoutEditorUpdates)
            {
                Fail("Timed out while waiting for the R07B joint witness.");
                return;
            }
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Fail(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }

            try
            {
                switch (phase)
                {
                    case ProbePhase.WaitingForSafeBoundary:
                        TryStartFixture();
                        break;
                    case ProbePhase.WaitingForIdentityTickCompletion:
                        ObserveIdentityTickCompletion();
                        break;
                    case ProbePhase.WaitingForTickCompletion:
                        ObserveTickCompletion();
                        break;
                    case ProbePhase.WaitingForIdentityPlan:
                        ObserveIdentityPlan();
                        break;
                    case ProbePhase.WaitingForTickPlan:
                        ObserveTickPlan();
                        break;
                    case ProbePhase.WaitingForNextTickCompletion:
                        ObserveNextTickCompletion();
                        break;
                    case ProbePhase.WaitingForNextTickPlan:
                        ObserveNextTickPlan();
                        break;
                }
            }
            catch (Exception exception)
            {
                Fail("Unhandled R07B probe exception: " + exception);
            }
        }

        private static void TryStartFixture()
        {
            if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0)
            {
                return;
            }

            string catalogFailure = ValidateCatalogAndFindPendingFixture();
            if (!string.IsNullOrEmpty(catalogFailure))
                return;

            if (!pauseRequested)
            {
                driver.SetPaused(true);
                pauseRequested = true;
                return;
            }
            if (!driver.IsPaused ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
            {
                pauseStableUpdates = 0;
                return;
            }
            if (pauseStableTick != driver.CurrentTickIndex)
            {
                pauseStableTick = driver.CurrentTickIndex;
                pauseStableUpdates = 0;
                return;
            }
            pauseStableUpdates++;
            if (pauseStableUpdates < 4)
                return;

            report.pendingFixtureOid = pendingFixtureOid;
            report.pendingFixtureFrame = pendingFixtureFrame;
            CaptureBaseline();
            BuildFixture();
            ScheduleTick(ProbePhase.WaitingForIdentityTickCompletion);
        }

        private static void CaptureBaseline()
        {
            baselineCaptured = true;
            baselineObjectCount = world.ObjectCount;
            baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            baselineObjectPoolActive = objectPool.ActiveObjectCountForAcceptance;
            baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineKillStats = CloneArray(world.KillStats);
            baselineDamageStats = CloneArray(world.DamageStats);
            baselineRngState = world.Rng.State;
            baselineRngCalls = world.Rng.CallCount;
            BaselineSounds.Clear();
            BaselineSounds.AddRange(world.PendingSounds);
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(BaselineEntities);

            report.baselineObjectCount = baselineObjectCount;
            report.baselineClaimedSlots = baselineClaimedSlots;
            report.baselineObjectPoolActive = baselineObjectPoolActive;
            report.baselineLogicPoolActive = baselineLogicPoolActive;
        }

        private static void BuildFixture()
        {
            directProducer = RegisterProbe(new ProbeOpointEntity(
                "R07B_DirectProducer",
                ProducerObjectIdBase,
                hasOpoint: true));

            CaptureSnapshot(SnapshotBefore);
            directProducer.SetSpawnOid(pendingFixtureOid);
            factory.ProcessOpointSpawn(directProducer);
            pendingEntity = FindSingleNewSpawn(pendingFixtureOid, SnapshotBefore);
            pendingEntity.SetPos(FixtureX, 0, FixtureZ);
            pendingEntity.RefreshRuntimeSnapshot();
            pendingOldHandle = RequireHandle(pendingEntity, "pending producer entity");
            directProducer.ClearOpoint();

            lateProducer = RegisterProbe(new ProbeOpointEntity(
                "R07B_LateProducer",
                ProducerObjectIdBase + 1,
                hasOpoint: true));
            lateProducer.ClearOpoint();

            oid223Entity = SpawnDirect(OidNoShadowA, FixtureX + FixtureSpacing, FixtureZ);
            oid224Entity = SpawnDirect(OidNoShadowB, FixtureX + FixtureSpacing * 2, FixtureZ);
            controlEntity = FindBaselineCharacterControl();
            directProducer.ClearOpoint();
            oid223Handle = RequireHandle(oid223Entity, "formal oid223");
            oid224Handle = RequireHandle(oid224Entity, "formal oid224");
            controlHandle = RequireHandle(controlEntity, "formal baseline character shadow control");
            controlExpectedCurrentDatOid = LF2Entity.ResolveCurrentDataObjectId(controlEntity);

            Require(pendingOldHandle.Slot < lateProducer.Runtime.SlotIndex,
                "The pending fixture must own a lower slot than the late producer.");
            report.pendingOldSlot = pendingOldHandle.Slot;
            report.pendingOldGeneration = pendingOldHandle.Generation;
        }

        private static void ObserveIdentityTickCompletion()
        {
            if (!TickCompleted())
                return;

            completionEditorUpdate = editorUpdates;
            phase = ProbePhase.WaitingForIdentityPlan;
        }

        private static void ObserveIdentityPlan()
        {
            if (!CurrentCentralPlanReady(expectedTick, completionEditorUpdate))
                return;

            CaptureIdentityEvidence();
            ReleaseIfOwned(ref oid223Entity);
            ReleaseIfOwned(ref oid224Entity);
            controlEntity = null;
            directProducer.ClearOpoint();
            pendingEntity.ImmediateFrame(pendingFixtureFrame);
            pendingEntity.RefreshRuntimeSnapshot();
            lateProducer.SetSpawnOid(OidLateChild);
            structuralBefore = world.StructuralWriterDiagnosticsForDiagnostics;
            ScheduleTick(ProbePhase.WaitingForTickCompletion);
        }

        private static LF2Entity SpawnDirect(int oid, int x, int z)
        {
            CaptureSnapshot(SnapshotBefore);
            directProducer.SetSpawnOid(oid);
            factory.ProcessOpointSpawn(directProducer);
            LF2Entity spawned = FindSingleNewSpawn(oid, SnapshotBefore);
            spawned.SetPos(x, 0, z);
            spawned.Runtime.SetVelocity(0d, 0d, 0d);
            spawned.RefreshRuntimeSnapshot();
            return spawned;
        }

        private static void ObserveTickCompletion()
        {
            if (!TickCompleted())
                return;

            Require(!world.TryResolveRuntimeHandleForDiagnostics(pendingOldHandle, out _),
                "The formal frame-logic pending producer did not release its old handle.");
            BattleStructuralWriterDiagnostics structuralAfter =
                world.StructuralWriterDiagnosticsForDiagnostics;
            report.pendingFreeDelta = structuralAfter.FreeCount - structuralBefore.FreeCount;
            report.generationReleaseDelta =
                structuralAfter.GenerationReleaseCount - structuralBefore.GenerationReleaseCount;
            Require(report.pendingFreeDelta > 0 && report.generationReleaseDelta > 0,
                "The pending producer did not cross the production free/generation-release boundary.");

            CaptureSnapshot(SnapshotAfter);
            lateChild = FindLateChild();
            lateChildHandle = RequireHandle(lateChild, "late opoint child");
            report.lateChildSlot = lateChildHandle.Slot;
            report.lateChildGeneration = lateChildHandle.Generation;
            report.sameSlotReused = lateChildHandle.Slot == pendingOldHandle.Slot;
            report.generationAdvanced = lateChildHandle.Generation != pendingOldHandle.Generation;
            Require(report.sameSlotReused && report.generationAdvanced,
                "Late opoint did not reuse the released lowest slot with a new generation.");
            lateProducer.ClearOpoint();
            completionEditorUpdate = editorUpdates;
            phase = ProbePhase.WaitingForTickPlan;
        }

        private static void ObserveTickPlan()
        {
            if (!CurrentCentralPlanReady(expectedTick, completionEditorUpdate))
                return;

            BattleCentralEntityDiagnostic oldEntity =
                BattleCentralRenderSystem.CaptureEntityDiagnostic(
                    world,
                    pendingOldHandle,
                    BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic newEntityAtT =
                BattleCentralRenderSystem.CaptureEntityDiagnostic(
                    world,
                    lateChildHandle,
                    BattleRenderCommandType.Entity);
            report.tickT = expectedTick;
            report.oldHandleReasonAtT = oldEntity.Reason.ToString();
            report.newHandleReasonAtT = newEntityAtT.Reason.ToString();
            report.oldHandleHasSnapshotAtT = oldEntity.HasSnapshot;
            report.oldHandleHasCommandAtT = oldEntity.HasCommand;
            report.oldHandleSubmittedAtT = oldEntity.Submitted;
            report.newHandleHasSnapshotAtT = newEntityAtT.HasSnapshot;
            report.newHandleHasCommandAtT = newEntityAtT.HasCommand;
            report.newHandleSubmittedAtT = newEntityAtT.Submitted;
            Require(oldEntity.Reason == BattleCentralEntityDiagnosticReason.GenerationMismatch &&
                    !oldEntity.HasSnapshot && !oldEntity.HasCommand && !oldEntity.Submitted,
                "The stale pending generation remained resolvable by the central T frame.");
            Require(newEntityAtT.Reason == BattleCentralEntityDiagnosticReason.MissingSnapshotEntity &&
                    !newEntityAtT.HasSnapshot && !newEntityAtT.HasCommand && !newEntityAtT.Submitted,
                "The late opoint polluted the already-frozen T presentation frame.");

            ScheduleTick(ProbePhase.WaitingForNextTickCompletion);
        }

        private static void CaptureIdentityEvidence()
        {
            report.oid223 = CaptureFormalIdentity(oid223Handle, OidNoShadowA, false);
            report.oid224 = CaptureFormalIdentity(oid224Handle, OidNoShadowB, false);
            report.control = CaptureFormalIdentity(
                controlHandle,
                controlExpectedCurrentDatOid,
                true);

            Require(report.oid223.bodyVisibleAndMaterialized &&
                    report.oid224.bodyVisibleAndMaterialized,
                "Formal OID223/224 bodies did not reach the current CentralOnly submission.");
            Require(report.oid223.shadowPixelOwnerAbsent && report.oid224.shadowPixelOwnerAbsent,
                "Formal current-DAT OID223/224 emitted shadow geometry/pixel ownership.");
            Require(report.control.bodyVisibleAndMaterialized &&
                    report.control.shadowVisibleAndMaterialized,
                "The ordinary formal control did not retain body and common-shadow visibility.");

            IdentityEvidence[] ordered = { report.oid223, report.oid224 };
            Array.Sort(ordered, (left, right) => left.slot.CompareTo(right.slot));
            report.painterOrderingInputsSameZ = ordered[0].zInt == ordered[1].zInt;
            int expectedComparison = ordered[0].zInt.CompareTo(ordered[1].zInt);
            if (expectedComparison == 0)
                expectedComparison = ordered[0].slot.CompareTo(ordered[1].slot);
            int actualComparison = ordered[0].presentationBaseOrder.CompareTo(
                ordered[1].presentationBaseOrder);
            report.painterOrderStable =
                Math.Sign(expectedComparison) == Math.Sign(actualComparison);
            report.sameZPainterSlotOrderStable =
                report.painterOrderingInputsSameZ && report.painterOrderStable;
            Require(report.painterOrderStable,
                "Painter base order no longer follows the Z/runtime-slot comparator.");
        }

        private static IdentityEvidence CaptureFormalIdentity(
            RuntimeEntityHandle handle,
            int expectedCurrentDatOid,
            bool expectsShadow)
        {
            BattleCentralEntityDiagnostic body =
                BattleCentralRenderSystem.CaptureEntityDiagnostic(
                    world,
                    handle,
                    BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic shadow =
                BattleCentralRenderSystem.CaptureEntityDiagnostic(
                    world,
                    handle,
                    BattleRenderCommandType.Shadow);
            var evidence = new IdentityEvidence
            {
                slot = handle.Slot,
                generation = handle.Generation,
                currentDatObjectId = body.CurrentDatObjectId,
                bodyReason = body.Reason.ToString(),
                shadowReason = shadow.Reason.ToString(),
                entityVisible = body.EntityVisible,
                shadowVisible = shadow.ShadowVisible,
                bodyHasSnapshot = body.HasSnapshot,
                bodyHasCommand = body.HasCommand,
                bodySubmitted = body.Submitted,
                bodyHasResolvedResource = body.HasResolvedResource,
                shadowHasSnapshot = shadow.HasSnapshot,
                shadowHasCommand = shadow.HasCommand,
                shadowSubmitted = shadow.Submitted,
                shadowHasResolvedResource = shadow.HasResolvedResource,
                presentationBaseOrder = body.PresentationBaseOrder,
            };
            BattlePresentationFrame frame = world.CurrentPixelFramePlan.CapturedFrame;
            if (frame != null)
            {
                for (int index = 0; index < frame.EntityCount; index++)
                {
                    BattlePresentationEntitySnapshot snapshot = frame.GetEntity(index);
                    if (snapshot.Handle.Equals(handle))
                    {
                        evidence.zInt = snapshot.ZInt;
                        break;
                    }
                }
            }
            evidence.bodyVisibleAndMaterialized =
                evidence.currentDatObjectId == expectedCurrentDatOid &&
                evidence.entityVisible && evidence.bodyHasSnapshot &&
                evidence.bodyHasCommand && evidence.bodyHasResolvedResource;
            evidence.shadowVisibleAndMaterialized =
                evidence.shadowVisible && evidence.shadowHasSnapshot &&
                evidence.shadowHasCommand && evidence.shadowHasResolvedResource;
            evidence.shadowPixelOwnerAbsent =
                evidence.shadowHasSnapshot &&
                !evidence.shadowHasCommand && !evidence.shadowSubmitted &&
                shadow.Reason == BattleCentralEntityDiagnosticReason.CommandSuppressed;
            return evidence;
        }

        private static LF2Entity FindBaselineCharacterControl()
        {
            for (int index = 0; index < BaselineEntities.Count; index++)
            {
                LF2Entity candidate = BaselineEntities[index];
                if (candidate != null &&
                    candidate.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.Character)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                "No formal baseline character is available as the ordinary shadow control.");
        }

        private static void ObserveNextTickCompletion()
        {
            if (!TickCompleted())
                return;
            completionEditorUpdate = editorUpdates;
            phase = ProbePhase.WaitingForNextTickPlan;
        }

        private static void ObserveNextTickPlan()
        {
            if (!CurrentCentralPlanReady(expectedTick, completionEditorUpdate))
                return;

            BattleCentralEntityDiagnostic oldEntity =
                BattleCentralRenderSystem.CaptureEntityDiagnostic(
                    world,
                    pendingOldHandle,
                    BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic newEntity =
                BattleCentralRenderSystem.CaptureEntityDiagnostic(
                    world,
                    lateChildHandle,
                    BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic newShadow =
                BattleCentralRenderSystem.CaptureEntityDiagnostic(
                    world,
                    lateChildHandle,
                    BattleRenderCommandType.Shadow);
            report.tickT1 = expectedTick;
            report.oldHandleReasonAtT1 = oldEntity.Reason.ToString();
            report.newHandleReasonAtT1 = newEntity.Reason.ToString();
            report.newHandleHasSnapshotAtT1 = newEntity.HasSnapshot;
            report.newHandleHasCommandAtT1 = newEntity.HasCommand;
            report.newHandleSubmittedAtT1 = newEntity.Submitted;
            report.newShadowHasCommandAtT1 = newShadow.HasCommand;
            report.newShadowSubmittedAtT1 = newShadow.Submitted;
            report.poolReuseVisibilityRestored =
                newEntity.EntityVisible && newEntity.HasSnapshot &&
                newEntity.HasCommand && newEntity.HasResolvedResource &&
                newShadow.ShadowVisible && newShadow.HasCommand &&
                newShadow.HasResolvedResource;
            Require(oldEntity.Reason == BattleCentralEntityDiagnosticReason.GenerationMismatch,
                "The stale old generation resolved after the T+1 replacement publication.");
            Require(report.poolReuseVisibilityRestored,
                "The T+1 same-slot replacement retained stale body/shadow hiding state.");

            report.productionVisibilityWriterSourceClosed = true;
            report.noExtraHidingObserved =
                report.oid223.bodyVisibleAndMaterialized &&
                report.oid224.bodyVisibleAndMaterialized &&
                report.control.bodyVisibleAndMaterialized &&
                report.control.shadowVisibleAndMaterialized &&
                report.poolReuseVisibilityRestored;
            Require(report.noExtraHidingObserved,
                "A formal body/shadow was hidden beyond the C++ current-DAT visibility gates.");
            FinishSuccess();
        }

        private static void ScheduleTick(ProbePhase waitingPhase)
        {
            expectedTick = driver.CurrentTickIndex + 1;
            bool accepted = workerPath
                ? driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                    buildPresentation: true)
                : driver.StepOneTick(ignorePaused: true, buildPresentation: true);
            Require(
                accepted,
                workerPath
                    ? "The production worker rejected the R07B diagnostic tick: " +
                      driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics
                    : "The production synchronous driver rejected the R07B diagnostic tick.");
            phase = waitingPhase;
        }

        private static bool TickCompleted()
        {
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return false;
            return driver.CurrentTickIndex >= expectedTick;
        }

        private static bool CurrentCentralPlanReady(int tick, int completedUpdate)
        {
            if (editorUpdates <= completedUpdate + 1 ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
            {
                return false;
            }

            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            if (!plan.IsValid || plan.SimulationTick != tick ||
                plan.CapturedFrame == null || plan.CapturedFrame.TickIndex != tick)
            {
                plan = BattleCentralRenderSystem.PrepareFrame(world);
            }
            BattlePresentationFrame frame = plan.CapturedFrame;
            if (report != null)
            {
                BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
                report.lastObservedPlanValid = plan.IsValid;
                report.lastObservedPlanOwner = plan.Owner.ToString();
                report.lastObservedPlanReason = plan.Reason ?? string.Empty;
                report.lastObservedPlanSimulationTick = plan.SimulationTick;
                report.lastObservedPlanHasSubmission = plan.Submission != null;
                report.lastObservedFrameTick = frame?.TickIndex ?? -1;
                report.lastObservedFrameCommandsMaterialized =
                    frame?.CommandsMaterialized == true;
                report.lastObservedPublishedTick = published?.TickIndex ?? -1;
                report.lastObservedPublishedEntityCount = published?.EntityCount ?? 0;
                report.lastObservedPublishedCommandCount = published?.CommandCount ?? 0;
                report.lastObservedCapturedEntities = DescribeCapturedEntities(frame);
                report.lastObservedPendingPublishedTick =
                    BattleCentralRenderSystem.PendingPublishedTickForDiagnostics;
                report.lastObservedMaterializedPublishedTick =
                    BattleCentralRenderSystem.LastMaterializedPublishedTickForDiagnostics;
            }
            return plan.IsValid && plan.Owner == BattlePixelFrameOwner.Central &&
                   !plan.IsStale && plan.Submission != null &&
                   frame != null && frame.CommandsMaterialized &&
                   plan.SimulationTick == tick && frame.TickIndex == tick;
        }

        private static string DescribeCapturedEntities(BattlePresentationFrame frame)
        {
            if (frame == null)
                return "<null>";

            string result = string.Empty;
            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(index);
                if (index > 0)
                    result += ";";
                result +=
                    $"slot={entity.Handle.Slot},gen={entity.Handle.Generation}," +
                    $"oid={entity.ObjectId},dat={entity.CurrentDatObjectId}," +
                    $"body={entity.EntityVisible},shadow={entity.ShadowVisible}";
            }
            return result;
        }

        private static string ValidateCatalogAndFindPendingFixture()
        {
            int[] requiredOids =
            {
                OidNoShadowA,
                OidNoShadowB,
                OidShadowControl,
                OidLateChild,
            };
            for (int index = 0; index < requiredOids.Length; index++)
            {
                int oid = requiredOids[index];
                ObjectDefinition definition = GameDataManager.Instance.GetObjectById(oid);
                LF2CharacterDataWrapper wrapper = characterManager.GetCharacterConfig(oid);
                if (definition == null || wrapper?.characterData == null)
                    return $"The formal production catalog is missing OID{oid}.";
            }

            List<ObjectDefinition> definitions = GameDataManager.Instance.GetAllObjects();
            int[] preferredHitFa = { 13, 11, 8, 9, 6, 5 };
            for (int pass = 0; pass < preferredHitFa.Length; pass++)
            {
                int desired = preferredHitFa[pass];
                for (int definitionIndex = 0; definitionIndex < definitions.Count; definitionIndex++)
                {
                    ObjectDefinition definition = definitions[definitionIndex];
                    if (definition == null || definition.type == (int)LF2ObjectType.Character)
                        continue;
                    LF2CharacterDataWrapper wrapper =
                        characterManager.GetCharacterConfig(definition.id);
                    List<LF2FrameData> frames = wrapper?.characterData?.frames;
                    if (frames == null)
                        continue;
                    for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                    {
                        LF2FrameData frame = frames[frameIndex];
                        if (frame != null && frame.hit_Fa == desired)
                        {
                            pendingFixtureOid = definition.id;
                            pendingFixtureFrame = frame.frameId;
                            return string.Empty;
                        }
                    }
                }
            }
            return "No formal non-character DAT frame exposes a production pending/free hit_Fa producer.";
        }

        private static ProbeOpointEntity RegisterProbe(ProbeOpointEntity entity)
        {
            world.Register(entity);
            Require(entity.Runtime?.SlotIndex >= 0,
                (entity?.Name ?? "probe") + " did not receive a runtime slot.");
            return entity;
        }

        private static LF2Entity FindSingleNewSpawn(int oid, List<LF2Entity> before)
        {
            CaptureSnapshot(SnapshotAfter);
            LF2Entity found = null;
            for (int index = 0; index < SnapshotAfter.Count; index++)
            {
                LF2Entity candidate = SnapshotAfter[index];
                if (candidate == null || candidate.ObjectId != oid ||
                    before.Contains(candidate) || candidate == directProducer ||
                    candidate == lateProducer)
                {
                    continue;
                }
                if (found != null)
                    throw new InvalidOperationException($"Multiple new OID{oid} entities appeared.");
                found = candidate;
            }
            if (found == null)
                throw new InvalidOperationException($"No new formal OID{oid} entity appeared.");
            return found;
        }

        private static LF2Entity FindLateChild()
        {
            LF2Entity found = null;
            for (int index = 0; index < SnapshotAfter.Count; index++)
            {
                LF2Entity candidate = SnapshotAfter[index];
                if (candidate == null || candidate.ObjectId != OidLateChild ||
                    BaselineEntities.Contains(candidate))
                {
                    continue;
                }
                if (found == null || candidate.Runtime.SlotIndex == pendingOldHandle.Slot)
                    found = candidate;
            }
            if (found == null)
                throw new InvalidOperationException("The production late-opoint child did not materialize.");
            return found;
        }

        private static void ReleaseIfOwned(ref LF2Entity entity)
        {
            LF2Entity current = entity;
            entity = null;
            if (current?.Match == world && current.Runtime?.SlotIndex >= 0)
                current.FreeEntityLikeExe();
            world.FlushPendingDestroyForDiagnostics();
        }

        private static RuntimeEntityHandle RequireHandle(LF2Entity entity, string label)
        {
            if (entity?.Runtime == null ||
                !world.TryGetCurrentRuntimeHandleForDiagnostics(
                    entity.Runtime.SlotIndex,
                    entity,
                    out RuntimeEntityHandle handle) ||
                !handle.IsValid)
            {
                throw new InvalidOperationException(label + " has no valid runtime handle.");
            }
            return handle;
        }

        private static void CaptureSnapshot(List<LF2Entity> destination)
        {
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(destination);
        }

        private static void FinishSuccess()
        {
            report.status = "PASS";
            report.message =
                "Production pending/free, same-slot generation invalidation, late T+1 publication, " +
                "formal OID223/224 current-DAT shadow suppression, ordinary shadow visibility, " +
                "and pool-reuse visibility passed.";
            report.endTick = driver.CurrentTickIndex;
            Cleanup();
            CaptureFinalState();
            Require(report.cleanupCompleted,
                "R07B cleanup did not restore the live-world baseline: " + report.cleanupErrors);
            WriteResult(report);
            Debug.Log(
                $"[BattleCentralLivenessIdentityVisibilityProbe] PASS: " +
                $"ticks={report.tickT}->{report.tickT1}, slot={report.pendingOldSlot}, " +
                $"generation={report.pendingOldGeneration}->{report.lateChildGeneration}.");
            StopObservation();
        }

        private static void Fail(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.message = message;
            report.endTick = driver?.CurrentTickIndex ?? -1;
            Cleanup();
            CaptureFinalState();
            WriteResult(report);
            Debug.LogError("[BattleCentralLivenessIdentityVisibilityProbe] FAIL: " + message);
            StopObservation();
        }

        private static void Cleanup()
        {
            if (!baselineCaptured || world == null)
                return;
            try
            {
                CaptureSnapshot(SnapshotAfter);
                for (int index = SnapshotAfter.Count - 1; index >= 0; index--)
                {
                    LF2Entity entity = SnapshotAfter[index];
                    if (entity == null || BaselineEntities.Contains(entity))
                        continue;
                    if (entity.Match == world && entity.Runtime?.SlotIndex >= 0)
                        entity.FreeEntityLikeExe();
                }
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                AppendCleanupError("entity-release", exception);
            }

            RestoreArray(world.KillStats, baselineKillStats);
            RestoreArray(world.DamageStats, baselineDamageStats);
            world.PendingSounds.Clear();
            world.PendingSounds.AddRange(BaselineSounds);
            world.Rng.RestoreState(baselineRngState, baselineRngCalls);
            try
            {
                world.RenderDispatchAll(driver.CurrentTickIndex, buildPresentation: true);
            }
            catch (Exception exception)
            {
                AppendCleanupError("presentation-refresh", exception);
            }
            if (driver != null && EditorApplication.isPlaying)
                driver.SetPaused(previousPaused);
        }

        private static void CaptureFinalState()
        {
            if (report == null)
                return;
            report.finalObjectCount = world?.ObjectCount ?? -1;
            report.finalClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.finalObjectPoolActive = objectPool?.ActiveObjectCountForAcceptance ?? -1;
            report.finalLogicPoolActive = LF2ReferencePool.Instance?.ActiveCount ?? -1;
            report.pauseRestored = driver == null || driver.IsPaused == previousPaused;
            report.cleanupCompleted = baselineCaptured &&
                string.IsNullOrEmpty(report.cleanupErrors) &&
                report.finalObjectCount == baselineObjectCount &&
                report.finalClaimedSlots == baselineClaimedSlots &&
                report.finalObjectPoolActive == baselineObjectPoolActive &&
                report.finalLogicPoolActive == baselineLogicPoolActive &&
                report.pauseRestored;
        }

        private static void AppendCleanupError(string label, Exception exception)
        {
            if (report != null)
                report.cleanupErrors += label + ":" + exception.Message + ";";
        }

        private static int[] CloneArray(int[] source)
        {
            return source != null ? (int[])source.Clone() : Array.Empty<int>();
        }

        private static void RestoreArray(int[] destination, int[] source)
        {
            if (destination != null && source != null)
                Array.Copy(source, destination, Math.Min(source.Length, destination.Length));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeReport
            {
                status = "FAIL",
                message = message,
                startTick = driver?.CurrentTickIndex ?? -1,
                endTick = driver?.CurrentTickIndex ?? -1,
            });
            Debug.LogError("[BattleCentralLivenessIdentityVisibilityProbe] FAIL: " + message);
        }

        private static void WriteResult(ProbeReport value)
        {
            string path = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                ResultRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            running = false;
        }

        private static void ResetProbeState()
        {
            driver = null;
            world = null;
            factory = null;
            objectPool = null;
            characterManager = null;
            directProducer = null;
            lateProducer = null;
            pendingEntity = null;
            oid223Entity = null;
            oid224Entity = null;
            controlEntity = null;
            lateChild = null;
            pendingOldHandle = RuntimeEntityHandle.Invalid;
            lateChildHandle = RuntimeEntityHandle.Invalid;
            oid223Handle = RuntimeEntityHandle.Invalid;
            oid224Handle = RuntimeEntityHandle.Invalid;
            controlHandle = RuntimeEntityHandle.Invalid;
            controlExpectedCurrentDatOid = -1;
            structuralBefore = default;
            report = null;
            phase = ProbePhase.None;
            editorUpdates = 0;
            expectedTick = -1;
            completionEditorUpdate = 0;
            pendingFixtureOid = -1;
            pendingFixtureFrame = -1;
            pauseStableTick = -1;
            pauseStableUpdates = 0;
            baselineObjectCount = 0;
            baselineClaimedSlots = 0;
            baselineObjectPoolActive = 0;
            baselineLogicPoolActive = 0;
            baselineKillStats = null;
            baselineDamageStats = null;
            baselineRngState = 0;
            baselineRngCalls = 0;
            previousPaused = false;
            workerPath = false;
            pauseRequested = false;
            baselineCaptured = false;
            running = false;
            BaselineEntities.Clear();
            SnapshotBefore.Clear();
            SnapshotAfter.Clear();
            BaselineSounds.Clear();
        }

        private sealed class ProbeOpointEntity : LF2OtherObject
        {
            private readonly LF2FrameData probeFrame;

            public ProbeOpointEntity(string name, int objectId, bool hasOpoint)
            {
                Name = name;
                ObjectId = objectId;
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                probeFrame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 10000,
                    next = 0,
                    pic = 999,
                    centerx = 0,
                    centery = 0,
                };
                if (hasOpoint)
                    SetSpawnOid(OidShadowControl);
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)LF2ObjectType.Other,
                        frames = new List<LF2FrameData> { probeFrame },
                    }));
                Frame.D = probeFrame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Frame.Prev2 = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Runtime.SetPosition(FixtureX, 0, FixtureZ);
                Runtime.SyncIntegerPosition();
                PS.dir = "right";
            }

            public void SetSpawnOid(int oid)
            {
                probeFrame.opoint = new ObjectPoint
                {
                    kind = 1,
                    oid = oid,
                    action = 0,
                    facing = 0,
                };
                Frame.D = probeFrame;
                AttackingCounter = 0;
            }

            public void ClearOpoint()
            {
                probeFrame.opoint = null;
                probeFrame.opoints?.Clear();
            }

            public override void SimFrameTick(int tickIndex)
            {
            }
        }

        [Serializable]
        private sealed class IdentityEvidence
        {
            public int slot;
            public long generation;
            public int currentDatObjectId;
            public string bodyReason = string.Empty;
            public string shadowReason = string.Empty;
            public bool entityVisible;
            public bool shadowVisible;
            public bool bodyHasSnapshot;
            public bool bodyHasCommand;
            public bool bodySubmitted;
            public bool bodyHasResolvedResource;
            public bool shadowHasSnapshot;
            public bool shadowHasCommand;
            public bool shadowSubmitted;
            public bool shadowHasResolvedResource;
            public int presentationBaseOrder;
            public int zInt;
            public bool bodyVisibleAndMaterialized;
            public bool shadowVisibleAndMaterialized;
            public bool shadowPixelOwnerAbsent;
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public int startTick;
            public int endTick;
            public bool workerPath;
            public int pendingFixtureOid;
            public int pendingFixtureFrame;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int baselineObjectPoolActive;
            public int baselineLogicPoolActive;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public bool pauseRestored;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public int tickT;
            public int tickT1;
            public int pendingOldSlot;
            public long pendingOldGeneration;
            public int lateChildSlot;
            public long lateChildGeneration;
            public bool sameSlotReused;
            public bool generationAdvanced;
            public long pendingFreeDelta;
            public long generationReleaseDelta;
            public string oldHandleReasonAtT = string.Empty;
            public string newHandleReasonAtT = string.Empty;
            public bool oldHandleHasSnapshotAtT;
            public bool oldHandleHasCommandAtT;
            public bool oldHandleSubmittedAtT;
            public bool newHandleHasSnapshotAtT;
            public bool newHandleHasCommandAtT;
            public bool newHandleSubmittedAtT;
            public string oldHandleReasonAtT1 = string.Empty;
            public string newHandleReasonAtT1 = string.Empty;
            public bool newHandleHasSnapshotAtT1;
            public bool newHandleHasCommandAtT1;
            public bool newHandleSubmittedAtT1;
            public bool newShadowHasCommandAtT1;
            public bool newShadowSubmittedAtT1;
            public bool poolReuseVisibilityRestored;
            public bool sameZPainterSlotOrderStable;
            public bool painterOrderingInputsSameZ;
            public bool painterOrderStable;
            public bool productionVisibilityWriterSourceClosed;
            public bool noExtraHidingObserved;
            public bool lastObservedPlanValid;
            public string lastObservedPlanOwner = string.Empty;
            public string lastObservedPlanReason = string.Empty;
            public int lastObservedPlanSimulationTick;
            public bool lastObservedPlanHasSubmission;
            public int lastObservedFrameTick;
            public bool lastObservedFrameCommandsMaterialized;
            public int lastObservedPublishedTick;
            public int lastObservedPublishedEntityCount;
            public int lastObservedPublishedCommandCount;
            public string lastObservedCapturedEntities = string.Empty;
            public int lastObservedPendingPublishedTick;
            public int lastObservedMaterializedPublishedTick;
            public IdentityEvidence oid223;
            public IdentityEvidence oid224;
            public IdentityEvidence control;
        }

        private enum ProbePhase
        {
            None,
            WaitingForSafeBoundary,
            WaitingForIdentityTickCompletion,
            WaitingForIdentityPlan,
            WaitingForTickCompletion,
            WaitingForTickPlan,
            WaitingForNextTickCompletion,
            WaitingForNextTickPlan,
        }
    }
}
#endif
