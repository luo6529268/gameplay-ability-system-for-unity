using NTSD.App;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2OtherObject
    {
        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null)
                return;

            Frame.D = targetFrame;
            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            if (!string.IsNullOrEmpty(Frame.D.sound))
                PlaySound(Frame.D.sound);

            if (switchDirAfterTrans && PS != null)
                SwitchDir(PS.dir == "right" ? "left" : "right");
        }

        public override void SimFrameTick(int tickIndex)
        {
            RunCommonFrameTick();
        }

        protected override bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            return Frame?.D != null && PS != null;
        }

        public override void SimTU(int tickIndex)
        {
            int dataType = GetCurrentDataObjectTypeForSimulation();
            if (dataType == (int)LF2ObjectType.Character)
            {
                RunSharedCharacterDatFrameAdvanceAsCharacter(tickIndex);
                return;
            }

            RunSharedNonCharacterDatFrameAdvance();
        }

        protected override bool FrameEvent()
        {
            return Frame?.D != null;
        }

        private void SetFrameDirect(int frameId, int waitCounter = int.MinValue)
        {
            if (frameId >= 0 && FrameCache?.HasFrame(frameId) != true)
                return;

            Frame.PN = Frame.N;
            Frame.N = frameId;
            Frame.D = FrameCache.GetFrameDataById(frameId);
            AttackingCounter = 0;

            if (Frame.D != null && Trans != null)
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
        }

        private static void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }
    }
}
