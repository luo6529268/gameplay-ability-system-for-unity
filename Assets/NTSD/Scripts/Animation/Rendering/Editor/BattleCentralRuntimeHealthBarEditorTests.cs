#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCentralRuntimeHealthBarEditorTests
    {
        [Test]
        public void EntitySnapshot_HealthValuesSurviveResolvedAndOrderedCopies()
        {
            var source = new BattlePresentationEntitySnapshot(
                RuntimeEntityHandle.Invalid,
                7,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                true,
                0,
                0,
                0,
                1,
                0,
                0,
                0,
                0f,
                0f,
                0,
                0,
                0f,
                0f,
                79f,
                79f,
                Vector2.zero,
                Rect.zero,
                new Vector2(0.5f, 0f),
                false,
                true,
                default,
                0,
                0,
                true,
                true,
                Vector2.zero,
                0,
                null,
                true,
                125,
                300,
                500);

            BattlePresentationEntitySnapshot resolved = source.WithResolvedSprite(
                81f,
                82f,
                new Rect(0f, 0f, 0.5f, 0.5f),
                new Vector2(0.4f, 0.1f),
                true,
                default,
                null);
            BattlePresentationEntitySnapshot ordered =
                resolved.WithPresentationBaseOrder(44);

            Assert.That(ordered.ShowOverheadHealthBar, Is.True);
            Assert.That(ordered.CurrentHealth, Is.EqualTo(125));
            Assert.That(ordered.RecoverableHealth, Is.EqualTo(300));
            Assert.That(ordered.MaximumHealth, Is.EqualTo(500));
            Assert.That(ordered.PresentationBaseOrder, Is.EqualTo(44));
        }

        [Test]
        public void PresentationCapture_CopiesLiveCharacterHealthValues()
        {
            var world = new SimulationWorld();
            var character = CreateCharacter();
            try
            {
                world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                world.Register(character);
                character.Health.HP = 125;
                character.Health.HPBound = 300;
                character.Health.HP3 = 500;

                world.RenderDispatchAll(9);

                BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
                Assert.That(frame, Is.Not.Null);
                Assert.That(frame.EntityCount, Is.EqualTo(1));
                BattlePresentationEntitySnapshot snapshot = frame.GetEntity(0);
                Assert.That(snapshot.ShowOverheadHealthBar, Is.True);
                Assert.That(snapshot.CurrentHealth, Is.EqualTo(125));
                Assert.That(snapshot.RecoverableHealth, Is.EqualTo(300));
                Assert.That(snapshot.MaximumHealth, Is.EqualTo(500));
                Assert.That(
                    snapshot.StableHealthAnchorHeightPixels,
                    Is.EqualTo(BattleHealthBarAnchor.DefaultCharacterHeightPixels));
            }
            finally
            {
                world.ResetRuntimeState();
            }
        }

        [Test]
        public void RenderFeature_NewInstanceEnablesDefaultRuntimeHealthStyle()
        {
            BattleRenderFeature feature = ScriptableObject.CreateInstance<BattleRenderFeature>();
            try
            {
                Assert.That(feature.RuntimeHealthBarsEnabled, Is.True);
                Assert.That(
                    feature.RuntimeHealthBarStyle.WidthPixels,
                    Is.EqualTo(BattleHealthBarStyle.Default.WidthPixels));
                Assert.That(
                    feature.RuntimeHealthBarStyle.HeightPixels,
                    Is.EqualTo(BattleHealthBarStyle.Default.HeightPixels));
            }
            finally
            {
                Object.DestroyImmediate(feature);
            }
        }

        [Test]
        public void RuntimeStyle_UsesEditorPreviewAuthoringValues()
        {
            var previewObject = new GameObject("RuntimeHealthStylePreview")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            BattleCentralEditorPreview preview =
                previewObject.AddComponent<BattleCentralEditorPreview>();
            BattleRenderFeature feature = ScriptableObject.CreateInstance<BattleRenderFeature>();
            var actor = new BattleCentralEditorPreviewActor();
            var style = new BattleHealthBarStyle(
                96f,
                11f,
                2f,
                13f,
                new Vector2(4f, 5f),
                new Color32(1, 2, 3, 4),
                new Color32(5, 6, 7, 8),
                new Color32(9, 10, 11, 12));
            System.IDisposable validationScope = null;
            try
            {
                preview.ConfigureForSelfCheck(null, actor, style);
                validationScope =
                    BattleCentralEditorPreview.BeginExclusiveValidationForSelfCheck(preview);
                feature.Create();
                BattleCentralRenderSystem.RefreshRuntimeHealthBarAuthoringSettings();

                Assert.That(
                    BattleCentralRenderSystem.RuntimeHealthBarsEnabledForSelfCheck,
                    Is.True);
                Assert.That(
                    BattleCentralRenderSystem.RuntimeHealthBarStyleForSelfCheck.WidthPixels,
                    Is.EqualTo(96f));
                Assert.That(
                    BattleCentralRenderSystem.RuntimeHealthBarStyleForSelfCheck.HeightPixels,
                    Is.EqualTo(11f));
                Assert.That(
                    BattleCentralRenderSystem.RuntimeHealthBarStyleForSelfCheck.OffsetPixels,
                    Is.EqualTo(new Vector2(4f, 5f)));
            }
            finally
            {
                validationScope?.Dispose();
                BattleCentralRenderSystem.UnregisterFeature(feature);
                Object.DestroyImmediate(feature);
                Object.DestroyImmediate(previewObject);
                BattleCentralRenderSystem.RefreshRuntimeHealthBarAuthoringSettings();
            }
        }

        [Test]
        public void RuntimeHealthBackend_UsesEntityCommandSpriteTopAndOneSubMesh()
        {
            var frame = new BattlePresentationFrame();
            var backend = new BattleHealthBarBatchBackend();
            try
            {
                frame.Reset(12);
                frame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Shadow,
                    new Vector3(1f, 2f, 3f),
                    true,
                    10,
                    20,
                    100));
                frame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Entity,
                    new Vector3(10f, 20f, 3f),
                    true,
                    40,
                    70,
                    100));
                frame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Entity,
                    new Vector3(30f, 40f, 4f),
                    false,
                    90,
                    90,
                    100));

                backend.BuildFromFrame(frame, BattleHealthBarStyle.Default, true);

                Assert.That(backend.BuiltFrame, Is.SameAs(frame));
                Assert.That(backend.ActiveBarCount, Is.EqualTo(1));
                Assert.That(backend.ActiveQuadCount, Is.EqualTo(3));
                Assert.That(backend.Mesh, Is.Not.Null);
                Assert.That(backend.Mesh.subMeshCount, Is.EqualTo(1));
                Assert.That(backend.Mesh.GetSubMesh(0).indexCount, Is.EqualTo(18));

                float spriteTop = 20f + 50f * NTSDRenderSpace.UnitsPerPixelY *
                                  NTSDRenderSpace.BattleVisualScale;
                float expectedBottom = spriteTop +
                                       BattleHealthBarStyle.Default.HeadGapPixels *
                                       NTSDRenderSpace.UnitsPerPixelY;
                Assert.That(
                    backend.GetVertexPosition(0).y,
                    Is.EqualTo(expectedBottom).Within(0.0001f));
            }
            finally
            {
                backend.Dispose();
                frame.ReleasePublicationBinding();
            }
        }

        [Test]
        public void RuntimeHealthBackend_DisabledBuildPublishesEmptyCurrentFrame()
        {
            var frame = new BattlePresentationFrame();
            var backend = new BattleHealthBarBatchBackend();
            try
            {
                frame.Reset(3);
                frame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Entity,
                    Vector3.zero,
                    true,
                    100,
                    100,
                    100));

                backend.BuildFromFrame(frame, BattleHealthBarStyle.Default, false);

                Assert.That(backend.BuiltFrame, Is.SameAs(frame));
                Assert.That(backend.ActiveBarCount, Is.Zero);
                Assert.That(backend.Mesh, Is.Null);
            }
            finally
            {
                backend.Dispose();
                frame.ReleasePublicationBinding();
            }
        }

        [Test]
        public void RuntimeHealthBackend_SubsequentFrameUpdatesCurrentWidth()
        {
            var firstFrame = new BattlePresentationFrame();
            var secondFrame = new BattlePresentationFrame();
            var backend = new BattleHealthBarBatchBackend();
            try
            {
                firstFrame.Reset(20);
                firstFrame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Entity,
                    Vector3.zero,
                    true,
                    100,
                    100,
                    100));
                secondFrame.Reset(21);
                secondFrame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Entity,
                    Vector3.zero,
                    true,
                    25,
                    100,
                    100));

                backend.BuildFromFrame(firstFrame, BattleHealthBarStyle.Default, true);
                float fullCurrentRight = backend.GetVertexPosition(10).x;
                int firstMutationVersion = backend.MutationVersion;

                backend.BuildFromFrame(secondFrame, BattleHealthBarStyle.Default, true);
                float quarterCurrentRight = backend.GetVertexPosition(10).x;

                Assert.That(backend.BuiltFrame, Is.SameAs(secondFrame));
                Assert.That(backend.MutationVersion, Is.GreaterThan(firstMutationVersion));
                Assert.That(quarterCurrentRight, Is.LessThan(fullCurrentRight));
            }
            finally
            {
                backend.Dispose();
                firstFrame.ReleasePublicationBinding();
                secondFrame.ReleasePublicationBinding();
            }
        }

        [Test]
        public void RuntimeHealthBackend_StableAnchorIgnoresAnimatedSpriteRectChanges()
        {
            var firstFrame = new BattlePresentationFrame();
            var secondFrame = new BattlePresentationFrame();
            var backend = new BattleHealthBarBatchBackend();
            var stableAnchor = new Vector2(12f, 34f);
            try
            {
                firstFrame.Reset(30);
                firstFrame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Entity,
                    new Vector3(2f, 3f, 4f),
                    true,
                    100,
                    100,
                    100,
                    stableAnchor,
                    true,
                    new Vector2(45f, 60f),
                    new Vector2(0.1f, 0.2f)));
                secondFrame.Reset(31);
                secondFrame.AddCommand(CreateCommand(
                    BattleRenderCommandType.Entity,
                    new Vector3(8f, 9f, 4f),
                    true,
                    100,
                    100,
                    100,
                    stableAnchor,
                    true,
                    new Vector2(180f, 25f),
                    new Vector2(0.85f, 0.9f)));

                backend.BuildFromFrame(firstFrame, BattleHealthBarStyle.Default, true);
                Vector3 firstBottomLeft = backend.GetVertexPosition(0);
                backend.BuildFromFrame(secondFrame, BattleHealthBarStyle.Default, true);
                Vector3 secondBottomLeft = backend.GetVertexPosition(0);

                Assert.That(secondBottomLeft.x, Is.EqualTo(firstBottomLeft.x).Within(0.0001f));
                Assert.That(secondBottomLeft.y, Is.EqualTo(firstBottomLeft.y).Within(0.0001f));
            }
            finally
            {
                backend.Dispose();
                firstFrame.ReleasePublicationBinding();
                secondFrame.ReleasePublicationBinding();
            }
        }

        private static BattleRenderCommand CreateCommand(
            BattleRenderCommandType type,
            Vector3 position,
            bool showHealth,
            int currentHealth,
            int recoverableHealth,
            int maximumHealth,
            Vector2 stableHealthAnchorWorld = default,
            bool hasStableHealthAnchor = false,
            Vector2 size = default,
            Vector2 pivot = default)
        {
            if (size == default)
                size = new Vector2(100f, 50f);
            if (pivot == default)
                pivot = new Vector2(0.5f, 0f);
            return new BattleRenderCommand(
                type,
                RuntimeEntityHandle.Invalid,
                1,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                position,
                size,
                pivot,
                new Rect(0f, 0f, 1f, 1f),
                false,
                default,
                showHealth,
                currentHealth,
                recoverableHealth,
                maximumHealth,
                stableHealthAnchorWorld,
                hasStableHealthAnchor);
        }

        private static LF2Character CreateCharacter()
        {
            var frame = new LF2FrameData
            {
                pic = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = "RuntimeHealthCapture",
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = 1;
            character.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            return character;
        }
    }
}
#endif
