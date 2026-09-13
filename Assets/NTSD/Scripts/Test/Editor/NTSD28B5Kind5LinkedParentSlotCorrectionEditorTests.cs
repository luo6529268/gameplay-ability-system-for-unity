#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
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
    public sealed class NTSD28B5Kind5LinkedParentSlotCorrectionEditorTests
    {
        [TestCase(-1, 0)]
        [TestCase(0, 0)]
        [TestCase(77, 77)]
        public void LinkedHolderHelpers_UseExactParentAndNeverHolderCopy(
            int holderStableId,
            int expectedSlot)
        {
            TypedCharacter attacker = CreateEntity(
                "B5LinkedHelper",
                9700,
                LF2ObjectType.LightWeapon,
                Frame());
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = holderStableId;

            Assert.That(
                attacker.ResolveReleaseNeutralHolderSlotOrImplicitZero(),
                Is.EqualTo(expectedSlot));
            Assert.That(
                attacker.ResolveReleaseNegativeLinkHolderSlotOrImplicitZero(),
                Is.EqualTo(expectedSlot));
            Assert.That(typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0, Is.True);
        }

        [Test]
        public void FrozenPair_UsesExactHighSlotHolderGroupNotRootCopyGroup()
        {
            var world = new SimulationWorld();
            TypedCharacter rootCopy = Register(
                world,
                CreateEntity("B5RootCopy", 9710, LF2ObjectType.Character, Frame()),
                5,
                15);
            TypedCharacter exactHolder = Register(
                world,
                CreateEntity("B5ExactHolder", 9711, LF2ObjectType.Character, Frame()),
                77,
                17);
            TypedCharacter attacker = Register(
                world,
                CreateEntity("B5HeldAttacker", 9712, LF2ObjectType.LightWeapon, Frame()),
                50,
                1);
            TypedCharacter target = Register(
                world,
                CreateEntity("B5Target", 9713, LF2ObjectType.Character, Frame()),
                51,
                2);
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = exactHolder.Runtime.SlotIndex;

            BattleHitCandidatePairSnapshot snapshot =
                BattleHitCandidatePairSnapshotFactory.Capture(
                    attacker,
                    target,
                    world);

            Assert.That(snapshot.Valid, Is.True);
            Assert.That(snapshot.LinkedHolderPresent, Is.True);
            Assert.That(snapshot.LinkedHolderBattleGroup, Is.EqualTo(17));
            Assert.That(typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0, Is.True);
        }

        [Test]
        public void Kind5Replacement_UsesExactHolderFrameNotRootCopyFrame()
        {
            var world = new SimulationWorld();
            TypedCharacter rootCopy = Register(
                world,
                CreateHolder("B5RootCopyFrame", 9720, replacementInjury: 33),
                5,
                15);
            TypedCharacter exactHolder = Register(
                world,
                CreateHolder("B5ExactHolderFrame", 9721, replacementInjury: 77),
                77,
                17);
            InteractionArea sourceItr = new InteractionArea
            {
                kind = 5,
                injury = 11,
                arest = 2,
                vrest = 3,
            };
            TypedCharacter attacker = Register(
                world,
                CreateEntity(
                    "B5Kind5Held",
                    9722,
                    LF2ObjectType.LightWeapon,
                    Frame(sourceItr)),
                50,
                1);
            TypedCharacter target = Register(
                world,
                CreateEntity("B5Kind5Target", 9723, LF2ObjectType.Character, Frame()),
                51,
                2);
            rootCopy.Runtime.TargetSlotIndex = attacker.Runtime.SlotIndex;
            exactHolder.Runtime.TargetSlotIndex = attacker.Runtime.SlotIndex;
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = exactHolder.Runtime.SlotIndex;

            InteractionArea resolved = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                attacker.GetCollisionFrameData(),
                sourceItr,
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);

            Assert.That(resolved, Is.Not.SameAs(sourceItr));
            Assert.That(resolved.kind, Is.Zero);
            Assert.That(resolved.injury, Is.EqualTo(77));
            Assert.That(zeroAttackerHpOnConsume, Is.False);
            Assert.That(releaseHeavyHeldTargetOnConsume, Is.False);
            Assert.That(typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0, Is.True);
        }

        private static TypedCharacter CreateHolder(
            string name,
            int objectId,
            int replacementInjury)
        {
            LF2FrameData frame = Frame(
                new InteractionArea { kind = 0, injury = 1 },
                new InteractionArea
                {
                    kind = 0,
                    injury = replacementInjury,
                    fall = 40,
                    bdefend = 50,
                    arest = 6,
                    vrest = 7,
                });
            frame.wpoints.Add(new WeaponPoint { attacking = 1 });
            return CreateEntity(name, objectId, LF2ObjectType.Character, frame);
        }

        private static LF2FrameData Frame(params InteractionArea[] itrs)
        {
            return new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 1000,
                next = 0,
                pic = 999,
                centerx = 39,
                centery = 79,
                itrs = new List<InteractionArea>(
                    itrs ?? Array.Empty<InteractionArea>()),
                wpoints = new List<WeaponPoint>(),
            };
        }

        private static TypedCharacter CreateEntity(
            string name,
            int objectId,
            LF2ObjectType type,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = (int)type,
                frames = new List<LF2FrameData> { frame },
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
            entity.Runtime.SetPosition(slot * 10, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
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
    internal sealed class NTSD28B5Kind5LinkedParentSlotRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-Kind5LinkedParent-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-Kind5LinkedParent-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B5Kind5LinkedParentSlotCorrectionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails =
            new StringBuilder(4096);
        private static NTSD28B5Kind5LinkedParentSlotRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B5Kind5LinkedParentSlotRequestRunner()
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
            activeCallbacks = new NTSD28B5Kind5LinkedParentSlotRequestRunner();
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
    internal static class NTSD28B5Kind5LinkedParentSlotPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-Kind5LinkedParent-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-Kind5LinkedParent-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B5Kind5LinkedParentSlotPlayRunner()
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
                    new NTSD28B5Kind5LinkedParentSlotCorrectionEditorTests();
                tests.LinkedHolderHelpers_UseExactParentAndNeverHolderCopy(-1, 0);
                tests.LinkedHolderHelpers_UseExactParentAndNeverHolderCopy(0, 0);
                tests.LinkedHolderHelpers_UseExactParentAndNeverHolderCopy(77, 77);
                tests.FrozenPair_UsesExactHighSlotHolderGroupNotRootCopyGroup();
                tests.Kind5Replacement_UsesExactHolderFrameNotRootCopyFrame();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=5\n" +
                    "exactSlots=implicit0,slot0,slot77\n" +
                    "holderCopyCarrierAbsent=" + (typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0).ToString() + "\nsceneMutation=none\n",
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
