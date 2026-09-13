using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Copies all 24 native OPoint fields across the immutable content and
    /// legacy task DTO boundary. Legacy runtime objectId never enters content.
    /// </summary>
    public static class BattleObjectPointValueAdapter
    {
        public static BattleObjectPointValue FromLegacyTask(ObjectPoint source)
        {
            return new BattleObjectPointValue(
                source.kind,
                source.x,
                source.y,
                source.action,
                source.dvx,
                source.dvy,
                source.oid,
                source.facing,
                source.z,
                source.dvz,
                source.hp,
                source.mp,
                source.team,
                source.reserve,
                source.effect,
                source.pic,
                source.centerx,
                source.centery,
                source.centerz,
                source.framea,
                source.attacking,
                source.join,
                source.join_reserve,
                source.join_pic);
        }

        public static ObjectPoint ToLegacyTask(BattleObjectPointValue source)
        {
            return new ObjectPoint
            {
                kind = source.Kind,
                x = source.X,
                y = source.Y,
                action = source.Action,
                dvx = source.Dvx,
                dvy = source.Dvy,
                oid = source.Oid,
                facing = source.Facing,
                objectId = 0,
                z = source.Z,
                dvz = source.Dvz,
                hp = source.Hp,
                mp = source.Mp,
                team = source.Team,
                reserve = source.Reserve,
                effect = source.Effect,
                pic = source.Pic,
                centerx = source.CenterX,
                centery = source.CenterY,
                centerz = source.CenterZ,
                framea = source.FrameA,
                attacking = source.Attacking,
                join = source.Join,
                join_reserve = source.JoinReserve,
                join_pic = source.JoinPic,
            };
        }
    }
}
