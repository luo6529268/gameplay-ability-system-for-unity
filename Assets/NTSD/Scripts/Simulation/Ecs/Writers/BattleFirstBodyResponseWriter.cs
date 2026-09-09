using System;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleFirstBodyResponseAttemptResult
    {
        internal BattleFirstBodyResponseAttemptResult(
            bool eligible,
            BattleFirstBodyResponseResult response,
            bool rollConsumed,
            int roll,
            bool brokenArmorBeforeResponse)
        {
            Eligible = eligible;
            Response = response;
            RollConsumed = rollConsumed;
            Roll = roll;
            BrokenArmorBeforeResponse = brokenArmorBeforeResponse;
        }

        public bool Eligible { get; }
        public BattleFirstBodyResponseResult Response { get; }
        public bool Applied => Eligible && Response.Applied;
        public bool RollConsumed { get; }
        public int Roll { get; }
        public bool BrokenArmorBeforeResponse { get; }
    }

    internal static class BattleFirstBodyResponseWriter
    {
        internal static bool IsUnarmoredContinuationDisposition(
            BattleHitCandidateDisposition disposition)
        {
            return disposition == BattleHitCandidateDisposition.Damage ||
                   disposition == BattleHitCandidateDisposition.Oid300Redirect;
        }

        internal static BattleFirstBodyResponseAttemptResult TryApply(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            BattleFirstBodyResponseAttemptResult attempt = ResolveAttempt(
                world,
                attacker,
                target,
                interaction,
                false,
                0);
            if (!attempt.Eligible)
                return attempt;

            if (attempt.Response.NeedsRoll)
            {
                int roll = world.NativeRandom.SynchronizedNext(
                    (uint)attempt.Response.Chance,
                    100);
                attempt = ResolveAttempt(
                    world,
                    attacker,
                    target,
                    interaction,
                    true,
                    roll);
            }

            if (attempt.Applied)
                ApplyResponse(attacker, target, in attempt);
            return attempt;
        }

        internal static BattleFirstBodyResponseAttemptResult ResolveAttempt(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction,
            bool hasRoll,
            int roll)
        {
            if (world == null || attacker?.Runtime == null ||
                target?.Runtime == null || target.Health == null ||
                interaction == null || interaction.kind != 0)
            {
                return default;
            }

            bool brokenArmorBeforeResponse = false;
            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            if (targetType == (int)LF2ObjectType.Character)
            {
                BattleOrdinaryCharacterDamageRoute route =
                    BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                        world,
                        attacker,
                        target,
                        interaction);
                if (!route.UsesUnarmoredHit)
                    return default;
                brokenArmorBeforeResponse = route.Kind ==
                    BattleOrdinaryCharacterDamageRouteKind
                        .UnarmoredType1BrokenFallback;
            }
            else if (HasSelectedType0Armor(target))
            {
                return default;
            }

            LF2FrameData currentFrame = target.Frame?.D;
            int firstBodyKind = currentFrame?.PrimaryBodyKind ?? 0;
            int firstBodyRespond = currentFrame?.PrimaryBodyRespond ?? 0;
            BattleFirstBodyResponseResult response =
                BattleFirstBodyResponseResolver.Resolve(
                    firstBodyKind,
                    firstBodyRespond,
                    interaction.injury,
                    attacker.RelationTeam,
                    hasRoll,
                    roll);
            return new BattleFirstBodyResponseAttemptResult(
                true,
                response,
                response.UsedProvidedRoll,
                response.UsedProvidedRoll ? roll : 0,
                brokenArmorBeforeResponse);
        }

        internal static void ApplyResponse(
            LF2Entity attacker,
            LF2Entity target,
            in BattleFirstBodyResponseAttemptResult attempt)
        {
            if (!attempt.Applied || attacker?.Runtime == null ||
                target?.Runtime == null || target.Health == null)
            {
                return;
            }

            BattleFirstBodyResponseResult response = attempt.Response;
            if (attempt.BrokenArmorBeforeResponse)
                target.Runtime.RuntimeArmorHp118 = -1;

            if (response.WriteTargetGroup)
                target.RelationTeam = response.TargetGroup;
            if (response.WriteTargetAction)
            {
                target.DirectWriteRawFramePreserveWaitCounter(
                    response.TargetAction);
                if (response.ResetTargetFrameCounter)
                    target.Runtime.FrameWaitCounter = 0;
            }
            if (response.WriteAttackerAction)
            {
                attacker.DirectWriteRawFramePreserveWaitCounter(
                    response.AttackerAction);
                if (response.ResetAttackerFrameCounter)
                    attacker.Runtime.FrameWaitCounter = 0;
            }
            if (response.ApplyHold)
            {
                attacker.FrameDelay = 3;
                target.FrameDelay = -3;
            }
            if (response.ApplyManualDamage)
            {
                int injury = response.ManualDamage;
                int remaining = unchecked(target.Health.HP - injury);
                target.Health.HP = Math.Max(0, remaining);
                target.Runtime.InputHpConsumedTotal34C = unchecked(
                    target.Runtime.InputHpConsumedTotal34C + injury);
                attacker.Runtime.InputScoreTotal348 = unchecked(
                    attacker.Runtime.InputScoreTotal348 + injury);
            }
        }

        private static bool HasSelectedType0Armor(LF2Entity target)
        {
            LF2CharacterData data =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            return data?.armors != null &&
                   data.armors.Count > 0 &&
                   data.armors[0] != null &&
                   data.armors[0].type == 0;
        }
    }
}
