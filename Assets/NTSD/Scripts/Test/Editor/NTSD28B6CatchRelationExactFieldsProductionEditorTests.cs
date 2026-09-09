#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    [Category("NTSD28_B6_CatchRelation")]
    public sealed class NTSD28B6CatchRelationExactFieldsProductionEditorTests
    {
        [Test]
        public void Kind3Success_WritesExactReciprocalAndRespondTimeout()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            InteractionArea itr = CreateItr(10, 20, 1);

            bool applied = world.InteractionWriter.TryApplyGrab(attacker, target, itr, 3);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.CaughtSlotIndex, Is.EqualTo(511));
            Assert.That(target.Runtime.CatchSourceSlot90, Is.Zero);
            Assert.That(target.Runtime.CatcherSlotIndex, Is.Zero);
            Assert.That(attacker.Runtime.CaughtDuration, Is.EqualTo(1));
        }

        [Test]
        public void Kind3MissingCaughtFrame_ReturnsFalseWithoutAnyMutation()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            attacker.Runtime.SetVelocity(7.25, 8.5, 9.75);
            target.Runtime.SetVelocity(-3.25, -4.5, -5.75);
            attacker.Runtime.CaughtSlotIndex = 77;
            attacker.Runtime.CaughtDuration = 88;
            target.Runtime.CatchSourceSlot90 = 66;
            target.Runtime.CatcherSlotIndex = 55;
            target.Runtime.Fall = 44;
            PairState before = PairState.Capture(attacker, target, world);

            bool applied = world.InteractionWriter.TryApplyGrab(
                attacker,
                target,
                CreateItr(10, 99, 1),
                3);

            Assert.That(applied, Is.False);
            Assert.That(PairState.Capture(attacker, target, world), Is.EqualTo(before));
        }

        [Test]
        public void Kind3SignedActions_FlipOnlyEncodedSidesAndUseAbsoluteFrames()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            attacker.Runtime.SetPosition(100, 12, 0);
            target.Runtime.SetPosition(140, 15, 0);
            attacker.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();

            bool applied = world.InteractionWriter.TryApplyGrab(
                attacker,
                target,
                CreateItr(-10, -20, -1),
                3);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Frame.N, Is.EqualTo(10));
            Assert.That(target.Frame.N, Is.EqualTo(20));
            Assert.That(attacker.Dirh(), Is.LessThan(0));
            Assert.That(target.Dirh(), Is.GreaterThan(0));
            Assert.That(attacker.Runtime.CaughtDuration, Is.EqualTo(-1));
            Assert.That(target.Runtime.CatchSourceSlot90, Is.Zero);
        }

        [TestCase(0, 300)]
        [TestCase(1, 1)]
        [TestCase(300, 300)]
        [TestCase(-1, -1)]
        public void Kind3Respond_UsesZeroDefaultAndPreservesNonZero(
            int respond,
            int expected)
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);

            bool applied = world.InteractionWriter.TryApplyGrab(
                attacker,
                target,
                CreateItr(10, 20, respond),
                3);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.CaughtDuration, Is.EqualTo(expected));
        }

        [TestCase(-10, 20, -1, -1)]
        [TestCase(10, -20, 1, 1)]
        [TestCase(-10, -20, -1, 1)]
        public void Kind3SignedActions_FlipOnlyTheirOwnInitialFacing(
            int catching,
            int caught,
            int expectedAttackerDir,
            int expectedTargetDir)
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);

            bool applied = world.InteractionWriter.TryApplyGrab(
                attacker,
                target,
                CreateItr(catching, caught, 0),
                3);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Frame.N, Is.EqualTo(10));
            Assert.That(target.Frame.N, Is.EqualTo(20));
            Assert.That(attacker.Dirh(), Is.EqualTo(expectedAttackerDir));
            Assert.That(target.Dirh(), Is.EqualTo(expectedTargetDir));
        }

        [TestCase(99, 20)]
        [TestCase(10, 99)]
        [TestCase(99, 98)]
        public void Kind3MissingAnyRelationFrame_PreservesWholePair(
            int catching,
            int caught)
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            attacker.Runtime.CaughtSlotIndex = 77;
            attacker.Runtime.CaughtDuration = 88;
            target.Runtime.CatchSourceSlot90 = 66;
            target.Runtime.CatcherSlotIndex = 55;
            target.Runtime.Fall = 44;
            PairState before = PairState.Capture(attacker, target, world);

            bool applied = world.InteractionWriter.TryApplyGrab(
                attacker,
                target,
                CreateItr(catching, caught, 1),
                3);

            Assert.That(applied, Is.False);
            Assert.That(PairState.Capture(attacker, target, world), Is.EqualTo(before));
        }

        [Test]
        public void Kind3MissingActionArrays_DefaultToAuthoredFrameZero()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            var itr = new InteractionArea { kind = 3, respond = 0 };

            bool applied = world.InteractionWriter.TryApplyGrab(attacker, target, itr, 3);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Frame.N, Is.Zero);
            Assert.That(target.Frame.N, Is.Zero);
            Assert.That(target.Runtime.CatchSourceSlot90, Is.Zero);
        }

        [Test]
        public void CurrentCriminalFrame340_AppliesAuthored341And130ExactRelation()
        {
            string path = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Config",
                "chars",
                "criminal.dat");
            Lf2DatFile dat = new Lf2DatParserV2().Parse(File.ReadAllText(path), path);
            var frames = new List<LF2FrameData>(dat.Frames.Count);
            for (int index = 0; index < dat.Frames.Count; index++)
                frames.Add(Lf2DatConverter.ConvertToFrameData(dat.Frames[index]));
            LF2FrameData source = frames.Find(value => value.frameId == 340);
            Assert.That(source, Is.Not.Null);
            InteractionArea itr = source.itrs.Find(value => value.kind == 3);
            Assert.That(itr, Is.Not.Null);
            var data = new LF2CharacterData
            {
                name = "criminal",
                type_sub = 300,
                frames = frames,
            };
            var world = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 512);
            TestCharacter attacker = CreateEntityFromData(
                world,
                "B6CriminalAttacker",
                300,
                0,
                data,
                340);
            LF2FrameData target0 = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 1,
                next = 0,
                centerx = 39,
                centery = 79,
            };
            LF2FrameData target130 = new LF2FrameData
            {
                frameId = 130,
                state = LF2States.BeingCaught,
                wait = 3,
                next = 130,
                centerx = 35,
                centery = 70,
                cpoint = new CatchPoint { x = 5, y = 9, kind = 2 },
            };
            var targetData = new LF2CharacterData
            {
                name = "B6CriminalCharacterTarget",
                type_sub = 9601,
                frames = new List<LF2FrameData> { target0, target130 },
            };
            TestCharacter target = CreateEntityFromData(
                world,
                "B6CriminalTarget",
                9601,
                511,
                targetData,
                0);
            attacker.Runtime.SetPosition(100, 10, 0);
            target.Runtime.SetPosition(140, 20, 0);
            attacker.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();

            bool applied = world.InteractionWriter.TryApplyGrab(attacker, target, itr, 3);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Frame.N, Is.EqualTo(341));
            Assert.That(target.Frame.N, Is.EqualTo(130));
            Assert.That(attacker.Runtime.CaughtSlotIndex, Is.EqualTo(511));
            Assert.That(target.Runtime.CatchSourceSlot90, Is.Zero);
            Assert.That(target.Runtime.CatcherSlotIndex, Is.Zero);
            Assert.That(attacker.Runtime.CaughtDuration, Is.EqualTo(300));
        }

        [Test]
        public void Kind1CompatibilityPath_KeepsLegacyTimeoutAndDoesNotWriteExactSource()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            target.Runtime.CatchSourceSlot90 = 66;

            bool applied = world.InteractionWriter.TryApplyGrab(
                attacker,
                target,
                CreateItr(10, 20, 1),
                1);

            Assert.That(applied, Is.True);
            Assert.That(attacker.Frame.N, Is.EqualTo(10));
            Assert.That(target.Frame.N, Is.EqualTo(20));
            Assert.That(attacker.Runtime.CaughtDuration, Is.EqualTo(300));
            Assert.That(target.Runtime.CatchSourceSlot90, Is.EqualTo(66));
            Assert.That(target.Runtime.CatcherSlotIndex, Is.Zero);
        }

        [Test]
        public void Kind3ShadowCompare_ProjectsExactSourceAndAllWriterEffects()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            attacker.Team = 1;
            attacker.RelationTeam = 1;
            target.Team = 2;
            target.RelationTeam = 2;
            attacker.Runtime.SetPosition(0, 0, 0);
            target.Runtime.SetPosition(10, 0, 0);
            attacker.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            LF2FrameData attackFrame = attacker.Frame.D;
            attackFrame.itrs.Add(new InteractionArea
            {
                kind = 3,
                x = -20,
                y = -20,
                w = 60,
                h = 40,
                zwidth = 20,
                catchingact = new[] { -10 },
                caughtact = new[] { 20 },
                respond = 1,
            });
            target.Frame.D.bodies.Add(new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            });
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(9600);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.True);
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(target.Runtime.CatchSourceSlot90, Is.Zero);
            Assert.That(target.Runtime.CatcherSlotIndex, Is.Zero);
            Assert.That(attacker.Runtime.CaughtDuration, Is.EqualTo(1));
        }

        [Test]
        public void Kind3ShadowCompare_MissingRelationFrameProjectsAtomicNoOp()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            attacker.Team = 1;
            attacker.RelationTeam = 1;
            target.Team = 2;
            target.RelationTeam = 2;
            attacker.Runtime.SetPosition(0, 0, 0);
            target.Runtime.SetPosition(10, 0, 0);
            attacker.Runtime.SetVelocity(7, 1, 2);
            target.Runtime.SetVelocity(-5, 3, 4);
            attacker.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            attacker.Runtime.CaughtSlotIndex = 77;
            attacker.Runtime.CaughtDuration = 88;
            target.Runtime.CatchSourceSlot90 = 66;
            target.Runtime.CatcherSlotIndex = 55;
            target.Runtime.Fall = 44;
            attacker.Frame.D.itrs.Add(new InteractionArea
            {
                kind = 3,
                x = -20,
                y = -20,
                w = 60,
                h = 40,
                zwidth = 20,
                catchingact = new[] { 10 },
                caughtact = new[] { 99 },
                respond = -1,
            });
            target.Frame.D.bodies.Add(new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            });
            PairState before = PairState.Capture(attacker, target, world);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(9601);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.True);
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(PairState.Capture(attacker, target, world), Is.EqualTo(before));
        }

        [Test]
        public void WarmedKind3SuccessAndUnsupported_DoNotAllocateManagedMemory()
        {
            SimulationWorld world = CreateWorld(out TestCharacter attacker, out TestCharacter target);
            InteractionArea success = CreateItr(10, 20, 0);
            InteractionArea unsupported = CreateItr(10, 99, 0);
            Assert.That(world.InteractionWriter.TryApplyGrab(attacker, target, success, 3), Is.True);
            Assert.That(world.InteractionWriter.TryApplyGrab(attacker, target, unsupported, 3), Is.False);

            _ = System.GC.GetAllocatedBytesForCurrentThread();
            long before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                world.InteractionWriter.TryApplyGrab(attacker, target, success, 3);
                world.InteractionWriter.TryApplyGrab(attacker, target, unsupported, 3);
            }
            long allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static SimulationWorld CreateWorld(
            out TestCharacter attacker,
            out TestCharacter target)
        {
            var world = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 512);
            attacker = CreateEntity(world, "B6CatchAttacker", 9600, 0, 10);
            target = CreateEntity(world, "B6CatchTarget", 9601, 511, 40);
            attacker.Runtime.SetPosition(10.75, 12.5, 3.25);
            target.Runtime.SetPosition(40.5, 15.75, 3.25);
            attacker.Runtime.SetVelocity(7, 1, 2);
            target.Runtime.SetVelocity(-5, 3, 4);
            attacker.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            attacker.SwitchDir("left");
            target.SwitchDir("right");
            attacker.Trans.SetWait(attacker.Frame.D.wait, 71);
            target.Trans.SetWait(target.Frame.D.wait, 73);
            return world;
        }

        private static TestCharacter CreateEntity(
            SimulationWorld world,
            string name,
            int objectId,
            int slot,
            int x)
        {
            var frame0 = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 1,
                next = 0,
                centerx = 4,
                centery = 8,
                cpoint = new CatchPoint { x = 2 },
            };
            var frame10 = new LF2FrameData
            {
                frameId = 10,
                state = LF2States.Catching,
                wait = 2,
                next = 10,
                centerx = 7,
                centery = 11,
                cpoint = new CatchPoint { x = 3 },
            };
            var frame20 = new LF2FrameData
            {
                frameId = 20,
                state = LF2States.BeingCaught,
                wait = 3,
                next = 20,
                centerx = 9,
                centery = 14,
                cpoint = new CatchPoint { x = 5 },
            };
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = new List<LF2FrameData> { frame0, frame10, frame20 },
            };
            var entity = new TestCharacter();
            entity.ModuleInitialize();
            entity.Name = name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = frame0;
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = frame0;
            entity.Runtime.PrevFrame2 = 0;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            return entity;
        }

        private static TestCharacter CreateEntityFromData(
            SimulationWorld world,
            string name,
            int objectId,
            int slot,
            LF2CharacterData data,
            int frameId)
        {
            var entity = new TestCharacter();
            entity.ModuleInitialize();
            entity.Name = name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            LF2FrameData frame = entity.FrameCache.GetFrameDataById(frameId);
            entity.Frame.D = frame;
            entity.Frame.N = frameId;
            entity.Frame.PN = frameId;
            entity.Frame.Prev = frameId;
            entity.Frame.Prev2 = frameId;
            entity.Frame.Prev2D = frame;
            entity.Runtime.PrevFrame2 = frameId;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private static InteractionArea CreateItr(int catching, int caught, int respond)
        {
            return new InteractionArea
            {
                kind = 3,
                catchingact = new[] { catching, 777 },
                caughtact = new[] { caught, 778 },
                respond = respond,
            };
        }

        private sealed class TestCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Character;
            }
        }

        private readonly struct PairState
        {
            private PairState(string value)
            {
                Value = value;
            }

            private string Value { get; }

            internal static PairState Capture(
                TestCharacter attacker,
                TestCharacter target,
                SimulationWorld world)
            {
                return new PairState(string.Join("|", new object[]
                {
                    attacker.Frame.N,
                    attacker.Runtime.Frame,
                    attacker.Frame.Prev,
                    attacker.Trans.WaitCounter,
                    attacker.Runtime.X,
                    attacker.Runtime.Y,
                    attacker.Runtime.XInt,
                    attacker.Runtime.YInt,
                    attacker.Runtime.Vx,
                    attacker.Runtime.Vy,
                    attacker.Runtime.Vz,
                    attacker.Runtime.Dir,
                    attacker.Runtime.CaughtSlotIndex,
                    attacker.Runtime.CaughtDuration,
                    target.Frame.N,
                    target.Runtime.Frame,
                    target.Frame.Prev,
                    target.Trans.WaitCounter,
                    target.Runtime.X,
                    target.Runtime.Y,
                    target.Runtime.XInt,
                    target.Runtime.YInt,
                    target.Runtime.Vx,
                    target.Runtime.Vy,
                    target.Runtime.Vz,
                    target.Runtime.Dir,
                    target.Runtime.CatchSourceSlot90,
                    target.Runtime.CatcherSlotIndex,
                    target.Runtime.Fall,
                    world.Rng.State,
                    world.Rng.CallCount,
                }));
            }

            public override bool Equals(object obj)
            {
                return obj is PairState other && Value == other.Value;
            }

            public override int GetHashCode()
            {
                return Value?.GetHashCode() ?? 0;
            }

            public override string ToString()
            {
                return Value;
            }
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28B6CatchRelationExactFieldsPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B6-CatchRelationExactFields-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B6-CatchRelationExactFields-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B6CatchRelationExactFieldsPlayRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (running || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || !File.Exists(RequestPath))
            {
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            running = true;
            File.Delete(RequestPath);
            if (File.Exists(ResultPath)) File.Delete(ResultPath);
            try
            {
                var tests = new NTSD28B6CatchRelationExactFieldsProductionEditorTests();
                tests.Kind3Success_WritesExactReciprocalAndRespondTimeout();
                tests.Kind3MissingCaughtFrame_ReturnsFalseWithoutAnyMutation();
                tests.Kind3SignedActions_FlipOnlyEncodedSidesAndUseAbsoluteFrames();
                tests.Kind3Respond_UsesZeroDefaultAndPreservesNonZero(0, 300);
                tests.Kind3Respond_UsesZeroDefaultAndPreservesNonZero(1, 1);
                tests.Kind3Respond_UsesZeroDefaultAndPreservesNonZero(300, 300);
                tests.Kind3Respond_UsesZeroDefaultAndPreservesNonZero(-1, -1);
                tests.Kind3SignedActions_FlipOnlyTheirOwnInitialFacing(-10, 20, -1, -1);
                tests.Kind3SignedActions_FlipOnlyTheirOwnInitialFacing(10, -20, 1, 1);
                tests.Kind3SignedActions_FlipOnlyTheirOwnInitialFacing(-10, -20, -1, 1);
                tests.Kind3MissingAnyRelationFrame_PreservesWholePair(99, 20);
                tests.Kind3MissingAnyRelationFrame_PreservesWholePair(10, 99);
                tests.Kind3MissingAnyRelationFrame_PreservesWholePair(99, 98);
                tests.Kind3MissingActionArrays_DefaultToAuthoredFrameZero();
                tests.CurrentCriminalFrame340_AppliesAuthored341And130ExactRelation();
                tests.Kind1CompatibilityPath_KeepsLegacyTimeoutAndDoesNotWriteExactSource();
                tests.Kind3ShadowCompare_ProjectsExactSourceAndAllWriterEffects();
                tests.Kind3ShadowCompare_MissingRelationFrameProjectsAtomicNoOp();
                tests.WarmedKind3SuccessAndUnsupported_DoNotAllocateManagedMemory();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=19\n" +
                    "currentCriminal340=341/130\n" +
                    "exactAndCompatRelation=matched\n" +
                    "signedMissingRespond=exact\n" +
                    "shadowDifferenceMask=0\n" +
                    "warmedAllocationBytes=0\n" +
                    "sceneMutation=none\n",
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    ResultPath,
                    "state=Failed\n" + exception,
                    new UTF8Encoding(false));
            }
            finally
            {
                EditorApplication.delayCall += ExitPlayMode;
            }
        }

        private static void ExitPlayMode()
        {
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
            running = false;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
