using NTSD.App;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using UnityEngine;

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
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return;

            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return;

            ApplyFrameVelocityForFrameAdvance();
            RunFrameAdvancePhysics();
            ConsumeForcedRuntimeIntPosition();
            RefreshRuntimeSnapshot();
        }

        protected override bool FrameEvent()
        {
            return Frame?.D != null;
        }

        private void ApplyFrameVelocityForFrameAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || PS == null)
                return;

            float vx = PS.vx;
            ApplyFrameAxisVelocity(frame.dvx, ref vx, Dirh());
            PS.vx = vx;

            if (frame.dvy > 500)
                PS.vy = frame.dvy - 550;
            else if (frame.dvy != 0)
                PS.vy += frame.dvy;

            if (frame.dvz > 500)
                PS.vz = frame.dvz - 550;
            else if (frame.dvz != 0)
                PS.vz += frame.dvz;
        }

        private static void ApplyFrameAxisVelocity(int value, ref float velocity, int direction)
        {
            if (value > 500)
            {
                velocity = value - 550;
                return;
            }

            if (value == 550)
            {
                velocity = 0f;
                return;
            }

            if (value > 0)
            {
                float target = value * direction;
                if (direction >= 0)
                {
                    if (velocity < target)
                        velocity = target;
                }
                else
                {
                    if (velocity > target)
                        velocity = target;
                }

                return;
            }

            if (value < 0)
            {
                float target = value * direction;
                if (direction >= 0)
                {
                    if (velocity > target)
                        velocity = target;
                }
                else
                {
                    if (velocity < target)
                        velocity = target;
                }
            }
        }

        private void RunFrameAdvancePhysics()
        {
            if (PS == null)
                return;

            float newY = PS.y + PS.vy;
            CharacterMechanics.WeaponDynamics(PS, GetOtherObjectGravity());

            if (ObjectId == 999 && IsBrokenFragment() && newY > -0.0001f)
            {
                PS.y = 0f;
                PS.vx = 0f;
                PS.vy = 0f;
                PS.vz = 0f;
                SetFrameDirect(101, 0);
                AttackingCounter = 0;
            }
        }

        private float GetOtherObjectGravity()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return NTSDGlobal.Gameplay.WeaponGravityDefault;

            return frame.state == 1002
                ? NTSDGlobal.Gameplay.WeaponGravityDefault1002
                : NTSDGlobal.Gameplay.WeaponGravityDefault;
        }

        private bool IsPureTransitionSmoke()
        {
            if (ObjectId != 999)
                return false;

            if (Runtime != null &&
                Runtime.SpawnSemantic == (int)ReleaseSpawnSemantic.TransitionEffect)
                return true;

            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return false;

            if (frame.state == 3005)
                return true;

            return frame.pic == 999 && frame.next == 1000;
        }

        private bool IsBrokenFragment()
        {
            return ObjectId == 999 &&
                   Runtime != null &&
                   Runtime.SpawnSemantic == (int)ReleaseSpawnSemantic.BrokenFragment;
        }


        private void SetFrameDirect(int frameId, int waitCounter = int.MinValue)
        {
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
