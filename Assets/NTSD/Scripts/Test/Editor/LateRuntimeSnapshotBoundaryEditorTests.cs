#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class LateRuntimeSnapshotBoundaryEditorTests
    {
        [Test]
        public void RecoveryHpZeroStillReachesFrameAndDeath()
        {
            int[] result = Capture(mode: 0);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.EqualTo(1));
            Assert.That(result[6], Is.EqualTo(1));
            Assert.That(result[7], Is.EqualTo(1));
            Assert.That(result[8], Is.Zero);
            Assert.That(result[9], Is.EqualTo(1));
            Assert.That(result[10], Is.Zero);
        }

        [Test]
        public void SuppressedFrameTickUsesSinglePostBranchSnapshot()
        {
            int[] result = Capture(mode: 1);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.EqualTo(1));
            Assert.That(result[7], Is.Zero);
        }

        [Test]
        public void CompletedCleanupKeepsContinueBoundaryWithoutSnapshot()
        {
            int[] result = Capture(mode: 2);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.Zero);
            Assert.That(result[11], Is.EqualTo(1));
            Assert.That(result[12], Is.Zero);
        }

        [Test]
        public void DepletedWeaponStillPlainFreesWithoutCleanupSnapshot()
        {
            int[] result = Capture(mode: 3);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.Zero);
            Assert.That(result[13], Is.Zero);
            Assert.That(result[14], Is.EqualTo(1));
        }

        private static void AssertRemovedStages(int[] result)
        {
            Assert.That(result[0], Is.Zero);
            Assert.That(result[1], Is.Zero);
            Assert.That(result[2], Is.Zero);
        }

        private static int[] Capture(int mode)
        {
            var world = new SimulationWorld();
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "CaptureLateRuntimeSnapshotBoundaryForSelfCheck",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (int[])method.Invoke(world, new object[] { mode });
        }
    }
}
#endif
