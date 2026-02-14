using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Extensions
{
    public interface INTSDItrKindService
    {
        bool IsAttackKind(int kind);
        bool IsPreInteractionKind(int kind);
        bool IsNTSDAttackKind(int kind);
        bool IsNTSDControlKind(int kind);
        bool ShouldHitTarget(int kind, LF2LivingObject attacker, LF2LivingObject target);
        void ProcessRandomMove(LF2LivingObject actor, InteractionArea itr);
        int? ProcessInputControl(LF2LivingObject actor, InteractionArea itr);
    }

    /// <summary>
    /// NTSD ITR Kind 语义服务（业务规则层）
    /// </summary>
    public class NTSDItrKindService : INTSDItrKindService
    {
        #region Kind 分类判断

        public bool IsAttackKind(int kind)
        {
            if (kind == 0 || kind == 4 || kind == 9 || kind == 15 || kind == 16)
                return true;

            return IsNTSDAttackKind(kind);
        }

        public bool IsPreInteractionKind(int kind)
        {
            return kind == 1 || kind == 2 || kind == 3 || kind == 7;
        }
        
        /// <summary>
        /// 判断是否为 NTSD 扩展的攻击类 Kind
        /// </summary>
        public bool IsNTSDAttackKind(int kind)
        {
            // 800-896: 治疗/所有者相关
            if (kind >= 800 && kind <= 896) return true;
            
            return false;
        }
        
        /// <summary>
        /// 判断是否为 NTSD 高级控制 Kind (100099-100103)
        /// </summary>
        public bool IsNTSDControlKind(int kind)
        {
            return kind >= NTSDConstants.ITR_KIND_RANDOM_MOVE && 
                   kind <= NTSDConstants.ITR_KIND_CONTROLLABLE_MARKER;
        }
        
        /// <summary>
        /// 判断是否为治疗队友 Kind (800-806)
        /// </summary>
        public bool IsHealTeammateKind(int kind)
        {
            return kind >= NTSDConstants.ITR_KIND_HEAL_TEAMMATE_START && 
                   kind <= NTSDConstants.ITR_KIND_HEAL_TEAMMATE_END;
        }
        
        /// <summary>
        /// 判断是否为治疗敌人 Kind (810-816)
        /// </summary>
        public bool IsHealEnemyKind(int kind)
        {
            return kind >= NTSDConstants.ITR_KIND_HEAL_ENEMY_START && 
                   kind <= NTSDConstants.ITR_KIND_HEAL_ENEMY_END;
        }
        
        /// <summary>
        /// 判断是否为所有者专属 Kind (880-886)
        /// </summary>
        public bool IsOwnerOnlyKind(int kind)
        {
            return kind >= NTSDConstants.ITR_KIND_OWNER_ONLY_START && 
                   kind <= NTSDConstants.ITR_KIND_OWNER_ONLY_END;
        }
        
        /// <summary>
        /// 判断是否为所有者队伍 Kind (890-896)
        /// </summary>
        public bool IsOwnerTeamKind(int kind)
        {
            return kind >= NTSDConstants.ITR_KIND_OWNER_TEAM_START && 
                   kind <= NTSDConstants.ITR_KIND_OWNER_TEAM_END;
        }
        
        #endregion
        
        #region 目标过滤
        
        /// <summary>
        /// 判断 ITR 是否应该命中目标
        /// 基于 NTSD DLL 的 z_adds_1.inc 逻辑
        /// </summary>
        /// <param name="kind">ITR kind</param>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">目标</param>
        /// <returns>true 表示应该命中</returns>
        public bool ShouldHitTarget(int kind, LF2LivingObject attacker, LF2LivingObject target)
        {
            if (attacker == null || target == null) return false;
            
            // Kind 800-806: 只对同队伍生效 (治疗队友)
            if (IsHealTeammateKind(kind))
            {
                return attacker.Team == target.Team && attacker.Team != 0;
            }
            
            // Kind 810-816: 只对敌方生效 (治疗敌人/特殊效果)
            if (IsHealEnemyKind(kind))
            {
                return attacker.Team != target.Team || attacker.Team == 0;
            }
            
            // Kind 880-886: 只对 owner 指向的对象生效
            if (IsOwnerOnlyKind(kind))
            {
                return target.OwnerId == attacker.StableId;
            }
            
            // Kind 890-896: 对 owner 同队伍的对象生效
            if (IsOwnerTeamKind(kind))
            {
                // 获取 attacker 的 owner
                int ownerTeam = attacker.Team;
                if (attacker.OwnerId >= 0)
                {
                    var owner = GetAnimatorById(attacker.OwnerId);
                    if (owner != null)
                    {
                        ownerTeam = owner.Team;
                    }
                }
                return ownerTeam == target.Team && ownerTeam != 0;
            }
            
            // 默认: 可以命中
            return true;
        }
        
        /// <summary>
        /// 根据 StableId 获取 LivingObject
        /// TODO: 使用对象注册表替代 FindObjectsByType
        /// </summary>
        private LF2LivingObject GetAnimatorById(int stableId)
        {
            // 暂时使用 FindObjectsByType，后续应改为对象注册表
            //var animators = Object.FindObjectsByType<LF2LivingObject>(FindObjectsSortMode.None);
            //foreach (var animator in animators)
            //{
            //    if (animator != null && animator.StableId == stableId)
            //        return animator;
            //}
            return null;
        }
        
        #endregion
        
        #region 高级控制 ITR 处理 (100099-100103)
        
        /// <summary>
        /// 处理 Kind 100099: 随机移动
        /// </summary>
        public void ProcessRandomMove(LF2LivingObject actor, InteractionArea itr)
        {
            if (actor == null || actor.PS == null || itr == null) return;
            
            // x - 随机 X 移动范围
            if (itr.x != 0)
            {
                int randX = Random.Range(-itr.x, itr.x + 1);
                actor.PS.x += randX;
            }
            
            // y - 随机 Y 移动范围
            if (itr.y != 0)
            {
                int randY = Random.Range(-itr.y, itr.y + 1);
                actor.PS.y += randY;
            }
            
            // zwidth - 随机 Z 移动范围
            if (itr.zwidth != 0)
            {
                int randZ = Random.Range(-itr.zwidth, itr.zwidth + 1);
                actor.PS.z += randZ;
            }
        }
        
        /// <summary>
        /// 处理 Kind 100100: 输入控制跳帧
        /// 根据玩家输入跳转到不同帧
        /// </summary>
        public int? ProcessInputControl(LF2LivingObject actor, InteractionArea itr)
        {
            //if (actor == null || actor._Character == null) return null;
            
            //var input = actor._Character._CharacterInput;
            //if (input == null) return null;
            
            //// z=1 时，左右键控制朝向
            //if (itr.zwidth == 1)
            //{
            //    if (input.IsLeft)
            //        actor.PS.dir = "left";
            //    else if (input.IsRight)
            //        actor.PS.dir = "right";
            //}
            
            //// 按键优先级: A > J > D > Left > Right > Up > Down
            //// injury - A键帧
            //if (itr.injury != 0 && input.IsAtt)
            //    return itr.injury;
            
            //// fall - J键帧
            //if (itr.fall != 0 && input.IsJump)
            //    return itr.fall;
            
            //// bdefend - D键帧
            //if (itr.bdefend != 0 && input.IsDef)
            //    return itr.bdefend;
            
            //// x - Left键帧
            //if (itr.x != 0 && input.IsLeft)
            //    return itr.x;
            
            //// y - Right键帧
            //if (itr.y != 0 && input.IsRight)
            //    return itr.y;
            
            //// w - Up键帧
            //if (itr.w != 0 && input.IsTop)
            //    return itr.w;
            
            //// h - Down键帧
            //if (itr.h != 0 && input.IsDown)
            //    return itr.h;
            
            return null;
        }
        
        #endregion
    }
}
