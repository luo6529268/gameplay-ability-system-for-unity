#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleUrpPipelineConfigurationEditorTests
    {
        private const string PipelinePath =
            "Assets/NTSD/New Universal Render Pipeline Asset.asset";
        private const string PipelineGuid = "b04ae6cf065427044a425cecfd2eec06";
        private const string RendererDataGuid = "572c783e17cfe1345982e258d706403f";

        [Test]
        public void GlobalPipelineUsesBattleUrpAssetAndActiveCentralRenderFeature()
        {
            UniversalRenderPipelineAsset expectedPipeline =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            Assert.That(expectedPipeline, Is.Not.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(PipelinePath), Is.EqualTo(PipelineGuid));
            Assert.That(GraphicsSettings.defaultRenderPipeline, Is.SameAs(expectedPipeline));
            Assert.That(GraphicsSettings.currentRenderPipeline, Is.SameAs(expectedPipeline));

            UniversalRendererData rendererData = ResolveDefaultRendererData(expectedPipeline);
            Assert.That(rendererData, Is.Not.Null);
            string rendererPath = AssetDatabase.GetAssetPath(rendererData);
            Assert.That(AssetDatabase.AssetPathToGUID(rendererPath), Is.EqualTo(RendererDataGuid));

            BattleRenderFeature battleFeature = null;
            for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
            {
                if (rendererData.rendererFeatures[i] is BattleRenderFeature candidate)
                {
                    battleFeature = candidate;
                    break;
                }
            }

            Assert.That(battleFeature, Is.Not.Null);
            Assert.That(battleFeature.isActive, Is.True);
            Assert.That(battleFeature.Material, Is.Not.Null);
            Assert.That(battleFeature.ArrayMaterial, Is.Not.Null);
            BattleRenderFeatureInstaller.ValidateOrThrow();
        }

        private static UniversalRendererData ResolveDefaultRendererData(
            UniversalRenderPipelineAsset pipeline)
        {
            var serializedPipeline = new SerializedObject(pipeline);
            SerializedProperty rendererDataList =
                serializedPipeline.FindProperty("m_RendererDataList");
            SerializedProperty defaultRendererIndex =
                serializedPipeline.FindProperty("m_DefaultRendererIndex");
            Assert.That(rendererDataList, Is.Not.Null);
            Assert.That(defaultRendererIndex, Is.Not.Null);
            Assert.That(defaultRendererIndex.intValue,
                Is.InRange(0, rendererDataList.arraySize - 1));
            return rendererDataList
                .GetArrayElementAtIndex(defaultRendererIndex.intValue)
                .objectReferenceValue as UniversalRendererData;
        }
    }
}
#endif
