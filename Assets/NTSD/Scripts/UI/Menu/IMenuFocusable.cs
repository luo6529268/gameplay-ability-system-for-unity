using UnityEngine;

namespace NTSD.UI.Menu
{
    public interface IMenuFocusable
    {
        void OnFocusEnter();
        void OnFocusExit();
        void OnNavigate(Vector2 direction);
        void OnConfirm();
        bool OnCancel();
    }
}
