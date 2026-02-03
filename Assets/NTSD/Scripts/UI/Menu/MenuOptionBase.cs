using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace NTSD.UI.Menu
{
    public enum HighlightType
    {
        ShowHide,
        Color,
    }

    public class MenuOptionBase : MonoBehaviour
    {
        [SerializeField] private HighlightType highlightType;

        [SerializeField] private GameObject highlightObject;

        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;

        [SerializeField] protected AudioClip selectSound;
        [SerializeField] protected AudioClip confirmSound;

        private bool _isInitialized = false;

        protected virtual void Awake()
        {
            //if (_isInitialized)
            //{
            //    return;
            //}

            //EnsureInitialized();
            //SetSelected(false);
        }

        public void EnsureInitialized()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void SetHighlightObject(GameObject highlightObject) 
        {
            this.highlightObject = highlightObject;
        }

        public virtual void SetSelected(bool selected)
        {
            EnsureInitialized();

            switch (highlightType)
            {
                case HighlightType.ShowHide:
                    if (highlightObject != null)
                        highlightObject.SetActive(selected);
                    break;

                case HighlightType.Color:
                    if (targetGraphic != null)
                        targetGraphic.color = selected ? selectedColor : normalColor;
                    break;
            }
        }

        public virtual void PlaySelectSound()
        {
            if (selectSound == null) return;
            MMSoundManagerSoundPlayEvent.Trigger(selectSound, MMSoundManagerPlayOptions.Default);
        }

        public virtual void PlayConfirmSound()
        {
            if (confirmSound == null) return;
            MMSoundManagerSoundPlayEvent.Trigger(confirmSound, MMSoundManagerPlayOptions.Default);
        }

    }
}
