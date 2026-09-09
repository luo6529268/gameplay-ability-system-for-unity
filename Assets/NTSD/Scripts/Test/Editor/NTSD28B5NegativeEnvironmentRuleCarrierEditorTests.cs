#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B5NegativeEnvironmentRuleCarrierEditorTests
    {
        [Test]
        public void RuntimeRoot_DefaultResetAndRawRestoreAreExact()
        {
            var runtime = new BattleRuntimeState();

            Assert.That(
                NTSD28HitResourceRulesRuntimeState
                    .DefaultNegativeEnvironmentDamage90,
                Is.EqualTo(9));
            Assert.That(runtime.NativeHitResourceRules.NegativeEnvironmentDamage90,
                Is.EqualTo(9));

            runtime.NativeHitResourceRules.RestoreForSnapshot(
                18, 19, 20, 21, -4);
            Assert.That(runtime.NativeHitResourceRules.NegativeEnvironmentDamage90,
                Is.EqualTo(-4));

            runtime.Reset();
            Assert.That(runtime.NativeHitResourceRules.NegativeEnvironmentDamage90,
                Is.EqualTo(9));
        }

        [Test]
        public void CoreScalarSnapshot_OwnsImmutableRawRuleValue()
        {
            var world = new SimulationWorld();
            world.Runtime.NativeHitResourceRules.RestoreForSnapshot(
                18, 19, 20, 21, 12);
            var snapshot = new BattleWorldCoreScalarSnapshot(
                world,
                StrictDelayedInputBufferEditorTests.CreateIdentity());

            world.Runtime.NativeHitResourceRules.Reset();

            Assert.That(snapshot.HitResourceRules.NegativeEnvironmentDamage90,
                Is.EqualTo(12));
        }

        [Test]
        public void FullBattleSnapshotRestore_PreservesRawRuleValue()
        {
            var source = new SimulationWorld();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            source.Runtime.NativeHitResourceRules.RestoreForSnapshot(
                18, 19, 20, 21, 12);
            BattleStateSnapshotBuffer snapshot =
                source.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(
                source.TryCaptureBattleStateSnapshot(identity, 0, snapshot),
                Is.True);

            var destination = new SimulationWorld();
            destination.Runtime.NativeHitResourceRules.RestoreForSnapshot(
                28, 29, 30, 31, -7);
            Assert.That(
                destination.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());

            Assert.That(
                destination.Runtime.NativeHitResourceRules
                    .NegativeEnvironmentDamage90,
                Is.EqualTo(12));
        }

        [Test]
        public void RuleValueChangesChecksumAndAppearsInFullParity()
        {
            var world = new SimulationWorld();
            ulong baseline = world.CaptureRuntimeChecksum64(0, null);

            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 12;
            ulong positiveChanged = world.CaptureRuntimeChecksum64(0, null);
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 0;
            ulong zeroChanged = world.CaptureRuntimeChecksum64(0, null);
            string parity = world.CaptureParityFrameSnapshot(0).ToJson(full: true);

            Assert.That(positiveChanged, Is.Not.EqualTo(baseline));
            Assert.That(zeroChanged, Is.Not.EqualTo(baseline));
            Assert.That(zeroChanged, Is.Not.EqualTo(positiveChanged));
            Assert.That(parity,
                Does.Contain("\"negativeEnvironmentDamage90\":0"));
        }

        [Test]
        public void SnapshotAndChecksumSchemas_AdvanceOnlyWorldDomains()
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
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 12;
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
