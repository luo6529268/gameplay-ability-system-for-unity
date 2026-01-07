///////////////////////////////////
//// This is a generated file. ////
////     Do not modify it.     ////
///////////////////////////////////

using System.Collections.Generic;

namespace GAS.Runtime
{
    public static class GTagLib
    {
        /// <summary>Ability</summary>
        public static GameplayTag Ability { get; } = new("Ability");

        /// <summary>Ability.Attack</summary>
        public static GameplayTag Ability_Attack { get; } = new("Ability.Attack");

        /// <summary>Ability.Defend</summary>
        public static GameplayTag Ability_Defend { get; } = new("Ability.Defend");

        /// <summary>Ability.Die</summary>
        public static GameplayTag Ability_Die { get; } = new("Ability.Die");

        /// <summary>Ability.Dodge</summary>
        public static GameplayTag Ability_Dodge { get; } = new("Ability.Dodge");

        /// <summary>Ability.Idle</summary>
        public static GameplayTag Ability_Idle { get; } = new("Ability.Idle");

        /// <summary>Ability.Jump</summary>
        public static GameplayTag Ability_Jump { get; } = new("Ability.Jump");

        /// <summary>Ability.Landed</summary>
        public static GameplayTag Ability_Landed { get; } = new("Ability.Landed");

        /// <summary>Ability.Move</summary>
        public static GameplayTag Ability_Move { get; } = new("Ability.Move");

        /// <summary>Ability.Run</summary>
        public static GameplayTag Ability_Run { get; } = new("Ability.Run");

        /// <summary>Ability.RunStop</summary>
        public static GameplayTag Ability_RunStop { get; } = new("Ability.RunStop");

        /// <summary>Ability.Sprint</summary>
        public static GameplayTag Ability_Sprint { get; } = new("Ability.Sprint");

        /// <summary>Ban</summary>
        public static GameplayTag Ban { get; } = new("Ban");

        /// <summary>Ban.Motion</summary>
        public static GameplayTag Ban_Motion { get; } = new("Ban.Motion");

        /// <summary>CD</summary>
        public static GameplayTag CD { get; } = new("CD");

        /// <summary>CD.Dodge</summary>
        public static GameplayTag CD_Dodge { get; } = new("CD.Dodge");

        /// <summary>CD.FireBullet</summary>
        public static GameplayTag CD_FireBullet { get; } = new("CD.FireBullet");

        /// <summary>CD.Skill0</summary>
        public static GameplayTag CD_Skill0 { get; } = new("CD.Skill0");

        /// <summary>CD.Skill1</summary>
        public static GameplayTag CD_Skill1 { get; } = new("CD.Skill1");

        /// <summary>Event</summary>
        public static GameplayTag Event { get; } = new("Event");

        /// <summary>Event.Attacking</summary>
        public static GameplayTag Event_Attacking { get; } = new("Event.Attacking");

        /// <summary>Event.Defending</summary>
        public static GameplayTag Event_Defending { get; } = new("Event.Defending");

        /// <summary>Event.Dodging</summary>
        public static GameplayTag Event_Dodging { get; } = new("Event.Dodging");

        /// <summary>Event.Dying</summary>
        public static GameplayTag Event_Dying { get; } = new("Event.Dying");

        /// <summary>Event.Idle</summary>
        public static GameplayTag Event_Idle { get; } = new("Event.Idle");

        /// <summary>Event.InAir</summary>
        public static GameplayTag Event_InAir { get; } = new("Event.InAir");

        /// <summary>Event.Jump</summary>
        public static GameplayTag Event_Jump { get; } = new("Event.Jump");

        /// <summary>Event.Landed</summary>
        public static GameplayTag Event_Landed { get; } = new("Event.Landed");

        /// <summary>Event.Locked</summary>
        public static GameplayTag Event_Locked { get; } = new("Event.Locked");

        /// <summary>Event.Moving</summary>
        public static GameplayTag Event_Moving { get; } = new("Event.Moving");

        /// <summary>Event.PerfectDefending</summary>
        public static GameplayTag Event_PerfectDefending { get; } = new("Event.PerfectDefending");

        /// <summary>Event.Running</summary>
        public static GameplayTag Event_Running { get; } = new("Event.Running");

        /// <summary>Event.RunStop</summary>
        public static GameplayTag Event_RunStop { get; } = new("Event.RunStop");

        /// <summary>Event.Sprint</summary>
        public static GameplayTag Event_Sprint { get; } = new("Event.Sprint");

        /// <summary>Faction</summary>
        public static GameplayTag Faction { get; } = new("Faction");

        /// <summary>Faction.Enemy</summary>
        public static GameplayTag Faction_Enemy { get; } = new("Faction.Enemy");

        /// <summary>Faction.Player</summary>
        public static GameplayTag Faction_Player { get; } = new("Faction.Player");

        /// <summary>StateNode</summary>
        public static GameplayTag StateNode { get; } = new("StateNode");

        /// <summary>StateNode.Buff</summary>
        public static GameplayTag State_Buff { get; } = new("StateNode.Buff");

        /// <summary>StateNode.Buff.BulkUp</summary>
        public static GameplayTag State_Buff_BulkUp { get; } = new("StateNode.Buff.BulkUp");

        /// <summary>StateNode.Buff.DefendBuff</summary>
        public static GameplayTag State_Buff_DefendBuff { get; } = new("StateNode.Buff.DefendBuff");

        /// <summary>StateNode.Buff.RunBuff</summary>
        public static GameplayTag State_Buff_RunBuff { get; } = new("StateNode.Buff.RunBuff");

        /// <summary>StateNode.Debuff</summary>
        public static GameplayTag State_Debuff { get; } = new("StateNode.Debuff");

        /// <summary>StateNode.Debuff.Death</summary>
        public static GameplayTag State_Debuff_Death { get; } = new("StateNode.Debuff.Death");

        /// <summary>StateNode.Debuff.LoseBalance</summary>
        public static GameplayTag State_Debuff_LoseBalance { get; } = new("StateNode.Debuff.LoseBalance");

        /// <summary>StateNode.Debuff.Stun</summary>
        public static GameplayTag State_Debuff_Stun { get; } = new("StateNode.Debuff.Stun");

        public static readonly IReadOnlyDictionary<string, GameplayTag> TagMap = new Dictionary<string, GameplayTag>
        {
            ["Ability"] = Ability,
            ["Ability.Attack"] = Ability_Attack,
            ["Ability.Defend"] = Ability_Defend,
            ["Ability.Die"] = Ability_Die,
            ["Ability.Dodge"] = Ability_Dodge,
            ["Ability.Idle"] = Ability_Idle,
            ["Ability.Jump"] = Ability_Jump,
            ["Ability.Landed"] = Ability_Landed,
            ["Ability.Move"] = Ability_Move,
            ["Ability.Run"] = Ability_Run,
            ["Ability.RunStop"] = Ability_RunStop,
            ["Ability.Sprint"] = Ability_Sprint,
            ["Ban"] = Ban,
            ["Ban.Motion"] = Ban_Motion,
            ["CD"] = CD,
            ["CD.Dodge"] = CD_Dodge,
            ["CD.FireBullet"] = CD_FireBullet,
            ["CD.Skill0"] = CD_Skill0,
            ["CD.Skill1"] = CD_Skill1,
            ["Event"] = Event,
            ["Event.Attacking"] = Event_Attacking,
            ["Event.Defending"] = Event_Defending,
            ["Event.Dodging"] = Event_Dodging,
            ["Event.Dying"] = Event_Dying,
            ["Event.Idle"] = Event_Idle,
            ["Event.InAir"] = Event_InAir,
            ["Event.Jump"] = Event_Jump,
            ["Event.Landed"] = Event_Landed,
            ["Event.Locked"] = Event_Locked,
            ["Event.Moving"] = Event_Moving,
            ["Event.PerfectDefending"] = Event_PerfectDefending,
            ["Event.Running"] = Event_Running,
            ["Event.RunStop"] = Event_RunStop,
            ["Event.Sprint"] = Event_Sprint,
            ["Faction"] = Faction,
            ["Faction.Enemy"] = Faction_Enemy,
            ["Faction.Player"] = Faction_Player,
            ["StateNode"] = StateNode,
            ["StateNode.Buff"] = State_Buff,
            ["StateNode.Buff.BulkUp"] = State_Buff_BulkUp,
            ["StateNode.Buff.DefendBuff"] = State_Buff_DefendBuff,
            ["StateNode.Buff.RunBuff"] = State_Buff_RunBuff,
            ["StateNode.Debuff"] = State_Debuff,
            ["StateNode.Debuff.Death"] = State_Debuff_Death,
            ["StateNode.Debuff.LoseBalance"] = State_Debuff_LoseBalance,
            ["StateNode.Debuff.Stun"] = State_Debuff_Stun,
        };
    }
}