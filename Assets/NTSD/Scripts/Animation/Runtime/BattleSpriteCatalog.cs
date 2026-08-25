using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation
{
    public enum BattleSpriteCentralBindingMode : byte
    {
        SourceTexture2D = 0,
        AtlasTextureArray = 1,
        AtlasPageTexture2D = 2,
    }

    public readonly struct BattleSpriteCentralBinding
    {
        public BattleSpriteCentralBinding(
            BattleSpriteCentralBindingMode mode,
            Texture texture,
            int atlasSlice,
            Rect normalizedUv,
            Rect atlasContentPixelRect,
            int atlasPageIndex = -1)
        {
            Mode = mode;
            Texture = texture;
            AtlasSlice = atlasSlice;
            AtlasPageIndex = atlasPageIndex;
            NormalizedUv = normalizedUv;
            AtlasContentPixelRect = atlasContentPixelRect;
        }

        public BattleSpriteCentralBindingMode Mode { get; }
        public Texture Texture { get; }
        public int AtlasSlice { get; }
        public int AtlasPageIndex { get; }
        public Rect NormalizedUv { get; }
        public Rect AtlasContentPixelRect { get; }
        public bool IsValid
        {
            get
            {
                if ((Mode != BattleSpriteCentralBindingMode.SourceTexture2D &&
                     Mode != BattleSpriteCentralBindingMode.AtlasTextureArray &&
                     Mode != BattleSpriteCentralBindingMode.AtlasPageTexture2D) ||
                    Texture == null || Texture.width <= 0 || Texture.height <= 0 ||
                    !IsFiniteRect(NormalizedUv) ||
                    NormalizedUv.x < 0f || NormalizedUv.y < 0f ||
                    NormalizedUv.width <= 0f || NormalizedUv.height <= 0f ||
                    NormalizedUv.xMax > 1f || NormalizedUv.yMax > 1f ||
                    !IsFiniteRect(AtlasContentPixelRect) ||
                    AtlasContentPixelRect.x < 0f || AtlasContentPixelRect.y < 0f ||
                    AtlasContentPixelRect.width <= 0f || AtlasContentPixelRect.height <= 0f ||
                    AtlasContentPixelRect.xMax > Texture.width ||
                    AtlasContentPixelRect.yMax > Texture.height)
                {
                    return false;
                }

                switch (Mode)
                {
                    case BattleSpriteCentralBindingMode.SourceTexture2D:
                        if (!(Texture is Texture2D) || AtlasSlice != 0 || AtlasPageIndex != -1)
                            return false;
                        break;
                    case BattleSpriteCentralBindingMode.AtlasTextureArray:
                        if (!(Texture is Texture2DArray arrayTexture) ||
                            AtlasSlice < 0 || AtlasSlice >= arrayTexture.depth ||
                            AtlasPageIndex < 0 || AtlasPageIndex != AtlasSlice)
                        {
                            return false;
                        }
                        break;
                    case BattleSpriteCentralBindingMode.AtlasPageTexture2D:
                        if (!(Texture is Texture2D) || AtlasSlice != 0 || AtlasPageIndex < 0)
                            return false;
                        break;
                    default:
                        return false;
                }

                const float epsilon = 0.001f;
                return Mathf.Abs(NormalizedUv.x * Texture.width - AtlasContentPixelRect.x) <= epsilon &&
                       Mathf.Abs(NormalizedUv.y * Texture.height - AtlasContentPixelRect.y) <= epsilon &&
                       Mathf.Abs(NormalizedUv.width * Texture.width - AtlasContentPixelRect.width) <= epsilon &&
                       Mathf.Abs(NormalizedUv.height * Texture.height - AtlasContentPixelRect.height) <= epsilon;
            }
        }

        private static bool IsFiniteRect(Rect rect)
        {
            return IsFinite(rect.x) && IsFinite(rect.y) &&
                   IsFinite(rect.width) && IsFinite(rect.height);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    /// <summary>
    /// Stable lookup key for a battle visual. ObjectId is intentionally not used
    /// here because an entity may replace its current DAT wrapper at runtime.
    /// </summary>
    public readonly struct BattleSpriteKey : IEquatable<BattleSpriteKey>
    {
        public readonly int VisualDataId;
        public readonly int EffectivePic;

        public BattleSpriteKey(int visualDataId, int effectivePic)
        {
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
        }

        public bool Equals(BattleSpriteKey other) =>
            VisualDataId == other.VisualDataId && EffectivePic == other.EffectivePic;

        public override bool Equals(object obj) => obj is BattleSpriteKey other && Equals(other);

        public override int GetHashCode() => unchecked((VisualDataId * 397) ^ EffectivePic);

        public static bool operator ==(BattleSpriteKey left, BattleSpriteKey right) => left.Equals(right);
        public static bool operator !=(BattleSpriteKey left, BattleSpriteKey right) => !left.Equals(right);

        public override string ToString() => $"({VisualDataId},{EffectivePic})";
    }

    public enum BattleVisualResourceKind : byte
    {
        None = 0,
        EntitySprite = 1,
        CommonShadow = 2,
        CommonSpark = 3,
        CommonWordGlyph = 4,
        CommonSpecialCom = 5,
    }

    public readonly struct BattleVisualResourceKey : IEquatable<BattleVisualResourceKey>
    {
        private readonly BattleSpriteKey entitySpriteKey;
        private readonly int commonSparkPic;
        private readonly int commonWordSheetIndex;
        private readonly int commonWordCharCode;

        private BattleVisualResourceKey(
            BattleVisualResourceKind kind,
            BattleSpriteKey entityKey,
            int commonSparkPic = -1,
            int commonWordSheetIndex = -1,
            int commonWordCharCode = -1)
        {
            Kind = kind;
            entitySpriteKey = entityKey;
            this.commonSparkPic = commonSparkPic;
            this.commonWordSheetIndex = commonWordSheetIndex;
            this.commonWordCharCode = commonWordCharCode;
        }

        public BattleVisualResourceKind Kind { get; }
        public BattleSpriteKey EntitySpriteKey => entitySpriteKey;
        public bool IsEntitySprite => Kind == BattleVisualResourceKind.EntitySprite;
        public bool IsCommonSpark => Kind == BattleVisualResourceKind.CommonSpark;
        public bool IsCommonWordGlyph => Kind == BattleVisualResourceKind.CommonWordGlyph;
        public bool IsCommonSpecialCom => Kind == BattleVisualResourceKind.CommonSpecialCom;
        public int CommonSparkPic => commonSparkPic;
        public int CommonWordSheetIndex => commonWordSheetIndex;
        public int CommonWordCharCode => commonWordCharCode;
        public static BattleVisualResourceKey CommonShadow { get; } =
            new BattleVisualResourceKey(BattleVisualResourceKind.CommonShadow, default);
        public static BattleVisualResourceKey CommonSpecialCom { get; } =
            CommonComLabel(BattleCommonVisualCatalog.SpecialComSheetIndex);

        public static BattleVisualResourceKey CommonComLabel(int sheetIndex)
        {
            if (sheetIndex < 0 || sheetIndex >= BattleCommonVisualCatalog.WordSheetCount)
                throw new ArgumentOutOfRangeException(nameof(sheetIndex));
            return new BattleVisualResourceKey(
                BattleVisualResourceKind.CommonSpecialCom,
                default,
                -1,
                sheetIndex);
        }

        public static BattleVisualResourceKey CommonSpark(int pic)
        {
            if (pic < 0 || pic >= BattleCommonVisualCatalog.SparkFrameCount)
                throw new ArgumentOutOfRangeException(nameof(pic));
            return new BattleVisualResourceKey(BattleVisualResourceKind.CommonSpark, default, pic);
        }

        public static BattleVisualResourceKey CommonWordGlyph(int sheetIndex, int charCode)
        {
            if (sheetIndex < 0 || sheetIndex >= BattleCommonVisualCatalog.WordSheetCount)
                throw new ArgumentOutOfRangeException(nameof(sheetIndex));
            if (charCode < 0 || charCode >= BattleCommonVisualCatalog.WordGlyphsPerSheet)
                throw new ArgumentOutOfRangeException(nameof(charCode));
            return new BattleVisualResourceKey(
                BattleVisualResourceKind.CommonWordGlyph,
                default,
                -1,
                sheetIndex,
                charCode);
        }

        public static BattleVisualResourceKey FromEntity(BattleSpriteKey key)
        {
            return new BattleVisualResourceKey(BattleVisualResourceKind.EntitySprite, key);
        }

        public bool Equals(BattleVisualResourceKey other)
        {
            return Kind == other.Kind &&
                   (Kind != BattleVisualResourceKind.EntitySprite || entitySpriteKey == other.entitySpriteKey) &&
                   (Kind != BattleVisualResourceKind.CommonSpark || commonSparkPic == other.commonSparkPic) &&
                   (Kind != BattleVisualResourceKind.CommonWordGlyph ||
                    (commonWordSheetIndex == other.commonWordSheetIndex &&
                     commonWordCharCode == other.commonWordCharCode)) &&
                   (Kind != BattleVisualResourceKind.CommonSpecialCom ||
                    commonWordSheetIndex == other.commonWordSheetIndex);
        }

        public override bool Equals(object obj) => obj is BattleVisualResourceKey other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                if (Kind == BattleVisualResourceKind.EntitySprite)
                    return ((int)Kind * 397) ^ entitySpriteKey.GetHashCode();
                if (Kind == BattleVisualResourceKind.CommonWordGlyph)
                    return (((int)Kind * 397) ^ commonWordSheetIndex) * 397 ^ commonWordCharCode;
                if (Kind == BattleVisualResourceKind.CommonSpecialCom)
                    return ((int)Kind * 397) ^ commonWordSheetIndex;
                return ((int)Kind * 397) ^ commonSparkPic;
            }
        }
        public static bool operator ==(BattleVisualResourceKey left, BattleVisualResourceKey right) => left.Equals(right);
        public static bool operator !=(BattleVisualResourceKey left, BattleVisualResourceKey right) => !left.Equals(right);

        public override string ToString()
        {
            if (IsEntitySprite)
                return $"Entity{entitySpriteKey}";
            if (IsCommonSpark)
                return $"CommonSpark({commonSparkPic})";
            if (IsCommonWordGlyph)
                return $"CommonWordGlyph({commonWordSheetIndex},{commonWordCharCode})";
            if (IsCommonSpecialCom)
                return $"CommonComLabel({commonWordSheetIndex})";
            return Kind.ToString();
        }
    }

    public sealed class BattleCommonVisualBinding
    {
        internal BattleCommonVisualBinding(
            BattleVisualResourceKey key,
            Sprite sprite,
            Texture2D texture,
            Material material,
            Rect pixelRect,
            Rect normalizedUv,
            Vector2 pixelSize,
            Vector2 pivot,
            BattleSpriteRenderState renderState)
            : this(
                key,
                sprite,
                texture,
                material,
                pixelRect,
                normalizedUv,
                pixelSize,
                pivot,
                renderState,
                CreateSourceBinding(texture, pixelRect))
        {
        }

        private BattleCommonVisualBinding(
            BattleVisualResourceKey key,
            Sprite sprite,
            Texture2D texture,
            Material material,
            Rect pixelRect,
            Rect normalizedUv,
            Vector2 pixelSize,
            Vector2 pivot,
            BattleSpriteRenderState renderState,
            BattleSpriteCentralBinding centralBinding)
        {
            Key = key;
            Sprite = sprite;
            Texture = texture;
            Material = material;
            PixelRect = pixelRect;
            NormalizedUv = normalizedUv;
            PixelSize = pixelSize;
            Pivot = pivot;
            RenderState = renderState;
            CentralBinding = centralBinding;
        }

        public BattleVisualResourceKey Key { get; }
        public Sprite Sprite { get; }
        public Texture2D Texture { get; }
        public Material Material { get; }
        public Rect PixelRect { get; }
        public Rect NormalizedUv { get; }
        public Vector2 PixelSize { get; }
        public Vector2 Pivot { get; }
        public BattleSpriteRenderState RenderState { get; }
        public BattleSpriteCentralBinding CentralBinding { get; }
        public Color32 Color => RenderState.Color;
        public int SpriteInstanceId => Sprite != null ? Sprite.GetInstanceID() : 0;
        public int TextureInstanceId => Texture != null ? Texture.GetInstanceID() : 0;
        public int MaterialInstanceId => Material != null ? Material.GetInstanceID() : 0;

        public bool MatchesSprite(Sprite sprite)
        {
            return sprite != null && ReferenceEquals(sprite, Sprite) && ReferenceEquals(sprite.texture, Texture) &&
                   sprite.rect == PixelRect;
        }

        public bool MatchesCommand(in BattleSpriteValueDescriptor descriptor)
        {
            return descriptor.HasLogicalResourceKey &&
                   descriptor.LogicalResourceKey == Key &&
                   descriptor.SpriteInstanceId == SpriteInstanceId &&
                   descriptor.TextureInstanceId == TextureInstanceId &&
                   descriptor.PixelRect == PixelRect &&
                   descriptor.PivotNormalized == Pivot;
        }

        internal BattleCommonVisualBinding WithCentralBinding(
            BattleSpriteCentralBinding centralBinding)
        {
            if (!centralBinding.IsValid)
                throw new ArgumentException("Common visual central binding must be valid.", nameof(centralBinding));

            return new BattleCommonVisualBinding(
                Key,
                Sprite,
                Texture,
                Material,
                PixelRect,
                NormalizedUv,
                PixelSize,
                Pivot,
                RenderState,
                centralBinding);
        }

        private static BattleSpriteCentralBinding CreateSourceBinding(
            Texture2D texture,
            Rect pixelRect)
        {
            float width = texture != null ? texture.width : 0f;
            float height = texture != null ? texture.height : 0f;
            Rect uv = width > 0f && height > 0f
                ? new Rect(
                    pixelRect.x / width,
                    pixelRect.y / height,
                    pixelRect.width / width,
                    pixelRect.height / height)
                : Rect.zero;
            return new BattleSpriteCentralBinding(
                BattleSpriteCentralBindingMode.SourceTexture2D,
                texture,
                0,
                uv,
                pixelRect);
        }
    }

    public sealed class BattleCommonVisualCatalog
    {
        public const int SparkFrameCount = 20;
        public const int WordSheetCount = 6;
        public const int WordGlyphsPerSheet = 256;
        public const int WordGlyphWidth = 8;
        public const int WordGlyphHeight = 16;
        public const int WordTextureWidth = 251;
        public const int WordTextureHeight = 257;
        public const int SpecialComSheetIndex = 5;
        public const int SpecialComWidth = 26;
        public const int SpecialComHeight = WordGlyphHeight;
        public const int ComLabelsTextureHeight = WordSheetCount * SpecialComHeight;
        private readonly BattleCommonVisualBinding[] sparks;
        private readonly Texture2D[] wordTextures;
        private readonly BattleCommonVisualBinding[][] wordGlyphs;
        private readonly BattleCommonVisualBinding[] comLabels;

        private BattleCommonVisualCatalog(
            BattleCommonVisualBinding shadow,
            BattleCommonVisualBinding[] sparks,
            Texture2D[] wordTextures,
            BattleCommonVisualBinding[][] wordGlyphs,
            string diagnostic)
            : this(
                shadow,
                sparks,
                wordTextures,
                wordGlyphs,
                (BattleCommonVisualBinding[])null,
                diagnostic)
        {
        }

        private BattleCommonVisualCatalog(
            BattleCommonVisualBinding shadow,
            BattleCommonVisualBinding[] sparks,
            Texture2D[] wordTextures,
            BattleCommonVisualBinding[][] wordGlyphs,
            BattleCommonVisualBinding specialCom,
            string diagnostic)
            : this(
                shadow,
                sparks,
                wordTextures,
                wordGlyphs,
                BuildComLabelsFromSpecial(specialCom),
                diagnostic)
        {
        }

        private BattleCommonVisualCatalog(
            BattleCommonVisualBinding shadow,
            BattleCommonVisualBinding[] sparks,
            Texture2D[] wordTextures,
            BattleCommonVisualBinding[][] wordGlyphs,
            BattleCommonVisualBinding[] comLabels,
            string diagnostic)
        {
            Shadow = shadow;
            this.sparks = sparks ?? Array.Empty<BattleCommonVisualBinding>();
            this.wordTextures = wordTextures ?? Array.Empty<Texture2D>();
            this.wordGlyphs = wordGlyphs ?? Array.Empty<BattleCommonVisualBinding[]>();
            this.comLabels = comLabels ?? Array.Empty<BattleCommonVisualBinding>();
            Diagnostic = diagnostic ?? string.Empty;
        }

        public static BattleCommonVisualCatalog Empty { get; } =
            new BattleCommonVisualCatalog(null, null, null, null,
                "Common shadow, spark, and word bindings have not been published.");

        public BattleCommonVisualBinding Shadow { get; }
        public BattleCommonVisualBinding SpecialCom =>
            comLabels.Length == WordSheetCount ? comLabels[SpecialComSheetIndex] : null;
        public IReadOnlyList<BattleCommonVisualBinding> Sparks => sparks;
        public IReadOnlyList<Texture2D> WordTextures => wordTextures;
        public IReadOnlyList<BattleCommonVisualBinding> ComLabels => comLabels;
        public string Diagnostic { get; }
        public bool IsShadowValid => Shadow != null;
        public bool IsSparkValid => sparks.Length == SparkFrameCount &&
                                     Array.TrueForAll(sparks, binding => binding != null);
        public bool IsWordsValid
        {
            get
            {
                if (wordTextures.Length != WordSheetCount || wordGlyphs.Length != WordSheetCount)
                    return false;

                for (int sheetIndex = 0; sheetIndex < WordSheetCount; sheetIndex++)
                {
                    if (wordTextures[sheetIndex] == null ||
                        wordGlyphs[sheetIndex] == null ||
                        wordGlyphs[sheetIndex].Length != WordGlyphsPerSheet ||
                        Array.Exists(wordGlyphs[sheetIndex], binding => binding == null))
                        return false;
                }

                return true;
            }
        }
        public bool IsValid => IsShadowValid;
        public bool IsRuntimeReady => IsShadowValid && IsSparkValid;
        public bool IsComplete => IsShadowValid && IsSparkValid && IsWordsValid;
        public bool IsSpecialComValid => SpecialCom != null;
        public bool IsComLabelsComplete =>
            comLabels.Length == WordSheetCount &&
            Array.TrueForAll(comLabels, binding => binding != null);

        public bool TryGetSpark(int pic, out BattleCommonVisualBinding binding)
        {
            if (pic >= 0 && pic < sparks.Length)
            {
                binding = sparks[pic];
                return binding != null;
            }

            binding = null;
            return false;
        }

        public bool TryGetSparkKey(Sprite sprite, out BattleVisualResourceKey key)
        {
            if (sprite != null)
            {
                for (int pic = 0; pic < sparks.Length; pic++)
                {
                    BattleCommonVisualBinding binding = sparks[pic];
                    if (binding != null && binding.MatchesSprite(sprite))
                    {
                        key = binding.Key;
                        return true;
                    }
                }
            }

            key = default;
            return false;
        }

        public bool TryGetWordGlyph(int sheetIndex, int charCode, out BattleCommonVisualBinding binding)
        {
            if (sheetIndex >= 0 && sheetIndex < wordGlyphs.Length &&
                charCode >= 0 && charCode < wordGlyphs[sheetIndex].Length)
            {
                binding = wordGlyphs[sheetIndex][charCode];
                return binding != null;
            }

            binding = null;
            return false;
        }

        public bool TryGetSpecialCom(out BattleCommonVisualBinding binding)
        {
            return TryGetComLabel(SpecialComSheetIndex, out binding);
        }

        public bool TryGetComLabel(int sheetIndex, out BattleCommonVisualBinding binding)
        {
            if (sheetIndex >= 0 && sheetIndex < comLabels.Length)
            {
                binding = comLabels[sheetIndex];
                return binding != null;
            }

            binding = null;
            return false;
        }

        public static Vector2 GetSpecialComPivotNormalized() =>
            new Vector2(4f / SpecialComWidth, 0.5f);

        public static Rect GetComLabelPixelRect(int sheetIndex)
        {
            if (sheetIndex < 0 || sheetIndex >= WordSheetCount)
                throw new ArgumentOutOfRangeException(nameof(sheetIndex));
            return new Rect(
                0f,
                sheetIndex * SpecialComHeight,
                SpecialComWidth,
                SpecialComHeight);
        }

        public static Rect GetWordGlyphPixelRect(int charCode)
        {
            if (charCode < 0 || charCode >= WordGlyphsPerSheet)
                return Rect.zero;

            int sourceX = WordGlyphHeight * (charCode % 16);
            int sourceYFromTop = WordGlyphHeight * (charCode / 16) + 1;
            return new Rect(
                sourceX,
                WordTextureHeight - sourceYFromTop - WordGlyphHeight,
                WordGlyphWidth,
                WordGlyphHeight);
        }

        public static Vector2 GetWordGlyphPivotNormalized() => new Vector2(0.5f, 0.5f);

        public static bool TryResolveSparkAge(int age, out int pic)
        {
            return BattleHitRecordLifecycleCatalog.Available.TryResolveAge(age, out pic);
        }

        public static Rect GetSparkPixelRect(int pic)
        {
            if (pic < 0 || pic >= SparkFrameCount)
                return Rect.zero;
            const int textureHeight = 256;
            int sourceX;
            int sourceYFromTop;
            int width;
            int height;
            if (pic < 5)
            {
                sourceX = pic * 102;
                sourceYFromTop = 0;
                width = 102;
                height = 80;
            }
            else if (pic < 10)
            {
                sourceX = (pic - 5) * 61;
                sourceYFromTop = 80;
                width = 61;
                height = 48;
            }
            else if (pic < 15)
            {
                sourceX = (pic - 10) * 102;
                sourceYFromTop = 128;
                width = 102;
                height = 80;
            }
            else
            {
                sourceX = (pic - 15) * 61;
                sourceYFromTop = 208;
                width = 61;
                height = 48;
            }

            return new Rect(sourceX, textureHeight - sourceYFromTop - height, width, height);
        }

        public static Vector2 GetSparkPivotNormalized(int pic)
        {
            return pic < 5 || (pic >= 10 && pic < 15)
                ? new Vector2(51f / 102f, 40f / 80f)
                : new Vector2(30f / 61f, 24f / 48f);
        }

        public static BattleCommonVisualCatalog Build(GameObject shadowPrefab)
        {
            if (shadowPrefab == null)
                return Invalid("GameConfig.ShadowPrefab is missing.");

            BattleCommonShadowDescriptor descriptor =
                shadowPrefab.GetComponent<BattleCommonShadowDescriptor>();
            if (descriptor == null)
                return Invalid("GameConfig.ShadowPrefab is missing its root BattleCommonShadowDescriptor.");
            if (!descriptor.TryValidate(out string diagnostic))
                return Invalid(diagnostic);

            Sprite sprite = descriptor.Sprite;
            Texture2D texture = sprite.texture;
            Material material = descriptor.Material;
            BattleSpriteMaterialSemantic semantic = BattleSpriteMaterialContract.Classify(material);

            Rect pixelRect = sprite.rect;
            Vector2 pivot = new Vector2(
                sprite.pivot.x / pixelRect.width,
                sprite.pivot.y / pixelRect.height);
            Rect normalizedUv = new Rect(
                pixelRect.x / texture.width,
                pixelRect.y / texture.height,
                pixelRect.width / texture.width,
                pixelRect.height / texture.height);
            var renderState = new BattleSpriteRenderState(
                descriptor.Color,
                descriptor.FlipX,
                descriptor.FlipY,
                descriptor.MaskInteraction,
                semantic);
            return new BattleCommonVisualCatalog(
                new BattleCommonVisualBinding(
                    BattleVisualResourceKey.CommonShadow,
                    sprite,
                    texture,
                    material,
                    pixelRect,
                    normalizedUv,
                    pixelRect.size,
                    pivot,
                    renderState),
                null,
                null,
                null,
                "Spark bindings have not been published.");
        }

        public static BattleCommonVisualCatalog Build(
            GameObject shadowPrefab,
            Texture2D sparkTexture,
            Sprite[] sparkSprites)
        {
            BattleCommonVisualCatalog shadowOnly = Build(shadowPrefab);
            if (!shadowOnly.IsShadowValid)
                return shadowOnly;
            return shadowOnly.WithSpark(sparkTexture, sparkSprites);
        }

        public static BattleCommonVisualCatalog Build(
            GameObject shadowPrefab,
            Texture2D sparkTexture,
            Sprite[] sparkSprites,
            Texture2D[] wordsTextures,
            Sprite[][] wordGlyphSprites)
        {
            BattleCommonVisualCatalog shadowAndSpark = Build(shadowPrefab, sparkTexture, sparkSprites);
            return shadowAndSpark.WithWords(wordsTextures, wordGlyphSprites);
        }

        public static BattleCommonVisualCatalog Build(
            GameObject shadowPrefab,
            Texture2D sparkTexture,
            Sprite[] sparkSprites,
            Texture2D[] wordsTextures,
            Sprite[][] wordGlyphSprites,
            Texture2D specialComTexture,
            Sprite specialComSprite)
        {
            return Build(
                    shadowPrefab,
                    sparkTexture,
                    sparkSprites,
                    wordsTextures,
                    wordGlyphSprites)
                .WithSpecialCom(specialComTexture, specialComSprite);
        }

        public static BattleCommonVisualCatalog Build(
            GameObject shadowPrefab,
            Texture2D sparkTexture,
            Sprite[] sparkSprites,
            Texture2D[] wordsTextures,
            Sprite[][] wordGlyphSprites,
            Texture2D comLabelsTexture,
            Sprite[] comLabelSprites)
        {
            return Build(
                    shadowPrefab,
                    sparkTexture,
                    sparkSprites,
                    wordsTextures,
                    wordGlyphSprites)
                .WithComLabels(comLabelsTexture, comLabelSprites);
        }

        public BattleCommonVisualCatalog WithSpark(Texture2D sparkTexture, Sprite[] sparkSprites)
        {
            if (!IsShadowValid)
                return this;
            if (sparkTexture == null || sparkTexture.width < 510 || sparkTexture.height != 256 ||
                sparkSprites == null || sparkSprites.Length != SparkFrameCount)
            {
                return new BattleCommonVisualCatalog(
                    Shadow,
                    null,
                    wordTextures,
                    wordGlyphs,
                    "SPARK.bmp is missing, corrupt, or does not contain 20 bindings.");
            }

            var bindings = new BattleCommonVisualBinding[SparkFrameCount];
            for (int pic = 0; pic < SparkFrameCount; pic++)
            {
                Sprite sprite = sparkSprites[pic];
                Rect expectedRect = GetSparkPixelRect(pic);
                Vector2 expectedPivot = GetSparkPivotNormalized(pic);
                if (sprite == null || sprite.texture != sparkTexture ||
                    sprite.rect != expectedRect ||
                    new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height) != expectedPivot)
                    return new BattleCommonVisualCatalog(
                        Shadow,
                        null,
                        wordTextures,
                        wordGlyphs,
                        $"SPARK binding {pic} is missing or references the wrong texture.");

                Rect pixelRect = sprite.rect;
                Vector2 pivot = new Vector2(
                    sprite.pivot.x / pixelRect.width,
                    sprite.pivot.y / pixelRect.height);
                Rect normalizedUv = new Rect(
                    pixelRect.x / sparkTexture.width,
                    pixelRect.y / sparkTexture.height,
                    pixelRect.width / sparkTexture.width,
                    pixelRect.height / sparkTexture.height);
                bindings[pic] = new BattleCommonVisualBinding(
                    BattleVisualResourceKey.CommonSpark(pic),
                    sprite,
                    sparkTexture,
                    null,
                    pixelRect,
                    normalizedUv,
                    pixelRect.size,
                    pivot,
                    new BattleSpriteRenderState(
                        Color.white,
                        false,
                        false,
                        SpriteMaskInteraction.None,
                        BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha));
            }

            return new BattleCommonVisualCatalog(
                Shadow,
                bindings,
                wordTextures,
                wordGlyphs,
                comLabels,
                IsWordsValid ? string.Empty : "WORDS bindings have not been published.");
        }

        public BattleCommonVisualCatalog WithWords(Texture2D[] wordsTextures, Sprite[][] wordGlyphSprites)
        {
            if (!IsShadowValid || !IsSparkValid)
                return this;
            if (wordsTextures == null || wordsTextures.Length != WordSheetCount ||
                wordGlyphSprites == null || wordGlyphSprites.Length != WordSheetCount)
            {
                return new BattleCommonVisualCatalog(
                    Shadow,
                    sparks,
                    null,
                    null,
                    "WORDS0.bmp through WORDS5.bmp must publish six 251x257 glyph sheets.");
            }

            var textures = new Texture2D[WordSheetCount];
            var bindings = new BattleCommonVisualBinding[WordSheetCount][];
            for (int sheetIndex = 0; sheetIndex < WordSheetCount; sheetIndex++)
            {
                Texture2D texture = wordsTextures[sheetIndex];
                Sprite[] sprites = wordGlyphSprites[sheetIndex];
                if (texture == null || texture.width != WordTextureWidth || texture.height != WordTextureHeight ||
                    sprites == null || sprites.Length != WordGlyphsPerSheet)
                {
                    return new BattleCommonVisualCatalog(
                        Shadow,
                        sparks,
                        null,
                        null,
                        $"WORDS{sheetIndex}.bmp is missing, corrupt, or does not contain 256 glyph bindings.");
                }

                textures[sheetIndex] = texture;
                bindings[sheetIndex] = new BattleCommonVisualBinding[WordGlyphsPerSheet];
                for (int charCode = 0; charCode < WordGlyphsPerSheet; charCode++)
                {
                    Sprite sprite = sprites[charCode];
                    Rect expectedRect = GetWordGlyphPixelRect(charCode);
                    Vector2 expectedPivot = GetWordGlyphPivotNormalized();
                    if (sprite == null || sprite.texture != texture || sprite.rect != expectedRect ||
                        new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height) != expectedPivot)
                    {
                        return new BattleCommonVisualCatalog(
                            Shadow,
                            sparks,
                            null,
                            null,
                            $"WORDS{sheetIndex} glyph {charCode} is missing or references the wrong texture.");
                    }

                    Rect pixelRect = sprite.rect;
                    Vector2 pivot = new Vector2(
                        sprite.pivot.x / pixelRect.width,
                        sprite.pivot.y / pixelRect.height);
                    Rect normalizedUv = new Rect(
                        pixelRect.x / texture.width,
                        pixelRect.y / texture.height,
                        pixelRect.width / texture.width,
                        pixelRect.height / texture.height);
                    bindings[sheetIndex][charCode] = new BattleCommonVisualBinding(
                        BattleVisualResourceKey.CommonWordGlyph(sheetIndex, charCode),
                        sprite,
                        texture,
                        null,
                        pixelRect,
                        normalizedUv,
                        pixelRect.size,
                        pivot,
                        new BattleSpriteRenderState(
                            Color.white,
                            false,
                            false,
                            SpriteMaskInteraction.None,
                            BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha));
                }
            }

            return new BattleCommonVisualCatalog(
                Shadow,
                sparks,
                textures,
                bindings,
                comLabels,
                string.Empty);
        }

        public BattleCommonVisualCatalog WithSpecialCom(
            Texture2D texture,
            Sprite sprite)
        {
            if (!IsComplete)
                return this;
            Rect expectedRect = new Rect(0f, 0f, SpecialComWidth, SpecialComHeight);
            Vector2 expectedPivot = GetSpecialComPivotNormalized();
            if (texture == null ||
                texture.width != SpecialComWidth ||
                texture.height != SpecialComHeight ||
                sprite == null ||
                sprite.texture != texture ||
                sprite.rect != expectedRect ||
                new Vector2(
                    sprite.pivot.x / sprite.rect.width,
                    sprite.pivot.y / sprite.rect.height) != expectedPivot)
            {
                return this;
            }

            return new BattleCommonVisualCatalog(
                Shadow,
                sparks,
                wordTextures,
                wordGlyphs,
                new BattleCommonVisualBinding(
                    BattleVisualResourceKey.CommonSpecialCom,
                    sprite,
                    texture,
                    null,
                    expectedRect,
                    new Rect(0f, 0f, 1f, 1f),
                    expectedRect.size,
                    expectedPivot,
                    new BattleSpriteRenderState(
                        Color.white,
                        false,
                        false,
                        SpriteMaskInteraction.None,
                        BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha)),
                Diagnostic);
        }

        public BattleCommonVisualCatalog WithComLabels(
            Texture2D texture,
            Sprite[] sprites)
        {
            if (!IsComplete)
                return this;
            if (texture == null ||
                texture.width != SpecialComWidth ||
                texture.height != ComLabelsTextureHeight ||
                sprites == null ||
                sprites.Length != WordSheetCount)
            {
                return this;
            }

            var bindings = new BattleCommonVisualBinding[WordSheetCount];
            Vector2 expectedPivot = GetSpecialComPivotNormalized();
            for (int sheetIndex = 0; sheetIndex < WordSheetCount; sheetIndex++)
            {
                Sprite sprite = sprites[sheetIndex];
                Rect expectedRect = GetComLabelPixelRect(sheetIndex);
                if (sprite == null ||
                    sprite.texture != texture ||
                    sprite.rect != expectedRect ||
                    new Vector2(
                        sprite.pivot.x / sprite.rect.width,
                        sprite.pivot.y / sprite.rect.height) != expectedPivot)
                {
                    return this;
                }

                bindings[sheetIndex] = new BattleCommonVisualBinding(
                    BattleVisualResourceKey.CommonComLabel(sheetIndex),
                    sprite,
                    texture,
                    null,
                    expectedRect,
                    new Rect(
                        0f,
                        expectedRect.y / texture.height,
                        1f,
                        (float)SpecialComHeight / texture.height),
                    expectedRect.size,
                    expectedPivot,
                    new BattleSpriteRenderState(
                        Color.white,
                        false,
                        false,
                        SpriteMaskInteraction.None,
                        BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha));
            }

            return new BattleCommonVisualCatalog(
                Shadow,
                sparks,
                wordTextures,
                wordGlyphs,
                bindings,
                Diagnostic);
        }

        internal BattleCommonVisualCatalog WithCentralBindings(
            IReadOnlyDictionary<BattleVisualResourceKey, BattleSpriteCentralBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));
            if (!IsRuntimeReady)
                throw new InvalidOperationException(
                    "A runtime-ready common visual catalog is required before central bindings can be published.");

            BattleCommonVisualBinding remappedShadow = RemapBinding(Shadow, bindings);
            var remappedSparks = new BattleCommonVisualBinding[sparks.Length];
            for (int pic = 0; pic < sparks.Length; pic++)
                remappedSparks[pic] = RemapBinding(sparks[pic], bindings);

            var remappedWords = new BattleCommonVisualBinding[wordGlyphs.Length][];
            for (int sheetIndex = 0; sheetIndex < wordGlyphs.Length; sheetIndex++)
            {
                BattleCommonVisualBinding[] sourceGlyphs = wordGlyphs[sheetIndex];
                remappedWords[sheetIndex] = new BattleCommonVisualBinding[sourceGlyphs.Length];
                for (int charCode = 0; charCode < sourceGlyphs.Length; charCode++)
                {
                    remappedWords[sheetIndex][charCode] =
                        RemapBinding(sourceGlyphs[charCode], bindings);
                }
            }

            var remappedComLabels = new BattleCommonVisualBinding[comLabels.Length];
            for (int sheetIndex = 0; sheetIndex < comLabels.Length; sheetIndex++)
            {
                remappedComLabels[sheetIndex] = comLabels[sheetIndex] != null
                    ? RemapBinding(comLabels[sheetIndex], bindings)
                    : null;
            }

            return new BattleCommonVisualCatalog(
                remappedShadow,
                remappedSparks,
                wordTextures,
                remappedWords,
                remappedComLabels,
                Diagnostic);
        }

        private static BattleCommonVisualBinding RemapBinding(
            BattleCommonVisualBinding source,
            IReadOnlyDictionary<BattleVisualResourceKey, BattleSpriteCentralBinding> bindings)
        {
            if (source == null ||
                !bindings.TryGetValue(source.Key, out BattleSpriteCentralBinding centralBinding) ||
                !centralBinding.IsValid)
            {
                throw new InvalidOperationException(
                    $"Missing central atlas binding for common visual {source?.Key.ToString() ?? "<null>"}.");
            }

            return source.WithCentralBinding(centralBinding);
        }

        private static BattleCommonVisualCatalog Invalid(string diagnostic)
        {
            return new BattleCommonVisualCatalog(null, null, null, null, diagnostic);
        }

        private static BattleCommonVisualBinding[] BuildComLabelsFromSpecial(
            BattleCommonVisualBinding specialCom)
        {
            if (specialCom == null)
                return null;
            var bindings = new BattleCommonVisualBinding[WordSheetCount];
            bindings[SpecialComSheetIndex] = specialCom;
            return bindings;
        }
    }

    /// <summary>
    /// Immutable source-rect and metric data consumed by legacy and future
    /// render backends. Rect coordinates use Unity's bottom-left pixel origin.
    /// </summary>
    public sealed class BattleSpriteEntry
    {
        public BattleSpriteKey Key { get; }
        public string SourceSheetPath { get; }
        public Texture2D SharedTexture { get; }
        public Rect PixelRect { get; }
        public Rect NormalizedUv { get; }
        public float PixelWidth { get; }
        public float PixelHeight { get; }
        public Vector2 Pivot { get; }
        public Sprite LegacySprite { get; }
        public BattleSpriteCentralBinding CentralBinding { get; }

        public BattleSpriteEntry(
            BattleSpriteKey key,
            string sourceSheetPath,
            Texture2D sharedTexture,
            Rect pixelRect,
            Vector2 pivot,
            Sprite legacySprite)
            : this(
                key,
                sourceSheetPath,
                sharedTexture,
                pixelRect,
                pivot,
                legacySprite,
                CreateSourceBinding(sharedTexture, pixelRect))
        {
        }

        internal BattleSpriteEntry(
            BattleSpriteKey key,
            string sourceSheetPath,
            Texture2D sharedTexture,
            Rect pixelRect,
            Vector2 pivot,
            Sprite legacySprite,
            BattleSpriteCentralBinding centralBinding)
        {
            Key = key;
            SourceSheetPath = sourceSheetPath ?? string.Empty;
            SharedTexture = sharedTexture;
            PixelRect = pixelRect;
            PixelWidth = pixelRect.width;
            PixelHeight = pixelRect.height;
            Pivot = pivot;
            LegacySprite = legacySprite;
            CentralBinding = centralBinding;

            float textureWidth = sharedTexture != null ? sharedTexture.width : 0f;
            float textureHeight = sharedTexture != null ? sharedTexture.height : 0f;
            NormalizedUv = textureWidth > 0f && textureHeight > 0f
                ? new Rect(
                    pixelRect.x / textureWidth,
                    pixelRect.y / textureHeight,
                    pixelRect.width / textureWidth,
                    pixelRect.height / textureHeight)
                : Rect.zero;
        }

        internal BattleSpriteEntry WithCentralBinding(BattleSpriteCentralBinding centralBinding)
        {
            return new BattleSpriteEntry(
                Key,
                SourceSheetPath,
                SharedTexture,
                PixelRect,
                Pivot,
                LegacySprite,
                centralBinding);
        }

        private static BattleSpriteCentralBinding CreateSourceBinding(Texture2D texture, Rect pixelRect)
        {
            float width = texture != null ? texture.width : 0f;
            float height = texture != null ? texture.height : 0f;
            Rect uv = width > 0f && height > 0f
                ? new Rect(pixelRect.x / width, pixelRect.y / height, pixelRect.width / width, pixelRect.height / height)
                : Rect.zero;
            return new BattleSpriteCentralBinding(
                BattleSpriteCentralBindingMode.SourceTexture2D,
                texture,
                0,
                uv,
                pixelRect);
        }
    }

    /// <summary>
    /// Immutable catalog published only after a complete prewarm pass succeeds.
    /// The builder below is intentionally the only mutable construction API.
    /// </summary>
    public sealed class BattleSpriteCatalog
    {
        private static readonly IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> EmptyEntries =
            new ReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry>(
                new Dictionary<BattleSpriteKey, BattleSpriteEntry>());

        private readonly IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> _entries;
        private readonly IReadOnlyDictionary<Sprite, BattleSpriteKey[]> _reverseKeys;

        public static BattleSpriteCatalog Empty { get; } =
            new BattleSpriteCatalog(EmptyEntries);

        public int Count => _entries.Count;

        internal BattleSpriteCatalog(IDictionary<BattleSpriteKey, BattleSpriteEntry> entries)
        {
            var immutableEntries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>(entries);
            _entries = new ReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry>(immutableEntries);
            _reverseKeys = BuildReverseKeys(immutableEntries);
        }

        private BattleSpriteCatalog(IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> entries)
        {
            _entries = entries;
            _reverseKeys = BuildReverseKeys(entries);
        }

        public bool TryGet(int visualDataId, int effectivePic, out BattleSpriteEntry entry)
        {
            return _entries.TryGetValue(new BattleSpriteKey(visualDataId, effectivePic), out entry);
        }

        public bool TryGet(BattleSpriteKey key, out BattleSpriteEntry entry)
        {
            return _entries.TryGetValue(key, out entry);
        }

        public bool TryGetKey(Sprite legacySprite, out BattleSpriteKey key)
        {
            if (legacySprite == null)
            {
                key = default;
                return false;
            }
            if (_reverseKeys.TryGetValue(legacySprite, out BattleSpriteKey[] keys) && keys.Length == 1)
            {
                key = keys[0];
                return true;
            }
            key = default;
            return false;
        }

        public bool TryGetKey(
            Sprite legacySprite,
            BattleSpriteKey preferredKey,
            out BattleSpriteKey key)
        {
            if (legacySprite != null &&
                _reverseKeys.TryGetValue(legacySprite, out BattleSpriteKey[] keys))
            {
                for (int index = 0; index < keys.Length; index++)
                {
                    if (keys[index] == preferredKey)
                    {
                        key = preferredKey;
                        return true;
                    }
                }
                if (keys.Length == 1)
                {
                    key = keys[0];
                    return true;
                }
            }
            key = default;
            return false;
        }

        public IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> Entries => _entries;

        internal BattleSpriteCatalog WithCentralBindings(
            IReadOnlyDictionary<BattleSpriteKey, BattleSpriteCentralBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            var entries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>(_entries.Count);
            foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in _entries)
            {
                if (!bindings.TryGetValue(pair.Key, out BattleSpriteCentralBinding binding) || !binding.IsValid)
                    throw new InvalidOperationException($"Missing central atlas binding for battle sprite {pair.Key}.");
                entries.Add(pair.Key, pair.Value.WithCentralBinding(binding));
            }
            return new BattleSpriteCatalog((IDictionary<BattleSpriteKey, BattleSpriteEntry>)entries);
        }

        private static IReadOnlyDictionary<Sprite, BattleSpriteKey[]> BuildReverseKeys(
            IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> entries)
        {
            var mutableKeys = new Dictionary<Sprite, List<BattleSpriteKey>>();
            foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in entries)
            {
                Sprite sprite = pair.Value?.LegacySprite;
                if (sprite == null)
                    continue;
                if (!mutableKeys.TryGetValue(sprite, out List<BattleSpriteKey> keys))
                {
                    keys = new List<BattleSpriteKey>(1);
                    mutableKeys.Add(sprite, keys);
                }
                keys.Add(pair.Key);
            }

            var reverseKeys = new Dictionary<Sprite, BattleSpriteKey[]>(mutableKeys.Count);
            foreach (KeyValuePair<Sprite, List<BattleSpriteKey>> pair in mutableKeys)
                reverseKeys.Add(pair.Key, pair.Value.ToArray());
            return new ReadOnlyDictionary<Sprite, BattleSpriteKey[]>(reverseKeys);
        }
    }

    public sealed class BattleSpriteCatalogLease : IDisposable
    {
        private Action release;

        internal BattleSpriteCatalogLease(BattleSpriteCatalog catalog, Action releaseAction)
        {
            Catalog = catalog ?? BattleSpriteCatalog.Empty;
            release = releaseAction;
        }

        public BattleSpriteCatalog Catalog { get; }
        public bool IsReleased => release == null;

        public void Dispose()
        {
            Action releaseAction = release;
            release = null;
            releaseAction?.Invoke();
        }
    }

    public sealed class BattleSpriteCatalogBuilder
    {
        private readonly Dictionary<BattleSpriteKey, BattleSpriteEntry> _entries =
            new Dictionary<BattleSpriteKey, BattleSpriteEntry>();

        public int Count => _entries.Count;

        public void Add(
            int visualDataId,
            int effectivePic,
            string sourceSheetPath,
            Texture2D sharedTexture,
            Rect pixelRect,
            Sprite legacySprite)
        {
            Add(
                visualDataId,
                effectivePic,
                sourceSheetPath,
                sharedTexture,
                pixelRect,
                new Vector2(0.5f, 0f),
                legacySprite);
        }

        public void Add(
            int visualDataId,
            int effectivePic,
            string sourceSheetPath,
            Texture2D sharedTexture,
            Rect pixelRect,
            Vector2 pivot,
            Sprite legacySprite)
        {
            if (visualDataId < 0)
                throw new ArgumentOutOfRangeException(nameof(visualDataId));
            if (effectivePic < 0)
                throw new ArgumentOutOfRangeException(nameof(effectivePic));
            if (sharedTexture == null)
                throw new ArgumentNullException(nameof(sharedTexture));
            if (pixelRect.width <= 0f || pixelRect.height <= 0f)
                throw new ArgumentOutOfRangeException(nameof(pixelRect));

            var key = new BattleSpriteKey(visualDataId, effectivePic);
            if (_entries.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Duplicate battle sprite key {key}; overlapping DAT file ranges are not allowed.");
            }

            _entries.Add(key, new BattleSpriteEntry(
                key,
                sourceSheetPath,
                sharedTexture,
                pixelRect,
                pivot,
                legacySprite));
        }

        public BattleSpriteCatalog Publish()
        {
            return new BattleSpriteCatalog(_entries);
        }
    }
}
