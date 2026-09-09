#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B5WorldHitResourceRulesCarrierEditorTests
    {
        [Test]
        public void RuntimeRoot_DefaultsAndResetRestorePlayableRuleValues()
        {
            var runtime = new BattleRuntimeState();

            Assert.That(runtime.NativeHitResourceRules, Is.Not.Null);
            Assert.That(runtime.NativeHitResourceRules.ActiveModeAttackingPercent1C,
                Is.Zero);
            Assert.That(runtime.NativeHitResourceRules.AttackerInjuryMpPercent34,
                Is.EqualTo(75));
            Assert.That(runtime.NativeHitResourceRules.TargetInjuryMpPercent38,
                Is.EqualTo(75));

            runtime.NativeHitResourceRules.RestoreForSnapshot(37, 61, 62);
            runtime.Reset();

            Assert.That(runtime.NativeHitResourceRules.ActiveModeAttackingPercent1C,
                Is.Zero);
            Assert.That(runtime.NativeHitResourceRules.AttackerInjuryMpPercent34,
                Is.EqualTo(75));
            Assert.That(runtime.NativeHitResourceRules.TargetInjuryMpPercent38,
                Is.EqualTo(75));
        }

        [Test]
        public void CoreScalarSnapshot_OwnsImmutableRuleValues()
        {
            var world = new SimulationWorld();
            world.Runtime.NativeHitResourceRules.RestoreForSnapshot(37, 61, 62);
            var snapshot = new BattleWorldCoreScalarSnapshot(
                world,
                StrictDelayedInputBufferEditorTests.CreateIdentity());

            world.Runtime.NativeHitResourceRules.Reset();

            Assert.That(snapshot.HitResourceRules.ActiveModeAttackingPercent1C,
                Is.EqualTo(37));
            Assert.That(snapshot.HitResourceRules.AttackerInjuryMpPercent34,
                Is.EqualTo(61));
            Assert.That(snapshot.HitResourceRules.TargetInjuryMpPercent38,
                Is.EqualTo(62));
        }

        [Test]
        public void EveryRuleValueChangesChecksumAndAppearsInFullParity()
        {
            var world = new SimulationWorld();
            ulong baseline = world.CaptureRuntimeChecksum64(0, null);

            world.Runtime.NativeHitResourceRules.ActiveModeAttackingPercent1C = 1;
            ulong attackingChanged = world.CaptureRuntimeChecksum64(0, null);
            world.Runtime.NativeHitResourceRules.Reset();
            world.Runtime.NativeHitResourceRules.AttackerInjuryMpPercent34 = 74;
            ulong attackerChanged = world.CaptureRuntimeChecksum64(0, null);
            world.Runtime.NativeHitResourceRules.Reset();
            world.Runtime.NativeHitResourceRules.TargetInjuryMpPercent38 = 73;
            ulong targetChanged = world.CaptureRuntimeChecksum64(0, null);
            world.Runtime.NativeHitResourceRules.RestoreForSnapshot(37, 61, 62);
            string parity = world.CaptureParityFrameSnapshot(0).ToJson(full: true);

            Assert.That(attackingChanged, Is.Not.EqualTo(baseline));
            Assert.That(attackerChanged, Is.Not.EqualTo(baseline));
            Assert.That(targetChanged, Is.Not.EqualTo(baseline));
            Assert.That(parity,
                Does.Contain("\"activeModeAttackingPercent1C\":37"));
            Assert.That(parity,
                Does.Contain("\"attackerInjuryMpPercent34\":61"));
            Assert.That(parity,
                Does.Contain("\"targetInjuryMpPercent38\":62"));
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
