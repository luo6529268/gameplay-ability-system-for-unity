using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 注册、运行时槽位和基础上下文。
    /// </summary>
    public partial class SimulationWorld
    {
        private sealed class RuntimeStableIdComparer : IComparer<ISimObject>
        {
            public int Compare(ISimObject left, ISimObject right)
            {
                int leftStableId = left is LF2Entity leftEntity
                    ? leftEntity.Runtime.StableId
                    : left?.StableId ?? int.MinValue;
                int rightStableId = right is LF2Entity rightEntity
                    ? rightEntity.Runtime.StableId
                    : right?.StableId ?? int.MinValue;
                return leftStableId.CompareTo(rightStableId);
            }
        }

        /// <summary>
        /// Compatibility lookup for diagnostics. Ordered traversal and bucket
        /// lifetime are owned by objectBucketRegistry.
        /// </summary>
        private readonly Dictionary<int, SimulationObjectBucket> _buckets;
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
        private readonly IComparer<ISimObject> runtimeStableIdComparer =
            new RuntimeStableIdComparer();
        private readonly int maxActiveRuntimeEntities;
        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private List<ISimObject> _pendingUnregister => battleBuffers.PendingUnregister;
        private List<LF2Entity> _pendingSlotReleasedDestroy =>
            battleBuffers.PendingSlotReleasedDestroy;
        private Dictionary<ISimObject, int> structuralPendingUnregisterSlots;
        private IBattleParityStructuralEventSink structuralEventSink;
        private int structuralEventTick;
        private string structuralEventPass = string.Empty;
        private int structuralEventCursorSlot = -1;
        private bool pendingDestroyScanCacheValid;
        private long pendingDestroyScanMutationEpoch;
        private ulong pendingDestroyScanOccupancyEpoch;
        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking = false;
        private List<LF2Entity> _entityScratch => battleBuffers.EntityScratch;
#if UNITY_EDITOR
        private readonly HashSet<LF2Entity> activeRuntimeEntitySnapshotSeen =
            new HashSet<LF2Entity>();
        private readonly System.Comparison<LF2Entity>
            activeRuntimeEntitySnapshotComparison;
#endif
        private int _cameraX;
        private int _cameraVel;

        public int ReleaseCameraX => _cameraX;
        internal int ReleaseCameraVelocityForServices => _cameraVel;
        internal bool IsUnityFixedWorldCameraStateClear => _cameraX == 0 && _cameraVel == 0;
        internal int RuntimeSlotCapacity => _runtimeSlots.LogicalCapacity;
        internal RuntimeSlotTable RuntimeSlotTableForModules => _runtimeSlots;
        internal int MaxRuntimeSlotsForServices => RuntimeSlotCapacity;
        internal int DynamicRuntimeSlotStartForServices => DynamicRuntimeSlotStart;
        internal BattleRuntimeProfile RuntimeProfileForServices => activeRuntimeProfile;
        internal CollisionBroadphaseBackend CollisionBroadphaseForServices { get; }
        internal int ClaimedRuntimeSlotCountForServices => _runtimeSlots.ClaimedCount;
        internal ulong RuntimeSlotOccupancyEpochForServices =>
            _runtimeSlots.OccupancyEpoch;
        public BattleRuntimeProfile RuntimeProfileForDiagnostics => activeRuntimeProfile;
        public int RuntimeSlotCapacityForDiagnostics => _runtimeSlots.LogicalCapacity;
        public CollisionBroadphaseBackend CollisionBroadphaseForDiagnostics =>
            CollisionBroadphaseForServices;
        public int ClaimedRuntimeSlotCountForDiagnostics => _runtimeSlots.ClaimedCount;
        public long PendingDestroyFullScanCount { get; private set; }
        public long PendingDestroySkipCount { get; private set; }
        public long PendingDestroyVisitedEntityCount { get; private set; }
        public long NullRegistrationRejectCountForDiagnostics { get; private set; }
        public long BucketCapacityRejectCountForDiagnostics { get; private set; }
        public long DuplicateRegistrationRejectCountForDiagnostics { get; private set; }
        public long RuntimeSlotCapacityRejectCountForDiagnostics { get; private set; }
        public long RuntimeRestBindRejectCountForDiagnostics { get; private set; }
        public long StableIdRegistrationRejectCountForDiagnostics { get; private set; }
        public long MissingUnregisterCountForDiagnostics { get; private set; }
        public long RuntimeSlotReleaseRejectCountForDiagnostics { get; private set; }
        public long RejectedVRestWriteCountForDiagnostics =>
            _runtimeRestStore.RejectedVRestWriteCount;
        public long RejectedSoundEventCountForDiagnostics =>
            battleBuffers.RejectedSoundEventCount;
        public bool ForceLegacyPendingDestroyScanForDiagnostics { get; set; }
        public bool EnableRegistryLifecycleLoggingForDiagnostics { get; set; } = false;
        internal RuntimeRestStore RuntimeRestStoreForServices => _runtimeRestStore;
        internal RuntimeSlotTable RuntimeSlotsForServices => _runtimeSlots;
        internal IBattleParityStructuralEventSink StructuralEventSinkForServices =>
            structuralEventSink;

        public void SetStructuralEventSinkForDiagnostics(
            IBattleParityStructuralEventSink sink,
            int tick,
            string pass)
        {
            structuralEventSink = sink;
            structuralEventTick = tick;
            structuralEventPass = pass ?? string.Empty;
            structuralEventCursorSlot = -1;
            if (sink != null)
            {
                if (structuralPendingUnregisterSlots == null)
                    structuralPendingUnregisterSlots = new Dictionary<ISimObject, int>();
                else
                    structuralPendingUnregisterSlots.Clear();
            }
            else
            {
                structuralPendingUnregisterSlots = null;
            }
        }

        public void SetStructuralEventContextForDiagnostics(int tick, string pass)
        {
            if (structuralEventSink == null)
                return;
            structuralEventTick = tick;
            structuralEventPass = pass ?? string.Empty;
            structuralEventCursorSlot = -1;
        }

        public int FindFirstFreeRuntimeSlotForDiagnostics(
            int startSlot,
            int endSlotExclusive)
        {
            return FindFirstFreeRuntimeSlot(startSlot, endSlotExclusive);
        }

        private void EmitStructuralEvent(
            string action,
            int slot,
            int searchStart,
            int searchEndExclusive,
            string before,
            string after,
            string sourceKind,
            int actorSlot = -1)
        {
            IBattleParityStructuralEventSink sink = structuralEventSink;
            if (sink == null)
                return;

            sink.Record(new BattleParityStructuralEvent
            {
                Tick = structuralEventTick,
                Pass = structuralEventPass,
                Action = action,
                CursorSlot = structuralEventCursorSlot,
                ActorSlot = actorSlot >= 0 ? actorSlot : structuralEventCursorSlot,
                Slot = slot,
                SearchStart = searchStart,
                SearchEndExclusive = searchEndExclusive,
                Before = before,
                After = after,
                SourceKind = sourceKind,
            });
        }

        private static string StructuralSourceKind(LF2Entity entity)
        {
            if (entity?.Runtime?.SpawnSemantic ==
                (int)ReleaseSpawnSemantic.StageSpawnAt)
            {
                return "stage";
            }
            return entity != null && entity.UsesDynamicRuntimeSlot()
                ? "dynamic"
                : "general";
        }

        private bool ContainsRegisteredEntityStableId(int stableId)
        {
            for (int bucketIndex = 0;
                 bucketIndex < objectBucketRegistry.OrderedCount;
                 bucketIndex++)
            {
                List<ISimObject> items =
                    objectBucketRegistry.GetOrderedBucket(bucketIndex).items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] is LF2Entity entity &&
                        entity.Runtime.StableId == stableId)
                    {
                        return true;
                    }
                }
            }

            return false;
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
            RuntimeCharacterConfigResolver characterConfigResolver)
            : this(
                BattleRuntimeProfile.Authority400,
                AuthorityRuntimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                characterConfigResolver)
        {
        }

        public SimulationWorld(
            BattleRuntimeProfile runtimeProfile,
            int runtimeSlotCapacity,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce,
            RuntimeCharacterConfigResolver characterConfigResolver = null)
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
            objectBucketRegistry = new SimulationObjectBucketRegistry();
            _buckets = objectBucketRegistry.LookupForCompatibility;
            _runtimeSlots = new RuntimeSlotTable(runtimeSlotCapacity, 20, DynamicRuntimeSlotStart);
            _runtimeRestStore = new RuntimeRestStore(runtimeSlotCapacity);
            battleBuffers = new SimulationBattleBufferModule(runtimeSlotCapacity);
            runtimeCapacityModule = new SimulationRuntimeCapacityModule(
                _runtimeSlots,
                _runtimeRestStore,
                battleBuffers,
                objectBucketRegistry);
            frameInputModule = new SimulationFrameInputModule(this);
            stageSpawnTaskConfigurator = new StageSpawnTaskConfigurator();
            runtimeCharacterConfigs =
                characterConfigResolver ?? new RuntimeCharacterConfigResolver();
            entityTraversal = new SimulationEntityTraversal(this, _runtimeSlots);
            queryAndLinkModule = new SimulationQueryAndLinkModule(this);
            randomWeaponDropBuffer = new SimulationRandomWeaponDropBuffer();
            lockstepChecksumModule = new BattleLockstepChecksumModule();
            stageWaveModule = new SimulationStageWaveModule(this);
            stageRenderModule = new SimulationStageRenderModule(this);
            paritySnapshotModule = new BattleParitySnapshotModule(this);
            aiInputSlots = new LF2Entity[runtimeSlotCapacity];
            InitializeAiSoASensingRows(runtimeSlotCapacity);
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this, collisionBroadphase);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
#if UNITY_EDITOR
            activeRuntimeEntitySnapshotComparison = CompareRuntimeSlotOrder;
#endif
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

#if UNITY_EDITOR
        /// <summary>
        /// Returns currently claimed, pass-active runtime entities by resolving a fresh
        /// generation-checked handle for every runtime slot. This intentionally does
        /// not use bucket/pass queries so diagnostic cleanup can find leaked entries.
        /// </summary>
        public void GetActiveRuntimeEntitySnapshotForDiagnostics(List<LF2Entity> dst)
        {
            if (dst == null)
                return;

            dst.Clear();
            activeRuntimeEntitySnapshotSeen.Clear();
            try
            {
                for (int runtimeSlot = 0;
                     runtimeSlot < RuntimeSlotCapacity;
                     runtimeSlot++)
                {
                    RuntimeSlotTable.ReadOnlySlotView view =
                        _runtimeSlots.GetReadOnlyView(runtimeSlot);
                    if (!view.Claimed || view.Entity == null || view.Generation == 0)
                        continue;

                    var handle = new RuntimeEntityHandle(runtimeSlot, view.Generation);
                    if (!_runtimeSlots.TryResolve(handle, out LF2Entity entity) ||
                        entity == null ||
                        entity.Runtime?.PendingFlushDestroy == true ||
                        !activeRuntimeEntitySnapshotSeen.Add(entity))
                    {
                        continue;
                    }

                    dst.Add(entity);
                }

                dst.Sort(activeRuntimeEntitySnapshotComparison);
            }
            finally
            {
                activeRuntimeEntitySnapshotSeen.Clear();
            }
        }

        /// <summary>
        /// Forces the same delayed destroy release boundary used by simulation passes.
        /// It never resets the world and is only valid outside a running pass.
        /// </summary>
        public void FlushPendingDestroyForDiagnostics()
        {
            if (_ticking)
                throw new System.InvalidOperationException(
                    "Diagnostic destroy flushing cannot run while SimulationWorld is ticking.");

            ReleasePendingDestroySlots();
            FlushPendingUnregister();
            FlushPendingEntityDestroy();
            FlushPendingUnregister();
        }
#endif

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
            EnsureAiSensingModeAvailableBeforeTick();
            stageRenderModule.Reset();
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

            HashSet<ISimObject> registeredObjects =
                battleBuffers.RegisteredObjectResetSet;
            registeredObjects.Clear();
            for (int bucketIndex = 0;
                 bucketIndex < objectBucketRegistry.OrderedCount;
                 bucketIndex++)
            {
                SimulationObjectBucket bucket =
                    objectBucketRegistry.GetOrderedBucket(bucketIndex);
                for (int itemIndex = 0;
                     itemIndex < bucket.items.Count;
                     itemIndex++)
                {
                    ISimObject item = bucket.items[itemIndex];
                    if (item != null)
                        registeredObjects.Add(item);
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
                entity.Runtime?.BindWorldMutationTracker(null);
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

            objectBucketRegistry.Clear();
            _runtimeSlots.Reset();
            _runtimeRestStore.ResetWorld();
            registeredObjects.Clear();
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
                NullRegistrationRejectCountForDiagnostics++;
                if (!runtimeCapacityModule.IsSealed)
                    Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

            // A pooled instance can be reused during the same dynamic late-slot scan.
            // Finalize its queued old lifecycle before registering the new one, and
            // remove the pending entry so the pass-finally flush cannot delete it.
            if (_pendingUnregister.Remove(obj))
                UnregisterImmediate(obj);

            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out SimulationObjectBucket bucket))
            {
                bucket = objectBucketRegistry.GetOrCreate(simOrder);
                if (bucket == null)
                {
                    BucketCapacityRejectCountForDiagnostics++;
                    if (!runtimeCapacityModule.IsSealed)
                    {
                        Debug.LogError(
                            $"[SimulationWorld] Registration rejected because the sealed " +
                            $"simulation bucket pool is exhausted: SimOrder={simOrder}, " +
                            $"StableId={obj.StableId}");
                    }
                    return;
                }
            }

            if (bucket.items.Contains(obj))
            {
                DuplicateRegistrationRejectCountForDiagnostics++;
                if (!runtimeCapacityModule.IsSealed)
                {
                    Debug.LogWarning(
                        $"[SimulationWorld] Object already registered: " +
                        $"SimOrder={simOrder}, StableId={obj.StableId}");
                }
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
                    objectBucketRegistry.RemoveIfEmpty(simOrder, bucket);
                    RuntimeSlotCapacityRejectCountForDiagnostics++;
                    if (!runtimeCapacityModule.IsSealed)
                    {
                        Debug.LogWarning(
                            $"[SimulationWorld] Runtime slot exhausted; registration rejected: " +
                            $"StableId={registeredEntity.StableId}, " +
                            $"Type={registeredEntity.GetType().Name}");
                    }
                    return;
                }

                registeredEntity.Runtime?.BindWorldMutationTracker(
                    runtimeMutationTracker);

                ResetRawRuntimeSlotState(runtimeSlot);
                if (registeredEntity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
                {
                    if (!ResetCooldownsForRuntimeSlot(runtimeSlot, registeredEntity))
                    {
                        RollbackRuntimeSlotRegistration(registeredEntity, runtimeSlot);
                        objectBucketRegistry.RemoveIfEmpty(simOrder, bucket);
                        RuntimeRestBindRejectCountForDiagnostics++;
                        if (!runtimeCapacityModule.IsSealed)
                        {
                            Debug.LogError(
                                $"[SimulationWorld] Runtime rest bind failed; registration rejected: " +
                                $"Slot={runtimeSlot}, StableId={registeredEntity.StableId}, " +
                                $"Type={registeredEntity.GetType().Name}");
                        }
                        return;
                    }
                }

                int requestedStableId = registeredEntity.Runtime.StableId;
                if (requestedStableId > 0)
                {
                    if (ContainsRegisteredEntityStableId(requestedStableId) ||
                        requestedStableId == int.MaxValue)
                    {
                        RollbackRuntimeSlotRegistration(registeredEntity, runtimeSlot);
                        objectBucketRegistry.RemoveIfEmpty(simOrder, bucket);
                        StableIdRegistrationRejectCountForDiagnostics++;
                        if (!runtimeCapacityModule.IsSealed)
                        {
                            Debug.LogError(
                                $"[SimulationWorld] StableId registration rejected: " +
                                $"StableId={requestedStableId}, " +
                                $"Type={registeredEntity.GetType().Name}");
                        }
                        return;
                    }

                    if (requestedStableId >= _nextAutoStableId)
                        _nextAutoStableId = requestedStableId + 1;
                }
                else
                {
                    registeredEntity.AssignStableIdForRegistration(AllocateStableId());
                }

                if (!registeredEntity.ShouldDeferInitialRuntimeSnapshot())
                    registeredEntity.RefreshRuntimeSnapshot();

                if (structuralEventSink != null)
                {
                    EmitStructuralEvent(
                        "allocate",
                        runtimeSlot,
                        -1,
                        -1,
                        "free",
                        "active",
                        StructuralSourceKind(registeredEntity));
                }
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
            if (EnableRegistryLifecycleLoggingForDiagnostics)
            {
                Debug.Log(
                    $"[SimulationWorld] Registered: SimOrder={simOrder}, " +
                    $"StableId={obj.StableId}, Type={obj.GetType().Name}");
            }
        }

        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                MissingUnregisterCountForDiagnostics++;
                if (!runtimeCapacityModule.IsSealed)
                    Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (_ticking)
            {
                int pendingSlot = -1;
                if (structuralEventSink != null && obj is LF2Entity pendingSlotEntity)
                    pendingSlot = pendingSlotEntity.Runtime?.SlotIndex ?? -1;
                if (obj is LF2Entity pendingEntity &&
                    !ReleaseRuntimeSlotAndClearPresentationBinding(pendingEntity))
                {
                    return;
                }
                if (!_pendingUnregister.Contains(obj))
                    _pendingUnregister.Add(obj);
                if (structuralEventSink != null)
                {
                    if (pendingSlot >= 0)
                        structuralPendingUnregisterSlots[obj] = pendingSlot;
                    EmitStructuralEvent(
                        "unregister-deferred",
                        pendingSlot,
                        -1,
                        -1,
                        "active",
                        "pending",
                        obj is LF2Entity pendingSource
                            ? StructuralSourceKind(pendingSource)
                            : "general");
                }
                return;
            }

            UnregisterImmediate(obj);
        }

        private void UnregisterImmediate(ISimObject obj)
        {
            int bucketKey = obj.SimOrder;
            _buckets.TryGetValue(bucketKey, out SimulationObjectBucket bucket);
            if (bucket == null || !bucket.items.Contains(obj))
            {
                bucket = null;
                for (int bucketIndex = 0;
                     bucketIndex < objectBucketRegistry.OrderedCount;
                     bucketIndex++)
                {
                    SimulationObjectBucket candidateBucket =
                        objectBucketRegistry.GetOrderedBucket(bucketIndex);
                    if (candidateBucket == null ||
                        !candidateBucket.items.Contains(obj))
                    {
                        continue;
                    }

                    bucketKey = candidateBucket.SimOrder;
                    bucket = candidateBucket;
                    break;
                }
            }

            if (bucket == null)
            {
                MissingUnregisterCountForDiagnostics++;
                if (!runtimeCapacityModule.IsSealed)
                {
                    Debug.LogWarning(
                        $"[SimulationWorld] Object not found in buckets: " +
                        $"CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                }
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
                MissingUnregisterCountForDiagnostics++;
                if (!runtimeCapacityModule.IsSealed)
                {
                    Debug.LogWarning(
                        $"[SimulationWorld] Object not found in buckets: " +
                        $"CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                }
                return;
            }

            bucket.dirty = true;
            obj.OnRemoved(_context);

            objectBucketRegistry.RemoveIfEmpty(bucketKey, bucket);

            if (EnableRegistryLifecycleLoggingForDiagnostics)
            {
                Debug.Log(
                    $"[SimulationWorld] Unregistered: SimOrder={bucketKey}, " +
                    $"StableId={obj.StableId}, Type={obj.GetType().Name}");
            }
        }

        private void FlushPendingUnregister()
        {
            if (_pendingUnregister.Count == 0) return;
            foreach (var obj in _pendingUnregister)
            {
                int pendingSlot = -1;
                if (structuralEventSink != null &&
                    structuralPendingUnregisterSlots != null &&
                    structuralPendingUnregisterSlots.TryGetValue(obj, out int recordedSlot))
                {
                    pendingSlot = recordedSlot;
                }
                UnregisterImmediate(obj);
                if (structuralEventSink != null)
                {
                    EmitStructuralEvent(
                        "unregister-flush",
                        pendingSlot,
                        -1,
                        -1,
                        "pending",
                        "free",
                        obj is LF2Entity entity
                            ? StructuralSourceKind(entity)
                            : "general");
                    structuralPendingUnregisterSlots?.Remove(obj);
                }
            }
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

        private int AllocateStableId()
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
                    return RecordStructuralSearch(
                        -1,
                        requiredSlot,
                        requiredSlot + 1,
                        entity);
                }

                if (!_runtimeSlots.TryClaim(requiredSlot, entity, out _))
                    return RecordStructuralSearch(
                        -1,
                        entity.Runtime.SpawnSemantic == (int)ReleaseSpawnSemantic.StageSpawnAt ? 20 : requiredSlot,
                        entity.Runtime.SpawnSemantic == (int)ReleaseSpawnSemantic.StageSpawnAt
                            ? RuntimeSlotCapacity
                            : requiredSlot + 1,
                        entity);

                return RecordStructuralSearch(
                    requiredSlot,
                    entity.Runtime.SpawnSemantic == (int)ReleaseSpawnSemantic.StageSpawnAt ? 20 : requiredSlot,
                    entity.Runtime.SpawnSemantic == (int)ReleaseSpawnSemantic.StageSpawnAt
                        ? RuntimeSlotCapacity
                        : requiredSlot + 1,
                    entity);
            }

            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            bool existingSlotInRange = existingSlot >= 0 && existingSlot < RuntimeSlotCapacity;
            bool existingSlotInAllowedRange = !requiresDynamicSlot || existingSlot >= DynamicRuntimeSlotStart;
            int minimumExistingSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            if (existingSlotInRange && existingSlotInAllowedRange &&
                existingSlot >= minimumExistingSlot &&
                _runtimeSlots.TryClaim(existingSlot, entity, out _))
            {
                return RecordStructuralSearch(
                    existingSlot,
                    minimumExistingSlot,
                    RuntimeSlotCapacity,
                    entity);
            }

            int startSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            int allocatedSlot = _runtimeSlots.AllocateLowest(startSlot, entity, out _);
            if (allocatedSlot >= 0 || !TryGrowDesktopRuntimeSlots((long)RuntimeSlotCapacity + 1))
                return RecordStructuralSearch(
                    allocatedSlot,
                    startSlot,
                    RuntimeSlotCapacity,
                    entity);

            return RecordStructuralSearch(
                _runtimeSlots.AllocateLowest(startSlot, entity, out _),
                startSlot,
                RuntimeSlotCapacity,
                entity);
        }

        private int RecordStructuralSearch(
            int slot,
            int searchStart,
            int searchEndExclusive,
            LF2Entity entity)
        {
            if (structuralEventSink == null)
                return slot;
            EmitStructuralEvent(
                "search",
                slot,
                searchStart,
                searchEndExclusive,
                "free",
                slot >= 0 ? "selected" : "exhausted",
                StructuralSourceKind(entity));
            return slot;
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
            if (!runtimeCapacityModule.TryAuthorizeGrowth())
                return false;
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
            GrowAiSoASensingRows(normalizedCapacity);
            return true;
        }

        private void ReleasePendingDestroySlots()
        {
            long mutationEpoch = runtimeMutationTracker.PendingFlushDestroyEpoch;
            ulong occupancyEpoch = _runtimeSlots.OccupancyEpoch;
            if (!ForceLegacyPendingDestroyScanForDiagnostics &&
                pendingDestroyScanCacheValid &&
                pendingDestroyScanMutationEpoch == mutationEpoch &&
                pendingDestroyScanOccupancyEpoch == occupancyEpoch)
            {
                PendingDestroySkipCount++;
                return;
            }

            PendingDestroyFullScanCount++;
            for (int slot = 0; slot < _runtimeSlots.LogicalCapacity; slot++)
            {
                LF2Entity entity = _runtimeSlots.GetCurrentOccupant(slot);
                if (entity == null)
                    continue;

                PendingDestroyVisitedEntityCount++;
                if (entity.Runtime == null ||
                    !entity.Runtime.PendingFlushDestroy)
                {
                    continue;
                }

                if (entity.Runtime.SlotIndex != slot)
                    continue;

                if (ReleaseRuntimeSlotAndClearPresentationBinding(entity) &&
                    !_pendingSlotReleasedDestroy.Contains(entity))
                {
                    _pendingSlotReleasedDestroy.Add(entity);
                }
            }

            long completedMutationEpoch =
                runtimeMutationTracker.PendingFlushDestroyEpoch;
            pendingDestroyScanMutationEpoch = completedMutationEpoch;
            pendingDestroyScanOccupancyEpoch = _runtimeSlots.OccupancyEpoch;
            pendingDestroyScanCacheValid = mutationEpoch == completedMutationEpoch;
        }

        private bool ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot < 0)
                return true;
            if (slot >= RuntimeSlotCapacity ||
                !object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(slot), entity))
            {
                RuntimeSlotReleaseRejectCountForDiagnostics++;
                if (!runtimeCapacityModule.IsSealed)
                {
                    Debug.LogError(
                        $"[SimulationWorld] Refusing runtime slot release without the matching claim: " +
                        $"EntitySlot={slot}, StableId={entity.StableId}");
                }
                return false;
            }

            bool wasBound = entity.ItrRest?.IsBound == true;
            if (wasBound && entity.ItrRest.BoundVictimSlot != slot)
            {
                RuntimeSlotReleaseRejectCountForDiagnostics++;
                if (!runtimeCapacityModule.IsSealed)
                {
                    Debug.LogError(
                        $"[SimulationWorld] Refusing runtime slot release with a mismatched rest binding: " +
                        $"EntitySlot={slot}, BoundVictimSlot={entity.ItrRest.BoundVictimSlot}, " +
                        $"StableId={entity.StableId}");
                }
                return false;
            }
            if (wasBound && !entity.ItrRest.Unbind(false))
                return false;

            if (!_runtimeSlots.Release(slot, entity))
            {
                if (wasBound && !entity.ItrRest.Bind(_runtimeRestStore, slot, false))
                {
                    RuntimeSlotReleaseRejectCountForDiagnostics++;
                    if (!runtimeCapacityModule.IsSealed)
                    {
                        Debug.LogError(
                            $"[SimulationWorld] Failed to restore runtime rest binding after slot release rollback: " +
                            $"Slot={slot}, StableId={entity.StableId}");
                    }
                }
                return false;
            }

            if (structuralEventSink != null)
            {
                EmitStructuralEvent(
                    "free",
                    slot,
                    -1,
                    -1,
                    "active",
                    "free",
                    StructuralSourceKind(entity),
                    slot);
            }
            entity.Runtime?.BindWorldMutationTracker(null);
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
            entity?.Runtime?.BindWorldMutationTracker(null);
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
                for (int bucketIndex = 0;
                     bucketIndex < objectBucketRegistry.OrderedCount;
                     bucketIndex++)
                {
                    SimulationObjectBucket bucket =
                        objectBucketRegistry.GetOrderedBucket(bucketIndex);
                    if (bucket == null)
                        continue;

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
