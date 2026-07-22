#if UNITY_EDITOR
using System;
using System.Linq;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering.Editor
{
    public static class BattleRenderFeatureInstaller
    {
        public const string RendererDataPath =
            "Assets/NTSD/New Universal Render Pipeline Asset_Renderer.asset";
        public const string MaterialPath =
            "Assets/NTSD/Materials/BattleCentralTransparent.mat";
        public const string ArrayMaterialPath =
            "Assets/NTSD/Materials/BattleCentralTransparentArray.mat";
        public const string ShaderName = BattleSpriteMaterialContract.CentralTextureShaderName;
        public const string ArrayShaderName = BattleSpriteMaterialContract.CentralArrayShaderName;

        [MenuItem("NTSD/Battle Rendering/Install Central Render Feature")]
        public static void Install()
        {
            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
                throw new InvalidOperationException($"UniversalRendererData not found: {RendererDataPath}");

            Material material = LoadOrCreateMaterial();
            Material arrayMaterial = LoadOrCreateMaterial(ArrayMaterialPath, ArrayShaderName, "BattleCentralTransparentArray");
            BattleRenderFeature[] existing = rendererData.rendererFeatures
                .OfType<BattleRenderFeature>()
                .Where(feature => feature != null)
                .ToArray();
            BattleRenderFeature feature;
            if (existing.Length == 0)
            {
                feature = ScriptableObject.CreateInstance<BattleRenderFeature>();
                feature.name = nameof(BattleRenderFeature);
                feature.Configure(material, arrayMaterial, BattleCentralDrawMode.OrderedChunks);
                AssetDatabase.AddObjectToAsset(feature, rendererData);
                rendererData.rendererFeatures.Add(feature);
            }
            else
            {
                feature = existing[0];
                feature.Configure(material, arrayMaterial, BattleCentralDrawMode.OrderedChunks);
                for (int index = 1; index < existing.Length; index++)
                {
                    rendererData.rendererFeatures.Remove(existing[index]);
                    UnityEngine.Object.DestroyImmediate(existing[index], true);
                }
            }

            SynchronizeFeatureMap(rendererData);
            rendererData.SetDirty();
            EditorUtility.SetDirty(rendererData);
            EditorUtility.SetDirty(feature);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(RendererDataPath, ImportAssetOptions.ForceUpdate);
            ValidateOrThrow();
            Debug.Log("[BattleRenderFeatureInstaller] Installed and validated BattleRenderFeature.");
        }

        [MenuItem("NTSD/Battle Rendering/Validate Central Render Feature")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
            Debug.Log("[BattleRenderFeatureInstaller] BattleRenderFeature validation passed.");
        }

        public static void ValidateOrThrow()
        {
            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
                throw new InvalidOperationException($"UniversalRendererData not found: {RendererDataPath}");

            BattleRenderFeature[] features = rendererData.rendererFeatures
                .OfType<BattleRenderFeature>()
                .Where(feature => feature != null)
                .ToArray();
            if (features.Length != 1)
                throw new InvalidOperationException($"Expected one BattleRenderFeature, found {features.Length}.");
            if (!AssetDatabase.IsSubAsset(features[0]))
                throw new InvalidOperationException("BattleRenderFeature must be serialized as a renderer-data subasset.");
            if (!BattleSpriteMaterialContract.IsDeclaredCentralMaterial(
                    features[0].Material,
                    false))
            {
                throw new InvalidOperationException(
                    "BattleRenderFeature material must declare the white premultiplied sprite-alpha contract.");
            }
            if (!BattleSpriteMaterialContract.IsDeclaredCentralMaterial(
                    features[0].ArrayMaterial,
                    true))
            {
                throw new InvalidOperationException(
                    "BattleRenderFeature array material must declare the white premultiplied sprite-alpha contract.");
            }
            if (features[0].InjectionPoint != RenderPassEvent.AfterRenderingTransparents)
                throw new InvalidOperationException("BattleRenderFeature injection point must be AfterRenderingTransparents.");

            var serialized = new SerializedObject(rendererData);
            SerializedProperty featureMap = serialized.FindProperty("m_RendererFeatureMap");
            if (featureMap == null || featureMap.arraySize != rendererData.rendererFeatures.Count)
                throw new InvalidOperationException("Renderer feature map is missing or out of sync.");
            for (int index = 0; index < featureMap.arraySize; index++)
            {
                ScriptableRendererFeature feature = rendererData.rendererFeatures[index];
                if (feature == null)
                    throw new InvalidOperationException($"Renderer feature {index} is null.");
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId) ||
                    featureMap.GetArrayElementAtIndex(index).longValue != localId)
                {
                    throw new InvalidOperationException($"Renderer feature map entry {index} is stale.");
                }
            }
        }

        private static Material LoadOrCreateMaterial()
        {
            return LoadOrCreateMaterial(MaterialPath, ShaderName, "BattleCentralTransparent");
        }

        private static Material LoadOrCreateMaterial(string path, string shaderName, string materialName)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                throw new InvalidOperationException($"Shader not found: {shaderName}");
            if (material == null)
            {
                material = new Material(shader) { name = materialName };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }
            if (material.HasProperty("_Color") && material.GetColor("_Color") != Color.white)
            {
                material.SetColor("_Color", Color.white);
                EditorUtility.SetDirty(material);
            }
            return material;
        }

        private static void SynchronizeFeatureMap(UniversalRendererData rendererData)
        {
            var serialized = new SerializedObject(rendererData);
            serialized.Update();
            SerializedProperty featureMap = serialized.FindProperty("m_RendererFeatureMap");
            featureMap.arraySize = rendererData.rendererFeatures.Count;
            for (int index = 0; index < rendererData.rendererFeatures.Count; index++)
            {
                ScriptableRendererFeature feature = rendererData.rendererFeatures[index];
                if (feature == null ||
                    !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId))
                {
                    throw new InvalidOperationException($"Cannot resolve renderer feature local id at {index}.");
                }
                featureMap.GetArrayElementAtIndex(index).longValue = localId;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
