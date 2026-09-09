#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ReducedHitDamagePureCoreEditorTests
    {
        [TestCase(false, 0, 49, 4)]
        [TestCase(true, 4, 40, 4)]
        [TestCase(false, 0, -49, -4)]
        [TestCase(false, 0, 9, 0)]
        [TestCase(false, 0, -9, 0)]
        public void NullAndType4_UseNativeSignedDivideByTen(
            bool hasArmor,
            int armorType,
            int injury,
            int expectedHpDamage)
        {
            object result = Resolve(
                injury, hasArmor, armorType, 0, 0, 0, 0);

            Assert.That(Bool(result, "Supported"), Is.True);
            Assert.That(Value(result, "HpDamage"), Is.EqualTo(expectedHpDamage));
            Assert.That(Value(result, "MpDamage"), Is.Zero);
            Assert.That(Value(result, "RuntimeArmorHpDelta"), Is.Zero);
        }

        [Test]
        public void Type3_IsExplicitlyUnsupportedAndDoesNotApproximate()
        {
            object result = Resolve(80, true, 3, 50, 25, 1, 20);

            Assert.That(Bool(result, "Supported"), Is.False);
            Assert.That(Value(result, "HpDamage"), Is.Zero);
            Assert.That(Value(result, "MpDamage"), Is.Zero);
            Assert.That(Value(result, "RuntimeArmorHpDelta"), Is.Zero);
        }

        [TestCase(41, 50, 20)]
        [TestCase(999, -7, 7)]
        [TestCase(999, 0, 0)]
        public void Decrease_UsesPercentageOrFixedAbsoluteDamage(
            int injury,
            int decrease,
            int expectedHpDamage)
        {
            object result = Resolve(injury, true, 1, decrease, 0, 0, 0);

            Assert.That(Bool(result, "Supported"), Is.True);
            Assert.That(Value(result, "HpDamage"), Is.EqualTo(expectedHpDamage));
        }

        [TestCase(80, 1, -80)]
        [TestCase(-80, 1, 80)]
        [TestCase(80, 0, 0)]
        public void PositiveArmorHp_TracksNegativeRawInjuryDelta(
            int injury,
            int armorHp,
            int expectedDelta)
        {
            object result = Resolve(injury, true, 1, 50, 0, armorHp, 0);

            Assert.That(Value(result, "RuntimeArmorHpDelta"),
                Is.EqualTo(expectedDelta));
        }

        [TestCase(80, 50, 25, 0, 10)]
        [TestCase(80, 50, -12, 0, 12)]
        [TestCase(80, 50, 0, 40, 0)]
        public void ArmorMp_DivertsReducedDamageFromHp(
            int injury,
            int decrease,
            int armorMp,
            int expectedHpDamage,
            int expectedMpDamage)
        {
            object result = Resolve(
                injury, true, 1, decrease, armorMp, 0, 0);

            Assert.That(Value(result, "HpDamage"), Is.EqualTo(expectedHpDamage));
            Assert.That(Value(result, "MpDamage"), Is.EqualTo(expectedMpDamage));
        }

        [TestCase(49, false, 0, 0, 0, 50, 8, 0)]
        [TestCase(80, true, 1, 50, 0, 25, 160, 0)]
        [TestCase(80, true, 1, 50, 25, 25, 0, 10)]
        [TestCase(80, true, 1, 50, 0, 0, 40, 0)]
        public void TargetScale_AppliesOnlyToResolvedHpDamage(
            int injury,
            bool hasArmor,
            int armorType,
            int decrease,
            int armorMp,
            int targetScale,
            int expectedHpDamage,
            int expectedMpDamage)
        {
            object result = Resolve(
                injury,
                hasArmor,
                armorType,
                decrease,
                armorMp,
                0,
                targetScale);

            Assert.That(Value(result, "HpDamage"), Is.EqualTo(expectedHpDamage));
            Assert.That(Value(result, "MpDamage"), Is.EqualTo(expectedMpDamage));
        }

        [Test]
        public void TargetScale_PreservesNativeLow32BitProduct()
        {
            object result = Resolve(
                int.MaxValue, false, 0, 0, 0, 0, 3);

            Assert.That(Value(result, "HpDamage"), Is.EqualTo(-26));
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            _ = BattleReducedHitDamageResolver.Resolve(
                80, true, 1, 50, 25, 1, 20);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                BattleReducedHitDamageResult result =
                    BattleReducedHitDamageResolver.Resolve(
                        index - 2048,
                        (index & 1) != 0,
                        index % 5,
                        (index % 201) - 100,
                        (index % 101) - 50,
                        index & 3,
                        index % 131);
                checksum = unchecked(checksum * 31 +
                    (result.Supported ? 1 : 0));
                checksum = unchecked(checksum * 31 + result.HpDamage);
                checksum = unchecked(checksum * 31 + result.MpDamage);
                checksum = unchecked(checksum * 31 +
                    result.RuntimeArmorHpDelta);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static object Resolve(
            int baseInjury,
            bool hasSelectedArmor,
            int armorType,
            int armorDecrease,
            int armorMp,
            int armorHp,
            int targetDamageScale)
        {
            Type resolver = Type.GetType(
                "NTSD.Simulation.Ecs.BattleReducedHitDamageResolver, Assembly-CSharp");
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
                    baseInjury,
                    hasSelectedArmor,
                    armorType,
                    armorDecrease,
                    armorMp,
                    armorHp,
                    targetDamageScale,
                });
        }

        private static int Value(object result, string name)
        {
            return (int)result.GetType().GetProperty(name).GetValue(result);
        }

        private static bool Bool(object result, string name)
        {
            return (bool)result.GetType().GetProperty(name).GetValue(result);
        }
    }
}
#endif
