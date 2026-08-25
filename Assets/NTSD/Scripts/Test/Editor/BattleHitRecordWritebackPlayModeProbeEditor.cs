#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Editor-only full-tick witness for an actual kind0 collision producer through the
    /// frozen presentation cycle and same-tick HitRecord writeback.
    /// Alignment contract: R8-HITWRITEBACK-001.
    /// </summary>
    public static class BattleHitRecordWritebackPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run HitRecord Writeback Play Probe";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01G_R07A_HitRecordWriteback.result.json";
        private const int TimeoutEditorUpdates = 2400;
        private const int ProbeObjectIdBase = 8400;
        private const int FixtureX = 200000;
        private const int PublishedTickCount = 3;
        private const int TotalTickCount = 4;

        private static readonly List<LF2Entity> OwnedEntities =
            new List<LF2Entity>(TotalTickCount * 2);
        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(16);
        private static readonly ProbeCharacter[] Attackers =
            new ProbeCharacter[TotalTickCount];
        private static readonly ProbeCharacter[] Victims =
            new ProbeCharacter[TotalTickCount];
        private static readonly uint[] VictimGenerations =
            new uint[TotalTickCount];
        private static readonly TickEvidence[] TickSamples =
            new TickEvidence[TotalTickCount];

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool objectPool;
        private static ProbeCharacter attacker;
        private static ProbeCharacter victim;
        private static LF2Entity recordOwner;
        private static BattleTickPhaseDiagnostics tickDiagnostics;
        private static ProbeReport report;
        private static ProbePhase phase;
        private static int editorUpdates;
        private static int completionEditorUpdate;
        private static int expectedTick;
        private static int tickOrdinal;
        private static int cycleIdBeforeTick;
        private static ulong rngCallsBeforeTick;
        private static int[] baselineKillStats;
        private static int[] baselineDamageStats;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;
        private static int baselineObjectCount;
        private static int baselineClaimedSlots;
        private static int baselineObjectPoolActive;
        private static int baselineLogicPoolActive;
        private static int warmTickAllocationViolationCount;
        private static int warmPresentationAllocationViolationCount;
        private static bool previousPaused;
        private static bool workerPath;
        private static bool enabledTickDiagnostics;
        private static bool baselineCaptured;
        private static bool running;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            ResetProbeState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            objectPool = LF2ObjectPool.Instance;
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            if (driver == null || world == null || objectPool == null ||
                LF2ReferencePool.Instance == null || manager == null)
            {
                WriteImmediateFailure(
                    "The production driver, world, pools, or character manager are unavailable.");
                return;
            }
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly)
            {
                WriteImmediateFailure("R07A requires the protected CentralOnly backend.");
                return;
            }
            if (world.RuntimeDataCatalog?.HitRecordLifecycleCatalog.IsAvailable != true ||
                !manager.CommonVisualCatalog.IsSparkValid)
            {
                WriteImmediateFailure(
                    "The production HitRecord lifecycle or common Spark publication is unavailable.");
                return;
            }

            previousPaused = driver.IsPaused;
            report = new ProbeReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                workerPath = driver.DedicatedSimulationWorkerActiveForDiagnostics,
            };
            running = true;
            phase = ProbePhase.WaitingForSafeBoundary;
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
                Fail("Timed out while waiting for the R07A full-tick witness.");
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
                    case ProbePhase.WaitingForTickCompletion:
                        ObserveTickCompletion();
                        break;
                    case ProbePhase.WaitingForLateFallback:
                        ObserveLateFallback();
                        break;
                }
            }
            catch (Exception exception)
            {
                Fail("Unhandled R07A probe exception: " + exception);
            }
        }

        private static void TryStartFixture()
        {
            if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0 ||
                world.ClaimedRuntimeSlotCountForDiagnostics <= 0 ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics ||
                HasLiveBaselineHitRecords())
            {
                return;
            }

            driver.SetPaused(true);
            CaptureBaseline();
            tickDiagnostics = world.ActiveBattleTickPhaseDiagnosticsForDiagnostics;
            if (tickDiagnostics == null)
            {
                tickDiagnostics = world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
                enabledTickDiagnostics = true;
            }

            BuildFixture();
            workerPath = driver.DedicatedSimulationWorkerActiveForDiagnostics;
            report.workerPath = workerPath;
            tickOrdinal = 0;
            ScheduleTick(buildPresentation: true);
        }

        private static bool HasLiveBaselineHitRecords()
        {
            int capacity = world.RuntimeSlotCapacityForDiagnostics;
            for (int slot = 0; slot < capacity; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity?.HitRecordCount > 0)
                    return true;
            }
            return false;
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

            report.baselineObjectCount = baselineObjectCount;
            report.baselineClaimedSlots = baselineClaimedSlots;
            report.baselineObjectPoolActive = baselineObjectPoolActive;
            report.baselineLogicPoolActive = baselineLogicPoolActive;
            report.baselineRngState = baselineRngState;
            report.baselineRngCalls = baselineRngCalls;
        }

        private static void BuildFixture()
        {
            for (int index = 0; index < Attackers.Length; index++)
            {
                var attackItr = new InteractionArea
                {
                    kind = 0,
                    x = -30,
                    y = -10,
                    w = 60,
                    h = 20,
                    zwidth = 15,
                    injury = 1,
                    fall = 61,
                    dvx = 0,
                    dvy = 0,
                    arest = 0,
                    vrest = 0,
                    effect = 0,
                };
                Attackers[index] = RegisterOwned(new ProbeCharacter(
                    "R07A_HitRecordAttacker_" + index,
                    ProbeObjectIdBase + index,
                    attackItr,
                    hasBody: false));
                Attackers[index].Team = 1;
                Attackers[index].RelationTeam = 1;
            }

            for (int index = 0; index < Victims.Length; index++)
            {
                Victims[index] = RegisterOwned(new ProbeCharacter(
                    "R07A_HitRecordVictim_" + index,
                    ProbeObjectIdBase + TotalTickCount + index,
                    itr: null,
                    hasBody: true));
                Victims[index].Team = 2;
                Victims[index].RelationTeam = 2;
                Victims[index].Health.HP = 100;
                Victims[index].Health.HPBound = 100;
                Victims[index].Health.HP3 = 100;
                Victims[index].Unk344 = 1;
                Victims[index].FallDamageDiv = 100;
                Victims[index].KillCount = -1;

                RuntimeEntityHandle victimHandle = default;
                Require(
                    Victims[index].Runtime.SlotIndex >
                    Attackers[Attackers.Length - 1].Runtime.SlotIndex &&
                    world.TryGetCurrentRuntimeHandleForDiagnostics(
                        Victims[index].Runtime.SlotIndex,
                        Victims[index],
                        out victimHandle),
                    "The actual-hit fixture did not receive deterministic ordered runtime handles.");
                VictimGenerations[index] = victimHandle.Generation;
            }

            attacker = Attackers[0];
            victim = Victims[0];
            report.attackerSlot = attacker.Runtime.SlotIndex;
            report.attackerStableId = attacker.Runtime.StableId;
            report.victimSlot = victim.Runtime.SlotIndex;
            report.victimStableId = victim.Runtime.StableId;
            report.victimGeneration = VictimGenerations[0];
        }

        private static T RegisterOwned<T>(T entity)
            where T : LF2Entity
        {
            world.Register(entity);
            Require(entity.Runtime?.SlotIndex >= 0,
                (entity?.Name ?? "entity") + " did not receive a runtime slot.");
            OwnedEntities.Add(entity);
            return entity;
        }

        private static void ScheduleTick(bool buildPresentation)
        {
            ArmPairForTick(tickOrdinal);
            expectedTick = driver.CurrentTickIndex + 1;
            rngCallsBeforeTick = world.Rng.CallCount;
            cycleIdBeforeTick = world.BattlePresentation.PublishedHitRecordCycle?.CycleId ?? 0;
            bool accepted;
            if (workerPath)
            {
                accepted = driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                    buildPresentation);
            }
            else
            {
                accepted = driver.StepOneTick(
                    ignorePaused: true,
                    buildPresentation: buildPresentation);
            }

            Require(
                accepted,
                workerPath
                    ? "The production worker rejected the diagnostic full tick: " +
                      driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics
                    : "The production synchronous driver rejected the full tick.");

            phase = ProbePhase.WaitingForTickCompletion;
            if (!workerPath)
                ObserveTickCompletion();
        }

        private static void ArmPairForTick(int ordinal)
        {
            Require(ordinal >= 0 && ordinal < Attackers.Length,
                "The actual-hit fixture selected an invalid pair ordinal.");
            for (int index = 0; index < Attackers.Length; index++)
            {
                int attackerX = index == ordinal
                    ? FixtureX
                    : FixtureX - 100000 - index * 10000;
                int victimX = index == ordinal
                    ? FixtureX + 10
                    : FixtureX - 50000 - index * 10000;
                Attackers[index].SetPosition(attackerX, 0, 0);
                Victims[index].SetPosition(victimX, 0, 0);
            }
            attacker = Attackers[ordinal];
            victim = Victims[ordinal];
        }

        private static void ObserveTickCompletion()
        {
            if (driver.CurrentTickIndex < expectedTick)
                return;
            Require(driver.CurrentTickIndex == expectedTick,
                $"The completed tick skipped the expected index {expectedTick}.");

            bool buildPresentation = tickOrdinal < PublishedTickCount;
            CaptureTickEvidence(buildPresentation);
            completionEditorUpdate = editorUpdates;
            phase = ProbePhase.WaitingForLateFallback;
        }

        private static void CaptureTickEvidence(bool buildPresentation)
        {
            recordOwner = victim.HitRecordCount > 0 ? victim : attacker;
            Require(ReferenceEquals(recordOwner, victim) && attacker.HitRecordCount == 0,
                "The larger-Z/slot actual hit record owner did not remain the victim fixture.");

            int expectedCount = tickOrdinal + 1;
            Require(recordOwner.HitRecordCount == 1,
                $"Tick {expectedTick} current victim produced {recordOwner.HitRecordCount} " +
                "hit records, expected 1.");
            Require(world.Rng.CallCount - rngCallsBeforeTick == 2UL,
                $"Tick {expectedTick} kind0 record consumed " +
                $"{world.Rng.CallCount - rngCallsBeforeTick} RNG calls, expected 2.");

            int[] liveAges = CopyLiveAgesThroughOrdinal(tickOrdinal);
            for (int index = 0; index < liveAges.Length; index++)
            {
                int expectedAge = expectedCount - index;
                Require(liveAges[index] == expectedAge,
                    $"Tick {expectedTick} live age[{index}]={liveAges[index]}, expected {expectedAge}.");
            }

            var evidence = new TickEvidence
            {
                tick = expectedTick,
                buildPresentation = buildPresentation,
                rngCalls = world.Rng.CallCount - rngCallsBeforeTick,
                liveAges = liveAges,
                cycleIdBefore = cycleIdBeforeTick,
                phaseDiagnosticsTick = tickDiagnostics?.LastTickIndex ?? -1,
                renderDispatchTimestampTicks = tickDiagnostics?.GetLastElapsedTimestampTicks(
                    BattleTickPhase.RenderDispatch) ?? 0L,
                framePostProcessTimestampTicks = tickDiagnostics?.GetLastElapsedTimestampTicks(
                    BattleTickPhase.FramePostProcess) ?? 0L,
                lateEntityUpdateTimestampTicks = tickDiagnostics?.GetLastElapsedTimestampTicks(
                    BattleTickPhase.LateEntityUpdate) ?? 0L,
            };
            Require(evidence.phaseDiagnosticsTick == expectedTick,
                "Battle tick diagnostics did not close on the full production tick.");

            BattleHitRecordPresentationCycle cycle =
                world.BattlePresentation.PublishedHitRecordCycle;
            if (buildPresentation)
            {
                Require(cycle != null && cycle.TickIndex == expectedTick &&
                        cycle.CycleId != cycleIdBeforeTick,
                    "The published HitRecord cycle did not belong to the completed tick.");
                Require(cycle.OwnerCount == expectedCount &&
                        cycle.HitRecordCount == expectedCount,
                    "The published HitRecord cycle did not contain every actual producer owner.");

                int[] frozenAges = new int[expectedCount];
                for (int index = 0; index < expectedCount; index++)
                {
                    BattleHitRecordOwnerSnapshot owner = cycle.GetOwner(index);
                    ProbeCharacter expectedOwner = Victims[index];
                    Require(
                        owner.StableId == expectedOwner.Runtime.StableId &&
                        owner.RuntimeSlot == expectedOwner.Runtime.SlotIndex &&
                        owner.Handle.Slot == expectedOwner.Runtime.SlotIndex &&
                        owner.Handle.Generation == VictimGenerations[index] &&
                        owner.HitRecordCount == 1,
                        "The frozen HitRecord owner handle/stable/slot/generation contract changed.");
                    frozenAges[index] = cycle.GetHitRecord(owner.HitRecordStart).Age;
                    int expectedFrozenAge = expectedCount - index - 1;
                    Require(frozenAges[index] == expectedFrozenAge,
                        $"Tick {expectedTick} frozen age[{index}]={frozenAges[index]}, " +
                        $"expected {expectedFrozenAge}.");
                }

                BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
                Require(frame != null && frame.TickIndex == expectedTick &&
                        frame.HitRecordCount == expectedCount,
                    "The published frame did not preserve the actual frozen HitRecord samples.");

                int hitCommandCount = 0;
                if (workerPath)
                {
                    Require(!frame.CommandsMaterialized,
                        "The worker publication must remain a pure, unmaterialized frame.");
                }
                else
                {
                    Require(frame.CommandsMaterialized,
                        "The non-worker central frame did not materialize HitRecord commands.");
                    hitCommandCount = CountHitCommands(
                        frame,
                        tickOrdinal);
                    Require(hitCommandCount == expectedCount,
                        $"The central frame emitted {hitCommandCount} hit commands, " +
                        $"expected {expectedCount}.");
                }

                evidence.cycleIdAfter = cycle.CycleId;
                evidence.cycleTick = cycle.TickIndex;
                evidence.frameTick = frame.TickIndex;
                evidence.ownerCount = cycle.OwnerCount;
                evidence.hitRecordCount = cycle.HitRecordCount;
                evidence.hitCommandCount = hitCommandCount;
                evidence.frozenAges = frozenAges;
            }
            else
            {
                Require(cycle != null && cycle.CycleId == cycleIdBeforeTick,
                    "A no-publication full tick unexpectedly replaced the frozen cycle.");
                evidence.cycleIdAfter = cycle.CycleId;
                evidence.cycleTick = cycle.TickIndex;
                evidence.frameTick = world.BattlePresentation.PublishedFrame?.TickIndex ?? -1;
                evidence.ownerCount = cycle.OwnerCount;
                evidence.hitRecordCount = cycle.HitRecordCount;
                evidence.hitCommandCount = 0;
                evidence.frozenAges = Array.Empty<int>();
            }

            TickSamples[tickOrdinal] = evidence;
        }

        private static int CountHitCommands(
            BattlePresentationFrame frame,
            int lastVictimOrdinal)
        {
            int count = 0;
            for (int commandIndex = 0; commandIndex < frame.CommandCount; commandIndex++)
            {
                BattleRenderCommand command = frame.GetCommand(commandIndex);
                if (command.Type != BattleRenderCommandType.HitRecord ||
                    !IsExpectedVictimStableId(command.StableId, lastVictimOrdinal))
                {
                    continue;
                }

                Require(BattleCommonVisualCatalog.TryResolveSparkAge(
                            command.EffectivePic,
                            out int expectedPic) && expectedPic == command.EffectivePic,
                    "The central HitRecord command no longer carries the C++ spark age/pic mapping.");
                count++;
            }
            return count;
        }

        private static bool IsExpectedVictimStableId(
            int stableId,
            int lastVictimOrdinal)
        {
            for (int index = 0; index <= lastVictimOrdinal; index++)
            {
                if (Victims[index].Runtime.StableId == stableId)
                    return true;
            }
            return false;
        }

        private static int[] CopyLiveAgesThroughOrdinal(int lastVictimOrdinal)
        {
            var ages = new int[lastVictimOrdinal + 1];
            for (int index = 0; index < Victims.Length; index++)
            {
                int expectedCount = index <= lastVictimOrdinal ? 1 : 0;
                Require(Victims[index].HitRecordCount == expectedCount,
                    $"Victim pair {index} has {Victims[index].HitRecordCount} records, " +
                    $"expected {expectedCount}.");
                if (expectedCount != 0)
                    ages[index] = Victims[index].GetHitRecordAge(0);
            }
            return ages;
        }

        private static void ObserveLateFallback()
        {
            if (editorUpdates <= completionEditorUpdate + 1)
                return;
            if (workerPath && driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            bool buildPresentation = tickOrdinal < PublishedTickCount;
            if (workerPath && buildPresentation)
            {
                var plan = world.CurrentPixelFramePlan;
                BattlePresentationFrame materializedFrame = plan.CapturedFrame;
                if (!plan.IsValid || plan.SimulationTick < expectedTick ||
                    materializedFrame == null || materializedFrame.TickIndex < expectedTick)
                {
                    return;
                }

                Require(plan.SimulationTick == expectedTick &&
                        materializedFrame.TickIndex == expectedTick &&
                        materializedFrame.CommandsMaterialized,
                    "The production central host did not materialize the completed worker publication.");
                int expectedCount = tickOrdinal + 1;
                Require(materializedFrame.HitRecordCount == expectedCount,
                    "The materialized central frame changed the frozen HitRecord count.");
                int hitCommandCount = CountHitCommands(
                    materializedFrame,
                    tickOrdinal);
                Require(hitCommandCount == expectedCount,
                    $"The materialized central frame emitted {hitCommandCount} hit commands, " +
                    $"expected {expectedCount}.");
                TickSamples[tickOrdinal].frameTick = materializedFrame.TickIndex;
                TickSamples[tickOrdinal].hitCommandCount = hitCommandCount;
            }

            int[] afterLate = CopyLiveAgesThroughOrdinal(tickOrdinal);
            int[] expected = TickSamples[tickOrdinal].liveAges;
            Require(ArraysEqual(afterLate, expected),
                $"Late/worker fallback advanced tick {expectedTick} HitRecords more than once.");
            TickSamples[tickOrdinal].lateFallbackIdempotent = true;

            if (tickOrdinal == 1)
            {
                BattleManagedMemoryBoundary boundary = driver.ManagedMemoryBoundary;
                warmTickAllocationViolationCount = boundary.AllocationViolationCount;
                warmPresentationAllocationViolationCount =
                    boundary.PresentationAllocationViolationCount;
            }

            if (tickOrdinal == TotalTickCount - 1)
            {
                BattleManagedMemoryBoundary boundary = driver.ManagedMemoryBoundary;
                report.warmTickAllocationViolationDelta =
                    boundary.AllocationViolationCount - warmTickAllocationViolationCount;
                report.warmPresentationAllocationViolationDelta =
                    boundary.PresentationAllocationViolationCount -
                    warmPresentationAllocationViolationCount;
                Require(report.warmTickAllocationViolationDelta == 0 &&
                        report.warmPresentationAllocationViolationDelta == 0,
                    "Warmed published/no-publication ticks crossed the managed-memory boundary.");
                FinishSuccess();
                return;
            }

            tickOrdinal++;
            ScheduleTick(buildPresentation: tickOrdinal < PublishedTickCount);
        }

        private static void FinishSuccess()
        {
            report.status = "PASS";
            report.message =
                "Actual kind0 collision produced frozen HitRecords, RenderDispatch advanced " +
                "live owners exactly once, Late remained idempotent, next-tick append/RNG and " +
                "CentralOnly no-publication lifecycle passed.";
            report.endTick = driver.CurrentTickIndex;
            report.ticks = (TickEvidence[])TickSamples.Clone();
            Cleanup();
            CaptureFinalState();
            Require(report.cleanupCompleted,
                "R07A cleanup did not restore the live-world baseline: " + report.cleanupErrors);
            WriteResult(report);
            Debug.Log(
                $"[BattleHitRecordWritebackPlayModeProbe] PASS: " +
                $"ticks={report.startTick}->{report.endTick}, worker={report.workerPath}.");
            StopObservation();
        }

        private static void Fail(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.message = message;
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.ticks = (TickEvidence[])TickSamples.Clone();
            Cleanup();
            CaptureFinalState();
            WriteResult(report);
            Debug.LogError("[BattleHitRecordWritebackPlayModeProbe] FAIL: " + message);
            StopObservation();
        }

        private static void Cleanup()
        {
            if (!baselineCaptured || world == null)
                return;

            try
            {
                for (int index = OwnedEntities.Count - 1; index >= 0; index--)
                {
                    LF2Entity entity = OwnedEntities[index];
                    if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                        world.Unregister(entity);
                }
                OwnedEntities.Clear();
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

            if (enabledTickDiagnostics)
            {
                world.DisableBattleTickPhaseDiagnosticsForDiagnostics();
                enabledTickDiagnostics = false;
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
            report.rngRestored = world?.Rng != null &&
                                 world.Rng.State == baselineRngState &&
                                 world.Rng.CallCount == baselineRngCalls;
            report.statsRestored = ArraysEqual(world?.KillStats, baselineKillStats) &&
                                   ArraysEqual(world?.DamageStats, baselineDamageStats);
            report.soundsRestored = PendingSoundsEqual(world?.PendingSounds, BaselineSounds);
            report.presentationOwnerCleared =
                world?.BattlePresentation?.PublishedHitRecordCycle != null &&
                world.BattlePresentation.PublishedHitRecordCycle.HitRecordCount == 0;
            report.pauseRestored = driver == null || driver.IsPaused == previousPaused;
            report.cleanupCompleted = baselineCaptured &&
                                      string.IsNullOrEmpty(report.cleanupErrors) &&
                                      report.finalObjectCount == baselineObjectCount &&
                                      report.finalClaimedSlots == baselineClaimedSlots &&
                                      report.finalObjectPoolActive == baselineObjectPoolActive &&
                                      report.finalLogicPoolActive == baselineLogicPoolActive &&
                                      report.rngRestored && report.statsRestored &&
                                      report.soundsRestored && report.presentationOwnerCleared &&
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
            if (destination == null || source == null)
                return;
            Array.Copy(source, destination, Math.Min(source.Length, destination.Length));
        }

        private static bool ArraysEqual(int[] left, int[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            for (int index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                    return false;
            }
            return true;
        }

        private static bool PendingSoundsEqual(
            IList<PendingSoundEvent> left,
            IList<PendingSoundEvent> right)
        {
            if (left == null || right == null || left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
            {
                if (left[index].Cue != right[index].Cue ||
                    left[index].WorldX != right[index].WorldX ||
                    left[index].Tick != right[index].Tick)
                {
                    return false;
                }
            }
            return true;
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
            Debug.LogError("[BattleHitRecordWritebackPlayModeProbe] FAIL: " + message);
        }

        private static void WriteResult(ProbeReport probeReport)
        {
            string path = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                ResultRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(probeReport, true));
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
            objectPool = null;
            attacker = null;
            victim = null;
            recordOwner = null;
            tickDiagnostics = null;
            report = null;
            phase = ProbePhase.WaitingForSafeBoundary;
            editorUpdates = 0;
            completionEditorUpdate = 0;
            expectedTick = 0;
            tickOrdinal = 0;
            cycleIdBeforeTick = 0;
            rngCallsBeforeTick = 0;
            baselineKillStats = null;
            baselineDamageStats = null;
            baselineRngState = 0;
            baselineRngCalls = 0;
            baselineObjectCount = 0;
            baselineClaimedSlots = 0;
            baselineObjectPoolActive = 0;
            baselineLogicPoolActive = 0;
            warmTickAllocationViolationCount = 0;
            warmPresentationAllocationViolationCount = 0;
            previousPaused = false;
            workerPath = false;
            enabledTickDiagnostics = false;
            baselineCaptured = false;
            running = false;
            OwnedEntities.Clear();
            BaselineSounds.Clear();
            Array.Clear(Attackers, 0, Attackers.Length);
            Array.Clear(Victims, 0, Victims.Length);
            Array.Clear(VictimGenerations, 0, VictimGenerations.Length);
            Array.Clear(TickSamples, 0, TickSamples.Length);
        }

        private sealed class ProbeCharacter : LF2Character
        {
            public ProbeCharacter(
                string name,
                int objectId,
                InteractionArea itr,
                bool hasBody)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(
                    objectId,
                    BuildCharacterData(name, itr, hasBody)));
                ImmediateFrame(0);
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 100;
                Health.HPBound = 100;
                Health.HP3 = 100;
                KillCount = -1;
                AiControlled = false;
            }

            public void SetPosition(int x, int y, int z)
            {
                Runtime.SetPosition(x, y, z);
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }

            private static LF2CharacterData BuildCharacterData(
                string name,
                InteractionArea itr,
                bool hasBody)
            {
                int[] frameIds =
                {
                    0, 18, 19, 20, 30, 35, 36, 180, 181, 182, 186, 200, 203, 219,
                };
                var frames = new List<LF2FrameData>(frameIds.Length);
                for (int index = 0; index < frameIds.Length; index++)
                {
                    int frameId = frameIds[index];
                    frames.Add(BuildFrame(
                        frameId,
                        frameId == 0 ? LF2States.Standing : LF2States.Falling,
                        hasBody,
                        frameId == 0 ? itr : null));
                }
                return new LF2CharacterData
                {
                    name = name,
                    type_sub = (int)LF2ObjectType.Character,
                    frames = frames,
                };
            }

            private static LF2FrameData BuildFrame(
                int frameId,
                int state,
                bool hasBody,
                InteractionArea itr)
            {
                var frame = new LF2FrameData
                {
                    frameId = frameId,
                    state = state,
                    wait = 10000,
                    next = frameId,
                    pic = 999,
                    centerx = 0,
                    centery = 0,
                };
                if (hasBody)
                {
                    frame.bodies.Add(new BodyBox
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                    });
                }
                if (itr != null)
                    frame.itrs.Add(itr);
                return frame;
            }
        }

        private enum ProbePhase
        {
            WaitingForSafeBoundary,
            WaitingForTickCompletion,
            WaitingForLateFallback,
        }

        [Serializable]
        private sealed class TickEvidence
        {
            public int tick;
            public bool buildPresentation;
            public ulong rngCalls;
            public int cycleIdBefore;
            public int cycleIdAfter;
            public int cycleTick;
            public int frameTick;
            public int ownerCount;
            public int hitRecordCount;
            public int hitCommandCount;
            public int phaseDiagnosticsTick;
            public long renderDispatchTimestampTicks;
            public long framePostProcessTimestampTicks;
            public long lateEntityUpdateTimestampTicks;
            public int[] frozenAges;
            public int[] liveAges;
            public bool lateFallbackIdempotent;
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public bool workerPath;
            public int attackerSlot;
            public int attackerStableId;
            public int victimSlot;
            public int victimStableId;
            public uint victimGeneration;
            public uint baselineRngState;
            public ulong baselineRngCalls;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int baselineObjectPoolActive;
            public int baselineLogicPoolActive;
            public int warmTickAllocationViolationDelta;
            public int warmPresentationAllocationViolationDelta;
            public TickEvidence[] ticks;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public bool rngRestored;
            public bool statsRestored;
            public bool soundsRestored;
            public bool presentationOwnerCleared;
            public bool pauseRestored;
            public bool cleanupCompleted;
            public string cleanupErrors;
        }
    }
}
#endif
