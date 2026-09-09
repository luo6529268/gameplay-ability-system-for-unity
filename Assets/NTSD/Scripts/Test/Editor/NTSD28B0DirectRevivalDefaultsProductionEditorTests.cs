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
    [Category("NTSD28_B0")]
    public sealed class NTSD28B0DirectRevivalDefaultsProductionEditorTests
    {
        [TestCase(0)]
        [TestCase(19)]
        public void DirectParticipant_PublishesRevivalDefaultsBeforeFirstSnapshot(
            int physicalSlot)
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(9700 + physicalSlot);
            entity.ModuleInitialize();
            entity.HP2Orig = 77;
            entity.HPOrig = 88;
            entity.RespawnCount = 99;

            Assert.That(
                BattleMatchConfigRuntimeAdapter
                    .PrepareDirectParticipantRegistration(
                        entity,
                        physicalSlot),
                Is.True);
            AssertRevivalDefaults(entity);

            LF2CharacterDataWrapper wrapper = entity.FrameCache.Wrapper;
            entity.ModuleBind(wrapper, entity.ObjectId, world);
            entity.Initialize(500, 500);
            entity.RefreshRuntimeSnapshot();

            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(physicalSlot));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(physicalSlot));
            AssertRevivalDefaults(entity);
            RuntimeSlotTable.ReadOnlySlotView view =
                world.RuntimeSlotTableForModules.GetReadOnlyView(physicalSlot);
            Assert.That(view.Claimed, Is.True);
            Assert.That(view.Entity, Is.SameAs(entity));
            Assert.That(view.Entity.HP2Orig, Is.EqualTo(1));
            Assert.That(view.Entity.HPOrig, Is.Zero);
            Assert.That(view.Entity.RespawnCount, Is.Zero);
            Assert.That(view.RawRuntime, Is.Not.Null);
            Assert.That(view.RawRuntime.HP2Orig, Is.Zero);
            Assert.That(view.RawRuntime.HPOrig, Is.Zero);
            Assert.That(view.RawRuntime.RespawnCount, Is.Zero);
        }

        [Test]
        public void InvalidParticipant_DoesNotMutateRevivalSentinels()
        {
            LF2Character entity = CreateCharacter(9799);
            entity.HP2Orig = 77;
            entity.HPOrig = 88;
            entity.RespawnCount = 99;

            Assert.That(
                BattleMatchConfigRuntimeAdapter
                    .PrepareDirectParticipantRegistration(entity, 20),
                Is.False);
            Assert.That(entity.HP2Orig, Is.EqualTo(77));
            Assert.That(entity.HPOrig, Is.EqualTo(88));
            Assert.That(entity.RespawnCount, Is.EqualTo(99));
            Assert.That(
                BattleMatchConfigRuntimeAdapter
                    .PrepareDirectParticipantRegistration(null, 0),
                Is.False);
        }

        private static void AssertRevivalDefaults(LF2Entity entity)
        {
            Assert.That(entity.HP2Orig, Is.EqualTo(1));
            Assert.That(entity.HPOrig, Is.Zero);
            Assert.That(entity.RespawnCount, Is.Zero);
        }

        private static LF2Character CreateCharacter(int objectId)
        {
            var data = new LF2CharacterData
            {
                name = $"B0DirectRevival{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 10000,
                        next = 0,
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
            return entity;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B0DirectRevivalDefaultsRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-DirectRevivalDefaults-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-DirectRevivalDefaults-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B0DirectRevivalDefaultsProductionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B0DirectRevivalDefaultsRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B0DirectRevivalDefaultsRequestRunner()
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
            activeCallbacks = new NTSD28B0DirectRevivalDefaultsRequestRunner();
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
    internal static class NTSD28B0DirectRevivalDefaultsPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-DirectRevivalDefaults-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-DirectRevivalDefaults-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B0DirectRevivalDefaultsPlayRunner()
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
                    new NTSD28B0DirectRevivalDefaultsProductionEditorTests();
                tests.DirectParticipant_PublishesRevivalDefaultsBeforeFirstSnapshot(0);
                tests.DirectParticipant_PublishesRevivalDefaultsBeforeFirstSnapshot(19);
                tests.InvalidParticipant_DoesNotMutateRevivalSentinels();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=3\nslots=0,19\n" +
                    "defaults=1,0,0\nrawBacking=0,0,0\nsceneMutation=none\n",
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
