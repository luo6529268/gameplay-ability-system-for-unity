using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Simulation.Ecs
{
    internal interface IBattleObjectPointStructuralMaterializer
    {
        void FlushTasks();
        void ProcessOpointSpawnCoreForStructuralWriter(LF2Entity spawner);
        LF2Entity MaterializeObjectForStructuralWriter(OPointCreateTask task);
        void MaterializeMultipleObjectsForStructuralWriter(
            OPointCreateMultipleTask task);
    }

    public enum BattleStructuralCommandType
    {
        None = 0,
        Spawn = 1,
        SpawnMultiple = 2,
        Register = 3,
        Unregister = 4,
        Free = 5,
        Destroy = 6,
        GenerationClaim = 7,
        GenerationRelease = 8,
    }

    public enum BattleStructuralPlaybackBoundary
    {
        None = 0,
        CurrentEntityImmediate = 1,
        CurrentPassSegment = 2,
        NextPass = 3,
        TickEnd = 4,
        DeferredUnregisterFree = 5,
    }

    public readonly struct BattleStructuralCommand
    {
        internal BattleStructuralCommand(
            BattleStructuralCommandType type,
            RuntimeEntityHandle source,
            RuntimeEntityHandle target,
            int oid,
            int requestedSlot,
            BattleStructuralPlaybackBoundary playbackBoundary,
            int tick,
            int authorityOrdinal)
        {
            Type = type;
            Source = source;
            Target = target;
            Oid = oid;
            RequestedSlot = requestedSlot;
            PlaybackBoundary = playbackBoundary;
            Tick = tick;
            AuthorityOrdinal = authorityOrdinal;
        }

        public BattleStructuralCommandType Type { get; }
        public RuntimeEntityHandle Source { get; }
        public RuntimeEntityHandle Target { get; }
        public int Oid { get; }
        public int RequestedSlot { get; }
        public BattleStructuralPlaybackBoundary PlaybackBoundary { get; }
        public int Tick { get; }
        public int AuthorityOrdinal { get; }
    }

    public readonly struct BattleStructuralWriterDiagnostics
    {
        internal BattleStructuralWriterDiagnostics(
            long commandCount,
            long spawnCount,
            long registerCount,
            long unregisterCount,
            long freeCount,
            long destroyCount,
            long generationClaimCount,
            long generationReleaseCount,
            int lastTick,
            int lastAuthorityOrdinal,
            BattleStructuralCommandType lastCommandType,
            BattleStructuralPlaybackBoundary lastBoundary,
            RuntimeEntityHandle lastSource,
            int lastRequestedSlot,
            int lastOid,
            BattleStructuralPlaybackBoundary lastSpawnBoundary,
            int lastSpawnAuthorityOrdinal,
            RuntimeEntityHandle lastSpawnSource,
            BattleStructuralCommand lastCommand)
        {
            CommandCount = commandCount;
            SpawnCount = spawnCount;
            RegisterCount = registerCount;
            UnregisterCount = unregisterCount;
            FreeCount = freeCount;
            DestroyCount = destroyCount;
            GenerationClaimCount = generationClaimCount;
            GenerationReleaseCount = generationReleaseCount;
            LastTick = lastTick;
            LastAuthorityOrdinal = lastAuthorityOrdinal;
            LastCommandType = lastCommandType;
            LastBoundary = lastBoundary;
            LastSource = lastSource;
            LastRequestedSlot = lastRequestedSlot;
            LastOid = lastOid;
            LastSpawnBoundary = lastSpawnBoundary;
            LastSpawnAuthorityOrdinal = lastSpawnAuthorityOrdinal;
            LastSpawnSource = lastSpawnSource;
            LastCommand = lastCommand;
        }

        public long CommandCount { get; }
        public long SpawnCount { get; }
        public long RegisterCount { get; }
        public long UnregisterCount { get; }
        public long FreeCount { get; }
        public long DestroyCount { get; }
        public long GenerationClaimCount { get; }
        public long GenerationReleaseCount { get; }
        public int LastTick { get; }
        public int LastAuthorityOrdinal { get; }
        public BattleStructuralCommandType LastCommandType { get; }
        public BattleStructuralPlaybackBoundary LastBoundary { get; }
        public RuntimeEntityHandle LastSource { get; }
        public int LastRequestedSlot { get; }
        public int LastOid { get; }
        public BattleStructuralPlaybackBoundary LastSpawnBoundary { get; }
        public int LastSpawnAuthorityOrdinal { get; }
        public RuntimeEntityHandle LastSpawnSource { get; }
        public BattleStructuralCommand LastCommand { get; }
    }

    /// <summary>
    /// Owns battle structural mutations while preserving the authority's exact
    /// playback boundary. It deliberately performs cursor-local opoint playback
    /// instead of collapsing all structural work into a tick-end buffer.
    /// </summary>
    internal sealed class BattleStructuralWriter
    {
        private readonly SimulationWorld world;
        private int activeTick = int.MinValue;
        private int authorityOrdinal;
        private int playbackTickOverride;
        private int playbackTickOverrideDepth;
        private long commandCount;
        private long spawnCount;
        private long registerCount;
        private long unregisterCount;
        private long freeCount;
        private long destroyCount;
        private long generationClaimCount;
        private long generationReleaseCount;
        private BattleStructuralCommandType lastCommandType;
        private BattleStructuralPlaybackBoundary lastBoundary;
        private RuntimeEntityHandle lastSource;
        private int lastRequestedSlot = -1;
        private int lastOid = -1;
        private BattleStructuralPlaybackBoundary lastSpawnBoundary;
        private int lastSpawnAuthorityOrdinal;
        private RuntimeEntityHandle lastSpawnSource;
        private BattleStructuralCommand lastCommand;

        internal BattleStructuralWriter(SimulationWorld world)
        {
            this.world = world;
        }

        internal BattleStructuralWriterDiagnostics Diagnostics =>
            new BattleStructuralWriterDiagnostics(
                commandCount,
                spawnCount,
                registerCount,
                unregisterCount,
                freeCount,
                destroyCount,
                generationClaimCount,
                generationReleaseCount,
                activeTick == int.MinValue ? -1 : activeTick,
                authorityOrdinal,
                lastCommandType,
                lastBoundary,
                lastSource,
                lastRequestedSlot,
                lastOid,
                lastSpawnBoundary,
                lastSpawnAuthorityOrdinal,
                lastSpawnSource,
                lastCommand);

        internal void ProcessLateOpointSegment(
            IBattleObjectPointStructuralMaterializer factory,
            LF2Entity spawner,
            int tickIndex)
        {
            if (factory == null || spawner?.Runtime == null)
                return;

            BeginTick(tickIndex);
            int previousOverride = playbackTickOverride;
            playbackTickOverride = tickIndex;
            playbackTickOverrideDepth++;
            try
            {
                factory.ProcessOpointSpawnCoreForStructuralWriter(spawner);
            }
            finally
            {
                playbackTickOverrideDepth--;
                playbackTickOverride = previousOverride;
            }
        }

        internal LF2Entity Spawn(
            IBattleObjectPointStructuralMaterializer factory,
            OPointCreateTask task,
            BattleStructuralPlaybackBoundary boundary)
        {
            if (factory == null || task == null)
                return null;

            task.targetWorld = world;
            RuntimeEntityHandle source = ResolveSource(task.parent);
            BattleStructuralCommand command = Record(
                BattleStructuralCommandType.Spawn,
                boundary,
                source,
                task.requiredRuntimeSlot,
                task.opoint.oid);
            lastSpawnBoundary = boundary;
            lastSpawnAuthorityOrdinal = command.AuthorityOrdinal;
            lastSpawnSource = source;
            spawnCount++;
            return factory.MaterializeObjectForStructuralWriter(task);
        }

        internal void SpawnMultiple(
            IBattleObjectPointStructuralMaterializer factory,
            OPointCreateMultipleTask task,
            BattleStructuralPlaybackBoundary boundary)
        {
            if (factory == null || task == null)
                return;

            task.targetWorld = world;
            RuntimeEntityHandle source = ResolveSource(task.parent);
            BattleStructuralCommand command = Record(
                BattleStructuralCommandType.SpawnMultiple,
                boundary,
                source,
                -1,
                task.opoint.oid);
            lastSpawnBoundary = boundary;
            lastSpawnAuthorityOrdinal = command.AuthorityOrdinal;
            lastSpawnSource = source;
            spawnCount++;
            factory.MaterializeMultipleObjectsForStructuralWriter(task);
        }

        internal void Register(ISimObject obj)
        {
            Record(
                BattleStructuralCommandType.Register,
                BattleStructuralPlaybackBoundary.CurrentEntityImmediate,
                ResolveSource(obj as LF2Entity),
                obj is LF2Entity entity ? entity.RequiredRuntimeSlot : -1,
                obj is LF2Entity living ? living.ObjectId : -1);
            registerCount++;
            world.RegisterCoreFromStructuralWriter(obj);
        }

        internal void Unregister(ISimObject obj)
        {
            BattleStructuralPlaybackBoundary boundary =
                world.IsTickingForStructuralWriter
                    ? BattleStructuralPlaybackBoundary.DeferredUnregisterFree
                    : BattleStructuralPlaybackBoundary.CurrentEntityImmediate;
            Record(
                BattleStructuralCommandType.Unregister,
                boundary,
                ResolveSource(obj as LF2Entity),
                obj is LF2Entity entity ? entity.Runtime?.SlotIndex ?? -1 : -1,
                obj is LF2Entity living ? living.ObjectId : -1);
            unregisterCount++;
            world.UnregisterCoreFromStructuralWriter(obj);
        }

        internal void Free(LF2Entity entity)
        {
            if (entity == null)
                return;

            Record(
                BattleStructuralCommandType.Free,
                BattleStructuralPlaybackBoundary.CurrentEntityImmediate,
                ResolveSource(entity),
                entity.Runtime?.SlotIndex ?? -1,
                entity.ObjectId);
            freeCount++;
            entity.FreeEntityLikeExeCoreForStructuralWriter();
        }

        internal void Destroy(LF2Entity entity)
        {
            if (entity == null)
                return;

            Record(
                BattleStructuralCommandType.Destroy,
                BattleStructuralPlaybackBoundary.CurrentEntityImmediate,
                ResolveSource(entity),
                entity.Runtime?.SlotIndex ?? -1,
                entity.ObjectId);
            destroyCount++;
            entity.DestroyEntityLikeExeCoreForStructuralWriter();
        }

        internal void RecordGenerationClaim(LF2Entity entity, int slot)
        {
            Record(
                BattleStructuralCommandType.GenerationClaim,
                BattleStructuralPlaybackBoundary.CurrentEntityImmediate,
                ResolveSource(entity),
                slot,
                entity?.ObjectId ?? -1);
            generationClaimCount++;
        }

        internal void RecordGenerationRelease(
            LF2Entity entity,
            RuntimeEntityHandle releasedHandle)
        {
            Record(
                BattleStructuralCommandType.GenerationRelease,
                world.IsTickingForStructuralWriter
                    ? BattleStructuralPlaybackBoundary.DeferredUnregisterFree
                    : BattleStructuralPlaybackBoundary.CurrentEntityImmediate,
                releasedHandle,
                releasedHandle.Slot,
                entity?.ObjectId ?? -1);
            generationReleaseCount++;
        }

        private RuntimeEntityHandle ResolveSource(LF2Entity entity)
        {
            if (entity?.Runtime == null)
                return RuntimeEntityHandle.Invalid;
            int slot = entity.Runtime.SlotIndex;
            return world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle)
                ? handle
                : RuntimeEntityHandle.Invalid;
        }

        private void BeginTick(int tickIndex)
        {
            if (activeTick == tickIndex)
                return;
            activeTick = tickIndex;
            authorityOrdinal = 0;
        }

        private BattleStructuralCommand Record(
            BattleStructuralCommandType type,
            BattleStructuralPlaybackBoundary boundary,
            RuntimeEntityHandle source,
            int requestedSlot,
            int oid)
        {
            int tick = playbackTickOverrideDepth > 0
                ? playbackTickOverride
                : world.CurrentTickIndex;
            BeginTick(tick);
            authorityOrdinal++;
            commandCount++;
            lastCommandType = type;
            lastBoundary = boundary;
            lastSource = source;
            lastRequestedSlot = requestedSlot;
            lastOid = oid;
            lastCommand = new BattleStructuralCommand(
                type,
                source,
                RuntimeEntityHandle.Invalid,
                oid,
                requestedSlot,
                boundary,
                tick,
                authorityOrdinal);
            return lastCommand;
        }
    }
}
