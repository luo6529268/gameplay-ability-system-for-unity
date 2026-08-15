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
            Assert.That(session.FrameHistory.TryGet(1, out LockstepFrameHistoryEntry entry), Is.True);
            Assert.That(entry.InputHash, Is.EqualTo(scope.Driver.LastAppliedFrameInput.GetCanonicalHash64()));
            Assert.That(session.ChecksumHistory.TryGet(
                1,
                out LockstepChecksumHistoryEntry checksumEntry), Is.True);
            Assert.That(checksumEntry.InputHash, Is.EqualTo(entry.InputHash));
            Assert.That(checksumEntry.HasStateChecksum, Is.False);
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
            Assert.That(session.FrameHistory.Count, Is.Zero);
            Assert.That(session.ChecksumHistory.Count, Is.Zero);
            Assert.That(session.TryAdvanceBuffered(), Is.False);
        }

        [Test]
        public void ReplayingTheSameJournalProducesIdenticalPerTickChecksums()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            FrameInputSet[] recordedFrames;
            ulong[] expectedChecksums;
            using (var source = new DriverScope())
            {
                source.Driver.ApplySettings(ManualChecksumSettings());
                var session = new BattleLockstepSession(
                    source.Driver,
                    identity,
                    0,
                    8,
                    8);
                FrameInputSet[] inputs =
                {
                    CanonicalFrame(
                        1,
                        SimulationInputButtons.Left,
                        SimulationInputButtons.Jump),
                    CanonicalFrame(
                        2,
                        SimulationInputButtons.Left | SimulationInputButtons.Attack,
                        SimulationInputButtons.None),
                    CanonicalFrame(
                        3,
                        SimulationInputButtons.None,
                        SimulationInputButtons.Defend),
                };
                expectedChecksums = new ulong[inputs.Length];
                for (int index = 0; index < inputs.Length; index++)
                {
                    Assert.That(
                        session.TryAdvanceManual(
                            inputs[index],
                            buildPresentation: false),
                        Is.True);
                    expectedChecksums[index] = source.Driver.LastFrameChecksumValue;
                    Assert.That(session.ChecksumHistory.TryGet(
                        index + 1,
                        out LockstepChecksumHistoryEntry checksumEntry), Is.True);
                    Assert.That(checksumEntry.HasStateChecksum, Is.True);
                    Assert.That(checksumEntry.StateChecksum,
                        Is.EqualTo(expectedChecksums[index]));
                }

                recordedFrames = CopyJournal(session.Journal);
            }

            using var replay = new DriverScope();
            replay.Driver.ApplySettings(ManualChecksumSettings());
            var replaySession = new BattleLockstepSession(
                replay.Driver,
                identity,
                0,
                8,
                8);
            for (int index = 0; index < recordedFrames.Length; index++)
            {
                Assert.That(
                    replaySession.TryAdvanceManual(
                        recordedFrames[index],
                        buildPresentation: false),
                    Is.True);
                Assert.That(
                    replay.Driver.LastFrameChecksumValue,
                    Is.EqualTo(expectedChecksums[index]),
                    $"tick={index + 1}");
            }
        }

        [Test]
        public void PresentationPublicationDoesNotChangeCanonicalTickChecksum()
        {
            ulong withoutPresentation;
            using (var logicOnly = new DriverScope())
            {
                logicOnly.Driver.ApplySettings(ManualChecksumSettings());
                Assert.That(
                    logicOnly.Driver.StepOneTick(
                        CanonicalFrame(
                            1,
                            SimulationInputButtons.Left | SimulationInputButtons.Attack,
                            SimulationInputButtons.Defend),
                        ignorePaused: true,
                        buildPresentation: false),
                    Is.True);
                withoutPresentation = logicOnly.Driver.LastFrameChecksumValue;
            }

            using var published = new DriverScope();
            published.Driver.ApplySettings(ManualChecksumSettings());
            Assert.That(
                published.Driver.StepOneTick(
                    CanonicalFrame(
                        1,
                        SimulationInputButtons.Left | SimulationInputButtons.Attack,
                        SimulationInputButtons.Defend),
                    ignorePaused: true,
                    buildPresentation: true),
                Is.True);

            Assert.That(
                published.Driver.LastFrameChecksumValue,
                Is.EqualTo(withoutPresentation));
        }

        private static LockstepSimulationSettings ManualChecksumSettings()
        {
            return new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                enableFrameChecksum = true,
                captureFullFrameSnapshotForDiagnostics = false,
            };
        }

        private static FrameInputSet CanonicalFrame(
            int tick,
            SimulationInputButtons playerTwo,
            SimulationInputButtons playerFive)
        {
            return new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(2, playerTwo),
                new SimulationPlayerInput(5, playerFive),
            });
        }

        private static FrameInputSet[] CopyJournal(LockstepReplayJournal journal)
        {
            var result = new FrameInputSet[journal.Count];
            for (int frameIndex = 0; frameIndex < journal.Count; frameIndex++)
            {
                FrameInputSet source = journal[frameIndex];
                var players = new SimulationPlayerInput[source.Players.Count];
                for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
                    players[playerIndex] = source.Players[playerIndex];
                result[frameIndex] = new FrameInputSet(source.TickIndex, players);
            }
            return result;
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
