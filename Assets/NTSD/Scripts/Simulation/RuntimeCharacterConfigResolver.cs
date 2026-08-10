using System;
using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Resolves immutable DAT character configuration for one simulation world.
    /// The optional override is a cold-path test seam owned by the world instead
    /// of mutable process-wide state.
    /// </summary>
    public sealed class RuntimeCharacterConfigResolver
    {
        private Func<int, LF2CharacterDataWrapper> overrideResolver;

        public RuntimeCharacterConfigResolver()
        {
        }

        public RuntimeCharacterConfigResolver(
            Func<int, LF2CharacterDataWrapper> resolver)
        {
            overrideResolver = resolver;
        }

        public LF2CharacterDataWrapper Resolve(int objectId)
        {
            LF2CharacterDataWrapper wrapper = overrideResolver?.Invoke(objectId);
            if (wrapper != null)
                return wrapper;

            return CharacterAnimtorManager.Instance?.GetCharacterConfig(objectId);
        }

        internal void SetOverrideForSelfCheck(
            Func<int, LF2CharacterDataWrapper> resolver)
        {
            overrideResolver = resolver;
        }
    }
}
