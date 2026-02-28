using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 武器抽象基类
    /// 严格对齐 FLF weapon.js typeweapon
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\weapon.js
    /// </summary>
    public abstract class LF2WeaponBase : LF2LivingObject
    {
        // ========== 配置字段 ==========
        protected int _objectId;
        protected int _lastState = -1;

        // ========== 持有者信息 ==========
        protected LF2LivingObject _holdObj;
        protected LF2LivingObject _holdPre;

        // ========== VRest 系统 ==========
        protected Dictionary<int, int> _vrest = new Dictionary<int, int>();

        // ========== 武器数据 ==========
        public int WeaponDropHurt { get; set; } = 10;
        public string WeaponDropSound { get; set; } = "";
        public string WeaponBrokenSound { get; set; } = "";
        public string WeaponHitSound { get; set; } = "";

        // ========== 公开属性 ==========
        public LF2LivingObject HoldObj => _holdObj;
        public LF2LivingObject HoldPre => _holdPre;

        public abstract bool IsLight { get; }
        public abstract bool IsHeavy { get; }
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
                Pick(task.parent);
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

        #region Generic State Handlers

        private void Generic_TU()
        {
            Interaction();

            int state = GetState();
            switch (state)
            {
                case 1001:
                case 2001:
                    break;
                default:
                    CharacterMechanics.Dynamics(PS);
                    break;
            }

            if (PS.y == 0 && PS.vy > 0)
            {
                if (GetSpeed() > NTSDGlobal.Gameplay.WeaponBounceupLimit)
                {
                    if (IsLight)
                    {
                        PS.vy = 0;
                        Trans.Frame(70, 0);
                    }
                    if (IsHeavy)
                    {
                        PS.vy = NTSDGlobal.Gameplay.WeaponBounceupSpeedY;
                    }
                    if (PS.vx != 0) PS.vx = Mathf.Sign(PS.vx) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                    if (PS.vz != 0) PS.vz = Mathf.Sign(PS.vz) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;

                    Health.HP -= WeaponDropHurt;
                }
                else
                {
                    Team = 0;
                    PS.vy = 0;
                    if (IsLight)
                    {
                        Trans.Frame(70, 0);
                    }
                    if (IsHeavy)
                    {
                        Trans.Frame(21, 0);
                    }
                }
                PS.zz = 0;
            }
        }

        private void Generic_Die()
        {
            Trans.Frame(1000, 0);
            PlaySound(WeaponBrokenSound);
            CreateBrokenEffect();
        }

        #endregion

        #region Specific State Handlers

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

        public override void Reset()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);

            _objectId = 0;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
            _holdObj = null;
            _holdPre = null;
            _vrest.Clear();

            ResetStableId();
        }

        public override void Destroy()
        {
            Generic_Die();
        }

        // ========== 帧转换回调 ==========

        // ========== ISimObject 生命周期 ==========

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

            if (Health.HP <= 0)
            {
                StateUpdate("die", null);
            }
        }

        // ========== 交互方法 ==========

        /// <summary>
        /// 对应 FLF weapon.prototype.interaction (weapon.js:216-273)
        /// </summary>
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
                    return;
                }
            }
        }

        protected virtual bool CanInteractTarget(InteractionArea itr, LF2LivingObject target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (Team != 0 && target.Team != 0 && Team == target.Team) return false;
            if (!target.ItrVrestTest(StableId)) return false;
            var kindService = Match?.ItrKindService;
            if (!kindService.ShouldHitTarget(itr.kind, this, target)) return false;

            return true;
        }

        protected virtual bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2LivingObject target)
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

        protected virtual bool TryApplyHit(InteractionArea itr, LF2LivingObject target)
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

        protected virtual bool HandlePreInteractionKind1(InteractionArea itr, LF2LivingObject target)
        {
            if (HoldObj != null) return false;
            if (!ItrArestTest()) return false;
            if (Renderer == null) return false;
            if (target is not LF2Character character) return false;
            if (character.GetHeldWeapon() != null) return false;

            Pick(target);
            character.HoldWeapon(Renderer);
            ItrArestUpdate(itr);
            target.ItrVrestUpdate(StableId, itr);
            return true;
        }

        protected virtual bool HandlePreInteractionKind2(InteractionArea itr, LF2LivingObject target)
        {
            return HandlePreInteractionKind1(itr, target);
        }

        protected virtual bool HandlePreInteractionKind3(InteractionArea itr, LF2LivingObject target)
        {
            // TODO: pre_interaction kind 3 placeholder
            return false;
        }

        protected virtual bool HandlePreInteractionKind7(InteractionArea itr, LF2LivingObject target)
        {
            // TODO: pre_interaction kind 7 placeholder
            return false;
        }

        /// <summary>
        /// 对应 FLF weapon.prototype.hit (weapon.js:275-365)
        /// </summary>
        public abstract bool Hit(InteractionArea itr, LF2LivingObject attacker);

        /// <summary>
        /// 对应 FLF weapon.prototype.act (weapon.js:367-465)
        /// </summary>
        public virtual WeaponActResult Act(LF2LivingObject holder, WeaponPoint wpoint, Vector3 holdpoint)
        {
            var result = new WeaponActResult();
            if (Frame.D == null) return result;

            // 切换到武器动作帧
            if (wpoint.weaponact > 0)
            {
                Trans.Frame(wpoint.weaponact, 0);
                Trans.Trans();
            }

            var fD = Frame.D;
            if (fD?.wpoints == null || fD.wpoints.Count == 0) return result;

            var fwpoint = fD.wpoints[0];
            if (fwpoint.kind == 2) // 可投掷
            {
                if (wpoint.dvx != 0) PS.vx = Dirh() * wpoint.dvx;
                if (wpoint.dvz != 0) PS.vz = holder.Controller.Dirv() * wpoint.dvz;
                if (wpoint.dvy != 0) PS.vy = wpoint.dvy;

                if (PS.vx != 0 || PS.vy != 0 || PS.vz != 0)
                {
                    // 投掷
                    float imx = IsLight ? 58 : 48;
                    float imy = IsLight ? -15 : -40;

                    SetPos(
                        holder.PS.x + Dirh() * imx,
                        holder.PS.y + imy,
                        holder.PS.z + PS.vz
                    );
                    PS.zz = 1;

                    Trans.Frame(IsLight ? 40 : 999, 0);
                    Trans.Trans();

                    _holdObj = null;
                    result.Thrown = true;
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
                }

                // 轻武器攻击
                if (IsLight && wpoint.attacking > 0)
                {
                    result.AttackResult = ProcessAttack(holder, wpoint, fD);
                }
            }

            return result;
        }

        /// <summary>
        /// 对应 FLF weapon.prototype.drop (weapon.js:468-481)
        /// </summary>
        public virtual void Drop(float dvx, float dvy)
        {
            Team = 0;
            _holdObj = null;

            if (dvx != 0) PS.vx = dvx * 0.5f;
            if (dvy != 0) PS.vy = dvy * 0.2f;
            PS.zz = 0;

            Trans.Frame(999, 0);
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

        // ========== VRest 系统 ==========
        private List<int> _vrestKeysCache = new List<int>();

        public bool IsVRest(LF2LivingObject obj)
        {
            if (obj == null) return false;
            return _vrest.ContainsKey(obj.StableId) && _vrest[obj.StableId] > 0;
        }

        public void SetVRest(LF2LivingObject obj, int value)
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

        public float GetSpeed()
        {
            return Mathf.Sqrt(PS.vx * PS.vx + PS.vy * PS.vy);
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            // TODO: 实现音效播放
        }

        public void CreateBrokenEffect()
        {
            // TODO: 实现破碎效果
        }

        public void CreateEffect(int type)
        {
            // TODO: 实现特效
        }

        public void WhirlwindForce(InteractionArea itr)
        {
            // TODO: 实现龙卷风效果
        }

        public override void FluteForce()
        {
            // TODO: 实现笛子效果
        }

        protected virtual WeaponAttackResult ProcessAttack(LF2LivingObject holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            // TODO: 实现攻击处理
            return new WeaponAttackResult();
        }

        protected void CoincideXYWithWPoint(Vector3 holdpoint, WeaponPoint wpoint)
        {
            float wpx = (PS.dir == "right") ? wpoint.x : -wpoint.x;
            float wpy = wpoint.y;

            PS.x = holdpoint.x - wpx;
            PS.y = holdpoint.y - wpy;
        }

        // ========== 初始化子步骤 ==========

        protected void InitializeParent(OPointCreateTask task)
        {
            _objectId = task.opoint.oid;
            Team = task.team;
        }

        protected void InitializePosition(OPointCreateTask task)
        {
            SetPos(0, 0, task.z);

            if (Frame.D == null) return;

            Vector3 centerPoint = MakePointCenter(Frame.D);
            CoincideXYForInit(task.pos, centerPoint);
        }

        protected void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(dir);
        }

        protected void InitializeFrame(OPointCreateTask task)
        {
            int action = (task.opoint.action == 0) ? 999 : task.opoint.action;
            Trans.Frame(action, 0);
        }

        protected void InitializeHealth()
        {
            Health.HP = 100; // 默认值，应从数据读取
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
    }

    // ========== 结果类 ==========

    public class WeaponActResult
    {
        public bool Thrown;
        public WeaponAttackResult AttackResult;
    }

    public class WeaponAttackResult
    {
        public int VRest;
        public int ARest;
        public int HitUid;
    }
}
