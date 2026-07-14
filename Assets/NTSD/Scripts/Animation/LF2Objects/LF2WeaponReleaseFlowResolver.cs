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

        public void ReleaseHeldWeaponRuntime(LF2Entity holder)
        {
            ClearHolderRuntime(holder);
            ClearWeaponHolderRuntime(clearHolderSlot: true, clearHolderCopy: false);
        }

        public void ReleaseHeldWeaponForConsume(LF2Entity holder)
        {
            ClearHolderRuntime(holder);
            ClearWeaponHolderRuntime(clearHolderSlot: true, clearHolderCopy: true);
        }

        private static void ClearHolderRuntime(LF2Entity holder)
        {
            if (holder is LF2Character character)
            {
                character.HoldWeapon(null);
                return;
            }

            if (holder?.Runtime == null)
                return;

            holder.Runtime.HeldWeaponStableId = -1;
            holder.Runtime.TargetSlotIndex = -1;
            holder.Runtime.LinkState = 0;
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
