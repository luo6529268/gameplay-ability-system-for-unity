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
    public sealed class AiSensingSoACandidateEditorTests
    {
        private const BindingFlags InstanceMembers =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        [Test]
        public void Candidate_PublicModeFailsFast_AndInternalSelfCheckGateOptsIn()
        {
            var world = new SimulationWorld();
            Assert.That(
                () => world.AiSensingMode = AiSensingMode.SoAAiSensing,
                Throws.TypeOf<NotSupportedException>());
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.LegacyAiSensing));

            EnableCandidate(world);
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
            Invoke(world, "SetAiSoACandidateFailureForSelfCheck", true, true);
            Invoke(world, "SetAiSoACandidateModeForSelfCheck", false);
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.LegacyAiSensing));
            Assert.That(GetBooleanField(
                world,
                "aiSoACandidateForceNearestFailureForSelfCheck"), Is.False);
            Assert.That(GetBooleanField(
                world,
                "aiSoACandidateForceSpecialFailureForSelfCheck"), Is.False);
        }

        [Test]
        public void Candidate_SnapshotAdapterPublishesPrimitiveKernelEpoch()
        {
            SimulationWorld world = CreateCacheWorld(out _);
            EnableCandidate(world);
            Invoke(world, "BuildAiInputSlotSnapshot");
            try
            {
                FieldInfo rowsField = typeof(SimulationWorld).GetField(
                    "aiSoASensingRows",
                    InstanceMembers);
                FieldInfo epochField = typeof(SimulationWorld).GetField(
                    "aiSoASensingSnapshotEpoch",
                    InstanceMembers);
                Assert.That(rowsField, Is.Not.Null);
                Assert.That(epochField, Is.Not.Null);
                var snapshot = rowsField.GetValue(world) as AiSensingSnapshot;
                Assert.That(snapshot, Is.Not.Null);
                Assert.That(
                    snapshot.CapturedOccupancyEpoch,
                    Is.EqualTo((ulong)epochField.GetValue(world)));
                Assert.That(snapshot.Included, Is.TypeOf<bool[]>());
                Assert.That(snapshot.Generation, Is.TypeOf<uint[]>());
                Assert.That(snapshot.Identity, Is.TypeOf<int[]>());
            }
            finally
            {
                Invoke(world, "ClearAiInputSlotSnapshot");
            }
        }

        [TestCase(false, false, 2)]
        [TestCase(true, false, 20)]
        [TestCase(true, true, 2)]
        public void Candidate_NormalSpecialScenarios_MatchIndependentLegacy(
            bool includeD5,
            bool includeC8Threat,
            int expectedTargetSlot)
        {
            SimulationWorld legacy = CreateSpecialWorld(
                includeD5,
                includeC8Threat,
                out LF2Character legacySelf);
            SimulationWorld candidate = CreateSpecialWorld(
                includeD5,
                includeC8Threat,
                out LF2Character candidateSelf);
            legacy.Rng.Seed(0x5EEDu);
            candidate.Rng.Seed(0x5EEDu);
            EnableCandidate(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(expectedTargetSlot));
            Assert.That(candidate.AiSoACandidateNearestQueryCountForDiagnostics, Is.EqualTo(1));
            Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics, Is.EqualTo(1));
            AssertNoLegacyCandidateScans(candidate);
        }

        [Test]
        public void Candidate_QueryDiagnostics_RecordNestedPhasesAndRowVisits_ThenReset()
        {
            SimulationWorld candidate = CreateSpecialWorld(
                includeD5: true,
                includeC8Threat: false,
                out _);
            candidate.Rng.Seed(0x5EEDu);
            EnableCandidate(candidate);
            BattleAiInputDetailDiagnostics timing =
                candidate.EnableBattleAiInputDetailDiagnosticsForDiagnostics();

            candidate.CharacterInputAll(2);

            Assert.That(
                BattleAiInputDetailDiagnostics.PhaseCount,
                Is.EqualTo((int)BattleAiInputDetailPhase.Count));
            Assert.That(
                BattleAiInputDetailDiagnostics.GetPhaseName(
                    BattleAiInputDetailPhase.CandidateNearest),
                Is.EqualTo("CharacterInput/AI/RemainingAiDecision/CandidateNearest"));
            Assert.That(
                BattleAiInputDetailDiagnostics.GetPhaseName(
                    BattleAiInputDetailPhase.CandidateSpecial),
                Is.EqualTo("CharacterInput/AI/RemainingAiDecision/CandidateSpecial"));
            Assert.That(
                timing.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.CandidateNearest),
                Is.GreaterThan(0));
            Assert.That(
                timing.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.CandidateSpecial),
                Is.GreaterThan(0));
            Assert.That(
                timing.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.RemainingAiDecision),
                Is.GreaterThanOrEqualTo(
                    timing.GetLastElapsedTimestampTicks(
                        BattleAiInputDetailPhase.CandidateNearest) +
                    timing.GetLastElapsedTimestampTicks(
                        BattleAiInputDetailPhase.CandidateSpecial)));
            Assert.That(candidate.AiSoACandidateGroundXRowVisitCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(candidate.AiSoACandidateAirXRowVisitCountForDiagnostics,
                Is.Zero,
                "The state-9 self skips the air query under the authority logic.");
            Assert.That(candidate.AiSoACandidateSpecialSlotVisitCountForDiagnostics,
                Is.GreaterThan(0));

            candidate.ResetAiSoACandidateDiagnostics();

            Assert.That(candidate.AiSoACandidateNearestQueryCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics, Is.Zero);
            Assert.That(
                candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoACandidateGroundXRowVisitCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoACandidateAirXRowVisitCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoACandidateSpecialSlotVisitCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void Candidate_EmptySpecialFastPath_ReturnsIdentityFactsWithoutRngOrFallback()
        {
            SimulationWorld candidate = CreateCacheWorld(out LF2Character self);
            candidate.Rng.Seed(0xE501u);
            EnableCandidate(candidate);
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                uint rngStateBefore = candidate.Rng.State;
                ulong rngCallsBefore = candidate.Rng.CallCount;

                Assert.That(
                    CaptureAiSoACandidateSpecial(
                        candidate,
                        self,
                        inputPhase: 2,
                        initialSelectedSlot: 2,
                        nearestBestDist: 40,
                        sameZLane: true,
                        out int selectedSlot,
                        out int bestDist,
                        out bool sameZLane,
                        out int flags),
                    Is.True);

                Assert.That(selectedSlot, Is.EqualTo(2));
                Assert.That(bestDist, Is.EqualTo(10000));
                Assert.That(sameZLane, Is.True);
                Assert.That(flags, Is.Zero);
                Assert.That(candidate.Rng.State, Is.EqualTo(rngStateBefore));
                Assert.That(candidate.Rng.CallCount, Is.EqualTo(rngCallsBefore));
                Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(
                    candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoACandidateSpecialSlotVisitCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidatePreRandomFailureCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidatePostRandomFailureCountForDiagnostics,
                    Is.Zero);
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void Candidate_EmptySpecialGuardTrue_EndToEndMatchesLegacy()
        {
            SimulationWorld legacy = CreateEmptySpecialGuardParityWorld(
                out LF2Character legacySelf);
            SimulationWorld candidate = CreateEmptySpecialGuardParityWorld(
                out LF2Character candidateSelf);
            legacy.Rng.Seed(0xE502u);
            candidate.Rng.Seed(0xE502u);
            EnableCandidate(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            Assert.That(candidateSelf.Runtime.Unk360,
                Is.EqualTo(legacySelf.Runtime.Unk360));
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(2));
            Assert.That(InputChecksum(candidateSelf.Runtime),
                Is.EqualTo(InputChecksum(legacySelf.Runtime)));
            Assert.That(candidate.Rng.State, Is.EqualTo(legacy.Rng.State));
            Assert.That(candidate.Rng.CallCount, Is.EqualTo(legacy.Rng.CallCount));
            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);

            Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSoACandidateSpecialSlotVisitCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoACandidatePreRandomFailureCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoACandidatePostRandomFailureCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void Candidate_EmptySpecialFastPath_SelectedGenerationDriftFailsClosed()
        {
            SimulationWorld candidate = CreateCacheWorld(out LF2Character self);
            EnableCandidate(candidate);
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                IncrementAiSoARowGeneration(candidate, 2);

                Assert.That(
                    CaptureAiSoACandidateSpecial(
                        candidate,
                        self,
                        inputPhase: 2,
                        initialSelectedSlot: 2,
                        nearestBestDist: 40,
                        sameZLane: true,
                        out _,
                        out _,
                        out _,
                        out _),
                    Is.False);
                Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics,
                    Is.Zero);
                Assert.That(
                    candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                    Is.Zero);
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void Candidate_EmptySpecialForceFull_RetainsCompleteGuardPath()
        {
            SimulationWorld candidate = CreateEmptySpecialGuardWorld(out LF2Character self);
            EnableCandidate(candidate);
            SetProperty(candidate, "ForceFullAiSpecialScanForDiagnostics", true);
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                Assert.That(
                    CaptureAiSoACandidateSpecial(
                        candidate,
                        self,
                        inputPhase: 1,
                        initialSelectedSlot: 2,
                        nearestBestDist: 50,
                        sameZLane: true,
                        out int selectedSlot,
                        out int bestDist,
                        out bool sameZLane,
                        out int flags),
                    Is.True);

                Assert.That(selectedSlot, Is.EqualTo(2));
                Assert.That(bestDist, Is.EqualTo(10000));
                Assert.That(sameZLane, Is.True);
                Assert.That(flags & (1 << 5), Is.Not.Zero, "guard7A");
                Assert.That(flags & (1 << 6), Is.Not.Zero, "guard7B");
                Assert.That(
                    candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidateSpecialSlotVisitCountForDiagnostics,
                    Is.GreaterThan(0));
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void Candidate_NonEmptySpecialList_RetainsCompleteSelectionPath()
        {
            SimulationWorld candidate = CreateSpecialWorld(
                includeD5: true,
                includeC8Threat: false,
                out LF2Character self);
            EnableCandidate(candidate);
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                Assert.That(
                    CaptureAiSoACandidateSpecial(
                        candidate,
                        self,
                        inputPhase: 2,
                        initialSelectedSlot: 2,
                        nearestBestDist: 150,
                        sameZLane: false,
                        out int selectedSlot,
                        out int bestDist,
                        out _,
                        out _),
                    Is.True);

                Assert.That(selectedSlot, Is.EqualTo(20));
                Assert.That(bestDist, Is.EqualTo(20));
                Assert.That(
                    candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidateSpecialSlotVisitCountForDiagnostics,
                    Is.EqualTo(1));
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void Shadow_EmptySpecialList_ComputesGuardFlagsAndComparesCleanly()
        {
            SimulationWorld shadow = CreateEmptySpecialGuardWorld(out LF2Character self);
            shadow.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            int flags = CaptureAiSoAShadowSpecialFlags(shadow, self, inputPhase: 1);

            Assert.That(flags & (1 << 5), Is.Not.Zero, "guard7A");
            Assert.That(flags & (1 << 6), Is.Not.Zero, "guard7B");
            Assert.That(
                shadow.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                Is.Zero);

            shadow.CharacterInputAll(2);

            Assert.That(
                shadow.AiSoASensingShadowComparisonPublishedForDiagnostics,
                Is.True);
            Assert.That(shadow.AiSoASensingShadowMismatchMaskForDiagnostics, Is.Zero);
        }

        [TestCase(16, 3)]
        [TestCase(1, 2)]
        public void Candidate_CacheRollZeroAndNonZero_UsesTheSingleCommonRngWriter(
            int seed,
            int expectedTargetSlot)
        {
            SimulationWorld legacy = CreateCacheWorld(out LF2Character legacySelf);
            SimulationWorld candidate = CreateCacheWorld(out LF2Character candidateSelf);
            legacy.Rng.Seed((uint)seed);
            candidate.Rng.Seed((uint)seed);
            EnableCandidate(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(expectedTargetSlot));
            Assert.That(candidate.Rng.CallCount, Is.GreaterThan(0));
            AssertNoLegacyCandidateScans(candidate);
        }

        [Test]
        public void DecisionRemainder_DefaultDisabled_PerformsZeroRowReads()
        {
            SimulationWorld candidate = CreateCacheWorld(out _);
            EnableCandidate(candidate);

            candidate.CharacterInputAll(2);

            Assert.That(candidate.AiSoADecisionRemainderEnabledForDiagnostics, Is.False);
            Assert.That(candidate.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderPreRandomFailureCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderPostRandomFailureCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderHardFailureCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderContextBindCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderGatewayValidationCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderRowVisitCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void DecisionRemainder_AccessorShortCircuitsUntilBound_ThenUsesSnapshotAndFallsBack()
        {
            SimulationWorld candidate = CreateCacheWorld(out LF2Character self);
            EnableCandidate(candidate);

            Assert.That(
                TryGetDecisionRemainderRow(candidate, self, out object disabledRows, out int disabledSlot),
                Is.False);
            Assert.That(disabledRows, Is.Null);
            Assert.That(disabledSlot, Is.EqualTo(-1));

            EnableDecisionRemainder(candidate);
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                Assert.That(
                    (bool)Invoke(candidate, "TryBindAiSoADecisionRowContext", self, 2, -1, null),
                    Is.True);
                int snapshotX = (int)Invoke(candidate, "X", self);

                SetPosition(self, snapshotX + 777, self.Runtime.YInt, self.Runtime.ZInt);

                Assert.That(
                    TryGetDecisionRemainderRow(candidate, self, out object boundRows, out int boundSlot),
                    Is.True);
                Assert.That(boundRows, Is.Not.Null);
                Assert.That(boundSlot, Is.EqualTo(self.Runtime.SlotIndex));
                Assert.That((int)Invoke(candidate, "X", self), Is.EqualTo(snapshotX),
                    "A bound decision context must continue reading the published SoA row.");

                Invoke(candidate, "CompleteAiSoADecisionRemainderInput");

                Assert.That(
                    TryGetDecisionRemainderRow(candidate, self, out object releasedRows, out int releasedSlot),
                    Is.False);
                Assert.That(releasedRows, Is.Null);
                Assert.That(releasedSlot, Is.EqualTo(-1));
                Assert.That((int)Invoke(candidate, "X", self),
                    Is.EqualTo(snapshotX + 777),
                    "Once the context is released, the accessor must fall back to the live runtime value.");
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void DecisionRemainder_ContextBindsOnce_AndUsesTwoConstantCostGateways()
        {
            SimulationWorld legacy = CreateCacheWorld(out LF2Character legacySelf);
            SimulationWorld candidate = CreateCacheWorld(out LF2Character candidateSelf);
            legacy.Rng.Seed(0x70C1u);
            candidate.Rng.Seed(0x70C1u);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidate.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics, Is.EqualTo(1));
            Assert.That(candidate.AiSoADecisionRemainderContextBindCountForDiagnostics, Is.EqualTo(1));
            Assert.That(candidate.AiSoADecisionRemainderGatewayValidationCountForDiagnostics, Is.EqualTo(2));
            Assert.That(candidate.AiSoADecisionRemainderRowVisitCountForDiagnostics, Is.EqualTo(6));
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderHardFailureCountForDiagnostics, Is.Zero);
        }

        [TestCase("dead")]
        [TestCase("coordinate")]
        public void DecisionRemainder_IneligibleCharacterInput_DoesNotCountAttempt(
            string condition)
        {
            SimulationWorld candidate = CreateCacheWorld(out LF2Character self);
            if (condition == "dead")
                self.Runtime.HP = 0;
            else
                self.Runtime.Unk3FC = 0;
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);

            candidate.CharacterInputAll(2);

            Assert.That(candidate.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderHardFailureCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void DecisionRemainder_DatChangeAndNonCharacter_DoNotCountAttempt()
        {
            var candidate = new SimulationWorld();
            candidate.Runtime.Flow.InputPhase = 2;
            var mutable = new MutableDatShell
            {
                CurrentDataObjectType = (int)LF2ObjectType.Character,
            };
            InitializeEntity(mutable, 0, 1, 1, 0, 0, 0, 9, 500, 0);
            mutable.AiControlled = true;
            candidate.Register(mutable);
            LF2Entity nonCharacter = RegisterOther(
                candidate, 1, 1, 1, 20, 0, 0, 9, 500);
            nonCharacter.AiControlled = true;
            RegisterCharacter(candidate, 2, 1, 2, 80, 0, 0, 0, 500, false);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);

            candidate.CharacterInputAll(2);
            Assert.That(candidate.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics,
                Is.EqualTo(1));

            candidate.ResetAiSoACandidateDiagnostics();
            mutable.CurrentDataObjectType = (int)LF2ObjectType.Other;
            candidate.CharacterInputAll(3);

            Assert.That(candidate.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderHardFailureCountForDiagnostics, Is.Zero);
        }

        [TestCase(16, 3)]
        [TestCase(1, 2)]
        public void DecisionRemainder_CachedRetainAndDrop_MatchLegacyRngAndOutput(
            int seed,
            int expectedTargetSlot)
        {
            SimulationWorld legacy = CreateCacheWorld(out LF2Character legacySelf);
            SimulationWorld candidate = CreateCacheWorld(out LF2Character candidateSelf);
            legacy.Rng.Seed((uint)seed);
            candidate.Rng.Seed((uint)seed);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(expectedTargetSlot));
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderRowVisitCountForDiagnostics,
                Is.EqualTo(6));
            Assert.That(candidate.AiSoADecisionRemainderGatewayValidationCountForDiagnostics,
                Is.EqualTo(2));
        }

        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        [TestCase(4, false)]
        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        public void DecisionRemainder_ContextMutation_UsesPreRandomFallbackOrPostRandomHardFailure(
            int mutationKind,
            bool afterRandom)
        {
            SimulationWorld legacy = CreateCacheWorld(out LF2Character legacySelf);
            SimulationWorld candidate = CreateCacheWorld(out LF2Character candidateSelf);
            legacy.Rng.Seed(0x51A7u);
            candidate.Rng.Seed(0x51A7u);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);
            Invoke(
                candidate,
                "SetAiSoADecisionRemainderMutationForSelfCheck",
                mutationKind,
                afterRandom);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidate.Rng.CallCount, Is.GreaterThan(0));
            Assert.That(candidate.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics,
                Is.EqualTo(afterRandom ? 0 : 1));
            Assert.That(candidate.AiSoADecisionRemainderPreRandomFailureCountForDiagnostics,
                Is.EqualTo(afterRandom ? 0 : 1));
            Assert.That(candidate.AiSoADecisionRemainderPostRandomFailureCountForDiagnostics,
                Is.EqualTo(afterRandom ? 1 : 0));
            Assert.That(candidate.AiSoADecisionRemainderHardFailureCountForDiagnostics,
                Is.EqualTo(afterRandom ? 1 : 0));
            Assert.That(candidate.AiSoADecisionRemainderContextBindCountForDiagnostics,
                Is.EqualTo(afterRandom ? 1 : 0));
            Assert.That(candidate.AiSoADecisionRemainderGatewayValidationCountForDiagnostics,
                Is.EqualTo(afterRandom ? 2 : 1));
            Assert.That(candidate.AiSoACandidateLegacyNearestScanCountForDiagnostics,
                Is.EqualTo(afterRandom ? 0 : 1));
            Assert.That(candidate.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                Is.EqualTo(afterRandom ? 0 : 1));
        }

        [Test]
        public void DecisionRemainder_NoTarget_ContextMoveModeAndPostPathRemainAuthoritative()
        {
            var legacy = new SimulationWorld();
            var candidate = new SimulationWorld();
            legacy.Runtime.Flow.InputPhase = 1;
            candidate.Runtime.Flow.InputPhase = 1;
            LF2Character legacySelf = RegisterCharacter(
                legacy, 20, 31, 1, 650, 0, 0, 9, 500, true);
            LF2Character candidateSelf = RegisterCharacter(
                candidate, 20, 31, 1, 650, 0, 0, 9, 500, true);
            RegisterCharacter(legacy, 0, 1, 1, 0, 0, 0, 0, 500, false);
            RegisterCharacter(candidate, 0, 1, 1, 0, 0, 0, 0, 500, false);
            legacy.Rng.Seed(0xA110u);
            candidate.Rng.Seed(0xA110u);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(-1));
            Assert.That(candidate.Runtime.Flow.AiMoveMode,
                Is.EqualTo(legacy.Runtime.Flow.AiMoveMode));
            Assert.That(candidate.Runtime.Flow.AiMoveMode, Is.EqualTo(2));
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void DecisionRemainder_ForeignRuntimeFactsRemainReadOnly()
        {
            SimulationWorld candidate = CreateCacheWorld(out LF2Character self);
            LF2Entity foreign = candidate.FindEntityByRuntimeSlotIncludingPending(2);
            ulong before = ForeignFactChecksum(foreign);
            candidate.Rng.Seed(0xF0E1u);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);

            candidate.CharacterInputAll(2);

            Assert.That(ForeignFactChecksum(foreign), Is.EqualTo(before));
            Assert.That(candidate.AiSoADecisionRemainderAppliedCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics,
                Is.Zero);
            Assert.That(self.Runtime.SlotIndex, Is.EqualTo(0));
        }

        [Test]
        public void Candidate_NormalPassSkipsLegacySnapshotProducts_WhileLegacyAndShadowRetainThem()
        {
            SimulationWorld candidate = CreateCacheWorld(out _);
            SimulationWorld legacy = CreateCacheWorld(out _);
            SimulationWorld shadow = CreateCacheWorld(out _);
            candidate.Rng.Seed(0xF00Du);
            legacy.Rng.Seed(0xF00Du);
            shadow.Rng.Seed(0xF00Du);

            EnableCandidate(candidate);
            candidate.CharacterInputAll(2);
            Assert.That(candidate.AiLegacyNearestFactsBuildCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiLegacySnapshotIndexBuildCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiLegacyQuadtreeSyncCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiLegacySnapshotMutationCountForDiagnostics, Is.Zero);

            legacy.ResetAiSoACandidateDiagnostics();
            legacy.CharacterInputAll(2);
            AssertLegacySnapshotProductsWereBuilt(legacy);

            shadow.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            shadow.ResetAiSoACandidateDiagnostics();
            shadow.CharacterInputAll(2);
            AssertLegacySnapshotProductsWereBuilt(shadow);
        }

        [Test]
        public void Candidate_FusedSnapshot_ClaimedInactiveSlotsAreSkippedWithoutInvalidation()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            LF2Character dormant = RegisterCharacter(
                world, 2, 1, 2, 5, 0, 0, 0, 500, false);
            LF2Character pendingDestroy = RegisterCharacter(
                world, 3, 1, 2, 10, 0, 0, 0, 500, false);
            RegisterCharacter(world, 4, 1, 2, 20, 0, 0, 0, 500, false);
            dormant.Runtime.OidMergeDormant = true;
            pendingDestroy.Runtime.PendingFlushDestroy = true;
            EnableCandidate(world);

            Invoke(world, "BuildAiInputSlotSnapshot");
            try
            {
                LF2Entity[] slots = GetAiInputSlots(world);
                bool[] included = GetAiSoAIncludedRows(world);
                Assert.That(slots[0], Is.SameAs(self));
                Assert.That(slots[2], Is.Null);
                Assert.That(slots[3], Is.Null);
                Assert.That(slots[4], Is.Not.Null);
                Assert.That(included[0], Is.True);
                Assert.That(included[2], Is.False);
                Assert.That(included[3], Is.False);
                Assert.That(included[4], Is.True);
                Assert.That(GetBooleanField(world, "aiSoASensingSnapshotValid"), Is.True);
                Assert.That(GetBooleanField(world, "aiSoASensingPassInvalidated"), Is.False);
                Assert.That(
                    world.AiSoACandidateFusedSnapshotFailureCountForDiagnostics,
                    Is.Zero);
                Assert.That(
                    world.AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics,
                    Is.EqualTo(world.RuntimeSlotCapacityForDiagnostics));
            }
            finally
            {
                Invoke(world, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void Candidate_FusedSnapshot_DesktopGrowCapturesHighSlotAndMatchesLegacy()
        {
            const int highSlot = 600;
            var legacy = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 64);
            var candidate = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 64);
            LF2Character legacySelf = RegisterCharacter(
                legacy, 0, 1, 1, 0, 0, 0, 9, 500, true);
            LF2Character candidateSelf = RegisterCharacter(
                candidate, 0, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(
                legacy, highSlot, 1, 2, 25, 0, 0, 0, 500, false);
            RegisterCharacter(
                candidate, highSlot, 1, 2, 25, 0, 0, 0, 500, false);
            legacy.Runtime.Flow.InputPhase = 2;
            candidate.Runtime.Flow.InputPhase = 2;
            legacy.Rng.Seed(0xD35Cu);
            candidate.Rng.Seed(0xD35Cu);
            EnableCandidate(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            Assert.That(candidate.RuntimeSlotCapacityForDiagnostics,
                Is.GreaterThan(highSlot));
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(highSlot));
            Assert.That(candidateSelf.Runtime.Unk360,
                Is.EqualTo(legacySelf.Runtime.Unk360));
            Assert.That(InputChecksum(candidateSelf.Runtime),
                Is.EqualTo(InputChecksum(legacySelf.Runtime)));
            Assert.That(candidate.Rng.State, Is.EqualTo(legacy.Rng.State));
            Assert.That(candidate.Rng.CallCount, Is.EqualTo(legacy.Rng.CallCount));
            Assert.That(
                candidate.AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics,
                Is.EqualTo(candidate.RuntimeSlotCapacityForDiagnostics));
            Assert.That(
                candidate.AiSoACandidateFusedSnapshotFailureCountForDiagnostics,
                Is.Zero);
            AssertNoLegacyCandidateScans(candidate);
        }

        [Test]
        public void Candidate_PreFallback_PreservesEqualSlotTieAndAirOverrideGroundFacts()
        {
            SimulationWorld legacy = CreateAirOverrideWorld(out LF2Character legacySelf);
            SimulationWorld candidate = CreateAirOverrideWorld(out LF2Character candidateSelf);
            legacy.Rng.Seed(0x4A11u);
            candidate.Rng.Seed(0x4A11u);
            EnableCandidate(candidate);
            Invoke(candidate, "SetAiSoACandidateFailureForSelfCheck", true, false);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(4),
                "the eligible air target overrides the selected ground slot, while the\n" +
                "ground distance/lane facts remain the values consumed by later AI work");
            Assert.That(candidate.AiSoACandidatePreRandomFailureCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSoACandidateLegacyNearestScanCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void Candidate_SpatialXIndex_PreservesUnorderedNegativeTieAndAirOverrideParity()
        {
            SimulationWorld legacy = CreateSpatialIndexWorld(out LF2Character legacySelf);
            SimulationWorld candidate = CreateSpatialIndexWorld(out LF2Character candidateSelf);
            legacy.Rng.Seed(0x51A7u);
            candidate.Rng.Seed(0x51A7u);
            EnableCandidate(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(4),
                "the in-range air candidate still overrides an equally-distant ground tie");
            AssertNoLegacyCandidateScans(candidate);
            Assert.That(candidate.AiLegacyNearestFactsBuildCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiLegacySnapshotIndexBuildCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiLegacyQuadtreeSyncCountForDiagnostics, Is.Zero);
            Assert.That(candidate.AiLegacySnapshotMutationCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void Candidate_SpatialXIndex_State3000DirectionMatchesLegacyWithoutFallback()
        {
            SimulationWorld legacy = CreateState3000World(-0.01, out LF2Character legacySelf);
            SimulationWorld candidate = CreateState3000World(-0.01, out LF2Character candidateSelf);
            legacy.Rng.Seed(0x3010u);
            candidate.Rng.Seed(0x3010u);
            EnableCandidate(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(2));
            AssertNoLegacyCandidateScans(candidate);
        }

        [TestCase(1, 1, 5, 2)]
        [TestCase(1, 1, 77, -1)]
        [TestCase(1, 5, 77, 2)]
        [TestCase(1, 5, 5, -1)]
        [TestCase(1, -7, 5, 2)]
        [TestCase(1, -7, 77, -1)]
        [TestCase(2, 1, 77, 2)]
        [TestCase(2, -7, 77, 2)]
        [TestCase(4, 77, 5, 2)]
        [TestCase(4, 77, -7, 2)]
        [TestCase(2, 77, 77, -1)]
        [TestCase(4, -7, -7, -1)]
        public void Candidate_RoleTeamGuard_UsesExactAuthorityPredicate(
            int inputPhase,
            int selfTeam,
            int candidateTeam,
            int expectedSlot)
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 0, 1, selfTeam, 0, 0, 0, 9, 500, true);
            RegisterCharacter(
                world, 2, 1, candidateTeam, 10, 0, 0, 0, 500, false);

            CaptureAndAssertNearestDifferential(
                world,
                self,
                inputPhase,
                expectedSlot,
                $"phase={inputPhase}, selfTeam={selfTeam}, candidateTeam={candidateTeam}");
        }

        [Test]
        public void Candidate_RoleTeamSpans_SupportArbitraryTeamsAndSortByXThenSlot()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 10, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 7, 1, 77, -20, 0, 0, 0, 500, false);
            RegisterCharacter(world, 2, 1, 77, -20, 0, 0, 0, 500, false);
            RegisterCharacter(world, 4, 1, 77, -40, 0, 0, 0, 500, false);
            RegisterCharacter(world, 3, 1, -7, -10, 0, 0, 0, 500, false);
            RegisterCharacter(world, 5, 1, 5, -30, 0, 0, 0, 500, false);

            EnableCandidate(world);
            Invoke(world, "BuildAiInputSlotSnapshot");
            try
            {
                CollectionAssert.AreEqual(
                    new[] { "-7:3", "1:10", "5:5", "77:4,2,7" },
                    GetAiSoARoleTeamSpanKeys(world, "Ground"));
            }
            finally
            {
                Invoke(world, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void Candidate_RoleTeamSpans_MergeEqualDistanceAcrossAllowedTeamsByLowerSlot()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 10, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 7, 1, -7, -10, 0, 0, 0, 500, false);
            RegisterCharacter(world, 2, 1, 77, 10, 0, 0, 0, 500, false);

            CaptureAndAssertNearestDifferential(
                world,
                self,
                inputPhase: 2,
                expectedSlot: 2,
                context: "equal-distance candidates in different allowed team spans");
        }

        [Test]
        public void Candidate_RoleIndexes_UseExactGroundAndAirSupersets()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 2, 1, 2, 20, 2, 0, 0, 1, false);
            RegisterCharacter(world, 3, 1, 2, 30, 3, 0, 0, 1, false);
            RegisterCharacter(world, 4, 1, 2, 40, 0, 0, 14, 1, false);
            RegisterCharacter(world, 5, 1, 2, 50, 0, 0, 0, 0, false);
            RegisterOther(world, 6, 900, 2, 60, 0, 0, 3000, 1);
            RegisterOther(world, 7, 900, 2, 70, 0, 0, 0, 1);
            RegisterOther(world, 8, 900, 2, 80, 3, 0, 3000, 1);

            EnableCandidate(world);
            Invoke(world, "BuildAiInputSlotSnapshot");
            try
            {
                CollectionAssert.AreEqual(
                    new[] { 0, 2, 6 },
                    GetAiSoARoleSlots(world, "GroundRoleSlotsByX", "GroundRoleSlotCount"));
                CollectionAssert.AreEqual(
                    new[] { 3, 4, 8 },
                    GetAiSoARoleSlots(world, "AirRoleSlotsByX", "AirRoleSlotCount"));
            }
            finally
            {
                Invoke(world, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void Candidate_AirOverrideAndState9_PreserveGroundDerivedFacts()
        {
            SimulationWorld world = CreateAirOverrideWorld(out LF2Character self);

            Assert.That(
                CaptureAiSoANearest(
                    world, self, 2, out int selected, out int bestDist, out bool sameZLane),
                Is.True);
            Assert.That(selected, Is.EqualTo(4));
            Assert.That(bestDist, Is.EqualTo(10),
                "air override must not replace the ground-derived best distance");
            Assert.That(sameZLane, Is.True,
                "air override must not replace the ground-derived lane fact");

            SetState(self, 9);
            Assert.That(
                CaptureAiSoANearest(
                    world, self, 2, out selected, out bestDist, out sameZLane),
                Is.True);
            Assert.That(selected, Is.EqualTo(2), "state 9 skips the air role query");
            Assert.That(bestDist, Is.EqualTo(10));
            Assert.That(sameZLane, Is.True);
        }

        [TestCase("team", 0)]
        [TestCase("hp", 0)]
        [TestCase("state", 0)]
        [TestCase("y", 0)]
        [TestCase("data_type", 0)]
        [TestCase("state3000", 0)]
        [TestCase("ground_to_air", 2)]
        [TestCase("death", 2)]
        public void Candidate_RoleProducts_RefreshAfterEligibilityMutation(
            string mutation,
            int expectedSlot)
        {
            SimulationWorld legacy = CreateRoleRefreshWorld(
                mutation, out LF2Entity legacyTarget, out LF2Character legacySelf);
            SimulationWorld candidate = CreateRoleRefreshWorld(
                mutation, out LF2Entity candidateTarget, out LF2Character candidateSelf);
            legacy.Rng.Seed(0xB011u);
            candidate.Rng.Seed(0xB011u);
            EnableCandidate(candidate);
            SetProperty(legacy, "ForceFullAiNearestScanForDiagnostics", true);

            Invoke(legacy, "BuildAiInputSlotSnapshot");
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                ApplyRoleEligibilityMutation(legacyTarget, mutation);
                ApplyRoleEligibilityMutation(candidateTarget, mutation);
                Invoke(
                    candidate,
                    "RefreshAiSoASensingShadowRowAfterCharacterInput",
                    candidateTarget);

                AssertPrepareSegmentParity(
                    candidate,
                    candidateSelf,
                    legacy,
                    legacySelf,
                    2);
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
                Invoke(legacy, "ClearAiInputSlotSnapshot");
            }

            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(expectedSlot), mutation);
            AssertNoLegacyCandidateScans(candidate);
        }

        [Test]
        public void Candidate_RoleIndexes_DeterministicDifferentialFuzzMatchesLegacyFullScan()
        {
            const int seed = 0x5A17;
            var random = new Random(seed);
            int[] teams = { -3, 1, 2, 5, 77 };
            int[] states = { 0, 9, 14, 3000 };
            int[] ys = { -4, -2, 0, 2, 4 };
            int[] hps = { 0, 1, 500 };

            for (int caseIndex = 0; caseIndex < 48; caseIndex++)
            {
                var world = new SimulationWorld();
                int inputPhase = caseIndex % 3 == 0 ? 1 : caseIndex % 3 == 1 ? 2 : 4;
                int selfTeam = teams[random.Next(teams.Length)];
                int selfState = random.Next(2) == 0 ? 0 : 9;
                LF2Character self = RegisterCharacter(
                    world, 0, 1, selfTeam, 0, 0, 0, selfState, 500, true);

                for (int slot = 1; slot <= 32; slot++)
                {
                    int team = teams[random.Next(teams.Length)];
                    int state = states[random.Next(states.Length)];
                    int x = random.Next(-400, 401);
                    int y = ys[random.Next(ys.Length)];
                    int z = random.Next(-80, 81);
                    int hp = hps[random.Next(hps.Length)];
                    LF2Entity target = random.Next(2) == 0
                        ? (LF2Entity)RegisterCharacter(
                            world, slot, 1, team, x, y, z, state, hp, false)
                        : RegisterOther(world, slot, 900, team, x, y, z, state, hp);
                    target.Runtime.Vx = random.Next(3) - 1;
                }

                CaptureAndAssertNearestDifferential(
                    world,
                    self,
                    inputPhase,
                    expectedSlot: null,
                    $"seed={seed}, case={caseIndex}, phase={inputPhase}, selfTeam={selfTeam}");
            }
        }

        [Test]
        public void Candidate_SpatialXIndex_RebuildsWhenEarlierCharacterCrossesRowsBeforeLaterAiQuery()
        {
            SimulationWorld legacy = CreateSpatialRefreshWorld(
                out LF2Character legacyMoved,
                out LF2Character legacySelf);
            SimulationWorld candidate = CreateSpatialRefreshWorld(
                out LF2Character candidateMoved,
                out LF2Character candidateSelf);
            legacy.Rng.Seed(0x58A1u);
            candidate.Rng.Seed(0x58A1u);
            EnableCandidate(candidate);
            SetProperty(legacy, "ForceFullAiNearestScanForDiagnostics", true);

            Invoke(legacy, "BuildAiInputSlotSnapshot");
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                SetPosition(legacyMoved, 10, 0, 0);
                SetPosition(candidateMoved, 10, 0, 0);
                Invoke(
                    candidate,
                    "RefreshAiSoASensingShadowRowAfterCharacterInput",
                    candidateMoved);

                AssertPrepareSegmentParity(
                    candidate,
                    candidateSelf,
                    legacy,
                    legacySelf,
                    2);
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
                Invoke(legacy, "ClearAiInputSlotSnapshot");
            }

            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(0),
                "the lower-slot character moved across the captured X order and must " +
                "be visible to the later AI query");
            AssertNoLegacyCandidateScans(candidate);
            AssertCandidateBuiltNoLegacySnapshotProducts(candidate);
        }

        [Test]
        public void Candidate_RoleTeamSpans_RebuildWhenEarlierSlotMigratesTeams()
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            LF2Character moved = RegisterCharacter(
                world, 0, 1, 1, 10, 0, 0, 0, 500, false);
            RegisterCharacter(world, 2, 1, 2, 80, 0, 0, 0, 500, false);
            LF2Character self = RegisterCharacter(
                world, 10, 1, 1, 0, 0, 0, 9, 500, true);
            EnableCandidate(world);

            Invoke(world, "BuildAiInputSlotSnapshot");
            try
            {
                moved.Team = 77;
                moved.RelationTeam = 77;
                Invoke(world, "RefreshAiSoASensingShadowRowAfterCharacterInput", moved);

                Invoke(world, "PrepareAiInputBasic", self, 2);
                Assert.That(self.Runtime.Unk360, Is.EqualTo(0));
                CollectionAssert.Contains(
                    GetAiSoARoleTeamSpanKeys(world, "Ground"),
                    "77:0");
            }
            finally
            {
                Invoke(world, "ClearAiInputSlotSnapshot");
            }

            AssertNoLegacyCandidateScans(world);
        }

        [TestCase("unique_min_excluded", 2, 300)]
        [TestCase("duplicate_min", 2, 100)]
        [TestCase("self_non_min", 1, 100)]
        [TestCase("only_member", 0, int.MaxValue)]
        public void Candidate_TeamAggregate_ExcludesSelfWithExactMinSemantics(
            string aggregateCase,
            int expectedOtherCount,
            int expectedOtherMinHp)
        {
            SimulationWorld candidate = CreateTeamAggregateWorld(
                aggregateCase,
                out LF2Character candidateSelf);
            candidate.Rng.Seed(0x7EA1u);
            EnableCandidate(candidate);

            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                GetAiSoASameTeamSummaryExcludingSelf(
                    candidate,
                    candidateSelf.Runtime.SlotIndex,
                    candidateSelf.Team,
                    out int otherCount,
                    out int otherMinHp);
                Assert.That(otherCount, Is.EqualTo(expectedOtherCount));
                Assert.That(otherMinHp, Is.EqualTo(expectedOtherMinHp));
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
            }

            AssertNoLegacyCandidateScans(candidate);
            AssertCandidateBuiltNoLegacySnapshotProducts(candidate);
        }

        [TestCase(249, 3)]
        [TestCase(250, 2)]
        public void Candidate_AirAbsoluteXBoundary_MatchesLegacyWithoutFallback(
            int airAbsoluteX,
            int expectedTargetSlot)
        {
            SimulationWorld legacy = CreateAirBoundaryWorld(
                airAbsoluteX,
                out LF2Character legacySelf);
            SimulationWorld candidate = CreateAirBoundaryWorld(
                airAbsoluteX,
                out LF2Character candidateSelf);
            legacy.Rng.Seed(0xA125u);
            candidate.Rng.Seed(0xA125u);
            EnableCandidate(candidate);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(expectedTargetSlot),
                "air targets require abs(X delta) < 250 exactly");
            AssertNoLegacyCandidateScans(candidate);
            AssertCandidateBuiltNoLegacySnapshotProducts(candidate);
        }

        [TestCase(39, 3)]
        [TestCase(40, 2)]
        public void Candidate_AirAbsoluteZBoundary_MatchesLegacyWithoutFallback(
            int airAbsoluteZ,
            int expectedTargetSlot)
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 0, 1, 1, 0, 0, 0, 0, 500, true);
            RegisterCharacter(world, 2, 1, 2, 10, 0, 0, 0, 500, false);
            RegisterCharacter(
                world, 3, 1, 77, 5, 3, airAbsoluteZ, 0, 500, false);

            CaptureAndAssertNearestDifferential(
                world,
                self,
                inputPhase: 2,
                expectedSlot: expectedTargetSlot,
                context: $"air targets require abs(Z delta) < 40 exactly; z={airAbsoluteZ}");
        }

        [Test]
        public void Candidate_DenseAlternatingTeams_VisitsOnlyAllowedGroundSpan()
        {
            const int targetCount = 128;
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            for (int slot = 1; slot <= targetCount; slot++)
            {
                int team = (slot & 1) == 1 ? 5 : 1;
                RegisterCharacter(
                    world, slot, 1, team, 0, 0, 0, 0, 500, false);
            }

            world.ResetAiSoACandidateDiagnostics();
            CaptureAndAssertNearestDifferential(
                world,
                self,
                inputPhase: 1,
                expectedSlot: 1,
                context: "dense alternating phase-1 team row");

            Assert.That(
                world.AiSoACandidateGroundXRowVisitCountForDiagnostics,
                Is.EqualTo(targetCount / 2),
                "only the team-5 span is authority-eligible for a non-team-5 self in phase 1");
            Assert.That(
                world.AiSoACandidateGroundXRowVisitCountForDiagnostics,
                Is.LessThan(targetCount + 1));
        }

        [TestCase(-0.01, 2)]
        [TestCase(0.001, -1)]
        public void Candidate_PreFallback_State3000UsesAuthorityVxBoundary(
            double vx,
            int expectedSlot)
        {
            SimulationWorld legacy = CreateState3000World(vx, out LF2Character legacySelf);
            SimulationWorld candidate = CreateState3000World(vx, out LF2Character candidateSelf);
            legacy.Rng.Seed(0x3010u);
            candidate.Rng.Seed(0x3010u);
            EnableCandidate(candidate);
            Invoke(candidate, "SetAiSoACandidateFailureForSelfCheck", true, false);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(expectedSlot));
        }

        [Test]
        public void Candidate_PostFallback_UsesFullAscendingSpecialSlotsWithoutLegacyIndex()
        {
            SimulationWorld legacy = CreateSpecialFallbackWorld(out LF2Character legacySelf);
            SimulationWorld candidate = CreateSpecialFallbackWorld(out LF2Character candidateSelf);
            legacy.Rng.Seed(0x5A10u);
            candidate.Rng.Seed(0x5A10u);
            EnableCandidate(candidate);
            Invoke(candidate, "SetAiSoACandidateFailureForSelfCheck", false, true);

            legacy.CharacterInputAll(2);
            candidate.CharacterInputAll(2);

            AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(20),
                "strict < selection keeps the first equally-qualified special slot");
            Assert.That(candidate.AiSoACandidatePostRandomFailureCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiLegacySnapshotIndexBuildCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void Candidate_PostFallback_SameTeamMutationUsesLiveFullSummaryParity()
        {
            SimulationWorld legacy = CreateSameTeamFallbackWorld(
                out LF2Character legacySelf,
                out LF2Character legacyTeammate);
            SimulationWorld candidate = CreateSameTeamFallbackWorld(
                out LF2Character candidateSelf,
                out LF2Character candidateTeammate);
            legacy.Rng.Seed(0x7A11u);
            candidate.Rng.Seed(0x7A11u);
            EnableCandidate(candidate);
            Invoke(candidate, "SetAiSoACandidateFailureForSelfCheck", false, true);

            Invoke(legacy, "BuildAiInputSlotSnapshot");
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                legacyTeammate.Runtime.HP = 10;
                candidateTeammate.Runtime.HP = 10;
                Invoke(legacy, "ObserveAiTeamHpSummaryMutation", legacyTeammate);
                Invoke(candidate, "ObserveAiCandidateCharacterInputMutation", candidateTeammate);
                Invoke(candidate, "RefreshAiSoASensingShadowRowAfterCharacterInput", candidateTeammate);

                AssertPrepareSegmentParity(
                    candidate, candidateSelf, legacy, legacySelf, 2);
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
                Invoke(legacy, "ClearAiInputSlotSnapshot");
            }

            // The authoritative Legacy path selects the special object at slot 20 here;
            // the purpose of this case is the invalidated live team summary, not a
            // nearest-enemy target expectation.
            Assert.That(candidateSelf.Runtime.Unk360, Is.EqualTo(20));
            Assert.That(candidate.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(candidate.AiSameTeamSummaryFallbackCountForDiagnostics,
                Is.EqualTo(1),
                "Candidate fallback must rebuild the same-team facts from the live full slot scan");
            Assert.That(candidate.AiLegacySnapshotMutationCountForDiagnostics, Is.Zero);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Candidate_PreAndPostRandomFailures_FallBackAtOneBoundaryOnly(
            bool failBeforeRandom)
        {
            SimulationWorld legacy = CreateTwoAiCacheWorld(
                out LF2Character legacyFirst,
                out LF2Character legacySecond);
            SimulationWorld candidate = CreateTwoAiCacheWorld(
                out LF2Character candidateFirst,
                out LF2Character candidateSecond);
            legacy.Rng.Seed(0x51A7u);
            candidate.Rng.Seed(0x51A7u);
            EnableCandidate(candidate);
            Invoke(
                candidate,
                "SetAiSoACandidateFailureForSelfCheck",
                failBeforeRandom,
                !failBeforeRandom);

            Invoke(legacy, "BuildAiInputSlotSnapshot");
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                AssertPrepareSegmentParity(
                    candidate,
                    candidateFirst,
                    legacy,
                    legacyFirst,
                    2);
                AssertPrepareSegmentParity(
                    candidate,
                    candidateSecond,
                    legacy,
                    legacySecond,
                    2);
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
                Invoke(legacy, "ClearAiInputSlotSnapshot");
            }

            AssertParity(candidate, candidateFirst, legacy, legacyFirst, 2);
            if (failBeforeRandom)
            {
                Assert.That(
                    candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidatePreRandomFailureCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoACandidatePostRandomFailureCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidateLegacyNearestScanCountForDiagnostics,
                    Is.EqualTo(2));
                Assert.That(candidate.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                    Is.EqualTo(2));
            }
            else
            {
                Assert.That(
                    candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                    Is.Zero,
                    "forced special failure must run before the empty-list fast path");
                Assert.That(candidate.AiSoACandidatePreRandomFailureCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidatePostRandomFailureCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoACandidateNearestQueryCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics,
                    Is.Zero);
                Assert.That(candidate.AiSoACandidateLegacyNearestScanCountForDiagnostics,
                    Is.EqualTo(1),
                    "post-RNG failover must not repeat the first AI nearest query; " +
                    "the higher-slot AI must enter Legacy directly from the pass latch");
                Assert.That(candidate.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                    Is.EqualTo(2));
            }

            Invoke(
                candidate,
                "SetAiSoACandidateFailureForSelfCheck",
                false,
                false);
            candidate.ResetAiSoACandidateDiagnostics();
            Invoke(legacy, "BuildAiInputSlotSnapshot");
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                AssertPrepareSegmentParity(
                    candidate,
                    candidateFirst,
                    legacy,
                    legacyFirst,
                    3);
                AssertPrepareSegmentParity(
                    candidate,
                    candidateSecond,
                    legacy,
                    legacySecond,
                    3);
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
                Invoke(legacy, "ClearAiInputSlotSnapshot");
            }

            Assert.That(candidate.AiSoACandidateNearestQueryCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                Is.EqualTo(2));
            AssertNoLegacyCandidateScans(candidate);
            AssertParity(candidate, candidateFirst, legacy, legacyFirst, 3);
        }

        [Test]
        public void Candidate_NoTarget_DoesNotRunSpecialOrLegacyScans()
        {
            var candidate = new SimulationWorld();
            candidate.Runtime.Flow.InputPhase = 2;
            LF2Character self = RegisterCharacter(
                candidate, 0, 1, 1, 0, 0, 0, 9, 500, true);
            self.Runtime.Unk360 = -1;
            candidate.Rng.Seed(0x4401u);
            EnableCandidate(candidate);

            candidate.CharacterInputAll(2);

            Assert.That(self.Runtime.Unk360, Is.EqualTo(-1));
            Assert.That(candidate.AiSoACandidateNearestQueryCountForDiagnostics, Is.EqualTo(1));
            Assert.That(candidate.AiSoACandidateSpecialQueryCountForDiagnostics, Is.Zero);
            AssertNoLegacyCandidateScans(candidate);
        }

        [Test]
        public void Candidate_EarlierCurrentCharacterDatNonCharacterShell_RefreshesItsRow()
        {
            var candidate = new SimulationWorld();
            candidate.Runtime.Flow.InputPhase = 2;
            CharacterDatShell shell = RegisterShell(
                candidate, 0, 1, 2, 10, 0, 0, 0, 500);
            ConfigureShellWalkingMutation(shell);
            shell.AiControlled = true;
            shell.Runtime.Dir = "right";
            LF2Character self = RegisterCharacter(
                candidate, 1, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(candidate, 2, 1, 2, 50, 0, 0, 0, 500, false);
            candidate.Rng.Seed(0xE411u);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);

            candidate.CharacterInputAll(2);

            Assert.That(shell, Is.Not.InstanceOf<LF2Character>());
            Assert.That(shell.Runtime.Frame, Is.EqualTo(6));
            Assert.That(shell.GetState(), Is.EqualTo(14));
            Assert.That(self.Runtime.Unk360, Is.EqualTo(2));
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics,
                Is.Zero);
            AssertNoLegacyCandidateScans(candidate);
        }

        [Test]
        public void Candidate_RealReleaseAndSameSlotReclaim_FailsBeforeRandomThenMatchesLegacy()
        {
            SimulationWorld legacy = CreateLifecycleWorld(
                out LF2Character legacySelf,
                out LF2Character legacyReleased);
            SimulationWorld candidate = CreateLifecycleWorld(
                out LF2Character candidateSelf,
                out LF2Character candidateReleased);
            legacy.Rng.Seed(0xC011u);
            candidate.Rng.Seed(0xC011u);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);
            Invoke(legacy, "BuildAiInputSlotSnapshot");
            Invoke(candidate, "BuildAiInputSlotSnapshot");
            try
            {
                legacy.Unregister(legacyReleased);
                candidate.Unregister(candidateReleased);
                LF2Character legacyReplacement = CreateCharacter(
                    2, 1, 2, 35, 0, 0, 0, 500, false, 0);
                LF2Character candidateReplacement = CreateCharacter(
                    2, 1, 2, 35, 0, 0, 0, 500, false, 0);
                legacyReplacement.Runtime.StableId = 202;
                candidateReplacement.Runtime.StableId = 202;
                legacy.Register(legacyReplacement);
                candidate.Register(candidateReplacement);

                Invoke(legacy, "PrepareAiInputBasic", legacySelf, 2);
                Invoke(candidate, "PrepareAiInputBasic", candidateSelf, 2);

                AssertParity(candidate, candidateSelf, legacy, legacySelf, 2);
                Assert.That(candidate.AiSoACandidatePreRandomFailureCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoACandidateLegacyNearestScanCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoADecisionRemainderPreRandomFailureCountForDiagnostics,
                    Is.EqualTo(1));
                Assert.That(candidate.AiSoADecisionRemainderPostRandomFailureCountForDiagnostics,
                    Is.Zero);
                Assert.That(
                    candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                    Is.Zero,
                    "epoch drift must fail before an empty-special result is accepted");
            }
            finally
            {
                Invoke(candidate, "ClearAiInputSlotSnapshot");
                Invoke(legacy, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void CharacterInputAllCandidateDecisionContext_Warmed128TicksAllocateZeroManagedBytes()
        {
            SimulationWorld candidate = CreateCacheWorld(out _);
            candidate.Rng.Seed(0xA110Cu);
            EnableCandidate(candidate);
            EnableDecisionRemainder(candidate);
            int tickIndex = 2;
            for (int index = 0; index < 32; index++)
                candidate.CharacterInputAll(tickIndex++);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 128; index++)
                candidate.CharacterInputAll(tickIndex++);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero,
                $"the complete warmed Candidate pass must allocate 0 B/tick; " +
                $"actual total={allocated} B, perTick={allocated / 128.0:F3} B");
            Assert.That(
                candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                Is.EqualTo(candidate.AiSoACandidateSpecialQueryCountForDiagnostics));
            Assert.That(
                candidate.AiSoACandidateEmptySpecialFastPathCountForDiagnostics,
                Is.GreaterThan(0));
            Assert.That(
                candidate.AiSoACandidateFusedSnapshotBuildCountForDiagnostics,
                Is.EqualTo(160));
            Assert.That(
                candidate.AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics,
                Is.EqualTo(160L * candidate.RuntimeSlotCapacityForDiagnostics));
            Assert.That(
                candidate.AiSoACandidateFusedSnapshotFailureCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderContextBindCountForDiagnostics,
                Is.EqualTo(160));
            Assert.That(candidate.AiSoADecisionRemainderGatewayValidationCountForDiagnostics,
                Is.EqualTo(320));
            Assert.That(candidate.AiSoADecisionRemainderRowVisitCountForDiagnostics,
                Is.EqualTo(960));
            Assert.That(candidate.AiSoADecisionRemainderFallbackCountForDiagnostics,
                Is.Zero);
            Assert.That(candidate.AiSoADecisionRemainderHardFailureCountForDiagnostics,
                Is.Zero);
            AssertNoLegacyCandidateScans(candidate);
        }

        [Test]
        public void CharacterInputAllCandidateForcedFallback_Warmed128TicksAllocateZeroManagedBytes()
        {
            SimulationWorld candidate = CreateCacheWorld(out _);
            candidate.Rng.Seed(0xF011u);
            EnableCandidate(candidate);
            Invoke(candidate, "SetAiSoACandidateFailureForSelfCheck", true, true);
            int tickIndex = 2;
            for (int index = 0; index < 32; index++)
                candidate.CharacterInputAll(tickIndex++);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 128; index++)
                candidate.CharacterInputAll(tickIndex++);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero,
                $"the warmed forced Candidate failover must allocate 0 B/tick; total={allocated} B");
        }

        private static SimulationWorld CreateSpecialWorld(
            bool includeD5,
            bool includeC8Threat,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 2, 1, 2, 100, 0, 50, 0, 500, false);
            if (includeD5)
                RegisterCharacter(world, 20, 0xD5, 1, 20, 0, 0, 0x3EC, 500, false);
            if (includeC8Threat)
                RegisterCharacter(world, 21, 0xC8, 2, 10, 3, 0, 0, 500, false, 60);
            return world;
        }

        private static SimulationWorld CreateCacheWorld(out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 2, 1, 2, 80, 0, 0, 0, 500, false);
            RegisterCharacter(world, 3, 1, 2, 20, 0, 20, 0, 500, false);
            self.Runtime.Unk360 = 2;
            return world;
        }

        private static SimulationWorld CreateEmptySpecialGuardWorld(
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 1;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            self.Runtime.PP = 300;
            RegisterCharacter(world, 2, 1, 5, 50, 0, 0, 0, 500, false);
            return world;
        }

        private static SimulationWorld CreateEmptySpecialGuardParityWorld(
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            self.Runtime.PP = 300;
            RegisterCharacter(world, 2, 1, 2, 50, 0, 0, 0, 500, false);
            return world;
        }

        private static SimulationWorld CreateAirOverrideWorld(out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, 500, true);
            // Same distance: authoritative strict tie keeps slot 2 as ground best.
            RegisterCharacter(world, 2, 1, 2, 10, 0, 0, 0, 500, false);
            RegisterCharacter(world, 3, 1, 2, -10, 0, 0, 0, 500, false);
            // Air selection is allowed to replace the target, but not bestDist/sameZLane.
            RegisterCharacter(world, 4, 1, 2, 30, 3, 0, 0, 500, false);
            return world;
        }

        private static SimulationWorld CreateSpatialIndexWorld(out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, -120, 0, 0, 0, 500, true);
            // Register deliberately out of X order.  Slots 2 and 7 are an equal ground
            // tie; the precise slot tie is retained even though the index is X-ordered.
            RegisterCharacter(world, 7, 1, 2, -90, 0, 20, 0, 500, false);
            RegisterCharacter(world, 3, 1, 2, 100000, 0, 0, 0, 500, false);
            RegisterCharacter(world, 2, 1, 2, -150, 0, 20, 0, 500, false);
            RegisterCharacter(world, 4, 1, 2, -100, 3, 0, 0, 500, false);
            RegisterCharacter(world, 6, 1, 1, -121, 0, 0, 0, 500, false);
            return world;
        }

        private static SimulationWorld CreateState3000World(
            double vx,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            LF2Entity approaching = RegisterOther(
                world, 2, 900, 2, 20, 0, 0, 3000, 500);
            approaching.Runtime.Vx = vx;
            return world;
        }

        private static SimulationWorld CreateSpatialRefreshWorld(
            out LF2Character moved,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            moved = RegisterCharacter(world, 0, 1, 2, 1000, 0, 0, 0, 500, false);
            RegisterCharacter(world, 2, 1, 2, 80, 0, 0, 0, 500, false);
            RegisterCharacter(world, 4, 1, 2, -120, 0, 0, 0, 500, false);
            self = RegisterCharacter(world, 10, 1, 1, 0, 0, 0, 9, 500, true);
            return world;
        }

        private static SimulationWorld CreateRoleRefreshWorld(
            string mutation,
            out LF2Entity target,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            var mutableTarget = new MutableDatShell
            {
                CurrentDataObjectType = (int)LF2ObjectType.Character,
            };
            int team = mutation == "team" ? 1 : 2;
            int y = mutation == "y" ? 3 : 0;
            int state = mutation == "state" ? 14 : 0;
            int hp = mutation == "hp" ? 0 : 500;
            if (mutation == "data_type" || mutation == "state3000")
                mutableTarget.CurrentDataObjectType = (int)LF2ObjectType.Other;
            InitializeEntity(mutableTarget, 0, 900, team, 10, y, 0, state, hp, 0);
            world.Register(mutableTarget);
            target = mutableTarget;
            RegisterCharacter(world, 2, 1, 2, 80, 0, 0, 0, 500, false);
            self = RegisterCharacter(world, 10, 1, 1, 0, 0, 0, 9, 500, true);
            return world;
        }

        private static void ApplyRoleEligibilityMutation(LF2Entity target, string mutation)
        {
            switch (mutation)
            {
                case "team":
                    target.Team = 3;
                    target.RelationTeam = 3;
                    break;
                case "hp":
                    target.Runtime.HP = 500;
                    break;
                case "state":
                    SetState(target, 0);
                    break;
                case "y":
                    SetPosition(target, target.Runtime.XInt, 0, target.Runtime.ZInt);
                    break;
                case "data_type":
                    ((MutableDatShell)target).CurrentDataObjectType =
                        (int)LF2ObjectType.Character;
                    break;
                case "state3000":
                    SetState(target, 3000);
                    target.Runtime.Vx = -0.01;
                    break;
                case "ground_to_air":
                    SetPosition(target, target.Runtime.XInt, 3, target.Runtime.ZInt);
                    break;
                case "death":
                    target.Runtime.HP = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mutation));
            }
        }

        private static SimulationWorld CreateTeamAggregateWorld(
            string aggregateCase,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 1;
            int selfHp = aggregateCase == "self_non_min" ? 300 : 100;
            self = RegisterCharacter(world, 0, 1, 77, 0, 0, 0, 9, selfHp, true);
            RegisterCharacter(world, 2, 1, 5, 100, 0, 0, 0, 500, false);

            switch (aggregateCase)
            {
                case "unique_min_excluded":
                    RegisterCharacter(world, 3, 1, 77, -30, 0, 0, 0, 300, false);
                    RegisterCharacter(world, 4, 1, 77, -60, 0, 0, 0, 400, false);
                    break;
                case "duplicate_min":
                    RegisterCharacter(world, 3, 1, 77, -30, 0, 0, 0, 100, false);
                    RegisterCharacter(world, 4, 1, 77, -60, 0, 0, 0, 400, false);
                    break;
                case "self_non_min":
                    RegisterCharacter(world, 3, 1, 77, -30, 0, 0, 0, 100, false);
                    break;
                case "only_member":
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aggregateCase));
            }

            return world;
        }

        private static SimulationWorld CreateAirBoundaryWorld(
            int airAbsoluteX,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, 500, true);
            RegisterCharacter(world, 2, 1, 2, 10, 0, 0, 0, 500, false);
            RegisterCharacter(world, 3, 1, 2, airAbsoluteX, 3, 0, 0, 500, false);
            return world;
        }

        private static SimulationWorld CreateSpecialFallbackWorld(out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 2, 1, 2, 100, 0, 50, 0, 500, false);
            RegisterCharacter(world, 20, 0xD5, 1, 10, 0, 0, 0x3EC, 500, false);
            RegisterCharacter(world, 21, 0xD5, 1, 20, 0, 0, 0x3EC, 500, false);
            return world;
        }

        private static SimulationWorld CreateSameTeamFallbackWorld(
            out LF2Character self,
            out LF2Character teammate)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 1;
            teammate = RegisterCharacter(world, 0, 1, 1, -30, 0, 0, 9, 500, false);
            self = RegisterCharacter(world, 1, 1, 1, 0, 0, 0, 9, 200, true);
            RegisterCharacter(world, 2, 1, 5, 100, 0, 50, 0, 500, false);
            RegisterCharacter(world, 3, 1, 1, -60, 0, 0, 9, 10, false);
            RegisterCharacter(world, 20, 0x7A, 1, 20, 0, 0, 0x3EC, 500, false);
            return world;
        }

        private static SimulationWorld CreateTwoAiCacheWorld(
            out LF2Character first,
            out LF2Character second)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            first = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            second = RegisterCharacter(world, 1, 1, 1, 10, 0, 5, 9, 500, true);
            RegisterCharacter(world, 2, 1, 2, 80, 0, 0, 0, 500, false);
            RegisterCharacter(world, 3, 1, 2, 20, 0, 20, 0, 500, false);
            first.Runtime.Unk360 = 2;
            second.Runtime.Unk360 = 2;
            return world;
        }

        private static SimulationWorld CreateLifecycleWorld(
            out LF2Character self,
            out LF2Character released)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            self.Runtime.StableId = 101;
            released = RegisterCharacter(world, 2, 1, 2, 40, 0, 0, 0, 500, false);
            released.Runtime.StableId = 102;
            RegisterCharacter(world, 3, 1, 2, -90, 0, 5, 0, 500, false)
                .Runtime.StableId = 103;
            self.Runtime.Unk360 = 2;
            return world;
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
            int hp,
            bool aiControlled,
            int frameId = 0)
        {
            LF2Character character = CreateCharacter(
                slot,
                objectId,
                team,
                x,
                y,
                z,
                state,
                hp,
                aiControlled,
                frameId);
            world.Register(character);
            return character;
        }

        private static LF2Character CreateCharacter(
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp,
            bool aiControlled,
            int frameId)
        {
            var character = new LF2Character();
            InitializeEntity(
                character,
                slot,
                objectId,
                team,
                x,
                y,
                z,
                state,
                hp,
                frameId);
            character.Controller = new EmptyController();
            character.AiControlled = aiControlled;
            return character;
        }

        private static CharacterDatShell RegisterShell(
            SimulationWorld world,
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp)
        {
            var shell = new CharacterDatShell();
            InitializeEntity(shell, slot, objectId, team, x, y, z, state, hp, 0);
            world.Register(shell);
            return shell;
        }

        private static LF2Entity RegisterOther(
            SimulationWorld world,
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp)
        {
            var entity = new OtherDatShell();
            InitializeEntity(entity, slot, objectId, team, x, y, z, state, hp, 0);
            world.Register(entity);
            return entity;
        }

        private static void InitializeEntity(
            LF2Entity entity,
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp,
            int frameId)
        {
            LF2Character character = entity as LF2Character;
            character?.ModuleInitialize();
            var frame = new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
            };
            var data = new LF2CharacterData
            {
                name = $"AiSoACandidate_{slot}_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(frameId);
            entity.Frame.PN = frameId;
            entity.Frame.N = frameId;
            character?.Initialize(500, 500);
            entity.FrameDelay = 0;
            entity.SetRequiredRuntimeSlot(slot);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Runtime.HP = hp;
            entity.Runtime.HP3 = 500;
            entity.Runtime.HPBound = 500;
            entity.Runtime.KillCount = -1;
            entity.Runtime.Unk3FC = -1001;
            entity.Runtime.Unk360 = -1;
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
        }

        private static void ConfigureShellWalkingMutation(CharacterDatShell shell)
        {
            var standing = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
            };
            var walking = new LF2FrameData
            {
                frameId = 6,
                state = 14,
                wait = 100,
                next = 6,
            };
            var data = new LF2CharacterData
            {
                name = "AiSoACandidate_CharacterDatShellWalkingMutation",
                type_sub = (int)LF2ObjectType.Character,
                walking_frame_rate = 1,
                walking_speed = 4f,
                walking_speedz = 2f,
                frames = new List<LF2FrameData> { standing, walking },
            };
            shell.FrameCache.Load(new LF2CharacterDataWrapper(shell.ObjectId, data));
            shell.Frame.D = shell.FrameCache.GetFrameDataById(0);
            shell.Frame.PN = 0;
            shell.Frame.N = 0;
            shell.Runtime.Frame = 0;
            shell.Runtime.NextFrame = 0;
        }

        private static void EnableCandidate(SimulationWorld world)
        {
            world.ResetAiSoACandidateDiagnostics();
            Invoke(world, "SetAiSoACandidateModeForSelfCheck", true);
        }

        private static void EnableDecisionRemainder(SimulationWorld world)
        {
            Invoke(world, "SetAiSoADecisionRemainderModeForSelfCheck", true);
        }

        private static bool TryGetDecisionRemainderRow(
            SimulationWorld world,
            LF2Entity entity,
            out object rows,
            out int slot)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "TryGetAiSoADecisionRemainderRow",
                InstanceMembers);
            Assert.That(method, Is.Not.Null, "TryGetAiSoADecisionRemainderRow");
            object[] arguments = { entity, null, -1 };
            bool found = (bool)method.Invoke(world, arguments);
            rows = arguments[1];
            slot = (int)arguments[2];
            return found;
        }

        private static void AssertNoLegacyCandidateScans(SimulationWorld world)
        {
            Assert.That(world.AiSoACandidateLegacyNearestScanCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidateLegacySpecialScanCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidatePreRandomFailureCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidatePostRandomFailureCountForDiagnostics, Is.Zero);
        }

        private static void AssertLegacySnapshotProductsWereBuilt(SimulationWorld world)
        {
            Assert.That(world.AiLegacyNearestFactsBuildCountForDiagnostics, Is.GreaterThan(0));
            Assert.That(world.AiLegacySnapshotIndexBuildCountForDiagnostics, Is.GreaterThan(0));
            Assert.That(world.AiLegacyQuadtreeSyncCountForDiagnostics, Is.GreaterThan(0));
            Assert.That(world.AiLegacySnapshotMutationCountForDiagnostics, Is.GreaterThan(0));
        }

        private static void AssertCandidateBuiltNoLegacySnapshotProducts(SimulationWorld world)
        {
            Assert.That(world.AiLegacyNearestFactsBuildCountForDiagnostics, Is.Zero);
            Assert.That(world.AiLegacySnapshotIndexBuildCountForDiagnostics, Is.Zero);
            Assert.That(world.AiLegacyQuadtreeSyncCountForDiagnostics, Is.Zero);
            Assert.That(world.AiLegacySnapshotMutationCountForDiagnostics, Is.Zero);
        }

        private static void AssertParity(
            SimulationWorld candidate,
            LF2Character candidateSelf,
            SimulationWorld legacy,
            LF2Character legacySelf,
            int tickIndex)
        {
            Assert.That(candidate.Rng.State, Is.EqualTo(legacy.Rng.State));
            Assert.That(candidate.Rng.CallCount, Is.EqualTo(legacy.Rng.CallCount));
            Assert.That(InputChecksum(candidateSelf.Runtime),
                Is.EqualTo(InputChecksum(legacySelf.Runtime)));
            BattleParityFrameSnapshot candidateSnapshot = candidate.CaptureParityFrameSnapshot(
                tickIndex,
                FrameInputSet.Empty(tickIndex));
            BattleParityFrameSnapshot legacySnapshot = legacy.CaptureParityFrameSnapshot(
                tickIndex,
                FrameInputSet.Empty(tickIndex));
            Assert.That(candidateSnapshot.Hashes.Input, Is.EqualTo(legacySnapshot.Hashes.Input));
            Assert.That(candidateSnapshot.Hashes.Rng, Is.EqualTo(legacySnapshot.Hashes.Rng));
            Assert.That(candidateSnapshot.Hashes.Overall, Is.EqualTo(legacySnapshot.Hashes.Overall));
        }

        private static void AssertPrepareSegmentParity(
            SimulationWorld candidate,
            LF2Character candidateSelf,
            SimulationWorld legacy,
            LF2Character legacySelf,
            int tickIndex)
        {
            ulong candidateCallsBefore = candidate.Rng.CallCount;
            ulong legacyCallsBefore = legacy.Rng.CallCount;
            Invoke(legacy, "PrepareAiInputBasic", legacySelf, tickIndex);
            Invoke(candidate, "PrepareAiInputBasic", candidateSelf, tickIndex);

            Assert.That(
                candidate.Rng.CallCount - candidateCallsBefore,
                Is.EqualTo(legacy.Rng.CallCount - legacyCallsBefore),
                "each AI segment must consume exactly the Legacy RNG calls; " +
                "failover must not repeat the cache Rand(30)");
            Assert.That(candidate.Rng.State, Is.EqualTo(legacy.Rng.State));
            Assert.That(InputChecksum(candidateSelf.Runtime),
                Is.EqualTo(InputChecksum(legacySelf.Runtime)));
        }

        private static void CaptureAndAssertNearestDifferential(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            int? expectedSlot,
            string context)
        {
            object[] legacyArguments = { self, inputPhase, true, true, 0, 0, false };
            Invoke(world, "CaptureAiNearestFactsTargetForSelfCheck", legacyArguments);
            int legacySelected = (int)legacyArguments[4];
            int legacyBestDist = (int)legacyArguments[5];
            bool legacySameZLane = (bool)legacyArguments[6];
            Assert.That(
                CaptureAiSoANearest(
                    world,
                    self,
                    inputPhase,
                    out int candidateSelected,
                    out int candidateBestDist,
                    out bool candidateSameZLane),
                Is.True,
                context);

            Assert.That(candidateSelected, Is.EqualTo(legacySelected), context);
            Assert.That(candidateBestDist, Is.EqualTo(legacyBestDist), context);
            Assert.That(candidateSameZLane, Is.EqualTo(legacySameZLane), context);
            if (expectedSlot.HasValue)
                Assert.That(candidateSelected, Is.EqualTo(expectedSlot.Value), context);
        }

        private static bool CaptureAiSoANearest(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            object[] arguments = { self, inputPhase, 0, 0, false };
            bool succeeded = (bool)Invoke(
                world,
                "CaptureAiSoASensingNearestForSelfCheck",
                arguments);
            selected = (int)arguments[2];
            bestDist = (int)arguments[3];
            sameZLane = (bool)arguments[4];
            return succeeded;
        }

        private static bool CaptureAiSoACandidateSpecial(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            int initialSelectedSlot,
            int nearestBestDist,
            bool sameZLane,
            out int selectedSlot,
            out int bestDist,
            out bool resultSameZLane,
            out int flags)
        {
            object[] arguments =
            {
                self,
                inputPhase,
                initialSelectedSlot,
                nearestBestDist,
                sameZLane,
                null
            };
            bool succeeded = (bool)Invoke(
                world,
                "TryRunAiSoACandidateSpecial",
                arguments);

            selectedSlot = -1;
            bestDist = 10000;
            resultSameZLane = false;
            flags = 0;
            object result = arguments[5];
            if (result == null)
                return succeeded;

            Type resultType = result.GetType();
            selectedSlot = (int)resultType.GetField("SelectedSlot", InstanceMembers)
                .GetValue(result);
            bestDist = (int)resultType.GetField("BestDist", InstanceMembers)
                .GetValue(result);
            resultSameZLane = (bool)resultType.GetField("SameZLane", InstanceMembers)
                .GetValue(result);
            flags = (int)resultType.GetField("Flags", InstanceMembers)
                .GetValue(result);
            return succeeded;
        }

        private static void IncrementAiSoARowGeneration(
            SimulationWorld world,
            int slot)
        {
            FieldInfo rowsField = typeof(SimulationWorld).GetField(
                "aiSoASensingRows",
                InstanceMembers);
            Assert.That(rowsField, Is.Not.Null, "aiSoASensingRows");
            object rows = rowsField.GetValue(world);
            Assert.That(rows, Is.Not.Null, "AiSoA rows must exist after snapshot build");
            FieldInfo generationField = rows.GetType().GetField(
                "Generation",
                InstanceMembers);
            Assert.That(generationField, Is.Not.Null, "Generation");
            uint[] generations = (uint[])generationField.GetValue(rows);
            generations[slot]++;
        }

        private static int CaptureAiSoAShadowSpecialFlags(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase)
        {
            Invoke(world, "BuildAiInputSlotSnapshot");
            try
            {
                object[] arguments =
                {
                    self.Runtime.SlotIndex,
                    inputPhase,
                    world.Rng.State,
                    false,
                    null
                };
                Assert.That(
                    (bool)Invoke(world, "TryRunAiSoASensingShadowQuery", arguments),
                    Is.True);
                object result = arguments[4];
                Assert.That(result, Is.Not.Null, "shadow result");
                return (int)result.GetType().GetField("SpecialFlags", InstanceMembers)
                    .GetValue(result);
            }
            finally
            {
                Invoke(world, "ClearAiInputSlotSnapshot");
            }
        }

        private static int[] GetAiSoARoleSlots(
            SimulationWorld world,
            string slotsFieldName,
            string countFieldName)
        {
            FieldInfo rowsField = typeof(SimulationWorld).GetField(
                "aiSoASensingRows",
                InstanceMembers);
            Assert.That(rowsField, Is.Not.Null, "aiSoASensingRows");
            object rows = rowsField.GetValue(world);
            Assert.That(rows, Is.Not.Null, "AiSoA rows must exist after snapshot build");
            Type rowsType = rows.GetType();
            FieldInfo slotsField = rowsType.GetField(slotsFieldName, InstanceMembers);
            FieldInfo countField = rowsType.GetField(countFieldName, InstanceMembers);
            Assert.That(slotsField, Is.Not.Null, slotsFieldName);
            Assert.That(countField, Is.Not.Null, countFieldName);
            int count = (int)countField.GetValue(rows);
            int[] source = (int[])slotsField.GetValue(rows);
            var result = new int[count];
            Array.Copy(source, result, count);
            return result;
        }

        private static LF2Entity[] GetAiInputSlots(SimulationWorld world)
        {
            FieldInfo field = typeof(SimulationWorld).GetField(
                "aiInputSlots",
                InstanceMembers);
            Assert.That(field, Is.Not.Null, "aiInputSlots");
            return (LF2Entity[])field.GetValue(world);
        }

        private static bool[] GetAiSoAIncludedRows(SimulationWorld world)
        {
            FieldInfo rowsField = typeof(SimulationWorld).GetField(
                "aiSoASensingRows",
                InstanceMembers);
            Assert.That(rowsField, Is.Not.Null, "aiSoASensingRows");
            object rows = rowsField.GetValue(world);
            Assert.That(rows, Is.Not.Null, "AiSoA rows must exist after snapshot build");
            FieldInfo includedField = rows.GetType().GetField(
                "Included",
                InstanceMembers);
            Assert.That(includedField, Is.Not.Null, "Included");
            return (bool[])includedField.GetValue(rows);
        }

        private static string[] GetAiSoARoleTeamSpanKeys(
            SimulationWorld world,
            string roleName)
        {
            FieldInfo rowsField = typeof(SimulationWorld).GetField(
                "aiSoASensingRows",
                InstanceMembers);
            Assert.That(rowsField, Is.Not.Null, "aiSoASensingRows");
            object rows = rowsField.GetValue(world);
            Assert.That(rows, Is.Not.Null, "AiSoA rows must exist after snapshot build");
            Type rowsType = rows.GetType();
            FieldInfo slotsField = rowsType.GetField(
                $"{roleName}RoleSlotsByX",
                InstanceMembers);
            FieldInfo summariesField = rowsType.GetField(
                $"{roleName}RoleTeamSummaries",
                InstanceMembers);
            FieldInfo summaryCountField = rowsType.GetField(
                $"{roleName}RoleTeamSummaryCount",
                InstanceMembers);
            Assert.That(slotsField, Is.Not.Null, $"{roleName}RoleSlotsByX");
            Assert.That(summariesField, Is.Not.Null, $"{roleName}RoleTeamSummaries");
            Assert.That(summaryCountField, Is.Not.Null, $"{roleName}RoleTeamSummaryCount");

            int[] slots = (int[])slotsField.GetValue(rows);
            Array summaries = (Array)summariesField.GetValue(rows);
            int summaryCount = (int)summaryCountField.GetValue(rows);
            var result = new string[summaryCount];
            for (int index = 0; index < summaryCount; index++)
            {
                object summary = summaries.GetValue(index);
                Type summaryType = summary.GetType();
                int team = (int)summaryType.GetField("Team", InstanceMembers)
                    .GetValue(summary);
                int start = (int)summaryType.GetField("Start", InstanceMembers)
                    .GetValue(summary);
                int count = (int)summaryType.GetField("Count", InstanceMembers)
                    .GetValue(summary);
                var spanSlots = new int[count];
                Array.Copy(slots, start, spanSlots, 0, count);
                result[index] = $"{team}:{string.Join(",", spanSlots)}";
            }

            return result;
        }

        private static ulong InputChecksum(NTSDEntityRuntime input)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            unchecked
            {
                hash = (hash ^ input.KeyUp) * prime;
                hash = (hash ^ input.KeyDown) * prime;
                hash = (hash ^ input.KeyLeft) * prime;
                hash = (hash ^ input.KeyRight) * prime;
                hash = (hash ^ input.KeyAttack) * prime;
                hash = (hash ^ input.KeyJump) * prime;
                hash = (hash ^ input.KeyDefend) * prime;
                hash = (hash ^ input.PrevUp) * prime;
                hash = (hash ^ input.PrevDown) * prime;
                hash = (hash ^ input.PrevLeft) * prime;
                hash = (hash ^ input.PrevRight) * prime;
                hash = (hash ^ input.PrevAttack) * prime;
                hash = (hash ^ input.PrevJump) * prime;
                hash = (hash ^ input.PrevDefend) * prime;
                hash = (hash ^ input.CdUp) * prime;
                hash = (hash ^ input.CdDown) * prime;
                hash = (hash ^ input.CdLeft) * prime;
                hash = (hash ^ input.CdRight) * prime;
                hash = (hash ^ input.CdAttack) * prime;
                hash = (hash ^ input.CdJump) * prime;
                hash = (hash ^ input.CdDefend) * prime;
                hash = (hash ^ (uint)input.Unk360) * prime;
            }
            return hash;
        }

        private static ulong ForeignFactChecksum(LF2Entity entity)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            NTSDEntityRuntime runtime = entity.Runtime;
            ulong hash = offset;
            unchecked
            {
                hash = (hash ^ (uint)entity.ObjectId) * prime;
                hash = (hash ^ (uint)runtime.StableId) * prime;
                hash = (hash ^ (uint)runtime.SlotIndex) * prime;
                hash = (hash ^ (uint)runtime.XInt) * prime;
                hash = (hash ^ (uint)runtime.YInt) * prime;
                hash = (hash ^ (uint)runtime.ZInt) * prime;
                hash = (hash ^ (uint)runtime.HP) * prime;
                hash = (hash ^ (uint)runtime.HP3) * prime;
                hash = (hash ^ (uint)runtime.HPBound) * prime;
                hash = (hash ^ (uint)runtime.PP) * prime;
                hash = (hash ^ (uint)runtime.RelationTeam) * prime;
                hash = (hash ^ (uint)runtime.Frame) * prime;
                hash = (hash ^ (uint)runtime.LinkState) * prime;
                hash = (hash ^ (uint)runtime.TargetSlotIndex) * prime;
                hash = (hash ^ (uint)runtime.HitStop) * prime;
                hash = (hash ^ (runtime.Dir == "left" ? 1u : 0u)) * prime;
            }
            return hash;
        }

        private static object Invoke(
            SimulationWorld world,
            string methodName,
            params object[] arguments)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                methodName,
                InstanceMembers);
            Assert.That(method, Is.Not.Null, methodName);
            return method.Invoke(world, arguments);
        }

        private static void GetAiSoASameTeamSummaryExcludingSelf(
            SimulationWorld world,
            int selfSlot,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            FieldInfo rowsField = typeof(SimulationWorld).GetField(
                "aiSoASensingRows",
                InstanceMembers);
            Assert.That(rowsField, Is.Not.Null, "aiSoASensingRows");
            object rows = rowsField.GetValue(world);
            Assert.That(rows, Is.Not.Null, "AiSoA rows must exist after snapshot build");

            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "GetAiSoASameTeamSummaryExcludingSelf",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "GetAiSoASameTeamSummaryExcludingSelf");
            object[] arguments = { rows, selfSlot, selfTeam, 0, 0 };
            method.Invoke(null, arguments);
            otherCount = (int)arguments[3];
            otherMinHp = (int)arguments[4];
        }

        private static void SetProperty(
            SimulationWorld world,
            string propertyName,
            object value)
        {
            PropertyInfo property = typeof(SimulationWorld).GetProperty(
                propertyName,
                InstanceMembers);
            Assert.That(property, Is.Not.Null, propertyName);
            property.SetValue(world, value);
        }

        private static void SetPosition(LF2Entity entity, int x, int y, int z)
        {
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
        }

        private static void SetState(LF2Entity entity, int state)
        {
            Assert.That(entity.Frame.D, Is.Not.Null);
            entity.Frame.D.state = state;
        }

        private static bool GetBooleanField(
            SimulationWorld world,
            string fieldName)
        {
            FieldInfo field = typeof(SimulationWorld).GetField(
                fieldName,
                InstanceMembers);
            Assert.That(field, Is.Not.Null, fieldName);
            return (bool)field.GetValue(world);
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }

        private sealed class CharacterDatShell : LF2OtherObject
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }

        private sealed class OtherDatShell : LF2OtherObject
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Other;
        }

        private sealed class MutableDatShell : LF2OtherObject
        {
            internal int CurrentDataObjectType { get; set; }

            public override int GetCurrentDataObjectTypeForSimulation() =>
                CurrentDataObjectType;
        }
    }
}
#endif
