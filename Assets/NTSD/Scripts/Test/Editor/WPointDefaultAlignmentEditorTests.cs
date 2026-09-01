#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("WPointDefaultAlignment")]
    public sealed class WPointDefaultAlignmentEditorTests
    {
        [Test]
        public void WeaponPointDefaultKindMatchesRelease()
        {
            Assert.That(new WeaponPoint().kind, Is.Zero);
        }

        [Test]
        public void WeaponPointOtherScalarsKeepTheirExistingZeroDefaults()
        {
            var point = new WeaponPoint();

            Assert.That(new[]
            {
                point.x,
                point.y,
                point.w,
                point.h,
                point.weaponact,
                point.attacking,
                point.cover,
                point.dvx,
                point.dvy,
                point.dvz,
                point.injury,
                point.fall,
                point.vaction,
                point.arest,
                point.vrest,
                point.effect,
                point.kill,
                point.bdefend,
            }, Is.All.Zero);
            Assert.That(point.rawProperties, Is.Empty);
        }

        [Test]
        public void ConverterUsesReleaseDefaultWhenKindIsAbsent()
        {
            WeaponPoint point = ConvertSingleWPoint(BuildWPoint(
                new Lf2DatProperty("x", "17")));

            Assert.That(point.kind, Is.Zero);
            Assert.That(point.x, Is.EqualTo(17));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(-7)]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void ConverterPreservesEveryExplicitSignedKind(int kind)
        {
            WeaponPoint point = ConvertSingleWPoint(BuildWPoint(
                new Lf2DatProperty("kind", kind.ToString(
                    System.Globalization.CultureInfo.InvariantCulture))));

            Assert.That(point.kind, Is.EqualTo(kind));
        }

        [Test]
        public void EmptyListFallbackUsesReleaseDefaultWithoutMutatingContent()
        {
            var frame = new LF2FrameData();
            var buffers = new SimulationBattleBufferModule(4);

            Assert.That(frame.wpoints, Is.Empty);
            Assert.That(buffers.DefaultHeldObjectWeaponPoint.Kind, Is.Zero);
            Assert.That(frame.wpoints, Is.Empty);
        }

        private static Lf2DatSubBlock BuildWPoint(params Lf2DatProperty[] properties)
        {
            var block = new Lf2DatSubBlock { Name = "wpoint" };
            for (int index = 0; index < properties.Length; index++)
                block.AddProperty(properties[index]);
            return block;
        }

        private static WeaponPoint ConvertSingleWPoint(Lf2DatSubBlock wpoint)
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 73 };
            frameBlock.SubBlocks.Add(wpoint);
            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);
            Assert.That(frame.wpoints, Has.Count.EqualTo(1));
            return frame.wpoints[0];
        }
    }
}
#endif
