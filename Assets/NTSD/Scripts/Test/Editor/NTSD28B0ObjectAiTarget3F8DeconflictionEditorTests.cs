#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B0")]
    public sealed class NTSD28B0ObjectAiTarget3F8DeconflictionEditorTests
    {
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(399)]
        public void CanonicalTargetAlias_UsesExistingPickerStorageAndCanonicalCopy(int targetSlot)
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();

            source.ObjectAiTargetSlot3F8 = targetSlot;

            Assert.That(source.PickerStableId, Is.EqualTo(targetSlot));
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            Assert.That(destination.ObjectAiTargetSlot3F8, Is.EqualTo(targetSlot));
            Assert.That(destination.PickerStableId, Is.EqualTo(targetSlot));

            source.Reset();
            Assert.That(source.ObjectAiTargetSlot3F8, Is.EqualTo(-1));
        }

        [Test]
        public void OpointTaskClear_ResetsOwnerAndTrackedTargetIndependently()
        {
            var task = new OPointCreateTask
            {
                ownerEntityIndex = 7,
                trackedTargetSlot = 0,
            };

            task.Clear();

            Assert.That(task.ownerEntityIndex, Is.EqualTo(-1));
            Assert.That(task.trackedTargetSlot, Is.EqualTo(-1));
        }

        [Test]
        public void CommonScan_WritesTargetWithoutOverwritingOwner()
        {
            var world = new SimulationWorld();
            LF2Character target = CreateCharacter(91000, LF2States.Standing, hitStun: 0);
            LF2OtherObject source = CreateOther(91001, hitFa: 3);
            target.SetRequiredRuntimeSlot(0);
            source.SetRequiredRuntimeSlot(50);
            target.Team = target.RelationTeam = 2;
            source.Team = source.RelationTeam = 1;
            source.OwnerEntityIndex = 7;
            target.Runtime.SetPosition(100.0, 0.0, 20.0);
            source.Runtime.SetPosition(0.0, 0.0, 0.0);
            target.Runtime.SyncIntegerPosition();
            source.Runtime.SyncIntegerPosition();
            world.Register(target);
            world.Register(source);

            source.RunFrameLogicBeforeAdvance();

            Assert.That(source.OwnerEntityIndex, Is.EqualTo(7));
            Assert.That(source.ObjectAiTargetSlot3F8, Is.EqualTo(0));
            Assert.That(source.Runtime.Vx, Is.EqualTo(0.7).Within(0.000001));
            Assert.That(source.Runtime.Vz, Is.EqualTo(0.17).Within(0.000001));
        }

        [Test]
        public void CommonScanMiss_PreservesNonSentinelStaleTargetAndOwner()
        {
            var world = new SimulationWorld();
            LF2OtherObject source = CreateOther(91002, hitFa: 3);
            source.SetRequiredRuntimeSlot(50);
            source.OwnerEntityIndex = 7;
            source.ObjectAiTargetSlot3F8 = 37;
            source.Health.HP = 88;
            world.Register(source);

            source.RunFrameLogicBeforeAdvance();

            Assert.That(source.OwnerEntityIndex, Is.EqualTo(7));
            Assert.That(source.ObjectAiTargetSlot3F8, Is.EqualTo(37));
            Assert.That(source.Health.HP, Is.EqualTo(88));
            Assert.That(source.Runtime.Vx, Is.Zero);
            Assert.That(source.Runtime.Vz, Is.Zero);
        }

        [Test]
        public void CommonScan_State14AndRenderPhaseGatesFollowAuthorityOrder()
        {
            var world = new SimulationWorld();
            LF2Character renderPhaseRejected = CreateCharacter(
                91003,
                LF2States.Standing,
                hitStun: 3);
            LF2Character state14Accepted = CreateCharacter(
                91004,
                LF2States.Lying,
                hitStun: 9);
            LF2OtherObject source = CreateOther(91005, hitFa: 3);
            renderPhaseRejected.SetRequiredRuntimeSlot(0);
            state14Accepted.SetRequiredRuntimeSlot(1);
            source.SetRequiredRuntimeSlot(50);
            renderPhaseRejected.Team = renderPhaseRejected.RelationTeam = 2;
            state14Accepted.Team = state14Accepted.RelationTeam = 2;
            source.Team = source.RelationTeam = 1;
            source.OwnerEntityIndex = 7;
            renderPhaseRejected.Runtime.SetPosition(10.0, 0.0, 0.0);
            state14Accepted.Runtime.SetPosition(20.0, 0.0, 0.0);
            source.Runtime.SetPosition(0.0, 0.0, 0.0);
            renderPhaseRejected.Runtime.SyncIntegerPosition();
            state14Accepted.Runtime.SyncIntegerPosition();
            source.Runtime.SyncIntegerPosition();
            world.Register(renderPhaseRejected);
            world.Register(state14Accepted);
            world.Register(source);

            source.RunFrameLogicBeforeAdvance();

            Assert.That(source.OwnerEntityIndex, Is.EqualTo(7));
            Assert.That(source.ObjectAiTargetSlot3F8, Is.EqualTo(1));
        }

        private static LF2OtherObject CreateOther(int objectId, int hitFa)
        {
            var entity = new LF2OtherObject
            {
                ObjectId = objectId,
                Name = $"B0TargetSource{objectId}",
            };
            var data = new LF2CharacterData
            {
                name = entity.Name,
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.ProjectileFlying, hitFa),
                },
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            return entity;
        }

        private static LF2Character CreateCharacter(int objectId, int state, int hitStun)
        {
            var entity = new LF2Character
            {
                ObjectId = objectId,
                Name = $"B0TargetCharacter{objectId}",
            };
            var data = new LF2CharacterData
            {
                name = entity.Name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state, hitFa: 0),
                },
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            entity.HitStun = hitStun;
            return entity;
        }

        private static LF2FrameData Frame(int frameId, int state, int hitFa)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = frameId,
                pic = 999,
                centerx = 39,
                centery = 79,
                hit_Fa = hitFa,
            };
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B0ObjectAiTarget3F8RequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-ObjectAiTarget3F8-v2.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-ObjectAiTarget3F8-v2.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B0ObjectAiTarget3F8DeconflictionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B0ObjectAiTarget3F8RequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B0ObjectAiTarget3F8RequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }

            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            File.Delete(RequestPath);

            activeCallbacks = new NTSD28B0ObjectAiTarget3F8RequestRunner();
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
            Object.DestroyImmediate(activeApi);
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
}
#endif
