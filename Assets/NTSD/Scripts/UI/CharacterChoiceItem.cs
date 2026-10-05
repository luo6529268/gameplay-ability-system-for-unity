using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NTSD.UI
{
    public sealed class CharacterChoiceItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private UnityEngine.UI.Image icon;
        [SerializeField] private GameObject selectionFrame;
        [SerializeField] private TextMeshProUGUI teamIndexText;
        private Action<int> clicked;

        public int CharacterId { get; private set; }
        public Sprite IconSprite => icon != null ? icon.sprite : null;

        public void Bind(int characterId, Sprite sprite, Action<int> onClick)
        {
            CharacterId = characterId;
            clicked = onClick;
            if (icon != null) icon.sprite = sprite;
            ShowSelection(false, false);
        }

        public void ShowSelection(bool selected, bool confirmed)
        {
            if (selectionFrame != null) selectionFrame.SetActive(selected);
            if (teamIndexText != null)
            {
                teamIndexText.text = selected && confirmed ? "取消" : string.Empty;
                teamIndexText.gameObject.SetActive(selected && confirmed);
            }
        }

        public void Release()
        {
            clicked = null;
            ShowSelection(false, false);
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isActiveAndEnabled && eventData.button == PointerEventData.InputButton.Left)
                clicked?.Invoke(CharacterId);
        }
    }
}
