using System;

namespace NTSD.Animation.LF2Objects
{
    // NOTE: Placeholder for future module extraction.
    // Currently unused; kept compiling to avoid breaking Unity compilation.
    internal sealed class LF2CharacterStateModule
    {
        private readonly LF2Character _owner;

        public LF2CharacterStateModule(LF2Character owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public void InitializeStates()
        {
        }

        public bool GenericStateUpdate(string eventType, object eventData = null)
        {
            return false;
        }

        public bool StateHandler(int stateId, string eventType, object eventData = null)
        {
            return false;
        }
    }
}
