using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Define
{
    public enum CharacterState
    {
        /**
         * 角色基础动作状态开始
         */
        STAND = 0,              // 站立状态
        WALK = 1,               // 行走状态
        RUN = 2,                // 跑步状态
        PUNCH = 3,              // 普通攻击状态
        JUMP = 4,               // 跳跃状态
        DASH = 5,               // 突进状态
        DEFEND = 7,             // 防御状态
        BROKEN_DEFEND = 8,      // 防御被破状态
        CATCHING = 9,           // 抓人状态
        PICKED_CAUGHT = 10,     // 被抓状态
        INJURED = 11,           // 受伤状态
        FALL = 12,              // 倒地状态
        ICE = 13,               // 冰冻效果状态
        LYING = 14,             // 躺在地上状态
        FROZEN = 15,            // 被冰封状态
        TIRED = 16,             // 晕眩状态
        DRINK = 17,             // 喝道具状态
        FIRE = 18,              // 燃烧状态
        BURN_RUN = 19,          // 烈火焚身状态
        DASH_SWORD = 301,       // 鬼哭斩状态
        CLOSED_BAD_GUY = 400,   // 传送到敌人身边
        CLOSED_TEAMMATE = 401,  // 传送到队友身边
        CURE_SELF = 1700,       // 自我治疗状态
        /**
         * 角色基础动作状态结束
         */

        /**
         * 投掷物状态开始
         */
        DISAPPEAR_WHEN_HIT = 15,    // 被击中后消失
        HIT_TEAMMATE = 18,          // 击中队友
        BALL_FLYING = 3000,         // 投掷物飞行中
        BALL_HITTING = 3001,        // 投掷物击中目标
        BALL_CANCELED = 3002,       // 投掷物被取消
        BALL_REBOUNDING = 3003,     // 投掷物反弹
        BALL_DISAPPEAR = 3004,      // 投掷物消失
        BALL_WIND_FLYING = 3005,    // 风系投掷物飞行
        BALL_HIT_HEART = 3006,      // 击中心脏
        /**
         * 投掷物状态结束
         */

        /**
         * 武器状态开始
         */
        WEAPON_IN_THE_SKY = 1000,   // 武器在空中
        WEAPON_ON_HAND = 1001,      // 武器在手中
        WEAPON_THROWING = 1002,     // 投掷武器中
        WEAPON_REBOUNDING = 1003,   // 武器反弹
        WEAPON_ON_GROUND = 1004,    // 武器在地上
        DELETE_MESSAGE = 9998,      // 删除消息
        /**
         * 武器状态结束
         */
    }
}
