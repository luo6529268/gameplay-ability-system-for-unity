#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
#if UNITY_INCLUDE_TESTS
using System.IO;
using NUnit.Framework;
using UnityEngine;
#endif

namespace NTSD.Test.Editor
{
    internal sealed class BattleScopedGcAllocationRecorder : IDisposable
    {
        internal const int DefaultCapacity = 256;
        private const int PositiveControlBytes = 1024 * 1024;
        private ProfilerRecorder recorder;
        private readonly int ownerThread;
        private bool active;
        private bool disposed;

        internal string Marker { get; private set; } = "GC.Alloc";
        internal string Category { get; private set; } = "Internal";
        internal string Unit { get; private set; } = "UNKNOWN";
        internal int Capacity { get; private set; }
        internal bool Calibrated { get; private set; }

        [Serializable]
        internal struct ScopeResult
        {
            public bool valid;
            public bool calibratedBefore;
            public bool saturated;
            public bool wrapped;
            public int sampleCount;
            public long allocationEvents;
            public long rawValueTotal;
        }

        [Serializable]
        internal struct CalibrationResult
        {
            public bool passed;
            public int knownLiveBytes;
            public ScopeResult positive;
            public ScopeResult empty;
        }

        internal BattleScopedGcAllocationRecorder(int capacity = DefaultCapacity)
        {
            if (capacity < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
            ownerThread = Thread.CurrentThread.ManagedThreadId;
            recorder = new ProfilerRecorder(ProfilerCategory.Internal, "GC.Alloc", capacity,
                ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
            if (recorder.Valid)
            {
                Unit = recorder.UnitType.ToString();
            }
            var handles = new List<ProfilerRecorderHandle>(256);
            ProfilerRecorderHandle.GetAvailable(handles);
            for (int i = 0; i < handles.Count; i++)
            {
                ProfilerRecorderDescription description = ProfilerRecorderHandle.GetDescription(handles[i]);
                if (description.Category == ProfilerCategory.Internal && description.Name == Marker)
                {
                    Category = description.Category.ToString();
                    Unit = description.UnitType.ToString();
                    break;
                }
            }

            // Warm native APIs outside the selected scope, without changing global Profiler settings.
            Begin();
            End();
        }

        internal bool Begin()
        {
            RequireOwner();
            if (active)
            {
                throw new InvalidOperationException("A GC allocation scope is already active.");
            }
            if (!recorder.Valid)
            {
                return false;
            }
            recorder.Reset();
            recorder.Start();
            active = true;
            return true;
        }

        internal ScopeResult End()
        {
            RequireOwner();
            if (!active || !recorder.Valid)
            {
                return default;
            }
            recorder.Stop();
            active = false;
            var result = new ScopeResult
            {
                calibratedBefore = Calibrated,
                sampleCount = recorder.Count,
                wrapped = recorder.WrappedAround
            };
            result.saturated = result.sampleCount >= Capacity;
            result.valid = !result.saturated && !result.wrapped && Unit != "UNKNOWN";
            int readableSamples = Math.Min(result.sampleCount, Capacity);
            for (int i = 0; i < readableSamples; i++)
            {
                ProfilerRecorderSample sample = recorder.GetSample(i);
                if (sample.Count <= 0 || sample.Count > long.MaxValue - result.allocationEvents ||
                    sample.Value < 0 || sample.Value > long.MaxValue - result.rawValueTotal)
                {
                    result.valid = false;
                    break;
                }
                result.allocationEvents += sample.Count;
                result.rawValueTotal += sample.Value;
            }
            return result;
        }

        internal CalibrationResult Calibrate()
        {
            RequireOwner();
            if (active)
            {
                throw new InvalidOperationException("Calibration must be outside a measured scope.");
            }
            Calibrated = false;
            var result = new CalibrationResult { knownLiveBytes = PositiveControlBytes };
            byte[] live = null;
            if (Begin())
            {
                try
                {
                    live = new byte[PositiveControlBytes];
                    live[0] = 1;
                    live[live.Length - 1] = 2;
                    GC.KeepAlive(live);
                }
                finally
                {
                    result.positive = End();
                }
            }
            if (Begin())
            {
                result.empty = End();
            }
            result.passed = live != null && live.Length == PositiveControlBytes &&
                result.positive.valid && result.positive.allocationEvents > 0 &&
                result.empty.valid && result.empty.allocationEvents == 0;
            Calibrated = result.passed;
            GC.KeepAlive(live);
            return result;
        }

        internal static bool HasZeroEvents(ScopeResult result,
            CalibrationResult before, CalibrationResult after)
        {
            return before.passed && after.passed && result.valid && result.calibratedBefore &&
                !result.wrapped && !result.saturated && result.allocationEvents == 0;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            RequireOwner();
            if (recorder.Valid)
            {
                recorder.Stop();
                recorder.Dispose();
            }
            active = false;
            Calibrated = false;
            disposed = true;
        }

        private void RequireOwner()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(BattleScopedGcAllocationRecorder));
            }
            if (Thread.CurrentThread.ManagedThreadId != ownerThread)
            {
                throw new InvalidOperationException("A GC recorder may only be used by its owner thread.");
            }
        }
    }

#if UNITY_INCLUDE_TESTS
    public sealed class BattleScopedGcAllocationRecorderEditorTests
    {
        [Serializable]
        private sealed class Observation
        {
            public string item;
            public string unityVersion;
            public string marker;
            public string category;
            public string unit;
            public int capacity;
            public BattleScopedGcAllocationRecorder.CalibrationResult before;
            public BattleScopedGcAllocationRecorder.CalibrationResult after;
            public BattleScopedGcAllocationRecorder.ScopeResult scope;
            public string status;
        }

        [Test]
        public void KnownLiveAllocationAndPreparedEmptyScope_CalibrateBeforeAndAfter()
        {
            using (var recorder = new BattleScopedGcAllocationRecorder())
            {
                var row = CreateObservation("PositiveAndEmpty", recorder);
                row.before = recorder.Calibrate();
                recorder.Begin();
                row.scope = recorder.End();
                row.after = recorder.Calibrate();
                bool passed = BattleScopedGcAllocationRecorder.HasZeroEvents(row.scope, row.before, row.after);
                row.status = passed ? "CALIBRATED_SCOPE_PASS" : "UNKNOWN_CALIBRATION_FAILED";
                Save(row);
                Assert.That(row.before.passed, Is.True, "Known 1MiB allocation must produce GC.Alloc events.");
                Assert.That(row.after.passed, Is.True, "Post-scope positive and negative controls must respond.");
                Assert.That(passed, Is.True);
            }
        }

        [Test]
        public void FixedCapacitySaturation_IsInvalidAndCannotCertifyZeroEvents()
        {
            using (var recorder = new BattleScopedGcAllocationRecorder(2))
            {
                var row = CreateObservation("Saturation", recorder);
                row.before = recorder.Calibrate();
                recorder.Begin();
                try
                {
                    for (int i = 0; i < 12; i++)
                    {
                        var live = new byte[1024];
                        live[0] = (byte)i;
                        GC.KeepAlive(live);
                    }
                }
                finally
                {
                    row.scope = recorder.End();
                }
                row.after = recorder.Calibrate();
                row.status = row.scope.saturated && !row.scope.valid ? "SATURATION_REJECTED" : "UNKNOWN";
                Save(row);
                Assert.That(row.scope.saturated, Is.True, "A nonresponsive recorder cannot prove saturation handling.");
                Assert.That(row.scope.valid, Is.False);
                Assert.That(BattleScopedGcAllocationRecorder.HasZeroEvents(row.scope, row.before, row.after), Is.False);
            }
        }

        [Test]
        public void MissingCalibrationOrInactiveScope_CannotCertifyZeroEvents()
        {
            using (var recorder = new BattleScopedGcAllocationRecorder())
            {
                Assert.That(recorder.End().valid, Is.False);
                Assert.That(BattleScopedGcAllocationRecorder.HasZeroEvents(default, default, default), Is.False);
            }
        }

        [Test]
        public void ActiveScope_ExceptionCleanupAndDispose_AreIdempotent()
        {
            var recorder = new BattleScopedGcAllocationRecorder();
            try
            {
                recorder.Begin();
                Assert.Throws<InvalidOperationException>(() => recorder.Begin());
                Assert.Throws<InvalidOperationException>(() => recorder.Calibrate());
            }
            finally
            {
                recorder.Dispose();
                recorder.Dispose();
            }
            Assert.Throws<ObjectDisposedException>(() => recorder.Begin());
        }

        private static Observation CreateObservation(string item, BattleScopedGcAllocationRecorder recorder)
        {
            return new Observation
            {
                item = item, unityVersion = Application.unityVersion, marker = recorder.Marker,
                category = recorder.Category, unit = recorder.Unit, capacity = recorder.Capacity
            };
        }

        private static void Save(Observation row)
        {
            string root = Path.Combine(Directory.GetCurrentDirectory(),
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH27-CALIBRATED-GC-20261007/calibration",
                row.item + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            using (var writer = new StreamWriter(new FileStream(Path.Combine(root, "result.json"),
                FileMode.CreateNew, FileAccess.Write, FileShare.Read)))
            {
                writer.Write(JsonUtility.ToJson(row, true));
            }
        }
    }
#endif
}
#endif
