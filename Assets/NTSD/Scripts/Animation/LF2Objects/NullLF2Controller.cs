using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// opoint 生成的角色默认不绑定玩家输入；后续 AI 可替换这个控制器。
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
