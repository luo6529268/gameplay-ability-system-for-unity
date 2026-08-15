#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsCharacterPostFrameTailPassEditorTests
    {
        [Test]
        public void DefaultMode_IsLegacyUntilPerformanceGatePasses()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsCharacterPostFrameTailPassModeForDiagnostics,
                Is.EqualTo(
                    BattleEcsCharacterPostFrameTailPassMode.Legacy));
        }

        [Test]
        public void DataOriented_ExactlyMatchesLegacyRuntimeMaintenance()
        {
            var dataWorld = new SimulationWorld();
            var legacyWorld = new SimulationWorld();
            dataWorld.ConfigureBattleEcsCharacterPostFrameTailPassForDiagnostics(
                BattleEcsCharacterPostFrameTailPassMode.DataOriented);
            legacyWorld.ConfigureBattleEcsCharacterPostFrameTailPassForDiagnostics(
                BattleEcsCharacterPostFrameTailPassMode.Legacy);
            LF2Character data = RegisterCharacter(dataWorld, 50);
            LF2Character legacy = RegisterCharacter(legacyWorld, 50);
            Configure(data.Runtime);
            Configure(legacy.Runtime);

            dataWorld.EntityPostFrameTailAll(10);
            legacyWorld.EntityPostFrameTailAll(10);

            AssertRuntimeEquals(legacy.Runtime, data.Runtime);
            BattleEcsCharacterPostFrameTailPassDiagnostics diagnostics =
                dataWorld.BattleEcsCharacterPostFrameTailPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.ExactCharacterCount, Is.EqualTo(1));
            Assert.That(diagnostics.CompatibilityFallbackCount, Is.Zero);
        }

        [Test]
        public void UnknownDerivedCharacter_FallsBackToVirtualCarrierClear()
        {
            var world = new SimulationWorld();
            var character = new DerivedCharacter();
            character.SetRequiredRuntimeSlot(50);
            world.Register(character);
            Configure(character.Runtime);

            world.EntityPostFrameTailAll(10);

            Assert.That(character.ClearCount, Is.EqualTo(1));
            BattleEcsCharacterPostFrameTailPassDiagnostics diagnostics =
                world.BattleEcsCharacterPostFrameTailPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.ExactCharacterCount, Is.Zero);
            Assert.That(diagnostics.CompatibilityFallbackCount, Is.EqualTo(1));
        }

        [Test]
        public void Extended1000_WarmedDataOrientedTailDoesNotAllocate()
        {
            const int capacity = 1050;
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                capacity);
            world.ConfigureBattleEcsCharacterPostFrameTailPassForDiagnostics(
                BattleEcsCharacterPostFrameTailPassMode.DataOriented);
            for (int slot = 50; slot < capacity; slot++)
                Configure(RegisterCharacter(world, slot).Runtime);

            world.EntityPostFrameTailAll(10);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.EntityPostFrameTailAll(11);
            long allocated =
                GC.GetAllocatedBytesForCurrentThread() - before;

            BattleEcsCharacterPostFrameTailPassDiagnostics diagnostics =
                world.BattleEcsCharacterPostFrameTailPassDiagnosticsForDiagnostics;
            Assert.That(allocated, Is.Zero);
            Assert.That(diagnostics.RunCount, Is.EqualTo(2000));
            Assert.That(diagnostics.ExactCharacterCount, Is.EqualTo(2000));
            Assert.That(diagnostics.CompatibilityFallbackCount, Is.Zero);
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot)
        {
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            return character;
        }

        private static void Configure(NTSDEntityRuntime runtime)
        {
            runtime.HP = 400;
            runtime.HPBound = 500;
            runtime.HealTimer = 1009;
            runtime.CatchTimer = 9;
            runtime.HitConfirm2 = 7;
            runtime.TransientMp = 4;
            runtime.TransientMp2 = 5;
            runtime.TransientMp3 = 6;
            runtime.TransientMp4 = 7;
        }

        private static void AssertRuntimeEquals(
            NTSDEntityRuntime expected,
            NTSDEntityRuntime actual)
        {
            Assert.That(actual.HP, Is.EqualTo(expected.HP));
            Assert.That(actual.HPBound, Is.EqualTo(expected.HPBound));
            Assert.That(actual.HealTimer, Is.EqualTo(expected.HealTimer));
            Assert.That(actual.CatchTimer, Is.EqualTo(expected.CatchTimer));
            Assert.That(actual.HitConfirm2, Is.EqualTo(expected.HitConfirm2));
            Assert.That(actual.TransientMp, Is.EqualTo(expected.TransientMp));
            Assert.That(actual.TransientMp2, Is.EqualTo(expected.TransientMp2));
            Assert.That(actual.TransientMp3, Is.EqualTo(expected.TransientMp3));
            Assert.That(actual.TransientMp4, Is.EqualTo(expected.TransientMp4));
        }

        private sealed class DerivedCharacter : LF2Character
        {
            internal int ClearCount { get; private set; }

            public override void ClearHitCandidateCarriers()
            {
                ClearCount++;
                base.ClearHitCandidateCarriers();
            }
        }
    }
}
#endif
