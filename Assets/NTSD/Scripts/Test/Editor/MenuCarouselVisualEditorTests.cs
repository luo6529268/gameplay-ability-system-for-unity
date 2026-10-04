#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using NTSD.UI.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class MenuCarouselVisualEditorTests
    {
        private const string Evidence = "artifacts/diagnostics/NTSD-MENU-CAROUSEL-VISUAL-001/";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string MenuHashKey = "NTSD-MENU-CAROUSEL-VISUAL-001.MenuHash";
        private const string BattleHashKey = "NTSD-MENU-CAROUSEL-VISUAL-001.BattleHash";

        [UnityEditor.MenuItem("NTSD/UI/Create Menu Carousel Defaults")]
        public static void CreateDefaultProfile()
        {
            const string path = "Assets/NTSD/Resources/UI/MenuCarouselStyle.asset";
            if (File.Exists(path)) return;
            var profile = ScriptableObject.CreateInstance<MenuCarouselStyle>();
            var serialized = new UnityEditor.SerializedObject(profile);
            serialized.FindProperty("fallbackFont").objectReferenceValue =
                UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(
                    "121a7e85a61f6a7498e2956216768afa"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.AssetDatabase.CreateAsset(profile, path);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(profile);
        }

        [Test]
        public void ShaderSupportsIndependentOutlineShadowAndHasNoErrors()
        {
            var shader = Resources.Load<Shader>("UI/MenuCarouselText");
            Assert.IsNotNull(shader);
            Assert.IsTrue(shader.isSupported);
            Assert.IsFalse(UnityEditor.ShaderUtil.GetShaderMessages(shader).Any(
                message => message.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error));
            var material = new Material(shader);
            try
            {
                Assert.IsTrue(material.HasProperty("_OutlineColor"));
                Assert.IsTrue(material.HasProperty("_UnderlayColor"));
                Assert.IsTrue(material.HasProperty("_OutlineSoftness"));
                Assert.IsTrue(material.HasProperty("_Stencil"));
            }
            finally { Object.DestroyImmediate(material); }
        }

        [Test]
        public void CenterEdgeAndOtherLabelsHaveIndependentMaterialsAndRestore()
        {
            var root = new GameObject("CarouselTextFixture");
            try
            {
                var first = NewLabel(root, "First");
                var second = NewLabel(root, "Second");
                var source = first.fontSharedMaterial;
                second.fontSharedMaterial = source;
                Color originalColor = first.color;
                float sourceWidth = source.GetFloat("_OutlineWidth");
                var effect = first.gameObject.AddComponent<MenuCarouselTextEffect>();
                effect.Acquire();
                effect.Apply(0, 0);
                Assert.AreEqual(Color.white, first.color);
                Assert.Greater(first.transform.localScale.x, 1.8f);
                Assert.AreNotSame(source, first.fontSharedMaterial);
                Assert.AreSame(source, second.fontSharedMaterial);
                Color outline = first.fontSharedMaterial.GetColor("_OutlineColor");
                Assert.Greater(outline.r, outline.g * 5);
                Assert.AreEqual(0f, first.fontSharedMaterial.GetColor("_UnderlayColor").r);
                float centerSoftness = first.fontSharedMaterial.GetFloat("_OutlineSoftness");
                effect.Apply(2.5f, 0.8f);
                Assert.Less(first.color.a, 0.5f);
                Assert.Less(first.transform.localScale.x, 1f);
                Assert.Greater(first.fontSharedMaterial.GetFloat("_OutlineSoftness"), centerSoftness);
                effect.Apply(3.5f, 1.1f);
                Assert.AreEqual(0, first.color.a);
                Assert.AreEqual(sourceWidth, source.GetFloat("_OutlineWidth"));
                effect.Release();
                Assert.AreSame(source, first.fontSharedMaterial);
                Assert.AreEqual(originalColor, first.color);
                Assert.AreEqual(Vector3.one, first.transform.localScale);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void ReacquireDoesNotCompoundScaleOrRetainOwnedPrimaryMaterials()
        {
            var root = new GameObject("CarouselReopenFixture");
            try
            {
                var label = NewLabel(root, "Text");
                label.transform.localScale = new Vector3(1.2f, 0.9f, 1f);
                var baseline = label.transform.localScale;
                var original = label.fontSharedMaterial;
                var effect = label.gameObject.AddComponent<MenuCarouselTextEffect>();
                for (int turn = 0; turn < 10; turn++)
                {
                    effect.Acquire();
                    effect.Apply(0, 0);
                    Assert.AreEqual(baseline.x * 1.9f, label.transform.localScale.x, 0.001f);
                    var owned = label.fontSharedMaterial;
                    effect.Release();
                    Assert.IsTrue(owned == null);
                    Assert.AreSame(original, label.fontSharedMaterial);
                    Assert.AreEqual(baseline, label.transform.localScale);
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void RuntimeFallbackPopulatesMissingGlyphsAndReleasesLastBorrower()
        {
            var style = Resources.Load<MenuCarouselStyle>("UI/MenuCarouselStyle");
            Assert.IsNotNull(style);
            var source = style.FallbackFont;
            var sourceMaterial = source.material;
            int sourceGlyphs = source.characterTable.Count;
            var first = style.AcquireFallback("闯对决赛战退出");
            var second = style.AcquireFallback("赛战退出");
            var atlas = first.atlasTextures[0];
            var material = first.material;
            try
            {
                Assert.AreSame(first, second);
                Assert.AreNotSame(source, first);
                foreach (char character in "闯对决赛战退出")
                    Assert.IsTrue(first.HasCharacter(character));
                style.ReleaseFallback();
                Assert.IsTrue(first != null);
                style.ReleaseFallback();
                Assert.IsTrue(first == null);
                Assert.IsTrue(atlas == null);
                Assert.IsTrue(material == null);
                Assert.AreSame(sourceMaterial, source.material);
                Assert.AreEqual(sourceGlyphs, source.characterTable.Count);
            }
            finally
            {
                style.ReleaseFallback();
                style.ReleaseFallback();
            }
        }

        private static TextMeshProUGUI NewLabel(GameObject parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent.transform, false);
            var label = child.AddComponent<TextMeshProUGUI>();
            label.font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                UnityEditor.AssetDatabase.GUIDToAssetPath("c10a07540530f01408fcb83191a6f0f3"));
            Assert.IsNotNull(label.font);
            label.text = "1";
            label.fontSize = 50;
            return label;
        }

        [UnityTest]
        public IEnumerator OriginalMenu_VisualsWrapFallbackMaterialsAndReopen()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Assert.IsFalse(scene.isDirty);
                Assert.AreEqual(0, scene.rootCount, "Only open Menu from the empty Test Runner scene.");
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(MenuScene);
            }
            Assert.AreEqual(MenuScene, UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
            Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
            UnityEditor.SessionState.SetString(MenuHashKey, Hash(MenuScene));
            UnityEditor.SessionState.SetString(BattleHashKey, Hash("Assets/NTSD/Scene/NTSD_Battle.unity"));
            Directory.CreateDirectory(Evidence);
            yield return new EnterPlayMode();
            for (int i = 0; i < 12; i++) yield return null;
            NTSD.UI.MenuUIController.Instance.ShowSelectGameMode();
            yield return null;
            var carousel = Object.FindObjectOfType<MenuLoopCarousel>();
            var list = carousel.GetComponentInParent<MenuOptionList>();
            Assert.AreEqual(7, list.OptionCount);
            var originalLabels = new TextMeshProUGUI[7];
            for (int i = 0; i < 7; i++) originalLabels[i] = list.GetOption(i).GetComponent<TextMeshProUGUI>();
            var center = originalLabels[list.CurrentIndex];
            var neighbor = originalLabels[(list.CurrentIndex + 1) % 7];
            Assert.Greater(center.transform.localScale.x, neighbor.transform.localScale.x * 1.5f);
            Assert.AreEqual(Color.white, center.color);
            Assert.Less(neighbor.color.r, center.color.r);
            foreach (var label in originalLabels)
            {
                label.ForceMeshUpdate();
                foreach (char character in label.text)
                    Assert.IsTrue(label.font.HasCharacter(character, searchFallbacks: true),
                        "Missing mode glyph: " + character);
                foreach (var material in label.fontSharedMaterials)
                    Assert.AreEqual("NTSD/UI/Menu Carousel Text", material.shader.name);
            }
            string firstShot = CaptureNew("center-vs.png");
            for (int frame = 0; frame < 120 && !File.Exists(firstShot); frame++) yield return null;
            Assert.IsTrue(File.Exists(firstShot));
            for (int i = 0; i < 7; i++) carousel.Navigate(1);
            yield return new WaitForSecondsRealtime(1f);
            Assert.AreEqual(0, list.CurrentIndex);
            Assert.AreEqual(0, center.rectTransform.anchoredPosition.y, 0.05f);
            carousel.Click(2);
            yield return new WaitForSecondsRealtime(1f);
            Assert.AreEqual(2, list.CurrentIndex);
            Assert.Greater(originalLabels[2].color.r, 0.999f);
            Assert.Greater(originalLabels[2].color.a, 0.999f);
            string secondShot = CaptureNew("center-tournament.png");
            for (int frame = 0; frame < 120 && !File.Exists(secondShot); frame++) yield return null;
            Assert.IsTrue(File.Exists(secondShot));
            NTSD.UI.MenuUIController.Instance.ShowMainMenu();
            yield return null;
            NTSD.UI.MenuUIController.Instance.ShowSelectGameMode();
            yield return null;
            Assert.AreEqual(0, list.CurrentIndex);
            Assert.AreEqual(7, carousel.GetComponentsInChildren<MenuCarouselTextEffect>().Length);
            for (int i = 0; i < 7; i++)
                Assert.AreSame(originalLabels[i], list.GetOption(i).GetComponent<TextMeshProUGUI>());
            yield return new ExitPlayMode();
            Assert.AreEqual(UnityEditor.SessionState.GetString(MenuHashKey, ""), Hash(MenuScene),
                "Menu scene was written during the test.");
            Assert.AreEqual(UnityEditor.SessionState.GetString(BattleHashKey, ""),
                Hash("Assets/NTSD/Scene/NTSD_Battle.unity"));
        }

        private static string CaptureNew(string name)
        {
            string path = Evidence + name;
            if (File.Exists(path))
                path = Evidence + Path.GetFileNameWithoutExtension(name) + "-" + System.DateTime.UtcNow.ToString("HHmmssfff") + ".png";
            ScreenCapture.CaptureScreenshot(path);
            return path;
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return System.BitConverter.ToString(sha.ComputeHash(stream));
        }

        [UnityTearDown]
        public IEnumerator ExitTestPlay()
        {
            if (Application.isPlaying) yield return new ExitPlayMode();
        }
    }
}
#endif
