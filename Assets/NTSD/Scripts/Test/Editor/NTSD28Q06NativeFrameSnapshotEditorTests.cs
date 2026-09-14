#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeFrameSnapshotEditorTests
    {
        [TestCase(BattleRuntimeProfile.Authority400, false, 7)]
        [TestCase(BattleRuntimeProfile.Authority400, false, 857)]
        [TestCase(BattleRuntimeProfile.Authority400, false, 998)]
        [TestCase(BattleRuntimeProfile.Authority400, false, 999)]
        [TestCase(BattleRuntimeProfile.Authority400, true, 7)]
        [TestCase(BattleRuntimeProfile.Authority400, true, 857)]
        [TestCase(BattleRuntimeProfile.Authority400, true, 998)]
        [TestCase(BattleRuntimeProfile.Authority400, true, 999)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false, 7)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false, 857)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false, 998)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false, 999)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true, 7)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true, 857)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true, 998)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true, 999)]
        public void NativeDescriptorIdentitySurvivesLocalAndTransferredRestore(BattleRuntimeProfile profile, bool transfer, int action)
        {
            var wrapper = Wrapper(true);
            var source = World(profile, wrapper);
            SimulationWorld target = null;
            try
            {
                var entity = Spawn(source);
                Bind(entity, action, action == 7 ? 998 : 7);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var input = new FrameInputSet(0, Array.Empty<SimulationPlayerInput>());
                string expected = source.CaptureLockstepChecksumSnapshot(0, input).OverallChecksum;
                var snapshot = source.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(source.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                target = transfer ? World(profile, wrapper) : source;
                if (transfer) snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
                else
                {
                    Bind(entity, 0, 0);
                    entity.Runtime.HP = 7;
                }
                Assert.That(target.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                var restored = target.FindEntityByRuntimeSlotForQuery(3);
                Assert.That(restored, Is.Not.Null);
                Assert.That(restored.Frame.N, Is.EqualTo(action));
                Assert.That(restored.Runtime.Frame, Is.EqualTo(action));
                Assert.That(restored.Frame.D, Is.SameAs(restored.FrameCache.GetNativeFrameDataById(action)));
                Assert.That(restored.Frame.D.frameId, Is.EqualTo(action));
                Assert.That(restored.Frame.D.wait, Is.EqualTo(action == 999 ? 2 : 0));
                int collision = action == 7 ? 998 : 7;
                Assert.That(restored.Frame.Prev2D.frameId, Is.EqualTo(collision));
                Assert.That(restored.Trans.WaitCounter, Is.EqualTo(action));
                Assert.That(target.CaptureLockstepChecksumSnapshot(0, input).OverallChecksum, Is.EqualTo(expected));
                if (transfer) Assert.That(restored, Is.Not.SameAs(entity));
            }
            finally
            {
                if (target != null && !ReferenceEquals(source, target)) Stop(target);
                Stop(source);
            }
        }

        [TestCase(999)]
        [TestCase(1000)]
        public void UnavailableDescriptorRejectsTransferWithoutChangingTarget(int frameId)
        {
            var wrapper = Wrapper(false);
            var source = World(BattleRuntimeProfile.Authority400, wrapper);
            var target = World(BattleRuntimeProfile.Authority400, wrapper);
            try
            {
                var entity = Spawn(source);
                entity.Frame.D = new LF2FrameData { frameId = frameId, wait = 0 };
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = source.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(source.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
                var input = new FrameInputSet(0, Array.Empty<SimulationPlayerInput>());
                string before = target.CaptureLockstepChecksumSnapshot(0, input).OverallChecksum;
                Assert.That(target.TryRestoreBattleStateSnapshot(identity, snapshot, out _), Is.False);
                Assert.That(target.CaptureLockstepChecksumSnapshot(0, input).OverallChecksum, Is.EqualTo(before));
                Assert.That(target.ObjectCount, Is.Zero);
            }
            finally { Stop(target); Stop(source); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitNullDescriptorIsPreservedRatherThanRepaired(bool transfer)
        {
            var wrapper = Wrapper(false);
            var source = World(BattleRuntimeProfile.Authority400, wrapper);
            SimulationWorld target = null;
            try
            {
                var entity = Spawn(source);
                Bind(entity, 857, 998);
                entity.Frame.D = null; entity.Frame.Prev2D = null;
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = source.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(source.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                target = transfer ? World(BattleRuntimeProfile.Authority400, wrapper) : source;
                if (transfer) snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
                else Bind(entity, 0, 0);
                Assert.That(target.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                var restored = target.FindEntityByRuntimeSlotForQuery(3);
                Assert.That(restored.Frame.N, Is.EqualTo(857));
                Assert.That(restored.Frame.D, Is.Null);
                Assert.That(restored.Frame.Prev2D, Is.Null);
            }
            finally
            {
                if (target != null && !ReferenceEquals(source, target)) Stop(target);
                Stop(source);
            }
        }

        internal static void Bind(LF2Entity entity, int current, int collision)
        {
            entity.Frame.N = current; entity.Runtime.Frame = current;
            entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(current);
            entity.Frame.Prev = current;
            entity.Frame.Prev2 = collision; entity.Runtime.PrevFrame2 = collision;
            entity.Frame.Prev2D = entity.FrameCache.GetNativeFrameDataById(collision);
            entity.Trans.SyncDirectFrameData(entity.Frame.D?.wait ?? 0, entity.Frame.D?.next ?? 0, current);
        }

        private static LF2CharacterDataWrapper Wrapper(bool include999)
        {
            var data = new LF2CharacterData();
            data.frames.Add(new LF2FrameData { frameId = 0, wait = 10, next = 0 });
            if (include999) data.frames.Add(new LF2FrameData { frameId = 999, wait = 2, next = 0, pic = 29 });
            return new LF2CharacterDataWrapper(31989, data);
        }

        private static SimulationWorld World(BattleRuntimeProfile profile, LF2CharacterDataWrapper wrapper)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1050,
                characterConfigResolver: new RuntimeCharacterConfigResolver(_ => wrapper));
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(31989, 0, "native-frame-snapshot.dat") }, _ => wrapper);
            world.SetLogicOnlyEntityMaterialization(true);
            return world;
        }

        private static LF2Entity Spawn(SimulationWorld world)
        {
            var task = new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = 3, preserveActionZero = true, dir = "right",
                opoint = new ObjectPoint { oid = 31989, kind = 1, action = 0 },
            };
            var entity = world.LogicEntityFactory.Create(task, out var failure);
            Assert.That(entity, Is.Not.Null, failure.ToString());
            return entity;
        }

        private static void Stop(SimulationWorld world)
        {
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06NativeFrameSnapshotPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_NativeFrameSnapshotPlay.request";
        private const string Result = "Temp/NTSD28_Q06_NativeFrameSnapshotPlay.result.json";

        static NTSD28Q06NativeFrameSnapshotPlayProbe()
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
                    NTSD28Q06NativeFrameSnapshotEditorTests.Bind(entity, 857, 998);
                    var modified = world.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, modified), Is.True);
                    string modifiedChecksum = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum;
                    NTSD28Q06NativeFrameSnapshotEditorTests.Bind(entity, 0, 0);
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, modified, out var failure), Is.True, failure.ToString());
                    var restored = world.FindEntityByRuntimeSlotForQuery(0);
                    Assert.That(restored, Is.SameAs(entity));
                    Assert.That(restored.Frame.D.frameId, Is.EqualTo(857));
                    Assert.That(restored.Frame.Prev2D.frameId, Is.EqualTo(998));
                    Assert.That(restored.Trans.WaitCounter, Is.EqualTo(857));
                    Assert.That(world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum, Is.EqualTo(modifiedChecksum));
                    report.currentDescriptor = restored.Frame.D.frameId;
                    report.collisionDescriptor = restored.Frame.Prev2D.frameId;
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
            public string scope = "Current-content real Scene paused snapshot descriptor restore only; no complete high-action tick.";
            public int tick, objectsBefore, objectsAfter, currentDescriptor, collisionDescriptor;
            public bool modifiedChecksumRestored, originalChecksumRestored;
        }
    }
}
#endif
