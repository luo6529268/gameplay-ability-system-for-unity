using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        /// <summary>
        /// 攻击状态处理（state=3）。
        /// 输入、命中和帧推进由正式通用流程处理，这里只保留 state=3 特有的帧事件。
        /// </summary>
        private bool State_Attack(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    // 空中攻击结束时仍在空中，则切回跳跃空中帧。
                    var D = Frame.D;
                    if (D.next == LF2StandardFrames.LoopToStart && PS.vy < 0)
                    {
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 3, "Attack", LF2StandardFrames.JumpingAir, "空中攻击结束，返回跳跃");
                        Trans.SetNext(LF2StandardFrames.JumpingAir);
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
