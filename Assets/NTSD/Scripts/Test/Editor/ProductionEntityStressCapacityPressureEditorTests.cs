#if UNITY_EDITOR
using NUnit.Framework;
using NTSD.Animation.Rendering.Editor;

namespace NTSD.Test.Editor
{
    // The gate samples counters only after warmup so setup capacity growth is allowed.
    public sealed class ProductionEntityStressCapacityPressureEditorTests
    {
        [Test]
        public void StableCounters_RecordWithoutAllocatingOrFailingGate()
        {
            var accumulator = new ProductionEntityStressCapacityPressureAccumulator();
            var report = new ProductionEntityStressCapacityPressureReport();
            var snapshot = new ProductionEntityStressCapacityPressureSnapshot
            {
                ObjectPoolObjectFetchReject = 3,
                InputEventReject = 5,
            };
            accumulator.Begin(in snapshot, report, required: true);
            accumulator.Record(121, in snapshot, report);

            long before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 122; tick < 378; tick++)
                accumulator.Record(tick, in snapshot, report);
            long allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.EqualTo(0L));
            Assert.That(report.passed, Is.True);
            Assert.That(report.violatingTickCount, Is.EqualTo(0));
            Assert.That(report.totalRejectedOrDroppedDelta, Is.EqualTo(0L));
        }

        [Test]
        public void CriticalDeltaFailsGate_WhilePresentationThrottleIsReportedSeparately()
        {
            var accumulator = new ProductionEntityStressCapacityPressureAccumulator();
            var report = new ProductionEntityStressCapacityPressureReport();
            var baseline = new ProductionEntityStressCapacityPressureSnapshot();
            accumulator.Begin(in baseline, report, required: true);
            var current = new ProductionEntityStressCapacityPressureSnapshot
            {
                ObjectPoolObjectFetchReject = 2,
                OneShotVoiceLimitDrop = 1,
            };

            accumulator.Record(121, in current, report);

            Assert.That(report.passed, Is.False);
            Assert.That(report.violatingTickCount, Is.EqualTo(1));
            Assert.That(report.firstViolatingLogicTick, Is.EqualTo(121));
            Assert.That(report.totalRejectedOrDroppedDelta, Is.EqualTo(3L));
            Assert.That(report.capacityCriticalDelta, Is.EqualTo(2L));
            Assert.That(report.presentationThrottleDelta, Is.EqualTo(1L));
            Assert.That(report.presentationThrottleTickCount, Is.EqualTo(1));
            Assert.That(report.firstPresentationThrottleLogicTick, Is.EqualTo(121));
            Assert.That(report.objectPoolObjectFetchRejectDelta, Is.EqualTo(2L));
            Assert.That(report.oneShotVoiceLimitDropDelta, Is.EqualTo(1L));
            Assert.That(report.firstViolationSourceMask & (1UL << 13), Is.Not.Zero);
            Assert.That(report.firstViolationSourceMask & (1UL << 25), Is.Zero);
            Assert.That(
                report.firstPresentationThrottleSourceMask & (1UL << 25),
                Is.Not.Zero);
        }

        [Test]
        public void PresentationThrottleAlone_DoesNotFailMemoryCapacityGate()
        {
            var accumulator = new ProductionEntityStressCapacityPressureAccumulator();
            var report = new ProductionEntityStressCapacityPressureReport();
            var baseline = new ProductionEntityStressCapacityPressureSnapshot();
            accumulator.Begin(in baseline, report, required: true);
            var current = new ProductionEntityStressCapacityPressureSnapshot
            {
                OneShotVoiceLimitDrop = 4,
            };

            accumulator.Record(121, in current, report);

            Assert.That(report.passed, Is.True);
            Assert.That(report.violatingTickCount, Is.Zero);
            Assert.That(report.capacityCriticalDelta, Is.Zero);
            Assert.That(report.presentationThrottleDelta, Is.EqualTo(4L));
            Assert.That(report.totalRejectedOrDroppedDelta, Is.EqualTo(4L));
        }

        [Test]
        public void CounterReset_IsRebasedAndLaterIncrementIsStillDetected()
        {
            var accumulator = new ProductionEntityStressCapacityPressureAccumulator();
            var report = new ProductionEntityStressCapacityPressureReport();
            var baseline = new ProductionEntityStressCapacityPressureSnapshot
            {
                InputEventReject = 5,
            };
            accumulator.Begin(in baseline, report, required: true);
            var reset = new ProductionEntityStressCapacityPressureSnapshot
            {
                InputEventReject = 0,
            };
            accumulator.Record(121, in reset, report);
            Assert.That(report.passed, Is.True);

            var afterReset = new ProductionEntityStressCapacityPressureSnapshot
            {
                InputEventReject = 1,
            };
            accumulator.Record(122, in afterReset, report);

            Assert.That(report.passed, Is.False);
            Assert.That(report.inputEventRejectDelta, Is.EqualTo(1L));
            Assert.That(report.firstViolatingLogicTick, Is.EqualTo(122));
        }
    }
}
#endif
