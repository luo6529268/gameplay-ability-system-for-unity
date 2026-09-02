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

                if (entity.RespawnCount <= 0)
                    ApplyRespawnWithoutStoredCount(entity);
                else
                    ApplyRespawnFromStoredCount(entity);

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

            int slotIndex = entity.Runtime?.SlotIndex ?? -1;
            if (slotIndex < 20 &&
                entity.KillCount < 0 &&
                entity.RelationTeam != 5)
            {
                return false;
            }

            int hitStop = entity.HitStun;
            return hitStop > 0 && hitStop < 5;
        }

        private void ApplyRespawnWithoutStoredCount(LF2Entity entity)
        {
            int hp2 = entity.HP2Orig;
            if (hp2 < 2)
            {
                entity.FreeEntityLikeExe();
                return;
            }

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

            if (count > 0)
            {
                int avgX = sumX / count;
                int avgZ = sumZ / count;
                entity.Runtime.X = avgX + entity.BattleRandInt(0, 51) - 26.0;
                entity.Runtime.XInt = (int)entity.Runtime.X;
                entity.Runtime.Z = avgZ + entity.BattleRandInt(0, 31) - 16.0;
                entity.Runtime.ZInt = (int)entity.Runtime.Z;
                entity.PS.x = entity.Runtime.X;
                entity.PS.z = entity.Runtime.Z;
            }

            entity.Health.PP = 500;
            entity.Health.PPBound = entity.Health.MaxPP;
            entity.Health.HPBound = entity.Health.HP3;
            entity.Health.HP = entity.Health.HPBound;
            entity.HitStun = 20;
            entity.DirectWriteFramePreserveWaitCounter(212);
            entity.PS.y = -300.0;
            entity.PS.vy = 0.0;
            entity.Runtime.Y = -300.0;
            entity.Runtime.Vy = 0.0;
            entity.Runtime.SyncIntegerPosition();
        }

        private void ApplyRespawnFromStoredCount(LF2Entity entity)
        {
            entity.HP2Orig = entity.HPOrig;
            entity.Health.PP = 0;
            entity.Health.HPBound = entity.RespawnCount;
            entity.Health.HP3 = entity.Health.HPBound;
            entity.Health.HP = entity.Health.HP3;
            entity.RespawnCount = 0;
            entity.HPOrig = 0;
            entity.RelationTeam = 1;

            if (entity.ObjectId >= 0x1E && entity.ObjectId <= 0x24)
                entity.Runtime.RenderPicOffset = 0x8C;

            entity.DirectWriteFramePreserveWaitCounter(0xDB);
            entity.AttackingCounter = 0;
            entity.FrameDelay = 0xA;
            TrySpawnRespawnEffect(entity);
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
            task.holderCopySlot = -1;
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
