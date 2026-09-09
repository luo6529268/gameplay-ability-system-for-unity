#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Animation;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B5HitGroupEligibilityPureCoreEditorTests
    {
        [TestCase(0)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(50)]
        public void InvalidSnapshot_AlwaysRejectsBeforeKindBypass(int kind)
        {
            BattleHitCandidatePairSnapshot pair = default;

            BattleHitGroupEligibilityResult result = Resolve(kind, 0, in pair, 0);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason,
                Is.EqualTo(BattleHitGroupEligibilityReason.InvalidSnapshot));
        }

        [TestCase(4)]
        [TestCase(8)]
        [TestCase(50)]
        public void OriginalKindBypass_AcceptsBeforeOrdinaryGroupBlock(int kind)
        {
            BattleHitCandidatePairSnapshot pair = Pair();

            BattleHitGroupEligibilityResult result = Resolve(kind, 21, in pair, 0);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Reason,
                Is.EqualTo(BattleHitGroupEligibilityReason.OriginalKindBypass));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        [TestCase(15)]
        [TestCase(16)]
        [TestCase(49)]
        [TestCase(51)]
        public void NonBypassKinds_ReachOrdinaryGroupBlock(int kind)
        {
            BattleHitCandidatePairSnapshot pair = Pair();

            BattleHitGroupEligibilityResult result = Resolve(kind, 21, in pair, 0);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason,
                Is.EqualTo(BattleHitGroupEligibilityReason.Rejected));
        }

        [TestCase(10)]
        [TestCase(13)]
        public void TargetCurrentStateBypass_AcceptsBeforeOidAndGroup(int targetState)
        {
            BattleHitCandidatePairSnapshot pair = Pair(targetCurrentState: targetState);

            BattleHitGroupEligibilityResult result = Resolve(0, 21, in pair, 0);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Reason,
                Is.EqualTo(BattleHitGroupEligibilityReason.TargetCurrentStateBypass));
        }

        [Test]
        public void FreezeColumn212_DifferentOidAccepts()
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                attackerObjectId: 1,
                targetObjectId: 212);

            BattleHitGroupEligibilityResult result = Resolve(0, 21, in pair, 0);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Reason,
                Is.EqualTo(BattleHitGroupEligibilityReason.FreezeColumnOverride));
        }

        [TestCase(10, 15, true)]
        [TestCase(20, 25, true)]
        [TestCase(11, 15, false)]
        [TestCase(10, 14, false)]
        public void FreezeColumn212_SameOidRequiresZeroToFiveActionPhase(
            int attackerAction,
            int targetAction,
            bool expected)
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                attackerObjectId: 212,
                targetObjectId: 212,
                attackerAction: attackerAction,
                targetAction: targetAction);

            BattleHitGroupEligibilityResult result = Resolve(0, 21, in pair, 0);

            Assert.That(result.Accepted, Is.EqualTo(expected));
            Assert.That(result.Reason, Is.EqualTo(expected
                ? BattleHitGroupEligibilityReason.FreezeColumnOverride
                : BattleHitGroupEligibilityReason.Rejected));
        }

        [Test]
        public void NonFreezeColumnOid_DoesNotUseActionPhaseOverride()
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                attackerObjectId: 213,
                targetObjectId: 213,
                attackerAction: 10,
                targetAction: 15);

            BattleHitGroupEligibilityResult result = Resolve(0, 21, in pair, 0);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason,
                Is.EqualTo(BattleHitGroupEligibilityReason.Rejected));
        }

        [TestCase(0, 0, 0, true, (int)BattleHitGroupEligibilityReason.ZeroAttackerGroup)]
        [TestCase(7, 8, 0, true, (int)BattleHitGroupEligibilityReason.OrdinaryDifferentGroup)]
        [TestCase(7, 7, 0, false, (int)BattleHitGroupEligibilityReason.Rejected)]
        [TestCase(7, 7, 190, true, (int)BattleHitGroupEligibilityReason.State190SameGroup)]
        [TestCase(7, 8, 190, false, (int)BattleHitGroupEligibilityReason.Rejected)]
        public void GroupGate_ImplementsZeroOrdinaryAndState190Reversal(
            int attackerGroup,
            int targetGroup,
            int attackerState,
            bool expected,
            int reason)
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                attackerGroup: attackerGroup,
                targetGroup: targetGroup,
                attackerCurrentState: attackerState);

            BattleHitGroupEligibilityResult result = Resolve(0, 21, in pair, 0);

            Assert.That(result.Accepted, Is.EqualTo(expected));
            Assert.That(result.Reason,
                Is.EqualTo((BattleHitGroupEligibilityReason)reason));
        }

        [TestCase(1, 0, 0, true)]
        [TestCase(3, 0, 0, true)]
        [TestCase(0, 18, 0, true)]
        [TestCase(0, 180, 0, true)]
        [TestCase(0, 0, 0, false)]
        [TestCase(2, 0, 0, false)]
        [TestCase(1, 0, 21, false)]
        [TestCase(3, 0, 22, false)]
        [TestCase(0, 18, 21, false)]
        [TestCase(0, 180, 22, false)]
        public void ModeAndStateExceptions_ExcludeEffects21And22(
            int modeGate18,
            int attackerState,
            int effect,
            bool expected)
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                attackerCurrentState: attackerState);

            BattleHitGroupEligibilityResult result =
                Resolve(0, effect, in pair, modeGate18);

            Assert.That(result.Accepted, Is.EqualTo(expected));
            Assert.That(result.Reason, Is.EqualTo(expected
                ? BattleHitGroupEligibilityReason.ModeOrStateEffectException
                : BattleHitGroupEligibilityReason.Rejected));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        public void TargetObjectTypeExceptions_AcceptAfterGroupExceptionBlock(int targetType)
        {
            BattleHitCandidatePairSnapshot pair = Pair(targetObjectType: targetType);

            BattleHitGroupEligibilityResult result = Resolve(0, 21, in pair, 0);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Reason,
                Is.EqualTo(BattleHitGroupEligibilityReason.TargetObjectTypeException));
        }

        [TestCase(0, 3, false, true, true)]
        [TestCase(0, 3, true, true, false)]
        [TestCase(1, 3, false, true, false)]
        [TestCase(0, 0, false, true, false)]
        public void FinalFacingException_HasAuthorityPolarity(
            int attackerType,
            int targetType,
            bool attackerFacing,
            bool targetFacing,
            bool expected)
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                attackerObjectType: attackerType,
                targetObjectType: targetType,
                attackerFacing: attackerFacing,
                targetFacing: targetFacing);

            BattleHitGroupEligibilityResult result = Resolve(0, 21, in pair, 0);

            Assert.That(result.Accepted, Is.EqualTo(expected));
            Assert.That(result.Reason, Is.EqualTo(expected
                ? BattleHitGroupEligibilityReason.OpposingFacingType3Exception
                : BattleHitGroupEligibilityReason.Rejected));
        }

        [Test]
        public void WarmResolver_AllocatesZeroManagedBytes()
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                attackerCurrentState: 190,
                attackerGroup: 7,
                targetGroup: 7);
            _ = Resolve(0, 0, in pair, 0);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int accepted = 0;
            for (int index = 0; index < 100000; index++)
            {
                if (Resolve(index & 1, index & 31, in pair, index & 3).Accepted)
                    accepted++;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(accepted, Is.GreaterThan(0));
            Assert.That(allocated, Is.Zero);
        }

        private static BattleHitGroupEligibilityResult Resolve(
            int kind,
            int effect,
            in BattleHitCandidatePairSnapshot pair,
            int modeGate18)
        {
            return BattleHitGroupEligibilityResolver.Resolve(
                kind,
                effect,
                in pair,
                modeGate18);
        }

        private static BattleHitCandidatePairSnapshot Pair(
            bool valid = true,
            int attackerObjectId = 1,
            int targetObjectId = 2,
            int attackerObjectType = 5,
            int targetObjectType = 0,
            int attackerGroup = 7,
            int targetGroup = 7,
            int attackerAction = 0,
            int targetAction = 0,
            int attackerCurrentState = 0,
            int targetCurrentState = 0,
            bool attackerFacing = false,
            bool targetFacing = false)
        {
            return new BattleHitCandidatePairSnapshot(
                valid,
                attackerObjectId,
                targetObjectId,
                attackerObjectType,
                targetObjectType,
                attackerGroup,
                targetGroup,
                attackerAction,
                targetAction,
                attackerCurrentState,
                targetCurrentState,
                101,
                102,
                103,
                104,
                attackerFacing,
                targetFacing,
                true,
                99);
        }
    }
}
#endif
