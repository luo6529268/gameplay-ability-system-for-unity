using System.Collections.Generic;
using UnityEngine;

namespace NTSD.LevelEditor
{
    [CreateAssetMenu(
        fileName = "BattleMapPresentation",
        menuName = "NTSD/Maps/Presentation Definition")]
    public sealed class BattleMapPresentationDefinition : ScriptableObject
    {
        [SerializeField] private string mapId = "";
        [SerializeField] private string displayName = "";
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private GameObject[] decorationPrefabs = new GameObject[0];

        public string MapId => mapId;
        public string DisplayName => displayName;
        public Sprite BackgroundSprite => backgroundSprite;
        public IReadOnlyList<GameObject> DecorationPrefabs => decorationPrefabs;

        public bool TryValidate(out string failure)
        {
            return BattleMapDefinitionValidation.TryValidateMapId(mapId, out failure);
        }
    }
}
