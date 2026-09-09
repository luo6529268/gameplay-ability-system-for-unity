#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NUnit.Framework;

using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28NativeSparkLifecycleCoreEditorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(8, false)]
        [TestCase(9, true)]
        [TestCase(19, true)]
        [TestCase(29, true)]
        [TestCase(39, true)]
        [TestCase(99, true)]
        [TestCase(100, false)]
        [TestCase(109, false)]
        public void TerminalCell_RequiresNativeRangeAndLastDigitNine(
            int nativeSparkId,
            bool expected)
        {
            Assert.That(
                LF2Entity.IsNativeSparkTerminalCell(nativeSparkId),
                Is.EqualTo(expected));
        }

        [Test]
        public void Advance_PreservesNonTailTerminalAndPopsOnlyTerminalTail()
        {
            var entity = new SparkFixtureEntity();
            entity.AddHitRecord(9, 10, 20);
            entity.AddHitRecord(17, 30, 40);

            Assert.That(entity.AdvanceNativeSparkLifecycle(), Is.True);
            AssertAges(entity, 9, 18);
            Assert.That(entity.GetHitRecordX(0), Is.EqualTo(10));
            Assert.That(entity.GetHitRecordZ(0), Is.EqualTo(20));
            Assert.That(entity.GetHitRecordX(1), Is.EqualTo(30));
            Assert.That(entity.GetHitRecordZ(1), Is.EqualTo(40));

            Assert.That(entity.AdvanceNativeSparkLifecycle(), Is.True);
            AssertAges(entity, 9, 19);
            Assert.That(entity.AdvanceNativeSparkLifecycle(), Is.True);
            AssertAges(entity, 9);
            Assert.That(entity.AdvanceNativeSparkLifecycle(), Is.True);
            Assert.That(entity.HitRecordCount, Is.Zero);
        }

        [Test]
        public void Advance_RemovesAtMostOneTerminalTailPerPass()
        {
            var entity = new SparkFixtureEntity();
            entity.AddHitRecord(9, 10, 20);
            entity.AddHitRecord(19, 30, 40);
            entity.AddHitRecord(29, 50, 60);

            entity.AdvanceNativeSparkLifecycle();
            AssertAges(entity, 9, 19);
            entity.AdvanceNativeSparkLifecycle();
            AssertAges(entity, 9);
            entity.AdvanceNativeSparkLifecycle();
            Assert.That(entity.HitRecordCount, Is.Zero);
        }

        [Test]
        public void Advance_IncrementsOnlyNonTerminalNativeRange()
        {
            var entity = new SparkFixtureEntity();
            entity.AddHitRecord(-1, 1, 2);
            entity.AddHitRecord(0, 3, 4);
            entity.AddHitRecord(98, 5, 6);
            entity.AddHitRecord(100, 7, 8);

            entity.AdvanceNativeSparkLifecycle();

            AssertAges(entity, -1, 1, 99, 100);
            Assert.That(entity.GetHitRecordX(0), Is.EqualTo(1));
            Assert.That(entity.GetHitRecordZ(3), Is.EqualTo(8));
        }

        [Test]
        public void Advance_RetainsGroupTerminalForOneCadenceThenPopsIt()
        {
            var entity = new SparkFixtureEntity();
            entity.AddHitRecord(30, 50, 60);

            for (int cadence = 0; cadence < 9; cadence++)
                entity.AdvanceNativeSparkLifecycle();

            AssertAges(entity, 39);
            Assert.That(entity.AdvanceNativeSparkLifecycle(), Is.True);
            Assert.That(entity.HitRecordCount, Is.Zero);
        }

        [Test]
        public void Advance_DoesNotWriteLegacyPresentationTickGuard()
        {
            var entity = new SparkFixtureEntity();
            entity.AddHitRecord(0, 10, 20);
            int before = entity.GetHitRecordLastAdvanceTickForSnapshot(0);

            entity.AdvanceNativeSparkLifecycle();

            Assert.That(before, Is.EqualTo(int.MinValue));
            Assert.That(
                entity.GetHitRecordLastAdvanceTickForSnapshot(0),
                Is.EqualTo(before));
            AssertAges(entity, 1);
        }

        [Test]
        public void Advance_RemainsAllocationFreeForFullTenRecordOwners()
        {
            var entity = new SparkFixtureEntity();
            PopulateTen(entity);
            entity.AdvanceNativeSparkLifecycle();
            entity.ResetRecords();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int cadence = 0; cadence < 4096; cadence++)
            {
                PopulateTen(entity);
                if (entity.AdvanceNativeSparkLifecycle())
                    checksum ^= cadence;
                checksum ^= entity.GetHitRecordAge(9);
                entity.ResetRecords();
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(checksum, Is.Not.EqualTo(int.MinValue));
        }

        private static void PopulateTen(SparkFixtureEntity entity)
        {
            for (int index = 0; index < LF2Entity.MaxHitRecordSlots; index++)
                entity.AddHitRecord(index * 10, 100 + index, 200 + index);
        }

        private static void AssertAges(
            SparkFixtureEntity entity,
            params int[] expected)
        {
            Assert.That(entity.HitRecordCount, Is.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
                Assert.That(entity.GetHitRecordAge(index), Is.EqualTo(expected[index]));
        }

        private sealed class SparkFixtureEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Other;
            }

            public void ResetRecords()
            {
                ResetSpark();
            }

            public override void Reset()
            {
                ResetSpark();
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }
    }
}
#endif
