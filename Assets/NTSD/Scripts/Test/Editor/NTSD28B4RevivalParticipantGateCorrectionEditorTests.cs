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
    [Category("NTSD28_B4")]
    public sealed class NTSD28B4RevivalParticipantGateCorrectionEditorTests
    {
        [TestCase(BattleEcsCharacterFrameTickPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterFrameTickPassMode.Legacy)]
        public void C25Production_UsesPhysicalSlotLivesAndRenderPhaseOnly(
            BattleEcsCharacterFrameTickPassMode mode)
        {
            AssertC25ProductionCase(mode, 0, -1, 1, 2, 1, 30);
            AssertC25ProductionCase(mode, 19, 999, 5, 2, 1, 30);
            AssertC25ProductionCase(mode, 20, 999, 5, 2, 1, 0);
            AssertC25ProductionCase(mode, 0, 999, 5, 1, 1, 0);
        }

        [Test]
        public void LegacyCharacterHook_DoesNotArmRevivalRenderPhase()
        {
            LF2Character character = CreateUnregisteredCharacter(9410);
            character.Health.HP = 0;
            character.HP2Orig = 2;
            character.KillCount = 999;
            character.RelationTeam = 5;
            character.HitStun = 1;
            character.AttackingCounter = 7;

            character.OnFrameTickBeforeWaitAdvance(14);

            Assert.That(character.HitStun, Is.Zero);
            Assert.That(character.AttackingCounter, Is.Zero);
        }

        [Test]
        public void DirectFrameCompatibility_DoesNotArmRevivalRenderPhase()
        {
            LF2Character character = CreateUnregisteredCharacter(9411);
            character.Health.HP = 0;
            character.HP2Orig = 2;
            character.KillCount = 999;
            character.RelationTeam = 5;
            character.HitStun = 1;
            character.AttackingCounter = 7;

            character.SimFrameTick(1);

            Assert.That(character.HitStun, Is.Zero);
            Assert.That(character.AttackingCounter, Is.Zero);
        }

        [TestCase(-1, 1, 1)]
        [TestCase(999, 1, 4)]
        [TestCase(-1, 5, 4)]
        [TestCase(999, 5, 1)]
        public void C07QueuedContinuation_IgnoresKillCountAndBattleGroup(
            int killCount,
            int relationTeam,
            int renderPhase)
        {
            var world = new SimulationWorld();
            LF2Character character = CreateRegisteredCharacter(
                world,
                0,
                9420 + renderPhase + relationTeam);
            ConfigureDeadState14(
                character,
                lives: 1,
                nextHp: 80,
                renderPhase,
                killCount,
                relationTeam);
            int spawnCount = 0;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) =>
            {
                spawnCount++;
                return source;
            });

            world.PostFrameAdvanceDeathCleanupAll(10);

            Assert.That(spawnCount, Is.EqualTo(1));
            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(0));
            Assert.That(character.Frame.N, Is.EqualTo(219));
            Assert.That(character.Health.HP, Is.EqualTo(80));
            Assert.That(character.RespawnCount, Is.Zero);
        }

        [TestCase(0, -1, 1)]
        [TestCase(19, 999, 5)]
        public void C07TerminalPrimary_RemainsForResultHandling(
            int slot,
            int killCount,
            int relationTeam)
        {
            var world = new SimulationWorld();
            LF2Character character = CreateRegisteredCharacter(
                world,
                slot,
                9430 + slot);
            ConfigureDeadState14(
                character,
                lives: 1,
                nextHp: 0,
                renderPhase: 2,
                killCount,
                relationTeam);

            world.PostFrameAdvanceDeathCleanupAll(11);

            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(slot));
            Assert.That(world.FindEntityByRuntimeSlotForQuery(slot),
                Is.SameAs(character));
            Assert.That(character.Frame.N, Is.EqualTo(14));
            Assert.That(character.HP2Orig, Is.EqualTo(1));
            Assert.That(character.HitStun, Is.EqualTo(2));
        }

        [TestCase(-1, 1)]
        [TestCase(999, 5)]
        public void C07TerminalTransient_IsRemovedRegardlessOfLegacyFields(
            int killCount,
            int relationTeam)
        {
            var world = new SimulationWorld();
            LF2Character character = CreateRegisteredCharacter(world, 20, 9460);
            ConfigureDeadState14(
                character,
                lives: 1,
                nextHp: 0,
                renderPhase: 2,
                killCount,
                relationTeam);

            world.PostFrameAdvanceDeathCleanupAll(12);

            Assert.That(world.FindEntityByRuntimeSlotForQuery(20), Is.Null);
            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(-1));
        }

        [TestCase(-1, 1)]
        [TestCase(999, 5)]
        public void C07NormalRevival_LivesTakePriorityOverQueuedHp(
            int killCount,
            int relationTeam)
        {
            var world = new SimulationWorld();
            LF2Character character = CreateRegisteredCharacter(world, 19, 9470);
            ConfigureDeadState14(
                character,
                lives: 2,
                nextHp: 80,
                renderPhase: 2,
                killCount,
                relationTeam);
            character.Health.HP3 = 180;
            int spawnCount = 0;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) =>
            {
                spawnCount++;
                return source;
            });

            world.PostFrameAdvanceDeathCleanupAll(13);

            Assert.That(spawnCount, Is.Zero);
            Assert.That(character.HP2Orig, Is.EqualTo(1));
            Assert.That(character.RespawnCount, Is.EqualTo(80));
            Assert.That(character.Frame.N, Is.EqualTo(212));
            Assert.That(character.Health.HP, Is.EqualTo(180));
            Assert.That(character.HitStun, Is.EqualTo(20));
        }

        [TestCase(0)]
        [TestCase(5)]
        public void C07Entry_RejectsRenderPhaseOutsideOneThroughFour(
            int renderPhase)
        {
            var world = new SimulationWorld();
            LF2Character character = CreateRegisteredCharacter(world, 0, 9480);
            ConfigureDeadState14(
                character,
                lives: 1,
                nextHp: 80,
                renderPhase,
                killCount: 999,
                relationTeam: 5);
            int spawnCount = 0;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, source) =>
            {
                spawnCount++;
                return source;
            });

            world.PostFrameAdvanceDeathCleanupAll(14);

            Assert.That(spawnCount, Is.Zero);
            Assert.That(character.Frame.N, Is.EqualTo(14));
            Assert.That(character.RespawnCount, Is.EqualTo(80));
        }

        private static void AssertC25ProductionCase(
            BattleEcsCharacterFrameTickPassMode mode,
            int slot,
            int killCount,
            int relationTeam,
            int lives,
            int initialRenderPhase,
            int expectedRenderPhase)
        {
            var world = new SimulationWorld();
            world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(mode);
            LF2Character character = CreateRegisteredCharacter(
                world,
                slot,
                9400 + slot + lives);
            ConfigureDeadState14(
                character,
                lives,
                nextHp: 0,
                initialRenderPhase,
                killCount,
                relationTeam);

            world.LateEntityUpdateAll(1);

            Assert.That(
                character.HitStun,
                Is.EqualTo(expectedRenderPhase),
                $"mode={mode}, slot={slot}, kill={killCount}, " +
                $"group={relationTeam}, lives={lives}");
        }

        private static LF2Character CreateRegisteredCharacter(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            LF2Character character = CreateUnregisteredCharacter(objectId);
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            character.RefreshRuntimeSnapshot();
            return character;
        }

        private static LF2Character CreateUnregisteredCharacter(int objectId)
        {
            var data = new LF2CharacterData
            {
                name = "B4RevivalGate_" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(14, LF2States.Lying),
                    Frame(212, LF2States.Jump),
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
            character.Health.HP3 = 500;
            return character;
        }

        private static void ConfigureDeadState14(
            LF2Character character,
            int lives,
            int nextHp,
            int renderPhase,
            int killCount,
            int relationTeam)
        {
            character.DirectWriteFramePreserveWaitCounter(14);
            character.Health.HP = 0;
            character.Health.HPBound = 10;
            character.Health.PP = 77;
            character.HP2Orig = lives;
            character.HPOrig = 6;
            character.RespawnCount = nextHp;
            character.HitStun = renderPhase;
            character.KillCount = killCount;
            character.Team = relationTeam;
            character.RelationTeam = relationTeam;
            character.AttackingCounter = 7;
            character.RefreshRuntimeSnapshot();
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
    internal sealed class NTSD28B4RevivalParticipantGateRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalParticipantGate-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalParticipantGate-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B4RevivalParticipantGateCorrectionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(4096);
        private static NTSD28B4RevivalParticipantGateRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B4RevivalParticipantGateRequestRunner()
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
            activeCallbacks = new NTSD28B4RevivalParticipantGateRequestRunner();
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
    internal static class NTSD28B4RevivalParticipantGatePlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalParticipantGate-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalParticipantGate-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B4RevivalParticipantGatePlayRunner()
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
                    new NTSD28B4RevivalParticipantGateCorrectionEditorTests();
                tests.C25Production_UsesPhysicalSlotLivesAndRenderPhaseOnly(
                    BattleEcsCharacterFrameTickPassMode.DataOriented);
                tests.C25Production_UsesPhysicalSlotLivesAndRenderPhaseOnly(
                    BattleEcsCharacterFrameTickPassMode.Legacy);
                tests.LegacyCharacterHook_DoesNotArmRevivalRenderPhase();
                tests.DirectFrameCompatibility_DoesNotArmRevivalRenderPhase();
                tests.C07QueuedContinuation_IgnoresKillCountAndBattleGroup(-1, 1, 1);
                tests.C07QueuedContinuation_IgnoresKillCountAndBattleGroup(999, 5, 4);
                tests.C07TerminalPrimary_RemainsForResultHandling(0, -1, 1);
                tests.C07TerminalPrimary_RemainsForResultHandling(19, 999, 5);
                tests.C07TerminalTransient_IsRemovedRegardlessOfLegacyFields(-1, 1);
                tests.C07TerminalTransient_IsRemovedRegardlessOfLegacyFields(999, 5);
                tests.C07NormalRevival_LivesTakePriorityOverQueuedHp(-1, 1);
                tests.C07NormalRevival_LivesTakePriorityOverQueuedHp(999, 5);
                tests.C07Entry_RejectsRenderPhaseOutsideOneThroughFour(0);
                tests.C07Entry_RejectsRenderPhaseOutsideOneThroughFour(5);
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\nlogicalCases=20\n" +
                    "c25Profiles=2\nc07Branches=queued,primary-retain," +
                    "transient-free,normal-priority\nsceneMutation=none\n",
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
