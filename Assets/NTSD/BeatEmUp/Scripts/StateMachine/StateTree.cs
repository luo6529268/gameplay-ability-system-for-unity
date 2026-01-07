using System.Linq;

namespace BeatEmUpTemplate2D
{
    public class StateTree
    {
        public StateNode Current;

        public void Tick()
        {
            if (Current == null) return;

            // 更新当前状态
            Current.Tick();

            // 检查可用的转换
            var transition = Current.Transitions.FirstOrDefault(t => t.CanTransit());
            if (transition != null)
            {
                //SwitchTo(transition.To);
            }
        }

        public void SwitchTo(StateNode next)
        {
            if (Current == next) return;
            Current?.Exit();
            Current = next;
            Current.Enter();
        }
    }
}