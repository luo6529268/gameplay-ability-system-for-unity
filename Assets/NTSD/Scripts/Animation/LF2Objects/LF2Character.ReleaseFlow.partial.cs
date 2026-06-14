using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        public override void SimTransit(int tickIndex)
        {
            RunReleaseFrameAdvance(consumeInitialRuntimePosition: true);
        }

        private bool RunReleaseFrameAdvance(bool consumeInitialRuntimePosition)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return false;

            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return false;

            ApplyDynamics();

            if (consumeInitialRuntimePosition)
                ConsumeForcedRuntimeIntPosition();

            return true;
        }

        public override void SimFrameTick(int tickIndex)
        {
            RunCommonFrameTick();

            int frameN = Frame?.N ?? 0;
            if (frameN == 110 || frameN == 114)
                InputState?.SetDefendLock(3);
        }

        protected override void ApplyCommonCaughtExitHitStop()
        {
            int oid = FrameCache?.Wrapper?.characterId ?? -1;
            if (!(oid / 10 == 3 && oid != 38))
                HitStun = 15;
        }

        protected override bool IsFrameTickLeftPressed()
        {
            return InputState?.Left == true || Controller?.IsLeft == true;
        }

        protected override bool IsFrameTickRightPressed()
        {
            return InputState?.Right == true || Controller?.IsRight == true;
        }

        protected override void ApplyFrame212JumpInit()
        {
            if ((Frame?.N ?? -1) != 212)
                return;

            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null || PS == null)
                return;

            PS.vy = characterData.jump_height;

            bool right = IsFrameTickRightPressed();
            bool left = IsFrameTickLeftPressed();
            bool up = InputState?.Up == true || Controller?.IsUp == true;
            bool down = InputState?.Down == true || Controller?.IsDown == true;

            if (right && !left)
            {
                PS.vx = characterData.jump_distance;
                SwitchDir("right");
            }
            else if (left && !right)
            {
                PS.vx = -characterData.jump_distance;
                SwitchDir("left");
            }

            if (up && !down)
                PS.vz = -characterData.jump_distancez;
            else if (down && !up)
                PS.vz = characterData.jump_distancez;
        }

        public override int ResolveFrameTickNext999Target(out bool allowJumpInit)
        {
            allowJumpInit = false;
            return PS != null && (int)PS.y != 0 ? 212 : 0;
        }
    }
}
