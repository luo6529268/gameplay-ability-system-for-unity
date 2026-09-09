#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B3")]
    public sealed class NTSD28B3State501RetirementEditorTests
    {
        [Test]
        public void State501SelfAndChildrenRemainBitwiseInert()
        {
            new NTSD.Test.EarlyFrameAdvanceOptimizationEditorTests()
                .State501SelfAndChildren_AreInertAcrossFastAndForcedLegacy();
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B3State501RetirementRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B3-State501-Retirement-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B3-State501-Retirement-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B3State501RetirementEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B3State501RetirementRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B3State501RetirementRequestRunner()
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
            activeCallbacks = new NTSD28B3State501RetirementRequestRunner();
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
    internal static class NTSD28B3State501RetirementPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B3-State501-Retirement-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B3-State501-Retirement-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B3State501RetirementPlayRunner()
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
                new NTSD28B3State501RetirementEditorTests()
                    .State501SelfAndChildrenRemainBitwiseInert();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=1\nfastAndLegacy=true\n" +
                    "entitiesUnchanged=10\nsceneMutation=none\n",
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
