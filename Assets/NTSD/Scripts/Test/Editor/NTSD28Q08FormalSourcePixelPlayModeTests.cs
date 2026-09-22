#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q08FormalSourcePixelPlayModeTests
    {
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string FormalFingerprint =
            "FF1218FF3FEB409FF6B2F8EDB1090591612B3D82D7FA91601E596D29CDF13DFB";
        private const string CaptureName = "formal-source-camera-v2";
        private const int Width = 960;

        [System.Serializable]
        private sealed class PixelReport
        {
            public string contentFingerprint;
            public string atlasMode;
            public int plannedPageCount;
            public long estimatedAtlasBytes;
            public long atlasBudgetBytes;
            public int simulationTick;
            public int centralCommandCount;
            public int nonClearPixels;
            public int width;
            public int height;
        }

        [UnityTest]
        [Explicit("Requires a live Editor world-camera render loop; batchmode does not invoke the registered BattleRenderFeature.")]
        [Timeout(180000)]
        public IEnumerator FormalSourceBindingProducesProductionCameraPixels()
        {
            if (Application.isBatchMode)
                Assert.Ignore("Batchmode did not provide the renderer observation required by CentralOnly output.");
            EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
            yield return new EnterPlayMode();

            CharacterAnimtorManager manager = null;
            BattlePixelFramePlan plan = default;
            Camera camera = null;
            for (int second = 0; second < 120; second++)
            {
                manager = CharacterAnimtorManager.TryGetInstance();
                plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
                camera = NTSDRenderSpace.WorldCamera;
                SimulationTickDriver currentDriver = SimulationTickDriver.Instance;
                if (manager?.PublishedLoganContentIdentity != null && camera != null &&
                    currentDriver?.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    currentDriver.World?.ObjectCount > 0 &&
                    currentDriver.World.BattlePresentation?.PublishedFrame != null)
                    break;
                yield return new WaitForSecondsRealtime(1f);
            }

            Assert.That(GameConfig.Instance?.BattleContentRuntimeRoot, Is.EqualTo(FormalRoot));
            Assert.That(manager?.PublishedLoganContentIdentity, Is.Not.Null);
            Assert.That(manager.PublishedLoganContentIdentity.SemanticFingerprint,
                Is.EqualTo(FormalFingerprint));
            BattleAtlasDiagnosticInputs inputs = manager.LastAtlasDiagnosticInputs;
            Assert.That(inputs, Is.Not.Null);
            Assert.That(inputs.Decision.RequestedMode, Is.EqualTo(BattleAtlasPolicyMode.Auto));
            Assert.That(inputs.Decision.EffectiveMode,
                Is.EqualTo(BattleAtlasPolicyMode.SourceTexture2D));
            Assert.That(inputs.EstimatedAtlasBytes,
                Is.GreaterThan(inputs.Capabilities.AtlasMemoryBudgetBytes));
            Assert.That(camera, Is.Not.Null);
            SimulationWorld world = SimulationTickDriver.Instance?.World;
            Assert.That(world, Is.Not.Null);
            camera.Render();
            plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            if (!plan.IsValid)
            {
                BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
                plan = BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(
                    Time.frameCount);
            }
            Assert.That(plan.IsValid && !plan.IsStale && plan.CapturedFrame != null,
                Is.True, "The queued production frame could not be materialized: " +
                         $"planReason={plan.Reason}; runtimeReason=" +
                         $"{BattleCentralRenderSystem.Diagnostics.RefusalReason}; " +
                         $"featureAvailable={BattleCentralRenderSystem.Diagnostics.FeatureAvailable}; " +
                         $"publishedCommands={world.BattlePresentation.PublishedFrame.CommandCount}.");

            int height = Mathf.Max(1, Mathf.RoundToInt(Width /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            var target = new RenderTexture(Width, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            int previousMask = camera.cullingMask;
            CameraClearFlags previousFlags = camera.clearFlags;
            Color previousColor = camera.backgroundColor;
            bool previousHdr = camera.allowHDR;
            bool previousMsaa = camera.allowMSAA;
            Texture2D readback = null;
            int nonClearPixels;
            string output = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "artifacts", "diagnostics",
                "NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001"));
            Directory.CreateDirectory(output);
            string imagePath = Path.Combine(output, CaptureName + ".png");
            string reportPath = Path.Combine(output, CaptureName + ".json");
            string bodyPassPath = Path.Combine(output, CaptureName + "-body-pass.txt");
            Assert.That(File.Exists(imagePath) || File.Exists(reportPath) || File.Exists(bodyPassPath),
                Is.False, "This witness must preserve prior result files.");
            try
            {
                target.Create();
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.white;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                readback = new Texture2D(Width, height, TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, Width, height), 0, 0, false);
                readback.Apply(false, false);
                nonClearPixels = 0;
                foreach (Color32 pixel in readback.GetPixels32())
                    if (pixel.r != 255 || pixel.g != 255 || pixel.b != 255)
                        nonClearPixels++;
                File.WriteAllBytes(imagePath,
                    readback.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                camera.targetTexture = previousTarget;
                camera.cullingMask = previousMask;
                camera.clearFlags = previousFlags;
                camera.backgroundColor = previousColor;
                camera.allowHDR = previousHdr;
                camera.allowMSAA = previousMsaa;
                if (readback != null)
                    Object.DestroyImmediate(readback);
                target.Release();
                Object.DestroyImmediate(target);
            }

            Assert.That(plan.CapturedFrame.CommandsMaterialized, Is.True);
            Assert.That(plan.CapturedFrame.CommandCount, Is.GreaterThan(0));

            var report = new PixelReport
            {
                contentFingerprint = manager.PublishedLoganContentIdentity.SemanticFingerprint,
                atlasMode = inputs.Decision.EffectiveMode.ToString(),
                plannedPageCount = inputs.PlannedPageCount,
                estimatedAtlasBytes = inputs.EstimatedAtlasBytes,
                atlasBudgetBytes = inputs.Capabilities.AtlasMemoryBudgetBytes,
                simulationTick = plan.SimulationTick,
                centralCommandCount = plan.CapturedFrame.CommandCount,
                nonClearPixels = nonClearPixels,
                width = Width,
                height = height,
            };
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
            Assert.That(nonClearPixels, Is.GreaterThan(0),
                "Formal source bindings produced no central pixels in the production world camera.");
            File.WriteAllText(bodyPassPath,
                $"PASS formalFingerprint={report.contentFingerprint} atlasMode={report.atlasMode} " +
                $"tick={report.simulationTick} commands={report.centralCommandCount} " +
                $"nonClearPixels={report.nonClearPixels}");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28Q08FormalSourcePixelRequestRunner : ICallbacks
    {
        private const string BattleScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string TestName =
            "NTSD.Test.NTSD28Q08FormalSourcePixelPlayModeTests.FormalSourceBindingProducesProductionCameraPixels";
        private static readonly string Root = Directory.GetParent(Application.dataPath).FullName;
        private static readonly string RequestPath = Path.Combine(
            Root, "Temp/NTSD28_Q08_FormalSourcePixel.request.json");
        private static readonly string ResultPath = Path.Combine(
            Root, "artifacts/diagnostics/NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001/original-editor-test-result.txt");
        private static TestRunnerApi activeApi;
        private static NTSD28Q08FormalSourcePixelRequestRunner activeCallbacks;
        private static readonly System.Text.StringBuilder Failures =
            new System.Text.StringBuilder(2048);

        [System.Serializable]
        private sealed class Request
        {
            public bool requested;
        }

        static NTSD28Q08FormalSourcePixelRequestRunner()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (activeApi != null || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
                return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
            if (request == null || !request.requested)
                return;
            request.requested = false;
            File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
            Directory.CreateDirectory(Path.GetDirectoryName(ResultPath));
            if (File.Exists(ResultPath))
                return;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != BattleScenePath || scene.isDirty)
            {
                File.WriteAllText(ResultPath,
                    $"state=PRECONDITION_FAILED scene={scene.path} dirty={scene.isDirty}");
                return;
            }
            activeCallbacks = new NTSD28Q08FormalSourcePixelRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            activeApi.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[] { TestName },
            })
            {
                runSynchronously = false,
            });
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            Failures.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string summary =
                $"state={result.ResultState}\npassed={result.PassCount}\n" +
                $"failed={result.FailCount}\nskipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\nmessage={result.Message}\n" +
                Failures;
            File.WriteAllText(ResultPath, summary);
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
            Failures.Append("test=").Append(result.FullName).Append('\n')
                .Append("state=").Append(result.ResultState).Append('\n')
                .Append("message=").Append(result.Message).Append('\n')
                .Append("stack=").Append(result.StackTrace).Append('\n');
        }
    }
}
#endif
