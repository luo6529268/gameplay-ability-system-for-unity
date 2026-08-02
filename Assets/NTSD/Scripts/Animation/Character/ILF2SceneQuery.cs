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

    public readonly struct SceneQueryHit
    {
        public readonly LF2Entity Target;
        public readonly int TargetSlot;
        public readonly int BodyX;
        public readonly int ItrIndex;
        public readonly InteractionArea RuntimeItr;
        public readonly bool ZeroAttackerHpOnConsume;
        public readonly bool ReleaseHeavyHeldTargetOnConsume;

        public SceneQueryHit(
            LF2Entity target,
            int bodyX,
            int itrIndex = -1,
            InteractionArea runtimeItr = null,
            bool zeroAttackerHpOnConsume = false,
            bool releaseHeavyHeldTargetOnConsume = false)
        {
            Target = target;
            TargetSlot = target?.Runtime?.SlotIndex ?? -1;
            BodyX = bodyX;
            ItrIndex = itrIndex;
            RuntimeItr = runtimeItr;
            ZeroAttackerHpOnConsume = zeroAttackerHpOnConsume;
            ReleaseHeavyHeldTargetOnConsume = releaseHeavyHeldTargetOnConsume;
        }

        internal SceneQueryHit(
            LF2Entity target,
            int targetSlot,
            int bodyX,
            int itrIndex,
            InteractionArea runtimeItr,
            bool zeroAttackerHpOnConsume,
            bool releaseHeavyHeldTargetOnConsume)
        {
            Target = target;
            TargetSlot = targetSlot;
            BodyX = bodyX;
            ItrIndex = itrIndex;
            RuntimeItr = runtimeItr;
            ZeroAttackerHpOnConsume = zeroAttackerHpOnConsume;
            ReleaseHeavyHeldTargetOnConsume = releaseHeavyHeldTargetOnConsume;
        }

        public LF2Entity ResolveCurrentTarget(SimulationWorld world)
        {
            if (world != null && TargetSlot >= 0)
                return world.FindEntityByRuntimeSlotForQuery(TargetSlot);

            return Target;
        }
    }
}
