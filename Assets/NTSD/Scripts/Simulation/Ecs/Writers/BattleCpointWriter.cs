using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns cpoint state writes performed by the pre-interaction pass.
    /// Runtime-slot traversal order remains owned by the battle pipeline.
    /// </summary>
    internal sealed class BattleCpointWriter
    {
        internal void RunKind1(
            SimulationWorld world,
            LF2Entity attacker)
        {
            if (attacker?.Runtime == null)
                return;

            LF2FrameData catcherFrame = attacker.GetCollisionFrameData();
            if (catcherFrame == null ||
                !catcherFrame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue cpoint) ||
                cpoint.Kind != 1 ||
                attacker.FrameDelay < 0)
                return;

            LF2Entity victim = world?.FindEntityByRuntimeSlotForQuery(
                attacker.CaughtSlotIndex);
            if (victim?.Frame == null)
            {
                attacker.DirectWriteRawFramePreserveWaitCounter(0);
                return;
            }

            LF2FrameData victimFrame = victim.GetCollisionFrameData();
            bool skipActions = false;
            bool skipDecrease = false;
            bool useFallbackFrameForThrow = false;
            if (victim.CatcherSlotIndex != attacker.Runtime.SlotIndex ||
                victimFrame == null ||
                !victimFrame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue victimCpoint) ||
                victimCpoint.Kind != 2)
            {
                attacker.DirectWriteRawFramePreserveWaitCounter(0);
                skipActions = true;
                skipDecrease = true;
                useFallbackFrameForThrow = true;
            }

            if (!skipDecrease && cpoint.Decrease > 0)
            {
                attacker.Runtime.CaughtDuration -= cpoint.Decrease;
            }
            else if (!skipDecrease && cpoint.Decrease < 0)
            {
                attacker.Runtime.CaughtDuration += cpoint.Decrease;
                if (attacker.Runtime.CaughtDuration < 0)
                {
                    attacker.DirectWriteRawFramePreserveWaitCounter(0);
                    victim.DirectWriteRawFramePreserveWaitCounter(181);
                    attacker.HitCount = 1;
                    victim.HitCount = 1;
                    victim.KnockbackVx = attacker.Runtime.XInt > victim.Runtime.XInt
                        ? -4f
                        : 4f;
                    victim.KnockbackVy = -3f;
                    victim.Runtime.Vx = victim.KnockbackVx;
                    victim.Runtime.Vy = victim.KnockbackVy;
                    skipActions = true;
                    useFallbackFrameForThrow = true;
                }
            }

            if (!skipActions)
                RunActionSelection(attacker, victim, cpoint);

            if (cpoint.ThrowVx != 0)
            {
                LF2FrameData throwFrame = useFallbackFrameForThrow
                    ? attacker.Frame?.D
                    : catcherFrame;
                ApplyThrow(attacker, victim, cpoint, throwFrame);
            }

            ApplyDirControl(attacker, cpoint);
        }

        internal void RunKind2Validation(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (entity?.Runtime == null)
                return;

            LF2FrameData frame = entity.Frame?.D;
            if (frame == null ||
                !frame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue cpoint) ||
                cpoint.Kind != 2)
                return;

            bool valid = false;
            LF2Entity catcher = world?.FindEntityByRuntimeSlotForQuery(
                entity.CatcherSlotIndex);
            if (catcher != null &&
                catcher.CaughtSlotIndex == entity.Runtime.SlotIndex)
            {
                LF2FrameData catcherFrame = catcher.Frame?.D;
                valid = catcherFrame != null &&
                    catcherFrame.TryGetPrimaryCatchPoint(
                        out BattleCatchPointValue catcherCpoint) &&
                    catcherCpoint.Kind == 1;
            }

            if (valid)
                return;

            entity.SetCpointRawFramePreserveWait(212);
            entity.Runtime.Vy = -3f;
            if (entity.Runtime.Y > -2f)
                entity.Runtime.Y = -2f;
            entity.RefreshRuntimeSnapshot();
        }

        internal void SyncHeldCpoint(
            SimulationWorld world,
            LF2Entity attacker)
        {
            if (attacker?.Runtime == null)
                return;

            LF2FrameData currentFrame = attacker.Frame?.D;
            if (currentFrame == null ||
                !currentFrame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue cpoint) ||
                cpoint.Kind != 1 ||
                currentFrame.state != LF2States.Catching)
            {
                return;
            }

            LF2Entity victim = world?.FindEntityByRuntimeSlotForQuery(
                attacker.CaughtSlotIndex);
            if (victim == null ||
                victim.CatcherSlotIndex != attacker.Runtime.SlotIndex)
            {
                return;
            }

            LF2FrameData victimFrame = victim.Frame?.D;
            if (victimFrame == null ||
                !victimFrame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue victimCpoint) ||
                victimCpoint.Kind != 2)
                return;

            SyncCaughtByCpoint(world, attacker, victim, currentFrame, cpoint);
        }

        private void RunActionSelection(
            LF2Entity attacker,
            LF2Entity victim,
            BattleCatchPointValue cpoint)
        {
            bool attackReady = attacker.Runtime.KeyJump != 0 &&
                attacker.Runtime.CdAttack > 0;
            bool jumpReady = attacker.Runtime.KeyDefend != 0 &&
                attacker.Runtime.CdJump > 0;

            if (attackReady && cpoint.Aaction != 0)
            {
                bool directionAllowed =
                    (attacker.Runtime.KeyLeft == 0 &&
                     attacker.Runtime.KeyRight == 0) ||
                     cpoint.Taction == 0;
                if (directionAllowed)
                    ApplyAction(attacker, victim, cpoint.Aaction);
            }

            if (attackReady && cpoint.Taction != 0)
            {
                bool hasDirection = attacker.Runtime.KeyLeft != 0 ||
                    attacker.Runtime.KeyRight != 0 ||
                    attacker.Runtime.KeyUp != 0 ||
                    attacker.Runtime.KeyDown != 0;
                if (hasDirection)
                    ApplyAction(attacker, victim, cpoint.Taction);
            }

            if (jumpReady && cpoint.Jaction != 0)
                ApplyAction(attacker, victim, cpoint.Jaction);
        }

        private void ApplyAction(
            LF2Entity attacker,
            LF2Entity victim,
            int actionFrame)
        {
            attacker.ApplySignedCpointFrame(actionFrame);
            int victimAction = attacker.Frame?.D?.PrimaryCatchPoint.Vaction ?? 0;
            victim.DirectWriteRawFramePreserveWaitCounter(victimAction);
            victim.AttackingCounter = 0;
            attacker.AttackingCounter = 0;
        }

        private void SyncCaughtByCpoint(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            LF2FrameData catcherFrame,
            BattleCatchPointValue cpoint)
        {
            if ((cpoint.Hurtable == 0 ||
                 (victim.FrameDelay == 0 && cpoint.Hurtable == 1)) &&
                cpoint.Vaction != 0)
            {
                victim.DirectWriteRawFramePreserveWaitCounter(cpoint.Vaction);
            }

            if (victim.Frame?.N < 0)
            {
                victim.SwitchDir(
                    victim.Runtime.Dir == "left" ? "right" : "left");
                victim.SetCpointRawFramePreserveWait(-victim.Frame.N);
            }

            int injury = cpoint.Injury;
            if (injury != 0 && attacker.AttackingCounter == 0)
                ApplyHeldInjury(world, attacker, victim, injury);

            SyncHeldPosition(attacker, victim, catcherFrame, cpoint);
        }

        private void ApplyHeldInjury(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            int injury)
        {
            if (victim.Health == null)
                return;

            if (injury > 0)
            {
                int actualInjury = injury;
                if (victim.FallDamageDiv > 0)
                    actualInjury = injury * 100 / victim.FallDamageDiv;

                if (victim.Health.HP > 0 &&
                    actualInjury >= victim.Health.HP &&
                    victim.KillCount == -1)
                {
                    LF2Entity holder = world?.FindEntityByRuntimeSlotForQuery(
                        attacker.HolderCopySlot);
                    if (holder != null)
                        holder.KillStat++;

                    int killStatIndex = victim.Unk344;
                    if (world != null &&
                        world.KillStats != null &&
                        killStatIndex > 0 &&
                        killStatIndex < 3 &&
                        killStatIndex < world.KillStats.Length)
                    {
                        world.KillStats[killStatIndex]++;
                    }
                }

                victim.Health.HP -= actualInjury;
                victim.Health.HPBound -= actualInjury / 3;
                victim.ComboCountVic += actualInjury;
                attacker.AttackingCounter = 1;
                attacker.FrameDelay = 2;
                victim.FrameDelay = -3;

                LF2Entity comboHolder = world?.FindEntityByRuntimeSlotForQuery(
                    attacker.HolderCopySlot);
                if (comboHolder != null)
                    comboHolder.ComboCountAtk += actualInjury;

                int damageStatIndex = victim.Unk344;
                if (world != null &&
                    world.DamageStats != null &&
                    damageStatIndex > 0 &&
                    damageStatIndex < 3 &&
                    damageStatIndex < world.DamageStats.Length)
                {
                    world.DamageStats[damageStatIndex] += actualInjury;
                }
                return;
            }

            victim.Health.HP += injury;
            victim.Health.HPBound += injury / 3;
            attacker.AttackingCounter = 1;
        }

        private void SyncHeldPosition(
            LF2Entity attacker,
            LF2Entity victim,
            LF2FrameData catcherFrame,
            BattleCatchPointValue cpoint)
        {
            int dx = attacker.Runtime.Dir == "right"
                ? attacker.Runtime.XInt - catcherFrame.centerx + cpoint.X
                : catcherFrame.centerx - cpoint.X + attacker.Runtime.XInt;
            int dy = attacker.Runtime.YInt - catcherFrame.centery + cpoint.Y;

            LF2FrameData victimFrame = victim.Frame?.D;
            int victimCpointX = victimFrame?.PrimaryCatchPoint.X ?? 0;
            int victimCpointY = victimFrame?.PrimaryCatchPoint.Y ?? 0;
            int victimCenterX = victimFrame?.centerx ?? 0;
            int victimCenterY = victimFrame?.centery ?? 0;

            victim.Runtime.X = victim.Runtime.Dir == "right"
                ? victimCenterX - victimCpointX + dx
                : victimCpointX - victimCenterX + dx;
            victim.Runtime.Y = victimCenterY - victimCpointY + dy;
            victim.Runtime.Z = attacker.Runtime.ZInt;

            int coverDiv = cpoint.Cover / 10;
            int coverRem = cpoint.Cover % 10;
            if (coverRem != 0)
            {
                victim.Runtime.Z += 1f;
                victim.Runtime.Y -= 1f;
            }
            else
            {
                victim.Runtime.Z -= 1f;
                victim.Runtime.Y += 1f;
            }

            if (coverDiv == 1)
                victim.SwitchDir(attacker.Runtime.Dir);
            else if (coverDiv == 2)
            {
                victim.SwitchDir(
                    attacker.Runtime.Dir == "right" ? "left" : "right");
            }

            victim.Runtime.SyncIntegerPosition();
            victim.RefreshRuntimeSnapshot();
        }

        private void ApplyThrow(
            LF2Entity attacker,
            LF2Entity victim,
            BattleCatchPointValue cpoint,
            LF2FrameData throwFrameSnapshot)
        {
            int sourceNextFrameId = throwFrameSnapshot?.next ?? 0;
            LF2FrameData sourceNextFrame =
                attacker.FrameCache?.HasFrame(sourceNextFrameId) == true
                    ? attacker.FrameCache.GetFrameDataById(sourceNextFrameId)
                    : null;

            if (cpoint.ThrowInjury == -1 &&
                attacker.HasStep10ThrowTransformVictimData(victim))
            {
                attacker.ApplyCpointThrowTransformToSelfAndOwnedObjects(victim);
            }

            if (cpoint.ThrowInjury > 0)
                victim.WeaponCount = cpoint.ThrowInjury;

            LF2FrameData throwFrame = throwFrameSnapshot ??
                attacker.FrameCache?.GetFrameDataById(attacker.Frame?.N ?? 0) ??
                attacker.Frame?.D;

            int centerX = throwFrame?.centerx ?? 0;
            int centerY = throwFrame?.centery ?? 0;
            int y = attacker.Runtime.YInt - centerY + cpoint.Y;
            int x = attacker.Runtime.Dir == "right"
                ? attacker.Runtime.XInt - centerX + cpoint.X
                : centerX - cpoint.X + attacker.Runtime.XInt;

            victim.Runtime.X = x;
            victim.Runtime.Y = y;
            victim.Runtime.XInt = x;
            victim.Runtime.YInt = y;

            int nextFrame = throwFrame?.next ?? 0;
            attacker.SetCpointRawFramePreserveWait(nextFrame, sourceNextFrame);
            attacker.SetCpointRawPrevFrame2(nextFrame, sourceNextFrame);
            attacker.AttackingCounter = 0;

            victim.Runtime.Vx = attacker.Runtime.Dir == "right"
                ? cpoint.ThrowVx
                : -cpoint.ThrowVx;
            victim.Runtime.Vy = cpoint.ThrowVy;
            victim.Runtime.Vz = 0f;
            if (attacker.Runtime.KeyUp != 0 && attacker.Runtime.KeyDown == 0)
                victim.Runtime.Vz = -cpoint.ThrowVz;
            else if (attacker.Runtime.KeyUp == 0 && attacker.Runtime.KeyDown != 0)
                victim.Runtime.Vz = cpoint.ThrowVz;

            victim.SetCpointRawFramePreserveWait(cpoint.Vaction);
            victim.SetCpointRawPrevFrame2(cpoint.Vaction);
        }

        private void ApplyDirControl(
            LF2Entity attacker,
            BattleCatchPointValue cpoint)
        {
            if (attacker.AttackingCounter != 2)
                return;

            if (cpoint.DirControl == 1)
            {
                if (attacker.Runtime.KeyRight != 0 &&
                    attacker.Runtime.KeyLeft == 0)
                {
                    attacker.SwitchDir("right");
                }
                else if (attacker.Runtime.KeyRight == 0 &&
                         attacker.Runtime.KeyLeft != 0)
                {
                    attacker.SwitchDir("left");
                }
            }
            else if (cpoint.DirControl == -1)
            {
                if (attacker.Runtime.KeyRight != 0 &&
                    attacker.Runtime.KeyLeft == 0)
                {
                    attacker.SwitchDir("left");
                }
                else if (attacker.Runtime.KeyRight == 0 &&
                         attacker.Runtime.KeyLeft != 0)
                {
                    attacker.SwitchDir("right");
                }
            }
        }
    }
}
