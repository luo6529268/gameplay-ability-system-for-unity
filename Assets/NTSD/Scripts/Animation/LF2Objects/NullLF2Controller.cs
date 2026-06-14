using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// opoint 生成角色的默认空控制器，不绑定玩家输入。
    /// 后续如果接入 AI，可以在不改生成流程的前提下替换掉它。
    /// </summary>
    public sealed class NullLF2Controller : ILF2Controller
    {
        public static readonly NullLF2Controller Instance = new NullLF2Controller();

        private NullLF2Controller()
        {
        }

        public bool IsUp => false;
        public bool IsDown => false;
        public bool IsLeft => false;
        public bool IsRight => false;
        public bool IsAttack => false;
        public bool IsJump => false;
        public bool IsDefend => false;
        public SimInputBuffer InputBuffer { get; set; }

        public int Dirv() => 1;

        public (int dx, int dz) GetMoveInput() => (0, 0);

        public void SetInputID(int inputId)
        {
        }
    }
}
