using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 特殊攻击对象（投射物、能量球等）
    /// 严格对齐 FLF specialattack.js
    ///
    /// 参考：
    /// - FLF specialattack.prototype.init (specialattack.js:303-339)
    /// - FLF specialattack states (specialattack.js:15-254)
    /// </summary>
    public class LF2SpecialAttack : LF2LivingObject
    {
        // ========== 配置字段 ==========
        private int _objectId;
        private ILF2LivingObject _parent;
        private int _lastState = -1;

        // ========== 状态机字段 ==========
        public bool NoBounce { get; set; }

        // ========== 追踪系统 ==========
        private ILF2LivingObject _chasingTarget;

        // ========== 公开属性 ==========
        public ILF2LivingObject Parent => _parent;

        // ========== ILF2Object 实现 ==========
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.SpecialAttack;
        // ========== 初始化方法 ==========

        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            _renderer = renderer;
            AllocateStableId();

            // 初始化基类字段
            PS = new PhysicsState();
            Trans = new FrameTransistor();
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();

            // 设置帧转换回调
            Trans.SetFrameTransitCallback(OnFrameTransit);

            if (!(taskBase is OPointCreateTask task))
            {
                Log.Error("[LF2SpecialAttack] Invalid task type");
                return;
            }

            InitializeParent(task);
            InitializePosition(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializeVelocity(task);
            InitializeHealth();

            SimulationTickDriver.Instance?.World?.Register(this);
        }

        public override void Reset()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);

            _parent = null;
            _objectId = 0;
            _renderer = null;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
            _chasingTarget = null;
            NoBounce = false;

            ResetStableId();
        }

        public override void Destroy()
        {
            LF2SpecialAttackStates.State3000_Exit(this);
        }

        // ========== 帧转换回调 ==========

        protected virtual void OnFrameTransit(int frameId, bool switchDir, int oldLock)
        {
            // 更新帧信息
            Frame.PN = Frame.N;
            Frame.N = frameId;
            
            // 从缓存获取帧数据
            Frame.D = FrameCache?.GetFrameDataById(frameId);

            // 切换方向
            if (switchDir)
            {
                string newDir = (PS.dir == "left") ? "right" : "left";
                SwitchDir(newDir);
            }

            // 调用帧更新
            FrameUpdate();
        }

        // ========== ISimObject 生命周期 ==========

        /// <summary>
        /// Transit 阶段 - 对应 FLF livingobject.transit()
        /// </summary>
        public override void SimTransit(int tickIndex)
        {
            Trans?.Trans();
        }

        /// <summary>
        /// TU 阶段 - 对应 FLF livingobject.TU()
        /// 严格对齐 FLF specialattack.js states
        /// </summary>
        public override void SimTU(int tickIndex)
        {
            int currentState = GetState();

            // 状态进入事件
            if (currentState != _lastState)
            {
                OnStateEntry(currentState);
                _lastState = currentState;
            }

            // Generic TU（所有状态共享）
            LF2SpecialAttackStates.Generic_TU(this);

            // 特定状态 TU
            switch (currentState)
            {
                case 15:
                    LF2SpecialAttackStates.State15_TU(this);
                    break;
                case 1002:
                    LF2SpecialAttackStates.State1002_TU(this);
                    break;
            }

            // State 300X 追踪逻辑（适用于多个状态）
            if (Frame.D != null && (Frame.D.hit_Fa == 1 || Frame.D.hit_Fa == 2 || Frame.D.hit_Fa == 10))
            {
                LF2SpecialAttackStates.State300X_TU(this);
            }

            // ItrRest 递减
            ItrRest?.Tick();

            // 检查死亡
            if (Health.HP <= 0)
            {
                LF2SpecialAttackStates.Generic_Die(this);
            }
        }

        // ========== 状态机方法 ==========

        private void OnStateEntry(int state)
        {
            switch (state)
            {
                case 1002:
                    LF2SpecialAttackStates.State1002_Entry(this);
                    break;
            }
        }

        // ========== 交互方法 ==========

        /// <summary>
        /// 对应 FLF specialattack.prototype.interaction (specialattack.js:342-395)
        /// </summary>
        public void Interaction()
        {
            if (Team == 0) return;
            // TODO: 实现碰撞检测逻辑
        }

        /// <summary>
        /// 对应 FLF specialattack.prototype.hit (specialattack.js:398-410)
        /// </summary>
        public bool Hit(InteractionArea itr, ILF2LivingObject attacker)
        {
            int state = GetState();

            switch (state)
            {
                case 3000:
                    return LF2SpecialAttackStates.State3000_Hit(this, attacker, itr);
                case 3006:
                    return LF2SpecialAttackStates.State3006_Hit(this, attacker, itr);
            }

            return false;
        }

        // ========== 追踪系统 ==========

        /// <summary>
        /// 对应 FLF specialattack.prototype.chase_target (specialattack.js:424-453)
        /// </summary>
        public ILF2LivingObject ChaseTarget()
        {
            // TODO: 实现目标选择逻辑
            return _chasingTarget;
        }

        // ========== 辅助方法 ==========

        public float GetSpeed()
        {
            return Mathf.Sqrt(PS.vx * PS.vx + PS.vy * PS.vy);
        }

        public bool IsLeavingBoundary(float margin)
        {
            // TODO: 实现边界检测
            return false;
        }

        public void CreateObject(ObjectPoint op)
        {
            if (op == null || op.oid <= 0) return;
            var task = new OPointCreateTask
            {
                opoint = op,
                parent = _parent,
                team = Team,
                pos = new Vector3(PS.x, PS.y, PS.z),
                z = PS.z,
                dir = PS.dir,
                dvz = 0
            };
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        public void CreateObjectAt(int oid, LF2SpecialAttack source)
        {
            var op = new ObjectPoint { oid = oid, action = 0, facing = 0 };
            var task = new OPointCreateTask
            {
                opoint = op,
                parent = source?._parent,
                team = source?.Team ?? 0,
                pos = new Vector3(source?.PS?.x ?? 0, source?.PS?.y ?? 0, source?.PS?.z ?? 0),
                z = source?.PS?.z ?? 0,
                dir = source?.PS?.dir ?? "right",
                dvz = 0
            };
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        public void CreateBrokenEffect()
        {
            // TODO: 实现破碎效果
        }

        public void PlaySound(string soundId)
        {
            // TODO: 实现音效播放
        }

        // ========== 初始化子步骤 ==========

        private void InitializeParent(OPointCreateTask task)
        {
            _parent = task.parent;
            _objectId = task.opoint.oid;
            Team = task.team;
        }

        private void InitializePosition(OPointCreateTask task)
        {
            SetPos(0, 0, task.z);

            if (Frame.D == null) return;

            Vector3 centerPoint = MakePointCenter(Frame.D);
            CoincideXYForInit(task.pos, centerPoint);
        }

        private void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(dir);
        }

        private void InitializeFrame(OPointCreateTask task)
        {
            int action = (task.opoint.action == 0) ? 999 : task.opoint.action;
            Trans.Frame(action, 0);
        }

        private void InitializeVelocity(OPointCreateTask task)
        {
            PS.vx = Dirh() * task.opoint.dvx;
            PS.vy = task.opoint.dvy;

            bool hasFrameDvx = (Frame.D != null && Frame.D.dvx != 0);
            PS.vz = hasFrameDvx ? task.dvz : 0f;
        }

        private void InitializeHealth()
        {
            Health.HP = NTSDGlobal.Default.Health.HpFull;
        }

        private Vector3 MakePointCenter(LF2FrameData frame)
        {
            float spriteWidth = Sprite?.GetWidthPx() ?? 0;

            int centerx = frame?.centerx ?? 0;
            int centery = frame?.centery ?? 0;

            float x = (PS.dir == "right")
                ? PS.sx + centerx
                : PS.sx + spriteWidth - centerx;

            float y = PS.sy + centery;
            float z = PS.sz + centery;

            return new Vector3(x, y, z);
        }

        private void CoincideXYForInit(Vector3 targetPos, Vector3 selfPoint)
        {
            float vx = targetPos.x - selfPoint.x;
            float vz = targetPos.z - selfPoint.z;
            PS.x += vx;
            PS.z += vz;
        }
    }
}
