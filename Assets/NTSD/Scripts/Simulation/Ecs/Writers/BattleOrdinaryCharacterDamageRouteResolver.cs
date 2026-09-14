using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal enum BattleOrdinaryCharacterDamageRouteKind : byte
    {
        Unsupported = 0,
        Unarmored = 1,
        ReducedDefense = 2,
        ReducedType1Armor = 3,
        UnarmoredType1Bypass = 4,
        UnarmoredType1ResourceFallback = 5,
        UnarmoredType1BrokenFallback = 6,
    }

    internal readonly struct BattleOrdinaryCharacterDamageRoute
    {
        internal BattleOrdinaryCharacterDamageRoute(
            BattleOrdinaryCharacterDamageRouteKind kind,
            LF2ArmorData armor,
            int effectiveInjury,
            BattleType1ArmorMatchResult armorMatch,
            BattleType1ArmorActivationResult armorActivation)
        {
            Kind = kind;
            Armor = armor;
            EffectiveInjury = effectiveInjury;
            ArmorMatch = armorMatch;
            ArmorActivation = armorActivation;
        }

        public BattleOrdinaryCharacterDamageRouteKind Kind { get; }
        public LF2ArmorData Armor { get; }
        public int EffectiveInjury { get; }
        public BattleType1ArmorMatchResult ArmorMatch { get; }
        public BattleType1ArmorActivationResult ArmorActivation { get; }

        internal bool UsesReducedHit =>
            Kind == BattleOrdinaryCharacterDamageRouteKind.ReducedDefense ||
            Kind == BattleOrdinaryCharacterDamageRouteKind.ReducedType1Armor;

        internal bool UsesUnarmoredHit =>
            Kind == BattleOrdinaryCharacterDamageRouteKind.Unarmored ||
            Kind == BattleOrdinaryCharacterDamageRouteKind.UnarmoredType1Bypass ||
            Kind == BattleOrdinaryCharacterDamageRouteKind
                .UnarmoredType1ResourceFallback ||
            Kind == BattleOrdinaryCharacterDamageRouteKind
                .UnarmoredType1BrokenFallback;
    }

    internal static class BattleOrdinaryCharacterDamageRouteResolver
    {
        internal static BattleOrdinaryCharacterDamageRoute Resolve(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (world == null || attacker?.Runtime == null ||
                target?.Runtime == null || target.Health == null ||
                interaction == null || attackerData == null || targetData == null ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                return Result(
                    BattleOrdinaryCharacterDamageRouteKind.Unsupported,
                    null,
                    interaction?.injury ?? 0);
            }

            if (interaction.kind != 0)
            {
                return Result(
                    BattleOrdinaryCharacterDamageRouteKind.Unarmored,
                    null,
                    interaction.injury);
            }

            BattleOrdinaryDefenseResult defense =
                BattleOrdinaryDefenseResolver.Resolve(
                    interaction.kind,
                    interaction.effect,
                    interaction.spark,
                    interaction.dbdefend,
                    interaction.dvx,
                    attacker.Dirh(),
                    target.Dirh(),
                    target.Frame?.D?.state ?? 0,
                    target.Health.HP,
                    attackerData.type_sub);
            if (defense.Decision == BattleOrdinaryDefenseDecisionKind.Applies)
            {
                return Result(
                    BattleOrdinaryCharacterDamageRouteKind.ReducedDefense,
                    null,
                    interaction.injury);
            }

            if (targetData.armors == null || targetData.armors.Count == 0 ||
                targetData.armors[0] == null || targetData.armors[0].type != 1)
            {
                return Result(
                    BattleOrdinaryCharacterDamageRouteKind.Unarmored,
                    null,
                    interaction.injury);
            }

            LF2ArmorData armor = targetData.armors[0];
            if (targetData.armors.Count != 1)
            {
                return Result(
                    BattleOrdinaryCharacterDamageRouteKind.Unsupported,
                    armor,
                    interaction.injury);
            }

            int activeModePercent = world.Runtime?.NativeHitResourceRules?.
                ActiveModeAttackingPercent1C ??
                NTSD28HitResourceRulesRuntimeState
                    .DefaultActiveModeAttackingPercent1C;
            int effectiveInjury = ResolveNativeAttackingInjury(
                interaction.injury,
                attackerData.definition_attacking,
                activeModePercent);
            BattleType1ArmorMatchResult match =
                BattleType1ArmorMatchResolver.Resolve(
                    armor,
                    interaction,
                    LF2Entity.ResolveCurrentDataObjectId(attacker),
                    attacker.Dirh() < 0 ? 1 : 0,
                    target.Dirh() < 0 ? 1 : 0,
                    target.Frame?.N ?? 0,
                    target.Frame?.D?.state ?? 0,
                    target.Runtime.Bdefend,
                    effectiveInjury,
                    true);
            if (match.Decision == BattleType1ArmorMatchDecisionKind.Bypassed ||
                match.Decision ==
                    BattleType1ArmorMatchDecisionKind.CandidateRejected)
            {
                return new BattleOrdinaryCharacterDamageRoute(
                    BattleOrdinaryCharacterDamageRouteKind
                        .UnarmoredType1Bypass,
                    armor,
                    effectiveInjury,
                    match,
                    default);
            }
            if (match.Decision != BattleType1ArmorMatchDecisionKind.Applies)
            {
                return new BattleOrdinaryCharacterDamageRoute(
                    BattleOrdinaryCharacterDamageRouteKind.Unsupported,
                    armor,
                    effectiveInjury,
                    match,
                    default);
            }

            BattleType1ArmorActivationResult activation =
                BattleType1ArmorActivationResolver.Resolve(
                    armor,
                    interaction.injury,
                    effectiveInjury,
                    target.Health.PP,
                    armor.hp != 0,
                    target.Runtime.RuntimeArmorHp118);
            if (activation.Available)
            {
                return new BattleOrdinaryCharacterDamageRoute(
                    BattleOrdinaryCharacterDamageRouteKind.ReducedType1Armor,
                    armor,
                    effectiveInjury,
                    match,
                    activation);
            }

            return new BattleOrdinaryCharacterDamageRoute(
                activation.ArmorHpBroken
                    ? BattleOrdinaryCharacterDamageRouteKind
                        .UnarmoredType1BrokenFallback
                    : BattleOrdinaryCharacterDamageRouteKind
                        .UnarmoredType1ResourceFallback,
                armor,
                effectiveInjury,
                match,
                activation);
        }

        internal static int ResolveNativeAttackingInjury(
            int injury,
            int definitionAttackingPercent,
            int activeModePercent)
        {
            int multiplier = definitionAttackingPercent;
            if (multiplier < 1)
                multiplier = activeModePercent;
            if (multiplier < 1 || injury == 0)
                return injury;

            uint productBits = unchecked(
                (uint)injury * (uint)multiplier);
            int product = unchecked((int)productBits);
            return product / 100;
        }

        private static BattleOrdinaryCharacterDamageRoute Result(
            BattleOrdinaryCharacterDamageRouteKind kind,
            LF2ArmorData armor,
            int effectiveInjury)
        {
            return new BattleOrdinaryCharacterDamageRoute(
                kind,
                armor,
                effectiveInjury,
                default,
                default);
        }
    }
}
