using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Input;
using NTSD.Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 所有战斗实体的最底层公共基类。
    /// 
    /// 你可以把它理解成“所有战斗对象共享的骨架”：
    /// 1. 统一持有 Runtime、Frame、Effect、Renderer 等核心数据。
    /// 2. 定义所有实体都可能会参与的生命周期入口。
    /// 3. 让角色、武器、技能体、特效体可以共享同一套基础框架。
    /// 
    /// 简单理解项目分层：
    /// - LF2Entity：最底层实体框架
    /// - LF2LivingObject：更像战斗单位的公共能力
    /// - LF2Character / LF2WeaponBase / LF2SpecialAttack：具体对象类型
    /// </summary>
    public abstract class LF2Entity : ILF2Entity
    {
        public const int OverlaySortingOrderOffset = 10000;
        protected static readonly List<LF2Entity> N30HistoryGateScratch = new List<LF2Entity>(32);
        private readonly NTSDInputStateModule sharedCharacterDatInputModule = new NTSDInputStateModule();
        internal static System.Func<int, LF2CharacterDataWrapper> RuntimeCharacterConfigResolverOverride;


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

        /// <summary>
        /// 逻辑真值运行时。
        /// 大部分真正参与战斗结算的位置、速度、状态字段，最终都应该落在这里。
        /// </summary>
        public NTSDEntityRuntime Runtime { get; } = new NTSDEntityRuntime();

        public PhysicsState PS { get; protected set; } = new PhysicsState();

        private static readonly DeterministicRng FallbackRng = new DeterministicRng(0x4E545344u);

        /// <summary>C++ release 实体类型值。</summary>
        public virtual int ReleaseEntityType => ObjectType;

        public virtual bool CountsAsRandomWeaponDropCandidate() => false;

        /// <summary>当前对象正在执行哪一帧逻辑，以及上一帧/碰撞快照帧等辅助信息。</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>当前对象对应的 DAT 帧数据缓存。</summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        /// <summary>帧切换控制器。负责 wait/next/frame jump 等帧推进细节。</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>效果状态。</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>Sprite 资源引用。</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>渲染器引用。</summary>
        public LF2ObjectRenderer Renderer { get; protected set; }

        /// <summary>当前实体所在的战斗世界。大多数情况下通过单例 Driver 反查。</summary>
        private SimulationWorld registeredWorld;

        public SimulationWorld Match => registeredWorld ?? SimulationTickDriver.Instance?.World;



        /// <summary>帧延迟计数器。大于 0 或小于 0 时，都会影响本帧是否真正推进。</summary>
        public int FrameDelay
        {
            get => Runtime.FrameDelay;
            set => Runtime.FrameDelay = value;
        }

        /// <summary>投掷后的同帧保护帧号，命中当前 frame 时直接跳过 frame advance / frame tick。</summary>
        public int ThrowFrameGuard
        {
            get => Runtime.ThrowFrameGuard;
            set => Runtime.ThrowFrameGuard = value;
        }

        /// <summary>C++ release Entity::attacking，帧等待/攻击状态计数器。</summary>
        public int AttackingCounter
        {
            get => Runtime.AttackingCounter;
            set => Runtime.AttackingCounter = value;
        }

        /// <summary>命中停帧/锁定计数。可以理解成“这一小段时间内对象被短暂停住”。</summary>
        public int HitStun
        {
            get => Runtime.HitStop;
            set => Runtime.HitStop = value;
        }

        /// <summary>累计击退 X 速度。</summary>
        public double KnockbackVx
        {
            get => Runtime.KnockbackVx;
            set => Runtime.KnockbackVx = value;
        }

        /// <summary>累计击退 Y 速度。</summary>
        public double KnockbackVy
        {
            get => Runtime.KnockbackVy;
            set => Runtime.KnockbackVy = value;
        }

        /// <summary>累计击退 Z 速度。</summary>
        public double KnockbackVz
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

        /// <summary>C++ release combo_count_vic；累计承受的连击伤害统计。</summary>
        public int ComboCountVic
        {
            get => Runtime.ComboCountVic;
            set => Runtime.ComboCountVic = value;
        }

        /// <summary>C++ release combo_count_atk；累计造成的连击伤害统计。</summary>
        public int ComboCountAtk
        {
            get => Runtime.ComboCountAtk;
            set => Runtime.ComboCountAtk = value;
        }

        /// <summary>C++ release kill_stat；击杀统计。</summary>
        public int KillStat
        {
            get => Runtime.KillStat;
            set => Runtime.KillStat = value;
        }

        /// <summary>C# authority Entity.Unk344；索引 1..2 指向全局击杀/伤害统计槽。</summary>
        public int Unk344
        {
            get => Runtime.Unk344;
            set => Runtime.Unk344 = value;
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

        /// <summary>C++ release 原始 HP 备份字段。</summary>
        public int HPOrig
        {
            get => Runtime.HPOrig;
            set => Runtime.HPOrig = value;
        }

        /// <summary>C++ release 原始 HP2/残机备份字段。</summary>
        public int HP2Orig
        {
            get => Runtime.HP2Orig;
            set => Runtime.HP2Orig = value;
        }

        /// <summary>C++ release 复活血量配置字段；0 表示走普通复活次数路径。</summary>
        public int RespawnCount
        {
            get => Runtime.RespawnCount;
            set => Runtime.RespawnCount = value;
        }

        /// <summary>C# 基线 presentation `PpDisplay`；输入扣费与帧推进回退维护的 PP 表现层累计面。</summary>
        public int PpDisplay
        {
            get => Runtime.PpDisplay;
            set => Runtime.PpDisplay = value;
        }

        protected bool IsPpModeEnabled()
        {
            return Match?.PpMode ?? NTSDGlobal.MPEnabled;
        }

        public int HitCount
        {
            get => Runtime.HitCount;
            set => Runtime.HitCount = value;
        }

        public int HitConfirm2
        {
            get => Runtime.HitConfirm2;
            set => Runtime.HitConfirm2 = value;
        }

        public virtual int FallCounter
        {
            get => Runtime.Fall;
            set => Runtime.Fall = value;
        }

        public int TransformOriginalObjectId
        {
            get => Runtime.TransformOriginalObjectId;
            set => Runtime.TransformOriginalObjectId = value;
        }

        public int TransformTargetObjectId
        {
            get => Runtime.TransformTargetObjectId;
            set => Runtime.TransformTargetObjectId = value;
        }

        public int CaughtSlotIndex
        {
            get => Runtime.CaughtSlotIndex;
            set => Runtime.CaughtSlotIndex = value;
        }

        public int CatcherSlotIndex
        {
            get => Runtime.CatcherSlotIndex;
            set => Runtime.CatcherSlotIndex = value;
        }

        public int HolderCopySlot
        {
            get => Runtime.HolderCopySlotIndex;
            set => Runtime.HolderCopySlotIndex = value;
        }

        public int RelationOwnerSlot
        {
            get => Runtime.RelationOwnerSlotIndex;
            set => Runtime.RelationOwnerSlotIndex = value;
        }

        public int SpawnerEntityIndex
        {
            get => Runtime.SpawnerSlotIndex;
            set => Runtime.SpawnerSlotIndex = value;
        }

        private bool _hasForcedRuntimeIntPosition;



        /// <summary>阴影 SpriteRenderer，由渲染器注入。</summary>
        public SpriteRenderer ShadowRenderer { get; private set; }

        /// <summary>注入阴影渲染器引用。</summary>
        public void SetShadowRenderer(SpriteRenderer sr) => ShadowRenderer = sr;

        /// <summary>更新阴影位置和显示状态。</summary>
        public void UpdateShadow(int renderFrame = 0)
        {
            if (ShadowRenderer == null || Runtime == null) return;

            int state = Frame?.D?.state ?? -1;
            int oid = ObjectId;
            bool hide = state == 3005
                     || state == 9997
                     || (Runtime?.LinkState ?? 0) < 0
                     || oid == 223
                     || oid == 224;

            ShadowRenderer.enabled = !hide;
            if (!hide)
            {
                var t = ShadowRenderer.transform;
                Sprite shadowSprite = ShadowRenderer.sprite;
                float shadowWidth = shadowSprite != null ? shadowSprite.rect.width : 0f;
                float shadowHeight = shadowSprite != null ? shadowSprite.rect.height : 0f;

                // C# 基准工程先计算阴影绘制矩形：
                // left = x + renderOffsetX - cameraX - shadowW / 2
                // top  = z - shadowH / 2
                // Unity Sprite 默认中心 pivot，这里把矩形换算回中心点。
                int cameraX = Match?.ReleaseCameraX ?? 0;
                float shadowLeft = GetRuntimeXInt() + GetRenderOffsetX() - cameraX - shadowWidth * 0.5f;
                float shadowTop = GetRenderZInt() - shadowHeight * 0.5f;
                float shadowCenterX = shadowLeft + shadowWidth * 0.5f;
                float shadowCenterY = shadowTop + shadowHeight * 0.5f;
                Vector3 worldPos = NTSDRenderSpace.ScreenPixelToWorld(shadowCenterX, shadowCenterY, t.position.z);
                t.position = NTSDRenderSpace.SnapWorldPosition(worldPos);
            }
        }



        /// <summary>命中记录数量，对齐 C# 基线 Entity.HitRecordCount。</summary>
        public int HitRecordCount { get; private set; } = 0;

        /// <summary>最大命中记录数量，对齐 C# 基线的 10 槽。</summary>
        public const int MaxHitRecordSlots = 10;

        private readonly int[] _hitRecordDamage = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordX = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordZ = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordLastAdvanceTick = new int[MaxHitRecordSlots];

        /// <summary>追加一条命中记录，供 SparkRenderer 按 C# 基线渲染。</summary>
        public void AddHitRecord(int age, int anchorX, int anchorZ)
        {
            if (HitRecordCount >= MaxHitRecordSlots)
                return;

            int slot = HitRecordCount++;
            _hitRecordDamage[slot] = age;
            _hitRecordX[slot] = anchorX;
            _hitRecordZ[slot] = anchorZ;
            _hitRecordLastAdvanceTick[slot] = int.MinValue;
        }

        /// <summary>记录一次 kind 0 命中；由受击对象调用。</summary>
        internal void RecordKind0Hit(LF2Entity attacker, InteractionArea itr)
        {
            if (attacker == null || itr == null)
                return;

            int attackerZ = attacker.Runtime.ZInt;
            int victimZ = Runtime.ZInt;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int victimSlot = Runtime.SlotIndex;
            LF2Entity recordOwner = attackerZ > victimZ ||
                                    (attackerZ == victimZ && attackerSlot > victimSlot)
                ? attacker
                : this;

            if (recordOwner.HitRecordCount >= MaxHitRecordSlots)
                return;

            int sparkPhase = itr.effect == 1 ? 1 : 0;
            int timer = itr.fall > 60 ? sparkPhase * 20 : sparkPhase * 20 + 10;
            LF2FrameData attackerFrame = attacker.GetFrameDataById(attacker.Frame?.N ?? 0) ?? attacker.Frame?.D;
            int attackerCenterX = attackerFrame?.centerx ?? 0;
            int attackerCenterY = attackerFrame?.centery ?? 0;
            int attackerX = attacker.Runtime.XInt;
            int attackerY = attacker.Runtime.YInt;
            int victimX = Runtime.XInt;
            int victimY = Runtime.YInt;

            int hitX;
            if (attacker.Dirh() > 0)
            {
                hitX = attackerX - attackerCenterX + itr.x + itr.w;
                if (hitX > victimX)
                    hitX = victimX;
            }
            else
            {
                hitX = attackerX + attackerCenterX - itr.x - itr.w;
                if (hitX < victimX)
                    hitX = victimX;
            }

            int hitYOffset = attackerY + (itr.h / 2) + itr.y - attackerCenterY;
            int lowerY = victimY - attackerCenterY;
            if (hitYOffset < lowerY)
                hitYOffset = (lowerY + hitYOffset) >> 1;
            else if (hitYOffset > victimY)
                hitYOffset = (victimY + hitYOffset) >> 1;

            int hitZ = attackerZ + hitYOffset + BattleRandInt(0, 9) - 4;
            hitX += BattleRandInt(0, 9) - 4;
            recordOwner.AddHitRecord(timer, hitX, hitZ);
        }

        /// <summary>读取指定命中记录年龄。</summary>
        public int GetHitRecordAge(int slotIndex) => _hitRecordDamage[slotIndex];

        /// <summary>读取指定命中记录 X 锚点。</summary>
        public int GetHitRecordX(int slotIndex) => _hitRecordX[slotIndex];

        /// <summary>读取指定命中记录 Z 锚点。</summary>
        public int GetHitRecordZ(int slotIndex) => _hitRecordZ[slotIndex];

        /// <summary>命中记录成功渲染后推进年龄。</summary>
        public void AdvanceHitRecord(int slotIndex, int tickIndex)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount)
                return;

            if (_hitRecordLastAdvanceTick[slotIndex] == tickIndex)
                return;

            _hitRecordDamage[slotIndex]++;
            _hitRecordLastAdvanceTick[slotIndex] = tickIndex;
        }

        /// <summary>仅当该记录位于尾槽时移除，对齐 C# 基线尾槽回收规则。</summary>
        public bool RemoveHitRecordIfTail(int slotIndex)
        {
            if (slotIndex != HitRecordCount - 1)
                return false;

            RemoveHitRecord(slotIndex);
            return true;
        }

        private void RemoveHitRecord(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount)
                return;

            int tail = HitRecordCount - 1;
            if (slotIndex < tail)
            {
                System.Array.Copy(_hitRecordDamage, slotIndex + 1, _hitRecordDamage, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordX, slotIndex + 1, _hitRecordX, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordZ, slotIndex + 1, _hitRecordZ, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordLastAdvanceTick, slotIndex + 1, _hitRecordLastAdvanceTick, slotIndex, tail - slotIndex);
            }

            _hitRecordDamage[tail] = 0;
            _hitRecordX[tail] = 0;
            _hitRecordZ[tail] = 0;
            _hitRecordLastAdvanceTick[tail] = 0;
            HitRecordCount--;
        }

        protected void ResetSpark()
        {
            HitRecordCount = 0;
            System.Array.Clear(_hitRecordDamage, 0, _hitRecordDamage.Length);
            System.Array.Clear(_hitRecordX, 0, _hitRecordX.Length);
            System.Array.Clear(_hitRecordZ, 0, _hitRecordZ.Length);
            System.Array.Clear(_hitRecordLastAdvanceTick, 0, _hitRecordLastAdvanceTick.Length);
        }



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
            string nextDir = dir == "left" ? "left" : "right";
            if (Runtime.Dir == nextDir)
                return;

            Runtime.Dir = nextDir;
            Sprite?.SwitchLR(nextDir);
        }

        public virtual int Dirh() => Runtime.Dir == "left" ? -1 : 1;

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
            if (Runtime == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);

            float lowLevel = -140f;
            float midLevel = -160f;
            float highLevel = -180f;

            Effect.Super = true;
            Runtime.Vx = 0;
            Runtime.Vz = 0;

            if (Runtime.Y > lowLevel)
                Runtime.Vy = (Runtime.Vy <= 0) ? -7.5f : -Runtime.Vy / 2f;
            else if (Runtime.Y <= lowLevel && Runtime.Y > midLevel)
                Runtime.Vy -= mass / 2f;
            else if (Runtime.Y <= midLevel && Runtime.Y > highLevel)
                Runtime.Vy += mass / 2f;

            switch ((LF2ObjectType)GetCurrentDataObjectType())
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
            Runtime.SetPosition(x, y, z);
        }

        /// <summary>创建武器破碎碎片特效。</summary>
        public virtual void BrokenEffectCreate(int id, int num = 8)
        {
            SpawnBrokenWeaponFragments(id);
        }

        protected void SpawnBrokenWeaponFragments(int sourceOid)
        {
            int count = BrokenWeaponFragmentCount(sourceOid);
            if (count <= 0 || Runtime == null) return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            for (int i = 0; i < count; i++)
            {
                int x = (int)Runtime.X + RandInt(0, 7) - 3;
                int y = (int)Runtime.Y + RandInt(0, 7) - 3;
                float vx = RandInt(0, 11) - 5f;
                float vy = BrokenWeaponFragmentVy(sourceOid, i);
                int frame = BrokenWeaponFragmentFrame(sourceOid, i);

                var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 999,
                    kind = 0,
                    action = frame,
                    facing = Runtime.Dir == "right" ? 0 : 1,
                    x = 0,
                    y = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0
                };
                task.parent = null;
                task.team = Team;
                task.pos = new Vector3(x, y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = Runtime.Dir;
                task.useDirectVelocity = true;
                task.directVx = vx;
                task.directVy = vy;
                task.directVz = 0f;
                task.releaseSpawnSemantic = LF2Tasks.ReleaseSpawnSemantic.BrokenFragment;
                factory.EnqueueCreateObject(task);
            }
        }

        private int BrokenWeaponFragmentCount(int oid)
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

        internal static int ResolveArestCooldown(int arest, int vrest)
        {
            return arest < 4 && vrest == 0 ? 4 : arest;
        }

        /// <summary>命中后更新 arest 冷却。</summary>
        public void ItrArestUpdate(InteractionArea itr)
        {
            if (ItrRest == null) return;
            if (itr == null || SuppressesGenericArest(itr.kind)) return;

            ItrRest.Arest = ResolveArestCooldown(itr.arest, itr.vrest);
        }

        /// <summary>检查指定攻击者的 vrest 冷却是否结束。</summary>
        public bool ItrVrestTest(int uid) => ItrRest == null || !ItrRest.HasVrest(uid);

        /// <summary>更新指定攻击者的 vrest 冷却。</summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            if (SuppressesGenericVrest(itr.kind)) return;
            if (itr.vrest > 0)
                ItrRest.SetVrest(attackerUid, itr.vrest);
        }

        /// <summary>更新击飞路径的 vrest 冷却，固定写 45。</summary>
        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr)
        {
            ItrVrestUpdate(attackerUid, itr);
        }

        private static bool SuppressesGenericArest(int kind)
        {
            return kind == 8 || kind == 10 || kind == 11 || kind == 14 || kind == 15 || kind == 16;
        }

        private static bool SuppressesGenericVrest(int kind)
        {
            return kind == 8 || kind == 10 || kind == 11 || kind == 14 || kind == 15;
        }

        public bool ItrVrestTest(int uid, bool releaseRuntimeSlot) => ItrVrestTest(uid);

        public void ItrVrestUpdate(int attackerUid, InteractionArea itr, bool releaseRuntimeSlot)
            => ItrVrestUpdate(attackerUid, itr);

        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr, bool releaseRuntimeSlot)
            => ItrVrestUpdateKnockdown(attackerUid, itr);

        protected bool TryApplyKind6HitConfirm(InteractionArea itr, LF2Entity target)
        {
            if (itr?.kind != 6 || target == null || target == this)
                return false;
            if (target.Runtime == null || target.Frame?.D == null)
                return false;
            if (target.Health != null && target.Health.HP <= 0)
                return false;
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr))
                return false;
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(this, target))
                return false;
            if (!BruteForceSceneQuery.RuntimeConsumeItrAllowed(this, itr, target))
                return false;

            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true))
                return false;

            target.HitConfirmCounter = 3;
            return true;
        }

        internal bool TryApplyKind6HitConfirmForCharacterDatInteraction(InteractionArea itr, LF2Entity target)
            => TryApplyKind6HitConfirm(itr, target);

        protected void ApplyKind14DirectionalBlockFrom(LF2Entity attacker)
        {
            if (attacker?.Runtime == null || Runtime == null)
                return;

            double attackerX = attacker.Runtime.X;
            double attackerZ = attacker.Runtime.Z;
            double victimX = Runtime.X;
            double victimZ = Runtime.Z;

            if (attackerX > victimX + 5f && (Runtime.Vx > 0f || KnockbackVx > 0f))
                Runtime.XBoundPositive = true;
            else if (attackerX < victimX - 5f && (Runtime.Vx < 0f || KnockbackVx < 0f))
                Runtime.XBoundNegative = true;

            if (attackerZ > victimZ + 2f && (Runtime.Vz > 0f || KnockbackVz > 0f))
                Runtime.ZBoundPositive = true;
            else if (attackerZ < victimZ - 2f && (Runtime.Vz < 0f || KnockbackVz < 0f))
                Runtime.ZBoundNegative = true;
        }

        /// <summary>立即写入指定帧，绕过 wait 推进。</summary>
        // 这是最直接的硬切帧入口：
        // 当前帧会立刻变成目标帧，不等待 FrameTransistor 下一拍再处理。
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

        /// <summary>按帧 ID 获取帧数据。</summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
            => FrameCache?.GetFrameDataById(frameId);

        /// <summary>请求跳转到指定帧。</summary>
        // 对外的标准跳帧入口，默认 wait=0。
        public virtual void TransitionToFrame(int frameId)
            => TransitionToFrame(frameId, 0);

        /// <summary>请求跳转到指定帧。</summary>
        // 和 ImmediateFrame 的区别在于：这里是把请求交给 FrameTransistor，
        // 让它按正式 frame_tick 顺序在后续推进里消费。
        public virtual void TransitionToFrame(int frameId, int wait = 0)
        {
            if (Trans == null)
                return;

            Trans.SetNext(frameId);
            Trans.SetWait(wait);
        }

        /// <summary>获取碰撞用 sprite 宽度，单位为像素。</summary>
        public virtual float GetSpriteWidthPxForCollision() => 0f;



        public abstract void Reset();
        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>从 SimulationWorld 注销自身。</summary>
        public virtual void UnregisterFromWorld()
        {
            Match?.Unregister(this);
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
            else
            {
                UnregisterFromWorld();
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        // FrameTransistor 真正执行换帧时，会先走到这里。
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans, Trans?.WaitCounter ?? 0);
        }

        /// <summary>帧转换回调，子类实现具体帧切换逻辑。</summary>
        // 需要额外参考 oldLock 或保留更细对齐语义时，子类实现这个重载。
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock) { }



        public int SimOrder => SimOrderConstants.GetSimOrderByObjectType(ObjectTypeEnum);

        public virtual void OnAdded(SimContext ctx)
        {
            registeredWorld = ctx?.World;
            RefreshRuntimeSnapshot();
        }

        public virtual void OnRemoved(SimContext ctx)
        {
            if (ReferenceEquals(registeredWorld, ctx?.World))
                registeredWorld = null;
            Runtime.SlotIndex = -1;
        }

        public virtual void SimTransit(int tickIndex) { }
        public virtual void SimTU(int tickIndex) { }
        public virtual void SimPostInteraction(int tickIndex)
        {
            LF2CharacterDatInteractionResolver.TryConsumeUnifiedStep7CandidateSequence(this);
        }
        public virtual void SimObjectInteraction(int tickIndex) { }
        public virtual void SimPreInteraction(int tickIndex) { }
        public virtual void SimEntityCollision(int tickIndex) { }
        public virtual void SimFrameTick(int tickIndex) { }

        /// <summary>模拟后期更新，默认刷新渲染深度。</summary>
        public virtual void SimLateTick(int tickIndex)
        {
            Sprite?.SetZ(GetRenderSortingOrder());
        }

        public virtual void RunFrameLogicBeforeAdvance() { }

        internal virtual bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame) => false;

        internal virtual bool SupportsPostInteractionPhase()
            => LF2CharacterDatInteractionResolver.CanResolveAttacker(this);

        internal virtual bool SupportsObjectInteractionPhase() => false;

        internal virtual bool UsesDynamicRuntimeSlot() => false;

        internal virtual bool IsStageBoundedCharacter() => false;

        internal virtual bool ShouldContributeToReleaseCamera() => false;

        internal virtual void ApplyPreFrameZBounds(float zMin, float zMax) { }

        internal virtual bool ApplyPreFrameXBounds(float stageWidth) => false;

        /// <summary>
        /// pre-collision 阶段的公共 state 特判。
        /// 对齐参考 C# `RunStateSpecialPreCollision`：
        /// - state 4000..4999：切换到 `state - 4000` 对应对象并进入 frame 0
        /// - state 8000..8999：切换到 `state - 8000` 对应对象并进入 frame 0，同时写入 140 hit stop
        /// 
        /// 这里仍然保持 Unity 当前架构边界：
        /// 只切换 `ObjectId + FrameCache`，不在这里改运行时 C# 实例类型。
        /// </summary>
        public virtual void RunStateSpecialPreCollision()
        {
            LF2FrameData frameData = Frame?.D;
            if (frameData == null)
                return;

            int state = frameData.state;
            if (state == 9995 && GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
            {
                ApplyStateDataTransform(50, false);
                return;
            }

            if (state >= 4000 && state < 5000)
            {
                ApplyStateDataTransform(state - 4000, false);
                return;
            }

            if (state >= 8000 && state < 9000)
                ApplyStateDataTransform(state - 8000, true);
        }

        internal virtual void RunPreCollisionRecoveryPhase(int tickIndex) { }

        /// <summary>
        /// 冷却递减后的输入消费阶段。
        /// 参考 C# 基准工程这里按当前 DAT `ObjType == 0` 分发角色输入；
        /// Unity 当前由 `LF2Character` 覆盖完整角色输入链；
        /// 对于“当前 DAT 已是 Character，但 CLR 运行时实例不是 LF2Character”的实体，
        /// 这里至少要补齐共享输入快照、基础 combo/direct frame jump，
        /// 以及不依赖完整角色 resolver 的 standing/walking 三个基础动作入口。
        /// </summary>
        internal virtual void RunPostCooldownInputPhase(int tickIndex)
        {
            if (Runtime == null || Runtime.LinkState < 0)
                return;

            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            if (AiControlled)
            {
                Match?.PrepareAiInputBasic(this, tickIndex);
            }
            else
            {
                UpdateSharedRuntimeInputSnapshotForSimulation(tickIndex);
            }

            if (this is LF2Character)
                return;

            RunSharedCharacterDatFrameJumpInputPhase();
            RunSharedCharacterDatStandingActionInputPhase();
        }

        internal virtual void RunCharacterInputPhase(int tickIndex)
        {
            RunPostCooldownInputPhase(tickIndex);
        }

        protected bool UsesSharedCharacterDatShellRouting()
        {
            return Runtime != null &&
                   this is not LF2Character &&
                   GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        /// <summary>
        /// 按当前运行时壳类型解析共享输入控制器。
        /// 这里不要求一定是 `LF2Character`，因为 transform 后的 current DAT character
        /// 仍然可能挂在 `LF2OtherObject` / `LF2SpecialAttack` / `LF2WeaponBase` 壳上。
        /// </summary>
        internal bool TryGetSharedInputControllerForSimulation(out ILF2Controller controller)
        {
            controller = null;

            if (this is LF2LivingObject living)
                controller = living.Controller;
            else if (this is LF2WeaponBase weapon)
                controller = weapon.Controller;

            return controller?.InputBuffer != null;
        }

        internal virtual void EnsureSharedCharacterDatControllerForSimulation()
        {
        }

        /// <summary>
        /// 把共享 controller 的输入缓冲滚入运行时输入快照。
        /// 结果菜单、battle-entry 清输入后的重新采样、post-cooldown 输入消费都可以复用这条入口。
        /// </summary>
        internal void UpdateSharedRuntimeInputSnapshotForSimulation(int tickIndex)
        {
            Runtime.RollInputFromCurrent();
            Runtime.TickInputCooldowns();

            if (!TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
                return;

            UpdateSharedRuntimeInputSnapshotFromBuffer(controller.InputBuffer, tickIndex);
        }

        private void UpdateSharedRuntimeInputSnapshotFromBuffer(SimInputBuffer inputBuffer, int tickIndex)
        {
            if (inputBuffer == null || !inputBuffer.TryDequeueAll(tickIndex, out System.Collections.Generic.List<SimInputEvent> events))
                return;

            for (int i = 0; i < events.Count; i++)
                ApplySharedRuntimeInputEvent(events[i].key, events[i].down);
        }

        private void RunSharedCharacterDatFrameJumpInputPhase()
        {
            if (Runtime == null)
                return;

            sharedCharacterDatInputModule.SyncFromRuntime(Runtime);
            sharedCharacterDatInputModule.ApplyFrameInput(this);
        }

        /// <summary>
        /// shared character-DAT 的最小 standing/walking 动作桥。
        /// 这里只补不依赖 `LF2CharacterActionResolver` 的基础 walk-run/attack/jump/defend 入口，
        /// 不扩到 running/dash/catching/held-weapon/release 全动作解析。
        /// </summary>
        private void RunSharedCharacterDatStandingActionInputPhase()
        {
            if (Runtime == null || this is LF2Character)
                return;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            ApplySharedCharacterDatSpecialStateLaneControl();

            if (TryRunSharedCharacterDatJumpAttackInputPhase())
                return;
            if (TryRunSharedCharacterDatCrouchInputPhase())
                return;
            if (TryRunSharedCharacterDatDefensiveRecoveryInputPhase())
                return;
            if (TryRunSharedCharacterDatRunningInputPhase())
                return;
            if (TryRunSharedCharacterDatDashAttackInputPhase())
                return;

            if ((Frame?.N ?? -1) == LF2StandardFrames.Defend)
            {
                // 参考 C# `ApplyCharacterInput(...)`：
                // frame 110 会先按左右输入刷新 facing，然后再继续走 standing-like 输入消费。
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("right");
                else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                    SwitchDir("left");
            }

            int state = Frame?.D?.state ?? -1;
            if (state != LF2States.Standing && state != LF2States.Walking)
                return;

            if (TryRunSharedCharacterDatHeavyWalkInputPhase())
                return;

            ApplySharedCharacterDatWalkRunMovement();

            if (TryRunSharedCharacterDatStandingAttackAction())
                return;
            if (TryRunSharedCharacterDatStandingJumpAction())
                return;

            TryRunSharedCharacterDatStandingDefendAction();
        }

        private bool TryRunSharedCharacterDatStandingAttackAction()
        {
            if (!IsSharedCharacterDatAttackInputReadyInternal())
                return false;

            int linkState = Runtime?.LinkState ?? 0;
            if (HitConfirmCounter > 0 &&
                linkState == 0 &&
                FrameCache?.HasFrame(LF2StandardFrames.SuperPunch) == true &&
                TryCharacterDatInputFrameJump(LF2StandardFrames.SuperPunch))
            {
                HitConfirmCounter = 0;
                return true;
            }

            if (linkState == 0)
            {
                bool usePunch = BattleRandInt(0, 2) == 0;
                int primary = usePunch ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                int fallback = usePunch ? LF2StandardFrames.Punch4 : LF2StandardFrames.Punch;
                return TryRunSharedCharacterDatStandingActionFrame(primary, fallback);
            }

            if (linkState == 101)
            {
                int primary = HasAnyDirectionInputForSharedCharacterDat()
                    ? LF2StandardFrames.LightWeaponThw
                    : RandomSharedCharacterDatWeaponAttackFrame();
                int fallback = primary == LF2StandardFrames.LightWeaponThw
                    ? 0
                    : LF2StandardFrames.LightWeaponThw;
                return TryRunSharedCharacterDatStandingActionFrame(primary, fallback);
            }

            if (linkState == 2)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.HeavyWeaponThw);

            if (linkState % 100 == 1)
                return TryRunSharedCharacterDatStandingActionFrame(RandomSharedCharacterDatWeaponAttackFrame());

            if (linkState == 4)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.LightWeaponThw);

            if (linkState == 6)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.SkyLgtWpThw);

            return false;
        }

        private bool TryRunSharedCharacterDatStandingJumpAction()
        {
            if (!IsSharedCharacterDatJumpInputReadyInternal())
                return false;

            return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.Jumping);
        }

        private bool TryRunSharedCharacterDatStandingDefendAction()
        {
            if (!IsSharedCharacterDatDefendInputReadyInternal(requireDefendLockOpen: true))
                return false;

            return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.Defend);
        }

        private bool TryRunSharedCharacterDatHeavyWalkInputPhase()
        {
            if (Runtime == null)
                return false;

            int state = Frame?.D?.state ?? -1;
            if (Runtime.LinkState != 2 || (state != LF2States.Standing && state != LF2States.Walking))
                return false;

            ApplySharedCharacterDatHeavyWalkMovement();

            if (IsSharedCharacterDatAttackInputReadyInternal() &&
                FrameCache?.HasFrame(LF2StandardFrames.HeavyWeaponThw) == true)
            {
                Runtime.AnimSub = 0;
                AttackingCounter = 0;
                ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
            }

            return true;
        }

        private void ApplySharedCharacterDatWalkRunMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null || Runtime.Y != 0f)
                return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1)
                rate = 1;

            int animSub = Runtime.AnimSub;
            if (animSub > 0)
                Runtime.AnimSub--;
            else if (animSub < 0)
                Runtime.AnimSub++;

            bool handled = false;
            bool vxSet = false;
            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
            {
                handled = true;
                if (Runtime.Dir == "left")
                    Runtime.AnimSub = 0;

                SwitchDir("right");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);
                Runtime.Vx = characterData.walking_speed;
                vxSet = true;

                if (Runtime.PrevRight == 0)
                    Runtime.AnimSub += 10;
                if (Runtime.AnimSub >= 11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.RunningStart);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (!handled && Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
            {
                if (Runtime.Dir == "right")
                    Runtime.AnimSub = 0;

                SwitchDir("left");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);
                Runtime.Vx = -characterData.walking_speed;
                vxSet = true;

                if (Runtime.PrevLeft == 0)
                    Runtime.AnimSub -= 10;
                if (Runtime.AnimSub <= -11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.RunningStart);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
            {
                if (!vxSet)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);

                Runtime.Vz = -characterData.walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
            else if (Runtime.KeyDown != 0 && Runtime.KeyUp == 0)
            {
                if (!vxSet)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);

                Runtime.Vz = characterData.walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
        }

        private void ApplySharedCharacterDatHeavyWalkMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null || Runtime.Y != 0f)
                return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1)
                rate = 1;

            int animSub = Runtime.AnimSub;
            if (animSub > 0)
                Runtime.AnimSub--;
            else if (animSub < 0)
                Runtime.AnimSub++;

            if ((Frame?.N ?? -1) < LF2StandardFrames.HeavyObjWalk0)
                SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);

            bool hasHorizontalMove = false;
            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
            {
                hasHorizontalMove = true;
                if (Runtime.Dir == "left")
                    Runtime.AnimSub = 0;

                SwitchDir("right");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);
                Runtime.Vx = characterData.heavy_walking_speed;

                if (Runtime.PrevRight == 0)
                    Runtime.AnimSub += 10;
                if (Runtime.AnimSub >= 11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjRun);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
            {
                hasHorizontalMove = true;
                if (Runtime.Dir == "right")
                    Runtime.AnimSub = 0;

                SwitchDir("left");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);
                Runtime.Vx = -characterData.heavy_walking_speed;

                if (Runtime.PrevLeft == 0)
                    Runtime.AnimSub -= 10;
                if (Runtime.AnimSub <= -11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjRun);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
            {
                if (!hasHorizontalMove)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);

                Runtime.Vz = -characterData.heavy_walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
            else if (Runtime.KeyDown != 0 && Runtime.KeyUp == 0)
            {
                if (!hasHorizontalMove)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);

                Runtime.Vz = characterData.heavy_walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
        }

        private bool TryRunSharedCharacterDatStandingActionFrame(int primaryFrameId, int fallbackFrameId = 0)
        {
            if (TryCharacterDatInputFrameJump(primaryFrameId))
                return true;

            if (fallbackFrameId > 0)
                return TryCharacterDatInputFrameJump(fallbackFrameId);

            return false;
        }

        /// <summary>
        /// shared character-DAT 的最小 jump attack 输入桥。
        /// 参考正式 C++ release `state_jumping`，这里只补无持有态空中 `key_jump -> frame 80`。
        /// </summary>
        private bool TryRunSharedCharacterDatJumpAttackInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Jump || Runtime.Y >= 0f)
                return false;
            if (Runtime.KeyJump == 0)
                return false;

            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                SwitchDir("right");
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                SwitchDir("left");

            int linkState = Runtime.LinkState;
            if (linkState == 0)
            {
                AttackingCounter = 0;
                if (!TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.JumpAttack, clampOnOverdraw: true))
                    return false;

                ImmediateFrame(LF2StandardFrames.JumpAttack);
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if (linkState % 100 == 1)
            {
                AttackingCounter = 0;
                ImmediateFrame(hasDirection ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
                return true;
            }

            if (linkState == 4 || linkState == 6)
            {
                SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.SkyLgtWpThw);
                return true;
            }

            return false;
        }

        /// <summary>
        /// shared character-DAT 的最小 running 输入桥。
        /// 当前补 stop-running、run attack、baseline running defend-to-dash-forward，
        /// 以及 release 风格的共享 held running 分支。
        /// </summary>
        private bool TryRunSharedCharacterDatRunningInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Running)
                return false;
            if (Runtime.LinkState == 2)
            {
                ApplySharedCharacterDatHeavyRunningMovement();

                if (IsSharedCharacterDatAttackInputReadyInternal())
                    ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);

                return true;
            }

            ApplySharedCharacterDatRunningMovement();

            if (TryRunSharedCharacterDatStopRunningInput())
                return true;

            if (IsSharedCharacterDatJumpInputReadyInternal())
            {
                LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
                if (characterData == null)
                    return false;

                ImmediateFrame(LF2StandardFrames.DashForward);
                Runtime.Vx = Runtime.Dir == "right"
                    ? characterData.dash_distance
                    : -characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                return true;
            }

            if (!IsSharedCharacterDatAttackInputReadyInternal())
                return false;

            int linkState = Runtime.LinkState;
            bool hasDirection = HasAnyDirectionInputForSharedCharacterDat();

            if (linkState % 100 == 1)
            {
                ImmediateFrame(hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.RunWeaponAtck);
                return true;
            }

            if (linkState == 4)
            {
                ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                return true;
            }

            if (linkState == 6)
            {
                ImmediateFrame(hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.SkyLgtWpThw);
                return true;
            }

            if (!TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.RunAttack))
                return false;

            ImmediateFrame(LF2StandardFrames.RunAttack);
            return true;
        }

        /// <summary>
        /// shared character-DAT 的最小 normal running 基线移动。
        /// 这里只补跑动帧推进、速度写入、斜向 lane 速度和反向 stop-running 前置帧维护，
        /// 不覆盖后续的 stop-running / dash / run-attack 分支。
        /// </summary>
        private void ApplySharedCharacterDatRunningMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null)
                return;

            AttackingCounter = 0;

            int rate = characterData.running_frame_rate;
            if (rate < 1)
                rate = 1;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 4);
            Runtime.AnimCounter = animCounter;

            int frameId = LF2StandardFrames.RunningStart + (animCounter / rate);
            if ((animCounter / rate) >= 3)
                frameId = LF2StandardFrames.Running1;

            if (Runtime.Dir == "right")
            {
                Runtime.Vx = characterData.running_speed;
                if (Runtime.KeyLeft != 0)
                    frameId = LF2StandardFrames.StopRunning;
            }
            else
            {
                Runtime.Vx = -characterData.running_speed;
                if (Runtime.KeyRight != 0)
                    frameId = LF2StandardFrames.StopRunning;
            }

            ApplySharedCharacterDatRunLane(characterData.running_speedz);
            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void ApplySharedCharacterDatHeavyRunningMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null)
                return;

            AttackingCounter = 0;

            int rate = characterData.running_frame_rate;
            if (rate < 1)
                rate = 1;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 4);
            Runtime.AnimCounter = animCounter;

            int frameId = LF2StandardFrames.HeavyObjRun + (animCounter / rate);
            if ((animCounter / rate) >= 3)
                frameId = LF2StandardFrames.TreeJump0;

            if (Runtime.Dir == "right")
            {
                Runtime.Vx = characterData.heavy_running_speed;
                if (Runtime.KeyLeft != 0)
                    frameId = LF2StandardFrames.TreeJump2;
            }
            else
            {
                Runtime.Vx = -characterData.heavy_running_speed;
                if (Runtime.KeyRight != 0)
                    frameId = LF2StandardFrames.TreeJump2;
            }

            bool upPressed = Runtime.KeyUp != 0 && Runtime.KeyDown == 0;
            bool downPressed = Runtime.KeyDown != 0 && Runtime.KeyUp == 0;
            if (upPressed)
            {
                Runtime.Vz = -characterData.heavy_running_speedz;
                Runtime.Vx *= 5f / 6f;
            }
            else if (downPressed)
            {
                Runtime.Vz = characterData.heavy_running_speedz;
                Runtime.Vx *= 5f / 6f;
            }

            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void ApplySharedCharacterDatRunLane(float speedZ)
        {
            if (Runtime == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
                Runtime.Vz = -speedZ;
            else if (downPressed && !upPressed)
                Runtime.Vz = speedZ;
        }

        /// <summary>
        /// shared character-DAT 的最小 stop-running 输入桥。
        /// 这里只补 running 状态下的反向水平输入切入 `StopRunning` 帧，
        /// 不扩到 state 5 后续帧事件或 dash-forward 变体。
        /// </summary>
        private bool TryRunSharedCharacterDatStopRunningInput()
        {
            if (Runtime == null)
                return false;

            bool facingRight = Runtime.Dir == "right";
            bool reversePressed = facingRight ? Runtime.KeyLeft != 0 : Runtime.KeyRight != 0;
            if (!reversePressed)
                return false;

            ImmediateFrame(LF2StandardFrames.StopRunning);
            return true;
        }

        /// <summary>
        /// shared character-DAT 的最小 crouch 输入桥。
        /// 这里只补 `frame 215` 的 defend / crouch-dash 分支。
        /// release `ApplyFrame215Landing(...)` 的 dash branch 没有 `LinkState` gate，
        /// 所以 transformed character-DAT 的 non-LF2Character shell 在 held 路径下也必须能进 dash。
        /// </summary>
        private bool TryRunSharedCharacterDatCrouchInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.N ?? -1) != LF2StandardFrames.Crouch)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return false;

            bool handled = false;
            if (IsSharedCharacterDatDefendInputReadyInternal())
            {
                ImmediateFrame(LF2StandardFrames.Rowing2);
                handled = true;
            }
            else if (IsSharedCharacterDatAttackInputReadyInternal())
            {
                ImmediateFrame(LF2StandardFrames.Rowing2);
                handled = true;
            }

            bool jumpReady = IsSharedCharacterDatJumpInputReadyInternal();
            bool rightPressed = Runtime.KeyRight != 0;
            bool leftPressed = Runtime.KeyLeft != 0;

            if ((rightPressed || Runtime.Vx > 0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                ImmediateFrame(Runtime.Dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                Runtime.AnimSub = 0;
                Runtime.Vx = characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                handled = true;
            }
            else if ((leftPressed || Runtime.Vx < -0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                ImmediateFrame(Runtime.Dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
                Runtime.AnimSub = 0;
                Runtime.Vx = -characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                handled = true;
            }

            ApplySharedCharacterDatDashLane(characterData.dash_distancez);

            return handled;
        }

        /// <summary>
        /// shared character-DAT 的最小倒地 recovery 输入桥。
        /// 这里只补 `FallingFront2/FallingBack2 + KeyDefend + CdJump` 的 recovery 分支。
        /// </summary>
        private bool TryRunSharedCharacterDatDefensiveRecoveryInputPhase()
        {
            if (Runtime == null)
                return false;

            int frameId = Frame?.N ?? -1;
            if (frameId != LF2StandardFrames.FallingFront2 && frameId != LF2StandardFrames.FallingBack2)
                return false;
            if (WeaponCount < 0 || !IsSharedCharacterDatJumpInputReadyInternal() || Health?.HP <= 0)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            bool backward = Runtime.Dir == "right" ? Runtime.Vx <= 0f : Runtime.Vx >= 0f;
            ImmediateFrame(backward ? LF2StandardFrames.Rowing : LF2StandardFrames.RowingBack);
            AttackingCounter = 0;

            if (characterData == null)
                return true;

            if (Runtime.Vy > characterData.rowing_height)
                Runtime.Vy = characterData.rowing_height;

            float rowingDistance = characterData.rowing_distance;
            if (Runtime.Vx > -1f && Runtime.Vx < 1f)
                Runtime.Vx = Runtime.Dir == "left" ? rowingDistance : -rowingDistance;
            else
                Runtime.Vx = Runtime.Vx > 0f ? rowingDistance : -rowingDistance;

            return true;
        }

        /// <summary>
        /// shared character-DAT 的最小 dash attack 输入桥。
        /// 这里按正式 C++ release `state_dash` 只补已确认的最小 held 分支：
        /// 无持有态 `DashAttack`、`linkState % 100 == 1 -> DashWeaponAtck`、
        /// `linkState == 4/6 && hasDirection -> SkyLgtWpThw`。
        /// </summary>
        private bool TryRunSharedCharacterDatDashAttackInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Dash)
                return false;

            ApplySharedCharacterDatDashFrameMaintenance();

            if (Runtime.KeyJump == 0)
                return false;

            bool dashForward = (Runtime.Dir == "right" && Runtime.Vx > 0f) ||
                               (Runtime.Dir == "left" && Runtime.Vx < 0f);
            if (!dashForward)
                return false;

            int linkState = Runtime.LinkState;
            if (linkState == 0)
            {
                if (!TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.DashAttack))
                    return false;

                ImmediateFrame(LF2StandardFrames.DashAttack);
                return true;
            }

            if (linkState % 100 == 1)
            {
                ImmediateFrame(LF2StandardFrames.DashWeaponAtck);
                Runtime.Vy -= 1f;
                AttackingCounter = 0;
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if ((linkState == 4 || linkState == 6) && hasDirection)
            {
                ImmediateFrame(LF2StandardFrames.SkyLgtWpThw);
                Runtime.Vy -= 1f;
                AttackingCounter = 0;
                return true;
            }

            return false;
        }

        private void ApplySharedCharacterDatDashFrameMaintenance()
        {
            if (Runtime == null)
                return;

            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                SwitchDir("right");
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                SwitchDir("left");

            bool facingRight = Runtime.Dir == "right";
            if (facingRight)
            {
                if (Frame.N != LF2StandardFrames.DashBack2 && Runtime.Vx < 0f)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (Runtime.Vx > 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward);
            }
            else
            {
                if (Runtime.Vx > 0f && Frame.N != LF2StandardFrames.DashBack2)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (Runtime.Vx < 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward);
            }
        }

        private bool HasAnyDirectionInputForSharedCharacterDat()
        {
            return Runtime != null &&
                   (Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0);
        }

        private int RandomSharedCharacterDatWeaponAttackFrame()
        {
            return BattleRandInt(0, 2) == 0
                ? LF2StandardFrames.NormalWeaponAtck
                : LF2StandardFrames.NormalWeaponAtck2;
        }

        private void StepSharedCharacterDatWalkAnimation(int rate, int frameBase)
        {
            if (Runtime == null)
                return;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 6);
            Runtime.AnimCounter = animCounter;

            int fi = animCounter / rate;
            int frameId = fi < 4 ? frameBase + fi : frameBase + (6 - fi);
            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void SetSharedCharacterDatMoveFrameDirect(int frameId)
        {
            if (Frame == null || FrameCache == null || Runtime == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return;

            Frame.PN = Frame.N;
            Frame.N = frameId;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            Runtime.NextFrame = Frame.D.next;
        }

        private void ApplySharedCharacterDatSpecialStateLaneControl()
        {
            if (Runtime == null || GetRuntimeYInt() != 0)
                return;

            int state = Frame?.D?.state ?? -1;
            if (state != LF2States.DeepSpecific && state != LF2States.FirenSpecific)
                return;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
                Runtime.Vz = -characterData.running_speedz;
            else if (downPressed && !upPressed)
                Runtime.Vz = characterData.running_speedz;
        }

        private void ApplySharedCharacterDatDashLane(float dashDistanceZ)
        {
            if (Runtime == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
                Runtime.Vz = -dashDistanceZ;
            else if (downPressed && !upPressed)
                Runtime.Vz = dashDistanceZ;
        }

        protected bool TrySpendSharedCharacterDatFramePpCost(int frameId, bool clampOnOverdraw = false)
        {
            if (!IsPpModeEnabled() || Health == null)
                return true;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            int ppCost = targetFrame.mp;
            if (!clampOnOverdraw && Health.PP < ppCost)
                return false;

            Health.PP -= ppCost;
            if (Health.PP >= 0)
            {
                SpendPpDisplay(ppCost);
            }
            else
            {
                Health.PP = 0;
            }

            return true;
        }

        private void ApplySharedRuntimeInputEvent(FuncKeyMask key, bool down, bool forceFreshEdge = false)
        {
            if (forceFreshEdge && down)
                ForceSharedRuntimePreviousState(key);

            switch (key)
            {
                case FuncKeyMask.right: Runtime.KeyRight = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.left: Runtime.KeyLeft = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.up: Runtime.KeyUp = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.down: Runtime.KeyDown = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.att: Runtime.KeyAttack = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.jump: Runtime.KeyJump = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.def: Runtime.KeyDefend = down ? (byte)1 : (byte)0; break;
            }

            if (!down)
                return;

            // shared character-DAT 输入镜像也要保持 reference 的交叉 cooldown 语义：
            // attack -> CdDefend, defend -> CdJump, jump -> CdAttack。
            switch (key)
            {
                case FuncKeyMask.right:
                    if (Runtime.PrevRight == 0)
                    {
                        Runtime.CdRight = 5;
                        Runtime.PushInputHistory(6);
                    }
                    break;
                case FuncKeyMask.left:
                    if (Runtime.PrevLeft == 0)
                    {
                        Runtime.CdLeft = 5;
                        Runtime.PushInputHistory(4);
                    }
                    break;
                case FuncKeyMask.up:
                    if (Runtime.PrevUp == 0)
                    {
                        Runtime.CdUp = 5;
                        Runtime.PushInputHistory(8);
                    }
                    break;
                case FuncKeyMask.down:
                    if (Runtime.PrevDown == 0)
                    {
                        Runtime.CdDown = 5;
                        Runtime.PushInputHistory(2);
                    }
                    break;
                case FuncKeyMask.att:
                    if (Runtime.PrevAttack == 0)
                    {
                        Runtime.CdDefend = 5;
                        Runtime.PushInputHistory(9);
                    }
                    break;
                case FuncKeyMask.jump:
                    if (Runtime.PrevJump == 0)
                    {
                        Runtime.CdAttack = 5;
                        Runtime.PushInputHistory(5);
                    }
                    break;
                case FuncKeyMask.def:
                    if (Runtime.PrevDefend == 0)
                    {
                        Runtime.CdJump = 5;
                        Runtime.PushInputHistory(0);
                    }
                    break;
            }
        }

        private void ForceSharedRuntimePreviousState(FuncKeyMask key)
        {
            switch (key)
            {
                case FuncKeyMask.right: Runtime.PrevRight = 0; break;
                case FuncKeyMask.left: Runtime.PrevLeft = 0; break;
                case FuncKeyMask.up: Runtime.PrevUp = 0; break;
                case FuncKeyMask.down: Runtime.PrevDown = 0; break;
                case FuncKeyMask.att: Runtime.PrevAttack = 0; break;
                case FuncKeyMask.jump: Runtime.PrevJump = 0; break;
                case FuncKeyMask.def: Runtime.PrevDefend = 0; break;
            }
        }

        /// <summary>
        /// 供“当前 DAT 是 Character”的通用输入消费链使用的 DJA guard。
        /// 这层判断只依赖共享 runtime / frame 数据，不要求 CLR 类型真的是 LF2Character。
        /// </summary>
        internal bool ShouldHoldCharacterDatDjaInputGuard(int targetFrame)
        {
            if (ObjectId != 6 || targetFrame != 300 || Health == null || Health.HP <= 177)
                return false;

            return Match?.Runtime?.Flow?.DjaGuardGlobal44F224 == 0;
        }

        internal bool CanEnterCharacterDatInputFrameJump()
        {
            if (ObjectId == 51 && Runtime?.Unk328 == 1)
                return false;

            return TransformOriginalObjectId == -1 && Runtime.LinkState != 2;
        }

        /// <summary>
        /// 通用输入跳帧入口。
        /// 参考 C# `DoFrameJump(...)`，用于当前 DAT 已经是 Character 的任意实体。
        /// </summary>
        internal bool TryCharacterDatInputFrameJump(int frameId)
        {
            bool flipFacing = false;
            if (frameId < 0)
            {
                frameId = -frameId;
                flipFacing = true;
            }

            if (frameId == 999)
                frameId = 0;

            if (FrameCache?.HasFrame(frameId) != true || Health == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            bool ppMode = IsPpModeEnabled();
            if (ppMode)
            {
                int ppCost = targetFrame.mp % 1000;
                int hpCost = (targetFrame.mp / 1000) * 10;
                if (Health.PP < ppCost || Health.HP <= hpCost)
                    return false;

                Health.HP -= hpCost;
                Health.PP -= ppCost;
                ComboCountVic += hpCost;
                SpendPpDisplay(ppCost);
            }

            if (flipFacing && ppMode)
                SwitchDir(Runtime.Dir == "right" ? "left" : "right");

            OnFrameTransit(frameId, false);
            return true;
        }

        /// <summary>
        /// 判断当前实体是否满足 N30 晚阶段输入触发条件。
        /// 这里按“当前 DAT 是否还是角色”判断，而不是按 CLR 子类判断。
        /// </summary>
        internal bool TryResolveLateN30InputTriggerCode(out int frameVal)
        {
            frameVal = 0;

            int slotIndex = Runtime?.SlotIndex ?? -1;
            if (slotIndex < 0 || slotIndex >= 10)
                return false;
            if (Health == null || Health.HP <= 0)
                return false;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return false;

            int[] history = Runtime?.InputHistory;
            if (history == null || history.Length < 6)
                return false;

            int a = history[2];
            int b = history[3];
            int c = history[4];
            int d = history[5];
            if (a == 9 && b == 0 && c == 9 && d == 0) frameVal = 100;
            else if (a == 9 && b == 9 && c == 9 && d == 9) frameVal = 102;
            else if (a == 9 && b == 5 && c == 9 && d == 5) frameVal = 104;

            return frameVal != 0;
        }

        /// <summary>
        /// 处理当前 DAT 仍是角色对象时的晚阶段 N30 输入触发。
        /// 参考实现按 slot + 当前 DAT 类型参与，所以不能只挂在 LF2Character 上。
        /// </summary>
        private void RunLateCharacterDatInputTrigger()
        {
            if (!TryResolveLateN30InputTriggerCode(out int frameVal))
                return;

            Runtime?.ClearInputHistoryTail();

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            int slotIndex = Runtime?.SlotIndex ?? -1;
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            ConfigureLateN30SpawnTask(task, slotIndex, frameVal);

            LF2Entity spawned = factory.CreateObjectImmediate(task);
            if (spawned == null)
                return;

            ApplyLateN30HistoryGateBroadcast(frameVal);
        }

        /// <summary>
        /// 统一写入晚阶段 N30 触发生成 998 效果时的运行时身份。
        /// Unity 侧同阵营筛选已经以 `RelationTeam -> Team` 作为当前真值，
        /// 所以这里的 effect 任务也必须沿用同一套来源，不能继续把 `team` 留成 0。
        /// </summary>
        private void ApplyLateN30SpawnIdentity(OPointCreateTask task, int slotIndex)
        {
            if (task == null)
                return;

            int sourceTeam = ResolveN30HistoryGateTeam(this);
            task.team = sourceTeam;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = sourceTeam;
            task.holderCopySlot = -1;
            task.spawnerEntityIndex = slotIndex;
        }

        /// <summary>
        /// 晚阶段 N30 生成的 `oid=998` 属于立即特效路径。
        /// 这类 task 的 `z` 已经直接编码了参考实现最终可见 Z，
        /// 不能再吃工厂通用的 post-init `Z+1` 抬高。
        /// </summary>
        private void ConfigureLateN30SpawnTask(OPointCreateTask task, int slotIndex, int frameVal)
        {
            if (task == null)
                return;

            task.opoint = new ObjectPoint { oid = 998, kind = 0, action = frameVal, facing = 0 };
            task.parent = null;
            ApplyLateN30SpawnIdentity(task, slotIndex);
            task.pos = new Vector3(GetRuntimeXInt(), 0f, GetRenderZInt());
            task.z = GetRenderZInt();
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = GetRuntimeXInt();
            task.initialRuntimeY = 0;
            task.initialRuntimeZ = GetRenderZInt();
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;
        }

        /// <summary>
        /// N30 晚阶段除了生成 998 效果外，
        /// 102 还要给同阵营角色打开 history gate，104 要关闭 gate。
        /// </summary>
        private void ApplyLateN30HistoryGateBroadcast(int frameVal)
        {
            if (frameVal != 102 && frameVal != 104)
                return;

            SimulationWorld world = Match;
            if (world == null)
                return;

            int sourceTeam = ResolveN30HistoryGateTeam(this);
            if (sourceTeam == 0)
                return;

            bool enabled = frameVal == 102;
            N30HistoryGateScratch.Clear();
            world.GetAllEntities(N30HistoryGateScratch);

            try
            {
                for (int i = 0; i < N30HistoryGateScratch.Count; i++)
                {
                    LF2Entity teammate = N30HistoryGateScratch[i];
                    if (teammate == null || teammate.Runtime == null || teammate.Health == null)
                        continue;
                    if (teammate.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                        continue;
                    if (teammate.Health.HP <= 0)
                        continue;
                    if (ResolveN30HistoryGateTeam(teammate) != sourceTeam)
                        continue;

                    teammate.Runtime.SetInputHistoryGate(enabled);
                }
            }
            finally
            {
                N30HistoryGateScratch.Clear();
            }
        }

        private static int ResolveN30HistoryGateTeam(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }

        /// <summary>
        /// 早期 state 400/401 传送特判入口。
        /// C++ release 只要求 source active 且有当前 frame；候选 target 才要求 Character DAT。
        /// source 不能按 CLR 类型或当前 DAT 类型提前排除。
        /// </summary>
        internal virtual void RunEarlyTeleportSpecialsPhase(System.Collections.Generic.List<LF2Entity> entities, bool frameToggleGate)
        {
            if (frameToggleGate || entities == null || Health == null)
                return;

            int state = Frame?.D?.state ?? -1;
            bool toEnemy = state == LF2States.TeleportToEnemy;
            bool toTeammate = state == LF2States.TeleportToTeammate;
            if (!toEnemy && !toTeammate)
                return;

            LF2Entity best = null;
            int bestDistance = toEnemy ? 10000 : -1;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity target = entities[i];
                if (target == null || target.Health == null)
                    continue;
                if (target.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;
                if (target.Health.HP <= 0)
                    continue;
                if (toEnemy && target.RelationTeam == RelationTeam)
                    continue;
                if (toTeammate && target.RelationTeam != RelationTeam)
                    continue;

                int distance = Mathf.Abs(target.GetRenderZInt() - GetRenderZInt()) +
                               Mathf.Abs(target.GetRuntimeXInt() - GetRuntimeXInt());
                if (toEnemy && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
                else if (toTeammate && distance > bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            if (best == null)
            {
                Runtime.Y = 0f;
                Runtime.YInt = 0;
                Runtime.Vx = 0f;
                Runtime.Vy = 0f;
                Runtime.Vz = 0f;
                return;
            }

            int offset = toEnemy ? 120 : 60;
            int nextZ = best.GetRenderZInt() + 1;
            int nextX = Runtime.Dir == "right"
                ? best.GetRuntimeXInt() - offset
                : best.GetRuntimeXInt() + offset;

            Runtime.Z = nextZ;
            Runtime.ZInt = nextZ;
            Runtime.X = nextX;
            Runtime.XInt = nextX;
            Runtime.Y = 0f;
            Runtime.YInt = 0;
            Runtime.Vx = 0f;
            Runtime.Vy = 0f;
            Runtime.Vz = 0f;
        }

        internal virtual void RunLateDeathOpointPreCleanupPhase() { }

        internal virtual bool TryRunLatePostOpointCleanupPhase() => false;

        internal virtual void RunLateTailBeforePrevFrame()
        {
            RunLateCharacterDatInputTrigger();
            SpawnLateTransitionEffects();
        }

        public virtual void MirrorLatePrevFrame()
        {
            if (Frame != null)
                Frame.Prev = Frame.N;
        }

        private void SpawnLateTransitionEffects()
        {
            LF2FrameData prevFrame = GetFrameDataById(Frame?.Prev ?? 0);
            LF2FrameData currentFrame = Frame?.D;
            if (prevFrame == null || currentFrame == null)
                return;

            int prevState = prevFrame.state;
            int currentState = currentFrame.state;
            bool spawned = false;

            if ((prevState == 13 || (Frame?.Prev ?? 0) == 200) &&
                currentState != 13 && (Frame?.N ?? 0) != 200)
            {
                QueueBattleSound("SFX_066");
                SpawnTransitionEffectBranch1();
                spawned = true;
            }

            if (prevState != 18 && prevState != 19)
                return;

            int count = 0;
            if (currentState != 18 && currentState != 19)
                count = 7;
            else if (BattleRandInt(0, 4) == 0)
                count = 1;

            if (count > 0)
            {
                SpawnTransitionEffectBranch2(count);
                spawned = true;
            }

            if (spawned)
                RefreshRuntimeSnapshot();
        }

        private void SpawnTransitionEffectBranch1()
        {
            for (int n = 0; n < 15; n++)
            {
                int frameId = n < 2 ? 120 : n < 5 ? 130 : n < 9 ? 125 : 135;
                SpawnTransitionEffect(
                    frameId,
                    GetRuntimeXInt() + BattleRandInt(0, 39) - 19f,
                    (float)(Runtime.Y - BattleRandInt(0, 29)),
                    GetRenderZInt(),
                    (float)(Runtime.Vx * 0.5f + BattleRandInt(0, 11) - 5f),
                    -((float)BattleRandInt(0, 20) / 2f) - 8f);
            }
        }

        private void SpawnTransitionEffectBranch2(int count)
        {
            for (int n = 0; n < count; n++)
            {
                SpawnTransitionEffect(
                    140,
                    GetRuntimeXInt() + BattleRandInt(0, 59) - 29f,
                    (float)(Runtime.Y - BattleRandInt(0, 29)),
                    GetRenderZInt(),
                    (float)(Runtime.Vx + BattleRandInt(0, 11) - 5f),
                    -1f);
            }
        }

        private void SpawnTransitionEffect(int frameId, float x, float y, int zInt, float vx, float vy)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = 999,
                kind = 0,
                action = frameId,
                facing = Runtime.Dir == "right" ? 0 : 1,
                x = 0,
                y = 0,
                dvx = 0,
                dvy = 0,
                dvz = 0,
            };
            task.parent = null;
            task.team = Team;
            task.relationTeam = RelationTeam != 0 ? RelationTeam : Team;
            task.useExplicitRelationIdentity = true;
            task.holderCopySlot = -1;
            task.pos = new Vector3(x, y, zInt);
            task.z = zInt;
            task.dir = Runtime.Dir;
            task.useDirectVelocity = true;
            task.directVx = vx;
            task.directVy = vy;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.TransitionEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = ReleaseInt(x);
            task.initialRuntimeY = ReleaseInt(y);
            task.initialRuntimeZ = zInt;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;

            factory.EnqueueCreateObject(task);
        }

        public virtual void FreeEntityLikeExe()
        {
            OnTransitDestroy();
        }

        public virtual void DirectWriteFramePreserveWaitCounter(int frameId)
        {
            SetFrameTickDirect(frameId);
        }

        public virtual void DirectWriteFrameImmediateWaitReset(int frameId)
        {
            SetFrameTickDirect(frameId, 0);
        }

        private void ApplyStateDataTransform(int targetObjectId, bool applyHitStop140)
        {
            if (targetObjectId < 0)
                return;

            LF2CharacterDataWrapper wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            ImmediateFrame(0);

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            if (applyHitStop140)
                HitStun = 140;

            RefreshRuntimeSnapshot();
        }

        internal static LF2CharacterDataWrapper ResolveRuntimeCharacterConfig(int targetObjectId)
        {
            LF2CharacterDataWrapper overrideWrapper = RuntimeCharacterConfigResolverOverride?.Invoke(targetObjectId);
            if (overrideWrapper != null)
                return overrideWrapper;

            return CharacterAnimtorManager.Instance?.GetCharacterConfig(targetObjectId);
        }

        internal bool TryApplyRuntimeIdentity(
            int targetObjectId,
            int targetFrameId,
            bool resetWaitCounter,
            out LF2CharacterDataWrapper wrapper)
        {
            wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return false;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            WeaponCount = wrapper.characterData?.weapon_hp ?? 0;

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            Frame.N = targetFrameId;
            Frame.D = FrameCache.GetFrameDataById(targetFrameId);
            if (Frame.D != null)
            {
                int waitCounter = resetWaitCounter ? 0 : (Trans?.WaitCounter ?? 0);
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
            }

            RefreshRuntimeSnapshot();
            return true;
        }

        internal bool TryReloadCurrentFrameDataForRuntimeIdentity(int targetObjectId)
        {
            LF2CharacterDataWrapper wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return false;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            WeaponCount = wrapper.characterData?.weapon_hp ?? 0;

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            Frame.D = FrameCache.GetFrameDataById(Frame.N);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);

            RefreshRuntimeSnapshot();
            return true;
        }

        public virtual int GetCurrentDataObjectTypeForSimulation() => ResolveCurrentDataObjectType(this);

        public virtual int GetCurrentDataObjectType() => GetCurrentDataObjectTypeForSimulation();

        /// <summary>
        /// 参考 C# release 的 `ObjTypeRules.ToRuntimeObjType(...)`：
        /// 运行时粗分类只区分“角色”与“非角色”。
        /// Unity 内部仍然保留完整 DAT type 供大多数逻辑使用，
        /// 这里只在 runtime 身份快照/校验层复用 release 语义。
        /// </summary>
        public static int ResolveReferenceRuntimeObjTypeFromDataType(int currentDataType)
        {
            return currentDataType == (int)LF2ObjectType.Character ? 0 : 1;
        }

        /// <summary>
        /// 按当前 DAT 包装器解析对象 type。
        /// C# 基准工程 EntityCategoryResolver 使用 CharData.ObjType，而不是实体子类类型；
        /// Unity 的对象池类型只决定实例来自哪个池，战斗判定必须读取当前 DAT type。
        /// </summary>
        public static int ResolveCurrentDataObjectType(LF2Entity entity)
        {
            if (entity == null)
                return -1;

            int wrapperOid = entity.FrameCache?.Wrapper?.characterId ?? entity.ObjectId;
            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(wrapperOid);
            return definition?.type ?? entity.ReleaseEntityType;
        }

        public virtual bool ShouldDeferInitialRuntimeSnapshot() => false;

        public virtual LF2FrameData GetCollisionFrameData()
        {
            return Frame?.Prev2D ?? Frame?.D;
        }

        public virtual void CaptureCollisionFrameSnapshot()
        {
            SyncCollisionSnapshotToCurrentFrame();
        }

        internal void SyncCollisionSnapshotToCurrentFrame()
        {
            if (Frame == null)
                return;

            Frame.Prev2 = Frame.N;
            Frame.Prev2D = Frame.D;
            Runtime.PrevFrame2 = Frame.Prev2;
        }

        internal bool ReloadCurrentFrameDataFromWrapper()
        {
            if (Frame == null || FrameCache == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(Frame.N);
            if (targetFrame == null)
                return false;

            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            RefreshRuntimeSnapshot();
            return true;
        }

        public virtual int GetRenderPicIndex()
        {
            int pic = Frame?.D?.pic ?? -1;
            return pic >= 0 ? pic + Runtime.RenderPicOffset : pic;
        }

        public virtual float GetDisplayZ()
        {
            if (GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                Runtime != null &&
                System.Math.Abs(Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (float)(Runtime.Z - Runtime.Type3VisualZOffset);
            }

            return GetRenderZInt();
        }

        public virtual int GetRenderSortingOrder()
        {
            int order = GetRenderZInt() + Mathf.RoundToInt(Runtime?.Zz ?? 0f);
            if (ShouldRenderAboveCharacters())
                order += OverlaySortingOrderOffset;
            return order;
        }

        public virtual float GetSpriteWidthPxForRender()
        {
            float width = Sprite?.GetWidthPx() ?? 0f;
            if (width <= 0f)
                width = GetSpriteWidthPxForCollision();
            return width;
        }

        public virtual float GetSpriteHeightPxForRender()
        {
            return Sprite?.GetHeightPx() ?? 0f;
        }

        public virtual int GetRuntimeXInt()
        {
            return Runtime.XInt != 0 ? Runtime.XInt : ReleaseInt(Runtime.X);
        }

        public virtual int GetRuntimeYInt()
        {
            return Runtime.YInt != 0 ? Runtime.YInt : ReleaseInt(Runtime.Y);
        }

        public virtual int GetRenderZInt()
        {
            return Runtime.ZInt != 0 ? Runtime.ZInt : ReleaseInt(Runtime.Z);
        }

        public virtual int GetCollisionZInt() => GetCollisionZInt(GetCollisionFrameData());

        public virtual int GetCollisionZInt(LF2FrameData frame)
        {
            if (GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack && Runtime != null)
            {
                if (System.Math.Abs(Runtime.Type3VisualZOffset) > 0.0001)
                    return ReleaseInt(Runtime.Z - Runtime.Type3VisualZOffset);

                if (frame != null && frame.hit_j > 0)
                    return ReleaseInt(Runtime.Z - (frame.hit_j - 50));
            }

            return GetRenderZInt();
        }

        public virtual float GetRenderOffsetX() => Runtime.RenderOffsetX;

        public void QueueBattleSound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            Match?.QueueSound(soundId, GetRuntimeXInt());
        }

        public virtual int ResolveReleaseNeutralHolderSlotOrImplicitZero()
        {
            int slot = HolderCopySlot;
            return slot >= 0 ? slot : 0;
        }

        public virtual int ResolveReleaseNegativeLinkHolderSlotOrImplicitZero()
        {
            int slot = Runtime.HolderStableId;
            if (slot < 0)
                slot = HolderCopySlot;
            return slot >= 0 ? slot : 0;
        }

        protected virtual float ResolveCurrentSpriteFileWidthPx()
        {
            float width = Sprite?.GetCurrentSpriteWidthPx() ?? 0f;
            return width > 0f ? width : Sprite?.GetWidthPx() ?? 0f;
        }

        protected virtual bool ShouldRenderAboveCharacters()
        {
            int semantic = Runtime?.SpawnSemantic ?? 0;
            return semantic == (int)ReleaseSpawnSemantic.ImmediateEffect ||
                   semantic == (int)ReleaseSpawnSemantic.TransitionEffect;
        }

        protected virtual bool IsBlockedByReleaseLinkOrCaughtCpoint()
        {
            return Runtime.LinkState < 0;
        }

        protected virtual void ApplyReleaseSceneQueryConsumeEffects(SceneQueryHit hitInfo)
        {
            if (hitInfo.ZeroAttackerHpOnConsume && Health != null)
                Health.HP = 0;

            if (hitInfo.ReleaseHeavyHeldTargetOnConsume && hitInfo.Target != null)
                ApplyHeavyHeldTargetReleaseConsumeEffect(hitInfo.Target);
        }

        internal void ApplyReleaseSceneQueryConsumeEffectsForCharacterDatInteraction(SceneQueryHit hitInfo)
            => ApplyReleaseSceneQueryConsumeEffects(hitInfo);

        /// <summary>
        /// C++ release `HitResolve.PreprocessCandidate` 中，重武器附着目标在特定 kind=0 命中前会先断开 2/-2 双向附着，
        /// 并把附着子物体切到随机落地帧、写入一个轻微下落速度。
        /// 这里补的是那条“命中前消费语义”，不是普通 held release。
        /// </summary>
        private void ApplyHeavyHeldTargetReleaseConsumeEffect(LF2Entity holderTarget)
        {
            if (holderTarget?.Runtime == null)
                return;

            int holderSlot = holderTarget.Runtime.SlotIndex;
            int heldTargetSlot = holderTarget.Runtime.ResolveActiveHeldSlotIndex();
            if (heldTargetSlot < 0)
            {
                holderTarget.Runtime.LinkState = 0;
                return;
            }

            LF2Entity heldTarget = holderTarget.Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot);
            if (heldTarget?.Runtime == null ||
                !heldTarget.Runtime.IsActivelyHeldBySlot(holderSlot) ||
                heldTarget.Runtime.LinkState != -2)
            {
                holderTarget.Runtime.LinkState = 0;
                return;
            }

            int attackerSlot = Runtime?.SlotIndex ?? -1;
            if (attackerSlot >= 0)
                holderTarget.ItrRest?.SetVrest(attackerSlot, 45);

            holderTarget.ItrRest?.SetVrest(heldTargetSlot, 30);
            holderTarget.Runtime.LinkState = 0;
            heldTarget.Runtime.LinkState = 0;
            heldTarget.ImmediateFrame(heldTarget.BattleRandInt(0, 6));
            heldTarget.Runtime.Vy = -1f;
            heldTarget.RefreshRuntimeSnapshot();
            holderTarget.RefreshRuntimeSnapshot();
        }

        public virtual void ApplySignedCpointFrame(int frameId)
        {
            if (frameId == 0)
                return;

            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            SetFrameTickDirect(frameId);
        }

        public virtual void ApplySignedImmediateFrameWaitReset(int frameId)
        {
            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            DirectWriteFrameImmediateWaitReset(frameId);
        }

        public virtual void ResetPooledEntityState()
        {
            _hasForcedRuntimeIntPosition = false;
            Runtime.PendingFlushDestroy = false;
            Runtime.TransformOriginalObjectId = -1;
            Runtime.TransformTargetObjectId = -1;
            Runtime.RenderOffsetX = 0f;
        }

        public virtual void ApplyForcedRuntimeIntPosition(int x, int y, int z)
        {
            Runtime.XInt = x;
            Runtime.YInt = y;
            Runtime.ZInt = z;
            _hasForcedRuntimeIntPosition = true;
        }

        public virtual void ClearForcedRuntimeIntPosition()
        {
            _hasForcedRuntimeIntPosition = false;
        }

        public virtual void ConsumeForcedRuntimeIntPosition()
        {
            _hasForcedRuntimeIntPosition = false;
            RefreshRuntimeIntPosition();
        }

        public virtual void ReleaseForcedRuntimeIntPositionAfterFirstPresentation(int tickIndex)
        {
            if (tickIndex >= Runtime.FirstPresentationTick)
                ConsumeForcedRuntimeIntPosition();
        }

        public virtual void RunCpointCheckStep10()
        {
            // step10 cpoint 维护是 battle loop 的交互阶段逻辑。
            // 它读取的是 collision snapshot / runtime link / cpoint 数据，
            // 不属于角色本地 `DispatchCurrentStateEvent(...)` 的 state 事件。
            LF2FrameData catcherFrame = GetCollisionFrameData();
            CatchPoint cpoint = catcherFrame?.cpoint;
            if (cpoint == null || cpoint.kind != 1 || FrameDelay < 0)
                return;

            LF2Entity victim = Match?.FindEntityByRuntimeSlotForQuery(CaughtSlotIndex);
            if (victim == null || victim.Frame == null)
            {
                DirectWriteFrameImmediateWaitReset(0);
                return;
            }

            bool skipActions = false;
            bool skipDecrease = false;
            LF2FrameData victimFrame = victim.GetCollisionFrameData();
            if (victim.CatcherSlotIndex != (Runtime?.SlotIndex ?? -1) ||
                victimFrame?.cpoint == null ||
                victimFrame.cpoint.kind != 2)
            {
                DirectWriteFrameImmediateWaitReset(0);
                skipActions = true;
                skipDecrease = true;
            }

            if (!skipDecrease && cpoint.decrease > 0)
            {
                Runtime.CaughtDuration -= cpoint.decrease;
            }
            else if (!skipDecrease && cpoint.decrease < 0)
            {
                Runtime.CaughtDuration += cpoint.decrease;
                if (Runtime.CaughtDuration < 0)
                {
                    DirectWriteFrameImmediateWaitReset(0);
                    victim.DirectWriteFrameImmediateWaitReset(181);
                    HitCount = 1;
                    victim.HitCount = 1;
                    victim.KnockbackVx = GetReleaseXInt() > victim.GetReleaseXInt() ? -4f : 4f;
                    victim.KnockbackVy = -3f;
                    victim.Runtime.Vx = victim.KnockbackVx;
                    victim.Runtime.Vy = victim.KnockbackVy;
                    skipActions = true;
                    skipDecrease = true;
                }
            }

            if (!skipActions)
                RunCpointActionSelectionStep10(cpoint, victim);

            if (!skipDecrease && cpoint.throwvx != 0)
                ApplyCpointThrowStep10(cpoint, victim, catcherFrame);

            if (!skipDecrease)
                ApplyCpointDirControlStep10(cpoint);
        }

        public virtual void RunCpointMismatchTailStep10()
        {
            // 这里是 step10 的 mismatch 收尾，
            // 仍然属于 pass 级交互维护，不是 frame/TU/state_entry 一类本地事件。
            CatchPoint cpoint = Frame?.D?.cpoint;
            if (cpoint == null || cpoint.kind != 2)
                return;

            bool valid = false;
            LF2Entity catcher = Match?.FindEntityByRuntimeSlotForQuery(CatcherSlotIndex);
            if (catcher != null && catcher.CaughtSlotIndex == (Runtime?.SlotIndex ?? -1))
            {
                CatchPoint catcherCpoint = catcher.Frame?.D?.cpoint;
                valid = catcherCpoint != null && catcherCpoint.kind == 1;
            }

            if (valid)
                return;

            SetCpointRawFramePreserveWait(212);
            Runtime.Vy = -3f;
            if (Runtime.Y > -2f)
                Runtime.Y = -2f;
            RefreshRuntimeSnapshot();
        }

        public virtual void RunWeaponSyncHeldStep10()
        {
            LF2FrameData currentFrame = Frame?.D;
            CatchPoint cpoint = currentFrame?.cpoint;
            if (currentFrame == null || cpoint == null || cpoint.kind != 1 || currentFrame.state != LF2States.Catching)
                return;

            LF2Entity victim = Match?.FindEntityByRuntimeSlotForQuery(CaughtSlotIndex);
            if (victim == null || victim.CatcherSlotIndex != (Runtime?.SlotIndex ?? -1))
                return;

            LF2FrameData victimFrame = victim.Frame?.D;
            if (victimFrame?.cpoint == null || victimFrame.cpoint.kind != 2)
                return;

            SyncCaughtByCpointStep10(victim, currentFrame, cpoint);
        }

        public virtual void ClearHitCandidateCarriers()
        {
            HitConfirm2 = 0;
        }

        protected virtual void RunCpointActionSelectionStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            if (!SupportsSharedCharacterDatCpointStep10() || cpoint == null || victimEntity == null)
                return;

            bool attackReady = IsSharedCharacterDatAttackInputReadyInternal();
            bool jumpReady = IsSharedCharacterDatJumpInputReadyInternal();

            if (attackReady && cpoint.aaction != 0)
            {
                bool dirOk = (Runtime.KeyLeft == 0 && Runtime.KeyRight == 0) || cpoint.taction == 0;
                if (dirOk)
                    ApplySharedCpointActionStep10(cpoint.aaction, victimEntity);
            }

            if (attackReady && cpoint.taction != 0)
            {
                bool anyDir = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
                if (anyDir)
                    ApplySharedCpointActionStep10(cpoint.taction, victimEntity);
            }

            if (jumpReady && cpoint.jaction != 0)
                ApplySharedCpointActionStep10(cpoint.jaction, victimEntity);
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            ApplyCpointThrowStep10(cpoint, victimEntity, null);
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity, LF2FrameData throwFrameSnapshot)
        {
            if (cpoint == null || victimEntity == null)
                return;

            if (cpoint.throwinjury == -1 && HasStep10ThrowTransformVictimData(victimEntity))
            {
                ApplyCpointThrowTransformToSelfAndOwnedObjects(victimEntity);
            }

            if (cpoint.throwinjury > 0)
                victimEntity.WeaponCount = cpoint.throwinjury;

            // cpoint_check keeps using the source cpoint, but geometry and next are read
            // from the attacker's current DAT/current frame after action/transform.
            LF2FrameData throwFrame = FrameCache?.GetFrameDataById(Frame?.N ?? 0) ?? Frame?.D;

            int centerX = throwFrame?.centerx ?? 0;
            int centerY = throwFrame?.centery ?? 0;
            int y = GetReleaseYInt() - centerY + cpoint.y;
            int x = Runtime.Dir == "right"
                ? GetReleaseXInt() - centerX + cpoint.x
                : centerX - cpoint.x + GetReleaseXInt();

            victimEntity.Runtime.X = x;
            victimEntity.Runtime.Y = y;
            victimEntity.Runtime.Vx = Runtime.Dir == "right" ? cpoint.throwvx : -cpoint.throwvx;
            victimEntity.Runtime.Vy = cpoint.throwvy;
            SetVictimThrowVzStep10(cpoint, victimEntity);

            int nextFrame = throwFrame?.next ?? 0;
            SetCpointRawFramePreserveWait(nextFrame);
            SetCpointRawPrevFrame2(nextFrame);
            AttackingCounter = 0;
            victimEntity.SetCpointRawFramePreserveWait(cpoint.vaction);
            victimEntity.SetCpointRawPrevFrame2(cpoint.vaction);
        }

        protected void ApplyCpointThrowTransformToSelfAndOwnedObjects(LF2Entity victimEntity)
        {
            if (victimEntity == null)
                return;

            LF2CharacterDataWrapper victimConfig = ResolveRuntimeCharacterConfig(victimEntity.ObjectId);
            if (victimConfig == null)
                return;

            TransformOriginalObjectId = ObjectId;
            TransformTargetObjectId = victimEntity.ObjectId;
            FrameCache.Load(victimConfig);
            ObjectId = victimEntity.ObjectId;
            WeaponCount = victimConfig.characterData?.weapon_hp ?? 0;
            SetCpointRawFramePreserveWait(0);
            Frame.PN = Frame.N;
            EnsureSharedCharacterDatControllerForSimulation();
            PropagateCpointThrowTransformToOwnedObjects(victimConfig, victimEntity.ObjectId);
        }

        protected virtual void SetVictimThrowVzStep10(CatchPoint cpoint, LF2Entity victim)
        {
            if (cpoint == null || victim == null)
                return;

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
                victim.Runtime.Vz = -cpoint.throwvz;
            else if (Runtime.KeyUp == 0 && Runtime.KeyDown != 0)
                victim.Runtime.Vz = cpoint.throwvz;
        }

        protected virtual void ApplyCpointDirControlStep10(CatchPoint cpoint)
        {
            if (!SupportsSharedCharacterDatCpointStep10() || cpoint == null || AttackingCounter != 2)
                return;

            if (cpoint.dircontrol == 1)
            {
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("right");
                else if (Runtime.KeyRight == 0 && Runtime.KeyLeft != 0)
                    SwitchDir("left");
            }
            else if (cpoint.dircontrol == -1)
            {
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("left");
                else if (Runtime.KeyRight == 0 && Runtime.KeyLeft != 0)
                    SwitchDir("right");
            }
        }

        protected virtual void ApplyCpointHeldInjuryStep10(LF2Entity victimEntity, int injury)
        {
            if (!SupportsSharedCharacterDatCpointStep10() || victimEntity == null || victimEntity.Health == null)
                return;

            if (injury > 0)
            {
                int actualInjury = injury;
                if (victimEntity.FallDamageDiv > 0)
                    actualInjury = injury * 100 / victimEntity.FallDamageDiv;

                if (victimEntity.Health.HP > 0 &&
                    actualInjury >= victimEntity.Health.HP &&
                    victimEntity.KillCount == -1)
                {
                    LF2Entity holder = Match?.FindEntityByRuntimeSlotForQuery(HolderCopySlot);
                    if (holder != null)
                        holder.KillStat++;
                }

                victimEntity.Health.HP -= actualInjury;
                victimEntity.Health.HPLost += actualInjury;
                victimEntity.Health.HPBound -= actualInjury / 3;
                victimEntity.ComboCountVic += actualInjury;
                AttackingCounter = 1;
                FrameDelay = 2;
                victimEntity.FrameDelay = -3;
                LF2Entity comboHolder = Match?.FindEntityByRuntimeSlotForQuery(HolderCopySlot);
                if (comboHolder != null)
                    comboHolder.ComboCountAtk += actualInjury;
                return;
            }

            victimEntity.Health.HP += injury;
            victimEntity.Health.HPBound += injury / 3;
            AttackingCounter = 1;
        }

        internal bool HasStep10ThrowTransformVictimData(LF2Entity victimEntity)
        {
            return victimEntity?.FrameCache?.Wrapper?.characterData != null;
        }

        private bool SupportsSharedCharacterDatCpointStep10()
        {
            return Runtime != null && GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        /// <summary>
        /// shared character-DAT 的攻击输入入口。
        /// 这里使用的是参考 C# 当前已落地的交叉 cooldown 语义：
        /// `KeyJump + CdAttack` 才表示这一拍要走 attack 输入分支。
        /// 把读取位置收束到单点，是为了后续如果还要细调输入链，
        /// 只需要改这一层，不必回头散改 step10 / shared character-DAT 调用点。
        /// </summary>
        protected virtual bool IsSharedCharacterDatAttackInputReadyInternal()
        {
            return Runtime.KeyJump != 0 && Runtime.CdAttack > 0;
        }

        /// <summary>
        /// shared character-DAT 的跳跃输入入口。
        /// 对齐参考 C# 的交叉 cooldown 语义：
        /// `KeyDefend + CdJump` 表示 jump 输入分支。
        /// </summary>
        protected virtual bool IsSharedCharacterDatJumpInputReadyInternal()
        {
            return Runtime.KeyDefend != 0 && Runtime.CdJump > 0;
        }

        /// <summary>
        /// shared character-DAT 的防御输入入口。
        /// 对齐参考 C# 的交叉 cooldown 语义：
        /// `KeyAttack + CdDefend` 表示 defend 输入分支。
        /// </summary>
        protected virtual bool IsSharedCharacterDatDefendInputReadyInternal(bool requireDefendLockOpen = false)
        {
            if (Runtime.KeyAttack == 0 || Runtime.CdDefend <= 0)
                return false;

            return !requireDefendLockOpen || Runtime.CdDefendLock <= 0;
        }

        private void ApplySharedCpointActionStep10(int actionFrame, LF2Entity victim)
        {
            if (victim == null)
                return;

            ApplySignedCpointActionFramePreserveWait(actionFrame);
            int victimAction = Frame?.D?.cpoint?.vaction ?? 0;
            victim.SetCpointRawFramePreserveWait(victimAction);
            victim.AttackingCounter = 0;
            AttackingCounter = 0;
        }

        internal void ApplySignedCpointActionFramePreserveWait(int frameId)
        {
            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            SetCpointRawFramePreserveWait(frameId);
        }

        private void PropagateCpointThrowTransformToOwnedObjects(LF2CharacterDataWrapper wrapper, int targetObjectId)
        {
            var objects = new List<LF2Entity>();
            Match?.GetAllEntities(objects);
            int selfSlotIndex = Runtime?.SlotIndex ?? -1;
            if (selfSlotIndex < 0)
                return;

            for (int i = 0; i < objects.Count; i++)
            {
                LF2Entity entity = objects[i];
                if (entity == null || entity == this)
                    continue;
                if (!(Match?.IsActiveForCurrentPassInternal(entity) ?? false))
                    continue;
                if (entity.KillCount != selfSlotIndex)
                    continue;

                entity.FrameCache.Load(wrapper);
                entity.ObjectId = targetObjectId;
                entity.WeaponCount = wrapper.characterData?.weapon_hp ?? 0;
                entity.EnsureSharedCharacterDatControllerForSimulation();

                if (!entity.ReloadCurrentFrameDataFromWrapper())
                    entity.RefreshRuntimeSnapshot();
            }
        }

        protected virtual void SyncCpointHeldPositionStep10(LF2Entity victimEntity, LF2FrameData catcherFrame, CatchPoint catcherCpoint)
        {
            if (victimEntity == null || catcherFrame == null || catcherCpoint == null)
                return;

            int catcherX = GetReleaseXInt();
            int catcherY = GetReleaseYInt();
            int catcherZ = GetReleaseZInt();
            int dx = Runtime.Dir == "right"
                ? catcherX - catcherFrame.centerx + catcherCpoint.x
                : catcherFrame.centerx - catcherCpoint.x + catcherX;
            int dy = catcherY - catcherFrame.centery + catcherCpoint.y;

            LF2FrameData victimActionFrame = victimEntity.FrameCache?.GetFrameDataById(catcherCpoint.vaction);
            LF2FrameData victimCurrentFrame = victimEntity.FrameCache?.GetFrameDataById(victimEntity.Frame?.N ?? 0);
            int victimCpointX = victimActionFrame?.cpoint?.x ?? 0;
            int victimCpointY = victimActionFrame?.cpoint?.y ?? 0;
            int victimCenterX = victimCurrentFrame?.centerx ?? 0;
            int victimCenterY = victimCurrentFrame?.centery ?? 0;

            victimEntity.Runtime.X = victimEntity.Runtime.Dir == "right"
                ? victimCenterX - victimCpointX + dx
                : victimCpointX - victimCenterX + dx;
            victimEntity.Runtime.Y = victimCenterY - victimCpointY + dy;
            victimEntity.Runtime.Z = catcherZ;

            int coverDiv = catcherCpoint.cover / 10;
            int coverRem = catcherCpoint.cover % 10;
            if (coverRem != 0)
            {
                victimEntity.Runtime.Z += 1f;
                victimEntity.Runtime.Y -= 1f;
            }
            else
            {
                victimEntity.Runtime.Z -= 1f;
                victimEntity.Runtime.Y += 1f;
            }

            if (coverDiv == 1)
                victimEntity.SwitchDir(Runtime.Dir);
            else if (coverDiv == 2)
                victimEntity.SwitchDir(Runtime.Dir == "right" ? "left" : "right");

            victimEntity.RefreshRuntimeSnapshot();
        }

        private void SyncCaughtByCpointStep10(LF2Entity victim, LF2FrameData catcherFrame, CatchPoint cpoint)
        {
            if (victim == null || cpoint == null)
                return;

            if (cpoint.hurtable == 0 || (victim.FrameDelay == 0 && cpoint.hurtable == 1))
            {
                victim.SetCpointRawFramePreserveWait(cpoint.vaction);
            }

            if (victim.Frame?.N < 0)
            {
                victim.SwitchDir(victim.Runtime.Dir == "left" ? "right" : "left");
                victim.SetCpointRawFramePreserveWait(-victim.Frame.N);
            }

            int injury = cpoint.injury;
            if (injury != 0 && AttackingCounter == 0)
                ApplyCpointHeldInjuryStep10(victim, injury);

            SyncCpointHeldPositionStep10(victim, catcherFrame, cpoint);
        }

        internal void SetCpointRawFramePreserveWait(int frameId)
        {
            if (Frame == null || FrameCache == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            Frame.N = frameId;
            Frame.D = targetFrame;
            if (targetFrame != null)
                Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            RefreshRuntimeSnapshot();
        }

        internal void SetCpointRawPrevFrame2(int frameId)
        {
            if (Frame == null)
                return;

            Frame.Prev2 = frameId;
            Frame.Prev2D = FrameCache?.GetFrameDataById(frameId);
            Runtime.PrevFrame2 = frameId;
        }

        private int GetReleaseXInt()
        {
            return Runtime.XInt;
        }

        private int GetReleaseYInt()
        {
            return Runtime.YInt;
        }

        private int GetReleaseZInt()
        {
            return Runtime.ZInt;
        }

        // 当 FrameTransistor 发现“当前 frame 已经不是 waitCounter 记录的那一帧”时，会先通知这里。
        public virtual void OnFrameTickFrameChangedFromWaitCounter() { }

        // FrameTransistor 在真正比较 wait 之前，会先进这里。
        // 公共计数器衰减和某些早退条件，都在这一层统一处理。
        public virtual bool OnFrameTickBeforeWaitAdvance(int previousFrame)
        {
            if (Frame?.D == null)
                return false;

            RunReleaseFrameTickCounters();

            if (Frame.D.cpoint != null && Frame.D.cpoint.kind == 2)
                return false;

            return ApplyObjectSpecificFrameTickBeforeWaitAdvance();
        }

        // FrameTransistor 决定要换帧时，通过这个钩子把目标帧请求交给实体自身处理。
        public virtual void OnFrameTickTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans);
        }

        // 真正换帧成功后，才会走到这个后置钩子。
        public virtual void OnFrameTickAfterWaitAdvance(int previousFrame, bool allowJumpInit)
        {
            ApplyCommonCaughtExitHitStop(previousFrame);
            ApplyCommonFrameTickPpDisplayPostAdvance();
        }

        // next=999 的最终落点由实体自己决定，不同对象可以有不同语义。
        public virtual int ResolveFrameTickNext999Target(out bool allowJumpInit)
        {
            allowJumpInit = false;
            return 0;
        }

        protected virtual bool ApplyObjectSpecificFrameTickBeforeWaitAdvance() => true;

        /// <summary>
        /// C# 基准工程 FrameTick.Tick 的公共计数器衰减段。
        /// 该段位于 cpoint kind=2 早退之前，所有实体都要按同一顺序执行。
        /// </summary>
        private void RunReleaseFrameTickCounters()
        {
            // AttackExempt is now decremented in RunCommonFrameTick before LinkState guard (BMD-062)

            if (HitStun > 0)
                HitStun--;
            else if (HitStun < 0)
                HitStun++;

            if (FallCounter > 0)
                FallCounter--;

            if (HitStateCount > 0)
                HitStateCount--;

            if (HitConfirmCounter > 0)
                HitConfirmCounter--;
        }

        protected virtual void ApplyCommonCaughtExitHitStop(int previousFrameId) { }

        protected virtual bool IsFrameTickLeftPressed() => false;

        protected virtual bool IsFrameTickRightPressed() => false;

        protected virtual bool IsFrameTickUpPressed() => false;

        protected virtual bool IsFrameTickDownPressed() => false;

        protected virtual int GetFrameTickCdUp() => Runtime?.CdUp ?? 0;

        protected virtual int GetFrameTickCdDown() => Runtime?.CdDown ?? 0;

        protected virtual void ApplyFrame212JumpInit() { }

        /// <summary>
        /// 对齐参考 `FrameTick` 的负 mp 帧推进后处理。
        /// 当前只收敛已确认的 PP 真值与 PpDisplay 累计面，不扩展到 HUD 刷新。
        /// </summary>
        protected void ApplyCommonFrameTickPpDisplayPostAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Health == null || !IsPpModeEnabled())
                return;
            if ((Frame?.N ?? -1) >= 400)
                return;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            int mpDelta = frame.mp;
            if (mpDelta >= 0)
                return;

            if (Health.PP < mpDelta)
            {
                SetFrameTickDirect(frame.hit_d);
                frame = Frame?.D;
                if (frame == null)
                    return;
            }
            else
            {
                Health.PP += mpDelta;
                RefundPpDisplay(-mpDelta);
            }

            int turnNext = frame.hit_d;
            if (turnNext <= 0 || GetRuntimeYInt() != 0)
                return;

            bool left = Runtime?.KeyLeft != 0;
            bool right = Runtime?.KeyRight != 0;
            if (left && !right && Runtime?.Dir == "right")
                SetFrameTickDirect(turnNext);
            else if (right && !left && Runtime?.Dir == "left")
                SetFrameTickDirect(turnNext);
        }

        protected bool TryEnterReleaseFrameAdvanceAfterDelay()
        {
            if (ThrowFrameGuard >= 0 && ThrowFrameGuard == (Frame?.N ?? -1))
                return false;

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

        protected void RunSharedCharacterDatFrameAdvanceAsCharacter(int tickIndex, bool consumeForcedRuntimeIntPosition = true)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return;

            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return;

            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return;

            float mass = NTSDGlobal.Default.Machanics.Mass;
            var mechanics = new CharacterMechanics();
            var context = new CharacterMechanicsContext(
                Runtime,
                Frame?.D,
                GetSpriteWidthPxForCollision(),
                mass,
                NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                point =>
                {
                    SimulationWorld world = Match;
                    return world == null || world.IsGroundPointWalkable(point);
                });

            MechanicsStepResult stepResult = mechanics.Step(context);
            if (stepResult.landed)
                ApplySharedCharacterDatLandingIfNeeded(stepResult.verticalVelocityBeforeLanding);

            Runtime.SyncIntegerPosition();
            PromoteSharedCharacterDatState12AirborneFrameIfNeeded(tickIndex);
            PromoteSharedCharacterDatBurningAirborneFrame205IfNeeded();
            ResetWeaponCountOutsideState12FrameAdvanceTail();

            if (consumeForcedRuntimeIntPosition)
                ConsumeForcedRuntimeIntPosition();
        }

        private void ApplySharedCharacterDatLandingIfNeeded(double landedVy) // P0-f-2b B2-1: float→double
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return;

            if (frame.state == LF2States.Falling || frame.state == LF2States.Burning)
            {
                QueueBattleSound("SFX_006");
                ApplySharedCharacterDatLandingWeaponCountDamage();

                if (landedVy <= 11.0f &&
                    Runtime.Vx <= 9.0f &&
                    Runtime.Vx >= -9.0f &&
                    frame.state != LF2States.Burning)
                {
                    Runtime.Y = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vx *= 1f / 3f;
                    AttackingCounter = 0;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack
                        ? LF2StandardFrames.LyingBack
                        : LF2StandardFrames.Lying);
                }
                else
                {
                    Runtime.Y = 0f;
                    Runtime.Vy = -3.5f;
                    if (Runtime.Vx > 7f)
                        Runtime.Vx = 7f;
                    if (Runtime.Vx < -7f)
                        Runtime.Vx = -7f;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack && frame.state != LF2States.Burning
                        ? LF2StandardFrames.FallingBack5
                        : LF2StandardFrames.FallingFront5);
                }

                return;
            }

            if (frame.state == LF2States.Frozen && landedVy > 0.0001f)
            {
                Runtime.Y = 0f;

                if (landedVy <= 17f && Runtime.Vx <= 9f && Runtime.Vx >= -9f)
                {
                    Runtime.Vx *= 1f / 3f;
                    Runtime.Vy = 0f;
                    return;
                }

                int injury = FallDamageDiv == 0 ? 10 : 1000 / FallDamageDiv;
                if (Health != null)
                {
                    Health.HP -= injury;
                    if (Health.HP < 0)
                        Health.HP = 0;
                }

                Runtime.Vy = -3.5f;
                if (Runtime.Vx > 7f)
                    Runtime.Vx = 7f;
                if (Runtime.Vx < -7f)
                    Runtime.Vx = -7f;
                ImmediateFrame(LF2StandardFrames.FallingFront5);
                return;
            }

            Runtime.Y = 0f;
            Runtime.Vy = 0f;
            Runtime.Vx *= 1f / 3f;
            AttackingCounter = 0;

            int landingFrame;
            if (frame.state == LF2States.CustomSkill1)
                landingFrame = 94;
            else if (Frame.N == LF2StandardFrames.JumpingAir || frame.state == LF2States.Rowing)
                landingFrame = LF2StandardFrames.Crouch;
            else
                landingFrame = LF2StandardFrames.Crouch2;

            ImmediateFrame(landingFrame);
        }

        private void ApplySharedCharacterDatLandingWeaponCountDamage()
        {
            if (WeaponCount == 0 || Health == null)
                return;

            int damage = WeaponCount < 0 ? -WeaponCount : WeaponCount;
            if (FallDamageDiv > 0)
                damage = damage * 100 / FallDamageDiv;

            Health.HP -= damage;
            Health.HPBound -= damage;
            if (Health.HP < 0)
                Health.HP = 0;
            if (Health.HPBound < 0)
                Health.HPBound = 0;
            WeaponCount = 0;
        }

        private void PromoteSharedCharacterDatState12AirborneFrameIfNeeded(int tickIndex)
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                return;

            if (Runtime == null || Runtime.Y >= 0f)
                return;

            int frameId = Frame.N;
            double vy = Runtime.Vy;

            if (frameId < LF2StandardFrames.FallingFront5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingFront3);

                PromoteSharedCharacterDatState12NegativeWeaponCountCadenceOverride(tickIndex);
            }
            else if (frameId > LF2StandardFrames.FallingFront5 && frameId < LF2StandardFrames.FallingBack5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingBack3);
            }
        }

        private void PromoteSharedCharacterDatState12NegativeWeaponCountCadenceOverride(int tickIndex)
        {
            if (WeaponCount >= 0)
                return;

            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                return;

            if (Runtime == null || Runtime.Y >= 0f || Runtime.Vy >= 12f)
                return;

            int cadencePhase = (tickIndex - 1) % 12;
            if (cadencePhase < 0)
                cadencePhase += 12;

            SetFrameTickDirect(cadencePhase >= 6
                ? LF2StandardFrames.FallingFront2
                : LF2StandardFrames.FallingFront1);
        }

        private void PromoteSharedCharacterDatBurningAirborneFrame205IfNeeded()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Burning)
                return;

            if (Frame.N >= LF2StandardFrames.Fire2)
                return;

            if (Runtime == null || Runtime.Y >= 0f || Runtime.Vy <= 1.0f)
                return;

            SetFrameTickDirect(LF2StandardFrames.Fire2);
        }

        protected void ResetWeaponCountOutsideState12FrameAdvanceTail()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                WeaponCount = 0;
        }

        protected void SetFrameTickDirect(int frameId)
        {
            SetFrameTickDirect(frameId, Trans?.WaitCounter ?? 0);
        }

        protected void SetFrameTickDirect(int frameId, int waitCounter)
        {
            if (Frame == null || FrameCache == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return;

            Frame.N = frameId;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, waitCounter);
        }

        /// <summary>
        /// 处理参考命中特效里的编码 effect 段。
        /// 5000..5999 表示直接扣 PP，6000..6999 表示直接写目标帧。
        /// 这两段只改逻辑真值，不属于 PpDisplay 的输入/表现累计来源。
        /// </summary>
        internal bool ApplyCommonEncodedHitEffectRange(int effectNum)
        {
            if (effectNum >= 5000 && effectNum < 6000)
            {
                if (Health != null)
                {
                    int nextPp = Health.PP - (effectNum - 5000);
                    Health.PP = nextPp < 0 ? 0 : nextPp;
                }

                return true;
            }

            if (effectNum >= 6000 && effectNum < 7000)
            {
                DirectWriteFramePreserveWaitCounter(effectNum - 6000);
                return true;
            }

            return false;
        }

        protected virtual bool RunCommonFrameTick()
        {
            if (ThrowFrameGuard >= 0 && ThrowFrameGuard == (Frame?.N ?? -1))
                return false;

            if (FrameDelay != 0 &&
                GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.SpecialAttack)
                return false;

            // BMD-062: AttackExempt decrements before LinkState<0 guard (baseline FrameTick.cs L24-28)
            if (AttackExempt > 0)
                AttackExempt--;

            if ((Runtime?.LinkState ?? 0) < 0)
                return false;

            if ((Frame?.D?.state ?? 0) == LF2States.HeavyWeaponInSky)
                SwitchDir(Runtime.Vx > 0f ? "right" : "left");

            bool advanced = Trans?.Trans() == true;
            if (!advanced)
                return false;

            int currentFrame = Frame?.N ?? -1;
            if (currentFrame == 110 || currentFrame == 114)
                Runtime.CdDefendLock = 3;
            if (currentFrame == 202)
                HitStun = 20;

            return true;
        }

        protected void SpendPpDisplay(int ppCost)
        {
            if (ppCost > 0 && Runtime != null)
                Runtime.PpDisplay += ppCost;
        }

        protected void RefundPpDisplay(int ppDelta)
        {
            if (ppDelta > 0 && Runtime != null)
                Runtime.PpDisplay -= ppDelta;
        }

        protected void ApplyNonCharacterFrameVelocityForFrameAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Runtime == null)
                return;

            double vx = Runtime.Vx;
            ApplyFrameAxisVelocity(frame.dvx, ref vx, Dirh());
            Runtime.Vx = vx;

            if (frame.dvy > 500)
                Runtime.Vy = frame.dvy - 550;
            else if (frame.dvy != 0)
                Runtime.Vy += frame.dvy;

            if (frame.dvz > 500)
            {
                Runtime.Vz = frame.dvz - 550;
                return;
            }

            if (frame.dvz == 0)
                return;

            if (IsFrameTickUpPressed() && GetFrameTickCdUp() >= GetFrameTickCdDown())
                Runtime.Vz = -frame.dvz;
            if (IsFrameTickDownPressed() && GetFrameTickCdDown() >= GetFrameTickCdUp())
                Runtime.Vz = frame.dvz;
        }

        private static void ApplyFrameAxisVelocity(int value, ref double velocity, int direction) // P0-f: double sim velocity
        {
            if (value > 500)
            {
                velocity = value - 550;
                return;
            }

            if (value == 550)
            {
                velocity = 0f;
                return;
            }

            if (value > 0)
            {
                float target = value * direction;
                if (direction >= 0)
                {
                    if (velocity < target)
                        velocity = target;
                }
                else if (velocity > target)
                {
                    velocity = target;
                }

                return;
            }

            if (value >= 0)
                return;

            float negativeTarget = value * direction;
            if (direction >= 0)
            {
                if (velocity > negativeTarget)
                    velocity = negativeTarget;
            }
            else if (velocity < negativeTarget)
            {
                velocity = negativeTarget;
            }
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

        /// <summary>刷新 Runtime 中的派生字段和非位置状态。</summary>
        public void RefreshRuntimeSnapshot()
        {
            RefreshRuntimeFromEntity();
        }

        protected virtual void RefreshRuntimeFromEntity()
        {
            int currentDataType = GetCurrentDataObjectTypeForSimulation();

            Runtime.StableId = StableId;
            Runtime.ObjectId = ObjectId;
            Runtime.ObjType = ResolveReferenceRuntimeObjTypeFromDataType(currentDataType);
            Runtime.EntityType = currentDataType;
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
            Runtime.HPOrig = HPOrig;
            Runtime.HP2Orig = HP2Orig;
            Runtime.RespawnCount = RespawnCount;

            if (!_hasForcedRuntimeIntPosition)
                RefreshRuntimeIntPosition();

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

        private void RefreshRuntimeIntPosition()
        {
            Runtime.SyncIntegerPosition();
        }

        /// <summary>
        /// C# 基准工程的 Physics.SyncIntegers 使用 (int) 强制转换。
        /// 这里必须截断而不是四舍五入，否则阴影、碰撞和 opoint 的整数坐标会持续偏移。
        /// </summary>
        private int ReleaseInt(double value) // P0-f: truncate double directly (baseline (int)X); float callers widen
        {
            return (int)value;
        }

    }
}
