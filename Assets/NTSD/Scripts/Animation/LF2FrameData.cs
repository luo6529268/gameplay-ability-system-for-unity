using UnityEngine;
using System.Collections.Generic;


namespace NTSD.Animation
{

    /// <summary>
    /// LF2 DAT文件帧数据
    /// </summary>
    [System.Serializable]
    public class LF2FrameData
    {
        public int frameId;
        public string frameName = "";

        [Header("基本参数")]
        public int pic = 0;
        public int state = 0;
        public int wait = 1;
        public int next = 0;
        public int dvx = 0;
        public int dvy = 0;
        public int dvz = 0;
        public int centerx = 0;
        public int centery = 0;
        public int mp = 0;

        [Header("按键响应")]
        public int hit_a = 0;
        public int hit_d = 0;
        public int hit_j = 0;
        public int hit_Fj = 0;
        public int hit_Fa = 0;
        public int hit_Da = 0;
        public int hit_Ua = 0;
        public int hit_ja = 0;
        public int hit_Dj = 0;
        public int hit_Uj = 0;

        [Header("武器点")]
        public List<WeaponPoint> wpoints = new List<WeaponPoint>();

        [Header("碰撞盒")]
        public List<BodyBox> bodies = new List<BodyBox>();

        [Header("交互区域")]
        public List<InteractionArea> itrs = new List<InteractionArea>();

        [Header("对象点")]
        public ObjectPoint opoint = null;

        [Header("声音")]
        public string sound = "";

        [Header("血点")]
        public BloodPoint bpoint = null;

        [Header("抓取点")]
        public CatchPoint cpoint = null;

        #region 公开接口

        public class HitValues
        {
            private LF2FrameData _frameData;

            public HitValues(LF2FrameData frameData)
            {
                _frameData = frameData;
            }

            public int this[string key]
            {
                get => _frameData[key];
                set => _frameData[key] = value;
            }
        }

        public HitValues Hit => new HitValues(this);

        public int PictureIndex => pic;

        public int State => state;

        public int Wait => wait;

        public int NextFrameId => next;

        #region 基本参数
        private Dictionary<string, int> _hitValues = new Dictionary<string, int>
    {
        { "a", 0 },
        { "d", 0 },
        { "j", 0 },
        { "Fa", 0 },
        { "Da", 0 },
        { "Ua", 0 },
        { "Dj", 0 },
        { "Uj", 0 },
        { "Fj", 0 },
        { "ja", 0 },
    };

        private bool IsInitHitValue = false;
        private int this[string key]
        {
            get
            {
                if (!IsInitHitValue)
                    InitializeHitValues();

                return _hitValues.TryGetValue(key, out int value) ? value : 0;
            }
            set
            {
                if (_hitValues.ContainsKey(key))
                {
                    _hitValues[key] = value;
                    // 同步更新对应的字段
                    switch (key)
                    {
                        case "a": hit_a = value; break;
                        case "d": hit_d = value; break;
                        case "j": hit_j = value; break;
                        case "Fa": hit_Fj = value; break;
                        case "Da": hit_Fa = value; break;
                        case "Ua": hit_Da = value; break;
                        case "Dj": hit_Ua = value; break;
                        case "Uj": hit_ja = value; break;
                        case "Fj": hit_Dj = value; break;
                        case "ja": hit_Uj = value; break;
                    }
                }
            }
        }


        private void InitializeHitValues()
        {
            IsInitHitValue = true;
            _hitValues["a"] = hit_a;
            _hitValues["j"] = hit_j;
            _hitValues["d"] = hit_d;

            _hitValues["Fa"] = hit_Fa;
            _hitValues["Da"] = hit_Da;
            _hitValues["Ua"] = hit_Ua;
            _hitValues["Dj"] = hit_Dj;
            _hitValues["Uj"] = hit_Uj;
            _hitValues["Fj"] = hit_Fj;
            _hitValues["ja"] = hit_ja;
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// 武器点数据
    /// </summary>
    [System.Serializable]
    public class WeaponPoint
    {
        public int kind = 1;
        public int x = 0;
        public int y = 0;
        public int weaponact = 0;
        public int attacking = 0;
        public int cover = 0;
        public int dvx = 0;
        public int dvy = 0;
        public int dvz = 0;
    }

    /// <summary>
    /// 身体碰撞盒
    /// </summary>
    [System.Serializable]
    public class BodyBox
    {
        public int kind = 0;
        public int x = 0;
        public int y = 0;
        public int w = 0;
        public int h = 0;
    }

    /// <summary>
    /// 交互区域
    /// </summary>
    [System.Serializable]
    public class InteractionArea
    {
        //itr 类型（0=普通攻击, 1=抓取, 2=被抓, 8=治疗, 14=阻挡...）
        public int kind = 0;
        //碰撞框位置和尺寸（相对于 centerx/centery）
        public int x = 0;
        public int y = 0;
        public int w = 0;
        public int h = 0;
        public int zwidth = 0;

        // LF2/FLF: 击退速度（可选字段；缺失时默认 0）
        // 参考：I:\C++Test\NTSD\F.LF-master\LF\character.js:1879-1883 (ef_dvx/ef_dvy 由 ITR.dvx/ITR.dvy 推导)
        public int dvx = 0;
        public int dvy = 0;
        public int dvz = 0;

        //伤害值
        public int injury = 0;
        public int fall = 0;
        //攻击者休息帧数
        //攻击者打中人之后，自己要等几帧才能再次打人。 这是 LF2 的攻击频率限制机制
        public int arest = 0;
        //受害者休息帧数
        public int vrest = 0;
        //击中效果
        public int effect = 0;

        // LF2/FLF: 防御破坏（可选字段；用于 defend/broken_defend 判定）
        public int bdefend = 0;
    }

    /// <summary>
    /// 对象点（生成投射物等）
    /// </summary>
    [System.Serializable]
    public class ObjectPoint
    {
        public int kind = 0;
        public int action = 0;
        public int objectId = 0;
        public int x = 0;
        public int y = 0;
        public int dvx = 0;
        public int dvy = 0;
        public int dvz = 0;
        public int oid = 0;
        public int facing = 0;
    }

    /// <summary>
    /// 血点（出血效果位置）
    /// </summary>
    [System.Serializable]
    public class BloodPoint
    {
        public int x = 0;
        public int y = 0;
    }

    /// <summary>
    /// 抓取点（Catch Point）
    /// </summary>
    [System.Serializable]
    public class CatchPoint
    {
        public int kind = 0;
        public int x = 0;
        public int y = 0;
        public int fronthurtact = 0;
        public int backhurtact = 0;
        public int vaction = 0;
        public int throwvz = 0;
        public int hurtable = 0;
        public int throwinjury = 0;
        public int decrease = 0;
    }
}
