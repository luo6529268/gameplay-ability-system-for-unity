using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;

namespace NTSD.Simulation
{
    internal enum BattleLogicEntityCreationFailure
    {
        None = 0,
        InvalidTask = 1,
        MissingObjectDefinition = 2,
        MissingCharacterData = 3,
        LogicPoolExhausted = 4,
        RuntimeSlotRejected = 5,
    }

    /// <summary>
    /// Creates battle entities without touching GameObject, MonoBehaviour,
    /// SpriteRenderer, Transform, or the Unity presentation object pool.
    /// </summary>
    internal sealed class BattleLogicEntityFactory
    {
        private readonly SimulationWorld world;

        internal BattleLogicEntityFactory(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal LF2Entity Create(
            OPointCreateTask task,
            out BattleLogicEntityCreationFailure failure,
            float spreadDvz = 0f)
        {
            failure = BattleLogicEntityCreationFailure.None;
            if (task == null || task.opoint.oid <= 0)
            {
                failure = BattleLogicEntityCreationFailure.InvalidTask;
                return null;
            }

            int oid = task.opoint.oid;
            ObjectDefinition definition = world.RuntimeDataCatalog.GetObjectDefinition(oid);
            if (definition == null)
            {
                failure = BattleLogicEntityCreationFailure.MissingObjectDefinition;
                return null;
            }

            LF2CharacterDataWrapper characterConfig =
                world.RuntimeDataCatalog.GetCharacterConfig(oid);
            if (characterConfig?.characterData == null)
            {
                failure = BattleLogicEntityCreationFailure.MissingCharacterData;
                return null;
            }

            BattleLogicReferencePool referencePool = world.LogicReferencePool;
            ILF2Object logicObject = referencePool?.Get(
                (LF2ObjectType)definition.type,
                oid,
                world);
            if (logicObject is not LF2Entity entity)
            {
                referencePool?.Release(logicObject);
                failure = BattleLogicEntityCreationFailure.LogicPoolExhausted;
                return null;
            }

            task.targetWorld = world;
            if (entity is LF2WeaponBase weaponBase &&
                characterConfig.characterData.weapon_strength_list?.Count > 0)
            {
                weaponBase.SetWeaponStrengthList(
                    characterConfig.characterData.weapon_strength_list);
            }

            LF2Character character = entity as LF2Character;
            character?.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(task.requiredRuntimeSlot);
            PrepareFinalRuntimePosition(task);
            entity.Init(task, null);

            if (character != null)
            {
                character.ModuleBind(characterConfig, oid, world);
                character.Initialize(
                    NTSDGlobal.Default.Health.HpFull,
                    NTSDGlobal.Default.Health.MpFull);
            }

            if (entity.Runtime.SlotIndex < 0)
            {
                ReleaseRejected(entity);
                failure = BattleLogicEntityCreationFailure.RuntimeSlotRejected;
                return null;
            }

            PostInitLiving(
                entity,
                task.parent,
                task.opoint,
                definition.type,
                spreadDvz,
                task.releaseOpointSpawn);
            ApplyReleaseOpointDirectionalVz(entity, task);
            ApplyDirectVelocity(entity, task);

            if (task.frameDelay > 0)
                entity.FrameDelay = task.frameDelay;
            if (task.attackExempt > 0)
                entity.AttackExempt = task.attackExempt;
            if (task.ownerEntityIndex >= 0)
                entity.OwnerEntityIndex = task.ownerEntityIndex;

            return entity;
        }

        internal LF2Entity CreateSnapshotShell(
            int runtimeSlot,
            in BattleRuntimeSlotSnapshot state,
            in BattleEntityBaseShellSnapshot baseState,
            in BattleWeaponShellSnapshot weaponState,
            out BattleLogicEntityCreationFailure failure)
        {
            failure = BattleLogicEntityCreationFailure.None;
            if (!state.Claimed ||
                state.Generation == 0 ||
                runtimeSlot < 0 ||
                state.CurrentDataObjectId <= 0)
            {
                failure = BattleLogicEntityCreationFailure.InvalidTask;
                return null;
            }

            ObjectDefinition definition = world.RuntimeDataCatalog
                .GetObjectDefinition(state.CurrentDataObjectId);
            if (definition == null || definition.type != state.CurrentDataObjectType)
            {
                failure = BattleLogicEntityCreationFailure.MissingObjectDefinition;
                return null;
            }

            LF2CharacterDataWrapper characterConfig = world.RuntimeDataCatalog
                .GetCharacterConfig(state.CurrentDataObjectId);
            if (characterConfig?.characterData == null)
            {
                failure = BattleLogicEntityCreationFailure.MissingCharacterData;
                return null;
            }

            int poolWeaponType = weaponState.HasPoolWeaponType
                ? weaponState.PoolWeaponType
                : state.RuntimeEntityType;
            BattleLogicReferencePool referencePool = world.LogicReferencePool;
            ILF2Object logicObject = referencePool?.GetSnapshotShell(
                state.EntityKind,
                state.CurrentDataObjectId,
                poolWeaponType,
                world);
            if (logicObject is not LF2Entity entity ||
                BattleWorldRuntimeSlotSnapshotBuffer.ResolveEntityKind(entity) !=
                    state.EntityKind)
            {
                referencePool?.Release(logicObject);
                failure = BattleLogicEntityCreationFailure.LogicPoolExhausted;
                return null;
            }

            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            if (task == null)
            {
                referencePool.Release(entity);
                failure = BattleLogicEntityCreationFailure.LogicPoolExhausted;
                return null;
            }

            task.targetWorld = world;
            task.requiredRuntimeSlot = runtimeSlot;
            task.opoint = new ObjectPoint
            {
                oid = state.CurrentDataObjectId,
                kind = 1,
                action = baseState.FrameDataId >= 0
                    ? baseState.FrameDataId
                    : 0,
                facing = 0,
            };
            task.dir = "right";
            task.preserveActionZero = true;
            task.useDirectRuntimePosition = true;
            task.skipPostInitZOffset = true;

            if (entity is LF2WeaponBase weaponBase &&
                characterConfig.characterData.weapon_strength_list?.Count > 0)
            {
                weaponBase.SetWeaponStrengthList(
                    characterConfig.characterData.weapon_strength_list);
            }

            LF2Character character = entity as LF2Character;
            character?.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(runtimeSlot);
            entity.Init(task, null);
            if (character != null)
            {
                character.ModuleBind(
                    characterConfig,
                    state.CurrentDataObjectId,
                    world);
                character.Initialize(
                    NTSDGlobal.Default.Health.HpFull,
                    NTSDGlobal.Default.Health.MpFull);
            }

            referencePool.Recycle(task);
            if (entity.Runtime.SlotIndex != runtimeSlot)
            {
                ReleaseRejected(entity);
                failure = BattleLogicEntityCreationFailure.RuntimeSlotRejected;
                return null;
            }

            return entity;
        }

        internal static void PrepareFinalRuntimePosition(OPointCreateTask task)
        {
            if (task == null)
                return;

            double x = task.useDirectRuntimePosition ? task.directX : task.pos.x;
            double y = task.useDirectRuntimePosition ? task.directY : task.pos.y;
            double z = task.useDirectRuntimePosition ? task.directZ : task.z;
            if (!task.skipPostInitZOffset)
                z += 1.0;

            task.useDirectRuntimePosition = true;
            task.directX = x;
            task.directY = y;
            task.directZ = z;
            task.skipPostInitZOffset = true;

            if (!task.useInitialRuntimeIntPosition)
            {
                task.useInitialRuntimeIntPosition = true;
                task.initialRuntimeX = (int)x;
                task.initialRuntimeY = (int)y;
                task.initialRuntimeZ = (int)z;
            }
        }

        internal static void PostInitLiving(
            LF2Entity living,
            LF2Entity parent,
            ObjectPoint op,
            int objectType,
            float dvz,
            bool releaseOpointSpawn)
        {
            if (living == null)
                return;

            if (parent != null)
            {
                living.Team = parent.Team;
                living.RelationTeam = parent.RelationTeam;
                living.HolderCopySlot = parent.HolderCopySlot;
                living.OwnerId = releaseOpointSpawn
                    ? -1
                    : (parent.OwnerId > -1 ? parent.OwnerId : parent.StableId);

                if (objectType == 0)
                {
                    living.KillCount = parent.KillCount > -1
                        ? parent.KillCount
                        : GetRuntimeSlotOrStableId(parent);
                    living.HitStun = parent.HitStun;
                    living.AiControlled = releaseOpointSpawn;
                }
                else if (!releaseOpointSpawn)
                {
                    living.KillCount = parent.KillCount > -1
                        ? parent.KillCount
                        : parent.StableId;
                }
            }

            if (op.oid == 5 || op.oid == 52)
            {
                living.Health.HP = 10;
                living.Health.HPBound = 10;
                living.Health.HP3 = 10;
                living.Health.PP = 5;
            }

            if (op.kind == 2 && parent != null)
            {
                parent.TrackerFlag = 1;
                living.TrackerFlag = -1;
                living.TrackerParent = parent;
                if (parent is LF2Character parentCharacter)
                {
                    parentCharacter.AttachOpointHeldObject(living);
                }
                else
                {
                    int parentSlot = parent.Runtime?.SlotIndex ?? -1;
                    int livingSlot = living.Runtime?.SlotIndex ?? -1;
                    parent.Runtime.LinkState = 1;
                    parent.Runtime.TargetSlotIndex = livingSlot;
                    parent.Runtime.HeldWeaponStableId = livingSlot;
                    living.Runtime.LinkState = -1;
                    living.Runtime.HolderStableId = parentSlot;
                }
                living.Team = parent.Team;
            }

            if (dvz != 0f)
            {
                living.PS.vz += dvz;
                float absoluteDvz = Math.Abs(dvz);
                if (living.PS.vx > 0f)
                    living.PS.vx -= absoluteDvz;
                else if (living.PS.vx < 0f)
                    living.PS.vx += absoluteDvz;
                else
                    living.PS.vx += dvz;
            }
        }

        internal static void ApplyDirectVelocity(
            LF2Entity living,
            OPointCreateTask task)
        {
            if (living?.PS == null || task == null || !task.useDirectVelocity)
                return;
            living.PS.vx = task.directVx;
            living.PS.vy = task.directVy;
            living.PS.vz = task.directVz;
        }

        internal static void ApplyReleaseOpointDirectionalVz(
            LF2Entity living,
            OPointCreateTask task)
        {
            if (living?.PS == null || task?.parent == null ||
                !task.releaseOpointSpawn || task.useDirectVelocity)
            {
                return;
            }

            LF2FrameData frame = living.Frame?.D;
            if (frame == null)
                return;

            int state = frame.state;
            if (state != LF2States.ProjectileFlying &&
                state != LF2States.WeaponThrowing &&
                state != LF2States.ObjectExpanding)
            {
                return;
            }
            if (task.opoint.oid == 223 || task.opoint.oid == 224)
                return;

            bool up = task.parent.Runtime.KeyUp != 0;
            bool down = task.parent.Runtime.KeyDown != 0;
            if (up && !down)
                living.PS.vz = -2.5f;
            else if (down && !up)
                living.PS.vz = 2.5f;

            if (task.opoint.oid == 211)
                living.PS.vz *= 0.25f;
        }

        private void ReleaseRejected(LF2Entity entity)
        {
            entity.UnregisterFromWorld();
            entity.Reset();
            world.LogicReferencePool?.Release(entity);
        }

        private static int GetRuntimeSlotOrStableId(LF2Entity entity)
        {
            if (entity == null)
                return -1;
            return entity.Runtime.SlotIndex >= 0
                ? entity.Runtime.SlotIndex
                : entity.StableId;
        }
    }
}
