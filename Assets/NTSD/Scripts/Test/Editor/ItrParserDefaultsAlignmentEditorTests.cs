#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("ItrParserDefaultsAlignment")]
    public sealed class ItrParserDefaultsAlignmentEditorTests
    {
        [Test]
        public void InteractionAreaDefaultZwidthMatchesRelease()
        {
            Assert.That(new InteractionArea().zwidth, Is.EqualTo(15));
        }

        [Test]
        public void ConverterUsesReleaseDefaultAndPreservesExplicitZwidth()
        {
            InteractionArea absent = ConvertSingleItr(BuildItr());
            InteractionArea explicitFifteen = ConvertSingleItr(BuildItr(
                new Lf2DatProperty("zwidth", "15")));
            InteractionArea explicitZero = ConvertSingleItr(BuildItr(
                new Lf2DatProperty("zwidth", "0")));
            InteractionArea explicitNegative = ConvertSingleItr(BuildItr(
                new Lf2DatProperty("zwidth", "-9")));

            Assert.That(absent.zwidth, Is.EqualTo(15));
            Assert.That(explicitFifteen.zwidth, Is.EqualTo(15));
            Assert.That(explicitZero.zwidth, Is.Zero);
            Assert.That(explicitNegative.zwidth, Is.EqualTo(-9));
        }

        [Test]
        public void OneValueActionPairsLeaveSecondaryScalarAtZero()
        {
            InteractionArea itr = ConvertSingleItr(BuildItr(
                new Lf2DatProperty("catchingact", "341"),
                new Lf2DatProperty("caughtact", "130")));

            Assert.That(itr.catchingact, Is.EqualTo(new[] { 341, 0 }));
            Assert.That(itr.caughtact, Is.EqualTo(new[] { 130, 0 }));
        }

        [Test]
        public void TwoValueActionPairsPreserveBothScalars()
        {
            InteractionArea itr = ConvertSingleItr(BuildItr(
                new Lf2DatProperty("catchingact", "341 342"),
                new Lf2DatProperty("caughtact", "130,131")));

            Assert.That(itr.catchingact, Is.EqualTo(new[] { 341, 342 }));
            Assert.That(itr.caughtact, Is.EqualTo(new[] { 130, 131 }));
        }

        [Test]
        public void EmptyPairsRemainAbsent()
        {
            InteractionArea itr = ConvertSingleItr(BuildItr(
                new Lf2DatProperty("catchingact", " "),
                new Lf2DatProperty("caughtact", "\t")));

            Assert.That(itr.catchingact, Is.Null);
            Assert.That(itr.caughtact, Is.Null);
        }

        [Test]
        public void UnityOnlySecondaryTagsKeepLegacyParsingForLaterAdmissionPackage()
        {
            InteractionArea itr = ConvertSingleItr(BuildItr(
                new Lf2DatProperty("catchingact2", "7"),
                new Lf2DatProperty("caughtact2", "8 9")));

            Assert.That(itr.catchingact2, Is.EqualTo(new[] { 7, 0 }));
            Assert.That(itr.caughtact2, Is.EqualTo(new[] { 8, 9 }));
        }

        private static Lf2DatSubBlock BuildItr(params Lf2DatProperty[] properties)
        {
            var block = new Lf2DatSubBlock { Name = "itr" };
            for (int index = 0; index < properties.Length; index++)
                block.AddProperty(properties[index]);
            return block;
        }

        private static InteractionArea ConvertSingleItr(Lf2DatSubBlock itr)
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 91 };
            frameBlock.SubBlocks.Add(itr);
            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);
            Assert.That(frame.itrs, Has.Count.EqualTo(1));
            return frame.itrs[0];
        }
    }
}
#endif

