using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Input;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
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
    public abstract class LF2Entity : ILF2Entity, ILF2FrameCacheObserver
    {
        private readonly NTSDInputStateModule sharedCharacterDatInputModule = new NTSDInputStateModule();
        private CharacterMechanics compatibilityCharacterMechanics;
        private int requiredRuntimeSlot = -1;
        private SimulationWorld dataObjectTypeCacheWorld;
        private int dataObjectTypeCacheTick = -1;
        private int dataObjectTypeCacheObjectId = -1;
        private ObjectDefinition dataObjectTypeCacheDefinition;
        private int dataObjectTypeCacheFallback;
        private readonly LF2HitCountersModule characterDatHitCounters;
        private readonly LF2CharacterDatHitResolver characterDatHitResolver;
        private readonly LF2CharacterDatInteractionResolver characterDatInteractionResolver;
        private static readonly (
            int oid,
            int frameId,
            int xOff,
            int yOff,
            int zOff,
            double vzDelta,
            int facing)[] HitFa11SpawnCatalog =
        {
            (211, 109,    0,    0,  0,  0.0, 2),
            (221,  81,    0, -100,  0,  0.0, 2),
            (212, 100,   80,   -3,  0, -7.0, 0),
            (212, 100,  100,   -3,  0,  0.0, 0),
            (212, 100,   80,   -3,  0,  7.0, 0),
            (212, 100,  -80,   -3,  0, -7.0, 1),
            (212, 100, -100,   -3,  0,  0.0, 1),
            (212, 100,  -80,   -3,  0,  7.0, 1),
            (211,  50,  -30,   -1, -5,  0.0, 1),
            (211,  50,   30,   -1, -5,  0.0, 1),
            (211,  50,  -30,   -1,  2,  0.0, 0),
            (211,  50,   30,   -1,  2,  0.0, 0),
            (211,  50,    0,   -1, -9,  0.0, 1),
            (211,  50,    0,   -1,  6,  0.0, 0),
        };
        protected LF2Entity()
        {
            FrameCache = new LF2FrameCache(this);
            Frame.BindRuntime(Runtime);
            characterDatHitCounters = new LF2HitCountersModule();
            characterDatHitCounters.BindRuntime(Runtime);
            characterDatHitResolver = new LF2CharacterDatHitResolver(
                this,
                characterDatHitCounters);
            characterDatInteractionResolver = new LF2CharacterDatInteractionResolver(this);
        }

        internal bool TryResolveCharacterDatHit(
            InteractionArea itr,
            LF2Entity attacker,
            Vector3 attackerPos,
            PhysicsState.BattleVolume volume)
        {
            return characterDatHitResolver.ResolveHit(itr, attacker, attackerPos, volume);
        }

        internal void ConsumeCharacterDatInteractionCandidates()
        {
            characterDatInteractionResolver.TryConsumeUnifiedStep7CandidateSequence();
        }

        internal virtual bool TryGetBattleHitCandidateConsumer(
            BattleHitExecutionPass pass,
            out IBattleHitCandidateConsumer consumer)
        {
            if (pass == BattleHitExecutionPass.Character &&
                UsesCharacterDatInteractionPhase())
            {
                consumer = characterDatInteractionResolver;
                return true;
            }

            consumer = null;
            return false;
        }


        /// <summary>对象名称。</summary>
        public string Name { get; set; }

        /// <summary>实体稳定 ID。</summary>
        public int StableId
        {
            get => Runtime.StableId;
            protected set
            {
                Runtime.StableId = value;
                PublishIdentityMetadataForSimulation();
            }
        }

        /// <summary>对象 ID。</summary>
        public int ObjectId
        {
            get => Runtime.ObjectId;
            set
            {
                Runtime.ObjectId = value;
                PublishIdentityMetadataForSimulation();
            }
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

        private readonly DeterministicRng fallbackRng =
            new DeterministicRng(0x4E545344u);

        /// <summary>C++ release 实体类型值。</summary>
        public virtual int ReleaseEntityType => ObjectType;

        public virtual bool CountsAsRandomWeaponDropCandidate()
            => GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character;

        internal int RequiredRuntimeSlot => requiredRuntimeSlot;

        public void SetRequiredRuntimeSlot(int runtimeSlot)
        {
            requiredRuntimeSlot = runtimeSlot;
        }

        internal void ClearRequiredRuntimeSlot()
        {
            requiredRuntimeSlot = -1;
        }

        /// <summary>当前对象正在执行哪一帧逻辑，以及上一帧/碰撞快照帧等辅助信息。</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>当前对象对应的 DAT 帧数据缓存。</summary>
        public LF2FrameCache FrameCache { get; protected set; }

        /// <summary>帧切换控制器。负责 wait/next/frame jump 等帧推进细节。</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>效果状态。</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>Sprite 资源引用。</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>渲染器引用。</summary>
        public LF2ObjectRenderer Renderer { get; protected set; }

        /// <summary>成功注册后所属的战斗世界。</summary>
        private SimulationWorld registeredWorld;
        private RuntimeCharacterConfigResolver selfCheckCharacterConfigResolver;

        public SimulationWorld Match => registeredWorld ?? SimulationTickDriver.Instance?.World;
        internal SimulationWorld RegisteredWorldForSimulation => registeredWorld;

        void ILF2FrameCacheObserver.OnFrameCacheIdentityChanged()
        {
            PublishIdentityMetadataForSimulation();
        }

        private void PublishIdentityMetadataForSimulation()
        {
            InvalidateDataObjectTypeTickCache();
            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            Runtime.ObjType = ResolveReferenceRuntimeObjTypeFromDataType(currentDataType);
            Runtime.EntityType = currentDataType;
            registeredWorld?.IdentityWriter.SyncFromEntity(this);
        }



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
            return Match?.PpMode ?? true;
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

        /// <summary>可选的旧版阴影 SpriteRenderer，由渲染适配器注入。</summary>
        public SpriteRenderer ShadowRenderer { get; private set; }

        /// <summary>注入可选的旧版阴影渲染器引用。</summary>
        public void SetShadowRenderer(SpriteRenderer sr)
        {
            ShadowRenderer = sr;
            Sprite?.InitializeShadow(sr);
        }

        /// <summary>更新阴影位置和显示状态。</summary>
        public void UpdateShadow(int renderFrame = 0)
        {
            if (Runtime == null) return;

            bool hide = ShouldHideShadowForPresentation(
                Frame?.D,
                Runtime.LinkState,
                ObjectId,
                Runtime.HitStop);

            if (hide)
                Sprite?.HideShadow();
            else
                Sprite?.ShowShadow();

            if (ShadowRenderer == null)
                return;

            // A sorting layer wins over sortingOrder in Unity. Keep shadows in the
            // same layer as entities and sparks so the compact presentation order
            // can interleave Shadow(A), Entity(A), Shadow(B), Entity(B).
            ShadowRenderer.sortingLayerName = "Object";
            if (Sprite == null)
                ShadowRenderer.enabled = !hide;
            if (!hide)
            {
                ShadowRenderer.sortingOrder = GetPresentationRenderSortingOrder(
                    SimulationWorld.PresentationShadowSubOrder);
                var t = ShadowRenderer.transform;

                // C# 基准工程先计算阴影绘制矩形：
                // left = x + renderOffsetX - cameraX - shadowW / 2
                // top  = z - shadowH / 2
                // Unity shadow uses a center pivot, so converting the rect back
                // to its center cancels shadowW/shadowH exactly. Keep this fixed
                // center-pivot contract independent of runtime Sprite metrics.
                int cameraX = Match?.ReleaseCameraX ?? 0;
                int renderOffsetX = (int)GetRenderOffsetX();
                float shadowCenterX = GetRuntimeXInt() + renderOffsetX - cameraX;
                float shadowCenterY = GetRenderZInt();
                Vector3 worldPos = NTSDRenderSpace.ScreenPixelToWorld(shadowCenterX, shadowCenterY, t.position.z);
                t.position = NTSDRenderSpace.SnapWorldPosition(worldPos);
            }

            Match?.RecordLegacyShadowProbe(this, ShadowRenderer);
        }

        internal void UpdateShadowManagedState()
        {
            if (Runtime == null)
                return;

            bool hide = ShouldHideShadowForPresentation(
                Frame?.D,
                Runtime.LinkState,
                ObjectId,
                Runtime.HitStop);
            Sprite?.SetShadowVisibleManagedOnly(!hide);
        }

        internal static bool ShouldHideShadowForPresentation(
            LF2FrameData currentFrame,
            int linkState,
            int objectId,
            int hitStop)
        {
            int state = currentFrame?.state ?? -1;
            return currentFrame == null
                || state == 3005
                || state == 9997
                || linkState < 0
                || objectId == 223
                || objectId == 224
                || !LF2ObjectRenderer.ShouldDrawShadowForHitStop(hitStop);
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

        internal int GetHitRecordLastAdvanceTickForSnapshot(int slotIndex)
            => _hitRecordLastAdvanceTick[slotIndex];

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

        internal void AdvanceHitRecordFromPresentation(int slotIndex, int expectedAge)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount ||
                _hitRecordDamage[slotIndex] != expectedAge)
            {
                return;
            }

            _hitRecordDamage[slotIndex]++;
        }

        internal bool RemoveHitRecordTailFromPresentation(
            int slotIndex,
            int expectedCount,
            int expectedAge)
        {
            if (HitRecordCount != expectedCount ||
                slotIndex != HitRecordCount - 1 ||
                slotIndex < 0 ||
                _hitRecordDamage[slotIndex] != expectedAge)
            {
                return false;
            }

            RemoveHitRecord(slotIndex);
            return true;
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
            Runtime.Dir = nextDir;
            if (PS != null)
                PS.dir = nextDir;
            Sprite?.SwitchLR(nextDir);
        }

        public virtual int Dirh() => Runtime?.IsFacingLeft == true ? -1 : 1;

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
        public void SetPos(double x, double y, double z)
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
                if (task == null)
                    break;
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
            return fallbackRng.NextInt(minInclusive, maxExclusive);
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

            if (RegisteredWorldForSimulation?.BoundaryWriter
                    .TryApplyKind14DirectionalBlock(attacker, this) == true)
            {
                return;
            }

            int attackerX = attacker.Runtime.XInt;
            int attackerZ = attacker.Runtime.ZInt;
            int victimX = Runtime.XInt;
            int victimZ = Runtime.ZInt;

            if (attackerX > victimX + 5 && (Runtime.Vx > 0.0 || KnockbackVx > 0.0))
                Runtime.XBoundPositive = true;
            else if (attackerX < victimX - 5 && (Runtime.Vx < 0.0 || KnockbackVx < 0.0))
                Runtime.XBoundNegative = true;

            if (attackerZ > victimZ + 2 && (Runtime.Vz > 0.0 || KnockbackVz > 0.0))
                Runtime.ZBoundPositive = true;
            else if (attackerZ < victimZ - 2 && (Runtime.Vz < 0.0 || KnockbackVz < 0.0))
                Runtime.ZBoundNegative = true;
        }

        /// <summary>立即写入指定帧，绕过 wait 推进。</summary>
        // 这是最直接的硬切帧入口：
        // 当前帧会立刻变成目标帧，不等待 FrameTransistor 下一拍再处理。
        public virtual void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;
            if (FrameCache?.HasFrame(frameId) != true) return;
            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            WriteCurrentFrameId(frameId);
            Frame.D = targetFrame;
            AttackingCounter = 0;

            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
        }

        /// <summary>
        /// 写入当前逻辑帧的唯一兼容入口。权威 C# 只有一个 Entity.Frame；
        /// Unity 迁移期的 Frame.N 与 Runtime.Frame 必须在同一写入点立即一致。
        /// </summary>
        protected internal void WriteCurrentFrameId(int frameId)
        {
            if (Frame == null)
                return;

            Frame.N = frameId;
            if (Runtime != null)
                Runtime.Frame = frameId;
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
            registeredWorld?.Unregister(this);
        }

        /// <summary>销毁当前对象的可视表现。</summary>
        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>FrameTransistor 检测到 next=1000 时调用，子类可实现销毁逻辑。</summary>
        public virtual void OnTransitDestroy()
        {
            SimulationWorld world = registeredWorld;
            if (world != null)
            {
                world.StructuralWriter.Destroy(this);
                return;
            }

            DestroyEntityLikeExeCoreForStructuralWriter();
        }

        internal virtual void DestroyEntityLikeExeCoreForStructuralWriter()
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
            InvalidateDataObjectTypeTickCache();
            RefreshRuntimeSnapshot();
        }

        public virtual void OnRemoved(SimContext ctx)
        {
            if (ReferenceEquals(registeredWorld, ctx?.World))
                registeredWorld = null;
            InvalidateDataObjectTypeTickCache();
            TrackerParent = null;
            Runtime.SlotIndex = -1;
        }

        internal LF2Entity ResolveTrackerParentFromRuntime()
        {
            int selfSlot = Runtime?.SlotIndex ?? -1;
            int parentSlot = Runtime?.HolderStableId ?? -1;
            if ((Runtime?.LinkState ?? 0) >= 0 || selfSlot < 0 || parentSlot < 0)
            {
                TrackerParent = null;
                return null;
            }

            LF2Entity parent = Match?.FindEntityByRuntimeSlotForQuery(parentSlot);
            if (parent == null && (TrackerParent?.Runtime?.SlotIndex ?? -1) == parentSlot)
                parent = TrackerParent;

            if (parent?.Runtime == null || parent.Runtime.LinkState <= 0 ||
                parent.Runtime.TargetSlotIndex != selfSlot)
            {
                TrackerParent = null;
                return null;
            }

            TrackerParent = parent;
            return parent;
        }

        public virtual void SimTransit(int tickIndex) { }
        public virtual void SimTU(int tickIndex) { }
        public virtual void SimPostInteraction(int tickIndex)
        {
            if (!UsesCharacterDatInteractionPhase())
                return;

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

        public virtual void RunFrameLogicBeforeAdvance()
        {
            RunCurrentDatFrameLogicBeforeAdvance();
        }

        private void RunCurrentDatFrameLogicBeforeAdvance()
        {
            int hitFa = Frame?.D?.hit_Fa ?? 0;
            if (Runtime == null || (hitFa != 1 && hitFa != 2 && hitFa != 3 && hitFa != 4 && hitFa != 5 && hitFa != 6 && hitFa != 7 && hitFa != 8 && hitFa != 9 && hitFa != 10 && hitFa != 11 && hitFa != 12 && hitFa != 13 && hitFa != 14))
                return;

            if (hitFa == 1)
            {
                RunHitFa1FrameLogic();
                return;
            }

            if (hitFa == 3)
            {
                RunHitFa3FrameLogic();
                return;
            }

            if (hitFa == 2 || hitFa == 4 || hitFa == 12 || hitFa == 14)
            {
                RunHitFa2Or4Or12Or14FrameLogic(hitFa);
                return;
            }

            if (hitFa == 10)
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 1.1f;
                else
                    Runtime.Vx += 1.1f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -30.0, 30.0);
                if (Runtime.Y > 3f)
                    Runtime.Y = 3f;

                SwitchDir(Runtime.Vx > 0f ? "right" : "left");
                Runtime.YInt = (int)Runtime.Y;
                return;
            }

            if (hitFa == 6 || hitFa == 9)
            {
                RunHitFa6Or9FrameLogic(hitFa);
                return;
            }

            if (hitFa == 8)
            {
                RunHitFa8FrameLogic();
                return;
            }

            if (hitFa == 11)
            {
                RunHitFa11FrameLogic();
                return;
            }

            if (hitFa == 13)
            {
                RunHitFa13FrameLogic();
                return;
            }

            if (hitFa == 5)
            {
                RunHitFa5FrameLogic();
                return;
            }

            RunHitFa7FrameLogic();
        }

        private void RunHitFa1FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(1);
            if (target == null || target.Health == null || target.Health.HP <= 0)
            {
                if (Health != null)
                    Health.HP = 0;
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 1);
            int selfZ = GetFrameLogicTargetZInt(this, 1);

            if (targetX > selfX)
                Runtime.Vx += 0.85f;
            if (targetX < selfX)
                Runtime.Vx -= 0.85f;
            if (targetZ > selfZ + 7)
                Runtime.Vz += 0.3f;
            if (targetZ < selfZ - 7)
                Runtime.Vz -= 0.3f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f鈫?.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 10f < target.Runtime.Y)
                    Runtime.Y += 1.2f;
                if (Runtime.Y + 10f > target.Runtime.Y)
                    Runtime.Y -= 1.2f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -13.0, 13.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.0, 2.0);
            if (Runtime.Y > 1f)
                Runtime.Y = 1f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private void RunHitFa3FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(3);
            if (target == null)
            {
                if (Health != null)
                    Health.HP = 0;

                return;
            }

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa3NoTargetDrift();
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 3);
            int selfZ = GetFrameLogicTargetZInt(this, 3);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 10)
                Runtime.Vz += 0.17f;
            if (targetZ < selfZ - 10)
                Runtime.Vz -= 0.17f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -16.0, 16.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.4, 2.4);
        }

        private void RunHitFa8FrameLogic()
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null || Match == null)
                return;

            int enemyCount = CountActiveEnemyCharacters();

            int count = 3;
            if (enemyCount > 4)
                count = (enemyCount - 3) / 2 + 3;

            if (ResolveRuntimeCharacterConfig(225)?.characterData == null)
            {
                Runtime.PendingFlushDestroy = true;
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int freeSlot = FindFirstAvailableFrameLogicSlot();
                if (freeSlot < 0)
                    break;

                double directVx = RandInt(0, 21) - 11;
                double directVy = 3.0 - RandInt(0, 24) * 0.25;
                double directVz = 3.0 - RandInt(0, 24) * 0.25;
                int ownerSlot = enemyCount > 0
                    ? FindNthActiveEnemyCharacterSlot(RandInt(0, enemyCount))
                    : GetRuntimeSlotOrNegative(this);

                OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                if (task == null)
                    break;
                task.opoint = new ObjectPoint
                {
                    oid = 225,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = Runtime.Dir;
                task.dvz = 0f;
                task.useDirectRuntimePosition = true;
                task.directX = Runtime.X;
                task.directY = Runtime.Y;
                task.directZ = Runtime.Z;
                task.useDirectVelocity = true;
                task.directVx = directVx;
                task.directVy = directVy;
                task.directVz = directVz;
                task.ownerEntityIndex = ownerSlot;
                task.requiredRuntimeSlot = freeSlot;
                FillHitFa8SpawnTask(task);
                LF2Entity spawned;
                try
                {
                    spawned = factory.CreateObjectImmediate(task);
                }
                finally
                {
                    referencePool.Recycle(task);
                }

                if (spawned == null || spawned.Runtime?.SlotIndex != freeSlot)
                    break;
            }

            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa6Or9FrameLogic(int hitFa)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null || Match == null)
                return;

            int max = hitFa == 9 ? 10 : 7;
            int maxPerLaterPass = hitFa == 9 ? 4 : 0;
            int attemptCount = 0;
            int loopCount = 0;
            int lastFreeSlot = -1;

            do
            {
                for (int enemySlot = 0;
                     enemySlot < Match.MaxRuntimeSlotsForServices;
                     enemySlot++)
                {
                    if (!(attemptCount < maxPerLaterPass || loopCount == 0))
                        continue;

                    LF2Entity target = Match.FindEntityByRuntimeSlotForQuery(enemySlot);
                    if (!IsActiveEnemyCharacter(target))
                        continue;

                    attemptCount++;
                    lastFreeSlot = FindFirstAvailableFrameLogicSlot();
                    if (lastFreeSlot < 0)
                    {
                        if (attemptCount >= max)
                            break;
                        continue;
                    }

                    int oid = hitFa == 9 ? RandInt(0, 2) + 221 : 220;
                    if (ResolveRuntimeCharacterConfig(oid)?.characterData == null)
                    {
                        if (attemptCount >= max)
                            break;
                        continue;
                    }

                    double vx;
                    double vy;
                    if (hitFa == 6)
                    {
                        vx = (target.GetRuntimeXInt() - GetRuntimeXInt()) / 50.0;
                        vy = -4.0 - RandInt(0, 4);
                    }
                    else
                    {
                        vx = RandInt(0, 21) - 11;
                        vy = -2.0 - RandInt(0, 40) * 0.1666666666666667;
                    }

                    OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                    if (task == null)
                        return;
                    task.opoint = new ObjectPoint
                    {
                        oid = oid,
                        kind = 0,
                        action = 0,
                        dvx = 0,
                        dvy = 0,
                        dvz = 0,
                        facing = 0,
                    };
                    task.parent = this;
                    task.team = Team;
                    task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                    task.z = (float)Runtime.Z;
                    task.dir = "right";
                    task.dvz = 0f;
                    task.useDirectRuntimePosition = true;
                    task.directX = Runtime.X;
                    task.directY = Runtime.Y;
                    task.directZ = Runtime.Z;
                    task.useDirectVelocity = true;
                    task.directVx = vx;
                    task.directVy = vy;
                    task.directVz = 0f;
                    task.ownerEntityIndex = enemySlot;
                    task.requiredRuntimeSlot = lastFreeSlot;
                    FillHitFa8SpawnTask(task);
                    LF2Entity spawned;
                    try
                    {
                        spawned = factory.CreateObjectImmediate(task);
                    }
                    finally
                    {
                        referencePool.Recycle(task);
                    }

                    if (spawned == null || spawned.Runtime?.SlotIndex != lastFreeSlot)
                    {
                        lastFreeSlot = -1;
                        break;
                    }

                    if (attemptCount >= max)
                        break;
                }

                loopCount++;
            } while (hitFa == 9 &&
                     attemptCount < maxPerLaterPass &&
                     attemptCount > 0 &&
                     lastFreeSlot != -1 &&
                     attemptCount < max);

            Runtime.PendingFlushDestroy = true;
        }

        private bool IsActiveEnemyCharacter(LF2Entity candidate)
        {
            return !IsDeadLikeFrameLogicTarget(candidate) &&
                   IsCharacterFrameLogicTarget(candidate) &&
                   ResolveFrameLogicRelationIdentity(candidate) !=
                   ResolveFrameLogicRelationIdentity();
        }

        private int CountActiveEnemyCharacters()
        {
            if (Match == null)
                return 0;

            int count = 0;
            for (int slot = 0; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                LF2Entity candidate = Match.FindEntityByRuntimeSlotForQuery(slot);
                if (IsActiveEnemyCharacter(candidate))
                    count++;
            }

            return count;
        }

        private int FindNthActiveEnemyCharacterSlot(int targetOrdinal)
        {
            if (Match == null || targetOrdinal < 0)
                return -1;

            int ordinal = 0;
            for (int slot = 0; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                LF2Entity candidate = Match.FindEntityByRuntimeSlotForQuery(slot);
                if (!IsActiveEnemyCharacter(candidate))
                    continue;
                if (ordinal == targetOrdinal)
                    return slot;
                ordinal++;
            }

            return -1;
        }

        private int FindFirstAvailableFrameLogicSlot()
        {
            return Match?.FindFirstFreeFrameLogicRuntimeSlot() ?? -1;
        }

        private static LF2Entity PublishFrameLogicObjectImmediate(
            LF2ObjectPointFactory factory,
            LF2ReferencePool referencePool,
            OPointCreateTask task,
            int requiredSlot)
        {
            if (factory == null || referencePool == null || task == null || requiredSlot < 0)
                return null;

            task.requiredRuntimeSlot = requiredSlot;
            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(task);
            }
            finally
            {
                referencePool.Recycle(task);
            }

            return spawned?.Runtime?.SlotIndex == requiredSlot ? spawned : null;
        }

        private void RunHitFa2Or4Or12Or14FrameLogic(int hitFa)
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(hitFa);
            NTSDEntityRuntime rawTargetRuntime = hitFa == 4 && target == null
                ? Match?.GetRawRuntimeSlotState(OwnerEntityIndex)
                : null;
            bool rawSlotTarget = rawTargetRuntime != null;

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            bool targetHasHp = target != null
                ? target.Health != null && target.Health.HP > 0
                : rawTargetRuntime != null && rawTargetRuntime.HP > 0;
            if (hitFa == 4 && targetHasHp)
            {
                int dx = (target?.GetRuntimeXInt() ?? rawTargetRuntime.XInt) - GetRuntimeXInt();
                int dy = (target?.GetRuntimeYInt() ?? rawTargetRuntime.YInt) - GetRuntimeYInt();
                int dz = (target != null ? GetFrameLogicZInt(target) : rawTargetRuntime.ZInt) - GetFrameLogicZInt(this);
                if (dx > -30 && dx < 30 && dy > 0 && dy < 80 && dz > -10 && dz < 10)
                {
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                    SetFrameTickDirect(60);
                    if (target != null)
                        target.CatchTimer = 100;
                    else
                        rawTargetRuntime.CatchTimer = 100;
                    return;
                }
            }

            if (target == null && !rawSlotTarget)
            {
                if (hitFa != 4 && Health != null)
                {
                    Health.HP = 0;
                    return;
                }

                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            int targetX = target?.GetRuntimeXInt() ?? rawTargetRuntime?.XInt ?? 0;
            int selfX = GetRuntimeXInt();
            int targetZ = target != null ? GetFrameLogicTargetZInt(target, hitFa) : rawTargetRuntime?.ZInt ?? 0;
            int selfZ = GetFrameLogicTargetZInt(this, hitFa);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 5)
                Runtime.Vz += 0.4f;
            if (targetZ < selfZ - 5)
                Runtime.Vz -= 0.4f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f鈫?.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (target != null && IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 40f < target.Runtime.Y)
                    Runtime.Y += 1f;
                if (Runtime.Y + 40f > target.Runtime.Y)
                    Runtime.Y -= 1f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            if (hitFa == 14)
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -1.5, 1.5);
            else
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();

            if (hitFa == 14)
            {
                double absVx = System.Math.Abs(Runtime.Vx);
                int curFrame = Frame?.N ?? -1;
                if (absVx >= 8f)
                {
                    if (curFrame > 40)
                        SetFrameTickDirect(curFrame - 50);
                }
                else if (curFrame < 10)
                {
                    SetFrameTickDirect(curFrame + 50);
                }
            }
        }

        private void RunHitFa7FrameLogic()
        {
            if (Match != null)
                SpawnHitFa7Clone();

            LF2Entity target = null;
            int targetSlot = Runtime.OwnerSlotIndex;
            if (Match != null && targetSlot >= 0)
                target = Match.FindEntityByRuntimeSlotForQuery(targetSlot) ??
                         Match.FindEntityByRuntimeSlotIncludingPending(targetSlot);

            bool rawSlotTarget = target == null && IsReferenceRuntimeSlot(targetSlot);
            bool valid = (target != null || rawSlotTarget) && Health != null && Health.HP > 0;
            if (valid)
            {
                int targetX = target?.GetRuntimeXInt() ?? 0;
                if (targetX > GetRuntimeXInt())
                {
                    Runtime.Vx += 0.7f;
                    Runtime.Vx += 0.7f;
                }
                else if (targetX < GetRuntimeXInt())
                {
                    Runtime.Vx -= 0.7f;
                    Runtime.Vx -= 0.7f;
                }

                int targetZ = target?.Runtime?.ZInt ?? 0;
                int selfZ = Runtime.ZInt;
                if (targetZ > selfZ + 5)
                    Runtime.Vz += 0.4f;
                if (targetZ < selfZ - 5)
                    Runtime.Vz -= 0.4f;

                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
                if (Runtime.Y > 1.4f)
                    Runtime.Y = 1.4f;
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);
            }
            else
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 2f;
                else
                    Runtime.Vx += 2f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.YInt = -25;
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }
            }

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private bool IsReferenceRuntimeSlot(int runtimeSlot)
        {
            return Match != null &&
                   runtimeSlot >= 0 &&
                   runtimeSlot < Match.MaxRuntimeSlotsForServices;
        }

        private void RunHitFa13FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null)
                return;

            int enemyCount = CountActiveEnemyCharacters();

            int freeSlot = FindFirstAvailableFrameLogicSlot();
            if (freeSlot < 0)
            {
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int spawnOid = 228;
            if (ResolveRuntimeCharacterConfig(spawnOid)?.characterData == null)
            {
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int chosenTarget = enemyCount == 0
                ? GetRuntimeSlotOrNegative(this)
                : FindNthActiveEnemyCharacterSlot(RandInt(0, enemyCount));

            int spawnYInt = Runtime.YInt + RandInt(0, 7) - 3;
            double spawnVz = 3.0 - RandInt(0, 24) * 0.25 + Runtime.Vz;
            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            if (task == null)
            {
                Runtime.PendingFlushDestroy = true;
                return;
            }
            task.opoint = new ObjectPoint
            {
                oid = spawnOid,
                kind = 0,
                action = 0,
                dvx = 0,
                dvy = 0,
                dvz = 0,
                facing = 0,
            };
            task.parent = this;
            task.team = Team;
            task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = Runtime.Dir;
            task.dvz = 0f;
            task.useDirectRuntimePosition = true;
            task.directX = Runtime.X;
            task.directY = Runtime.Y;
            task.directZ = Runtime.Z;
            task.useDirectVelocity = true;
            task.directVx = Runtime.Vx;
            task.directVy = 0.1;
            task.directVz = spawnVz;
            task.ownerEntityIndex = chosenTarget;
            FillHitFa13SpawnTask(task);
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = spawnYInt;
            task.initialRuntimeZ = Runtime.ZInt;
            PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);

            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa5FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null)
                return;

            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int allySlot = 0; allySlot < Match.MaxRuntimeSlotsForServices; allySlot++)
            {
                LF2Entity ally = Match.FindEntityByRuntimeSlotForQuery(allySlot);
                if (IsDeadLikeFrameLogicTarget(ally))
                    continue;
                if (!IsCharacterFrameLogicTarget(ally))
                    continue;
                if (ResolveFrameLogicRelationIdentity(ally) != selfTeam)
                    continue;

                int freeSlot = FindFirstAvailableFrameLogicSlot();
                if (freeSlot < 0)
                    continue;
                if (ResolveRuntimeCharacterConfig(219)?.characterData == null)
                    continue;

                OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                if (task == null)
                    break;
                task.opoint = new ObjectPoint
                {
                    oid = 219,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = "right";
                task.dvz = 0f;
                task.useDirectRuntimePosition = true;
                task.directX = Runtime.X;
                task.directY = Runtime.Y;
                task.directZ = Runtime.Z;
                task.useDirectVelocity = true;
                task.directVx = (ally.GetRuntimeXInt() - GetRuntimeXInt()) / 50.0;
                task.directVy = 0.0;
                task.directVz = 0.0;
                task.ownerEntityIndex = allySlot;
                FillHitFa13SpawnTask(task);
                task.initialRuntimeX = Runtime.XInt;
                task.initialRuntimeY = Runtime.YInt;
                task.initialRuntimeZ = Runtime.ZInt;
                PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);
            }

            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa11FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null)
                return;

            for (int i = 0; i < HitFa11SpawnCatalog.Length; i++)
            {
                var spawn = HitFa11SpawnCatalog[i];
                if (ResolveRuntimeCharacterConfig(spawn.oid)?.characterData == null)
                    continue;

                int freeSlot = FindFirstAvailableFrameLogicSlot();
                if (freeSlot < 0)
                    break;

                string spawnDir = spawn.facing == 2
                    ? Runtime.Dir
                    : spawn.facing == 0 ? "right" : "left";
                int spawnX = Runtime.XInt + spawn.xOff;
                int spawnY = Runtime.YInt + spawn.yOff;
                int spawnZ = Runtime.ZInt + spawn.zOff;
                OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                if (task == null)
                    break;
                task.opoint = new ObjectPoint
                {
                    oid = spawn.oid,
                    kind = 0,
                    action = spawn.frameId,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3(spawnX, spawnY, spawnZ);
                task.z = spawnZ;
                task.dir = spawnDir;
                task.dvz = 0f;
                task.useDirectRuntimePosition = true;
                task.directX = spawnX;
                task.directY = spawnY;
                task.directZ = spawnZ;
                task.useDirectVelocity = true;
                task.directVx = Runtime.Vx;
                task.directVy = Runtime.Vy;
                task.directVz = Runtime.Vz + spawn.vzDelta;
                FillHitFa13SpawnTask(task);
                task.initialRuntimeX = spawnX;
                task.initialRuntimeY = spawnY;
                task.initialRuntimeZ = spawnZ;
                PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);
            }

            Runtime.PendingFlushDestroy = true;
            ResolveFrameLogicTargetByHitFa(11);

            if (OwnerEntityIndex < 0)
            {
                if (Health != null)
                    Health.HP = 0;
                return;
            }

            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");

        }

        private void SpawnHitFa7Clone()
        {
            if (Match == null || FrameCache?.Wrapper?.characterData == null)
                return;

            int freeSlot = FindFirstAvailableFrameLogicSlot();
            if (freeSlot < 0)
                return;

            int cloneOid = FrameCache.Wrapper.characterId;
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null || ResolveRuntimeCharacterConfig(cloneOid)?.characterData == null)
                return;

            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            if (task == null)
                return;
            task.opoint = new ObjectPoint
            {
                oid = cloneOid,
                kind = 0,
                action = 40,
                dvx = 0,
                dvy = 0,
                dvz = 0,
                facing = 0,
            };
            task.team = Team;
            task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = "right";
            task.useDirectRuntimePosition = true;
            task.directX = Runtime.X;
            task.directY = Runtime.Y;
            task.directZ = Runtime.Z;
            task.useDirectVelocity = true;
            task.directVx = 0.0;
            task.directVy = 0.0;
            task.directVz = 0.0;
            FillHitFa13SpawnTask(task);
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = Runtime.YInt;
            task.initialRuntimeZ = Runtime.ZInt;
            PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);
        }

        private void FillHitFa13SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = (int)task.pos.x;
            task.initialRuntimeY = (int)task.pos.y;
            task.initialRuntimeZ = (int)task.pos.z;
        }

        private void FillHitFa8SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = Runtime.YInt;
            task.initialRuntimeZ = Runtime.ZInt;
        }

        private LF2Entity ResolveFrameLogicTargetByHitFa(int hitFa)
        {
            if (Match == null)
                return null;

            if (hitFa == 4)
            {
                return OwnerEntityIndex >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(OwnerEntityIndex) ??
                      Match.FindEntityByRuntimeSlotIncludingPending(OwnerEntityIndex)
                    : null;
            }

            int selfTeam = ResolveFrameLogicRelationIdentity();
            int holderTeam = -1;
            if (SpawnerEntityIndex >= 0)
            {
                LF2Entity spawner = Match.FindEntityByRuntimeSlotForQuery(SpawnerEntityIndex);
                if (spawner != null)
                    holderTeam = ResolveFrameLogicRelationIdentity(spawner);
            }

            int currentTargetSlot = OwnerEntityIndex;
            bool needScan = true;
            LF2Entity target = currentTargetSlot >= 0
                ? Match.FindEntityByRuntimeSlotForQuery(currentTargetSlot)
                : null;

            if (target != null)
            {
                bool valid = !IsDeadLikeFrameLogicTarget(target) &&
                             IsCharacterFrameLogicTarget(target) &&
                             target.GetState() != LF2States.Lying &&
                             Mathf.Abs(target.HitStun) <= 2f &&
                             ResolveFrameLogicRelationIdentity(target) != selfTeam;
                if (valid && holderTeam != ResolveFrameLogicRelationIdentity(target))
                    needScan = false;
                if (!valid)
                    target = null;
            }

            if (needScan)
            {
                int bestDist = 10000;
                int bestSlot = -1;
                for (int slot = 0;
                     slot < Match.MaxRuntimeSlotsForServices;
                     slot++)
                {
                    LF2Entity obj = Match.FindEntityByRuntimeSlotForQuery(slot);
                    if (obj == null || ReferenceEquals(obj, this))
                        continue;
                    if (IsDeadLikeFrameLogicTarget(obj))
                        continue;
                    if (!IsCharacterFrameLogicTarget(obj))
                        continue;

                    int objTeam = ResolveFrameLogicRelationIdentity(obj);
                    if (objTeam == selfTeam)
                        continue;
                    if (holderTeam >= 0 && objTeam == holderTeam)
                        continue;
                    if ((obj.GetState() == LF2States.Lying || Mathf.Abs(obj.HitStun) > 2f) && currentTargetSlot != -1)
                        continue;

                    int dist = Mathf.Abs(obj.GetRuntimeXInt() - GetRuntimeXInt()) +
                               Mathf.Abs(GetFrameLogicTargetZInt(obj, hitFa) - GetFrameLogicTargetZInt(this, hitFa));
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestSlot = GetRuntimeSlotOrNegative(obj);
                    }
                }

                OwnerEntityIndex = bestSlot;
                target = bestSlot >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(bestSlot)
                    : null;
            }

            return target;
        }

        private int ResolveFrameLogicRelationIdentity()
        {
            return ResolveFrameLogicRelationIdentity(this);
        }

        private static int ResolveFrameLogicRelationIdentity(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }

        private static bool IsCharacterFrameLogicTarget(LF2Entity entity)
        {
            return entity?.GetCurrentDataObjectType() == (int)LF2ObjectType.Character;
        }

        private static bool IsDeadLikeFrameLogicTarget(LF2Entity entity)
        {
            if (entity == null)
                return true;
            if (entity is LF2LivingObject living && living.Dead)
                return true;

            return entity.Health == null || entity.Health.HP <= 0;
        }

        private static int GetRuntimeSlotOrNegative(LF2Entity entity)
        {
            return entity?.Runtime?.SlotIndex ?? -1;
        }

        private void ApplyHitFa2Or4Or12Or14NoTargetCatch(int hitFa)
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();
        }

        private void ApplyHitFa3NoTargetDrift()
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
        }

        private void ApplyHitFa2FrameSelection()
        {
            double absVx = System.Math.Abs(Runtime.Vx);
            int curFrame = Frame?.N ?? -1;
            if (absVx > 14f)
            {
                if (curFrame != 5 && curFrame != 6)
                    SetFrameTickDirect(5);
            }
            else if (absVx > 7f)
            {
                if (curFrame != 3 && curFrame != 4)
                    SetFrameTickDirect(3);
            }
            else
            {
                if (curFrame != 1 && curFrame != 2)
                    SetFrameTickDirect(1);
            }
        }

        private static int GetFrameLogicZInt(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            if (entity.GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                entity.Runtime != null &&
                System.Math.Abs(entity.Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (int)(entity.Runtime.Z - entity.Runtime.Type3VisualZOffset);
            }

            return entity.Runtime?.ZInt ?? 0;
        }

        private static int GetFrameLogicTargetZInt(LF2Entity entity, int hitFa)
        {
            if (hitFa == 1 || hitFa == 3 || hitFa == 7 || hitFa == 12 || hitFa == 14)
                return entity?.Runtime?.ZInt ?? 0;

            return GetFrameLogicZInt(entity);
        }

        internal virtual bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame)
        {
            return frame != null &&
                   frame.hit_Fa > 0 &&
                   GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character;
        }

        internal bool SupportsPostInteractionPhase() => UsesCharacterDatInteractionPhase();

        internal bool SupportsObjectInteractionPhase() => !UsesCharacterDatInteractionPhase();

        protected bool UsesCharacterDatInteractionPhase()
            => GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;

        internal virtual bool UsesDynamicRuntimeSlot() => false;

        internal virtual bool IsStageBoundedCharacter()
            => GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;

        internal virtual bool ShouldContributeToReleaseCamera() => false;

        internal virtual void ApplyPreFrameZBounds(float zMin, float zMax)
        {
            if (Runtime == null)
                return;

            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (currentDataType == (int)LF2ObjectType.SpecialAttack)
            {
                double logicZ = Runtime.Z - Runtime.Type3VisualZOffset;
                logicZ = System.Math.Clamp(logicZ, zMin - 1.0, zMax + 1.0);
                Runtime.Z = logicZ + Runtime.Type3VisualZOffset;
            }
            else if (currentDataType == (int)LF2ObjectType.Character)
            {
                Runtime.Z = System.Math.Clamp(Runtime.Z, zMin, zMax);
            }
            else
            {
                Runtime.Z = System.Math.Clamp(Runtime.Z, zMin - 1.0, zMax + 1.0);
            }

            Runtime.ZInt = (int)Runtime.Z;
        }

        // C++ PreFrame keeps the background width separate from the phase-only character override.
        internal virtual bool ApplyPreFrameXBounds(float baseStageWidth, int xMaxOverride)
        {
            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (currentDataType == (int)LF2ObjectType.SpecialAttack)
            {
                if (Runtime.X < -300f || Runtime.X > baseStageWidth + 300f)
                {
                    FreeEntityLikeExe();
                    return true;
                }
            }
            else if (currentDataType == (int)LF2ObjectType.Character)
            {
                int slotIndex = Runtime?.SlotIndex ?? StableId;
                if (slotIndex >= 20)
                {
                    if (Runtime.X < -100f)
                        Runtime.X = -100f;
                    if (Runtime.X > baseStageWidth + 100f)
                        Runtime.X = baseStageWidth + 100f;
                }
                else
                {
                    if (RelationTeam == 5)
                    {
                        if (Runtime.X < -300f)
                            Runtime.X = -300f;
                    }
                    else if (Runtime.X < 0f)
                    {
                        Runtime.X = 0f;
                    }

                    if (Runtime.X > baseStageWidth)
                        Runtime.X = baseStageWidth;

                    if (xMaxOverride > 0 &&
                        Runtime.X > xMaxOverride &&
                        RelationTeam != 5 &&
                        HitStun == 0)
                    {
                        Runtime.X = xMaxOverride;
                    }
                }
            }
            else if ((ObjectId == 122 || ObjectId == 123) && Unk344 > 0)
            {
                if (Runtime.X < 10f)
                    Runtime.X = 10f;
                if (Runtime.X > baseStageWidth - 10f)
                    Runtime.X = baseStageWidth - 10f;
            }
            else if (Runtime.YInt == 0 && (Runtime.X < 0f || Runtime.X > baseStageWidth))
            {
                FreeEntityLikeExe();
                return true;
            }

            Runtime.XInt = (int)Runtime.X;
            return false;
        }

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

        internal virtual void RunPreCollisionRecoveryPhase(int tickIndex)
        {
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character || Health == null)
                return;

            BattleFlowRuntimeState flow = Match?.Runtime?.Flow;
            bool stepWaitGate = flow != null && flow.BattleStepMode == 1 && flow.BattleStepGate != 1;
            bool period12 = tickIndex % NTSDGlobal.Gameplay.HpRecoverPeriod == 0;
            if (Health.HP > 0 && Health.HP < Health.HPBound && period12 && !stepWaitGate)
                Health.HP++;

            if (WeaponCount < 0 && period12 && !stepWaitGate)
            {
                int injury = NTSDGlobal.Gameplay.NegativeWeaponCountInjury;
                if (FallDamageDiv > 0)
                    injury = NTSDGlobal.Gameplay.NegativeWeaponCountScaledInjury / FallDamageDiv;

                Health.HP -= injury;
                Health.HPBound -= injury / NTSDGlobal.Gameplay.NegativeWeaponCountHpBoundDivisor;
                if (Health.HP < 0)
                    Health.HP = 0;
                if (Health.HPBound < 0)
                    Health.HPBound = 0;
                ComboCountVic += 9;
            }

            if (tickIndex % NTSDGlobal.Gameplay.PpRecoverPeriod != 0)
                return;
            if (KillCount != -1 && Health.PP >= NTSDGlobal.Gameplay.PpRecoverLowLimit)
                return;
            if (Health.PP >= NTSDGlobal.Gameplay.PpRecoverCap || HitStun < 0 || stepWaitGate)
                return;

            int hpForRate = System.Math.Min(Health.HP, NTSDGlobal.Gameplay.PpRecoverCap);
            if (ObjectId == 51 || ObjectId == 52)
                hpForRate /= 2;

            Health.PP += ((NTSDGlobal.Gameplay.PpRecoverCap - hpForRate) /
                          NTSDGlobal.Gameplay.PpRecoverHpRateDivisor) + 1;
        }

        /// <summary>
        /// 冷却递减后的输入消费阶段。
        /// 参考 C# 基准工程这里按当前 DAT `ObjType == 0` 分发角色输入；
        /// Unity 当前由 `LF2Character` 覆盖完整角色输入链；
        /// 对于“当前 DAT 已是 Character，但 CLR 运行时实例不是 LF2Character”的实体，
        /// 这里至少要补齐共享输入快照、基础 combo/direct frame jump，
        /// 以及不依赖完整角色 resolver 的 standing/walking 三个基础动作入口。
        /// </summary>
        internal virtual void RunHumanInputPollPhase(int tickIndex)
        {
            if (Runtime == null || AiControlled)
                return;

            UpdateSharedRuntimeInputSnapshotForSimulation(tickIndex);
        }

        internal virtual void ClearBattleEntryInputState()
        {
            if (registeredWorld != null)
                registeredWorld.CharacterInputWriter.ResetInputState(Runtime);
            else
                Runtime?.ResetInputState();
            sharedCharacterDatInputModule.Reset();
        }

        internal virtual void RunCharacterInputPhase(int tickIndex)
        {
            if (Runtime == null || Runtime.LinkState < 0)
                return;

            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            RunCharacterInputPhaseForKnownCharacterDat(tickIndex);
        }

        internal virtual void RunCharacterInputPhaseForKnownCharacterDat(int tickIndex)
        {
            if (Runtime == null || Runtime.LinkState < 0)
                return;

            if (AiControlled)
                Match?.PrepareAiInputBasic(this, tickIndex);

            if (this is LF2Character)
                return;

            RunSharedCharacterDatFrameJumpInputPhase();
            RunSharedCharacterDatStandingActionInputPhase();
            ApplyNonCharacterFrameVelocityForFrameAdvance();
        }

        /// <summary>
        /// Combined compatibility entry for focused resolver self-checks. Production ticks call
        /// RunHumanInputPollPhase and RunCharacterInputPhase at separate C# authority phases.
        /// </summary>
        internal virtual void RunPostCooldownInputPhase(int tickIndex)
        {
            if (!AiControlled)
                RunHumanInputPollPhase(tickIndex);
            RunCharacterInputPhase(tickIndex);
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
            SimInputBuffer inputBuffer = null;
            if (TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
            {
                inputBuffer = controller.InputBuffer;
                sharedCharacterDatInputModule.SyncProgressFromRuntime(Runtime);
            }
            else
            {
                // A controllerless shared shell can still receive the complete current-tick
                // packet through runtime fields. Seed the local adapter from that authority
                // snapshot; controller-backed sparse input must keep its private held mirror.
                sharedCharacterDatInputModule.SyncFromRuntime(Runtime);
            }
            sharedCharacterDatInputModule.PollFromBuffer(
                inputBuffer,
                tickIndex,
                this);
        }

        private void RunSharedCharacterDatFrameJumpInputPhase()
        {
            if (Runtime == null)
                return;

            if (AiControlled && registeredWorld != null)
            {
                registeredWorld.CharacterInputActionResolver.ApplyFrameInputFromRuntimeProgress(
                    this,
                    registeredWorld.CharacterInputWriter,
                    registeredWorld.ActiveBattleAiInputDetailDiagnosticsForDiagnostics);
                return;
            }

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
            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            if (HitConfirmCounter > 0 &&
                linkState == 0 &&
                FrameCache?.HasFrame(LF2StandardFrames.SuperPunch) == true &&
                TryCharacterDatInputFrameJump(LF2StandardFrames.SuperPunch))
            {
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

            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.Jumping);
        }

        private bool TryRunSharedCharacterDatStandingDefendAction()
        {
            if (!IsSharedCharacterDatDefendInputReadyInternal(requireDefendLockOpen: true))
                return false;

            Runtime.AnimSub = 0;
            AttackingCounter = 0;
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
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.HeavyWeaponThw);
            }

            return true;
        }

        private void ApplySharedCharacterDatWalkRunMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null || Runtime.YInt != 0)
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
            if (characterData == null || Runtime == null || Runtime.YInt != 0)
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
            if ((Frame?.D?.state ?? -1) != LF2States.Jump || Runtime.YInt >= 0)
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

                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.JumpAttack);
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if (linkState % 100 == 1)
            {
                AttackingCounter = 0;
                SetSharedCharacterDatInputFrameDirect(
                    hasDirection ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
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
        /// 当前补 stop-running、run attack、running defend、running jump，
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
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.HeavyWeaponThw);

                return true;
            }

            ApplySharedCharacterDatRunningMovement();

            if (IsSharedCharacterDatAttackInputReadyInternal())
            {
                int linkState = Runtime.LinkState;
                bool hasDirection = HasAnyDirectionInputForSharedCharacterDat();

                if (linkState % 100 == 1)
                {
                    SetSharedCharacterDatInputFrameDirect(
                        hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.RunWeaponAtck);
                }
                else if (linkState == 4)
                {
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.LightWeaponThw);
                }
                else if (linkState == 6)
                {
                    SetSharedCharacterDatInputFrameDirect(
                        hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.SkyLgtWpThw);
                }
                else if (TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.RunAttack))
                {
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.RunAttack);
                }
            }

            if (IsSharedCharacterDatDefendInputReadyInternal())
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.Rowing2);

            if (IsSharedCharacterDatJumpInputReadyInternal())
            {
                LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
                if (characterData == null)
                    return true;

                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashForward);
                Runtime.AnimSub = 0;
                Runtime.Vx = Runtime.Dir == "right"
                    ? characterData.dash_distance
                    : -characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
            }

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
                Runtime.Vx *= 5.0 / 6.0;
            }
            else if (downPressed)
            {
                Runtime.Vz = characterData.heavy_running_speedz;
                Runtime.Vx *= 5.0 / 6.0;
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
            {
                Runtime.Vz = -speedZ;
                Runtime.Vx *= 5.0 / 6.0;
            }
            else if (downPressed && !upPressed)
            {
                Runtime.Vz = speedZ;
                Runtime.Vx *= 5.0 / 6.0;
            }
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
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.Rowing2);
                handled = true;
            }

            bool jumpReady = IsSharedCharacterDatJumpInputReadyInternal();
            bool rightPressed = Runtime.KeyRight != 0;
            bool leftPressed = Runtime.KeyLeft != 0;

            if ((rightPressed || Runtime.Vx > 0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(
                    Runtime.Dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                Runtime.AnimSub = 0;
                Runtime.Vx = characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                handled = true;
            }
            else if ((leftPressed || Runtime.Vx < -0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(
                    Runtime.Dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
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
            SetSharedCharacterDatInputFrameDirect(
                backward ? LF2StandardFrames.Rowing : LF2StandardFrames.RowingBack);
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

                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashAttack);
                return true;
            }

            if (linkState % 100 == 1)
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashWeaponAtck);
                Runtime.Vy -= 1f;
                AttackingCounter = 0;
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if ((linkState == 4 || linkState == 6) && hasDirection)
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.SkyLgtWpThw);
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

            WriteCurrentFrameId(frameId);
            Runtime.FrameWaitCounter = 0;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            Runtime.NextFrame = Frame.D.next;
        }

        private bool SetSharedCharacterDatInputFrameDirect(int frameId)
        {
            if (Frame == null || FrameCache == null || Runtime == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            WriteCurrentFrameId(frameId);
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            Runtime.NextFrame = targetFrame.next;
            return true;
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
            return TransformOriginalObjectId == -1 && Runtime.LinkState != 2;
        }

        /// <summary>
        /// 通用输入跳帧入口。
        /// 参考 C# `DoFrameJump(...)`，用于当前 DAT 已经是 Character 的任意实体。
        /// </summary>
        internal bool TryCharacterDatInputFrameJump(int frameId)
        {
            SimulationWorld world = RegisteredWorldForSimulation;
            return world != null
                ? world.CharacterActionWriter.TryCharacterDatInputFrameJump(this, frameId)
                : TryCharacterDatInputFrameJumpCompatibility(frameId);
        }

        internal bool TryCharacterDatInputFrameJumpCompatibility(int frameId)
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

            return SetSharedCharacterDatInputFrameDirect(frameId);
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

            if (registeredWorld != null)
                registeredWorld.CharacterInputWriter.ClearInputHistoryTail(Runtime);
            else
                Runtime?.ClearInputHistoryTail();

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            int slotIndex = Runtime?.SlotIndex ?? -1;
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            if (task == null)
                return;
            ConfigureLateN30SpawnTask(task, slotIndex, frameVal);

            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(task);
            }
            finally
            {
                LF2ReferencePool.Instance.Recycle(task);
            }
            if (spawned == null)
                return;

            ApplyLateN30HistoryGateBroadcast(frameVal, spawned);
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
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;
        }

        /// <summary>
        /// N30 晚阶段除了生成 998 效果外，100 写入同 Unk364 角色的
        /// 随机坐标，102 打开 history gate，104 关闭 history gate。
        /// </summary>
        internal void ApplyLateN30HistoryGateBroadcast(int frameVal, LF2Entity spawned = null)
        {
            if (frameVal != 100 && frameVal != 102 && frameVal != 104)
                return;

            SimulationWorld world = Match;
            if (world == null)
                return;

            int sourceTeam = frameVal == 100 ? RelationTeam : ResolveN30HistoryGateTeam(this);
            if (sourceTeam == 0 && frameVal != 100)
                return;

            // C# authority writes the spawned effect's integer coordinates, then
            // consumes exactly two RNG values for every eligible same-Unk364
            // living character when triggerCode=100.
            int spawnX = spawned?.Runtime?.XInt ?? Runtime?.XInt ?? 0;
            int spawnZ = spawned?.Runtime?.ZInt ?? Runtime?.ZInt ?? 0;

            bool enabled = frameVal == 102;
            for (int slot = 0;
                 slot < world.MaxRuntimeSlotsForServices;
                 slot++)
            {
                LF2Entity teammate = world.FindEntityByRuntimeSlotForQuery(slot);
                if (teammate == null || teammate.Runtime == null || teammate.Health == null)
                    continue;
                if (teammate.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;
                if (teammate.Health.HP <= 0)
                    continue;
                int teammateTeam = frameVal == 100 ? teammate.RelationTeam : ResolveN30HistoryGateTeam(teammate);
                if (teammateTeam != sourceTeam)
                    continue;

                if (frameVal == 100)
                {
                    int targetX = spawnX + (world.Rng.NextRaw() % 0x51) - 0x28;
                    int targetZ = spawnZ + (world.Rng.NextRaw() % 0x51) - 0x28;
                    world.AiInputWriter.SetCoordinateTarget(
                        teammate.Runtime,
                        targetX,
                        targetZ);
                }
                else
                {
                    world.CharacterInputWriter.SetInputHistoryGate(
                        teammate.Runtime,
                        enabled);
                }
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

        internal bool RunEarlyTeleportSpecialsPhaseWithMutationReport(
            System.Collections.Generic.List<LF2Entity> entities,
            bool frameToggleGate)
        {
            // LF2Character's production override delegates directly to this base
            // implementation. Unknown derived shells remain fail-closed because an
            // override may add writes outside the teleport runtime fields.
            bool canReportExactly = GetType() == typeof(LF2Character);
            int state = Frame?.D?.state ?? -1;
            bool stalePublishedSnapshot =
                canReportExactly &&
                !IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp();
            bool writesTeleportRuntime =
                canReportExactly &&
                !frameToggleGate &&
                entities != null &&
                Health != null &&
                (state == LF2States.TeleportToEnemy ||
                 state == LF2States.TeleportToTeammate);

            RunEarlyTeleportSpecialsPhase(entities, frameToggleGate);
            return !canReportExactly ||
                   stalePublishedSnapshot ||
                   writesTeleportRuntime;
        }

        internal virtual void RunLateDeathOpointPreCleanupPhase()
        {
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;
            if (Health == null || Health.HP > 0 || Runtime == null)
                return;

            DropHeldObjectForCurrentDatDeath();

            int frameId = Frame?.N ?? -1;
            if (frameId < 12 || frameId == 110 || frameId == 111)
                EnterCurrentDatDeathBounceFrame();

            if (Runtime.YInt == 0 && Runtime.Y == 0.0 && Runtime.Vy == 0.0 && KnockbackVy == 0.0)
            {
                int currentFrame = Frame?.N ?? -1;
                bool groundDeathFrame =
                    (currentFrame >= 180 && currentFrame <= 189 && currentFrame != 184) ||
                    (currentFrame >= 212 && currentFrame <= 214);
                if (groundDeathFrame)
                    EnterCurrentDatDeathBounceFrame();
            }
        }

        internal virtual bool TryRunLatePostOpointCleanupPhase()
        {
            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character || Runtime == null ||
                Runtime.WeaponFlightCounter >= 0)
            {
                return false;
            }

            Runtime.WeaponFlightCounter = 0;
            QueueBattleSound(FrameCache?.Wrapper?.characterData?.weapon_broken_sound);
            Runtime.PendingFlushDestroy = true;
            return true;
        }

        private void DropHeldObjectForCurrentDatDeath()
        {
            if (this is LF2Character character)
            {
                character.ForceDropHeldWeaponForLateDeathInternal();
                return;
            }

            int holderSlot = Runtime?.SlotIndex ?? -1;
            int heldSlot = Runtime?.ResolveActiveHeldSlotIndex() ?? -1;
            LF2Entity held = heldSlot >= 0
                ? Match?.FindEntityByRuntimeSlotForQuery(heldSlot) ??
                  Match?.FindEntityByRuntimeSlotIncludingPending(heldSlot)
                : null;

            Runtime.LinkState = 0;
            Runtime.TargetSlotIndex = -1;
            Runtime.HeldWeaponStableId = -1;
            if (held?.Runtime == null || held.Runtime.HolderStableId != holderSlot)
                return;

            held.Runtime.LinkState = 0;
            held.Runtime.HolderStableId = -1;
            held.HolderCopySlot = 99;
        }

        private void EnterCurrentDatDeathBounceFrame()
        {
            DirectWriteRawFramePreserveWaitCounter(186);
            Runtime.Vy = -3.0;
            KnockbackVy = -3.0;
            Runtime.Y = -1.0;
            Runtime.YInt = -1;
        }

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
            bool shouldSpawnBranch1 =
                (prevState == 13 || (Frame?.Prev ?? 0) == 200) &&
                currentState != 13 && (Frame?.N ?? 0) != 200;
            bool shouldSpawnBranch2 = prevState == 18 || prevState == 19;
            if (!shouldSpawnBranch1 && !shouldSpawnBranch2)
                return;

            bool spawned = false;
            bool hasEffectResources = LF2ObjectPointFactory.Instance != null &&
                                      ResolveRuntimeCharacterConfig(999) != null;
            int availableSlots = 0;
            bool availableSlotsCalculated = false;

            if (hasEffectResources && shouldSpawnBranch1)
            {
                availableSlots = CountAvailableTransitionEffectSlots();
                availableSlotsCalculated = true;
                Match?.QueueSound("SFX_066", Runtime.XInt);
                spawned |= SpawnTransitionEffectBranch1(ref availableSlots);
            }

            if (!shouldSpawnBranch2)
                return;

            int count = 0;
            if (currentState != 18 && currentState != 19)
                count = 7;
            else if (BattleRandInt(0, 4) == 0)
                count = 1;

            if (count > 0)
            {
                if (hasEffectResources && !availableSlotsCalculated)
                {
                    availableSlots = CountAvailableTransitionEffectSlots();
                    availableSlotsCalculated = true;
                }

                spawned |= SpawnTransitionEffectBranch2(count, ref availableSlots);
            }

            if (spawned)
            {
                if (Match == null)
                    RefreshRuntimeSnapshot();
                else
                    Match.RefreshLateTransitionRuntimeSnapshot(this);
            }
        }

        private bool SpawnTransitionEffectBranch1(ref int availableSlots)
        {
            int initialSlots = availableSlots;
            for (int n = 0; n < 15; n++)
            {
                if (availableSlots <= 0)
                    break;

                double y = Runtime.Y - BattleRandInt(0, 29);
                double x = Runtime.X + BattleRandInt(0, 39) - 19.0;
                double vy = -(BattleRandInt(0, 20) / 2.0) - 8.0;
                double vx = Runtime.Vx * 0.5 + BattleRandInt(0, 11) - 5.0;
                int frameId = n < 2 ? 120 : n < 5 ? 130 : n < 9 ? 125 : 135;
                SpawnTransitionEffect(
                    frameId,
                    x,
                    y,
                    vx,
                    vy);
                availableSlots--;
            }

            return availableSlots < initialSlots;
        }

        private bool SpawnTransitionEffectBranch2(int count, ref int availableSlots)
        {
            int initialSlots = availableSlots;
            for (int n = 0; n < count; n++)
            {
                if (availableSlots <= 0)
                    break;

                double y = Runtime.Y - BattleRandInt(0, 29);
                double x = Runtime.X + BattleRandInt(0, 59) - 29.0;
                double vx = Runtime.Vx + BattleRandInt(0, 11) - 5.0;
                int frameId = 140 + BattleRandInt(0, 1);
                SpawnTransitionEffect(
                    frameId,
                    x,
                    y,
                    vx,
                    -1.0);
                availableSlots--;
            }

            return availableSlots < initialSlots;
        }

        private int CountAvailableTransitionEffectSlots()
        {
            if (Match == null)
                return 350;

            int available = 0;
            for (int slot = Match.DynamicRuntimeSlotStartForServices;
                 slot < Match.MaxRuntimeSlotsForServices;
                 slot++)
            {
                if (Match.FindEntityByRuntimeSlotForQuery(slot) == null)
                    available++;
            }

            return available;
        }

        private void SpawnTransitionEffect(int frameId, double x, double y, double vx, double vy)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            if (task == null)
                return;
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
            task.pos = new Vector3((float)x, (float)y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = Runtime.Dir;
            task.useDirectRuntimePosition = true;
            task.directX = x;
            task.directY = y;
            task.directZ = Runtime.Z;
            task.useDirectVelocity = true;
            task.directVx = vx;
            task.directVy = vy;
            task.directVz = 0.0;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.TransitionEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = Runtime.YInt;
            task.initialRuntimeZ = Runtime.ZInt;
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;

            factory.EnqueueCreateObject(task);
        }

        public virtual void FreeEntityLikeExe()
        {
            SimulationWorld world = registeredWorld;
            if (world != null)
            {
                world.StructuralWriter.Free(this);
                return;
            }

            FreeEntityLikeExeCoreForStructuralWriter();
        }

        internal void FreeEntityLikeExeCoreForStructuralWriter()
        {
            Sprite?.Hide();
            Sprite?.HideShadow();
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

        public virtual void DirectWriteFramePreserveWaitCounter(int frameId)
        {
            SetFrameTickDirect(frameId);
        }

        internal void DirectWriteRawFramePreserveWaitCounter(int frameId)
        {
            if (Frame == null)
                return;

            WriteCurrentFrameId(frameId);
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
        }

        internal void DirectWriteHeldFramePreserveWaitCounter(int frameId)
        {
            if (Frame == null)
                return;

            WriteCurrentFrameId(frameId);
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
        }

        public virtual void DirectWriteFrameImmediateWaitReset(int frameId)
        {
            SetFrameTickImmediateRawDirect(frameId);
        }

        internal void SetFrameLogicRawFramePreserveAttacking(int frameId)
        {
            SetFrameTickDirect(frameId);
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
            Runtime.WeaponFlightCounter = wrapper.characterData?.weapon_hp ?? 0;
            DirectWriteRawFramePreserveWaitCounter(0);

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            if (applyHitStop140)
                HitStun = 140;

            RefreshRuntimeSnapshot();
        }

        internal void SetRuntimeCharacterConfigResolverForSelfCheck(
            RuntimeCharacterConfigResolver resolver)
        {
            selfCheckCharacterConfigResolver = resolver;
        }

        internal RuntimeCharacterConfigResolver
            RuntimeCharacterConfigResolverForSelfCheck =>
                selfCheckCharacterConfigResolver;

        internal LF2CharacterDataWrapper ResolveRuntimeCharacterConfig(int targetObjectId)
        {
            RuntimeCharacterConfigResolver resolver =
                selfCheckCharacterConfigResolver ??
                registeredWorld?.RuntimeCharacterConfigs;
            if (resolver != null)
                return resolver.Resolve(targetObjectId);

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

            WriteCurrentFrameId(targetFrameId);
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

            int wrapperOid = ResolveCurrentDataObjectId(entity);
            int fallbackType = entity.ReleaseEntityType;
            SimulationWorld world = entity.registeredWorld;
            int activeTick = world?.ActiveDataObjectTypeCacheTick ?? -1;
            if (activeTick >= 0 &&
                ReferenceEquals(entity.dataObjectTypeCacheWorld, world) &&
                entity.dataObjectTypeCacheTick == activeTick &&
                entity.dataObjectTypeCacheObjectId == wrapperOid &&
                entity.dataObjectTypeCacheFallback == fallbackType)
            {
                return entity.dataObjectTypeCacheDefinition?.type ?? fallbackType;
            }

            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(wrapperOid);
            if (activeTick >= 0)
            {
                entity.dataObjectTypeCacheWorld = world;
                entity.dataObjectTypeCacheTick = activeTick;
                entity.dataObjectTypeCacheObjectId = wrapperOid;
                entity.dataObjectTypeCacheDefinition = definition;
                entity.dataObjectTypeCacheFallback = fallbackType;
            }

            return definition?.type ?? fallbackType;
        }

        private void InvalidateDataObjectTypeTickCache()
        {
            dataObjectTypeCacheWorld = null;
            dataObjectTypeCacheTick = -1;
            dataObjectTypeCacheObjectId = -1;
            dataObjectTypeCacheDefinition = null;
            dataObjectTypeCacheFallback = 0;
        }

        /// <summary>
        /// 按当前 DAT 包装器解析对象 oid；没有当前包装器时回退到实体的正式 runtime 身份。
        /// </summary>
        public static int ResolveCurrentDataObjectId(LF2Entity entity)
        {
            return entity?.FrameCache?.Wrapper?.characterId ?? entity?.ObjectId ?? -1;
        }

        public virtual bool ShouldDeferInitialRuntimeSnapshot() => false;

        public virtual LF2FrameData GetCollisionFrameData()
        {
            if (Frame == null || FrameCache == null)
                return null;

            if (FrameCache.HasFrame(Frame.Prev2) && Frame.Prev2D != null)
                return Frame.Prev2D;

            if (FrameCache.HasFrame(Frame.N) && Frame.D != null)
                return Frame.D;

            return null;
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
            return GetDisplayZForCurrentDataType(GetCurrentDataObjectType());
        }

        internal float GetDisplayZForCurrentDataType(int currentDataObjectType)
        {
            if (currentDataObjectType == (int)LF2ObjectType.SpecialAttack &&
                Runtime != null &&
                System.Math.Abs(Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (float)(Runtime.Z - Runtime.Type3VisualZOffset);
            }

            return GetRenderZInt();
        }

        public virtual int GetRenderSortingOrder()
        {
            return GetPresentationRenderSortingOrder(SimulationWorld.PresentationEntitySubOrder);
        }

        public int GetHitRecordRenderSortingOrder()
        {
            return GetPresentationRenderSortingOrder(SimulationWorld.PresentationHitRecordSubOrder);
        }

        private int GetPresentationRenderSortingOrder(int subOrder)
        {
            return Match != null
                ? Match.GetPresentationRenderSortingOrder(this, subOrder)
                : subOrder;
        }

        /// <summary>
        /// Renderer-facing entity sub-order. draw_entity position may use its
        /// display Z offset, while release draw ordering remains ZInt/slot.
        /// </summary>
        public int GetDisplayRenderSortingOrder(float displayZ, float zOffset)
        {
            return GetRenderSortingOrder();
        }

        public virtual float GetSpriteWidthPxForRender()
        {
            float width = TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
                ? entry.PixelWidth
                : 0f;
            if (width <= 0f)
                width = GetSpriteWidthPxForCollision();
            return width;
        }

        public virtual float GetSpriteHeightPxForRender()
        {
            return TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
                ? entry.PixelHeight
                : 0f;
        }

        public bool TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
        {
            entry = null;
            int effectivePic = GetRenderPicIndex();
            if (effectivePic < 0 || effectivePic == 999)
                return false;

            int visualDataId = ResolveCurrentDataObjectId(this);
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            return manager != null &&
                   manager.TryGetSpriteEntry(visualDataId, effectivePic, out entry);
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
            if (string.IsNullOrWhiteSpace(soundId))
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
            return TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
                ? entry.PixelWidth
                : 0f;
        }

        protected virtual bool ShouldRenderAboveCharacters()
        {
            int semantic = Runtime?.SpawnSemantic ?? 0;
            return semantic == (int)ReleaseSpawnSemantic.ImmediateEffect ||
                   semantic == (int)ReleaseSpawnSemantic.TransitionEffect;
        }

        protected internal virtual bool IsBlockedByReleaseLinkOrCaughtCpoint()
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
            requiredRuntimeSlot = -1;
            Runtime.PendingFlushDestroy = false;
            Runtime.TransformOriginalObjectId = -1;
            Runtime.TransformTargetObjectId = -1;
            Runtime.RenderOffsetX = 0f;
        }

        /// <summary>
        /// Resets the logic-only reference components that are owned for the complete
        /// lifetime of a pooled entity. Presentation bindings are deliberately excluded:
        /// an in-place formal Entity::reset (for example oid 51 splitting back to 8/7)
        /// must preserve its renderer and sprite catalog. LF2ObjectRenderer.ResetState
        /// owns the separate pool-release presentation reset.
        /// </summary>
        protected void ResetReusableRuntimeComponents()
        {
            PS?.Reset();

            if (Frame != null)
            {
                Frame.PN = 0;
                Frame.Prev = 0;
                WriteCurrentFrameId(0);
                Frame.D = null;
                Frame.Prev2 = 0;
                Frame.Prev2D = null;
            }

            Effect?.Reset();
            ItrRest?.Reset();
            Trans?.Reset();
        }

        public void ApplyInitialRuntimePosition(OPointCreateTask task)
        {
            if (task == null)
                return;

            Runtime.X = task.useDirectRuntimePosition ? task.directX : task.pos.x;
            Runtime.Y = task.useDirectRuntimePosition ? task.directY : task.pos.y;
            Runtime.Z = task.useDirectRuntimePosition ? task.directZ : task.z;

            if (task.useInitialRuntimeIntPosition)
            {
                Runtime.XInt = task.initialRuntimeX;
                Runtime.YInt = task.initialRuntimeY;
                Runtime.ZInt = task.initialRuntimeZ;
                return;
            }

            Runtime.SyncIntegerPosition();
        }

        public virtual void ApplyForcedRuntimeIntPosition(int x, int y, int z)
        {
            Runtime.XInt = x;
            Runtime.YInt = y;
            Runtime.ZInt = z;
        }

        public virtual void RunCpointCheckStep10()
        {
            Match?.CpointWriter.RunKind1(Match, this);
        }

        public virtual void RunCpointMismatchTailStep10()
        {
            Match?.CpointWriter.RunKind2Validation(Match, this);
        }

        public virtual void RunWeaponSyncHeldStep10()
        {
            Match?.CpointWriter.SyncHeldCpoint(Match, this);
        }

        public virtual void ClearHitCandidateCarriers()
        {
            HitConfirm2 = 0;
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            ApplyCpointThrowStep10(cpoint, victimEntity, null);
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity, LF2FrameData throwFrameSnapshot)
        {
            if (cpoint == null || victimEntity == null)
                return;

            LF2FrameData sourceThrowFrame = throwFrameSnapshot ?? Frame?.D;
            int sourceNextFrameId = sourceThrowFrame?.next ?? 0;
            LF2FrameData sourceNextFrame = FrameCache?.HasFrame(sourceNextFrameId) == true
                ? FrameCache.GetFrameDataById(sourceNextFrameId)
                : null;

            if (cpoint.throwinjury == -1 && HasStep10ThrowTransformVictimData(victimEntity))
            {
                ApplyCpointThrowTransformToSelfAndOwnedObjects(victimEntity);
            }

            if (cpoint.throwinjury > 0)
                victimEntity.WeaponCount = cpoint.throwinjury;

            LF2FrameData throwFrame = throwFrameSnapshot ??
                FrameCache?.GetFrameDataById(Frame?.N ?? 0) ??
                Frame?.D;

            int centerX = throwFrame?.centerx ?? 0;
            int centerY = throwFrame?.centery ?? 0;
            int y = GetReleaseYInt() - centerY + cpoint.y;
            int x = Runtime.Dir == "right"
                ? GetReleaseXInt() - centerX + cpoint.x
                : centerX - cpoint.x + GetReleaseXInt();

            victimEntity.Runtime.X = x;
            victimEntity.Runtime.Y = y;
            victimEntity.Runtime.XInt = x;
            victimEntity.Runtime.YInt = y;

            int nextFrame = throwFrame?.next ?? 0;
            SetCpointRawFramePreserveWait(nextFrame, sourceNextFrame);
            SetCpointRawPrevFrame2(nextFrame, sourceNextFrame);
            AttackingCounter = 0;

            victimEntity.Runtime.Vx = Runtime.Dir == "right" ? cpoint.throwvx : -cpoint.throwvx;
            victimEntity.Runtime.Vy = cpoint.throwvy;
            SetVictimThrowVzStep10(cpoint, victimEntity);

            victimEntity.SetCpointRawFramePreserveWait(cpoint.vaction);
            victimEntity.SetCpointRawPrevFrame2(cpoint.vaction);
        }

        internal void ApplyCpointThrowTransformToSelfAndOwnedObjects(LF2Entity victimEntity)
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

            victim.Runtime.Vz = 0f;
            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
                victim.Runtime.Vz = -cpoint.throwvz;
            else if (Runtime.KeyUp == 0 && Runtime.KeyDown != 0)
                victim.Runtime.Vz = cpoint.throwvz;
        }

        internal bool HasStep10ThrowTransformVictimData(LF2Entity victimEntity)
        {
            return victimEntity?.FrameCache?.Wrapper?.characterData != null;
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
            int selfSlotIndex = Runtime?.SlotIndex ?? -1;
            if (Match == null || selfSlotIndex < 0)
                return;

            for (int slot = 0;
                 slot < Match.MaxRuntimeSlotsForServices;
                 slot++)
            {
                LF2Entity entity = Match.FindEntityByRuntimeSlotForQuery(slot);
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

            LF2FrameData victimCurrentFrame = victimEntity.Frame?.D;
            int victimCpointX = victimCurrentFrame?.cpoint?.x ?? 0;
            int victimCpointY = victimCurrentFrame?.cpoint?.y ?? 0;
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

            victimEntity.Runtime.SyncIntegerPosition();
            victimEntity.RefreshRuntimeSnapshot();
        }

        internal void SetCpointRawFramePreserveWait(int frameId)
            => SetCpointRawFramePreserveWait(frameId, null);

        internal void SetCpointRawFramePreserveWait(int frameId, LF2FrameData sourceFrame)
        {
            if (Frame == null || FrameCache == null)
                return;
            bool sourceFrameMatches = sourceFrame != null && sourceFrame.frameId == frameId;
            if (frameId >= 0 && !FrameCache.HasFrame(frameId) && !sourceFrameMatches)
                return;

            LF2FrameData targetFrame = sourceFrameMatches
                ? sourceFrame
                : FrameCache.GetFrameDataById(frameId);
            WriteCurrentFrameId(frameId);
            Frame.D = targetFrame;
            if (targetFrame != null)
                Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            RefreshRuntimeSnapshot();
        }

        internal void SetCpointRawPrevFrame2(int frameId)
            => SetCpointRawPrevFrame2(frameId, null);

        internal void SetCpointRawPrevFrame2(int frameId, LF2FrameData sourceFrame)
        {
            if (Frame == null)
                return;

            Frame.Prev2 = frameId;
            Frame.Prev2D = sourceFrame != null && sourceFrame.frameId == frameId
                ? sourceFrame
                : FrameCache?.GetFrameDataById(frameId);
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
        public virtual void OnFrameTickFrameChangedFromWaitCounter()
        {
            int frameId = Frame?.N ?? -1;
            string soundId = Frame?.D?.sound;
            if (frameId < 0 || frameId >= LF2FrameCache.MaxFrameIdExclusive || string.IsNullOrWhiteSpace(soundId))
                return;

            Match?.QueueSound(soundId, Runtime.XInt);
        }

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
        internal void RunReleaseFrameTickCounters()
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

        protected virtual void ApplyCommonCaughtExitHitStop(int previousFrameId)
        {
            LF2FrameData previousFrame = FrameCache?.GetFrameDataById(previousFrameId);
            if (previousFrame == null || previousFrame.state != LF2States.Lying)
                return;

            if ((Frame?.D?.state ?? 0) == LF2States.Frozen)
                return;

            if (RelationTeam == 5 || Unk344 != 0)
            {
                if ((Match?.Difficulty ?? 2) == 2)
                    return;

                int gameMode = Match?.BattleGameModeId ?? 0;
                bool oidSkip = (gameMode == 1 || gameMode == 4) &&
                               ObjectId / 5 == 3 &&
                               ObjectId != 38;
                if (oidSkip)
                    return;
            }

            HitStun = 15;
        }

        internal void ApplyCaughtExitHitStopForWorldPass(int previousFrameId)
        {
            ApplyCommonCaughtExitHitStop(previousFrameId);
        }

        protected virtual bool IsFrameTickLeftPressed() => Runtime?.KeyLeft != 0;

        protected virtual bool IsFrameTickRightPressed() => Runtime?.KeyRight != 0;

        protected virtual bool IsFrameTickUpPressed() => Runtime?.KeyUp != 0;

        protected virtual bool IsFrameTickDownPressed() => Runtime?.KeyDown != 0;

        protected virtual int GetFrameTickCdUp() => Runtime?.CdUp ?? 0;

        protected virtual int GetFrameTickCdDown() => Runtime?.CdDown ?? 0;

        protected virtual void ApplyFrame212JumpInit()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return;

            Runtime.Vy = characterData.jump_height;
            if (IsFrameTickRightPressed() && !IsFrameTickLeftPressed())
                Runtime.Vx = characterData.jump_distance;
            else if (IsFrameTickLeftPressed() && !IsFrameTickRightPressed())
                Runtime.Vx = -characterData.jump_distance;

            if (IsFrameTickUpPressed() && !IsFrameTickDownPressed())
                Runtime.Vz = -characterData.jump_distancez;
            else if (IsFrameTickDownPressed() && !IsFrameTickUpPressed())
                Runtime.Vz = characterData.jump_distancez;
        }

        internal void ApplyFrame212JumpInitForWorldPass()
        {
            ApplyFrame212JumpInit();
        }

        /// <summary>
        /// 对齐参考 `FrameTick` 的负 mp 帧推进后处理。
        /// 当前只收敛已确认的 PP 真值与 PpDisplay 累计面，不扩展到 HUD 刷新。
        /// </summary>
        protected void ApplyCommonFrameTickPpDisplayPostAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Health == null || !IsPpModeEnabled())
                return;
            if ((Frame?.N ?? -1) >= LF2FrameCache.MaxFrameIdExclusive)
                return;
            int mpDelta = frame.mp;
            if (mpDelta >= 0)
                return;

            if (Health.PP < mpDelta)
            {
                SetFrameTickImmediateRawDirect(frame.hit_d);
                frame = Frame?.D;
                if (frame == null)
                    return;
            }
            else
            {
                Health.PP += mpDelta;
                SpendPpDisplay(-mpDelta);
            }

            int turnNext = frame.hit_d;
            if (turnNext <= 0 || GetRuntimeYInt() != 0)
                return;

            bool left = Runtime?.KeyLeft != 0;
            bool right = Runtime?.KeyRight != 0;
            if (left && !right && Runtime?.Dir == "right")
                SetFrameTickImmediateRawDirect(turnNext);
            else if (right && !left && Runtime?.Dir == "left")
                SetFrameTickImmediateRawDirect(turnNext);
        }

        internal void ApplyFrameTickPpDisplayForWorldPass()
        {
            ApplyCommonFrameTickPpDisplayPostAdvance();
        }

        protected internal bool TryEnterReleaseFrameAdvanceAfterDelay()
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

        protected void RunSharedCharacterDatFrameAdvanceAsCharacter(int tickIndex)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return;

            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return;

            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return;

            float mass = NTSDGlobal.Default.Machanics.Mass;
            var context = new CharacterMechanicsContext(
                Runtime,
                Frame?.D,
                GetSpriteWidthPxForCollision(),
                mass,
                NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity);

            BattleMechanicsStepResult stepResult =
                ResolveCharacterMechanics().StepBattleLogic(context);
            if (Frame?.D != null && context.spriteWidthPx > 0f)
            {
                Runtime.UpdateSpriteOrigin(
                    Frame.D.centerx,
                    Frame.D.centery,
                    context.spriteWidthPx);
            }
            RegisteredWorldForSimulation?.BoundaryWriter.SyncConsumedFlags(Runtime);
            if (ShouldResolveCharacterLanding(stepResult))
            {
                ApplySharedCharacterDatLandingIfNeeded(
                    stepResult.VerticalVelocityBeforeLanding);
            }

            Runtime.SyncIntegerPosition();
            PromoteSharedCharacterDatState12AirborneFrameIfNeeded(tickIndex);
            PromoteSharedCharacterDatBurningAirborneFrame205IfNeeded();
            ResetWeaponCountOutsideState12FrameAdvanceTail();

        }

        protected CharacterMechanics ResolveCharacterMechanics()
        {
            SimulationWorld world = RegisteredWorldForSimulation;
            if (world != null)
                return world.CharacterMechanicsForServices;

            compatibilityCharacterMechanics ??= new CharacterMechanics();
            return compatibilityCharacterMechanics;
        }

        internal bool ShouldResolveCharacterLanding(BattleMechanicsStepResult stepResult)
        {
            return stepResult.Landed;
        }

        protected bool RunSharedNonCharacterDatFrameAdvance()
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return false;
            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return false;
            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return false;

            ApplyNonCharacterFrameVelocityForFrameAdvance();

            int dataType = GetCurrentDataObjectTypeForSimulation();
            LF2FrameData frame = Frame?.D;
            if (Runtime == null || frame == null)
                return false;

            if (dataType == (int)LF2ObjectType.ThrowWeapon || ObjectId == 120)
                Runtime.X += Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;
            if (ObjectId == 101)
                Runtime.X -= Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;

            if (dataType == (int)LF2ObjectType.SpecialAttack && frame.hit_j > 0)
            {
                double visualZ = frame.hit_j - 50;
                Runtime.Z += visualZ;
                Runtime.Type3VisualZOffset += visualZ;
            }

            if ((dataType == (int)LF2ObjectType.ThrowWeapon || dataType == (int)LF2ObjectType.Drink) &&
                frame.state == 1000 &&
                System.Math.Abs(Runtime.Vx) > 9.0)
            {
                SetFrameTickDirect(40);
                frame = Frame?.D ?? frame;
            }

            double gravity = ResolveCurrentDatWeaponGravity(dataType, frame.state);
            bool landed = CharacterMechanics.WeaponDynamics(Runtime, gravity, out double landingVy);
            RegisteredWorldForSimulation?.BoundaryWriter.SyncConsumedFlags(Runtime);
            ApplyCurrentDatNonCharacterLanding(dataType, frame, landingVy, landed);
            ResetWeaponCountOutsideState12FrameAdvanceTail();

            Runtime.SyncIntegerPosition();
            RefreshRuntimeSnapshot();
            return true;
        }

        protected bool ApplyCurrentDatNonCharacterLanding(
            int dataType,
            LF2FrameData landingFrame,
            double landingVy,
            bool crossedGround)
        {
            if (Runtime == null || landingFrame == null)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            int dropHurt = characterData?.weapon_drop_hurt ?? 0;
            string dropSound = characterData?.weapon_drop_sound;
            int state = landingFrame.state;

            if (dataType == (int)LF2ObjectType.LightWeapon)
            {
                if (!crossedGround || landingVy <= 0.0001)
                    return true;

                Runtime.WeaponFlightCounter -= dropHurt;
                Runtime.Y = 0.0;
                if (landingVy <= 9.9)
                {
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(state == LF2States.WeaponThrowing ? 70 : 60);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }
                else if (state == LF2States.WeaponThrowing)
                {
                    Runtime.Vy = -8.0;
                    SetFrameTickRawDirect(7);
                    SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                    Runtime.Vx *= 0.5;
                    QueueBattleSound(dropSound);
                }
                else
                {
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(60);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }

                return true;
            }

            if (dataType == (int)LF2ObjectType.HeavyWeapon)
            {
                if (!crossedGround)
                    return true;

                Runtime.WeaponFlightCounter -= 1;
                Runtime.Y = 0.0;
                if (landingVy > 9.0)
                {
                    QueueBattleSound(dropSound);
                    Runtime.Vy = -5.0;
                    SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                    Runtime.Vx *= 0.5;
                }
                else
                {
                    Runtime.WeaponFlightCounter -= dropHurt;
                    if (Runtime.WeaponFlightCounter < 0)
                        Runtime.WeaponFlightCounter = 0;
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(20);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }

                return true;
            }

            if (dataType == (int)LF2ObjectType.ThrowWeapon ||
                dataType == (int)LF2ObjectType.Drink)
            {
                if (!crossedGround || landingVy <= 0.0001)
                    return true;

                Runtime.WeaponFlightCounter -= dropHurt;
                if (dataType == (int)LF2ObjectType.Drink && Health != null && Health.HP <= 0)
                    Runtime.WeaponFlightCounter = -1;

                Runtime.Y = 0.0;
                bool highSpeed = landingVy > 8.5 || Runtime.Vx < -10.0 || Runtime.Vx > 10.0;
                bool bounceState = state == LF2States.WeaponThrowing || state == LF2States.WeaponInSky;
                if (highSpeed && bounceState)
                {
                    Runtime.Vy = landingVy * -0.7;
                    if (Runtime.Vy < -10.0)
                        Runtime.Vy = -10.0;
                    Runtime.Vx *= 0.7;
                    SetFrameTickRawDirect(0);
                    QueueBattleSound(dropSound);
                }
                else
                {
                    Runtime.Vy = 0.0;
                    Runtime.Vx *= 0.7;
                    SetFrameTickRawDirect(state == LF2States.WeaponThrowing ? 70 : 60);
                    AttackingCounter = 0;
                }

                return true;
            }

            if (ObjectId == 999 && crossedGround)
            {
                Runtime.Y = 0.0;
                Runtime.Vy = 0.0;
                Runtime.Vx = 0.0;
                SetFrameTickRawDirect(101);
                AttackingCounter = 0;
                return true;
            }

            return false;
        }

        private double ResolveCurrentDatWeaponGravity(int dataType, int state)
        {
            if (dataType == (int)LF2ObjectType.SpecialAttack)
                return 0.0;
            if (dataType == (int)LF2ObjectType.Drink)
                return NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
            if (dataType == (int)LF2ObjectType.ThrowWeapon)
                return 0.85;
            if (state != LF2States.WeaponThrowing)
                return NTSDGlobal.Gameplay.WeaponGravityDefault;

            switch (ObjectId)
            {
                case 124:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub7C;
                case 120:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub78;
                case 101:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
                default:
                    return NTSDGlobal.Gameplay.WeaponGravityDefault1002;
            }
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

                if (landedVy <= 11.0 &&
                    Runtime.Vx <= 9.0 &&
                    Runtime.Vx >= -9.0 &&
                    frame.state != LF2States.Burning)
                {
                    Runtime.Y = 0.0;
                    Runtime.Vy = 0.0;
                    Runtime.Vx *= 0.3333333333333333;
                    AttackingCounter = 0;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack
                        ? LF2StandardFrames.LyingBack
                        : LF2StandardFrames.Lying);
                }
                else
                {
                    Runtime.Y = 0.0;
                    Runtime.Vy = -3.5;
                    if (Runtime.Vx > 7.0)
                        Runtime.Vx = 7.0;
                    if (Runtime.Vx < -7.0)
                        Runtime.Vx = -7.0;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack && frame.state != LF2States.Burning
                        ? LF2StandardFrames.FallingBack5
                        : LF2StandardFrames.FallingFront5);
                }

                return;
            }

            if (frame.state == LF2States.Frozen && landedVy > 0.0001)
            {
                Runtime.Y = 0.0;

                if (landedVy <= 17.0 && Runtime.Vx <= 9.0 && Runtime.Vx >= -9.0)
                {
                    Runtime.Vx *= 0.3333333333333333;
                    Runtime.Vy = 0.0;
                    return;
                }

                int injury = FallDamageDiv == 0 ? 10 : 1000 / FallDamageDiv;
                if (Health != null)
                    Health.HP -= injury;

                Runtime.Vy = -3.5;
                if (Runtime.Vx > 7.0)
                    Runtime.Vx = 7.0;
                if (Runtime.Vx < -7.0)
                    Runtime.Vx = -7.0;
                ImmediateFrame(LF2StandardFrames.FallingFront5);
                return;
            }

            Runtime.Y = 0.0;
            Runtime.Vy = 0.0;
            Runtime.Vx *= 0.3333333333333333;
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

        protected internal void ResetWeaponCountOutsideState12FrameAdvanceTail()
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
            WriteCurrentFrameId(frameId);
            Frame.D = targetFrame;
            if (targetFrame != null)
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

            int dataType = GetCurrentDataObjectTypeForSimulation();
            if (FrameDelay != 0 && dataType != (int)LF2ObjectType.SpecialAttack)
                return false;

            if (AttackExempt > 0)
                AttackExempt--;

            if ((Runtime?.LinkState ?? 0) < 0)
                return false;

            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return false;
            if (frame.cpoint != null && frame.cpoint.kind == 2)
                return false;

            if (dataType == (int)LF2ObjectType.SpecialAttack && frame.hit_a > 0 && Health != null)
            {
                Health.HP -= frame.hit_a;
                if (Health.HP <= 0)
                {
                    Health.HP = 0;
                    SetFrameTickImmediateRawDirect(frame.hit_d);
                    frame = Frame?.D;
                    if (frame == null)
                        return false;
                }
            }

            RunReleaseFrameTickCounters();

            int waitCounter = Trans?.WaitCounter ?? 0;
            if ((Frame?.N ?? 0) != waitCounter)
            {
                OnFrameTickFrameChangedFromWaitCounter();
                AttackingCounter = 0;
            }

            AttackingCounter++;

            int state = frame.state;
            bool suppressJumpInit = false;
            if (state == 0 && GetRuntimeYInt() < 0)
            {
                SetFrameTickImmediateRawDirect(212);
                suppressJumpInit = true;
                frame = Frame?.D;
                if (frame == null)
                    return false;
                state = frame.state;
            }

            if (dataType == (int)LF2ObjectType.HeavyWeapon &&
                state == LF2States.HeavyWeaponInSky &&
                GetRuntimeYInt() == 0 &&
                System.Math.Abs(Runtime.Vx) < 0.1)
            {
                return false;
            }

            if (state == LF2States.Lying && Health != null && Health.HP <= 0)
            {
                if ((KillCount >= 0 || RelationTeam == 5 || (Runtime?.SlotIndex ?? -1) >= 20) && HitStun <= 0)
                    HitStun = 30;
                AttackingCounter = 0;
            }

            if (state == LF2States.HeavyWeaponInSky)
                SwitchDir(Runtime.Vx > 0f ? "right" : "left");

            int wait = Trans?.Wait ?? frame.wait;
            if (AttackingCounter > wait)
            {
                int next = Trans?.Next ?? frame.next;
                AttackingCounter = 0;
                if (next != 0)
                {
                    bool allowJumpInit = true;
                    int targetFrame = next;
                    if (targetFrame == 999)
                    {
                        bool to212 = GetRuntimeYInt() != 0 && dataType == (int)LF2ObjectType.Character;
                        targetFrame = to212 ? 212 : 0;
                        suppressJumpInit = to212;
                        allowJumpInit = false;
                    }
                    else if (targetFrame < 0)
                    {
                        targetFrame = -targetFrame;
                        SwitchDir(Runtime?.Dir == "left" ? "right" : "left");
                    }

                    int previousFrame = waitCounter;
                    SetFrameTickImmediateRawDirect(targetFrame);
                    int frameAfterTransit = Frame?.N ?? targetFrame;
                    if (frameAfterTransit < 0 || frameAfterTransit >= LF2FrameCache.MaxFrameIdExclusive || Frame?.D == null)
                        return false;

                    ApplyCommonCaughtExitHitStop(previousFrame);
                    if (frameAfterTransit == 212 && allowJumpInit && !suppressJumpInit)
                        ApplyFrame212JumpInit();
                    ApplyCommonFrameTickPpDisplayPostAdvance();
                }
            }

            int currentFrame = Frame?.N ?? -1;
            if (currentFrame == 110 || currentFrame == 114)
            {
                if (registeredWorld != null)
                    registeredWorld.CharacterInputWriter.SetDefendLock(Runtime, 3);
                else
                    Runtime.CdDefendLock = 3;
            }
            if (currentFrame == 202)
                HitStun = 20;

            LF2FrameData currentData = Frame?.D;
            if (currentData != null)
                Trans?.SyncWaitCounterFrame(currentFrame);

            return true;
        }

        internal bool RunCommonFrameTickFromTransistor()
        {
            return RunCommonFrameTick();
        }

        private void SetFrameTickRawDirect(int frameId)
        {
            if (Frame == null)
                return;

            WriteCurrentFrameId(frameId);
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
        }

        internal void SetFrameTickImmediateRawDirect(int frameId)
        {
            SetFrameTickRawDirect(frameId);
            if (Runtime != null)
                Runtime.FrameWaitCounter = 0;
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



        /// <summary>在 SimulationWorld 成功接纳实体生命周期后分配稳定 ID。</summary>
        internal void AssignStableIdForRegistration(int stableId)
        {
            if (StableId > 0)
                return;

            StableId = stableId;
        }

        internal void RestoreStableIdAfterLifecycleReset(int stableId)
        {
            StableId = stableId;
        }

        /// <summary>重置稳定 ID。</summary>
        protected void ResetStableId()
        {
            StableId = 0;
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

        /// <summary>
        /// FrameAdvance 的正式角色 writer 已经把 frame、transition、health、motion
        /// 与计数状态直接写入 Runtime。exact 生产角色无需在同一 pass 尾部重新复制
        /// 整个对象快照；未知派生类型仍保留虚拟刷新作为兼容边界。
        /// </summary>
        internal bool RefreshRuntimeSnapshotAfterFrameAdvance()
        {
            if (GetType() == typeof(LF2Character))
                return false;

            RefreshRuntimeSnapshot();
            return true;
        }

        /// <summary>
        /// 权威碰撞冻结 pass 只把当前 Frame 冻结到 PrevFrame2。exact 生产角色的
        /// CaptureCollisionFrameSnapshot 已原子更新 Frame.Prev2、Prev2D 与
        /// Runtime.PrevFrame2，无需随后重建整份 Runtime；未知派生类型保留虚拟回退。
        /// </summary>
        internal bool RefreshRuntimeSnapshotAfterCollisionSnapshot()
        {
            if (GetType() == typeof(LF2Character))
                return false;

            RefreshRuntimeSnapshot();
            return true;
        }

        /// <summary>
        /// Frame post-process 与 entity post-frame tail 只修改已经直接绑定 Runtime 的
        /// motion、hit accumulator、health、timer 与 transient 字段。exact 生产角色无需在
        /// pass 尾部再次复制整份对象快照；未知派生类型保留虚拟刷新，避免绕过扩展副作用。
        /// </summary>
        internal bool RefreshRuntimeSnapshotAfterPostFrameMaintenance()
        {
            if (GetType() == typeof(LF2Character))
                return false;

            RefreshRuntimeSnapshot();
            return true;
        }

        /// <summary>
        /// CharacterInput 的 frame、transition 与 health 写入口已经直接更新 Runtime。
        /// 保留该空入口作为 U6 迁移期的调用边界，避免默认路径再逐实体复制一遍同一真值；
        /// 强制 Legacy A/B 仍由 SimulationWorld 直接调用完整 RefreshRuntimeSnapshot。
        /// </summary>
        internal void RefreshRuntimeSnapshotAfterCharacterInput()
        {
        }

        /// <summary>
        /// LateEntityUpdate 的生产角色写入口已经把身份、frame、transition、health 与
        /// 计数状态直接写入 Runtime。仅当 exact LF2Character 的最小非别名字段也仍然
        /// 与 Runtime 一致时，才允许省略 pass 尾部的整份对象快照；未知派生类型或
        /// 发现陈旧字段时继续走完整刷新，保持扩展与异常路径 fail-closed。
        /// </summary>
        internal bool RequiresRuntimeSnapshotAfterLateEntityUpdate()
        {
            return GetType() != typeof(LF2Character) ||
                   !IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp();
        }

        internal bool IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp()
        {
            if (Runtime == null)
                return false;

            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (Runtime.ObjType != ResolveReferenceRuntimeObjTypeFromDataType(currentDataType) ||
                Runtime.EntityType != currentDataType ||
                Runtime.Frame != (Frame?.N ?? 0) ||
                Runtime.WaitCounter != (Trans?.WaitCounter ?? 0) ||
                Runtime.NextFrame != (Trans?.Next ?? 0))
            {
                return false;
            }

            return true;
        }

        protected virtual void RefreshRuntimeFromEntity()
        {
            RefreshBaseRuntimeFromEntity();
        }

        private void RefreshBaseRuntimeFromEntity()
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
