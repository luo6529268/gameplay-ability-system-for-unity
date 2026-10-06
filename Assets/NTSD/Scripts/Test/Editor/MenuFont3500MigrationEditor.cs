#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NTSD.Test.Editor
{
    public sealed class MenuFont3500MigrationEditor : EditorWindow
    {
        [SerializeField] private TMP_FontAsset targetFont;
        private Label sceneLabel;
        private Label countLabel;
        private Label statusLabel;
        private ScrollView results;
        private Button applyButton;

        [MenuItem("NTSD/UI/补齐当前场景 TMP 字体")]
        public static void OpenWindow()
        {
            var window = GetWindow<MenuFont3500MigrationEditor>();
            window.titleContent = new GUIContent("补齐 TMP 字体");
            window.minSize = new Vector2(420f, 300f);
            window.Show();
        }

        private void OnEnable()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += RefreshScene;
        }

        private void OnDisable()
        {
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= RefreshScene;
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = root.style.paddingRight = 8;
            root.style.paddingTop = root.style.paddingBottom = 8;
            root.Add(new HelpBox("只补齐当前活动场景中未指定或引用失效的 TextMeshProUGUI 字体，包含未激活对象。已指定字体的组件会跳过。", HelpBoxMessageType.Info));
            var fontField = new ObjectField("目标 FontAsset")
            {
                name = "targetFont",
                objectType = typeof(TMP_FontAsset),
                allowSceneObjects = false,
                value = targetFont
            };
            fontField.RegisterValueChangedCallback(evt =>
            {
                targetFont = evt.newValue as TMP_FontAsset;
                RefreshScene();
            });
            root.Add(fontField);
            sceneLabel = new Label();
            countLabel = new Label();
            root.Add(sceneLabel);
            root.Add(countLabel);
            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.Add(new Button(RefreshScene) { text = "扫描当前场景" });
            applyButton = new Button(Apply) { name = "applyFont", text = "补齐缺失字体" };
            applyButton.style.marginLeft = 6;
            actions.Add(applyButton);
            root.Add(actions);
            statusLabel = new Label();
            statusLabel.style.whiteSpace = WhiteSpace.Normal;
            root.Add(statusLabel);
            results = new ScrollView();
            results.style.flexGrow = 1;
            root.Add(results);
            RefreshScene();
        }

        private void OnActiveSceneChanged(Scene previous, Scene current) => RefreshScene();
        private void OnPlayModeStateChanged(PlayModeStateChange state) => RefreshScene();

        private void RefreshScene()
        {
            if (results == null) return;
            Scene scene = SceneManager.GetActiveScene();
            bool editable = CanEditScene(scene);
            sceneLabel.text = "当前场景：" + (scene.IsValid() ? scene.name : "无");
            var missing = editable ? FindMissingFonts(scene) : new List<TextMeshProUGUI>();
            countLabel.text = editable ? $"缺失字体组件：{missing.Count}" : "请在普通场景的编辑模式下使用。";
            applyButton.SetEnabled(editable && targetFont != null && missing.Count > 0);
            results.Clear();
            foreach (TextMeshProUGUI label in missing)
            {
                GameObject owner = label.gameObject;
                var row = new Button(() =>
                {
                    if (owner == null) return;
                    Selection.activeGameObject = owner;
                    EditorGUIUtility.PingObject(owner);
                }) { text = GetHierarchyPath(owner.transform) };
                results.Add(row);
            }
        }

        private void Apply()
        {
            try
            {
                int changed = AssignMissingFonts(SceneManager.GetActiveScene(), targetFont);
                RefreshScene();
                statusLabel.text = $"已补齐 {changed} 个组件。可按 Ctrl+Z 撤销，请自行保存场景。";
            }
            catch (Exception exception)
            {
                RefreshScene();
                statusLabel.text = exception.Message;
                Debug.LogException(exception);
            }
        }

        private static bool CanEditScene(Scene scene)
        {
            return scene.IsValid() && scene.isLoaded && !EditorSceneManager.IsPreviewScene(scene) &&
                !EditorApplication.isPlayingOrWillChangePlaymode && PrefabStageUtility.GetCurrentPrefabStage() == null;
        }

        internal static List<TextMeshProUGUI> FindMissingFonts(Scene scene)
        {
            var missing = new List<TextMeshProUGUI>();
            if (!scene.IsValid() || !scene.isLoaded) return missing;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TextMeshProUGUI label in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    var serialized = new SerializedObject(label);
                    if (serialized.FindProperty("m_fontAsset").objectReferenceValue == null)
                        missing.Add(label);
                }
            }
            return missing;
        }

        internal static int AssignMissingFonts(Scene scene, TMP_FontAsset font)
        {
            if (font == null || !EditorUtility.IsPersistent(font))
                throw new InvalidOperationException("请先选择一个项目中的 TMP FontAsset 文件。");
            if (!CanEditScene(scene) || scene != SceneManager.GetActiveScene())
                throw new InvalidOperationException("只能处理当前活动场景，请退出 Play 或预制体编辑模式。");
            if (font.material == null || font.atlasTexture == null)
                throw new InvalidOperationException("所选 FontAsset 缺少材质或图集，请先检查字体资源。");
            List<TextMeshProUGUI> missing = FindMissingFonts(scene);
            if (missing.Count == 0) return 0;
            const string undoName = "补齐当前场景 TMP 字体";
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(undoName);
            Undo.RegisterCompleteObjectUndo(missing.ToArray(), undoName);
            try
            {
                foreach (TextMeshProUGUI label in missing)
                {
                    label.font = font;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(label);
                    EditorUtility.SetDirty(label);
                }
            }
            catch
            {
                Undo.RevertAllDownToGroup(group);
                throw;
            }
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            return missing.Count;
        }

        private static string GetHierarchyPath(Transform item)
        {
            string path = item.name;
            while (item.parent != null)
            {
                item = item.parent;
                path = item.name + "/" + path;
            }
            return path;
        }
    }
}
#endif
