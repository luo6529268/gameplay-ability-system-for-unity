using System.Collections.Generic;
using MoreMountains.Feedbacks;
using NTSD.App;
using UnityEngine;
using UnityEngine.Events;

namespace NTSD.UI
{
    public sealed class CharacterSelectionBoard : MonoBehaviour
    {
        [SerializeField] private CharacterSelectionController selection;
        [SerializeField] private GameObject characterList;
        [SerializeField] private Transform content;
        [SerializeField] private CharacterChoiceItem template;
        [SerializeField] private GameObject teamList;
        [SerializeField] private UnityEngine.UI.Button[] teamButtons;
        [SerializeField] private GameObject[] teamSelections;

        private readonly List<CharacterChoiceItem> cells = new List<CharacterChoiceItem>();
        private readonly List<SelectRoleItem> subscribedSlots = new List<SelectRoleItem>();
        private UnityAction[] teamActions;
        private MMMiniObjectPooler pool;
        private Sprite randomCandidateSprite;
        private SelectRoleItem activePlayer;

        public SelectRoleItem ActivePlayer => activePlayer;
        public IReadOnlyList<CharacterChoiceItem> Choices => cells;

        private void Awake()
        {
            if (template != null)
            {
                randomCandidateSprite = template.IconSprite;
                template.gameObject.SetActive(false);
            }
            if (teamButtons == null) return;
            teamActions = new UnityAction[teamButtons.Length];
            for (int i = 0; i < teamActions.Length; i++)
            {
                int index = i;
                teamActions[i] = () => SelectTeam(index);
            }
        }

        private void OnEnable()
        {
            if (selection != null && selection.PlayerSlots != null)
            {
                foreach (SelectRoleItem slot in selection.PlayerSlots)
                {
                    if (slot == null) continue;
                    subscribedSlots.Add(slot);
                    slot.SelectionChanged += OnSelectionChanged;
                    slot.FocusRequested += OnFocusRequested;
                }
            }
            if (teamActions != null)
                for (int i = 0; i < teamActions.Length; i++)
                    if (teamButtons[i] != null) teamButtons[i].onClick.AddListener(teamActions[i]);
            activePlayer = FindJoinedPlayer();
            Refresh();
        }

        private void OnDisable()
        {
            foreach (SelectRoleItem slot in subscribedSlots)
            {
                if (slot == null) continue;
                slot.SelectionChanged -= OnSelectionChanged;
                slot.FocusRequested -= OnFocusRequested;
            }
            subscribedSlots.Clear();
            if (teamActions != null)
                for (int i = 0; i < teamActions.Length; i++)
                    if (teamButtons[i] != null) teamButtons[i].onClick.RemoveListener(teamActions[i]);
            ReleaseCells();
            activePlayer = null;
            if (characterList != null) characterList.SetActive(false);
            if (teamList != null) teamList.SetActive(false);
            if (teamSelections != null)
                foreach (GameObject frame in teamSelections)
                    if (frame != null) frame.SetActive(false);
        }

        private SelectRoleItem FindJoinedPlayer()
        {
            foreach (SelectRoleItem slot in subscribedSlots)
                if (slot != null && slot.isActiveAndEnabled && slot.State != SelectRoleState.Idle) return slot;
            return null;
        }

        private void OnSelectionChanged(SelectRoleItem slot)
        {
            if (!isActiveAndEnabled) return;
            if (slot.isActiveAndEnabled && slot.State != SelectRoleState.Idle)
                activePlayer = slot;
            else if (activePlayer == slot)
                activePlayer = FindJoinedPlayer();
            Refresh();
        }

        private void OnFocusRequested(SelectRoleItem slot) => FocusPlayer(slot);

        public bool FocusPlayer(SelectRoleItem slot)
        {
            if (!isActiveAndEnabled || slot == null || !slot.isActiveAndEnabled ||
                slot.State == SelectRoleState.Idle || !subscribedSlots.Contains(slot)) return false;
            activePlayer = slot;
            Refresh();
            return true;
        }

        private void HandleCharacterClick(int characterId)
        {
            if (activePlayer == null || !activePlayer.isActiveAndEnabled) return;
            if (activePlayer.State == SelectRoleState.SelectingTeam)
            {
                if (activePlayer.SelectedCharacterId == characterId) activePlayer.OnCancel();
                return;
            }
            if (activePlayer.State != SelectRoleState.SelectingCharacter) return;
            if (activePlayer.SelectedCharacterId == characterId)
                activePlayer.OnConfirmCharacter();
            else
                activePlayer.SelectCharacter(characterId);
        }

        private void SelectTeam(int index)
        {
            if (isActiveAndEnabled && activePlayer != null && activePlayer.isActiveAndEnabled)
                activePlayer.SelectTeam(index);
        }

        private void Refresh()
        {
            bool joined = activePlayer != null;
            if (characterList != null) characterList.SetActive(joined);
            if (joined) RefreshCells();
            else ReleaseCells();
            bool confirmed = joined && activePlayer.State == SelectRoleState.SelectingTeam;
            if (teamList != null) teamList.SetActive(confirmed);
            var config = GameConfig.Instance;
            if (teamButtons != null)
            {
                for (int i = 0; i < teamButtons.Length; i++)
                {
                    bool available = config != null && config.TeamOptions != null && i < config.TeamOptions.Length;
                    if (teamButtons[i] != null) teamButtons[i].interactable = confirmed && available;
                }
            }
            if (teamSelections != null)
                for (int i = 0; i < teamSelections.Length; i++)
                    if (teamSelections[i] != null)
                        teamSelections[i].SetActive(confirmed && activePlayer.SelectedTeamIndex == i);
            foreach (CharacterChoiceItem cell in cells)
                cell.ShowSelection(joined && cell.CharacterId == activePlayer.SelectedCharacterId, confirmed);
        }

        private void RefreshCells()
        {
            IReadOnlyList<int> ids = activePlayer.AvailableCharacterIds;
            if (template == null || content == null || ids == null) return;
            bool rebuild = cells.Count != ids.Count;
            if (!rebuild)
                for (int i = 0; i < ids.Count; i++)
                    if (cells[i].CharacterId != ids[i]) { rebuild = true; break; }
            if (rebuild)
            {
                ReleaseCells();
                EnsurePool();
                for (int i = 0; i < ids.Count; i++)
                {
                    GameObject item = pool.GetPooledGameObject();
                    if (item == null) break;
                    item.transform.SetParent(content, false);
                    item.transform.SetAsLastSibling();
                    item.transform.localScale = Vector3.one;
                    var cell = item.GetComponent<CharacterChoiceItem>();
                    cells.Add(cell);
                    item.SetActive(true);
                }
            }
            var resources = CharacterUIResourceManager.TryGetInstance();
            for (int i = 0; i < cells.Count; i++)
            {
                int id = ids[i];
                Sprite sprite = id == GameConfig.RandomCharacterId ? randomCandidateSprite
                    : resources != null ? resources.GetCharacterUISprites(id)?.SmallSprite : null;
                cells[i].Bind(id, sprite, HandleCharacterClick);
            }
        }

        private void EnsurePool()
        {
            if (pool != null) return;
            var owner = new GameObject("CharacterChoicePool");
            owner.SetActive(false);
            owner.transform.SetParent(transform, false);
            pool = owner.AddComponent<MMMiniObjectPooler>();
            pool.GameObjectToPool = template.gameObject;
            pool.PoolSize = 0;
            pool.PoolCanExpand = true;
            pool.MutualizeWaitingPools = false;
            owner.SetActive(true);
        }

        private void ReleaseCells()
        {
            foreach (CharacterChoiceItem cell in cells)
                if (cell != null) cell.Release();
            cells.Clear();
        }
    }
}
