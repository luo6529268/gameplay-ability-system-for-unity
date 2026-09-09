#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5OrdinaryDefensePureResolverEditorTests
    {
        [TestCase(1, 0, 7, 100)]
        [TestCase(0, 61, 7, 100)]
        [TestCase(0, 0, 8, 100)]
        [TestCase(0, 0, 7, 0)]
        public void FrontGates_ReturnInactive(
            int kind,
            int effect,
            int defenderState,
            int defenderHp)
        {
            object result = Resolve(
                kind, effect, 0, 0, 0, 1, 1, defenderState, defenderHp, 1);

            Assert.That(Decision(result), Is.EqualTo("Inactive"));
            Assert.That(UsedTwoWay(result), Is.False);
        }

        [TestCase(70)]
        [TestCase(75)]
        public void ExtendedDefenseStates_ApplyWithoutState7DirectionalGate(
            int defenderState)
        {
            object result = Resolve(
                0, 60, 0, 0, 0, 1, 1, defenderState, 1, 1);

            Assert.That(Decision(result), Is.EqualTo("Applies"));
            Assert.That(UsedTwoWay(result), Is.False);
        }

        [TestCase(1, 0, 0, 0, 1, false)]
        [TestCase(1, 1, 1, 0, 1, false)]
        [TestCase(1, 1, -1, 0, 1, false)]
        [TestCase(1, 1, 0, 1, 1, false)]
        [TestCase(1, 1, 0, 0, -1, false)]
        [TestCase(1, 1, 0, 0, 1, true)]
        public void State7_EachNativeTriggerApplies(
            int attackerFacing,
            int defenderFacing,
            int spark,
            int dbdefend,
            int dvx,
            bool useTwoWayOid)
        {
            object result = Resolve(
                0,
                0,
                spark,
                dbdefend,
                dvx,
                attackerFacing,
                defenderFacing,
                7,
                100,
                useTwoWayOid ? 822 : 1);

            Assert.That(Decision(result), Is.EqualTo("Applies"));
            Assert.That(UsedTwoWay(result), Is.EqualTo(useTwoWayOid));
        }

        [Test]
        public void ProtectedState7WithoutOid822_BypassesToArmorOrUnarmored()
        {
            object result = Resolve(0, 60, 0, 0, 0, 1, 1, 7, 100, 821);

            Assert.That(Decision(result), Is.EqualTo("Bypassed"));
            Assert.That(UsedTwoWay(result), Is.False);
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            _ = BattleOrdinaryDefenseResolver.Resolve(
                0, 0, 0, 0, 0, 1, 1, 7, 100, 822);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                BattleOrdinaryDefenseResult result =
                    BattleOrdinaryDefenseResolver.Resolve(
                        index & 1,
                        index & 127,
                        index,
                        index & 3,
                        (index & 7) - 4,
                        index & 1,
                        (index >> 1) & 1,
                        index % 3 == 0 ? 7 : index % 3 == 1 ? 70 : 75,
                        index & 127,
                        index % 17 == 0 ? 822 : index);
                checksum = unchecked(
                    checksum * 31 + (int)result.Decision);
                checksum = unchecked(
                    checksum * 31 +
                    (result.UsedTwoWayDefenseObjectId ? 1 : 0));
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static object Resolve(
            int kind,
            int effect,
            int spark,
            int dbdefend,
            int dvx,
            int attackerFacing,
            int defenderFacing,
            int defenderState,
            int defenderHp,
            int attackerObjectId)
        {
            Type resolver = Type.GetType(
                "NTSD.Simulation.Ecs.BattleOrdinaryDefenseResolver, Assembly-CSharp");
            Assert.That(resolver, Is.Not.Null);
            MethodInfo method = resolver.GetMethod(
                "Resolve",
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(
                null,
                new object[]
                {
                    kind,
                    effect,
                    spark,
                    dbdefend,
                    dvx,
                    attackerFacing,
                    defenderFacing,
                    defenderState,
                    defenderHp,
                    attackerObjectId,
                });
        }

        private static string Decision(object result)
        {
            return result.GetType().GetProperty("Decision").GetValue(result)
                .ToString();
        }

        private static bool UsedTwoWay(object result)
        {
            return (bool)result.GetType()
                .GetProperty("UsedTwoWayDefenseObjectId").GetValue(result);
        }
    }
}
#endif
