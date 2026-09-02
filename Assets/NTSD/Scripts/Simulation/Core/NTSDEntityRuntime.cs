using System;
using System.Runtime.CompilerServices;
using System.Threading;
using NTSD.Simulation.Ecs;

namespace NTSD.Simulation
{
    /// <summary>
    /// 所有战斗实体共享的运行时字段。
    /// 这里按语义镜像 C++ release 实体布局；Unity 的渲染、对象池和组件引用不写入战斗真相状态。
    /// </summary>
    [Serializable]
    public sealed class NTSDEntityRuntime
    {
        private int pendingFlushDestroy;
        private long pendingFlushDestroyMutationEpoch;
        [NonSerialized] private SimulationWorldMutationTracker worldMutationTracker;
        [NonSerialized] private BattleFrameMotionStore frameMotionStore;
        [NonSerialized] private int frameMotionStoreSlot = -1;
        [NonSerialized] private BattleRelationLinkStore relationLinkStore;
        [NonSerialized] private int relationLinkStoreSlot = -1;
        [NonSerialized] private BattleVitalStore vitalStore;
        [NonSerialized] private int vitalStoreSlot = -1;

        public long PendingFlushDestroyMutationEpochForDiagnostics =>
            Volatile.Read(ref pendingFlushDestroyMutationEpoch);

        internal void BindWorldMutationTracker(
            SimulationWorldMutationTracker mutationTracker)
        {
            if (ReferenceEquals(worldMutationTracker, mutationTracker))
                return;

            SimulationWorldMutationTracker previous = worldMutationTracker;
            worldMutationTracker = mutationTracker;
            previous?.NotifyPendingFlushDestroyMutation();
            mutationTracker?.NotifyPendingFlushDestroyMutation();
        }

        internal void BindFrameMotionStore(
            BattleFrameMotionStore store,
            int slot)
        {
            frameMotionStore = store;
            frameMotionStoreSlot = store != null ? slot : -1;
        }

        internal void UnbindFrameMotionStore(
            BattleFrameMotionStore store,
            int slot)
        {
            if (!ReferenceEquals(frameMotionStore, store) ||
                frameMotionStoreSlot != slot)
            {
                return;
            }

            frameMotionStore = null;
            frameMotionStoreSlot = -1;
        }

        internal void BindRelationLinkStore(
            BattleRelationLinkStore store,
            int slot)
        {
            relationLinkStore = store;
            relationLinkStoreSlot = store != null ? slot : -1;
        }

        internal void UnbindRelationLinkStore(
            BattleRelationLinkStore store,
            int slot)
        {
            if (!ReferenceEquals(relationLinkStore, store) ||
                relationLinkStoreSlot != slot)
            {
                return;
            }

            relationLinkStore = null;
            relationLinkStoreSlot = -1;
        }

        internal void BindVitalStore(BattleVitalStore store, int slot)
        {
            vitalStore = store;
            vitalStoreSlot = store != null ? slot : -1;
        }

        internal void UnbindVitalStore(BattleVitalStore store, int slot)
        {
            if (!ReferenceEquals(vitalStore, store) || vitalStoreSlot != slot)
                return;

            vitalStore = null;
            vitalStoreSlot = -1;
        }

        public int SlotIndex = -1;
        public int StableId;
        public int ObjectId;

        public int ObjType;
        public int EntityType;
        public int TransformOriginalObjectId = -1;
        public int TransformTargetObjectId = -1;

        public int Team;
        private int relationTeam;
        public int OwnerSlotIndex = -1;
        public int OwnerStableId = -1;
        public int RelationOwnerSlotIndex = -1;
        public int SpawnerSlotIndex = -1;
        public int GrabbedBy;
        private int linkState;
        private int targetSlotIndex = -1;
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
        private int xInt;
        private int yInt;
        private int zInt;
        private double vx;
        public double Vy;
        public double Vz;
        private byte facingLeft;
        private int frame;
        private int frameState;
        private int hitStop;

        public int XInt
        {
            get => xInt;
            set => SetFrameMotionValue(ref xInt, value, RuntimeFrameMotionField.XInt);
        }

        public int YInt
        {
            get => yInt;
            set => SetFrameMotionValue(ref yInt, value, RuntimeFrameMotionField.YInt);
        }

        public int ZInt
        {
            get => zInt;
            set => SetFrameMotionValue(ref zInt, value, RuntimeFrameMotionField.ZInt);
        }

        public double Vx
        {
            get => vx;
            set => SetFrameMotionValue(ref vx, value, RuntimeFrameMotionField.Vx);
        }

        public float SpriteX;
        public float SpriteY;
        public float SpriteZ;
        public double Type3VisualZOffset;
        public float RenderOffsetX;
        public string Dir
        {
            get => facingLeft != 0 ? "left" : "right";
            set
            {
                byte nextFacingLeft = value == "left" ? (byte)1 : (byte)0;
                if (facingLeft == nextFacingLeft)
                    return;

                facingLeft = nextFacingLeft;
                frameMotionStore?.CaptureFacing(
                    frameMotionStoreSlot,
                    facingLeft != 0);
            }
        }

        public bool IsFacingLeft => facingLeft != 0;

        public bool IsFacingRight => facingLeft == 0;
        public float Zz;
        public bool XBoundPositive;
        public bool XBoundNegative;
        public bool ZBoundPositive;
        public bool ZBoundNegative;

        public int Frame
        {
            get => frame;
            set => SetFrameMotionValue(ref frame, value, RuntimeFrameMotionField.Frame);
        }

        internal int FrameState
        {
            get => frameState;
            set => SetFrameMotionValue(
                ref frameState,
                value,
                RuntimeFrameMotionField.State);
        }

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

        public int HitStop
        {
            get => hitStop;
            set => SetFrameMotionValue(
                ref hitStop,
                value,
                RuntimeFrameMotionField.HitStop);
        }
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
        private int killCount = -1;
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
                {
                    Interlocked.Increment(ref pendingFlushDestroyMutationEpoch);
                    worldMutationTracker?.NotifyPendingFlushDestroyMutation();
                }
            }
        }

        private int hp = 500;
        private int hpBound = 500;
        private int hp3 = 500;
        public int HPOrig;
        public int HP2Orig;
        public int RespawnCount;
        public int HPLost;
        public int MP = 500;
        public int MPMax = 500;
        private int pp = 500;
        public int PPMax = 500;
        public int PPBound = 500;
        public int PpDisplay;

        public int HP
        {
            get => hp;
            set => SetVitalValue(ref hp, value, RuntimeVitalField.Hp);
        }

        public int HPBound
        {
            get => hpBound;
            set => SetVitalValue(ref hpBound, value, RuntimeVitalField.HpBound);
        }

        public int HP3
        {
            get => hp3;
            set => SetVitalValue(ref hp3, value, RuntimeVitalField.Hp3);
        }

        public int PP
        {
            get => pp;
            set => SetVitalValue(ref pp, value, RuntimeVitalField.Pp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetVitalValue(
            ref int field,
            int value,
            RuntimeVitalField changedField)
        {
            if (field == value)
                return;

            field = value;
            vitalStore?.CaptureChangedField(vitalStoreSlot, changedField, value);
        }

        public int RelationTeam
        {
            get => relationTeam;
            set => SetRelationLinkValue(
                ref relationTeam,
                value,
                RuntimeRelationLinkField.RelationTeam);
        }

        public int LinkState
        {
            get => linkState;
            set => SetRelationLinkValue(
                ref linkState,
                value,
                RuntimeRelationLinkField.LinkState);
        }

        public int KillCount
        {
            get => killCount;
            set => SetRelationLinkValue(
                ref killCount,
                value,
                RuntimeRelationLinkField.KillCount);
        }

        public int TargetSlotIndex
        {
            get => targetSlotIndex;
            set => SetRelationLinkValue(
                ref targetSlotIndex,
                value,
                RuntimeRelationLinkField.TargetSlot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetRelationLinkValue(
            ref int field,
            int value,
            RuntimeRelationLinkField changedField)
        {
            if (field == value)
                return;

            field = value;
            relationLinkStore?.CaptureChangedField(
                relationLinkStoreSlot,
                changedField,
                value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFrameMotionValue(
            ref double field,
            double value,
            RuntimeFrameMotionField changedField)
        {
            if (field.Equals(value))
                return;

            field = value;
            frameMotionStore?.CaptureChangedField(
                frameMotionStoreSlot,
                changedField,
                value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFrameMotionValue(
            ref int field,
            int value,
            RuntimeFrameMotionField changedField)
        {
            if (field == value)
                return;

            field = value;
            frameMotionStore?.CaptureChangedField(
                frameMotionStoreSlot,
                changedField,
                value);
        }

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
            int nextX = (int)X;
            int nextY = (int)Y;
            int nextZ = (int)Z;

            if (xInt == nextX && yInt == nextY && zInt == nextZ)
                return;

            xInt = nextX;
            yInt = nextY;
            zInt = nextZ;
            frameMotionStore?.CaptureIntegerPosition(
                frameMotionStoreSlot,
                nextX,
                nextY,
                nextZ);
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

        internal bool HasCanonicalSnapshotStorage =>
            InputHistory != null && InputHistory.Length == 6;

        internal bool TryCopyCanonicalStateTo(NTSDEntityRuntime destination)
        {
            if (destination == null ||
                !HasCanonicalSnapshotStorage ||
                !destination.HasCanonicalSnapshotStorage)
            {
                return false;
            }

            destination.pendingFlushDestroy =
                Volatile.Read(ref pendingFlushDestroy);
            destination.pendingFlushDestroyMutationEpoch =
                Volatile.Read(ref pendingFlushDestroyMutationEpoch);
            destination.SlotIndex = SlotIndex;
            destination.StableId = StableId;
            destination.ObjectId = ObjectId;
            destination.ObjType = ObjType;
            destination.EntityType = EntityType;
            destination.TransformOriginalObjectId = TransformOriginalObjectId;
            destination.TransformTargetObjectId = TransformTargetObjectId;
            destination.Team = Team;
            destination.relationTeam = relationTeam;
            destination.OwnerSlotIndex = OwnerSlotIndex;
            destination.OwnerStableId = OwnerStableId;
            destination.RelationOwnerSlotIndex = RelationOwnerSlotIndex;
            destination.SpawnerSlotIndex = SpawnerSlotIndex;
            destination.GrabbedBy = GrabbedBy;
            destination.linkState = linkState;
            destination.targetSlotIndex = targetSlotIndex;
            destination.CaughtSlotIndex = CaughtSlotIndex;
            destination.CatcherSlotIndex = CatcherSlotIndex;
            destination.HeldWeaponStableId = HeldWeaponStableId;
            destination.ThrowFrameGuard = ThrowFrameGuard;
            destination.ReleaseTick = ReleaseTick;
            destination.CaughtDuration = CaughtDuration;
            destination.PickupCount = PickupCount;
            destination.CaughtFrontFlag = CaughtFrontFlag;
            destination.CatchingStateTU = CatchingStateTU;
            destination.JumpAttackLock = JumpAttackLock;
            destination.AnimCounter = AnimCounter;
            destination.AnimSub = AnimSub;
            destination.LateSpecialTargetX = LateSpecialTargetX;
            destination.LateSpecialTargetZ = LateSpecialTargetZ;
            Array.Copy(InputHistory, destination.InputHistory, InputHistory.Length);
            destination.CdAttack = CdAttack;
            destination.CdJump = CdJump;
            destination.CdDefend = CdDefend;
            destination.CdDefendLock = CdDefendLock;
            destination.CdRight = CdRight;
            destination.CdLeft = CdLeft;
            destination.CdUp = CdUp;
            destination.CdDown = CdDown;
            destination.ComboDra = ComboDra;
            destination.ComboDla = ComboDla;
            destination.ComboDua = ComboDua;
            destination.ComboDda = ComboDda;
            destination.ComboDrj = ComboDrj;
            destination.ComboDlj = ComboDlj;
            destination.ComboDuj = ComboDuj;
            destination.ComboDdj = ComboDdj;
            destination.ComboDja = ComboDja;
            destination.PrevUp = PrevUp;
            destination.PrevDown = PrevDown;
            destination.PrevLeft = PrevLeft;
            destination.PrevRight = PrevRight;
            destination.PrevJump = PrevJump;
            destination.PrevDefend = PrevDefend;
            destination.PrevAttack = PrevAttack;
            destination.KeyUp = KeyUp;
            destination.KeyDown = KeyDown;
            destination.KeyLeft = KeyLeft;
            destination.KeyRight = KeyRight;
            destination.KeyAttack = KeyAttack;
            destination.KeyJump = KeyJump;
            destination.KeyDefend = KeyDefend;
            destination.HolderStableId = HolderStableId;
            destination.HolderCopySlotIndex = HolderCopySlotIndex;
            destination.PickerStableId = PickerStableId;
            destination.TrackerFlag = TrackerFlag;
            destination.AiControlled = AiControlled;
            destination.X = X;
            destination.Y = Y;
            destination.Z = Z;
            destination.xInt = xInt;
            destination.yInt = yInt;
            destination.zInt = zInt;
            destination.vx = vx;
            destination.Vy = Vy;
            destination.Vz = Vz;
            destination.facingLeft = facingLeft;
            destination.frame = frame;
            destination.frameState = frameState;
            destination.hitStop = hitStop;
            destination.SpriteX = SpriteX;
            destination.SpriteY = SpriteY;
            destination.SpriteZ = SpriteZ;
            destination.Type3VisualZOffset = Type3VisualZOffset;
            destination.RenderOffsetX = RenderOffsetX;
            destination.Zz = Zz;
            destination.XBoundPositive = XBoundPositive;
            destination.XBoundNegative = XBoundNegative;
            destination.ZBoundPositive = ZBoundPositive;
            destination.ZBoundNegative = ZBoundNegative;
            destination.PrevFrame2 = PrevFrame2;
            destination.FirstPresentationTick = FirstPresentationTick;
            destination.SpawnSemantic = SpawnSemantic;
            destination.SuppressFrameTickUntilTick = SuppressFrameTickUntilTick;
            destination.SuppressLateFrameTickUntilTick = SuppressLateFrameTickUntilTick;
            destination.SuppressPostInteractionUntilTick = SuppressPostInteractionUntilTick;
            destination.SuppressObjectInteractionUntilTick = SuppressObjectInteractionUntilTick;
            destination.SuppressPreInteractionUntilTick = SuppressPreInteractionUntilTick;
            destination.SuppressCollisionCandidateUntilTick = SuppressCollisionCandidateUntilTick;
            destination.RenderPicOffset = RenderPicOffset;
            destination.WaitCounter = WaitCounter;
            destination.FrameWaitCounter = FrameWaitCounter;
            destination.NextFrame = NextFrame;
            destination.AttackingCounter = AttackingCounter;
            destination.FrameDelay = FrameDelay;
            destination.KnockbackVx = KnockbackVx;
            destination.KnockbackVy = KnockbackVy;
            destination.KnockbackVz = KnockbackVz;
            destination.ShakeTimer = ShakeTimer;
            destination.AttackExempt = AttackExempt;
            destination.HitStateCount = HitStateCount;
            destination.Fall = Fall;
            destination.Bdefend = Bdefend;
            destination.HitCount = HitCount;
            destination.HitConfirmEa = HitConfirmEa;
            destination.HitConfirm2 = HitConfirm2;
            destination.HealTimer = HealTimer;
            destination.CatchTimer = CatchTimer;
            destination.killCount = killCount;
            destination.ComboCountVic = ComboCountVic;
            destination.ComboCountAtk = ComboCountAtk;
            destination.KillStat = KillStat;
            destination.Unk328 = Unk328;
            destination.Unk32C = Unk32C;
            destination.Unk330 = Unk330;
            destination.Unk334 = Unk334;
            destination.Unk338 = Unk338;
            destination.Unk344 = Unk344;
            destination.Unk360 = Unk360;
            destination.Unk3FC = Unk3FC;
            destination.Unk400 = Unk400;
            destination.ShotCount = ShotCount;
            destination.WeaponCount = WeaponCount;
            destination.FallDamageDiv = FallDamageDiv;
            destination.WeaponFlightCounter = WeaponFlightCounter;
            destination.WeaponDropHurt = WeaponDropHurt;
            destination.WeaponState = WeaponState;
            destination.Blink = Blink;
            destination.HitCandidateCount = HitCandidateCount;
            destination.HitCandidateNearestDistance = HitCandidateNearestDistance;
            destination.HitCandidateKind1Distance = HitCandidateKind1Distance;
            destination.HitCandidateExtraDistance = HitCandidateExtraDistance;
            destination.TransientMp = TransientMp;
            destination.TransientMp2 = TransientMp2;
            destination.TransientMp3 = TransientMp3;
            destination.TransientMp4 = TransientMp4;
            destination.OidMergeDormant = OidMergeDormant;
            destination.hp = hp;
            destination.hpBound = hpBound;
            destination.hp3 = hp3;
            destination.HPOrig = HPOrig;
            destination.HP2Orig = HP2Orig;
            destination.RespawnCount = RespawnCount;
            destination.HPLost = HPLost;
            destination.MP = MP;
            destination.MPMax = MPMax;
            destination.pp = pp;
            destination.PPMax = PPMax;
            destination.PPBound = PPBound;
            destination.PpDisplay = PpDisplay;
            return true;
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
            FrameState = 0;
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
