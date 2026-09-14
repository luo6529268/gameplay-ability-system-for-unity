using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleNativeOrdinaryHitPreludePlan
    {
        internal BattleNativeOrdinaryHitPreludePlan(BattleOrdinaryCharacterDamageRoute route, LF2ArmorData firstArmor, int targetType)
        {
            Route = route;
            FeedbackArmor = route.UsesUnarmoredHit && firstArmor?.type == 0 && targetType != 0 ? firstArmor : null;
            RejectAfterPrelude = route.UsesUnarmoredHit && firstArmor != null && firstArmor.type != 0 && firstArmor.type != 1;
        }

        internal BattleOrdinaryCharacterDamageRoute Route { get; }
        internal LF2ArmorData FeedbackArmor { get; }
        internal bool RejectAfterPrelude { get; }
        internal bool RunUnarmoredPrelude => Route.UsesUnarmoredHit;
        internal bool FeedbackOnly => FeedbackArmor != null;
        internal bool RejectBeforePrelude => Route.Kind == BattleOrdinaryCharacterDamageRouteKind.Unsupported;
    }

    internal static class BattleNativeOrdinaryHitPrelude
    {
        // Alignment contract: NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001.
        internal static BattleNativeOrdinaryHitPreludePlan Resolve(SimulationWorld world, LF2Entity attacker, LF2Entity target, InteractionArea itr)
        {
            var route = BattleOrdinaryCharacterDamageRouteResolver.ResolveForNativeEntry(world, attacker, target, itr);
            var armors = LF2HitResolveRuntimeData.ResolveCharacterData(target)?.armors;
            var firstArmor = armors != null && armors.Count > 0 ? armors[0] : null;
            return new BattleNativeOrdinaryHitPreludePlan(route, firstArmor, target.GetCurrentDataObjectTypeForSimulation());
        }

        internal static void Apply(SimulationWorld world, LF2Entity attacker, LF2Entity target, InteractionArea itr,
            in BattleNativeOrdinaryHitPreludePlan plan)
        {
            if (!plan.RunUnarmoredPrelude)
                return;
            if (plan.Route.Kind == BattleOrdinaryCharacterDamageRouteKind.UnarmoredType1BrokenFallback)
                target.Runtime.RuntimeArmorHp118 = -1;

            int targetSlot = target.Runtime.SlotIndex;
            int childSlot = target.Runtime.TargetSlotIndex;
            var child = childSlot >= 0 ? world.FindEntityByRuntimeSlotForQuery(childSlot) : null;
            bool reciprocal = child?.Runtime != null && child.Runtime.HolderStableId == targetSlot;
            if (reciprocal && target.Runtime.LinkState == 2 && child.Runtime.LinkState == -2)
            {
                attacker.ItrRest?.SetVrest(childSlot, 45);
                target.ItrRest?.SetVrest(childSlot, 30);
                target.Runtime.LinkState = 0;
                child.Runtime.LinkState = 0;
                child.DirectWriteNativeRawFramePreserveWaitCounter(world.NativeRandom.SynchronizedNext(0xECu, 6));
                target.Runtime.Vy = -1.0000000000000258;
            }
            if (reciprocal && IsSpecialLinkRestGate(target, itr))
            {
                attacker.ItrRest?.SetVrest(childSlot, 45);
                target.ItrRest?.SetVrest(childSlot, 30);
            }
        }

        internal static bool IsSpecialLinkRestGate(LF2Entity target, InteractionArea itr)
        {
            var frame = target.FrameCache?.GetNativeFrameDataById(target.Runtime.WaitCounter);
            int kind = frame?.primaryBodyKindForEffectSuppression ?? 0;
            int property = LF2HitResolveRuntimeData.ResolveCharacterData(target)?.property ?? 0;
            int caughtAction = itr.caughtact != null && itr.caughtact.Length > 0 ? itr.caughtact[0] : 0;
            return kind == 50 || kind == 52 || frame?.state == 602 || frame?.state == 603 ||
                property == 2 || property == 3 ||
                (itr.effect >= 8 && itr.effect <= 16 && (caughtAction == -2 || caughtAction == -3));
        }

        internal static bool AppendFeedback(SimulationWorld world, LF2Entity attacker, LF2Entity target, InteractionArea itr,
            int interactionIndex, in BattleNativeOrdinaryHitPreludePlan plan)
        {
            BattleNativeHitSparkWriter.Append(world, attacker, target, itr, interactionIndex, plan.FeedbackArmor, false, false);
            return true;
        }
    }
}
