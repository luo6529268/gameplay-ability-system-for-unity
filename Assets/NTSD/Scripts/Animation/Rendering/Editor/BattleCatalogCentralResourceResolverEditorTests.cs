#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCatalogCentralResourceResolverEditorTests
    {
        private static readonly FieldInfo FallbackMaterialField =
            typeof(BattleCatalogCentralResourceResolver).GetField(
                "fallbackMaterial",
                BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void Configure_ValidAndInvalid2DMaterials_PreserveResolverStatus()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            AssertResolved(fixture.Resolver, command);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Invalid2DMaterial, fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, command);
            AssertUnresolved(fixture.Resolver, command);
        }

        [Test]
        public void Configure_RevalidatesMaterialMutatedSinceThePreviousBuild()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);

            fixture.Valid2DMaterial.SetColor("_Color", Color.black);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, command);
        }

        [Test]
        public void Configure_ValidAndInvalidArrayMaterials_PreserveResolverStatus()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            BattleRenderCommand command = fixture.CreateCommand(1);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            AssertResolved(fixture.Resolver, command);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.InvalidArrayMaterial);
            AssertUnresolved(fixture.Resolver, command);
            AssertUnresolved(fixture.Resolver, command);
        }

        [Test]
        public void Resolve_AllTemplateKinds_KeepColdAndCachedFieldsIdentical()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            using var common = new CommonFixture("cold-cached");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            BattleRenderCommand[] commands =
            {
                fixture.CreateCommand(0),
                fixture.CreateCommand(1),
                common.CreateShadowCommand(),
                common.CreateSparkCommand(),
                common.CreateWordCommand(),
            };

            for (int commandIndex = 0; commandIndex < commands.Length; commandIndex++)
            {
                BattleCentralResourceStatus coldStatus =
                    fixture.Resolver.Resolve(commands[commandIndex], out BattleCentralResolvedResource cold);
                BattleCentralResourceStatus cachedStatus =
                    fixture.Resolver.Resolve(commands[commandIndex], out BattleCentralResolvedResource cached);

                Assert.That(coldStatus, Is.EqualTo(BattleCentralResourceStatus.Resolved));
                Assert.That(cachedStatus, Is.EqualTo(coldStatus));
                AssertResourcesEqual(cold, cached);
            }
        }

        [Test]
        public void Resolve_EntityExactSignatureMutations_FailClosedAfterWarmup()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            BattleRenderCommand command = fixture.CreateCommand(0);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);

            foreach (SignatureMutation mutation in Enum.GetValues(typeof(SignatureMutation)))
            {
                BattleRenderCommand mutated = MutateEntityCommand(command, mutation);
                BattleCentralResourceStatus expected =
                    mutation == SignatureMutation.MaterialSemantic ||
                    mutation == SignatureMutation.MaskInteraction
                        ? BattleCentralResourceStatus.UnsupportedRenderState
                        : BattleCentralResourceStatus.UnresolvedVisual;
                Assert.That(
                    fixture.Resolver.Resolve(mutated, out _),
                    Is.EqualTo(expected),
                    mutation.ToString());
            }
        }

        [Test]
        public void Resolve_MaterialOutsideConfigureReferences_UsesImmediateValidation()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);

            Assert.That(FallbackMaterialField, Is.Not.Null);
            AssertResolved(fixture.Resolver, command);
            FallbackMaterialField.SetValue(fixture.Resolver, fixture.Invalid2DMaterial);
            AssertUnresolved(fixture.Resolver, command);

            FallbackMaterialField.SetValue(fixture.Resolver, fixture.AlternateValid2DMaterial);
            AssertResolved(fixture.Resolver, command);
        }

        [Test]
        public void Resolve_SparkMaterialIdentityMutation_FailsClosedAfterWarmup()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            using var common = new CommonFixture("spark-material");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            BattleRenderCommand command = common.CreateSparkCommand();
            AssertResolved(fixture.Resolver, command);

            BattleSpriteValueDescriptor descriptor = command.SpriteDescriptor;
            var mutatedDescriptor = new BattleSpriteValueDescriptor(
                descriptor.RequiresSprite,
                descriptor.HasSprite,
                descriptor.SpriteInstanceId,
                descriptor.TextureInstanceId,
                descriptor.MaterialInstanceId + 1,
                descriptor.PixelRect,
                descriptor.PivotNormalized,
                descriptor.LogicalResourceKey);
            AssertUnresolved(fixture.Resolver, CloneCommand(command, mutatedDescriptor));
        }

        [Test]
        public void Resolve_CachedTemplate_InjectsCurrentCommandColor()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);

            Color32 firstColor = new Color32(17, 31, 47, 63);
            Color32 secondColor = new Color32(79, 97, 113, 131);
            Assert.That(
                fixture.Resolver.Resolve(
                    fixture.CreateCommand(0, firstColor),
                    out BattleCentralResolvedResource first),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                fixture.Resolver.Resolve(
                    fixture.CreateCommand(0, secondColor),
                    out BattleCentralResolvedResource second),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));

            Assert.That(first.Color, Is.EqualTo(firstColor));
            Assert.That(second.Color, Is.EqualTo(secondColor));
            Assert.That(ReferenceEquals(first.Texture, second.Texture), Is.True);
            Assert.That(ReferenceEquals(first.Material, second.Material), Is.True);
        }

        [Test]
        public void Configure_EveryCallDropsPriorCatalogCommonAndMaterialTemplates()
        {
            using var first = new ResolverFixture(includeArrayBinding: true, name: "epoch-a");
            using var second = new ResolverFixture(includeArrayBinding: true, name: "epoch-b");
            using var commonA = new CommonFixture("epoch-common-a");
            using var commonB = new CommonFixture("epoch-common-b");
            BattleCatalogCentralResourceResolver resolver = first.Resolver;
            BattleRenderCommand entityA = first.CreateCommand(0);
            BattleRenderCommand entityB = second.CreateCommand(0);
            BattleRenderCommand arrayB = second.CreateCommand(1);
            BattleRenderCommand shadowA = commonA.CreateShadowCommand();
            BattleRenderCommand shadowB = commonB.CreateShadowCommand();

            resolver.Configure(
                first.Catalog,
                commonA.Catalog,
                first.Valid2DMaterial,
                first.ValidArrayMaterial);
            AssertResolved(resolver, entityA);
            AssertResolved(resolver, shadowA);

            resolver.Configure(
                second.Catalog,
                commonA.Catalog,
                first.Valid2DMaterial,
                first.ValidArrayMaterial);
            AssertUnresolved(resolver, entityA);
            Assert.That(
                resolver.Resolve(entityB, out BattleCentralResolvedResource entityBResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(ReferenceEquals(entityBResource.Texture, second.SourceTexture), Is.True);

            resolver.Configure(
                second.Catalog,
                commonB.Catalog,
                first.Valid2DMaterial,
                first.ValidArrayMaterial);
            AssertUnresolved(resolver, shadowA);
            Assert.That(
                resolver.Resolve(shadowB, out BattleCentralResolvedResource shadowBResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(ReferenceEquals(shadowBResource.Texture, commonB.Texture), Is.True);

            resolver.Configure(
                second.Catalog,
                commonB.Catalog,
                first.AlternateValid2DMaterial,
                first.ValidArrayMaterial);
            Assert.That(
                resolver.Resolve(entityB, out BattleCentralResolvedResource fallbackResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                ReferenceEquals(fallbackResource.Material, first.AlternateValid2DMaterial),
                Is.True);

            resolver.Configure(
                second.Catalog,
                commonB.Catalog,
                first.AlternateValid2DMaterial,
                first.AlternateValidArrayMaterial);
            Assert.That(
                resolver.Resolve(arrayB, out BattleCentralResolvedResource arrayResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                ReferenceEquals(arrayResource.Material, first.AlternateValidArrayMaterial),
                Is.True);

            resolver.Configure(null, null, null, null);
            AssertUnresolved(resolver, entityB);
            AssertUnresolved(resolver, shadowB);
        }

        [Test]
        public void Resolve_InvalidBindingAndMaterial_RemainCachedUnresolved()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand validCommand = fixture.CreateCommand(0);
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Invalid2DMaterial,
                fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, validCommand);
            AssertUnresolved(fixture.Resolver, validCommand);

            BattleSpriteCatalog invalidCatalog = fixture.CreateInvalidBindingCatalog();
            BattleRenderCommand invalidBindingCommand =
                fixture.CreateCommand(invalidCatalog, ResolverFixture.InvalidBindingVisualDataId, 0);
            fixture.Resolver.Configure(
                invalidCatalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, invalidBindingCommand);
            AssertUnresolved(fixture.Resolver, invalidBindingCommand);
        }

        [Test]
        public void Resolve_WarmedEntityShadowSparkAndWordTemplates_AllocateZeroBytes()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            using var common = new CommonFixture("allocation");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            BattleRenderCommand entity = fixture.CreateCommand(0);
            BattleRenderCommand shadow = common.CreateShadowCommand();
            BattleRenderCommand spark = common.CreateSparkCommand();
            BattleRenderCommand word = common.CreateWordCommand();
            AssertResolved(fixture.Resolver, entity);
            AssertResolved(fixture.Resolver, shadow);
            AssertResolved(fixture.Resolver, spark);
            AssertResolved(fixture.Resolver, word);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                fixture.Resolver.Resolve(entity, out _);
                fixture.Resolver.Resolve(shadow, out _);
                fixture.Resolver.Resolve(spark, out _);
                fixture.Resolver.Resolve(word, out _);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
        }

        private static void AssertResolved(
            BattleCatalogCentralResourceResolver resolver,
            in BattleRenderCommand command)
        {
            Assert.That(
                resolver.Resolve(command, out _),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
        }

        private static void AssertUnresolved(
            BattleCatalogCentralResourceResolver resolver,
            in BattleRenderCommand command)
        {
            Assert.That(
                resolver.Resolve(command, out _),
                Is.EqualTo(BattleCentralResourceStatus.UnresolvedVisual));
        }

        private static void AssertResourcesEqual(
            in BattleCentralResolvedResource expected,
            in BattleCentralResolvedResource actual)
        {
            Assert.That(ReferenceEquals(actual.Texture, expected.Texture), Is.True);
            Assert.That(ReferenceEquals(actual.Material, expected.Material), Is.True);
            Assert.That(actual.NormalizedUv, Is.EqualTo(expected.NormalizedUv));
            Assert.That(actual.PixelSize, Is.EqualTo(expected.PixelSize));
            Assert.That(actual.Pivot, Is.EqualTo(expected.Pivot));
            Assert.That(actual.Color, Is.EqualTo(expected.Color));
            Assert.That(actual.MaterialVariant, Is.EqualTo(expected.MaterialVariant));
            Assert.That(actual.AtlasSlice, Is.EqualTo(expected.AtlasSlice));
            Assert.That(actual.BindingMode, Is.EqualTo(expected.BindingMode));
            Assert.That(actual.AtlasPageIndex, Is.EqualTo(expected.AtlasPageIndex));
        }

        private static BattleRenderCommand MutateEntityCommand(
            in BattleRenderCommand command,
            SignatureMutation mutation)
        {
            BattleSpriteValueDescriptor source = command.SpriteDescriptor;
            bool requiresSprite = source.RequiresSprite;
            bool hasSprite = source.HasSprite;
            bool hasLogicalKey = source.HasLogicalResourceKey;
            int spriteInstanceId = source.SpriteInstanceId;
            int textureInstanceId = source.TextureInstanceId;
            int materialInstanceId = source.MaterialInstanceId;
            Rect pixelRect = source.PixelRect;
            Vector2 descriptorPivot = source.PivotNormalized;
            BattleVisualResourceKey logicalKey = source.LogicalResourceKey;
            int visualDataId = command.VisualDataId;
            int effectivePic = command.EffectivePic;
            Vector2 size = command.Size;
            Vector2 commandPivot = command.Pivot;
            Rect normalizedUv = command.NormalizedUv;
            BattleSpriteRenderState renderState = command.RenderState;

            switch (mutation)
            {
                case SignatureMutation.MissingLogicalKey:
                    hasLogicalKey = false;
                    break;
                case SignatureMutation.LogicalKey:
                    logicalKey = BattleVisualResourceKey.FromEntity(new BattleSpriteKey(17, 1));
                    break;
                case SignatureMutation.VisualDataId:
                    visualDataId++;
                    break;
                case SignatureMutation.EffectivePic:
                    effectivePic++;
                    break;
                case SignatureMutation.RequiresSprite:
                    requiresSprite = !requiresSprite;
                    break;
                case SignatureMutation.HasSprite:
                    hasSprite = !hasSprite;
                    break;
                case SignatureMutation.SpriteInstanceId:
                    spriteInstanceId++;
                    break;
                case SignatureMutation.TextureInstanceId:
                    textureInstanceId++;
                    break;
                case SignatureMutation.MaterialInstanceId:
                    materialInstanceId++;
                    break;
                case SignatureMutation.PixelRect:
                    pixelRect.x++;
                    break;
                case SignatureMutation.DescriptorPivot:
                    descriptorPivot.x += 0.125f;
                    break;
                case SignatureMutation.CommandPivot:
                    commandPivot.x += 0.125f;
                    break;
                case SignatureMutation.NormalizedUv:
                    normalizedUv.x += 0.125f;
                    break;
                case SignatureMutation.Size:
                    size.x++;
                    break;
                case SignatureMutation.MaterialSemantic:
                    renderState = new BattleSpriteRenderState(
                        renderState.Color,
                        renderState.FlipX,
                        renderState.FlipY,
                        renderState.MaskInteraction,
                        BattleSpriteMaterialSemantic.Unsupported);
                    break;
                case SignatureMutation.MaskInteraction:
                    renderState = new BattleSpriteRenderState(
                        renderState.Color,
                        renderState.FlipX,
                        renderState.FlipY,
                        SpriteMaskInteraction.VisibleInsideMask,
                        renderState.MaterialSemantic);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
            }

            BattleSpriteValueDescriptor descriptor = hasLogicalKey
                ? new BattleSpriteValueDescriptor(
                    requiresSprite,
                    hasSprite,
                    spriteInstanceId,
                    textureInstanceId,
                    materialInstanceId,
                    pixelRect,
                    descriptorPivot,
                    logicalKey)
                : new BattleSpriteValueDescriptor(
                    requiresSprite,
                    hasSprite,
                    spriteInstanceId,
                    textureInstanceId,
                    materialInstanceId,
                    pixelRect,
                    descriptorPivot);
            return CloneCommand(
                command,
                descriptor,
                visualDataId,
                effectivePic,
                size,
                commandPivot,
                normalizedUv,
                renderState);
        }

        private static BattleRenderCommand CloneCommand(
            in BattleRenderCommand command,
            BattleSpriteValueDescriptor descriptor,
            int? visualDataId = null,
            int? effectivePic = null,
            Vector2? size = null,
            Vector2? pivot = null,
            Rect? normalizedUv = null,
            BattleSpriteRenderState? renderState = null)
        {
            return new BattleRenderCommand(
                command.Type,
                command.Handle,
                command.StableId,
                visualDataId ?? command.VisualDataId,
                effectivePic ?? command.EffectivePic,
                command.ZInt,
                command.RuntimeSlot,
                command.SortOrder,
                command.SortingLayerId,
                command.LocalSequence,
                command.Position,
                size ?? command.Size,
                pivot ?? command.Pivot,
                normalizedUv ?? command.NormalizedUv,
                renderState ?? command.RenderState,
                descriptor);
        }

        private enum SignatureMutation
        {
            MissingLogicalKey,
            LogicalKey,
            VisualDataId,
            EffectivePic,
            RequiresSprite,
            HasSprite,
            SpriteInstanceId,
            TextureInstanceId,
            MaterialInstanceId,
            PixelRect,
            DescriptorPivot,
            CommandPivot,
            NormalizedUv,
            Size,
            MaterialSemantic,
            MaskInteraction,
        }

        private sealed class ResolverFixture : IDisposable
        {
            public const int InvalidBindingVisualDataId = 18;

            private static readonly ConstructorInfo EntryWithCentralBindingConstructor =
                typeof(BattleSpriteEntry).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleSpriteKey),
                        typeof(string),
                        typeof(Texture2D),
                        typeof(Rect),
                        typeof(Vector2),
                        typeof(Sprite),
                        typeof(BattleSpriteCentralBinding),
                    },
                    null);
            private static readonly ConstructorInfo CatalogConstructor =
                typeof(BattleSpriteCatalog).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(IDictionary<BattleSpriteKey, BattleSpriteEntry>),
                    },
                    null);

            private readonly Texture2D texture;
            private readonly Texture2DArray arrayTexture;
            private readonly Sprite sourceSprite;
            private readonly Sprite arraySprite;

            public ResolverFixture(bool includeArrayBinding, string name = "resolver-fixture")
            {
                texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = $"{name}-texture",
                };
                arrayTexture = new Texture2DArray(8, 8, 1, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = $"{name}-array",
                };
                sourceSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0f),
                    1f);
                sourceSprite.hideFlags = HideFlags.HideAndDontSave;
                sourceSprite.name = $"{name}-source-sprite";
                arraySprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0f),
                    1f);
                arraySprite.hideFlags = HideFlags.HideAndDontSave;
                arraySprite.name = $"{name}-array-sprite";
                Shader textureShader = Shader.Find(BattleSpriteMaterialContract.CentralTextureShaderName);
                Shader arrayShader = Shader.Find(BattleSpriteMaterialContract.CentralArrayShaderName);
                Assert.That(textureShader, Is.Not.Null);
                Assert.That(arrayShader, Is.Not.Null);

                Valid2DMaterial = CreateMaterial(textureShader);
                AlternateValid2DMaterial = CreateMaterial(textureShader);
                Invalid2DMaterial = CreateMaterial(arrayShader);
                ValidArrayMaterial = CreateMaterial(arrayShader);
                AlternateValidArrayMaterial = CreateMaterial(arrayShader);
                InvalidArrayMaterial = CreateMaterial(textureShader);
                Catalog = CreateCatalog(includeArrayBinding);
                Resolver = new BattleCatalogCentralResourceResolver();
            }

            public BattleSpriteCatalog Catalog { get; }
            public BattleCatalogCentralResourceResolver Resolver { get; }
            public Material Valid2DMaterial { get; }
            public Material AlternateValid2DMaterial { get; }
            public Material Invalid2DMaterial { get; }
            public Material ValidArrayMaterial { get; }
            public Material AlternateValidArrayMaterial { get; }
            public Material InvalidArrayMaterial { get; }
            public Texture2D SourceTexture => texture;

            public BattleRenderCommand CreateCommand(
                int effectivePic,
                Color32? color = null)
            {
                return CreateCommand(Catalog, 17, effectivePic, color);
            }

            public BattleRenderCommand CreateCommand(
                BattleSpriteCatalog sourceCatalog,
                int visualDataId,
                int effectivePic,
                Color32? color = null)
            {
                Assert.That(
                    sourceCatalog.TryGet(visualDataId, effectivePic, out BattleSpriteEntry entry),
                    Is.True);
                Color32 commandColor = color ?? new Color32(255, 255, 255, 255);
                var renderState = new BattleSpriteRenderState(
                    commandColor,
                    false,
                    false,
                    SpriteMaskInteraction.None,
                    BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
                return new BattleRenderCommand(
                    BattleRenderCommandType.Entity,
                    new RuntimeEntityHandle(effectivePic, 1),
                    effectivePic + 1,
                    visualDataId,
                    effectivePic,
                    0,
                    effectivePic,
                    0,
                    0,
                    effectivePic,
                    Vector3.zero,
                    new Vector2(entry.PixelWidth, entry.PixelHeight),
                    entry.Pivot,
                    entry.NormalizedUv,
                    renderState,
                    new BattleSpriteValueDescriptor(
                        true,
                        entry.LegacySprite != null,
                        entry.LegacySprite != null ? entry.LegacySprite.GetInstanceID() : 0,
                        entry.SharedTexture != null ? entry.SharedTexture.GetInstanceID() : 0,
                        0,
                        entry.PixelRect,
                        entry.Pivot,
                        BattleVisualResourceKey.FromEntity(entry.Key)));
            }

            public BattleSpriteCatalog CreateInvalidBindingCatalog()
            {
                Assert.That(EntryWithCentralBindingConstructor, Is.Not.Null);
                Assert.That(CatalogConstructor, Is.Not.Null);
                var key = new BattleSpriteKey(InvalidBindingVisualDataId, 0);
                var entry = (BattleSpriteEntry)EntryWithCentralBindingConstructor.Invoke(
                    new object[]
                    {
                        key,
                        "invalid-binding",
                        texture,
                        new Rect(0f, 0f, 8f, 8f),
                        new Vector2(0.5f, 0f),
                        sourceSprite,
                        default(BattleSpriteCentralBinding),
                    });
                var entries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>
                {
                    [key] = entry,
                };
                return (BattleSpriteCatalog)CatalogConstructor.Invoke(new object[] { entries });
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(sourceSprite);
                UnityEngine.Object.DestroyImmediate(arraySprite);
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(arrayTexture);
                UnityEngine.Object.DestroyImmediate(Valid2DMaterial);
                UnityEngine.Object.DestroyImmediate(AlternateValid2DMaterial);
                UnityEngine.Object.DestroyImmediate(Invalid2DMaterial);
                UnityEngine.Object.DestroyImmediate(ValidArrayMaterial);
                UnityEngine.Object.DestroyImmediate(AlternateValidArrayMaterial);
                UnityEngine.Object.DestroyImmediate(InvalidArrayMaterial);
            }

            private BattleSpriteCatalog CreateCatalog(bool includeArrayBinding)
            {
                var builder = new BattleSpriteCatalogBuilder();
                builder.Add(
                    17,
                    0,
                    "resolver-fixture",
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    sourceSprite);
                if (!includeArrayBinding)
                    return builder.Publish();

                builder.Add(
                    17,
                    1,
                    "resolver-fixture",
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    arraySprite);
                BattleSpriteCatalog sourceCatalog = builder.Publish();
                var bindings = new Dictionary<BattleSpriteKey, BattleSpriteCentralBinding>
                {
                    [new BattleSpriteKey(17, 0)] = new BattleSpriteCentralBinding(
                        BattleSpriteCentralBindingMode.SourceTexture2D,
                        texture,
                        0,
                        new Rect(0f, 0f, 1f, 1f),
                        new Rect(0f, 0f, 8f, 8f)),
                    [new BattleSpriteKey(17, 1)] = new BattleSpriteCentralBinding(
                        BattleSpriteCentralBindingMode.AtlasTextureArray,
                        arrayTexture,
                        0,
                        new Rect(0f, 0f, 1f, 1f),
                        new Rect(0f, 0f, 8f, 8f),
                        0),
                };
                MethodInfo withCentralBindings = typeof(BattleSpriteCatalog).GetMethod(
                    "WithCentralBindings",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(withCentralBindings, Is.Not.Null);
                return (BattleSpriteCatalog)withCentralBindings.Invoke(sourceCatalog, new object[] { bindings });
            }

            private static Material CreateMaterial(Shader shader)
            {
                var material = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                material.SetColor("_Color", Color.white);
                return material;
            }
        }

        private sealed class CommonFixture : IDisposable
        {
            private const int SparkPic = 3;
            private const int WordSheetIndex = 2;
            private const int WordCharCode = 65;

            private static readonly ConstructorInfo BindingConstructor =
                typeof(BattleCommonVisualBinding).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleVisualResourceKey),
                        typeof(Sprite),
                        typeof(Texture2D),
                        typeof(Material),
                        typeof(Rect),
                        typeof(Rect),
                        typeof(Vector2),
                        typeof(Vector2),
                        typeof(BattleSpriteRenderState),
                        typeof(BattleSpriteCentralBinding),
                    },
                    null);
            private static readonly ConstructorInfo CatalogConstructor =
                typeof(BattleCommonVisualCatalog).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleCommonVisualBinding),
                        typeof(BattleCommonVisualBinding[]),
                        typeof(Texture2D[]),
                        typeof(BattleCommonVisualBinding[][]),
                        typeof(string),
                    },
                    null);

            private readonly Texture2D sparkTexture;
            private readonly Texture2D wordTexture;
            private readonly Sprite shadowSprite;
            private readonly Sprite sparkSprite;
            private readonly Sprite wordSprite;
            private readonly BattleCommonVisualBinding shadowBinding;
            private readonly BattleCommonVisualBinding sparkBinding;
            private readonly BattleCommonVisualBinding wordBinding;

            public CommonFixture(string name)
            {
                Assert.That(BindingConstructor, Is.Not.Null);
                Assert.That(CatalogConstructor, Is.Not.Null);

                Texture = CreateTexture($"{name}-shadow");
                sparkTexture = CreateTexture($"{name}-spark");
                wordTexture = CreateTexture($"{name}-word");
                shadowSprite = CreateSprite(Texture, $"{name}-shadow-sprite");
                sparkSprite = CreateSprite(sparkTexture, $"{name}-spark-sprite");
                wordSprite = CreateSprite(wordTexture, $"{name}-word-sprite");
                shadowBinding = CreateBinding(
                    BattleVisualResourceKey.CommonShadow,
                    shadowSprite,
                    Texture);
                sparkBinding = CreateBinding(
                    BattleVisualResourceKey.CommonSpark(SparkPic),
                    sparkSprite,
                    sparkTexture);
                wordBinding = CreateBinding(
                    BattleVisualResourceKey.CommonWordGlyph(WordSheetIndex, WordCharCode),
                    wordSprite,
                    wordTexture);

                var sparks = new BattleCommonVisualBinding[
                    BattleCommonVisualCatalog.SparkFrameCount];
                sparks[SparkPic] = sparkBinding;
                var wordTextures = new Texture2D[BattleCommonVisualCatalog.WordSheetCount];
                wordTextures[WordSheetIndex] = wordTexture;
                var words = new BattleCommonVisualBinding[
                    BattleCommonVisualCatalog.WordSheetCount][];
                for (int sheetIndex = 0;
                     sheetIndex < BattleCommonVisualCatalog.WordSheetCount;
                     sheetIndex++)
                {
                    words[sheetIndex] = sheetIndex == WordSheetIndex
                        ? new BattleCommonVisualBinding[
                            BattleCommonVisualCatalog.WordGlyphsPerSheet]
                        : Array.Empty<BattleCommonVisualBinding>();
                }
                words[WordSheetIndex][WordCharCode] = wordBinding;
                Catalog = (BattleCommonVisualCatalog)CatalogConstructor.Invoke(
                    new object[]
                    {
                        shadowBinding,
                        sparks,
                        wordTextures,
                        words,
                        string.Empty,
                    });
            }

            public BattleCommonVisualCatalog Catalog { get; }
            public Texture2D Texture { get; }

            public BattleRenderCommand CreateShadowCommand()
            {
                return CreateCommand(
                    BattleRenderCommandType.Shadow,
                    -1,
                    -1,
                    shadowBinding);
            }

            public BattleRenderCommand CreateSparkCommand()
            {
                return CreateCommand(
                    BattleRenderCommandType.HitRecord,
                    -1,
                    SparkPic,
                    sparkBinding);
            }

            public BattleRenderCommand CreateWordCommand()
            {
                return CreateCommand(
                    BattleRenderCommandType.OverlayGlyph,
                    WordSheetIndex,
                    WordCharCode,
                    wordBinding);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(shadowSprite);
                UnityEngine.Object.DestroyImmediate(sparkSprite);
                UnityEngine.Object.DestroyImmediate(wordSprite);
                UnityEngine.Object.DestroyImmediate(Texture);
                UnityEngine.Object.DestroyImmediate(sparkTexture);
                UnityEngine.Object.DestroyImmediate(wordTexture);
            }

            private static Texture2D CreateTexture(string name)
            {
                return new Texture2D(8, 8, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = name,
                };
            }

            private static Sprite CreateSprite(Texture2D texture, string name)
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0.5f),
                    1f);
                sprite.hideFlags = HideFlags.HideAndDontSave;
                sprite.name = name;
                return sprite;
            }

            private static BattleCommonVisualBinding CreateBinding(
                BattleVisualResourceKey key,
                Sprite sprite,
                Texture2D texture)
            {
                var pixelRect = new Rect(0f, 0f, 8f, 8f);
                var normalizedUv = new Rect(0f, 0f, 1f, 1f);
                var pivot = new Vector2(0.5f, 0.5f);
                var centralBinding = new BattleSpriteCentralBinding(
                    BattleSpriteCentralBindingMode.SourceTexture2D,
                    texture,
                    0,
                    normalizedUv,
                    pixelRect);
                return (BattleCommonVisualBinding)BindingConstructor.Invoke(
                    new object[]
                    {
                        key,
                        sprite,
                        texture,
                        null,
                        pixelRect,
                        normalizedUv,
                        pixelRect.size,
                        pivot,
                        BattleSpriteRenderState.Default(),
                        centralBinding,
                    });
            }

            private static BattleRenderCommand CreateCommand(
                BattleRenderCommandType type,
                int visualDataId,
                int effectivePic,
                BattleCommonVisualBinding binding)
            {
                return new BattleRenderCommand(
                    type,
                    RuntimeEntityHandle.Invalid,
                    1,
                    visualDataId,
                    effectivePic,
                    0,
                    0,
                    0,
                    0,
                    0,
                    Vector3.zero,
                    binding.PixelSize,
                    binding.Pivot,
                    binding.NormalizedUv,
                    binding.RenderState,
                    new BattleSpriteValueDescriptor(
                        true,
                        true,
                        binding.SpriteInstanceId,
                        binding.TextureInstanceId,
                        binding.MaterialInstanceId,
                        binding.PixelRect,
                        binding.Pivot,
                        binding.Key));
            }
        }
    }
}
#endif
