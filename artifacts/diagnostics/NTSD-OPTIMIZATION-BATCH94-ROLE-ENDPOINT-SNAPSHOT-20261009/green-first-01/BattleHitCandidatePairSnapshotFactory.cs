using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleHitCandidatePairSnapshotFactory
    {
        internal readonly struct Endpoint
        {
            internal Endpoint(LF2Entity entity, LF2FrameData currentFrame, LF2FrameData tickFrame)
            {
                Valid = true;
                ObjectId = LF2Entity.ResolveCurrentDataObjectId(entity);
                ObjectType = entity.GetCurrentDataObjectTypeForSimulation();
                BattleGroup = entity.RelationTeam;
                Action = entity.Frame.N;
                CurrentState = currentFrame?.state ?? 0;
                PreviousState = entity.FrameCache.GetNativeFrameDataById(entity.Frame.Prev)?.state ?? 0;
                TickState = tickFrame.state;
                Facing = entity.Runtime.IsFacingLeft;
            }

            internal bool Valid { get; }
            internal int ObjectId { get; }
            internal int ObjectType { get; }
            internal int BattleGroup { get; }
            internal int Action { get; }
            internal int CurrentState { get; }
            internal int PreviousState { get; }
            internal int TickState { get; }
            internal bool Facing { get; }
        }

        internal static Endpoint CaptureEndpoint(
            LF2Entity entity,
            LF2FrameData currentFrame,
            LF2FrameData tickFrame)
        {
            if (entity?.Runtime == null || entity.Frame == null ||
                entity.FrameCache == null || tickFrame == null)
            {
                return default;
            }
            return new Endpoint(entity, currentFrame, tickFrame);
        }

        internal static BattleHitCandidatePairSnapshot ComposeFromEndpoints(
            LF2Entity attacker,
            SimulationWorld world,
            in Endpoint attackerEndpoint,
            in Endpoint targetEndpoint)
        {
            if (!attackerEndpoint.Valid || !targetEndpoint.Valid)
                return default;

            // Alignment contract: NTSD-OPT-H07-ROLE-ENDPOINT-SNAPSHOT-094.
            // Holder lookup remains live at the original pair capture point.
            int holderSlot = attacker.ResolveReleaseNeutralHolderSlotOrImplicitZero();
            LF2Entity linkedHolder = holderSlot >= 0
                ? (world ?? attacker.RegisteredWorldForSimulation ?? attacker.Match)?
                    .FindEntityByRuntimeSlotForQuery(holderSlot)
                : null;
            return new BattleHitCandidatePairSnapshot(
                true,
                attackerEndpoint.ObjectId, targetEndpoint.ObjectId,
                attackerEndpoint.ObjectType, targetEndpoint.ObjectType,
                attackerEndpoint.BattleGroup, targetEndpoint.BattleGroup,
                attackerEndpoint.Action, targetEndpoint.Action,
                attackerEndpoint.CurrentState, targetEndpoint.CurrentState,
                attackerEndpoint.PreviousState, targetEndpoint.PreviousState,
                attackerEndpoint.TickState, targetEndpoint.TickState,
                attackerEndpoint.Facing, targetEndpoint.Facing,
                linkedHolder != null, linkedHolder?.RelationTeam ?? 0);
        }

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

            return CapturePayload(attacker, target, world,
                attackerCurrentFrame, targetCurrentFrame, attackerTickFrame, targetTickFrame);
        }

        internal static BattleHitCandidatePairSnapshot CaptureWithResolvedFrames(
            LF2Entity attacker,
            LF2Entity target,
            SimulationWorld world,
            LF2FrameData attackerCurrentFrame,
            LF2FrameData targetCurrentFrame,
            LF2FrameData attackerTickFrame,
            LF2FrameData targetTickFrame)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                attacker.Frame == null || target.Frame == null ||
                attacker.FrameCache == null || target.FrameCache == null ||
                attackerTickFrame == null || targetTickFrame == null)
            {
                return default;
            }

            // Alignment contract: NTSD-OPT-H07-ROLE-SNAPSHOT-FRAME-REUSE-089.
            // Only authored frame references are reused; all live pair fields keep their capture point.
            return CapturePayload(attacker, target, world,
                attackerCurrentFrame, targetCurrentFrame, attackerTickFrame, targetTickFrame);
        }

        private static BattleHitCandidatePairSnapshot CapturePayload(
            LF2Entity attacker,
            LF2Entity target,
            SimulationWorld world,
            LF2FrameData attackerCurrentFrame,
            LF2FrameData targetCurrentFrame,
            LF2FrameData attackerTickFrame,
            LF2FrameData targetTickFrame)
        {
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
