using NTSD.Simulation.Presentation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q09ReviveLivesGlyphAnchorEditorTests
    {
        [TestCase(2, "x2")]
        [TestCase(10, "x10")]
        [TestCase(100, "x100")]
        [TestCase(1000, "x999")]
        public void ReviveLives_UsesFormalFixedAnchorAndClampedGlyphs(int lives, string expected)
        {
            var slot = new BattleEntityOverlayRuntimeSlot(
                0, lives, 0, 0, 1, 0, 100, 20, 30, 0, 0, 6);
            var labels = new char[
                BattleEntityOverlayLayout.SlotCount,
                BattleEntityOverlayLayout.SlotLabelCharacterCapacity];
            var states = new int[BattleEntityOverlayLayout.SlotCount];
            var glyphs = new BattleEntityOverlayGlyph[
                BattleEntityOverlayLayout.MaximumGlyphCount];

            Assert.That(BattleEntityOverlayLayout.TryBuild(
                in slot, labels, states, glyphs, out int count), Is.True);
            Assert.That(count, Is.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(glyphs[index].CharCode, Is.EqualTo(expected[index]));
                Assert.That(glyphs[index].Type, Is.EqualTo(BattleEntityOverlayGlyphType.Counter));
                Assert.That(glyphs[index].PixelX, Is.EqualTo(100 - 13 +
                    index * BattleEntityOverlayLayout.GlyphAdvance));
                Assert.That(glyphs[index].PixelY, Is.EqualTo(20 + 30 - 6 - 7));
            }
        }
    }
}
