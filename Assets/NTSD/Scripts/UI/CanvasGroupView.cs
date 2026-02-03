using UnityEngine;

namespace NTSD.UI
{
    public sealed class CanvasGroupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;

        private void Reset()
        {
            group = GetComponent<CanvasGroup>();
        }

        public void Show(bool interactable = true)
        {
            if (group == null) return;
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = interactable;
        }

        public void Hide()
        {
            if (group == null) return;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        public void SetInputBlocked(bool blocked)
        {
            if (group == null) return;
            group.blocksRaycasts = blocked;
            group.interactable = !blocked;
        }
    }
}
