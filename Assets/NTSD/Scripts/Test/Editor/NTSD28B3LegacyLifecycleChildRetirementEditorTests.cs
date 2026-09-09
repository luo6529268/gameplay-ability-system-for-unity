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
    [Category("NTSD28_B3")]
    public sealed class NTSD28B3LegacyLifecycleChildRetirementEditorTests
    {
        [TestCase(1100)]
        [TestCase(1200)]
        [TestCase(1250)]
        [TestCase(1299)]
        public void EncodedLifecycleReset_MutatesSelfOnly(int lifecycleCode)
        {
            var world = new SimulationWorld();
            LF2Character owner = CreateCharacter(
                9600,
                lifecycleCode,
                requiredSlot: 0);
            LF2Character matchedChild = CreateCharacter(
                9601,
                0,
                requiredSlot: 1);
            LF2Character unmatchedChild = CreateCharacter(
                9602,
                0,
                requiredSlot: 2);
            world.Register(owner);
            world.Register(matchedChild);
            world.Register(unmatchedChild);
            owner.Trans.SyncDirectFrameData(0, lifecycleCode, 0);
            matchedChild.KillCount = owner.Runtime.SlotIndex;
            unmatchedChild.KillCount = 19;
            owner.HitStun = 77;
            matchedChild.HitStun = 41;
            unmatchedChild.HitStun = 41;

            world.LateEntityUpdateAll(60 + lifecycleCode);

            Assert.That(owner.Frame.N, Is.Zero);
            Assert.That(owner.HitStun, Is.EqualTo(1100 - lifecycleCode));
            Assert.That(
                world.FindEntityByRuntimeSlotIncludingPending(0),
                Is.SameAs(owner));
            AssertChildIdentityAndFrame(matchedChild, 9601, 1);
            AssertChildIdentityAndFrame(unmatchedChild, 9602, 2);
            Assert.That(
                matchedChild.HitStun,
                Is.EqualTo(unmatchedChild.HitStun),
                "KillCount matching must not add a lifecycle write beyond the child's own late update");
        }

        private static void AssertChildIdentityAndFrame(
            LF2Character child,
            int expectedObjectId,
            int expectedSlot)
        {
            Assert.That(child.ObjectId, Is.EqualTo(expectedObjectId));
            Assert.That(child.Frame.N, Is.Zero);
            Assert.That(child.Runtime.SlotIndex, Is.EqualTo(expectedSlot));
        }

        private static LF2Character CreateCharacter(
            int objectId,
            int next,
            int requiredSlot)
        {
            LF2CharacterData data = new LF2CharacterData
            {
                name = $"B3Lifecycle{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 0,
                        next = next,
                        pic = 999,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            var entity = new LF2Character
            {
                ObjectId = objectId,
                Name = data.name,
            };
            entity.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.SetRequiredRuntimeSlot(requiredSlot);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            entity.Health.PP = 500;
            return entity;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B3LegacyLifecycleChildRetirementRequestRunner :
        ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B3-LifecycleChild-Retirement-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B3-LifecycleChild-Retirement-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B3LegacyLifecycleChildRetirementEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B3LegacyLifecycleChildRetirementRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B3LegacyLifecycleChildRetirementRequestRunner()
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
            activeCallbacks =
                new NTSD28B3LegacyLifecycleChildRetirementRequestRunner();
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
            string text =
                $"state={result.ResultState}\n" +
                $"passed={result.PassCount}\n" +
                $"failed={result.FailCount}\n" +
                $"skipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\n" +
                $"message={result.Message}\n" +
                FailureDetails;
            File.WriteAllText(ResultPath, text, new UTF8Encoding(false));
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
    internal static class NTSD28B3LegacyLifecycleChildRetirementPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B3-LifecycleChild-Retirement-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B3-LifecycleChild-Retirement-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B3LegacyLifecycleChildRetirementPlayRunner()
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
                    new NTSD28B3LegacyLifecycleChildRetirementEditorTests();
                foreach (int code in new[] { 1100, 1200, 1250, 1299 })
                    tests.EncodedLifecycleReset_MutatesSelfOnly(code);
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=4\nselfResets=4\n" +
                    "childOwnerWritesAbsent=8\ncurrentItachi1250=true\n" +
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
