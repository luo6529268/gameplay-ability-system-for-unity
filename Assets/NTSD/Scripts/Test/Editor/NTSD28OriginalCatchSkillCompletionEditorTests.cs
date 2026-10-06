#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28OriginalCatchSkillCompletionEditorTests
    {
        private const string OutputRoot = "artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/";

        [TestCase(false)]
        [TestCase(true)]
        public void FullDriverCloneHitConsumesHeldSkill(bool projected)
        {
            var rows = new List<object>();
            bool caught = false;
            bool released = false;
            bool removed = false;
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                "Assets/NTSD/Content/LoganRuntime", OutputRoot + "held-attack-clone-unity-scenario.json",
                BattleRuntimeProfile.Authority400, 64, (driver, unused, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    world.Runtime.Flow.FrameToggle = 0;
                    world.Runtime.Flow.InputPhase = 0;
                    var entities = new List<LF2Entity>();
                    if (projected)
                    {
                        world.ConfigureFixedViewRunDistance(2048, 1152);
                        world.GetAllEntities(entities);
                        foreach (LF2Entity entity in entities)
                        {
                            entity.Runtime.Z = 450;
                            entity.Runtime.SetSourceRulePosition(entity.Runtime.X, entity.Runtime.Z);
                            entity.Runtime.X = world.SpatialProjection.SourceToViewX(entity.Runtime.X);
                            entity.Runtime.Z = world.SpatialProjection.SourceToViewZ(entity.Runtime.Z);
                            entity.Runtime.SyncIntegerPosition();
                            entity.Runtime.SyncSourceRuleIntegerPosition();
                        }
                    }
                    for (int tick = 1; tick <= 64; tick++)
                    {
                        SimulationInputButtons buttons = tick == 29 || tick == 30
                            ? SimulationInputButtons.Jump : SimulationInputButtons.None;
                        Assert.That(driver.StepOneTick(new FrameInputSet(tick, new[]
                        {
                            new SimulationPlayerInput(0, buttons),
                            new SimulationPlayerInput(1, SimulationInputButtons.None)
                        }), ignorePaused: true, buildPresentation: false), Is.True);
                        world.GetAllEntities(entities);
                        LF2Entity attacker = entities.FirstOrDefault(value => value.Runtime.SlotIndex == 0);
                        LF2Entity ball = entities.FirstOrDefault(value => value.ObjectId == 434);
                        caught |= attacker?.Runtime.CaughtSlotIndex == 1;
                        released |= (ball != null && ball.Runtime.LinkState >= 0) ||
                            (caught && ball == null && attacker.Runtime.LinkState == 0);
                        removed |= caught && ball == null;
                        rows.Add(new { tick, action = attacker?.Frame.N, heldAction = ball?.Frame.N,
                            link = ball?.Runtime.LinkState, exists = ball != null });
                    }
                }, useProjectMode: true);
            string output = OutputRoot + "original-common-fix-01/clone-completion-" + projected + "-" +
                System.Guid.NewGuid().ToString("N") + ".json";
            using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(rows, Formatting.Indented));
            Assert.That(caught, Is.True);
            Assert.That(released, Is.True);
            Assert.That(removed, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FullDriverBodyCatchKeepsVerifiedAuthoredBehavior(bool projected)
        {
            new NTSD28RasenganCommonRegressionEditorTests()
                .HeldRasenganAttackAndTerminationMatchesSelectedReference("held-attack-body", projected);
        }

        [TestCase(30)]
        [TestCase(47)]
        public void LostVictimCatchUsesAuthoredHeldSkillCompletion(int completion)
        {
            Fixture fixture = Create(completion);
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.EqualTo(10));
            Assert.That(fixture.Held.Runtime.LinkState, Is.LessThan(0));
            fixture.Attacker.DirectWriteNativeRawFramePreserveWaitCounter(completion);
            fixture.World.HeldObjectProcessAll(1);
            Assert.That(fixture.Held.Frame.N, Is.InRange(0, 5));
            Assert.That(fixture.Held.Frame.D.next, Is.EqualTo(1000));
            Assert.That(fixture.Held.Runtime.LinkState, Is.Zero);
        }

        [Test]
        public void VanishedVictimUsesTheSameAuthoredCompletion()
        {
            Fixture fixture = Create(30);
            fixture.Attacker.Runtime.CaughtSlotIndex = 70;
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.EqualTo(10));
        }

        [Test]
        public void ClearedCaughtTargetStillUsesAuthoredCompletion()
        {
            Fixture fixture = Create(30);
            fixture.Attacker.Runtime.CaughtSlotIndex = -1;
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.EqualTo(10));
            Assert.That(fixture.Held.Runtime.LinkState, Is.LessThan(0));
        }

        [Test]
        public void ValidVictimCatchKeepsHeldSkill()
        {
            Fixture fixture = Create(30);
            fixture.Victim.Frame.D.cpoint = new CatchPoint { kind = 2 };
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.EqualTo(10));
            Assert.That(fixture.Held.Runtime.LinkState, Is.LessThan(0));
        }

        [Test]
        public void OrdinaryHeldWeaponKeepsNativeAbort()
        {
            Fixture fixture = Create(30);
            fixture.Held.DataObjectType = (int)LF2ObjectType.LightWeapon;
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.Zero);
            Assert.That(fixture.Held.Runtime.LinkState, Is.LessThan(0));
        }

        [Test]
        public void CyclicGrabWithoutConsumingExitKeepsNativeAbort()
        {
            Fixture fixture = Create(30);
            fixture.Attacker.Frame.D.next = 10;
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.Zero);
        }

        [Test]
        public void StaleVictimOrHeldReciprocalDoesNotConsumeSkill()
        {
            Fixture fixture = Create(30);
            fixture.Victim.Runtime.CatchSourceSlot90 = 99;
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.Zero);
            fixture = Create(30);
            fixture.Held.Runtime.HolderStableId = 99;
            fixture.World.CpointWriter.RunKind1(fixture.World, fixture.Attacker);
            Assert.That(fixture.Attacker.Frame.N, Is.Zero);
        }

        private static Fixture Create(int completion)
        {
            var world = new SimulationWorld(BattleRuntimeProfile.Authority400, 400);
            var catchFrame = new LF2FrameData
            {
                frameId = 10, state = LF2States.Catching, wait = 0, next = 11,
                cpoint = new CatchPoint { kind = 1, vaction = 20 },
                wpoints = new List<WeaponPoint> { new WeaponPoint { kind = 1, weaponact = 35 } }
            };
            var middle = new LF2FrameData
            {
                frameId = 11, state = LF2States.Catching, wait = 0, next = completion,
                cpoint = new CatchPoint { kind = 1 }
            };
            var exit = new LF2FrameData
            {
                frameId = completion, state = 3, wait = 2, next = 999,
                wpoints = new List<WeaponPoint> { new WeaponPoint { kind = 3, weaponact = 80 } }
            };
            TestEntity attacker = Entity(world, 0, 0, new[] { catchFrame, middle, exit }, 10);
            TestEntity victim = Entity(world, 1, 0, new[] { new LF2FrameData { frameId = 20, state = 15 } }, 20);
            TestEntity held = Entity(world, 2, 3, new[]
            {
                new LF2FrameData { frameId = 35, state = 1001 },
                new LF2FrameData { frameId = 80, state = 15, next = 1000 }
            }.Concat(Enumerable.Range(0, 6).Select(value =>
                new LF2FrameData { frameId = value, state = 3005, next = 1000 })).ToArray(), 35);
            attacker.Runtime.CaughtSlotIndex = 1;
            victim.Runtime.CatchSourceSlot90 = 0;
            attacker.Runtime.TargetSlotIndex = 2;
            held.Runtime.HolderStableId = 0;
            held.Runtime.LinkState = -1;
            return new Fixture(world, attacker, victim, held);
        }

        private static TestEntity Entity(SimulationWorld world, int slot, int type,
            LF2FrameData[] frames, int action)
        {
            var entity = new TestEntity { DataObjectType = type };
            entity.ModuleInitialize();
            entity.ObjectId = 900 + slot;
            var data = new LF2CharacterData { name = "CommonCatch" + slot, frames = new List<LF2FrameData>(frames) };
            if (!data.frames.Any(value => value.frameId == 0))
                data.frames.Add(new LF2FrameData { frameId = 0 });
            entity.FrameCache.Load(new LF2CharacterDataWrapper(entity.ObjectId, data));
            LF2FrameData frame = entity.FrameCache.GetNativeFrameDataById(action);
            entity.Frame.D = frame;
            entity.Frame.N = entity.Frame.PN = entity.Frame.Prev = entity.Frame.Prev2 = action;
            entity.Frame.Prev2D = frame;
            entity.Runtime.PrevFrame2 = action;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private sealed class TestEntity : LF2Character
        {
            internal int DataObjectType;
            public override int GetCurrentDataObjectTypeForSimulation() => DataObjectType;
        }

        private sealed class Fixture
        {
            internal Fixture(SimulationWorld world, TestEntity attacker, TestEntity victim, TestEntity held)
            {
                World = world;
                Attacker = attacker;
                Victim = victim;
                Held = held;
            }
            internal SimulationWorld World { get; }
            internal TestEntity Attacker { get; }
            internal TestEntity Victim { get; }
            internal TestEntity Held { get; }
        }
    }
}
#endif
