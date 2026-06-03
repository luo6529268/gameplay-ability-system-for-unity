using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using System.Collections.Generic;
using UnityEngine;
using NTSD.Animation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 所有战斗实体的抽象基类（对应反汇编 entity 结构体公共部分）
    ///
    /// 反汇编确认：
    ///   Entity_FrameAdvance (0x416240) 对所有 400 个 entity slot 统一调用
    ///   RenderDispatch (0x41D010) 对所有 active entity 统一画阴影和 sprite
    ///   PostRender (0x41D830) spark slot 系统对所有 entity 共用
    ///
    /// 层次：
    ///   ILF2Object  → 帧驱动框架契约（对象池 + 模拟系统）
    ///   ILF2Entity  → 战斗实体公共契约
    ///   LF2Entity   → 公共字段与逻辑实现（本类）
    ///     ├── LF2LivingObject  → 角色专属
    ///     ├── LF2WeaponBase    → 武器专属
    ///     └── LF2SpecialAttack → 技能专属
    /// </summary>
    public abstract class LF2Entity : ILF2Entity
    {
        // ─────────────────────────────────────────────────────────────────
        #region 标识字段

        /// <summary>对象名称</summary>
        public string Name { get; set; }

        /// <summary>唯一 ID（对应反汇编 entity StableId）</summary>
        public int StableId
        {
            get => Runtime.StableId;
            protected set => Runtime.StableId = value;
        }

        /// <summary>对象 ID（对应 entity ObjectId）</summary>
        public int ObjectId
        {
            get => Runtime.ObjectId;
            set => Runtime.ObjectId = value;
        }

        /// <summary>队伍 ID（entity+364h，值 1~5 对应不同队伍）</summary>
        public int Team
        {
            get => Runtime.Team;
            set => Runtime.Team = value;
        }

        /// <summary>阵营标记（entity+8h，0=右/1=左）</summary>
        public int TeamSide
        {
            get => Runtime.TeamSide;
            set => Runtime.TeamSide = value;
        }

        /// <summary>所有者 entity slot index（entity+2F4h），-1 表示无</summary>
        public int OwnerId
        {
            get => Runtime.OwnerStableId;
            set => Runtime.OwnerStableId = value;
        }

        /// <summary>被抓取状态（entity+98h grabbed_by）</summary>
        public int GrabbedBy
        {
            get => Runtime.GrabbedBy;
            set => Runtime.GrabbedBy = value;
        }

        /// <summary>kind==2 tracker 标志（parent=1, child=-1）</summary>
        public int TrackerFlag
        {
            get => Runtime.TrackerFlag;
            set => Runtime.TrackerFlag = value;
        }

        /// <summary>kind==2 tracker 子对象引用（entity+9Ch）</summary>
        public LF2Entity TrackerChild { get; set; }

        /// <summary>kind==2 tracker 父对象引用（entity+0A0h）</summary>
        public LF2Entity TrackerParent { get; set; }

        /// <summary>当前命中时使用的 itr slot 索引（用于 spark 计时器计算）</summary>
        public int CurrentItrIndex { get; set; }

        /// <summary>对象类型（int，由子类 ObjectTypeEnum 决定）</summary>
        public int ObjectType => (int)ObjectTypeEnum;

        /// <summary>对象类型枚举（子类实现）</summary>
        public abstract LF2ObjectType ObjectTypeEnum { get; }

        public NTSDEntityRuntime Runtime { get; } = new NTSDEntityRuntime();

        public virtual int ReleaseEntityType => ObjectType;

        /// <summary>对象类型别名（等同 ObjectTypeEnum，兼容旧代码）</summary>
        public LF2ObjectType Type => ObjectTypeEnum;

        // ─────────────────────────────────────────────────────────────────
        #region 核心模块字段

        /// <summary>物理状态（entity+58h/60h/68h x/y/z float）</summary>
        public PhysicsState PS { get; protected set; }

        /// <summary>帧信息（entity+70h frame index）</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>dat 帧数据缓存（entity+368h）</summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        /// <summary>帧转换器</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>效果状态（TimeIn/Stuck 等）</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>Sprite 资源引用</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>渲染器引用</summary>
        public LF2ObjectRenderer Renderer { get; protected set; }

        /// <summary>模拟世界引用</summary>
        public SimulationWorld Match => SimulationTickDriver.Instance?.World;

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region 战斗字段

        /// <summary>帧延迟计数器（entity+0B4h）</summary>
        public int FrameDelay
        {
            get => Runtime.FrameDelay;
            set => Runtime.FrameDelay = value;
        }

        /// <summary>命中锁定标志（entity+88h hit_stun）</summary>
        public int HitStun
        {
            get => Runtime.HitStop;
            set => Runtime.HitStop = value;
        }

        /// <summary>累积击退 X 速度（entity+28h knockback_vx）</summary>
        public float KnockbackVx
        {
            get => Runtime.KnockbackVx;
            set => Runtime.KnockbackVx = value;
        }

        /// <summary>累积击退 Y 速度（entity+30h knockback_vy）</summary>
        public float KnockbackVy
        {
            get => Runtime.KnockbackVy;
            set => Runtime.KnockbackVy = value;
        }

        /// <summary>累积击退 Z 速度（entity+38h knockback_vz）</summary>
        public float KnockbackVz
        {
            get => Runtime.KnockbackVz;
            set => Runtime.KnockbackVz = value;
        }

        /// <summary>角色类型（entity+20h char_type）</summary>
        public int CharType
        {
            get => Runtime.CharType;
            set => Runtime.CharType = value;
        }

        /// <summary>震屏计时器（entity+8h shake_timer）</summary>
        public int ShakeTimer
        {
            get => Runtime.ShakeTimer;
            set => Runtime.ShakeTimer = value;
        }

        /// <summary>攻击豁免计数器（entity+0ECh attack_exempt）。命中后设 6，每帧 -1；LF2LivingObject 重写为 HitCounters.AttackExempt。</summary>
        public virtual int AttackExempt
        {
            get => Runtime.AttackExempt;
            set => Runtime.AttackExempt = value;
        }

        /// <summary>所有者实体索引（entity+756=0x2F4h），初始值 -1；opoint 生成时设为生成者索引。P1-6</summary>
        public int OwnerEntityIndex
        {
            get => Runtime.OwnerSlotIndex;
            set => Runtime.OwnerSlotIndex = value;
        }

        /// <summary>弹射计数（entity+308h）</summary>
        public int ShotCount
        {
            get => Runtime.ShotCount;
            set => Runtime.ShotCount = value;
        }

        // ItrRest / Health / HealTimer 在各子类中定义具体字段；
        // 此处提供虚属性，让 LF2Entity 的公共方法（ItrArestTest 等）能统一访问。
        /// <summary>itr 攻击冷却追踪器（子类提供具体实例）</summary>
        public virtual LF2ItrRestTracker ItrRest { get; protected set; } = null;
        /// <summary>生命值（子类提供具体实例）</summary>
        public virtual LF2Health Health { get; protected set; } = null;
        /// <summary>HP 恢复计时器</summary>
        public virtual int HealTimer { get; set; } = 0;

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region 阴影（RenderDispatch 统一渲染）

        /// <summary>
        /// 阴影 SpriteRenderer（对应反汇编 RenderDispatch shadow blit）
        /// 由子类渲染器在初始化时通过 SetShadowRenderer() 注入
        /// </summary>
        public SpriteRenderer ShadowRenderer { get; private set; }

        /// <summary>注入阴影渲染器引用</summary>
        public void SetShadowRenderer(SpriteRenderer sr) => ShadowRenderer = sr;

        /// <summary>
        /// 更新阴影位置（对应反汇编 RenderDispatch shadow 公式）
        /// shadow_x = px / ppu, shadow_y = pz / ppu（只用地面深度，不含 py）
        /// 隐藏条件：state==3005, state==9997, py &lt; -70, blink 且 renderFrame%4 >= 2
        /// </summary>
        public void UpdateShadow(int renderFrame = 0)
        {
            if (ShadowRenderer == null || PS == null) return;

            int state = Frame?.D?.state ?? -1;
            int oid = ObjectId;
            // 反汇编 0x0041D1C9~0x0041D20B：state==3005/9997, oid==223/224, y<=-70, abs(y)&3>=2
            bool hide = state == 3005
                     || state == 9997
                     || oid == 223
                     || oid == 224;

            ShadowRenderer.enabled = !hide;
            if (!hide)
            {
                var t = ShadowRenderer.transform;
                t.position = new Vector3(PS.x / SimulationConstants.PIXELS_PER_UNIT, PS.z / SimulationConstants.PIXELS_PER_UNIT, t.position.z);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region Spark 系统（PostRender 所有 entity 共用）

        /// <summary>当前活跃 spark slot 数量（obj[0x36C]）</summary>
        public int SparkSlotCount { get; private set; } = 0;

        /// <summary>最大 spark slot 数量（反汇编 cmp ecx, 0Ah → 上限 10）</summary>
        public const int MaxSparkSlots = 10;

        private readonly int[]   _sparkTimers        = new int[MaxSparkSlots];
        private readonly float[] _sparkWorldX        = new float[MaxSparkSlots];
        private readonly float[] _sparkWorldY        = new float[MaxSparkSlots];
        private readonly float[] _sparkWorldZ        = new float[MaxSparkSlots];
        private readonly int[]   _sparkLastTickFrame = new int[MaxSparkSlots];

        /// <summary>命中时追加新 spark slot</summary>
        public void AddSparkSlot(int timerInitial, float worldX, float worldY, float worldZ, int currentRenderFrame = -1)
        {
            if (SparkSlotCount >= MaxSparkSlots) return;
            int slot = SparkSlotCount;
            _sparkTimers[slot]        = timerInitial;
            _sparkLastTickFrame[slot] = currentRenderFrame;
            _sparkWorldX[slot]        = worldX;
            _sparkWorldY[slot]        = worldY;
            _sparkWorldZ[slot]        = worldZ;
            SparkSlotCount++;
        }

        /// <summary>读取指定 slot 的 timer 值</summary>
        public int GetSparkTimer(int slotIndex) => _sparkTimers[slotIndex];

        /// <summary>读取指定 slot 的世界坐标</summary>
        public Vector3 GetSparkWorldPos(int slotIndex)
            => new Vector3(_sparkWorldX[slotIndex], _sparkWorldY[slotIndex], _sparkWorldZ[slotIndex]);

        /// <summary>推进指定 slot 的 timer（已废弃，请使用 TickAllSparkTimers）</summary>
        public void IncrementSparkTimer(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < SparkSlotCount)
                _sparkTimers[slotIndex]++;
        }

        /// <summary>移除最后一个 slot</summary>
        public void RemoveLastSparkSlot()
        {
            if (SparkSlotCount > 0) SparkSlotCount--;
        }

        private void RemoveSparkSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SparkSlotCount) return;
            int tail = SparkSlotCount - 1;
            if (slotIndex < tail)
            {
                System.Array.Copy(_sparkTimers,        slotIndex + 1, _sparkTimers,        slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldX,        slotIndex + 1, _sparkWorldX,        slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldY,        slotIndex + 1, _sparkWorldY,        slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldZ,        slotIndex + 1, _sparkWorldZ,        slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkLastTickFrame, slotIndex + 1, _sparkLastTickFrame, slotIndex, tail - slotIndex);
            }
            SparkSlotCount--;
        }

        /// <summary>
        /// 每游戏逻辑帧（30Hz）递增所有 spark slot 的 timer，并移除过期 slot。
        /// 对应反汇编 PostRender spark timer 递增 + slot 移除逻辑。
        /// </summary>
        public void TickAllSparkTimers(int renderFrame)
        {
            for (int i = SparkSlotCount - 1; i >= 0; i--)
            {
                if (renderFrame - _sparkLastTickFrame[i] < 2) continue;
                _sparkLastTickFrame[i] = renderFrame;
                _sparkTimers[i]++;
                int t = _sparkTimers[i];
                bool remove = (t >= 5 && t < 10) || (t >= 15 && t < 30) || (t >= 39);
                if (remove) RemoveSparkSlot(i);
            }
        }
        protected void ResetSpark() => SparkSlotCount = 0;

        #endregion
        /// <summary>状态处理器帧号返回通道</summary>
        public int StateReturnFrame { get; protected set; } = 0;

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region 状态系统（所有 entity 共用）

        /// <summary>
        /// 状态更新分发（对应 FLF livingobject.prototype.state_update）
        /// </summary>
        /// <summary>通用状态事件处理（子类重写）</summary>
        protected virtual bool StateExitEvent() => false;
        protected virtual bool StateEntryEvent() => false;
        protected virtual bool FrameForceEvent() => false;
        protected virtual bool FrameEvent() => false;
        protected virtual bool TransitEvent() => false;
        protected virtual bool TUForceEvent() => false;
        protected virtual bool TUEvent() => false;
        protected virtual bool DieEvent() => false;
        protected virtual bool DestroyEvent() => false;
        protected virtual bool ComboEvent(string combo) => false;
        protected virtual void PostComboEvent() { }
        protected virtual bool FellOntoGroundEvent(out int frameId) { frameId = 0; return false; }
        protected virtual bool FallOntoGroundEvent(out int frameId) { frameId = 0; return false; }
        protected virtual bool HitStopEvent(out int frameId) { frameId = 0; return false; }
        protected virtual bool PostInteractionEvent(out int frameId) { frameId = 0; return false; }

        /// <summary>获取当前状态</summary>
        public virtual int GetState() => Frame.D?.state ?? 0;

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region 方向系统（所有 entity 共用）

        public virtual void SwitchDir(string dir)
        {
            if (PS == null) return;
            if (PS.dir == "left" && dir == "right") { PS.dir = "right"; Sprite?.SwitchLR("right"); }
            else if (PS.dir == "right" && dir == "left") { PS.dir = "left"; Sprite?.SwitchLR("left"); }
        }

        public virtual void SwitchDir(DIRECTION direction)
            => SwitchDir(direction == DIRECTION.LEFT ? "left" : "right");

        public virtual int Dirh() => PS?.dir == "left" ? -1 : 1;

        public virtual int Dirv() => 1;

        protected virtual string CalculateDirection(int facing, string parentDir)
        {
            int face = facing >= 20 ? facing % 10 : facing;
            if (face == 0) return parentDir;
            if (face == 1) return parentDir == "right" ? "left" : "right";
            if (face >= 2 && face <= 10) return "right";
            if (face >= 11 && face <= 19) return "left";
            return parentDir;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region 笛子/风暴受力（kind=10/11，所有 entity 共用）

        /// <summary>
        /// 受到 itr kind=10/11 时的受力处理（角色、武器均适用）
        /// 对应反汇编 entity_flute_force 逻辑
        /// </summary>
        public virtual void FluteForce()
        {
            if (PS == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);

            const float lowLevel  = -140f;
            const float midLevel  = -160f;
            const float highLevel = -180f;

            Effect.Super = true;
            PS.vx = 0;
            PS.vz = 0;

            if (PS.y > lowLevel)
                PS.vy = (PS.vy <= 0) ? -7.5f : -PS.vy / 2f;
            else if (PS.y <= lowLevel && PS.y > midLevel)
                PS.vy -= mass / 2f;
            else if (PS.y <= midLevel && PS.y > highLevel)
                PS.vy += mass / 2f;

            switch (ObjectTypeEnum)
            {
                case LF2ObjectType.Character:
                    if (Frame.N >= 55) TransitionToFrame(40, 20);
                    break;
                case LF2ObjectType.HeavyWeapon:
                    if (Frame.N >= 5) TransitionToFrame(1, 20);
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region 位置与数据工具

        public void SetPos(float x, float y, float z)
        {
            if (PS == null) return;
            PS.x = x; PS.y = y; PS.z = z;
        }

        public virtual int GetFirstFrameByState(int state)
            => FrameCache?.GetFirstFrameByState(state) ?? -1;

        public virtual void BrokenEffectCreate(int id, int num = 8) { }

        public virtual void PlayFrameByID(int frameId) => Trans?.Frame(frameId, 0);

        
        // ─────────────────────────────────────────────────────────────────
        // 以下方法在所有 entity 中均需可调用（角色/武器/技能均有 ItrRest）
        // 对应反汇编 itr_arest / itr_vrest 公共逻辑

        /// <summary>检查 itr 攻击冷却（arest）是否可攻击</summary>
        public bool ItrArestTest() => ItrRest == null || ItrRest.Arest <= 0;

        /// <summary>命中后更新 arest 冷却</summary>
        public void ItrArestUpdate(InteractionArea itr)
        {
            if (ItrRest == null) return;
            if (itr != null && itr.arest > 0)
                ItrRest.Arest = itr.arest;
            else if (itr == null || itr.vrest <= 0)
                ItrRest.Arest = NTSDGlobal.Default.Character.ARest;
        }

        /// <summary>检查对指定攻击者的 vrest 冷却是否结束</summary>
        public bool ItrVrestTest(int uid) => ItrRest == null || !ItrRest.HasVrest(uid);

        /// <summary>更新对指定攻击者的 vrest 冷却</summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            // 反汇编 0x0042D7C5：itr.arest > 0 → vrest = itr.arest；否则 vrest = itr.vrest
            int vrest = (itr.arest > 0) ? itr.arest : itr.vrest;
            ItrRest.SetVrest(attackerUid, vrest);
        }

        /// <summary>更新对指定攻击者的 vrest 冷却（击飞路径，固定值 45）</summary>
        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            // 反汇编 0x0042D762：击飞路径固定写 45
            ItrRest.SetVrest(attackerUid, 45);
        }

        /// <summary>
        /// 直接切换到指定帧（绕过 Trans.wait，立即生效）
        /// 对应反汇编 frame_update 中的直接写帧逻辑
        /// </summary>
        public virtual void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;
            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            Frame.N  = frameId;
            Frame.D  = targetFrame;

            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SetWait(Frame.D.wait, 99);
            Trans.SetNext(Frame.D.next, 99);
        }

        /// <summary>
        /// 每帧时间更新（TU = time-unit update）
        /// 各子类在此处 override 实现各自的帧内计时逻辑
        /// </summary>
        public virtual void TUUpdate() { }

        /// <summary>按帧 ID 获取帧数据</summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
            => FrameCache?.GetFrameDataById(frameId);

        /// <summary>跳转到指定帧</summary>
        public virtual void TransitionToFrame(int frameId, int wait = 0)
            => Trans?.Frame(frameId, wait);

        /// <summary>获取碰撞用 sprite 宽度（像素），子类重写</summary>
        public virtual float GetSpriteWidthPxForCollision() => 0f;

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region ILF2Poolable / ILF2Object 接口（子类实现）

        public abstract void Reset();
        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);
        /// <summary>从 SimulationWorld 注销自身（及关联对象）。仅由 ResetState() 调用，不在 Reset() 里调用。</summary>
        public virtual void UnregisterFromWorld()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
        }

        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>当 FrameTransistor 检测到 next=1000 时调用（子类实现销毁逻辑）</summary>
        public virtual void OnTransitDestroy()
        {
            DestroyEvent();
            Destroy();
            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        /// <summary>帧转换回调（子类实现具体帧切换逻辑）</summary>
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock) { }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region ISimObject 接口

        public int SimOrder => SimOrderConstants.GetSimOrderByObjectType(ObjectTypeEnum);

        public virtual void OnAdded(SimContext ctx)
        {
            RefreshRuntimeSnapshot();
        }
        public virtual void OnRemoved(SimContext ctx)
        {
            Runtime.SlotIndex = -1;
        }
        public virtual void SimTransit(int tickIndex) { }
        public virtual void SimTU(int tickIndex) { }
        public virtual void SimPostInteraction(int tickIndex) { }
        public virtual void SimPreInteraction(int tickIndex) { }
        public virtual void SimEntityCollision(int tickIndex) { }
        public virtual void SimLateTick(int tickIndex)
        {
            if (PS != null) Sprite?.SetZ(PS.z + PS.zz);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region 保护工具方法

        protected void AllocateStableId()
        {
            StableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;
            Runtime.StableId = StableId;
        }

        protected void ResetStableId()
        {
            StableId = 0;
            Runtime.StableId = 0;
        }

        public void SetRuntimeSlotIndex(int slotIndex)
        {
            Runtime.SlotIndex = slotIndex;
        }

        public void RefreshRuntimeSnapshot()
        {
            RefreshRuntimeFromEntity();
        }

        protected virtual void RefreshRuntimeFromEntity()
        {
            Runtime.StableId = StableId;
            Runtime.ObjectId = ObjectId;
            Runtime.ObjType = ObjectType;
            Runtime.EntityType = ReleaseEntityType;
            Runtime.Team = Team;
            Runtime.TeamSide = TeamSide;
            Runtime.OwnerSlotIndex = OwnerEntityIndex;
            Runtime.OwnerStableId = OwnerId;
            Runtime.GrabbedBy = GrabbedBy;
            Runtime.TrackerFlag = TrackerFlag;
            Runtime.TrackerChildStableId = TrackerChild?.StableId ?? -1;
            Runtime.TrackerParentStableId = TrackerParent?.StableId ?? -1;
            Runtime.Frame = Frame?.N ?? 0;
            Runtime.PrevFrame = Frame?.PN ?? 0;
            Runtime.WaitCounter = Trans?.WaitCounter ?? 0;
            Runtime.NextFrame = Trans?.Next ?? 0;
            Runtime.FrameDelay = FrameDelay;
            Runtime.HitStop = HitStun;
            Runtime.AttackExempt = AttackExempt;
            Runtime.ShotCount = ShotCount;
        }

        #endregion
    }
}
