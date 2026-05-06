using UnityEngine;
using NTSD.App;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
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
    public class LF2SpecialAttack : LF2Entity
    {
        // ========== 技能专属字段（不在 LF2Entity 的） ==========

        /// <summary>交互冷却（技能也有 itr 碰撞冷却）</summary>
        public override LF2ItrRestTracker ItrRest { get; protected set; }

        /// <summary>生命值（技能耐久/存活帧数等）</summary>
        public override LF2Health Health { get; protected set; } = new LF2Health();
        // ========== 配置字段 ==========
        private int _objectId;
        private LF2LivingObject _parent;
        private int _lastState = -1;

        // ========== 状态机字段 ==========
        public bool NoBounce { get; set; }

        // ========== 追踪系统 ==========
        private LF2LivingObject _chasingTarget;
        private readonly System.Collections.Generic.Dictionary<int, int> _chasedCounts
            = new System.Collections.Generic.Dictionary<int, int>();
        private bool _hitFaFired;

        // ========== 公开属性 ==========
        public LF2LivingObject Parent => _parent;

        // ========== ILF2Object 实现 ==========
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.SpecialAttack;
        // ========== 初始化方法 ==========

        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            AllocateStableId();

            // 初始化基类字段
            PS = new PhysicsState();
            Trans = new FrameTransistor(this);
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();

            // 初始化状态处理器
            InitializeStates();

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

            Renderer = renderer;
            SimulationTickDriver.Instance?.World?.Register(this);
        }

        protected override void InitializeStates()
        {
            _states[15] = State_15;
            _states[1002] = State_1002;
            _states[LF2States.ProjectileFlying] = State_3000;
            _states[LF2States.ProjectileHiting] = State_3001;
            _states[LF2States.ProjectileHit] = State_3002;
            _states[LF2States.ProjectileTeleport] = State_3003;
            _states[LF2States.ObjectFlying] = State_3005;
            _states[LF2States.ObjectExpanding] = State_3006;
        }

        protected override bool OnGenericStateEvent(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "TU":
                    Generic_TU();
                    return false;
                case "frame":
                    Generic_Frame();
                    return false;
                case "frame_force":
                case "TU_force":
                    Generic_Force();
                    return true;
                case "leaving":
                    Generic_Leaving();
                    return false;
                case "die":
                    Generic_Die();
                    return true;
                default:
                    return false;
            }
        }

        #region Generic State Handlers

        private void Generic_TU()
        {
            Interaction();
            CharacterMechanics.Dynamics(PS);

            var frame = Frame.D;
            if (frame != null && frame.hit_a != 0)
            {
                Health.HP -= frame.hit_a;
            }
        }

        private void Generic_Frame()
        {
            var frame = Frame.D;
            if (frame == null) return;

            if (frame.opoint.HasValue && frame.opoint.Value.oid > 0)
            {
                CreateObject(frame.opoint.Value);
            }

            if (!string.IsNullOrEmpty(frame.sound))
            {
                PlaySound(frame.sound);
            }

            if (Frame.N == 15)
            {
                Trans.Frame(1000, 0);
            }
        }

        private void Generic_Force()
        {
            var frame = Frame.D;
            if (frame == null) return;

            if (frame.hit_j != 0)
            {
                float dvz = frame.hit_j - 50;
                PS.vz = dvz;
            }
        }

        private void Generic_Leaving()
        {
            if (IsLeavingBoundary(200))
            {
                Trans.Frame(1000, 0);
            }
        }

        private void Generic_Die()
        {
            var frame = Frame.D;
            if (frame != null && frame.hit_d != 0)
            {
                Trans.Frame(frame.hit_d, 0);
            }
        }

        #endregion

        #region Specific State Handlers

        private bool State_15(string eventType, object eventData)
        {
            if (eventType == "TU")
            {
                var frame = Frame.D;
                if (frame != null && frame.dvx != 0)
                {
                    PS.vx = Dirh() * frame.dvx;
                }
                return true;
            }
            return false;
        }

        private bool State_1002(string eventType, object eventData)
        {
            if (eventType == "state_entry")
            {
                NoBounce = (Parent?.PS?.y ?? 0) == 0;
                return true;
            }
            if (eventType == "TU")
            {
                if (PS.y == 0 && PS.vy > 0)
                {
                    if (NoBounce)
                    {
                        Trans.Frame(1000, 0);
                    }
                    else if (GetSpeed() > NTSDGlobal.Gameplay.WeaponBounceupLimit)
                    {
                        Trans.Frame(10, 0);
                        PS.vy = NTSDGlobal.Gameplay.WeaponBounceupSpeedY;
                        if (PS.vx != 0) PS.vx = Mathf.Sign(PS.vx) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                        if (PS.vz != 0) PS.vz = Mathf.Sign(PS.vz) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;
                    }
                }
                return true;
            }
            return false;
        }

        private bool State_3000(string eventType, object eventData)
        {
            if (eventType == "TU")
            {
                ProcessChaseLogic();
                return true;
            }
            return false;
        }

        private bool State_3001(string eventType, object eventData)
        {
            if (eventType == "TU")
            {
                ProcessChaseLogic();
                return true;
            }
            return false;
        }

        private bool State_3002(string eventType, object eventData)
        {
            return false;
        }

        private bool State_3003(string eventType, object eventData)
        {
            if (eventType == "TU")
            {
                ProcessChaseLogic();
                return true;
            }
            return false;
        }

        private bool State_3005(string eventType, object eventData)
        {
            if (eventType == "TU")
            {
                ProcessChaseLogic();
                return true;
            }
            return false;
        }

        private bool State_3006(string eventType, object eventData)
        {
            if (eventType == "TU")
            {
                ProcessChaseLogic();
                return true;
            }
            return false;
        }

        private void ProcessChaseLogic()
        {
            var frame = Frame.D;
            if (frame == null) return;

            int hitFa = frame.hit_Fa;

            // hit_Fa=10: 反汇编 0x404E78
            // vx < 0 → vx -= 1.1; vx >= 0 → vx += 1.1; clamp vx to [-30, 30]; if y <= 3 → y = 3
            if (hitFa == 10)
            {
                if (PS.vx < 0f) PS.vx -= 1.1f;
                else PS.vx += 1.1f;
                if (PS.vx > 30f) PS.vx = 30f;
                else if (PS.vx < -30f) PS.vx = -30f;
                if (PS.y <= 3f) PS.y = 3f;
                return;
            }

            // N-11: hit_Fa=11 — one-shot spawn + nearest-enemy search, then tracking
            // 反汇编 Entity_FrameLogic case 11 (0x004030C0)
            if (hitFa == 11 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa11Spawn();
                // After spawn: find nearest enemy, write to OwnerEntityIndex
                // If no target found → HP=0 (self-destruct)
                if (!ApplyHitFa11FindTarget())
                {
                    Health.HP = 0;
                    return;
                }
                // Fall through to tracking (loc_404F13 → case 11 path → loc_405623 → loc_405784)
            }

            // N-13: hit_Fa=13 — one-shot enemy-seeking entity spawn on frame entry
            if (hitFa == 13 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa13Spawn();
                return;
            }

            // hit_Fa=8 — one-shot homing scatter spawn (oid=225)
            // 反汇编 Entity_FrameLogic case 8 (0x004030C0)
            if (hitFa == 8 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa8Spawn();
                return;
            }

            // hit_Fa=5 — one-shot ally heal spawn (oid=219)
            // 反汇编 Entity_FrameLogic case 5 (0x004030C0)
            if (hitFa == 5 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa5Spawn();
                return;
            }

            // hit_Fa=6/9 — shared path (反汇编 Entity_FrameLogic case 6/9, 0x004030C0)
            // case 6: oid=220, v221(cap)=0(no outer loop), v217(max)=7
            // case 9: oid=rand(2)+221, v221(cap)=4, v217(max)=10
            if ((hitFa == 6 || hitFa == 9) && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa6Or9Spawn(hitFa);
                return;
            }

            // hit_Fa=3 — per-frame nearest-enemy search + case 1 tracking
            // 反汇编 loc_40356E: cmp eax, 4/5/6/7 全不匹配 → loc_403649 nearest-enemy search
            // 搜索后 OwnerEntityIndex != -1 → loc_404F13 (case 1 tracking)；否则 HP=0
            if (hitFa == 3)
            {
                if (!ApplyHitFa11FindTarget())
                {
                    Health.HP = 0;
                    return;
                }
                ApplyHitFa3Tracking();
                return;
            }

            // Dispatch tracking by hit_Fa
            // 反汇编 loc_404F13: cmp eax, 1
            if (hitFa == 1)
            {
                ApplyHitFa1Tracking();
                return;
            }

            // 反汇编 loc_405132: case 4/14/12/2/7
            if (hitFa == 2 || hitFa == 4 || hitFa == 7 || hitFa == 12 || hitFa == 14)
            {
                ApplyHitFa2_14Tracking(hitFa);
                return;
            }

            // 反汇编 loc_405623: case 11 fallthrough (HP/active check + vx±2.0 tracking)
            if (hitFa == 11)
            {
                ApplyHitFa11Tracking();
            }
        }

        // ========== N-11: hit_Fa=11 爆炸生成 ==========
        // 反汇编 Entity_FrameLogic case 11 (0x004030C0)
        private void ApplyHitFa11Spawn()
        {
            float x = PS.x, y = PS.y, z = PS.z;
            float vx = PS.vx, vy = PS.vy, vz = PS.vz;
            bool facingRight = PS.dir == "right";
            int facing = facingRight ? 0 : 1;

            // 反汇编 0x403102~0x40379E: 14次 sub_402C00 调用
            // 参数映射: arg_10=[ecx+10h]=z, arg_14=[ecx+14h]=y, arg_18=[ecx+18h]=x
            // 1. oid=211, frame=109, pos=self, vel=self, facing=self
            SpawnEntityDirect(211, 109, x, y, z, vx, vy, vz, facing);
            // 2. oid=221, frame=81, y-100, vel=self, facing=self
            SpawnEntityDirect(221, 81, x, y - 100, z, vx, vy, vz, facing);
            // 3. oid=212, frame=100, z+80, y-3, vz-7, facing=0
            SpawnEntityDirect(212, 100, x, y - 3, z + 80, vx, vy, vz - 7f, 0);
            // 4. oid=212, frame=100, z+100, y-3, facing=0
            SpawnEntityDirect(212, 100, x, y - 3, z + 100, vx, vy, vz, 0);
            // 5. oid=212, frame=100, z+80, y-3, vz+7, facing=0
            SpawnEntityDirect(212, 100, x, y - 3, z + 80, vx, vy, vz + 7f, 0);
            // 6. oid=212, frame=100, z-80, y-3, vz-7, facing=1
            SpawnEntityDirect(212, 100, x, y - 3, z - 80, vx, vy, vz - 7f, 1);
            // 7. oid=212, frame=100, z-100, y-3, facing=1
            SpawnEntityDirect(212, 100, x, y - 3, z - 100, vx, vy, vz, 1);
            // 8. oid=212, frame=100, z-80, y-3, vz+7, facing=1
            SpawnEntityDirect(212, 100, x, y - 3, z - 80, vx, vy, vz + 7f, 1);
            // 9. oid=211, frame=50, x-5, y-1, z-30, facing=1
            SpawnEntityDirect(211, 50, x - 5, y - 1, z - 30, vx, vy, vz, 1);
            // 10. oid=211, frame=50, x-5, y-1, z+30, facing=1
            SpawnEntityDirect(211, 50, x - 5, y - 1, z + 30, vx, vy, vz, 1);
            // 11. oid=211, frame=50, x+2, y-1, z-30, facing=0
            SpawnEntityDirect(211, 50, x + 2, y - 1, z - 30, vx, vy, vz, 0);
            // 12. oid=211, frame=50, x+2, y-1, z+30, facing=0
            SpawnEntityDirect(211, 50, x + 2, y - 1, z + 30, vx, vy, vz, 0);
            // 13. oid=211, frame=50, x-9, y-1, z=self, facing=1
            SpawnEntityDirect(211, 50, x - 9, y - 1, z, vx, vy, vz, 1);
            // 14. oid=211, frame=50, x+6, y-1, z=self, facing=0
            SpawnEntityDirect(211, 50, x + 6, y - 1, z, vx, vy, vz, 0);
        }

        // ========== N-11: hit_Fa=11 寻找最近敌方目标 ==========
        // 反汇编 Entity_FrameLogic loc_403649 (0x004030C0)
        // hit_Fa=11 one-shot 和 hit_Fa=3 per-frame 共用
        // 遍历所有实体，找最近敌方（active, HP>0, 非武器, 不同data, abs(z_diff)<=2）
        // 写入 OwnerEntityIndex；若无目标返回 false
        private bool ApplyHitFa11FindTarget()
        {
            // 检查 [ecx+2F8h] (HealTimer index) 是否有效，取其 data 作为 saved_data
            // saved_data 用于排除与 HealTimer 指向实体相同 data 的目标
            LF2CharacterDataWrapper savedWrapper = null;
            if (HealTimer >= 0)
            {
                var allObjs = new System.Collections.Generic.List<LF2LivingObject>(16);
                Match?.GetAllLivingObjects(allObjs);
                for (int i = 0; i < allObjs.Count; i++)
                {
                    if (allObjs[i].StableId == HealTimer && !allObjs[i].Dead)
                    {
                        savedWrapper = allObjs[i].FrameCache?.Wrapper;
                        break;
                    }
                }
            }

            // 检查已有 OwnerEntityIndex 是否仍然有效
            if (OwnerEntityIndex >= 0)
            {
                var allObjs = new System.Collections.Generic.List<LF2LivingObject>(16);
                Match?.GetAllLivingObjects(allObjs);
                for (int i = 0; i < allObjs.Count; i++)
                {
                    var obj = allObjs[i];
                    if (obj.StableId != OwnerEntityIndex) continue;
                    if (obj.Dead) break;
                    if (obj.Health == null || obj.Health.HP <= 0) break;
                    // 反汇编 0x403600: cmp [ecx+edx*8+7ACh], 0Eh — 躺地状态排除
                    if (obj.GetState() == LF2States.Lying) break;
                    var objFrame = obj.Frame?.D;
                    if (objFrame != null && objFrame.hit_Fa == 14) break;
                    float zDiff = obj.PS.z - PS.z;
                    if (Mathf.Abs(zDiff) > 2) break;
                    // 同 data（同角色类型）→ 排除
                    if (obj.FrameCache?.Wrapper == FrameCache?.Wrapper) break;
                    if (savedWrapper != null && obj.FrameCache?.Wrapper == savedWrapper) break;
                    // 目标仍然有效，直接进入追踪
                    return true;
                }
            }

            // 遍历所有实体寻找最近敌方目标
            // 反汇编 loc_403649: 遍历 0~399，跳过 self/inactive/武器/同data/savedData/hit_Fa==14/abs(z)>2
            var candidates = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(candidates);

            int bestIndex = -1;
            float bestDist = 10000f;

            for (int i = 0; i < candidates.Count; i++)
            {
                var obj = candidates[i];
                if (obj == null || obj.PS == null) continue;
                if (ReferenceEquals(obj, this)) continue;
                if (obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (obj is LF2WeaponBase) continue;  // [+6F8h] != 0 → 武器
                // 反汇编 0x403600: cmp [ecx+edx*8+7ACh], 0Eh — 躺地状态排除
                if (obj.GetState() == LF2States.Lying) continue;
                // 同 data（同角色类型）→ 排除
                if (obj.FrameCache?.Wrapper == FrameCache?.Wrapper) continue;
                if (savedWrapper != null && obj.FrameCache?.Wrapper == savedWrapper) continue;
                var objFrame = obj.Frame?.D;
                if (objFrame != null && objFrame.hit_Fa == 14) continue;
                float zDiff = obj.PS.z - PS.z;
                if (Mathf.Abs(zDiff) > 2) continue;

                float dist = Mathf.Abs(obj.PS.x - PS.x) + Mathf.Abs(zDiff);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = obj.StableId;
                }
            }

            if (bestIndex < 0)
                return false;

            OwnerEntityIndex = bestIndex;
            return true;
        }

        // ========== hit_Fa=3 per-frame 追踪逻辑 ==========
        // 反汇编 0x405656 (Entity_FrameLogic)
        // 无 HP/active 检查；vx ±= 0.7（x 方向追踪），vz ±= 0.17（z 方向追踪，±10 死区），vx clamp ±16，vz clamp ±2.4
        private void ApplyHitFa3Tracking()
        {
            LF2LivingObject target = FindOwnerTarget();
            if (target == null) return;  // 反汇编无 HP/active 检查，但 target 必须存在

            // x 方向追踪: dbl_443288=0.7
            if (target.PS.x > PS.x)
                PS.vx += 0.7f;
            else if (target.PS.x < PS.x)
                PS.vx -= 0.7f;

            // z 方向追踪: ±10 死区, dbl_443228=0.17
            if (target.PS.z > PS.z + 10)
                PS.vz += 0.17f;
            else if (target.PS.z < PS.z - 10)
                PS.vz -= 0.17f;

            // vx clamp ±16.0: dbl_443220=16.0, 40300000h=16.0
            if (PS.vx > 16.0f) PS.vx = 16.0f;
            else if (PS.vx < -16.0f) PS.vx = -16.0f;

            // vz clamp ±2.4: dbl_443210=2.4, dbl_443208=-2.4
            if (PS.vz > 2.4f) PS.vz = 2.4f;
            else if (PS.vz < -2.4f) PS.vz = -2.4f;

            // facing update
            SwitchDir(PS.vx >= 0 ? "right" : "left");
        }

        // ========== N-11: hit_Fa=11 per-frame 追踪逻辑 ==========
        // 反汇编 loc_405623 → loc_405784 (0x004030C0)
        // HP/active 检查：if HP > 0 AND target active → return（不追踪）
        // else → 执行追踪：vx ±= 2.0（基于 vx 符号），vx clamp ±17.0，facing
        private void ApplyHitFa11Tracking()
        {
            LF2LivingObject target = FindOwnerTarget();

            // 反汇编 loc_405640:
            // cmp [ecx+2FCh](HP), 0; jle → 0x405784（HP<=0 → 执行追踪）
            // cmp [edi+esi+4](target active), 0; jz → 0x405784（target 不活跃 → 执行追踪）
            // if HP > 0 AND target active → return（不执行追踪）
            bool targetActive = target != null && !target.Dead;
            bool selfHpPositive = Health != null && Health.HP > 0;
            if (selfHpPositive && targetActive)
                return;

            // 反汇编 loc_405784: vx ±= 2.0（基于 vx 符号），dbl_443298=2.0
            if (PS.vx < 0)
                PS.vx -= 2.0f;
            else
                PS.vx += 2.0f;

            // vx clamp ±17.0 (dbl_443200=17.0, dbl_4431F8=-17.0)
            if (PS.vx > 17.0f) PS.vx = 17.0f;
            else if (PS.vx < -17.0f) PS.vx = -17.0f;

            // facing update
            SwitchDir(PS.vx >= 0 ? "right" : "left");
        }

        // ========== N-13: hit_Fa=13 敌方追踪实体生成 ==========
        // 反汇编 Entity_FrameLogic case 13 (0x004030C0)
        private void ApplyHitFa13Spawn()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            // 收集敌方存活非武器目标列表
            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            var enemies = new System.Collections.Generic.List<int>(8);
            for (int i = 0; i < allObjects.Count; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.PS == null) continue;
                if (obj.Dead) continue;
                if (obj.Health != null && obj.Health.HP <= 0) continue;
                if (obj.Team == Team) continue;
                if (obj is LF2WeaponBase) continue;
                enemies.Add(obj.StableId);
            }

            // 选择随机目标（无敌方时用自身 StableId）
            int targetStableId = (enemies.Count > 0)
                ? enemies[UnityEngine.Random.Range(0, enemies.Count)]
                : StableId;

            // 生成 oid=228，继承 pos/vel/team/facing，y += rand(7)-3
            float spawnY = PS.y + UnityEngine.Random.Range(0, 7) - 3;
            int facing = PS.dir == "right" ? 0 : 1;
            var op = new ObjectPoint
            {
                oid = 228, kind = 0, action = 0,
                dvx = 0, dvy = 0, dvz = 0,
                facing = facing
            };
            var t620 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            t620.opoint = op; t620.parent = _parent; t620.team = Team;
            t620.pos = new Vector3(PS.x, spawnY, PS.z); t620.z = PS.z; t620.dir = PS.dir; t620.dvz = 0;
            t620.useDirectVelocity = true; t620.directVx = PS.vx; t620.directVy = PS.vy; t620.directVz = PS.vz;
            factory.EnqueueCreateObject(t620);

            // 注：OwnerEntityIndex 通过 OPointCreateTask.ownerEntityIndex 传递给工厂
            // 工厂在 PostInitLiving 后写入 living.OwnerEntityIndex
            StateUpdate("die", null);
        }

        // ========== hit_Fa=8: oid=225 散射追踪生成 ==========
        // 反汇编 Entity_FrameLogic case 8 (0x004030C0)
        // 扫描敌方（不同team, HP>0, 非武器）→ count=max(3,(n-3)/2+3) if n>4 else 3
        // 每次生成 oid=225，vx=rand(21)-11, vy=3.0-rand(24)*0.25, vz=3.0-rand(24)*0.25
        // OwnerEntityIndex = 随机敌方 StableId；自身失活
        private void ApplyHitFa8Spawn()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            // 收集敌方存活非武器目标列表
            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            var enemies = new System.Collections.Generic.List<int>(8);
            for (int i = 0; i < allObjects.Count; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (obj.Team == Team) continue;
                if (obj is LF2WeaponBase) continue;
                enemies.Add(obj.StableId);
            }

            // count = (enemyCount > 4) ? (enemyCount-3)/2+3 : 3
            // 反汇编: v29 = 3; if (v189 > 4) v29 = (v189-3)/2+3
            int enemyCount = enemies.Count;
            int count = 3;
            if (enemyCount > 4)
                count = (enemyCount - 3) / 2 + 3;

            int facing = PS.dir == "right" ? 0 : 1;
            for (int i = 0; i < count; i++)
            {
                // vx = rand(21)-11, vy = 3.0-rand(24)*0.25, vz = 3.0-rand(24)*0.25
                // 反汇编: vx=[+64]=sub_419D40(21)-11; vy=[+72]=3.0-rand(24)*0.25; vz=[+80]=3.0-rand(24)*0.25
                float vx = UnityEngine.Random.Range(0, 21) - 11;
                float vy = 3.0f - UnityEngine.Random.Range(0, 24) * 0.25f;
                float vz = 3.0f - UnityEngine.Random.Range(0, 24) * 0.25f;

                // OwnerEntityIndex = 随机敌方 StableId（若无敌方则用自身）
                // 反汇编: if (v189) [+1016]=v226[rand(v189)]; else [+1016]=a2
                int ownerIdx = (enemyCount > 0)
                    ? enemies[UnityEngine.Random.Range(0, enemyCount)]
                    : StableId;

                var op = new ObjectPoint
                {
                    oid = 225, kind = 0, action = 0,
                    dvx = 0, dvy = 0, dvz = 0,
                    facing = facing
                };
                var t692 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                t692.opoint = op; t692.parent = _parent; t692.team = Team;
                t692.pos = new Vector3(PS.x, PS.y, PS.z); t692.z = PS.z; t692.dir = PS.dir; t692.dvz = 0;
                t692.useDirectVelocity = true; t692.directVx = vx; t692.directVy = vy; t692.directVz = vz;
                t692.ownerEntityIndex = ownerIdx;
                factory.EnqueueCreateObject(t692);
            }

            // 自身失活
            // 反汇编: *((_BYTE *)this + v2 + 4) = 0; (deactivate self)
            StateUpdate("die", null);
        }

        // ========== hit_Fa=5: oid=219 友方治疗生成 ==========
        // 反汇编 Entity_FrameLogic case 5 (0x004030C0)
        // 扫描友方（同team, HP>0, 非武器）→ 每个友方生成 oid=219
        // vx=(ally.x-self.x)/50, vy=0, vz=0, dir=right
        // OwnerEntityIndex = ally.StableId；自身失活
        private void ApplyHitFa5Spawn()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            // 收集友方存活非武器目标列表
            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            var allies = new System.Collections.Generic.List<LF2LivingObject>(8);
            for (int i = 0; i < allObjects.Count; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (obj.Team != Team) continue;
                if (obj is LF2WeaponBase) continue;
                allies.Add(obj);
            }

            // 为每个友方生成 oid=219
            // 反汇编 case 5: 遍历友方，vx=(ally.x-self.x)/50, vy=0, vz=0, dir=right
            for (int i = 0; i < allies.Count; i++)
            {
                var ally = allies[i];
                float vx = (ally.PS.x - PS.x) / 50.0f;

                var op = new ObjectPoint
                {
                    oid = 219, kind = 0, action = 0,
                    dvx = 0, dvy = 0, dvz = 0,
                    facing = 0
                };
                var t751 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                t751.opoint = op; t751.parent = _parent; t751.team = Team;
                t751.pos = new Vector3(PS.x, PS.y, PS.z); t751.z = PS.z; t751.dir = "right"; t751.dvz = 0;
                t751.useDirectVelocity = true; t751.directVx = vx; t751.directVy = 0f; t751.directVz = 0f;
                t751.ownerEntityIndex = ally.StableId;
                factory.EnqueueCreateObject(t751);
            }

            // 自身失活
            StateUpdate("die", null);
        }

        // ========== hit_Fa=6: oid=220 敌方追踪生成（最多7个）==========
        // ========== hit_Fa=6/9: 共享路径（反汇编 Entity_FrameLogic case 6/9, 0x004030C0）==========
        // case 6: oid=220, v221(cap)=0(外层循环不执行), v217(max)=7
        //         vx=(enemy.x-self.x)/50, vy=-(4+rand(4))
        // case 9: oid=rand(2)+221, v221(cap)=4, v217(max)=10
        //         vx=rand(21)-11, vy=-2.0-rand(40)*0.1667
        // OwnerEntityIndex = enemy.StableId
        private void ApplyHitFa6Or9Spawn(int hitFa)
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            // 反汇编差异参数
            int cap = (hitFa == 9) ? 4 : 0;   // v221: case6=0(无外层限制), case9=4
            int max = (hitFa == 9) ? 10 : 7;  // v217: case6=7, case9=10

            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);

            int spawnCount = 0;
            for (int i = 0; i < allObjects.Count && spawnCount < max; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (obj.Team == Team) continue;
                if (obj is LF2WeaponBase) continue;
                // cap>0: 外层循环限制（case 9 最多 cap 个）
                if (cap > 0 && spawnCount >= cap) break;

                int oid;
                float vx, vy;
                if (hitFa == 6)
                {
                    oid = 220;
                    vx = (obj.PS.x - PS.x) / 50.0f;
                    vy = -(4 + UnityEngine.Random.Range(0, 4));
                }
                else
                {
                    oid = UnityEngine.Random.Range(0, 2) + 221;
                    vx = UnityEngine.Random.Range(0, 21) - 11;
                    vy = -2.0f - UnityEngine.Random.Range(0, 40) * 0.1667f;
                }

                int facing = PS.dir == "right" ? 0 : 1;
                var op = new ObjectPoint
                {
                    oid = oid, kind = 0, action = 0,
                    dvx = 0, dvy = 0, dvz = 0,
                    facing = facing
                };
                var t798 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                t798.opoint = op; t798.parent = _parent; t798.team = Team;
                t798.pos = new Vector3(PS.x, PS.y, PS.z); t798.z = PS.z; t798.dir = PS.dir; t798.dvz = 0;
                t798.useDirectVelocity = true; t798.directVx = vx; t798.directVy = vy; t798.directVz = 0f;
                t798.ownerEntityIndex = obj.StableId;
                factory.EnqueueCreateObject(t798);
                spawnCount++;
            }
        }

        // ========== hit_Fa=1 per-frame 追踪逻辑 ==========
        // 反汇编 Entity_FrameLogic loc_404F13 case 1 (0x004030C0)
        // 无 HP/active 检查；vx±0.85, vz±0.3(±7死区), vy*=0.7142857, y dead zone y+10→±1.2, vx clamp ±13, vz clamp ±2.0, y clamp ±1.0
        private void ApplyHitFa1Tracking()
        {
            LF2LivingObject target = FindOwnerTarget();
            if (target == null) return;  // 反汇编无 HP/active 检查，但 target 必须存在

            // vx: target.x > self.x → vx += 0.85; target.x < self.x → vx -= 0.85
            // 反汇编 0x404F31: cmp [ecx+10h](x整数), [eax+10h](target.x); fadd/fsub dbl_4432D8(0.85)
            if (target.PS.x > PS.x)
                PS.vx += 0.85f;
            else if (target.PS.x < PS.x)
                PS.vx -= 0.85f;

            // vz: ±7 dead zone, step 0.3
            // 反汇编 0x404F79: cmp [ecx+18h](z整数)+7, [eax+18h](target.z); fadd/fsub dbl_4432D0(0.3)
            if (target.PS.z > PS.z + 7)
                PS.vz += 0.3f;
            else if (target.PS.z < PS.z - 7)
                PS.vz -= 0.3f;

            // vy *= 0.7142857
            // 反汇编 0x404FC8: fld [eax+48h](vy); fmul dbl_4432C8(0.7142857); fstp [eax+48h]
            PS.vy *= 0.7142857f;

            // y 方向追踪（直接修改 y 坐标，不通过 vy）
            // 反汇编 0x404FF0: fld [ecx+60h](self.y); fadd 10.0; fcomp [eax+60h](target.y)
            // if self.y+10 < target.y → self.y += 1.2（self 比 target 高，向下移动）
            // if self.y+10 > target.y → self.y -= 1.2（self 比 target 低，向上移动）
            bool targetIsWeapon = target is LF2WeaponBase;
            if (!targetIsWeapon)
            {
                if (PS.y + 10.0f < target.PS.y)
                    PS.y += 1.2f;
                else if (PS.y + 10.0f > target.PS.y)
                    PS.y -= 1.2f;
            }
            else
            {
                // target is weapon: if y > 0 → y += 1.0
                // 反汇编 0x40503B: fld 0.0; fcomp [ecx+60h](self.y); test ah,1; jz; fadd dbl_4432B0(1.0)
                // C0=1 if 0.0 < self.y (i.e., self.y > 0) → self.y += 1.0
                if (PS.y > 0)
                    PS.y += 1.0f;
            }

            // vx clamp ±13.0
            // 反汇编 0x405061: fcomp dbl_4432A8(13.0); mov [ecx+44h],402A0000h(13.0); fcomp dbl_4432A0(-13.0)
            if (PS.vx > 13.0f) PS.vx = 13.0f;
            else if (PS.vx < -13.0f) PS.vx = -13.0f;

            // vz clamp ±2.0
            // 反汇编 0x4050A0: fld [ecx+50h](vz); fcomp dbl_443298(2.0); mov [ecx+54h],40000000h(2.0); fcomp dbl_443290(-2.0)
            if (PS.vz > 2.0f) PS.vz = 2.0f;
            else if (PS.vz < -2.0f) PS.vz = -2.0f;

            // y clamp ±1.0（直接修改 y 坐标）
            // 反汇编 0x4050E5: fcomp dbl_4432B0(1.0); fstp [ecx+64h](y hi); fcomp dbl_443328(-1.0)
            if (PS.y > 1.0f) PS.y = 1.0f;
            else if (PS.y < -1.0f) PS.y = -1.0f;

            // facing update
            SwitchDir(PS.vx >= 0 ? "right" : "left");
        }

        // ========== hit_Fa=2/4/7/12/14 per-frame 追踪逻辑 ==========
        // 反汇编 Entity_FrameLogic loc_405132 (0x004030C0)
        // vx±0.7, case7双次, vz±0.4(±5死区), vy*=0.7142857(2/4/12/14), y dead zone 40→±1.0
        // vx clamp ±14, y clamp ≤1.4, case14 vz clamp ±1.5, case12/4/2 vz clamp ±2.2
        private void ApplyHitFa2_14Tracking(int hitFa)
        {
            LF2LivingObject target = FindOwnerTarget();
            // 反汇编 0x40522D: cmp [ecx+2FCh](self.HP), 0; jle → 跳出
            // 反汇编 0x405239: cmp [edi+esi+4](target active), 0; jnz → 跳出
            if (Health == null || Health.HP <= 0) return;
            bool targetActive = target != null && !target.Dead;
            if (!targetActive) return;

            // vx: target.x > self.x → vx += 0.7; target.x < self.x → vx -= 0.7
            // 反汇编 loc_405132: cmp [ecx+10h],[eax+10h]; jle/jge; fadd/fsub dbl_443288(0.7)
            if (target.PS.x > PS.x)
                PS.vx += 0.7f;
            else if (target.PS.x < PS.x)
                PS.vx -= 0.7f;

            // case 7: double vx adjustment
            // 反汇编 0x00405281: cmp [esp+680h+var_660], 7; jnz; second vx adjust
            if (hitFa == 7)
            {
                if (target.PS.x > PS.x)
                    PS.vx += 0.7f;
                else if (target.PS.x < PS.x)
                    PS.vx -= 0.7f;
            }

            // vz: ±5 dead zone, step 0.4
            // 反汇编: add eax,5 / sub eax,5; fadd/fsub dbl_443280(0.4)
            if (target.PS.z > PS.z + 5)
                PS.vz += 0.4f;
            else if (target.PS.z < PS.z - 5)
                PS.vz -= 0.4f;

            // case 2/4/12/14: vy *= 0.7142857
            // 反汇编 loc_4053C5: cmp eax,2/4/0Ch; jz; fmul dbl_4432C8; [+48h]=vy
            // case 7 不走此路径（jnz loc_40545B）
            if (hitFa == 2 || hitFa == 4 || hitFa == 12 || hitFa == 14)
                PS.vy *= 0.7142857f;

            // y 方向追踪（直接修改 y 坐标）: if target is non-weapon
            // 反汇编 loc_4053C5: cmp [ecx+6F8h],ebx; jnz; fld [ecx+60h](self.y); fadd dbl_443270(40.0); fcomp [eax+60h](target.y)
            // → if self.y+40 < target.y → self.y += 1.0（self 比 target 高，向下）
            // → if self.y+40 > target.y → self.y -= 1.0（self 比 target 低，向上）
            // if target is weapon: if y > 0 → y += 1.0（反汇编 0x40543F: fld 0.0; fcomp [ecx+60h]; if 0.0 < y → y += 1.0）
            bool targetIsWeapon = target is LF2WeaponBase;
            if (!targetIsWeapon)
            {
                if (PS.y + 40.0f < target.PS.y)
                    PS.y += 1.0f;
                else if (PS.y + 40.0f > target.PS.y)
                    PS.y -= 1.0f;
            }
            else
            {
                if (PS.y > 0)
                    PS.y += 1.0f;
            }

            // case 7: proximity check → frame=60, clear vel
            // 反汇编 0x00405348: cmp eax,7; jnz loc_40545B; fld [ecx+48h](vy); fcomp dbl_443278(4.0)
            // → if vy < 4.0 → vy += 0.4; y += vy; if y > -25 → frame=60, clear vel, target.HealTimer=100
            if (hitFa == 7)
            {
                if (PS.vy < 4.0f)
                    PS.vy += 0.4f;
                // 反汇编 0x405368: fld [eax+48h](vy); fadd [eax+60h](y); fstp [eax+60h] — y += vy
                PS.y += PS.vy;
                // 反汇编 0x40537F: cmp [eax+14h](y整数), 0FFFFFFE7h (-25); jle; frame=60, clear vel
                if (PS.y > -25)
                {
                    Trans.Frame(60, 0);
                    PS.vx = 0; PS.vy = 0; PS.vz = 0;
                    // 反汇编 0x4053AE: mov [edx+0E4h], 64h — target.HealTimer = 100
                    var ownerTarget = FindOwnerTarget();
                    if (ownerTarget != null) ownerTarget.HealTimer = 100;
                    return;
                }
            }

            // vx clamp ±14.0
            // 反汇编 0x00405465: fcomp dbl_443268(14.0); mov [ecx+44h],402C0000h; fcomp dbl_443260(-14.0); mov [ecx+44h],C02C0000h
            if (PS.vx > 14.0f) PS.vx = 14.0f;
            else if (PS.vx < -14.0f) PS.vx = -14.0f;

            // y 上限钳制（只有上限，无下限）
            // 反汇编 0x4054A4: fcomp dbl_443258(1.4); test ah,41h; jnz; mov [ecx+60h]=1.4
            if (PS.y > 1.4f) PS.y = 1.4f;

            // case 14: vz clamp ±1.5
            // 反汇编 0x004054D5: fcomp dbl_443250(1.5); mov [ecx+54h],3FF80000h; fcomp dbl_443248(-1.5); mov [ecx+54h],BFF80000h
            // [ecx+50h/54h] = vz (double)
            if (hitFa == 14)
            {
                if (PS.vz > 1.5f) PS.vz = 1.5f;
                else if (PS.vz < -1.5f) PS.vz = -1.5f;
            }
            else
            {
                // case 2/4/12: vz secondary clamp ±2.2
                // 反汇编 0x0040550F: fcomp dbl_443240(2.2); mov [ecx+50h],9999999Ah/40019999h; fcomp dbl_443238(-2.2)
                if (hitFa == 2 || hitFa == 4 || hitFa == 12)
                {
                    if (PS.vz > 2.2f) PS.vz = 2.2f;
                    else if (PS.vz < -2.2f) PS.vz = -2.2f;
                }
            }

            // facing update
            // 反汇编 0x40554F: fcomp 0.0; test ah,41h; jnz; facing=0(left); else facing=1(right)
            // vx > 0 → right; vx <= 0 → left
            SwitchDir(PS.vx > 0 ? "right" : "left");

            // hit_Fa=2: 速度驱动帧切换
            // 反汇编 0x4058FD: cmp edx,2; jnz 0x406030
            // |vx| > 14 → frame=5（不是5/6）; |vx| > 7 → frame=3（不是3/4）
            if (hitFa == 2)
            {
                float absVx = System.Math.Abs(PS.vx);
                int curFrame = Frame?.N ?? -1;
                if (absVx > 14.0f)
                {
                    if (curFrame != 5 && curFrame != 6)
                        Trans.Frame(5, 0);
                }
                else if (absVx > 7.0f)
                {
                    if (curFrame != 3 && curFrame != 4)
                        Trans.Frame(3, 0);
                }
            }
        }

        // ========== 辅助：通过 OwnerEntityIndex 找追踪目标 ==========
        private LF2LivingObject FindOwnerTarget()
        {
            if (OwnerEntityIndex < 0) return null;
            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            for (int i = 0; i < allObjects.Count; i++)
            {
                if (allObjects[i].StableId == OwnerEntityIndex)
                    return allObjects[i];
            }
            return null;
        }

        // ========== sub_402C00 等价：直接速度生成实体 ==========
        private void SpawnEntityDirect(int oid, int frameId, float x, float y, float z,
            float vx, float vy, float vz, int facing)
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            string dir = facing == 0 ? "right" : "left";
            var op = new ObjectPoint
            {
                oid = oid, kind = 0, action = frameId,
                dvx = 0, dvy = 0, dvz = 0,
                facing = facing
            };
            var t1047 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            t1047.opoint = op; t1047.parent = _parent; t1047.team = Team;
            t1047.pos = new Vector3(x, y, z); t1047.z = z; t1047.dir = dir; t1047.dvz = 0;
            t1047.useDirectVelocity = true; t1047.directVx = vx; t1047.directVy = vy; t1047.directVz = vz;
            factory.EnqueueCreateObject(t1047);
        }

        #endregion

        public override void Reset()
        {
            _parent = null;
            _objectId = 0;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
            _hitFaFired = false;
            _chasingTarget = null;
            _chasedCounts.Clear();
            NoBounce = false;
            ShotCount = 0;
            ResetSpark();
            ResetStableId();
        }

        public override void Destroy()
        {
            CreateBrokenEffect();
        }

        // ========== ISimObject 生命周期 ==========

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null) return;

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
                StateUpdate("state_exit");

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                HitStun = 0;
                StateUpdate("state_entry");
                _lastState = Frame.D.state;
            }

            Trans.SetWait(Frame.D.wait, 99);
            Trans.SetNext(Frame.D.next, 99);
            StateUpdate("frame");

            if (!string.IsNullOrEmpty(Frame.D.sound))
                PlaySound(Frame.D.sound);
        }

        /// <summary>
        /// Transit 阶段 - 对应 FLF livingobject.transit()
        /// </summary>
        public override void SimTransit(int tickIndex)
        {
            Trans?.Trans();
        }

        /// <summary>
        /// EntityCollision 阶段 - 对应反汇编 Entity_Collision (sub_4138F0) 共用路径
        /// 技能无 HitStateCount/HitConfirmEa/Fall 字段，仅处理 ShakeTimer（+8h）双向趋零
        /// 反汇编 0x004139A0-0x004139B3
        /// </summary>
        public override void SimEntityCollision(int tickIndex)
        {
            var fD = Frame?.D;

            // 反汇编 0x41392B: GrabbedBy < 0 → return（被持有时跳过）
            if (GrabbedBy < 0) return;

            // 反汇编 0x413957: frame.state==2 → return（Jumping 跳过）
            if (fD != null && fD.state == 2) return;

            if (ShakeTimer > 0) ShakeTimer--;
            else if (ShakeTimer < 0) ShakeTimer++;

            // 反汇编 0x413D0C-0x413D69: frame.mp < 0 && MPEnabled → MP cost
            // +308h(PP) += mp (mp<0, so PP decreases); if PP < mp (signed, only if PP already negative) → frame=next
            if (fD != null && fD.mp < 0 && NTSDGlobal.MPEnabled && Health != null)
            {
                Health.PP += fD.mp; // PP -= |mp|
                if (Health.PP < fD.mp) // signed: only triggers if PP was already negative
                    Trans.Frame(fD.next, 0);
            }

            // 反汇编 0x413DEB: frame==202 → ShakeTimer=20
            if (Frame?.N == 202)
                ShakeTimer = 20;
        }

        /// <summary>
        /// TU 阶段 - 对应 FLF livingobject.TU()
        /// 严格对齐 FLF specialattack.js states
        /// </summary>
        public override void SimTU(int tickIndex)
        {
            int currentState = GetState();

            if (currentState != _lastState)
            {
                _hitFaFired = false;
                StateUpdate("state_entry", null);
                _lastState = currentState;
            }

            StateUpdate("TU", null);

            ItrRest?.Tick();

            if (Health.HP <= 0)
            {
                StateUpdate("die", null);
            }
        }

        // ========== 交互方法 ==========

        /// <summary>
        /// 对应 FLF specialattack.prototype.interaction (specialattack.js:342-395)
        /// </summary>
        public void Interaction()
        {
            if (Team == 0) return;

            var frame = Frame?.D;
            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (frame == null || sceneQuery == null) return;
            if (PS == null) return;

            var itrs = frame.itrs;
            if (itrs == null || itrs.Count == 0) return;

            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return;

            var itrVolumes = PS.GetItrVolumes(itrs, frame.centerx, frame.centery, spriteWidthPx, itrZWidthPx: 0f);
            int count = Mathf.Min(itrs.Count, itrVolumes.Count);

            for (int i = 0; i < count; i++)
            {
                var itr = itrs[i];
                if (itr == null) continue;

                var candidates = sceneQuery.QueryBodies(itrVolumes[i], this);
                if (candidates == null || candidates.Count == 0) continue;

                for (int c = 0; c < candidates.Count; c++)
                {
                    var target = candidates[c];
                    if (!CanInteractTarget(itr, target)) continue;

                    if (!DispatchInteractionByKind(kindService, itr, target)) continue;

                    ItrArestUpdate(itr);
                    target.ItrVrestUpdate(StableId, itr);

                    // 反汇编 Game_FrameUpdate 0x422667/0x42267F：
                    // state 3003（瞬移）命中时 attacker 也设 vrest=10（双向冷却）
                    if (GetState() == LF2States.ProjectileTeleport)
                    {
                        int vrest = itr.vrest > 0 ? itr.vrest : NTSDGlobal.Default.Weapon.VRest;
                        ItrVrestUpdate(target.StableId, new InteractionArea { vrest = vrest });
                    }

                    return;
                }
            }
        }

        private bool CanInteractTarget(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (Team != 0 && target.Team != 0 && Team == target.Team) return false;
            if (!target.ItrVrestTest(StableId)) return false;
            var kindService = Match?.ItrKindService;
            // 反汇编 0x0041A0C9：kind=8/14 可命中武器（绕过 LF2LivingObject 过滤）
            if (itr.kind != 8 && itr.kind != 14)
            {
                if (target is not LF2LivingObject livingTarget || !kindService.ShouldHitTarget(itr.kind, this, livingTarget)) return false;
            }

            return true;
        }

        private bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (kindService != null && kindService.IsAttackKind(itr.kind))
            {
                return TryApplyHit(itr, target);
            }

            switch (itr.kind)
            {
                case 1:
                    return HandlePreInteractionKind1(itr, target);
                case 2:
                    return HandlePreInteractionKind2(itr, target);
                case 3:
                    return HandlePreInteractionKind3(itr, target);
                case 7:
                    return HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        private bool TryApplyHit(InteractionArea itr, LF2Entity target)
        {
            if (!ItrArestTest()) return false;

            if (target is LF2WeaponBase weapon)
            {
                return weapon.Hit(itr, this);
            }

            if (target is LF2SpecialAttack specialAttack)
            {
                return specialAttack.Hit(itr, this);
            }

            if (target is LF2Character character)
            {
                if (PS != null)
                {
                    var attackerPos = new Vector3(PS.x, PS.y, PS.z);
                    return character.Hit(itr, this, attackerPos, default);
                }
            }

            return false;
        }

        private bool HandlePreInteractionKind1(InteractionArea itr, LF2Entity target)
        {
            // FLF specialattack.js 中无 pre_interaction 逻辑，直接返回 false
            return false;
        }

        private bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            return false;
        }

        private bool HandlePreInteractionKind3(InteractionArea itr, LF2Entity target)
        {
            return false;
        }

        private bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            return false;
        }

        /// <summary>
        /// 对应 FLF specialattack.prototype.hit (specialattack.js:398-410)
        /// </summary>
        public bool Hit(InteractionArea itr, LF2Entity attacker)
        {
            // N-10 反汇编 Entity_AI_Update case 9 (entity_type==3 分支)：
            // kind=9 命中 type=3 实体时：播放 broken sound，attacker.FrameDelay=-3；
            // 目标 state==3005 → frame=40；否则 → frame=30、清速度、同步 owner
            if (itr.kind == 9)
            {
                var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
                if (!string.IsNullOrEmpty(charData?.weapon_broken_sound))
                    PlaySound(charData.weapon_broken_sound);

                attacker.FrameDelay = -3;

                int curState = GetState();
                if (curState == LF2States.ObjectFlying) // 3005
                {
                    NoBounce = true; // 反汇编 0x42D92A: byte[+0xEB]=1
                    Trans.Frame(40, 0);
                }
                else
                {
                    // 反汇编 0x0042ED3C: victim.data = attacker.data（data 替换）
                    // 0x0042ED50: victim.owner_slot = attacker.owner_slot
                    var attackerSpecial = attacker as LF2SpecialAttack;
                    if (attackerSpecial?.FrameCache?.Wrapper != null)
                        FrameCache.Load(attackerSpecial.FrameCache.Wrapper);
                    Team = attacker.Team;
                    OwnerEntityIndex = attacker.OwnerEntityIndex >= 0 ? attacker.OwnerEntityIndex : attacker.StableId;
                    NoBounce = true; // 反汇编 0x42DBCF: byte[+0xEB]=1
                    Trans.Frame(30, 0);
                    HitStun = 0;
                    PS.vx = 0f; PS.vy = 0f; PS.vz = 0f;
                }
                return true;
            }

            int state = GetState();
            bool result = false;

            switch (state)
            {
                case LF2States.ProjectileFlying:
                    result = Hit_State3000(attacker, itr);
                    break;
                case LF2States.ObjectExpanding:
                    result = Hit_State3006(attacker, itr);
                    break;
            }

            // N-8 反汇编 0x0042DAAC-0x0042DB06：命中后 victim 根据 oid 自毁
            if (result)
                ApplyPostHitSelfDestruct(attacker);

            return result;
        }

        /// <summary>
        /// 反汇编 Entity_AI_Update 0x0042DAAC-0x0042DB06：N-8
        /// 被命中后根据自身 oid 自毁：
        ///   oid=201(death) && entity_type==0 → deactivate self
        ///   oid=214(flash) && entity_type==0 → self HP=0
        /// entity_type==0 对应 attacker 是非武器（角色）目标
        /// </summary>
        private void ApplyPostHitSelfDestruct(LF2Entity attacker)
        {
            // entity_type of attacker: weapon types are 1/2/3/4/6; characters are 0
            bool attackerIsChar = attacker is LF2Character;
            if (!attackerIsChar) return;

            if (_objectId == 201)
                StateUpdate("die", null);
            else if (_objectId == 214)
                Health.HP = 0;
        }

        private bool Hit_State3000(LF2Entity attacker, InteractionArea itr)
        {
            var frame = Frame.D;

            if (itr.kind == 14)
            {
                Trans.SetWait(0, 20);
                return true;
            }

            if (attacker != null)
            {
                if (Team == attacker.Team && PS.dir == attacker.PS?.dir)
                {
                    return false;
                }
            }

            var frameItr = GetFirstItr(frame);
            if (frameItr != null && frameItr.effect == 3)
            {
                var attackerSA = attacker as LF2SpecialAttack;
                if (attackerSA != null && attackerSA.GetState() == LF2States.ProjectileFlying &&
                    itr.effect != 3 && itr.effect != 2)
                {
                    return true;
                }
            }

            var attackerSpecial = attacker as LF2SpecialAttack;
            if (attackerSpecial != null)
            {
                if (frameItr != null && frameItr.effect != 3 && frameItr.effect != 2 && itr.effect == 3)
                {
                    PS.vx = 0;
                    // N-9 反汇编 Entity_AI_Update 0x0042DBF8 / 0x0042DD94：
                    // 忍偶系变身逻辑
                    int selfOid = _objectId;
                    bool isValidTarget = selfOid == 200 || selfOid == 203 || selfOid == 205
                        || selfOid == 206 || selfOid == 207 || selfOid == 215 || selfOid == 216;

                    if (attackerSpecial._objectId == 209 && isValidTarget)
                    {
                        // Block2: attacker.oid==209 → target.data=karasu, frame=40
                        // 反汇编 0x0042DC4D: target.team=attacker.team, target.[+354h]=attacker.[+354h]
                        // 0x0042DC73: target.data=attacker.data
                        // 0x0042DC80: target.[+70h/74h/78h]=40
                        var karasuWrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(209);
                        if (karasuWrapper != null)
                        {
                            Team = attackerSpecial.Team;
                            OwnerId = attackerSpecial.OwnerId;
                            FrameCache.Load(karasuWrapper);
                            Trans.Frame(40, 0);
                            Frame.PN = 40;
                            // [+78h] 备用帧=40（项目无独立字段，PN 已覆盖主要用途）
                        }
                        return true;
                    }

                    if (attackerSpecial._objectId == 213 && isValidTarget)
                    {
                        // Block3: attacker.oid==213 → 在 data 列表里找 oid=209 的 data
                        // 反汇编 0x0042DE03: 遍历 world data 列表找 oid=0xD1(209)
                        // 找到后: target.data=karasu_data, target.[+70h/74h/78h]=target.[+70h]（保持当前帧）
                        // target.team=attacker.TrackerParent.team, target.[+354h]=attacker.TrackerParent.[+354h]
                        var karasuWrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(209);
                        if (karasuWrapper != null)
                        {
                            int savedFrame = Frame.N;
                            FrameCache.Load(karasuWrapper);
                            Trans.Frame(savedFrame, 0);
                            Frame.PN = savedFrame;
                        }
                        // team/owner 来自 attacker 的 TrackerParent
                        var parent = attackerSpecial.TrackerParent as LF2SpecialAttack;
                        if (parent != null)
                        {
                            Team = parent.Team;
                            OwnerId = parent.OwnerId;
                        }
                        else
                        {
                            Team = attackerSpecial.Team;
                            OwnerId = attackerSpecial.OwnerId;
                        }
                        return true;
                    }

                    Trans.Frame(1000, 0);
                    CreateObjectAt(209, attackerSpecial);
                    return true;
                }

                if (itr.kind == 0)
                {
                    PS.vx = 0;
                    Trans.Frame(20, 0);
                    return true;
                }
            }

            if (itr.kind == 0)
            {
                PS.vx = 0;
                Team = attacker?.Team ?? 0;
                Trans.Frame(30, 0);
                Trans.Trans();
                TUUpdate();
                Trans.Trans();
                TUUpdate();
                return true;
            }

            return false;
        }

        private bool Hit_State3006(LF2Entity attacker, InteractionArea itr)
        {
            var attackerSA = attacker as LF2SpecialAttack;
            if (attackerSA != null)
            {
                int attackerState = attackerSA.GetState();

                if (attackerState == LF2States.ObjectFlying || attackerState == LF2States.ObjectExpanding)
                {
                    Trans.Frame(20, 0);
                    PS.vx = 0;
                    PS.vz = 0;
                    return true;
                }

                if (attackerState == LF2States.ProjectileFlying)
                {
                    PS.vx = (PS.vx > 0 ? -1 : 1) * 7;
                    return true;
                }
            }

            if (itr.kind == 0)
            {
                PS.vx = (PS.vx > 0 ? -1 : 1) * 1;
                if (itr.bdefend > NTSDGlobal.Gameplay.DefendBreakLimit)
                {
                    Health.HP = 0;
                }
                return true;
            }

            return false;
        }

        private static InteractionArea GetFirstItr(LF2FrameData frame)
        {
            if (frame?.itrs == null || frame.itrs.Count == 0) return null;
            return frame.itrs[0];
        }

        // ========== 追踪系统 ==========

        /// <summary>
        /// 从场景查询敌方角色，按距离+重复惩罚排序，选取最近目标。
        /// 对应 FLF specialattack.prototype.chase_target (specialattack.js:424-453)
        /// </summary>
        public LF2LivingObject ChaseTarget()
        {
            var sceneQuery = Match?.SceneQuery;
            if (sceneQuery == null || PS == null) return _chasingTarget;

            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match.GetAllLivingObjects(allObjects);

            LF2LivingObject best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < allObjects.Count; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.PS == null) continue;
                if (obj.Team == Team) continue;
                if (obj.Type != LF2ObjectType.Character) continue;
                if (obj.Health != null && obj.Health.HP <= 0) continue;

                float dx = obj.PS.x - PS.x;
                float dz = obj.PS.z - PS.z;
                float score = UnityEngine.Mathf.Sqrt(dx * dx + dz * dz);

                int chaseCount;
                if (_chasedCounts.TryGetValue(obj.StableId, out chaseCount))
                    score += 500f * chaseCount;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = obj;
                }
            }

            if (best != null)
            {
                _chasingTarget = best;
                if (!_chasedCounts.ContainsKey(best.StableId))
                    _chasedCounts[best.StableId] = 1;
                else
                    _chasedCounts[best.StableId]++;
            }

            return _chasingTarget;
        }

        // ========== 辅助方法 ==========

        public float GetSpeed()
        {
            return Mathf.Sqrt(PS.vx * PS.vx + PS.vy * PS.vy);
        }

        public bool IsLeavingBoundary(float margin)
        {
            if (PS == null) return false;
            float nx = PS.sx + PS.vx;
            float ny = PS.sy + PS.vy;
            float spriteW = Sprite?.GetWidthPx() ?? 0f;

            // FLF background.prototype.leaving: nx+width < -xt || nx > bgWidth+xt || ny < -600 || ny > 100
            if (ny < -600f || ny > 100f) return true;
            if (nx + spriteW < -margin) return true;

            float bgWidth = 1500f; // FLF 默认背景宽度
            var bwm = NTSD.LevelEditor.BoundaryWallManager.Instance;
            if (bwm != null && bwm.TryGetStageBoundsPx(out var bounds))
                bgWidth = bounds.xMaxPx - bounds.xMinPx;

            if (nx > bgWidth + margin) return true;
            return false;
        }

        public void CreateBrokenEffect()
        {
            // 对应 FLF state_exit: if ($.match.broken_list[$.id]) $.brokeneffect_create($.id)
            BrokenEffectCreate(_objectId);
        }

        public void CreateObject(ObjectPoint op)
        {
            if (op.oid <= 0) return;
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = op;
            task.parent = _parent;
            task.team = Team;
            task.pos = new Vector3(PS.x, PS.y, PS.z);
            task.z = PS.z;
            task.dir = PS.dir;
            task.dvz = 0;
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        public void CreateObjectAt(int oid, LF2SpecialAttack source)
        {
            var op = new ObjectPoint { oid = oid, action = 0, facing = 0 };
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = op;
            task.parent = source?._parent;
            task.team = source?.Team ?? 0;
            task.pos = new Vector3(source?.PS?.x ?? 0, source?.PS?.y ?? 0, source?.PS?.z ?? 0);
            task.z = source?.PS?.z ?? 0;
            task.dir = source?.PS?.dir ?? "right";
            task.dvz = 0;
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }

        // ========== 初始化子步骤 ==========

        private void InitializeParent(OPointCreateTask task)
        {
            _parent = task.parent as LF2LivingObject;
            _objectId = task.opoint.oid;
            ObjectId = task.opoint.oid;
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
            var wrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(_objectId);
            FrameCache.Load(wrapper);
            int action = task.opoint.action;
            if (action == 0 && FrameCache.GetFrameDataById(0) == null)
                action = 999;
            Frame.D = FrameCache.GetFrameDataById(action);
            if (action == 0)
                Trans.SetWait(0);
            else
                Trans.Frame(action, 0);
        }

        private void InitializeVelocity(OPointCreateTask task)
        {
            if (task.useDirectVelocity)
            {
                // sub_402C00 直接赋值路径：vx/vy/vz 不经过 Dirh() 乘法
                PS.vx = task.directVx;
                PS.vy = task.directVy;
                PS.vz = task.directVz;
                return;
            }

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
