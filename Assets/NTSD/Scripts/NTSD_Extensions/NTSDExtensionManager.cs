using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD 扩展系统管理器
    /// 整合所有 NTSD DLL 扩展功能
    /// </summary>
    public class NTSDExtensionManager : MonoBehaviour
    {
        public static NTSDExtensionManager Instance { get; private set; }
        
        [Header("子系统")]
        [SerializeField] private NTSDTimeStopSystem _timeStopSystem;
        
        public NTSDTimeStopSystem TimeStopSystem => _timeStopSystem;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // 确保时停系统存在
            if (_timeStopSystem == null)
            {
                _timeStopSystem = GetComponent<NTSDTimeStopSystem>();
                if (_timeStopSystem == null)
                    _timeStopSystem = gameObject.AddComponent<NTSDTimeStopSystem>();
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        /// <summary>
        /// 每帧更新所有扩展系统
        /// </summary>
        public void Tick()
        {
            _timeStopSystem?.Tick();
        }
        
        #region ITR Kind 处理
        
        /// <summary>
        /// 判断 ITR 是否应该命中目标 (考虑 NTSD 扩展 Kind)
        /// </summary>
        public bool ShouldItrHitTarget(int kind, ILF2LivingObject attacker, ILF2LivingObject target)
        {
            // 先检查 NTSD 扩展 Kind
            if (NTSDItrKindHandler.IsNTSDAttackKind(kind))
            {
                return NTSDItrKindHandler.ShouldHitTarget(kind, attacker, target);
            }
            
            // 默认: 可以命中
            return true;
        }
        
        /// <summary>
        /// 处理 NTSD 高级控制 ITR (100099-100103)
        /// </summary>
        public void ProcessControlItr(ILF2LivingObject actor, InteractionArea itr)
        {
            if (!NTSDItrKindHandler.IsNTSDControlKind(itr.kind)) return;
            
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
        
        #endregion
        
        #region Effect 处理
        
        /// <summary>
        /// 处理 ITR Effect (包括 NTSD 扩展)
        /// </summary>
        public void ProcessEffect(InteractionArea itr, int damage, ILF2LivingObject attacker, ILF2LivingObject target)
        {
            if (NTSDEffectHandler.IsNTSDEffect(itr.effect))
            {
                var result = NTSDEffectHandler.ProcessEffect(itr.effect, damage, attacker, target);
                NTSDEffectHandler.ApplyEffectResult(result, attacker, target);
            }
        }
        
        #endregion
        
        #region State 处理
        
        /// <summary>
        /// 处理扩展 State
        /// </summary>
        /// <returns>(是否处理, 目标帧)</returns>
        public (bool handled, int? targetFrame) ProcessExtendedState(ILF2LivingObject character, int state)
        {
            return NTSDStateHandler.ProcessExtendedState(character, state);
        }
        
        /// <summary>
        /// 检查 State 9xxx 受击跳帧
        /// </summary>
        public int? CheckHitGotoState(ILF2LivingObject character)
        {
            if (character?.Frame.D == null) return null;
            
            var parsed = NTSDStateHandler.ParseState(character.Frame.D.state);
            if (parsed.type == NTSDStateHandler.ExtendedStateType.HitGoto)
            {
                return parsed.param1;
            }
            return null;
        }
        
        #endregion
        
        #region 伤害处理
        
        /// <summary>
        /// 完整的伤害处理流程
        /// </summary>
        public NTSDDamageCalculator.DamageResult ProcessDamage(
            InteractionArea itr,
            ILF2LivingObject attacker,
            ILF2LivingObject target)
        {
            return NTSDDamageCalculator.ProcessDamage(itr, attacker, target);
        }
        
        #endregion
        
        #region 时停处理
        
        /// <summary>
        /// 检查对象是否应该被时停冻结
        /// </summary>
        public bool ShouldFreeze(ILF2LivingObject obj)
        {
            return _timeStopSystem != null && _timeStopSystem.ShouldFreeze(obj);
        }
        
        /// <summary>
        /// 激活时停
        /// </summary>
        public void ActivateTimeStop(int duration, int exemptTeam = 0, int stopType = 0)
        {
            _timeStopSystem?.Activate(duration, exemptTeam, stopType);
        }
        
        #endregion
    }
}
