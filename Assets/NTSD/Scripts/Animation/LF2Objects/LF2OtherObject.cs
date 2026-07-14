using NTSD.Animation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// type=5 其他对象。
    /// 当前主要覆盖 broken_weapon(oid=999) 的碎片、烟雾和转场效果。
    /// 逻辑以 C++ release 的 frame_advance / frame_tick / state==9998 清理链为准。
    /// </summary>
    public partial class LF2OtherObject : LF2Entity
    {
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
        public NTSDEntityCategory EntityCategory => NTSDEntityCategory.Effect;
        internal override bool UsesDynamicRuntimeSlot() => true;

        public override LF2ItrRestTracker ItrRest { get; protected set; }

        public override LF2Health Health { get; protected set; } = new LF2Health();
    }
}
