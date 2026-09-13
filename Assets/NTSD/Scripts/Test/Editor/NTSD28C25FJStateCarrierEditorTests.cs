#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25FJStateCarrierEditorTests
    {
        [Test]
        public void DefaultsAndReset_MatchNativeStateCarrierContract()
        {
            var runtime = new NTSDEntityRuntime();
            AssertDefaults(runtime);

            MutateAll(runtime, 21);
            runtime.Reset();

            AssertDefaults(runtime);
        }

        [Test]
        public void SnapshotAndChecksumSchemas_AdvanceForC25FJState()
        {
            Assert.That(
                BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(13));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(21));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion,
                Is.EqualTo(24));
        }

        [Test]
        public void EveryCarrier_ChangesParityChecksum()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(0);
            world.Register(entity);
            NTSDEntityRuntime runtime = entity.Runtime;
            string baseline = world.CaptureParityFrameSnapshot(1).OverallChecksum;
            var cases = new List<(string Name, Action Set, Action Reset)>
            {
                Case(nameof(runtime.HitResourceInjuryDouble1A0), () => runtime.HitResourceInjuryDouble1A0 = 1, () => runtime.HitResourceInjuryDouble1A0 = 0),
                Case(nameof(runtime.DelayTimer134), () => runtime.DelayTimer134 = 2, () => runtime.DelayTimer134 = 0),
                Case(nameof(runtime.JoinTimer148), () => runtime.JoinTimer148 = 3, () => runtime.JoinTimer148 = 0),
                Case(nameof(runtime.PoisonTimer120), () => runtime.PoisonTimer120 = 4, () => runtime.PoisonTimer120 = 0),
                Case(nameof(runtime.PoisonType124), () => runtime.PoisonType124 = 5, () => runtime.PoisonType124 = 0),
                Case(nameof(runtime.PoisonStrength128), () => runtime.PoisonStrength128 = 6, () => runtime.PoisonStrength128 = 0),
                Case(nameof(runtime.JoinOverrideActive170), () => runtime.JoinOverrideActive170 = 7, () => runtime.JoinOverrideActive170 = 0),
                Case(nameof(runtime.JoinOriginalBattleGroup174), () => runtime.JoinOriginalBattleGroup174 = 8, () => runtime.JoinOriginalBattleGroup174 = 0),
                Case(nameof(runtime.NativeComputerState1B8), () => runtime.NativeComputerState1B8 = 9, () => runtime.NativeComputerState1B8 = 0),
                Case(nameof(runtime.NativeTimer1BC), () => runtime.NativeTimer1BC = 10, () => runtime.NativeTimer1BC = 0),
                Case(nameof(runtime.RuntimeArmorHp118), () => runtime.RuntimeArmorHp118 = 11, () => runtime.RuntimeArmorHp118 = 0),
                Case(nameof(runtime.ArmorRecoveryTimer11C), () => runtime.ArmorRecoveryTimer11C = 12, () => runtime.ArmorRecoveryTimer11C = -1),
            };

            for (int index = 0; index < cases.Count; index++)
            {
                cases[index].Set();
                Assert.That(
                    world.CaptureParityFrameSnapshot(1).OverallChecksum,
                    Is.Not.EqualTo(baseline),
                    cases[index].Name);
                cases[index].Reset();
                Assert.That(
                    world.CaptureParityFrameSnapshot(1).OverallChecksum,
                    Is.EqualTo(baseline),
                    cases[index].Name + " reset");
            }
        }

        [Test]
        public void ParityAndRaw_ExposeIndependentNativeStateAndArmor()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(0);
            world.Register(entity);
            MutateAll(entity.Runtime, 31);

            string parity = world.CaptureParityFrameSnapshot(1).ToJson(full: true);
            string raw = NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1);

            Assert.That(parity, Does.Contain("\"nativeReactionStatus\""));
            Assert.That(parity, Does.Contain("\"hitResourceInjuryDouble1A0\":31"));
            Assert.That(parity, Does.Contain("\"nativeComputerState1B8\":39"));
            Assert.That(parity, Does.Contain("\"armorRecoveryTimer11C\":42"));
            Assert.That(raw, Does.Contain("\"runtimeArmorHp\":41"));
            Assert.That(raw, Does.Contain("\"armorRecoveryTimer\":42"));
            Assert.That(raw, Does.Contain("\"verifiedCount\":44"));
            Assert.That(raw, Does.Contain("\"missingCount\":6"));
        }

        private static (string Name, Action Set, Action Reset) Case(
            string name,
            Action set,
            Action reset)
        {
            return (name, set, reset);
        }

        private static void MutateAll(NTSDEntityRuntime runtime, int seed)
        {
            runtime.HitResourceInjuryDouble1A0 = seed;
            runtime.DelayTimer134 = seed + 1;
            runtime.JoinTimer148 = seed + 2;
            runtime.PoisonTimer120 = seed + 3;
            runtime.PoisonType124 = seed + 4;
            runtime.PoisonStrength128 = seed + 5;
            runtime.JoinOverrideActive170 = seed + 6;
            runtime.JoinOriginalBattleGroup174 = seed + 7;
            runtime.NativeComputerState1B8 = seed + 8;
            runtime.NativeTimer1BC = seed + 9;
            runtime.RuntimeArmorHp118 = seed + 10;
            runtime.ArmorRecoveryTimer11C = seed + 11;
        }

        private static void AssertDefaults(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.HitResourceInjuryDouble1A0, Is.Zero);
            Assert.That(runtime.DelayTimer134, Is.Zero);
            Assert.That(runtime.JoinTimer148, Is.Zero);
            Assert.That(runtime.PoisonTimer120, Is.Zero);
            Assert.That(runtime.PoisonType124, Is.Zero);
            Assert.That(runtime.PoisonStrength128, Is.Zero);
            Assert.That(runtime.JoinOverrideActive170, Is.Zero);
            Assert.That(runtime.JoinOriginalBattleGroup174, Is.Zero);
            Assert.That(runtime.NativeComputerState1B8, Is.Zero);
            Assert.That(runtime.NativeTimer1BC, Is.Zero);
            Assert.That(runtime.RuntimeArmorHp118, Is.Zero);
            Assert.That(runtime.ArmorRecoveryTimer11C, Is.EqualTo(-1));
        }
    }
}
#endif
