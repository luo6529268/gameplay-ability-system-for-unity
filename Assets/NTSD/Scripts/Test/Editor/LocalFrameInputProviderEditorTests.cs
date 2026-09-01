#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.Game;
using NTSD.Input;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class LocalFrameInputProviderEditorTests
    {
        [Test]
        public void CharacterInputSourcePreservesExistingCrossedActionMapping()
        {
            AssertActionMapping(
                input => input.SetAttackActionPressed(true),
                SimulationInputButtons.Jump,
                FuncKeyMask.jump);
            AssertActionMapping(
                input => input.SetJumpActionPressed(true),
                SimulationInputButtons.Defend,
                FuncKeyMask.def);
            AssertActionMapping(
                input => input.SetDefendActionPressed(true),
                SimulationInputButtons.Attack,
                FuncKeyMask.att);
        }

        [Test]
        public void PreallocatedFrameInputExposesOnlyCapturedPlayers()
        {
            var storage = new SimulationPlayerInput[8];
            storage[0] = new SimulationPlayerInput(0, SimulationInputButtons.Left);
            storage[1] = new SimulationPlayerInput(2, SimulationInputButtons.Attack);
            storage[2] = new SimulationPlayerInput(5, SimulationInputButtons.Jump);
            storage[3] = new SimulationPlayerInput(7, SimulationInputButtons.Defend);
            FrameInputSet frame = FrameInputSetPreallocation.CreateReusable();

            frame.ResetPreallocated(17, storage, 3);

            Assert.That(frame.TickIndex, Is.EqualTo(17));
            Assert.That(frame.Players.Count, Is.EqualTo(3));
            Assert.That(frame.Players[0].PlayerSlot, Is.Zero);
            Assert.That(frame.Players[1].PlayerSlot, Is.EqualTo(2));
            Assert.That(frame.Players[2].PlayerSlot, Is.EqualTo(5));
            Assert.That(
                frame.GetCanonicalHash64(),
                Is.EqualTo(new FrameInputSet(17, new[]
                {
                    storage[0],
                    storage[1],
                    storage[2],
                }).GetCanonicalHash64()));
        }

        [Test]
        public void DiscardTickRemovesOnlyTheTargetDirectInputPacket()
        {
            var buffer = new SimInputBuffer();
            buffer.EnqueueForTick(3, FuncKeyMask.left, true);
            buffer.EnqueueForTick(3, FuncKeyMask.jump, true);
            buffer.EnqueueForTick(4, FuncKeyMask.right, true);

            buffer.DiscardTick(3);

            Assert.That(buffer.CurrentTickIndex, Is.EqualTo(3));
            Assert.That(buffer.BufferedTickCount, Is.EqualTo(1));
            Assert.That(buffer.TryDequeueAll(3, out _), Is.False);
            Assert.That(buffer.TryDequeueAll(4, out SimInputEventBatch remaining), Is.True);
            Assert.That(remaining.Count, Is.EqualTo(1));
            Assert.That(remaining[0].key, Is.EqualTo(FuncKeyMask.right));
            Assert.That(remaining[0].down, Is.True);
        }

        [Test]
        public void LocalProviderCapturesEdgesAndReplacesDirectCallbackPacket()
        {
            var world = new SimulationWorld();
            var character = new LF2Character
            {
                ObjectId = 3,
                Team = 1,
                AiControlled = false,
            };
            world.Register(character);
            BindHumanRosterSlot(world, character, 0);

            var controller = (CharacterInputModule)character.Controller;
            var provider = new LocalSimulationFrameInputProvider();
            provider.BindWorld(world);

            controller.SetAttackActionPressed(true);
            FrameInputSet pressed = provider.GetFrameInput(1);
            AssertFrameButtons(
                pressed,
                SimulationInputButtons.Jump,
                SimulationInputButtons.Jump,
                SimulationInputButtons.None);
            provider.BeforeSimTick(1);
            Assert.That(controller.InputBuffer.BufferedTickCount, Is.Zero);
            world.ApplyFrameInputSet(pressed);
            AssertCompletePacket(controller.InputBuffer, 1, FuncKeyMask.jump, true);

            FrameInputSet held = provider.GetFrameInput(2);
            AssertFrameButtons(
                held,
                SimulationInputButtons.Jump,
                SimulationInputButtons.None,
                SimulationInputButtons.None);
            provider.BeforeSimTick(2);
            world.ApplyFrameInputSet(held);
            AssertCompletePacket(controller.InputBuffer, 2, FuncKeyMask.jump, true);

            controller.SetAttackActionPressed(false);
            FrameInputSet released = provider.GetFrameInput(3);
            AssertFrameButtons(
                released,
                SimulationInputButtons.None,
                SimulationInputButtons.None,
                SimulationInputButtons.Jump);
            provider.BeforeSimTick(3);
            Assert.That(controller.InputBuffer.BufferedTickCount, Is.Zero);
            world.ApplyFrameInputSet(released);
            AssertCompletePacket(controller.InputBuffer, 3, FuncKeyMask.jump, false);
        }

        private static void AssertActionMapping(
            System.Action<CharacterInputModule> press,
            SimulationInputButtons expectedButton,
            FuncKeyMask expectedKey)
        {
            var input = new CharacterInputModule();
            press(input);

            Assert.That(
                ((ILocalFrameInputSource)input).CaptureHeldSimulationButtons(),
                Is.EqualTo(expectedButton));
            Assert.That(input.InputBuffer.TryDequeueAll(1, out SimInputEventBatch events), Is.True);
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].key, Is.EqualTo(expectedKey));
            Assert.That(events[0].down, Is.True);
        }

        private static void BindHumanRosterSlot(
            SimulationWorld world,
            LF2Character character,
            int playerSlot)
        {
            BattleSlotRuntimeState slot = world.Runtime.Roster.Slots[playerSlot];
            slot.Active = true;
            slot.IsHuman = true;
            slot.CharacterId = character.ObjectId;
            slot.Team = character.Team;
            slot.InputId = playerSlot;
            slot.AiId = -1;
            slot.RuntimeSlotIndex = character.Runtime.SlotIndex;
            slot.StableId = character.Runtime.StableId;
            world.Runtime.Roster.ActiveSlotCount = 1;
        }

        private static void AssertFrameButtons(
            FrameInputSet frame,
            SimulationInputButtons held,
            SimulationInputButtons pressed,
            SimulationInputButtons released)
        {
            Assert.That(frame.Players.Count, Is.EqualTo(1));
            SimulationPlayerInput player = frame.Players[0];
            Assert.That(player.PlayerSlot, Is.Zero);
            Assert.That(player.Buttons, Is.EqualTo(held));
            Assert.That(player.PressedButtons, Is.EqualTo(pressed));
            Assert.That(player.ReleasedButtons, Is.EqualTo(released));
        }

        private static void AssertCompletePacket(
            SimInputBuffer buffer,
            int tick,
            FuncKeyMask expectedKey,
            bool expectedDown)
        {
            Assert.That(buffer.TryDequeueAll(tick, out SimInputEventBatch events), Is.True);
            Assert.That(events.Count, Is.EqualTo(7));
            bool found = false;
            for (int index = 0; index < events.Count; index++)
            {
                if (events[index].key != expectedKey)
                    continue;

                found = true;
                Assert.That(events[index].down, Is.EqualTo(expectedDown));
                Assert.That(events[index].completePacket, Is.True);
            }

            Assert.That(found, Is.True);
        }
    }
}
#endif
