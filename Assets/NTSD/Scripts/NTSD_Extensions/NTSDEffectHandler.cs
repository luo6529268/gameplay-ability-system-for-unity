using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD ITR Effect 扩展处理器
    /// 处理 effect 1xxx-9xxx 的特殊效果
    /// </summary>
    public static class NTSDEffectHandler
    {
        #region Effect 分类判断
        
        /// <summary>
        /// 判断是否为 NTSD 扩展 Effect
        /// </summary>
        public static bool IsNTSDEffect(int effect)
        {
            return effect >= 1000 && effect <= 9999;
        }
        
        /// <summary>
        /// 判断是否为 HP 吸取 Effect (1xxx)
        /// </summary>
        public static bool IsHPDrainEffect(int effect)
        {
            return effect >= NTSDConstants.EFFECT_HP_DRAIN_START && 
                   effect <= NTSDConstants.EFFECT_HP_DRAIN_END;
        }
        
        /// <summary>
        /// 判断是否为 HP 治疗 Effect (2xxx)
        /// </summary>
        public static bool IsHPHealEffect(int effect)
        {
            return effect >= NTSDConstants.EFFECT_HP_HEAL_START && 
                   effect <= NTSDConstants.EFFECT_HP_HEAL_END;
        }
        
        /// <summary>
        /// 判断是否为 MP 吸取 Effect (3xxx)
        /// </summary>
        public static bool IsMPDrainEffect(int effect)
        {
            return effect >= NTSDConstants.EFFECT_MP_DRAIN_START && 
                   effect <= NTSDConstants.EFFECT_MP_DRAIN_END;
        }
        
        /// <summary>
        /// 判断是否为 MP 给予 Effect (4xxx)
        /// </summary>
        public static bool IsMPGiveEffect(int effect)
        {
            return effect >= NTSDConstants.EFFECT_MP_GIVE_START && 
                   effect <= NTSDConstants.EFFECT_MP_GIVE_END;
        }
        
        /// <summary>
        /// 判断是否为强制跳帧 Effect (5xxx)
        /// </summary>
        public static bool IsForceFrameEffect(int effect)
        {
            return effect >= NTSDConstants.EFFECT_FORCE_FRAME_START && 
                   effect <= NTSDConstants.EFFECT_FORCE_FRAME_END;
        }
        
        #endregion
        
        #region Effect 参数提取
        
        /// <summary>
        /// 从 Effect 值提取参数 (xxx 部分)
        /// </summary>
        public static int GetEffectParam(int effect)
        {
            return effect % 1000;
        }
        
        #endregion
        
        #region Effect 处理结果
        
        public struct EffectResult
        {
            public int attackerHPChange;
            public int attackerMPChange;
            public int targetHPChange;
            public int targetMPChange;
            public int? targetForceFrame;
            public bool handled;
        }
        
        #endregion
        
        #region Effect 处理
        
        /// <summary>
        /// 处理 NTSD 扩展 Effect
        /// </summary>
        /// <param name="effect">Effect 值</param>
        /// <param name="damage">造成的伤害</param>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">目标</param>
        /// <returns>Effect 处理结果</returns>
        public static EffectResult ProcessEffect(int effect, int damage, ILF2LivingObject attacker, ILF2LivingObject target)
        {
            var result = new EffectResult { handled = false };
            
            if (!IsNTSDEffect(effect)) return result;
            
            int param = GetEffectParam(effect);
            
            // Effect 1xxx: HP 吸取
            if (IsHPDrainEffect(effect))
            {
                result.handled = true;
                int drainAmount = Mathf.RoundToInt(damage * param / 100f);
                result.attackerHPChange = drainAmount;
                return result;
            }
            
            // Effect 2xxx: HP 治疗
            if (IsHPHealEffect(effect))
            {
                result.handled = true;
                result.targetHPChange = param;
                return result;
            }
            
            // Effect 3xxx: MP 吸取
            if (IsMPDrainEffect(effect))
            {
                result.handled = true;
                result.targetMPChange = -param;
                result.attackerMPChange = param;
                return result;
            }
            
            // Effect 4xxx: MP 给予
            if (IsMPGiveEffect(effect))
            {
                result.handled = true;
                result.targetMPChange = param;
                return result;
            }
            
            // Effect 5xxx: 强制跳帧
            if (IsForceFrameEffect(effect))
            {
                result.handled = true;
                result.targetForceFrame = param;
                return result;
            }
            
            return result;
        }
        
        /// <summary>
        /// 应用 Effect 结果到角色
        /// </summary>
        public static void ApplyEffectResult(EffectResult result, ILF2LivingObject attacker, ILF2LivingObject target)
        {
            if (!result.handled) return;
            
            // 应用攻击者变化
            if (attacker != null && attacker.CharacterStats != null)
            {
                if (result.attackerHPChange != 0)
                {
                    attacker.CharacterStats.CurrentHP = Mathf.Clamp(
                        attacker.CharacterStats.CurrentHP + result.attackerHPChange,
                        0, attacker.CharacterStats.MaxHP);
                }
                if (result.attackerMPChange != 0)
                {
                    attacker.CharacterStats.CurrentMP = Mathf.Clamp(
                        attacker.CharacterStats.CurrentMP + result.attackerMPChange,
                        0, attacker.CharacterStats.MaxMP);
                }
            }
            
            // 应用目标变化
            if (target != null && target.CharacterStats != null)
            {
                if (result.targetHPChange != 0)
                {
                    target.CharacterStats.CurrentHP = Mathf.Clamp(
                        target.CharacterStats.CurrentHP + result.targetHPChange,
                        0, target.CharacterStats.MaxHP);
                }
                if (result.targetMPChange != 0)
                {
                    target.CharacterStats.CurrentMP = Mathf.Clamp(
                        target.CharacterStats.CurrentMP + result.targetMPChange,
                        0, target.CharacterStats.MaxMP);
                }
                
                // 强制跳帧
                if (result.targetForceFrame.HasValue)
                {
                    target.TransitionToFrame(result.targetForceFrame.Value, 10);
                }
            }
        }
        
        #endregion
    }
}
