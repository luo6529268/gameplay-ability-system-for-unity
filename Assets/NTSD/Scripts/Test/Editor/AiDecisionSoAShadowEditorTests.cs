#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiDecisionSoAShadowEditorTests
    {
        private const BindingFlags InstanceMembers =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        [Test]
        public void BattleTickPhaseDiagnostics_ProfilerScopesRemainBalancedAcrossLifecycleResets()
        {
            var diagnostics = new BattleTickPhaseDiagnostics();
            diagnostics.BeginTick(1);
            diagnostics.BeginPhase(BattleTickPhase.BattleFlow);
            diagnostics.EndTick();
            Assert.That(diagnostics.HasActivePhaseForDiagnostics, Is.False);
            Assert.That(diagnostics.LastTickIndex, Is.EqualTo(-1));

            diagnostics.SetEnabled(true);
            diagnostics.BeginTick(41);
            diagnostics.BeginPhase(BattleTickPhase.BattleFlow);
            Assert.That(diagnostics.ActivePhaseForDiagnostics,
                Is.EqualTo(BattleTickPhase.BattleFlow));

            System.Threading.Thread.SpinWait(5000);
            diagnostics.BeginPhase(BattleTickPhase.Cooldown);
            Assert.That(diagnostics.GetLastElapsedTimestampTicks(
                BattleTickPhase.BattleFlow), Is.GreaterThan(0));
            Assert.That(diagnostics.ActivePhaseForDiagnostics,
                Is.EqualTo(BattleTickPhase.Cooldown));

            diagnostics.EndPhase(BattleTickPhase.BattleFlow);
            Assert.That(diagnostics.ActivePhaseForDiagnostics,
                Is.EqualTo(BattleTickPhase.Cooldown));
            diagnostics.EndTick();
            diagnostics.EndTick();
            Assert.That(diagnostics.HasActivePhaseForDiagnostics, Is.False);
            Assert.That(diagnostics.LastTickIndex, Is.EqualTo(41));

            diagnostics.BeginPhase(BattleTickPhase.Stage);
            diagnostics.BeginPhase(BattleTickPhase.Stage);
            diagnostics.BeginTick(42);
            Assert.That(diagnostics.HasActivePhaseForDiagnostics, Is.False);
            Assert.That(diagnostics.GetLastPhaseSumTimestampTicks(), Is.Zero);

            diagnostics.BeginPhase(BattleTickPhase.BattleResults);
            diagnostics.SetEnabled(false);
            Assert.That(diagnostics.Enabled, Is.False);
            Assert.That(diagnostics.HasActivePhaseForDiagnostics, Is.False);
            Assert.That(diagnostics.LastTickIndex, Is.EqualTo(-1));
            Assert.That(diagnostics.GetLastPhaseSumTimestampTicks(), Is.Zero);
        }

        [Test]
        public void UnifiedSnapshotShadow_DisabledFastPathDoesNotAllocateOrRecordDuplicateCapture()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            BattleAiInputDetailDiagnostics timing =
                world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(
                world.AiUnifiedSnapshotShadowMode,
                Is.EqualTo(AiUnifiedSnapshotShadowMode.Disabled));
            Assert.That(world.AiUnifiedSnapshotShadowRowsAllocatedForDiagnostics, Is.False);
            Assert.That(world.AiUnifiedSnapshotShadowBuildCountForDiagnostics, Is.Zero);
            Assert.That(
                timing.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateCapture),
                Is.Zero);
            Assert.That(
                timing.GetLastSlotVisitCount(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateCapture),
                Is.Zero);
            Assert.That(
                timing.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateIndexBuild),
                Is.Zero);
        }

        [Test]
        public void UnifiedSnapshotShadow_CandidateAndIndexedCanonicalMatchWithExplicitBoundaryEncodings()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            RegisterCharacter(world, 1, 2, 2, 90, 0, 0, 9, false);
            self.Runtime.XBoundPositive = true;
            world.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(0x51A0u);
            Invoke(world, "SetAiSoACandidateModeForSelfCheck", true);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Shadow;
            world.ResetAiUnifiedSnapshotShadowDiagnostics();
            BattleAiInputDetailDiagnostics timing =
                world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiUnifiedSnapshotShadowBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics,
                Is.EqualTo(world.RuntimeSlotCapacityForDiagnostics));
            Assert.That(world.AiUnifiedSnapshotShadowRefreshCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiUnifiedSnapshotShadowSensingComparedCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(world.AiUnifiedSnapshotShadowDecisionComparedCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(world.AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(world.AiUnifiedSnapshotShadowUnavailableCountForDiagnostics, Is.Zero);
            Assert.That(world.AiUnifiedSnapshotShadowMismatchCountForDiagnostics, Is.Zero);
            Assert.That(
                timing.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateIndexBuild),
                Is.GreaterThan(0),
                "observer index work must be timed outside production SnapshotIndexBuild");
            Assert.That(
                world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Kind,
                Is.EqualTo(AiUnifiedSnapshotMismatchKind.None));
        }

        [Test]
        public void UnifiedSnapshotShadow_BitExactDecisionBoundaryMismatchIsReported()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.XBoundPositive = true;
            world.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(0xB0A1u);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Shadow;
            world.SetAiUnifiedSnapshotBoundaryMutationForSelfCheck(
                AiUnifiedSnapshotConsumer.IndexedDecision,
                0,
                1);
            world.ResetAiUnifiedSnapshotShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiUnifiedSnapshotShadowMismatchCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(
                world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Consumer,
                Is.EqualTo(AiUnifiedSnapshotConsumer.IndexedDecision));
            Assert.That(
                world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Kind,
                Is.EqualTo(AiUnifiedSnapshotMismatchKind.BoundaryFlags));
            Assert.That(world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Slot,
                Is.EqualTo(0));
        }

        [TestCase(AiUnifiedSnapshotExceptionStage.Prepare)]
        [TestCase(AiUnifiedSnapshotExceptionStage.Capture)]
        [TestCase(AiUnifiedSnapshotExceptionStage.BuildIndexes)]
        [TestCase(AiUnifiedSnapshotExceptionStage.Validate)]
        [TestCase(AiUnifiedSnapshotExceptionStage.InitialSensingCompare)]
        [TestCase(AiUnifiedSnapshotExceptionStage.InitialDecisionCompare)]
        [TestCase(AiUnifiedSnapshotExceptionStage.Refresh)]
        [TestCase(AiUnifiedSnapshotExceptionStage.RefreshCapture)]
        [TestCase(AiUnifiedSnapshotExceptionStage.RefreshBuildIndexes)]
        [TestCase(AiUnifiedSnapshotExceptionStage.RefreshCompare)]
        public void UnifiedSnapshotShadow_InjectedObserverExceptionNeverEscapesOrChangesAuthority(
            AiUnifiedSnapshotExceptionStage stage)
        {
            var expectedWorld = new SimulationWorld();
            var actualWorld = new SimulationWorld();
            LF2Character expectedSelf = RegisterCharacter(
                expectedWorld, 0, 7, 1, 0, 0, 0, 2, true);
            LF2Character actualSelf = RegisterCharacter(
                actualWorld, 0, 7, 1, 0, 0, 0, 2, true);
            RegisterCharacter(expectedWorld, 1, 2, 2, 90, 0, 0, 9, false);
            RegisterCharacter(actualWorld, 1, 2, 2, 90, 0, 0, 9, false);
            expectedWorld.Runtime.Flow.InputPhase = 2;
            actualWorld.Runtime.Flow.InputPhase = 2;
            expectedWorld.Rng.Seed(0xE771u);
            actualWorld.Rng.Seed(0xE771u);
            Invoke(expectedWorld, "SetAiSoACandidateModeForSelfCheck", true);
            Invoke(actualWorld, "SetAiSoACandidateModeForSelfCheck", true);
            expectedWorld.AiDecisionExecutionMode =
                AiDecisionExecutionMode.IndexedCanonical;
            actualWorld.AiDecisionExecutionMode =
                AiDecisionExecutionMode.IndexedCanonical;
            actualWorld.AiUnifiedSnapshotShadowMode =
                AiUnifiedSnapshotShadowMode.Shadow;
            actualWorld.ResetAiUnifiedSnapshotShadowDiagnostics();
            actualWorld.SetAiUnifiedSnapshotExceptionForSelfCheck(stage);

            expectedWorld.CharacterInputAll(2);
            Assert.DoesNotThrow(() => actualWorld.CharacterInputAll(2));

            AssertDecisionStateEqual(
                expectedWorld,
                expectedSelf,
                actualWorld,
                actualSelf);
            Assert.That(
                actualWorld.AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics,
                Is.EqualTo(stage));
            Assert.That(
                actualWorld.AiUnifiedSnapshotShadowFirstExceptionTypeForDiagnostics,
                Is.EqualTo(typeof(InvalidOperationException)));
            Assert.That(
                actualWorld.AiUnifiedSnapshotShadowUnavailableCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                actualWorld.AiUnifiedSnapshotShadowMismatchCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void UnifiedSnapshotShadow_ThousandCharactersKeepComparisonVisitsLinear()
        {
            const int characterCount = 1000;
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            for (int slot = 0; slot < characterCount; slot++)
            {
                RegisterCharacter(
                    world,
                    slot,
                    slot + 1,
                    slot & 1,
                    slot,
                    0,
                    0,
                    2,
                    false);
            }
            Invoke(world, "SetAiSoACandidateModeForSelfCheck", true);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Shadow;
            world.ResetAiUnifiedSnapshotShadowDiagnostics();

            world.CharacterInputAll(2);

            long capacity = world.RuntimeSlotCapacityForDiagnostics;
            Assert.That(world.AiUnifiedSnapshotShadowBuildCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(world.AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics,
                Is.EqualTo(capacity));
            Assert.That(world.AiUnifiedSnapshotShadowRefreshCountForDiagnostics,
                Is.EqualTo(characterCount));
            Assert.That(
                world.AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics,
                Is.EqualTo(2L * capacity));
            Assert.That(
                world.AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics,
                Is.EqualTo(2L * characterCount));
            Assert.That(
                world.AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics +
                world.AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics,
                Is.LessThanOrEqualTo(4L * capacity),
                "per-character refresh comparison must remain O(1), not rescan capacity");
            Assert.That(
                world.AiUnifiedSnapshotShadowDerivedComparisonEntryVisitCountForDiagnostics,
                Is.LessThanOrEqualTo(4L * capacity),
                "derived arrays are compared only by the two initial full oracles");
            Assert.That(
                world.AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics,
                Is.EqualTo(2L * characterCount));
            Assert.That(
                world.AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics,
                Is.Zero,
                "refresh must use the O(1) mutation witness, never a derived-array loop");
            Assert.That(world.AiUnifiedSnapshotShadowUnavailableCountForDiagnostics,
                Is.Zero);
            Assert.That(world.AiUnifiedSnapshotShadowMismatchCountForDiagnostics,
                Is.Zero);
        }

        [TestCase(UnifiedRefreshMutationKind.X)]
        [TestCase(UnifiedRefreshMutationKind.Team)]
        [TestCase(UnifiedRefreshMutationKind.Hp)]
        [TestCase(UnifiedRefreshMutationKind.LivingAndRole)]
        public void UnifiedSnapshotShadow_MutationWitnessCoversDerivedProductInputs(
            UnifiedRefreshMutationKind mutationKind)
        {
            var world = CreateUnifiedCandidateWorld();
            LF2Character character = RegisterCharacter(
                world, 0, 7, 1, 10, 0, 0, 2, false);
            world.ResetAiUnifiedSnapshotShadowDiagnostics();

            PrepareUnifiedManualPass(world);
            try
            {
                switch (mutationKind)
                {
                    case UnifiedRefreshMutationKind.X:
                        character.Runtime.SetPosition(75, 0, 0);
                        character.Runtime.SyncIntegerPosition();
                        break;
                    case UnifiedRefreshMutationKind.Team:
                        character.Runtime.RelationTeam = 3;
                        break;
                    case UnifiedRefreshMutationKind.Hp:
                        character.Runtime.HP = 450;
                        break;
                    case UnifiedRefreshMutationKind.LivingAndRole:
                        character.Runtime.HP = 0;
                        break;
                    default:
                        Assert.Fail("Unknown mutation kind.");
                        break;
                }

                RefreshUnifiedManualPass(world, character);

                Assert.That(
                    world.AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics,
                    Is.EqualTo(2));
                Assert.That(
                    world.AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics,
                    Is.Zero);
                Assert.That(world.AiUnifiedSnapshotShadowUnavailableCountForDiagnostics,
                    Is.Zero);
                Assert.That(world.AiUnifiedSnapshotShadowMismatchCountForDiagnostics,
                    Is.Zero);
            }
            finally
            {
                EndUnifiedManualPass(world);
            }
        }

        [Test]
        public void UnifiedSnapshotShadow_SpecialMembershipMutationInvalidatesPass()
        {
            var world = CreateUnifiedCandidateWorld();
            LF2Character character = RegisterCharacter(
                world, 20, 100, 1, 10, 0, 0, 2, false);
            world.ResetAiUnifiedSnapshotShadowDiagnostics();

            PrepareUnifiedManualPass(world);
            try
            {
                character.ObjectId = 2;
                RefreshUnifiedManualPass(world, character);

                Assert.That(world.AiUnifiedSnapshotShadowUnavailableCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(
                    world.AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics,
                    Is.Zero);
                Assert.That(
                    world.AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics,
                    Is.Zero);
            }
            finally
            {
                EndUnifiedManualPass(world);
            }
        }

        [TestCase(
            AiUnifiedSnapshotProductMutationKind.FallbackReference,
            AiUnifiedSnapshotMismatchKind.FallbackReference,
            AiUnifiedSnapshotField.FallbackSlot)]
        [TestCase(
            AiUnifiedSnapshotProductMutationKind.MoveModeFirst10Hp,
            AiUnifiedSnapshotMismatchKind.MoveModeProduct,
            AiUnifiedSnapshotField.MoveModeHp)]
        public void UnifiedSnapshotShadow_FallbackAndFirstTenProductsAreCompared(
            AiUnifiedSnapshotProductMutationKind mutationKind,
            AiUnifiedSnapshotMismatchKind expectedKind,
            AiUnifiedSnapshotField expectedField)
        {
            var world = CreateUnifiedCandidateWorld();
            RegisterCharacter(world, 0, 7, 1, 10, 0, 0, 2, false);
            world.SetAiUnifiedSnapshotProductMutationForSelfCheck(mutationKind, 0);
            world.ResetAiUnifiedSnapshotShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiUnifiedSnapshotShadowMismatchCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Kind,
                Is.EqualTo(expectedKind));
            Assert.That(world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Field,
                Is.EqualTo(expectedField));
            Assert.That(world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Slot,
                Is.EqualTo(0));
        }

        [Test]
        public void UnifiedSnapshotShadow_MutationWitnessMismatchIsReported()
        {
            var world = CreateUnifiedCandidateWorld();
            RegisterCharacter(world, 0, 7, 1, 10, 0, 0, 2, false);
            world.SetAiUnifiedSnapshotWitnessMutationForSelfCheck(
                AiUnifiedSnapshotConsumer.SoASensing);
            world.ResetAiUnifiedSnapshotShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiUnifiedSnapshotShadowMismatchCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Consumer,
                Is.EqualTo(AiUnifiedSnapshotConsumer.SoASensing));
            Assert.That(world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Kind,
                Is.EqualTo(AiUnifiedSnapshotMismatchKind.MutationWitness));
            Assert.That(world.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Field,
                Is.EqualTo(AiUnifiedSnapshotField.WitnessOrdinal));
        }

        [Test]
        public void UnifiedSnapshotShadow_MutationWitnessPreservesAuthorityState()
        {
            var expectedWorld = CreateUnifiedCandidateWorld();
            var actualWorld = CreateUnifiedCandidateWorld();
            LF2Character expectedSelf = RegisterCharacter(
                expectedWorld, 0, 7, 1, 10, 0, 0, 2, true);
            LF2Character actualSelf = RegisterCharacter(
                actualWorld, 0, 7, 1, 10, 0, 0, 2, true);
            RegisterCharacter(expectedWorld, 1, 2, 2, 90, 0, 0, 9, false);
            RegisterCharacter(actualWorld, 1, 2, 2, 90, 0, 0, 9, false);
            expectedWorld.AiUnifiedSnapshotShadowMode =
                AiUnifiedSnapshotShadowMode.Disabled;
            expectedWorld.Rng.Seed(0x7117u);
            actualWorld.Rng.Seed(0x7117u);
            actualWorld.ResetAiUnifiedSnapshotShadowDiagnostics();

            expectedWorld.CharacterInputAll(2);
            actualWorld.CharacterInputAll(2);

            AssertDecisionStateEqual(
                expectedWorld,
                expectedSelf,
                actualWorld,
                actualSelf);
            Assert.That(actualWorld.AiUnifiedSnapshotShadowMismatchCountForDiagnostics,
                Is.Zero);
            Assert.That(
                actualWorld.AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics,
                Is.EqualTo(4));
        }

        [Test]
        public void DisabledFastPath_RunsLegacyWithoutEnteringShadowOrRecorder()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.Unk3FC = 400;
            self.Runtime.Unk400 = 0;
            world.Runtime.Flow.AiDifficulty = 2;
            world.Runtime.Flow.AiRand3 = 6;
            world.Runtime.Flow.AiRand5 = 10;
            world.Runtime.Flow.AiRand15 = 30;
            world.Runtime.Flow.AiRand20 = 40;
            world.Rng.Seed(0x1234u);
            world.ResetAiDecisionShadowDiagnostics();
            ulong rngCallsBefore = world.Rng.CallCount;

            Assert.That(world.AiDecisionShadowMode,
                Is.EqualTo(AiDecisionShadowMode.Disabled));
            Assert.That(world.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.Legacy));

            world.CharacterInputAll(2);

            Assert.That(world.Rng.CallCount, Is.GreaterThan(rngCallsBefore),
                "the authoritative legacy core must still execute");
            Assert.That(world.AiDecisionShadowBeginInvocationCountForTests, Is.Zero);
            Assert.That(world.AiDecisionShadowCompleteInvocationCountForTests, Is.Zero);
            Assert.That(world.AiDecisionShadowEligibleCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowUnavailableCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowComparedCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowCloneRngCallCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowRowVisitCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionSharedRefreshCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionIndexedEligibleCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedAvailableCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedComparedCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowComparisonActiveForTests, Is.False);
            Assert.That(world.AiDecisionLegacyRngRecordingForTests, Is.False);
            Assert.That(world.AiDecisionLegacyRngCountForTests, Is.Zero);
            Assert.That(
                world.AiDecisionShadowFirstExceptionStageForDiagnostics,
                Is.EqualTo(AiDecisionShadowExceptionStage.None));
            Assert.That(world.AiDecisionShadowFirstExceptionTypeForDiagnostics, Is.Null);
        }

        [Test]
        public void IndexedCanonical_CommitsSameInputWorldTargetAndRngAsLegacy()
        {
            var legacy = new SimulationWorld();
            var indexed = new SimulationWorld();
            LF2Character legacySelf = RegisterCharacter(legacy, 0, 7, 1, 0, 0, 0, 2, true);
            LF2Character indexedSelf = RegisterCharacter(indexed, 0, 7, 1, 0, 0, 0, 2, true);
            RegisterCharacter(legacy, 1, 2, 2, 90, 0, 0, 9, false);
            RegisterCharacter(indexed, 1, 2, 2, 90, 0, 0, 9, false);
            legacy.Runtime.Flow.InputPhase = 2;
            indexed.Runtime.Flow.InputPhase = 2;
            legacy.Rng.Seed(0xC011u);
            indexed.Rng.Seed(0xC011u);
            indexed.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            indexed.ResetAiDecisionShadowDiagnostics();

            legacy.CharacterInputAll(2);
            indexed.CharacterInputAll(2);

            AssertDecisionStateEqual(legacy, legacySelf, indexed, indexedSelf);
            Assert.That(indexed.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(indexed.AiDecisionSharedRefreshCountForDiagnostics, Is.EqualTo(2),
                "the shared pass refreshes both the AI writer row and the non-AI target row");
            Assert.That(indexed.AiDecisionIndexedCanonicalEligibleCountForDiagnostics, Is.EqualTo(1));
            Assert.That(indexed.AiDecisionIndexedCanonicalCommittedCountForDiagnostics, Is.EqualTo(1));
            Assert.That(indexed.AiDecisionIndexedCanonicalFallbackCountForDiagnostics, Is.Zero);
            Assert.That(indexed.AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics, Is.Zero,
                "the canonical default must not double-run the Full oracle");
        }

        [Test]
        public void UnifiedAuthority_DirectCanonicalInputMatchesSnapshotCopyAcrossTicks()
        {
            var copied = new SimulationWorld();
            var direct = new SimulationWorld();
            copied.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            direct.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            direct.ConfigureAiDecisionOwnedInputModeForDiagnostics(
                AiDecisionOwnedInputMode.CanonicalStoreDirect);

            int[] slots = { 0, 3, 7 };
            var copiedCharacters = new LF2Character[slots.Length];
            var directCharacters = new LF2Character[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                copiedCharacters[index] = RegisterCharacter(
                    copied,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 80,
                    0,
                    0,
                    index == 1 ? 9 : 2,
                    true);
                directCharacters[index] = RegisterCharacter(
                    direct,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 80,
                    0,
                    0,
                    index == 1 ? 9 : 2,
                    true);
            }
            copied.Runtime.Flow.InputPhase = 2;
            direct.Runtime.Flow.InputPhase = 2;
            copied.Rng.Seed(0xD1EC7u);
            direct.Rng.Seed(0xD1EC7u);

            for (int tick = 2; tick < 18; tick++)
            {
                copied.CharacterInputAll(tick);
                direct.CharacterInputAll(tick);
                for (int index = 0; index < slots.Length; index++)
                {
                    AssertDecisionStateEqual(
                        copied,
                        copiedCharacters[index],
                        direct,
                        directCharacters[index]);
                }
            }

            Assert.That(
                direct.AiDecisionIndexedCanonicalCommittedCountForDiagnostics,
                Is.EqualTo(copied.AiDecisionIndexedCanonicalCommittedCountForDiagnostics));
            Assert.That(
                direct.AiDecisionIndexedCanonicalFallbackCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void IndexedCanonical_PreCommitFailureFallsBackWithoutPartialWrites()
        {
            var legacy = new SimulationWorld();
            var indexed = new SimulationWorld();
            LF2Character legacySelf = RegisterCharacter(legacy, 0, 7, 1, 0, 0, 0, 2, true);
            LF2Character indexedSelf = RegisterCharacter(indexed, 0, 7, 1, 0, 0, 0, 2, true);
            legacySelf.Runtime.Unk3FC = 400;
            indexedSelf.Runtime.Unk3FC = 400;
            legacySelf.Runtime.Unk400 = 0;
            indexedSelf.Runtime.Unk400 = 0;
            legacy.Runtime.Flow.AiRand3 = 6;
            indexed.Runtime.Flow.AiRand3 = 6;
            legacy.Rng.Seed(0xFA11u);
            indexed.Rng.Seed(0xFA11u);
            indexed.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            indexed.SetAiDecisionIndexedCanonicalPreCommitFailureForSelfCheck(
                AiDecisionAvailability.EpochMismatch);
            indexed.ResetAiDecisionShadowDiagnostics();

            legacy.CharacterInputAll(2);
            indexed.CharacterInputAll(2);

            AssertDecisionStateEqual(legacy, legacySelf, indexed, indexedSelf);
            Assert.That(indexed.AiDecisionIndexedCanonicalCommittedCountForDiagnostics, Is.Zero);
            Assert.That(indexed.AiDecisionIndexedCanonicalFallbackCountForDiagnostics, Is.EqualTo(1));
            Assert.That(indexed.AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics,
                Is.EqualTo(AiDecisionAvailability.EpochMismatch));
        }

        [Test]
        public void DataOrientedProfile_MatchesLegacyFullDispatcherForPosition38()
        {
            var legacy = new SimulationWorld();
            var dataOriented = new SimulationWorld();
            dataOriented.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character legacySelf = RegisterCharacter(
                legacy, 0, 52, 1, 0, 0, 0, 2, true);
            LF2Character dataSelf = RegisterCharacter(
                dataOriented, 0, 52, 1, 0, 0, 0, 2, true);
            LF2Character legacyTarget = RegisterCharacter(
                legacy, 1, 100, 2, 90, 0, 0, 3, false);
            RegisterCharacter(dataOriented, 1, 100, 2, 90, 0, 0, 3, false);
            legacy.Runtime.Flow.InputPhase = 2;
            dataOriented.Runtime.Flow.InputPhase = 2;
            legacySelf.Runtime.Unk360 = -1;
            dataSelf.Runtime.Unk360 = -1;
            legacy.Rng.Seed(27u);
            dataOriented.Rng.Seed(27u);

            legacy.CharacterInputAll(2);
            dataOriented.CharacterInputAll(2);

            Assert.That(legacySelf.Runtime.Unk360, Is.EqualTo(1));
            Assert.That(legacyTarget.GetState(), Is.EqualTo(3));
            Assert.That(legacySelf.Runtime.ComboDua, Is.EqualTo(3),
                "the fixture must reach source-derived position38 predicted-DUA branch");
            AssertDecisionStateEqual(
                legacy,
                legacySelf,
                dataOriented,
                dataSelf);
            Assert.That(
                dataOriented.AiDecisionIndexedCanonicalFallbackCountForDiagnostics,
                Is.Zero);
            Assert.That(
                dataOriented.AiDecisionIndexedCanonicalCommittedCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void IndexedCanonical_FullOracleUsesConfiguredLowFrequencySampling()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, true);
            RegisterCharacter(world, 1, 2, 2, 90, 0, 0, 9, true);
            world.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(0x0A11u);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiDecisionIndexedCanonicalFullOracleSampleInterval = 2;
            world.ResetAiDecisionShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiDecisionIndexedCanonicalEligibleCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionIndexedCanonicalCommittedCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(world.AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void DeepShadow_RemainsPerAiOracleAlongsideSharedLegacyRows()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, true);
            RegisterCharacter(world, 1, 2, 2, 90, 0, 0, 9, true);
            world.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(0xB17Au);
            world.AiDecisionShadowMode = AiDecisionShadowMode.Shadow;
            world.ResetAiDecisionShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionSharedRefreshCountForDiagnostics,
                Is.GreaterThanOrEqualTo(2));
            Assert.That(world.AiDecisionIndexedEligibleCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedAvailableCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedComparedCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void SharedShadow_BuildsOnceAndRefreshesLowSlotBeforeHighSlotEvaluation()
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            LF2Character low = RegisterCharacter(world, 0, 1, 1, 10, 0, 0, 0, true);
            LF2Character high = RegisterCharacter(world, 1, 2, 2, 90, 0, 0, 9, true);
            RegisterCharacter(world, 2, 3, 1, 180, 0, 0, 0, false);
            low.Runtime.Unk360 = 1;
            high.Runtime.Unk360 = -1;
            world.Rng.Seed(0xB17Au);
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            world.SetAiDecisionSharedPostLegacyStateMutationForSelfCheck(0, 14);
            world.ResetAiDecisionShadowDiagnostics();

            LF2FrameData sharedEmptyFrame = GetSharedEmptyFrameForIsolation();
            int originalEmptyFrameState = sharedEmptyFrame.state;
            try
            {
                world.CharacterInputAll(2);

                Assert.That(low.Frame.D, Is.SameAs(sharedEmptyFrame),
                    "the fixture must explicitly witness the shared missing-frame sentinel it mutates");
                Assert.That(sharedEmptyFrame.state, Is.EqualTo(14));
                Assert.That(world.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
                Assert.That(world.AiDecisionSharedRefreshCountForDiagnostics,
                    Is.GreaterThanOrEqualTo(2));
                Assert.That(world.AiDecisionShadowEligibleCountForDiagnostics, Is.EqualTo(2));
                Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.EqualTo(2));
                Assert.That(world.AiDecisionShadowComparedCountForDiagnostics, Is.EqualTo(2));
                Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
                Assert.That(world.AiDecisionIndexedEligibleCountForDiagnostics, Is.EqualTo(2));
                Assert.That(world.AiDecisionIndexedAvailableCountForDiagnostics, Is.EqualTo(2));
                Assert.That(world.AiDecisionIndexedUnavailableCountForDiagnostics, Is.Zero);
                Assert.That(world.AiDecisionIndexedComparedCountForDiagnostics, Is.EqualTo(2));
                Assert.That(world.AiDecisionIndexedMismatchCountForDiagnostics, Is.Zero);
                Assert.That(world.AiDecisionIndexedFullRowVisitCountForDiagnostics,
                    Is.GreaterThan(0));
                Assert.That(world.AiDecisionIndexedRowVisitCountForDiagnostics,
                    Is.GreaterThan(0));
                Assert.That(
                    world.AiDecisionIndexedFullRowVisitCountForDiagnostics,
                    Is.GreaterThan(world.AiDecisionIndexedRowVisitCountForDiagnostics),
                    "empty SpecialSlots must avoid the FullScan 20..capacity walk");
                Assert.That(world.AiDecisionShadowLastExpectedForDiagnostics.InitialSelectedSlot,
                    Is.EqualTo(2),
                    "the high slot must see the low slot's post-legacy state 14 row");
            }
            finally
            {
                sharedEmptyFrame.state = originalEmptyFrameState;
            }

            Assert.That(sharedEmptyFrame.state, Is.EqualTo(originalEmptyFrameState));
        }

        [TestCase(0, AiDecisionAvailability.EpochMismatch)]
        [TestCase(1, AiDecisionAvailability.GenerationMismatch)]
        [TestCase(2, AiDecisionAvailability.StableIdMismatch)]
        [TestCase(3, AiDecisionAvailability.SelfNotIncluded)]
        public void SharedShadow_PreflightIdentityFailureFailsOpenWholePass(
            int mutationKind,
            AiDecisionAvailability expectedReason)
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.Unk3FC = 400;
            self.Runtime.Unk400 = 0;
            world.Rng.Seed(0x1234u);
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            world.SetAiDecisionSharedPreflightMutationForSelfCheck(mutationKind, 0);
            world.ResetAiDecisionShadowDiagnostics();
            ulong rngCallsBefore = world.Rng.CallCount;

            world.CharacterInputAll(2);

            Assert.That(world.Rng.CallCount, Is.GreaterThan(rngCallsBefore),
                "preflight failure must not suppress authoritative legacy AI");
            Assert.That(world.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionSharedPassAvailableForTests, Is.False);
            Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowComparedCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowUnavailableCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionShadowFirstUnavailableReasonForDiagnostics,
                Is.EqualTo(expectedReason));
        }

        [TestCase(AiDecisionShadowExceptionStage.SharedBuild)]
        [TestCase(AiDecisionShadowExceptionStage.SharedPreflight)]
        [TestCase(AiDecisionShadowExceptionStage.KernelEvaluate)]
        [TestCase(AiDecisionShadowExceptionStage.SharedRefresh)]
        public void SharedShadow_InjectedExceptionRecordsStageAndLegacyContinues(
            AiDecisionShadowExceptionStage stage)
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.Unk3FC = 400;
            self.Runtime.Unk400 = 0;
            world.Runtime.Flow.AiDifficulty = 2;
            world.Runtime.Flow.AiRand3 = 6;
            world.Runtime.Flow.AiRand5 = 10;
            world.Runtime.Flow.AiRand15 = 30;
            world.Runtime.Flow.AiRand20 = 40;
            world.Rng.Seed(0x1234u);
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            world.ResetAiDecisionShadowDiagnostics();
            world.SetAiDecisionShadowExceptionStageForSelfCheck(stage);
            ulong rngCallsBefore = world.Rng.CallCount;

            world.CharacterInputAll(2);

            Assert.That(world.Rng.CallCount, Is.GreaterThan(rngCallsBefore),
                "shadow exceptions must not suppress authoritative legacy AI");
            Assert.That(
                world.AiDecisionShadowFirstExceptionStageForDiagnostics,
                Is.EqualTo(stage));
            Assert.That(world.AiDecisionShadowFirstExceptionTypeForDiagnostics, Is.Not.Null);
            Assert.That(
                world.AiDecisionShadowFirstExceptionTypeForDiagnostics.Name,
                Is.EqualTo("AiDecisionShadowSelfCheckException"));
        }

        [Test]
        public void SharedShadow_FirstExceptionDiagnosticCannotBeOverwritten()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.Unk3FC = 400;
            self.Runtime.Unk400 = 0;
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            world.ResetAiDecisionShadowDiagnostics();
            world.SetAiDecisionShadowExceptionStageForSelfCheck(
                AiDecisionShadowExceptionStage.KernelEvaluate);
            world.CharacterInputAll(2);
            Type firstType = world.AiDecisionShadowFirstExceptionTypeForDiagnostics;

            world.SetAiDecisionShadowExceptionStageForSelfCheck(
                AiDecisionShadowExceptionStage.SharedBuild);
            world.CharacterInputAll(3);

            Assert.That(
                world.AiDecisionShadowFirstExceptionStageForDiagnostics,
                Is.EqualTo(AiDecisionShadowExceptionStage.KernelEvaluate));
            Assert.That(world.AiDecisionShadowFirstExceptionTypeForDiagnostics,
                Is.SameAs(firstType));
        }

        [Test]
        public void CoordinatePriorFlow_ShadowMatchesLegacyRngOrderAndOutput()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.Unk3FC = 400;
            self.Runtime.Unk400 = 0;
            world.Runtime.Flow.AiDifficulty = 2;
            world.Runtime.Flow.AiRand3 = 6;
            world.Runtime.Flow.AiRand5 = 10;
            world.Runtime.Flow.AiRand15 = 30;
            world.Runtime.Flow.AiRand20 = 40;
            world.Runtime.Flow.AiMoveMode = 1;
            world.Runtime.Flow.AiStageTargetX = 777;
            world.Rng.Seed(0x1234u);
            world.AiDecisionShadowMode = AiDecisionShadowMode.Shadow;
            world.ResetAiDecisionShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiDecisionShadowEligibleCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionShadowComparedCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowFirstMismatchReasonForDiagnostics,
                Is.EqualTo(AiDecisionShadowMismatchReason.None));
            Assert.That(world.AiDecisionShadowCloneRngCallCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionShadowBeginInvocationCountForTests, Is.EqualTo(1));
            Assert.That(world.AiDecisionShadowCompleteInvocationCountForTests, Is.EqualTo(1));
            Assert.That(world.AiDecisionShadowComparisonActiveForTests, Is.False);
            Assert.That(world.AiDecisionLegacyRngRecordingForTests, Is.False);
            Assert.That(world.AiDecisionLegacyRngCountForTests, Is.EqualTo(1));
            Assert.That(world.Runtime.Flow.AiRand3, Is.EqualTo(6));
            Assert.That(world.Runtime.Flow.AiStageTargetX, Is.EqualTo(777));
        }

        [Test]
        public void SequentialLowThenHighDecision_ShadowComparesBothEntries()
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            LF2Character low = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, true);
            LF2Character high = RegisterCharacter(world, 1, 2, 2, 90, 0, 0, 9, true);
            RegisterCharacter(world, 2, 3, 2, 180, 0, 0, 0, false);
            low.Runtime.Unk360 = 1;
            high.Runtime.Unk360 = 0;
            world.Rng.Seed(0xB17Au);
            world.AiDecisionShadowMode = AiDecisionShadowMode.Shadow;
            world.ResetAiDecisionShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiDecisionShadowEligibleCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionShadowComparedCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowRowVisitCountForDiagnostics, Is.GreaterThan(0));
        }

        [Test]
        public void InvalidInputHistory_IsUnavailableAndLegacyStillRuns()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 0, true);
            self.Runtime.InputHistory = null;
            self.Runtime.Unk3FC = 400;
            self.Runtime.Unk400 = 0;
            world.AiDecisionShadowMode = AiDecisionShadowMode.Shadow;
            world.ResetAiDecisionShadowDiagnostics();

            world.CharacterInputAll(2);

            Assert.That(world.AiDecisionShadowEligibleCountForDiagnostics, Is.EqualTo(1),
                "eligible count");
            Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.Zero,
                "available count");
            Assert.That(world.AiDecisionShadowUnavailableCountForDiagnostics, Is.EqualTo(1),
                "unavailable count");
            Assert.That(world.AiDecisionShadowComparedCountForDiagnostics, Is.Zero,
                "compared count");
            Assert.That(self.Runtime.InputHistory, Has.Length.EqualTo(6),
                "the unavailable shadow must not prevent the authoritative legacy path");
        }

        [Test]
        public void WarmedShadow_128TicksAllocatesZeroBytes()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.Unk3FC = 10000;
            self.Runtime.Unk400 = 0;
            world.Runtime.Flow.AiRand3 = 6;
            world.AiDecisionShadowMode = AiDecisionShadowMode.Shadow;
            for (int tick = 2; tick < 34; tick++)
                world.CharacterInputAll(tick);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 34; tick < 162; tick++)
                world.CharacterInputAll(tick);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void WarmedSharedShadow_128TicksAllocatesZeroBytes()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(world, 0, 7, 1, 0, 0, 0, 2, true);
            self.Runtime.Unk3FC = 10000;
            self.Runtime.Unk400 = 0;
            world.Runtime.Flow.AiRand3 = 6;
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            for (int tick = 2; tick < 34; tick++)
                world.CharacterInputAll(tick);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 34; tick < 162; tick++)
                world.CharacterInputAll(tick);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(world.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(160));
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedEligibleCountForDiagnostics, Is.EqualTo(160));
            Assert.That(world.AiDecisionIndexedComparedCountForDiagnostics, Is.EqualTo(160));
            Assert.That(world.AiDecisionIndexedMismatchCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void UnifiedAuthority_DefaultAndSwitchMatrixEnforcesConfigurationAndTickBoundary()
        {
            var world = new SimulationWorld();
            Assert.That(world.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.LegacySeparate));
            Assert.Throws<InvalidOperationException>(() =>
                world.AiUnifiedSnapshotExecutionMode =
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority);

            Invoke(world, "SetAiSoACandidateModeForSelfCheck", true);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiUnifiedSnapshotExecutionMode =
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority;
            Assert.Throws<InvalidOperationException>(() =>
                world.AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Shadow);
            Assert.Throws<InvalidOperationException>(() =>
                world.AiDecisionExecutionMode = AiDecisionExecutionMode.Legacy);

            Invoke(world, "BeginDeferredEntityMutationPass");
            try
            {
                Assert.Throws<InvalidOperationException>(() =>
                    world.AiUnifiedSnapshotExecutionMode =
                        AiUnifiedSnapshotExecutionMode.LegacySeparate);
            }
            finally
            {
                Invoke(world, "EndDeferredEntityMutationPass");
            }

            world.AiUnifiedSnapshotExecutionMode =
                AiUnifiedSnapshotExecutionMode.LegacySeparate;
            Invoke(world, "SetAiSoACandidateModeForSelfCheck", false);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.Legacy;
            Assert.That(world.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.LegacySeparate));
        }

        [Test]
        public void UnifiedAuthority_NonContiguousSlotsMatchLegacySeparateInputRngAndWorldState()
        {
            var legacy = new SimulationWorld();
            var unified = new SimulationWorld();
            int[] slots = { 0, 3, 7 };
            var legacyCharacters = new LF2Character[slots.Length];
            var unifiedCharacters = new LF2Character[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                int slot = slots[index];
                int state = index == 1 ? 9 : 2;
                legacyCharacters[index] = RegisterCharacter(
                    legacy, slot, index + 1, index & 1, slot * 40, 0, 0, state, true);
                unifiedCharacters[index] = RegisterCharacter(
                    unified, slot, index + 1, index & 1, slot * 40, 0, 0, state, true);
            }
            ConfigureGateBWorld(legacy, unifiedAuthority: false, 0xB771u);
            ConfigureGateBWorld(unified, unifiedAuthority: true, 0xB771u);
            unified.ResetAiUnifiedSnapshotExecutionDiagnostics();
            unified.ResetAiSoACandidateDiagnostics();
            unified.ResetAiDecisionShadowDiagnostics();

            legacy.CharacterInputAll(2);
            unified.CharacterInputAll(2);

            for (int index = 0; index < slots.Length; index++)
            {
                AssertDecisionStateEqual(
                    legacy,
                    legacyCharacters[index],
                    unified,
                    unifiedCharacters[index]);
                AssertEntityObservableStateEqual(
                    legacyCharacters[index],
                    unifiedCharacters[index]);
            }
            Assert.That(unified.AiUnifiedSnapshotExecutionBuildCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(unified.AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                unified.AiUnifiedSnapshotExecutionCanonicalInitialCaptureCountForDiagnostics,
                Is.EqualTo(slots.Length));
            Assert.That(unified.AiUnifiedSnapshotExecutionReadCountForDiagnostics,
                Is.EqualTo(slots.Length));
            Assert.That(unified.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics,
                Is.EqualTo(slots.Length));
            Assert.That(unified.AiSoACandidateFusedSnapshotBuildCountForDiagnostics,
                Is.Zero);
            Assert.That(unified.AiDecisionSharedBuildCountForDiagnostics, Is.Zero);
            Assert.That(unified.AiSoACandidateSnapshotRefreshCountForDiagnostics,
                Is.Zero);
            Assert.That(unified.AiDecisionSharedRefreshCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void UnifiedAuthority_IncrementalPostInputRefreshMatchesFullRefreshAcrossTicks()
        {
            var incremental = new SimulationWorld
            {
                ValidateIncrementalAiUnifiedRowForDiagnostics = true,
            };
            var full = new SimulationWorld
            {
                ForceFullCharacterInputPostRefreshForDiagnostics = true,
            };
            int[] slots = { 0, 3, 7, 20 };
            var incrementalCharacters = new LF2Character[slots.Length];
            var fullCharacters = new LF2Character[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                int state = index == 1 ? 9 : index == 3 ? 14 : 2;
                incrementalCharacters[index] = RegisterCharacter(
                    incremental,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 60,
                    index == 3 ? 12 : 0,
                    0,
                    state,
                    true);
                fullCharacters[index] = RegisterCharacter(
                    full,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 60,
                    index == 3 ? 12 : 0,
                    0,
                    state,
                    true);
            }
            ConfigureGateBWorld(incremental, unifiedAuthority: true, 0x91A7u);
            ConfigureGateBWorld(full, unifiedAuthority: true, 0x91A7u);

            for (int tick = 2; tick < 32; tick++)
            {
                incremental.CharacterInputAll(tick);
                full.CharacterInputAll(tick);

                for (int index = 0; index < slots.Length; index++)
                {
                    AssertDecisionStateEqual(
                        full,
                        fullCharacters[index],
                        incremental,
                        incrementalCharacters[index]);
                    AssertEntityObservableStateEqual(
                        fullCharacters[index],
                        incrementalCharacters[index]);
                }

                Assert.That(
                    incremental.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                    Is.True,
                    $"incremental published snapshot must remain internally valid at tick {tick}");
                Assert.That(
                    full.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                    Is.True,
                    $"full published snapshot must remain internally valid at tick {tick}");
            }

            Assert.That(
                incremental.AiUnifiedSnapshotExecutionIncrementalValidationCountForDiagnostics,
                Is.EqualTo(30L * slots.Length));
        }

        [Test]
        public void UnifiedAuthority_NoPendingRefreshSkipMatchesLegacyRefreshAcrossTicks()
        {
            var baseline = new SimulationWorld();
            var skipped = new SimulationWorld
            {
                AiUnifiedSnapshotNoPendingRefreshSkipForDiagnostics = true,
            };
            int[] slots = { 20, 23, 27 };
            var baselineCharacters = new LF2Character[slots.Length];
            var skippedCharacters = new LF2Character[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                baselineCharacters[index] = RegisterCharacter(
                    baseline,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 60,
                    0,
                    0,
                    2,
                    false);
                skippedCharacters[index] = RegisterCharacter(
                    skipped,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 60,
                    0,
                    0,
                    2,
                    false);
            }
            ConfigureGateBWorld(baseline, unifiedAuthority: true, 0x5A1Fu);
            ConfigureGateBWorld(skipped, unifiedAuthority: true, 0x5A1Fu);

            for (int tick = 2; tick < 18; tick++)
            {
                baseline.CharacterInputAll(tick);
                skipped.CharacterInputAll(tick);
                for (int index = 0; index < slots.Length; index++)
                {
                    AssertDecisionStateEqual(
                        baseline,
                        baselineCharacters[index],
                        skipped,
                        skippedCharacters[index]);
                    AssertEntityObservableStateEqual(
                        baselineCharacters[index],
                        skippedCharacters[index]);
                }

                Assert.That(
                    skipped.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                    Is.True,
                    $"no-pending skip must preserve the published snapshot at tick {tick}");
            }

            Assert.That(
                skipped.AiUnifiedSnapshotExecutionNoPendingRefreshSkipCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(
                skipped.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics,
                Is.EqualTo(baseline.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics));
        }

        [Test]
        public void UnifiedAuthority_RollingSnapshotMatchesForcedFullRebuildAcrossTicks()
        {
            var rolling = new SimulationWorld
            {
                ValidateIncrementalAiUnifiedRowForDiagnostics = true,
            };
            var full = new SimulationWorld
            {
                ForceFullAiUnifiedSnapshotRebuildForDiagnostics = true,
            };
            int[] slots = { 0, 3, 7, 20 };
            var rollingCharacters = new LF2Character[slots.Length];
            var fullCharacters = new LF2Character[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                int state = index == 1 ? 9 : index == 3 ? 14 : 2;
                rollingCharacters[index] = RegisterCharacter(
                    rolling,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 60,
                    index == 3 ? 12 : 0,
                    0,
                    state,
                    true);
                fullCharacters[index] = RegisterCharacter(
                    full,
                    slots[index],
                    index + 1,
                    index & 1,
                    index * 60,
                    index == 3 ? 12 : 0,
                    0,
                    state,
                    true);
            }
            ConfigureGateBWorld(rolling, unifiedAuthority: true, 0xC011u);
            ConfigureGateBWorld(full, unifiedAuthority: true, 0xC011u);

            for (int tick = 2; tick < 32; tick++)
            {
                rolling.CharacterInputAll(tick);
                full.CharacterInputAll(tick);
                for (int index = 0; index < slots.Length; index++)
                {
                    AssertDecisionStateEqual(
                        full,
                        fullCharacters[index],
                        rolling,
                        rollingCharacters[index]);
                    AssertEntityObservableStateEqual(
                        fullCharacters[index],
                        rollingCharacters[index]);
                }

                int directZ = tick * 3;
                rollingCharacters[1].Runtime.ZInt = directZ;
                fullCharacters[1].Runtime.ZInt = directZ;
            }

            Assert.That(
                rolling.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics,
                Is.EqualTo(29));
            Assert.That(
                full.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics,
                Is.Zero);
            Assert.That(
                rolling.AiUnifiedSnapshotExecutionCanonicalInitialCaptureCountForDiagnostics,
                Is.EqualTo(slots.Length));
            Assert.That(
                full.AiUnifiedSnapshotExecutionCanonicalInitialCaptureCountForDiagnostics,
                Is.EqualTo(30L * slots.Length));
        }

        [Test]
        public void UnifiedAuthority_OccupancyEpochChangeForcesCompleteRebuild()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 1, 10, 0, 0, 2, true);
            LF2Character replaced =
                RegisterCharacter(world, 3, 2, 2, 90, 0, 0, 9, true);
            ConfigureGateBWorld(world, unifiedAuthority: true, 0xE09Cu);

            world.CharacterInputAll(2);
            world.Unregister(replaced);
            LF2Character replacement =
                RegisterCharacter(world, 3, 3, 2, 120, 0, 0, 9, true);
            Assert.DoesNotThrow(() => replacement.Runtime.LinkState = 1);
            world.CharacterInputAll(3);

            Assert.That(
                world.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.AiUnifiedSnapshotExecutionCanonicalInitialCaptureCountForDiagnostics,
                Is.EqualTo(4));
            Assert.That(
                world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                Is.True);
        }

        [Test]
        public void UnifiedAuthority_AscendingRefreshMakesLowVisibleToHighWithoutReverseEarlyVisibility()
        {
            var world = new SimulationWorld();
            LF2Character low = RegisterCharacter(world, 0, 1, 1, 10, 0, 0, 2, true);
            LF2Character high = RegisterCharacter(world, 3, 2, 2, 90, 0, 0, 9, true);
            RegisterCharacter(world, 7, 3, 1, 180, 0, 0, 2, false);
            ConfigureGateBWorld(world, unifiedAuthority: true, 0xB17Au);
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            world.SetCharacterInputPassMutationOverrideForSelfCheck((_, entity) =>
            {
                if (entity?.Runtime?.SlotIndex == 0 && entity.Frame?.D != null)
                    entity.Frame.D.state = 14;
            });
            world.SetAiUnifiedSnapshotExecutionVisibilityProbeForSelfCheck(0, 3, 3, 0);

            LF2FrameData sharedEmptyFrame = GetSharedEmptyFrameForIsolation();
            int originalEmptyFrameState = sharedEmptyFrame.state;
            try
            {
                world.CharacterInputAll(2);

                Assert.That(low.Frame.D, Is.SameAs(sharedEmptyFrame),
                    "the unified refresh fixture must own its shared missing-frame mutation");
                Assert.That(sharedEmptyFrame.state, Is.EqualTo(14));
                Assert.That(world.AiUnifiedSnapshotExecutionProbeStateAForTests,
                    Is.EqualTo(9),
                    "the low slot must observe the high slot before the later high-slot input");
                Assert.That(world.AiUnifiedSnapshotExecutionProbeStateBForTests,
                    Is.EqualTo(14),
                    "the high slot must observe the low slot's post-input unified row refresh");
                Assert.That(low.GetState(), Is.EqualTo(14));
                Assert.That(high.GetState(), Is.EqualTo(9));
            }
            finally
            {
                world.SetCharacterInputPassMutationOverrideForSelfCheck(null);
                sharedEmptyFrame.state = originalEmptyFrameState;
            }

            Assert.That(sharedEmptyFrame.state, Is.EqualTo(originalEmptyFrameState));
        }

        [TestCase(AiUnifiedSnapshotExceptionStage.Prepare)]
        [TestCase(AiUnifiedSnapshotExceptionStage.Capture)]
        [TestCase(AiUnifiedSnapshotExceptionStage.BuildIndexes)]
        [TestCase(AiUnifiedSnapshotExceptionStage.Validate)]
        public void UnifiedAuthority_PreCommitFailureUsesOneCompleteLegacySeparatePass(
            AiUnifiedSnapshotExceptionStage stage)
        {
            var expected = new SimulationWorld();
            var actual = new SimulationWorld();
            int[] slots = { 0, 3, 7 };
            var expectedCharacters = new LF2Character[slots.Length];
            var actualCharacters = new LF2Character[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                expectedCharacters[index] = RegisterCharacter(
                    expected, slots[index], index + 1, index & 1, index * 80, 0, 0, 2, true);
                actualCharacters[index] = RegisterCharacter(
                    actual, slots[index], index + 1, index & 1, index * 80, 0, 0, 2, true);
            }
            ConfigureGateBWorld(expected, unifiedAuthority: false, 0xFA11u);
            ConfigureGateBWorld(actual, unifiedAuthority: true, 0xFA11u);
            actual.ResetAiUnifiedSnapshotExecutionDiagnostics();
            actual.ResetAiSoACandidateDiagnostics();
            actual.ResetAiDecisionShadowDiagnostics();
            actual.SetAiUnifiedSnapshotExecutionFailureForSelfCheck(stage);

            expected.CharacterInputAll(2);
            Assert.DoesNotThrow(() => actual.CharacterInputAll(2));

            for (int index = 0; index < slots.Length; index++)
            {
                AssertDecisionStateEqual(
                    expected,
                    expectedCharacters[index],
                    actual,
                    actualCharacters[index]);
                AssertEntityObservableStateEqual(
                    expectedCharacters[index],
                    actualCharacters[index]);
            }
            Assert.That(actual.AiUnifiedSnapshotExecutionReadCountForDiagnostics,
                Is.Zero);
            Assert.That(actual.AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics,
                Is.Zero);
            Assert.That(actual.AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(actual.AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(actual.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics,
                Is.Zero);
            Assert.That(actual.AiSoACandidateFusedSnapshotBuildCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(actual.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(actual.AiSoACandidateSnapshotRefreshCountForDiagnostics,
                Is.EqualTo(slots.Length));
            Assert.That(actual.AiDecisionSharedRefreshCountForDiagnostics,
                Is.EqualTo(slots.Length));
        }

        [TestCase(AiUnifiedSnapshotExceptionStage.InitialSensingCompare, 0)]
        [TestCase(AiUnifiedSnapshotExceptionStage.InitialDecisionCompare, 0)]
        [TestCase(AiUnifiedSnapshotExceptionStage.Refresh, 1)]
        [TestCase(AiUnifiedSnapshotExceptionStage.RefreshCapture, 1)]
        [TestCase(AiUnifiedSnapshotExceptionStage.RefreshBuildIndexes, 1)]
        public void UnifiedAuthority_PostPublicationFailureIsHardBreachWithoutMixedFallback(
            AiUnifiedSnapshotExceptionStage stage,
            int expectedReads)
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 2, true);
            RegisterCharacter(world, 3, 2, 2, 90, 0, 0, 9, true);
            ConfigureGateBWorld(world, unifiedAuthority: true, 0xC0DEu);
            world.ResetAiUnifiedSnapshotExecutionDiagnostics();
            world.ResetAiSoACandidateDiagnostics();
            world.ResetAiDecisionShadowDiagnostics();
            world.SetAiUnifiedSnapshotExecutionFailureForSelfCheck(stage);

            Assert.Throws<InvalidOperationException>(() => world.CharacterInputAll(2));

            Assert.That(world.AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(world.AiUnifiedSnapshotExecutionReadCountForDiagnostics,
                Is.EqualTo(expectedReads));
            Assert.That(world.AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics,
                Is.Zero);
            Assert.That(world.AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics,
                Is.Zero);
            Assert.That(world.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(world.AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics,
                Is.EqualTo(stage));
            Assert.That(world.AiSoACandidateFusedSnapshotBuildCountForDiagnostics,
                Is.Zero);
            Assert.That(world.AiDecisionSharedBuildCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidateSnapshotRefreshCountForDiagnostics,
                Is.Zero);
            Assert.That(world.AiDecisionSharedRefreshCountForDiagnostics, Is.Zero);
            Assert.That(world.AiUnifiedSnapshotShadowRefreshCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void UnifiedAuthority_PublishedStateCoversIdentityIndexesBoundariesFallbackAndFirst10()
        {
            var world = new SimulationWorld();
            LF2Character first = RegisterCharacter(world, 0, 1, 1, 10, 0, 0, 2, false);
            LF2Character role = RegisterCharacter(world, 3, 2, 2, 90, 0, 0, 9, false);
            LF2Character special = RegisterCharacter(world, 20, 0xC8, 1, 180, 0, 0, 1000, false);
            first.Runtime.XBoundPositive = true;
            world.BoundaryWriter.SyncConsumedFlags(first.Runtime);
            ConfigureGateBWorld(world, unifiedAuthority: true, 0x5150u);
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                3, role, out RuntimeEntityHandle roleHandle), Is.True);

            world.CharacterInputAll(2);

            Assert.That(world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                Is.True);
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedCapacityForTests,
                Is.EqualTo(world.RuntimeSlotCapacityForDiagnostics));
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedEpochForTests,
                Is.GreaterThan(0));
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedEpochIsCurrentForTests,
                Is.True);
            Assert.That(world.GetAiUnifiedSnapshotExecutionPublishedGenerationForSelfCheck(3),
                Is.EqualTo(unchecked((int)roleHandle.Generation)));
            Assert.That(world.GetAiUnifiedSnapshotExecutionPublishedStableIdForSelfCheck(3),
                Is.EqualTo(role.Runtime.StableId));
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedSpecialSlotCountForTests,
                Is.EqualTo(1));
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedGroundRoleCountForTests,
                Is.GreaterThan(0));
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedTeamSummaryCountForTests,
                Is.GreaterThan(0));
            Assert.That(world.GetAiUnifiedSnapshotExecutionPublishedSensingBoundaryForSelfCheck(0),
                Is.EqualTo(1 << 3));
            Assert.That(world.GetAiUnifiedSnapshotExecutionPublishedDecisionBoundaryForSelfCheck(0),
                Is.EqualTo(1));
            Assert.That(world.IsAiUnifiedSnapshotExecutionPublishedFallbackForSelfCheck(0, first),
                Is.True);
            Assert.That(world.IsAiUnifiedSnapshotExecutionPublishedFallbackForSelfCheck(20, special),
                Is.True);
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedFirst10ValidForTests,
                Is.True);
            Assert.That(world.IsAiUnifiedSnapshotExecutionPublishedFirst10PresentForSelfCheck(0),
                Is.True);
        }

        [Test]
        public void UnifiedAuthority_Oid5152MergeInvalidatesMembershipBeforeNextRollForward()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers =
                BuildOid5152LifecycleWrappers();
            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            var world = new SimulationWorld(resolver);
            LF2Character self = RegisterCharacter(
                world, 0, 7, 5152, 520, 0, 572, 2, false);
            LF2Character partner = RegisterCharacter(
                world, 10, 8, 5152, 540, 0, 576, 2, false);
            self.Health.HP = 80;
            self.Health.HPBound = 100;
            self.Health.HP3 = 250;
            partner.Health.HP = 70;
            partner.Health.HPBound = 90;
            partner.Health.HP3 = 250;
            ConfigureGateBWorld(world, unifiedAuthority: true, 0x5152u);

            world.CharacterInputAll(2);
            Assert.That(
                world.AiUnifiedSnapshotExecutionBuildCountForDiagnostics,
                Is.EqualTo(1));

            self.ImmediateFrame(0);
            partner.ImmediateFrame(0);
            self.RelationTeam = 5152;
            partner.RelationTeam = 5152;
            self.Health.HP = 80;
            self.Health.HPBound = 100;
            partner.Health.HP = 70;
            partner.Health.HPBound = 90;
            self.Runtime.Unk338 = 0;
            partner.Runtime.Unk338 = 0;
            self.Runtime.SetPosition(520, 0, 572);
            partner.Runtime.SetPosition(540, 0, 576);
            self.Runtime.SyncIntegerPosition();
            partner.Runtime.SyncIntegerPosition();

            world.Oid5152RuntimeMaintenanceAll(2);
            Assert.That(self.ObjectId, Is.EqualTo(51));
            Assert.That(partner.Runtime.OidMergeDormant, Is.True);

            world.CharacterInputAll(3);

            Assert.That(
                world.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics,
                Is.Zero,
                "Dormant row-membership changes must not roll the old Included set forward.");
            Assert.That(
                world.AiUnifiedSnapshotExecutionBuildCountForDiagnostics,
                Is.EqualTo(2),
                "The first post-merge input pass must rebuild the unified snapshot.");
            Assert.That(
                world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                Is.True);
        }

        [Test]
        public void UnifiedAuthority_Oid5152SplitReactivatesOriginalGenerationWithoutStaleRow()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers =
                BuildOid5152LifecycleWrappers();
            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            var world = new SimulationWorld(resolver);
            LF2Character self = RegisterCharacter(
                world, 0, 7, 5152, 530, 0, 575, 2, false);
            LF2Character partner = RegisterCharacter(
                world, 10, 8, 5152, 540, 0, 576, 2, false);
            Assert.That(self.TryApplyRuntimeIdentity(51, 290, true, out _), Is.True);
            self.Runtime.Unk328 = 1;
            self.Runtime.Unk32C = 10;
            self.Runtime.Unk330 = 7;
            self.Runtime.Unk334 = 8;
            self.Runtime.Unk338 = 0;
            self.Health.HP = 150;
            self.Health.HPBound = 190;
            self.Health.HP3 = 250;
            self.Health.PP = 500;
            partner.Runtime.OidMergeDormant = true;
            ConfigureGateBWorld(world, unifiedAuthority: true, 0x5153u);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(0, self, out RuntimeEntityHandle selfHandle),
                Is.True);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(10, partner, out RuntimeEntityHandle partnerHandle),
                Is.True);

            world.CharacterInputAll(2);
            Assert.DoesNotThrow(() => world.Oid5152RuntimeMaintenanceAll(2));

            Assert.That(self.ObjectId, Is.EqualTo(7));
            Assert.That(partner.ObjectId, Is.EqualTo(8));
            Assert.That(partner.Runtime.OidMergeDormant, Is.False);
            Assert.That(
                world.TryResolveRuntimeHandleForDiagnostics(selfHandle, out LF2Entity resolvedSelf) &&
                ReferenceEquals(resolvedSelf, self),
                Is.True);
            Assert.That(
                world.TryResolveRuntimeHandleForDiagnostics(partnerHandle, out LF2Entity resolvedPartner) &&
                ReferenceEquals(resolvedPartner, partner),
                Is.True);

            world.CharacterInputAll(3);

            Assert.That(
                world.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.AiUnifiedSnapshotExecutionBuildCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                Is.True);
        }

        [Test]
        public void AiSensingHitJ_FollowsCurrentFrameAcrossInitialCaptureAndRefresh()
        {
            SimulationWorld world = CreateUnifiedCandidateWorld();
            LF2Character character = RegisterCharacter(
                world,
                0,
                11,
                1,
                10,
                0,
                0,
                2,
                false,
                290,
                77);

            PrepareUnifiedManualPass(world);
            Assert.That((int)Invoke(world, "HitJ", character), Is.EqualTo(290));

            character.ImmediateFrame(1);
            RefreshUnifiedManualPass(world, character);
            Assert.That((int)Invoke(world, "HitJ", character), Is.EqualTo(77));
            EndUnifiedManualPass(world);
        }

        [Test]
        public void UnifiedAuthority_PublishesCurrentFrameHitJ()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(
                world,
                0,
                5011,
                1,
                10,
                0,
                0,
                2,
                false,
                290);
            ConfigureGateBWorld(world, unifiedAuthority: true, 0x11A1u);

            Assert.That(
                character.GetFrameDataById(character.Runtime.Frame)?.hit_j ?? 0,
                Is.EqualTo(290),
                "the fixture must expose hit_j through the current logical frame before the pass");

            world.CharacterInputAll(2);

            int currentHitJ =
                character.GetFrameDataById(character.Runtime.Frame)?.hit_j ?? 0;
            Assert.That(
                world.GetAiUnifiedSnapshotExecutionPublishedHitJForSelfCheck(0),
                Is.EqualTo(currentHitJ),
                "the published row must follow the post-input current logical frame DAT");
            Assert.That(
                world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                Is.True);
        }

        [Test]
        public void AiSensingSnapshot_GrowCopiesHitJ()
        {
            var snapshot = new GrowableAiSensingSnapshot(1);
            snapshot.HitJ[0] = 290;

            GrowableAiSensingSnapshot grown = snapshot.Grow(4);

            Assert.That(grown.Capacity, Is.EqualTo(4));
            Assert.That(grown.HitJ[0], Is.EqualTo(290));
            Assert.That(grown.HitJ[3], Is.Zero);
        }

        private static void ConfigureGateBWorld(
            SimulationWorld world,
            bool unifiedAuthority,
            uint seed)
        {
            Invoke(world, "SetAiSoACandidateModeForSelfCheck", true);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Disabled;
            world.AiUnifiedSnapshotExecutionMode = unifiedAuthority
                ? AiUnifiedSnapshotExecutionMode.UnifiedAuthority
                : AiUnifiedSnapshotExecutionMode.LegacySeparate;
            world.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(seed);
        }

        private static Dictionary<int, LF2CharacterDataWrapper>
            BuildOid5152LifecycleWrappers()
        {
            return new Dictionary<int, LF2CharacterDataWrapper>
            {
                [7] = new LF2CharacterDataWrapper(
                    7,
                    BuildOid5152LifecycleData("Focused_Oid7", 7, false)),
                [8] = new LF2CharacterDataWrapper(
                    8,
                    BuildOid5152LifecycleData("Focused_Oid8", 8, false)),
                [51] = new LF2CharacterDataWrapper(
                    51,
                    BuildOid5152LifecycleData("Focused_Oid51", 51, true)),
            };
        }

        private static LF2CharacterData BuildOid5152LifecycleData(
            string name,
            int oid,
            bool merged)
        {
            var frames = new List<LF2FrameData>
            {
                new LF2FrameData
                {
                    frameId = 0,
                    frameName = name + "_root",
                    state = 2,
                    wait = 100,
                    next = 0,
                    centerx = 39,
                    centery = 79,
                },
                new LF2FrameData
                {
                    frameId = 112,
                    frameName = name + "_split",
                    state = 0,
                    wait = 1,
                    next = 112,
                    centerx = 39,
                    centery = 79,
                },
            };
            if (merged)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = 290,
                    frameName = name + "_merged",
                    state = 15,
                    wait = 2,
                    next = 999,
                    centerx = 39,
                    centery = 79,
                    hit_ja = 0,
                });
            }

            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.Character,
                frames = frames,
            };
        }

        private static void AssertEntityObservableStateEqual(
            LF2Character expected,
            LF2Character actual)
        {
            Assert.That(actual.Runtime.SlotIndex, Is.EqualTo(expected.Runtime.SlotIndex));
            Assert.That(actual.ObjectId, Is.EqualTo(expected.ObjectId));
            Assert.That(actual.Runtime.HP, Is.EqualTo(expected.Runtime.HP));
            Assert.That(actual.Runtime.HP3, Is.EqualTo(expected.Runtime.HP3));
            Assert.That(actual.Runtime.PP, Is.EqualTo(expected.Runtime.PP));
            Assert.That(actual.Runtime.XInt, Is.EqualTo(expected.Runtime.XInt));
            Assert.That(actual.Runtime.YInt, Is.EqualTo(expected.Runtime.YInt));
            Assert.That(actual.Runtime.ZInt, Is.EqualTo(expected.Runtime.ZInt));
            Assert.That(actual.Frame.N, Is.EqualTo(expected.Frame.N));
            Assert.That(actual.GetState(), Is.EqualTo(expected.GetState()));
            Assert.That(actual.Runtime.LinkState, Is.EqualTo(expected.Runtime.LinkState));
            Assert.That(actual.Runtime.TargetSlotIndex,
                Is.EqualTo(expected.Runtime.TargetSlotIndex));
        }

        private static SimulationWorld CreateUnifiedCandidateWorld()
        {
            var world = new SimulationWorld();
            Invoke(world, "SetAiSoACandidateModeForSelfCheck", true);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Shadow;
            world.Runtime.Flow.InputPhase = 2;
            return world;
        }

        private static void PrepareUnifiedManualPass(SimulationWorld world)
        {
            Invoke(world, "BuildAiInputSlotSnapshot");
            Invoke(world, "PrepareAiDecisionSharedPass");
            Invoke(world, "CompleteAiUnifiedSnapshotShadowInitialComparison");
        }

        private static void RefreshUnifiedManualPass(
            SimulationWorld world,
            LF2Entity entity)
        {
            Invoke(world, "RefreshAiDecisionSharedRowAfterCharacterInput", entity);
            Invoke(world, "ObserveAiCandidateCharacterInputMutation", entity);
            Invoke(world, "RefreshAiSoASensingShadowRowAfterCharacterInput", entity);
            Invoke(world, "RefreshAiUnifiedSnapshotShadowRowAfterCharacterInput", entity);
        }

        private static void EndUnifiedManualPass(SimulationWorld world)
        {
            Invoke(world, "EndAiDecisionSharedPass");
            Invoke(world, "ClearAiInputSlotSnapshot");
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            bool aiControlled,
            int hitJ = 0,
            int? secondHitJ = null)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
                hit_j = hitJ,
            };
            var frames = new List<LF2FrameData> { frame };
            if (secondHitJ.HasValue)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = 1,
                    state = state,
                    wait = 100,
                    next = 1,
                    centerx = 0,
                    centery = 0,
                    hit_j = secondHitJ.Value,
                });
            }
            var data = new LF2CharacterData
            {
                name = $"AiDecisionShadow_{slot}_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = frames,
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(slot);
            character.Team = team;
            character.RelationTeam = team;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Runtime.PP = 0;
            character.Runtime.KillCount = -1;
            character.Runtime.Unk3FC = -1001;
            character.Runtime.Unk400 = -1001;
            character.Runtime.SetPosition(x, y, z);
            character.Runtime.SyncIntegerPosition();
            character.Controller = new EmptyController();
            character.AiControlled = aiControlled;
            world.Register(character);
            return character;
        }

        private static LF2FrameData GetSharedEmptyFrameForIsolation()
        {
            FieldInfo emptyFrameField = typeof(LF2FrameCache).GetField(
                "EmptyFrame",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(emptyFrameField, Is.Not.Null);
            return (LF2FrameData)emptyFrameField.GetValue(null);
        }

        private static object Invoke(SimulationWorld world, string methodName, params object[] args)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(methodName, InstanceMembers);
            Assert.That(method, Is.Not.Null, $"Missing SimulationWorld.{methodName}.");
            return method.Invoke(world, args);
        }

        private static void AssertDecisionStateEqual(
            SimulationWorld expectedWorld,
            LF2Character expected,
            SimulationWorld actualWorld,
            LF2Character actual)
        {
            CollectionAssert.AreEqual(expected.Runtime.InputHistory, actual.Runtime.InputHistory);
            Assert.That(actual.Runtime.CdAttack, Is.EqualTo(expected.Runtime.CdAttack));
            Assert.That(actual.Runtime.CdJump, Is.EqualTo(expected.Runtime.CdJump));
            Assert.That(actual.Runtime.CdDefend, Is.EqualTo(expected.Runtime.CdDefend));
            Assert.That(actual.Runtime.CdDefendLock, Is.EqualTo(expected.Runtime.CdDefendLock));
            Assert.That(actual.Runtime.CdRight, Is.EqualTo(expected.Runtime.CdRight));
            Assert.That(actual.Runtime.CdLeft, Is.EqualTo(expected.Runtime.CdLeft));
            Assert.That(actual.Runtime.CdUp, Is.EqualTo(expected.Runtime.CdUp));
            Assert.That(actual.Runtime.CdDown, Is.EqualTo(expected.Runtime.CdDown));
            Assert.That(actual.Runtime.ComboDra, Is.EqualTo(expected.Runtime.ComboDra), "ComboDra");
            Assert.That(actual.Runtime.ComboDla, Is.EqualTo(expected.Runtime.ComboDla), "ComboDla");
            Assert.That(actual.Runtime.ComboDua, Is.EqualTo(expected.Runtime.ComboDua), "ComboDua");
            Assert.That(actual.Runtime.ComboDda, Is.EqualTo(expected.Runtime.ComboDda), "ComboDda");
            Assert.That(actual.Runtime.ComboDrj, Is.EqualTo(expected.Runtime.ComboDrj), "ComboDrj");
            Assert.That(actual.Runtime.ComboDlj, Is.EqualTo(expected.Runtime.ComboDlj), "ComboDlj");
            Assert.That(actual.Runtime.ComboDuj, Is.EqualTo(expected.Runtime.ComboDuj), "ComboDuj");
            Assert.That(actual.Runtime.ComboDdj, Is.EqualTo(expected.Runtime.ComboDdj), "ComboDdj");
            Assert.That(actual.Runtime.ComboDja, Is.EqualTo(expected.Runtime.ComboDja), "ComboDja");
            Assert.That(actual.Runtime.KeyUp, Is.EqualTo(expected.Runtime.KeyUp));
            Assert.That(actual.Runtime.KeyDown, Is.EqualTo(expected.Runtime.KeyDown));
            Assert.That(actual.Runtime.KeyLeft, Is.EqualTo(expected.Runtime.KeyLeft));
            Assert.That(actual.Runtime.KeyRight, Is.EqualTo(expected.Runtime.KeyRight));
            Assert.That(actual.Runtime.KeyAttack, Is.EqualTo(expected.Runtime.KeyAttack));
            Assert.That(actual.Runtime.KeyJump, Is.EqualTo(expected.Runtime.KeyJump));
            Assert.That(actual.Runtime.KeyDefend, Is.EqualTo(expected.Runtime.KeyDefend));
            Assert.That(actual.Runtime.Unk360, Is.EqualTo(expected.Runtime.Unk360));
            Assert.That(actual.Runtime.Unk3FC, Is.EqualTo(expected.Runtime.Unk3FC));
            Assert.That(actual.Runtime.Unk400, Is.EqualTo(expected.Runtime.Unk400));
            Assert.That(actualWorld.Runtime.Flow.AiDifficulty,
                Is.EqualTo(expectedWorld.Runtime.Flow.AiDifficulty));
            Assert.That(actualWorld.Runtime.Flow.AiRand3,
                Is.EqualTo(expectedWorld.Runtime.Flow.AiRand3));
            Assert.That(actualWorld.Runtime.Flow.AiRand5,
                Is.EqualTo(expectedWorld.Runtime.Flow.AiRand5));
            Assert.That(actualWorld.Runtime.Flow.AiRand15,
                Is.EqualTo(expectedWorld.Runtime.Flow.AiRand15));
            Assert.That(actualWorld.Runtime.Flow.AiRand20,
                Is.EqualTo(expectedWorld.Runtime.Flow.AiRand20));
            Assert.That(actualWorld.Runtime.Flow.AiMoveMode,
                Is.EqualTo(expectedWorld.Runtime.Flow.AiMoveMode));
            Assert.That(actualWorld.Runtime.Flow.AiStageTargetX,
                Is.EqualTo(expectedWorld.Runtime.Flow.AiStageTargetX));
            Assert.That(actualWorld.Rng.State, Is.EqualTo(expectedWorld.Rng.State));
            Assert.That(actualWorld.Rng.CallCount, Is.EqualTo(expectedWorld.Rng.CallCount));
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }

        private sealed class GrowableAiSensingSnapshot : AiSensingSnapshot
        {
            internal GrowableAiSensingSnapshot(int capacity)
                : base(capacity)
            {
            }

            internal GrowableAiSensingSnapshot Grow(int capacity)
            {
                var grown = new GrowableAiSensingSnapshot(capacity);
                CopyTo(grown);
                return grown;
            }
        }

        public enum UnifiedRefreshMutationKind
        {
            X = 0,
            Team = 1,
            Hp = 2,
            LivingAndRole = 3,
        }
    }
}
#endif
