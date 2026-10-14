using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.UI;
using UnityEngine;

namespace NTSD.Animation
{
    public partial class CharacterAnimtorManager
    {
        [SerializeField] private bool useMatchImageResidency = true;
        private BattleImageLoadPlan publishedImageLoadPlan;

        internal BattleImageLoadPlan PublishedImageLoadPlan => publishedImageLoadPlan;
        internal BattleImageMemoryBudget LastNativeImageMemoryBudget { get; private set; }
        internal int LastSelectedBodySheetDeclarationCount { get; private set; }
        public int ResidentBodyDefinitionCount => MergedSprites.Count;
        public string PublishedImageLoadPlanFingerprint => publishedImageLoadPlan?.Fingerprint;
        public string LastImageResidencyDiagnostic { get; private set; }

        public bool UseMatchImageResidency => useMatchImageResidency;

        public void ConfigureMatchImageResidency(bool enabled)
        {
            if (activeNativePrewarm != null || spritePrewarmDisposed || !NativeContentBoundaryIsOpen())
                throw new InvalidOperationException("Image phase mode requires an inactive content boundary.");
            useMatchImageResidency = enabled;
        }

        internal bool UseMatchImageResidencyForDiagnostics
        {
            get => UseMatchImageResidency;
            set => ConfigureMatchImageResidency(value);
        }

        private bool IsPublishedImagePlanConsistent()
        {
            if (publishedImageLoadPlan == null)
                return true;
            if (MergedSprites.Count != publishedImageLoadPlan.DefinitionIds.Count)
                return false;
            foreach (int id in publishedImageLoadPlan.DefinitionIds)
                if (!MergedSprites.ContainsKey(id)) return false;
            foreach (BattleSpriteKey key in SpriteCatalog.Entries.Keys)
                if (!publishedImageLoadPlan.IncludesDefinition(key.VisualDataId)) return false;
            return publishedImageLoadPlan.Phase == BattleImageLoadPhase.Menu
                ? SpriteCatalog.Count == 0
                : CommonVisualCatalog.IsRuntimeReady;
        }

        private static Dictionary<int, CharacterUISprites> CaptureReusableNativeUi(
            NativePrewarmOperation operation, IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs)
        {
            var result = new Dictionary<int, CharacterUISprites>();
            foreach (var entry in configs)
            {
                LF2CharacterData data = entry.Value.characterData;
                if (string.IsNullOrEmpty(data.head) && string.IsNullOrEmpty(data.small)) continue;
                CharacterUISprites current = operation.UI.GetCharacterUISprites(entry.Key);
                if (current == null || (!string.IsNullOrEmpty(data.head) && current.HeadSprite == null) ||
                    (!string.IsNullOrEmpty(data.small) && current.SmallSprite == null))
                    throw new InvalidOperationException("Published menu UI is incomplete for OID " + entry.Key);
                var copy = new CharacterUISprites { HeadSprite = current.HeadSprite, SmallSprite = current.SmallSprite };
                result.Add(entry.Key, copy);
                foreach (Sprite sprite in new[] { copy.HeadSprite, copy.SmallSprite })
                {
                    if (sprite == null) continue;
                    operation.BorrowedUiSprites.Add(sprite);
                    operation.BorrowedUiResources.Add(sprite.texture);
                }
            }
            return result;
        }

        private static bool IsBorrowedNativeUiCurrent(NativePrewarmOperation operation)
        {
            if (operation.Data.PublishedVisualContentKey != operation.Candidate.SourceCacheKey ||
                operation.UI.PublishedVisualContentKey != operation.Candidate.SourceCacheKey)
                return false;
            foreach (var entry in operation.ReusedUi)
            {
                CharacterUISprites current = operation.UI.GetCharacterUISprites(entry.Key);
                if (current == null || current.HeadSprite != entry.Value.HeadSprite ||
                    current.SmallSprite != entry.Value.SmallSprite ||
                    (entry.Value.HeadSprite != null && entry.Value.HeadSprite.texture == null) ||
                    (entry.Value.SmallSprite != null && entry.Value.SmallSprite.texture == null))
                    return false;
            }
            return true;
        }

        private async UniTask<bool> LoadNativeMenuImagesAsync(NativePrewarmOperation operation,
            Dictionary<int, LF2CharacterDataWrapper> configs, Action<string> progress)
        {
            var declarations = new List<(int characterId, SpriteFileInfo fileInfo, HashSet<int> ownedEffectivePics)>();
            BattleRenderingDeviceCapabilities capabilities = BattleRenderingDeviceCapabilities.FromSystem();
            operation.MemoryBudget = await RunNativeImageBudgetPreflightAsync(operation, declarations, configs,
                EstimateOwnedImageBytes(), capabilities.AtlasMemoryBudgetBytes, capabilities.MaxTextureSize);
            LastNativeImageMemoryBudget = operation.MemoryBudget;
            LastSelectedBodySheetDeclarationCount = 0;
            if (!CanCompleteSpritePrewarmInvocation(operation.Invocation)) return false;
            Dictionary<int, CharacterUISprites> ui = await PrepareNativeUISpritesAsync(operation, configs, progress);
            if (!CanCompleteSpritePrewarmInvocation(operation.Invocation)) return false;
            await RunNativePrewarmWorkAsync(operation.Candidate.AssertInputsCurrent);
            await UniTask.SwitchToMainThread();
            bool committed = TryCommitNativePublication(operation, configs, new Dictionary<int, List<Sprite>>(),
                BattleSpriteCatalog.Empty, ui, BattleCommonVisualCatalog.Empty, "Menu images: catalog and UI only.");
            if (committed)
            {
                LastImageResidencyDiagnostic = "Menu: 0 body definitions; existing catalog and all UI retained.";
                PrewarmCompleted?.Invoke();
            }
            return committed;
        }

        internal UniTask<bool> LoadPlannedImagesForOwnersAsync(LoganVisualContentCandidate candidate,
            GameDataManager data, CharacterUIResourceManager ui, BattleImageLoadPlan plan,
            Action<string> progress = null)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            return LoadLoganContentWithScopeAsync(candidate, data, ui, progress, null, plan);
        }

        internal static bool MatchImagePreparationBoundaryAllows(BattleRuntimeLifecycleState lifecycle,
            int objectCount, int claimedSlots, bool sealedData, bool sealedCapacity, int catalogLeases, bool appAllows)
        {
            return appAllows && lifecycle == BattleRuntimeLifecycleState.Preparing && objectCount == 0 &&
                claimedSlots == 0 && !sealedData && !sealedCapacity && catalogLeases == 0;
        }

        public async UniTask<bool> PrepareBattleImagesAsync(SimulationTickDriver driver,
            IReadOnlyCollection<int> rootIds, Action<string> progress = null)
        {
            LoganVisualContentCandidate candidate = publishedLoganCandidate;
            if (candidate == null || !IsLoganPublicationCurrent(candidate))
                throw new InvalidOperationException("Menu content must be published before match images.");
            if (driver == null || driver.World == null)
                throw new InvalidOperationException("Match images require a Preparing World.");
            int generation = driver.PreparationGeneration;
            SimulationWorld world = driver.World;
            BattleStageCampaignSet campaigns = world.StageCampaigns;
            string configuredRoot = ConfiguredContentRoot;
            bool IsCurrent()
            {
                var app = NTSD.App.AppManager.Instance;
                bool appAllows = app == null || app.State == NTSD.App.AppFlowState.BattleLoading;
                return this != null && !spritePrewarmDisposed && driver != null &&
                    driver.IsBattlePreparationCurrent(generation, world) &&
                    ReferenceEquals(publishedLoganCandidate, candidate) && ConfiguredContentRoot == configuredRoot &&
                    ReferenceEquals(campaigns, world.StageCampaigns) &&
                    MatchImagePreparationBoundaryAllows(driver.LifecycleState, world.ObjectCount,
                        world.ClaimedRuntimeSlotCountForServices, world.RuntimeDataCatalog.IsSealedForBattle,
                        world.RuntimeCapacity.IsSealed, spriteCatalogRendererBindings.Values.Sum(), appAllows);
            }
            if (!IsCurrent()) throw new InvalidOperationException("Match image preparation boundary is closed.");
            BattleImageLoadPlan plan;
            if (!useMatchImageResidency)
            {
                plan = BattleImageLoadPlan.FullCatalogue(TotalCharacterFrameConfig.Keys, "Full image preload explicitly selected.");
            }
            else if (rootIds == null || rootIds.Count == 0)
            {
                plan = BattleImageLoadPlan.FullCatalogue(TotalCharacterFrameConfig.Keys, "No resolved match roots; preserve full catalogue fallback.");
            }
            else
            {
                var types = candidate.Catalog.Entries.ToDictionary(entry => entry.Id, entry => entry.Type);
                var stageRoots = new HashSet<int>();
                foreach (BattleStageCampaignValue campaign in campaigns.Campaigns)
                    foreach (BattleStagePhaseValue phase in campaign.Phases)
                        foreach (BattleStageSpawnValue spawn in phase.Spawns)
                            if (spawn.Id >= 0) stageRoots.Add(spawn.Id);
                try
                {
                    plan = BattleVisualDependencySelector.Build(TotalCharacterFrameConfig, types, rootIds,
                        candidate.Catalog.FusionInput.Catalog, candidate.Catalog.KindInput.Catalog,
                        stageRoots, includeScenario: true, includeCompatibility: true);
                }
                catch (ArgumentException error)
                {
                    plan = BattleImageLoadPlan.FullCatalogue(TotalCharacterFrameConfig.Keys,
                        "Unresolved match resource roots: " + error.Message);
                }
            }
            if (publishedImageLoadPlan?.Fingerprint == plan.Fingerprint &&
                publishedImageLoadPlan.Phase != BattleImageLoadPhase.Menu && IsPublishedImagePlanConsistent())
                return true;
            bool loaded = await LoadLoganContentWithScopeAsync(candidate, GameDataManager.TryGetInstance(),
                CharacterUIResourceManager.TryGetInstance(), progress, IsCurrent, plan, IsCurrent);
            if (!loaded || !IsCurrent()) return false;
            LastImageResidencyDiagnostic = plan.Phase + ": " + plan.DefinitionIds.Count +
                " body definitions, " + LastSelectedBodySheetDeclarationCount + " declarations; " +
                plan.MissingDependencies.Count + " existing unresolved references. " + plan.FallbackReason;
            Debug.Log("[Match image residency] " + LastImageResidencyDiagnostic);
            return true;
        }

        internal bool TryReleaseBattleImagesForMenu(out string diagnostic)
        {
            if (!useMatchImageResidency || publishedLoganCandidate == null ||
                publishedImageLoadPlan?.Phase == BattleImageLoadPhase.Menu)
            {
                diagnostic = string.Empty;
                return true;
            }
            var driver = SimulationTickDriver.Instance;
            var world = driver != null ? driver.World : null;
            if (spritePrewarmDisposed || activeNativePrewarm != null || HasPendingNativePrewarmWork ||
                (driver != null && driver.LifecycleState != BattleRuntimeLifecycleState.Stopped &&
                    driver.LifecycleState != BattleRuntimeLifecycleState.Uninitialized) ||
                (world != null && (world.ObjectCount != 0 || world.ClaimedRuntimeSlotCountForServices != 0)) ||
                spriteCatalogRendererBindings.Values.Any(count => count != 0))
            {
                diagnostic = "Battle images retained: work, entities, or catalog leases remain at the menu boundary.";
                return false;
            }
            var ui = CharacterUIResourceManager.TryGetInstance();
            if (ui == null || ui.PublishedVisualContentKey != PublishedVisualContentKey)
            {
                diagnostic = "Battle images retained: UI source identity is no longer current.";
                return false;
            }
            var keptSprites = new HashSet<Sprite>();
            var keptResources = new HashSet<UnityEngine.Object>();
            foreach (int id in TotalCharacterFrameConfig.Keys)
            {
                CharacterUISprites value = ui.GetCharacterUISprites(id);
                if (value == null) continue;
                foreach (Sprite sprite in new[] { value.HeadSprite, value.SmallSprite })
                {
                    if (sprite == null) continue;
                    keptSprites.Add(sprite);
                    keptResources.Add(sprite.texture);
                }
            }
            BattleSpriteCatalog previousCatalog = SpriteCatalog;
            var oldSprites = new HashSet<Sprite>(publishedOwnedSprites);
            var oldResources = new HashSet<UnityEngine.Object>(publishedOwnedResources);
            oldSprites.ExceptWith(keptSprites);
            oldResources.ExceptWith(keptResources);
            // Alignment contract: NTSD-M12-MATCH-IMAGE-RESIDENCY-001. Only after completed shutdown/unload.
            SpriteCatalog = BattleSpriteCatalog.Empty;
            CommonVisualCatalog = BattleCommonVisualCatalog.Empty;
            MergedSprites = new Dictionary<int, List<Sprite>>();
            publishedOwnedSprites = keptSprites;
            publishedOwnedResources = keptResources;
            publishedImageLoadPlan = BattleImageLoadPlan.Menu();
            LastSelectedBodySheetDeclarationCount = 0;
            LastNativeImageMemoryBudget = null;
            QueueRetiredSpritePublication(previousCatalog, oldSprites, oldResources);
            RetireSupersededSpritePublicationsAfterRefreshBoundary();
            LastImageResidencyDiagnostic = "Menu: battle-only textures retired; DAT and UI publication retained.";
            diagnostic = string.Empty;
            return true;
        }
    }
}
