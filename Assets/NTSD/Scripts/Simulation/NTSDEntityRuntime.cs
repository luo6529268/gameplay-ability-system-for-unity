using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// 所有战斗实体共享的运行时字段。
    /// 这里按语义镜像 C++ release 实体布局；Unity 的渲染、对象池和组件引用不写入战斗真相状态。
    /// </summary>
    [Serializable]
    public sealed class NTSDEntityRuntime
    {
        public int SlotIndex = -1;
        public int StableId;
        public int ObjectId;

        public int ObjType;
        public int EntityType;

        public int Team;
        public int OwnerSlotIndex = -1;
        public int OwnerStableId = -1;
        public int GrabbedBy;
        public int LinkState;
        public int TargetSlotIndex = -1;
        public int HeldWeaponStableId = -1;
        public int CaughtDuration;
        public int CaughtFrontFlag = 1;
        public int CatchingStateTU;
        public int JumpAttackLock;
        public int AnimCounter;
        public int AnimSub;
        public int HolderStableId = -1;
        public int PickerStableId = -1;
        public int TrackerFlag;
        public bool AiControlled;

        public float X;
        public float Y;
        public float Z;
        public float Vx;
        public float Vy;
        public float Vz;
        public float SpriteX;
        public float SpriteY;
        public float SpriteZ;

        public int Frame;
        public int WaitCounter;
        public int NextFrame;
        public int AttackingCounter;
        public int FrameDelay;
        public int HitStop;
        public float KnockbackVx;
        public float KnockbackVy;
        public float KnockbackVz;
        public int ShakeTimer;
        public int AttackExempt;
        public int HitStateCount;
        public int Fall;
        public int Bdefend;
        public int HitCount;
        public int HitConfirmEa;
        public int HealTimer;
        public int KillCount = -1;
        public int ShotCount;
        public int WeaponCount;
        public int FallDamageDiv;
        public int WeaponFlightCounter;
        public int WeaponDropHurt;
        public int WeaponState;
        public int Blink;

        public int HP = 500;
        public int HPBound = 500;
        public int HPLost;
        public int MP = 500;
        public int MPMax = 500;
        public int PP = 500;
        public int PPMax = 500;
        public int PPBound = 500;

        public int MergeFlag = -1;
        public int MergePartnerSlotIndex = -1;
        public int MergeSelfObjectId;
        public int MergePartnerObjectId;
        public int MergeTimer;
        public int RespawnCount;
        public int RespawnCountdown;

        public readonly int[] InputHistory = new int[6];

        public void Reset()
        {
            SlotIndex = -1;
            StableId = 0;
            ObjectId = 0;
            ObjType = 0;
            EntityType = 0;
            Team = 0;
            OwnerSlotIndex = -1;
            OwnerStableId = -1;
            GrabbedBy = 0;
            LinkState = 0;
            TargetSlotIndex = -1;
            HeldWeaponStableId = -1;
            CaughtDuration = 0;
            CaughtFrontFlag = 1;
            CatchingStateTU = 0;
            JumpAttackLock = 0;
            AnimCounter = 0;
            AnimSub = 0;
            HolderStableId = -1;
            PickerStableId = -1;
            TrackerFlag = 0;
            AiControlled = false;
            X = 0f;
            Y = 0f;
            Z = 0f;
            Vx = 0f;
            Vy = 0f;
            Vz = 0f;
            SpriteX = 0f;
            SpriteY = 0f;
            SpriteZ = 0f;
            Frame = 0;
            WaitCounter = 0;
            NextFrame = 0;
            AttackingCounter = 0;
            FrameDelay = 0;
            HitStop = 0;
            KnockbackVx = 0f;
            KnockbackVy = 0f;
            KnockbackVz = 0f;
            ShakeTimer = 0;
            AttackExempt = 0;
            HitStateCount = 0;
            Fall = 0;
            Bdefend = 0;
            HitCount = 0;
            HitConfirmEa = 0;
            HealTimer = 0;
            KillCount = -1;
            ShotCount = 0;
            WeaponCount = 0;
            FallDamageDiv = 0;
            WeaponFlightCounter = 0;
            WeaponDropHurt = 0;
            WeaponState = 0;
            Blink = 0;
            HP = 500;
            HPBound = 500;
            HPLost = 0;
            MP = 500;
            MPMax = 500;
            PP = 500;
            PPMax = 500;
            PPBound = 500;
            MergeFlag = -1;
            MergePartnerSlotIndex = -1;
            MergeSelfObjectId = 0;
            MergePartnerObjectId = 0;
            MergeTimer = 0;
            RespawnCount = 0;
            RespawnCountdown = 0;
            Array.Clear(InputHistory, 0, InputHistory.Length);
        }
    }
}
