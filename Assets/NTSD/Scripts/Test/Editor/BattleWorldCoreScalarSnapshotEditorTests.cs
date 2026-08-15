#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleWorldCoreScalarSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsImmutableScalarValuesAndSessionIdentity()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            SimulationWorld world = scope.Driver.World;

            world.Runtime.Match.LocalGameModeId = 4;
            world.Runtime.Match.BackgroundId = 17;
            world.Runtime.Stage.SetSceneSnapshot(1200, 150, 410, 3, 9);
            world.Runtime.Stage.ApplyPhaseBound(960);
            world.Runtime.StageProgression.StageSeriesIdx = 6;
            world.Runtime.StageProgression.WaveIdx = 2;
            world.Runtime.Flow.AiMoveMode = 3;
            world.Runtime.Flow.NeedClearInput = true;
            world.Rng.Seed(0x12345678U);
            world.Rng.NextRaw();

            Assert.That(session.TryCaptureWorldCoreScalarSnapshot(out var snapshot), Is.True);

            world.Runtime.Match.LocalGameModeId = 99;
            world.Runtime.Stage.ClearPhaseBound();
            world.Runtime.Flow.AiMoveMode = 0;
            world.Rng.NextRaw();

            Assert.That(snapshot.SchemaVersion,
                Is.EqualTo(BattleWorldCoreScalarSnapshot.CurrentSchemaVersion));
            Assert.That(snapshot.ProtocolSchemaVersion, Is.EqualTo(identity.SchemaVersion));
            Assert.That(snapshot.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(snapshot.Match.LocalGameModeId, Is.EqualTo(4));
            Assert.That(snapshot.Match.BackgroundId, Is.EqualTo(17));
            Assert.That(snapshot.Stage.BaseStageWidthPx, Is.EqualTo(1200));
            Assert.That(snapshot.Stage.StageWidthPx, Is.EqualTo(960));
            Assert.That(snapshot.Stage.XMaxOverride, Is.EqualTo(960));
            Assert.That(snapshot.Progression.StageSeriesIdx, Is.EqualTo(6));
            Assert.That(snapshot.Progression.WaveIdx, Is.EqualTo(2));
            Assert.That(snapshot.Flow.AiMoveMode, Is.EqualTo(3));
            Assert.That(snapshot.Flow.NeedClearInput, Is.True);
            Assert.That(snapshot.RngCallCount, Is.EqualTo(1UL));
        }

        [Test]
        public void WarmScalarCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            Assert.That(session.TryCaptureWorldCoreScalarSnapshot(out _), Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldCoreScalarSnapshot(out _))
                    Assert.Fail($"Scalar snapshot capture failed at {index}.");
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
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
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("BattleWorldCoreScalarSnapshotTests")
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
