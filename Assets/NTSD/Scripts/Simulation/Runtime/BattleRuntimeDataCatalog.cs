using System;
using System.Collections.Generic;
using NTSD.Animation;

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
            BattleHitRecordLifecycleCatalog hitRecordLifecycleCatalog = default)
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
                LF2CharacterDataWrapper config = configResolver(definition.id);
                if (config?.characterData != null)
                    characterConfigs[definition.id] = config;
            }

            generation = generation == int.MaxValue ? 1 : generation + 1;
            HitRecordLifecycleCatalog = hitRecordLifecycleCatalog;
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
}
