using System;
using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Copies only the four release Bdy geometry fields across the immutable
    /// content and legacy Unity/editor DTO boundary.
    /// </summary>
    public static class BattleBodyBoxValueAdapter
    {
        public static BattleBodyBoxValue FromLegacy(BodyBox source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new BattleBodyBoxValue(
                source.x,
                source.y,
                source.w,
                source.h);
        }

        public static BodyBox ToLegacy(BattleBodyBoxValue source)
        {
            var destination = new BodyBox();
            CopyToLegacy(source, destination);
            return destination;
        }

        public static void CopyToLegacy(
            BattleBodyBoxValue source,
            BodyBox destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            destination.kind = 0;
            destination.x = source.X;
            destination.y = source.Y;
            destination.w = source.W;
            destination.h = source.H;
            destination.rawProperties.Clear();
        }
    }
}

