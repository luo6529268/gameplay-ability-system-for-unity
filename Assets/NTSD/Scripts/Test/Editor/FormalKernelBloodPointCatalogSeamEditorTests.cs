#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("FormalKernelBloodPointCatalogSeam")]
    public sealed class FormalKernelBloodPointCatalogSeamEditorTests
    {
        [Test]
        public void ValueUsesExactReleaseOrderDefaultEqualityAndHash()
        {
            var value = new BattleBloodPointValue(17, -29);
            Assert.That(value.X, Is.EqualTo(17));
            Assert.That(value.Y, Is.EqualTo(-29));
            Assert.That(default(BattleBloodPointValue),
                Is.EqualTo(new BattleBloodPointValue(0, 0)));
            Assert.That(value, Is.EqualTo(new BattleBloodPointValue(17, -29)));
            Assert.That(value.GetHashCode(),
                Is.EqualTo(new BattleBloodPointValue(17, -29).GetHashCode()));
            Assert.That(value, Is.Not.EqualTo(new BattleBloodPointValue(18, -29)));
            Assert.That(value, Is.Not.EqualTo(new BattleBloodPointValue(17, -28)));
        }

        [Test]
        public void CatalogDefensivelyCopiesAndPreservesOrderedDuplicates()
        {
            var source = new List<BattleBloodPointValue>
            {
                new BattleBloodPointValue(1, 2),
                new BattleBloodPointValue(1, 2),
                new BattleBloodPointValue(-3, -4),
            };
            var catalog = new BattleBloodPointCatalog(source);
            source[0] = new BattleBloodPointValue(99, 99);
            source.Reverse();

            Assert.That(catalog.Count, Is.EqualTo(3));
            Assert.That(catalog[0], Is.EqualTo(new BattleBloodPointValue(1, 2)));
            Assert.That(catalog[1], Is.EqualTo(catalog[0]));
            Assert.That(catalog[2], Is.EqualTo(new BattleBloodPointValue(-3, -4)));
        }

        [Test]
        public void EmptyAndSingleZeroRemainDistinct()
        {
            var empty = new BattleBloodPointCatalog(
                Array.Empty<BattleBloodPointValue>());
            var oneZero = new BattleBloodPointCatalog(new[]
            {
                default(BattleBloodPointValue),
            });

            Assert.That(empty.Count, Is.Zero);
            Assert.That(empty.TryGetPrimary(out _), Is.False);
            Assert.That(oneZero.Count, Is.EqualTo(1));
            Assert.That(oneZero.TryGetPrimary(out BattleBloodPointValue primary),
                Is.True);
            Assert.That(primary, Is.EqualTo(default(BattleBloodPointValue)));
        }

        [Test]
        public void CanonicalScalarWriterUsesCountThenSourceOrderedXY()
        {
            var catalog = new BattleBloodPointCatalog(new[]
            {
                new BattleBloodPointValue(1, -2),
                new BattleBloodPointValue(int.MinValue, int.MaxValue),
            });
            var destination = new int[7];
            destination[0] = 111;
            destination[6] = 222;

            int written = catalog.CopyCanonicalScalars(destination, 1);

            Assert.That(written, Is.EqualTo(5));
            Assert.That(destination, Is.EqualTo(new[]
            {
                111, 2, 1, -2, int.MinValue, int.MaxValue, 222,
            }));
        }

        [Test]
        public void ConverterPreservesEveryBlockAndExposesFirstPrimaryAlias()
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 18 };
            frameBlock.SubBlocks.Add(BuildBPoint(1, 2));
            frameBlock.SubBlocks.Add(BuildBPoint(-3, -4));

            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);

            Assert.That(frame.BloodPoints.Count, Is.EqualTo(2));
            Assert.That(frame.BloodPoints[0],
                Is.EqualTo(new BattleBloodPointValue(1, 2)));
            Assert.That(frame.BloodPoints[1],
                Is.EqualTo(new BattleBloodPointValue(-3, -4)));
            Assert.That(frame.TryGetPrimaryBloodPoint(out BattleBloodPointValue primary),
                Is.True);
            Assert.That(primary, Is.EqualTo(frame.BloodPoints[0]));
            Assert.That(frame.bpoint, Is.Not.Null);
            Assert.That(frame.bpoint.x, Is.EqualTo(1));
            Assert.That(frame.bpoint.y, Is.EqualTo(2));
        }

        [Test]
        public void ConverterDistinguishesEmptyFromOneExplicitZero()
        {
            LF2FrameData empty = Lf2DatConverter.ConvertToFrameData(
                new Lf2FrameBlock { FrameIndex = 19 });
            var zeroBlock = new Lf2FrameBlock { FrameIndex = 20 };
            zeroBlock.SubBlocks.Add(BuildBPoint(0, 0));
            LF2FrameData oneZero = Lf2DatConverter.ConvertToFrameData(zeroBlock);

            Assert.That(empty.BloodPoints.Count, Is.Zero);
            Assert.That(empty.TryGetPrimaryBloodPoint(out _), Is.False);
            Assert.That(empty.bpoint, Is.Null);
            Assert.That(oneZero.BloodPoints.Count, Is.EqualTo(1));
            Assert.That(oneZero.TryGetPrimaryBloodPoint(out _), Is.True);
            Assert.That(oneZero.bpoint, Is.Not.Null);
        }

        [Test]
        public void ConverterRejectsEveryUnknownExplicitProperty()
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 21 };
            Lf2DatSubBlock bpoint = BuildBPoint(1, 2);
            bpoint.AddProperty(new Lf2DatProperty("unknown", "0"));
            frameBlock.SubBlocks.Add(bpoint);

            Assert.Throws<InvalidOperationException>(() =>
                Lf2DatConverter.ConvertToFrameData(frameBlock));
        }

        private static Lf2DatSubBlock BuildBPoint(int x, int y)
        {
            var block = new Lf2DatSubBlock { Name = "bpoint" };
            block.AddProperty(new Lf2DatProperty("x", x.ToString()));
            block.AddProperty(new Lf2DatProperty("y", y.ToString()));
            return block;
        }
    }
}
#endif
