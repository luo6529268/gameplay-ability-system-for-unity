#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class InProcessLockstepAuthoritySessionEditorTests
    {
        [Test]
        public void ServerAndTwoClientsConsumeTheSameContinuousAuthorityJournal()
        {
            const int tickCount = 48;
            ulong[] first = RunScenario(tickCount);
            ulong[] second = RunScenario(tickCount);

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void AuthorityJournalOwnsFrameStorageBeforeWorldsConsumeIt()
        {
            LockstepStartBarrier barrier = CreateBarrier();
            InProcessLockstepAuthoritySession session = CreateSession(barrier, 4);
            var players = new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.Left),
                new SimulationPlayerInput(1, SimulationInputButtons.Attack),
            };
            var source = new FrameInputSet(1, players);

            Assert.That(session.TryAdvance(source), Is.True);
            ulong authorityHash = session.AuthorityJournal[0].GetCanonicalHash64();
            players[0] = new SimulationPlayerInput(0, SimulationInputButtons.Right);
            players[1] = new SimulationPlayerInput(1, SimulationInputButtons.None);

            Assert.That(
                session.AuthorityJournal[0].GetCanonicalHash64(),
                Is.EqualTo(authorityHash));
            Assert.That(session.Server.Journal[0].GetCanonicalHash64(), Is.EqualTo(authorityHash));
            Assert.That(session.Clients[0].Journal[0].GetCanonicalHash64(), Is.EqualTo(authorityHash));
            Assert.That(session.Clients[1].Journal[0].GetCanonicalHash64(), Is.EqualTo(authorityHash));
        }

        [Test]
        public void WrongTickFailsBeforeAnyWorldAdvances()
        {
            LockstepStartBarrier barrier = CreateBarrier();
            InProcessLockstepAuthoritySession session = CreateSession(barrier, 4);

            Assert.That(session.TryAdvance(Frame(2)), Is.False);
            Assert.That(session.Status, Is.EqualTo(InProcessLockstepAuthorityStatus.Faulted));
            Assert.That(
                session.LastFailureReason,
                Is.EqualTo(InProcessAuthorityFailureReason.WrongFrameTick));
            Assert.That(session.AuthorityJournal.Count, Is.Zero);
            Assert.That(session.Server.CurrentTick, Is.Zero);
            Assert.That(session.Clients[0].CurrentTick, Is.Zero);
            Assert.That(session.Clients[1].CurrentTick, Is.Zero);
        }

        [Test]
        public void MismatchedStartBarrierIsRejectedBeforeTickZero()
        {
            LockstepStartBarrier barrier = CreateBarrier();
            LockstepStartBarrier different = CreateBarrier(policyVersion: 2);
            var server = new InProcessBattleKernelHost(barrier, 0, 4);
            var clients = new[]
            {
                new InProcessBattleKernelHost(barrier, 1, 4),
                new InProcessBattleKernelHost(different, 2, 4),
            };

            Assert.Throws<ArgumentException>(() =>
                new InProcessLockstepAuthoritySession(barrier, server, clients, 4));
            Assert.That(server.CurrentTick, Is.Zero);
            Assert.That(clients[0].CurrentTick, Is.Zero);
            Assert.That(clients[1].CurrentTick, Is.Zero);
        }

        [Test]
        public void FirstChecksumDifferenceLatchesReplicaAndTick()
        {
            LockstepStartBarrier barrier = CreateBarrier();
            InProcessLockstepAuthoritySession session = CreateSession(barrier, 4);
            Assert.That(session.TryAdvance(Frame(1)), Is.True);

            session.Clients[1].WorldForDiagnostics.Rng.Seed(0xDEADBEEFu);

            Assert.That(session.TryAdvance(Frame(2)), Is.False);
            Assert.That(
                session.LastFailureReason,
                Is.EqualTo(InProcessAuthorityFailureReason.StateChecksumMismatch));
            Assert.That(session.FirstDifference.HasDifference, Is.True);
            Assert.That(session.FirstDifference.TickIndex, Is.EqualTo(2));
            Assert.That(session.FirstDifference.ClientReplicaIndex, Is.EqualTo(2));
            Assert.That(
                session.FirstDifference.ClientStateChecksum,
                Is.Not.EqualTo(session.FirstDifference.ServerStateChecksum));
            Assert.That(session.FirstDifference.HasStructuredWitness, Is.True);
            Assert.That(
                session.FirstDifference.StructuredWitness.FirstDifferingDomain,
                Is.EqualTo(InProcessLockstepChecksumDomain.Rng));
            Assert.That(
                session.FirstDifference.StructuredWitness.ServerRngState,
                Is.Not.EqualTo(session.FirstDifference.StructuredWitness.ClientRngState));
            Assert.That(
                session.FirstDifference.StructuredWitness.ServerSnapshot,
                Is.Not.Null);
            Assert.That(
                session.FirstDifference.StructuredWitness.ClientSnapshot,
                Is.Not.Null);
            Assert.That(session.TryAdvance(Frame(3)), Is.False);
            Assert.That(
                session.LastFailureReason,
                Is.EqualTo(InProcessAuthorityFailureReason.SessionAlreadyFaulted));
        }

        [Test]
        public void SlotGenerationDifferenceCapturesFirstSlotAndBothGenerations()
        {
            const int runtimeSlot = 50;
            LockstepStartBarrier barrier = CreateBarrier();
            InProcessLockstepAuthoritySession session = CreateSession(barrier, 4);
            RegisterTestCharacter(session.Server, runtimeSlot, 100, 1);
            RegisterTestCharacter(session.Clients[0], runtimeSlot, 100, 1);
            LF2Character divergentClient = RegisterTestCharacter(
                session.Clients[1], runtimeSlot, 100, 1);

            Assert.That(session.TryAdvance(Frame(1)), Is.True);
            uint serverGeneration = session.Server.WorldForDiagnostics
                .RuntimeSlotsForServices
                .GetReadOnlyView(runtimeSlot)
                .Generation;

            session.Clients[1].WorldForDiagnostics.Unregister(divergentClient);
            RegisterTestCharacter(session.Clients[1], runtimeSlot, 101, 1);
            uint clientGeneration = session.Clients[1].WorldForDiagnostics
                .RuntimeSlotsForServices
                .GetReadOnlyView(runtimeSlot)
                .Generation;

            Assert.That(clientGeneration, Is.Not.EqualTo(serverGeneration));
            Assert.That(session.TryAdvance(Frame(2)), Is.False);
            Assert.That(
                session.LastFailureReason,
                Is.EqualTo(InProcessAuthorityFailureReason.StateChecksumMismatch));
            Assert.That(session.FirstDifference.HasStructuredWitness, Is.True);
            Assert.That(
                session.FirstDifference.StructuredWitness.FirstDifferingDomain,
                Is.EqualTo(InProcessLockstepChecksumDomain.Slots));
            Assert.That(
                session.FirstDifference.StructuredWitness.FirstDifferingRuntimeSlot,
                Is.EqualTo(runtimeSlot));
            Assert.That(
                session.FirstDifference.StructuredWitness.ServerSlotGeneration,
                Is.EqualTo(serverGeneration));
            Assert.That(
                session.FirstDifference.StructuredWitness.ClientSlotGeneration,
                Is.EqualTo(clientGeneration));
        }

        [Test]
        public void ServerAndTwoClientsKeepRealTestEntityJournalChecksumsAligned()
        {
            const int tickCount = 12;
            LockstepStartBarrier barrier = CreateBarrier();
            InProcessLockstepAuthoritySession session = CreateSession(barrier, tickCount);
            RegisterTestCharacter(session.Server, 50, 100, 1);
            RegisterTestCharacter(session.Clients[0], 50, 100, 1);
            RegisterTestCharacter(session.Clients[1], 50, 100, 1);

            for (int tick = 1; tick <= tickCount; tick++)
            {
                Assert.That(session.TryAdvance(Frame(tick)), Is.True, $"tick={tick}");
                Assert.That(
                    session.Clients[0].LastStateChecksum,
                    Is.EqualTo(session.Server.LastStateChecksum));
                Assert.That(
                    session.Clients[1].LastStateChecksum,
                    Is.EqualTo(session.Server.LastStateChecksum));
            }

            Assert.That(session.Server.WorldForDiagnostics.ObjectCount, Is.EqualTo(1));
            Assert.That(session.Clients[0].WorldForDiagnostics.ObjectCount, Is.EqualTo(1));
            Assert.That(session.Clients[1].WorldForDiagnostics.ObjectCount, Is.EqualTo(1));
            Assert.That(session.Server.DiagnosticSnapshotCaptureCount, Is.Zero);
            Assert.That(session.Clients[0].DiagnosticSnapshotCaptureCount, Is.Zero);
            Assert.That(session.Clients[1].DiagnosticSnapshotCaptureCount, Is.Zero);
        }

        private static ulong[] RunScenario(int tickCount)
        {
            LockstepStartBarrier barrier = CreateBarrier();
            InProcessLockstepAuthoritySession session = CreateSession(barrier, tickCount);
            var result = new ulong[tickCount];
            for (int tick = 1; tick <= tickCount; tick++)
            {
                Assert.That(session.TryAdvance(Frame(tick)), Is.True, $"tick={tick}");
                Assert.That(session.CurrentTick, Is.EqualTo(tick));
                Assert.That(session.Server.CurrentTick, Is.EqualTo(tick));
                Assert.That(session.Clients[0].CurrentTick, Is.EqualTo(tick));
                Assert.That(session.Clients[1].CurrentTick, Is.EqualTo(tick));
                Assert.That(
                    session.Clients[0].LastStateChecksum,
                    Is.EqualTo(session.Server.LastStateChecksum));
                Assert.That(
                    session.Clients[1].LastStateChecksum,
                    Is.EqualTo(session.Server.LastStateChecksum));
                result[tick - 1] = session.Server.LastStateChecksum;
            }

            Assert.That(session.AuthorityJournal.Count, Is.EqualTo(tickCount));
            Assert.That(session.FirstDifference.HasDifference, Is.False);
            Assert.That(session.Server.DiagnosticSnapshotCaptureCount, Is.Zero);
            Assert.That(session.Clients[0].DiagnosticSnapshotCaptureCount, Is.Zero);
            Assert.That(session.Clients[1].DiagnosticSnapshotCaptureCount, Is.Zero);
            return result;
        }

        private static LF2Character RegisterTestCharacter(
            InProcessBattleKernelHost host,
            int runtimeSlot,
            int objectId,
            int team)
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
                name = $"S0Witness_{runtimeSlot}_{objectId}",
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
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.Team = team;
            character.RelationTeam = team;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Runtime.PP = 0;
            character.Runtime.KillCount = -1;
            character.Runtime.Unk3FC = -1001;
            character.Runtime.Unk400 = -1001;
            character.Runtime.SetPosition(runtimeSlot * 4, 0, 200);
            character.Runtime.SyncIntegerPosition();
            character.Controller = new EmptyController();
            character.AiControlled = false;
            host.WorldForDiagnostics.Register(character);
            return character;
        }

        private static InProcessLockstepAuthoritySession CreateSession(
            LockstepStartBarrier barrier,
            int capacity)
        {
            var server = new InProcessBattleKernelHost(barrier, 0, capacity);
            var clients = new[]
            {
                new InProcessBattleKernelHost(barrier, 1, capacity),
                new InProcessBattleKernelHost(barrier, 2, capacity),
            };
            return new InProcessLockstepAuthoritySession(
                barrier,
                server,
                clients,
                capacity);
        }

        private static LockstepStartBarrier CreateBarrier(int policyVersion = 1)
        {
            var identity = new LockstepSessionIdentity(
                LockstepSessionIdentity.CurrentSchemaVersion,
                sessionId: 0x51000001UL,
                seed: 0x51A7u,
                catalogFingerprint: 0xCA7A10UL,
                stageFingerprint: 0x57A6EUL,
                playerSlots: new[] { 1, 0 });
            return new LockstepStartBarrier(
                identity,
                ruleFingerprint: 0xC0DE0001UL,
                policyVersion,
                BattleRuntimeProfilePolicy.Create(BattleRuntimeProfile.Authority400));
        }

        private static FrameInputSet Frame(int tick)
        {
            SimulationInputButtons playerZero = (tick % 4) switch
            {
                0 => SimulationInputButtons.None,
                1 => SimulationInputButtons.Right,
                2 => SimulationInputButtons.Right | SimulationInputButtons.Attack,
                _ => SimulationInputButtons.Jump,
            };
            SimulationInputButtons playerOne = (tick % 3) switch
            {
                0 => SimulationInputButtons.Left,
                1 => SimulationInputButtons.Defend,
                _ => SimulationInputButtons.None,
            };
            return new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(0, playerZero),
                new SimulationPlayerInput(1, playerOne),
            });
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;

            public int Dirv() => 0;

            public (int dx, int dz) GetMoveInput() => (0, 0);

            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
