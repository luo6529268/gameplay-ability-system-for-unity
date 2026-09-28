#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q09EarthquakeStatePublicationEditorTests
    {
        [Test]
        public void SuccessfulTickWithoutPresentationReleasesOwnerAndKeepsOffset()
        {
            var world = new SimulationWorld();
            world.Runtime.Earthquake.RestoreForSnapshot(0, 2, 0);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1, buildPresentation: false);

            AssertTuple(world.Runtime.Earthquake, -1, 2, 0);
        }

        [Test]
        public void AscendingOwnerScanRetainsOffsetsUntilAnotherEncodedFrameWrites()
        {
            var world = new SimulationWorld();
            var first = new LF2Character { ObjectId = 726 };
            var second = new LF2Character { ObjectId = 727 };
            first.SetRequiredRuntimeSlot(0);
            second.SetRequiredRuntimeSlot(1);
            world.Register(first);
            world.Register(second);
            var state = world.Runtime.Earthquake;

            first.Frame.D = new LF2FrameData { state = 55052 };
            second.Frame.D = new LF2FrameData { state = 55053 };
            state.Advance(world);
            AssertTuple(state, 0, 2, 0);

            first.Frame.D = new LF2FrameData { state = 0 };
            state.Advance(world);
            AssertTuple(state, 1, 3, 0);

            second.Frame.D = new LF2FrameData { state = 0 };
            state.Advance(world);
            AssertTuple(state, -1, 3, 0);

            second.Frame.D = new LF2FrameData { state = 55050 };
            state.Advance(world);
            AssertTuple(state, 1, 0, 0);

            world.Runtime.Reset();
            AssertTuple(world.Runtime.Earthquake, -1, 0, 0);
        }

        [Test]
        public void ReleasedOwnerOffsetSurvivesSnapshotRestoreChecksumAndFrozenFrameCopy()
        {
            using var scope = new DriverScope();
            SimulationWorld world = scope.Driver.World;
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();

            world.Runtime.Earthquake.RestoreForSnapshot(0, 2, -1);
            world.Runtime.Earthquake.Advance(world);
            AssertTuple(world.Runtime.Earthquake, -1, 2, -1);
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);
            ulong capturedChecksum = world.CaptureRuntimeChecksum64(0, null);
            Assert.That(snapshot.Core.Earthquake.OwnerSlot, Is.EqualTo(-1));
            Assert.That(snapshot.Core.Earthquake.BackgroundOffsetX, Is.EqualTo(2));
            Assert.That(snapshot.Core.Earthquake.BackgroundOffsetY, Is.EqualTo(-1));

            world.Runtime.Earthquake.Reset();
            Assert.That(world.CaptureRuntimeChecksum64(0, null),
                Is.Not.EqualTo(capturedChecksum));
            Assert.That(scope.Driver.TryRestoreBattleStateSnapshot(
                identity, snapshot, out BattleStateSnapshotRestoreFailure failure),
                Is.True, failure.ToString());
            AssertTuple(world.Runtime.Earthquake, -1, 2, -1);
            Assert.That(world.CaptureRuntimeChecksum64(0, null),
                Is.EqualTo(capturedChecksum));

            var source = new BattlePresentationFrame
            {
                EarthquakeOwnerSlot = world.Runtime.Earthquake.OwnerSlot,
                EarthquakeBackgroundOffsetX = world.Runtime.Earthquake.BackgroundOffsetX,
                EarthquakeBackgroundOffsetY = world.Runtime.Earthquake.BackgroundOffsetY,
            };
            var frozen = new BattlePresentationFrame();
            frozen.CopyFrom(source);
            source.Reset(1);
            Assert.That(frozen.EarthquakeOwnerSlot, Is.EqualTo(-1));
            Assert.That(frozen.EarthquakeBackgroundOffsetX, Is.EqualTo(2));
            Assert.That(frozen.EarthquakeBackgroundOffsetY, Is.EqualTo(-1));
            Assert.That(source.EarthquakeBackgroundOffsetX, Is.Zero);
            Assert.That(source.EarthquakeBackgroundOffsetY, Is.Zero);
        }

        private static void AssertTuple(
            NTSD28EarthquakeRuntimeState state,
            int owner,
            int x,
            int y)
        {
            Assert.That(state.OwnerSlot, Is.EqualTo(owner));
            Assert.That(state.BackgroundOffsetX, Is.EqualTo(x));
            Assert.That(state.BackgroundOffsetY, Is.EqualTo(y));
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField", flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("Q09 Earthquake State Test")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                Driver.RecreateWorld();
                Driver.SetPaused(true);
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
#endif
