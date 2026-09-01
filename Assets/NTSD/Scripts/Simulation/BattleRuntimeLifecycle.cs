namespace NTSD.Simulation
{
    public enum BattleRuntimeLifecycleState : byte
    {
        Uninitialized = 0,
        Preparing = 1,
        Running = 2,
        Stopping = 3,
        Stopped = 4,
    }

    public enum BattleRuntimeShutdownStage : byte
    {
        None = 0,
        TickAndInputClosed = 1,
        WorkerStopped = 2,
        SpawnIntakeClosed = 3,
        AllocationUnsealed = 4,
        PresentationCleared = 5,
        PendingObjectPointTasksDiscarded = 6,
        RenderersReturned = 7,
        WorldLogicCleared = 8,
        WorldUnbound = 9,
        ObjectPoolQuiesced = 10,
        RuntimeMapCleared = 11,
    }

    public enum BattleRuntimeShutdownStatus : byte
    {
        AwaitingRuntimeMapCleanup = 0,
        Completed = 1,
        AlreadyStopping = 2,
        AlreadyStopped = 3,
        Failed = 4,
    }

    public readonly struct BattleRuntimeShutdownReport
    {
        internal BattleRuntimeShutdownReport(
            BattleRuntimeShutdownStatus status,
            BattleRuntimeShutdownStage completedStage,
            string failureReason,
            int discardedObjectPointTasks,
            int returnedRenderers,
            int returnedSpriteRenderers,
            int remainingWorldObjects,
            int remainingRuntimeSlots,
            int remainingPoolBorrowers)
        {
            Status = status;
            CompletedStage = completedStage;
            FailureReason = failureReason ?? string.Empty;
            DiscardedObjectPointTasks = discardedObjectPointTasks;
            ReturnedRenderers = returnedRenderers;
            ReturnedSpriteRenderers = returnedSpriteRenderers;
            RemainingWorldObjects = remainingWorldObjects;
            RemainingRuntimeSlots = remainingRuntimeSlots;
            RemainingPoolBorrowers = remainingPoolBorrowers;
        }

        public BattleRuntimeShutdownStatus Status { get; }
        public BattleRuntimeShutdownStage CompletedStage { get; }
        public string FailureReason { get; }
        public int DiscardedObjectPointTasks { get; }
        public int ReturnedRenderers { get; }
        public int ReturnedSpriteRenderers { get; }
        public int RemainingWorldObjects { get; }
        public int RemainingRuntimeSlots { get; }
        public int RemainingPoolBorrowers { get; }

        public bool RuntimeStagesCompleted =>
            CompletedStage >= BattleRuntimeShutdownStage.ObjectPoolQuiesced &&
            Status != BattleRuntimeShutdownStatus.Failed;

        public bool IsComplete =>
            Status == BattleRuntimeShutdownStatus.Completed ||
            Status == BattleRuntimeShutdownStatus.AlreadyStopped;
    }

    internal sealed class BattleRuntimeShutdownDiagnostics
    {
        internal BattleRuntimeShutdownStage CompletedStage { get; private set; }
        internal string FailureReason { get; private set; } = string.Empty;
        internal int DiscardedObjectPointTasks { get; set; }
        internal int ReturnedRenderers { get; set; }
        internal int ReturnedSpriteRenderers { get; set; }

        internal void Reset()
        {
            CompletedStage = BattleRuntimeShutdownStage.None;
            FailureReason = string.Empty;
            DiscardedObjectPointTasks = 0;
            ReturnedRenderers = 0;
            ReturnedSpriteRenderers = 0;
        }

        internal void Complete(BattleRuntimeShutdownStage stage)
        {
            if (stage > CompletedStage)
                CompletedStage = stage;
        }

        internal void Fail(BattleRuntimeShutdownStage stage, string reason)
        {
            Complete(stage);
            FailureReason = reason ?? string.Empty;
        }

        internal void ClearFailure()
        {
            FailureReason = string.Empty;
        }
    }
}
