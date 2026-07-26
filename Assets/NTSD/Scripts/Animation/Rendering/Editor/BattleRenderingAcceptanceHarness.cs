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
        public bool enterPlayMode;
        public int playModeWarmupFrames = 600;
        public bool exitPlayModeAfterRun;
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
        public int segmentCount;
        public int chunkCount;
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
                        ? "production LF2ObjectPointFactory initialization + pool expansion + SimulationWorld publication; skill-input opoint is not asserted"
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
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            LF2ObjectPointFactory objectPointFactory = LF2ObjectPointFactory.Instance;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            if (pool == null || referencePool == null || objectPointFactory == null ||
                world == null || manager == null || dataManager == null)
            {
                result.available = false;
                result.passed = false;
                result.evidence = string.Format(
                    CultureInfo.InvariantCulture,
                    "required production services missing: objectPool={0}; referencePool={1}; objectPointFactory={2}; " +
                    "world={3}; animatorManager={4}; dataManager={5}",
                    pool != null,
                    referencePool != null,
                    objectPointFactory != null,
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
            int logicActiveBefore = referencePool.ActiveCount;
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

            var blockingRenderers = new List<LF2ObjectRenderer>(availableBefore);
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
                for (int index = 0; index < availableBefore; index++)
                {
                    GameObject blockerRoot = pool.Get(out LF2ObjectRenderer blockerRenderer);
                    if (blockerRoot == null || blockerRenderer == null ||
                        !owners.Add(blockerRenderer.GetInstanceID()))
                    {
                        valid = false;
                        break;
                    }
                    blockingRenderers.Add(blockerRenderer);
                }

                if (blockingRenderers.Count == availableBefore)
                {
                    for (int liveIndex = 0; liveIndex < liveEntityCount; liveIndex++)
                    {
                        bool isCharacter = liveIndex == 0;
                        ProductionSample sample = isCharacter ? characterSample : weaponSample;
                        OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                        task.opoint = new ObjectPoint
                        {
                            kind = 1,
                            oid = sample.ObjectId,
                            action = sample.Frame.frameId,
                            facing = 0,
                        };
                        task.team = 1;
                        task.dir = "right";
                        task.useDirectRuntimePosition = true;
                        task.directX = 40 + liveIndex * 24;
                        task.directY = 0;
                        task.directZ = 120 + liveIndex * 3;
                        task.useDirectVelocity = true;
                        task.preserveActionZero = true;
                        task.skipPostInitZOffset = true;

                        LF2Entity entity;
                        try
                        {
                            entity = objectPointFactory.CreateObjectImmediate(task);
                        }
                        finally
                        {
                            referencePool.Recycle(task);
                        }

                        if (entity == null)
                        {
                            valid = false;
                            break;
                        }
                        liveEntities.Add(entity);
                        valid &= isCharacter ? entity is LF2Character : entity is LF2WeaponBase;
                        LF2ObjectRenderer renderer = entity.Renderer;
                        GameObject root = renderer != null ? renderer.transform.parent?.gameObject : null;
                        if (root == null || !owners.Add(renderer.GetInstanceID()))
                        {
                            valid = false;
                            break;
                        }
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
                        liveHandles.Add(handle);
                        liveMounts.Add(mounts);
                        liveEvidence.Add(new BattleRenderingLiveEntityEvidence
                        {
                            checkoutIndex = availableBefore + liveIndex,
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
                }
                availableWhileExhausted = pool.AvailableObjectCountForAcceptance;
                valid &= blockingRenderers.Count + liveEntities.Count == acquireCount &&
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
                for (int index = liveEntities.Count - 1; index >= 0; index--)
                    liveEntities[index].FreeEntityLikeExe();
                for (int index = blockingRenderers.Count - 1; index >= 0; index--)
                    pool.Release(blockingRenderers[index]);
            }

            int availableAfter = pool.AvailableObjectCountForAcceptance;
            int activeAfter = pool.ActiveObjectCountForAcceptance;
            int claimedAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
            int logicActiveAfter = referencePool.ActiveCount;
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
                    !BattleCentralPresentationMountDiagnostics.HasValidOwnerRuntimeBindingForAcceptance(
                        evidence.ownerInstanceId);
                valid &= evidence.releasedHandleRejected &&
                         evidence.releasedMountsCleared &&
                         evidence.releasedOwnerBindingCleared &&
                         evidence.resourceResolved &&
                         evidence.nonTransparentPixels > 0;
            }

            valid &= availableAfter == availableBefore + liveEntityCount &&
                      activeAfter == activeBefore &&
                      claimedAfter == claimedBefore &&
                      logicActiveAfter == logicActiveBefore;
            report.livePoolEntities = liveEvidence.ToArray();
            result.sourceCount = liveEvidence.Count;
            result.resolvedCount = CountResolvedLiveEvidence(liveEvidence);
            result.nonTransparentPixels = SumLivePixels(liveEvidence);
            result.evidence = string.Format(
                CultureInfo.InvariantCulture,
                "scope=production LF2ObjectPointFactory initialization + pool expansion + world publication; " +
                "skill-input opoint is explicitly not asserted; " +
                "availableBefore={0}; totalCheckout={1}; expandedAndPublished={2}; requestedExtra={3}; availableAtPeak={4}; " +
                "availableAfter={5}; activeBeforeAfter={6}/{7}; claimedBeforeAfter={8}/{9}; " +
                "logicActiveBeforeAfter={10}/{11}; uniqueCheckoutOwners={12}; uniqueRuntimeHandles={13}; " +
                "allReleaseResidueChecks={14}",
                availableBefore,
                blockingRenderers.Count + liveEntities.Count,
                liveEvidence.Count,
                config.LivePoolExtraCount,
                availableWhileExhausted,
                availableAfter,
                activeBefore,
                activeAfter,
                claimedBefore,
                claimedAfter,
                logicActiveBefore,
                logicActiveAfter,
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
            AggregateProductionBackendCounts(
                result,
                report.productionCharacterResource,
                report.productionWeaponResource);
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
                            report.productionWeaponResource.passed &&
                            result.segmentCount == 2 &&
                            result.chunkCount == 2;
            result.evidence = string.Format(
                CultureInfo.InvariantCulture,
                "immutableProductionFrameTick={0}; characterKey={1}; characterType={2}; " +
                "weaponKey={3}; weaponType={4}; independentBackendSegments/Chunks={5}/{6}; " +
                "source-vs-central pixel parity={7}",
                publishedFrame.TickIndex,
                report.productionCharacterResource.resourceKey,
                report.productionCharacterResource.currentDatObjectType,
                report.productionWeaponResource.resourceKey,
                report.productionWeaponResource.currentDatObjectType,
                result.segmentCount,
                result.chunkCount,
                result.passed);
            return result;
        }

        internal static void AggregateProductionBackendCounts(
            BattleRenderingAcceptanceCase aggregate,
            BattleRenderingProductionResourceEvidence character,
            BattleRenderingProductionResourceEvidence weapon)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));
            if (character == null)
                throw new ArgumentNullException(nameof(character));
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));

            // Each resource is rendered by its own backend, so report the sum of those independent builds.
            aggregate.segmentCount = character.segmentCount + weapon.segmentCount;
            aggregate.chunkCount = character.chunkCount + weapon.chunkCount;
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
                          backend.SegmentCount == 1 &&
                          backend.ActiveChunkCount == 1 &&
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
                segmentCount = backend.SegmentCount,
                chunkCount = backend.ActiveChunkCount,
                legacyNonTransparentPixels = legacy.NonTransparentPixelCount,
                centralNonTransparentPixels = central.NonTransparentPixelCount,
                meanChannelDifference = difference.MeanChannelDifference,
                maximumChannelDifference = difference.MaximumChannelDifference,
                evidence = string.Format(
                    CultureInfo.InvariantCulture,
                    "typedSnapshot={0}; sourceSprite={1}; centralBinding={2}/slice{3}; " +
                    "segments/chunks={4}/{5}; legacyPixels={6}; centralPixels={7}; mean/maxDiff={8:F4}/{9}",
                    snapshot.CurrentDatObjType,
                    entry.LegacySprite != null,
                    resource.BindingMode,
                    resource.AtlasSlice,
                    backend.SegmentCount,
                    backend.ActiveChunkCount,
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
