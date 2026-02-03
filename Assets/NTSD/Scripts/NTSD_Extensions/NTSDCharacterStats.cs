using UnityEngine;
using System;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD 角色属性系统
    /// 基于 DLL rpg.inc, loading.inc, regen.inc 逻辑还原
    /// </summary>
    [Serializable]
    public class NTSDCharacterStats
    {
        #region 基础属性
        
        [Header("生命值")]
        [SerializeField] private int _currentHP;
        [SerializeField] private int _maxHP = NTSDConstants.DEFAULT_MAX_HP;
        [SerializeField] private int _darkHP;  // 暗HP (受伤后缓慢恢复的目标值)
        
        [Header("魔法值")]
        [SerializeField] private int _currentMP;
        [SerializeField] private int _maxMP = NTSDConstants.DEFAULT_MAX_MP;
        [SerializeField] private int _mpCap;   // MP上限 (可能低于 MaxMP)
        
        public int CurrentHP
        {
            get => _currentHP;
            set => _currentHP = Mathf.Clamp(value, 0, _maxHP);
        }
        
        public int MaxHP
        {
            get => _maxHP;
            set => _maxHP = Mathf.Max(1, value);
        }
        
        public int DarkHP
        {
            get => _darkHP;
            set => _darkHP = Mathf.Clamp(value, 0, _maxHP);
        }
        
        public int CurrentMP
        {
            get => _currentMP;
            set => _currentMP = Mathf.Clamp(value, 0, EffectiveMaxMP);
        }
        
        public int MaxMP
        {
            get => _maxMP;
            set => _maxMP = Mathf.Max(0, value);
        }
        
        public int MPCap
        {
            get => _mpCap;
            set => _mpCap = Mathf.Max(0, value);
        }
        
        /// <summary>实际最大MP (考虑 MPCap)</summary>
        public int EffectiveMaxMP => _mpCap > 0 ? Mathf.Min(_maxMP, _mpCap) : _maxMP;
        
        #endregion
        
        #region RPG 属性 (8个)
        
        [Header("属性值 (Stats)")]
        [SerializeField] private int[] _stats = new int[NTSDConstants.STAT_COUNT];
        
        [Header("抗性值 (Resists)")]
        [SerializeField] private int[] _resists = new int[NTSDConstants.RESIST_COUNT];
        
        public int[] Stats => _stats;
        public int[] Resists => _resists;
        
        public int GetStat(int index) => index >= 0 && index < _stats.Length ? _stats[index] : 0;
        public int GetResist(int index) => index >= 0 && index < _resists.Length ? _resists[index] : 0;
        
        public void SetStat(int index, int value)
        {
            if (index >= 0 && index < _stats.Length)
                _stats[index] = value;
        }
        
        public void SetResist(int index, int value)
        {
            if (index >= 0 && index < _resists.Length)
                _resists[index] = value;
        }
        
        #endregion
        
        #region 暴击系统
        
        [Header("暴击")]
        [SerializeField] private float _critChance = NTSDConstants.DEFAULT_CRIT_CHANCE;
        [SerializeField] private float _critMultiplier = NTSDConstants.DEFAULT_CRIT_MULTIPLIER;
        
        public float CritChance
        {
            get => _critChance;
            set => _critChance = Mathf.Clamp01(value);
        }
        
        public float CritMultiplier
        {
            get => _critMultiplier;
            set => _critMultiplier = Mathf.Max(1f, value);
        }
        
        #endregion
        
        #region 回复系统
        
        [Header("回复")]
        [SerializeField] private int _hpRegenRate;
        [SerializeField] private int _hpRegenCooldown = 12;
        [SerializeField] private int _mpRegenRate;
        [SerializeField] private int _mpRegenCooldown = 8;
        
        private int _hpRegenTimer;
        private int _mpRegenTimer;
        private int _hurtCooldownTimer;
        
        public int HPRegenRate { get => _hpRegenRate; set => _hpRegenRate = value; }
        public int HPRegenCooldown { get => _hpRegenCooldown; set => _hpRegenCooldown = Mathf.Max(1, value); }
        public int MPRegenRate { get => _mpRegenRate; set => _mpRegenRate = value; }
        public int MPRegenCooldown { get => _mpRegenCooldown; set => _mpRegenCooldown = Mathf.Max(1, value); }
        
        #endregion
        
        #region 装备槽
        
        [Header("装备")]
        [SerializeField] private NTSDEquipmentSlot[] _equipment = new NTSDEquipmentSlot[NTSDConstants.EQUIPMENT_SLOT_COUNT];
        
        public NTSDEquipmentSlot[] Equipment => _equipment;
        
        #endregion
        
        #region Buff槽
        
        [Header("Buff")]
        [SerializeField] private NTSDBuffSlot[] _buffs = new NTSDBuffSlot[NTSDConstants.BUFF_SLOT_COUNT];
        
        public NTSDBuffSlot[] Buffs => _buffs;
        
        #endregion
        
        #region 初始化
        
        public NTSDCharacterStats()
        {
            _stats = new int[NTSDConstants.STAT_COUNT];
            _resists = new int[NTSDConstants.RESIST_COUNT];
            _equipment = new NTSDEquipmentSlot[NTSDConstants.EQUIPMENT_SLOT_COUNT];
            _buffs = new NTSDBuffSlot[NTSDConstants.BUFF_SLOT_COUNT];
            
            for (int i = 0; i < _equipment.Length; i++)
                _equipment[i] = new NTSDEquipmentSlot();
            for (int i = 0; i < _buffs.Length; i++)
                _buffs[i] = new NTSDBuffSlot();
        }
        
        public void Initialize(int maxHP, int maxMP)
        {
            _maxHP = maxHP;
            _currentHP = maxHP;
            _darkHP = maxHP;
            _maxMP = maxMP;
            _currentMP = maxMP;
        }
        
        #endregion
        
        #region 属性重算 (基于 rpg.inc)
        
        /// <summary>
        /// 重新计算所有属性 (从装备和Buff累加)
        /// 对应 DLL rpg.inc 的 all_stats_calculation
        /// </summary>
        public void RecalculateStats(int[] baseStats = null, int[] baseResists = null)
        {
            // 1. 清空当前属性
            Array.Clear(_stats, 0, _stats.Length);
            Array.Clear(_resists, 0, _resists.Length);
            
            // 2. 累加装备属性
            for (int i = 0; i < _equipment.Length; i++)
            {
                var equip = _equipment[i];
                if (equip == null || equip.ItemId <= 0) continue;
                
                for (int s = 0; s < NTSDConstants.STAT_COUNT; s++)
                {
                    _stats[s] += equip.GetStat(s);
                    _resists[s] += equip.GetResist(s);
                }
            }
            
            // 3. 累加Buff属性
            for (int i = 0; i < _buffs.Length; i++)
            {
                var buff = _buffs[i];
                if (buff == null || !buff.IsActive) continue;
                
                for (int s = 0; s < NTSDConstants.STAT_COUNT; s++)
                {
                    _stats[s] += buff.GetStat(s);
                    _resists[s] += buff.GetResist(s);
                }
            }
            
            // 4. 加上角色基础属性
            if (baseStats != null)
            {
                for (int s = 0; s < Mathf.Min(baseStats.Length, _stats.Length); s++)
                    _stats[s] += baseStats[s];
            }
            if (baseResists != null)
            {
                for (int s = 0; s < Mathf.Min(baseResists.Length, _resists.Length); s++)
                    _resists[s] += baseResists[s];
            }
        }
        
        #endregion
        
        #region 回复处理 (基于 regen.inc)
        
        /// <summary>
        /// 每帧更新回复逻辑
        /// 对应 DLL regen.inc
        /// </summary>
        public void TickRegeneration()
        {
            // HP 回复
            if (_hpRegenRate > 0 && _currentHP < _maxHP)
            {
                _hpRegenTimer++;
                if (_hpRegenTimer >= _hpRegenCooldown)
                {
                    _hpRegenTimer = 0;
                    _currentHP = Mathf.Min(_currentHP + _hpRegenRate, _maxHP);
                }
            }
            
            // MP 回复
            if (_mpRegenRate > 0 && _currentMP < EffectiveMaxMP)
            {
                _mpRegenTimer++;
                if (_mpRegenTimer >= _mpRegenCooldown)
                {
                    _mpRegenTimer = 0;
                    _currentMP = Mathf.Min(_currentMP + _mpRegenRate, EffectiveMaxMP);
                }
            }
            
            // 暗HP恢复 (受伤后缓慢恢复)
            if (_darkHP > _currentHP)
            {
                _hurtCooldownTimer++;
                if (_hurtCooldownTimer >= 30) // 30帧后开始恢复
                {
                    _currentHP++;
                }
            }
            else
            {
                _hurtCooldownTimer = 0;
            }
        }
        
        /// <summary>
        /// 受到伤害时调用
        /// </summary>
        public void OnDamaged(int damage)
        {
            _currentHP = Mathf.Max(0, _currentHP - damage);
            _hurtCooldownTimer = 0;
        }
        
        /// <summary>
        /// 更新暗HP (受伤时设置为当前HP)
        /// </summary>
        public void UpdateDarkHP()
        {
            if (_currentHP < _darkHP)
                _darkHP = _currentHP;
        }
        
        #endregion
        
        #region Buff 管理
        
        /// <summary>
        /// 更新所有Buff的持续时间
        /// </summary>
        public void TickBuffs()
        {
            for (int i = 0; i < _buffs.Length; i++)
            {
                if (_buffs[i] != null && _buffs[i].IsActive)
                {
                    _buffs[i].Tick();
                }
            }
        }
        
        /// <summary>
        /// 添加Buff
        /// </summary>
        public bool AddBuff(NTSDBuffSlot buff)
        {
            for (int i = 0; i < _buffs.Length; i++)
            {
                if (_buffs[i] == null || !_buffs[i].IsActive)
                {
                    _buffs[i] = buff;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 移除指定ID的Buff
        /// </summary>
        public void RemoveBuff(int buffId)
        {
            for (int i = 0; i < _buffs.Length; i++)
            {
                if (_buffs[i] != null && _buffs[i].BuffId == buffId)
                {
                    _buffs[i].Clear();
                }
            }
        }
        
        #endregion
    }
}
