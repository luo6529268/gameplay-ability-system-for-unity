using GAS.Runtime.State.Internal;

namespace GAS.Runtime.State.Internal
{
    // Minimal compatibility bridge to map external/legacy state IDs to internal IState instances.
    // This is a placeholder to ease gradual migration from LF2WeaponStates / LF2SpecialAttackStates.
    public static class CompatibilityBridge
    {
        // Example method to map legacy state id to internal state id
        public static int MapLegacyStateIdToInternal(int legacyStateId)
        {
            // TODO: implement actual mapping once legacy enum is known
            // For now, return legacyStateId as-is as a placeholder.
            return legacyStateId;
        }
    }
}
