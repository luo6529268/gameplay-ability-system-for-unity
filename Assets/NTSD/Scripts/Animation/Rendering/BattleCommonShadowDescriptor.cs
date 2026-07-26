using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation.Rendering
{
    /// <summary>
    /// Serialized resource and render-state contract for the central common shadow.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleCommonShadowDescriptor : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Material material;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private bool flipX;
        [SerializeField] private bool flipY;
        [SerializeField] private SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None;

        public Sprite Sprite => sprite;
        public Material Material => material;
        public Color Color => color;
        public bool FlipX => flipX;
        public bool FlipY => flipY;
        public SpriteMaskInteraction MaskInteraction => maskInteraction;

        public bool TryValidate(out string diagnostic)
        {
            if (sprite == null)
            {
                diagnostic = "GameConfig.ShadowPrefab BattleCommonShadowDescriptor is missing its Sprite.";
                return false;
            }

            Texture2D texture = sprite.texture;
            if (texture == null)
            {
                diagnostic = "GameConfig.ShadowPrefab BattleCommonShadowDescriptor Sprite has no Texture2D.";
                return false;
            }

            if (BattleSpriteMaterialContract.Classify(material) !=
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha)
            {
                diagnostic = "GameConfig.ShadowPrefab BattleCommonShadowDescriptor Material does not declare premultiplied sprite alpha semantics.";
                return false;
            }

            if (maskInteraction != SpriteMaskInteraction.None)
            {
                diagnostic = "GameConfig.ShadowPrefab BattleCommonShadowDescriptor uses an unsupported mask interaction.";
                return false;
            }

            Rect pixelRect = sprite.rect;
            if (pixelRect.width <= 0f || pixelRect.height <= 0f ||
                texture.width <= 0 || texture.height <= 0)
            {
                diagnostic = "GameConfig.ShadowPrefab BattleCommonShadowDescriptor Sprite has invalid pixel metrics.";
                return false;
            }

            diagnostic = string.Empty;
            return true;
        }

        internal void ConfigureForSelfCheck(
            Sprite configuredSprite,
            Material configuredMaterial,
            Color configuredColor,
            bool configuredFlipX = false,
            bool configuredFlipY = false,
            SpriteMaskInteraction configuredMaskInteraction = SpriteMaskInteraction.None)
        {
            sprite = configuredSprite;
            material = configuredMaterial;
            color = configuredColor;
            flipX = configuredFlipX;
            flipY = configuredFlipY;
            maskInteraction = configuredMaskInteraction;
        }
    }
}
