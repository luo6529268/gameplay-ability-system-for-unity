using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleHeldInjuryEvent
    {
        internal BattleHeldInjuryEvent(int catcherSlot, int caughtSlot)
        {
            CatcherSlot = catcherSlot;
            CaughtSlot = caughtSlot;
        }

        internal int CatcherSlot { get; }
        internal int CaughtSlot { get; }
    }

    /// <summary>
    /// Owns cpoint state writes performed by the pre-interaction pass.
    /// Runtime-slot traversal order remains owned by the battle pipeline.
    /// </summary>
    internal sealed class BattleCpointWriter
    {
        private List<BattleHeldInjuryEvent> heldInjuryEventSink;

        internal void BeginHeldInjuryEventCollection(
            List<BattleHeldInjuryEvent> eventSink)
        {
            heldInjuryEventSink = eventSink;
        }

        internal void EndHeldInjuryEventCollection()
        {
            heldInjuryEventSink = null;
        }

        internal bool ShouldRunKind1Advance(LF2Entity entity)
        {
            if (entity?.Runtime == null)
                return false;

            LF2FrameData frame = entity.GetCollisionFrameData();
            return frame != null &&
                   frame.TryGetPrimaryCatchPoint(
                       out BattleCatchPointValue cpoint) &&
                   cpoint.Kind == 1 &&
                   entity.FrameDelay >= 0;
        }

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
                attacker.DirectWriteNativeRawFramePreserveWaitCounter(0);
                return;
            }

            LF2FrameData victimFrame = victim.GetCollisionFrameData();
            if (victim.Runtime.CatchSourceSlot90 != attacker.Runtime.SlotIndex ||
                victimFrame == null ||
                !victimFrame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue victimCpoint) ||
                victimCpoint.Kind != 2)
            {
                attacker.DirectWriteNativeRawFramePreserveWaitCounter(0);
                return;
            }

            if (cpoint.Decrease > 0)
            {
                attacker.Runtime.CaughtDuration -= cpoint.Decrease;
            }
            else if (cpoint.Decrease < 0)
            {
                attacker.Runtime.CaughtDuration += cpoint.Decrease;
                if (attacker.Runtime.CaughtDuration < 0)
                {
                    attacker.DirectWriteNativeRawFramePreserveWaitCounter(0);
                    victim.DirectWriteNativeRawFramePreserveWaitCounter(181);
                    attacker.AttackingCounter = 1;
                    victim.AttackingCounter = 1;
                    victim.KnockbackVx = attacker.Runtime.XInt > victim.Runtime.XInt
                        ? -4f
                        : 4f;
                    victim.KnockbackVy = -3f;
                    victim.Runtime.Vx = victim.KnockbackVx;
                    victim.Runtime.Vy = victim.KnockbackVy;
                    return;
                }
            }

            RunActionSelection(attacker, victim, cpoint);

            if (cpoint.ThrowVx != 0)
                ApplyThrow(world, attacker, victim, cpoint, catcherFrame);

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
                entity.Runtime.CatchSourceSlot90);
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
                victim.Runtime.CatchSourceSlot90 != attacker.Runtime.SlotIndex)
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
            // Alignment contract: NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001.
            var runtime = attacker.Runtime;
            bool attackReady = runtime.KeyJump != 0 && runtime.CdAttack > 0;
            bool horizontal = runtime.KeyLeft != 0 || runtime.KeyRight != 0;
            bool direction = horizontal || runtime.KeyUp != 0 || runtime.KeyDown != 0;
            int requested = 0;
            if (attackReady && (!horizontal || cpoint.Taction == 0))
                requested = cpoint.Aaction;
            if (attackReady && direction && cpoint.Taction != 0)
                requested = cpoint.Taction;
            if (runtime.KeyAttack != 0 && runtime.CdDefend > 0)
                requested = cpoint.Daction;
            if (runtime.PrevUp != 0 && runtime.CdUp > 0)
                requested = cpoint.Uzaction;
            if (runtime.PrevDown != 0 && runtime.CdDown > 0)
                requested = cpoint.Dzaction;
            if (runtime.Dir == "left")
            {
                if (runtime.PrevRight != 0 && runtime.CdRight > 0)
                    requested = cpoint.Faction;
                if (runtime.PrevLeft != 0 && runtime.CdLeft > 0)
                    requested = cpoint.Baction;
            }
            else
            {
                if (runtime.PrevLeft != 0 && runtime.CdLeft > 0)
                    requested = cpoint.Faction;
                if (runtime.PrevRight != 0 && runtime.CdRight > 0)
                    requested = cpoint.Baction;
            }
            if (runtime.KeyDefend != 0 && runtime.CdJump > 0)
                requested = cpoint.Jaction;
            if (requested != 0)
                ApplyAction(attacker, victim, requested);
        }

        private void ApplyAction(
            LF2Entity attacker,
            LF2Entity victim,
            int actionFrame)
        {
            if (actionFrame < 0)
            {
                attacker.SwitchDir(attacker.Runtime.Dir == "left" ? "right" : "left");
                actionFrame = -actionFrame;
            }
            attacker.SetCpointRawFramePreserveWait(actionFrame);
            int victimAction = attacker.Frame?.D?.PrimaryCatchPoint.Vaction ?? 0;
            victim.SetCpointRawFramePreserveWait(victimAction);
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
            if (cpoint.Hurtable == 0 ||
                (victim.FrameDelay == 0 && cpoint.Hurtable == 1))
            {
                // Alignment contract:
                // NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001.
                // Native settlement commits signed/zero vaction first, then
                // fences every remaining side effect on the new kind-2 frame.
                ApplySettlementVictimAction(victim, cpoint.Vaction);
                LF2FrameData postActionFrame = victim.Frame?.D;
                if (postActionFrame == null ||
                    !postActionFrame.TryGetPrimaryCatchPoint(
                        out BattleCatchPointValue postActionCpoint) ||
                    postActionCpoint.Kind != 2)
                {
                    return;
                }
            }

            int injury = cpoint.Injury;
            if (injury != 0 && attacker.AttackingCounter == 0)
                ApplyHeldInjury(world, attacker, victim, injury, cpoint.Cover);

            SyncHeldPosition(attacker, victim, catcherFrame, cpoint);
        }

        private static void ApplySettlementVictimAction(
            LF2Entity victim,
            int encodedAction)
        {
            int action = encodedAction;
            if (action < 0)
            {
                victim.SwitchDir(
                    victim.Runtime.Dir == "left" ? "right" : "left");
                action = -action;
            }
            victim.DirectWriteNativeRawFramePreserveWaitCounter(action);
        }

        private void ApplyHeldInjury(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            int injury,
            int cover)
        {
            if (attacker?.Runtime == null ||
                victim?.Runtime == null ||
                victim.Health == null ||
                injury <= 0)
            {
                return;
            }

            // Alignment contract:
            // NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001.
            LF2Entity resourceAttacker =
                BattleDamageWriter.ResolveNativeHitResourceAttacker(
                    world,
                    attacker.Runtime.SlotIndex);
            if (resourceAttacker?.Runtime != null)
            {
                BattleDamageWriter.ApplyNativeHitDisplaySteps(
                    victim.Runtime,
                    injury);
            }

            int damage = injury;
            if (victim.Runtime.IncomingDamageScale340 > 0)
            {
                damage = unchecked((int)(
                    (long)injury * 100L /
                    victim.Runtime.IncomingDamageScale340));
            }

            LF2Entity credit = ResolveHeldInjuryCredit(world, attacker);
            if (victim.Health.HP > 0 &&
                damage >= victim.Health.HP &&
                victim.Runtime.OrdinaryCreditGate2F4 == -1 &&
                credit?.Runtime != null)
            {
                credit.Runtime.KnockoutCount358 = unchecked(
                    credit.Runtime.KnockoutCount358 + 1);
                world?.RecordNativeKnockout(
                    victim.Runtime.SlotIndex,
                    credit.Runtime.SlotIndex,
                    credit.Runtime.SlotIndex);
            }

            victim.Health.HP -= damage;
            victim.Health.HPBound -= damage / 3;
            victim.Runtime.InputHpConsumedTotal34C = unchecked(
                victim.Runtime.InputHpConsumedTotal34C + damage);
            if (credit?.Runtime != null)
            {
                credit.Runtime.InputScoreTotal348 = unchecked(
                    credit.Runtime.InputScoreTotal348 + damage);
            }

            attacker.AttackingCounter = 1;
            if (cover != 3)
            {
                if (cover != 1)
                    attacker.FrameDelay = 2;
                if (cover != 2)
                    victim.FrameDelay = -3;
            }

            // Alignment contract:
            // NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001.
            // The pipeline consumes these physical slots only after the
            // complete live settlement scan has finished.
            heldInjuryEventSink?.Add(new BattleHeldInjuryEvent(
                attacker.Runtime.SlotIndex,
                victim.Runtime.SlotIndex));
        }

        private static LF2Entity ResolveHeldInjuryCredit(
            SimulationWorld world,
            LF2Entity attacker)
        {
            if (attacker?.Runtime == null)
                return null;

            LF2Entity credit = null;
            if (attacker.Runtime.OwnerSlotIndex >= 0)
            {
                credit = world?.FindEntityByRuntimeSlotForQuery(
                    attacker.Runtime.OwnerSlotIndex);
            }
            if (credit == null &&
                attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character)
            {
                credit = attacker;
            }
            return credit;
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
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            BattleCatchPointValue cpoint,
            LF2FrameData throwFrameSnapshot)
        {
            if (cpoint.ThrowInjury > 0)
            {
                ApplyThrowInjuryDisplayLead(
                    world,
                    attacker,
                    victim,
                    cpoint.ThrowInjury);
                victim.Runtime.EnvironmentState320 = cpoint.ThrowInjury;
                victim.Runtime.EnvironmentSourceSlot160 =
                    victim.Runtime.SlotIndex;
            }

            LF2FrameData throwFrame = throwFrameSnapshot ??
                attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame?.N ?? 0) ??
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
            // Alignment contract: NTSD28-Q06-CPOINT-THROW-NATIVE-RAW-BINDING-001.
            attacker.SetCpointRawFramePreserveWait(nextFrame);
            attacker.SetCpointRawPrevFrame2(nextFrame);
            attacker.AttackingCounter = 0;

            victim.Runtime.Vx = attacker.Runtime.Dir == "right"
                ? cpoint.ThrowVx
                : -cpoint.ThrowVx;
            victim.Runtime.Vy = cpoint.ThrowVy;
            bool depthUp = attacker.Runtime.KeyUp != 0;
            bool depthDown = attacker.Runtime.KeyDown != 0;
            if (depthUp != depthDown)
            {
                victim.Runtime.Vz = depthUp
                    ? -cpoint.ThrowVz
                    : cpoint.ThrowVz;
            }

            victim.SetCpointRawFramePreserveWait(cpoint.Vaction);
            victim.SetCpointRawPrevFrame2(cpoint.Vaction);
            victim.AttackingCounter = 0;
        }

        private static void ApplyThrowInjuryDisplayLead(
            SimulationWorld world,
            LF2Entity attackEffectSource,
            LF2Entity target,
            int injury)
        {
            LF2Entity resourceAttacker =
                BattleDamageWriter.ResolveNativeHitResourceAttacker(
                    world,
                    attackEffectSource.Runtime.SlotIndex);
            if (resourceAttacker?.Runtime == null)
                return;

            BattleDamageWriter.ApplyNativeHitDisplaySteps(
                target.Runtime,
                injury);
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
