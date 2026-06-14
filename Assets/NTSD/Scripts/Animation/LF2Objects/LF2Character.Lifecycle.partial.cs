using NTSD.Animation.LF2Tasks;
using NTSD.Game;
using NTSD.LevelEditor;
using NTSD.Simulation;
using UnityEngine;
using NTSD.Input;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        public void InjectDependencies(
            Transform entityTransform,
            Transform visualTransform,
            string name)
        {
            EntityTransform = entityTransform;
            Name = name;
        }

        /// <summary>
        /// 池化路径专用初始化（无参版本）。
        /// InjectDependencies 之后、ModuleBind 之前调用。
        /// 初始化物理和状态机运行所需的基础字段。
        /// </summary>
        public void ModuleInitialize()
        {
            _mech = new CharacterMechanics();
            _cachedIsPointWalkable = point =>
            {
                SimulationWorld world = Match;
                if (world != null)
                    return world.IsGroundPointWalkable(point);

                BoundaryWallManager manager = BoundaryWallManager.Instance;
                return manager == null || manager.IsPointWalkable(point);
            };

            PS.x = 0;
            PS.y = 0;
            PS.z = 0;
            PS.vx = 0;
            PS.vy = 0;
            PS.vz = 0;
        }

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            Renderer = renderer;

            if (task is not OPointCreateTask opTask)
                return;

            InitializeFromOpoint(opTask);
        }

        public override void Reset()
        {
            InputState?.Reset();
            _hitCounters?.Reset();
            ItrRest?.Reset();
            PS?.Reset();
            WeaponPointModule?.Reset();
            _heldWeapon = null;

            // C++ release 对齐 0x00421185/0x00421191：spawn/reset 时清 Entity::attacking。
            AttackingCounter = 0;
            FrameDelay = 10;
            ShotCount = 0;
            ResetSpark();
            _initializedFromOpoint = false;
            AiControlled = false;
            if (Controller is NullLF2Controller)
                Controller = new CharacterInputModule();
            ResetStateRuntime();
        }

        public override void Destroy()
        {
            Reset();
        }

        public override void UnregisterFromWorld()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
        }

        /// <summary>
        /// 角色不回收到对象池，只执行 destroy 逻辑
        /// </summary>
        public override void OnTransitDestroy()
        {
            DestroyEvent();
            Destroy();
        }

        /// <summary>
        /// 模块绑定（对应 LF2CharacterAnimator.ModuleBind）
        /// </summary>
        public void ModuleBind(LF2CharacterDataWrapper frameDataWrapper, int characterId)
        {
            FrameCache.Load(frameDataWrapper);

            if (!_initializedFromOpoint)
            {
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
            }
            else
            {
                Frame.D = FrameCache.GetFrameDataById(Frame.N);
                if (Frame.D == null)
                {
                    Frame.N = 0;
                    Frame.PN = 0;
                    Frame.D = FrameCache.GetFrameDataById(0);
                }
            }

            if (Frame.D != null)
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);

            InputState?.Reset();
            ItrRest?.Reset();
            _hitCounters?.Reset();

            _mass = NTSDSpec.GetMassOrDefault(characterId);

            SimulationTickDriver.Instance?.World?.Register(this);

            // C++ release 常规角色在进入战斗后，holder_copy 应该落到自身运行时槽位，
            // 作为后续 opoint / 命中归属 / 统计链的基线。
            // 只有 opoint 子角色保留从父对象继承来的 holder_copy。
            if (!_initializedFromOpoint)
                HolderCopySlot = Runtime?.SlotIndex ?? -1;

            if (WeaponPointModule != null && WeaponPointModule.Factory == null && LF2WeaponPointFactory.Instance != null)
                WeaponPointModule.SetFactory(LF2WeaponPointFactory.Instance);
        }

        /// <summary>
        /// 初始化角色属性
        /// </summary>
        public void Initialize(int maxHp, int maxMp)
        {
            Health.HP = maxHp;
            Health.HPBound = maxHp;
            Health.HP3 = maxHp;
            Health.MP = maxMp;
            Health.PP = maxMp;
            Health.MaxPP = maxMp;
            Health.PPBound = maxMp;

            // C++ release entity+340h = MaxMP，用于 kind=0/16 伤害 MP% 缩放
            Health.MaxMP = maxMp;
            InputState.Reset();
            HitCounters.Reset();
            ItrRest.Reset();
        }

        protected override void ResetStateRuntime()
        {
            CaughtDuration = 0;
            CaughtFront = true;
            JumpAttackLock = 0;
            WeaponCount = 0;
            FallDamageDiv = 0;
            GrabbedBy = 0;
            CaughtSlotIndex = -1;
            CatcherSlotIndex = -1;
            TrackerFlag = 0;
            TrackerParent = null;
            HolderCopySlot = -1;
            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
            Runtime.LinkState = 0;
            Runtime.TargetSlotIndex = -1;
            Runtime.HolderStableId = -1;
            Runtime.HeldWeaponStableId = -1;
        }
    }
}
