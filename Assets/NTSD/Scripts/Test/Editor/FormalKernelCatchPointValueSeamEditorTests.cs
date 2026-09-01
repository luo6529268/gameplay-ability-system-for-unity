#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("FormalKernelCatchPointValueSeam")]
    public sealed class FormalKernelCatchPointValueSeamEditorTests
    {
        [Test]
        public void ValueUsesExactNineteenScalarOrderDefaultEqualityAndHash()
        {
            BattleCatchPointValue value = Value(1);

            Assert.That(value.Kind, Is.EqualTo(1));
            Assert.That(value.X, Is.EqualTo(2));
            Assert.That(value.Y, Is.EqualTo(3));
            Assert.That(value.Injury, Is.EqualTo(4));
            Assert.That(value.Cover, Is.EqualTo(5));
            Assert.That(value.Vaction, Is.EqualTo(6));
            Assert.That(value.Aaction, Is.EqualTo(7));
            Assert.That(value.Jaction, Is.EqualTo(8));
            Assert.That(value.Daction, Is.EqualTo(9));
            Assert.That(value.ThrowVx, Is.EqualTo(10));
            Assert.That(value.ThrowVy, Is.EqualTo(11));
            Assert.That(value.Hurtable, Is.EqualTo(12));
            Assert.That(value.Decrease, Is.EqualTo(13));
            Assert.That(value.DirControl, Is.EqualTo(14));
            Assert.That(value.Taction, Is.EqualTo(15));
            Assert.That(value.ThrowInjury, Is.EqualTo(16));
            Assert.That(value.ThrowVz, Is.EqualTo(17));
            Assert.That(value.FrontHurtAct, Is.EqualTo(18));
            Assert.That(value.BackHurtAct, Is.EqualTo(19));
            Assert.That(default(BattleCatchPointValue), Is.EqualTo(Value(0, 0)));
            Assert.That(value, Is.EqualTo(Value(1)));
            Assert.That(value.GetHashCode(), Is.EqualTo(Value(1).GetHashCode()));
            Assert.That(value, Is.Not.EqualTo(Value(2)));
        }

        [Test]
        public void CatalogDefensivelyCopiesPreservesDuplicatesAndWritesCanonicalOrder()
        {
            var source = new List<BattleCatchPointValue>
            {
                Value(1),
                Value(1),
                Value(-30),
            };
            var catalog = new BattleCatchPointCatalog(source);
            source[0] = Value(90);
            source.Reverse();

            Assert.That(catalog.Count, Is.EqualTo(3));
            Assert.That(catalog[0], Is.EqualTo(Value(1)));
            Assert.That(catalog[1], Is.EqualTo(catalog[0]));
            Assert.That(catalog[2], Is.EqualTo(Value(-30)));

            var scalars = new int[1 + 19 * 3];
            Assert.That(catalog.CopyCanonicalScalars(scalars, 0),
                Is.EqualTo(scalars.Length));
            Assert.That(scalars[0], Is.EqualTo(3));
            Assert.That(scalars[1], Is.EqualTo(1));
            Assert.That(scalars[19], Is.EqualTo(19));
            Assert.That(scalars[20], Is.EqualTo(1));
            Assert.That(scalars[39], Is.EqualTo(-30));
            Assert.That(scalars[57], Is.EqualTo(-12));
        }

        [Test]
        public void EmptyAndSingleZeroRemainDistinct()
        {
            var empty = new BattleCatchPointCatalog(
                Array.Empty<BattleCatchPointValue>());
            var oneZero = new BattleCatchPointCatalog(new[]
            {
                default(BattleCatchPointValue),
            });

            Assert.That(empty.TryGetPrimary(out _), Is.False);
            Assert.That(oneZero.Count, Is.EqualTo(1));
            Assert.That(oneZero.TryGetPrimary(out BattleCatchPointValue value),
                Is.True);
            Assert.That(value, Is.EqualTo(default(BattleCatchPointValue)));
        }

        [Test]
        public void ConverterPreservesEveryBlockAndExposesFirstPrimary()
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 82 };
            frameBlock.SubBlocks.Add(BuildCPoint(
                new Lf2DatProperty("kind", "1"),
                new Lf2DatProperty("x", "2"),
                new Lf2DatProperty("y", "3")));
            frameBlock.SubBlocks.Add(BuildCPoint(
                new Lf2DatProperty("kind", "2"),
                new Lf2DatProperty("x", "99"),
                new Lf2DatProperty("y", "100")));

            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);

            Assert.That(frame.CatchPoints.Count, Is.EqualTo(2));
            Assert.That(frame.CatchPoints[0].Kind, Is.EqualTo(1));
            Assert.That(frame.CatchPoints[1].Kind, Is.EqualTo(2));
            Assert.That(frame.TryGetPrimaryCatchPoint(out BattleCatchPointValue primary),
                Is.True);
            Assert.That(primary, Is.EqualTo(frame.CatchPoints[0]));
            Assert.That(frame.cpoint, Is.Not.Null);
            Assert.That(frame.cpoint.kind, Is.EqualTo(1));
            Assert.That(frame.cpoint.x, Is.EqualTo(2));
        }

        [Test]
        public void ConverterPreservesAliasProvenanceAndSourceOrderResolution()
        {
            LF2FrameData aliasThenCanonical = ConvertSingle(
                new Lf2DatProperty("fronthurtact", "230"),
                new Lf2DatProperty("injury", "310"),
                new Lf2DatProperty("backhurtact", "232"),
                new Lf2DatProperty("cover", "320"));
            LF2FrameData canonicalThenAlias = ConvertSingle(
                new Lf2DatProperty("injury", "310"),
                new Lf2DatProperty("fronthurtact", "230"),
                new Lf2DatProperty("cover", "320"),
                new Lf2DatProperty("backhurtact", "232"));

            BattleCatchPointValue first = aliasThenCanonical.CatchPoints[0];
            Assert.That(first.Injury, Is.EqualTo(310));
            Assert.That(first.Cover, Is.EqualTo(320));
            Assert.That(first.FrontHurtAct, Is.EqualTo(230));
            Assert.That(first.BackHurtAct, Is.EqualTo(232));

            BattleCatchPointValue second = canonicalThenAlias.CatchPoints[0];
            Assert.That(second.Injury, Is.EqualTo(230));
            Assert.That(second.Cover, Is.EqualTo(232));
            Assert.That(second.FrontHurtAct, Is.EqualTo(230));
            Assert.That(second.BackHurtAct, Is.EqualTo(232));
        }

        [Test]
        public void AdapterPreservesSignedValuesAndRejectsUnknownMetadata()
        {
            var legacy = new CatchPoint
            {
                kind = int.MinValue,
                x = -842150451,
                y = int.MaxValue,
                throwinjury = -842150451,
                throwvz = int.MinValue,
            };
            BattleCatchPointValue value =
                BattleCatchPointValueAdapter.FromLegacy(legacy);
            Assert.That(value.Kind, Is.EqualTo(int.MinValue));
            Assert.That(value.X, Is.EqualTo(-842150451));
            Assert.That(value.Y, Is.EqualTo(int.MaxValue));
            Assert.That(value.ThrowInjury, Is.EqualTo(-842150451));
            Assert.That(value.ThrowVz, Is.EqualTo(int.MinValue));

            legacy.rawProperties["unknown"] = "0";
            Assert.Throws<InvalidOperationException>(() =>
                BattleCatchPointValueAdapter.FromLegacy(legacy));
        }

        [Test]
        public void ConverterRejectsEveryUnknownExplicitProperty()
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 83 };
            frameBlock.SubBlocks.Add(BuildCPoint(
                new Lf2DatProperty("kind", "1"),
                new Lf2DatProperty("unknown", "0")));

            Assert.Throws<InvalidOperationException>(() =>
                Lf2DatConverter.ConvertToFrameData(frameBlock));
        }

        [Test]
        public void WarmedPrimaryReadAllocatesNoManagedMemory()
        {
            var frame = new LF2FrameData
            {
                cpoint = new CatchPoint { kind = 1, x = 2, y = 3 },
            };
            Assert.That(frame.TryGetPrimaryCatchPoint(out _), Is.True);
            long before = GC.GetAllocatedBytesForCurrentThread();
            int sum = 0;
            for (int iteration = 0; iteration < 1000; iteration++)
            {
                if (frame.TryGetPrimaryCatchPoint(out BattleCatchPointValue value))
                    sum += value.Kind;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(sum, Is.EqualTo(1000));
            Assert.That(allocated, Is.Zero);
        }

        private static BattleCatchPointValue Value(int start, int step = 1)
        {
            return new BattleCatchPointValue(
                start,
                start + step,
                start + step * 2,
                start + step * 3,
                start + step * 4,
                start + step * 5,
                start + step * 6,
                start + step * 7,
                start + step * 8,
                start + step * 9,
                start + step * 10,
                start + step * 11,
                start + step * 12,
                start + step * 13,
                start + step * 14,
                start + step * 15,
                start + step * 16,
                start + step * 17,
                start + step * 18);
        }

        private static LF2FrameData ConvertSingle(
            params Lf2DatProperty[] properties)
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 84 };
            frameBlock.SubBlocks.Add(BuildCPoint(properties));
            return Lf2DatConverter.ConvertToFrameData(frameBlock);
        }

        private static Lf2DatSubBlock BuildCPoint(
            params Lf2DatProperty[] properties)
        {
            var block = new Lf2DatSubBlock { Name = "cpoint" };
            for (int index = 0; index < properties.Length; index++)
                block.AddProperty(properties[index]);
            return block;
        }
    }
}
#endif
