#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("FormalKernelWeaponPointValueSeam")]
    public sealed class FormalKernelWeaponPointValueSeamEditorTests
    {
        [Test]
        public void ValueUsesExactReleaseOrderAndZeroDefault()
        {
            Assert.That(default(BattleWeaponPointValue), Is.EqualTo(
                new BattleWeaponPointValue(0, 0, 0, 0, 0, 0, 0, 0, 0)));

            var value = new BattleWeaponPointValue(1, 2, 3, 4, 5, 6, 7, 8, 9);
            Assert.That(new[]
            {
                value.Kind,
                value.X,
                value.Y,
                value.Attacking,
                value.Cover,
                value.WeaponAct,
                value.Dvx,
                value.Dvy,
                value.Dvz,
            }, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void EqualityAndHashIncludeAllNineScalars()
        {
            var baseline = new BattleWeaponPointValue(1, 2, 3, 4, 5, 6, 7, 8, 9);
            Assert.That(baseline, Is.EqualTo(
                new BattleWeaponPointValue(1, 2, 3, 4, 5, 6, 7, 8, 9)));
            Assert.That(baseline.GetHashCode(), Is.EqualTo(
                new BattleWeaponPointValue(1, 2, 3, 4, 5, 6, 7, 8, 9).GetHashCode()));

            for (int scalar = 0; scalar < 9; scalar++)
            {
                int[] fields = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                fields[scalar]++;
                Assert.That(baseline, Is.Not.EqualTo(new BattleWeaponPointValue(
                    fields[0], fields[1], fields[2], fields[3], fields[4],
                    fields[5], fields[6], fields[7], fields[8])));
            }
        }

        [Test]
        public void LegacyProjectionCopiesOrderAndIsDefensive()
        {
            var first = BuildLegacy(1, 2, 3, 4, 5, 6, 7, 8, 9);
            var duplicate = BuildLegacy(1, 2, 3, 4, 5, 6, 7, 8, 9);
            var last = BuildLegacy(-1, -2, -3, -4, -5, -6, -7, -8, -9);
            var source = new List<WeaponPoint> { first, duplicate, last };

            BattleWeaponPointValue[] copy =
                BattleWeaponPointValueAdapter.CopyOrdered(source);
            first.kind = 99;
            source.Reverse();

            Assert.That(copy, Has.Length.EqualTo(3));
            Assert.That(copy[0], Is.EqualTo(copy[1]));
            Assert.That(copy[0].Kind, Is.EqualTo(1));
            Assert.That(copy[2].Kind, Is.EqualTo(-1));
        }

        [Test]
        public void PrimaryAccessorUsesFirstEntryOrLocalDefault()
        {
            var values = new List<BattleWeaponPointValue>
            {
                new BattleWeaponPointValue(3, 2, 1, 4, 5, 6, 7, 8, 9),
                new BattleWeaponPointValue(9, 8, 7, 6, 5, 4, 3, 2, 1),
            };

            Assert.That(BattleWeaponPointValueAdapter.PrimaryOrDefault(values),
                Is.EqualTo(values[0]));
            Assert.That(BattleWeaponPointValueAdapter.PrimaryOrDefault(
                Array.Empty<BattleWeaponPointValue>()), Is.EqualTo(default(BattleWeaponPointValue)));
            Assert.That(BattleWeaponPointValueAdapter.PrimaryOrDefault(null),
                Is.EqualTo(default(BattleWeaponPointValue)));
        }

        [Test]
        public void LegacyAdapterRejectsUnityExtrasAndUnknownProperties()
        {
            WeaponPoint explicitExtra = BuildLegacy(1, 2, 3, 4, 5, 6, 7, 8, 9);
            explicitExtra.rawProperties["w"] = "0";
            Assert.Throws<InvalidOperationException>(() =>
                BattleWeaponPointValueAdapter.FromLegacy(explicitExtra));

            WeaponPoint unknown = BuildLegacy(1, 2, 3, 4, 5, 6, 7, 8, 9);
            unknown.rawProperties["mystery"] = "0";
            Assert.Throws<InvalidOperationException>(() =>
                BattleWeaponPointValueAdapter.FromLegacy(unknown));

            WeaponPoint nonzeroDormantExtra = BuildLegacy(1, 2, 3, 4, 5, 6, 7, 8, 9);
            nonzeroDormantExtra.injury = 1;
            Assert.Throws<InvalidOperationException>(() =>
                BattleWeaponPointValueAdapter.FromLegacy(nonzeroDormantExtra));
        }

        [Test]
        public void ConverterPreservesSourceOrderAndRejectsNonReleaseTags()
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 44 };
            frameBlock.SubBlocks.Add(BuildParsedWPoint(1, 2));
            frameBlock.SubBlocks.Add(BuildParsedWPoint(-7, -8));
            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);

            Assert.That(frame.FormalWeaponPoints.Count, Is.EqualTo(2));
            Assert.That(frame.FormalWeaponPoints[0].Kind, Is.EqualTo(1));
            Assert.That(frame.FormalWeaponPoints[0].X, Is.EqualTo(2));
            Assert.That(frame.FormalWeaponPoints[1].Kind, Is.EqualTo(-7));
            Assert.That(frame.FormalWeaponPoints[1].X, Is.EqualTo(-8));

            var invalidFrame = new Lf2FrameBlock { FrameIndex = 45 };
            Lf2DatSubBlock invalid = BuildParsedWPoint(1, 2);
            invalid.AddProperty(new Lf2DatProperty("injury", "0"));
            invalidFrame.SubBlocks.Add(invalid);
            Assert.Throws<InvalidOperationException>(() =>
                Lf2DatConverter.ConvertToFrameData(invalidFrame));
        }

        [Test]
        public void PrimaryAccessorIsAllocationFreeWhenWarmed()
        {
            var values = new List<BattleWeaponPointValue>
            {
                new BattleWeaponPointValue(1, 2, 3, 4, 5, 6, 7, 8, 9),
            };
            int sink = BattleWeaponPointValueAdapter.PrimaryOrDefault(values).Kind;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
                sink ^= BattleWeaponPointValueAdapter.PrimaryOrDefault(values).Kind;
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(sink, Is.Not.EqualTo(int.MinValue));
            Assert.That(allocated, Is.Zero);
        }

        private static WeaponPoint BuildLegacy(
            int kind,
            int x,
            int y,
            int attacking,
            int cover,
            int weaponAct,
            int dvx,
            int dvy,
            int dvz)
        {
            return new WeaponPoint
            {
                kind = kind,
                x = x,
                y = y,
                attacking = attacking,
                cover = cover,
                weaponact = weaponAct,
                dvx = dvx,
                dvy = dvy,
                dvz = dvz,
            };
        }

        private static Lf2DatSubBlock BuildParsedWPoint(int kind, int x)
        {
            var block = new Lf2DatSubBlock { Name = "wpoint" };
            block.AddProperty(new Lf2DatProperty("kind", kind.ToString()));
            block.AddProperty(new Lf2DatProperty("x", x.ToString()));
            return block;
        }
    }
}
#endif
