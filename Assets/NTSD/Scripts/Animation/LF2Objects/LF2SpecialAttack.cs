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

            if (frame.opoint != null && frame.opoint.oid > 0)
            {
                CreateObject(frame.opoint);
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

            if (frame.hit_Fa == 1 || frame.hit_Fa == 2)
            {
                if (Health.HP > 0)
                {
                    var target = ChaseTarget();
                    if (target != null)
                    {
                        float dx = target.PS.x - PS.x;
                        float dz = target.PS.z - PS.z;

                        if (PS.vx * Mathf.Sign(dx) < 14)
                        {
                            PS.vx += Mathf.Sign(dx) * 0.7f;
                        }
                        if (PS.vz * Mathf.Sign(dz) < 2.2f)
                        {
                            PS.vz += Mathf.Sign(dz) * 0.4f;
                        }

                        SwitchDir(PS.vx >= 0 ? "right" : "left");
                    }
                }
            }

            if (frame.hit_Fa == 10)
            {
                PS.vx = Mathf.Sign(PS.vx) * 17;
                PS.vz = 0;
            }
        }

        #endregion

        public override void Reset()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);

            _parent = null;
            _objectId = 0;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
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

            if (currentState != _lastState)
            {
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
            if (target is not LF2LivingObject livingTarget || !kindService.ShouldHitTarget(itr.kind, this, livingTarget)) return false;

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
            int state = GetState();

            switch (state)
            {
                case LF2States.ProjectileFlying:
                    return Hit_State3000(attacker, itr);
                case LF2States.ObjectExpanding:
                    return Hit_State3006(attacker, itr);
            }

            return false;
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

            if (itr.kind == 0 || itr.kind == 9)
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
            if (itr.kind == 9)
            {
                PS.vx *= -1;
                PS.z += 0.3f;
                return true;
            }

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

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
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
            var wrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(_objectId);
            FrameCache.Load(wrapper);
            Frame.D = FrameCache.GetFrameDataById(action);
            UnityEngine.Debug.Log($"[LF2SpecialAttack] InitializeFrame: oid={_objectId}, action={action}, wrapper={(wrapper == null ? "NULL" : wrapper.characterData?.frames?.Count.ToString())}, Frame.D={Frame.D?.frameId}");
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
