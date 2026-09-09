#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5StandardHitRestPureResolverEditorTests
    {
        [TestCase(0, 3, -3)]
        [TestCase(1, 7, -3)]
        [TestCase(2, 3, -7)]
        [TestCase(3, 7, -7)]
        public void Recover_SelectivelySuppressesHoldWrites(
            int recover,
            int expectedAttackerHold,
            int expectedTargetHold)
        {
            object result = Resolve(7, -7, recover, 0, 0, 0, 0, 0);

            Assert.That(Value(result, "AttackerHold"),
                Is.EqualTo(expectedAttackerHold));
            Assert.That(Value(result, "TargetHold"),
                Is.EqualTo(expectedTargetHold));
        }

        [TestCase(2, 0, 7, -3)]
        [TestCase(3, 0, 7, -3)]
        [TestCase(0, 2, 3, -7)]
        [TestCase(0, 4, 3, -7)]
        [TestCase(2, 2, 7, -7)]
        public void DefinitionEffects_SelectivelySuppressHoldWrites(
            int attackerEffect,
            int targetEffect,
            int expectedAttackerHold,
            int expectedTargetHold)
        {
            object result = Resolve(
                7,
                -7,
                0,
                attackerEffect,
                targetEffect,
                0,
                0,
                0);

            Assert.That(Value(result, "AttackerHold"),
                Is.EqualTo(expectedAttackerHold));
            Assert.That(Value(result, "TargetHold"),
                Is.EqualTo(expectedTargetHold));
        }

        [TestCase(0, 3, -3, 10, 10)]
        [TestCase(2, 1, -1, 8, 8)]
        [TestCase(3, 0, 0, 7, 7)]
        [TestCase(5, 0, 0, 5, 5)]
        public void TimingReduction_AppliesNativeClamps(
            int reduction,
            int expectedAttackerHold,
            int expectedTargetHold,
            int expectedArest,
            int expectedVrest)
        {
            object result = Resolve(9, -9, 0, 0, 0, 10, 10, reduction);

            Assert.That(Value(result, "AttackerHold"),
                Is.EqualTo(expectedAttackerHold));
            Assert.That(Value(result, "TargetHold"),
                Is.EqualTo(expectedTargetHold));
            Assert.That(Value(result, "Arest"), Is.EqualTo(expectedArest));
            Assert.That(Bool(result, "ShouldWriteVrest"), Is.True);
            Assert.That(Value(result, "Vrest"), Is.EqualTo(expectedVrest));
        }

        [TestCase(3, 0, 5, 4, false, 0)]
        [TestCase(4, 0, 5, 1, false, 0)]
        [TestCase(1, 1, 5, 1, true, 1)]
        [TestCase(9, 256, 0, 9, true, 0)]
        [TestCase(9, 257, 0, 9, true, 1)]
        [TestCase(9, 258, 0, 9, true, 2)]
        [TestCase(9, -1, 0, 9, false, 0)]
        public void ArestAndVrest_RespectSpecialCaseAndNativeByteInput(
            int arest,
            int vrest,
            int reduction,
            int expectedArest,
            bool expectedWrite,
            int expectedVrest)
        {
            object result = Resolve(0, 0, 3, 0, 0, arest, vrest, reduction);

            Assert.That(Value(result, "Arest"), Is.EqualTo(expectedArest));
            Assert.That(Bool(result, "ShouldWriteVrest"),
                Is.EqualTo(expectedWrite));
            Assert.That(Value(result, "Vrest"), Is.EqualTo(expectedVrest));
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            _ = BattleStandardHitRestResolver.Resolve(
                7, -7, 0, 0, 0, 10, 17, 2);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                BattleStandardHitRestResult result =
                    BattleStandardHitRestResolver.Resolve(
                        7,
                        -7,
                        index & 3,
                        index & 7,
                        (index >> 1) & 7,
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
            int recover,
            int attackerDefinitionEffect,
            int targetDefinitionEffect,
            int arest,
            int vrest,
            int timingReduction)
        {
            Type resolver = Type.GetType(
                "NTSD.Simulation.Ecs.BattleStandardHitRestResolver, Assembly-CSharp");
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
                    recover,
                    attackerDefinitionEffect,
                    targetDefinitionEffect,
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
