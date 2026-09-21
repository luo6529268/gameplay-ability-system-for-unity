using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativePostDisplayResourceWriter
    {
        internal static void Advance(
            LF2Entity entity,
            int selectedModeFullRestoreGate18,
            bool hasStageBounds,
            int width,
            int zNear,
            int zFar)
        {
            var runtime = entity?.Runtime;
            if (runtime == null || entity.Health == null ||
                runtime.NativeLifecycleResolutionPending ||
                entity.GetCurrentDataObjectTypeForSimulation() != 0)
                return;

            // Alignment contract: NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001.
            var frame = entity.FrameCache?.GetNativeFrameDataById(runtime.Frame);
            if (frame == null)
                return;

            var metadata = entity.FrameCache.Wrapper?.characterData?.NativeMetadata;
            int action = runtime.Frame;
            int depletedAction = metadata?.Bmp.Int32OrDefault("frame_0mp", 0) ?? 0;
            bool protectedAction = (action >= 112 && action <= 114) ||
                (action >= 130 && action <= 144) ||
                (action >= 180 && action <= 192) ||
                (action >= 200 && action <= 206) ||
                (action >= 220 && action <= 231);
            if (depletedAction > 0 && entity.Health.HP > 0 &&
                entity.Health.HPBound <= 0 && !protectedAction)
                entity.WriteCurrentFrameId(depletedAction);

            int previousState = entity.FrameCache.GetNativeFrameDataById(entity.Frame.Prev)?.state ?? 0;
            switch (previousState)
            {
                case 62:
                    entity.Health.HP = 1;
                    break;
                case 63:
                    entity.Health.HP = 0;
                    entity.Health.HPBound = 0;
                    break;
                case 64:
                    entity.Health.PP = 0;
                    break;
                case 65:
                    entity.Health.HP = entity.Health.HP3;
                    entity.Health.HPBound = entity.Health.HP3;
                    break;
                case 66:
                    entity.Health.PP = 500;
                    break;
                case 405:
                    if (hasStageBounds && width > 0 && zNear <= zFar)
                    {
                        runtime.SetPosition(width / 2, runtime.YInt, zNear + (zFar - zNear) / 2);
                        runtime.SyncIntegerPosition();
                        runtime.SetVelocity(0, 0, 0);
                    }
                    break;
            }
            if (previousState >= 4000 && previousState <= 4999)
                entity.HP2Orig = previousState - 4000;
            if (previousState >= 3640 && previousState <= 3645)
                entity.RelationTeam = previousState - 3640;

            // Restore uses the entry descriptor, even after a raw frame_0mp action write.
            if (runtime.OrdinaryCreditGate2F4 == -1 && frame.state != 63 &&
                (runtime.FullRestoreTimer1B0 > 0 ||
                 selectedModeFullRestoreGate18 == 2 || selectedModeFullRestoreGate18 == 3))
            {
                entity.Health.HP = entity.Health.HP3;
                entity.Health.HPBound = entity.Health.HP3;
                runtime.InputHpConsumedTotal34C = 0;
                runtime.KnockoutCount358 = 0;
            }

            if (entity.Health.HP > entity.Health.HP3)
            {
                entity.Health.HP = entity.Health.HP3;
                entity.Health.HPBound = entity.Health.HP3;
            }
            if (entity.Health.HPBound > entity.Health.HP3)
                entity.Health.HPBound = entity.Health.HP3;
            int maxMp = metadata?.Stats.Int32OrDefault("max_mp", 0) ?? 0;
            if (maxMp > 0 && entity.Health.PP >= maxMp)
                entity.Health.PP = maxMp;
        }
    }
}
