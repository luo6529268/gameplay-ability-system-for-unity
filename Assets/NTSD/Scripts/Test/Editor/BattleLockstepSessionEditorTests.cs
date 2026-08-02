#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleLockstepSessionEditorTests
    {
        [Test]
        public void MissingPacketAndWrongTickNeverAdvanceDriver()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);

            Assert.That(session.SubmitBuffered(
                StrictDelayedInputBufferEditorTests.Packet(identity, 1, 2,
                    SimulationInputButtons.Attack)), Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(session.TryAdvanceBuffered(), Is.False);
            Assert.That(session.CurrentTick, Is.Zero);
            Assert.That(scope.Driver.CurrentTickIndex, Is.Zero);
            Assert.That(scope.Driver.StepOneTick(FrameInputSet.Empty(2), ignorePaused: true), Is.False);
            Assert.That(scope.Driver.CurrentTickIndex, Is.Zero);
        }

        [Test]
        public void LockstepBufferedProviderNullAndLocalFallbackProviderBothFailClosed()
        {
            using var scope = new DriverScope();
            scope.Driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.LockstepBuffered,
                requireInputFrameReady = true,
            });

            scope.Driver.SetFrameInputProvider(null);
            Assert.That(scope.Driver.StepOneTick(ignorePaused: true), Is.False);
            scope.Driver.SetFrameInputProvider(new LocalSimulationFrameInputProvider());
            Assert.That(scope.Driver.StepOneTick(ignorePaused: true), Is.False);
            Assert.That(scope.Driver.CurrentTickIndex, Is.Zero);
        }

        [Test]
        public void DelayTwoRequiresExplicitNeutralBootstrapAndTargetsExactFutureTick()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 2, 8, 8);

            Assert.That(session.SubmitLocal(2, SimulationInputButtons.Attack),
                Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(session.SubmitLocal(5, SimulationInputButtons.None),
                Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(session.TryAdvanceBuffered(), Is.False);
            Assert.That(session.LastReason, Is.EqualTo(LockstepProtocolReason.BootstrapRequired));

            Assert.That(session.BootstrapNeutralDelayFrames(), Is.True);
            Assert.That(session.TryAdvanceBuffered(), Is.True);
            AssertNeutral(scope.Driver.LastAppliedFrameInput, 1);
            Assert.That(session.TryAdvanceBuffered(), Is.True);
            AssertNeutral(scope.Driver.LastAppliedFrameInput, 2);
            Assert.That(session.TryAdvanceBuffered(), Is.True);
            Assert.That(scope.Driver.LastAppliedFrameInput.TickIndex, Is.EqualTo(3));
            Assert.That(scope.Driver.LastAppliedFrameInput.Players[0].Buttons,
                Is.EqualTo(SimulationInputButtons.Attack));
        }

        [Test]
        public void DelayZeroTargetsNextSimulationTickWithoutBootstrapPackets()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);

            Assert.That(session.BootstrapNeutralDelayFrames(), Is.True);
            Assert.That(session.SubmitLocal(5, SimulationInputButtons.Jump),
                Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(session.SubmitLocal(2, SimulationInputButtons.Left),
                Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(session.TryAdvanceBuffered(), Is.True);
            Assert.That(scope.Driver.LastAppliedFrameInput.TickIndex, Is.EqualTo(1));
            Assert.That(scope.Driver.LastAppliedFrameInput.Players[0].PlayerSlot, Is.EqualTo(2));
            Assert.That(scope.Driver.LastAppliedFrameInput.Players[1].PlayerSlot, Is.EqualTo(5));
        }

        [Test]
        public void DriverFactoryConsumesConfiguredInputDelay()
        {
            using var scope = new DriverScope();
            scope.Driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.LockstepBuffered,
                inputDelayTicks = 2,
                requireInputFrameReady = true,
            });
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();

            BattleLockstepSession session = scope.Driver.CreateStrictLockstepSession(identity, 8, 8);

            Assert.That(session.InputDelayTicks, Is.EqualTo(2));
            Assert.That(session.BootstrapComplete, Is.False);
        }

        [Test]
        public void BufferedAndManualUseSameExplicitDriverTransaction()
        {
            TickWitness local = RunLocal();
            TickWitness buffered = RunBuffered();
            TickWitness manual = RunManual();

            Assert.That(local.Tick, Is.EqualTo(manual.Tick));
            Assert.That(local.InputHash, Is.EqualTo(manual.InputHash));
            Assert.That(local.RngState, Is.EqualTo(manual.RngState));
            Assert.That(local.RngCalls, Is.EqualTo(manual.RngCalls));
            Assert.That(buffered.Tick, Is.EqualTo(manual.Tick));
            Assert.That(buffered.InputHash, Is.EqualTo(manual.InputHash));
            Assert.That(buffered.RngState, Is.EqualTo(manual.RngState));
            Assert.That(buffered.RngCalls, Is.EqualTo(manual.RngCalls));
            Assert.That(buffered.Player0, Is.EqualTo(2));
            Assert.That(buffered.Player1, Is.EqualTo(5));
        }

        [Test]
        public void ResetClearsPendingInputAndJournalCursor()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            Assert.That(session.SubmitLocal(2, SimulationInputButtons.Attack),
                Is.EqualTo(LockstepProtocolReason.None));

            session.Reset();

            Assert.That(session.Buffer.BufferedFrameCount, Is.Zero);
            Assert.That(session.Journal.Count, Is.Zero);
            Assert.That(session.TryAdvanceBuffered(), Is.False);
        }

        private static TickWitness RunBuffered()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            session.SubmitBuffered(StrictDelayedInputBufferEditorTests.Packet(
                identity, 1, 5, SimulationInputButtons.Jump));
            session.SubmitBuffered(StrictDelayedInputBufferEditorTests.Packet(
                identity, 1, 2, SimulationInputButtons.Left));
            Assert.That(session.TryAdvanceBuffered(), Is.True);
            return TickWitness.Capture(scope.Driver);
        }

        private static TickWitness RunLocal()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            session.SubmitLocal(5, SimulationInputButtons.Jump);
            session.SubmitLocal(2, SimulationInputButtons.Left);
            Assert.That(session.TryAdvanceBuffered(), Is.True);
            return TickWitness.Capture(scope.Driver);
        }

        private static TickWitness RunManual()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var frame = new FrameInputSet(1, new[]
            {
                new SimulationPlayerInput(2, SimulationInputButtons.Left),
                new SimulationPlayerInput(5, SimulationInputButtons.Jump),
            });
            Assert.That(session.TryAdvanceManual(frame), Is.True);
            return TickWitness.Capture(scope.Driver);
        }

        private static void AssertNeutral(FrameInputSet frame, int tick)
        {
            Assert.That(frame.TickIndex, Is.EqualTo(tick));
            Assert.That(frame.Players.Count, Is.EqualTo(2));
            Assert.That(frame.Players[0].Buttons, Is.EqualTo(SimulationInputButtons.None));
            Assert.That(frame.Players[1].Buttons, Is.EqualTo(SimulationInputButtons.None));
        }

        private readonly struct TickWitness
        {
            public TickWitness(
                int tick,
                ulong inputHash,
                uint rngState,
                ulong rngCalls,
                int player0,
                int player1)
            {
                Tick = tick;
                InputHash = inputHash;
                RngState = rngState;
                RngCalls = rngCalls;
                Player0 = player0;
                Player1 = player1;
            }

            public int Tick { get; }
            public ulong InputHash { get; }
            public uint RngState { get; }
            public ulong RngCalls { get; }
            public int Player0 { get; }
            public int Player1 { get; }

            public static TickWitness Capture(SimulationTickDriver driver)
            {
                return new TickWitness(
                    driver.CurrentTickIndex,
                    driver.LastAppliedFrameInput.GetCanonicalHash64(),
                    driver.World.Rng.State,
                    driver.World.Rng.CallCount,
                    driver.LastAppliedFrameInput.Players[0].PlayerSlot,
                    driver.LastAppliedFrameInput.Players[1].PlayerSlot);
            }
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField", flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("BattleLockstepSessionTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                Driver.RecreateWorld();
                Driver.SetPaused(true);
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
#endif
