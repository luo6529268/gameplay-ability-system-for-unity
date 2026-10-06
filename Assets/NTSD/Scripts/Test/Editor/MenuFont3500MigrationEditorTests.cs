using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NTSD.Test.Editor
{
    public sealed class MenuFont3500MigrationEditorTests
    {
        [MenuItem("NTSD/Diagnostics/Validate Missing TMP Font Editor")]
        public static void ValidateInCurrentEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run this editor check outside Play mode.");
            var tests = new MenuFont3500MigrationEditorTests();
            tests.AssignOnlyMissingFontsInActiveSceneIncludingInactiveAndSupportsUndo();
            tests.WindowHasConfigurableFontFieldAndApplyIsDisabledWithoutTarget();
            const string evidence = "artifacts/diagnostics/NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001/";
            Directory.CreateDirectory(evidence);
            File.WriteAllText(evidence + "editor-check-passed-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + ".json",
                "{\"passed\":true,\"checks\":2,\"activeSceneDirtyPreserved\":true}");
            Debug.Log("Missing TMP Font editor checks passed: 2; current Scene dirty state preserved.");
        }

        [Test]
        public void AssignOnlyMissingFontsInActiveSceneIncludingInactiveAndSupportsUndo()
        {
            Scene original = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(original.path))
                Assert.Ignore("Run NTSD/Diagnostics/Validate Missing TMP Font Editor from a saved Scene.");
            bool originalDirty = original.isDirty;
            Scene first = default;
            var originalLabels = original.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<TextMeshProUGUI>(true)).ToArray();
            var originalFonts = originalLabels.Select(label => label.font).ToArray();
            try
            {
                TMP_FontAsset font = AssetDatabase.FindAssets("t:TMP_FontAsset")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid)))
                    .First(asset => asset != null && asset.material != null && asset.atlasTexture != null);
                first = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                var missing = Label(first, "MissingFont", null);
                var disabled = Label(first, "DisabledMissingFont", null);
                disabled.enabled = false;
                var assigned = Label(first, "AssignedFont", font);
                SceneManager.SetActiveScene(first);
                CollectionAssert.AreEquivalent(new[] { missing, disabled },
                    MenuFont3500MigrationEditor.FindMissingFonts(first));
                Assert.Throws<InvalidOperationException>(() =>
                    MenuFont3500MigrationEditor.AssignMissingFonts(first, null));
                Assert.Throws<InvalidOperationException>(() =>
                    MenuFont3500MigrationEditor.AssignMissingFonts(original, font));
                Assert.AreEqual(2, MenuFont3500MigrationEditor.AssignMissingFonts(first, font));
                Assert.AreSame(font, missing.font);
                Assert.AreSame(font.material, missing.fontSharedMaterial);
                Assert.AreSame(font, disabled.font);
                Assert.AreSame(font, assigned.font);
                CollectionAssert.AreEqual(originalFonts, originalLabels.Select(label => label.font));
                Assert.AreEqual(0, MenuFont3500MigrationEditor.AssignMissingFonts(first, font));
                Assert.IsTrue(first.isDirty);
                Undo.PerformUndo();
                Assert.IsNull(missing.font);
                Assert.IsNull(disabled.font);
                Assert.AreSame(font, assigned.font);
                CollectionAssert.AreEqual(originalFonts, originalLabels.Select(label => label.font));
            }
            finally
            {
                SceneManager.SetActiveScene(original);
                if (first.IsValid()) EditorSceneManager.CloseScene(first, true);
            }
            Assert.AreEqual(originalDirty, original.isDirty);
        }

        [Test]
        public void WindowHasConfigurableFontFieldAndApplyIsDisabledWithoutTarget()
        {
            var window = ScriptableObject.CreateInstance<MenuFont3500MigrationEditor>();
            try
            {
                window.CreateGUI();
                var font = window.rootVisualElement.Q<UnityEditor.UIElements.ObjectField>("targetFont");
                Assert.IsNotNull(font);
                Assert.AreEqual(typeof(TMP_FontAsset), font.objectType);
                Assert.IsFalse(font.allowSceneObjects);
                Assert.IsNull(font.value);
                Assert.IsFalse(window.rootVisualElement.Q<Button>("applyFont").enabledSelf);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(window);
            }
        }

        private static TextMeshProUGUI Label(Scene scene, string name, TMP_FontAsset font)
        {
            var owner = new GameObject(name, typeof(RectTransform));
            owner.SetActive(false);
            SceneManager.MoveGameObjectToScene(owner, scene);
            var label = owner.AddComponent<TextMeshProUGUI>();
            var serialized = new SerializedObject(label);
            serialized.FindProperty("m_fontAsset").objectReferenceValue = font;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return label;
        }
    }
}
