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
        public int OptionCount => options == null ? 0 : options.Count;
        public bool HasFocus { get; private set; }

        private MenuLoopCarousel carousel;
        private int lastConfirmFrame = -1;

        public MenuOptionBase GetOption(int index) => options[index];

        public void BindCarousel(MenuLoopCarousel owner)
        {
            carousel = owner;
        }

        public void UnbindCarousel(MenuLoopCarousel owner)
        {
            if (carousel == owner) carousel = null;
        }

        public void SelectIndex(int index)
        {
            if (options == null || index < 0 || index >= options.Count || index == CurrentIndex) return;
            CurrentIndex = index;
            UpdateAllSelections();
            options[CurrentIndex]?.PlaySelectSound();
        }

        public event Action OnCancelled;
        public event Action<int> OnOptionConfirmed;

        private void OnEnable()
        {
            CurrentIndex = 0;
            lastConfirmFrame = -1;
            if (carousel != null) carousel.ResetSelection();
            MenuFocusManager.Instance?.Push(this);
        }

        private void OnDisable()
        {
            HasFocus = false;
            MenuFocusManager.Instance?.Pop();
        }

        public void OnFocusEnter()
        {
            HasFocus = true;
            UpdateAllSelections();
        }

        public void OnFocusExit()
        {
            HasFocus = false;
        }

        public void OnNavigate(Vector2 direction)
        {
            if (options == null || options.Count == 0) return;

            if (verticalSelect && direction.y == 0)
                return;

            if (!verticalSelect && direction.x == 0)
                return;


            if (carousel != null && carousel.isActiveAndEnabled)
            {
                carousel.Navigate(verticalSelect ? (direction.y > 0 ? -1 : 1) : (direction.x > 0 ? 1 : -1));
                return;
            }

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

            if (carousel != null && carousel.isActiveAndEnabled)
            {
                if (lastConfirmFrame == Time.frameCount || !carousel.PrepareConfirm()) return;
                lastConfirmFrame = Time.frameCount;
            }

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
