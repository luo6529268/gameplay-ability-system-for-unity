using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD 伤害计算系统
    /// 基于 DLL bodies.inc 的伤害计算逻辑
    /// </summary>
    public static class NTSDDamageCalculator
    {
        #region 伤害类型枚举
        
        /// <summary>
        /// 伤害类型 (对应 8 种属性)
        /// </summary>
        public enum DamageType
        {
            Physical = 0,   // 物理
            Fire = 1,       // 火
            Ice = 2,        // 冰
            Lightning = 3,  // 雷
            Wind = 4,       // 风
            Earth = 5,      // 土
            Light = 6,      // 光
            Dark = 7        // 暗
        }
        
        #endregion
        
        #region 伤害计算结果
        
        public struct DamageResult
        {
            public int baseDamage;
            public int finalDamage;
            public bool isCrit;
            public float critMultiplier;
            public int attackerStatBonus;
            public int targetResistReduction;
        }
        
        #endregion
        
        #region 伤害计算
        
        /// <summary>
        /// 计算最终伤害
        /// 基于 DLL bodies.inc 的伤害公式
        /// </summary>
        /// <param name="baseInjury">基础伤害 (ITR.injury)</param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">目标</param>
        /// <returns>伤害计算结果</returns>
        public static DamageResult CalculateDamage(
            int baseInjury, 
            DamageType damageType, 
            ILF2LivingObject attacker, 
            ILF2LivingObject target)
        {
            var result = new DamageResult
            {
                baseDamage = baseInjury,
                finalDamage = baseInjury,
                isCrit = false,
                critMultiplier = 1f
            };
            
            if (baseInjury <= 0) return result;
            
            // 获取攻击者属性
            int attackerStat = 0;
            float critChance = 0f;
            float critMult = NTSDConstants.DEFAULT_CRIT_MULTIPLIER;
            
            if (attacker != null && attacker.CharacterStats != null)
            {
                attackerStat = attacker.CharacterStats.GetStat((int)damageType);
                critChance = attacker.CharacterStats.CritChance;
                critMult = attacker.CharacterStats.CritMultiplier;
            }
            
            // 获取目标抗性
            int targetResist = 0;
            if (target != null && target.CharacterStats != null)
            {
                targetResist = target.CharacterStats.GetResist((int)damageType);
            }
            
            // 1. 基础伤害 + 属性加成
            // 公式: damage = baseInjury * (1 + attackerStat / 100)
            float damage = baseInjury * (1f + attackerStat / 100f);
            result.attackerStatBonus = Mathf.RoundToInt(baseInjury * attackerStat / 100f);
            
            // 2. 抗性减免
            // 公式: damage = damage * (1 - targetResist / 100)
            // 抗性上限 90%
            float resistReduction = Mathf.Min(targetResist / 100f, 0.9f);
            result.targetResistReduction = Mathf.RoundToInt(damage * resistReduction);
            damage = damage * (1f - resistReduction);
            
            // 3. 暴击判定
            if (critChance > 0f && Random.value < critChance)
            {
                result.isCrit = true;
                result.critMultiplier = critMult;
                damage *= critMult;
            }
            
            // 4. 最终伤害 (最小为1)
            result.finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            
            return result;
        }
        
        /// <summary>
        /// 简化版伤害计算 (无属性系统)
        /// </summary>
        public static int CalculateSimpleDamage(int baseInjury, ILF2LivingObject attacker, ILF2LivingObject target)
        {
            var result = CalculateDamage(baseInjury, DamageType.Physical, attacker, target);
            return result.finalDamage;
        }
        
        #endregion
        
        #region BDY Kind 扩展解析
        
        /// <summary>
        /// 解析扩展 BDY Kind (1xxyyyzzzww 格式)
        /// </summary>
        public struct ParsedBdyKind
        {
            public bool isExtended;
            public int type;      // xx
            public int param1;    // yyy
            public int param2;    // zzz
            public int param3;    // ww
        }
        
        /// <summary>
        /// 解析 BDY Kind
        /// </summary>
        public static ParsedBdyKind ParseBdyKind(int kind)
        {
            var result = new ParsedBdyKind { isExtended = false };
            
            // 检查是否为扩展 Kind (1000-4999 或 1xxyyyzzzww)
            if (kind >= NTSDConstants.BDY_KIND_EXTENDED_MIN && kind <= NTSDConstants.BDY_KIND_EXTENDED_MAX)
            {
                result.isExtended = true;
                result.type = kind;
                return result;
            }
            
            // 解析 1xxyyyzzzww 格式
            if (kind >= 100000000)
            {
                result.isExtended = true;
                result.param3 = kind % 100;           // ww
                kind /= 100;
                result.param2 = kind % 1000;          // zzz
                kind /= 1000;
                result.param1 = kind % 1000;          // yyy
                kind /= 1000;
                result.type = kind % 100;             // xx
            }
            
            return result;
        }
        
        #endregion
        
        #region 应用伤害
        
        /// <summary>
        /// 应用伤害到目标
        /// </summary>
        public static void ApplyDamage(ILF2LivingObject target, int damage)
        {
            if (target == null || target.CharacterStats == null) return;
            
            target.CharacterStats.OnDamaged(damage);
            target.CharacterStats.UpdateDarkHP();
        }
        
        /// <summary>
        /// 完整的伤害处理流程
        /// </summary>
        public static DamageResult ProcessDamage(
            InteractionArea itr,
            ILF2LivingObject attacker,
            ILF2LivingObject target)
        {
            // 1. 计算伤害
            var result = CalculateDamage(itr.injury, DamageType.Physical, attacker, target);
            
            // 2. 处理 NTSD 扩展 Effect
            if (NTSDEffectHandler.IsNTSDEffect(itr.effect))
            {
                var effectResult = NTSDEffectHandler.ProcessEffect(itr.effect, result.finalDamage, attacker, target);
                NTSDEffectHandler.ApplyEffectResult(effectResult, attacker, target);
            }
            
            // 3. 应用伤害
            ApplyDamage(target, result.finalDamage);
            
            return result;
        }
        
        #endregion
    }
}
