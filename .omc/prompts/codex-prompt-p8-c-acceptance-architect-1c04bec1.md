---
provider: "codex"
agent_role: "architect"
model: "gpt-5.6-sol"
files:
  - "Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingAcceptanceHarness.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingAcceptanceEditorTests.cs"
  - "Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs"
  - "Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattleCentralPresentationMountRegistry.cs"
  - "Temp/P8-C-PostFix-EditMode/P8-C-report.json"
  - "Temp/P8-C-PostFix-RequestedUnavailable/P8-C-report.json"
timestamp: "2026-07-23T08:49:06.522Z"
---

--- File: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingAcceptanceHarness.cs ---
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering.Editor
{
    [Serializable]
    public sealed class BattleRenderingAcceptanceRequest
    {
        public string outputDirectory = "Temp/P8-C-Acceptance";
        public int imageSize = 256;
        public bool exerciseLivePool;
        public int livePoolExtraCount = 1;
    }

    public readonly struct BattleRenderingAcceptanceConfig
    {
        public BattleRenderingAcceptanceConfig(
            string outputDirectory,
            int imageSize,
            bool exerciseLivePool,
            int livePoolExtraCount)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("An output directory is required.", nameof(outputDirectory));
            if (imageSize < 64 || imageSize > 2048)
                throw new ArgumentOutOfRangeException(nameof(imageSize), "Image size must be in [64, 2048].");
            if (livePoolExtraCount < 1 || livePoolExtraCount > 64)
                throw new ArgumentOutOfRangeException(
                    nameof(livePoolExtraCount),
                    "Live pool extra count must be in [1, 64].");

            OutputDirectory = Path.GetFullPath(outputDirectory);
            ImageSize = imageSize;
            ExerciseLivePool = exerciseLivePool;
            LivePoolExtraCount = livePoolExtraCount;
        }

        public string OutputDirectory { get; }
        public int ImageSize { get; }
        public bool ExerciseLivePool { get; }
        public int LivePoolExtraCount { get; }

        public static BattleRenderingAcceptanceConfig FromRequest(
            BattleRenderingAcceptanceRequest request,
            string projectRoot)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(projectRoot))
                throw new ArgumentException("A project root is required.", nameof(projectRoot));

            string output = string.IsNullOrWhiteSpace(request.outputDirectory)
                ? "Temp/P8-C-Acceptance"
                : request.outputDirectory.Trim();
            if (!Path.IsPathRooted(output))
                output = Path.Combine(projectRoot, output);
            return new BattleRenderingAcceptanceConfig(
                output,
                request.imageSize,
                request.exerciseLivePool,
                request.livePoolExtraCount);
        }
    }

    [Serializable]
    public sealed class BattleRenderingAcceptanceCase
    {
        public string name;
        public bool passed;
        public bool available = true;
        public string evidence;
        public int sourceCount;
        public int resolvedCount;
        public int segmentCount;
        public int chunkCount;
        public int nonTransparentPixels;
        public float meanChannelDifference;
        public int maximumChannelDifference;
    }

    [Serializable]
    public sealed class BattleRenderingLiveEntityEvidence
    {
        public int checkoutIndex;
        public int ownerInstanceId;
        public int stableId;
        public int objectId;
        public int currentDatObjectType;
        public int runtimeSlot;
        public long runtimeGeneration;
        public int entityMountCount;
        public int shadowMountCount;
        public bool mountOwnerAndGenerationMatch;
        public int entityCommandIndex = -1;
        public string resourceKey;
        public string sourceSheetPath;
        public string bindingMode;
        public int atlasSlice;
        public string pixelRect;
        public string normalizedUv;
        public string pivot;
        public bool resourceResolved;
        public int nonTransparentPixels;
        public bool releasedHandleRejected;
        public bool releasedMountsCleared;
        public bool releasedOwnerBindingCleared;
    }

    [Serializable]
    public sealed class BattleRenderingProductionResourceEvidence
    {
        public string category;
        public bool available;
        public bool passed;
        public int objectId;
        public int currentDatObjectType;
        public int runtimeSlot;
        public long runtimeGeneration;
        public int effectivePic;
        public string resourceKey;
        public string sourceSheetPath;
        public string sourceTextureName;
        public string centralTextureName;
        public string bindingMode;
        public int atlasSlice;
        public string pixelRect;
        public string normalizedUv;
        public string pivot;
        public int legacyNonTransparentPixels;
        public int centralNonTransparentPixels;
        public float meanChannelDifference;
        public int maximumChannelDifference;
        public string evidence;
    }

    [Serializable]
    public sealed class BattleRenderingAcceptanceReport
    {
        public string schema = "ntsd-battle-rendering-acceptance-v1";
        public bool passed;
        public int deterministicSeed = 1314149188;
        public int imageSize;
        public bool livePoolRequested;
        public string syntheticFixtureEvidenceScope =
            "synthetic fixture only: generation, isolated expansion, atlas, ordering, chunk, missing-resource, and baseline parity cases";
        public BattleRenderingAcceptanceCase generationReuse;
        public BattleRenderingAcceptanceCase isolatedPoolExpansion;
        public BattleRenderingAcceptanceCase livePoolExpansion;
        public BattleRenderingAcceptanceCase productionCatalogPixelParity;
        public BattleRenderingAcceptanceCase atlasArrayAndOrderedPages;
        public BattleRenderingAcceptanceCase transparentResourceInterleave;
        public BattleRenderingAcceptanceCase categoryOcclusionOrder;
        public BattleRenderingAcceptanceCase chunkBoundaries;
        public BattleRenderingAcceptanceCase missingResourceFailClosed;
        public BattleRenderingAcceptanceCase legacyCentralPixelParity;
        public string legacyPng;
        public string centralPng;
        public string parityDiffPng;
        public string generationReusePng;
        public string isolatedExpansionPng;
        public string arrayPng;
        public string orderedPagesPng;
        public string atlasDiffPng;
        public string interleavePng;
        public string categoryOcclusionPng;
        public string chunk4097Png;
        public string liveProductionPng;
        public string productionCharacterLegacyPng;
        public string productionCharacterCentralPng;
        public string productionCharacterDiffPng;
        public string productionWeaponLegacyPng;
        public string productionWeaponCentralPng;
        public string productionWeaponDiffPng;
        public bool skillInputOpointCovered;
        public string livePoolScope;
        public BattleRenderingLiveEntityEvidence[] livePoolEntities =
            Array.Empty<BattleRenderingLiveEntityEvidence>();
        public BattleRenderingProductionResourceEvidence productionCharacterResource;
        public BattleRenderingProductionResourceEvidence productionWeaponResource;

        public string ToJson(bool pretty = true)
        {
            return JsonUtility.ToJson(this, pretty);
        }
    }

    public static class BattleRenderingAcceptanceHarness
    {
        public const string ReportFileName = "P8-C-report.json";
        public const string LegacyFileName = "P8-C-legacy.png";
        public const string CentralFileName = "P8-C-central.png";
        public const string ParityDiffFileName = "P8-C-parity-diff.png";
        public const string GenerationReuseFileName = "P8-C-generation-reuse.png";
        public const string IsolatedExpansionFileName = "P8-C-isolated-expansion-33.png";
        public const string ArrayFileName = "P8-C-array.png";
        public const string OrderedPagesFileName = "P8-C-ordered-pages.png";
        public const string AtlasDiffFileName = "P8-C-atlas-diff.png";
        public const string InterleaveFileName = "P8-C-aba-interleave.png";
        public const string CategoryOcclusionFileName = "P8-C-category-occlusion.png";
        public const string Chunk4097FileName = "P8-C-chunk-4097.png";
        public const string LiveProductionFileName = "P8-C-live-production.png";
        public const string ProductionCharacterLegacyFileName = "P8-C-production-character-legacy.png";
        public const string ProductionCharacterCentralFileName = "P8-C-production-character-central.png";
        public const string ProductionCharacterDiffFileName = "P8-C-production-character-diff.png";
        public const string ProductionWeaponLegacyFileName = "P8-C-production-weapon-legacy.png";
        public const string ProductionWeaponCentralFileName = "P8-C-production-weapon-central.png";
        public const string ProductionWeaponDiffFileName = "P8-C-production-weapon-diff.png";

        private const int GenerationReuseIterations = 1000;
        private const int IsolatedExpansionPrewarm = 16;
        private const int IsolatedExpansionCount = 33;
        private const int OffscreenLayer = 31;
        private const float PixelParityMeanTolerance = 3f;
        private const int PixelParityMaximumTolerance = 32;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexArrayId = Shader.PropertyToID("_MainTexArray");

        public static BattleRenderingAcceptanceReport Run(BattleRenderingAcceptanceConfig config)
        {
            Directory.CreateDirectory(config.OutputDirectory);
            var resources = new FixtureResources();
            try
            {
                resources.Initialize();
                var report = new BattleRenderingAcceptanceReport
                {
                    imageSize = config.ImageSize,
                    livePoolRequested = config.ExerciseLivePool,
                    legacyPng = LegacyFileName,
                    centralPng = CentralFileName,
                    parityDiffPng = ParityDiffFileName,
                    generationReusePng = GenerationReuseFileName,
                    isolatedExpansionPng = IsolatedExpansionFileName,
                    arrayPng = ArrayFileName,
                    orderedPagesPng = OrderedPagesFileName,
                    atlasDiffPng = AtlasDiffFileName,
                    interleavePng = InterleaveFileName,
                    categoryOcclusionPng = CategoryOcclusionFileName,
                    chunk4097Png = Chunk4097FileName,
                    liveProductionPng = LiveProductionFileName,
                    productionCharacterLegacyPng = ProductionCharacterLegacyFileName,
                    productionCharacterCentralPng = ProductionCharacterCentralFileName,
                    productionCharacterDiffPng = ProductionCharacterDiffFileName,
                    productionWeaponLegacyPng = ProductionWeaponLegacyFileName,
                    productionWeaponCentralPng = ProductionWeaponCentralFileName,
                    productionWeaponDiffPng = ProductionWeaponDiffFileName,
                    skillInputOpointCovered = false,
                    livePoolScope = config.ExerciseLivePool
                        ? "production pool checkout + SimulationWorld registration/publication; skill-input opoint is not asserted"
                        : "not requested",
                };

                report.generationReuse = RunGenerationReuse(
                    resources,
                    config,
                    report.generationReusePng);
                report.isolatedPoolExpansion = RunIsolatedPoolExpansion(
                    resources,
                    config,
                    report.isolatedExpansionPng);
                report.livePoolExpansion = RunLivePoolExpansion(resources, config, report);
                report.atlasArrayAndOrderedPages = RunAtlasArrayAndPages(
                    resources,
                    config,
                    report.arrayPng,
                    report.orderedPagesPng,
                    report.atlasDiffPng);
                report.transparentResourceInterleave = RunTransparentInterleave(
                    resources,
                    config,
                    report.interleavePng);
                report.categoryOcclusionOrder = RunCategoryOrder(
                    resources,
                    config,
                    report.categoryOcclusionPng);
                report.chunkBoundaries = RunChunkBoundaries(
                    resources,
                    config,
                    report.chunk4097Png);
                report.missingResourceFailClosed = RunMissingResourceFailClosed(resources, config.ImageSize);
                report.legacyCentralPixelParity = RunLegacyCentralPixelParity(
                    resources,
                    config,
                    report.legacyPng,
                    report.centralPng,
                    report.parityDiffPng);

                report.passed = report.generationReuse.passed &&
                                report.isolatedPoolExpansion.passed &&
                                (!config.ExerciseLivePool || report.livePoolExpansion.passed) &&
                                (!config.ExerciseLivePool || report.productionCatalogPixelParity.passed) &&
                                report.atlasArrayAndOrderedPages.passed &&
                                report.transparentResourceInterleave.passed &&
                                report.categoryOcclusionOrder.passed &&
                                report.chunkBoundaries.passed &&
                                report.missingResourceFailClosed.passed &&
                                report.legacyCentralPixelParity.passed;

                WriteUtf8(
                    Path.Combine(config.OutputDirectory, ReportFileName),
                    report.ToJson());
                return report;
            }
            finally
            {
                resources.Dispose();
            }
        }

        private static BattleRenderingAcceptanceCase RunGenerationReuse(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            string imageName)
        {
            var result = Case("pool-reuse-1000-generation");
            var table = new RuntimeSlotTable(64, 20, 50);
            var entity = new AcceptanceEntity();
            RuntimeEntityHandle previous = RuntimeEntityHandle.Invalid;
            RuntimeEntityHandle lastClaimed = RuntimeEntityHandle.Invalid;
            bool valid = true;

            var resolver = new FixtureResolver(resources, FixtureResolverMode.VisualData);
            var frame = new BattlePresentationFrame();
            using var backend = new BattleDynamicMeshBackend();
            PixelImage finalImage = default;
            for (int iteration = 0; iteration < GenerationReuseIterations; iteration++)
            {
                entity.SetFixtureStableId(iteration + 1);
                if (!table.TryClaim(7, entity, out RuntimeEntityHandle current))
                {
                    valid = false;
                    break;
                }
                if (previous.IsValid && (current == previous || table.TryResolve(previous, out _)))
                    valid = false;

                int visualDataId = (iteration & 1) == 0 ? 1 : 2;
                BuildFrame(
                    frame,
                    iteration + 1,
                    CreateCommand(
                        BattleRenderCommandType.Entity,
                        current,
                        iteration + 1,
                        visualDataId,
                        visualDataId - 1,
                        iteration,
                        Vector3.zero,
                        new Vector2(16f, 16f),
                        new Vector2(0.5f, 0.5f)));
                if (frame.GetCommand(0).Handle != current ||
                    frame.GetCommand(0).StableId != entity.StableId ||
                    resolver.Resolve(frame.GetCommand(0), out BattleCentralResolvedResource resource) !=
                        BattleCentralResourceStatus.Resolved ||
                    resource.Texture != (visualDataId == 2 ? resources.TextureB : resources.TextureA))
                {
                    valid = false;
                }

                lastClaimed = current;
                if (iteration == GenerationReuseIterations - 1)
                {
                    backend.Build(frame, resolver);
                    finalImage = RenderCentral(backend, config.ImageSize);
                }
                if (!table.Release(current) || table.TryResolve(current, out _))
                    valid = false;
                previous = current;
            }

            RuntimeSlotTable.ReadOnlySlotView released = table.GetReadOnlyView(7);
            Color32 center = SampleWorld(finalImage, Vector2.zero);
            valid &= lastClaimed.IsValid && lastClaimed.Generation == 1999u &&
                     !released.Claimed && released.Generation == 2000u &&
                     backend.Diagnostics.ResolvedCommandCount == 1 &&
                     finalImage.NonTransparentPixelCount > 0 &&
                     center.a > 0 && center.g > center.r + 20 && center.g > center.b + 20;
            WriteImage(config.OutputDirectory, imageName, finalImage);

            result.passed = valid;
            result.sourceCount = GenerationReuseIterations;
            result.resolvedCount = backend.Diagnostics.ResolvedCommandCount;
            result.segmentCount = backend.SegmentCount;
            result.chunkCount = backend.ActiveChunkCount;
            result.nonTransparentPixels = finalImage.NonTransparentPixelCount;
            result.evidence = string.Format(
                CultureInfo.InvariantCulture,
                "slot=7; iterations={0}; finalClaimGeneration={1}; releasedGeneration={2}; " +
                "oldHandlesRejected={3}; finalResource=B; centerRGBA={4},{5},{6},{7}",
                GenerationReuseIterations,
                lastClaimed.Generation,
                released.Generation,
                valid,
                center.r,
                center.g,
                center.b,
                center.a);
            return result;
        }

        private static BattleRenderingAcceptanceCase RunIsolatedPoolExpansion(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            string imageName)
        {
            var result = Case("isolated-presentation-expansion-beyond-prewarm");
            var roots = new List<GameObject>(IsolatedExpansionCount);
            var handles = new List<RuntimeEntityHandle>(IsolatedExpansionCount);
            var uniqueHandles = new HashSet<RuntimeEntityHandle>();
            var owners = new HashSet<int>();
            bool valid = true;
            try
            {
                for (int index = 0; index < IsolatedExpansionCount; index++)
                {
                    RuntimeEntityHandle handle = new RuntimeEntityHandle(1000 + index, (uint)(index + 1));
                    GameObject root = new GameObject("P8C_PooledRoot_" + index)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    root.SetActive(false);
                    roots.Add(root);

                    GameObject model = new GameObject("EntityModel");
                    model.transform.SetParent(root.transform, false);
                    LF2ObjectRenderer owner = model.AddComponent<LF2ObjectRenderer>();
                    BattleCentralPresentationMount entityMount =
                        model.AddComponent<BattleCentralPresentationMount>();
                    ConfigureMount(
                        entityMount,
                        BattleCentralPresentationMountRole.EntityModel,
                        BattleCentralPresentationMountPurpose.EntitySprite,
                        owner,
                        handle);

                    GameObject shadow = new GameObject("Shadow");
                    shadow.transform.SetParent(root.transform, false);
                    BattleCentralPresentationMount shadowMount =
                        shadow.AddComponent<BattleCentralPresentationMount>();
                    ConfigureMount(
                        shadowMount,
                        BattleCentralPresentationMountRole.Shadow,
                        BattleCentralPresentationMountPurpose.CommonShadow,
                        owner,
                        handle);

                    valid &= owners.Add(owner.GetInstanceID()) && uniqueHandles.Add(handle) &&
                             entityMount.RuntimeHandle == handle && shadowMount.RuntimeHandle == handle &&
                             entityMount.OwnerRenderer == owner && shadowMount.OwnerRenderer == owner;
                    handles.Add(handle);
                }

                var frame = new BattlePresentationFrame();
                var commands = new BattleRenderCommand[IsolatedExpansionCount];
                int cursor = 0;
                for (int index = 0; index < handles.Count; index++)
                {
                    RuntimeEntityHandle handle = handles[index];
                    int column = index % 11;
                    int row = index / 11;
                    commands[cursor] = CreateCommand(
                        BattleRenderCommandType.Entity,
                        handle,
                        cursor + 1,
                        (cursor & 1) == 0 ? 1 : 2,
                        cursor & 1,
                        cursor,
                        new Vector3(-0.8f + column * 0.16f, -0.16f + row * 0.16f, 0f),
                        new Vector2(8f, 8f),
                        new Vector2(0.5f, 0.5f));
                    cursor++;
                }
                BuildFrame(frame, 2001, commands);
                using var backend = new BattleDynamicMeshBackend();
                backend.Build(frame, new FixtureResolver(resources, FixtureResolverMode.VisualData));
                PixelImage expansionImage = RenderCentral(backend, config.ImageSize);
                int visibleSamples = 0;
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    if (SampleWorld(expansionImage, frame.GetCommand(index).Position).a > 0)
                        visibleSamples++;
                }
                WriteImage(config.OutputDirectory, imageName, expansionImage);
                valid &= backend.Diagnostics.ResolvedCommandCount == IsolatedExpansionCount &&
                         backend.ActiveChunkCount == 1 && uniqueHandles.Count == IsolatedExpansionCount &&
                         owners.Count == IsolatedExpansionCount && visibleSamples == IsolatedExpansionCount &&
                         expansionImage.NonTransparentPixelCount > 0;
                result.resolvedCount = backend.Diagnostics.ResolvedCommandCount;
                result.segmentCount = backend.SegmentCount;
                result.chunkCount = backend.ActiveChunkCount;
                result.nonTransparentPixels = expansionImage.NonTransparentPixelCount;
                result.evidence = string.Format(
                    CultureInfo.InvariantCulture,
                    "prewarmBaseline={0}; expandedCount={1}; uniqueOwners={2}; uniqueHandles={3}; " +
                    "visibleCenterSamples={4}; rendererlessMountPairs=true",
                    IsolatedExpansionPrewarm,
                    IsolatedExpansionCount,
                    owners.Count,
                    uniqueHandles.Count,
                    visibleSamples);
            }
            finally
            {
                for (int index = 0; index < roots.Count; index++)
                    DestroyImmediateSafe(roots[index]);
            }

            result.passed = valid;
            result.sourceCount = IsolatedExpansionCount;
            return result;
        }

        private static BattleRenderingAcceptanceCase RunLivePoolExpansion(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            BattleRenderingAcceptanceReport report)
        {
            var result = Case("live-production-pool-world-publication-expansion");
            report.productionCatalogPixelParity = Case("production-character-weapon-source-central-pixel-parity");
            if (!config.ExerciseLivePool)
            {
                result.available = false;
                result.passed = true;
                result.evidence = "not requested; use the request flag in Play Mode for production LF2ObjectPool evidence";
                report.productionCatalogPixelParity.available = false;
                report.productionCatalogPixelParity.passed = true;
                report.productionCatalogPixelParity.evidence =
                    "not requested; production catalog evidence requires a loaded Play Mode battle world";
                return result;
            }
            if (!Application.isPlaying)
            {
                result.available = false;
                result.passed = false;
                result.evidence = "requested but unavailable outside Play Mode";
                FailProductionParity(report, result.evidence);
                return result;
            }

            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            LF2ReferencePool referencePool = LF2ReferencePool.TryGetInstance();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            if (pool == null || referencePool == null || world == null || manager == null || dataManager == null)
            {
                result.available = false;
                result.passed = false;
                result.evidence = string.Format(
                    CultureInfo.InvariantCulture,
                    "required production services missing: objectPool={0}; referencePool={1}; world={2}; animatorManager={3}; dataManager={4}",
                    pool != null,
                    referencePool != null,
                    world != null,
                    manager != null,
                    dataManager != null);
                FailProductionParity(report, result.evidence);
                return result;
            }

            if (!TryFindProductionSample(manager, dataManager, true, out ProductionSample characterSample) ||
                !TryFindProductionSample(manager, dataManager, false, out ProductionSample weaponSample))
            {
                result.available = false;
                result.passed = false;
                result.evidence =
                    "production catalog did not contain both a loaded character and a loaded weapon frame with a valid legacy sprite and central binding";
                FailProductionParity(report, result.evidence);
                return result;
            }

            Material featureMaterial = BattleCentralRenderSystem.RegisteredFeatureMaterialForAcceptance;
            Material featureArrayMaterial = BattleCentralRenderSystem.RegisteredFeatureArrayMaterialForAcceptance;
            if (!BattleSpriteMaterialContract.IsDeclaredCentralMaterial(featureMaterial, false) ||
                (weaponSample.Entry.CentralBinding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray &&
                 !BattleSpriteMaterialContract.IsDeclaredCentralMaterial(featureArrayMaterial, true)))
            {
                result.available = false;
                result.passed = false;
                result.evidence =
                    "production BattleRenderFeature materials are unavailable or violate the central shader contract";
                FailProductionParity(report, result.evidence);
                return result;
            }

            int availableBefore = pool.AvailableObjectCountForAcceptance;
            int activeBefore = pool.ActiveObjectCountForAcceptance;
            int claimedBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            int liveEntityCount = Math.Max(config.LivePoolExtraCount, 2);
            int acquireCount = checked(availableBefore + liveEntityCount);
            if (acquireCount > 512)
            {
                result.available = false;
                result.passed = false;
                result.evidence = "refused because forcing expansion would acquire more than 512 live pool objects";
                FailProductionParity(report, result.evidence);
                return result;
            }

            var acquired = new List<LF2ObjectRenderer>(acquireCount);
            var liveLogic = new List<ILF2Object>(liveEntityCount);
            var liveEntities = new List<LF2Entity>(liveEntityCount);
            var liveHandles = new List<RuntimeEntityHandle>(liveEntityCount);
            var liveMounts = new List<BattleCentralPresentationMount[]>(liveEntityCount);
            var liveEvidence = new List<BattleRenderingLiveEntityEvidence>(liveEntityCount);
            var owners = new HashSet<int>();
            var uniqueHandles = new HashSet<RuntimeEntityHandle>();
            bool valid = true;
            int availableWhileExhausted = -1;
            BattlePresentationBackendMode previousMode = world.BattlePresentation.Mode;
            try
            {
                for (int index = 0; index < acquireCount; index++)
                {
                    GameObject root = pool.Get(out LF2ObjectRenderer renderer);
                    if (root == null || renderer == null || !owners.Add(renderer.GetInstanceID()))
                    {
                        valid = false;
                        break;
                    }
                    acquired.Add(renderer);
                    if (index < availableBefore)
                        continue;

                    bool isCharacter = index == availableBefore;
                    ProductionSample sample = isCharacter ? characterSample : weaponSample;
                    ILF2Object logicObject = referencePool.Get(
                        (LF2ObjectType)sample.ObjectType,
                        sample.ObjectId);
                    if (logicObject is not LF2Entity entity)
                    {
                        valid = false;
                        break;
                    }
                    liveLogic.Add(logicObject);
                    var task = new OPointCreateTask
                    {
                        opoint = new ObjectPoint
                        {
                            kind = 1,
                            oid = sample.ObjectId,
                            action = sample.Frame.frameId,
                            facing = 0,
                        },
                        useDirectRuntimePosition = true,
                        directX = 40 + (index - availableBefore) * 24,
                        directY = 0,
                        directZ = 120 + (index - availableBefore) * 3,
                        useDirectVelocity = true,
                        preserveActionZero = true,
                    };
                    renderer.SetLogicObject(logicObject, task);
                    renderer.ForceRefreshPresentation();
                    if (entity.Match != world || entity.Runtime?.SlotIndex < 0 ||
                        !world.TryGetCurrentRuntimeHandleForDiagnostics(
                            entity.Runtime.SlotIndex,
                            entity,
                            out RuntimeEntityHandle handle) ||
                        !uniqueHandles.Add(handle))
                    {
                        valid = false;
                        break;
                    }

                    BattleCentralPresentationMount[] mounts =
                        root.GetComponentsInChildren<BattleCentralPresentationMount>(true);
                    int entityMounts = 0;
                    int shadowMounts = 0;
                    bool mountHandlesMatch = true;
                    for (int mountIndex = 0; mountIndex < mounts.Length; mountIndex++)
                    {
                        BattleCentralPresentationMount mount = mounts[mountIndex];
                        if (mount.OwnerRenderer != renderer)
                        {
                            mountHandlesMatch = false;
                            continue;
                        }
                        mountHandlesMatch &= mount.RuntimeHandle == handle;
                        if (mount.Purpose == BattleCentralPresentationMountPurpose.EntitySprite)
                            entityMounts++;
                        else if (mount.Purpose == BattleCentralPresentationMountPurpose.CommonShadow)
                            shadowMounts++;
                    }
                    valid &= entityMounts == 1 && shadowMounts == 1 && mountHandlesMatch;
                    liveEntities.Add(entity);
                    liveHandles.Add(handle);
                    liveMounts.Add(mounts);
                    liveEvidence.Add(new BattleRenderingLiveEntityEvidence
                    {
                        checkoutIndex = index,
                        ownerInstanceId = renderer.GetInstanceID(),
                        stableId = entity.StableId,
                        objectId = entity.ObjectId,
                        currentDatObjectType = entity.GetCurrentDataObjectTypeForSimulation(),
                        runtimeSlot = handle.Slot,
                        runtimeGeneration = handle.Generation,
                        entityMountCount = entityMounts,
                        shadowMountCount = shadowMounts,
                        mountOwnerAndGenerationMatch = mountHandlesMatch,
                    });
                }
                availableWhileExhausted = pool.AvailableObjectCountForAcceptance;
                valid &= acquired.Count == acquireCount &&
                         availableWhileExhausted == 0 &&
                         liveEntities.Count == liveEntityCount &&
                         liveHandles.Count == liveEntityCount;

                world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                world.BattlePresentation.BeginFrame(world, world.CurrentTickIndex);
                BattlePresentationFrame publishedFrame = world.BattlePresentation.PublishedFrame;
                if (publishedFrame == null || publishedFrame.CommandCount == 0 ||
                    !ReferenceEquals(publishedFrame.BoundCatalogForAcceptance, manager.SpriteCatalog))
                {
                    valid = false;
                }
                else
                {
                    var resolver = new BattleCatalogCentralResourceResolver();
                    resolver.Configure(
                        publishedFrame.BoundCatalogForAcceptance,
                        publishedFrame.CommonVisualCatalog,
                        featureMaterial,
                        featureArrayMaterial);
                    var liveCommands = new BattleRenderCommand[liveHandles.Count];
                    for (int liveIndex = 0; liveIndex < liveHandles.Count; liveIndex++)
                    {
                        RuntimeEntityHandle handle = liveHandles[liveIndex];
                        BattleRenderingLiveEntityEvidence evidence = liveEvidence[liveIndex];
                        if (!TryFindEntityCommand(
                                publishedFrame,
                                handle,
                                out int commandIndex,
                                out BattleRenderCommand command) ||
                            !TryFindSnapshot(
                                publishedFrame,
                                handle,
                                out BattlePresentationEntitySnapshot snapshot) ||
                            resolver.Resolve(command, out BattleCentralResolvedResource resource) !=
                                BattleCentralResourceStatus.Resolved ||
                            !publishedFrame.BoundCatalogForAcceptance.TryGet(
                                command.SpriteDescriptor.LogicalResourceKey.EntitySpriteKey,
                                out BattleSpriteEntry entry))
                        {
                            valid = false;
                            continue;
                        }

                        BuildFrameForSingleCommand(publishedFrame.TickIndex, command, out BattlePresentationFrame singleFrame);
                        using var backend = new BattleDynamicMeshBackend();
                        backend.Build(singleFrame, resolver);
                        float halfExtent = ResolveCommandHalfExtent(command);
                        PixelImage image = RenderCentral(
                            backend,
                            config.ImageSize,
                            command.Position,
                            halfExtent);
                        if (liveIndex == 0)
                            WriteImage(config.OutputDirectory, report.liveProductionPng, image);

                        evidence.entityCommandIndex = commandIndex;
                        evidence.currentDatObjectType = snapshot.CurrentDatObjType;
                        evidence.resourceKey = command.SpriteDescriptor.LogicalResourceKey.ToString();
                        evidence.sourceSheetPath = entry.SourceSheetPath;
                        evidence.bindingMode = resource.BindingMode.ToString();
                        evidence.atlasSlice = resource.AtlasSlice;
                        evidence.pixelRect = FormatRect(entry.PixelRect);
                        evidence.normalizedUv = FormatRect(resource.NormalizedUv);
                        evidence.pivot = FormatVector(resource.Pivot);
                        evidence.resourceResolved = true;
                        evidence.nonTransparentPixels = image.NonTransparentPixelCount;
                        liveCommands[liveIndex] = command;
                        valid &= snapshot.CurrentDatObjType ==
                                 (liveIndex == 0 ? characterSample.ObjectType : weaponSample.ObjectType) &&
                                 command.Handle == handle &&
                                 command.RuntimeSlot == handle.Slot &&
                                 backend.Diagnostics.ResolvedCommandCount == 1 &&
                                 image.NonTransparentPixelCount > 0;
                    }

                    report.productionCatalogPixelParity = RunProductionCatalogPixelParity(
                        resources,
                        config,
                        report,
                        publishedFrame,
                        resolver,
                        characterSample,
                        weaponSample,
                        liveHandles.Count > 0 ? liveHandles[0] : RuntimeEntityHandle.Invalid,
                        liveHandles.Count > 1 ? liveHandles[1] : RuntimeEntityHandle.Invalid);
                    valid &= report.productionCatalogPixelParity.passed;
                }
            }
            finally
            {
                world.SetBattlePresentationBackend(previousMode);
                for (int index = acquired.Count - 1; index >= 0; index--)
                    pool.Release(acquired[index]);
                for (int index = liveLogic.Count - 1; index >= 0; index--)
                    referencePool.Release(liveLogic[index]);
            }

            int availableAfter = pool.AvailableObjectCountForAcceptance;
            int activeAfter = pool.ActiveObjectCountForAcceptance;
            int claimedAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
            for (int index = 0; index < liveEvidence.Count; index++)
            {
                BattleRenderingLiveEntityEvidence evidence = liveEvidence[index];
                RuntimeEntityHandle handle = liveHandles[index];
                BattleCentralPresentationMount[] mounts = liveMounts[index];
                evidence.releasedHandleRejected = !world.TryResolveRuntimeHandleForDiagnostics(handle, out _);
                evidence.releasedMountsCleared = true;
                for (int mountIndex = 0; mountIndex < mounts.Length; mountIndex++)
                    evidence.releasedMountsCleared &= !mounts[mountIndex].RuntimeHandle.IsValid;
                evidence.releasedOwnerBindingCleared =
                    !BattleCentralPresentationMountDiagnostics.HasOwnerRuntimeBindingForAcceptance(
                        evidence.ownerInstanceId);
                valid &= evidence.releasedHandleRejected &&
                         evidence.releasedMountsCleared &&
                         evidence.releasedOwnerBindingCleared &&
                         evidence.resourceResolved &&
                         evidence.nonTransparentPixels > 0;
            }

            valid &= availableAfter == availableBefore + liveEntityCount &&
                     activeAfter == activeBefore &&
                     claimedAfter == claimedBefore;
            report.livePoolEntities = liveEvidence.ToArray();
            result.sourceCount = liveEvidence.Count;
            result.resolvedCount = CountResolvedLiveEvidence(liveEvidence);
            result.nonTransparentPixels = SumLivePixels(liveEvidence);
            result.evidence = string.Format(
                CultureInfo.InvariantCulture,
                "scope=production pool checkout + world publication; skill-input opoint is explicitly not asserted; " +
                "availableBefore={0}; totalCheckout={1}; expandedAndPublished={2}; requestedExtra={3}; availableAtPeak={4}; " +
                "availableAfter={5}; activeBeforeAfter={6}/{7}; claimedBeforeAfter={8}/{9}; " +
                "uniqueCheckoutOwners={10}; uniqueRuntimeHandles={11}; allReleaseResidueChecks={12}",
                availableBefore,
                acquired.Count,
                liveEvidence.Count,
                config.LivePoolExtraCount,
                availableWhileExhausted,
                availableAfter,
                activeBefore,
                activeAfter,
                claimedBefore,
                claimedAfter,
                owners.Count,
                uniqueHandles.Count,
                valid);
            result.passed = valid;
            return result;
        }

        private static void FailProductionParity(
            BattleRenderingAcceptanceReport report,
            string evidence)
        {
            report.productionCatalogPixelParity ??=
                Case("production-character-weapon-source-central-pixel-parity");
            report.productionCatalogPixelParity.available = false;
            report.productionCatalogPixelParity.passed = false;
            report.productionCatalogPixelParity.evidence = evidence ?? string.Empty;
        }

        private static bool TryFindProductionSample(
            CharacterAnimtorManager manager,
            GameDataManager dataManager,
            bool character,
            out ProductionSample sample)
        {
            sample = default;
            List<ObjectDefinition> definitions = dataManager.GetAllObjects();
            BattleSpriteCatalog catalog = manager.SpriteCatalog;
            if (definitions == null || catalog == null || catalog.Count == 0)
                return false;

            for (int definitionIndex = 0; definitionIndex < definitions.Count; definitionIndex++)
            {
                ObjectDefinition definition = definitions[definitionIndex];
                bool matchesType = character
                    ? definition.type == (int)LF2ObjectType.Character
                    : definition.type == (int)LF2ObjectType.LightWeapon ||
                      definition.type == (int)LF2ObjectType.HeavyWeapon ||
                      definition.type == (int)LF2ObjectType.ThrowWeapon ||
                      definition.type == (int)LF2ObjectType.Drink;
                if (!matchesType)
                    continue;

                LF2CharacterData data = manager.GetCharacterData(definition.id);
                if (data?.frames == null)
                    continue;
                for (int frameIndex = 0; frameIndex < data.frames.Count; frameIndex++)
                {
                    LF2FrameData frame = data.frames[frameIndex];
                    if (frame == null || frame.pic < 0 || frame.pic == 999 ||
                        !catalog.TryGet(definition.id, frame.pic, out BattleSpriteEntry entry) ||
                        entry?.LegacySprite == null || entry.SharedTexture == null ||
                        !entry.CentralBinding.IsValid)
                    {
                        continue;
                    }

                    sample = new ProductionSample(
                        definition.id,
                        definition.type,
                        frame,
                        entry);
                    return true;
                }
            }

            return false;
        }

        private static BattleRenderingAcceptanceCase RunProductionCatalogPixelParity(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            BattleRenderingAcceptanceReport report,
            BattlePresentationFrame publishedFrame,
            BattleCatalogCentralResourceResolver resolver,
            ProductionSample expectedCharacter,
            ProductionSample expectedWeapon,
            RuntimeEntityHandle characterHandle,
            RuntimeEntityHandle weaponHandle)
        {
            var result = Case("production-character-weapon-source-central-pixel-parity");
            bool characterFound = TryFindTypedProductionCommand(
                publishedFrame,
                publishedFrame.BoundCatalogForAcceptance,
                resolver,
                expectedCharacter.ObjectType,
                characterHandle,
                out BattlePresentationEntitySnapshot characterSnapshot,
                out BattleRenderCommand characterCommand,
                out BattleSpriteEntry characterEntry,
                out BattleCentralResolvedResource characterResource);
            bool weaponFound = TryFindTypedProductionCommand(
                publishedFrame,
                publishedFrame.BoundCatalogForAcceptance,
                resolver,
                expectedWeapon.ObjectType,
                weaponHandle,
                out BattlePresentationEntitySnapshot weaponSnapshot,
                out BattleRenderCommand weaponCommand,
                out BattleSpriteEntry weaponEntry,
                out BattleCentralResolvedResource weaponResource);
            if (!characterFound || !weaponFound)
            {
                result.available = false;
                result.passed = false;
                result.evidence = string.Format(
                    CultureInfo.InvariantCulture,
                    "production immutable frame missing required typed entity command: character={0}; weapon={1}; " +
                    "catalogCharacterCandidate={2}/{3}; catalogWeaponCandidate={4}/{5}",
                    characterFound,
                    weaponFound,
                    expectedCharacter.ObjectId,
                    expectedCharacter.ObjectType,
                    expectedWeapon.ObjectId,
                    expectedWeapon.ObjectType);
                return result;
            }

            report.productionCharacterResource = BuildProductionResourceEvidence(
                "character",
                config,
                characterSnapshot,
                characterCommand,
                characterEntry,
                characterResource,
                resolver,
                resources.LegacyMaterial,
                report.productionCharacterLegacyPng,
                report.productionCharacterCentralPng,
                report.productionCharacterDiffPng);
            report.productionWeaponResource = BuildProductionResourceEvidence(
                "weapon",
                config,
                weaponSnapshot,
                weaponCommand,
                weaponEntry,
                weaponResource,
                resolver,
                resources.LegacyMaterial,
                report.productionWeaponLegacyPng,
                report.productionWeaponCentralPng,
                report.productionWeaponDiffPng);

            result.sourceCount = 2;
            result.resolvedCount =
                (report.productionCharacterResource.available ? 1 : 0) +
                (report.productionWeaponResource.available ? 1 : 0);
            result.nonTransparentPixels =
                report.productionCharacterResource.centralNonTransparentPixels +
                report.productionWeaponResource.centralNonTransparentPixels;
            result.meanChannelDifference = Mathf.Max(
                report.productionCharacterResource.meanChannelDifference,
                report.productionWeaponResource.meanChannelDifference);
            result.maximumChannelDifference = Math.Max(
                report.productionCharacterResource.maximumChannelDifference,
                report.productionWeaponResource.maximumChannelDifference);
            result.passed = report.productionCharacterResource.passed &&
                            report.productionWeaponResource.passed;
            result.evidence = string.Format(
                CultureInfo.InvariantCulture,
                "immutableProductionFrameTick={0}; characterKey={1}; characterType={2}; " +
                "weaponKey={3}; weaponType={4}; source-vs-central pixel parity={5}",
                publishedFrame.TickIndex,
                report.productionCharacterResource.resourceKey,
                report.productionCharacterResource.currentDatObjectType,
                report.productionWeaponResource.resourceKey,
                report.productionWeaponResource.currentDatObjectType,
                result.passed);
            return result;
        }

        private static bool TryFindTypedProductionCommand(
            BattlePresentationFrame frame,
            BattleSpriteCatalog catalog,
            BattleCatalogCentralResourceResolver resolver,
            int requiredDatType,
            RuntimeEntityHandle requiredHandle,
            out BattlePresentationEntitySnapshot snapshot,
            out BattleRenderCommand command,
            out BattleSpriteEntry entry,
            out BattleCentralResolvedResource resource)
        {
            snapshot = default;
            command = default;
            entry = null;
            resource = default;
            for (int entityIndex = 0; entityIndex < frame.EntityCount; entityIndex++)
            {
                BattlePresentationEntitySnapshot candidate = frame.GetEntity(entityIndex);
                if (candidate.CurrentDatObjType != requiredDatType ||
                    (requiredHandle.IsValid && candidate.Handle != requiredHandle) ||
                    !TryFindEntityCommand(frame, candidate.Handle, out _, out BattleRenderCommand candidateCommand) ||
                    !candidateCommand.SpriteDescriptor.HasLogicalResourceKey ||
                    !candidateCommand.SpriteDescriptor.LogicalResourceKey.IsEntitySprite ||
                    !catalog.TryGet(
                        candidateCommand.SpriteDescriptor.LogicalResourceKey.EntitySpriteKey,
                        out BattleSpriteEntry candidateEntry) ||
                    resolver.Resolve(candidateCommand, out BattleCentralResolvedResource candidateResource) !=
                        BattleCentralResourceStatus.Resolved)
                {
                    continue;
                }

                snapshot = candidate;
                command = candidateCommand;
                entry = candidateEntry;
                resource = candidateResource;
                return true;
            }
            return false;
        }

        private static BattleRenderingProductionResourceEvidence BuildProductionResourceEvidence(
            string category,
            BattleRenderingAcceptanceConfig config,
            in BattlePresentationEntitySnapshot snapshot,
            in BattleRenderCommand command,
            BattleSpriteEntry entry,
            in BattleCentralResolvedResource resource,
            IBattleCentralResourceResolver resolver,
            Material legacyMaterial,
            string legacyPng,
            string centralPng,
            string diffPng)
        {
            BuildFrameForSingleCommand(config.ImageSize, command, out BattlePresentationFrame singleFrame);
            using var backend = new BattleDynamicMeshBackend();
            backend.Build(singleFrame, resolver);
            float halfExtent = ResolveCommandHalfExtent(command);
            PixelImage central = RenderCentral(
                backend,
                config.ImageSize,
                command.Position,
                halfExtent);
            PixelImage legacy = RenderProductionLegacy(
                command,
                entry,
                legacyMaterial,
                config.ImageSize,
                halfExtent);
            PixelDifference difference = ComparePixels(legacy, central);
            WriteImage(config.OutputDirectory, legacyPng, legacy);
            WriteImage(config.OutputDirectory, centralPng, central);
            WriteImage(config.OutputDirectory, diffPng, difference.DifferenceImage);

            bool passed = entry != null && entry.LegacySprite != null &&
                          resource.Texture != null && resource.Material != null &&
                          backend.Diagnostics.ResolvedCommandCount == 1 &&
                          legacy.NonTransparentPixelCount > 0 &&
                          central.NonTransparentPixelCount > 0 &&
                          difference.MeanChannelDifference <= PixelParityMeanTolerance &&
                          difference.MaximumChannelDifference <= PixelParityMaximumTolerance;
            return new BattleRenderingProductionResourceEvidence
            {
                category = category,
                available = true,
                passed = passed,
                objectId = snapshot.CurrentDatObjectId,
                currentDatObjectType = snapshot.CurrentDatObjType,
                runtimeSlot = snapshot.Handle.Slot,
                runtimeGeneration = snapshot.Handle.Generation,
                effectivePic = snapshot.EffectivePic,
                resourceKey = command.SpriteDescriptor.LogicalResourceKey.ToString(),
                sourceSheetPath = entry.SourceSheetPath,
                sourceTextureName = entry.SharedTexture != null ? entry.SharedTexture.name : string.Empty,
                centralTextureName = resource.Texture != null ? resource.Texture.name : string.Empty,
                bindingMode = resource.BindingMode.ToString(),
                atlasSlice = resource.AtlasSlice,
                pixelRect = FormatRect(entry.PixelRect),
                normalizedUv = FormatRect(resource.NormalizedUv),
                pivot = FormatVector(resource.Pivot),
                legacyNonTransparentPixels = legacy.NonTransparentPixelCount,
                centralNonTransparentPixels = central.NonTransparentPixelCount,
                meanChannelDifference = difference.MeanChannelDifference,
                maximumChannelDifference = difference.MaximumChannelDifference,
                evidence = string.Format(
                    CultureInfo.InvariantCulture,
                    "typedSnapshot={0}; sourceSprite={1}; centralBinding={2}/slice{3}; " +
                    "legacyPixels={4}; centralPixels={5}; mean/maxDiff={6:F4}/{7}",
                    snapshot.CurrentDatObjType,
                    entry.LegacySprite != null,
                    resource.BindingMode,
                    resource.AtlasSlice,
                    legacy.NonTransparentPixelCount,
                    central.NonTransparentPixelCount,
                    difference.MeanChannelDifference,
                    difference.MaximumChannelDifference),
            };
        }

        private static bool TryFindEntityCommand(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out int commandIndex,
            out BattleRenderCommand command)
        {
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand candidate = frame.GetCommand(index);
                if (candidate.Type == BattleRenderCommandType.Entity && candidate.Handle == handle)
                {
                    commandIndex = index;
                    command = candidate;
                    return true;
                }
            }
            commandIndex = -1;
            command = default;
            return false;
        }

        private static bool TryFindSnapshot(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattlePresentationEntitySnapshot snapshot)
        {
            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot candidate = frame.GetEntity(index);
                if (candidate.Handle == handle)
                {
                    snapshot = candidate;
                    return true;
                }
            }
            snapshot = default;
            return false;
        }

        private static void BuildFrameForSingleCommand(
            int tickIndex,
            in BattleRenderCommand command,
            out BattlePresentationFrame frame)
        {
            frame = new BattlePresentationFrame();
            BuildFrame(frame, tickIndex, command);
        }

        private static float ResolveCommandHalfExtent(in BattleRenderCommand command)
        {
            float width = command.Size.x * NTSDRenderSpace.BattleVisualScale /
                          SimulationConstants.PIXELS_PER_UNIT;
            float height = command.Size.y * NTSDRenderSpace.BattleVisualScale /
                           SimulationConstants.PIXELS_PER_UNIT;
            return Mathf.Max(0.25f, Mathf.Max(width, height) * 0.75f);
        }

        private static string FormatRect(Rect value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:F6},{1:F6},{2:F6},{3:F6}",
                value.x,
                value.y,
                value.width,
                value.height);
        }

        private static string FormatVector(Vector2 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:F6},{1:F6}",
                value.x,
                value.y);
        }

        private static int CountResolvedLiveEvidence(List<BattleRenderingLiveEntityEvidence> evidence)
        {
            int count = 0;
            for (int index = 0; index < evidence.Count; index++)
            {
                if (evidence[index].resourceResolved)
                    count++;
            }
            return count;
        }

        private static int SumLivePixels(List<BattleRenderingLiveEntityEvidence> evidence)
        {
            int count = 0;
            for (int index = 0; index < evidence.Count; index++)
                count = checked(count + evidence[index].nonTransparentPixels);
            return count;
        }

        private static BattleRenderingAcceptanceCase RunAtlasArrayAndPages(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            string arrayName,
            string pagesName,
            string diffName)
        {
            var result = Case("texture-array-uv-and-ordered-pages-fallback");
            var capabilities = new BattleRenderingDeviceCapabilities(
                "P8-C Fixture GPU",
                "P8-C Fixture Device",
                "P8-C Fixture API",
                true,
                4096,
                8,
                true,
                true,
                long.MaxValue);
            BattleAtlasPolicyDecision arrayDecision = BattleRenderingPolicyResolver.ResolveAtlas(
                capabilities,
                2,
                Array.Empty<string>(),
                nameof(BattleAtlasPolicyMode.TextureArray));
            var noArrayCapabilities = new BattleRenderingDeviceCapabilities(
                "P8-C No Array GPU",
                "P8-C Fixture Device",
                "P8-C Fixture API",
                false,
                4096,
                8,
                true,
                true,
                long.MaxValue);
            BattleAtlasPolicyDecision fallbackDecision = BattleRenderingPolicyResolver.ResolveAtlas(
                noArrayCapabilities,
                2,
                Array.Empty<string>(),
                nameof(BattleAtlasPolicyMode.TextureArray));

            var frame = new BattlePresentationFrame();
            BuildFrame(
                frame,
                3001,
                CreateCommand(BattleRenderCommandType.Entity, new RuntimeEntityHandle(1, 1), 1, 1, 0, 0,
                    new Vector3(-0.28f, 0f, 0f), new Vector2(40f, 40f), new Vector2(0.5f, 0.5f),
                    new Rect(0f, 0f, 0.75f, 1f)),
                CreateCommand(BattleRenderCommandType.Entity, new RuntimeEntityHandle(2, 1), 2, 1, 1, 1,
                    new Vector3(0.28f, 0f, 0f), new Vector2(40f, 40f), new Vector2(0.5f, 0.5f),
                    new Rect(0.25f, 0f, 0.75f, 1f)));

            bool environmentSupportsArray = SystemInfo.supports2DArrayTextures;
            if (!environmentSupportsArray || resources.ArrayTexture == null || resources.ArrayMaterial == null)
            {
                result.available = false;
                result.passed = false;
                result.evidence = "Texture2DArray offscreen sampling is unavailable in the current Editor graphics device";
                return result;
            }

            using var arrayBackend = new BattleDynamicMeshBackend();
            using var pagesBackend = new BattleDynamicMeshBackend();
            arrayBackend.Build(frame, new AtlasArrayResolver(resources));
            pagesBackend.Build(frame, new AtlasPagesResolver(resources));
            PixelImage arrayImage = RenderCentral(arrayBackend, config.ImageSize);
            PixelImage pagesImage = RenderCentral(pagesBackend, config.ImageSize);
            PixelDifference difference = ComparePixels(arrayImage, pagesImage);
            WriteImage(config.OutputDirectory, arrayName, arrayImage);
            WriteImage(config.OutputDirectory, pagesName, pagesImage);
            WriteImage(config.OutputDirectory, diffName, difference.DifferenceImage);

            bool valid = arrayDecision.EffectiveMode == BattleAtlasPolicyMode.TextureArray &&
                         fallbackDecision.EffectiveMode == BattleAtlasPolicyMode.OrderedPages &&
                         !string.IsNullOrEmpty(fallbackDecision.FallbackOrRefusalReason) &&
                         arrayBackend.SegmentCount == 1 &&
                         arrayBackend.GetSegment(0).BindingMode ==
                             BattleSpriteCentralBindingMode.AtlasTextureArray &&
                         pagesBackend.SegmentCount == 2 &&
                         pagesBackend.GetSegment(0).BindingMode ==
                             BattleSpriteCentralBindingMode.AtlasPageTexture2D &&
                         arrayImage.NonTransparentPixelCount > 0 &&
                         pagesImage.NonTransparentPixelCount > 0 &&
                         difference.MeanChannelDifference <= PixelParityMeanTolerance &&
                         difference.MaximumChannelDifference <= PixelParityMaximumTolerance;
            result.passed = valid;
            result.sourceCount = frame.CommandCount;
            result.resolvedCount = arrayBackend.Diagnostics.ResolvedCommandCount;
            result.segmentCount = pagesBackend.SegmentCount;
            result.chunkCount = arrayBackend.ActiveChunkCount;
            result.nonTransparentPixels = Math.Min(
                arrayImage.NonTransparentPixelCount,
                pagesImage.NonTransparentPixelCount);
            result.meanChannelDifference = difference.MeanChannelDifference;
            result.maximumChannelDifference = difference.MaximumChannelDifference;
            result.evidence = "arraySlice=0,1; asymmetricUV=0..0.75,0.25..1; " +
                              "arraySegments=1; orderedPageSegments=2; fallbackReason=" +
                              fallbackDecision.FallbackOrRefusalReason;
            return result;
        }

        private static BattleRenderingAcceptanceCase RunTransparentInterleave(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            string imageName)
        {
            var result = Case("transparent-a-b-a-resource-interleave");
            var frame = new BattlePresentationFrame();
            BuildFrame(
                frame,
                4001,
                CreateCommand(BattleRenderCommandType.Entity, new RuntimeEntityHandle(10, 1), 10, 1, 0, 0,
                    new Vector3(-0.06f, 0f, 0f), new Vector2(64f, 64f), new Vector2(0.5f, 0.5f)),
                CreateCommand(BattleRenderCommandType.Entity, new RuntimeEntityHandle(11, 1), 11, 2, 0, 1,
                    new Vector3(0.06f, 0f, 0f), new Vector2(64f, 64f), new Vector2(0.5f, 0.5f)),
                CreateCommand(BattleRenderCommandType.Entity, new RuntimeEntityHandle(12, 1), 12, 1, 0, 2,
                    new Vector3(0f, 0.04f, 0f), new Vector2(64f, 64f), new Vector2(0.5f, 0.5f)));

            using var backend = new BattleDynamicMeshBackend();
            backend.Build(frame, new FixtureResolver(resources, FixtureResolverMode.VisualData));
            PixelImage image = RenderCentral(backend, config.ImageSize);
            WriteImage(config.OutputDirectory, imageName, image);
            bool valid = backend.SegmentCount == 3 &&
                         backend.GetSegment(0).FirstCommandIndex == 0 &&
                         backend.GetSegment(1).FirstCommandIndex == 1 &&
                         backend.GetSegment(2).FirstCommandIndex == 2 &&
                         backend.GetSegment(0).Texture == resources.TextureA &&
                         backend.GetSegment(1).Texture == resources.TextureB &&
                         backend.GetSegment(2).Texture == resources.TextureA &&
                         image.NonTransparentPixelCount > 0;
            result.passed = valid;
            result.sourceCount = frame.CommandCount;
            result.resolvedCount = backend.Diagnostics.ResolvedCommandCount;
            result.segmentCount = backend.SegmentCount;
            result.chunkCount = backend.ActiveChunkCount;
            result.nonTransparentPixels = image.NonTransparentPixelCount;
            result.evidence = "segmentCommandStarts=0,1,2; textureOrder=A,B,A; overlapping alpha pixels rendered";
            return result;
        }

        private static BattleRenderingAcceptanceCase RunCategoryOrder(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            string imageName)
        {
            var result = Case("shadow-entity-overlay-hitrecord-order");
            RuntimeEntityHandle handle = new RuntimeEntityHandle(20, 1);
            var frame = new BattlePresentationFrame();
            BuildFrame(
                frame,
                5001,
                CreateCommand(BattleRenderCommandType.Shadow, handle, 20, 10, 0, 100,
                    Vector3.zero, new Vector2(72f, 72f), new Vector2(0.5f, 0.5f)),
                CreateCommand(BattleRenderCommandType.Entity, handle, 20, 11, 0, 101,
                    Vector3.zero, new Vector2(58f, 58f), new Vector2(0.5f, 0.5f)),
                CreateCommand(BattleRenderCommandType.OverlayGlyph, handle, 20, 12, 0, 102,
                    Vector3.zero, new Vector2(42f, 42f), new Vector2(0.5f, 0.5f)),
                CreateCommand(BattleRenderCommandType.HitRecord, handle, 20, 13, 0, 103,
                    Vector3.zero, new Vector2(24f, 24f), new Vector2(0.5f, 0.5f)));

            using var backend = new BattleDynamicMeshBackend();
            backend.Build(frame, new FixtureResolver(resources, FixtureResolverMode.CommandCategory));
            PixelImage image = RenderCentral(backend, config.ImageSize);
            WriteImage(config.OutputDirectory, imageName, image);
            bool valid = backend.Diagnostics.ResolvedCommandCount == 4 && backend.SegmentCount == 4;
            for (int index = 0; index < 4; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                BattleCentralRenderSegment segment = backend.GetSegment(index);
                valid &= command.SortOrder == 100 + index &&
                         command.LocalSequence == index &&
                         segment.FirstCommandIndex == index &&
                         segment.CommandCount == 1;
            }

            float pixelsPerWorldUnit = config.ImageSize * 0.5f;
            float hitHalfWidth = 24f * NTSDRenderSpace.UnitsPerPixelX *
                                 NTSDRenderSpace.BattleVisualScale * pixelsPerWorldUnit * 0.5f;
            float overlayHalfWidth = 42f * NTSDRenderSpace.UnitsPerPixelX *
                                     NTSDRenderSpace.BattleVisualScale * pixelsPerWorldUnit * 0.5f;
            float entityHalfWidth = 58f * NTSDRenderSpace.UnitsPerPixelX *
                                    NTSDRenderSpace.BattleVisualScale * pixelsPerWorldUnit * 0.5f;
            float shadowHalfWidth = 72f * NTSDRenderSpace.UnitsPerPixelX *
                                    NTSDRenderSpace.BattleVisualScale * pixelsPerWorldUnit * 0.5f;
            Color32 center = SamplePixelOffset(image, 0, 0);
            Color32 overlayRing = SamplePixelOffset(
                image,
                Mathf.RoundToInt((hitHalfWidth + overlayHalfWidth) * 0.5f),
                0);
            Color32 entityRing = SamplePixelOffset(
                image,
                Mathf.RoundToInt((overlayHalfWidth + entityHalfWidth) * 0.5f),
                0);
            Color32 shadowRing = SamplePixelOffset(
                image,
                Mathf.RoundToInt((entityHalfWidth + shadowHalfWidth) * 0.5f),
                0);
            valid &= image.NonTransparentPixelCount > 0 &&
                     NearlyColor(center, resources.HitExpected, 2) &&
                     NearlyColor(overlayRing, resources.OverlayExpected, 2) &&
                     NearlyColor(entityRing, resources.EntityExpected, 2) &&
                     NearlyColor(shadowRing, resources.ShadowExpected, 2);
            result.passed = valid;
            result.sourceCount = frame.CommandCount;
            result.resolvedCount = backend.Diagnostics.ResolvedCommandCount;
            result.segmentCount = backend.SegmentCount;
            result.chunkCount = backend.ActiveChunkCount;
            result.nonTransparentPixels = image.NonTransparentPixelCount;
            result.evidence = string.Format(
                CultureInfo.InvariantCulture,
                "orderedTypes=Shadow,Entity,OverlayGlyph,HitRecord; sortOrder=100,101,102,103; " +
                "sampleRGBA=center:{0}/{1}/{2}/{3},overlay:{4}/{5}/{6}/{7}," +
                "entity:{8}/{9}/{10}/{11},shadow:{12}/{13}/{14}/{15}",
                center.r, center.g, center.b, center.a,
                overlayRing.r, overlayRing.g, overlayRing.b, overlayRing.a,
                entityRing.r, entityRing.g, entityRing.b, entityRing.a,
                shadowRing.r, shadowRing.g, shadowRing.b, shadowRing.a);
            return result;
        }

        private static BattleRenderingAcceptanceCase RunChunkBoundaries(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            string imageName)
        {
            var result = Case("mesh-chunk-4095-4096-4097");
            int[] boundaries = { 4095, 4096, 4097 };
            bool valid = true;
            var frame = new BattlePresentationFrame();
            using var backend = new BattleDynamicMeshBackend();
            PixelImage stressImage = default;
            for (int boundaryIndex = 0; boundaryIndex < boundaries.Length; boundaryIndex++)
            {
                int count = boundaries[boundaryIndex];
                BuildGridFrame(frame, 6001 + boundaryIndex, count);
                backend.Build(frame, new FixtureResolver(resources, FixtureResolverMode.VisualData));
                int expectedChunks = (count + BattleDynamicMeshBackend.QuadsPerChunk - 1) /
                                     BattleDynamicMeshBackend.QuadsPerChunk;
                valid &= backend.Diagnostics.ResolvedCommandCount == count &&
                         backend.ActiveChunkCount == expectedChunks &&
                         backend.GetChunkActiveQuadCount(expectedChunks - 1) ==
                             (count - (expectedChunks - 1) * BattleDynamicMeshBackend.QuadsPerChunk);
                for (int chunkIndex = 0; chunkIndex < expectedChunks; chunkIndex++)
                {
                    Mesh mesh = backend.GetChunkMesh(chunkIndex);
                    valid &= mesh.indexFormat == IndexFormat.UInt16 &&
                             mesh.vertexCount == BattleDynamicMeshBackend.VerticesPerChunk;
                }
                if (count == 4097)
                    stressImage = RenderCentral(backend, config.ImageSize);
            }

            WriteImage(config.OutputDirectory, imageName, stressImage);
            valid &= stressImage.NonTransparentPixelCount >= 4097 &&
                     backend.SegmentCount == 2 &&
                     backend.GetSegment(0).QuadCount == 4096 &&
                     backend.GetSegment(1).QuadCount == 1;
            result.passed = valid;
            result.sourceCount = 4097;
            result.resolvedCount = backend.Diagnostics.ResolvedCommandCount;
            result.segmentCount = backend.SegmentCount;
            result.chunkCount = backend.ActiveChunkCount;
            result.nonTransparentPixels = stressImage.NonTransparentPixelCount;
            result.evidence = "boundaries=4095:1chunk,4096:1chunk,4097:2chunks; UInt16; stress pixels >= command count";
            return result;
        }

        private static BattleRenderingAcceptanceCase RunMissingResourceFailClosed(
            FixtureResources resources,
            int imageSize)
        {
            var result = Case("missing-resource-fail-closed");
            var frame = new BattlePresentationFrame();
            using var backend = new BattleDynamicMeshBackend();
            BuildFrame(
                frame,
                7001,
                CreateCommand(BattleRenderCommandType.Entity, new RuntimeEntityHandle(30, 1), 30, -999, 0, 0,
                    Vector3.zero, new Vector2(64f, 64f), new Vector2(0.5f, 0.5f)));
            backend.Build(frame, new FixtureResolver(resources, FixtureResolverMode.VisualData));
            PixelImage image = RenderCentral(backend, imageSize);
            bool valid = backend.Diagnostics.SourceCommandCount == 1 &&
                         backend.Diagnostics.ResolvedCommandCount == 0 &&
                         backend.Diagnostics.UnresolvedCommandCount == 1 &&
                         backend.Diagnostics.FirstUnresolvedCommandIndex == 0 &&
                         backend.ActiveChunkCount == 0 && backend.SegmentCount == 0 &&
                         image.NonTransparentPixelCount == 0;
            result.passed = valid;
            result.sourceCount = backend.Diagnostics.SourceCommandCount;
            result.resolvedCount = backend.Diagnostics.ResolvedCommandCount;
            result.segmentCount = backend.SegmentCount;
            result.chunkCount = backend.ActiveChunkCount;
            result.nonTransparentPixels = image.NonTransparentPixelCount;
            result.evidence = "missing visual resolved=0; unresolved=1; chunks=0; segments=0; transparent output";
            return result;
        }

        private static BattleRenderingAcceptanceCase RunLegacyCentralPixelParity(
            FixtureResources resources,
            BattleRenderingAcceptanceConfig config,
            string legacyName,
            string centralName,
            string diffName)
        {
            var result = Case("rendererless-frozen-frame-legacy-central-pixel-parity");
            RuntimeEntityHandle handle = new RuntimeEntityHandle(40, 9);
            var frame = new BattlePresentationFrame();
            BuildFrame(
                frame,
                8001,
                CreateCommand(BattleRenderCommandType.Shadow, handle, 40, 10, 0, 200,
                    new Vector3(-0.02f, -0.02f, 0f), new Vector2(76f, 54f), new Vector2(0.5f, 0.5f)),
                CreateCommand(BattleRenderCommandType.Entity, handle, 40, 11, 0, 201,
                    new Vector3(0f, 0f, 0f), new Vector2(62f, 70f), new Vector2(0.5f, 0.5f),
                    new Rect(0.05f, 0.1f, 0.9f, 0.85f), flipX: true),
                CreateCommand(BattleRenderCommandType.OverlayGlyph, handle, 40, 12, 0, 202,
                    new Vector3(0.03f, 0.05f, 0f), new Vector2(44f, 38f), new Vector2(0.5f, 0.5f)),
                CreateCommand(BattleRenderCommandType.HitRecord, handle, 40, 13, 0, 203,
                    new Vector3(-0.03f, 0.08f, 0f), new Vector2(28f, 28f), new Vector2(0.5f, 0.5f)));

            var resolver = new FixtureResolver(resources, FixtureResolverMode.CommandCategory);
            using var backend = new BattleDynamicMeshBackend();
            backend.Build(frame, resolver);
            PixelImage central = RenderCentral(backend, config.ImageSize);
            LegacyRenderResult legacy = RenderLegacy(frame, resolver, resources.LegacyMaterial, config.ImageSize);
            PixelDifference difference = ComparePixels(legacy.Image, central);
            WriteImage(config.OutputDirectory, legacyName, legacy.Image);
            WriteImage(config.OutputDirectory, centralName, central);
            WriteImage(config.OutputDirectory, diffName, difference.DifferenceImage);

            bool valid = frame.CommandCount > 0 && legacy.PresenterCount == frame.CommandCount &&
                         legacy.Image.NonTransparentPixelCount > 0 &&
                         central.NonTransparentPixelCount > 0 &&
                         backend.Diagnostics.ResolvedCommandCount == frame.CommandCount &&
                         difference.MeanChannelDifference <= PixelParityMeanTolerance &&
                         difference.MaximumChannelDifference <= PixelParityMaximumTolerance;
            result.passed = valid;
            result.sourceCount = frame.CommandCount;
            result.resolvedCount = backend.Diagnostics.ResolvedCommandCount;
            result.segmentCount = backend.SegmentCount;
            result.chunkCount = backend.ActiveChunkCount;
            result.nonTransparentPixels = Math.Min(
                legacy.Image.NonTransparentPixelCount,
                central.NonTransparentPixelCount);
            result.meanChannelDifference = difference.MeanChannelDifference;
            result.maximumChannelDifference = difference.MaximumChannelDifference;
            result.evidence = string.Format(
                CultureInfo.InvariantCulture,
                "frozenTick={0}; immutableCommands={1}; legacySpriteRenderers={2}; legacyPixels={3}; centralPixels={4}",
                frame.TickIndex,
                frame.CommandCount,
                legacy.PresenterCount,
                legacy.Image.NonTransparentPixelCount,
                central.NonTransparentPixelCount);
            return result;
        }

        private static void BuildGridFrame(BattlePresentationFrame frame, int tickIndex, int count)
        {
            var commands = new BattleRenderCommand[count];
            const int columns = 65;
            const float span = 1.8f;
            float xStep = span / (columns - 1);
            int rows = (count + columns - 1) / columns;
            float yStep = span / Math.Max(1, rows - 1);
            for (int index = 0; index < count; index++)
            {
                int column = index % columns;
                int row = index / columns;
                commands[index] = CreateCommand(
                    BattleRenderCommandType.Entity,
                    new RuntimeEntityHandle(index, 1),
                    index + 1,
                    1,
                    0,
                    index,
                    new Vector3(-0.9f + column * xStep, -0.9f + row * yStep, 0f),
                    new Vector2(1.2f, 1.2f),
                    new Vector2(0.5f, 0.5f));
            }
            BuildFrame(frame, tickIndex, commands);
        }

        private static BattleRenderCommand CreateCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int sortOrder,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect? uv = null,
            bool flipX = false,
            bool flipY = false)
        {
            var renderState = new BattleSpriteRenderState(
                new Color32(255, 255, 255, 255),
                flipX,
                flipY,
                SpriteMaskInteraction.None,
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
            return new BattleRenderCommand(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                0,
                handle.Slot,
                sortOrder,
                0,
                (int)type,
                position,
                size,
                pivot,
                uv ?? new Rect(0f, 0f, 1f, 1f),
                renderState,
                default);
        }

        private static void BuildFrame(
            BattlePresentationFrame frame,
            int tickIndex,
            params BattleRenderCommand[] commands)
        {
            FrameAccess.Reset(frame, tickIndex);
            for (int index = 0; index < commands.Length; index++)
                FrameAccess.AddCommand(frame, commands[index]);
        }

        private static PixelImage RenderCentral(BattleDynamicMeshBackend backend, int imageSize)
        {
            return RenderCommandBuffer(imageSize, commandBuffer =>
            {
                var properties = new MaterialPropertyBlock();
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    if (segment.Material == null || segment.Texture == null)
                        continue;
                    properties.Clear();
                    if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray)
                        properties.SetTexture(MainTexArrayId, segment.Texture);
                    else
                        properties.SetTexture(MainTexId, segment.Texture);
                    commandBuffer.DrawMesh(
                        backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity,
                        segment.Material,
                        segment.SubMeshIndex,
                        0,
                        properties);
                }
            });
        }

        private static PixelImage RenderCentral(
            BattleDynamicMeshBackend backend,
            int imageSize,
            Vector2 viewCenter,
            float halfExtent)
        {
            return RenderCommandBuffer(imageSize, viewCenter, halfExtent, commandBuffer =>
            {
                var properties = new MaterialPropertyBlock();
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    if (segment.Material == null || segment.Texture == null)
                        continue;
                    properties.Clear();
                    if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray)
                        properties.SetTexture(MainTexArrayId, segment.Texture);
                    else
                        properties.SetTexture(MainTexId, segment.Texture);
                    commandBuffer.DrawMesh(
                        backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity,
                        segment.Material,
                        segment.SubMeshIndex,
                        0,
                        properties);
                }
            });
        }

        private static PixelImage RenderProductionLegacy(
            in BattleRenderCommand command,
            BattleSpriteEntry entry,
            Material legacyMaterial,
            int imageSize,
            float halfExtent)
        {
            GameObject root = null;
            try
            {
                root = new GameObject("P8-C Production Legacy Presenter")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    layer = OffscreenLayer,
                };
                root.transform.position = command.Position;
                float sourceWidth = Mathf.Max(0.0001f, entry.PixelWidth);
                float sourceHeight = Mathf.Max(0.0001f, entry.PixelHeight);
                root.transform.localScale = new Vector3(
                    NTSDRenderSpace.BattleVisualScale * command.Size.x / sourceWidth,
                    NTSDRenderSpace.BattleVisualScale * command.Size.y / sourceHeight,
                    1f);
                SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = entry.LegacySprite;
                renderer.sharedMaterial = legacyMaterial;
                renderer.color = command.Color;
                renderer.flipX = command.FlipX;
                renderer.flipY = command.FlipY;
                renderer.sortingOrder = command.SortOrder;
                return RenderCommandBuffer(
                    imageSize,
                    command.Position,
                    halfExtent,
                    commandBuffer => commandBuffer.DrawRenderer(renderer, legacyMaterial));
            }
            finally
            {
                DestroyImmediateSafe(root);
            }
        }

        private static LegacyRenderResult RenderLegacy(
            BattlePresentationFrame frame,
            IBattleCentralResourceResolver resolver,
            Material legacyMaterial,
            int imageSize)
        {
            var roots = new List<GameObject>(frame.CommandCount);
            var sprites = new List<Sprite>(frame.CommandCount);
            var renderers = new List<SpriteRenderer>(frame.CommandCount);
            try
            {
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (resolver.Resolve(command, out BattleCentralResolvedResource resource) !=
                            BattleCentralResourceStatus.Resolved ||
                        !(resource.Texture is Texture2D texture))
                    {
                        continue;
                    }

                    Rect normalized = resource.NormalizedUv;
                    var pixelRect = new Rect(
                        normalized.x * texture.width,
                        normalized.y * texture.height,
                        normalized.width * texture.width,
                        normalized.height * texture.height);
                    Sprite sprite = Sprite.Create(
                        texture,
                        pixelRect,
                        resource.Pivot,
                        SimulationConstants.PIXELS_PER_UNIT,
                        0,
                        SpriteMeshType.FullRect,
                        Vector4.zero,
                        false);
                    sprite.name = "P8-C Legacy " + index;
                    sprites.Add(sprite);

                    GameObject root = new GameObject("P8-C Legacy Presenter " + index)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        layer = OffscreenLayer,
                    };
                    roots.Add(root);
                    root.transform.position = command.Position;
                    float sourceWidth = Mathf.Max(0.0001f, pixelRect.width);
                    float sourceHeight = Mathf.Max(0.0001f, pixelRect.height);
                    root.transform.localScale = new Vector3(
                        NTSDRenderSpace.BattleVisualScale * resource.PixelSize.x / sourceWidth,
                        NTSDRenderSpace.BattleVisualScale * resource.PixelSize.y / sourceHeight,
                        1f);
                    SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sharedMaterial = legacyMaterial;
                    renderer.color = command.Color;
                    renderer.flipX = command.FlipX;
                    renderer.flipY = command.FlipY;
                    renderer.sortingOrder = command.SortOrder;
                    renderers.Add(renderer);
                }

                PixelImage image = RenderCommandBuffer(imageSize, commandBuffer =>
                {
                    for (int index = 0; index < renderers.Count; index++)
                        commandBuffer.DrawRenderer(renderers[index], legacyMaterial);
                });
                return new LegacyRenderResult(image, renderers.Count);
            }
            finally
            {
                for (int index = 0; index < roots.Count; index++)
                    DestroyImmediateSafe(roots[index]);
                for (int index = 0; index < sprites.Count; index++)
                    DestroyImmediateSafe(sprites[index]);
            }
        }

        private static PixelImage RenderCommandBuffer(
            int imageSize,
            Action<CommandBuffer> enqueueDraws)
        {
            return RenderCommandBuffer(imageSize, Vector2.zero, 1f, enqueueDraws);
        }

        private static PixelImage RenderCommandBuffer(
            int imageSize,
            Vector2 viewCenter,
            float halfExtent,
            Action<CommandBuffer> enqueueDraws)
        {
            if (halfExtent <= 0f)
                throw new ArgumentOutOfRangeException(nameof(halfExtent));
            var target = new RenderTexture(
                imageSize,
                imageSize,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear)
            {
                name = "P8-C Acceptance Target",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            target.Create();
            var commandBuffer = new CommandBuffer { name = "P8-C Acceptance Offscreen" };
            RenderTexture previous = RenderTexture.active;
            Texture2D readback = null;
            try
            {
                commandBuffer.SetRenderTarget(target);
                commandBuffer.SetViewport(new Rect(0f, 0f, imageSize, imageSize));
                commandBuffer.ClearRenderTarget(false, true, Color.clear);
                Matrix4x4 projection = GL.GetGPUProjectionMatrix(
                    Matrix4x4.Ortho(
                        -halfExtent,
                        halfExtent,
                        -halfExtent,
                        halfExtent,
                        -10f,
                        10f),
                    true);
                Matrix4x4 view = Matrix4x4.Translate(
                    new Vector3(-viewCenter.x, -viewCenter.y, 0f));
                commandBuffer.SetViewProjectionMatrices(view, projection);
                enqueueDraws(commandBuffer);
                Graphics.ExecuteCommandBuffer(commandBuffer);

                RenderTexture.active = target;
                readback = new Texture2D(
                    imageSize,
                    imageSize,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                readback.ReadPixels(new Rect(0f, 0f, imageSize, imageSize), 0, 0, false);
                Color32[] pixels = readback.GetPixels32();
                int nonTransparent = CountNonTransparent(pixels);
                return new PixelImage(imageSize, imageSize, pixels, nonTransparent);
            }
            finally
            {
                RenderTexture.active = previous;
                commandBuffer.Release();
                DestroyImmediateSafe(readback);
                target.Release();
                DestroyImmediateSafe(target);
            }
        }

        private static PixelDifference ComparePixels(PixelImage left, PixelImage right)
        {
            if (left.Width != right.Width || left.Height != right.Height ||
                left.Pixels == null || right.Pixels == null || left.Pixels.Length != right.Pixels.Length)
            {
                throw new InvalidOperationException("Pixel images must have identical dimensions.");
            }

            var differencePixels = new Color32[left.Pixels.Length];
            long channelDifference = 0;
            int maximum = 0;
            for (int index = 0; index < left.Pixels.Length; index++)
            {
                Color32 a = left.Pixels[index];
                Color32 b = right.Pixels[index];
                int red = Math.Abs(a.r - b.r);
                int green = Math.Abs(a.g - b.g);
                int blue = Math.Abs(a.b - b.b);
                int alpha = Math.Abs(a.a - b.a);
                channelDifference += red + green + blue + alpha;
                maximum = Math.Max(maximum, Math.Max(Math.Max(red, green), Math.Max(blue, alpha)));
                differencePixels[index] = new Color32((byte)red, (byte)green, (byte)blue, 255);
            }

            float mean = left.Pixels.Length == 0
                ? 0f
                : (float)(channelDifference / (double)(left.Pixels.Length * 4));
            var differenceImage = new PixelImage(
                left.Width,
                left.Height,
                differencePixels,
                CountNonBlack(differencePixels));
            return new PixelDifference(mean, maximum, differenceImage);
        }

        private static void WriteImage(string directory, string fileName, PixelImage image)
        {
            if (image.Pixels == null || image.Pixels.Length == 0)
                throw new InvalidOperationException("Cannot write an empty pixel image.");
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(image.Width, image.Height, TextureFormat.RGBA32, false, true)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                texture.SetPixels32(image.Pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(Path.Combine(directory, fileName), texture.EncodeToPNG());
            }
            finally
            {
                DestroyImmediateSafe(texture);
            }
        }

        private static int CountNonTransparent(Color32[] pixels)
        {
            int count = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                if (pixels[index].a != 0)
                    count++;
            }
            return count;
        }

        private static int CountNonBlack(Color32[] pixels)
        {
            int count = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                if (pixel.r != 0 || pixel.g != 0 || pixel.b != 0)
                    count++;
            }
            return count;
        }

        private static Color32 SampleWorld(PixelImage image, Vector2 worldPosition)
        {
            int x = Mathf.Clamp(
                Mathf.FloorToInt((worldPosition.x + 1f) * 0.5f * image.Width),
                0,
                image.Width - 1);
            int y = Mathf.Clamp(
                Mathf.FloorToInt((worldPosition.y + 1f) * 0.5f * image.Height),
                0,
                image.Height - 1);
            return image.Pixels[y * image.Width + x];
        }

        private static Color32 SamplePixelOffset(PixelImage image, int offsetX, int offsetY)
        {
            int x = Mathf.Clamp(image.Width / 2 + offsetX, 0, image.Width - 1);
            int y = Mathf.Clamp(image.Height / 2 + offsetY, 0, image.Height - 1);
            return image.Pixels[y * image.Width + x];
        }

        private static bool NearlyColor(Color32 actual, Color32 expected, int tolerance)
        {
            return Math.Abs(actual.r - expected.r) <= tolerance &&
                   Math.Abs(actual.g - expected.g) <= tolerance &&
                   Math.Abs(actual.b - expected.b) <= tolerance &&
                   Math.Abs(actual.a - expected.a) <= tolerance;
        }

        private static BattleRenderingAcceptanceCase Case(string name)
        {
            return new BattleRenderingAcceptanceCase
            {
                name = name,
                evidence = string.Empty,
            };
        }

        private static void ConfigureMount(
            BattleCentralPresentationMount mount,
            BattleCentralPresentationMountRole role,
            BattleCentralPresentationMountPurpose purpose,
            LF2ObjectRenderer owner,
            RuntimeEntityHandle handle)
        {
            MountAccess.Configure(mount, role, purpose, owner);
            MountAccess.SetHandle(mount, handle);
        }

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static void DestroyImmediateSafe(UnityEngine.Object value)
        {
            if (value != null)
                UnityEngine.Object.DestroyImmediate(value);
        }

        private readonly struct PixelImage
        {
            public PixelImage(int width, int height, Color32[] pixels, int nonTransparentPixelCount)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
                NonTransparentPixelCount = nonTransparentPixelCount;
            }

            public int Width { get; }
            public int Height { get; }
            public Color32[] Pixels { get; }
            public int NonTransparentPixelCount { get; }
        }

        private readonly struct PixelDifference
        {
            public PixelDifference(
                float meanChannelDifference,
                int maximumChannelDifference,
                PixelImage differenceImage)
            {
                MeanChannelDifference = meanChannelDifference;
                MaximumChannelDifference = maximumChannelDifference;
                DifferenceImage = differenceImage;
            }

            public float MeanChannelDifference { get; }
            public int MaximumChannelDifference { get; }
            public PixelImage DifferenceImage { get; }
        }

        private readonly struct LegacyRenderResult
        {
            public LegacyRenderResult(PixelImage image, int presenterCount)
            {
                Image = image;
                PresenterCount = presenterCount;
            }

            public PixelImage Image { get; }
            public int PresenterCount { get; }
        }

        private readonly struct ProductionSample
        {
            public ProductionSample(
                int objectId,
                int objectType,
                LF2FrameData frame,
                BattleSpriteEntry entry)
            {
                ObjectId = objectId;
                ObjectType = objectType;
                Frame = frame;
                Entry = entry;
            }

            public int ObjectId { get; }
            public int ObjectType { get; }
            public LF2FrameData Frame { get; }
            public BattleSpriteEntry Entry { get; }
        }

        private enum FixtureResolverMode : byte
        {
            VisualData = 0,
            CommandCategory = 1,
        }

        private sealed class FixtureResolver : IBattleCentralResourceResolver
        {
            private readonly FixtureResources resources;
            private readonly FixtureResolverMode mode;

            public FixtureResolver(FixtureResources resources, FixtureResolverMode mode)
            {
                this.resources = resources;
                this.mode = mode;
            }

            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                if (command.VisualDataId < 0)
                {
                    resource = default;
                    return BattleCentralResourceStatus.UnresolvedVisual;
                }

                Texture2D texture;
                if (mode == FixtureResolverMode.CommandCategory)
                {
                    texture = command.Type switch
                    {
                        BattleRenderCommandType.Shadow => resources.ShadowTexture,
                        BattleRenderCommandType.Entity => resources.EntityTexture,
                        BattleRenderCommandType.OverlayGlyph => resources.OverlayTexture,
                        BattleRenderCommandType.HitRecord => resources.HitTexture,
                        _ => null,
                    };
                }
                else
                {
                    texture = command.VisualDataId == 2 ? resources.TextureB : resources.TextureA;
                }

                if (texture == null || resources.CentralMaterial == null)
                {
                    resource = default;
                    return BattleCentralResourceStatus.UnresolvedVisual;
                }
                resource = new BattleCentralResolvedResource(
                    texture,
                    resources.CentralMaterial,
                    command.NormalizedUv,
                    command.Size,
                    command.Pivot,
                    command.Color,
                    (int)command.RenderState.MaterialSemantic,
                    0,
                    BattleSpriteCentralBindingMode.SourceTexture2D);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private sealed class AtlasArrayResolver : IBattleCentralResourceResolver
        {
            private readonly FixtureResources resources;

            public AtlasArrayResolver(FixtureResources resources)
            {
                this.resources = resources;
            }

            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(
                    resources.ArrayTexture,
                    resources.ArrayMaterial,
                    command.NormalizedUv,
                    command.Size,
                    command.Pivot,
                    command.Color,
                    (int)command.RenderState.MaterialSemantic,
                    command.EffectivePic,
                    BattleSpriteCentralBindingMode.AtlasTextureArray);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private sealed class AtlasPagesResolver : IBattleCentralResourceResolver
        {
            private readonly FixtureResources resources;

            public AtlasPagesResolver(FixtureResources resources)
            {
                this.resources = resources;
            }

            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                Texture2D texture = command.EffectivePic == 1 ? resources.TextureB : resources.TextureA;
                resource = new BattleCentralResolvedResource(
                    texture,
                    resources.CentralMaterial,
                    command.NormalizedUv,
                    command.Size,
                    command.Pivot,
                    command.Color,
                    (int)command.RenderState.MaterialSemantic,
                    command.EffectivePic,
                    BattleSpriteCentralBindingMode.AtlasPageTexture2D);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private sealed class FixtureResources : IDisposable
        {
            private static readonly Color32 ShadowColor = new Color32(25, 35, 45, 255);
            private static readonly Color32 EntityColor = new Color32(220, 45, 55, 255);
            private static readonly Color32 OverlayColor = new Color32(245, 210, 35, 255);
            private static readonly Color32 HitColor = new Color32(40, 115, 240, 255);

            public Texture2D TextureA { get; private set; }
            public Texture2D TextureB { get; private set; }
            public Texture2D ShadowTexture { get; private set; }
            public Texture2D EntityTexture { get; private set; }
            public Texture2D OverlayTexture { get; private set; }
            public Texture2D HitTexture { get; private set; }
            public Texture2DArray ArrayTexture { get; private set; }
            public Material CentralMaterial { get; private set; }
            public Material ArrayMaterial { get; private set; }
            public Material LegacyMaterial { get; private set; }
            public Color32 ShadowExpected => ShadowColor;
            public Color32 EntityExpected => EntityColor;
            public Color32 OverlayExpected => OverlayColor;
            public Color32 HitExpected => HitColor;

            public void Initialize()
            {
                Shader centralShader = Shader.Find(BattleSpriteMaterialContract.CentralTextureShaderName);
                Shader arrayShader = Shader.Find(BattleSpriteMaterialContract.CentralArrayShaderName);
                Shader legacyShader = Shader.Find(BattleSpriteMaterialContract.BuiltInSpriteShaderName);
                if (centralShader == null || arrayShader == null || legacyShader == null)
                {
                    throw new InvalidOperationException(
                        "P8-C acceptance requires the central 2D, central array, and Sprites/Default shaders.");
                }

                TextureA = CreatePatternTexture(new Color32(235, 35, 45, 150), 1);
                TextureB = CreatePatternTexture(new Color32(30, 220, 90, 165), 2);
                ShadowTexture = CreateSolidTexture(ShadowColor, "P8-C Shadow");
                EntityTexture = CreateSolidTexture(EntityColor, "P8-C Entity");
                OverlayTexture = CreateSolidTexture(OverlayColor, "P8-C Overlay");
                HitTexture = CreateSolidTexture(HitColor, "P8-C HitRecord");
                CentralMaterial = NewMaterial(centralShader, "P8-C Central Material");
                ArrayMaterial = NewMaterial(arrayShader, "P8-C Array Material");
                LegacyMaterial = NewMaterial(legacyShader, "P8-C Legacy Material");

                if (SystemInfo.supports2DArrayTextures)
                {
                    ArrayTexture = new Texture2DArray(32, 32, 2, TextureFormat.RGBA32, false, true)
                    {
                        name = "P8-C Texture Array",
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    ArrayTexture.SetPixels32(TextureA.GetPixels32(), 0, 0);
                    ArrayTexture.SetPixels32(TextureB.GetPixels32(), 1, 0);
                    ArrayTexture.Apply(false, false);
                }
            }

            public void Dispose()
            {
                DestroyImmediateSafe(ArrayTexture);
                DestroyImmediateSafe(TextureA);
                DestroyImmediateSafe(TextureB);
                DestroyImmediateSafe(ShadowTexture);
                DestroyImmediateSafe(EntityTexture);
                DestroyImmediateSafe(OverlayTexture);
                DestroyImmediateSafe(HitTexture);
                DestroyImmediateSafe(CentralMaterial);
                DestroyImmediateSafe(ArrayMaterial);
                DestroyImmediateSafe(LegacyMaterial);
            }

            private static Material NewMaterial(Shader shader, string name)
            {
                var material = new Material(shader)
                {
                    name = name,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                material.SetColor("_Color", Color.white);
                return material;
            }

            private static Texture2D CreatePatternTexture(Color32 baseColor, int phase)
            {
                const int size = 32;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
                {
                    name = "P8-C Pattern " + phase,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int index = y * size + x;
                        if (x < 2 || y < 2 || x >= size - 2 || y >= size - 2 ||
                            ((x + y + phase) % 13 == 0))
                        {
                            pixels[index] = Color.clear;
                            continue;
                        }
                        byte red = (byte)Math.Min(255, baseColor.r + ((x + phase * 3) % 11));
                        byte green = (byte)Math.Min(255, baseColor.g + ((y + phase * 5) % 13));
                        byte blue = (byte)Math.Min(255, baseColor.b + ((x * 3 + y + phase) % 9));
                        pixels[index] = new Color32(red, green, blue, baseColor.a);
                    }
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                return texture;
            }

            private static Texture2D CreateSolidTexture(Color32 color, string name)
            {
                const int size = 8;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
                {
                    name = name,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                var pixels = new Color32[size * size];
                for (int index = 0; index < pixels.Length; index++)
                    pixels[index] = color;
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                return texture;
            }
        }

        private sealed class AcceptanceEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public void SetFixtureStableId(int value)
            {
                StableId = value;
            }

            public override void Reset()
            {
                Runtime.Reset();
            }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
            {
            }
        }

        private static class FrameAccess
        {
            private delegate void ResetDelegate(
                BattlePresentationFrame frame,
                int tickIndex,
                BattleCommonVisualCatalog commonVisualCatalog);

            private delegate void AddCommandDelegate(
                BattlePresentationFrame frame,
                in BattleRenderCommand command);

            private static readonly ResetDelegate ResetMethod =
                (ResetDelegate)typeof(BattlePresentationFrame)
                    .GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic)
                    .CreateDelegate(typeof(ResetDelegate));

            private static readonly AddCommandDelegate AddCommandMethod =
                (AddCommandDelegate)typeof(BattlePresentationFrame)
                    .GetMethod("AddCommand", BindingFlags.Instance | BindingFlags.NonPublic)
                    .CreateDelegate(typeof(AddCommandDelegate));

            public static void Reset(BattlePresentationFrame frame, int tickIndex)
            {
                ResetMethod(frame, tickIndex, null);
            }

            public static void AddCommand(BattlePresentationFrame frame, in BattleRenderCommand command)
            {
                AddCommandMethod(frame, command);
            }
        }

        private static class MountAccess
        {
            private delegate void ConfigureDelegate(
                BattleCentralPresentationMount mount,
                BattleCentralPresentationMountRole role,
                BattleCentralPresentationMountPurpose purpose,
                LF2ObjectRenderer owner);

            private delegate void SetHandleDelegate(
                BattleCentralPresentationMount mount,
                RuntimeEntityHandle handle);

            private static readonly ConfigureDelegate ConfigureMethod =
                (ConfigureDelegate)typeof(BattleCentralPresentationMount)
                    .GetMethod("ConfigureForSelfCheck", BindingFlags.Instance | BindingFlags.NonPublic)
                    .CreateDelegate(typeof(ConfigureDelegate));

            private static readonly SetHandleDelegate SetHandleMethod =
                (SetHandleDelegate)typeof(BattleCentralPresentationMount)
                    .GetMethod("SetRuntimeHandle", BindingFlags.Instance | BindingFlags.NonPublic)
                    .CreateDelegate(typeof(SetHandleDelegate));

            public static void Configure(
                BattleCentralPresentationMount mount,
                BattleCentralPresentationMountRole role,
                BattleCentralPresentationMountPurpose purpose,
                LF2ObjectRenderer owner)
            {
                ConfigureMethod(mount, role, purpose, owner);
            }

            public static void SetHandle(
                BattleCentralPresentationMount mount,
                RuntimeEntityHandle handle)
            {
                SetHandleMethod(mount, handle);
            }
        }
    }
}
#endif


--- File: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingAcceptanceEditorTests.cs ---
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingAcceptanceEditorTests
    {
        [Test]
        public void Config_NormalizesProjectRelativeOutputAndRejectsInvalidRanges()
        {
            string root = Path.GetFullPath(Path.Combine("Temp", "P8-C-ConfigRoot"));
            var request = new BattleRenderingAcceptanceRequest
            {
                outputDirectory = "evidence",
                imageSize = 128,
                exerciseLivePool = false,
                livePoolExtraCount = 2,
            };

            BattleRenderingAcceptanceConfig config =
                BattleRenderingAcceptanceConfig.FromRequest(request, root);

            Assert.That(config.OutputDirectory, Is.EqualTo(Path.GetFullPath(Path.Combine(root, "evidence"))));
            Assert.That(config.ImageSize, Is.EqualTo(128));
            Assert.That(config.ExerciseLivePool, Is.False);
            Assert.That(config.LivePoolExtraCount, Is.EqualTo(2));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BattleRenderingAcceptanceConfig("Temp/P8-C", 32, false, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BattleRenderingAcceptanceConfig("Temp/P8-C", 128, false, 0));
        }

        [Test]
        public void FullMatrix_IsDeterministicAndWritesNonEmptyLegacyCentralEvidence()
        {
            string output = Path.GetFullPath(Path.Combine("Temp", "P8-C-EditModeTest"));
            var config = new BattleRenderingAcceptanceConfig(output, 256, false, 1);

            BattleRenderingAcceptanceReport report =
                BattleRenderingAcceptanceHarness.Run(config);

            Assert.That(report.passed, Is.True, report.ToJson());
            Assert.That(report.generationReuse.passed, Is.True);
            Assert.That(report.generationReuse.sourceCount, Is.EqualTo(1000));
            Assert.That(report.generationReuse.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.isolatedPoolExpansion.sourceCount, Is.EqualTo(33));
            Assert.That(report.isolatedPoolExpansion.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.livePoolExpansion.available, Is.False);
            Assert.That(report.atlasArrayAndOrderedPages.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.transparentResourceInterleave.segmentCount, Is.EqualTo(3));
            Assert.That(report.categoryOcclusionOrder.resolvedCount, Is.EqualTo(4));
            Assert.That(report.categoryOcclusionOrder.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.chunkBoundaries.sourceCount, Is.EqualTo(4097));
            Assert.That(report.chunkBoundaries.chunkCount, Is.EqualTo(2));
            Assert.That(report.missingResourceFailClosed.nonTransparentPixels, Is.EqualTo(0));
            Assert.That(report.legacyCentralPixelParity.sourceCount, Is.GreaterThan(0));
            Assert.That(report.legacyCentralPixelParity.nonTransparentPixels, Is.GreaterThan(0));

            string firstJson = report.ToJson();
            string secondJson = report.ToJson();
            Assert.That(secondJson, Is.EqualTo(firstJson));
            StringAssert.Contains("ntsd-battle-rendering-acceptance-v1", firstJson);
            StringAssert.Contains("synthetic fixture only", report.syntheticFixtureEvidenceScope);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ReportFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.LegacyFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.CentralFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ParityDiffFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.GenerationReuseFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.IsolatedExpansionFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ArrayFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.OrderedPagesFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.AtlasDiffFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.InterleaveFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.CategoryOcclusionFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.Chunk4097FileName);
        }

        [Test]
        public void RequestedProductionPath_FailsExplicitlyOutsidePlayModeAndWritesReport()
        {
            string output = Path.GetFullPath(Path.Combine("Temp", "P8-C-RequestedProductionUnavailable"));
            var config = new BattleRenderingAcceptanceConfig(output, 128, true, 1);

            BattleRenderingAcceptanceReport report =
                BattleRenderingAcceptanceHarness.Run(config);

            Assert.That(report.livePoolRequested, Is.True);
            Assert.That(report.passed, Is.False);
            Assert.That(report.livePoolExpansion.available, Is.False);
            Assert.That(report.livePoolExpansion.passed, Is.False);
            StringAssert.Contains("requested but unavailable", report.livePoolExpansion.evidence);
            Assert.That(report.productionCatalogPixelParity.available, Is.False);
            Assert.That(report.productionCatalogPixelParity.passed, Is.False);
            StringAssert.Contains("requested but unavailable", report.productionCatalogPixelParity.evidence);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ReportFileName);
        }

        private static void AssertArtifact(string directory, string name)
        {
            string path = Path.Combine(directory, name);
            Assert.That(File.Exists(path), Is.True, path);
            Assert.That(new FileInfo(path).Length, Is.GreaterThan(0), path);
        }
    }
}
#endif


--- File: Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs ---
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Tools;
using NTSD.App;
using Cysharp.Threading.Tasks;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2 对象池（MonoBehaviour 单例）
    /// 配置数据从 GameConfig.Instance 读取。
    /// </summary>
    public class LF2ObjectPool : MMSingleton<LF2ObjectPool>
    {
        [Header("父节点配置")]
        [SerializeField] private Transform _poolRoot;
        [SerializeField] private Transform _activeRoot;
        [SerializeField] private Transform _spriteRoot;

        // ========== 池数据结构 ==========
        private LinkedList<GameObject> _availableObjects;
        private HashSet<GameObject> _activeObjects;
        private Dictionary<GameObject, float> _releaseTimeMap;
        private float _lastCheckTime;

        private Stack<SpriteRenderer> _spritePool;
        private Material _spriteDefaultSharedMaterial;

        // ========== 配置快捷访问 ==========
        private static GameConfig Cfg => GameConfig.Instance;

        // 缓存 prefab 引用，避免懒加载时 GameConfig.Instance 为 null
        private GameObject _cachedLF2ObjectPrefab;

        // Read-only acceptance evidence; avoids editor tooling reflecting private pool state.
        public int AvailableObjectCountForAcceptance => _availableObjects?.Count ?? 0;
        public int ActiveObjectCountForAcceptance => _activeObjects?.Count ?? 0;

        // ========== 生命周期 ==========

        protected override void Awake()
        {
            base.Awake();
            NormalizeTransform(transform);

            _availableObjects = new LinkedList<GameObject>();
            _activeObjects = new HashSet<GameObject>();
            _releaseTimeMap = new Dictionary<GameObject, float>();
            _spritePool = new Stack<SpriteRenderer>(32);

            // 缓存 prefab 引用 - 延迟到 CreateNewObject 时再获取
            _cachedLF2ObjectPrefab = null;

            for (int i = 0; i < (Cfg?.PoolInitialSize ?? 0); i++)
                CreateNewObject();

            int spritePoolSize = Cfg?.PoolInitialSpritePoolSize ?? 16;
            for (int i = 0; i < spritePoolSize; i++)
            {
                var go = new GameObject("Spark");
                go.layer = LayerMask.NameToLayer("Battle");
                Transform parent = _spriteRoot != null ? _spriteRoot : transform;
                go.transform.SetParent(parent, false);
                var sr = go.AddComponent<SpriteRenderer>();
                CaptureOrApplySpriteDefaultMaterial(sr);
                LF2ObjectRenderer.NormalizeSpriteRendererState(sr, _spriteDefaultSharedMaterial);
                sr.sortingLayerName = "Object";
                sr.gameObject.SetActive(false);
                _spritePool.Push(sr);
            }
        }

        // ========== 核心方法 ==========

        /// <summary>
        /// 创建新对象：优先使用 Prefab，否则动态创建最小 GameObject。
        /// </summary>
        private LF2ObjectRenderer CreateNewObject()
        {
            if (_cachedLF2ObjectPrefab == null) _cachedLF2ObjectPrefab = Cfg?.LF2ObjectPrefab;

            GameObject go;
            if (_cachedLF2ObjectPrefab != null)
            {
                go = Instantiate(_cachedLF2ObjectPrefab, _poolRoot != null ? _poolRoot : this.transform);
                go.layer = LayerMask.NameToLayer("Battle");
            }
            else
            {
                go = new GameObject("LF2Object");
                go.layer = LayerMask.NameToLayer("Battle");
                go.SetActive(false);

                var entityModel = new GameObject("EntityModel");
                entityModel.layer = LayerMask.NameToLayer("Battle");
                entityModel.transform.SetParent(go.transform, false);
                LF2ObjectRenderer fallbackRenderer = entityModel.AddComponent<LF2ObjectRenderer>();
                BattleCentralPresentationMount entityMount =
                    entityModel.AddComponent<BattleCentralPresentationMount>();
                entityMount.ConfigureRuntimeFallback(
                    BattleCentralPresentationMountRole.EntityModel,
                    BattleCentralPresentationMountPurpose.EntitySprite,
                    fallbackRenderer);

                var shadow = new GameObject("Shadow");
                shadow.layer = LayerMask.NameToLayer("Battle");
                shadow.transform.SetParent(go.transform, false);
                BattleCentralPresentationMount shadowMount =
                    shadow.AddComponent<BattleCentralPresentationMount>();
                shadowMount.ConfigureRuntimeFallback(
                    BattleCentralPresentationMountRole.Shadow,
                    BattleCentralPresentationMountPurpose.CommonShadow,
                    fallbackRenderer);
            }

            NormalizeTransform(go.transform, resetScale: false);

            go.SetActive(false);

            // LF2ObjectRenderer 挂在子节点 EntityModel 上，不在根节点
            var r = go.GetComponentInChildren<LF2ObjectRenderer>(true);
            if (r == null)
            {
                Log.Error("[LF2ObjectPool] EntityModel missing LF2ObjectRenderer");
                Destroy(go);
                return null;
            }

            _availableObjects.AddLast(go);
            return r;
        }

        /// <summary>从池中获取对象（懒加载）</summary>
        public GameObject Get(out LF2ObjectRenderer EntityModel)
        {
            int maxPoolSize = Cfg?.PoolMaxSize ?? 200;

            GameObject go;
            EntityModel = null;
            if (_availableObjects.Count == 0)
            {
                if (_activeObjects.Count >= maxPoolSize)
                    Log.Warn("[LF2ObjectPool] Pool over limit: active={0}/{1}, expanding.", _activeObjects.Count, maxPoolSize);
                CreateNewObject();
                if (_availableObjects.Count == 0)
                {
                    Log.Error("[LF2ObjectPool] CreateNewObject failed (active={0})", _activeObjects.Count);
                    return null;
                }
            }

            go = _availableObjects.First.Value;
            _availableObjects.RemoveFirst();

            Transform activeParent = _activeRoot != null ? _activeRoot : this.transform;
            go.transform.SetParent(activeParent, false);
            NormalizeTransform(go.transform, resetScale: false);

            go.SetActive(true);
            _activeObjects.Add(go);
            EntityModel = go.GetComponentInChildren<LF2ObjectRenderer>(true);
            if (EntityModel != null)
            {
                // 回收时 EntityModel 子节点会被 ResetState 关闭，取出时必须显式恢复。
                EntityModel.gameObject.SetActive(true);
                EntityModel.RestorePooledVisualState();
            }
            return go;
        }

        /// <summary>
        /// 批量预热接口（对齐 C++ release SceneManager_Init 的 400 个实体实例预分配）。
        /// </summary>
        public async UniTask PrewarmAsync(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateNewObject();
                // 每实例化 5 个对象让出一帧，确保 Loading 动画不卡顿
                if (i % 5 == 0) await UniTask.Yield();
            }
            Log.Info("[LF2ObjectPool] Bulk Prewarm: {0} GameObjects", count);
        }

        /// <summary>归还对象到池</summary>
        public void Release(LF2ObjectRenderer r)
        {
            if (r == null) return;

            r.ResetState();

            var go = r.transform.parent.gameObject;

            if (_poolRoot != null)
                go.transform.SetParent(_poolRoot, false);

            go.SetActive(false);
            _activeObjects.Remove(go);
            _availableObjects.AddLast(go);
            _releaseTimeMap[go] = Time.time;
        }

        // ========== 超时卸载 ==========

        private void Update()
        {
            int initialSize = Cfg?.PoolInitialSize ?? 0;
            float expireTime = Cfg?.PoolExpireTimeSeconds ?? 120f;
            float checkInterval = Cfg?.PoolCheckIntervalSeconds ?? 10f;

            if (_availableObjects.Count <= initialSize)
            {
                _releaseTimeMap.Clear();
                return;
            }

            if (Time.time - _lastCheckTime < checkInterval) return;
            _lastCheckTime = Time.time;

            var node = _availableObjects.First;
            while (node != null)
            {
                var next = node.Next;
                var obj = node.Value;

                if (_releaseTimeMap.TryGetValue(obj, out float t) &&
                    Time.time - t >= expireTime)
                {
                    _availableObjects.Remove(node);
                    _releaseTimeMap.Remove(obj);
                    Destroy(obj);

                    if (_availableObjects.Count <= initialSize)
                    {
                        _releaseTimeMap.Clear();
                        break;
                    }
                }

                node = next;
            }
        }

        // ========== Bucket B：SpriteRenderer 桶 ==========

        /// <summary>
        /// 从轻量 SpriteRenderer 桶取出一个 SpriteRenderer（懒加载）。
        /// 池空时创建新 GameObject 并挂载 SpriteRenderer，统一挂在 _spriteRoot 下（Inspector 指定，null 时挂在本对象上）。
        /// 取出后 SetActive(true)，不注册 SimulationWorld。
        /// </summary>
        public SpriteRenderer GetSprite()
        {
            SpriteRenderer sr;
            if (_spritePool.Count > 0)
            {
                sr = _spritePool.Pop();
            }
            else
            {
                var go = new GameObject("Spark");
                go.layer = LayerMask.NameToLayer("Battle");
                // 挂到场景根节点，避免父节点 inactive 导致无法显示
                Transform parent = _spriteRoot != null ? _spriteRoot : null;
                if (parent != null)
                    go.transform.SetParent(parent, false);
                sr = go.AddComponent<SpriteRenderer>();
                CaptureOrApplySpriteDefaultMaterial(sr);
                sr.sortingLayerName = "Object";
            }

            CaptureOrApplySpriteDefaultMaterial(sr);
            LF2ObjectRenderer.NormalizeSpriteRendererState(sr, _spriteDefaultSharedMaterial);
            sr.gameObject.SetActive(true);
            return sr;
        }

        /// <summary>
        /// 归还 SpriteRenderer 到轻量桶：清空 sprite，SetActive(false)，压栈。
        /// 防重复归还：已处于非激活状态则直接跳过。
        /// </summary>
        public void ReleaseSprite(SpriteRenderer sr)
        {
            if (sr == null) return;
            if (!sr.gameObject.activeSelf) return;  // 已归还过，防重复压栈
            sr.sprite = null;
            CaptureOrApplySpriteDefaultMaterial(sr);
            LF2ObjectRenderer.NormalizeSpriteRendererState(sr, _spriteDefaultSharedMaterial);
            sr.gameObject.SetActive(false);
            _spritePool.Push(sr);
        }

        public string GetPoolStatus() =>
            $"Available: {_availableObjects.Count}, Active: {_activeObjects.Count}";

        private static void NormalizeTransform(Transform target, bool resetScale = true)
        {
            if (target == null) return;
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            if (resetScale)
                target.localScale = Vector3.one;
        }

        private void CaptureOrApplySpriteDefaultMaterial(SpriteRenderer renderer)
        {
            if (renderer == null)
                return;
            if (_spriteDefaultSharedMaterial == null)
                _spriteDefaultSharedMaterial =
                    LF2ObjectRenderer.ResolveBorrowedDefaultSharedMaterial(renderer);
            else if (renderer.sharedMaterial != _spriteDefaultSharedMaterial)
                renderer.sharedMaterial = _spriteDefaultSharedMaterial;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 注册、运行时槽位和基础上下文。
    /// </summary>
    public partial class SimulationWorld
    {
        /// <summary>同一 SimOrder 的对象桶；只有桶内容变化后才延迟重新排序。</summary>
        private class Bucket
        {
            public List<ISimObject> items = new List<ISimObject>();
            public bool dirty = false;

            public void EnsureSorted(System.Func<ISimObject, int> stableIdSelector)
            {
                if (dirty)
                {
                    items = items.OrderBy(stableIdSelector).ToList();
                    dirty = false;
                }
            }
        }

        /// <summary>按 SimOrder 建立的模拟桶，SortedDictionary 保证 pass 顺序。</summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();
        /// <summary>注册对象时注入的模拟上下文。</summary>
        private SimContext _context;
        /// <summary>给没有显式运行时 ID 的对象自动分配 StableId。</summary>
        private int _nextAutoStableId = 100;
        internal const int AuthorityRuntimeSlotCapacity =
            BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity;
        private const int DynamicRuntimeSlotStart = 50;
        private readonly BattleRuntimeProfile activeRuntimeProfile;
        private readonly RuntimeSlotTable _runtimeSlots;
        private readonly RuntimeRestStore _runtimeRestStore;
        private readonly int maxActiveRuntimeEntities;
        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();
        private readonly List<LF2Entity> _pendingSlotReleasedDestroy = new List<LF2Entity>();
        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking = false;
        private readonly List<LF2Entity> _entityScratch = new List<LF2Entity>(128);
        private int _cameraX;
        private int _cameraVel;

        public int ReleaseCameraX => _cameraX;
        internal bool IsUnityFixedWorldCameraStateClear => _cameraX == 0 && _cameraVel == 0;
        internal int RuntimeSlotCapacity => _runtimeSlots.LogicalCapacity;
        internal int MaxRuntimeSlotsForServices => RuntimeSlotCapacity;
        internal int DynamicRuntimeSlotStartForServices => DynamicRuntimeSlotStart;
        internal BattleRuntimeProfile RuntimeProfileForServices => activeRuntimeProfile;
        internal CollisionBroadphaseBackend CollisionBroadphaseForServices { get; }
        internal int ClaimedRuntimeSlotCountForServices => _runtimeSlots.ClaimedCount;
        public int ClaimedRuntimeSlotCountForDiagnostics => _runtimeSlots.ClaimedCount;
        internal RuntimeRestStore RuntimeRestStoreForServices => _runtimeRestStore;

        private int GetRuntimeStableId(ISimObject obj)
        {
            return obj is LF2Entity entity ? entity.Runtime.StableId : obj.StableId;
        }

        private int GetRuntimeSlotOrder(LF2Entity entity)
        {
            if (entity == null) return int.MaxValue;
            int slot = entity.Runtime?.SlotIndex ?? -1;
            return slot >= 0 ? slot : entity.StableId;
        }

        private int CompareRuntimeSlotOrder(LF2Entity a, LF2Entity b)
        {
            int cmp = GetRuntimeSlotOrder(a).CompareTo(GetRuntimeSlotOrder(b));
            if (cmp != 0) return cmp;
            return (a?.StableId ?? int.MaxValue).CompareTo(b?.StableId ?? int.MaxValue);
        }

        private void RefreshRuntimeSnapshot(ISimObject obj)
        {
            if (obj is LF2Entity entity)
                entity.RefreshRuntimeSnapshot();
        }

        private List<int> GetBucketKeySnapshot()
        {
            return _buckets.Count > 0 ? new List<int>(_buckets.Keys) : null;
        }

        public ILF2SceneQuery SceneQuery { get; private set; }
        public INTSDItrKindService ItrKindService { get; private set; }
        public DeterministicRng Rng { get; private set; }
        public BattleRuntimeState Runtime { get; private set; }
        public int[] KillStats => Runtime.KillStats;
        public int[] DamageStats => Runtime.DamageStats;

        public SimulationWorld()
            : this(BattleRuntimeProfile.Authority400, AuthorityRuntimeSlotCapacity)
        {
        }

        internal SimulationWorld(
            BattleRuntimeProfile runtimeProfile,
            int runtimeSlotCapacity,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce)
        {
            if (runtimeSlotCapacity < DynamicRuntimeSlotStart)
                throw new System.ArgumentOutOfRangeException(nameof(runtimeSlotCapacity),
                    "Runtime slot capacity must include the dynamic slot band.");
            if (runtimeProfile == BattleRuntimeProfile.Authority400 &&
                runtimeSlotCapacity != AuthorityRuntimeSlotCapacity)
            {
                throw new System.ArgumentException(
                    "Authority400 worlds must use exactly 400 runtime slots.",
                    nameof(runtimeSlotCapacity));
            }

            activeRuntimeProfile = runtimeProfile;
            CollisionBroadphaseForServices = collisionBroadphase;
            maxActiveRuntimeEntities = runtimeProfile == BattleRuntimeProfile.MobileExtended
                ? BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities
                : int.MaxValue;
            _runtimeSlots = new RuntimeSlotTable(runtimeSlotCapacity, 20, DynamicRuntimeSlotStart);
            _runtimeRestStore = new RuntimeRestStore(runtimeSlotCapacity);
            aiInputSlots = new LF2Entity[runtimeSlotCapacity];
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this, collisionBroadphase);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
        }

        internal NTSDEntityRuntime GetRawRuntimeSlotState(int runtimeSlot)
        {
            return _runtimeSlots.GetRawRuntime(runtimeSlot);
        }

        internal bool TryGetCurrentRuntimeHandle(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return _runtimeSlots.TryGetCurrentHandle(runtimeSlot, expectedEntity, out handle);
        }

        internal bool TryResolveRuntimeHandle(RuntimeEntityHandle handle, out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        public bool TryGetCurrentRuntimeHandleForDiagnostics(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return _runtimeSlots.TryGetCurrentHandle(runtimeSlot, expectedEntity, out handle);
        }

        public bool TryResolveRuntimeHandleForDiagnostics(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        internal bool TryGetRuntimeSlotReadOnlyView(
            int runtimeSlot,
            out RuntimeSlotTable.ReadOnlySlotView view)
        {
            if (!_runtimeSlots.IsAddressable(runtimeSlot))
            {
                view = default;
                return false;
            }

            view = _runtimeSlots.GetReadOnlyView(runtimeSlot);
            return true;
        }

        private void ResetRawRuntimeSlotState(int runtimeSlot)
        {
            GetRawRuntimeSlotState(runtimeSlot)?.Reset();
        }

        public void ResetRuntimeState()
        {
            _battlePresentation.Reset();
            ResetRegisteredObjects();

            Runtime ??= new BattleRuntimeState();
            Runtime.Reset();
            // Unity lockstep owns one deterministic stream per SimulationWorld. The
            // explicit reset seed is an adapter boundary: it makes a world reset
            // replayable without sharing RNG state between independent Unity worlds.
            // It must remain distinct from MatchConfig.seed, which is applied by the
            // simulation driver at the formal battle-bootstrap boundary.
            Rng?.Seed(0x4E545344u);
            PendingSounds.Clear();
            _cameraX = 0;
            _cameraVel = 0;
            _nextAutoStableId = 100;
        }

        private void ResetRegisteredObjects()
        {
            (SceneQuery as BruteForceSceneQuery)?.ResetFormalSpatialBroadphase();

            var registeredObjects = new HashSet<ISimObject>();
            List<int> bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys != null)
            {
                for (int keyIndex = 0; keyIndex < bucketKeys.Count; keyIndex++)
                {
                    int key = bucketKeys[keyIndex];
                    if (!_buckets.TryGetValue(key, out Bucket bucket))
                        continue;

                    for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                    {
                        ISimObject item = bucket.items[itemIndex];
                        if (item != null)
                            registeredObjects.Add(item);
                    }
                }
            }

            _ticking = false;
            _pendingUnregister.Clear();
            _pendingSlotReleasedDestroy.Clear();
            _entityScratch.Clear();

            foreach (ISimObject item in registeredObjects)
            {
                item.OnRemoved(_context);
                if (item is not LF2Entity entity)
                    continue;

                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                    entity.Renderer);
                entity.ItrRest?.Unbind(false);
                entity.ItrRest?.Reset();
                entity.Reset();
                entity.Runtime?.Reset();
                entity.SetRuntimeSlotIndex(-1);
                entity.ClearRequiredRuntimeSlot();
                entity.FrameCache?.Clear();
                if (entity.Frame != null)
                {
                    entity.Frame.PN = 0;
                    entity.Frame.Prev = 0;
                    entity.Frame.N = 0;
                    entity.Frame.D = null;
                    entity.Frame.Prev2 = 0;
                    entity.Frame.Prev2D = null;
                }

                entity.Trans?.Reset();
                entity.Effect?.Reset();
                entity.Sprite?.SetPresentationSuppressed(true);
                entity.Sprite?.Hide();
                entity.Sprite?.HideShadow();
            }

            _buckets.Clear();
            _runtimeSlots.Reset();
            _runtimeRestStore.ResetWorld();
        }

        public int CurrentTickIndex => Runtime?.Flow?.CurrentTickIndex ?? 0;
        public int SparkRenderFrame => Runtime?.Flow?.SparkRenderFrame ?? 0;
        public int BattleGameModeId => Runtime?.Match?.BattleGameModeId ?? 0;
        public int LocalGameModeId => Runtime?.Match?.LocalGameModeId ?? 0;
        public int Difficulty => Runtime?.Match?.Difficulty ?? 2;
        public int BackgroundId => Runtime?.Match?.BackgroundId ?? -1;
        public int MatchSeed => Runtime?.Match?.Seed ?? 0;
        public int AiPhaseGate => Runtime?.Flow?.AiPhaseGate ?? 0;
        public int InputPhase => Runtime?.Flow?.InputPhase ?? 0;
        public int FrameMod12 => Runtime?.Flow?.FrameMod12 ?? 0;
        public int FrameToggle => Runtime?.Flow?.FrameToggle ?? 0;
        public int BattleExitCountdown => Runtime?.Flow?.BattleExitCountdown ?? 0;
        public int RouteOutRequest => Runtime?.Flow?.RouteOutRequest ?? 0;
        public int Mode2Request => Runtime?.Flow?.Mode2Request ?? 0;
        public bool NeedClearInput => Runtime?.Flow?.NeedClearInput ?? false;
        public List<BattleStageCampaignData> StageCampaigns => Runtime?.StageCampaigns;
        public BattleStageProgressionState StageProgression => Runtime?.StageProgression;
        public bool StageProgressionValid => Runtime?.StageProgressionValid ?? false;
        public int StageSpawnWaveApplied => Runtime?.StageSpawnWaveApplied ?? -1;
        public int StageSpawnWaveDeferredEntryApplied => Runtime?.StageSpawnWaveDeferredEntryApplied ?? -1;
        public int StageSpawnRuntimeWave => Runtime?.StageSpawnRuntimeWave ?? -1;
        public List<int> StageSpawnRuntimeTargetTotal => Runtime?.StageSpawnRuntimeTargetTotal;
        public List<int> StageSpawnRuntimeEntryCount => Runtime?.StageSpawnRuntimeEntryCount;
        public List<int> StageSpawnRuntimeSpawnedTotal => Runtime?.StageSpawnRuntimeSpawnedTotal;
        public List<int[]> StageSpawnRuntimeSlots => Runtime?.StageSpawnRuntimeSlots;

        public void SetAiPhaseGate(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.AiPhaseGate = value;
        }

        public void SetBattleExitCountdown(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.BattleExitCountdown = value;
        }

        public void SetRouteOutRequest(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.RouteOutRequest = value;
        }

        public void SetMode2Request(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.Mode2Request = value;
        }

        public void SetNeedClearInput(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.NeedClearInput = value;
        }

        public void AdvanceBattleFlowTick(int tickIndex)
        {
            if (Runtime?.Flow == null)
                return;

            Runtime.Flow.CurrentTickIndex = tickIndex;
            Runtime.Flow.InputPhase = (Runtime.Flow.InputPhase + 1) & 1;
            Runtime.Flow.FrameMod12 = tickIndex % 12;
            Runtime.Flow.FrameToggle = 1 - Runtime.Flow.FrameToggle;
        }

        public void SetStageProgressionValid(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageProgressionValid = value;
        }

        public void SetStageSpawnWaveApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveApplied = value;
        }

        public void SetStageSpawnWaveDeferredEntryApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveDeferredEntryApplied = value;
        }

        public void SetStageSpawnRuntimeWave(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnRuntimeWave = value;
        }

        public void Register(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

            // A pooled instance can be reused during the same dynamic late-slot scan.
            // Finalize its queued old lifecycle before registering the new one, and
            // remove the pending entry so the pass-finally flush cannot delete it.
            if (_pendingUnregister.Remove(obj))
                UnregisterImmediate(obj);

            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                bucket = new Bucket();
                _buckets[simOrder] = bucket;
            }

            if (bucket.items.Contains(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object already registered: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            if (obj is LF2Entity registeredEntity)
            {
                _pendingSlotReleasedDestroy.Remove(registeredEntity);
                registeredEntity.ItrRest?.Unbind(false);
                int runtimeSlot = AllocateRuntimeSlot(registeredEntity);
                registeredEntity.SetRuntimeSlotIndex(runtimeSlot);
                registeredEntity.ClearRequiredRuntimeSlot();
                if (runtimeSlot < 0)
                {
                    if (bucket.items.Count == 0)
                        _buckets.Remove(simOrder);
                    Debug.LogWarning(
                        $"[SimulationWorld] Runtime slot exhausted; registration rejected: " +
                        $"StableId={registeredEntity.StableId}, Type={registeredEntity.GetType().Name}");
                    return;
                }

                ResetRawRuntimeSlotState(runtimeSlot);
                if (registeredEntity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
                {
                    if (!ResetCooldownsForRuntimeSlot(runtimeSlot, registeredEntity))
                    {
                        RollbackRuntimeSlotRegistration(registeredEntity, runtimeSlot);
                        if (bucket.items.Count == 0)
                            _buckets.Remove(simOrder);
                        Debug.LogError(
                            $"[SimulationWorld] Runtime rest bind failed; registration rejected: " +
                            $"Slot={runtimeSlot}, StableId={registeredEntity.StableId}, " +
                            $"Type={registeredEntity.GetType().Name}");
                        return;
                    }
                }

                if (!registeredEntity.ShouldDeferInitialRuntimeSnapshot())
                    registeredEntity.RefreshRuntimeSnapshot();
            }

            bucket.items.Add(obj);
            bucket.dirty = true;
            obj.OnAdded(_context);
            if (obj is LF2Entity addedEntity &&
                TryGetCurrentRuntimeHandle(
                    addedEntity.Runtime.SlotIndex,
                    addedEntity,
                    out RuntimeEntityHandle runtimeHandle))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                    addedEntity.Renderer,
                    runtimeHandle);
            }
            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (_ticking)
            {
                if (obj is LF2Entity pendingEntity &&
                    !ReleaseRuntimeSlotAndClearPresentationBinding(pendingEntity))
                {
                    return;
                }
                if (!_pendingUnregister.Contains(obj))
                    _pendingUnregister.Add(obj);
                return;
            }

            UnregisterImmediate(obj);
        }

        private void UnregisterImmediate(ISimObject obj)
        {
            int bucketKey = obj.SimOrder;
            _buckets.TryGetValue(bucketKey, out Bucket bucket);
            if (bucket == null || !bucket.items.Contains(obj))
            {
                bucket = null;
                List<int> bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys != null)
                {
                    for (int i = 0; i < bucketKeys.Count; i++)
                    {
                        int candidateKey = bucketKeys[i];
                        if (!_buckets.TryGetValue(candidateKey, out Bucket candidateBucket) ||
                            !candidateBucket.items.Contains(obj))
                        {
                            continue;
                        }

                        bucketKey = candidateKey;
                        bucket = candidateBucket;
                        break;
                    }
                }
            }

            if (bucket == null)
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            if (obj is LF2Entity entity &&
                entity.Runtime?.SlotIndex >= 0 &&
                !ReleaseRuntimeSlotAndClearPresentationBinding(entity))
            {
                return;
            }

            if (!bucket.items.Remove(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;
            obj.OnRemoved(_context);

            if (bucket.items.Count == 0)
                _buckets.Remove(bucketKey);

            Debug.Log($"[SimulationWorld] Unregistered: SimOrder={bucketKey}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        private void FlushPendingUnregister()
        {
            if (_pendingUnregister.Count == 0) return;
            foreach (var obj in _pendingUnregister)
                UnregisterImmediate(obj);
            _pendingUnregister.Clear();
        }

        private void FlushPendingEntityDestroy()
        {
            // Pending entities are deliberately hidden from active pass queries. Scan the
            // runtime registry directly so the C# authority's late FreeEntity boundary still finalizes them.
            _entityScratch.Clear();
            for (int i = 0; i < _pendingSlotReleasedDestroy.Count; i++)
            {
                LF2Entity released = _pendingSlotReleasedDestroy[i];
                if (released != null && !_entityScratch.Contains(released))
                    _entityScratch.Add(released);
            }
            _pendingSlotReleasedDestroy.Clear();

            for (int runtimeSlot = 0; runtimeSlot < RuntimeSlotCapacity; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (entity?.Runtime != null &&
                    entity.Runtime.PendingFlushDestroy &&
                    !_entityScratch.Contains(entity))
                {
                    _entityScratch.Add(entity);
                }
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity.Runtime != null)
                    entity.Runtime.PendingFlushDestroy = false;

                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        private bool IsActiveForCurrentPass(ISimObject obj)
        {
            if (obj == null || _pendingUnregister.Contains(obj))
                return false;

            if (obj is LF2Entity entity && entity.Runtime != null)
            {
                if (entity.Runtime.OidMergeDormant)
                    return false;

                if (entity.Runtime.PendingFlushDestroy)
                    return false;
            }

            return true;
        }

        internal bool IsActiveForCurrentPassInternal(ISimObject obj)
        {
            return IsActiveForCurrentPass(obj);
        }

        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        private int AllocateRuntimeSlot(LF2Entity entity)
        {
            ReleasePendingDestroySlots();

            if (_runtimeSlots.ClaimedCount >= maxActiveRuntimeEntities)
                return -1;

            bool requiresDynamicSlot = entity.UsesDynamicRuntimeSlot();
            int requiredSlot = entity.RequiredRuntimeSlot;
            if (requiredSlot != -1)
            {
                if (requiredSlot >= RuntimeSlotCapacity &&
                    !TryGrowDesktopRuntimeSlots((long)requiredSlot + 1))
                {
                    return -1;
                }

                if (!_runtimeSlots.TryClaim(requiredSlot, entity, out _))
                    return -1;

                return requiredSlot;
            }

            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            bool existingSlotInRange = existingSlot >= 0 && existingSlot < RuntimeSlotCapacity;
            bool existingSlotInAllowedRange = !requiresDynamicSlot || existingSlot >= DynamicRuntimeSlotStart;
            int minimumExistingSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            if (existingSlotInRange && existingSlotInAllowedRange &&
                existingSlot >= minimumExistingSlot &&
                _runtimeSlots.TryClaim(existingSlot, entity, out _))
            {
                return existingSlot;
            }

            int startSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            int allocatedSlot = _runtimeSlots.AllocateLowest(startSlot, entity, out _);
            if (allocatedSlot >= 0 || !TryGrowDesktopRuntimeSlots((long)RuntimeSlotCapacity + 1))
                return allocatedSlot;

            return _runtimeSlots.AllocateLowest(startSlot, entity, out _);
        }

        private int FindFirstFreeRuntimeSlot(int startSlot, int endSlotExclusive)
        {
            ReleasePendingDestroySlots();

            if (_runtimeSlots.ClaimedCount >= maxActiveRuntimeEntities)
                return -1;

            bool scansCurrentTail = endSlotExclusive >= RuntimeSlotCapacity;
            int slot = _runtimeSlots.PeekLowest(startSlot, endSlotExclusive);
            if (slot >= 0 || !scansCurrentTail ||
                !TryGrowDesktopRuntimeSlots((long)RuntimeSlotCapacity + 1))
            {
                return slot;
            }

            return _runtimeSlots.PeekLowest(startSlot, RuntimeSlotCapacity);
        }

        private bool TryGrowDesktopRuntimeSlots(long minimumCapacity)
        {
            if (minimumCapacity <= RuntimeSlotCapacity)
                return true;
            if (activeRuntimeProfile != BattleRuntimeProfile.DesktopExtended ||
                minimumCapacity > int.MaxValue)
            {
                return false;
            }

            int normalizedCapacity;
            try
            {
                normalizedCapacity = BattleRuntimeProfilePolicy.NormalizeDesktopCapacity(
                    (int)minimumCapacity);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return false;
            }

            var grownAiInputSlots = new LF2Entity[normalizedCapacity];
            System.Array.Copy(aiInputSlots, grownAiInputSlots, aiInputSlots.Length);
            if (!_runtimeRestStore.GrowTo(normalizedCapacity) ||
                !_runtimeSlots.GrowTo(normalizedCapacity))
                return false;

            aiInputSlots = grownAiInputSlots;
            return true;
        }

        private void ReleasePendingDestroySlots()
        {
            List<int> bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null)
                return;

            for (int keyIndex = 0; keyIndex < bucketKeys.Count; keyIndex++)
            {
                int key = bucketKeys[keyIndex];
                if (!_buckets.TryGetValue(key, out Bucket bucket))
                    continue;

                for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                {
                    if (bucket.items[itemIndex] is not LF2Entity entity ||
                        entity.Runtime == null ||
                        !entity.Runtime.PendingFlushDestroy)
                    {
                        continue;
                    }

                    int slot = entity.Runtime.SlotIndex;
                    if (slot < 0 || slot >= RuntimeSlotCapacity)
                        continue;

                    if (object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(slot), entity) &&
                        ReleaseRuntimeSlotAndClearPresentationBinding(entity) &&
                        !_pendingSlotReleasedDestroy.Contains(entity))
                    {
                        _pendingSlotReleasedDestroy.Add(entity);
                    }
                }
            }
        }

        private bool ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot < 0)
                return true;
            if (slot >= RuntimeSlotCapacity ||
                !object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(slot), entity))
            {
                Debug.LogError(
                    $"[SimulationWorld] Refusing runtime slot release without the matching claim: " +
                    $"EntitySlot={slot}, StableId={entity.StableId}");
                return false;
            }

            bool wasBound = entity.ItrRest?.IsBound == true;
            if (wasBound && entity.ItrRest.BoundVictimSlot != slot)
            {
                Debug.LogError(
                    $"[SimulationWorld] Refusing runtime slot release with a mismatched rest binding: " +
                    $"EntitySlot={slot}, BoundVictimSlot={entity.ItrRest.BoundVictimSlot}, " +
                    $"StableId={entity.StableId}");
                return false;
            }
            if (wasBound && !entity.ItrRest.Unbind(false))
                return false;

            if (!_runtimeSlots.Release(slot, entity))
            {
                if (wasBound && !entity.ItrRest.Bind(_runtimeRestStore, slot, false))
                {
                    Debug.LogError(
                        $"[SimulationWorld] Failed to restore runtime rest binding after slot release rollback: " +
                        $"Slot={slot}, StableId={entity.StableId}");
                }
                return false;
            }

            entity.SetRuntimeSlotIndex(-1);
            return true;
        }

        private bool ReleaseRuntimeSlotAndClearPresentationBinding(LF2Entity entity)
        {
            NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                entity?.Renderer);
            if (ReleaseRuntimeSlot(entity))
                return true;

            int slot = entity?.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 &&
                TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle restoredHandle))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                    entity.Renderer,
                    restoredHandle);
            }

            return false;
        }

        private void RollbackRuntimeSlotRegistration(LF2Entity entity, int runtimeSlot)
        {
            entity?.ItrRest?.Unbind(false);
            if (entity != null &&
                object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(runtimeSlot), entity))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                    entity.Renderer);
                _runtimeSlots.Release(runtimeSlot, entity);
            }
            entity?.SetRuntimeSlotIndex(-1);
        }

        internal bool RestoreStageSpawnRestState(int runtimeSlot, LF2Entity entity)
        {
            if (!_runtimeSlots.IsAddressable(runtimeSlot) ||
                entity?.Runtime == null ||
                entity.Runtime.SlotIndex != runtimeSlot ||
                entity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
            {
                return false;
            }

            return entity.ItrRest != null &&
                   entity.ItrRest.Bind(_runtimeRestStore, runtimeSlot, false);
        }

        internal int GetRawRestArest(int runtimeSlot)
        {
            return _runtimeRestStore.GetARest(runtimeSlot);
        }

        internal int GetRawRestVrest(int victimSlot, int attackerSlot)
        {
            return _runtimeRestStore.GetVRest(victimSlot, attackerSlot);
        }

        public int ObjectCount
        {
            get
            {
                int count = 0;
                var bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys == null) return 0;

                foreach (int simOrder in bucketKeys)
                {
                    if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                    for (int i = 0; i < bucket.items.Count; i++)
                    {
                        ISimObject obj = bucket.items[i];
                        if (obj is LF2Entity entity)
                        {
                            if (_pendingUnregister.Contains(entity))
                                continue;

                            if (entity.Runtime != null &&
                                (entity.Runtime.OidMergeDormant || entity.Runtime.PendingFlushDestroy))
                                continue;
                        }

                        count++;
                    }
                }
                return count;
            }
        }

        public SimContext Context => _context;
    }
}


--- File: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs ---
using System;
using System.Collections.Generic;
using System.Threading;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Simulation.Presentation
{
    public enum BattleRenderCommandType : byte
    {
        Shadow = 0,
        Entity = 1,
        OverlayGlyph = 2,
        HitRecord = 3,
    }

    public enum BattlePresentationDifferenceKind : byte
    {
        None = 0,
        ExpectedMissing = 1,
        UnexpectedLegacy = 2,
        Category = 3,
        Identity = 4,
        Visual = 5,
        Position = 6,
        Size = 7,
        Flip = 8,
        SortOrder = 9,
        Color = 10,
        RenderState = 11,
        ResourceKey = 12,
    }

    public enum BattleOverlayParityState : byte
    {
        None = 0,
        AuthorityExpectedButLegacyMissing = 1,
    }

    public enum BattlePresentationParityStatus : byte
    {
        None = 0,
        PendingLegacyFrame = 1,
        Complete = 2,
        IncompleteLegacyFrame = 3,
    }

    public enum BattleSpriteMaterialSemantic : byte
    {
        Unsupported = 0,
        PremultipliedSpriteAlpha = 1,
    }

    public readonly struct BattleSpriteRenderState
    {
        public BattleSpriteRenderState(
            Color32 color,
            bool flipX,
            bool flipY,
            SpriteMaskInteraction maskInteraction,
            BattleSpriteMaterialSemantic materialSemantic)
        {
            Color = color;
            FlipX = flipX;
            FlipY = flipY;
            MaskInteraction = maskInteraction;
            MaterialSemantic = materialSemantic;
        }

        public Color32 Color { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }
        public SpriteMaskInteraction MaskInteraction { get; }
        public BattleSpriteMaterialSemantic MaterialSemantic { get; }
        public bool IsSupported =>
            MaskInteraction == SpriteMaskInteraction.None &&
            MaterialSemantic == BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;

        public static BattleSpriteRenderState Default(bool flipX = false)
        {
            return new BattleSpriteRenderState(
                new Color32(255, 255, 255, 255),
                flipX,
                false,
                SpriteMaskInteraction.None,
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
        }
    }

    public static class BattleSpriteMaterialContract
    {
        public const string BuiltInSpriteShaderName = "Sprites/Default";
        public const string CentralTextureShaderName = "NTSD/BattleCentralTransparent";
        public const string CentralArrayShaderName = "NTSD/BattleCentralTransparentArray";
        public const string AlphaContractTag = "NTSDAlphaContract";
        public const string PremultipliedAlphaContract = "PremultipliedSpriteAlpha";

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public static BattleSpriteMaterialSemantic Classify(Material material)
        {
            if (material == null || material.shader == null)
                return BattleSpriteMaterialSemantic.Unsupported;

            string shaderName = material.shader.name;
            if (shaderName != BuiltInSpriteShaderName &&
                shaderName != CentralTextureShaderName &&
                shaderName != CentralArrayShaderName)
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            if (shaderName != BuiltInSpriteShaderName &&
                material.GetTag(AlphaContractTag, false, string.Empty) != PremultipliedAlphaContract)
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            if (!material.HasProperty(ColorId) || !IsWhite(material.GetColor(ColorId)) ||
                material.IsKeywordEnabled("PIXELSNAP_ON"))
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            return BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
        }

        public static bool IsDeclaredCentralMaterial(Material material, bool textureArray)
        {
            if (material == null || material.shader == null)
                return false;
            string expectedShader = textureArray
                ? CentralArrayShaderName
                : CentralTextureShaderName;
            return material.shader.name == expectedShader &&
                   Classify(material) == BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
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

    public readonly struct BattleSpriteValueDescriptor
    {
        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized)
            : this(
                requiresSprite,
                hasSprite,
                spriteInstanceId,
                textureInstanceId,
                materialInstanceId,
                pixelRect,
                pivotNormalized,
                false,
                default(BattleSpriteKey))
        {
        }

        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized,
            bool hasLogicalResourceKey,
            BattleSpriteKey logicalResourceKey)
        {
            RequiresSprite = requiresSprite;
            HasSprite = hasSprite;
            SpriteInstanceId = spriteInstanceId;
            TextureInstanceId = textureInstanceId;
            MaterialInstanceId = materialInstanceId;
            PixelRect = pixelRect;
            PivotNormalized = pivotNormalized;
            HasLogicalResourceKey = hasLogicalResourceKey;
            LogicalResourceKey = BattleVisualResourceKey.FromEntity(logicalResourceKey);
        }

        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized,
            BattleVisualResourceKey logicalResourceKey)
        {
            RequiresSprite = requiresSprite;
            HasSprite = hasSprite;
            SpriteInstanceId = spriteInstanceId;
            TextureInstanceId = textureInstanceId;
            MaterialInstanceId = materialInstanceId;
            PixelRect = pixelRect;
            PivotNormalized = pivotNormalized;
            HasLogicalResourceKey = true;
            LogicalResourceKey = logicalResourceKey;
        }

        public bool RequiresSprite { get; }
        public bool HasSprite { get; }
        public int SpriteInstanceId { get; }
        public int TextureInstanceId { get; }
        public int MaterialInstanceId { get; }
        public Rect PixelRect { get; }
        public Vector2 PivotNormalized { get; }
        public bool HasLogicalResourceKey { get; }
        public BattleVisualResourceKey LogicalResourceKey { get; }
    }

    public readonly struct BattlePresentationHitRecordSnapshot
    {
        public BattlePresentationHitRecordSnapshot(int age, int anchorX, int anchorZ)
        {
            Age = age;
            AnchorX = anchorX;
            AnchorZ = anchorZ;
        }

        public int Age { get; }
        public int AnchorX { get; }
        public int AnchorZ { get; }
    }

    public readonly struct BattleHitRecordOwnerSnapshot
    {
        public BattleHitRecordOwnerSnapshot(
            RuntimeEntityHandle handle,
            int stableId,
            int zInt,
            int runtimeSlot,
            int presentationBaseOrder,
            float renderOffsetX,
            int cameraX,
            int hitRecordStart,
            int hitRecordCount)
        {
            Handle = handle;
            StableId = stableId;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            PresentationBaseOrder = presentationBaseOrder;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            HitRecordStart = hitRecordStart;
            HitRecordCount = hitRecordCount;
        }

        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int PresentationBaseOrder { get; }
        public float RenderOffsetX { get; }
        public int CameraX { get; }
        public int HitRecordStart { get; }
        public int HitRecordCount { get; }
    }

    public sealed class BattleHitRecordPresentationCycle
    {
        private BattleHitRecordOwnerSnapshot[] owners = new BattleHitRecordOwnerSnapshot[16];
        private BattlePresentationHitRecordSnapshot[] hitRecords =
            new BattlePresentationHitRecordSnapshot[16];
        private CharacterAnimtorManager bindingManager;
        private BattleSpriteCatalog boundCatalog = BattleSpriteCatalog.Empty;

        public int CycleId { get; private set; }
        public int TickIndex { get; private set; }
        public int OwnerCount { get; private set; }
        public int HitRecordCount { get; private set; }
        public BattleCommonVisualCatalog CommonVisualCatalog { get; private set; } =
            BattleCommonVisualCatalog.Empty;
        public bool HasValidSparkPublication => CommonVisualCatalog.IsSparkValid;

        public BattleHitRecordOwnerSnapshot GetOwner(int index)
        {
            if ((uint)index >= (uint)OwnerCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return owners[index];
        }

        public BattlePresentationHitRecordSnapshot GetHitRecord(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return hitRecords[index];
        }

        internal void Reset(
            int cycleId,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog)
        {
            ReleasePublicationBinding();
            CycleId = cycleId;
            TickIndex = tickIndex;
            OwnerCount = 0;
            HitRecordCount = 0;
            CommonVisualCatalog = commonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
        }

        internal void AddOwner(in BattleHitRecordOwnerSnapshot owner)
        {
            EnsureCapacity(ref owners, OwnerCount + 1);
            owners[OwnerCount++] = owner;
        }

        internal void AddHitRecord(in BattlePresentationHitRecordSnapshot hitRecord)
        {
            EnsureCapacity(ref hitRecords, HitRecordCount + 1);
            hitRecords[HitRecordCount++] = hitRecord;
        }

        internal void RetainPublicationBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog,
            BattleHitRecordPresentationCycle previousCycle)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (manager == null || ReferenceEquals(nextCatalog, BattleSpriteCatalog.Empty))
                return;

            if (previousCycle != null &&
                previousCycle.bindingManager == manager &&
                ReferenceEquals(previousCycle.boundCatalog, nextCatalog))
            {
                bindingManager = previousCycle.bindingManager;
                boundCatalog = previousCycle.boundCatalog;
                previousCycle.bindingManager = null;
                previousCycle.boundCatalog = BattleSpriteCatalog.Empty;
                return;
            }

            manager.RegisterRendererCatalogBinding(nextCatalog);
            bindingManager = manager;
            boundCatalog = nextCatalog;
        }

        internal void ReleasePublicationBinding()
        {
            CharacterAnimtorManager manager = bindingManager;
            BattleSpriteCatalog catalog = boundCatalog;
            bindingManager = null;
            boundCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        private static void EnsureCapacity<T>(ref T[] array, int required)
        {
            if (required <= array.Length)
                return;
            int next = array.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref array, next);
        }
    }

    public readonly struct BattlePresentationEntitySnapshot
    {
        public BattlePresentationEntitySnapshot(
            RuntimeEntityHandle handle,
            int stableId,
            int objectId,
            int currentDatObjectId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int presentationBaseOrder,
            int hitStop,
            bool hasCurrentFrame,
            int state,
            int linkState,
            int hp2Orig,
            int relationTeam,
            int currentDatObjType,
            int xInt,
            int yInt,
            float displayZ,
            float renderOffsetX,
            int cameraX,
            int frameDelay,
            float centerX,
            float centerY,
            float pixelWidth,
            float pixelHeight,
            Vector2 heldVisualAttachmentOffsetPixels,
            Rect normalizedUv,
            Vector2 pivot,
            bool flipX,
            bool hasCatalogKey,
            BattleSpriteValueDescriptor spriteDescriptor,
            int hitRecordStart,
            int hitRecordCount,
            bool entityVisible = true,
            bool shadowVisible = true,
            Vector2 localOffsetPixels = default(Vector2))
        {
            Handle = handle;
            StableId = stableId;
            ObjectId = objectId;
            CurrentDatObjectId = currentDatObjectId;
            EffectivePic = effectivePic;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            PresentationBaseOrder = presentationBaseOrder;
            HitStop = hitStop;
            HasCurrentFrame = hasCurrentFrame;
            State = state;
            LinkState = linkState;
            HP2Orig = hp2Orig;
            RelationTeam = relationTeam;
            CurrentDatObjType = currentDatObjType;
            XInt = xInt;
            YInt = yInt;
            DisplayZ = displayZ;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            FrameDelay = frameDelay;
            CenterX = centerX;
            CenterY = centerY;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            HeldVisualAttachmentOffsetPixels = heldVisualAttachmentOffsetPixels;
            NormalizedUv = normalizedUv;
            Pivot = pivot;
            FlipX = flipX;
            HasCatalogKey = hasCatalogKey;
            SpriteDescriptor = spriteDescriptor;
            HitRecordStart = hitRecordStart;
            HitRecordCount = hitRecordCount;
            EntityVisible = entityVisible;
            ShadowVisible = shadowVisible;
            LocalOffsetPixels = localOffsetPixels;
        }

        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int ObjectId { get; }
        public int CurrentDatObjectId { get; }
        public int VisualDataId => CurrentDatObjectId;
        public int EffectivePic { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int PresentationBaseOrder { get; }
        public int HitStop { get; }
        public bool HasCurrentFrame { get; }
        public int State { get; }
        public int LinkState { get; }
        public int HP2Orig { get; }
        public int RelationTeam { get; }
        public int CurrentDatObjType { get; }
        public int XInt { get; }
        public int YInt { get; }
        public float DisplayZ { get; }
        public float RenderOffsetX { get; }
        public int CameraX { get; }
        public int FrameDelay { get; }
        public float CenterX { get; }
        public float CenterY { get; }
        public float PixelWidth { get; }
        public float PixelHeight { get; }
        public Vector2 HeldVisualAttachmentOffsetPixels { get; }
        public Rect NormalizedUv { get; }
        public Vector2 Pivot { get; }
        public bool FlipX { get; }
        public bool HasCatalogKey { get; }
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
        public int HitRecordStart { get; }
        public int HitRecordCount { get; }
        public bool EntityVisible { get; }
        public bool ShadowVisible { get; }
        public Vector2 LocalOffsetPixels { get; }
    }

    public readonly struct BattleRenderCommand
    {
        public BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            bool flipX,
            BattleSpriteValueDescriptor spriteDescriptor)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                zInt,
                runtimeSlot,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                pivot,
                normalizedUv,
                BattleSpriteRenderState.Default(flipX),
                spriteDescriptor)
        {
        }

        public BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            BattleSpriteRenderState renderState,
            BattleSpriteValueDescriptor spriteDescriptor)
        {
            Type = type;
            Handle = handle;
            StableId = stableId;
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            SortOrder = sortOrder;
            SortingLayerId = sortingLayerId;
            LocalSequence = localSequence;
            Position = position;
            Size = size;
            Pivot = pivot;
            NormalizedUv = normalizedUv;
            RenderState = renderState;
            SpriteDescriptor = spriteDescriptor;
        }

        public BattleRenderCommandType Type { get; }
        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int VisualDataId { get; }
        public int EffectivePic { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int SortOrder { get; }
        public int SortingLayerId { get; }
        public int LocalSequence { get; }
        public Vector3 Position { get; }
        public Vector2 Size { get; }
        public Vector2 Pivot { get; }
        public Rect NormalizedUv { get; }
        public BattleSpriteRenderState RenderState { get; }
        public Color32 Color => RenderState.Color;
        public bool FlipX => RenderState.FlipX;
        public bool FlipY => RenderState.FlipY;
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
    }

    public sealed class BattlePresentationFrame
    {
        private BattlePresentationEntitySnapshot[] entities = new BattlePresentationEntitySnapshot[16];
        private BattlePresentationHitRecordSnapshot[] hitRecords = new BattlePresentationHitRecordSnapshot[16];
        private BattleRenderCommand[] commands = new BattleRenderCommand[64];
        private readonly char[,] slotLabelChars = new char[10, 12];
        private readonly int[] slotLabelState = new int[10];
        private CharacterAnimtorManager bindingManager;
        private BattleSpriteCatalog boundCatalog = BattleSpriteCatalog.Empty;

        public int TickIndex { get; internal set; }
        public int EntityCount { get; internal set; }
        public int HitRecordCount { get; internal set; }
        public int CommandCount { get; internal set; }
        public int OverlayUnsupportedCount { get; internal set; }
        public BattleCommonVisualCatalog CommonVisualCatalog { get; internal set; } =
            BattleCommonVisualCatalog.Empty;
        public BattleCommonVisualBinding CommonShadowBinding { get; internal set; }
        public string CommonShadowDiagnostic { get; internal set; } = string.Empty;
        public int EntityCapacity => entities.Length;
        public int HitRecordCapacity => hitRecords.Length;
        public int CommandCapacity => commands.Length;
        internal char[,] SlotLabelChars => slotLabelChars;
        internal int[] SlotLabelState => slotLabelState;
        public BattleSpriteCatalog BoundCatalogForAcceptance => boundCatalog;
        internal BattleSpriteCatalog BoundCatalog => boundCatalog;

        public BattlePresentationEntitySnapshot GetEntity(int index)
        {
            if ((uint)index >= (uint)EntityCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return entities[index];
        }

        public BattlePresentationHitRecordSnapshot GetHitRecord(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return hitRecords[index];
        }

        public BattleRenderCommand GetCommand(int index)
        {
            if ((uint)index >= (uint)CommandCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return commands[index];
        }

        internal void CopyFrom(BattlePresentationFrame source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (ReferenceEquals(this, source))
                return;

            ReleasePublicationBinding();
            EnsureEntityCapacity(source.EntityCount);
            EnsureHitRecordCapacity(source.HitRecordCount);
            EnsureCommandCapacity(source.CommandCount);
            Array.Copy(source.entities, entities, source.EntityCount);
            Array.Copy(source.hitRecords, hitRecords, source.HitRecordCount);
            Array.Copy(source.commands, commands, source.CommandCount);
            Array.Copy(source.slotLabelChars, slotLabelChars, source.slotLabelChars.Length);
            Array.Copy(source.slotLabelState, slotLabelState, source.slotLabelState.Length);

            TickIndex = source.TickIndex;
            EntityCount = source.EntityCount;
            HitRecordCount = source.HitRecordCount;
            CommandCount = source.CommandCount;
            OverlayUnsupportedCount = source.OverlayUnsupportedCount;
            CommonVisualCatalog = source.CommonVisualCatalog;
            CommonShadowBinding = source.CommonShadowBinding;
            CommonShadowDiagnostic = source.CommonShadowDiagnostic;
            // Submission catalog binding owns resource lifetime for frozen copies.
            boundCatalog = source.boundCatalog;
        }

        internal void Reset(
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog = null)
        {
            ReleasePublicationBinding();
            TickIndex = tickIndex;
            EntityCount = 0;
            HitRecordCount = 0;
            CommandCount = 0;
            OverlayUnsupportedCount = 0;
            Array.Clear(slotLabelChars, 0, slotLabelChars.Length);
            Array.Clear(slotLabelState, 0, slotLabelState.Length);
            CommonVisualCatalog = commonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
            CommonShadowBinding = commonVisualCatalog?.Shadow;
            CommonShadowDiagnostic = commonVisualCatalog?.Diagnostic ??
                                     BattleCommonVisualCatalog.Empty.Diagnostic;
        }

        internal void RetainPublicationBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog,
            BattlePresentationFrame previousFrame)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (manager == null || ReferenceEquals(nextCatalog, BattleSpriteCatalog.Empty))
                return;

            if (previousFrame != null &&
                previousFrame.bindingManager == manager &&
                ReferenceEquals(previousFrame.boundCatalog, nextCatalog))
            {
                bindingManager = previousFrame.bindingManager;
                boundCatalog = previousFrame.boundCatalog;
                previousFrame.bindingManager = null;
                previousFrame.boundCatalog = BattleSpriteCatalog.Empty;
                return;
            }

            manager.RegisterRendererCatalogBinding(nextCatalog);
            bindingManager = manager;
            boundCatalog = nextCatalog;
        }

        internal void ReleasePublicationBinding()
        {
            CharacterAnimtorManager manager = bindingManager;
            BattleSpriteCatalog catalog = boundCatalog;
            bindingManager = null;
            boundCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        internal void EnsureEntityCapacity(int required) => EnsureCapacity(ref entities, required);
        internal void EnsureHitRecordCapacity(int required) => EnsureCapacity(ref hitRecords, required);
        internal void EnsureCommandCapacity(int required) => EnsureCapacity(ref commands, required);

        internal void AddEntity(in BattlePresentationEntitySnapshot entity)
        {
            EnsureEntityCapacity(EntityCount + 1);
            entities[EntityCount++] = entity;
        }

        internal void AddHitRecord(in BattlePresentationHitRecordSnapshot hitRecord)
        {
            EnsureHitRecordCapacity(HitRecordCount + 1);
            hitRecords[HitRecordCount++] = hitRecord;
        }

        internal void AddCommand(in BattleRenderCommand command)
        {
            EnsureCommandCapacity(CommandCount + 1);
            commands[CommandCount++] = command;
        }

        private static void EnsureCapacity<T>(ref T[] array, int required)
        {
            if (required <= array.Length)
                return;

            int next = array.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref array, next);
        }
    }

    public readonly struct LegacyPresentationProbe
    {
        public LegacyPresentationProbe(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            bool flipX,
            BattleSpriteValueDescriptor spriteDescriptor)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                BattleSpriteRenderState.Default(flipX),
                spriteDescriptor)
        {
        }

        public LegacyPresentationProbe(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            BattleSpriteRenderState renderState,
            BattleSpriteValueDescriptor spriteDescriptor)
        {
            Type = type;
            Handle = handle;
            StableId = stableId;
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
            SortOrder = sortOrder;
            SortingLayerId = sortingLayerId;
            LocalSequence = localSequence;
            Position = position;
            Size = size;
            RenderState = renderState;
            SpriteDescriptor = spriteDescriptor;
        }

        public BattleRenderCommandType Type { get; }
        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int VisualDataId { get; }
        public int EffectivePic { get; }
        public int SortOrder { get; }
        public int SortingLayerId { get; }
        public int LocalSequence { get; }
        public Vector3 Position { get; }
        public Vector2 Size { get; }
        public BattleSpriteRenderState RenderState { get; }
        public Color32 Color => RenderState.Color;
        public bool FlipX => RenderState.FlipX;
        public bool FlipY => RenderState.FlipY;
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
    }

    public sealed class BattlePresentationParityDiagnostics
    {
        public BattlePresentationParityStatus Status { get; internal set; }
        public int TickIndex { get; internal set; }
        public int ExpectedCount { get; internal set; }
        public int ActualCount { get; internal set; }
        public int DifferenceCount { get; internal set; }
        public int FirstDifferenceIndex { get; internal set; } = -1;
        public BattlePresentationDifferenceKind FirstDifferenceKind { get; internal set; }
        public BattleOverlayParityState OverlayState { get; internal set; }
        public int OverlayUnsupportedCount { get; internal set; }
        public int IncompleteLegacyFrameCount { get; internal set; }
        public int FirstIncompleteLegacyTick { get; internal set; } = -1;
        public int LastIncompleteLegacyTick { get; internal set; } = -1;
        public int CompletedLegacyFrameCount { get; internal set; }
        public bool HasFirstExpectedCommand { get; internal set; }
        public BattleRenderCommand FirstExpectedCommand { get; internal set; }
        public bool HasFirstActualProbe { get; internal set; }
        public LegacyPresentationProbe FirstActualProbe { get; internal set; }
    }

    public sealed class BattlePresentationCoordinator
    {
        private static readonly Comparison<LF2Entity> EntityOrderComparison = CompareEntityOrder;
        private static readonly int ObjectSortingLayerId = SortingLayer.NameToID("Object");
        private readonly BattlePresentationFrame frameA = new BattlePresentationFrame();
        private readonly BattlePresentationFrame frameB = new BattlePresentationFrame();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleA =
            new BattleHitRecordPresentationCycle();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleB =
            new BattleHitRecordPresentationCycle();
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(128);
        private readonly BattleEntityOverlayGlyph[] overlayGlyphScratch =
            new BattleEntityOverlayGlyph[32];
        private LegacyPresentationProbe[] legacyProbes = new LegacyPresentationProbe[64];
        private BattlePresentationFrame publishedFrame;
        private BattleHitRecordPresentationCycle publishedHitRecordCycle;
        private BattlePresentationBackendMode mode;
        private int nextHitRecordCycleId;
        private int finalizedHitRecordCycleId;
        private int legacyProbeCount;
        private int probeSequence;
        private bool awaitingLegacyCompletion;

        public BattlePresentationCoordinator()
        {
            mode = BattlePresentationBackendMode.LegacyOnly;
            Diagnostics = new BattlePresentationParityDiagnostics();
        }

        public BattlePresentationBackendMode Mode => mode;
        public BattlePresentationFrame PublishedFrame => Volatile.Read(ref publishedFrame);
        public BattleHitRecordPresentationCycle PublishedHitRecordCycle =>
            Volatile.Read(ref publishedHitRecordCycle);
        public BattlePresentationParityDiagnostics Diagnostics { get; }
        public bool IsCapturingLegacyProbes => awaitingLegacyCompletion;
        internal int LastHitRecordOwnerLookupCount { get; private set; }

        public void SetMode(BattlePresentationBackendMode value)
        {
            BattlePresentationBackendResolver.ValidateAvailable(value);
            mode = value;
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                if (awaitingLegacyCompletion)
                    RecordIncompleteLegacyFrame();
                awaitingLegacyCompletion = false;
                legacyProbeCount = 0;
            }
        }

        public void BeginFrame(SimulationWorld world, int tickIndex)
        {
            if (world == null)
                return;

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleCommonVisualCatalog commonVisualCatalog =
                manager?.CommonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
            BattleHitRecordPresentationCycle previousCycle = PublishedHitRecordCycle;
            BattleHitRecordPresentationCycle writeCycle =
                ReferenceEquals(previousCycle, hitRecordCycleA)
                    ? hitRecordCycleB
                    : hitRecordCycleA;
            int cycleId = nextHitRecordCycleId == int.MaxValue ? 1 : nextHitRecordCycleId + 1;
            nextHitRecordCycleId = cycleId;
            CaptureHitRecordCycle(
                world,
                tickIndex,
                cycleId,
                commonVisualCatalog,
                writeCycle);
            if (writeCycle.HitRecordCount > 0 && commonVisualCatalog.IsSparkValid)
            {
                writeCycle.RetainPublicationBinding(
                    manager,
                    manager?.SpriteCatalog,
                    previousCycle);
            }
            Interlocked.Exchange(ref publishedHitRecordCycle, writeCycle);
            previousCycle?.ReleasePublicationBinding();

            // Legacy overlays consume the same immutable command snapshot, but the
            // central renderer still refuses to build or submit geometry in this mode.
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                CaptureBuildAndPublishFrame(world, tickIndex, commonVisualCatalog, writeCycle, manager);
                return;
            }

            if (mode == BattlePresentationBackendMode.CentralOnly)
            {
                CaptureBuildAndPublishFrame(world, tickIndex, commonVisualCatalog, writeCycle, manager);
                awaitingLegacyCompletion = false;
                legacyProbeCount = 0;
                return;
            }

            if (mode != BattlePresentationBackendMode.CentralShadowBuild)
                return;

            if (awaitingLegacyCompletion)
                RecordIncompleteLegacyFrame();

            CaptureBuildAndPublishFrame(world, tickIndex, commonVisualCatalog, writeCycle, manager);
            legacyProbeCount = 0;
            probeSequence = 0;
            awaitingLegacyCompletion = true;
            Diagnostics.Status = BattlePresentationParityStatus.PendingLegacyFrame;
            Diagnostics.TickIndex = tickIndex;
        }

        public bool FinalizePublishedHitRecordCycle(SimulationWorld world)
        {
            BattleHitRecordPresentationCycle cycle = PublishedHitRecordCycle;
            if (world == null || cycle == null || cycle.CycleId == finalizedHitRecordCycleId)
                return false;

            finalizedHitRecordCycleId = cycle.CycleId;
            if (!cycle.HasValidSparkPublication)
                return false;

            bool changed = false;
            try
            {
                for (int ownerIndex = 0; ownerIndex < cycle.OwnerCount; ownerIndex++)
                {
                    BattleHitRecordOwnerSnapshot owner = cycle.GetOwner(ownerIndex);
                    if (!world.TryResolveRuntimeHandle(owner.Handle, out LF2Entity entity) ||
                        entity == null || entity.HitRecordCount != owner.HitRecordCount)
                    {
                        continue;
                    }

                    bool sampleMatches = true;
                    for (int hitIndex = 0; hitIndex < owner.HitRecordCount; hitIndex++)
                    {
                        BattlePresentationHitRecordSnapshot hit = cycle.GetHitRecord(
                            owner.HitRecordStart + hitIndex);
                        if (entity.GetHitRecordAge(hitIndex) != hit.Age)
                        {
                            sampleMatches = false;
                            break;
                        }
                    }
                    if (!sampleMatches)
                        continue;

                    for (int hitIndex = 0; hitIndex < owner.HitRecordCount; hitIndex++)
                    {
                        BattlePresentationHitRecordSnapshot hit = cycle.GetHitRecord(
                            owner.HitRecordStart + hitIndex);
                        if (BattleCommonVisualCatalog.TryResolveSparkAge(hit.Age, out _))
                        {
                            entity.AdvanceHitRecordFromPresentation(hitIndex, hit.Age);
                            changed = true;
                        }
                        else if (hitIndex == owner.HitRecordCount - 1)
                        {
                            changed |= entity.RemoveHitRecordTailFromPresentation(
                                hitIndex,
                                owner.HitRecordCount,
                                hit.Age);
                        }
                    }
                }
            }
            finally
            {
                cycle.ReleasePublicationBinding();
            }

            return changed;
        }

        public void ReleaseResources()
        {
            frameA.ReleasePublicationBinding();
            frameB.ReleasePublicationBinding();
            hitRecordCycleA.ReleasePublicationBinding();
            hitRecordCycleB.ReleasePublicationBinding();
        }

        public void Reset()
        {
            ReleaseResources();
            Interlocked.Exchange(ref publishedFrame, null);
            Interlocked.Exchange(ref publishedHitRecordCycle, null);
            entityScratch.Clear();
            nextHitRecordCycleId = 0;
            finalizedHitRecordCycleId = 0;
            legacyProbeCount = 0;
            probeSequence = 0;
            awaitingLegacyCompletion = false;
            LastHitRecordOwnerLookupCount = 0;
            Diagnostics.Status = BattlePresentationParityStatus.None;
            Diagnostics.TickIndex = 0;
            Diagnostics.ExpectedCount = 0;
            Diagnostics.ActualCount = 0;
            Diagnostics.DifferenceCount = 0;
            Diagnostics.FirstDifferenceIndex = -1;
            Diagnostics.FirstDifferenceKind = BattlePresentationDifferenceKind.None;
            Diagnostics.OverlayState = BattleOverlayParityState.None;
            Diagnostics.OverlayUnsupportedCount = 0;
            Diagnostics.IncompleteLegacyFrameCount = 0;
            Diagnostics.FirstIncompleteLegacyTick = -1;
            Diagnostics.LastIncompleteLegacyTick = -1;
            Diagnostics.CompletedLegacyFrameCount = 0;
            Diagnostics.HasFirstExpectedCommand = false;
            Diagnostics.FirstExpectedCommand = default;
            Diagnostics.HasFirstActualProbe = false;
            Diagnostics.FirstActualProbe = default;
        }

        public void CompleteLegacyFrame()
        {
            if (!awaitingLegacyCompletion)
                return;

            awaitingLegacyCompletion = false;
            ComparePublishedFrameToLegacyProbes();
            Diagnostics.Status = BattlePresentationParityStatus.Complete;
            Diagnostics.CompletedLegacyFrameCount++;
        }

        private void RecordIncompleteLegacyFrame()
        {
            int incompleteTick = PublishedFrame?.TickIndex ?? Diagnostics.TickIndex;
            Diagnostics.Status = BattlePresentationParityStatus.IncompleteLegacyFrame;
            Diagnostics.IncompleteLegacyFrameCount++;
            if (Diagnostics.FirstIncompleteLegacyTick < 0)
                Diagnostics.FirstIncompleteLegacyTick = incompleteTick;
            Diagnostics.LastIncompleteLegacyTick = incompleteTick;
            awaitingLegacyCompletion = false;
            legacyProbeCount = 0;
        }

        internal void RecordLegacyProbe(in LegacyPresentationProbe probe)
        {
            if (!awaitingLegacyCompletion)
                return;

            EnsureLegacyProbeCapacity(legacyProbeCount + 1);
            legacyProbes[legacyProbeCount++] = new LegacyPresentationProbe(
                probe.Type,
                probe.Handle,
                probe.StableId,
                probe.VisualDataId,
                probe.EffectivePic,
                probe.SortOrder,
                probe.SortingLayerId,
                probeSequence++,
                probe.Position,
                probe.Size,
                probe.RenderState,
                probe.SpriteDescriptor);
        }

        internal void RecordLegacyHitRecordProbe(
            in BattleHitRecordOwnerSnapshot owner,
            SpriteRenderer renderer,
            int hitRecordIndex,
            BattleCommonVisualBinding binding)
        {
            if (!awaitingLegacyCompletion || renderer == null || !renderer.enabled)
                return;

            Sprite sprite = renderer.sprite;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = sprite != null && rect.width > 0f && rect.height > 0f
                ? new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height)
                : Vector2.zero;
            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer.sharedMaterial;
            var renderState = new BattleSpriteRenderState(
                renderer.color,
                renderer.flipX,
                renderer.flipY,
                renderer.maskInteraction,
                BattleSpriteMaterialContract.Classify(material));
            bool matchesPublishedBinding = binding?.MatchesSprite(sprite) == true;
            BattleSpriteValueDescriptor descriptor = matchesPublishedBinding
                ? new BattleSpriteValueDescriptor(
                    true,
                    true,
                    sprite.GetInstanceID(),
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    binding.Key)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
            RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.HitRecord,
                owner.Handle,
                owner.StableId,
                -1,
                -1,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                hitRecordIndex,
                renderer.transform.position,
                sprite != null ? sprite.rect.size : Vector2.zero,
                renderState,
                descriptor));
        }

        internal void RecordLegacyOverlayProbe(
            in BattleRenderCommand command,
            SpriteRenderer renderer,
            BattleCommonVisualBinding binding)
        {
            if (!awaitingLegacyCompletion || renderer == null || !renderer.enabled)
                return;

            Sprite sprite = renderer.sprite;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = sprite != null && rect.width > 0f && rect.height > 0f
                ? new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height)
                : Vector2.zero;
            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer.sharedMaterial;
            var renderState = new BattleSpriteRenderState(
                renderer.color,
                renderer.flipX,
                renderer.flipY,
                renderer.maskInteraction,
                BattleSpriteMaterialContract.Classify(material));
            bool matchesPublishedBinding = binding != null &&
                                         binding.Key == command.SpriteDescriptor.LogicalResourceKey &&
                                         binding.MatchesSprite(sprite);
            BattleSpriteValueDescriptor descriptor = matchesPublishedBinding
                ? new BattleSpriteValueDescriptor(
                    true,
                    true,
                    sprite.GetInstanceID(),
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    binding.Key)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
            RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.OverlayGlyph,
                command.Handle,
                command.StableId,
                command.VisualDataId,
                command.EffectivePic,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                command.LocalSequence,
                renderer.transform.position,
                sprite != null ? sprite.rect.size : Vector2.zero,
                renderState,
                descriptor));
        }

        internal void ResetLegacyProbesForSelfCheck()
        {
            if (!awaitingLegacyCompletion)
                return;
            legacyProbeCount = 0;
            probeSequence = 0;
        }

        private void CaptureHitRecordCycle(
            SimulationWorld world,
            int tickIndex,
            int cycleId,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle cycle)
        {
            cycle.Reset(cycleId, tickIndex, commonVisualCatalog);
            world.GetPresentationEntitiesNoAlloc(entityScratch);
            entityScratch.Sort(EntityOrderComparison);
            for (int index = 0; index < entityScratch.Count; index++)
            {
                LF2Entity entity = entityScratch[index];
                NTSDEntityRuntime runtime = entity?.Runtime;
                int slot = runtime?.SlotIndex ?? -1;
                if (entity == null || runtime == null || slot < 0 ||
                    runtime.OidMergeDormant || runtime.PendingFlushDestroy ||
                    tickIndex < runtime.FirstPresentationTick ||
                    !world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                {
                    continue;
                }

                int sampledCount = Math.Min(entity.HitRecordCount, LF2Entity.MaxHitRecordSlots);
                if (sampledCount <= 0)
                    continue;
                int hitRecordStart = cycle.HitRecordCount;
                for (int hitIndex = 0; hitIndex < sampledCount; hitIndex++)
                {
                    cycle.AddHitRecord(new BattlePresentationHitRecordSnapshot(
                        entity.GetHitRecordAge(hitIndex),
                        entity.GetHitRecordX(hitIndex),
                        entity.GetHitRecordZ(hitIndex)));
                }
                cycle.AddOwner(new BattleHitRecordOwnerSnapshot(
                    handle,
                    runtime.StableId,
                    runtime.ZInt,
                    slot,
                    entity.GetRenderSortingOrder() - SimulationWorld.PresentationEntitySubOrder,
                    entity.GetRenderOffsetX(),
                    world.ReleaseCameraX,
                    hitRecordStart,
                    sampledCount));
            }
            entityScratch.Clear();
        }

        private void CaptureBuildAndPublishFrame(
            SimulationWorld world,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle hitRecordCycle,
            CharacterAnimtorManager manager)
        {
            BattlePresentationFrame previousFrame = PublishedFrame;
            BattlePresentationFrame writeFrame = ReferenceEquals(previousFrame, frameA) ? frameB : frameA;
            CaptureAndBuild(world, tickIndex, commonVisualCatalog, hitRecordCycle, writeFrame);
            if (RequiresPublicationBinding(writeFrame))
            {
                writeFrame.RetainPublicationBinding(
                    manager,
                    manager?.SpriteCatalog,
                    previousFrame);
            }

            Interlocked.Exchange(ref publishedFrame, writeFrame);
            previousFrame?.ReleasePublicationBinding();
        }

        private static bool RequiresPublicationBinding(BattlePresentationFrame frame)
        {
            if (frame == null)
                return false;

            for (int commandIndex = 0; commandIndex < frame.CommandCount; commandIndex++)
            {
                BattleSpriteValueDescriptor descriptor = frame.GetCommand(commandIndex).SpriteDescriptor;
                if (descriptor.HasLogicalResourceKey && descriptor.HasSprite)
                    return true;
            }

            return false;
        }

        private void CaptureAndBuild(
            SimulationWorld world,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle hitRecordCycle,
            BattlePresentationFrame frame)
        {
            frame.Reset(tickIndex, commonVisualCatalog);
            Array.Copy(
                world.Runtime.SlotLabels.BattleSlotLabels,
                frame.SlotLabelChars,
                frame.SlotLabelChars.Length);
            Array.Copy(
                world.Runtime.SlotLabels.BattleSlotLabelState,
                frame.SlotLabelState,
                frame.SlotLabelState.Length);
            world.GetPresentationEntitiesNoAlloc(entityScratch);
            entityScratch.Sort(EntityOrderComparison);
            frame.EnsureEntityCapacity(entityScratch.Count);
            int hitRecordOwnerCursor = 0;
            LastHitRecordOwnerLookupCount = 0;

            for (int i = 0; i < entityScratch.Count; i++)
            {
                LF2Entity entity = entityScratch[i];
                NTSDEntityRuntime runtime = entity?.Runtime;
                int slot = runtime?.SlotIndex ?? -1;
                if (entity == null || runtime == null || slot < 0 ||
                    runtime.OidMergeDormant || runtime.PendingFlushDestroy ||
                    tickIndex < runtime.FirstPresentationTick ||
                    !world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                {
                    continue;
                }

                LF2FrameData currentFrame = entity.Frame?.D;
                int visualDataId = LF2Entity.ResolveCurrentDataObjectId(entity);
                int effectivePic = entity.GetRenderPicIndex();
                bool hasCatalogKey = entity.TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry);
                Sprite catalogSprite = entry?.LegacySprite;
                Texture2D catalogTexture = entry?.SharedTexture;
                var spriteDescriptor = new BattleSpriteValueDescriptor(
                    hasCatalogKey,
                    catalogSprite != null,
                    catalogSprite != null ? catalogSprite.GetInstanceID() : 0,
                    catalogTexture != null ? catalogTexture.GetInstanceID() : 0,
                    0,
                    entry?.PixelRect ?? Rect.zero,
                    entry?.Pivot ?? Vector2.zero,
                    hasCatalogKey,
                    hasCatalogKey ? entry.Key : default);
                int hitRecordStart = frame.HitRecordCount;
                int sourceHitRecordCount = 0;
                BattleHitRecordOwnerSnapshot hitRecordOwner = default;
                if (hitRecordOwnerCursor < hitRecordCycle.OwnerCount)
                {
                    LastHitRecordOwnerLookupCount++;
                    BattleHitRecordOwnerSnapshot candidate =
                        hitRecordCycle.GetOwner(hitRecordOwnerCursor);
                    if (candidate.Handle.Equals(handle))
                    {
                        hitRecordOwner = candidate;
                        sourceHitRecordCount = candidate.HitRecordCount;
                        hitRecordOwnerCursor++;
                    }
                }
                frame.EnsureHitRecordCapacity(frame.HitRecordCount + sourceHitRecordCount);
                for (int hitIndex = 0; hitIndex < sourceHitRecordCount; hitIndex++)
                {
                    frame.AddHitRecord(hitRecordCycle.GetHitRecord(
                        hitRecordOwner.HitRecordStart + hitIndex));
                }

                int holderSlot = runtime.HolderStableId;
                LF2Entity holder = world.FindEntityByRuntimeSlotForQuery(holderSlot);
                Vector2 heldVisualAttachmentOffsetPixels =
                    LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels(
                        runtime,
                        currentFrame,
                        holder,
                        NTSDRenderSpace.BattleVisualScale);
                LF2Sprite entitySprite = entity.Sprite;
                bool entityVisible = entitySprite?.EntityVisible ?? true;
                bool shadowVisible = entitySprite?.ShadowVisible ?? true;
                Vector2 localOffsetPixels = entitySprite?.LocalOffsetPixels ?? Vector2.zero;

                frame.AddEntity(new BattlePresentationEntitySnapshot(
                    handle,
                    runtime.StableId,
                    entity.ObjectId,
                    visualDataId,
                    effectivePic,
                    runtime.ZInt,
                    slot,
                    entity.GetRenderSortingOrder() - SimulationWorld.PresentationEntitySubOrder,
                    runtime.HitStop,
                    currentFrame != null,
                    currentFrame?.state ?? -1,
                    runtime.LinkState,
                    runtime.HP2Orig,
                    runtime.RelationTeam,
                    entity.GetCurrentDataObjectTypeForSimulation(),
                    entity.GetRuntimeXInt(),
                    entity.GetRuntimeYInt(),
                    entity.GetDisplayZ(),
                    entity.GetRenderOffsetX(),
                    world.ReleaseCameraX,
                    entity.FrameDelay,
                    currentFrame?.centerx ?? 0f,
                    currentFrame?.centery ?? 0f,
                    entry?.PixelWidth ?? 0f,
                    entry?.PixelHeight ?? 0f,
                    heldVisualAttachmentOffsetPixels,
                    entry?.NormalizedUv ?? Rect.zero,
                    entry?.Pivot ?? new Vector2(0.5f, 0f),
                    string.Equals(runtime.Dir, "left", StringComparison.Ordinal),
                    hasCatalogKey,
                    spriteDescriptor,
                    hitRecordStart,
                    sourceHitRecordCount,
                    entityVisible,
                    shadowVisible,
                    localOffsetPixels));
            }

            entityScratch.Clear();
            BuildCommands(frame);
        }

        private void BuildCommands(BattlePresentationFrame frame)
        {
            frame.EnsureCommandCapacity(Math.Max(16, frame.EntityCount * 8 + frame.HitRecordCount));
            for (int rank = 0; rank < frame.EntityCount; rank++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(rank);
                int baseOrder = entity.PresentationBaseOrder;
                int localSequence = 0;

                bool drawShadow = entity.ShadowVisible && entity.HasCurrentFrame &&
                                  entity.State != 3005 && entity.State != 9997 &&
                                  entity.LinkState >= 0 && entity.ObjectId != 223 &&
                                  entity.ObjectId != 224 && frame.CommonShadowBinding != null &&
                                  LF2ObjectRenderer.ShouldDrawShadowForHitStop(entity.HitStop);
                if (drawShadow)
                {
                    BattleCommonVisualBinding shadow = frame.CommonShadowBinding;
                    Vector3 shadowPosition = NTSDRenderSpace.ScreenPixelToWorld(
                        entity.XInt + (int)entity.RenderOffsetX - entity.CameraX,
                        entity.ZInt,
                        0f);
                    AddCommand(frame, new BattleRenderCommand(
                        BattleRenderCommandType.Shadow,
                        entity.Handle,
                        entity.StableId,
                        -1,
                        -1,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder,
                        ObjectSortingLayerId,
                        localSequence++,
                        NTSDRenderSpace.SnapWorldPosition(shadowPosition),
                        shadow.PixelSize,
                        shadow.Pivot,
                        shadow.NormalizedUv,
                        shadow.RenderState,
                        new BattleSpriteValueDescriptor(
                            true,
                            true,
                            shadow.SpriteInstanceId,
                            shadow.TextureInstanceId,
                            shadow.MaterialInstanceId,
                            shadow.PixelRect,
                            shadow.Pivot,
                            BattleVisualResourceKey.CommonShadow)));
                }

                bool drawEntity = entity.EntityVisible && entity.State >= 0 &&
                                  entity.EffectivePic != 999 &&
                                  entity.HasCatalogKey &&
                                  LF2ObjectRenderer.ShouldDrawEntityForHitStop(entity.HitStop);
                if (drawEntity)
                {
                    Vector2 pivotPixels = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                        entity.XInt,
                        entity.YInt,
                        entity.DisplayZ,
                        entity.RenderOffsetX,
                        entity.CameraX,
                        entity.FrameDelay,
                        frame.TickIndex,
                        entity.FlipX,
                        entity.PixelWidth,
                        entity.PixelHeight,
                        entity.CenterX,
                        entity.CenterY,
                        NTSDRenderSpace.BattleVisualScale);
                    pivotPixels += entity.HeldVisualAttachmentOffsetPixels;
                    pivotPixels += entity.LocalOffsetPixels * NTSDRenderSpace.BattleVisualScale;
                    Vector3 entityPosition = NTSDRenderSpace.ScreenPixelToWorld(pivotPixels.x, pivotPixels.y, 0f);
                    AddCommand(frame, new BattleRenderCommand(
                        BattleRenderCommandType.Entity,
                        entity.Handle,
                        entity.StableId,
                        entity.VisualDataId,
                        entity.EffectivePic,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder + 1,
                        ObjectSortingLayerId,
                        localSequence++,
                        entityPosition,
                        new Vector2(entity.PixelWidth, entity.PixelHeight),
                        entity.Pivot,
                        entity.NormalizedUv,
                        entity.FlipX,
                        entity.SpriteDescriptor));
                }

                if (entity.HasCurrentFrame)
                {
                    var overlayRuntimeSlot = new BattleEntityOverlayRuntimeSlot(
                        entity.RuntimeSlot,
                        entity.HP2Orig,
                        entity.RelationTeam,
                        entity.CurrentDatObjType,
                        entity.CurrentDatObjectId,
                        entity.HitStop,
                        entity.XInt,
                        entity.YInt,
                        entity.ZInt,
                        (int)entity.RenderOffsetX,
                        entity.CameraX,
                        (int)entity.CenterY);
                    if (BattleEntityOverlayLayout.TryBuild(
                            in overlayRuntimeSlot,
                            frame.SlotLabelChars,
                            frame.SlotLabelState,
                            overlayGlyphScratch,
                            out int overlayGlyphCount))
                    {
                        for (int glyphIndex = 0; glyphIndex < overlayGlyphCount; glyphIndex++)
                        {
                            BattleEntityOverlayGlyph glyph = overlayGlyphScratch[glyphIndex];
                            if (!frame.CommonVisualCatalog.TryGetWordGlyph(
                                    glyph.SheetIndex,
                                    glyph.CharCode,
                                    out BattleCommonVisualBinding binding))
                            {
                                continue;
                            }

                            Vector3 glyphPosition = NTSDRenderSpace.ScreenPixelToWorld(
                                glyph.PixelX,
                                glyph.PixelY,
                                0f);
                            AddCommand(frame, new BattleRenderCommand(
                                BattleRenderCommandType.OverlayGlyph,
                                entity.Handle,
                                entity.StableId,
                                glyph.SheetIndex,
                                glyph.CharCode,
                                entity.ZInt,
                                entity.RuntimeSlot,
                                baseOrder + 2,
                                ObjectSortingLayerId,
                                localSequence++,
                                glyphPosition,
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
                                    binding.Key)));
                        }
                    }
                }

                for (int hitIndex = 0; hitIndex < entity.HitRecordCount; hitIndex++)
                {
                    BattlePresentationHitRecordSnapshot hit = frame.GetHitRecord(
                        entity.HitRecordStart + hitIndex);
                    if (!TryResolveSparkFrame(
                            hit.Age,
                            out int pic,
                            out Vector2 size,
                            out Rect pixelRect))
                        continue;
                    if (!frame.CommonVisualCatalog.TryGetSpark(pic, out BattleCommonVisualBinding spark))
                        continue;

                    Vector3 hitPosition = NTSDRenderSpace.ScreenPixelToWorld(
                        hit.AnchorX + entity.RenderOffsetX - entity.CameraX,
                        hit.AnchorZ,
                        0f);
                    AddCommand(frame, new BattleRenderCommand(
                        BattleRenderCommandType.HitRecord,
                        entity.Handle,
                        entity.StableId,
                        -1,
                        pic,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder + 3,
                        ObjectSortingLayerId,
                        hitIndex,
                        hitPosition,
                        spark.PixelSize,
                        spark.Pivot,
                        spark.NormalizedUv,
                        spark.RenderState,
                        new BattleSpriteValueDescriptor(
                            true,
                            true,
                            spark.SpriteInstanceId,
                            spark.TextureInstanceId,
                            spark.MaterialInstanceId,
                            spark.PixelRect,
                            spark.Pivot,
                            spark.Key)));
                }
            }
        }

        internal static bool TryResolveSparkFrame(
            int age,
            out int pic,
            out Vector2 size,
            out Rect pixelRect)
        {
            if (!BattleCommonVisualCatalog.TryResolveSparkAge(age, out pic))
            {
                size = Vector2.zero;
                pixelRect = Rect.zero;
                return false;
            }

            pixelRect = BattleCommonVisualCatalog.GetSparkPixelRect(pic);
            size = pixelRect.size;
            return true;
        }

        internal static Rect GetAuthoritySparkPixelRect(int pic)
        {
            return BattleCommonVisualCatalog.GetSparkPixelRect(pic);
        }

        internal static Vector2 GetAuthoritySparkPivotNormalized(int pic)
        {
            return BattleCommonVisualCatalog.GetSparkPivotNormalized(pic);
        }

        private static void AddCommand(BattlePresentationFrame frame, in BattleRenderCommand command)
        {
            frame.AddCommand(command);
        }

        private void ComparePublishedFrameToLegacyProbes()
        {
            BattlePresentationFrame frame = PublishedFrame;
            Diagnostics.TickIndex = frame?.TickIndex ?? 0;
            Diagnostics.ExpectedCount = 0;
            Diagnostics.ActualCount = legacyProbeCount;
            Diagnostics.DifferenceCount = 0;
            Diagnostics.FirstDifferenceIndex = -1;
            Diagnostics.FirstDifferenceKind = BattlePresentationDifferenceKind.None;
            Diagnostics.HasFirstExpectedCommand = false;
            Diagnostics.FirstExpectedCommand = default;
            Diagnostics.HasFirstActualProbe = false;
            Diagnostics.FirstActualProbe = default;
            Diagnostics.OverlayUnsupportedCount = frame?.OverlayUnsupportedCount ?? 0;
            Diagnostics.OverlayState = BattleOverlayParityState.None;
            if (frame == null)
                return;

            SortLegacyProbes();
            int expectedIndex = 0;
            int actualIndex = 0;
            while (true)
            {
                bool hasExpected = expectedIndex < frame.CommandCount;
                bool hasActual = actualIndex < legacyProbeCount;
                if (!hasExpected && !hasActual)
                    break;

                int comparisonIndex = Diagnostics.ExpectedCount;
                if (!hasExpected)
                {
                    RegisterDifference(
                        comparisonIndex,
                        BattlePresentationDifferenceKind.UnexpectedLegacy,
                        default,
                        false,
                        legacyProbes[actualIndex],
                        true);
                    actualIndex++;
                    continue;
                }
                Diagnostics.ExpectedCount++;
                if (!hasActual)
                {
                    RegisterDifference(
                        comparisonIndex,
                        BattlePresentationDifferenceKind.ExpectedMissing,
                        frame.GetCommand(expectedIndex),
                        true,
                        default,
                        false);
                    expectedIndex++;
                    continue;
                }

                BattleRenderCommand expected = frame.GetCommand(expectedIndex++);
                LegacyPresentationProbe actual = legacyProbes[actualIndex++];
                BattlePresentationDifferenceKind difference = Compare(expected, actual);
                if (difference != BattlePresentationDifferenceKind.None)
                {
                    RegisterDifference(
                        comparisonIndex,
                        difference,
                        expected,
                        true,
                        actual,
                        true);
                }
            }
        }

        private static BattlePresentationDifferenceKind Compare(
            in BattleRenderCommand expected,
            in LegacyPresentationProbe actual)
        {
            if (expected.Type != actual.Type)
                return BattlePresentationDifferenceKind.Category;
            if (expected.Handle != actual.Handle || expected.StableId != actual.StableId)
                return BattlePresentationDifferenceKind.Identity;
            if (expected.SpriteDescriptor.RequiresSprite && !actual.SpriteDescriptor.HasSprite)
                return BattlePresentationDifferenceKind.Visual;
            if (expected.SpriteDescriptor.HasLogicalResourceKey &&
                (!actual.SpriteDescriptor.HasLogicalResourceKey ||
                 expected.SpriteDescriptor.LogicalResourceKey != actual.SpriteDescriptor.LogicalResourceKey))
            {
                return BattlePresentationDifferenceKind.ResourceKey;
            }
            Rect expectedRect = expected.SpriteDescriptor.PixelRect;
            Rect actualRect = actual.SpriteDescriptor.PixelRect;
            if (expectedRect.width > 0f && expectedRect.height > 0f &&
                ((expectedRect.position - actualRect.position).sqrMagnitude > 0.000001f ||
                 (expectedRect.size - actualRect.size).sqrMagnitude > 0.000001f))
            {
                return BattlePresentationDifferenceKind.Visual;
            }
            if (expectedRect.width > 0f && expectedRect.height > 0f &&
                (expected.SpriteDescriptor.PivotNormalized -
                 actual.SpriteDescriptor.PivotNormalized).sqrMagnitude > 0.000001f)
            {
                return BattlePresentationDifferenceKind.Visual;
            }
            if (expected.SortOrder != actual.SortOrder)
                return BattlePresentationDifferenceKind.SortOrder;
            if (expected.SortingLayerId != actual.SortingLayerId)
                return BattlePresentationDifferenceKind.SortOrder;
            if ((expected.Position - actual.Position).sqrMagnitude > 0.000001f)
                return BattlePresentationDifferenceKind.Position;
            if (expected.Size.sqrMagnitude > 0.000001f &&
                (expected.Size - actual.Size).sqrMagnitude > 0.000001f)
                return BattlePresentationDifferenceKind.Size;
            if (!expected.RenderState.IsSupported || !actual.RenderState.IsSupported ||
                expected.RenderState.MaterialSemantic != actual.RenderState.MaterialSemantic ||
                expected.RenderState.MaskInteraction != actual.RenderState.MaskInteraction)
            {
                return BattlePresentationDifferenceKind.RenderState;
            }
            if (!expected.Color.Equals(actual.Color))
                return BattlePresentationDifferenceKind.Color;
            if (expected.FlipX != actual.FlipX || expected.FlipY != actual.FlipY)
                return BattlePresentationDifferenceKind.Flip;
            return BattlePresentationDifferenceKind.None;
        }

        internal static BattlePresentationDifferenceKind CompareForSelfCheck(
            in BattleRenderCommand expected,
            in LegacyPresentationProbe actual)
        {
            return Compare(expected, actual);
        }

        private static bool HasOverlayGlyphCommands(BattlePresentationFrame frame)
        {
            if (frame == null)
                return false;

            for (int index = 0; index < frame.CommandCount; index++)
            {
                if (frame.GetCommand(index).Type == BattleRenderCommandType.OverlayGlyph)
                    return true;
            }

            return false;
        }

        private void RegisterDifference(
            int index,
            BattlePresentationDifferenceKind kind,
            in BattleRenderCommand expected,
            bool hasExpected,
            in LegacyPresentationProbe actual,
            bool hasActual)
        {
            Diagnostics.DifferenceCount++;
            if (Diagnostics.FirstDifferenceIndex >= 0)
                return;
            Diagnostics.FirstDifferenceIndex = index;
            Diagnostics.FirstDifferenceKind = kind;
            Diagnostics.HasFirstExpectedCommand = hasExpected;
            Diagnostics.FirstExpectedCommand = expected;
            Diagnostics.HasFirstActualProbe = hasActual;
            Diagnostics.FirstActualProbe = actual;
        }

        private void SortLegacyProbes()
        {
            for (int i = 1; i < legacyProbeCount; i++)
            {
                LegacyPresentationProbe current = legacyProbes[i];
                int j = i - 1;
                while (j >= 0 && CompareProbeOrder(current, legacyProbes[j]) < 0)
                {
                    legacyProbes[j + 1] = legacyProbes[j];
                    j--;
                }
                legacyProbes[j + 1] = current;
            }
        }

        private static int CompareProbeOrder(in LegacyPresentationProbe left, in LegacyPresentationProbe right)
        {
            int order = left.SortOrder.CompareTo(right.SortOrder);
            return order != 0 ? order : left.LocalSequence.CompareTo(right.LocalSequence);
        }

        private static int CompareEntityOrder(LF2Entity left, LF2Entity right)
        {
            int z = (left?.Runtime?.ZInt ?? int.MaxValue).CompareTo(right?.Runtime?.ZInt ?? int.MaxValue);
            return z != 0
                ? z
                : (left?.Runtime?.SlotIndex ?? int.MaxValue).CompareTo(right?.Runtime?.SlotIndex ?? int.MaxValue);
        }

        private void EnsureLegacyProbeCapacity(int required)
        {
            if (required <= legacyProbes.Length)
                return;
            int next = legacyProbes.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref legacyProbes, next);
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs ---
using System;
using System.Threading;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleCentralRuntimeDiagnostics
    {
        public BattlePresentationBackendMode RequestedMode { get; internal set; }
        public BattlePresentationBackendMode EffectivePixelMode { get; internal set; }
        public bool FeatureAvailable { get; internal set; }
        public bool MaterialAvailable { get; internal set; }
        public bool FrameAvailable { get; internal set; }
        public bool AllCategoryOwnershipReady { get; internal set; }
        public bool CommonShadowBindingReady { get; internal set; }
        public bool CommonSparkBindingReady { get; internal set; }
        public bool SubmissionReady { get; internal set; }
        public bool SubmittedPixelsLastFrame { get; internal set; }
        public int SubmissionCount { get; internal set; }
        public int LastSubmissionDrawCount { get; internal set; }
        public int SimulationTick { get; internal set; }
        public int DisplayTick { get; internal set; }
        public bool IsStale { get; internal set; }
        public string Reason { get; internal set; } = string.Empty;
        public string RefusalReason { get; internal set; } = string.Empty;
    }

    public static class BattleCentralRenderSystem
    {
        private const int RendererObservationMaxAgeFrames = 2;

        private static readonly BattleDynamicMeshBackend[] Backends =
        {
            new BattleDynamicMeshBackend(),
            new BattleDynamicMeshBackend(),
        };
        private static readonly BattleCentralSubmission[] SlotSubmissions =
        {
            new BattleCentralSubmission(Backends[0]),
            new BattleCentralSubmission(Backends[1]),
        };
        private static readonly BattleDynamicMeshBackend EmptyBackend = new BattleDynamicMeshBackend();
        private static readonly BattleCatalogCentralResourceResolver CatalogResolver =
            new BattleCatalogCentralResourceResolver();
        private static readonly BattleCatalogCentralResourceResolver DiagnosticCatalogResolver =
            new BattleCatalogCentralResourceResolver();
        private static readonly BattleCentralRuntimeDiagnostics RuntimeDiagnostics =
            new BattleCentralRuntimeDiagnostics();

        private static FeatureRegistration[] featureRegistrations = new FeatureRegistration[4];
        private static int featureRegistrationCount;
        private static BattleRenderFeature featureOwner;
        private static Material featureMaterial;
        private static Material featureArrayMaterial;
        private static BattleRenderFeature observedFeatureOwner;
        private static ScriptableRenderer observedRenderer;
        private static Camera observedWorldCamera;
        private static int observedUnityFrame = -1;
        private static BattlePresentationBackendMode requestedMode = BattlePresentationBackendMode.CentralOnly;
        private static BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks;
        private static BattleCentralDrawMode serializedDrawMode = BattleCentralDrawMode.OrderedChunks;
        private static BattleDrawPolicyDecision drawPolicyDecision = new BattleDrawPolicyDecision(
            BattleDrawPolicyMode.Auto,
            BattleCentralDrawMode.OrderedChunks,
            string.Empty);
        private static SimulationWorld publishedPlanWorld;
        private static int publishedPlanGeneration;
        private static BattleDynamicMeshBackend lastBuiltBackend = Backends[0];
        private static CharacterAnimtorManager diagnosticCatalogManager;
        private static BattleSpriteCatalog diagnosticCatalog = BattleSpriteCatalog.Empty;
        private static int nextGeneration;

        public static BattleDynamicMeshBackend MeshBackend => lastBuiltBackend;
        public static BattleCentralRuntimeDiagnostics Diagnostics => RuntimeDiagnostics;
        public static BattlePixelFramePlan CurrentPixelFramePlan
        {
            get
            {
                SimulationWorld world = Volatile.Read(ref publishedPlanWorld);
                BattlePixelFramePlan plan = world != null
                    ? world.CurrentPixelFramePlan
                    : default;
                return plan.IsValid && plan.Generation == Volatile.Read(ref publishedPlanGeneration)
                    ? plan
                    : default;
            }
        }
        internal static int RegisteredFeatureCount => featureRegistrationCount;
        internal static BattleRenderFeature RegisteredFeature => featureOwner;
        public static Material RegisteredFeatureMaterialForAcceptance => featureMaterial;
        public static Material RegisteredFeatureArrayMaterialForAcceptance => featureArrayMaterial;
        internal static Material RegisteredFeatureMaterial => featureMaterial;
        internal static Material RegisteredFeatureArrayMaterial => featureArrayMaterial;
        internal static BattleCentralDrawMode RegisteredFeatureDrawMode => drawMode;
        public static BattleDrawPolicyDecision DrawPolicyDecision => drawPolicyDecision;

        internal static void RegisterFeature(
            BattleRenderFeature owner,
            Material material,
            BattleCentralDrawMode mode)
        {
            RegisterFeature(owner, material, null, mode);
        }

        internal static void RegisterFeature(
            BattleRenderFeature owner,
            Material material,
            Material arrayMaterial,
            BattleCentralDrawMode mode)
        {
            if (owner == null)
                return;

            int existingIndex = FindRegistration(owner);
            if (existingIndex >= 0)
                RemoveRegistrationAt(existingIndex);
            EnsureRegistrationCapacity(featureRegistrationCount + 1);
            featureRegistrations[featureRegistrationCount++] =
                new FeatureRegistration(owner, material, arrayMaterial, mode);
            ApplyActiveRegistration();
        }

        internal static void UnregisterFeature(BattleRenderFeature owner)
        {
            int index = FindRegistration(owner);
            if (index < 0)
                return;
            RemoveRegistrationAt(index);
            ApplyActiveRegistration();
        }

        internal static void RecordFeatureCameraAvailability(
            BattleRenderFeature owner,
            ScriptableRenderer renderer,
            Camera camera,
            CameraRenderType renderType)
        {
            if (owner == null || owner != featureOwner || renderer == null ||
                !IsWorldRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera))
            {
                return;
            }

            observedFeatureOwner = owner;
            observedRenderer = renderer;
            observedWorldCamera = camera;
            observedUnityFrame = Time.frameCount;
        }

        public static BattlePixelFramePlan PrepareFrame(SimulationWorld world)
        {
            BattlePresentationBackendMode mode =
                world?.BattlePresentation?.Mode ?? BattlePresentationBackendMode.CentralOnly;
            BattlePresentationFrame frame = world?.BattlePresentation?.PublishedFrame;
            int simulationTick = frame?.TickIndex ?? world?.CurrentTickIndex ?? 0;
            BattlePixelFramePlan current = world != null ? world.CurrentPixelFramePlan : default;
            if (current.IsValid && ReferenceEquals(current.World, world) &&
                current.SimulationTick == simulationTick &&
                current.RequestedMode == mode && CurrentPixelFramePlan.Generation == current.Generation)
            {
                return current;
            }

            requestedMode = mode;
            ResetPerFrameDiagnostics(mode, frame != null);

            if (world == null)
            {
                return CommitCentralFailurePlan(
                    null,
                    simulationTick,
                    "SimulationWorld is unavailable.");
            }
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    simulationTick,
                    "LegacyOnly does not build or submit central geometry.");
            }

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleSpriteCatalog catalog = manager != null
                ? manager.SpriteCatalog
                : BattleSpriteCatalog.Empty;
            BattleCommonVisualCatalog commonVisualCatalog = manager != null
                ? manager.CommonVisualCatalog
                : BattleCommonVisualCatalog.Empty;
            RuntimeDiagnostics.CommonShadowBindingReady = commonVisualCatalog.IsShadowValid;
            RuntimeDiagnostics.CommonSparkBindingReady = commonVisualCatalog.IsSparkValid;

            if (!TryGetReusableBackend(out int backendIndex, out BattleDynamicMeshBackend stagingBackend))
            {
                const string reason =
                    "No central staging backend is available because the previous submission is still leased.";
                return mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(world, simulationTick, reason)
                    : CommitLegacyPlan(world, frame, mode, simulationTick, reason);
            }

            bool rendererReady = TryValidateActiveRenderer(out string rendererReason);
            bool frameReady = frame != null;
            bool commonReady = commonVisualCatalog.IsComplete;
            if (mode == BattlePresentationBackendMode.CentralOnly &&
                (!rendererReady || !frameReady || !commonReady))
            {
                string reason = !rendererReady
                    ? rendererReason
                    : !frameReady
                        ? "No current immutable presentation frame is available."
                        : "The common shadow, spark, or WORDS catalog is incomplete.";
                return CommitCentralFailurePlan(world, simulationTick, reason);
            }

            try
            {
                BattleCentralSubmission stagingSubmission = SlotSubmissions[backendIndex];
                BattlePresentationFrame buildFrame = frame != null
                    ? stagingSubmission.CaptureFrame(frame)
                    : null;
                BattleSpriteCatalog buildCatalog = buildFrame?.BoundCatalog ?? catalog;
                BattleCommonVisualCatalog buildCommonVisualCatalog =
                    buildFrame?.CommonVisualCatalog ?? commonVisualCatalog;
                CatalogResolver.Configure(
                    buildCatalog,
                    buildCommonVisualCatalog,
                    featureMaterial,
                    featureArrayMaterial);
                stagingBackend.Build(buildFrame, CatalogResolver, drawMode);
                lastBuiltBackend = stagingBackend;
            }
            catch (Exception exception)
            {
                stagingBackend.Clear();
                string reason =
                    $"Central geometry build failed: {exception.GetType().Name}: {exception.Message}";
                return mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(world, simulationTick, reason)
                    : CommitLegacyPlan(world, frame, mode, simulationTick, reason);
            }

            bool allCategoryOwnershipReady = frameReady && commonReady &&
                                             frame.OverlayUnsupportedCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedCategoryCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedRenderStateCount == 0 &&
                                             stagingBackend.Diagnostics.UnresolvedCommandCount == 0;
            RuntimeDiagnostics.AllCategoryOwnershipReady = allCategoryOwnershipReady;

            if (mode == BattlePresentationBackendMode.CentralShadowBuild)
            {
                BindDiagnosticCatalog(manager, stagingBackend.BuiltFrame?.BoundCatalog ?? catalog);
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    simulationTick,
                    "CentralShadowBuild builds diagnostics but fixes pixel ownership to Legacy.",
                    true);
            }

            if (!allCategoryOwnershipReady)
            {
                return CommitCentralFailurePlan(
                    world,
                    simulationTick,
                    BuildOwnershipRefusalReason(stagingBackend));
            }

            ReleaseDiagnosticCatalogBinding();
            int generation = NextGeneration();
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            BattlePresentationFrame capturedFrame = stagingBackend.BuiltFrame;
            submission.Publish(
                world,
                capturedFrame,
                simulationTick,
                generation,
                manager,
                capturedFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                capturedFrame,
                mode,
                BattlePixelFrameOwner.Central,
                simulationTick,
                simulationTick,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = true;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        public static bool CentralOnlyOwnsPixels(SimulationWorld world)
        {
            return world != null &&
                   world.BattlePresentation.Mode == BattlePresentationBackendMode.CentralOnly;
        }

        public static bool ShouldSuppressLegacyMaterializers(SimulationWorld world)
        {
            return CentralOnlyOwnsPixels(world);
        }

        public static bool ShouldUseCentralPixels(SimulationWorld world)
        {
            BattlePixelFramePlan plan = world != null ? world.CurrentPixelFramePlan : default;
            BattlePixelFramePlan globalPlan = CurrentPixelFramePlan;
            BattleCentralSubmission submission = plan.Submission;
            return plan.IsValid && globalPlan.IsValid && plan.Generation == globalPlan.Generation &&
                   ReferenceEquals(plan.World, world) &&
                   plan.Owner == BattlePixelFrameOwner.Central &&
                   plan.RequestedMode == BattlePresentationBackendMode.CentralOnly &&
                   submission != null &&
                   !submission.IsRetired && ReferenceEquals(submission.World, world) &&
                   ReferenceEquals(submission.CapturedFrame, plan.CapturedFrame) &&
                   submission.IsBackendBuildCurrent &&
                   submission.TickIndex == plan.DisplayTick &&
                   submission.Generation == plan.Generation;
        }

        internal static bool TryAcquireSubmission(
            Camera camera,
            CameraRenderType renderType,
            out BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            lease = default;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            SimulationWorld world = plan.World;
            if (!CanRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera) ||
                !ShouldUseCentralPixels(world))
            {
                return false;
            }

            if (!plan.Submission.TryAcquire(out lease))
                return false;
            if (ShouldUseCentralPixels(world) &&
                lease.Generation == plan.Generation && lease.TickIndex == plan.TickIndex)
            {
                return true;
            }

            lease.Dispose();
            lease = default;
            return false;
        }

        internal static bool IsSubmissionLeaseCurrent(
            BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            BattleCentralSubmission submission = lease.Submission;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            return submission != null && plan.IsValid &&
                   ReferenceEquals(plan.Submission, submission) &&
                   plan.Generation == lease.Generation && plan.TickIndex == lease.TickIndex &&
                   ShouldUseCentralPixels(plan.World);
        }

        internal static BattlePixelFramePlan PublishReadyCentralPlanForSelfCheck(
            SimulationWorld world)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            int tickIndex = frame?.TickIndex ?? 0;
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly || frame == null)
            {
                return world.BattlePresentation.Mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(
                        world,
                        tickIndex,
                        "Self-check central publication requires a current CentralOnly frame.")
                    : CommitLegacyPlan(
                        world,
                        frame,
                        world.BattlePresentation.Mode,
                        tickIndex,
                        "Self-check central publication requires a current CentralOnly frame.");
            }
            if (!TryGetReusableBackend(out int backendIndex, out BattleDynamicMeshBackend backend))
            {
                return CommitCentralFailurePlan(
                    world,
                    tickIndex,
                    "Self-check central publication found no reusable backend slot.");
            }

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            BattlePresentationFrame capturedFrame = submission.CaptureFrame(frame);
            CatalogResolver.Configure(
                capturedFrame.BoundCatalog,
                capturedFrame.CommonVisualCatalog,
                featureMaterial,
                featureArrayMaterial);
            backend.Build(capturedFrame, CatalogResolver, drawMode);
            lastBuiltBackend = backend;
            int generation = NextGeneration();
            submission.Publish(
                world,
                capturedFrame,
                tickIndex,
                generation,
                manager,
                capturedFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                capturedFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                tickIndex,
                tickIndex,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.FrameAvailable = true;
            RuntimeDiagnostics.AllCategoryOwnershipReady = true;
            RuntimeDiagnostics.SubmissionReady = true;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        internal static BattlePixelFramePlan PublishBuiltCentralPlanForSelfCheck(
            SimulationWorld world)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Built central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            BattlePresentationFrame builtFrame = lastBuiltBackend.BuiltFrame;
            if (frame == null || builtFrame == null || frame.TickIndex != builtFrame.TickIndex)
                throw new InvalidOperationException("The self-check requires the current immutable frame tick to be built.");

            int backendIndex = Array.IndexOf(Backends, lastBuiltBackend);
            if (backendIndex < 0)
                throw new InvalidOperationException("The built backend is not a publishable central slot.");
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            if (!submission.IsReusable)
                throw new InvalidOperationException("The built backend submission slot is still leased.");

            int generation = NextGeneration();
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            submission.Publish(
                world,
                builtFrame,
                builtFrame.TickIndex,
                generation,
                manager,
                builtFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                builtFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                builtFrame.TickIndex,
                builtFrame.TickIndex,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.FrameAvailable = true;
            RuntimeDiagnostics.AllCategoryOwnershipReady = true;
            RuntimeDiagnostics.SubmissionReady = true;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        internal static BattlePixelFramePlan PublishStaleCentralPlanForSelfCheck(
            SimulationWorld world,
            int simulationTick)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Stale central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            BattlePixelFramePlan current = world.CurrentPixelFramePlan;
            if (!current.IsValid || current.Owner != BattlePixelFrameOwner.Central ||
                current.Submission == null || current.Submission.IsRetired)
            {
                throw new InvalidOperationException("The self-check requires a live central submission.");
            }
            return CommitCentralFailurePlan(world, simulationTick, "Self-check retained last-good frame.");
        }

        public static bool CanRenderCamera(Camera camera, CameraRenderType renderType, Camera worldCamera)
        {
            return CanRenderCamera(
                camera,
                renderType,
                worldCamera,
                camera != null ? camera.cameraType : CameraType.Game,
                Application.isPlaying);
        }

        internal static bool CanRenderCamera(
            Camera camera,
            CameraRenderType renderType,
            Camera worldCamera,
            CameraType cameraType,
            bool isPlaying)
        {
            if (renderType != CameraRenderType.Base || camera == null || worldCamera == null)
                return false;
            if (camera == worldCamera)
                return true;
#if UNITY_EDITOR
            return isPlaying && cameraType == CameraType.SceneView;
#else
            return false;
#endif
        }

        private static bool IsWorldRenderCamera(
            Camera camera,
            CameraRenderType renderType,
            Camera worldCamera)
        {
            return camera != null && worldCamera != null && camera == worldCamera &&
                   renderType == CameraRenderType.Base;
        }

        internal static void RecordSubmission(
            BattleCentralSubmission.BattleCentralSubmissionLease lease,
            int drawCount)
        {
            if (!lease.IsValid)
                return;
            RecordSubmission(lease.Submission, lease.Generation, lease.TickIndex, drawCount);
        }

#if UNITY_EDITOR
        internal static void RecordSubmissionForSelfCheck(
            BattlePixelFramePlan plan,
            int drawCount)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Central submission recording self-check hook is editor-only.");
            BattlePixelFramePlan current = CurrentPixelFramePlan;
            if (!plan.IsValid || plan.Submission == null ||
                !current.IsValid || current.Generation != plan.Generation ||
                !ReferenceEquals(current.Submission, plan.Submission))
            {
                throw new InvalidOperationException(
                    "The self-check can record only the current central submission generation.");
            }
            RecordSubmission(plan.Submission, plan.Generation, plan.DisplayTick, drawCount);
        }
#endif

        private static void RecordSubmission(
            BattleCentralSubmission submission,
            int generation,
            int tickIndex,
            int drawCount)
        {
            if (submission == null ||
                !submission.TryRecordExecutedDraws(generation, tickIndex, drawCount))
            {
                return;
            }

            RuntimeDiagnostics.SubmissionCount += drawCount;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            if (!plan.IsValid || !ReferenceEquals(plan.Submission, submission) ||
                plan.Generation != generation || plan.DisplayTick != tickIndex)
            {
                return;
            }

            int executedDrawCount = submission.GetExecutedDrawCount(generation, tickIndex);
            RuntimeDiagnostics.SubmittedPixelsLastFrame = executedDrawCount > 0;
            RuntimeDiagnostics.LastSubmissionDrawCount = executedDrawCount;
        }

        public static BattleRenderingDiagnosticReport CaptureDiagnosticReport()
        {
            BattleAtlasDiagnosticInputs atlasInputs = CharacterAnimtorManager.Instance?.LastAtlasDiagnosticInputs;
            if (atlasInputs == null)
                return null;

            BattleCentralBuildDiagnostics build = lastBuiltBackend.Diagnostics;
            return new BattleRenderingDiagnosticReport(
                atlasInputs,
                drawPolicyDecision,
                build.SourceCommandCount,
                build.ResolvedCommandCount,
                build.UnresolvedCommandCount,
                build.UnsupportedCategoryCount,
                build.ActiveChunkCount,
                build.SegmentCount,
                RuntimeDiagnostics.LastSubmissionDrawCount,
                RuntimeDiagnostics.RequestedMode,
                RuntimeDiagnostics.EffectivePixelMode,
                CurrentPixelFramePlan.CapturedFrame?.EntityCount ?? 0);
        }

        public static BattleCentralEntityDiagnostic CaptureEntityDiagnostic(
            SimulationWorld world,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType = BattleRenderCommandType.Entity)
        {
            if (world == null || !handle.IsValid ||
                !world.TryGetRuntimeSlotReadOnlyView(handle.Slot, out RuntimeSlotTable.ReadOnlySlotView slotView))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle,
                    handle,
                    commandType);
            }
            if (!slotView.Claimed || slotView.Generation != handle.Generation)
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.GenerationMismatch,
                    handle,
                    commandType);
            }

            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            BattlePresentationFrame frame = plan.RequestedMode ==
                                                BattlePresentationBackendMode.CentralShadowBuild &&
                                            lastBuiltBackend.BuiltFrame != null
                ? lastBuiltBackend.BuiltFrame
                : plan.CapturedFrame ?? world.BattlePresentation.PublishedFrame;
            if (frame == null || !TryFindSnapshot(frame, handle, out BattlePresentationEntitySnapshot snapshot))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.MissingSnapshotEntity,
                    handle,
                    commandType);
            }

            if (!TryFindCommand(frame, handle, commandType, out int commandIndex, out BattleRenderCommand command))
            {
                BattleCentralEntityDiagnosticReason reason =
                    commandType == BattleRenderCommandType.Entity && !snapshot.EntityVisible ||
                    commandType == BattleRenderCommandType.Shadow && !snapshot.ShadowVisible
                        ? BattleCentralEntityDiagnosticReason.PresentationVisibilityFalse
                        : BattleCentralEntityDiagnosticReason.CommandSuppressed;
                return CreateEntityDiagnostic(reason, handle, commandType, snapshot, true);
            }

            if (!command.RenderState.IsSupported)
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.UnsupportedRenderState,
                    handle,
                    commandType,
                    snapshot,
                    true,
                    command,
                    true,
                    commandIndex);
            }

            BattleCentralEntityDiagnosticReason resourceReason = ResolveDiagnosticResource(
                frame,
                command,
                out BattleCentralResolvedResource resource);
            if (resourceReason != BattleCentralEntityDiagnosticReason.None)
            {
                return CreateEntityDiagnostic(
                    resourceReason,
                    handle,
                    commandType,
                    snapshot,
                    true,
                    command,
                    true,
                    commandIndex);
            }

            BattleDynamicMeshBackend backend = plan.Submission != null &&
                                                ReferenceEquals(plan.Submission.CapturedFrame, frame)
                ? plan.Submission.Backend
                : ReferenceEquals(lastBuiltBackend.BuiltFrame, frame)
                    ? lastBuiltBackend
                    : null;
            int segmentIndex = FindSegmentIndex(backend, commandIndex);
            int chunkIndex = segmentIndex >= 0 ? backend.GetSegment(segmentIndex).ChunkIndex : -1;
            bool backendBuildCurrent = plan.Submission == null ||
                                       plan.Submission.IsBackendBuildCurrent;
            bool submissionStructurallyCurrent = backendBuildCurrent &&
                                                 plan.Owner == BattlePixelFrameOwner.Central &&
                                                 plan.Submission != null &&
                                                 !plan.Submission.IsRetired &&
                                                 ReferenceEquals(plan.CapturedFrame, frame) &&
                                                 ReferenceEquals(plan.Submission.Backend, backend) &&
                                                 segmentIndex >= 0;
            bool submitted = submissionStructurallyCurrent &&
                             plan.Submission.GetExecutedDrawCount(
                                 plan.Generation,
                                 plan.DisplayTick) > 0;
            return CreateEntityDiagnostic(
                !backendBuildCurrent
                    ? BattleCentralEntityDiagnosticReason.BackendMutationMismatch
                    : !submitted
                        ? BattleCentralEntityDiagnosticReason.NotSubmitted
                        : plan.IsStale
                            ? BattleCentralEntityDiagnosticReason.StalePlan
                            : BattleCentralEntityDiagnosticReason.None,
                handle,
                commandType,
                snapshot,
                true,
                command,
                true,
                commandIndex,
                resource,
                true,
                segmentIndex,
                chunkIndex,
                submitted);
        }

#if UNITY_EDITOR
        internal static BattleCentralEntityDiagnosticReason CaptureResourceReasonForSelfCheck(
            BattlePresentationFrame frame,
            in BattleRenderCommand command)
        {
            if (!command.RenderState.IsSupported)
                return BattleCentralEntityDiagnosticReason.UnsupportedRenderState;
            return ResolveDiagnosticResource(frame, command, out _);
        }
#endif

        public static BattleCentralEntityDiagnostic CaptureEntityDiagnosticBySlot(
            SimulationWorld world,
            int runtimeSlot,
            BattleRenderCommandType commandType = BattleRenderCommandType.Entity)
        {
            if (world == null ||
                !world.TryGetRuntimeSlotReadOnlyView(runtimeSlot, out RuntimeSlotTable.ReadOnlySlotView view) ||
                !view.Claimed || view.Entity == null ||
                !world.TryGetCurrentRuntimeHandle(runtimeSlot, view.Entity, out RuntimeEntityHandle handle))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle,
                    RuntimeEntityHandle.Invalid,
                    commandType);
            }

            return CaptureEntityDiagnostic(world, handle, commandType);
        }

        private static BattleCentralEntityDiagnosticReason ResolveDiagnosticResource(
            BattlePresentationFrame frame,
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = default;
            if (!command.SpriteDescriptor.HasLogicalResourceKey)
                return BattleCentralEntityDiagnosticReason.MissingCatalogKey;

            if (command.Type == BattleRenderCommandType.Entity)
            {
                BattleVisualResourceKey logicalKey = command.SpriteDescriptor.LogicalResourceKey;
                if (!logicalKey.IsEntitySprite ||
                    !frame.BoundCatalog.TryGet(logicalKey.EntitySpriteKey, out BattleSpriteEntry entry) ||
                    entry.Key.VisualDataId != command.VisualDataId ||
                    entry.Key.EffectivePic != command.EffectivePic)
                {
                    return BattleCentralEntityDiagnosticReason.MissingCatalogKey;
                }

                BattleSpriteCentralBinding binding = entry.CentralBinding;
                if (binding.Texture == null)
                    return BattleCentralEntityDiagnosticReason.MissingTextureOrMaterial;
                if (!binding.IsValid)
                    return BattleCentralEntityDiagnosticReason.InvalidCentralBinding;
                Material material = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray
                    ? featureArrayMaterial
                    : featureMaterial;
                bool expectsArray = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray;
                if (!BattleSpriteMaterialContract.IsDeclaredCentralMaterial(material, expectsArray))
                    return BattleCentralEntityDiagnosticReason.MissingTextureOrMaterial;

                resource = new BattleCentralResolvedResource(
                    binding.Texture,
                    material,
                    binding.NormalizedUv,
                    new Vector2(entry.PixelWidth, entry.PixelHeight),
                    entry.Pivot,
                    command.Color,
                    (int)command.RenderState.MaterialSemantic,
                    binding.AtlasSlice,
                    binding.Mode);
                return BattleCentralEntityDiagnosticReason.None;
            }

            DiagnosticCatalogResolver.Configure(
                frame.BoundCatalog,
                frame.CommonVisualCatalog,
                featureMaterial,
                featureArrayMaterial);
            BattleCentralResourceStatus status = DiagnosticCatalogResolver.Resolve(command, out resource);
            return status switch
            {
                BattleCentralResourceStatus.Resolved => BattleCentralEntityDiagnosticReason.None,
                BattleCentralResourceStatus.UnsupportedRenderState =>
                    BattleCentralEntityDiagnosticReason.UnsupportedRenderState,
                BattleCentralResourceStatus.UnsupportedCategory =>
                    BattleCentralEntityDiagnosticReason.UnresolvedResource,
                _ => BattleCentralEntityDiagnosticReason.UnresolvedResource,
            };
        }

        private static bool TryFindSnapshot(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattlePresentationEntitySnapshot snapshot)
        {
            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot candidate = frame.GetEntity(index);
                if (candidate.Handle == handle)
                {
                    snapshot = candidate;
                    return true;
                }
            }

            snapshot = default;
            return false;
        }

        private static bool TryFindCommand(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType,
            out int commandIndex,
            out BattleRenderCommand command)
        {
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand candidate = frame.GetCommand(index);
                if (candidate.Handle == handle && candidate.Type == commandType)
                {
                    commandIndex = index;
                    command = candidate;
                    return true;
                }
            }

            commandIndex = -1;
            command = default;
            return false;
        }

        private static int FindSegmentIndex(BattleDynamicMeshBackend backend, int commandIndex)
        {
            if (backend == null)
                return -1;
            for (int index = 0; index < backend.SegmentCount; index++)
            {
                BattleCentralRenderSegment segment = backend.GetSegment(index);
                if (commandIndex >= segment.FirstCommandIndex &&
                    commandIndex < segment.FirstCommandIndex + segment.CommandCount)
                {
                    return index;
                }
            }

            return -1;
        }

        private static BattleCentralEntityDiagnostic CreateEntityDiagnostic(
            BattleCentralEntityDiagnosticReason reason,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType,
            BattlePresentationEntitySnapshot snapshot = default,
            bool hasSnapshot = false,
            BattleRenderCommand command = default,
            bool hasCommand = false,
            int commandIndex = -1,
            BattleCentralResolvedResource resource = default,
            bool hasResolvedResource = false,
            int segmentIndex = -1,
            int chunkIndex = -1,
            bool submitted = false)
        {
            return new BattleCentralEntityDiagnostic(
                reason,
                handle,
                commandType,
                snapshot,
                hasSnapshot,
                command,
                hasCommand,
                resource,
                hasResolvedResource,
                commandIndex,
                segmentIndex,
                chunkIndex,
                submitted);
        }

        internal static void ResolveDrawPolicyForPublication(
            GameConfig config,
            string[] commandLineArguments = null)
        {
            drawPolicyDecision = BattleRenderingPolicyResolver.ResolveDraw(
                config,
                serializedDrawMode,
                commandLineArguments);
            drawMode = drawPolicyDecision.EffectiveMode;
        }

        public static void ResetRuntime()
        {
            BattleCentralPresentationMountRegistry.ResetAllRuntimeBindings();
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            Volatile.Write(ref publishedPlanGeneration, 0);
            Volatile.Write(ref publishedPlanWorld, null);
            previous.Submission?.Retire();
            previous.World?.PublishPixelFramePlan(default);
            ReleaseDiagnosticCatalogBinding();
            for (int index = 0; index < Backends.Length; index++)
            {
                BattleCentralSubmission submission = SlotSubmissions[index];
                submission.Retire();
                if (submission.IsReusable)
                    Backends[index].Clear();
            }
            lastBuiltBackend = Backends[0];
            requestedMode = BattlePresentationBackendMode.CentralOnly;
            ResetPerFrameDiagnostics(BattlePresentationBackendMode.CentralOnly, false);
            RuntimeDiagnostics.RefusalReason = string.Empty;
        }

        private static BattlePixelFramePlan CommitLegacyPlan(
            SimulationWorld world,
            BattlePresentationFrame frame,
            BattlePresentationBackendMode mode,
            int tickIndex,
            string reason,
            bool preserveBuildDiagnostics = false)
        {
            if (!preserveBuildDiagnostics)
            {
                ReleaseDiagnosticCatalogBinding();
                EmptyBackend.Clear();
                lastBuiltBackend = EmptyBackend;
            }
            var plan = new BattlePixelFramePlan(
                world,
                frame,
                mode,
                BattlePixelFrameOwner.Legacy,
                tickIndex,
                tickIndex,
                NextGeneration(),
                false,
                reason,
                null);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.LegacyOnly;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = reason ?? string.Empty;
            return plan;
        }

        private static BattlePixelFramePlan CommitCentralFailurePlan(
            SimulationWorld world,
            int simulationTick,
            string reason)
        {
            ReleaseDiagnosticCatalogBinding();
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            BattleCentralSubmission submission = previous.IsValid &&
                                                   ReferenceEquals(previous.World, world) &&
                                                   previous.Owner == BattlePixelFrameOwner.Central &&
                                                   previous.Submission != null &&
                                                   !previous.Submission.IsRetired
                ? previous.Submission
                : null;
            BattlePresentationFrame displayFrame = submission?.CapturedFrame;
            int displayTick = submission?.TickIndex ?? -1;
            int generation = submission?.Generation ?? NextGeneration();
            var plan = new BattlePixelFramePlan(
                world,
                displayFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                simulationTick,
                displayTick,
                generation,
                true,
                reason,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = submission != null;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            SetPlanDiagnostics(plan);
            int retainedDrawCount = submission?.GetExecutedDrawCount(generation, displayTick) ?? 0;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = retainedDrawCount > 0;
            RuntimeDiagnostics.LastSubmissionDrawCount = retainedDrawCount;
            RuntimeDiagnostics.RefusalReason = reason ?? string.Empty;
            if (submission == null)
            {
                EmptyBackend.Clear();
                lastBuiltBackend = EmptyBackend;
            }
            return plan;
        }

        private static void PublishPlan(SimulationWorld world, BattlePixelFramePlan plan)
        {
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            world?.PublishPixelFramePlan(plan);
            Volatile.Write(ref publishedPlanWorld, world);
            Volatile.Write(ref publishedPlanGeneration, plan.Generation);
            if (previous.IsValid && !ReferenceEquals(previous.World, world))
                previous.World?.PublishPixelFramePlan(default);
            if (plan.Submission != null && !ReferenceEquals(previous.Submission, plan.Submission))
            {
                RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
                RuntimeDiagnostics.LastSubmissionDrawCount = 0;
            }
            if (!ReferenceEquals(previous.Submission, plan.Submission))
                previous.Submission?.Retire();
        }

        private static bool TryGetReusableBackend(
            out int backendIndex,
            out BattleDynamicMeshBackend backend)
        {
            BattleCentralSubmission currentSubmission = CurrentPixelFramePlan.Submission;
            for (int index = 0; index < Backends.Length; index++)
            {
                BattleCentralSubmission slotSubmission = SlotSubmissions[index];
                if (ReferenceEquals(slotSubmission, currentSubmission))
                    continue;
                if (!slotSubmission.IsReusable)
                    continue;

                backendIndex = index;
                backend = Backends[index];
                return true;
            }

            backendIndex = -1;
            backend = null;
            return false;
        }

        private static bool TryValidateActiveRenderer(out string reason)
        {
            Camera worldCamera = NTSDRenderSpace.WorldCamera;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable =
                BattleSpriteMaterialContract.IsDeclaredCentralMaterial(featureMaterial, false);
            if (featureOwner == null || !featureOwner.isActive)
            {
                reason = "BattleRenderFeature is not registered and active; CentralOnly output is fail-closed.";
                return false;
            }
            if (!RuntimeDiagnostics.MaterialAvailable)
            {
                reason = "The central battle material is missing or violates the declared alpha contract.";
                return false;
            }
            if (worldCamera == null || !worldCamera.enabled || !worldCamera.gameObject.activeInHierarchy)
            {
                reason = "The bound battle world camera is unavailable or disabled.";
                return false;
            }
            try
            {
                if (!worldCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData) ||
                    cameraData.scriptableRenderer == null ||
                    !ReferenceEquals(cameraData.scriptableRenderer, observedRenderer))
                {
                    reason = "The battle world camera is not using the renderer that invoked BattleRenderFeature.";
                    return false;
                }
            }
            catch (Exception exception)
            {
                reason = $"The battle world-camera renderer could not be validated: {exception.GetType().Name}.";
                return false;
            }
            int observationAge = observedUnityFrame < 0 ? int.MaxValue : Time.frameCount - observedUnityFrame;
            if (observedFeatureOwner != featureOwner || observedWorldCamera != worldCamera ||
                observationAge < 0 || observationAge > RendererObservationMaxAgeFrames)
            {
                reason = "The active world-camera renderer has not recently invoked the registered BattleRenderFeature.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private static string BuildOwnershipRefusalReason(BattleDynamicMeshBackend backend)
        {
            BattleCentralBuildDiagnostics diagnostics = backend.Diagnostics;
            return "Central frame ownership is incomplete: " +
                   $"unresolved={diagnostics.UnresolvedCommandCount}, " +
                   $"unsupportedCategory={diagnostics.UnsupportedCategoryCount}, " +
                   $"unsupportedState={diagnostics.UnsupportedRenderStateCount}.";
        }

        private static void BindDiagnosticCatalog(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (ReferenceEquals(diagnosticCatalogManager, manager) &&
                ReferenceEquals(diagnosticCatalog, nextCatalog))
            {
                return;
            }

            ReleaseDiagnosticCatalogBinding();
            diagnosticCatalogManager = manager;
            diagnosticCatalog = nextCatalog;
            diagnosticCatalogManager?.RegisterRendererCatalogBinding(diagnosticCatalog);
        }

        private static void ReleaseDiagnosticCatalogBinding()
        {
            CharacterAnimtorManager manager = diagnosticCatalogManager;
            BattleSpriteCatalog catalog = diagnosticCatalog;
            diagnosticCatalogManager = null;
            diagnosticCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        private static void ResetPerFrameDiagnostics(
            BattlePresentationBackendMode mode,
            bool frameAvailable)
        {
            RuntimeDiagnostics.RequestedMode = mode;
            RuntimeDiagnostics.EffectivePixelMode = mode == BattlePresentationBackendMode.CentralOnly
                ? BattlePresentationBackendMode.CentralOnly
                : BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;
            RuntimeDiagnostics.FrameAvailable = frameAvailable;
            RuntimeDiagnostics.AllCategoryOwnershipReady = false;
            RuntimeDiagnostics.CommonShadowBindingReady = false;
            RuntimeDiagnostics.CommonSparkBindingReady = false;
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
            RuntimeDiagnostics.LastSubmissionDrawCount = 0;
            RuntimeDiagnostics.SimulationTick = 0;
            RuntimeDiagnostics.DisplayTick = -1;
            RuntimeDiagnostics.IsStale = false;
            RuntimeDiagnostics.Reason = string.Empty;
            RuntimeDiagnostics.RefusalReason = string.Empty;
        }

        private static void SetPlanDiagnostics(BattlePixelFramePlan plan)
        {
            RuntimeDiagnostics.SimulationTick = plan.SimulationTick;
            RuntimeDiagnostics.DisplayTick = plan.DisplayTick;
            RuntimeDiagnostics.IsStale = plan.IsStale;
            RuntimeDiagnostics.Reason = plan.Reason;
        }

        private static int NextGeneration()
        {
            int generation = Interlocked.Increment(ref nextGeneration);
            if (generation > 0)
                return generation;
            Interlocked.Exchange(ref nextGeneration, 1);
            return 1;
        }

        private static int FindRegistration(BattleRenderFeature owner)
        {
            if (owner == null)
                return -1;
            for (int index = featureRegistrationCount - 1; index >= 0; index--)
            {
                if (featureRegistrations[index].Owner == owner)
                    return index;
            }
            return -1;
        }

        private static void RemoveRegistrationAt(int index)
        {
            for (int source = index + 1; source < featureRegistrationCount; source++)
                featureRegistrations[source - 1] = featureRegistrations[source];
            featureRegistrationCount--;
            featureRegistrations[featureRegistrationCount] = default;
        }

        private static void EnsureRegistrationCapacity(int required)
        {
            if (required <= featureRegistrations.Length)
                return;
            int next = featureRegistrations.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref featureRegistrations, next);
        }

        private static void ApplyActiveRegistration()
        {
            FeatureRegistration active = featureRegistrationCount > 0
                ? featureRegistrations[featureRegistrationCount - 1]
                : default;
            featureOwner = active.Owner;
            featureMaterial = active.Material;
            featureArrayMaterial = active.ArrayMaterial;
            serializedDrawMode = featureOwner != null
                ? active.DrawMode
                : BattleCentralDrawMode.OrderedChunks;
            drawPolicyDecision = featureOwner != null
                ? BattleRenderingPolicyResolver.ResolveDraw(GameConfig.Instance, serializedDrawMode)
                : new BattleDrawPolicyDecision(
                    BattleDrawPolicyMode.Auto,
                    BattleCentralDrawMode.OrderedChunks,
                    string.Empty);
            drawMode = drawPolicyDecision.EffectiveMode;
            observedFeatureOwner = null;
            observedRenderer = null;
            observedWorldCamera = null;
            observedUnityFrame = -1;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;
        }

        private readonly struct FeatureRegistration
        {
            public FeatureRegistration(
                BattleRenderFeature owner,
                Material material,
                Material arrayMaterial,
                BattleCentralDrawMode drawMode)
            {
                Owner = owner;
                Material = material;
                ArrayMaterial = arrayMaterial;
                DrawMode = drawMode;
            }

            public BattleRenderFeature Owner { get; }
            public Material Material { get; }
            public Material ArrayMaterial { get; }
            public BattleCentralDrawMode DrawMode { get; }
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralPresentationMountRegistry.cs ---
using System;
using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.Rendering
{
    /// <summary>
    /// Tracks active presentation mounts and assigns their generation-aware runtime identity.
    /// </summary>
    internal static class BattleCentralPresentationMountRegistry
    {
        private static readonly List<BattleCentralPresentationMount> ActiveMounts =
            new List<BattleCentralPresentationMount>(32);

        private static readonly Dictionary<int, OwnerRuntimeBinding> OwnerRuntimeBindings =
            new Dictionary<int, OwnerRuntimeBinding>(32);

        private static readonly List<int> StaleOwnerInstanceIds = new List<int>(8);

        internal static void Register(BattleCentralPresentationMount mount)
        {
            if (mount == null || ActiveMounts.Contains(mount))
                return;

            PruneDestroyedEntries();
            ActiveMounts.Add(mount);
            if (IsConfigurationValid(mount) &&
                TryGetOwnerRuntimeBinding(mount.OwnerRenderer, out RuntimeEntityHandle runtimeHandle))
            {
                mount.SetRuntimeHandle(runtimeHandle);
            }
        }

        internal static void Unregister(BattleCentralPresentationMount mount)
        {
            if (ReferenceEquals(mount, null))
                return;

            mount.SetRuntimeHandle(RuntimeEntityHandle.Invalid);
            for (int index = ActiveMounts.Count - 1; index >= 0; index--)
            {
                if (ReferenceEquals(ActiveMounts[index], mount))
                    ActiveMounts.RemoveAt(index);
            }
        }

        internal static void BindOwnerRuntime(
            LF2ObjectRenderer ownerRenderer,
            RuntimeEntityHandle runtimeHandle)
        {
            if (ownerRenderer == null)
                return;

            PruneDestroyedEntries();
            CacheOwnerRuntimeBinding(ownerRenderer, runtimeHandle);

            // EntityModel is mounted on the renderer itself. Bind it directly as well as
            // through ActiveMounts so a pool activation that misses OnEnable registration
            // cannot retain an invalid handle after the logic entity is registered.
            BattleCentralPresentationMount ownerMount =
                ownerRenderer.GetComponent<BattleCentralPresentationMount>();
            if (ownerMount != null)
            {
                ownerMount.SetRuntimeHandle(ownerMount.isActiveAndEnabled &&
                                            IsConfigurationValid(ownerMount)
                    ? runtimeHandle
                    : RuntimeEntityHandle.Invalid);
            }

            for (int index = 0; index < ActiveMounts.Count; index++)
            {
                BattleCentralPresentationMount mount = ActiveMounts[index];
                if (mount.OwnerRenderer == ownerRenderer)
                    mount.SetRuntimeHandle(IsConfigurationValid(mount)
                        ? runtimeHandle
                        : RuntimeEntityHandle.Invalid);
            }
        }

        internal static void ResetOwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer)
        {
            BindOwnerRuntime(ownerRenderer, RuntimeEntityHandle.Invalid);
        }

        internal static void RemoveOwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer)
        {
            if (ReferenceEquals(ownerRenderer, null))
                return;

            int instanceId = ownerRenderer.GetInstanceID();
            if (OwnerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) &&
                ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                OwnerRuntimeBindings.Remove(instanceId);
            }

            for (int index = ActiveMounts.Count - 1; index >= 0; index--)
            {
                BattleCentralPresentationMount mount = ActiveMounts[index];
                if (mount == null)
                {
                    ActiveMounts.RemoveAt(index);
                    continue;
                }

                if (ReferenceEquals(mount.OwnerRenderer, ownerRenderer))
                {
                    mount.SetRuntimeHandle(RuntimeEntityHandle.Invalid);
                }
            }
        }

        internal static int OwnerRuntimeBindingCountForSelfCheck => OwnerRuntimeBindings.Count;

        internal static bool HasOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return OwnerRuntimeBindings.ContainsKey(ownerInstanceId);
        }

        internal static bool HasOwnerRuntimeBindingForSelfCheck(int ownerInstanceId)
        {
            return HasOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        }

        internal static void ResetAllRuntimeBindings()
        {
            PruneDestroyedEntries();
            foreach (int ownerInstanceId in OwnerRuntimeBindings.Keys)
                OwnerRuntimeBindings[ownerInstanceId].Handle = RuntimeEntityHandle.Invalid;
            for (int index = 0; index < ActiveMounts.Count; index++)
                ActiveMounts[index].SetRuntimeHandle(RuntimeEntityHandle.Invalid);
        }

        internal static void ValidateActiveMounts()
        {
            if (!ValidateActiveMounts(out string error))
                throw new InvalidOperationException(error);
        }

        internal static bool ValidateActiveMounts(out string error)
        {
            PruneDestroyedEntries();
            var seenOwnerPurposes = new HashSet<OwnerPurposeKey>();
            for (int index = 0; index < ActiveMounts.Count; index++)
            {
                BattleCentralPresentationMount mount = ActiveMounts[index];
                if (!IsConfigurationValid(mount))
                {
                    error = DescribeInvalidConfiguration(mount);
                    return false;
                }

                var key = new OwnerPurposeKey(mount.OwnerRenderer, mount.Purpose);
                if (!seenOwnerPurposes.Add(key))
                {
                    error = Describe(mount, "duplicates another active owner/purpose mount.");
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool IsRolePurposePairValid(
            BattleCentralPresentationMountRole role,
            BattleCentralPresentationMountPurpose purpose)
        {
            return (role == BattleCentralPresentationMountRole.EntityModel &&
                    purpose == BattleCentralPresentationMountPurpose.EntitySprite) ||
                   (role == BattleCentralPresentationMountRole.Shadow &&
                    purpose == BattleCentralPresentationMountPurpose.CommonShadow);
        }

        private static bool IsMountedOnExpectedNode(BattleCentralPresentationMount mount)
        {
            if (mount.Role == BattleCentralPresentationMountRole.EntityModel)
                return mount.gameObject == mount.OwnerRenderer.gameObject;

            return mount.transform.parent != null &&
                   mount.transform.parent == mount.OwnerRenderer.transform.parent;
        }

        private static bool IsConfigurationValid(BattleCentralPresentationMount mount)
        {
            return mount != null && mount.OwnerRenderer != null &&
                   IsRolePurposePairValid(mount.Role, mount.Purpose) &&
                   IsMountedOnExpectedNode(mount);
        }

        private static string DescribeInvalidConfiguration(BattleCentralPresentationMount mount)
        {
            if (mount.OwnerRenderer == null)
                return Describe(mount, "has no owner LF2ObjectRenderer.");
            if (!IsRolePurposePairValid(mount.Role, mount.Purpose))
                return Describe(mount, "has an invalid role/purpose pairing.");
            return Describe(mount, "is mounted on the wrong node for its role.");
        }

        private static string Describe(BattleCentralPresentationMount mount, string detail)
        {
            string name = mount != null ? mount.name : "<destroyed mount>";
            return $"Battle central presentation mount '{name}' {detail}";
        }

        private static void CacheOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer,
            RuntimeEntityHandle runtimeHandle)
        {
            int instanceId = ownerRenderer.GetInstanceID();
            if (OwnerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) &&
                ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                binding.Handle = runtimeHandle;
                return;
            }

            OwnerRuntimeBindings[instanceId] = new OwnerRuntimeBinding(ownerRenderer, runtimeHandle);
        }

        private static bool TryGetOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer,
            out RuntimeEntityHandle runtimeHandle)
        {
            runtimeHandle = RuntimeEntityHandle.Invalid;
            if (ownerRenderer == null)
                return false;

            int instanceId = ownerRenderer.GetInstanceID();
            if (!OwnerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) ||
                !ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                return false;
            }

            runtimeHandle = binding.Handle;
            return runtimeHandle.IsValid;
        }

        private static void PruneDestroyedEntries()
        {
            for (int index = ActiveMounts.Count - 1; index >= 0; index--)
            {
                if (ActiveMounts[index] == null)
                    ActiveMounts.RemoveAt(index);
            }

            StaleOwnerInstanceIds.Clear();
            foreach (KeyValuePair<int, OwnerRuntimeBinding> pair in OwnerRuntimeBindings)
            {
                if (pair.Value.OwnerRenderer == null)
                    StaleOwnerInstanceIds.Add(pair.Key);
            }

            for (int index = 0; index < StaleOwnerInstanceIds.Count; index++)
                OwnerRuntimeBindings.Remove(StaleOwnerInstanceIds[index]);
        }

        private sealed class OwnerRuntimeBinding
        {
            public OwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer, RuntimeEntityHandle handle)
            {
                OwnerRenderer = ownerRenderer;
                Handle = handle;
            }

            public LF2ObjectRenderer OwnerRenderer { get; }
            public RuntimeEntityHandle Handle { get; set; }
        }

        private readonly struct OwnerPurposeKey : IEquatable<OwnerPurposeKey>
        {
            private readonly LF2ObjectRenderer ownerRenderer;
            private readonly BattleCentralPresentationMountPurpose purpose;

            public OwnerPurposeKey(
                LF2ObjectRenderer ownerRenderer,
                BattleCentralPresentationMountPurpose purpose)
            {
                this.ownerRenderer = ownerRenderer;
                this.purpose = purpose;
            }

            public bool Equals(OwnerPurposeKey other)
            {
                return ownerRenderer == other.ownerRenderer && purpose == other.purpose;
            }

            public override bool Equals(object obj)
            {
                return obj is OwnerPurposeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((ownerRenderer != null ? ownerRenderer.GetInstanceID() : 0) * 397) ^
                           (int)purpose;
                }
            }
        }
    }

    /// <summary>Read-only bridge for editor acceptance evidence.</summary>
    public static class BattleCentralPresentationMountDiagnostics
    {
        public static bool HasOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return BattleCentralPresentationMountRegistry
                .HasOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        }
    }
}


--- File: Temp/P8-C-PostFix-EditMode/P8-C-report.json ---
{
    "schema": "ntsd-battle-rendering-acceptance-v1",
    "passed": true,
    "deterministicSeed": 1314149188,
    "imageSize": 256,
    "livePoolRequested": false,
    "syntheticFixtureEvidenceScope": "synthetic fixture only: generation, isolated expansion, atlas, ordering, chunk, missing-resource, and baseline parity cases",
    "generationReuse": {
        "name": "pool-reuse-1000-generation",
        "passed": true,
        "available": true,
        "evidence": "slot=7; iterations=1000; finalClaimGeneration=1999; releasedGeneration=2000; oldHandlesRejected=True; finalResource=B; centerRGBA=19,150,60,165",
        "sourceCount": 1000,
        "resolvedCount": 1,
        "segmentCount": 1,
        "chunkCount": 1,
        "nonTransparentPixels": 624,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "isolatedPoolExpansion": {
        "name": "isolated-presentation-expansion-beyond-prewarm",
        "passed": true,
        "available": true,
        "evidence": "prewarmBaseline=16; expandedCount=33; uniqueOwners=33; uniqueHandles=33; visibleCenterSamples=33; rendererlessMountPairs=true",
        "sourceCount": 33,
        "resolvedCount": 33,
        "segmentCount": 33,
        "chunkCount": 1,
        "nonTransparentPixels": 5464,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "livePoolExpansion": {
        "name": "live-production-pool-world-publication-expansion",
        "passed": true,
        "available": false,
        "evidence": "not requested; use the request flag in Play Mode for production LF2ObjectPool evidence",
        "sourceCount": 0,
        "resolvedCount": 0,
        "segmentCount": 0,
        "chunkCount": 0,
        "nonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "productionCatalogPixelParity": {
        "name": "production-character-weapon-source-central-pixel-parity",
        "passed": true,
        "available": false,
        "evidence": "not requested; production catalog evidence requires a loaded Play Mode battle world",
        "sourceCount": 0,
        "resolvedCount": 0,
        "segmentCount": 0,
        "chunkCount": 0,
        "nonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "atlasArrayAndOrderedPages": {
        "name": "texture-array-uv-and-ordered-pages-fallback",
        "passed": true,
        "available": true,
        "evidence": "arraySlice=0,1; asymmetricUV=0..0.75,0.25..1; arraySegments=1; orderedPageSegments=2; fallbackReason=Forced TextureArray was refused by the capability gate: Texture2DArray sampling is unsupported.",
        "sourceCount": 2,
        "resolvedCount": 2,
        "segmentCount": 2,
        "chunkCount": 1,
        "nonTransparentPixels": 8550,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "transparentResourceInterleave": {
        "name": "transparent-a-b-a-resource-interleave",
        "passed": true,
        "available": true,
        "evidence": "segmentCommandStarts=0,1,2; textureOrder=A,B,A; overlapping alpha pixels rendered",
        "sourceCount": 3,
        "resolvedCount": 3,
        "segmentCount": 3,
        "chunkCount": 1,
        "nonTransparentPixels": 13516,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "categoryOcclusionOrder": {
        "name": "shadow-entity-overlay-hitrecord-order",
        "passed": true,
        "available": true,
        "evidence": "orderedTypes=Shadow,Entity,OverlayGlyph,HitRecord; sortOrder=100,101,102,103; sampleRGBA=center:40/115/240/255,overlay:245/210/35/255,entity:220/45/55/255,shadow:25/35/45/255",
        "sourceCount": 4,
        "resolvedCount": 4,
        "segmentCount": 4,
        "chunkCount": 1,
        "nonTransparentPixels": 19044,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "chunkBoundaries": {
        "name": "mesh-chunk-4095-4096-4097",
        "passed": true,
        "available": true,
        "evidence": "boundaries=4095:1chunk,4096:1chunk,4097:2chunks; UInt16; stress pixels >= command count",
        "sourceCount": 4097,
        "resolvedCount": 4097,
        "segmentCount": 2,
        "chunkCount": 2,
        "nonTransparentPixels": 15049,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "missingResourceFailClosed": {
        "name": "missing-resource-fail-closed",
        "passed": true,
        "available": true,
        "evidence": "missing visual resolved=0; unresolved=1; chunks=0; segments=0; transparent output",
        "sourceCount": 1,
        "resolvedCount": 0,
        "segmentCount": 0,
        "chunkCount": 0,
        "nonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "legacyCentralPixelParity": {
        "name": "rendererless-frozen-frame-legacy-central-pixel-parity",
        "passed": true,
        "available": true,
        "evidence": "frozenTick=8001; immutableCommands=4; legacySpriteRenderers=4; legacyPixels=18758; centralPixels=18758",
        "sourceCount": 4,
        "resolvedCount": 4,
        "segmentCount": 4,
        "chunkCount": 1,
        "nonTransparentPixels": 18758,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "legacyPng": "P8-C-legacy.png",
    "centralPng": "P8-C-central.png",
    "parityDiffPng": "P8-C-parity-diff.png",
    "generationReusePng": "P8-C-generation-reuse.png",
    "isolatedExpansionPng": "P8-C-isolated-expansion-33.png",
    "arrayPng": "P8-C-array.png",
    "orderedPagesPng": "P8-C-ordered-pages.png",
    "atlasDiffPng": "P8-C-atlas-diff.png",
    "interleavePng": "P8-C-aba-interleave.png",
    "categoryOcclusionPng": "P8-C-category-occlusion.png",
    "chunk4097Png": "P8-C-chunk-4097.png",
    "liveProductionPng": "P8-C-live-production.png",
    "productionCharacterLegacyPng": "P8-C-production-character-legacy.png",
    "productionCharacterCentralPng": "P8-C-production-character-central.png",
    "productionCharacterDiffPng": "P8-C-production-character-diff.png",
    "productionWeaponLegacyPng": "P8-C-production-weapon-legacy.png",
    "productionWeaponCentralPng": "P8-C-production-weapon-central.png",
    "productionWeaponDiffPng": "P8-C-production-weapon-diff.png",
    "skillInputOpointCovered": false,
    "livePoolScope": "not requested",
    "livePoolEntities": [],
    "productionCharacterResource": {
        "category": "",
        "available": false,
        "passed": false,
        "objectId": 0,
        "currentDatObjectType": 0,
        "runtimeSlot": 0,
        "runtimeGeneration": 0,
        "effectivePic": 0,
        "resourceKey": "",
        "sourceSheetPath": "",
        "sourceTextureName": "",
        "centralTextureName": "",
        "bindingMode": "",
        "atlasSlice": 0,
        "pixelRect": "",
        "normalizedUv": "",
        "pivot": "",
        "legacyNonTransparentPixels": 0,
        "centralNonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0,
        "evidence": ""
    },
    "productionWeaponResource": {
        "category": "",
        "available": false,
        "passed": false,
        "objectId": 0,
        "currentDatObjectType": 0,
        "runtimeSlot": 0,
        "runtimeGeneration": 0,
        "effectivePic": 0,
        "resourceKey": "",
        "sourceSheetPath": "",
        "sourceTextureName": "",
        "centralTextureName": "",
        "bindingMode": "",
        "atlasSlice": 0,
        "pixelRect": "",
        "normalizedUv": "",
        "pivot": "",
        "legacyNonTransparentPixels": 0,
        "centralNonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0,
        "evidence": ""
    }
}

--- File: Temp/P8-C-PostFix-RequestedUnavailable/P8-C-report.json ---
{
    "schema": "ntsd-battle-rendering-acceptance-v1",
    "passed": false,
    "deterministicSeed": 1314149188,
    "imageSize": 128,
    "livePoolRequested": true,
    "syntheticFixtureEvidenceScope": "synthetic fixture only: generation, isolated expansion, atlas, ordering, chunk, missing-resource, and baseline parity cases",
    "generationReuse": {
        "name": "pool-reuse-1000-generation",
        "passed": true,
        "available": true,
        "evidence": "slot=7; iterations=1000; finalClaimGeneration=1999; releasedGeneration=2000; oldHandlesRejected=True; finalResource=B; centerRGBA=20,149,61,165",
        "sourceCount": 1000,
        "resolvedCount": 1,
        "segmentCount": 1,
        "chunkCount": 1,
        "nonTransparentPixels": 180,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "isolatedPoolExpansion": {
        "name": "isolated-presentation-expansion-beyond-prewarm",
        "passed": true,
        "available": true,
        "evidence": "prewarmBaseline=16; expandedCount=33; uniqueOwners=33; uniqueHandles=33; visibleCenterSamples=33; rendererlessMountPairs=true",
        "sourceCount": 33,
        "resolvedCount": 33,
        "segmentCount": 33,
        "chunkCount": 1,
        "nonTransparentPixels": 1368,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "livePoolExpansion": {
        "name": "live-production-pool-world-publication-expansion",
        "passed": false,
        "available": false,
        "evidence": "requested but unavailable outside Play Mode",
        "sourceCount": 0,
        "resolvedCount": 0,
        "segmentCount": 0,
        "chunkCount": 0,
        "nonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "productionCatalogPixelParity": {
        "name": "production-character-weapon-source-central-pixel-parity",
        "passed": false,
        "available": false,
        "evidence": "requested but unavailable outside Play Mode",
        "sourceCount": 0,
        "resolvedCount": 0,
        "segmentCount": 0,
        "chunkCount": 0,
        "nonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "atlasArrayAndOrderedPages": {
        "name": "texture-array-uv-and-ordered-pages-fallback",
        "passed": true,
        "available": true,
        "evidence": "arraySlice=0,1; asymmetricUV=0..0.75,0.25..1; arraySegments=1; orderedPageSegments=2; fallbackReason=Forced TextureArray was refused by the capability gate: Texture2DArray sampling is unsupported.",
        "sourceCount": 2,
        "resolvedCount": 2,
        "segmentCount": 2,
        "chunkCount": 1,
        "nonTransparentPixels": 2132,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "transparentResourceInterleave": {
        "name": "transparent-a-b-a-resource-interleave",
        "passed": true,
        "available": true,
        "evidence": "segmentCommandStarts=0,1,2; textureOrder=A,B,A; overlapping alpha pixels rendered",
        "sourceCount": 3,
        "resolvedCount": 3,
        "segmentCount": 3,
        "chunkCount": 1,
        "nonTransparentPixels": 3418,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "categoryOcclusionOrder": {
        "name": "shadow-entity-overlay-hitrecord-order",
        "passed": true,
        "available": true,
        "evidence": "orderedTypes=Shadow,Entity,OverlayGlyph,HitRecord; sortOrder=100,101,102,103; sampleRGBA=center:40/115/240/255,overlay:245/210/35/255,entity:220/45/55/255,shadow:25/35/45/255",
        "sourceCount": 4,
        "resolvedCount": 4,
        "segmentCount": 4,
        "chunkCount": 1,
        "nonTransparentPixels": 4900,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "chunkBoundaries": {
        "name": "mesh-chunk-4095-4096-4097",
        "passed": true,
        "available": true,
        "evidence": "boundaries=4095:1chunk,4096:1chunk,4097:2chunks; UInt16; stress pixels >= command count",
        "sourceCount": 4097,
        "resolvedCount": 4097,
        "segmentCount": 2,
        "chunkCount": 2,
        "nonTransparentPixels": 4734,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "missingResourceFailClosed": {
        "name": "missing-resource-fail-closed",
        "passed": true,
        "available": true,
        "evidence": "missing visual resolved=0; unresolved=1; chunks=0; segments=0; transparent output",
        "sourceCount": 1,
        "resolvedCount": 0,
        "segmentCount": 0,
        "chunkCount": 0,
        "nonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "legacyCentralPixelParity": {
        "name": "rendererless-frozen-frame-legacy-central-pixel-parity",
        "passed": true,
        "available": true,
        "evidence": "frozenTick=8001; immutableCommands=4; legacySpriteRenderers=4; legacyPixels=4756; centralPixels=4756",
        "sourceCount": 4,
        "resolvedCount": 4,
        "segmentCount": 4,
        "chunkCount": 1,
        "nonTransparentPixels": 4756,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0
    },
    "legacyPng": "P8-C-legacy.png",
    "centralPng": "P8-C-central.png",
    "parityDiffPng": "P8-C-parity-diff.png",
    "generationReusePng": "P8-C-generation-reuse.png",
    "isolatedExpansionPng": "P8-C-isolated-expansion-33.png",
    "arrayPng": "P8-C-array.png",
    "orderedPagesPng": "P8-C-ordered-pages.png",
    "atlasDiffPng": "P8-C-atlas-diff.png",
    "interleavePng": "P8-C-aba-interleave.png",
    "categoryOcclusionPng": "P8-C-category-occlusion.png",
    "chunk4097Png": "P8-C-chunk-4097.png",
    "liveProductionPng": "P8-C-live-production.png",
    "productionCharacterLegacyPng": "P8-C-production-character-legacy.png",
    "productionCharacterCentralPng": "P8-C-production-character-central.png",
    "productionCharacterDiffPng": "P8-C-production-character-diff.png",
    "productionWeaponLegacyPng": "P8-C-production-weapon-legacy.png",
    "productionWeaponCentralPng": "P8-C-production-weapon-central.png",
    "productionWeaponDiffPng": "P8-C-production-weapon-diff.png",
    "skillInputOpointCovered": false,
    "livePoolScope": "production pool checkout + SimulationWorld registration/publication; skill-input opoint is not asserted",
    "livePoolEntities": [],
    "productionCharacterResource": {
        "category": "",
        "available": false,
        "passed": false,
        "objectId": 0,
        "currentDatObjectType": 0,
        "runtimeSlot": 0,
        "runtimeGeneration": 0,
        "effectivePic": 0,
        "resourceKey": "",
        "sourceSheetPath": "",
        "sourceTextureName": "",
        "centralTextureName": "",
        "bindingMode": "",
        "atlasSlice": 0,
        "pixelRect": "",
        "normalizedUv": "",
        "pivot": "",
        "legacyNonTransparentPixels": 0,
        "centralNonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0,
        "evidence": ""
    },
    "productionWeaponResource": {
        "category": "",
        "available": false,
        "passed": false,
        "objectId": 0,
        "currentDatObjectType": 0,
        "runtimeSlot": 0,
        "runtimeGeneration": 0,
        "effectivePic": 0,
        "resourceKey": "",
        "sourceSheetPath": "",
        "sourceTextureName": "",
        "centralTextureName": "",
        "bindingMode": "",
        "atlasSlice": 0,
        "pixelRect": "",
        "normalizedUv": "",
        "pivot": "",
        "legacyNonTransparentPixels": 0,
        "centralNonTransparentPixels": 0,
        "meanChannelDifference": 0.0,
        "maximumChannelDifference": 0,
        "evidence": ""
    }
}

[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# P8-C acceptance architect verification

Review the current P8-C production acceptance implementation without editing files.

Verify specifically:
- the new runtime APIs are narrow and read-only, with acceptance/diagnostic naming rather than exposing mutable registries;
- requested production cases fail closed when Play Mode/live services are unavailable;
- the live path, when available, uses real LF2ObjectPool checkout, real registered logic entities/runtime handles, unique mounts, published commands, catalog resource resolution, nontransparent pixels, and complete cleanup/release;
- representative production character and weapon resources come from the bound live catalog/frame and report resource keys plus central binding modes;
- synthetic fixtures remain separately identified;
- focused tests cover the report contract and unavailable-production failure behavior;
- no P8-D benchmark or documentation changes were introduced by this P8-C implementation.

Use the fresh evidence artifacts:
- Temp/P8-C-PostFix-EditMode/P8-C-report.json (expected deterministic PASS)
- Temp/P8-C-PostFix-RequestedUnavailable/P8-C-report.json (expected requested production FAIL outside Play Mode)

Return findings first, ordered by severity with exact file/line references. If there are no blocking findings, explicitly state that. Distinguish static verification from the remaining Play Mode prerequisite.
