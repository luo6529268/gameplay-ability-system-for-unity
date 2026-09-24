#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B4")]
    public sealed class NTSD28B4RevivalNormalFloorRngProductionEditorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void NormalRevivalRandomOffsetUsesViewRatio(bool configuredView)
        {
            var world = new SimulationWorld();
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            LF2Character dead = CreateCharacter(world, 0, 9650, 3);
            LF2Character peerA = CreateCharacter(world, 1, 9651, 3);
            LF2Character peerB = CreateCharacter(world, 2, 9652, 3);
            ConfigureNormal(dead, collisionYReference: -37);
            SetPosition(dead, 40, -100, 60, 11, -100, 12);
            SetPosition(peerA, 100, 0, 40, 100, 0, 40);
            SetPosition(peerB, 160, 0, 20, 160, 0, 20);

            var expectedRandom = new NTSD28NativeRandom();
            expectedRandom.RestoreSynchronized(
                world.NativeRandom.CaptureSynchronizedState());
            int randomX = expectedRandom.SynchronizedNext(0x90u, 0x33) - 25;
            int randomZ = expectedRandom.SynchronizedNext(0x91u, 0x1f) - 15;
            double expectedX = 130 + randomX *
                (configuredView ? 2048.0 / 1333.0 : 1.0);
            double expectedZ = 30 + randomZ *
                (configuredView ? 1152.0 / 730.0 : 1.0);
            ulong beforeCalls = world.NativeRandom.CaptureScalarState().SynchronizedCalls;

            world.PostFrameAdvanceDeathCleanupAll(1);

            Assert.That(dead.Runtime.X, Is.EqualTo(expectedX).Within(0.000001));
            Assert.That(dead.Runtime.Z, Is.EqualTo(expectedZ).Within(0.000001));
            Assert.That(dead.Runtime.XInt, Is.EqualTo(11));
            Assert.That(dead.Runtime.ZInt, Is.EqualTo(12));
            Assert.That(dead.Runtime.Y, Is.EqualTo(-37));
            Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(beforeCalls + 2));
        }

        [Test]
        public void NormalRevival_UsesSynchronizedRngAndPreservesIntegerXZ()
        {
            var world = new SimulationWorld();
            LF2Character dead = CreateCharacter(world, 0, 9600, 3);
            LF2Character peerA = CreateCharacter(world, 1, 9601, 3);
            LF2Character peerB = CreateCharacter(world, 2, 9602, 3);
            LF2OtherObject nonCharacter = CreateOther(world, 3, 9603, 3);
            ConfigureNormal(dead, collisionYReference: -37);
            SetPosition(dead, 40, -100, 60, 11, -100, 12);
            SetPosition(peerA, 100, 0, 40, 100, 0, 40);
            SetPosition(peerB, 160, 0, 20, 160, 0, 20);
            SetPosition(nonCharacter, 1000, 0, 1000, 1000, 0, 1000);

            var expectedRandom = new NTSD28NativeRandom();
            expectedRandom.RestoreSynchronized(
                world.NativeRandom.CaptureSynchronizedState());
            int expectedX = 130 +
                expectedRandom.SynchronizedNext(0x90u, 0x33) - 25;
            int expectedZ = 30 +
                expectedRandom.SynchronizedNext(0x91u, 0x1f) - 15;
            NTSD28NativeRandomScalarState nativeBefore =
                world.NativeRandom.CaptureScalarState();
            uint legacyStateBefore = world.Rng.State;
            ulong legacyCallsBefore = world.Rng.CallCount;

            world.PostFrameAdvanceDeathCleanupAll(1);

            Assert.That(dead.Runtime.X, Is.EqualTo(expectedX));
            Assert.That(dead.Runtime.Z, Is.EqualTo(expectedZ));
            Assert.That(dead.Runtime.XInt, Is.EqualTo(11));
            Assert.That(dead.Runtime.ZInt, Is.EqualTo(12));
            Assert.That(dead.PS.x, Is.EqualTo(expectedX));
            Assert.That(dead.PS.z, Is.EqualTo(expectedZ));
            Assert.That(
                world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(nativeBefore.SynchronizedCalls + 2));
            Assert.That(
                world.NativeRandom.CaptureScalarState().LastSynchronizedCallSite,
                Is.EqualTo(0x91u));
            Assert.That(world.Rng.State, Is.EqualTo(legacyStateBefore));
            Assert.That(world.Rng.CallCount, Is.EqualTo(legacyCallsBefore));
            AssertNormalTail(dead, expectedFloor: -37);
        }

        [Test]
        public void SumXZero_PreservesPreciseXZAndConsumesNoRng()
        {
            var world = new SimulationWorld();
            LF2Character dead = CreateCharacter(world, 0, 9610, 4);
            LF2Character peerA = CreateCharacter(world, 1, 9611, 4);
            LF2Character peerB = CreateCharacter(world, 2, 9612, 4);
            ConfigureNormal(dead, collisionYReference: 0);
            SetPosition(dead, 40, -100, 60, 11, -100, 12);
            SetPosition(peerA, 100, 0, 40, 100, 0, 40);
            SetPosition(peerB, -100, 0, 20, -100, 0, 20);
            NTSD28NativeRandomScalarState nativeBefore =
                world.NativeRandom.CaptureScalarState();
            uint legacyStateBefore = world.Rng.State;
            ulong legacyCallsBefore = world.Rng.CallCount;

            world.PostFrameAdvanceDeathCleanupAll(2);

            Assert.That(dead.Runtime.X, Is.EqualTo(40));
            Assert.That(dead.Runtime.Z, Is.EqualTo(60));
            Assert.That(dead.Runtime.XInt, Is.EqualTo(11));
            Assert.That(dead.Runtime.ZInt, Is.EqualTo(12));
            Assert.That(
                world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(nativeBefore.SynchronizedCalls));
            Assert.That(world.Rng.State, Is.EqualTo(legacyStateBefore));
            Assert.That(world.Rng.CallCount, Is.EqualTo(legacyCallsBefore));
            AssertNormalTail(dead, expectedFloor: 0);
        }

        [Test]
        public void NonTypeZeroAndOtherGroup_DoNotEnterPeerAverage()
        {
            var world = new SimulationWorld();
            LF2Character dead = CreateCharacter(world, 0, 9620, 6);
            LF2Character otherGroup = CreateCharacter(world, 1, 9621, 7);
            LF2OtherObject nonCharacter = CreateOther(world, 2, 9622, 6);
            ConfigureNormal(dead, collisionYReference: 0);
            SetPosition(dead, 40, -100, 60, 11, -100, 12);
            SetPosition(otherGroup, 300, 0, 400, 300, 0, 400);
            SetPosition(nonCharacter, 500, 0, 600, 500, 0, 600);
            ulong nativeCallsBefore =
                world.NativeRandom.CaptureScalarState().SynchronizedCalls;

            world.PostFrameAdvanceDeathCleanupAll(3);

            Assert.That(dead.Runtime.X, Is.EqualTo(40));
            Assert.That(dead.Runtime.Z, Is.EqualTo(60));
            Assert.That(
                world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(nativeCallsBefore));
            AssertNormalTail(dead, expectedFloor: 0);
        }

        [TestCase(-25, -25)]
        [TestCase(0, 0)]
        [TestCase(25, 0)]
        public void NormalRevival_UsesSameEffectiveFloorAsC06(
            int collisionYReference,
            int expectedFloor)
        {
            var world = new SimulationWorld();
            LF2Character dead = CreateCharacter(world, 0, 9630, 8);
            ConfigureNormal(dead, collisionYReference);
            SetPosition(dead, 40, -100, 60, 11, -100, 12);

            world.PostFrameAdvanceDeathCleanupAll(4);

            AssertNormalTail(dead, expectedFloor);
            Assert.That(dead.Runtime.X, Is.EqualTo(40));
            Assert.That(dead.Runtime.Z, Is.EqualTo(60));
            Assert.That(dead.Runtime.XInt, Is.EqualTo(11));
            Assert.That(dead.Runtime.ZInt, Is.EqualTo(12));
        }

        [Test]
        public void NegativeIntegerAverages_TruncateTowardZeroBeforeRng()
        {
            var world = new SimulationWorld();
            LF2Character dead = CreateCharacter(world, 0, 9640, 9);
            LF2Character peerA = CreateCharacter(world, 1, 9641, 9);
            LF2Character peerB = CreateCharacter(world, 2, 9642, 9);
            ConfigureNormal(dead, collisionYReference: 0);
            SetPosition(dead, 40, -100, 60, 11, -100, 12);
            SetPosition(peerA, -3, 0, -5, -3, 0, -5);
            SetPosition(peerB, 0, 0, 0, 0, 0, 0);
            var expectedRandom = new NTSD28NativeRandom();
            expectedRandom.RestoreSynchronized(
                world.NativeRandom.CaptureSynchronizedState());
            int expectedX = -1 +
                expectedRandom.SynchronizedNext(0x90u, 0x33) - 25;
            int expectedZ = -2 +
                expectedRandom.SynchronizedNext(0x91u, 0x1f) - 15;

            world.PostFrameAdvanceDeathCleanupAll(5);

            Assert.That(dead.Runtime.X, Is.EqualTo(expectedX));
            Assert.That(dead.Runtime.Z, Is.EqualTo(expectedZ));
            Assert.That(dead.Runtime.XInt, Is.EqualTo(11));
            Assert.That(dead.Runtime.ZInt, Is.EqualTo(12));
        }

        private static void AssertNormalTail(
            LF2Character dead,
            int expectedFloor)
        {
            Assert.That(dead.HP2Orig, Is.EqualTo(2));
            Assert.That(dead.HPOrig, Is.EqualTo(4));
            Assert.That(dead.RespawnCount, Is.EqualTo(80));
            Assert.That(dead.Health.PP, Is.EqualTo(500));
            Assert.That(dead.Health.PPBound, Is.EqualTo(333));
            Assert.That(dead.Health.HPBound, Is.EqualTo(180));
            Assert.That(dead.Health.HP, Is.EqualTo(180));
            Assert.That(dead.HitStun, Is.EqualTo(20));
            Assert.That(dead.Frame.N, Is.EqualTo(212));
            Assert.That(dead.AttackingCounter, Is.EqualTo(9));
            Assert.That(dead.Runtime.Y, Is.EqualTo(expectedFloor));
            Assert.That(dead.Runtime.YInt, Is.EqualTo(expectedFloor));
            Assert.That(dead.Runtime.Vy, Is.Zero);
            Assert.That(dead.PS.y, Is.EqualTo(expectedFloor));
            Assert.That(dead.PS.vy, Is.Zero);
        }

        private static void ConfigureNormal(
            LF2Character dead,
            int collisionYReference)
        {
            dead.DirectWriteFramePreserveWaitCounter(14);
            dead.Health.HP = 0;
            dead.Health.HPBound = 10;
            dead.Health.HP3 = 180;
            dead.Health.PP = 77;
            dead.Health.PPBound = 333;
            dead.HP2Orig = 3;
            dead.HPOrig = 4;
            dead.RespawnCount = 80;
            dead.HitStun = 2;
            dead.AttackingCounter = 9;
            dead.Runtime.CollisionYReference = collisionYReference;
            dead.RefreshRuntimeSnapshot();
        }

        private static void SetPosition(
            LF2Entity entity,
            double preciseX,
            double preciseY,
            double preciseZ,
            int integerX,
            int integerY,
            int integerZ)
        {
            entity.Runtime.SetPosition(integerX, integerY, integerZ);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SetPosition(preciseX, preciseY, preciseZ);
            entity.PS.x = preciseX;
            entity.PS.y = preciseY;
            entity.PS.z = preciseZ;
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int relationTeam)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            BindEntity(
                character,
                slot,
                objectId,
                relationTeam,
                LF2ObjectType.Character,
                new List<LF2FrameData>
                {
                    Frame(14, LF2States.Lying),
                    Frame(212, LF2States.Jump),
                });
            character.Initialize(500, 500);
            world.Register(character);
            character.RefreshRuntimeSnapshot();
            return character;
        }

        private static LF2OtherObject CreateOther(
            SimulationWorld world,
            int slot,
            int objectId,
            int relationTeam)
        {
            var other = new LF2OtherObject();
            BindEntity(
                other,
                slot,
                objectId,
                relationTeam,
                LF2ObjectType.Other,
                new List<LF2FrameData> { Frame(0, LF2States.Standing) });
            world.Register(other);
            other.RefreshRuntimeSnapshot();
            return other;
        }

        private static void BindEntity(
            LF2Entity entity,
            int slot,
            int objectId,
            int relationTeam,
            LF2ObjectType objectType,
            List<LF2FrameData> frames)
        {
            var data = new LF2CharacterData
            {
                name = "B4Normal_" + objectId,
                type_sub = (int)objectType,
                frames = frames,
            };
            entity.ObjectId = objectId;
            entity.Name = data.name;
            entity.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(frames[0].frameId);
            entity.RelationTeam = relationTeam;
            entity.Team = relationTeam;
            entity.SetRequiredRuntimeSlot(slot);
        }

        private static LF2FrameData Frame(int frameId, int state)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 1000,
                next = frameId,
                pic = 999,
                centerx = 39,
                centery = 79,
                itrs = new List<InteractionArea>(),
            };
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B4RevivalNormalFloorRngRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalNormalFloorRng-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalNormalFloorRng-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B4RevivalNormalFloorRngProductionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(4096);
        private static NTSD28B4RevivalNormalFloorRngRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B4RevivalNormalFloorRngRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null || EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }
            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            File.Delete(RequestPath);
            activeCallbacks = new NTSD28B4RevivalNormalFloorRngRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            activeApi.Execute(new ExecutionSettings(
                new Filter
                {
                    testMode = TestMode.EditMode,
                    testNames = new[] { FocusedTestClass },
                })
            {
                runSynchronously = false,
            });
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            FailureDetails.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string resultText =
                $"state={result.ResultState}\n" +
                $"passed={result.PassCount}\n" +
                $"failed={result.FailCount}\n" +
                $"skipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\n" +
                $"message={result.Message}\n" +
                FailureDetails;
            File.WriteAllText(ResultPath, resultText, new UTF8Encoding(false));
            activeApi.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result?.Test == null || result.Test.IsSuite || result.FailCount <= 0)
                return;
            FailureDetails.Append("--- failure ---\n");
            FailureDetails.Append("test=").Append(result.FullName).Append('\n');
            FailureDetails.Append("state=").Append(result.ResultState).Append('\n');
            FailureDetails.Append("message=").Append(result.Message).Append('\n');
            FailureDetails.Append("stack=").Append(result.StackTrace).Append('\n');
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28B4RevivalNormalFloorRngPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalNormalFloorRng-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalNormalFloorRng-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B4RevivalNormalFloorRngPlayRunner()
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
            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            try
            {
                var tests =
                    new NTSD28B4RevivalNormalFloorRngProductionEditorTests();
                tests.NormalRevival_UsesSynchronizedRngAndPreservesIntegerXZ();
                tests.SumXZero_PreservesPreciseXZAndConsumesNoRng();
                tests.NonTypeZeroAndOtherGroup_DoNotEnterPeerAverage();
                tests.NormalRevival_UsesSameEffectiveFloorAsC06(-25, -25);
                tests.NormalRevival_UsesSameEffectiveFloorAsC06(0, 0);
                tests.NormalRevival_UsesSameEffectiveFloorAsC06(25, 0);
                tests.NegativeIntegerAverages_TruncateTowardZeroBeforeRng();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\nlogicalCases=7\n" +
                    "rngCallSites=0x90,0x91\nfloors=-25,0,25\n" +
                    "sumX=nonzero,zero\nintegerXZDeferred=true\n" +
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
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
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
