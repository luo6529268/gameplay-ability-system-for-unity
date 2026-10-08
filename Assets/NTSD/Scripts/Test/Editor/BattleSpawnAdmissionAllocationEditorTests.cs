#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using NTSD.Animation;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class BattleSpawnAdmissionAllocationEditorTests
    {
        private static readonly MethodInfo AdmissionMethod = typeof(SimulationWorld).Assembly
            .GetType("NTSD.Simulation.Ecs.BattleNativeDirectSpawnWriter", true)
            .GetMethod("IsInitialActionAdmitted", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly Func<LF2CharacterDataWrapper, int, bool> Admit =
            (Func<LF2CharacterDataWrapper, int, bool>)Delegate.CreateDelegate(
                typeof(Func<LF2CharacterDataWrapper, int, bool>), AdmissionMethod);

        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(998, true)]
        [TestCase(999, false)]
        [TestCase(1000, false)]
        [TestCase(int.MaxValue, false)]
        public void AdmissionPreservesNativeActionBounds(int action, bool expected)
        {
            Assert.That(LF2FrameCache.NativeMaxFrameIdExclusive, Is.EqualTo(1000));
            Assert.That(Admit(Wrapper(), action), Is.EqualTo(expected));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MissingWrapperOrDataRejects(bool missingWrapper)
        {
            var wrapper = missingWrapper ? null : new LF2CharacterDataWrapper(1, null);
            Assert.That(Admit(wrapper, 0), Is.False);
            Assert.That(Admit(wrapper, 999), Is.False);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Authored999IsAdmittedAtEveryPosition(int position)
        {
            var wrapper = Wrapper(0, 1, 2);
            wrapper.characterData.frames[position].frameId = 999;
            Assert.That(Admit(wrapper, 999), Is.True);
        }

        [Test]
        public void Authored999TracksDefinitionMutationWithoutCaching()
        {
            var wrapper = Wrapper(0);
            Assert.That(Admit(wrapper, 999), Is.False);
            wrapper.characterData.frames.Add(new LF2FrameData { frameId = 999 });
            Assert.That(Admit(wrapper, 999), Is.True);
            wrapper.characterData.frames.RemoveAt(1);
            Assert.That(Admit(wrapper, 999), Is.False);
        }

        [Test]
        public void Non999AndInvalidActionsDoNotInspectFrames()
        {
            var wrapper = Wrapper();
            wrapper.characterData.frames = null;
            Assert.That(Admit(wrapper, 0), Is.True);
            Assert.That(Admit(wrapper, 998), Is.True);
            Assert.That(Admit(wrapper, -1), Is.False);
            Assert.That(Admit(wrapper, 1000), Is.False);
        }

        [Test]
        public void Authored999PreservesNullListException()
        {
            var wrapper = Wrapper();
            wrapper.characterData.frames = null;
            Assert.Throws<NullReferenceException>(() => Admit(wrapper, 999));
        }

        [Test]
        public void Authored999PreservesNullEntryExceptionBeforeMatch()
        {
            var wrapper = Wrapper();
            wrapper.characterData.frames.Add(null);
            wrapper.characterData.frames.Add(new LF2FrameData { frameId = 999 });
            Assert.Throws<NullReferenceException>(() => Admit(wrapper, 999));
        }

        [Test]
        public void Authored999StopsAtFirstMatchBeforeNull()
        {
            var wrapper = Wrapper(999);
            wrapper.characterData.frames.Add(null);
            Assert.That(Admit(wrapper, 999), Is.True);
        }

        [Test]
        public void AdmissionMatchesLegacyExpressionAcrossNativeRange()
        {
            var wrappers = new[]
            {
                null,
                new LF2CharacterDataWrapper(1, null),
                Wrapper(),
                Wrapper(0, 1, 998),
                Wrapper(0, 999, 998)
            };
            foreach (var wrapper in wrappers)
            {
                for (int action = -1; action <= 1000; action++)
                {
                    Assert.That(Admit(wrapper, action), Is.EqualTo(LegacyAdmission(wrapper, action)),
                        "action=" + action);
                }
            }
        }

        [Test]
        public void AdmissionCompiledIlHasNoManagedConstructionOrDelegate()
        {
            Assert.That(ManagedConstructionOpcodes(AdmissionMethod), Is.Empty,
                "The admission hot method must not construct a closure, delegate, array or boxed value.");
        }

        [Test]
        public void LegacyCapturedExpressionIsPositiveConstructionControl()
        {
            var legacy = typeof(BattleSpawnAdmissionAllocationEditorTests).GetMethod(
                nameof(LegacyAdmission), BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(ManagedConstructionOpcodes(legacy), Does.Contain("newobj"),
                "The IL decoder must detect the known captured-expression allocation source.");
        }

        private static LF2CharacterDataWrapper Wrapper(params int[] frameIds)
        {
            var data = new LF2CharacterData();
            foreach (int frameId in frameIds)
            {
                data.frames.Add(new LF2FrameData { frameId = frameId });
            }
            return new LF2CharacterDataWrapper(1, data);
        }

        private static bool LegacyAdmission(LF2CharacterDataWrapper wrapper, int action)
        {
            return wrapper?.characterData != null && action >= 0 &&
                action < LF2FrameCache.NativeMaxFrameIdExclusive &&
                (action != 999 || wrapper.characterData.frames.Exists(frame => frame.frameId == action));
        }

        internal static bool LoadedAdmissionHasManagedConstruction()
        {
            return ManagedConstructionOpcodes(AdmissionMethod).Count != 0;
        }

        private static List<string> ManagedConstructionOpcodes(MethodInfo method)
        {
            Assert.That(method, Is.Not.Null);
            var body = method.GetMethodBody();
            Assert.That(body, Is.Not.Null);
            byte[] il = body.GetILAsByteArray();
            Assert.That(il, Is.Not.Null);
            var singleByte = new OpCode[256];
            var doubleByte = new OpCode[256];
            foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(OpCode))
                {
                    continue;
                }
                var opcode = (OpCode)field.GetValue(null);
                ushort value = unchecked((ushort)opcode.Value);
                if (value < 256)
                {
                    singleByte[value] = opcode;
                }
                else if ((value & 0xff00) == 0xfe00)
                {
                    doubleByte[value & 0xff] = opcode;
                }
            }

            var constructions = new List<string>();
            int offset = 0;
            while (offset < il.Length)
            {
                byte first = il[offset++];
                OpCode opcode;
                if (first == 0xfe)
                {
                    Assert.That(offset, Is.LessThan(il.Length), "Truncated two-byte opcode.");
                    opcode = doubleByte[il[offset++]];
                }
                else
                {
                    opcode = singleByte[first];
                }
                Assert.That(opcode.Size, Is.GreaterThan(0), "Invalid compiled opcode.");
                if (opcode == OpCodes.Newobj || opcode == OpCodes.Newarr || opcode == OpCodes.Box ||
                    opcode == OpCodes.Ldftn || opcode == OpCodes.Ldvirtftn)
                {
                    constructions.Add(opcode.Name);
                }

                int operandBytes;
                switch (opcode.OperandType)
                {
                    case OperandType.InlineNone:
                        operandBytes = 0;
                        break;
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                        operandBytes = 1;
                        break;
                    case OperandType.InlineVar:
                        operandBytes = 2;
                        break;
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        operandBytes = 8;
                        break;
                    case OperandType.InlineSwitch:
                        Assert.That(offset + 4, Is.LessThanOrEqualTo(il.Length));
                        int count = BitConverter.ToInt32(il, offset);
                        Assert.That(count, Is.GreaterThanOrEqualTo(0));
                        operandBytes = checked(4 + count * 4);
                        break;
                    default:
                        operandBytes = 4;
                        break;
                }
                offset = checked(offset + operandBytes);
                Assert.That(offset, Is.LessThanOrEqualTo(il.Length), "Truncated opcode operand.");
            }
            Assert.That(offset, Is.EqualTo(il.Length));
            return constructions;
        }
    }

    [UnityEditor.InitializeOnLoad]
    internal sealed class BattleSpawnAdmissionRequestRunner : UnityEditor.TestTools.TestRunner.Api.ICallbacks
    {
        private const string OutputRoot =
            "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH41-SPAWN-ADMISSION-ZERO-GC-20261007/";
        private static readonly string[] Phases = { "red-01", "green-01", "green-02", "affected-01" };
        private static BattleSpawnAdmissionRequestRunner active;
        private static UnityEditor.TestTools.TestRunner.Api.TestRunnerApi api;
        private static double nextPoll;
        private readonly string phase;

        static BattleSpawnAdmissionRequestRunner()
        {
            UnityEditor.EditorApplication.update += Poll;
        }

        private BattleSpawnAdmissionRequestRunner(string phase)
        {
            this.phase = phase;
        }

        private static void Poll()
        {
            if (active != null || UnityEditor.EditorApplication.isCompiling ||
                UnityEditor.EditorApplication.isUpdating ||
                UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ||
                UnityEditor.EditorApplication.timeSinceStartup < nextPoll)
            {
                return;
            }
            nextPoll = UnityEditor.EditorApplication.timeSinceStartup + 1;
            var isRunActive = typeof(UnityEditor.TestTools.TestRunner.Api.TestRunnerApi)
                .GetMethod("IsRunActive", BindingFlags.Static | BindingFlags.NonPublic);
            if (isRunActive == null || (bool)isRunActive.Invoke(null, null))
            {
                return;
            }

            foreach (string candidate in Phases)
            {
                string sessionKey = "NTSD-OPT-041-" + candidate;
                if (UnityEditor.SessionState.GetBool(sessionKey, false) ||
                    !System.IO.File.Exists(OutputRoot + candidate + ".request"))
                {
                    continue;
                }
                if (candidate != "red-01" &&
                    BattleSpawnAdmissionAllocationEditorTests.LoadedAdmissionHasManagedConstruction())
                {
                    continue;
                }
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (scene.path != "Assets/NTSD/Scene/NTSD_Menu.unity" ||
                    scene.isDirty || UnityEngine.SceneManagement.SceneManager.sceneCount != 1)
                {
                    return;
                }
                if (System.IO.File.Exists(OutputRoot + candidate + ".xml") ||
                    System.IO.File.Exists(OutputRoot + candidate + "-editor.json"))
                {
                    UnityEditor.SessionState.SetBool(sessionKey, true);
                    return;
                }

                UnityEditor.SessionState.SetBool(sessionKey, true);
                active = new BattleSpawnAdmissionRequestRunner(candidate);
                api = UnityEngine.ScriptableObject.CreateInstance<
                    UnityEditor.TestTools.TestRunner.Api.TestRunnerApi>();
                api.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
                api.RegisterCallbacks(active);
                api.Execute(new UnityEditor.TestTools.TestRunner.Api.ExecutionSettings(
                    new UnityEditor.TestTools.TestRunner.Api.Filter
                    {
                        testMode = UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,
                        testNames = candidate == "affected-01" ? AffectedNames() : new[]
                        {
                            "NTSD.Test.Editor.BattleSpawnAdmissionAllocationEditorTests"
                        }
                    })
                {
                    runSynchronously = false
                });
                return;
            }
        }

        private static string[] AffectedNames()
        {
            return new[]
            {
                "NTSD.Test.Editor.NTSD28C25DefinitionCloneEditorTests.C25b_State9996_UsesExactSynchronizedCallsAndNativeBirthDefaults",
                "NTSD.Test.Editor.NTSD28C25DefinitionCloneEditorTests.C25b_MissingDefinitionsSkipRandom_FullCapacityConsumesAllTuples",
                "NTSD.Test.Editor.NTSD28C25DefinitionCloneEditorTests.C25b_NewbornSlotVisibility_FollowsDynamicAscendingCursor",
                "NTSD.Test.NTSD28Q06State9996DirectSpawnEditorTests.RecycledCloneTaskPreservesOrdinaryOpointBirth",
                "NTSD.Test.NTSD28Q06NativeWeaponPieceEditorTests.PooledTaskClearsNativeBirthFlag"
            };
        }

        public void RunStarted(UnityEditor.TestTools.TestRunner.Api.ITestAdaptor testsToRun) { }
        public void TestStarted(UnityEditor.TestTools.TestRunner.Api.ITestAdaptor test) { }
        public void TestFinished(UnityEditor.TestTools.TestRunner.Api.ITestResultAdaptor result) { }

        public void RunFinished(UnityEditor.TestTools.TestRunner.Api.ITestResultAdaptor result)
        {
            try
            {
                SaveNew(OutputRoot + phase + ".xml", result.ToXml().OuterXml);
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                var evidence = new EditorEvidence
                {
                    phase = phase,
                    scenePath = scene.path,
                    sceneDirty = scene.isDirty,
                    sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount,
                    isPlaying = UnityEditor.EditorApplication.isPlaying,
                    unityVersion = UnityEngine.Application.unityVersion,
                    resultState = result.ResultState,
                    passed = result.PassCount,
                    failed = result.FailCount,
                    skipped = result.SkipCount
                };
                SaveNew(OutputRoot + phase + "-editor.json", UnityEngine.JsonUtility.ToJson(evidence, true));
            }
            finally
            {
                api.UnregisterCallbacks(this);
                UnityEngine.Object.DestroyImmediate(api);
                api = null;
                active = null;
            }
        }

        private static void SaveNew(string path, string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.CreateNew,
                System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        [Serializable]
        private sealed class EditorEvidence
        {
            public string phase;
            public string scenePath;
            public bool sceneDirty;
            public int sceneCount;
            public bool isPlaying;
            public string unityVersion;
            public string resultState;
            public int passed;
            public int failed;
            public int skipped;
        }
    }
}
#endif
