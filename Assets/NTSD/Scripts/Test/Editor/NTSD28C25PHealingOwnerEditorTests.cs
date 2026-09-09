#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25PHealingOwnerEditorTests
    {
        [Test]
        public void PlacementFollowsPreviousActionCommitInsideLiveSlotTail()
        {
            string source = File.ReadAllText(Path.Combine(
                UnityEngine.Application.dataPath,
                "NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs"));
            int previousActionCommit = source.IndexOf(
                "obj.MirrorLatePrevFrame();",
                System.StringComparison.Ordinal);
            int healing = source.IndexOf(
                "AdvanceNativeHealing(obj);",
                System.StringComparison.Ordinal);

            Assert.That(previousActionCommit, Is.GreaterThanOrEqualTo(0));
            Assert.That(healing, Is.GreaterThan(previousActionCommit));
        }

        [Test]
        public void KernelAdvancesEncodedThenOrdinaryTimerInAuthorityOrder()
        {
            int hp = 400;
            int encoded = 1009;
            int ordinary = 9;

            BattleNativeHealingKernel.Advance(
                ref hp,
                500,
                ref encoded,
                ref ordinary,
                false);

            Assert.That(hp, Is.EqualTo(416));
            Assert.That(encoded, Is.EqualTo(1008));
            Assert.That(ordinary, Is.EqualTo(8));
        }

        [Test]
        public void KernelClosesEncodedThousandBoundaryAndArmsState1700Last()
        {
            int hp = 400;
            int encoded = 1001;
            int ordinary = 0;
            BattleNativeHealingKernel.Advance(
                ref hp,
                500,
                ref encoded,
                ref ordinary,
                false);
            Assert.That((hp, encoded), Is.EqualTo((408, 0)));

            encoded = 1009;
            BattleNativeHealingKernel.Advance(
                ref hp,
                500,
                ref encoded,
                ref ordinary,
                true);
            Assert.That(encoded, Is.EqualTo(1100));
        }

        [Test]
        public void ProductionOwnerRunsForLivingTypeZeroAndGlobalTailDoesNotRepeatIt()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(world, null, 0, LF2States.Standing);
            character.Health.HP = 400;
            character.Health.HPBound = 500;
            character.HealTimer = 1009;
            character.CatchTimer = 9;

            world.LateEntityUpdateAll(1);

            Assert.That(character.Health.HP, Is.EqualTo(416));
            Assert.That(character.HealTimer, Is.EqualTo(1008));
            Assert.That(character.CatchTimer, Is.EqualTo(8));

            world.EntityPostFrameTailAll(1);

            Assert.That(character.Health.HP, Is.EqualTo(416));
            Assert.That(character.HealTimer, Is.EqualTo(1008));
            Assert.That(character.CatchTimer, Is.EqualTo(8));
        }

        [Test]
        public void ProductionOwnerSkipsDeadTypeZeroAndCurrentDatNonCharacter()
        {
            var deadWorld = new SimulationWorld();
            LF2Character dead = CreateCharacter(deadWorld);
            dead.Health.HP = 0;
            dead.Health.HPBound = 500;
            dead.HealTimer = 1009;
            dead.CatchTimer = 9;
            deadWorld.LateEntityUpdateAll(1);
            Assert.That((dead.HealTimer, dead.CatchTimer), Is.EqualTo((1009, 9)));

            var nonCharacterWorld = new SimulationWorld();
            LF2Character nonCharacter = CreateCharacter(
                nonCharacterWorld,
                new TypedCharacter(LF2ObjectType.SpecialAttack));
            nonCharacter.Health.HP = 400;
            nonCharacter.Health.HPBound = 500;
            nonCharacter.HealTimer = 1009;
            nonCharacter.CatchTimer = 9;
            nonCharacterWorld.LateEntityUpdateAll(1);
            Assert.That(nonCharacter.Health.HP, Is.EqualTo(400));
            Assert.That((nonCharacter.HealTimer, nonCharacter.CatchTimer),
                Is.EqualTo((1009, 9)));
        }

        [Test]
        public void ProductionState1700ArmsEncodedTimerAtEndOfHealingTail()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(
                world,
                null,
                0,
                1700);
            character.Health.HP = 400;
            character.Health.HPBound = 500;
            character.HealTimer = 1009;

            world.LateEntityUpdateAll(1);

            Assert.That(character.Health.HP, Is.EqualTo(408));
            Assert.That(character.HealTimer, Is.EqualTo(1100));
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            LF2Character character = null,
            int slot = 0,
            int state = LF2States.Standing)
        {
            var data = new LF2CharacterData
            {
                name = "C25pHealing",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = state,
                        wait = 100,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            character ??= new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = 8000 + slot;
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(character.ObjectId, data));
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
