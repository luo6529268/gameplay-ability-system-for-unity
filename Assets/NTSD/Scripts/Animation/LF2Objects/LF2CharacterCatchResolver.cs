using NTSD.Animation;
using NTSD.Input;
using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 抓取/被抓状态机与 cpoint 抓取动作处理器。
    ///
    /// 当角色处于抓人（Catching, state 9）或被抓（BeingCaught, state 10）状态时，
    /// 状态事件以及全局 step10 阶段的 cpoint 动作选择、投掷、控向、持续伤害、位置同步
    /// 都由这个类负责。
    /// </summary>
    internal sealed class LF2CharacterCatchResolver
    {
        private readonly LF2Character _character;

        public LF2CharacterCatchResolver(LF2Character character)
        {
            _character = character;
        }

        public bool ProcessCatchingInput()
        {
            Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", "input", _character.CurrentFrameId);
            // C# authority advances catching action selection in the global step10 cpoint pass.
            // 输入阶段只保留按键状态，不在这里直接跳帧。
            return false;
        }

        public bool StateCatching(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, _character.CurrentFrameId);
                    _character.Runtime.CaughtDuration = 300;
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, _character.CurrentFrameId);
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, _character.CurrentFrameId);
                    return false;

                case "TU":
                    // C# authority advances catching in the global step10 pass.
                    return false;

                default:
                    return false;
            }
        }

        public bool StateBeingCaught(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, _character.CurrentFrameId);
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, _character.CurrentFrameId);
                    _character.Trans.SetWait(99);
                    return false;

                case "TU":
                    // C# authority synchronizes the caught entity in the global held-cpoint pass.
                    return false;

                default:
                    return false;
            }
        }

        public void RunCpointActionSelectionStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            if (victimEntity == null)
                return;

            bool attackReady = _character.Runtime.KeyJump != 0 && _character.Runtime.CdAttack > 0;
            bool jumpReady = _character.Runtime.KeyDefend != 0 && _character.Runtime.CdJump > 0;

            if (attackReady && cpoint.aaction != 0)
            {
                bool dirOk = (_character.Runtime.KeyLeft == 0 && _character.Runtime.KeyRight == 0) || cpoint.taction == 0;
                if (dirOk)
                    ApplyCpointActionStep10(cpoint.aaction, victimEntity);
            }

            if (attackReady && cpoint.taction != 0)
            {
                bool anyDir = _character.Runtime.KeyLeft != 0 ||
                    _character.Runtime.KeyRight != 0 ||
                    _character.Runtime.KeyUp != 0 ||
                    _character.Runtime.KeyDown != 0;
                if (anyDir)
                    ApplyCpointActionStep10(cpoint.taction, victimEntity);
            }

            if (jumpReady && cpoint.jaction != 0)
                ApplyCpointActionStep10(cpoint.jaction, victimEntity);
        }

        private void ApplyCpointActionStep10(int actionFrame, LF2Entity victim)
        {
            _character.ApplySignedImmediateFrameWaitReset(actionFrame);
            int victimAction = _character.Frame?.D?.cpoint?.vaction ?? 0;
            victim.DirectWriteFrameImmediateWaitReset(victimAction);
            victim.AttackingCounter = 0;
            _character.AttackingCounter = 0;
        }

        public void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            _character.ApplyCpointThrowStep10BaseInternal(cpoint, victimEntity);
        }

        public void ApplyCpointThrowStep10(
            CatchPoint cpoint,
            LF2Entity victimEntity,
            LF2FrameData throwFrameSnapshot)
        {
            _character.ApplyCpointThrowStep10BaseInternal(cpoint, victimEntity, throwFrameSnapshot);
        }

        public void SetVictimThrowVzStep10(CatchPoint cpoint, LF2Entity victim)
        {
            if (victim == null)
                return;

            victim.Runtime.Vz = 0f;
            if (_character.Runtime.KeyUp != 0 && _character.Runtime.KeyDown == 0)
                victim.Runtime.Vz = -cpoint.throwvz;
            else if (_character.Runtime.KeyUp == 0 && _character.Runtime.KeyDown != 0)
                victim.Runtime.Vz = cpoint.throwvz;
        }

        public void ApplyCpointDirControlStep10(CatchPoint cpoint)
        {
            if (_character.AttackingCounter != 2)
                return;

            if (cpoint.dircontrol == 1)
            {
                if (_character.Runtime.KeyRight != 0 && _character.Runtime.KeyLeft == 0) _character.SwitchDir("right");
                else if (_character.Runtime.KeyRight == 0 && _character.Runtime.KeyLeft != 0) _character.SwitchDir("left");
            }
            else if (cpoint.dircontrol == -1)
            {
                if (_character.Runtime.KeyRight != 0 && _character.Runtime.KeyLeft == 0) _character.SwitchDir("left");
                else if (_character.Runtime.KeyRight == 0 && _character.Runtime.KeyLeft != 0) _character.SwitchDir("right");
            }
        }

        public void ApplyCpointHeldInjuryStep10(LF2Entity victimEntity, int injury)
        {
            if (victimEntity == null || victimEntity.Health == null)
                return;

            if (injury > 0)
            {
                int actualInjury = injury;
                if (victimEntity.FallDamageDiv > 0)
                    actualInjury = injury * 100 / victimEntity.FallDamageDiv;

                if (victimEntity.Health.HP > 0 &&
                    actualInjury >= victimEntity.Health.HP &&
                    victimEntity.KillCount == -1)
                {
                    LF2Entity holder = _character.Match?.FindEntityByRuntimeSlotForQuery(_character.HolderCopySlot);
                    if (holder != null)
                        holder.KillStat++;

                }

                victimEntity.Health.HP -= actualInjury;
                victimEntity.Health.HPBound -= actualInjury / 3;
                victimEntity.ComboCountVic += actualInjury;
                _character.AttackingCounter = 1;
                _character.FrameDelay = 2;
                victimEntity.FrameDelay = -3;

                LF2Entity comboHolder = _character.Match?.FindEntityByRuntimeSlotForQuery(_character.HolderCopySlot);
                if (comboHolder != null)
                    comboHolder.ComboCountAtk += actualInjury;

                return;
            }

            // C# authority uses negative cpoint injury as the healing branch:
            // victim.hp += attacking; victim.hp_max += attacking / 3; attacker.attacking = 1。
            victimEntity.Health.HP += injury;
            victimEntity.Health.HPBound += injury / 3;
            _character.AttackingCounter = 1;
        }

        public void SyncCpointHeldPositionStep10(LF2Entity victimEntity, LF2FrameData catcherFrame, CatchPoint catcherCpoint)
        {
            if (victimEntity == null || catcherFrame == null || catcherCpoint == null)
                return;

            int catcherX = _character.Runtime != null ? _character.Runtime.XInt : (int)_character.PS.x;
            int catcherY = _character.Runtime != null ? _character.Runtime.YInt : (int)_character.PS.y;
            int catcherZ = _character.Runtime != null ? _character.Runtime.ZInt : (int)_character.PS.z;
            int dx = _character.Runtime.Dir == "right"
                ? catcherX - catcherFrame.centerx + catcherCpoint.x
                : catcherFrame.centerx - catcherCpoint.x + catcherX;
            int dy = catcherY - catcherFrame.centery + catcherCpoint.y;

            LF2FrameData victimCurrentFrame = victimEntity.Frame?.D;
            int victimCpointX = victimCurrentFrame?.cpoint?.x ?? 0;
            int victimCpointY = victimCurrentFrame?.cpoint?.y ?? 0;
            int victimCenterX = victimCurrentFrame?.centerx ?? 0;
            int victimCenterY = victimCurrentFrame?.centery ?? 0;

            victimEntity.Runtime.X = victimEntity.Runtime.Dir == "right"
                ? victimCenterX - victimCpointX + dx
                : victimCpointX - victimCenterX + dx;
            victimEntity.Runtime.Y = victimCenterY - victimCpointY + dy;
            victimEntity.Runtime.Z = catcherZ;

            int coverDiv = catcherCpoint.cover / 10;
            int coverRem = catcherCpoint.cover % 10;
            if (coverRem != 0)
            {
                victimEntity.Runtime.Z += 1f;
                victimEntity.Runtime.Y -= 1f;
            }
            else
            {
                victimEntity.Runtime.Z -= 1f;
                victimEntity.Runtime.Y += 1f;
            }

            if (coverDiv == 1)
                victimEntity.SwitchDir(_character.Runtime.Dir);
            else if (coverDiv == 2)
                victimEntity.SwitchDir(_character.Runtime.Dir == "right" ? "left" : "right");

            victimEntity.RefreshRuntimeSnapshot();
        }

    }
}
