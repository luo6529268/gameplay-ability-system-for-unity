using UnityEngine;

namespace NTSD.App
{
    /// <summary>
    /// Compatibility shell for scenes created while the former battle-camera adapter existed.
    /// It deliberately owns no presentation, camera, or battle-state behavior.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class BattleCameraSafeArea : MonoBehaviour
    {
    }
}
