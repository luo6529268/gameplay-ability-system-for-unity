#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q05SnapshotRetiredShellPoolEditorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void RestoreReturnsOnlyDiscardedBorrowersAndShutdownHasNoOrphans(bool pureValue)
        {
            SimulationWorld world = CreateWorld();
            var kept = Spawn(world, 50);
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            var future = Spawn(world, 51);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(2));
            if (pureValue) snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
            Assert.That(world.ObjectCount, Is.EqualTo(1));
            Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(1));
            if (!pureValue) Assert.That(world.FindEntityByRuntimeSlotForQuery(50), Is.SameAs(kept));
            Assert.That(future.Runtime.SlotIndex, Is.EqualTo(-1));
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
        }

        [Test]
        public void DiscardedRendererBindingFailsBeforeWorldMutation()
        {
            SimulationWorld world = CreateWorld();
            var entity = Spawn(world, 50);
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
            var host = new GameObject("Q05RetiredRenderer") { hideFlags = HideFlags.HideAndDontSave };
            var rendererProperty = typeof(LF2Entity).GetProperty("Renderer", BindingFlags.Instance | BindingFlags.Public);
            try
            {
                var renderer = host.AddComponent<LF2ObjectRenderer>();
                rendererProperty.SetValue(entity, renderer);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.False);
                Assert.That(failure, Is.EqualTo(BattleStateSnapshotRestoreFailure.EntityShellMismatch));
                Assert.That(world.FindEntityByRuntimeSlotForQuery(50), Is.SameAs(entity));
                Assert.That(entity.Renderer, Is.SameAs(renderer));
                Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(1));
            }
            finally
            {
                rendererProperty.SetValue(entity, null);
                Object.DestroyImmediate(host);
            }
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
        }

        private static SimulationWorld CreateWorld()
        {
            var world = new SimulationWorld();
            var data = new LF2CharacterData();
            data.frames.Add(new LF2FrameData { frameId = 0, wait = 1000, next = 0 });
            var wrapper = new LF2CharacterDataWrapper(31991, data);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(31991, 4, "fixture.dat") }, _ => wrapper);
            world.SetLogicOnlyEntityMaterialization(true);
            return world;
        }

        private static LF2Entity Spawn(SimulationWorld world, int slot)
        {
            var task = world.LogicReferencePool.Fetch<OPointCreateTask>();
            task.targetWorld = world;
            task.requiredRuntimeSlot = slot;
            task.opoint = new ObjectPoint { oid = 31991, kind = 1, action = 0 };
            task.preserveActionZero = true;
            task.useDirectRuntimePosition = true;
            task.skipPostInitZOffset = true;
            task.dir = "right";
            try
            {
                var entity = world.LogicEntityFactory.Create(task, out var failure);
                Assert.That(entity, Is.Not.Null, failure.ToString());
                return entity;
            }
            finally { world.LogicReferencePool.Recycle(task); }
        }
    }
}
#endif
