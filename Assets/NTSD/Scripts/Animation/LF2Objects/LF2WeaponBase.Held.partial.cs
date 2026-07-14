namespace NTSD.Animation.LF2Objects
{
    public abstract partial class LF2WeaponBase
    {
        public virtual void Drop(double dvx, double dvy)
        {
            LF2Entity holder = GetRuntimeHolderEntity();
            Team = 0;
            ReleaseHeldWeaponRuntimeInternal(holder);
            Runtime.WeaponState = 0;

            ImmediateFrame(RandInt(0, 16));
            Runtime.WeaponState = 0;
            Runtime.Vx = dvx * (1.0 / 3.0);
            Runtime.Vy = dvy;

            if (Runtime.Y < -2.0)
                Runtime.Y = -2.0;

            PS.zz = 0;
        }
    }
}
