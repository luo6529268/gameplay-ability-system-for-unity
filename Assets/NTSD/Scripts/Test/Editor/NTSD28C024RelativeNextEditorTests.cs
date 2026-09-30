#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28C024RelativeNextEditorTests
    {
        private const string StagedRuntime =
            "Assets/NTSD/Content/LoganRuntime";

        [Test]
        public void CurrentSasukeObjectSelectsRelativeActionWithOneSynchronizedRoll()
        {
            var catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(StagedRuntime),
                ProjectBattleModeConfig.LoadDefault().Capture());
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(
                    catalog);
            Assert.That(configs.TryGetValue(315, out LF2CharacterDataWrapper data),
                Is.True, "current indexed formal OID315 must be staged");
            LF2FrameData source = data.characterData.frames.Find(
                frame => frame.frameId == 300);
            Assert.That(source, Is.Not.Null);
            Assert.That(source.state, Is.EqualTo(15));
            Assert.That(source.wait, Is.EqualTo(1));
            Assert.That(source.next, Is.EqualTo(1320));
            Assert.That(data.characterData.frames.Exists(
                frame => frame.frameId == 313), Is.True);

            TypedCharacter entity = CreateEntity(data, 315, 300, 1);
            var world = new SimulationWorld();
            world.Register(entity);
            try
            {
                SetFirstSynchronizedRoll(world, 11);
                RunNativeFrameBody(entity);
                Assert.That(entity.Frame.N, Is.EqualTo(313));
                Assert.That(entity.Trans.WaitCounter, Is.EqualTo(313));
                Assert.That(entity.AttackingCounter, Is.Zero);
                var rng = world.NativeRandom.CaptureScalarState();
                Assert.That(rng.SynchronizedCalls, Is.EqualTo(1UL));
                Assert.That(rng.LastSynchronizedCallSite,
                    Is.EqualTo(0x00452390u));
            }
            finally
            {
                world.Unregister(entity);
            }
        }

        [Test]
        public void CurrentSasukeObjectFullTickSelectsRelativeAction()
        {
            var catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(StagedRuntime),
                ProjectBattleModeConfig.LoadDefault().Capture());
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(
                    catalog);
            Assert.That(configs.TryGetValue(315, out LF2CharacterDataWrapper data),
                Is.True);
            TypedCharacter entity = CreateEntity(data, 315, 300, 1);
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.Register(entity);
            try
            {
                SetFirstSynchronizedRoll(world, 11);
                var input = new FrameInputSet(1,
                    Array.Empty<SimulationPlayerInput>());
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                Assert.That(entity.Frame.N, Is.EqualTo(313));
                Assert.That(entity.Trans.WaitCounter, Is.EqualTo(313));
                Assert.That(entity.AttackingCounter, Is.Zero);
                var rng = world.NativeRandom.CaptureScalarState();
                Assert.That(rng.SynchronizedCalls, Is.EqualTo(1UL));
                Assert.That(rng.LastSynchronizedCallSite,
                    Is.EqualTo(0x00452390u));
            }
            finally
            {
                world.Unregister(entity);
            }
        }

        [Test]
        public void CurrentSasukeObjectHeldWaitDoesNotRollOrLeaveAction()
        {
            var catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(StagedRuntime),
                ProjectBattleModeConfig.LoadDefault().Capture());
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(
                    catalog);
            Assert.That(configs.TryGetValue(315, out LF2CharacterDataWrapper data),
                Is.True);
            TypedCharacter entity = CreateEntity(data, 315, 300, 0);
            var world = new SimulationWorld();
            world.Register(entity);
            try
            {
                RunNativeFrameBody(entity);
                Assert.That(entity.Frame.N, Is.EqualTo(300));
                Assert.That(entity.AttackingCounter, Is.EqualTo(1));
                Assert.That(world.NativeRandom.CaptureScalarState()
                    .SynchronizedCalls, Is.Zero);
            }
            finally
            {
                world.Unregister(entity);
            }
        }

        [Test]
        public void ZeroSpanSelectsNextActionWithoutConsumingRng()
        {
            LF2CharacterDataWrapper data = SyntheticData(1300, 15);
            TypedCharacter entity = CreateEntity(data, 7024, 300, 0);
            var world = new SimulationWorld();
            world.Register(entity);
            try
            {
                RunNativeFrameBody(entity);
                Assert.That(entity.Frame.N, Is.EqualTo(301));
                Assert.That(entity.Trans.WaitCounter, Is.EqualTo(301));
                Assert.That(world.NativeRandom.CaptureScalarState()
                    .SynchronizedCalls, Is.Zero);
            }
            finally
            {
                world.Unregister(entity);
            }
        }

        [Test]
        public void StateEightyRetainsTerminalCodeHandling()
        {
            LF2CharacterDataWrapper data = SyntheticData(1320, 80);
            TypedCharacter entity = CreateEntity(data, 7025, 300, 0);
            var world = new SimulationWorld();
            world.Register(entity);
            try
            {
                RunNativeFrameBody(entity);
                Assert.That(entity.Frame.N, Is.EqualTo(1320));
                Assert.That(entity.Runtime.NativeLifecycleResolutionPending,
                    Is.True);
                Assert.That(world.NativeRandom.CaptureScalarState()
                    .SynchronizedCalls, Is.Zero);
            }
            finally
            {
                world.Unregister(entity);
            }
        }

        private static LF2CharacterDataWrapper SyntheticData(int next, int state)
        {
            var source = new LF2FrameData
            {
                frameId = 300,
                state = state,
                wait = 0,
                next = next,
            };
            var destination = new LF2FrameData
            {
                frameId = 301,
                state = 15,
                wait = 1,
                next = 0,
            };
            return new LF2CharacterDataWrapper(7024, new LF2CharacterData
            {
                name = "C024RelativeNext",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData> { source, destination },
            });
        }

        private static TypedCharacter CreateEntity(
            LF2CharacterDataWrapper data,
            int objectId,
            int action,
            int counter)
        {
            var entity = new TypedCharacter();
            entity.ModuleInitialize();
            entity.Name = data.characterData.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(data);
            entity.WriteCurrentFrameId(action);
            entity.Frame.D = entity.FrameCache.GetFrameDataById(action);
            entity.Frame.PN = action;
            entity.Initialize(500, 500);
            entity.Trans.SyncDirectFrameData(
                entity.Frame.D.wait, entity.Frame.D.next, action);
            entity.SetRequiredRuntimeSlot(50);
            entity.Team = 1;
            entity.RelationTeam = 1;
            entity.Runtime.ObjType = (int)LF2ObjectType.SpecialAttack;
            entity.Runtime.HP = 500;
            entity.Runtime.HP3 = 500;
            entity.Runtime.HPBound = 500;
            entity.Controller = new EmptyController();
            entity.AiControlled = false;
            entity.AttackingCounter = counter;
            return entity;
        }

        private static void SetFirstSynchronizedRoll(
            SimulationWorld world, byte tableValue)
        {
            NTSD28SynchronizedRandomState state =
                world.NativeRandom.CaptureSynchronizedState();
            state.Counter = 0;
            state.Index = 0;
            state.Calls = 0;
            state.Table[1] = tableValue;
            world.NativeRandom.RestoreSynchronized(state);
        }

        private static void RunNativeFrameBody(LF2Entity entity)
        {
            entity.BeginNativeC25FrameTickForWorldPass();
            try
            {
                entity.RunNativeC25FrameBodyForWorldPass();
            }
            finally
            {
                entity.EndNativeC25FrameTickForWorldPass();
            }
        }

        private sealed class TypedCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.SpecialAttack;
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } =
                new SimInputBuffer();
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
