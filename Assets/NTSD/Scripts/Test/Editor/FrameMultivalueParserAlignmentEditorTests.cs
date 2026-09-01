#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Linq;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("FrameMultivalueParserAlignment")]
    public sealed class FrameMultivalueParserAlignmentEditorTests
    {
        [Test]
        public void ParserAndConverterPreserveAllFourTwoValueItrProperties()
        {
            LF2FrameData frame = ParseFrame(@"
<frame> 0 pair
itr:
  catchingact: 100 101
  caughtact: 200 201
  catchingact2: 300 301
  caughtact2: 400 401
itr_end:
<frame_end>");

            Assert.That(frame.itrs.Count, Is.EqualTo(1));
            Assert.That(frame.itrs[0].catchingact, Is.EqualTo(new[] { 100, 101 }));
            Assert.That(frame.itrs[0].caughtact, Is.EqualTo(new[] { 200, 201 }));
            Assert.That(frame.itrs[0].catchingact2, Is.EqualTo(new[] { 300, 301 }));
            Assert.That(frame.itrs[0].caughtact2, Is.EqualTo(new[] { 400, 401 }));
        }

        [Test]
        public void OneValueKeepsSecondaryZeroAndDoesNotSwallowFollowingProperty()
        {
            LF2FrameData frame = ParseFrame(@"
<frame> 1 one
itr:
  catchingact: 120
  injury: 9
  caughtact: 130
  fall: 7
itr_end:
<frame_end>");

            Assert.That(frame.itrs[0].catchingact, Is.EqualTo(new[] { 120, 0 }));
            Assert.That(frame.itrs[0].caughtact, Is.EqualTo(new[] { 130, 0 }));
            Assert.That(frame.itrs[0].injury, Is.EqualTo(9));
            Assert.That(frame.itrs[0].fall, Is.EqualTo(7));
        }

        [Test]
        public void SignedAndOverflowTokensRetainExistingSaturationSemantics()
        {
            LF2FrameData frame = ParseFrame(@"
<frame> 2 signed
itr:
  catchingact: -2147483649 +2147483648
itr_end:
<frame_end>");

            Assert.That(frame.itrs[0].catchingact,
                Is.EqualTo(new[] { int.MinValue, int.MaxValue }));
        }

        [Test]
        public void NonDecimalTokenIsNotConsumedAsSecondValue()
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(@"
<frame> 3 text
itr:
  catchingact: 140 not-a-number
  injury: 11
itr_end:
<frame_end>");
            Lf2DatSubBlock itr = dat.Frames[0].SubBlocks[0];

            Assert.That(itr.Properties.Single(p => p.Key == "catchingact").Value,
                Is.EqualTo("140"));
            Assert.That(Lf2DatConverter.ConvertToFrameData(dat.Frames[0])
                    .itrs[0].injury,
                Is.EqualTo(11));
        }

        private static LF2FrameData ParseFrame(string text)
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(text);
            Assert.That(dat.Frames.Count, Is.EqualTo(1));
            return Lf2DatConverter.ConvertToFrameData(dat.Frames[0]);
        }
    }
}
#endif
