using UnityEngine;

namespace NTSD.App
{
    /// <summary>
    /// Compatibility shell for scenes created while platform-specific background variants
    /// existed. Bg (2)'s SpriteRenderer now owns its assigned Sprite directly.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BattleBackgroundPlatformSelector : MonoBehaviour
    {
    }
}
