#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class StableIdDeterminismEditorTests
    {
        [Test]
        public void ResetWorldWorkload_ColdAndWarmLogicAndRenderersKeepSlotChecksumStable()
        {
            using var logging = new DisabledLoggingScope();
            using var driverScope = new SimulationDriverWorldScope();
            var world = new SimulationWorld();
            driverScope.SetWorld(world);
            world.ResetRuntimeState();

            var logic = new StableIdProbeEntity();
            GameObject coldRendererObject = CreateInactiveRenderer(
                "StableId_ColdRenderer",
                out LF2ObjectRenderer coldRenderer);
            GameObject warmedRendererObject = null;

            try
            {
                BattleParityFrameSnapshot coldLogicColdRenderer =
                    RunSingleEntityWorkload(world, logic, coldRenderer);

                world.ResetRuntimeState();
                warmedRendererObject = CreateInactiveRenderer(
                    "StableId_WarmedRenderer",
                    out LF2ObjectRenderer warmedRenderer);
                BattleParityFrameSnapshot warmLogicColdRenderer =
                    RunSingleEntityWorkload(world, logic, warmedRenderer);

                world.ResetRuntimeState();
                BattleParityFrameSnapshot warmLogicWarmRenderer =
                    RunSingleEntityWorkload(world, logic, warmedRenderer);

                Assert.That(coldLogicColdRenderer.Hashes.Slots,
                    Is.EqualTo(warmLogicColdRenderer.Hashes.Slots));
                Assert.That(coldLogicColdRenderer.Hashes.Slots,
                    Is.EqualTo(warmLogicWarmRenderer.Hashes.Slots));
                Assert.That(coldLogicColdRenderer.OverallChecksum,
                    Is.EqualTo(warmLogicColdRenderer.OverallChecksum));
                Assert.That(coldLogicColdRenderer.OverallChecksum,
                    Is.EqualTo(warmLogicWarmRenderer.OverallChecksum));
            }
            finally
            {
                world.ResetRuntimeState();
                UnityEngine.Object.DestroyImmediate(coldRendererObject);
                if (warmedRendererObject != null)
                    UnityEngine.Object.DestroyImmediate(warmedRendererObject);
            }
        }

        [Test]
        public void ExplicitDuplicateFailedAdmissionAndSameSlotReuse_CommitIdentityExactlyOnce()
        {
            var world = new SimulationWorld();
            var explicitEntity = new StableIdProbeEntity();
            explicitEntity.Prepare(50, 150);
            world.Register(explicitEntity);

            Assert.That(explicitEntity.StableId, Is.EqualTo(150));
            Assert.That(explicitEntity.Runtime.SlotIndex, Is.EqualTo(50));
            uint firstGeneration = GetSlotGeneration(world, 50);

            var duplicate = new StableIdProbeEntity();
            duplicate.Prepare(51, 150);
            LogAssert.Expect(
                LogType.Error,
                new Regex("StableId registration rejected: StableId=150"));
            world.Register(duplicate);
            Assert.That(duplicate.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(1));

            var automatic = new StableIdProbeEntity();
            automatic.Prepare(51);
            world.Register(automatic);
            Assert.That(automatic.StableId, Is.EqualTo(151),
                "a committed explicit ID must advance the automatic allocation floor");

            var rejected = new StableIdProbeEntity();
            rejected.Prepare(51);
            LogAssert.Expect(
                LogType.Warning,
                new Regex("Runtime slot exhausted; registration rejected"));
            world.Register(rejected);
            Assert.That(rejected.StableId, Is.Zero,
                "failed runtime-slot admission must not allocate a visible identity");

            world.Unregister(automatic);
            world.Register(rejected);
            Assert.That(rejected.StableId, Is.EqualTo(152),
                "failed admission must not advance the automatic identity sequence");
            Assert.That(rejected.Runtime.SlotIndex, Is.EqualTo(51));

            world.Unregister(explicitEntity);
            explicitEntity.Prepare(50);
            world.Register(explicitEntity);
            Assert.That(explicitEntity.StableId, Is.EqualTo(153));
            Assert.That(GetSlotGeneration(world, 50), Is.GreaterThan(firstGeneration));
        }

        [Test]
        public void ZeroIdentityRestBindRollback_DoesNotCommitOrConsumeAutomaticIdentity()
        {
            var world = new SimulationWorld();
            var foreignStore = new RuntimeRestStore(400);
            var rejected = new RestBindRollbackProbeEntity();
            Assert.That(rejected.BindForeignTracker(foreignStore, 20), Is.True);
            rejected.SetRequiredRuntimeSlot(20);

            LogAssert.Expect(
                LogType.Error,
                new Regex("Runtime rest bind failed; registration rejected"));
            world.Register(rejected);

            Assert.That(rejected.StableId, Is.Zero);
            Assert.That(rejected.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero);
            Assert.That(world.ObjectCount, Is.Zero);

            var accepted = new StableIdProbeEntity();
            accepted.Prepare(20);
            world.Register(accepted);
            Assert.That(accepted.StableId, Is.EqualTo(100),
                "rest-bind rollback with zero identity must not advance the allocator");
            Assert.That(accepted.Runtime.SlotIndex, Is.EqualTo(20));
        }

        [Test]
        public void ActiveRenderers_ReversedColdWarmActivationOrderPreservesLogicRngAndChecksum()
        {
            using var logging = new DisabledLoggingScope();
            using var driverScope = new SimulationDriverWorldScope();
            var world = new SimulationWorld();
            driverScope.SetWorld(world);
            world.ResetRuntimeState();

            var logic = new StableIdProbeEntity();
            GameObject firstObject = CreateInactiveRenderer(
                "StableId_ActiveRendererA",
                out _);
            GameObject secondObject = CreateInactiveRenderer(
                "StableId_ActiveRendererB",
                out _);

            try
            {
                firstObject.SetActive(true);
                secondObject.SetActive(true);
                logic.Prepare(50);
                world.Register(logic);
                int firstStableId = logic.StableId;
                uint firstRngState = world.Rng.State;
                BattleParityFrameSnapshot firstSnapshot =
                    world.CaptureParityFrameSnapshot(0);

                firstObject.SetActive(false);
                secondObject.SetActive(false);
                world.ResetRuntimeState();

                secondObject.SetActive(true);
                firstObject.SetActive(true);
                logic.Prepare(50);
                world.Register(logic);
                int secondStableId = logic.StableId;
                uint secondRngState = world.Rng.State;
                BattleParityFrameSnapshot secondSnapshot =
                    world.CaptureParityFrameSnapshot(0);

                Assert.That(firstStableId, Is.EqualTo(100));
                Assert.That(secondStableId, Is.EqualTo(firstStableId));
                Assert.That(secondRngState, Is.EqualTo(firstRngState));
                Assert.That(secondSnapshot.Hashes.Rng, Is.EqualTo(firstSnapshot.Hashes.Rng));
                Assert.That(secondSnapshot.Hashes.Slots, Is.EqualTo(firstSnapshot.Hashes.Slots));
                Assert.That(secondSnapshot.OverallChecksum,
                    Is.EqualTo(firstSnapshot.OverallChecksum));
            }
            finally
            {
                firstObject.SetActive(false);
                secondObject.SetActive(false);
                world.ResetRuntimeState();
                UnityEngine.Object.DestroyImmediate(firstObject);
                UnityEngine.Object.DestroyImmediate(secondObject);
            }
        }

        private static BattleParityFrameSnapshot RunSingleEntityWorkload(
            SimulationWorld world,
            StableIdProbeEntity entity,
            LF2ObjectRenderer renderer)
        {
            _ = renderer.StableId;
            entity.Prepare(50);
            world.Register(entity);
            Assert.That(entity.StableId, Is.EqualTo(100),
                "presentation allocation and pool warmth must not consume logic StableIds");
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(50));
            return world.CaptureParityFrameSnapshot(0);
        }

        private static GameObject CreateInactiveRenderer(
            string name,
            out LF2ObjectRenderer renderer)
        {
            var host = new GameObject(name)
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            host.SetActive(false);
            host.AddComponent<SpriteRenderer>();
            renderer = host.AddComponent<LF2ObjectRenderer>();
            return host;
        }

        private static uint GetSlotGeneration(SimulationWorld world, int runtimeSlot)
        {
            Assert.That(
                world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                    runtimeSlot,
                    out RuntimeSlotTable.ReadOnlySlotView view),
                Is.True);
            return view.Generation;
        }

        private sealed class StableIdProbeEntity : LF2OtherObject
        {
            public void Prepare(int requiredRuntimeSlot, int explicitStableId = 0)
            {
                Reset();
                ObjectId = 31998;
                Runtime.StableId = explicitStableId;
                Health.BindRuntime(Runtime);
                PS.BindRuntime(Runtime);
                ItrRest ??= new LF2ItrRestTracker();
                Trans ??= new FrameTransistor(this);
                SetRequiredRuntimeSlot(requiredRuntimeSlot);
            }
        }

        private sealed class RestBindRollbackProbeEntity : LF2Entity
        {
            private readonly LF2ItrRestTracker initialTracker = new LF2ItrRestTracker();
            private readonly LF2ItrRestTracker foreignTracker = new LF2ItrRestTracker();
            private bool initialTrackerRead;

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public override LF2ItrRestTracker ItrRest
            {
                get
                {
                    if (!initialTrackerRead)
                    {
                        initialTrackerRead = true;
                        return initialTracker;
                    }

                    return foreignTracker;
                }
                protected set { }
            }

            public bool BindForeignTracker(RuntimeRestStore store, int runtimeSlot)
            {
                return foreignTracker.Bind(store, runtimeSlot, false);
            }

            public override void Reset() { }

            public override void Init(
                NTSD.Animation.LF2Tasks.LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
                Renderer = renderer;
            }
        }

        private sealed class SimulationDriverWorldScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver originalInstance;
            private readonly SimulationTickDriver driver;
            private readonly FieldInfo worldField;
            private readonly SimulationWorld originalWorld;
            private readonly GameObject temporaryDriverObject;

            public SimulationDriverWorldScope()
            {
                const BindingFlags flags =
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType?.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                originalInstance = instanceField.GetValue(null) as SimulationTickDriver;
                driver = SimulationTickDriver.Instance;
                if (driver == null)
                {
                    temporaryDriverObject = new GameObject("StableId_SimulationTickDriver")
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    driver = temporaryDriverObject.AddComponent<SimulationTickDriver>();
                    instanceField.SetValue(null, driver);
                }

                worldField = typeof(SimulationTickDriver).GetField(
                    "_world",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(worldField, Is.Not.Null);
                originalWorld = worldField.GetValue(driver) as SimulationWorld;
            }

            public void SetWorld(SimulationWorld world)
            {
                worldField.SetValue(driver, world);
            }

            public void Dispose()
            {
                worldField.SetValue(driver, originalWorld);
                instanceField.SetValue(null, originalInstance);
                if (temporaryDriverObject != null)
                    UnityEngine.Object.DestroyImmediate(temporaryDriverObject);
            }
        }

        private sealed class DisabledLoggingScope : IDisposable
        {
            private readonly bool original;

            public DisabledLoggingScope()
            {
                original = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
            }

            public void Dispose()
            {
                Debug.unityLogger.logEnabled = original;
            }
        }
    }
}
#endif
