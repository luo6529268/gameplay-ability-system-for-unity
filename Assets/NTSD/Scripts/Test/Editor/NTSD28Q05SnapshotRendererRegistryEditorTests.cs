#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q05SnapshotRendererRegistryEditorTests
    {
        [Test]
        public void InPlaceRestoreRetainsRegisteredRendererAndRepeatsWithoutAllocation()
        {
            using var scope = new Scope();
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = scope.World.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(scope.World.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            Assert.That(snapshot.Core.ObjectCount, Is.EqualTo(2));
            Assert.That(snapshot.RuntimeSlots.ClaimedCount, Is.EqualTo(1));
            var input = new FrameInputSet(0, Array.Empty<SimulationPlayerInput>());
            string expected = scope.World.CaptureLockstepChecksumSnapshot(0, input).OverallChecksum;
            scope.Entity.Runtime.X = 991;
            Assert.That(scope.World.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
            Assert.That(scope.World.CaptureLockstepChecksumSnapshot(0, input).OverallChecksum, Is.EqualTo(expected));
            for (int index = 0; index < 8; index++)
                Assert.That(scope.World.TryRestoreBattleStateSnapshot(identity, snapshot, out failure), Is.True);
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool success = true;
            for (int index = 0; index < 8; index++)
                success &= scope.World.TryRestoreBattleStateSnapshot(identity, snapshot, out failure);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(success, Is.True, failure.ToString());
            Assert.That(allocated, Is.Zero);
            Assert.That(scope.World.ObjectCount, Is.EqualTo(2));
            Assert.That(scope.Entity.Renderer, Is.SameAs(scope.Renderer));
            Assert.That(scope.Renderer.LogicObject, Is.SameAs(scope.Entity));
            scope.World.Unregister(scope.Renderer);
            Assert.That(scope.World.ObjectCount, Is.EqualTo(1), "Renderer remains registered exactly once.");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ClaimedButInactiveRuntimeRestoresItsOriginalActiveCount(bool pendingDestroy)
        {
            using var scope = new Scope();
            scope.Entity.Runtime.OidMergeDormant = !pendingDestroy;
            scope.Entity.Runtime.PendingFlushDestroy = pendingDestroy;
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = scope.World.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(scope.World.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            Assert.That(snapshot.Core.ObjectCount, Is.EqualTo(1));
            scope.Entity.Runtime.OidMergeDormant = false;
            scope.Entity.Runtime.PendingFlushDestroy = false;
            Assert.That(scope.World.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
            Assert.That(scope.World.ObjectCount, Is.EqualTo(1));
            Assert.That(scope.World.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(1));
            Assert.That(scope.Entity.Runtime.PendingFlushDestroy, Is.EqualTo(pendingDestroy));
            Assert.That(scope.Entity.Runtime.OidMergeDormant, Is.EqualTo(!pendingDestroy));
        }

        [Test]
        public void UnmodeledSimulationObjectIsRejectedWithoutDroppingIt()
        {
            using var scope = new Scope();
            var unknown = new UnknownSimObject();
            scope.World.Register(unknown);
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = scope.World.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(scope.World.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            scope.Entity.Runtime.X = 515;
            Assert.That(scope.World.TryRestoreBattleStateSnapshot(identity, snapshot, out _), Is.False);
            Assert.That(scope.World.ObjectCount, Is.EqualTo(3));
            Assert.That(scope.Entity.Runtime.X, Is.EqualTo(515));
            scope.World.Unregister(unknown);
        }

        [Test]
        public void BrokenRendererOwnerBindingIsRejectedBeforeMutation()
        {
            using var scope = new Scope();
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = scope.World.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(scope.World.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            Scope.LogicField.SetValue(scope.Renderer, null);
            scope.Entity.Runtime.X = 616;
            Assert.That(scope.World.TryRestoreBattleStateSnapshot(identity, snapshot, out _), Is.False);
            Assert.That(scope.Entity.Runtime.X, Is.EqualTo(616));
            Assert.That(scope.World.ObjectCount, Is.EqualTo(2));
        }

        private sealed class UnknownSimObject : ISimObject
        {
            public int SimOrder => 99;
            public int StableId => 991;
        }

        private sealed class Scope : IDisposable
        {
            internal static readonly FieldInfo LogicField = typeof(LF2ObjectRenderer)
                .GetField("_logicObject", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly PropertyInfo RendererProperty = typeof(LF2Entity).GetProperty("Renderer");
            internal readonly SimulationWorld World = new SimulationWorld();
            internal readonly LF2Character Entity = new LF2Character { ObjectId = 7 };
            internal readonly LF2ObjectRenderer Renderer;
            private readonly GameObject host;

            internal Scope()
            {
                World.Register(Entity);
                host = new GameObject("Q05SnapshotRenderer") { hideFlags = HideFlags.HideAndDontSave };
                host.SetActive(false);
                Renderer = host.AddComponent<LF2ObjectRenderer>();
                LogicField.SetValue(Renderer, Entity);
                RendererProperty.SetValue(Entity, Renderer);
                World.Register(Renderer);
            }

            public void Dispose()
            {
                World.Unregister(Renderer);
                RendererProperty.SetValue(Entity, null);
                LogicField.SetValue(Renderer, null);
                UnityEngine.Object.DestroyImmediate(host);
                World.BeginBattleShutdown();
                Assert.That(World.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }
    }
}
#endif
