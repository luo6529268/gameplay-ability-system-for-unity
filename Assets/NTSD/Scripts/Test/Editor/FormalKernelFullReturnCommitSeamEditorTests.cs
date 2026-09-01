#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class FormalKernelFullReturnCommitSeamEditorTests
    {
        [Test]
        public void FullTailReturnPublishesExactlyOneHostResult()
        {
            InProcessBattleKernelHost host = CreateHost();

            Assert.That(host.TryStepOneTick(Frame(1)), Is.True);
            Assert.That(host.Status, Is.EqualTo(InProcessBattleKernelHostStatus.Advanced));
            Assert.That(host.CurrentTick, Is.EqualTo(1));
            Assert.That(host.Journal.Count, Is.EqualTo(1));
            Assert.That(host.FrameHistory.Count, Is.EqualTo(1));
            Assert.That(host.ChecksumHistory.Count, Is.EqualTo(1));
            Assert.That(host.LastInputHash, Is.Not.Zero);
            Assert.That(host.LastStateChecksum, Is.Not.Zero);
        }

        [Test]
        public void BattleEntryInputClearReturnPublishesNoCompletedHostResult()
        {
            InProcessBattleKernelHost host = CreateHost();
            host.WorldForDiagnostics.SetNeedClearInput(true);

            AssertRejectedWithoutPublication(host);
        }

        [Test]
        public void StepWaitReturnPublishesNoCompletedHostResult()
        {
            InProcessBattleKernelHost host = CreateHost();
            host.WorldForDiagnostics.Runtime.Flow.BattleStepMode = 1;

            AssertRejectedWithoutPublication(host);
        }

        private static void AssertRejectedWithoutPublication(
            InProcessBattleKernelHost host)
        {
            Assert.That(host.TryStepOneTick(Frame(1)), Is.False);
            Assert.That(host.Status, Is.EqualTo(InProcessBattleKernelHostStatus.Faulted));
            Assert.That(host.LastReason, Is.EqualTo(LockstepProtocolReason.DriverRejectedFrame));
            Assert.That(host.CurrentTick, Is.Zero);
            Assert.That(host.Journal.Count, Is.Zero);
            Assert.That(host.FrameHistory.Count, Is.Zero);
            Assert.That(host.ChecksumHistory.Count, Is.Zero);
            Assert.That(host.LastInputHash, Is.Zero);
            Assert.That(host.LastStateChecksum, Is.Zero);
            Assert.That(host.TryStepOneTick(Frame(1)), Is.False);
        }

        private static InProcessBattleKernelHost CreateHost()
        {
            var identity = new LockstepSessionIdentity(
                LockstepSessionIdentity.CurrentSchemaVersion,
                sessionId: 0x51000002UL,
                seed: 0x51A7u,
                catalogFingerprint: 0xCA7A10UL,
                stageFingerprint: 0x57A6EUL,
                playerSlots: new[] { 1, 0 });
            var barrier = new LockstepStartBarrier(
                identity,
                ruleFingerprint: 0xC0DE0001UL,
                policyVersion: 1,
                BattleRuntimeProfilePolicy.Create(BattleRuntimeProfile.Authority400));
            return new InProcessBattleKernelHost(barrier, 0, 4);
        }

        private static FrameInputSet Frame(int tick)
        {
            return new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.Right),
                new SimulationPlayerInput(1, SimulationInputButtons.Defend),
            });
        }
    }
}
#endif
