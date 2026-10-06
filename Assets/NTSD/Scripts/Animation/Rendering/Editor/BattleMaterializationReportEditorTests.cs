#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleMaterializationReportEditorTests
    {
        private static readonly string[] ScalarNames =
        {
            "MaterializationRequestCount", "PublicationChangedRequestCount", "AlphaChangedRequestCount",
            "RepeatedSampleRequestCount", "MaterializationAttemptCount", "PublicationChangedAttemptCount",
            "AlphaChangedAttemptCount", "RepeatedSampleAttemptCount", "SameUnityFrameReuseCount",
            "SameSampleReuseCount", "ReentrantSkipCount",
        };
        private static readonly string[] GroupNames =
        {
            "MaterializationBuilds", "PublicationChangedBuilds", "AlphaChangedBuilds", "RepeatedSampleBuilds",
        };
        private static readonly string[] BuildNames =
        {
            "BuildInvocationCount", "AdmittedBuildCount", "CompletedBuildCount", "RejectedBeforeBuildCount",
            "FailedBuildCount", "VertexUploadCallCount", "UploadedVertexCount", "UploadedVertexBytes",
        };

        private static IEnumerable<int> CounterOffsets()
        {
            for (int offset = 0; offset < 43; offset++)
                yield return offset;
        }

        [Test]
        public void ZeroSnapshot_HasCompleteSchema_ExplicitScopeAndUnits()
        {
            object report = Capture(new BattleCentralRuntimeDiagnostics());
            var root = Projection(report);
            Assert.That(root["schemaVersion"], Is.EqualTo("ntsd-central-materialization-counters-v1"));
            Assert.That(root["scope"], Is.EqualTo("cumulative-since-reset"));
            Assert.That(root["counterEpoch"], Is.TypeOf<long>());
            var units = Map(root["units"]);
            Assert.That(units["uploadedVertexBytes"], Is.EqualTo("bytes"));
            Assert.That(units["uploadedVertexCount"], Is.EqualTo("vertices"));
            Assert.That(Flatten(Map(root["counts"])).Count, Is.EqualTo(43));
            foreach (long value in Flatten(Map(root["counts"])))
                Assert.That(value, Is.Zero);
            Assert.That(root.ContainsKey("verdict"), Is.False);
            Assert.That(root["limitations"].ToString(), Does.Contain("not GPU"));
            Assert.That(Json(report), Does.Contain("cumulative-since-reset"));
        }

        [Test]
        public void Snapshot_FreezesAllLongValues_AfterLiveMutationAndReset()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            for (int offset = 0; offset < 43; offset++)
                SetCounter(diagnostics, offset, 4294967300L + offset);
            object report = Capture(diagnostics);
            string frozen = Json(report);
            Reset(diagnostics);
            var values = Flatten(Map(Projection(report)["counts"]));
            for (int offset = 0; offset < 43; offset++)
                Assert.That(values[offset], Is.EqualTo(4294967300L + offset));
            Assert.That(Json(report), Is.EqualTo(frozen));
        }

        [TestCase(BattleCentralDisplaySampleKind.PublicationChanged, "publicationChanged")]
        [TestCase(BattleCentralDisplaySampleKind.AlphaChanged, "alphaChanged")]
        [TestCase(BattleCentralDisplaySampleKind.Repeated, "repeated")]
        public void BuildCategories_KeepRejectedFailedAndCompletedApiSeparate(
            BattleCentralDisplaySampleKind kind, string group)
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var payload = new BattleCentralBuildDiagnostics
            {
                VertexUploadCallCount = 2, UploadedVertexCount = 68, UploadedVertexBytes = 4294967300L,
            };
            RecordBuild(diagnostics, kind, true, true, payload);
            RecordBuild(diagnostics, kind, false, false, payload);
            RecordBuild(diagnostics, kind, true, false, payload);
            var builds = Map(Map(Projection(Capture(diagnostics))["counts"])["builds"]);
            var selected = Map(builds[group]);
            Assert.That(selected["buildInvocationCount"], Is.EqualTo(3L));
            Assert.That(selected["admittedBuildCount"], Is.EqualTo(2L));
            Assert.That(selected["completedBuildCount"], Is.EqualTo(1L));
            Assert.That(selected["rejectedBeforeBuildCount"], Is.EqualTo(1L));
            Assert.That(selected["failedBuildCount"], Is.EqualTo(1L));
            Assert.That(selected["vertexUploadCallCount"], Is.EqualTo(4L));
            Assert.That(selected["uploadedVertexBytes"], Is.EqualTo(8589934600L));
            CollectionAssert.AreEqual(Flatten(selected), Flatten(Map(builds["total"])));
        }

        [Test]
        public void Window_UsesFrozenEndpoints_AndDeltaNotLiveCounters()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            for (int offset = 0; offset < 43; offset++)
                SetCounter(diagnostics, offset, 100 + offset);
            object baseline = Capture(diagnostics);
            for (int offset = 0; offset < 43; offset++)
                SetCounter(diagnostics, offset, 105 + offset);
            object end = Capture(diagnostics);
            object window = Window(end, baseline, out string reason);
            Assert.That(reason, Is.Empty);
            Reset(diagnostics);
            var root = Projection(window);
            Assert.That(root["scope"], Is.EqualTo("window-delta"));
            foreach (long value in Flatten(Map(root["counts"])))
                Assert.That(value, Is.EqualTo(5L));
            var endpoints = Map(root["window"]);
            CollectionAssert.AreEqual(Flatten(Map(Projection(baseline)["counts"])), Flatten(Map(endpoints["start"])));
            CollectionAssert.AreEqual(Flatten(Map(Projection(end)["counts"])), Flatten(Map(endpoints["end"])));
            Assert.That(Json(window), Does.Contain("window-delta"));
        }

        [Test]
        public void EmptyWindow_IsValidObservedZero()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            object baseline = Capture(diagnostics);
            object window = Window(Capture(diagnostics), baseline, out string reason);
            Assert.That(reason, Is.Empty);
            foreach (long value in Flatten(Map(Projection(window)["counts"])))
                Assert.That(value, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Reset_RejectsOldBaseline_EvenWhenCountsCatchUp(bool catchUp)
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            object baseline = Capture(diagnostics);
            long oldEpoch = (long)Projection(baseline)["counterEpoch"];
            Reset(diagnostics);
            if (catchUp)
                for (int offset = 0; offset < 43; offset++)
                    SetCounter(diagnostics, offset, 99);
            object current = Capture(diagnostics);
            Assert.That((long)Projection(current)["counterEpoch"], Is.GreaterThan(oldEpoch));
            AssertRejected(current, baseline, "epoch");
        }

        [Test]
        public void DifferentSource_RejectsIdenticalCountersAndEpoch()
        {
            AssertRejected(Capture(new BattleCentralRuntimeDiagnostics()),
                Capture(new BattleCentralRuntimeDiagnostics()), "source");
        }

        [Test]
        public void NullBaseline_IsRejectedWithoutPartialResult()
        {
            AssertRejected(Capture(new BattleCentralRuntimeDiagnostics()), null, "baseline");
        }

        [TestCaseSource(nameof(CounterOffsets))]
        public void AnyCounterDecrease_RejectsWholeWindow_NoClamp(int decreasingOffset)
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            for (int offset = 0; offset < 43; offset++)
                SetCounter(diagnostics, offset, 10);
            object baseline = Capture(diagnostics);
            SetCounter(diagnostics, decreasingOffset, 9);
            AssertRejected(Capture(diagnostics), baseline, "decreased");
        }

        [Test]
        public void WindowCannotBeReusedAsCumulativeEndpoint()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            object baseline = Capture(diagnostics);
            object current = Capture(diagnostics);
            object window = Window(current, baseline, out _);
            AssertRejected(current, window, "cumulative");
            AssertRejected(window, baseline, "cumulative");
        }

        [Test]
        public void ProjectionMutation_DoesNotChangeFrozenReportOrFutureProjection()
        {
            object report = Capture(new BattleCentralRuntimeDiagnostics());
            string frozen = Json(report);
            var projection = Projection(report);
            Map(Map(projection["counts"])["requests"])["total"] = 123L;
            projection["scope"] = "wrong";
            Assert.That(Json(report), Is.EqualTo(frozen));
        }

        [Test]
        public void Capture_IsAnExplicitRead_NoLiveCounterResetOrBuild()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            SetCounter(diagnostics, 0, 7);
            object first = Capture(diagnostics);
            object second = Capture(diagnostics);
            Assert.That(diagnostics.MaterializationRequestCount, Is.EqualTo(7));
            Assert.That(diagnostics.MaterializationBuilds.BuildInvocationCount, Is.Zero);
            Assert.That(Json(second), Is.EqualTo(Json(first)));
        }

        [Test]
        public void EpochResetAndExistingAggregation_WarmedHotPathAllocatesZero()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            Capture(diagnostics);
            Action reset = (Action)Required(typeof(BattleCentralRuntimeDiagnostics), "ResetMaterializationCounters")
                .CreateDelegate(typeof(Action), diagnostics);
            var record = (Action<BattleCentralDisplaySampleKind, bool, bool, BattleCentralBuildDiagnostics>)
                Required(typeof(BattleCentralRuntimeDiagnostics), "RecordBackendBuild").CreateDelegate(
                    typeof(Action<BattleCentralDisplaySampleKind, bool, bool, BattleCentralBuildDiagnostics>), diagnostics);
            var payload = new BattleCentralBuildDiagnostics { VertexUploadCallCount = 1, UploadedVertexCount = 4 };
            for (int index = 0; index < 64; index++)
            {
                reset();
                record(BattleCentralDisplaySampleKind.AlphaChanged, true, true, payload);
            }
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 512; index++)
            {
                reset();
                record(BattleCentralDisplaySampleKind.AlphaChanged, true, true, payload);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            Assert.That(diagnostics.AlphaChangedBuilds.BuildInvocationCount, Is.EqualTo(1));
        }

        private static object Capture(BattleCentralRuntimeDiagnostics diagnostics)
        {
            return Required(typeof(BattleCentralRuntimeDiagnostics), "CaptureMaterializationReport")
                .Invoke(diagnostics, null);
        }

        private static MethodInfo Required(Type type, string name)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Missing report contract: " + name);
            return method;
        }

        private static Dictionary<string, object> Projection(object report)
        {
            return (Dictionary<string, object>)Required(report.GetType(), "ToProjection").Invoke(report, null);
        }

        private static string Json(object report)
        {
            return (string)Required(report.GetType(), "ToJson").Invoke(report, null);
        }

        private static object Window(object current, object baseline, out string reason)
        {
            object[] arguments = { baseline, null, null };
            bool success = (bool)Required(current.GetType(), "TryCreateWindow").Invoke(current, arguments);
            reason = (string)arguments[2];
            Assert.That(success, Is.EqualTo(arguments[1] != null));
            return arguments[1];
        }

        private static void AssertRejected(object current, object baseline, string reasonFragment)
        {
            Assert.That(Window(current, baseline, out string reason), Is.Null);
            Assert.That(reason, Does.Contain(reasonFragment));
        }

        private static Dictionary<string, object> Map(object value)
        {
            return (Dictionary<string, object>)value;
        }

        private static List<long> Flatten(Dictionary<string, object> root)
        {
            var values = new List<long>();
            foreach (object value in root.Values)
            {
                if (value is Dictionary<string, object> nested)
                    values.AddRange(Flatten(nested));
                else
                {
                    Assert.That(value, Is.TypeOf<long>());
                    values.Add((long)value);
                }
            }
            return values;
        }

        private static void SetCounter(BattleCentralRuntimeDiagnostics diagnostics, int offset, long value)
        {
            object owner = offset < 11 ? diagnostics : typeof(BattleCentralRuntimeDiagnostics)
                .GetProperty(GroupNames[(offset - 11) / 8]).GetValue(diagnostics);
            string property = offset < 11 ? ScalarNames[offset] : BuildNames[(offset - 11) % 8];
            owner.GetType().GetProperty(property).SetValue(owner, value);
        }

        private static void Reset(BattleCentralRuntimeDiagnostics diagnostics)
        {
            Required(typeof(BattleCentralRuntimeDiagnostics), "ResetMaterializationCounters").Invoke(diagnostics, null);
        }

        private static void RecordBuild(BattleCentralRuntimeDiagnostics diagnostics,
            BattleCentralDisplaySampleKind kind, bool admitted, bool completed, BattleCentralBuildDiagnostics payload)
        {
            Required(typeof(BattleCentralRuntimeDiagnostics), "RecordBackendBuild")
                .Invoke(diagnostics, new object[] { kind, admitted, completed, payload });
        }
    }
}
#endif
