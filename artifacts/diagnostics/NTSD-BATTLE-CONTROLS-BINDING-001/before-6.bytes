using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NTSD.UI
{
    /// <summary>
    /// NTSD UI 的通用 Button 交互基类。
    /// 只维护交互状态；Shader、缩放、音效等表现由派生类或订阅组件实现。
    /// </summary>
    [AddComponentMenu("NTSD/UI/NTSD Button")]
    public class NTSDButton : Button
    {
        private readonly HashSet<int> pressedPointers = new HashSet<int>();
        private bool hasApplicationFocus = true;
        private bool applicationPaused;

        public event Action<NTSDButton, bool> PressedStateChanged;

        public bool IsPointerPressed { get; private set; }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left ||
                !IsActive() || !IsInteractable() ||
                !hasApplicationFocus || applicationPaused ||
                !pressedPointers.Add(eventData.pointerId))
            {
                return;
            }

            base.OnPointerDown(eventData);
            if (!IsActive() || !IsInteractable() ||
                !hasApplicationFocus || applicationPaused)
            {
                InstantClearState();
                return;
            }

            SetPressed(pressedPointers.Count > 0);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            ReleasePointer(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            ReleasePointer(eventData);
            base.OnPointerExit(eventData);
        }

        private void ReleasePointer(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left ||
                !pressedPointers.Remove(eventData.pointerId) || pressedPointers.Count > 0)
                return;

            // Keep Selectable's single down flag until the final held pointer leaves.
            base.OnPointerUp(eventData);
            SetPressed(false);
        }

        protected override void OnDisable()
        {
            InstantClearState();
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            InstantClearState();
            base.OnDestroy();
        }

        protected override void InstantClearState()
        {
            pressedPointers.Clear();
            base.InstantClearState();
            SetPressed(false);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (state == SelectionState.Disabled && pressedPointers.Count > 0)
                InstantClearState();

            base.DoStateTransition(state, instant);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            hasApplicationFocus = hasFocus;
            if (!hasFocus)
                InstantClearState();
        }

        private void OnApplicationPause(bool paused)
        {
            applicationPaused = paused;
            if (paused)
                InstantClearState();
        }

        protected virtual void OnPressedStateChanged(bool pressed)
        {
        }

        private void SetPressed(bool pressed)
        {
            if (IsPointerPressed == pressed)
                return;

            IsPointerPressed = pressed;
            OnPressedStateChanged(pressed);
            PressedStateChanged?.Invoke(this, pressed);
        }
    }
}
