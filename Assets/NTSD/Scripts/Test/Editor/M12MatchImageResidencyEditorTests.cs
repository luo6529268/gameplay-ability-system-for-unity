#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class M12MatchImageResidencyEditorTests
    {
        private NTSD28B11AtomicPublicationEditorTests fixture;
        private CharacterAnimtorManager manager;
        private GameDataManager data;
        private CharacterUIResourceManager ui;

        private static T Field<T>(object owner, string name)
        {
            return (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);
        }

        [SetUp]
        public void SetUp()
        {
            fixture = new NTSD28B11AtomicPublicationEditorTests
            {
                EvidenceDirectory = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                    "artifacts/diagnostics/M12-match-image-residency-20261013/editor-owner-evidence")
            };
            fixture.SetUpUsingProductionDefault();
            manager = fixture.M12Manager;
            data = Field<GameDataManager>(fixture, "data");
            ui = Field<CharacterUIResourceManager>(fixture, "ui");
            typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>).GetField(
                "_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, manager);
            typeof(MoreMountains.Tools.MMSingleton<GameDataManager>).GetField(
                "_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, data);
            typeof(MoreMountains.Tools.MMSingleton<CharacterUIResourceManager>).GetField(
                "_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, ui);
        }

        [TearDown]
        public void TearDown()
        {
            if (manager != null) manager.NativeBeforeUploadForDiagnostics = null;
            fixture?.TearDown();
        }

        private LoganVisualContentCandidate TwoDefinitionCandidate()
        {
            var method = typeof(NTSD28B11AtomicPublicationEditorTests).GetMethod(
                "MultiDefinitionCandidate", BindingFlags.NonPublic | BindingFlags.Instance);
            return (LoganVisualContentCandidate)method.Invoke(fixture, new object[] { "ImagePhases", true });
        }

        private async UniTask<bool> Load(LoganVisualContentCandidate candidate, BattleImageLoadPlan plan)
        {
            var observe = typeof(NTSD28B11AtomicPublicationEditorTests).GetMethod(
                "ObserveOwners", BindingFlags.NonPublic | BindingFlags.Instance);
            observe.Invoke(fixture, null);
            try
            {
                return await manager.LoadPlannedImagesForOwnersAsync(candidate, data, ui, plan,
                    _ => observe.Invoke(fixture, null));
            }
            finally { observe.Invoke(fixture, null); }
        }

        private BattleImageLoadPlan Only56(LoganVisualContentCandidate candidate)
        {
            return BattleVisualDependencySelector.Build(Field<Dictionary<int, LF2CharacterDataWrapper>>(manager, "TotalCharacterFrameConfig"),
                candidate.Catalog.Entries.ToDictionary(entry => entry.Id, entry => entry.Type),
                new[] { 56 }, null, null, includeScenario: false, includeCompatibility: false);
        }

        [UnityTest]
        public IEnumerator MenuPublishesAllDefinitionsAndUiWithoutBattleTextures()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = TwoDefinitionCandidate();
                Assert.That(await Load(candidate, BattleImageLoadPlan.Menu()), Is.True);
                Assert.That(Field<Dictionary<int, LF2CharacterDataWrapper>>(manager, "TotalCharacterFrameConfig").Count, Is.EqualTo(2));
                Assert.That(data.GetObjectById(56), Is.Not.Null);
                Assert.That(data.GetObjectById(57), Is.Not.Null);
                Assert.That(ui.GetCharacterUISprites(56).HeadSprite, Is.Not.Null);
                Assert.That(ui.GetCharacterUISprites(57).SmallSprite, Is.Not.Null);
                Assert.That(manager.SpriteCatalog.Count, Is.Zero);
                Assert.That(manager.CommonVisualCatalog.IsRuntimeReady, Is.False);
                Assert.That(manager.ResidentBodyDefinitionCount, Is.Zero);
                Assert.That(manager.LastSelectedBodySheetDeclarationCount, Is.Zero);
                Assert.That(manager.IsPublishedContentUsable, Is.True);
            });
        }

        [UnityTest]
        public IEnumerator MatchLoadsOnlySelectedBodyAndCarriesTheSameUiAndDefinitions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = TwoDefinitionCandidate();
                Assert.That(await Load(candidate, BattleImageLoadPlan.Menu()), Is.True);
                Sprite head = ui.GetCharacterUISprites(56).HeadSprite;
                Sprite small = ui.GetCharacterUISprites(57).SmallSprite;
                var definition = data.GetObjectById(56);
                Assert.That(await Load(candidate, Only56(candidate)), Is.True);
                Assert.That(Field<Dictionary<int, List<Sprite>>>(manager, "MergedSprites").Keys, Is.EquivalentTo(new[] { 56 }));
                Assert.That(manager.SpriteCatalog.Entries.Keys.All(key => key.VisualDataId == 56), Is.True);
                Assert.That(manager.LastSelectedBodySheetDeclarationCount, Is.EqualTo(1));
                Assert.That(manager.CommonVisualCatalog.IsRuntimeReady, Is.True);
                Assert.That(ui.GetCharacterUISprites(56).HeadSprite, Is.SameAs(head));
                Assert.That(ui.GetCharacterUISprites(57).SmallSprite, Is.SameAs(small));
                Assert.That(data.GetObjectById(56), Is.SameAs(definition));
                Assert.That(head != null && small != null, Is.True);
                Assert.That(manager.IsPublishedContentUsable, Is.True);
            });
        }

        [UnityTest]
        public IEnumerator CancelledMatchKeepsMenuUiAndPublishedDefinitions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = TwoDefinitionCandidate();
                Assert.That(await Load(candidate, BattleImageLoadPlan.Menu()), Is.True);
                Sprite head = ui.GetCharacterUISprites(56).HeadSprite;
                var definition = data.GetObjectById(57);
                bool cancelled = false;
                manager.NativeBeforeUploadForDiagnostics = () =>
                {
                    cancelled = true;
                    manager.CancelNativeContentPrewarm();
                    return UniTask.CompletedTask;
                };
                Assert.That(await Load(candidate, Only56(candidate)), Is.False);
                Assert.That(cancelled, Is.True);
                await manager.WaitForNativePrewarmDrainAsync();
                Assert.That(manager.PublishedImageLoadPlan.Phase, Is.EqualTo(BattleImageLoadPhase.Menu));
                Assert.That(manager.SpriteCatalog.Count, Is.Zero);
                Assert.That(ui.GetCharacterUISprites(56).HeadSprite, Is.SameAs(head));
                Assert.That(head != null && head.texture != null, Is.True);
                Assert.That(data.GetObjectById(57), Is.SameAs(definition));
                Assert.That(manager.HasPendingNativePrewarmWork, Is.False);
                Assert.That(manager.IsPublishedContentUsable, Is.True);
            });
        }

        [UnityTest]
        public IEnumerator MenuRetirementWaitsForLastCatalogLeaseAndKeepsUi()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = TwoDefinitionCandidate();
                Assert.That(await Load(candidate, BattleImageLoadPlan.Menu()), Is.True);
                Assert.That(await Load(candidate, Only56(candidate)), Is.True);
                Sprite head = ui.GetCharacterUISprites(56).HeadSprite;
                Sprite body = Field<Dictionary<int, List<Sprite>>>(manager, "MergedSprites")[56].First(value => value != null);
                var lease = manager.AcquireCentralCatalogLease(manager.SpriteCatalog);
                try
                {
                    Assert.That(manager.TryReleaseBattleImagesForMenu(out string blocked), Is.False);
                    Assert.That(blocked, Does.Contain("leases"));
                    Assert.That(body != null && head != null, Is.True);
                }
                finally { lease.Dispose(); }
                Assert.That(manager.TryReleaseBattleImagesForMenu(out _), Is.True);
                Assert.That(manager.ResidentBodyDefinitionCount, Is.Zero);
                Assert.That(manager.SpriteCatalog.Count, Is.Zero);
                Assert.That(manager.CommonVisualCatalog.IsRuntimeReady, Is.False);
                Assert.That(body == null, Is.True, "Retired native Sprite must be destroyed at the zero-lease boundary.");
                Assert.That(head != null && head.texture != null, Is.True);
                Assert.That(manager.IsPublishedContentUsable, Is.True);
                Assert.That(manager.TryReleaseBattleImagesForMenu(out _), Is.True, "Menu retirement is idempotent.");
            });
        }

        [UnityTest]
        public IEnumerator SharedPageUsesEachDatFrameMappingWithOneTexture()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var paging = new M12SpritePagingEditorTests();
                var sprites = new HashSet<Sprite>();
                var textures = new HashSet<Texture2D>();
                try
                {
                    paging.SetUp();
                    var manifest = Field<Newtonsoft.Json.Linq.JObject>(paging, "manifest");
                    var rects = manifest["sources"][1]["chunks"][0]["rects"];
                    var first = rects[0].DeepClone();
                    rects[0] = rects[1].DeepClone();
                    rects[1] = first;
                    const BindingFlags hiddenInstance = BindingFlags.NonPublic | BindingFlags.Instance;
                    const BindingFlags hiddenStatic = BindingFlags.NonPublic | BindingFlags.Static;
                    typeof(M12SpritePagingEditorTests).GetMethod("PublishManifest", hiddenInstance).Invoke(paging, null);
                    var candidate = (LoganVisualContentCandidate)typeof(M12SpritePagingEditorTests)
                        .GetMethod("Capture", hiddenInstance).Invoke(paging, null);
                    var configs = candidate.CopyCharacterConfigs();
                    object declarations = typeof(M12SpritePagingEditorTests).GetMethod("Declarations", hiddenStatic)
                        .Invoke(null, new object[] { configs });
                    var groups = (IList)typeof(CharacterAnimtorManager).GetMethod("BuildNativeBodySheetGroupsWithReuse", hiddenStatic)
                        .Invoke(null, new object[] { declarations, candidate, true, false });
                    Assert.That(groups.Count, Is.EqualTo(1));
                    var metrics = new CharacterAnimtorManager.NativeDecodeAdmission(1000000, 1000000);
                    object operation = typeof(M12SpritePagingEditorTests).GetMethod("Operation", hiddenStatic)
                        .Invoke(null, new object[] { candidate, true, metrics, null });
                    var byId = new Dictionary<int, List<Sprite>>
                    {
                        [100] = new List<Sprite> { null, null }, [101] = new List<Sprite> { null, null }
                    };
                    using var cpu = new System.Threading.SemaphoreSlim(0, 1);
                    using var upload = new System.Threading.SemaphoreSlim(1, 1);
                    var result = await (UniTask<(int created, int failed)>)typeof(CharacterAnimtorManager)
                        .GetMethod("ProcessNativeBodySheetGroupAsync", hiddenStatic).Invoke(null, new object[]
                        {
                            groups[0], byId, sprites, textures, new List<BattleAtlasSourcePixels>(),
                            new Dictionary<string, Sprite>(), new Dictionary<Sprite, string>(), null, cpu, upload,
                            Field<BattleContentSource>(paging, "source"), new Func<bool>(() => true), operation, null
                        });
                    Assert.That(result, Is.EqualTo((4, 0)));
                    Assert.That(textures.Count, Is.EqualTo(1));
                    Assert.That(byId[100][0].texture, Is.SameAs(byId[101][0].texture));
                    Assert.That(byId[100][0].rect, Is.EqualTo(new Rect(1, 1, 2, 2)));
                    Assert.That(byId[101][0].rect, Is.EqualTo(new Rect(5, 1, 2, 2)));
                    Assert.That(byId[101][1].rect, Is.EqualTo(byId[100][0].rect));
                    Assert.That(metrics.PendingPixels + metrics.RetainedPixels + metrics.Active, Is.Zero);
                    Assert.That(cpu.CurrentCount, Is.EqualTo(1));
                    Assert.That(upload.CurrentCount, Is.EqualTo(1));
                }
                finally
                {
                    CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures);
                    paging.TearDown();
                }
            });
        }

        [UnityTest]
        public IEnumerator DeployedFormalPagesAndAstcManifestCaptureOnWorker()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var mode = ProjectBattleModeConfig.LoadDefault().Capture();
                var source = BattleContentSource.ForLoganRuntime(Path.GetFullPath("Assets/NTSD/Content/LoganRuntime"));
                var candidate = await UniTask.RunOnThreadPool(() => LoganVisualContentCandidate.Capture(source, mode));
                Assert.That(candidate.Catalog.Entries.Count, Is.EqualTo(330));
                Assert.That(candidate.DerivedPages.Pages.Count, Is.EqualTo(56));
                Assert.That(candidate.AstcTextures, Is.Not.Null);
                candidate.AssertInputsCurrent();
                var configs = candidate.CopyCharacterConfigs();
                var plan = BattleVisualDependencySelector.Build(configs,
                    candidate.Catalog.Entries.ToDictionary(entry => entry.Id, entry => entry.Type),
                    new[] { 28, 18, 65, 23, 8, 10, 66, 27 }, candidate.Catalog.FusionInput.Catalog,
                    candidate.Catalog.KindInput.Catalog, includeScenario: true, includeCompatibility: true);
                var declarations = new List<(int characterId, SpriteFileInfo fileInfo, HashSet<int> ownedEffectivePics)>();
                const BindingFlags hiddenStatic = BindingFlags.NonPublic | BindingFlags.Static;
                foreach (var config in plan.SelectBodyConfigs(configs))
                {
                    var ownership = (List<HashSet<int>>)typeof(CharacterAnimtorManager)
                        .GetMethod("BuildFirstDeclaredSpriteOwnership", hiddenStatic)
                        .Invoke(null, new object[] { config.characterData.files });
                    for (int index = 0; index < ownership.Count; index++)
                        if (ownership[index].Count != 0)
                            declarations.Add((config.characterId, config.characterData.files[index], ownership[index]));
                }
                Assert.That(plan.DefinitionIds.Count, Is.EqualTo(82));
                Assert.That(declarations.Count, Is.EqualTo(130));
                var groups = (IList)typeof(CharacterAnimtorManager).GetMethod("BuildNativeBodySheetGroupsWithReuse", hiddenStatic)
                    .Invoke(null, new object[] { declarations, candidate, true, false });
                Assert.That(groups.Count, Is.EqualTo(122), "One physical decode per selected source or derived page.");
                var compressedGroups = (IList)typeof(CharacterAnimtorManager).GetMethod("BuildNativeBodySheetGroupsWithReuse", hiddenStatic)
                    .Invoke(null, new object[] { declarations, candidate, true, true });
                Assert.That(compressedGroups.Count, Is.LessThanOrEqualTo(122));
            });
        }

        [Test]
        public void BakedClampSchemaStillRejectsUnknownProperties()
        {
            const string known = "{\"version\":1,\"generatorVersion\":\"NTSD_DAT_PAGES_V1\",\"sources\":[{\"clampBaked\":true}]}";
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Error
            };
            var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<BattleDerivedSpritePages.ManifestJson>(known, settings);
            Assert.That(manifest.sources[0].clampBaked, Is.True);
            Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() =>
                Newtonsoft.Json.JsonConvert.DeserializeObject<BattleDerivedSpritePages.ManifestJson>(
                    known.Replace("clampBaked", "unrecognizedProperty"), settings));
        }

        [UnityTest]
        public IEnumerator ExplicitSourceTextureModeDoesNotReserveAnAtlas()
        {
            return UniTask.ToCoroutine(async () =>
            {
                GameConfig.Instance.BattleAtlasModeName = "SourceTexture2D";
                var candidate = TwoDefinitionCandidate();
                Assert.That(await Load(candidate, BattleImageLoadPlan.Menu()), Is.True);
                Assert.That(await Load(candidate, Only56(candidate)), Is.True);
                Assert.That(manager.LastNativeImageMemoryBudget.ReservedAtlasBytes, Is.Zero,
                    "SourceTexture2D explicitly disables atlas allocation in the existing policy.");
                Assert.That(manager.LastAtlasDiagnosticInputs.Decision.EffectiveMode,
                    Is.EqualTo(NTSD.Animation.Rendering.BattleAtlasPolicyMode.SourceTexture2D));
                int reads = 0, preReads = 0;
                bool success = CharacterAnimtorManager.TryBuildMatchCentralAtlasPublication(
                    manager.SpriteCatalog, manager.CommonVisualCatalog, new[] { 56 },
                    NTSD.Animation.Rendering.BattleRenderingDeviceCapabilities.FromSystem(), GameConfig.Instance,
                    Array.Empty<string>(), out var rebound, out var common, out var resources,
                    out string diagnostic, out var decision, out _, (texture, path) =>
                    {
                        reads++;
                        throw new InvalidOperationException("Explicit SourceTexture2D must not read atlas pixels.");
                    }, sources =>
                    {
                        preReads++;
                        throw new InvalidOperationException("Explicit SourceTexture2D must not reserve an atlas.");
                    });
                Assert.That(success, Is.True, diagnostic);
                Assert.That(reads + preReads, Is.Zero);
                Assert.That(resources.Count, Is.Zero);
                Assert.That(decision.EffectiveMode, Is.EqualTo(NTSD.Animation.Rendering.BattleAtlasPolicyMode.SourceTexture2D));
                Assert.That(rebound.Entries.Values.All(entry => entry.CentralBinding.Mode == BattleSpriteCentralBindingMode.SourceTexture2D), Is.True);
                Assert.That(common.IsRuntimeReady, Is.True);
                const string atlasRequested = "OrderedPages still invokes the pre-read gate.";
                var expected = Assert.Throws<InvalidOperationException>(() =>
                    CharacterAnimtorManager.TryBuildMatchCentralAtlasPublication(
                        manager.SpriteCatalog, manager.CommonVisualCatalog, new[] { 56 },
                        NTSD.Animation.Rendering.BattleRenderingDeviceCapabilities.FromSystem(), GameConfig.Instance,
                        new[] { NTSD.Animation.Rendering.BattleRenderingPolicyResolver.AtlasModeArgument, "OrderedPages" },
                        out _, out _, out _, out _, out _, out _, null,
                        sources => throw new InvalidOperationException(atlasRequested)));
                Assert.That(expected.Message, Is.EqualTo(atlasRequested));
            });
        }

        [UnityTest]
        public IEnumerator FullCatalogueFallbackStillPublishesBothBodies()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = TwoDefinitionCandidate();
                manager.ConfigureMatchImageResidency(false);
                Assert.That(await fixture.M12Load(candidate), Is.True);
                Assert.That(Field<Dictionary<int, List<Sprite>>>(manager, "MergedSprites").Keys, Is.EquivalentTo(new[] { 56, 57 }));
                Assert.That(manager.PublishedImageLoadPlan.Phase, Is.EqualTo(BattleImageLoadPhase.FullCatalogue));
                Assert.That(manager.TryReleaseBattleImagesForMenu(out _), Is.True);
                Assert.That(manager.ResidentBodyDefinitionCount, Is.EqualTo(2));
            });
        }

        [TestCase("human")]
        [TestCase("team")]
        [TestCase("input")]
        [TestCase("ai")]
        [TestCase("physical-slot")]
        [TestCase("stage")]
        [TestCase("mode")]
        [TestCase("seed")]
        [TestCase("name")]
        [TestCase("background")]
        [TestCase("difficulty")]
        [TestCase("one-tu")]
        [TestCase("knockout")]
        [TestCase("bracket")]
        public void ConfigurationChangeDuringImageAwaitRefusesStaleRoster(string changed)
        {
            var match = new MatchConfig { gameMode = new GameModeConfig() };
            match.players.Add(new PlayerSlotConfig { use = true, isHuman = true, characterId = 56 });
            match.players.Add(null);
            string fingerprint = AppManager.MatchImageConfigurationFingerprint(match);
            Assert.That(AppManager.MatchImageConfigurationStillCurrent(match, fingerprint, match), Is.True);
            switch (changed)
            {
                case "human": match.players[0].isHuman = false; break;
                case "team": match.players[0].team = 2; break;
                case "input": match.players[0].inputId = 2; break;
                case "ai": match.players[0].aiId = 2; break;
                case "physical-slot": match.players.Reverse(); break;
                case "stage": match.stageCampaignFilePath = "changed.dat"; break;
                case "mode": match.gameMode.battleGameModeId = 2; break;
                case "seed": match.seed = 2; break;
                case "name": match.nativeBattlePlayerNames.Add("changed"); break;
                case "background": match.backgroundId = 2; break;
                case "difficulty": match.difficulty = 0; break;
                case "one-tu": match.oneTuInput = true; break;
                case "knockout": match.nativeKnockoutFeedRuntimeDisplayEnabled = false; break;
                case "bracket": match.nativeBracketPlayerNames.Add(true); break;
            }
            Assert.That(AppManager.MatchImageConfigurationStillCurrent(match, fingerprint, match), Is.False);
        }

        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, 0, false, false, 0, true, true)]
        [TestCase(BattleRuntimeLifecycleState.Running, 0, 0, false, false, 0, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Stopping, 0, 0, false, false, 0, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Stopped, 0, 0, false, false, 0, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 1, 0, false, false, 0, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, 1, false, false, 0, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, 0, true, false, 0, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, 0, false, true, 0, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, 0, false, false, 1, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, 0, false, false, 0, false, false)]
        public void MatchPreparationRefusesLiveSealedLeasedOrStaleWorld(
            BattleRuntimeLifecycleState lifecycle, int objects, int slots, bool sealedData,
            bool sealedCapacity, int leases, bool appAllows, bool expected)
        {
            Assert.That(CharacterAnimtorManager.MatchImagePreparationBoundaryAllows(lifecycle,
                objects, slots, sealedData, sealedCapacity, leases, appAllows), Is.EqualTo(expected));
        }
    }
}
#endif
