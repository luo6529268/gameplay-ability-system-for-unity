using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD State 扩展处理器
    /// 处理 State 2xxxyyy, 3xxxyyy, 8xxx, 9xxx 的特殊逻辑
    /// </summary>
    public static class NTSDStateHandler
    {
        #region State 类型枚举
        
        public enum ExtendedStateType
        {
            None = 0,
            MPCost = 2,      // 2xxxyyy: MP消耗跳帧
            TimeStop = 3,    // 3xxxyyy: 时停触发
            DirectFrame = 8, // 8xxx: 直接跳帧
            HitGoto = 9      // 9xxx: 受击跳帧
        }
        
        #endregion
        
        #region State 解析结果
        
        public struct ParsedState
        {
            public ExtendedStateType type;
            public int param1;  // xxx 部分
            public int param2;  // yyy 部分 (仅 2xxxyyy, 3xxxyyy)
            public bool isExtended;
        }
        
        #endregion
        
        #region State 分类判断
        
        /// <summary>
        /// 判断是否为 NTSD 扩展 State
        /// </summary>
        public static bool IsNTSDExtendedState(int state)
        {
            // 2xxxyyy: MP消耗
            if (state >= NTSDConstants.STATE_MP_COST_MIN && state <= NTSDConstants.STATE_MP_COST_MAX)
                return true;
            
            // 3xxxyyy: 时停
            if (state >= NTSDConstants.STATE_TIMESTOP_MIN && state <= NTSDConstants.STATE_TIMESTOP_MAX)
                return true;
            
            // 8xxx: 直接跳帧
            if (state >= NTSDConstants.STATE_DIRECT_FRAME_MIN && state <= NTSDConstants.STATE_DIRECT_FRAME_MAX)
                return true;
            
            // 9xxx: 受击跳帧
            if (state >= NTSDConstants.STATE_HIT_GOTO_MIN && state <= NTSDConstants.STATE_HIT_GOTO_MAX)
                return true;
            
            return false;
        }
        
        /// <summary>
        /// 解析扩展 State
        /// </summary>
        public static ParsedState ParseState(int state)
        {
            var result = new ParsedState { type = ExtendedStateType.None, isExtended = false };
            
            // 2xxxyyy: MP消耗跳帧
            if (state >= NTSDConstants.STATE_MP_COST_MIN && state <= NTSDConstants.STATE_MP_COST_MAX)
            {
                result.type = ExtendedStateType.MPCost;
                result.param1 = (state / 1000) % 1000;  // xxx: MP消耗量
                result.param2 = state % 1000;           // yyy: 目标帧
                result.isExtended = true;
                return result;
            }
            
            // 3xxxyyy: 时停触发
            if (state >= NTSDConstants.STATE_TIMESTOP_MIN && state <= NTSDConstants.STATE_TIMESTOP_MAX)
            {
                result.type = ExtendedStateType.TimeStop;
                result.param1 = (state / 1000) % 1000;  // xxx: 时停帧数
                result.param2 = state % 1000;           // yyy: 目标帧
                result.isExtended = true;
                return result;
            }
            
            // 8xxx: 直接跳帧 (无帧修复)
            if (state >= NTSDConstants.STATE_DIRECT_FRAME_MIN && state <= NTSDConstants.STATE_DIRECT_FRAME_MAX)
            {
                result.type = ExtendedStateType.DirectFrame;
                result.param1 = state % 1000;  // xxx: 目标帧
                result.isExtended = true;
                return result;
            }
            
            // 9xxx: 受击跳帧
            if (state >= NTSDConstants.STATE_HIT_GOTO_MIN && state <= NTSDConstants.STATE_HIT_GOTO_MAX)
            {
                result.type = ExtendedStateType.HitGoto;
                result.param1 = state % 1000;  // xxx: 受击时跳转的帧
                result.isExtended = true;
                return result;
            }
            
            return result;
        }
        
        #endregion
        
        #region State 处理
        
        /// <summary>
        /// 处理 State 2xxxyyy: MP消耗跳帧
        /// </summary>
        /// <returns>成功跳转的目标帧，失败返回 null</returns>
        public static int? ProcessMPCostState(LF2LivingObject character, int mpCost, int targetFrame)
        {
            if (character == null || character.CharacterStats == null) return null;
            
            // 检查 MP 是否足够
            if (character.CharacterStats.CurrentMP >= mpCost)
            {
                // 消耗 MP
                character.CharacterStats.CurrentMP -= mpCost;
                return targetFrame;
            }
            
            return null;
        }
        
        /// <summary>
        /// 处理 State 3xxxyyy: 时停触发
        /// </summary>
        /// <returns>跳转的目标帧</returns>
        public static int ProcessTimeStopState(LF2LivingObject character, int duration, int targetFrame)
        {
            // 激活时停系统
            if (NTSDTimeStopSystem.Instance != null)
            {
                // 默认: 时停发起者的队伍不受影响
                int exemptTeam = character != null ? character.Team : 0;
                NTSDTimeStopSystem.Instance.Activate(duration, exemptTeam);
            }
            
            return targetFrame;
        }
        
        /// <summary>
        /// 处理 State 8xxx: 直接跳帧 (无帧修复)
        /// </summary>
        public static int ProcessDirectFrameState(int targetFrame)
        {
            return targetFrame;
        }
        
        /// <summary>
        /// 处理 State 9xxx: 受击跳帧
        /// 当角色被击中时，跳转到指定帧
        /// </summary>
        public static int ProcessHitGotoState(int targetFrame)
        {
            return targetFrame;
        }
        
        /// <summary>
        /// 统一处理扩展 State
        /// </summary>
        /// <returns>处理结果: (是否处理, 目标帧)</returns>
        public static (bool handled, int? targetFrame) ProcessExtendedState(LF2LivingObject character, int state)
        {
            var parsed = ParseState(state);
            if (!parsed.isExtended) return (false, null);
            
            switch (parsed.type)
            {
                case ExtendedStateType.MPCost:
                    var mpResult = ProcessMPCostState(character, parsed.param1, parsed.param2);
                    return (true, mpResult);
                
                case ExtendedStateType.TimeStop:
                    var tsResult = ProcessTimeStopState(character, parsed.param1, parsed.param2);
                    return (true, tsResult);
                
                case ExtendedStateType.DirectFrame:
                    return (true, ProcessDirectFrameState(parsed.param1));
                
                case ExtendedStateType.HitGoto:
                    // HitGoto 只在受击时触发，这里只返回解析结果
                    return (true, null);
                
                default:
                    return (false, null);
            }
        }
        
        #endregion
    }
}
