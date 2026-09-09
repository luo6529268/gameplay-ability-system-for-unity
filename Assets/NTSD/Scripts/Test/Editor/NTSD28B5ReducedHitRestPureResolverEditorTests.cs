#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ReducedHitRestPureResolverEditorTests
    {
        [TestCase(false, 0)]
        [TestCase(true, -1)]
        public void DefaultDelay_OverwritesBothHoldsAndAppliesReduction(
            bool hasArmor,
            int delay)
        {
            object result = Resolve(-9, 7, 0, 0, hasArmor, delay, 10, 10, 2);

            Assert.That(Value(result, "AttackerHold"), Is.EqualTo(1));
            Assert.That(Value(result, "TargetHold"), Is.EqualTo(-3));
            Assert.That(Bool(result, "ClearTargetFrameCounter"), Is.False);
        }

        [Test]
        public void DefinitionEffects_SuppressOnlyOwnedDefaultHoldWrites()
        {
            object result = Resolve(-9, 7, 3, 4, false, 0, 10, 10, 2);

            Assert.That(Value(result, "AttackerHold"), Is.EqualTo(-9));
            Assert.That(Value(result, "TargetHold"), Is.EqualTo(7));
        }

        [TestCase(1203, 17, 7, true)]
        [TestCase(11203, 17, 7, false)]
        [TestCase(-11203, -7, 13, false)]
        [TestCase(0, 5, 10, true)]
        public void PackedDelay_UsesSignedBase100Segments(
            int delay,
            int expectedAttacker,
            int expectedTarget,
            bool expectedClear)
        {
            object result = Resolve(5, 10, 3, 4, true, delay, 10, 10, 5);

            Assert.That(Value(result, "AttackerHold"),
                Is.EqualTo(expectedAttacker));
            Assert.That(Value(result, "TargetHold"),
                Is.EqualTo(expectedTarget));
            Assert.That(Bool(result, "ClearTargetFrameCounter"),
                Is.EqualTo(expectedClear));
        }

        [TestCase(3, 0, 4, false, 0)]
        [TestCase(20, 0, 12, false, 0)]
        [TestCase(9, 258, 9, true, 4)]
        [TestCase(13, 255, 12, true, 12)]
        [TestCase(9, 256, 9, true, 4)]
        [TestCase(9, -1, 9, false, 0)]
        public void DirectRests_UseFourTwelveAndNativeByteRules(
            int arest,
            int vrest,
            int expectedArest,
            bool expectedWrite,
            int expectedVrest)
        {
            object result = Resolve(0, 0, 0, 0, false, 0, arest, vrest, 5);

            Assert.That(Value(result, "Arest"), Is.EqualTo(expectedArest));
            Assert.That(Bool(result, "ShouldWriteVrest"),
                Is.EqualTo(expectedWrite));
            Assert.That(Value(result, "Vrest"), Is.EqualTo(expectedVrest));
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            _ = BattleReducedHitRestResolver.Resolve(
                5, 10, 0, 0, true, 11203, 10, 258, 2);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                BattleReducedHitRestResult result =
                    BattleReducedHitRestResolver.Resolve(
                        index & 15,
                        index & 31,
                        index & 7,
                        (index >> 1) & 7,
                        (index & 1) != 0,
                        index - 2048,
                        index & 31,
                        index,
                        index % 6);
                checksum = unchecked(checksum * 31 + result.AttackerHold);
                checksum = unchecked(checksum * 31 + result.TargetHold);
                checksum = unchecked(checksum * 31 + result.Arest);
                checksum = unchecked(checksum * 31 + result.Vrest);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static object Resolve(
            int currentAttackerHold,
            int currentTargetHold,
            int attackerDefinitionEffect,
            int targetDefinitionEffect,
            bool hasSelectedArmor,
            int selectedArmorDelay,
            int arest,
            int vrest,
            int timingReduction)
        {
            Type resolver = Type.GetType(
                "NTSD.Simulation.Ecs.BattleReducedHitRestResolver, Assembly-CSharp");
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
                    currentAttackerHold,
                    currentTargetHold,
                    attackerDefinitionEffect,
                    targetDefinitionEffect,
                    hasSelectedArmor,
                    selectedArmorDelay,
                    arest,
                    vrest,
                    timingReduction,
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
