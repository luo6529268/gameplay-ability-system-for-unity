using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Runtime fields shared by every battle entity.
    /// This mirrors the formal C++ release entity layout at a semantic level, while Unity keeps
    /// rendering, pooling, and component references outside the gameplay state.
    /// </summary>
    [Serializable]
    public sealed class NTSDEntityRuntime
    {
        public const int CharacterObjType = 0;

        public int SlotIndex = -1;
        public int StableId;
        public int ObjectId;

        public int ObjType;
        public int EntityType;

        public int Team;
        public int TeamSide;
        public int OwnerSlotIndex = -1;
        public int OwnerStableId = -1;
        public int GrabbedBy;
        public int TrackerFlag;
        public int TrackerChildStableId = -1;
        public int TrackerParentStableId = -1;

        public int Frame;
        public int PrevFrame;
        public int PrevFrame2;
        public int WaitCounter;
        public int NextFrame;
        public int FrameDelay;
        public int HitStop;
        public float KnockbackVx;
        public float KnockbackVy;
        public float KnockbackVz;
        public int CharType;
        public int ShakeTimer;
        public int AttackExempt;
        public int ShotCount;
        public int MergeFlag = -1;
        public int MergePartnerSlotIndex = -1;
        public int MergeSelfObjectId;
        public int MergePartnerObjectId;
        public int MergeTimer;
        public int RespawnCount;
        public int RespawnCountdown;

        public readonly int[] InputHistory = new int[6];

        public bool IsCharacter => ObjType == CharacterObjType;
        public bool IsNonCharacter => ObjType != CharacterObjType;

        public void Reset()
        {
            SlotIndex = -1;
            StableId = 0;
            ObjectId = 0;
            ObjType = 0;
            EntityType = 0;
            Team = 0;
            TeamSide = 0;
            OwnerSlotIndex = -1;
            OwnerStableId = -1;
            GrabbedBy = 0;
            TrackerFlag = 0;
            TrackerChildStableId = -1;
            TrackerParentStableId = -1;
            Frame = 0;
            PrevFrame = 0;
            PrevFrame2 = 0;
            WaitCounter = 0;
            NextFrame = 0;
            FrameDelay = 0;
            HitStop = 0;
            KnockbackVx = 0f;
            KnockbackVy = 0f;
            KnockbackVz = 0f;
            CharType = 0;
            ShakeTimer = 0;
            AttackExempt = 0;
            ShotCount = 0;
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
