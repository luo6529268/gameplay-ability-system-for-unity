#if UNITY_EDITOR && UNITY_INCLUDE_TESTS

using System.Reflection;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q09EarthquakeBackgroundEditorTests
    {
        private const int TestLayer = 29;
        private const int CaptureSize = 64;

        [Test]
        public void FixedFullViewProjectsSourceEarthquakeOffsetAtShaderOutlet()
        {
            var mapObject = new GameObject("Q09 Projected Earthquake Map");
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                mapObject.hideFlags = HideFlags.HideAndDontSave;
                texture = CreateSolidTexture(Color.red);
                sprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0.5f), 100f);
                SpriteRenderer renderer = mapObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                BattleBackgroundPlatformPresentation presentation =
                    mapObject.AddComponent<BattleBackgroundPlatformPresentation>();
                presentation.EditorLiveCameraFrame = false;
                SetField(presentation, "sourceRenderer", renderer);
                Material originalMaterial = renderer.sharedMaterial;
                var frame = new BattlePresentationFrame
                {
                    EarthquakeBackgroundOffsetX = 2,
                    EarthquakeBackgroundOffsetY = 1,
                };
                BattleSpatialProjection projection =
                    BattleSpatialProjection.FromReferenceViewport(2048, 1152);
                MethodInfo apply = typeof(BattleBackgroundPlatformPresentation)
                    .GetMethod("ApplyProjectedEarthquakeFrameForDiagnostics",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(apply, Is.Not.Null);
                apply.Invoke(presentation, new object[] { frame, projection });

                Material projectedMaterial = renderer.sharedMaterial;
                Assert.That(projectedMaterial, Is.Not.SameAs(originalMaterial));
                Vector4 localOffset = projectedMaterial.GetVector(
                    Shader.PropertyToID("_EarthquakeLocalOffset"));
                Assert.That(localOffset.x,
                    Is.EqualTo(2.0 * 2048.0 / 1333.0 / 100.0).Within(0.000001));
                Assert.That(localOffset.y,
                    Is.EqualTo(-1152.0 / 730.0 / 100.0).Within(0.000001));
                Assert.That(frame.EarthquakeBackgroundOffsetX, Is.EqualTo(2));
                Assert.That(frame.EarthquakeBackgroundOffsetY, Is.EqualTo(1));

                apply.Invoke(presentation,
                    new object[] { new BattlePresentationFrame(), projection });
                Assert.That(renderer.sharedMaterial, Is.SameAs(originalMaterial));
                Assert.That(mapObject.transform.position, Is.EqualTo(Vector3.zero));
            }
            finally
            {
                Object.DestroyImmediate(mapObject);
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void FrozenOffsetMovesOnlyMapPixelsAndRestoresDefaultFrame()
        {
            var cameraObject = new GameObject("Q09 Background Camera");
            var mapObject = new GameObject("Q09 Map Sprite");
            var unrelatedObject = new GameObject("Q09 Unrelated Sprite");
            Texture2D redTexture = null;
            Texture2D greenTexture = null;
            Sprite redSprite = null;
            Sprite greenSprite = null;
            RenderTexture target = null;
            Texture2D readback = null;
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                cameraObject.hideFlags = HideFlags.HideAndDontSave;
                mapObject.hideFlags = HideFlags.HideAndDontSave;
                unrelatedObject.hideFlags = HideFlags.HideAndDontSave;
                mapObject.layer = TestLayer;
                unrelatedObject.layer = TestLayer;

                Camera camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.orthographic = true;
                camera.orthographicSize = 0.32f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.cullingMask = 1 << TestLayer;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                target = new RenderTexture(CaptureSize, CaptureSize, 0);
                target.Create();
                camera.targetTexture = target;

                redTexture = CreateSolidTexture(Color.red);
                greenTexture = CreateSolidTexture(Color.green);
                redSprite = Sprite.Create(redTexture,
                    new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 100f);
                greenSprite = Sprite.Create(greenTexture,
                    new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 100f);
                SpriteRenderer mapRenderer = mapObject.AddComponent<SpriteRenderer>();
                mapRenderer.sprite = redSprite;
                SpriteRenderer unrelatedRenderer =
                    unrelatedObject.AddComponent<SpriteRenderer>();
                unrelatedRenderer.sprite = greenSprite;
                unrelatedObject.transform.position = new Vector3(-0.2f, 0.2f, 0f);

                BattleBackgroundPlatformPresentation presentation =
                    mapObject.AddComponent<BattleBackgroundPlatformPresentation>();
                presentation.EditorLiveCameraFrame = false;
                SetField(presentation, "targetCamera", camera);
                SetField(presentation, "sourceRenderer", mapRenderer);

                readback = new Texture2D(CaptureSize, CaptureSize, TextureFormat.RGBA32, false);
                Color32[] baseline = Capture(camera, target, readback);
                Vector2Int redBefore = FindCentroid(baseline, red: true);
                Vector2Int greenBefore = FindCentroid(baseline, red: false);
                Bounds originalBounds = mapRenderer.bounds;
                Vector3 originalMapPosition = mapObject.transform.position;
                Vector3 originalCameraPosition = camera.transform.position;
                float originalCameraSize = camera.orthographicSize;
                Material originalMaterial = mapRenderer.sharedMaterial;

                MethodInfo apply = typeof(BattleBackgroundPlatformPresentation)
                    .GetMethod("ApplyEarthquakeFrameForDiagnostics",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(apply, Is.Not.Null,
                    "The project Map Sprite has no frozen-earthquake visual consumer.");
                var offsetFrame = new BattlePresentationFrame
                {
                    EarthquakeBackgroundOffsetX = 2,
                    EarthquakeBackgroundOffsetY = 1,
                };
                apply.Invoke(presentation, new object[] { offsetFrame });

                Color32[] shifted = Capture(camera, target, readback);
                Vector2Int redAfter = FindCentroid(shifted, red: true);
                Vector2Int greenAfter = FindCentroid(shifted, red: false);
                Assert.That(redAfter, Is.EqualTo(redBefore + new Vector2Int(2, -1)));
                Assert.That(greenAfter, Is.EqualTo(greenBefore));
                Assert.That(mapRenderer.bounds, Is.EqualTo(originalBounds));
                Assert.That(mapObject.transform.position, Is.EqualTo(originalMapPosition));
                Assert.That(camera.transform.position, Is.EqualTo(originalCameraPosition));
                Assert.That(camera.orthographicSize, Is.EqualTo(originalCameraSize));
                Assert.That(mapRenderer.sharedMaterial, Is.SameAs(originalMaterial));

                MethodInfo endCamera = typeof(BattleBackgroundPlatformPresentation)
                    .GetMethod("OnEndCameraRendering",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(endCamera, Is.Not.Null);
                apply.Invoke(presentation, new object[] { offsetFrame });
                endCamera.Invoke(presentation,
                    new object[] { default(ScriptableRenderContext), null });
                Assert.That(mapRenderer.sharedMaterial, Is.Not.SameAs(originalMaterial));
                endCamera.Invoke(presentation,
                    new object[] { default(ScriptableRenderContext), camera });
                Assert.That(mapRenderer.sharedMaterial, Is.SameAs(originalMaterial));

                apply.Invoke(presentation, new object[] { new BattlePresentationFrame() });
                CollectionAssert.AreEqual(baseline, Capture(camera, target, readback));
                Assert.That(mapRenderer.sharedMaterial, Is.SameAs(originalMaterial));
            }
            finally
            {
                RenderTexture.active = previousActive;
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(mapObject);
                Object.DestroyImmediate(unrelatedObject);
                if (redSprite != null)
                    Object.DestroyImmediate(redSprite);
                if (greenSprite != null)
                    Object.DestroyImmediate(greenSprite);
                if (redTexture != null)
                    Object.DestroyImmediate(redTexture);
                if (greenTexture != null)
                    Object.DestroyImmediate(greenTexture);
                if (readback != null)
                    Object.DestroyImmediate(readback);
                if (target != null)
                    Object.DestroyImmediate(target);
            }
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color[64];
            for (int index = 0; index < pixels.Length; index++)
                pixels[index] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Color32[] Capture(Camera camera, RenderTexture target,
            Texture2D readback)
        {
            camera.Render();
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, CaptureSize, CaptureSize), 0, 0, false);
            readback.Apply(false);
            return readback.GetPixels32();
        }

        private static Vector2Int FindCentroid(Color32[] pixels, bool red)
        {
            int sumX = 0;
            int sumY = 0;
            int count = 0;
            for (int y = 0; y < CaptureSize; y++)
            {
                for (int x = 0; x < CaptureSize; x++)
                {
                    Color32 pixel = pixels[y * CaptureSize + x];
                    bool match = red
                        ? pixel.r > 200 && pixel.g < 50 && pixel.b < 50
                        : pixel.g > 200 && pixel.r < 50 && pixel.b < 50;
                    if (!match)
                        continue;
                    sumX += x;
                    sumY += y;
                    count++;
                }
            }
            Assert.That(count, Is.GreaterThan(0),
                red ? "Red map pixels were not rendered." : "Unrelated green pixels were not rendered.");
            return new Vector2Int(sumX / count, sumY / count);
        }

        private static void SetField(object owner, string name, object value)
        {
            FieldInfo field = owner.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(owner, value);
        }
    }
}
#endif
