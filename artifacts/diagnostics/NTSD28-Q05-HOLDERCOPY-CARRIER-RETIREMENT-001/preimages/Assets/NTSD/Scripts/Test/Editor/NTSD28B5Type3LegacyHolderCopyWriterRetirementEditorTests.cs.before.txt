#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B5")]
    public sealed class NTSD28B5Type3LegacyHolderCopyWriterRetirementEditorTests
    {
        [Test]
        public void Kind9Actual_PreservesTargetHolderCopySentinel()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = Register(
                world,
                CreateEntity("Type3HolderCopyAttacker", 9800,
                    LF2ObjectType.Character),
                0,
                7);
            TypedCharacter target = Register(
                world,
                CreateEntity("Type3HolderCopyTarget", 9801,
                    LF2ObjectType.SpecialAttack),
                1,
                2);
            attacker.HolderCopySlot = 77;
            target.HolderCopySlot = 99;
            target.Runtime.SetVelocity(2, 3, 4);
            target.KnockbackVx = 5;
            target.KnockbackVy = 6;
            target.KnockbackVz = 7;

            bool applied = new BattleDamageWriter().ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                new InteractionArea { kind = 9, effect = 0 });

            Assert.That(applied, Is.True);
            Assert.That(target.HolderCopySlot, Is.EqualTo(99));
            Assert.That(target.RelationTeam, Is.EqualTo(7));
            Assert.That(target.Frame.N, Is.EqualTo(30));
            Assert.That(target.Runtime.AnimCounter, Is.Zero);
            Assert.That(target.Runtime.Vx, Is.Zero);
            Assert.That(target.Runtime.Vy, Is.Zero);
            Assert.That(target.Runtime.Vz, Is.Zero);
            Assert.That(attacker.HolderCopySlot, Is.EqualTo(77));
        }

        [Test]
        public void Kind9HitPlan_PreservesTargetHolderCopySentinel()
        {
            new NTSD.Test.BattleHitExecutionPlanEditorTests()
                .ShadowCompare_Type3Kind9WriterEffectMatchesAuthorityState(
                    LF2States.Standing,
                    expectedFrame: 30,
                    expectRelationCopy: true);
        }

        [Test]
        public void Type3ProductionSources_ContainNoLegacyHolderCopyWriter()
        {
            string damageWriter = File.ReadAllText(ProjectPath(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs"));
            string hitPlan = File.ReadAllText(ProjectPath(
                "Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs"));

            StringAssert.DoesNotContain(
                "target.HolderCopySlot = source.HolderCopySlot;",
                damageWriter);
            StringAssert.DoesNotContain(
                "projection.TargetHolderCopySlot = attacker.HolderCopySlot;",
                hitPlan);
            StringAssert.DoesNotContain(
                "projection.TargetHolderCopySlot = relationSource.HolderCopySlot;",
                hitPlan);
            StringAssert.DoesNotContain(
                "projection.TargetHolderCopySlot = attackerSlot;",
                hitPlan,
                "Retired pickup paths must preserve the reserved HolderCopy carrier.");
        }

        [Test]
        public void ExistingSelfCheckType3Contract_PreservesTargetHolderCopySentinel()
        {
            MethodInfo method = typeof(BattleRuntimeSelfCheck).GetMethod(
                "CheckSpecialAttackHitResolveAuditContracts",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, null);
        }

        private static TypedCharacter CreateEntity(
            string name,
            int objectId,
            LF2ObjectType type)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, LF2States.Standing),
                Frame(30, LF2States.Standing),
                Frame(40, LF2States.ObjectFlying),
            };
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = (int)type,
                frames = frames,
            };
            var entity = new TypedCharacter(type)
            {
                Name = name,
                ObjectId = objectId,
            };
            entity.ModuleInitialize();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Initialize(500, 500);
            return entity;
        }

        private static TypedCharacter Register(
            SimulationWorld world,
            TypedCharacter entity,
            int slot,
            int group)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = group;
            entity.RelationTeam = group;
            entity.Runtime.SetPosition(slot * 20, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
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

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType type;

            internal TypedCharacter(LF2ObjectType type)
            {
                this.type = type;
            }

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)type;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B5Type3HolderCopyRetirementRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-Type3HolderCopyRetirement-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-Type3HolderCopyRetirement-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B5Type3LegacyHolderCopyWriterRetirementEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails =
            new StringBuilder(4096);
        private static NTSD28B5Type3HolderCopyRetirementRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B5Type3HolderCopyRetirementRequestRunner()
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
            activeCallbacks = new NTSD28B5Type3HolderCopyRetirementRequestRunner();
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
    internal static class NTSD28B5Type3HolderCopyRetirementPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-Type3HolderCopyRetirement-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-Type3HolderCopyRetirement-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B5Type3HolderCopyRetirementPlayRunner()
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
                    new NTSD28B5Type3LegacyHolderCopyWriterRetirementEditorTests();
                tests.Kind9Actual_PreservesTargetHolderCopySentinel();
                tests.Kind9HitPlan_PreservesTargetHolderCopySentinel();
                tests.Type3ProductionSources_ContainNoLegacyHolderCopyWriter();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=3\n" +
                    "targetHolderCopy=99\nsourceHolderCopy=77\n" +
                    "type3ExtraAssignments=0\nsceneMutation=none\n",
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
