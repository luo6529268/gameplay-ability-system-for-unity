#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class FrameAdvanceRuntimeSnapshotEditorTests
    {
        [Test]
        public void ExactCharacter_UsesCanonicalRuntimeWithoutWideSnapshotRepair()
        {
            var character = new LF2Character();
            character.Frame.N = 77;
            character.Runtime.Frame = 77;

            bool refreshed = character.RefreshRuntimeSnapshotAfterFrameAdvance();

            Assert.That(refreshed, Is.False);
            Assert.That(character.Runtime.Frame, Is.EqualTo(77));
        }

        [Test]
        public void UnknownDerivedCharacter_FallsBackToVirtualSnapshotRefresh()
        {
            var character = new DerivedCharacter();
            int refreshCountBefore = character.RefreshCount;
            character.Frame.N = 77;
            character.Runtime.Frame = -1;

            bool refreshed = character.RefreshRuntimeSnapshotAfterFrameAdvance();

            Assert.That(refreshed, Is.True);
            Assert.That(character.RefreshCount, Is.EqualTo(refreshCountBefore + 1));
            Assert.That(character.Runtime.Frame, Is.EqualTo(77));
        }

        [Test]
        public void WarmedExactCharacterPath_AllocatesNoManagedMemory()
        {
            var character = new LF2Character();
            character.RefreshRuntimeSnapshotAfterFrameAdvance();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4096; i++)
                character.RefreshRuntimeSnapshotAfterFrameAdvance();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void ExactCharacter_PostFrameMaintenanceUsesDirectRuntimeBindings()
        {
            var character = new LF2Character();
            character.Runtime.SetVelocity(7.0, -3.0, 2.0);
            character.Runtime.HitCount = 4;
            character.Runtime.KnockbackVx = 10.0;
            character.Runtime.KnockbackVy = 11.0;
            character.Runtime.KnockbackVz = 12.0;
            character.Runtime.HealTimer = 1099;
            character.Runtime.CatchTimer = 17;

            bool refreshed =
                character.RefreshRuntimeSnapshotAfterPostFrameMaintenance();

            Assert.That(refreshed, Is.False);
            Assert.That(character.PS.vx, Is.EqualTo(7.0));
            Assert.That(character.PS.vy, Is.EqualTo(-3.0));
            Assert.That(character.PS.vz, Is.EqualTo(2.0));
            Assert.That(character.HitCount, Is.EqualTo(4));
            Assert.That(character.KnockbackVx, Is.EqualTo(10.0));
            Assert.That(character.KnockbackVy, Is.EqualTo(11.0));
            Assert.That(character.KnockbackVz, Is.EqualTo(12.0));
            Assert.That(character.HealTimer, Is.EqualTo(1099));
            Assert.That(character.CatchTimer, Is.EqualTo(17));
        }

        [Test]
        public void UnknownDerivedCharacter_PostFrameMaintenanceFallsBackToVirtualRefresh()
        {
            var character = new DerivedCharacter();
            int refreshCountBefore = character.RefreshCount;

            bool refreshed =
                character.RefreshRuntimeSnapshotAfterPostFrameMaintenance();

            Assert.That(refreshed, Is.True);
            Assert.That(character.RefreshCount, Is.EqualTo(refreshCountBefore + 1));
        }

        [Test]
        public void WarmedExactCharacterPostFrameMaintenance_AllocatesNoManagedMemory()
        {
            var character = new LF2Character();
            character.RefreshRuntimeSnapshotAfterPostFrameMaintenance();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4096; i++)
                character.RefreshRuntimeSnapshotAfterPostFrameMaintenance();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void BattleMechanicsStep_PreservesCompatibilityStepLogicalResult()
        {
            var mechanics = new CharacterMechanics();
            var compatibilityRuntime = CreateMechanicsRuntime();
            var battleRuntime = CreateMechanicsRuntime();
            var frame = new LF2FrameData
            {
                centerx = 39,
                centery = 79,
            };
            var compatibilityContext = new CharacterMechanicsContext(
                compatibilityRuntime,
                frame,
                80f,
                1f,
                0.01f,
                0.85);
            var battleContext = new CharacterMechanicsContext(
                battleRuntime,
                frame,
                80f,
                1f,
                0.01f,
                0.85);

            MechanicsStepResult compatibility = mechanics.Step(compatibilityContext);
            BattleMechanicsStepResult battle = mechanics.StepBattleLogic(battleContext);

            Assert.That(battle.BoundaryMode, Is.EqualTo(compatibility.boundaryMode));
            Assert.That(battle.Landed, Is.EqualTo(compatibility.landed));
            Assert.That(
                battle.VerticalVelocityBeforeLanding,
                Is.EqualTo(compatibility.verticalVelocityBeforeLanding));
            AssertRuntimeLogicalStateEqual(compatibilityRuntime, battleRuntime);
        }

        [Test]
        public void WarmedBattleMechanicsStep_AllocatesNoManagedMemory()
        {
            var mechanics = new CharacterMechanics();
            var runtime = CreateMechanicsRuntime();
            var context = new CharacterMechanicsContext(
                runtime,
                null,
                0f,
                0f,
                0f,
                0.85);
            mechanics.StepBattleLogic(context);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4096; i++)
                mechanics.StepBattleLogic(context);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void RegisteredEntities_ShareWorldOwnedCharacterMechanics()
        {
            var world = new SimulationWorld();
            var first = new MechanicsProbeCharacter();
            var second = new MechanicsProbeCharacter();
            world.Register(first);
            world.Register(second);

            CharacterMechanics firstMechanics = first.ResolveMechanics();
            CharacterMechanics secondMechanics = second.ResolveMechanics();

            Assert.That(firstMechanics, Is.SameAs(secondMechanics));
            Assert.That(ReadCompatibilityMechanics(first), Is.Null);
            Assert.That(ReadCompatibilityMechanics(second), Is.Null);
        }

        [Test]
        public void UnregisteredEntity_CreatesCompatibilityMechanicsLazily()
        {
            var character = new MechanicsProbeCharacter();

            Assert.That(ReadCompatibilityMechanics(character), Is.Null);
            CharacterMechanics first = character.ResolveMechanics();
            CharacterMechanics second = character.ResolveMechanics();

            Assert.That(first, Is.SameAs(second));
            Assert.That(ReadCompatibilityMechanics(character), Is.SameAs(first));
        }

        [Test]
        public void CompatibilityStep_MaterializesLegacySpriteOriginOnlyAtAdapterBoundary()
        {
            var mechanics = new CharacterMechanics();
            var compatibilityRuntime = CreateMechanicsRuntime();
            var battleRuntime = CreateMechanicsRuntime();
            var frame = new LF2FrameData
            {
                centerx = 39,
                centery = 79,
            };
            var compatibilityContext = new CharacterMechanicsContext(
                compatibilityRuntime,
                frame,
                80f,
                1f,
                0.01f,
                0.85);
            var battleContext = new CharacterMechanicsContext(
                battleRuntime,
                frame,
                80f,
                1f,
                0.01f,
                0.85);

            mechanics.Step(compatibilityContext);
            mechanics.StepBattleLogic(battleContext);

            Assert.That(compatibilityRuntime.SpriteX, Is.Not.Zero);
            Assert.That(compatibilityRuntime.SpriteY, Is.Not.Zero);
            Assert.That(compatibilityRuntime.SpriteZ, Is.Not.Zero);
            Assert.That(battleRuntime.SpriteX, Is.Zero);
            Assert.That(battleRuntime.SpriteY, Is.Zero);
            Assert.That(battleRuntime.SpriteZ, Is.Zero);
        }

        [Test]
        public void CharacterFrameAdvance_DefaultsToDataOrientedOwnership()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsCharacterFrameAdvancePassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterFrameAdvancePassMode.DataOriented));
        }

        [Test]
        public void CharacterFrameAdvance_DataOrientedMatchesLegacyReleaseFlow()
        {
            var dataWorld = new SimulationWorld();
            var legacyWorld = new SimulationWorld();
            legacyWorld.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(
                BattleEcsCharacterFrameAdvancePassMode.Legacy);
            LF2Character dataCharacter = CreateFrameAdvanceCharacter(dataWorld);
            LF2Character legacyCharacter = CreateFrameAdvanceCharacter(legacyWorld);

            dataWorld.SerialTickAll(7);
            legacyWorld.SerialTickAll(7);

            AssertRuntimeLogicalStateEqual(
                legacyCharacter.Runtime,
                dataCharacter.Runtime);
            Assert.That(dataCharacter.Frame.N, Is.EqualTo(legacyCharacter.Frame.N));
            Assert.That(dataCharacter.FrameDelay, Is.EqualTo(legacyCharacter.FrameDelay));
            Assert.That(dataCharacter.WeaponCount, Is.EqualTo(legacyCharacter.WeaponCount));
            Assert.That(
                dataWorld.BattleEcsCharacterFrameAdvancePassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.EqualTo(1));
        }

        [Test]
        public void CharacterFrameAdvance_UnknownDerivedTypeFallsBackToVirtualPath()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateFrameAdvanceCharacter(
                world,
                new DerivedCharacter());

            world.SerialTickAll(7);

            Assert.That(character.Runtime.X, Is.Not.EqualTo(12.0));
            Assert.That(
                world.BattleEcsCharacterFrameAdvancePassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.Zero);
            Assert.That(
                world.BattleEcsCharacterFrameAdvancePassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.EqualTo(1));
        }

        [Test]
        public void CharacterRecovery_DefaultsToDataOrientedOwnership()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsCharacterRecoveryPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterRecoveryPassMode.DataOriented));
        }

        [Test]
        public void CharacterRecovery_DataOrientedMatchesLegacyPeriodicWrites()
        {
            var dataWorld = new SimulationWorld();
            var legacyWorld = new SimulationWorld();
            legacyWorld.ConfigureBattleEcsCharacterRecoveryPassForDiagnostics(
                BattleEcsCharacterRecoveryPassMode.Legacy);
            LF2Character dataCharacter = CreateRecoveryCharacter(dataWorld);
            LF2Character legacyCharacter = CreateRecoveryCharacter(legacyWorld);

            dataWorld.LateEntityUpdateAll(12);
            legacyWorld.LateEntityUpdateAll(12);

            Assert.That(dataCharacter.Health.HP, Is.EqualTo(legacyCharacter.Health.HP));
            Assert.That(
                dataCharacter.Health.HPBound,
                Is.EqualTo(legacyCharacter.Health.HPBound));
            Assert.That(dataCharacter.Health.PP, Is.EqualTo(legacyCharacter.Health.PP));
            Assert.That(
                dataCharacter.ComboCountVic,
                Is.EqualTo(legacyCharacter.ComboCountVic));
            Assert.That(
                dataWorld.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.EqualTo(1));
            Assert.That(
                dataWorld.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.Zero);
        }

        [Test]
        public void CharacterRecovery_NonPeriodicTickUsesProvenNoOp()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateRecoveryCharacter(world);

            world.LateEntityUpdateAll(1);

            Assert.That(character.Health.HP, Is.EqualTo(400));
            Assert.That(character.Health.HPBound, Is.EqualTo(500));
            Assert.That(character.Health.PP, Is.EqualTo(100));
            Assert.That(character.ComboCountVic, Is.Zero);
            Assert.That(
                world.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics
                    .ProvenNoOpCount,
                Is.EqualTo(1));
        }

        [Test]
        public void CharacterRecovery_UnknownDerivedTypeFallsBackToVirtualPath()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateRecoveryCharacter(
                world,
                new DerivedCharacter());

            world.LateEntityUpdateAll(12);

            Assert.That(character.Health.HP, Is.EqualTo(392));
            Assert.That(
                world.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.Zero);
            Assert.That(
                world.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.EqualTo(1));
        }

        [Test]
        public void CharacterFrameTick_DefaultsToDataOrientedOwnership()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsCharacterFrameTickPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterFrameTickPassMode.DataOriented));
        }

        [Test]
        public void CharacterFrameTick_DataOrientedMatchesLegacyTransitionAndCounters()
        {
            var dataWorld = new SimulationWorld();
            var legacyWorld = new SimulationWorld();
            legacyWorld.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(
                BattleEcsCharacterFrameTickPassMode.Legacy);
            LF2Character dataCharacter = CreateFrameTickCharacter(dataWorld);
            LF2Character legacyCharacter = CreateFrameTickCharacter(legacyWorld);

            dataWorld.LateEntityUpdateAll(1);
            legacyWorld.LateEntityUpdateAll(1);

            Assert.That(dataCharacter.Frame.N, Is.EqualTo(legacyCharacter.Frame.N));
            Assert.That(
                dataCharacter.AttackingCounter,
                Is.EqualTo(legacyCharacter.AttackingCounter));
            Assert.That(dataCharacter.AttackExempt, Is.EqualTo(legacyCharacter.AttackExempt));
            Assert.That(dataCharacter.HitStun, Is.EqualTo(legacyCharacter.HitStun));
            Assert.That(dataCharacter.FallCounter, Is.EqualTo(legacyCharacter.FallCounter));
            Assert.That(
                dataCharacter.HitStateCount,
                Is.EqualTo(legacyCharacter.HitStateCount));
            Assert.That(
                dataCharacter.HitConfirmCounter,
                Is.EqualTo(legacyCharacter.HitConfirmCounter));
            Assert.That(
                dataCharacter.Trans.WaitCounter,
                Is.EqualTo(legacyCharacter.Trans.WaitCounter));
            Assert.That(
                dataWorld.BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.EqualTo(1));
            Assert.That(
                dataWorld.BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.Zero);
        }

        [Test]
        public void CharacterFrameTick_UnknownDerivedTypeFallsBackToVirtualPath()
        {
            var world = new SimulationWorld();
            var derived = new DerivedCharacter();
            CreateFrameTickCharacter(world, derived);

            world.LateEntityUpdateAll(1);

            Assert.That(derived.FrameTickCount, Is.EqualTo(1));
            Assert.That(
                world.BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.Zero);
            Assert.That(
                world.BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.EqualTo(1));
        }

        [Test]
        public void WarmedCharacterFrameTickPass_AllocatesNoManagedMemory()
        {
            var world = new SimulationWorld();
            for (int i = 0; i < 32; i++)
                CreateFrameTickCharacter(world, objectId: 100 + i, wait: 100, next: 0);

            world.LateEntityUpdateAll(1);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();

            world.LateEntityUpdateAll(2);

            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            Assert.That(
                world.BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.EqualTo(64));
        }

        private static NTSDEntityRuntime CreateMechanicsRuntime()
        {
            var runtime = new NTSDEntityRuntime();
            runtime.SetPosition(120.5, -3.25, 48.75);
            runtime.SetVelocity(4.0, 1.5, -2.0);
            runtime.SyncIntegerPosition();
            runtime.XBoundPositive = true;
            return runtime;
        }

        private static LF2Character CreateFrameAdvanceCharacter(
            SimulationWorld world,
            LF2Character character = null)
        {
            character ??= new LF2Character();
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                centerx = 39,
                centery = 79,
            };
            var data = new LF2CharacterData
            {
                name = "FrameAdvanceProbe",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };

            character.ModuleInitialize();
            character.ObjectId = 1;
            character.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Initialize(500, 500);
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Runtime.SetPosition(12.0, -3.0, 20.0);
            character.Runtime.SetVelocity(4.0, 1.5, -2.0);
            character.Runtime.SyncIntegerPosition();
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            return character;
        }

        private static LF2Character CreateRecoveryCharacter(
            SimulationWorld world,
            LF2Character character = null)
        {
            character = CreateFrameAdvanceCharacter(world, character);
            character.Health.HP = 400;
            character.Health.HPBound = 500;
            character.Health.PP = 100;
            character.KillCount = -1;
            character.WeaponCount = -1;
            character.FallDamageDiv = 0;
            character.HitStun = 0;
            character.ComboCountVic = 0;
            character.Runtime.SuppressLateFrameTickUntilTick = int.MaxValue;
            return character;
        }

        private static LF2Character CreateFrameTickCharacter(
            SimulationWorld world,
            LF2Character character = null,
            int objectId = 2,
            int wait = 0,
            int next = 1)
        {
            character ??= new LF2Character();
            var first = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = wait,
                next = next,
                centerx = 39,
                centery = 79,
            };
            var second = new LF2FrameData
            {
                frameId = 1,
                state = 0,
                wait = 100,
                next = 1,
                centerx = 39,
                centery = 79,
            };
            var data = new LF2CharacterData
            {
                name = "FrameTickProbe" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { first, second },
            };

            character.ModuleInitialize();
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Initialize(500, 500);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Trans.SyncDirectFrameData(wait, next, 0);
            character.AttackExempt = 2;
            character.HitStun = 2;
            character.FallCounter = 2;
            character.HitStateCount = 2;
            character.HitConfirmCounter = 2;
            character.Runtime.SetPosition(20.0, 0.0, 30.0);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.SuppressLateFrameTickUntilTick = 0;
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            return character;
        }

        private static void AssertRuntimeLogicalStateEqual(
            NTSDEntityRuntime expected,
            NTSDEntityRuntime actual)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X));
            Assert.That(actual.Y, Is.EqualTo(expected.Y));
            Assert.That(actual.Z, Is.EqualTo(expected.Z));
            Assert.That(actual.Vx, Is.EqualTo(expected.Vx));
            Assert.That(actual.Vy, Is.EqualTo(expected.Vy));
            Assert.That(actual.Vz, Is.EqualTo(expected.Vz));
            Assert.That(actual.XInt, Is.EqualTo(expected.XInt));
            Assert.That(actual.YInt, Is.EqualTo(expected.YInt));
            Assert.That(actual.ZInt, Is.EqualTo(expected.ZInt));
            Assert.That(actual.XBoundPositive, Is.EqualTo(expected.XBoundPositive));
            Assert.That(actual.XBoundNegative, Is.EqualTo(expected.XBoundNegative));
            Assert.That(actual.ZBoundPositive, Is.EqualTo(expected.ZBoundPositive));
            Assert.That(actual.ZBoundNegative, Is.EqualTo(expected.ZBoundNegative));
        }

        private static CharacterMechanics ReadCompatibilityMechanics(
            LF2Entity entity)
        {
            FieldInfo field = typeof(LF2Entity).GetField(
                "compatibilityCharacterMechanics",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (CharacterMechanics)field.GetValue(entity);
        }

        private sealed class DerivedCharacter : LF2Character
        {
            public int RefreshCount { get; private set; }
            public int FrameTickCount { get; private set; }

            protected override void RefreshRuntimeFromEntity()
            {
                RefreshCount++;
                base.RefreshRuntimeFromEntity();
            }

            public override void SimFrameTick(int tickIndex)
            {
                FrameTickCount++;
                base.SimFrameTick(tickIndex);
            }
        }

        private sealed class MechanicsProbeCharacter : LF2Character
        {
            public CharacterMechanics ResolveMechanics()
            {
                return ResolveCharacterMechanics();
            }
        }
    }
}
#endif
