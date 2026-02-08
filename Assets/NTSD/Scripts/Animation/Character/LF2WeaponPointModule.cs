using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    public interface ILF2WeaponPointFactory
    {
        /// <summary>
        /// 更新/同步 wpoint（武器跟随、weaponact、攻击状态等）。
        /// 由外部系统实现（武器实体/装备系统/网络同步等）。
        /// </summary>
        void UpdateWeaponPoints(LF2LivingObject animator, LF2FrameData frameData, List<WeaponPoint> weaponPoints);
    }

    /// <summary>
    /// FLF 对齐：character.wpoint() 的驱动入口（纯数据层，不继承 Mono）。
    /// 模块职责：在 transit 阶段读取当前帧的 wpoints 并委托给外部注入的 factory。
    /// </summary>
    public sealed class LF2WeaponPointModule
    {
        public ILF2WeaponPointFactory Factory { get; private set; }

        public void SetFactory(ILF2WeaponPointFactory factory)
        {
            Factory = factory;
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

