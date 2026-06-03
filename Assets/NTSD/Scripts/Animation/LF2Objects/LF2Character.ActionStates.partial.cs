using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        private bool State_Defending(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}", 7, "Defending", eventType);

                    if (Frame.N == LF2StandardFrames.Defend1)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 7, "Defending", "防御成功 → 延长等待时间");
                        Trans.IncWait(LF2StateConstants.DefendSuccessWaitBonus);
                    }
                    break;

                case "combo":
                    return ProcessDefendingInputCommand(eventData as string);
            }

            return false;
        }

        private bool State_Charging(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);
                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);
                    return false;

                case "combo":
                    return ProcessChargingInputCommand(eventData as string);

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);
                    return false;

                default:
                    return false;
            }
        }
    }
}
