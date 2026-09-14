#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeLifecycleCarrierEditorTests
    {
        private static readonly string[] Names = { "NativeRuntimeStateCode", "NativeLifecycleResolutionPending", "NativeLifecycleCode" };

        private static FieldInfo Field(string name)
        {
            var field = typeof(NTSDEntityRuntime).GetField(name);
            Assert.That(field, Is.Not.Null, "Independent source lifecycle carrier missing: " + name);
            return field;
        }

        [TestCase("NativeRuntimeStateCode", -99)]
        [TestCase("NativeLifecycleResolutionPending", true)]
        [TestCase("NativeLifecycleCode", 1101)]
        public void NewResetCopyAndFingerprintRespectEachCarrier(string name, object value)
        {
            var field = Field(name);
            object empty = value is bool ? (object)false : 0;
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            Assert.That(field.GetValue(source), Is.EqualTo(empty));
            var original = BattleRuntimeFingerprint.Compute(source);
            field.SetValue(source, value);
            Assert.That(BattleRuntimeFingerprint.Compute(source), Is.Not.EqualTo(original));
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            Assert.That(field.GetValue(destination), Is.EqualTo(value));
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 128; i++) source.TryCopyCanonicalStateTo(destination);
            Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
            source.Reset();
            Assert.That(field.GetValue(source), Is.EqualTo(empty));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ClaimedAndRawSnapshotRestoresIndependentValues(BattleRuntimeProfile profile)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1050);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            try
            {
                var raw = world.RuntimeSlotTableForModules.GetRawRuntime(world.RuntimeSlotCapacity - 1);
                string before = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
                object[] values = { -99, true, 1101 };
                for (int i = 0; i < Names.Length; i++)
                {
                    Field(Names[i]).SetValue(entity.Runtime, values[i]);
                    Field(Names[i]).SetValue(raw, values[i]);
                }
                entity.Runtime.HitStop = 4;
                entity.Runtime.PendingFlushDestroy = false;
                string expected = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
                Assert.That(expected, Is.Not.EqualTo(before));
                string capture = NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1);
                Assert.That(capture, Does.Contain("\"runtimeStateCode\":-99"));
                Assert.That(capture, Does.Contain("\"renderPhase\":4"));
                Assert.That(capture, Does.Contain("\"resolutionPending\":true"));
                Assert.That(capture, Does.Contain("\"code\":1101"));
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                for (int i = 0; i < Names.Length; i++)
                {
                    object empty = values[i] is bool ? (object)false : 0;
                    Field(Names[i]).SetValue(entity.Runtime, empty);
                    Field(Names[i]).SetValue(raw, empty);
                }
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                for (int i = 0; i < Names.Length; i++)
                {
                    Assert.That(Field(Names[i]).GetValue(entity.Runtime), Is.EqualTo(values[i]));
                    Assert.That(Field(Names[i]).GetValue(world.RuntimeSlotTableForModules.GetRawRuntime(world.RuntimeSlotCapacity - 1)), Is.EqualTo(values[i]));
                }
                Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.EqualTo(expected));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PreviousSchemaRejectsBeforeMutation(bool entityPayload)
        {
            var world = new SimulationWorld();
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            object payload = entityPayload ? (object)snapshot.EntityRuntime : snapshot;
            payload.GetType().GetProperty("SchemaVersion").SetValue(payload, entityPayload ? 14 : 22);
            string before = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out _), Is.False);
            Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.EqualTo(before));
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06NativeLifecyclePlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_NativeLifecyclePlay.request";
        private const string Result = "Temp/NTSD28_Q06_NativeLifecyclePlay.result.json";

        static NTSD28Q06NativeLifecyclePlayProbe()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run")
                return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            File.WriteAllText(Request, "running");
            var report = new Report { tick = driver.CurrentTickIndex, objectsBefore = world.ObjectCount };
            try
            {
                var entity = world.FindEntityByRuntimeSlotForQuery(0);
                Assert.That(entity?.Renderer, Is.Not.Null);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var saved = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, saved), Is.True);
                var input = new FrameInputSet(report.tick, Array.Empty<SimulationPlayerInput>());
                string originalChecksum = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum;
                try
                {
                    entity.WriteCurrentFrameId(1101);
                    entity.Frame.D = null;
                    entity.Runtime.NativeRuntimeStateCode = -99;
                    entity.Runtime.NativeLifecycleResolutionPending = true;
                    entity.Runtime.NativeLifecycleCode = 1101;
                    var modified = world.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, modified), Is.True);
                    string modifiedChecksum = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum;
                    entity.Runtime.NativeRuntimeStateCode = 0;
                    entity.Runtime.NativeLifecycleResolutionPending = false;
                    entity.Runtime.NativeLifecycleCode = 0;
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, modified, out var failure), Is.True, failure.ToString());
                    var restored = world.FindEntityByRuntimeSlotForQuery(0);
                    Assert.That(restored, Is.SameAs(entity));
                    Assert.That(restored.Runtime.NativeRuntimeStateCode, Is.EqualTo(-99));
                    Assert.That(restored.Runtime.NativeLifecycleResolutionPending, Is.True);
                    Assert.That(restored.Runtime.NativeLifecycleCode, Is.EqualTo(1101));
                    Assert.That(world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum, Is.EqualTo(modifiedChecksum));
                    world.LateEntityUpdateAll(report.tick);
                    Assert.That(restored.Frame.N, Is.Zero);
                    Assert.That(restored.Frame.Prev, Is.EqualTo(1101));
                    Assert.That(restored.Runtime.PrevFrame2, Is.Zero);
                    Assert.That(restored.Runtime.NativeRuntimeStateCode, Is.EqualTo(-1));
                    Assert.That(restored.Runtime.NativeLifecycleResolutionPending, Is.False);
                    Assert.That(restored.Runtime.NativeLifecycleCode, Is.Zero);
                    report.stateCode = restored.Runtime.NativeRuntimeStateCode;
                    report.frameAction = restored.Frame.N;
                    report.modifiedChecksumRestored = true;
                }
                finally
                {
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                    Assert.That(world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum, Is.EqualTo(originalChecksum));
                    report.originalChecksumRestored = true;
                }
                report.objectsAfter = world.ObjectCount;
                Assert.That(report.objectsAfter, Is.EqualTo(report.objectsBefore));
                report.status = "PASS";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }
            File.WriteAllText(Result, JsonConvert.SerializeObject(report, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }

        private sealed class Report
        {
            public string status, error;
            public string scope = "Current-content real Scene paused carrier restore plus actual C25 late entry encoded lifecycle; no complete input/physics tick or fragment parity claim.";
            public int tick, objectsBefore, objectsAfter, stateCode, frameAction;
            public bool modifiedChecksumRestored, originalChecksumRestored;
        }
    }
}
#endif
