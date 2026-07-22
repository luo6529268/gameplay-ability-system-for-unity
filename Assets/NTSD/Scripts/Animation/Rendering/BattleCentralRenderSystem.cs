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
        private static BattlePresentationBackendMode requestedMode = BattlePresentationBackendMode.LegacyOnly;
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
                world?.BattlePresentation?.Mode ?? BattlePresentationBackendMode.LegacyOnly;
            BattlePresentationFrame frame = world?.BattlePresentation?.PublishedFrame;
            int tickIndex = frame?.TickIndex ?? 0;
            BattlePixelFramePlan current = world != null ? world.CurrentPixelFramePlan : default;
            if (current.IsValid && ReferenceEquals(current.World, world) &&
                ReferenceEquals(current.CapturedFrame, frame) && current.TickIndex == tickIndex &&
                current.RequestedMode == mode && CurrentPixelFramePlan.Generation == current.Generation)
            {
                return current;
            }

            requestedMode = mode;
            ResetPerFrameDiagnostics(mode, frame != null);

            if (world == null)
                return CommitLegacyPlan(null, frame, mode, tickIndex, "SimulationWorld is unavailable.");
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    tickIndex,
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
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    tickIndex,
                    "No central staging backend is available because the previous submission is still leased.");
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
                return CommitLegacyPlan(world, frame, mode, tickIndex, reason);
            }

            try
            {
                CatalogResolver.Configure(
                    catalog,
                    commonVisualCatalog,
                    featureMaterial,
                    featureArrayMaterial);
                stagingBackend.Build(frame, CatalogResolver, drawMode);
                lastBuiltBackend = stagingBackend;
            }
            catch (Exception exception)
            {
                stagingBackend.Clear();
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    tickIndex,
                    $"Central geometry build failed: {exception.GetType().Name}: {exception.Message}");
            }

            bool allCategoryOwnershipReady = frameReady && commonReady &&
                                             frame.OverlayUnsupportedCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedCategoryCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedRenderStateCount == 0 &&
                                             stagingBackend.Diagnostics.UnresolvedCommandCount == 0;
            RuntimeDiagnostics.AllCategoryOwnershipReady = allCategoryOwnershipReady;

            if (mode == BattlePresentationBackendMode.CentralShadowBuild)
            {
                BindDiagnosticCatalog(manager, catalog);
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    tickIndex,
                    "CentralShadowBuild builds diagnostics but fixes pixel ownership to Legacy.",
                    true);
            }

            if (!allCategoryOwnershipReady)
            {
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    tickIndex,
                    BuildOwnershipRefusalReason(stagingBackend));
            }

            ReleaseDiagnosticCatalogBinding();
            int generation = NextGeneration();
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            submission.Publish(
                world,
                frame,
                tickIndex,
                generation,
                manager,
                catalog);
            var plan = new BattlePixelFramePlan(
                world,
                frame,
                mode,
                BattlePixelFrameOwner.Central,
                tickIndex,
                generation,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = true;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        public static bool ShouldUseCentralPixels(SimulationWorld world)
        {
            BattlePixelFramePlan plan = world != null ? world.CurrentPixelFramePlan : default;
            BattlePixelFramePlan globalPlan = CurrentPixelFramePlan;
            BattlePresentationFrame currentFrame = world?.BattlePresentation?.PublishedFrame;
            BattleCentralSubmission submission = plan.Submission;
            return plan.IsValid && globalPlan.IsValid && plan.Generation == globalPlan.Generation &&
                   ReferenceEquals(plan.World, world) &&
                   ReferenceEquals(plan.CapturedFrame, currentFrame) &&
                   plan.Owner == BattlePixelFrameOwner.Central &&
                   plan.RequestedMode == BattlePresentationBackendMode.CentralOnly &&
                   plan.TickIndex == currentFrame?.TickIndex && submission != null &&
                   !submission.IsRetired && ReferenceEquals(submission.World, world) &&
                   ReferenceEquals(submission.CapturedFrame, currentFrame) &&
                   submission.TickIndex == plan.TickIndex &&
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
                return CommitLegacyPlan(
                    world,
                    frame,
                    world.BattlePresentation.Mode,
                    tickIndex,
                    "Self-check central publication requires a current CentralOnly frame.");
            }
            if (!TryGetReusableBackend(out int backendIndex, out BattleDynamicMeshBackend backend))
            {
                return CommitLegacyPlan(
                    world,
                    frame,
                    BattlePresentationBackendMode.CentralOnly,
                    tickIndex,
                    "Self-check central publication found no reusable backend slot.");
            }

            backend.Clear();
            lastBuiltBackend = backend;
            int generation = NextGeneration();
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            submission.Publish(
                world,
                frame,
                tickIndex,
                generation,
                manager,
                manager?.SpriteCatalog ?? BattleSpriteCatalog.Empty);
            var plan = new BattlePixelFramePlan(
                world,
                frame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                tickIndex,
                generation,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.FrameAvailable = true;
            RuntimeDiagnostics.AllCategoryOwnershipReady = true;
            RuntimeDiagnostics.SubmissionReady = true;
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
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

        internal static void RecordSubmission(int drawCount)
        {
            RuntimeDiagnostics.SubmittedPixelsLastFrame = drawCount > 0;
            RuntimeDiagnostics.LastSubmissionDrawCount = drawCount;
            RuntimeDiagnostics.SubmissionCount += drawCount;
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
                RuntimeDiagnostics.EffectivePixelMode);
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
            requestedMode = BattlePresentationBackendMode.LegacyOnly;
            ResetPerFrameDiagnostics(BattlePresentationBackendMode.LegacyOnly, false);
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
                NextGeneration(),
                reason,
                null);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.RefusalReason = reason ?? string.Empty;
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
                reason = "BattleRenderFeature is not registered and active; pixel output falls back to Legacy.";
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
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;
            RuntimeDiagnostics.FrameAvailable = frameAvailable;
            RuntimeDiagnostics.AllCategoryOwnershipReady = false;
            RuntimeDiagnostics.CommonShadowBindingReady = false;
            RuntimeDiagnostics.CommonSparkBindingReady = false;
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
            RuntimeDiagnostics.LastSubmissionDrawCount = 0;
            RuntimeDiagnostics.RefusalReason = string.Empty;
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
