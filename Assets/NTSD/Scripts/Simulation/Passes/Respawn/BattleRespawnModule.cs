using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the post-frame death gate, respawn mutation and immediate respawn effect.
    /// </summary>
    internal sealed class BattleRespawnModule
    {
        private readonly SimulationWorld world;
        private readonly List<LF2Entity> entityScratch =
            new List<LF2Entity>(64);

        internal BattleRespawnModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal void RunPostFrameAdvanceDeathCleanup(int tickIndex)
        {
            world.GetActiveEntitiesByRuntimeSlotForModule(entityScratch);
            for (int index = 0; index < entityScratch.Count; index++)
            {
                LF2Entity entity = entityScratch[index];
                if (!PassesRespawnGate(entity))
                    continue;

                // Alignment contract:
                // NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001.
                if (entity.HP2Orig < 2)
                {
                    if (entity.RespawnCount > 0)
                    {
                        ApplyRespawnFromStoredCount(entity);
                    }
                    else if ((entity.Runtime?.SlotIndex ?? -1) > 19)
                    {
                        entity.FreeEntityLikeExe();
                    }
                }
                else
                {
                    ApplyRespawnWithoutStoredCount(entity);
                }

                if (world.IsActiveForCurrentPassInternal(entity))
                    world.RefreshRuntimeSnapshotForModule(entity);
            }

            entityScratch.Clear();
        }

        private bool PassesRespawnGate(LF2Entity entity)
        {
            if (entity?.Health == null ||
                !world.IsActiveForCurrentPassInternal(entity))
            {
                return false;
            }

            LF2FrameData frame = entity.Frame?.D;
            if (frame == null ||
                frame.state != LF2States.Lying ||
                entity.Health.HP > 0)
            {
                return false;
            }

            int hitStop = entity.HitStun;
            return hitStop > 0 && hitStop < 5;
        }

        private void ApplyRespawnWithoutStoredCount(LF2Entity entity)
        {
            int hp2 = entity.HP2Orig;
            entity.HP2Orig = hp2 - 1;

            int relationTeam = entity.RelationTeam;
            int sumX = 0;
            int sumZ = 0;
            int count = 0;

            for (int index = 0; index < entityScratch.Count; index++)
            {
                LF2Entity other = entityScratch[index];
                if (other == null ||
                    other == entity ||
                    other.Health == null)
                {
                    continue;
                }
                if (other.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
                {
                    continue;
                }
                if (other.RelationTeam != relationTeam)
                    continue;

                sumX += other.Runtime.XInt;
                sumZ += other.Runtime.ZInt;
                count++;
            }

            if (sumX != 0 && count > 0)
            {
                int avgX = sumX / count;
                int avgZ = sumZ / count;
                entity.Runtime.X = avgX +
                    world.NativeRandom.SynchronizedNext(0x90u, 0x33) - 25.0;
                entity.Runtime.Z = avgZ +
                    world.NativeRandom.SynchronizedNext(0x91u, 0x1f) - 15.0;
                entity.PS.x = entity.Runtime.X;
                entity.PS.z = entity.Runtime.Z;
            }

            entity.Health.PP = 500;
            entity.Health.HPBound = entity.Health.HP3;
            entity.Health.HP = entity.Health.HPBound;
            entity.HitStun = 20;
            entity.DirectWriteFramePreserveWaitCounter(212);
            int floorY = entity.Runtime.CollisionYReference < 0
                ? entity.Runtime.CollisionYReference
                : 0;
            entity.PS.y = floorY;
            entity.PS.vy = 0.0;
            entity.Runtime.Y = floorY;
            entity.Runtime.YInt = floorY;
            entity.Runtime.Vy = 0.0;
        }

        private void ApplyRespawnFromStoredCount(LF2Entity entity)
        {
            if (!TryResolveQueuedControllerGroup(entity, out int battleGroup))
                return;

            entity.HP2Orig = entity.HPOrig;
            entity.Health.PP = 0;
            entity.Health.HPBound = entity.RespawnCount;
            entity.Health.HP3 = entity.Health.HPBound;
            entity.Health.HP = entity.Health.HP3;
            entity.RespawnCount = 0;
            entity.HPOrig = 0;
            entity.RelationTeam = battleGroup;

            int visual = entity.Runtime.ReviveVisualId184;
            if (visual < 1)
            {
                if ((entity.ObjectId >= 30 && entity.ObjectId <= 36) ||
                    entity.ObjectId == 39)
                {
                    visual = 140;
                }
                else if (entity.ObjectId == 37)
                {
                    visual = 114;
                }
            }
            if (visual > 0)
            {
                entity.Runtime.RenderPicOffset = visual;
                entity.Runtime.ReviveVisualRuntime180 = visual;
            }

            entity.DirectWriteFramePreserveWaitCounter(0xDB);
            entity.AttackingCounter = 0;
            entity.FrameDelay = 0xA;
            TrySpawnRespawnEffect(entity);
        }

        private bool TryResolveQueuedControllerGroup(
            LF2Entity entity,
            out int battleGroup)
        {
            battleGroup = 0;
            int nativeControllerSlot = entity?.Runtime?.Unk360 ?? -1;
            bool inactiveSlotOneFallback = nativeControllerSlot == -1;
            int controllerSlot = inactiveSlotOneFallback
                ? 1
                : nativeControllerSlot;
            LF2Entity controller =
                world.FindEntityByRuntimeSlotForQuery(controllerSlot);
            if (controller == null)
                return inactiveSlotOneFallback;

            battleGroup = controller.RelationTeam;
            return true;
        }

        private LF2Entity TrySpawnRespawnEffect(LF2Entity entity)
        {
            if (entity == null)
                return null;

            LF2Entity overrideSpawned =
                world.InvokeRespawnEffectSpawnOverrideForModule(entity);
            if (overrideSpawned != null)
                return overrideSpawned;

            ILF2ObjectPointFactory factory =
                world.ResolveObjectPointFactoryForSimulation();
            if (factory == null)
                return null;

            BattleLogicReferencePool referencePool = world.LogicReferencePool;
            OPointCreateTask task = referencePool?.Fetch<OPointCreateTask>();
            if (task == null)
                return null;

            task.opoint = new ObjectPoint
            {
                oid = 998,
                kind = 0,
                action = 6,
                facing = 0,
            };
            task.parent = null;
            task.team = 0;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = entity.RelationTeam;
            task.spawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            task.pos = new Vector3(
                entity.GetRuntimeXInt(),
                entity.GetRuntimeYInt(),
                entity.GetRenderZInt());
            task.z = entity.GetRenderZInt();
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = entity.GetRuntimeXInt();
            task.initialRuntimeY = entity.GetRuntimeYInt();
            task.initialRuntimeZ = entity.GetRenderZInt() + 1;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;
            task.targetWorld = world;

            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(task);
            }
            finally
            {
                referencePool.Recycle(task);
            }
            if (spawned == null)
                return null;

            spawned.RelationTeam = entity.RelationTeam;
            spawned.SpawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            spawned.RefreshRuntimeSnapshot();
            return spawned;
        }
    }
}
