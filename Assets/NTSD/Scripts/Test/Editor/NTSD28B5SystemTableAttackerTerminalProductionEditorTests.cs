#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B5SystemTableAttackerTerminalProductionEditorTests
    {
        [Test]
        public void SelectedArmorReducedHit_Oid201AttackerRemainsActive()
        {
            CreateSelectedArmorScenario(
                201,
                out SimulationWorld world,
                out LF2SpecialAttack attacker,
                out TypedCharacter target,
                out InteractionArea interaction);

            bool applied = InvokeTryApplyHit(attacker, interaction, target);

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(490));
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.EqualTo(80));
            Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(0));
            Assert.That(world.FindEntityByRuntimeSlotForQuery(0), Is.SameAs(attacker));
        }

        [Test]
        public void SelectedArmorReducedHit_Oid214AttackerKeepsHp()
        {
            CreateSelectedArmorScenario(
                214,
                out _,
                out LF2SpecialAttack attacker,
                out TypedCharacter target,
                out InteractionArea interaction);

            bool applied = InvokeTryApplyHit(attacker, interaction, target);

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(490));
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.EqualTo(80));
            Assert.That(attacker.Health.HP, Is.EqualTo(100));
        }

        [TestCase(201)]
        [TestCase(214)]
        public void OrdinaryDefenseReducedHit_SystemTableAttackerRemainsActive(
            int attackerObjectId)
        {
            var world = new SimulationWorld();
            LF2SpecialAttack attacker = CreateSpecialAttack(
                world,
                attackerObjectId,
                0);
            TypedCharacter target = CreateCharacter(
                world,
                7000 + attackerObjectId,
                1);
            InteractionArea interaction = CreateHit();
            interaction.dvx = 5;
            attacker.SwitchDir("right");
            target.SwitchDir("left");
            target.ImmediateFrame(7);

            bool applied = InvokeTryApplyHit(attacker, interaction, target);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(0));
            Assert.That(attacker.Health.HP, Is.EqualTo(100));
            Assert.That(world.FindEntityByRuntimeSlotForQuery(0), Is.SameAs(attacker));
            Assert.That(target.Health.HP, Is.EqualTo(498));
        }

        [TestCase(201)]
        [TestCase(214)]
        public void BrokenArmorUnarmoredFallback_ConsumesSystemTable(
            int attackerObjectId)
        {
            var world = new SimulationWorld();
            LF2SpecialAttack attacker = CreateSpecialAttack(
                world,
                attackerObjectId,
                0);
            LF2ArmorData armor = CreateArmor();
            armor.hp = 100;
            TypedCharacter target = CreateCharacter(
                world,
                7000 + attackerObjectId,
                1,
                armor);
            target.Runtime.RuntimeArmorHp118 = 20;

            bool applied = InvokeTryApplyHit(attacker, CreateHit(), target);

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(480));
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.Zero);
            if (attackerObjectId == 201)
            {
                Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(-1));
                Assert.That(world.FindEntityByRuntimeSlotForQuery(0), Is.Null);
            }
            else
            {
                Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(0));
                Assert.That(attacker.Health.HP, Is.Zero);
            }
        }

        [Test]
        public void UnarmoredHit_Oid201ReleasesAttackerAfterTargetWrites()
        {
            var world = new SimulationWorld();
            LF2SpecialAttack attacker = CreateSpecialAttack(world, 201, 0);
            TypedCharacter target = CreateCharacter(world, 7201, 1);

            bool applied = InvokeTryApplyHit(attacker, CreateHit(), target);

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(480));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(world.FindEntityByRuntimeSlotForQuery(0), Is.Null);
        }

        [Test]
        public void UnarmoredHit_Oid201RetainsSlotThroughHitRecordTailThenReleases()
        {
            var world = new SimulationWorld();
            LF2SpecialAttack attacker = CreateSpecialAttack(world, 201, 1);
            TypedCharacter target = CreateCharacter(world, 7301, 0);

            bool applied = InvokeTryApplyHit(attacker, CreateHit(), target);

            Assert.That(applied, Is.True);
            Assert.That(attacker.HitRecordCount, Is.EqualTo(1));
            Assert.That(target.HitRecordCount, Is.Zero);
            Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.Null);
        }

        [Test]
        public void UnarmoredHit_Oid214ZerosAttackerHp()
        {
            var world = new SimulationWorld();
            LF2SpecialAttack attacker = CreateSpecialAttack(world, 214, 0);
            TypedCharacter target = CreateCharacter(world, 7214, 1);

            bool applied = InvokeTryApplyHit(attacker, CreateHit(), target);

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(480));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(attacker.Health.HP, Is.Zero);
        }

        [TestCase(BattleHitExecutionPlanMode.DataOriented, 201)]
        [TestCase(BattleHitExecutionPlanMode.ShadowCompare, 214)]
        public void SelectedArmorReducedHit_HitPlanModesExcludeSystemTable(
            BattleHitExecutionPlanMode mode,
            int attackerObjectId)
        {
            CreateSelectedArmorScenario(
                attackerObjectId,
                out SimulationWorld world,
                out LF2SpecialAttack attacker,
                out TypedCharacter target,
                out InteractionArea interaction);
            ConfigureCollisionPair(attacker, target, interaction);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(mode);

            world.ObjectInteractionTickAll(901);

            Assert.That(
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics
                    .CurrentTickPlanValid,
                Is.True);
            Assert.That(attacker.Runtime.SlotIndex, Is.EqualTo(0));
            Assert.That(attacker.Health.HP, Is.EqualTo(100));
            Assert.That(target.Health.HP, Is.EqualTo(490));
        }

        private static void CreateSelectedArmorScenario(
            int attackerObjectId,
            out SimulationWorld world,
            out LF2SpecialAttack attacker,
            out TypedCharacter target,
            out InteractionArea interaction)
        {
            world = new SimulationWorld();
            attacker = CreateSpecialAttack(world, attackerObjectId, 0);
            LF2ArmorData armor = CreateArmor();
            armor.hp = 100;
            target = CreateCharacter(world, 7000 + attackerObjectId, 1, armor);
            target.Runtime.RuntimeArmorHp118 = 100;
            interaction = CreateHit();
        }

        private static bool InvokeTryApplyHit(
            LF2SpecialAttack attacker,
            InteractionArea interaction,
            LF2Entity target)
        {
            MethodInfo method = typeof(LF2SpecialAttack).GetMethod(
                "TryApplyHit",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(attacker, new object[] { interaction, target });
        }

        private static void ConfigureCollisionPair(
            LF2SpecialAttack attacker,
            TypedCharacter target,
            InteractionArea interaction)
        {
            interaction.x = -30;
            interaction.y = -10;
            interaction.w = 60;
            interaction.h = 20;
            interaction.zwidth = 15;
            attacker.Frame.D.itrs.Add(interaction);
            target.Frame.D.bodies.Add(new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            });
            attacker.Runtime.SetPosition(0, 0, 0);
            target.Runtime.SetPosition(10, 0, 0);
            attacker.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            attacker.RefreshRuntimeSnapshot();
            target.RefreshRuntimeSnapshot();
        }

        private static LF2SpecialAttack CreateSpecialAttack(
            SimulationWorld world,
            int objectId,
            int slot)
        {
            LF2CharacterData data = CreateData("B5SystemTableAttacker", objectId);
            var entity = new LF2SpecialAttack
            {
                Name = data.name,
                ObjectId = objectId,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = 0;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.RelationTeam = 1;
            return entity;
        }

        private static TypedCharacter CreateCharacter(
            SimulationWorld world,
            int objectId,
            int slot,
            params LF2ArmorData[] armors)
        {
            LF2CharacterData data = CreateData("B5SystemTableTarget", objectId);
            data.armors = new List<LF2ArmorData>(armors);
            var entity = new TypedCharacter
            {
                ObjectId = objectId,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.PP = 500;
            entity.KillCount = 0;
            entity.RelationTeam = 2;
            return entity;
        }

        private static LF2CharacterData CreateData(string name, int objectId)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = id == 7 ? LF2States.Defending : LF2States.Standing,
                    wait = 100,
                    next = id,
                });
            }

            return new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = frames,
            };
        }

        private static LF2ArmorData CreateArmor()
        {
            return new LF2ArmorData
            {
                type = 1,
                ratio = 0,
                decrease = 50,
                mp = 0,
                fall = -1,
                bdefend = -1,
                injury = -1,
                hp = 0,
                delay = -1,
            };
        }

        private static InteractionArea CreateHit()
        {
            return new InteractionArea
            {
                kind = 0,
                injury = 20,
                fall = 0,
                dvx = 0,
                dvy = 0,
                arest = 10,
                vrest = 0,
                effect = 0,
                bdefend = 0,
            };
        }

        private sealed class TypedCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Character;
            }
        }
    }
}
#endif
