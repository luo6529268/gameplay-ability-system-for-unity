#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    // Alignment contract: NTSD-M12-TRANSPARENT-TRIM-001.
    public sealed class M12SpriteTrimEditorTests
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private T Track<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        internal static BattleRenderCommand Command(BattleSpriteEntry entry, bool flipX, bool flipY, Color32 color)
        {
            return new BattleRenderCommand(BattleRenderCommandType.Entity,
                new RuntimeEntityHandle(0, 1), 1, entry.Key.VisualDataId, entry.Key.EffectivePic,
                0, 0, 0, 0, 0, Vector3.zero,
                new Vector2(entry.PixelWidth, entry.PixelHeight), entry.Pivot, entry.NormalizedUv,
                new BattleSpriteRenderState(color, flipX, flipY, SpriteMaskInteraction.None,
                    BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha),
                new BattleSpriteValueDescriptor(true, entry.LegacySprite != null,
                    entry.LegacySprite != null ? entry.LegacySprite.GetInstanceID() : 0,
                    entry.SharedTexture.GetInstanceID(), 0, entry.PixelRect, entry.Pivot,
                    BattleVisualResourceKey.FromEntity(entry.Key)));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void CroppedLegacyAndCentralGeometryRetainNativeCanvasAndMirroring(bool flipX, bool flipY)
        {
            var texture = Track(new Texture2D(8, 8, TextureFormat.RGBA32, false));
            var pixelRect = new Rect(1, 1, 3, 5);
            var content = new Rect(8, 3, 3, 5);
            var canvas = new Vector2(12, 10);
            var legacy = Track(Sprite.Create(texture, pixelRect, new Vector2(-2f / 3f, -3f / 5f),
                100f, 0, SpriteMeshType.FullRect));
            Assert.That(legacy.pivot.x, Is.EqualTo(-2f).Within(0.00001f));
            Assert.That(legacy.pivot.y, Is.EqualTo(-3f).Within(0.00001f));
            var builder = new BattleSpriteCatalogBuilder();
            builder.Add(71, 0, "trim-fixture", texture, pixelRect, new Vector2(.5f, 0), legacy, canvas, content);
            var catalog = builder.Publish();
            Assert.That(catalog.TryGet(71, 0, out var entry), Is.True);
            Assert.That(entry.PixelWidth, Is.EqualTo(12));
            Assert.That(entry.PixelHeight, Is.EqualTo(10));
            Assert.That(entry.Pivot, Is.EqualTo(new Vector2(.5f, 0)));
            Assert.That(entry.ContentRect, Is.EqualTo(content));
            var rebound = entry.WithCentralBinding(entry.CentralBinding);
            Assert.That(rebound.ContentRect, Is.EqualTo(content));
            Assert.That(rebound.PixelWidth, Is.EqualTo(12));
            Assert.That(rebound.PixelHeight, Is.EqualTo(10));
            var material = Track(new Material(Shader.Find("NTSD/BattleCentralTransparent")));
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.Configure(catalog, material);
            var command = Command(entry, flipX, flipY, new Color32(255, 255, 255, 128));
            Assert.That(resolver.Resolve(command, out var resource), Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(resource.ContentRect, Is.EqualTo(content));
            var frame = new BattlePresentationFrame();
            frame.Reset(1);
            frame.AddCommand(command);
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(1);
            backend.Build(frame, resolver);
            Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(1));
            Assert.That(backend.GetChunkActiveQuadCount(0), Is.EqualTo(1));
            Vector3[] vertices = backend.GetChunkMesh(0).vertices;
            float sx = NTSDRenderSpace.UnitsPerPixelX * NTSDRenderSpace.BattleVisualScale;
            float sy = NTSDRenderSpace.UnitsPerPixelY * NTSDRenderSpace.BattleVisualScale;
            float left = (-6 + (flipX ? 1 : 8)) * sx;
            float bottom = (flipY ? 2 : 3) * sy;
            Assert.That(vertices[0].x, Is.EqualTo(left).Within(0.00001f));
            Assert.That(vertices[0].y, Is.EqualTo(bottom).Within(0.00001f));
            Assert.That(vertices[3].x, Is.EqualTo(left + 3 * sx).Within(0.00001f));
            Assert.That(vertices[3].y, Is.EqualTo(bottom + 5 * sy).Within(0.00001f));
            float legacyLeft = (flipX ? -legacy.bounds.max.x : legacy.bounds.min.x) * 100 * sx;
            float legacyBottom = (flipY ? -legacy.bounds.max.y : legacy.bounds.min.y) * 100 * sy;
            // SpriteRenderer flips around the original canvas anchor; central UV flips mirror the crop offset.
            Assert.That(legacyLeft, Is.EqualTo(left).Within(0.00001f));
            if (!flipY) Assert.That(legacyBottom, Is.EqualTo(bottom).Within(0.00001f));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void LegacyBodyMetricsNormalizeOnlyWhenActualSpriteMatchesTheCatalog(bool sameSprite)
        {
            var texture = Track(new Texture2D(8, 8));
            var rect = new Rect(1, 1, 3, 5);
            var sprite = Track(Sprite.Create(texture, rect, new Vector2(-2f / 3f, -3f / 5f),
                100f, 0, SpriteMeshType.FullRect));
            var entry = new BattleSpriteEntry(new BattleSpriteKey(71, 0), "trim", texture, rect,
                new Vector2(.5f, 0), sprite, new Vector2(12, 10), new Rect(8, 3, 3, 5));
            var actual = sameSprite ? sprite : Track(Sprite.Create(texture, rect, new Vector2(.5f, 0), 100f));
            Vector2 size = rect.size;
            Vector2 rawPivot = new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height);
            var descriptor = new BattleSpriteValueDescriptor(true, true, actual.GetInstanceID(),
                texture.GetInstanceID(), 0, rect, rawPivot, BattleVisualResourceKey.FromEntity(entry.Key));
            SimulationStageRenderModule.ResolveLegacyEntityCanvasMetrics(entry, actual, ref size, ref descriptor);
            Assert.That(size, Is.EqualTo(sameSprite ? new Vector2(12, 10) : rect.size));
            Assert.That(descriptor.PivotNormalized, Is.EqualTo(sameSprite ? entry.Pivot : rawPivot));
            Assert.That(descriptor.SpriteInstanceId, Is.EqualTo(actual.GetInstanceID()));
            Assert.That(descriptor.PixelRect, Is.EqualTo(rect));
        }

        [Test]
        public void NativeGeometryWithoutTrimRemainsFullCanvas()
        {
            var texture = Track(new Texture2D(8, 8));
            var entry = new BattleSpriteEntry(new BattleSpriteKey(71, 0), "native", texture,
                new Rect(1, 1, 3, 5), new Vector2(.5f, 0), null);
            Assert.That(entry.ContentRect, Is.EqualTo(new Rect(0, 0, 3, 5)));
            Assert.That(entry.PixelWidth, Is.EqualTo(3));
            Assert.That(entry.PixelHeight, Is.EqualTo(5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleSpriteEntry(entry.Key, "invalid", texture,
                entry.PixelRect, entry.Pivot, null, new Vector2(12, 10), new Rect(11, 3, 3, 5)));
        }
    }
}
#endif
