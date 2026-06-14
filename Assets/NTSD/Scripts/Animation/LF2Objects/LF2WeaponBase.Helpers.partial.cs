using UnityEngine;
using NTSD.App;
using NTSD.Animation;

namespace NTSD.Animation.LF2Objects
{
    public abstract partial class LF2WeaponBase
    {
        public void WhirlwindForce(InteractionArea itr, LF2Entity attacker)
        {
            if (attacker?.PS == null || PS == null) return;

            int state = ResolveRuntimeWeaponState();
            bool lightLike = WeaponType == 1 || WeaponType == 4 || WeaponType == 6;
            bool heavyLike = WeaponType == 2;

            if (lightLike)
            {
                if (ObjectId == 201 || ObjectId == 202) return;
                if (state != LF2States.WeaponInSky)
                    SetFrameDirect(0);
                ApplyWhirlwindVelocity(attacker, 3f);
            }
            else if (heavyLike)
            {
                if (state != LF2States.HeavyWeaponInSky)
                    SetFrameDirect(0);
                ApplyWhirlwindVelocity(attacker, 2.3f);
            }
        }

        private void ApplyWhirlwindVelocity(LF2Entity attacker, float vyDelta)
        {
            KnockbackVx = PS.vx + ((PS.x > attacker.PS.x) ? -1f : 1f);
            PS.vx = KnockbackVx;

            KnockbackVz = PS.vz + ((PS.z > attacker.PS.z) ? -0.5f : 0.5f);
            PS.vz = KnockbackVz;

            if (PS.y >= -2f)
            {
                PS.y = -2f;
                PS.vy = -6f;
            }

            if (PS.vy > -6f)
            {
                PS.vy -= vyDelta;
                KnockbackVy = PS.vy;
            }
        }

        public override void FluteForce()
        {
        }

        protected void CoincideXYWithWPoint(Vector3 holdpoint, WeaponPoint wpoint)
        {
            var weapFD = Frame?.D;
            int wcx = weapFD?.centerx ?? 0;
            int wcy = weapFD?.centery ?? 0;
            int wpx = wpoint?.x ?? 0;
            int wpy = wpoint?.y ?? 0;

            if (PS.dir == "right")
                PS.x = holdpoint.x + wcx - wpx;
            else
                PS.x = holdpoint.x + wpx - wcx;

            PS.y = holdpoint.y + wcy - wpy;
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }

        public void CreateBrokenEffect()
        {
            SpawnBrokenWeaponFragments(ObjectId);
        }

        protected Vector3 MakePointCenter(LF2FrameData frame)
        {
            float x = PS?.x ?? 0f;
            float y = PS?.y ?? 0f;
            float z = GetDisplayZ();

            return new Vector3(x, y, z);
        }

        protected void CoincideXYForInit(Vector3 targetPos, Vector3 selfPoint)
        {
            float vx = targetPos.x - selfPoint.x;
            float vz = targetPos.z - selfPoint.z;
            PS.x += vx;
            PS.z += vz;
        }
    }
}
