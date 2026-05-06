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
    /// 武器抽象基类
    /// 严格对齐 FLF weapon.js typeweapon
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\weapon.js
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

        /// <summary>HP 恢复计时器（回旋镖捕获等）</summary>
        public override int HealTimer { get; set; } = 0;
        // ========== 配置字段 ==========
        protected int _objectId;
        protected int _lastState = -1;

        // ========== 持有者信息 ==========
        protected LF2LivingObject _holdObj;
        protected LF2LivingObject _holdPre;

        // 反汇编 [entity+3F8h]：投掷者 StableId，投掷后保留（不随投掷清零），用于回旋镖捕获检测
        public int PickerStableId { get; set; } = -1;

        // 本帧重力累加量，由 WeaponFlightPhysics 计算，WeaponDynamics 在 y+=vy 后使用
        // 对齐反汇编 0x4164BD：gravity 在 y 更新后、新 y<0 时才加入 vy
        protected float _gravityToAdd;

        // ========== VRest 系统 ==========
        protected Dictionary<int, int> _vrest = new Dictionary<int, int>();

        // ========== 武器数据 ==========
        public int WeaponDropHurt { get; set; } = 10;

        // weapon_strength_list（由 CharacterAnimtorManager 在加载时注入）
        protected List<WeaponStrengthEntry> _weaponStrengthList;
        public string WeaponDropSound { get; set; } = "";
        public string WeaponBrokenSound { get; set; } = "";
        public string WeaponHitSound { get; set; } = "";

        // ========== 公开属性 ==========
        public LF2LivingObject HoldObj => _holdObj;
        public LF2LivingObject HoldPre => _holdPre;

        public abstract bool IsLight { get; }
        public abstract bool IsHeavy { get; }
        // 反汇编 [weapon+368h+6F8h]：0=普通轻武器, 1=重武器, 2=轻特殊, 4=特殊重武器, 6=饮料类
        public abstract int WeaponType { get; }
        // 反汇编 this+800：笛子命中累积器，子类实现存储
        public virtual int FluteWeight { get => 0; set { } }
        // ========== 初始化方法 ==========

        #region 生命周期（Init → InitializeStates → Reset → Destroy → 初始化子步骤）

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
                Log.Error($"[{GetType().Name}] Invalid task type");
                return;
            }

            InitializeParent(task);
            InitializePosition(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializeHealth();

            // FLF: if (T.opoint.kind === 2) 被角色持有
            if (task.opoint.kind == 2 && task.parent != null)
            {
                Pick(task.parent as LF2LivingObject);
                // 反汇编：opoint kind=2 生成武器时，角色持有关系双向绑定
                // weapon.Pick(parent) 设置武器侧；还需告知角色侧持有此武器
                (task.parent as LF2Character)?.HoldWeapon(this);
            }

            Renderer = renderer;
            SimulationTickDriver.Instance?.World?.Register(this);
        }

        protected override void InitializeStates()
        {
            _states[LF2States.WeaponInSky] = State_WeaponInSky;
            _states[LF2States.WeaponOnHand] = State_WeaponOnHand;
            _states[LF2States.WeaponThrowing] = State_WeaponThrowing;
            _states[LF2States.WeaponJustOnGround] = State_WeaponJustOnGround;
            _states[LF2States.WeaponOnGround] = State_WeaponOnGround;
        }

        public override void Reset()
        {
            FrameCache.Clear();
            _objectId = 0;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
            _holdObj = null;
            _holdPre = null;
            _vrest.Clear();
            ShotCount = 0;
            PickerStableId = -1;
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
            _objectId = task.opoint.oid;
            Team = task.team;
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
            int action = (task.opoint.action == 0) ? 999 : task.opoint.action;
            // 加载帧数据
            var wrapper = CharacterAnimtorManager.Instance.GetCharacterConfig(_objectId);
            FrameCache.Load(wrapper);
            Frame.D = FrameCache.GetFrameDataById(action);
            Trans.Frame(action, 0);
        }

        protected void InitializeHealth()
        {
            // 从 DAT 数据读取 weapon_hp / weapon_drop_hurt（反汇编 ParseCharData 0x0040D8F0）
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
            if (charData != null && charData.weapon_hp > 0)
            {
                Health.HP = charData.weapon_hp;
                WeaponDropHurt = charData.weapon_drop_hurt > 0 ? charData.weapon_drop_hurt : WeaponDropHurt;
            }
            else
            {
                Health.HP = 100;
            }
            // 反汇编 Entity_Spawn 0x402A74：[entity+31Ch] = charData[+90h] = weapon_hp
            OnHealthInitialized(charData);
        }

        /// <summary>
        /// InitializeHealth 完成后回调，供子类初始化 _flightCounter 等依赖 weapon_hp 的字段。
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

        public override void SimTransit(int tickIndex)
        {
            Trans?.Trans();
        }

        public override void SimTU(int tickIndex)
        {
            int currentState = GetState();

            if (currentState != _lastState)
            {
                StateUpdate("state_entry", null);
                _lastState = currentState;
            }

            StateUpdate("TU", null);

            UpdateVRest();
            ItrRest?.Tick();

            // 反汇编 Entity_AI_Update 0x004228B8-0x004228C6：
            // type=1/2/4/6 武器，_flightCounter < 0 → 武器消失
            if (_holdObj == null && IsWeaponDestroyable() && GetFlightCounter() < 0)
            {
                StateUpdate("die", null);
                return;
            }

            if (Health.HP <= 0)
            {
                StateUpdate("die", null);
            }
        }

        /// <summary>反汇编 0x004228A0: type=1/2/4/6 才检查 flightCounter</summary>
        protected virtual bool IsWeaponDestroyable() => false;

        /// <summary>供基类 SimTU 读取 _flightCounter</summary>
        protected virtual int GetFlightCounter() => 0;

        #endregion

        #region 帧事件回调（OnGenericStateEvent → OnInFlightFrameUpdate → OnLanded → WeaponFlightPhysics → OnThrown）

        protected override bool OnGenericStateEvent(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "TU":
                    Generic_TU();
                    return false;
                case "die":
                    Generic_Die();
                    return true;
                default:
                    return false;
            }
        }

        protected virtual void OnInFlightFrameUpdate() { }

        /// <summary>
        /// 飞行武器落地后的弹射与停止处理
        /// 对应反汇编 Entity_FrameAdvance 0x4164A9-0x416577（y>=0 路径）
        /// 子类按 WeaponType 重写以实现差异化落地行为
        /// </summary>
        protected virtual void OnLanded()
        {
            // 基类不做任何清零——所有 type 分支由 LF2Weapon.OnLanded() 完整覆盖并 return。
        }

        /// <summary>
        /// 飞行武器每帧的特化物理（在 Dynamics 之前执行）
        /// 对应反汇编 Entity_FrameAdvance 0x416240-0x416577（在空中时的 type 分流）
        /// 子类按 WeaponType 重写
        /// </summary>
        protected virtual void WeaponFlightPhysics() { }

        /// <summary>
        /// 投掷成功后的初始化回调（子类用于初始化 _flightCounter 等）
        /// </summary>
        protected virtual void OnThrown() { }

        #endregion

        #region 通用状态处理（Generic_TU → CheckBoomerangCatch → Generic_Die）

        private void Generic_TU()
        {
            // ── 反汇编 Entity_FrameAdvance 0x416240 入口守卫 ──────────────────────
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

            // 2. held_by [+98h]：被持有时 >= 0，跳过全部飞行物理（反汇编 0x41627D: jl loc_416D9E）
            if (_holdObj != null) return;

            // 3. frame.state == 2（freeze/被抓）时跳过（反汇编 0x4162A1: jz loc_416D9E）
            if (Frame?.D?.state == 2) return;
            // ────────────────────────────────────────────────────────────────────

            Interaction();

            int state = GetState();
            switch (state)
            {
                case LF2States.WeaponOnHand:
                case 2001:
                    break;
                default:
                    // 严格对齐反汇编 Entity_FrameAdvance 0x4162EB-0x416DA4 的执行顺序：
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

                    // 反汇编 0x416577~0x4166CE：type=0 空中（新y<0）时的帧动态切换
                    if (PS.y < -0.0001f)
                        OnInFlightFrameUpdate();
                    break;
            }

            // 反汇编 0x4164A9：新y >= -0.0001 且旧y < 0 表示本帧落地
            if (PS.y >= 0 && PS.vy > 0)
                OnLanded();

            // 反汇编 LABEL_182 末尾：if (frame.state != 12) this+800 = 0
            if ((Frame?.D?.state ?? -1) != LF2States.Falling)
                FluteWeight = 0;

            // 反汇编 0x00405132：type=4 回旋镖飞行中，距投掷者够近时自动回收
            if (WeaponType == 4 && _holdObj == null && PickerStableId >= 0)
                CheckBoomerangCatch();
        }

        /// <summary>
        /// 反汇编 EXE 0x00405132：回旋镖（type=4）捕获检测。
        /// x：|dx| &lt; 30（对称）
        /// z：thrower.z - 80 &lt; weapon.z &lt; thrower.z（单向，武器必须在投掷者前方区间内）
        /// y：|dy| &lt; 10（对称）
        /// 满足条件：frame=60, vx/vy/vz=0

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
            // 反汇编 0x405187-0x405196：z 为单向检测
            // weapon.z <= thrower.z-80 OR weapon.z >= thrower.z → 跳过
            float dy = Mathf.Abs(PS.y - thrower.PS.y);
            if (dx >= 30f || PS.z <= thrower.PS.z - 80f || PS.z >= thrower.PS.z || dy >= 10f) return;

            PS.vx = 0f;
            PS.vy = 0f;
            PS.vz = 0f;
            Trans.Frame(60, 0);
            Trans.Trans();
            // 反汇编 0x004051FC：捕获后设置 thrower.[+0E4h] = 100（HP 恢复计时器）
            thrower.HealTimer = 100;
        }

        /// <summary>
        /// 飞行武器在空中（新y&lt;0）时的帧动态更新。
        /// 对应反汇编 Entity_FrameAdvance 0x416577-0x4166CE（type==0 的 Falling/Burning 帧切换）。
        /// 子类按 WeaponType 重写。

        private void Generic_Die()
        {
            // 反汇编：武器 HP 耗尽后，EXE 由 GameMode 层负责生成 broken_weapon 对象并回收武器 entity（当前框架未实现）。
            // 此处播放破碎音效，broken_weapon 生成留作 GameMode 框架完善后实现。
            PlaySound(WeaponBrokenSound);
            CreateBrokenEffect();
        }

        #endregion

        #region 具体状态处理（State_WeaponInSky / OnHand / Throwing / JustOnGround / OnGround 虚方法）

        protected virtual bool State_WeaponInSky(string eventType, object eventData)
        {
            return false;
        }

        protected virtual bool State_WeaponOnHand(string eventType, object eventData)
        {
            return false;
        }

        protected virtual bool State_WeaponThrowing(string eventType, object eventData)
        {
            return false;
        }

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

            // 反汇编 sub_419F80：外层遍历所有 itr，命中后 goto LABEL_184（继续下一个 itr）
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
                    break; // 每个 itr 只命中一个目标（反汇编内层 bdy 循环命中即跳到下一个 itr）
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
            var kindService = Match?.ItrKindService;
            if (target is not LF2LivingObject livingTarget || !kindService.ShouldHitTarget(itr.kind, this, livingTarget)) return false;

            // 反汇编 0x41A0C9-0x41A20B：itr.attacking 目标过滤
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
                case 8:
                    return HandleWeaponKind8Attach(itr, target);
                default:
                    return false;
            }
        }

        protected virtual bool HandleWeaponKind3Stick(InteractionArea itr, LF2Entity target)
        {
            if (target is LF2WeaponBase) return false;
            if (!ItrArestTest()) return false;

            int catchingFrame = itr.catchingact != null && itr.catchingact.Length > 0 ? itr.catchingact[0] : 0;
            int caughtFrame   = itr.caughtact   != null && itr.caughtact.Length   > 0 ? itr.caughtact[0]   : 0;
            if (catchingFrame <= 0 && caughtFrame <= 0)
                return HandlePreInteractionKind3(itr, target); // 无粘附帧 → 普通攻击

            if (catchingFrame > 0) Trans.Frame(catchingFrame, 0);
            if (caughtFrame > 0 && target is LF2Character ch)
            {
                ch.Trans?.Frame(caughtFrame, 0);
                ch.Trans?.Trans();
            }
            return true;
        }

        /// <summary>
        /// 反汇编 0x42EC85：itr.kind=8 爆符粘附/爆炸。
        /// state=1002 时粘附（vx/vy/vz=0，切爆炸帧）；
        /// state=3002 时爆炸传送（victim 传送到武器位置，heal_timer=throwvz+1000）。

        protected virtual bool HandleWeaponKind8Attach(InteractionArea itr, LF2Entity target)
        {
            if (target is not LF2Character victim) return false;
            if (!ItrArestTest()) return false;

            int curState = Frame?.D?.state ?? -1;

            if (curState == LF2States.WeaponThrowing) // 1002：粘附阶段
            {
                PS.vx = 0f; PS.vy = 0f; PS.vz = 0f;
                // 切到爆炸帧：优先 frame=80，否则 frame=70
                int explodeFrame = GetFrameDataById(80) != null ? 80
                                 : GetFrameDataById(70) != null ? 70 : -1;
                if (explodeFrame >= 0) { Trans.Frame(explodeFrame, 0); Trans.Trans(); }
                return true;
            }

            if (curState == 3002) // 爆炸阶段：传送 victim
            {
                victim.Health.HP -= itr.injury;
                victim.Effect.Heal = itr.throwvz + 1000;
                if (itr.dvx > 0) { victim.Trans?.Frame(itr.dvx, 0); victim.Trans?.Trans(); }
                victim.PS.x = PS.x;
                victim.PS.z = PS.z + 1f;
                FrameDelay = 3;
                victim.FrameDelay = -3;
                return true;
            }

            return false;
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
            // 反汇编 0x0042E9F0-0x0042E9FC：type_sub=0x78 或 0x7C → grabbed_by=101（优先检查）
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
            int typeSub = charData?.type_sub ?? 0;
            if (typeSub == 0x78 || typeSub == 0x7C)
                pickerLink = 101;
            else if (IsHeavy)           // C# type=1 = disasm entity_type=2
                pickerLink = 2;
            else if (WeaponType == 4)
                pickerLink = 4;
            else if (WeaponType == 6)
                pickerLink = Health.HP > 0 ? 6 : 4;
            else
                pickerLink = 0;    // light weapon

            character.GrabbedBy = pickerLink;
            GrabbedBy = -pickerLink;
        }

        private void ApplyPickupFrameJump(LF2Character character)
        {
            int jumpFrame = IsHeavy ? 116 : 115;
            if (character.GetFrameDataById(jumpFrame) != null)
            {
                character.Trans?.Frame(jumpFrame, 0);
                character.Trans?.Trans();
            }
        }

        protected virtual bool HandlePreInteractionKind1(InteractionArea itr, LF2Entity target)
        {
            if (HoldObj != null) return false;
            if (!ItrArestTest()) return false;
            if (Renderer == null) return false;
            if (target is not LF2Character character) return false;
            if (character.GetHeldWeapon() != null) return false;

            // 只有地面武器才能被拾取（反汇编 0x00407378：仅检查 state=1004 和 2004）
            int wstate = GetState();
            bool isOnGround = wstate == LF2States.WeaponOnGround
                           || wstate == LF2States.HeavyWeaponOnGround;
            if (!isOnGround) return false;

            Pick(character);
            character.HoldWeapon(this);
            ApplyPickupGrabbedBy(character);
            ItrArestUpdate(itr);
            target.ItrVrestUpdate(StableId, itr);
            return true;
        }

        protected virtual bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            if (HoldObj != null) return false;
            if (!ItrArestTest()) return false;
            if (Renderer == null) return false;
            if (target is not LF2Character character) return false;
            if (character.GetHeldWeapon() != null) return false;

            // 只有地面武器才能被拾取（反汇编 0x00407378：仅检查 state=1004 和 2004）
            int wstate = GetState();
            bool isOnGround = wstate == LF2States.WeaponOnGround
                           || wstate == LF2States.HeavyWeaponOnGround;
            if (!isOnGround) return false;

            Pick(character);
            character.HoldWeapon(this);
            ApplyPickupGrabbedBy(character);
            // 反汇编 0x42EA9C/0x42EC29：kind=2 拾取后跳转 frame=115/116
            ApplyPickupFrameJump(character);
            ItrArestUpdate(itr);
            target.ItrVrestUpdate(StableId, itr);
            return true;
        }

        protected virtual bool HandlePreInteractionKind3(InteractionArea itr, LF2Entity target)
        {
            // 反汇编 sub_419F80：kind=3 时若 target.charData.type != 0（即目标是武器）则跳过
            // 否则走普通命中路径，与 kind=0 相同
            if (target is LF2WeaponBase) return false;
            return TryApplyHit(itr, target);
        }

        protected virtual bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            // 反汇编 0x42E97B/0x42E984：kind=7 近身拾取，与 kind=1 相同但无帧跳转
            return HandlePreInteractionKind1(itr, target);
        }

        #endregion

        #region 战斗（Hit → Act → ForceClearHolder → Drop → Pick → ProcessDrinkConsumption → OnDrinkConsumed → ProcessAttack → SetWeaponStrengthList → GetStrengthEntry）

        /// <summary>
        /// 对应 FLF weapon.prototype.hit (weapon.js:275-365)
        /// </summary>
        public abstract bool Hit(InteractionArea itr, LF2Entity attacker);

        /// <summary>
        /// 对应 FLF weapon.prototype.act (weapon.js:367-465)
        /// 反汇编 AI_Process2 (0x0041AAC0)
        /// </summary>
        public virtual WeaponActResult Act(LF2LivingObject holder, WeaponPoint wpoint, Vector3 holdpoint)
        {
            var result = new WeaponActResult();
            if (Frame.D == null) return result;

            // 反汇编 AI_Process2 0x0041AFFC：
            // 持有者处于 Falling(12) 或 BeingCaught(10) 时强制脱落武器
            // → 双方 arest=0，武器随机帧[0,15]，速度继承持有者速度 * 1/3
            int holderState = holder?.GetState() ?? -1;
            if (holderState == LF2States.Falling || holderState == LF2States.BeingCaught)
            {
                ItrRest.Arest = 0;
                holder.ItrRest.Arest = 0;

                Trans.Frame(UnityEngine.Random.Range(0, 16), 0);

                // 反汇编 0x41B035-0x41B075：按 holder.CharType 选速度来源
                // CharType==1：vx = holder.KnockbackVx * 1/3（[holder+28h]），vy = holder.KnockbackVy（直接复制）
                // CharType!=1：vx = holder.vx * 1/3，vy = holder.vy（直接复制）
                // vz 不设置（反汇编无 vz 赋值）
                const float kVelFactor = 1f / 3f;
                if (holder.CharType == 1)
                {
                    PS.vx = holder.KnockbackVx * kVelFactor;  // [holder+28h] = KnockbackVx
                    PS.vy = holder.KnockbackVy;                // 直接复制，不乘1/3
                }
                else
                {
                    PS.vx = holder.PS.vx * kVelFactor;
                    PS.vy = holder.PS.vy;                // 直接复制，不乘1/3
                }

                // 反汇编 0x41B07A-0x41B08D：if weapon.y_float > -2.0 → y_float = -2.0
                // 确保武器脱落时至少在地面以上2单位
                if (PS.y > -2.0f) PS.y = -2.0f;

                // 反汇编 0x41B011：character.grabbed_by=0, weapon.grabbed_by=0
                GrabbedBy = 0;
                if (holder is LF2Character ch2) ch2.GrabbedBy = 0;

                _holdObj = null;
                (holder as LF2Character)?.HoldWeapon(null);
                result.ForceDrop = true;
                return result;
            }

            // 反汇编 0x41AEAD：weapon.[+0B4h] = holder.[+0B4h]
            // [+0B4h] = FrameDelay（帧延迟计数器），武器与持有者同步
            FrameDelay = holder.FrameDelay;

            // 切换到武器动作帧
            // 反汇编 0x41AE98：直接写 weapon.frame = holder_wpoint.action，不触发帧事件
            if (wpoint.weaponact > 0)
            {
                ImmediateFrame(wpoint.weaponact);
            }

            var fD = Frame.D;
            if (fD?.wpoints == null || fD.wpoints.Count == 0) return result;

            var fwpoint = fD.wpoints[0];

            // 反汇编 AI_Process2 0x41ABF2：触发条件是 holder 当前帧的 frame.state == 17（0x11）
            // [ecx+edx*8+7ACh] = charData.frames[frame].state，7ACh-7A4h=8=state偏移
            // 不是 wpoint.kind，是 holder 帧的 state 字段
            if (holder?.Frame?.D?.state == 17)
            {
                ProcessDrinkConsumption(holder, result);
                return result;
            }

            if (fwpoint.kind == 2) // 可投掷
            {
                if (wpoint.dvx != 0)
                {
                    UnityEngine.Debug.Log($"[Weapon Act] id={_objectId} wt={WeaponType} fwpoint.kind={fwpoint.kind} wpoint.dvx={wpoint.dvx}");
                    // 反汇编 AI_Process2 0x41B094~0x41B21D：
                    // 按 weapon.type 分流投掷路径：
                    //   type=1/4/6 → heavy throw：frame固定40，双方arest归零
                    //   type=2     → light throw：Random_Int(6)帧，双方arest归零
                    //   type=0     → 无投掷路径，dvx有值时仍走kind=3（ProcessForceDropPoint）
                    int wt = WeaponType;
                    bool isHeavyThrow = wt == 1 || wt == 4 || wt == 6;
                    bool isLightThrow = wt == 2;

                    if (isHeavyThrow)
                    {
                        // 反汇编 0x41B0C2-0x41B155：frame=40，vx按facing，vy=dvy
                        // dvz 由 holder.key_up/key_down 控制（0x41B114-0x41B155）：
                        //   key_up!=0 && key_down==0 → vz = -dvz
                        //   key_up==0 && key_down!=0 → vz = +dvz
                        //   其他 → vz 不变
                        Trans.Frame(40, 0);
                        Trans.Trans();
                        UnityEngine.Debug.Log($"[Weapon Throw] id={_objectId} frame40 exists={GetFrameDataById(40) != null} Frame.N={Frame.N} Frame.D={Frame.D?.state}");
                        PS.vx = Dirh() * wpoint.dvx;
                        // 反汇编 0x41B0F7: fild [edx+1Ch] -> weapon.vy = dvy（无条件赋值，无零值守卫）
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
                        PickerStableId = holder?.StableId ?? -1;
                        OnThrown();
                        result.Thrown = true;
                    }
                    else if (isLightThrow)
                    {
                        // 反汇编 0x41B173-0x41B219：Random(6)帧，vx按facing，vy=dvy
                        // dvz 控制逻辑同上（0x41B1DB-0x41B216）
                        Trans.Frame(UnityEngine.Random.Range(0, 6), 0);
                        Trans.Trans();
                        PS.vx = Dirh() * wpoint.dvx;
                        // 反汇编 0x41B1B7: fild [edx+1Ch] -> weapon.vy = dvy（无条件赋值，无零值守卫）
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
                        PickerStableId = holder?.StableId ?? -1;
                        OnThrown();
                        result.Thrown = true;
                    }
                    // type=0：dvx非零也不投掷，转kind=3强制丢弃（反汇编 0x41B155→0x41B16D→0x41B21D）
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

                    // coincideXY
                    CoincideXYWithWPoint(holdpoint, fwpoint);

                    // 反汇编 AI_Process2 0x41AFA7-0x41AFCC：
                    // cover==0 → z+=1, y-=1（武器在角色前面，稍偏前/上）
                    // cover!=0 → z-=1, y+=1（武器在角色后面，稍偏后/下）
                    if (cover == 0) { PS.z += 1f; PS.y -= 1f; }
                    else            { PS.z -= 1f; PS.y += 1f; }
                }
            }

            // 反汇编 GameMode_Process 0x0041BDDF：武器 state==1001（持有中）且持有者 wpoint.attacking>0 才攻击
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
        }

        /// <summary>
        /// 反汇编 AI_Process2 0x41B011-0x41B08D：角色被打（state=12/10）时武器强制脱落。
        ///   weapon.frame = Random(16)
        ///   vx = holder.vx * 1/3（dvx 传入 holder.vx）
        ///   vy = holder.vy（dvy 传入 holder.vy，直接复制不乘系数）
        ///   if weapon.y > -2.0 → weapon.y = -2.0
        /// </summary>
        public virtual void Drop(float dvx, float dvy)
        {
            Team = 0;
            _holdObj = null;
            GrabbedBy = 0;

            // 反汇编 0x41B019: weapon.frame = Random(16)
            Trans.Frame(UnityEngine.Random.Range(0, 16), 0);

            // 反汇编 0x41B035-0x41B075: vx = holder.vx * 1/3, vy = holder.vy（直接复制）
            PS.vx = dvx * (1f / 3f);
            PS.vy = dvy;

            // 反汇编 0x41B07A: if weapon.y > -2.0 → weapon.y = -2.0
            if (PS.y > -2.0f) PS.y = -2.0f;

            PS.zz = 0;
        }

        /// <summary>
        /// 对应 FLF weapon.prototype.pick (weapon.js:484-498)
        /// </summary>
        public virtual bool Pick(LF2LivingObject holder)
        {
            if (_holdObj != null) return false;

            _holdObj = holder;
            _holdPre = holder;
            Team = holder.Team;

            return true;
        }


        /// <summary>
        /// 饮料消耗处理（反汇编 AI_Process2 0x41ABF2-0x41AE73）。
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

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
            int typeSub = charData?.type_sub ?? 0;

            // 反汇编 0x41AC21：type_sub == 0x7A → 饮料（liquid）路径
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

                // 0x41ADDA: 如果 weapon.[+2F4h]（_flightCounter 别字段?）> -1 且 PP > 150(0x96)
                // → clamp PP <= 150。[+2F4h] 是另一个计数器字段，暂时忽略此细节
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
            // 0x41AD73: weapon.[+31Ch]（_flightCounter）= 0
            if (holder is LF2Character holderChar) holderChar.GrabbedBy = 0;
            GrabbedBy = 0;
            Trans.Frame(0, 0);
            PS.vx = UnityEngine.Random.Range(0, 7) - 3f;
            PS.vy = -8.0f;  // double 0xC020000000000000 = -8.0
            PS.vz = 0f;
            PS.zz = 0;
            Team = 0;
            holder.Trans?.Frame(0, 0);
            OnDrinkConsumed();
            _holdObj = null;
            (holder as LF2Character)?.HoldWeapon(null);
            result.ForceDrop = true;
        }

        /// <summary>
        /// 饮料消耗完毕后的子类钩子，用于重置 _flightCounter 等字段。
        /// 反汇编 0x41AD73: weapon.[+31Ch] = 0
        /// </summary>
        protected virtual void OnDrinkConsumed() { }

        protected virtual WeaponAttackResult ProcessAttack(LF2LivingObject holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            // TODO: 实现攻击处理
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

        #region 辅助方法（WhirlwindForce → FluteForce → CoincideXYWithWPoint → GetSpeed → PlaySound → CreateBrokenEffect → CreateEffect → MakePointCenter → CoincideXYForInit）

        public void WhirlwindForce(InteractionArea itr, LF2Entity attacker)
        {
            // TODO: 实现龙卷风效果
        }

        public override void FluteForce()
        {
            // 反汇编 Entity_AI_Update line 1535：kind=10/11 命中时 this+800 = -20
        }

        /// <summary>
        /// 将武器与持有者的 wpoint 对齐。
        /// 反汇编 AI_Process2 0x41AEDF-0x41AF8F：
        ///   dir=right: weapon.x = holdpoint.x + weapon_frame.centerx - weapon_spriteWidth
        ///   dir=left:  weapon.x = holdpoint.x + weapon_spriteWidth - weapon_frame.centerx
        ///   weapon.y   = holdpoint.y + weapon_frame.centery - weapon_wpoint.y
        /// </summary>
        protected void CoincideXYWithWPoint(Vector3 holdpoint, WeaponPoint wpoint)
        {
            var weapFD = Frame?.D;
            int wcx = weapFD?.centerx ?? 0;
            int wcy = weapFD?.centery ?? 0;
            float wSpriteW = Sprite?.GetWidthPx() ?? 0f;
            int wpy = wpoint?.y ?? 0;

            if (PS.dir == "right")
                PS.x = holdpoint.x + wcx - wSpriteW;
            else
                PS.x = holdpoint.x + wSpriteW - wcx;

            PS.y = holdpoint.y + wcy - wpy;
        }

        public float GetSpeed()
        {
            return Mathf.Sqrt(PS.vx * PS.vx + PS.vy * PS.vy);
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }

        public void CreateBrokenEffect()
        {
            // TODO: 实现破碎效果
        }

        public void CreateEffect(int type)
        {
            // TODO: 实现特效
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

        #region VRest 系统（IsVRest / SetVRest / UpdateVRest）

        // ========== VRest 系统 ==========
        private List<int> _vrestKeysCache = new List<int>();
        private List<LF2LivingObject> _boomerangQueryCache = new List<LF2LivingObject>(8);

        public bool IsVRest(LF2Entity obj)
        {
            if (obj == null) return false;
            return _vrest.ContainsKey(obj.StableId) && _vrest[obj.StableId] > 0;
        }

        public void SetVRest(LF2Entity obj, int value)
        {
            if (obj == null) return;
            _vrest[obj.StableId] = value;
        }

        private void UpdateVRest()
        {
            _vrestKeysCache.Clear();
            foreach (var key in _vrest.Keys)
            {
                _vrestKeysCache.Add(key);
            }
            foreach (var key in _vrestKeysCache)
            {
                if (_vrest[key] > 0)
                    _vrest[key]--;
            }
        }

        // ========== 辅助方法 ==========

        #endregion

    }

    // ========== 结果类 ==========

    public class WeaponActResult
    {
        public bool Thrown;
        public bool ForceDrop;       // 反汇编 AI_Process2 0x41AFFC：持有者 Falling/BeingCaught 时强制脱落
        public bool NeedsKind3Drop;  // 反汇编 0x41B155~0x41B16D：type=0武器dvx≠0时转kind=3强制丢弃
        public WeaponAttackResult AttackResult;
    }

    public class WeaponAttackResult
    {
        public int VRest;
        public int ARest;
        public int HitUid;
    }
}
