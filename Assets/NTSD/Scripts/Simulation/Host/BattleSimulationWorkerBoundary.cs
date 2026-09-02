using System;
using System.Threading;

namespace NTSD.Simulation
{
    internal readonly struct BattleSimulationStageSnapshot
    {
        internal BattleSimulationStageSnapshot(
            int stageWidth,
            int zMin,
            int zMax,
            int perspectiveNear,
            int perspectiveFar)
        {
            StageWidth = stageWidth;
            ZMin = zMin;
            ZMax = zMax;
            PerspectiveNear = perspectiveNear;
            PerspectiveFar = perspectiveFar;
            IsValid = true;
        }

        internal int StageWidth { get; }
        internal int ZMin { get; }
        internal int ZMax { get; }
        internal int PerspectiveNear { get; }
        internal int PerspectiveFar { get; }
        internal bool IsValid { get; }

        internal static BattleSimulationStageSnapshot Capture(
            BattleStageRuntimeState stage)
        {
            return stage == null
                ? default
                : new BattleSimulationStageSnapshot(
                    stage.BaseStageWidthPx,
                    stage.ZMin,
                    stage.ZMax,
                    stage.PerspectiveNear,
                    stage.PerspectiveFar);
        }

        internal void Apply(BattleStageRuntimeState stage)
        {
            if (!IsValid || stage == null)
                return;

            stage.SetSceneSnapshot(
                StageWidth,
                ZMin,
                ZMax,
                PerspectiveNear,
                PerspectiveFar);
        }
    }

    internal readonly struct BattleSimulationTickRequest
    {
        internal BattleSimulationTickRequest(
            FrameInputSet frameInput,
            bool buildPresentation,
            in BattleSimulationStageSnapshot stage)
        {
            FrameInput = frameInput;
            BuildPresentation = buildPresentation;
            Stage = stage;
        }

        internal FrameInputSet FrameInput { get; }
        internal bool BuildPresentation { get; }
        internal BattleSimulationStageSnapshot Stage { get; }
    }

    internal sealed class BattleSimulationThreadOwnership
    {
        private readonly int mainThreadId;
        private int simulationThreadId;

        internal BattleSimulationThreadOwnership()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        internal int MainThreadId => mainThreadId;
        internal int SimulationThreadId => Volatile.Read(ref simulationThreadId);
        internal bool IsMainThread => Thread.CurrentThread.ManagedThreadId == mainThreadId;
        internal bool IsSimulationThread =>
            Thread.CurrentThread.ManagedThreadId == Volatile.Read(ref simulationThreadId);

        internal bool TryBindSimulationThread()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int existing = Interlocked.CompareExchange(
                ref simulationThreadId,
                currentThreadId,
                0);
            return existing == 0 || existing == currentThreadId;
        }

        internal void RequireMainThread()
        {
            if (!IsMainThread)
                throw new InvalidOperationException(
                    "Unity presentation and materialization must run on the main thread.");
        }

        internal void RequireSimulationThread()
        {
            if (!IsSimulationThread)
                throw new InvalidOperationException(
                    "The SimulationWorld owner is the dedicated simulation thread.");
        }
    }

    internal static class BattleSimulationExecutionContext
    {
        internal const string WorkerThreadName = "NTSD Battle Simulation";

        internal static bool IsSimulationWorkerThread =>
            string.Equals(
                Thread.CurrentThread.Name,
                WorkerThreadName,
                StringComparison.Ordinal);
    }

    internal sealed class BattleSimulationInputQueue
    {
        private sealed class Cell
        {
            internal readonly SimulationPlayerInput[] Players;
            internal readonly FrameInputSet Frame =
                FrameInputSetPreallocation.CreateReusable();
            internal BattleSimulationStageSnapshot Stage;
            internal bool BuildPresentation;

            internal Cell(int maximumPlayerCount)
            {
                Players = new SimulationPlayerInput[maximumPlayerCount];
            }
        }

        private readonly Cell[] cells;
        private readonly int maximumPlayerCount;
        private long readSequence;
        private long writeSequence;

        internal BattleSimulationInputQueue(int capacity, int maximumPlayerCount)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (maximumPlayerCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumPlayerCount));

            cells = new Cell[capacity];
            this.maximumPlayerCount = maximumPlayerCount;
            for (int index = 0; index < cells.Length; index++)
                cells[index] = new Cell(maximumPlayerCount);
        }

        internal int Capacity => cells.Length;
        internal int MaximumPlayerCount => maximumPlayerCount;
        internal long EnqueuedCount => Volatile.Read(ref writeSequence);
        internal long DequeuedCount => Volatile.Read(ref readSequence);
        internal int Count
        {
            get
            {
                long count = Volatile.Read(ref writeSequence) -
                             Volatile.Read(ref readSequence);
                return count <= 0 ? 0 : count >= cells.Length ? cells.Length : (int)count;
            }
        }

        internal bool TryEnqueue(FrameInputSet source)
        {
            BattleSimulationStageSnapshot stage = default;
            return TryEnqueue(source, buildPresentation: true, in stage);
        }

        internal bool TryEnqueue(
            FrameInputSet source,
            bool buildPresentation,
            in BattleSimulationStageSnapshot stage)
        {
            if (source == null || source.Players == null ||
                source.Players.Count > maximumPlayerCount)
            {
                return false;
            }

            long nextWrite = Volatile.Read(ref writeSequence);
            long observedRead = Volatile.Read(ref readSequence);
            if (nextWrite - observedRead >= cells.Length)
                return false;

            Cell cell = cells[(int)(nextWrite % cells.Length)];
            int playerCount = source.Players.Count;
            for (int index = 0; index < playerCount; index++)
                cell.Players[index] = source.Players[index];
            cell.Frame.ResetPreallocated(
                source.TickIndex,
                cell.Players,
                playerCount);
            cell.BuildPresentation = buildPresentation;
            cell.Stage = stage;

            Volatile.Write(ref writeSequence, nextWrite + 1);
            return true;
        }

        internal bool TryDequeue(
            FrameInputSet destination,
            SimulationPlayerInput[] destinationPlayers)
        {
            return TryDequeue(
                destination,
                destinationPlayers,
                out _);
        }

        internal bool TryDequeue(
            FrameInputSet destination,
            SimulationPlayerInput[] destinationPlayers,
            out BattleSimulationTickRequest request)
        {
            if (destination == null || destinationPlayers == null)
            {
                request = default;
                return false;
            }

            long nextRead = Volatile.Read(ref readSequence);
            if (nextRead >= Volatile.Read(ref writeSequence))
            {
                request = default;
                return false;
            }

            Cell cell = cells[(int)(nextRead % cells.Length)];
            int playerCount = cell.Frame.Players.Count;
            if (playerCount > destinationPlayers.Length)
            {
                request = default;
                return false;
            }

            for (int index = 0; index < playerCount; index++)
                destinationPlayers[index] = cell.Players[index];
            destination.ResetPreallocated(
                cell.Frame.TickIndex,
                destinationPlayers,
                playerCount);
            request = new BattleSimulationTickRequest(
                destination,
                cell.BuildPresentation,
                in cell.Stage);

            Volatile.Write(ref readSequence, nextRead + 1);
            return true;
        }

        internal void ResetWhenStopped()
        {
            if (Volatile.Read(ref readSequence) != Volatile.Read(ref writeSequence))
                throw new InvalidOperationException(
                    "The simulation input queue must be drained before reset.");

            Volatile.Write(ref readSequence, 0L);
            Volatile.Write(ref writeSequence, 0L);
            for (int index = 0; index < cells.Length; index++)
            {
                cells[index].Frame.ResetPreallocated(0, null);
                cells[index].Stage = default;
                cells[index].BuildPresentation = false;
            }
        }

        internal void DiscardAndResetWhenStopped()
        {
            long finalWrite = Volatile.Read(ref writeSequence);
            Volatile.Write(ref readSequence, finalWrite);
            ResetWhenStopped();
        }
    }

    internal readonly struct BattleSimulationTickPublication
    {
        internal BattleSimulationTickPublication(
            int tickIndex,
            ulong inputHash,
            ulong stateChecksum,
            bool hasStateChecksum,
            bool hasPresentationFrame = false,
            long executionElapsedTimestampTicks = 0L)
        {
            TickIndex = tickIndex;
            InputHash = inputHash;
            StateChecksum = stateChecksum;
            HasStateChecksum = hasStateChecksum;
            HasPresentationFrame = hasPresentationFrame;
            ExecutionElapsedTimestampTicks = executionElapsedTimestampTicks;
        }

        internal int TickIndex { get; }
        internal ulong InputHash { get; }
        internal ulong StateChecksum { get; }
        internal bool HasStateChecksum { get; }
        internal bool HasPresentationFrame { get; }
        internal long ExecutionElapsedTimestampTicks { get; }
    }

    internal sealed class BattleSimulationPublicationBuffer
    {
        private sealed class Cell
        {
            internal int TickIndex;
            internal ulong InputHash;
            internal ulong StateChecksum;
            internal bool HasStateChecksum;
            internal bool HasPresentationFrame;
            internal long ExecutionElapsedTimestampTicks;
        }

        private readonly Cell[] cells = { new Cell(), new Cell() };
        private long publishedSequence;

        internal long PublishedSequence => Volatile.Read(ref publishedSequence);

        internal void Publish(in BattleSimulationTickPublication publication)
        {
            long nextSequence = Volatile.Read(ref publishedSequence) + 1;
            Cell cell = cells[(int)(nextSequence & 1L)];
            cell.TickIndex = publication.TickIndex;
            cell.InputHash = publication.InputHash;
            cell.StateChecksum = publication.StateChecksum;
            cell.HasStateChecksum = publication.HasStateChecksum;
            cell.HasPresentationFrame = publication.HasPresentationFrame;
            cell.ExecutionElapsedTimestampTicks =
                publication.ExecutionElapsedTimestampTicks;
            Volatile.Write(ref publishedSequence, nextSequence);
        }

        internal bool TryReadLatest(
            ref long consumedSequence,
            out BattleSimulationTickPublication publication)
        {
            for (int attempt = 0; attempt < 4; attempt++)
            {
                long before = Volatile.Read(ref publishedSequence);
                if (before == 0 || before == consumedSequence)
                    break;

                Cell cell = cells[(int)(before & 1L)];
                var candidate = new BattleSimulationTickPublication(
                    cell.TickIndex,
                    cell.InputHash,
                    cell.StateChecksum,
                    cell.HasStateChecksum,
                    cell.HasPresentationFrame,
                    cell.ExecutionElapsedTimestampTicks);
                long after = Volatile.Read(ref publishedSequence);
                if (before != after)
                    continue;

                consumedSequence = after;
                publication = candidate;
                return true;
            }

            publication = default;
            return false;
        }
    }

    internal interface IBattleSimulationTickExecutor
    {
        BattleSimulationTickPublication Execute(in BattleSimulationTickRequest request);
        void OnPresentationConsumed(in BattleSimulationTickPublication publication);
    }

    internal sealed class BattleWorldSimulationTickExecutor : IBattleSimulationTickExecutor
    {
        private readonly SimulationWorld world;
        private readonly NTSDBattleTickSystem tickSystem;
        private readonly bool captureChecksum;
        private readonly BattleManagedMemoryBoundary managedMemoryBoundary;

        internal BattleWorldSimulationTickExecutor(
            SimulationWorld world,
            NTSDBattleTickSystem tickSystem,
            bool captureChecksum,
            BattleManagedMemoryBoundary managedMemoryBoundary = null)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.tickSystem = tickSystem ?? throw new ArgumentNullException(nameof(tickSystem));
            this.captureChecksum = captureChecksum;
            this.managedMemoryBoundary = managedMemoryBoundary;
        }

        public BattleSimulationTickPublication Execute(
            in BattleSimulationTickRequest request)
        {
            FrameInputSet frameInput = request.FrameInput;
            if (frameInput == null)
                throw new InvalidOperationException("The simulation worker request has no canonical input frame.");

            int tickIndex = frameInput.TickIndex;
            long executionStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            managedMemoryBoundary?.BeginSimulationWorkerTick();
            try
            {
                request.Stage.Apply(world.Runtime?.Stage);
                if (world.Runtime?.Flow != null)
                    world.Runtime.Flow.SparkRenderFrame = tickIndex;
                world.ApplyFrameInputSet(frameInput);
                tickSystem.RunSimulationWorkerTick(tickIndex, request.BuildPresentation);

                ulong checksum = captureChecksum
                    ? world.CaptureRuntimeChecksum64(tickIndex, frameInput)
                    : 0UL;
                return new BattleSimulationTickPublication(
                    tickIndex,
                    frameInput.GetCanonicalHash64(),
                    checksum,
                    captureChecksum,
                    request.BuildPresentation,
                    System.Diagnostics.Stopwatch.GetTimestamp() - executionStartedAt);
            }
            finally
            {
                managedMemoryBoundary?.ObserveAfterSimulationWorkerTick(tickIndex);
            }
        }

        public void OnPresentationConsumed(
            in BattleSimulationTickPublication publication)
        {
            if (publication.HasPresentationFrame)
                world.BattlePresentation.FinalizePublishedHitRecordCycle(world);
        }
    }

    internal sealed class DedicatedBattleSimulationWorker : IDisposable
    {
        private readonly BattleSimulationThreadOwnership ownership;
        private readonly BattleSimulationInputQueue inputQueue;
        private readonly BattleSimulationPublicationBuffer publicationBuffer;
        private readonly IBattleSimulationTickExecutor executor;
        private readonly SimulationPlayerInput[] workerPlayers;
        private readonly FrameInputSet workerFrame =
            FrameInputSetPreallocation.CreateReusable();
        private readonly AutoResetEvent inputAvailable = new AutoResetEvent(false);
        private readonly AutoResetEvent publicationConsumed = new AutoResetEvent(false);
        private Thread workerThread;
        private Exception failure;
        private long acknowledgedPublicationSequence;
        private long finalizedPublicationSequence;
        private int stopRequested;
        private int running;
        private int disposed;

        internal DedicatedBattleSimulationWorker(
            int inputCapacity,
            int maximumPlayerCount,
            IBattleSimulationTickExecutor executor)
        {
            this.executor = executor ?? throw new ArgumentNullException(nameof(executor));
            ownership = new BattleSimulationThreadOwnership();
            inputQueue = new BattleSimulationInputQueue(
                inputCapacity,
                maximumPlayerCount);
            publicationBuffer = new BattleSimulationPublicationBuffer();
            workerPlayers = new SimulationPlayerInput[maximumPlayerCount];
        }

        internal BattleSimulationThreadOwnership Ownership => ownership;
        internal bool IsRunning => Volatile.Read(ref running) != 0;
        internal int PendingInputCount => inputQueue.Count;
        internal long PublishedSequence => publicationBuffer.PublishedSequence;

        internal bool IsPresentationConsumptionFinalized(long publicationSequence)
        {
            return publicationSequence > 0 &&
                   Volatile.Read(ref finalizedPublicationSequence) >= publicationSequence;
        }
        internal Exception Failure => Volatile.Read(ref failure);

        internal void Start()
        {
            ThrowIfDisposed();
            if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
                throw new InvalidOperationException("The simulation worker is already running.");

            Volatile.Write(ref stopRequested, 0);
            Volatile.Write(ref failure, null);
            workerThread = new Thread(Run)
            {
                IsBackground = true,
                Name = BattleSimulationExecutionContext.WorkerThreadName,
            };
            workerThread.Start();
            if (!SpinWait.SpinUntil(
                    () => ownership.SimulationThreadId != 0 || Failure != null,
                    5000))
            {
                Stop();
                throw new TimeoutException(
                    "The dedicated simulation thread did not acquire ownership within five seconds.");
            }
            if (Failure != null)
            {
                Exception startupFailure = Failure;
                Stop();
                throw new InvalidOperationException(
                    "The dedicated simulation thread failed during startup.",
                    startupFailure);
            }
        }

        internal bool TrySubmit(FrameInputSet frameInput)
        {
            BattleSimulationStageSnapshot stage = default;
            return TrySubmit(frameInput, buildPresentation: true, in stage);
        }

        internal bool TrySubmit(
            FrameInputSet frameInput,
            bool buildPresentation,
            in BattleSimulationStageSnapshot stage)
        {
            ThrowIfDisposed();
            if (!IsRunning || Failure != null ||
                Volatile.Read(ref stopRequested) != 0 ||
                !inputQueue.TryEnqueue(frameInput, buildPresentation, in stage))
            {
                return false;
            }

            inputAvailable.Set();
            return true;
        }

        internal bool TryReadLatest(
            ref long consumedSequence,
            out BattleSimulationTickPublication publication)
        {
            ThrowIfDisposed();
            return publicationBuffer.TryReadLatest(
                ref consumedSequence,
                out publication);
        }

        internal void AcknowledgePresentationConsumed(long publicationSequence)
        {
            ThrowIfDisposed();
            if (publicationSequence <= 0 ||
                publicationSequence > publicationBuffer.PublishedSequence)
            {
                throw new ArgumentOutOfRangeException(nameof(publicationSequence));
            }

            long observed = Volatile.Read(ref acknowledgedPublicationSequence);
            while (publicationSequence > observed)
            {
                long previous = Interlocked.CompareExchange(
                    ref acknowledgedPublicationSequence,
                    publicationSequence,
                    observed);
                if (previous == observed)
                    break;
                observed = previous;
            }
            publicationConsumed.Set();
        }

        internal void Stop()
        {
            if (Interlocked.Exchange(ref stopRequested, 1) == 0)
            {
                inputAvailable.Set();
                publicationConsumed.Set();
            }

            Thread thread = workerThread;
            if (thread != null && thread != Thread.CurrentThread)
                thread.Join();
            workerThread = null;
            Volatile.Write(ref running, 0);
            inputQueue.DiscardAndResetWhenStopped();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
                return;
            Stop();
            inputAvailable.Dispose();
            publicationConsumed.Dispose();
        }

        private void Run()
        {
            try
            {
                if (!ownership.TryBindSimulationThread())
                {
                    throw new InvalidOperationException(
                        "The dedicated simulation thread could not acquire world ownership.");
                }

                while (Volatile.Read(ref stopRequested) == 0)
                {
                    if (!inputQueue.TryDequeue(
                            workerFrame,
                            workerPlayers,
                            out BattleSimulationTickRequest request))
                    {
                        inputAvailable.WaitOne();
                        continue;
                    }

                    BattleSimulationTickPublication publication =
                        executor.Execute(in request);
                    if (publication.TickIndex != workerFrame.TickIndex)
                    {
                        throw new InvalidOperationException(
                            "The simulation executor published a different tick than the canonical input frame.");
                    }

                    publicationBuffer.Publish(in publication);
                    long publishedSequence = publicationBuffer.PublishedSequence;
                    while (Volatile.Read(ref stopRequested) == 0 &&
                           Volatile.Read(ref acknowledgedPublicationSequence) < publishedSequence)
                    {
                        publicationConsumed.WaitOne();
                    }
                    if (Volatile.Read(ref acknowledgedPublicationSequence) >= publishedSequence)
                    {
                        executor.OnPresentationConsumed(in publication);
                        Volatile.Write(ref finalizedPublicationSequence, publishedSequence);
                    }
                }
            }
            catch (Exception exception)
            {
                Volatile.Write(ref failure, exception);
                Volatile.Write(ref stopRequested, 1);
            }
            finally
            {
                Volatile.Write(ref running, 0);
            }
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref disposed) != 0)
                throw new ObjectDisposedException(nameof(DedicatedBattleSimulationWorker));
        }
    }
}
