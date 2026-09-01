#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("CPointResolvedHurtActionAlignment")]
    public sealed class CPointResolvedHurtActionAlignmentEditorTests
    {
        [Test]
        public void ResolvedCanonicalValuesOverrideDifferentAliasMembers()
        {
            var cpoint = new CatchPoint
            {
                kind = 2,
                injury = 310,
                cover = 320,
                fronthurtact = 230,
                backhurtact = 232,
            };

            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: true),
                Is.EqualTo(310));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: false),
                Is.EqualTo(320));
        }

        [Test]
        public void LegacyAliasOnlyInputRemainsCompatibleAfterParserResolution()
        {
            CatchPoint cpoint = ConvertSingleCPoint(
                new Lf2DatProperty("fronthurtact", "230"),
                new Lf2DatProperty("backhurtact", "232"));

            Assert.That(cpoint.injury, Is.EqualTo(230));
            Assert.That(cpoint.cover, Is.EqualTo(232));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: true),
                Is.EqualTo(230));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: false),
                Is.EqualTo(232));
        }

        [Test]
        public void AliasBeforeCanonicalUsesLaterCanonicalResolvedValues()
        {
            CatchPoint cpoint = ConvertSingleCPoint(
                new Lf2DatProperty("fronthurtact", "230"),
                new Lf2DatProperty("injury", "310"),
                new Lf2DatProperty("backhurtact", "232"),
                new Lf2DatProperty("cover", "320"));

            Assert.That(cpoint.fronthurtact, Is.EqualTo(230));
            Assert.That(cpoint.backhurtact, Is.EqualTo(232));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: true),
                Is.EqualTo(310));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: false),
                Is.EqualTo(320));
        }

        [Test]
        public void CanonicalBeforeAliasUsesLaterAliasResolvedValues()
        {
            CatchPoint cpoint = ConvertSingleCPoint(
                new Lf2DatProperty("injury", "310"),
                new Lf2DatProperty("fronthurtact", "230"),
                new Lf2DatProperty("cover", "320"),
                new Lf2DatProperty("backhurtact", "232"));

            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: true),
                Is.EqualTo(230));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: false),
                Is.EqualTo(232));
        }

        [Test]
        public void ZeroResolvedActionRemainsZero()
        {
            var cpoint = new CatchPoint
            {
                kind = 2,
                injury = 0,
                cover = 0,
                fronthurtact = 230,
                backhurtact = 232,
            };

            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: true),
                Is.Zero);
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction(
                    cpoint,
                    oppositeFacing: false),
                Is.Zero);
        }

        private static CatchPoint ConvertSingleCPoint(
            params Lf2DatProperty[] properties)
        {
            var cpoint = new Lf2DatSubBlock { Name = "cpoint" };
            cpoint.AddProperty(new Lf2DatProperty("kind", "2"));
            for (int index = 0; index < properties.Length; index++)
                cpoint.AddProperty(properties[index]);

            var frame = new Lf2FrameBlock { FrameIndex = 81 };
            frame.SubBlocks.Add(cpoint);
            return Lf2DatConverter.ConvertToFrameData(frame).cpoint;
        }
    }
}
#endif
