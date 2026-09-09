using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    internal sealed class NTSD28InputTwoPassModule
    {
        private readonly SimulationWorld world;
        private readonly RuntimeSlotTable slots;

        internal NTSD28InputTwoPassModule(
            SimulationWorld world,
            RuntimeSlotTable slots)
        {
            this.world = world;
            this.slots = slots;
        }

        internal int LastProducerFreezeCount { get; private set; }
        internal int LastProxyCopyCount { get; private set; }
        internal int LastProxyRejectCount { get; private set; }

        internal void BeginPass()
        {
            LastProducerFreezeCount = 0;
            LastProxyCopyCount = 0;
            LastProxyRejectCount = 0;
        }

        internal void FreezeProducerState(LF2Entity entity)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            if (runtime == null)
                return;

            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            input.Current[0] = runtime.KeyUp;
            input.Current[1] = runtime.KeyDown;
            input.Current[2] = runtime.KeyLeft;
            input.Current[3] = runtime.KeyRight;
            input.Current[4] = runtime.KeyJump;
            input.Current[5] = runtime.KeyDefend;
            input.Current[6] = runtime.KeyAttack;

            input.Previous[0] = runtime.PrevUp;
            input.Previous[1] = runtime.PrevDown;
            input.Previous[2] = runtime.PrevLeft;
            input.Previous[3] = runtime.PrevRight;
            input.Previous[4] = runtime.PrevJump;
            input.Previous[5] = runtime.PrevDefend;
            input.Previous[6] = runtime.PrevAttack;

            if (world.UsesNTSD28NativeInputPipeline)
            {
                if (entity.AiControlled)
                    MergeNativeAiComboRequests(runtime);
                LastProducerFreezeCount++;
                return;
            }

            input.EdgeWindow[0] = runtime.CdAttack;
            input.EdgeWindow[1] = runtime.CdJump;
            input.EdgeWindow[2] = runtime.CdDefend;
            input.EdgeWindow[3] = runtime.CdRight;
            input.EdgeWindow[4] = runtime.CdLeft;
            input.EdgeWindow[5] = runtime.CdUp;
            input.EdgeWindow[6] = runtime.CdDown;
            input.DefendReentryCooldown = runtime.CdDefendLock;

            // This is a declared migration bridge for the still-legacy AI producer.
            // It must disappear once every producer writes the exact combo10 bank.
            byte[] combo = input.ComboState;
            combo[0] = MapHorizontal(runtime.ComboDra, runtime.ComboDla);
            combo[1] = MapHorizontal(runtime.ComboDrj, runtime.ComboDlj);
            combo[2] = MapDepth(runtime.ComboDua);
            combo[3] = MapDepth(runtime.ComboDuj);
            combo[4] = MapDepth(runtime.ComboDda);
            combo[5] = MapDepth(runtime.ComboDdj);
            combo[8] = runtime.ComboDja == 3 ? (byte)1 : (byte)0;
            LastProducerFreezeCount++;
        }

        internal void ProcessNativeSampledState(LF2Entity entity)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            if (runtime == null || !world.UsesNTSD28NativeInputPipeline)
                return;

            bool deadType0 = runtime.ObjType == 0 && runtime.HP <= 0;
            if (!deadType0)
                NTSD28NativeInputPreprocessor.ApplyCurrentButtonRemap(runtime);
            bool suppressJumpEdge =
                NTSD28NativeInputPreprocessor.ShouldSuppressJumpEdge(runtime);
            NTSD28NativeComboStateMachine.ProcessSampledInput(
                runtime,
                suppressJumpEdge);
            if (!deadType0)
            {
                world.CharacterActionWriter.RouteNativeComboAction(entity);
                world.CharacterActionWriter.RouteNativeThreeButtonFields(entity);
                world.CharacterActionWriter.RouteNativeDirectionFields(entity);
                if (!world.CharacterActionWriter.RouteNativeGroundBuiltins(entity))
                {
                    world.CharacterActionWriter
                        .RouteNativeAirDashRedirectBuiltins(entity);
                }
            }
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(runtime);
            if (entity.AiControlled)
            {
                world.CharacterInputWriter
                    .SynchronizeNativeExactAiStateFromRuntime(runtime);
            }
            if (entity is LF2Character character)
                character.InputState?.SyncFromRuntime(runtime);
        }

        internal bool TryApplyProxy(LF2Entity target)
        {
            NTSDEntityRuntime targetRuntime = target?.Runtime;
            if (targetRuntime == null ||
                targetRuntime.InputProxyCounter14C <= 0 ||
                targetRuntime.InputProxyEnabled17C == 0 ||
                targetRuntime.InputProxySourceSlot178 < 0)
            {
                return false;
            }

            int sourceSlot = targetRuntime.InputProxySourceSlot178;
            if (targetRuntime.ObjType != 0 ||
                !slots.IsAddressable(sourceSlot))
            {
                LastProxyRejectCount++;
                return false;
            }

            RuntimeSlotTable.ReadOnlySlotView sourceView =
                slots.GetReadOnlyView(sourceSlot);
            LF2Entity source = sourceView.Entity;
            NTSDEntityRuntime sourceRuntime = source?.Runtime;
            if (!sourceView.Claimed || source == null || sourceRuntime == null ||
                !world.IsActiveForCurrentPassInternal(source) ||
                sourceRuntime.HP <= 0 || sourceRuntime.ObjType != 0)
            {
                LastProxyRejectCount++;
                return false;
            }

            // Alignment contract: NTSD28-B2-AI-SAMPLE-PROXY-TWO-PASS-001.
            targetRuntime.NativeInputProxy.CopyFrom(sourceRuntime.NativeInputProxy);
            LastProxyCopyCount++;
            return true;
        }

        private static byte MapHorizontal(byte right, byte left)
        {
            if (right >= 3)
                return 4;
            if (left >= 3)
                return 5;
            if (right == 2)
                return 2;
            if (left == 2)
                return 3;
            return right == 1 || left == 1 ? (byte)1 : (byte)0;
        }

        private static byte MapDepth(byte value)
        {
            return value <= 3 ? value : (byte)0;
        }

        private static void MergeNativeAiComboRequests(
            NTSDEntityRuntime runtime)
        {
            byte[] combo = runtime.NativeInputProxy.ComboState;
            if (runtime.ComboDua == 3)
                combo[2] = 3;
            if (runtime.ComboDuj == 3)
                combo[3] = 3;
            if (runtime.ComboDda == 3)
                combo[4] = 3;
            if (runtime.ComboDdj == 3)
                combo[5] = 3;
            if (runtime.ComboDja == 3)
                combo[8] = 1;
        }

    }
}
