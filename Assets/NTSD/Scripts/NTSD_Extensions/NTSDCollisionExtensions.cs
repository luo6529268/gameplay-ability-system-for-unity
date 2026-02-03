using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD 碰撞系统扩展
    /// 提供与 LF2CollisionSystem 集成的辅助方法
    /// </summary>
    public static class NTSDCollisionExtensions
    {
        /// <summary>
        /// 判断 ITR 是否为攻击类 Kind (包含 NTSD 扩展)
        /// </summary>
        public static bool IsAttackKindExtended(int kind)
        {
            // 原版 FLF kind
            if (kind == 0 || kind == 4 || kind == 9 || kind == 15 || kind == 16)
                return true;
            
            // NTSD 扩展: kind 800-896 (治疗/所有者相关)
            if (NTSDItrKindHandler.IsNTSDAttackKind(kind))
                return true;
            
            return false;
        }
        
        /// <summary>
        /// 判断 ITR 是否应该命中目标 (考虑 NTSD 扩展 Kind 的目标过滤)
        /// 在 LF2CollisionSystem.ProcessPostInteractionTick 的 Intersect 检测后调用
        /// </summary>
        public static bool ShouldHitTarget(int kind, ILF2LivingObject attacker, ILF2LivingObject target)
        {
            // 原版 Kind 不做过滤
            if (!NTSDItrKindHandler.IsNTSDAttackKind(kind))
                return true;
            
            // NTSD 扩展 Kind 需要目标过滤
            return NTSDItrKindHandler.ShouldHitTarget(kind, attacker, target);
        }
        
        /// <summary>
        /// 处理命中事件 (包含 NTSD 扩展效果)
        /// </summary>
        public static void ProcessHit(InteractionArea itr, ILF2LivingObject attacker, ILF2LivingObject target)
        {
            // 1. 计算伤害
            var damageResult = NTSDDamageCalculator.ProcessDamage(itr, attacker, target);
            
            // 2. 处理 NTSD 扩展 Effect
            if (NTSDEffectHandler.IsNTSDEffect(itr.effect))
            {
                var effectResult = NTSDEffectHandler.ProcessEffect(itr.effect, damageResult.finalDamage, attacker, target);
                NTSDEffectHandler.ApplyEffectResult(effectResult, attacker, target);
            }
            
            // 3. 检查目标的 State 9xxx (受击跳帧)
            if (target != null && target.Frame.D != null)
            {
                var parsed = NTSDStateHandler.ParseState(target.Frame.D.state);
                if (parsed.type == NTSDStateHandler.ExtendedStateType.HitGoto)
                {
                    target.TransitionToFrame(parsed.param1, 10);
                }
            }
        }
        
        /// <summary>
        /// 处理控制类 ITR (100099-100103)
        /// 在帧更新时调用
        /// </summary>
        public static void ProcessControlItrs(ILF2LivingObject actor)
        {
            if (actor == null || actor.Frame.D == null) return;
            
            var itrs = actor.Frame.D.itrs;
            if (itrs == null || itrs.Count == 0) return;
            
            foreach (var itr in itrs)
            {
                if (!NTSDItrKindHandler.IsNTSDControlKind(itr.kind)) continue;
                
                switch (itr.kind)
                {
                    case NTSDConstants.ITR_KIND_RANDOM_MOVE:
                        NTSDItrKindHandler.ProcessRandomMove(actor, itr);
                        break;
                        
                    case NTSDConstants.ITR_KIND_INPUT_CONTROL:
                        var targetFrame = NTSDItrKindHandler.ProcessInputControl(actor, itr);
                        if (targetFrame.HasValue)
                        {
                            actor.TransitionToFrame(targetFrame.Value, 0);
                        }
                        break;
                }
            }
        }
    }
}
