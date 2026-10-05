#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28BattleAudioAlignmentEditorTests
    {
        [TestCase(20, false, 0, "SFX_001")]
        [TestCase(80, false, 0, "SFX_006")]
        [TestCase(20, true, 0, "SFX_001")]
        [TestCase(20, false, 1, "SFX_001,SFX_032,SFX_001")]
        [TestCase(80, false, 1, "SFX_006,SFX_033,SFX_006")]
        public void OrdinaryHurtUsesReactionTimerAndVictimPosition(
            int timer, bool knockback, int effect, string expected)
        {
            using var fixture = new Fixture(0, 0);
            fixture.Target.Runtime.Fall = timer;
            LF2HitResolveRuntimeData.RecordStandardHurtSounds(
                fixture.Attacker, fixture.Target, new InteractionArea { effect = effect }, knockback);
            AssertSounds(fixture.World, expected.Split(','), 700);
        }

        [Test]
        public void OrdinarySpecialAttackBrokenCueKeepsAttackerPositionBeforeVictimBase()
        {
            using var fixture = new Fixture(3, 0);
            fixture.AttackerData.weapon_broken_sound = @"data\020.wav";
            LF2HitResolveRuntimeData.RecordStandardHurtSounds(
                fixture.Attacker, fixture.Target, new InteractionArea(), false);
            Assert.That(fixture.World.PendingSounds, Has.Count.EqualTo(2));
            Assert.That(fixture.World.PendingSounds[0].Cue, Is.EqualTo(@"data\020.wav"));
            Assert.That(fixture.World.PendingSounds[0].WorldX, Is.EqualTo(300));
            Assert.That(fixture.World.PendingSounds[1].Cue, Is.EqualTo("SFX_001"));
            Assert.That(fixture.World.PendingSounds[1].WorldX, Is.EqualTo(700));
        }

        [TestCase(2, "SFX_001,SFX_068")]
        [TestCase(3, "SFX_001,SFX_065")]
        [TestCase(20, "SFX_001,SFX_068")]
        [TestCase(21, "SFX_001,SFX_068")]
        [TestCase(22, "SFX_001,SFX_068")]
        [TestCase(23, "SFX_001,SFX_068")]
        [TestCase(30, "SFX_001,SFX_065")]
        public void ActualOrdinaryDamageQueuesPostEffectAfterBase(int effect, string expected)
        {
            using var fixture = new Fixture(0, 0);
            var itr = new InteractionArea { kind = 0, injury = 5, fall = 1, effect = effect, dvx = 1 };
            Assert.That(fixture.World.DamageWriter.ApplyStandardCharacterDamage(
                fixture.World, fixture.Attacker, fixture.Target, fixture.Target.HitCounters, itr), Is.True);
            AssertSounds(fixture.World, expected.Split(','), 700);
        }

        [TestCase(BattleHitExecutionPlanMode.ShadowCompare, 2, "SFX_068")]
        [TestCase(BattleHitExecutionPlanMode.ShadowCompare, 3, "SFX_065")]
        [TestCase(BattleHitExecutionPlanMode.ShadowCompare, 23, "SFX_068")]
        [TestCase(BattleHitExecutionPlanMode.DataOriented, 2, "SFX_068")]
        [TestCase(BattleHitExecutionPlanMode.DataOriented, 3, "SFX_065")]
        [TestCase(BattleHitExecutionPlanMode.DataOriented, 23, "SFX_068")]
        public void CapturedPostHitSoundProjectionMatchesWriter(
            BattleHitExecutionPlanMode mode, int effect, string postCue)
        {
            using var fixture = new Fixture(0, 0);
            fixture.Attacker.RelationTeam = 1;
            fixture.Target.RelationTeam = 2;
            fixture.Attacker.Frame.D.itrs.Add(new InteractionArea
            {
                kind = 0, x = 0, y = 0, w = 450, h = 100,
                injury = 5, fall = 1, dvx = 1, effect = effect,
            });
            fixture.Target.Frame.D.bodies.Add(new BattleBodyBoxValue(0, 0, 30, 100));
            fixture.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
            fixture.World.CaptureCollisionFrameSnapshotsAll();
            fixture.World.CollectCollisionCandidatesAll();
            fixture.World.PostInteractionTickAll(1);
            Assert.That(fixture.World.BattleHitExecutionPlanDiagnosticsForDiagnostics.FailureCount, Is.Zero);
            Assert.That(fixture.World.BattleHitExecutionPlanDiagnosticsForDiagnostics.ObservationMismatchCount, Is.Zero);
            AssertSounds(fixture.World, new[] { "SFX_001", postCue }, 700);
        }

        [Test]
        public void ActualOid100MultiplierCuePrecedesVictimBase()
        {
            using var fixture = new Fixture(0, 0);
            fixture.Target.ObjectId = 100;
            fixture.Target.Runtime.LinkState = -1;
            var itr = new InteractionArea { kind = 0, injury = 5, fall = 1, dvx = 1 };
            Assert.That(fixture.World.DamageWriter.ApplyStandardCharacterDamage(
                fixture.World, fixture.Attacker, fixture.Target, fixture.Target.HitCounters, itr), Is.True);
            AssertSounds(fixture.World, new[] { "SFX_039", "SFX_001" }, 700);
        }

        [TestCase(0, 0, "hit.wav", "drop.wav", "hit.wav")]
        [TestCase(0, 7, "hit.wav", "drop.wav", "drop.wav")]
        [TestCase(0, 70, "hit.wav", "drop.wav", "drop.wav")]
        [TestCase(0, 75, "hit.wav", "drop.wav", "drop.wav")]
        [TestCase(0, 7, "hit.wav", "", "SFX_002")]
        [TestCase(0, 0, "", "drop.wav", "SFX_002")]
        [TestCase(3, 7, "hit.wav", "drop.wav", "broken.wav")]
        [TestCase(3, 0, "hit.wav", "drop.wav", null)]
        public void ReducedHurtUsesPreHitDatCueAtVictimPosition(
            int type, int state, string hit, string drop, string expected)
        {
            using var fixture = new Fixture(type, state);
            fixture.TargetData.weapon_hit_sound = hit;
            fixture.TargetData.weapon_drop_sound = drop;
            fixture.AttackerData.weapon_broken_sound = expected == "broken.wav" ? expected : "";
            MethodInfo lead = typeof(NTSD.Simulation.Ecs.BattleDamageWriter).GetMethod(
                "RecordAlternateLeadSound", BindingFlags.Static | BindingFlags.NonPublic);
            lead.Invoke(null, new object[] { fixture.Attacker, fixture.Target });
            AssertSounds(fixture.World, expected == null ? Array.Empty<string>() : new[] { expected }, 700);
        }

        [TestCase(2, 9.0, false)]
        [TestCase(2, 9.01, true)]
        [TestCase(4, 8.5, false)]
        [TestCase(4, 8.51, true)]
        [TestCase(6, 8.51, true)]
        [TestCase(1, 10.0, false)]
        public void NonCharacterBounceUsesFixedChannelFourAndPreservesThreshold(
            int type, double vy, bool sounds)
        {
            using var fixture = new Fixture(type, 0);
            fixture.AttackerData.weapon_drop_sound = "incorrect-dat-drop.wav";
            var frame = new LF2FrameData { frameId = 0, state = 1002 };
            MethodInfo landing = typeof(LF2Entity).GetMethod(
                "ApplyCurrentDatNonCharacterLanding", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(int), typeof(LF2FrameData), typeof(double), typeof(bool) }, null);
            landing.Invoke(fixture.Attacker, new object[] { type, frame, vy, true });
            AssertSounds(fixture.World, sounds ? new[] { "SFX_011" } : Array.Empty<string>(), 300);
        }

        [TestCase(9, 2, true, "TryRunSharedCharacterDatRunningInputPhase", 213, 23.0)]
        [TestCase(215, 0, true, "TryRunSharedCharacterDatCrouchInputPhase", 213, 23.0)]
        [TestCase(215, 0, false, "TryRunSharedCharacterDatCrouchInputPhase", 214, -23.0)]
        public void DashInputKeepsDatFrameSoundWithoutExtraChannelSeven(
            int action, int state, bool right, string inputPhase, int expectedAction, double expectedVx)
        {
            using var fixture = new Fixture(0, 0);
            fixture.AttackerData.dash_distance = 23;
            fixture.AttackerData.dash_height = -6.875f;
            foreach (LF2FrameData frame in fixture.AttackerData.frames)
            {
                frame.UsesLoganFrameNumbers = true;
                if (frame.frameId == expectedAction)
                    frame.SealFrameSounds(new[] { "c/1naruto/w/a7.wav" });
            }
            fixture.Attacker.Frame.N = action;
            fixture.Attacker.Frame.D = fixture.Attacker.FrameCache.GetNativeFrameDataById(action);
            fixture.Attacker.Frame.D.state = state;
            fixture.Attacker.Runtime.Dir = "right";
            fixture.Attacker.Runtime.KeyRight = right ? 1 : 0;
            fixture.Attacker.Runtime.KeyLeft = right ? 0 : 1;
            fixture.Attacker.Runtime.KeyDefend = 1;
            fixture.Attacker.Runtime.CdJump = 1;
            MethodInfo input = typeof(LF2Entity).GetMethod(
                inputPhase, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(input.Invoke(fixture.Attacker, null), Is.EqualTo(true));
            Assert.That(fixture.Attacker.Frame.N, Is.EqualTo(expectedAction));
            Assert.That(fixture.Attacker.Runtime.Vx, Is.EqualTo(expectedVx));
            Assert.That(fixture.Attacker.Runtime.Vy, Is.EqualTo(-6.875));
            AssertSounds(fixture.World, Array.Empty<string>(), 300);
            MethodInfo frameAudio = typeof(LF2Entity).GetMethod(
                "QueueNativeC25FrameSounds", BindingFlags.Instance | BindingFlags.NonPublic);
            frameAudio.Invoke(fixture.Attacker, null);
            AssertSounds(fixture.World, new[] { "c/1naruto/w/a7.wav" }, 300);
        }

        private static void AssertSounds(SimulationWorld world, string[] expected, int x)
        {
            Assert.That(world.PendingSounds, Has.Count.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(world.PendingSounds[index].Cue, Is.EqualTo(expected[index]), "event " + index);
                Assert.That(world.PendingSounds[index].WorldX, Is.EqualTo(x), "event " + index);
            }
        }

        private sealed class Fixture : IDisposable
        {
            public readonly SimulationWorld World = new SimulationWorld();
            public readonly LF2CharacterData AttackerData = new LF2CharacterData();
            public readonly LF2CharacterData TargetData = new LF2CharacterData();
            public readonly LF2Character Attacker;
            public readonly LF2Character Target;

            public Fixture(int type, int targetState)
            {
                Attacker = Create(1, 1, type, 0, 300, AttackerData);
                Target = Create(2, 2, 0, targetState, 700, TargetData);
            }

            private LF2Character Create(int oid, int slot, int type, int state, int x, LF2CharacterData data)
            {
                var frame = new LF2FrameData { frameId = 0, state = state };
                data.type_sub = type;
                data.frames = new List<LF2FrameData> { frame };
                foreach (int action in new[] { 7, 9, 20, 60, 70, 180, 186, 200, 203, 213, 214, 215, 216, 220, 226 })
                    data.frames.Add(new LF2FrameData { frameId = action });
                var entity = new AudioEntity { ObjectId = oid, DataType = type };
                entity.ModuleInitialize();
                entity.FrameCache.Load(new LF2CharacterDataWrapper(oid, data));
                entity.Frame.N = 0;
                entity.Frame.D = frame;
                entity.Initialize(500, 500);
                entity.Runtime.SetPosition(x, 0, 100);
                entity.Runtime.SyncIntegerPosition();
                entity.SetRequiredRuntimeSlot(slot);
                World.Register(entity);
                return entity;
            }

            public void Dispose()
            {
                World.BeginBattleShutdown();
                Assert.That(World.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        private sealed class AudioEntity : LF2Character
        {
            public int DataType;
            public override int GetCurrentDataObjectTypeForSimulation() => DataType;
        }
    }
}
#endif
