#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
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

        [TestCase(BattleEcsCharacterPostFrameTailPassMode.Legacy)]
        [TestCase(BattleEcsCharacterPostFrameTailPassMode.DataOriented)]
        public void ActiveEntityTail_ClearsSpecialHitLatchForCharacterAndHealthlessType3(
            BattleEcsCharacterPostFrameTailPassMode mode)
        {
            var world = new SimulationWorld();
            world.ConfigureBattleEcsCharacterPostFrameTailPassForDiagnostics(mode);
            LF2Character character = RegisterCharacter(world, 50);
            var projectile = new HealthlessType3Entity { ObjectId = 815 };
            projectile.SetRequiredRuntimeSlot(51);
            projectile.Runtime.ObjType = 3;
            world.Register(projectile);
            Assert.That(projectile.ObjectTypeEnum,
                Is.EqualTo(LF2ObjectType.SpecialAttack));
            Assert.That(projectile.Health, Is.Null);
            character.Runtime.SpecialHitLatch0EB = true;
            projectile.Runtime.SpecialHitLatch0EB = true;
            character.HitConfirm2 = 7;

            world.EntityPostFrameTailAll(1);

            Assert.That(character.Runtime.SpecialHitLatch0EB, Is.False);
            Assert.That(projectile.Runtime.SpecialHitLatch0EB, Is.False);
            Assert.That(character.HitConfirm2, Is.Zero);
        }

        [Test]
        public void Type3Latch_SuppressesFirstCompleteHitTickAndAllowsNextTick()
        {
            var world = new SimulationWorld();
            var attackFrame = new LF2FrameData
            {
                frameId = 0,
                state = 3000,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>
                {
                    new InteractionArea
                    {
                        kind = 0, x = -20, y = -20, w = 60, h = 40,
                        zwidth = 30, injury = 10, vrest = 0,
                    },
                },
            };
            var targetFrame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
            };
            targetFrame.bodies.Add(new BodyBox
            {
                kind = 0, x = -10, y = -10, w = 20, h = 20,
            });

            var attacker = new LF2SpecialAttack { ObjectId = 815 };
            attacker.FrameCache.Load(new LF2CharacterDataWrapper(
                815, new LF2CharacterData
                {
                    type_sub = 3,
                    frames = new List<LF2FrameData> { attackFrame },
                }));
            attacker.Frame.N = 0;
            attacker.Frame.PN = 0;
            attacker.Frame.D = attackFrame;
            attacker.SetRequiredRuntimeSlot(55);
            attacker.Runtime.ObjType = 3;
            attacker.Health.HP = 100;
            attacker.Team = 1;
            attacker.RelationTeam = 1;
            attacker.Runtime.SetPosition(100, 0, 100);
            attacker.Runtime.SyncIntegerPosition();
            world.Register(attacker);

            var target = new LF2Character { ObjectId = 7 };
            target.ModuleInitialize();
            target.FrameCache.Load(new LF2CharacterDataWrapper(
                7, new LF2CharacterData
                {
                    type_sub = 0,
                    frames = new List<LF2FrameData> { targetFrame },
                }));
            target.Frame.N = 0;
            target.Frame.PN = 0;
            target.Frame.D = targetFrame;
            target.Initialize(500, 500);
            target.SetRequiredRuntimeSlot(1);
            target.Team = 2;
            target.RelationTeam = 2;
            target.Runtime.SetPosition(110, 0, 100);
            target.Runtime.SyncIntegerPosition();
            world.Register(target);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1),
                "The synthetic type3/character pair must form one real candidate.");
            world.EndCollisionCandidateConsumption();

            attacker.Runtime.SpecialHitLatch0EB = true;
            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.RunReleaseTick(1, buildPresentation: false);

            Assert.That(target.Health.HP, Is.EqualTo(500),
                "The type3 latch must suppress the first tick's ordinary hit.");
            Assert.That(attacker.Runtime.SpecialHitLatch0EB, Is.False,
                "The shared active-entity tail must clear the latch.");

            tickSystem.RunReleaseTick(2, buildPresentation: false);

            Assert.That(target.Health.HP, Is.LessThan(500),
                "The next complete tick must admit the same ordinary hit; " +
                "candidateCount=" + attacker.Runtime.HitCandidateCount +
                ", attackerAction=" + attacker.Frame.N +
                ", targetAction=" + target.Frame.N +
                ", attackerX=" + attacker.Runtime.XInt +
                ", targetX=" + target.Runtime.XInt);
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

        private sealed class HealthlessType3Entity : LF2OtherObject
        {
            internal HealthlessType3Entity()
            {
                Health = null;
            }

            public override LF2ObjectType ObjectTypeEnum =>
                LF2ObjectType.SpecialAttack;
        }
    }
}
#endif
