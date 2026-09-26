using System;
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
        public event Action<NTSDButton, bool> PressedStateChanged;

        public bool IsPointerPressed { get; private set; }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (IsActive() && IsInteractable())
                SetPressed(true);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            SetPressed(false);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            SetPressed(false);
        }

        protected override void OnDisable()
        {
            SetPressed(false);
            base.OnDisable();
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
