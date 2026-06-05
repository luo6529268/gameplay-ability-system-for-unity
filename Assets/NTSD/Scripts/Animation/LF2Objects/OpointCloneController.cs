using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// opoint 生成角色使用的控制器。
    /// C++ release 在 spawn_from_opoint 中把 type=0 子实体标记为 ai_controlled；
    /// Unity 这里保留同构入口，后续可以继续移植 prepare_ai_input 的完整 AI 决策。
    /// </summary>
    public sealed class OpointCloneController : ILF2Controller
    {
        public OpointCloneController()
        {
            InputBuffer = new SimInputBuffer();
        }

        public bool IsUp { get; private set; }
        public bool IsDown { get; private set; }
        public bool IsLeft { get; private set; }
        public bool IsRight { get; private set; }
        public bool IsAttack { get; private set; }
        public bool IsJump { get; private set; }
        public bool IsDefend { get; private set; }
        public SimInputBuffer InputBuffer { get; set; }

        public int Dirv()
        {
            if (IsUp && !IsDown) return -1;
            if (IsDown && !IsUp) return 1;
            return 1;
        }

        public (int dx, int dz) GetMoveInput()
        {
            int dx = 0;
            int dz = 0;
            if (IsRight && !IsLeft) dx = 1;
            else if (IsLeft && !IsRight) dx = -1;
            if (IsDown && !IsUp) dz = 1;
            else if (IsUp && !IsDown) dz = -1;
            return (dx, dz);
        }

        public void SetInputID(int inputId)
        {
        }

        public void PrepareInput(LF2Character self, int tickIndex, SimulationWorld world)
        {
            IsUp = false;
            IsDown = false;
            IsLeft = false;
            IsRight = false;
            IsAttack = false;
            IsJump = false;
            IsDefend = false;

            // C++ 的 prepare_ai_input 是完整 AI 决策系统。
            // 当前先保留同构入口，避免分身继续继承父角色按键。
        }
    }
}
