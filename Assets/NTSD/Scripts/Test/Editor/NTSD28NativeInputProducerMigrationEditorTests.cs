#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Input;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeInputProducerMigrationEditorTests
    {
        [Test]
        public void DataOrientedRegistrationAndBattleEntryClear_UseNativeHistoryInitialState()
        {
            var world = CreateDataOrientedWorld();
            LF2Character character = RegisterCharacter(world, 0, 801);

            AssertNativeHistoryInitial(character.Runtime);
            character.Runtime.InputHistory[3] = 9;
            character.Runtime.InputHistory[4] = 0;
            character.Runtime.InputHistory[5] = 5;

            world.ClearBattleEntryInputAll();

            AssertNativeHistoryInitial(character.Runtime);
        }

        [Test]
        public void DataOrientedHumanProducer_DefersEdgeUntilSecondPassAndDecaysOnce()
        {
            var world = CreateDataOrientedWorld();
            LF2Character character = RegisterCharacter(world, 0, 802);
            character.Controller.InputBuffer.EnqueueCompletePacketKeyForTick(
                2,
                FuncKeyMask.right,
                true);

            character.RunHumanInputPollPhase(2);

            Assert.That(character.Runtime.KeyRight, Is.EqualTo(1));
            Assert.That(character.Runtime.PrevRight, Is.Zero);
            Assert.That(character.Runtime.CdRight, Is.Zero,
                "the first-pass producer must not create a legacy edge");
            Assert.That(character.Runtime.InputHistory[5], Is.EqualTo(-1));

            world.CharacterInputAll(2);

            Assert.That(character.Runtime.CdRight, Is.EqualTo(5));
            Assert.That(character.Runtime.InputHistory[5], Is.EqualTo(6));
            Assert.That(character.Runtime.NativeInputProxy.EdgeWindow[3], Is.EqualTo(5));
            Assert.That(character.Runtime.NativeInputProxy.Current[3], Is.EqualTo(1));

            character.RunHumanInputPollPhase(3);

            Assert.That(character.Runtime.CdRight, Is.EqualTo(5),
                "held input must not decay in the next first-pass producer");
            Assert.That(character.Runtime.InputHistory[4], Is.EqualTo(-1));
            Assert.That(character.Runtime.InputHistory[5], Is.EqualTo(6));

            world.CharacterInputAll(3);

            Assert.That(character.Runtime.CdRight, Is.EqualTo(4));
            Assert.That(character.Runtime.NativeInputProxy.EdgeWindow[3], Is.EqualTo(4));
            Assert.That(character.Runtime.InputHistory[4], Is.EqualTo(-1));
            Assert.That(character.Runtime.InputHistory[5], Is.EqualTo(6));
        }

        [Test]
        public void LegacyHumanProducer_KeepsExistingEagerEdgeBehavior()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.LegacyCanonical);
            LF2Character character = RegisterCharacter(world, 0, 803);
            character.Controller.InputBuffer.EnqueueCompletePacketKeyForTick(
                2,
                FuncKeyMask.right,
                true);

            character.RunHumanInputPollPhase(2);

            Assert.That(character.Runtime.CdRight, Is.EqualTo(5));
            Assert.That(character.Runtime.InputHistory[5], Is.EqualTo(6));
        }

        [Test]
        public void SynchronizedAiCandidate_WritesButtonsWithoutLegacyEdgeOrHistoryAdvance()
        {
            var random = new NTSD28NativeRandom(0x2811u);
            AiDecisionSnapshot snapshot = CreateAiSnapshot();
            snapshot.SetSynchronizedRngCursor(random.CaptureSynchronizedCursor());
            AiDecisionWitness witness = default;

            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            Assert.That(witness.Input.KeyRight, Is.EqualTo(1));
            Assert.That(witness.Input.CdRight, Is.Zero);
            Assert.That(witness.Input.History1, Is.EqualTo(-1));
            Assert.That(witness.Input.History5, Is.EqualTo(-1));
        }

        [Test]
        public void DataOrientedProxy_CopyPrecedesNativeEdgeProcessing()
        {
            var world = CreateDataOrientedWorld();
            LF2Character target = RegisterCharacter(world, 0, 804);
            LF2Character source = RegisterCharacter(world, 1, 805);
            target.Runtime.InputProxyCounter14C = 2;
            target.Runtime.InputProxySourceSlot178 = 1;
            target.Runtime.InputProxyEnabled17C = 1;
            world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(
                (_, entity) =>
                {
                    if (!ReferenceEquals(entity, source))
                        return;

                    entity.Runtime.KeyRight = 1;
                    entity.Runtime.PrevRight = 0;
                });

            world.CharacterInputAll(2);

            Assert.That(world.LastNTSD28InputProxyCopyCountForDiagnostics, Is.EqualTo(1));
            Assert.That(source.Runtime.NativeInputProxy.EdgeWindow[3], Is.EqualTo(5));
            Assert.That(target.Runtime.NativeInputProxy.EdgeWindow[3], Is.EqualTo(5));
            Assert.That(target.Runtime.InputHistory[5], Is.EqualTo(6));
        }

        [Test]
        public void DataOrientedAiDirectComboRequest_MergesIntoExactBank()
        {
            var world = CreateDataOrientedWorld();
            LF2Character character = RegisterCharacter(world, 0, 806);
            character.AiControlled = true;
            world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(
                (_, entity) => entity.Runtime.ComboDua = 3);

            world.CharacterInputAll(2);

            Assert.That(character.Runtime.NativeInputProxy.ComboState[2], Is.EqualTo(3));
        }

        [Test]
        public void DataOrientedDeadType0SecondPass_ClearsAllDerivedInputState()
        {
            var world = CreateDataOrientedWorld();
            LF2Character character = RegisterCharacter(world, 0, 808);
            NTSDEntityRuntime runtime = character.Runtime;
            runtime.HP = 0;
            runtime.KeyJump = 1;
            runtime.PrevJump = 1;
            runtime.CdAttack = 4;
            runtime.NativeInputProxy.EdgeWindow[0] = 4;
            runtime.NativeInputProxy.Current[4] = 1;
            runtime.NativeInputProxy.Previous[4] = 1;
            runtime.NativeInputProxy.ComboState[2] = 3;
            runtime.NativeInputProxy.ProxyTail = 0x5A;
            runtime.InputHistory[4] = 9;
            runtime.InputHistory[5] = 5;

            world.CharacterInputAll(2);

            Assert.That(runtime.KeyJump, Is.Zero);
            Assert.That(runtime.PrevJump, Is.Zero);
            Assert.That(runtime.CdAttack, Is.Zero);
            Assert.That(runtime.NativeInputProxy.ProxyTail, Is.Zero);
            Assert.That(runtime.NativeInputProxy.EdgeWindow, Is.All.Zero);
            Assert.That(runtime.NativeInputProxy.Current, Is.All.Zero);
            Assert.That(runtime.NativeInputProxy.Previous, Is.All.Zero);
            Assert.That(runtime.NativeInputProxy.ComboState, Is.All.Zero);
            AssertNativeHistoryInitial(runtime);
        }

        [Test]
        public void WarmDataOrientedHumanTwoPass_AllocatesZeroManagedBytes()
        {
            var world = CreateDataOrientedWorld();
            LF2Character character = RegisterCharacter(world, 0, 807);
            for (int tick = 2; tick < 18; tick++)
            {
                character.RunHumanInputPollPhase(tick);
                world.CharacterInputAll(tick);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 18; tick < 146; tick++)
            {
                character.RunHumanInputPollPhase(tick);
                world.CharacterInputAll(tick);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static SimulationWorld CreateDataOrientedWorld()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.Runtime.Flow.InputPhase = 0;
            return world;
        }

        private static void AssertNativeHistoryInitial(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.InputHistory[0], Is.Zero);
            for (int index = 1; index < runtime.InputHistory.Length; index++)
                Assert.That(runtime.InputHistory[index], Is.EqualTo(-1), $"history[{index}]");
        }

        private static AiDecisionSnapshot CreateAiSnapshot()
        {
            const ulong epoch = 19UL;
            var snapshot = new AiDecisionSnapshot(2);
            snapshot.Reset(epoch);
            SetAiRow(snapshot.Rows, 0, 901, 1, 0, 0);
            SetAiRow(snapshot.Rows, 1, 902, 2, 120, 0);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 901;
            snapshot.OccupancyEpoch = epoch;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1001;
            snapshot.Input.Unk400 = -1001;
            snapshot.Input.History1 = -1;
            snapshot.Input.History2 = -1;
            snapshot.Input.History3 = -1;
            snapshot.Input.History4 = -1;
            snapshot.Input.History5 = -1;
            snapshot.World.Difficulty = 1;
            snapshot.World.BattleMode = 0;
            snapshot.World.InputPhase = 0;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = -200;
            snapshot.World.StageZMax = 200;
            return snapshot;
        }

        private static void SetAiRow(
            AiSensingSnapshot rows,
            int slot,
            int stableId,
            int team,
            int x,
            int facing)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = 1;
            rows.Identity[slot] = stableId;
            rows.ObjectId[slot] = slot + 1;
            rows.DataObjectType[slot] = 0;
            rows.State[slot] = 2;
            rows.Frame[slot] = 0;
            rows.X[slot] = x;
            rows.Y[slot] = 0;
            rows.Z[slot] = 0;
            rows.Facing[slot] = facing;
            rows.Hp[slot] = 500;
            rows.Hp3[slot] = 500;
            rows.HpMax[slot] = 500;
            rows.Pp[slot] = 300;
            rows.Team[slot] = team;
            rows.CachedTargetSlot[slot] = -1;
            rows.CoordinateTargetX[slot] = -1001;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int objectId)
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
                name = $"NTSD28NativeProducer_{slot}_{objectId}",
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
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Controller = new EmptyController();
            character.AiControlled = false;
            world.Register(character);
            return character;
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
