namespace NTSD.Animation.LF2Objects
{
    internal sealed class LF2OtherObjectFrameModule
    {
        private readonly LF2OtherObject owner;

        public LF2OtherObjectFrameModule(LF2OtherObject owner)
        {
            this.owner = owner;
        }

        public void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            owner.Frame.PN = owner.Frame.N;
            owner.Frame.N = targetFrameId;

            LF2FrameData targetFrame = owner.FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null)
                return;

            owner.Frame.D = targetFrame;
            owner.Trans.SyncDirectFrameData(owner.Frame.D.wait, owner.Frame.D.next);

            if (!string.IsNullOrEmpty(owner.Frame.D.sound))
                owner.QueueBattleSound(owner.Frame.D.sound);

            if (switchDirAfterTrans && owner.PS != null)
                owner.SwitchDir(owner.PS.dir == "right" ? "left" : "right");
        }

        public void SimFrameTick(int tickIndex)
        {
            owner.RunCommonFrameTickFromModule();
        }

        public bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            return owner.Frame?.D != null && owner.PS != null;
        }

        public void SimTU(int tickIndex)
        {
            int dataType = owner.GetCurrentDataObjectTypeForSimulation();
            if (dataType == (int)LF2ObjectType.Character)
            {
                owner.RunSharedCharacterFrameAdvanceFromModule(tickIndex);
                return;
            }

            owner.RunSharedNonCharacterFrameAdvanceFromModule();
        }

        public bool FrameEvent()
        {
            return owner.Frame?.D != null;
        }

        public void SetFrameDirect(int frameId, int waitCounter = int.MinValue)
        {
            if (frameId >= 0 && owner.FrameCache?.HasFrame(frameId) != true)
                return;

            owner.Frame.PN = owner.Frame.N;
            owner.Frame.N = frameId;
            owner.Frame.D = owner.FrameCache.GetFrameDataById(frameId);
            owner.AttackingCounter = 0;

            if (owner.Frame.D != null && owner.Trans != null)
            {
                owner.Trans.SyncDirectFrameData(
                    owner.Frame.D.wait,
                    owner.Frame.D.next,
                    waitCounter);
            }
        }
    }
}
