using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        /// <summary>
        /// C++ release run_late_entity_update 开头的角色特殊状态处理。
        /// 顺序必须保持为：9995 角色替换 -> 4000/8000 通用替换 -> 9996 碎片。
        /// </summary>
        public override void RunStateSpecialPreCollision()
        {
            // N-26 C++ release 0x004219F1 test edx,edx + jnz：仅 entity_type==0（角色）的 state==9995
            // data 替换为 oid=50, frame=0
            var fD = Frame?.D;
            if (fD != null && fD.state == 9995)
            {
                var wrapper50 = CharacterAnimtorManager.Instance?.GetCharacterConfig(50);
                if (wrapper50 != null)
                {
                    FrameCache.Load(wrapper50);
                    ImmediateFrame(0);
                }
            }

            base.RunStateSpecialPreCollision();
        }

        /// <summary>
        /// C++ release 在 frame_logic/frame_advance 前执行的角色早期特殊逻辑。
        /// </summary>
        /// <summary>
        /// C++ release post_cooldown_input：在 cooldown 后、frame_advance 前消费输入。
        /// </summary>
        internal override void RunPostCooldownInputPhase(int tickIndex)
        {
            if (Runtime.LinkState < 0)
                return;

            InputState?.UpdateFromBuffer(Controller?.InputBuffer, tickIndex, this);
            ComboUpdate();
        }

        /// <summary>
        /// C++ release step3：state 400/401 在 frame_logic/frame_advance 前处理传送定位。
        /// </summary>
        internal override void RunEarlyTeleportSpecialsPhase(List<LF2Entity> entities, bool frameToggleGate)
        {
            if (!frameToggleGate || entities == null || PS == null || Health == null)
                return;

            int state = Frame?.D?.state ?? -1;
            bool toEnemy = state == LF2States.TeleportToEnemy;
            bool toTeammate = state == LF2States.TeleportToTeammate;
            if (!toEnemy && !toTeammate)
                return;

            LF2Character best = null;
            float bestDistance = toEnemy ? 10000f : -1f;

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] is not LF2Character target)
                    continue;
                if (target == this || target.PS == null || target.Health == null)
                    continue;
                if (target.Health.HP <= 0)
                    continue;
                if (toEnemy && target.RelationTeam == RelationTeam)
                    continue;
                if (toTeammate && target.RelationTeam != RelationTeam)
                    continue;

                float distance = Mathf.Abs(target.PS.z - PS.z) + Mathf.Abs(target.PS.x - PS.x);
                if (toEnemy && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
                else if (toTeammate && distance > bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            if (best == null)
            {
                PS.y = 0f;
                PS.vx = 0f;
                PS.vy = 0f;
                PS.vz = 0f;
                return;
            }

            float offset = toEnemy ? 120f : 60f;
            PS.z = best.PS.z + 1f;
            PS.x = PS.dir == "right" ? best.PS.x - offset : best.PS.x + offset;
            PS.y = 0f;
            PS.vx = 0f;
            PS.vy = 0f;
            PS.vz = 0f;
        }
    }
}
