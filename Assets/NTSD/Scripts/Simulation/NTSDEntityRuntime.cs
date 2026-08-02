using System;
using System.Threading;

namespace NTSD.Simulation
{
    /// <summary>
    /// 所有战斗实体共享的运行时字段。
    /// 这里按语义镜像 C++ release 实体布局；Unity 的渲染、对象池和组件引用不写入战斗真相状态。
    /// </summary>
    [Serializable]
    public sealed class NTSDEntityRuntime
    {
        private static long pendingFlushDestroyMutationEpoch;
        private int pendingFlushDestroy;

        public static long PendingFlushDestroyMutationEpochForDiagnostics =>
            Volatile.Read(ref pendingFlushDestroyMutationEpoch);

        public int SlotIndex = -1;
        public int StableId;
        public int ObjectId;

        public int ObjType;
        public int EntityType;
        public int TransformOriginalObjectId = -1;
        public int TransformTargetObjectId = -1;

        public int Team;
        public int RelationTeam;
        public int OwnerSlotIndex = -1;
        public int OwnerStableId = -1;
        public int RelationOwnerSlotIndex = -1;
        public int SpawnerSlotIndex = -1;
        public int GrabbedBy;
        public int LinkState;
        public int TargetSlotIndex = -1;
        public int CaughtSlotIndex = -1;
        public int CatcherSlotIndex = -1;
        public int HeldWeaponStableId = -1;
        public int ThrowFrameGuard = -1;
        public int ReleaseTick = -1;
        public int CaughtDuration;
        public int PickupCount;
        public int CaughtFrontFlag = 1;
        public int CatchingStateTU;
        public int JumpAttackLock;
        public int AnimCounter;
        public int AnimSub;
        public int LateSpecialTargetX;
        public int LateSpecialTargetZ;
        public int[] InputHistory = new int[6];
        public byte CdAttack;
        public byte CdJump;
        public byte CdDefend;
        public byte CdDefendLock;
        public byte CdRight;
        public byte CdLeft;
        public byte CdUp;
        public byte CdDown;
        public byte ComboDra;
        public byte ComboDla;
        public byte ComboDua;
        public byte ComboDda;
        public byte ComboDrj;
        public byte ComboDlj;
        public byte ComboDuj;
        public byte ComboDdj;
        public byte ComboDja;
        public byte PrevUp;
        public byte PrevDown;
        public byte PrevLeft;
        public byte PrevRight;
        public byte PrevJump;
        public byte PrevDefend;
        public byte PrevAttack;
        public byte KeyUp;
        public byte KeyDown;
        public byte KeyLeft;
        public byte KeyRight;
        public byte KeyAttack;
        public byte KeyJump;
        public byte KeyDefend;
        public int HolderStableId = -1;
        public int HolderCopySlotIndex = 99;
        public int PickerStableId = -1;
        public int TrackerFlag;
        public bool AiControlled;

        public double X;
        public double Y;
        public double Z;
        public int XInt;
        public int YInt;
        public int ZInt;
        public double Vx;
        public double Vy;
        public double Vz;
        public float SpriteX;
        public float SpriteY;
        public float SpriteZ;
        public double Type3VisualZOffset;
        public float RenderOffsetX;
        public string Dir = "right";
        public float Zz;
        public bool XBoundPositive;
        public bool XBoundNegative;
        public bool ZBoundPositive;
        public bool ZBoundNegative;

        public int Frame;
        public int PrevFrame2;
        public int FirstPresentationTick;
        public int SpawnSemantic;
        public int SuppressFrameTickUntilTick;
        public int SuppressLateFrameTickUntilTick;
        public int SuppressPostInteractionUntilTick;
        public int SuppressObjectInteractionUntilTick;
        public int SuppressPreInteractionUntilTick;
        public int SuppressCollisionCandidateUntilTick;
        public int RenderPicOffset;
        public int WaitCounter;
        public int FrameWaitCounter;
        public int NextFrame;
        public int AttackingCounter;
        public int FrameDelay;
        public int HitStop;
        public double KnockbackVx = 0.1;
        public double KnockbackVy = 0.1;
        public double KnockbackVz = 0.1;
        public int ShakeTimer;
        public int AttackExempt;
        public int HitStateCount;
        public int Fall;
        public int Bdefend;
        public int HitCount;
        public int HitConfirmEa;
        public int HitConfirm2;
        public int HealTimer;
        public int CatchTimer;
        public int KillCount = -1;
        public int ComboCountVic;
        public int ComboCountAtk;
        public int KillStat;
        public int Unk328 = -1;
        public int Unk32C = -1;
        public int Unk330;
        public int Unk334;
        public int Unk338;
        public int Unk344;
        public int Unk360 = -1;
        public int Unk3FC = -1000;
        public int Unk400 = -1000;
        public int ShotCount;
        public int WeaponCount;
        public int FallDamageDiv;
        public int WeaponFlightCounter;
        public int WeaponDropHurt;
        public int WeaponState;
        public int Blink;
        public int HitCandidateCount;
        public int HitCandidateNearestDistance = 1000;
        public int HitCandidateKind1Distance = 1000;
        public int HitCandidateExtraDistance = 1000;
        public int TransientMp;
        public int TransientMp2 = 1000;
        public int TransientMp3 = 1000;
        public int TransientMp4 = 1000;
        public bool OidMergeDormant;
        public bool PendingFlushDestroy
        {
            get => Volatile.Read(ref pendingFlushDestroy) != 0;
            set
            {
                int next = value ? 1 : 0;
                if (Interlocked.Exchange(ref pendingFlushDestroy, next) != next)
                    Interlocked.Increment(ref pendingFlushDestroyMutationEpoch);
            }
        }

        public int HP = 500;
        public int HPBound = 500;
        public int HP3 = 500;
        public int HPOrig;
        public int HP2Orig;
        public int RespawnCount;
        public int HPLost;
        public int MP = 500;
        public int MPMax = 500;
        public int PP = 500;
        public int PPMax = 500;
        public int PPBound = 500;
        public int PpDisplay;

        public void SetPosition(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void SetVelocity(double vx, double vy, double vz)
        {
            Vx = vx;
            Vy = vy;
            Vz = vz;
        }

        public void SyncIntegerPosition()
        {
            XInt = (int)X;
            YInt = (int)Y;
            ZInt = (int)Z;
        }

        public void UpdateSpriteOrigin(int centerx, int centery, float spriteWidthPx)
        {
            SpriteX = (float)(Dir == "right"
                ? X - centerx
                : X + centerx - spriteWidthPx);
            SpriteY = (float)(Y + Z - centery);
            SpriteZ = (float)Z;
        }

        public void ClearBounds()
        {
            XBoundPositive = false;
            XBoundNegative = false;
            ZBoundPositive = false;
            ZBoundNegative = false;
        }

        public int ResolveActiveHeldSlotIndex()
        {
            return LinkState > 0 ? TargetSlotIndex : -1;
        }

        public int ResolveActiveHolderSlotIndex()
        {
            return LinkState < 0 ? HolderStableId : -1;
        }

        public bool IsActivelyHeldBySlot(int holderSlotIndex)
        {
            return LinkState < 0 && HolderStableId == holderSlotIndex;
        }

        public void RollInputFromCurrent()
        {
            PrevUp = KeyUp;
            PrevDown = KeyDown;
            PrevLeft = KeyLeft;
            PrevRight = KeyRight;
            PrevJump = KeyJump;
            PrevDefend = KeyDefend;
            PrevAttack = KeyAttack;
        }

        public bool HasInputHistoryGate()
        {
            EnsureInputHistory();
            return InputHistory[0] != 0;
        }

        public void ClearDirectionalInputKeys()
        {
            KeyUp = KeyDown = KeyLeft = KeyRight = 0;
        }

        public void ClearActionInputKeys()
        {
            KeyAttack = KeyJump = KeyDefend = 0;
        }

        public void ResetInputState()
        {
            CdAttack = CdJump = CdDefend = CdDefendLock = CdRight = CdLeft = CdUp = CdDown = 0;
            ComboDra = ComboDla = ComboDua = ComboDda = ComboDrj = ComboDlj = ComboDuj = ComboDdj = ComboDja = 0;
            EnsureInputHistory();
            Array.Clear(InputHistory, 0, InputHistory.Length);
            PrevUp = PrevDown = PrevLeft = PrevRight = PrevJump = PrevDefend = PrevAttack = 0;
            ClearDirectionalInputKeys();
            ClearActionInputKeys();
        }

        public void ApplyInputEdges()
        {
            if (PrevRight == 0 && KeyRight == 1) { CdRight = 5; PushInputHistory(6); }
            if (PrevLeft == 0 && KeyLeft == 1) { CdLeft = 5; PushInputHistory(4); }
            if (PrevUp == 0 && KeyUp == 1) { CdUp = 5; PushInputHistory(8); }
            if (PrevDown == 0 && KeyDown == 1) { CdDown = 5; PushInputHistory(2); }
            if (PrevAttack == 0 && KeyAttack == 1) { CdDefend = 5; PushInputHistory(9); }
            if (PrevDefend == 0 && KeyDefend == 1) { CdJump = 5; PushInputHistory(0); }
            if (PrevJump == 0 && KeyJump == 1) { CdAttack = 5; PushInputHistory(5); }
        }

        public void PushInputHistory(int keyNum)
        {
            EnsureInputHistory();
            InputHistory[1] = InputHistory[2];
            InputHistory[2] = InputHistory[3];
            InputHistory[3] = InputHistory[4];
            InputHistory[4] = InputHistory[5];
            InputHistory[5] = keyNum;
        }

        public void SetInputHistoryGate(bool enabled)
        {
            EnsureInputHistory();
            InputHistory[0] = enabled ? 1 : 0;
        }

        public void ClearInputHistoryTail()
        {
            EnsureInputHistory();
            Array.Clear(InputHistory, 1, InputHistory.Length - 1);
        }

        public void TickInputCooldowns()
        {
            if (CdRight > 0) CdRight--;
            if (CdLeft > 0) CdLeft--;
            if (CdUp > 0) CdUp--;
            if (CdDown > 0) CdDown--;
            if (CdJump > 0) CdJump--;
            if (CdAttack > 0) CdAttack--;
            if (CdDefend > 0) CdDefend--;
            if (CdDefendLock > 0) CdDefendLock--;
        }

        private void EnsureInputHistory()
        {
            if (InputHistory == null || InputHistory.Length != 6)
                InputHistory = new int[6];
        }

        public void Reset()
        {
            SlotIndex = -1;
            StableId = 0;
            ObjectId = 0;
            ObjType = 0;
            EntityType = 0;
            TransformOriginalObjectId = -1;
            TransformTargetObjectId = -1;
            Team = 0;
            RelationTeam = 0;
            OwnerSlotIndex = -1;
            OwnerStableId = -1;
            RelationOwnerSlotIndex = -1;
            SpawnerSlotIndex = -1;
            GrabbedBy = 0;
            LinkState = 0;
            TargetSlotIndex = -1;
            CaughtSlotIndex = -1;
            CatcherSlotIndex = -1;
            HeldWeaponStableId = -1;
            ThrowFrameGuard = -1;
            ReleaseTick = -1;
            CaughtDuration = 0;
            PickupCount = 0;
            CaughtFrontFlag = 1;
            CatchingStateTU = 0;
            JumpAttackLock = 0;
            AnimCounter = 0;
            AnimSub = 0;
            LateSpecialTargetX = 0;
            LateSpecialTargetZ = 0;
            EnsureInputHistory();
            Array.Clear(InputHistory, 0, InputHistory.Length);
            CdAttack = 0;
            CdJump = 0;
            CdDefend = 0;
            CdDefendLock = 0;
            CdRight = 0;
            CdLeft = 0;
            CdUp = 0;
            CdDown = 0;
            ComboDra = 0;
            ComboDla = 0;
            ComboDua = 0;
            ComboDda = 0;
            ComboDrj = 0;
            ComboDlj = 0;
            ComboDuj = 0;
            ComboDdj = 0;
            ComboDja = 0;
            PrevUp = 0;
            PrevDown = 0;
            PrevLeft = 0;
            PrevRight = 0;
            PrevJump = 0;
            PrevDefend = 0;
            PrevAttack = 0;
            KeyUp = 0;
            KeyDown = 0;
            KeyLeft = 0;
            KeyRight = 0;
            KeyAttack = 0;
            KeyJump = 0;
            KeyDefend = 0;
            HolderStableId = -1;
            HolderCopySlotIndex = 99;
            PickerStableId = -1;
            TrackerFlag = 0;
            AiControlled = false;
            X = 0f;
            Y = 0f;
            Z = 0f;
            XInt = 0;
            YInt = 0;
            ZInt = 0;
            Vx = 0f;
            Vy = 0f;
            Vz = 0f;
            SpriteX = 0f;
            SpriteY = 0f;
            SpriteZ = 0f;
            Type3VisualZOffset = 0.0;
            RenderOffsetX = 0f;
            Dir = "right";
            Zz = 0f;
            ClearBounds();
            Frame = 0;
            PrevFrame2 = 0;
            FirstPresentationTick = 0;
            SpawnSemantic = 0;
            SuppressFrameTickUntilTick = 0;
            SuppressLateFrameTickUntilTick = 0;
            SuppressPostInteractionUntilTick = 0;
            SuppressObjectInteractionUntilTick = 0;
            SuppressPreInteractionUntilTick = 0;
            SuppressCollisionCandidateUntilTick = 0;
            RenderPicOffset = 0;
            WaitCounter = 0;
            FrameWaitCounter = 0;
            NextFrame = 0;
            AttackingCounter = 0;
            FrameDelay = 0;
            HitStop = 0;
            KnockbackVx = 0.1;
            KnockbackVy = 0.1;
            KnockbackVz = 0.1;
            ShakeTimer = 0;
            AttackExempt = 0;
            HitStateCount = 0;
            Fall = 0;
            Bdefend = 0;
            HitCount = 0;
            HitConfirmEa = 0;
            HitConfirm2 = 0;
            HealTimer = 0;
            CatchTimer = 0;
            KillCount = -1;
            ComboCountVic = 0;
            ComboCountAtk = 0;
            KillStat = 0;
            Unk328 = -1;
            Unk32C = -1;
            Unk330 = 0;
            Unk334 = 0;
            Unk338 = 0;
            Unk344 = 0;
            Unk360 = -1;
            Unk3FC = -1000;
            Unk400 = -1000;
            ShotCount = 0;
            WeaponCount = 0;
            FallDamageDiv = 0;
            WeaponFlightCounter = 0;
            WeaponDropHurt = 0;
            WeaponState = 0;
            Blink = 0;
            HitCandidateCount = 0;
            HitCandidateNearestDistance = 1000;
            HitCandidateKind1Distance = 1000;
            HitCandidateExtraDistance = 1000;
            TransientMp = 0;
            TransientMp2 = 1000;
            TransientMp3 = 1000;
            TransientMp4 = 1000;
            OidMergeDormant = false;
            PendingFlushDestroy = false;
            HP = 500;
            HPBound = 500;
            HP3 = 500;
            HPOrig = 0;
            HP2Orig = 0;
            RespawnCount = 0;
            HPLost = 0;
            MP = 500;
            MPMax = 500;
            PP = 500;
            PPMax = 500;
            PPBound = 500;
            PpDisplay = 0;
        }
    }
}
