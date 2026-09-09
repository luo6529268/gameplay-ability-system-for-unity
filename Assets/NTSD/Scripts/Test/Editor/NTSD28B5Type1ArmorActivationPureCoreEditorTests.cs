#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type1ArmorActivationPureCoreEditorTests
    {
        [TestCase(9, false)]
        [TestCase(10, true)]
        public void PercentageCost_UsesTwoStagesAndExactMpBoundary(
            int currentMp,
            bool expectedAvailable)
        {
            LF2ArmorData armor = Armor(mp: 25, decrease: 50);

            object result = Resolve(
                armor, 80, 80, currentMp, false, 0);

            AssertResult(result, expectedAvailable, 10, false, false, 0,
                expectedAvailable ? "None" : "InsufficientMp");
        }

        [Test]
        public void NonpositiveFields_AreAbsoluteRatherThanPercentage()
        {
            LF2ArmorData armor = Armor(mp: -7, decrease: -30);

            object result = Resolve(armor, 999, 999, 7, false, 0);

            AssertResult(result, true, 7, false, false, 0, "None");
        }

        [Test]
        public void ZeroComputedMpCost_IsRaisedToOne()
        {
            LF2ArmorData armor = Armor(mp: 25, decrease: 1);

            object result = Resolve(armor, 1, 1, 0, false, 0);

            AssertResult(result, false, 1, false, false, 0,
                "InsufficientMp");
        }

        [Test]
        public void MpBranch_TakesPriorityOverArmorHpRequirements()
        {
            LF2ArmorData armor = Armor(mp: 25, decrease: 50, hp: 100);

            object result = Resolve(armor, 80, 1000, 10, false, 0);

            AssertResult(result, true, 10, false, false, 0, "None");
        }

        [Test]
        public void ZeroMpAndZeroHp_IsAvailableWithoutResourceWrite()
        {
            LF2ArmorData armor = Armor(mp: 0, decrease: 50, hp: 0);

            object result = Resolve(armor, 80, 80, 0, false, 0);

            AssertResult(result, true, 0, false, false, 0, "None");
        }

        [Test]
        public void ArmorHpConfiguredButMissing_IsUnavailableWithoutBreak()
        {
            LF2ArmorData armor = Armor(mp: 0, decrease: 50, hp: 100);

            object result = Resolve(armor, 80, 40, 0, false, 0);

            AssertResult(result, false, 0, false, false, 0,
                "MissingRuntimeArmorHp");
        }

        [TestCase(39)]
        [TestCase(40)]
        public void ArmorHpAtOrBelowEffectiveInjury_BreaksToNegativeOne(
            int runtimeArmorHp)
        {
            LF2ArmorData armor = Armor(mp: 0, decrease: 50, hp: 100);

            object result = Resolve(
                armor, 80, 40, 0, true, runtimeArmorHp);

            AssertResult(result, false, 0, true, true, -1,
                "ArmorHpExhausted");
        }

        [Test]
        public void ArmorHpStrictlyAboveEffectiveInjury_RemainsAvailable()
        {
            LF2ArmorData armor = Armor(mp: 0, decrease: 50, hp: 100);

            object result = Resolve(armor, 80, 40, 0, true, 41);

            AssertResult(result, true, 0, false, false, 0, "None");
        }

        [Test]
        public void PercentageMath_UsesLongIntermediateAndUncheckedIntWriteback()
        {
            LF2ArmorData armor = Armor(
                mp: 100,
                decrease: int.MaxValue);
            int reduced = unchecked((int)(
                ((long)int.MaxValue * int.MaxValue) / 100));
            int expectedCost = unchecked((int)(((long)reduced * 100) / 100));

            object result = Resolve(
                armor, int.MaxValue, 0, int.MaxValue, false, 0);

            AssertResult(result, int.MaxValue >= expectedCost,
                expectedCost == 0 ? 1 : expectedCost,
                false, false, 0,
                int.MaxValue >= (expectedCost == 0 ? 1 : expectedCost)
                    ? "None"
                    : "InsufficientMp");
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            LF2ArmorData armor = Armor(mp: 25, decrease: 50, hp: 100);
            _ = BattleType1ArmorActivationResolver.Resolve(
                armor, 80, 40, 10, true, 41);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 17;
            for (int index = 0; index < 4096; index++)
            {
                armor.mp = index % 3 == 0 ? 25 : 0;
                BattleType1ArmorActivationResult result =
                    BattleType1ArmorActivationResolver.Resolve(
                        armor,
                        index,
                        index & 127,
                        index & 255,
                        (index & 1) == 0,
                        index & 63);
                checksum = unchecked(checksum * 31 + result.MpCost);
                checksum = unchecked(
                    checksum * 31 + (result.Available ? 1 : 0));
                checksum = unchecked(checksum * 31 + (int)result.Reason);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static LF2ArmorData Armor(int mp, int decrease, int hp = 0)
        {
            return new LF2ArmorData
            {
                mp = mp,
                decrease = decrease,
                hp = hp,
            };
        }

        private static object Resolve(
            LF2ArmorData armor,
            int baseInjury,
            int effectiveInjury,
            int currentMp,
            bool hasRuntimeArmorHp,
            int runtimeArmorHp)
        {
            Type resolver = Type.GetType(
                "NTSD.Simulation.Ecs.BattleType1ArmorActivationResolver, Assembly-CSharp");
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
                    armor,
                    baseInjury,
                    effectiveInjury,
                    currentMp,
                    hasRuntimeArmorHp,
                    runtimeArmorHp,
                });
        }

        private static void AssertResult(
            object result,
            bool expectedAvailable,
            int expectedMpCost,
            bool expectedBroken,
            bool expectedHasNextArmorHp,
            int expectedNextArmorHp,
            string expectedReason)
        {
            Type type = result.GetType();
            Assert.That(type.GetProperty("Available").GetValue(result),
                Is.EqualTo(expectedAvailable));
            Assert.That(type.GetProperty("MpCost").GetValue(result),
                Is.EqualTo(expectedMpCost));
            Assert.That(type.GetProperty("ArmorHpBroken").GetValue(result),
                Is.EqualTo(expectedBroken));
            Assert.That(type.GetProperty("HasNextArmorHp").GetValue(result),
                Is.EqualTo(expectedHasNextArmorHp));
            Assert.That(type.GetProperty("NextArmorHp").GetValue(result),
                Is.EqualTo(expectedNextArmorHp));
            Assert.That(type.GetProperty("Reason").GetValue(result).ToString(),
                Is.EqualTo(expectedReason));
        }
    }
}
#endif
