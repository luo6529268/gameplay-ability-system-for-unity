using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleHitCandidatePairSnapshotFactory
    {
        internal static BattleHitCandidatePairSnapshot Capture(
            LF2Entity attacker,
            LF2Entity target,
            SimulationWorld world = null)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                attacker.Frame == null || target.Frame == null ||
                attacker.FrameCache == null || target.FrameCache == null)
            {
                return default;
            }

            LF2FrameData attackerCurrentFrame = attacker.FrameCache.HasFrame(
                attacker.Frame.N)
                    ? attacker.Frame.D
                    : null;
            LF2FrameData targetCurrentFrame = target.FrameCache.HasFrame(
                target.Frame.N)
                    ? target.Frame.D
                    : null;
            LF2FrameData attackerTickFrame = attacker.GetCollisionFrameData();
            LF2FrameData targetTickFrame = target.GetCollisionFrameData();
            if (attackerCurrentFrame == null || targetCurrentFrame == null ||
                attackerTickFrame == null || targetTickFrame == null)
            {
                return default;
            }

            LF2FrameData attackerPreviousFrame =
                attacker.FrameCache.GetFrameDataById(attacker.Frame.Prev);
            LF2FrameData targetPreviousFrame =
                target.FrameCache.GetFrameDataById(target.Frame.Prev);

            int holderSlot =
                attacker.ResolveReleaseNeutralHolderSlotOrImplicitZero();
            LF2Entity linkedHolder = holderSlot >= 0
                ? (world ?? attacker.RegisteredWorldForSimulation ?? attacker.Match)?
                    .FindEntityByRuntimeSlotForQuery(holderSlot)
                : null;

            return new BattleHitCandidatePairSnapshot(
                true,
                LF2Entity.ResolveCurrentDataObjectId(attacker),
                LF2Entity.ResolveCurrentDataObjectId(target),
                attacker.GetCurrentDataObjectTypeForSimulation(),
                target.GetCurrentDataObjectTypeForSimulation(),
                attacker.RelationTeam,
                target.RelationTeam,
                attacker.Frame.N,
                target.Frame.N,
                attackerCurrentFrame.state,
                targetCurrentFrame.state,
                attackerPreviousFrame?.state ?? 0,
                targetPreviousFrame?.state ?? 0,
                attackerTickFrame.state,
                targetTickFrame.state,
                attacker.Runtime.IsFacingLeft,
                target.Runtime.IsFacingLeft,
                linkedHolder != null,
                linkedHolder?.RelationTeam ?? 0);
        }
    }
}
