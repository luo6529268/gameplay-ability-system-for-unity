#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class W05OpointLifecycleEditorTests
    {
        private const int SpawnOid = 31995;

        [Test]
        public void W05A_ProductionFactory_ReleasesPendingHolesInLowestOrder_AndCapacityFailureDoesNotLeak()
        {
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
            using var isolatedPool = new IsolatedObjectPoolScope(pool);
            using var driver = new SimulationDriverWorldScope();

            var world = new SimulationWorld();
            driver.SetWorld(world);
            var spawner = new OpointSpawner(SpawnOid, "holes");
            var lowPending = new DynamicSlotOccupant(101);
            var ordinaryHole = new DynamicSlotOccupant(102);
            var highPending = new DynamicSlotOccupant(103);
            world.Register(spawner);
            world.Register(lowPending);
            world.Register(ordinaryHole);
            world.Register(highPending);
            Assert.That(spawner.Runtime.SlotIndex, Is.EqualTo(50));
            Assert.That(lowPending.Runtime.SlotIndex, Is.EqualTo(51));
            Assert.That(ordinaryHole.Runtime.SlotIndex, Is.EqualTo(52));
            Assert.That(highPending.Runtime.SlotIndex, Is.EqualTo(53));
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                51, lowPending, out RuntimeEntityHandle oldLowHandle), Is.True);
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                53, highPending, out RuntimeEntityHandle oldHighHandle), Is.True);

            lowPending.Runtime.PendingFlushDestroy = true;
            highPending.Runtime.PendingFlushDestroy = true;
            world.Unregister(ordinaryHole);

            var spawned = new List<LF2Entity>();
            for (int expectedSlot = 51; expectedSlot <= 53; expectedSlot++)
            {
                factory.ProcessOpointSpawn(spawner);
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(expectedSlot);
                Assert.That(entity, Is.Not.Null, $"production opoint must select hole {expectedSlot}");
                Assert.That(entity.ObjectId, Is.EqualTo(SpawnOid));
                spawned.Add(entity);
            }

            Assert.That(world.TryResolveRuntimeHandleForDiagnostics(oldLowHandle, out _), Is.False);
            Assert.That(world.TryResolveRuntimeHandleForDiagnostics(oldHighHandle, out _), Is.False);
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                51, spawned[0], out RuntimeEntityHandle newLowHandle), Is.True);
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                53, spawned[2], out RuntimeEntityHandle newHighHandle), Is.True);
            Assert.That(newLowHandle.Generation, Is.Not.EqualTo(oldLowHandle.Generation));
            Assert.That(newHighHandle.Generation, Is.Not.EqualTo(oldHighHandle.Generation));

            foreach (LF2Entity entity in spawned)
                entity.FreeEntityLikeExe();
            world.FlushPendingDestroyForDiagnostics();
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);

            var fullWorld = new SimulationWorld();
            driver.SetWorld(fullWorld);
            var fullSpawner = new OpointSpawner(SpawnOid, "capacity");
            fullWorld.Register(fullSpawner);
            for (int index = 0; index < 349; index++)
                fullWorld.Register(new DynamicSlotOccupant(1000 + index));
            Assert.That(fullWorld.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(350));
            int availableBefore = pool.AvailableObjectCountForAcceptance;
            int activeBefore = pool.ActiveObjectCountForAcceptance;

            factory.ProcessOpointSpawn(fullSpawner);

            Assert.That(pool.AvailableObjectCountForAcceptance, Is.EqualTo(availableBefore));
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.EqualTo(activeBefore));
            Assert.That(fullWorld.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(350));
        }

        [Test]
        public void W05B_RealLateOpoint_PublishesNextTickThenPoolReuseRejectsOldGenerationAndGhostCommand()
        {
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
            using var isolatedPool = new IsolatedObjectPoolScope(pool);
            using var sprites = new CharacterSpriteScope(SpawnOid);
            using var driver = new SimulationDriverWorldScope();

            factory.FlushTasks();
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            driver.SetWorld(world);
            var firstProducer = new OpointSpawner(SpawnOid, "first");
            var reuseProducer = new OpointSpawner(SpawnOid, "reuse")
            {
                AttackingCounter = 1,
            };
            world.Register(firstProducer);
            world.Register(reuseProducer);

            world.RenderDispatchAll(40);
            BattlePresentationFrame tick40PublishedFrame = world.BattlePresentation.PublishedFrame;
            BattlePresentationFrame tick40RenderedFrame = MaterializeCentralFrame(world);
            Assert.That(tick40PublishedFrame.TickIndex, Is.EqualTo(40),
                "W05B-A01: RenderDispatch(T) must publish tick 40");

            world.LateEntityUpdateAll(40);
            LF2Entity first = FindSpawn(world);
            Assert.That(first, Is.Not.Null,
                "W05B-A02: the first late opoint spawn must exist");
            firstProducer.AttackingCounter = 1;
            Assert.That(first.Runtime.SlotIndex, Is.EqualTo(52),
                "W05B-A03: the first spawn must claim the next high runtime slot");
            Assert.That(first.AttackingCounter, Is.EqualTo(1),
                "W05B-A04: a high-slot newborn must execute later in the same real late pass");
            Assert.That(first.Runtime.FirstPresentationTick, Is.EqualTo(0),
                "W05B-A05: pass ordering must not force FirstPresentationTick=T+1");
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                first.Runtime.SlotIndex, first, out RuntimeEntityHandle oldHandle), Is.True,
                "W05B-A06: the first spawn must expose its current runtime handle");
            Assert.That(oldHandle.IsValid, Is.True,
                "W05B-A07: the first spawn handle must be valid");
            LF2ObjectRenderer reusedRenderer = first.Renderer;
            AssertMountsBound(reusedRenderer, oldHandle, "W05B-A08");
            Assert.That(world.BattlePresentation.PublishedFrame, Is.SameAs(tick40PublishedFrame),
                "W05B-A09: late update must not replace the published tick-40 frame");
            Assert.That(ContainsCommand(tick40RenderedFrame, oldHandle), Is.False,
                "W05B-A10: late opoint must not mutate the frame already published by RenderDispatch(T)");

            world.RenderDispatchAll(41);
            BattlePresentationFrame tick41RenderedFrame = MaterializeCentralFrame(world);
            Assert.That(ContainsCommand(tick41RenderedFrame, oldHandle), Is.True,
                "W05B-A11: the first central entity command must appear at RenderDispatch(T+1)");

            int activeBeforeRelease = pool.ActiveObjectCountForAcceptance;
            first.FreeEntityLikeExe();
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.EqualTo(activeBeforeRelease - 1),
                "W05B-A12: releasing the first entity must return exactly one pooled renderer");
            Assert.That(world.TryResolveRuntimeHandleForDiagnostics(oldHandle, out _), Is.False,
                "W05B-A13: the released generation must immediately stop resolving");
            AssertMountsCleared(reusedRenderer, "W05B-A14");

            reuseProducer.AttackingCounter = 0;
            world.LateEntityUpdateAll(41);
            LF2Entity replacement = world.FindEntityByRuntimeSlotForQuery(oldHandle.Slot);
            Assert.That(replacement, Is.Not.Null,
                "W05B-A15: the reuse producer must fill the released runtime slot");
            Assert.That(replacement.ObjectId, Is.EqualTo(SpawnOid),
                "W05B-A16: the replacement must be the requested opoint oid");
            Assert.That(replacement.Renderer, Is.SameAs(reusedRenderer),
                "W05B-A17: the released production renderer must be checked out again from the isolated pool");
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                oldHandle.Slot, replacement, out RuntimeEntityHandle replacementHandle), Is.True,
                "W05B-A18: the replacement must expose its current runtime handle");
            Assert.That(replacementHandle.IsValid, Is.True,
                "W05B-A19: the replacement handle must be valid");
            Assert.That(replacementHandle.Generation, Is.Not.EqualTo(oldHandle.Generation),
                "W05B-A20: same-slot pool reuse must advance the runtime generation");
            AssertMountsBound(reusedRenderer, replacementHandle, "W05B-A21");
            Assert.That(world.TryResolveRuntimeHandleForDiagnostics(oldHandle, out _), Is.False,
                "W05B-A22: acquiring a replacement must not resurrect the old generation");

            BattlePresentationFrame stillTick41 = tick41RenderedFrame;
            Assert.That(ContainsCommand(stillTick41, oldHandle), Is.True,
                "W05B-A23: published frames are immutable snapshots until the next dispatch");
            Assert.That(ContainsCommand(stillTick41, replacementHandle), Is.False,
                "W05B-A24: the replacement must remain absent before RenderDispatch(T+2)");

            world.RenderDispatchAll(42);
            BattlePresentationFrame tick42Frame = MaterializeCentralFrame(world);
            Assert.That(ContainsCommand(tick42Frame, oldHandle), Is.False,
                "W05B-A25: same-slot reuse must not emit a ghost command for the old generation");
            Assert.That(ContainsCommand(tick42Frame, replacementHandle), Is.True,
                "W05B-A26: RenderDispatch(T+2) must publish the replacement generation");

            replacement.FreeEntityLikeExe();
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero,
                "W05B-A27: W05B must return every renderer checked out from its isolated pool");
        }

        [Test]
        public void W05A_LowSlotReuse_IsDeferredUntilTheNextRealLatePass()
        {
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
            using var isolatedPool = new IsolatedObjectPoolScope(pool);
            using var driver = new SimulationDriverWorldScope();

            var world = new SimulationWorld();
            driver.SetWorld(world);
            var releasedLow = new DynamicSlotOccupant(201);
            var beforeProducer = new DynamicSlotOccupant(202);
            var producer = new OpointSpawner(SpawnOid, "low");
            world.Register(releasedLow);
            world.Register(beforeProducer);
            world.Register(producer);
            int lowSlot = releasedLow.Runtime.SlotIndex;
            Assert.That(lowSlot, Is.LessThan(producer.Runtime.SlotIndex));
            world.Unregister(releasedLow);

            world.LateEntityUpdateAll(70);
            LF2Entity spawned = world.FindEntityByRuntimeSlotForQuery(lowSlot);
            Assert.That(spawned, Is.Not.Null);
            producer.AttackingCounter = 1;
            Assert.That(spawned.ObjectId, Is.EqualTo(SpawnOid));
            Assert.That(spawned.AttackingCounter, Is.Zero,
                "a newborn behind the live scan cursor must not execute in its creation pass");

            world.LateEntityUpdateAll(71);
            Assert.That(spawned.AttackingCounter, Is.EqualTo(1));
            spawned.FreeEntityLikeExe();
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
            factory.FlushTasks();
        }

        [Test]
        public void W05C_SealedProductionOpointSpawnAndRelease_DoesNotAllocate()
        {
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            bool restoreObjectPoolSeal = pool.IsBattleCapacitySealed;
            bool restoreReferencePoolSeal = referencePool.IsBattleCapacitySealed;

            pool.UnsealBattleCapacity();
            referencePool.UnsealBattleCapacity();
            referencePool.PrewarmTasks<OPointCreateTask>(8);
            referencePool.PrepareObjectCapacity(LF2ObjectType.Other, 8);

            try
            {
                using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
                using var isolatedPool = new IsolatedObjectPoolScope(pool);
                using var driver = new SimulationDriverWorldScope();

                var world = new SimulationWorld();
                driver.SetWorld(world);
                var spawner = new OpointSpawner(SpawnOid, "zero-gc");
                world.Register(spawner);

                factory.ProcessOpointSpawn(spawner);
                LF2Entity warmup = FindSpawn(world);
                Assert.That(warmup, Is.Not.Null);
                warmup.FreeEntityLikeExe();
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);

                pool.SealBattleCapacity();
                referencePool.SealBattleCapacity();

                bool allSpawnsSucceeded = true;
                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                {
                    factory.ProcessOpointSpawn(spawner);
                    LF2Entity spawned = FindSpawn(world);
                    if (spawned == null)
                    {
                        allSpawnsSucceeded = false;
                        continue;
                    }

                    spawned.FreeEntityLikeExe();
                }
                long allocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(allocatedBytes, Is.Zero);
                Assert.That(allSpawnsSucceeded, Is.True);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(1));
            }
            finally
            {
                if (!restoreObjectPoolSeal)
                    pool.UnsealBattleCapacity();
                else
                    pool.SealBattleCapacity();

                if (!restoreReferencePoolSeal)
                    referencePool.UnsealBattleCapacity();
                else
                    referencePool.SealBattleCapacity();
            }
        }

        [Test]
        public void W05D_SealedProductionSixObjectOpointSpawnAndRelease_DoesNotAllocate()
        {
            const int spawnCount = 6;
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            bool restoreObjectPoolSeal = pool.IsBattleCapacitySealed;
            bool restoreReferencePoolSeal = referencePool.IsBattleCapacitySealed;

            pool.UnsealBattleCapacity();
            referencePool.UnsealBattleCapacity();
            referencePool.PrewarmTasks<OPointCreateTask>(spawnCount + 2);
            referencePool.PrepareObjectCapacity(LF2ObjectType.Other, spawnCount + 2);

            try
            {
                using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
                using var isolatedPool = new IsolatedObjectPoolScope(pool);
                using var driver = new SimulationDriverWorldScope();

                var world = new SimulationWorld();
                driver.SetWorld(world);
                var spawner = new OpointSpawner(SpawnOid, "six-zero-gc", spawnCount * 10);
                var spawnedEntities = new LF2Entity[spawnCount];
                world.Register(spawner);

                factory.ProcessOpointSpawn(spawner);
                Assert.That(CollectSpawns(world, spawnedEntities), Is.EqualTo(spawnCount));
                ReleaseSpawns(spawnedEntities);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);

                pool.SealBattleCapacity();
                referencePool.SealBattleCapacity();

                bool allSpawnsSucceeded = true;
                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                {
                    factory.ProcessOpointSpawn(spawner);
                    if (CollectSpawns(world, spawnedEntities) != spawnCount)
                        allSpawnsSucceeded = false;
                    ReleaseSpawns(spawnedEntities);
                }
                long allocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(allocatedBytes, Is.Zero);
                Assert.That(allSpawnsSucceeded, Is.True);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(1));
            }
            finally
            {
                if (!restoreObjectPoolSeal)
                    pool.UnsealBattleCapacity();
                else
                    pool.SealBattleCapacity();

                if (!restoreReferencePoolSeal)
                    referencePool.UnsealBattleCapacity();
                else
                    referencePool.SealBattleCapacity();
            }
        }

        [Test]
        public void W05E_SealedProductionDeathCleanupAndPoolReturn_DoesNotAllocate()
        {
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            bool restoreObjectPoolSeal = pool.IsBattleCapacitySealed;
            bool restoreReferencePoolSeal = referencePool.IsBattleCapacitySealed;

            pool.UnsealBattleCapacity();
            referencePool.UnsealBattleCapacity();
            referencePool.PrewarmTasks<OPointCreateTask>(8);
            referencePool.PrepareObjectCapacity(LF2ObjectType.Other, 8);

            try
            {
                using var configs = new RuntimeObjectConfigScope(
                    SpawnOid,
                    BuildDeathCleanupSpawnData());
                using var isolatedPool = new IsolatedObjectPoolScope(pool);
                using var driver = new SimulationDriverWorldScope();

                var world = new SimulationWorld();
                driver.SetWorld(world);
                var spawner = new OpointSpawner(SpawnOid, "death-cleanup-zero-gc");
                world.Register(spawner);

                factory.ProcessOpointSpawn(spawner);
                LF2Entity warmup = FindSpawn(world);
                Assert.That(warmup, Is.Not.Null);
                PrepareDeathCleanup(warmup);
                world.PostFrameAdvanceDeathCleanupAll(0);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);

                pool.SealBattleCapacity();
                referencePool.SealBattleCapacity();

                bool allCleanupCyclesSucceeded = true;
                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                {
                    factory.ProcessOpointSpawn(spawner);
                    LF2Entity spawned = FindSpawn(world);
                    if (spawned == null)
                    {
                        allCleanupCyclesSucceeded = false;
                        continue;
                    }

                    PrepareDeathCleanup(spawned);
                    world.PostFrameAdvanceDeathCleanupAll(iteration + 1);
                    if (FindSpawn(world) != null || pool.ActiveObjectCountForAcceptance != 0)
                        allCleanupCyclesSucceeded = false;
                }
                long allocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(allocatedBytes, Is.Zero);
                Assert.That(allCleanupCyclesSucceeded, Is.True);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(1));
            }
            finally
            {
                if (!restoreObjectPoolSeal)
                    pool.UnsealBattleCapacity();
                else
                    pool.SealBattleCapacity();

                if (!restoreReferencePoolSeal)
                    referencePool.UnsealBattleCapacity();
                else
                    referencePool.SealBattleCapacity();
            }
        }

        [Test]
        public void W05F_WorldOwnedStructuralWriter_PreservesLateBoundaryAndGenerationLifecycle()
        {
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
            using var isolatedPool = new IsolatedObjectPoolScope(pool);
            using var driver = new SimulationDriverWorldScope();

            var world = new SimulationWorld();
            driver.SetWorld(world);
            var spawner = new OpointSpawner(SpawnOid, "structural-writer");
            world.Register(spawner);
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                spawner.Runtime.SlotIndex,
                spawner,
                out RuntimeEntityHandle spawnerHandle), Is.True);

            BattleStructuralWriterDiagnostics before =
                world.StructuralWriterDiagnosticsForDiagnostics;
            factory.ProcessOpointSpawn(spawner);
            LF2Entity spawned = FindSpawn(world);
            Assert.That(spawned, Is.Not.Null);
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                spawned.Runtime.SlotIndex,
                spawned,
                out RuntimeEntityHandle spawnedHandle), Is.True);

            BattleStructuralWriterDiagnostics afterSpawn =
                world.StructuralWriterDiagnosticsForDiagnostics;
            Assert.That(afterSpawn.SpawnCount, Is.EqualTo(before.SpawnCount + 1));
            Assert.That(afterSpawn.RegisterCount, Is.GreaterThan(before.RegisterCount));
            Assert.That(afterSpawn.GenerationClaimCount,
                Is.EqualTo(before.GenerationClaimCount + 1));
            Assert.That(afterSpawn.LastSpawnBoundary,
                Is.EqualTo(BattleStructuralPlaybackBoundary.CurrentEntityImmediate));
            Assert.That(afterSpawn.LastSpawnSource, Is.EqualTo(spawnerHandle));
            Assert.That(afterSpawn.LastSpawnAuthorityOrdinal, Is.GreaterThan(0));

            spawned.FreeEntityLikeExe();
            BattleStructuralWriterDiagnostics afterFree =
                world.StructuralWriterDiagnosticsForDiagnostics;
            Assert.That(afterFree.FreeCount, Is.EqualTo(afterSpawn.FreeCount + 1));
            Assert.That(afterFree.UnregisterCount,
                Is.GreaterThan(afterSpawn.UnregisterCount));
            Assert.That(afterFree.GenerationReleaseCount,
                Is.EqualTo(afterSpawn.GenerationReleaseCount + 1));
            Assert.That(world.TryResolveRuntimeHandleForDiagnostics(spawnedHandle, out _),
                Is.False);
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
        }

        [Test]
        public void W05G_TransitDestroy_UsesStructuralWriterAndInvalidatesGeneration()
        {
            LF2ObjectPointFactory factory = RequireFactoryAndPools(out LF2ObjectPool pool);
            using var configs = new RuntimeObjectConfigScope(SpawnOid, BuildSpawnData());
            using var isolatedPool = new IsolatedObjectPoolScope(pool);
            using var driver = new SimulationDriverWorldScope();

            var world = new SimulationWorld();
            driver.SetWorld(world);
            var spawner = new OpointSpawner(SpawnOid, "structural-destroy");
            world.Register(spawner);
            factory.ProcessOpointSpawn(spawner);
            LF2Entity spawned = FindSpawn(world);
            Assert.That(spawned, Is.Not.Null);
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                spawned.Runtime.SlotIndex,
                spawned,
                out RuntimeEntityHandle spawnedHandle), Is.True);

            BattleStructuralWriterDiagnostics before =
                world.StructuralWriterDiagnosticsForDiagnostics;
            spawned.OnTransitDestroy();
            BattleStructuralWriterDiagnostics after =
                world.StructuralWriterDiagnosticsForDiagnostics;

            Assert.That(after.DestroyCount, Is.EqualTo(before.DestroyCount + 1));
            Assert.That(after.UnregisterCount, Is.GreaterThan(before.UnregisterCount));
            Assert.That(after.GenerationReleaseCount,
                Is.EqualTo(before.GenerationReleaseCount + 1));
            Assert.That(world.TryResolveRuntimeHandleForDiagnostics(spawnedHandle, out _),
                Is.False);
            Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
        }

        private static LF2ObjectPointFactory RequireFactoryAndPools(out LF2ObjectPool pool)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            pool = LF2ObjectPool.Instance;
            Assert.That(factory, Is.Not.Null);
            Assert.That(pool, Is.Not.Null);
            Assert.That(LF2ReferencePool.Instance, Is.Not.Null);
            return factory;
        }

        private static LF2Entity FindSpawn(SimulationWorld world)
        {
            for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity?.ObjectId == SpawnOid)
                    return entity;
            }
            return null;
        }

        private static int CollectSpawns(SimulationWorld world, LF2Entity[] destination)
        {
            int count = 0;
            for (int index = 0; index < destination.Length; index++)
                destination[index] = null;

            for (int slot = 50;
                 slot < world.RuntimeSlotCapacityForDiagnostics && count < destination.Length;
                 slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity?.ObjectId == SpawnOid)
                    destination[count++] = entity;
            }

            return count;
        }

        private static void ReleaseSpawns(LF2Entity[] spawnedEntities)
        {
            for (int index = 0; index < spawnedEntities.Length; index++)
            {
                LF2Entity entity = spawnedEntities[index];
                spawnedEntities[index] = null;
                entity?.FreeEntityLikeExe();
            }
        }

        private static void PrepareDeathCleanup(LF2Entity entity)
        {
            entity.RelationTeam = 5;
            entity.Health.HP = 0;
            entity.HP2Orig = 1;
            entity.HitStun = 2;
        }

        private static bool ContainsCommand(BattlePresentationFrame frame, RuntimeEntityHandle handle)
        {
            if (frame == null)
                return false;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type == BattleRenderCommandType.Entity && command.Handle == handle)
                    return true;
            }
            return false;
        }

        private static BattlePresentationFrame MaterializeCentralFrame(SimulationWorld world)
        {
            BattlePixelFramePlan plan =
                BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
            Assert.That(plan.IsValid, Is.True,
                "W05B: the central presentation host must publish a valid pixel plan");
            Assert.That(plan.UsesCentralPixels, Is.True,
                "W05B: the materialized pixel plan must retain central ownership");
            Assert.That(plan.CapturedFrame, Is.Not.Null,
                "W05B: central ownership requires an immutable materialized frame");
            return plan.CapturedFrame;
        }

        private static void AssertMountsBound(
            LF2ObjectRenderer renderer,
            RuntimeEntityHandle expectedHandle,
            string assertionId)
        {
            BattleCentralPresentationMount[] mounts = renderer.transform.parent
                .GetComponentsInChildren<BattleCentralPresentationMount>(true);
            Assert.That(mounts.Count(value => value.OwnerRenderer == renderer), Is.EqualTo(2),
                $"{assertionId}-mount-count: isolated fallback renderer must expose entity and shadow mounts");
            Assert.That(mounts.All(value =>
                    value.OwnerRenderer != renderer || value.RuntimeHandle == expectedHandle), Is.True,
                $"{assertionId}-generation: every owner mount must bind the current runtime generation");
        }

        private static void AssertMountsCleared(LF2ObjectRenderer renderer, string assertionId)
        {
            BattleCentralPresentationMount[] mounts = renderer.transform.parent
                .GetComponentsInChildren<BattleCentralPresentationMount>(true);
            Assert.That(mounts.All(value => !value.RuntimeHandle.IsValid), Is.True,
                $"{assertionId}: every pooled mount must clear its released runtime generation");
        }

        private static LF2CharacterData BuildSpawnData()
        {
            return new LF2CharacterData
            {
                name = "W05ProductionOpoint",
                type_sub = SpawnOid,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = 9999,
                        wait = 100,
                        next = 0,
                        pic = 0,
                    },
                },
            };
        }

        private static LF2CharacterData BuildDeathCleanupSpawnData()
        {
            return new LF2CharacterData
            {
                name = "W05ProductionDeathCleanup",
                type_sub = SpawnOid,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Lying,
                        wait = 100,
                        next = 0,
                        pic = 0,
                    },
                },
            };
        }

        private class DynamicSlotOccupant : LF2OtherObject
        {
            public DynamicSlotOccupant(int stableId)
            {
                StableId = stableId;
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }
        }

        private sealed class OpointSpawner : DynamicSlotOccupant
        {
            public OpointSpawner(int spawnOid, string label, int facing = 0)
                : base(label.GetHashCode())
            {
                Name = $"W05OpointSpawner_{label}";
                ObjectId = 739;
                LF2FrameData frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 100,
                    next = 0,
                    pic = 0,
                    centerx = 0,
                    centery = 0,
                    opoint = new ObjectPoint
                    {
                        kind = 1,
                        oid = spawnOid,
                        action = 0,
                        facing = facing,
                    },
                };
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, new LF2CharacterData
                {
                    name = Name,
                    frames = new List<LF2FrameData> { frame },
                }));
                Frame.D = frame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                PS.dir = "right";
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
            }

            public override void SimFrameTick(int tickIndex)
            {
            }
        }

        private sealed class RuntimeObjectConfigScope : IDisposable
        {
            private readonly GameDataManager dataManager;
            private readonly CharacterAnimtorManager animatorManager;
            private readonly FieldInfo objectLookupField;
            private readonly FieldInfo cachedConfigField;
            private readonly FieldInfo frameConfigField;
            private readonly object originalObjectLookup;
            private readonly object originalCachedConfig;
            private readonly object originalFrameConfigs;

            public RuntimeObjectConfigScope(int oid, LF2CharacterData data)
            {
                dataManager = GameDataManager.Instance;
                animatorManager = CharacterAnimtorManager.Instance;
                Assert.That(dataManager, Is.Not.Null);
                Assert.That(animatorManager, Is.Not.Null);
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                objectLookupField = typeof(GameDataManager).GetField("objectLookup", flags);
                cachedConfigField = typeof(GameDataManager).GetField("cachedConfig", flags);
                frameConfigField = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", flags);
                Assert.That(objectLookupField, Is.Not.Null);
                Assert.That(cachedConfigField, Is.Not.Null);
                Assert.That(frameConfigField, Is.Not.Null);
                originalObjectLookup = objectLookupField.GetValue(dataManager);
                originalCachedConfig = cachedConfigField.GetValue(dataManager);
                originalFrameConfigs = frameConfigField.GetValue(animatorManager);
                var config = new GameDataConfig();
                var definition = new ObjectDefinition(oid, (int)LF2ObjectType.Other, "w05-opoint.dat");
                config.objects.Add(definition);
                objectLookupField.SetValue(dataManager,
                    new Dictionary<int, ObjectDefinition> { [oid] = definition });
                cachedConfigField.SetValue(dataManager, config);
                frameConfigField.SetValue(animatorManager,
                    new Dictionary<int, LF2CharacterDataWrapper>
                    {
                        [oid] = new LF2CharacterDataWrapper(oid, data),
                    });
            }

            public void Dispose()
            {
                objectLookupField.SetValue(dataManager, originalObjectLookup);
                cachedConfigField.SetValue(dataManager, originalCachedConfig);
                frameConfigField.SetValue(animatorManager, originalFrameConfigs);
            }
        }

        private sealed class CharacterSpriteScope : IDisposable
        {
            private readonly CharacterAnimtorManager manager;
            private readonly Dictionary<int, List<Sprite>> sprites;
            private readonly int oid;
            private readonly bool hadOriginal;
            private readonly List<Sprite> original;
            private readonly Sprite sprite;
            private readonly FieldInfo catalogField;
            private readonly BattleSpriteCatalog originalCatalog;

            public CharacterSpriteScope(int oid)
            {
                this.oid = oid;
                manager = CharacterAnimtorManager.Instance;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                FieldInfo spritesField = typeof(CharacterAnimtorManager).GetField("MergedSprites", flags);
                catalogField = typeof(CharacterAnimtorManager).GetField(
                    "<SpriteCatalog>k__BackingField", flags);
                Assert.That(spritesField, Is.Not.Null);
                Assert.That(catalogField, Is.Not.Null);
                sprites = spritesField.GetValue(manager) as Dictionary<int, List<Sprite>>;
                Assert.That(sprites, Is.Not.Null);
                hadOriginal = sprites.TryGetValue(oid, out original);
                sprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0, 0, 1, 1),
                    new Vector2(0.5f, 0),
                    100f);
                sprites[oid] = new List<Sprite> { sprite };
                originalCatalog = catalogField.GetValue(manager) as BattleSpriteCatalog ??
                                  BattleSpriteCatalog.Empty;
                var builder = new BattleSpriteCatalogBuilder();
                builder.Add(
                    oid,
                    0,
                    "w05-opoint.bmp",
                    Texture2D.whiteTexture,
                    new Rect(0, 0, 1, 1),
                    sprite);
                catalogField.SetValue(manager, builder.Publish());
            }

            public void Dispose()
            {
                catalogField.SetValue(manager, originalCatalog);
                if (hadOriginal)
                    sprites[oid] = original;
                else
                    sprites.Remove(oid);
                UnityEngine.Object.DestroyImmediate(sprite);
            }
        }

        private sealed class IsolatedObjectPoolScope : IDisposable
        {
            private readonly LF2ObjectPool pool;
            private readonly FieldInfo availableField;
            private readonly FieldInfo activeField;
            private readonly FieldInfo releaseMapField;
            private readonly FieldInfo spritePoolField;
            private readonly FieldInfo cachedPrefabField;
            private readonly object originalAvailable;
            private readonly object originalActive;
            private readonly object originalReleaseMap;
            private readonly object originalSpritePool;
            private readonly object originalCachedPrefab;
            private readonly GameConfig configuredGameConfig;
            private readonly GameObject originalConfiguredObjectPrefab;

            public IsolatedObjectPoolScope(LF2ObjectPool pool)
            {
                this.pool = pool;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                Type type = typeof(LF2ObjectPool);
                availableField = type.GetField("_availableObjects", flags);
                activeField = type.GetField("_activeObjects", flags);
                releaseMapField = type.GetField("_releaseTimeMap", flags);
                spritePoolField = type.GetField("_spritePool", flags);
                cachedPrefabField = type.GetField("_cachedLF2ObjectPrefab", flags);
                Assert.That(availableField, Is.Not.Null);
                Assert.That(activeField, Is.Not.Null);
                Assert.That(releaseMapField, Is.Not.Null);
                Assert.That(spritePoolField, Is.Not.Null);
                Assert.That(cachedPrefabField, Is.Not.Null);
                originalAvailable = availableField.GetValue(pool);
                originalActive = activeField.GetValue(pool);
                originalReleaseMap = releaseMapField.GetValue(pool);
                originalSpritePool = spritePoolField.GetValue(pool);
                originalCachedPrefab = cachedPrefabField.GetValue(pool);
                configuredGameConfig = GameConfig.Instance;
                originalConfiguredObjectPrefab = configuredGameConfig != null
                    ? configuredGameConfig.LF2ObjectPrefab
                    : null;
                availableField.SetValue(pool, new Queue<GameObject>(8));
                activeField.SetValue(pool, new HashSet<GameObject>(8));
                releaseMapField.SetValue(pool, new Dictionary<GameObject, float>(8));
                spritePoolField.SetValue(pool, new Stack<SpriteRenderer>());
                if (configuredGameConfig != null)
                    configuredGameConfig.LF2ObjectPrefab = null;
                cachedPrefabField.SetValue(pool, null);
            }

            public void Dispose()
            {
                var objects = new HashSet<GameObject>();
                Collect(availableField.GetValue(pool), objects);
                Collect(activeField.GetValue(pool), objects);
                availableField.SetValue(pool, originalAvailable);
                activeField.SetValue(pool, originalActive);
                releaseMapField.SetValue(pool, originalReleaseMap);
                spritePoolField.SetValue(pool, originalSpritePool);
                cachedPrefabField.SetValue(pool, originalCachedPrefab);
                if (configuredGameConfig != null)
                    configuredGameConfig.LF2ObjectPrefab = originalConfiguredObjectPrefab;
                foreach (GameObject item in objects)
                    UnityEngine.Object.DestroyImmediate(item);
            }

            private static void Collect(object source, HashSet<GameObject> objects)
            {
                if (source is Queue<GameObject> available)
                {
                    foreach (GameObject item in available)
                        if (item != null) objects.Add(item);
                }
                else if (source is HashSet<GameObject> active)
                {
                    foreach (GameObject item in active)
                        if (item != null) objects.Add(item);
                }
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
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType?.GetField(
                    "<Instance>k__BackingField", flags);
                Assert.That(instanceField, Is.Not.Null);
                originalInstance = instanceField.GetValue(null) as SimulationTickDriver;
                driver = SimulationTickDriver.Instance;
                if (driver == null)
                {
                    temporaryDriverObject = new GameObject("W05_SimulationTickDriver");
                    driver = temporaryDriverObject.AddComponent<SimulationTickDriver>();
                    instanceField.SetValue(null, driver);
                }
                worldField = typeof(SimulationTickDriver).GetField(
                    "_world", BindingFlags.Instance | BindingFlags.NonPublic);
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
    }
}
#endif
