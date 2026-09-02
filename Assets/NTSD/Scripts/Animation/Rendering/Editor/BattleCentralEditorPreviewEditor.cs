#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    [CustomEditor(typeof(BattleCentralEditorPreview))]
    public sealed class BattleCentralEditorPreviewEditor : UnityEditor.Editor
    {
        private const string SampleSourcePath =
            "Assets/NTSD/Sprite/Character/Zuozhu/sasuke_0.bmp";
        private const string FootMarkerPath =
            "Assets/NTSD/Sprite/UIPanels/FootSelf.png";
        private const string CommonShadowPrefabPath =
            "Assets/NTSD/Prefabs/Common/Shadow.prefab";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var preview = (BattleCentralEditorPreview)target;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("SceneView Authoring", EditorStyles.boldLabel);
            DrawSourceStatus(preview);

            if (GUILayout.Button("配置佐助示例并聚焦 SceneView"))
                ConfigureSampleAndFocus(preview);
            if (GUILayout.Button("移动控制器到战斗相机中心"))
                MoveToWorldCameraCenter(preview);
            if (GUILayout.Button("聚焦当前预览"))
                FocusPreview(preview);

            EditorGUILayout.HelpBox(
                "选中此组件后，可在 SceneView 拖动 Actor 0、Actor 1… 的位置手柄。" +
                "青色框是 Sprite 边界，灰色框是角色原有 Shadow，黄色框是新增 FootSelf，" +
                "红色框是头顶血条；" +
                "Actor 0 的 Foot Marker/HP Offset 手柄调整全部 Actor 的对应布局。",
                MessageType.Info);
        }

        private void OnSceneGUI()
        {
            var preview = (BattleCentralEditorPreview)target;
            serializedObject.Update();
            SerializedProperty actorsProperty = serializedObject.FindProperty("actors");
            if (actorsProperty == null)
                return;
            SerializedProperty healthOffsetProperty = serializedObject
                .FindProperty("healthBarStyle")?
                .FindPropertyRelative("offsetPixels");
            SerializedProperty footMarkerOffsetProperty = serializedObject
                .FindProperty("footMarkerStyle")?
                .FindPropertyRelative("offsetPixels");

            for (int actorIndex = 0; actorIndex < actorsProperty.arraySize; actorIndex++)
            {
                SerializedProperty actorProperty =
                    actorsProperty.GetArrayElementAtIndex(actorIndex);
                SerializedProperty visibleProperty =
                    actorProperty.FindPropertyRelative("visible");
                if (visibleProperty != null && !visibleProperty.boolValue)
                    continue;

                SerializedProperty anchorProperty =
                    actorProperty.FindPropertyRelative("anchor");
                SerializedProperty localPositionProperty =
                    actorProperty.FindPropertyRelative("localPivotPosition");
                if (localPositionProperty == null)
                    continue;

                Transform anchor = anchorProperty?.objectReferenceValue as Transform;
                Transform reference = anchor != null ? anchor : preview.transform;
                Vector3 worldPosition =
                    reference.TransformPoint(localPositionProperty.vector3Value);

                Handles.color = new Color(1f, 0.82f, 0.15f, 1f);
                float handleSize = HandleUtility.GetHandleSize(worldPosition) * 0.12f;
                Handles.SphereHandleCap(
                    0,
                    worldPosition,
                    Quaternion.identity,
                    handleSize,
                    EventType.Repaint);
                Handles.Label(
                    worldPosition + Vector3.up * handleSize,
                    $"Actor {actorIndex}");

                EditorGUI.BeginChangeCheck();
                Vector3 nextWorldPosition =
                    Handles.PositionHandle(worldPosition, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(preview, $"Move Preview Actor {actorIndex}");
                    localPositionProperty.vector3Value =
                        reference.InverseTransformPoint(nextWorldPosition);
                    serializedObject.ApplyModifiedProperties();
                    preview.RequestEditorPreviewRefresh();
                    EditorUtility.SetDirty(preview);
                    serializedObject.Update();
                }

                if (!preview.TryGetEditorLayout(
                        actorIndex,
                        out BattleCentralEditorPreviewLayout layout))
                {
                    continue;
                }

                Handles.color = new Color(0.1f, 0.9f, 1f, 0.95f);
                Handles.DrawWireCube(layout.SpriteBounds.center, layout.SpriteBounds.size);
                Handles.DrawLine(
                    layout.PivotWorldPosition + Vector3.left * handleSize * 0.5f,
                    layout.PivotWorldPosition + Vector3.right * handleSize * 0.5f);
                Handles.DrawLine(
                    layout.PivotWorldPosition + Vector3.down * handleSize * 0.5f,
                    layout.PivotWorldPosition + Vector3.up * handleSize * 0.5f);
                if (layout.HasCommonShadow)
                {
                    Handles.color = new Color(0.55f, 0.55f, 0.55f, 1f);
                    Handles.DrawWireCube(
                        layout.CommonShadowBounds.center,
                        layout.CommonShadowBounds.size);
                }
                if (layout.HasFootMarker)
                {
                    Handles.color = new Color(1f, 0.78f, 0.05f, 1f);
                    Handles.DrawWireCube(
                        layout.FootMarkerBounds.center,
                        layout.FootMarkerBounds.size);
                    if (actorIndex == 0 && footMarkerOffsetProperty != null)
                    {
                        Vector3 offsetHandlePosition = layout.FootMarkerBounds.center;
                        Handles.Label(
                            offsetHandlePosition + Vector3.down * handleSize,
                            "Foot Marker Offset（全部 Actor）");
                        EditorGUI.BeginChangeCheck();
                        Vector3 nextOffsetHandlePosition = Handles.Slider2D(
                            offsetHandlePosition,
                            Vector3.forward,
                            Vector3.right,
                            Vector3.up,
                            handleSize * 0.75f,
                            Handles.SphereHandleCap,
                            0f);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(preview, "Move Preview Foot Markers");
                            Vector3 worldDelta =
                                nextOffsetHandlePosition - offsetHandlePosition;
                            Vector2 offsetPixels =
                                footMarkerOffsetProperty.vector2Value;
                            offsetPixels.x +=
                                worldDelta.x / NTSDRenderSpace.UnitsPerPixelX;
                            offsetPixels.y +=
                                worldDelta.y / NTSDRenderSpace.UnitsPerPixelY;
                            footMarkerOffsetProperty.vector2Value = offsetPixels;
                            serializedObject.ApplyModifiedProperties();
                            preview.RequestEditorPreviewRefresh();
                            EditorUtility.SetDirty(preview);
                            serializedObject.Update();
                        }
                    }
                }
                if (layout.HasHealthBar)
                {
                    Handles.color = new Color(1f, 0.2f, 0.2f, 1f);
                    Handles.DrawWireCube(
                        layout.HealthBarBounds.center,
                        layout.HealthBarBounds.size);
                    if (actorIndex == 0 && healthOffsetProperty != null)
                    {
                        Vector3 offsetHandlePosition = layout.HealthBarBounds.center;
                        Handles.Label(
                            offsetHandlePosition + Vector3.up * handleSize,
                            "HP Offset（全部 Actor）");
                        EditorGUI.BeginChangeCheck();
                        Vector3 nextOffsetHandlePosition = Handles.Slider2D(
                            offsetHandlePosition,
                            Vector3.forward,
                            Vector3.right,
                            Vector3.up,
                            handleSize * 0.75f,
                            Handles.SphereHandleCap,
                            0f);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(preview, "Move Preview Health Bars");
                            Vector3 worldDelta =
                                nextOffsetHandlePosition - offsetHandlePosition;
                            Vector2 offsetPixels = healthOffsetProperty.vector2Value;
                            offsetPixels.x += worldDelta.x / NTSDRenderSpace.UnitsPerPixelX;
                            offsetPixels.y += worldDelta.y / NTSDRenderSpace.UnitsPerPixelY;
                            healthOffsetProperty.vector2Value = offsetPixels;
                            serializedObject.ApplyModifiedProperties();
                            preview.RequestEditorPreviewRefresh();
                            EditorUtility.SetDirty(preview);
                            serializedObject.Update();
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ConfigureSampleAndFocus(BattleCentralEditorPreview preview)
        {
            Texture2D sample = AssetDatabase.LoadAssetAtPath<Texture2D>(SampleSourcePath);
            if (sample == null)
            {
                Debug.LogError(
                    $"[BattleCentralEditorPreview] Sample source is unavailable: {SampleSourcePath}");
                return;
            }
            Sprite footMarker = AssetDatabase.LoadAssetAtPath<Sprite>(FootMarkerPath);
            if (footMarker == null)
            {
                Debug.LogWarning(
                    $"[BattleCentralEditorPreview] Foot marker is unavailable: {FootMarkerPath}");
            }
            GameObject commonShadowPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(CommonShadowPrefabPath);
            if (commonShadowPrefab == null)
            {
                Debug.LogWarning(
                    $"[BattleCentralEditorPreview] Common shadow is unavailable: " +
                    CommonShadowPrefabPath);
            }

            serializedObject.Update();
            serializedObject.FindProperty("drawCommonShadows").boolValue =
                commonShadowPrefab != null;
            serializedObject.FindProperty("commonShadowPrefab").objectReferenceValue =
                commonShadowPrefab;
            serializedObject.FindProperty("drawFootMarkers").boolValue =
                footMarker != null;
            serializedObject.FindProperty("footMarkerSprite").objectReferenceValue =
                footMarker;
            SerializedProperty footMarkerStyleProperty =
                serializedObject.FindProperty("footMarkerStyle");
            BattleFootMarkerStyle footMarkerStyle = BattleFootMarkerStyle.Default;
            footMarkerStyleProperty.FindPropertyRelative("widthPixels").floatValue =
                footMarkerStyle.WidthPixels;
            footMarkerStyleProperty.FindPropertyRelative("heightPixels").floatValue =
                footMarkerStyle.HeightPixels;
            footMarkerStyleProperty.FindPropertyRelative("offsetPixels").vector2Value =
                footMarkerStyle.OffsetPixels;
            footMarkerStyleProperty.FindPropertyRelative("tint").colorValue = Color.white;
            SerializedProperty actorsProperty = serializedObject.FindProperty("actors");
            if (actorsProperty.arraySize == 0)
                actorsProperty.InsertArrayElementAtIndex(0);
            SerializedProperty actorProperty = actorsProperty.GetArrayElementAtIndex(0);
            actorProperty.FindPropertyRelative("visible").boolValue = true;
            actorProperty.FindPropertyRelative("resolveFromCharacterManager").boolValue = false;
            actorProperty.FindPropertyRelative("sprite").objectReferenceValue = null;
            actorProperty.FindPropertyRelative("sourceSheet").objectReferenceValue = sample;
            actorProperty.FindPropertyRelative("sourceRectPixels").rectIntValue =
                BattleCentralEditorPreview.ResolveTopLeftSourceRectForEditor(
                    sample,
                    79,
                    79);
            actorProperty.FindPropertyRelative("sourcePivot").vector2Value =
                new Vector2(0.5f, 0f);
            actorProperty.FindPropertyRelative("anchor").objectReferenceValue = null;
            actorProperty.FindPropertyRelative("localPivotPosition").vector3Value = Vector3.zero;
            actorProperty.FindPropertyRelative("showCommonShadow").boolValue = true;
            actorProperty.FindPropertyRelative("showFootMarker").boolValue = true;
            actorProperty.FindPropertyRelative("showHealthBar").boolValue = true;
            actorProperty.FindPropertyRelative("currentHealth").intValue = 35;
            actorProperty.FindPropertyRelative("recoverableHealth").intValue = 75;
            actorProperty.FindPropertyRelative("maximumHealth").intValue = 100;
            serializedObject.ApplyModifiedProperties();
            preview.RequestEditorPreviewRefresh();
            EditorUtility.SetDirty(preview);
            SceneView.RepaintAll();
            FocusPreview(preview);
        }

        private static void MoveToWorldCameraCenter(BattleCentralEditorPreview preview)
        {
            Camera worldCamera = NTSDRenderSpace.WorldCamera;
            if (worldCamera == null)
            {
                Debug.LogWarning(
                    "[BattleCentralEditorPreview] The battle world camera is unavailable.");
                return;
            }

            Undo.RecordObject(preview.transform, "Move Preview To Battle Camera Center");
            Vector3 cameraPosition = worldCamera.transform.position;
            preview.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);
            preview.transform.hasChanged = true;
            EditorUtility.SetDirty(preview.transform);
            preview.RequestEditorPreviewRefresh();
            SceneView.RepaintAll();
            FocusPreview(preview);
        }

        private static void FocusPreview(BattleCentralEditorPreview preview)
        {
            SceneView sceneView = SceneView.lastActiveSceneView ??
                                  EditorWindow.GetWindow<SceneView>();
            if (sceneView == null)
                return;

            bool found = false;
            Bounds bounds = default;
            for (int actorIndex = 0; actorIndex < preview.EditorActorCount; actorIndex++)
            {
                if (!preview.TryGetEditorLayout(
                        actorIndex,
                        out BattleCentralEditorPreviewLayout layout))
                {
                    continue;
                }

                Bounds actorBounds = layout.SpriteBounds;
                if (layout.HasCommonShadow)
                    actorBounds.Encapsulate(layout.CommonShadowBounds);
                if (layout.HasFootMarker)
                    actorBounds.Encapsulate(layout.FootMarkerBounds);
                if (layout.HasHealthBar)
                    actorBounds.Encapsulate(layout.HealthBarBounds);
                if (found)
                    bounds.Encapsulate(actorBounds);
                else
                {
                    bounds = actorBounds;
                    found = true;
                }
            }

            if (!found)
                bounds = new Bounds(preview.transform.position, new Vector3(2f, 2f, 1f));
            Vector3 size = bounds.size;
            size.x = Mathf.Max(size.x, 1f);
            size.y = Mathf.Max(size.y, 1f);
            size.z = Mathf.Max(size.z, 1f);
            bounds.size = size;
            sceneView.Frame(bounds, false);
            sceneView.Repaint();
        }

        private static void DrawSourceStatus(BattleCentralEditorPreview preview)
        {
            bool hasStableEditorSource = false;
            bool managerOnly = false;
            for (int actorIndex = 0; actorIndex < preview.EditorActorCount; actorIndex++)
            {
                BattleCentralEditorPreviewActor actor = preview.GetEditorActor(actorIndex);
                if (actor == null || !actor.Visible)
                    continue;
                if (actor.Sprite != null || actor.SourceSheet != null)
                    hasStableEditorSource = true;
                else if (actor.ResolveFromCharacterManager)
                    managerOnly = true;
            }

            if (hasStableEditorSource)
                return;
            EditorGUILayout.HelpBox(
                managerOnly
                    ? "当前 Actor 只依赖 CharacterAnimtorManager；未进入 Play Mode 时资源可能尚未加载。可点击下方按钮配置稳定的 BMP Source Sheet 预览。"
                    : "当前没有可供 Edit Mode 解析的 Sprite 或 Source Sheet，因此 SceneView 不会生成角色和血条。",
                MessageType.Warning);
        }
    }
}
#endif
