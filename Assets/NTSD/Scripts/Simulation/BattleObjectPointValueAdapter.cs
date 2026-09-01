using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Copies the eight formal OPoint fields across the immutable content and
    /// legacy task DTO boundary. Legacy objectId/dvz never enter content.
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
                source.facing);
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
                dvz = 0,
            };
        }
    }
}
