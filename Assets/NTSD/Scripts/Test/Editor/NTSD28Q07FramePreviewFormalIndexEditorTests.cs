#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.Editor;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07FramePreviewFormalIndexEditorTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void OpeningFramePreview_RespectsSelectedContentRoot(bool formalRoot)
        {
            FieldInfo configField = typeof(GameConfig).GetField("_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            GameConfig originalConfig = GameConfig.Instance;
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            configField.SetValue(null, config);
            FieldInfo dataField = typeof(MMSingleton<GameDataManager>).GetField("_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            object originalData = dataField.GetValue(null);
            var gameObject = new GameObject("Q07 Frame Preview Index")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            CharacterFramePreviewWindow window = null;
            try
            {
                GameDataManager data = gameObject.AddComponent<GameDataManager>();
                Assert.That(data.IsLoaded(), Is.False);
                dataField.SetValue(null, data);
                Assert.That(GameDataManager.Instance, Is.SameAs(data));
                config.BattleContentRuntimeRoot = formalRoot
                    ? "Assets/NTSD/Content/LoganRuntime"
                    : string.Empty;
                window = ScriptableObject.CreateInstance<CharacterFramePreviewWindow>();
                Assert.That(data.IsLoaded(), Is.EqualTo(!formalRoot));
            }
            finally
            {
                configField.SetValue(null, originalConfig);
                dataField.SetValue(null, originalData);
                if (window != null) Object.DestroyImmediate(window);
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(config);
            }
        }
    }
}
#endif
