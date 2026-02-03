using UnityEngine;
using System;

namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD 装备槽
    /// 基于 DLL rpg.inc 的装备系统
    /// </summary>
    [Serializable]
    public class NTSDEquipmentSlot
    {
        [SerializeField] private int _itemId;
        [SerializeField] private int[] _stats = new int[NTSDConstants.STAT_COUNT];
        [SerializeField] private int[] _resists = new int[NTSDConstants.RESIST_COUNT];
        
        public int ItemId { get => _itemId; set => _itemId = value; }
        
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
        
        public void Clear()
        {
            _itemId = 0;
            Array.Clear(_stats, 0, _stats.Length);
            Array.Clear(_resists, 0, _resists.Length);
        }
    }
    
    /// <summary>
    /// NTSD Buff槽
    /// 基于 DLL rpg.inc 的Buff系统
    /// </summary>
    [Serializable]
    public class NTSDBuffSlot
    {
        [SerializeField] private int _buffId;
        [SerializeField] private int _duration;
        [SerializeField] private int[] _stats = new int[NTSDConstants.STAT_COUNT];
        [SerializeField] private int[] _resists = new int[NTSDConstants.RESIST_COUNT];
        
        public int BuffId { get => _buffId; set => _buffId = value; }
        public int Duration { get => _duration; set => _duration = value; }
        public bool IsActive => _buffId > 0 && _duration > 0;
        
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
        
        public void Tick()
        {
            if (_duration > 0)
                _duration--;
        }
        
        public void Clear()
        {
            _buffId = 0;
            _duration = 0;
            Array.Clear(_stats, 0, _stats.Length);
            Array.Clear(_resists, 0, _resists.Length);
        }
    }
}
