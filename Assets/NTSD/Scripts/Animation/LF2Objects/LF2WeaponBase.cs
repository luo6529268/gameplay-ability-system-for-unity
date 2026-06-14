using System.Collections.Generic;
using NTSD.Animation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 对齐 C++ release 的武器基类。
    /// </summary>
    public abstract partial class LF2WeaponBase : LF2Entity
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

        // C++ release [entity+3F8h]：picker_idx，保存的是运行时槽位。
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
        public override NTSDEntityCategory EntityCategory => NTSDEntityCategory.Weapon;
        public override bool CountsAsRandomWeaponDropCandidate()
        {
            int weaponType = WeaponType;
            return weaponType == 1 || weaponType == 2 || weaponType == 4 || weaponType == 6;
        }
        internal override bool UsesDynamicRuntimeSlot() => true;
        // C++ release weapon_count：笛子命中累积器，子类实现存储。
        public virtual int FluteWeight { get => 0; set { } }

        /// <summary>C++ release 0x004228A0: type=1/2/4/6 才检查 flightCounter</summary>
        protected virtual bool IsWeaponDestroyable() => false;

        /// <summary>供基类 SimTU 读取 WeaponFlightCounter。</summary>
        protected virtual int GetFlightCounter() => 0;

        protected virtual void OnHealthInitialized(LF2CharacterData charData) { }

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

        protected virtual bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            return GetState() switch
            {
                LF2States.WeaponJustOnGround => State_WeaponJustOnGround(eventType, eventData),
                LF2States.WeaponOnGround => State_WeaponOnGround(eventType, eventData),
                _ => false,
            };
        }

        protected virtual bool State_WeaponJustOnGround(string eventType, object eventData)
        {
            return false;
        }

        protected virtual bool State_WeaponOnGround(string eventType, object eventData)
        {
            return false;
        }

        protected int ResolveRuntimeWeaponState()
        {
            int runtimeState = Runtime?.WeaponState ?? 0;
            return runtimeState != 0 ? runtimeState : GetState();
        }

        internal int GetResolvedWeaponStateForExternalUse()
        {
            return ResolveRuntimeWeaponState();
        }

        /// <summary>
        /// C++ release 语义下的武器受击入口。
        /// </summary>
        public abstract bool Hit(InteractionArea itr, LF2Entity attacker);

        protected virtual bool CanInteractTarget(InteractionArea itr, LF2Entity target, int hitBodyX = 0)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr)) return false;
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(this, target)) return false;
            if (!BruteForceSceneQuery.RuntimeConsumeItrAllowed(this, itr, target)) return false;
            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true)) return false;

            return true;
        }

        private static bool IsWeaponTarget(LF2Entity target)
        {
            return target is LF2WeaponBase;
        }

        private static LF2Character AsCharacterTarget(LF2Entity target)
        {
            return target as LF2Character;
        }

        private static LF2SpecialAttack AsSpecialAttackTarget(LF2Entity target)
        {
            return target as LF2SpecialAttack;
        }

        private bool TryGetPickupCharacterTarget(LF2Entity target, out LF2Character character)
        {
            character = AsCharacterTarget(target);
            if (HoldObj != null)
                return false;
            if (!ItrArestTest())
                return false;
            if (Renderer == null || character == null)
                return false;
            if (character.GetHeldWeapon() != null)
                return false;

            int weaponState = ResolveRuntimeWeaponState();
            return weaponState == LF2States.WeaponOnGround ||
                   weaponState == LF2States.HeavyWeaponOnGround;
        }

        private bool CompletePickupInteraction(LF2Character character, InteractionArea itr, bool applyPickupFrameJump)
        {
            if (character == null || !Pick(character))
                return false;

            character.HoldWeapon(this);
            ApplyPickupGrabbedBy(character);
            if (applyPickupFrameJump)
                ApplyPickupFrameJump(character);

            ItrArestUpdate(itr);
            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0)
                character.ItrVrestUpdate(selfSlot, itr, true);
            return true;
        }

        protected virtual bool HandleWeaponKind3Stick(InteractionArea itr, LF2Entity target)
        {
            if (IsWeaponTarget(target)) return false;
            if (!ItrArestTest()) return false;

            int catchingFrame = itr.catchingact != null && itr.catchingact.Length > 0 ? itr.catchingact[0] : 0;
            int caughtFrame = itr.caughtact != null && itr.caughtact.Length > 0 ? itr.caughtact[0] : 0;
            if (catchingFrame <= 0 && caughtFrame <= 0)
                return HandlePreInteractionKind3(itr, target);

            if (catchingFrame > 0)
            {
                SetFrameDirect(catchingFrame);
            }
            LF2Character character = AsCharacterTarget(target);
            if (caughtFrame > 0 && character != null)
            {
                character.ImmediateFrame(caughtFrame);
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

            LF2SpecialAttack specialAttack = AsSpecialAttackTarget(target);
            if (specialAttack != null)
            {
                return specialAttack.Hit(itr, this);
            }

            LF2Character character = AsCharacterTarget(target);
            if (character != null)
            {
                if (PS != null)
                {
                    var attackerPos = new Vector3(PS.x, PS.y, PS.z);
                    return character.Hit(itr, this, attackerPos, default);
                }
            }

            return false;
        }

        protected virtual bool HandlePreInteractionKind1(InteractionArea itr, LF2Entity target)
        {
            return TryGetPickupCharacterTarget(target, out LF2Character character) &&
                   CompletePickupInteraction(character, itr, applyPickupFrameJump: false);
        }

        protected virtual bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            return TryGetPickupCharacterTarget(target, out LF2Character character) &&
                   CompletePickupInteraction(character, itr, applyPickupFrameJump: true);
        }

        protected virtual bool HandlePreInteractionKind3(InteractionArea itr, LF2Entity target)
        {
            if (IsWeaponTarget(target)) return false;
            return TryApplyHit(itr, target);
        }

        protected virtual bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            return HandlePreInteractionKind1(itr, target);
        }

        /// <summary>
        /// C++ release 语义下的武器拾取入口。
        /// </summary>
        public virtual bool Pick(LF2LivingObject holder)
        {
            if (_holdObj != null) return false;

            _holdObj = holder;
            var holderEntity = holder as LF2Entity;
            Runtime.HolderStableId = holderEntity?.Runtime?.SlotIndex ?? -1;
            HolderCopySlot = holderEntity?.Runtime?.SlotIndex ?? -1;
            Team = holder.Team;
            RelationTeam = holderEntity?.RelationTeam ?? holder.Team;

            return true;
        }

        /// <summary>
        /// 饮料消耗完毕后的子类钩子，用于重置 WeaponFlightCounter 等字段。
        /// C++ release 0x41AD73: weapon.[+31Ch] = 0
        /// </summary>
        protected virtual void OnDrinkConsumed() { }

        protected virtual WeaponAttackResult ProcessAttack(LF2LivingObject holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            return new WeaponAttackResult();
        }

        private readonly List<LF2LivingObject> _boomerangQueryCache = new List<LF2LivingObject>(8);
    }

    public class WeaponActResult
    {
        public bool Thrown;
        public bool ForceDrop;
        public bool NeedsKind3Drop;
        public WeaponAttackResult AttackResult;
    }

    public class WeaponAttackResult
    {
        public int VRest;
        public int ARest;
        public int HitUid;
    }
}
