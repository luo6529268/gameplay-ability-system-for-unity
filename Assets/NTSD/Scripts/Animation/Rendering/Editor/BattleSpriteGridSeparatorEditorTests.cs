#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleSpriteGridSeparatorEditorTests
    {
        [Test]
        public void LegacySheet_DeclaredGuttersBecomeTransparentWithoutChangingFrameContent()
        {
            const int textureWidth = 800;
            const int textureHeight = 560;
            var pixels = new Color32[textureWidth * textureHeight];
            var content = new Color32(120, 80, 40, 255);
            var separator = new Color32(0, 255, 0, 255);
            for (int y = 481; y < textureHeight; y++)
                for (int x = 0; x < 79; x++)
                    pixels[y * textureWidth + x] = content;
            for (int x = 0; x < textureWidth; x++)
                pixels[480 * textureWidth + x] = separator;
            for (int y = 0; y < textureHeight; y++)
                pixels[y * textureWidth + 79] = separator;
            RuntimeSpriteProcessor.ProcessSheetPixelsFast(pixels);

            var file = new SpriteFileInfo("synthetic-legacy-sheet.bmp", 0, 69, 79, 79, 10, 7);
            CharacterAnimtorManager.ResolveEffectiveGrid(
                file,
                textureWidth,
                textureHeight,
                out int rows,
                out int columns);
            Rect?[] rects = CharacterAnimtorManager.BuildIndexedSpriteRects(
                file,
                textureWidth,
                textureHeight,
                rows,
                columns);
            Assert.That(rects[0], Is.EqualTo(new Rect(0f, 481f, 79f, 79f)));

            Rect firstRect = rects[0].Value;
            int opaqueContentBefore = CountOpaquePixels(pixels, textureWidth, firstRect);
            Assert.That(opaqueContentBefore, Is.GreaterThan(0));
            int horizontalSeparatorY = Mathf.RoundToInt(firstRect.y) - 1;
            int opaqueGreenSeparatorBefore = CountOpaqueGreenRow(
                pixels,
                textureWidth,
                horizontalSeparatorY);
            Assert.That(opaqueGreenSeparatorBefore, Is.GreaterThan(600));

            RuntimeSpriteProcessor.ClearDetectedGridSeparatorAlpha(
                pixels,
                textureWidth,
                textureHeight);

            Assert.That(
                CountOpaquePixels(pixels, textureWidth, firstRect),
                Is.EqualTo(opaqueContentBefore));
            Assert.That(
                CountOpaquePixelsInRow(pixels, textureWidth, horizontalSeparatorY),
                Is.Zero);
            Assert.That(
                CountOpaquePixelsInColumn(pixels, textureWidth, textureHeight, 79),
                Is.Zero);
        }

        [Test]
        public void GridClear_DoesNotApplyGlobalGreenKeyInsideSpriteContent()
        {
            const int textureWidth = 160;
            const int textureHeight = 160;
            var pixels = new Color32[textureWidth * textureHeight];
            var green = new Color32(0, 255, 0, 255);
            pixels[40 * textureWidth + 40] = green;
            for (int x = 0; x < textureWidth; x++)
                pixels[80 * textureWidth + x] = green;
            for (int y = 0; y < textureHeight; y++)
                pixels[y * textureWidth + 79] = green;

            RuntimeSpriteProcessor.ClearDetectedGridSeparatorAlpha(
                pixels,
                textureWidth,
                textureHeight);

            Assert.That(pixels[40 * textureWidth + 40], Is.EqualTo(green));
            Assert.That(pixels[80 * textureWidth + 20].a, Is.Zero);
            Assert.That(pixels[20 * textureWidth + 79].a, Is.Zero);
        }

        private static int CountOpaquePixels(Color32[] pixels, int textureWidth, Rect rect)
        {
            int count = 0;
            int xMin = Mathf.RoundToInt(rect.xMin);
            int xMax = Mathf.RoundToInt(rect.xMax);
            int yMin = Mathf.RoundToInt(rect.yMin);
            int yMax = Mathf.RoundToInt(rect.yMax);
            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    if (pixels[y * textureWidth + x].a > 0)
                        count++;
                }
            }
            return count;
        }

        private static int CountOpaqueGreenRow(Color32[] pixels, int textureWidth, int y)
        {
            int count = 0;
            int rowStart = y * textureWidth;
            for (int x = 0; x < textureWidth; x++)
            {
                Color32 pixel = pixels[rowStart + x];
                if (pixel.a > 0 && pixel.g > 200 && pixel.r < 40 && pixel.b < 40)
                    count++;
            }
            return count;
        }

        private static int CountOpaquePixelsInRow(Color32[] pixels, int textureWidth, int y)
        {
            int count = 0;
            int rowStart = y * textureWidth;
            for (int x = 0; x < textureWidth; x++)
            {
                if (pixels[rowStart + x].a > 0)
                    count++;
            }
            return count;
        }

        private static int CountOpaquePixelsInColumn(
            Color32[] pixels,
            int textureWidth,
            int textureHeight,
            int x)
        {
            int count = 0;
            for (int y = 0; y < textureHeight; y++)
            {
                if (pixels[y * textureWidth + x].a > 0)
                    count++;
            }
            return count;
        }
    }
}
#endif
