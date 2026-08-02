#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCommonAtlasBindingEditorTests
    {
        private static readonly MethodInfo ResetFrameMethod =
            typeof(BattlePresentationFrame).GetMethod(
                "Reset",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(int), typeof(BattleCommonVisualCatalog) },
                null);
        private static readonly MethodInfo AddCommandMethod =
            typeof(BattlePresentationFrame).GetMethod(
                "AddCommand",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo BuildUnifiedPublicationMethod =
            typeof(CharacterAnimtorManager).GetMethod(
                "TryBuildUnifiedCentralAtlasPublication",
                BindingFlags.Static | BindingFlags.NonPublic);

        [Test]
        [Category("NTSD_W08Regression")]
        public void ArrayPublication_BindsShadowSparkAndWordsWithoutChangingDescriptorIdentity()
        {
            using var fixture = new CommonFixture();
            BattleAtlasPlanResult planResult = BattleAtlasLayoutPlanner.Plan(fixture.Descriptors);
            Assert.That(planResult.Succeeded, Is.True, planResult.Diagnostic);

            var policy = new BattleAtlasCapabilityPolicy(
                true,
                4096,
                256,
                true,
                false,
                256L * 1024L * 1024L);
            Assert.That(
                BattleAtlasResourceBuilder.TryBuild(
                    planResult.Plan,
                    fixture.Sources,
                    policy,
                    out BattleAtlasResources resources,
                    out string buildDiagnostic),
                Is.True,
                buildDiagnostic);
            fixture.Track(resources);

            Assert.That(resources.Mode, Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(
                BattleAtlasResourceBuilder.TryBindCommonCatalog(
                    fixture.Catalog,
                    planResult.Plan,
                    resources,
                    fixture.SourcePaths,
                    null,
                    out BattleCommonVisualCatalog bound,
                    out string bindDiagnostic),
                Is.True,
                bindDiagnostic);

            BattleCommonVisualBinding sourceShadow = fixture.Catalog.Shadow;
            BattleCommonVisualBinding boundShadow = bound.Shadow;
            Assert.That(bound.TryGetSpark(13, out BattleCommonVisualBinding boundSpark), Is.True);
            Assert.That(bound.TryGetWordGlyph(5, 'L', out BattleCommonVisualBinding boundWord), Is.True);
            Assert.That(fixture.Catalog.TryGetSpark(13, out BattleCommonVisualBinding sourceSpark), Is.True);
            Assert.That(fixture.Catalog.TryGetWordGlyph(5, 'L', out BattleCommonVisualBinding sourceWord), Is.True);

            AssertDescriptorIdentity(sourceShadow, boundShadow);
            AssertDescriptorIdentity(sourceSpark, boundSpark);
            AssertDescriptorIdentity(sourceWord, boundWord);
            Assert.That(boundShadow.CentralBinding.Texture, Is.SameAs(resources.TextureArray));
            Assert.That(boundSpark.CentralBinding.Texture, Is.SameAs(resources.TextureArray));
            Assert.That(boundWord.CentralBinding.Texture, Is.SameAs(resources.TextureArray));
            Assert.That(boundShadow.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(boundSpark.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(boundWord.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(boundShadow.CentralBinding.IsValid, Is.True);
            Assert.That(boundSpark.CentralBinding.IsValid, Is.True);
            Assert.That(boundWord.CentralBinding.IsValid, Is.True);

            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.Configure(
                BattleSpriteCatalog.Empty,
                bound,
                fixture.FallbackMaterial,
                fixture.ArrayMaterial);
            AssertArrayResolved(resolver, CreateCommand(BattleRenderCommandType.Shadow, boundShadow, -1, -1));
            AssertArrayResolved(resolver, CreateCommand(BattleRenderCommandType.HitRecord, boundSpark, -1, 13));
            AssertArrayResolved(resolver, CreateCommand(BattleRenderCommandType.OverlayGlyph, boundWord, 5, 'L'));
            BattleRenderCommand staleShadow =
                CreateCommand(BattleRenderCommandType.Shadow, boundShadow, -1, -1, 1);
            Assert.That(
                resolver.Resolve(staleShadow, out _),
                Is.EqualTo(BattleCentralResourceStatus.UnresolvedVisual),
                "Atlas remapping must not weaken descriptor Sprite/Texture/Material identity validation.");
            Assert.That(ResetFrameMethod, Is.Not.Null);
            Assert.That(AddCommandMethod, Is.Not.Null);
            var frame = new BattlePresentationFrame();
            ResetFrameMethod.Invoke(frame, new object[] { 1, bound });
            AddCommandMethod.Invoke(
                frame,
                new object[] { CreateCommand(BattleRenderCommandType.Shadow, boundShadow, -1, -1) });
            AddCommandMethod.Invoke(
                frame,
                new object[] { CreateCommand(BattleRenderCommandType.HitRecord, boundSpark, -1, 13) });
            AddCommandMethod.Invoke(
                frame,
                new object[] { CreateCommand(BattleRenderCommandType.OverlayGlyph, boundWord, 5, 'L') });
            AddCommandMethod.Invoke(
                frame,
                new object[] { CreateCommand(BattleRenderCommandType.Shadow, boundShadow, -1, -1) });
            using (var backend = new BattleDynamicMeshBackend())
            {
                backend.Build(frame, resolver, BattleCentralDrawMode.OrderedChunks);
                Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(4));
                Assert.That(backend.SegmentCount, Is.EqualTo(1),
                    "Interleaved common command kinds sharing one array/material variant must collapse into one ordered segment.");
            }

            var orderedPolicy = new BattleAtlasCapabilityPolicy(
                false,
                4096,
                0,
                true,
                false,
                0);
            Assert.That(
                BattleAtlasResourceBuilder.TryBuild(
                    planResult.Plan,
                    fixture.Sources,
                    orderedPolicy,
                    out BattleAtlasResources orderedResources,
                    out string orderedBuildDiagnostic),
                Is.True,
                orderedBuildDiagnostic);
            fixture.Track(orderedResources);
            Assert.That(
                BattleAtlasResourceBuilder.TryBindCommonCatalog(
                    fixture.Catalog,
                    planResult.Plan,
                    orderedResources,
                    fixture.SourcePaths,
                    null,
                    out BattleCommonVisualCatalog ordered,
                    out string orderedBindDiagnostic),
                Is.True,
                orderedBindDiagnostic);
            Assert.That(ordered.Shadow.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasPageTexture2D));
            Assert.That(ordered.TryGetSpark(13, out BattleCommonVisualBinding orderedSpark), Is.True);
            Assert.That(ordered.TryGetWordGlyph(5, 'L', out BattleCommonVisualBinding orderedWord), Is.True);
            AssertOrderedPageBinding(planResult.Plan, fixture.SourcePaths, ordered.Shadow);
            AssertOrderedPageBinding(planResult.Plan, fixture.SourcePaths, orderedSpark);
            AssertOrderedPageBinding(planResult.Plan, fixture.SourcePaths, orderedWord);

            fixture.CreateWithoutShadowSource(
                out List<BattleAtlasSourcePixels> nonShadowSources,
                out List<BattleAtlasSheetDescriptor> nonShadowDescriptors,
                out string shadowPath);
            BattleAtlasPlanResult nonShadowPlanResult =
                BattleAtlasLayoutPlanner.Plan(nonShadowDescriptors);
            Assert.That(nonShadowPlanResult.Succeeded, Is.True, nonShadowPlanResult.Diagnostic);
            Assert.That(
                BattleAtlasResourceBuilder.TryBuild(
                    nonShadowPlanResult.Plan,
                    nonShadowSources,
                    policy,
                    out BattleAtlasResources nonShadowResources,
                    out string nonShadowBuildDiagnostic),
                Is.True,
                nonShadowBuildDiagnostic);
            fixture.Track(nonShadowResources);
            Assert.That(
                BattleAtlasResourceBuilder.TryBindCommonCatalog(
                    fixture.Catalog,
                    nonShadowPlanResult.Plan,
                    nonShadowResources,
                    fixture.SourcePaths,
                    new[] { shadowPath },
                    out BattleCommonVisualCatalog retainedShadow,
                    out string retainedDiagnostic),
                Is.True,
                retainedDiagnostic);
            Assert.That(retainedShadow.Shadow.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
            Assert.That(retainedShadow.Shadow.CentralBinding.Texture,
                Is.SameAs(fixture.Catalog.Shadow.Texture));
            Assert.That(retainedShadow.TryGetSpark(13, out BattleCommonVisualBinding retainedSpark), Is.True);
            Assert.That(retainedSpark.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
        }

        [Test]
        public void UnifiedPublication_OversizedShadowRetainsOnlyItsSourceTexture2D()
        {
            using var fixture = new CommonFixture(BattleAtlasLayoutPlanner.MaximumContentSize + 1);
            Assert.That(BuildUnifiedPublicationMethod, Is.Not.Null);
            var capabilities = new BattleRenderingDeviceCapabilities(
                "test",
                "test",
                "test",
                true,
                4096,
                256,
                true,
                false,
                256L * 1024L * 1024L);
            object[] arguments =
            {
                BattleSpriteCatalog.Empty,
                fixture.Catalog,
                fixture.Sources,
                fixture.SourcePaths,
                Array.Empty<string>(),
                capabilities,
                null,
                Array.Empty<string>(),
                null,
                null,
                null,
                null,
                null,
                null,
            };

            bool succeeded = (bool)BuildUnifiedPublicationMethod.Invoke(null, arguments);
            Assert.That(succeeded, Is.True, arguments[11] as string);
            fixture.Track(arguments[10] as IEnumerable<UnityEngine.Object>);
            var bound = arguments[9] as BattleCommonVisualCatalog;
            Assert.That(bound, Is.Not.Null);
            Assert.That(bound.Shadow.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
            Assert.That(bound.Shadow.CentralBinding.Texture, Is.SameAs(fixture.Catalog.Shadow.Texture));
            Assert.That(bound.TryGetSpark(13, out BattleCommonVisualBinding spark), Is.True);
            Assert.That(spark.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(arguments[11] as string, Does.Contain("oversizedSource2DRetainedCount=1"));
        }

        private static void AssertDescriptorIdentity(
            BattleCommonVisualBinding expected,
            BattleCommonVisualBinding actual)
        {
            Assert.That(actual, Is.Not.SameAs(expected));
            Assert.That(actual.Key, Is.EqualTo(expected.Key));
            Assert.That(actual.Sprite, Is.SameAs(expected.Sprite));
            Assert.That(actual.Texture, Is.SameAs(expected.Texture));
            Assert.That(actual.Material, Is.SameAs(expected.Material));
            Assert.That(actual.PixelRect, Is.EqualTo(expected.PixelRect));
            Assert.That(actual.NormalizedUv, Is.EqualTo(expected.NormalizedUv));
            Assert.That(actual.PixelSize, Is.EqualTo(expected.PixelSize));
            Assert.That(actual.Pivot, Is.EqualTo(expected.Pivot));
            Assert.That(actual.RenderState, Is.EqualTo(expected.RenderState));
        }

        private static void AssertArrayResolved(
            BattleCatalogCentralResourceResolver resolver,
            in BattleRenderCommand command)
        {
            Assert.That(
                resolver.Resolve(command, out BattleCentralResolvedResource resource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(resource.BindingMode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(resource.Texture, Is.TypeOf<Texture2DArray>());
            Assert.That(resource.AtlasPageIndex, Is.EqualTo(resource.AtlasSlice));
        }

        private static void AssertOrderedPageBinding(
            BattleAtlasPlan plan,
            IReadOnlyDictionary<BattleVisualResourceKey, string> sourcePaths,
            BattleCommonVisualBinding binding)
        {
            Assert.That(binding.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasPageTexture2D));
            Assert.That(plan.TryGetPlacement(sourcePaths[binding.Key], out BattleAtlasPlacement placement),
                Is.True);
            Assert.That(binding.CentralBinding.AtlasPageIndex, Is.EqualTo(placement.PageIndex));
            Assert.That(binding.CentralBinding.AtlasSlice, Is.Zero);
            Assert.That(binding.CentralBinding.Texture, Is.TypeOf<Texture2D>());
        }

        private static BattleRenderCommand CreateCommand(
            BattleRenderCommandType type,
            BattleCommonVisualBinding binding,
            int visualDataId,
            int effectivePic,
            int spriteInstanceIdOffset = 0)
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
                SortingLayer.NameToID("Object"),
                0,
                Vector3.zero,
                binding.PixelSize,
                binding.Pivot,
                binding.NormalizedUv,
                binding.RenderState,
                new BattleSpriteValueDescriptor(
                    true,
                    true,
                    binding.SpriteInstanceId + spriteInstanceIdOffset,
                    binding.TextureInstanceId,
                    binding.MaterialInstanceId,
                    binding.PixelRect,
                    binding.Pivot,
                    binding.Key));
        }

        private sealed class CommonFixture : IDisposable
        {
            private static readonly MethodInfo ConfigureShadowMethod =
                typeof(BattleCommonShadowDescriptor).GetMethod(
                    "ConfigureForSelfCheck",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
            private readonly Sprite[] sparkSprites;
            private readonly Texture2D[] wordTextures;
            private readonly Sprite[][] wordSprites;

            public CommonFixture(int shadowTextureWidth = 8)
            {
                Shader textureShader = Shader.Find(BattleSpriteMaterialContract.CentralTextureShaderName);
                Shader arrayShader = Shader.Find(BattleSpriteMaterialContract.CentralArrayShaderName);
                Assert.That(textureShader, Is.Not.Null);
                Assert.That(arrayShader, Is.Not.Null);
                FallbackMaterial = Track(CreateMaterial(textureShader));
                ArrayMaterial = Track(CreateMaterial(arrayShader));

                Texture2D shadowTexture = Track(CreateTexture(shadowTextureWidth, 8, "common-shadow"));
                Sprite shadowSprite = Track(Sprite.Create(
                    shadowTexture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect));
                GameObject shadowPrefab = Track(new GameObject("CommonAtlasBinding_Shadow"));
                BattleCommonShadowDescriptor descriptor =
                    shadowPrefab.AddComponent<BattleCommonShadowDescriptor>();
                Assert.That(ConfigureShadowMethod, Is.Not.Null);
                ConfigureShadowMethod.Invoke(
                    descriptor,
                    new object[]
                    {
                        shadowSprite,
                        FallbackMaterial,
                        Color.white,
                        false,
                        false,
                        SpriteMaskInteraction.None,
                    });

                Texture2D sparkTexture = Track(CreateTexture(510, 256, "common-spark"));
                sparkSprites = new Sprite[BattleCommonVisualCatalog.SparkFrameCount];
                for (int pic = 0; pic < sparkSprites.Length; pic++)
                {
                    sparkSprites[pic] = Track(Sprite.Create(
                        sparkTexture,
                        BattleCommonVisualCatalog.GetSparkPixelRect(pic),
                        BattleCommonVisualCatalog.GetSparkPivotNormalized(pic),
                        100f,
                        0,
                        SpriteMeshType.FullRect));
                }

                wordTextures = new Texture2D[BattleCommonVisualCatalog.WordSheetCount];
                wordSprites = new Sprite[BattleCommonVisualCatalog.WordSheetCount][];
                for (int sheetIndex = 0; sheetIndex < wordTextures.Length; sheetIndex++)
                {
                    Texture2D wordTexture = Track(CreateTexture(
                        BattleCommonVisualCatalog.WordTextureWidth,
                        BattleCommonVisualCatalog.WordTextureHeight,
                        $"common-word-{sheetIndex}"));
                    wordTextures[sheetIndex] = wordTexture;
                    wordSprites[sheetIndex] =
                        new Sprite[BattleCommonVisualCatalog.WordGlyphsPerSheet];
                    for (int charCode = 0; charCode < wordSprites[sheetIndex].Length; charCode++)
                    {
                        wordSprites[sheetIndex][charCode] = Track(Sprite.Create(
                            wordTexture,
                            BattleCommonVisualCatalog.GetWordGlyphPixelRect(charCode),
                            BattleCommonVisualCatalog.GetWordGlyphPivotNormalized(),
                            100f,
                            0,
                            SpriteMeshType.FullRect));
                    }
                }

                Catalog = BattleCommonVisualCatalog.Build(
                    shadowPrefab,
                    sparkTexture,
                    sparkSprites,
                    wordTextures,
                    wordSprites);
                Assert.That(Catalog.IsComplete, Is.True, Catalog.Diagnostic);

                Sources = new List<BattleAtlasSourcePixels>();
                Descriptors = new List<BattleAtlasSheetDescriptor>();
                SourcePaths = new Dictionary<BattleVisualResourceKey, string>();
                AddSource("common-shadow", shadowTexture, BattleVisualResourceKey.CommonShadow);
                for (int pic = 0; pic < sparkSprites.Length; pic++)
                    SourcePaths[BattleVisualResourceKey.CommonSpark(pic)] = "common-spark";
                AddSource("common-spark", sparkTexture, null);
                for (int sheetIndex = 0; sheetIndex < wordTextures.Length; sheetIndex++)
                {
                    string path = $"common-word-{sheetIndex}";
                    AddSource(path, wordTextures[sheetIndex], null);
                    for (int charCode = 0;
                         charCode < BattleCommonVisualCatalog.WordGlyphsPerSheet;
                         charCode++)
                    {
                        SourcePaths[BattleVisualResourceKey.CommonWordGlyph(sheetIndex, charCode)] = path;
                    }
                }
            }

            public BattleCommonVisualCatalog Catalog { get; }
            public Material FallbackMaterial { get; }
            public Material ArrayMaterial { get; }
            public List<BattleAtlasSourcePixels> Sources { get; }
            public List<BattleAtlasSheetDescriptor> Descriptors { get; }
            public Dictionary<BattleVisualResourceKey, string> SourcePaths { get; }

            public void Track(BattleAtlasResources resources)
            {
                if (resources == null)
                    return;
                foreach (UnityEngine.Object resource in resources.OwnedObjects)
                    owned.Add(resource);
            }

            public void Track(IEnumerable<UnityEngine.Object> resources)
            {
                if (resources == null)
                    return;
                foreach (UnityEngine.Object resource in resources)
                    owned.Add(resource);
            }

            public void CreateWithoutShadowSource(
                out List<BattleAtlasSourcePixels> sources,
                out List<BattleAtlasSheetDescriptor> descriptors,
                out string shadowPath)
            {
                shadowPath = SourcePaths[BattleVisualResourceKey.CommonShadow];
                string normalizedShadow = BattleAtlasLayoutPlanner.NormalizePath(shadowPath);
                sources = new List<BattleAtlasSourcePixels>();
                for (int index = 0; index < Sources.Count; index++)
                {
                    BattleAtlasSourcePixels source = Sources[index];
                    if (BattleAtlasLayoutPlanner.NormalizePath(source.Path) != normalizedShadow)
                        sources.Add(source);
                }

                descriptors = new List<BattleAtlasSheetDescriptor>();
                for (int index = 0; index < Descriptors.Count; index++)
                {
                    BattleAtlasSheetDescriptor descriptor = Descriptors[index];
                    if (BattleAtlasLayoutPlanner.NormalizePath(descriptor.Path) != normalizedShadow)
                        descriptors.Add(descriptor);
                }
            }

            public void Dispose()
            {
                for (int index = owned.Count - 1; index >= 0; index--)
                {
                    if (owned[index] != null)
                        UnityEngine.Object.DestroyImmediate(owned[index]);
                }
            }

            private void AddSource(
                string path,
                Texture2D texture,
                BattleVisualResourceKey? key)
            {
                Sources.Add(new BattleAtlasSourcePixels(
                    path,
                    texture.width,
                    texture.height,
                    new Color32[texture.width * texture.height]));
                Descriptors.Add(new BattleAtlasSheetDescriptor(path, texture.width, texture.height));
                if (key.HasValue)
                    SourcePaths[key.Value] = path;
            }

            private T Track<T>(T value) where T : UnityEngine.Object
            {
                owned.Add(value);
                return value;
            }

            private static Texture2D CreateTexture(int width, int height, string name)
            {
                return new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = name,
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
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
    }
}
#endif
