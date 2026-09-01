#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class FrameInputSetSeamEditorTests
    {
        [Test]
        public void PublicValueContractPreservesBitsOrderEdgesAndGoldenHash()
        {
            Assert.That((byte)SimulationInputButtons.Right, Is.EqualTo(1));
            Assert.That((byte)SimulationInputButtons.Left, Is.EqualTo(2));
            Assert.That((byte)SimulationInputButtons.Up, Is.EqualTo(4));
            Assert.That((byte)SimulationInputButtons.Down, Is.EqualTo(8));
            Assert.That((byte)SimulationInputButtons.Attack, Is.EqualTo(16));
            Assert.That((byte)SimulationInputButtons.Jump, Is.EqualTo(32));
            Assert.That((byte)SimulationInputButtons.Defend, Is.EqualTo(64));

            var players = new[]
            {
                new SimulationPlayerInput(
                    0,
                    (SimulationInputButtons)127,
                    (SimulationInputButtons)85,
                    (SimulationInputButtons)42),
                new SimulationPlayerInput(
                    3,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.Jump | SimulationInputButtons.Defend),
                new SimulationPlayerInput(
                    19,
                    SimulationInputButtons.None,
                    SimulationInputButtons.None,
                    (SimulationInputButtons)127),
            };
            var frame = new FrameInputSet(123456789, players);

            Assert.That(frame.IsCanonicalFor(123456789, new[] { 0, 3, 19 }), Is.True);
            Assert.That(frame.IsCanonicalFor(123456789, new[] { 3, 0, 19 }), Is.False);
            Assert.That(frame.Players[0].CanonicalEquals(players[0]), Is.True);
            Assert.That(frame.Players[1].CanonicalEquals(players[1]), Is.True);
            Assert.That(frame.Players[2].CanonicalEquals(players[2]), Is.True);
            Assert.That(frame.GetCanonicalHash64(), Is.EqualTo(0x25B94F895B464DCBUL));
        }

        [Test]
        public void ClientReusableFrameMatchesImmutableValueAndRejectsImmutableReset()
        {
            var storage = new[]
            {
                new SimulationPlayerInput(
                    0,
                    SimulationInputButtons.Left,
                    SimulationInputButtons.Left),
                new SimulationPlayerInput(
                    2,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.None,
                    SimulationInputButtons.Jump),
            };
            FrameInputSet reusable = FrameInputSetPreallocation.CreateReusable();
            reusable.ResetPreallocated(17, storage, 2);
            var immutable = new FrameInputSet(17, storage);

            Assert.That(FrameInputSetPreallocation.IsReusable(reusable), Is.True);
            Assert.That(FrameInputSetPreallocation.IsReusable(immutable), Is.False);
            Assert.That(reusable.TickIndex, Is.EqualTo(immutable.TickIndex));
            Assert.That(reusable.Players.Count, Is.EqualTo(immutable.Players.Count));
            Assert.That(reusable.GetCanonicalHash64(), Is.EqualTo(immutable.GetCanonicalHash64()));
            Assert.Throws<InvalidOperationException>(() =>
                immutable.ResetPreallocated(18, storage, 2));
        }

        [Test]
        public void WarmReusableResetAndHashDoNotAllocate()
        {
            var storage = new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.Right),
                new SimulationPlayerInput(1, SimulationInputButtons.Defend),
            };
            FrameInputSet frame = FrameInputSetPreallocation.CreateReusable();
            ulong hash = 0UL;
            for (int tick = 1; tick <= 16; tick++)
            {
                frame.ResetPreallocated(tick, storage, 2);
                hash ^= frame.GetCanonicalHash64();
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 17; tick <= 4112; tick++)
            {
                frame.ResetPreallocated(tick, storage, 2);
                hash ^= frame.GetCanonicalHash64();
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(hash, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void DenseTraceBuilderPreservesSortedSlotsAndHeldCarryOnly()
        {
            Dictionary<int, FrameInputSet> timeline =
                FrameInputDenseTraceBuilder.BuildTimeline(
                    4,
                    new[] { 2, 0, 2 },
                    new[]
                    {
                        new FrameInputSet(2, new[]
                        {
                            new SimulationPlayerInput(
                                2,
                                SimulationInputButtons.Attack,
                                SimulationInputButtons.Attack),
                        }),
                        new FrameInputSet(3, new[]
                        {
                            new SimulationPlayerInput(
                                0,
                                SimulationInputButtons.Left,
                                SimulationInputButtons.Left),
                        }),
                    });

            Assert.That(timeline.Count, Is.EqualTo(4));
            for (int tick = 1; tick <= 4; tick++)
            {
                Assert.That(timeline[tick].Players.Count, Is.EqualTo(2));
                Assert.That(timeline[tick].Players[0].PlayerSlot, Is.Zero);
                Assert.That(timeline[tick].Players[1].PlayerSlot, Is.EqualTo(2));
                Assert.That(timeline[tick].Players[0].PressedButtons, Is.EqualTo(SimulationInputButtons.None));
                Assert.That(timeline[tick].Players[0].ReleasedButtons, Is.EqualTo(SimulationInputButtons.None));
                Assert.That(timeline[tick].Players[1].PressedButtons, Is.EqualTo(SimulationInputButtons.None));
                Assert.That(timeline[tick].Players[1].ReleasedButtons, Is.EqualTo(SimulationInputButtons.None));
            }

            Assert.That(timeline[1].Players[0].Buttons, Is.EqualTo(SimulationInputButtons.None));
            Assert.That(timeline[1].Players[1].Buttons, Is.EqualTo(SimulationInputButtons.None));
            Assert.That(timeline[2].Players[1].Buttons, Is.EqualTo(SimulationInputButtons.Attack));
            Assert.That(timeline[3].Players[0].Buttons, Is.EqualTo(SimulationInputButtons.Left));
            Assert.That(timeline[4].Players[0].Buttons, Is.EqualTo(SimulationInputButtons.Left));
            Assert.That(timeline[4].Players[1].Buttons, Is.EqualTo(SimulationInputButtons.Attack));
        }
    }
}
#endif
