#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeRandomDirectCallTraceEditorTests
    {
        [Test]
        public void DiagnosticObserver_ReceivesDirectCallsWithAfterState()
        {
            var random = new NTSD28NativeRandom();
            random.ResetFromSeed(682973786u);
            var recorder = new Recorder();
            random.SetDiagnosticCallObserver(recorder);

            uint crtResult = random.CrtNext();
            int synchronizedResult = random.SynchronizedNext(0x82u, 2);

            Assert.That(recorder.CrtCalls, Has.Count.EqualTo(1));
            Assert.That(recorder.CrtCalls[0].Result, Is.EqualTo(crtResult));
            Assert.That(recorder.CrtCalls[0].TotalCalls, Is.EqualTo(3001UL));
            Assert.That(recorder.SynchronizedCalls, Has.Count.EqualTo(1));
            Assert.That(recorder.SynchronizedCalls[0].CallSite, Is.EqualTo(0x82u));
            Assert.That(recorder.SynchronizedCalls[0].UpperBound, Is.EqualTo(2));
            Assert.That(
                recorder.SynchronizedCalls[0].Result,
                Is.EqualTo(synchronizedResult));
            Assert.That(recorder.SynchronizedCalls[0].CounterAfter, Is.EqualTo(1));
            Assert.That(recorder.SynchronizedCalls[0].IndexAfter, Is.EqualTo(1));
            Assert.That(recorder.SynchronizedCalls[0].TotalCalls, Is.EqualTo(1UL));
        }

        [Test]
        public void NullDiagnosticObserver_LeavesResultsAndStateUnchanged()
        {
            var observed = new NTSD28NativeRandom();
            var plain = new NTSD28NativeRandom();
            observed.ResetFromSeed(123u);
            plain.ResetFromSeed(123u);
            var recorder = new Recorder();
            observed.SetDiagnosticCallObserver(recorder);
            observed.SetDiagnosticCallObserver(null);

            Assert.That(observed.CrtNext(), Is.EqualTo(plain.CrtNext()));
            Assert.That(
                observed.SynchronizedNext(0x91u, 31),
                Is.EqualTo(plain.SynchronizedNext(0x91u, 31)));
            Assert.That(recorder.CrtCalls, Is.Empty);
            Assert.That(recorder.SynchronizedCalls, Is.Empty);
            AssertScalarEqual(
                plain.CaptureScalarState(),
                observed.CaptureScalarState());
        }

        [Test]
        public void NullDiagnosticObserver_WarmDirectCallsAllocateZeroBytes()
        {
            var random = new NTSD28NativeRandom(0x55667788u);
            random.SetDiagnosticCallObserver(null);
            _ = random.CrtNext();
            _ = random.SynchronizedNext(0x82u, 2);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                _ = random.CrtNext();
                _ = random.SynchronizedNext(0x82u, 2);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static void AssertScalarEqual(
            NTSD28NativeRandomScalarState expected,
            NTSD28NativeRandomScalarState actual)
        {
            Assert.That(actual.CrtState, Is.EqualTo(expected.CrtState));
            Assert.That(actual.CrtCalls, Is.EqualTo(expected.CrtCalls));
            Assert.That(actual.SynchronizedCounter,
                Is.EqualTo(expected.SynchronizedCounter));
            Assert.That(actual.SynchronizedIndex,
                Is.EqualTo(expected.SynchronizedIndex));
            Assert.That(actual.SynchronizedCalls,
                Is.EqualTo(expected.SynchronizedCalls));
            Assert.That(actual.LastSynchronizedCallSite,
                Is.EqualTo(expected.LastSynchronizedCallSite));
        }

        private sealed class Recorder : INTSD28NativeRandomCallObserver
        {
            internal readonly List<NTSD28NativeCrtCall> CrtCalls = new();
            internal readonly List<NTSD28NativeSynchronizedCall>
                SynchronizedCalls = new();

            public void OnCrtNext(NTSD28NativeCrtCall call)
            {
                CrtCalls.Add(call);
            }

            public void OnSynchronizedNext(
                NTSD28NativeSynchronizedCall call)
            {
                SynchronizedCalls.Add(call);
            }
        }
    }
}
#endif
