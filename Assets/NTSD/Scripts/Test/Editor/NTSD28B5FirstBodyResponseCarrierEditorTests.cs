#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;
using System.Reflection;

using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    [Category("NTSD28B5FirstBodyResponseCarrier")]
    public sealed class NTSD28B5FirstBodyResponseCarrierEditorTests
    {
        [Test]
        public void Converter_CapturesOnlyFirstBodyKindAndRespondWithLastValueWins()
        {
            var frame = new Lf2FrameBlock { FrameIndex = 30 };
            var first = new Lf2DatSubBlock { Name = "bdy" };
            first.AddProperty(new Lf2DatProperty("kind", "1033"));
            first.AddProperty(new Lf2DatProperty("respond", "-1"));
            first.AddProperty(new Lf2DatProperty("kind", "1055"));
            first.AddProperty(new Lf2DatProperty("respond", "9"));
            first.AddProperty(new Lf2DatProperty("x", "21"));
            var second = new Lf2DatSubBlock { Name = "bdy" };
            second.AddProperty(new Lf2DatProperty("kind", "2000"));
            second.AddProperty(new Lf2DatProperty("respond", "77"));
            second.AddProperty(new Lf2DatProperty("x", "42"));
            frame.SubBlocks.Add(first);
            frame.SubBlocks.Add(second);

            LF2FrameData converted = Lf2DatConverter.ConvertToFrameData(frame);

            Assert.That(converted.PrimaryBodyKind, Is.EqualTo(1055));
            Assert.That(
                converted.primaryBodyKindForEffectSuppression,
                Is.EqualTo(1055));
            Assert.That(converted.PrimaryBodyRespond, Is.EqualTo(9));
            Assert.That(converted.bodies, Has.Count.EqualTo(2));
            Assert.That(converted.bodies[0].X, Is.EqualTo(21));
            Assert.That(converted.bodies[1].X, Is.EqualTo(42));
        }

        [Test]
        public void Converter_MissingFirstBodyRespondDefaultsToZero()
        {
            var frame = new Lf2FrameBlock { FrameIndex = 1 };
            var body = new Lf2DatSubBlock { Name = "bdy" };
            body.AddProperty(new Lf2DatProperty("kind", "1000"));
            frame.SubBlocks.Add(body);

            LF2FrameData converted = Lf2DatConverter.ConvertToFrameData(frame);

            Assert.That(converted.PrimaryBodyKind, Is.EqualTo(1000));
            Assert.That(converted.PrimaryBodyRespond, Is.Zero);
        }

        [Test]
        public void FormalBodyGeometry_RemainsExactFourScalarContract()
        {
            PropertyInfo[] properties = typeof(BattleBodyBoxValue).GetProperties(
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(properties, Has.Length.EqualTo(4));
            Assert.That(properties[0].Name, Is.EqualTo(nameof(BattleBodyBoxValue.X)));
            Assert.That(properties[1].Name, Is.EqualTo(nameof(BattleBodyBoxValue.Y)));
            Assert.That(properties[2].Name, Is.EqualTo(nameof(BattleBodyBoxValue.W)));
            Assert.That(properties[3].Name, Is.EqualTo(nameof(BattleBodyBoxValue.H)));
        }

        [Test]
        public void FrozenCriminalContent_ExposesTwentyTwoFirstBodyActionResponses()
        {
            string path = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Config",
                "chars",
                "criminal.dat");
            Lf2DatFile dat = new Lf2DatParserV2().Parse(
                File.ReadAllText(path),
                path);
            int responseFrameCount = 0;

            for (int index = 0; index < dat.Frames.Count; index++)
            {
                LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(
                    dat.Frames[index]);
                if (frame.PrimaryBodyKind >= 1000 &&
                    frame.PrimaryBodyKind < 2999)
                {
                    responseFrameCount++;
                    Assert.That(frame.PrimaryBodyRespond, Is.Zero);
                }
            }

            Assert.That(responseFrameCount, Is.EqualTo(22));
        }
    }
}
#endif
