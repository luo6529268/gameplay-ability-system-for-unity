#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;

using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattlePresentationInitializationEditorTests
    {
        [Test]
        public void BattleBootstrapDoesNotDisableSerializedPresentationOnActivation()
        {
            GameObject host = null;
            Camera worldCamera = null;
            try
            {
                host = new GameObject("BattlePresentationInitializationTests");
                host.SetActive(false);
                worldCamera = new GameObject("WorldCamera").AddComponent<Camera>();
                Camera uiCamera = new GameObject("UiCamera").AddComponent<Camera>();
                Canvas canvas = new GameObject(
                    "BattleCanvas",
                    typeof(RectTransform),
                    typeof(Canvas)).GetComponent<Canvas>();
                worldCamera.transform.SetParent(host.transform, false);
                uiCamera.transform.SetParent(host.transform, false);
                canvas.transform.SetParent(host.transform, false);
                worldCamera.enabled = true;
                uiCamera.enabled = true;
                canvas.enabled = true;

                BattleBootstrap bootstrap = host.AddComponent<BattleBootstrap>();
                SetPrivateField(bootstrap, "worldCamera", worldCamera);
                SetPrivateField(bootstrap, "uiCamera", uiCamera);
                SetPrivateField(bootstrap, "battleCanvas", canvas);
                NTSDRenderSpace.BindWorldCamera(worldCamera);

                host.SetActive(true);

                Assert.That(worldCamera.enabled, Is.True);
                Assert.That(uiCamera.enabled, Is.True);
                Assert.That(canvas.enabled, Is.True);
                Assert.That(NTSDRenderSpace.WorldCamera, Is.SameAs(worldCamera));
            }
            finally
            {
                NTSDRenderSpace.ClearBoundWorldCamera(worldCamera);
                if (host != null)
                    Object.DestroyImmediate(host);
            }
        }

        private static void SetPrivateField(
            BattleBootstrap bootstrap,
            string fieldName,
            Object value)
        {
            FieldInfo field = typeof(BattleBootstrap).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(bootstrap, value);
        }

    }
}
#endif
