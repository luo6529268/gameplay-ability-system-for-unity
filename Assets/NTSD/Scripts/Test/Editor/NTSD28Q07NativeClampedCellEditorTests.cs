#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28Q07NativeClampedCell")]
    public sealed class NTSD28Q07NativeClampedCellEditorTests
    {
        [Test]
        public void PointClampSamplesEveryBoundaryWithoutChangingCellSize()
        {
            var red = new Color32(255, 0, 0, 255);
            var green = new Color32(0, 255, 0, 255);
            var blue = new Color32(0, 0, 255, 255);
            var clear = new Color32(0, 0, 0, 0);
            Color32[] source = { red, green, blue, clear }; // bottom row, then top row

            Assert.That(CharacterAnimtorManager.BuildNativeClampedCellPixels(
                source, 2, 2, -1, -1, 3, 3), Is.EqualTo(new[]
                { red, red, green, red, red, green, blue, blue, clear }));
            Assert.That(CharacterAnimtorManager.BuildNativeClampedCellPixels(
                source, 2, 2, 1, 1, 2, 2), Is.EqualTo(new[]
                { clear, clear, clear, clear }));
            Assert.That(CharacterAnimtorManager.BuildNativeClampedCellPixels(
                source, 2, 2, 0, 0, 2, 2), Is.EqualTo(source));
        }

        [Test]
        public void FormalHunterPic64MatchesPairedSourceWhiteCellAndDeduplicates()
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath,
                "NTSD/Content/LoganRuntime/vfs/m/nin/hun.png"));
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes(path)), Is.True);
                Assert.That(texture.width, Is.EqualTo(799));
                Assert.That(texture.height, Is.EqualTo(480));
                Color32[] pixels = texture.GetPixels32();
                Color32[] pic64 = CharacterAnimtorManager.BuildNativeClampedCellPixels(
                    pixels, 799, 480, 320, -79, 79, 79);
                Color32[] sameEdge = CharacterAnimtorManager.BuildNativeClampedCellPixels(
                    pixels, 799, 480, 320, -159, 79, 79);
                Assert.That(pic64, Has.Length.EqualTo(6241));
                foreach (Color32 pixel in pic64)
                    Assert.That(pixel, Is.EqualTo(new Color32(255, 255, 255, 255)));
                Assert.That(CharacterAnimtorManager.HashNativeClampedCellPixels(pic64, 79, 79),
                    Is.EqualTo(CharacterAnimtorManager.HashNativeClampedCellPixels(
                        sameEdge, 79, 79)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void CatalogUsesDerivedCellForLegacyAndCentralSourceBinding()
        {
            var normalTexture = new Texture2D(3, 1, TextureFormat.RGBA32, false);
            var derivedTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            Sprite normal = null;
            Sprite derived = null;
            try
            {
                normalTexture.Apply(false, false);
                derivedTexture.Apply(false, false);
                normal = Sprite.Create(normalTexture, new Rect(0, 0, 1, 1),
                    new Vector2(0.5f, 0f), 100f, 0, SpriteMeshType.FullRect);
                derived = Sprite.Create(derivedTexture, new Rect(0, 0, 1, 1),
                    new Vector2(0.5f, 0f), 100f, 0, SpriteMeshType.FullRect);
                var data = new LF2CharacterData();
                data.files.Add(new SpriteFileInfo("formal.png", 0, 1, 1, 1, 2, 1));
                var staged = new List<Sprite> { normal, derived };
                var configs = new Dictionary<int, LF2CharacterDataWrapper>
                {
                    [32] = new LF2CharacterDataWrapper(32, data),
                };
                var sprites = new Dictionary<int, List<Sprite>> { [32] = staged };
                var derivedPaths = new Dictionary<Sprite, string>
                {
                    [derived] = "virtual/native-clamp-cell.rgba",
                };

                BattleSpriteCatalog catalog = CharacterAnimtorManager.BuildBattleSpriteCatalog(
                    configs, sprites, derivedPaths);
                Assert.That(catalog.TryGet(32, 0, out BattleSpriteEntry normalEntry), Is.True);
                Assert.That(normalEntry.SourceSheetPath, Is.EqualTo("formal.png"));
                Assert.That(catalog.TryGet(32, 1, out BattleSpriteEntry derivedEntry), Is.True);
                Assert.That(derivedEntry.SourceSheetPath,
                    Is.EqualTo("virtual/native-clamp-cell.rgba"));
                Assert.That(derivedEntry.LegacySprite, Is.SameAs(derived));
                Assert.That(derivedEntry.SharedTexture, Is.SameAs(derivedTexture));
                Assert.That(derivedEntry.CentralBinding.IsValid, Is.True);
                Assert.That(derivedEntry.PixelWidth, Is.EqualTo(1f));
                Assert.That(derivedEntry.PixelHeight, Is.EqualTo(1f));
            }
            finally
            {
                if (normal != null) UnityEngine.Object.DestroyImmediate(normal);
                if (derived != null) UnityEngine.Object.DestroyImmediate(derived);
                UnityEngine.Object.DestroyImmediate(normalTexture);
                UnityEngine.Object.DestroyImmediate(derivedTexture);
            }
        }
    }
}
#endif
