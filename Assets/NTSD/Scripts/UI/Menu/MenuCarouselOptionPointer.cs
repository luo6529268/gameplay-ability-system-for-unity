using UnityEngine;
using UnityEngine.EventSystems;

namespace NTSD.UI.Menu
{
    [DisallowMultipleComponent]
    public sealed class MenuCarouselOptionPointer : MonoBehaviour, IPointerClickHandler
    {
        private MenuLoopCarousel carousel;
        private int index;

        public void Bind(MenuLoopCarousel owner, int optionIndex)
        {
            carousel = owner;
            index = optionIndex;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || eventData.dragging) return;
            if (carousel != null) carousel.Click(index);
        }
    }
}
