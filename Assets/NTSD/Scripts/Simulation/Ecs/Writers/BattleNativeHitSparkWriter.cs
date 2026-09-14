using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeHitSparkWriter
    {
        // Alignment contract: NTSD28-Q06-HIT-SPARK-UNITY-001.
        internal static void Append(SimulationWorld world, LF2Entity attacker, LF2Entity target,
            InteractionArea itr, int interactionIndex, LF2ArmorData armor, bool armorSparkGate, bool unarmoredBranch)
        {
            if (world == null || attacker?.Runtime == null || target?.Runtime == null ||
                itr == null || itr.kind != 0 || itr.spark == -1)
                return;

            var source = attacker.Runtime;
            var victim = target.Runtime;
            LF2Entity host = source.ZInt > victim.ZInt ||
                (source.ZInt == victim.ZInt && source.SlotIndex > victim.SlotIndex) ? attacker : target;
            if (host.HitRecordCount >= LF2Entity.MaxHitRecordSlots)
                return;

            int encoded = armorSparkGate && armor != null && armor.spark != 0 ? armor.spark : itr.spark;
            int group = unarmoredBranch && itr.effect == 1 ? 1 : interactionIndex;
            int id = encoded >= 100 && encoded < 200 ? encoded - 100 : (group * 2 + (itr.fall <= 60 ? 1 : 0)) * 10;
            var sourceFrame = attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame?.Prev2 ?? -1);
            var targetFrame = target.FrameCache?.GetNativeFrameDataById(target.Frame?.Prev2 ?? -1);
            if (sourceFrame == null || targetFrame == null)
                return;

            int x = attacker.Dirh() > 0
                ? Math.Min(source.XInt - sourceFrame.centerx + itr.x + itr.w, victim.XInt)
                : Math.Max(source.XInt + sourceFrame.centerx - itr.x - itr.w, victim.XInt);
            int y = source.YInt + itr.h / 2 + (itr.cover == 0 ? itr.y : itr.cover) - sourceFrame.centery;
            int targetTop = victim.YInt - sourceFrame.centery;
            if (y < targetTop)
                y = (y + targetTop) / 2;
            else if (y > victim.YInt)
                y = (y + victim.YInt) / 2;
            y += victim.ZInt;
            y += (int)(world.NativeRandom.CrtNext() % 9u) - 4;
            x += (int)(world.NativeRandom.CrtNext() % 9u) - 4;
            host.AddHitRecord(id, x, y);
        }
    }
}
