using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Tools;
using MoreMountains.Tools;
using UnityEngine.Pool;

namespace NTSD.Animation
{
    /// <summary>
    /// OPoint 工厂实现 - Enqueue + Flush 模式
    /// 对应 FLF match.js tasks 队列 + process_tasks()
    /// 
    /// 参考：
    /// - FLF match.js:332 (process_tasks/process_task)
    /// - FLF specialattack.js:303 (specialattack.prototype.init)
    /// - FLF AI.js:20 (type: 0=character, 1=lightweapon, 2=heavyweapon, 3=specialattack, 4=baseball, 5=criminal, 6=drink)
    /// </summary>
    public class LF2ObjectPointFactory : MMSingleton<LF2ObjectPointFactory>, ILF2ObjectPointFactory
    {
        [Header("Prefab 映射 - OID 优先")]
        [SerializeField] private List<OidPrefabEntry> _oidPrefabs = new List<OidPrefabEntry>();

        [Header("Prefab 映射 - Type Fallback")]
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

        // ========== 任务队列（统一链表） ==========
        // 对应 FLF match.tasks 数组
        private readonly LinkedList<LF2TaskBase> _taskQueue = new LinkedList<LF2TaskBase>();

        protected override void Awake()
        {
            base.Awake();
            BuildPrefabMaps();
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
            _taskQueue.AddLast(task);
        }

        public void EnqueueCreateMultipleObjects(OPointCreateMultipleTask task)
        {
            _taskQueue.AddLast(task);
        }

        // ========== FlushTasks（处理队列） ==========
        /// <summary>
        /// 处理所有待处理任务并清空队列。
        /// 对应 FLF match.js process_tasks()。
        /// 在串行 Tick 模式下每个对象 Transit 后均会调用；队列为空时无操作。
        /// </summary>
        public void FlushTasks()
        {
            Debug.Log($"[OPointFactory] FlushTasks: queue count={_taskQueue.Count}");
            var node = _taskQueue.First;
            while (node != null)
            {
                ProcessTask(node.Value);
                node = node.Next;
            }
            _taskQueue.Clear();
        }

        // ========== 任务处理（分发） ==========
        /// <summary>
        /// 根据任务类型分发处理
        /// 对应 FLF match.js process_task(T)
        /// </summary>
        private void ProcessTask(LF2TaskBase task)
        {
            switch (task.TaskType)
            {
                case LF2TaskType.CreateObject:
                    ProcessCreateObject((OPointCreateTask)task);
                    break;

                case LF2TaskType.CreateMultipleObjects:
                    ProcessCreateMultipleObjects((OPointCreateMultipleTask)task);
                    break;

                default:
                    Log.Warn($"[LF2ObjectPointFactory] Unknown task type: {task.TaskType}");
                    break;
            }
        }

        // ========== 单对象创建 ==========

        /// <summary>
        /// 处理单对象创建任务
        /// 对应 FLF match.js:338-354 (case 'create_object')
        /// </summary>
        private void ProcessCreateObject(OPointCreateTask task)
        {
            // 1. 检查 oid
            int oid = task.opoint.oid;
            if (oid <= 0) return;

            // 2. 获取对象定义
            var def = GameDataManager.Instance?.GetObjectById(oid);
            if (def == null)
            {
                Log.Error($"[Factory] Object {oid} not exists");
                return;
            }

            int objType = def.type;

            // 3. 跳过 type==0 (character)
            if (objType == 0)
            {
                Log.Warn($"[Factory] Character creation via opoint not supported, oid={oid}");
                return;
            }

            // 4. 从对象池获取对象
            var entityObj = LF2ObjectPool.Instance.Get(out LF2ObjectRenderer EntityModel);
            if (EntityModel == null)
            {
                Log.Error("[Factory] Failed to get object from pool");
                return;
            }

            // 5. 从逻辑对象池获取逻辑对象
            ILF2Object logicObject = CreateLogicObject(objType, oid);
            if (logicObject == null)
            {
                Log.Error($"[Factory] Failed to get logic object from pool, type={objType}, oid={oid}");
                LF2ObjectPool.Instance.Release(EntityModel);
                return;
            }

            // 5.1 武器对象注入 weapon_strength_list
            if (logicObject is LF2WeaponBase weaponBase)
            {
                var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
                if (charData?.weapon_strength_list?.Count > 0)
                    weaponBase.SetWeaponStrengthList(charData.weapon_strength_list);
            }

            // 6. 设置逻辑对象并初始化
            EntityModel.SetLogicObject(logicObject, task);

            if (logicObject is LF2LivingObject living)
            {
                // 7. 过滤纯音效对象（pic=999, wait=0, next=1000）——播放 sound 后直接 Release
                int action = (task.opoint.action == 0) ? 999 : task.opoint.action;
                var frameData = living.GetFrameDataById(action);
                Debug.Log($"[OPointFactory] ProcessCreateObject: oid={oid}, action={action}, frameData={frameData?.frameId}, pic={frameData?.pic}, wait={frameData?.wait}, next={frameData?.next}, sound={frameData?.sound}");
                if (frameData != null && frameData.pic == 999 && frameData.wait == 0 && frameData.next == 1000)
                {
                    Debug.Log($"[OPointFactory] Pure sound frame detected, playing sound: {frameData.sound}");
                    if (!string.IsNullOrEmpty(frameData.sound))
                        AppManager.Instance?.SoundPlayer?.PlaySfx(frameData.sound);
                    LF2ObjectPool.Instance?.Release(EntityModel);
                    LF2ReferencePool.Instance?.Release(logicObject);
                    return;
                }

                PostInitLiving(living, task.parent, task.opoint, objType, 0f);
            }
        }

        // ========== 多对象创建 ==========

        /// <summary>
        /// 处理多对象创建任务
        /// 对应 FLF match.js:355-392 (case 'create_multiple_objects')
        /// </summary>
        private void ProcessCreateMultipleObjects(OPointCreateMultipleTask task)
        {
            int oid = task.opoint.oid;
            if (oid <= 0 || task.number <= 0) return;

            var def = GameDataManager.Instance?.GetObjectById(oid);
            if (def == null)
            {
                Log.Error($"[Factory] Object {oid} not exists");
                return;
            }

            int objType = def.type;

            if (objType == 0)
            {
                Log.Warn($"[Factory] Character creation via opoint not supported, oid={oid}");
                return;
            }

            // 对应反汇编 0x004225B6：dvz_i = i * 10.0 / (count-1) - 5.0，固定范围 [-5, +5]
            List<float> vzArray = ListPool<float>.Get();
            if (task.number == 1)
            {
                vzArray.Add(0f);
            }
            else
            {
                for (int i = 0; i < task.number; i++)
                    vzArray.Add(i * 10f / (task.number - 1) - 5f);
            }

            foreach (float vz in vzArray)
            {
                var entityObj = LF2ObjectPool.Instance.Get(out LF2ObjectRenderer EntityModel);
                if (EntityModel == null) break;

                ILF2Object logicObject = CreateLogicObject(objType, oid);
                if (logicObject == null)
                {
                    LF2ObjectPool.Instance.Release(EntityModel);
                    continue;
                }

                if (logicObject is LF2WeaponBase wb)
                {
                    var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
                    if (charData?.weapon_strength_list?.Count > 0)
                        wb.SetWeaponStrengthList(charData.weapon_strength_list);
                }

                var singleTask = new OPointCreateTask
                {
                    opoint = task.opoint,
                    parent = task.parent,
                    team   = task.team,
                    pos    = task.pos,
                    z      = task.z,
                    dir    = task.dir,
                    dvz    = vz,
                };

                EntityModel.SetLogicObject(logicObject, singleTask);

                if (logicObject is LF2LivingObject living)
                {
                    // 过滤纯音效对象（pic=999, wait=0, next=1000）——播放 sound 后直接 Release
                    int action = (task.opoint.action == 0) ? 999 : task.opoint.action;
                    var frameData = living.GetFrameDataById(action);
                    if (frameData != null && frameData.pic == 999 && frameData.wait == 0 && frameData.next == 1000)
                    {
                        if (!string.IsNullOrEmpty(frameData.sound))
                            AppManager.Instance?.SoundPlayer?.PlaySfx(frameData.sound);
                        LF2ObjectPool.Instance?.Release(EntityModel);
                        LF2ReferencePool.Instance?.Release(logicObject);
                        continue;
                    }

                    PostInitLiving(living, task.parent, task.opoint, objType, vz);
                }
            }

            ListPool<float>.Release(vzArray);
        }

        /// <summary>
        /// SetLogicObject 之后的统一后处理
        /// 对应反汇编 opoint 创建后初始化序列（0x004223B5-0x0042277E）
        /// </summary>
        private void PostInitLiving(LF2LivingObject living, LF2LivingObject parent, ObjectPoint op, int objType, float dvz)
        {
            // z_float +1（对应反汇编 0x004223DD：new.z_float = parent.z_float + 1.0）
            living.PS.z += 1f;

            if (parent != null)
            {
                // team_side 继承（对应反汇编 0x0042251F：new.team_side = parent.team_side）
                living.TeamSide = parent.TeamSide;

                // owner_id 继承链（对应反汇编 0x004224F8-0x0042250B）
                living.OwnerId = parent.OwnerId > -1 ? parent.OwnerId : parent.StableId;
            }

            // type==5 或 52 特殊 HP 初始化（对应反汇编 0x00422687-0x004226C3）
            if (objType == 5 || objType == 52)
            {
                living.Health.HP     = 10;
                living.Health.MP     = 10;
                living.Health.HPBound = 10;
            }

            // type==3 且 parent 处于 state 3003(teleport) 时互设 itr_rest（对应反汇编 0x0042262A-0x0042267F）
            if (parent != null && objType == 3 && parent.Frame?.D?.state == 3003)
            {
                parent.ItrRest?.SetVrest(living.StableId, 10);
                living.ItrRest?.SetVrest(parent.StableId, 10);
            }

            // kind==2 追踪绑定（对应反汇编 0x00422729-0x0042277E，仅限 type=3 specialattack）
            if (op.kind == 2 && parent != null && objType == 3)
            {
                parent.TrackerFlag  = 1;
                living.TrackerFlag  = -1;
                parent.TrackerChild = living;
                living.TrackerParent = parent;
            }

            // 多对象 dvz 侧偏 vx（对应反汇编 0x004225E8-0x00422627）
            // dvz 影响 vx 方向：向左扩散时 vx 减小，向右扩散时 vx 增大
            if (dvz != 0f)
            {
                float absDvz = Mathf.Abs(dvz);
                // dir 与 dvz 同向 → 扩散 → vx 加 absDvz；反向 → 收拢 → vx 减 absDvz
                float vxSign = living.PS.vx >= 0f ? 1f : -1f;
                float dvzSign = dvz >= 0f ? 1f : -1f;
                if (vxSign == dvzSign)
                    living.PS.vx += absDvz;
                else
                    living.PS.vx -= absDvz;
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
        /// 对应 FLF factory[type]
        /// </summary>
        private ILF2Object CreateLogicObject(int objectType, int oid)
        {
            // 将 int type 映射到 LF2ObjectType 枚举
            LF2ObjectType objTypeEnum = (LF2ObjectType)objectType;
            // 从逻辑对象池获取对象（池会自动处理 ObjectId 赋值）
            return LF2ReferencePool.Instance.Get(objTypeEnum, oid);
        }
    }
}
