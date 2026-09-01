using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Tools;
using NTSD.Extensions;
using MoreMountains.Tools;
using UnityEngine.Pool;

namespace NTSD.Animation
{
    /// <summary>
    /// OPoint 工厂实现，使用 Enqueue + Flush 模式承载 C++ release 的 opoint 生成语义。
    /// C++ release 在 frame_advance/process_opoint_spawn 中创建对象；
    /// Unity 通过任务队列延迟到确定的模拟阶段统一创建。
    /// </summary>
    public class LF2ObjectPointFactory : MMSingleton<LF2ObjectPointFactory>,
        ILF2ObjectPointFactory,
        IBattleObjectPointStructuralMaterializer
    {
        [Header("Prefab 映射 - OID 优先")]
        [SerializeField] private List<OidPrefabEntry> _oidPrefabs = new List<OidPrefabEntry>();

        [Header("Prefab 映射 - 类型兜底")]
        [SerializeField] private List<TypePrefabEntry> _typePrefabs = new List<TypePrefabEntry>();

        [System.Serializable]
        public class OidPrefabEntry
        {
            public int oid;
            public GameObject prefab;
        }

        [System.Serializable]
        public class TypePrefabEntry
        {
            public int type;
            public GameObject prefab;
        }

        private Dictionary<int, GameObject> _oidPrefabMap;
        private Dictionary<int, GameObject> _typePrefabMap;
        private readonly List<LF2Entity> _spawnedBuffer = new List<LF2Entity>(16);

        // ========== 任务队列（统一链表） ==========
        // 对应 C++ release 中一帧内延迟处理的 opoint 创建请求。
        private readonly LF2TaskRingBuffer _taskQueue = new LF2TaskRingBuffer(
            BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
        private bool _acceptingSpawnRequests = true;
        private BattleLogicReferencePool _shutdownReferencePool;

        public int PendingTaskCountForDiagnostics => _taskQueue.Count;
        public int TaskQueueCapacityForDiagnostics => _taskQueue.Capacity;
        public long RejectedTaskCountForDiagnostics => _taskQueue.RejectedEnqueueCount;
        public long UnknownTaskTypeCountForDiagnostics { get; private set; }
        public long MissingObjectDefinitionCountForDiagnostics { get; private set; }
        public bool AcceptingSpawnRequestsForDiagnostics => _acceptingSpawnRequests;
        public long ShutdownRejectedTaskCountForDiagnostics { get; private set; }
        public long ShutdownDiscardedTaskCountForDiagnostics { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _acceptingSpawnRequests = true;
            BuildPrefabMaps();
        }

        public void BeginBattlePreparation()
        {
            _acceptingSpawnRequests = true;
            _shutdownReferencePool = null;
        }

        public void BeginBattleShutdown(SimulationWorld world)
        {
            _acceptingSpawnRequests = false;
            _shutdownReferencePool = world?.LogicReferencePool;
        }

        private void BuildPrefabMaps()
        {
            _oidPrefabMap = new Dictionary<int, GameObject>();
            foreach (var e in _oidPrefabs)
                if (e.prefab != null && !_oidPrefabMap.ContainsKey(e.oid))
                    _oidPrefabMap[e.oid] = e.prefab;

            _typePrefabMap = new Dictionary<int, GameObject>();
            foreach (var e in _typePrefabs)
                if (e.prefab != null && !_typePrefabMap.ContainsKey(e.type))
                    _typePrefabMap[e.type] = e.prefab;
        }

        // ========== Enqueue API ==========
        public void EnqueueCreateObject(OPointCreateTask task)
        {
            if (!_acceptingSpawnRequests)
            {
                RejectTaskDuringShutdown(task);
                return;
            }
            if (!_taskQueue.TryEnqueue(task))
                ResolveReferencePool(task?.targetWorld ?? task?.parent?.Match)?.Recycle(task);
        }

        public void EnqueueCreateMultipleObjects(OPointCreateMultipleTask task)
        {
            if (!_acceptingSpawnRequests)
            {
                RejectTaskDuringShutdown(task);
                return;
            }
            if (!_taskQueue.TryEnqueue(task))
                ResolveReferencePool(task?.targetWorld ?? task?.parent?.Match)?.Recycle(task);
        }

        public void PrepareTaskQueueCapacity(int capacity)
        {
            _taskQueue.EnsureCapacity(capacity);
        }

        public void SealBattleTaskCapacity()
        {
            _taskQueue.SealCapacity();
        }

        public void UnsealBattleTaskCapacity()
        {
            _taskQueue.UnsealCapacity();
        }

        // ========== FlushTasks（处理队列） ==========
        /// <summary>
        /// 处理本次调用开始前已经入队的任务。
        /// 处理期间新入队的任务保留到下一次边界刷新，避免 opoint 在同一轮 Flush 中递归展开。
        /// </summary>
        public void FlushTasks()
        {
            if (!_acceptingSpawnRequests)
            {
                DiscardPendingTasks();
                return;
            }

            int taskCount = _taskQueue.Count;
            for (int i = 0; i < taskCount; i++)
            {
                if (!_taskQueue.TryDequeue(out LF2TaskBase task))
                    break;

                ProcessTask(task);
            }
        }

        // ========== 任务处理（分发） ==========
        /// <summary>
        /// 根据任务类型分发处理 opoint 创建请求。
        /// </summary>
        private void ProcessTask(LF2TaskBase task)
        {
            switch (task.TaskType)
            {
                case LF2TaskType.CreateObject:
                    ProcessCreateObject(
                        (OPointCreateTask)task,
                        BattleStructuralPlaybackBoundary.CurrentPassSegment);
                    break;

                case LF2TaskType.CreateMultipleObjects:
                    ProcessCreateMultipleObjects(
                        (OPointCreateMultipleTask)task,
                        BattleStructuralPlaybackBoundary.CurrentPassSegment);
                    break;

                default:
                    UnknownTaskTypeCountForDiagnostics++;
                    if (!_taskQueue.CapacitySealed)
                        Log.Warn($"[LF2ObjectPointFactory] Unknown task type: {task.TaskType}");
                    break;
            }

            if (task is ILF2Recyclable recyclable)
                ResolveReferencePoolForTask(task)?.Recycle(recyclable);
        }

        // ========== 单对象创建 ==========

        /// <summary>
        /// 处理单对象创建任务，对齐 C++ release 的 spawn_from_opoint 单对象路径。
        /// </summary>
        /// <summary>
        /// C++ release process_opoint_spawn：在实体 late tail 按当前帧直接处理 opoint。
        /// 普通 DAT opoint 不再由 FrameEvent 提前触发，避免和正式版时序错位。
        /// </summary>
        public void ProcessOpointSpawn(LF2Entity spawner)
        {
            if (!_acceptingSpawnRequests)
                return;

            SimulationWorld world = spawner?.Match;
            if (world != null)
            {
                world.StructuralWriter.ProcessLateOpointSegment(
                    this,
                    spawner,
                    world.CurrentTickIndex);
                return;
            }

            ProcessOpointSpawnCoreForStructuralWriter(spawner);
        }

        internal void ProcessOpointSpawnCoreForStructuralWriter(LF2Entity spawner)
        {
            if (!_acceptingSpawnRequests)
                return;
            if (spawner == null || spawner.PS == null) return;

            LF2FrameData frame = spawner.Frame?.D;
            if (frame == null) return;

            bool hasList = frame.opoints != null && frame.opoints.Count > 0;
            bool hasSingle = frame.opoint.HasValue;
            if (!hasList && !hasSingle) return;

            BattleObjectPointValue firstOp =
                hasList ? frame.opoints[0] : frame.opoint.Value;
            if (firstOp.Kind <= 0 ||
                firstOp.Oid <= 0 ||
                spawner.AttackingCounter != 0)
            {
                return;
            }
            if (spawner.FrameDelay != 0 &&
                spawner.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                return;

            if (hasList)
            {
                for (int i = 0; i < frame.opoints.Count; i++)
                {
                    _spawnedBuffer.Clear();
                    ProcessOneLateOpoint(spawner, frame, frame.opoints[i]);
                    ApplyMultiSpawnExemptAndVrest(_spawnedBuffer);
                }
            }
            else
            {
                _spawnedBuffer.Clear();
                ProcessOneLateOpoint(spawner, frame, frame.opoint.Value);
                ApplyMultiSpawnExemptAndVrest(_spawnedBuffer);
            }

            _spawnedBuffer.Clear();
        }

        public void ProcessOpointSpawnAlignedToCpp(LF2Entity spawner)
        {
            ProcessOpointSpawn(spawner);
        }

        private void ProcessOneLateOpoint(
            LF2Entity spawner,
            LF2FrameData frame,
            BattleObjectPointValue op)
        {
            if (op.Kind <= 0 || op.Oid <= 0) return;

            int spawnCount = 1;
            int facingMode = op.Facing;
            if (op.Facing > 10)
            {
                spawnCount = op.Facing / 10;
                facingMode = op.Facing % 10;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                int requiredRuntimeSlot = spawner.Match?.FindFirstFreeFrameLogicRuntimeSlot() ?? -1;
                if (requiredRuntimeSlot < 0)
                    continue;

                ObjectPoint spawnOp =
                    BattleObjectPointValueAdapter.ToLegacyTask(op);
                spawnOp.facing = facingMode;

                BattleLogicReferencePool referencePool =
                    ResolveReferencePool(spawner.Match);
                OPointCreateTask task = referencePool?.Fetch<OPointCreateTask>();
                if (task == null)
                    break;
                task.opoint = spawnOp;
                task.parent = spawner;
                task.team = spawner.Team;
                ConfigureLateOpointPosition(task, spawner, frame, op);
                task.dir = spawner.PS.dir;
                task.dvz = 0f;
                task.preserveActionZero = true;
                task.releaseSpawnSemantic = ReleaseSpawnSemantic.LateOpoint;
                task.releaseOpointSpawn = true;
                task.requiredRuntimeSlot = requiredRuntimeSlot;

                LF2Entity spawned = ProcessCreateObject(
                    task,
                    BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
                referencePool?.Recycle(task);
                if (spawned == null) continue;

                if (op.Kind != 2)
                    spawned.Runtime.HolderStableId = 0;

                if (spawnCount > 1)
                {
                    float spread = i * 10f / (spawnCount - 1) - 5f;
                    spawned.PS.vz += spread;
                    float absSpread = Mathf.Abs(spread);
                    if (spawned.PS.vx > 0f)
                        spawned.PS.vx -= absSpread;
                    else if (spawned.PS.vx < 0f)
                        spawned.PS.vx += absSpread;
                    else
                        spawned.PS.vx += spread;
                }

                if (spawner.GetCurrentDataObjectTypeForSimulation() == 3 && frame.state == 3003)
                    ApplyState3003LinkedVrest(spawner, spawned);

                spawned.AttackExempt = 0;
                _spawnedBuffer.Add(spawned);
            }
        }

        private static void ConfigureLateOpointPosition(
            OPointCreateTask task,
            LF2Entity spawner,
            LF2FrameData frame,
            BattleObjectPointValue op)
        {
            int spawnX = spawner.Runtime.Dir == "right"
                ? spawner.Runtime.XInt - frame.centerx + op.X
                : spawner.Runtime.XInt + frame.centerx - op.X;
            int spawnY = spawner.Runtime.YInt - frame.centery + op.Y;
            double spawnZ = spawner.Runtime.Z + 1.0;

            task.pos = new Vector3(spawnX, spawnY, (float)spawnZ);
            task.z = (float)spawnZ;
            task.useDirectRuntimePosition = true;
            task.directX = spawnX;
            task.directY = spawnY;
            task.directZ = spawnZ;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = spawnX;
            task.initialRuntimeY = spawnY;
            task.initialRuntimeZ = (int)spawnZ;
        }

        void IBattleObjectPointStructuralMaterializer
            .ProcessOpointSpawnCoreForStructuralWriter(LF2Entity spawner)
        {
            ProcessOpointSpawnCoreForStructuralWriter(spawner);
        }

        public static void PrepareFinalRuntimePositionForCreation(OPointCreateTask task)
        {
            BattleLogicEntityFactory.PrepareFinalRuntimePosition(task);
        }

        private static void ApplyMultiSpawnExemptAndVrest(List<LF2Entity> spawned)
        {
            int spawnedCount = spawned.Count;
            if (spawnedCount <= 1) return;

            int center = spawnedCount / 2;
            for (int i = 0; i < spawnedCount; i++)
            {
                LF2Entity entity = spawned[i];
                if (entity == null) continue;

                if ((spawnedCount & 1) == 0)
                {
                    if (i < center - 1) entity.AttackExempt = (center - i - 1) * 2;
                    else if (i > center) entity.AttackExempt = (i - center) * 2;
                }
                else
                {
                    if (i < center) entity.AttackExempt = (center - i) * 2;
                    else if (i > center) entity.AttackExempt = (i - center) * 2;
                }

                for (int prev = 0; prev < i; prev++)
                {
                    LF2Entity other = spawned[prev];
                    if (other == null) continue;
                    int entitySlot = entity.Runtime?.SlotIndex ?? -1;
                    int otherSlot = other.Runtime?.SlotIndex ?? -1;
                    if (entitySlot < 0 || otherSlot < 0) continue;
                    entity.ItrRest?.SetVrest(otherSlot, 40);
                    other.ItrRest?.SetVrest(entitySlot, 40);
                }
            }
        }

        private LF2Entity ProcessCreateObject(
            OPointCreateTask task,
            BattleStructuralPlaybackBoundary boundary)
        {
            SimulationWorld world = task?.targetWorld ??
                                    task?.parent?.Match ??
                                    SimulationTickDriver.Instance?.World;
            return world != null
                ? world.StructuralWriter.Spawn(this, task, boundary)
                : MaterializeObjectForStructuralWriter(task);
        }

        internal LF2Entity MaterializeObjectForStructuralWriter(OPointCreateTask task)
        {
            if (!_acceptingSpawnRequests)
                return null;

            SimulationWorld requestedWorld = task?.targetWorld ?? task?.parent?.Match;
            if (requestedWorld?.UsesLogicOnlyEntityMaterialization == true)
            {
                LF2Entity logicEntity = requestedWorld.LogicEntityFactory.Create(
                    task,
                    out BattleLogicEntityCreationFailure failure);
                if (failure == BattleLogicEntityCreationFailure.MissingObjectDefinition)
                    MissingObjectDefinitionCountForDiagnostics++;
                return logicEntity;
            }

            SimulationWorld world = task?.targetWorld ??
                                    task?.parent?.Match ??
                                    SimulationTickDriver.Instance?.World;
            // 1. 检查 oid
            int oid = task.opoint.oid;
            if (oid <= 0) return null;

            // 2. 获取对象定义
            var def = ResolveObjectDefinition(world, oid);
            if (def == null)
            {
                MissingObjectDefinitionCountForDiagnostics++;
                if (!_taskQueue.CapacitySealed)
                    Log.Error($"[Factory] Object {oid} not exists");
                return null;
            }

            LF2CharacterDataWrapper combatData = ResolveCharacterConfig(world, oid);
            if (combatData?.characterData == null)
                return null;

            int objType = def.type;

            // 4. 从对象池获取对象
            var entityObj = LF2ObjectPool.Instance.Get(out LF2ObjectRenderer EntityModel);
            if (EntityModel == null)
            {
                if (LF2ObjectPool.Instance != null &&
                    !LF2ObjectPool.Instance.IsBattleCapacitySealed)
                {
                    Log.Error("[Factory] Failed to get object from pool");
                }
                return null;
            }

            // 5. 从逻辑对象池获取逻辑对象
            BattleLogicReferencePool referencePool = ResolveReferencePool(world);
            ILF2Object logicObject = CreateLogicObject(referencePool, objType, oid);
            if (logicObject == null)
            {
                if (referencePool != null &&
                    !referencePool.IsBattleCapacitySealed)
                {
                    Log.Error($"[Factory] Failed to get logic object from pool, type={objType}, oid={oid}");
                }
                LF2ObjectPool.Instance.Release(EntityModel);
                return null;
            }

            // 5.1 武器对象注入 weapon_strength_list
            if (logicObject is LF2WeaponBase weaponBase)
            {
                var charData = ResolveCharacterData(world, oid);
                if (charData?.weapon_strength_list?.Count > 0)
                    weaponBase.SetWeaponStrengthList(charData.weapon_strength_list);
            }

            // 5.2 角色对象初始化（ModuleInitialize 在 SetLogicObject 之前，ModuleBind 在之后，不绑定输入）
            var spawnedChar = logicObject as LF2Character;
            spawnedChar?.ModuleInitialize();
            if (logicObject is LF2Entity requestedEntity)
                requestedEntity.SetRequiredRuntimeSlot(task.requiredRuntimeSlot);

            // 6. 设置逻辑对象并初始化
            PrepareFinalRuntimePositionForCreation(task);
            EntityModel.SetLogicObject(logicObject, task);

            if (spawnedChar != null)
            {
                var charFrameData = ResolveCharacterConfig(world, oid);
                if (charFrameData != null)
                    spawnedChar.ModuleBind(charFrameData, oid, world);
                spawnedChar.Initialize(NTSDGlobal.Default.Health.HpFull, NTSDGlobal.Default.Health.MpFull);
            }

            if (logicObject is LF2Entity registeredLiving && registeredLiving.Runtime.SlotIndex < 0)
            {
                ReleaseRejectedSpawn(EntityModel, logicObject, referencePool);
                return null;
            }

            // 所有 LF2Entity（角色、武器、特效）的通用后处理
            if (logicObject is LF2Entity living)
            {
                // 7. 过滤纯音效对象（pic=999, wait=0, next=1000）——播放 sound 后直接 Release
                PostInitLiving(
                    living,
                    task.parent,
                    task.opoint,
                    objType,
                    0f,
                    task.releaseOpointSpawn);
                ApplyReleaseOpointDirectionalVz(living, task);
                ApplyDirectVelocity(living, task);

                if (task.frameDelay > 0)
                    living.FrameDelay = task.frameDelay;

                if (task.attackExempt > 0)
                    living.AttackExempt = task.attackExempt;

                // 生成后写入 OwnerEntityIndex（C++ release 对齐 hit_Fa=5/6/8/9 case 直接写 [+1016]）
                if (task.ownerEntityIndex >= 0)
                    living.OwnerEntityIndex = task.ownerEntityIndex;

                return living;
            }

            return null;
        }

        public LF2Entity CreateObjectImmediate(OPointCreateTask task)
        {
            if (!_acceptingSpawnRequests)
            {
                ShutdownRejectedTaskCountForDiagnostics++;
                return null;
            }
            return ProcessCreateObject(
                task,
                BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
        }

        LF2Entity IBattleObjectPointStructuralMaterializer
            .MaterializeObjectForStructuralWriter(OPointCreateTask task)
        {
            return MaterializeObjectForStructuralWriter(task);
        }

        // ========== 多对象创建 ==========

        /// <summary>
        /// 处理多对象创建任务，对齐 C++ release 的 opoint 多对象散射路径。
        /// </summary>
        private void ProcessCreateMultipleObjects(
            OPointCreateMultipleTask task,
            BattleStructuralPlaybackBoundary boundary)
        {
            SimulationWorld world = task?.targetWorld ??
                                    task?.parent?.Match ??
                                    SimulationTickDriver.Instance?.World;
            if (world != null)
            {
                world.StructuralWriter.SpawnMultiple(this, task, boundary);
                return;
            }

            MaterializeMultipleObjectsForStructuralWriter(task);
        }

        internal void MaterializeMultipleObjectsForStructuralWriter(
            OPointCreateMultipleTask task)
        {
            if (!_acceptingSpawnRequests)
                return;

            SimulationWorld requestedWorld = task?.targetWorld ?? task?.parent?.Match;
            if (requestedWorld?.UsesLogicOnlyEntityMaterialization == true)
            {
                MaterializeMultipleLogicObjects(requestedWorld, task);
                return;
            }

            SimulationWorld world = task?.targetWorld ??
                                    task?.parent?.Match ??
                                    SimulationTickDriver.Instance?.World;
            int oid = task.opoint.oid;
            if (oid <= 0 || task.number <= 0) return;

            var def = ResolveObjectDefinition(world, oid);
            if (def == null)
            {
                MissingObjectDefinitionCountForDiagnostics++;
                if (!_taskQueue.CapacitySealed)
                    Log.Error($"[Factory] Object {oid} not exists");
                return;
            }

            LF2CharacterDataWrapper combatData = ResolveCharacterConfig(world, oid);
            if (combatData?.characterData == null)
                return;

            int objType = def.type;

            // C++ release 对齐 0x004225B6：dvz_i = i * 10.0 / (count-1) - 5.0，固定范围 [-5, +5]
            for (int spawnIndex = 0; spawnIndex < task.number; spawnIndex++)
            {
                float vz = task.number == 1
                    ? 0f
                    : spawnIndex * 10f / (task.number - 1) - 5f;
                var entityObj = LF2ObjectPool.Instance.Get(out LF2ObjectRenderer EntityModel);
                if (EntityModel == null) break;

                BattleLogicReferencePool referencePool = ResolveReferencePool(world);
                ILF2Object logicObject = CreateLogicObject(referencePool, objType, oid);
                if (logicObject == null)
                {
                    LF2ObjectPool.Instance.Release(EntityModel);
                    continue;
                }

                if (logicObject is LF2WeaponBase wb)
                {
                    var charData = ResolveCharacterData(world, oid);
                    if (charData?.weapon_strength_list?.Count > 0)
                        wb.SetWeaponStrengthList(charData.weapon_strength_list);
                }

                var spawnedChar = logicObject as LF2Character;
                spawnedChar?.ModuleInitialize();

                var singleTask = referencePool?.Fetch<OPointCreateTask>();
                if (singleTask == null)
                {
                    ReleaseRejectedSpawn(EntityModel, logicObject, referencePool);
                    break;
                }
                singleTask.opoint = task.opoint;
                singleTask.parent = task.parent;
                singleTask.targetWorld = world;
                singleTask.team   = task.team;
                singleTask.pos    = task.pos;
                singleTask.z      = task.z;
                singleTask.dir    = task.dir;
                singleTask.dvz    = vz;
                singleTask.useDirectVelocity = task.useDirectVelocity;
                singleTask.directVx = task.directVx;
                singleTask.directVy = task.directVy;
                singleTask.directVz = task.directVz;
                singleTask.preserveActionZero = task.preserveActionZero;
                singleTask.skipPostInitZOffset = false;
                singleTask.ownerEntityIndex = task.ownerEntityIndex;
                singleTask.frameDelay = task.frameDelay;
                singleTask.attackExempt = task.attackExempt;
                singleTask.releaseOpointSpawn = task.releaseOpointSpawn;

                PrepareFinalRuntimePositionForCreation(singleTask);
                EntityModel.SetLogicObject(logicObject, singleTask);

                if (spawnedChar != null)
                {
                    var charFrameData = ResolveCharacterConfig(world, oid);
                    if (charFrameData != null)
                        spawnedChar.ModuleBind(charFrameData, oid, world);
                    spawnedChar.Initialize(NTSDGlobal.Default.Health.HpFull, NTSDGlobal.Default.Health.MpFull);
                }

                if (logicObject is LF2Entity registeredLiving && registeredLiving.Runtime.SlotIndex < 0)
                {
                    ReleaseRejectedSpawn(EntityModel, logicObject, referencePool);
                    referencePool?.Recycle(singleTask);
                    continue;
                }

                // 所有 LF2Entity（角色、武器、特效）的通用后处理
                if (logicObject is LF2Entity living)
                {
                    // 过滤纯音效对象（pic=999, wait=0, next=1000）——播放 sound 后直接 Release
                    PostInitLiving(
                        living,
                        task.parent,
                        task.opoint,
                        objType,
                        vz,
                        task.releaseOpointSpawn);
                    ApplyReleaseOpointDirectionalVz(living, singleTask);
                    ApplyDirectVelocity(living, singleTask);
                }

                referencePool?.Recycle(singleTask);
            }

        }

        void IBattleObjectPointStructuralMaterializer
            .MaterializeMultipleObjectsForStructuralWriter(
                OPointCreateMultipleTask task)
        {
            MaterializeMultipleObjectsForStructuralWriter(task);
        }

        private void MaterializeMultipleLogicObjects(
            SimulationWorld world,
            OPointCreateMultipleTask task)
        {
            if (task == null || task.number <= 0)
                return;

            BattleLogicReferencePool referencePool = ResolveReferencePool(world);
            for (int spawnIndex = 0; spawnIndex < task.number; spawnIndex++)
            {
                float spreadDvz = task.number == 1
                    ? 0f
                    : spawnIndex * 10f / (task.number - 1) - 5f;
                OPointCreateTask singleTask = referencePool?.Fetch<OPointCreateTask>();
                if (singleTask == null)
                    break;

                singleTask.opoint = task.opoint;
                singleTask.parent = task.parent;
                singleTask.targetWorld = world;
                singleTask.team = task.team;
                singleTask.pos = task.pos;
                singleTask.z = task.z;
                singleTask.dir = task.dir;
                singleTask.dvz = spreadDvz;
                singleTask.useDirectVelocity = task.useDirectVelocity;
                singleTask.directVx = task.directVx;
                singleTask.directVy = task.directVy;
                singleTask.directVz = task.directVz;
                singleTask.preserveActionZero = task.preserveActionZero;
                singleTask.skipPostInitZOffset = false;
                singleTask.ownerEntityIndex = task.ownerEntityIndex;
                singleTask.frameDelay = task.frameDelay;
                singleTask.attackExempt = task.attackExempt;
                singleTask.releaseOpointSpawn = task.releaseOpointSpawn;

                world.LogicEntityFactory.Create(
                    singleTask,
                    out BattleLogicEntityCreationFailure failure,
                    spreadDvz);
                if (failure == BattleLogicEntityCreationFailure.MissingObjectDefinition)
                    MissingObjectDefinitionCountForDiagnostics++;
                referencePool.Recycle(singleTask);
            }
        }

        /// <summary>
        /// SetLogicObject 之后的统一后处理
        /// C++ release 对齐 opoint 创建后初始化序列（0x004223B5-0x0042277E）
        /// </summary>
        private void PostInitLiving(
            LF2Entity living,
            LF2Entity parent,
            ObjectPoint op,
            int objType,
            float dvz,
            bool releaseOpointSpawn)
        {
            if (parent != null)
            {
                // Inherit the C++ release relation identity from the spawning entity.
                living.Team = parent.Team;
                living.RelationTeam = parent.RelationTeam;
                living.HolderCopySlot = parent.HolderCopySlot;

                // owner_id 继承链（C++ release 对齐 0x004224F8-0x0042250B）
                living.OwnerId = releaseOpointSpawn
                    ? -1
                    : (parent.OwnerId > -1 ? parent.OwnerId : parent.StableId);

                // kill_count 继承链：父实体已有归属时沿用，否则记录父实体 StableId。
                if (objType == 0)
                {
                    living.KillCount = parent.KillCount > -1 ? parent.KillCount : GetRuntimeSlotOrStableId(parent);
                    living.HitStun = parent.HitStun;
                    living.AiControlled = releaseOpointSpawn;
                }
                else if (!releaseOpointSpawn)
                {
                    living.KillCount = parent.KillCount > -1 ? parent.KillCount : parent.StableId;
                }
            }

            // oid==5 或 52 特殊 HP 初始化（C++ release 对齐 0x00422694：cmp ecx, 5 / cmp ecx, 34h，检查 data.oid 不是 type）
            if (op.oid == 5 || op.oid == 52)
            {
                living.Health.HP = 10;
                living.Health.HPBound = 10;
                living.Health.HP3 = 10;
                living.Health.PP = 5;
            }

            // kind==2 追踪绑定（C++ release 对齐 0x00422729-0x0042277E，无 entity_type 守卫）
            if (op.kind == 2 && parent != null)
            {
                parent.TrackerFlag  = 1;
                living.TrackerFlag  = -1;
                living.TrackerParent = parent;
                if (parent is LF2Character parentCharacter)
                    parentCharacter.AttachOpointHeldObject(living);
                else
                {
                    int parentSlot = parent.Runtime?.SlotIndex ?? -1;
                    int livingSlot = living.Runtime?.SlotIndex ?? -1;
                    parent.Runtime.LinkState = 1;
                    parent.Runtime.TargetSlotIndex = livingSlot;
                    parent.Runtime.HeldWeaponStableId = livingSlot;
                    living.Runtime.LinkState = -1;
                    living.Runtime.HolderStableId = parentSlot;
                }
                // C++ release 0x00422778-0x0042277E：spawned[+364h] = parent[+364h]（team 再次同步）
                living.Team = parent.Team;
            }

            // 多对象 dvz 侧偏 vx/vz（C++ release 对齐 0x004225DB/0x004225E8-0x00422627）
            // dvz 直接加到 vz（0x004225DB: entity.vz += dvz_i）
            // dvz 影响 vx 方向：向左扩散时 vx 减小，向右扩散时 vx 增大
            if (dvz != 0f)
            {
                living.PS.vz += dvz;
                float absDvz = Mathf.Abs(dvz);
                // dir 与 dvz 同向 → 扩散 → vx 加 absDvz；反向 → 收拢 → vx 减 absDvz
                if (living.PS.vx > 0f)
                    living.PS.vx -= absDvz;
                else if (living.PS.vx < 0f)
                    living.PS.vx += absDvz;
                else
                    living.PS.vx += dvz;
            }
        }

        private string CalculateDirection(int facing, string parentDir)
        {
            int face = facing >= 20 ? facing % 10 : facing;
            if (face == 0) return parentDir;
            if (face == 1) return (parentDir == "right") ? "left" : "right";
            if (face >= 2 && face <= 10) return "right";
            if (face >= 11 && face <= 19) return "left";
            return parentDir;
        }

        private static int GetRuntimeSlotOrStableId(LF2Entity entity)
        {
            if (entity == null) return -1;
            return entity.Runtime.SlotIndex >= 0 ? entity.Runtime.SlotIndex : entity.StableId;
        }

        private static void ApplyState3003LinkedVrest(LF2Entity spawner, LF2Entity spawned)
        {
            int linkedSlot = spawner?.Runtime?.AnimCounter ?? -1;
            int spawnedSlot = spawned?.Runtime?.SlotIndex ?? -1;
            if (linkedSlot < 0 || spawnedSlot < 0) return;

            LF2Entity linked = spawner.Match?.FindEntityByRuntimeSlotForQuery(linkedSlot);
            if (linked == null) return;

            linked.ItrRest?.SetVrest(spawnedSlot, 10);
            spawned.ItrRest?.SetVrest(linkedSlot, 10);
        }

        internal static void ReleaseRejectedSpawn(
            LF2ObjectRenderer renderer,
            ILF2Object logicObject,
            BattleLogicReferencePool referencePool = null)
        {
            if (renderer != null)
            {
                LF2ObjectPool.Instance?.Release(renderer);
            }
            else if (logicObject is LF2Entity entity)
            {
                entity.UnregisterFromWorld();
                entity.Reset();
            }
            (referencePool ??
             (logicObject as LF2Entity)?.Match?.LogicReferencePool ??
             LF2ReferencePool.Instance?.SimulationCore)?.Release(logicObject);
        }

        private static void ApplyDirectVelocity(LF2Entity living, OPointCreateTask task)
        {
            if (living?.PS == null || task == null || !task.useDirectVelocity) return;
            living.PS.vx = task.directVx;
            living.PS.vy = task.directVy;
            living.PS.vz = task.directVz;
        }

        private static void ApplyReleaseOpointDirectionalVz(LF2Entity living, OPointCreateTask task)
        {
            if (living?.PS == null || task?.parent == null || !task.releaseOpointSpawn) return;
            if (task.useDirectVelocity) return;

            LF2FrameData frame = living.Frame?.D;
            if (frame == null) return;

            int state = frame.state;
            if (state != LF2States.ProjectileFlying &&
                state != LF2States.WeaponThrowing &&
                state != LF2States.ObjectExpanding)
                return;

            if (task.opoint.oid == 223 || task.opoint.oid == 224) return;

            bool up = task.parent.Runtime.KeyUp != 0;
            bool down = task.parent.Runtime.KeyDown != 0;

            if (up && !down)
                living.PS.vz = -2.5f;
            else if (down && !up)
                living.PS.vz = 2.5f;

            if (task.opoint.oid == 211)
                living.PS.vz *= 0.25f;
        }

        public void RegisterOidPrefab(int oid, GameObject prefab)
        {
            if (prefab != null) _oidPrefabMap[oid] = prefab;
        }

        public void RegisterTypePrefab(int type, GameObject prefab)
        {
            if (prefab != null) _typePrefabMap[type] = prefab;
        }

        // ========== 辅助方法 ==========

        /// <summary>
        /// 创建逻辑对象（从逻辑对象池获取）
        /// 根据 C++ release 对象 type 映射到 Unity 逻辑对象池。
        /// </summary>
        private static ILF2Object CreateLogicObject(
            BattleLogicReferencePool referencePool,
            int objectType,
            int oid)
        {
            // 将 int type 映射到 LF2ObjectType 枚举
            LF2ObjectType objTypeEnum = (LF2ObjectType)objectType;
            // 从逻辑对象池获取对象（池会自动处理 ObjectId 赋值）
            return referencePool?.Get(objTypeEnum, oid);
        }

        private static BattleLogicReferencePool ResolveReferencePool(
            SimulationWorld world)
        {
            return world?.LogicReferencePool ??
                   LF2ReferencePool.Instance?.SimulationCore;
        }

        private static BattleLogicReferencePool ResolveReferencePoolForTask(
            LF2TaskBase task)
        {
            if (task is OPointCreateTask createTask)
            {
                return ResolveReferencePool(
                    createTask.targetWorld ?? createTask.parent?.Match);
            }
            if (task is OPointCreateMultipleTask multipleTask)
            {
                return ResolveReferencePool(
                    multipleTask.targetWorld ?? multipleTask.parent?.Match);
            }
            return LF2ReferencePool.Instance?.SimulationCore;
        }

        public int DiscardPendingTasks()
        {
            int discarded = 0;
            while (_taskQueue.TryDequeue(out LF2TaskBase task))
            {
                discarded++;
                if (task is ILF2Recyclable recyclable)
                {
                    ResolveReferencePoolForShutdownTask(task)?.Recycle(recyclable);
                }
            }

            ShutdownDiscardedTaskCountForDiagnostics += discarded;
            return discarded;
        }

        private void RejectTaskDuringShutdown(LF2TaskBase task)
        {
            ShutdownRejectedTaskCountForDiagnostics++;
            if (task is ILF2Recyclable recyclable)
                ResolveReferencePoolForShutdownTask(task)?.Recycle(recyclable);
        }

        private BattleLogicReferencePool ResolveReferencePoolForShutdownTask(
            LF2TaskBase task)
        {
            SimulationWorld world = task switch
            {
                OPointCreateTask createTask =>
                    createTask.targetWorld ?? createTask.parent?.RegisteredWorldForSimulation,
                OPointCreateMultipleTask multipleTask =>
                    multipleTask.targetWorld ?? multipleTask.parent?.RegisteredWorldForSimulation,
                _ => null,
            };
            return world?.LogicReferencePool ??
                   _shutdownReferencePool ??
                   LF2ReferencePool.TryGetInstance()?.SimulationCore;
        }

        private static ObjectDefinition ResolveObjectDefinition(
            SimulationWorld world,
            int objectId)
        {
            BattleRuntimeDataCatalog catalog = world?.RuntimeDataCatalog;
            ObjectDefinition definition = catalog?.GetObjectDefinition(objectId);
            if (definition != null || catalog?.IsSealedForBattle == true)
                return definition;
            return GameDataManager.Instance?.GetObjectById(objectId);
        }

        private static LF2CharacterDataWrapper ResolveCharacterConfig(
            SimulationWorld world,
            int objectId)
        {
            BattleRuntimeDataCatalog catalog = world?.RuntimeDataCatalog;
            LF2CharacterDataWrapper config = catalog?.GetCharacterConfig(objectId);
            if (config != null || catalog?.IsSealedForBattle == true)
                return config;
            return CharacterAnimtorManager.Instance?.GetCharacterConfig(objectId);
        }

        private static LF2CharacterData ResolveCharacterData(
            SimulationWorld world,
            int objectId)
        {
            return ResolveCharacterConfig(world, objectId)?.characterData;
        }
    }
}
