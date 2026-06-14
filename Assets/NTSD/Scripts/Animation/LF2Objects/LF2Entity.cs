using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 所有战斗实体的抽象基类，承载 C++ release 实体运行时的公共字段、帧入口和 Unity 桥接。
    /// </summary>
    public abstract class LF2Entity : ILF2Entity
    {

        /// <summary>对象名称。</summary>
        public string Name { get; set; }

        /// <summary>实体稳定 ID。</summary>
        public int StableId
        {
            get => Runtime.StableId;
            protected set => Runtime.StableId = value;
        }

        /// <summary>对象 ID。</summary>
        public int ObjectId
        {
            get => Runtime.ObjectId;
            set => Runtime.ObjectId = value;
        }

        /// <summary>队伍 ID。</summary>
        public int Team
        {
            get => Runtime.Team;
            set => Runtime.Team = value;
        }

        public virtual int RelationTeam
        {
            get => Runtime.RelationTeam;
            set => Runtime.RelationTeam = value;
        }

        /// <summary>生成者 StableId；-1 表示无生成者。</summary>
        public int OwnerId
        {
            get => Runtime.OwnerStableId;
            set => Runtime.OwnerStableId = value;
        }

        /// <summary>被抓取状态。</summary>
        public int GrabbedBy
        {
            get => Runtime.GrabbedBy;
            set => Runtime.GrabbedBy = value;
        }

        /// <summary>kind==2 的 tracker 标记。</summary>
        public int TrackerFlag
        {
            get => Runtime.TrackerFlag;
            set => Runtime.TrackerFlag = value;
        }

        /// <summary>kind==2 的 tracker 父对象引用。</summary>
        public LF2Entity TrackerParent { get; set; }

        /// <summary>当前命中的 itr 槽位索引，用于 spark 计时。</summary>
        public int CurrentItrIndex { get; set; }

        /// <summary>对象类型整数值，由子类 ObjectTypeEnum 决定。</summary>
        public int ObjectType => (int)ObjectTypeEnum;

        /// <summary>对象类型枚举，由子类实现。</summary>
        public abstract LF2ObjectType ObjectTypeEnum { get; }

        /// <summary>C++ release 实体运行时字段镜像。</summary>
        public NTSDEntityRuntime Runtime { get; } = new NTSDEntityRuntime();

        private static readonly DeterministicRng FallbackRng = new DeterministicRng(0x4E545344u);

        /// <summary>C++ release 实体类型值。</summary>
        public virtual int ReleaseEntityType => ObjectType;

        public virtual NTSDEntityCategory EntityCategory => NTSDEntityCategory.Special;

        public virtual bool CountsAsRandomWeaponDropCandidate() => false;

        /// <summary>对象类型枚举入口。</summary>
        public LF2ObjectType Type => ObjectTypeEnum;



        /// <summary>物理状态。</summary>
        public PhysicsState PS { get; protected set; }

        /// <summary>当前帧信息。</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>DAT 帧数据缓存。</summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        /// <summary>帧转换器。</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>效果状态。</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>Sprite 资源引用。</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>渲染器引用。</summary>
        public LF2ObjectRenderer Renderer { get; protected set; }

        /// <summary>模拟世界引用。</summary>
        public SimulationWorld Match => SimulationTickDriver.Instance?.World;



        /// <summary>帧延迟计数器。</summary>
        public int FrameDelay
        {
            get => Runtime.FrameDelay;
            set => Runtime.FrameDelay = value;
        }

        /// <summary>C++ release Entity::attacking，帧等待/攻击状态计数器。</summary>
        public int AttackingCounter
        {
            get => Runtime.AttackingCounter;
            set => Runtime.AttackingCounter = value;
        }

        /// <summary>命中停帧/锁定计数。</summary>
        public int HitStun
        {
            get => Runtime.HitStop;
            set => Runtime.HitStop = value;
        }

        /// <summary>累计击退 X 速度。</summary>
        public float KnockbackVx
        {
            get => Runtime.KnockbackVx;
            set => Runtime.KnockbackVx = value;
        }

        /// <summary>累计击退 Y 速度。</summary>
        public float KnockbackVy
        {
            get => Runtime.KnockbackVy;
            set => Runtime.KnockbackVy = value;
        }

        /// <summary>累计击退 Z 速度。</summary>
        public float KnockbackVz
        {
            get => Runtime.KnockbackVz;
            set => Runtime.KnockbackVz = value;
        }

        /// <summary>震屏计时器。</summary>
        public int ShakeTimer
        {
            get => Runtime.ShakeTimer;
            set => Runtime.ShakeTimer = value;
        }

        /// <summary>攻击豁免计数器；角色类改用 HitCounters 存储。</summary>
        public virtual int AttackExempt
        {
            get => Runtime.AttackExempt;
            set => Runtime.AttackExempt = value;
        }

        public virtual int HitStateCount
        {
            get => Runtime.HitStateCount;
            set => Runtime.HitStateCount = value;
        }

        public virtual int HitConfirmCounter
        {
            get => Runtime.HitConfirmEa;
            set => Runtime.HitConfirmEa = value;
        }

        /// <summary>生成者实体索引，opoint 生成时写入。</summary>
        public int OwnerEntityIndex
        {
            get => Runtime.OwnerSlotIndex;
            set => Runtime.OwnerSlotIndex = value;
        }

        /// <summary>发射/生成计数。</summary>
        public int ShotCount
        {
            get => Runtime.ShotCount;
            set => Runtime.ShotCount = value;
        }

        /// <summary>C++ release ai_controlled 标记；角色生成后由输入准备阶段消费。</summary>
        public bool AiControlled
        {
            get => Runtime.AiControlled;
            set => Runtime.AiControlled = value;
        }

        /// <summary>itr 攻击冷却跟踪器。</summary>
        public virtual LF2ItrRestTracker ItrRest { get; protected set; } = null;

        /// <summary>生命和资源状态。</summary>
        public virtual LF2Health Health { get; protected set; } = null;

        /// <summary>HP 恢复计时器。</summary>
        public virtual int HealTimer
        {
            get => Runtime.HealTimer;
            set => Runtime.HealTimer = value;
        }

        public virtual int CatchTimer
        {
            get => Runtime.CatchTimer;
            set => Runtime.CatchTimer = value;
        }

        /// <summary>C++ release kill_count；-1 表示普通实体，&gt;=0 表示关联的生成者/归属槽。</summary>
        public int KillCount
        {
            get => Runtime.KillCount;
            set => Runtime.KillCount = value;
        }

        /// <summary>C++ release weapon_count；角色受笛子命中时可为负，武器侧用于飞行/笛子累计。</summary>
        public int WeaponCount
        {
            get => Runtime.WeaponCount;
            set => Runtime.WeaponCount = value;
        }

        /// <summary>C++ release fall_damage_div；落地持续扣血分支的伤害缩放除数。</summary>
        public int FallDamageDiv
        {
            get => Runtime.FallDamageDiv;
            set => Runtime.FallDamageDiv = value;
        }

        public int HitCount
        {
            get => Runtime.HitCount;
            set => Runtime.HitCount = value;
        }



        /// <summary>阴影 SpriteRenderer，由渲染器注入。</summary>
        public SpriteRenderer ShadowRenderer { get; private set; }

        /// <summary>注入阴影渲染器引用。</summary>
        public void SetShadowRenderer(SpriteRenderer sr) => ShadowRenderer = sr;

        /// <summary>更新阴影位置和显示状态。</summary>
        public void UpdateShadow(int renderFrame = 0)
        {
            if (ShadowRenderer == null || PS == null) return;

            int state = Frame?.D?.state ?? -1;
            int oid = ObjectId;
            bool hide = state == 3005
                     || state == 9997
                     || oid == 223
                     || oid == 224;

            ShadowRenderer.enabled = !hide;
            if (!hide)
            {
                var t = ShadowRenderer.transform;
                Vector2 groundPos = PhysicsState.ToUnityGroundPoint(PS.x, PS.z);
                t.position = new Vector3(groundPos.x, groundPos.y, t.position.z);
            }
        }



        /// <summary>当前活跃 spark 槽数量。</summary>
        public int SparkSlotCount { get; private set; } = 0;

        /// <summary>最大 spark 槽数量。</summary>
        public const int MaxSparkSlots = 10;

        private readonly int[] _sparkTimers = new int[MaxSparkSlots];
        private readonly float[] _sparkWorldX = new float[MaxSparkSlots];
        private readonly float[] _sparkWorldY = new float[MaxSparkSlots];
        private readonly float[] _sparkWorldZ = new float[MaxSparkSlots];

        /// <summary>命中时追加新的 Spark 记录。</summary>
        public void AddSparkSlot(int timerInitial, float worldX, float worldY, float worldZ, int currentRenderFrame = -1)
        {
            if (SparkSlotCount >= MaxSparkSlots) return;
            int slot = SparkSlotCount;
            _sparkTimers[slot] = timerInitial;
            _sparkWorldX[slot] = worldX;
            _sparkWorldY[slot] = worldY;
            _sparkWorldZ[slot] = worldZ;
            SparkSlotCount++;
        }

        /// <summary>读取指定 Spark 记录的年龄。</summary>
        public int GetSparkTimer(int slotIndex) => _sparkTimers[slotIndex];

        /// <summary>读取指定 Spark 记录的世界坐标。</summary>
        public Vector3 GetSparkWorldPos(int slotIndex)
            => new Vector3(_sparkWorldX[slotIndex], _sparkWorldY[slotIndex], _sparkWorldZ[slotIndex]);

        public void AdvanceSparkSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SparkSlotCount) return;
            _sparkTimers[slotIndex]++;
        }

        public bool RemoveSparkSlotIfTail(int slotIndex)
        {
            if (slotIndex != SparkSlotCount - 1) return false;
            RemoveSparkSlot(slotIndex);
            return true;
        }

        private void RemoveSparkSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SparkSlotCount) return;
            int tail = SparkSlotCount - 1;
            if (slotIndex < tail)
            {
                System.Array.Copy(_sparkTimers, slotIndex + 1, _sparkTimers, slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldX, slotIndex + 1, _sparkWorldX, slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldY, slotIndex + 1, _sparkWorldY, slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldZ, slotIndex + 1, _sparkWorldZ, slotIndex, tail - slotIndex);
            }
            SparkSlotCount--;
        }


        protected void ResetSpark() => SparkSlotCount = 0;



        /// <summary>Unity 保留的状态事件入口；具体行为以 C++ release 运行时为准。</summary>
        protected virtual bool StateExitEvent() => false;
        protected virtual bool StateEntryEvent() => false;
        protected virtual bool FrameEvent() => false;
        protected virtual bool TransitEvent() => false;
        protected virtual bool TUEvent() => false;
        protected virtual bool DieEvent() => false;
        protected virtual bool DestroyEvent() => false;

        /// <summary>获取当前状态。</summary>
        public virtual int GetState() => Frame.D?.state ?? 0;



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



        /// <summary>受到 itr kind=10/11 时的受力处理，角色和武器共用。</summary>
        public virtual void FluteForce()
        {
            if (PS == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);

            const float lowLevel = -140f;
            const float midLevel = -160f;
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
                    if (Frame.N >= 55) ImmediateFrame(40);
                    break;
                case LF2ObjectType.HeavyWeapon:
                    if (Frame.N >= 5) ImmediateFrame(1);
                    break;
            }
        }



        /// <summary>写入实体位置。</summary>
        public void SetPos(float x, float y, float z)
        {
            if (PS == null) return;
            PS.x = x; PS.y = y; PS.z = z;
        }

        /// <summary>创建武器破碎碎片特效。</summary>
        public virtual void BrokenEffectCreate(int id, int num = 8)
        {
            SpawnBrokenWeaponFragments(id);
        }

        protected void SpawnBrokenWeaponFragments(int sourceOid)
        {
            int count = BrokenWeaponFragmentCount(sourceOid);
            if (count <= 0 || PS == null) return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            for (int i = 0; i < count; i++)
            {
                int x = (int)PS.x + RandInt(0, 7) - 3;
                int y = (int)PS.y + RandInt(0, 7) - 3;
                float vx = RandInt(0, 11) - 5f;
                float vy = BrokenWeaponFragmentVy(sourceOid, i);
                int frame = BrokenWeaponFragmentFrame(sourceOid, i);

                var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 999,
                    kind = 0,
                    action = frame,
                    facing = PS.dir == "right" ? 0 : 1,
                    x = 0,
                    y = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0
                };
                task.parent = null;
                task.team = Team;
                task.pos = new Vector3(x, y, PS.z);
                task.z = PS.z;
                task.dir = PS.dir;
                task.useDirectVelocity = true;
                task.directVx = vx;
                task.directVy = vy;
                task.directVz = 0f;
                factory.EnqueueCreateObject(task);
            }
        }

        private static int BrokenWeaponFragmentCount(int oid)
        {
            if (oid == 101 || oid == 218) return 7;
            if (oid == 100 || oid == 213 || oid == 217) return 5;
            if (oid == 201 || oid == 120 || oid == 124) return 3;
            if (oid == 150) return 13;
            if (oid == 151) return 15;
            if (oid == 121) return 4;
            if (oid == 122 || oid == 123) return 9;
            return 0;
        }

        private float BrokenWeaponFragmentVy(int oid, int fragmentIndex)
        {
            if (oid == 150 || oid == 151 || oid == 213)
                return -(RandInt(0, 20) / 2f) - 8f;

            if (oid == 100 || oid == 101 || oid == 201 || oid == 120 || oid == 121 ||
                oid == 122 || oid == 123 || oid == 124 || oid == 217 || oid == 218)
            {
                if ((oid == 122 || oid == 123) && fragmentIndex >= 3)
                    return -(RandInt(0, 18) / 2f) - 4f;

                return -(RandInt(0, 8) / 2f) - 6f;
            }

            return 0f;
        }

        private int BrokenWeaponFragmentFrame(int oid, int fragmentIndex)
        {
            if (oid == 150) return RandInt(0, 4) + (fragmentIndex < 5 ? 0 : 4);
            if (oid == 100) return RandInt(0, 4) + (fragmentIndex < 2 ? 10 : 14);
            if (oid == 213) return RandInt(0, 4) + (fragmentIndex < 2 ? 150 : 154);
            if (oid == 101)
            {
                if (fragmentIndex < 5) return RandInt(0, 2) * 4 + RandInt(0, 4) + 20;
                return RandInt(0, 4) + 30;
            }
            if (oid == 151)
            {
                if (fragmentIndex < 2) return RandInt(0, 4) + 40;
                if (fragmentIndex < 5) return RandInt(0, 4) + 44;
                if (fragmentIndex < 8) return RandInt(0, 4) + 50;
                return RandInt(0, 4) + 54;
            }
            if (oid == 120) return RandInt(0, 4) + (fragmentIndex < 2 ? 54 : 30);
            if (oid == 124) return RandInt(0, 4) + 170;
            if (oid == 121) return RandInt(0, 4) + 60;
            if (oid == 122)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 70;
                if (fragmentIndex < 3) return RandInt(0, 4) + 80;
                return RandInt(0, 4) + 74;
            }
            if (oid == 123)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 160;
                if (fragmentIndex < 3) return RandInt(0, 4) + 164;
                return RandInt(0, 4) + 74;
            }
            if (oid == 217 || oid == 218) return RandInt(0, 4) + 174;
            return 0;
        }

        /// <summary>正式战斗随机数入口，对应 C++ release 的 ntsd_rand()。</summary>
        public int BattleRandInt(int minInclusive, int maxExclusive)
            => RandInt(minInclusive, maxExclusive);

        protected int RandInt(int minInclusive, int maxExclusive)
        {
            var rng = Match?.Rng;
            if (rng != null) return rng.NextInt(minInclusive, maxExclusive);
            return FallbackRng.NextInt(minInclusive, maxExclusive);
        }

        /// <summary>检查 itr arest 冷却是否允许攻击。</summary>
        public bool ItrArestTest() => ItrRest == null || ItrRest.Arest <= 0;

        /// <summary>命中后更新 arest 冷却。</summary>
        public void ItrArestUpdate(InteractionArea itr)
        {
            if (ItrRest == null) return;
            if (itr != null && itr.arest > 0)
                ItrRest.Arest = itr.arest;
            else if (itr == null || itr.vrest <= 0)
                ItrRest.Arest = NTSDGlobal.Default.Character.ARest;
        }

        /// <summary>检查指定攻击者的 vrest 冷却是否结束。</summary>
        public bool ItrVrestTest(int uid) => ItrRest == null || !ItrRest.HasVrest(uid);

        /// <summary>更新指定攻击者的 vrest 冷却。</summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            int vrest = (itr.arest > 0) ? itr.arest : itr.vrest;
            ItrRest.SetVrest(attackerUid, vrest);
        }

        /// <summary>更新击飞路径的 vrest 冷却，固定写 45。</summary>
        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            ItrRest.SetVrest(attackerUid, 45);
        }

        /// <summary>立即写入指定帧，绕过 wait 推进。</summary>
        public virtual void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;
            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            Frame.N = frameId;
            Frame.D = targetFrame;
            AttackingCounter = 0;

            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
        }

        /// <summary>每帧时间更新入口，各子类按需重写。</summary>
        public virtual void TUUpdate() { }

        /// <summary>按帧 ID 获取帧数据。</summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
            => FrameCache?.GetFrameDataById(frameId);

        /// <summary>请求跳转到指定帧。</summary>
        public virtual void TransitionToFrame(int frameId)
            => TransitionToFrame(frameId, 0);

        /// <summary>请求跳转到指定帧。</summary>
        public virtual void TransitionToFrame(int frameId, int wait = 0)
            => Trans?.Frame(frameId, wait);

        /// <summary>获取碰撞用 sprite 宽度，单位为像素。</summary>
        public virtual float GetSpriteWidthPxForCollision() => 0f;



        public abstract void Reset();
        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>从 SimulationWorld 注销自身。</summary>
        public virtual void UnregisterFromWorld()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
        }

        /// <summary>销毁当前对象的可视表现。</summary>
        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>FrameTransistor 检测到 next=1000 时调用，子类可实现销毁逻辑。</summary>
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

        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans, Trans?.WaitCounter ?? 0);
        }

        /// <summary>帧转换回调，子类实现具体帧切换逻辑。</summary>
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock) { }



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
        public virtual void SimObjectInteraction(int tickIndex) { }
        public virtual void SimPreInteraction(int tickIndex) { }
        public virtual void SimEntityCollision(int tickIndex) { }
        public virtual void SimFrameTick(int tickIndex) { }

        /// <summary>模拟后期更新，默认刷新渲染深度。</summary>
        public virtual void SimLateTick(int tickIndex)
        {
            if (PS != null) Sprite?.SetZ(PS.z + PS.zz);
        }

        public virtual void RunFrameLogicBeforeAdvance() { }

        internal virtual bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame) => false;

        internal virtual bool SupportsPostInteractionPhase() => false;

        internal virtual bool SupportsObjectInteractionPhase() => false;

        internal virtual bool UsesDynamicRuntimeSlot() => false;

        internal virtual bool IsStageBoundedCharacter() => false;

        internal virtual bool ShouldContributeToReleaseCamera() => false;

        internal virtual void ApplyPreFrameZBounds(float zMin, float zMax) { }

        internal virtual bool ApplyPreFrameXBounds(float stageWidth) => false;

        public virtual void RunStateSpecialPreCollision() { }

        internal virtual void RunPreCollisionRecoveryPhase(int tickIndex) { }

        internal virtual void RunPostCooldownInputPhase(int tickIndex) { }

        internal virtual void RunEarlyTeleportSpecialsPhase(System.Collections.Generic.List<LF2Entity> entities, bool frameToggleGate) { }

        internal virtual void RunLateDeathOpointPreCleanupPhase() { }

        internal virtual bool TryRunLatePostOpointCleanupPhase() => false;

        internal virtual void RunLateTailBeforePrevFrame() { }

        public virtual void MirrorLatePrevFrame()
        {
            if (Frame != null)
                Frame.Prev = Frame.N;
        }

        public virtual void FreeEntityLikeExe()
        {
            OnTransitDestroy();
        }

        public virtual void DirectWriteFramePreserveWaitCounter(int frameId)
        {
            SetFrameTickDirect(frameId);
        }

        public virtual int GetCurrentDataObjectTypeForSimulation() => ObjectType;

        public virtual void RunCpointCheckStep10() { }

        public virtual void RunCpointMismatchTailStep10() { }

        public virtual void RunWeaponSyncHeldStep10() { }

        public virtual void ClearHitCandidateCarriers() { }

        protected virtual void RunCpointActionSelectionStep10(CatchPoint cpoint, LF2Entity victimEntity) { }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity) { }

        protected virtual void SetVictimThrowVzStep10(CatchPoint cpoint, LF2Entity victim) { }

        protected virtual void ApplyCpointDirControlStep10(CatchPoint cpoint) { }

        protected virtual void ApplyCpointHeldInjuryStep10(LF2Entity victimEntity, int injury) { }

        protected virtual void SyncCpointHeldPositionStep10(LF2Entity victimEntity, LF2FrameData catcherFrame, CatchPoint catcherCpoint) { }

        public virtual void OnFrameTickFrameChangedFromWaitCounter() { }

        public virtual bool OnFrameTickBeforeWaitAdvance(int previousFrame) => true;

        public virtual void OnFrameTickTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans);
        }

        public virtual void OnFrameTickAfterWaitAdvance(int previousFrame, bool allowJumpInit) { }

        public virtual int ResolveFrameTickNext999Target(out bool allowJumpInit)
        {
            allowJumpInit = false;
            return 0;
        }

        protected virtual bool ApplyObjectSpecificFrameTickBeforeWaitAdvance() => true;

        protected virtual void ApplyCommonCaughtExitHitStop() { }

        protected virtual bool IsFrameTickLeftPressed() => false;

        protected virtual bool IsFrameTickRightPressed() => false;

        protected virtual void ApplyFrame212JumpInit() { }

        protected bool TryEnterReleaseFrameAdvanceAfterDelay()
        {
            if (FrameDelay > 0)
            {
                FrameDelay--;
                return false;
            }

            if (FrameDelay < 0)
            {
                FrameDelay++;
                return false;
            }

            return true;
        }

        protected void SetFrameTickDirect(int frameId)
        {
            if (Frame == null || FrameCache == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return;

            Frame.N = frameId;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
        }

        protected virtual void RunCommonFrameTick()
        {
            Trans?.Trans();
        }



        /// <summary>分配稳定 ID。</summary>
        protected void AllocateStableId()
        {
            StableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;
            Runtime.StableId = StableId;
        }

        /// <summary>重置稳定 ID。</summary>
        protected void ResetStableId()
        {
            StableId = 0;
            Runtime.StableId = 0;
        }

        /// <summary>写入运行时槽位索引。</summary>
        public void SetRuntimeSlotIndex(int slotIndex)
        {
            Runtime.SlotIndex = slotIndex;
        }

        /// <summary>刷新运行时字段镜像。</summary>
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
            Runtime.OwnerSlotIndex = OwnerEntityIndex;
            Runtime.OwnerStableId = OwnerId;
            Runtime.GrabbedBy = GrabbedBy;
            Runtime.TrackerFlag = TrackerFlag;
            Runtime.Frame = Frame?.N ?? 0;
            Runtime.WaitCounter = Trans?.WaitCounter ?? 0;
            Runtime.NextFrame = Trans?.Next ?? 0;
            Runtime.AttackingCounter = AttackingCounter;
            Runtime.FrameDelay = FrameDelay;
            Runtime.HitStop = HitStun;
            Runtime.AttackExempt = AttackExempt;
            Runtime.HealTimer = HealTimer;
            Runtime.KillCount = KillCount;
            Runtime.ShotCount = ShotCount;

            if (PS != null)
            {
                Runtime.X = PS.x;
                Runtime.Y = PS.y;
                Runtime.Z = PS.z;
                Runtime.Vx = PS.vx;
                Runtime.Vy = PS.vy;
                Runtime.Vz = PS.vz;
                Runtime.SpriteX = PS.sx;
                Runtime.SpriteY = PS.sy;
                Runtime.SpriteZ = PS.sz;
            }

            if (Health != null)
            {
                Runtime.HP = Health.HP;
                Runtime.MP = Health.MP;
                Runtime.PP = Health.PP;
                Runtime.PPMax = Health.MaxPP;
                Runtime.PPBound = Health.PPBound;
                Runtime.HPLost = Health.HPLost;
                Runtime.HPBound = Health.HPBound;
                Runtime.MPMax = Health.MaxMP;
            }
        }

    }
}
