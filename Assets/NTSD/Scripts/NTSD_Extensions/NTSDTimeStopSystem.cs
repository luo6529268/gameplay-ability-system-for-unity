using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using System.Collections.Generic;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD 时停系统
    /// 基于 DLL time.inc 逻辑还原
    /// </summary>
    public class NTSDTimeStopSystem : MonoBehaviour
    {
        public static NTSDTimeStopSystem Instance { get; private set; }
        
        [Header("时停状态")]
        [SerializeField] private int _timer;
        [SerializeField] private int _exemptTeam;
        [SerializeField] private int _stopType;
        
        /// <summary>时停剩余帧数</summary>
        public int Timer => _timer;
        
        /// <summary>豁免队伍 (0=全体冻结, 1-4=指定队伍不受影响)</summary>
        public int ExemptTeam => _exemptTeam;
        
        /// <summary>时停类型 (0=全体, 2=只冻结角色)</summary>
        public int StopType => _stopType;
        
        /// <summary>是否处于时停状态</summary>
        public bool IsActive => _timer > 0;
        
        // 每个对象的时停计时器
        private Dictionary<int, int> _objectTimers = new Dictionary<int, int>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        /// <summary>
        /// 激活时停
        /// </summary>
        /// <param name="duration">持续帧数</param>
        /// <param name="exemptTeam">豁免队伍 (0=无豁免)</param>
        /// <param name="stopType">类型 (0=全体, 2=只冻结角色)</param>
        public void Activate(int duration, int exemptTeam = 0, int stopType = 0)
        {
            _timer = duration;
            _exemptTeam = exemptTeam;
            _stopType = stopType;
            _objectTimers.Clear();
        }
        
        /// <summary>
        /// 取消时停
        /// </summary>
        public void Cancel()
        {
            _timer = 0;
            _exemptTeam = 0;
            _stopType = 0;
            _objectTimers.Clear();
        }
        
        /// <summary>
        /// 每帧更新 (在游戏主循环中调用)
        /// </summary>
        public void Tick()
        {
            if (_timer > 0)
            {
                _timer--;
                if (_timer <= 0)
                {
                    Cancel();
                }
            }
        }
        
        /// <summary>
        /// 判断对象是否应该被冻结
        /// </summary>
        public bool ShouldFreeze(LF2LivingObject obj)
        {
            if (obj == null) return false;
            if (_timer <= 0) return false;
            
            // 检查队伍豁免
            if (_exemptTeam != 0 && obj.Team == _exemptTeam)
                return false;
            
            // 检查类型豁免 (type 2 = 只冻结角色)
            //if (_stopType == NTSDConstants.TIMESTOP_TYPE_CHARACTERS_ONLY && obj.ObjectType != 0)
            //    return false;
            
            return true;
        }
        
        /// <summary>
        /// 获取对象的时停计时器
        /// </summary>
        public int GetObjectTimer(int stableId)
        {
            return _objectTimers.TryGetValue(stableId, out int timer) ? timer : 0;
        }
        
        /// <summary>
        /// 设置对象的时停计时器 (新对象进入时停区域时)
        /// </summary>
        public void SetObjectTimer(int stableId, int timer)
        {
            _objectTimers[stableId] = timer;
        }
        
        /// <summary>
        /// 处理对象的时停逻辑
        /// </summary>
        /// <returns>true 表示对象被冻结，应跳过更新</returns>
        public bool ProcessObject(LF2LivingObject obj)
        {
            if (obj == null) return false;
            if (!ShouldFreeze(obj)) return false;
            
            int stableId = obj.StableId;
            
            // 新对象初始化时停计时器
            if (!_objectTimers.ContainsKey(stableId) || _objectTimers[stableId] == 0)
            {
                _objectTimers[stableId] = _timer;
            }
            
            // 减少对象计时器
            if (_objectTimers[stableId] > 0)
            {
                _objectTimers[stableId]--;
            }
            
            return true; // 对象被冻结
        }
    }
}
