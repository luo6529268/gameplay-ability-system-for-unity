using UnityEngine;
using UnityEditor;
using NTSD.UI.Menu;

namespace NTSD.Editor
{
    [CustomEditor(typeof(MenuOptionBase), true)]
    public class MenuOptionBaseEditor : UnityEditor.Editor
    {
        private SerializedProperty highlightType;
        private SerializedProperty highlightObject;
        private SerializedProperty targetGraphic;
        private SerializedProperty normalColor;
        private SerializedProperty selectedColor;
        private SerializedProperty selectSound;
        private SerializedProperty confirmSound;

        private void OnEnable()
        {
            highlightType = serializedObject.FindProperty("highlightType");
            highlightObject = serializedObject.FindProperty("highlightObject");
            targetGraphic = serializedObject.FindProperty("targetGraphic");
            normalColor = serializedObject.FindProperty("normalColor");
            selectedColor = serializedObject.FindProperty("selectedColor");
            selectSound = serializedObject.FindProperty("selectSound");
            confirmSound = serializedObject.FindProperty("confirmSound");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (highlightType == null)
            {
                EditorGUILayout.HelpBox("Failed to load properties. Please reselect the object.", MessageType.Warning);
                return;
            }

            EditorGUILayout.PropertyField(highlightType);
            EditorGUILayout.Space();

            HighlightType type = (HighlightType)highlightType.enumValueIndex;

            switch (type)
            {
                case HighlightType.ShowHide:
                    EditorGUILayout.LabelField("ShowHide Settings", EditorStyles.boldLabel);
                    if (highlightObject != null)
                        EditorGUILayout.PropertyField(highlightObject);
                    break;

                case HighlightType.Color:
                    EditorGUILayout.LabelField("Color Settings", EditorStyles.boldLabel);
                    if (targetGraphic != null)
                        EditorGUILayout.PropertyField(targetGraphic);
                    if (normalColor != null)
                        EditorGUILayout.PropertyField(normalColor);
                    if (selectedColor != null)
                        EditorGUILayout.PropertyField(selectedColor);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
            if (selectSound != null)
                EditorGUILayout.PropertyField(selectSound);
            if (confirmSound != null)
                EditorGUILayout.PropertyField(confirmSound);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
