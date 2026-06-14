using UnityEngine;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        private LF2Entity GetHeldEntity()
        {
            return _heldWeapon as LF2Entity;
        }

        private LF2WeaponBase GetHeldWeaponBase()
        {
            return _heldWeapon as LF2WeaponBase;
        }

        private bool HasHeldObject()
        {
            return _heldWeapon != null;
        }

        private bool IsHeldHeavyWeapon()
        {
            return GetHeldWeaponBase()?.IsHeavy == true;
        }

        private int GetHeldObjectId()
        {
            return _heldWeapon?.ObjectId ?? -1;
        }

        private bool IsHeldObjectAttackable()
        {
            int objectId = GetHeldObjectId();
            return objectId > 0 && NTSDSpec.IsWeaponAttackable(objectId);
        }

        private bool CanHeldObjectStandThrow()
        {
            int objectId = GetHeldObjectId();
            return objectId > 0 &&
                   (NTSDSpec.CanJustThrowWeapon(objectId) || NTSDSpec.CanStandThrowWeapon(objectId));
        }

        private bool CanHeldObjectRunThrow()
        {
            int objectId = GetHeldObjectId();
            return objectId > 0 && NTSDSpec.CanRunThrowWeapon(objectId);
        }

        private static LF2WeaponBase AsWeaponEntity(LF2Entity entity)
        {
            return entity as LF2WeaponBase;
        }

        private static bool IsWeaponEntity(LF2Entity entity)
        {
            return entity is LF2WeaponBase;
        }

        private static bool IsHeavyWeaponAttacker(LF2Entity attacker)
        {
            return attacker is LF2WeaponBase weapon && weapon.WeaponType == 2;
        }

        /// <summary>
        /// 持有武器，并同步正式运行时的持有关系字段。
        /// </summary>
        public void HoldWeapon(ILF2Object weapon)
        {
            _heldWeapon = weapon;
            LF2Entity held = GetHeldEntity();
            Runtime.HeldWeaponStableId = held?.Runtime?.SlotIndex ?? -1;
            Runtime.TargetSlotIndex = held?.Runtime?.SlotIndex ?? -1;
            Runtime.LinkState = ResolveHeldWeaponLinkState(weapon);

            if (held != null)
            {
                held.Runtime.HolderStableId = Runtime?.SlotIndex ?? -1;
                held.HolderCopySlot = Runtime?.SlotIndex ?? -1;
                held.Runtime.LinkState = ResolveHeldObjectLinkState(weapon);
                if (held.GrabbedBy == 0)
                    held.GrabbedBy = -1;
                held.Team = Team;
                held.RelationTeam = RelationTeam;
            }
        }

        /// <summary>
        /// opoint kind=2 生成后的持有绑定，对齐 C++ release spawn_from_opoint。
        /// </summary>
        public void AttachOpointHeldObject(LF2Entity held)
        {
            _heldWeapon = held;
            Runtime.LinkState = 1;
            Runtime.TargetSlotIndex = held?.Runtime?.SlotIndex ?? -1;
            Runtime.HeldWeaponStableId = held?.Runtime?.SlotIndex ?? -1;

            if (held == null)
                return;

            held.GrabbedBy = -1;
            held.Runtime.LinkState = -1;
            held.Runtime.HolderStableId = Runtime?.SlotIndex ?? -1;
            held.Runtime.TargetSlotIndex = -1;
            held.Runtime.HeldWeaponStableId = -1;
            held.HolderCopySlot = HolderCopySlot;
            held.Team = Team;
            held.RelationTeam = RelationTeam;
        }

        /// <summary>
        /// 获取当前持有的武器
        /// </summary>
        public ILF2Object GetHeldWeapon()
        {
            return _heldWeapon;
        }

        /// <summary>
        /// 按 C++ release AI_Process2 的 held-object pass 同步/释放当前持有对象。
        /// 武器仍复用 LF2WeaponBase.Act 的攻击细节；非武器 opoint kind=2 也会走同一释放规则。
        /// </summary>
        public bool ReleaseHeldObjectByWPoint(WeaponPoint holderWPoint, out WeaponActResult result)
        {
            return ReleaseHeldObjectByWPoint(GetHeldEntity(), holderWPoint, out result);
        }

        /// <summary>
        /// C++ release AI_Process2 遍历 link_state&lt;0 对象后，按 holder 当前 wpoint 同步/释放。
        /// </summary>
        public bool ReleaseHeldObjectByWPoint(LF2Entity held, WeaponPoint holderWPoint, out WeaponActResult result)
        {
            result = new WeaponActResult();
            if (holderWPoint == null || held == null || held.PS == null)
                return false;

            if (!ReferenceEquals(_heldWeapon, held))
                _heldWeapon = held;

            Vector3 holdpoint = CalcHeldObjectPoint(holderWPoint);

            if (holderWPoint.kind == 3)
            {
                SyncHeldObjectFrameAndPosition(held, holderWPoint, holdpoint);
                DropHeldObjectRandomly(held);
                return true;
            }

            LF2WeaponBase weapon = AsWeaponEntity(held);
            if (weapon != null)
            {
                result = weapon.Act(this, holderWPoint, holdpoint);
                return true;
            }

            SyncHeldObjectFrameAndPosition(held, holderWPoint, holdpoint);

            LF2FrameData heldFrame = held.Frame?.D;
            if (heldFrame != null && (heldFrame.state == LF2States.Falling || heldFrame.state == LF2States.BeingCaught))
            {
                DropHeldObjectFromDamagedHolder(held);
                return true;
            }

            if (holderWPoint.dvx != 0)
            {
                int objType = held.ObjectType;
                if (objType == 1 || objType == 4 || objType == 6)
                {
                    held.ImmediateFrame(40);
                    ApplyHeldObjectThrowVelocity(held, holderWPoint);
                    ClearReleasedHeldObject(held, clearTeam: false);
                    result.Thrown = true;
                    return true;
                }

                if (objType == 2)
                {
                    held.ImmediateFrame(RandInt(0, 6));
                    ApplyHeldObjectThrowVelocity(held, holderWPoint);
                    ClearReleasedHeldObject(held, clearTeam: false);
                    result.Thrown = true;
                    return true;
                }
            }

            return true;
        }

        private Vector3 CalcHeldObjectPoint(WeaponPoint wpoint)
        {
            var frame = Frame?.D;
            if (PS == null || frame == null)
                return Vector3.zero;

            int holderX = Runtime != null ? Runtime.XInt : (int)PS.x;
            int holderY = Runtime != null ? Runtime.YInt : (int)PS.y;
            int holderZ = Runtime != null ? Runtime.ZInt : (int)PS.z;
            float x = PS.dir == "right"
                ? holderX - frame.centerx + wpoint.x
                : holderX + frame.centerx - wpoint.x;
            float y = holderY - frame.centery + wpoint.y;
            return new Vector3(x, y, holderZ);
        }

        private void SyncHeldObjectFrameAndPosition(LF2Entity held, WeaponPoint holderWPoint, Vector3 holdpoint)
        {
            if (held == null || held.PS == null)
                return;

            held.ImmediateFrame(holderWPoint.weaponact);
            held.FrameDelay = FrameDelay;
            held.SwitchDir(PS?.dir ?? held.PS.dir);

            LF2FrameData heldFrame = held.Frame?.D;
            int heldCx = heldFrame?.centerx ?? 0;
            int heldCy = heldFrame?.centery ?? 0;
            WeaponPoint heldWPoint = heldFrame?.wpoints != null && heldFrame.wpoints.Count > 0
                ? heldFrame.wpoints[0]
                : null;
            int heldWpx = heldWPoint?.x ?? 0;
            int heldWpy = heldWPoint?.y ?? 0;

            held.PS.x = held.PS.dir == "right"
                ? holdpoint.x + heldCx - heldWpx
                : holdpoint.x + heldWpx - heldCx;
            held.PS.y = holdpoint.y + heldCy - heldWpy;
            held.PS.z = Runtime != null ? Runtime.ZInt : (PS?.z ?? held.PS.z);

            int cover = holderWPoint.cover;
            int coverDiv = cover / 10;
            int coverRem = cover % 10;
            if (coverRem != 0)
            {
                held.PS.z += 1f;
                held.PS.y -= 1f;
            }
            else
            {
                held.PS.z -= 1f;
                held.PS.y += 1f;
            }

            if (coverDiv == 1)
                held.SwitchDir(PS?.dir ?? held.PS.dir);
            else if (coverDiv == 2)
                held.SwitchDir((PS?.dir ?? held.PS.dir) == "right" ? "left" : "right");
        }

        private void ApplyHeldObjectThrowVelocity(LF2Entity held, WeaponPoint holderWPoint)
        {
            held.PS.vx = PS?.dir == "left" ? -holderWPoint.dvx : holderWPoint.dvx;
            held.PS.vy = holderWPoint.dvy;

            bool up = InputState?.Up == true || Controller?.IsUp == true;
            bool down = InputState?.Down == true || Controller?.IsDown == true;
            if (up && !down)
                held.PS.vz = -holderWPoint.dvz;
            else if (!up && down)
                held.PS.vz = holderWPoint.dvz;

            held.PS.zz = 1f;
        }

        private void DropHeldObjectFromDamagedHolder(LF2Entity held)
        {
            held.ImmediateFrame(RandInt(0, 16));
            if (HitCount == 1)
            {
                held.PS.vx = KnockbackVx * (1f / 3f);
                held.PS.vy = KnockbackVy;
                held.PS.vz = KnockbackVz;
            }
            else
            {
                held.PS.vx = PS.vx * (1f / 3f);
                held.PS.vy = PS.vy;
                held.PS.vz = PS.vz;
            }

            if (held.PS.y < -2f)
                held.PS.y = -2f;

            ClearReleasedHeldObject(held, clearTeam: false);
        }

        private void DropHeldObjectRandomly(LF2Entity held)
        {
            held.ImmediateFrame(RandInt(0, 6));
            held.PS.vx = RandInt(0, 7) - 3f;
            held.PS.vy = -RandInt(0, 4);
            held.PS.vz = (RandInt(0, 5) - 2) * 0.2f;
            held.PS.zz = 0f;
            // C++ release AI_Process2 的 wp.kind==3 只解除持有关系并写随机抛落速度，
            // 不会在这里把被持有对象的 team 清零。
            ClearReleasedHeldObject(held, clearTeam: false);
        }

        private void ClearReleasedHeldObject(LF2Entity held, bool clearTeam)
        {
            if (clearTeam)
            {
                held.Team = 0;
                held.RelationTeam = 0;
            }

            held.GrabbedBy = 0;
            held.Runtime.LinkState = 0;
            LF2WeaponBase weapon = AsWeaponEntity(held);
            if (weapon != null)
                weapon.ForceClearHolder(preserveRuntimeOwnerFields: true);

            _heldWeapon = null;
            GrabbedBy = 0;
            Runtime.LinkState = 0;
        }

        protected override void RefreshRuntimeFromEntity()
        {
            base.RefreshRuntimeFromEntity();

            LF2Entity held = GetHeldEntity();
            Runtime.HeldWeaponStableId = held?.Runtime?.SlotIndex ?? -1;
            Runtime.TargetSlotIndex = held?.Runtime?.SlotIndex ?? -1;
            if (HasHeldObject())
                Runtime.LinkState = ResolveHeldWeaponLinkState(_heldWeapon);
            Runtime.Blink = _deadBlinkCount;
        }

        internal bool TryDropHeldWeaponFallbackRandomly()
        {
            LF2WeaponBase weapon = GetHeldWeaponBase();
            if (weapon == null)
                return false;

            ItrRest.Arest = 0;
            weapon.ItrRest.Arest = 0;

            weapon.SetFrameDirect(weapon.BattleRandInt(0, 6));
            weapon.PS.vx = weapon.BattleRandInt(0, 7) - 3;
            weapon.PS.vy = -weapon.BattleRandInt(0, 4);
            weapon.PS.vz = (weapon.BattleRandInt(0, 5) - 2) * 0.2f;
            weapon.PS.zz = 0;
            weapon.Team = 0;
            weapon.ForceClearHolder();
            HoldWeapon(null);
            return true;
        }

        private int ResolveHeldWeaponLinkState(ILF2Object weapon)
        {
            if (weapon is LF2Entity && weapon is not LF2WeaponBase)
                return 1;

            if (weapon is not LF2WeaponBase weaponBase)
                return 0;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(weaponBase.ObjectId);
            int typeSub = charData?.type_sub ?? 0;

            // C++ release 拾取路径：特殊 type_sub 优先，其次按武器 entity_type 写角色 link_state。
            if (typeSub == 0x78 || typeSub == 0x7C)
                return 101;
            if (weaponBase.IsHeavy)
                return 2;
            if (weaponBase.WeaponType == 4)
                return 4;
            if (weaponBase.WeaponType == 6)
                return weaponBase.Health?.HP > 0 ? 6 : 4;

            return 1;
        }

        private int ResolveHeldObjectLinkState(ILF2Object held)
        {
            if (held is LF2WeaponBase weaponBase && weaponBase.IsHeavy)
                return -2;

            return -1;
        }
    }
}
