#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B5FirstBodyResponsePureCoreEditorTests
    {
        [TestCase(999)]
        [TestCase(2999)]
        [TestCase(999999999)]
        [TestCase(1999999999)]
        [TestCase(2000000000)]
        public void StrictRanges_RejectValuesOutsideAuthorityGates(int firstBodyKind)
        {
            BattleFirstBodyResponseResult result = Resolve(firstBodyKind);

            Assert.That(result.Kind, Is.EqualTo(BattleFirstBodyResponseKind.None));
            Assert.That(result.Recognized, Is.False);
            Assert.That(result.Applied, Is.False);
            Assert.That(result.NeedsRoll, Is.False);
        }

        [TestCase(1000, 0, true)]
        [TestCase(1998, 998, true)]
        [TestCase(1999, -1, false)]
        [TestCase(2000, 0, false)]
        [TestCase(2998, 998, false)]
        public void ActionRange_UsesStrict1999SplitAndPreservesOddMinusOne(
            int firstBodyKind,
            int expectedTargetAction,
            bool expectedHold)
        {
            BattleFirstBodyResponseResult result = Resolve(
                firstBodyKind,
                firstBodyRespond: 5,
                attackerGroup: 47);

            Assert.That(result.Kind,
                Is.EqualTo(BattleFirstBodyResponseKind.ActionRange));
            Assert.That(result.Recognized, Is.True);
            Assert.That(result.Applied, Is.True);
            Assert.That(result.FirstBodyKind, Is.EqualTo(firstBodyKind));
            Assert.That(result.FirstBodyRespond, Is.EqualTo(5));
            Assert.That(result.WriteTargetAction, Is.True);
            Assert.That(result.TargetAction, Is.EqualTo(expectedTargetAction));
            Assert.That(result.ResetTargetFrameCounter, Is.False);
            Assert.That(result.WriteAttackerAction, Is.False);
            Assert.That(result.WriteTargetGroup, Is.True);
            Assert.That(result.TargetGroup, Is.EqualTo(5));
            Assert.That(result.ApplyHold, Is.EqualTo(expectedHold));
            Assert.That(result.ApplyManualDamage, Is.False);
        }

        [TestCase(-1, 47)]
        [TestCase(0, 1)]
        [TestCase(6, 6)]
        public void ActionRange_ProjectsRespondUsingAuthorityMapping(
            int firstBodyRespond,
            int expectedGroup)
        {
            BattleFirstBodyResponseResult result = Resolve(
                1033,
                firstBodyRespond,
                attackerGroup: 47);

            Assert.That(result.TargetGroup, Is.EqualTo(expectedGroup));
            Assert.That(result.WriteTargetGroup, Is.True);
        }

        [Test]
        public void EncodedChanceZero_AppliesWithoutRoll()
        {
            int encoded = Encode(0, 123, 456, 7);

            BattleFirstBodyResponseResult result = Resolve(encoded);

            Assert.That(result.Kind,
                Is.EqualTo(BattleFirstBodyResponseKind.EncodedApplied));
            Assert.That(result.Chance, Is.Zero);
            Assert.That(result.NeedsRoll, Is.False);
            Assert.That(result.UsedProvidedRoll, Is.False);
            Assert.That(result.Applied, Is.True);
        }

        [TestCase(1)]
        [TestCase(99)]
        public void EncodedPositiveChance_RequiresExternalRoll(int chance)
        {
            int encoded = Encode(chance, 123, 456, 0);

            BattleFirstBodyResponseResult result = Resolve(encoded);

            Assert.That(result.Kind,
                Is.EqualTo(BattleFirstBodyResponseKind.EncodedNeedsRoll));
            Assert.That(result.Chance, Is.EqualTo(chance));
            Assert.That(result.NeedsRoll, Is.True);
            Assert.That(result.UsedProvidedRoll, Is.False);
            Assert.That(result.Applied, Is.False);
        }

        [TestCase(1, 0, true)]
        [TestCase(1, 1, false)]
        [TestCase(99, 98, true)]
        [TestCase(99, 99, false)]
        public void EncodedChance_UsesStrictRollLessThanChance(
            int chance,
            int roll,
            bool expectedApplied)
        {
            BattleFirstBodyResponseResult result = Resolve(
                Encode(chance, 123, 456, 0),
                hasRoll: true,
                roll: roll);

            Assert.That(result.Kind, Is.EqualTo(expectedApplied
                ? BattleFirstBodyResponseKind.EncodedApplied
                : BattleFirstBodyResponseKind.EncodedChanceRejected));
            Assert.That(result.UsedProvidedRoll, Is.True);
            Assert.That(result.Roll, Is.EqualTo(roll));
            Assert.That(result.Applied, Is.EqualTo(expectedApplied));
        }

        [Test]
        public void EncodedChance100_IsOutsideStrictEncodedRange()
        {
            int chance100 = Encode(100, 0, 0, 0);

            BattleFirstBodyResponseResult result = Resolve(chance100);

            Assert.That(chance100, Is.EqualTo(2000000000));
            Assert.That(result.Kind, Is.EqualTo(BattleFirstBodyResponseKind.None));
            Assert.That(result.Recognized, Is.False);
            Assert.That(result.NeedsRoll, Is.False);
        }

        [TestCase(998, true)]
        [TestCase(999, false)]
        public void EncodedTargetAction_WritesOnlyBelow999(
            int targetAction,
            bool expectedWrite)
        {
            BattleFirstBodyResponseResult result = Resolve(
                Encode(0, targetAction, 999, 0));

            Assert.That(result.TargetAction, Is.EqualTo(targetAction));
            Assert.That(result.WriteTargetAction, Is.EqualTo(expectedWrite));
            Assert.That(result.ResetTargetFrameCounter, Is.EqualTo(expectedWrite));
        }

        [TestCase(998, true)]
        [TestCase(999, false)]
        public void EncodedAttackerAction_WritesOnlyBelow999(
            int attackerAction,
            bool expectedWrite)
        {
            BattleFirstBodyResponseResult result = Resolve(
                Encode(0, 999, attackerAction, 0));

            Assert.That(result.AttackerAction, Is.EqualTo(attackerAction));
            Assert.That(result.WriteAttackerAction, Is.EqualTo(expectedWrite));
            Assert.That(result.ResetAttackerFrameCounter, Is.EqualTo(expectedWrite));
        }

        [TestCase(0, false, false, false)]
        [TestCase(1, true, false, false)]
        [TestCase(2, false, false, true)]
        [TestCase(3, true, false, false)]
        [TestCase(4, false, true, false)]
        [TestCase(5, true, true, false)]
        [TestCase(6, false, true, true)]
        [TestCase(7, true, true, false)]
        [TestCase(8, false, false, false)]
        [TestCase(9, false, false, false)]
        public void EncodedEffects_ProjectExactGroupHoldDamageCombination(
            int effect,
            bool expectedGroup,
            bool expectedHold,
            bool expectedDamage)
        {
            BattleFirstBodyResponseResult result = Resolve(
                Encode(0, 999, 999, effect),
                rawInjury: 37,
                attackerGroup: 47);

            Assert.That(result.Effect, Is.EqualTo(effect));
            Assert.That(result.WriteTargetGroup, Is.EqualTo(expectedGroup));
            Assert.That(result.TargetGroup, Is.EqualTo(expectedGroup ? 47 : 0));
            Assert.That(result.ApplyHold, Is.EqualTo(expectedHold));
            Assert.That(result.ApplyManualDamage, Is.EqualTo(expectedDamage));
            Assert.That(result.ManualDamage, Is.EqualTo(expectedDamage ? 37 : 0));
        }

        [TestCase(-9)]
        [TestCase(2000000000)]
        public void EncodedManualDamage_PreservesRawInjury(int rawInjury)
        {
            BattleFirstBodyResponseResult result = Resolve(
                Encode(0, 999, 999, 2),
                rawInjury: rawInjury);

            Assert.That(result.ApplyManualDamage, Is.True);
            Assert.That(result.ManualDamage, Is.EqualTo(rawInjury));
        }

        [Test]
        public void RejectedChance_PreservesDecodedFieldsButProjectsNoWrites()
        {
            BattleFirstBodyResponseResult result = Resolve(
                Encode(50, 123, 456, 7),
                rawInjury: 37,
                attackerGroup: 47,
                hasRoll: true,
                roll: 50);

            Assert.That(result.Kind,
                Is.EqualTo(BattleFirstBodyResponseKind.EncodedChanceRejected));
            Assert.That(result.TargetAction, Is.EqualTo(123));
            Assert.That(result.AttackerAction, Is.EqualTo(456));
            Assert.That(result.Effect, Is.EqualTo(7));
            Assert.That(result.WriteTargetAction, Is.False);
            Assert.That(result.WriteAttackerAction, Is.False);
            Assert.That(result.WriteTargetGroup, Is.False);
            Assert.That(result.ApplyHold, Is.False);
            Assert.That(result.ApplyManualDamage, Is.False);
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            _ = Resolve(Encode(50, 123, 456, 7), hasRoll: true, roll: 10);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 100000; index++)
            {
                BattleFirstBodyResponseResult result = Resolve(
                    Encode(index % 100, index % 1000, (index * 3) % 1000, index % 10),
                    rawInjury: index,
                    attackerGroup: index % 17,
                    hasRoll: true,
                    roll: index % 100);
                checksum = unchecked(checksum * 31 + (int)result.Kind);
                checksum = unchecked(checksum * 31 + result.TargetAction);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static BattleFirstBodyResponseResult Resolve(
            int firstBodyKind,
            int firstBodyRespond = 0,
            int rawInjury = 0,
            int attackerGroup = 0,
            bool hasRoll = false,
            int roll = 0)
        {
            return BattleFirstBodyResponseResolver.Resolve(
                firstBodyKind,
                firstBodyRespond,
                rawInjury,
                attackerGroup,
                hasRoll,
                roll);
        }

        private static int Encode(
            int chance,
            int targetAction,
            int attackerAction,
            int effect)
        {
            return 1000000000 +
                   chance * 10000000 +
                   targetAction * 10000 +
                   attackerAction * 10 +
                   effect;
        }
    }
}
#endif
