using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NTSD.UI.Menu
{
    [DisallowMultipleComponent]
    public sealed class MenuLoopCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IScrollHandler, IInitializePotentialDragHandler
    {
        [SerializeField, Min(0.01f)] private float snapSeconds = 0.085f;
        [SerializeField, Min(0.1f)] private float wheelItems = 1f;

        private readonly MenuCarouselMotion motion = new MenuCarouselMotion();
        private readonly List<ItemState> items = new List<ItemState>();
        private MenuOptionList list;
        private UnityEngine.UI.ScrollRect scroll;
        private UnityEngine.UI.LayoutGroup layout;
        private UnityEngine.UI.ContentSizeFitter fitter;
        private UnityEngine.UI.Image dragSurface;
        private RectTransform viewport;
        private RectTransform content;
        private RectState contentState;
        private bool scrollEnabled;
        private bool layoutEnabled;
        private bool fitterEnabled;
        private bool initialized;
        private bool ownsLayout;
        private int dragPointerId = int.MinValue;
        private int suppressClickThroughFrame = -1;
        private Vector2 lastDragPoint;
        private float itemSpacing;
        private float originalSpacing;

        public MenuCarouselMotion Motion => motion;
        public float ItemSpacing => itemSpacing;
        public bool IsDragging => motion.IsDragging;
        public bool AcceptsInput => ownsLayout && list != null && list.HasFocus && isActiveAndEnabled;

        private sealed class ItemState
        {
            public RectTransform Rect;
            public RectState State;
            public MenuCarouselOptionPointer Pointer;
            public bool PointerEnabled;
        }

        private struct RectState
        {
            public Vector2 Min, Max, Pivot, Size, Position;
            public Vector3 Scale;

            public RectState(RectTransform rect)
            {
                Min = rect.anchorMin;
                Max = rect.anchorMax;
                Pivot = rect.pivot;
                Size = rect.sizeDelta;
                Position = rect.anchoredPosition;
                Scale = rect.localScale;
            }

            public void Restore(RectTransform rect)
            {
                rect.anchorMin = Min;
                rect.anchorMax = Max;
                rect.pivot = Pivot;
                rect.sizeDelta = Size;
                rect.anchoredPosition = Position;
                rect.localScale = Scale;
            }
        }

        public bool Configure(MenuOptionList owner, UnityEngine.UI.ScrollRect source)
        {
            if (initialized) return list == owner;
            if (owner == null || source == null || source.content == null || owner.OptionCount < 2) return false;
            RectTransform sourceViewport = source.viewport != null
                ? source.viewport : source.transform as RectTransform;
            if (sourceViewport == null) return false;

            for (int i = 0; i < owner.OptionCount; i++)
            {
                MenuOptionBase option = owner.GetOption(i);
                if (option == null || option.transform.parent != source.content ||
                    !(option.transform is RectTransform)) return false;
            }

            list = owner;
            scroll = source;
            viewport = sourceViewport;
            content = source.content;
            layout = content.GetComponent<UnityEngine.UI.LayoutGroup>();
            fitter = content.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            var vertical = layout as UnityEngine.UI.VerticalLayoutGroup;
            originalSpacing = vertical != null ? vertical.spacing : 50f;

            for (int i = 0; i < list.OptionCount; i++)
            {
                var rect = (RectTransform)list.GetOption(i).transform;
                var pointer = rect.GetComponent<MenuCarouselOptionPointer>();
                if (pointer == null) pointer = rect.gameObject.AddComponent<MenuCarouselOptionPointer>();
                pointer.Bind(this, i);
                items.Add(new ItemState { Rect = rect, Pointer = pointer, PointerEnabled = pointer.enabled });
            }

            // A transparent raycast surface lets empty space start a drag as well.
            var surface = new GameObject("CarouselDragSurface", typeof(RectTransform),
                typeof(UnityEngine.UI.Image));
            surface.layer = gameObject.layer;
            surface.transform.SetParent(transform, false);
            surface.transform.SetAsFirstSibling();
            var surfaceRect = (RectTransform)surface.transform;
            surfaceRect.anchorMin = Vector2.zero;
            surfaceRect.anchorMax = Vector2.one;
            surfaceRect.offsetMin = surfaceRect.offsetMax = Vector2.zero;
            dragSurface = surface.GetComponent<UnityEngine.UI.Image>();
            dragSurface.color = Color.clear;
            dragSurface.raycastTarget = true;

            initialized = true;
            list.BindCarousel(this);
            if (isActiveAndEnabled) AcquireLayout();
            return true;
        }

        private void OnEnable()
        {
            if (initialized) AcquireLayout();
        }

        private void OnDisable()
        {
            ReleaseLayout();
        }

        private void OnDestroy()
        {
            ReleaseLayout();
            if (list != null) list.UnbindCarousel(this);
            if (dragSurface != null)
            {
                if (Application.isPlaying) Destroy(dragSurface.gameObject);
                else DestroyImmediate(dragSurface.gameObject);
            }
        }

        private void AcquireLayout()
        {
            if (ownsLayout) return;
            contentState = new RectState(content);
            scrollEnabled = scroll.enabled;
            layoutEnabled = layout != null && layout.enabled;
            fitterEnabled = fitter != null && fitter.enabled;
            scroll.StopMovement();
            scroll.enabled = false;
            if (layout != null) layout.enabled = false;
            if (fitter != null) fitter.enabled = false;
            foreach (ItemState item in items)
            {
                item.State = new RectState(item.Rect);
                item.Pointer.enabled = true;
            }
            ownsLayout = true;
            dragSurface.gameObject.SetActive(true);
            ResetSelection();
        }

        private void ReleaseLayout()
        {
            if (!ownsLayout) return;
            ownsLayout = false;
            dragPointerId = int.MinValue;
            motion.Reset(items.Count, 0);
            foreach (ItemState item in items)
            {
                if (item.Rect != null) item.State.Restore(item.Rect);
                if (item.Pointer != null) item.Pointer.enabled = item.PointerEnabled;
            }
            if (content != null) contentState.Restore(content);
            if (layout != null) layout.enabled = layoutEnabled;
            if (fitter != null) fitter.enabled = fitterEnabled;
            if (scroll != null) scroll.enabled = scrollEnabled;
            if (dragSurface != null) dragSurface.gameObject.SetActive(false);
        }

        public void ResetSelection()
        {
            if (!ownsLayout) return;
            dragPointerId = int.MinValue;
            suppressClickThroughFrame = -1;
            motion.Reset(items.Count, list.CurrentIndex);
            RefreshLayout();
        }

        private void LateUpdate()
        {
            if (!ownsLayout) return;
            if (!list.HasFocus && motion.IsDragging) CancelDrag();
            motion.Advance(Time.unscaledDeltaTime, snapSeconds);
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            float maximumHeight = 1f;
            foreach (ItemState item in items)
                maximumHeight = Mathf.Max(maximumHeight, item.State.Size.y * Mathf.Abs(item.State.Scale.y));
            // Alignment contract: NTSD-MENU-LOOP-CAROUSEL-001.
            // The wrapped item switches ends only when its entire rect is outside the viewport.
            itemSpacing = Mathf.Max(maximumHeight + originalSpacing,
                (viewport.rect.height + maximumHeight * 2f) / items.Count);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = Vector2.zero;
            content.anchoredPosition = Vector2.zero;
            for (int i = 0; i < items.Count; i++)
            {
                RectTransform rect = items[i].Rect;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, -(float)motion.Offset(i) * itemSpacing);
            }
        }

        public void Navigate(int direction)
        {
            if (!AcceptsInput || motion.IsDragging) return;
            motion.Move(direction);
            list.SelectIndex(motion.SelectedIndex);
        }

        public bool PrepareConfirm()
        {
            if (!AcceptsInput || motion.IsDragging || Time.frameCount <= suppressClickThroughFrame) return false;
            motion.FinishSnap();
            RefreshLayout();
            return true;
        }

        public void Click(int index)
        {
            if (!AcceptsInput || motion.IsDragging || Time.frameCount <= suppressClickThroughFrame) return;
            if (index == list.CurrentIndex && motion.IsSettled)
            {
                list.OnConfirm();
                return;
            }
            motion.Select(index);
            list.SelectIndex(motion.SelectedIndex);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!AcceptsInput || motion.IsDragging || eventData.button != PointerEventData.InputButton.Left) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position,
                eventData.pressEventCamera, out lastDragPoint)) return;
            dragPointerId = eventData.pointerId;
            motion.BeginDrag();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!AcceptsInput || !motion.IsDragging || eventData.pointerId != dragPointerId) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position,
                eventData.pressEventCamera, out Vector2 localPoint)) return;
            motion.Drag((localPoint.y - lastDragPoint.y) / Mathf.Max(1f, itemSpacing));
            lastDragPoint = localPoint;
            list.SelectIndex(motion.SelectedIndex);
            RefreshLayout();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!motion.IsDragging || eventData.pointerId != dragPointerId) return;
            eventData.eligibleForClick = false;
            CancelDrag();
        }

        private void CancelDrag()
        {
            dragPointerId = int.MinValue;
            motion.EndDrag();
            suppressClickThroughFrame = Time.frameCount + 1;
            list.SelectIndex(motion.SelectedIndex);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!AcceptsInput || motion.IsDragging || Mathf.Abs(eventData.scrollDelta.y) < 0.001f) return;
            int steps = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(eventData.scrollDelta.y) * wheelItems));
            Navigate(eventData.scrollDelta.y > 0 ? -steps : steps);
        }
    }
}
