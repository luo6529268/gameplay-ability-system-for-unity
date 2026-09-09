#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.App;
using NTSD.Input;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28InputPhaseCadenceEditorTests
    {
        [Test]
        public void TwoTu_HoldsCurrentUntilPhaseZeroForPressAndRelease()
        {
            var input = new NTSDInputStateModule();
            var buffer = new SimInputBuffer();
            buffer.EnqueueCompletePacketKeyForTick(1, FuncKeyMask.right, true);

            input.UpdateFromBuffer(buffer, 1, null, updateCurrent: false);
            Assert.That(input.Right, Is.False);
            Assert.That(input.PreviousRight, Is.False);

            input.UpdateFromBuffer(buffer, 2, null, updateCurrent: true);
            Assert.That(input.Right, Is.True);
            Assert.That(input.PreviousRight, Is.False);

            buffer.EnqueueCompletePacketKeyForTick(3, FuncKeyMask.right, false);
            input.UpdateFromBuffer(buffer, 3, null, updateCurrent: false);
            Assert.That(input.Right, Is.True);
            Assert.That(input.PreviousRight, Is.True);

            input.UpdateFromBuffer(buffer, 4, null, updateCurrent: true);
            Assert.That(input.Right, Is.False);
            Assert.That(input.PreviousRight, Is.True);
        }

        [Test]
        public void WorldInputPhase_DefaultsTwoTu_AndOneTuForcesZero()
        {
            var world = new SimulationWorld();

            world.AdvanceBattleFlowTick(1);
            Assert.That(world.InputPhase, Is.EqualTo(1));
            world.AdvanceBattleFlowTick(2);
            Assert.That(world.InputPhase, Is.Zero);

            world.SetOneTuInputForBattle(true);
            world.AdvanceBattleFlowTick(3);
            Assert.That(world.InputPhase, Is.Zero);
            world.AdvanceBattleFlowTick(4);
            Assert.That(world.InputPhase, Is.Zero);
            Assert.That(world.OneTuInput, Is.True);

            world.ResetRuntimeState();
            Assert.That(world.OneTuInput, Is.False);
            Assert.That(world.InputPhase, Is.Zero);
        }

        [Test]
        public void MatchConfig_DefaultsTwoTu_AndCarriesExplicitOneTu()
        {
            var defaultConfig = new MatchConfig();
            var oneTuConfig = new MatchConfig { oneTuInput = true };

            Assert.That(defaultConfig.oneTuInput, Is.False);
            Assert.That(oneTuConfig.oneTuInput, Is.True);
        }

        [Test]
        public void SnapshotRestore_PreservesOneTuModeAndCurrentPhase()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var source = new SimulationWorld();
            source.SetOneTuInputForBattle(true);
            source.AdvanceBattleFlowTick(1);
            BattleStateSnapshotBuffer snapshot =
                source.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(
                source.TryCaptureBattleStateSnapshot(identity, 1, snapshot),
                Is.True);

            source.SetOneTuInputForBattle(false);
            source.AdvanceBattleFlowTick(2);
            Assert.That(source.InputPhase, Is.EqualTo(1));

            Assert.That(
                source.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());
            Assert.That(source.OneTuInput, Is.True);
            Assert.That(source.InputPhase, Is.Zero);
        }

        [Test]
        public void RuntimeChecksum_TracksOneTuModeIndependentlyOfPhase()
        {
            var world = new SimulationWorld();
            ulong twoTu = world.CaptureRuntimeChecksum64(0, null);

            world.SetOneTuInputForBattle(true);
            ulong oneTu = world.CaptureRuntimeChecksum64(0, null);

            Assert.That(oneTu, Is.Not.EqualTo(twoTu));
            Assert.That(world.InputPhase, Is.Zero);
        }
    }
}
#endif
