#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class StageBoundsRuntimeSyncEditorTests
    {
        [Test]
        public void ClampPass_ClampsOutOfRangeAndTruncatesInRangeNegativeZTowardZero()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld(zMin: -10, zMax: 10);
            LF2Character above = RegisterCharacter(world, 20.75, 1);
            LF2Character inRange = RegisterCharacter(world, -3.9, 2);
            LF2Character below = RegisterCharacter(world, -20.25, 3);

            world.ClampCharacterZToStageBoundsAll();

            Assert.That(above.PS.z, Is.EqualTo(10.0));
            Assert.That(above.Runtime.Z, Is.EqualTo(10.0));
            Assert.That(above.Runtime.ZInt, Is.EqualTo(10));
            Assert.That(inRange.PS.z, Is.EqualTo(-3.9));
            Assert.That(inRange.Runtime.Z, Is.EqualTo(-3.9));
            Assert.That(inRange.Runtime.ZInt, Is.EqualTo(-3));
            Assert.That(below.PS.z, Is.EqualTo(-10.0));
            Assert.That(below.Runtime.Z, Is.EqualTo(-10.0));
            Assert.That(below.Runtime.ZInt, Is.EqualTo(-10));
        }

        [Test]
        public void ClampPass_CommitsOnlyZIntegerAndUsesVirtualRefreshForUnknownSubclass()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld();
            LF2Character divergent = RegisterCharacter(world, 500.25, 1);
            divergent.Runtime.X = 12.75;
            divergent.Runtime.Y = -4.25;
            divergent.Runtime.XInt = 700;
            divergent.Runtime.YInt = -200;
            divergent.Runtime.ZInt = 901;

            var unknown = new UnknownStageCharacter();
            unknown.PS.z = 500.25;
            world.Register(unknown);
            unknown.ResetRefreshProbe();
            unknown.Runtime.ZInt = -123;

            world.ClampCharacterZToStageBoundsAll();

            Assert.That(divergent.PS.z, Is.EqualTo(350.0));
            Assert.That(divergent.Runtime.Z, Is.EqualTo(350.0));
            Assert.That(divergent.Runtime.X, Is.EqualTo(12.75));
            Assert.That(divergent.Runtime.Y, Is.EqualTo(-4.25));
            Assert.That(divergent.Runtime.XInt, Is.EqualTo(700));
            Assert.That(divergent.Runtime.YInt, Is.EqualTo(-200));
            Assert.That(divergent.Runtime.ZInt, Is.EqualTo(350));
            Assert.That(unknown.PS.z, Is.EqualTo(350.0));
            Assert.That(unknown.Runtime.Z, Is.EqualTo(350.0));
            Assert.That(unknown.Runtime.ZInt, Is.EqualTo(350));
            Assert.That(unknown.RefreshProbeCount, Is.EqualTo(1));
            Assert.That(unknown.Runtime.Unk330, Is.EqualTo(1));
        }

        [Test]
        public void ClampPass_RefreshesOnlyActiveCharacterEntities()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld();
            LF2Character active = RegisterCharacter(world, 500.0, 1);
            LF2Character pending = RegisterCharacter(world, 500.0, 2);
            LF2Character dormant = RegisterCharacter(world, 500.0, 3);
            pending.Runtime.PendingFlushDestroy = true;
            dormant.Runtime.OidMergeDormant = true;

            var weapon = new LF2Weapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.PS.z = 500.0;
            world.Register(weapon);
            weapon.Runtime.SetPosition(0.0, 0.0, 500.0);
            weapon.Runtime.SyncIntegerPosition();
            var special = new LF2SpecialAttack();
            special.Runtime.EntityType = (int)LF2ObjectType.SpecialAttack;
            special.PS.z = 500.0;
            world.Register(special);
            special.Runtime.SetPosition(0.0, 0.0, 500.0);
            special.Runtime.SyncIntegerPosition();

            world.ClampCharacterZToStageBoundsAll();

            Assert.That(active.PS.z, Is.EqualTo(350.0));
            Assert.That(active.Runtime.Z, Is.EqualTo(350.0));
            Assert.That(pending.PS.z, Is.EqualTo(500.0));
            Assert.That(pending.Runtime.Z, Is.EqualTo(500.0));
            Assert.That(dormant.PS.z, Is.EqualTo(500.0));
            Assert.That(dormant.Runtime.Z, Is.EqualTo(500.0));
            Assert.That(weapon.PS.z, Is.EqualTo(500.0));
            Assert.That(weapon.Runtime.Z, Is.EqualTo(500.0));
            Assert.That(special.PS.z, Is.EqualTo(500.0));
            Assert.That(special.Runtime.Z, Is.EqualTo(500.0));
        }

        [Test]
        public void ClampPass_SlotReuseSecondMutationAndRepeatedPassAreDeterministic()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld();
            LF2Character oldCharacter = RegisterCharacter(world, 500.0, 1);
            int reusedSlot = oldCharacter.Runtime.SlotIndex;

            world.ClampCharacterZToStageBoundsAll();
            world.Unregister(oldCharacter);

            LF2Character replacement = RegisterCharacter(world, 100.0, 7);
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(reusedSlot));
            replacement.Team = 9;
            replacement.Health.HP = 321;
            // Mirror the canonical frame writer contract. Stage bounds owns
            // only Z/ZInt and must not repair unrelated compatibility fields.
            replacement.Frame.N = 77;
            replacement.Runtime.Frame = 77;
            replacement.PS.z = 410.75;

            world.ClampCharacterZToStageBoundsAll();
            BattleParityFrameSnapshot first =
                world.CaptureParityFrameSnapshot(tickIndex: 19);

            world.ClampCharacterZToStageBoundsAll();
            BattleParityFrameSnapshot second =
                world.CaptureParityFrameSnapshot(tickIndex: 19);

            Assert.That(replacement.PS.z, Is.EqualTo(350.0));
            Assert.That(replacement.Runtime.Z, Is.EqualTo(350.0));
            Assert.That(replacement.Runtime.ZInt, Is.EqualTo(350));
            Assert.That(replacement.Runtime.Team, Is.EqualTo(9));
            Assert.That(replacement.Runtime.HP, Is.EqualTo(321));
            Assert.That(replacement.Runtime.Frame, Is.EqualTo(77));
            Assert.That(second.OverallChecksum, Is.EqualTo(first.OverallChecksum));
        }

        [Test]
        public void WarmedClampPass_AllocatesNoManagedMemoryAcross512Iterations()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld();
            for (int i = 0; i < 64; i++)
                RegisterCharacter(world, 200.25 + (i % 10), i);

            for (int i = 0; i < 32; i++)
                world.ClampCharacterZToStageBoundsAll();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 512; i++)
                world.ClampCharacterZToStageBoundsAll();
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
        }

        private static SimulationWorld CreateWorld(int zMin = 180, int zMax = 350)
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(800, zMin, zMax, 0, 0);
            return world;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            double z,
            int seed)
        {
            var character = new LF2Character();
            character.Team = seed % 4;
            character.Health.HP = 400 + seed;
            character.Health.MP = 300 + seed;
            character.PS.z = z;
            world.Register(character);

            character.Runtime.ZInt = 123456 + seed;
            return character;
        }

        private sealed class UnknownStageCharacter : LF2Character
        {
            public int RefreshProbeCount { get; private set; }

            public void ResetRefreshProbe()
            {
                RefreshProbeCount = 0;
                Runtime.Unk330 = 0;
            }

            protected override void RefreshRuntimeFromEntity()
            {
                base.RefreshRuntimeFromEntity();
                RefreshProbeCount++;
                Runtime.Unk330++;
            }
        }

        private sealed class DisabledLoggingScope : IDisposable
        {
            private readonly bool previous;

            public DisabledLoggingScope()
            {
                previous = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
            }

            public void Dispose()
            {
                Debug.unityLogger.logEnabled = previous;
            }
        }
    }
}
#endif
