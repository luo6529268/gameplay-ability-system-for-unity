namespace NTSD.Animation
{
    public interface ILF2ObjectPointFactory
    {
        /// <summary>
        /// 创建/处理 opoint 生成对象。
        /// 由外部系统实现（例如对象池/Prefab 管理/联网同步等），模块本身不触碰 Unity Instantiate。
        /// </summary>
        void CreateObject(LF2CharacterAnimator animator, LF2FrameData frameData, ObjectPoint opoint);
    }

    /// <summary>
    /// FLF 对齐：character.opoint() 的驱动入口（纯数据层，不继承 Mono）。
    /// 模块职责：在帧更新时检测 opoint，并委托给外部注入的 factory 创建对象。
    /// </summary>
    public sealed class LF2ObjectPointModule
    {
        public ILF2ObjectPointFactory Factory { get; private set; }

        public void SetFactory(ILF2ObjectPointFactory factory)
        {
            Factory = factory;
        }

        public void ProcessFrame(LF2CharacterAnimator animator)
        {
            if (animator == null) return;

            LF2FrameData frame = animator.CurrentFrame;
            if (frame == null) return;

            ObjectPoint op = frame.opoint;
            if (op == null) return;
            if (op.oid <= 0) return;

            Factory?.CreateObject(animator, frame, op);
        }
    }
}

