namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// Owns held-weapon relationship teardown. Runtime slot/link fields are the
    /// authoritative relationship state; no parallel object-reference cache is kept.
    /// </summary>
    internal sealed class LF2WeaponReleaseFlowResolver
    {
        private readonly LF2WeaponBase weapon;

        public LF2WeaponReleaseFlowResolver(LF2WeaponBase weapon)
        {
            this.weapon = weapon;
        }

        public void ForceClearHolder(bool preserveRuntimeOwnerFields = false)
        {
            ClearWeaponHolderRuntime(
                clearHolderSlot: !preserveRuntimeOwnerFields,
                clearHolderCopy: !preserveRuntimeOwnerFields);
        }

        public void ReleaseHeldWeaponRuntime(LF2Entity holder, bool stampReleaseTick = false)
        {
            if (stampReleaseTick)
                weapon.Runtime.ReleaseTick = weapon.Match?.CurrentTickIndex ?? 0;
            ClearReleasedLinks(holder);
        }

        public void ReleaseHeldWeaponForConsume(LF2Entity holder)
        {
            weapon.Runtime.ReleaseTick = weapon.Match?.CurrentTickIndex ?? 0;
            ClearReleasedLinks(holder);
            if (holder?.Runtime != null)
                holder.Runtime.TargetSlotIndex = 0;
            weapon.Runtime.HolderStableId = 0;
        }

        private void ClearReleasedLinks(LF2Entity holder)
        {
            if (holder?.Runtime == null)
                return;

            holder.Runtime.LinkState = 0;
            if (holder.Runtime.HeldWeaponStableId == weapon.Runtime.SlotIndex)
            {
                holder.Runtime.HeldWeaponStableId = -1;
                holder.Runtime.ThrowFrameGuard = -1;
            }

            if (holder is LF2Character character)
                character.HeldWeaponReferenceInternal = null;

            weapon.GrabbedBy = 0;
            weapon.Runtime.LinkState = 0;
        }

        private void ClearWeaponHolderRuntime(bool clearHolderSlot, bool clearHolderCopy)
        {
            weapon.GrabbedBy = 0;
            weapon.Runtime.LinkState = 0;
            if (clearHolderSlot)
                weapon.Runtime.HolderStableId = -1;
            if (clearHolderCopy)
                weapon.HolderCopySlot = -1;
        }
    }
}
