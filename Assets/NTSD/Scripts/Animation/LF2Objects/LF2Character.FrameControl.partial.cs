using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        /// <summary>
        /// 武器点更新，执行当前帧 wpoint 行为。
        /// </summary>
        public void WPointUpdate()
        {
            WeaponPointModule?.ProcessTransit(this);
        }

        public bool IsHeavyWeapon()
        {
            return IsHeldHeavyWeapon();
        }

        /// <summary>
        /// 丢弃当前持有的武器。
        /// </summary>
        public void DropWeapon(float dvx = 0, float dvy = 0)
        {
            GetHeldWeaponBase()?.Drop(dvx, dvy);

            _heldWeapon = null;
            Runtime.HeldWeaponStableId = -1;
            Runtime.TargetSlotIndex = -1;
            Runtime.LinkState = 0;
        }

        internal void ForceReleaseHeldObjectReference(LF2Entity held)
        {
            if (held == null)
                return;

            if (ReferenceEquals(_heldWeapon, held))
                _heldWeapon = null;

            Runtime.HeldWeaponStableId = -1;
            Runtime.TargetSlotIndex = -1;
            Runtime.LinkState = 0;
        }

        public override void TransitionToFrame(int frameId)
        {
            Trans.Frame(frameId);
        }

        /// <summary>
        /// C++ release sub_414C30：输入/连招命中字段触发的直接跳帧。
        /// 该路径使用目标帧的 mp 字段检查并扣除 PP/HP，成功后直接进入目标帧。
        /// </summary>
        internal bool TryInputFrameJump(int frameId)
        {
            bool flipFacing = false;
            if (frameId < 0)
            {
                frameId = -frameId;
                flipFacing = true;
            }

            if (frameId == 999)
                frameId = 0;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null || Health == null)
                return false;

            if (NTSDGlobal.MPEnabled)
            {
                int ppCost = targetFrame.mp % 1000;
                int hpCost = (targetFrame.mp / 1000) * 10;
                if (Health.PP < ppCost || Health.HP <= hpCost)
                    return false;

                Health.HP -= hpCost;
                Health.PP -= ppCost;

                // C++ release 的负帧翻面只在 PP/HP 检查成功路径上执行。
                if (flipFacing)
                    SwitchDir(PS.dir == "right" ? "left" : "right");
            }

            OnFrameTransit(frameId, false);
            return true;
        }

        /// <summary>
        /// C++ release 普通攻击类动作的 PP 消耗门控。
        /// 跑攻/冲刺攻要求 PP 足够；站立拳/空中拳按 C++ release 扣到 0 后仍允许进帧。
        /// </summary>
        internal bool TrySpendFramePpCost(int frameId, bool clampOnOverdraw = false)
        {
            if (!NTSDGlobal.MPEnabled || Health == null)
                return true;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            int ppCost = targetFrame.mp;
            if (!clampOnOverdraw && Health.PP < ppCost)
                return false;

            Health.PP -= ppCost;
            if (Health.PP < 0)
                Health.PP = 0;
            return true;
        }

        public int CurrentFrameId => Frame.N;
        public LF2FrameData CurrentFrame => Frame.D;
        public int PreviousFrameId => Frame.PN;

        /// <summary>
        /// C++ release step4 后的 state==14 死亡/复活处理，在第一次 AI_Process2 前执行。
        /// </summary>
    }
}
