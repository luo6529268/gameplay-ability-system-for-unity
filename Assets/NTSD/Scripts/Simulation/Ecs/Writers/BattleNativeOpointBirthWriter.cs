using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeOpointBirthWriter
    {
        internal static void InitializeBirth(LF2Entity child, OPointCreateTask task)
        {
            // Alignment contract: NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001.
            LF2Entity parent = task.parent;
            ObjectPoint point = task.opoint;
            var runtime = child.Runtime;
            BattleSpawnVitalsWriter.Apply(child, point);
            child.OwnerId = -1;
            child.Team = point.team == 0 ? parent.Team : point.team;
            child.RelationTeam = point.team == 0 ? parent.RelationTeam : point.team;
            child.OwnerEntityIndex = parent.OwnerEntityIndex;
            runtime.TargetSlotIndex = 0;
            runtime.HolderStableId = 0;

            child.WriteCurrentFrameId(point.action);
            child.Frame.D = child.FrameCache.GetNativeFrameDataById(point.action);
            child.Trans.SyncDirectFrameData(child.Frame.D.wait, child.Frame.D.next, point.action);
            child.Frame.Prev = point.action;
            child.AttackingCounter = 0;
            child.SyncCollisionSnapshotToCurrentFrame();
            runtime.NativeSoundActionLatch = -1;

            if (point.kind == 1)
            {
                int rawDeltaX = RandomDelta(child.Match, point.centerx);
                double nextX = runtime.XInt +
                    rawDeltaX *
                    (child.Match?.FixedViewRunDistanceScale ?? 1.0);
                runtime.XInt = (int)nextX;
                runtime.YInt += RandomDelta(child.Match, point.centery);
                int rawDeltaZ = RandomDelta(child.Match, point.centerz);
                double nextZ = runtime.ZInt +
                    rawDeltaZ *
                    (child.Match?.FixedViewRunVerticalDistanceScale ?? 1.0);
                runtime.ZInt = (int)nextZ;
                int action = point.action + RandomDelta(child.Match, point.framea);
                child.WriteCurrentFrameId(action);
                // Descriptor follows the current action; transition latch and previous action stay at birth.
                child.Frame.D = child.FrameCache.GetNativeFrameDataById(action);
                runtime.X = nextX;
                runtime.Y = runtime.YInt;
                runtime.Z = nextZ;
                // Alignment contract: NTSD28-USER-SOURCE-COORDINATE-KIND1-RANDOM-001; reuse draws in the unscaled history.
                if (runtime.SourceRulePositionInitialized)
                {
                    runtime.SourceRuleXInt += rawDeltaX;
                    runtime.SourceRuleZInt += rawDeltaZ;
                    runtime.SourceRuleX = runtime.SourceRuleXInt;
                    runtime.SourceRuleZ = runtime.SourceRuleZInt;
                }
            }

            runtime.HitResourceInjuryDouble1A0 = parent.Runtime.HitResourceInjuryDouble1A0;
            if (parent.Runtime.HitResourceSuppression15C == 1)
                runtime.HitResourceSuppression15C = 1;
            int type = child.GetCurrentDataObjectTypeForSimulation();
            if (type == 0)
            {
                runtime.HitResourceSuppression15C = 1;
                runtime.OrdinaryCreditGate2F4 = parent.Runtime.OrdinaryCreditGate2F4 > -1
                    ? parent.Runtime.OrdinaryCreditGate2F4 : parent.Runtime.SlotIndex;
                runtime.HitStop = parent.Runtime.HitStop;
                child.AiControlled = true;
            }
            if (point.kind == 1 && point.effect > 0)
                runtime.HitStop = point.effect;
            if ((type == 0 || type == 5) && parent.Runtime.OrdinaryCreditGate2F4 != 2)
            {
                runtime.HP2Orig = point.reserve;
                runtime.RespawnCount = point.join;
                runtime.HPOrig = point.join_reserve;
                runtime.ReviveVisualId184 = point.join_pic;
            }
            int defend = child.FrameCache.Wrapper.characterData.NativeMetadata?.Stats.Int32OrDefault("defend", 0) ?? 0;
            runtime.IncomingDamageScale340 = defend > 0 && defend != 100 ? defend : 0;

            bool left = point.facing == 0 ? parent.Runtime.IsFacingLeft
                : point.facing == 1 && !parent.Runtime.IsFacingLeft;
            child.PS.dir = left ? "left" : "right";
            child.PS.vx = left ? -point.dvx : point.dvx;
            child.PS.vy = point.dvy;
            double vz = point.kind == 1 ? point.dvz : 0;
            int state = child.Frame.D?.state ?? 0;
            if ((state == 1002 || state == 3000 || state == 3006) && point.oid != 223 && point.oid != 224)
            {
                bool up = parent.Runtime.KeyUp != 0;
                bool down = parent.Runtime.KeyDown != 0;
                if (up && !down) vz = -2.5;
                else if (down && !up) vz = 2.5;
                if (point.oid == 211) vz *= 0.25;
            }
            child.PS.vz = vz;
            if (point.kind == 2)
            {
                runtime.LinkState = -1;
                runtime.HolderStableId = parent.Runtime.SlotIndex;
                parent.Runtime.TargetSlotIndex = runtime.SlotIndex;
                parent.Runtime.HeldWeaponStableId = runtime.SlotIndex;
                parent.Runtime.LinkState = point.framea == 1 ? 101 : 1;
                if (point.hp > 0) runtime.WeaponFlightCounter = point.hp;
            }
        }

        internal static void ApplySpread(LF2Entity child, int ordinal, int count)
        {
            if (count <= 1) return;
            double spread = ordinal * 10.0 / (count - 1) - 5.0;
            double vx = child.PS.vx;
            bool sameSign = (vx > 0 && spread > 0) || (vx < 0 && spread < 0);
            child.PS.vx = vx + (sameSign ? -spread : spread);
            child.PS.vz += spread;
        }

        private static int RandomDelta(SimulationWorld world, int amplitude)
        {
            if (amplitude <= 0) return 0;
            int delta = world.NativeRandom.SynchronizedNext(0x0044D3ABu, amplitude);
            return world.NativeRandom.SynchronizedNext(0x0044D3B5u, 50) >= 25 ? -delta : delta;
        }
    }
}
