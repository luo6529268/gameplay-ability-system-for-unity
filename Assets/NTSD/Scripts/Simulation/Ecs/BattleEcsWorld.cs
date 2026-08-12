using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public readonly struct BattleEcsCapacityProfile : IEquatable<BattleEcsCapacityProfile>
    {
        public BattleEcsCapacityProfile(BattleRuntimeProfile runtimeProfile, int slotCapacity)
        {
            if (slotCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));

            RuntimeProfile = runtimeProfile;
            SlotCapacity = slotCapacity;
        }

        public BattleRuntimeProfile RuntimeProfile { get; }
        public int SlotCapacity { get; }

        public bool Equals(BattleEcsCapacityProfile other)
        {
            return RuntimeProfile == other.RuntimeProfile &&
                   SlotCapacity == other.SlotCapacity;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleEcsCapacityProfile other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)RuntimeProfile * 397) ^ SlotCapacity;
            }
        }
    }

    [Flags]
    public enum BattleEcsMembership : ushort
    {
        None = 0,
        Claimed = 1 << 0,
        Active = 1 << 1,
        PendingDestroy = 1 << 2,
        Dormant = 1 << 3,
        Character = 1 << 4,
        Weapon = 1 << 5,
        Projectile = 1 << 6,
        Effect = 1 << 7,
        HasBody = 1 << 8,
        HasItr = 1 << 9,
        HasAi = 1 << 10,
        HasHolder = 1 << 11,
    }

    public enum BattleEcsShadowMismatchKind : byte
    {
        None = 0,
        OccupancyEpoch = 1,
        Claimed = 2,
        Generation = 3,
        EntityReference = 4,
        Membership = 5,
        Identity = 6,
        Motion = 7,
        Frame = 8,
        Vital = 9,
        Input = 10,
        Link = 11,
        RuntimeFingerprint = 12,
        CaptureException = 13,
    }

    public readonly struct BattleEcsShadowEntityView
    {
        internal BattleEcsShadowEntityView(
            RuntimeEntityHandle handle,
            int stableId,
            int objectId,
            int dataObjectType,
            int team,
            int x,
            int y,
            int z,
            double vx,
            double vy,
            double vz,
            int frame,
            int state,
            int hp,
            int pp,
            int linkState,
            int targetSlot,
            BattleEcsMembership membership)
        {
            Handle = handle;
            StableId = stableId;
            ObjectId = objectId;
            DataObjectType = dataObjectType;
            Team = team;
            X = x;
            Y = y;
            Z = z;
            Vx = vx;
            Vy = vy;
            Vz = vz;
            Frame = frame;
            State = state;
            Hp = hp;
            Pp = pp;
            LinkState = linkState;
            TargetSlot = targetSlot;
            Membership = membership;
        }

        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int ObjectId { get; }
        public int DataObjectType { get; }
        public int Team { get; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public double Vx { get; }
        public double Vy { get; }
        public double Vz { get; }
        public int Frame { get; }
        public int State { get; }
        public int Hp { get; }
        public int Pp { get; }
        public int LinkState { get; }
        public int TargetSlot { get; }
        public BattleEcsMembership Membership { get; }
    }

    internal readonly struct BattleEcsOptionalLink
    {
        public BattleEcsOptionalLink(
            int linkState,
            int holderStableId,
            int targetSlot,
            int caughtSlot,
            int catcherSlot)
        {
            LinkState = linkState;
            HolderStableId = holderStableId;
            TargetSlot = targetSlot;
            CaughtSlot = caughtSlot;
            CatcherSlot = catcherSlot;
        }

        public int LinkState { get; }
        public int HolderStableId { get; }
        public int TargetSlot { get; }
        public int CaughtSlot { get; }
        public int CatcherSlot { get; }
    }

    internal sealed class BattleEcsIdentityStore
    {
        public BattleEcsIdentityStore(int capacity)
        {
            Generation = new uint[capacity];
            StableId = new int[capacity];
            ObjectId = new int[capacity];
            ObjType = new int[capacity];
            EntityType = new int[capacity];
            Team = new int[capacity];
            RelationTeam = new int[capacity];
            OwnerSlot = new int[capacity];
            OwnerStableId = new int[capacity];
            RelationOwnerSlot = new int[capacity];
            SpawnerSlot = new int[capacity];
        }

        internal readonly uint[] Generation;
        internal readonly int[] StableId;
        internal readonly int[] ObjectId;
        internal readonly int[] ObjType;
        internal readonly int[] EntityType;
        internal readonly int[] Team;
        internal readonly int[] RelationTeam;
        internal readonly int[] OwnerSlot;
        internal readonly int[] OwnerStableId;
        internal readonly int[] RelationOwnerSlot;
        internal readonly int[] SpawnerSlot;
    }

    internal sealed class BattleEcsMotionStore
    {
        public BattleEcsMotionStore(int capacity)
        {
            X = new double[capacity];
            Y = new double[capacity];
            Z = new double[capacity];
            XInt = new int[capacity];
            YInt = new int[capacity];
            ZInt = new int[capacity];
            Vx = new double[capacity];
            Vy = new double[capacity];
            Vz = new double[capacity];
            Facing = new byte[capacity];
            Bounds = new byte[capacity];
        }

        internal readonly double[] X;
        internal readonly double[] Y;
        internal readonly double[] Z;
        internal readonly int[] XInt;
        internal readonly int[] YInt;
        internal readonly int[] ZInt;
        internal readonly double[] Vx;
        internal readonly double[] Vy;
        internal readonly double[] Vz;
        internal readonly byte[] Facing;
        internal readonly byte[] Bounds;
    }

    internal sealed class BattleEcsFrameStore
    {
        public BattleEcsFrameStore(int capacity)
        {
            Frame = new int[capacity];
            State = new int[capacity];
            PrevFrame = new int[capacity];
            WaitCounter = new int[capacity];
            FrameWaitCounter = new int[capacity];
            NextFrame = new int[capacity];
            FrameDelay = new int[capacity];
            HitStop = new int[capacity];
            AttackingCounter = new int[capacity];
            FirstPresentationTick = new int[capacity];
            SpawnSemantic = new int[capacity];
            SuppressFrameTickUntilTick = new int[capacity];
            SuppressLateFrameTickUntilTick = new int[capacity];
            SuppressPostInteractionUntilTick = new int[capacity];
            SuppressObjectInteractionUntilTick = new int[capacity];
            SuppressPreInteractionUntilTick = new int[capacity];
            SuppressCollisionCandidateUntilTick = new int[capacity];
        }

        internal readonly int[] Frame;
        internal readonly int[] State;
        internal readonly int[] PrevFrame;
        internal readonly int[] WaitCounter;
        internal readonly int[] FrameWaitCounter;
        internal readonly int[] NextFrame;
        internal readonly int[] FrameDelay;
        internal readonly int[] HitStop;
        internal readonly int[] AttackingCounter;
        internal readonly int[] FirstPresentationTick;
        internal readonly int[] SpawnSemantic;
        internal readonly int[] SuppressFrameTickUntilTick;
        internal readonly int[] SuppressLateFrameTickUntilTick;
        internal readonly int[] SuppressPostInteractionUntilTick;
        internal readonly int[] SuppressObjectInteractionUntilTick;
        internal readonly int[] SuppressPreInteractionUntilTick;
        internal readonly int[] SuppressCollisionCandidateUntilTick;
    }

    internal sealed class BattleEcsVitalStore
    {
        public BattleEcsVitalStore(int capacity)
        {
            Hp = new int[capacity];
            HpBound = new int[capacity];
            Hp3 = new int[capacity];
            Mp = new int[capacity];
            MpMax = new int[capacity];
            Pp = new int[capacity];
            PpMax = new int[capacity];
            PpBound = new int[capacity];
            Fall = new int[capacity];
            Bdefend = new int[capacity];
            HitCount = new int[capacity];
            KillCount = new int[capacity];
            ComboCountVictim = new int[capacity];
            ComboCountAttacker = new int[capacity];
            KillStat = new int[capacity];
            DamageLost = new int[capacity];
        }

        internal readonly int[] Hp;
        internal readonly int[] HpBound;
        internal readonly int[] Hp3;
        internal readonly int[] Mp;
        internal readonly int[] MpMax;
        internal readonly int[] Pp;
        internal readonly int[] PpMax;
        internal readonly int[] PpBound;
        internal readonly int[] Fall;
        internal readonly int[] Bdefend;
        internal readonly int[] HitCount;
        internal readonly int[] KillCount;
        internal readonly int[] ComboCountVictim;
        internal readonly int[] ComboCountAttacker;
        internal readonly int[] KillStat;
        internal readonly int[] DamageLost;
    }

    internal sealed class BattleEcsInputStore
    {
        public BattleEcsInputStore(int capacity)
        {
            Held = new byte[capacity];
            Previous = new byte[capacity];
            Cooldown = new ulong[capacity];
            Combo = new ulong[capacity];
            ComboDja = new byte[capacity];
            History0 = new int[capacity];
            History1 = new int[capacity];
            History2 = new int[capacity];
            History3 = new int[capacity];
            History4 = new int[capacity];
            History5 = new int[capacity];
        }

        internal readonly byte[] Held;
        internal readonly byte[] Previous;
        internal readonly ulong[] Cooldown;
        internal readonly ulong[] Combo;
        internal readonly byte[] ComboDja;
        internal readonly int[] History0;
        internal readonly int[] History1;
        internal readonly int[] History2;
        internal readonly int[] History3;
        internal readonly int[] History4;
        internal readonly int[] History5;
    }

    internal sealed class BattleEcsLinkStore
    {
        public BattleEcsLinkStore(int capacity)
        {
            LinkState = new int[capacity];
            HolderStableId = new int[capacity];
            HolderCopySlot = new int[capacity];
            TargetSlot = new int[capacity];
            CaughtSlot = new int[capacity];
            CatcherSlot = new int[capacity];
            HeldWeaponStableId = new int[capacity];
            PickerStableId = new int[capacity];
            GrabbedBy = new int[capacity];
            TrackerFlag = new int[capacity];
        }

        internal readonly int[] LinkState;
        internal readonly int[] HolderStableId;
        internal readonly int[] HolderCopySlot;
        internal readonly int[] TargetSlot;
        internal readonly int[] CaughtSlot;
        internal readonly int[] CatcherSlot;
        internal readonly int[] HeldWeaponStableId;
        internal readonly int[] PickerStableId;
        internal readonly int[] GrabbedBy;
        internal readonly int[] TrackerFlag;
    }

    internal sealed class BattleEcsWorld
    {
        private readonly BattleSlotBitSet claimed;
        private readonly BattleSlotBitSet active;
        private readonly BattleSlotBitSet pendingDestroy;
        private readonly BattleSlotBitSet dormant;
        private readonly BattleSlotBitSet characters;
        private readonly BattleSlotBitSet weapons;
        private readonly BattleSlotBitSet projectiles;
        private readonly BattleSlotBitSet effects;
        private readonly BattleSlotBitSet hasBody;
        private readonly BattleSlotBitSet hasItr;
        private readonly BattleSlotBitSet hasAi;
        private readonly BattleSlotBitSet hasHolder;
        private readonly BattleRuntimeFingerprint[] runtimeFingerprints;

        public BattleEcsWorld(BattleEcsCapacityProfile capacityProfile)
        {
            CapacityProfile = capacityProfile;
            int capacity = capacityProfile.SlotCapacity;
            Identity = new BattleEcsIdentityStore(capacity);
            Motion = new BattleEcsMotionStore(capacity);
            Frame = new BattleEcsFrameStore(capacity);
            Vital = new BattleEcsVitalStore(capacity);
            Input = new BattleEcsInputStore(capacity);
            Links = new BattleEcsLinkStore(capacity);
            claimed = new BattleSlotBitSet(capacity);
            active = new BattleSlotBitSet(capacity);
            pendingDestroy = new BattleSlotBitSet(capacity);
            dormant = new BattleSlotBitSet(capacity);
            characters = new BattleSlotBitSet(capacity);
            weapons = new BattleSlotBitSet(capacity);
            projectiles = new BattleSlotBitSet(capacity);
            effects = new BattleSlotBitSet(capacity);
            hasBody = new BattleSlotBitSet(capacity);
            hasItr = new BattleSlotBitSet(capacity);
            hasAi = new BattleSlotBitSet(capacity);
            hasHolder = new BattleSlotBitSet(capacity);
            OptionalLinks = new BattleSparseSet<BattleEcsOptionalLink>(capacity, capacity);
            runtimeFingerprints = new BattleRuntimeFingerprint[capacity];
        }

        public BattleEcsCapacityProfile CapacityProfile { get; }
        public int CapturedTick { get; private set; } = -1;
        public ulong CapturedOccupancyEpoch { get; private set; }
        public int ClaimedCount => claimed.Count;

        internal BattleEcsIdentityStore Identity { get; }
        internal BattleEcsMotionStore Motion { get; }
        internal BattleEcsFrameStore Frame { get; }
        internal BattleEcsVitalStore Vital { get; }
        internal BattleEcsInputStore Input { get; }
        internal BattleEcsLinkStore Links { get; }
        internal BattleSparseSet<BattleEcsOptionalLink> OptionalLinks { get; }

        public void BeginCapture(int tickIndex, ulong occupancyEpoch)
        {
            CapturedTick = tickIndex;
            CapturedOccupancyEpoch = occupancyEpoch;
            claimed.ClearAll();
            active.ClearAll();
            pendingDestroy.ClearAll();
            dormant.ClearAll();
            characters.ClearAll();
            weapons.ClearAll();
            projectiles.ClearAll();
            effects.ClearAll();
            hasBody.ClearAll();
            hasItr.ClearAll();
            hasAi.ClearAll();
            hasHolder.ClearAll();
            OptionalLinks.Clear();
        }

        public void CaptureSlot(
            int slot,
            bool isClaimed,
            uint generation,
            LF2Entity entity)
        {
            ValidateSlot(slot);
            Identity.Generation[slot] = generation;
            if (!isClaimed || entity == null || entity.Runtime == null)
            {
                ClearSlot(slot);
                Identity.Generation[slot] = generation;
                return;
            }

            NTSDEntityRuntime runtime = entity.Runtime;
            LF2FrameData frame = entity.Frame?.D;
            int dataObjectType = entity.GetCurrentDataObjectTypeForSimulation();

            SetMembership(slot, BuildCanonicalMembership(runtime, frame, dataObjectType));

            CaptureIdentity(slot, runtime, dataObjectType);
            CaptureMotion(slot, runtime);
            CaptureFrame(slot, runtime, frame);
            CaptureVital(slot, runtime);
            CaptureInput(slot, runtime);
            CaptureLinks(slot, runtime);
            runtimeFingerprints[slot] = BattleRuntimeFingerprint.Compute(runtime);

            if (runtime.LinkState != 0)
            {
                var link = new BattleEcsOptionalLink(
                    runtime.LinkState,
                    runtime.HolderStableId,
                    runtime.TargetSlotIndex,
                    runtime.CaughtSlotIndex,
                    runtime.CatcherSlotIndex);
                OptionalLinks.AddOrSet(slot, link);
            }
        }

        public bool TryGetEntityView(int slot, out BattleEcsShadowEntityView view)
        {
            if ((uint)slot >= (uint)CapacityProfile.SlotCapacity || !claimed.Contains(slot))
            {
                view = default;
                return false;
            }

            view = new BattleEcsShadowEntityView(
                new RuntimeEntityHandle(slot, Identity.Generation[slot]),
                Identity.StableId[slot],
                Identity.ObjectId[slot],
                Identity.EntityType[slot],
                Identity.Team[slot],
                Motion.XInt[slot],
                Motion.YInt[slot],
                Motion.ZInt[slot],
                Motion.Vx[slot],
                Motion.Vy[slot],
                Motion.Vz[slot],
                Frame.Frame[slot],
                Frame.State[slot],
                Vital.Hp[slot],
                Vital.Pp[slot],
                Links.LinkState[slot],
                Links.TargetSlot[slot],
                GetMembership(slot));
            return true;
        }

        public BattleEcsMembership GetMembership(int slot)
        {
            if ((uint)slot >= (uint)CapacityProfile.SlotCapacity)
                return BattleEcsMembership.None;

            BattleEcsMembership membership = BattleEcsMembership.None;
            AddMembership(claimed, slot, BattleEcsMembership.Claimed, ref membership);
            AddMembership(active, slot, BattleEcsMembership.Active, ref membership);
            AddMembership(pendingDestroy, slot, BattleEcsMembership.PendingDestroy, ref membership);
            AddMembership(dormant, slot, BattleEcsMembership.Dormant, ref membership);
            AddMembership(characters, slot, BattleEcsMembership.Character, ref membership);
            AddMembership(weapons, slot, BattleEcsMembership.Weapon, ref membership);
            AddMembership(projectiles, slot, BattleEcsMembership.Projectile, ref membership);
            AddMembership(effects, slot, BattleEcsMembership.Effect, ref membership);
            AddMembership(hasBody, slot, BattleEcsMembership.HasBody, ref membership);
            AddMembership(hasItr, slot, BattleEcsMembership.HasItr, ref membership);
            AddMembership(hasAi, slot, BattleEcsMembership.HasAi, ref membership);
            AddMembership(hasHolder, slot, BattleEcsMembership.HasHolder, ref membership);
            return membership;
        }

        public int FindNextActiveSlot(int startSlot)
        {
            return active.FindNextSet(startSlot);
        }

        internal BattleRuntimeFingerprint GetRuntimeFingerprint(int slot)
        {
            ValidateSlot(slot);
            return runtimeFingerprints[slot];
        }

        internal bool MatchesCanonicalSlot(
            int slot,
            bool isClaimed,
            uint generation,
            LF2Entity entity,
            out BattleEcsShadowMismatchKind mismatchKind)
        {
            ValidateSlot(slot);
            bool shadowClaimed = claimed.Contains(slot);
            if (shadowClaimed != isClaimed)
                return Mismatch(BattleEcsShadowMismatchKind.Claimed, out mismatchKind);
            if (Identity.Generation[slot] != generation)
                return Mismatch(BattleEcsShadowMismatchKind.Generation, out mismatchKind);
            if (!isClaimed)
            {
                mismatchKind = BattleEcsShadowMismatchKind.None;
                return true;
            }

            NTSDEntityRuntime runtime = entity?.Runtime;
            if (runtime == null)
                return Mismatch(BattleEcsShadowMismatchKind.EntityReference, out mismatchKind);

            LF2FrameData frame = entity.Frame?.D;
            int dataObjectType = entity.GetCurrentDataObjectTypeForSimulation();
            if (GetMembership(slot) != BuildCanonicalMembership(runtime, frame, dataObjectType))
                return Mismatch(BattleEcsShadowMismatchKind.Membership, out mismatchKind);
            if (!MatchesIdentity(slot, runtime, dataObjectType))
                return Mismatch(BattleEcsShadowMismatchKind.Identity, out mismatchKind);
            if (!MatchesMotion(slot, runtime))
                return Mismatch(BattleEcsShadowMismatchKind.Motion, out mismatchKind);
            if (!MatchesFrame(slot, runtime, frame))
                return Mismatch(BattleEcsShadowMismatchKind.Frame, out mismatchKind);
            if (!MatchesVital(slot, runtime))
                return Mismatch(BattleEcsShadowMismatchKind.Vital, out mismatchKind);
            if (!MatchesInput(slot, runtime))
                return Mismatch(BattleEcsShadowMismatchKind.Input, out mismatchKind);
            if (!MatchesLinks(slot, runtime))
                return Mismatch(BattleEcsShadowMismatchKind.Link, out mismatchKind);
            if (!runtimeFingerprints[slot].Equals(BattleRuntimeFingerprint.Compute(runtime)))
                return Mismatch(BattleEcsShadowMismatchKind.RuntimeFingerprint, out mismatchKind);

            mismatchKind = BattleEcsShadowMismatchKind.None;
            return true;
        }

        private void CaptureIdentity(int slot, NTSDEntityRuntime runtime, int dataObjectType)
        {
            Identity.StableId[slot] = runtime.StableId;
            Identity.ObjectId[slot] = runtime.ObjectId;
            Identity.ObjType[slot] = runtime.ObjType;
            Identity.EntityType[slot] = dataObjectType;
            Identity.Team[slot] = runtime.Team;
            Identity.RelationTeam[slot] = runtime.RelationTeam;
            Identity.OwnerSlot[slot] = runtime.OwnerSlotIndex;
            Identity.OwnerStableId[slot] = runtime.OwnerStableId;
            Identity.RelationOwnerSlot[slot] = runtime.RelationOwnerSlotIndex;
            Identity.SpawnerSlot[slot] = runtime.SpawnerSlotIndex;
        }

        private void CaptureMotion(int slot, NTSDEntityRuntime runtime)
        {
            Motion.X[slot] = runtime.X;
            Motion.Y[slot] = runtime.Y;
            Motion.Z[slot] = runtime.Z;
            Motion.XInt[slot] = runtime.XInt;
            Motion.YInt[slot] = runtime.YInt;
            Motion.ZInt[slot] = runtime.ZInt;
            Motion.Vx[slot] = runtime.Vx;
            Motion.Vy[slot] = runtime.Vy;
            Motion.Vz[slot] = runtime.Vz;
            Motion.Facing[slot] = runtime.Dir == "left" ? (byte)1 : (byte)0;
            Motion.Bounds[slot] = PackBounds(runtime);
        }

        private void CaptureFrame(int slot, NTSDEntityRuntime runtime, LF2FrameData frame)
        {
            Frame.Frame[slot] = runtime.Frame;
            Frame.State[slot] = frame?.state ?? 0;
            Frame.PrevFrame[slot] = runtime.PrevFrame2;
            Frame.WaitCounter[slot] = runtime.WaitCounter;
            Frame.FrameWaitCounter[slot] = runtime.FrameWaitCounter;
            Frame.NextFrame[slot] = runtime.NextFrame;
            Frame.FrameDelay[slot] = runtime.FrameDelay;
            Frame.HitStop[slot] = runtime.HitStop;
            Frame.AttackingCounter[slot] = runtime.AttackingCounter;
            Frame.FirstPresentationTick[slot] = runtime.FirstPresentationTick;
            Frame.SpawnSemantic[slot] = runtime.SpawnSemantic;
            Frame.SuppressFrameTickUntilTick[slot] = runtime.SuppressFrameTickUntilTick;
            Frame.SuppressLateFrameTickUntilTick[slot] = runtime.SuppressLateFrameTickUntilTick;
            Frame.SuppressPostInteractionUntilTick[slot] = runtime.SuppressPostInteractionUntilTick;
            Frame.SuppressObjectInteractionUntilTick[slot] = runtime.SuppressObjectInteractionUntilTick;
            Frame.SuppressPreInteractionUntilTick[slot] = runtime.SuppressPreInteractionUntilTick;
            Frame.SuppressCollisionCandidateUntilTick[slot] = runtime.SuppressCollisionCandidateUntilTick;
        }

        private void CaptureVital(int slot, NTSDEntityRuntime runtime)
        {
            Vital.Hp[slot] = runtime.HP;
            Vital.HpBound[slot] = runtime.HPBound;
            Vital.Hp3[slot] = runtime.HP3;
            Vital.Mp[slot] = runtime.MP;
            Vital.MpMax[slot] = runtime.MPMax;
            Vital.Pp[slot] = runtime.PP;
            Vital.PpMax[slot] = runtime.PPMax;
            Vital.PpBound[slot] = runtime.PPBound;
            Vital.Fall[slot] = runtime.Fall;
            Vital.Bdefend[slot] = runtime.Bdefend;
            Vital.HitCount[slot] = runtime.HitCount;
            Vital.KillCount[slot] = runtime.KillCount;
            Vital.ComboCountVictim[slot] = runtime.ComboCountVic;
            Vital.ComboCountAttacker[slot] = runtime.ComboCountAtk;
            Vital.KillStat[slot] = runtime.KillStat;
            Vital.DamageLost[slot] = runtime.HPLost;
        }

        private void CaptureInput(int slot, NTSDEntityRuntime runtime)
        {
            Input.Held[slot] = PackInput(
                runtime.KeyUp,
                runtime.KeyDown,
                runtime.KeyLeft,
                runtime.KeyRight,
                runtime.KeyAttack,
                runtime.KeyJump,
                runtime.KeyDefend);
            Input.Previous[slot] = PackInput(
                runtime.PrevUp,
                runtime.PrevDown,
                runtime.PrevLeft,
                runtime.PrevRight,
                runtime.PrevAttack,
                runtime.PrevJump,
                runtime.PrevDefend);
            Input.Cooldown[slot] = PackBytes(
                runtime.CdAttack,
                runtime.CdJump,
                runtime.CdDefend,
                runtime.CdDefendLock,
                runtime.CdRight,
                runtime.CdLeft,
                runtime.CdUp,
                runtime.CdDown);
            Input.Combo[slot] = PackBytes(
                runtime.ComboDra,
                runtime.ComboDla,
                runtime.ComboDua,
                runtime.ComboDda,
                runtime.ComboDrj,
                runtime.ComboDlj,
                runtime.ComboDuj,
                runtime.ComboDdj);
            Input.ComboDja[slot] = runtime.ComboDja;

            int[] history = runtime.InputHistory;
            Input.History0[slot] = ReadHistory(history, 0);
            Input.History1[slot] = ReadHistory(history, 1);
            Input.History2[slot] = ReadHistory(history, 2);
            Input.History3[slot] = ReadHistory(history, 3);
            Input.History4[slot] = ReadHistory(history, 4);
            Input.History5[slot] = ReadHistory(history, 5);
        }

        private void CaptureLinks(int slot, NTSDEntityRuntime runtime)
        {
            Links.LinkState[slot] = runtime.LinkState;
            Links.HolderStableId[slot] = runtime.HolderStableId;
            Links.HolderCopySlot[slot] = runtime.HolderCopySlotIndex;
            Links.TargetSlot[slot] = runtime.TargetSlotIndex;
            Links.CaughtSlot[slot] = runtime.CaughtSlotIndex;
            Links.CatcherSlot[slot] = runtime.CatcherSlotIndex;
            Links.HeldWeaponStableId[slot] = runtime.HeldWeaponStableId;
            Links.PickerStableId[slot] = runtime.PickerStableId;
            Links.GrabbedBy[slot] = runtime.GrabbedBy;
            Links.TrackerFlag[slot] = runtime.TrackerFlag;
        }

        private void ClearSlot(int slot)
        {
            Identity.StableId[slot] = 0;
            Identity.ObjectId[slot] = 0;
            Identity.ObjType[slot] = 0;
            Identity.EntityType[slot] = 0;
            Identity.Team[slot] = 0;
            Identity.RelationTeam[slot] = 0;
            Identity.OwnerSlot[slot] = 0;
            Identity.OwnerStableId[slot] = 0;
            Identity.RelationOwnerSlot[slot] = 0;
            Identity.SpawnerSlot[slot] = 0;
            Motion.X[slot] = Motion.Y[slot] = Motion.Z[slot] = 0.0;
            Motion.XInt[slot] = Motion.YInt[slot] = Motion.ZInt[slot] = 0;
            Motion.Vx[slot] = Motion.Vy[slot] = Motion.Vz[slot] = 0.0;
            Motion.Facing[slot] = Motion.Bounds[slot] = 0;
            Frame.Frame[slot] = Frame.State[slot] = Frame.PrevFrame[slot] = 0;
            Frame.WaitCounter[slot] = Frame.FrameWaitCounter[slot] = 0;
            Frame.NextFrame[slot] = Frame.FrameDelay[slot] = Frame.HitStop[slot] = 0;
            Frame.AttackingCounter[slot] = Frame.FirstPresentationTick[slot] = 0;
            Frame.SpawnSemantic[slot] = 0;
            Frame.SuppressFrameTickUntilTick[slot] = 0;
            Frame.SuppressLateFrameTickUntilTick[slot] = 0;
            Frame.SuppressPostInteractionUntilTick[slot] = 0;
            Frame.SuppressObjectInteractionUntilTick[slot] = 0;
            Frame.SuppressPreInteractionUntilTick[slot] = 0;
            Frame.SuppressCollisionCandidateUntilTick[slot] = 0;
            Vital.Hp[slot] = Vital.HpBound[slot] = Vital.Hp3[slot] = 0;
            Vital.Mp[slot] = Vital.MpMax[slot] = Vital.Pp[slot] = 0;
            Vital.PpMax[slot] = Vital.PpBound[slot] = 0;
            Vital.Fall[slot] = Vital.Bdefend[slot] = Vital.HitCount[slot] = 0;
            Vital.KillCount[slot] = Vital.ComboCountVictim[slot] = 0;
            Vital.ComboCountAttacker[slot] = Vital.KillStat[slot] = 0;
            Vital.DamageLost[slot] = 0;
            Input.Held[slot] = Input.Previous[slot] = 0;
            Input.Cooldown[slot] = Input.Combo[slot] = 0;
            Input.ComboDja[slot] = 0;
            Input.History0[slot] = Input.History1[slot] = Input.History2[slot] = 0;
            Input.History3[slot] = Input.History4[slot] = Input.History5[slot] = 0;
            Links.LinkState[slot] = Links.HolderStableId[slot] = 0;
            Links.HolderCopySlot[slot] = Links.TargetSlot[slot] = 0;
            Links.CaughtSlot[slot] = Links.CatcherSlot[slot] = 0;
            Links.HeldWeaponStableId[slot] = Links.PickerStableId[slot] = 0;
            Links.GrabbedBy[slot] = Links.TrackerFlag[slot] = 0;
            runtimeFingerprints[slot] = default;
        }

        private static BattleEcsMembership BuildCanonicalMembership(
            NTSDEntityRuntime runtime,
            LF2FrameData frame,
            int dataObjectType)
        {
            BattleEcsMembership membership = BattleEcsMembership.Claimed;
            if (runtime.PendingFlushDestroy)
                membership |= BattleEcsMembership.PendingDestroy;
            else if (!runtime.OidMergeDormant)
                membership |= BattleEcsMembership.Active;
            if (runtime.OidMergeDormant)
                membership |= BattleEcsMembership.Dormant;

            switch ((LF2ObjectType)dataObjectType)
            {
                case LF2ObjectType.Character:
                    membership |= BattleEcsMembership.Character;
                    break;
                case LF2ObjectType.LightWeapon:
                case LF2ObjectType.HeavyWeapon:
                case LF2ObjectType.ThrowWeapon:
                case LF2ObjectType.Drink:
                    membership |= BattleEcsMembership.Weapon;
                    break;
                case LF2ObjectType.SpecialAttack:
                    membership |= BattleEcsMembership.Projectile;
                    break;
                default:
                    membership |= BattleEcsMembership.Effect;
                    break;
            }

            if (frame?.bodies != null && frame.bodies.Count != 0)
                membership |= BattleEcsMembership.HasBody;
            if (frame?.itrs != null && frame.itrs.Count != 0)
                membership |= BattleEcsMembership.HasItr;
            if (runtime.AiControlled)
                membership |= BattleEcsMembership.HasAi;
            if (runtime.LinkState < 0 || runtime.HolderStableId >= 0)
                membership |= BattleEcsMembership.HasHolder;
            return membership;
        }

        private void SetMembership(int slot, BattleEcsMembership membership)
        {
            SetMembership(claimed, slot, membership, BattleEcsMembership.Claimed);
            SetMembership(active, slot, membership, BattleEcsMembership.Active);
            SetMembership(pendingDestroy, slot, membership, BattleEcsMembership.PendingDestroy);
            SetMembership(dormant, slot, membership, BattleEcsMembership.Dormant);
            SetMembership(characters, slot, membership, BattleEcsMembership.Character);
            SetMembership(weapons, slot, membership, BattleEcsMembership.Weapon);
            SetMembership(projectiles, slot, membership, BattleEcsMembership.Projectile);
            SetMembership(effects, slot, membership, BattleEcsMembership.Effect);
            SetMembership(hasBody, slot, membership, BattleEcsMembership.HasBody);
            SetMembership(hasItr, slot, membership, BattleEcsMembership.HasItr);
            SetMembership(hasAi, slot, membership, BattleEcsMembership.HasAi);
            SetMembership(hasHolder, slot, membership, BattleEcsMembership.HasHolder);
        }

        private static void SetMembership(
            BattleSlotBitSet bitSet,
            int slot,
            BattleEcsMembership membership,
            BattleEcsMembership flag)
        {
            if ((membership & flag) != 0)
                bitSet.Set(slot);
            else
                bitSet.Clear(slot);
        }

        private bool MatchesIdentity(int slot, NTSDEntityRuntime runtime, int dataObjectType)
        {
            return Identity.StableId[slot] == runtime.StableId &&
                   Identity.ObjectId[slot] == runtime.ObjectId &&
                   Identity.ObjType[slot] == runtime.ObjType &&
                   Identity.EntityType[slot] == dataObjectType &&
                   Identity.Team[slot] == runtime.Team &&
                   Identity.RelationTeam[slot] == runtime.RelationTeam &&
                   Identity.OwnerSlot[slot] == runtime.OwnerSlotIndex &&
                   Identity.OwnerStableId[slot] == runtime.OwnerStableId &&
                   Identity.RelationOwnerSlot[slot] == runtime.RelationOwnerSlotIndex &&
                   Identity.SpawnerSlot[slot] == runtime.SpawnerSlotIndex;
        }

        private bool MatchesMotion(int slot, NTSDEntityRuntime runtime)
        {
            return Motion.X[slot] == runtime.X &&
                   Motion.Y[slot] == runtime.Y &&
                   Motion.Z[slot] == runtime.Z &&
                   Motion.XInt[slot] == runtime.XInt &&
                   Motion.YInt[slot] == runtime.YInt &&
                   Motion.ZInt[slot] == runtime.ZInt &&
                   Motion.Vx[slot] == runtime.Vx &&
                   Motion.Vy[slot] == runtime.Vy &&
                   Motion.Vz[slot] == runtime.Vz &&
                   Motion.Facing[slot] == (runtime.Dir == "left" ? (byte)1 : (byte)0) &&
                   Motion.Bounds[slot] == PackBounds(runtime);
        }

        private bool MatchesFrame(int slot, NTSDEntityRuntime runtime, LF2FrameData frame)
        {
            return Frame.Frame[slot] == runtime.Frame &&
                   Frame.State[slot] == (frame?.state ?? 0) &&
                   Frame.PrevFrame[slot] == runtime.PrevFrame2 &&
                   Frame.WaitCounter[slot] == runtime.WaitCounter &&
                   Frame.FrameWaitCounter[slot] == runtime.FrameWaitCounter &&
                   Frame.NextFrame[slot] == runtime.NextFrame &&
                   Frame.FrameDelay[slot] == runtime.FrameDelay &&
                   Frame.HitStop[slot] == runtime.HitStop &&
                   Frame.AttackingCounter[slot] == runtime.AttackingCounter &&
                   Frame.FirstPresentationTick[slot] == runtime.FirstPresentationTick &&
                   Frame.SpawnSemantic[slot] == runtime.SpawnSemantic &&
                   Frame.SuppressFrameTickUntilTick[slot] == runtime.SuppressFrameTickUntilTick &&
                   Frame.SuppressLateFrameTickUntilTick[slot] == runtime.SuppressLateFrameTickUntilTick &&
                   Frame.SuppressPostInteractionUntilTick[slot] == runtime.SuppressPostInteractionUntilTick &&
                   Frame.SuppressObjectInteractionUntilTick[slot] == runtime.SuppressObjectInteractionUntilTick &&
                   Frame.SuppressPreInteractionUntilTick[slot] == runtime.SuppressPreInteractionUntilTick &&
                   Frame.SuppressCollisionCandidateUntilTick[slot] == runtime.SuppressCollisionCandidateUntilTick;
        }

        private bool MatchesVital(int slot, NTSDEntityRuntime runtime)
        {
            return Vital.Hp[slot] == runtime.HP &&
                   Vital.HpBound[slot] == runtime.HPBound &&
                   Vital.Hp3[slot] == runtime.HP3 &&
                   Vital.Mp[slot] == runtime.MP &&
                   Vital.MpMax[slot] == runtime.MPMax &&
                   Vital.Pp[slot] == runtime.PP &&
                   Vital.PpMax[slot] == runtime.PPMax &&
                   Vital.PpBound[slot] == runtime.PPBound &&
                   Vital.Fall[slot] == runtime.Fall &&
                   Vital.Bdefend[slot] == runtime.Bdefend &&
                   Vital.HitCount[slot] == runtime.HitCount &&
                   Vital.KillCount[slot] == runtime.KillCount &&
                   Vital.ComboCountVictim[slot] == runtime.ComboCountVic &&
                   Vital.ComboCountAttacker[slot] == runtime.ComboCountAtk &&
                   Vital.KillStat[slot] == runtime.KillStat &&
                   Vital.DamageLost[slot] == runtime.HPLost;
        }

        private bool MatchesInput(int slot, NTSDEntityRuntime runtime)
        {
            return Input.Held[slot] == PackInput(
                       runtime.KeyUp, runtime.KeyDown, runtime.KeyLeft, runtime.KeyRight,
                       runtime.KeyAttack, runtime.KeyJump, runtime.KeyDefend) &&
                   Input.Previous[slot] == PackInput(
                       runtime.PrevUp, runtime.PrevDown, runtime.PrevLeft, runtime.PrevRight,
                       runtime.PrevAttack, runtime.PrevJump, runtime.PrevDefend) &&
                   Input.Cooldown[slot] == PackBytes(
                       runtime.CdAttack, runtime.CdJump, runtime.CdDefend, runtime.CdDefendLock,
                       runtime.CdRight, runtime.CdLeft, runtime.CdUp, runtime.CdDown) &&
                   Input.Combo[slot] == PackBytes(
                       runtime.ComboDra, runtime.ComboDla, runtime.ComboDua, runtime.ComboDda,
                       runtime.ComboDrj, runtime.ComboDlj, runtime.ComboDuj, runtime.ComboDdj) &&
                   Input.ComboDja[slot] == runtime.ComboDja &&
                   Input.History0[slot] == ReadHistory(runtime.InputHistory, 0) &&
                   Input.History1[slot] == ReadHistory(runtime.InputHistory, 1) &&
                   Input.History2[slot] == ReadHistory(runtime.InputHistory, 2) &&
                   Input.History3[slot] == ReadHistory(runtime.InputHistory, 3) &&
                   Input.History4[slot] == ReadHistory(runtime.InputHistory, 4) &&
                   Input.History5[slot] == ReadHistory(runtime.InputHistory, 5);
        }

        private bool MatchesLinks(int slot, NTSDEntityRuntime runtime)
        {
            if (Links.LinkState[slot] != runtime.LinkState ||
                Links.HolderStableId[slot] != runtime.HolderStableId ||
                Links.HolderCopySlot[slot] != runtime.HolderCopySlotIndex ||
                Links.TargetSlot[slot] != runtime.TargetSlotIndex ||
                Links.CaughtSlot[slot] != runtime.CaughtSlotIndex ||
                Links.CatcherSlot[slot] != runtime.CatcherSlotIndex ||
                Links.HeldWeaponStableId[slot] != runtime.HeldWeaponStableId ||
                Links.PickerStableId[slot] != runtime.PickerStableId ||
                Links.GrabbedBy[slot] != runtime.GrabbedBy ||
                Links.TrackerFlag[slot] != runtime.TrackerFlag)
            {
                return false;
            }

            bool expectedOptional = runtime.LinkState != 0;
            bool hasOptional = OptionalLinks.TryGet(slot, out BattleEcsOptionalLink link);
            return hasOptional == expectedOptional &&
                   (!expectedOptional ||
                    (link.LinkState == runtime.LinkState &&
                     link.HolderStableId == runtime.HolderStableId &&
                     link.TargetSlot == runtime.TargetSlotIndex &&
                     link.CaughtSlot == runtime.CaughtSlotIndex &&
                     link.CatcherSlot == runtime.CatcherSlotIndex));
        }

        private static bool Mismatch(
            BattleEcsShadowMismatchKind kind,
            out BattleEcsShadowMismatchKind mismatchKind)
        {
            mismatchKind = kind;
            return false;
        }

        private static void AddMembership(
            BattleSlotBitSet bitSet,
            int slot,
            BattleEcsMembership flag,
            ref BattleEcsMembership membership)
        {
            if (bitSet.Contains(slot))
                membership |= flag;
        }

        private static byte PackBounds(NTSDEntityRuntime runtime)
        {
            byte value = 0;
            if (runtime.XBoundPositive) value |= 1 << 0;
            if (runtime.XBoundNegative) value |= 1 << 1;
            if (runtime.ZBoundPositive) value |= 1 << 2;
            if (runtime.ZBoundNegative) value |= 1 << 3;
            return value;
        }

        private static byte PackInput(
            byte up,
            byte down,
            byte left,
            byte right,
            byte attack,
            byte jump,
            byte defend)
        {
            byte value = 0;
            if (up != 0) value |= 1 << 0;
            if (down != 0) value |= 1 << 1;
            if (left != 0) value |= 1 << 2;
            if (right != 0) value |= 1 << 3;
            if (attack != 0) value |= 1 << 4;
            if (jump != 0) value |= 1 << 5;
            if (defend != 0) value |= 1 << 6;
            return value;
        }

        private static ulong PackBytes(
            byte b0,
            byte b1,
            byte b2,
            byte b3,
            byte b4,
            byte b5,
            byte b6,
            byte b7)
        {
            return b0 |
                   ((ulong)b1 << 8) |
                   ((ulong)b2 << 16) |
                   ((ulong)b3 << 24) |
                   ((ulong)b4 << 32) |
                   ((ulong)b5 << 40) |
                   ((ulong)b6 << 48) |
                   ((ulong)b7 << 56);
        }

        private static int ReadHistory(int[] history, int index)
        {
            return history != null && (uint)index < (uint)history.Length
                ? history[index]
                : 0;
        }

        private void ValidateSlot(int slot)
        {
            if ((uint)slot >= (uint)CapacityProfile.SlotCapacity)
                throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }

    internal readonly struct BattleRuntimeFingerprint : IEquatable<BattleRuntimeFingerprint>
    {
        private BattleRuntimeFingerprint(ulong a, ulong b, ulong c, ulong d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public ulong A { get; }
        public ulong B { get; }
        public ulong C { get; }
        public ulong D { get; }

        public bool Equals(BattleRuntimeFingerprint other)
        {
            return A == other.A && B == other.B && C == other.C && D == other.D;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleRuntimeFingerprint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)A * 397) ^ (int)B ^ (int)C ^ (int)D;
            }
        }

        public static BattleRuntimeFingerprint Compute(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return default;

            var hash = new FingerprintBuilder(0);
            hash.Add(runtime.SlotIndex); hash.Add(runtime.StableId); hash.Add(runtime.ObjectId);
            hash.Add(runtime.ObjType); hash.Add(runtime.EntityType);
            hash.Add(runtime.TransformOriginalObjectId); hash.Add(runtime.TransformTargetObjectId);
            hash.Add(runtime.Team); hash.Add(runtime.RelationTeam);
            hash.Add(runtime.OwnerSlotIndex); hash.Add(runtime.OwnerStableId);
            hash.Add(runtime.RelationOwnerSlotIndex); hash.Add(runtime.SpawnerSlotIndex);
            hash.Add(runtime.GrabbedBy); hash.Add(runtime.LinkState); hash.Add(runtime.TargetSlotIndex);
            hash.Add(runtime.CaughtSlotIndex); hash.Add(runtime.CatcherSlotIndex);
            hash.Add(runtime.HeldWeaponStableId); hash.Add(runtime.ThrowFrameGuard);
            hash.Add(runtime.ReleaseTick); hash.Add(runtime.CaughtDuration); hash.Add(runtime.PickupCount);
            hash.Add(runtime.CaughtFrontFlag); hash.Add(runtime.CatchingStateTU);
            hash.Add(runtime.JumpAttackLock); hash.Add(runtime.AnimCounter); hash.Add(runtime.AnimSub);
            hash.Add(runtime.LateSpecialTargetX); hash.Add(runtime.LateSpecialTargetZ);
            hash.Add(runtime.InputHistory);
            hash.Add(runtime.CdAttack); hash.Add(runtime.CdJump); hash.Add(runtime.CdDefend);
            hash.Add(runtime.CdDefendLock); hash.Add(runtime.CdRight); hash.Add(runtime.CdLeft);
            hash.Add(runtime.CdUp); hash.Add(runtime.CdDown);
            hash.Add(runtime.ComboDra); hash.Add(runtime.ComboDla); hash.Add(runtime.ComboDua);
            hash.Add(runtime.ComboDda); hash.Add(runtime.ComboDrj); hash.Add(runtime.ComboDlj);
            hash.Add(runtime.ComboDuj); hash.Add(runtime.ComboDdj); hash.Add(runtime.ComboDja);
            hash.Add(runtime.PrevUp); hash.Add(runtime.PrevDown); hash.Add(runtime.PrevLeft);
            hash.Add(runtime.PrevRight); hash.Add(runtime.PrevJump); hash.Add(runtime.PrevDefend);
            hash.Add(runtime.PrevAttack); hash.Add(runtime.KeyUp); hash.Add(runtime.KeyDown);
            hash.Add(runtime.KeyLeft); hash.Add(runtime.KeyRight); hash.Add(runtime.KeyAttack);
            hash.Add(runtime.KeyJump); hash.Add(runtime.KeyDefend);
            hash.Add(runtime.HolderStableId); hash.Add(runtime.HolderCopySlotIndex);
            hash.Add(runtime.PickerStableId); hash.Add(runtime.TrackerFlag); hash.Add(runtime.AiControlled);
            hash.Add(runtime.X); hash.Add(runtime.Y); hash.Add(runtime.Z);
            hash.Add(runtime.XInt); hash.Add(runtime.YInt); hash.Add(runtime.ZInt);
            hash.Add(runtime.Vx); hash.Add(runtime.Vy); hash.Add(runtime.Vz);
            hash.Add(runtime.SpriteX); hash.Add(runtime.SpriteY); hash.Add(runtime.SpriteZ);
            hash.Add(runtime.Type3VisualZOffset); hash.Add(runtime.RenderOffsetX);
            hash.Add(runtime.Dir); hash.Add(runtime.Zz);
            hash.Add(runtime.XBoundPositive); hash.Add(runtime.XBoundNegative);
            hash.Add(runtime.ZBoundPositive); hash.Add(runtime.ZBoundNegative);
            hash.Add(runtime.Frame); hash.Add(runtime.PrevFrame2); hash.Add(runtime.FirstPresentationTick);
            hash.Add(runtime.SpawnSemantic); hash.Add(runtime.SuppressFrameTickUntilTick);
            hash.Add(runtime.SuppressLateFrameTickUntilTick); hash.Add(runtime.SuppressPostInteractionUntilTick);
            hash.Add(runtime.SuppressObjectInteractionUntilTick); hash.Add(runtime.SuppressPreInteractionUntilTick);
            hash.Add(runtime.SuppressCollisionCandidateUntilTick); hash.Add(runtime.RenderPicOffset);
            hash.Add(runtime.WaitCounter); hash.Add(runtime.FrameWaitCounter); hash.Add(runtime.NextFrame);
            hash.Add(runtime.AttackingCounter); hash.Add(runtime.FrameDelay); hash.Add(runtime.HitStop);
            hash.Add(runtime.KnockbackVx); hash.Add(runtime.KnockbackVy); hash.Add(runtime.KnockbackVz);
            hash.Add(runtime.ShakeTimer); hash.Add(runtime.AttackExempt); hash.Add(runtime.HitStateCount);
            hash.Add(runtime.Fall); hash.Add(runtime.Bdefend); hash.Add(runtime.HitCount);
            hash.Add(runtime.HitConfirmEa); hash.Add(runtime.HitConfirm2); hash.Add(runtime.HealTimer);
            hash.Add(runtime.CatchTimer); hash.Add(runtime.KillCount); hash.Add(runtime.ComboCountVic);
            hash.Add(runtime.ComboCountAtk); hash.Add(runtime.KillStat);
            hash.Add(runtime.Unk328); hash.Add(runtime.Unk32C); hash.Add(runtime.Unk330);
            hash.Add(runtime.Unk334); hash.Add(runtime.Unk338); hash.Add(runtime.Unk344);
            hash.Add(runtime.Unk360); hash.Add(runtime.Unk3FC); hash.Add(runtime.Unk400);
            hash.Add(runtime.ShotCount); hash.Add(runtime.WeaponCount); hash.Add(runtime.FallDamageDiv);
            hash.Add(runtime.WeaponFlightCounter); hash.Add(runtime.WeaponDropHurt);
            hash.Add(runtime.WeaponState); hash.Add(runtime.Blink); hash.Add(runtime.HitCandidateCount);
            hash.Add(runtime.HitCandidateNearestDistance); hash.Add(runtime.HitCandidateKind1Distance);
            hash.Add(runtime.HitCandidateExtraDistance); hash.Add(runtime.TransientMp);
            hash.Add(runtime.TransientMp2); hash.Add(runtime.TransientMp3); hash.Add(runtime.TransientMp4);
            hash.Add(runtime.OidMergeDormant); hash.Add(runtime.PendingFlushDestroy);
            hash.Add(runtime.HP); hash.Add(runtime.HPBound); hash.Add(runtime.HP3);
            hash.Add(runtime.HPOrig); hash.Add(runtime.HP2Orig); hash.Add(runtime.RespawnCount);
            hash.Add(runtime.HPLost); hash.Add(runtime.MP); hash.Add(runtime.MPMax);
            hash.Add(runtime.PP); hash.Add(runtime.PPMax); hash.Add(runtime.PPBound);
            hash.Add(runtime.PpDisplay);
            return hash.ToFingerprint();
        }

        private struct FingerprintBuilder
        {
            private ulong a;
            private ulong b;
            private ulong c;
            private ulong d;

            public FingerprintBuilder(int _)
            {
                a = 1469598103934665603UL;
                b = 1099511628211UL;
                c = 7809847782465536322UL;
                d = 9650029242287828579UL;
            }

            public void Add(bool value) => Add(value ? 1UL : 0UL);
            public void Add(byte value) => Add((ulong)value);
            public void Add(int value) => Add(unchecked((ulong)(long)value));
            public void Add(uint value) => Add((ulong)value);
            public void Add(float value) => Add(unchecked((ulong)(uint)BitConverter.SingleToInt32Bits(value)));
            public void Add(double value) => Add(unchecked((ulong)BitConverter.DoubleToInt64Bits(value)));

            public void Add(string value)
            {
                if (value == null)
                {
                    Add(ulong.MaxValue);
                    return;
                }

                Add(value.Length);
                for (int i = 0; i < value.Length; i++)
                    Add(value[i]);
            }

            public void Add(int[] values)
            {
                if (values == null)
                {
                    Add(ulong.MaxValue);
                    return;
                }

                Add(values.Length);
                for (int i = 0; i < values.Length; i++)
                    Add(values[i]);
            }

            private void Add(ulong value)
            {
                a = (a ^ value) * 1099511628211UL;
                b = (b + value + 0x9E3779B97F4A7C15UL) * 14029467366897019727UL;
                c ^= value + 0x517CC1B727220A95UL + (c << 6) + (c >> 2);
                d = ((d << 13) | (d >> 51)) ^ value;
                d *= 11400714785074694791UL;
            }

            public BattleRuntimeFingerprint ToFingerprint()
            {
                return new BattleRuntimeFingerprint(a, b, c, d);
            }
        }
    }
}
