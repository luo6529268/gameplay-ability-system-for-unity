using System;
using System.Threading;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;

namespace NTSD.Animation.Rendering
{
    public enum BattlePixelFrameOwner : byte
    {
        Legacy = 0,
        Central = 1,
    }

    public readonly struct BattlePixelFramePlan
    {
        internal BattlePixelFramePlan(
            SimulationWorld world,
            BattlePresentationFrame capturedFrame,
            BattlePresentationBackendMode requestedMode,
            BattlePixelFrameOwner owner,
            int simulationTick,
            int displayTick,
            int generation,
            bool isStale,
            string reason,
            BattleCentralSubmission submission)
        {
            World = world;
            CapturedFrame = capturedFrame;
            RequestedMode = requestedMode;
            Owner = owner;
            SimulationTick = simulationTick;
            DisplayTick = displayTick;
            Generation = generation;
            IsStale = isStale;
            Reason = reason ?? string.Empty;
            Submission = submission;
        }

        public SimulationWorld World { get; }
        public BattlePresentationFrame CapturedFrame { get; }
        public BattlePresentationBackendMode RequestedMode { get; }
        public BattlePixelFrameOwner Owner { get; }
        public int SimulationTick { get; }
        public int DisplayTick { get; }
        public int TickIndex => DisplayTick;
        public int Generation { get; }
        public bool IsStale { get; }
        public string Reason { get; }
        public string FallbackReason => Reason;
        public BattleCentralSubmission Submission { get; }
        public bool IsValid => World != null;
        public bool UsesCentralPixels => Owner == BattlePixelFrameOwner.Central && Submission != null;
        public bool SuppressesLegacyMaterializers =>
            RequestedMode == BattlePresentationBackendMode.CentralOnly &&
            Owner == BattlePixelFrameOwner.Central;
    }

    public sealed class BattleCentralSubmission
    {
        private readonly BattlePresentationFrame frozenFrame = new BattlePresentationFrame();
        private CharacterAnimtorManager catalogManager;
        private BattleSpriteCatalog catalog = BattleSpriteCatalog.Empty;
        private int readLeaseCount;
        private int readLeaseToken;
        private int nextReadLeaseToken;
        private int retired = 1;
        private int resourcesReleased = 1;
        private int submittedGeneration;
        private int submittedTickIndex = -1;
        private int submittedDrawCount;

        internal BattleCentralSubmission(
            BattleDynamicMeshBackend backend,
            BattleFootMarkerBatchBackend footMarkerBackend,
            BattleHealthBarBatchBackend healthBackend)
        {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
            FootMarkerBackend = footMarkerBackend ??
                                throw new ArgumentNullException(nameof(footMarkerBackend));
            HealthBackend = healthBackend ?? throw new ArgumentNullException(nameof(healthBackend));
        }

        public SimulationWorld World { get; private set; }
        public BattlePresentationFrame CapturedFrame { get; private set; }
        public int TickIndex { get; private set; }
        public int Generation { get; private set; }
        public BattleDynamicMeshBackend Backend { get; }
        public BattleFootMarkerBatchBackend FootMarkerBackend { get; }
        public BattleHealthBarBatchBackend HealthBackend { get; }
        public int BackendMutationVersion { get; private set; }
        public int FootMarkerBackendMutationVersion { get; private set; }
        public int HealthBackendMutationVersion { get; private set; }
        public int ReadLeaseCount => Volatile.Read(ref readLeaseCount);
        public bool IsRetired => Volatile.Read(ref retired) != 0;
        internal bool IsBackendBuildCurrent =>
            Backend != null && Backend.MutationVersion == BackendMutationVersion &&
            ReferenceEquals(Backend.BuiltFrame, CapturedFrame) &&
            FootMarkerBackend != null &&
            FootMarkerBackend.MutationVersion == FootMarkerBackendMutationVersion &&
            ReferenceEquals(FootMarkerBackend.BuiltFrame, CapturedFrame) &&
            HealthBackend != null &&
            HealthBackend.MutationVersion == HealthBackendMutationVersion &&
            ReferenceEquals(HealthBackend.BuiltFrame, CapturedFrame);
        internal bool IsReusable => IsRetired && ReadLeaseCount == 0;

        internal BattlePresentationFrame CaptureFrame(
            BattlePresentationFrame source,
            BattleTickDetailPhaseDiagnostics detailDiagnostics = null)
        {
            if (!IsReusable)
                throw new InvalidOperationException("Cannot capture into a leased central submission slot.");
            frozenFrame.CopyFrom(
                source ?? throw new ArgumentNullException(nameof(source)),
                detailDiagnostics);
            return frozenFrame;
        }

        internal void PrepareCapacity(
            int entityCapacity,
            int hitRecordCapacity,
            int commandCapacity)
        {
            if (!IsReusable)
            {
                throw new InvalidOperationException(
                    "Cannot resize a central submission while it is published or leased.");
            }

            frozenFrame.PrepareCapacity(
                entityCapacity,
                hitRecordCapacity,
                commandCapacity);
            FootMarkerBackend.PrepareCapacity(
                Math.Min(entityCapacity, BattleFootMarkerBatchBackend.MaximumMarkersPerBatch));
            HealthBackend.PrepareCapacity(
                Math.Min(entityCapacity, BattleHealthBarBatchBackend.MaximumBarsPerBatch));
        }

        internal void Publish(
            SimulationWorld world,
            BattlePresentationFrame capturedFrame,
            int tickIndex,
            int generation,
            CharacterAnimtorManager manager,
            BattleSpriteCatalog publishedCatalog)
        {
            if (!IsReusable)
                throw new InvalidOperationException("Cannot publish into a leased central submission slot.");
            if (!ReferenceEquals(capturedFrame, frozenFrame) ||
                !ReferenceEquals(Backend.BuiltFrame, frozenFrame) ||
                !ReferenceEquals(FootMarkerBackend.BuiltFrame, frozenFrame) ||
                !ReferenceEquals(HealthBackend.BuiltFrame, frozenFrame))
            {
                throw new InvalidOperationException(
                    "A central submission must publish the independent frozen frame used by its backend build.");
            }

            ReleaseCatalogBinding();
            World = world;
            CapturedFrame = capturedFrame;
            TickIndex = tickIndex;
            Generation = generation;
            BackendMutationVersion = Backend.MutationVersion;
            FootMarkerBackendMutationVersion = FootMarkerBackend.MutationVersion;
            HealthBackendMutationVersion = HealthBackend.MutationVersion;
            catalogManager = manager;
            catalog = publishedCatalog ?? BattleSpriteCatalog.Empty;
            catalogManager?.RegisterRendererCatalogBinding(catalog);
            Volatile.Write(ref submittedDrawCount, 0);
            Volatile.Write(ref submittedTickIndex, -1);
            Volatile.Write(ref submittedGeneration, 0);
            Volatile.Write(ref resourcesReleased, 0);
            Volatile.Write(ref retired, 0);
        }

        internal bool TryRecordExecutedDraws(int generation, int tickIndex, int drawCount)
        {
            if (drawCount < 0)
                throw new ArgumentOutOfRangeException(nameof(drawCount));
            if (Volatile.Read(ref retired) != 0 || generation != Generation || tickIndex != TickIndex)
                return false;

            Volatile.Write(ref submittedGeneration, generation);
            Volatile.Write(ref submittedTickIndex, tickIndex);
            Volatile.Write(ref submittedDrawCount, drawCount);
            return true;
        }

        internal int GetExecutedDrawCount(int generation, int tickIndex)
        {
            return Volatile.Read(ref submittedGeneration) == generation &&
                   Volatile.Read(ref submittedTickIndex) == tickIndex
                ? Volatile.Read(ref submittedDrawCount)
                : 0;
        }

        internal bool TryAcquire(out BattleCentralSubmissionLease lease)
        {
            lease = default;
            if (Volatile.Read(ref retired) != 0 ||
                Interlocked.CompareExchange(ref readLeaseCount, 1, 0) != 0)
                return false;

            int token = Interlocked.Increment(ref nextReadLeaseToken);
            if (token <= 0)
            {
                Interlocked.Exchange(ref nextReadLeaseToken, 1);
                token = 1;
            }
            Volatile.Write(ref readLeaseToken, token);
            if (Volatile.Read(ref retired) == 0)
            {
                lease = new BattleCentralSubmissionLease(this, token, Generation);
                return true;
            }

            ReleaseReadLease(token, Generation);
            return false;
        }

        internal void Retire()
        {
            if (Interlocked.Exchange(ref retired, 1) != 0)
                return;
            TryReleaseResources();
        }

        private void ReleaseReadLease(int token, int generation)
        {
            if (generation != Generation || token == 0 ||
                Interlocked.CompareExchange(ref readLeaseToken, 0, token) != token)
            {
                return;
            }

            Interlocked.Exchange(ref readLeaseCount, 0);
            TryReleaseResources();
        }

        private void TryReleaseResources()
        {
            if (Volatile.Read(ref retired) == 0 || Volatile.Read(ref readLeaseCount) != 0 ||
                Interlocked.Exchange(ref resourcesReleased, 1) != 0)
            {
                return;
            }

            ReleaseCatalogBinding();
        }

        private void ReleaseCatalogBinding()
        {
            CharacterAnimtorManager manager = catalogManager;
            BattleSpriteCatalog publishedCatalog = catalog;
            catalogManager = null;
            catalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(publishedCatalog);
        }

        public readonly struct BattleCentralSubmissionLease : IDisposable
        {
            private readonly BattleCentralSubmission submission;
            private readonly int token;
            private readonly int generation;

            internal BattleCentralSubmissionLease(
                BattleCentralSubmission value,
                int leaseToken,
                int submissionGeneration)
            {
                submission = value;
                token = leaseToken;
                generation = submissionGeneration;
            }

            public BattleCentralSubmission Submission => submission;
            public BattleDynamicMeshBackend Backend => IsValid ? submission.Backend : null;
            public BattleFootMarkerBatchBackend FootMarkerBackend =>
                IsValid ? submission.FootMarkerBackend : null;
            public BattleHealthBarBatchBackend HealthBackend =>
                IsValid ? submission.HealthBackend : null;
            public int TickIndex => IsValid ? submission.TickIndex : -1;
            public int Generation => generation;
            public bool IsValid => submission != null && token != 0 && submission.Generation == generation;

            public void Dispose()
            {
                submission?.ReleaseReadLease(token, generation);
            }
        }
    }
}
