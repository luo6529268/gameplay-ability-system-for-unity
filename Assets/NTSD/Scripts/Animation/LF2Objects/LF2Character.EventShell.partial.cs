namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        protected override bool FrameEvent()
        {
            return RunFramePhase() || DispatchCurrentStateEvent("frame");
        }

        protected override bool TUEvent()
        {
            return RunTUPhase() || DispatchCurrentStateEvent("TU");
        }

        protected override bool TransitEvent()
        {
            return RunTransitPhase() || DispatchCurrentStateEvent("transit");
        }

        protected override bool StateExitEvent()
        {
            return RunStateExitPhase() || DispatchCurrentStateEvent("state_exit");
        }

        protected override bool StateEntryEvent()
        {
            return DispatchCurrentStateEvent("state_entry");
        }

        private bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            return (Frame.D?.state ?? -1) switch
            {
                LF2States.Standing => State_Standing(eventType, eventData),
                LF2States.Walking => State_Walking(eventType, eventData),
                LF2States.Running => State_Running(eventType, eventData),
                LF2States.Attack => State_Attack(eventType, eventData),
                LF2States.Jump => State_Jump(eventType, eventData),
                LF2States.Dash => State_Dash(eventType, eventData),
                LF2States.Rowing => State_Rowing(eventType, eventData),
                LF2States.Catching => State_Catching(eventType, eventData),
                LF2States.BeingCaught => State_BeingCaught(eventType, eventData),
                LF2States.Injured => State_Injured(eventType, eventData),
                LF2States.Falling => State_Falling(eventType, eventData),
                LF2States.Frozen => State_Frozen(eventType, eventData),
                LF2States.Lying => State_Lying(eventType, eventData),
                LF2States.StopRunning => State_StopRunning(eventType, eventData),
                LF2States.Burning => State_Burning(eventType, eventData),
                _ => false,
            };
        }

        protected override void ComboUpdate()
        {
            InputState?.ApplyFrameInput(this);
        }

        internal void RunTuCoreForSelfCheck()
        {
            RunTUCore();
        }
    }
}
