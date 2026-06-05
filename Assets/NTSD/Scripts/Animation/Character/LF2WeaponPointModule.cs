using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    public interface ILF2WeaponPointFactory
    {
        /// <summary>
        /// 更新/同步 wpoint。具体的武器跟随、weaponact、攻击状态由外部工厂实现。
        /// </summary>
        void UpdateWeaponPoints(LF2LivingObject animator, LF2FrameData frameData, List<WeaponPoint> weaponPoints);
    }

    /// <summary>
    /// 读取当前帧的 wpoints，并在 transit 阶段委托给工厂处理。
    /// 行为以 C++ release 的持有武器同步和投掷逻辑为基准。
    /// </summary>
    public sealed class LF2WeaponPointModule
    {
        public ILF2WeaponPointFactory Factory { get; private set; }

        public void SetFactory(ILF2WeaponPointFactory factory)
        {
            Factory = factory;
        }

        public void Reset()
        {
            Factory = null;
        }

        public void ProcessTransit(LF2LivingObject animator)
        {
            if (animator == null) return;

            LF2FrameData frame = animator.Frame.D;
            if (frame == null) return;

            var wpoints = frame.wpoints;
            if (wpoints == null || wpoints.Count == 0) return;

            Factory?.UpdateWeaponPoints(animator, frame, wpoints);
        }
    }
}
