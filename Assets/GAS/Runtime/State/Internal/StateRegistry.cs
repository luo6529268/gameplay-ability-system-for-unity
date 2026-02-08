using System.Collections.Generic;

namespace GAS.Runtime.State.Internal
{
    // Simple registry to map state IDs to concrete state implementations.
    public class StateRegistry
    {
        private readonly Dictionary<int, IState> _states = new Dictionary<int, IState>();

        public void RegisterState(IState state)
        {
            if (state == null) return;
            _states[state.StateId] = state;
        }

        public IState GetState(int id)
        {
            _states.TryGetValue(id, out var state);
            return state;
        }
    }
}
