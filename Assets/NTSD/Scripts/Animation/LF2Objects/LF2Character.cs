using NTSD.Animation.LF2Tasks;
using NTSD.Game;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 角色专用战斗逻辑，基于 LF2LivingObject 分层实现。
    /// 战斗行为以 C++ release 的实体、帧和输入模型为准；
    /// Unity 专用代码只负责组件装配、对象池和数据适配。
    /// 
    /// 继承关系：LF2LivingObject -> LF2Character。
    /// </summary>
    public partial class LF2Character : LF2LivingObject
    {
        // ========== ILF2Object 实现 ==========

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;
        public override NTSDEntityCategory EntityCategory => NTSDEntityCategory.Character;
        internal override bool UsesDynamicRuntimeSlot() => _initializedFromOpoint;
        internal override bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame) => frame != null;

        // ========== 角色专用模块 ==========

        public NTSDInputStateModule InputState { get; private set; }

        /// <summary>
        /// 处理正式流程中的 wpoint 持有、投掷和攻击请求。
        /// </summary>
        public LF2WeaponPointModule WeaponPointModule { get; private set; }

        /// <summary>
        /// 处理 fall / bdefend 等受击累计计数。
        /// </summary>
        private readonly LF2HitCountersModule _hitCounters;
        public override LF2HitCountersModule HitCounters => _hitCounters;

        // ========== 武器持有 ==========

        /// <summary>当前持有的武器对象引用。正式持有关系字段同步到 Runtime。</summary>
        private ILF2Object _heldWeapon;

        // ========== Unity 组件引用 ==========
        public Transform EntityTransform { get; private set; }
        // ========== 物理计算 ==========

        private CharacterMechanics _mech;
        private float _mass = NTSDGlobal.Default.Machanics.Mass;
        private Func<Vector2, bool> _cachedIsPointWalkable;
        // ========== 抓取系统字段 ==========

        // 抓取持续计数。C++ release 抓取成功时写抓取者 caught_duration=300，
        // 后续由抓取者当前帧 cpoint.decrease 驱动递减或逃脱。
        protected int CaughtDuration { get => Runtime.CaughtDuration; set => Runtime.CaughtDuration = value; }
        // 被抓方向：true=正面，false=背面。
        protected bool CaughtFront { get => Runtime.CaughtFrontFlag != 0; set => Runtime.CaughtFrontFlag = value ? 1 : 0; }
        private int JumpAttackLock { get => Runtime.JumpAttackLock; set => Runtime.JumpAttackLock = value; }

        // ========== 死亡闪烁计数 ==========
        // -1 = 不执行；0 = 开始；1~29 = 持续；>=30 = 结束销毁
        private int _deadBlinkCount = -1;

        private bool _initializedFromOpoint;

        public bool InitializedFromOpoint => _initializedFromOpoint;

        // ========== 构造函数 ==========

        public LF2Character() : base()
        {
            AllocateStableId();

            // 创建角色专用模块
            InputState = new NTSDInputStateModule();
            WeaponPointModule = new LF2WeaponPointModule();
            _hitCounters = new LF2HitCountersModule();

            // 基类字段初始化
            ItrRest = new LF2ItrRestTracker();
            PS = new PhysicsState();
            PS.BindRuntime(Runtime);
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            Health = new LF2Health();
            Health.BindRuntime(Runtime);
            _hitCounters.BindRuntime(Runtime);
            Sprite = new LF2Sprite();
            Trans = new FrameTransistor(this);
            Controller = new CharacterInputModule();

            // 角色状态分发固定写在 switch 中，不再保留运行时 handler 表。
        }

        /// <summary>
        /// 应用物理动力学
        /// </summary>
        public void ApplyDynamics()
        {
            var ctx = new CharacterMechanicsContext(
                PS,
                Frame.D,
                GetSpriteWidthPxForCollision(),
                _mass,
                NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                _cachedIsPointWalkable
            );

            var stepResult = _mech.Step(ctx);
            if (stepResult.landed)
            {
                HandleLandingEvent(stepResult.verticalVelocityBeforeLanding);

                float spriteWidthPx = GetSpriteWidthPxForCollision();
                if (Frame?.D != null && spriteWidthPx > 0f)
                    PS.UpdateSpriteOrigin(Frame.D.centerx, Frame.D.centery, spriteWidthPx);
            }
        }

        // ========== 方向控制 ==========

        // ========== 武器持有 ==========

        // ========== 帧播放接口 ==========

    }
}
