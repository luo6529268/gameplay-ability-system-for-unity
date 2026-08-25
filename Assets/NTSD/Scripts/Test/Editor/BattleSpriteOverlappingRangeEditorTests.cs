#if UNITY_EDITOR
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleSpriteOverlappingRangeEditorTests
    {
        [Test]
        [Category("BattleSpriteOverlappingRange")]
        public void FirstDeclaredOwnership_ExcludesLaterOverlappingPics()
        {
            var files = new List<SpriteFileInfo>
            {
                new SpriteFileInfo("first.bmp", 106, 120, 1, 1, 15, 1),
                new SpriteFileInfo("later.bmp", 112, 200, 1, 1, 89, 1),
            };

            List<HashSet<int>> ownership =
                CharacterAnimtorManager.BuildFirstDeclaredSpriteOwnership(files);

            Assert.That(ownership, Has.Count.EqualTo(2));
            Assert.That(ownership[0], Has.Count.EqualTo(15));
            Assert.That(ownership[0].Contains(106), Is.True);
            Assert.That(ownership[0].Contains(112), Is.True);
            Assert.That(ownership[0].Contains(120), Is.True);
            Assert.That(ownership[1], Has.Count.EqualTo(80));
            Assert.That(ownership[1].Contains(112), Is.False);
            Assert.That(ownership[1].Contains(120), Is.False);
            Assert.That(ownership[1].Contains(121), Is.True);
            Assert.That(ownership[1].Contains(200), Is.True);
        }

        [Test]
        [Category("BattleSpriteOverlappingRange")]
        public void Catalog_UsesFirstOwnerSheet_WhenStagingCompletesInReverseOrder()
        {
            Texture2D firstTexture = null;
            Texture2D laterTexture = null;
            Sprite firstSprite = null;
            Sprite laterSprite = null;
            try
            {
                firstTexture = CreateTexture(30, 2, "first-owner-texture");
                laterTexture = CreateTexture(178, 2, "later-owner-texture");
                firstSprite = Sprite.Create(
                    firstTexture,
                    new Rect(12f, 1f, 1f, 1f),
                    new Vector2(0.5f, 0f),
                    100f,
                    0,
                    SpriteMeshType.FullRect);
                laterSprite = Sprite.Create(
                    laterTexture,
                    new Rect(18f, 1f, 1f, 1f),
                    new Vector2(0.5f, 0f),
                    100f,
                    0,
                    SpriteMeshType.FullRect);

                var data = new LF2CharacterData();
                data.files.Add(new SpriteFileInfo("first.bmp", 106, 120, 1, 1, 15, 1));
                data.files.Add(new SpriteFileInfo("later.bmp", 112, 200, 1, 1, 89, 1));
                List<HashSet<int>> ownership =
                    CharacterAnimtorManager.BuildFirstDeclaredSpriteOwnership(data.files);
                var staged = new List<Sprite>(new Sprite[201]);

                // Model the real asynchronous completion order: the later DAT sheet
                // finishes first, but it cannot claim an effective pic owned by file 0.
                TryStage(staged, ownership[1], 112, laterSprite);
                TryStage(staged, ownership[1], 121, laterSprite);
                TryStage(staged, ownership[0], 112, firstSprite);

                Assert.That(staged[112], Is.SameAs(firstSprite));
                Assert.That(staged[121], Is.SameAs(laterSprite));

                var configs = new Dictionary<int, LF2CharacterDataWrapper>
                {
                    [56] = new LF2CharacterDataWrapper(56, data),
                };
                var sprites = new Dictionary<int, List<Sprite>>
                {
                    [56] = staged,
                };

                BattleSpriteCatalog catalog =
                    CharacterAnimtorManager.BuildBattleSpriteCatalog(configs, sprites);

                Assert.That(catalog.Count, Is.EqualTo(2));
                Assert.That(catalog.TryGet(56, 112, out BattleSpriteEntry firstEntry), Is.True);
                Assert.That(firstEntry.SourceSheetPath, Is.EqualTo("first.bmp"));
                Assert.That(firstEntry.SharedTexture, Is.SameAs(firstTexture));
                Assert.That(firstEntry.LegacySprite, Is.SameAs(firstSprite));
                Assert.That(catalog.TryGet(56, 121, out BattleSpriteEntry laterEntry), Is.True);
                Assert.That(laterEntry.SourceSheetPath, Is.EqualTo("later.bmp"));
                Assert.That(laterEntry.SharedTexture, Is.SameAs(laterTexture));
                Assert.That(laterEntry.LegacySprite, Is.SameAs(laterSprite));
            }
            finally
            {
                DestroyImmediate(firstSprite);
                DestroyImmediate(laterSprite);
                DestroyImmediate(firstTexture);
                DestroyImmediate(laterTexture);
            }
        }

        private static Texture2D CreateTexture(int width, int height, string name)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.Apply(false, false);
            return texture;
        }

        private static void TryStage(
            IList<Sprite> staged,
            ISet<int> ownedEffectivePics,
            int effectivePic,
            Sprite sprite)
        {
            if (ownedEffectivePics.Contains(effectivePic))
                staged[effectivePic] = sprite;
        }

        private static void DestroyImmediate(Object value)
        {
            if (value != null)
                Object.DestroyImmediate(value);
        }
    }
}
#endif
