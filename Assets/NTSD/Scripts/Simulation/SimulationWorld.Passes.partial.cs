using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的正式版战斗 pass 执行入口。
    /// </summary>
    public partial class SimulationWorld
    {
        private void RunDeferredMutationEntityPass(System.Action<LF2Entity> action)
        {
            if (action == null)
                return;

            _ticking = true;
            try
            {
                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (entity == null || !IsActiveForCurrentPass(entity))
                        return;

                    action(entity);
                });
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        public void PostCooldownInputAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                entity.RunPostCooldownInputPhase(tickIndex);
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void SerialTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                ForEachEntityByRuntimeSlot(entity =>
                {
                    entity.SimTransit(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });

                ForEachEntityByRuntimeSlot(entity =>
                {
                    entity.SimTU(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });

                CleanupState9998Entities();
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private void CleanupState9998Entities()
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null || frame.state != 9998) continue;
                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        public void PostFrameAdvanceDeathCleanupAll(int tickIndex)
        {
        }

        public void EarlyFrameAdvanceSpecialsAll(int tickIndex)
        {
            bool teleportGate = (tickIndex & 1) == 0;

            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                entity.RunEarlyTeleportSpecialsPhase(_entityScratch, teleportGate);
                if (!IsActiveForCurrentPass(entity))
                    continue;
                RefreshRuntimeSnapshot(entity);
            }

            RunEarlyState500Specials(_entityScratch);
            RunEarlyState501Specials(_entityScratch);
            _entityScratch.Clear();
        }

        private void RunEarlyState500Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null)
                    continue;

                if (frame.state != 500)
                    continue;

                if (entity.TransformTargetObjectId == -1 || entity.TransformOriginalObjectId >= 0)
                {
                    // BMD-023: state=500 reset branch must mirror baseline SetFrameImmediate:
                    // write Frame + FrameWaitCounter only, never Attacking. Unity's
                    // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                    entity.DirectWriteFramePreserveWaitCounter(0);
                    RefreshRuntimeSnapshot(entity);
                }
            }
        }

        private void RunEarlyState501Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null)
                    continue;

                if (frame.state != 501 || entity.TransformTargetObjectId <= -1)
                    continue;

                var wrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(entity.TransformTargetObjectId);
                if (wrapper == null)
                    continue;

                entity.TransformOriginalObjectId = entity.ObjectId;
                entity.FrameCache.Load(wrapper);
                entity.ObjectId = entity.TransformTargetObjectId;
                // BMD-023: state=501 transform branch must mirror baseline SetFrameImmediate:
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);

                int ownerStableId = entity.StableId;
                int ownerSlotIndex = entity.Runtime?.SlotIndex ?? ownerStableId;

                for (int j = 0; j < entities.Count; j++)
                {
                    LF2Entity child = entities[j];
                    if (child == null || child == entity)
                        continue;
                    if (child.KillCount != ownerStableId && child.KillCount != ownerSlotIndex)
                        continue;
                    if (child.Health != null && child.Health.HP <= 0)
                        continue;

                    child.FrameCache.Load(wrapper);
                    child.ObjectId = entity.ObjectId;
                    // BMD-023: state=501 child-transform branch must mirror baseline SetFrameImmediate.
                    // Same Y<0→212 / Y≥0→0 split as LF2Character.ApplyObjectSpecificFrameTickBeforeWaitAdvance:
                    // write Frame + FrameWaitCounter only, never Attacking. Unity's
                    // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                    child.DirectWriteFramePreserveWaitCounter(child.PS != null && child.PS.y < 0f ? 212 : 0);
                    RefreshRuntimeSnapshot(child);
                }
            }
        }

        public void FrameLogicBeforeAdvanceAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                LF2FrameData frame = entity.Frame?.D;
                if (!entity.SupportsFrameLogicBeforeAdvancePhase(frame))
                    return;

                entity.RunFrameLogicBeforeAdvance();
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void CaptureCollisionFrameSnapshotsAll()
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (entity.Runtime != null && entity.Runtime.SuppressCollisionCandidateUntilTick > 0)
                {
                    int currentTick = CurrentTickIndex;
                    if (currentTick < entity.Runtime.SuppressCollisionCandidateUntilTick)
                        return;
                }

                entity.CaptureCollisionFrameSnapshot();
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void CollectCollisionCandidatesAll()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.CollectCollisionCandidates();
        }

        public void EndCollisionCandidateConsumption()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.EndCollisionCandidateConsumption();
        }

        public void LateEntityUpdateAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
                {
                    LF2Entity obj = FindEntityByRuntimeSlotCurrent(runtimeSlot);

                    if (obj == null)
                        continue;
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    obj.RunStateSpecialPreCollision();
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    obj.RunPreCollisionRecoveryPhase(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    if (obj.Runtime != null && tickIndex < obj.Runtime.SuppressLateFrameTickUntilTick)
                    {
                        RefreshRuntimeSnapshot(obj);
                    }
                    else
                    {
                        obj.SimFrameTick(tickIndex);
                    }
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    obj.SimEntityCollision(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    if (HandleLateFrameTickExit(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    obj.RunLateDeathOpointPreCleanupPhase();
                    if (!IsActiveForCurrentPass(obj))
                        continue;
                    RefreshRuntimeSnapshot(obj);

                    var opointFactory = LF2ObjectPointFactory.Instance;
                    if (opointFactory != null)
                        opointFactory.ProcessOpointSpawnAlignedToCpp(obj);
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    if (obj.TryRunLatePostOpointCleanupPhase())
                    {
                        RefreshRuntimeSnapshot(obj);
                        continue;
                    }

                    obj.RunLateTailBeforePrevFrame();
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    RefreshRuntimeSnapshot(obj);
                    obj.MirrorLatePrevFrame();
                    RefreshRuntimeSnapshot(obj);
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private bool HandleLateFrameTickExit(LF2Entity entity)
        {
            if (entity?.Frame == null)
                return false;

            int frameId = entity.Frame.N;
            if (frameId < 0 || frameId >= 400)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            LF2FrameData frameData = entity.Frame.D;
            if (frameData != null && frameData.state == 9998)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            int frameGroup = frameId / 100;
            if (frameGroup == 11 || frameGroup == 12)
            {
                int ownerSlot = GetRuntimeSlotOrder(entity);
                GetAllEntities(_entityScratch);
                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity other = _entityScratch[i];
                    if (other != null && other.KillCount == ownerSlot)
                        other.HitStun = 1100 - frameId;
                }

                _entityScratch.Clear();
                entity.HitStun = 1100 - frameId;
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);
                return true;
            }

            if (frameId < 0 || frameId >= 400)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            return false;
        }

        public void EntityPostFrameTailAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity == null || entity.Health == null)
                    return;

                if (entity.HealTimer / 1000 == 1 && entity.Health.HP > 0)
                {
                    entity.HealTimer--;
                    if (entity.HealTimer % 8 == 0)
                    {
                        if (entity.Health.HP < entity.Health.HPBound)
                        {
                            entity.Health.HP += 8;
                            if (entity.Health.HP > entity.Health.HPBound)
                                entity.Health.HP = entity.Health.HPBound;
                        }
                        else
                        {
                            entity.HealTimer = 0;
                        }
                    }

                    if (entity.HealTimer % 1000 == 0)
                        entity.HealTimer = 0;
                }

                if (entity.CatchTimer > 0 && entity.Health.HP > 0)
                {
                    entity.CatchTimer--;
                    if (entity.CatchTimer % 8 == 0 && entity.Health.HP < entity.Health.HPBound)
                    {
                        entity.Health.HP += 8;
                        if (entity.Health.HP > entity.Health.HPBound)
                        {
                            entity.Health.HP = entity.Health.HPBound;
                            entity.CatchTimer = 0;
                        }
                    }
                }

                LF2FrameData frame = entity.Frame?.D;
                if (frame != null && frame.state == 1700)
                    entity.HealTimer = 1100;

                entity.ClearHitCandidateCarriers();

                RefreshRuntimeSnapshot(entity);
            });

            RunReleaseEntityCleanupTail();
        }

        private void RunReleaseEntityCleanupTail()
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null || entity.Health == null)
                    continue;

                LF2FrameData frame = entity.Frame?.D;
                int dataType = entity.GetCurrentDataObjectTypeForSimulation();

                if (dataType == (int)LF2ObjectType.Character)
                {
                    if (frame != null &&
                        entity.Health.HP <= 0 &&
                        frame.state == 14 &&
                        entity.FrameDelay <= 0 &&
                        entity.Runtime != null &&
                        entity.Runtime.WaitCounter > frame.wait * 3)
                    {
                        entity.FreeEntityLikeExe();
                    }

                    continue;
                }

                if (entity.Health.HP <= 0)
                    entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        public void FramePostProcessAll()
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity.FrameDelay != 0) return;

                if (entity.HitCount > 0)
                {
                    float denom = entity.HitCount + 1;
                    entity.PS.vx = entity.KnockbackVx * 2f / denom;
                    entity.PS.vy = entity.KnockbackVy * 2f / denom;
                    entity.PS.vz = entity.KnockbackVz * 2f / denom;
                }
                entity.KnockbackVx = 0f;
                entity.KnockbackVy = 0f;
                entity.KnockbackVz = 0f;
                entity.HitCount = 0;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void VrestTickAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                entity.ItrRest?.TickArest();
                entity.Runtime?.TickDefendLockCooldown();
                ClearAttackExemptIfCurrentFrameCannotHit(entity);
                RefreshRuntimeSnapshot(entity);
            });
        }

        private void ClearAttackExemptIfCurrentFrameCannotHit(LF2Entity entity)
        {
            if (entity == null || entity.AttackExempt <= 0)
                return;

            LF2FrameData frame = entity.Frame?.D;
            bool clear = frame == null;
            if (!clear)
            {
                bool hasList = frame.opoints != null && frame.opoints.Count > 0;
                bool hasSingle = frame.opoint.HasValue;
                clear = !hasList && !hasSingle;
            }

            if (clear)
                entity.AttackExempt = 0;
        }

        public void PostInteractionTickAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (!entity.SupportsPostInteractionPhase()) return;
                if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPostInteractionUntilTick)
                    return;
                entity.SimPostInteraction(tickIndex);
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void ObjectInteractionTickAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (!entity.SupportsObjectInteractionPhase()) return;
                if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressObjectInteractionUntilTick)
                    return;
                entity.SimObjectInteraction(tickIndex);
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void PreInteractionTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                GetActiveEntitiesByRuntimeSlot(_entityScratch);
                if (_entityScratch.Count == 0) return;

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    entity.RunCpointCheckStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    entity.RunCpointMismatchTailStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                _entityScratch.Clear();

                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        return;
                    if (!IsActiveForCurrentPass(entity))
                        return;

                    entity.RunWeaponSyncHeldStep10();
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });
            }
            finally
            {
                _entityScratch.Clear();
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        public void RandomWeaponDropTickAll(int tickIndex)
        {
            int weaponCount = 0;
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj is LF2Entity entity && entity.CountsAsRandomWeaponDropCandidate())
                        weaponCount++;
                }
            }
            if (weaponCount >= 4) return;
            if (Rng.NextInt(0, 200) != 0) return;

            var manager = CharacterAnimtorManager.Instance;
            if (manager == null) return;

            var candidates = new System.Collections.Generic.List<int>();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null) continue;
                if (oid == 122 || oid == 123)
                {
                    if (Rng.NextInt(0, 2) == 0) continue;
                }
                candidates.Add(oid);
            }
            if (candidates.Count == 0) return;

            int selectedOid = candidates[Rng.NextInt(0, candidates.Count)];

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(selectedOid);
            int flyFrame = -1;
            int minFrame = int.MaxValue;
            if (charData?.frames != null)
            {
                foreach (var f in charData.frames)
                {
                    if (f == null) continue;
                    if (f.frameId > 0 && f.frameId < minFrame) minFrame = f.frameId;
                    if (flyFrame < 0 && f.frameId > 0 && (
                        f.state == LF2States.WeaponInSky ||
                        f.state == LF2States.WeaponThrowing ||
                        f.state == LF2States.HeavyWeaponInSky))
                        flyFrame = f.frameId;
                }
            }
            if (flyFrame < 0) flyFrame = minFrame != int.MaxValue ? minFrame : 0;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out _, out _);
            if (stageWidth <= 60 || zMax - zMin <= 60) return;

            int r1 = Rng.NextInt(0, 30);
            int r2 = Rng.NextInt(0, 30);
            int r3 = Rng.NextInt(0, 30);
            int r4 = Rng.NextInt(0, 30);
            float lf2X = r1 * ((stageWidth - 60) / 30) + r2 + 30;
            float lf2Z = r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
            const float lf2Y = -500f;

            var spawnTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();

            spawnTask.opoint = new ObjectPoint
            {
                oid = selectedOid,
                kind = 0,
                action = flyFrame,
                x = Mathf.RoundToInt(lf2X),
                y = Mathf.RoundToInt(lf2Y),
                dvx = 0,
                dvy = 0,
                facing = 0,
            };
            spawnTask.parent = null; spawnTask.team = 0;
            spawnTask.pos = new UnityEngine.Vector3(lf2X, lf2Y, 0);
            spawnTask.z = lf2Z; spawnTask.dir = "right"; spawnTask.dvz = 0;
            factory.CreateObjectImmediate(spawnTask);
        }

        public void Mode2RandomWeaponDropTailAll(int tickIndex)
        {
            int mode2Request = Mode2Request;
            if (mode2Request == 0)
                return;

            if (mode2Request == 1)
            {
                SpawnMode2RandomWeapons();
            }
            else if (mode2Request == 2)
            {
                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (!entity.CountsAsRandomWeaponDropCandidate())
                        return;

                    entity.Runtime.WeaponFlightCounter = -1;
                    RefreshRuntimeSnapshot(entity);
                });
            }

            SetMode2Request(0);
        }

        private void SpawnMode2RandomWeapons()
        {
            var manager = CharacterAnimtorManager.Instance;
            if (manager == null)
                return;

            var candidates = new List<int>();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null)
                    continue;

                if (oid == 122 && Rng.NextInt(0, 2) == 0)
                    continue;

                candidates.Add(oid);
            }

            if (candidates.Count == 0)
                return;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out _, out _);
            if (stageWidth <= 60 || zMax - zMin <= 60)
                return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            for (int chooseIndex = 0; chooseIndex < candidates.Count; chooseIndex++)
            {
                int oid = candidates[chooseIndex];

                bool hasFreeSlot = false;
                for (int slot = DynamicRuntimeSlotStart; slot < MaxRuntimeSlots; slot++)
                {
                    if (!_runtimeSlotUsed[slot])
                    {
                        hasFreeSlot = true;
                        break;
                    }
                }

                if (!hasFreeSlot)
                    break;

                int r1 = Rng.NextInt(0, 30);
                int r2 = Rng.NextInt(0, 30);
                int r3 = Rng.NextInt(0, 30);
                int r4 = Rng.NextInt(0, 30);
                float lf2X = r1 * ((stageWidth - 60) / 30) + r2 + 30;
                float lf2Z = r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
                const float lf2Y = -500f;

                var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
                int flyFrame = -1;
                int minFrame = int.MaxValue;
                if (charData?.frames != null)
                {
                    foreach (var f in charData.frames)
                    {
                        if (f == null)
                            continue;
                        if (f.frameId > 0 && f.frameId < minFrame)
                            minFrame = f.frameId;
                        if (flyFrame < 0 && f.frameId > 0 &&
                            (f.state == LF2States.WeaponInSky ||
                             f.state == LF2States.WeaponThrowing ||
                             f.state == LF2States.HeavyWeaponInSky))
                        {
                            flyFrame = f.frameId;
                        }
                    }
                }

                if (flyFrame < 0)
                    flyFrame = minFrame != int.MaxValue ? minFrame : 0;

                var spawnTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                spawnTask.opoint = new ObjectPoint
                {
                    oid = oid,
                    kind = 0,
                    action = flyFrame,
                    x = Mathf.RoundToInt(lf2X),
                    y = Mathf.RoundToInt(lf2Y),
                    dvx = 0,
                    dvy = 0,
                    facing = 0,
                };
                spawnTask.parent = null;
                spawnTask.team = 0;
                spawnTask.pos = new Vector3(lf2X, lf2Y, 0f);
                spawnTask.z = lf2Z;
                spawnTask.dir = "right";
                spawnTask.dvz = 0f;
                factory.CreateObjectImmediate(spawnTask);
            }
        }
    }
}
