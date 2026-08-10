using UnityEngine;

namespace NTSD.Simulation.Presentation
{
    /// <summary>
    /// Presentation-owned classifier for the known battle materials. Shader
    /// references are resolved before battle so the hot path never reads
    /// Shader.name or creates managed strings.
    /// </summary>
    public sealed class BattleSpriteMaterialClassifier
    {
        private readonly Shader builtInSpriteShader;
        private readonly Shader centralTextureShader;
        private readonly Shader centralArrayShader;
        private readonly int colorId;

        public BattleSpriteMaterialClassifier()
        {
            builtInSpriteShader = Shader.Find(
                BattleSpriteMaterialContract.BuiltInSpriteShaderName);
            centralTextureShader = Shader.Find(
                BattleSpriteMaterialContract.CentralTextureShaderName);
            centralArrayShader = Shader.Find(
                BattleSpriteMaterialContract.CentralArrayShaderName);
            colorId = Shader.PropertyToID("_Color");
        }

        public BattleSpriteMaterialSemantic Classify(Material material)
        {
            if (material == null)
                return BattleSpriteMaterialSemantic.Unsupported;

            Shader shader = material.shader;
            if (shader == null ||
                (shader != builtInSpriteShader &&
                 shader != centralTextureShader &&
                 shader != centralArrayShader))
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            if (!material.HasProperty(colorId) ||
                !IsWhite(material.GetColor(colorId)) ||
                material.IsKeywordEnabled("PIXELSNAP_ON"))
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            return BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
        }

        public bool IsDeclaredCentralMaterial(Material material, bool textureArray)
        {
            if (material == null)
                return false;

            Shader expectedShader = textureArray
                ? centralArrayShader
                : centralTextureShader;
            return material.shader == expectedShader &&
                   Classify(material) ==
                   BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
        }

        private static bool IsWhite(Color color)
        {
            const float epsilon = 0.000001f;
            return Mathf.Abs(color.r - 1f) <= epsilon &&
                   Mathf.Abs(color.g - 1f) <= epsilon &&
                   Mathf.Abs(color.b - 1f) <= epsilon &&
                   Mathf.Abs(color.a - 1f) <= epsilon;
        }
    }
}
