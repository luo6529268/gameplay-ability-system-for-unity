using System;
using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    /// <summary>
    /// Allocation-free deterministic checksum writer used by the fixed-tick path.
    /// Diagnostic canonical JSON snapshots remain a separate, explicitly requested path.
    /// </summary>
    internal struct BattleChecksum64Builder
    {
        private const ulong Offset = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        private ulong value;

        public void Reset()
        {
            value = Offset;
        }

        public void AddBoolean(bool item)
        {
            AddByte(item ? (byte)1 : (byte)0);
        }

        public void AddByte(byte item)
        {
            value = (value ^ item) * Prime;
        }

        public void AddInt32(int item)
        {
            AddUInt32(unchecked((uint)item));
        }

        public void AddUInt32(uint item)
        {
            AddByte((byte)item);
            AddByte((byte)(item >> 8));
            AddByte((byte)(item >> 16));
            AddByte((byte)(item >> 24));
        }

        public void AddInt64(long item)
        {
            AddUInt64(unchecked((ulong)item));
        }

        public void AddUInt64(ulong item)
        {
            AddUInt32((uint)item);
            AddUInt32((uint)(item >> 32));
        }

        public void AddSingle(float item)
        {
            AddInt32(BitConverter.SingleToInt32Bits(item));
        }

        public void AddDouble(double item)
        {
            AddInt64(BitConverter.DoubleToInt64Bits(item));
        }

        public void AddIntArray(int[] items)
        {
            int count = items?.Length ?? 0;
            AddInt32(count);
            for (int index = 0; index < count; index++)
                AddInt32(items[index]);
        }

        public void AddIntList(IReadOnlyList<int> items)
        {
            int count = items?.Count ?? 0;
            AddInt32(count);
            for (int index = 0; index < count; index++)
                AddInt32(items[index]);
        }

        public void AddStringOrdinal(string item)
        {
            int count = item?.Length ?? 0;
            AddInt32(count);
            for (int index = 0; index < count; index++)
                AddUInt32(item[index]);
        }

        public void AddNormalizedSoundCue(string cue)
        {
            if (string.IsNullOrEmpty(cue))
            {
                AddInt32(0);
                return;
            }

            int first = 0;
            int last = cue.Length - 1;
            while (first <= last && char.IsWhiteSpace(cue[first]))
                first++;
            while (last >= first && char.IsWhiteSpace(cue[last]))
                last--;

            int identifierStart = first;
            for (int index = first; index <= last; index++)
            {
                char current = cue[index];
                if (current == '/' || current == '\\')
                    identifierStart = index + 1;
            }

            const string prefix = "snddata_";
            if (last - identifierStart + 1 >= prefix.Length)
            {
                bool matchesPrefix = true;
                for (int index = 0; index < prefix.Length; index++)
                {
                    if (char.ToLowerInvariant(cue[identifierStart + index]) != prefix[index])
                    {
                        matchesPrefix = false;
                        break;
                    }
                }

                if (matchesPrefix)
                    identifierStart += prefix.Length;
            }

            int length = Math.Max(0, last - identifierStart + 1);
            AddInt32(length);
            for (int index = identifierStart; index <= last; index++)
                AddUInt32(char.ToLowerInvariant(cue[index]));
        }

        public ulong Complete()
        {
            ulong result = value;
            result ^= result >> 33;
            result *= 0xff51afd7ed558ccdUL;
            result ^= result >> 33;
            result *= 0xc4ceb9fe1a85ec53UL;
            result ^= result >> 33;
            return result;
        }
    }

    /// <summary>
    /// Match-owned lockstep checksum module. It reads the current simulation truth
    /// directly and never constructs canonical object graphs on the battle hot path.
    /// </summary>
    internal sealed class BattleLockstepChecksumModule
    {
        private const int SchemaVersion = 2;
        private BattleChecksum64Builder builder;

        public ulong Capture(SimulationWorld world, int tickIndex, FrameInputSet frameInput)
        {
            if (world == null)
                return 0UL;

            builder.Reset();
            builder.AddInt32(SchemaVersion);
            builder.AddInt32(tickIndex);
            AppendInput(frameInput, tickIndex);
            AppendMetadata(world, tickIndex);
            AppendWorld(world);
            AppendSlots(world);
            world.RuntimeRestStoreForServices.AppendDeterministicChecksum(ref builder);
            AppendStats(world);
            AppendEvents(world.PendingSounds);
            return builder.Complete();
        }

        private void AppendInput(FrameInputSet frameInput, int tickIndex)
        {
            if (frameInput == null)
            {
                builder.AddInt32(tickIndex);
                builder.AddInt32(0);
                return;
            }

            builder.AddInt32(frameInput.TickIndex);
            IReadOnlyList<SimulationPlayerInput> players = frameInput.Players;
            int count = players?.Count ?? 0;
            builder.AddInt32(count);
            for (int index = 0; index < count; index++)
            {
                SimulationPlayerInput player = players[index];
                builder.AddInt32(player.PlayerSlot);
                builder.AddByte((byte)player.Buttons);
                builder.AddByte((byte)player.PressedButtons);
                builder.AddByte((byte)player.ReleasedButtons);
            }
        }

        private void AppendMetadata(SimulationWorld world, int tickIndex)
        {
            builder.AddInt32((int)world.RuntimeProfileForServices);
            builder.AddInt32(tickIndex);
            builder.AddInt32(world.MaxRuntimeSlotsForServices);
            builder.AddInt32(world.ClaimedRuntimeSlotCountForServices);
            builder.AddInt32(world.ObjectCount);
            builder.AddUInt32(world.Rng?.State ?? 0U);
            builder.AddUInt64(world.Rng?.CallCount ?? 0UL);
        }

        private void AppendWorld(SimulationWorld world)
        {
            BattleRuntimeState battle = world.Runtime;
            BattleMatchRuntimeState match = battle?.Match;
            BattleStageRuntimeState stage = battle?.Stage;
            BattleFlowRuntimeState flow = battle?.Flow;
            BattleResultsRuntimeState results = battle?.Results;
            BattleRosterRuntimeState roster = battle?.Roster;
            BattleStageProgressionState progression = battle?.StageProgression;

            builder.AddInt32(match?.LocalGameModeId ?? 0);
            builder.AddInt32(match?.BattleGameModeId ?? 0);
            builder.AddInt32(match?.BackgroundId ?? -1);
            builder.AddInt32(match?.Difficulty ?? 2);
            builder.AddInt32(match?.Seed ?? 0);
            builder.AddBoolean(match?.PpMode ?? true);

            builder.AddInt32(stage?.StageWidthPx ?? 800);
            builder.AddInt32(stage?.ZMin ?? 180);
            builder.AddInt32(stage?.ZMax ?? 350);
            builder.AddInt32(stage?.BoundRight ?? 800);
            builder.AddInt32(stage?.XMaxOverride ?? 0);
            builder.AddInt32(stage?.CameraMaxOverride ?? 0);
            builder.AddInt32(world.ReleaseCameraX);
            builder.AddInt32(world.ReleaseCameraVelocityForServices);

            builder.AddInt32(flow?.CurrentTickIndex ?? 0);
            builder.AddInt32(flow?.AiPhaseGate ?? 0);
            builder.AddInt32(flow?.InputPhase ?? 0);
            builder.AddInt32(flow?.FrameMod12 ?? 0);
            builder.AddInt32(flow?.FrameToggle ?? 0);
            builder.AddInt32(flow?.AiDifficulty ?? 0);
            builder.AddInt32(flow?.AiRand3 ?? 0);
            builder.AddInt32(flow?.AiRand5 ?? 0);
            builder.AddInt32(flow?.AiRand15 ?? 0);
            builder.AddInt32(flow?.AiRand20 ?? 0);
            builder.AddInt32(flow?.AiMoveMode ?? 0);
            builder.AddInt32(flow?.AiStageTargetX ?? 0);
            builder.AddInt32(flow?.BattleStepMode ?? 0);
            builder.AddInt32(flow?.BattleStepGate ?? 0);
            builder.AddInt32(flow?.DjaGuardGlobal44F224 ?? 0);
            builder.AddBoolean(flow?.HumanInputPolledExternally ?? false);
            builder.AddBoolean(flow?.NeedClearInput ?? false);

            BattleSlotRuntimeState[] rosterSlots = roster?.Slots;
            builder.AddInt32(roster?.ActiveSlotCount ?? 0);
            for (int index = 0; index < 8; index++)
            {
                BattleSlotRuntimeState slot = rosterSlots != null && index < rosterSlots.Length
                    ? rosterSlots[index]
                    : null;
                builder.AddBoolean(slot?.Active ?? false);
                builder.AddBoolean(slot != null && slot.Active && !slot.IsHuman);
                builder.AddInt32(slot?.CharacterId ?? -1);
                builder.AddInt32(slot?.Team ?? 1);
                builder.AddInt32(slot?.RuntimeSlotIndex ?? -1);
            }

            builder.AddInt32(results?.Phase ?? 0);
            builder.AddInt32(results?.Timer ?? 0);
            builder.AddInt32(results?.Winner ?? -1);
            builder.AddBoolean(results?.HadBoth ?? false);
            builder.AddInt32(results?.BattleEndPhase ?? 0);
            builder.AddInt32(results?.PendingWinner ?? -2);
            builder.AddInt32(results?.TeamCount ?? 0);
            builder.AddIntArray(results?.TeamIds);
            builder.AddInt32(results?.PendingHostAction ?? 0);

            builder.AddInt32(progression?.StageSeriesIdx ?? 0);
            builder.AddInt32(progression?.WaveIdx ?? -1);
            builder.AddInt32(progression?.Round ?? 0);
            builder.AddInt32(progression?.RoundMax ?? 0);
            builder.AddBoolean(battle?.StageProgressionValid ?? false);
            builder.AddInt32(battle?.StageSpawnWaveApplied ?? -1);
            builder.AddInt32(battle?.StageSpawnWaveDeferredEntryApplied ?? -1);
            builder.AddInt32(battle?.StageSpawnRuntimeWave ?? -1);
            builder.AddIntList(battle?.StageSpawnRuntimeTargetTotal);
            builder.AddIntList(battle?.StageSpawnRuntimeEntryCount);
            builder.AddIntList(battle?.StageSpawnRuntimeSpawnedTotal);

            IReadOnlyList<int[]> runtimeSlots = battle?.StageSpawnRuntimeSlots;
            int runtimeSlotListCount = runtimeSlots?.Count ?? 0;
            builder.AddInt32(runtimeSlotListCount);
            for (int index = 0; index < runtimeSlotListCount; index++)
                builder.AddIntArray(runtimeSlots[index]);
        }

        private void AppendSlots(SimulationWorld world)
        {
            RuntimeSlotTable slots = world.RuntimeSlotsForServices;
            int logicalCapacity = slots.LogicalCapacity;
            builder.AddInt32(logicalCapacity);
            for (int runtimeSlot = 0; runtimeSlot < logicalCapacity; runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view = slots.GetReadOnlyView(runtimeSlot);
                LF2Entity entity = view.Entity;
                NTSDEntityRuntime runtime = entity?.Runtime ?? view.RawRuntime;
                bool projectRawState = runtime != null;

                builder.AddInt32(runtimeSlot);
                builder.AddBoolean(view.Claimed);
                builder.AddUInt32(view.Generation);
                builder.AddInt32(entity?.FrameCache?.Wrapper != null
                    ? entity.FrameCache.Wrapper.characterId
                    : entity?.ObjectId ?? -1);
                builder.AddInt32(runtime?.StableId ?? 0);
                AppendEntityRuntime(world, entity, runtimeSlot, runtime, projectRawState);
            }
        }

        private void AppendEntityRuntime(
            SimulationWorld world,
            LF2Entity entity,
            int runtimeSlot,
            NTSDEntityRuntime runtime,
            bool projectRawState)
        {
            bool isDefault = entity == null && !projectRawState;
            bool active = entity != null && world.IsActiveForCurrentPassInternal(entity);

            builder.AddBoolean(active);
            builder.AddBoolean(runtime?.AiControlled ?? false);
            builder.AddInt32(entity?.GetCurrentDataObjectTypeForSimulation() ?? -1);
            builder.AddInt32(isDefault ? -1 : runtime.ObjectId);
            builder.AddInt32(isDefault ? 0 : runtime.EntityType);
            builder.AddInt32(isDefault ? 0 : runtime.ObjType);
            builder.AddInt32(runtime?.OwnerStableId ?? -1);
            builder.AddInt32(runtimeSlot);
            builder.AddInt32(isDefault ? 0 : runtime.Team);
            builder.AddInt32(runtime?.RelationTeam ?? 0);

            builder.AddInt32(isDefault || runtime.Dir == "right" ? 0 : 1);
            builder.AddInt32(isDefault ? 0 : (int)runtime.RenderOffsetX);
            builder.AddDouble(isDefault ? 0.0 : runtime.Type3VisualZOffset);
            builder.AddDouble(isDefault ? 0.0 : runtime.X);
            builder.AddInt32(isDefault ? 0 : runtime.XInt);
            builder.AddDouble(isDefault ? 0.0 : runtime.Y);
            builder.AddInt32(isDefault ? 0 : runtime.YInt);
            builder.AddDouble(isDefault ? 0.0 : runtime.Z);
            builder.AddInt32(isDefault ? 0 : runtime.ZInt);

            builder.AddInt32(isDefault ? 0 : runtime.Fall);
            builder.AddInt32(isDefault ? 0 : runtime.HitCount);
            builder.AddDouble(isDefault ? 0.1 : runtime.KnockbackVx);
            builder.AddDouble(isDefault ? 0.1 : runtime.KnockbackVy);
            builder.AddDouble(isDefault ? 0.1 : runtime.KnockbackVz);
            builder.AddDouble(isDefault ? 0.0 : runtime.Vx);
            builder.AddDouble(isDefault ? 0.0 : runtime.Vy);
            builder.AddDouble(isDefault ? 0.0 : runtime.Vz);

            builder.AddInt32(isDefault ? 0 : runtime.AnimCounter);
            builder.AddInt32(isDefault ? 0 : runtime.AnimSub);
            builder.AddInt32(isDefault ? 0 : runtime.AttackingCounter);
            builder.AddInt32(isDefault ? 0 : runtime.Frame);
            builder.AddInt32(isDefault ? 0 : runtime.FrameDelay);
            builder.AddInt32(isDefault ? 0 : runtime.FrameWaitCounter);
            builder.AddInt32(isDefault ? 0 : runtime.HitStateCount);
            builder.AddInt32(isDefault ? 0 : runtime.HitStop);
            builder.AddInt32(isDefault ? 0 : entity?.Frame?.Prev ?? 0);
            builder.AddInt32(isDefault ? 0 : runtime.PrevFrame2);
            builder.AddInt32(isDefault ? 0 : runtime.WaitCounter);

            builder.AddInt32(isDefault ? -1 : runtime.CatcherSlotIndex);
            builder.AddInt32(isDefault ? 0 : runtime.CaughtDuration);
            builder.AddInt32(isDefault ? -1 : runtime.CaughtSlotIndex);
            builder.AddInt32(isDefault ? 0 : runtime.CatchingStateTU);
            builder.AddInt32(isDefault ? -1 : runtime.HeldWeaponStableId);
            builder.AddInt32(isDefault ? 99 : runtime.HolderCopySlotIndex);
            builder.AddInt32(isDefault ? -1 : runtime.HolderStableId);
            builder.AddInt32(isDefault ? 0 : runtime.LinkState);
            builder.AddInt32(isDefault ? -1 : runtime.PickerStableId);
            builder.AddInt32(isDefault ? 0 : runtime.PickupCount);
            builder.AddInt32(runtime?.ReleaseTick ?? -1);
            builder.AddInt32(isDefault ? -1 : runtime.TargetSlotIndex);
            builder.AddInt32(isDefault ? -1 : runtime.ThrowFrameGuard);

            builder.AddInt32(isDefault ? 0 : runtime.TransientMp);
            builder.AddInt32(isDefault ? 1000 : runtime.TransientMp2);
            builder.AddInt32(isDefault ? 1000 : runtime.TransientMp3);
            builder.AddInt32(isDefault ? 1000 : runtime.TransientMp4);

            builder.AddInt32(isDefault ? 0 : runtime.ComboCountAtk);
            builder.AddInt32(isDefault ? 0 : runtime.ComboCountVic);
            builder.AddInt32(isDefault ? 0 : runtime.FallDamageDiv);
            builder.AddInt32(isDefault ? 500 : runtime.HP);
            builder.AddInt32(isDefault ? 500 : runtime.HP3);
            builder.AddInt32(isDefault ? 500 : runtime.HPBound);
            builder.AddInt32(isDefault ? -1 : runtime.KillCount);
            builder.AddInt32(isDefault ? 0 : runtime.KillStat);
            builder.AddInt32(isDefault ? 500 : runtime.PP);
            builder.AddInt32(isDefault ? 0 : runtime.RespawnCount);
            builder.AddInt32(isDefault ? -1 : runtime.SpawnerSlotIndex);
            builder.AddInt32(isDefault ? 0 : runtime.Unk344);
            builder.AddInt32(isDefault ? 0 : runtime.WeaponCount);

            builder.AddByte(runtime?.CdAttack ?? 0);
            builder.AddByte(runtime?.CdDefend ?? 0);
            builder.AddByte(runtime?.CdDefendLock ?? 0);
            builder.AddByte(runtime?.CdDown ?? 0);
            builder.AddByte(runtime?.CdJump ?? 0);
            builder.AddByte(runtime?.CdLeft ?? 0);
            builder.AddByte(runtime?.CdRight ?? 0);
            builder.AddByte(runtime?.CdUp ?? 0);
            builder.AddByte(runtime?.ComboDda ?? 0);
            builder.AddByte(runtime?.ComboDdj ?? 0);
            builder.AddByte(runtime?.ComboDja ?? 0);
            builder.AddByte(runtime?.ComboDla ?? 0);
            builder.AddByte(runtime?.ComboDlj ?? 0);
            builder.AddByte(runtime?.ComboDra ?? 0);
            builder.AddByte(runtime?.ComboDrj ?? 0);
            builder.AddByte(runtime?.ComboDua ?? 0);
            builder.AddByte(runtime?.ComboDuj ?? 0);
            builder.AddIntArray(isDefault ? null : runtime.InputHistory);
            builder.AddByte(runtime?.KeyAttack ?? 0);
            builder.AddByte(runtime?.KeyDefend ?? 0);
            builder.AddByte(runtime?.KeyDown ?? 0);
            builder.AddByte(runtime?.KeyJump ?? 0);
            builder.AddByte(runtime?.KeyLeft ?? 0);
            builder.AddByte(runtime?.KeyRight ?? 0);
            builder.AddByte(runtime?.KeyUp ?? 0);
            builder.AddByte(runtime?.PrevAttack ?? 0);
            builder.AddByte(runtime?.PrevDefend ?? 0);
            builder.AddByte(runtime?.PrevDown ?? 0);
            builder.AddByte(runtime?.PrevJump ?? 0);
            builder.AddByte(runtime?.PrevLeft ?? 0);
            builder.AddByte(runtime?.PrevRight ?? 0);
            builder.AddByte(runtime?.PrevUp ?? 0);

            builder.AddInt32(isDefault ? 0 : runtime.Blink);
            builder.AddInt32(isDefault ? 0 : runtime.HP2Orig);
            builder.AddInt32(isDefault ? 0 : runtime.HPOrig);
            builder.AddInt32(isDefault ? 0 : runtime.PpDisplay);

            builder.AddInt32(isDefault ? 0 : runtime.AttackExempt);
            builder.AddBoolean(runtime?.ZBoundNegative ?? false);
            builder.AddBoolean(runtime?.ZBoundPositive ?? false);
            builder.AddBoolean(runtime?.XBoundNegative ?? false);
            builder.AddBoolean(runtime?.XBoundPositive ?? false);
            builder.AddInt32(runtime?.CatchTimer ?? 0);
            builder.AddInt32(isDefault ? 0 : runtime.HealTimer);
            builder.AddInt32(isDefault ? 0 : runtime.HitConfirmEa);
            builder.AddInt32(isDefault ? 0 : runtime.HitConfirm2);
            builder.AddInt32(runtime?.RenderPicOffset ?? 0);
            builder.AddInt32(runtime?.WeaponFlightCounter ?? 0);
            builder.AddInt32(runtime?.TransformOriginalObjectId ?? -1);
            builder.AddInt32(isDefault ? -1 : runtime.Unk328);
            builder.AddInt32(isDefault ? -1 : runtime.Unk32C);
            builder.AddInt32(isDefault ? 0 : runtime.Unk330);
            builder.AddInt32(isDefault ? 0 : runtime.Unk334);
            builder.AddInt32(isDefault ? 0 : runtime.Unk338);
            builder.AddInt32(runtime?.TransformTargetObjectId ?? -1);
            builder.AddInt32(isDefault ? -1 : runtime.Unk360);
            builder.AddInt32(isDefault ? -1000 : runtime.Unk3FC);
            builder.AddInt32(isDefault ? -1000 : runtime.Unk400);
            builder.AddInt32(isDefault ? 0 : runtime.WeaponState);
        }

        private void AppendStats(SimulationWorld world)
        {
            builder.AddIntArray(world.DamageStats);
            builder.AddIntArray(world.KillStats);
        }

        private void AppendEvents(IReadOnlyList<PendingSoundEvent> sounds)
        {
            int count = sounds?.Count ?? 0;
            builder.AddInt32(count);
            for (int index = 0; index < count; index++)
            {
                PendingSoundEvent sound = sounds[index];
                builder.AddNormalizedSoundCue(sound.Cue);
                builder.AddInt32(sound.Tick);
                builder.AddInt32(sound.WorldX);
            }
        }
    }
}
