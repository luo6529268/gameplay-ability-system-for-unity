namespace GAS.Runtime.State.Internal
{
    public interface IState
    {
        int StateId { get; }
        void Enter();
        void Update();
        void Exit();
    }
}
