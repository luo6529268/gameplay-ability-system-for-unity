#if UNITY_EDITOR
using NUnit.Framework;
using NTSD.App;
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
        public void RuntimeAnimation_UsesConfiguredFrameBoundariesAndLoops()
        {
            double duration = BattleFootMarkerAnimation.DefaultFrameDurationSeconds;

            Assert.That(
                BattleFootMarkerAnimation.ResolveFrameIndex(0d, (float)duration, 6),
                Is.EqualTo(0));
            Assert.That(
                BattleFootMarkerAnimation.ResolveFrameIndex(
                    duration * 0.99d,
                    (float)duration,
                    6),
                Is.EqualTo(0));
            Assert.That(
                BattleFootMarkerAnimation.ResolveFrameIndex(
                    duration * 1.01d,
                    (float)duration,
                    6),
                Is.EqualTo(1));
            Assert.That(
                BattleFootMarkerAnimation.ResolveFrameIndex(
                    duration * 5.01d,
                    (float)duration,
                    6),
                Is.EqualTo(5));
            Assert.That(
                BattleFootMarkerAnimation.ResolveFrameIndex(
                    duration * 6.01d,
                    (float)duration,
                    6),
                Is.EqualTo(0));
            Assert.That(
                BattleFootMarkerAnimation.ResolveFrameIndex(
                    -duration * 0.01d,
                    (float)duration,
                    6),
                Is.EqualTo(5));
        }

        [Test]
        public void RuntimeAuthoring_UsesPreviewSpriteSizeOffsetAndTint()
        {
            GameObject previewObject = null;
            Texture2D texture = null;
            Texture2D secondTexture = null;
            Sprite sprite = null;
            Sprite secondSprite = null;
            Material material = null;
            BattleRenderFeature feature = null;
            GameConfig gameConfig = null;
            System.IDisposable validationScope = null;
            try
            {
                texture = NewTexture(128, 48);
                secondTexture = NewTexture(128, 48);
                sprite = NewSprite(texture);
                secondSprite = NewSprite(secondTexture);
                gameConfig = ScriptableObject.CreateInstance<GameConfig>();
                gameConfig.FootMarkerSprite = sprite;
                gameConfig.FootMarkerAnimationFrames = new[] { sprite, secondSprite };
                gameConfig.FootMarkerAnimationFrameDurationSeconds = 0.08f;
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
                preview.ConfigureGameConfigFootMarkerForSelfCheck(gameConfig, style);
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
                    BattleCentralRenderSystem.RuntimeFootMarkerAnimationFramesForSelfCheck,
                    Is.SameAs(gameConfig.FootMarkerAnimationFrames));
                Assert.That(
                    BattleCentralRenderSystem
                        .RuntimeFootMarkerAnimationFrameDurationSecondsForSelfCheck,
                    Is.EqualTo(0.08f));
                Assert.That(
                    BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0d),
                    Is.SameAs(texture));
                Assert.That(
                    BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0.081d),
                    Is.SameAs(secondTexture));
                Assert.That(
                    BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0.161d),
                    Is.SameAs(texture));
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
                if (gameConfig != null)
                    Object.DestroyImmediate(gameConfig);
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (secondSprite != null)
                    Object.DestroyImmediate(secondSprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
                if (secondTexture != null)
                    Object.DestroyImmediate(secondTexture);
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

        [Test]
        public void RuntimeConfig_UsesGameConfigWhenNoAuthoring()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.True);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerSpriteForSelfCheck, Is.SameAs(fixture.FirstSprite));
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerAnimationFramesForSelfCheck,
                    Is.SameAs(fixture.Config.FootMarkerAnimationFrames));
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerAnimationFrameDurationSecondsForSelfCheck,
                    Is.EqualTo(0.125f));
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.SizePixels,
                    Is.EqualTo(BattleFootMarkerStyle.Default.SizePixels));
                Assert.That(BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0d), Is.SameAs(fixture.FirstTexture));
                Assert.That(BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0.126d), Is.SameAs(fixture.SecondTexture));
                Assert.That(BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0.251d), Is.SameAs(fixture.FirstTexture));
            }
        }

        [Test]
        public void RuntimeConfig_FramesOnlyResolvesReference()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                fixture.Config.FootMarkerSprite = null;
                fixture.Config.FootMarkerAnimationFrames = new[] { null, fixture.SecondSprite };
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.True);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerSpriteForSelfCheck, Is.SameAs(fixture.SecondSprite));
                Assert.That(BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0d), Is.SameAs(fixture.SecondTexture));
            }
        }

        [Test]
        public void RuntimeConfig_MissingConfigDisables()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                fixture.BindConfig(null);
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.False);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerSpriteForSelfCheck, Is.Null);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerAnimationFramesForSelfCheck, Is.Empty);
            }
        }

        [Test]
        public void RuntimeConfig_EmptyConfigDisables()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                fixture.Config.FootMarkerSprite = null;
                fixture.Config.FootMarkerAnimationFrames = null;
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.False);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerSpriteForSelfCheck, Is.Null);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerAnimationFramesForSelfCheck, Is.Empty);
            }
        }

        [TestCase(0f)]
        [TestCase(-0.1f)]
        public void RuntimeConfig_InvalidDurationUsesDefault(float duration)
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                fixture.Config.FootMarkerAnimationFrameDurationSeconds = duration;
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.True);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerAnimationFrameDurationSecondsForSelfCheck,
                    Is.EqualTo(BattleFootMarkerAnimation.DefaultFrameDurationSeconds));
            }
        }

        [Test]
        public void RuntimeConfig_ExplicitDisabledAuthoringWins()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                fixture.SetExplicitAuthoring(null, BattleFootMarkerStyle.Default);
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.False);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerSpriteForSelfCheck, Is.SameAs(fixture.FirstSprite));
            }
        }

        [Test]
        public void RuntimeConfig_ExplicitAuthoringWins()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                var style = new BattleFootMarkerStyle(64f, 24f, new Vector2(3f, -4f), new Color32(11, 22, 33, 44));
                fixture.SetExplicitAuthoring(fixture.SecondSprite, style);
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.True);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerSpriteForSelfCheck, Is.SameAs(fixture.SecondSprite));
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerAnimationFramesForSelfCheck, Is.Empty);
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.SizePixels, Is.EqualTo(style.SizePixels));
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.OffsetPixels, Is.EqualTo(style.OffsetPixels));
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkerStyleForSelfCheck.Tint, Is.EqualTo(style.Tint));
                Assert.That(BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0d), Is.SameAs(fixture.SecondTexture));
            }
        }

        [Test]
        public void RuntimeConfig_TextureSamplingAllocatesZeroAfterWarmup()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(BattleCentralRenderSystem.RuntimeFootMarkersEnabledForSelfCheck, Is.True);
                for (int index = 0; index < 64; index++)
                    BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(index * 0.033d);
                long beforeBytes = System.GC.GetAllocatedBytesForCurrentThread();
                Texture sampled = null;
                for (int index = 0; index < 4096; index++)
                    sampled = BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(index * 0.033d);
                long allocatedBytes = System.GC.GetAllocatedBytesForCurrentThread() - beforeBytes;
                Assert.That(sampled, Is.Not.Null);
                Assert.That(allocatedBytes, Is.Zero, "Texture selection only; not a complete render-path 0GC certificate.");
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessHoldsBothFramesBeforeSampling()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Texture2D[] held = ReadPreparedFootTextures();
                Assert.That(held, Is.EqualTo(new[] { fixture.FirstTexture, fixture.SecondTexture }));
                Assert.That(ReadPreparedFootReferenceTexture(), Is.SameAs(fixture.FirstTexture));
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessReusesArrayForUnchangedCount()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Texture2D[] held = ReadPreparedFootTextures();
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(ReadPreparedFootTextures(), Is.SameAs(held));
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessRefreshesInPlaceFrameReplacement()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Texture2D[] held = ReadPreparedFootTextures();
                fixture.Config.FootMarkerAnimationFrames[1] = null;
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(ReadPreparedFootTextures(), Is.SameAs(held));
                Assert.That(held[1], Is.Null);
                Assert.That(BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0.126d),
                    Is.SameAs(fixture.FirstTexture));
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessReleasesOldFramesOnShorterConfig()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                ReadPreparedFootTextures();
                fixture.Config.FootMarkerAnimationFrames = new[] { fixture.SecondSprite };
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(ReadPreparedFootTextures(), Is.EqualTo(new[] { fixture.SecondTexture }));
                Assert.That(ReadPreparedFootReferenceTexture(), Is.SameAs(fixture.SecondTexture));
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessHoldsFallbackWhenFramesEmpty()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                fixture.Config.FootMarkerAnimationFrames = System.Array.Empty<Sprite>();
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(ReadPreparedFootTextures(), Is.Empty);
                Assert.That(ReadPreparedFootReferenceTexture(), Is.SameAs(fixture.FirstTexture));
                Assert.That(BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(0.126d),
                    Is.SameAs(fixture.FirstTexture));
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessReleasesReferencesWhenConfigRemoved()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                ReadPreparedFootTextures();
                fixture.BindConfig(null);
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(ReadPreparedFootTextures(), Is.Empty);
                Assert.That(ReadPreparedFootReferenceTexture(), Is.Null);
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessStillHonorsExplicitAuthoring()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                fixture.SetExplicitAuthoring(fixture.SecondSprite, BattleFootMarkerStyle.Default);
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Assert.That(ReadPreparedFootTextures(), Is.Empty);
                Assert.That(ReadPreparedFootReferenceTexture(), Is.SameAs(fixture.SecondTexture));
            }
        }

        [Test]
        public void RuntimeConfig_ReadinessRepeatedSamplingKeepsStorage()
        {
            using (var fixture = new RuntimeConfigFixture())
            {
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
                Texture2D[] held = ReadPreparedFootTextures();
                System.GC.Collect();
                long beforeBytes = System.GC.GetAllocatedBytesForCurrentThread();
                Texture selected = null;
                for (int index = 0; index < 4096; index++)
                    selected = BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture(index * 0.033d);
                long allocatedBytes = System.GC.GetAllocatedBytesForCurrentThread() - beforeBytes;
                Assert.That(allocatedBytes, Is.Zero, "Only selection; the complete camera window is a separate gate.");
                Assert.That(selected, Is.Not.Null);
                Assert.That(ReadPreparedFootTextures(), Is.SameAs(held));
            }
        }

        private static Texture2D[] ReadPreparedFootTextures()
        {
            var field = typeof(BattleCentralRenderSystem).GetField("runtimeFootMarkerAnimationTextures",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.That(field, Is.Not.Null, "All animation textures must be acquired and held before camera sampling.");
            return (Texture2D[])field.GetValue(null);
        }

        private static Texture2D ReadPreparedFootReferenceTexture()
        {
            var field = typeof(BattleCentralRenderSystem).GetField("runtimeFootMarkerReferenceTexture",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.That(field, Is.Not.Null, "The fallback/reference texture must remain held too.");
            return (Texture2D)field.GetValue(null);
        }

        private sealed class RuntimeConfigFixture : System.IDisposable
        {
            private readonly System.Reflection.FieldInfo instanceField;
            private readonly GameConfig originalConfig;
            private readonly Material material;
            private readonly BattleRenderFeature feature;
            private GameObject previewObject;
            private System.IDisposable validationScope;

            internal readonly GameConfig Config;
            internal readonly Texture2D FirstTexture;
            internal readonly Texture2D SecondTexture;
            internal readonly Sprite FirstSprite;
            internal readonly Sprite SecondSprite;

            internal RuntimeConfigFixture()
            {
                Assert.That(BattleCentralEditorPreview.TryGetRuntimeFootMarkerAuthoringSettings(
                    out _, out _, out _, out _, out _), Is.False,
                    "This production-config test requires no loaded authoring; never remove scene objects to satisfy it.");
                instanceField = typeof(GameConfig).GetField("_instance",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                Assert.That(instanceField, Is.Not.Null);
                originalConfig = (GameConfig)instanceField.GetValue(null);
                FirstTexture = NewTexture(128, 48);
                SecondTexture = NewTexture(128, 48);
                FirstSprite = NewSprite(FirstTexture);
                SecondSprite = NewSprite(SecondTexture);
                Config = ScriptableObject.CreateInstance<GameConfig>();
                Config.hideFlags = HideFlags.HideAndDontSave;
                Config.FootMarkerSprite = FirstSprite;
                Config.FootMarkerAnimationFrames = new[] { FirstSprite, SecondSprite };
                Config.FootMarkerAnimationFrameDurationSeconds = 0.125f;
                BindConfig(Config);
                material = NewCentralMaterial();
                feature = ScriptableObject.CreateInstance<BattleRenderFeature>();
                feature.hideFlags = HideFlags.HideAndDontSave;
                feature.Configure(material, BattleCentralDrawMode.OrderedChunks);
            }

            internal void BindConfig(GameConfig config)
            {
                instanceField.SetValue(null, config);
            }

            internal void SetExplicitAuthoring(Sprite sprite, in BattleFootMarkerStyle style)
            {
                previewObject = new GameObject("RuntimeConfigAuthoringOverride") { hideFlags = HideFlags.HideAndDontSave };
                var preview = previewObject.AddComponent<BattleCentralEditorPreview>();
                preview.ConfigureForSelfCheck(material, new BattleCentralEditorPreviewActor(), BattleHealthBarStyle.Default);
                preview.ConfigureFootMarkerForSelfCheck(sprite, style);
                validationScope = BattleCentralEditorPreview.BeginExclusiveValidationForSelfCheck(preview);
            }

            public void Dispose()
            {
                validationScope?.Dispose();
                if (previewObject != null)
                    Object.DestroyImmediate(previewObject);
                BindConfig(originalConfig);
                BattleCentralRenderSystem.UnregisterFeature(feature);
                Object.DestroyImmediate(feature);
                Object.DestroyImmediate(Config);
                Object.DestroyImmediate(FirstSprite);
                Object.DestroyImmediate(SecondSprite);
                Object.DestroyImmediate(FirstTexture);
                Object.DestroyImmediate(SecondTexture);
                Object.DestroyImmediate(material);
                BattleCentralRenderSystem.RefreshRuntimeFootMarkerAuthoringSettings();
            }
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
