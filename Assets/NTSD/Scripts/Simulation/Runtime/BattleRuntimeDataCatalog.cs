using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;

namespace NTSD.Simulation
{
    public readonly struct BattleHitRecordLifecycleCatalog
    {
        public BattleHitRecordLifecycleCatalog(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }

        public static BattleHitRecordLifecycleCatalog Unavailable => default;
        public static BattleHitRecordLifecycleCatalog Available =>
            new BattleHitRecordLifecycleCatalog(true);

        public bool IsAvailable { get; }

        public bool TryResolveAge(int age, out int pic)
        {
            pic = -1;
            if (!IsAvailable)
                return false;

            if (age >= 0 && age < 5)
                pic = age;
            else if (age >= 10 && age < 15)
                pic = age - 5;
            else if (age >= 20 && age < 29)
                pic = (age - 20) / 2 + 10;
            else if (age >= 30 && age < 39)
                pic = (age - 30) / 2 + 15;

            return pic >= 0 && pic < BattleCommonVisualCatalog.SparkFrameCount;
        }
    }

    /// <summary>
    /// Immutable battle-DAT lookup owned by one SimulationWorld. Unity managers
    /// populate it before the battle allocation seal; simulation ticks only read
    /// the managed lookup and never query a MonoBehaviour singleton.
    /// </summary>
    public sealed class BattleRuntimeDataCatalog
    {
        private readonly Dictionary<int, ObjectDefinition> objectDefinitions =
            new Dictionary<int, ObjectDefinition>();
        private readonly Dictionary<int, LF2CharacterDataWrapper> characterConfigs =
            new Dictionary<int, LF2CharacterDataWrapper>();
        private ObjectDefinition[] orderedObjectDefinitions =
            Array.Empty<ObjectDefinition>();
        private bool sealedForBattle;
        private int generation;

        public bool IsReady { get; private set; }
        public LoganFusionCatalog FusionCatalog { get; private set; }
        public LoganKindCatalog KindCatalog { get; private set; }
        public LoganContentIdentity LoganContentIdentity { get; private set; }
        public bool IsSealedForBattle => sealedForBattle;
        public int Generation => generation;
        public int ObjectDefinitionCount => objectDefinitions.Count;
        public int CharacterConfigCount => characterConfigs.Count;
        public IReadOnlyList<ObjectDefinition> ObjectDefinitions =>
            orderedObjectDefinitions;
        public BattleHitRecordLifecycleCatalog HitRecordLifecycleCatalog
        {
            get;
            private set;
        }

        public void Prepare(
            IReadOnlyList<ObjectDefinition> definitions,
            Func<int, LF2CharacterDataWrapper> configResolver,
            BattleHitRecordLifecycleCatalog hitRecordLifecycleCatalog = default,
            LoganObjectCatalog loganCatalog = null)
        {
            if (sealedForBattle)
            {
                throw new InvalidOperationException(
                    "Battle runtime DAT cannot be replaced while the battle is sealed.");
            }
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));
            if (configResolver == null)
                throw new ArgumentNullException(nameof(configResolver));

            LF2CharacterDataWrapper[] preparedLoganConfigs = null;
            if (loganCatalog != null)
            {
                var identity = loganCatalog.ContentIdentity;
                if (identity.DecodeContractTag != Animation.LoganContentIdentity.CurrentDecodeContractTag ||
                    identity.ObjectDefinitionFingerprint != loganCatalog.DefinitionFingerprint ||
                    identity.FusionInputFingerprint != loganCatalog.FusionInput.InputFingerprint ||
                    identity.FusionSemanticFingerprint != loganCatalog.FusionInput.SemanticFingerprint ||
                    identity.ModeInputFingerprint != loganCatalog.ModeComboInput?.InputFingerprint ||
                    identity.ModeSemanticFingerprint != loganCatalog.ModeComboInput?.SemanticFingerprint ||
                    identity.KindInputFingerprint != loganCatalog.KindInput.InputFingerprint ||
                    identity.KindSemanticFingerprint != loganCatalog.KindInput.SemanticFingerprint)
                    throw new ArgumentException("Prepared fusion data requires its complete current content identity.", nameof(loganCatalog));
                if (definitions.Count != loganCatalog.Entries.Count)
                    throw new ArgumentException("Prepared objects do not match the captured Logan catalog.", nameof(definitions));
                for (int index = 0; index < definitions.Count; index++)
                {
                    var definition = definitions[index];
                    var entry = loganCatalog.Entries[index];
                    if (definition == null || definition.id != entry.Id || definition.type != entry.Type)
                        throw new ArgumentException("Prepared object order or type does not match the captured Logan catalog.", nameof(definitions));
                }
                preparedLoganConfigs = new LF2CharacterDataWrapper[definitions.Count];
                for (int index = 0; index < definitions.Count; index++)
                {
                    var config = configResolver(definitions[index].id);
                    if (config != null && config.characterId != definitions[index].id)
                        throw new ArgumentException("Prepared character config has a different object identity.", nameof(configResolver));
                    preparedLoganConfigs[index] = config;
                }
            }

            objectDefinitions.Clear();
            characterConfigs.Clear();
            if (orderedObjectDefinitions.Length != definitions.Count)
                orderedObjectDefinitions = new ObjectDefinition[definitions.Count];
            objectDefinitions.EnsureCapacity(definitions.Count);
            characterConfigs.EnsureCapacity(definitions.Count);
            for (int index = 0; index < definitions.Count; index++)
            {
                ObjectDefinition definition = definitions[index];
                orderedObjectDefinitions[index] = definition;
                if (definition == null)
                    continue;

                objectDefinitions[definition.id] = definition;
                LF2CharacterDataWrapper config = preparedLoganConfigs != null
                    ? preparedLoganConfigs[index] : configResolver(definition.id);
                if (config?.characterData != null)
                    characterConfigs[definition.id] = config;
            }

            generation = generation == int.MaxValue ? 1 : generation + 1;
            HitRecordLifecycleCatalog = hitRecordLifecycleCatalog;
            FusionCatalog = loganCatalog?.FusionInput.Catalog;
            KindCatalog = loganCatalog?.KindInput.Catalog;
            LoganContentIdentity = loganCatalog?.ContentIdentity;
            IsReady = objectDefinitions.Count > 0;
        }

        public ObjectDefinition GetObjectDefinition(int objectId)
        {
            return objectDefinitions.TryGetValue(
                objectId,
                out ObjectDefinition definition)
                ? definition
                : null;
        }

        public LF2CharacterDataWrapper GetCharacterConfig(int objectId)
        {
            return characterConfigs.TryGetValue(
                objectId,
                out LF2CharacterDataWrapper config)
                ? config
                : null;
        }

        public LF2CharacterData GetCharacterData(int objectId)
        {
            return GetCharacterConfig(objectId)?.characterData;
        }

        internal void Seal()
        {
            if (!IsReady)
                throw new InvalidOperationException("Battle runtime DAT is not prepared.");
            sealedForBattle = true;
        }

        internal void Unseal()
        {
            sealedForBattle = false;
        }
    }

    internal static class BattleKindTableRules
    {
        internal static LoganKindCatalog ResolveCatalog(LF2Entity entity)
        {
            SimulationWorld world = entity?.RegisteredWorldForSimulation ?? entity?.Match;
            return world?.RuntimeDataCatalog?.KindCatalog ?? LoganKindCatalogInput.LockedCatalog;
        }

        internal static bool RejectCandidate(LoganKindCatalog catalog,
            int attackerOid, int targetOid, int targetType, int interactionKind)
        {
            if (targetType != (int)LF2ObjectType.SpecialAttack || interactionKind == 9)
                return false;
            return catalog?.FindEffect(targetOid)?.RespondsTo(attackerOid) == true;
        }

        internal static LoganKindRecord FindTransform(LF2Entity attacker, LF2Entity target)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                attacker.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
                return null;

            int attackerOid = LF2Entity.ResolveCurrentDataObjectId(attacker);
            int targetOid = LF2Entity.ResolveCurrentDataObjectId(target);
            foreach (LoganKindRecord record in ResolveCatalog(attacker).Records)
                if (record.Binds(attackerOid) && record.RespondsTo(targetOid))
                    return record;
            return null;
        }

        internal static int ResponseFrame(LoganKindRecord record) =>
            record == null ? 0 : record.Frame == 0 ? 40 : record.Frame;
    }
}
