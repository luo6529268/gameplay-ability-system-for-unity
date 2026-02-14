using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.LF2Objects;
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

        private int _lastFlushTick = -1;

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

        public void EnqueueCreateNPCCharacters(OPointCreateNPCTask task)
        {
            _taskQueue.AddLast(task);
        }

        // ========== FlushTasks（处理队列） ==========
        /// <summary>
        /// 处理所有任务并清空队列
        /// 对应 FLF match.js process_tasks()
        /// </summary>
        public void FlushTasks()
        {
            int tick = NTSD.Simulation.SimulationTickDriver.Instance?.CurrentTickIndex ?? Time.frameCount;
            if (_lastFlushTick == tick) return;
            _lastFlushTick = tick;

            // 遍历链表并处理
            var node = _taskQueue.First;
            while (node != null)
            {
                ProcessTask(node.Value);
                node = node.Next;
            }

            // 清空队列
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

                case LF2TaskType.CreateNPCCharacters:
                    ProcessCreateNPCCharacters((OPointCreateNPCTask)task);
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
            var renderer = LF2ObjectPool.Instance.Get();
            if (renderer == null)
            {
                Log.Error("[Factory] Failed to get object from pool");
                return;
            }

            // 5. 从逻辑对象池获取逻辑对象
            ILF2Object logicObject = CreateLogicObject(objType, oid);
            if (logicObject == null)
            {
                Log.Error($"[Factory] Failed to get logic object from pool, type={objType}, oid={oid}");
                LF2ObjectPool.Instance.Release(renderer);
                return;
            }

            // 6. 设置逻辑对象并初始化
            renderer.SetLogicObject(logicObject, task);
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

            // type==0 (character) 跳过
            if (objType == 0)
            {
                Log.Warn($"[Factory] Character creation via opoint not supported, oid={oid}");
                return;
            }

            // 计算 vz 数组（对齐 FLF）
            List<float> vzArray = ListPool<float>.Get();
            int maxNum = Mathf.FloorToInt(task.number / 2f);
            if (task.number % 2 == 1)
            {
                for (int i = -maxNum; i <= maxNum; i++) vzArray.Add(i * task.vz);
            }
            else
            {
                for (int i = -maxNum; i <= maxNum; i++) if (i != 0) vzArray.Add(i * task.vz);
            }

            // 为每个 vz 创建对象
            foreach (float vz in vzArray)
            {
                var renderer = LF2ObjectPool.Instance.Get();
                if (renderer == null) break;

                ILF2Object logicObject = CreateLogicObject(objType, oid);
                if (logicObject == null)
                {
                    LF2ObjectPool.Instance.Release(renderer);
                    continue;
                }

                // 创建修改后的 task（vz 不同）
                var singleTask = new OPointCreateTask
                {
                    opoint = task.opoint,
                    parent = task.parent,
                    team = task.team,
                    pos = task.pos,
                    z = task.z,
                    dir = task.dir,
                    dvz = vz  // ← 使用计算的 vz
                };

                renderer.SetLogicObject(logicObject, singleTask);
            }

            ListPool<float>.Release(vzArray);
        }

        private void ProcessCreateNPCCharacters(OPointCreateNPCTask task)
        {
            Log.Warn("[OPointFactory] NPC spawn not implemented: {0} NPCs for team {1}", task.number, task.team);
        }


        #region Initialize Object
        //private void InitializeObject(GameObject obj, OPointCreateTask task, int objType)
        //{
        //    var animator = obj.GetComponentInChildren<LF2CharacterAnimator>();
        //    if (animator == null) { obj.transform.position = new Vector3(task.pos.x, -task.pos.y, task.pos.z); return; }

        //    animator.Team = task.team;
        //    animator.ObjectType = objType;
        //    if (task.parent != null) animator.OwnerId = task.parent.StableId;

        //    // P4: 确保初始化顺序 - 先设置方向和帧，再计算位置
        //    string dir = CalculateDirection(task.opoint.facing, task.dir);
        //    if (animator.ps != null) animator.ps.dir = dir;

        //    // 先切换到目标帧，确保 CurrentFrame 有效
        //    int action = task.opoint.action == 0 ? 999 : task.opoint.action;
        //    animator.TransitionToFrame(action, 20);

        //    // 现在 CurrentFrame 已就绪，可以计算位置
        //    float dirH = (dir == "right") ? 1f : -1f;
        //    if (animator.ps != null)
        //    {
        //        if (objType == 3)
        //        {
        //            InitSpecialAttackPosition(animator, task, dir);
        //        }
        //        else
        //        {
        //            animator.ps.x = task.pos.x;
        //            animator.ps.y = task.pos.y;
        //            animator.ps.z = task.z;
        //        }
        //        animator.ps.vx = dirH * task.opoint.dvx;
        //        animator.ps.vy = task.opoint.dvy;
        //        animator.ps.vz = (task.opoint.dvx != 0) ? task.dvz : 0f;
        //    }
        //}

        //private void InitializeObjectMultiple(GameObject obj, OPointCreateTask task, int objType, float vz)
        //{
        //    var animator = obj.GetComponentInChildren<LF2CharacterAnimator>();
        //    if (animator == null) { obj.transform.position = new Vector3(task.pos.x, -task.pos.y, task.z + vz); return; }

        //    animator.Team = task.team;
        //    animator.ObjectType = objType;
        //    if (task.parent != null) animator.OwnerId = task.parent.StableId;

        //    // P4: 确保初始化顺序
        //    // 0.1: 基于新对象最终 dir 计算偏置，不用 parentDir
        //    string dir = CalculateDirection(task.opoint.facing, task.dir);
        //    if (animator.ps != null) animator.ps.dir = dir;

        //    int action = task.opoint.action == 0 ? 999 : task.opoint.action;
        //    animator.TransitionToFrame(action, 20);

        //    if (animator.ps != null)
        //    {
        //        if (objType == 3)
        //        {
        //            InitSpecialAttackPosition(animator, task, dir);
        //        }
        //        else
        //        {
        //            animator.ps.x = task.pos.x;
        //            animator.ps.y = task.pos.y;
        //            animator.ps.z = task.z;
        //        }
        //        // 0.1: vx 偏置基于新对象最终 dir
        //        // FLF: dir=="left" → vx += abs(vz); dir=="right" → vx -= abs(vz)
        //        float baseVx = ((dir == "right") ? 1f : -1f) * task.opoint.dvx;
        //        float vzOffset = (dir == "left") ? Mathf.Abs(vz) : -Mathf.Abs(vz);
        //        animator.ps.vx = baseVx + vzOffset;
        //        animator.ps.vy = task.opoint.dvy;
        //        animator.ps.vz = vz;
        //    }
        //}

        /// <summary>
        /// FLF specialattack.init: set_pos(0,0,z) then coincideXY(pos, make_point(frame.D,'center'))
        /// </summary>
        //private void InitSpecialAttackPosition(LF2CharacterAnimator animator, OPointCreateTask task, string dir)
        //{
        //    animator.ps.z = task.z;
        //    // coincideXY: 将 opoint 位置与对象 center 对齐
        //    var frame = animator.CurrentFrame;
        //    float centerX = frame?.centerx ?? 0;
        //    float centerY = frame?.centery ?? 0;
        //    float spriteW = animator.GetSpriteWidthPxForCollision();

        //    float offsetX = (dir == "right") ? centerX : (spriteW - centerX);
        //    animator.ps.x = task.pos.x - offsetX;
        //    animator.ps.y = task.pos.y - centerY;
        //}

        private string CalculateDirection(int facing, string parentDir)
        {
            int face = facing >= 20 ? facing % 10 : facing;
            if (face == 0) return parentDir;
            if (face == 1) return (parentDir == "right") ? "left" : "right";
            if (face >= 2 && face <= 10) return "right";
            if (face >= 11 && face <= 19) return "left";
            return parentDir;
        }

        private GameObject GetPrefab(int oid, int objType)
        {
            if (_oidPrefabMap.TryGetValue(oid, out var p1)) return p1;
            if (_typePrefabMap.TryGetValue(objType, out var p2)) return p2;
            return null;
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
            return LF2ObjectLogicPool.Instance.Get(objTypeEnum, oid);
        }
        #endregion
    }
}
