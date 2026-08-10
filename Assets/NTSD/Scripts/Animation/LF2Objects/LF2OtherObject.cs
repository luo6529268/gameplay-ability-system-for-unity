using NTSD.Animation;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// type=5 其他对象。
    /// 当前主要覆盖 broken_weapon(oid=999) 的碎片、烟雾和转场效果。
    /// 逻辑以 C++ release 的 frame_advance / frame_tick / state==9998 清理链为准。
    /// </summary>
    public class LF2OtherObject : LF2Entity
    {
        private readonly LF2OtherObjectFrameModule frameModule;
        private readonly LF2OtherObjectLifecycleModule lifecycleModule;

        public LF2OtherObject()
        {
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();
            Trans = new FrameTransistor(this);
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);
            frameModule = new LF2OtherObjectFrameModule(this);
            lifecycleModule = new LF2OtherObjectLifecycleModule(this, frameModule);
        }

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
        public NTSDEntityCategory EntityCategory => NTSDEntityCategory.Effect;
        public long InvalidInitTaskTypeCountForDiagnostics =>
            lifecycleModule.InvalidTaskTypeCountForDiagnostics;
        internal override bool UsesDynamicRuntimeSlot() => true;

        public override LF2ItrRestTracker ItrRest { get; protected set; }

        public override LF2Health Health { get; protected set; } = new LF2Health();

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            frameModule.OnFrameTransit(targetFrameId, switchDirAfterTrans);
        }

        public override void SimFrameTick(int tickIndex)
        {
            frameModule.SimFrameTick(tickIndex);
        }

        protected override bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            return frameModule.ApplyObjectSpecificFrameTickBeforeWaitAdvance();
        }

        public override void SimTU(int tickIndex)
        {
            frameModule.SimTU(tickIndex);
        }

        protected override bool FrameEvent()
        {
            return frameModule.FrameEvent();
        }

        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            lifecycleModule.Init(taskBase, renderer);
        }

        public override void Reset()
        {
            lifecycleModule.Reset();
        }

        internal void AssignRendererFromLifecycle(LF2ObjectRenderer renderer)
        {
            Renderer = renderer;
        }

        internal string CalculateDirectionFromLifecycle(int facing, string parentDir)
        {
            return CalculateDirection(facing, parentDir);
        }

        internal bool RunCommonFrameTickFromModule()
        {
            return RunCommonFrameTick();
        }

        internal void RunSharedCharacterFrameAdvanceFromModule(int tickIndex)
        {
            RunSharedCharacterDatFrameAdvanceAsCharacter(tickIndex);
        }

        internal bool RunSharedNonCharacterFrameAdvanceFromModule()
        {
            return RunSharedNonCharacterDatFrameAdvance();
        }

        internal void ResetReusableRuntimeComponentsFromLifecycle()
        {
            ResetReusableRuntimeComponents();
        }

        internal void ResetSparkFromLifecycle()
        {
            ResetSpark();
        }

        internal void ResetStableIdFromLifecycle()
        {
            ResetStableId();
        }
    }
}
