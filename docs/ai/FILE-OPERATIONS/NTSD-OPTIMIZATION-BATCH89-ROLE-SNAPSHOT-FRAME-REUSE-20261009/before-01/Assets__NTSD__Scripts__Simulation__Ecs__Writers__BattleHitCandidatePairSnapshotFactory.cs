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

            LF2FrameData attackerCurrentFrame =
                attacker.FrameCache.GetNativeFrameDataById(attacker.Frame.N);
            LF2FrameData targetCurrentFrame =
                target.FrameCache.GetNativeFrameDataById(target.Frame.N);
            LF2FrameData attackerTickFrame = attacker.GetCollisionFrameData();
            LF2FrameData targetTickFrame = target.GetCollisionFrameData();
            if (attackerTickFrame == null || targetTickFrame == null)
            {
                return default;
            }

            LF2FrameData attackerPreviousFrame =
                attacker.FrameCache.GetNativeFrameDataById(attacker.Frame.Prev);
            LF2FrameData targetPreviousFrame =
                target.FrameCache.GetNativeFrameDataById(target.Frame.Prev);

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
                attackerCurrentFrame?.state ?? 0,
                targetCurrentFrame?.state ?? 0,
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
