using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.UI.Menu
{
    public class MenuOptionList : MonoBehaviour, IMenuFocusable
    {
        [Header("Options")]
        [SerializeField] private List<MenuOptionBase> options;

        [Header("Settings")]
        [SerializeField] private bool wrapAround = true;
        [SerializeField] private bool handleCancel = false;
        [SerializeField] private bool verticalSelect = true;
        public int CurrentIndex { get; private set; }

        public event Action OnCancelled;
        public event Action<int> OnOptionConfirmed;

        private void OnEnable()
        {
            CurrentIndex = 0;
            MenuFocusManager.Instance?.Push(this);
        }

        private void OnDisable()
        {
            MenuFocusManager.Instance?.Pop();
        }

        public void OnFocusEnter()
        {
            UpdateAllSelections();
        }

        public void OnFocusExit()
        {
        }

        public void OnNavigate(Vector2 direction)
        {
            if (options == null || options.Count == 0) return;

            if (verticalSelect && direction.y == 0)
                return;

            if (!verticalSelect && direction.x == 0)
                return;


            int newIndex = CurrentIndex;

            if (verticalSelect)
                newIndex += direction.y > 0.5f ? -1 : 1;
            else
                newIndex += direction.x > 0.5f ? 1 : -1;

            if (newIndex < 0)
            {
                newIndex = wrapAround ? options.Count - 1 : 0;
            }
            else if (newIndex >= options.Count)
            {
                newIndex = wrapAround ? 0 : options.Count - 1;
            }

            if (newIndex != CurrentIndex)
            {
                CurrentIndex = newIndex;
                UpdateAllSelections();
                options[CurrentIndex]?.PlaySelectSound();
            }
        }

        public void OnConfirm()
        {
            if (options == null || CurrentIndex < 0 || CurrentIndex >= options.Count) return;

            var option = options[CurrentIndex];
            option?.PlayConfirmSound();

            OnOptionConfirmed?.Invoke(CurrentIndex);
        }

        public bool OnCancel()
        {
            if (!handleCancel) return false;

            OnCancelled?.Invoke();
            return true;
        }

        private void UpdateAllSelections()
        {
            if (options == null) return;

            for (int i = 0; i < options.Count; i++)
            {
                options[i]?.SetSelected(i == CurrentIndex);
            }
        }
    }
}
