using UnityEngine;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 持有武器/持有对象（held object）链路处理器。
    ///
    /// 负责角色对当前持有对象的绑定（HoldWeapon / AttachOpointHeldObject）、
    /// 按 holder 当前 wpoint 的同步与释放（ReleaseHeldObjectByWPoint）、
    /// 以及 link_state 解析。对齐 C++ release AI_Process2 的 held-object pass。
    /// </summary>
    internal sealed class LF2CharacterWeaponLinkResolver
    {
        private readonly LF2Character _character;

        public LF2CharacterWeaponLinkResolver(LF2Character character)
        {
            _character = character;
        }

        private LF2Entity GetHeldEntity()
        {
            int holderSlot = _character.Runtime?.SlotIndex ?? -1;
            int heldSlot = _character.Runtime?.TargetSlotIndex ?? -1;
            if ((_character.Runtime?.LinkState ?? 0) <= 0 || holderSlot < 0 || heldSlot < 0)
            {
                ClearStaleHeldReference();
                return null;
            }

            LF2Entity held = _character.Match?.FindEntityByRuntimeSlotForQuery(heldSlot);
            if (held == null)
            {
                held = _character.HeldWeaponReferenceInternal as LF2Entity;
                if ((held?.Runtime?.SlotIndex ?? -1) != heldSlot)
                    held = null;
            }

            if (held?.Runtime == null || held.Runtime.LinkState >= 0 ||
                held.Runtime.HolderStableId != holderSlot)
            {
                ClearStaleHeldReference();
                return null;
            }

            _character.HeldWeaponReferenceInternal = held;
            return held;
        }

        private void ClearStaleHeldReference()
        {
            _character.HeldWeaponReferenceInternal = null;
            if (_character.Runtime == null)
                return;
            _character.Runtime.LinkState = 0;
            _character.Runtime.TargetSlotIndex = -1;
            _character.Runtime.HeldWeaponStableId = -1;
        }

        public LF2WeaponBase GetHeldWeaponBase()
        {
            return GetHeldEntity() as LF2WeaponBase;
        }

        public bool HasHeldObject()
        {
            return GetHeldEntity() != null;
        }

        public bool IsHeldHeavyWeapon()
        {
            return GetHeldWeaponBase()?.IsHeavy == true;
        }

        private int GetHeldObjectId()
        {
            return GetHeldEntity()?.ObjectId ?? -1;
        }

        public bool IsHeldObjectAttackable()
        {
            int objectId = GetHeldObjectId();
            return objectId > 0 && NTSDSpec.IsWeaponAttackable(objectId);
        }

        public bool CanHeldObjectStandThrow()
        {
            int objectId = GetHeldObjectId();
            return objectId > 0 &&
                   (NTSDSpec.CanJustThrowWeapon(objectId) || NTSDSpec.CanStandThrowWeapon(objectId));
        }

        public bool CanHeldObjectRunThrow()
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

        /// <summary>
        /// 持有武器，并同步正式运行时的持有关系字段。
        /// </summary>
        public void HoldWeapon(ILF2Object weapon)
        {
            _character.HeldWeaponReferenceInternal = weapon;
            LF2Entity held = weapon as LF2Entity;
            _character.Runtime.HeldWeaponStableId = held?.Runtime?.SlotIndex ?? -1;
            _character.Runtime.TargetSlotIndex = held?.Runtime?.SlotIndex ?? -1;
            _character.Runtime.LinkState = ResolveHeldWeaponLinkState(weapon);

            if (held != null)
            {
                held.Runtime.HolderStableId = _character.Runtime?.SlotIndex ?? -1;
                held.HolderCopySlot = _character.Runtime?.SlotIndex ?? -1;
                held.Runtime.LinkState = ResolveHeldObjectLinkState(weapon);
                if (held.GrabbedBy == 0)
                    held.GrabbedBy = -1;
                held.Team = _character.Team;
                held.RelationTeam = _character.RelationTeam;
            }
        }

        /// <summary>
        /// opoint kind=2 生成后的持有绑定，对齐 C++ release spawn_from_opoint。
        /// </summary>
        public void AttachOpointHeldObject(LF2Entity held)
        {
            _character.HeldWeaponReferenceInternal = held;
            _character.Runtime.LinkState = 1;
            _character.Runtime.TargetSlotIndex = held?.Runtime?.SlotIndex ?? -1;
            _character.Runtime.HeldWeaponStableId = held?.Runtime?.SlotIndex ?? -1;

            if (held == null)
                return;

            held.GrabbedBy = -1;
            held.Runtime.LinkState = -1;
            held.Runtime.HolderStableId = _character.Runtime?.SlotIndex ?? -1;
            held.Runtime.TargetSlotIndex = -1;
            held.Runtime.HeldWeaponStableId = -1;
            held.HolderCopySlot = _character.HolderCopySlot;
            held.Team = _character.Team;
            held.RelationTeam = _character.RelationTeam;
        }

        /// <summary>
        /// 获取当前持有的武器
        /// </summary>
        public ILF2Object GetHeldWeapon()
        {
            return GetHeldEntity();
        }

        /// <summary>
        /// 按 C++ release AI_Process2 的 held-object pass 同步/释放当前持有对象。
        /// 武器仍复用 LF2WeaponBase.Act 的攻击细节；非武器 opoint kind=2 也会走同一释放规则。
        /// </summary>
        public bool ReleaseHeldObjectByWPoint(WeaponPoint holderWPoint, out WeaponActResult result)
        {
            return ReleaseHeldObjectByWPoint(GetHeldEntity(), holderWPoint, out result);
        }

        public bool DropHeldObjectByWPoint(WeaponPoint holderWPoint)
        {
            return DropHeldObjectByWPoint(GetHeldEntity(), holderWPoint);
        }

        private bool DropHeldObjectByWPoint(
            LF2Entity held,
            WeaponPoint holderWPoint)
        {
            if (holderWPoint == null || held == null || held.PS == null)
                return false;

            if (!ReferenceEquals(_character.HeldWeaponReferenceInternal, held))
                _character.HeldWeaponReferenceInternal = held;

            Vector3 holdpoint = CalcHeldObjectPoint(holderWPoint);
            SyncHeldObjectFrameAndPosition(held, holderWPoint, holdpoint);
            DropHeldObjectRandomly(held);
            return true;
        }

        /// <summary>
        /// C++ release AI_Process2 遍历 link_state&lt;0 对象后，按 holder 当前 wpoint 同步/释放。
        /// </summary>
        public bool ReleaseHeldObjectByWPoint(LF2Entity held, WeaponPoint holderWPoint, out WeaponActResult result)
        {
            result = default;
            if (holderWPoint == null || held == null || held.PS == null)
                return false;

            if (!ReferenceEquals(_character.HeldWeaponReferenceInternal, held))
                _character.HeldWeaponReferenceInternal = held;

            Vector3 holdpoint = CalcHeldObjectPoint(holderWPoint);

            if (holderWPoint.kind == 3)
            {
                return DropHeldObjectByWPoint(held, holderWPoint);
            }

            LF2WeaponBase weapon = AsWeaponEntity(held);
            if (weapon != null)
            {
                result = weapon.Act(_character, holderWPoint, holdpoint);
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
                    held.DirectWriteHeldFramePreserveWaitCounter(40);
                    ApplyHeldObjectThrowVelocity(held, holderWPoint);
                    ClearReleasedHeldObject(held, clearTeam: false);
                    result.Thrown = true;
                    return true;
                }

                if (objType == 2)
                {
                    held.DirectWriteHeldFramePreserveWaitCounter(_character.RandIntInternal(0, 6));
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
            var frame = _character.Frame?.D;
            if (_character.PS == null || frame == null)
                return Vector3.zero;

            int holderX = _character.Runtime != null ? _character.Runtime.XInt : (int)_character.PS.x;
            int holderY = _character.Runtime != null ? _character.Runtime.YInt : (int)_character.PS.y;
            int holderZ = _character.Runtime != null ? _character.Runtime.ZInt : (int)_character.PS.z;
            float x = _character.PS.dir == "right"
                ? holderX - frame.centerx + wpoint.x
                : holderX + frame.centerx - wpoint.x;
            float y = holderY - frame.centery + wpoint.y;
            return new Vector3(x, y, holderZ);
        }

        private void SyncHeldObjectFrameAndPosition(LF2Entity held, WeaponPoint holderWPoint, Vector3 holdpoint)
        {
            if (held == null || held.PS == null)
                return;

            held.DirectWriteHeldFramePreserveWaitCounter(holderWPoint.weaponact);
            held.SwitchDir(_character.PS?.dir ?? held.PS.dir);
            held.FrameDelay = _character.FrameDelay;

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
            held.PS.z = _character.Runtime != null ? _character.Runtime.ZInt : (_character.PS?.z ?? held.PS.z);
            held.PS.zz = 0f;

            int cover = holderWPoint.cover;
            if (cover == 0)
            {
                held.PS.z += 1f;
                held.PS.y -= 1f;
            }
            else
            {
                held.PS.z -= 1f;
                held.PS.y += 1f;
            }

            held.Runtime.SyncIntegerPosition();
        }

        private void ApplyHeldObjectThrowVelocity(LF2Entity held, WeaponPoint holderWPoint)
        {
            held.PS.vx = _character.PS?.dir == "left" ? -holderWPoint.dvx : holderWPoint.dvx;
            held.PS.vy = holderWPoint.dvy;

            bool up = _character.Runtime.KeyUp != 0;
            bool down = _character.Runtime.KeyDown != 0;
            if (up && !down)
                held.PS.vz = -holderWPoint.dvz;
            else if (!up && down)
                held.PS.vz = holderWPoint.dvz;

            held.PS.zz = 0f;
        }

        private void DropHeldObjectFromDamagedHolder(LF2Entity held)
        {
            held.DirectWriteHeldFramePreserveWaitCounter(_character.RandIntInternal(0, 16));
            if (_character.HitCount == 1)
            {
                held.PS.vx = _character.KnockbackVx * (1f / 3f);
                held.PS.vy = _character.KnockbackVy;
                held.PS.vz = _character.KnockbackVz;
            }
            else
            {
                held.PS.vx = _character.PS.vx * (1f / 3f);
                held.PS.vy = _character.PS.vy;
                held.PS.vz = _character.PS.vz;
            }

            if (held.PS.y < -2f)
                held.PS.y = -2f;

            ClearReleasedHeldObject(held, clearTeam: false);
        }

        private void DropHeldObjectRandomly(LF2Entity held)
        {
            held.DirectWriteHeldFramePreserveWaitCounter(_character.RandIntInternal(0, 6));
            held.PS.vx = _character.RandIntInternal(0, 7) - 3f;
            held.PS.vy = -_character.RandIntInternal(0, 4);
            held.PS.vz = (_character.RandIntInternal(0, 5) - 2) * 0.2f;
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

            _character.HeldWeaponReferenceInternal = null;
            _character.GrabbedBy = 0;
            _character.Runtime.LinkState = 0;
            if (_character.Runtime.HeldWeaponStableId == held.Runtime.SlotIndex)
            {
                _character.Runtime.HeldWeaponStableId = -1;
                _character.Runtime.ThrowFrameGuard = -1;
            }
        }

        /// <summary>
        /// 强制释放当前持有对象引用（仅清运行时字段，不动被持有对象的位置/速度）。
        /// 原实现位于 LF2Character.FrameControl.partial.cs。
        /// </summary>
        public void ForceReleaseHeldObjectReference(LF2Entity held)
        {
            if (held == null)
                return;

            if (ReferenceEquals(_character.HeldWeaponReferenceInternal, held))
                _character.HeldWeaponReferenceInternal = null;

            _character.Runtime.HeldWeaponStableId = -1;
            _character.Runtime.TargetSlotIndex = -1;
            _character.Runtime.LinkState = 0;
        }

        internal bool TryDropHeldWeaponFallbackRandomly()
        {
            LF2WeaponBase weapon = GetHeldWeaponBase();
            if (weapon == null)
                return false;

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

        // ------------------------------------------------------------------
        // 以下 4 个方法为 LF2Character.cs 中已存在的委派调用点
        // (_weaponLinkResolver.X())，但其方法体在当前 partial 及整个 git 历史
        // 中均无法找到（在提交 c793097f 引入委派 stub 时从未编写 resolver 实现）。
        //
        // 依据 CLAUDE.md「不得实现反汇编中不存在的逻辑 / 若无法确认逻辑来源必须暂停」，
        // 此处保留为无操作占位（不臆造任何逻辑），并在交付报告中标记为阻塞项，
        // 待编排者从 NTSD 反汇编溯源后补齐真实实现。
        //
        // 现状说明：
        //  - ClearConsumedHeldWeaponReference / ClearReleasedHeldWeaponReference /
        //    ClearHolderLinkRuntimeOnly：主文件中的对应 internal 包装方法目前无任何调用者。
        //  - RunWeaponSyncHeldStep10：由 SimulationWorld.Passes.partial.cs:579 经
        //    override 调用；其 base.RunWeaponSyncHeldStep10()（LF2Entity）已执行 cpoint
        //    抓取同步，角色特化的持有武器同步是否需要额外逻辑尚待反汇编确认。
        // ------------------------------------------------------------------

        public void ClearConsumedHeldWeaponReference(LF2Entity held)
        {
            // TODO(NTSD): 无可溯源实现，待补齐。见上方说明。
        }

        public void ClearReleasedHeldWeaponReference(LF2Entity held)
        {
            // TODO(NTSD): 无可溯源实现，待补齐。见上方说明。
        }

        public void ClearHolderLinkRuntimeOnly()
        {
            // TODO(NTSD): 无可溯源实现，待补齐。见上方说明。
        }

        public void RunWeaponSyncHeldStep10()
        {
            GetHeldEntity();
        }
    }
}
