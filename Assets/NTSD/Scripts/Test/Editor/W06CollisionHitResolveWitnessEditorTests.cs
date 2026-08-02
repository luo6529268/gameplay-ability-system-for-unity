#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Input;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class W06CollisionHitResolveWitnessEditorTests
    {
        [Test]
        [Category("NTSD_W06")]
        public void InteractionPhase_OrdersCharacterHitRandomDropThenObjectHit()
        {
            var world = new SimulationWorld();
            world.Rng.Seed(0x5EEDu);
            var observations = new List<string>();
            var characterProbe = CreatePhaseProbe(
                world,
                observations,
                "character",
                LF2ObjectType.Character,
                0,
                6001);
            var objectProbe = CreatePhaseProbe(
                world,
                observations,
                "object",
                LF2ObjectType.SpecialAttack,
                20,
                6002);

            InvokeInteractionPhase(world, 11);

            Assert.That(observations, Is.EqualTo(new[] { "character", "object" }));
            Assert.That(characterProbe.ObservedRngCalls, Is.EqualTo(0),
                "character hit consume must run before the natural random-drop RNG gate");
            Assert.That(objectProbe.ObservedRngCalls, Is.EqualTo(1),
                "object hit consume must observe the random-drop gate side effect");
            Assert.That(world.Rng.CallCount, Is.EqualTo(1));
        }

        [Test]
        [Category("NTSD_W06")]
        public void FrozenCandidates_ResolveAscendingWithoutGeometryOrTeamRefilter()
        {
            InteractionArea itr = new InteractionArea
            {
                kind = 0,
                x = -30,
                y = -10,
                w = 60,
                h = 20,
                zwidth = 15,
                injury = 10,
                dvx = 1,
                arest = 4,
                vrest = 1,
            };
            BodyBox body = new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            };
            var hitOrder = new List<int>();
            var hitRngCalls = new List<ulong>();
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "W06_Attacker",
                6100,
                MakeFrame(itr, null));
            RecordingVictim first = CreateVictim(
                "W06_First",
                6101,
                MakeFrame(null, body),
                hitOrder,
                hitRngCalls,
                world);
            RecordingVictim second = CreateVictim(
                "W06_Second",
                6102,
                MakeFrame(null, body),
                hitOrder,
                hitRngCalls,
                world);
            Register(world, attacker, 0, 1, 0);
            Register(world, first, 1, 2, -5);
            Register(world, second, 2, 2, 5);
            world.Rng.Seed(0x606u);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                ((BruteForceSceneQuery)world.SceneQuery).TryGetCollisionCandidateSequence(
                    attacker,
                    out List<SceneQueryHit> candidates),
                Is.True);
            Assert.That(candidates.Count, Is.EqualTo(2));
            Assert.That(candidates[0].TargetSlot, Is.EqualTo(1));
            Assert.That(candidates[1].TargetSlot, Is.EqualTo(2));

            first.Runtime.SetPosition(10000, 0, 0);
            first.Runtime.SyncIntegerPosition();
            first.RelationTeam = attacker.RelationTeam;
            int firstHp = first.Health.HP;
            int secondHp = second.Health.HP;

            world.PostInteractionTickAll(12);
            world.EndCollisionCandidateConsumption();

            Assert.That(hitOrder, Is.EqualTo(new[] { 1, 2 }),
                "hit resolve must consume the frozen candidate carrier in ascending slot order");
            Assert.That(first.Health.HP, Is.LessThan(firstHp),
                "post-collection geometry/team changes must not re-filter the frozen candidate");
            Assert.That(second.Health.HP, Is.LessThan(secondHp));
            Assert.That(hitRngCalls.Count, Is.EqualTo(2));
            Assert.That(hitRngCalls[1], Is.EqualTo(hitRngCalls[0] + 2),
                "the later candidate must observe the earlier hit-record Z/X RNG calls");
            Assert.That(world.Rng.CallCount, Is.EqualTo(hitRngCalls[1] + 2),
                "each accepted kind-0 hit must consume its own Z/X RNG pair in candidate order");
        }

        private static PhaseProbeCharacter CreatePhaseProbe(
            SimulationWorld world,
            List<string> observations,
            string label,
            LF2ObjectType objectType,
            int slot,
            int objectId)
        {
            var probe = new PhaseProbeCharacter(world, observations, label, objectType);
            Initialize(probe, label, objectId, MakeFrame(null, null));
            Register(world, probe, slot, 1, 0);
            return probe;
        }

        private static LF2Character CreateCharacter(string name, int objectId, LF2FrameData frame)
        {
            var character = new WitnessCharacter();
            Initialize(character, name, objectId, frame);
            return character;
        }

        private static RecordingVictim CreateVictim(
            string name,
            int objectId,
            LF2FrameData frame,
            List<int> hitOrder,
            List<ulong> hitRngCalls,
            SimulationWorld world)
        {
            var victim = new RecordingVictim(hitOrder, hitRngCalls, world);
            Initialize(victim, name, objectId, frame);
            return victim;
        }

        private static void Initialize(
            LF2Character character,
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new NoopController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.Prev2 = 0;
            character.Frame.Prev2D = character.Frame.D;
            character.Runtime.PrevFrame2 = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
        }

        private static void Register(
            SimulationWorld world,
            LF2Entity entity,
            int slot,
            int team,
            int x)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(slot));
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.Health.HP3 = 100;
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.PrevFrame2 = 0;
        }

        private static LF2FrameData MakeFrame(InteractionArea itr, BodyBox body)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (itr != null)
                frame.itrs.Add(itr);
            if (body != null)
                frame.bodies.Add(body);
            return frame;
        }

        private static void InvokeInteractionPhase(SimulationWorld world, int tickIndex)
        {
            MethodInfo method = typeof(NTSDBattleTickSystem).GetMethod(
                "RunInteractionPhase",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(new NTSDBattleTickSystem(world), new object[] { tickIndex, null });
        }

        private class WitnessCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }

        private sealed class RecordingVictim : WitnessCharacter
        {
            private readonly List<int> hitOrder;
            private readonly List<ulong> hitRngCalls;
            private readonly SimulationWorld world;

            internal RecordingVictim(
                List<int> hitOrder,
                List<ulong> hitRngCalls,
                SimulationWorld world)
            {
                this.hitOrder = hitOrder;
                this.hitRngCalls = hitRngCalls;
                this.world = world;
            }

            public override bool Hit(
                InteractionArea itr,
                LF2Entity attacker,
                Vector3 attackerPos,
                PhysicsState.BattleVolume volume)
            {
                hitOrder.Add(Runtime.SlotIndex);
                hitRngCalls.Add(world.Rng.CallCount);
                return base.Hit(itr, attacker, attackerPos, volume);
            }
        }

        private sealed class PhaseProbeCharacter : LF2Character
        {
            private readonly SimulationWorld world;
            private readonly List<string> observations;
            private readonly string label;
            private readonly LF2ObjectType objectType;

            internal PhaseProbeCharacter(
                SimulationWorld world,
                List<string> observations,
                string label,
                LF2ObjectType objectType)
            {
                this.world = world;
                this.observations = observations;
                this.label = label;
                this.objectType = objectType;
            }

            internal ulong ObservedRngCalls { get; private set; }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)objectType;

            public override void SimPostInteraction(int tickIndex)
            {
                observations.Add(label);
                ObservedRngCalls = world.Rng.CallCount;
            }

            public override void SimObjectInteraction(int tickIndex)
            {
                observations.Add(label);
                ObservedRngCalls = world.Rng.CallCount;
            }
        }

        private sealed class NoopController : ILF2Controller
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
            public void SetInputID(int inputId) { }
        }
    }
}
#endif
