#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5WeaponDurabilityAttackingInjuryProductionEditorTests
    {
        [TestCase(LF2ObjectType.LightWeapon)]
        [TestCase(LF2ObjectType.HeavyWeapon)]
        [TestCase(LF2ObjectType.ThrowWeapon)]
        [TestCase(LF2ObjectType.Drink)]
        public void Actual_TypeOneTwoFourSixUseDefinitionAttacking(
            LF2ObjectType targetType)
        {
            CreatePair(targetType, 250, 300, out SimulationWorld world,
                out TestCharacter attacker, out LF2Weapon target);

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world, attacker, target, Interaction(10));

            Assert.That(applied, Is.True);
            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(75));
        }

        [Test]
        public void Actual_DefinitionWinsOverModeAndHpDamageStaysRaw()
        {
            CreatePair(LF2ObjectType.LightWeapon, 200, 300,
                out SimulationWorld world,
                out TestCharacter attacker,
                out LF2Weapon target);

            world.DamageWriter.ApplyWeaponDamage(
                world, attacker, target, Interaction(10));

            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(80));
            Assert.That(target.Health.HP, Is.EqualTo(90),
                "attacking multiplier belongs to weapon durability, not HP damage");
        }

        [Test]
        public void Actual_NonPositiveDefinitionFallsBackToPositiveMode()
        {
            CreatePair(LF2ObjectType.LightWeapon, 0, 300,
                out SimulationWorld world,
                out TestCharacter attacker,
                out LF2Weapon target);

            world.DamageWriter.ApplyWeaponDamage(
                world, attacker, target, Interaction(10));

            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(70));
        }

        [TestCase(0, 0)]
        [TestCase(-1, -1)]
        public void Actual_NonPositiveDefinitionAndModePreserveRawInjury(
            int definitionAttacking,
            int modeAttacking)
        {
            CreatePair(LF2ObjectType.LightWeapon,
                definitionAttacking,
                modeAttacking,
                out SimulationWorld world,
                out TestCharacter attacker,
                out LF2Weapon target);

            world.DamageWriter.ApplyWeaponDamage(
                world, attacker, target, Interaction(10));

            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(90));
        }

        [Test]
        public void Actual_RawBdefendHundredStillForcesBrokenSentinel()
        {
            CreatePair(LF2ObjectType.LightWeapon, 250, 0,
                out SimulationWorld world,
                out TestCharacter attacker,
                out LF2Weapon target);
            InteractionArea interaction = Interaction(10);
            interaction.bdefend = 100;

            world.DamageWriter.ApplyWeaponDamage(
                world, attacker, target, interaction);

            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(-1));
        }

        [Test]
        public void HitPlanShadow_ProjectsSameEffectiveDurability()
        {
            CreatePair(LF2ObjectType.LightWeapon, 250, 0,
                out SimulationWorld world,
                out TestCharacter attacker,
                out LF2Weapon target);
            InteractionArea interaction = Interaction(10);
            interaction.vrest = 1;
            interaction.x = -100;
            interaction.y = -20;
            interaction.w = 240;
            interaction.h = 40;
            interaction.zwidth = 15;
            attacker.Frame.D.itrs.Add(interaction);
            target.Frame.D.bodies.Add(new BodyBox
            {
                kind = 0,
                x = 0,
                y = -10,
                w = 10,
                h = 20,
            });
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(3210);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.True);
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(75));
            Assert.That(target.Health.HP, Is.EqualTo(90));
        }

        private static void CreatePair(
            LF2ObjectType targetType,
            int definitionAttacking,
            int modeAttacking,
            out SimulationWorld world,
            out TestCharacter attacker,
            out LF2Weapon target)
        {
            world = new SimulationWorld();
            attacker = Character(world, 9010, 0, definitionAttacking);
            target = Weapon(world, 9011, 1, targetType);
            world.Runtime.NativeHitResourceRules.ActiveModeAttackingPercent1C =
                modeAttacking;
            target.Runtime.WeaponFlightCounter = 100;
        }

        private static InteractionArea Interaction(int injury)
        {
            return new InteractionArea
            {
                kind = 0,
                injury = injury,
                fall = 1,
                dvx = 1,
                arest = 10,
                vrest = 1,
            };
        }

        private static TestCharacter Character(
            SimulationWorld world,
            int objectId,
            int slot,
            int definitionAttacking)
        {
            LF2CharacterData data = Data(objectId, 241);
            data.definition_attacking = definitionAttacking;
            var entity = new TestCharacter
            {
                Name = "DurabilityAttacker",
                ObjectId = objectId,
            };
            entity.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Initialize(500, 500);
            world.Register(entity);
            Configure(entity, slot + 1);
            return entity;
        }

        private static LF2Weapon Weapon(
            SimulationWorld world,
            int objectId,
            int slot,
            LF2ObjectType objectType)
        {
            LF2CharacterData data = Data(objectId, 21);
            var entity = new LF2Weapon
            {
                Name = "DurabilityTarget",
                ObjectId = objectId,
            };
            entity.SetWeaponType((int)objectType);
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            world.Register(entity);
            Configure(entity, slot + 1);
            return entity;
        }

        private static LF2CharacterData Data(int objectId, int frameCount)
        {
            var frames = new List<LF2FrameData>(frameCount);
            for (int frameId = 0; frameId < frameCount; frameId++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = frameId,
                    state = LF2States.Standing,
                    wait = 100,
                    next = frameId,
                });
            }

            return new LF2CharacterData
            {
                name = $"Durability{objectId}",
                type_sub = objectId,
                frames = frames,
            };
        }

        private static void Configure(LF2Entity entity, int team)
        {
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.Health.HP3 = 100;
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.ItrRest.Reset();
        }

        private sealed class TestCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }
    }
}
#endif
