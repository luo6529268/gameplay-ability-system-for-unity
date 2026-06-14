using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    public interface ILF2WeaponPointFactory
    {
        /// <summary>
        /// 按当前帧的 wpoint 执行武器挂点与 weaponact 逻辑。
        /// </summary>
        void UpdateWeaponPoints(LF2LivingObject animator, LF2FrameData frameData, List<WeaponPoint> weaponPoints);
    }

    /// <summary>
    /// 处理角色帧上的 wpoint transit。该模块只负责转发到工厂，
    /// 具体行为仍由 C++ release 映射后的武器点实现决定。
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
