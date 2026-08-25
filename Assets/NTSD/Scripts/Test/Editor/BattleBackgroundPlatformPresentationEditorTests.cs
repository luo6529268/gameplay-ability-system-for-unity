using System.Reflection;

using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using NTSD.App;

namespace NTSD.Test.Editor
{
    public sealed class BattleBackgroundPlatformPresentationEditorTests
    {
        [TestCase(RuntimePlatform.Android)]
        [TestCase(RuntimePlatform.IPhonePlayer)]
        public void Resolver_MapsMobilePlayersToMobilePresentation(RuntimePlatform platform)
        {
            Assert.That(
                BattleBackgroundPlatformPresentation.ResolvePresentationPlatform(platform),
                Is.EqualTo(BattleBackgroundPresentationPlatform.Mobile));
        }

        [TestCase(RuntimePlatform.WindowsEditor)]
        [TestCase(RuntimePlatform.WindowsPlayer)]
        [TestCase(RuntimePlatform.OSXEditor)]
        [TestCase(RuntimePlatform.LinuxPlayer)]
        public void Resolver_MapsDesktopPlatformsToFullPresentation(RuntimePlatform platform)
        {
            Assert.That(
                BattleBackgroundPlatformPresentation.ResolvePresentationPlatform(platform),
                Is.EqualTo(BattleBackgroundPresentationPlatform.Desktop));
        }

        [Test]
        public void Resolver_UsesEditorPreviewWithoutChangingPlayerMappingContract()
        {
            Assert.That(
                BattleBackgroundPlatformPresentation.ResolvePresentationPlatform(
                    RuntimePlatform.WindowsEditor,
                    BattleBackgroundEditorPreviewMode.Mobile),
                Is.EqualTo(BattleBackgroundPresentationPlatform.Mobile));
            Assert.That(
                BattleBackgroundPlatformPresentation.ResolvePresentationPlatform(
                    RuntimePlatform.Android,
                    BattleBackgroundEditorPreviewMode.Desktop),
                Is.EqualTo(BattleBackgroundPresentationPlatform.Desktop));
        }

        [Test]
        public void DesktopPresentation_HasNoBottomGap()
        {
            float resolvedGap =
                BattleBackgroundPlatformPresentation.ResolveBottomGapNormalized(
                    BattleBackgroundPresentationPlatform.Desktop,
                    0.5f);

            Assert.That(resolvedGap, Is.Zero);
        }

        [Test]
        public void MobilePresentation_UsesTheConfiguredBottomGap()
        {
            float resolvedGap =
                BattleBackgroundPlatformPresentation.ResolveBottomGapNormalized(
                    BattleBackgroundPresentationPlatform.Mobile,
                    0.2f);

            Assert.That(resolvedGap, Is.EqualTo(0.2f));
        }

        [Test]
        public void MobilePresentation_ClampsTheBottomGap()
        {
            float belowMinimum =
                BattleBackgroundPlatformPresentation.ResolveBottomGapNormalized(
                    BattleBackgroundPresentationPlatform.Mobile,
                    -1f);
            float aboveMaximum =
                BattleBackgroundPlatformPresentation.ResolveBottomGapNormalized(
                    BattleBackgroundPresentationPlatform.Mobile,
                    1f);

            Assert.That(belowMinimum, Is.Zero);
            Assert.That(
                aboveMaximum,
                Is.EqualTo(BattleBackgroundPlatformPresentation.MaximumAndroidBottomGapNormalized));
        }

        [Test]
        public void EditorLiveCameraFrame_OnlyAllowsEditModeCameraWritesWhenEnabled()
        {
            Assert.That(
                BattleBackgroundPlatformPresentation.ShouldApplyWorldCameraFrame(
                    isPlaying: false,
                    editorLiveCameraFrame: true),
                Is.True);
            Assert.That(
                BattleBackgroundPlatformPresentation.ShouldApplyWorldCameraFrame(
                    isPlaying: false,
                    editorLiveCameraFrame: false),
                Is.False);
        }

        [Test]
        public void EditorLiveCameraFrame_DoesNotDisableTheExistingPlayerCameraFrameContract()
        {
            Assert.That(
                BattleBackgroundPlatformPresentation.ShouldApplyWorldCameraFrame(
                    isPlaying: true,
                    editorLiveCameraFrame: false),
                Is.True);
        }

        [Test]
        public void SourceOwner_RejectsAForeignDuplicatePresentationComponent()
        {
            var sourceOwner = new GameObject("Background Source Owner");
            var foreignObject = new GameObject("Foreign Presentation Owner");

            try
            {
                SpriteRenderer sourceRenderer = sourceOwner.AddComponent<SpriteRenderer>();

                Assert.That(
                    BattleBackgroundPlatformPresentation.IsValidSourceRendererOwner(
                        sourceOwner,
                        sourceRenderer),
                    Is.True);
                Assert.That(
                    BattleBackgroundPlatformPresentation.IsValidSourceRendererOwner(
                        foreignObject,
                        sourceRenderer),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(foreignObject);
                Object.DestroyImmediate(sourceOwner);
            }
        }

        [Test]
        public void EditorLiveCameraFrame_EditorUpdateTracksSpriteReplacementAndRestoresBaseline()
        {
            GameObject backgroundObject = null;
            GameObject cameraObject = null;
            Texture2D firstTexture = null;
            Texture2D replacementTexture = null;
            Sprite firstSprite = null;
            Sprite replacementSprite = null;

            try
            {
                cameraObject = new GameObject("Background Frame Test Camera");
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.aspect = 2f;
                Vector3 baselinePosition = new Vector3(17f, 23f, -10f);
                const float baselineOrthographicSize = 12f;
                camera.transform.position = baselinePosition;
                camera.orthographicSize = baselineOrthographicSize;

                backgroundObject = new GameObject("Background Frame Test Source");
                backgroundObject.transform.position = new Vector3(4f, 6f, 0f);
                SpriteRenderer sourceRenderer =
                    backgroundObject.AddComponent<SpriteRenderer>();
                BattleBackgroundPlatformPresentation presentation =
                    backgroundObject.AddComponent<BattleBackgroundPlatformPresentation>();
                ConfigurePresentation(presentation, camera, sourceRenderer);

                firstTexture = new Texture2D(200, 100);
                firstSprite = Sprite.Create(
                    firstTexture,
                    new Rect(0f, 0f, 200f, 100f),
                    new Vector2(0.5f, 0.5f),
                    100f);
                sourceRenderer.sprite = firstSprite;
                InvokeEditorUpdate(presentation);

                Rect firstExpected =
                    BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                        sourceRenderer.bounds,
                        BattleBackgroundPlatformPresentation.ResolveOutputAspect(camera),
                        0f);
                AssertCameraFrame(camera, firstExpected);

                replacementTexture = new Texture2D(400, 200);
                replacementSprite = Sprite.Create(
                    replacementTexture,
                    new Rect(0f, 0f, 400f, 200f),
                    new Vector2(0.5f, 0.5f),
                    100f);
                sourceRenderer.sprite = replacementSprite;
                InvokeEditorUpdate(presentation);

                Rect replacementExpected =
                    BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                        sourceRenderer.bounds,
                        BattleBackgroundPlatformPresentation.ResolveOutputAspect(camera),
                        0f);
                AssertCameraFrame(camera, replacementExpected);
                Assert.That(
                    replacementExpected.size,
                    Is.Not.EqualTo(firstExpected.size));

                presentation.EditorLiveCameraFrame = false;
                Assert.That(camera.transform.position, Is.EqualTo(baselinePosition));
                Assert.That(
                    camera.orthographicSize,
                    Is.EqualTo(baselineOrthographicSize));
            }
            finally
            {
                if (backgroundObject != null)
                    Object.DestroyImmediate(backgroundObject);
                if (cameraObject != null)
                    Object.DestroyImmediate(cameraObject);
                if (firstSprite != null)
                    Object.DestroyImmediate(firstSprite);
                if (replacementSprite != null)
                    Object.DestroyImmediate(replacementSprite);
                if (firstTexture != null)
                    Object.DestroyImmediate(firstTexture);
                if (replacementTexture != null)
                    Object.DestroyImmediate(replacementTexture);
            }
        }

        [Test]
        public void WorldCameraFrame_DesktopMatchingAspect_UsesTheWholeWorldBackground()
        {
            Bounds backgroundBounds = CreateBackgroundBounds();
            Rect frame = BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                backgroundBounds,
                16f / 9f,
                0f);

            AssertRect(frame, -0.24f, 14.24f, 20.48f, 11.52f);
        }

        [Test]
        public void WorldCameraFrame_MobileGap_MovesFrameBelowMapWithoutScaling()
        {
            Bounds backgroundBounds = CreateBackgroundBounds();
            Rect frame = BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                backgroundBounds,
                16f / 9f,
                1f / 9f);

            AssertRect(frame, -0.24f, 12.96f, 20.48f, 11.52f);
            Assert.That(
                backgroundBounds.min.y - frame.yMin,
                Is.EqualTo(1.28f).Within(0.0001f));
        }

        [Test]
        public void WorldCameraFrame_WiderViewport_CropsOnlyMapTopFromTheSameWorldCoordinates()
        {
            Rect frame = BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                CreateBackgroundBounds(),
                2f,
                0f);

            AssertRect(frame, -0.24f, 14.24f, 20.48f, 10.24f);
        }

        [Test]
        public void WorldCameraFrame_NarrowViewport_CropsSidesSymmetrically()
        {
            Rect frame = BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                CreateBackgroundBounds(),
                4f / 3f,
                0f);

            AssertRect(frame, 2.32f, 14.24f, 15.36f, 11.52f);
        }

        [Test]
        public void WorldCameraFrame_ReplacementSpriteBoundsProduceANewCameraFrame()
        {
            Rect frame = BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                new Bounds(
                    new Vector3(-3f, 7f, 0f),
                    new Vector3(32f, 18f, 0f)),
                16f / 9f,
                0f);

            AssertRect(frame, -19f, -2f, 32f, 18f);
        }

        [Test]
        public void WorldCameraFrame_InvalidInput_FailsClosed()
        {
            Rect frame = BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame(
                default,
                16f / 9f,
                BattleBackgroundPlatformPresentation.DefaultAndroidBottomGapNormalized);

            Assert.That(frame, Is.EqualTo(default(Rect)));
        }

        [Test]
        public void BottomOverlayShader_IsAvailableFromResources()
        {
            Shader shader = Resources.Load<Shader>(
                BattleBackgroundPlatformPresentation.BottomOverlayShaderResourcePath);

            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.name, Is.EqualTo("NTSD/Presentation/BattleBottomOverlay"));
        }

        private static Bounds CreateBackgroundBounds()
        {
            return new Bounds(
                new Vector3(10f, 20f, 0f),
                new Vector3(20.48f, 11.52f, 0f));
        }

        private static void ConfigurePresentation(
            BattleBackgroundPlatformPresentation presentation,
            Camera targetCamera,
            SpriteRenderer sourceRenderer)
        {
            var serializedPresentation = new SerializedObject(presentation);
            serializedPresentation.FindProperty("targetCamera").objectReferenceValue =
                targetCamera;
            serializedPresentation.FindProperty("sourceRenderer").objectReferenceValue =
                sourceRenderer;
            serializedPresentation.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void InvokeEditorUpdate(
            BattleBackgroundPlatformPresentation presentation)
        {
            MethodInfo update = typeof(BattleBackgroundPlatformPresentation).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(update, Is.Not.Null);
            update.Invoke(presentation, null);
        }

        private static void AssertCameraFrame(Camera camera, Rect expected)
        {
            Assert.That(
                camera.transform.position.x,
                Is.EqualTo(expected.center.x).Within(0.0001f));
            Assert.That(
                camera.transform.position.y,
                Is.EqualTo(expected.center.y).Within(0.0001f));
            Assert.That(
                camera.orthographicSize,
                Is.EqualTo(expected.height * 0.5f).Within(0.0001f));
        }

        private static void AssertRect(
            Rect actual,
            float expectedX,
            float expectedY,
            float expectedWidth,
            float expectedHeight)
        {
            Assert.That(actual.x, Is.EqualTo(expectedX).Within(0.0001f));
            Assert.That(actual.y, Is.EqualTo(expectedY).Within(0.0001f));
            Assert.That(actual.width, Is.EqualTo(expectedWidth).Within(0.0001f));
            Assert.That(actual.height, Is.EqualTo(expectedHeight).Within(0.0001f));
        }
    }
}
