#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;

namespace NTSD.Test.Editor
{
    public static class MenuFont3500MigrationEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string OldFontPath = "Assets/NTSD/MoreMountains/MMTools/Demos/MMTween/Fonts/Lato/SDF/JifengBladeArtSC-3500 SDF.asset";
        private const string NewFontPath = "Assets/NTSD/MoreMountains/MMTools/Demos/MMTween/Fonts/Lato/SDF/JifengBladeArtSC-3500-Plus SDF.asset";
        private const string FallbackFontPath = "Assets/NTSD/MoreMountains/MMTools/Demos/MMTween/Fonts/Lato/SDF/SOURCEHANSERIFCN-BOLD SDF.asset";
        private const string PreparedManifestPath = "artifacts/diagnostics/NTSD-MENU-FONT-3500-PLUS-REBIND-001/prepared-manifest.json";
        private const string OldMaterialName = "JifengBladeArtSC-3500 Atlas Material (Instance)";
        private const string NewMaterialName = "JifengBladeArtSC-3500-Plus Atlas Material (Instance)";

        [Serializable]
        private sealed class PreparedManifest
        {
            public string SceneSha;
            public string FontSha;
            public string SceneCopy;
            public string FontCopy;
        }

        [MenuItem("NTSD/UI/Rebind Menu Font 3500 Plus")]
        public static void Rebind()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath || scene.isDirty || Application.isPlaying || SceneManager.sceneCount != 1)
                throw new InvalidOperationException("Open only the clean NTSD_Menu scene in Edit Mode before rebinding.");
            if (!File.Exists(PreparedManifestPath))
                throw new InvalidOperationException("Create the protected Scene/font manifest after the final Editor compilation.");
            PreparedManifest prepared = JsonUtility.FromJson<PreparedManifest>(File.ReadAllText(PreparedManifestPath));
            if (prepared == null || !File.Exists(prepared.SceneCopy) || !File.Exists(prepared.FontCopy) ||
                Sha256(ScenePath) != prepared.SceneSha || Sha256(prepared.SceneCopy) != prepared.SceneSha ||
                Sha256(NewFontPath) != prepared.FontSha || Sha256(prepared.FontCopy) != prepared.FontSha)
                throw new InvalidOperationException("Menu scene or target font changed after the protected preimage.");

            TMP_FontAsset oldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OldFontPath);
            TMP_FontAsset newFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NewFontPath);
            TMP_FontAsset fallbackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
            if (oldFont == null || newFont == null || newFont.sourceFontFile == null ||
                fallbackFont == null || newFont.atlasTextures == null || newFont.atlasTextures.Length == 0)
                throw new InvalidOperationException("The declared font assets or target atlas are unavailable.");

            var oldLabels = new List<TextMeshProUGUI>();
            var targetLabels = new List<TextMeshProUGUI>();
            var oldInputs = new List<TMP_InputField>();
            var sceneSubmeshes = new List<TMP_SubMeshUI>();
            var sceneImages = new List<RawImage>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TextMeshProUGUI label in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (label.font == oldFont) oldLabels.Add(label);
                    else if (label.font == newFont) targetLabels.Add(label);
                }
                foreach (TMP_InputField input in root.GetComponentsInChildren<TMP_InputField>(true))
                    if (input.fontAsset == oldFont) oldInputs.Add(input);
                sceneSubmeshes.AddRange(root.GetComponentsInChildren<TMP_SubMeshUI>(true));
                sceneImages.AddRange(root.GetComponentsInChildren<RawImage>(true));
            }
            if (oldLabels.Count == 0)
                throw new InvalidOperationException(
                    $"Unexpected font bindings: old labels {oldLabels.Count}, existing target labels {targetLabels.Count}, old inputs {oldInputs.Count}.");

            var characters = new HashSet<char>();
            foreach (TextMeshProUGUI label in oldLabels)
                foreach (char character in label.text) characters.Add(character);
            foreach (TextMeshProUGUI label in targetLabels)
                foreach (char character in label.text) characters.Add(character);
            foreach (TMP_InputField input in oldInputs)
                foreach (char character in input.text) characters.Add(character);
            foreach (TMP_Character character in newFont.characterTable)
                if (character.unicode <= char.MaxValue) characters.Add((char)character.unicode);

            if (FontEngine.LoadFontFace(newFont.sourceFontFile, Mathf.RoundToInt(newFont.faceInfo.pointSize)) !=
                FontEngineError.Success)
                throw new InvalidOperationException("Cannot load the declared 3500-Plus source font face.");
            var supported = new StringBuilder(characters.Count);
            var unsupported = new StringBuilder();
            foreach (char character in characters)
            {
                if (FontEngine.TryGetGlyphIndex(character, out uint glyphIndex) && glyphIndex != 0)
                    supported.Append(character);
                else unsupported.Append(character);
            }
            string glyphs = supported.ToString();
            fallbackFont.ReadFontAssetDefinition();
            foreach (char character in unsupported.ToString())
                if (character != '\u200B' && !fallbackFont.HasCharacter(character))
                    throw new InvalidOperationException("The existing fallback lacks menu glyph: " + character);
            TMP_FontAsset probe = TMP_FontAsset.CreateFontAsset(newFont.sourceFontFile,
                Mathf.RoundToInt(newFont.faceInfo.pointSize), newFont.atlasPadding,
                newFont.atlasRenderMode, newFont.atlasWidth, newFont.atlasHeight);
            try
            {
                bool complete = probe.TryAddCharacters(glyphs, out string missing);
                Debug.Log($"3500-Plus preflight: requested {glyphs.Length}, packed {probe.characterTable.Count}, " +
                    $"complete {complete}, missing: {missing}, unsupported source: {unsupported}");
                foreach (char character in glyphs)
                    if (!probe.HasCharacter(character))
                        throw new InvalidOperationException("The 3500-Plus atlas cannot hold menu glyph: " + character);
            }
            finally
            {
                Material probeMaterial = probe.material;
                Texture2D[] probeAtlases = probe.atlasTextures;
                foreach (Texture2D atlas in probeAtlases) UnityEngine.Object.DestroyImmediate(atlas);
                UnityEngine.Object.DestroyImmediate(probeMaterial);
                UnityEngine.Object.DestroyImmediate(probe);
            }

            // This TMP fork can duplicate glyph indices when extending an existing atlas in place.
            // The preflight includes every existing character, so rebuild the same atlas once.
            newFont.ClearFontAssetData();
            if (!newFont.TryAddCharacters(glyphs, out string targetMissing))
                throw new InvalidOperationException("Target 3500-Plus atlas did not pack all glyphs: " + targetMissing);
            foreach (char character in glyphs)
                if (!newFont.HasCharacter(character))
                    throw new InvalidOperationException("The target 3500-Plus atlas is missing menu glyph: " + character);
            if (newFont.fallbackFontAssetTable == null)
                newFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (!newFont.fallbackFontAssetTable.Contains(fallbackFont))
                newFont.fallbackFontAssetTable.Add(fallbackFont);
            newFont.material.SetTexture("_MainTex", newFont.atlasTextures[0]);

            var customMaterials = new HashSet<Material>();
            foreach (Material material in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (material.name != OldMaterialName) continue;
                string path = AssetDatabase.GetAssetPath(material);
                if (string.IsNullOrEmpty(path) || path == ScenePath)
                    customMaterials.Add(material);
            }
            foreach (Material material in customMaterials)
            {
                material.SetTexture("_MainTex", newFont.atlasTextures[0]);
                material.name = NewMaterialName;
                EditorUtility.SetDirty(material);
            }
            foreach (TextMeshProUGUI label in oldLabels)
            {
                Material material = label.fontSharedMaterial;
                bool keepSceneMaterial = material != null &&
                    customMaterials.Contains(material);
                label.font = newFont;
                label.fontSharedMaterial = keepSceneMaterial ? material : newFont.material;
                EditorUtility.SetDirty(label);
            }
            foreach (TMP_InputField input in oldInputs)
            {
                input.fontAsset = newFont;
                EditorUtility.SetDirty(input);
            }
            int reboundSubmeshes = 0;
            int renamedSubmeshes = 0;
            foreach (TMP_SubMeshUI submesh in sceneSubmeshes)
            {
                if (submesh.fontAsset == oldFont)
                {
                    submesh.fontAsset = newFont;
                    if (submesh.sharedMaterial == oldFont.material)
                        submesh.sharedMaterial = newFont.material;
                    EditorUtility.SetDirty(submesh);
                    reboundSubmeshes++;
                }
                if (!submesh.gameObject.name.Contains("JifengBladeArtSC-3500") ||
                    submesh.gameObject.name.Contains("JifengBladeArtSC-3500-Plus")) continue;
                submesh.gameObject.name = submesh.gameObject.name.Replace("JifengBladeArtSC-3500", "JifengBladeArtSC-3500-Plus");
                EditorUtility.SetDirty(submesh.gameObject);
                renamedSubmeshes++;
            }
            int reboundImages = 0;
            foreach (RawImage image in sceneImages)
            {
                if (image.texture != oldFont.atlasTextures[0]) continue;
                image.texture = newFont.atlasTextures[0];
                EditorUtility.SetDirty(image);
                reboundImages++;
            }

            EditorUtility.SetDirty(newFont.atlasTextures[0]);
            EditorUtility.SetDirty(newFont.material);
            EditorUtility.SetDirty(newFont);
            AssetDatabase.SaveAssetIfDirty(newFont.atlasTextures[0]);
            AssetDatabase.SaveAssetIfDirty(newFont.material);
            AssetDatabase.SaveAssetIfDirty(newFont);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new IOException("Could not save the exact NTSD_Menu scene.");
            Debug.Log($"Menu font rebound: {oldLabels.Count} labels, {oldInputs.Count} input, " +
                $"{customMaterials.Count} scene materials, {renamedSubmeshes} submesh names, " +
                $"{reboundSubmeshes} submesh fonts, {reboundImages} atlas images, " +
                $"{glyphs.Length} supported glyphs; unsupported: {unsupported}.");
        }

        private static string Sha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
#endif
