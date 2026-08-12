#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleHitExecutionPlanEditorTests
    {
        [Test]
        public void DefaultMode_DoesNotCaptureOrChangeRuntime()
        {
            Scenario scenario = CreateScenario();
            BattleExtendedChecksumSnapshot before =
                scenario.World.CaptureExtendedChecksumSnapshot(9);
            uint rngBefore = scenario.World.Rng.State;
            ulong rngCallsBefore = scenario.World.Rng.CallCount;

            scenario.World.CaptureBattleHitExecutionPlanPass(
                9,
                BattleHitExecutionPass.Character);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.Mode, Is.EqualTo(BattleHitExecutionPlanMode.Disabled));
            Assert.That(diagnostics.CharacterPassCaptureCount, Is.Zero);
            Assert.That(diagnostics.PlannedCandidateCount, Is.Zero);
            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(0, out _),
                Is.False);
            Assert.That(scenario.World.Rng.State, Is.EqualTo(rngBefore));
            Assert.That(scenario.World.Rng.CallCount, Is.EqualTo(rngCallsBefore));
            Assert.That(
                scenario.World.CaptureExtendedChecksumSnapshot(9).OverallChecksum,
                Is.EqualTo(before.OverallChecksum));
        }

        [Test]
        public void CharacterAndObjectPasses_CaptureExactFrozenSlotOrder()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCapture);
            BattleExtendedChecksumSnapshot before =
                scenario.World.CaptureExtendedChecksumSnapshot(10);
            uint rngBefore = scenario.World.Rng.State;
            ulong rngCallsBefore = scenario.World.Rng.CallCount;

            scenario.World.CaptureBattleHitExecutionPlanPass(
                10,
                BattleHitExecutionPass.Character);
            scenario.World.CaptureBattleHitExecutionPlanPass(
                10,
                BattleHitExecutionPass.Object);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.FailureCount, Is.Zero);
            Assert.That(diagnostics.CharacterPassCaptureCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObjectPassCaptureCount, Is.EqualTo(1));
            Assert.That(diagnostics.PlannedAttackerCount, Is.EqualTo(2));
            Assert.That(diagnostics.PlannedCandidateCount, Is.EqualTo(2));

            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    0,
                    out BattleHitExecutionPlanEntryView characterEntry),
                Is.True);
            Assert.That(characterEntry.Pass, Is.EqualTo(BattleHitExecutionPass.Character));
            Assert.That(characterEntry.AttackerHandle.Slot, Is.EqualTo(0));
            Assert.That(characterEntry.AttackerPrevFrame2, Is.Zero);
            Assert.That(characterEntry.CandidateOrdinal, Is.Zero);
            Assert.That(characterEntry.TargetSlot, Is.EqualTo(1));
            Assert.That(characterEntry.TargetHandleSnapshot.Slot, Is.EqualTo(1));
            Assert.That(characterEntry.ItrIndex, Is.Zero);
            Assert.That(characterEntry.ItrKind, Is.Zero);
            Assert.That(characterEntry.SourceItrFingerprint, Is.Not.Zero);

            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    1,
                    out BattleHitExecutionPlanEntryView objectEntry),
                Is.True);
            Assert.That(objectEntry.Pass, Is.EqualTo(BattleHitExecutionPass.Object));
            Assert.That(objectEntry.AttackerHandle.Slot, Is.EqualTo(20));
            Assert.That(objectEntry.CandidateOrdinal, Is.Zero);
            Assert.That(objectEntry.TargetSlot, Is.EqualTo(21));
            Assert.That(objectEntry.TargetHandleSnapshot.Slot, Is.EqualTo(21));
            Assert.That(objectEntry.ItrIndex, Is.Zero);

            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(2, out _),
                Is.False);
            Assert.That(scenario.World.Rng.State, Is.EqualTo(rngBefore));
            Assert.That(scenario.World.Rng.CallCount, Is.EqualTo(rngCallsBefore));
            Assert.That(
                scenario.World.CaptureExtendedChecksumSnapshot(10).OverallChecksum,
                Is.EqualTo(before.OverallChecksum));
        }

        [Test]
        public void CandidatePlan_RemainsSlotBasedWhenGeometryChangesAfterCollection()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCapture);
            scenario.CharacterVictim.Runtime.SetPosition(10000, 0, 0);
            scenario.CharacterVictim.Runtime.SyncIntegerPosition();
            scenario.CharacterVictim.RelationTeam = scenario.CharacterAttacker.RelationTeam;

            scenario.World.CaptureBattleHitExecutionPlanPass(
                11,
                BattleHitExecutionPass.Character);

            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    0,
                    out BattleHitExecutionPlanEntryView entry),
                Is.True);
            Assert.That(entry.TargetSlot, Is.EqualTo(1));
            Assert.That(entry.TargetHandleSnapshot.Slot, Is.EqualTo(1));
            Assert.That(
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics
                    .CurrentTickPlanValid,
                Is.True);
        }

        [Test]
        public void DuplicatePassCapture_FailsPlanClosedWithoutAddingEntries()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCapture);
            scenario.World.CaptureBattleHitExecutionPlanPass(
                12,
                BattleHitExecutionPass.Character);
            long capturedCount = scenario.World
                .BattleHitExecutionPlanDiagnosticsForDiagnostics
                .PlannedCandidateCount;

            scenario.World.CaptureBattleHitExecutionPlanPass(
                12,
                BattleHitExecutionPass.Character);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(BattleHitExecutionPlanFailureReason.DuplicatePassCapture));
            Assert.That(diagnostics.PlannedCandidateCount, Is.EqualTo(capturedCount));
        }

        [Test]
        public void EndedCandidateVisibility_FailsPlanClosed()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCapture);
            scenario.World.EndCollisionCandidateConsumption();

            scenario.World.CaptureBattleHitExecutionPlanPass(
                13,
                BattleHitExecutionPass.Character);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.CandidateSourceUnavailable));
            Assert.That(diagnostics.PlannedCandidateCount, Is.Zero);
        }

        [Test]
        public void WarmedShadowCapture_AllocatesNoManagedMemory()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCapture);
            for (int tick = 20; tick < 52; tick++)
            {
                scenario.World.CaptureBattleHitExecutionPlanPass(
                    tick,
                    BattleHitExecutionPass.Character);
                scenario.World.CaptureBattleHitExecutionPlanPass(
                    tick,
                    BattleHitExecutionPass.Object);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 100; tick < 612; tick++)
            {
                scenario.World.CaptureBattleHitExecutionPlanPass(
                    tick,
                    BattleHitExecutionPass.Character);
                scenario.World.CaptureBattleHitExecutionPlanPass(
                    tick,
                    BattleHitExecutionPass.Object);
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(
                after - before,
                Is.Zero,
                $"ShadowCapture allocated {after - before} bytes. " +
                DescribeDiagnostics(
                    scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics));
            Assert.That(
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics
                    .CurrentTickPlanValid,
                Is.True);
        }

        [Test]
        public void ShadowCompare_ObservesLegacyCharacterAndObjectConsumptionInPlanOrder()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(700);
            scenario.World.ObjectInteractionTickAll(700);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservationPassCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedCandidateCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedPreprocessCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedDispositionCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedConsumeEffectsCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedDispatchCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedAbortTerminationCount, Is.Zero);
            Assert.That(diagnostics.SkippedCandidateCountAfterAbort, Is.Zero);
            Assert.That(diagnostics.ObservationMismatchCount, Is.Zero);
            Assert.That(diagnostics.FailureCount, Is.Zero);
            Assert.That(scenario.CharacterVictim.Health.HP, Is.LessThan(100));
            Assert.That(scenario.ObjectVictim.Health.HP, Is.LessThan(100));
        }

        [Test]
        public void WarmedShadowCompareCandidateRead_AllocatesNoManagedMemory()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            for (int tick = 800; tick < 832; tick++)
            {
                ObserveSingleCandidate(
                    scenario.World,
                    scenario.CharacterAttacker,
                    tick);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 900; tick < 1412; tick++)
            {
                ObserveSingleCandidate(
                    scenario.World,
                    scenario.CharacterAttacker,
                    tick);
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(
                after - before,
                Is.Zero,
                $"ShadowCompare candidate observation allocated {after - before} bytes. " +
                DescribeDiagnostics(
                    scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics));
            Assert.That(
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics
                    .ObservationMismatchCount,
                Is.Zero);
        }

        [Test]
        public void ShadowCompare_MismatchedLegacyReadFailsPlanClosed()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            scenario.World.CaptureBattleHitExecutionPlanPass(
                720,
                BattleHitExecutionPass.Character);
            Assert.That(
                scenario.World.SceneQuery.TryGetCollisionCandidateRange(
                    scenario.CharacterAttacker,
                    out CollisionCandidateRange candidates),
                Is.True);
            Assert.That(candidates.TryGet(0, out SceneQueryHit hit), Is.True);
            Assert.That(
                scenario.World.TryGetCurrentRuntimeHandle(
                    scenario.CharacterAttacker.Runtime.SlotIndex,
                    scenario.CharacterAttacker,
                    out RuntimeEntityHandle attackerHandle),
                Is.True);
            Assert.That(
                scenario.World.BeginBattleHitExecutionPlanLegacyObservation(
                    720,
                    BattleHitExecutionPass.Character),
                Is.True);

            var mismatched = new SceneQueryHit(
                hit.Target,
                hit.TargetSlot,
                hit.BodyX,
                hit.ItrIndex + 1,
                hit.RuntimeItr,
                hit.ZeroAttackerHpOnConsume,
                hit.ReleaseHeavyHeldTargetOnConsume);
            scenario.World.ObserveBattleHitExecutionPlanLegacyCandidateRead(
                attackerHandle,
                0,
                mismatched);
            scenario.World.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationEntryMismatch));
            Assert.That(diagnostics.ObservationMismatchCount, Is.EqualTo(1));
        }

        [Test]
        public void ShadowCompare_ProjectsCurrentKind9PreprocessAtCandidateRead()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 9;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(730);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedCandidateCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObservedPreprocessCount, Is.EqualTo(1));
            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    0,
                    out BattleHitExecutionPlanEntryView entry),
                Is.True);
            Assert.That(entry.PreprocessObserved, Is.True);
            Assert.That(entry.DispositionObserved, Is.True);
            Assert.That(
                entry.ObservedDisposition,
                Is.EqualTo(entry.ExpectedDisposition));
            Assert.That(
                entry.ExpectedDisposition,
                Is.EqualTo(BattleHitCandidateDisposition.Damage));
            Assert.That(
                entry.ObservedResolvedItrFingerprint,
                Is.EqualTo(entry.ExpectedResolvedItrFingerprint));
            Assert.That(entry.ExpectedZeroAttackerHpAfterPreprocess, Is.True);
            Assert.That(entry.ObservedZeroAttackerHpAfterPreprocess, Is.True);
            Assert.That(
                entry.ExpectedReleaseHeavyHeldTargetAfterPreprocess,
                Is.False);
            Assert.That(
                entry.ObservedReleaseHeavyHeldTargetAfterPreprocess,
                Is.False);
            Assert.That(entry.ConsumeEffectsObserved, Is.True);
            Assert.That(
                entry.ObservedConsumeEffectsFingerprint,
                Is.EqualTo(entry.ExpectedConsumeEffectsFingerprint));
            Assert.That(
                entry.ObservedRngStateAfterConsume,
                Is.EqualTo(entry.ExpectedRngStateAfterConsume));
            Assert.That(
                entry.ObservedRngCallCountAfterConsume,
                Is.EqualTo(entry.ExpectedRngCallCountAfterConsume));
            Assert.That(scenario.CharacterAttacker.Health.HP, Is.Zero);
        }

        [Test]
        public void DirectConsumeEffects_AppliesEncodedZeroHpFlag()
        {
            Scenario scenario = CreateScenario();
            SceneQueryHit hit = new SceneQueryHit(
                scenario.CharacterVictim,
                0,
                0,
                scenario.CharacterAttacker.GetCollisionFrameData().itrs[0],
                zeroAttackerHpOnConsume: true,
                releaseHeavyHeldTargetOnConsume: false);

            Assert.That(hit.ZeroAttackerHpOnConsume, Is.True);
            scenario.CharacterAttacker.ApplyReleaseSceneQueryConsumeEffectsInternal(hit);

            Assert.That(scenario.CharacterAttacker.Health.HP, Is.Zero);
        }

        [Test]
        public void DirectShadowObservation_Kind9ConsumeEffectsMatch()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 9;
            scenario.World.CaptureCollisionFrameSnapshotsAll();
            scenario.World.CollectCollisionCandidatesAll();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            ObserveSingleCandidateWithConsumeEffects(
                scenario.World,
                scenario.CharacterAttacker,
                735);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(scenario.CharacterAttacker.Health.HP, Is.Zero);
        }

        [Test]
        public void DirectConsumeEffects_HeavyHeldReleaseAppliesEncodedEffects()
        {
            Scenario scenario = CreateScenario();
            TypedCharacter heldTarget = CreateEntity(
                scenario.World,
                "HitPlanDirectHeavyHeldTarget",
                7131,
                2,
                LF2ObjectType.HeavyWeapon,
                scenario.CharacterVictim.Team,
                10,
                hasItr: false,
                hasBody: false);
            scenario.CharacterVictim.Runtime.LinkState = 2;
            scenario.CharacterVictim.Runtime.TargetSlotIndex = 2;
            heldTarget.Runtime.LinkState = -2;
            heldTarget.Runtime.HolderStableId = 1;
            scenario.World.Rng.Seed(0x12345678u);
            SceneQueryHit hit = new SceneQueryHit(
                scenario.CharacterVictim,
                0,
                0,
                scenario.CharacterAttacker.GetCollisionFrameData().itrs[0],
                zeroAttackerHpOnConsume: false,
                releaseHeavyHeldTargetOnConsume: true);

            scenario.CharacterAttacker.ApplyReleaseSceneQueryConsumeEffectsInternal(hit);

            Assert.That(scenario.CharacterVictim.Runtime.LinkState, Is.Zero);
            Assert.That(heldTarget.Runtime.LinkState, Is.Zero);
            Assert.That(heldTarget.Frame.N, Is.InRange(0, 5));
            Assert.That(heldTarget.Runtime.Vy, Is.EqualTo(-1.0));
            Assert.That(scenario.World.GetRawRestVrest(1, 0), Is.EqualTo(45));
            Assert.That(scenario.World.GetRawRestVrest(1, 2), Is.EqualTo(30));
            Assert.That(scenario.World.Rng.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void ShadowCompare_HeavyHeldReleaseMatchesVrestLinkFrameVelocityAndRng()
        {
            Scenario scenario = CreateScenario();
            TypedCharacter heldTarget = CreateEntity(
                scenario.World,
                "HitPlanHeavyHeldTarget",
                7130,
                2,
                LF2ObjectType.HeavyWeapon,
                scenario.CharacterVictim.Team,
                10,
                hasItr: false,
                hasBody: false);
            scenario.CharacterVictim.Runtime.LinkState = 2;
            scenario.CharacterVictim.Runtime.TargetSlotIndex = 2;
            heldTarget.Runtime.LinkState = -2;
            heldTarget.Runtime.HolderStableId = 1;
            scenario.World.CaptureCollisionFrameSnapshotsAll();
            scenario.World.CollectCollisionCandidatesAll();
            scenario.World.Rng.Seed(0x12345678u);
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(740);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedConsumeEffectsCount, Is.EqualTo(1));
            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    0,
                    out BattleHitExecutionPlanEntryView entry),
                Is.True);
            Assert.That(entry.ExpectedReleaseHeavyHeldTargetAfterPreprocess, Is.True);
            Assert.That(entry.ObservedReleaseHeavyHeldTargetAfterPreprocess, Is.True);
            Assert.That(entry.ConsumeEffectsObserved, Is.True);
            Assert.That(
                entry.ObservedConsumeEffectsFingerprint,
                Is.EqualTo(entry.ExpectedConsumeEffectsFingerprint));
            Assert.That(
                entry.ObservedRngStateAfterConsume,
                Is.EqualTo(entry.ExpectedRngStateAfterConsume));
            Assert.That(
                entry.ObservedRngCallCountAfterConsume,
                Is.EqualTo(entry.ExpectedRngCallCountAfterConsume));
            Assert.That(scenario.CharacterVictim.Runtime.LinkState, Is.Zero);
            Assert.That(heldTarget.Runtime.LinkState, Is.Zero);
            Assert.That(heldTarget.Frame.N, Is.InRange(0, 5));
            Assert.That(heldTarget.Runtime.Vy, Is.EqualTo(-1.0));
            Assert.That(scenario.World.GetRawRestVrest(1, 0), Is.EqualTo(45));
            Assert.That(scenario.World.GetRawRestVrest(1, 2), Is.EqualTo(30));
        }

        [Test]
        public void WarmedShadowCompareConsumeEffectsObservation_AllocatesNoManagedMemory()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            for (int tick = 1500; tick < 1532; tick++)
            {
                ObserveSingleCandidateWithConsumeEffects(
                    scenario.World,
                    scenario.CharacterAttacker,
                    tick);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 1600; tick < 2112; tick++)
            {
                ObserveSingleCandidateWithConsumeEffects(
                    scenario.World,
                    scenario.CharacterAttacker,
                    tick);
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(
                after - before,
                Is.Zero,
                $"Consume-effects observation allocated {after - before} bytes. " +
                DescribeDiagnostics(
                    scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics));
            Assert.That(
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics
                    .ObservationMismatchCount,
                Is.Zero);
        }

        [Test]
        public void ShadowCompare_MismatchedPreprocessFailsPlanClosed()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            scenario.World.CaptureBattleHitExecutionPlanPass(
                731,
                BattleHitExecutionPass.Character);
            Assert.That(
                scenario.World.BeginBattleHitExecutionPlanLegacyObservation(
                    731,
                    BattleHitExecutionPass.Character),
                Is.True);
            Assert.That(
                scenario.World.SceneQuery.TryGetCollisionCandidateRange(
                    scenario.CharacterAttacker,
                    out CollisionCandidateRange candidates),
                Is.True);
            Assert.That(candidates.TryGet(0, out SceneQueryHit hit), Is.True);

            scenario.World.ObserveBattleHitExecutionPlanLegacyPreprocess(
                scenario.CharacterAttacker,
                hit.ResolveCurrentTarget(scenario.World),
                new InteractionArea { kind = 14 },
                false,
                false);
            scenario.World.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationPreprocessMismatch));
            Assert.That(diagnostics.ObservedPreprocessCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObservationMismatchCount, Is.EqualTo(1));
        }

        [TestCase(0, BattleHitCandidateDisposition.Damage)]
        [TestCase(9, BattleHitCandidateDisposition.Damage)]
        [TestCase(6, BattleHitCandidateDisposition.HitConfirm)]
        [TestCase(8, BattleHitCandidateDisposition.Kind8)]
        [TestCase(14, BattleHitCandidateDisposition.Kind14)]
        [TestCase(15, BattleHitCandidateDisposition.Kind15Or16)]
        [TestCase(16, BattleHitCandidateDisposition.Kind15Or16)]
        [TestCase(10, BattleHitCandidateDisposition.Kind10Or11)]
        [TestCase(11, BattleHitCandidateDisposition.Kind10Or11)]
        [TestCase(1, BattleHitCandidateDisposition.Kind1Grab)]
        [TestCase(3, BattleHitCandidateDisposition.Kind3Grab)]
        [TestCase(2, BattleHitCandidateDisposition.Pickup)]
        [TestCase(7, BattleHitCandidateDisposition.Pickup)]
        [TestCase(4, BattleHitCandidateDisposition.Unsupported)]
        [TestCase(5, BattleHitCandidateDisposition.Unsupported)]
        [TestCase(99, BattleHitCandidateDisposition.Unsupported)]
        public void ShadowCompare_ResolvedKindDispositionMatchesAuthoritySwitch(
            int sourceKind,
            BattleHitCandidateDisposition expectedDisposition)
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = sourceKind;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            ObserveSingleCandidate(
                scenario.World,
                scenario.CharacterAttacker,
                742 + sourceKind);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedDispositionCount, Is.EqualTo(1));
            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    0,
                    out BattleHitExecutionPlanEntryView entry),
                Is.True);
            Assert.That(entry.DispositionObserved, Is.True);
            Assert.That(entry.ExpectedDisposition, Is.EqualTo(expectedDisposition));
            Assert.That(entry.ObservedDisposition, Is.EqualTo(expectedDisposition));
            Assert.That(
                entry.ExpectedResolvedItrKind,
                Is.EqualTo(entry.ObservedResolvedItrKind));
        }

        [Test]
        public void ShadowCompare_UnconvertedKind4IsAuthorityNoOp()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.WeaponCount = 0;
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 4;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(743);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedDispositionCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObservedConsumeEffectsCount, Is.Zero);
            Assert.That(diagnostics.ObservedDispatchCount, Is.Zero);
            Assert.That(scenario.CharacterVictim.Health.HP, Is.EqualTo(100));
            Assert.That(
                scenario.World.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    0,
                    out BattleHitExecutionPlanEntryView entry),
                Is.True);
            Assert.That(
                entry.ExpectedDisposition,
                Is.EqualTo(BattleHitCandidateDisposition.Unsupported));
            Assert.That(
                entry.ObservedDisposition,
                Is.EqualTo(BattleHitCandidateDisposition.Unsupported));
        }

        [Test]
        public void ShadowCompare_WrongDispositionFailsPlanClosed()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            scenario.World.CaptureBattleHitExecutionPlanPass(
                744,
                BattleHitExecutionPass.Character);
            Assert.That(
                scenario.World.BeginBattleHitExecutionPlanLegacyObservation(
                    744,
                    BattleHitExecutionPass.Character),
                Is.True);
            Assert.That(
                scenario.World.SceneQuery.TryGetCollisionCandidateRange(
                    scenario.CharacterAttacker,
                    out CollisionCandidateRange candidates),
                Is.True);
            Assert.That(candidates.TryGet(0, out SceneQueryHit hit), Is.True);
            LF2Entity target = hit.ResolveCurrentTarget(scenario.World);
            LF2FrameData frame = scenario.CharacterAttacker.GetCollisionFrameData();
            InteractionArea resolvedItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                scenario.CharacterAttacker,
                target,
                frame,
                frame.itrs[hit.ItrIndex],
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            scenario.World.ObserveBattleHitExecutionPlanLegacyPreprocess(
                scenario.CharacterAttacker,
                target,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);

            scenario.World.ObserveBattleHitExecutionPlanLegacyDisposition(
                scenario.CharacterAttacker,
                target,
                resolvedItr,
                BattleHitCandidateDisposition.Unsupported);
            scenario.World.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationDispositionMismatch));
            Assert.That(diagnostics.ObservedDispositionCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObservationMismatchCount, Is.EqualTo(1));
        }

        [TestCase(0, false, 10, 0, LF2StandardFrames.Injured, 20, 1.1, "SFX_001")]
        [TestCase(0, false, 30, 0, LF2StandardFrames.Injured4, 40, 1.1, "SFX_001")]
        [TestCase(0, false, 50, 0, LF2StandardFrames.Injured6, 60, 1.1, "SFX_001")]
        [TestCase(0, false, 70, 0, LF2StandardFrames.FallingBack, 0, 1.1, "SFX_001")]
        [TestCase(9, true, 10, 0, LF2StandardFrames.Injured, 20, 1.1, "SFX_001")]
        [TestCase(0, false, 10, 1, LF2StandardFrames.Injured, 20, 1.1, "SFX_002")]
        [TestCase(0, false, 70, 1, LF2StandardFrames.FallingBack, 0, 1.1, "SFX_002")]
        [TestCase(0, false, 10, 2, LF2StandardFrames.Injured, 20, 1.1, "SFX_006")]
        [TestCase(0, false, 10, 3, LF2StandardFrames.Injured, 20, 1.1, "SFX_010")]
        [TestCase(0, false, 10, 5, LF2StandardFrames.Injured, 20, 1.1, "SFX_004")]
        [TestCase(0, false, 10, 20, LF2StandardFrames.Injured, 20, 1.1, "SFX_001")]
        [TestCase(0, false, 10, 21, LF2StandardFrames.Injured, 20, 1.1, "SFX_001")]
        [TestCase(0, false, 10, 22, LF2StandardFrames.Injured, 20, -0.9, "SFX_001")]
        [TestCase(0, false, 10, 23, LF2StandardFrames.Injured, 20, -0.9, "SFX_001")]
        [TestCase(0, false, 10, 30, LF2StandardFrames.Injured, 20, 1.1, "SFX_001")]
        public void ShadowCompare_StandardCharacterDamageWriterEffectMatchesAuthorityState(
            int sourceKind,
            bool expectAttackerHpZero,
            int fall,
            int effect,
            int expectedFrame,
            int expectedFall,
            double expectedKnockbackVx,
            string expectedEffectCue)
        {
            Scenario scenario = CreateScenario();
            InteractionArea itr =
                scenario.CharacterAttacker.GetCollisionFrameData().itrs[0];
            itr.kind = sourceKind;
            itr.injury = 10;
            itr.fall = fall;
            itr.dvx = 1;
            itr.effect = effect;
            itr.arest = 2;
            itr.vrest = 3;
            scenario.CharacterVictim.Health.HP = 100;
            scenario.CharacterVictim.Health.HPBound = 100;
            scenario.CharacterVictim.ComboCountVic = 3;
            scenario.CharacterVictim.KillCount = -1;
            scenario.CharacterVictim.Unk344 = 1;
            scenario.CharacterVictim.HitStateCount = 2;
            scenario.CharacterAttacker.FrameDelay = 0;
            scenario.CharacterVictim.FrameDelay = 0;
            scenario.World.DamageStats[1] = 4;
            scenario.World.Rng.Seed(0x10203040u);
            uint firstRngState;
            uint secondRngState;
            unchecked
            {
                firstRngState = 0x10203040u * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
            }
            int expectedHitZ = (int)((firstRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((secondRngState >> 16) & 0x7FFFu) % 9 - 4;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(746);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(
                scenario.CharacterAttacker.Health.HP,
                Is.EqualTo(expectAttackerHpZero ? 0 : 100));
            Assert.That(scenario.CharacterVictim.Health.HP, Is.EqualTo(90));
            Assert.That(scenario.CharacterVictim.Health.HPBound, Is.EqualTo(97));
            Assert.That(scenario.CharacterVictim.ComboCountVic, Is.EqualTo(13));
            Assert.That(scenario.World.DamageStats[1], Is.EqualTo(14));
            Assert.That(
                scenario.CharacterVictim.Frame.N,
                Is.EqualTo(expectedFrame));
            Assert.That(
                scenario.CharacterVictim.Runtime.Frame,
                Is.EqualTo(expectedFrame));
            Assert.That(scenario.CharacterVictim.FallCounter, Is.EqualTo(expectedFall));
            Assert.That(scenario.CharacterVictim.HitCount, Is.EqualTo(1));
            Assert.That(scenario.CharacterVictim.HitStateCount, Is.EqualTo(45));
            Assert.That(
                scenario.CharacterVictim.KnockbackVx,
                Is.EqualTo(expectedKnockbackVx).Within(0.0000001));
            Assert.That(
                scenario.CharacterVictim.KnockbackVy,
                Is.EqualTo(fall > 60 ? -6.9 : 0.1).Within(0.0000001));
            Assert.That(scenario.CharacterAttacker.FrameDelay, Is.EqualTo(3));
            Assert.That(scenario.CharacterVictim.FrameDelay, Is.EqualTo(-3));
            Assert.That(scenario.CharacterAttacker.AttackExempt, Is.EqualTo(2));
            Assert.That(
                scenario.World.GetRawRestVrest(1, 0),
                Is.EqualTo(3));
            Assert.That(scenario.CharacterVictim.HitRecordCount, Is.EqualTo(1));
            Assert.That(
                scenario.CharacterVictim.GetHitRecordAge(0),
                Is.EqualTo(
                    fall > 60
                        ? (effect == 1 ? 20 : 0)
                        : (effect == 1 ? 30 : 10)));
            Assert.That(scenario.CharacterVictim.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(scenario.CharacterVictim.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(scenario.World.Rng.State, Is.EqualTo(secondRngState));
            Assert.That(scenario.World.Rng.CallCount, Is.EqualTo(2));
            Assert.That(
                scenario.World.PendingSounds.Count,
                Is.EqualTo(effect == 1 ? 4 : 2));
            Assert.That(
                scenario.World.PendingSounds[0].Cue,
                Is.EqualTo(expectedEffectCue));
            Assert.That(
                scenario.World.PendingSounds[1].Cue,
                Is.EqualTo(fall > 60 ? "SFX_006" : "SFX_001"));
            if (effect == 1)
            {
                Assert.That(
                    scenario.World.PendingSounds[2].Cue,
                    Is.EqualTo(fall > 60 ? "SFX_033" : "SFX_032"));
                Assert.That(
                    scenario.World.PendingSounds[3].Cue,
                    Is.EqualTo(fall > 60 ? "SFX_006" : "SFX_001"));
            }
        }

        [TestCase(5, 0)]
        [TestCase(52, 16)]
        public void ShadowCompare_Oid5Or52DamageDoesNotReapplySpawnVitals(
            int targetOid,
            int hitStateCount)
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanSpawnVitalsAttacker",
                7283,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            TypedCharacter target = CreateEntity(
                world,
                $"HitPlanOid{targetOid}Target",
                targetOid,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true);
            InteractionArea itr = attacker.GetCollisionFrameData().itrs[0];
            itr.kind = 0;
            itr.injury = 10;
            itr.fall = 10;
            itr.dvx = 1;
            itr.effect = 0;
            itr.arest = 2;
            itr.vrest = 3;
            target.Health.HP = 100;
            target.Health.HPBound = 100;
            target.Health.HP3 = 77;
            target.Health.PP = 66;
            target.HitStateCount = hitStateCount;
            target.KillCount = -1;
            target.Unk344 = 1;
            world.Rng.Seed(0x55667788u);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(779 + targetOid);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Health.HP, Is.EqualTo(90));
            Assert.That(target.Health.HPBound, Is.EqualTo(97));
            Assert.That(
                target.Health.HP3,
                Is.EqualTo(77),
                "OID 5/52 spawn vitals are initialized once by FrameTick/opoint and must not be reapplied after a hit.");
            Assert.That(target.Health.PP, Is.EqualTo(66));
        }

        [Test]
        public void ShadowCompare_LethalStandardCharacterDamageWriterEffectMatchesAuthorityState()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanLethalAttacker",
                7270,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            TypedCharacter target = CreateEntity(
                world,
                "HitPlanLethalTarget",
                7271,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true);
            TypedCharacter holder = CreateEntity(
                world,
                "HitPlanLethalHolder",
                7272,
                2,
                LF2ObjectType.Character,
                1,
                1000,
                hasItr: false,
                hasBody: false);

            InteractionArea itr = attacker.GetCollisionFrameData().itrs[0];
            itr.kind = 0;
            itr.injury = 10;
            itr.fall = 10;
            itr.dvx = 0;
            itr.dvy = 0;
            itr.effect = 0;
            itr.arest = 2;
            itr.vrest = 3;
            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            target.Health.HP = 10;
            target.Health.HPBound = 100;
            target.ComboCountVic = 2;
            target.KillCount = -1;
            target.Unk344 = 1;
            holder.ComboCountAtk = 3;
            holder.KillStat = 4;
            world.DamageStats[1] = 5;
            world.KillStats[1] = 7;
            world.Rng.Seed(0x13572468u);
            uint firstRngState;
            uint secondRngState;
            unchecked
            {
                firstRngState = 0x13572468u * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
            }
            int expectedHitZ = (int)((firstRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((secondRngState >> 16) & 0x7FFFu) % 9 - 4;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(773);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Health.HP, Is.Zero);
            Assert.That(target.Health.HPBound, Is.EqualTo(97));
            Assert.That(target.ComboCountVic, Is.EqualTo(12));
            Assert.That(holder.ComboCountAtk, Is.EqualTo(13));
            Assert.That(holder.KillStat, Is.EqualTo(5));
            Assert.That(world.DamageStats[1], Is.EqualTo(15));
            Assert.That(world.KillStats[1], Is.EqualTo(8));
            Assert.That(target.Frame.N, Is.EqualTo(LF2StandardFrames.FallingBack));
            Assert.That(target.Runtime.Frame, Is.EqualTo(LF2StandardFrames.FallingBack));
            Assert.That(target.FallCounter, Is.Zero);
            Assert.That(target.KnockbackVx, Is.EqualTo(5.1).Within(0.0000001));
            Assert.That(target.KnockbackVy, Is.EqualTo(-6.9).Within(0.0000001));
            Assert.That(target.HitCount, Is.EqualTo(1));
            Assert.That(target.HitStateCount, Is.EqualTo(45));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(attacker.AttackExempt, Is.EqualTo(2));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(3));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.GetHitRecordAge(0), Is.EqualTo(10));
            Assert.That(target.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(target.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(world.Rng.State, Is.EqualTo(secondRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(2));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(2));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_001"));
            Assert.That(world.PendingSounds[1].Cue, Is.EqualTo("SFX_006"));
        }

        [Test]
        public void ShadowCompare_OidD6SpecialAttackerZerosOwnHpAfterCharacterDamage()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            LF2SpecialAttack attacker = CreateSpecialAttackEntity(
                world,
                "HitPlanOidD6Attacker",
                0xD6,
                0,
                1,
                0);
            TypedCharacter target = CreateEntity(
                world,
                "HitPlanOidD6Target",
                7276,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true);
            attacker.Frame.D.itrs.Add(new InteractionArea
            {
                kind = 0,
                x = -30,
                y = -10,
                w = 60,
                h = 20,
                zwidth = 15,
                injury = 10,
                fall = 10,
                dvx = 1,
                arest = 2,
                vrest = 3,
                effect = 0,
            });
            attacker.Health.HP = 100;
            target.Health.HP = 100;
            target.Health.HPBound = 100;
            target.KillCount = -1;
            target.Unk344 = 1;
            world.Rng.Seed(0x24681357u);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(775);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(attacker.Health.HP, Is.Zero);
            Assert.That(target.Health.HP, Is.EqualTo(90));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
        }

        [Test]
        public void ShadowCompare_OidC9SpecialAttackerReleasesItsRuntimeSlotAfterCharacterDamage()
        {
            CreateOidC9LifecycleScenario(
                out SimulationWorld world,
                out LF2SpecialAttack attacker,
                out TypedCharacter target);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    attacker,
                    out RuntimeEntityHandle originalHandle),
                Is.True);

            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.ObjectInteractionTickAll(776);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedLifecycleEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.Zero);
            Assert.That(diagnostics.LastLifecycleEffectDifferenceMask, Is.Zero);
            Assert.That(
                world.TryResolveRuntimeHandleForDiagnostics(originalHandle, out _),
                Is.False);
            Assert.That(world.FindEntityByRuntimeSlotIncludingPending(0), Is.Null);
            Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(target.Health.HP, Is.EqualTo(90));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
        }

        [Test]
        public void ShadowCompare_MissingOidC9LifecycleObservationFailsPlanClosed()
        {
            CreateOidC9LifecycleScenario(
                out SimulationWorld world,
                out LF2SpecialAttack attacker,
                out _);
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            PrepareSingleLifecycleEffectObservation(world, attacker, 777);

            world.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationLifecycleEffectMissing));
            Assert.That(diagnostics.ObservedLifecycleEffectCount, Is.Zero);
        }

        [Test]
        public void ShadowCompare_UnreleasedOidC9LifecycleFailsPlanClosed()
        {
            CreateOidC9LifecycleScenario(
                out SimulationWorld world,
                out LF2SpecialAttack attacker,
                out _);
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            PrepareSingleLifecycleEffectObservation(world, attacker, 778);

            world.ObserveBattleHitExecutionPlanLegacyLifecycleEffect(attacker);
            world.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationLifecycleEffectMismatch));
            Assert.That(diagnostics.ObservedLifecycleEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastLifecycleEffectDifferenceMask, Is.Not.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ShadowCompare_AlternateCharacterDamageWriterEffectMatchesAuthorityState(
            bool lethal)
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanAlternateDamageAttacker",
                7280,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            var defendFrame = new LF2FrameData
            {
                frameId = 7,
                state = LF2States.Defending,
                wait = 1,
                next = 7,
            };
            TypedCharacter target = CreateEntity(
                world,
                "HitPlanAlternateDamageTarget",
                37,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true,
                extraFrame: defendFrame);
            TypedCharacter holder = CreateEntity(
                world,
                "HitPlanAlternateDamageHolder",
                7282,
                2,
                LF2ObjectType.Character,
                1,
                1000,
                hasItr: false,
                hasBody: false);
            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            target.Runtime.PrevFrame2 = defendFrame.frameId;
            target.Frame.Prev2 = defendFrame.frameId;
            target.Frame.Prev2D = defendFrame;
            target.Health.HP = lethal ? 2 : 100;
            target.Health.HPBound = 100;
            target.FallDamageDiv = 200;
            target.KillCount = -1;
            target.Unk344 = 1;
            target.ComboCountVic = 0;
            target.HitStateCount = 5;
            target.AttackingCounter = 9;
            holder.ComboCountAtk = 4;
            holder.KillStat = 4;
            world.DamageStats[1] = 3;
            world.KillStats[1] = 7;
            InteractionArea itr = attacker.GetCollisionFrameData().itrs[0];
            itr.kind = 0;
            itr.injury = 50;
            itr.fall = 10;
            itr.dvx = lethal ? 0 : -4;
            itr.effect = 0;
            itr.bdefend = 10;
            itr.arest = 20;
            itr.vrest = 9;
            world.Rng.Seed(0x11223344u);
            uint firstRngState;
            uint secondRngState;
            unchecked
            {
                firstRngState = 0x11223344u * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
            }
            int expectedHitZ = (int)((firstRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((secondRngState >> 16) & 0x7FFFu) % 9 - 4;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(774);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Health.HP, Is.EqualTo(lethal ? 0 : 98));
            Assert.That(target.Health.HPBound, Is.EqualTo(100));
            Assert.That(target.ComboCountVic, Is.EqualTo(2));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(target.HitStateCount, Is.EqualTo(15));
            Assert.That(target.HitCount, Is.EqualTo(1));
            Assert.That(target.FallCounter, Is.EqualTo(lethal ? 80 : 0));
            Assert.That(
                target.KnockbackVx,
                Is.EqualTo(lethal ? 3.1 : -1.9).Within(0.0000001));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(target.FrameDelay, Is.EqualTo(-5));
            Assert.That(attacker.AttackExempt, Is.EqualTo(12));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(9));
            Assert.That(holder.ComboCountAtk, Is.EqualTo(6));
            Assert.That(holder.KillStat, Is.EqualTo(lethal ? 5 : 4));
            Assert.That(world.DamageStats[1], Is.EqualTo(5));
            Assert.That(world.KillStats[1], Is.EqualTo(lethal ? 8 : 7));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.GetHitRecordAge(0), Is.EqualTo(10));
            Assert.That(target.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(target.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(world.Rng.State, Is.EqualTo(secondRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(2));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(1));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_017"));
            Assert.That(world.PendingSounds[0].WorldX, Is.EqualTo(target.Runtime.XInt));
        }

        [TestCase(LF2ObjectType.LightWeapon, 10, 0, false)]
        [TestCase(LF2ObjectType.HeavyWeapon, 10, 0, false)]
        [TestCase(LF2ObjectType.HeavyWeapon, 50, 0, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, 10, 0, false)]
        [TestCase(LF2ObjectType.Drink, 10, 0, false)]
        [TestCase(LF2ObjectType.LightWeapon, 10, 4, false)]
        [TestCase(LF2ObjectType.HeavyWeapon, 10, 4, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, 10, 4, false)]
        [TestCase(LF2ObjectType.Drink, 10, 4, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, 10, 0, true)]
        public void ShadowCompare_StandardObjectDamageWriterEffectMatchesAuthorityState(
            LF2ObjectType targetType,
            int fall,
            int effect,
            bool oid100Held)
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanObjectDamageAttacker",
                7290,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            LF2Weapon target = CreateWeaponEntity(
                world,
                "HitPlanObjectDamageTarget",
                oid100Held ? 100 : 7291 + (int)targetType,
                1,
                targetType,
                2,
                10);
            InteractionArea itr = attacker.GetCollisionFrameData().itrs[0];
            itr.kind = 0;
            itr.injury = 10;
            itr.fall = fall;
            itr.dvx = oid100Held ? 1 : 0;
            itr.dvy = 0;
            itr.effect = effect;
            itr.bdefend = 0;
            itr.arest = 2;
            itr.vrest = 3;
            target.Runtime.WeaponFlightCounter = 100;
            target.HitCount = 0;
            target.HitStateCount = 0;
            target.HitConfirm2 = 0;
            if (oid100Held)
                target.Runtime.LinkState = -1;
            world.Rng.Seed(0x55667788u);

            uint firstRngState;
            uint secondRngState;
            uint thirdRngState;
            unchecked
            {
                firstRngState = 0x55667788u * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
                thirdRngState = secondRngState * 0x343FDu + 0x269EC3u;
            }
            bool heavy = targetType == LF2ObjectType.HeavyWeapon;
            bool heavyLowFallHitCount = heavy && fall <= 40;
            bool heavyLowFall = heavy && fall <= 40 && effect != 4;
            uint hitZRngState = heavyLowFall ? firstRngState : secondRngState;
            uint hitXRngState = heavyLowFall ? secondRngState : thirdRngState;
            int expectedFrame = heavyLowFall
                ? 20
                : heavy
                    ? (int)((firstRngState >> 16) & 0x7FFFu) % 6
                : (int)((firstRngState >> 16) & 0x7FFFu) % 16;
            int expectedHitZ = (int)((hitZRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((hitXRngState >> 16) & 0x7FFFu) % 9 - 4;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(775 + (int)targetType);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(90));
            Assert.That(target.HitConfirm2, Is.EqualTo(1));
            Assert.That(target.RelationTeam, Is.EqualTo(attacker.RelationTeam));
            Assert.That(target.HitCount, Is.EqualTo(heavyLowFallHitCount ? 0 : 1));
            Assert.That(target.FallCounter, Is.Zero);
            Assert.That(
                target.KnockbackVx,
                Is.EqualTo(oid100Held ? 10.0 : 5.1).Within(0.0000001));
            Assert.That(
                target.KnockbackVy,
                Is.EqualTo(heavyLowFallHitCount ? 0.1 : -6.9).Within(0.0000001));
            Assert.That(target.Frame.N, Is.EqualTo(expectedFrame));
            Assert.That(target.Runtime.Frame, Is.EqualTo(expectedFrame));
            Assert.That(target.HitStateCount, Is.EqualTo(45));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(attacker.AttackExempt, Is.EqualTo(2));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(2));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(3));
            int expectedSelfVrest = targetType == LF2ObjectType.HeavyWeapon
                ? (fall <= 40 && effect != 4 ? 3 : 19)
                : (targetType == LF2ObjectType.ThrowWeapon ||
                   targetType == LF2ObjectType.Drink ? 30 : 0);
            Assert.That(world.GetRawRestVrest(0, 0), Is.EqualTo(expectedSelfVrest));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.GetHitRecordAge(0), Is.EqualTo(10));
            Assert.That(target.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(target.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(world.Rng.State, Is.EqualTo(heavyLowFall ? secondRngState : thirdRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(heavyLowFall ? 2 : 3));
            Assert.That(
                world.PendingSounds.Count,
                Is.EqualTo((targetType == LF2ObjectType.Drink ? 0 : 1) +
                           (oid100Held ? 1 : 0)));
            if (world.PendingSounds.Count > 0)
                Assert.That(
                    world.PendingSounds[0].Cue,
                    Is.EqualTo(effect == 4 ? "SFX_011" : "SFX_001"));
            if (oid100Held)
                Assert.That(world.PendingSounds[1].Cue, Is.EqualTo("SFX_039"));
        }

        [TestCase(0, 30, 100, 1, "SFX_001")]
        [TestCase(2, 20, 100, 1, "SFX_006")]
        [TestCase(3, 30, 100, 1, "SFX_010")]
        [TestCase(5, 30, 100, 1, "SFX_004")]
        [TestCase(21, 30, 100, 1, "SFX_001")]
        [TestCase(22, 30, 100, 1, "SFX_001")]
        [TestCase(23, 30, 100, 2, "SFX_068")]
        [TestCase(30, 30, 100, 1, "SFX_001")]
        [TestCase(5005, 30, 95, 1, "SFX_001")]
        [TestCase(5999, 30, 0, 1, "SFX_001")]
        [TestCase(6033, 33, 100, 1, "SFX_001")]
        public void ShadowCompare_StandardType3DamageWriterEffectMatchesAuthorityState(
            int effect,
            int expectedFrame,
            int expectedPp,
            int expectedSoundCount,
            string expectedLastCue)
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanType3Attacker",
                7300,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            LF2SpecialAttack target = CreateSpecialAttackEntity(
                world,
                "HitPlanType3Target",
                7301,
                1,
                2,
                10);
            TypedCharacter holder = CreateEntity(
                world,
                "HitPlanType3Holder",
                7302,
                2,
                LF2ObjectType.Character,
                1,
                1000,
                hasItr: false,
                hasBody: false);
            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            target.Runtime.SetVelocity(2.0, 3.0, 4.0);
            target.KnockbackVx = 5.0;
            target.KnockbackVy = 6.0;
            target.KnockbackVz = 7.0;
            target.AttackingCounter = 9;
            target.Health.PP = 100;
            InteractionArea itr = attacker.GetCollisionFrameData().itrs[0];
            itr.kind = 0;
            itr.injury = 10;
            itr.fall = 10;
            itr.dvx = 3;
            itr.dvy = 4;
            itr.effect = effect;
            itr.bdefend = 0;
            itr.arest = 2;
            itr.vrest = 3;
            world.Rng.Seed(0x66778899u);
            uint firstRngState;
            uint secondRngState;
            unchecked
            {
                firstRngState = 0x66778899u * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
            }
            int expectedHitZ = (int)((firstRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((secondRngState >> 16) & 0x7FFFu) % 9 - 4;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(781);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Frame.N, Is.EqualTo(expectedFrame));
            Assert.That(target.Runtime.Frame, Is.EqualTo(expectedFrame));
            Assert.That(target.RelationTeam, Is.EqualTo(attacker.RelationTeam));
            Assert.That(target.HolderCopySlot, Is.EqualTo(holder.Runtime.SlotIndex));
            Assert.That(target.HitConfirm2, Is.EqualTo(1));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(target.Runtime.Vx, Is.Zero);
            Assert.That(target.Runtime.Vy, Is.Zero);
            Assert.That(target.Runtime.Vz, Is.Zero);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(target.FallCounter, Is.EqualTo(10));
            Assert.That(target.Health.PP, Is.EqualTo(expectedPp));
            Assert.That(target.HitCount, Is.EqualTo(1));
            Assert.That(target.HitStateCount, Is.EqualTo(45));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(attacker.AttackExempt, Is.EqualTo(2));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(2));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(3));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.GetHitRecordAge(0), Is.EqualTo(10));
            Assert.That(target.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(target.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(world.Rng.State, Is.EqualTo(secondRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(2));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(expectedSoundCount));
            Assert.That(world.PendingSounds[expectedSoundCount - 1].Cue, Is.EqualTo(expectedLastCue));
        }

        [TestCase(LF2States.ObjectFlying)]
        public void ShadowCompare_Type3StateSyncWriterEffectMatchesAuthorityState(int synchronizedState)
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            LF2SpecialAttack attacker = CreateSpecialAttackEntity(
                world,
                "HitPlanType3SyncAttacker",
                7400,
                0,
                1,
                0);
            LF2SpecialAttack target = CreateSpecialAttackEntity(
                world,
                "HitPlanType3SyncTarget",
                7401,
                1,
                2,
                10);
            attacker.Frame.D.state = synchronizedState;
            target.Frame.D.state = synchronizedState;
            var itr = new InteractionArea
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 30,
                h = 20,
                zwidth = 12,
                injury = 10,
                fall = 10,
                dvx = 3,
                dvy = 4,
                effect = 0,
                bdefend = 0,
                arest = 2,
                vrest = 3,
            };
            attacker.Frame.D.itrs.Add(itr);
            attacker.Runtime.SetVelocity(2.0, 3.0, 4.0);
            attacker.KnockbackVx = 5.0;
            attacker.KnockbackVy = 6.0;
            attacker.KnockbackVz = 7.0;
            attacker.AttackingCounter = 8;
            attacker.FrameDelay = 6;
            target.Runtime.SetVelocity(-2.0, -3.0, -4.0);
            target.KnockbackVx = -5.0;
            target.KnockbackVy = -6.0;
            target.KnockbackVz = -7.0;
            target.AttackingCounter = 9;
            world.Rng.Seed(0x778899AAu);
            uint firstRngState;
            uint secondRngState;
            unchecked
            {
                firstRngState = 0x778899AAu * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
            }
            int expectedHitZ = (int)((firstRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((secondRngState >> 16) & 0x7FFFu) % 9 - 4;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.ObjectInteractionTickAll(782);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(attacker.Frame.N, Is.EqualTo(20));
            Assert.That(attacker.Runtime.Frame, Is.EqualTo(20));
            Assert.That(attacker.AttackingCounter, Is.Zero);
            Assert.That(attacker.Runtime.Vx, Is.Zero);
            Assert.That(attacker.Runtime.Vy, Is.Zero);
            Assert.That(attacker.Runtime.Vz, Is.Zero);
            Assert.That(attacker.KnockbackVx, Is.Zero);
            Assert.That(attacker.KnockbackVy, Is.Zero);
            Assert.That(attacker.KnockbackVz, Is.Zero);
            Assert.That(attacker.FrameDelay, Is.EqualTo(-3));
            Assert.That(target.Frame.N, Is.EqualTo(20));
            Assert.That(target.Runtime.Frame, Is.EqualTo(20));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(target.Runtime.Vx, Is.Zero);
            Assert.That(target.Runtime.Vy, Is.Zero);
            Assert.That(target.Runtime.Vz, Is.Zero);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(target.HitConfirm2, Is.Zero);
            Assert.That(target.RelationTeam, Is.EqualTo(2));
            Assert.That(target.FallCounter, Is.EqualTo(10));
            Assert.That(target.HitCount, Is.EqualTo(1));
            Assert.That(target.HitStateCount, Is.EqualTo(45));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(attacker.AttackExempt, Is.EqualTo(2));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(2));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(3));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.GetHitRecordAge(0), Is.EqualTo(10));
            Assert.That(target.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(target.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(world.Rng.State, Is.EqualTo(secondRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(2));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(1));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_001"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ShadowCompare_Type3D1KarasuIdentityWriterEffectMatchesAuthorityState(
            bool expandingStateSync)
        {
            LF2CharacterDataWrapper attackerWrapper = null;
            var runtimeConfigs = new RuntimeCharacterConfigResolver(
                oid => oid == 0xD1 ? attackerWrapper : null);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                characterConfigResolver: runtimeConfigs);
            LF2SpecialAttack attacker = CreateSpecialAttackEntity(
                world,
                "HitPlanType3D1Attacker",
                0xD1,
                0,
                1,
                0);
            attackerWrapper = attacker.FrameCache.Wrapper;
            attackerWrapper.characterData.weapon_hp = 17;
            attacker.WeaponCount = 17;
            attacker.Runtime.SetVelocity(2.0, 3.0, 4.0);
            attacker.KnockbackVx = 5.0;
            attacker.KnockbackVy = 6.0;
            attacker.KnockbackVz = 7.0;
            attacker.AttackingCounter = 8;
            attacker.FrameDelay = 6;
            if (expandingStateSync)
            {
                attacker.Frame.D.state = LF2States.ObjectExpanding;
                attacker.GetFrameDataById(40).state = LF2States.ObjectExpanding;
            }
            LF2SpecialAttack target = CreateSpecialAttackEntity(
                world,
                "HitPlanType3KarasuTarget",
                0xC8,
                1,
                2,
                10);
            attacker.HolderCopySlot = 77;
            var itr = new InteractionArea
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 30,
                h = 20,
                zwidth = 12,
                injury = 10,
                fall = 10,
                dvx = 3,
                dvy = 4,
                effect = 0,
                bdefend = 0,
                arest = 2,
                vrest = 3,
            };
            attacker.Frame.D.itrs.Add(itr);
            target.Runtime.SetVelocity(-2.0, -3.0, -4.0);
            target.KnockbackVx = -5.0;
            target.KnockbackVy = -6.0;
            target.KnockbackVz = -7.0;
            target.AttackingCounter = 9;
            world.Rng.Seed(0x8899AABBu);
            uint firstRngState;
            uint secondRngState;
            unchecked
            {
                firstRngState = 0x8899AABBu * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
            }
            int expectedHitZ = (int)((firstRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((secondRngState >> 16) & 0x7FFFu) % 9 - 4;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.ObjectInteractionTickAll(783);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.ObjectId, Is.EqualTo(0xD1));
            Assert.That(target.FrameCache.Wrapper.characterId, Is.EqualTo(0xD1));
            Assert.That(
                target.GetCurrentDataObjectTypeForSimulation(),
                Is.EqualTo((int)LF2ObjectType.SpecialAttack));
            Assert.That(target.WeaponCount, Is.EqualTo(17));
            Assert.That(target.Frame.N, Is.EqualTo(expandingStateSync ? 20 : 40));
            Assert.That(target.Runtime.Frame, Is.EqualTo(expandingStateSync ? 20 : 40));
            Assert.That(target.Frame.Prev, Is.EqualTo(40));
            Assert.That(target.Trans.WaitCounter, Is.EqualTo(40));
            Assert.That(target.RelationTeam, Is.EqualTo(attacker.RelationTeam));
            Assert.That(target.HolderCopySlot, Is.EqualTo(77));
            Assert.That(target.HitConfirm2, Is.EqualTo(1));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(target.Runtime.Vx, Is.Zero);
            Assert.That(target.Runtime.Vy, Is.Zero);
            Assert.That(target.Runtime.Vz, Is.Zero);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(target.FallCounter, Is.EqualTo(10));
            Assert.That(target.HitCount, Is.EqualTo(1));
            Assert.That(target.HitStateCount, Is.EqualTo(45));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(attacker.Frame.N, Is.EqualTo(expandingStateSync ? 20 : 0));
            Assert.That(attacker.Runtime.Frame, Is.EqualTo(expandingStateSync ? 20 : 0));
            Assert.That(attacker.AttackingCounter, Is.EqualTo(expandingStateSync ? 0 : 8));
            Assert.That(attacker.Runtime.Vx, Is.EqualTo(expandingStateSync ? 0.0 : 2.0));
            Assert.That(attacker.Runtime.Vy, Is.EqualTo(expandingStateSync ? 0.0 : 3.0));
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(expandingStateSync ? 0.0 : 4.0));
            Assert.That(attacker.KnockbackVx, Is.EqualTo(expandingStateSync ? 0.0 : 5.0));
            Assert.That(attacker.KnockbackVy, Is.EqualTo(expandingStateSync ? 0.0 : 6.0));
            Assert.That(attacker.KnockbackVz, Is.EqualTo(expandingStateSync ? 0.0 : 7.0));
            Assert.That(attacker.FrameDelay, Is.EqualTo(expandingStateSync ? -3 : 3));
            Assert.That(attacker.AttackExempt, Is.EqualTo(2));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(2));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(3));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.GetHitRecordAge(0), Is.EqualTo(10));
            Assert.That(target.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(target.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(world.Rng.State, Is.EqualTo(secondRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(2));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(1));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_001"));
        }

        [TestCase(8, false)]
        [TestCase(0xD5, true)]
        public void ShadowCompare_Type3ActiveD1IdentityWriterEffectMatchesAuthorityState(
            int attackerOid,
            bool heldByCharacter)
        {
            LF2CharacterDataWrapper d1Wrapper = null;
            var runtimeConfigs = new RuntimeCharacterConfigResolver(
                oid => oid == 0xD1 ? d1Wrapper : null);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                characterConfigResolver: runtimeConfigs);
            LF2Entity attacker;
            InteractionArea itr;
            if (heldByCharacter)
            {
                LF2SpecialAttack specialAttacker = CreateSpecialAttackEntity(
                    world,
                    "HitPlanType3D5Attacker",
                    attackerOid,
                    0,
                    1,
                    0);
                itr = new InteractionArea
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 30,
                    h = 20,
                    zwidth = 12,
                };
                specialAttacker.Frame.D.itrs.Add(itr);
                attacker = specialAttacker;
            }
            else
            {
                attacker = CreateEntity(
                    world,
                    "HitPlanType3Oid8Attacker",
                    attackerOid,
                    0,
                    LF2ObjectType.Character,
                    1,
                    0,
                    hasItr: true,
                    hasBody: false);
                itr = attacker.GetCollisionFrameData().itrs[0];
            }

            LF2SpecialAttack target = CreateSpecialAttackEntity(
                world,
                "HitPlanType3ActiveKarasuTarget",
                0xC8,
                1,
                2,
                10);
            TypedCharacter holder = CreateEntity(
                world,
                "HitPlanType3D5Holder",
                7502,
                2,
                LF2ObjectType.Character,
                4,
                1000,
                hasItr: false,
                hasBody: false);
            holder.HolderCopySlot = 88;
            LF2SpecialAttack d1Source = CreateSpecialAttackEntity(
                world,
                "HitPlanType3D1Source",
                0xD1,
                3,
                5,
                1000);
            d1Wrapper = d1Source.FrameCache.Wrapper;
            d1Wrapper.characterData.weapon_hp = 17;
            d1Source.WeaponCount = 17;
            attacker.HolderCopySlot = 77;
            if (heldByCharacter)
            {
                attacker.Runtime.LinkState = -1;
                attacker.Runtime.HolderStableId = holder.Runtime.SlotIndex;
            }

            itr.kind = 0;
            itr.injury = 10;
            itr.fall = 10;
            itr.dvx = 3;
            itr.dvy = 4;
            itr.effect = 0;
            itr.bdefend = 0;
            itr.arest = 2;
            itr.vrest = 3;
            target.Runtime.SetVelocity(-2.0, -3.0, -4.0);
            target.KnockbackVx = -5.0;
            target.KnockbackVy = -6.0;
            target.KnockbackVz = -7.0;
            target.AttackingCounter = 9;
            world.Rng.Seed(0x99AABBCCu);
            uint firstRngState;
            uint secondRngState;
            unchecked
            {
                firstRngState = 0x99AABBCCu * 0x343FDu + 0x269EC3u;
                secondRngState = firstRngState * 0x343FDu + 0x269EC3u;
            }
            int expectedHitZ = (int)((firstRngState >> 16) & 0x7FFFu) % 9 - 4;
            int expectedHitX = 10 +
                (int)((secondRngState >> 16) & 0x7FFFu) % 9 - 4;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            if (heldByCharacter)
                world.ObjectInteractionTickAll(784);
            else
                world.PostInteractionTickAll(784);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.ObjectId, Is.EqualTo(0xD1));
            Assert.That(target.FrameCache.Wrapper.characterId, Is.EqualTo(0xD1));
            Assert.That(target.WeaponCount, Is.EqualTo(17));
            Assert.That(target.Frame.N, Is.EqualTo(30));
            Assert.That(target.Runtime.Frame, Is.EqualTo(30));
            Assert.That(target.Frame.Prev, Is.EqualTo(30));
            Assert.That(
                target.RelationTeam,
                Is.EqualTo(heldByCharacter ? holder.RelationTeam : attacker.RelationTeam));
            Assert.That(
                target.HolderCopySlot,
                Is.EqualTo(heldByCharacter ? holder.HolderCopySlot : attacker.HolderCopySlot));
            Assert.That(target.HitConfirm2, Is.EqualTo(1));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(target.Runtime.Vx, Is.Zero);
            Assert.That(target.Runtime.Vy, Is.Zero);
            Assert.That(target.Runtime.Vz, Is.Zero);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(target.FallCounter, Is.EqualTo(10));
            Assert.That(target.HitCount, Is.EqualTo(1));
            Assert.That(target.HitStateCount, Is.EqualTo(45));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(attacker.AttackExempt, Is.EqualTo(2));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(2));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(3));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.GetHitRecordAge(0), Is.EqualTo(10));
            Assert.That(target.GetHitRecordX(0), Is.EqualTo(expectedHitX));
            Assert.That(target.GetHitRecordZ(0), Is.EqualTo(expectedHitZ));
            Assert.That(world.Rng.State, Is.EqualTo(secondRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(2));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(1));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_001"));
        }

        [Test]
        public void ShadowCompare_Kind6WriterEffectMatchesAuthorityState()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 6;
            scenario.CharacterVictim.HitConfirmCounter = 0;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(747);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(scenario.CharacterVictim.HitConfirmCounter, Is.EqualTo(3));
        }

        [Test]
        public void ShadowCompare_Kind8WriterEffectMatchesAuthorityState()
        {
            Scenario scenario = CreateScenario();
            InteractionArea itr = scenario.CharacterAttacker.GetCollisionFrameData().itrs[0];
            itr.kind = 8;
            itr.injury = 27;
            itr.dvx = 0;
            scenario.CharacterVictim.Runtime.SetPosition(73.5, 0, -12.25);
            scenario.CharacterVictim.Runtime.SyncIntegerPosition();
            scenario.CharacterAttacker.Runtime.SetPosition(-50, 0, 30);
            scenario.CharacterAttacker.Runtime.SyncIntegerPosition();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(748);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(scenario.CharacterVictim.HealTimer, Is.EqualTo(1027));
            Assert.That(
                scenario.CharacterAttacker.Runtime.X,
                Is.EqualTo(scenario.CharacterVictim.Runtime.X));
            Assert.That(
                scenario.CharacterAttacker.Runtime.Z,
                Is.EqualTo(scenario.CharacterVictim.Runtime.Z + 1.0));
            Assert.That(
                scenario.CharacterAttacker.Runtime.XInt,
                Is.EqualTo(scenario.CharacterVictim.Runtime.XInt));
            Assert.That(
                scenario.CharacterAttacker.Runtime.ZInt,
                Is.EqualTo(scenario.CharacterVictim.Runtime.ZInt + 1));
        }

        [Test]
        public void ShadowCompare_Kind14WriterEffectMatchesAuthorityDespiteFalseDispatchReturn()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 14;
            scenario.CharacterAttacker.Runtime.SetPosition(20, 0, 10);
            scenario.CharacterAttacker.Runtime.SyncIntegerPosition();
            scenario.CharacterVictim.Runtime.SetPosition(0, 0, 0);
            scenario.CharacterVictim.Runtime.SetVelocity(2, 0, 3);
            scenario.CharacterVictim.Runtime.SyncIntegerPosition();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(749);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObservedDispatchCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(scenario.CharacterVictim.Runtime.XBoundPositive, Is.True);
            Assert.That(scenario.CharacterVictim.Runtime.ZBoundPositive, Is.True);
        }

        [TestCase(1, 9.25, 3.5)]
        [TestCase(3, 8.5, 3.5)]
        public void ShadowCompare_GrabWriterEffectMatchesAuthorityState(
            int kind,
            double expectedAttackerX,
            double expectedTargetX)
        {
            Scenario scenario = CreateScenario();
            InteractionArea itr = scenario.CharacterAttacker.GetCollisionFrameData().itrs[0];
            itr.kind = kind;
            itr.catchingact = new[] { 0 };
            itr.caughtact = new[] { 0 };
            scenario.CharacterAttacker.Frame.D.cpoint = new CatchPoint { x = 2 };
            scenario.CharacterVictim.Frame.D.cpoint = new CatchPoint { x = 3 };
            scenario.CharacterAttacker.Frame.D.centerx = 4;
            scenario.CharacterAttacker.Frame.D.centery = 8;
            scenario.CharacterVictim.Frame.D.centerx = 6;
            scenario.CharacterVictim.Frame.D.centery = 11;
            scenario.CharacterAttacker.Runtime.SetPosition(2.75, 4.75, 0);
            scenario.CharacterAttacker.Runtime.SyncIntegerPosition();
            scenario.CharacterAttacker.Runtime.SetVelocity(7, 0, 0);
            scenario.CharacterVictim.Runtime.SetPosition(10.25, 7.5, 0);
            scenario.CharacterVictim.Runtime.SyncIntegerPosition();
            scenario.CharacterVictim.Runtime.SetVelocity(-5, 0, 0);
            scenario.CharacterVictim.FallCounter = 37;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(752 + kind);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(scenario.CharacterAttacker.Runtime.Vx, Is.Zero);
            Assert.That(scenario.CharacterVictim.Runtime.Vx, Is.Zero);
            Assert.That(scenario.CharacterAttacker.Runtime.X, Is.EqualTo(expectedAttackerX));
            Assert.That(scenario.CharacterVictim.Runtime.X, Is.EqualTo(expectedTargetX));
            Assert.That(scenario.CharacterVictim.Runtime.Y, Is.EqualTo(7.0));
            Assert.That(scenario.CharacterAttacker.CaughtSlotIndex, Is.EqualTo(1));
            Assert.That(scenario.CharacterVictim.CatcherSlotIndex, Is.Zero);
            Assert.That(scenario.CharacterAttacker.Runtime.CaughtDuration, Is.EqualTo(300));
            Assert.That(scenario.CharacterVictim.FallCounter, Is.Zero);
        }

        [Test]
        public void ShadowCompare_Kind7GenericPickupWriterEffectMatchesAuthorityState()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 7;
            scenario.CharacterAttacker.Runtime.LinkState = 0;
            scenario.CharacterAttacker.Runtime.PickupCount = 2;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(756);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(scenario.CharacterAttacker.Runtime.LinkState, Is.EqualTo(1));
            Assert.That(scenario.CharacterVictim.Runtime.LinkState, Is.EqualTo(-1));
            Assert.That(scenario.CharacterAttacker.Runtime.TargetSlotIndex, Is.EqualTo(1));
            Assert.That(scenario.CharacterAttacker.Runtime.HeldWeaponStableId, Is.EqualTo(1));
            Assert.That(scenario.CharacterVictim.Runtime.HolderStableId, Is.Zero);
            Assert.That(scenario.CharacterVictim.HolderCopySlot, Is.Zero);
            Assert.That(scenario.CharacterAttacker.Runtime.PickupCount, Is.EqualTo(3));
        }

        [Test]
        public void ShadowCompare_Kind2LightWeaponPickupWriterEffectMatchesAuthorityState()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            var pickingFrame = new LF2FrameData
            {
                frameId = LF2StandardFrames.PickingLight,
                state = LF2States.Standing,
                wait = 1,
                next = LF2StandardFrames.PickingLight,
            };
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanPickupAttacker",
                7200,
                0,
                LF2ObjectType.Character,
                7,
                0,
                hasItr: true,
                hasBody: false,
                extraFrame: pickingFrame);
            TypedCharacter target = CreateEntity(
                world,
                "HitPlanPickupTarget",
                7201,
                1,
                LF2ObjectType.LightWeapon,
                8,
                10,
                hasItr: false,
                hasBody: true);
            attacker.Runtime.AttackingCounter = 9;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            target.Frame.D.state = LF2States.WeaponOnGround;
            attacker.GetCollisionFrameData().itrs[0].kind = 2;
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(757);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(attacker.Frame.N, Is.EqualTo(LF2StandardFrames.PickingLight));
            Assert.That(attacker.Runtime.LinkState, Is.EqualTo(1));
            Assert.That(target.Runtime.LinkState, Is.EqualTo(-1));
            Assert.That(attacker.Runtime.AttackingCounter, Is.Zero);
            Assert.That(target.RelationTeam, Is.EqualTo(attacker.RelationTeam));
        }

        [Test]
        public void ShadowCompare_Kind10CharacterWriterEffectMatchesAuthorityState()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanFluteAttacker",
                7220,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            var fluteFrame = new LF2FrameData
            {
                frameId = 182,
                state = LF2States.Standing,
                wait = 1,
                next = 182,
            };
            TypedCharacter target = CreateEntity(
                world,
                "HitPlanFluteTarget",
                7221,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true,
                extraFrame: fluteFrame);
            TypedCharacter holder = CreateEntity(
                world,
                "HitPlanFluteHolder",
                7222,
                2,
                LF2ObjectType.Character,
                1,
                1000,
                hasItr: false,
                hasBody: false);
            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            target.KillCount = -1;
            target.Unk344 = 1;
            target.WeaponCount = 5;
            target.Runtime.SetPosition(10.25, -10.5, 3.75);
            target.Runtime.SyncIntegerPosition();
            target.Runtime.SetVelocity(4.25, -2.5, -3.75);
            world.DamageStats[1] = 7;
            attacker.GetCollisionFrameData().itrs[0].kind = 10;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(768);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.WeaponCount, Is.EqualTo(-20));
            Assert.That(target.Frame.N, Is.EqualTo(182));
            Assert.That(target.Runtime.Vx, Is.EqualTo(4.25 * 0.9345794392523364));
            Assert.That(target.Runtime.Vz, Is.EqualTo(-3.75 * 0.9345794392523364));
            Assert.That(target.Runtime.Vy, Is.EqualTo(-5.5));
            Assert.That(target.KnockbackVy, Is.EqualTo(-5.5));
            Assert.That(holder.ComboCountAtk, Is.EqualTo(11));
            Assert.That(world.DamageStats[1], Is.EqualTo(18));
        }

        [Test]
        public void ShadowCompare_Kind11NonNegativeWeaponCountIsAuthorityNoOp()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 11;
            scenario.CharacterVictim.WeaponCount = 0;
            scenario.CharacterVictim.Runtime.SetVelocity(3.25, -2.5, -4.75);
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(769);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(scenario.CharacterVictim.WeaponCount, Is.Zero);
            Assert.That(scenario.CharacterVictim.Runtime.Vx, Is.EqualTo(3.25));
            Assert.That(scenario.CharacterVictim.Runtime.Vy, Is.EqualTo(-2.5));
            Assert.That(scenario.CharacterVictim.Runtime.Vz, Is.EqualTo(-4.75));
        }

        [Test]
        public void ShadowCompare_Kind15CharacterWriterEffectMatchesAuthorityState()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 15;
            scenario.CharacterAttacker.Runtime.SetPosition(0.25, 0, 0.25);
            scenario.CharacterAttacker.Runtime.SyncIntegerPosition();
            scenario.CharacterVictim.Runtime.SetPosition(10.75, -10.5, 5.75);
            scenario.CharacterVictim.Runtime.SyncIntegerPosition();
            scenario.CharacterVictim.Runtime.SetVelocity(4.25, -2.5, -3.75);
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            scenario.World.PostInteractionTickAll(770);

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(scenario.CharacterVictim.Runtime.Vx, Is.EqualTo(3.25));
            Assert.That(scenario.CharacterVictim.KnockbackVx, Is.EqualTo(3.25));
            Assert.That(scenario.CharacterVictim.Runtime.Vz, Is.EqualTo(-4.25));
            Assert.That(scenario.CharacterVictim.KnockbackVz, Is.EqualTo(-4.25));
            Assert.That(scenario.CharacterVictim.Runtime.Vy, Is.EqualTo(-5.5));
            Assert.That(scenario.CharacterVictim.KnockbackVy, Is.EqualTo(-5.5));
        }

        [Test]
        public void ShadowCompare_Kind16CharacterWriterEffectMatchesFullAuthorityState()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            var drainFrame = new LF2FrameData
            {
                frameId = LF2StandardFrames.MpDrain,
                state = LF2States.Standing,
                wait = 1,
                next = LF2StandardFrames.MpDrain,
            };
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanKind16Attacker",
                7230,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            TypedCharacter target = CreateEntity(
                world,
                "HitPlanKind16Target",
                7231,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true,
                extraFrame: drainFrame);
            TypedCharacter heldTarget = CreateEntity(
                world,
                "HitPlanKind16HeldTarget",
                7232,
                2,
                LF2ObjectType.HeavyWeapon,
                2,
                1000,
                hasItr: false,
                hasBody: false);
            TypedCharacter holder = CreateEntity(
                world,
                "HitPlanKind16Holder",
                7233,
                3,
                LF2ObjectType.Character,
                1,
                1000,
                hasItr: false,
                hasBody: false);

            InteractionArea itr = attacker.GetCollisionFrameData().itrs[0];
            itr.kind = 16;
            itr.injury = 50;
            itr.vrest = 7;
            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            target.Health.HP = 20;
            target.Health.HPBound = 100;
            target.FallDamageDiv = 200;
            target.KillCount = -1;
            target.Unk344 = 1;
            target.ComboCountVic = 3;
            target.AttackingCounter = 9;
            holder.ComboCountAtk = 4;
            holder.KillStat = 2;
            world.DamageStats[1] = 7;
            world.KillStats[1] = 8;
            target.Runtime.LinkState = 2;
            target.Runtime.TargetSlotIndex = heldTarget.Runtime.SlotIndex;
            heldTarget.Runtime.LinkState = -2;
            heldTarget.Runtime.HolderStableId = target.Runtime.SlotIndex;
            world.Rng.Seed(0x12345678u);
            uint expectedRngState;
            unchecked
            {
                expectedRngState = 0x12345678u * 0x343FDu + 0x269EC3u;
            }
            int expectedHeldFrame = (int)((expectedRngState >> 16) & 0x7FFFu) % 6;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(771);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Health.HP, Is.EqualTo(-5));
            Assert.That(target.Health.HPBound, Is.EqualTo(92));
            Assert.That(target.ComboCountVic, Is.EqualTo(28));
            Assert.That(target.Frame.N, Is.EqualTo(LF2StandardFrames.MpDrain));
            Assert.That(target.Runtime.Frame, Is.EqualTo(LF2StandardFrames.MpDrain));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(holder.ComboCountAtk, Is.EqualTo(29));
            Assert.That(holder.KillStat, Is.EqualTo(3));
            Assert.That(world.DamageStats[1], Is.EqualTo(32));
            Assert.That(world.KillStats[1], Is.EqualTo(9));
            Assert.That(world.GetRawRestVrest(1, 0), Is.EqualTo(7));
            Assert.That(world.GetRawRestVrest(0, 2), Is.EqualTo(45));
            Assert.That(world.GetRawRestVrest(1, 2), Is.EqualTo(30));
            Assert.That(target.Runtime.LinkState, Is.Zero);
            Assert.That(heldTarget.Runtime.LinkState, Is.Zero);
            Assert.That(heldTarget.Frame.N, Is.EqualTo(expectedHeldFrame));
            Assert.That(heldTarget.Runtime.Frame, Is.EqualTo(expectedHeldFrame));
            Assert.That(heldTarget.Runtime.Vy, Is.EqualTo(-1.0));
            Assert.That(world.Rng.State, Is.EqualTo(expectedRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(1));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(1));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_065"));
            Assert.That(world.PendingSounds[0].WorldX, Is.EqualTo(target.Runtime.XInt));
        }

        [Test]
        public void ShadowCompare_Kind16InvalidHeldRelationPreservesVictimLinkState()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            var drainFrame = new LF2FrameData
            {
                frameId = LF2StandardFrames.MpDrain,
                state = LF2States.Standing,
                wait = 1,
                next = LF2StandardFrames.MpDrain,
            };
            TypedCharacter attacker = CreateEntity(
                world,
                "HitPlanKind16InvalidAttacker",
                7240,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            TypedCharacter target = CreateEntity(
                world,
                "HitPlanKind16InvalidTarget",
                7241,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true,
                extraFrame: drainFrame);
            TypedCharacter staleHeldTarget = CreateEntity(
                world,
                "HitPlanKind16InvalidHeldTarget",
                7242,
                2,
                LF2ObjectType.HeavyWeapon,
                2,
                1000,
                hasItr: false,
                hasBody: false);
            InteractionArea itr = attacker.GetCollisionFrameData().itrs[0];
            itr.kind = 16;
            itr.injury = 0;
            target.Runtime.LinkState = 2;
            target.Runtime.TargetSlotIndex = staleHeldTarget.Runtime.SlotIndex;
            staleHeldTarget.Runtime.LinkState = -2;
            staleHeldTarget.Runtime.HolderStableId = 77;
            world.Rng.Seed(0x87654321u);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(772);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Runtime.LinkState, Is.EqualTo(2));
            Assert.That(staleHeldTarget.Runtime.LinkState, Is.EqualTo(-2));
            Assert.That(world.Rng.State, Is.EqualTo(0x87654321u));
            Assert.That(world.Rng.CallCount, Is.Zero);
        }

        [Test]
        public void WarmedShadowCompareWriterEffectObservation_AllocatesNoManagedMemory()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 6;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            for (int tick = 760; tick < 792; tick++)
            {
                scenario.CharacterVictim.HitConfirmCounter = 0;
                scenario.World.PostInteractionTickAll(tick);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 800; tick < 1312; tick++)
            {
                scenario.CharacterVictim.HitConfirmCounter = 0;
                scenario.World.PostInteractionTickAll(tick);
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(
                after - before,
                Is.Zero,
                $"Writer-effect shadow allocated {after - before} bytes. " +
                DescribeDiagnostics(
                    scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics));
            Assert.That(
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics
                    .CurrentTickPlanValid,
                Is.True);
        }

        [Test]
        public void ShadowCompare_WrongWriterEffectFailsPlanClosed()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 6;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            PrepareSingleWriterEffectObservation(
                scenario.World,
                scenario.CharacterAttacker,
                750,
                out LF2Entity target,
                out _);

            scenario.World.ObserveBattleHitExecutionPlanLegacyWriterEffect(
                scenario.CharacterAttacker,
                target);
            scenario.World.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationWriterEffectMismatch));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Not.Zero);
        }

        [Test]
        public void ShadowCompare_MissingWriterEffectObservationFailsPlanClosed()
        {
            Scenario scenario = CreateScenario();
            scenario.CharacterAttacker.GetCollisionFrameData().itrs[0].kind = 6;
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            PrepareSingleWriterEffectObservation(
                scenario.World,
                scenario.CharacterAttacker,
                751,
                out _,
                out _);

            scenario.World.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationWriterEffectMissing));
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.Zero);
        }

        [Test]
        public void Kind5ReplacementDoesNotRetroactivelyReleaseHeavyHeldTarget()
        {
            Scenario scenario = CreateScenario();
            TypedCharacter heldTarget = CreateEntity(
                scenario.World,
                "HitPlanKind5HeldTarget",
                7132,
                2,
                LF2ObjectType.HeavyWeapon,
                scenario.CharacterVictim.Team,
                10,
                hasItr: false,
                hasBody: false);
            TypedCharacter holder = CreateEntity(
                scenario.World,
                "HitPlanKind5Holder",
                7133,
                3,
                LF2ObjectType.Character,
                scenario.CharacterAttacker.Team,
                0,
                hasItr: true,
                hasBody: false);
            holder.GetCollisionFrameData().itrs.Add(new InteractionArea
            {
                kind = 0,
                injury = 10,
                arest = 4,
                vrest = 1,
            });
            holder.GetCollisionFrameData().wpoints.Add(new WeaponPoint
            {
                attacking = 1,
            });
            holder.Runtime.TargetSlotIndex = scenario.CharacterAttacker.Runtime.SlotIndex;
            scenario.CharacterAttacker.Runtime.LinkState = -1;
            scenario.CharacterAttacker.HolderCopySlot = holder.Runtime.SlotIndex;
            scenario.CharacterVictim.Runtime.LinkState = 2;
            scenario.CharacterVictim.Runtime.TargetSlotIndex = heldTarget.Runtime.SlotIndex;
            heldTarget.Runtime.LinkState = -2;
            heldTarget.Runtime.HolderStableId = scenario.CharacterVictim.Runtime.SlotIndex;
            InteractionArea sourceItr = scenario.CharacterAttacker
                .GetCollisionFrameData()
                .itrs[0];
            sourceItr.kind = 5;

            InteractionArea resolvedItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                scenario.CharacterAttacker,
                scenario.CharacterVictim,
                scenario.CharacterAttacker.GetCollisionFrameData(),
                sourceItr,
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);

            Assert.That(resolvedItr, Is.Not.Null);
            Assert.That(resolvedItr.kind, Is.EqualTo(0));
            Assert.That(zeroAttackerHpOnConsume, Is.False);
            Assert.That(
                releaseHeavyHeldTargetOnConsume,
                Is.False,
                "Authority checks heavy-held release before kind5 replacement.");
            Assert.That(scenario.CharacterVictim.Runtime.LinkState, Is.EqualTo(2));
            Assert.That(heldTarget.Runtime.LinkState, Is.EqualTo(-2));
        }

        [Test]
        public void ShadowCompare_Oid300AbortSkipsOnlyRemainingCandidatesForSameAttacker()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter abortingAttacker = CreateEntity(
                world,
                "HitPlanOid300Attacker",
                7140,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            TypedCharacter oid300Victim = CreateEntity(
                world,
                "HitPlanOid300Victim",
                300,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true);
            TypedCharacter skippedVictim = CreateEntity(
                world,
                "HitPlanSkippedVictim",
                7142,
                2,
                LF2ObjectType.Character,
                3,
                15,
                hasItr: false,
                hasBody: true);
            TypedCharacter laterAttacker = CreateEntity(
                world,
                "HitPlanLaterAttacker",
                7143,
                10,
                LF2ObjectType.Character,
                4,
                5000,
                hasItr: true,
                hasBody: false);
            TypedCharacter laterVictim = CreateEntity(
                world,
                "HitPlanLaterVictim",
                7144,
                11,
                LF2ObjectType.Character,
                5,
                5010,
                hasItr: false,
                hasBody: true);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(abortingAttacker.Runtime.HitCandidateCount, Is.EqualTo(2));
            Assert.That(laterAttacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(745);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.PlannedCandidateCount, Is.EqualTo(3));
            Assert.That(diagnostics.ObservedCandidateCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedDispositionCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedDispatchCount, Is.EqualTo(2));
            Assert.That(diagnostics.ObservedAbortTerminationCount, Is.EqualTo(1));
            Assert.That(diagnostics.SkippedCandidateCountAfterAbort, Is.EqualTo(1));
            Assert.That(diagnostics.ObservationMismatchCount, Is.Zero);
            Assert.That(
                world.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                    0,
                    out BattleHitExecutionPlanEntryView oid300Entry),
                Is.True);
            Assert.That(oid300Entry.DispositionObserved, Is.True);
            Assert.That(
                oid300Entry.ExpectedDisposition,
                Is.EqualTo(BattleHitCandidateDisposition.Oid300Redirect));
            Assert.That(
                oid300Entry.ObservedDisposition,
                Is.EqualTo(BattleHitCandidateDisposition.Oid300Redirect));
            Assert.That(oid300Victim.Health.HP, Is.EqualTo(100));
            Assert.That(skippedVictim.Health.HP, Is.EqualTo(100));
            Assert.That(laterVictim.Health.HP, Is.LessThan(100));
        }

        [Test]
        public void ShadowCompare_FalseAbortTerminationFailsPlanClosed()
        {
            Scenario scenario = CreateScenario();
            scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            scenario.World.CaptureBattleHitExecutionPlanPass(
                746,
                BattleHitExecutionPass.Character);
            Assert.That(
                scenario.World.BeginBattleHitExecutionPlanLegacyObservation(
                    746,
                    BattleHitExecutionPass.Character),
                Is.True);
            Assert.That(
                scenario.World.SceneQuery.TryGetCollisionCandidateRange(
                    scenario.CharacterAttacker,
                    out CollisionCandidateRange candidates),
                Is.True);
            Assert.That(candidates.TryGet(0, out SceneQueryHit hit), Is.True);
            LF2Entity target = hit.ResolveCurrentTarget(scenario.World);
            LF2FrameData frame = scenario.CharacterAttacker.GetCollisionFrameData();
            InteractionArea resolvedItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                scenario.CharacterAttacker,
                target,
                frame,
                frame.itrs[hit.ItrIndex],
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            scenario.World.ObserveBattleHitExecutionPlanLegacyPreprocess(
                scenario.CharacterAttacker,
                target,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
            scenario.World.PrepareBattleHitExecutionPlanLegacyDispatchObservation(
                scenario.CharacterAttacker,
                target,
                resolvedItr);
            scenario.World.ObserveBattleHitExecutionPlanLegacyDispatch(
                scenario.CharacterAttacker,
                dispatchSucceeded: false,
                terminatedRemainingCandidates: true);
            scenario.World.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.False);
            Assert.That(
                diagnostics.FirstFailureReason,
                Is.EqualTo(
                    BattleHitExecutionPlanFailureReason.ObservationDispatchMismatch));
            Assert.That(diagnostics.ObservedDispatchCount, Is.EqualTo(1));
            Assert.That(diagnostics.ObservationMismatchCount, Is.EqualTo(1));
        }

        private static void ObserveSingleCandidate(
            SimulationWorld world,
            LF2Entity attacker,
            int tick)
        {
            world.CaptureBattleHitExecutionPlanPass(
                tick,
                BattleHitExecutionPass.Character);
            if (!world.BeginBattleHitExecutionPlanLegacyObservation(
                    tick,
                    BattleHitExecutionPass.Character))
            {
                throw new InvalidOperationException(
                    "Expected hit-plan legacy observation to begin.");
            }
            if (!world.SceneQuery.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange candidates))
            {
                throw new InvalidOperationException(
                    "Expected frozen collision candidate range to remain readable.");
            }
            if (!candidates.TryGet(0, out SceneQueryHit hit))
            {
                throw new InvalidOperationException(
                    "Expected frozen collision candidate to remain readable.");
            }
            LF2Entity target = hit.ResolveCurrentTarget(world);
            LF2FrameData frame = attacker.GetCollisionFrameData();
            InteractionArea resolvedItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                frame,
                frame.itrs[hit.ItrIndex],
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            world.ObserveBattleHitExecutionPlanLegacyPreprocess(
                attacker,
                target,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
            world.ObserveBattleHitExecutionPlanLegacyDisposition(
                attacker,
                target,
                resolvedItr,
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    target,
                    resolvedItr,
                    consumeGateAccepted: true));
            world.EndBattleHitExecutionPlanLegacyObservation();
        }

        private static void PrepareSingleWriterEffectObservation(
            SimulationWorld world,
            LF2Entity attacker,
            int tick,
            out LF2Entity target,
            out InteractionArea resolvedItr)
        {
            world.CaptureBattleHitExecutionPlanPass(
                tick,
                BattleHitExecutionPass.Character);
            if (!world.BeginBattleHitExecutionPlanLegacyObservation(
                    tick,
                    BattleHitExecutionPass.Character))
            {
                throw new InvalidOperationException(
                    "Expected hit-plan legacy observation to begin.");
            }
            if (!world.SceneQuery.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange candidates) ||
                !candidates.TryGet(0, out SceneQueryHit hit))
            {
                throw new InvalidOperationException(
                    "Expected frozen collision candidate to remain readable.");
            }

            target = hit.ResolveCurrentTarget(world);
            LF2FrameData frame = attacker.GetCollisionFrameData();
            resolvedItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                frame,
                frame.itrs[hit.ItrIndex],
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            world.ObserveBattleHitExecutionPlanLegacyPreprocess(
                attacker,
                target,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
            BattleHitCandidateDisposition disposition =
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    target,
                    resolvedItr,
                    consumeGateAccepted: true);
            world.ObserveBattleHitExecutionPlanLegacyDisposition(
                attacker,
                target,
                resolvedItr,
                disposition);
            world.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                attacker,
                target,
                resolvedItr,
                disposition);
        }

        private static void PrepareSingleLifecycleEffectObservation(
            SimulationWorld world,
            LF2Entity attacker,
            int tick)
        {
            world.CaptureBattleHitExecutionPlanPass(
                tick,
                BattleHitExecutionPass.Object);
            if (!world.BeginBattleHitExecutionPlanLegacyObservation(
                    tick,
                    BattleHitExecutionPass.Object))
            {
                throw new InvalidOperationException(
                    "Expected object hit-plan legacy observation to begin.");
            }
            if (!world.SceneQuery.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange candidates) ||
                !candidates.TryGet(0, out SceneQueryHit hit))
            {
                throw new InvalidOperationException(
                    "Expected frozen OID C9 collision candidate to remain readable.");
            }

            LF2Entity target = hit.ResolveCurrentTarget(world);
            LF2FrameData frame = attacker.GetCollisionFrameData();
            InteractionArea resolvedItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                frame,
                frame.itrs[hit.ItrIndex],
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            world.ObserveBattleHitExecutionPlanLegacyPreprocess(
                attacker,
                target,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
            BattleHitCandidateDisposition disposition =
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    target,
                    resolvedItr,
                    consumeGateAccepted: true);
            world.ObserveBattleHitExecutionPlanLegacyDisposition(
                attacker,
                target,
                resolvedItr,
                disposition);
            world.PrepareBattleHitExecutionPlanLegacyLifecycleEffectObservation(
                attacker,
                target,
                resolvedItr,
                disposition);
        }

        private static void ObserveSingleCandidateWithConsumeEffects(
            SimulationWorld world,
            LF2Entity attacker,
            int tick)
        {
            world.CaptureBattleHitExecutionPlanPass(
                tick,
                BattleHitExecutionPass.Character);
            if (!world.BeginBattleHitExecutionPlanLegacyObservation(
                    tick,
                    BattleHitExecutionPass.Character))
            {
                throw new InvalidOperationException(
                    "Expected hit-plan legacy observation to begin.");
            }
            if (!world.SceneQuery.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange candidates) ||
                !candidates.TryGet(0, out SceneQueryHit hit))
            {
                throw new InvalidOperationException(
                    "Expected frozen collision candidate to remain readable.");
            }

            LF2Entity target = hit.ResolveCurrentTarget(world);
            LF2FrameData frame = attacker.GetCollisionFrameData();
            InteractionArea resolvedItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                frame,
                frame.itrs[hit.ItrIndex],
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            world.ObserveBattleHitExecutionPlanLegacyPreprocess(
                attacker,
                target,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
            world.ObserveBattleHitExecutionPlanLegacyDisposition(
                attacker,
                target,
                resolvedItr,
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    target,
                    resolvedItr,
                    consumeGateAccepted: true));
            world.PrepareBattleHitExecutionPlanLegacyConsumeEffectsObservation(
                attacker,
                target);
            SceneQueryHit consumeHit = new SceneQueryHit(
                target,
                hit.BodyX,
                hit.ItrIndex,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
            ((LF2Character)attacker).ApplyReleaseSceneQueryConsumeEffectsInternal(
                consumeHit);
            if (consumeHit.ZeroAttackerHpOnConsume && attacker.Health.HP != 0)
            {
                throw new InvalidOperationException(
                    $"Encoded zero-HP consume flag was not applied; " +
                    $"flag={consumeHit.ZeroAttackerHpOnConsume}, hp={attacker.Health.HP}.");
            }
            world.ObserveBattleHitExecutionPlanLegacyConsumeEffects(
                attacker,
                target);
            world.PrepareBattleHitExecutionPlanLegacyDispatchObservation(
                attacker,
                target,
                resolvedItr);
            world.ObserveBattleHitExecutionPlanLegacyDispatch(
                attacker,
                dispatchSucceeded: false,
                terminatedRemainingCandidates: false);
            world.EndBattleHitExecutionPlanLegacyObservation();
        }

        private static string DescribeDiagnostics(
            BattleHitExecutionPlanDiagnostics diagnostics)
        {
            return
                $"mode={diagnostics.Mode}, tick={diagnostics.CapturedTick}, " +
                $"characterPasses={diagnostics.CharacterPassCaptureCount}, " +
                $"objectPasses={diagnostics.ObjectPassCaptureCount}, " +
                $"planned={diagnostics.PlannedCandidateCount}, " +
                $"observationPasses={diagnostics.ObservationPassCount}, " +
                $"observed={diagnostics.ObservedCandidateCount}, " +
                $"preprocess={diagnostics.ObservedPreprocessCount}, " +
                $"dispositions={diagnostics.ObservedDispositionCount}, " +
                $"consumeEffects={diagnostics.ObservedConsumeEffectsCount}, " +
                $"writerEffects={diagnostics.ObservedWriterEffectCount}, " +
                $"lifecycleEffects={diagnostics.ObservedLifecycleEffectCount}, " +
                $"dispatches={diagnostics.ObservedDispatchCount}, " +
                $"abortTerminations={diagnostics.ObservedAbortTerminationCount}, " +
                $"abortSkipped={diagnostics.SkippedCandidateCountAfterAbort}, " +
                $"mismatches={diagnostics.ObservationMismatchCount}, " +
                $"failures={diagnostics.FailureCount}, " +
                $"consumeDiff=0x{diagnostics.LastConsumeEffectsDifferenceMask:X}, " +
                $"writerDiff=0x{diagnostics.LastWriterEffectDifferenceMask:X}, " +
                $"lifecycleDiff=0x{diagnostics.LastLifecycleEffectDifferenceMask:X}, " +
                $"firstFailure={diagnostics.FirstFailureReason}, " +
                $"firstSlot={diagnostics.FirstFailureAttackerSlot}, " +
                $"firstOrdinal={diagnostics.FirstFailureCandidateOrdinal}.";
        }

        private static Scenario CreateScenario()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            TypedCharacter characterAttacker = CreateEntity(
                world,
                "HitPlanCharacterAttacker",
                7100,
                0,
                LF2ObjectType.Character,
                1,
                0,
                hasItr: true,
                hasBody: false);
            TypedCharacter characterVictim = CreateEntity(
                world,
                "HitPlanCharacterVictim",
                7101,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true);
            TypedCharacter objectAttacker = CreateEntity(
                world,
                "HitPlanObjectAttacker",
                7120,
                20,
                LF2ObjectType.SpecialAttack,
                3,
                1000,
                hasItr: true,
                hasBody: false);
            TypedCharacter objectVictim = CreateEntity(
                world,
                "HitPlanObjectVictim",
                7121,
                21,
                LF2ObjectType.Character,
                4,
                1010,
                hasItr: false,
                hasBody: true);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(characterAttacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            Assert.That(objectAttacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            return new Scenario(
                world,
                characterAttacker,
                characterVictim,
                objectAttacker,
                objectVictim);
        }

        private static void CreateOidC9LifecycleScenario(
            out SimulationWorld world,
            out LF2SpecialAttack attacker,
            out TypedCharacter target)
        {
            world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            attacker = CreateSpecialAttackEntity(
                world,
                "HitPlanOidC9Attacker",
                0xC9,
                0,
                1,
                0);
            target = CreateEntity(
                world,
                "HitPlanOidC9Target",
                7277,
                1,
                LF2ObjectType.Character,
                2,
                10,
                hasItr: false,
                hasBody: true);
            attacker.Frame.D.itrs.Add(new InteractionArea
            {
                kind = 0,
                x = -30,
                y = -10,
                w = 60,
                h = 20,
                zwidth = 15,
                injury = 10,
                fall = 10,
                dvx = 1,
                arest = 2,
                vrest = 3,
                effect = 0,
            });
            target.Health.HP = 100;
            target.Health.HPBound = 100;
            target.KillCount = -1;
            target.Unk344 = 1;
            world.Rng.Seed(0x10293847u);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            string name,
            int objectId,
            int slot,
            LF2ObjectType objectType,
            int team,
            int x,
            bool hasItr,
            bool hasBody,
            LF2FrameData extraFrame = null)
        {
            LF2FrameData frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (hasItr)
            {
                frame.itrs.Add(new InteractionArea
                {
                    kind = 0,
                    x = -30,
                    y = -10,
                    w = 60,
                    h = 20,
                    zwidth = 15,
                    injury = 10,
                    dvx = 1,
                    arest = 4,
                    vrest = 1,
                });
            }
            if (hasBody)
            {
                if (objectId == 300)
                {
                    frame.bodies.Add(new BodyBox
                    {
                        kind = 0,
                        x = 1001,
                        y = -10,
                        w = 20,
                        h = 20,
                    });
                }
                frame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            }

            var frames = new List<LF2FrameData> { frame };
            if (extraFrame != null)
                frames.Add(extraFrame);
            if (objectType == LF2ObjectType.HeavyWeapon)
            {
                for (int frameId = 1; frameId < 6; frameId++)
                {
                    frames.Add(new LF2FrameData
                    {
                        frameId = frameId,
                        state = LF2States.Standing,
                        wait = 1,
                        next = frameId,
                        centerx = 0,
                        centery = 0,
                    });
                }
            }
            if (objectId == 300)
            {
                var futureFrame = new LF2FrameData
                {
                    frameId = 6,
                    state = LF2States.Standing,
                    wait = 1,
                    next = 6,
                    centerx = 0,
                    centery = 0,
                };
                futureFrame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
                frames.Add(futureFrame);
            }

            var data = new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = frames,
            };
            var entity = new TypedCharacter(objectType);
            entity.ModuleInitialize();
            entity.Name = name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = 0;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.Health.HP3 = 100;
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private static LF2Weapon CreateWeaponEntity(
            SimulationWorld world,
            string name,
            int objectId,
            int slot,
            LF2ObjectType objectType,
            int team,
            int x)
        {
            var frames = new List<LF2FrameData>(21);
            for (int frameId = 0; frameId <= 20; frameId++)
            {
                var frame = new LF2FrameData
                {
                    frameId = frameId,
                    state = LF2States.Standing,
                    wait = 1,
                    next = frameId,
                    centerx = 0,
                    centery = 0,
                };
                if (frameId == 0)
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
                frames.Add(frame);
            }

            var data = new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = frames,
            };
            var weapon = new LF2Weapon();
            weapon.SetWeaponType((int)objectType);
            weapon.Name = name;
            weapon.ObjectId = objectId;
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            weapon.Frame.D = weapon.FrameCache.GetFrameDataById(0);
            weapon.Frame.N = 0;
            weapon.Frame.PN = 0;
            weapon.Frame.Prev = 0;
            weapon.Frame.Prev2 = 0;
            weapon.Frame.Prev2D = weapon.Frame.D;
            weapon.Runtime.PrevFrame2 = 0;
            weapon.SetRequiredRuntimeSlot(slot);
            world.Register(weapon);
            weapon.Team = team;
            weapon.RelationTeam = team;
            weapon.Health.HP = 100;
            weapon.Health.HPBound = 100;
            weapon.Health.HP3 = 100;
            weapon.Runtime.SetPosition(x, 0, 0);
            weapon.Runtime.SetVelocity(0, 0, 0);
            weapon.Runtime.SyncIntegerPosition();
            weapon.RefreshRuntimeSnapshot();
            return weapon;
        }

        private static LF2SpecialAttack CreateSpecialAttackEntity(
            SimulationWorld world,
            string name,
            int objectId,
            int slot,
            int team,
            int x)
        {
            var frame0 = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            frame0.bodies.Add(new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            });
            var frame30 = new LF2FrameData
            {
                frameId = 30,
                state = LF2States.Standing,
                wait = 1,
                next = 30,
                centerx = 0,
                centery = 0,
            };
            var frame20 = new LF2FrameData
            {
                frameId = 20,
                state = LF2States.Standing,
                wait = 1,
                next = 20,
                centerx = 0,
                centery = 0,
            };
            var frame33 = new LF2FrameData
            {
                frameId = 33,
                state = LF2States.Standing,
                wait = 1,
                next = 33,
                centerx = 0,
                centery = 0,
            };
            var frame40 = new LF2FrameData
            {
                frameId = 40,
                state = LF2States.Standing,
                wait = 1,
                next = 40,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = new List<LF2FrameData> { frame0, frame20, frame30, frame33, frame40 },
            };
            var specialAttack = new LF2SpecialAttack
            {
                Name = name,
                ObjectId = objectId,
            };
            specialAttack.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            specialAttack.Frame.D = specialAttack.FrameCache.GetFrameDataById(0);
            specialAttack.Frame.N = 0;
            specialAttack.Frame.PN = 0;
            specialAttack.Frame.Prev = 0;
            specialAttack.Frame.Prev2 = 0;
            specialAttack.Frame.Prev2D = specialAttack.Frame.D;
            specialAttack.Runtime.PrevFrame2 = 0;
            specialAttack.SetRequiredRuntimeSlot(slot);
            world.Register(specialAttack);
            specialAttack.Team = team;
            specialAttack.RelationTeam = team;
            specialAttack.Health.HP = 100;
            specialAttack.Health.HPBound = 100;
            specialAttack.Health.HP3 = 100;
            specialAttack.Runtime.SetPosition(x, 0, 0);
            specialAttack.Runtime.SetVelocity(0, 0, 0);
            specialAttack.Runtime.SyncIntegerPosition();
            specialAttack.RefreshRuntimeSnapshot();
            return specialAttack;
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType objectType;

            internal TypedCharacter(LF2ObjectType objectType)
            {
                this.objectType = objectType;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)objectType;
            }
        }

        private readonly struct Scenario
        {
            internal Scenario(
                SimulationWorld world,
                TypedCharacter characterAttacker,
                TypedCharacter characterVictim,
                TypedCharacter objectAttacker,
                TypedCharacter objectVictim)
            {
                World = world;
                CharacterAttacker = characterAttacker;
                CharacterVictim = characterVictim;
                ObjectAttacker = objectAttacker;
                ObjectVictim = objectVictim;
            }

            internal SimulationWorld World { get; }
            internal TypedCharacter CharacterAttacker { get; }
            internal TypedCharacter CharacterVictim { get; }
            internal TypedCharacter ObjectAttacker { get; }
            internal TypedCharacter ObjectVictim { get; }
        }
    }
}
#endif
