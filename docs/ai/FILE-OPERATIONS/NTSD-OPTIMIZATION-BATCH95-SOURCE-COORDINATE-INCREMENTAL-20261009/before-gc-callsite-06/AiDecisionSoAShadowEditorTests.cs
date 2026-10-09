#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation;
using NUnit.Framework;
using Unity.Profiling;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

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
            Assert.That(world.AiUnifiedSnapshotShadowRefreshCountForDiagnostics, Is.EqualTo(3),
                "one AI producer publication plus both retained routing publications are expected");
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
                Is.EqualTo(6),
                "three row publications are compared against both shadow consumers");
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
            Assert.That(world.AiDecisionSharedRefreshCountForDiagnostics, Is.EqualTo(2),
                "the AI producer row and retained post-routing row are published separately");
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
        public void IndexedCanonical_CommitsNativeSynchronizedRngWithoutMutatingLegacyRng()
        {
            var indexed = new SimulationWorld();
            LF2Character indexedSelf = RegisterCharacter(indexed, 0, 7, 1, 0, 0, 0, 2, true);
            RegisterCharacter(indexed, 1, 2, 2, 90, 0, 0, 9, false);
            indexed.Runtime.Flow.InputPhase = 2;
            indexed.Rng.Seed(0xC011u);
            indexed.NativeRandom.ResetFromSeed(0xC011u);
            indexed.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            indexed.ResetAiDecisionShadowDiagnostics();
            uint legacyStateBefore = indexed.Rng.State;
            ulong legacyCallsBefore = indexed.Rng.CallCount;
            NTSD28NativeRandomScalarState nativeBefore =
                indexed.NativeRandom.CaptureScalarState();

            indexed.CharacterInputAll(2);

            NTSD28NativeRandomScalarState nativeAfter =
                indexed.NativeRandom.CaptureScalarState();
            Assert.That(indexedSelf.Runtime.Unk360, Is.EqualTo(1));
            Assert.That(nativeAfter.SynchronizedCalls,
                Is.GreaterThan(nativeBefore.SynchronizedCalls));
            Assert.That(indexed.Rng.State, Is.EqualTo(legacyStateBefore));
            Assert.That(indexed.Rng.CallCount, Is.EqualTo(legacyCallsBefore));
            Assert.That(indexed.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(indexed.AiDecisionSharedRefreshCountForDiagnostics, Is.EqualTo(3),
                "the shared pass publishes the AI producer row before routing, then both retained rows");
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
            indexed.NativeRandom.ResetFromSeed(0xFA11u);
            indexed.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            indexed.SetAiDecisionIndexedCanonicalPreCommitFailureForSelfCheck(
                AiDecisionAvailability.EpochMismatch);
            indexed.ResetAiDecisionShadowDiagnostics();
            NTSD28NativeRandomScalarState nativeBefore =
                indexed.NativeRandom.CaptureScalarState();

            legacy.CharacterInputAll(2);
            indexed.CharacterInputAll(2);

            AssertDecisionStateEqual(legacy, legacySelf, indexed, indexedSelf);
            NTSD28NativeRandomScalarState nativeAfter =
                indexed.NativeRandom.CaptureScalarState();
            Assert.That(nativeAfter.SynchronizedCounter,
                Is.EqualTo(nativeBefore.SynchronizedCounter));
            Assert.That(nativeAfter.SynchronizedIndex,
                Is.EqualTo(nativeBefore.SynchronizedIndex));
            Assert.That(nativeAfter.SynchronizedCalls,
                Is.EqualTo(nativeBefore.SynchronizedCalls));
            Assert.That(indexed.AiDecisionIndexedCanonicalCommittedCountForDiagnostics, Is.Zero);
            Assert.That(indexed.AiDecisionIndexedCanonicalFallbackCountForDiagnostics, Is.EqualTo(1));
            Assert.That(indexed.AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics,
                Is.EqualTo(AiDecisionAvailability.EpochMismatch));
        }

        [Test]
        public void DataOrientedProfile_SynchronizedAuthoritySkipsLegacyPosition38Tree()
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
            dataOriented.NativeRandom.ResetFromSeed(27u);
            int legacyProducerComboDua = int.MinValue;
            int dataProducerComboDua = int.MinValue;
            legacy.SetCharacterInputProducerPassMutationOverrideForSelfCheck((_, entity) =>
            {
                if (ReferenceEquals(entity, legacySelf))
                    legacyProducerComboDua = entity.Runtime.ComboDua;
            });
            dataOriented.SetCharacterInputProducerPassMutationOverrideForSelfCheck((_, entity) =>
            {
                if (ReferenceEquals(entity, dataSelf))
                    dataProducerComboDua = entity.Runtime.ComboDua;
            });

            legacy.CharacterInputAll(2);
            dataOriented.CharacterInputAll(2);

            Assert.That(legacySelf.Runtime.Unk360, Is.EqualTo(1));
            Assert.That(legacyTarget.GetState(), Is.EqualTo(3));
            Assert.That(legacyProducerComboDua, Is.EqualTo(3),
                "the producer phase must reach source-derived position38 predicted-DUA branch");
            Assert.That(dataProducerComboDua, Is.Zero,
                "NTSD 2.8 synchronized authority must not execute the old 66-expression profile tree");
            Assert.That(dataOriented.NativeRandom.CaptureScalarState().SynchronizedCalls,
                Is.GreaterThan(0UL));
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
            var noOracle = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, true);
            RegisterCharacter(world, 1, 2, 2, 90, 0, 0, 9, true);
            RegisterCharacter(noOracle, 0, 1, 1, 0, 0, 0, 0, true);
            RegisterCharacter(noOracle, 1, 2, 2, 90, 0, 0, 9, true);
            world.Runtime.Flow.InputPhase = 2;
            noOracle.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(0x0A11u);
            noOracle.Rng.Seed(0x0A11u);
            world.NativeRandom.ResetFromSeed(0x0A11u);
            noOracle.NativeRandom.ResetFromSeed(0x0A11u);
            world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            noOracle.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
            world.AiDecisionIndexedCanonicalFullOracleSampleInterval = 2;
            world.ResetAiDecisionShadowDiagnostics();
            noOracle.ResetAiDecisionShadowDiagnostics();

            world.CharacterInputAll(2);
            noOracle.CharacterInputAll(2);

            NTSD28NativeRandomScalarState sampled =
                world.NativeRandom.CaptureScalarState();
            NTSD28NativeRandomScalarState unsampled =
                noOracle.NativeRandom.CaptureScalarState();

            Assert.That(world.AiDecisionIndexedCanonicalEligibleCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionIndexedCanonicalCommittedCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(world.AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics,
                Is.Zero);
            Assert.That(sampled.SynchronizedCalls, Is.GreaterThan(0UL));
            Assert.That(sampled.SynchronizedCounter,
                Is.EqualTo(unsampled.SynchronizedCounter));
            Assert.That(sampled.SynchronizedIndex,
                Is.EqualTo(unsampled.SynchronizedIndex));
            Assert.That(sampled.SynchronizedCalls,
                Is.EqualTo(unsampled.SynchronizedCalls),
                "the sampled Full oracle must not commit its candidate cursor");
            Assert.That(sampled.LastSynchronizedCallSite,
                Is.EqualTo(unsampled.LastSynchronizedCallSite));
        }

        [Test]
        public void IndexedOracleComparison_DetectsCallSiteDifference()
        {
            var fullSnapshot = new AiDecisionSnapshot(1);
            var indexedSnapshot = new AiDecisionSnapshot(1);
            fullSnapshot.RngTraceCallSites[0] = 0x1Eu;
            indexedSnapshot.RngTraceCallSites[0] = 0x21u;
            fullSnapshot.RngTraceModuli[0] = 17;
            indexedSnapshot.RngTraceModuli[0] = 17;
            fullSnapshot.RngTraceRaw[0] = 9;
            indexedSnapshot.RngTraceRaw[0] = 9;
            fullSnapshot.RngTraceValues[0] = 9;
            indexedSnapshot.RngTraceValues[0] = 9;
            var full = new AiDecisionWitness
            {
                RngDrawCount = 1,
                RngOrderHash = 123UL,
            };
            var indexed = new AiDecisionWitness
            {
                RngDrawCount = 1,
                RngOrderHash = 123UL,
            };

            Assert.That(
                SimulationAiDecisionModule.CompareIndexedWitnesses(
                    fullSnapshot,
                    full,
                    true,
                    indexedSnapshot,
                    indexed,
                    true),
                Is.EqualTo(AiDecisionIndexedMismatchReason.RngTrace));
        }

        [Test]
        public void DeepShadow_RemainsPerAiOracleAlongsideSharedLegacyRows()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, true);
            RegisterCharacter(world, 1, 2, 2, 90, 0, 0, 9, true);
            world.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(0xB17Au);
            world.NativeRandom.ResetFromSeed(0xB17Au);
            world.AiDecisionShadowMode = AiDecisionShadowMode.Shadow;
            world.ResetAiDecisionShadowDiagnostics();
            NTSD28NativeRandomScalarState nativeBefore =
                world.NativeRandom.CaptureScalarState();

            world.CharacterInputAll(2);

            NTSD28NativeRandomScalarState nativeAfter =
                world.NativeRandom.CaptureScalarState();
            Assert.That(world.AiDecisionSharedBuildCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiDecisionSharedRefreshCountForDiagnostics,
                Is.GreaterThanOrEqualTo(2));
            Assert.That(world.AiDecisionIndexedEligibleCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedAvailableCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedComparedCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionIndexedMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiDecisionShadowAvailableCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.AiDecisionShadowMismatchCountForDiagnostics, Is.Zero);
            AssertNativeRandomScalarStateEqual(nativeBefore, nativeAfter,
                "DeepShadow oracle evaluation must discard synchronized RNG candidates");
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
            world.NativeRandom.ResetFromSeed(0xB17Au);
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            world.SetAiDecisionSharedPostLegacyStateMutationForSelfCheck(0, 14);
            world.ResetAiDecisionShadowDiagnostics();
            NTSD28NativeRandomScalarState nativeBefore =
                world.NativeRandom.CaptureScalarState();

            LF2FrameData lowProducerFrame = low.Frame.D;
            int originalLowProducerFrameState = lowProducerFrame.state;
            try
            {
                world.CharacterInputAll(2);

                Assert.That(lowProducerFrame.state, Is.EqualTo(14),
                    "the producer-side mutation must precede later AI evaluation");
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
                AssertNativeRandomScalarStateEqual(
                    nativeBefore,
                    world.NativeRandom.CaptureScalarState(),
                    "SharedShadow oracle evaluation must discard synchronized RNG candidates");
            }
            finally
            {
                lowProducerFrame.state = originalLowProducerFrameState;
            }

            Assert.That(lowProducerFrame.state, Is.EqualTo(originalLowProducerFrameState));
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
                Is.EqualTo(2 * slots.Length),
                "each AI publishes once after producer freeze and once after retained routing");
            Assert.That(unified.AiSoACandidateFusedSnapshotBuildCountForDiagnostics,
                Is.Zero);
            Assert.That(unified.AiDecisionSharedBuildCountForDiagnostics, Is.Zero);
            Assert.That(unified.AiSoACandidateSnapshotRefreshCountForDiagnostics,
                Is.Zero);
            Assert.That(unified.AiDecisionSharedRefreshCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void SourceCoordinateIncremental_DefaultOff()
        {
            var world = new SimulationWorld();
            Assert.That(SourceCoordinateProperty(
                "AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics").GetValue(world), Is.False);
            Assert.That(SourceCoordinateRefreshCount(world), Is.Zero);
        }

        [TestCase(500, 610)]
        [TestCase(-311, -73)]
        [TestCase(0, 0)]
        public void SourceCoordinateIncremental_FullRefreshParityAcrossBothBoundaries(int sourceX, int sourceZ)
        {
            var full = new SimulationWorld();
            var candidate = new SimulationWorld
            {
                ValidateIncrementalAiUnifiedRowForDiagnostics = true,
            };
            SetSourceCoordinateIncremental(candidate, true);
            int[] slots = { 0, 21, 43 };
            var left = new LF2Character[slots.Length];
            var right = new LF2Character[slots.Length];
            for (int index = 0; index < slots.Length; index++)
            {
                int state = index == 2 ? 14 : 2;
                left[index] = RegisterCharacter(full, slots[index], index + 1, index & 1,
                    index * 60 + 10, 0, index * 4, state, true);
                right[index] = RegisterCharacter(candidate, slots[index], index + 1, index & 1,
                    index * 60 + 10, 0, index * 4, state, true);
            }
            ConfigureGateBWorld(full, true, 0xC005u);
            ConfigureGateBWorld(candidate, true, 0xC005u);
            for (int tick = 2; tick < 10; tick++)
            {
                for (int index = 0; index < slots.Length; index++)
                {
                    SetSourceCoordinatePosition(left[index], sourceX + index * 40 + tick, sourceZ + index * 4);
                    SetSourceCoordinatePosition(right[index], sourceX + index * 40 + tick, sourceZ + index * 4);
                }
                full.CharacterInputAll(tick);
                candidate.CharacterInputAll(tick);
                AssertSourceCoordinateRowsEqual(full, candidate);
                for (int index = 0; index < slots.Length; index++)
                {
                    AssertDecisionStateEqual(full, left[index], candidate, right[index]);
                    AssertEntityObservableStateEqual(left[index], right[index]);
                    Assert.That(right[index].Runtime.SourceRuleXInt,
                        Is.EqualTo(left[index].Runtime.SourceRuleXInt));
                    Assert.That(right[index].Runtime.SourceRuleZInt,
                        Is.EqualTo(left[index].Runtime.SourceRuleZInt));
                }
                AssertNativeRandomScalarStateEqual(full.NativeRandom.CaptureScalarState(),
                    candidate.NativeRandom.CaptureScalarState(), "source incremental native RNG");
                Assert.That(candidate.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(),
                    Is.EqualTo(full.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck()),
                    "post-pass live validator reads physical positions after the source latch clears");
            }
            Assert.That(candidate.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics,
                Is.EqualTo(16L * slots.Length));
            Assert.That(SourceCoordinateRefreshCount(candidate), Is.InRange(1L, 16L * slots.Length));
            Assert.That(candidate.AiUnifiedSnapshotExecutionIncrementalValidationCountForDiagnostics,
                Is.EqualTo(SourceCoordinateRefreshCount(candidate)),
                "only source incremental commits use the incremental validation oracle");
            Assert.That(SourceCoordinateRefreshCount(full), Is.Zero);
            Assert.That(candidate.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics, Is.Zero,
                "source-mode rolling rebuild protection is not relaxed by this candidate");
        }

        [Test]
        public void SourceCoordinateIncremental_PublisherStagesSourceXZAndPreservesPendingY()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 21, 1, 1, 10, 0, 4, 2, false);
            SetSourceCoordinatePosition(character, 500, 610);
            ConfigureGateBWorld(world, true, 0xC006u);
            Invoke(world, "BuildAiInputSlotSnapshot");
            var publisher = world.AiUnifiedRowPublisherForServices;
            int[] x = SourceCoordinatePublisherArray<int>(publisher, "x");
            int[] y = SourceCoordinatePublisherArray<int>(publisher, "y");
            int[] z = SourceCoordinatePublisherArray<int>(publisher, "z");
            uint generation = SourceCoordinatePublisherArray<uint>(publisher, "generations")[21];
            publisher.PublishIntegerPosition(21, generation, 999, 17, 888);
            SourceCoordinatePublisherMethod(publisher).Invoke(publisher,
                new object[] { 21, generation, -123, 246 });
            Assert.That(x[21], Is.EqualTo(500), "staging cannot publish before the boundary");
            Assert.That(y[21], Is.Zero);
            Assert.That(z[21], Is.EqualTo(610));
            Assert.That(publisher.TryCommitPending(21, generation, out bool roleChanged, out _), Is.True);
            Assert.That(x[21], Is.EqualTo(-123));
            Assert.That(y[21], Is.EqualTo(17), "source X/Z must not overwrite the physical Y pending value");
            Assert.That(z[21], Is.EqualTo(246));
            Assert.That(roleChanged, Is.True);
            EndUnifiedManualPass(world);
        }

        [Test]
        public void SourceCoordinateIncremental_PublisherRejectsStaleGeneration()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 21, 1, 1, 10, 0, 4, 2, false);
            SetSourceCoordinatePosition(character, 500, 610);
            ConfigureGateBWorld(world, true, 0xC007u);
            Invoke(world, "BuildAiInputSlotSnapshot");
            var publisher = world.AiUnifiedRowPublisherForServices;
            uint generation = SourceCoordinatePublisherArray<uint>(publisher, "generations")[21];
            MethodInfo method = SourceCoordinatePublisherMethod(publisher);
            TargetInvocationException failure = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(publisher, new object[] { 21, generation + 1u, -123, 246 }));
            Assert.That(failure.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(SourceCoordinatePublisherArray<int>(publisher, "x")[21], Is.EqualTo(500));
            Assert.That(publisher.HasPendingValues(21, generation), Is.False);
            EndUnifiedManualPass(world);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SourceCoordinateIncremental_ForceFullOrMutationHookKeepsFullCapture(bool useHook)
        {
            var world = new SimulationWorld();
            SetSourceCoordinateIncremental(world, true);
            LF2Character character = RegisterCharacter(world, 21, 1, 1, 10, 0, 4, 2, true);
            SetSourceCoordinatePosition(character, 500, 610);
            ConfigureGateBWorld(world, true, 0xC008u);
            if (useHook)
                world.SetCharacterInputProducerPassMutationOverrideForSelfCheck((_, __) => { });
            else
                world.ForceFullCharacterInputPostRefreshForDiagnostics = true;
            world.CharacterInputAll(2);
            Assert.That(SourceCoordinateRefreshCount(world), Is.Zero);
            Assert.That(world.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(), Is.True);
        }

        [Test]
        public void SourceCoordinateIncremental_NonSourceRowsRetainExistingIncrementalPath()
        {
            var world = new SimulationWorld
            {
                ValidateIncrementalAiUnifiedRowForDiagnostics = true,
            };
            SetSourceCoordinateIncremental(world, true);
            LF2Character character = RegisterCharacter(world, 21, 1, 1, 10, 0, 4, 2, true);
            character.Runtime.SourceRulePositionInitialized = false;
            ConfigureGateBWorld(world, true, 0xC009u);
            world.CharacterInputAll(2);
            Assert.That(SourceCoordinateRefreshCount(world), Is.Zero);
            Assert.That(world.AiUnifiedSnapshotExecutionIncrementalValidationCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(), Is.True);
        }

        [Test]
        public void SourceCoordinateIncremental_CustomSubtypeKeepsFullCapture()
        {
            var world = new SimulationWorld();
            SetSourceCoordinateIncremental(world, true);
            var frame = new LF2FrameData { frameId = 0, state = 2, wait = 100, next = 0 };
            var data = new LF2CharacterData
            {
                type_sub = 0,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new SourceCoordinateCustomCharacter();
            character.ModuleInitialize();
            character.ObjectId = 1;
            character.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            character.Frame.D = frame;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(21);
            character.Controller = new EmptyController();
            character.AiControlled = true;
            world.Register(character);
            SetSourceCoordinatePosition(character, 500, 610);
            ConfigureGateBWorld(world, true, 0xC010u);
            world.CharacterInputAll(2);
            Assert.That(SourceCoordinateRefreshCount(world), Is.Zero);
            Assert.That(world.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics, Is.EqualTo(2));
        }

        [Test]
        public void SourceCoordinateIncremental_WarmedInputPassHasNoManagedAllocation()
        {
            var world = new SimulationWorld();
            SetSourceCoordinateIncremental(world, true);
            for (int index = 0; index < 3; index++)
            {
                LF2Character character = RegisterCharacter(world, 20 + index, index + 1, index & 1,
                    index * 60, 0, index * 4, 2, false);
                SetSourceCoordinatePosition(character, 500 + index * 60, 610 + index * 4);
            }
            ConfigureGateBWorld(world, true, 0xC011u);
            for (int tick = 2; tick < 34; tick++)
                world.CharacterInputAll(tick);
            long sourceBefore = SourceCoordinateRefreshCount(world);
            using (var recorder = new NTSD.Test.Editor.BattleScopedGcAllocationRecorder())
            {
                var before = recorder.Calibrate();
                Assert.That(before.passed, Is.True);
                bool started = recorder.Begin();
                NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult scope;
                try
                {
                    for (int tick = 34; tick < 50; tick++)
                        world.CharacterInputAll(tick);
                }
                finally
                {
                    scope = recorder.End();
                }
                var after = recorder.Calibrate();
                Assert.That(started, Is.True);
                Assert.That(NTSD.Test.Editor.BattleScopedGcAllocationRecorder.HasZeroEvents(
                    scope, before, after), Is.True, "complete local warmed CharacterInputAll scope");
            }
            Assert.That(SourceCoordinateRefreshCount(world) - sourceBefore, Is.EqualTo(16L * 3));
            Assert.That(world.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics, Is.EqualTo(48L * 3));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SourceCoordinateIncremental_RoleMutationKeepsFullCapture(bool changeState)
        {
            var world = new SimulationWorld();
            SetSourceCoordinateIncremental(world, true);
            LF2Character character = RegisterCharacter(world, 21, 1, 1, 10, 17, 4, 2, false);
            SetSourceCoordinatePosition(character, 500, 610);
            ConfigureGateBWorld(world, true, 0xC012u);
            Invoke(world, "BuildAiInputSlotSnapshot");
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedGroundRoleCountForTests, Is.EqualTo(1));
            if (changeState)
                character.Frame.D.state = 14;
            else
                character.Runtime.HitStop = 3;
            Invoke(world, "RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput", character);
            Assert.That(SourceCoordinateRefreshCount(world), Is.Zero);
            Assert.That(world.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics, Is.EqualTo(1));
            Assert.That(world.AiUnifiedSnapshotExecutionPublishedGroundRoleCountForTests, Is.Zero);
            Assert.That(world.ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck(), Is.True);
            EndUnifiedManualPass(world);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SourceCoordinateIncremental_Isolated1000InputCost(bool aiControlled)
        {
            const int count = 1000;
            const int samplesPerBlock = 12;
            var full = new SimulationWorld(BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            var candidate = new SimulationWorld(BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            SetSourceCoordinateIncremental(candidate, true);
            var left = new LF2Character[count];
            var right = new LF2Character[count];
            for (int index = 0; index < count; index++)
            {
                int x = index % 40 * 40;
                int z = index / 40 * 10;
                left[index] = RegisterCharacter(full, index, 1, index & 1, x, 0, z, 2, aiControlled);
                right[index] = RegisterCharacter(candidate, index, 1, index & 1, x, 0, z, 2, aiControlled);
                SetSourceCoordinatePosition(left[index], x + 500, z + 610);
                SetSourceCoordinatePosition(right[index], x + 500, z + 610);
            }
            Assert.That(full.ObjectCount, Is.EqualTo(count), "all requested registrations must succeed");
            Assert.That(candidate.ObjectCount, Is.EqualTo(count));
            Assert.That(full.RuntimeSlotCapacityForDiagnostics, Is.GreaterThanOrEqualTo(count));
            ConfigureGateBWorld(full, true, 0xC014u);
            ConfigureGateBWorld(candidate, true, 0xC014u);
            for (int tick = 2; tick < 18; tick++)
            {
                full.CharacterInputAll(tick);
                candidate.CharacterInputAll(tick);
            }
            long sourceBefore = SourceCoordinateRefreshCount(candidate);
            long aiReadsBefore = candidate.AiUnifiedSnapshotExecutionReadCountForDiagnostics;
            double totalFullMs = 0;
            double totalCandidateMs = 0;
            using (var recorder = new NTSD.Test.Editor.BattleScopedGcAllocationRecorder())
            {
                for (int block = 0; block < 4; block++)
                {
                    int tick = 18 + block * samplesPerBlock;
                    double fullMs;
                    double candidateMs;
                    if ((block & 1) == 0)
                    {
                        fullMs = MeasureSourceCoordinateInputBlock(full, recorder, tick, samplesPerBlock);
                        candidateMs = MeasureSourceCoordinateInputBlock(candidate, recorder, tick, samplesPerBlock);
                    }
                    else
                    {
                        candidateMs = MeasureSourceCoordinateInputBlock(candidate, recorder, tick, samplesPerBlock);
                        fullMs = MeasureSourceCoordinateInputBlock(full, recorder, tick, samplesPerBlock);
                    }
                    totalFullMs += fullMs;
                    totalCandidateMs += candidateMs;
                    AssertSourceCoordinateRowsEqual(full, candidate);
                    AssertNativeRandomScalarStateEqual(full.NativeRandom.CaptureScalarState(),
                        candidate.NativeRandom.CaptureScalarState(), "isolated 1000 input native RNG");
                    for (int index = 0; index < count; index++)
                    {
                        AssertDecisionStateEqual(full, left[index], candidate, right[index]);
                        AssertEntityObservableStateEqual(left[index], right[index]);
                    }
                    Assert.That(full.LastNTSD28InputProducerFreezeCountForDiagnostics, Is.EqualTo(count));
                    Assert.That(candidate.LastNTSD28InputProducerFreezeCountForDiagnostics, Is.EqualTo(count));
                    TestContext.WriteLine("LOCAL_INPUT_ONLY ai=" + aiControlled + " block=" + block +
                        " OFF_ms_per_pass=" + fullMs / samplesPerBlock +
                        " ON_ms_per_pass=" + candidateMs / samplesPerBlock);
                }
            }
            Assert.That(SourceCoordinateRefreshCount(full), Is.Zero);
            Assert.That(SourceCoordinateRefreshCount(candidate) - sourceBefore, Is.GreaterThan(0));
            Assert.That(candidate.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics,
                Is.EqualTo(64L * count * (aiControlled ? 2 : 1)));
            Assert.That(candidate.AiUnifiedSnapshotExecutionReadCountForDiagnostics - aiReadsBefore,
                Is.EqualTo(aiControlled ? 48L * count : 0));
            Assert.That(candidate.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics, Is.Zero);
            TestContext.WriteLine("LOCAL_INPUT_ONLY ai=" + aiControlled + " samples=48" +
                " entities=" + count + " sampled_source_incremental=" +
                (SourceCoordinateRefreshCount(candidate) - sourceBefore) +
                " OFF_mean_ms=" + totalFullMs / 48 +
                " ON_mean_ms=" + totalCandidateMs / 48 +
                " relative_change_percent=" + (totalCandidateMs / totalFullMs - 1) * 100);
        }

        private static double MeasureSourceCoordinateInputBlock(
            SimulationWorld world,
            NTSD.Test.Editor.BattleScopedGcAllocationRecorder recorder,
            int firstTick,
            int samples)
        {
            var before = recorder.Calibrate();
            bool started = recorder.Begin();
            long begin = System.Diagnostics.Stopwatch.GetTimestamp();
            NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult scope;
            long end;
            try
            {
                for (int tick = firstTick; tick < firstTick + samples; tick++)
                    world.CharacterInputAll(tick);
            }
            finally
            {
                end = System.Diagnostics.Stopwatch.GetTimestamp();
                scope = recorder.End();
            }
            var after = recorder.Calibrate();
            Assert.That(started, Is.True);
            Assert.That(NTSD.Test.Editor.BattleScopedGcAllocationRecorder.HasZeroEvents(
                scope, before, after), Is.True, "isolated input block calibrated GC event scope");
            return (end - begin) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
        }

        private static PropertyInfo SourceCoordinateProperty(string name)
        {
            PropertyInfo property = typeof(SimulationWorld).GetProperty(name, InstanceMembers);
            Assert.That(property, Is.Not.Null, "Missing source-coordinate candidate API: " + name);
            return property;
        }

        private static void SetSourceCoordinateIncremental(SimulationWorld world, bool enabled)
        {
            SourceCoordinateProperty("AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics")
                .SetValue(world, enabled);
        }

        private static long SourceCoordinateRefreshCount(SimulationWorld world)
        {
            return (long)SourceCoordinateProperty(
                "AiUnifiedSnapshotSourceCoordinateRefreshCountForDiagnostics").GetValue(world);
        }

        private static void SetSourceCoordinatePosition(LF2Character character, double x, double z)
        {
            character.Runtime.SetSourceRulePosition(x, z);
            character.Runtime.SyncSourceRuleIntegerPosition();
        }

        private static T[] SourceCoordinatePublisherArray<T>(object publisher, string fieldName)
        {
            return (T[])publisher.GetType().GetField(fieldName, InstanceMembers).GetValue(publisher);
        }

        private static MethodInfo SourceCoordinatePublisherMethod(object publisher)
        {
            MethodInfo method = publisher.GetType().GetMethod("PublishSourceRuleCoordinates", InstanceMembers);
            Assert.That(method, Is.Not.Null, "Missing source X/Z staging API");
            return method;
        }

        private static void AssertSourceCoordinateRowsEqual(SimulationWorld full, SimulationWorld candidate)
        {
            PropertyInfo property = typeof(SimulationWorld).GetProperty(
                "aiUnifiedSnapshotPublishedState", InstanceMembers);
            object fullState = property.GetValue(full);
            object candidateState = property.GetValue(candidate);
            object fullRows = fullState.GetType().GetField("Rows", InstanceMembers).GetValue(fullState);
            object candidateRows = candidateState.GetType().GetField("Rows", InstanceMembers).GetValue(candidateState);
            foreach (string fieldName in new[]
            {
                "Included", "Generation", "Identity", "ObjectId", "NativeAiProfileObjectId",
                "DataObjectType", "InputHistoryGate", "X", "Y", "Z", "Hp", "Hp3", "HpMax",
                "Pp", "Team", "State", "Frame", "HitJ", "LinkState", "OwnerSlot", "CachedTargetSlot",
                "CoordinateTargetX", "Vx", "Facing", "TargetSlot", "HitStop", "BoundaryFlags",
                "SpecialScanMember",
            })
            {
                FieldInfo field = fullRows.GetType().GetField(fieldName, InstanceMembers);
                CollectionAssert.AreEqual((Array)field.GetValue(fullRows),
                    (Array)field.GetValue(candidateRows), fieldName);
            }
            Assert.That(AiSensingKernel.ValidateIndexedContract((AiSensingSnapshot)fullRows), Is.True);
            Assert.That(AiSensingKernel.ValidateIndexedContract((AiSensingSnapshot)candidateRows), Is.True);
            foreach (string fieldName in new[]
            {
                "MoveModeFirst10Present", "MoveModeFirst10Eligible", "MoveModeFirst10Generation",
                "MoveModeFirst10Hp", "MoveModeFirst10X", "MoveModeFirst10Z",
            })
            {
                FieldInfo field = fullState.GetType().GetField(fieldName, InstanceMembers);
                CollectionAssert.AreEqual((Array)field.GetValue(fullState),
                    (Array)field.GetValue(candidateState), fieldName);
            }
            Assert.That(candidateState.GetType().GetField("MoveModeFirst10Valid", InstanceMembers)
                .GetValue(candidateState), Is.True);
        }

        private sealed class SourceCoordinateCustomCharacter : LF2Character
        {
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void SourceCoordinateIncremental_DriverApplicableFormalRoot(
            string side, string trace, string expectedSha)
        {
            string tracePath = SourceCoordinateAdmissionConstant("RootTrace") + trace;
            Assert.That(SourceCoordinateAdmissionMethod("HashFile").Invoke(null,
                new object[] { tracePath }), Is.EqualTo(expectedSha));
            var native = new List<JObject>();
            foreach (string line in File.ReadLines(tracePath))
            {
                JObject row = JObject.Parse(line);
                if ((int)row["tick"] >= 1 && (int)row["tick"] <= 12)
                    native.Add(row);
            }
            Assert.That(native.Count, Is.EqualTo(12));
            string scenario = SourceCoordinateAdmissionConstant("ScenarioRoot") +
                "unity-scenario-" + side + ".json";
            SourceCoordinateDriverRow[] full = RunSourceCoordinateDriver(
                scenario, false, ProductionEntityStressMode.Combat1000, 12, native.ToArray(), false);
            SourceCoordinateDriverRow[] candidate = RunSourceCoordinateDriver(
                scenario, false, ProductionEntityStressMode.Combat1000, 12, native.ToArray(), true);
            AssertSourceCoordinateDriverRowsEqual(full, candidate, false);
            SaveSourceCoordinateDriverRows(side, full, candidate, false);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void SourceCoordinateIncremental_DriverThousandCanonicalAi(
            ProductionEntityStressMode mode)
        {
            string scenario = SourceCoordinateAdmissionConstant("RoleSakuraScenario");
            SourceCoordinateDriverRow[] full = RunSourceCoordinateDriver(
                scenario, true, mode, 32, null, false);
            SourceCoordinateDriverRow[] candidate = RunSourceCoordinateDriver(
                scenario, true, mode, 32, null, true);
            SaveSourceCoordinateDriverRows(mode.ToString(), full, candidate, true);
            AssertSourceCoordinateDriverRowsEqual(full, candidate, true);
            for (int index = 16; index < full.Length; index++)
            {
                Assert.That(NTSD.Test.Editor.BattleScopedGcAllocationRecorder.HasZeroEvents(
                    full[index].gcScope, full[index].gcBefore, full[index].gcAfter), Is.True,
                    "baseline full Driver GC tick=" + full[index].tick);
                Assert.That(NTSD.Test.Editor.BattleScopedGcAllocationRecorder.HasZeroEvents(
                    candidate[index].gcScope, candidate[index].gcBefore, candidate[index].gcAfter), Is.True,
                    "candidate full Driver GC tick=" + candidate[index].tick);
            }
        }

        [Test]
        public void SourceCoordinateIncremental_DriverGcStageAttribution()
        {
            var attribution = new SourceCoordinateDriverGcAttribution();
            SourceCoordinateDriverRow[] rows = RunSourceCoordinateDriver(
                SourceCoordinateAdmissionConstant("RoleSakuraScenario"), true,
                ProductionEntityStressMode.Dispersed1000, 32, null, false, attribution);
            SourceCoordinateAdmissionMethod("SaveNew").Invoke(null, new object[]
            {
                "dispersed1000-stage-gc-attribution.json",
                new
                {
                    evidence = "GC_ATTRIBUTION_ONLY; stage prepare plus remaining StepOneTick; not a zero-GC qualification",
                    attribution,
                    ticks = rows.Select(row => new
                    {
                        row.tick, row.entities, row.actualAi, row.extendedHashes, row.lockstepHashes,
                        row.nativeState, row.legacyState, row.legacyCalls, row.gcScope,
                    }).ToArray(),
                },
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH95-SOURCE-COORDINATE-INCREMENTAL-20261009/gc-attribution-03/",
            });
            Assert.That(attribution.before.passed && attribution.after.passed, Is.True);
            for (int index = 16; index < rows.Length; index++)
            {
                Assert.That(attribution.stage[index].valid && rows[index].gcScope.valid, Is.True,
                    "both attribution scopes must be valid at tick=" + rows[index].tick);
                Assert.That(rows[index].actualAi, Is.EqualTo(1000));
            }
        }

        [Test]
        public void SourceCoordinateIncremental_DriverGcWrapperAttribution()
        {
            var attribution = new SourceCoordinateDriverGcAttribution { splitWrapper = true };
            SourceCoordinateDriverRow[] rows = RunSourceCoordinateDriver(
                SourceCoordinateAdmissionConstant("RoleSakuraScenario"), true,
                ProductionEntityStressMode.Dispersed1000, 32, null, false, attribution);
            SourceCoordinateAdmissionMethod("SaveNew").Invoke(null, new object[]
            {
                "dispersed1000-wrapper-gc-attribution.json",
                new
                {
                    evidence = "GC_ATTRIBUTION_ONLY; ordered wrapper decomposition; not a full Driver zero-GC qualification",
                    labels = new[] { "StopWorker", "CoreStep", "SoundDispatch", "InspectorRefresh" },
                    attribution,
                    ticks = rows.Select(row => new
                    {
                        row.tick, row.entities, row.actualAi, row.extendedHashes, row.lockstepHashes,
                        row.nativeState, row.legacyState, row.legacyCalls,
                    }).ToArray(),
                },
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH95-SOURCE-COORDINATE-INCREMENTAL-20261009/gc-attribution-04/",
            });
            Assert.That(attribution.before.passed && attribution.after.passed, Is.True);
            JObject baseline = JObject.Parse(File.ReadAllText(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH95-SOURCE-COORDINATE-INCREMENTAL-20261009/" +
                "driver-qualification-01/Dispersed1000-source-coordinate-driver-parity-94c326d0b02a46d88ea58afdff466844.json"));
            for (int index = 0; index < rows.Length; index++)
            {
                JObject current = JObject.FromObject(rows[index]);
                foreach (string field in new[]
                    { "extendedHashes", "lockstepHashes", "nativeState", "legacyState", "legacyCalls" })
                    Assert.That(JToken.DeepEquals(current[field], baseline["baseline"][index][field]),
                        Is.True, field + " decomposed wrapper tick=" + rows[index].tick);
                if (index <= 15)
                    continue;
                Assert.That(attribution.stage[index].valid, Is.True);
                foreach (var scope in attribution.wrapper[index])
                    Assert.That(scope.valid, Is.True, "wrapper attribution scope must be valid");
            }
        }

        [UnityTest]
        public IEnumerator SourceCoordinateIncremental_DriverGcCallsite()
        {
            var capture = new SourceCoordinateDriverGcCallsite();
            try
            {
                capture.Prepare();
                SourceCoordinateDriverRow[] rows = RunSourceCoordinateDriver(
                    SourceCoordinateAdmissionConstant("RoleSakuraScenario"), true,
                    ProductionEntityStressMode.Dispersed1000, 32, null, false, null, capture);
                yield return null;
                capture.StopAndExport(rows[16]);
                JObject baseline = JObject.Parse(File.ReadAllText(
                    "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH95-SOURCE-COORDINATE-INCREMENTAL-20261009/" +
                    "driver-qualification-01/Dispersed1000-source-coordinate-driver-parity-94c326d0b02a46d88ea58afdff466844.json"));
                for (int index = 0; index < rows.Length; index++)
                {
                    JObject current = JObject.FromObject(rows[index]);
                    foreach (string field in new[]
                        { "extendedHashes", "lockstepHashes", "nativeState", "legacyState", "legacyCalls" })
                        Assert.That(JToken.DeepEquals(current[field], baseline["baseline"][index][field]),
                            Is.True, field + " callsite capture tick=" + rows[index].tick);
                }
            }
            finally
            {
                capture.Restore();
            }
        }

        private sealed class SourceCoordinateDriverGcCallsite
        {
            private const string MarkerName = "NTSD.Optimization.Batch95.HeadlessDriver.GC";
            private readonly ProfilerMarker marker = new ProfilerMarker(MarkerName);
            private readonly Type profilerOwner = typeof(NTSD.Test.Editor.BattleOptimizationCpuGcCaptureEditor);
            private object settings;
            private string root;
            private int previousLastFrame;
            private bool previousHistorySaved;

            private MethodInfo Method(string name)
            {
                return profilerOwner.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            }

            public void Prepare()
            {
                NTSD.Test.Editor.BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
                root = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH95-SOURCE-COORDINATE-INCREMENTAL-20261009/" +
                    "gc-callsite-05/" + Guid.NewGuid().ToString("N");
                Assert.That(Directory.Exists(root), Is.False);
                Directory.CreateDirectory(root);
                settings = Method("ReadSettings").Invoke(null, null);
                if (ProfilerDriver.firstFrameIndex >= 0)
                {
                    Method("SaveProfilerHistory").Invoke(null,
                        new object[] { Path.Combine(root, "prior-history.raw") });
                    previousHistorySaved = true;
                }
                marker.Begin();
                marker.End();
            }

            public void BeforeTick(int tick)
            {
                if (tick != 17)
                    return;
                previousLastFrame = ProfilerDriver.lastFrameIndex;
                Profiler.enableBinaryLog = false;
                ProfilerDriver.profileEditor = true;
                ProfilerDriver.SetAreaEnabled(ProfilerArea.CPU, true);
                Profiler.enableAllocationCallstacks = true;
                Profiler.enabled = true;
            }

            public bool Step(SimulationTickDriver driver, FrameInputSet input)
            {
                marker.Begin();
                try
                {
                    return driver.StepOneTick(input, true, false);
                }
                finally
                {
                    marker.End();
                }
            }

            public void StopAndExport(SourceCoordinateDriverRow row)
            {
                Profiler.enabled = false;
                Method("SaveProfilerHistory").Invoke(null,
                    new object[] { Path.Combine(root, "cpu-gc.raw") });
                int first = previousLastFrame < 0
                    ? ProfilerDriver.firstFrameIndex : ProfilerDriver.GetNextFrameIndex(previousLastFrame);
                int last = ProfilerDriver.lastFrameIndex;
                var allocations = new List<object>();
                var stack = new List<ulong>(64);
                int envelopes = 0;
                int scanned = 0;
                bool stacksResolved = true;
                for (int frame = first; frame >= 0 && frame <= last;
                    frame = ProfilerDriver.GetNextFrameIndex(frame))
                {
                    Assert.That(++scanned, Is.LessThanOrEqualTo(128), "bounded fresh profiler frame range");
                    using (RawFrameDataView view = ProfilerDriver.GetRawFrameDataView(frame, 0))
                    {
                        if (!view.valid)
                            continue;
                        Assert.That(view.threadName, Is.EqualTo("Main Thread"));
                        for (int index = 0; index < view.sampleCount; index++)
                        {
                            if (view.GetSampleName(index) != MarkerName)
                                continue;
                            envelopes++;
                            double begin = view.GetSampleStartTimeMs(index);
                            double end = begin + view.GetSampleTimeMs(index);
                            Assert.That(double.IsNaN(begin) || double.IsInfinity(begin) || end < begin, Is.False);
                            for (int sample = index + 1; sample < view.sampleCount; sample++)
                            {
                                double start = view.GetSampleStartTimeMs(sample);
                                if (view.GetSampleName(sample) != "GC.Alloc" || start < begin || start >= end)
                                    continue;
                                stack.Clear();
                                view.GetSampleCallstack(sample, stack);
                                var calls = new List<object>(stack.Count);
                                bool resolved = false;
                                foreach (ulong address in stack)
                                {
                                    FrameDataView.MethodInfo method = view.ResolveMethodInfo(address);
                                    resolved |= !string.IsNullOrEmpty(method.methodName);
                                    calls.Add(new
                                    {
                                        address = address.ToString("X16"), method = method.methodName,
                                        file = method.sourceFileName, line = method.sourceFileLine,
                                    });
                                }
                                int metadata = view.GetSampleMetadataCount(sample);
                                stacksResolved &= resolved && metadata > 0;
                                allocations.Add(new
                                {
                                    frame, sample, startMs = start,
                                    bytes = metadata > 0 ? view.GetSampleMetadataAsLong(sample, 0) : -1,
                                    calls,
                                });
                            }
                        }
                    }
                    if (frame == last)
                        break;
                }
                Save("driver-gc-callstacks.json", new
                {
                    evidence = "HEADLESS_DRIVER_CALLSITE_ONLY; no performance or zero-GC acceptance",
                    marker = MarkerName, first, last, previousLastFrame, envelopes, stacksResolved,
                    row.tick, row.actualAi, row.gcScope, row.gcBefore, row.gcAfter, allocations,
                });
                Assert.That(envelopes, Is.EqualTo(1), "one complete warmed Driver envelope");
                Assert.That(row.gcBefore.passed && row.gcAfter.passed && row.gcScope.valid, Is.True);
                Assert.That(allocations.Count, Is.EqualTo(row.gcScope.allocationEvents));
                Assert.That(stacksResolved && allocations.Count > 0, Is.True, "actual allocation callsites required");
            }

            public void Restore()
            {
                if (settings == null)
                    return;
                Method("RestoreSettings").Invoke(null, new[] { settings });
                object after = Method("ReadSettings").Invoke(null, null);
                bool restored = JToken.DeepEquals(JObject.FromObject(settings), JObject.FromObject(after));
                Save("profiler-settings-audit.json", new
                {
                    before = settings, after, restored, previousHistorySaved,
                    history = "No clear or reload; retained previous history plus owned capture frames",
                });
                Assert.That(restored, Is.True, "all preserved Profiler settings restored");
            }

            private void Save(string file, object value)
            {
                using var stream = new FileStream(Path.Combine(root, file), FileMode.CreateNew,
                    FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream);
                writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
            }
        }

        private sealed class SourceCoordinateDriverGcAttribution
        {
            public string marker;
            public string unit;
            public bool boundaryManagerPresent;
            public int enabledBoundaryCount;
            public bool splitWrapper;
            public NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult[][] wrapper =
                Enumerable.Range(0, 32).Select(_ =>
                    new NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult[4]).ToArray();
            private Action<bool> stopWorker;
            private Func<FrameInputSet, bool, bool, bool> coreStep;
            private Action dispatchSounds;
            private Action refreshInspector;
            public NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult[] stage =
                new NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult[32];
            public NTSD.Test.Editor.BattleScopedGcAllocationRecorder.CalibrationResult before;
            public NTSD.Test.Editor.BattleScopedGcAllocationRecorder.CalibrationResult after;

            public void PrepareWrapper(SimulationTickDriver driver)
            {
                Type type = typeof(SimulationTickDriver);
                stopWorker = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), driver,
                    type.GetMethod("StopDedicatedSimulationWorker", InstanceMembers));
                coreStep = (Func<FrameInputSet, bool, bool, bool>)Delegate.CreateDelegate(
                    typeof(Func<FrameInputSet, bool, bool, bool>), driver,
                    type.GetMethod("StepOneTickInternal", InstanceMembers, null,
                        new[] { typeof(FrameInputSet), typeof(bool), typeof(bool) }, null));
                dispatchSounds = (Action)Delegate.CreateDelegate(typeof(Action), driver,
                    type.GetMethod("DispatchPublishedSoundsAfterSuccessfulTick", InstanceMembers));
                refreshInspector = (Action)Delegate.CreateDelegate(typeof(Action), driver,
                    type.GetMethod("RefreshInspectorState", InstanceMembers));
            }

            public bool StepSplit(FrameInputSet input, int index,
                NTSD.Test.Editor.BattleScopedGcAllocationRecorder recorder)
            {
                recorder.Begin();
                try
                {
                    stopWorker(true);
                }
                finally
                {
                    wrapper[index][0] = recorder.End();
                }
                bool stepped;
                recorder.Begin();
                try
                {
                    stepped = coreStep(input, false, true);
                }
                finally
                {
                    wrapper[index][1] = recorder.End();
                }
                recorder.Begin();
                try
                {
                    if (stepped)
                        dispatchSounds();
                }
                finally
                {
                    wrapper[index][2] = recorder.End();
                }
                recorder.Begin();
                try
                {
                    refreshInspector();
                }
                finally
                {
                    wrapper[index][3] = recorder.End();
                }
                return stepped;
            }
        }

        private static MethodInfo SourceCoordinateAdmissionMethod(string name)
        {
            MethodInfo method = typeof(NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests)
                .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "existing Driver qualification helper: " + name);
            return method;
        }

        private static string SourceCoordinateAdmissionConstant(string name)
        {
            return (string)typeof(NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests)
                .GetField(name, BindingFlags.Static | BindingFlags.NonPublic).GetRawConstantValue();
        }

        private static SourceCoordinateDriverRow[] RunSourceCoordinateDriver(
            string scenario, bool thousand, ProductionEntityStressMode mode,
            int ticks, JObject[] native, bool enabled,
            SourceCoordinateDriverGcAttribution attribution = null,
            SourceCoordinateDriverGcCallsite callsite = null)
        {
            var result = new SourceCoordinateDriverRow[callsite == null ? ticks : 17];
            Action<SimulationTickDriver, FrameInputSet[], NTSD.Simulation.Lockstep.LockstepSessionIdentity> verify =
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    var query = (BruteForceSceneQuery)world.SceneQuery;
                    query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                    SourceCoordinateAdmissionMethod("SetCandidate").Invoke(null, new object[] { query, true });
                    if (thousand)
                        SourceCoordinateAdmissionMethod("PrepareThousandAi").Invoke(null,
                            new object[] { world, mode, 1 });
                    bool previous = world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics;
                    bool previousOracle = world.ValidateIncrementalAiUnifiedRowForDiagnostics;
                    Assert.That(previous, Is.False, "source incremental remains default OFF");
                    NTSD.Test.Editor.BattleScopedGcAllocationRecorder recorder = null;
                    var before = default(NTSD.Test.Editor.BattleScopedGcAllocationRecorder.CalibrationResult);
                    var measured = new NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult[ticks];
                    var preparedRows = new Dictionary<object, object[]>();
                    foreach (string property in new[]
                    {
                        "aiUnifiedSnapshotPublishedState", "aiUnifiedSnapshotScratchState",
                        "aiUnifiedSnapshotStandbyState",
                    })
                    {
                        object state = typeof(SimulationWorld).GetProperty(property, InstanceMembers).GetValue(world);
                        if (state != null && !preparedRows.ContainsKey(state))
                            preparedRows.Add(state, SourceCoordinateDriverStorage(state));
                    }
                    Assert.That(preparedRows.Count, Is.EqualTo(2), "both execution buffers are cold prepared");
                    ulong nativeCallsBefore = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                    try
                    {
                        world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics = enabled;
                        world.ValidateIncrementalAiUnifiedRowForDiagnostics = true;
                        driver.ApplySettings(new LockstepSimulationSettings
                        {
                            driveMode = SimulationDriveMode.Manual,
                            enableFrameChecksum = false,
                        });
                        if (thousand)
                            recorder = new NTSD.Test.Editor.BattleScopedGcAllocationRecorder();
                        if (recorder != null)
                        {
                            before = recorder.Calibrate();
                            Assert.That(before.passed, Is.True, "full Driver GC positive/empty controls");
                            if (attribution != null)
                            {
                                attribution.marker = recorder.Marker;
                                attribution.unit = recorder.Unit;
                                attribution.before = before;
                                var boundaryManager = NTSD.LevelEditor.BoundaryWallManager.TryGetInstance();
                                attribution.boundaryManagerPresent = boundaryManager != null;
                                attribution.enabledBoundaryCount = boundaryManager?.EnabledBoundaries.Count ?? 0;
                                if (attribution.splitWrapper)
                                    attribution.PrepareWrapper(driver);
                            }
                        }
                        for (int tick = 1; tick <= result.Length; tick++)
                        {
                            callsite?.BeforeTick(tick);
                            if (attribution != null && tick > 16)
                            {
                                bool stageStarted = recorder.Begin();
                                try
                                {
                                    world.PrepareStageRuntimeSnapshotForTick(tick);
                                }
                                finally
                                {
                                    if (stageStarted)
                                        attribution.stage[tick - 1] = recorder.End();
                                }
                                Assert.That(stageStarted, Is.True, "stage attribution scope starts");
                            }
                            bool splitWrapper = attribution?.splitWrapper == true && tick > 16;
                            bool started = tick > 16 && recorder != null && !splitWrapper && recorder.Begin();
                            bool stepped;
                            try
                            {
                                if (splitWrapper)
                                {
                                    stepped = attribution.StepSplit(inputs[tick - 1], tick - 1, recorder);
                                    measured[tick - 1] = attribution.wrapper[tick - 1][1];
                                    started = true;
                                }
                                else
                                {
                                    stepped = callsite != null && tick == 17
                                        ? callsite.Step(driver, inputs[tick - 1])
                                        : driver.StepOneTick(inputs[tick - 1], true, false);
                                }
                            }
                            finally
                            {
                                if (started && !splitWrapper)
                                    measured[tick - 1] = recorder.End();
                            }
                            Assert.That(stepped, Is.True, "full Driver tick=" + tick);
                            if (tick > 16 && recorder != null)
                                Assert.That(started, Is.True, "full Driver GC scope starts");
                            if (native != null)
                                SourceCoordinateAdmissionMethod("CompareRoot").Invoke(null,
                                    new object[] { world, native[tick - 1], tick });
                            Assert.That(world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics,
                                Is.EqualTo(enabled), "cold owner remains applied");
                            Assert.That(world.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics,
                                Is.Zero, "full Driver hard breach tick=" + tick);
                            Assert.That(world.AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics,
                                Is.Zero, "full Driver precommit failure tick=" + tick);
                            Assert.That(query.LastFormalCollectorModeForDiagnostics,
                                Is.EqualTo(CollisionFormalCollectorMode.ForceBruteForce));
                            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
                            Assert.That(query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics ||
                                query.EnableRoleCapacityRecordGuardForDiagnostics ||
                                world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, Is.False,
                                "no unadmitted candidate composition");
                            object published = typeof(SimulationWorld).GetProperty(
                                "aiUnifiedSnapshotPublishedState", InstanceMembers).GetValue(world);
                            object rows = published.GetType().GetField("Rows", InstanceMembers).GetValue(published);
                            Assert.That(AiSensingKernel.ValidateIndexedContract((AiSensingSnapshot)rows), Is.True);
                            var actualRows = new List<object>();
                            var rowFields = new Dictionary<string, object>();
                            foreach (string field in new[]
                            {
                                "Included", "Generation", "Identity", "ObjectId", "NativeAiProfileObjectId",
                                "DataObjectType", "InputHistoryGate", "X", "Y", "Z", "Hp", "Hp3", "HpMax",
                                "Pp", "Team", "State", "Frame", "HitJ", "LinkState", "OwnerSlot", "CachedTargetSlot",
                                "CoordinateTargetX", "Vx", "Facing", "TargetSlot", "HitStop", "BoundaryFlags",
                                "SpecialScanMember",
                            })
                            {
                                object array = rows.GetType().GetField(field, InstanceMembers).GetValue(rows);
                                actualRows.Add(array);
                                rowFields.Add(field, array);
                            }
                            foreach (string field in new[]
                            {
                                "MoveModeFirst10Present", "MoveModeFirst10Eligible", "MoveModeFirst10Generation",
                                "MoveModeFirst10Hp", "MoveModeFirst10X", "MoveModeFirst10Z",
                            })
                            {
                                object array = published.GetType().GetField(field, InstanceMembers).GetValue(published);
                                actualRows.Add(array);
                                rowFields.Add(field, array);
                            }
                            Assert.That(preparedRows.ContainsKey(published), Is.True,
                                "published state belongs to the cold-prepared two-buffer set");
                            object[] expectedRows = preparedRows[published];
                            for (int index = 0; index < expectedRows.Length; index++)
                                Assert.That(ReferenceEquals(actualRows[index], expectedRows[index]), Is.True,
                                    "no hot row storage growth index=" + index);
                            int actualAi = 0;
                            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                                if (world.FindEntityByRuntimeSlotForQuery(slot) is LF2Character character &&
                                    character.AiControlled)
                                    actualAi++;
                            result[tick - 1] = new SourceCoordinateDriverRow
                            {
                                tick = tick,
                                entities = world.ObjectCount,
                                actualAi = actualAi,
                                extendedHashes = JsonConvert.SerializeObject(thousand
                                    ? (object)world.CaptureExtendedChecksumSnapshot(tick, inputs[tick - 1]).Hashes
                                    : world.CaptureParityFrameSnapshot(tick, inputs[tick - 1],
                                        includeFullDomains: true).Hashes),
                                lockstepHashes = JsonConvert.SerializeObject(world.CaptureLockstepChecksumSnapshot(
                                    tick, inputs[tick - 1]).Hashes),
                                nativeState = JsonConvert.SerializeObject(world.NativeRandom.CaptureScalarState()),
                                legacyState = world.Rng.State,
                                legacyCalls = world.Rng.CallCount,
                                publishedRows = JsonConvert.SerializeObject(rowFields),
                                sourceRefreshes = SourceCoordinateRefreshCount(world),
                                aiReads = world.AiUnifiedSnapshotExecutionReadCountForDiagnostics,
                                sourceRollForward = world.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics,
                                incrementalValidations = world.AiUnifiedSnapshotExecutionIncrementalValidationCountForDiagnostics,
                            };
                            if (thousand)
                                Assert.That(actualAi, Is.EqualTo(1000), "actual canonical AI tick=" + tick);
                        }
                        if (recorder != null)
                        {
                            var after = recorder.Calibrate();
                            if (attribution != null)
                                attribution.after = after;
                            for (int index = 16; index < result.Length; index++)
                            {
                                result[index].gcScope = measured[index];
                                result[index].gcBefore = before;
                                result[index].gcAfter = after;
                            }
                        }
                        if (thousand)
                        {
                            Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                                Is.GreaterThan(nativeCallsBefore), "canonical AI RNG actually ran");
                            Assert.That(world.AiUnifiedSnapshotExecutionReadCountForDiagnostics, Is.GreaterThan(0));
                            if (enabled)
                                Assert.That(SourceCoordinateRefreshCount(world), Is.GreaterThan(0),
                                    "candidate must actually exercise source-coordinate refresh");
                        }
                    }
                    finally
                    {
                        try
                        {
                            recorder?.Dispose();
                        }
                        finally
                        {
                            world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics = previous;
                            world.ValidateIncrementalAiUnifiedRowForDiagnostics = previousOracle;
                            Assert.That(world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics,
                                Is.EqualTo(previous), "cold flag restores before ordered Driver shutdown");
                            Assert.That(world.ValidateIncrementalAiUnifiedRowForDiagnostics,
                                Is.EqualTo(previousOracle), "cold oracle restores");
                        }
                    }
                };
            SourceCoordinateAdmissionMethod("WithRoleDriverScenario").Invoke(null, new object[]
            {
                SourceCoordinateAdmissionConstant("RuntimeRoot"), scenario,
                thousand ? BattleRuntimeProfile.MobileExtended : BattleRuntimeProfile.Authority400,
                ticks, verify, true,
            });
            return result;
        }

        private static void AssertSourceCoordinateDriverRowsEqual(
            SourceCoordinateDriverRow[] full, SourceCoordinateDriverRow[] candidate, bool thousand)
        {
            Assert.That(candidate.Length, Is.EqualTo(full.Length));
            for (int index = 0; index < full.Length; index++)
            {
                SourceCoordinateDriverRow a = full[index], b = candidate[index];
                string label = "source-coordinate Driver first-difference tick=" + a.tick;
                Assert.That(b.entities, Is.EqualTo(a.entities), label + " entities");
                Assert.That(b.actualAi, Is.EqualTo(a.actualAi), label + " AI roster");
                Assert.That(b.extendedHashes, Is.EqualTo(a.extendedHashes), label + " complete hashes");
                Assert.That(b.lockstepHashes, Is.EqualTo(a.lockstepHashes), label + " lockstep hashes");
                Assert.That(b.nativeState, Is.EqualTo(a.nativeState), label + " full native scalar RNG");
                Assert.That(b.legacyState, Is.EqualTo(a.legacyState), label + " legacy RNG state");
                Assert.That(b.legacyCalls, Is.EqualTo(a.legacyCalls), label + " legacy RNG calls");
                Assert.That(b.publishedRows, Is.EqualTo(a.publishedRows), label + " row/first10 fields");
                Assert.That(a.sourceRefreshes, Is.Zero, label + " baseline does not use candidate");
                Assert.That(b.sourceRollForward, Is.EqualTo(a.sourceRollForward), label + " source rebuild protection");
                Assert.That(b.aiReads, Is.EqualTo(a.aiReads), label + " actual consumer reads");
            }
            if (thousand)
                Assert.That(candidate[candidate.Length - 1].sourceRefreshes, Is.GreaterThan(0));
        }

        private static object[] SourceCoordinateDriverStorage(object published)
        {
            object rows = published.GetType().GetField("Rows", InstanceMembers).GetValue(published);
            var arrays = new List<object>();
            foreach (string field in new[]
            {
                "Included", "Generation", "Identity", "ObjectId", "NativeAiProfileObjectId",
                "DataObjectType", "InputHistoryGate", "X", "Y", "Z", "Hp", "Hp3", "HpMax",
                "Pp", "Team", "State", "Frame", "HitJ", "LinkState", "OwnerSlot", "CachedTargetSlot",
                "CoordinateTargetX", "Vx", "Facing", "TargetSlot", "HitStop", "BoundaryFlags",
                "SpecialScanMember",
            })
                arrays.Add(rows.GetType().GetField(field, InstanceMembers).GetValue(rows));
            foreach (string field in new[]
            {
                "MoveModeFirst10Present", "MoveModeFirst10Eligible", "MoveModeFirst10Generation",
                "MoveModeFirst10Hp", "MoveModeFirst10X", "MoveModeFirst10Z",
            })
                arrays.Add(published.GetType().GetField(field, InstanceMembers).GetValue(published));
            return arrays.ToArray();
        }

        private static void SaveSourceCoordinateDriverRows(string name,
            SourceCoordinateDriverRow[] full, SourceCoordinateDriverRow[] candidate, bool thousand)
        {
            SourceCoordinateAdmissionMethod("SaveNew").Invoke(null, new object[]
            {
                name + "-source-coordinate-driver-parity.json",
                new
                {
                    baseline = full, candidate, thousand, performanceEvidence = false,
                    gcScope = thousand ? "full StepOneTick; 16warm+16sample per OFF/ON; calibrated events" : "not measured",
                    checksumEvidenceSerializationOutsideGcScope = true,
                },
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH95-SOURCE-COORDINATE-INCREMENTAL-20261009/driver-qualification-01/",
            });
        }

        private sealed class SourceCoordinateDriverRow
        {
            public int tick;
            public int entities;
            public int actualAi;
            public string extendedHashes;
            public string lockstepHashes;
            public string nativeState;
            public uint legacyState;
            public ulong legacyCalls;
            public string publishedRows;
            public long sourceRefreshes;
            public long aiReads;
            public long sourceRollForward;
            public long incrementalValidations;
            public NTSD.Test.Editor.BattleScopedGcAllocationRecorder.ScopeResult gcScope;
            public NTSD.Test.Editor.BattleScopedGcAllocationRecorder.CalibrationResult gcBefore;
            public NTSD.Test.Editor.BattleScopedGcAllocationRecorder.CalibrationResult gcAfter;
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
                Is.EqualTo(60L * slots.Length),
                "thirty ticks validate both producer and retained routing publications");
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
        public void UnifiedAuthority_LinkedSlotReleaseInvalidatesRowBeforeRelationCleanup()
        {
            var world = new SimulationWorld();
            LF2Character dormantHolder =
                RegisterCharacter(world, 1, 1, 1, 10, 0, 0, 2, false);
            LF2Character released =
                RegisterCharacter(world, 50, 2, 2, 90, 0, 0, 9, false);
            dormantHolder.Runtime.LinkState = 1;
            dormantHolder.Runtime.TargetSlotIndex = 50;
            dormantHolder.Runtime.OidMergeDormant = true;

            const int capacity = 64;
            var included = new bool[capacity];
            var generations = new uint[capacity];
            var integers = new int[capacity];
            var doubles = new double[capacity];
            // A committed AI pass excludes the dormant holder while relation cleanup still visits it.
            world.AiUnifiedRowPublisherForServices.BeginPass(
                1, included, generations, integers, included,
                integers, integers, integers, integers, integers, integers, integers,
                integers, integers, integers, integers, integers, integers, integers,
                doubles, integers, integers, integers, integers, integers, integers);

            Assert.DoesNotThrow(() => world.Unregister(released));
            Assert.That(dormantHolder.Runtime.LinkState, Is.Zero);
            Assert.That(dormantHolder.Runtime.TargetSlotIndex, Is.Zero);
            Assert.That(world.AiUnifiedRowPublisherForServices.Active, Is.False);
        }

        [Test]
        public void UnifiedAuthority_AscendingProducerRefreshMakesLowVisibleToHighWithoutRoutingLeakage()
        {
            var world = new SimulationWorld();
            LF2Character low = RegisterCharacter(world, 0, 1, 1, 10, 0, 0, 2, true);
            LF2Character high = RegisterCharacter(world, 3, 2, 2, 90, 0, 0, 9, true);
            RegisterCharacter(world, 7, 3, 1, 180, 0, 0, 2, false);
            ConfigureGateBWorld(world, unifiedAuthority: true, 0xB17Au);
            world.AiDecisionShadowMode = AiDecisionShadowMode.SharedShadow;
            world.SetCharacterInputProducerPassMutationOverrideForSelfCheck((_, entity) =>
            {
                if (entity?.Runtime?.SlotIndex == 0 && entity.Frame?.D != null)
                    entity.Frame.D.state = 14;
            });
            world.SetAiUnifiedSnapshotExecutionVisibilityProbeForSelfCheck(0, 3, 3, 0);

            LF2FrameData lowProducerFrame = low.Frame.D;
            int originalLowProducerFrameState = lowProducerFrame.state;
            try
            {
                world.CharacterInputAll(2);

                Assert.That(lowProducerFrame.state, Is.EqualTo(14));
                Assert.That(world.AiUnifiedSnapshotExecutionProbeStateAForTests,
                    Is.EqualTo(9),
                    "the low slot must observe the high slot before the later high-slot input");
                Assert.That(world.AiUnifiedSnapshotExecutionProbeStateBForTests,
                    Is.EqualTo(14),
                    "the high slot must observe the low slot's producer-side unified row refresh");
                Assert.That(high.GetState(), Is.EqualTo(9));
            }
            finally
            {
                world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(null);
                lowProducerFrame.state = originalLowProducerFrameState;
            }

            Assert.That(lowProducerFrame.state, Is.EqualTo(originalLowProducerFrameState));
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
                Is.EqualTo(2 * slots.Length));
            Assert.That(actual.AiDecisionSharedRefreshCountForDiagnostics,
                Is.EqualTo(2 * slots.Length));
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

        private static void AssertNativeRandomScalarStateEqual(
            NTSD28NativeRandomScalarState expected,
            NTSD28NativeRandomScalarState actual,
            string message)
        {
            Assert.That(actual.SynchronizedCounter,
                Is.EqualTo(expected.SynchronizedCounter), message);
            Assert.That(actual.SynchronizedIndex,
                Is.EqualTo(expected.SynchronizedIndex), message);
            Assert.That(actual.SynchronizedCalls,
                Is.EqualTo(expected.SynchronizedCalls), message);
            Assert.That(actual.LastSynchronizedCallSite,
                Is.EqualTo(expected.LastSynchronizedCallSite), message);
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
