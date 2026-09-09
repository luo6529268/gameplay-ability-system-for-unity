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
    public sealed class NTSD28B5InputHpCostCompatSharedTransactionEditorTests
    {
        [TestCase(BattleAiExecutionProfile.LegacyCanonical)]
        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical)]
        public void RegisteredProfiles_UseExactSharedTransaction(
            BattleAiExecutionProfile profile)
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(profile);
            LF2Character character = CreateCharacter();
            world.Register(character);

            ArrangeExactCostCase(character);
            bool applied = character.TryCharacterDatInputFrameJump(-20);

            AssertExactCostResult(character, applied, profile.ToString());
        }

        [Test]
        public void UnregisteredCompatibility_UsesExactSharedTransaction()
        {
            LF2Character character = CreateCharacter();
            ArrangeExactCostCase(character);

            bool applied = character.TryCharacterDatInputFrameJump(-20);

            AssertExactCostResult(character, applied, "unregistered");
        }

        [Test]
        public void RegisteredLegacy_FallbackAndGuardsMatchExactTransaction()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.LegacyCanonical);
            LF2Character character = CreateCharacter();
            world.Register(character);
            character.ComboCountVic = 31;
            character.Runtime.InputHpConsumedTotal34C = 5;
            character.Runtime.InputMpConsumedTotal350 = 7;
            character.Runtime.InputLastAction144 = 333;
            character.Runtime.InputSpecialGate194 = 71;
            character.Health.HP = 100;
            character.Health.HPBound = 100;
            character.Health.PP = 0;
            character.SwitchDir("right");

            bool fallback = character.TryCharacterDatInputFrameJump(-30);

            Assert.That(fallback, Is.True);
            Assert.That(character.Frame.N, Is.EqualTo(71));
            Assert.That(character.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(character.Health.HP, Is.EqualTo(100));
            Assert.That(character.Health.HPBound, Is.EqualTo(100));
            Assert.That(character.Health.PP, Is.Zero);
            Assert.That(character.Runtime.InputHpConsumedTotal34C, Is.EqualTo(5));
            Assert.That(character.Runtime.InputMpConsumedTotal350, Is.EqualTo(7));
            Assert.That(character.Runtime.InputLastAction144, Is.EqualTo(333));
            Assert.That(character.ComboCountVic, Is.EqualTo(31));

            SetCurrentFrame(character, 0);
            character.Runtime.InputActionLock130 = 1;
            Assert.That(character.TryCharacterDatInputFrameJump(20), Is.False);
            Assert.That(character.Frame.N, Is.Zero);
            character.Runtime.InputActionLock130 = 0;
            Assert.That(character.TryCharacterDatInputFrameJump(404), Is.False);
            Assert.That(character.TryCharacterDatInputFrameJump(999), Is.True);
            Assert.That(character.Frame.N, Is.Zero);
            Assert.That(character.Runtime.InputLastAction144, Is.Zero);
        }

        [Test]
        public void UnregisteredSharedTransaction_AllocatesZeroAfterWarmup()
        {
            LF2Character character = CreateCharacter();
            character.Runtime.InputLocalResourceEnabled49D034 = false;
            Assert.That(character.TryCharacterDatInputFrameJump(20), Is.True);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool applied = true;
            for (int index = 0; index < 4096; index++)
                applied &= character.TryCharacterDatInputFrameJump(20);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(applied, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void ProductionSources_ContainOneExactCoreAndNoLegacyHpCostWriter()
        {
            string writer = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs");
            string entity = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs");
            string character = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs");
            string compatibility = Slice(
                entity,
                "internal bool TryCharacterDatInputFrameJumpCompatibility(int frameId)",
                "internal bool TryResolveLateN30InputTriggerCode(out int frameVal)");
            string deadAlias = Slice(
                character,
                "internal bool TryInputFrameJump(int frameId)",
                "public int CurrentFrameId => Frame.N;");

            Assert.That(Count(entity, "ComboCountVic += hpCost"), Is.Zero);
            Assert.That(Count(character, "ComboCountVic += hpCost"), Is.Zero);
            Assert.That(Count(writer, "InputHpConsumedTotal34C += hpCost"),
                Is.EqualTo(1));
            StringAssert.Contains("ApplyNativeInputAction", compatibility);
            StringAssert.DoesNotContain("Health.HP -=", compatibility);
            StringAssert.DoesNotContain("SpendPpDisplay", compatibility);
            StringAssert.DoesNotContain("new BattleCharacterActionWriter", compatibility);
            StringAssert.Contains("TryCharacterDatInputFrameJump", deadAlias);
            StringAssert.DoesNotContain("Health.HP -=", deadAlias);
        }

        private static void ArrangeExactCostCase(LF2Character character)
        {
            character.FrameCache.Wrapper.characterData.recmp = 50;
            character.Health.HP = 100;
            character.Health.HPBound = 100;
            character.Health.PP = 30;
            character.Runtime.InputModeCostMultiplier30 = 25;
            character.Runtime.InputDoubleCost19C = 1;
            character.Runtime.InputHpConsumedTotal34C = 5;
            character.Runtime.InputMpConsumedTotal350 = 7;
            character.Runtime.InputLastAction144 = 333;
            character.ComboCountVic = 31;
            character.SwitchDir("right");
        }

        private static void AssertExactCostResult(
            LF2Character character,
            bool applied,
            string label)
        {
            Assert.That(applied, Is.True, label);
            Assert.That(character.Frame.N, Is.EqualTo(21), label);
            Assert.That(character.Runtime.Dir, Is.EqualTo("left"), label);
            Assert.That(character.Health.PP, Is.EqualTo(6), label);
            Assert.That(character.Health.HP, Is.EqualTo(71), label);
            Assert.That(character.Health.HPBound, Is.EqualTo(97), label);
            Assert.That(character.Runtime.InputMpConsumedTotal350,
                Is.EqualTo(31), label);
            Assert.That(character.Runtime.InputHpConsumedTotal34C,
                Is.EqualTo(34), label);
            Assert.That(character.Runtime.InputLastAction144, Is.EqualTo(21), label);
            Assert.That(character.ComboCountVic, Is.EqualTo(31), label);
        }

        private static LF2Character CreateCharacter()
        {
            var data = new LF2CharacterData
            {
                name = "B5InputCompatSharedTransaction",
                type_sub = (int)LF2ObjectType.Character,
                recmp = 50,
                frames = new List<LF2FrameData>
                {
                    Frame(0),
                    Frame(20, state: 1000021, mp: 2025, hp: 9),
                    Frame(21),
                    Frame(30, mp: 25),
                    Frame(71),
                },
            };
            var character = new LF2Character
            {
                Name = data.name,
                ObjectId = 9970,
            };
            character.ModuleInitialize();
            character.SetRequiredRuntimeSlot(0);
            character.FrameCache.Load(new LF2CharacterDataWrapper(9970, data));
            character.ImmediateFrame(0);
            character.Initialize(500, 500);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.SetPosition(0, 0, 0);
            character.Runtime.SyncIntegerPosition();
            return character;
        }

        private static LF2FrameData Frame(
            int frameId,
            int state = 0,
            int mp = 0,
            int hp = 0)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
                pic = 999,
                mp = mp,
                hp = hp,
            };
        }

        private static void SetCurrentFrame(LF2Character character, int frameId)
        {
            character.WriteCurrentFrameId(frameId);
            character.Frame.D = character.FrameCache.GetFrameDataById(frameId);
            character.Frame.PN = frameId;
            character.Runtime.NextFrame = character.Frame.D?.next ?? 0;
            character.Trans.SyncDirectFrameData(
                character.Frame.D?.wait ?? 0,
                character.Runtime.NextFrame,
                0);
        }

        private static string Source(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return File.ReadAllText(Path.GetFullPath(
                Path.Combine(root ?? string.Empty, relativePath)));
        }

        private static string Slice(string source, string start, string end)
        {
            int startIndex = source.IndexOf(start, StringComparison.Ordinal);
            int endIndex = source.IndexOf(end, startIndex + start.Length,
                StringComparison.Ordinal);
            Assert.That(startIndex, Is.GreaterThanOrEqualTo(0), start);
            Assert.That(endIndex, Is.GreaterThan(startIndex), end);
            return source.Substring(startIndex, endIndex - startIndex);
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(token, offset,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }
            return count;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B5InputHpCostCompatSharedTransactionRequestRunner :
        ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-InputHpCostCompatSharedTransaction-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-InputHpCostCompatSharedTransaction-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B5InputHpCostCompatSharedTransactionEditorTests";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(4096);
        private static NTSD28B5InputHpCostCompatSharedTransactionRequestRunner
            activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B5InputHpCostCompatSharedTransactionRequestRunner()
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
            if (File.Exists(ResultPath)) File.Delete(ResultPath);
            File.Delete(RequestPath);
            activeCallbacks =
                new NTSD28B5InputHpCostCompatSharedTransactionRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            activeApi.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[] { FocusedTestClass },
            }) { runSynchronously = false });
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            FailureDetails.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string text = $"state={result.ResultState}\npassed={result.PassCount}\n" +
                $"failed={result.FailCount}\nskipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\nmessage={result.Message}\n" +
                FailureDetails;
            File.WriteAllText(ResultPath, text, new UTF8Encoding(false));
            activeApi.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
        }

        public void TestStarted(ITestAdaptor test) { }

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
    internal static class NTSD28B5InputHpCostCompatSharedTransactionPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-InputHpCostCompatSharedTransaction-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-InputHpCostCompatSharedTransaction-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B5InputHpCostCompatSharedTransactionPlayRunner()
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
                var tests =
                    new NTSD28B5InputHpCostCompatSharedTransactionEditorTests();
                tests.RegisteredProfiles_UseExactSharedTransaction(
                    BattleAiExecutionProfile.LegacyCanonical);
                tests.RegisteredProfiles_UseExactSharedTransaction(
                    BattleAiExecutionProfile.DataOrientedCanonical);
                tests.UnregisteredCompatibility_UsesExactSharedTransaction();
                tests.RegisteredLegacy_FallbackAndGuardsMatchExactTransaction();
                tests.UnregisteredSharedTransaction_AllocatesZeroAfterWarmup();
                tests.ProductionSources_ContainOneExactCoreAndNoLegacyHpCostWriter();
                File.WriteAllText(ResultPath,
                    "state=Passed\ncases=6\n" +
                    "profiles=LegacyCanonical,DataOrientedCanonical,unregistered\n" +
                    "exactTransaction=shared\nlegacyCombo=preserved\n" +
                    "sceneMutation=none\n",
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                File.WriteAllText(ResultPath, "state=Failed\n" + exception,
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
