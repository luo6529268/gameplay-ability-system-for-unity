using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Animation
{
    /// <summary>
    /// 战斗场景查询接口，对应 C++ release 在碰撞阶段读取 body 命中目标的查询能力。
    /// </summary>
    public interface ILF2SceneQuery
    {
        /// <summary>
        /// 通过世界体积查询所有 body 命中结果。
        /// </summary>
        List<SceneQueryHit> QueryBodyHits(in PhysicsState.BattleVolume vol, LF2Entity exclude);

        List<SceneQueryHit> QueryBodyHits(LF2Entity attacker, LF2FrameData attackerFrame, InteractionArea itr);

        List<SceneQueryHit> QueryBodyHits(
            LF2Entity attacker,
            LF2FrameData attackerFrame,
            InteractionArea itr,
            in PhysicsState.BattleVolume volume);

        bool TryGetCollisionCandidateRange(
            LF2Entity attacker,
            out CollisionCandidateRange candidates);

        /// <summary>
        /// Returns a pooled sequence valid only until the current collision-candidate
        /// consumption lifecycle ends. Callers must not retain or mutate the list.
        /// </summary>
        bool TryGetCollisionCandidateSequence(
            LF2Entity attacker,
            out List<SceneQueryHit> candidates);
    }

    /// <summary>
    /// Allocation-free view of the candidate snapshot selected for one battle tick.
    /// The view is invalidated when collision-candidate consumption ends.
    /// </summary>
    public readonly struct CollisionCandidateRange
    {
        private readonly BruteForceSceneQuery owner;
        private readonly List<SceneQueryHit> legacyCandidates;
        private readonly RuntimeEntityHandle attackerHandle;
        private readonly int count;
        private readonly int consumptionEpoch;
        private readonly bool storeAuthority;

        internal CollisionCandidateRange(
            BruteForceSceneQuery owner,
            List<SceneQueryHit> legacyCandidates,
            RuntimeEntityHandle attackerHandle,
            int count,
            int consumptionEpoch,
            bool storeAuthority)
        {
            this.owner = owner;
            this.legacyCandidates = legacyCandidates;
            this.attackerHandle = attackerHandle;
            this.count = count;
            this.consumptionEpoch = consumptionEpoch;
            this.storeAuthority = storeAuthority;
        }

        public int Count => owner != null &&
                            owner.IsCollisionCandidateRangeValidForServices(consumptionEpoch)
            ? count
            : 0;

        public bool TryGet(int index, out SceneQueryHit hit)
        {
            if (owner == null)
            {
                hit = default;
                return false;
            }

            return owner.TryReadCollisionCandidateRangeEntryForServices(
                legacyCandidates,
                attackerHandle,
                count,
                index,
                consumptionEpoch,
                storeAuthority,
                out hit);
        }
    }

    /// <summary>
    /// Candidate-time pair values frozen by the native collector before nearest
    /// selection and later hit consumption. Default is the explicit invalid
    /// compatibility value used by immediate and hand-authored queries.
    /// </summary>
    public readonly struct BattleHitCandidatePairSnapshot :
        System.IEquatable<BattleHitCandidatePairSnapshot>
    {
        public BattleHitCandidatePairSnapshot(
            bool valid,
            int attackerObjectId,
            int targetObjectId,
            int attackerObjectType,
            int targetObjectType,
            int attackerBattleGroup,
            int targetBattleGroup,
            int attackerAction,
            int targetAction,
            int attackerCurrentState,
            int targetCurrentState,
            int attackerPreviousState,
            int targetPreviousState,
            int attackerTickState,
            int targetTickState,
            bool attackerFacing,
            bool targetFacing,
            bool linkedHolderPresent,
            int linkedHolderBattleGroup)
        {
            Valid = valid;
            AttackerObjectId = attackerObjectId;
            TargetObjectId = targetObjectId;
            AttackerObjectType = attackerObjectType;
            TargetObjectType = targetObjectType;
            AttackerBattleGroup = attackerBattleGroup;
            TargetBattleGroup = targetBattleGroup;
            AttackerAction = attackerAction;
            TargetAction = targetAction;
            AttackerCurrentState = attackerCurrentState;
            TargetCurrentState = targetCurrentState;
            AttackerPreviousState = attackerPreviousState;
            TargetPreviousState = targetPreviousState;
            AttackerTickState = attackerTickState;
            TargetTickState = targetTickState;
            AttackerFacing = attackerFacing;
            TargetFacing = targetFacing;
            LinkedHolderPresent = linkedHolderPresent;
            LinkedHolderBattleGroup = linkedHolderBattleGroup;
        }

        public bool Valid { get; }
        public int AttackerObjectId { get; }
        public int TargetObjectId { get; }
        public int AttackerObjectType { get; }
        public int TargetObjectType { get; }
        public int AttackerBattleGroup { get; }
        public int TargetBattleGroup { get; }
        public int AttackerAction { get; }
        public int TargetAction { get; }
        public int AttackerCurrentState { get; }
        public int TargetCurrentState { get; }
        public int AttackerPreviousState { get; }
        public int TargetPreviousState { get; }
        public int AttackerTickState { get; }
        public int TargetTickState { get; }
        public bool AttackerFacing { get; }
        public bool TargetFacing { get; }
        public bool LinkedHolderPresent { get; }
        public int LinkedHolderBattleGroup { get; }

        public bool Equals(BattleHitCandidatePairSnapshot other)
        {
            return Valid == other.Valid &&
                   AttackerObjectId == other.AttackerObjectId &&
                   TargetObjectId == other.TargetObjectId &&
                   AttackerObjectType == other.AttackerObjectType &&
                   TargetObjectType == other.TargetObjectType &&
                   AttackerBattleGroup == other.AttackerBattleGroup &&
                   TargetBattleGroup == other.TargetBattleGroup &&
                   AttackerAction == other.AttackerAction &&
                   TargetAction == other.TargetAction &&
                   AttackerCurrentState == other.AttackerCurrentState &&
                   TargetCurrentState == other.TargetCurrentState &&
                   AttackerPreviousState == other.AttackerPreviousState &&
                   TargetPreviousState == other.TargetPreviousState &&
                   AttackerTickState == other.AttackerTickState &&
                   TargetTickState == other.TargetTickState &&
                   AttackerFacing == other.AttackerFacing &&
                   TargetFacing == other.TargetFacing &&
                   LinkedHolderPresent == other.LinkedHolderPresent &&
                   LinkedHolderBattleGroup == other.LinkedHolderBattleGroup;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleHitCandidatePairSnapshot other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Valid ? 1 : 0;
                hash = (hash * 397) ^ AttackerObjectId;
                hash = (hash * 397) ^ TargetObjectId;
                hash = (hash * 397) ^ AttackerObjectType;
                hash = (hash * 397) ^ TargetObjectType;
                hash = (hash * 397) ^ AttackerBattleGroup;
                hash = (hash * 397) ^ TargetBattleGroup;
                hash = (hash * 397) ^ AttackerAction;
                hash = (hash * 397) ^ TargetAction;
                hash = (hash * 397) ^ AttackerCurrentState;
                hash = (hash * 397) ^ TargetCurrentState;
                hash = (hash * 397) ^ AttackerPreviousState;
                hash = (hash * 397) ^ TargetPreviousState;
                hash = (hash * 397) ^ AttackerTickState;
                hash = (hash * 397) ^ TargetTickState;
                hash = (hash * 397) ^ (AttackerFacing ? 1 : 0);
                hash = (hash * 397) ^ (TargetFacing ? 1 : 0);
                hash = (hash * 397) ^ (LinkedHolderPresent ? 1 : 0);
                hash = (hash * 397) ^ LinkedHolderBattleGroup;
                return hash;
            }
        }

        public static bool operator ==(
            BattleHitCandidatePairSnapshot left,
            BattleHitCandidatePairSnapshot right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleHitCandidatePairSnapshot left,
            BattleHitCandidatePairSnapshot right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct SceneQueryHit
    {
        public readonly LF2Entity Target;
        public readonly int TargetSlot;
        public readonly int BodyX;
        public readonly int ItrIndex;
        public readonly InteractionArea RuntimeItr;
        public readonly bool ZeroAttackerHpOnConsume;
        public readonly bool ReleaseHeavyHeldTargetOnConsume;
        public readonly BattleHitCandidatePairSnapshot PairSnapshot;

        public SceneQueryHit(
            LF2Entity target,
            int bodyX,
            int itrIndex = -1,
            InteractionArea runtimeItr = null,
            bool zeroAttackerHpOnConsume = false,
            bool releaseHeavyHeldTargetOnConsume = false,
            BattleHitCandidatePairSnapshot pairSnapshot = default)
        {
            Target = target;
            TargetSlot = target?.Runtime?.SlotIndex ?? -1;
            BodyX = bodyX;
            ItrIndex = itrIndex;
            RuntimeItr = runtimeItr;
            ZeroAttackerHpOnConsume = zeroAttackerHpOnConsume;
            ReleaseHeavyHeldTargetOnConsume = releaseHeavyHeldTargetOnConsume;
            PairSnapshot = pairSnapshot;
        }

        internal SceneQueryHit(
            LF2Entity target,
            int targetSlot,
            int bodyX,
            int itrIndex,
            InteractionArea runtimeItr,
            bool zeroAttackerHpOnConsume,
            bool releaseHeavyHeldTargetOnConsume,
            BattleHitCandidatePairSnapshot pairSnapshot = default)
        {
            Target = target;
            TargetSlot = targetSlot;
            BodyX = bodyX;
            ItrIndex = itrIndex;
            RuntimeItr = runtimeItr;
            ZeroAttackerHpOnConsume = zeroAttackerHpOnConsume;
            ReleaseHeavyHeldTargetOnConsume = releaseHeavyHeldTargetOnConsume;
            PairSnapshot = pairSnapshot;
        }

        public LF2Entity ResolveCurrentTarget(SimulationWorld world)
        {
            if (world != null && TargetSlot >= 0)
                return world.FindEntityByRuntimeSlotForQuery(TargetSlot);

            return Target;
        }
    }
}
