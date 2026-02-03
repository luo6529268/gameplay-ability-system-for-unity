using UnityEngine;

namespace NTSD.UI.Menu
{
    public class MenuOptionShowHide : MenuOptionBase
    {
        protected override void OnInitialize()
        {
            if (this.transform.childCount > 0)
                SetHighlightObject(this.transform.GetChild(0).gameObject);
        }
    }
}
