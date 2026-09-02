using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns the world-bound character action transaction entered after input
    /// resolution. The compatibility implementations still live on the entity
    /// adapters while U6 migrates their storage, but production callers cross
    /// this single composition boundary before mutating frame, facing, motion,
    /// HP/PP or action statistics.
    /// </summary>
    internal sealed class BattleCharacterActionWriter
    {
        private readonly LF2CharacterActionResolver releaseInputResolver =
            new LF2CharacterActionResolver();

        internal bool TryCharacterDatInputFrameJump(
            LF2Entity character,
            int frameId)
        {
            return character != null &&
                   character.TryCharacterDatInputFrameJumpCompatibility(frameId);
        }

        internal bool ProcessReleaseInput(LF2Character character)
        {
            if (character == null)
                return false;

            return releaseInputResolver.ProcessReleaseInput(character);
        }

        internal bool TryApplyExactCharacterFrameVelocityTail(
            LF2Character character)
        {
            if (character == null || character.GetType() != typeof(LF2Character))
                return false;

            LF2FrameData frame = character?.Frame?.D;
            NTSDEntityRuntime runtime = character?.Runtime;
            if (frame == null || runtime == null)
                return true;

            double vx = runtime.Vx;
            ApplyAxisVelocity(frame.dvx, ref vx, runtime.Dir == "left" ? -1 : 1);
            runtime.Vx = vx;

            if (frame.dvy > 500)
                runtime.Vy = frame.dvy - 550;
            else if (frame.dvy != 0)
                runtime.Vy += frame.dvy;

            if (frame.dvz > 500)
            {
                runtime.Vz = frame.dvz - 550;
                return true;
            }

            if (frame.dvz == 0)
                return true;

            if (runtime.KeyUp != 0 && runtime.CdUp >= runtime.CdDown)
                runtime.Vz = -frame.dvz;
            if (runtime.KeyDown != 0 && runtime.CdDown >= runtime.CdUp)
                runtime.Vz = frame.dvz;
            return true;
        }

        private void ApplyAxisVelocity(
            int value,
            ref double velocity,
            int direction)
        {
            if (value > 500)
            {
                velocity = value - 550;
                return;
            }

            if (value == 0)
                return;

            double target = value * direction;
            if (value > 0)
            {
                if (direction >= 0)
                {
                    if (velocity < target)
                        velocity = target;
                }
                else if (velocity > target)
                {
                    velocity = target;
                }

                return;
            }

            if (direction >= 0)
            {
                if (velocity > target)
                    velocity = target;
            }
            else if (velocity < target)
            {
                velocity = target;
            }
        }
    }
}
