#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;

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
