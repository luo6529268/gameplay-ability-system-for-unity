using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.LevelEditor
{
    [CreateAssetMenu(
        fileName = "BattleMapCatalog",
        menuName = "NTSD/Maps/Map Catalog")]
    public sealed class BattleMapCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private string mapId = "";
            [SerializeField] private BattleMapBoundaryDefinition boundaryDefinition;
            [SerializeField] private BattleMapPresentationDefinition presentationDefinition;

            public string MapId => mapId;
            public BattleMapBoundaryDefinition BoundaryDefinition => boundaryDefinition;
            public BattleMapPresentationDefinition PresentationDefinition => presentationDefinition;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryValidate(out string failure)
        {
            if (entries == null || entries.Count == 0)
            {
                failure = "Map catalog must contain at least one entry.";
                return false;
            }

            var mapIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < entries.Count; index++)
            {
                if (!TryValidateEntry(entries[index], out string entryFailure))
                {
                    failure = entryFailure;
                    return false;
                }

                if (!mapIds.Add(entries[index].MapId))
                {
                    failure = "Map catalog contains a duplicate MapId.";
                    return false;
                }
            }

            failure = string.Empty;
            return true;
        }

        public bool TryResolve(
            string requestedMapId,
            out Entry resolvedEntry,
            out string failure)
        {
            resolvedEntry = null;
            if (!BattleMapDefinitionValidation.TryValidateMapId(requestedMapId, out failure))
                return false;

            if (!TryValidate(out failure))
                return false;

            for (int index = 0; index < entries.Count; index++)
            {
                Entry entry = entries[index];
                if (!BattleMapDefinitionValidation.MapIdsMatch(entry.MapId, requestedMapId))
                    continue;

                resolvedEntry = entry;
                failure = string.Empty;
                return true;
            }

            failure = "Map catalog does not contain the requested MapId.";
            return false;
        }

        private static bool TryValidateEntry(Entry entry, out string failure)
        {
            if (entry == null)
            {
                failure = "Map catalog contains a null entry.";
                return false;
            }

            if (!BattleMapDefinitionValidation.TryValidateMapId(entry.MapId, out failure))
                return false;

            if (entry.BoundaryDefinition == null)
            {
                failure = "Map catalog entry has no boundary definition.";
                return false;
            }

            if (entry.PresentationDefinition == null)
            {
                failure = "Map catalog entry has no presentation definition.";
                return false;
            }

            if (!entry.BoundaryDefinition.TryValidate(out failure))
                return false;

            if (!entry.PresentationDefinition.TryValidate(out failure))
                return false;

            if (!BattleMapDefinitionValidation.MapIdsMatch(
                    entry.MapId,
                    entry.BoundaryDefinition.MapId) ||
                !BattleMapDefinitionValidation.MapIdsMatch(
                    entry.MapId,
                    entry.PresentationDefinition.MapId))
            {
                failure = "Map catalog entry MapId does not match its definitions.";
                return false;
            }

            failure = string.Empty;
            return true;
        }
    }
}
