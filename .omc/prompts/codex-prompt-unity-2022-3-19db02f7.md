---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.DetailTimingDiagnostics.cs"
  - "Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs"
  - "Temp/NTSD_ProductionEntityStress.dispersed-full-ai-occupancy-epoch-detail-20260726.json"
timestamp: "2026-07-26T01:38:23.542Z"
---

--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs ---
using System;
using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Spatial;

namespace NTSD.Simulation
{
    public partial class SimulationWorld
    {
        private LF2Entity[] aiInputSlots;
        private readonly LooseQuadtreeBroadphase aiInputSpatialBroadphase = new LooseQuadtreeBroadphase();
        private readonly List<IncrementalSpatialEntry> aiInputSpatialEntries =
            new List<IncrementalSpatialEntry>(128);
        private readonly List<RuntimeEntityHandle> aiInputSpatialHandles =
            new List<RuntimeEntityHandle>(128);
        private readonly List<int> aiInputSpatialSlots = new List<int>(128);
        private readonly LooseQuadtreeBroadphase aiInputGroundSpatialBroadphase =
            new LooseQuadtreeBroadphase();
        private readonly List<IncrementalSpatialEntry> aiInputGroundSpatialEntries =
            new List<IncrementalSpatialEntry>(128);
        private readonly Dictionary<int, AiGroundTeamPartition> aiInputGroundTeamPartitions =
            new Dictionary<int, AiGroundTeamPartition>(8);
        private readonly List<AiGroundTeamPartition> aiInputActiveGroundTeamPartitions =
            new List<AiGroundTeamPartition>(8);
        private readonly LooseQuadtreeBroadphase aiInputAirSpatialBroadphase =
            new LooseQuadtreeBroadphase();
        private readonly List<IncrementalSpatialEntry> aiInputAirSpatialEntries =
            new List<IncrementalSpatialEntry>(32);
        private readonly List<int> aiSpecialScanSlots = new List<int>(32);
        private readonly List<int> aiPhase1TargetSlots = new List<int>(32);
        private int[] aiPhase1TeamBySlot;
        private uint[] aiPhase1GenerationBySlot;
        private bool aiPhase1TargetSlotsValid;
        private readonly bool[] aiMoveModeFirst10Present = new bool[10];
        private readonly bool[] aiMoveModeFirst10Eligible = new bool[10];
        private readonly uint[] aiMoveModeFirst10Generation = new uint[10];
        private readonly int[] aiMoveModeFirst10Hp = new int[10];
        private readonly int[] aiMoveModeFirst10X = new int[10];
        private readonly int[] aiMoveModeFirst10Z = new int[10];
        private int aiMoveModeTopSlot = -1;
        private int aiMoveModeTopX = -1;
        private int aiMoveModeTopZ;
        private int aiMoveModeSecondSlot = -1;
        private int aiMoveModeSecondX = -1;
        private int aiMoveModeSecondZ;
        private bool aiMoveModeFirst10Valid;
        private readonly Dictionary<int, AiTeamHpSummary> aiTeamHpSummaries =
            new Dictionary<int, AiTeamHpSummary>(8);
        private bool[] aiTeamHpSnapshotEligible;
        private int[] aiTeamHpSnapshotTeams;
        private int[] aiTeamHpSnapshotValues;
        private bool[] aiInputGroundRoleBySlot;
        private int[] aiInputGroundXBySlot;
        private int[] aiInputGroundZBySlot;
        private int[] aiInputGroundTeamBySlot;
        private uint[] aiInputGroundGenerationBySlot;
        private bool[] aiInputAirRoleBySlot;
        private ulong aiInputSlotSnapshotOccupancyEpoch;
        private bool aiInputSpatialReady;
        private bool aiInputGroundSpatialReady;
        private bool aiInputGroundTeamPartitionsValid;
        private bool aiInputAirSpatialReady;
        private int aiInputAirRoleCount;
        private bool aiInputAirRoleCountValid;
        private bool aiTeamHpSummaryValid;

        // Diagnostic A/B switch. Production uses the compact slot list built from the same snapshot.
        internal bool ForceFullAiSpecialScanForDiagnostics { get; set; }
        internal bool ForceFullAiPhase1TargetScanForDiagnostics { get; set; }
        internal bool ForceFullAiSameTeamScanForDiagnostics { get; set; }
        internal bool ForceFullAiMoveModeScanForDiagnostics { get; set; }
        internal bool ForceFullAiNearestScanForDiagnostics { get; set; }
        internal bool ForceLegacyAiNearestQueryForDiagnostics { get; set; }
        internal bool EnableAiNearestBestFirstShadowForDiagnostics { get; set; }
        internal int AiSameTeamSummaryFallbackCountForDiagnostics { get; private set; }
        internal int AiNearestBestFirstShadowMismatchCountForDiagnostics { get; private set; }
        internal string AiNearestBestFirstFirstShadowMismatchForDiagnostics { get; private set; }
        internal int AiNearestAirPassCountForDiagnostics { get; private set; }

        private struct AiTeamHpSummary
        {
            public int Count;
            public int MinHp;
            public int MinCount;
            public int SecondMinHp;

            public void Add(int hp)
            {
                if (Count == 0)
                {
                    Count = 1;
                    MinHp = hp;
                    MinCount = 1;
                    SecondMinHp = int.MaxValue;
                    return;
                }

                Count++;
                if (hp < MinHp)
                {
                    SecondMinHp = MinHp;
                    MinHp = hp;
                    MinCount = 1;
                }
                else if (hp == MinHp)
                {
                    MinCount++;
                }
                else if (hp < SecondMinHp)
                {
                    SecondMinHp = hp;
                }
            }
        }

        private sealed class AiGroundTeamPartition
        {
            internal AiGroundTeamPartition(int team)
            {
                Team = team;
            }

            internal int Team { get; }
            internal LooseQuadtreeBroadphase Broadphase { get; } =
                new LooseQuadtreeBroadphase();
            internal List<IncrementalSpatialEntry> Entries { get; } =
                new List<IncrementalSpatialEntry>(32);
        }

        private struct AiInputContext
        {
            public int Difficulty;
            public int Rand3;
            public int Rand5;
            public int Rand15;
            public int Rand20;
            public int MoveMode;
            public int StageTargetX;
            public int InputPhase;
        }

        private struct AiNearestPointFilter : IIncrementalPointNearestFilter
        {
            public SimulationWorld World;
            public LF2Entity Self;
            public int InputPhase;
            public bool Air;

            public IncrementalPointFilterDecision Evaluate(RuntimeEntityHandle handle)
            {
                int slot = handle.Slot;
                if (World == null ||
                    World.aiInputSlotSnapshotOccupancyEpoch == 0 ||
                    World.RuntimeSlotOccupancyEpochForServices !=
                    World.aiInputSlotSnapshotOccupancyEpoch ||
                    !handle.IsValid ||
                    slot < 0 ||
                    slot >= World.aiInputSlots.Length ||
                    World.aiInputGroundGenerationBySlot == null ||
                    slot >= World.aiInputGroundGenerationBySlot.Length ||
                    handle.Generation !=
                    World.aiInputGroundGenerationBySlot[slot])
                {
                    return IncrementalPointFilterDecision.Abort;
                }

                LF2Entity candidate = World.aiInputSlots[slot];
                if (candidate?.Runtime == null ||
                    candidate.Runtime.SlotIndex != slot)
                {
                    return IncrementalPointFilterDecision.Abort;
                }

                bool accepted = Air
                    ? IsAirAiTargetCandidate(Self, candidate, InputPhase)
                    : IsGroundAiTargetCandidate(Self, candidate, InputPhase);
                return accepted
                    ? IncrementalPointFilterDecision.Accept
                    : IncrementalPointFilterDecision.Reject;
            }
        }

        private void BuildAiInputSlotSnapshot()
        {
            aiInputSlotSnapshotOccupancyEpoch = 0;
            ulong occupancyEpochBefore =
                RuntimeSlotOccupancyEpochForServices;
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            AiSameTeamSummaryFallbackCountForDiagnostics = 0;
            ResetAiNearestAirPassCountForSelfCheck();
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);
            Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
            GetAllEntities(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                int slot = entity?.Runtime?.SlotIndex ?? -1;
                if (slot >= 0 && slot < aiInputSlots.Length && IsActiveForCurrentPass(entity))
                    aiInputSlots[slot] = entity;
            }
            _entityScratch.Clear();
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);
            BuildAiSnapshotIndices();
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);
            SynchronizeAiInputSpatialSnapshot();
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);

            ulong occupancyEpochAfter =
                RuntimeSlotOccupancyEpochForServices;
            if (occupancyEpochBefore == occupancyEpochAfter)
                aiInputSlotSnapshotOccupancyEpoch = occupancyEpochAfter;
        }

        private void ClearAiInputSlotSnapshot()
        {
            aiInputSlotSnapshotOccupancyEpoch = 0;
            Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
            aiSpecialScanSlots.Clear();
            aiPhase1TargetSlots.Clear();
            if (aiPhase1TeamBySlot != null)
                Array.Clear(aiPhase1TeamBySlot, 0, aiPhase1TeamBySlot.Length);
            if (aiPhase1GenerationBySlot != null)
            {
                Array.Clear(
                    aiPhase1GenerationBySlot,
                    0,
                    aiPhase1GenerationBySlot.Length);
            }
            aiPhase1TargetSlotsValid = false;
            aiTeamHpSummaries.Clear();
            if (aiTeamHpSnapshotEligible != null)
                Array.Clear(aiTeamHpSnapshotEligible, 0, aiTeamHpSnapshotEligible.Length);
            if (aiTeamHpSnapshotTeams != null)
                Array.Clear(aiTeamHpSnapshotTeams, 0, aiTeamHpSnapshotTeams.Length);
            if (aiTeamHpSnapshotValues != null)
                Array.Clear(aiTeamHpSnapshotValues, 0, aiTeamHpSnapshotValues.Length);
            if (aiInputGroundRoleBySlot != null)
                Array.Clear(aiInputGroundRoleBySlot, 0, aiInputGroundRoleBySlot.Length);
            if (aiInputGroundTeamBySlot != null)
                Array.Clear(aiInputGroundTeamBySlot, 0, aiInputGroundTeamBySlot.Length);
            if (aiInputGroundGenerationBySlot != null)
                Array.Clear(aiInputGroundGenerationBySlot, 0, aiInputGroundGenerationBySlot.Length);
            if (aiInputAirRoleBySlot != null)
                Array.Clear(aiInputAirRoleBySlot, 0, aiInputAirRoleBySlot.Length);
            aiInputSpatialReady = false;
            aiInputGroundSpatialReady = false;
            aiInputGroundTeamPartitionsValid = false;
            aiInputAirSpatialReady = false;
            aiInputAirRoleCount = 0;
            aiInputAirRoleCountValid = false;
            aiTeamHpSummaryValid = false;
            aiMoveModeFirst10Valid = false;
        }

        private void BuildAiSnapshotIndices()
        {
            EnsureAiTeamHpSnapshotCapacity();
            Array.Clear(aiTeamHpSnapshotEligible, 0, aiTeamHpSnapshotEligible.Length);
            Array.Clear(aiTeamHpSnapshotTeams, 0, aiTeamHpSnapshotTeams.Length);
            Array.Clear(aiTeamHpSnapshotValues, 0, aiTeamHpSnapshotValues.Length);
            aiTeamHpSummaries.Clear();
            aiSpecialScanSlots.Clear();
            aiPhase1TargetSlots.Clear();
            Array.Clear(aiPhase1TeamBySlot, 0, aiPhase1TeamBySlot.Length);
            Array.Clear(
                aiPhase1GenerationBySlot,
                0,
                aiPhase1GenerationBySlot.Length);
            aiPhase1TargetSlotsValid = false;
            ResetAiMoveModeFirst10Snapshot();
            bool phase1TargetSlotsProven = true;
            bool moveModeFirst10Proven = true;

            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                LF2Entity entity = aiInputSlots[slot];
                if (IsLivingCharacterDat(entity))
                {
                    int summaryTeam = Team(entity);
                    int hp = Hp(entity);
                    aiTeamHpSnapshotEligible[slot] = true;
                    aiTeamHpSnapshotTeams[slot] = summaryTeam;
                    aiTeamHpSnapshotValues[slot] = hp;

                    aiTeamHpSummaries.TryGetValue(
                        summaryTeam,
                        out AiTeamHpSummary summary);
                    summary.Add(hp);
                    aiTeamHpSummaries[summaryTeam] = summary;
                }

                if (slot >= 20 &&
                    entity != null &&
                    IsAiSpecialScanObjectId(entity.ObjectId))
                {
                    aiSpecialScanSlots.Add(slot);
                }

                if (entity == null)
                    continue;

                int team = Team(entity);
                bool handleProven = TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle);
                if (handleProven)
                {
                    aiPhase1TeamBySlot[slot] = team;
                    aiPhase1GenerationBySlot[slot] = handle.Generation;
                    if (team == 5)
                        aiPhase1TargetSlots.Add(slot);
                }
                else
                {
                    phase1TargetSlotsProven = false;
                }

                if (slot < aiMoveModeFirst10Present.Length)
                {
                    CaptureAiMoveModeFirst10Candidate(
                        slot,
                        entity,
                        handleProven,
                        handle);
                    if (!handleProven)
                        moveModeFirst10Proven = false;
                }
            }

            aiTeamHpSummaryValid = true;
            aiPhase1TargetSlotsValid = phase1TargetSlotsProven;
            aiMoveModeFirst10Valid = moveModeFirst10Proven;
        }

        private void EnsureAiTeamHpSnapshotCapacity()
        {
            if (aiTeamHpSnapshotEligible?.Length == aiInputSlots.Length &&
                aiInputGroundRoleBySlot?.Length == aiInputSlots.Length &&
                aiInputGroundTeamBySlot?.Length == aiInputSlots.Length &&
                aiInputGroundGenerationBySlot?.Length == aiInputSlots.Length &&
                aiInputAirRoleBySlot?.Length == aiInputSlots.Length &&
                aiPhase1TeamBySlot?.Length == aiInputSlots.Length &&
                aiPhase1GenerationBySlot?.Length == aiInputSlots.Length)
                return;

            aiTeamHpSnapshotEligible = new bool[aiInputSlots.Length];
            aiTeamHpSnapshotTeams = new int[aiInputSlots.Length];
            aiTeamHpSnapshotValues = new int[aiInputSlots.Length];
            aiInputGroundRoleBySlot = new bool[aiInputSlots.Length];
            aiInputGroundXBySlot = new int[aiInputSlots.Length];
            aiInputGroundZBySlot = new int[aiInputSlots.Length];
            aiInputGroundTeamBySlot = new int[aiInputSlots.Length];
            aiInputGroundGenerationBySlot = new uint[aiInputSlots.Length];
            aiInputAirRoleBySlot = new bool[aiInputSlots.Length];
            aiPhase1TeamBySlot = new int[aiInputSlots.Length];
            aiPhase1GenerationBySlot = new uint[aiInputSlots.Length];
        }

        private void ObserveAiTeamHpSummaryMutation(LF2Entity entity)
        {
            ObserveAiPhase1TargetSlotsMutation(entity);
            ObserveAiMoveModeFirst10Mutation(entity);
            ObserveAiGroundSpatialRoleMutation(entity);
            ObserveAiAirSpatialRoleMutation(entity);
            if (!aiTeamHpSummaryValid || entity?.Runtime == null)
                return;

            int slot = entity.Runtime.SlotIndex;
            if (slot < 0 || slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], entity))
            {
                aiTeamHpSummaryValid = false;
                return;
            }

            bool currentEligible = IsActiveForCurrentPass(entity) && IsLivingCharacterDat(entity);
            if (currentEligible != aiTeamHpSnapshotEligible[slot] ||
                (currentEligible &&
                 (Team(entity) != aiTeamHpSnapshotTeams[slot] ||
                  Hp(entity) != aiTeamHpSnapshotValues[slot])))
            {
                aiTeamHpSummaryValid = false;
            }
        }

        private void ObserveAiAirSpatialRoleMutation(LF2Entity entity)
        {
            if (!aiInputAirRoleCountValid)
                return;
            if (entity?.Runtime == null)
            {
                InvalidateAiAirRoleSnapshot();
                return;
            }

            int slot = Slot(entity);
            if (slot < 0 || slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
            {
                InvalidateAiAirRoleSnapshot();
                return;
            }

            bool airRole = IsAirAiSpatialRole(entity);
            if (aiInputAirRoleBySlot[slot] == airRole)
                return;

            bool updated;
            if (airRole)
            {
                int x = X(entity);
                int z = Z(entity);
                int x2 = x == int.MaxValue ? int.MaxValue : x + 1;
                int z2 = z == int.MaxValue ? int.MaxValue : z + 1;
                updated = x2 > x && z2 > z &&
                          aiInputAirSpatialBroadphase.TryUpsertIncremental(
                              handle,
                              new SpatialAabbXZ(x, z, x2, z2));
            }
            else
            {
                updated = aiInputAirSpatialBroadphase.TryRemoveIncremental(handle);
            }

            if (!updated)
            {
                ResetAiAirSpatialIndex();
            }

            aiInputAirRoleBySlot[slot] = airRole;
            aiInputAirRoleCount += airRole ? 1 : -1;
            if (aiInputAirRoleCount < 0 ||
                aiInputAirRoleCount > aiInputSlots.Length)
                InvalidateAiAirRoleSnapshot();
        }

        private void ResetAiAirSpatialIndex()
        {
            aiInputAirSpatialBroadphase.ResetIncremental();
            aiInputAirSpatialReady = false;
        }

        private void InvalidateAiAirRoleSnapshot()
        {
            ResetAiAirSpatialIndex();
            aiInputAirRoleCount = 0;
            aiInputAirRoleCountValid = false;
        }

        [System.Diagnostics.Conditional("UNITY_INCLUDE_TESTS")]
        private void ResetAiNearestAirPassCountForSelfCheck()
        {
            AiNearestAirPassCountForDiagnostics = 0;
        }

        [System.Diagnostics.Conditional("UNITY_INCLUDE_TESTS")]
        private void RecordAiNearestAirPassForSelfCheck()
        {
            AiNearestAirPassCountForDiagnostics++;
        }

        private void ObserveAiGroundSpatialRoleMutation(LF2Entity entity)
        {
            if (entity?.Runtime == null)
            {
                InvalidateAiSpatialIndicesForCoordinateMutation();
                return;
            }

            int slot = Slot(entity);
            if (slot < 0 || slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != aiInputGroundGenerationBySlot[slot])
            {
                InvalidateAiSpatialIndicesForCoordinateMutation();
                return;
            }

            int x = X(entity);
            int z = Z(entity);
            if (aiInputGroundXBySlot[slot] != x ||
                aiInputGroundZBySlot[slot] != z)
            {
                InvalidateAiSpatialIndicesForCoordinateMutation();
                return;
            }

            ObserveAiGroundTeamPartitionMutation(entity);
            if (!aiInputGroundSpatialReady)
                return;

            bool groundRole = IsGroundAiSpatialRole(entity);
            bool previousGroundRole = aiInputGroundRoleBySlot[slot];
            if (previousGroundRole == groundRole)
                return;

            bool updated;
            if (groundRole)
            {
                int x2 = x == int.MaxValue ? int.MaxValue : x + 1;
                int z2 = z == int.MaxValue ? int.MaxValue : z + 1;
                updated = x2 > x && z2 > z &&
                          aiInputGroundSpatialBroadphase.TryUpsertIncremental(
                              handle,
                              new SpatialAabbXZ(x, z, x2, z2));
            }
            else
            {
                updated = aiInputGroundSpatialBroadphase.TryRemoveIncremental(handle);
            }

            if (!updated)
            {
                aiInputGroundSpatialBroadphase.ResetIncremental();
                aiInputGroundSpatialReady = false;
                return;
            }

            aiInputGroundRoleBySlot[slot] = groundRole;
            aiInputGroundXBySlot[slot] = x;
            aiInputGroundZBySlot[slot] = z;
        }

        private void InvalidateAiSpatialIndicesForCoordinateMutation()
        {
            aiInputSpatialBroadphase.ResetIncremental();
            aiInputSpatialReady = false;
            aiInputGroundSpatialBroadphase.ResetIncremental();
            aiInputGroundSpatialReady = false;
            ResetAiAirSpatialIndex();
            InvalidateAiGroundTeamPartitions();
        }

        private void ObserveAiGroundTeamPartitionMutation(LF2Entity entity)
        {
            if (!aiInputGroundTeamPartitionsValid)
                return;
            if (entity?.Runtime == null)
            {
                InvalidateAiGroundTeamPartitions();
                return;
            }

            int slot = Slot(entity);
            if (slot < 0 || slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle) ||
                handle.Generation != aiInputGroundGenerationBySlot[slot])
            {
                InvalidateAiGroundTeamPartitions();
                return;
            }

            bool groundRole = IsGroundAiSpatialRole(entity);
            if (groundRole != aiInputGroundRoleBySlot[slot] ||
                Team(entity) != aiInputGroundTeamBySlot[slot] ||
                X(entity) != aiInputGroundXBySlot[slot] ||
                Z(entity) != aiInputGroundZBySlot[slot])
            {
                InvalidateAiGroundTeamPartitions();
            }
        }

        private void PrepareAiGroundTeamPartitionsForSnapshot()
        {
            for (int index = 0; index < aiInputActiveGroundTeamPartitions.Count; index++)
                aiInputActiveGroundTeamPartitions[index].Entries.Clear();
            aiInputActiveGroundTeamPartitions.Clear();
            aiInputGroundTeamPartitionsValid = false;
        }

        private AiGroundTeamPartition GetAiGroundTeamPartition(int team)
        {
            if (!aiInputGroundTeamPartitions.TryGetValue(team, out AiGroundTeamPartition partition))
            {
                partition = new AiGroundTeamPartition(team);
                aiInputGroundTeamPartitions.Add(team, partition);
            }

            if (partition.Entries.Count == 0)
                aiInputActiveGroundTeamPartitions.Add(partition);
            return partition;
        }

        private void InvalidateAiGroundTeamPartitions()
        {
            aiInputGroundTeamPartitionsValid = false;
            for (int index = 0; index < aiInputActiveGroundTeamPartitions.Count; index++)
                aiInputActiveGroundTeamPartitions[index].Broadphase.ResetIncremental();
        }

        private void SynchronizeAiGroundTeamPartitions(in SpatialAabbXZ preferredRoot)
        {
            if (aiInputActiveGroundTeamPartitions.Count > 2)
            {
                InvalidateAiGroundTeamPartitions();
                return;
            }

            for (int index = 0; index < aiInputActiveGroundTeamPartitions.Count; index++)
            {
                AiGroundTeamPartition partition = aiInputActiveGroundTeamPartitions[index];
                SpatialSynchronizeResult result =
                    partition.Broadphase.Synchronize(partition.Entries, preferredRoot);
                if (!result.Succeeded || result.IndexedCount != partition.Entries.Count)
                {
                    InvalidateAiGroundTeamPartitions();
                    return;
                }
            }

            aiInputGroundTeamPartitionsValid = true;
        }

        private bool TryGetAiSameTeamSummaryExcludingSelf(
            LF2Entity self,
            out int otherCount,
            out int otherMinHp)
        {
            otherCount = 0;
            otherMinHp = int.MaxValue;
            if (!aiTeamHpSummaryValid || self?.Runtime == null)
                return false;

            int slot = Slot(self);
            int selfTeam = Team(self);
            int selfHp = Hp(self);
            if (slot < 0 || slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], self) ||
                !aiTeamHpSnapshotEligible[slot] ||
                aiTeamHpSnapshotTeams[slot] != selfTeam ||
                aiTeamHpSnapshotValues[slot] != selfHp ||
                !aiTeamHpSummaries.TryGetValue(selfTeam, out AiTeamHpSummary summary))
            {
                aiTeamHpSummaryValid = false;
                return false;
            }

            otherCount = summary.Count - 1;
            if (otherCount <= 0)
                return true;

            otherMinHp = selfHp == summary.MinHp && summary.MinCount == 1
                ? summary.SecondMinHp
                : summary.MinHp;
            return true;
        }

        private void ScanAiSameTeamSummaryExcludingSelf(
            LF2Entity self,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            otherCount = 0;
            otherMinHp = int.MaxValue;
            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                LF2Entity teammate = AiAt(slot);
                if (teammate == null || teammate == self ||
                    !IsLivingCharacterDat(teammate) || Team(teammate) != selfTeam)
                {
                    continue;
                }

                int teammateHp = Hp(teammate);
                if (teammateHp < otherMinHp)
                    otherMinHp = teammateHp;
                otherCount++;
            }
        }

        private bool ResolveAiSameTeamSummaryExcludingSelf(
            LF2Entity self,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            if (!ForceFullAiSameTeamScanForDiagnostics &&
                TryGetAiSameTeamSummaryExcludingSelf(self, out otherCount, out otherMinHp))
            {
                return true;
            }

            if (!ForceFullAiSameTeamScanForDiagnostics)
                AiSameTeamSummaryFallbackCountForDiagnostics++;
            ScanAiSameTeamSummaryExcludingSelf(self, selfTeam, out otherCount, out otherMinHp);
            return false;
        }

        private static bool IsAiSpecialScanObjectId(int objectId)
        {
            return objectId / 100 == 1 ||
                   objectId == 0xC8 ||
                   objectId == 0xD3 ||
                   objectId == 0xD4 ||
                   objectId == 0xD5;
        }

        private void ResetAiMoveModeFirst10Snapshot()
        {
            Array.Clear(
                aiMoveModeFirst10Present,
                0,
                aiMoveModeFirst10Present.Length);
            Array.Clear(
                aiMoveModeFirst10Eligible,
                0,
                aiMoveModeFirst10Eligible.Length);
            Array.Clear(
                aiMoveModeFirst10Generation,
                0,
                aiMoveModeFirst10Generation.Length);
            Array.Clear(
                aiMoveModeFirst10Hp,
                0,
                aiMoveModeFirst10Hp.Length);
            Array.Clear(
                aiMoveModeFirst10X,
                0,
                aiMoveModeFirst10X.Length);
            Array.Clear(
                aiMoveModeFirst10Z,
                0,
                aiMoveModeFirst10Z.Length);
            aiMoveModeTopSlot = -1;
            aiMoveModeTopX = -1;
            aiMoveModeTopZ = 0;
            aiMoveModeSecondSlot = -1;
            aiMoveModeSecondX = -1;
            aiMoveModeSecondZ = 0;
            aiMoveModeFirst10Valid = false;
        }

        private void CaptureAiMoveModeFirst10Candidate(
            int slot,
            LF2Entity entity,
            bool handleProven,
            RuntimeEntityHandle handle)
        {
            aiMoveModeFirst10Present[slot] = true;
            aiMoveModeFirst10Hp[slot] = Hp(entity);
            if (handleProven)
                aiMoveModeFirst10Generation[slot] = handle.Generation;

            bool eligible = IsLivingCharacterDat(entity);
            aiMoveModeFirst10Eligible[slot] = eligible;
            if (!eligible)
                return;

            int x = X(entity);
            int z = Z(entity);
            aiMoveModeFirst10X[slot] = x;
            aiMoveModeFirst10Z[slot] = z;
            if (x <= -1)
                return;

            if (aiMoveModeTopSlot < 0 || x > aiMoveModeTopX)
            {
                aiMoveModeSecondSlot = aiMoveModeTopSlot;
                aiMoveModeSecondX = aiMoveModeTopX;
                aiMoveModeSecondZ = aiMoveModeTopZ;
                aiMoveModeTopSlot = slot;
                aiMoveModeTopX = x;
                aiMoveModeTopZ = z;
                return;
            }

            if (aiMoveModeSecondSlot < 0 || x > aiMoveModeSecondX)
            {
                aiMoveModeSecondSlot = slot;
                aiMoveModeSecondX = x;
                aiMoveModeSecondZ = z;
            }
        }

        private void ObserveAiMoveModeFirst10Mutation(LF2Entity entity)
        {
            if (!aiMoveModeFirst10Valid)
                return;
            if (entity?.Runtime == null)
            {
                aiMoveModeFirst10Valid = false;
                return;
            }

            int slot = Slot(entity);
            if (slot < 0 || slot >= aiMoveModeFirst10Present.Length)
            {
                for (int index = 0;
                     index < aiMoveModeFirst10Present.Length;
                     index++)
                {
                    if (ReferenceEquals(aiInputSlots[index], entity))
                    {
                        aiMoveModeFirst10Valid = false;
                        break;
                    }
                }
                return;
            }

            if (!aiMoveModeFirst10Present[slot] ||
                !ReferenceEquals(aiInputSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != aiMoveModeFirst10Generation[slot] ||
                Hp(entity) != aiMoveModeFirst10Hp[slot])
            {
                aiMoveModeFirst10Valid = false;
                return;
            }

            bool eligible = IsLivingCharacterDat(entity);
            if (eligible != aiMoveModeFirst10Eligible[slot] ||
                (eligible &&
                 (X(entity) != aiMoveModeFirst10X[slot] ||
                  Z(entity) != aiMoveModeFirst10Z[slot])))
            {
                aiMoveModeFirst10Valid = false;
            }
        }

        private void ObserveAiPhase1TargetSlotsMutation(LF2Entity entity)
        {
            if (!aiPhase1TargetSlotsValid)
                return;
            if (entity?.Runtime == null)
            {
                aiPhase1TargetSlotsValid = false;
                return;
            }

            int slot = Slot(entity);
            if (slot < 0 || slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != aiPhase1GenerationBySlot[slot] ||
                Team(entity) != aiPhase1TeamBySlot[slot])
            {
                aiPhase1TargetSlotsValid = false;
            }
        }

        private void SynchronizeAiInputSpatialSnapshot()
        {
            aiInputSpatialEntries.Clear();
            aiInputGroundSpatialEntries.Clear();
            aiInputAirSpatialEntries.Clear();
            PrepareAiGroundTeamPartitionsForSnapshot();
            Array.Clear(aiInputGroundRoleBySlot, 0, aiInputGroundRoleBySlot.Length);
            Array.Clear(aiInputGroundTeamBySlot, 0, aiInputGroundTeamBySlot.Length);
            Array.Clear(
                aiInputGroundGenerationBySlot,
                0,
                aiInputGroundGenerationBySlot.Length);
            Array.Clear(aiInputAirRoleBySlot, 0, aiInputAirRoleBySlot.Length);
            aiInputAirRoleCount = 0;
            aiInputAirRoleCountValid = false;
            bool hasBounds = false;
            bool spatialCoordinatesValid = true;
            int minX = 0;
            int minZ = 0;
            int maxX = 0;
            int maxZ = 0;
            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                LF2Entity entity = aiInputSlots[slot];
                if (entity == null)
                    continue;
                if (!TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                {
                    aiInputSpatialBroadphase.ResetIncremental();
                    aiInputGroundSpatialBroadphase.ResetIncremental();
                    InvalidateAiGroundTeamPartitions();
                    InvalidateAiAirRoleSnapshot();
                    aiInputSpatialReady = false;
                    aiInputGroundSpatialReady = false;
                    return;
                }

                bool airRole = IsAirAiSpatialRole(entity);
                aiInputAirRoleBySlot[slot] = airRole;
                if (airRole)
                    aiInputAirRoleCount++;

                int x = X(entity);
                int z = Z(entity);
                int x2 = x == int.MaxValue ? int.MaxValue : x + 1;
                int z2 = z == int.MaxValue ? int.MaxValue : z + 1;
                if (x2 <= x || z2 <= z)
                {
                    spatialCoordinatesValid = false;
                    continue;
                }

                var bounds = new SpatialAabbXZ(x, z, x2, z2);
                var entry = new IncrementalSpatialEntry(handle, bounds);
                aiInputSpatialEntries.Add(entry);
                bool groundRole = IsGroundAiSpatialRole(entity);
                aiInputGroundRoleBySlot[slot] = groundRole;
                aiInputGroundXBySlot[slot] = x;
                aiInputGroundZBySlot[slot] = z;
                aiInputGroundTeamBySlot[slot] = Team(entity);
                aiInputGroundGenerationBySlot[slot] = handle.Generation;
                if (groundRole)
                {
                    aiInputGroundSpatialEntries.Add(entry);
                    GetAiGroundTeamPartition(Team(entity)).Entries.Add(entry);
                }
                if (airRole)
                {
                    aiInputAirSpatialEntries.Add(entry);
                }
                if (!hasBounds)
                {
                    minX = x;
                    minZ = z;
                    maxX = x2;
                    maxZ = z2;
                    hasBounds = true;
                }
                else
                {
                    minX = Math.Min(minX, x);
                    minZ = Math.Min(minZ, z);
                    maxX = Math.Max(maxX, x2);
                    maxZ = Math.Max(maxZ, z2);
                }
            }

            aiInputAirRoleCountValid =
                aiInputAirRoleCount >= 0 &&
                aiInputAirRoleCount <= aiInputSlots.Length;
            if (!spatialCoordinatesValid || !hasBounds)
            {
                aiInputSpatialBroadphase.ResetIncremental();
                aiInputGroundSpatialBroadphase.ResetIncremental();
                InvalidateAiGroundTeamPartitions();
                InvalidateAiAirRoleSnapshot();
                aiInputSpatialReady = false;
                aiInputGroundSpatialReady = false;
                return;
            }

            var preferredRoot = new SpatialAabbXZ(minX, minZ, maxX, maxZ);
            SynchronizeAiGroundTeamPartitions(preferredRoot);
            SpatialSynchronizeResult result = aiInputSpatialBroadphase.Synchronize(
                aiInputSpatialEntries,
                preferredRoot);
            aiInputSpatialReady = result.Succeeded &&
                                  result.IndexedCount == aiInputSpatialEntries.Count;
            if (!aiInputSpatialReady)
            {
                aiInputSpatialBroadphase.ResetIncremental();
                InvalidateAiGroundTeamPartitions();
            }

            if (aiInputGroundSpatialEntries.Count == aiInputSpatialEntries.Count)
            {
                aiInputGroundSpatialBroadphase.ResetIncremental();
                aiInputGroundSpatialReady = false;
            }
            else
            {
                SpatialSynchronizeResult groundResult =
                    aiInputGroundSpatialBroadphase.Synchronize(
                        aiInputGroundSpatialEntries,
                        preferredRoot);
                aiInputGroundSpatialReady = groundResult.Succeeded &&
                                            groundResult.IndexedCount ==
                                            aiInputGroundSpatialEntries.Count;
                if (!aiInputGroundSpatialReady)
                    aiInputGroundSpatialBroadphase.ResetIncremental();
            }

            SpatialSynchronizeResult airResult = aiInputAirSpatialBroadphase.Synchronize(
                aiInputAirSpatialEntries,
                preferredRoot);
            aiInputAirSpatialReady = airResult.Succeeded &&
                                     airResult.IndexedCount ==
                                     aiInputAirSpatialEntries.Count;
            if (!aiInputAirSpatialReady)
            {
                ResetAiAirSpatialIndex();
            }
        }

        private bool TryQueryAiInputSlots(in SpatialAabbXZ bounds, out List<int> slots)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.RecordSpatialQuery();
            slots = aiInputSpatialSlots;
            slots.Clear();
            if (!aiInputSpatialReady || !bounds.IsValid)
                return false;

            aiInputSpatialHandles.Clear();
            try
            {
                aiInputSpatialBroadphase.QueryHandles(bounds, aiInputSpatialHandles);
                diagnostics?.RecordQueriedHandleVisits(aiInputSpatialHandles.Count);
            }
            catch
            {
                aiInputSpatialBroadphase.ResetIncremental();
                aiInputSpatialReady = false;
                return false;
            }

            for (int i = 0; i < aiInputSpatialHandles.Count; i++)
            {
                RuntimeEntityHandle handle = aiInputSpatialHandles[i];
                int slot = handle.Slot;
                if (slot < 0 || slot >= aiInputSlots.Length ||
                    !TryResolveRuntimeHandle(handle, out LF2Entity entity) ||
                    !ReferenceEquals(entity, aiInputSlots[slot]))
                {
                    slots.Clear();
                    aiInputSpatialBroadphase.ResetIncremental();
                    aiInputSpatialReady = false;
                    return false;
                }
                slots.Add(slot);
            }

            // Synchronize rejects duplicate handles/slots, and every incremental record
            // belongs to exactly one node. QueryHandles visits each node once, so its
            // result cannot contain a duplicate slot. Preserve its native traversal order.
            return true;
        }

        internal void PrepareAiInputBasic(LF2Entity self, int tickIndex)
        {
            if (self?.Runtime == null || self.Runtime.HP <= 0)
                return;

            NTSDEntityRuntime input = self.Runtime;
            if (input.Unk3FC > -1000)
            {
                RollAndClearAiKeys(input);
                MoveTowardCoordinate(self, CreateCoordinateAiInputContext());
                input.ApplyInputEdges();
                return;
            }

            AiInputContext ai = CreateAiInputContext(self, tickIndex);

            int selectedSlot = FindNearestAiTargetSlot(self, ai, out int bestDist, out bool sameZLane);
            int savedTargetSlot = input.Unk360;
            LF2Entity cached = AiAt(savedTargetSlot);
            if (IsLivingCharacterDat(cached) && Rand(30) > 0)
                selectedSlot = savedTargetSlot;
            else
                input.Unk360 = selectedSlot;

            if (selectedSlot < 0)
            {
                RollAndClearAiKeys(input);
                AiPostNoTargetFallback(self, cached, ai);
                input.ApplyInputEdges();
                return;
            }

            int selectedBeforeSpecialScan = selectedSlot;
            bool specialObjectProximity = false;
            bool specialLeft = false;
            bool specialRight = false;
            bool specialUp = false;
            bool specialDown = false;
            bool specialGuard7A = false;
            bool specialGuard7B = false;
            bool specialForce7AGround = false;
            bool specialC8ThreatSeen = false;
            bool specialPostSelectionSeen = false;
            int specialBestDist = 10000;

            if (ai.InputPhase == 1 || ai.InputPhase == 4)
            {
                int selfTeam = Team(self);
                if (selfTeam != 5)
                {
                    specialForce7AGround = true;
                    if (Hp(self) > (4 * Hp3(self)) / 5 || Hp(self) > Hp3(self) - 130)
                        specialForce7AGround = false;
                    if (Hp(self) > 430 || Hp(self) > Hp3(self) - 130)
                        specialGuard7A = true;

                    ResolveAiSameTeamSummaryExcludingSelf(
                        self,
                        selfTeam,
                        out int sameTeamCount,
                        out int sameTeamMinHp);
                    if (sameTeamMinHp < Hp(self)) specialForce7AGround = false;
                    if (sameTeamMinHp < Hp(self) - 200) specialGuard7A = true;
                    if (sameTeamCount == 0) specialForce7AGround = false;
                }
            }

            if (self.Runtime.KillCount > -1) { specialGuard7A = true; specialGuard7B = true; }
            if (Pp(self) > 250) specialGuard7B = true;
            if (ai.InputPhase == 1 && Team(self) == 1) specialGuard7B = true;
            if (Slot(self) >= 20 && ai.InputPhase == 4) specialGuard7B = true;

            int specialScanCount = ForceFullAiSpecialScanForDiagnostics
                ? aiInputSlots.Length - 20
                : aiSpecialScanSlots.Count;
            for (int specialScanIndex = 0; specialScanIndex < specialScanCount; specialScanIndex++)
            {
                int i = ForceFullAiSpecialScanForDiagnostics
                    ? specialScanIndex + 20
                    : aiSpecialScanSlots[specialScanIndex];
                LF2Entity obj = AiAt(i);
                if (obj == null) continue;
                int objOid = obj.ObjectId;
                int objState = State(obj);
                if (objOid == 0xC8)
                {
                    int frameGroup = Frame(obj) / 10;
                    bool threat = frameGroup == 6 && Team(obj) != Team(self);
                    if (!threat && frameGroup == 5)
                    {
                        bool lowHpWindow = (Hp(self) >= Hp3(self) - 70 || Hp(self) >= Hp3(self) - 200) &&
                                           (Hp(self) >= (3 * Hp3(self)) / 5 || Hp(self) < Hp3(self) - 200);
                        threat = (self.ObjectId == 2 || self.ObjectId == 34) && lowHpWindow && Team(obj) == Team(self);
                    }
                    if (threat) specialC8ThreatSeen = true;
                    if (threat && Abs(Z(obj) - Z(self)) < 25 && Abs(X(obj) - X(self)) < 150)
                    {
                        specialObjectProximity = true;
                        if (Abs(Z(obj) - Z(self)) < 20)
                        {
                            if (Abs(X(obj) - X(self)) < 180)
                            {
                                if (Z(obj) <= Z(self)) specialUp = true; else specialDown = true;
                            }
                            if (X(obj) <= X(self)) specialLeft = true; else specialRight = true;
                        }
                    }
                }

                if ((objOid == 0xD3 && objState == 0x12) || (objOid == 0xD4 && Frame(obj) >= 150 && Frame(obj) <= 170))
                {
                    if (Abs(X(obj) - X(self)) < 80)
                    {
                        if (Z(obj) > Z(self) + 20) specialDown = true;
                        else if (Z(obj) < Z(self) - 20) specialUp = true;
                    }
                    if (Abs(Z(obj) - Z(self)) < 20)
                    {
                        if (X(obj) > X(self) + 100) specialRight = true;
                        else if (X(obj) < X(self) - 100) specialLeft = true;
                    }
                }

                if (!specialPostSelectionSeen && !specialC8ThreatSeen && !sameZLane && input.LinkState == 0)
                {
                    int dist = Distance(self, obj);
                    bool oidCandidate = objOid / 100 == 1 || objOid == 0xD5;
                    bool guarded = (objOid == 0x7A && specialGuard7A) || (objOid == 0x7B && specialGuard7B) ||
                                   (input.HasInputHistoryGate() && objOid != 0x7A);
                    if (dist < 2 * bestDist && dist < specialBestDist && oidCandidate && !guarded &&
                        obj.Runtime.LinkState == 0 && (objState == 0x3EC || objState == 0x7D4))
                    {
                        selectedSlot = i;
                        specialBestDist = dist;
                    }
                }

                if (objOid == 0xC8 && Frame(obj) / 10 == 5 && Abs(X(obj) - X(self)) < 300 &&
                    Abs(Z(obj) - Z(self)) < 90 && Team(obj) == Team(self))
                {
                    bool pressure = (Hp(self) < HpMax(self) - 70 && Hp(self) < 140) ||
                                    (Hp(self) < (3 * HpMax(self)) / 5 && Hp(self) >= 140);
                    if (pressure) selectedSlot = i;
                    specialPostSelectionSeen = true;
                }

                if (specialForce7AGround && objOid == 0x7A && objState == 0x3EC && input.LinkState == 0)
                {
                    selectedSlot = i;
                    specialPostSelectionSeen = true;
                }
            }

            if (specialC8ThreatSeen) selectedSlot = selectedBeforeSpecialScan;
            input.Unk360 = selectedSlot;
            RollAndClearAiKeys(input);
            LF2Entity target = AiAt(selectedSlot);
            if (target == null) { input.ApplyInputEdges(); return; }
            int selfState = State(self);
            int targetState = State(target);
            int targetOid = target.ObjectId;

            if (X(target) > X(self) && Facing(self) == 1) input.KeyRight = 1;
            if (X(target) < X(self) && Facing(self) == 0) input.KeyLeft = 1;
            if (selfState == 2) { if (Facing(self) == 1) input.KeyRight = 1; else input.KeyLeft = 1; }

            int blockRoll = Rand(ai.Rand5 + 8);
            if (blockRoll == 0 && (input.ZBoundNegative || input.ZBoundPositive || input.XBoundNegative || input.XBoundPositive))
            { input.PrevJump = 0; input.KeyJump = 1; }

            if (AiPreUpdateTarget3000SideEffect(self, target, selfState, targetState, ai)) { input.ApplyInputEdges(); return; }

            if (input.HasInputHistoryGate() && input.LinkState > 0)
            {
                LF2Entity held = AiAt(input.TargetSlotIndex);
                if (held != null && (held.ObjectId == 0x7A || held.ObjectId == 0x7B))
                { input.PrevJump = 0; input.KeyJump = 1; input.ApplyInputEdges(); return; }
            }

            bool coordinateAllowsSpecial = !input.HasInputHistoryGate() || AiPostCacheCoordinateAllowsSpecial(self);
            if (coordinateAllowsSpecial && (targetState == 0x3EC || targetState == 0x7D4))
            {
                if (input.HasInputHistoryGate() && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240) &&
                    targetOid != 0x7A && targetOid != 0x7B) { input.ApplyInputEdges(); return; }
                MoveTowardTarget(self, target, ai, selfState);
                if (Abs(Z(target) - Z(self)) <= 3 && Abs(X(target) - X(self)) <= 6) { input.PrevJump = 0; input.KeyJump = 1; }
                input.ApplyInputEdges(); return;
            }

            if (targetState == 14 || Abs(Y(target)) > 2)
            {
                if (X(target) > ai.StageTargetX - 30) { input.KeyLeft = 1; input.PrevLeft = 0; input.ApplyInputEdges(); return; }
                if (X(target) < 30) { input.KeyRight = 1; input.PrevRight = 0; input.ApplyInputEdges(); return; }
                if (Abs(Z(target) - Z(self)) <= 45 || Abs(X(target) - X(self)) <= 350)
                {
                    if (X(target) > X(self)) { input.KeyLeft = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevLeft = 0; }
                    else { input.KeyRight = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevRight = 0; }
                    if (Z(target) < Z(self) || Z(target) < StageZMin + 10) input.KeyDown = 1; else input.KeyUp = 1;
                }
                input.ApplyInputEdges(); return;
            }

            bool c8Allowed = (input.HasInputHistoryGate() && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240)) ||
                             (targetState != 14 && Abs(Y(target)) <= 2);
            if (c8Allowed && targetOid == 0xC8)
            {
                if (X(target) > X(self) + 7) input.KeyRight = 1; else if (X(target) < X(self) - 7) input.KeyLeft = 1;
                if (Z(target) > Z(self) + 2) input.KeyDown = 1; else if (Z(target) < Z(self) - 2) input.KeyUp = 1;
                input.ApplyInputEdges(); return;
            }

            if (Rand(ai.Rand5 + 1) == 0)
            {
                if (AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                    AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane) ||
                    AiUpdateOid1ComboDecision(self, target, targetState) ||
                    AiUpdateCloseOid1Decision(self, target) ||
                    AiUpdateOid4ComboDecision(self, target) ||
                    AiUpdateOid5ComboDecision(self, target))
                { input.ApplyInputEdges(); return; }
            }

            if (AiUpdateOid33_19_16PredictedDuaDecision(self, target, targetState) ||
                AiUpdateOid52_1_2_21PreLabel591Decision(self, target, targetState) ||
                AiUpdateLabel591Oid51_2_18_7Decision(self, target))
            { input.ApplyInputEdges(); return; }

            bool closeOrFree = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
            int selfOid = self.ObjectId;
            bool widePath = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if (!widePath)
            {
                bool targetPressure = Hp(target) > Hp(self) * 2 || (Hp(self) <= 100 && Hp3(self) > 100);
                widePath = targetPressure && ai.InputPhase == 1 && IsCharacterDat(target) && Slot(self) >= 20 && Team(self) != 5;
            }

            if (closeOrFree)
            {
                if ((specialRight || ai.MoveMode == 1) && selfState == 2 && Facing(self) == 0) input.KeyLeft = 1;
                if (specialLeft && selfState == 2 && Facing(self) == 1) input.KeyRight = 1;
                int threshold = widePath ? 170 : 60;
                int near = widePath ? 150 : 0;
                if (selfState != 19)
                {
                    if ((X(target) > X(self) + threshold || ((X(target) > X(self) + near || (selfState == 7 && X(target) > X(self))) && Facing(self) == 1)) &&
                        !specialRight && ((widePath && ai.MoveMode == 0) || (!widePath && (ai.MoveMode == 0 || Facing(self) == 1))))
                    { input.KeyRight = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevRight = 0; }
                    if ((X(target) < X(self) - threshold || ((X(target) < X(self) - near || (selfState == 7 && X(target) < X(self))) && Facing(self) == 0)) && !specialLeft)
                    { input.KeyLeft = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevLeft = 0; }
                    if (((Z(target) > Z(self) + 3 && !specialObjectProximity) || ((specialRight || specialLeft) && specialUp)) && !specialDown) input.KeyDown = 1;
                    if (((Z(target) < Z(self) - 3 && !specialObjectProximity) || ((specialRight || specialLeft) && specialDown)) && !specialUp) input.KeyUp = 1;
                }
            }

            if (input.LinkState > 0 && !AiProcessHelper(self, target, ai, selfState, targetState, sameZLane, specialObjectProximity))
            { input.ApplyInputEdges(); return; }

            if (Rand(ai.Difficulty * 7 + 10) == 0 && (targetState == 3 || targetState / 100 == 3) &&
                Abs(Z(target) - Z(self)) < 9 && ((Facing(target) == 0 && X(target) < X(self)) || (Facing(target) == 1 && X(target) > X(self))))
                input.KeyAttack = 1;
            if (closeOrFree && Rand(2 * (ai.Rand5 + 10)) < 3 && Rand(20) < 3 && targetState != 14) input.KeyDefend = 1;
            bool selfGroup = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if ((!selfGroup || targetState == 16) && Abs(X(target) - 2 * (int)self.Runtime.Vx - X(self)) < 50 &&
                Abs(Z(target) - Z(self)) < 5 && Rand(ai.Rand3 + 3) == 0 && targetState != 14) input.KeyJump = 1;

            AiProcessSubCallerPrewrite(self, target, ai, selfState, targetState);
            AiProcessSubLabel435PressurePrewrite(self, target, ai, selfState, targetState);
            AiProcessSubHelper(self, target, ai, targetState, specialLeft, specialRight);
            input.ApplyInputEdges();
        }

        private AiInputContext CreateAiInputContext(LF2Entity self, int tickIndex)
        {
            int inputPhase = InputPhase;
            int difficulty = Difficulty;
            bool forceZero = AiPhaseGate == 1;
            if (!forceZero && inputPhase == 1 && Team(self) != 5)
                forceZero = Slot(self) < 20 || self.ObjectId < 30;
            if (forceZero || difficulty < 0) difficulty = 0;
            AiInputContext ai = new AiInputContext
            {
                Difficulty = difficulty,
                Rand3 = difficulty * 3,
                Rand5 = difficulty * 5,
                Rand15 = difficulty * 15,
                Rand20 = difficulty * 20,
                InputPhase = inputPhase,
                StageTargetX = Runtime?.Stage?.XMaxOverride > 0 ? Runtime.Stage.XMaxOverride : (Runtime?.Stage?.StageWidthPx ?? 800),
            };
            AiUpdateMoveModeScan(self, ref ai);
            if (Runtime?.Flow != null)
            {
                Runtime.Flow.AiDifficulty = ai.Difficulty;
                Runtime.Flow.AiRand3 = ai.Rand3;
                Runtime.Flow.AiRand5 = ai.Rand5;
                Runtime.Flow.AiRand15 = ai.Rand15;
                Runtime.Flow.AiRand20 = ai.Rand20;
                Runtime.Flow.AiMoveMode = ai.MoveMode;
                Runtime.Flow.AiStageTargetX = ai.StageTargetX;
            }
            return ai;
        }

        private AiInputContext CreateCoordinateAiInputContext()
        {
            BattleFlowRuntimeState flow = Runtime?.Flow;
            return new AiInputContext
            {
                Difficulty = flow?.AiDifficulty ?? 0,
                Rand3 = flow?.AiRand3 ?? 0,
                Rand5 = flow?.AiRand5 ?? 0,
                Rand15 = flow?.AiRand15 ?? 0,
                Rand20 = flow?.AiRand20 ?? 0,
                MoveMode = flow?.AiMoveMode ?? 0,
                StageTargetX = flow?.AiStageTargetX ?? (Runtime?.Stage?.StageWidthPx ?? 800),
                InputPhase = InputPhase,
            };
        }

        private int StageZMin => Runtime?.Stage?.ZMin ?? 180;
        private int StageZMax => Runtime?.Stage?.ZMax ?? 350;
        private int Rand(int modulus) => Rng.NextRaw() % Math.Max(1, modulus);
        private LF2Entity AiAt(int slot) => slot >= 0 && slot < aiInputSlots.Length ? aiInputSlots[slot] : null;
        private static int X(LF2Entity e) => e.Runtime.XInt;
        private static int Y(LF2Entity e) => e.Runtime.YInt;
        private static int Z(LF2Entity e) => e.Runtime.ZInt;
        private static int Hp(LF2Entity e) => e.Runtime.HP;
        private static int Hp3(LF2Entity e) => e.Runtime.HP3;
        private static int HpMax(LF2Entity e) => e.Runtime.HPBound;
        private static int Pp(LF2Entity e) => e.Runtime.PP;
        private static int Team(LF2Entity e) => e.Runtime.RelationTeam;
        private static int Slot(LF2Entity e) => e.Runtime.SlotIndex;
        private static int Frame(LF2Entity e) => e.Runtime.Frame;
        private static int State(LF2Entity e) => e.GetState();
        private static int Facing(LF2Entity e) => e.Runtime.Dir == "left" ? 1 : 0;
        private static int Abs(int value) => Math.Abs(value);
        private static int Distance(LF2Entity a, LF2Entity b) => Abs(X(b) - X(a)) + Abs(Z(b) - Z(a));
        private static bool IsCharacterDat(LF2Entity e) => e != null && e.GetCurrentDataObjectTypeForSimulation() == 0;
        private static bool IsLivingCharacterDat(LF2Entity e) => IsCharacterDat(e) && Hp(e) > 0;

        private int FindNearestAiTargetSlot(LF2Entity self, AiInputContext ai, out int bestDist, out bool sameZLane)
        {
            if (ForceFullAiNearestScanForDiagnostics)
                return FindNearestAiTargetSlotBrute(self, ai, out bestDist, out sameZLane);

            if (ai.InputPhase == 1 &&
                Team(self) != 5 &&
                aiPhase1TargetSlotsValid &&
                !ForceFullAiPhase1TargetScanForDiagnostics)
            {
                BattleAiInputDetailDiagnostics diagnostics =
                    ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
                try
                {
                    return FindNearestAiPhase1TargetSlotIndexed(self, out bestDist, out sameZLane);
                }
                finally
                {
                    diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
                }
            }

            uint rngStateBefore = 0;
            ulong rngCallsBefore = 0;
            ulong inputSignatureBefore = 0;
            if (EnableAiNearestBestFirstShadowForDiagnostics)
            {
                rngStateBefore = Rng?.State ?? 0;
                rngCallsBefore = Rng?.CallCount ?? 0;
                inputSignatureBefore = CaptureAiNearestInputSignature(self?.Runtime);
            }

            bool formalSucceeded;
            int selected;
            if (ForceLegacyAiNearestQueryForDiagnostics)
            {
                formalSucceeded = TryFindNearestAiTargetSlotSpatial(
                    self,
                    ai,
                    out selected,
                    out bestDist,
                    out sameZLane);
            }
            else
            {
                formalSucceeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out selected,
                    out bestDist,
                    out sameZLane,
                    true);
            }

            if (formalSucceeded && EnableAiNearestBestFirstShadowForDiagnostics)
            {
                bool shadowSucceeded;
                int shadowSelected;
                int shadowBestDist;
                bool shadowSameZLane;
                if (ForceLegacyAiNearestQueryForDiagnostics)
                {
                    shadowSucceeded = TryFindNearestAiTargetSlotBestFirst(
                        self,
                        ai,
                        out shadowSelected,
                        out shadowBestDist,
                        out shadowSameZLane,
                        true);
                }
                else
                {
                    shadowSucceeded = TryFindNearestAiTargetSlotSpatial(
                        self,
                        ai,
                        out shadowSelected,
                        out shadowBestDist,
                        out shadowSameZLane);
                }

                uint rngStateAfter = Rng?.State ?? 0;
                ulong rngCallsAfter = Rng?.CallCount ?? 0;
                ulong inputSignatureAfter = CaptureAiNearestInputSignature(self?.Runtime);
                if (!shadowSucceeded ||
                    selected != shadowSelected ||
                    bestDist != shadowBestDist ||
                    sameZLane != shadowSameZLane ||
                    rngStateBefore != rngStateAfter ||
                    rngCallsBefore != rngCallsAfter ||
                    inputSignatureBefore != inputSignatureAfter)
                {
                    RecordAiNearestBestFirstShadowMismatch(
                        self,
                        formalSucceeded,
                        selected,
                        bestDist,
                        sameZLane,
                        shadowSucceeded,
                        shadowSelected,
                        shadowBestDist,
                        shadowSameZLane,
                        rngStateBefore,
                        rngStateAfter,
                        rngCallsBefore,
                        rngCallsAfter,
                        inputSignatureBefore,
                        inputSignatureAfter);
                }
            }

            if (formalSucceeded)
                return selected;

            ActiveBattleAiInputDetailDiagnosticsForDiagnostics?.RecordBruteFallback();
            return FindNearestAiTargetSlotBrute(self, ai, out bestDist, out sameZLane);
        }

        private int FindNearestAiPhase1TargetSlotIndexed(
            LF2Entity self,
            out int bestDist,
            out bool sameZLane)
        {
            int selected = -1;
            bestDist = 10000;
            sameZLane = false;
            ActiveBattleAiInputDetailDiagnosticsForDiagnostics?.RecordPhase1ListVisits(
                aiPhase1TargetSlots.Count);
            for (int index = 0; index < aiPhase1TargetSlots.Count; index++)
            {
                int slot = aiPhase1TargetSlots[index];
                LF2Entity candidate = AiAt(slot);
                if (!IsGroundAiTargetCandidate(self, candidate, 1))
                    continue;

                int dist = Distance(self, candidate);
                if (IsBetterAiTargetCandidate(dist, slot, bestDist, selected))
                {
                    bestDist = dist;
                    selected = slot;
                }
            }

            sameZLane = selected >= 0 && Abs(Z(AiAt(selected)) - Z(self)) < 15;
            if (State(self) == 9)
                return selected;

            int bestAirDist = 10000;
            int airSelectedSlot = -1;
            RecordAiNearestAirPassForSelfCheck();
            ActiveBattleAiInputDetailDiagnosticsForDiagnostics?.RecordPhase1ListVisits(
                aiPhase1TargetSlots.Count);
            for (int index = 0; index < aiPhase1TargetSlots.Count; index++)
            {
                int slot = aiPhase1TargetSlots[index];
                LF2Entity candidate = AiAt(slot);
                if (!IsAirAiTargetCandidate(self, candidate, 1))
                    continue;

                int dist = Distance(self, candidate);
                if (!IsBetterAiTargetCandidate(dist, slot, bestAirDist, airSelectedSlot) ||
                    Abs(Z(candidate) - Z(self)) >= 40 || Abs(X(candidate) - X(self)) >= 250)
                    continue;

                bestAirDist = dist;
                airSelectedSlot = slot;
            }

            if (airSelectedSlot >= 0)
                selected = airSelectedSlot;
            return selected;
        }

        private bool TryFindNearestAiTargetSlotBestFirst(
            LF2Entity self,
            AiInputContext ai,
            out int selected,
            out int bestDist,
            out bool sameZLane,
            bool allowAirRoleFastPath = false)
        {
            selected = -1;
            bestDist = 10000;
            sameZLane = false;
            if (!aiInputSpatialReady && !aiInputGroundSpatialReady)
                return false;

            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
            try
            {
                var filter = new AiNearestPointFilter
                {
                    World = this,
                    Self = self,
                    InputPhase = ai.InputPhase,
                    Air = false,
                };
                bool partitionSucceeded =
                    TryFindNearestGroundInSingleAllowedTeamPartition(
                        self,
                        ai.InputPhase,
                        ref filter,
                        diagnostics,
                        out bool partitionHandled,
                        out RuntimeEntityHandle nearestHandle,
                        out bestDist,
                        out _);
                bool succeeded = partitionHandled && partitionSucceeded;
                if (!partitionHandled)
                {
                    diagnostics?.RecordSpatialQuery();
                    LooseQuadtreeBroadphase groundBroadphase =
                        aiInputGroundSpatialReady
                            ? aiInputGroundSpatialBroadphase
                            : aiInputSpatialBroadphase;
                    succeeded = groundBroadphase.TryFindNearestPointManhattan(
                        X(self),
                        Z(self),
                        10000,
                        10000,
                        10000,
                        ref filter,
                        out nearestHandle,
                        out bestDist,
                        out int visitedRecords);
                    diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                    diagnostics?.RecordCandidateVisits(visitedRecords);
                    if (!succeeded &&
                        ReferenceEquals(
                            groundBroadphase,
                            aiInputGroundSpatialBroadphase))
                    {
                        aiInputGroundSpatialBroadphase.ResetIncremental();
                        aiInputGroundSpatialReady = false;
                        if (!aiInputSpatialReady)
                            return false;

                        diagnostics?.RecordSpatialQuery();
                        succeeded =
                            aiInputSpatialBroadphase.TryFindNearestPointManhattan(
                                X(self),
                                Z(self),
                                10000,
                                10000,
                                10000,
                                ref filter,
                                out nearestHandle,
                                out bestDist,
                                out visitedRecords);
                        diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                        diagnostics?.RecordCandidateVisits(visitedRecords);
                    }
                }
                if (!succeeded)
                {
                    aiInputSpatialBroadphase.ResetIncremental();
                    aiInputSpatialReady = false;
                    return false;
                }
                selected = nearestHandle.Slot;
            }
            catch
            {
                if (aiInputGroundSpatialReady)
                {
                    aiInputGroundSpatialBroadphase.ResetIncremental();
                    aiInputGroundSpatialReady = false;
                    return TryFindNearestAiTargetSlotBestFirst(
                        self,
                        ai,
                        out selected,
                        out bestDist,
                        out sameZLane,
                        allowAirRoleFastPath);
                }
                else
                {
                    aiInputSpatialBroadphase.ResetIncremental();
                    aiInputSpatialReady = false;
                }
                return false;
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
            }

            sameZLane = selected >= 0 && Abs(Z(AiAt(selected)) - Z(self)) < 15;
            if (State(self) == 9)
                return true;
            if (allowAirRoleFastPath &&
                aiInputAirRoleCountValid &&
                aiInputAirRoleCount == 0)
            {
                return true;
            }

            RecordAiNearestAirPassForSelfCheck();
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestAir);
            try
            {
                var filter = new AiNearestPointFilter
                {
                    World = this,
                    Self = self,
                    InputPhase = ai.InputPhase,
                    Air = true,
                };
                diagnostics?.RecordSpatialQuery();
                LooseQuadtreeBroadphase airBroadphase = aiInputAirSpatialReady
                    ? aiInputAirSpatialBroadphase
                    : aiInputSpatialBroadphase;
                bool succeeded = airBroadphase.TryFindNearestPointManhattan(
                    X(self),
                    Z(self),
                    10000,
                    250,
                    40,
                    ref filter,
                    out RuntimeEntityHandle nearestAirHandle,
                    out _,
                    out int visitedRecords);
                diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                diagnostics?.RecordCandidateVisits(visitedRecords);
                if (!succeeded && ReferenceEquals(airBroadphase, aiInputAirSpatialBroadphase))
                {
                    ResetAiAirSpatialIndex();
                    diagnostics?.RecordSpatialQuery();
                    succeeded = aiInputSpatialBroadphase.TryFindNearestPointManhattan(
                        X(self),
                        Z(self),
                        10000,
                        250,
                        40,
                        ref filter,
                        out nearestAirHandle,
                        out _,
                        out visitedRecords);
                    diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                    diagnostics?.RecordCandidateVisits(visitedRecords);
                }
                if (!succeeded)
                {
                    aiInputSpatialBroadphase.ResetIncremental();
                    aiInputSpatialReady = false;
                    return false;
                }
                if (nearestAirHandle.IsValid)
                    selected = nearestAirHandle.Slot;
            }
            catch
            {
                if (aiInputAirSpatialReady)
                {
                    ResetAiAirSpatialIndex();
                    return TryFindNearestAiTargetSlotBestFirst(
                        self,
                        ai,
                        out selected,
                        out bestDist,
                        out sameZLane,
                        allowAirRoleFastPath);
                }
                else
                {
                    aiInputSpatialBroadphase.ResetIncremental();
                    aiInputSpatialReady = false;
                    return false;
                }
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestAir);
            }
            return true;
        }

        private bool TryFindNearestGroundInSingleAllowedTeamPartition(
            LF2Entity self,
            int inputPhase,
            ref AiNearestPointFilter filter,
            BattleAiInputDetailDiagnostics diagnostics,
            out bool handled,
            out RuntimeEntityHandle nearestHandle,
            out int nearestDistance,
            out int visitedRecords)
        {
            handled = false;
            nearestHandle = RuntimeEntityHandle.Invalid;
            nearestDistance = 10000;
            visitedRecords = 0;
            if (!aiInputGroundTeamPartitionsValid)
                return false;

            int allowedPartitionCount = CountAllowedGroundTeamPartitions(
                Team(self),
                inputPhase,
                out AiGroundTeamPartition allowedPartition);
            if (allowedPartitionCount > 1)
                return false;

            handled = true;
            if (allowedPartition == null)
                return true;

            try
            {
                diagnostics?.RecordSpatialQuery();
                bool succeeded =
                    allowedPartition.Broadphase.TryFindNearestPointManhattan(
                        X(self),
                        Z(self),
                        10000,
                        10000,
                        10000,
                        ref filter,
                        out nearestHandle,
                        out nearestDistance,
                        out visitedRecords);
                diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                diagnostics?.RecordCandidateVisits(visitedRecords);
                if (succeeded)
                    return true;
            }
            catch
            {
            }

            InvalidateAiGroundTeamPartitions();
            handled = false;
            nearestHandle = RuntimeEntityHandle.Invalid;
            nearestDistance = 10000;
            visitedRecords = 0;
            return false;
        }

        private static bool IsGroundTeamPartitionAllowed(
            int selfTeam,
            int candidateTeam,
            int inputPhase)
        {
            if (inputPhase != 1)
                return candidateTeam != selfTeam;
            return selfTeam == 5
                ? candidateTeam != 5
                : candidateTeam == 5;
        }

        private int CountAllowedGroundTeamPartitions(
            int selfTeam,
            int inputPhase,
            out AiGroundTeamPartition singlePartition)
        {
            int count = 0;
            singlePartition = null;
            for (int index = 0;
                 index < aiInputActiveGroundTeamPartitions.Count;
                 index++)
            {
                AiGroundTeamPartition partition =
                    aiInputActiveGroundTeamPartitions[index];
                if (!IsGroundTeamPartitionAllowed(
                        selfTeam,
                        partition.Team,
                        inputPhase))
                {
                    continue;
                }

                count++;
                if (count == 1)
                    singlePartition = partition;
                else
                    singlePartition = null;
            }
            return count;
        }

        private bool TryFindNearestAiTargetSlotSpatial(
            LF2Entity self,
            AiInputContext ai,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            selected = -1;
            bestDist = 10000;
            sameZLane = false;
            int radius = 64;
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
            try
            {
                while (radius <= 10000)
                {
                    int boundedRadius = Math.Min(radius, 9999);
                    diagnostics?.RecordRadius(boundedRadius);
                    SpatialAabbXZ bounds = AroundAiPoint(self, boundedRadius, boundedRadius);
                    if (!TryQueryAiInputSlots(bounds, out List<int> slots))
                        return false;

                    diagnostics?.RecordCandidateVisits(slots.Count);
                    for (int index = 0; index < slots.Count; index++)
                    {
                        int slot = slots[index];
                        LF2Entity candidate = AiAt(slot);
                        if (!IsGroundAiTargetCandidate(self, candidate, ai.InputPhase))
                            continue;
                        int dist = Distance(self, candidate);
                        if (IsBetterAiTargetCandidate(dist, slot, bestDist, selected))
                        {
                            bestDist = dist;
                            selected = slot;
                        }
                    }

                    if (bestDist <= boundedRadius || boundedRadius == 9999)
                        break;
                    diagnostics?.RecordRadiusExpansion();
                    radius = radius > 4999 ? 10000 : radius * 2;
                }
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
            }

            sameZLane = selected >= 0 && Abs(Z(AiAt(selected)) - Z(self)) < 15;
            if (State(self) == 9)
                return true;

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestAir);
            RecordAiNearestAirPassForSelfCheck();
            try
            {
                if (!TryQueryAiInputSlots(AroundAiPoint(self, 249, 39), out List<int> airSlots))
                    return false;

                int bestAirDist = 10000;
                int airSelectedSlot = -1;
                diagnostics?.RecordCandidateVisits(airSlots.Count);
                for (int index = 0; index < airSlots.Count; index++)
                {
                    int slot = airSlots[index];
                    LF2Entity candidate = AiAt(slot);
                    if (!IsAirAiTargetCandidate(self, candidate, ai.InputPhase))
                        continue;
                    int dist = Distance(self, candidate);
                    if (!IsBetterAiTargetCandidate(dist, slot, bestAirDist, airSelectedSlot) ||
                        Abs(Z(candidate) - Z(self)) >= 40 || Abs(X(candidate) - X(self)) >= 250)
                        continue;
                    bestAirDist = dist;
                    airSelectedSlot = slot;
                }
                if (airSelectedSlot >= 0)
                    selected = airSelectedSlot;
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestAir);
            }
            return true;
        }

        private int FindNearestAiTargetSlotBrute(LF2Entity self, AiInputContext ai, out int bestDist, out bool sameZLane)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
            diagnostics?.RecordBruteSlotVisits(aiInputSlots.Length);
            int selected = FindNearestGroundAiTargetSlotBrute(
                self,
                ai.InputPhase,
                out bestDist);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);

            sameZLane = selected >= 0 && Abs(Z(AiAt(selected)) - Z(self)) < 15;
            if (State(self) != 9)
            {
                RecordAiNearestAirPassForSelfCheck();
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestAir);
                diagnostics?.RecordBruteSlotVisits(aiInputSlots.Length);
                int bestAirDist = 10000;
                int airSelectedSlot = -1;
                for (int i = 0; i < aiInputSlots.Length; i++)
                {
                    LF2Entity candidate = AiAt(i);
                    if (!IsAirAiTargetCandidate(self, candidate, ai.InputPhase))
                        continue;
                    int dist = Distance(self, candidate);
                    if (!IsBetterAiTargetCandidate(dist, i, bestAirDist, airSelectedSlot) ||
                        Abs(Z(candidate) - Z(self)) >= 40 || Abs(X(candidate) - X(self)) >= 250)
                    {
                        continue;
                    }
                    bestAirDist = dist;
                    airSelectedSlot = i;
                }
                if (airSelectedSlot >= 0)
                    selected = airSelectedSlot;
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestAir);
            }
            return selected;
        }

        private int FindNearestGroundAiTargetSlotBrute(
            LF2Entity self,
            int inputPhase,
            out int bestDist)
        {
            int selected = -1;
            bestDist = 10000;
            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                LF2Entity candidate = AiAt(slot);
                if (!IsGroundAiTargetCandidate(self, candidate, inputPhase))
                    continue;

                int distance = Distance(self, candidate);
                if (IsBetterAiTargetCandidate(distance, slot, bestDist, selected))
                {
                    bestDist = distance;
                    selected = slot;
                }
            }
            return selected;
        }

        private static bool IsBetterAiTargetCandidate(
            int candidateDistance,
            int candidateSlot,
            int bestDistance,
            int selectedSlot)
        {
            return candidateDistance < bestDistance ||
                   (candidateDistance == bestDistance &&
                    selectedSlot >= 0 &&
                    candidateSlot < selectedSlot);
        }

        private static bool IsGroundAiTargetCandidate(LF2Entity self, LF2Entity candidate, int inputPhase)
        {
            if (candidate == null || candidate == self)
                return false;
            int state = State(candidate);
            if (!IsCharacterDat(candidate))
            {
                if (state != 3000)
                    return false;
                if (X(candidate) > X(self))
                {
                    if (!(candidate.Runtime.Vx < 0.001))
                        return false;
                }
                else if (X(candidate) < X(self))
                {
                    if (!(candidate.Runtime.Vx > 0.001))
                        return false;
                }
                else
                {
                    return false;
                }
            }
            return TeamCandidateAllowed(self, candidate, inputPhase) &&
                   Hp(candidate) > 0 &&
                   state != 14 &&
                   Abs(Y(candidate)) <= 2;
        }

        private static bool IsAirAiTargetCandidate(LF2Entity self, LF2Entity candidate, int inputPhase)
        {
            return candidate != null &&
                   candidate != self &&
                   TeamCandidateAllowed(self, candidate, inputPhase) &&
                   Hp(candidate) > 0 &&
                   (State(candidate) == 14 || Abs(Y(candidate)) > 2);
        }

        private static bool IsAirAiSpatialRole(LF2Entity candidate)
        {
            return candidate != null &&
                   (State(candidate) == 14 || Abs(Y(candidate)) > 2);
        }

        private static bool IsGroundAiSpatialRole(LF2Entity candidate)
        {
            if (candidate == null || State(candidate) == 14 || Abs(Y(candidate)) > 2)
                return false;

            return IsCharacterDat(candidate) || State(candidate) == 3000;
        }

        private static SpatialAabbXZ AroundAiPoint(LF2Entity entity, int radiusX, int radiusZ)
        {
            int x = X(entity);
            int z = Z(entity);
            return new SpatialAabbXZ(
                SaturatingAdd(x, -radiusX),
                SaturatingAdd(z, -radiusZ),
                SaturatingAdd(x, radiusX + 1),
                SaturatingAdd(z, radiusZ + 1));
        }

        private static int SaturatingAdd(int value, int delta)
        {
            long result = (long)value + delta;
            if (result < int.MinValue)
                return int.MinValue;
            return result > int.MaxValue ? int.MaxValue : (int)result;
        }

        private static ulong CaptureAiNearestInputSignature(NTSDEntityRuntime input)
        {
            if (input == null)
                return 0;

            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            unchecked
            {
                hash = (hash ^ input.KeyUp) * prime;
                hash = (hash ^ input.KeyDown) * prime;
                hash = (hash ^ input.KeyLeft) * prime;
                hash = (hash ^ input.KeyRight) * prime;
                hash = (hash ^ input.KeyAttack) * prime;
                hash = (hash ^ input.KeyJump) * prime;
                hash = (hash ^ input.KeyDefend) * prime;
                hash = (hash ^ input.PrevUp) * prime;
                hash = (hash ^ input.PrevDown) * prime;
                hash = (hash ^ input.PrevLeft) * prime;
                hash = (hash ^ input.PrevRight) * prime;
                hash = (hash ^ input.PrevAttack) * prime;
                hash = (hash ^ input.PrevJump) * prime;
                hash = (hash ^ input.PrevDefend) * prime;
                hash = (hash ^ input.CdUp) * prime;
                hash = (hash ^ input.CdDown) * prime;
                hash = (hash ^ input.CdLeft) * prime;
                hash = (hash ^ input.CdRight) * prime;
                hash = (hash ^ input.CdAttack) * prime;
                hash = (hash ^ input.CdJump) * prime;
                hash = (hash ^ input.CdDefend) * prime;
                hash = (hash ^ (uint)input.Unk360) * prime;
                if (input.InputHistory != null)
                {
                    for (int i = 0; i < input.InputHistory.Length; i++)
                        hash = (hash ^ (uint)input.InputHistory[i]) * prime;
                }
            }
            return hash;
        }

        private void RecordAiNearestBestFirstShadowMismatch(
            LF2Entity self,
            bool formalSucceeded,
            int formalSelected,
            int formalBestDist,
            bool formalSameZLane,
            bool shadowSucceeded,
            int shadowSelected,
            int shadowBestDist,
            bool shadowSameZLane,
            uint rngStateBefore,
            uint rngStateAfter,
            ulong rngCallsBefore,
            ulong rngCallsAfter,
            ulong inputSignatureBefore,
            ulong inputSignatureAfter)
        {
            AiNearestBestFirstShadowMismatchCountForDiagnostics++;
            if (AiNearestBestFirstFirstShadowMismatchForDiagnostics != null)
                return;

            AiNearestBestFirstFirstShadowMismatchForDiagnostics =
                $"slot={Slot(self)} formal={formalSucceeded}:{formalSelected}/{formalBestDist}/{formalSameZLane} " +
                $"shadow={shadowSucceeded}:{shadowSelected}/{shadowBestDist}/{shadowSameZLane} " +
                $"rng={rngStateBefore}:{rngCallsBefore}->{rngStateAfter}:{rngCallsAfter} " +
                $"input={inputSignatureBefore}->{inputSignatureAfter}";
        }

        internal bool AiNearestSpatialMatchesBruteForSelfCheck(LF2Entity self, int inputPhase)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                var ai = new AiInputContext { InputPhase = inputPhase };
                bool bestFirstSucceeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out int bestFirstSlot,
                    out int bestFirstDistance,
                    out bool bestFirstSameZ);
                bool spatialSucceeded = TryFindNearestAiTargetSlotSpatial(
                    self,
                    ai,
                    out int spatialSlot,
                    out int spatialDistance,
                    out bool spatialSameZ);
                int bruteSlot = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                return spatialSucceeded &&
                       bestFirstSucceeded &&
                       spatialSlot == bruteSlot &&
                       spatialDistance == bruteDistance &&
                       spatialSameZ == bruteSameZ &&
                       bestFirstSlot == bruteSlot &&
                       bestFirstDistance == bruteDistance &&
                       bestFirstSameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiSnapshotIndexProductsMatchLegacyForSelfCheck()
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiPhase1TargetSlotsValid || !aiTeamHpSummaryValid)
                    return false;

                int specialIndex = 0;
                int phase1Index = 0;
                var expectedSummaries =
                    new Dictionary<int, AiTeamHpSummary>(8);
                for (int slot = 0; slot < aiInputSlots.Length; slot++)
                {
                    LF2Entity entity = aiInputSlots[slot];
                    if (slot >= 20 &&
                        entity != null &&
                        IsAiSpecialScanObjectId(entity.ObjectId))
                    {
                        if (specialIndex >= aiSpecialScanSlots.Count ||
                            aiSpecialScanSlots[specialIndex] != slot)
                        {
                            return false;
                        }
                        specialIndex++;
                    }

                    if (entity != null && Team(entity) == 5)
                    {
                        if (phase1Index >= aiPhase1TargetSlots.Count ||
                            aiPhase1TargetSlots[phase1Index] != slot)
                        {
                            return false;
                        }
                        phase1Index++;
                    }

                    if (!IsLivingCharacterDat(entity))
                        continue;

                    int team = Team(entity);
                    expectedSummaries.TryGetValue(
                        team,
                        out AiTeamHpSummary summary);
                    summary.Add(Hp(entity));
                    expectedSummaries[team] = summary;
                }

                if (specialIndex != aiSpecialScanSlots.Count ||
                    phase1Index != aiPhase1TargetSlots.Count ||
                    expectedSummaries.Count != aiTeamHpSummaries.Count)
                {
                    return false;
                }

                foreach (KeyValuePair<int, AiTeamHpSummary> pair
                         in expectedSummaries)
                {
                    if (!aiTeamHpSummaries.TryGetValue(
                            pair.Key,
                            out AiTeamHpSummary actual) ||
                        actual.Count != pair.Value.Count ||
                        actual.MinHp != pair.Value.MinHp ||
                        actual.MinCount != pair.Value.MinCount ||
                        actual.SecondMinHp != pair.Value.SecondMinHp)
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiMoveModeSnapshotMatchesFullForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out bool snapshotValid,
            out int topSlot,
            out int secondSlot,
            out int snapshotMoveMode,
            out int fullMoveMode)
        {
            snapshotValid = false;
            topSlot = -1;
            secondSlot = -1;
            snapshotMoveMode = 0;
            fullMoveMode = 0;
            BuildAiInputSlotSnapshot();
            bool previousForceFull = ForceFullAiMoveModeScanForDiagnostics;
            try
            {
                snapshotValid = aiMoveModeFirst10Valid;
                topSlot = aiMoveModeTopSlot;
                secondSlot = aiMoveModeSecondSlot;

                var snapshotContext =
                    new AiInputContext { InputPhase = inputPhase };
                ForceFullAiMoveModeScanForDiagnostics = false;
                AiUpdateMoveModeScan(self, ref snapshotContext);
                snapshotMoveMode = snapshotContext.MoveMode;

                var fullContext = new AiInputContext { InputPhase = inputPhase };
                ForceFullAiMoveModeScanForDiagnostics = true;
                AiUpdateMoveModeScan(self, ref fullContext);
                fullMoveMode = fullContext.MoveMode;
                return snapshotValid && snapshotMoveMode == fullMoveMode;
            }
            finally
            {
                ForceFullAiMoveModeScanForDiagnostics = previousForceFull;
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiMoveModeValueMutationFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int candidateHp,
            int candidateX,
            int candidateZ,
            out bool snapshotValid,
            out int fallbackMoveMode,
            out int fullMoveMode)
        {
            snapshotValid = true;
            fallbackMoveMode = 0;
            fullMoveMode = 0;
            BuildAiInputSlotSnapshot();
            bool previousForceFull = ForceFullAiMoveModeScanForDiagnostics;
            try
            {
                if (!aiMoveModeFirst10Valid)
                    return false;

                candidate.Runtime.HP = candidateHp;
                candidate.Runtime.X = candidateX;
                candidate.Runtime.XInt = candidateX;
                candidate.Runtime.Z = candidateZ;
                candidate.Runtime.ZInt = candidateZ;
                ObserveAiTeamHpSummaryMutation(candidate);
                snapshotValid = aiMoveModeFirst10Valid;
                if (snapshotValid)
                    return false;

                var fallbackContext =
                    new AiInputContext { InputPhase = 1 };
                ForceFullAiMoveModeScanForDiagnostics = false;
                AiUpdateMoveModeScan(self, ref fallbackContext);
                fallbackMoveMode = fallbackContext.MoveMode;

                var fullContext = new AiInputContext { InputPhase = 1 };
                ForceFullAiMoveModeScanForDiagnostics = true;
                AiUpdateMoveModeScan(self, ref fullContext);
                fullMoveMode = fullContext.MoveMode;
                return fallbackMoveMode == fullMoveMode;
            }
            finally
            {
                ForceFullAiMoveModeScanForDiagnostics = previousForceFull;
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiMoveModeIdentityMutationFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            LF2Entity replacement,
            out bool snapshotValid,
            out int fallbackMoveMode,
            out int fullMoveMode)
        {
            snapshotValid = true;
            fallbackMoveMode = 0;
            fullMoveMode = 0;
            BuildAiInputSlotSnapshot();
            bool previousForceFull = ForceFullAiMoveModeScanForDiagnostics;
            try
            {
                if (!aiMoveModeFirst10Valid ||
                    candidate?.Runtime == null)
                {
                    return false;
                }

                int slot = Slot(candidate);
                Unregister(candidate);
                LF2Entity current = replacement ?? candidate;
                current.SetRequiredRuntimeSlot(slot);
                Register(current);
                ObserveAiTeamHpSummaryMutation(current);
                snapshotValid = aiMoveModeFirst10Valid;
                if (snapshotValid)
                    return false;

                var fallbackContext =
                    new AiInputContext { InputPhase = 1 };
                ForceFullAiMoveModeScanForDiagnostics = false;
                AiUpdateMoveModeScan(self, ref fallbackContext);
                fallbackMoveMode = fallbackContext.MoveMode;

                var fullContext = new AiInputContext { InputPhase = 1 };
                ForceFullAiMoveModeScanForDiagnostics = true;
                AiUpdateMoveModeScan(self, ref fullContext);
                fullMoveMode = fullContext.MoveMode;
                return fallbackMoveMode == fullMoveMode;
            }
            finally
            {
                ForceFullAiMoveModeScanForDiagnostics = previousForceFull;
                ClearAiInputSlotSnapshot();
            }
        }

        internal long MeasureAiMoveModeSnapshotAllocationsForSelfCheck(
            LF2Entity self,
            int iterations)
        {
            BuildAiInputSlotSnapshot();
            bool previousForceFull = ForceFullAiMoveModeScanForDiagnostics;
            try
            {
                if (!aiMoveModeFirst10Valid)
                    return -1;

                ForceFullAiMoveModeScanForDiagnostics = false;
                for (int index = 0; index < 16; index++)
                {
                    var context = new AiInputContext { InputPhase = 1 };
                    AiUpdateMoveModeScan(self, ref context);
                }

                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int index = 0; index < iterations; index++)
                {
                    var context = new AiInputContext { InputPhase = 1 };
                    AiUpdateMoveModeScan(self, ref context);
                }
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                ForceFullAiMoveModeScanForDiagnostics = previousForceFull;
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiAirRoleMutationMatchesBruteForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int candidateY)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                candidate.Runtime.Y = candidateY;
                candidate.Runtime.YInt = candidateY;
                ObserveAiAirSpatialRoleMutation(candidate);
                if (!aiInputAirSpatialReady)
                    return false;

                var ai = new AiInputContext { InputPhase = inputPhase };
                bool bestFirstSucceeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out int bestFirstSlot,
                    out int bestFirstDistance,
                    out bool bestFirstSameZ);
                int bruteSlot = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                return bestFirstSucceeded &&
                       bestFirstSlot == bruteSlot &&
                       bestFirstDistance == bruteDistance &&
                       bestFirstSameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiAirRoleCountMutationForSelfCheck(
            LF2Entity candidate,
            int airState,
            int airY,
            int groundState,
            int groundY,
            out int initialCount,
            out int airCount,
            out int groundCount)
        {
            initialCount = -1;
            airCount = -1;
            groundCount = -1;
            BuildAiInputSlotSnapshot();
            try
            {
                initialCount = aiInputAirRoleCount;
                if (!aiInputAirRoleCountValid)
                    return false;

                candidate.Runtime.Y = airY;
                candidate.Runtime.YInt = airY;
                if (candidate.Frame?.D != null)
                    candidate.Frame.D.state = airState;
                ObserveAiAirSpatialRoleMutation(candidate);
                airCount = aiInputAirRoleCount;
                if (!aiInputAirRoleCountValid)
                    return false;

                candidate.Runtime.Y = groundY;
                candidate.Runtime.YInt = groundY;
                if (candidate.Frame?.D != null)
                    candidate.Frame.D.state = groundState;
                ObserveAiAirSpatialRoleMutation(candidate);
                groundCount = aiInputAirRoleCount;
                return aiInputAirRoleCountValid;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiAirNullMutationInvalidatesCountForSelfCheck()
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputAirRoleCountValid)
                    return false;
                ObserveAiAirSpatialRoleMutation(null);
                return !aiInputAirRoleCountValid;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiAirInvalidCoordinateInvalidatesCountForSelfCheck(
            LF2Entity candidate,
            out int count,
            out bool valid)
        {
            if (candidate?.Runtime == null)
            {
                count = -1;
                valid = false;
                return false;
            }

            candidate.Runtime.X = int.MaxValue;
            candidate.Runtime.XInt = int.MaxValue;
            BuildAiInputSlotSnapshot();
            try
            {
                count = aiInputAirRoleCount;
                valid = aiInputAirRoleCountValid;
                return count == 0 && !valid;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiAirFastPathMatchesOracleForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool invalidateAirSnapshot,
            out int snapshotCount,
            out bool snapshotValid,
            out int fastAirPassCount,
            out int oracleAirPassCount)
        {
            snapshotCount = -1;
            snapshotValid = false;
            fastAirPassCount = -1;
            oracleAirPassCount = -1;
            BuildAiInputSlotSnapshot();
            try
            {
                snapshotCount = aiInputAirRoleCount;
                snapshotValid = aiInputAirRoleCountValid;
                if (invalidateAirSnapshot)
                    InvalidateAiAirRoleSnapshot();

                var ai = new AiInputContext { InputPhase = inputPhase };
                int passBefore = AiNearestAirPassCountForDiagnostics;
                bool fastSucceeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out int fastSlot,
                    out int fastDistance,
                    out bool fastSameZ,
                    true);
                fastAirPassCount =
                    AiNearestAirPassCountForDiagnostics - passBefore;

                passBefore = AiNearestAirPassCountForDiagnostics;
                bool oracleSucceeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out int oracleSlot,
                    out int oracleDistance,
                    out bool oracleSameZ,
                    false);
                oracleAirPassCount =
                    AiNearestAirPassCountForDiagnostics - passBefore;

                int bruteSlot = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                return fastSucceeded &&
                       oracleSucceeded &&
                       fastSlot == oracleSlot &&
                       fastDistance == oracleDistance &&
                       fastSameZ == oracleSameZ &&
                       oracleSlot == bruteSlot &&
                       oracleDistance == bruteDistance &&
                       oracleSameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal int AiAirExecutionModePassCountForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceFull,
            bool forceLegacy,
            bool shadow)
        {
            BuildAiInputSlotSnapshot();
            bool previousFull = ForceFullAiNearestScanForDiagnostics;
            bool previousLegacy = ForceLegacyAiNearestQueryForDiagnostics;
            bool previousShadow = EnableAiNearestBestFirstShadowForDiagnostics;
            try
            {
                ForceFullAiNearestScanForDiagnostics = forceFull;
                ForceLegacyAiNearestQueryForDiagnostics = forceLegacy;
                EnableAiNearestBestFirstShadowForDiagnostics = shadow;
                int before = AiNearestAirPassCountForDiagnostics;
                var ai = new AiInputContext { InputPhase = inputPhase };
                FindNearestAiTargetSlot(self, ai, out _, out _);
                return AiNearestAirPassCountForDiagnostics - before;
            }
            finally
            {
                ForceFullAiNearestScanForDiagnostics = previousFull;
                ForceLegacyAiNearestQueryForDiagnostics = previousLegacy;
                EnableAiNearestBestFirstShadowForDiagnostics = previousShadow;
                ClearAiInputSlotSnapshot();
            }
        }

        internal long MeasureAiAirZeroFastPathAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputAirRoleCountValid || aiInputAirRoleCount != 0)
                    return -1;

                var ai = new AiInputContext { InputPhase = inputPhase };
                for (int index = 0; index < 16; index++)
                {
                    if (!TryFindNearestAiTargetSlotBestFirst(
                            self,
                            ai,
                            out _,
                            out _,
                            out _,
                            true))
                    {
                        return -1;
                    }
                }

                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int index = 0; index < iterations; index++)
                {
                    if (!TryFindNearestAiTargetSlotBestFirst(
                            self,
                            ai,
                            out _,
                            out _,
                            out _,
                            true))
                    {
                        return -1;
                    }
                }
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGroundNearestMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out int groundVisitedRecords,
            out int allVisitedRecords,
            out int groundIndexedCount,
            out int selectedSlot,
            out int selectedDistance)
        {
            groundVisitedRecords = 0;
            allVisitedRecords = 0;
            groundIndexedCount = 0;
            selectedSlot = -1;
            selectedDistance = 10000;
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputSpatialReady)
                    return false;

                LooseQuadtreeBroadphase groundBroadphase =
                    aiInputGroundSpatialReady
                        ? aiInputGroundSpatialBroadphase
                        : aiInputSpatialBroadphase;
                groundIndexedCount = aiInputGroundSpatialReady
                    ? aiInputGroundSpatialBroadphase.IncrementalIndexedCount
                    : aiInputSpatialBroadphase.IncrementalIndexedCount;
                bool groundSucceeded = TryFindNearestGroundInBroadphaseForSelfCheck(
                    groundBroadphase,
                    self,
                    inputPhase,
                    out int groundSlot,
                    out int groundDistance,
                    out groundVisitedRecords);
                bool allSucceeded = TryFindNearestGroundInBroadphaseForSelfCheck(
                    aiInputSpatialBroadphase,
                    self,
                    inputPhase,
                    out int allSlot,
                    out int allDistance,
                    out allVisitedRecords);
                int bruteSlot = FindNearestGroundAiTargetSlotBrute(
                    self,
                    inputPhase,
                    out int bruteDistance);
                selectedSlot = groundSlot;
                selectedDistance = groundDistance;
                return groundSucceeded &&
                       allSucceeded &&
                       groundSlot == bruteSlot &&
                       groundDistance == bruteDistance &&
                       allSlot == bruteSlot &&
                       allDistance == bruteDistance;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGroundTeamPartitionMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out int allowedPartitionCount,
            out bool partitionHandled,
            out int partitionVisitedRecords,
            out int groundVisitedRecords,
            out int selectedSlot,
            out int selectedDistance)
        {
            allowedPartitionCount = -1;
            partitionHandled = false;
            partitionVisitedRecords = 0;
            groundVisitedRecords = 0;
            selectedSlot = -1;
            selectedDistance = 10000;
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputSpatialReady)
                    return false;

                allowedPartitionCount = CountAllowedGroundTeamPartitions(
                    Team(self),
                    inputPhase,
                    out _);
                var filter = new AiNearestPointFilter
                {
                    World = this,
                    Self = self,
                    InputPhase = inputPhase,
                    Air = false,
                };
                bool partitionSucceeded =
                    TryFindNearestGroundInSingleAllowedTeamPartition(
                        self,
                        inputPhase,
                        ref filter,
                        null,
                        out partitionHandled,
                        out RuntimeEntityHandle partitionHandle,
                        out int partitionDistance,
                        out partitionVisitedRecords);

                LooseQuadtreeBroadphase groundBroadphase =
                    aiInputGroundSpatialReady
                        ? aiInputGroundSpatialBroadphase
                        : aiInputSpatialBroadphase;
                bool groundSucceeded =
                    TryFindNearestGroundInBroadphaseForSelfCheck(
                        groundBroadphase,
                        self,
                        inputPhase,
                        out _,
                        out _,
                        out groundVisitedRecords);

                var ai = new AiInputContext { InputPhase = inputPhase };
                bool formalSucceeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out selectedSlot,
                    out selectedDistance,
                    out bool selectedSameZ,
                    true);
                int bruteSlot = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                if (!groundSucceeded ||
                    !formalSucceeded ||
                    selectedSlot != bruteSlot ||
                    selectedDistance != bruteDistance ||
                    selectedSameZ != bruteSameZ)
                {
                    return false;
                }

                if (!partitionHandled)
                {
                    return allowedPartitionCount > 1 ||
                           !aiInputGroundTeamPartitionsValid;
                }

                int bruteGroundSlot = FindNearestGroundAiTargetSlotBrute(
                    self,
                    inputPhase,
                    out int bruteGroundDistance);
                return partitionSucceeded &&
                       partitionHandle.Slot == bruteGroundSlot &&
                       partitionDistance == bruteGroundDistance &&
                       allowedPartitionCount <= 1;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGroundTeamPartitionMutationFallbackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int candidateTeam,
            int candidateX)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputGroundTeamPartitionsValid)
                    return false;

                bool positionChanged = X(candidate) != candidateX;
                candidate.Runtime.RelationTeam = candidateTeam;
                candidate.Runtime.X = candidateX;
                candidate.Runtime.XInt = candidateX;
                ObserveAiGroundSpatialRoleMutation(candidate);
                if (aiInputGroundTeamPartitionsValid)
                    return false;

                var ai = new AiInputContext { InputPhase = inputPhase };
                bool bestFirstSucceeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out _,
                    out _,
                    out _,
                    true);
                if (positionChanged)
                {
                    if (bestFirstSucceeded ||
                        aiInputSpatialReady ||
                        aiInputGroundSpatialReady ||
                        aiInputAirSpatialReady)
                    {
                        return false;
                    }
                }
                else if (!bestFirstSucceeded || !aiInputSpatialReady)
                {
                    return false;
                }

                int selected = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out int distance,
                    out bool sameZ);
                int brute = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                return selected == brute &&
                       distance == bruteDistance &&
                       sameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGroundTeamPartitionFaultFallbackForSelfCheck(
            LF2Entity self,
            int inputPhase)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputGroundTeamPartitionsValid ||
                    CountAllowedGroundTeamPartitions(
                        Team(self),
                        inputPhase,
                        out AiGroundTeamPartition partition) != 1)
                {
                    return false;
                }

                partition.Broadphase.ResetIncremental();
                var ai = new AiInputContext { InputPhase = inputPhase };
                bool succeeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out int selected,
                    out int distance,
                    out bool sameZ,
                    true);
                int brute = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                return succeeded &&
                       !aiInputGroundTeamPartitionsValid &&
                       selected == brute &&
                       distance == bruteDistance &&
                       sameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal long MeasureAiGroundTeamPartitionAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputGroundTeamPartitionsValid ||
                    CountAllowedGroundTeamPartitions(
                        Team(self),
                        inputPhase,
                        out _) != 1)
                {
                    return -1;
                }

                var ai = new AiInputContext { InputPhase = inputPhase };
                for (int index = 0; index < 16; index++)
                {
                    if (!TryFindNearestAiTargetSlotBestFirst(
                            self,
                            ai,
                            out _,
                            out _,
                            out _,
                            true))
                    {
                        return -1;
                    }
                }

                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int index = 0; index < iterations; index++)
                {
                    if (!TryFindNearestAiTargetSlotBestFirst(
                            self,
                            ai,
                            out _,
                            out _,
                            out _,
                            true))
                    {
                        return -1;
                    }
                }
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGroundRoleMutationMatchesBruteForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int candidateX,
            int candidateY,
            int candidateZ,
            int candidateState,
            out int fullRebuildDelta,
            out int inPlaceUpdateDelta,
            out int migrationDelta)
        {
            fullRebuildDelta = 0;
            inPlaceUpdateDelta = 0;
            migrationDelta = 0;
            BuildAiInputSlotSnapshot();
            try
            {
                int rebuildBefore =
                    aiInputGroundSpatialBroadphase.IncrementalFullRebuildCount;
                int inPlaceBefore =
                    aiInputGroundSpatialBroadphase.IncrementalInPlaceUpdateCount;
                int migrationBefore =
                    aiInputGroundSpatialBroadphase.IncrementalMigrationCount;
                candidate.Runtime.SetPosition(candidateX, candidateY, candidateZ);
                candidate.Runtime.SyncIntegerPosition();
                if (candidate.Frame?.D != null)
                    candidate.Frame.D.state = candidateState;
                bool coordinatesChanged =
                    aiInputGroundXBySlot[Slot(candidate)] != candidateX ||
                    aiInputGroundZBySlot[Slot(candidate)] != candidateZ;
                ObserveAiGroundSpatialRoleMutation(candidate);

                fullRebuildDelta =
                    aiInputGroundSpatialBroadphase.IncrementalFullRebuildCount -
                    rebuildBefore;
                inPlaceUpdateDelta =
                    aiInputGroundSpatialBroadphase.IncrementalInPlaceUpdateCount -
                    inPlaceBefore;
                migrationDelta =
                    aiInputGroundSpatialBroadphase.IncrementalMigrationCount -
                    migrationBefore;
                if (coordinatesChanged)
                {
                    if (aiInputSpatialReady ||
                        aiInputGroundSpatialReady ||
                        aiInputAirSpatialReady ||
                        aiInputGroundTeamPartitionsValid)
                    {
                        return false;
                    }

                    var ai = new AiInputContext { InputPhase = inputPhase };
                    int selected = FindNearestAiTargetSlot(
                        self,
                        ai,
                        out int selectedDistance,
                        out bool selectedSameZ);
                    int brute = FindNearestAiTargetSlotBrute(
                        self,
                        ai,
                        out int fullBruteDistance,
                        out bool fullBruteSameZ);
                    return selected == brute &&
                           selectedDistance == fullBruteDistance &&
                           selectedSameZ == fullBruteSameZ;
                }

                if (!aiInputGroundSpatialReady)
                    return false;

                bool groundSucceeded = TryFindNearestGroundInBroadphaseForSelfCheck(
                    aiInputGroundSpatialBroadphase,
                    self,
                    inputPhase,
                    out int groundSlot,
                    out int groundDistance,
                    out _);
                int bruteSlot = FindNearestGroundAiTargetSlotBrute(
                    self,
                    inputPhase,
                    out int bruteDistance);
                return groundSucceeded &&
                       groundSlot == bruteSlot &&
                       groundDistance == bruteDistance;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal int RunAiGroundNearestQueriesForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputGroundSpatialReady)
                    return int.MinValue;

                int checksum = 0;
                for (int i = 0; i < iterations; i++)
                {
                    if (!TryFindNearestGroundInBroadphaseForSelfCheck(
                            aiInputGroundSpatialBroadphase,
                            self,
                            inputPhase,
                            out int selected,
                            out int distance,
                            out int visited))
                    {
                        return int.MinValue;
                    }
                    checksum = unchecked(
                        checksum * 31 + selected * 17 + distance + visited);
                }
                return checksum;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

#if UNITY_INCLUDE_TESTS
        internal bool AiNearestOccupancyMutationFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity transientEntity,
            int transientSlot,
            int inputPhase,
            bool releaseBeforeQuery,
            out bool epochChanged,
            out bool fastAborted)
        {
            epochChanged = false;
            fastAborted = false;
            BuildAiInputSlotSnapshot();
            RuntimeEntityHandle transientHandle = RuntimeEntityHandle.Invalid;
            bool transientClaimed = false;
            try
            {
                ulong snapshotEpoch = aiInputSlotSnapshotOccupancyEpoch;
                if (snapshotEpoch == 0 ||
                    !_runtimeSlots.TryClaim(
                        transientSlot,
                        transientEntity,
                        out transientHandle))
                {
                    return false;
                }

                transientClaimed = true;
                if (releaseBeforeQuery)
                {
                    if (!_runtimeSlots.Release(transientHandle))
                        return false;
                    transientClaimed = false;
                }

                epochChanged =
                    RuntimeSlotOccupancyEpochForServices != snapshotEpoch;
                return epochChanged &&
                       AiNearestFastAbortAndFallbackMatchesFullForSelfCheck(
                           self,
                           inputPhase,
                           out fastAborted);
            }
            finally
            {
                if (transientClaimed)
                    _runtimeSlots.Release(transientHandle);
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiNearestOccupancyReuseFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            LF2Entity replacement,
            int inputPhase,
            out bool generationChanged,
            out bool fastAborted)
        {
            generationChanged = false;
            fastAborted = false;
            BuildAiInputSlotSnapshot();
            int candidateSlot = Slot(candidate);
            RuntimeEntityHandle candidateHandle = RuntimeEntityHandle.Invalid;
            RuntimeEntityHandle replacementHandle = RuntimeEntityHandle.Invalid;
            bool candidateReleased = false;
            bool replacementClaimed = false;
            try
            {
                if (candidateSlot < 0 ||
                    replacement == null ||
                    ReferenceEquals(candidate, replacement) ||
                    !TryGetCurrentRuntimeHandle(
                        candidateSlot,
                        candidate,
                        out candidateHandle) ||
                    !_runtimeSlots.Release(candidateHandle))
                {
                    return false;
                }

                candidateReleased = true;
                if (!_runtimeSlots.TryClaim(
                        candidateSlot,
                        replacement,
                        out replacementHandle))
                {
                    return false;
                }

                replacementClaimed = true;
                generationChanged =
                    replacementHandle.Generation != candidateHandle.Generation;
                return generationChanged &&
                       AiNearestFastAbortAndFallbackMatchesFullForSelfCheck(
                           self,
                           inputPhase,
                           out fastAborted);
            }
            finally
            {
                if (replacementClaimed)
                {
                    _runtimeSlots.Release(replacementHandle);
                    replacementClaimed = false;
                }
                if (candidateReleased)
                    _runtimeSlots.TryClaim(candidateSlot, candidate, out _);
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiNearestGenerationMismatchFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            out bool fastAborted)
        {
            fastAborted = false;
            BuildAiInputSlotSnapshot();
            try
            {
                int candidateSlot = Slot(candidate);
                if (candidateSlot < 0 ||
                    candidateSlot >= aiInputGroundGenerationBySlot.Length ||
                    aiInputGroundGenerationBySlot[candidateSlot] == 0)
                {
                    return false;
                }

                uint generation =
                    aiInputGroundGenerationBySlot[candidateSlot];
                aiInputGroundGenerationBySlot[candidateSlot] =
                    generation == uint.MaxValue ? 1u : generation + 1u;
                return AiNearestFastAbortAndFallbackMatchesFullForSelfCheck(
                    self,
                    inputPhase,
                    out fastAborted);
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        private bool AiNearestFastAbortAndFallbackMatchesFullForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out bool fastAborted)
        {
            var ai = new AiInputContext { InputPhase = inputPhase };
            uint rngStateBefore = Rng?.State ?? 0;
            ulong rngCallsBefore = Rng?.CallCount ?? 0;
            fastAborted = !TryFindNearestAiTargetSlotBestFirst(
                self,
                ai,
                out _,
                out _,
                out _,
                true);

            bool previousForceFull = ForceFullAiNearestScanForDiagnostics;
            bool previousForceLegacy = ForceLegacyAiNearestQueryForDiagnostics;
            bool previousShadow = EnableAiNearestBestFirstShadowForDiagnostics;
            try
            {
                ForceFullAiNearestScanForDiagnostics = false;
                ForceLegacyAiNearestQueryForDiagnostics = false;
                EnableAiNearestBestFirstShadowForDiagnostics = false;
                int fallbackSlot = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out int fallbackDistance,
                    out bool fallbackSameZ);

                ForceFullAiNearestScanForDiagnostics = true;
                int fullSlot = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out int fullDistance,
                    out bool fullSameZ);
                return fastAborted &&
                       fallbackSlot == fullSlot &&
                       fallbackDistance == fullDistance &&
                       fallbackSameZ == fullSameZ &&
                       rngStateBefore == (Rng?.State ?? 0) &&
                       rngCallsBefore == (Rng?.CallCount ?? 0);
            }
            finally
            {
                ForceFullAiNearestScanForDiagnostics = previousForceFull;
                ForceLegacyAiNearestQueryForDiagnostics = previousForceLegacy;
                EnableAiNearestBestFirstShadowForDiagnostics = previousShadow;
            }
        }
#endif

        internal bool AiGroundFailClosedFallbackMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiInputGroundSpatialReady || !aiInputSpatialReady)
                    return false;

                InvalidateAiGroundTeamPartitions();
                aiInputGroundSpatialBroadphase.ResetIncremental();
                var ai = new AiInputContext { InputPhase = inputPhase };
                bool succeeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out int selected,
                    out int distance,
                    out bool sameZ);
                int brute = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                return succeeded &&
                       !aiInputGroundSpatialReady &&
                       selected == brute &&
                       distance == bruteDistance &&
                       sameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        private bool TryFindNearestGroundInBroadphaseForSelfCheck(
            LooseQuadtreeBroadphase broadphase,
            LF2Entity self,
            int inputPhase,
            out int selected,
            out int distance,
            out int visitedRecords)
        {
            var filter = new AiNearestPointFilter
            {
                World = this,
                Self = self,
                InputPhase = inputPhase,
                Air = false,
            };
            bool succeeded = broadphase.TryFindNearestPointManhattan(
                X(self),
                Z(self),
                10000,
                10000,
                10000,
                ref filter,
                out RuntimeEntityHandle nearestHandle,
                out distance,
                out visitedRecords);
            selected = nearestHandle.Slot;
            return succeeded;
        }

        internal bool AiSpecialScanSlotsMatchForSelfCheck(IReadOnlyList<int> expectedSlots)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (aiSpecialScanSlots.Count != expectedSlots.Count)
                    return false;

                for (int index = 0; index < expectedSlots.Count; index++)
                {
                    if (aiSpecialScanSlots[index] != expectedSlots[index])
                        return false;
                }

                return true;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiPhase1TargetSlotsMatchForSelfCheck(IReadOnlyList<int> expectedSlots)
        {
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiPhase1TargetSlotsValid ||
                    aiPhase1TargetSlots.Count != expectedSlots.Count)
                    return false;

                for (int index = 0; index < expectedSlots.Count; index++)
                {
                    if (aiPhase1TargetSlots[index] != expectedSlots[index])
                        return false;
                }

                return true;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiPhase1TeamMutationMatchesBruteForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int candidateTeam,
            out bool phase1ListValid,
            out int selectedSlot)
        {
            phase1ListValid = false;
            selectedSlot = -1;
            BuildAiInputSlotSnapshot();
            try
            {
                if (!aiPhase1TargetSlotsValid ||
                    Team(self) == 5 ||
                    Team(candidate) == candidateTeam)
                {
                    return false;
                }

                candidate.Runtime.RelationTeam = candidateTeam;
                ObserveAiTeamHpSummaryMutation(candidate);
                phase1ListValid = aiPhase1TargetSlotsValid;
                if (phase1ListValid)
                    return false;

                var ai = new AiInputContext { InputPhase = 1 };
                selectedSlot = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out int selectedDistance,
                    out bool selectedSameZ);
                int bruteSlot = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                return selectedSlot == bruteSlot &&
                       selectedDistance == bruteDistance &&
                       selectedSameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal void CaptureAiSameTeamDecisionForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceFullScan,
            out bool evaluated,
            out bool usedSummary,
            out int otherCount,
            out int otherMinHp,
            out bool force7AGround,
            out bool guard7A)
        {
            bool previousForceFull = ForceFullAiSameTeamScanForDiagnostics;
            ForceFullAiSameTeamScanForDiagnostics = forceFullScan;
            BuildAiInputSlotSnapshot();
            try
            {
                evaluated = (inputPhase == 1 || inputPhase == 4) && Team(self) != 5;
                usedSummary = false;
                otherCount = 0;
                otherMinHp = int.MaxValue;
                force7AGround = false;
                guard7A = false;
                if (!evaluated)
                    return;

                int selfHp = Hp(self);
                int selfHp3 = Hp3(self);
                force7AGround = true;
                if (selfHp > (4 * selfHp3) / 5 || selfHp > selfHp3 - 130)
                    force7AGround = false;
                if (selfHp > 430 || selfHp > selfHp3 - 130)
                    guard7A = true;

                usedSummary = ResolveAiSameTeamSummaryExcludingSelf(
                    self,
                    Team(self),
                    out otherCount,
                    out otherMinHp);
                if (otherMinHp < selfHp)
                    force7AGround = false;
                if (otherMinHp < selfHp - 200)
                    guard7A = true;
                if (otherCount == 0)
                    force7AGround = false;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                ForceFullAiSameTeamScanForDiagnostics = previousForceFull;
            }
        }

        internal void CaptureAiNearestTargetForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceFullPhase1Scan,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            bool previousForceFull = ForceFullAiPhase1TargetScanForDiagnostics;
            ForceFullAiPhase1TargetScanForDiagnostics = forceFullPhase1Scan;
            BuildAiInputSlotSnapshot();
            try
            {
                var ai = new AiInputContext { InputPhase = inputPhase };
                selected = FindNearestAiTargetSlot(self, ai, out bestDist, out sameZLane);
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                ForceFullAiPhase1TargetScanForDiagnostics = previousForceFull;
            }
        }

        internal string CaptureAiSpecialScanSlotsForSelfCheck()
        {
            BuildAiInputSlotSnapshot();
            try
            {
                return string.Join(",", aiSpecialScanSlots);
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        private static bool TeamCandidateAllowed(LF2Entity self, LF2Entity candidate, int inputPhase)
        {
            if (Team(candidate) != Team(self))
            {
                if (inputPhase != 1) return true;
                if (Team(self) == 5) return true;
            }
            if (Team(candidate) != 5) return false;
            if (inputPhase != 1) return false;
            return Team(candidate) != Team(self);
        }

        private void AiUpdateMoveModeScan(LF2Entity self, ref AiInputContext ai)
        {
            if (ai.InputPhase != 1 || Team(self) == 5)
                return;

            if (ForceFullAiMoveModeScanForDiagnostics ||
                !aiMoveModeFirst10Valid ||
                !IsAiMoveModeSnapshotSelfCurrent(self))
            {
                AiUpdateMoveModeScanFull(self, ref ai);
                return;
            }

            int candidateSlot = aiMoveModeTopSlot == Slot(self)
                ? aiMoveModeSecondSlot
                : aiMoveModeTopSlot;
            if (candidateSlot < 0)
                return;

            int rightmostX = candidateSlot == aiMoveModeTopSlot
                ? aiMoveModeTopX
                : aiMoveModeSecondX;
            int rightmostZ = candidateSlot == aiMoveModeTopSlot
                ? aiMoveModeTopZ
                : aiMoveModeSecondZ;
            ApplyAiMoveModeFromRightmost(self, rightmostX, rightmostZ, ref ai);
        }

        private bool IsAiMoveModeSnapshotSelfCurrent(LF2Entity self)
        {
            if (self?.Runtime == null)
                return false;

            int slot = Slot(self);
            return slot >= 0 &&
                   slot < aiInputSlots.Length &&
                   ReferenceEquals(aiInputSlots[slot], self) &&
                   TryGetCurrentRuntimeHandle(
                       slot,
                       self,
                       out RuntimeEntityHandle handle) &&
                   handle.Generation == aiPhase1GenerationBySlot[slot];
        }

        private void AiUpdateMoveModeScanFull(
            LF2Entity self,
            ref AiInputContext ai)
        {
            int rightmostX = -1;
            int rightmostZ = 0;
            for (int i = 0; i < 10; i++)
            {
                LF2Entity candidate = CurrentAiMoveModeCandidateAt(i);
                if (candidate == null ||
                    candidate == self ||
                    !IsLivingCharacterDat(candidate))
                {
                    continue;
                }

                if (X(candidate) > rightmostX)
                {
                    rightmostX = X(candidate);
                    rightmostZ = Z(candidate);
                }
            }
            if (rightmostX < 0)
                return;

            ApplyAiMoveModeFromRightmost(
                self,
                rightmostX,
                rightmostZ,
                ref ai);
        }

        private LF2Entity CurrentAiMoveModeCandidateAt(int slot)
        {
            if (!TryGetRuntimeSlotReadOnlyView(
                    slot,
                    out RuntimeSlotTable.ReadOnlySlotView view) ||
                !view.Claimed ||
                view.Entity == null ||
                !IsActiveForCurrentPass(view.Entity))
            {
                return null;
            }
            return view.Entity;
        }

        private static void ApplyAiMoveModeFromRightmost(
            LF2Entity self,
            int rightmostX,
            int rightmostZ,
            ref AiInputContext ai)
        {
            if (X(self) > rightmostX &&
                X(self) + Abs(Z(self) - rightmostZ) / 2 - rightmostX > 200)
            {
                ai.MoveMode = 1;
            }
            if (X(self) > rightmostX + 400)
                ai.MoveMode = 2;
        }

        private void AiPostNoTargetFallback(LF2Entity self, LF2Entity savedTarget, AiInputContext ai)
        {
            if (savedTarget != null)
            {
                bool close = !self.Runtime.HasInputHistoryGate() || (Abs(Z(self) - Z(savedTarget)) <= 150 && Abs(X(self) - X(savedTarget)) <= 240);
                if (close && ai.MoveMode == 1) self.Runtime.KeyLeft = 1;
            }
            if ((self.ObjectId == 7 && Frame(self) >= 255 && Frame(self) <= 261) ||
                (self.ObjectId == 9 && Frame(self) >= 280 && Frame(self) <= 290) ||
                (self.ObjectId == 32 && Frame(self) >= 240 && Frame(self) <= 245))
                self.Runtime.KeyAttack = 1;
        }

        private static void RollAndClearAiKeys(NTSDEntityRuntime input)
        {
            input.RollInputFromCurrent();
            input.ClearDirectionalInputKeys();
            input.ClearActionInputKeys();
        }

        private void MoveTowardCoordinate(LF2Entity self, AiInputContext ai)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (input.Unk3FC <= -1000 || input.Unk400 <= -1000) return;
            if (X(self) > input.Unk3FC + 6)
            {
                input.KeyLeft = 1;
                if (X(self) > input.Unk3FC + 250 && Rand(ai.Rand3 + 3) == 0) input.PrevLeft = 0;
                if (X(self) < input.Unk3FC + 100 && State(self) == 2 && Facing(self) == 1) input.KeyRight = 1;
            }
            else if (X(self) < input.Unk3FC - 6)
            {
                input.KeyRight = 1;
                if (X(self) < input.Unk3FC - 250 && Rand(ai.Rand3 + 3) == 0) input.PrevRight = 0;
                if (X(self) > input.Unk3FC - 100 && State(self) == 2 && Facing(self) == 0) input.KeyLeft = 1;
            }
            if (Z(self) < input.Unk400 - 3) input.KeyDown = 1;
            else if (Z(self) > input.Unk400 + 3) input.KeyUp = 1;
            if (input.XBoundPositive || input.XBoundNegative) { input.PrevJump = 0; input.KeyJump = 1; }
            if (Abs(input.Unk400 - Z(self)) <= 90 && Abs(input.Unk3FC - X(self)) <= 90)
            { input.Unk3FC = -1000; input.Unk400 = -1000; }
        }

        private void MoveTowardTarget(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (X(self) > X(target) + 6)
            {
                input.KeyLeft = 1;
                if (X(self) > X(target) + 250 && Rand(ai.Rand3 + 3) == 0) input.PrevLeft = 0;
                if (X(self) < X(target) + 100 && selfState == 2 && Facing(self) == 1) input.KeyRight = 1;
            }
            else if (X(self) < X(target) - 6)
            {
                if (ai.MoveMode == 0) input.KeyRight = 1;
                if (X(self) < X(target) - 250 && Rand(ai.Rand3 + 3) == 0 && ai.MoveMode == 0) input.PrevRight = 0;
                if (X(self) > X(target) - 100 && selfState == 2 && Facing(self) == 0) input.KeyLeft = 1;
            }
            if (Z(self) < Z(target) - 3) input.KeyDown = 1;
            else if (Z(self) > Z(target) + 3) input.KeyUp = 1;
        }

        private static bool AiPostCacheCoordinateAllowsSpecial(LF2Entity self)
        {
            NTSDEntityRuntime r = self.Runtime;
            if (r.Unk3FC <= -1000) return true;
            if (Abs(r.Unk400 - Z(self)) > 90 || Abs(r.Unk3FC - X(self)) > 90) return false;
            r.Unk3FC = -1000; r.Unk400 = -1000;
            return true;
        }

        private bool AiPreUpdateTarget3000SideEffect(LF2Entity self, LF2Entity target, int selfState, int targetState, AiInputContext ai)
        {
            if (targetState != 3000) return false;
            bool randomGate = ai.Rand3 <= 0 || Rand(ai.Rand3) == 0;
            if (selfState != 7 && randomGate &&
                ((X(target) > X(self) && X(target) < X(self) + 200 && target.Runtime.Vx < 0.0) ||
                 (X(target) < X(self) && X(target) > X(self) - 200 && target.Runtime.Vx > 0.0)))
            { self.Runtime.PrevAttack = 0; self.Runtime.KeyAttack = 1; }
            if (X(target) > X(self) && Facing(self) == 1) self.Runtime.KeyRight = 1;
            if (X(target) < X(self) && Facing(self) == 0) self.Runtime.KeyLeft = 1;
            return true;
        }

        private bool AiUpdateOid33_19_16PredictedDuaDecision(LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = self.ObjectId;
            if (oid != 33 && oid != 19 && oid != 16) return false;
            if (Rand(5) != 0 && targetState != 16 && targetState != 8) return false;
            bool facing = (Facing(self) == 0 && X(self) < X(target)) || (Facing(self) == 1 && X(self) > X(target));
            if (Abs(X(target) + (int)self.Runtime.Vx - X(self)) < 60 && Abs(Z(target) - Z(self)) < 7 && Pp(self) > 150 && facing)
            { self.Runtime.ComboDua = 3; return true; }
            return false;
        }

        private bool AiUpdateOid52_1_2_21PreLabel591Decision(LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = self.ObjectId;
            if (oid != 52 && oid != 1 && oid != 2 && oid != 21) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (targetState == 3 && Pp(self) > 125 && Rand(10) == 0 && dx < 120 && dz < 10)
            { self.Runtime.ComboDja = 3; return true; }
            if (Pp(self) > 125 && Rand(5) == 0 && dx < 100 && dz < 30)
            { if (X(target) > X(self)) self.Runtime.ComboDuj = 3; return true; }
            if (Pp(self) > 125 && Rand(14) == 0 && dx < 700 && dz < 150)
            { if (X(target) > X(self)) self.Runtime.ComboDra = 3; else self.Runtime.ComboDla = 3; return true; }
            if (Pp(self) > 125 && Rand(5) == 0 && dz < 20)
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            bool predictedGate = Rand(5) == 0 || targetState == 16 || targetState == 8;
            bool facing = (Facing(self) == 0 && X(self) < X(target)) || (Facing(self) == 1 && X(self) > X(target));
            if (predictedGate && Abs(X(target) + (int)self.Runtime.Vx - X(self)) < 100 && dz < 7 && Pp(self) < 100 && facing)
            { self.Runtime.ComboDua = 3; return true; }
            return false;
        }

        private bool AiUpdateLabel591Oid51_2_18_7Decision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 51 && oid != 2 && oid != 18 && oid != 7) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Frame(self) > 265 && Frame(self) < 280 && (dz > 13 || !IsCharacterDat(target)))
            { self.Runtime.PrevAttack = 0; self.Runtime.KeyAttack = 1; return true; }
            if (Pp(self) > 300 && Rand(10) == 0 && dx < 300 && dz < 200) { self.Runtime.ComboDuj = 3; return true; }
            if (Pp(self) > 300 && Rand(10) == 0 && dx < 950) { self.Runtime.ComboDua = 3; return true; }
            if (Rand(5) == 0 && Pp(self) > 250 && dx < 1200 && dx > 40 && dz < 13)
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            return false;
        }

        private bool AiUpdateFirstDecision(LF2Entity self, LF2Entity target, int nearestTargetDist, bool specialObjectProximity)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (Rand(10) == 0 && Pp(self) > 85 &&
                ((Hp(self) < HpMax(self) - 70 && Hp(self) < 450) || (Hp(self) < (3 * HpMax(self)) / 5 && Hp(self) >= 140)))
            { self.Runtime.ComboDdj = 3; return true; }
            if (nearestTargetDist < 10000 && Rand(30) == 0 && Pp(self) > 250) { self.Runtime.ComboDua = 3; return true; }
            int targetOid = target.ObjectId;
            bool split = targetOid == 2 || targetOid == 9 || targetOid == 10 || targetOid == 11 || targetOid == 33 || targetOid == 34;
            int maxDx = split ? 500 : 250;
            int targetPpMin = split ? 220 : 170;
            if (Rand(15) == 0 && Abs(X(target) - X(self)) > 100 && Abs(X(target) - X(self)) < maxDx &&
                Abs(Z(target) - Z(self)) < 30 && Pp(self) > 100 && Pp(target) > targetPpMin && !specialObjectProximity)
            { if (X(target) <= X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3; return true; }
            return false;
        }

        private bool AiUpdateTeammateGuardDecision(LF2Entity self, AiInputContext ai, int nearestTargetDist, bool sameZLane)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (self.Runtime.LinkState != 0 && Frame(self) >= 9) return false;
            bool hpWindow = (Hp(self) >= HpMax(self) - 70 || Hp(self) >= 140) &&
                            (Hp(self) >= (3 * HpMax(self)) / 5 || Hp(self) < 140);
            if (!hpWindow || sameZLane) return false;
            for (int i = 0; i < 20; i++)
            {
                LF2Entity cand = AiAt(i);
                if (cand == null || cand == self || Team(cand) == 0 || Team(cand) != Team(self) ||
                    Abs(X(cand) - X(self)) >= 250 || Abs(Z(cand) - Z(self)) >= 60 || Pp(self) <= 350)
                    continue;
                bool lowHp = (Hp(cand) < HpMax(cand) - 90 && Hp(cand) < 140) ||
                             (Hp(cand) < (3 * HpMax(cand)) / 5 && Hp(cand) >= 140);
                if (!lowHp || Hp(cand) <= 0 || Distance(self, cand) >= nearestTargetDist / 3) continue;
                if (X(cand) > X(self) && Facing(self) == 1 && Abs(X(cand) - X(self)) >= 5)
                { self.Runtime.KeyRight = 1; self.Runtime.KeyLeft = 0; return true; }
                if (X(cand) < X(self) && Facing(self) != 1 && Abs(X(cand) - X(self)) >= 5)
                { self.Runtime.KeyRight = 0; self.Runtime.KeyLeft = 1; return true; }
                self.Runtime.ComboDuj = 3; return true;
            }
            return false;
        }

        private bool AiUpdateOid1ComboDecision(LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 21 && oid != 17) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Frame(self) >= 260 && Frame(self) <= 289 && dx < 100 && dz < 7) return false;
            if (Rand(7) == 0 && dx < 150 && dz < 8 && Pp(self) > 150 &&
                ((Rand(10) == 0 && targetState != 3) || (Rand(3) > 0 && (targetState == 16 || targetState == 8 || targetState == 11))))
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            if (Rand(7) == 0 && dx < 100 && dz < 7 && Pp(self) > 75)
            {
                if (Pp(self) <= 150 || ((Rand(10) != 0 || targetState == 3) && (Rand(3) <= 0 || targetState != 16)))
                { self.Runtime.ComboDda = 3; return true; }
                if (X(target) <= X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3;
                return true;
            }
            return false;
        }

        private bool AiUpdateCloseOid1Decision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 21 && oid != 17) return false;
            if (Frame(self) < 260 || Frame(self) > 289 || Abs(X(target) - X(self)) >= 100 || Abs(Z(target) - Z(self)) >= 7) return false;
            if ((Y(target) == 0 && Y(self) == 0 && Rand(3) == 0) || (Y(target) < 0 && Y(self) < 0 && Rand(7) == 0))
            { self.Runtime.KeyJump = 1; self.Runtime.PrevJump = 0; return true; }
            if ((Y(target) >= 0 || Rand(5) != 0) && Rand(30) != 0) return true;
            bool targetRight = X(target) > X(self);
            bool targetLeft = X(target) < X(self);
            if ((targetRight && Facing(self) == 0) || (targetLeft && Facing(self) == 1)) self.Runtime.KeyDefend = 1;
            self.Runtime.PrevDefend = 0;
            return true;
        }

        private bool AiUpdateOid4ComboDecision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 4 && oid != 10 && oid != 19) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Pp(self) > 360 && dx < 100 && dz < 70 && Rand(Hp(self) / 5 + 10) == 0)
            { self.Runtime.ComboDuj = 3; return true; }
            if (Rand(45) == 0 && dx > 100 && dx < 550 && dz < 20 && Pp(self) > 170)
            { if (X(target) <= X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3; return true; }
            if (Rand(30) == 0 && Pp(self) > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                bool facing = (Facing(self) == 0 && X(self) < X(target)) || (Facing(self) == 1 && X(self) > X(target));
                if (facing) { self.Runtime.ComboDja = 3; return true; }
            }
            return false;
        }

        private bool AiUpdateOid5ComboDecision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 5 && oid != 19) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Pp(self) > 450 && dx > 100 && dz > 50 && Rand(3) == 0)
            { if (Rand(2) != 0) self.Runtime.ComboDdj = 3; else self.Runtime.ComboDuj = 3; return true; }
            if (Pp(self) > 70 && dx > 100 && dx < 160 && dz < 8 && Rand(10) == 0)
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            if (Rand(30) == 0 && Pp(self) > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                if (Facing(self) == 0 && X(self) < X(target)) { self.Runtime.ComboDra = 3; return true; }
                if (Facing(self) == 1 && X(self) > X(target)) { self.Runtime.ComboDla = 3; return true; }
            }
            return false;
        }

        private static bool AiProcessSubOidGroup(int oid) => oid <= 29 || oid == 33 || oid == 34;
        private static bool AiSpecialOidForSubGate(int oid) => oid == 18 || oid == 5 || oid == 31 || oid == 36;

        private void AiProcessSubHelper(LF2Entity self, LF2Entity target, AiInputContext ai, int targetState, bool specialLeft, bool specialRight)
        {
            NTSDEntityRuntime input = self.Runtime;
            int oid = self.ObjectId;
            int predictedTargetX = X(target) + 2 * (int)target.Runtime.Vx;
            if (Pp(self) < 150) input.ComboDja = 3;
            if (Abs(X(target) - 2 * (int)self.Runtime.Vx - X(self)) < 80 && Abs(Z(target) - Z(self)) < 5 &&
                Rand(ai.Rand3 + 3) == 0 && targetState != 14) input.KeyJump = 1;
            if ((specialLeft && X(target) > X(self)) || (specialRight && X(target) < X(self))) return;
            if (Rand(ai.Rand3 + 1) != 0) return;
            int predictedDelta = Abs(predictedTargetX - X(self));
            if (AiProcessSubOidGroup(oid) && predictedDelta > 100 && predictedDelta < 900 && Abs(Z(target) - Z(self)) < 5 &&
                Rand(ai.Rand3 + 10) == 0 && targetState != 14) input.KeyAttack = 1;
            bool facing = (Facing(self) == 0 && X(target) > X(self)) || (Facing(self) == 1 && X(target) < X(self));
            if (AiProcessSubOidGroup(oid) && predictedDelta > 90 && facing && (Frame(self) == 110 || Frame(self) >= 235) &&
                Abs(Z(target) - Z(self)) < 13 && targetState != 14)
            {
                input.PrevRight = input.PrevLeft = input.PrevJump = 0;
                if (X(target) <= X(self)) input.KeyLeft = 1; else input.KeyRight = 1;
                if (oid != 34 || Rand(2) != 0) input.KeyJump = 1; else input.KeyDefend = 1;
            }
            if (oid == 1 && predictedDelta > 100 && predictedDelta < 300 && Abs(Z(target) - Z(self)) < 5 &&
                Rand(ai.Rand5 + 10) == 0 && targetState != 14) input.KeyAttack = 1;
            if (oid == 1 && predictedDelta > 90 && facing && (Frame(self) == 110 || Frame(self) >= 235) &&
                Abs(Z(target) - Z(self)) < 7 && targetState != 14)
            {
                input.PrevRight = input.PrevLeft = input.PrevJump = 0;
                if (X(target) <= X(self)) input.KeyLeft = 1; else input.KeyRight = 1;
                input.KeyJump = 1;
            }
        }

        private void AiProcessSubCallerPrewrite(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState)
        {
            NTSDEntityRuntime input = self.Runtime;
            bool specialOid = AiSpecialOidForSubGate(self.ObjectId);
            if (input.LinkState == 0 && targetState == 16 && specialOid &&
                Abs(X(target) - 2 * (int)input.Vx - X(self)) < 350 && Abs(Z(target) - Z(self)) < 5 && Rand(ai.Rand3 + 3) == 0)
            {
                if ((X(target) > X(self) && Facing(self) == 0) || (X(target) <= X(self) && Facing(self) == 1)) input.KeyJump = 1;
            }
            if (input.LinkState != 0 || targetState == 16 || !specialOid) return;
            bool closeTrigger = X(target) - X(self) < 100 && Abs(Z(target) - Z(self)) < 80 && Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger && selfState != 7)
            {
                if (Abs(X(target) - 2 * (int)input.Vx - X(self)) < 300 && Abs(Z(target) - Z(self)) < 5 &&
                    Rand(ai.Rand3 + 3) == 0 && targetState != 14 &&
                    ((X(target) > X(self) && Facing(self) == 0) || (X(target) <= X(self) && Facing(self) == 1))) input.KeyJump = 1;
            }
            else if (selfState != 7)
            {
                bool closeWindow = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
                ApplyPressureRetreat(self, target, ai, closeWindow);
                if (closeWindow && Rand(17) == 0) input.KeyDefend = 1;
            }
        }

        private void AiProcessSubLabel435PressurePrewrite(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState)
        {
            NTSDEntityRuntime input = self.Runtime;
            bool specialOid = AiSpecialOidForSubGate(self.ObjectId);
            if (targetState != 16 && specialOid && input.LinkState == 0) return;
            bool pressure = Hp(target) > Hp(self) * 2 || (Hp(self) <= 100 && Hp3(self) > 100);
            if (!pressure || ai.InputPhase != 1 || !IsCharacterDat(target) || Slot(self) < 20 || Team(self) == 5) return;
            bool closeTrigger = X(target) - X(self) < 100 && Abs(Z(target) - Z(self)) < 80 && Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger || selfState == 7) return;
            bool closeWindow = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
            ApplyPressureRetreat(self, target, ai, closeWindow);
            if (closeWindow && Rand(17) == 0) input.KeyDefend = 1;
        }

        private static void ApplyPressureRetreat(LF2Entity self, LF2Entity target, AiInputContext ai, bool closeWindow)
        {
            if (!closeWindow) return;
            if ((X(target) < 250 || X(target) < X(self)) && X(target) <= ai.StageTargetX - 250)
            { self.Runtime.KeyRight = 1; self.Runtime.PrevRight = 0; }
            else if (X(target) > ai.StageTargetX - 250 || X(target) > X(self))
            { self.Runtime.KeyLeft = 1; self.Runtime.PrevLeft = 0; }
        }

        private bool AiProcessHelper(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState, bool sameZLane, bool specialObjectProximity)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (Rand(ai.Rand3 + 1) > 0) return false;
            int heldSlot = input.TargetSlotIndex;
            if (heldSlot < 0 || heldSlot >= aiInputSlots.Length) return true;
            LF2Entity held = AiAt(heldSlot);
            int heldOid = held != null ? held.ObjectId : -1;
            bool lineCover = false;
            for (int i = 0; i < 20; i++)
            {
                LF2Entity cand = AiAt(i);
                if (cand == null || cand == self || Team(cand) == 0 || Team(target) != Team(self) || Hp(cand) <= 0 ||
                    State(cand) == 14 || Abs(Y(cand)) > 2) continue;
                if (Abs(Z(cand) - Z(self)) < 15 && ((X(self) < X(cand) && X(cand) < X(target)) || (X(target) < X(cand) && X(cand) < X(self))))
                    lineCover = true;
            }
            if (selfState == 2 && Rand(ai.Rand3 + 5) == 0)
            { if (lineCover) input.KeyDefend = 1; else input.KeyJump = 1; }

            int vxTwice = 2 * (int)input.Vx;
            if (heldOid == 100 || heldOid == 101 || heldOid == 120 || heldOid == 121 || heldOid == 124)
            {
                if (Abs(X(target) - vxTwice - X(self)) < 10000 && Abs(Z(target) - Z(self)) < 6 && Rand(ai.Rand3 + 3) == 0 && targetState != 14)
                    input.KeyJump = 1;
                if (heldOid == 124 && Rand(ai.Rand15 + 30) == 0) input.KeyJump = 1;
                if (Rand(ai.Rand3 + 5) == 0)
                {
                    bool close = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
                    if (close && Abs(X(target) - X(self)) < 600 && Abs(Z(target) - Z(self)) < 20)
                    {
                        if (X(target) > X(self) && ai.MoveMode == 0) { input.KeyRight = 1; input.PrevRight = 0; }
                        if (X(target) < X(self)) { input.KeyLeft = 1; input.PrevLeft = 0; }
                    }
                }
            }
            if ((heldOid == 150 || heldOid == 151) && !lineCover && Abs(X(target) - vxTwice - X(self)) < 5000 &&
                Abs(Z(target) - Z(self)) < 10 && Rand(ai.Rand5 + 7) == 0 && targetState != 14) input.KeyJump = 1;
            if (heldOid != 122 && heldOid != 123) return true;

            input.ClearActionInputKeys(); input.ClearDirectionalInputKeys();
            if (selfState == 17 && sameZLane && !specialObjectProximity && input.HitStop != 0)
            { input.KeyAttack = 1; return false; }
            if (input.HasInputHistoryGate() && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240)) return false;
            if (Z(target) < StageZMin + 30) input.KeyDown = 1;
            else if (Z(target) < StageZMax - 30) input.KeyUp = 1;
            else if (Z(target) > Z(self)) input.KeyUp = 1;
            else input.KeyDown = 1;

            if (X(target) < 400 && X(self) < 200)
            {
                input.KeyRight = 1;
                if (Rand(ai.Rand3 + 7) == 0) input.PrevRight = 0;
                if (Rand(ai.Rand3 + 5) == 0 && selfState == 2) input.KeyDefend = 1;
                return false;
            }
            if (X(target) > ai.StageTargetX - 400 && X(self) > ai.StageTargetX - 200)
            {
                input.KeyLeft = 1;
                if (Rand(ai.Rand3 + 7) == 0) input.PrevLeft = 0;
                if (Rand(ai.Rand3 + 5) == 0 && selfState == 2) input.KeyDefend = 1;
                return false;
            }
            if (Abs(X(target) - X(self)) < 350 && Abs(Z(target) - Z(self)) < 70)
            {
                if (X(target) > X(self)) { input.KeyLeft = 1; if (Rand(ai.Rand3 + 4) == 0) input.PrevLeft = 0; }
                if (X(target) <= X(self)) { input.KeyRight = 1; if (Rand(ai.Rand3 + 4) == 0) input.PrevRight = 0; }
                return false;
            }
            if (selfState == 2)
            { if (Facing(self) == 0) input.KeyLeft = 1; if (Facing(self) == 1) input.KeyRight = 1; return false; }
            if (Rand(5) != 0) return false;
            if (specialObjectProximity || (self.ObjectId != 2 && self.ObjectId != 34) || Pp(self) <= 150 || Rand(ai.Rand3 + 3) <= 0)
            { input.KeyJump = 1; return false; }
            if (X(target) > X(self)) input.ComboDrj = 3; else input.ComboDlj = 3;
            return true;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的正式版战斗 pass 执行入口。
    /// </summary>
    public partial class SimulationWorld
    {
        internal static System.Func<SimulationWorld, LF2Entity, LF2Entity> RespawnEffectSpawnOverride;
        internal int LastCollisionPairVRestEligibilityVisitCount { get; private set; }

        private void RunDeferredMutationEntityPass(System.Action<LF2Entity> action)
        {
            if (action == null)
                return;

            _ticking = true;
            try
            {
                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (entity == null || !IsActiveForCurrentPass(entity))
                        return;

                    action(entity);
                });
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        public void PostCooldownInputAll(int tickIndex)
        {
            PostCooldownHumanInputAll(tickIndex);
            CharacterInputAll(tickIndex);
        }

        public void FlushQueuedObjectPointTasks()
        {
            LF2ObjectPointFactory.Instance?.FlushTasks();
        }

        public void PostCooldownHumanInputAll(int tickIndex)
        {
            RefreshActiveHumanRosterInputBindings();
            RunDeferredMutationEntityPass(entity =>
            {
                if (!IsBoundActiveHumanRosterInputEntity(entity) ||
                    !entity.TryGetSharedInputControllerForSimulation(out _))
                    return;
                entity.RunHumanInputPollPhase(tickIndex);
                if (IsActiveForCurrentPass(entity))
                    RefreshRuntimeSnapshot(entity);
            });
        }

        public void ClearBattleEntryInputAll()
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (entity.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    return;

                entity.ClearBattleEntryInputState();
                if (IsActiveForCurrentPass(entity))
                    RefreshRuntimeSnapshot(entity);
            });
        }

        public void AiInputAndComboAll(int tickIndex)
        {
            if (tickIndex <= 1)
                return;

            BuildAiInputSlotSnapshot();
            try
            {
                RunDeferredMutationEntityPass(entity =>
                {
                    if (!entity.AiControlled || entity.GetCurrentDataObjectTypeForSimulation() != 0)
                        return;
                    entity.RunCharacterInputPhase(tickIndex);
                    if (IsActiveForCurrentPass(entity))
                        RefreshRuntimeSnapshot(entity);
                    ObserveAiTeamHpSummaryMutation(entity);
                });
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        public void CharacterInputAll(int tickIndex)
        {
            if (tickIndex <= 1)
                return;

            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            BattleAiInputDetailDiagnostics aiDetailDiagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            aiDetailDiagnostics?.BeginTick(tickIndex);
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.CharacterInputSnapshotBuild);
            BuildAiInputSlotSnapshot();
            detailDiagnostics?.EndPhase(
                BattleTickDetailPhase.CharacterInputSnapshotBuild);
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.CharacterInputEntityInputPass);
            try
            {
                RunDeferredMutationEntityPass(entity =>
                {
                    if (entity.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                        return;

                    entity.RunCharacterInputPhase(tickIndex);
                    if (IsActiveForCurrentPass(entity))
                    {
                        aiDetailDiagnostics?.BeginPhase(
                            BattleAiInputDetailPhase.RefreshRuntimeSnapshot);
                        RefreshRuntimeSnapshot(entity);
                        aiDetailDiagnostics?.RecordRefresh();
                        aiDetailDiagnostics?.EndPhase(
                            BattleAiInputDetailPhase.RefreshRuntimeSnapshot);
                    }
                    ObserveAiTeamHpSummaryMutation(entity);
                });
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputEntityInputPass);
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
                ClearAiInputSlotSnapshot();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
            }
        }

        public void Oid5152RuntimeMaintenanceAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < 20; runtimeSlot++)
                {
                    LF2Entity obj = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                    if (obj == null || !IsActiveForCurrentPass(obj))
                        continue;

                    if (obj.Runtime.Unk338 > 0)
                    {
                        obj.Runtime.Unk338--;
                        RefreshRuntimeSnapshot(obj);
                    }

                    if (obj.ObjectId == 51)
                    {
                        TrySplitOid51BackToPair(obj);
                    }
                    else if (obj.ObjectId == 7 || obj.ObjectId == 8)
                    {
                        TryMergeOid7Or8Into51(obj);
                    }
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private bool TryMergeOid7Or8Into51(LF2Entity self)
        {
            if (self?.Runtime == null || self.Health == null)
                return false;

            int selfSlot = self.Runtime.SlotIndex;
            LF2FrameData selfFrame = self.Frame?.D;
            if (selfSlot < 0 || selfSlot >= 10 || selfFrame == null || selfFrame.state != 2)
                return false;
            if (self.Health.HP <= 0 || self.Runtime.Unk338 != 0)
                return false;
            if (!PassesOid5152HpGate(self))
                return false;

            LF2CharacterDataWrapper oid51Wrapper = LF2Entity.ResolveRuntimeCharacterConfig(51);
            if (oid51Wrapper == null)
                return false;

            int selfX = self.GetRuntimeXInt();
            int selfZ = self.GetRenderZInt();
            int selfRelationTeam = ResolveOid5152RelationTeam(self);
            int partnerOid = 15 - self.ObjectId;

            for (int partnerSlot = 0; partnerSlot < 20; partnerSlot++)
            {
                if (partnerSlot == selfSlot)
                    continue;

                LF2Entity partner = FindEntityByRuntimeSlotForQuery(partnerSlot);
                if (partner?.Runtime == null || partner.Health == null)
                    continue;
                if (partner.ObjectId != partnerOid || partner.Health.HP <= 0 || partner.Runtime.Unk338 != 0)
                    continue;
                if (!PassesOid5152HpGate(partner))
                    continue;
                if (ResolveOid5152RelationTeam(partner) != selfRelationTeam)
                    continue;

                LF2FrameData partnerFrame = partner.Frame?.D;
                int partnerFrameId = partner.Frame?.N ?? -1;
                if (partnerFrame == null || partnerFrameId < 0 || partnerFrameId >= LF2FrameCache.MaxFrameIdExclusive)
                    continue;
                if (partnerFrame.state == 14)
                    continue;
                if (partnerFrame.state != 2 && (partner.GetRuntimeYInt() != 0 || partnerSlot <= 9))
                    continue;

                int partnerX = partner.GetRuntimeXInt();
                int partnerZ = partner.GetRenderZInt();
                if (Mathf.Abs(selfX - partnerX) >= 50 || Mathf.Abs(selfZ - partnerZ) >= 8)
                    continue;
                if (partnerSlot <= 9 && selfX <= partnerX)
                    continue;

                int mergedHpBound = self.Health.HPBound + partner.Health.HPBound;
                if (mergedHpBound > self.Health.HP3)
                    mergedHpBound = self.Health.HP3;

                int mergedHp = self.Health.HP + partner.Health.HP;
                if (mergedHp > mergedHpBound)
                    mergedHp = mergedHpBound;

                int midpointX = (selfX + partnerX) / 2;
                int midpointZ = (selfZ + partnerZ) / 2;
                int originalSelfOid = self.ObjectId;

                self.Runtime.Unk328 = 1;
                self.Runtime.Unk32C = partnerSlot;
                self.Runtime.Unk330 = originalSelfOid;
                self.Runtime.Unk334 = partner.ObjectId;
                self.Runtime.Unk338 = 4500;
                self.Health.HPBound = mergedHpBound;
                self.Health.HP = mergedHp;
                self.Runtime.Vx = 0f;
                self.Runtime.X = midpointX;
                self.Runtime.Z = midpointZ;
                self.Runtime.XInt = midpointX;
                self.Runtime.ZInt = midpointZ;

                partner.Runtime.Vy = 0f;
                partner.Runtime.OidMergeDormant = true;

                self.TryApplyRuntimeIdentity(51, 290, false, out _);
                self.Health.PP = 500;
                self.RefreshRuntimeSnapshot();
                partner.RefreshRuntimeSnapshot();
                return true;
            }

            return false;
        }

        private bool TrySplitOid51BackToPair(LF2Entity self)
        {
            if (self?.Runtime == null || self.Health == null)
                return false;
            if (self.ObjectId != 51 || self.Runtime.Unk328 != 1 || self.Runtime.Unk338 > 0)
                return false;

            int currentFrameId = self.Frame?.N ?? -1;
            if (currentFrameId >= 9 && currentFrameId <= 260)
                return false;

            int originalOid = self.Runtime.Unk330;
            if (LF2Entity.ResolveRuntimeCharacterConfig(originalOid) == null)
                return false;

            int aggregateHp = self.Health.HP;
            int aggregateHpBound = self.Health.HPBound;
            int partnerSlot = self.Runtime.Unk32C;
            int partnerOid = self.Runtime.Unk334;
            double splitX = self.Runtime.X;
            double splitZ = self.Runtime.Z;
            int splitXInt = self.GetRuntimeXInt();
            int splitZInt = self.GetRenderZInt();
            double preservedVy = self.Runtime.Vy;
            double preservedVz = self.Runtime.Vz;
            string preservedDir = self.Runtime.Dir;

            self.TryApplyRuntimeIdentity(originalOid, currentFrameId, false, out _);
            self.Runtime.Unk328 = -1;
            self.Runtime.Unk338 = 900;
            self.RefreshRuntimeSnapshot();

            if (partnerSlot < 0)
                return true;

            LF2Entity partner = FindEntityByRuntimeSlotIncludingDormant(partnerSlot);
            if (partner == null || LF2Entity.ResolveRuntimeCharacterConfig(partnerOid) == null)
                return true;

            int halfHp = aggregateHp / 2;
            int halfHpBound = aggregateHpBound / 2;
            int partnerStableId = partner.Runtime.StableId;
            int partnerRuntimeSlot = partner.Runtime.SlotIndex;
            LF2ItrRestTracker.StateSnapshot partnerRestState = partner.ItrRest?.CaptureState();

            self.TryApplyRuntimeIdentity(originalOid, 112, false, out _);
            self.Health.HP = halfHp;
            self.Health.HPBound = halfHpBound;
            self.Health.PP = 0;
            self.Runtime.Y = 0f;
            self.Runtime.YInt = 0;
            self.Runtime.Vx = 0f;
            self.Runtime.Vy = preservedVy;
            self.Runtime.Vz = preservedVz;
            self.Runtime.Dir = preservedDir;
            self.RefreshRuntimeSnapshot();

            partner.Reset();
            // LF2Character.Reset has pool-specific defaults that differ from formal Entity::reset.
            partner.FrameDelay = 0;
            partner.KnockbackVx = 0.1;
            partner.KnockbackVy = 0.1;
            partner.KnockbackVz = 0.1;
            partner.HolderCopySlot = 99;
            partner.Effect?.Reset();
            if (partner is LF2Character partnerCharacter)
                partnerCharacter.DeadBlinkCountInternal = -1;
            if (partner.Frame != null)
            {
                partner.Frame.PN = 0;
                partner.Frame.Prev = 0;
                partner.Frame.Prev2 = 0;
                partner.Frame.Prev2D = null;
            }
            partner.ItrRest?.RestoreState(partnerRestState);
            partner.Runtime.StableId = partnerStableId;
            partner.SetRuntimeSlotIndex(partnerRuntimeSlot);
            partner.Runtime.OidMergeDormant = false;
            partner.TryApplyRuntimeIdentity(partnerOid, 112, true, out _);
            partner.Health.HP = halfHp;
            partner.Health.HPBound = halfHpBound;
            partner.Health.PP = 0;
            partner.RelationTeam = self.RelationTeam;
            partner.Runtime.X = splitX;
            partner.Runtime.Y = 0f;
            partner.Runtime.Z = splitZ;
            partner.Runtime.XInt = splitXInt;
            partner.Runtime.YInt = 0;
            partner.Runtime.ZInt = splitZInt;
            partner.Runtime.Vx = 0f;
            partner.Runtime.Vy = 0f;
            partner.Runtime.Vz = 0f;
            partner.SwitchDir(preservedDir == "right" ? "left" : "right");
            partner.RefreshRuntimeSnapshot();
            return true;
        }

        private bool PassesOid5152HpGate(LF2Entity entity)
        {
            if (entity?.Health == null || entity.Health.HP <= 0)
                return false;

            return BattleGameModeId == 1 || entity.Health.HP < 177;
        }

        private static int ResolveOid5152RelationTeam(LF2Entity entity)
        {
            return entity?.RelationTeam ?? 0;
        }

        public void SerialTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                // C++ frame_advance scans objects[0..399] and completes one entity before
                // advancing to the next slot. The dynamic scan lets a flushed producer in a
                // later slot participate this tick; a reused lower slot waits until next tick.
                ForEachEntityByRuntimeSlot(entity =>
                {
                    // C++ keeps this tick's held state visible through frame advance and the
                    // later frame_tick pass. The input phase owns rolling/clearing the next
                    // tick, so clearing here loses jump direction and inherited momentum.
                    entity.SimTransit(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        return;

                    entity.SimTU(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });

                CleanupState9998Entities();
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private void CleanupState9998Entities()
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null || frame.state != 9998) continue;
                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        public void PostFrameAdvanceDeathCleanupAll(int tickIndex)
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                entity?.Runtime?.SyncIntegerPosition();
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (!PassesRespawnGate(entity))
                    continue;

                if (entity.RespawnCount <= 0)
                {
                    ApplyRespawnWithoutStoredCount(entity);
                }
                else
                {
                    ApplyRespawnFromStoredCount(entity);
                }

                if (IsActiveForCurrentPass(entity))
                    RefreshRuntimeSnapshot(entity);
            }

            _entityScratch.Clear();
        }

        private bool PassesRespawnGate(LF2Entity entity)
        {
            if (entity?.Health == null || !IsActiveForCurrentPass(entity))
                return false;

            LF2FrameData frame = entity.Frame?.D;
            if (frame == null || frame.state != LF2States.Lying || entity.Health.HP > 0)
                return false;

            int slotIndex = entity.Runtime?.SlotIndex ?? -1;
            if (slotIndex < 20 && entity.KillCount < 0 && entity.RelationTeam != 5)
                return false;

            int hitStop = entity.HitStun;
            return hitStop > 0 && hitStop < 5;
        }

        private void ApplyRespawnWithoutStoredCount(LF2Entity entity)
        {
            int hp2 = entity.HP2Orig;
            if (hp2 < 2)
            {
                entity.FreeEntityLikeExe();
                return;
            }

            entity.HP2Orig = hp2 - 1;

            int relationTeam = entity.RelationTeam;
            int sumX = 0;
            int sumZ = 0;
            int count = 0;

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity other = _entityScratch[i];
                if (other == null || other == entity || other.Health == null)
                    continue;

                if (other.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;

                if (other.RelationTeam != relationTeam)
                    continue;

                sumX += other.Runtime.XInt;
                sumZ += other.Runtime.ZInt;
                count++;
            }

            if (count > 0)
            {
                int avgX = sumX / count;
                int avgZ = sumZ / count;
                entity.Runtime.X = avgX + entity.BattleRandInt(0, 51) - 26.0;
                entity.Runtime.XInt = (int)entity.Runtime.X;
                entity.Runtime.Z = avgZ + entity.BattleRandInt(0, 31) - 16.0;
                entity.Runtime.ZInt = (int)entity.Runtime.Z;
                entity.PS.x = entity.Runtime.X;
                entity.PS.z = entity.Runtime.Z;
            }

            entity.Health.PP = 500;
            entity.Health.PPBound = entity.Health.MaxPP;
            entity.Health.HPBound = entity.Health.HP3;
            entity.Health.HP = entity.Health.HPBound;
            entity.HitStun = 20;
            entity.DirectWriteFramePreserveWaitCounter(212);
            entity.PS.y = -300.0;
            entity.PS.vy = 0.0;
            entity.Runtime.Y = -300.0;
            entity.Runtime.Vy = 0.0;
            entity.Runtime.SyncIntegerPosition();
        }

        private void ApplyRespawnFromStoredCount(LF2Entity entity)
        {
            entity.HP2Orig = entity.HPOrig;
            entity.Health.PP = 0;
            entity.Health.HPBound = entity.RespawnCount;
            entity.Health.HP3 = entity.Health.HPBound;
            entity.Health.HP = entity.Health.HP3;
            entity.RespawnCount = 0;
            entity.HPOrig = 0;
            entity.RelationTeam = 1;

            if (entity.ObjectId >= 0x1E && entity.ObjectId <= 0x24)
                entity.Runtime.RenderPicOffset = 0x8C;

            entity.DirectWriteFramePreserveWaitCounter(0xDB);
            entity.AttackingCounter = 0;
            entity.FrameDelay = 0xA;

            TrySpawnRespawnEffect(entity);
        }

        private LF2Entity TrySpawnRespawnEffect(LF2Entity entity)
        {
            if (entity == null)
                return null;

            LF2Entity overrideSpawned = RespawnEffectSpawnOverride?.Invoke(this, entity);
            if (overrideSpawned != null)
                return overrideSpawned;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return null;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint { oid = 998, kind = 0, action = 6, facing = 0 };
            task.parent = null;
            task.team = 0;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = entity.RelationTeam;
            task.holderCopySlot = -1;
            task.spawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            task.pos = new Vector3(entity.GetRuntimeXInt(), entity.GetRuntimeYInt(), entity.GetRenderZInt());
            task.z = entity.GetRenderZInt();
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = entity.GetRuntimeXInt();
            task.initialRuntimeY = entity.GetRuntimeYInt();
            task.initialRuntimeZ = entity.GetRenderZInt() + 1;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;

            LF2Entity spawned = factory.CreateObjectImmediate(task);
            if (spawned == null)
                return null;

            spawned.RelationTeam = entity.RelationTeam;
            spawned.SpawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            spawned.RefreshRuntimeSnapshot();
            return spawned;
        }

        public void EarlyFrameAdvanceSpecialsAll(int tickIndex)
        {
            bool teleportGate = FrameToggle != 0;

            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                entity.RunEarlyTeleportSpecialsPhase(_entityScratch, teleportGate);
                if (!IsActiveForCurrentPass(entity))
                    continue;
                RefreshRuntimeSnapshot(entity);
            }

            RunEarlyState500Specials(_entityScratch);
            RunEarlyState501Specials(_entityScratch);
            _entityScratch.Clear();
        }

        private void RunEarlyState500Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null)
                    continue;

                if (frame.state != 500)
                    continue;

                if (entity.TransformTargetObjectId == -1 || entity.TransformOriginalObjectId >= 0)
                {
                    // BMD-023: state=500 reset branch must mirror baseline SetFrameImmediate:
                    // write Frame + FrameWaitCounter only, never Attacking. Unity's
                    // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                    entity.DirectWriteFramePreserveWaitCounter(0);
                    RefreshRuntimeSnapshot(entity);
                }
            }
        }

        private void RunEarlyState501Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null)
                    continue;

                if (frame.state != 501 || entity.TransformTargetObjectId <= -1)
                    continue;

                LF2CharacterDataWrapper wrapper = LF2Entity.ResolveRuntimeCharacterConfig(entity.TransformTargetObjectId);
                if (wrapper == null)
                    continue;

                entity.TransformOriginalObjectId = entity.ObjectId;
                entity.FrameCache.Load(wrapper);
                entity.ObjectId = entity.TransformTargetObjectId;
                // BMD-023: state=501 transform branch must mirror baseline SetFrameImmediate:
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                entity.DirectWriteRawFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);

                int ownerSlotIndex = entity.Runtime?.SlotIndex ?? -1;
                if (ownerSlotIndex < 0)
                    continue;

                for (int j = 0; j < entities.Count; j++)
                {
                    LF2Entity child = entities[j];
                    if (child == null)
                        continue;
                    if (child.KillCount != ownerSlotIndex)
                        continue;
                    if (child.Health != null && child.Health.HP <= 0)
                        continue;

                    child.FrameCache.Load(wrapper);
                    child.ObjectId = entity.ObjectId;
                    // BMD-023: state=501 child-transform branch must mirror baseline SetFrameImmediate.
                    // The authority selects from the integer Y snapshot, not the floating render position.
                    // write Frame + FrameWaitCounter only, never Attacking. Unity's
                    // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                    child.DirectWriteRawFramePreserveWaitCounter(child.Runtime != null && child.Runtime.YInt < 0 ? 212 : 0);
                    RefreshRuntimeSnapshot(child);
                }
            }
        }

        public void FrameLogicBeforeAdvanceAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                LF2FrameData frame = entity.Frame?.D;
                if (frame == null ||
                    frame.hit_Fa <= 0 ||
                    entity.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                    return;

                entity.RunFrameLogicBeforeAdvance();
                FlushQueuedObjectPointTasks();
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        internal int FindFirstFreeFrameLogicRuntimeSlot()
        {
            return FindFirstFreeRuntimeSlot(DynamicRuntimeSlotStart, RuntimeSlotCapacity);
        }

        public void CaptureCollisionFrameSnapshotsAll()
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (entity.Runtime != null && entity.Runtime.SuppressCollisionCandidateUntilTick > 0)
                {
                    int currentTick = CurrentTickIndex;
                    if (currentTick < entity.Runtime.SuppressCollisionCandidateUntilTick)
                        return;
                }

                entity.CaptureCollisionFrameSnapshot();
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void CollectCollisionCandidatesAll()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.CollectCollisionCandidates();
        }

        public void TickCollisionPairVRestAll()
        {
            _runtimeRestStore.BeginCollisionPairVRestEligibility();
            int visitedItems = 0;
            foreach (KeyValuePair<int, Bucket> pair in _buckets)
            {
                List<ISimObject> items = pair.Value.items;
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    visitedItems++;
                    if (items[itemIndex] is not LF2Entity entity ||
                        !IsActiveForCurrentPass(entity) ||
                        entity.FrameCache?.Wrapper?.characterData == null)
                    {
                        continue;
                    }

                    int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                    if (!_runtimeSlots.IsAddressable(runtimeSlot) ||
                        !object.ReferenceEquals(
                            _runtimeSlots.GetCurrentOccupant(runtimeSlot),
                            entity))
                    {
                        continue;
                    }

                    _runtimeRestStore.MarkCollisionPairVRestEligible(runtimeSlot);
                }
            }
            LastCollisionPairVRestEligibilityVisitCount = visitedItems;
            _runtimeRestStore.TickMarkedCollisionPairVRest();
        }

        public void EndCollisionCandidateConsumption()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.EndCollisionCandidateConsumption();
        }

        public void LateEntityUpdateAll(int tickIndex)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            // The production object-point factory is pass-stable. Resolve it lazily so an
            // empty LateEntityUpdateAll invocation retains the existing no-auto-create behavior.
            LF2ObjectPointFactory opointFactory = null;
            bool opointFactoryResolved = false;
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < RuntimeSlotCapacity; runtimeSlot++)
                {
                    LF2Entity obj = FindEntityByRuntimeSlotCurrent(runtimeSlot);

                    if (obj == null)
                        continue;
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);
                    obj.RunStateSpecialPreCollision();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityStateSpecial);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityRecovery);
                    obj.RunPreCollisionRecoveryPhase(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityRecovery);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityRecovery);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);
                    if (obj.Runtime == null ||
                        tickIndex >= obj.Runtime.SuppressLateFrameTickUntilTick)
                    {
                        obj.SimFrameTick(tickIndex);
                    }
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityFrameTick);
                        continue;
                    }
                    RefreshLateRuntimeSnapshot(
                        obj,
                        BattleLateRuntimeSnapshotStage.FrameTick,
                        detailDiagnostics);
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityCollision);
                    obj.SimEntityCollision(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityCollision);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityCollision);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);
                    bool exitedLateFrameTick = HandleLateFrameTickExit(obj);
                    if (exitedLateFrameTick)
                    {
                        if (obj is LF2SpecialAttack)
                            FlushQueuedObjectPointTasks();
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityFrameExit);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);
                    obj.RunLateDeathOpointPreCleanupPhase();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityDeathOpoint);
                        continue;
                    }
                    RefreshLateRuntimeSnapshot(
                        obj,
                        BattleLateRuntimeSnapshotStage.DeathOpoint,
                        detailDiagnostics);
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);
                    if (!opointFactoryResolved)
                    {
                        opointFactory = LF2ObjectPointFactory.Instance;
                        opointFactoryResolved = true;
                    }
                    LF2FrameData opointFrame = obj.Frame?.D;
                    bool frameHasOpoint = opointFrame != null &&
                        ((opointFrame.opoints != null && opointFrame.opoints.Count > 0) ||
                         opointFrame.opoint.HasValue);
                    if (opointFactory != null && frameHasOpoint)
                        opointFactory.ProcessOpointSpawnAlignedToCpp(obj);
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityOpointProcess);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    bool completedLateCleanup = obj.TryRunLatePostOpointCleanupPhase();
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    if (completedLateCleanup)
                    {
                        detailDiagnostics?.BeginPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        FlushQueuedObjectPointTasks();
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    obj.RunLateTailBeforePrevFrame();
                    FlushQueuedObjectPointTasks();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    RefreshLateRuntimeSnapshot(
                        obj,
                        BattleLateRuntimeSnapshotStage.TailAndQueuedFlush,
                        detailDiagnostics);
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                    obj.MirrorLatePrevFrame();
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                }
            }
            finally
            {
                _ticking = false;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
            }
        }

        private void RefreshLateRuntimeSnapshot(
            LF2Entity entity,
            BattleLateRuntimeSnapshotStage stage,
            BattleTickDetailPhaseDiagnostics diagnostics)
        {
            if (diagnostics == null)
            {
                RefreshRuntimeSnapshot(entity);
                return;
            }

            diagnostics.BeginLateRuntimeSnapshot(stage);
            try
            {
                RefreshRuntimeSnapshot(entity);
            }
            finally
            {
                diagnostics.EndLateRuntimeSnapshot(stage);
            }
        }

        private bool HandleLateFrameTickExit(LF2Entity entity)
        {
            if (entity?.Frame == null)
                return false;

            int frameId = entity.Frame.N;
            int frameGroup = frameId / 100;
            if (frameGroup == 11 || frameGroup == 12)
            {
                int ownerSlot = GetRuntimeSlotOrder(entity);
                GetAllEntities(_entityScratch);
                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity other = _entityScratch[i];
                    if (other != null && other.KillCount == ownerSlot)
                        other.HitStun = 1100 - frameId;
                }

                _entityScratch.Clear();
                entity.HitStun = 1100 - frameId;
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);
                return true;
            }

            if (frameId < 0 || frameId >= LF2FrameCache.MaxFrameIdExclusive)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            return false;
        }

        public void EntityPostFrameTailAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity == null || entity.Health == null)
                    return;

                if (entity.HealTimer / 1000 == 1 && entity.Health.HP > 0)
                {
                    entity.HealTimer--;
                    if (entity.HealTimer % 8 == 0)
                    {
                        if (entity.Health.HP < entity.Health.HPBound)
                        {
                            entity.Health.HP += 8;
                            if (entity.Health.HP > entity.Health.HPBound)
                                entity.Health.HP = entity.Health.HPBound;
                        }
                        else
                        {
                            entity.HealTimer = 0;
                        }
                    }

                    if (entity.HealTimer % 1000 == 0)
                        entity.HealTimer = 0;
                }

                if (entity.CatchTimer > 0 && entity.Health.HP > 0)
                {
                    entity.CatchTimer--;
                    if (entity.CatchTimer % 8 == 0 && entity.Health.HP < entity.Health.HPBound)
                    {
                        entity.Health.HP += 8;
                        if (entity.Health.HP > entity.Health.HPBound)
                        {
                            entity.Health.HP = entity.Health.HPBound;
                            entity.CatchTimer = 0;
                        }
                    }
                }

                LF2FrameData frame = entity.Frame?.D;
                if (frame != null && frame.state == 1700)
                    entity.HealTimer = 1100;

                entity.ClearHitCandidateCarriers();
                entity.Runtime.TransientMp = 0;
                entity.Runtime.TransientMp2 = 1000;
                entity.Runtime.TransientMp3 = 1000;
                entity.Runtime.TransientMp4 = 1000;
                RefreshRuntimeSnapshot(entity);
            });

        }

        public void FramePostProcessAll()
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity.FrameDelay != 0) return;

                if (entity.HitCount > 0)
                {
                    float denom = entity.HitCount + 1;
                    entity.PS.vx = entity.KnockbackVx * 2f / denom;
                    entity.PS.vy = entity.KnockbackVy * 2f / denom;
                    entity.PS.vz = entity.KnockbackVz * 2f / denom;
                }
                entity.KnockbackVx = 0f;
                entity.KnockbackVy = 0f;
                entity.KnockbackVz = 0f;
                entity.HitCount = 0;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void VrestTickAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(entity =>
            {
                entity.ItrRest?.TickArest();
                ClearAttackExemptIfCurrentFrameCannotHit(entity);
                RefreshRuntimeSnapshot(entity);
            });
        }

        private void ClearAttackExemptIfCurrentFrameCannotHit(LF2Entity entity)
        {
            if (entity == null || entity.AttackExempt <= 0)
                return;

            LF2CharacterData entityData = (entity as LF2LivingObject)?._FrameDataWrapper?.characterData
                ?? entity.FrameCache?.Wrapper?.characterData;
            if (entityData == null)
                return;

            LF2FrameData frame = entity.Frame?.D;
            bool clear = frame?.itrs == null || frame.itrs.Count == 0;
            if (!clear &&
                frame.state == LF2States.WeaponOnHand &&
                entity.Runtime != null)
            {
                int holderSlot = entity.Runtime.ResolveActiveHolderSlotIndex();
                LF2Entity holder = holderSlot >= 0
                    ? FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                LF2CharacterData holderData = (holder as LF2LivingObject)?._FrameDataWrapper?.characterData
                    ?? holder?.FrameCache?.Wrapper?.characterData;
                if (holder != null && holderData != null)
                {
                    LF2FrameData holderFrame = holder.Frame?.D;
                    clear = holderFrame?.wpoints == null ||
                            holderFrame.wpoints.Count == 0 ||
                            holderFrame.wpoints[0].attacking == 0;
                }
            }

            if (clear)
                entity.AttackExempt = 0;
        }

        public void PostInteractionTickAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (!entity.SupportsPostInteractionPhase()) return;
                if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPostInteractionUntilTick)
                    return;
                entity.SimPostInteraction(tickIndex);
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void ObjectInteractionTickAll(int tickIndex)
        {
            RunDeferredMutationEntityPass(entity =>
            {
                if (!entity.SupportsObjectInteractionPhase()) return;
                if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressObjectInteractionUntilTick)
                    return;
                entity.SimObjectInteraction(tickIndex);
                if (entity is LF2SpecialAttack)
                    FlushQueuedObjectPointTasks();
                if (!IsActiveForCurrentPass(entity))
                    return;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void PreInteractionTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                GetActiveEntitiesByRuntimeSlot(_entityScratch);
                if (_entityScratch.Count == 0) return;

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    entity.RunCpointCheckStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    entity.RunCpointMismatchTailStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                _entityScratch.Clear();

                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        return;
                    if (!IsActiveForCurrentPass(entity))
                        return;

                    entity.RunWeaponSyncHeldStep10();
                    if (!IsActiveForCurrentPass(entity))
                        return;
                    RefreshRuntimeSnapshot(entity);
                });
            }
            finally
            {
                _entityScratch.Clear();
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        public void RandomWeaponDropTickAll(int tickIndex)
        {
            int weaponCount = 0;
            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity.CountsAsRandomWeaponDropCandidate())
                    weaponCount++;
            });
            if (weaponCount >= 4) return;
            if (Rng.NextInt(0, 200) != 0) return;

            int freeSlot = FindFirstFreeRuntimeSlot(DynamicRuntimeSlotStart, RuntimeSlotCapacity);
            if (freeSlot < 0) return;

            var manager = CharacterAnimtorManager.Instance;
            var dataManager = GameDataManager.Instance;
            if (manager == null || dataManager == null) return;

            var candidates = new List<int>();
            var seenOids = new HashSet<int>();
            List<ObjectDefinition> loadedObjects = dataManager.GetAllObjects();
            for (int i = 0; i < loadedObjects.Count; i++)
            {
                int oid = loadedObjects[i].id;
                if (!seenOids.Add(oid)) continue;
                if (oid < 100 || oid >= 200) continue;
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null) continue;
                if (oid == 122 || oid == 123)
                {
                    if (Rng.NextInt(0, 2) == 0) continue;
                    if (BattleGameModeId >= 1 && BattleGameModeId <= 4) continue;
                }
                candidates.Add(oid);
            }
            if (candidates.Count == 0) return;

            int selectedOid = candidates[Rng.NextInt(0, candidates.Count)];
            var factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null) return;

            BattleStageRuntimeState stage = Runtime?.Stage;
            int xMaxOverride = stage?.XMaxOverride ?? 0;
            int stageWidth = stage?.BaseStageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            int r1 = Rng.NextInt(0, 30);
            int xBase = xMaxOverride == 0 ? stageWidth - 60 : xMaxOverride - 60;
            int xStep = xBase / 30;
            int r2 = Rng.NextInt(0, 30);
            int r3 = Rng.NextInt(0, 30);
            int zBase = zMax - zMin - 60;
            int zStep = zBase / 30;
            int r4 = Rng.NextInt(0, 30);
            double lf2X = r1 * xStep + r2 + 30;
            double lf2Z = r3 * zStep + r4 + zMin + 30;
            const double lf2Y = -500.0;

            OPointCreateTask spawnTask = referencePool.Fetch<OPointCreateTask>();
            spawnTask.opoint = new ObjectPoint
            {
                oid = selectedOid,
                kind = 0,
                action = 0,
                x = (int)lf2X,
                y = (int)lf2Y,
                dvx = 0,
                dvy = 0,
                facing = 0,
            };
            spawnTask.parent = null;
            spawnTask.team = 0;
            spawnTask.requiredRuntimeSlot = freeSlot;
            spawnTask.pos = new Vector3((float)lf2X, (float)lf2Y, 0f);
            spawnTask.z = (float)lf2Z;
            spawnTask.dir = "right";
            spawnTask.dvz = 0f;
            spawnTask.preserveActionZero = true;
            spawnTask.skipPostInitZOffset = true;
            spawnTask.useDirectRuntimePosition = true;
            spawnTask.directX = lf2X;
            spawnTask.directY = lf2Y;
            spawnTask.directZ = lf2Z;
            spawnTask.useDirectVelocity = true;
            spawnTask.directVx = 0.0;
            spawnTask.directVy = 0.0;
            spawnTask.directVz = 0.0;
            spawnTask.useInitialRuntimeIntPosition = true;
            spawnTask.initialRuntimeX = (int)lf2X;
            spawnTask.initialRuntimeY = (int)lf2Y;
            spawnTask.initialRuntimeZ = (int)lf2Z;
            spawnTask.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;

            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(spawnTask);
            }
            finally
            {
                referencePool.Recycle(spawnTask);
            }

            if (spawned == null || spawned.Runtime?.SlotIndex != freeSlot) return;

            spawned.Health.HP = selectedOid == 122 ? 200 : 500;
            spawned.Health.HPBound = 500;
            spawned.Health.HP3 = 500;
            spawned.Health.PP = 500;
            spawned.KillCount = -1;
            ResetCooldownsForRuntimeSlot(freeSlot);
            spawned.RefreshRuntimeSnapshot();
        }

        private void ResetCooldownsForRuntimeSlot(int runtimeSlot)
        {
            ResetCooldownsForRuntimeSlot(
                runtimeSlot,
                FindEntityByRuntimeSlotIncludingDormant(runtimeSlot));
        }

        public void Mode2RandomWeaponDropTailAll(int tickIndex)
        {
            int mode2Request = Mode2Request;
            if (mode2Request == 0)
                return;

            if (mode2Request == 1)
            {
                SpawnMode2RandomWeapons();
            }
            else if (mode2Request == 2)
            {
                ForEachEntityByRuntimeSlot(entity =>
                {
                    if (!entity.CountsAsRandomWeaponDropCandidate())
                        return;

                    entity.Runtime.WeaponFlightCounter = -1;
                    RefreshRuntimeSnapshot(entity);
                });
            }

            SetMode2Request(0);
        }

        private void SpawnMode2RandomWeapons()
        {
            var manager = CharacterAnimtorManager.Instance;
            if (manager == null)
                return;

            var candidates = new List<int>();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null)
                    continue;

                if (oid == 122 && Rng.NextInt(0, 2) == 0)
                    continue;

                candidates.Add(oid);
            }

            if (candidates.Count == 0)
                return;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out _, out _);
            if (stageWidth <= 60 || zMax - zMin <= 60)
                return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            for (int chooseIndex = 0; chooseIndex < candidates.Count; chooseIndex++)
            {
                int oid = candidates[chooseIndex];

                bool hasFreeSlot = false;
                for (int slot = DynamicRuntimeSlotStart; slot < RuntimeSlotCapacity; slot++)
                {
                    if (!_runtimeSlots.IsClaimed(slot))
                    {
                        hasFreeSlot = true;
                        break;
                    }
                }

                if (!hasFreeSlot)
                    break;

                int r1 = Rng.NextInt(0, 30);
                int r2 = Rng.NextInt(0, 30);
                int r3 = Rng.NextInt(0, 30);
                int r4 = Rng.NextInt(0, 30);
                float lf2X = r1 * ((stageWidth - 60) / 30) + r2 + 30;
                float lf2Z = r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
                const float lf2Y = -500f;

                var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
                int flyFrame = -1;
                int minFrame = int.MaxValue;
                if (charData?.frames != null)
                {
                    foreach (var f in charData.frames)
                    {
                        if (f == null)
                            continue;
                        if (f.frameId > 0 && f.frameId < minFrame)
                            minFrame = f.frameId;
                        if (flyFrame < 0 && f.frameId > 0 &&
                            (f.state == LF2States.WeaponInSky ||
                             f.state == LF2States.WeaponThrowing ||
                             f.state == LF2States.HeavyWeaponInSky))
                        {
                            flyFrame = f.frameId;
                        }
                    }
                }

                if (flyFrame < 0)
                    flyFrame = minFrame != int.MaxValue ? minFrame : 0;

                var spawnTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                spawnTask.opoint = new ObjectPoint
                {
                    oid = oid,
                    kind = 0,
                    action = flyFrame,
                    x = Mathf.RoundToInt(lf2X),
                    y = Mathf.RoundToInt(lf2Y),
                    dvx = 0,
                    dvy = 0,
                    facing = 0,
                };
                spawnTask.parent = null;
                spawnTask.team = 0;
                spawnTask.pos = new Vector3(lf2X, lf2Y, 0f);
                spawnTask.z = lf2Z;
                spawnTask.dir = "right";
                spawnTask.dvz = 0f;
                factory.CreateObjectImmediate(spawnTask);
            }
        }

#if UNITY_INCLUDE_TESTS
        internal int[] CaptureLateRuntimeSnapshotBoundaryForSelfCheck(int mode)
        {
            LF2Entity entity;
            LateRuntimeSnapshotProbe probe = null;
            LateRuntimeSnapshotWeaponProbe weapon = null;
            if (mode == 3)
            {
                weapon = new LateRuntimeSnapshotWeaponProbe();
                weapon.BindData();
                entity = weapon;
            }
            else
            {
                probe = new LateRuntimeSnapshotProbe(
                    zeroHpDuringRecovery: mode == 0,
                    cleanupCompleted: mode == 2);
                entity = probe;
            }

            Register(entity);
            if (mode == 1)
                entity.Runtime.SuppressLateFrameTickUntilTick = 2;

            BattleTickDetailPhaseDiagnostics diagnostics =
                EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            diagnostics.BeginTick(1);
            LateEntityUpdateAll(1);

            return new[]
            {
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.Recovery),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTickSuppressed),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.CleanupCompleted),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTick),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.DeathOpoint),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.TailAndQueuedFlush),
                probe?.RecoveryCount ?? 0,
                probe?.FrameTickCount ?? 0,
                probe?.FrameTickObservedHp ?? 0,
                probe?.DeathOpointCount ?? 0,
                probe?.DeathOpointObservedHp ?? 0,
                probe?.CleanupCount ?? 0,
                probe?.TailCount ?? 0,
                ObjectCount,
                weapon?.PendingDestroyObserved == true ? 1 : 0,
            };
        }

        private sealed class LateRuntimeSnapshotProbe : LF2Entity
        {
            private readonly bool zeroHpDuringRecovery;
            private readonly bool cleanupCompleted;

            internal int RecoveryCount { get; private set; }
            internal int FrameTickCount { get; private set; }
            internal int FrameTickObservedHp { get; private set; }
            internal int DeathOpointCount { get; private set; }
            internal int DeathOpointObservedHp { get; private set; }
            internal int CleanupCount { get; private set; }
            internal int TailCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum =>
                LF2ObjectType.Character;
            internal override bool UsesDynamicRuntimeSlot() => true;

            internal LateRuntimeSnapshotProbe(
                bool zeroHpDuringRecovery,
                bool cleanupCompleted)
            {
                this.zeroHpDuringRecovery = zeroHpDuringRecovery;
                this.cleanupCompleted = cleanupCompleted;
                Name = "LateRuntimeSnapshotProbe";
                ObjectId = 1;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 100;
                Health.HPBound = 100;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                var frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 1,
                    next = 0,
                    centerx = 0,
                    centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)LF2ObjectType.Character,
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            internal override void RunPreCollisionRecoveryPhase(int tickIndex)
            {
                RecoveryCount++;
                if (zeroHpDuringRecovery)
                    Health.HP = 0;
            }

            public override void SimFrameTick(int tickIndex)
            {
                FrameTickCount++;
                FrameTickObservedHp = Runtime.HP;
            }

            internal override void RunLateDeathOpointPreCleanupPhase()
            {
                DeathOpointCount++;
                DeathOpointObservedHp = Runtime.HP;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                CleanupCount++;
                return cleanupCompleted;
            }

            internal override void RunLateTailBeforePrevFrame()
            {
                TailCount++;
            }

            public override void Reset()
            {
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }

        private sealed class LateRuntimeSnapshotWeaponProbe : LF2Weapon
        {
            internal bool PendingDestroyObserved { get; private set; }

            internal void BindData()
            {
                Name = "LateRuntimeSnapshotDepletedWeapon";
                ObjectId = 100;
                SetWeaponType((int)LF2ObjectType.LightWeapon);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                var frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 100,
                    next = 0,
                    centerx = 0,
                    centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = 100,
                        weapon_hp = 1,
                        weapon_broken_sound = "LateSnapshot_Depleted",
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 1;
                Health.HPBound = 1;
                Runtime.WeaponFlightCounter = -1;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                bool completed = base.TryRunLatePostOpointCleanupPhase();
                PendingDestroyObserved |= Runtime.PendingFlushDestroy;
                return completed;
            }
        }
#endif
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.DetailTimingDiagnostics.cs ---
using System;
using System.Diagnostics;
using System.Threading;

namespace NTSD.Simulation
{
    public enum BattleTickDetailPhase
    {
        CharacterInputSnapshotBuild = 0,
        CharacterInputEntityInputPass = 1,
        CharacterInputSnapshotClear = 2,
        LateEntityStateSpecial = 3,
        LateEntityRecovery = 4,
        LateEntityFrameTick = 5,
        LateEntityCollision = 6,
        LateEntityFrameExit = 7,
        LateEntityDeathOpoint = 8,
        LateEntityOpointProcess = 9,
        LateEntityCleanup = 10,
        LateEntityTailAndQueuedFlush = 11,
        LateEntityPrevFrameMirror = 12,
        LateEntityFinalPendingFlush = 13,
        RenderPresentationOrder = 14,
        RenderBeginFrame = 15,
        RenderPrepareFrameAndLegacyCapacityGuard = 16,
        RenderLateRendererUpdate = 17,
        RenderBeginFrameSortEntities = 18,
        RenderBeginFrameCaptureHitRecords = 19,
        RenderBeginFrameCaptureEntities = 20,
        RenderBeginFrameBuildCommands = 21,
        RenderPrepareFrameFrozenFrameCopy = 22,
        RenderPrepareFrameResolveCommands = 23,
        RenderPrepareFrameWriteQuads = 24,
        RenderPrepareFrameSetVertexBufferData = 25,
        RenderPrepareFrameSetSubMeshes = 26,
        RenderExecuteCommandBuffer = 27,
        Count = 28,
    }

    /// <summary>
    /// Stable LateEntityUpdate call-site markers for the runtime snapshot diagnostic.
    /// These name pass positions rather than source-code line numbers so reports remain
    /// comparable when the implementation moves.
    /// </summary>
    public enum BattleLateRuntimeSnapshotStage
    {
        StateSpecial = 0,
        Recovery = 1,
        FrameTickSuppressed = 2,
        FrameTick = 3,
        FrameExit = 4,
        DeathOpoint = 5,
        CleanupCompleted = 6,
        TailAndQueuedFlush = 7,
        PrevFrameMirror = 8,
        Count = 9,
    }

    public sealed class BattleTickDetailPhaseDiagnostics
    {
        private readonly long[] elapsedTimestampTicks =
            new long[(int)BattleTickDetailPhase.Count];
        private readonly long[] deferredRenderElapsedTimestampTicks =
            new long[(int)BattleTickDetailPhase.Count];
        private readonly BattleTickDetailPhase[] activePhases =
            new BattleTickDetailPhase[8];
        private readonly long[] activePhaseTimestamps = new long[8];
        private readonly long[] lateRuntimeSnapshotElapsedTimestampTicks =
            new long[(int)BattleLateRuntimeSnapshotStage.Count];
        private readonly long[] lateRuntimeSnapshotCallCounts =
            new long[(int)BattleLateRuntimeSnapshotStage.Count];
        private readonly BattleLateRuntimeSnapshotStage[] activeLateRuntimeSnapshotStages =
            new BattleLateRuntimeSnapshotStage[4];
        private readonly long[] activeLateRuntimeSnapshotTimestamps = new long[4];
        private int activePhaseDepth;
        private int activeLateRuntimeSnapshotDepth;
        private int deferredRenderMaterializationDepth;

        public static int PhaseCount => (int)BattleTickDetailPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastTickIndex { get; private set; } = -1;

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            activePhaseDepth = 0;
            LastTickIndex = -1;
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
            Array.Clear(
                lateRuntimeSnapshotElapsedTimestampTicks,
                0,
                lateRuntimeSnapshotElapsedTimestampTicks.Length);
            Array.Clear(lateRuntimeSnapshotCallCounts, 0, lateRuntimeSnapshotCallCounts.Length);
            Array.Clear(
                activeLateRuntimeSnapshotTimestamps,
                0,
                activeLateRuntimeSnapshotTimestamps.Length);
            deferredRenderMaterializationDepth = 0;
            activeLateRuntimeSnapshotDepth = 0;
            for (int index = 0; index < deferredRenderElapsedTimestampTicks.Length; index++)
                Interlocked.Exchange(ref deferredRenderElapsedTimestampTicks[index], 0);
        }

        public void BeginTick(int tickIndex)
        {
            if (!Enabled)
                return;

            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
            Array.Clear(
                lateRuntimeSnapshotElapsedTimestampTicks,
                0,
                lateRuntimeSnapshotElapsedTimestampTicks.Length);
            Array.Clear(lateRuntimeSnapshotCallCounts, 0, lateRuntimeSnapshotCallCounts.Length);
            Array.Clear(
                activeLateRuntimeSnapshotTimestamps,
                0,
                activeLateRuntimeSnapshotTimestamps.Length);
            activePhaseDepth = 0;
            activeLateRuntimeSnapshotDepth = 0;
            deferredRenderMaterializationDepth = 0;
            for (int index = 0; index < deferredRenderElapsedTimestampTicks.Length; index++)
            {
                elapsedTimestampTicks[index] =
                    Interlocked.Exchange(ref deferredRenderElapsedTimestampTicks[index], 0);
            }
            LastTickIndex = tickIndex;
        }

        public bool BeginDeferredRenderMaterialization()
        {
            if (!Enabled)
                return false;

            deferredRenderMaterializationDepth++;
            return true;
        }

        public void EndDeferredRenderMaterialization()
        {
            if (!Enabled || deferredRenderMaterializationDepth <= 0)
                return;

            deferredRenderMaterializationDepth--;
        }

        public void BeginPhase(BattleTickDetailPhase phase)
        {
            if (!Enabled || (uint)phase >= (uint)BattleTickDetailPhase.Count ||
                activePhaseDepth >= activePhases.Length)
            {
                return;
            }

            activePhases[activePhaseDepth] = phase;
            activePhaseTimestamps[activePhaseDepth] = Stopwatch.GetTimestamp();
            activePhaseDepth++;
        }

        public void EndPhase(BattleTickDetailPhase phase)
        {
            if (!Enabled || activePhaseDepth == 0 ||
                activePhases[activePhaseDepth - 1] != phase)
            {
                return;
            }

            activePhaseDepth--;
            long elapsed = Stopwatch.GetTimestamp() -
                           activePhaseTimestamps[activePhaseDepth];
            if (deferredRenderMaterializationDepth > 0 &&
                IsDeferredMaterializationPhase(phase))
            {
                Interlocked.Add(
                    ref deferredRenderElapsedTimestampTicks[(int)phase],
                    elapsed);
            }
            else
            {
                elapsedTimestampTicks[(int)phase] += elapsed;
            }
            activePhaseTimestamps[activePhaseDepth] = 0;
        }

        public void RecordDeferredPhaseElapsed(
            BattleTickDetailPhase phase,
            long elapsedTicks)
        {
            if (!Enabled || phase != BattleTickDetailPhase.RenderExecuteCommandBuffer ||
                elapsedTicks <= 0)
            {
                return;
            }

            Interlocked.Add(
                ref deferredRenderElapsedTimestampTicks[(int)phase],
                elapsedTicks);
        }

        public long GetLastElapsedTimestampTicks(BattleTickDetailPhase phase)
        {
            return (uint)phase < (uint)BattleTickDetailPhase.Count
                ? elapsedTimestampTicks[(int)phase]
                : 0;
        }

        public long GetLastPhaseSumTimestampTicks()
        {
            long sum = 0;
            for (int i = 0; i < elapsedTimestampTicks.Length; i++)
                sum += elapsedTimestampTicks[i];
            return sum;
        }

        public void BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage stage)
        {
            if (!Enabled || (uint)stage >= (uint)BattleLateRuntimeSnapshotStage.Count ||
                activeLateRuntimeSnapshotDepth >= activeLateRuntimeSnapshotStages.Length)
            {
                return;
            }

            lateRuntimeSnapshotCallCounts[(int)stage]++;
            activeLateRuntimeSnapshotStages[activeLateRuntimeSnapshotDepth] = stage;
            activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth] =
                Stopwatch.GetTimestamp();
            activeLateRuntimeSnapshotDepth++;
        }

        public void EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage stage)
        {
            if (!Enabled || activeLateRuntimeSnapshotDepth == 0 ||
                activeLateRuntimeSnapshotStages[activeLateRuntimeSnapshotDepth - 1] != stage)
            {
                return;
            }

            activeLateRuntimeSnapshotDepth--;
            lateRuntimeSnapshotElapsedTimestampTicks[(int)stage] += Stopwatch.GetTimestamp() -
                activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth];
            activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth] = 0;
        }

        public long GetLastLateRuntimeSnapshotElapsedTimestampTicks(
            BattleLateRuntimeSnapshotStage stage)
        {
            return (uint)stage < (uint)BattleLateRuntimeSnapshotStage.Count
                ? lateRuntimeSnapshotElapsedTimestampTicks[(int)stage]
                : 0;
        }

        public long GetLastLateRuntimeSnapshotCallCount(BattleLateRuntimeSnapshotStage stage)
        {
            return (uint)stage < (uint)BattleLateRuntimeSnapshotStage.Count
                ? lateRuntimeSnapshotCallCounts[(int)stage]
                : 0;
        }

        public static string GetLateRuntimeSnapshotStageName(
            BattleLateRuntimeSnapshotStage stage)
        {
            switch (stage)
            {
                case BattleLateRuntimeSnapshotStage.StateSpecial:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/StateSpecial";
                case BattleLateRuntimeSnapshotStage.Recovery:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/Recovery";
                case BattleLateRuntimeSnapshotStage.FrameTickSuppressed:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/FrameTickSuppressed";
                case BattleLateRuntimeSnapshotStage.FrameTick:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/FrameTick";
                case BattleLateRuntimeSnapshotStage.FrameExit:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/FrameExit";
                case BattleLateRuntimeSnapshotStage.DeathOpoint:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/DeathOpoint";
                case BattleLateRuntimeSnapshotStage.CleanupCompleted:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/CleanupCompleted";
                case BattleLateRuntimeSnapshotStage.TailAndQueuedFlush:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/TailAndQueuedFlush";
                case BattleLateRuntimeSnapshotStage.PrevFrameMirror:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/PrevFrameMirror";
                default:
                    return string.Empty;
            }
        }

        public static string GetPhaseName(BattleTickDetailPhase phase)
        {
            switch (phase)
            {
                case BattleTickDetailPhase.CharacterInputSnapshotBuild:
                    return "CharacterInput/SnapshotBuild";
                case BattleTickDetailPhase.CharacterInputEntityInputPass:
                    return "CharacterInput/EntityInputPass";
                case BattleTickDetailPhase.CharacterInputSnapshotClear:
                    return "CharacterInput/SnapshotClear";
                case BattleTickDetailPhase.LateEntityStateSpecial:
                    return "LateEntityUpdate/StateSpecial";
                case BattleTickDetailPhase.LateEntityRecovery:
                    return "LateEntityUpdate/Recovery";
                case BattleTickDetailPhase.LateEntityFrameTick:
                    return "LateEntityUpdate/FrameTick";
                case BattleTickDetailPhase.LateEntityCollision:
                    return "LateEntityUpdate/EntityCollision";
                case BattleTickDetailPhase.LateEntityFrameExit:
                    return "LateEntityUpdate/FrameExit";
                case BattleTickDetailPhase.LateEntityDeathOpoint:
                    return "LateEntityUpdate/DeathOpoint";
                case BattleTickDetailPhase.LateEntityOpointProcess:
                    return "LateEntityUpdate/OpointProcess";
                case BattleTickDetailPhase.LateEntityCleanup:
                    return "LateEntityUpdate/Cleanup";
                case BattleTickDetailPhase.LateEntityTailAndQueuedFlush:
                    return "LateEntityUpdate/TailAndQueuedFlush";
                case BattleTickDetailPhase.LateEntityPrevFrameMirror:
                    return "LateEntityUpdate/PrevFrameMirror";
                case BattleTickDetailPhase.LateEntityFinalPendingFlush:
                    return "LateEntityUpdate/FinalPendingFlush";
                case BattleTickDetailPhase.RenderPresentationOrder:
                    return "Render/PresentationOrder";
                case BattleTickDetailPhase.RenderBeginFrame:
                    return "Render/BeginFrame";
                case BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard:
                    return "Render/PrepareFrame/LegacyCapacityGuard";
                case BattleTickDetailPhase.RenderLateRendererUpdate:
                    return "Render/LateRendererUpdate";
                case BattleTickDetailPhase.RenderBeginFrameSortEntities:
                    return "Render/BeginFrame/SortEntities";
                case BattleTickDetailPhase.RenderBeginFrameCaptureHitRecords:
                    return "Render/BeginFrame/CaptureHitRecords";
                case BattleTickDetailPhase.RenderBeginFrameCaptureEntities:
                    return "Render/BeginFrame/CaptureEntities";
                case BattleTickDetailPhase.RenderBeginFrameBuildCommands:
                    return "Render/BeginFrame/BuildCommands";
                case BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy:
                    return "Render/PrepareFrame/FrozenFrameCopy";
                case BattleTickDetailPhase.RenderPrepareFrameResolveCommands:
                    return "Render/PrepareFrame/ResolveCommands";
                case BattleTickDetailPhase.RenderPrepareFrameWriteQuads:
                    return "Render/PrepareFrame/WriteQuads";
                case BattleTickDetailPhase.RenderPrepareFrameSetVertexBufferData:
                    return "Render/PrepareFrame/SetVertexBufferData";
                case BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes:
                    return "Render/PrepareFrame/SetSubMeshes";
                case BattleTickDetailPhase.RenderExecuteCommandBuffer:
                    return "Render/ExecuteCommandBuffer";
                default:
                    return string.Empty;
            }
        }

        private static bool IsDeferredMaterializationPhase(BattleTickDetailPhase phase)
        {
            return phase == BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard ||
                   phase >= BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy &&
                   phase <= BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes;
        }
    }

    public enum BattleAiInputDetailPhase
    {
        SnapshotSlotSnapshot = 0,
        SnapshotIndexBuild = 1,
        SnapshotQuadtreeSync = 2,
        FindNearestGround = 3,
        FindNearestAir = 4,
        RemainingAiDecision = 5,
        InputStateSyncFromRuntime = 6,
        ComboUpdate = 7,
        RefreshRuntimeSnapshot = 8,
        Count = 9,
    }

    /// <summary>
    /// Per-CharacterInput diagnostic recorder. It is independent from the top-level
    /// detail phase recorder so AI sub-phases can nest without replacing its active phase.
    /// </summary>
    public sealed class BattleAiInputDetailDiagnostics
    {
        public const int RadiusHistogramBucketCount = 9;

        private readonly long[] elapsedTimestampTicks =
            new long[(int)BattleAiInputDetailPhase.Count];
        private readonly BattleAiInputDetailPhase[] activePhases =
            new BattleAiInputDetailPhase[4];
        private readonly long[] activePhaseTimestamps = new long[4];
        private readonly long[] radiusHistogram = new long[RadiusHistogramBucketCount];
        private int activePhaseDepth;

        public static int PhaseCount => (int)BattleAiInputDetailPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastTickIndex { get; private set; } = -1;
        public long AiCount { get; private set; }
        public long SpatialQueryCount { get; private set; }
        public long QueriedHandleVisits { get; private set; }
        public long CandidateVisits { get; private set; }
        public long RadiusExpansions { get; private set; }
        public long BruteFallbackCount { get; private set; }
        public long BruteSlotVisits { get; private set; }
        public long Phase1ListVisits { get; private set; }
        public long RefreshCount { get; private set; }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            Reset(-1);
        }

        public void BeginTick(int tickIndex)
        {
            if (Enabled)
                Reset(tickIndex);
        }

        public void BeginPhase(BattleAiInputDetailPhase phase)
        {
            if (!Enabled || (uint)phase >= (uint)BattleAiInputDetailPhase.Count ||
                activePhaseDepth >= activePhases.Length)
            {
                return;
            }

            activePhases[activePhaseDepth] = phase;
            activePhaseTimestamps[activePhaseDepth] = Stopwatch.GetTimestamp();
            activePhaseDepth++;
        }

        public void EndPhase(BattleAiInputDetailPhase phase)
        {
            if (!Enabled || activePhaseDepth == 0 ||
                activePhases[activePhaseDepth - 1] != phase)
            {
                return;
            }

            activePhaseDepth--;
            elapsedTimestampTicks[(int)phase] +=
                Stopwatch.GetTimestamp() - activePhaseTimestamps[activePhaseDepth];
            activePhaseTimestamps[activePhaseDepth] = 0;
        }

        public long GetLastElapsedTimestampTicks(BattleAiInputDetailPhase phase)
        {
            return (uint)phase < (uint)BattleAiInputDetailPhase.Count
                ? elapsedTimestampTicks[(int)phase]
                : 0;
        }

        public long GetRadiusHistogramValue(int index)
        {
            return (uint)index < (uint)radiusHistogram.Length ? radiusHistogram[index] : 0;
        }

        public static string GetPhaseName(BattleAiInputDetailPhase phase)
        {
            switch (phase)
            {
                case BattleAiInputDetailPhase.SnapshotSlotSnapshot:
                    return "CharacterInput/AI/SnapshotSlotSnapshot";
                case BattleAiInputDetailPhase.SnapshotIndexBuild:
                    return "CharacterInput/AI/SnapshotIndexBuild";
                case BattleAiInputDetailPhase.SnapshotQuadtreeSync:
                    return "CharacterInput/AI/SnapshotQuadtreeSync";
                case BattleAiInputDetailPhase.FindNearestGround:
                    return "CharacterInput/AI/FindNearestGround";
                case BattleAiInputDetailPhase.FindNearestAir:
                    return "CharacterInput/AI/FindNearestAir";
                case BattleAiInputDetailPhase.RemainingAiDecision:
                    return "CharacterInput/AI/RemainingAiDecision";
                case BattleAiInputDetailPhase.InputStateSyncFromRuntime:
                    return "CharacterInput/AI/InputStateSyncFromRuntime";
                case BattleAiInputDetailPhase.ComboUpdate:
                    return "CharacterInput/AI/ComboUpdate";
                case BattleAiInputDetailPhase.RefreshRuntimeSnapshot:
                    return "CharacterInput/AI/RefreshRuntimeSnapshot";
                default:
                    return string.Empty;
            }
        }

        public void RecordAi() { if (Enabled) AiCount++; }
        public void RecordSpatialQuery() { if (Enabled) SpatialQueryCount++; }
        public void RecordQueriedHandleVisits(int count) { if (Enabled) QueriedHandleVisits += count; }
        public void RecordCandidateVisits(int count) { if (Enabled) CandidateVisits += count; }
        public void RecordRadius(int radius)
        {
            if (!Enabled)
                return;
            int bucket = radius <= 64 ? 0 : radius <= 128 ? 1 : radius <= 256 ? 2 :
                radius <= 512 ? 3 : radius <= 1024 ? 4 : radius <= 2048 ? 5 :
                radius <= 4096 ? 6 : radius <= 8192 ? 7 : 8;
            radiusHistogram[bucket]++;
        }
        public void RecordRadiusExpansion() { if (Enabled) RadiusExpansions++; }
        public void RecordBruteFallback() { if (Enabled) BruteFallbackCount++; }
        public void RecordBruteSlotVisits(int count) { if (Enabled) BruteSlotVisits += count; }
        public void RecordPhase1ListVisits(int count) { if (Enabled) Phase1ListVisits += count; }
        public void RecordRefresh() { if (Enabled) RefreshCount++; }

        private void Reset(int tickIndex)
        {
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
            Array.Clear(radiusHistogram, 0, radiusHistogram.Length);
            activePhaseDepth = 0;
            LastTickIndex = tickIndex;
            AiCount = 0;
            SpatialQueryCount = 0;
            QueriedHandleVisits = 0;
            CandidateVisits = 0;
            RadiusExpansions = 0;
            BruteFallbackCount = 0;
            BruteSlotVisits = 0;
            Phase1ListVisits = 0;
            RefreshCount = 0;
        }
    }

    public partial class SimulationWorld
    {
        private BattleTickDetailPhaseDiagnostics battleTickDetailPhaseDiagnostics;
        private BattleAiInputDetailDiagnostics battleAiInputDetailDiagnostics;

        public bool BattleTickDetailPhaseDiagnosticsAllocatedForDiagnostics =>
            battleTickDetailPhaseDiagnostics != null;

        public BattleTickDetailPhaseDiagnostics ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics =>
            battleTickDetailPhaseDiagnostics != null &&
            battleTickDetailPhaseDiagnostics.Enabled
                ? battleTickDetailPhaseDiagnostics
                : null;

        public BattleTickDetailPhaseDiagnostics EnableBattleTickDetailPhaseDiagnosticsForDiagnostics()
        {
            if (battleTickDetailPhaseDiagnostics == null)
                battleTickDetailPhaseDiagnostics = new BattleTickDetailPhaseDiagnostics();
            battleTickDetailPhaseDiagnostics.SetEnabled(true);
            return battleTickDetailPhaseDiagnostics;
        }

        public void DisableBattleTickDetailPhaseDiagnosticsForDiagnostics()
        {
            battleTickDetailPhaseDiagnostics?.SetEnabled(false);
        }

        public bool BattleAiInputDetailDiagnosticsAllocatedForDiagnostics =>
            battleAiInputDetailDiagnostics != null;

        public BattleAiInputDetailDiagnostics ActiveBattleAiInputDetailDiagnosticsForDiagnostics =>
            battleAiInputDetailDiagnostics != null && battleAiInputDetailDiagnostics.Enabled
                ? battleAiInputDetailDiagnostics
                : null;

        public BattleAiInputDetailDiagnostics EnableBattleAiInputDetailDiagnosticsForDiagnostics()
        {
            if (battleAiInputDetailDiagnostics == null)
                battleAiInputDetailDiagnostics = new BattleAiInputDetailDiagnostics();
            battleAiInputDetailDiagnostics.SetEnabled(true);
            return battleAiInputDetailDiagnostics;
        }

        public void DisableBattleAiInputDetailDiagnosticsForDiagnostics()
        {
            battleAiInputDetailDiagnostics?.SetEnabled(false);
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs ---
using System;
using System.Diagnostics;

namespace NTSD.Simulation
{
    public enum BattleTickPhase
    {
        BattleFlow = 0,
        Cooldown = 1,
        HumanInput = 2,
        RuntimeMaintenance = 3,
        InputClear = 4,
        CharacterInput = 5,
        EarlyFrameAdvance = 6,
        FrameLogic = 7,
        FrameAdvance = 8,
        DeathCleanup = 9,
        StageBounds = 10,
        PreInteraction = 11,
        HeldLinkValidation = 12,
        HeldProcess = 13,
        CollisionSnapshot = 14,
        PairVRest = 15,
        CandidateCollect = 16,
        CharacterHitConsumePostInteraction = 17,
        RandomWeaponDrop = 18,
        ObjectHitConsume = 19,
        CandidateConsumptionEnd = 20,
        PreFrameBounds = 21,
        Stage = 22,
        RenderDispatch = 23,
        FramePostProcess = 24,
        LateEntityUpdate = 25,
        RandomWeaponDropTail = 26,
        EntityPostFrameTail = 27,
        BattleResults = 28,
        Count = 29,
    }

    public sealed class BattleTickPhaseDiagnostics
    {
        private readonly long[] elapsedTimestampTicks = new long[(int)BattleTickPhase.Count];
        private BattleTickPhase activePhase = BattleTickPhase.Count;
        private long activePhaseTimestamp;

        public static int PhaseCount => (int)BattleTickPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastTickIndex { get; private set; } = -1;

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            activePhase = BattleTickPhase.Count;
            activePhaseTimestamp = 0;
            LastTickIndex = -1;
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
        }

        public void BeginTick(int tickIndex)
        {
            if (!Enabled)
                return;

            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            activePhase = BattleTickPhase.Count;
            activePhaseTimestamp = 0;
            LastTickIndex = tickIndex;
        }

        public void BeginPhase(BattleTickPhase phase)
        {
            if (!Enabled || (uint)phase >= (uint)BattleTickPhase.Count)
                return;

            activePhase = phase;
            activePhaseTimestamp = Stopwatch.GetTimestamp();
        }

        public void EndPhase(BattleTickPhase phase)
        {
            if (!Enabled || activePhase != phase)
                return;

            elapsedTimestampTicks[(int)phase] += Stopwatch.GetTimestamp() - activePhaseTimestamp;
            activePhase = BattleTickPhase.Count;
            activePhaseTimestamp = 0;
        }

        public long GetLastElapsedTimestampTicks(BattleTickPhase phase)
        {
            return (uint)phase < (uint)BattleTickPhase.Count
                ? elapsedTimestampTicks[(int)phase]
                : 0;
        }

        public long GetLastPhaseSumTimestampTicks()
        {
            long sum = 0;
            for (int i = 0; i < elapsedTimestampTicks.Length; i++)
                sum += elapsedTimestampTicks[i];
            return sum;
        }

        public static string GetPhaseName(BattleTickPhase phase)
        {
            switch (phase)
            {
                case BattleTickPhase.BattleFlow: return "BattleFlow";
                case BattleTickPhase.Cooldown: return "Cooldown";
                case BattleTickPhase.HumanInput: return "HumanInput";
                case BattleTickPhase.RuntimeMaintenance: return "RuntimeMaintenance";
                case BattleTickPhase.InputClear: return "InputClear";
                case BattleTickPhase.CharacterInput: return "CharacterInput";
                case BattleTickPhase.EarlyFrameAdvance: return "EarlyFrameAdvance";
                case BattleTickPhase.FrameLogic: return "FrameLogic";
                case BattleTickPhase.FrameAdvance: return "FrameAdvance";
                case BattleTickPhase.DeathCleanup: return "DeathCleanup";
                case BattleTickPhase.StageBounds: return "StageBounds";
                case BattleTickPhase.PreInteraction: return "PreInteraction";
                case BattleTickPhase.HeldLinkValidation: return "HeldLinkValidation";
                case BattleTickPhase.HeldProcess: return "HeldProcess";
                case BattleTickPhase.CollisionSnapshot: return "CollisionSnapshot";
                case BattleTickPhase.PairVRest: return "PairVRest";
                case BattleTickPhase.CandidateCollect: return "CandidateCollect";
                case BattleTickPhase.CharacterHitConsumePostInteraction:
                    return "CharacterHitConsumePostInteraction";
                case BattleTickPhase.RandomWeaponDrop: return "RandomWeaponDrop";
                case BattleTickPhase.ObjectHitConsume: return "ObjectHitConsume";
                case BattleTickPhase.CandidateConsumptionEnd: return "CandidateConsumptionEnd";
                case BattleTickPhase.PreFrameBounds: return "PreFrameBounds";
                case BattleTickPhase.Stage: return "Stage";
                case BattleTickPhase.RenderDispatch: return "RenderDispatch";
                case BattleTickPhase.FramePostProcess: return "FramePostProcess";
                case BattleTickPhase.LateEntityUpdate: return "LateEntityUpdate";
                case BattleTickPhase.RandomWeaponDropTail: return "RandomWeaponDropTail";
                case BattleTickPhase.EntityPostFrameTail: return "EntityPostFrameTail";
                case BattleTickPhase.BattleResults: return "BattleResults";
                default: return string.Empty;
            }
        }
    }

    public partial class SimulationWorld
    {
        private BattleTickPhaseDiagnostics battleTickPhaseDiagnostics;

        public BattleTickPhaseDiagnostics ActiveBattleTickPhaseDiagnosticsForDiagnostics =>
            battleTickPhaseDiagnostics != null && battleTickPhaseDiagnostics.Enabled
                ? battleTickPhaseDiagnostics
                : null;

        public BattleTickPhaseDiagnostics EnableBattleTickPhaseDiagnosticsForDiagnostics()
        {
            if (battleTickPhaseDiagnostics == null)
                battleTickPhaseDiagnostics = new BattleTickPhaseDiagnostics();
            battleTickPhaseDiagnostics.SetEnabled(true);
            return battleTickPhaseDiagnostics;
        }

        public void DisableBattleTickPhaseDiagnosticsForDiagnostics()
        {
            battleTickPhaseDiagnostics?.SetEnabled(false);
        }
    }

    /// <summary>
    /// Unity NTSD 战斗 tick 调度器。
    /// pass 顺序以 C# authority 工程为基准；实体专属行为保留在 LF2Entity 子类中，
    /// 本类只负责集中维护这些 pass 的执行时机。
    /// </summary>
    public sealed class NTSDBattleTickSystem
    {
        private readonly SimulationWorld world;

        public NTSDBattleTickSystem(SimulationWorld world)
        {
            this.world = world;
        }

        public void RunReleaseTick(int tickIndex)
        {
            RunReleaseTick(tickIndex, buildPresentation: true);
        }

        public void RunReleaseTick(int tickIndex, bool buildPresentation)
        {
            if (world == null) return;

            BattleTickPhaseDiagnostics diagnostics =
                world.ActiveBattleTickPhaseDiagnosticsForDiagnostics;
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            diagnostics?.BeginTick(tickIndex);
            detailDiagnostics?.BeginTick(tickIndex);
            diagnostics?.BeginPhase(BattleTickPhase.BattleFlow);
            if (world.Runtime?.Flow != null)
                world.Runtime.Flow.HumanInputPolledExternally = false;
            world.PendingSounds.Clear();
            world.AdvanceBattleFlowTick(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.BattleFlow);
            if (world.Runtime?.Results?.IsActive == true)
            {
                diagnostics?.BeginPhase(BattleTickPhase.HumanInput);
                PostCooldownHumanInput(tickIndex);
                diagnostics?.EndPhase(BattleTickPhase.HumanInput);
                diagnostics?.BeginPhase(BattleTickPhase.BattleResults);
                BattleResultsFlow();
                diagnostics?.EndPhase(BattleTickPhase.BattleResults);
                return;
            }

            diagnostics?.BeginPhase(BattleTickPhase.Cooldown);
            TickCooldowns(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.Cooldown);
            diagnostics?.BeginPhase(BattleTickPhase.HumanInput);
            PostCooldownHumanInput(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.HumanInput);
            if (!RunFrameAdvancePhase(tickIndex, diagnostics))
                return;
            RunInteractionPhase(tickIndex, diagnostics);
            RunPresentationAndCleanupPhase(tickIndex, buildPresentation, diagnostics);
        }

        private bool RunFrameAdvancePhase(
            int tickIndex,
            BattleTickPhaseDiagnostics diagnostics)
        {
            diagnostics?.BeginPhase(BattleTickPhase.RuntimeMaintenance);
            Oid5152RuntimeMaintenance(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.RuntimeMaintenance);
            if (world.NeedClearInput)
            {
                diagnostics?.BeginPhase(BattleTickPhase.InputClear);
                world.SetNeedClearInput(false);
                world.ClearBattleEntryInputAll();
                diagnostics?.EndPhase(BattleTickPhase.InputClear);
                return false;
            }

            diagnostics?.BeginPhase(BattleTickPhase.CharacterInput);
            CharacterInput(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.CharacterInput);

            diagnostics?.BeginPhase(BattleTickPhase.EarlyFrameAdvance);
            EarlyFrameAdvanceSpecials(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.EarlyFrameAdvance);
            diagnostics?.BeginPhase(BattleTickPhase.FrameLogic);
            FrameLogicBeforeAdvance(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.FrameLogic);
            diagnostics?.BeginPhase(BattleTickPhase.FrameAdvance);
            FrameAdvanceAll(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.FrameAdvance);
            diagnostics?.BeginPhase(BattleTickPhase.DeathCleanup);
            PostFrameAdvanceDeathCleanup(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.DeathCleanup);
            diagnostics?.BeginPhase(BattleTickPhase.StageBounds);
            ClampCharacterZToStageBounds();
            diagnostics?.EndPhase(BattleTickPhase.StageBounds);
            diagnostics?.BeginPhase(BattleTickPhase.PreInteraction);
            ResolvePreInteractions(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.PreInteraction);
            diagnostics?.BeginPhase(BattleTickPhase.HeldLinkValidation);
            ValidateHeldLinks(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.HeldLinkValidation);
            diagnostics?.BeginPhase(BattleTickPhase.StageBounds);
            ClampCharacterZToStageBounds();
            diagnostics?.EndPhase(BattleTickPhase.StageBounds);
            diagnostics?.BeginPhase(BattleTickPhase.HeldProcess);
            ProcessHeldObjects(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.HeldProcess);
            diagnostics?.BeginPhase(BattleTickPhase.CollisionSnapshot);
            CaptureCollisionFrameSnapshots();
            diagnostics?.EndPhase(BattleTickPhase.CollisionSnapshot);
            diagnostics?.BeginPhase(BattleTickPhase.PairVRest);
            TickCollisionPairVRest();
            diagnostics?.EndPhase(BattleTickPhase.PairVRest);
            diagnostics?.BeginPhase(BattleTickPhase.CandidateCollect);
            CollectCollisionCandidates();
            diagnostics?.EndPhase(BattleTickPhase.CandidateCollect);
            return true;
        }

        private void RunInteractionPhase(
            int tickIndex,
            BattleTickPhaseDiagnostics diagnostics)
        {
            diagnostics?.BeginPhase(BattleTickPhase.CharacterHitConsumePostInteraction);
            ResolvePostInteractions(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.CharacterHitConsumePostInteraction);
            diagnostics?.BeginPhase(BattleTickPhase.RandomWeaponDrop);
            RandomWeaponDrop(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.RandomWeaponDrop);
            diagnostics?.BeginPhase(BattleTickPhase.ObjectHitConsume);
            ResolveObjectInteractions(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.ObjectHitConsume);
            diagnostics?.BeginPhase(BattleTickPhase.CandidateConsumptionEnd);
            EndCollisionCandidateConsumption();
            diagnostics?.EndPhase(BattleTickPhase.CandidateConsumptionEnd);
        }

        private void RunPresentationAndCleanupPhase(
            int tickIndex,
            bool buildPresentation,
            BattleTickPhaseDiagnostics diagnostics)
        {
            diagnostics?.BeginPhase(BattleTickPhase.PreFrameBounds);
            PreFrameBounds();
            diagnostics?.EndPhase(BattleTickPhase.PreFrameBounds);
            diagnostics?.BeginPhase(BattleTickPhase.Stage);
            CurrentWaveStage(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.Stage);
            diagnostics?.BeginPhase(BattleTickPhase.RenderDispatch);
            RenderDispatch(tickIndex, buildPresentation);
            diagnostics?.EndPhase(BattleTickPhase.RenderDispatch);
            diagnostics?.BeginPhase(BattleTickPhase.FramePostProcess);
            FramePostProcess();
            diagnostics?.EndPhase(BattleTickPhase.FramePostProcess);
            diagnostics?.BeginPhase(BattleTickPhase.LateEntityUpdate);
            LateEntityUpdate(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.LateEntityUpdate);
            diagnostics?.BeginPhase(BattleTickPhase.RandomWeaponDropTail);
            Mode2RandomWeaponDropTail(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.RandomWeaponDropTail);
            diagnostics?.BeginPhase(BattleTickPhase.EntityPostFrameTail);
            EntityPostFrameTail(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.EntityPostFrameTail);
            diagnostics?.BeginPhase(BattleTickPhase.BattleResults);
            BattleResultsFlow();
            diagnostics?.EndPhase(BattleTickPhase.BattleResults);
        }

        private void TickCooldowns(int tickIndex)
        {
            world.VrestTickAll(tickIndex);
        }

        private void PostCooldownHumanInput(int tickIndex)
        {
            world.PostCooldownHumanInputAll(tickIndex);
            if (world.Runtime?.Flow != null)
                world.Runtime.Flow.HumanInputPolledExternally = true;
        }

        private void CharacterInput(int tickIndex)
        {
            world.CharacterInputAll(tickIndex);
        }

        private void ProcessHeldObjects(int tickIndex)
        {
            world.HeldObjectProcessAll(tickIndex);
        }

        private void Oid5152RuntimeMaintenance(int tickIndex)
        {
            world.Oid5152RuntimeMaintenanceAll(tickIndex);
        }

        private void CaptureCollisionFrameSnapshots()
        {
            world.CaptureCollisionFrameSnapshotsAll();
        }

        private void CollectCollisionCandidates()
        {
            world.CollectCollisionCandidatesAll();
        }

        private void TickCollisionPairVRest()
        {
            world.TickCollisionPairVRestAll();
        }

        private void EndCollisionCandidateConsumption()
        {
            world.EndCollisionCandidateConsumption();
        }

        private void FrameLogicBeforeAdvance(int tickIndex)
        {
            world.FrameLogicBeforeAdvanceAll(tickIndex);
        }

        private void EarlyFrameAdvanceSpecials(int tickIndex)
        {
            world.EarlyFrameAdvanceSpecialsAll(tickIndex);
        }

        private void ResolvePreInteractions(int tickIndex)
        {
            world.PreInteractionTickAll(tickIndex);
        }

        private void FrameAdvanceAll(int tickIndex)
        {
            world.SerialTickAll(tickIndex);
        }

        private void PostFrameAdvanceDeathCleanup(int tickIndex)
        {
            world.PostFrameAdvanceDeathCleanupAll(tickIndex);
        }

        private void RandomWeaponDrop(int tickIndex)
        {
            world.RandomWeaponDropTickAll(tickIndex);
        }

        private void ResolvePostInteractions(int tickIndex)
        {
            world.PostInteractionTickAll(tickIndex);
        }

        private void ResolveObjectInteractions(int tickIndex)
        {
            world.ObjectInteractionTickAll(tickIndex);
        }

        private void ValidateHeldLinks(int tickIndex)
        {
            world.ValidateHeldLinksAll(tickIndex);
        }

        private void ClampCharacterZToStageBounds()
        {
            world.ClampCharacterZToStageBoundsAll();
        }

        private void FramePostProcess()
        {
            world.FramePostProcessAll();
        }

        private void CurrentWaveStage(int tickIndex)
        {
            world.CurrentWaveStageTickAll();
        }

        private void RenderDispatch(int tickIndex, bool buildPresentation)
        {
            world.RenderDispatchAll(tickIndex, buildPresentation);
        }

        private void PreFrameBounds()
        {
            world.ApplyPreFrameBoundsAll();
        }

        private void LateEntityUpdate(int tickIndex)
        {
            world.LateEntityUpdateAll(tickIndex);
        }

        private void Mode2RandomWeaponDropTail(int tickIndex)
        {
            world.Mode2RandomWeaponDropTailAll(tickIndex);
        }

        private void EntityPostFrameTail(int tickIndex)
        {
            world.EntityPostFrameTailAll(tickIndex);
        }

        private void BattleResultsFlow()
        {
            world.UpdateBattleResultsFlow();
        }
    }
}


--- File: Temp/NTSD_ProductionEntityStress.dispersed-full-ai-occupancy-epoch-detail-20260726.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "inputMode": "ai",
    "startedUtc": "2026-07-25T20:38:29.7618466Z",
    "updatedUtc": "2026-07-25T20:39:00.5634323Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-full-ai-occupancy-epoch-detail-20260726.json",
    "failure": "",
    "harnessValidity": true,
    "performanceVerdict": "EvidenceOnlyNoThreshold",
    "requestedEntityCount": 1000,
    "selectedCharacterOid": 1,
    "totalEntitiesCreated": 1000,
    "lifecycleReplacements": 0,
    "activeGameObjectCount": 0,
    "stressRootChildCount": 0,
    "worldObjectCount": 0,
    "worldEntityCount": 0,
    "peakWorldEntityCount": 1000,
    "claimedRuntimeSlotCount": 0,
    "runtimeProfile": "MobileExtended",
    "runtimeSlotCapacity": 1050,
    "broadphaseBackend": "LooseQuadtree",
    "formalCollectorRequestedMode": "configured",
    "formalCollectorMode": "role",
    "formalCollectorBodyEntries": 1971,
    "formalCollectorItrQueries": 9,
    "logicTicksExecuted": 432,
    "warmupTicksCompleted": 30,
    "sampledLogicTicks": 402,
    "sampledUnityFrames": 108,
    "framesWithCatchUp": 108,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 374,
    "aiControlledEntityTicks": 432000,
    "collisionCandidateCountSum": 17529,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 428594,
    "broadphasePairCountPeak": 23262,
    "broadphaseFallbackParticipantPeak": 154,
    "broadphaseAbortedTicks": 0,
    "broadphaseLastIndexedCount": 999,
    "damageStatTotal": 0,
    "killStatTotal": 0,
    "opointCounterAvailable": true,
    "observedOpointCreates": 0,
    "opointCounterReason": "Runtime-derived observable proxy: unique active non-harness runtime handles observed after each logic tick. It is not a production opoint creation counter.",
    "logicTickMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick",
        "unavailableReason": "",
        "sampleCount": 402,
        "average": 53.48298805970149,
        "maximum": 139.9207,
        "p95": 92.47510499999999,
        "p99": 115.29652500000003
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 108,
        "average": 266.21687715804139,
        "maximum": 579.4619917869568,
        "p95": 406.6514641046524,
        "p99": 480.2005034685132
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 402,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": true,
    "phaseTimingSource": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
    "phaseTimings": [
        {
            "phase": "BattleFlow",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0010741293532338312,
                "maximum": 0.0039000000000000004,
                "p95": 0.0019,
                "p99": 0.0022
            }
        },
        {
            "phase": "Cooldown",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.041157213930349,
                "maximum": 2.9264,
                "p95": 1.9344050000000002,
                "p99": 2.6221150000000006
            }
        },
        {
            "phase": "HumanInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.19641467661691548,
                "maximum": 0.4506,
                "p95": 0.3014299999999999,
                "p99": 0.3980200000000001
            }
        },
        {
            "phase": "RuntimeMaintenance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.05761840796019901,
                "maximum": 0.17850000000000003,
                "p95": 0.078495,
                "p99": 0.11521400000000018
            }
        },
        {
            "phase": "InputClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "phase": "CharacterInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 17.918694029850756,
                "maximum": 49.575,
                "p95": 35.097485,
                "p99": 42.42759600000002
            }
        },
        {
            "phase": "EarlyFrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.9614689054726372,
                "maximum": 3.0483000000000004,
                "p95": 1.7948399999999998,
                "p99": 2.1976300000000017
            }
        },
        {
            "phase": "FrameLogic",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.22708830845771156,
                "maximum": 0.8163,
                "p95": 0.31699,
                "p99": 0.5156120000000003
            }
        },
        {
            "phase": "FrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 2.5995485074626889,
                "maximum": 7.4305,
                "p95": 5.301484999999999,
                "p99": 6.469051000000001
            }
        },
        {
            "phase": "DeathCleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.20947587064676627,
                "maximum": 0.8086,
                "p95": 0.33760999999999999,
                "p99": 0.4937870000000001
            }
        },
        {
            "phase": "StageBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.9023835820895528,
                "maximum": 6.2605,
                "p95": 3.197095,
                "p99": 4.330148
            }
        },
        {
            "phase": "PreInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 2.566824129353235,
                "maximum": 15.6796,
                "p95": 3.8774949999999994,
                "p99": 6.543063000000001
            }
        },
        {
            "phase": "HeldLinkValidation",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.09827537313432837,
                "maximum": 0.7260000000000001,
                "p95": 0.129695,
                "p99": 0.16707100000000003
            }
        },
        {
            "phase": "HeldProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.08708955223880595,
                "maximum": 0.7841,
                "p95": 0.12494999999999999,
                "p99": 0.17996300000000013
            }
        },
        {
            "phase": "CollisionSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.8191855721393035,
                "maximum": 2.9925,
                "p95": 1.2411949999999999,
                "p99": 1.9401380000000006
            }
        },
        {
            "phase": "PairVRest",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.16715696517412929,
                "maximum": 0.4939,
                "p95": 0.262905,
                "p99": 0.3665160000000002
            }
        },
        {
            "phase": "CandidateCollect",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 4.679899502487569,
                "maximum": 14.211500000000001,
                "p95": 9.43179,
                "p99": 11.552324000000004
            }
        },
        {
            "phase": "CharacterHitConsumePostInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.7418457711442784,
                "maximum": 5.4245,
                "p95": 3.2005049999999999,
                "p99": 3.8829120000000008
            }
        },
        {
            "phase": "RandomWeaponDrop",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.2598788557213928,
                "maximum": 0.8236,
                "p95": 0.41317,
                "p99": 0.5454160000000001
            }
        },
        {
            "phase": "ObjectHitConsume",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.3302853233830846,
                "maximum": 3.3800000000000005,
                "p95": 0.43939,
                "p99": 0.6185480000000002
            }
        },
        {
            "phase": "CandidateConsumptionEnd",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0026194029850746278,
                "maximum": 0.0108,
                "p95": 0.005200000000000001,
                "p99": 0.007998000000000002
            }
        },
        {
            "phase": "PreFrameBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.4649027363184082,
                "maximum": 3.5058000000000004,
                "p95": 2.53023,
                "p99": 3.1513750000000027
            }
        },
        {
            "phase": "Stage",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0008783582089552245,
                "maximum": 0.0027,
                "p95": 0.0017000000000000002,
                "p99": 0.0022
            }
        },
        {
            "phase": "RenderDispatch",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 4.250592039800996,
                "maximum": 35.2857,
                "p95": 15.957875,
                "p99": 19.488888
            }
        },
        {
            "phase": "FramePostProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.7921340796019899,
                "maximum": 3.0496000000000005,
                "p95": 1.4683899999999997,
                "p99": 2.565751000000001
            }
        },
        {
            "phase": "LateEntityUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 10.130267164179106,
                "maximum": 43.8356,
                "p95": 19.985745,
                "p99": 27.62425500000001
            }
        },
        {
            "phase": "RandomWeaponDropTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0006848258706467659,
                "maximum": 0.0019,
                "p95": 0.0012000000000000002,
                "p99": 0.0016
            }
        },
        {
            "phase": "EntityPostFrameTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.8947995024875624,
                "maximum": 2.7670000000000005,
                "p95": 1.7088399999999998,
                "p99": 2.258471000000001
            }
        },
        {
            "phase": "BattleResults",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0008746268656716425,
                "maximum": 0.0032,
                "p95": 0.0016,
                "p99": 0.002399000000000001
            }
        }
    ],
    "phaseTimingUnattributedMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings",
        "unavailableReason": "",
        "sampleCount": 402,
        "average": 0.07987064676616634,
        "maximum": 0.29849999999999,
        "p95": 0.15384500000000399,
        "p99": 0.23765899999999333
    },
    "detailPhaseTimingEnabled": true,
    "detailPhaseTimingSource": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
    "detailPhaseTimingUnavailableReason": "",
    "detailPhaseTimings": [
        {
            "phase": "CharacterInput/SnapshotBuild",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 3.0691278606965146,
                "maximum": 11.5494,
                "p95": 4.39523,
                "p99": 4.74894
            }
        },
        {
            "phase": "CharacterInput/EntityInputPass",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 14.837261442786069,
                "maximum": 45.845600000000008,
                "p95": 30.84495,
                "p99": 35.697596000000007
            }
        },
        {
            "phase": "CharacterInput/SnapshotClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.007078606965174127,
                "maximum": 0.0328,
                "p95": 0.0119,
                "p99": 0.017488000000000015
            }
        },
        {
            "phase": "LateEntityUpdate/StateSpecial",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.1532410447761193,
                "maximum": 0.4572,
                "p95": 0.32721500000000006,
                "p99": 0.3617410000000001
            }
        },
        {
            "phase": "LateEntityUpdate/Recovery",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.4182527363184081,
                "maximum": 1.6675,
                "p95": 0.8291749999999999,
                "p99": 1.1422370000000007
            }
        },
        {
            "phase": "LateEntityUpdate/FrameTick",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 4.1264221393034818,
                "maximum": 16.9826,
                "p95": 9.847475000000001,
                "p99": 12.846647000000005
            }
        },
        {
            "phase": "LateEntityUpdate/EntityCollision",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.06077487562189056,
                "maximum": 0.1549,
                "p95": 0.087795,
                "p99": 0.09808500000000002
            }
        },
        {
            "phase": "LateEntityUpdate/FrameExit",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.05440771144278609,
                "maximum": 0.1082,
                "p95": 0.086185,
                "p99": 0.09499600000000001
            }
        },
        {
            "phase": "LateEntityUpdate/DeathOpoint",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.8622830845771142,
                "maximum": 12.275,
                "p95": 1.067585,
                "p99": 1.1678480000000003
            }
        },
        {
            "phase": "LateEntityUpdate/OpointProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 2.6549738805970146,
                "maximum": 27.963700000000004,
                "p95": 5.926844999999998,
                "p99": 7.149727000000001
            }
        },
        {
            "phase": "LateEntityUpdate/Cleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.1967679104477611,
                "maximum": 0.3653,
                "p95": 0.29350000000000006,
                "p99": 0.3283820000000001
            }
        },
        {
            "phase": "LateEntityUpdate/TailAndQueuedFlush",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.9656333333333336,
                "maximum": 1.7017,
                "p95": 1.40293,
                "p99": 1.5655550000000003
            }
        },
        {
            "phase": "LateEntityUpdate/PrevFrameMirror",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.05339104477611943,
                "maximum": 0.1903,
                "p95": 0.08219000000000001,
                "p99": 0.09497000000000003
            }
        },
        {
            "phase": "LateEntityUpdate/FinalPendingFlush",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.06615746268656714,
                "maximum": 0.21350000000000003,
                "p95": 0.10179,
                "p99": 0.13717200000000005
            }
        },
        {
            "phase": "Render/PresentationOrder",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.22611542288557216,
                "maximum": 1.703,
                "p95": 1.08316,
                "p99": 1.211934
            }
        },
        {
            "phase": "Render/BeginFrame",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 2.5437420398009955,
                "maximum": 31.6381,
                "p95": 12.341804999999999,
                "p99": 15.903284000000003
            }
        },
        {
            "phase": "Render/PrepareFrame/LegacyCapacityGuard",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 3.172381343283582,
                "maximum": 23.111800000000004,
                "p95": 14.514969999999999,
                "p99": 18.828435000000007
            }
        },
        {
            "phase": "Render/LateRendererUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.477588308457712,
                "maximum": 4.6428,
                "p95": 2.404685,
                "p99": 3.112144000000001
            }
        },
        {
            "phase": "Render/BeginFrame/SortEntities",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.21224900497512429,
                "maximum": 1.5318,
                "p95": 0.9755499999999998,
                "p99": 1.222687
            }
        },
        {
            "phase": "Render/BeginFrame/CaptureHitRecords",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.03461442786069653,
                "maximum": 0.46840000000000006,
                "p95": 0.160455,
                "p99": 0.35528100000000026
            }
        },
        {
            "phase": "Render/BeginFrame/CaptureEntities",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.6590686567164178,
                "maximum": 21.4276,
                "p95": 3.0723900000000006,
                "p99": 5.3170490000000039
            }
        },
        {
            "phase": "Render/BeginFrame/BuildCommands",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.634329850746269,
                "maximum": 11.7775,
                "p95": 7.9430549999999979,
                "p99": 9.962865
            }
        },
        {
            "phase": "Render/PrepareFrame/FrozenFrameCopy",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0388310945273632,
                "maximum": 0.3806,
                "p95": 0.19481999999999997,
                "p99": 0.2733950000000001
            }
        },
        {
            "phase": "Render/PrepareFrame/ResolveCommands",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.66565696517413,
                "maximum": 11.7756,
                "p95": 7.434995,
                "p99": 9.571328
            }
        },
        {
            "phase": "Render/PrepareFrame/WriteQuads",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.827857462686567,
                "maximum": 6.6031,
                "p95": 3.8444949999999999,
                "p99": 5.109664000000001
            }
        },
        {
            "phase": "Render/PrepareFrame/SetVertexBufferData",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.016322885572139308,
                "maximum": 0.19790000000000003,
                "p95": 0.083575,
                "p99": 0.13766000000000015
            }
        },
        {
            "phase": "Render/PrepareFrame/SetSubMeshes",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.002897014925373134,
                "maximum": 0.0256,
                "p95": 0.014089999999999999,
                "p99": 0.019296000000000005
            }
        },
        {
            "phase": "Render/ExecuteCommandBuffer",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.000679850746268657,
                "maximum": 0.0088,
                "p95": 0.0032,
                "p99": 0.003999000000000001
            }
        }
    ],
    "aiInputDetailTimingSource": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
    "aiInputDetailTimingUnavailableReason": "",
    "aiInputDetailTimings": [
        {
            "phase": "CharacterInput/AI/SnapshotSlotSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.40308084577114436,
                "maximum": 0.8109000000000001,
                "p95": 0.6285299999999999,
                "p99": 0.7143440000000001
            }
        },
        {
            "phase": "CharacterInput/AI/SnapshotIndexBuild",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.44679850746268637,
                "maximum": 1.338,
                "p95": 0.7299450000000001,
                "p99": 0.8522760000000003
            }
        },
        {
            "phase": "CharacterInput/AI/SnapshotQuadtreeSync",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 2.2172589552238817,
                "maximum": 10.4038,
                "p95": 3.15375,
                "p99": 3.4623740000000007
            }
        },
        {
            "phase": "CharacterInput/AI/FindNearestGround",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 5.711885820895518,
                "maximum": 26.7939,
                "p95": 15.02012,
                "p99": 17.435613000000008
            }
        },
        {
            "phase": "CharacterInput/AI/FindNearestAir",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 2.036353482587065,
                "maximum": 6.061100000000001,
                "p95": 4.886995,
                "p99": 5.5354600000000009
            }
        },
        {
            "phase": "CharacterInput/AI/RemainingAiDecision",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 10.086407213930345,
                "maximum": 36.3358,
                "p95": 23.512529999999999,
                "p99": 26.733925000000004
            }
        },
        {
            "phase": "CharacterInput/AI/InputStateSyncFromRuntime",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.10712810945273627,
                "maximum": 0.7342000000000001,
                "p95": 0.1956,
                "p99": 0.24944200000000006
            }
        },
        {
            "phase": "CharacterInput/AI/ComboUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 1.1743691542288562,
                "maximum": 3.0817,
                "p95": 2.21067,
                "p99": 2.545287
            }
        },
        {
            "phase": "CharacterInput/AI/RefreshRuntimeSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.9236385572139301,
                "maximum": 2.3514,
                "p95": 1.8053049999999996,
                "p99": 2.234881
            }
        }
    ],
    "lateRuntimeSnapshotTimingSource": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
    "lateRuntimeSnapshotTimingUnavailableReason": "",
    "lateRuntimeSnapshotTimings": [
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/StateSpecial",
            "callCount": 0,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/Recovery",
            "callCount": 0,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/FrameTickSuppressed",
            "callCount": 0,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/FrameTick",
            "callCount": 402000,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.6175196517412935,
                "maximum": 1.1275,
                "p95": 0.93524,
                "p99": 1.078573
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/FrameExit",
            "callCount": 0,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/DeathOpoint",
            "callCount": 402000,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.5814385572139306,
                "maximum": 11.5607,
                "p95": 0.6878650000000001,
                "p99": 0.7696580000000003
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/CleanupCompleted",
            "callCount": 0,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/TailAndQueuedFlush",
            "callCount": 402000,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.5712825870646769,
                "maximum": 0.915,
                "p95": 0.766665,
                "p99": 0.834987
            }
        },
        {
            "stage": "LateEntityUpdate/RefreshRuntimeSnapshot/PrevFrameMirror",
            "callCount": 0,
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps around individual LateEntityUpdate RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, not source-code line numbers; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 402,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        }
    ],
    "aiInputDetailCounters": {
        "available": true,
        "unavailableReason": "",
        "aiCount": 402000,
        "spatialQueryCount": 401416,
        "queriedHandleVisits": 10113022,
        "candidateVisits": 10113022,
        "radiusExpansions": 0,
        "bruteFallbackCount": 0,
        "bruteSlotVisits": 0,
        "phase1ListVisits": 0,
        "refreshCount": 402000,
        "radiusHistogram": [
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        ]
    },
    "loggingPolicy": {
        "originalFilterLogType": "Log",
        "runningFilterLogType": "Error",
        "policy": "Suppress Log and Warning during the stress run while retaining Error.",
        "applied": false,
        "restored": true
    },
    "teardown": {
        "attempted": true,
        "restored": true,
        "activeStateRestored": true,
        "driverStateRestored": true,
        "loggingStateRestored": true,
        "activeGameObjectsBefore": 1000,
        "activeGameObjectsAfter": 0,
        "worldObjectsBefore": 2000,
        "worldObjectsAfter": 0,
        "worldEntitiesBefore": 1000,
        "worldEntitiesAfter": 0,
        "claimedSlotsBefore": 1000,
        "claimedSlotsAfter": 0,
        "objectPoolActiveBeforeRun": 0,
        "objectPoolActiveAfter": 0,
        "objectPoolAvailableBeforeRun": 10,
        "objectPoolAvailableAfter": 1000,
        "retainedInactiveObjectPoolCapacityBeforeRun": 10,
        "retainedInactiveObjectPoolCapacityAfter": 1000,
        "retainedInactiveObjectPoolCapacityDelta": 990,
        "retainedInactiveObjectPoolCapacityPolicy": "Informational inactive cache capacity only; it is not active cleanup residue and the stress harness does not trim it.",
        "referencePoolActiveBeforeRun": 0,
        "referencePoolActiveAfter": 0,
        "cleanupExceptionCount": 0,
        "cleanupExceptions": "",
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1000 (delta=990; doesNotAffectRestored=True)"
    }
}

[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# 任务：选择下一批等价性能优化

项目：Unity 2022.3 NTSD 战斗 runtime。

唯一战斗逻辑权威：
`J:\QQFile\NTSD2.4\ntsd_release_C#`

当前 Unity 1000 全 AI 最新详细报告：
`Temp/NTSD_ProductionEntityStress.dispersed-full-ai-occupancy-epoch-detail-20260726.json`

已知热点（详细诊断开启）：

- `CharacterInput = 17.919 ms/tick`
- `RemainingAiDecision = 10.086 ms/tick`
- `LateEntityUpdate = 10.130 ms/tick`
- Late `FrameTick = 4.126 ms/tick`
- Late `OpointProcess = 2.655 ms/tick`
- `CandidateCollect = 4.680 ms/tick`
- `RenderDispatch = 4.251 ms/tick`

现在另有一次 `enableDetailPhaseTiming=false` 的生产基线正在运行。

请只读分析，不要修改任何文件。重点回答：

1. 在 `SimulationWorld.AiInput.partial.cs::PrepareAiInputBasic` 中，将
   `RemainingAiDecision` 继续拆成哪些粗粒度阶段，才能低扰动定位热点？
2. 找出 1～3 个可证明等价、预计收益最大的第一批实现候选。必须保持：
   - 权威 C# 的 runtime slot 升序；
   - RNG 调用次数与调用顺序；
   - 同 tick 可观察 mutation；
   - 早退顺序；
   - 输入边沿与 `ApplyInputEdges` 时机。
3. 评估按 OID 分发角色专属决策、缓存重复 runtime 字段、维护紧凑活动 slot 表的语义风险。
4. 分析 Late `FrameTick` 和 `OpointProcess`。本次报告 `observedOpointCreates=0`，
   但 `OpointProcess` 仍为 2.655 ms。判断这是诊断成本、无效检查还是正式逻辑成本，
   并给出安全优化边界。
5. 给出聚焦测试矩阵和 old/new A/B oracle。不要建议跳 tick、降低 AI 数量或改变玩法。

输出必须给出精确文件、方法和建议实现顺序，并明确哪些方案暂时不要做。
