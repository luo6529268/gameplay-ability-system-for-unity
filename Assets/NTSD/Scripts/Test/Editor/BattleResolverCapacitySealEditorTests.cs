#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleResolverCapacitySealEditorTests
    {
        private const BindingFlags InternalInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags InternalStatic = BindingFlags.Static | BindingFlags.NonPublic;

        [TestCase(0, 0)]
        [TestCase(17, 17)]
        [TestCase(512, 17)]
        [TestCase(17, 512)]
        public void SealedPrepare_RejectsBeforeChangingWarmedStorageAndLimits(int entityLimit, int trustedLimit)
        {
            using var fixture = new Fixture();
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.PrepareCapacity(17, 17);
            resolver.Configure(fixture.Catalog, fixture.Material);
            Assert.That(resolver.Resolve(fixture.Commands[0], out var prior), Is.EqualTo(BattleCentralResourceStatus.Resolved));
            resolver.SealCapacity();
            var storage = new Storage(resolver);
            int generation = resolver.BindingGeneration;
            int clears = resolver.TemplateClears;

            Assert.Throws<InvalidOperationException>(() => resolver.PrepareCapacity(entityLimit, trustedLimit));

            storage.AssertUnchanged(resolver);
            Assert.That(Field(resolver, "preparedEntityTemplateLimit"), Is.EqualTo(17));
            Assert.That(Field(resolver, "preparedTrustedResourceLimit"), Is.EqualTo(17));
            Assert.That(resolver.BindingGeneration, Is.EqualTo(generation));
            Assert.That(resolver.TemplateClears, Is.EqualTo(clears));
            Assert.That(resolver.Resolve(fixture.Commands[0], out var after), Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(after.Texture, Is.SameAs(prior.Texture));
            Assert.That(after.Material, Is.SameAs(prior.Material));
            Assert.That(after.NormalizedUv, Is.EqualTo(prior.NormalizedUv));
            Assert.That(after.Color, Is.EqualTo(prior.Color));
            Assert.That(resolver.TrustedResourceCacheHits, Is.GreaterThan(0));
        }

        [TestCase(-1, 17)]
        [TestCase(17, -1)]
        public void Prepare_NegativeArgumentStillRejectsBeforeMutation(int entityLimit, int trustedLimit)
        {
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.PrepareCapacity(17, 17);
            resolver.SealCapacity();
            var storage = new Storage(resolver);
            Assert.Throws<ArgumentOutOfRangeException>(() => resolver.PrepareCapacity(entityLimit, trustedLimit));
            storage.AssertUnchanged(resolver);
            Assert.That(Field(resolver, "preparedEntityTemplateLimit"), Is.EqualTo(17));
            Assert.That(Field(resolver, "preparedTrustedResourceLimit"), Is.EqualTo(17));
        }

        [Test]
        public void Unseal_AllowsNextPreparationWithoutDroppingWarmedBindings()
        {
            using var fixture = new Fixture();
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.PrepareCapacity(1, 1);
            resolver.Configure(fixture.Catalog, fixture.Material);
            Assert.That(resolver.Resolve(fixture.Commands[0], out var prior), Is.EqualTo(BattleCentralResourceStatus.Resolved));
            resolver.SealCapacity();
            resolver.UnsealCapacity();
            resolver.PrepareCapacity(512, 512);
            resolver.SealCapacity();
            Assert.That(resolver.Resolve(fixture.Commands[0], out var after), Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(after.Texture, Is.SameAs(prior.Texture));
            Assert.That(after.Material, Is.SameAs(prior.Material));
            Assert.That(after.NormalizedUv, Is.EqualTo(prior.NormalizedUv));
            Assert.That(TemplateCount(resolver), Is.EqualTo(1));
            Assert.That(TrustedCount(resolver), Is.EqualTo(1));
            Assert.That(Field(resolver, "preparedEntityTemplateLimit"), Is.EqualTo(512));
            Assert.That(Field(resolver, "preparedTrustedResourceLimit"), Is.EqualTo(512));
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(17, false)]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(17, true)]
        public void SealedCache_ColdMissesAndFullCacheKeepEveryResolvedOutputWithoutGrowth(int capacity, bool prepared)
        {
            using var fixture = new Fixture();
            var warmup = new BattleCatalogCentralResourceResolver();
            warmup.PrepareCapacity(fixture.Commands.Length, fixture.Commands.Length);
            warmup.Configure(fixture.Catalog, fixture.Material);
            foreach (var command in fixture.Commands)
                Resolve(warmup, command, prepared, out _);

            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.PrepareCapacity(capacity, capacity);
            resolver.SealCapacity();
            resolver.Configure(fixture.Catalog, fixture.Material);
            var storage = new Storage(resolver);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool valid = true;
            for (int iteration = 0; iteration < 64; iteration++)
            {
                resolver.Configure(fixture.Catalog, fixture.Material);
                for (int index = 0; index < fixture.Commands.Length; index++)
                {
                    var command = fixture.Commands[index];
                    var status = Resolve(resolver, command, prepared, out var resource);
                    valid &= status == BattleCentralResourceStatus.Resolved &&
                             ReferenceEquals(resource.Texture, fixture.Texture) &&
                             ReferenceEquals(resource.Material, fixture.Material) &&
                             resource.NormalizedUv == command.NormalizedUv &&
                             resource.PixelSize == command.Size &&
                             resource.Pivot == command.Pivot &&
                             resource.Color.r == command.Color.r &&
                             resource.Color.g == command.Color.g &&
                             resource.Color.b == command.Color.b &&
                             resource.Color.a == command.Color.a;
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(valid, Is.True, "Cache saturation must not discard resolved draws or per-command fields.");
            Assert.That(allocated, Is.Zero, "Scope: Configure plus cold/full-cache Resolve, not the entire central render path.");
            storage.AssertUnchanged(resolver);
            Assert.That(TemplateCount(resolver), Is.EqualTo(capacity));
            Assert.That(TrustedCount(resolver), Is.EqualTo(capacity));
            Assert.That(resolver.SealedCapacityCacheSkips, Is.GreaterThan(0));
            Assert.That(Field(resolver, "preparedEntityTemplateLimit"), Is.EqualTo(capacity));
            Assert.That(Field(resolver, "preparedTrustedResourceLimit"), Is.EqualTo(capacity));
        }

        [Test]
        public void SealedConfigure_MaterialChangesClearBindingsButReusePreparedStorage()
        {
            using var fixture = new Fixture();
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.PrepareCapacity(17, 17);
            resolver.SealCapacity();
            resolver.Configure(fixture.Catalog, fixture.Material);
            Assert.That(resolver.Resolve(fixture.Commands[0], out _), Is.EqualTo(BattleCentralResourceStatus.Resolved));
            var storage = new Storage(resolver);
            int generation = resolver.BindingGeneration;
            resolver.Configure(fixture.Catalog, fixture.AlternateMaterial);
            Assert.That(resolver.BindingGeneration, Is.Not.EqualTo(generation));
            Assert.That(TemplateCount(resolver), Is.Zero);
            Assert.That(TrustedCount(resolver), Is.Zero);
            Assert.That(resolver.Resolve(fixture.Commands[0], out var resource), Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(resource.Material, Is.SameAs(fixture.AlternateMaterial));
            storage.AssertUnchanged(resolver);
            Assert.That(Field(resolver, "capacitySealed"), Is.EqualTo(true));
            Assert.That(Field(resolver, "preparedEntityTemplateLimit"), Is.EqualTo(17));
            Assert.That(Field(resolver, "preparedTrustedResourceLimit"), Is.EqualTo(17));
        }

        [Test]
        public void CentralOwner_SealsBothResolversAndEndAllowsPreparationAgain()
        {
            BattleCentralRenderSystem.ResetRuntime();
            InvokeCentral("EndBattleCapacitySeal");
            try
            {
                InvokeCentral("PrepareBattleCapacity", 1, 8, 17);
                var primary = CentralResolver("CatalogResolver");
                var diagnostic = CentralResolver("DiagnosticCatalogResolver");
                Assert.Throws<InvalidOperationException>(() => primary.PrepareCapacity(512, 512));
                Assert.Throws<InvalidOperationException>(() => diagnostic.PrepareCapacity(512, 512));
                InvokeCentral("EndBattleCapacitySeal");
                Assert.That(Field(primary, "capacitySealed"), Is.EqualTo(false));
                Assert.That(Field(diagnostic, "capacitySealed"), Is.EqualTo(false));
                Assert.DoesNotThrow(() => InvokeCentral("PrepareBattleCapacity", 1, 8, 17));
            }
            finally
            {
                InvokeCentral("EndBattleCapacitySeal");
                BattleCentralRenderSystem.ResetRuntime();
            }
        }

        private static BattleCentralResourceStatus Resolve(BattleCatalogCentralResourceResolver resolver,
            in BattleRenderCommand command, bool prepared, out BattleCentralResolvedResource resource)
        {
            return prepared ? resolver.ResolvePrepared(command, out resource) : resolver.Resolve(command, out resource);
        }

        private static object Field(object owner, string name)
        {
            return owner.GetType().GetField(name, InternalInstance).GetValue(owner);
        }

        private static int TemplateCount(BattleCatalogCentralResourceResolver resolver)
        {
            return ((IDictionary)Field(resolver, "entityTemplates")).Count;
        }

        private static int TrustedCount(BattleCatalogCentralResourceResolver resolver)
        {
            return (int)Field(resolver, "trustedResources").GetType().GetProperty("Count")
                .GetValue(Field(resolver, "trustedResources"));
        }

        private static BattleCatalogCentralResourceResolver CentralResolver(string name)
        {
            return (BattleCatalogCentralResourceResolver)typeof(BattleCentralRenderSystem)
                .GetField(name, InternalStatic).GetValue(null);
        }

        private static object InvokeCentral(string name, params object[] arguments)
        {
            return typeof(BattleCentralRenderSystem).GetMethod(name, InternalStatic).Invoke(null, arguments);
        }

        private sealed class Storage
        {
            private readonly object dictionaryEntries;
            private readonly object trustedKeys;
            private readonly object trustedValues;
            private readonly object hotIdentities;

            public Storage(BattleCatalogCentralResourceResolver resolver)
            {
                dictionaryEntries = DictionaryEntries(Field(resolver, "entityTemplates"));
                object trusted = Field(resolver, "trustedResources");
                trustedKeys = Field(trusted, "keys");
                trustedValues = Field(trusted, "values");
                hotIdentities = Field(resolver, "preparedHotResourceIdentities");
            }

            public void AssertUnchanged(BattleCatalogCentralResourceResolver resolver)
            {
                Assert.That(DictionaryEntries(Field(resolver, "entityTemplates")), Is.SameAs(dictionaryEntries));
                Assert.That(Field(Field(resolver, "trustedResources"), "keys"), Is.SameAs(trustedKeys));
                Assert.That(Field(Field(resolver, "trustedResources"), "values"), Is.SameAs(trustedValues));
                Assert.That(Field(resolver, "preparedHotResourceIdentities"), Is.SameAs(hotIdentities));
            }

            private static object DictionaryEntries(object dictionary)
            {
                var field = dictionary.GetType().GetField("_entries", InternalInstance) ??
                            dictionary.GetType().GetField("entries", InternalInstance);
                Assert.That(field, Is.Not.Null);
                return field.GetValue(dictionary);
            }
        }

        private sealed class Fixture : IDisposable
        {
            public Texture2D Texture { get; }
            public Material Material { get; }
            public Material AlternateMaterial { get; }
            public BattleSpriteCatalog Catalog { get; }
            public BattleRenderCommand[] Commands { get; }

            public Fixture()
            {
                Texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                var shader = Shader.Find(BattleSpriteMaterialContract.CentralTextureShaderName);
                Assert.That(shader, Is.Not.Null);
                Material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                AlternateMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                Material.SetColor("_Color", Color.white);
                AlternateMaterial.SetColor("_Color", Color.white);
                var builder = new BattleSpriteCatalogBuilder();
                for (int index = 0; index < 32; index++)
                    builder.Add(17, index, "resolver-capacity-fixture", Texture, new Rect(0f, 0f, 8f, 8f), null);
                Catalog = builder.Publish();
                Commands = new BattleRenderCommand[Catalog.Count];
                ConstructorInfo trustedConstructor = null;
                foreach (var candidate in typeof(BattleRenderCommand).GetConstructors(InternalInstance))
                {
                    var parameters = candidate.GetParameters();
                    if (parameters.Length == 28 &&
                        parameters[14].ParameterType == typeof(BattleSpriteRenderState) &&
                        parameters[15].ParameterType == typeof(BattleSpriteValueDescriptor) &&
                        parameters[16].ParameterType == typeof(object))
                        trustedConstructor = candidate;
                }
                Assert.That(trustedConstructor, Is.Not.Null, "Use the existing trusted command constructor shape.");
                for (int index = 0; index < Commands.Length; index++)
                {
                    Catalog.TryGet(17, index, out var entry);
                    var color = new Color32((byte)(index + 1), 20, 30, 255);
                    var state = new BattleSpriteRenderState(color, false, false, SpriteMaskInteraction.None,
                        BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
                    var descriptor = new BattleSpriteValueDescriptor(true, false, 0, Texture.GetInstanceID(), 0,
                        entry.PixelRect, entry.Pivot, BattleVisualResourceKey.FromEntity(entry.Key));
                    Commands[index] = (BattleRenderCommand)trustedConstructor.Invoke(new object[]
                    {
                        BattleRenderCommandType.Entity, new RuntimeEntityHandle(index, 1), index + 1,
                        17, index, 0, index, index, 0, index, Vector3.zero,
                        entry.PixelRect.size, entry.Pivot, entry.NormalizedUv, state, descriptor, entry,
                        false, 0, 0, 0, Vector2.zero, false, Vector2.zero, false, false, 1f,
                        default(BattlePresentationMotionAnchor),
                    });
                }
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(AlternateMaterial);
                UnityEngine.Object.DestroyImmediate(Material);
                UnityEngine.Object.DestroyImmediate(Texture);
            }
        }
    }
}
#endif
