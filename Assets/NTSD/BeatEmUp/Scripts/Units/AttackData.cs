using UnityEngine;
using System.Collections.Generic;

namespace BeatEmUpTemplate2D
{

    /**
     * 攻击类型枚举
     * 定义了游戏中所有可能的攻击类型
     */
    public enum ATTACKTYPE
    {
        NONE,       // 无攻击
        PUNCH,      // 拳击
        KICK,       // 踢击
        GROUNDPOUND,// 重击
        GRAB,       // 抓取
        GRABPUNCH,  // 抓取后拳击
        GRABKICK,   // 抓取后踢击
        GRABTHROW,  // 抓取后投掷
        WEAPON      // 武器攻击
    };

    /**
     * 攻击数据类
     * 用于存储单个攻击的所有相关数据
     */
    [System.Serializable]
    public class AttackData
    {
        public string name; //攻击名称（可选）
        public int damage; //造成的伤害值
        public string animationState = ""; //动画状态，与Animator组件中定义的状态对应
        public string sfx = ""; //命中时播放的音效名称
        public ATTACKTYPE attackType = ATTACKTYPE.PUNCH; //攻击类型
        public bool knockdown; //该攻击是否会造成击倒效果
        [HideInInspector] public bool foldout; //在Inspector中是否展开显示（隐藏）
        [HideInInspector] public GameObject inflictor; //造成伤害的游戏对象（隐藏）

        /**
         * 攻击数据构造函数
         * @param name 攻击名称
         * @param damage 伤害值
         * @param inflictor 造成伤害的游戏对象
         * @param attackType 攻击类型
         * @param knockdown 是否造成击倒
         * @param sfx 音效名称（可选参数，默认为空）
         */
        public AttackData(string name, int damage, GameObject inflictor, ATTACKTYPE attackType, bool knockdown, string sfx = "")
        {
            this.name = name;
            this.damage = damage;
            this.inflictor = inflictor;
            this.attackType = attackType;
            this.knockdown = knockdown;
            this.sfx = sfx;
        }
    }

    /**
     * 连招类
     * 用于存储一系列攻击数据，形成连招
     */
    [System.Serializable]
    public class Combo
    {
        public string comboName = "[New Combo]"; //连招名称
        public List<AttackData> attackSequence = new List<AttackData>(); //攻击序列列表
        [HideInInspector] public bool foldout; //在Inspector中是否展开显示（隐藏）
    }

}