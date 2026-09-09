#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25IArmorRecoveryEditorTests
    {
        [Test]
        public void PlacementIsStrictlyBetweenReactionTailAndAttackerRest()
        {
            string source = File.ReadAllText(Path.Combine(
                UnityEngine.Application.dataPath,
                "NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs"));
            int reaction = source.IndexOf("AdvanceNativeReactionAndStatusTail(", System.StringComparison.Ordinal);
            int armor = source.IndexOf("AdvanceNativeArmorRecovery(", System.StringComparison.Ordinal);
            int rest = source.IndexOf("DecrementNativeAttackerRest(", System.StringComparison.Ordinal);

            Assert.That(reaction, Is.GreaterThanOrEqualTo(0));
            Assert.That(armor, Is.GreaterThan(reaction));
            Assert.That(rest, Is.GreaterThan(armor));
        }

        [Test]
        public void BrokenArmorCountsDownReloadsAndThenHoldsPositiveHp()
        {
            int armorHp = 0;
            int timer = 3;

            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, true, true, 10, 3);
            Assert.That((armorHp, timer), Is.EqualTo((0, 2)));
            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, true, true, 10, 3);
            Assert.That((armorHp, timer), Is.EqualTo((0, 1)));
            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, true, true, 10, 3);
            Assert.That((armorHp, timer), Is.EqualTo((10, 3)));
            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, true, true, 10, 3);
            Assert.That((armorHp, timer), Is.EqualTo((10, 3)));
        }

        [Test]
        public void BodyGateFreezesAndTimerZeroHandlesMissingOrZeroHpProfile()
        {
            int armorHp = 0;
            int timer = 2;
            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, false, true, 10, 3);
            Assert.That((armorHp, timer), Is.EqualTo((0, 2)));

            timer = 0;
            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, true, false, 0, 0);
            Assert.That((armorHp, timer), Is.EqualTo((0, -1)));

            timer = 0;
            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, true, true, 0, 8);
            Assert.That((armorHp, timer), Is.EqualTo((0, -1)));

            timer = 0;
            BattleNativeArmorRecoveryKernel.Advance(
                ref armorHp, ref timer, true, true, 7, 0);
            Assert.That((armorHp, timer), Is.EqualTo((7, -1)));
        }

        [Test]
        public void ProductionOwnerDisablesTimerZeroWhenFormalProfileIsUnavailable()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(world);
            character.Runtime.RuntimeArmorHp118 = 0;
            character.Runtime.ArmorRecoveryTimer11C = 0;

            world.LateEntityUpdateAll(1);

            Assert.That(character.Runtime.RuntimeArmorHp118, Is.Zero);
            Assert.That(character.Runtime.ArmorRecoveryTimer11C, Is.EqualTo(-1));
        }

        [Test]
        public void ProductionOwnerHonorsRelationAndHoldGatesWithTypeThreeException()
        {
            var heldWorld = new SimulationWorld();
            LF2Character held = CreateCharacter(heldWorld);
            held.FrameDelay = 2;
            held.Runtime.ArmorRecoveryTimer11C = 0;
            heldWorld.LateEntityUpdateAll(1);
            Assert.That(held.Runtime.ArmorRecoveryTimer11C, Is.Zero);

            var linkedWorld = new SimulationWorld();
            LF2Character linked = CreateCharacter(linkedWorld);
            linked.Runtime.LinkState = -1;
            linked.Runtime.ArmorRecoveryTimer11C = 0;
            linkedWorld.LateEntityUpdateAll(1);
            Assert.That(linked.Runtime.ArmorRecoveryTimer11C, Is.Zero);

            var typeThreeWorld = new SimulationWorld();
            LF2Character typeThree = CreateCharacter(
                typeThreeWorld,
                new TypedCharacter(LF2ObjectType.SpecialAttack),
                50);
            typeThree.FrameDelay = 2;
            typeThree.Runtime.ArmorRecoveryTimer11C = 0;
            typeThreeWorld.LateEntityUpdateAll(1);
            Assert.That(typeThree.Runtime.ArmorRecoveryTimer11C, Is.EqualTo(-1));
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            LF2Character character = null,
            int slot = 0)
        {
            var data = new LF2CharacterData
            {
                name = "C25iNoProfile",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 100,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            character ??= new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = 7900 + slot;
            character.FrameCache.Load(new LF2CharacterDataWrapper(character.ObjectId, data));
            character.Initialize(500, 500);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Trans.SyncDirectFrameData(100, 0, 0);
            character.SetRequiredRuntimeSlot(slot);
            character.Runtime.SuppressLateFrameTickUntilTick = 0;
            world.Register(character);
            return character;
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly int dataType;

            internal TypedCharacter(LF2ObjectType dataType)
            {
                this.dataType = (int)dataType;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => dataType;
        }
    }
}
#endif
