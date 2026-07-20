using BeatEmUpTemplate2D;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 角色交互结算处理器（release step7/step9 候选消费 + 被抓入口）。
    ///
    /// 负责按 step6 记录的 candidate 顺序逐条消费碰撞候选，并按 itr.kind 分派
    /// 攻击、抓取、拾取等类型；以及作为被抓方响应抓取者的 CaughtA 请求。
    /// </summary>
    internal sealed class LF2CharacterInteractionResolver
    {
        private readonly LF2Character _character;

        public LF2CharacterInteractionResolver(LF2Character character)
        {
            _character = character;
        }

        /// <summary>
        /// C++ release step7/step9 的角色候选消费主路径。
        /// 按 step6 记录下来的 candidate 顺序逐条消费，并按 itr.kind 当场分派，
        /// 避免 Unity 侧再拆成先攻击、后抓取/拾取两段，造成同 tick 顺序偏差。
        /// </summary>
        public bool TryConsumeUnifiedStep7CandidateSequence()
        {
            LF2FrameData frame = _character.GetCollisionFrameData();
            var sceneQuery = _character.Match?.SceneQuery;
            var kindService = _character.Match?.ItrKindService;
            if (frame?.itrs == null || sceneQuery == null || kindService == null)
                return false;

            if (!sceneQuery.TryGetCollisionCandidateSequence(_character, out var candidates) || candidates == null)
                return false;

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < candidateLimit; candidateIndex++)
            {
                SceneQueryHit hitInfo = candidates[candidateIndex];
                int itrIndex = hitInfo.ItrIndex;
                if (IsReleaseInvalidCandidateItrIndex(itrIndex, frame))
                    continue;

                LF2Entity target = hitInfo.ResolveCurrentTarget(_character.Match);
                if (target == null)
                    continue;

                bool zeroAttackerHpOnConsume;
                bool releaseHeavyHeldTargetOnConsume;
                InteractionArea itr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                    _character,
                    target,
                    frame,
                    frame.itrs[itrIndex],
                    out zeroAttackerHpOnConsume,
                    out releaseHeavyHeldTargetOnConsume);
                if (itr == null)
                    continue;

                hitInfo = new SceneQueryHit(
                    target,
                    hitInfo.BodyX,
                    hitInfo.ItrIndex,
                    itr,
                    zeroAttackerHpOnConsume,
                    releaseHeavyHeldTargetOnConsume);

                if (!CanConsumeRecordedCandidate(target))
                    continue;

                if (kindService.IsPreInteractionKind(itr.kind))
                {
                    TryApplyPreInteraction(itr, target);
                    continue;
                }

                if (itr.kind == 6)
                {
                    target.HitConfirmCounter = 3;
                    continue;
                }

                if (!kindService.IsAttackKind(itr.kind))
                    continue;
                _character.ApplyReleaseSceneQueryConsumeEffectsInternal(hitInfo);
                var attackerPos = new Vector3((float)_character.PS.x, (float)_character.PS.y, (float)_character.PS.z);
                _character.CurrentItrIndex = itrIndex;
                bool abortAfterSuccessfulHit = LF2HitResolveRuntimeData.ShouldAbortRemainingHitPairsAfterOid300Redirect(
                    target,
                    itr);
                int targetDataType = target.GetCurrentDataObjectTypeForSimulation();
                bool hit;
                if (targetDataType == (int)LF2ObjectType.Character)
                {
                    hit = target is LF2Character living
                        ? living.Hit(itr, _character, attackerPos, default)
                        : LF2CharacterDatHitResolver.CanResolveTarget(target) &&
                          LF2CharacterDatHitResolver.TryResolveHit(target, itr, _character, attackerPos, default);
                }
                else if ((targetDataType == (int)LF2ObjectType.LightWeapon ||
                          targetDataType == (int)LF2ObjectType.HeavyWeapon ||
                          targetDataType == (int)LF2ObjectType.ThrowWeapon ||
                          targetDataType == (int)LF2ObjectType.Drink) &&
                         target is LF2WeaponBase weapon)
                {
                    hit = weapon.Hit(itr, _character);
                }
                else if (targetDataType == (int)LF2ObjectType.SpecialAttack &&
                         target is LF2SpecialAttack specialAttack)
                {
                    hit = specialAttack.Hit(itr, _character);
                }
                else
                {
                    hit = false;
                }
                if (!hit)
                    continue;
                if (abortAfterSuccessfulHit)
                    return true;

            }

            return true;
        }

        private static bool IsReleaseInvalidCandidateItrIndex(int itrIndex, LF2FrameData collisionFrame)
        {
            return collisionFrame?.itrs == null || itrIndex < 0 || itrIndex >= collisionFrame.itrs.Count;
        }

        private bool CanConsumeRecordedCandidate(LF2Entity target)
        {
            if (target == null || target == _character || target.Runtime == null)
                return false;
            if (target.Runtime.PendingFlushDestroy || target.FrameCache == null)
                return false;

            int selfSlot = _character.Runtime?.SlotIndex ?? -1;
            return selfSlot < 0 || target.ItrVrestTest(selfSlot, true);
        }

        private static int GetDataObjectTypeForReleaseConsume(LF2Entity entity)
        {
            return LF2Entity.ResolveCurrentDataObjectType(entity);
        }

        internal bool TryApplyPreInteraction(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null)
                return false;

            switch (itr.kind)
            {
                case 1:
                    return target is LF2LivingObject lo1 && HandlePreInteractionKind(itr, lo1);
                case 3:
                    return TryApplyKind3Grab(_character, target, itr);
                case 2:
                    return HandlePreInteractionKind2(itr, target);
                case 7:
                    return HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        internal static bool TryApplyKind3Grab(LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            if (attacker?.Runtime == null || victim?.Runtime == null || itr == null)
                return false;
            if (LF2Entity.ResolveCurrentDataObjectType(victim) != (int)LF2ObjectType.Character)
                return false;

            int catchingFrame = itr.catchingact != null && itr.catchingact.Length > 0
                ? itr.catchingact[0]
                : LF2StandardFrames.Catching;
            int caughtFrame = itr.caughtact != null && itr.caughtact.Length > 0
                ? itr.caughtact[0]
                : LF2StandardFrames.PickedCaught;

            attacker.Runtime.Vx = 0.0;
            victim.Runtime.Vx = 0.0;

            int attackerXInt = attacker.Runtime.XInt;
            int attackerYInt = attacker.Runtime.YInt;
            int victimXInt = victim.Runtime.XInt;
            bool attackerFacesLeft = attackerXInt > victimXInt;
            attacker.SwitchDir(attackerFacesLeft ? "left" : "right");
            victim.SwitchDir(attackerFacesLeft ? "right" : "left");

            attacker.SetCpointRawFramePreserveWait(catchingFrame);
            victim.SetCpointRawFramePreserveWait(caughtFrame);

            attacker.Runtime.X = attackerXInt;
            attacker.Runtime.Y = attackerYInt;
            AlignKind3GrabPair(attacker, victim, attackerXInt, attackerYInt, victimXInt);

            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.Runtime.CaughtDuration = 300;
            victim.FallCounter = 0;
            attacker.RefreshRuntimeSnapshot();
            victim.RefreshRuntimeSnapshot();
            return true;
        }

        internal static bool TryApplyKind1Grab(LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            if (attacker?.Runtime == null || victim?.Runtime == null || itr == null)
                return false;
            if (LF2Entity.ResolveCurrentDataObjectType(victim) != (int)LF2ObjectType.Character)
                return false;

            int catchingFrame = itr.catchingact != null && itr.catchingact.Length > 0
                ? itr.catchingact[0]
                : LF2StandardFrames.Catching;
            int caughtFrame = itr.caughtact != null && itr.caughtact.Length > 0
                ? itr.caughtact[0]
                : LF2StandardFrames.PickedCaught;

            attacker.Runtime.Vx = 0.0;
            victim.Runtime.Vx = 0.0;

            int attackerXInt = attacker.Runtime.XInt;
            int attackerYInt = attacker.Runtime.YInt;
            int victimXInt = victim.Runtime.XInt;
            bool attackerFacesLeft = attackerXInt > victimXInt;
            attacker.SwitchDir(attackerFacesLeft ? "left" : "right");
            victim.SwitchDir(attackerFacesLeft ? "right" : "left");

            attacker.SetCpointRawFramePreserveWait(catchingFrame);
            victim.SetCpointRawFramePreserveWait(caughtFrame);

            victim.Runtime.X = victimXInt;
            victim.Runtime.Y = victim.Runtime.YInt;
            AlignKind3GrabPair(attacker, victim, attackerXInt, attackerYInt, victimXInt);

            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.Runtime.CaughtDuration = 300;
            victim.FallCounter = 0;
            attacker.RefreshRuntimeSnapshot();
            victim.RefreshRuntimeSnapshot();
            return true;
        }

        private static void AlignKind3GrabPair(
            LF2Entity attacker,
            LF2Entity victim,
            int attackerXInt,
            int attackerYInt,
            int victimXInt)
        {
            LF2FrameData attackerFrame = attacker.Frame?.D;
            LF2FrameData victimFrame = victim.Frame?.D;

            int attackerWact = attackerFrame?.cpoint?.x ?? 0;
            int victimWact = victimFrame?.cpoint?.x ?? 0;
            int attackerCx = attackerFrame?.centerx ?? 0;
            int attackerCy = attackerFrame?.centery ?? 0;
            int victimCx = victimFrame?.centerx ?? 0;
            int victimCy = victimFrame?.centery ?? 0;

            victim.Runtime.X = attacker.Runtime.Dir == "right"
                ? attackerXInt - attackerCx - victimCx + attackerWact + victimWact
                : attackerCx + victimCx + attackerXInt - attackerWact - victimWact;
            victim.Runtime.Y = victimCy - attackerCy + attackerYInt;

            double lerp = (victimXInt - victim.Runtime.X) * 0.5;
            victim.Runtime.X += lerp;
            attacker.Runtime.X += lerp;
            victim.Runtime.XInt = (int)victim.Runtime.X;
            attacker.Runtime.XInt = (int)attacker.Runtime.X;
        }

        /// <summary>
        /// 处理抓取类型的预交互（itr kind 1/3）。
        /// </summary>
        private bool HandlePreInteractionKind(InteractionArea itr, LF2LivingObject target)
        {
            // 只处理角色类型
            if (GetDataObjectTypeForReleaseConsume(target) != (int)LF2ObjectType.Character)
                return false;

            // 转换为 LF2Character 以调用 CaughtA
            var targetChar = target as LF2Character;
            if (targetChar == null)
                return false;

            // 调用被抓者的 CaughtA，获取抓取方向
            string dir = targetChar.CaughtA(itr, _character, new Vector3((float)_character.PS.x, (float)_character.PS.y, (float)_character.PS.z));
            if (dir == null)
                return false;

            // C++ release kind=1/3：抓取者写 itr.catchingact[0]，不是 effect。
            int catchFrame = itr.catchingact != null && itr.catchingact.Length > 0
                ? itr.catchingact[0]
                : LF2StandardFrames.Catching;
            _character.ImmediateFrame(catchFrame);

            // 设置抓取目标
            _character.Catching = target;
            _character.CaughtSlotIndex = target.Runtime?.SlotIndex ?? -1;
            target.CatcherSlotIndex = _character.Runtime?.SlotIndex ?? -1;

            ApplyImmediateCatchPairState(targetChar);

            return true;
        }

        /// <summary>
        /// C++ release kind=1/3 在 step7 成功后会立刻同步一组抓取状态：
        /// 双方 vx 清零、按 x 关系改朝向、写 caught_duration、victim fall=0，并立刻对位。
        /// 这些字段如果等到 step10 才补，会导致抓取建立后的当帧表现偏掉。
        /// </summary>
        private void ApplyImmediateCatchPairState(LF2Character targetChar)
        {
            if (targetChar == null || _character.PS == null || targetChar.PS == null)
                return;

            _character.PS.vx = 0f;
            targetChar.PS.vx = 0f;
            _character.KnockbackVx = 0f;
            targetChar.KnockbackVx = 0f;

            bool attackerFacesLeft = Mathf.RoundToInt((float)_character.PS.x) > Mathf.RoundToInt((float)targetChar.PS.x);
            _character.SwitchDir(attackerFacesLeft ? "left" : "right");
            targetChar.SwitchDir(attackerFacesLeft ? "right" : "left");

            _character.SetCaughtDurationInternal(300);
            targetChar.FallCounter = 0;
            if (targetChar.HitCounters != null)
                targetChar.HitCounters.ResetFall();

            LF2FrameData attackerFrame = _character.Frame?.D;
            LF2FrameData victimFrame = targetChar.Frame?.D;
            if (attackerFrame == null || victimFrame == null)
                return;

            int attackerWact = attackerFrame.cpoint?.x ?? 0;
            int victimWact = victimFrame.cpoint?.x ?? 0;
            int attackerCx = attackerFrame.centerx;
            int attackerCy = attackerFrame.centery;
            int victimCx = victimFrame.centerx;
            int victimCy = victimFrame.centery;

            int attackerXInt = Mathf.RoundToInt((float)_character.PS.x);
            int attackerYInt = Mathf.RoundToInt((float)_character.PS.y);
            int victimXInt = Mathf.RoundToInt((float)targetChar.PS.x);

            float victimNewX;
            if (_character.PS.dir == "right")
                victimNewX = attackerXInt - attackerCx - victimCx + attackerWact + victimWact;
            else
                victimNewX = attackerCx + victimCx + attackerXInt - attackerWact - victimWact;

            float victimNewY = victimCy - attackerCy + attackerYInt;
            targetChar.PS.x = victimNewX;
            targetChar.PS.y = victimNewY;

            float lerp = (victimXInt - victimNewX) * 0.5f;
            targetChar.PS.x += lerp;
            _character.PS.x += lerp;
        }

        private bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            return PickupWeapon(itr, target);
        }

        // 武器拾取共享逻辑。
        // playAnimation：kind=2 时播放拾取帧，kind=7 时不播。
        private bool PickupWeapon(InteractionArea itr, LF2Entity target, bool skipGroundCheck = false)
        {
            if (_character.HasHeldObjectInternal())
                return false;

            // kind=2 只允许拾取地面上的武器；kind=7 只检查 picker==0，不检查地面状态。
            if (!skipGroundCheck)
            {
                int wstate = target is LF2WeaponBase targetWeapon
                    ? targetWeapon.GetResolvedWeaponStateForExternalUse()
                    : target.GetState();
                bool isOnGround = wstate == LF2States.WeaponOnGround
                               || wstate == LF2States.WeaponJustOnGround
                               || wstate == LF2States.HeavyWeaponOnGround;
                if (!isOnGround)
                    return false;
            }

            if (!LF2CharacterDatInteractionResolver.TryApplyCurrentDatPickupCandidate(
                    _character,
                    target,
                    itr.kind))
                return false;

            _character.HeldWeaponReferenceInternal = target;
            return true;
        }

        private bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            // C++ release 0x0042E97B/0x0042E984：kind=7 近身拾取
            // 条件：target.picker==0（武器未被持有），无 att 键守卫，无重武器排除
            // 与 kind=2 相同逻辑，但不播放拾取动画帧
            return PickupWeapon(itr, target, skipGroundCheck: true);
        }

        /// <summary>
        /// 被抓取处理。
        /// 由抓取者调用，在被抓目标身上执行。
        /// 返回 true 表示抓取成功，catchSide 输出 "front"/"back"；返回 false 表示抓取失败。
        /// </summary>
        /// <param name="itr">抓取者的 itr 数据</param>
        /// <param name="attacker">抓取者</param>
        /// <param name="attackerPos">抓取者位置</param>
        /// <param name="catchSide">抓取方向 "front"/"back"，失败时为 null</param>
        public bool TryCaughtA(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos, out string catchSide)
        {
            catchSide = null;

            // 再次验证抓取条件。
            if (!((itr.kind == 1 && _character.GetState() == LF2States.Injured2) || itr.kind == 3))
                return false;

            // 判断正面/背面。
            bool isFront = (attackerPos.x > _character.PS.x) == (_character.PS.dir == "right");
            _character.SetCaughtFrontInternal(isFront);

            // C++ release kind=1/3：被抓者写 itr.caughtact[0]，不是 catchingact。
            int caughtFrame = itr.caughtact != null && itr.caughtact.Length > 0
                ? itr.caughtact[0]
                : LF2StandardFrames.PickedCaught;
            _character.ImmediateFrame(caughtFrame);

            // C++ release 里抓取会重置/改写部分受击计数；此处后续按正式流程校正。
            //if (Health != null) Health.Fall = 0;

            // 记录抓取者。
            _character.Catching = attacker;
            _character.CatcherSlotIndex = attacker?.Runtime?.SlotIndex ?? -1;
            if (attacker != null)
                attacker.CaughtSlotIndex = _character.Runtime?.SlotIndex ?? -1;

            // 被抓时丢弃当前武器。
            _character.DropWeapon();

            catchSide = isFront ? "front" : "back";
            return true;
        }
    }
}
