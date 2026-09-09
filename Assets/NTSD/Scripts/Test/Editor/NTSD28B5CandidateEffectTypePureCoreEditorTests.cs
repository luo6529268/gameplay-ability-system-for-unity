#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5CandidateEffectTypePureCoreEditorTests
    {
        [TestCase(13, 0, true)]
        [TestCase(13, 1, false)]
        [TestCase(13, 3, false)]
        [TestCase(13, 6, false)]
        [TestCase(14, 3, true)]
        [TestCase(14, 0, false)]
        [TestCase(14, 2, false)]
        [TestCase(14, 6, false)]
        [TestCase(15, 0, true)]
        [TestCase(15, 3, true)]
        [TestCase(15, 1, false)]
        [TestCase(15, 5, false)]
        [TestCase(16, 0, false)]
        [TestCase(16, 1, true)]
        [TestCase(16, 2, true)]
        [TestCase(16, 3, true)]
        [TestCase(16, 4, true)]
        [TestCase(16, 5, false)]
        [TestCase(16, 6, true)]
        public void Effect13Through16_UsesNativeTargetTypeMatrix(
            int effect,
            int targetType,
            bool accepted)
        {
            Assert.That(
                BattleHitCandidateEffectTypeResolver.Accepts(
                    effect,
                    targetType),
                Is.EqualTo(accepted));
        }

        [TestCase(-1, -7)]
        [TestCase(0, 0)]
        [TestCase(8, 5)]
        [TestCase(12, 3)]
        [TestCase(17, 99)]
        [TestCase(6500, 0)]
        public void OtherEffects_AreUnrestricted(int effect, int targetType)
        {
            Assert.That(
                BattleHitCandidateEffectTypeResolver.Accepts(
                    effect,
                    targetType),
                Is.True);
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            _ = BattleHitCandidateEffectTypeResolver.Accepts(16, 3);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                bool accepted = BattleHitCandidateEffectTypeResolver.Accepts(
                    index % 23 - 3,
                    index % 10 - 2);
                checksum = unchecked(checksum * 31 + (accepted ? 1 : 0));
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }
    }
}
#endif

