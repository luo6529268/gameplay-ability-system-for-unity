#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class CharacterInputLiveSlotLoopEditorTests
    {
        private const BindingFlags StaticMembers =
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        [TearDown]
        public void TearDown()
        {
            SetMutationHook(null);
        }

        [Test]
        public void CharacterInputAll_LiveAscendingSlots_AdmitsHighNewbornAndDefersRecycledLowSlot()
        {
            var world = new SimulationWorld();
            LF2Character original = RegisterCharacter(world, 0, 100);
            LF2Character slotOne = RegisterCharacter(world, 1, 101);
            LF2Character slotTwo = RegisterCharacter(world, 2, 102);
            LF2Character replacement = null;
            LF2Character highNewborn = null;
            var visited = new List<LF2Entity>(4);

            SetMutationHook((activeWorld, entity) =>
            {
                visited.Add(entity);
                if (!ReferenceEquals(entity, original))
                    return;

                activeWorld.Unregister(original);
                replacement = CreateCharacter(0, 200);
                activeWorld.Register(replacement);
                highNewborn = CreateCharacter(3, 203);
                activeWorld.Register(highNewborn);
            });

            world.CharacterInputAll(2);

            CollectionAssert.AreEqual(
                new LF2Entity[] { original, slotOne, slotTwo, highNewborn },
                visited);
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(0));
            Assert.That(highNewborn.Runtime.SlotIndex, Is.EqualTo(3));
            CollectionAssert.DoesNotContain(visited, replacement);
        }

        [Test]
        public void CharacterInputAll_MutationThrows_StillFlushesDeferredUnregisterAndRestoresTicking()
        {
            var world = new SimulationWorld();
            LF2Character trigger = RegisterCharacter(world, 0, 300);
            LF2Character removed = RegisterCharacter(world, 1, 301);

            SetMutationHook((activeWorld, entity) =>
            {
                if (!ReferenceEquals(entity, trigger))
                    return;

                activeWorld.Unregister(removed);
                throw new InvalidOperationException("character-input-mutation-probe");
            });

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => world.CharacterInputAll(2));
            Assert.That(exception.Message, Is.EqualTo("character-input-mutation-probe"));

            SetMutationHook(null);
            LF2Character replacement = CreateCharacter(1, 401);
            Assert.DoesNotThrow(() => world.Register(replacement));
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(1));
            Assert.DoesNotThrow(() => world.CharacterInputAll(3));
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            LF2Character character = CreateCharacter(slot, objectId);
            world.Register(character);
            return character;
        }

        private static LF2Character CreateCharacter(int slot, int objectId)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = $"CharacterInputLiveSlot_{slot}_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(slot);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Runtime.SetPosition(slot * 20, 0, 0);
            character.Runtime.SyncIntegerPosition();
            character.Controller = new EmptyController();
            character.AiControlled = false;
            return character;
        }

        private static void SetMutationHook(Action<SimulationWorld, LF2Entity> hook)
        {
            FieldInfo field = typeof(SimulationWorld).GetField(
                "CharacterInputPassMutationOverrideForSelfCheck",
                StaticMembers);
            Assert.That(field, Is.Not.Null);
            field.SetValue(null, hook);
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
