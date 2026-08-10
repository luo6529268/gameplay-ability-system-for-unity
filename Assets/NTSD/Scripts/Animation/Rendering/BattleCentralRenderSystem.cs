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
        private const int MaximumCommonTrustedResourceCount =
            1 +
            BattleCommonVisualCatalog.SparkFrameCount +
            BattleCommonVisualCatalog.WordSheetCount *
            BattleCommonVisualCatalog.WordGlyphsPerSheet +
            BattleCommonVisualCatalog.WordSheetCount;

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
        private static AttemptedBuildDiagnostics lastAttemptedBuildDiagnostics;
        private static SimulationWorld pendingPublishedWorld;
        private static BattlePresentationFrame pendingPublishedFrame;
        private static BattlePresentationBackendMode pendingPublishedMode;
        private static int pendingPublishedTick = -1;
        private static int pendingPublicationVersion;
        private static int lastMaterializedPublicationVersion;
        private static int lastMaterializedPublishedTick = -1;
        private static int lastMaterializedUnityFrame = -1;
        private static int materializationInProgress;

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
        public static int PendingPublishedTickForDiagnostics =>
            Volatile.Read(ref pendingPublishedTick);
        public static int LastMaterializedPublishedTickForDiagnostics =>
            Volatile.Read(ref lastMaterializedPublishedTick);

        internal static void PrepareBattleCapacity(
            int entityCapacity,
            int commandCapacity,
            int catalogEntryCapacity)
        {
            if (entityCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(entityCapacity));
            if (commandCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(commandCapacity));
            if (catalogEntryCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(catalogEntryCapacity));

            int hitRecordCapacity = checked(
                entityCapacity *
                NTSD.Animation.LF2Objects.LF2Entity.MaxHitRecordSlots);
            for (int index = 0; index < Backends.Length; index++)
            {
                Backends[index].PrepareCapacity(commandCapacity);
                SlotSubmissions[index].PrepareCapacity(
                    entityCapacity,
                    hitRecordCapacity,
                    commandCapacity);
            }

            int trustedResourceCapacity = checked(
                catalogEntryCapacity + MaximumCommonTrustedResourceCount);
            CatalogResolver.PrepareCapacity(
                catalogEntryCapacity,
                trustedResourceCapacity);
            DiagnosticCatalogResolver.PrepareCapacity(
                catalogEntryCapacity,
                trustedResourceCapacity);
            CatalogResolver.SealCapacity();
            DiagnosticCatalogResolver.SealCapacity();
        }

        internal static void EndBattleCapacitySeal()
        {
            CatalogResolver.UnsealCapacity();
            DiagnosticCatalogResolver.UnsealCapacity();
        }

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
            return FlushLatestPublishedFrame(world);
        }

        public static BattlePixelFramePlan FlushLatestPublishedFrame(SimulationWorld world)
        {
            QueueLatestPublishedFrame(world);
            return MaterializeLatestPublishedFrame(
                -1,
                true,
                world,
                false);
        }

        internal static void QueueLatestPublishedFrame(SimulationWorld world)
        {
            if (world == null)
                return;

            ClearPublishedPlanForWorldChange(world);
            BattlePresentationFrame frame = world.BattlePresentation?.PublishedFrame;
            BattlePresentationBackendMode mode =
                world.BattlePresentation?.Mode ?? BattlePresentationBackendMode.CentralOnly;
            int tickIndex = frame?.TickIndex ?? world.CurrentTickIndex;
            if (ReferenceEquals(Volatile.Read(ref pendingPublishedWorld), world) &&
                ReferenceEquals(Volatile.Read(ref pendingPublishedFrame), frame) &&
                Volatile.Read(ref pendingPublishedTick) == tickIndex &&
                pendingPublishedMode == mode)
            {
                return;
            }

            Volatile.Write(ref pendingPublishedWorld, world);
            Volatile.Write(ref pendingPublishedFrame, frame);
            pendingPublishedMode = mode;
            Volatile.Write(ref pendingPublishedTick, tickIndex);
            int version = Interlocked.Increment(ref pendingPublicationVersion);
            if (version <= 0)
            {
                Interlocked.Exchange(ref pendingPublicationVersion, 1);
                Volatile.Write(ref lastMaterializedPublicationVersion, 0);
            }
        }

#if UNITY_EDITOR
        public static void QueueLatestPublishedFrameForSelfCheck(SimulationWorld world)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Latest-frame queue self-check hook is editor-only.");
            QueueLatestPublishedFrame(world);
        }
#endif

        private static void ClearPublishedPlanForWorldChange(SimulationWorld nextWorld)
        {
            BattlePixelFramePlan current = CurrentPixelFramePlan;
            if (!current.IsValid || ReferenceEquals(current.World, nextWorld))
                return;

            Volatile.Write(ref publishedPlanGeneration, 0);
            Volatile.Write(ref publishedPlanWorld, null);
            current.World?.PublishPixelFramePlan(default);
            current.Submission?.Retire();
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
            RuntimeDiagnostics.LastSubmissionDrawCount = 0;
        }

        internal static void MaterializeLatestPublishedFrameForCamera(
            BattleRenderFeature owner,
            Camera camera,
            CameraRenderType renderType)
        {
            if (owner == null || owner != featureOwner ||
                !IsWorldRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera))
                return;

            MaterializeLatestPublishedFrame(
                Time.frameCount,
                false,
                null,
                true);
        }

#if UNITY_EDITOR
        public static BattlePixelFramePlan MaterializeLatestPublishedFrameForSelfCheck(
            int unityFrame)
        {
            if (!Application.isEditor)
            {
                throw new InvalidOperationException(
                    "Latest-frame materialization self-check hook is editor-only.");
            }

            return MaterializeLatestPublishedFrame(
                unityFrame,
                false,
                null,
                false);
        }
#endif

        private static BattlePixelFramePlan MaterializeLatestPublishedFrame(
            int unityFrame,
            bool force,
            SimulationWorld expectedWorld,
            bool deferDetailTiming)
        {
            SimulationWorld world = Volatile.Read(ref pendingPublishedWorld);
            int publicationVersion = Volatile.Read(ref pendingPublicationVersion);
            if (world == null || publicationVersion <= 0 ||
                expectedWorld != null && !ReferenceEquals(expectedWorld, world))
            {
                return expectedWorld != null
                    ? expectedWorld.CurrentPixelFramePlan
                    : CurrentPixelFramePlan;
            }

            if (!force)
            {
                if (Volatile.Read(ref lastMaterializedUnityFrame) == unityFrame ||
                    Volatile.Read(ref lastMaterializedPublicationVersion) == publicationVersion)
                {
                    return CurrentPixelFramePlan;
                }
            }

            if (Interlocked.CompareExchange(ref materializationInProgress, 1, 0) != 0)
                return CurrentPixelFramePlan;

            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            bool deferredTimingStarted = deferDetailTiming &&
                                         detailDiagnostics?.BeginDeferredRenderMaterialization() == true;
            try
            {
                if (!force)
                    Volatile.Write(ref lastMaterializedUnityFrame, unityFrame);
                Volatile.Write(ref lastMaterializedPublicationVersion, publicationVersion);

                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard);
                BattlePixelFramePlan plan;
                try
                {
                    plan = PrepareFrameImmediate(world);
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard);
                }

                Volatile.Write(
                    ref lastMaterializedPublishedTick,
                    plan.IsValid ? plan.SimulationTick : -1);
                return plan;
            }
            finally
            {
                if (deferredTimingStarted)
                    detailDiagnostics.EndDeferredRenderMaterialization();
                Volatile.Write(ref materializationInProgress, 0);
            }
        }

        private static BattlePixelFramePlan PrepareFrameImmediate(SimulationWorld world)
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
            lastAttemptedBuildDiagnostics = default;

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
                BattleTickDetailPhaseDiagnostics detailDiagnostics =
                    world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                BattleCentralSubmission stagingSubmission = SlotSubmissions[backendIndex];
                BattlePresentationFrame buildFrame = frame != null
                    ? stagingSubmission.CaptureFrame(frame, detailDiagnostics)
                    : null;
                BattleSpriteCatalog buildCatalog = buildFrame?.BoundCatalog ?? catalog;
                BattleCommonVisualCatalog buildCommonVisualCatalog =
                    buildFrame?.CommonVisualCatalog ?? commonVisualCatalog;
                CatalogResolver.Configure(
                    buildCatalog,
                    buildCommonVisualCatalog,
                    featureMaterial,
                    featureArrayMaterial);
                stagingBackend.Build(
                    buildFrame,
                    CatalogResolver,
                    drawMode,
                    detailDiagnostics);
                lastBuiltBackend = stagingBackend;
                lastAttemptedBuildDiagnostics = AttemptedBuildDiagnostics.Capture(
                    stagingBackend,
                    simulationTick);
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
            return TryAcquireSubmission(
                camera,
                renderType,
                camera != null ? camera.cameraType : CameraType.Game,
                Application.isPlaying,
                out lease);
        }

#if UNITY_EDITOR
        public static bool TryAcquireSubmissionForSelfCheck(
            Camera camera,
            CameraRenderType renderType,
            CameraType cameraType,
            bool isPlaying,
            out BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Camera submission self-check hook is editor-only.");
            return TryAcquireSubmission(
                camera,
                renderType,
                cameraType,
                isPlaying,
                out lease);
        }
#endif

        private static bool TryAcquireSubmission(
            Camera camera,
            CameraRenderType renderType,
            CameraType cameraType,
            bool isPlaying,
            out BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            lease = default;
            if (Volatile.Read(ref pendingPublicationVersion) !=
                Volatile.Read(ref lastMaterializedPublicationVersion))
            {
                return false;
            }

            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            SimulationWorld world = plan.World;
            if (!CanRenderCamera(
                    camera,
                    renderType,
                    NTSDRenderSpace.WorldCamera,
                    cameraType,
                    isPlaying) ||
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

        public static BattlePixelFramePlan PublishReadyCentralPlanForSelfCheck(
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
            lastAttemptedBuildDiagnostics = AttemptedBuildDiagnostics.Capture(backend, tickIndex);
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

            return CaptureDiagnosticReportForSelfCheck(atlasInputs);
        }

        internal static BattleRenderingDiagnosticReport CaptureDiagnosticReportForSelfCheck(
            BattleAtlasDiagnosticInputs atlasInputs)
        {
            if (atlasInputs == null)
                throw new ArgumentNullException(nameof(atlasInputs));
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            BattleCentralBuildDiagnostics build = null;
            AttemptedBuildDiagnostics attempted = default;
            BattlePresentationFrame reportFrame = null;
            int submissionDrawCount = 0;
            bool submissionBuildCurrent = false;

            if (plan.IsValid && plan.Submission != null && !plan.Submission.IsRetired &&
                plan.Submission.Generation == plan.Generation &&
                plan.Submission.TickIndex == plan.DisplayTick &&
                ReferenceEquals(plan.Submission.CapturedFrame, plan.CapturedFrame))
            {
                reportFrame = plan.Submission.CapturedFrame;
                submissionBuildCurrent = plan.Submission.IsBackendBuildCurrent;
                if (submissionBuildCurrent)
                {
                    build = plan.Submission.Backend.Diagnostics;
                    submissionDrawCount = plan.Submission.GetExecutedDrawCount(
                        plan.Generation,
                        plan.DisplayTick);
                }
            }
            else if (plan.IsValid &&
                     plan.RequestedMode == BattlePresentationBackendMode.CentralShadowBuild &&
                     lastBuiltBackend?.BuiltFrame != null &&
                     lastBuiltBackend.Diagnostics.TickIndex == plan.DisplayTick)
            {
                build = lastBuiltBackend.Diagnostics;
                reportFrame = lastBuiltBackend.BuiltFrame;
                submissionBuildCurrent = true;
            }
            else if (plan.IsValid && lastAttemptedBuildDiagnostics.IsValid &&
                     lastAttemptedBuildDiagnostics.SimulationTick == plan.SimulationTick)
            {
                attempted = lastAttemptedBuildDiagnostics;
                reportFrame = attempted.Frame;
                submissionBuildCurrent = attempted.IsValid;
            }

            int sourceCommandCount = build != null
                ? build.SourceCommandCount
                : attempted.IsValid ? attempted.SourceCommandCount : 0;
            int resolvedCommandCount = build != null
                ? build.ResolvedCommandCount
                : attempted.IsValid ? attempted.ResolvedCommandCount : 0;
            int unresolvedCommandCount = build != null
                ? build.UnresolvedCommandCount
                : attempted.IsValid ? attempted.UnresolvedCommandCount : 0;
            int unsupportedCategoryCount = build != null
                ? build.UnsupportedCategoryCount
                : attempted.IsValid ? attempted.UnsupportedCategoryCount : 0;
            int unsupportedRenderStateCount = build != null
                ? build.UnsupportedRenderStateCount
                : attempted.IsValid ? attempted.UnsupportedRenderStateCount : 0;
            int activeChunkCount = build != null
                ? build.ActiveChunkCount
                : attempted.IsValid ? attempted.ActiveChunkCount : 0;
            int segmentCount = build != null
                ? build.SegmentCount
                : attempted.IsValid ? attempted.SegmentCount : 0;
            int buildTick = build != null
                ? build.TickIndex
                : attempted.IsValid ? attempted.BuildTick : -1;
            int firstUnresolvedCommandIndex = build != null
                ? build.FirstUnresolvedCommandIndex
                : attempted.IsValid ? attempted.FirstUnresolvedCommandIndex : -1;
            BattleRenderCommandType firstUnresolvedCommandType = build != null
                ? build.FirstUnresolvedCommandType
                : attempted.FirstUnresolvedCommandType;
            BattleCentralResourceStatus firstUnresolvedStatus = build != null
                ? build.FirstUnresolvedStatus
                : attempted.FirstUnresolvedStatus;
            return new BattleRenderingDiagnosticReport(
                atlasInputs,
                drawPolicyDecision,
                sourceCommandCount,
                resolvedCommandCount,
                unresolvedCommandCount,
                unsupportedCategoryCount,
                activeChunkCount,
                segmentCount,
                submissionDrawCount,
                plan.IsValid ? plan.RequestedMode : RuntimeDiagnostics.RequestedMode,
                RuntimeDiagnostics.EffectivePixelMode,
                reportFrame?.EntityCount ?? 0,
                plan.IsValid ? plan.Generation : 0,
                buildTick,
                plan.IsValid ? plan.SimulationTick : -1,
                plan.IsValid ? plan.DisplayTick : -1,
                plan.IsValid && plan.IsStale,
                plan.IsValid ? plan.Reason : RuntimeDiagnostics.RefusalReason,
                submissionBuildCurrent,
                unsupportedRenderStateCount,
                firstUnresolvedCommandIndex,
                firstUnresolvedCommandType,
                firstUnresolvedStatus);
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
                    binding.Mode,
                    binding.AtlasPageIndex);
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
            Volatile.Write(ref pendingPublishedWorld, null);
            Volatile.Write(ref pendingPublishedFrame, null);
            pendingPublishedMode = BattlePresentationBackendMode.CentralOnly;
            Volatile.Write(ref pendingPublishedTick, -1);
            Volatile.Write(ref pendingPublicationVersion, 0);
            Volatile.Write(ref lastMaterializedPublicationVersion, 0);
            Volatile.Write(ref lastMaterializedPublishedTick, -1);
            Volatile.Write(ref lastMaterializedUnityFrame, -1);
            Volatile.Write(ref materializationInProgress, 0);
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
            lastAttemptedBuildDiagnostics = default;
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

        private readonly struct AttemptedBuildDiagnostics
        {
            private AttemptedBuildDiagnostics(
                int simulationTick,
                BattlePresentationFrame frame,
                BattleCentralBuildDiagnostics diagnostics)
            {
                SimulationTick = simulationTick;
                Frame = frame;
                BuildTick = diagnostics.TickIndex;
                SourceCommandCount = diagnostics.SourceCommandCount;
                ResolvedCommandCount = diagnostics.ResolvedCommandCount;
                UnresolvedCommandCount = diagnostics.UnresolvedCommandCount;
                UnsupportedCategoryCount = diagnostics.UnsupportedCategoryCount;
                UnsupportedRenderStateCount = diagnostics.UnsupportedRenderStateCount;
                ActiveChunkCount = diagnostics.ActiveChunkCount;
                SegmentCount = diagnostics.SegmentCount;
                FirstUnresolvedCommandIndex = diagnostics.FirstUnresolvedCommandIndex;
                FirstUnresolvedCommandType = diagnostics.FirstUnresolvedCommandType;
                FirstUnresolvedStatus = diagnostics.FirstUnresolvedStatus;
                IsValid = true;
            }

            public bool IsValid { get; }
            public int SimulationTick { get; }
            public BattlePresentationFrame Frame { get; }
            public int BuildTick { get; }
            public int SourceCommandCount { get; }
            public int ResolvedCommandCount { get; }
            public int UnresolvedCommandCount { get; }
            public int UnsupportedCategoryCount { get; }
            public int UnsupportedRenderStateCount { get; }
            public int ActiveChunkCount { get; }
            public int SegmentCount { get; }
            public int FirstUnresolvedCommandIndex { get; }
            public BattleRenderCommandType FirstUnresolvedCommandType { get; }
            public BattleCentralResourceStatus FirstUnresolvedStatus { get; }

            public static AttemptedBuildDiagnostics Capture(
                BattleDynamicMeshBackend backend,
                int simulationTick)
            {
                return backend == null
                    ? default
                    : new AttemptedBuildDiagnostics(
                        simulationTick,
                        backend.BuiltFrame,
                        backend.Diagnostics);
            }
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
