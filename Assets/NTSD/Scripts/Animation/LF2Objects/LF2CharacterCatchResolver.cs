using NTSD.Animation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 抓取/被抓状态机与 cpoint 抓取动作处理器。
    ///
    /// 当角色处于抓人（Catching, state 9）或被抓（BeingCaught, state 10）状态时，
    /// 状态事件以及全局 step10 阶段的 cpoint 动作选择、投掷、控向、持续伤害、位置同步
    /// 都由这个类负责。
    /// </summary>
    internal sealed class LF2CharacterCatchResolver
    {
        private readonly LF2Character _character;

        public LF2CharacterCatchResolver(LF2Character character)
        {
            _character = character;
        }

        public bool ProcessCatchingInput()
        {
            // C# authority advances catching action selection in the global step10 cpoint pass.
            // 输入阶段只保留按键状态，不在这里直接跳帧。
            return false;
        }

        public bool StateCatching(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    _character.Runtime.CaughtDuration = 300;
                    return false;

                case "state_exit":
                    return false;

                case "frame":
                    return false;

                case "TU":
                    // C# authority advances catching in the global step10 pass.
                    return false;

                default:
                    return false;
            }
        }

        public bool StateBeingCaught(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    return false;

                case "frame":
                    _character.Trans.SetWait(99);
                    return false;

                case "TU":
                    // C# authority synchronizes the caught entity in the global held-cpoint pass.
                    return false;

                default:
                    return false;
            }
        }

    }
}
