#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class SimulationTickHostPolicyEditorTests
    {
        [Test]
        public void OfflineLocal_ExecutesAtMostOneAutomaticTickPerUpdate()
        {
            var settings = new LockstepSimulationSettings
            {
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
            };
            settings.Normalize();
            var policy = new OfflineLocalTickPolicy();

            policy.BeginUpdate(SimulationConstants.SIM_DT * 4f, settings);

            Assert.That(policy.ShouldAttemptAutomaticTick(0, settings), Is.True);
            Assert.That(
                policy.ShouldBuildPresentationForNextTick(0, settings),
                Is.True);
            policy.CommitAutomaticTick();
            Assert.That(policy.ShouldAttemptAutomaticTick(1, settings), Is.False);
            Assert.That(
                policy.Accumulator,
                Is.GreaterThanOrEqualTo(SimulationConstants.SIM_DT * 2.99f));

            policy.BeginUpdate(0f, settings);
            Assert.That(
                policy.ShouldAttemptAutomaticTick(0, settings),
                Is.True,
                "the remaining backlog may advance on the next Unity Update");
        }

        [Test]
        public void ManualAndNetworkPolicies_NeverConsumeWallClockAutomatically()
        {
            var settings = new LockstepSimulationSettings();
            SimulationTickHostPolicy[] policies =
            {
                new ManualReplayTickPolicy(),
                new NetworkLockstepTickPolicy(),
            };

            foreach (SimulationTickHostPolicy policy in policies)
            {
                policy.BeginUpdate(10f, settings);
                Assert.That(policy.UsesWallClock, Is.False);
                Assert.That(policy.Accumulator, Is.Zero);
                Assert.That(
                    policy.ShouldAttemptAutomaticTick(0, settings),
                    Is.False);
                Assert.That(
                    policy.ShouldBuildPresentationForNextTick(0, settings),
                    Is.True);
            }
        }
    }
}
#endif
