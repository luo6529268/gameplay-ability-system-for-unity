using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
    [InitializeOnLoad]
    internal static class PortableTMPSetup
    {
        private const string SettingsPath = "Assets/TextMeshPro/Resources/TextMeshPro/TMPSettings.asset";
        private const string SourceFontPath = "Assets/TextMeshPro/Fonts/LiberationSans.ttf";
        private const string FontFolder = "Assets/TextMeshPro/Resources/TextMeshPro/FontAssets";
        private const string FontPath = FontFolder + "/LiberationSans SDF.asset";

        static PortableTMPSetup()
        {
            EditorApplication.delayCall += Initialize;
        }

        [MenuItem("Tools/Portable TextMeshPro/Initialize Default Font")]
        public static void Initialize()
        {
            TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(SettingsPath);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (settings == null || sourceFont == null)
            {
                Debug.LogWarning("Portable TextMeshPro: settings or LiberationSans.ttf is missing; default font was not initialized.");
                return;
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (fontAsset == null)
            {
                if (!AssetDatabase.IsValidFolder(FontFolder))
                    AssetDatabase.CreateFolder("Assets/TextMeshPro/Resources/TextMeshPro", "FontAssets");

                fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
                if (fontAsset == null || fontAsset.material == null || fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0)
                {
                    Debug.LogError("Portable TextMeshPro: failed to create the default font asset.");
                    return;
                }

                fontAsset.name = "LiberationSans SDF";
                fontAsset.atlasTextures[0].name = "LiberationSans SDF Atlas";
                fontAsset.material.name = "LiberationSans SDF Material";
                AssetDatabase.CreateAsset(fontAsset, FontPath);
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            SerializedObject serializedSettings = new SerializedObject(settings);
            SerializedProperty defaultFont = serializedSettings.FindProperty("m_defaultFontAsset");
            if (defaultFont != null && defaultFont.objectReferenceValue != fontAsset)
            {
                defaultFont.objectReferenceValue = fontAsset;
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
