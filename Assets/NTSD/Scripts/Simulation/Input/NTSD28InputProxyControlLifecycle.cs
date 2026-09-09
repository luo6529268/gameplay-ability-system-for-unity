namespace NTSD.Simulation
{
    internal static class NTSD28InputProxyControlLifecycle
    {
        internal static void ApplyConfirmedMimicCounter(
            NTSDEntityRuntime target,
            int payload)
        {
            if (target != null)
                target.InputProxyCounter14C = payload;
        }

        internal static bool TryEnableFromConfirmedHit(
            NTSDEntityRuntime attacker,
            NTSDEntityRuntime target)
        {
            if (attacker == null || target == null ||
                attacker.ObjType != 0 || target.ObjType != 0 ||
                attacker.SlotIndex < 0 ||
                target.InputProxyCounter14C <= 0 ||
                target.InputProxyEnabled17C == 1)
            {
                return false;
            }

            // Alignment contract: NTSD28-B2-PROXY-CONTROL-LIFECYCLE-001.
            target.InputProxySourceSlot178 = attacker.SlotIndex;
            target.InputProxyEnabled17C = 1;
            return true;
        }

        internal static void AdvanceReactionTail(
            NTSDEntityRuntime runtime,
            bool nativeEntityBodySkipped,
            int currentHp)
        {
            if (runtime == null)
                return;

            if (!nativeEntityBodySkipped && currentHp > 0 &&
                runtime.InputProxyCounter14C > 0)
            {
                runtime.InputProxyCounter14C--;
            }

            if (runtime.InputProxyCounter14C <= 0 &&
                runtime.InputProxyEnabled17C == 1)
            {
                runtime.InputProxyEnabled17C = 0;
            }
        }
    }
}
