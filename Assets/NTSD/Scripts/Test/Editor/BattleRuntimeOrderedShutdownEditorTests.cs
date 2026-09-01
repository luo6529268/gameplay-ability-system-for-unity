#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleRuntimeOrderedShutdownEditorTests
    {
        [Test]
        public void DriverShutdown_ClosesRuntimeStagesBeforeStoppedAndIsIdempotent()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Preparing));

            driver.SetPaused(false);
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Running));

            BattleRuntimeShutdownReport runtimeReport =
                driver.ShutdownBattleRuntime();
            Assert.That(runtimeReport.Status,
                Is.EqualTo(BattleRuntimeShutdownStatus.AwaitingRuntimeMapCleanup));
            Assert.That(runtimeReport.CompletedStage,
                Is.EqualTo(BattleRuntimeShutdownStage.ObjectPoolQuiesced));
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Stopping));
            Assert.That(driver.IsPaused, Is.True);
            Assert.That(driver.StepOneTick(ignorePaused: true), Is.False,
                "ignorePaused must never bypass Stopping");
            driver.SetPaused(false);
            Assert.That(driver.IsPaused, Is.True);

            BattleRuntimeShutdownReport mapPending =
                driver.CompleteBattleRuntimeShutdownAfterMapCleanup(false);
            Assert.That(mapPending.Status,
                Is.EqualTo(BattleRuntimeShutdownStatus.Failed));
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Stopping));

            BattleRuntimeShutdownReport completed =
                driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true);
            Assert.That(completed.Status,
                Is.EqualTo(BattleRuntimeShutdownStatus.Completed));
            Assert.That(completed.CompletedStage,
                Is.EqualTo(BattleRuntimeShutdownStage.RuntimeMapCleared));
            Assert.That(completed.FailureReason, Is.Empty);
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Stopped));
            Assert.That(driver.World, Is.Null);

            BattleRuntimeShutdownReport repeated = driver.ShutdownBattleRuntime();
            Assert.That(repeated.Status,
                Is.EqualTo(BattleRuntimeShutdownStatus.AlreadyStopped));
            Assert.That(repeated.IsComplete, Is.True);
        }

        [Test]
        public void LogicOpointShutdown_RejectsAndRecyclesWithoutMaterializing()
        {
            var world = new SimulationWorld();
            var referencePool = new BattleLogicReferencePool();
            referencePool.PrewarmTasks<OPointCreateTask>(2);
            world.BindLogicReferencePool(referencePool);
            world.SetLogicOnlyEntityMaterialization(true);
            world.BeginBattlePreparation();

            OPointCreateTask pending = referencePool.Fetch<OPointCreateTask>();
            Assert.That(pending, Is.Not.Null);
            pending.targetWorld = world;
            pending.opoint = new ObjectPoint { oid = 1 };
            world.ResolveObjectPointFactoryForSimulation().EnqueueCreateObject(pending);
            Assert.That(world.LogicObjectPointRuntime.PendingTaskCountForDiagnostics,
                Is.EqualTo(1));

            int availableBeforeDiscard = referencePool.AvailableCreateTaskCount;
            world.BeginBattleShutdown();
            int discarded = world.DiscardPendingObjectPointTasks();
            Assert.That(discarded, Is.EqualTo(1));
            Assert.That(world.LogicObjectPointRuntime.PendingTaskCountForDiagnostics,
                Is.Zero);
            Assert.That(referencePool.AvailableCreateTaskCount,
                Is.EqualTo(availableBeforeDiscard + 1));

            OPointCreateTask rejected = referencePool.Fetch<OPointCreateTask>();
            rejected.targetWorld = world;
            rejected.opoint = new ObjectPoint { oid = 1 };
            world.ResolveObjectPointFactoryForSimulation().EnqueueCreateObject(rejected);
            Assert.That(world.LogicObjectPointRuntime.PendingTaskCountForDiagnostics,
                Is.Zero);
            Assert.That(world.ObjectCount, Is.Zero,
                "shutdown reject must not materialize an entity");
        }

        [Test]
        public void StructuralShutdown_RejectsRegisterButAllowsExistingWorldReset()
        {
            var world = new SimulationWorld();
            world.BeginBattlePreparation();
            world.BeginBattleShutdown();

            var candidate = new DummySimObject();
            world.Register(candidate);
            Assert.That(world.ObjectCount, Is.Zero);
            Assert.That(world.StructuralWriter.AcceptingStructuralCreatesForDiagnostics,
                Is.False);
            Assert.That(world.StructuralWriter.ShutdownRejectedCreateCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(world.TryShutdownAndClearLogicState(out int released, out string failure),
                Is.True, failure);
            Assert.That(released, Is.Zero);
            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void ObjectPoolShutdown_ReturnsAllBorrowersAndQuiesces()
        {
            GameObject host = new GameObject("OrderedShutdown_ObjectPool");
            try
            {
                LF2ObjectPool pool = host.AddComponent<LF2ObjectPool>();
                MethodInfo awake = typeof(LF2ObjectPool).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(awake, Is.Not.Null);
                awake.Invoke(pool, null);
                GameObject borrowed = pool.Get(out LF2ObjectRenderer renderer);
                SpriteRenderer sprite = pool.GetSprite();
                Assert.That(borrowed, Is.Not.Null);
                Assert.That(renderer, Is.Not.Null);
                Assert.That(sprite, Is.Not.Null);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.EqualTo(1));
                Assert.That(pool.ActiveSpriteCountForAcceptance, Is.EqualTo(1));

                pool.BeginBattleShutdown();
                Assert.That(pool.Get(out _), Is.Null);
                Assert.That(pool.GetSprite(), Is.Null);
                Assert.That(pool.ReleaseAllActiveForShutdown(
                    out int returnedRenderers,
                    out int returnedSprites,
                    out string releaseFailure), Is.True, releaseFailure);
                Assert.That(returnedRenderers, Is.EqualTo(1));
                Assert.That(returnedSprites, Is.EqualTo(1));
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
                Assert.That(pool.ActiveSpriteCountForAcceptance, Is.Zero);
                Assert.That(pool.CompleteBattleQuiesce(out string quiesceFailure),
                    Is.True, quiesceFailure);
                Assert.That(pool.IsQuiescedForDiagnostics, Is.True);
                Assert.That(pool.AcceptingRequestsForDiagnostics, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private sealed class DummySimObject : ISimObject
        {
            public int SimOrder => 17;
            public int StableId => 1701;
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly FieldInfo referencePoolInstanceField;
            private readonly LF2ReferencePool previousReferencePool;
            private readonly GameObject host;

            internal DriverScope()
            {
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                referencePoolInstanceField = typeof(MMSingleton<LF2ReferencePool>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(referencePoolInstanceField, Is.Not.Null);
                previousReferencePool =
                    referencePoolInstanceField.GetValue(null) as LF2ReferencePool;
                host = new GameObject("BattleRuntimeOrderedShutdownEditorTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                Driver.RecreateWorld();
            }

            internal SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                LF2ReferencePool current =
                    referencePoolInstanceField.GetValue(null) as LF2ReferencePool;
                if (current != null && current != previousReferencePool)
                    UnityEngine.Object.DestroyImmediate(current.gameObject);
                referencePoolInstanceField.SetValue(null, previousReferencePool);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
#endif
