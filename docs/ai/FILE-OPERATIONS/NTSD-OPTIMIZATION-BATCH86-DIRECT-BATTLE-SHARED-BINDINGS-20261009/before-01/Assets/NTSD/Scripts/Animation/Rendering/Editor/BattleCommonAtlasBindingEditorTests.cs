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
        [Test]
        public void MatchDependencies_AllFramesMultiOpointCyclesAndStateTransforms()
        {
            var configs = new Dictionary<int, LF2CharacterDataWrapper>();
            foreach (int id in new[] { 1, 2, 3, 4, 50, 99 })
                configs[id] = new LF2CharacterDataWrapper(id, new LF2CharacterData());
            configs[1].characterData.frames.Add(new LF2FrameData
            {
                state = 4003,
                opoint = new BattleObjectPointValue(1, 0, 0, 0, 0, 0, 2, 0),
                opoints = new List<BattleObjectPointValue>
                {
                    new BattleObjectPointValue(1, 0, 0, 0, 0, 0, 4, 0),
                },
            });
            configs[2].characterData.frames.Add(new LF2FrameData { state = 8050 });
            configs[3].characterData.frames.Add(new LF2FrameData { state = 9995 });
            configs[4].characterData.frames.Add(new LF2FrameData
            {
                opoint = new BattleObjectPointValue(1, 0, 0, 0, 0, 0, 1, 0),
            });
            var method = MatchMethod("CollectBattleVisualDependencyIds");
            var ids = (HashSet<int>)method.Invoke(null, new object[] { configs, new[] { 1 } });
            Assert.That(ids, Is.EquivalentTo(new[] { 1, 2, 3, 4, 50 }));
            Assert.That(configs.Count, Is.EqualTo(6));
        }

        [Test]
        public void MatchDependencies_UnregisteredRootRejectedWithoutChangingConfigs()
        {
            var configs = new Dictionary<int, LF2CharacterDataWrapper>();
            var exception = Assert.Throws<TargetInvocationException>(() =>
                MatchMethod("CollectBattleVisualDependencyIds").Invoke(null,
                    new object[] { configs, new[] { 123 } }));
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(configs, Is.Empty);
        }

        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, false, 0, true)]
        [TestCase(BattleRuntimeLifecycleState.Running, 0, false, 0, false)]
        [TestCase(BattleRuntimeLifecycleState.Stopping, 0, false, 0, false)]
        [TestCase(BattleRuntimeLifecycleState.Stopped, 0, false, 0, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 1, false, 0, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, true, 0, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, false, 1, false)]
        public void MatchBindings_ColdBoundaryOnly(BattleRuntimeLifecycleState lifecycle,
            int objects, bool sealedData, int leases, bool expected)
        {
            Assert.That(MatchMethod("BattleVisualBindingBoundaryAllows").Invoke(null,
                new object[] { lifecycle, objects, sealedData, leases }), Is.EqualTo(expected));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MatchPublication_CompleteCatalogSharedCommonAndBudgetFallback(bool withinBudget)
        {
            using var fixture = new CommonFixture();
            var builder = new BattleSpriteCatalogBuilder();
            builder.Add(1, 0, "selected-body", fixture.Catalog.Shadow.Texture,
                fixture.Catalog.Shadow.PixelRect, fixture.Catalog.Shadow.Sprite);
            builder.Add(99, 7, "unselected-body", fixture.Catalog.Shadow.Texture,
                fixture.Catalog.Shadow.PixelRect, fixture.Catalog.Shadow.Sprite);
            BattleSpriteCatalog source = builder.Publish();
            var capabilities = new BattleRenderingDeviceCapabilities(
                "test", "test", "test", true, 4096, 256, true, false,
                withinBudget ? 256L * 1024L * 1024L : 1);
            object[] args =
            {
                source, fixture.Catalog, new[] { 1 }, capabilities, null,
                Array.Empty<string>(), null, null, null, null, null, null, null,
            };
            Assert.That((bool)MatchMethod("TryBuildMatchCentralAtlasPublication").Invoke(null, args),
                Is.True, args[9] as string);
            var bound = (BattleSpriteCatalog)args[6];
            var common = (BattleCommonVisualCatalog)args[7];
            fixture.Track((HashSet<UnityEngine.Object>)args[8]);
            Assert.That(bound.Count, Is.EqualTo(source.Count));
            foreach (var pair in source.Entries)
            {
                BattleSpriteEntry entry = bound.Entries[pair.Key];
                Assert.That(entry.SharedTexture, Is.SameAs(pair.Value.SharedTexture));
                Assert.That(entry.LegacySprite, Is.SameAs(pair.Value.LegacySprite));
                Assert.That(entry.PixelRect, Is.EqualTo(pair.Value.PixelRect));
                Assert.That(entry.Pivot, Is.EqualTo(pair.Value.Pivot));
                Assert.That(entry.CentralBinding.IsValid, Is.True);
            }
            BattleSpriteCentralBinding selected = bound.Entries[new BattleSpriteKey(1, 0)].CentralBinding;
            BattleSpriteCentralBinding other = bound.Entries[new BattleSpriteKey(99, 7)].CentralBinding;
            Assert.That(other.Mode, Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
            Assert.That(other.Texture, Is.SameAs(fixture.Catalog.Shadow.Texture));
            AssertDescriptorIdentity(fixture.Catalog.Shadow, common.Shadow);
            if (withinBudget)
            {
                Assert.That(selected.Mode, Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
                Assert.That(selected.Texture, Is.SameAs(common.Shadow.CentralBinding.Texture));
            }
            else
            {
                Assert.That(selected.Mode, Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
                Assert.That((HashSet<UnityEngine.Object>)args[8], Is.Empty);
                Assert.That(args[9] as string, Does.Contain("exceeds budget"));
            }
        }

        [Test]
        public void MatchSourceCatalog_PreviousAtlasIsNotRetainedAsSourceIdentity()
        {
            using var fixture = new CommonFixture();
            var builder = new BattleSpriteCatalogBuilder();
            builder.Add(1, 0, "body", fixture.Catalog.Shadow.Texture,
                fixture.Catalog.Shadow.PixelRect, fixture.Catalog.Shadow.Sprite);
            BattleSpriteCatalog source = builder.Publish();
            object[] args =
            {
                source, fixture.Catalog, new[] { 1 },
                new BattleRenderingDeviceCapabilities("test", "test", "test",
                    true, 4096, 256, true, false, 256L * 1024L * 1024L),
                null, Array.Empty<string>(), null, null, null, null, null, null, null,
            };
            Assert.That((bool)MatchMethod("TryBuildMatchCentralAtlasPublication").Invoke(null, args),
                Is.True, args[9] as string);
            fixture.Track((HashSet<UnityEngine.Object>)args[8]);
            var previous = (BattleSpriteCatalog)args[6];
            args[0] = previous;
            args[1] = args[7];
            args[2] = Array.Empty<int>();
            Assert.That((bool)MatchMethod("TryBuildMatchCentralAtlasPublication").Invoke(null, args),
                Is.True, args[9] as string);
            fixture.Track((HashSet<UnityEngine.Object>)args[8]);
            var rebound = (BattleSpriteCatalog)args[6];
            Assert.That(rebound.Entries[new BattleSpriteKey(1, 0)].CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
            Assert.That(rebound.Entries[new BattleSpriteKey(1, 0)].CentralBinding.Texture,
                Is.SameAs(source.Entries[new BattleSpriteKey(1, 0)].SharedTexture));
        }

        private static MethodInfo MatchMethod(string name)
        {
            MethodInfo method = typeof(CharacterAnimtorManager).GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Missing cold match binding API: " + name);
            return method;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MatchPublication_SamePathDistinctTexturesRequireEqualPixels(bool equalPixels)
        {
            using var fixture = new CommonFixture();
            Texture2D first = fixture.Catalog.Shadow.Texture;
            Color32[] pixels = first.GetPixels32();
            if (!equalPixels)
                pixels[0] = new Color32(77, 1, 2, 255);
            var second = new Texture2D(first.width, first.height, TextureFormat.RGBA32, false);
            second.SetPixels32(pixels);
            second.Apply();
            Sprite sprite = Sprite.Create(second, fixture.Catalog.Shadow.PixelRect,
                fixture.Catalog.Shadow.Pivot);
            fixture.Track(new UnityEngine.Object[] { sprite, second });
            var builder = new BattleSpriteCatalogBuilder();
            builder.Add(1, 0, "shared-body", first, fixture.Catalog.Shadow.PixelRect,
                fixture.Catalog.Shadow.Sprite);
            builder.Add(2, 0, "shared-body", second, fixture.Catalog.Shadow.PixelRect, sprite);
            object[] args =
            {
                builder.Publish(), fixture.Catalog, new[] { 1, 2 },
                new BattleRenderingDeviceCapabilities("test", "test", "test",
                    true, 4096, 256, true, false, 256L * 1024L * 1024L),
                null, Array.Empty<string>(), null, null, null, null, null, null, null,
            };
            MethodInfo method = MatchMethod("TryBuildMatchCentralAtlasPublication");
            if (!equalPixels)
            {
                Assert.That(Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, args)).InnerException, Is.TypeOf<System.IO.InvalidDataException>());
                return;
            }
            Assert.That((bool)method.Invoke(null, args), Is.True, args[9] as string);
            fixture.Track((HashSet<UnityEngine.Object>)args[8]);
            var bound = (BattleSpriteCatalog)args[6];
            Assert.That(bound.Entries[new BattleSpriteKey(1, 0)].CentralBinding.Texture,
                Is.SameAs(bound.Entries[new BattleSpriteKey(2, 0)].CentralBinding.Texture));
            Assert.That(bound.Entries[new BattleSpriteKey(2, 0)].SharedTexture, Is.SameAs(second));
        }

        [Test]
        public void MatchPublication_NonReadableSourceUsesColdPixelsWithoutChangingImporter()
        {
            using var fixture = new CommonFixture();
            Texture2D body = fixture.Catalog.Shadow.Texture;
            Color32[] original = body.GetPixels32();
            body.Apply(false, true);
            Assert.That(body.isReadable, Is.False);
            var builder = new BattleSpriteCatalogBuilder();
            builder.Add(1, 0, "body", body, fixture.Catalog.Shadow.PixelRect,
                fixture.Catalog.Shadow.Sprite);
            Func<Texture2D, string, Color32[]> provider = (texture, path) =>
                texture == body ? original : texture.GetPixels32();
            object[] args =
            {
                builder.Publish(), fixture.Catalog, new[] { 1 },
                new BattleRenderingDeviceCapabilities("test", "test", "test",
                    true, 4096, 256, true, false, 256L * 1024L * 1024L),
                null, Array.Empty<string>(), null, null, null, null, null, null, provider,
            };
            Assert.That((bool)MatchMethod("TryBuildMatchCentralAtlasPublication").Invoke(null, args),
                Is.True, args[9] as string);
            fixture.Track((HashSet<UnityEngine.Object>)args[8]);
            Assert.That(body.isReadable, Is.False);
            Assert.That(((BattleSpriteCatalog)args[6]).Entries[new BattleSpriteKey(1, 0)]
                .CentralBinding.Texture, Is.SameAs(((BattleCommonVisualCatalog)args[7])
                .Shadow.CentralBinding.Texture));
        }

        [Test]
        public void MatchWindow_TwoCombatRequestsDifferOnlyInOutputPath()
        {
            MethodInfo method = typeof(BattleCommonAtlasBindingEditorTests).Assembly.GetType(
                "NTSD.Test.Editor.BattleOptimizationWindowsAiSuiteEditor").GetMethod(
                "BuildMatchCentralBindingsRequest", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            var off = (ProductionEntityStressRequest)method.Invoke(null, new object[] { 0 });
            var on = (ProductionEntityStressRequest)method.Invoke(null, new object[] { 1 });
            Assert.That(off.action, Is.EqualTo("combat1000"));
            Assert.That(off.warmupTicks, Is.EqualTo(120));
            Assert.That(off.sampleTicks, Is.EqualTo(180));
            Assert.That(off.outputPath, Does.Contain("/00-combat1000-off/"));
            Assert.That(on.outputPath, Does.Contain("/01-combat1000-on/"));
            off.outputPath = on.outputPath = string.Empty;
            Assert.That(JsonUtility.ToJson(off), Is.EqualTo(JsonUtility.ToJson(on)));
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void MatchWindow_RejectsOutsideFinitePair(int index)
        {
            MethodInfo method = typeof(BattleCommonAtlasBindingEditorTests).Assembly.GetType(
                "NTSD.Test.Editor.BattleOptimizationWindowsAiSuiteEditor").GetMethod(
                "BuildMatchCentralBindingsRequest", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            Assert.That(Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { index })).InnerException,
                Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void NativeSparkBlackKey_RemovesOnlyExactRgbBlack()
        {
            MethodInfo method = typeof(CharacterAnimtorManager).GetMethod(
                "ApplyNativeSparkBlackKey",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            var pixels = new[]
            {
                new Color32(0, 0, 0, 255),
                new Color32(0, 0, 1, 255),
                new Color32(1, 0, 0, 255),
                new Color32(4, 5, 6, 127)
            };
            var result = (Color32[])method.Invoke(null, new object[] { pixels });
            Assert.That(result, Is.SameAs(pixels));
            Assert.That(result[0].a, Is.Zero);
            Assert.That(result[1].a, Is.EqualTo(255));
            Assert.That(result[2].a, Is.EqualTo(255));
            Assert.That(result[3].a, Is.EqualTo(127));
        }

        [Test]
        public void NativeSparkCatalog_MapsRawIdsToTwentyInBoundsCells()
        {
            using var fixture = new CommonFixture();
            var texture = new Texture2D(500, 320, TextureFormat.RGBA32, false);
            var sprites = new Sprite[BattleCommonVisualCatalog.SparkFrameCount];
            try
            {
                for (int pic = 0; pic < sprites.Length; pic++)
                {
                    Rect rect = BattleCommonVisualCatalog.GetNativeSparkPixelRect(
                        pic, texture.width, texture.height, 99, 79);
                    Assert.That(rect.width, Is.EqualTo(99));
                    Assert.That(rect.height, Is.EqualTo(79));
                    sprites[pic] = Sprite.Create(texture, rect,
                        BattleCommonVisualCatalog.GetNativeSparkPivotNormalized(
                            99, 79), 100f, 0,
                        SpriteMeshType.FullRect);
                }

                BattleCommonVisualCatalog native = fixture.Catalog.WithNativeSpark(
                    texture, sprites, 99, 79);
                Assert.That(native.IsRuntimeReady, Is.True, native.Diagnostic);
                Assert.That(native.IsNativeSpark, Is.True);
                foreach (int id in new[] { 0, 4, 10, 20, 21, 24, 30, 34 })
                {
                    Assert.That(native.TryGetSparkForAge(id, out int pic,
                        out BattleCommonVisualBinding binding), Is.True, $"id {id}");
                    int expectedPic = id / 10 * 5 + id % 10;
                    Assert.That(pic, Is.EqualTo(expectedPic));
                    Assert.That(binding.Key,
                        Is.EqualTo(BattleVisualResourceKey.CommonSpark(expectedPic)));
                    Assert.That(binding.PixelRect,
                        Is.EqualTo(BattleCommonVisualCatalog.GetNativeSparkPixelRect(
                            expectedPic, 500, 320, 99, 79)));
                    Assert.That(binding.Pivot,
                        Is.EqualTo(new Vector2(49f / 99f, 40f / 79f)));
                }
                foreach (int id in new[] { -1, 5, 9, 19, 25, 35, 39, 40, 99, 100 })
                    Assert.That(native.TryGetSparkForAge(id, out _, out _), Is.False,
                        $"id {id} must not draw");
                Assert.That(fixture.Catalog.TryGetSparkForAge(20,
                    out int legacyPic, out _), Is.True);
                Assert.That(legacyPic, Is.EqualTo(10),
                    "Legacy BMP mapping must remain available for nonformal content.");
            }
            finally
            {
                foreach (Sprite sprite in sprites)
                    if (sprite != null)
                        UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

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
        public void OrderedPageAssembly_MatchesWholePlanPixelsForMultiplePages()
        {
            var plan = new BattleAtlasPlan(
                BattleAtlasLayoutPlanner.PageSize,
                BattleAtlasLayoutPlanner.ExtrusionPadding,
                new List<BattleAtlasPlacement>
                {
                    new BattleAtlasPlacement("a.png", 0, new RectInt(0, 0, 4, 4), new RectInt(1, 1, 2, 2)),
                    new BattleAtlasPlacement("b.png", 1, new RectInt(0, 0, 4, 4), new RectInt(1, 1, 2, 2)),
                });
            var sources = new Dictionary<string, BattleAtlasSourcePixels>(StringComparer.Ordinal)
            {
                ["a.png"] = new BattleAtlasSourcePixels("a.png", 2, 2, new[]
                {
                    new Color32(1, 2, 3, 255), new Color32(4, 5, 6, 255),
                    new Color32(7, 8, 9, 255), new Color32(10, 11, 12, 255),
                }),
                ["b.png"] = new BattleAtlasSourcePixels("b.png", 2, 2, new[]
                {
                    new Color32(13, 14, 15, 255), new Color32(16, 17, 18, 255),
                    new Color32(19, 20, 21, 255), new Color32(22, 23, 24, 255),
                }),
            };

            Color32[][] wholePlan = BattleAtlasResourceBuilder.AssemblePages(plan, sources);
            for (int page = 0; page < plan.PageCount; page++)
            {
                Color32[] singlePage = BattleAtlasResourceBuilder.AssemblePage(plan, sources, page);
                Assert.That(singlePage.Length, Is.EqualTo(wholePlan[page].Length));
                for (int pixel = 0; pixel < singlePage.Length; pixel++)
                {
                    if (!singlePage[pixel].Equals(wholePlan[page][pixel]))
                        Assert.Fail($"Atlas pixel mismatch at page {page}, index {pixel}.");
                }
            }
        }

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
            Assert.That(bound.TryGetSpecialCom(out BattleCommonVisualBinding boundSpecialCom), Is.True);
            Assert.That(fixture.Catalog.TryGetSpark(13, out BattleCommonVisualBinding sourceSpark), Is.True);
            Assert.That(fixture.Catalog.TryGetWordGlyph(5, 'L', out BattleCommonVisualBinding sourceWord), Is.True);
            Assert.That(fixture.Catalog.TryGetSpecialCom(
                out BattleCommonVisualBinding sourceSpecialCom), Is.True);

            AssertDescriptorIdentity(sourceShadow, boundShadow);
            AssertDescriptorIdentity(sourceSpark, boundSpark);
            AssertDescriptorIdentity(sourceWord, boundWord);
            AssertDescriptorIdentity(sourceSpecialCom, boundSpecialCom);
            Assert.That(boundShadow.CentralBinding.Texture, Is.SameAs(resources.TextureArray));
            Assert.That(boundSpark.CentralBinding.Texture, Is.SameAs(resources.TextureArray));
            Assert.That(boundWord.CentralBinding.Texture, Is.SameAs(resources.TextureArray));
            Assert.That(boundSpecialCom.CentralBinding.Texture, Is.SameAs(resources.TextureArray));
            Assert.That(boundShadow.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(boundSpark.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(boundWord.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(boundSpecialCom.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
            Assert.That(boundShadow.CentralBinding.IsValid, Is.True);
            Assert.That(boundSpark.CentralBinding.IsValid, Is.True);
            Assert.That(boundWord.CentralBinding.IsValid, Is.True);
            Assert.That(boundSpecialCom.CentralBinding.IsValid, Is.True);

            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.Configure(
                BattleSpriteCatalog.Empty,
                bound,
                fixture.FallbackMaterial,
                fixture.ArrayMaterial);
            AssertArrayResolved(resolver, CreateCommand(BattleRenderCommandType.Shadow, boundShadow, -1, -1));
            AssertArrayResolved(resolver, CreateCommand(BattleRenderCommandType.HitRecord, boundSpark, -1, 13));
            AssertArrayResolved(resolver, CreateCommand(BattleRenderCommandType.OverlayGlyph, boundWord, 5, 'L'));
            AssertArrayResolved(resolver, CreateCommand(
                BattleRenderCommandType.OverlayGlyph,
                boundSpecialCom,
                5,
                'C'));
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
                new object[]
                {
                    CreateCommand(
                        BattleRenderCommandType.OverlayGlyph,
                        boundSpecialCom,
                        5,
                        'C'),
                });
            AddCommandMethod.Invoke(
                frame,
                new object[] { CreateCommand(BattleRenderCommandType.Shadow, boundShadow, -1, -1) });
            using (var backend = new BattleDynamicMeshBackend())
            {
                backend.Build(frame, resolver, BattleCentralDrawMode.OrderedChunks);
                Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(5));
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
            Assert.That(ordered.TryGetSpecialCom(
                out BattleCommonVisualBinding orderedSpecialCom), Is.True);
            AssertOrderedPageBinding(planResult.Plan, fixture.SourcePaths, ordered.Shadow);
            AssertOrderedPageBinding(planResult.Plan, fixture.SourcePaths, orderedSpark);
            AssertOrderedPageBinding(planResult.Plan, fixture.SourcePaths, orderedWord);
            AssertOrderedPageBinding(planResult.Plan, fixture.SourcePaths, orderedSpecialCom);

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

        [Test]
        public void UnifiedPublication_AutoOverBudgetRetainsEverySourceBinding()
        {
            using var fixture = new CommonFixture();
            var capabilities = new BattleRenderingDeviceCapabilities(
                "test", "test", "test", true, 4096, 256, true, false, 1);
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

            Assert.That((bool)BuildUnifiedPublicationMethod.Invoke(null, arguments),
                Is.True, arguments[11] as string);
            var bound = arguments[9] as BattleCommonVisualCatalog;
            var owned = arguments[10] as HashSet<UnityEngine.Object>;
            var decision = arguments[12] as BattleAtlasPolicyDecision;
            var inputs = arguments[13] as BattleAtlasDiagnosticInputs;
            Assert.That(bound, Is.Not.Null);
            Assert.That(owned, Is.Empty);
            Assert.That(decision.RequestedMode, Is.EqualTo(BattleAtlasPolicyMode.Auto));
            Assert.That(decision.EffectiveMode, Is.EqualTo(BattleAtlasPolicyMode.SourceTexture2D));
            Assert.That(inputs.PlannedPageCount, Is.GreaterThan(0));
            Assert.That(inputs.EstimatedAtlasBytes, Is.GreaterThan(capabilities.AtlasMemoryBudgetBytes));
            Assert.That(inputs.CatalogResourceMode,
                Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
            Assert.That(bound.Shadow.CentralBinding.Texture,
                Is.SameAs(fixture.Catalog.Shadow.CentralBinding.Texture));
            Assert.That(bound.Shadow, Is.SameAs(fixture.Catalog.Shadow));
            Assert.That(bound.TryGetSpark(13, out BattleCommonVisualBinding spark), Is.True);
            Assert.That(spark.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
            Assert.That(fixture.Catalog.TryGetSpark(13, out BattleCommonVisualBinding sourceSpark), Is.True);
            Assert.That(spark.CentralBinding.Texture,
                Is.SameAs(sourceSpark.CentralBinding.Texture));
            Assert.That(spark, Is.SameAs(sourceSpark));
            Assert.That(bound.TryGetWordGlyph(5, 'L', out BattleCommonVisualBinding word), Is.True);
            Assert.That(word.CentralBinding.Mode,
                Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
            Assert.That(fixture.Catalog.TryGetWordGlyph(5, 'L', out BattleCommonVisualBinding sourceWord), Is.True);
            Assert.That(word, Is.SameAs(sourceWord));
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.Configure(BattleSpriteCatalog.Empty, bound,
                fixture.FallbackMaterial, fixture.ArrayMaterial);
            var frame = new BattlePresentationFrame();
            Assert.That(ResetFrameMethod, Is.Not.Null);
            Assert.That(AddCommandMethod, Is.Not.Null);
            ResetFrameMethod.Invoke(frame, new object[] { 1, bound });
            AddCommandMethod.Invoke(frame,
                new object[] { CreateCommand(BattleRenderCommandType.Shadow, bound.Shadow, -1, -1) });
            AddCommandMethod.Invoke(frame,
                new object[] { CreateCommand(BattleRenderCommandType.HitRecord, spark, -1, 13) });
            AddCommandMethod.Invoke(frame,
                new object[] { CreateCommand(BattleRenderCommandType.OverlayGlyph, word, 5, 'L') });
            AddCommandMethod.Invoke(frame,
                new object[] { CreateCommand(BattleRenderCommandType.Shadow, bound.Shadow, -1, -1) });
            using (var backend = new BattleDynamicMeshBackend())
            {
                backend.Build(frame, resolver, BattleCentralDrawMode.OrderedChunks);
                Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(4));
                Assert.That(backend.SegmentCount, Is.EqualTo(4));
                Texture[] expectedTextures =
                {
                    bound.Shadow.CentralBinding.Texture,
                    spark.CentralBinding.Texture,
                    word.CentralBinding.Texture,
                    bound.Shadow.CentralBinding.Texture,
                };
                for (int index = 0; index < expectedTextures.Length; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    Assert.That(segment.FirstCommandIndex, Is.EqualTo(index));
                    Assert.That(segment.Texture, Is.SameAs(expectedTextures[index]));
                    Assert.That(segment.BindingMode,
                        Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
                }
            }
            Assert.That(arguments[11] as string, Does.Contain("exceeds budget"));

            arguments[7] = new[]
            {
                BattleRenderingPolicyResolver.AtlasModeArgument,
                nameof(BattleAtlasPolicyMode.OrderedPages),
            };
            Assert.That((bool)BuildUnifiedPublicationMethod.Invoke(null, arguments),
                Is.True, arguments[11] as string);
            fixture.Track(arguments[10] as IEnumerable<UnityEngine.Object>);
            decision = arguments[12] as BattleAtlasPolicyDecision;
            Assert.That(decision.EffectiveMode, Is.EqualTo(BattleAtlasPolicyMode.OrderedPages));
            Assert.That(((HashSet<UnityEngine.Object>)arguments[10]).Count, Is.GreaterThan(0));
            Assert.That(BattleRenderingPolicyResolver.TryParseAtlasMode(
                nameof(BattleAtlasPolicyMode.SourceTexture2D), out BattleAtlasPolicyMode parsed), Is.True);
            Assert.That(parsed, Is.EqualTo(BattleAtlasPolicyMode.SourceTexture2D));
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

                Texture2D specialComTexture = Track(CreateTexture(
                    BattleCommonVisualCatalog.SpecialComWidth,
                    BattleCommonVisualCatalog.SpecialComHeight,
                    "common-special-com"));
                Sprite specialComSprite = Track(Sprite.Create(
                    specialComTexture,
                    new Rect(
                        0f,
                        0f,
                        BattleCommonVisualCatalog.SpecialComWidth,
                        BattleCommonVisualCatalog.SpecialComHeight),
                    BattleCommonVisualCatalog.GetSpecialComPivotNormalized(),
                    100f,
                    0,
                    SpriteMeshType.FullRect));
                Catalog = BattleCommonVisualCatalog.Build(
                    shadowPrefab,
                    sparkTexture,
                    sparkSprites,
                    wordTextures,
                    wordSprites,
                    specialComTexture,
                    specialComSprite);
                Assert.That(Catalog.IsComplete, Is.True, Catalog.Diagnostic);
                Assert.That(Catalog.IsSpecialComValid, Is.True);

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
                AddSource(
                    "common-special-com",
                    specialComTexture,
                    BattleVisualResourceKey.CommonSpecialCom);
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
