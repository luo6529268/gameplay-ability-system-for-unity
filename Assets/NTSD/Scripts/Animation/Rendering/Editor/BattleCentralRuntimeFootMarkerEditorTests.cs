#if UNITY_EDITOR
using NUnit.Framework;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCentralRuntimeFootMarkerEditorTests
    {
        [Test]
        public void RuntimeSizing_UsesStableResourceHeightRelativeToStandardCharacter()
        {
            Assert.That(
                BattleFootMarkerSizing.ResolveStableCharacterScale(
                    BattleHealthBarAnchor.DefaultCharacterHeightPixels),
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                BattleFootMarkerSizing.ResolveStableCharacterScale(
                    BattleHealthBarAnchor.DefaultCharacterHeightPixels * 2f),
                Is.EqualTo(2f).Within(0.0001f));
            Assert.That(
                BattleFootMarkerSizing.ResolveStableCharacterScale(0f),
                Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void RuntimeAuthoring_UsesPreviewSpriteSizeOffsetAndTint()
        {
            GameObject previewObject = null;
            Texture2D texture = null;
            Sprite sprite = null;
            Material material = null;
            BattleRenderFeature feature = null;
            System.IDisposable validationScope = null;
            try
            {
                texture = NewTexture(128, 48);
                sprite = NewSprite(texture);
                material = NewCentralMaterial();
                var style = new BattleFootMarkerStyle(
                    64f,
                    24f,
                    new Vector2(3f, -4f),
                    new Color32(11, 22, 33, 44));
                previewObject = new GameObject("RuntimeFootSelfStylePreview")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                BattleCentralEditorPreview preview =
                    previewObject.AddComponent<BattleCentralEditorPreview>();
                preview.ConfigureForSelfCheck(
                    material,
                    new BattleCentralEditorPreviewActor(),
                    BattleHealthBarStyle.Default);
                preview.ConfigureFootMarkerForSelfCheck(sprite, style);
                validationScope =
                    BattleCentralEditorPreview.BeginExclusiveValidationForSelfCheck(preview);
                feature = ScriptableObject.CreateInstance<BattleRenderFeature>();
                feature.Configure(material, BattleCentralDrawMode.OrderedChunks);

                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();

                Assert.That(
                    BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck,
                    Is.True);
                Assert.That(
                    BattleCentralRenderSystem.RuntimeFootMarkerSpriteForSelfCheck,
                    Is.SameAs(sprite));
                Assert.That(
                    BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.WidthPixels,
                    Is.EqualTo(64f));
                Assert.That(
                    BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.HeightPixels,
                    Is.EqualTo(24f));
                Assert.That(
                    BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.OffsetPixels,
                    Is.EqualTo(new Vector2(3f, -4f)));
                Assert.That(
                    BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.Tint,
                    Is.EqualTo(new Color32(11, 22, 33, 44)));
            }
            finally
            {
                validationScope?.Dispose();
                if (feature != null)
                {
                    BattleCentralRenderSystem.UnregisterFeature(feature);
                    Object.DestroyImmediate(feature);
                }
                if (previewObject != null)
                    Object.DestroyImmediate(previewObject);
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
                if (material != null)
                    Object.DestroyImmediate(material);
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
            }
        }

        [Test]
        public void RuntimeBackend_UsesStableGroundAnchorAndPreviewFinalPixels()
        {
            var backend = new BattleFootMarkerBatchBackend();
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                texture = NewTexture(128, 48);
                sprite = NewSprite(texture);
                var style = new BattleFootMarkerStyle(
                    64f,
                    24f,
                    new Vector2(3f, -4f),
                    new Color32(11, 22, 33, 44));
                var frame = new BattlePresentationFrame();
                frame.Reset(1);
                frame.AddCommand(CreateEntityCommand(
                    new Vector3(100f, 200f, 3f),
                    new Vector2(10f, 20f),
                    true));
                frame.CommandsMaterialized = true;

                backend.BuildFromFrame(frame, sprite, style, true);

                Assert.That(backend.BuiltFrame, Is.SameAs(frame));
                Assert.That(backend.ActiveMarkerCount, Is.EqualTo(1));
                Assert.That(backend.ActiveQuadCount, Is.EqualTo(1));
                Assert.That(backend.Mesh, Is.Not.Null);
                Assert.That(backend.Mesh.subMeshCount, Is.EqualTo(1));
                Assert.That(backend.Texture, Is.SameAs(texture));
                float centerX = 10f + 3f * NTSDRenderSpace.UnitsPerPixelX;
                float centerY = 20f - 4f * NTSDRenderSpace.UnitsPerPixelY;
                float width = 64f * NTSDRenderSpace.UnitsPerPixelX;
                float height = 24f * NTSDRenderSpace.UnitsPerPixelY;
                Vector3 bottomLeft = backend.GetVertexPosition(0);
                Vector3 topRight = backend.GetVertexPosition(3);
                Assert.That(
                    bottomLeft.x,
                    Is.EqualTo(centerX - width * 0.5f).Within(0.0001f));
                Assert.That(
                    bottomLeft.y,
                    Is.EqualTo(centerY - height * 0.5f).Within(0.0001f));
                Assert.That(bottomLeft.z, Is.EqualTo(3f).Within(0.0001f));
                Assert.That(
                    topRight.x,
                    Is.EqualTo(centerX + width * 0.5f).Within(0.0001f));
                Assert.That(
                    topRight.y,
                    Is.EqualTo(centerY + height * 0.5f).Within(0.0001f));
                Assert.That(topRight.z, Is.EqualTo(3f).Within(0.0001f));
                Assert.That(
                    backend.GetVertexColor(0),
                    Is.EqualTo(new Color32(11, 22, 33, 44)));
            }
            finally
            {
                backend.Dispose();
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void RuntimeBackend_ScalesBaseSizeByStableCharacterScaleButKeepsOffsetUnscaled()
        {
            var backend = new BattleFootMarkerBatchBackend();
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                texture = NewTexture(128, 48);
                sprite = NewSprite(texture);
                var style = new BattleFootMarkerStyle(
                    64f,
                    24f,
                    new Vector2(3f, -4f),
                    Color.white);
                var frame = new BattlePresentationFrame();
                frame.Reset(1);
                frame.AddCommand(CreateEntityCommand(
                    new Vector3(100f, 500f, 3f),
                    new Vector2(10f, 20f),
                    true,
                    2f));
                frame.CommandsMaterialized = true;

                backend.BuildFromFrame(frame, sprite, style, true);

                float centerX = 10f + 3f * NTSDRenderSpace.UnitsPerPixelX;
                float centerY = 20f - 4f * NTSDRenderSpace.UnitsPerPixelY;
                float width = 128f * NTSDRenderSpace.UnitsPerPixelX;
                float height = 48f * NTSDRenderSpace.UnitsPerPixelY;
                Vector3 bottomLeft = backend.GetVertexPosition(0);
                Vector3 topRight = backend.GetVertexPosition(3);
                Assert.That(
                    bottomLeft.x,
                    Is.EqualTo(centerX - width * 0.5f).Within(0.0001f));
                Assert.That(
                    bottomLeft.y,
                    Is.EqualTo(centerY - height * 0.5f).Within(0.0001f));
                Assert.That(
                    topRight.x,
                    Is.EqualTo(centerX + width * 0.5f).Within(0.0001f));
                Assert.That(
                    topRight.y,
                    Is.EqualTo(centerY + height * 0.5f).Within(0.0001f));
            }
            finally
            {
                backend.Dispose();
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void RuntimeBackend_FiltersNonSelfAndClearsOldFrame()
        {
            var backend = new BattleFootMarkerBatchBackend();
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                texture = NewTexture(128, 48);
                sprite = NewSprite(texture);
                var firstFrame = new BattlePresentationFrame();
                firstFrame.Reset(1);
                firstFrame.AddCommand(CreateEntityCommand(
                    Vector3.zero,
                    Vector2.zero,
                    true));
                firstFrame.AddCommand(CreateEntityCommand(
                    Vector3.one,
                    Vector2.one,
                    false));
                firstFrame.CommandsMaterialized = true;
                backend.BuildFromFrame(
                    firstFrame,
                    sprite,
                    BattleFootMarkerStyle.Default,
                    true);
                Assert.That(backend.ActiveMarkerCount, Is.EqualTo(1));

                var secondFrame = new BattlePresentationFrame();
                secondFrame.Reset(2);
                secondFrame.CommandsMaterialized = true;
                backend.BuildFromFrame(
                    secondFrame,
                    sprite,
                    BattleFootMarkerStyle.Default,
                    true);

                Assert.That(backend.BuiltFrame, Is.SameAs(secondFrame));
                Assert.That(backend.ActiveMarkerCount, Is.Zero);
                Assert.That(backend.ActiveVertexCount, Is.Zero);
            }
            finally
            {
                backend.Dispose();
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void RuntimeBackend_OneThousandSelfMarkersRemainOneMeshAndSubMesh()
        {
            var backend = new BattleFootMarkerBatchBackend();
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                texture = NewTexture(128, 48);
                sprite = NewSprite(texture);
                var frame = new BattlePresentationFrame();
                frame.Reset(1);
                for (int index = 0; index < 1000; index++)
                {
                    frame.AddCommand(CreateEntityCommand(
                        new Vector3(index, 0f, 0f),
                        new Vector2(index, 0f),
                        true));
                }
                frame.CommandsMaterialized = true;

                backend.BuildFromFrame(
                    frame,
                    sprite,
                    BattleFootMarkerStyle.Default,
                    true);

                Assert.That(backend.ActiveMarkerCount, Is.EqualTo(1000));
                Assert.That(backend.ActiveQuadCount, Is.EqualTo(1000));
                Assert.That(backend.ActiveVertexCount, Is.EqualTo(4000));
                Assert.That(backend.ActiveIndexCount, Is.EqualTo(6000));
                Assert.That(backend.Mesh.subMeshCount, Is.EqualTo(1));
            }
            finally
            {
                backend.Dispose();
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
        }

        private static BattleRenderCommand CreateEntityCommand(
            Vector3 visualPosition,
            Vector2 stableFootAnchorWorld,
            bool showSelfFootMarker,
            float footMarkerScale = 1f)
        {
            return new BattleRenderCommand(
                BattleRenderCommandType.Entity,
                RuntimeEntityHandle.Invalid,
                1,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                visualPosition,
                new Vector2(999f, 777f),
                new Vector2(0.25f, 0.75f),
                new Rect(0f, 0f, 1f, 1f),
                false,
                default,
                stableFootAnchorWorld: stableFootAnchorWorld,
                hasStableFootAnchor: true,
                showSelfFootMarker: showSelfFootMarker,
                footMarkerScale: footMarkerScale);
        }

        private static Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
        }

        private static Sprite NewSprite(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static Material NewCentralMaterial()
        {
            Shader shader = Shader.Find("NTSD/BattleCentralTransparent");
            Assert.That(shader, Is.Not.Null);
            return new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
        }
    }
}
#endif
