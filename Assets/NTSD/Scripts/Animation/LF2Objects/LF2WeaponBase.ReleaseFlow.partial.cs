namespace NTSD.Animation.LF2Objects
{
    public abstract partial class LF2WeaponBase
    {
        protected override bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Runtime == null)
                return false;

            if (WeaponType == 2 &&
                frame.state == LF2States.HeavyWeaponInSky &&
                Runtime.YInt == 0 &&
                System.Math.Abs(Runtime.Vx) < 0.1)
            {
                SetFrameTickDirect(20);
                return Frame?.D != null;
            }

            return true;
        }
    }
}
