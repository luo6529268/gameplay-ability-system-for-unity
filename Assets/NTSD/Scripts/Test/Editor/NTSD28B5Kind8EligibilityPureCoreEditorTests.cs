#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Kind8EligibilityPureCoreEditorTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void ExactSelector0Through6_AcceptsMatchingType(int objectType)
        {
            AssertKind(
                Resolve(objectType, 0, objectType, 1, 2, -1, -1, 0),
                BattleKind8EligibilityKind.Accepted);
        }

        [Test]
        public void ExactSelector_RejectsDifferentType()
        {
            AssertKind(
                Resolve(3, 0, 0, 1, 2, -1, -1, 0),
                BattleKind8EligibilityKind.TargetTypeRejected);
        }

        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(4, true)]
        [TestCase(6, true)]
        [TestCase(0, false)]
        [TestCase(3, false)]
        [TestCase(5, false)]
        public void Selector7_AcceptsOnlyNativeWeaponGroup(
            int objectType,
            bool accepted)
        {
            AssertKind(
                Resolve(7, 0, objectType, 1, 2, -1, -1, 0),
                accepted
                    ? BattleKind8EligibilityKind.Accepted
                    : BattleKind8EligibilityKind.TargetTypeRejected);
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(6)]
        public void Selector8_IsUnrestricted(int objectType)
        {
            AssertKind(
                Resolve(8, 0, objectType, 1, 2, -1, -1, 0),
                BattleKind8EligibilityKind.Accepted);
        }

        [TestCase(-1)]
        [TestCase(9)]
        public void InvalidTypeSelector_Rejects(int selector)
        {
            AssertKind(
                Resolve(selector, 0, 0, 1, 2, -1, -1, 0),
                BattleKind8EligibilityKind.TargetTypeRejected);
        }

        [TestCase(0, 1, 2, -1, -1, 0, true)]
        [TestCase(1, 5, 5, -1, -1, 0, true)]
        [TestCase(1, 5, 6, -1, -1, 0, false)]
        [TestCase(2, 5, 6, -1, -1, 0, true)]
        [TestCase(2, 5, 5, -1, -1, 0, false)]
        [TestCase(3, 7, 7, 12, 12, 0, true)]
        [TestCase(3, 7, 7, 12, 13, 0, false)]
        [TestCase(3, 7, 8, 12, 12, 0, false)]
        [TestCase(4, 9, 9, 6, 6, 6, true)]
        [TestCase(4, 9, 9, 6, 6, 2, false)]
        [TestCase(4, 9, 9, 6, 7, 6, false)]
        public void RelationSelector_UsesNativeGroupOwnerModeOrder(
            int selector,
            int attackerGroup,
            int targetGroup,
            int attackerOwner,
            int targetOwner,
            int mode,
            bool accepted)
        {
            AssertKind(
                Resolve(
                    8,
                    selector,
                    0,
                    attackerGroup,
                    targetGroup,
                    attackerOwner,
                    targetOwner,
                    mode),
                accepted
                    ? BattleKind8EligibilityKind.Accepted
                    : BattleKind8EligibilityKind.RelationRejected);
        }

        [TestCase(-1)]
        [TestCase(5)]
        public void InvalidRelationSelector_Rejects(int selector)
        {
            AssertKind(
                Resolve(8, selector, 0, 1, 2, -1, -1, 0),
                BattleKind8EligibilityKind.RelationRejected);
        }

        [Test]
        public void SameZeroGroup_IsStillSameGroup()
        {
            AssertKind(
                Resolve(8, 1, 0, 0, 0, -1, -1, 0),
                BattleKind8EligibilityKind.Accepted);
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            _ = Resolve(8, 4, 3, 9, 9, 6, 6, 6);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                BattleKind8EligibilityResult result = Resolve(
                    index % 11 - 1,
                    index % 8 - 1,
                    index % 7,
                    index % 5,
                    (index >> 1) % 5,
                    index % 9 - 1,
                    (index >> 2) % 9 - 1,
                    index % 7);
                checksum = unchecked(checksum * 31 + (int)result.Kind);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static BattleKind8EligibilityResult Resolve(
            int targetTypeSelector,
            int relationSelector,
            int targetObjectType,
            int attackerBattleGroup,
            int targetBattleGroup,
            int attackerOwnerSlot,
            int targetOwnerSlot,
            int battleModeContext)
        {
            return BattleKind8EligibilityResolver.Resolve(
                targetTypeSelector,
                relationSelector,
                targetObjectType,
                attackerBattleGroup,
                targetBattleGroup,
                attackerOwnerSlot,
                targetOwnerSlot,
                battleModeContext);
        }

        private static void AssertKind(
            BattleKind8EligibilityResult result,
            BattleKind8EligibilityKind expected)
        {
            Assert.That(result.Kind, Is.EqualTo(expected));
            Assert.That(result.Accepted, Is.EqualTo(
                expected == BattleKind8EligibilityKind.Accepted));
        }
    }
}
#endif

