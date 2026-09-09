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
    public sealed class NTSD28B4RevivalQueuedContinuationProductionEditorTests
    {
        [Test]
        public void ExplicitActiveController_OwnsQueuedGroupAndMutationOrder()
        {
            var world = new SimulationWorld();
            CreateCharacter(world, 0, 9500, 3);
            LF2Character continuation = CreateCharacter(world, 20, 37, 9);
            ConfigureQueued(continuation, controllerSlot: 0);
            bool callbackSawFinalMechanicalState = false;
            int spawnCount = 0;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) =>
            {
                spawnCount++;
                callbackSawFinalMechanicalState =
                    source.HP2Orig == 4 &&
                    source.HPOrig == 0 &&
                    source.RespawnCount == 0 &&
                    source.Health.HP == 320 &&
                    source.Health.HPBound == 320 &&
                    source.Health.HP3 == 320 &&
                    source.Health.PP == 0 &&
                    source.RelationTeam == 3 &&
                    source.Runtime.RenderPicOffset == 114 &&
                    source.Runtime.ReviveVisualRuntime180 == 114 &&
                    source.Frame.N == 219 &&
                    source.AttackingCounter == 0 &&
                    source.FrameDelay == 10;
                return source;
            });

            world.PostFrameAdvanceDeathCleanupAll(1);

            Assert.That(spawnCount, Is.EqualTo(1));
            Assert.That(callbackSawFinalMechanicalState, Is.True);
            AssertQueuedSuccess(continuation, 3, 114);
        }

        [TestCase(7)]
        [TestCase(10000)]
        public void MissingExplicitController_DefersWithoutPartialMutation(
            int controllerSlot)
        {
            var world = new SimulationWorld();
            LF2Character continuation = CreateCharacter(world, 20, 9510, 9);
            ConfigureQueued(continuation, controllerSlot);
            continuation.Runtime.RenderPicOffset = 23;
            continuation.Runtime.ReviveVisualRuntime180 = 24;
            continuation.AttackingCounter = 5;
            continuation.FrameDelay = 6;
            int spawnCount = 0;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) =>
            {
                spawnCount++;
                return source;
            });

            world.PostFrameAdvanceDeathCleanupAll(2);

            Assert.That(spawnCount, Is.Zero);
            Assert.That(continuation.HP2Orig, Is.EqualTo(1));
            Assert.That(continuation.HPOrig, Is.EqualTo(4));
            Assert.That(continuation.RespawnCount, Is.EqualTo(320));
            Assert.That(continuation.Health.HP, Is.Zero);
            Assert.That(continuation.Health.HPBound, Is.EqualTo(10));
            Assert.That(continuation.Health.HP3, Is.EqualTo(10));
            Assert.That(continuation.Health.PP, Is.EqualTo(77));
            Assert.That(continuation.RelationTeam, Is.EqualTo(9));
            Assert.That(continuation.Runtime.RenderPicOffset, Is.EqualTo(23));
            Assert.That(continuation.Runtime.ReviveVisualRuntime180,
                Is.EqualTo(24));
            Assert.That(continuation.Frame.N, Is.EqualTo(14));
            Assert.That(continuation.AttackingCounter, Is.EqualTo(5));
            Assert.That(continuation.FrameDelay, Is.EqualTo(6));
        }

        [Test]
        public void MinusOneController_UsesActivePhysicalSlotOneGroup()
        {
            var world = new SimulationWorld();
            CreateCharacter(world, 1, 9520, 7);
            LF2Character continuation = CreateCharacter(world, 20, 39, 9);
            ConfigureQueued(continuation, controllerSlot: -1);
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) => source);

            world.PostFrameAdvanceDeathCleanupAll(3);

            AssertQueuedSuccess(continuation, 7, 140);
        }

        [Test]
        public void MinusOneController_UsesInactivePhysicalSlotOneZeroGroup()
        {
            var world = new SimulationWorld();
            LF2Character continuation = CreateCharacter(world, 20, 39, 9);
            ConfigureQueued(continuation, controllerSlot: -1);
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) => source);

            world.PostFrameAdvanceDeathCleanupAll(4);

            AssertQueuedSuccess(continuation, 0, 140);
        }

        [TestCase(30, 140)]
        [TestCase(36, 140)]
        [TestCase(39, 140)]
        [TestCase(37, 114)]
        [TestCase(38, 23)]
        [TestCase(44, 23)]
        public void NonPositiveVisual_UsesOnlyNativeObjectFallbacks(
            int objectId,
            int expectedVisual)
        {
            var world = new SimulationWorld();
            CreateCharacter(world, 0, 9530, 2);
            LF2Character continuation = CreateCharacter(
                world,
                20,
                objectId,
                9);
            ConfigureQueued(continuation, controllerSlot: 0);
            continuation.Runtime.RenderPicOffset = 23;
            continuation.Runtime.ReviveVisualRuntime180 = 24;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) => source);

            world.PostFrameAdvanceDeathCleanupAll(5);

            Assert.That(continuation.Runtime.RenderPicOffset,
                Is.EqualTo(expectedVisual));
            Assert.That(
                continuation.Runtime.ReviveVisualRuntime180,
                Is.EqualTo(expectedVisual == 23 ? 24 : expectedVisual));
        }

        [Test]
        public void PositiveExplicitVisual_OverridesObjectFallback()
        {
            var world = new SimulationWorld();
            CreateCharacter(world, 0, 9540, 2);
            LF2Character continuation = CreateCharacter(world, 20, 37, 9);
            ConfigureQueued(continuation, controllerSlot: 0);
            continuation.Runtime.ReviveVisualId184 = 77;
            continuation.Runtime.RenderPicOffset = 23;
            continuation.Runtime.ReviveVisualRuntime180 = 24;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) => source);

            world.PostFrameAdvanceDeathCleanupAll(6);

            AssertQueuedSuccess(continuation, 2, 77);
            Assert.That(continuation.Runtime.ReviveVisualId184, Is.EqualTo(77));
        }

        [Test]
        public void ReviveVisualCarriers_CopyAndResetWithCanonicalRuntime()
        {
            var source = new NTSDEntityRuntime
            {
                ReviveVisualId184 = 77,
                ReviveVisualRuntime180 = 88,
            };
            var copy = new NTSDEntityRuntime();

            Assert.That(source.TryCopyCanonicalStateTo(copy), Is.True);
            Assert.That(copy.ReviveVisualId184, Is.EqualTo(77));
            Assert.That(copy.ReviveVisualRuntime180, Is.EqualTo(88));

            source.Reset();
            Assert.That(source.ReviveVisualId184, Is.Zero);
            Assert.That(source.ReviveVisualRuntime180, Is.Zero);
        }

        private static void AssertQueuedSuccess(
            LF2Character continuation,
            int expectedGroup,
            int expectedVisual)
        {
            Assert.That(continuation.HP2Orig, Is.EqualTo(4));
            Assert.That(continuation.HPOrig, Is.Zero);
            Assert.That(continuation.RespawnCount, Is.Zero);
            Assert.That(continuation.Health.HP, Is.EqualTo(320));
            Assert.That(continuation.Health.HPBound, Is.EqualTo(320));
            Assert.That(continuation.Health.HP3, Is.EqualTo(320));
            Assert.That(continuation.Health.PP, Is.Zero);
            Assert.That(continuation.RelationTeam, Is.EqualTo(expectedGroup));
            Assert.That(continuation.Runtime.RenderPicOffset,
                Is.EqualTo(expectedVisual));
            Assert.That(continuation.Runtime.ReviveVisualRuntime180,
                Is.EqualTo(expectedVisual));
            Assert.That(continuation.Frame.N, Is.EqualTo(219));
            Assert.That(continuation.AttackingCounter, Is.Zero);
            Assert.That(continuation.FrameDelay, Is.EqualTo(10));
        }

        private static void ConfigureQueued(
            LF2Character continuation,
            int controllerSlot)
        {
            continuation.DirectWriteFramePreserveWaitCounter(14);
            continuation.Health.HP = 0;
            continuation.Health.HPBound = 10;
            continuation.Health.HP3 = 10;
            continuation.Health.PP = 77;
            continuation.HP2Orig = 1;
            continuation.HPOrig = 4;
            continuation.RespawnCount = 320;
            continuation.HitStun = 2;
            continuation.Runtime.Unk360 = controllerSlot;
            continuation.RefreshRuntimeSnapshot();
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int relationTeam)
        {
            var data = new LF2CharacterData
            {
                name = "B4Queued_" + objectId + "_" + slot,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(14, LF2States.Lying),
                    Frame(219, LF2States.Standing),
                },
            };
            var character = new LF2Character
            {
                ObjectId = objectId,
                Name = data.name,
            };
            character.ModuleInitialize();
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            character.ImmediateFrame(14);
            character.Initialize(500, 500);
            character.RelationTeam = relationTeam;
            character.Team = relationTeam;
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            character.RefreshRuntimeSnapshot();
            return character;
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
    internal sealed class NTSD28B4RevivalQueuedContinuationRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalQueuedContinuation-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalQueuedContinuation-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B4RevivalQueuedContinuationProductionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(4096);
        private static NTSD28B4RevivalQueuedContinuationRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B4RevivalQueuedContinuationRequestRunner()
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
            activeCallbacks = new NTSD28B4RevivalQueuedContinuationRequestRunner();
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
            File.WriteAllText(
                ResultPath,
                resultText,
                new UTF8Encoding(false));
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
    internal static class NTSD28B4RevivalQueuedContinuationPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalQueuedContinuation-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalQueuedContinuation-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B4RevivalQueuedContinuationPlayRunner()
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
                    new NTSD28B4RevivalQueuedContinuationProductionEditorTests();
                tests.ExplicitActiveController_OwnsQueuedGroupAndMutationOrder();
                tests.MissingExplicitController_DefersWithoutPartialMutation(7);
                tests.MissingExplicitController_DefersWithoutPartialMutation(10000);
                tests.MinusOneController_UsesActivePhysicalSlotOneGroup();
                tests.MinusOneController_UsesInactivePhysicalSlotOneZeroGroup();
                tests.NonPositiveVisual_UsesOnlyNativeObjectFallbacks(30, 140);
                tests.NonPositiveVisual_UsesOnlyNativeObjectFallbacks(36, 140);
                tests.NonPositiveVisual_UsesOnlyNativeObjectFallbacks(39, 140);
                tests.NonPositiveVisual_UsesOnlyNativeObjectFallbacks(37, 114);
                tests.NonPositiveVisual_UsesOnlyNativeObjectFallbacks(38, 23);
                tests.NonPositiveVisual_UsesOnlyNativeObjectFallbacks(44, 23);
                tests.PositiveExplicitVisual_OverridesObjectFallback();
                tests.ReviveVisualCarriers_CopyAndResetWithCanonicalRuntime();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\nlogicalCases=13\n" +
                    "controller=active,missing,out-of-range,fallback-active," +
                    "fallback-inactive\nvisual=30,36,39,37,38,44\n" +
                    "explicitVisual=77\ncarrierCopyReset=pass\n" +
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
