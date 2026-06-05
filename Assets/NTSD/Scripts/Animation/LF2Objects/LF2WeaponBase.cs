using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Tools;
using NTSD.Simulation;
using NTSD.Animation;
using NTSD.App;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 对齐 C++ release 的武器基类。
    /// </summary>
    public abstract class LF2WeaponBase : LF2Entity
    {
        // ========== 武器专属字段（不在 LF2Entity 的） ==========

        /// <summary>交互冷却（武器也有 itr 碰撞冷却）</summary>
        public override LF2ItrRestTracker ItrRest { get; protected set; }

        /// <summary>生命值（武器耐久度等）</summary>
        public override LF2Health Health { get; protected set; } = new LF2Health();

        /// <summary>控制器（武器由持有者间接控制）</summary>
        public ILF2Controller Controller { get; set; }

        // ========== 配置字段 ==========
        protected int _lastState = -1;

        // ========== 持有者信息 ==========
        protected LF2LivingObject _holdObj;

        // C++ release [entity+3F8h]：投掷者 StableId，投掷后保留，用于回旋镖捕获检测。
        public int PickerStableId
        {
            get => Runtime.PickerStableId;
            set => Runtime.PickerStableId = value;
        }

        // 本帧重力累加量，由 WeaponFlightPhysics 计算，WeaponDynamics 在 y+=vy 后使用
        // 对齐 C++ release 0x4164BD：gravity 在 y 更新后、新 y<0 时才加入 vy
        protected float _gravityToAdd;

        // ========== 武器数据 ==========
        public int WeaponDropHurt
        {
            get => Runtime.WeaponDropHurt > 0 ? Runtime.WeaponDropHurt : 10;
            set => Runtime.WeaponDropHurt = value;
        }

        // weapon_strength_list（由 CharacterAnimtorManager 在加载时注入）
        protected List<WeaponStrengthEntry> _weaponStrengthList;
        public string WeaponDropSound { get; set; } = "";
        public string WeaponBrokenSound { get; set; } = "";
        public string WeaponHitSound { get; set; } = "";

        // ========== 公开属性 ==========
        public LF2LivingObject HoldObj => _holdObj;

        public abstract bool IsLight { get; }
        public abstract bool IsHeavy { get; }
        // C++ release [weapon+368h+6F8h]：0=普通轻武器, 1=重武器, 2=轻特殊, 4=特殊重武器, 6=饮料类
        public abstract int WeaponType { get; }
        public override int ReleaseEntityType => WeaponType;
        // C++ release weapon_count：笛子命中累积器，子类实现存储。
        public virtual int FluteWeight { get => 0; set { } }
        // ========== 初始化方法 ==========

        #region 生命周期（Init → Reset → Destroy → 初始化子步骤）

        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            AllocateStableId();

            // 初始化基类字段
            PS = new PhysicsState();
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);
            Trans = new FrameTransistor(this);
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();

            if (!(taskBase is OPointCreateTask task))
            {
                Log.Error($"[{GetType().Name}] Invalid task type");
                return;
            }

            InitializeParent(task);
            InitializePosition(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializeVelocity(task);
            InitializeHealth();

            // opoint kind=2：生成后立即由父角色持有。
            if (task.opoint.kind == 2 && task.parent != null)
            {
                Pick(task.parent as LF2LivingObject);
                // C++ release：opoint kind=2 生成武器时，角色和被持有对象双向绑定。
                (task.parent as LF2Character)?.AttachOpointHeldObject(this);
            }

            Renderer = renderer;
            SimulationTickDriver.Instance?.World?.Register(this);
        }

        public override void Reset()
        {
            FrameCache.Clear();
            Runtime.Reset();
            ObjectId = 0;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
            _holdObj = null;
            ShotCount = 0;
            PickerStableId = -1;
            Runtime.HolderStableId = -1;
            ResetSpark();
            ResetStableId();
        }

        public override void Destroy()
        {
            Generic_Die();
        }

        // ========== 初始化子步骤 ==========

        protected void InitializeParent(OPointCreateTask task)
        {
            ObjectId = task.opoint.oid;
            Team = task.team;
            Runtime.OwnerStableId = task.parent?.StableId ?? -1;
        }

        protected void InitializePosition(OPointCreateTask task)
        {
            PS.z = task.z;
            // parent==null 时（如测试直接生成），用 task.pos 作为初始世界坐标
            // 正常 opoint 路径 parent!=null，x/y 由 Act() 的 CoincideXY 对齐
            if (task.parent == null)
            {
                PS.x = task.pos.x;
                PS.y = task.pos.y;
            }
        }

        protected void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(dir);
        }

        protected void InitializeFrame(OPointCreateTask task)
        {
            int action = (task.opoint.action == 0 && !task.preserveActionZero) ? 999 : task.opoint.action;
            // 加载帧数据
            var wrapper = CharacterAnimtorManager.Instance.GetCharacterConfig(ObjectId);
            FrameCache.Load(wrapper);
            Frame.D = FrameCache.GetFrameDataById(action);
            SetFrameDirect(action);
        }

        protected void InitializeVelocity(OPointCreateTask task)
        {
            if (task.useDirectVelocity)
            {
                PS.vx = task.directVx;
                PS.vy = task.directVy;
                PS.vz = task.directVz;
            }
        }

        protected void InitializeHealth()
        {
            // 从 DAT 数据读取 weapon_hp / weapon_drop_hurt（C++ release ParseCharData 0x0040D8F0）
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            if (charData != null && charData.weapon_hp > 0)
            {
                Health.HP = charData.weapon_hp;
                WeaponDropHurt = charData.weapon_drop_hurt > 0 ? charData.weapon_drop_hurt : WeaponDropHurt;
            }
            else
            {
                Health.HP = 100;
            }
            // C++ release Entity_Spawn 0x402A74：[entity+31Ch] = charData[+90h] = weapon_hp
            OnHealthInitialized(charData);
        }

        /// <summary>
        /// InitializeHealth 完成后回调，供子类初始化 WeaponFlightCounter 等依赖 weapon_hp 的字段。
        /// </summary>
        protected virtual void OnHealthInitialized(LF2CharacterData charData) { }

        #endregion

        #region 每帧驱动接口（SimTransit → SimTU → IsWeaponDestroyable → GetFlightCounter）

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null) return;

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
                StateExitEvent();

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                AttackingCounter = 0;
                StateEntryEvent();
                _lastState = Frame.D.state;
            }

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            if (!string.IsNullOrEmpty(Frame.D.sound))
                PlaySound(Frame.D.sound);
        }

        public override void SimTransit(int tickIndex)
        {
            Trans?.Trans();
        }

        /// <summary>
        /// PreInteraction 阶段 - C++ release 对齐 sub_419F80 武器 wpoint→itr 拾取检测
        /// 把当前帧的 wpoint（kind=1/2/7）当作临时 itr，检测周围角色 bdy，触发拾取。
        /// </summary>
        public override void SimPreInteraction(int tickIndex)
        {
            if (HoldObj != null) return;
            if (!ItrArestTest()) return;

            var fD = Frame?.D;
            if (fD == null) return;
            if (fD.wpoints == null || fD.wpoints.Count == 0) return;

            var sceneQuery = Match?.SceneQuery;
            if (sceneQuery == null) return;

            float spriteW = GetSpriteWidthPxForCollision();
            if (spriteW <= 0f) return;

            bool facingLeft = PS.dir == "left";

            foreach (var wp in fD.wpoints)
            {
                if (wp == null) continue;
                if (wp.kind != 1 && wp.kind != 2 && wp.kind != 7) continue;
                if (wp.w <= 0 || wp.h <= 0) continue;

                float localX = facingLeft ? (spriteW - wp.x - wp.w) : wp.x;
                var vol = new PhysicsState.BattleVolume(
                    PS.sx, PS.sy, PS.sz,
                    localX, wp.y,
                    wp.w, wp.h,
                    NTSDGlobal.Default.Itr.ZWidth
                );

                var candidates = sceneQuery.QueryBodies(vol, this);
                if (candidates == null || candidates.Count == 0) continue;

                var tmpItr = new InteractionArea
                {
                    kind = wp.kind,
                    x = wp.x,
                    y = wp.y,
                    w = wp.w,
                    h = wp.h,
                    injury = wp.injury,
                    fall = wp.fall,
                    vaction = wp.vaction,
                    arest = wp.arest,
                    vrest = wp.vrest,
                    effect = wp.effect,
                    kill = wp.kill,
                    bdefend = wp.bdefend,
                };

                for (int c = 0; c < candidates.Count; c++)
                {
                    var target = candidates[c];
                    if (target == null || target == this) continue;
                    if (target.Health != null && target.Health.HP <= 0) continue;
                    if (!target.ItrVrestTest(StableId)) continue;

                    bool picked = false;
                    if (wp.kind == 1 || wp.kind == 7)
                        picked = HandlePreInteractionKind1(tmpItr, target);
                    else if (wp.kind == 2)
                        picked = HandlePreInteractionKind2(tmpItr, target);

                    if (picked)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// EntityCollision 阶段 - C++ release 对齐 Entity_Collision (sub_4138F0)
        /// 武器专属路径：N-1~N-5 + ShakeTimer 趋零 + wait/next 推进 + Frame.PN 更新
        /// </summary>
        public override void SimEntityCollision(int tickIndex)
        {
            var fD = Frame?.D;
            if (fD == null) return;

            // 0x4138F9: FrameDelay != 0 && entity_type != 3 → return
            if (FrameDelay != 0 && WeaponType != 3) return;

            // 0x41391A: AttackExempt > 0 → dec
            if (AttackExempt > 0) AttackExempt--;

            // 0x41392B: GrabbedBy < 0 → return（被持有时跳过）
            if (GrabbedBy < 0) return;

            // 0x413937: cpoint.kind == 2 → return
            if (fD.cpoint != null && fD.cpoint.kind == 2) return;

            // N-1（0x41395F）: entity_type==3 && frame.hit_a > 0 → HP -= hit_a；HP<=0 → frame=hit_d
            if (WeaponType == 3 && fD.hit_a > 0 && Health != null)
            {
                Health.HP -= fD.hit_a;
                if (Health.HP <= 0)
                {
                    Health.HP = 0;
                    SetFrameDirect(fD.hit_d);
                    fD = Frame.D;
                    if (fD == null) return;
                }
            }

            // 0x4139A0: ShakeTimer 双向趋零
            if (ShakeTimer > 0) ShakeTimer--;
            else if (ShakeTimer < 0) ShakeTimer++;

            // 0x413A14: 帧号变化时重置 Entity::attacking；0x413A1A: 无条件 ++
            if (Frame.N != Frame.PN)
                AttackingCounter = 0;
            AttackingCounter++;

            // N-2（0x413A27）: entity_type>=0 && frame.state==0 && y<0 → frame=212
            if (WeaponType >= 0 && fD.state == 0 && PS.y < 0)
            {
                SetFrameDirect(212);
                fD = Frame.D;
                if (fD == null) return;
            }

            // N-3（0x413A55）: entity_type==2 && frame.state==2000 && y==0 && |vx|<0.1 → frame=20
            if (WeaponType == 2 && fD.state == 2000 && PS.y >= 0 && Mathf.Abs(PS.vx) < 0.1f)
            {
                SetFrameDirect(20);
                fD = Frame.D;
                if (fD == null) return;
            }

            // N-4（0x413AB7）: frame.state==14 && HP<=0 → 条件满足时 ShakeTimer=30, attacking=0
            if (fD.state == 14 && Health != null && Health.HP <= 0)
            {
                // 0x413AC8: OwnerEntityIndex < 0 && Team != 5 → 若 ShakeTimer<=0 → ShakeTimer=30
                if (OwnerEntityIndex < 0 && Team != 5)
                {
                    if (ShakeTimer <= 0)
                        ShakeTimer = 30;
                }
                AttackingCounter = 0;
            }

            // N-4 facing（0x413AFC）: frame.state==2000 → vx==0→facing=left; vx!=0→facing=right
            if (fD.state == 2000)
            {
                if (PS.vx == 0f) SwitchDir("left");
                else SwitchDir("right");
            }

            // wait/next 推进（0x413B2F）: Entity::attacking > wait → attacking=0, frame=next
            if (AttackingCounter > fD.wait)
            {
                AttackingCounter = 0;
                int nextFrame = fD.next;
                if (nextFrame != 0)
                {
                    // 0x413B65: next < 0 → flip facing, frame = -next
                    if (nextFrame < 0)
                    {
                        bool wasRight = PS.dir == "right";
                        SwitchDir(wasRight ? "left" : "right");
                        nextFrame = -nextFrame;
                    }
                    SetFrameDirect(nextFrame);
                    fD = Frame.D;
                    if (fD == null) return;
                }
            }

            // N-5（0x413B84）: frame.next==999 → y<0 && entity_type==0 → frame=212; else → frame=0
            if (fD.next == 999)
            {
                if (PS.y < 0 && WeaponType == 0)
                {
                    SetFrameDirect(212);
                }
                else
                {
                    SetFrameDirect(0);
                }
                fD = Frame.D;
                if (fD == null) return;
            }

            // 0x413BAC: frame < 0 || frame >= 400 → return
            if (Frame.N < 0 || Frame.N >= 400) return;

            // frame==212 after normal frame advance：C++ release 会同步背景速度。
            // Unity 战斗场景当前由关卡/相机系统维护背景速度，这里不直接写实体字段。

            // 0x413DEB: frame==0xCAh=202 → ShakeTimer=20
            if (Frame.N == 202)
                ShakeTimer = 20;

            // 0x413DFB: Frame.PN = Frame.N
            Frame.PN = Frame.N;
        }

        /// <summary>
        /// 直接设置帧编号（C++ release 对齐直接写 [esi+70h]），不触发 OnFrameTransit 回调。
        /// </summary>
        protected internal void SetFrameDirect(int frameId)
        {
            Frame.N = frameId;
            Frame.D = FrameCache.GetFrameDataById(frameId);
            AttackingCounter = 0;
            if (Frame.D != null && Trans != null)
            {
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            }
        }

        public override void SimTU(int tickIndex)
        {
            int currentState = GetState();

            if (currentState != _lastState)
            {
                StateEntryEvent();
                _lastState = currentState;
            }

            TUEvent();

            // C++ release Entity_AI_Update 0x004228B8-0x004228C6：
            // type=1/2/4/6 武器，WeaponFlightCounter < 0 → 武器消失。
            if (_holdObj == null && IsWeaponDestroyable() && GetFlightCounter() < 0)
            {
                DieEvent();
                return;
            }

            if (Health.HP <= 0)
            {
                DieEvent();
            }
        }

        /// <summary>C++ release 0x004228A0: type=1/2/4/6 才检查 flightCounter</summary>
        protected virtual bool IsWeaponDestroyable() => false;

        /// <summary>供基类 SimTU 读取 WeaponFlightCounter。</summary>
        protected virtual int GetFlightCounter() => 0;

        #endregion

        #region 帧事件回调（typed event dispatch → OnInFlightFrameUpdate → OnLanded → WeaponFlightPhysics → OnThrown）

        protected override bool StateEntryEvent() => DispatchCurrentStateEvent("state_entry");

        protected override bool FrameEvent() => DispatchCurrentStateEvent("frame");

        protected override bool TUEvent()
        {
            Generic_TU();
            return DispatchCurrentStateEvent("TU");
        }

        protected override bool DieEvent()
        {
            Generic_Die();
            return true;
        }

        protected virtual bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            return GetState() switch
            {
                LF2States.WeaponJustOnGround => State_WeaponJustOnGround(eventType, eventData),
                LF2States.WeaponOnGround => State_WeaponOnGround(eventType, eventData),
                _ => false,
            };
        }

        protected virtual void OnInFlightFrameUpdate() { }

        /// <summary>
        /// 飞行武器落地后的弹射与停止处理
        /// C++ release 对齐 Entity_FrameAdvance 0x4164A9-0x416577（y>=0 路径）
        /// 子类按 WeaponType 重写以实现差异化落地行为
        /// </summary>
        protected virtual void OnLanded()
        {
            // 基类不做任何清零——所有 type 分支由 LF2Weapon.OnLanded() 完整覆盖并 return。
        }

        /// <summary>
        /// 飞行武器每帧的特化物理（在 Dynamics 之前执行）
        /// C++ release 对齐 Entity_FrameAdvance 0x416240-0x416577（在空中时的 type 分流）
        /// 子类按 WeaponType 重写
        /// </summary>
        protected virtual void WeaponFlightPhysics() { }

        /// <summary>
        /// 投掷成功后的初始化回调（子类用于初始化 WeaponFlightCounter 等）。
        /// </summary>
        protected virtual void OnThrown() { }

        #endregion

        #region 通用状态处理（Generic_TU → CheckBoomerangCatch → Generic_Die）

        private void Generic_TU()
        {
            // ── C++ release Entity_FrameAdvance 0x416240 入口守卫 ──────────────────────
            // 1. FrameDelay [+0B4h] 递减/递增并无条件提前退出
            //    > 0 → dec → 无条件返回（0x416258-0x41627C: dec; 无论新值是否==0都retn）
            //    < 0 → inc → 无条件返回（0x41626F-0x41627C: inc; retn，不检查新值）
            //    == 0 → 继续执行（0x416256: jz loc_41627D → held check）
            if (FrameDelay > 0)
            {
                FrameDelay--;
                return;
            }
            else if (FrameDelay < 0)
            {
                FrameDelay++;
                return;
            }

            // 2. held_by [+98h]：被持有时 >= 0，跳过全部飞行物理（C++ release 0x41627D: jl loc_416D9E）
            if (_holdObj != null) return;

            // 3. cpoint.kind == 2 时跳过（C++ release 0x4162A7: cmp cpoint.kind,2; jz loc_416D9E）
            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2) return;
            // ────────────────────────────────────────────────────────────────────

            Interaction();

            int state = GetState();
            switch (state)
            {
                case LF2States.WeaponOnHand:
                case 2001:
                    break;
                default:
                    // 严格对齐 C++ release Entity_FrameAdvance 0x4162EB-0x416DA4 的执行顺序：
                    // 1. x += vx（边界）
                    // 2. type=4/typeSub=78: x += vx*0.2；typeSub=65: x -= vx*0.2
                    // 3. z += vz（边界）
                    // 4. 边界标志清零
                    // 5. type=3 hit_j: z += hit_j-50
                    // 6. 地面摩擦（y_int >= 0，即 y 更新前判断）← 关键：摩擦在 y+=vy 之前
                    // 7. type=4/6 + state=1000 + |vx|>9: frame=40（回旋镖）
                    // 8. y += vy（更新y位置）
                    // 9. if 新y < 0（空中）: 加重力  ← WeaponDynamics 内部执行
                    // 10. if 新y >= 0（落地）: 走落地 switch(type)
                    _gravityToAdd = 0f;
                    WeaponFlightPhysics();
                    CharacterMechanics.WeaponDynamics(PS, _gravityToAdd);

                    // C++ release 0x416577~0x4166CE：type=0 空中（新y<0）时的帧动态切换
                    if (PS.y < -0.0001f)
                        OnInFlightFrameUpdate();
                    break;
            }

            // C++ release 0x4164A9：新y >= -0.0001 且旧y < 0 表示本帧落地
            if (PS.y >= 0 && PS.vy > 0)
                OnLanded();

            // C++ release LABEL_182 末尾：if (frame.state != 12) this+800 = 0
            if ((Frame?.D?.state ?? -1) != LF2States.Falling)
                FluteWeight = 0;

            // C++ release 0x00405132：type=4 回旋镖飞行中，距投掷者够近时自动回收
            if (WeaponType == 4 && _holdObj == null && PickerStableId >= 0)
                CheckBoomerangCatch();
        }

        /// <summary>
        /// C++ release EXE 0x00405132：回旋镖（type=4）捕获检测。
        /// x：|dx| &lt; 30（对称）
        /// z：thrower.z - 80 &lt; weapon.z &lt; thrower.z（单向，武器必须在投掷者前方区间内）
        /// y：|dy| &lt; 10（对称）
        /// 满足条件：frame=60, vx/vy/vz=0
        /// </summary>

        private void CheckBoomerangCatch()
        {
            var world = SimulationTickDriver.Instance?.World;
            if (world == null) return;

            world.GetAllLivingObjects(_boomerangQueryCache);

            LF2LivingObject thrower = null;
            foreach (var obj in _boomerangQueryCache)
            {
                if (obj.StableId == PickerStableId) { thrower = obj; break; }
            }
            if (thrower == null || thrower.Health?.HP <= 0) return;

            float dx = Mathf.Abs(PS.x - thrower.PS.x);
            // C++ release 0x405187-0x405196：z 为单向检测
            // weapon.z <= thrower.z-80 OR weapon.z >= thrower.z → 跳过
            float dy = Mathf.Abs(PS.y - thrower.PS.y);
            if (dx >= 30f || PS.z <= thrower.PS.z - 80f || PS.z >= thrower.PS.z || dy >= 10f) return;

            PS.vx = 0f;
            PS.vy = 0f;
            PS.vz = 0f;
            SetFrameDirect(60);
            // C++ release 0x004051FC：捕获后设置 thrower.[+0E4h] = 100（HP 恢复计时器）
            thrower.HealTimer = 100;
        }

        /// <summary>
        /// 飞行武器在空中（新y&lt;0）时的帧动态更新。
        /// C++ release 对齐 Entity_FrameAdvance 0x416577-0x4166CE（type==0 的 Falling/Burning 帧切换）。
        /// 子类按 WeaponType 重写。
        /// </summary>

        private void Generic_Die()
        {
            // C++ release 中武器 HP 耗尽后由 GameMode 层生成 broken_weapon 碎片并回收武器实体。
            // Unity 侧在这里播放破碎音效并请求战斗世界生成碎片。
            PlaySound(WeaponBrokenSound);
            CreateBrokenEffect();
        }

        #endregion

        #region 具体状态处理（JustOnGround / OnGround 虚方法）

        protected virtual bool State_WeaponJustOnGround(string eventType, object eventData)
        {
            return false;
        }

        protected virtual bool State_WeaponOnGround(string eventType, object eventData)
        {
            return false;
        }

        #endregion

        #region 交互系统（Interaction → CanInteractTarget → DispatchInteractionByKind → Kind3Stick → Kind8Attach → TryApplyHit → ApplyPickupGrabbedBy → ApplyPickupFrameJump → Kind1 → Kind2 → Kind3 → Kind7）

        public virtual void Interaction()
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

            // C++ release sub_419F80：外层遍历所有 itr，命中后 goto LABEL_184（继续下一个 itr）
            // 不是命中即 return，每个 itr 都独立检查所有候选目标
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
                    break; // 每个 itr 只命中一个目标（C++ release内层 bdy 循环命中即跳到下一个 itr）
                }
            }
        }

        protected virtual bool CanInteractTarget(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (Team != 0 && target.Team != 0 && Team == target.Team) return false;
            if (!target.ItrVrestTest(StableId)) return false;
            if (target is not LF2LivingObject) return false;

            // C++ release 0x41A0C9-0x41A20B：itr.attacking 目标过滤
            int targetState = target.GetState();
            int targetFrame = target.Frame?.N ?? -1;
            // EXE sub_419F80 0x0041A6A4：itr.kind=5 且 attacking!=0 → 拾取路径，跳过伤害
            if (itr.kind == 5 && itr.attacking != 0) return false;
            switch (itr.attacking)
            {
                case 4:
                    if (target is not LF2Character) return false;
                    break;
                case 20:
                    if (target is not LF2Character) return false;
                    if (targetState == 18 || targetState == 19) return false;
                    break;
                case 21:
                    if (targetState == 18 || targetState == 19) return false;
                    break;
                case 30:
                    if (targetFrame == 200 || targetFrame == 201 || targetFrame == 202) return false;
                    break;
            }

            return true;
        }

        protected virtual bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr.kind == 8)
            {
                if (target is not LF2Character) return false;
                if (DeferState3005Kind8LeadIn()) return false;
                return TryApplyHit(itr, target);
            }

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
                    return HandleWeaponKind3Stick(itr, target);
                case 7:
                    return HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        private bool DeferState3005Kind8LeadIn()
        {
            var activeFrame = Frame?.D;
            if (activeFrame == null || activeFrame.state != LF2States.ObjectFlying)
            {
                return false;
            }

            // C++ release defer_state3005_kind8_lead_in：
            // state=3005 且当前/下一帧带 hit_Fa 或 opoint 时，延后 kind=8 命中。
            if (activeFrame.hit_Fa > 0 || (activeFrame.opoints != null && activeFrame.opoints.Count > 0))
            {
                return true;
            }

            if (activeFrame.next <= 0 || activeFrame.next == Frame.N)
            {
                return false;
            }

            var nextFrame = GetFrameDataById(activeFrame.next);
            return nextFrame != null
                && (nextFrame.hit_Fa > 0 || (nextFrame.opoints != null && nextFrame.opoints.Count > 0));
        }

        protected virtual bool HandleWeaponKind3Stick(InteractionArea itr, LF2Entity target)
        {
            if (target is LF2WeaponBase) return false;
            if (!ItrArestTest()) return false;

            int catchingFrame = itr.catchingact != null && itr.catchingact.Length > 0 ? itr.catchingact[0] : 0;
            int caughtFrame   = itr.caughtact   != null && itr.caughtact.Length   > 0 ? itr.caughtact[0]   : 0;
            if (catchingFrame <= 0 && caughtFrame <= 0)
                return HandlePreInteractionKind3(itr, target); // 无粘附帧 → 普通攻击

            if (catchingFrame > 0) SetFrameDirect(catchingFrame);
            if (caughtFrame > 0 && target is LF2Character ch)
            {
                ch.ImmediateFrame(caughtFrame);
            }
            return true;
        }

        protected virtual bool TryApplyHit(InteractionArea itr, LF2Entity target)
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

        private void ApplyPickupGrabbedBy(LF2Character character)
        {
            int pickerLink;
            // C++ release 0x0042E9F0-0x0042E9FC：type_sub=0x78 或 0x7C → grabbed_by=101（优先检查）
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            int typeSub = charData?.type_sub ?? 0;
            if (typeSub == 0x78 || typeSub == 0x7C)
                pickerLink = 101;
            else if (IsHeavy)           // 重武器拾取标记
                pickerLink = 2;
            else if (WeaponType == 4)
                pickerLink = 4;
            else if (WeaponType == 6)
                pickerLink = Health.HP > 0 ? 6 : 4;
            else
                pickerLink = 0;    // 轻武器

            character.GrabbedBy = pickerLink;
            GrabbedBy = -pickerLink;
        }

        private void ApplyPickupFrameJump(LF2Character character)
        {
            int jumpFrame = IsHeavy ? 116 : 115;
            if (character.GetFrameDataById(jumpFrame) != null)
            {
                character.ImmediateFrame(jumpFrame);
            }
        }

        protected virtual bool HandlePreInteractionKind1(InteractionArea itr, LF2Entity target)
        {
            if (HoldObj != null)
            {
                return false;
            }
            if (!ItrArestTest())
            {
                return false;
            }
            if (Renderer == null)
            {
                return false;
            }
            if (target is not LF2Character character)
            {
                return false;
            }
            if (character.GetHeldWeapon() != null)
            {
                return false;
            }

            // 只有地面武器才能被拾取（C++ release 0x00407378：仅检查 state=1004 和 2004）
            int wstate = GetState();
            bool isOnGround = wstate == LF2States.WeaponOnGround
                           || wstate == LF2States.HeavyWeaponOnGround;
            if (!isOnGround)
            {
                return false;
            }

            bool pickOk = Pick(character);
            if (!pickOk)
            {
                return false;
            }
            character.HoldWeapon(this);
            ApplyPickupGrabbedBy(character);
            ItrArestUpdate(itr);
            target.ItrVrestUpdate(StableId, itr);
            return true;
        }

        protected virtual bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            if (HoldObj != null)
            {
                return false;
            }
            if (!ItrArestTest())
            {
                return false;
            }
            if (Renderer == null)
            {
                return false;
            }
            if (target is not LF2Character character)
            {
                return false;
            }
            if (character.GetHeldWeapon() != null)
            {
                return false;
            }

            // 只有地面武器才能被拾取（C++ release 0x00407378：仅检查 state=1004 和 2004）
            int wstate = GetState();
            bool isOnGround = wstate == LF2States.WeaponOnGround
                           || wstate == LF2States.HeavyWeaponOnGround;
            if (!isOnGround)
            {
                return false;
            }

            bool pickOk = Pick(character);
            if (!pickOk)
            {
                return false;
            }
            character.HoldWeapon(this);
            ApplyPickupGrabbedBy(character);
            // C++ release 0x42EA9C/0x42EC29：kind=2 拾取后跳转 frame=115/116
            ApplyPickupFrameJump(character);
            ItrArestUpdate(itr);
            target.ItrVrestUpdate(StableId, itr);
            return true;
        }

        protected virtual bool HandlePreInteractionKind3(InteractionArea itr, LF2Entity target)
        {
            // C++ release sub_419F80：kind=3 时若 target.charData.type != 0（即目标是武器）则跳过
            // 否则走普通命中路径，与 kind=0 相同
            if (target is LF2WeaponBase) return false;
            return TryApplyHit(itr, target);
        }

        protected virtual bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            // C++ release 0x42E97B/0x42E984：kind=7 近身拾取，与 kind=1 相同但无帧跳转
            return HandlePreInteractionKind1(itr, target);
        }

        public override float GetSpriteWidthPxForCollision()
        {
            var wrapper = FrameCache?.Wrapper;
            var files = wrapper?.characterData?.files;
            if (files == null || files.Count == 0)
                return 0f;

            return files[0].width + 1;
        }

        #endregion

        #region 战斗（Hit → Act → ForceClearHolder → Drop → Pick → ProcessDrinkConsumption → OnDrinkConsumed → ProcessAttack → SetWeaponStrengthList → GetStrengthEntry）

        /// <summary>
        /// C++ release 语义下的武器受击入口。
        /// </summary>
        public abstract bool Hit(InteractionArea itr, LF2Entity attacker);

        /// <summary>
        /// C++ release 语义下的持有武器动作入口。
        /// </summary>
        public virtual WeaponActResult Act(LF2LivingObject holder, WeaponPoint wpoint, Vector3 holdpoint)
        {
            var result = new WeaponActResult();
            if (Frame.D == null) return result;

            // C++ release AI_Process2 0x0041AFFC：
            // 持有者处于 Falling(12) 或 BeingCaught(10) 时强制脱落武器
            // → 双方 arest=0，武器随机帧[0,15]，速度继承持有者速度 * 1/3
            int holderState = holder?.GetState() ?? -1;
            if (holderState == LF2States.Falling || holderState == LF2States.BeingCaught)
            {
                ItrRest.Arest = 0;
                holder.ItrRest.Arest = 0;

                ImmediateFrame(RandInt(0, 16));

                // C++ release 0x41B035-0x41B075：按 holder.hit_count 选速度来源。
                // hit_count==1：vx = holder.KnockbackVx * 1/3，vy = holder.KnockbackVy。
                // hit_count!=1：vx = holder.vx * 1/3，vy = holder.vy。
                // vz 不设置（C++ release无 vz 赋值）
                const float kVelFactor = 1f / 3f;
                if (holder.HitCount == 1)
                {
                    PS.vx = holder.KnockbackVx * kVelFactor;
                    PS.vy = holder.KnockbackVy;
                }
                else
                {
                    PS.vx = holder.PS.vx * kVelFactor;
                    PS.vy = holder.PS.vy;
                }

                // C++ release 0x41B07A-0x41B08D：if weapon.y_float > -2.0 → y_float = -2.0
                // 确保武器脱落时至少在地面以上2单位
                if (PS.y > -2.0f) PS.y = -2.0f;

                // C++ release 0x41B011：character.grabbed_by=0, weapon.grabbed_by=0
                GrabbedBy = 0;
                if (holder is LF2Character ch2) ch2.GrabbedBy = 0;

                _holdObj = null;
                (holder as LF2Character)?.HoldWeapon(null);
                Runtime.LinkState = 0;
                Runtime.HolderStableId = -1;
                result.ForceDrop = true;
                return result;
            }

            // C++ release 0x41AEAD：weapon.[+0B4h] = holder.[+0B4h]
            // [+0B4h] = FrameDelay（帧延迟计数器），武器与持有者同步
            FrameDelay = holder.FrameDelay;

            // 切换到武器动作帧
            // C++ release 0x41AE98：直接写 weapon.frame = holder_wpoint.action，不触发帧事件
            if (wpoint.weaponact > 0)
            {
                ImmediateFrame(wpoint.weaponact);
            }

            var fD = Frame.D;
            if (fD?.wpoints == null || fD.wpoints.Count == 0) return result;

            var fwpoint = fD.wpoints[0];

            // C++ release AI_Process2 0x41ABF2：触发条件是 holder 当前帧的 frame.state == 17（0x11）
            // [ecx+edx*8+7ACh] = charData.frames[frame].state，7ACh-7A4h=8=state偏移
            // 不是 wpoint.kind，是 holder 帧的 state 字段
            if (holder?.Frame?.D?.state == 17)
            {
                ProcessDrinkConsumption(holder, result);
                return result;
            }

            if (wpoint.dvx != 0)
            {
                // C++ release AI_Process2 0x41B094~0x41B21D：
                // 按 weapon.type 分流投掷路径：
                //   type=1/4/6 → heavy throw：frame固定40，双方arest归零
                //   type=2     → light throw：Random_Int(6)帧，双方arest归零
                //   type=0     → 无投掷路径，dvx有值时仍走kind=3（ProcessForceDropPoint）
                int wt = WeaponType;
                bool isHeavyThrow = wt == 1 || wt == 4 || wt == 6;
                bool isLightThrow = wt == 2;

                if (isHeavyThrow)
                {
                    // C++ release 0x41B0C2-0x41B155：frame=40，vx按facing，vy=dvy
                    // dvz 由 holder.key_up/key_down 控制（0x41B114-0x41B155）：
                    //   key_up!=0 && key_down==0 → vz = -dvz
                    //   key_up==0 && key_down!=0 → vz = +dvz
                    //   其他 → vz 不变
                    ImmediateFrame(40);
                    PS.vx = Dirh() * wpoint.dvx;
                    // C++ release 0x41B0F7: fild [edx+1Ch] -> weapon.vy = dvy（无条件赋值，无零值守卫）
                    PS.vy = wpoint.dvy;
                    if (wpoint.dvz != 0)
                    {
                        bool keyUp   = holder.Controller?.IsUp   ?? false;
                        bool keyDown = holder.Controller?.IsDown  ?? false;
                        if (keyUp && !keyDown)       PS.vz = -wpoint.dvz;
                        else if (!keyUp && keyDown)  PS.vz =  wpoint.dvz;
                    }
                    ItrRest.Arest = 0;
                    holder.ItrRest.Arest = 0;
                    PS.zz = 1;
                    _holdObj = null;
                    (holder as LF2Character)?.HoldWeapon(null);
                    GrabbedBy = 0;
                    Runtime.LinkState = 0;
                    Runtime.HolderStableId = -1;
                    PickerStableId = holder?.StableId ?? -1;
                    OnThrown();
                    result.Thrown = true;
                }
                else if (isLightThrow)
                {
                    // C++ release 0x41B173-0x41B219：Random(6)帧，vx按facing，vy=dvy
                    // dvz 控制逻辑同上（0x41B1DB-0x41B216）
                    ImmediateFrame(RandInt(0, 6));
                    PS.vx = Dirh() * wpoint.dvx;
                    // C++ release 0x41B1B7: fild [edx+1Ch] -> weapon.vy = dvy（无条件赋值，无零值守卫）
                    PS.vy = wpoint.dvy;
                    if (wpoint.dvz != 0)
                    {
                        bool keyUp   = holder.Controller?.IsUp   ?? false;
                        bool keyDown = holder.Controller?.IsDown  ?? false;
                        if (keyUp && !keyDown)       PS.vz = -wpoint.dvz;
                        else if (!keyUp && keyDown)  PS.vz =  wpoint.dvz;
                    }
                    ItrRest.Arest = 0;
                    holder.ItrRest.Arest = 0;
                    PS.zz = 1;
                    _holdObj = null;
                    (holder as LF2Character)?.HoldWeapon(null);
                    GrabbedBy = 0;
                    Runtime.LinkState = 0;
                    Runtime.HolderStableId = -1;
                    PickerStableId = holder?.StableId ?? -1;
                    OnThrown();
                    result.Thrown = true;
                }
                // type=0：dvx非零也不投掷，转kind=3强制丢弃（C++ release 0x41B155→0x41B16D→0x41B21D）
                else
                {
                    result.NeedsKind3Drop = true;
                    return result;
                }
            }

            if (!result.Thrown)
            {
                // 继续被持有
                int cover = wpoint.cover != 0 ? wpoint.cover : NTSDGlobal.Default.WPoint.Cover;
                PS.zz = (cover == 1) ? -1 : 1;

                SwitchDir(holder.PS.dir);
                PS.sz = PS.z = holder.PS.z;

                // 按 wpoint 对齐武器位置。
                CoincideXYWithWPoint(holdpoint, fwpoint);

                // C++ release AI_Process2 0x41AFA7-0x41AFCC：
                // cover==0 → z+=1, y-=1（武器在角色前面，稍偏前/上）
                // cover!=0 → z-=1, y+=1（武器在角色后面，稍偏后/下）
                if (cover == 0) { PS.z += 1f; PS.y -= 1f; }
                else            { PS.z -= 1f; PS.y += 1f; }
            }

            // C++ release GameMode_Process 0x0041BDDF：武器 state==1001（持有中）且持有者 wpoint.attacking>0 才攻击
            // attacking 读自持有者角色的 wpoint（本 wpoint 参数），不受 fwpoint.kind 约束
            if (GetState() == LF2States.WeaponOnHand && IsLight && wpoint.attacking > 0)
            {
                result.AttackResult = ProcessAttack(holder, wpoint, fD);
            }

            return result;
        }

        /// <summary>
        /// 强制清除持有关系（不触发 Drop() 的帧切换）。
        /// 供 kind=3 强制丢弃时使用，避免 Drop() 覆盖已设好的随机帧。
        /// </summary>
        public void ForceClearHolder()
        {
            _holdObj = null;
            GrabbedBy = 0;
            Runtime.LinkState = 0;
            Runtime.HolderStableId = -1;
        }

        /// <summary>
        /// C++ release AI_Process2 0x41B011-0x41B08D：角色被打（state=12/10）时武器强制脱落。
        ///   weapon.frame = Random(16)
        ///   vx = holder.vx * 1/3（dvx 传入 holder.vx）
        ///   vy = holder.vy（dvy 传入 holder.vy，直接复制不乘系数）
        ///   if weapon.y > -2.0 → weapon.y = -2.0
        /// </summary>
        public virtual void Drop(float dvx, float dvy)
        {
            Team = 0;
            _holdObj = null;
            Runtime.HolderStableId = -1;
            Runtime.LinkState = 0;
            GrabbedBy = 0;

            // C++ release 0x41B019: weapon.frame = Random(16)
            ImmediateFrame(RandInt(0, 16));

            // C++ release 0x41B035-0x41B075: vx = holder.vx * 1/3, vy = holder.vy（直接复制）
            PS.vx = dvx * (1f / 3f);
            PS.vy = dvy;

            // C++ release 0x41B07A: if weapon.y > -2.0 → weapon.y = -2.0
            if (PS.y > -2.0f) PS.y = -2.0f;

            PS.zz = 0;
        }

        /// <summary>
        /// C++ release 语义下的武器拾取入口。
        /// </summary>
        public virtual bool Pick(LF2LivingObject holder)
        {
            if (_holdObj != null) return false;

            _holdObj = holder;
            Runtime.HolderStableId = holder?.StableId ?? -1;
            Team = holder.Team;

            return true;
        }


        /// <summary>
        /// 饮料消耗处理（C++ release AI_Process2 0x41ABF2-0x41AE73）。
        /// 触发条件：holder 帧的 wpoint.kind == 17（0x11）。
        ///
        /// 逻辑：
        ///   type_sub == 0x7A（饮料）：weapon.hp-=1；每6帧给 holder.PP += 5（上限 max_pp）
        ///   type_sub == 0x7B（食物）：每帧 weapon.hp -= 2，holder.PP += 3（已有不同上限逻辑）
        ///   weapon.hp <= 0：双方 arest=0，脱落武器（随机帧/速度，同 kind=3 drop）
        /// </summary>
        protected virtual void ProcessDrinkConsumption(LF2LivingObject holder, WeaponActResult result)
        {
            if (holder?.Health == null) return;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            int typeSub = charData?.type_sub ?? 0;

            // C++ release 0x41AC21：type_sub == 0x7A → 饮料（liquid）路径
            if (typeSub == 0x7A)
            {
                // 0x41AC2E: weapon.hp [+2FCh] > 0 才执行
                if (Health.HP <= 0) return;

                // 0x41AC3C: weapon.hp-- (每帧 -1)
                Health.HP--;

                // 0x41AC45-0x41AC55: weapon.hp % 5 == 0 → 执行 MaxPP 分支
                // [holder+300h] += 2（MaxPP += 2），然后 clamp 到 weapon.hp（当前值）
                if (Health.HP % 5 == 0)
                {
                    int newMaxPP = holder.Health.MaxPP + 2;
                    // 0x41AC74-0x41AC98: weapon.hp += 4，再 clamp MaxPP <= weapon.hp
                    // 注意：weapon.hp 先 +4（恢复一部分？），再 clamp MaxPP
                    // 实际上 [eax+2FCh] += 4 后作为 cap 使用——即 HP 在此路径被补4
                    Health.HP += 4;
                    int cap = Health.HP;
                    holder.Health.MaxPP = Mathf.Min(newMaxPP, cap);
                }

                // 0x41ACBD-0x41ACCF: weapon.hp（更新后）% 6 == 0 → PP += 5，上限 500
                if (Health.HP % 6 == 0)
                {
                    int newPP = holder.Health.PP + 5;
                    holder.Health.PP = Mathf.Min(newPP, NTSDGlobal.Gameplay.DrinkPPCap); // 500 = 0x1F4
                }
            }
            else if (typeSub == 0x7B) // 食物（food）路径
            {
                // 0x41AD96: weapon.hp > 0 才执行
                if (Health.HP <= 0) return;

                // 0x41ADA4: weapon.hp -= 2（add 0xFFFFFFFE = -2）
                Health.HP -= 2;

                // 0x41ADBA: holder.[+308h]（PP）+= 3
                int newPP = holder.Health.PP + 3;
                // 0x41ADCA: clamp PP <= 500
                newPP = Mathf.Min(newPP, NTSDGlobal.Gameplay.DrinkPPCap); // 0x1F4

                // C++ release 还有额外 PP 上限分支，当前战斗复刻先保留通用 500 上限。
                holder.Health.PP = newPP;
            }
            else
            {
                return;
            }

            // 0x41AD10 / 0x41AE07: weapon.hp <= 0 → 消耗完毕，脱落武器
            if (Health.HP > 0) return;

            // 0x41AD1B: holder.grabbed_by [+98h] = 0  ← 先清 holder
            // 0x41AD23: weapon.grabbed_by [+98h] = 0  ← 再清 weapon
            // 0x41AD30: holder.slot_idx [+9Ch] = 0  → HoldWeapon(null) 处理
            // 0x41AD38: weapon.holder_idx [+0A0h] = 0 → _holdObj=null 处理
            // 0x41AD40: weapon.frame = 0
            // 0x41AD45: weapon.vx = 0（[+40h]）
            // 0x41AD48: weapon.vy = -8.0（double: low=0, high=0xC0200000 → -8.0）
            // 0x41AD4F: call Random_Int(7); vx = Random(7) - 3
            // 0x41AD6E: holder.frame = 0
            // 0x41AD73: weapon.[+31Ch]（WeaponFlightCounter）= 0。
            if (holder is LF2Character holderChar) holderChar.GrabbedBy = 0;
            GrabbedBy = 0;
            ImmediateFrame(0);
            PS.vx = RandInt(0, 7) - 3f;
            PS.vy = -8.0f;  // double 0xC020000000000000 = -8.0
            PS.vz = 0f;
            PS.zz = 0;
            Team = 0;
            holder.ImmediateFrame(0);
            OnDrinkConsumed();
            _holdObj = null;
            (holder as LF2Character)?.HoldWeapon(null);
            Runtime.LinkState = 0;
            Runtime.HolderStableId = -1;
            result.ForceDrop = true;
        }

        protected override void RefreshRuntimeFromEntity()
        {
            base.RefreshRuntimeFromEntity();
            Runtime.HolderStableId = _holdObj?.StableId ?? -1;
            Runtime.PickerStableId = PickerStableId;
            Runtime.WeaponState = GetState();
            Runtime.WeaponDropHurt = WeaponDropHurt;
        }

        /// <summary>
        /// 饮料消耗完毕后的子类钩子，用于重置 WeaponFlightCounter 等字段。
        /// C++ release 0x41AD73: weapon.[+31Ch] = 0
        /// </summary>
        protected virtual void OnDrinkConsumed() { }

        protected virtual WeaponAttackResult ProcessAttack(LF2LivingObject holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            // 基类不处理具体攻击；正式武器命中逻辑由 LF2Weapon 覆盖实现。
            return new WeaponAttackResult();
        }

        public void SetWeaponStrengthList(List<WeaponStrengthEntry> list)
        {
            _weaponStrengthList = list;
        }

        protected WeaponStrengthEntry GetStrengthEntry(int attackingIndex)
        {
            if (_weaponStrengthList == null || attackingIndex <= 0) return null;
            return _weaponStrengthList.Find(e => e.index == attackingIndex);
        }

        #endregion

        #region 辅助方法（WhirlwindForce → FluteForce → CoincideXYWithWPoint → PlaySound → CreateBrokenEffect → MakePointCenter → CoincideXYForInit）

        public void WhirlwindForce(InteractionArea itr, LF2Entity attacker)
        {
            if (attacker?.PS == null || PS == null) return;

            int state = GetState();
            bool lightLike = WeaponType == 1 || WeaponType == 4 || WeaponType == 6;
            bool heavyLike = WeaponType == 2;

            if (lightLike)
            {
                if (ObjectId == 201 || ObjectId == 202) return;
                if (state != LF2States.WeaponInSky)
                    SetFrameDirect(0);
                ApplyWhirlwindVelocity(attacker, 3f);
            }
            else if (heavyLike)
            {
                if (state != LF2States.HeavyWeaponInSky)
                    SetFrameDirect(0);
                ApplyWhirlwindVelocity(attacker, 2.3f);
            }
        }

        private void ApplyWhirlwindVelocity(LF2Entity attacker, float vyDelta)
        {
            KnockbackVx = PS.vx + ((PS.x > attacker.PS.x) ? -1f : 1f);
            PS.vx = KnockbackVx;

            KnockbackVz = PS.vz + ((PS.z > attacker.PS.z) ? -0.5f : 0.5f);
            PS.vz = KnockbackVz;

            if (PS.y >= -2f)
            {
                PS.y = -2f;
                PS.vy = -6f;
            }

            if (PS.vy > -6f)
            {
                PS.vy -= vyDelta;
                KnockbackVy = PS.vy;
            }
        }

        public override void FluteForce()
        {
            // C++ release Entity_AI_Update line 1535：kind=10/11 命中时 this+800 = -20
        }

        /// <summary>
        /// 将武器与持有者的 wpoint 对齐。
        /// C++ release AI_Process2 0x41AEDF-0x41AF8F：
        ///   dir=right: weapon.x = holdpoint.x + weapon_frame.centerx - weapon_wpoint.x
        ///   dir=left:  weapon.x = holdpoint.x + weapon_wpoint.x - weapon_frame.centerx
        ///   weapon.y   = holdpoint.y + weapon_frame.centery - weapon_wpoint.y
        /// </summary>
        protected void CoincideXYWithWPoint(Vector3 holdpoint, WeaponPoint wpoint)
        {
            var weapFD = Frame?.D;
            int wcx = weapFD?.centerx ?? 0;
            int wcy = weapFD?.centery ?? 0;
            int wpx = wpoint?.x ?? 0;
            int wpy = wpoint?.y ?? 0;

            if (PS.dir == "right")
                PS.x = holdpoint.x + wcx - wpx;
            else
                PS.x = holdpoint.x + wpx - wcx;

            PS.y = holdpoint.y + wcy - wpy;
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }

        public void CreateBrokenEffect()
        {
            SpawnBrokenWeaponFragments(ObjectId);
        }

        protected Vector3 MakePointCenter(LF2FrameData frame)
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

        protected void CoincideXYForInit(Vector3 targetPos, Vector3 selfPoint)
        {
            float vx = targetPos.x - selfPoint.x;
            float vz = targetPos.z - selfPoint.z;
            PS.x += vx;
            PS.z += vz;
        }

        #endregion

        #region 回旋镖查询缓存

        private List<LF2LivingObject> _boomerangQueryCache = new List<LF2LivingObject>(8);

        #endregion

    }

    // ========== 结果类 ==========

    public class WeaponActResult
    {
        public bool Thrown;
        public bool ForceDrop;       // C++ release AI_Process2 0x41AFFC：持有者 Falling/BeingCaught 时强制脱落
        public bool NeedsKind3Drop;  // C++ release 0x41B155~0x41B16D：type=0武器dvx≠0时转kind=3强制丢弃
        public WeaponAttackResult AttackResult;
    }

    public class WeaponAttackResult
    {
        public int VRest;
        public int ARest;
        public int HitUid;
    }
}

