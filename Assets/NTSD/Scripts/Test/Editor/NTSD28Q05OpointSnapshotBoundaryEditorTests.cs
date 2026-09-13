#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05OpointSnapshotBoundaryEditorTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static LockstepSessionIdentity Identity => NTSD.Test.StrictDelayedInputBufferEditorTests.CreateIdentity();

        internal static BattleStateSnapshotBuffer Capture(SimulationWorld world)
        {
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(Identity, world.CurrentTickIndex, snapshot), Is.True);
            return snapshot;
        }

        internal static void AssertRejected(SimulationWorld world, BattleStateSnapshotBuffer saved, BattleStateSnapshotBuffer target)
        {
            ulong checksum = world.CaptureRuntimeChecksum64(world.CurrentTickIndex, null);
            Assert.That(world.TryCaptureBattleStateSnapshot(Identity, world.CurrentTickIndex, target), Is.False);
            Assert.That(target.IsValid, Is.False);
            Assert.That(world.TryRestoreBattleStateSnapshot(Identity, saved, out var reason), Is.False);
            Assert.That(reason, Is.EqualTo(BattleStateSnapshotRestoreFailure.WorldBusy));
            Assert.That(saved.IsValid, Is.True);
            Assert.That(world.CaptureRuntimeChecksum64(world.CurrentTickIndex, null), Is.EqualTo(checksum));
        }

        internal static LF2TaskRingBuffer Queue(object owner, string field)
        {
            return (LF2TaskRingBuffer)owner.GetType().GetField(field, PrivateInstance).GetValue(owner);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LogicPendingTaskRejectsWithoutConsumingOrChangingWorld(bool multiple)
        {
            var world = new SimulationWorld();
            var foreign = new SimulationWorld();
            var saved = Capture(world);
            var target = Capture(world);
            LF2TaskBase task;
            if (multiple)
            {
                var value = new OPointCreateMultipleTask { targetWorld = foreign, number = 3 };
                task = value;
                world.LogicObjectPointRuntime.EnqueueCreateMultipleObjects(value);
            }
            else
            {
                var value = new OPointCreateTask { targetWorld = foreign, ownerEntityIndex = 31 };
                task = value;
                world.LogicObjectPointRuntime.EnqueueCreateObject(value);
            }
            var queue = Queue(world.LogicObjectPointRuntime, "taskQueue");
            try
            {
                AssertRejected(world, saved, target);
                Assert.That(queue.Count, Is.EqualTo(1));
                Assert.That(Capture(foreign).IsValid, Is.True, "The logic queue belongs to its runtime owner, not task.targetWorld.");
            }
            finally
            {
                Assert.That(queue.TryDequeue(out LF2TaskBase retained), Is.True);
                Assert.That(retained, Is.SameAs(task));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RendererPendingTaskIsFilteredByActualWorld(bool multiple)
        {
            using var factory = new FactoryScope();
            var world = new SimulationWorld();
            var other = new SimulationWorld();
            var saved = Capture(world);
            var target = Capture(world);
            LF2TaskBase task;
            if (multiple)
            {
                var value = new OPointCreateMultipleTask { targetWorld = world, number = 2 };
                task = value;
                factory.Factory.EnqueueCreateMultipleObjects(value);
            }
            else
            {
                var value = new OPointCreateTask { targetWorld = world };
                task = value;
                factory.Factory.EnqueueCreateObject(value);
            }
            AssertRejected(world, saved, target);
            Assert.That(Capture(other).IsValid, Is.True);
            Assert.That(factory.Tasks.Count, Is.EqualTo(1));
            Assert.That(factory.Tasks.TryDequeue(out var retained), Is.True);
            Assert.That(retained, Is.SameAs(task));
        }

        [Test]
        public void RendererRegisteredParentResolvesOwnershipWithoutCreatingServices()
        {
            using var factory = new FactoryScope();
            var world = new SimulationWorld();
            var other = new SimulationWorld();
            var parent = new LF2Character();
            world.Register(parent);
            var saved = Capture(world);
            var target = Capture(world);
            factory.Factory.EnqueueCreateObject(new OPointCreateTask { parent = parent });
            AssertRejected(world, saved, target);
            Assert.That(Capture(other).IsValid, Is.True);
            Assert.That(factory.Tasks.Count, Is.EqualTo(1));
        }

        [Test]
        public void RingRefusalDoesNotAdvanceOrPublishItsStagingBuffer()
        {
            var world = new SimulationWorld();
            world.AdvanceBattleFlowTick(1);
            var ring = new LockstepSnapshotRing(Identity, world, 1, 2);
            world.LogicObjectPointRuntime.EnqueueCreateObject(new OPointCreateTask());
            var queue = Queue(world.LogicObjectPointRuntime, "taskQueue");
            Assert.That(ring.TryCaptureNext(1, out var reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.SnapshotCaptureFailed));
            Assert.That(ring.Count, Is.Zero);
            Assert.That(ring.ShouldCapture(1), Is.True);
            Assert.That(queue.Count, Is.EqualTo(1));
            queue.TryDequeue(out _);
            Assert.That(ring.TryCaptureNext(1, out reason), Is.True);
        }

        [Test]
        public void FullTickBoundaryAndShutdownRejectSnapshots()
        {
            var world = new SimulationWorld();
            var saved = Capture(world);
            var target = Capture(world);
            MethodInfo enter = typeof(SimulationWorld).GetMethod("EnterSnapshotTickBoundary", PrivateInstance);
            MethodInfo exit = typeof(SimulationWorld).GetMethod("ExitSnapshotTickBoundary", PrivateInstance);
            Assert.That(enter, Is.Not.Null);
            Assert.That(exit, Is.Not.Null);
            enter.Invoke(world, null);
            try { AssertRejected(world, saved, target); }
            finally { exit.Invoke(world, null); }
            Assert.That(Capture(world).IsValid, Is.True);
            world.BeginBattleShutdown();
            AssertRejected(world, saved, target);
        }

        [TestCase("single", false)]
        [TestCase("multiple", false)]
        [TestCase("late", false)]
        [TestCase("single", true)]
        public void StructuralPlaybackRejectsCaptureAndBalancesOnReturnOrThrow(string mode, bool throws)
        {
            var world = new SimulationWorld();
            var saved = Capture(world);
            var target = Capture(world);
            var materializer = new ProbeMaterializer
            {
                Callback = () =>
                {
                    AssertRejected(world, saved, target);
                    if (throws) throw new InvalidOperationException("expected probe exception");
                },
            };
            Action run = () =>
            {
                if (mode == "multiple") world.StructuralWriter.SpawnMultiple(materializer, new OPointCreateMultipleTask(), BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
                else if (mode == "late") world.StructuralWriter.ProcessLateOpointSegment(materializer, new LF2Character(), 1);
                else world.StructuralWriter.Spawn(materializer, new OPointCreateTask(), BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
            };
            if (throws) Assert.Throws<InvalidOperationException>(() => run());
            else run();
            Assert.That(materializer.Calls, Is.EqualTo(1));
            Assert.That(Capture(world).IsValid, Is.True);
        }

        [Test]
        public void BusyHostRestoreDoesNotStopOrResetWorkerSubmission()
        {
            using var scope = new DriverScope();
            var saved = Capture(scope.Driver.World);
            FieldInfo flight = typeof(SimulationTickDriver).GetField("_simulationWorkerTickInFlight", PrivateInstance);
            FieldInfo submitted = typeof(SimulationTickDriver).GetField("_simulationWorkerSubmittedTick", PrivateInstance);
            flight.SetValue(scope.Driver, true);
            submitted.SetValue(scope.Driver, 17);
            try
            {
                Assert.That(scope.Driver.TryRestoreBattleStateSnapshot(Identity, saved, out var reason), Is.False);
                Assert.That(reason, Is.EqualTo(BattleStateSnapshotRestoreFailure.WorldBusy));
                Assert.That(flight.GetValue(scope.Driver), Is.True);
                Assert.That(submitted.GetValue(scope.Driver), Is.EqualTo(17));
                Assert.That(saved.IsValid, Is.True);
            }
            finally { flight.SetValue(scope.Driver, false); submitted.SetValue(scope.Driver, 0); }
        }

        [Test]
        public void SessionTickMismatchInvalidatesDestination()
        {
            using var scope = new DriverScope();
            var session = new BattleLockstepSession(scope.Driver, Identity, 0, 8, 8);
            var target = Capture(scope.Driver.World);
            typeof(SimulationTickDriver).GetField("_tickIndex", PrivateInstance).SetValue(scope.Driver, 1);
            Assert.That(session.TryCaptureBattleStateSnapshot(target), Is.False);
            Assert.That(target.IsValid, Is.False);
        }

        [Test]
        public void QueuePeekPreservesWrappedOrderAndCounters()
        {
            var queue = new LF2TaskRingBuffer(2);
            var first = new OPointCreateTask();
            var second = new OPointCreateTask();
            var third = new OPointCreateTask();
            queue.TryEnqueue(first); queue.TryEnqueue(second); queue.TryDequeue(out _); queue.TryEnqueue(third);
            MethodInfo peek = typeof(LF2TaskRingBuffer).GetMethod("TryPeekAt", PrivateInstance);
            Assert.That(peek, Is.Not.Null);
            object[] args = { 0, null };
            Assert.That(peek.Invoke(queue, args), Is.True);
            Assert.That(args[1], Is.SameAs(second));
            args[0] = 1;
            Assert.That(peek.Invoke(queue, args), Is.True);
            Assert.That(args[1], Is.SameAs(third));
            args[0] = 2;
            Assert.That(peek.Invoke(queue, args), Is.False);
            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(queue.RejectedEnqueueCount, Is.Zero);
        }

        [Test]
        public void WarmBoundaryObservationDoesNotAllocateOrCreateFactory()
        {
            FieldInfo field = typeof(MoreMountains.Tools.MMSingleton<LF2ObjectPointFactory>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            object previous = field.GetValue(null);
            field.SetValue(null, null);
            try
            {
                var world = new SimulationWorld();
                MethodInfo get = typeof(SimulationWorld).GetProperty("IsBattleSnapshotBoundaryReady", PrivateInstance).GetGetMethod(true);
                var ready = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), world, get);
                for (int i = 0; i < 10; i++) Assert.That(ready(), Is.True);
                long before = GC.GetAllocatedBytesForCurrentThread();
                bool allReady = true;
                for (int i = 0; i < 1000; i++) allReady &= ready();
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(allReady, Is.True);
                Assert.That(allocated, Is.Zero);
                Assert.That(field.GetValue(null), Is.Null);
            }
            finally { field.SetValue(null, previous); }
        }

        [Test]
        public void RendererExplicitTargetWinsOverRegisteredParent()
        {
            using var factory = new FactoryScope();
            var parentWorld = new SimulationWorld();
            var targetWorld = new SimulationWorld();
            var parent = new LF2Character();
            parentWorld.Register(parent);
            var saved = Capture(targetWorld);
            var target = Capture(targetWorld);
            factory.Factory.EnqueueCreateObject(new OPointCreateTask { parent = parent, targetWorld = targetWorld });
            Assert.That(Capture(parentWorld).IsValid, Is.True);
            AssertRejected(targetWorld, saved, target);
        }

        [Test]
        public void UnboundRendererTaskUsesExistingHostWorldOnly()
        {
            using var driver = new DriverScope();
            using var factory = new FactoryScope();
            var world = driver.Driver.World;
            var saved = Capture(world);
            var target = Capture(world);
            factory.Factory.EnqueueCreateObject(new OPointCreateTask());
            AssertRejected(world, saved, target);
            Assert.That(Capture(new SimulationWorld()).IsValid, Is.True);
        }

        [Test]
        public void ActualTickCarriesBoundaryThroughTypeQueriesAndReleasesIt()
        {
            var world = new SimulationWorld();
            var entity = new TypeQueryProbe();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(9000, new LF2CharacterData
            {
                type_sub = 0,
                frames = new System.Collections.Generic.List<LF2FrameData>
                {
                    new LF2FrameData { frameId = 0, state = 0, wait = 100, next = 0 },
                },
            }));
            entity.ObjectId = 9000;
            world.Register(entity);
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            var target = world.CreateBattleStateSnapshotBufferForBootstrap();
            bool called = false;
            bool captured = true;
            entity.Query = () =>
            {
                if (called) return;
                called = true;
                captured = world.TryCaptureBattleStateSnapshot(Identity, world.CurrentTickIndex, target);
            };
            new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
            entity.Query = null;
            Assert.That(called, Is.True);
            Assert.That(captured, Is.False);
            Assert.That(Capture(world).IsValid, Is.True);
        }

        private sealed class TypeQueryProbe : LF2Character
        {
            internal Action Query;
            public override int GetCurrentDataObjectTypeForSimulation() { Query?.Invoke(); return 0; }
        }

        private sealed class ProbeMaterializer : IBattleObjectPointStructuralMaterializer
        {
            internal Action Callback;
            internal int Calls;
            private void Invoke() { Calls++; Callback(); }
            public void FlushTasks() => Invoke();
            public void ProcessOpointSpawnCoreForStructuralWriter(LF2Entity spawner) => Invoke();
            public LF2Entity MaterializeObjectForStructuralWriter(OPointCreateTask task) { Invoke(); return null; }
            public void MaterializeMultipleObjectsForStructuralWriter(OPointCreateMultipleTask task) => Invoke();
        }

        internal sealed class FactoryScope : IDisposable
        {
            private readonly FieldInfo instance;
            private readonly object previous;
            private readonly GameObject host;
            internal readonly LF2ObjectPointFactory Factory;
            internal LF2TaskRingBuffer Tasks => Queue(Factory, "_taskQueue");
            internal FactoryScope()
            {
                instance = typeof(MoreMountains.Tools.MMSingleton<LF2ObjectPointFactory>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                previous = instance.GetValue(null);
                host = new GameObject("SnapshotQueueFactory") { hideFlags = HideFlags.HideAndDontSave };
                host.SetActive(false);
                Factory = host.AddComponent<LF2ObjectPointFactory>();
                instance.SetValue(null, Factory);
            }
            public void Dispose()
            {
                while (Tasks.TryDequeue(out _)) { }
                instance.SetValue(null, previous);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instance;
            private readonly object previous;
            private readonly GameObject host;
            internal readonly SimulationTickDriver Driver;
            internal DriverScope()
            {
                instance = typeof(SimulationTickDriver).BaseType.GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
                previous = instance.GetValue(null);
                instance.SetValue(null, null);
                host = new GameObject("SnapshotBoundaryDriver") { hideFlags = HideFlags.HideAndDontSave };
                Driver = host.AddComponent<SimulationTickDriver>();
                Driver.RecreateWorld();
                Driver.SetPaused(true);
            }
            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                instance.SetValue(null, previous);
            }
        }
    }
}
#endif
