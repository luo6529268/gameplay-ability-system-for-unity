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
            int tickIndex,
            int generation,
            string fallbackReason,
            BattleCentralSubmission submission)
        {
            World = world;
            CapturedFrame = capturedFrame;
            RequestedMode = requestedMode;
            Owner = owner;
            TickIndex = tickIndex;
            Generation = generation;
            FallbackReason = fallbackReason ?? string.Empty;
            Submission = submission;
        }

        public SimulationWorld World { get; }
        public BattlePresentationFrame CapturedFrame { get; }
        public BattlePresentationBackendMode RequestedMode { get; }
        public BattlePixelFrameOwner Owner { get; }
        public int TickIndex { get; }
        public int Generation { get; }
        public string FallbackReason { get; }
        public BattleCentralSubmission Submission { get; }
        public bool IsValid => World != null;
        public bool UsesCentralPixels => Owner == BattlePixelFrameOwner.Central && Submission != null;
    }

    public sealed class BattleCentralSubmission
    {
        private CharacterAnimtorManager catalogManager;
        private BattleSpriteCatalog catalog = BattleSpriteCatalog.Empty;
        private int readLeaseCount;
        private int readLeaseToken;
        private int nextReadLeaseToken;
        private int retired = 1;
        private int resourcesReleased = 1;

        internal BattleCentralSubmission(BattleDynamicMeshBackend backend)
        {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        public SimulationWorld World { get; private set; }
        public BattlePresentationFrame CapturedFrame { get; private set; }
        public int TickIndex { get; private set; }
        public int Generation { get; private set; }
        public BattleDynamicMeshBackend Backend { get; }
        public int ReadLeaseCount => Volatile.Read(ref readLeaseCount);
        public bool IsRetired => Volatile.Read(ref retired) != 0;
        internal bool IsReusable => IsRetired && ReadLeaseCount == 0;

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

            ReleaseCatalogBinding();
            World = world;
            CapturedFrame = capturedFrame;
            TickIndex = tickIndex;
            Generation = generation;
            catalogManager = manager;
            catalog = publishedCatalog ?? BattleSpriteCatalog.Empty;
            catalogManager?.RegisterRendererCatalogBinding(catalog);
            Volatile.Write(ref resourcesReleased, 0);
            Volatile.Write(ref retired, 0);
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
