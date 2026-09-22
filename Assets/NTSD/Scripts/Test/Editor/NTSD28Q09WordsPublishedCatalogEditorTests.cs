#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q09WordsPublishedCatalogEditorTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator FormalWordsPublishSixSheetsThroughTheProductionCatalog()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var publication = new NTSD28B11AtomicPublicationEditorTests();
                var managerSingleton = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", PrivateStatic);
                var dataSingleton = typeof(MoreMountains.Tools.MMSingleton<GameDataManager>)
                    .GetField("_instance", PrivateStatic);
                object previousManager = managerSingleton.GetValue(null);
                object previousData = dataSingleton.GetValue(null);
                CharacterAnimtorManager manager = null;
                bool initialized = false;
                try
                {
                    publication.SetUp();
                    initialized = true;
                    Type fixtureType = typeof(NTSD28B11AtomicPublicationEditorTests);
                    manager = (CharacterAnimtorManager)fixtureType.GetField("manager", PrivateInstance)
                        .GetValue(publication);
                    var data = (GameDataManager)fixtureType.GetField("data", PrivateInstance)
                        .GetValue(publication);
                    managerSingleton.SetValue(null, manager);
                    dataSingleton.SetValue(null, data);

                    string root = CreateFixtureWithFormalWords();
                    var candidate = LoganVisualContentCandidate.Capture(
                        BattleContentSource.ForLoganRuntime(root));
                    Assert.That(candidate.WordsInput, Is.Not.Null);
                    var load = (UniTask<bool>)fixtureType.GetMethod("Load", PrivateInstance)
                        .Invoke(publication, new object[] { candidate, null });
                    Assert.That(await load, Is.True);

                    BattleCommonVisualCatalog catalog = manager.CommonVisualCatalog;
                    Assert.That(catalog.IsComplete, Is.True, catalog.Diagnostic);
                    Assert.That(catalog.WordTextures.Count, Is.EqualTo(6));
                    for (int sheet = 0; sheet < 6; sheet++)
                    {
                        Texture2D texture = catalog.WordTextures[sheet];
                        Assert.That(texture.width, Is.EqualTo(251));
                        Assert.That(texture.height, Is.EqualTo(257));
                        Assert.That(catalog.TryGetWordGlyph(sheet, 'A', out var binding), Is.True);
                        Assert.That(binding.Sprite.texture, Is.SameAs(texture));
                        Assert.That(binding.Sprite.rect,
                            Is.EqualTo(BattleCommonVisualCatalog.GetWordGlyphPixelRect('A')));
                        Assert.That(binding.CentralBinding.IsValid, Is.True,
                            $"WORDS{sheet} glyph did not reach the central renderer binding.");
                    }
                }
                finally
                {
                    try
                    {
                        if (manager != null && !(bool)typeof(CharacterAnimtorManager)
                            .GetField("spritePrewarmDisposed", PrivateInstance).GetValue(manager))
                            typeof(CharacterAnimtorManager).GetMethod("OnDestroy", PrivateInstance)
                                .Invoke(manager, null);
                    }
                    finally
                    {
                        try { if (initialized) publication.TearDown(); }
                        finally
                        {
                            managerSingleton.SetValue(null, previousManager);
                            dataSingleton.SetValue(null, previousData);
                        }
                    }
                }
            });
        }

        private static string CreateFixtureWithFormalWords()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string formalRoot = Path.Combine(projectRoot, "Assets/NTSD/Content/LoganRuntime");
            string root = Path.Combine(projectRoot, "Temp/NTSD28Q09WordsPublication",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat/data"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/c"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/sprite/UI"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\n" +
                "object,0,56,0,a.dat,missing\n");
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"),
                "<bmp_begin>\nname: words\nhead: c/head.png\nsmall: c/small.png\n" +
                "file(20-19): c/body.png w: 5 h: 1 row: 1 col: 1\n<bmp_end>\n" +
                "<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n");

            var body = new Texture2D(6, 2, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[12];
                for (int index = 0; index < pixels.Length; index++)
                    pixels[index] = new Color32(0, 255, 0, 255);
                body.SetPixels32(pixels);
                body.Apply();
                byte[] bytes = body.EncodeToPNG();
                foreach (string name in new[] { "body", "head", "small" })
                    File.WriteAllBytes(Path.Combine(root, "vfs/c", name + ".png"), bytes);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(body);
            }

            File.Copy(Path.Combine(formalRoot, "decoded_dat/data/resource.dat"),
                Path.Combine(root, "decoded_dat/data/resource.dat"));
            for (int sheet = 0; sheet < BattleCommonVisualCatalog.WordSheetCount; sheet++)
            {
                string name = "WORDS" + sheet + ".png";
                File.Copy(Path.Combine(formalRoot, "vfs/sprite/UI", name),
                    Path.Combine(root, "vfs/sprite/UI", name));
            }
            return root;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28Q09WordsPublicationRequestRunner : ICallbacks
    {
        private const string TestName =
            "NTSD.Test.NTSD28Q09WordsPublishedCatalogEditorTests.FormalWordsPublishSixSheetsThroughTheProductionCatalog";
        private static readonly string Root = Directory.GetParent(Application.dataPath).FullName;
        private static readonly string RequestPath = Path.Combine(
            Root, "Temp/NTSD28_Q09_WordsPublication.request.json");
        private static readonly string ResultPath = Path.Combine(
            Root, "artifacts/diagnostics/NTSD28-Q09-WORDS-PUBLICATION-001/original-editor-test-result.txt");
        private static TestRunnerApi activeApi;
        private static NTSD28Q09WordsPublicationRequestRunner activeCallbacks;
        private static readonly System.Text.StringBuilder Failures =
            new System.Text.StringBuilder(2048);

        [Serializable]
        private sealed class Request
        {
            public bool requested;
        }

        static NTSD28Q09WordsPublicationRequestRunner()
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
            activeCallbacks = new NTSD28Q09WordsPublicationRequestRunner();
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
            File.WriteAllText(ResultPath,
                $"state={result.ResultState}\npassed={result.PassCount}\n" +
                $"failed={result.FailCount}\nskipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\nmessage={result.Message}\n" +
                Failures);
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
            Failures.Append("test=").Append(result.FullName).Append('\n')
                .Append("state=").Append(result.ResultState).Append('\n')
                .Append("message=").Append(result.Message).Append('\n')
                .Append("stack=").Append(result.StackTrace).Append('\n');
        }
    }
}
#endif
