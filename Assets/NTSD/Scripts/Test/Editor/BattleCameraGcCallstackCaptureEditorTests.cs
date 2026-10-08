#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditorInternal;
using UnityEngine.Profiling;

namespace NTSD.Test.Editor
{
    public sealed class BattleCameraGcCallstackCaptureEditorTests
    {
        [Test]
        public void Request_CapturesFirstEightCamerasWithoutReplay()
        {
            object request = CreateRequest();
            Assert.That(Field<int>(request, "targetCameraFrames"), Is.EqualTo(8));
            Assert.That(Field<bool>(request, "runNextCycle"), Is.False);
            Assert.That(Field<bool>(request, "replayProductionCatalog"), Is.False);
            Assert.That(Field<bool>(request, "cameraGcCallstackCapture"), Is.True);
        }

        [Test]
        public void Request_PreservesCalibratedFullCameraAndObserverScopes()
        {
            object request = CreateRequest();
            Assert.That(Field<bool>(request, "requireActiveAuxiliaryCoverage"), Is.True);
            Assert.That(Field<bool>(request, "calibratedAllocationSampling"), Is.True);
            Assert.That(Field<bool>(request, "allocationFrameProvenance"), Is.True);
            Assert.That(Field<bool>(request, "allocationObserverStages"), Is.True);
            Assert.That(Field<bool>(request, "prepareObserverLiterals"), Is.True);
        }

        [Test]
        public void Request_DoesNotAddTimingOrInputExperiment()
        {
            object request = CreateRequest();
            Assert.That(Field<bool>(request, "sampleTiming"), Is.False);
            Assert.That(Field<bool>(request, "dynamicInput"), Is.False);
            Assert.That(Field<int>(request, "cycle"), Is.EqualTo(1));
        }

        [Test]
        public void Request_PreservesCallerOwnedFreshOutputRoot()
        {
            Assert.That(Field<string>(CreateRequest(), "outputRoot"),
                Is.EqualTo("test-only-camera-root/"));
        }

        [TestCase(4d, 5d, 8d, false)]
        [TestCase(5d, 5d, 8d, true)]
        [TestCase(6d, 5d, 8d, true)]
        [TestCase(8d, 5d, 8d, true)]
        [TestCase(9d, 5d, 8d, false)]
        [TestCase(7d, 8d, 5d, false)]
        public void CameraTimeRange_ExcludesOutsideAndReversedBoundaries(
            double sample, double beginEnd, double endStart, bool expected)
        {
            Assert.That((bool)Method(typeof(BattleOptimizationCpuGcCaptureEditor),
                "IsWithinCameraTime").Invoke(null, new object[] { sample, beginEnd, endStart }),
                Is.EqualTo(expected));
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(9, false)]
        public void CameraCompletion_RequiresExactFixedCapacity(int completed, bool expected)
        {
            Assert.That((bool)Method(typeof(BattleOptimizationCpuGcCaptureEditor),
                "HasCompletedCameraWindow").Invoke(null, new object[] { completed }),
                Is.EqualTo(expected));
        }

        [Test]
        public void LateRequest_PreservesFullScopeAndFiniteSingleWindow()
        {
            object request = CreateLateRequest();
            Assert.That(Field<int>(request, "targetCameraFrames"), Is.EqualTo(1800));
            Assert.That(Field<bool>(request, "runNextCycle"), Is.False);
            Assert.That(Field<bool>(request, "lateCameraGcCallstackCapture"), Is.True);
            Assert.That(Field<bool>(request, "cameraGcCallstackCapture"), Is.True);
            Assert.That(Field<bool>(request, "calibratedAllocationSampling"), Is.True);
            Assert.That(Field<bool>(request, "allocationObserverStages"), Is.True);
            Assert.That(Field<bool>(request, "prepareObserverLiterals"), Is.True);
            Assert.That(Field<bool>(request, "requireActiveAuxiliaryCoverage"), Is.True);
        }

        [Test]
        public void LateRequest_RemainsDiagnosticWithoutReplayTimingOrInput()
        {
            object request = CreateLateRequest();
            Assert.That(Field<bool>(request, "replayProductionCatalog"), Is.False);
            Assert.That(Field<bool>(request, "sampleTiming"), Is.False);
            Assert.That(Field<bool>(request, "dynamicInput"), Is.False);
            Assert.That(Field<int>(request, "cycle"), Is.EqualTo(1));
            Assert.That(Field<string>(request, "outputRoot"), Is.EqualTo("test-only-late-root/"));
            Assert.That(Field<string>(request, "allocationScope"), Does.Contain("DIAGNOSTIC_ONLY"));
        }

        [Test]
        public void LateRing_RetainsOrdinal937WithoutGrowingOrStoppingAtEight()
        {
            object capture = Method(typeof(BattleOptimizationCpuGcCaptureEditor),
                "CreateLateCameraState").Invoke(null, new object[] { "test-only-late-root/" });
            Array ring = Field<Array>(capture, "cameraBoundaries");
            Assert.That(ring.Length, Is.EqualTo(8));
            for (int ordinal = 1; ordinal <= 937; ordinal++)
            {
                Method(typeof(BattleOptimizationCpuGcCaptureEditor), "OpenLateCameraBoundary")
                    .Invoke(null, new object[] { capture, ordinal + 100, ordinal });
                Method(typeof(BattleOptimizationCpuGcCaptureEditor), "CompleteLateCameraBoundary")
                    .Invoke(null, new object[] { capture, ordinal + 100, ordinal,
                        ordinal == 937 ? 12L : 0L, true });
                Assert.That(Field<bool>(capture, "finishPending"), Is.EqualTo(ordinal == 937));
            }
            Assert.That(Field<Array>(capture, "cameraBoundaries"), Is.SameAs(ring));
            Assert.That(Field<int>(capture, "completedCameras"), Is.EqualTo(937));
            Assert.That(Field<int>(capture, "triggerOrdinal"), Is.EqualTo(937));
            Assert.That(Field<bool>(capture, "cameraBoundariesValid"), Is.True);
            for (int ordinal = 930; ordinal <= 937; ordinal++)
                Assert.That(Field<int>(ring.GetValue((ordinal - 1) % 8), "ordinal"), Is.EqualTo(ordinal));
            Assert.That(Field<long>(ring.GetValue(936 % 8), "allocationEvents"), Is.EqualTo(12));
        }

        [TestCase(1, 0L, true, false)]
        [TestCase(8, 0L, true, false)]
        [TestCase(937, 12L, true, true)]
        [TestCase(1799, 0L, true, false)]
        [TestCase(1800, 0L, true, true)]
        [TestCase(12, 0L, false, true)]
        [TestCase(12, -1L, true, true)]
        public void LateStop_RequiresEventInvalidScopeOrFiniteLimit(
            int completed, long events, bool valid, bool expected)
        {
            Assert.That((bool)Method(typeof(BattleOptimizationCpuGcCaptureEditor),
                "ShouldStopLateCameraWindow").Invoke(null, new object[] { completed, events, valid }),
                Is.EqualTo(expected));
        }

        [Test]
        public void LateRing_RejectsCrossFrameCameraBoundary()
        {
            object capture = Method(typeof(BattleOptimizationCpuGcCaptureEditor),
                "CreateLateCameraState").Invoke(null, new object[] { "test-only-late-root/" });
            Method(typeof(BattleOptimizationCpuGcCaptureEditor), "OpenLateCameraBoundary")
                .Invoke(null, new object[] { capture, 10, 20 });
            Method(typeof(BattleOptimizationCpuGcCaptureEditor), "CompleteLateCameraBoundary")
                .Invoke(null, new object[] { capture, 11, 20, 0L, true });
            Assert.That(Field<bool>(capture, "cameraBoundariesValid"), Is.False);
            Assert.That(Field<bool>(capture, "finishPending"), Is.True);
        }

        [TestCase(0, false)]
        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(300, true)]
        [TestCase(301, false)]
        public void LateHistory_RejectsUnboundedOrInsufficientHistory(int capacity, bool expected)
        {
            Assert.That((bool)Method(typeof(BattleOptimizationCpuGcCaptureEditor),
                "IsLateHistoryCapacitySafe").Invoke(null, new object[] { capacity }), Is.EqualTo(expected));
        }

        [Test]
        public void LateHistory_ReadDoesNotInitializeOrChangeNativeHistory()
        {
            Type settings = typeof(ProfilerDriver).Assembly.GetType("UnityEditor.Profiling.ProfilerUserSettings", true);
            FieldInfo cached = settings.GetField("m_FrameCount", BindingFlags.Static | BindingFlags.NonPublic);
            int before = (int)cached.GetValue(null);
            int first = ProfilerDriver.firstFrameIndex;
            int last = ProfilerDriver.lastFrameIndex;
            Assert.That((int)Method(typeof(BattleOptimizationCpuGcCaptureEditor), "ReadLateHistoryCapacity")
                .Invoke(null, null), Is.EqualTo(before));
            Assert.That((int)cached.GetValue(null), Is.EqualTo(before));
            Assert.That(ProfilerDriver.firstFrameIndex, Is.EqualTo(first));
            Assert.That(ProfilerDriver.lastFrameIndex, Is.EqualTo(last));
        }

        [Test]
        public void LateSettings_RecordsProducerBufferSeparatelyFromHistory()
        {
            object settings = Method(typeof(BattleOptimizationCpuGcCaptureEditor),
                "ReadSettings").Invoke(null, null);
            Assert.That(Field<int>(settings, "maximumBufferedBytes"), Is.EqualTo(Profiler.maxUsedMemory));
            Assert.That(typeof(ProfilerDriver).GetMethod("SaveProfile", new[] { typeof(string) }).ReturnType,
                Is.EqualTo(typeof(void)), "Use the installed Unity 2022 API, not master bindings.");
        }

        private static object CreateLateRequest()
        {
            return Method(typeof(BattleCentralProductionWindowSceneProbeEditor),
                "CreateLateCameraCallstackReport").Invoke(null, new object[] { "test-only-late-root/" });
        }

        private static object CreateRequest()
        {
            return Method(typeof(BattleCentralProductionWindowSceneProbeEditor),
                "CreateCameraCallstackReport").Invoke(null, new object[] { "test-only-camera-root/" });
        }

        private static T Field<T>(object value, string name)
        {
            FieldInfo field = value.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            return (T)field.GetValue(value);
        }

        private static MethodInfo Method(Type type, string name)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, name + " is not implemented.");
            return method;
        }
    }
}
#endif
