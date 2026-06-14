using UnityEngine;
using NTSD.Animation;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    public abstract partial class LF2WeaponBase
    {
        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null) return;

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
                StateExitEvent();

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                AttackingCounter = 0;
                StateEntryEvent();
                _lastState = Frame.D.state;
            }

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            if (!string.IsNullOrEmpty(Frame.D.sound))
                PlaySound(Frame.D.sound);
        }

        public override void SimFrameTick(int tickIndex)
        {
            RunCommonFrameTick();
        }

        public override void RunFrameLogicBeforeAdvance()
        {
            RunWeaponFrameLogicBeforeAdvance();
        }

        protected override bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            var fD = Frame?.D;
            var ps = PS;
            if (fD == null || ps == null)
                return false;

            if (WeaponType == 2 && fD.state == 2000 && (int)ps.y == 0 && Mathf.Abs(ps.vx) < 0.1f)
            {
                SetFrameTickDirect(20);
                return Frame?.D != null;
            }

            return true;
        }

        private void RunWeaponFrameLogicBeforeAdvance()
        {
            if (PS == null)
                return;

            int state = ResolveRuntimeWeaponState();

            // 对齐 C++ release 的 frame_logic：type 4/6 空中武器在 vx 超过阈值后切到 frame 40。
            if ((WeaponType == 4 || WeaponType == 6) &&
                state == LF2States.WeaponInSky &&
                (PS.vx > NTSDGlobal.Gameplay.WeaponBoomerangVxMax ||
                 PS.vx < NTSDGlobal.Gameplay.WeaponBoomerangVxMin))
            {
                SetFrameDirect(40);
                Runtime.WeaponState = LF2States.WeaponInSky;
                state = Runtime.WeaponState;
            }

            // 对齐 C++ release 的 frame_logic：state 1002 转入 2000，2000 逐步减速后再回落到 3000。
            if (state == LF2States.WeaponThrowing)
            {
                Runtime.WeaponState = LF2States.HeavyWeaponInSky;
                return;
            }

            if (state == LF2States.HeavyWeaponInSky)
            {
                PS.vx *= 0.5f;
                if (Mathf.Abs(PS.vx) < 0.5f)
                {
                    PS.vx = 0f;
                    Runtime.WeaponState = LF2States.ProjectileFlying;
                }
            }
        }

        protected internal void SetFrameDirect(int frameId, int waitCounter = int.MinValue)
        {
            Frame.N = frameId;
            Frame.D = FrameCache.GetFrameDataById(frameId);
            AttackingCounter = 0;
            if (Frame.D != null && Trans != null)
            {
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
            }
        }

        public override void SimTU(int tickIndex)
        {
            RunFrameAdvancePhysics();
            ConsumeForcedRuntimeIntPosition();
            RefreshRuntimeSnapshot();
        }

        internal bool TryRunLateBrokenWeaponCleanup()
        {
            if (_holdObj != null || !IsWeaponDestroyable() || GetFlightCounter() >= 0)
                return false;

            Runtime.WeaponFlightCounter = 0;
            PlaySound(WeaponBrokenSound);
            CreateBrokenEffect();
            OnTransitDestroy();
            return true;
        }

        internal override bool TryRunLatePostOpointCleanupPhase()
        {
            return TryRunLateBrokenWeaponCleanup();
        }

        internal override bool ApplyPreFrameXBounds(float stageWidth)
        {
            if (PS == null)
                return false;

            if ((ObjectId == 122 || ObjectId == 123) && Runtime.WeaponFlightCounter > 0)
            {
                if (PS.x < 10f) PS.x = 10f;
                if (PS.x > stageWidth - 10f) PS.x = stageWidth - 10f;
                return false;
            }

            return base.ApplyPreFrameXBounds(stageWidth);
        }

        protected override bool StateEntryEvent() => DispatchCurrentStateEvent("state_entry");

        protected override bool FrameEvent() => DispatchCurrentStateEvent("frame");

        protected override bool DieEvent()
        {
            RunDiePhase();
            return true;
        }

        private void RunFrameAdvancePhysics()
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return;

            if (_holdObj != null) return;
            if (IsBlockedByReleaseLinkOrCaughtCpoint()) return;

            int state = ResolveRuntimeWeaponState();
            switch (state)
            {
                case LF2States.WeaponOnHand:
                case 2001:
                    break;
                default:
                    _gravityToAdd = 0f;
                    WeaponFlightPhysics();
                    CharacterMechanics.WeaponDynamics(PS, _gravityToAdd);

                    if (PS.y < -0.0001f)
                        OnInFlightFrameUpdate();
                    break;
            }

            if (PS.y >= 0 && PS.vy > 0)
                OnLanded();

            if ((Frame?.D?.state ?? -1) != LF2States.Falling)
                FluteWeight = 0;

            if (WeaponType == 4 && _holdObj == null && PickerStableId >= 0)
                CheckBoomerangCatch();
        }

        public override void SimObjectInteraction(int tickIndex)
        {
            if (FrameDelay != 0) return;
            if (AttackExempt > 0) return;
            if (IsBlockedByHeldOrCaughtForWeaponPasses()) return;

            Interaction();
        }

        internal override bool SupportsObjectInteractionPhase() => true;

        private void CheckBoomerangCatch()
        {
            var world = SimulationTickDriver.Instance?.World;
            if (world == null) return;

            world.GetAllLivingObjects(_boomerangQueryCache);

            LF2LivingObject thrower = null;
            foreach (var obj in _boomerangQueryCache)
            {
                if ((obj.Runtime?.SlotIndex ?? -1) == PickerStableId) { thrower = obj; break; }
            }
            if (thrower == null || thrower.Health?.HP <= 0) return;

            float dx = Mathf.Abs(PS.x - thrower.PS.x);
            float dy = Mathf.Abs(PS.y - thrower.PS.y);
            if (dx >= 30f || PS.z <= thrower.PS.z - 80f || PS.z >= thrower.PS.z || dy >= 10f) return;

            PS.vx = 0f;
            PS.vy = 0f;
            PS.vz = 0f;
            SetFrameDirect(60);
            thrower.CatchTimer = 100;
        }

        private void RunDiePhase()
        {
            PlaySound(WeaponBrokenSound);
            CreateBrokenEffect();
        }

        protected bool IsBlockedByHeldOrCaughtForWeaponPasses()
        {
            if (_holdObj != null)
                return true;

            return IsBlockedByReleaseLinkOrCaughtCpoint();
        }
    }
}
