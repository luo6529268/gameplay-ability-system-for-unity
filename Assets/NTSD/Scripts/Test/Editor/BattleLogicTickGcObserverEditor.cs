#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Threading;
using NTSD.Animation.Rendering.Editor;
using UnityEngine;
#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
#endif

namespace NTSD.Test.Editor
{
    internal sealed class BattleLogicTickGcObserverEditor : IDisposable
    {
        [Serializable]
        internal sealed class Evidence
        {
            public string scope = "Entire synchronous main-thread SimulationTickDriver.StepOneTick; original final steady-sample policy";
            public string marker;
            public string unit;
            public int capacity;
            public BattleScopedGcAllocationRecorder.CalibrationResult before;
            public BattleScopedGcAllocationRecorder.CalibrationResult after;
            public int begunTicks;
            public int endedTicks;
            public int acceptedTicks;
            public int steadyScopes;
            public int nonsteadyScopes;
            public int invalidScopes;
            public int rejectedTicks;
            public int sequenceErrors;
            public long steadyAllocationEvents;
            public long nonsteadyAllocationEvents;
            public long steadyRawValue;
            public long provenSteadyAllocatedBytes = -1;
            public int firstNonzeroSteadyTick;
            public int expectedTicks;
            public int expectedSamples;
            public bool coverageValid;
            public bool zeroAllocationPassed;
            public string status = "UNKNOWN / INCOMPLETE";
        }

        private readonly BattleScopedGcAllocationRecorder recorder;
        private readonly int ownerThread;
        private readonly Evidence evidence;
        private ProductionEntityStressRunner runner;
        private BattleScopedGcAllocationRecorder.ScopeResult pending;
        private bool active;
        private bool pendingReady;
        private bool recordingStarted;
        private bool disposed;

        internal BattleLogicTickGcObserverEditor(int capacity = BattleScopedGcAllocationRecorder.DefaultCapacity)
        {
            ownerThread = Thread.CurrentThread.ManagedThreadId;
            recorder = new BattleScopedGcAllocationRecorder(capacity);
            evidence = new Evidence { marker = recorder.Marker, unit = recorder.Unit, capacity = capacity };
            evidence.before = recorder.Calibrate();
        }

        internal void Attach(ProductionEntityStressRunner target)
        {
            RequireOwner();
            if (target == null || runner != null || !evidence.before.passed)
                throw new InvalidOperationException("A calibrated, unowned main-thread recorder is required.");
            target.ConfigureDriverTickObserverForDiagnostics(BeginDriverStep, EndDriverStep, AcceptCompletedTick);
            runner = target;
        }

        internal void BeginDriverStep()
        {
            RequireOwner();
            if (active || pendingReady)
                RejectSequence();
            recordingStarted = recorder.Begin();
            active = true;
            evidence.begunTicks++;
        }

        internal void EndDriverStep(bool stepped)
        {
            RequireOwner();
            if (!active)
                RejectSequence();
            pending = recordingStarted ? recorder.End() : default;
            active = false;
            pendingReady = true;
            evidence.endedTicks++;
            if (!stepped)
                evidence.rejectedTicks++;
        }

        internal void AcceptCompletedTick(int tick, bool sampled)
        {
            RequireOwner();
            if (!pendingReady || active || tick != evidence.acceptedTicks + 1)
                RejectSequence();
            evidence.acceptedTicks++;
            if (!pending.valid || !pending.calibratedBefore || pending.saturated || pending.wrapped)
                evidence.invalidScopes++;
            if (sampled)
            {
                evidence.steadyScopes++;
                evidence.steadyAllocationEvents += pending.allocationEvents;
                evidence.steadyRawValue += pending.rawValueTotal;
                if (pending.allocationEvents != 0 && evidence.firstNonzeroSteadyTick == 0)
                    evidence.firstNonzeroSteadyTick = tick;
            }
            else
            {
                evidence.nonsteadyScopes++;
                evidence.nonsteadyAllocationEvents += pending.allocationEvents;
            }
            pendingReady = false;
        }

        internal Evidence Finish(int expectedTicks, int expectedSamples)
        {
            RequireOwner();
            evidence.expectedTicks = expectedTicks;
            evidence.expectedSamples = expectedSamples;
            evidence.coverageValid = expectedTicks > 0 && expectedSamples > 0 &&
                !active && !pendingReady && evidence.sequenceErrors == 0 && evidence.rejectedTicks == 0 &&
                evidence.begunTicks == expectedTicks && evidence.endedTicks == expectedTicks &&
                evidence.acceptedTicks == expectedTicks && evidence.steadyScopes == expectedSamples &&
                evidence.nonsteadyScopes + evidence.steadyScopes == expectedTicks;
            if (active)
            {
                recorder.End();
                active = false;
                evidence.invalidScopes++;
            }
            try
            {
                evidence.after = recorder.Calibrate();
                bool reliable = evidence.coverageValid && evidence.before.passed &&
                    evidence.after.passed && evidence.invalidScopes == 0;
                evidence.zeroAllocationPassed = reliable && evidence.steadyAllocationEvents == 0;
                evidence.provenSteadyAllocatedBytes = evidence.zeroAllocationPassed ? 0 : -1;
                evidence.status = !reliable ? "UNKNOWN / INVALID_CALIBRATION_OR_COVERAGE" :
                    evidence.zeroAllocationPassed ? "CALIBRATED_FULL_DRIVER_ZERO_EVENTS" :
                    "CALIBRATED_FULL_DRIVER_NONZERO_EVENTS";
                return evidence;
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            RequireOwner();
            try
            {
                if (runner != null)
                    runner.ConfigureDriverTickObserverForDiagnostics(null, null, null);
            }
            finally
            {
                runner = null;
                recorder.Dispose();
                active = false;
                disposed = true;
            }
        }

        private void RejectSequence()
        {
            evidence.sequenceErrors++;
            throw new InvalidOperationException("A full Driver allocation scope boundary was missing or repeated.");
        }

        private void RequireOwner()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleLogicTickGcObserverEditor));
            if (Thread.CurrentThread.ManagedThreadId != ownerThread)
                throw new InvalidOperationException("A Driver allocation observer is main-thread owned.");
        }
    }

#if UNITY_INCLUDE_TESTS
    public sealed class BattleLogicTickGcObserverEditorTests
    {
        private static int begins;
        private static int ends;
        private static int accepts;

        [Test]
        public void Runner_DefaultHookIsEmptyAndDetachIsIdempotent()
        {
            RequireHook();
            GameObject root = CreateRunner(out ProductionEntityStressRunner runner);
            try
            {
                Assert.That(HookAttached(runner), Is.False);
                Configure(runner, null, null, null);
                Configure(runner, null, null, null);
                Assert.That(HookAttached(runner), Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Runner_RejectsIncompleteCallbackGroup(int missing)
        {
            RequireHook();
            GameObject root = CreateRunner(out ProductionEntityStressRunner runner);
            try
            {
                AssertInvocationRejected(() => Configure(runner,
                    missing == 0 ? null : (Action)ProbeBegin,
                    missing == 1 ? null : (Action<bool>)ProbeEnd,
                    missing == 2 ? null : (Action<int, bool>)ProbeAccept));
                Assert.That(HookAttached(runner), Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void Runner_RejectsSecondOwnerAndAttachAfterFirstTick()
        {
            RequireHook();
            GameObject root = CreateRunner(out ProductionEntityStressRunner runner);
            try
            {
                Configure(runner, ProbeBegin, ProbeEnd, ProbeAccept);
                Assert.That(HookAttached(runner), Is.True);
                AssertInvocationRejected(() => Configure(runner, ProbeBegin, ProbeEnd, ProbeAccept));
                Configure(runner, null, null, null);
                runner.Report.logicTicksExecuted = 1;
                AssertInvocationRejected(() => Configure(runner, ProbeBegin, ProbeEnd, ProbeAccept));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void Runner_RejectsDedicatedWorkerScope()
        {
            RequireHook();
            GameObject root = CreateRunner(out ProductionEntityStressRunner runner, true);
            try { AssertInvocationRejected(() => Configure(runner, ProbeBegin, ProbeEnd, ProbeAccept)); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void CalibratedEmptyScope_ProvesZeroOnlyWithExactCoverage()
        {
            object observer = CreateObserver();
            try
            {
                Tick(observer, true, 1, true);
                object row = Finish(observer, 1, 1);
                Assert.That(Value<bool>(row, "zeroAllocationPassed"), Is.True);
                Assert.That(Value<long>(row, "steadyAllocationEvents"), Is.Zero);
                Assert.That(Value<long>(row, "provenSteadyAllocatedBytes"), Is.Zero);
                Assert.That(Value<bool>(row, "coverageValid"), Is.True);
                Assert.That(Value<int>(row, "begunTicks"), Is.EqualTo(1));
                Assert.That(Value<int>(row, "endedTicks"), Is.EqualTo(1));
                Assert.That(Value<int>(row, "acceptedTicks"), Is.EqualTo(1));
            }
            finally { ((IDisposable)observer).Dispose(); }
        }

        [Test]
        public void Runner_BeginFailurePreservesOriginalAndDoesNotCallEnd()
        {
            RequireHook();
            GameObject root = CreateRunner(out ProductionEntityStressRunner runner);
            try
            {
                Configure(runner, ProbeBeginFailure, ProbeEnd, ProbeAccept);
                MethodInfo step = typeof(ProductionEntityStressRunner).GetMethod(
                    "StepMeasuredTick", BindingFlags.Instance | BindingFlags.NonPublic);
                TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                    () => step.Invoke(runner, new object[] { true, false }));
                Assert.That(exception.InnerException.Message, Is.EqualTo("Begin failure sentinel"));
                Assert.That(ends, Is.Zero, "An unopened scope must not run a balancing End.");
                Configure(runner, null, null, null);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void Runner_DetachDuringInFlightScopeIsRejected()
        {
            RequireHook();
            GameObject root = CreateRunner(out ProductionEntityStressRunner runner);
            FieldInfo flight = typeof(ProductionEntityStressRunner).GetField(
                "driverTickObserverInFlight", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                Configure(runner, ProbeBegin, ProbeEnd, ProbeAccept);
                flight.SetValue(runner, true);
                AssertInvocationRejected(() => Configure(runner, null, null, null));
                Assert.That(HookAttached(runner), Is.True);
                flight.SetValue(runner, false);
                Configure(runner, null, null, null);
            }
            finally { flight.SetValue(runner, false); UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void ObserverAttachAndDispose_ReleaseRunnerOwnership()
        {
            RequireHook();
            GameObject root = CreateRunner(out ProductionEntityStressRunner runner);
            object observer = CreateObserver();
            object second = CreateObserver();
            try
            {
                MethodInfo attach = observer.GetType().GetMethod("Attach", BindingFlags.Instance | BindingFlags.NonPublic);
                attach.Invoke(observer, new object[] { runner });
                Assert.That(HookAttached(runner), Is.True);
                AssertInvocationRejected(() => attach.Invoke(second, new object[] { runner }));
                ((IDisposable)observer).Dispose();
                Assert.That(HookAttached(runner), Is.False);
                attach.Invoke(second, new object[] { runner });
                ((IDisposable)second).Dispose();
                Assert.That(HookAttached(runner), Is.False);
            }
            finally
            {
                ((IDisposable)observer).Dispose();
                ((IDisposable)second).Dispose();
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void KnownAllocation_IsNotFalseZeroOrPortableRawBytes()
        {
            object observer = CreateObserver();
            try
            {
                Action begin = Callback<Action>(observer, "BeginDriverStep");
                Action<bool> end = Callback<Action<bool>>(observer, "EndDriverStep");
                Action<int, bool> accept = Callback<Action<int, bool>>(observer, "AcceptCompletedTick");
                begin();
                byte[] live = new byte[65536];
                live[0] = 7;
                GC.KeepAlive(live);
                end(true);
                accept(1, true);
                object row = Finish(observer, 1, 1);
                Assert.That(Value<long>(row, "steadyAllocationEvents"), Is.GreaterThan(0));
                Assert.That(Value<bool>(row, "zeroAllocationPassed"), Is.False);
                Assert.That(Value<long>(row, "provenSteadyAllocatedBytes"), Is.EqualTo(-1));
                GC.KeepAlive(live);
            }
            finally { ((IDisposable)observer).Dispose(); }
        }

        [Test]
        public void WarmAndNonsteadyScopes_AreCountedButNotSteadyAllocation()
        {
            object observer = CreateObserver();
            try
            {
                Action begin = Callback<Action>(observer, "BeginDriverStep");
                Action<bool> end = Callback<Action<bool>>(observer, "EndDriverStep");
                Action<int, bool> accept = Callback<Action<int, bool>>(observer, "AcceptCompletedTick");
                begin();
                byte[] live = new byte[16384];
                GC.KeepAlive(live);
                end(true);
                accept(1, false);
                Tick(observer, true, 2, true);
                object row = Finish(observer, 2, 1);
                Assert.That(Value<int>(row, "nonsteadyScopes"), Is.EqualTo(1));
                Assert.That(Value<long>(row, "nonsteadyAllocationEvents"), Is.GreaterThan(0));
                Assert.That(Value<bool>(row, "zeroAllocationPassed"), Is.True);
                GC.KeepAlive(live);
            }
            finally { ((IDisposable)observer).Dispose(); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RefusedTickOrMissingSampleCoverage_CannotPass(bool wrongCoverage)
        {
            object observer = CreateObserver();
            try
            {
                Tick(observer, wrongCoverage, 1, true);
                object row = Finish(observer, wrongCoverage ? 2 : 1, 1);
                Assert.That(Value<bool>(row, "zeroAllocationPassed"), Is.False);
                Assert.That(Value<bool>(row, "coverageValid"), Is.False);
            }
            finally { ((IDisposable)observer).Dispose(); }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void InvalidSequence_IsRejectedAndCannotBecomeZero(int defect)
        {
            object observer = CreateObserver();
            try
            {
                Action begin = Callback<Action>(observer, "BeginDriverStep");
                Action<bool> end = Callback<Action<bool>>(observer, "EndDriverStep");
                Action<int, bool> accept = Callback<Action<int, bool>>(observer, "AcceptCompletedTick");
                if (defect == 0) { begin(); Assert.Throws<InvalidOperationException>(() => begin()); }
                if (defect == 1) Assert.Throws<InvalidOperationException>(() => end(true));
                if (defect == 2) Assert.Throws<InvalidOperationException>(() => accept(1, true));
                if (defect == 3) { begin(); end(true); Assert.Throws<InvalidOperationException>(() => begin()); }
            }
            finally { ((IDisposable)observer).Dispose(); }
        }

        [Test]
        public void FixedCapacityOverflow_IsInvalidAndNeverExpands()
        {
            object observer = CreateObserver(2);
            try
            {
                Action begin = Callback<Action>(observer, "BeginDriverStep");
                Action<bool> end = Callback<Action<bool>>(observer, "EndDriverStep");
                Action<int, bool> accept = Callback<Action<int, bool>>(observer, "AcceptCompletedTick");
                begin();
                for (int i = 0; i < 12; i++) GC.KeepAlive(new byte[64]);
                end(true);
                accept(1, true);
                object row = Finish(observer, 1, 1);
                Assert.That(Value<int>(row, "capacity"), Is.EqualTo(2));
                Assert.That(Value<int>(row, "invalidScopes"), Is.GreaterThan(0));
                Assert.That(Value<bool>(row, "zeroAllocationPassed"), Is.False);
            }
            finally { ((IDisposable)observer).Dispose(); }
        }

        [Test]
        public void OwnerThreadAndDisposedScope_AreRejected()
        {
            object observer = CreateObserver();
            Action begin = Callback<Action>(observer, "BeginDriverStep");
            Exception failure = null;
            var thread = new Thread(() => { try { begin(); } catch (Exception e) { failure = e; } });
            thread.Start();
            Assert.That(thread.Join(5000), Is.True);
            Assert.That(failure, Is.TypeOf<InvalidOperationException>());
            ((IDisposable)observer).Dispose();
            ((IDisposable)observer).Dispose();
            Assert.Throws<ObjectDisposedException>(() => begin());
        }

        [TestCase(0)]
        [TestCase(1)]
        public void ScopedRequest_PreservesOriginalProductionWorkload(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildLogicGcScopeRequest", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            var request = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            MethodInfo original = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteProductionRequest", BindingFlags.Static | BindingFlags.NonPublic);
            var baseline = (ProductionEntityStressRequest)original.Invoke(null, new object[] { index });
            Assert.That(request.outputPath, Does.Contain("BATCH54-LOGIC-GC-SCOPE"));
            request.outputPath = baseline.outputPath;
            Assert.That(JsonUtility.ToJson(request), Is.EqualTo(JsonUtility.ToJson(baseline)));
            Assert.That(request.warmupTicks, Is.EqualTo(120));
            Assert.That(request.sampleTicks, Is.EqualTo(180));
            Assert.That(request.useDedicatedSimulationWorker, Is.False);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void ScopedRequest_RejectsOutOfRangeIndex(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildLogicGcScopeRequest", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => method.Invoke(null, new object[] { index }));
            Assert.That(exception.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        private static void Tick(object observer, bool stepped, int tick, bool sampled)
        {
            Action begin = Callback<Action>(observer, "BeginDriverStep");
            Action<bool> end = Callback<Action<bool>>(observer, "EndDriverStep");
            Action<int, bool> accept = Callback<Action<int, bool>>(observer, "AcceptCompletedTick");
            begin();
            end(stepped);
            accept(tick, sampled);
        }

        private static object CreateObserver(int capacity = 256)
        {
            Type type = typeof(BattleLogicTickGcObserverEditorTests).Assembly.GetType(
                "NTSD.Test.Editor.BattleLogicTickGcObserverEditor");
            Assert.That(type, Is.Not.Null, "Reliable full-driver observer has not been implemented.");
            ConstructorInfo ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(int) }, null);
            Assert.That(ctor, Is.Not.Null);
            return ctor.Invoke(new object[] { capacity });
        }

        private static T Callback<T>(object observer, string name) where T : Delegate
        {
            MethodInfo method = observer.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (T)Delegate.CreateDelegate(typeof(T), observer, method);
        }

        private static object Finish(object observer, int ticks, int samples) =>
            observer.GetType().GetMethod("Finish", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(observer, new object[] { ticks, samples });

        private static T Value<T>(object row, string field) =>
            (T)row.GetType().GetField(field).GetValue(row);

        private static MethodInfo RequireHook()
        {
            MethodInfo method = typeof(ProductionEntityStressRunner).GetMethod(
                "ConfigureDriverTickObserverForDiagnostics", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, "No complete-driver GC scope hook exists.");
            return method;
        }

        private static void Configure(ProductionEntityStressRunner runner, Action begin,
            Action<bool> end, Action<int, bool> accept) =>
            RequireHook().Invoke(runner, new object[] { begin, end, accept });

        private static bool HookAttached(ProductionEntityStressRunner runner) =>
            (bool)typeof(ProductionEntityStressRunner).GetProperty(
                "DriverTickObserverAttachedForDiagnostics").GetValue(runner);

        private static GameObject CreateRunner(out ProductionEntityStressRunner runner, bool dedicated = false)
        {
            var root = new GameObject("Logic GC scope fixture") { hideFlags = HideFlags.HideAndDontSave };
            runner = root.AddComponent<ProductionEntityStressRunner>();
            Type runnerType = typeof(ProductionEntityStressRunner);
            runnerType.GetField("cleaned", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(runner, true);
            runnerType.GetField("report", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                runner, new ProductionEntityStressReport());
            Type configType = runnerType.Assembly.GetType(
                "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
            var request = new ProductionEntityStressRequest { useDedicatedSimulationWorker = dedicated };
            object config = configType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { request, System.IO.Path.GetFullPath(Application.dataPath + "/..") });
            runnerType.GetField("config", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(runner, config);
            begins = ends = accepts = 0;
            return root;
        }

        private static void AssertInvocationRejected(TestDelegate action)
        {
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(action);
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        private static void ProbeBegin() { begins++; }
        private static void ProbeBeginFailure() { throw new InvalidOperationException("Begin failure sentinel"); }
        private static void ProbeEnd(bool stepped) { ends++; }
        private static void ProbeAccept(int tick, bool sampled) { accepts++; }
    }
#endif
}
#endif
