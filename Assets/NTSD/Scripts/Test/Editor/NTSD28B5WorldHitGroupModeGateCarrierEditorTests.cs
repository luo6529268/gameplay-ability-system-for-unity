#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B5WorldHitGroupModeGateCarrierEditorTests
    {
        [Test]
        public void RuntimeRoot_DefaultResetAndRestoreOwnModeGate18()
        {
            var runtime = new BattleRuntimeState();

            Assert.That(runtime.NativeHitResourceRules.ActiveModeHitGroupGate18,
                Is.Zero);

            runtime.NativeHitResourceRules.RestoreForSnapshot(3, 37, 61, 62);
            Assert.That(runtime.NativeHitResourceRules.ActiveModeHitGroupGate18,
                Is.EqualTo(3));

            runtime.Reset();
            Assert.That(runtime.NativeHitResourceRules.ActiveModeHitGroupGate18,
                Is.Zero);
        }

        [Test]
        public void CoreScalarSnapshot_FreezesModeGate18()
        {
            var world = new SimulationWorld();
            world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 = 3;
            var snapshot = new BattleWorldCoreScalarSnapshot(
                world,
                StrictDelayedInputBufferEditorTests.CreateIdentity());

            world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 = 0;

            Assert.That(snapshot.HitResourceRules.ActiveModeHitGroupGate18,
                Is.EqualTo(3));
        }

        [Test]
        public void ModeGate18_ChangesChecksumAndAppearsInFullParity()
        {
            var world = new SimulationWorld();
            ulong baseline = world.CaptureRuntimeChecksum64(0, null);

            world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 = 1;
            ulong changed = world.CaptureRuntimeChecksum64(0, null);
            string parity = world.CaptureParityFrameSnapshot(0).ToJson(full: true);

            Assert.That(changed, Is.Not.EqualTo(baseline));
            Assert.That(parity,
                Does.Contain("\"activeModeHitGroupGate18\":1"));
        }

        [Test]
        public void WorldSchemas_AdvanceWithoutEntityRuntimeSchema()
        {
            Assert.That(BattleWorldCoreScalarSnapshot.CurrentSchemaVersion,
                Is.EqualTo(11));
            Assert.That(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(12));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(20));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion,
                Is.EqualTo(23));
        }

        [Test]
        public void WarmScalarCaptureAndChecksum_DoNotAllocate()
        {
            var world = new SimulationWorld();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 = 3;
            _ = new BattleWorldCoreScalarSnapshot(world, identity);
            _ = world.CaptureRuntimeChecksum64(0, null);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            ulong checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                _ = new BattleWorldCoreScalarSnapshot(world, identity);
                checksum ^= world.CaptureRuntimeChecksum64(index, null);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }
    }
}
#endif
