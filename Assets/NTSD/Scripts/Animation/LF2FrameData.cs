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
        public ObjectPoint? opoint = null;

        [Header("声音")]
        public string sound = "";

        [Header("血点")]
        public BloodPoint bpoint = null;

        [Header("抓取点")]
        public CatchPoint cpoint = null;

        public Dictionary<string, string> rawProperties = new Dictionary<string, string>();
        public List<ObjectPoint> opoints = new List<ObjectPoint>();

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
        public int w = 0;
        public int h = 0;
        public int weaponact = 0;
        public int attacking = 0;
        public int cover = 0;
        public int dvx = 0;
        public int dvy = 0;
        public int dvz = 0;
        // 反汇编 0x0042CA9F：wpoint[9..16] 对应 itr 的 injury/fall/vaction/arest/vrest/effect/kill/bdefend
        public int injury = 0;
        public int fall = 0;
        public int vaction = 0;
        public int arest = 0;
        public int vrest = 0;
        public int effect = 0;
        public int kill = 0;
        public int bdefend = 0;
        public Dictionary<string, string> rawProperties = new Dictionary<string, string>();
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
        public Dictionary<string, string> rawProperties = new Dictionary<string, string>();
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
        public int vaction = 0;
        //攻击者休息帧数
        //攻击者打中人之后，自己要等几帧才能再次打人。 这是 LF2 的攻击频率限制机制
        public int arest = 0;
        //受害者休息帧数
        public int vrest = 0;
        //击中效果
        public int effect = 0;
        // 死字段：kill: 从未出现在任何 dat 文件中，始终为 0，不影响任何逻辑
        public int kill = 0;

        // LF2/FLF: 防御破坏（可选字段；用于 defend/broken_defend 判定）
        public int bdefend = 0;
        public Dictionary<string, string> rawProperties = new Dictionary<string, string>();

        // FLF: 抓取成功后抓取者切换的帧 [正面帧, 背面帧]（仅 kind=1/3 有效）
        // 对应 FLF character.js:2235-2237: trans.frame(ITR.catchingact[0/1], 10)
        public int[] catchingact = null;

        // FLF: 被抓者切换的帧 [正面帧, 背面帧]（仅 kind=1/3 有效）
        public int[] caughtact = null;
        // 反汇编 0x41A0C9：itr.attacking 目标过滤（4=仅角色, 20=角色且非抓取态, 21=非抓取态, 30=非特定帧）
        public int attacking = 0;
        // 反汇编 0x0042EC85：itr.kind=8 爆炸传送时的 heal_timer 偏移量
        public int throwvz = 0;

        public InteractionArea ShallowCopy() => (InteractionArea)MemberwiseClone();
    }

    /// <summary>
    /// 对象点（生成投射物等）
    /// </summary>
    public struct ObjectPoint
    {
        public int kind;
        public int action;
        public int objectId;
        public int x;
        public int y;
        public int dvx;
        public int dvy;
        public int dvz;
        public int oid;
        public int facing;
    }

    /// <summary>
    /// 血点（出血效果位置）
    /// </summary>
    [System.Serializable]
    public class BloodPoint
    {
        public int x = 0;
        public int y = 0;
        public Dictionary<string, string> rawProperties = new Dictionary<string, string>();
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
        // NTSD 2.4 反汇编确认的额外字段
        public int injury = 0;      // 抓取伤害
        public int cover = 0;       // Z轴层级
        public int aaction = 0;     // 攻击动作帧
        public int jaction = 0;     // 跳跃动作帧
        public int taction = 0;     // 投掷动作帧
        public int daction = 0;     // 防御动作帧
        public int throwvx = 0;     // 投掷X速度
        public int throwvy = 0;     // 投掷Y速度
        public int dircontrol = 0;  // 方向控制
        public Dictionary<string, string> rawProperties = new Dictionary<string, string>();
    }

    [System.Serializable]
    public class WeaponStrengthEntry
    {
        public int index = 0;
        public int dvx = 0;
        public int dvy = 0;
        public int fall = 0;
        public int vrest = 0;
        public int arest = 0;
        public int bdefend = 0;
        public int injury = 0;
        public int effect = 0;
    }
}
