using System;
using System.Collections.Generic;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Spatial;

using AiSoASensingRows =
    NTSD.Simulation.SimulationAiSensingModule.AiSoASensingRows;

namespace NTSD.Simulation
{
    internal sealed class SimulationAiInputModule
    {
        private readonly SimulationWorld world;
        internal struct AiInputContext
        {
            internal int Difficulty;
            internal int Rand3;
            internal int Rand5;
            internal int Rand15;
            internal int Rand20;
            internal int MoveMode;
            internal int StageTargetX;
            internal int InputPhase;
        }

        internal struct AiTeamHpSummary
        {
            internal int Count;
            internal int MinHp;
            internal int MinCount;
            internal int SecondMinHp;

            internal void Add(int hp)
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

        internal sealed class AiGroundTeamPartition
        {
            internal int Team { get; private set; }
            internal LooseQuadtreeBroadphase Broadphase { get; } =
                new LooseQuadtreeBroadphase();
            internal List<IncrementalSpatialEntry> Entries { get; } =
                new List<IncrementalSpatialEntry>(32);

            internal void ResetForTeam(int team)
            {
                Team = team;
                Entries.Clear();
            }
        }

        internal struct AiNearestSlotFacts
        {
            internal LF2Entity Entity;
            internal uint HandleGeneration;
            internal uint SnapshotVersion;
            internal ulong OccupancyEpoch;
            internal int Slot;
            internal int X;
            internal int Y;
            internal int Z;
            internal int Hp;
            internal int Team;
            internal int State;
            internal int DataObjectType;
            internal double Vx;
            internal bool Active;
            internal bool Included;
            internal bool GroundRole;
            internal bool AirRole;
        }

        internal struct AiNearestSnapshotStamp
        {
            internal LF2Entity[] Slots;
            internal uint[] GenerationBySlot;
            internal AiNearestSlotFacts[] FactsBySlot;
            internal ulong OccupancyEpoch;
            internal uint FactsVersion;
            internal int SlotCount;
        }

        internal struct AiNearestPointFilter : IIncrementalPointNearestFilter
        {
            internal SimulationWorld World;
            internal SimulationAiInputModule Module;
            internal AiNearestSnapshotStamp Stamp;
            internal LF2Entity SelfEntity;
            internal int SelfSlot;
            internal int SelfX;
            internal int SelfTeam;
            internal int InputPhase;
            internal bool Air;
            internal bool UseSnapshotFacts;

            public IncrementalPointFilterDecision Evaluate(RuntimeEntityHandle handle)
            {
                int slot = handle.Slot;
                if (World == null || Module == null || !handle.IsValid)
                    return IncrementalPointFilterDecision.Abort;

                if (UseSnapshotFacts)
                {
                    if (!Module.IsNearestSnapshotStampCurrent(
                            Stamp,
                            World.RuntimeSlotOccupancyEpochForServices) ||
                        slot < 0 || slot >= Stamp.SlotCount ||
                        handle.Generation != Stamp.GenerationBySlot[slot])
                    {
                        return IncrementalPointFilterDecision.Abort;
                    }

                    LF2Entity candidate = Stamp.Slots[slot];
                    ref readonly AiNearestSlotFacts facts =
                        ref Stamp.FactsBySlot[slot];
                    if (facts.SnapshotVersion != Stamp.FactsVersion ||
                        facts.OccupancyEpoch != Stamp.OccupancyEpoch ||
                        facts.Slot != slot ||
                        facts.HandleGeneration != handle.Generation ||
                        !facts.Included || !facts.Active ||
                        !ReferenceEquals(facts.Entity, candidate))
                    {
                        return IncrementalPointFilterDecision.Abort;
                    }

                    bool accepted = Air
                        ? World.IsAirAiTargetCandidateForInputModule(
                            SelfEntity,
                            SelfSlot,
                            SelfTeam,
                            facts,
                            InputPhase)
                        : World.IsGroundAiTargetCandidateForInputModule(
                            SelfEntity,
                            SelfSlot,
                            SelfX,
                            SelfTeam,
                            facts,
                            InputPhase);
                    return accepted
                        ? IncrementalPointFilterDecision.Accept
                        : IncrementalPointFilterDecision.Reject;
                }

                if (Module.SlotSnapshotOccupancyEpoch == 0 ||
                    World.RuntimeSlotOccupancyEpochForServices !=
                        Module.SlotSnapshotOccupancyEpoch ||
                    slot < 0 || slot >= Module.Slots.Length ||
                    Module.GroundGenerationBySlot == null ||
                    slot >= Module.GroundGenerationBySlot.Length ||
                    handle.Generation != Module.GroundGenerationBySlot[slot])
                {
                    return IncrementalPointFilterDecision.Abort;
                }

                LF2Entity live = Module.Slots[slot];
                if (live?.Runtime == null || live.Runtime.SlotIndex != slot)
                    return IncrementalPointFilterDecision.Abort;

                bool liveAccepted = Air
                    ? World.IsAirAiTargetCandidateForInputModule(
                        SelfEntity,
                        live,
                        InputPhase)
                    : World.IsGroundAiTargetCandidateForInputModule(
                        SelfEntity,
                        live,
                        InputPhase);
                return liveAccepted
                    ? IncrementalPointFilterDecision.Accept
                    : IncrementalPointFilterDecision.Reject;
            }
        }

        internal SimulationAiInputModule(
            SimulationWorld world,
            int runtimeSlotCapacity)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            Slots = new LF2Entity[runtimeSlotCapacity];
        }

        internal LF2Entity[] Slots { get; set; }
        internal LooseQuadtreeBroadphase SpatialBroadphase { get; } =
            new LooseQuadtreeBroadphase();
        internal List<IncrementalSpatialEntry> SpatialEntries { get; } =
            new List<IncrementalSpatialEntry>(128);
        internal List<RuntimeEntityHandle> SpatialHandles { get; } =
            new List<RuntimeEntityHandle>(128);
        internal List<int> SpatialSlots { get; } = new List<int>(128);
        internal List<LF2Entity> EntityScratch { get; } = new List<LF2Entity>(128);
        internal LooseQuadtreeBroadphase GroundSpatialBroadphase { get; } =
            new LooseQuadtreeBroadphase();
        internal List<IncrementalSpatialEntry> GroundSpatialEntries { get; } =
            new List<IncrementalSpatialEntry>(128);
        internal LooseQuadtreeBroadphase AirSpatialBroadphase { get; } =
            new LooseQuadtreeBroadphase();
        internal List<IncrementalSpatialEntry> AirSpatialEntries { get; } =
            new List<IncrementalSpatialEntry>(32);
        internal List<int> SpecialScanSlots { get; } = new List<int>(32);
        internal List<int> Phase1TargetSlots { get; } = new List<int>(32);
        internal int[] Phase1TeamBySlot { get; set; }
        internal uint[] Phase1GenerationBySlot { get; set; }
        internal bool Phase1TargetSlotsValid { get; set; }
        internal bool[] MoveModeFirst10Present { get; set; } = new bool[10];
        internal bool[] MoveModeFirst10Eligible { get; set; } = new bool[10];
        internal uint[] MoveModeFirst10Generation { get; set; } = new uint[10];
        internal int[] MoveModeFirst10Hp { get; set; } = new int[10];
        internal int[] MoveModeFirst10X { get; set; } = new int[10];
        internal int[] MoveModeFirst10Z { get; set; } = new int[10];
        internal int MoveModeTopSlot { get; set; } = -1;
        internal int MoveModeTopX { get; set; } = -1;
        internal int MoveModeTopZ { get; set; }
        internal int MoveModeSecondSlot { get; set; } = -1;
        internal int MoveModeSecondX { get; set; } = -1;
        internal int MoveModeSecondZ { get; set; }
        internal bool MoveModeFirst10Valid { get; set; }
        internal bool[] TeamHpSnapshotEligible { get; set; }
        internal int[] TeamHpSnapshotTeams { get; set; }
        internal int[] TeamHpSnapshotValues { get; set; }
        internal bool[] GroundRoleBySlot { get; set; }
        internal int[] GroundXBySlot { get; set; }
        internal int[] GroundZBySlot { get; set; }
        internal int[] GroundTeamBySlot { get; set; }
        internal uint[] GroundGenerationBySlot { get; set; }
        internal bool[] AirRoleBySlot { get; set; }
        internal Dictionary<int, AiGroundTeamPartition> GroundTeamPartitions
        {
            get;
        } = new Dictionary<int, AiGroundTeamPartition>(2);
        internal List<AiGroundTeamPartition> ActiveGroundTeamPartitions { get; } =
            new List<AiGroundTeamPartition>(2);
        internal AiGroundTeamPartition[] GroundTeamPartitionPool { get; } =
            new[]
        {
            new AiGroundTeamPartition(),
            new AiGroundTeamPartition(),
        };
        internal bool GroundTeamPartitionOverflow { get; set; }
        internal Dictionary<int, AiTeamHpSummary> TeamHpSummaries { get; } =
            new Dictionary<int, AiTeamHpSummary>(8);
        internal AiNearestSlotFacts[] NearestFactsBySlot { get; set; }
        internal uint NearestFactsVersionCounter { get; set; }
        internal uint NearestFactsActiveVersion { get; set; }
        internal ulong SlotSnapshotOccupancyEpoch { get; set; }
        internal bool SpatialReady { get; set; }
        internal bool GroundSpatialReady { get; set; }
        internal bool GroundTeamPartitionsValid { get; set; }
        internal bool AirSpatialReady { get; set; }
        internal int AirRoleCount { get; set; }
        internal bool AirRoleCountValid { get; set; }
        internal bool TeamHpSummaryValid { get; set; }
        internal bool ForceFullSpecialScan { get; set; }
        internal bool ForceFullPhase1TargetScan { get; set; }
        internal bool ForceFullSameTeamScan { get; set; }
        internal bool ForceFullMoveModeScan { get; set; }
        internal bool ForceFullNearestScan { get; set; }
        internal bool ForceLegacyNearestQuery { get; set; }
        internal bool ForceLegacyNearestFilter { get; set; }
        internal bool EnableNearestBestFirstShadow { get; set; }
        internal int SameTeamSummaryFallbackCount { get; set; }
        internal int NearestBestFirstShadowMismatchCount { get; set; }
        internal string NearestBestFirstFirstShadowMismatch { get; set; }
        internal int NearestAirPassCount { get; set; }
        internal int LegacyNearestFactsBuildCount { get; set; }
        internal int LegacySnapshotIndexBuildCount { get; set; }
        internal int LegacyQuadtreeSyncCount { get; set; }
        internal int LegacySnapshotMutationCount { get; set; }

        internal void ResetAirSpatialIndex()
        {
            AirSpatialBroadphase.ResetIncremental();
            AirSpatialReady = false;
        }

        internal void InvalidateAirRoleSnapshot()
        {
            ResetAirSpatialIndex();
            AirRoleCount = 0;
            AirRoleCountValid = false;
        }

        internal void ResetNearestAirPassCount()
        {
            NearestAirPassCount = 0;
        }

        internal void RecordNearestAirPass()
        {
            NearestAirPassCount++;
        }

        internal void PrepareGroundTeamPartitionsForSnapshot()
        {
            for (int index = 0; index < ActiveGroundTeamPartitions.Count; index++)
                ActiveGroundTeamPartitions[index].Entries.Clear();
            ActiveGroundTeamPartitions.Clear();
            GroundTeamPartitions.Clear();
            GroundTeamPartitionOverflow = false;
            GroundTeamPartitionsValid = false;
        }

        internal AiGroundTeamPartition GetGroundTeamPartition(int team)
        {
            if (!GroundTeamPartitions.TryGetValue(
                    team,
                    out AiGroundTeamPartition partition))
            {
                int partitionIndex = ActiveGroundTeamPartitions.Count;
                if (partitionIndex >= GroundTeamPartitionPool.Length)
                {
                    GroundTeamPartitionOverflow = true;
                    return null;
                }

                partition = GroundTeamPartitionPool[partitionIndex];
                partition.ResetForTeam(team);
                GroundTeamPartitions.Add(team, partition);
                ActiveGroundTeamPartitions.Add(partition);
            }
            return partition;
        }

        internal void InvalidateGroundTeamPartitions()
        {
            GroundTeamPartitionsValid = false;
            for (int index = 0; index < ActiveGroundTeamPartitions.Count; index++)
            {
                ActiveGroundTeamPartitions[index].Broadphase.ResetIncremental();
            }
        }

        internal void SynchronizeGroundTeamPartitions(in SpatialAabbXZ preferredRoot)
        {
            if (GroundTeamPartitionOverflow)
            {
                InvalidateGroundTeamPartitions();
                return;
            }

            for (int index = 0; index < ActiveGroundTeamPartitions.Count; index++)
            {
                AiGroundTeamPartition partition = ActiveGroundTeamPartitions[index];
                SpatialSynchronizeResult result =
                    partition.Broadphase.Synchronize(partition.Entries, preferredRoot);
                if (!result.Succeeded || result.IndexedCount != partition.Entries.Count)
                {
                    InvalidateGroundTeamPartitions();
                    return;
                }
            }

            GroundTeamPartitionsValid = true;
        }

        internal void ResetMoveModeFirst10Snapshot()
        {
            System.Array.Clear(MoveModeFirst10Present, 0, MoveModeFirst10Present.Length);
            System.Array.Clear(MoveModeFirst10Eligible, 0, MoveModeFirst10Eligible.Length);
            System.Array.Clear(MoveModeFirst10Generation, 0, MoveModeFirst10Generation.Length);
            System.Array.Clear(MoveModeFirst10Hp, 0, MoveModeFirst10Hp.Length);
            System.Array.Clear(MoveModeFirst10X, 0, MoveModeFirst10X.Length);
            System.Array.Clear(MoveModeFirst10Z, 0, MoveModeFirst10Z.Length);
            MoveModeTopSlot = -1;
            MoveModeTopX = -1;
            MoveModeTopZ = 0;
            MoveModeSecondSlot = -1;
            MoveModeSecondX = -1;
            MoveModeSecondZ = 0;
            MoveModeFirst10Valid = false;
        }

        internal void EnsureSnapshotCapacity()
        {
            if (TeamHpSnapshotEligible?.Length == Slots.Length &&
                GroundRoleBySlot?.Length == Slots.Length &&
                GroundTeamBySlot?.Length == Slots.Length &&
                GroundGenerationBySlot?.Length == Slots.Length &&
                AirRoleBySlot?.Length == Slots.Length &&
                NearestFactsBySlot?.Length == Slots.Length &&
                Phase1TeamBySlot?.Length == Slots.Length &&
                Phase1GenerationBySlot?.Length == Slots.Length)
            {
                return;
            }

            TeamHpSnapshotEligible = new bool[Slots.Length];
            TeamHpSnapshotTeams = new int[Slots.Length];
            TeamHpSnapshotValues = new int[Slots.Length];
            GroundRoleBySlot = new bool[Slots.Length];
            GroundXBySlot = new int[Slots.Length];
            GroundZBySlot = new int[Slots.Length];
            GroundTeamBySlot = new int[Slots.Length];
            GroundGenerationBySlot = new uint[Slots.Length];
            AirRoleBySlot = new bool[Slots.Length];
            NearestFactsBySlot = new AiNearestSlotFacts[Slots.Length];
            Phase1TeamBySlot = new int[Slots.Length];
            Phase1GenerationBySlot = new uint[Slots.Length];
        }

        internal uint AdvanceNearestFactsVersion()
        {
            unchecked
            {
                NearestFactsVersionCounter++;
                if (NearestFactsVersionCounter == 0)
                    NearestFactsVersionCounter = 1;
            }
            return NearestFactsVersionCounter;
        }

        internal bool TryGetSameTeamSummaryExcludingSelf(
            LF2Entity self,
            int slot,
            int selfTeam,
            int selfHp,
            out int otherCount,
            out int otherMinHp)
        {
            otherCount = 0;
            otherMinHp = int.MaxValue;
            if (!TeamHpSummaryValid || self?.Runtime == null)
                return false;

            if (slot < 0 || slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], self) ||
                !TeamHpSnapshotEligible[slot] ||
                TeamHpSnapshotTeams[slot] != selfTeam ||
                TeamHpSnapshotValues[slot] != selfHp ||
                !TeamHpSummaries.TryGetValue(
                    selfTeam,
                    out AiTeamHpSummary summary))
            {
                TeamHpSummaryValid = false;
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

        internal static bool IsSpecialScanObjectId(int objectId)
        {
            return objectId / 100 == 1 ||
                   objectId == 0xC8 ||
                   objectId == 0xD3 ||
                   objectId == 0xD4 ||
                   objectId == 0xD5;
        }

        internal void CaptureMoveModeFirst10Candidate(
            int slot,
            int hp,
            bool handleProven,
            RuntimeEntityHandle handle,
            bool eligible,
            int x,
            int z)
        {
            MoveModeFirst10Present[slot] = true;
            MoveModeFirst10Hp[slot] = hp;
            if (handleProven)
                MoveModeFirst10Generation[slot] = handle.Generation;

            MoveModeFirst10Eligible[slot] = eligible;
            if (!eligible)
                return;

            MoveModeFirst10X[slot] = x;
            MoveModeFirst10Z[slot] = z;
            if (x <= -1)
                return;

            if (MoveModeTopSlot < 0 || x > MoveModeTopX)
            {
                MoveModeSecondSlot = MoveModeTopSlot;
                MoveModeSecondX = MoveModeTopX;
                MoveModeSecondZ = MoveModeTopZ;
                MoveModeTopSlot = slot;
                MoveModeTopX = x;
                MoveModeTopZ = z;
                return;
            }

            if (MoveModeSecondSlot < 0 || x > MoveModeSecondX)
            {
                MoveModeSecondSlot = slot;
                MoveModeSecondX = x;
                MoveModeSecondZ = z;
            }
        }

        internal void ClearSlotSnapshot(bool useSoACandidate)
        {
            SlotSnapshotOccupancyEpoch = 0;
            NearestFactsActiveVersion = 0;
            if (useSoACandidate)
            {
                System.Array.Clear(Slots, 0, Slots.Length);
                ResetMoveModeFirst10Snapshot();
                TeamHpSummaryValid = false;
                Phase1TargetSlotsValid = false;
                SpatialReady = false;
                GroundSpatialReady = false;
                GroundTeamPartitionsValid = false;
                AirSpatialReady = false;
                AirRoleCount = 0;
                AirRoleCountValid = false;
                return;
            }

            if (NearestFactsBySlot != null)
            {
                System.Array.Clear(
                    NearestFactsBySlot,
                    0,
                    NearestFactsBySlot.Length);
            }
            System.Array.Clear(Slots, 0, Slots.Length);
            SpecialScanSlots.Clear();
            Phase1TargetSlots.Clear();
            if (Phase1TeamBySlot != null)
                System.Array.Clear(Phase1TeamBySlot, 0, Phase1TeamBySlot.Length);
            if (Phase1GenerationBySlot != null)
            {
                System.Array.Clear(
                    Phase1GenerationBySlot,
                    0,
                    Phase1GenerationBySlot.Length);
            }
            Phase1TargetSlotsValid = false;
            TeamHpSummaries.Clear();
            if (TeamHpSnapshotEligible != null)
            {
                System.Array.Clear(
                    TeamHpSnapshotEligible,
                    0,
                    TeamHpSnapshotEligible.Length);
            }
            if (TeamHpSnapshotTeams != null)
                System.Array.Clear(TeamHpSnapshotTeams, 0, TeamHpSnapshotTeams.Length);
            if (TeamHpSnapshotValues != null)
                System.Array.Clear(TeamHpSnapshotValues, 0, TeamHpSnapshotValues.Length);
            if (GroundRoleBySlot != null)
                System.Array.Clear(GroundRoleBySlot, 0, GroundRoleBySlot.Length);
            if (GroundTeamBySlot != null)
                System.Array.Clear(GroundTeamBySlot, 0, GroundTeamBySlot.Length);
            if (GroundGenerationBySlot != null)
            {
                System.Array.Clear(
                    GroundGenerationBySlot,
                    0,
                    GroundGenerationBySlot.Length);
            }
            if (AirRoleBySlot != null)
                System.Array.Clear(AirRoleBySlot, 0, AirRoleBySlot.Length);
            SpatialReady = false;
            GroundSpatialReady = false;
            GroundTeamPartitionsValid = false;
            AirSpatialReady = false;
            AirRoleCount = 0;
            AirRoleCountValid = false;
            TeamHpSummaryValid = false;
            MoveModeFirst10Valid = false;
        }

        internal void BuildSnapshotIndices(SimulationWorld world)
        {
            LegacySnapshotIndexBuildCount++;
            EnsureSnapshotCapacity();
            System.Array.Clear(TeamHpSnapshotEligible, 0, TeamHpSnapshotEligible.Length);
            System.Array.Clear(TeamHpSnapshotTeams, 0, TeamHpSnapshotTeams.Length);
            System.Array.Clear(TeamHpSnapshotValues, 0, TeamHpSnapshotValues.Length);
            TeamHpSummaries.Clear();
            SpecialScanSlots.Clear();
            Phase1TargetSlots.Clear();
            System.Array.Clear(Phase1TeamBySlot, 0, Phase1TeamBySlot.Length);
            System.Array.Clear(Phase1GenerationBySlot, 0, Phase1GenerationBySlot.Length);
            Phase1TargetSlotsValid = false;
            ResetMoveModeFirst10Snapshot();
            bool phase1TargetSlotsProven = true;
            bool moveModeFirst10Proven = true;

            for (int slot = 0; slot < Slots.Length; slot++)
            {
                LF2Entity entity = Slots[slot];
                if (world.IsLivingCharacterDatForAiInputModule(entity))
                {
                    int summaryTeam = world.GetAiTeamForInputModule(entity);
                    int hp = world.GetAiHpForInputModule(entity);
                    TeamHpSnapshotEligible[slot] = true;
                    TeamHpSnapshotTeams[slot] = summaryTeam;
                    TeamHpSnapshotValues[slot] = hp;

                    TeamHpSummaries.TryGetValue(
                        summaryTeam,
                        out AiTeamHpSummary summary);
                    summary.Add(hp);
                    TeamHpSummaries[summaryTeam] = summary;
                }

                if (slot >= 20 && entity != null &&
                    IsSpecialScanObjectId(entity.ObjectId))
                {
                    SpecialScanSlots.Add(slot);
                }

                if (entity == null)
                    continue;

                int team = world.GetAiTeamForInputModule(entity);
                bool handleProven = world.TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle);
                if (handleProven)
                {
                    Phase1TeamBySlot[slot] = team;
                    Phase1GenerationBySlot[slot] = handle.Generation;
                    if (team == 5)
                        Phase1TargetSlots.Add(slot);
                }
                else
                {
                    phase1TargetSlotsProven = false;
                }

                if (slot < MoveModeFirst10Present.Length)
                {
                    bool eligible = world.IsLivingCharacterDatForAiInputModule(entity);
                    CaptureMoveModeFirst10Candidate(
                        slot,
                        world.GetAiHpForInputModule(entity),
                        handleProven,
                        handle,
                        eligible,
                        world.GetAiXForInputModule(entity),
                        world.GetAiZForInputModule(entity));
                    if (!handleProven)
                        moveModeFirst10Proven = false;
                }
            }

            TeamHpSummaryValid = true;
            Phase1TargetSlotsValid = phase1TargetSlotsProven;
            MoveModeFirst10Valid = moveModeFirst10Proven;
        }

        internal void BuildCandidateSnapshotProducts()
        {
            TeamHpSummaryValid = false;
            Phase1TargetSlotsValid = false;
            SpatialReady = false;
            GroundSpatialReady = false;
            GroundTeamPartitionsValid = false;
            AirSpatialReady = false;
            AirRoleCount = 0;
            AirRoleCountValid = false;
        }

        internal bool CaptureNearestFactsSnapshot(
            SimulationWorld world,
            uint snapshotVersion,
            ulong occupancyEpoch)
        {
            LegacyNearestFactsBuildCount++;
            System.Array.Clear(NearestFactsBySlot, 0, NearestFactsBySlot.Length);
            bool proven = true;
            for (int slot = 0; slot < Slots.Length; slot++)
            {
                LF2Entity entity = Slots[slot];
                if (entity == null)
                    continue;

                if (!world.TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                    !TryCaptureNearestSlotFacts(
                        world,
                        entity,
                        slot,
                        handle.Generation,
                        snapshotVersion,
                        occupancyEpoch,
                        out AiNearestSlotFacts facts))
                {
                    proven = false;
                    continue;
                }

                NearestFactsBySlot[slot] = facts;
            }
            return proven;
        }

        internal bool TryCaptureNearestSlotFacts(
            SimulationWorld world,
            LF2Entity entity,
            int slot,
            uint handleGeneration,
            uint snapshotVersion,
            ulong occupancyEpoch,
            out AiNearestSlotFacts facts)
        {
            facts = default;
            if (entity?.Runtime == null ||
                handleGeneration == 0 ||
                snapshotVersion == 0 ||
                occupancyEpoch == 0 ||
                slot < 0 ||
                slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], entity) ||
                entity.Runtime.SlotIndex != slot ||
                !world.IsActiveForCurrentPassInternal(entity))
            {
                return false;
            }

            NTSDEntityRuntime runtime = entity.Runtime;
            int state = world.GetAiStateForInputModule(entity);
            int dataObjectType = entity.GetCurrentDataObjectTypeForSimulation();
            int y = runtime.YInt;
            bool airRole = state == 14 || System.Math.Abs(y) > 2;
            facts = new AiNearestSlotFacts
            {
                Entity = entity,
                HandleGeneration = handleGeneration,
                SnapshotVersion = snapshotVersion,
                OccupancyEpoch = occupancyEpoch,
                Slot = slot,
                X = runtime.XInt,
                Y = y,
                Z = runtime.ZInt,
                Hp = runtime.HP,
                Team = runtime.RelationTeam,
                State = state,
                DataObjectType = dataObjectType,
                Vx = runtime.Vx,
                Active = true,
                Included = true,
                GroundRole = !airRole && (dataObjectType == 0 || state == 3000),
                AirRole = airRole,
            };
            return true;
        }

        internal void BuildInputSlotSnapshot(SimulationWorld world)
        {
            SlotSnapshotOccupancyEpoch = 0;
            NearestFactsActiveVersion = 0;
            bool useSoACandidate = world.AiInputUsesSoACandidateForModule;
            uint factsVersion = useSoACandidate ? 0 : AdvanceNearestFactsVersion();
            int runtimeCapacityBefore = world.RuntimeSlotCapacity;
            ulong occupancyEpochBefore = world.RuntimeSlotOccupancyEpochForServices;
            BattleAiInputDetailDiagnostics diagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            SameTeamSummaryFallbackCount = 0;
            ResetNearestAirPassCount();
            if (world.TryPrepareUnifiedAiInputSnapshotForModule(diagnostics))
                return;
            if (!useSoACandidate)
                EnsureSnapshotCapacity();
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);
            if (useSoACandidate)
            {
                world.CaptureAiCandidateFusedSnapshotForInputModule(
                    runtimeCapacityBefore,
                    occupancyEpochBefore);
            }
            else
            {
                System.Array.Clear(Slots, 0, Slots.Length);
                world.GetAllEntitiesForAiInputModule(EntityScratch);
                for (int index = 0; index < EntityScratch.Count; index++)
                {
                    LF2Entity entity = EntityScratch[index];
                    int slot = entity?.Runtime?.SlotIndex ?? -1;
                    if (slot >= 0 && slot < Slots.Length &&
                        world.IsActiveForCurrentPassInternal(entity))
                    {
                        Slots[slot] = entity;
                    }
                }
                EntityScratch.Clear();
                world.CaptureAiSensingShadowSnapshotForInputModule(
                    occupancyEpochBefore);
            }
            bool factsProven = !useSoACandidate &&
                               CaptureNearestFactsSnapshot(
                                   world,
                                   factsVersion,
                                   occupancyEpochBefore);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);
            if (useSoACandidate)
                BuildCandidateSnapshotProducts();
            else
                BuildSnapshotIndices(world);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);
            if (!useSoACandidate)
                world.SynchronizeAiInputSpatialSnapshotForModule();
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);
            ulong occupancyEpochAfter = world.RuntimeSlotOccupancyEpochForServices;
            world.ObserveAiSensingSnapshotBuildEpochForInputModule(
                occupancyEpochBefore,
                occupancyEpochAfter);
            if (occupancyEpochBefore == occupancyEpochAfter)
            {
                SlotSnapshotOccupancyEpoch = occupancyEpochAfter;
                if (factsProven)
                    NearestFactsActiveVersion = factsVersion;
            }
            world.PrepareAiUnifiedSnapshotShadowPassForInputModule(diagnostics);
        }

        internal void SynchronizeSpatialSnapshot(SimulationWorld world)
        {
            LegacyQuadtreeSyncCount++;
            SpatialEntries.Clear();
            GroundSpatialEntries.Clear();
            AirSpatialEntries.Clear();
            PrepareGroundTeamPartitionsForSnapshot();
            System.Array.Clear(GroundRoleBySlot, 0, GroundRoleBySlot.Length);
            System.Array.Clear(GroundTeamBySlot, 0, GroundTeamBySlot.Length);
            System.Array.Clear(GroundGenerationBySlot, 0, GroundGenerationBySlot.Length);
            System.Array.Clear(AirRoleBySlot, 0, AirRoleBySlot.Length);
            AirRoleCount = 0;
            AirRoleCountValid = false;
            bool hasBounds = false;
            bool spatialCoordinatesValid = true;
            int minX = 0;
            int minZ = 0;
            int maxX = 0;
            int maxZ = 0;
            for (int slot = 0; slot < Slots.Length; slot++)
            {
                LF2Entity entity = Slots[slot];
                if (entity == null)
                    continue;
                if (!world.TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    SpatialBroadphase.ResetIncremental();
                    GroundSpatialBroadphase.ResetIncremental();
                    InvalidateGroundTeamPartitions();
                    InvalidateAirRoleSnapshot();
                    SpatialReady = false;
                    GroundSpatialReady = false;
                    return;
                }

                bool airRole = world.IsAirAiSpatialRoleForInputModule(entity);
                AirRoleBySlot[slot] = airRole;
                if (airRole)
                    AirRoleCount++;

                int x = world.GetAiXForInputModule(entity);
                int z = world.GetAiZForInputModule(entity);
                int x2 = x == int.MaxValue ? int.MaxValue : x + 1;
                int z2 = z == int.MaxValue ? int.MaxValue : z + 1;
                if (x2 <= x || z2 <= z)
                {
                    spatialCoordinatesValid = false;
                    continue;
                }

                var bounds = new SpatialAabbXZ(x, z, x2, z2);
                var entry = new IncrementalSpatialEntry(handle, bounds);
                SpatialEntries.Add(entry);
                bool groundRole = world.IsGroundAiSpatialRoleForInputModule(entity);
                GroundRoleBySlot[slot] = groundRole;
                GroundXBySlot[slot] = x;
                GroundZBySlot[slot] = z;
                int team = world.GetAiTeamForInputModule(entity);
                GroundTeamBySlot[slot] = team;
                GroundGenerationBySlot[slot] = handle.Generation;
                if (groundRole)
                {
                    GroundSpatialEntries.Add(entry);
                    AiGroundTeamPartition partition = GetGroundTeamPartition(team);
                    partition?.Entries.Add(entry);
                }
                if (airRole)
                    AirSpatialEntries.Add(entry);
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
                    minX = System.Math.Min(minX, x);
                    minZ = System.Math.Min(minZ, z);
                    maxX = System.Math.Max(maxX, x2);
                    maxZ = System.Math.Max(maxZ, z2);
                }
            }

            AirRoleCountValid = AirRoleCount >= 0 && AirRoleCount <= Slots.Length;
            if (!spatialCoordinatesValid || !hasBounds)
            {
                SpatialBroadphase.ResetIncremental();
                GroundSpatialBroadphase.ResetIncremental();
                InvalidateGroundTeamPartitions();
                InvalidateAirRoleSnapshot();
                SpatialReady = false;
                GroundSpatialReady = false;
                return;
            }

            var preferredRoot = new SpatialAabbXZ(minX, minZ, maxX, maxZ);
            SynchronizeGroundTeamPartitions(preferredRoot);
            SpatialSynchronizeResult result =
                SpatialBroadphase.Synchronize(SpatialEntries, preferredRoot);
            SpatialReady = result.Succeeded && result.IndexedCount == SpatialEntries.Count;
            if (!SpatialReady)
            {
                SpatialBroadphase.ResetIncremental();
                InvalidateGroundTeamPartitions();
            }

            if (GroundSpatialEntries.Count == SpatialEntries.Count)
            {
                GroundSpatialBroadphase.ResetIncremental();
                GroundSpatialReady = false;
            }
            else
            {
                SpatialSynchronizeResult groundResult =
                    GroundSpatialBroadphase.Synchronize(
                        GroundSpatialEntries,
                        preferredRoot);
                GroundSpatialReady = groundResult.Succeeded &&
                                     groundResult.IndexedCount ==
                                     GroundSpatialEntries.Count;
                if (!GroundSpatialReady)
                    GroundSpatialBroadphase.ResetIncremental();
            }

            SpatialSynchronizeResult airResult =
                AirSpatialBroadphase.Synchronize(AirSpatialEntries, preferredRoot);
            AirSpatialReady = airResult.Succeeded &&
                              airResult.IndexedCount == AirSpatialEntries.Count;
            if (!AirSpatialReady)
                ResetAirSpatialIndex();
        }

        internal void ObserveNearestFactsMutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (NearestFactsActiveVersion == 0)
                return;
            if (world.RuntimeSlotOccupancyEpochForServices !=
                    SlotSnapshotOccupancyEpoch ||
                entity?.Runtime == null)
            {
                NearestFactsActiveVersion = 0;
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (slot < 0 || slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], entity) ||
                !world.TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle))
            {
                NearestFactsActiveVersion = 0;
                return;
            }

            AiNearestSlotFacts previous = NearestFactsBySlot[slot];
            if (previous.SnapshotVersion != NearestFactsActiveVersion ||
                previous.OccupancyEpoch != SlotSnapshotOccupancyEpoch ||
                previous.HandleGeneration != handle.Generation ||
                !previous.Included ||
                !previous.Active ||
                !ReferenceEquals(previous.Entity, entity) ||
                !TryCaptureNearestSlotFacts(
                    world,
                    entity,
                    slot,
                    handle.Generation,
                    NearestFactsActiveVersion,
                    SlotSnapshotOccupancyEpoch,
                    out AiNearestSlotFacts current))
            {
                NearestFactsActiveVersion = 0;
                return;
            }

            NearestFactsBySlot[slot] = current;
        }

        internal void ObserveTeamHpSummaryMutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            LegacySnapshotMutationCount++;
            ObserveNearestFactsMutation(world, entity);
            ObservePhase1TargetSlotsMutation(world, entity);
            ObserveMoveModeFirst10Mutation(world, entity);
            ObserveGroundSpatialRoleMutation(world, entity);
            ObserveAirSpatialRoleMutation(world, entity);
            if (!TeamHpSummaryValid || entity?.Runtime == null)
                return;

            int slot = entity.Runtime.SlotIndex;
            if (slot < 0 || slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], entity))
            {
                TeamHpSummaryValid = false;
                return;
            }

            bool currentEligible =
                world.IsActiveForCurrentPassInternal(entity) &&
                world.IsLivingCharacterDatForAiInputModule(entity);
            if (currentEligible != TeamHpSnapshotEligible[slot] ||
                (currentEligible &&
                 (world.GetAiTeamForInputModule(entity) != TeamHpSnapshotTeams[slot] ||
                  world.GetAiHpForInputModule(entity) != TeamHpSnapshotValues[slot])))
            {
                TeamHpSummaryValid = false;
            }
        }

        internal void ObserveCandidateCharacterInputMutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            ObserveMoveModeFirst10Mutation(world, entity);
        }

        internal void ObserveAirSpatialRoleMutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (!AirRoleCountValid)
                return;
            if (entity?.Runtime == null)
            {
                InvalidateAirRoleSnapshot();
                return;
            }

            int slot = world.GetAiSlotForInputModule(entity);
            if (slot < 0 || slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], entity) ||
                !world.TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle))
            {
                InvalidateAirRoleSnapshot();
                return;
            }

            bool airRole = world.IsAirAiSpatialRoleForInputModule(entity);
            if (AirRoleBySlot[slot] == airRole)
                return;

            bool updated;
            if (airRole)
            {
                int x = world.GetAiXForInputModule(entity);
                int z = world.GetAiZForInputModule(entity);
                int x2 = x == int.MaxValue ? int.MaxValue : x + 1;
                int z2 = z == int.MaxValue ? int.MaxValue : z + 1;
                updated = x2 > x && z2 > z &&
                          AirSpatialBroadphase.TryUpsertIncremental(
                              handle,
                              new SpatialAabbXZ(x, z, x2, z2));
            }
            else
            {
                updated = AirSpatialBroadphase.TryRemoveIncremental(handle);
            }

            if (!updated)
                ResetAirSpatialIndex();

            AirRoleBySlot[slot] = airRole;
            AirRoleCount += airRole ? 1 : -1;
            if (AirRoleCount < 0 || AirRoleCount > Slots.Length)
                InvalidateAirRoleSnapshot();
        }

        internal void ObserveGroundSpatialRoleMutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (entity?.Runtime == null)
            {
                InvalidateSpatialIndicesForCoordinateMutation();
                return;
            }

            int slot = world.GetAiSlotForInputModule(entity);
            if (slot < 0 || slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], entity) ||
                !world.TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != GroundGenerationBySlot[slot])
            {
                InvalidateSpatialIndicesForCoordinateMutation();
                return;
            }

            int x = world.GetAiXForInputModule(entity);
            int z = world.GetAiZForInputModule(entity);
            if (GroundXBySlot[slot] != x || GroundZBySlot[slot] != z)
            {
                InvalidateSpatialIndicesForCoordinateMutation();
                return;
            }

            ObserveGroundTeamPartitionMutation(world, entity);
            if (!GroundSpatialReady)
                return;

            bool groundRole = world.IsGroundAiSpatialRoleForInputModule(entity);
            bool previousGroundRole = GroundRoleBySlot[slot];
            if (previousGroundRole == groundRole)
                return;

            bool updated;
            if (groundRole)
            {
                int x2 = x == int.MaxValue ? int.MaxValue : x + 1;
                int z2 = z == int.MaxValue ? int.MaxValue : z + 1;
                updated = x2 > x && z2 > z &&
                          GroundSpatialBroadphase.TryUpsertIncremental(
                              handle,
                              new SpatialAabbXZ(x, z, x2, z2));
            }
            else
            {
                updated = GroundSpatialBroadphase.TryRemoveIncremental(handle);
            }

            if (!updated)
            {
                GroundSpatialBroadphase.ResetIncremental();
                GroundSpatialReady = false;
                return;
            }

            GroundRoleBySlot[slot] = groundRole;
            GroundXBySlot[slot] = x;
            GroundZBySlot[slot] = z;
        }

        internal void InvalidateSpatialIndicesForCoordinateMutation()
        {
            SpatialBroadphase.ResetIncremental();
            SpatialReady = false;
            GroundSpatialBroadphase.ResetIncremental();
            GroundSpatialReady = false;
            ResetAirSpatialIndex();
            InvalidateGroundTeamPartitions();
        }

        internal void ObserveGroundTeamPartitionMutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (!GroundTeamPartitionsValid)
                return;
            if (entity?.Runtime == null)
            {
                InvalidateGroundTeamPartitions();
                return;
            }

            int slot = world.GetAiSlotForInputModule(entity);
            if (slot < 0 || slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], entity) ||
                !world.TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != GroundGenerationBySlot[slot])
            {
                InvalidateGroundTeamPartitions();
                return;
            }

            bool groundRole = world.IsGroundAiSpatialRoleForInputModule(entity);
            if (groundRole != GroundRoleBySlot[slot] ||
                world.GetAiTeamForInputModule(entity) != GroundTeamBySlot[slot] ||
                world.GetAiXForInputModule(entity) != GroundXBySlot[slot] ||
                world.GetAiZForInputModule(entity) != GroundZBySlot[slot])
            {
                InvalidateGroundTeamPartitions();
            }
        }

        internal void ObserveMoveModeFirst10Mutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (!MoveModeFirst10Valid)
                return;
            if (entity?.Runtime == null)
            {
                MoveModeFirst10Valid = false;
                return;
            }

            int slot = world.GetAiSlotForInputModule(entity);
            if (slot < 0 || slot >= MoveModeFirst10Present.Length)
            {
                for (int index = 0; index < MoveModeFirst10Present.Length; index++)
                {
                    if (ReferenceEquals(Slots[index], entity))
                    {
                        MoveModeFirst10Valid = false;
                        break;
                    }
                }
                return;
            }

            if (!MoveModeFirst10Present[slot] ||
                !ReferenceEquals(Slots[slot], entity) ||
                !world.TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != MoveModeFirst10Generation[slot] ||
                world.GetAiHpForInputModule(entity) != MoveModeFirst10Hp[slot])
            {
                MoveModeFirst10Valid = false;
                return;
            }

            bool eligible = world.IsLivingCharacterDatForAiInputModule(entity);
            if (eligible != MoveModeFirst10Eligible[slot] ||
                (eligible &&
                 (world.GetAiXForInputModule(entity) != MoveModeFirst10X[slot] ||
                  world.GetAiZForInputModule(entity) != MoveModeFirst10Z[slot])))
            {
                MoveModeFirst10Valid = false;
            }
        }

        internal void ObservePhase1TargetSlotsMutation(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (!Phase1TargetSlotsValid)
                return;
            if (entity?.Runtime == null)
            {
                Phase1TargetSlotsValid = false;
                return;
            }

            int slot = world.GetAiSlotForInputModule(entity);
            if (slot < 0 || slot >= Slots.Length ||
                !ReferenceEquals(Slots[slot], entity) ||
                !world.TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != Phase1GenerationBySlot[slot] ||
                world.GetAiTeamForInputModule(entity) != Phase1TeamBySlot[slot])
            {
                Phase1TargetSlotsValid = false;
            }
        }

        internal bool TryQueryInputSlots(
            SimulationWorld world,
            in SpatialAabbXZ bounds,
            out List<int> slots)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.RecordSpatialQuery();
            slots = SpatialSlots;
            slots.Clear();
            if (!SpatialReady || !bounds.IsValid)
                return false;

            SpatialHandles.Clear();
            try
            {
                SpatialBroadphase.QueryHandles(bounds, SpatialHandles);
                diagnostics?.RecordQueriedHandleVisits(SpatialHandles.Count);
            }
            catch
            {
                SpatialBroadphase.ResetIncremental();
                SpatialReady = false;
                return false;
            }

            for (int index = 0; index < SpatialHandles.Count; index++)
            {
                RuntimeEntityHandle handle = SpatialHandles[index];
                int slot = handle.Slot;
                if (slot < 0 || slot >= Slots.Length ||
                    !world.TryResolveRuntimeHandle(handle, out LF2Entity entity) ||
                    !ReferenceEquals(entity, Slots[slot]))
                {
                    slots.Clear();
                    SpatialBroadphase.ResetIncremental();
                    SpatialReady = false;
                    return false;
                }
                slots.Add(slot);
            }

            return true;
        }

        internal bool TryCreateNearestSnapshotStamp(
            ulong currentOccupancyEpoch,
            out AiNearestSnapshotStamp stamp)
        {
            stamp = default;
            LF2Entity[] slots = Slots;
            uint[] generations = GroundGenerationBySlot;
            AiNearestSlotFacts[] facts = NearestFactsBySlot;
            ulong epoch = SlotSnapshotOccupancyEpoch;
            uint version = NearestFactsActiveVersion;
            int count = slots?.Length ?? 0;
            if (slots == null || epoch == 0 || version == 0 ||
                generations == null || facts == null ||
                generations.Length != count || facts.Length != count ||
                currentOccupancyEpoch != epoch)
            {
                return false;
            }

            stamp = new AiNearestSnapshotStamp
            {
                Slots = slots,
                GenerationBySlot = generations,
                FactsBySlot = facts,
                OccupancyEpoch = epoch,
                FactsVersion = version,
                SlotCount = count,
            };
            return true;
        }

        internal bool IsNearestSnapshotStampCurrent(
            in AiNearestSnapshotStamp stamp,
            ulong currentOccupancyEpoch)
        {
            return stamp.OccupancyEpoch != 0 &&
                   stamp.FactsVersion != 0 &&
                   ReferenceEquals(stamp.Slots, Slots) &&
                   ReferenceEquals(stamp.GenerationBySlot, GroundGenerationBySlot) &&
                   ReferenceEquals(stamp.FactsBySlot, NearestFactsBySlot) &&
                   stamp.SlotCount == Slots.Length &&
                   stamp.SlotCount == GroundGenerationBySlot.Length &&
                   stamp.SlotCount == NearestFactsBySlot.Length &&
                   SlotSnapshotOccupancyEpoch == stamp.OccupancyEpoch &&
                   currentOccupancyEpoch == stamp.OccupancyEpoch &&
                   NearestFactsActiveVersion == stamp.FactsVersion;
        }

        internal bool TryFindNearestGroundInSingleAllowedTeamPartition(
            SimulationWorld world,
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
            if (!GroundTeamPartitionsValid)
                return false;

            int allowedPartitionCount = CountAllowedGroundTeamPartitions(
                world.GetAiTeamForInputModule(self),
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
                        world.GetAiXForInputModule(self),
                        world.GetAiZForInputModule(self),
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

            InvalidateGroundTeamPartitions();
            handled = false;
            nearestHandle = RuntimeEntityHandle.Invalid;
            nearestDistance = 10000;
            visitedRecords = 0;
            return false;
        }

        internal static bool IsGroundTeamPartitionAllowed(
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

        internal int CountAllowedGroundTeamPartitions(
            int selfTeam,
            int inputPhase,
            out AiGroundTeamPartition singlePartition)
        {
            int count = 0;
            singlePartition = null;
            for (int index = 0; index < ActiveGroundTeamPartitions.Count; index++)
            {
                AiGroundTeamPartition partition = ActiveGroundTeamPartitions[index];
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

        internal static bool IsBetterTargetCandidate(
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

        internal bool IsGroundTargetCandidate(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase)
        {
            if (candidate == null || candidate == self)
                return false;
            int state = world.GetAiStateForInputModule(candidate);
            if (!world.IsCharacterDatForAiInputModule(candidate))
            {
                if (state != 3000)
                    return false;
                int candidateX = world.GetAiXForInputModule(candidate);
                int selfX = world.GetAiXForInputModule(self);
                if (candidateX > selfX)
                {
                    if (!(candidate.Runtime.Vx < 0.001))
                        return false;
                }
                else if (candidateX < selfX)
                {
                    if (!(candidate.Runtime.Vx > 0.001))
                        return false;
                }
                else
                {
                    return false;
                }
            }
            return TeamCandidateAllowed(
                       world.GetAiTeamForInputModule(self),
                       world.GetAiTeamForInputModule(candidate),
                       inputPhase) &&
                   world.GetAiHpForInputModule(candidate) > 0 &&
                   state != 14 &&
                   System.Math.Abs(world.GetAiYForInputModule(candidate)) <= 2;
        }

        internal static bool IsGroundTargetCandidate(
            LF2Entity self,
            int selfSlot,
            int selfX,
            int selfTeam,
            in AiNearestSlotFacts candidate,
            int inputPhase)
        {
            if (!candidate.Included || !candidate.Active ||
                candidate.Entity == null ||
                ReferenceEquals(candidate.Entity, self) ||
                candidate.Slot == selfSlot ||
                !candidate.GroundRole)
            {
                return false;
            }

            if (candidate.DataObjectType != 0)
            {
                if (candidate.State != 3000)
                    return false;
                if (candidate.X > selfX)
                {
                    if (!(candidate.Vx < 0.001))
                        return false;
                }
                else if (candidate.X < selfX)
                {
                    if (!(candidate.Vx > 0.001))
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return TeamCandidateAllowed(selfTeam, candidate.Team, inputPhase) &&
                   candidate.Hp > 0 &&
                   candidate.State != 14 &&
                   System.Math.Abs(candidate.Y) <= 2;
        }

        internal bool IsAirTargetCandidate(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase)
        {
            return candidate != null &&
                   candidate != self &&
                   TeamCandidateAllowed(
                       world.GetAiTeamForInputModule(self),
                       world.GetAiTeamForInputModule(candidate),
                       inputPhase) &&
                   world.GetAiHpForInputModule(candidate) > 0 &&
                   (world.GetAiStateForInputModule(candidate) == 14 ||
                    System.Math.Abs(world.GetAiYForInputModule(candidate)) > 2);
        }

        internal static bool IsAirTargetCandidate(
            LF2Entity self,
            int selfSlot,
            int selfTeam,
            in AiNearestSlotFacts candidate,
            int inputPhase)
        {
            return candidate.Included &&
                   candidate.Active &&
                   candidate.Entity != null &&
                   !ReferenceEquals(candidate.Entity, self) &&
                   candidate.Slot != selfSlot &&
                   TeamCandidateAllowed(selfTeam, candidate.Team, inputPhase) &&
                   candidate.Hp > 0 &&
                   candidate.AirRole;
        }

        internal bool IsAirSpatialRole(SimulationWorld world, LF2Entity candidate)
        {
            return candidate != null &&
                   (world.GetAiStateForInputModule(candidate) == 14 ||
                    System.Math.Abs(world.GetAiYForInputModule(candidate)) > 2);
        }

        internal bool IsGroundSpatialRole(
            SimulationWorld world,
            LF2Entity candidate)
        {
            if (candidate == null ||
                world.GetAiStateForInputModule(candidate) == 14 ||
                System.Math.Abs(world.GetAiYForInputModule(candidate)) > 2)
            {
                return false;
            }

            return world.IsCharacterDatForAiInputModule(candidate) ||
                   world.GetAiStateForInputModule(candidate) == 3000;
        }

        internal SpatialAabbXZ AroundPoint(
            SimulationWorld world,
            LF2Entity entity,
            int radiusX,
            int radiusZ)
        {
            int x = world.GetAiXForInputModule(entity);
            int z = world.GetAiZForInputModule(entity);
            return new SpatialAabbXZ(
                SaturatingAdd(x, -radiusX),
                SaturatingAdd(z, -radiusZ),
                SaturatingAdd(x, radiusX + 1),
                SaturatingAdd(z, radiusZ + 1));
        }

        internal static int SaturatingAdd(int value, int delta)
        {
            long result = (long)value + delta;
            if (result < int.MinValue)
                return int.MinValue;
            return result > int.MaxValue ? int.MaxValue : (int)result;
        }

        internal static bool TeamCandidateAllowed(
            int selfTeam,
            int candidateTeam,
            int inputPhase)
        {
            if (candidateTeam != selfTeam)
            {
                if (inputPhase != 1)
                    return true;
                if (selfTeam == 5)
                    return true;
            }
            if (candidateTeam != 5)
                return false;
            if (inputPhase != 1)
                return false;
            return candidateTeam != selfTeam;
        }

        internal static ulong CaptureNearestInputSignature(NTSDEntityRuntime input)
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
                    for (int index = 0; index < input.InputHistory.Length; index++)
                        hash = (hash ^ (uint)input.InputHistory[index]) * prime;
                }
            }
            return hash;
        }

        internal int FindNearestTargetSlot(
            SimulationWorld world,
            LF2Entity self,
            AiInputContext ai,
            out int bestDist,
            out bool sameZLane)
        {
            if (ForceFullNearestScan)
                return FindNearestTargetSlotBrute(
                    world,
                    self,
                    ai,
                    out bestDist,
                    out sameZLane);

            if (ai.InputPhase == 1 &&
                world.Team(self) != 5 &&
                Phase1TargetSlotsValid &&
                !ForceFullPhase1TargetScan)
            {
                BattleAiInputDetailDiagnostics diagnostics =
                    world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
                try
                {
                    return FindNearestPhase1TargetSlotIndexed(
                        world,
                        self,
                        out bestDist,
                        out sameZLane);
                }
                finally
                {
                    diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
                }
            }

            uint rngStateBefore = 0;
            ulong rngCallsBefore = 0;
            ulong inputSignatureBefore = 0;
            if (EnableNearestBestFirstShadow)
            {
                rngStateBefore = world.Rng?.State ?? 0;
                rngCallsBefore = world.Rng?.CallCount ?? 0;
                inputSignatureBefore = CaptureNearestInputSignature(self?.Runtime);
            }

            bool formalSucceeded;
            int selected;
            if (ForceLegacyNearestQuery)
            {
                formalSucceeded = TryFindNearestTargetSlotSpatial(
                    world,
                    self,
                    ai,
                    out selected,
                    out bestDist,
                    out sameZLane);
            }
            else
            {
                formalSucceeded = TryFindNearestTargetSlotBestFirst(
                    world,
                    self,
                    ai,
                    out selected,
                    out bestDist,
                    out sameZLane,
                    true);
            }

            if (formalSucceeded && EnableNearestBestFirstShadow)
            {
                bool shadowSucceeded;
                int shadowSelected;
                int shadowBestDist;
                bool shadowSameZLane;
                if (ForceLegacyNearestQuery)
                {
                    shadowSucceeded = TryFindNearestTargetSlotBestFirst(
                        world,
                        self,
                        ai,
                        out shadowSelected,
                        out shadowBestDist,
                        out shadowSameZLane,
                        true);
                }
                else
                {
                    shadowSucceeded = TryFindNearestTargetSlotSpatial(
                        world,
                        self,
                        ai,
                        out shadowSelected,
                        out shadowBestDist,
                        out shadowSameZLane);
                }

                uint rngStateAfter = world.Rng?.State ?? 0;
                ulong rngCallsAfter = world.Rng?.CallCount ?? 0;
                ulong inputSignatureAfter = CaptureNearestInputSignature(self?.Runtime);
                if (!shadowSucceeded ||
                    selected != shadowSelected ||
                    bestDist != shadowBestDist ||
                    sameZLane != shadowSameZLane ||
                    rngStateBefore != rngStateAfter ||
                    rngCallsBefore != rngCallsAfter ||
                    inputSignatureBefore != inputSignatureAfter)
                {
                    RecordNearestBestFirstShadowMismatch(
                        world,
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

            world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics?
                .RecordBruteFallback();
            return FindNearestTargetSlotBrute(
                world,
                self,
                ai,
                out bestDist,
                out sameZLane);
        }

        internal int FindNearestPhase1TargetSlotIndexed(
            SimulationWorld world,
            LF2Entity self,
            out int bestDist,
            out bool sameZLane)
        {
            int selected = -1;
            bestDist = 10000;
            sameZLane = false;
            world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics?
                .RecordPhase1ListVisits(Phase1TargetSlots.Count);
            for (int index = 0; index < Phase1TargetSlots.Count; index++)
            {
                int slot = Phase1TargetSlots[index];
                LF2Entity candidate = world.AiAt(slot);
                if (!IsGroundTargetCandidate(world, self, candidate, 1))
                    continue;

                int distance = world.Distance(self, candidate);
                if (IsBetterTargetCandidate(distance, slot, bestDist, selected))
                {
                    bestDist = distance;
                    selected = slot;
                }
            }

            sameZLane = selected >= 0 &&
                Math.Abs(world.Z(world.AiAt(selected)) - world.Z(self)) < 15;
            if (world.State(self) == 9)
                return selected;

            int bestAirDist = 10000;
            int airSelectedSlot = -1;
            RecordNearestAirPass();
            world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics?
                .RecordPhase1ListVisits(Phase1TargetSlots.Count);
            for (int index = 0; index < Phase1TargetSlots.Count; index++)
            {
                int slot = Phase1TargetSlots[index];
                LF2Entity candidate = world.AiAt(slot);
                if (!IsAirTargetCandidate(world, self, candidate, 1))
                    continue;

                int distance = world.Distance(self, candidate);
                if (!IsBetterTargetCandidate(
                        distance,
                        slot,
                        bestAirDist,
                        airSelectedSlot) ||
                    Math.Abs(world.Z(candidate) - world.Z(self)) >= 40 ||
                    Math.Abs(world.X(candidate) - world.X(self)) >= 250)
                {
                    continue;
                }

                bestAirDist = distance;
                airSelectedSlot = slot;
            }

            if (airSelectedSlot >= 0)
                selected = airSelectedSlot;
            return selected;
        }

        internal AiNearestPointFilter CreateNearestPointFilter(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            bool air)
        {
            NTSDEntityRuntime runtime = self?.Runtime;
            var filter = new AiNearestPointFilter
            {
                World = world,
                Module = this,
                SelfEntity = self,
                SelfSlot = runtime?.SlotIndex ?? -1,
                SelfX = runtime?.XInt ?? 0,
                SelfTeam = runtime?.RelationTeam ?? 0,
                InputPhase = inputPhase,
                Air = air,
                UseSnapshotFacts = !ForceLegacyNearestFilter,
            };
            if (filter.UseSnapshotFacts &&
                TryCreateNearestSnapshotStamp(
                    world.RuntimeSlotOccupancyEpochForServices,
                    out AiNearestSnapshotStamp stamp))
            {
                filter.Stamp = stamp;
            }
            return filter;
        }

        internal bool TryFindNearestTargetSlotBestFirst(
            SimulationWorld world,
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
            if (!SpatialReady && !GroundSpatialReady)
                return false;

            BattleAiInputDetailDiagnostics diagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
            try
            {
                AiNearestPointFilter filter = CreateNearestPointFilter(
                    world,
                    self,
                    ai.InputPhase,
                    false);
                bool partitionSucceeded =
                    TryFindNearestGroundInSingleAllowedTeamPartition(
                        world,
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
                    LooseQuadtreeBroadphase groundBroadphase = GroundSpatialReady
                        ? GroundSpatialBroadphase
                        : SpatialBroadphase;
                    succeeded = groundBroadphase.TryFindNearestPointManhattan(
                        world.X(self),
                        world.Z(self),
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
                        ReferenceEquals(groundBroadphase, GroundSpatialBroadphase))
                    {
                        GroundSpatialBroadphase.ResetIncremental();
                        GroundSpatialReady = false;
                        if (!SpatialReady)
                            return false;

                        diagnostics?.RecordSpatialQuery();
                        succeeded = SpatialBroadphase.TryFindNearestPointManhattan(
                            world.X(self),
                            world.Z(self),
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
                    SpatialBroadphase.ResetIncremental();
                    SpatialReady = false;
                    return false;
                }
                selected = nearestHandle.Slot;
            }
            catch
            {
                if (GroundSpatialReady)
                {
                    GroundSpatialBroadphase.ResetIncremental();
                    GroundSpatialReady = false;
                    return TryFindNearestTargetSlotBestFirst(
                        world,
                        self,
                        ai,
                        out selected,
                        out bestDist,
                        out sameZLane,
                        allowAirRoleFastPath);
                }

                SpatialBroadphase.ResetIncremental();
                SpatialReady = false;
                return false;
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
            }

            sameZLane = selected >= 0 &&
                Math.Abs(world.Z(world.AiAt(selected)) - world.Z(self)) < 15;
            if (world.State(self) == 9)
                return true;
            if (allowAirRoleFastPath && AirRoleCountValid && AirRoleCount == 0)
                return true;

            RecordNearestAirPass();
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestAir);
            try
            {
                AiNearestPointFilter filter = CreateNearestPointFilter(
                    world,
                    self,
                    ai.InputPhase,
                    true);
                diagnostics?.RecordSpatialQuery();
                LooseQuadtreeBroadphase airBroadphase = AirSpatialReady
                    ? AirSpatialBroadphase
                    : SpatialBroadphase;
                bool succeeded = airBroadphase.TryFindNearestPointManhattan(
                    world.X(self),
                    world.Z(self),
                    10000,
                    250,
                    40,
                    ref filter,
                    out RuntimeEntityHandle nearestAirHandle,
                    out _,
                    out int visitedRecords);
                diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                diagnostics?.RecordCandidateVisits(visitedRecords);
                if (!succeeded &&
                    ReferenceEquals(airBroadphase, AirSpatialBroadphase))
                {
                    ResetAirSpatialIndex();
                    diagnostics?.RecordSpatialQuery();
                    succeeded = SpatialBroadphase.TryFindNearestPointManhattan(
                        world.X(self),
                        world.Z(self),
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
                    SpatialBroadphase.ResetIncremental();
                    SpatialReady = false;
                    return false;
                }
                if (nearestAirHandle.IsValid)
                    selected = nearestAirHandle.Slot;
            }
            catch
            {
                if (AirSpatialReady)
                {
                    ResetAirSpatialIndex();
                    return TryFindNearestTargetSlotBestFirst(
                        world,
                        self,
                        ai,
                        out selected,
                        out bestDist,
                        out sameZLane,
                        allowAirRoleFastPath);
                }

                SpatialBroadphase.ResetIncremental();
                SpatialReady = false;
                return false;
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestAir);
            }
            return true;
        }

        internal bool TryFindNearestTargetSlotSpatial(
            SimulationWorld world,
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
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
            try
            {
                while (radius <= 10000)
                {
                    int boundedRadius = Math.Min(radius, 9999);
                    diagnostics?.RecordRadius(boundedRadius);
                    SpatialAabbXZ bounds = AroundPoint(
                        world,
                        self,
                        boundedRadius,
                        boundedRadius);
                    if (!TryQueryInputSlots(world, bounds, out List<int> slots))
                        return false;

                    diagnostics?.RecordCandidateVisits(slots.Count);
                    for (int index = 0; index < slots.Count; index++)
                    {
                        int slot = slots[index];
                        LF2Entity candidate = world.AiAt(slot);
                        if (!IsGroundTargetCandidate(
                                world,
                                self,
                                candidate,
                                ai.InputPhase))
                        {
                            continue;
                        }
                        int distance = world.Distance(self, candidate);
                        if (IsBetterTargetCandidate(
                                distance,
                                slot,
                                bestDist,
                                selected))
                        {
                            bestDist = distance;
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

            sameZLane = selected >= 0 &&
                Math.Abs(world.Z(world.AiAt(selected)) - world.Z(self)) < 15;
            if (world.State(self) == 9)
                return true;

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestAir);
            RecordNearestAirPass();
            try
            {
                if (!TryQueryInputSlots(
                        world,
                        AroundPoint(world, self, 249, 39),
                        out List<int> airSlots))
                {
                    return false;
                }

                int bestAirDist = 10000;
                int airSelectedSlot = -1;
                diagnostics?.RecordCandidateVisits(airSlots.Count);
                for (int index = 0; index < airSlots.Count; index++)
                {
                    int slot = airSlots[index];
                    LF2Entity candidate = world.AiAt(slot);
                    if (!IsAirTargetCandidate(
                            world,
                            self,
                            candidate,
                            ai.InputPhase))
                    {
                        continue;
                    }
                    int distance = world.Distance(self, candidate);
                    if (!IsBetterTargetCandidate(
                            distance,
                            slot,
                            bestAirDist,
                            airSelectedSlot) ||
                        Math.Abs(world.Z(candidate) - world.Z(self)) >= 40 ||
                        Math.Abs(world.X(candidate) - world.X(self)) >= 250)
                    {
                        continue;
                    }
                    bestAirDist = distance;
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

        internal int FindNearestTargetSlotBrute(
            SimulationWorld world,
            LF2Entity self,
            AiInputContext ai,
            out int bestDist,
            out bool sameZLane)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
            diagnostics?.RecordBruteSlotVisits(Slots.Length);
            int selected = FindNearestGroundTargetSlotBrute(
                world,
                self,
                ai.InputPhase,
                out bestDist);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);

            sameZLane = selected >= 0 &&
                Math.Abs(world.Z(world.AiAt(selected)) - world.Z(self)) < 15;
            if (world.State(self) != 9)
            {
                RecordNearestAirPass();
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestAir);
                diagnostics?.RecordBruteSlotVisits(Slots.Length);
                int bestAirDist = 10000;
                int airSelectedSlot = -1;
                for (int slot = 0; slot < Slots.Length; slot++)
                {
                    LF2Entity candidate = world.AiAt(slot);
                    if (!IsAirTargetCandidate(
                            world,
                            self,
                            candidate,
                            ai.InputPhase))
                    {
                        continue;
                    }
                    int distance = world.Distance(self, candidate);
                    if (!IsBetterTargetCandidate(
                            distance,
                            slot,
                            bestAirDist,
                            airSelectedSlot) ||
                        Math.Abs(world.Z(candidate) - world.Z(self)) >= 40 ||
                        Math.Abs(world.X(candidate) - world.X(self)) >= 250)
                    {
                        continue;
                    }
                    bestAirDist = distance;
                    airSelectedSlot = slot;
                }
                if (airSelectedSlot >= 0)
                    selected = airSelectedSlot;
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestAir);
            }
            return selected;
        }

        internal int FindNearestGroundTargetSlotBrute(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            out int bestDist)
        {
            int selected = -1;
            bestDist = 10000;
            for (int slot = 0; slot < Slots.Length; slot++)
            {
                LF2Entity candidate = world.AiAt(slot);
                if (!IsGroundTargetCandidate(world, self, candidate, inputPhase))
                    continue;

                int distance = world.Distance(self, candidate);
                if (IsBetterTargetCandidate(distance, slot, bestDist, selected))
                {
                    bestDist = distance;
                    selected = slot;
                }
            }
            return selected;
        }

        private void RecordNearestBestFirstShadowMismatch(
            SimulationWorld world,
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
            NearestBestFirstShadowMismatchCount++;
            if (NearestBestFirstFirstShadowMismatch != null)
                return;

            NearestBestFirstFirstShadowMismatch =
                $"slot={world.Slot(self)} formal={formalSucceeded}:{formalSelected}/{formalBestDist}/{formalSameZLane} " +
                $"shadow={shadowSucceeded}:{shadowSelected}/{shadowBestDist}/{shadowSameZLane} " +
                $"rng={rngStateBefore}:{rngCallsBefore}->{rngStateAfter}:{rngCallsAfter} " +
                $"input={inputSignatureBefore}->{inputSignatureAfter}";
        }

        internal void UpdateMoveModeScan(
            SimulationWorld world,
            AiSensingMode sensingMode,
            AiSoASensingRows sensingRows,
            bool useDecisionRows,
            LF2Entity self,
            ref AiInputContext ai)
        {
            if (ai.InputPhase != 1 || world.Team(self) == 5)
                return;

            if (useDecisionRows)
            {
                UpdateMoveModeScanRows(world, sensingRows, self, ref ai);
                return;
            }

            if (ForceFullMoveModeScan ||
                !MoveModeFirst10Valid ||
                !IsMoveModeSnapshotSelfCurrent(
                    world,
                    sensingMode,
                    sensingRows,
                    self))
            {
                UpdateMoveModeScanFull(world, self, ref ai);
                return;
            }

            int candidateSlot = MoveModeTopSlot == world.Slot(self)
                ? MoveModeSecondSlot
                : MoveModeTopSlot;
            if (candidateSlot < 0)
                return;

            int rightmostX = candidateSlot == MoveModeTopSlot
                ? MoveModeTopX
                : MoveModeSecondX;
            int rightmostZ = candidateSlot == MoveModeTopSlot
                ? MoveModeTopZ
                : MoveModeSecondZ;
            ApplyMoveModeFromRightmost(
                world,
                self,
                rightmostX,
                rightmostZ,
                ref ai);
        }

        internal void UpdateMoveModeScanRows(
            SimulationWorld world,
            AiSoASensingRows rows,
            LF2Entity self,
            ref AiInputContext ai)
        {
            int selfSlot = world.Slot(self);
            int rightmostX = -1;
            int rightmostZ = 0;
            int count = Math.Min(10, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics?
                    .RecordPhaseSlotVisits(
                        BattleAiInputDetailPhase.ContextMoveMode,
                        1);

                if (slot == selfSlot ||
                    !rows.Included[slot] ||
                    rows.DataObjectType[slot] != 0 ||
                    rows.Hp[slot] <= 0)
                {
                    continue;
                }

                if (rows.X[slot] > rightmostX)
                {
                    rightmostX = rows.X[slot];
                    rightmostZ = rows.Z[slot];
                }
            }

            if (rightmostX >= 0)
            {
                ApplyMoveModeFromRightmost(
                    world,
                    self,
                    rightmostX,
                    rightmostZ,
                    ref ai);
            }
        }

        internal bool IsMoveModeSnapshotSelfCurrent(
            SimulationWorld world,
            AiSensingMode sensingMode,
            AiSoASensingRows sensingRows,
            LF2Entity self)
        {
            if (self?.Runtime == null)
                return false;

            world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics?
                .RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    1);
            int slot = world.Slot(self);
            if (sensingMode == AiSensingMode.SoAAiSensing)
            {
                return slot >= 0 &&
                       slot < Slots.Length &&
                       sensingRows != null &&
                       slot < sensingRows.Capacity &&
                       ReferenceEquals(Slots[slot], self) &&
                       sensingRows.Included[slot] &&
                       self.Runtime.StableId == sensingRows.Identity[slot] &&
                       world.TryGetCurrentRuntimeHandle(
                           slot,
                           self,
                           out RuntimeEntityHandle candidateHandle) &&
                       candidateHandle.Generation == sensingRows.Generation[slot];
            }

            return slot >= 0 &&
                   slot < Slots.Length &&
                   ReferenceEquals(Slots[slot], self) &&
                   world.TryGetCurrentRuntimeHandle(
                       slot,
                       self,
                       out RuntimeEntityHandle handle) &&
                   handle.Generation == Phase1GenerationBySlot[slot];
        }

        internal void UpdateMoveModeScanFull(
            SimulationWorld world,
            LF2Entity self,
            ref AiInputContext ai)
        {
            int rightmostX = -1;
            int rightmostZ = 0;
            BattleAiInputDetailDiagnostics diagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            for (int slot = 0; slot < 10; slot++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    1);
                LF2Entity candidate = CurrentMoveModeCandidateAt(world, slot);
                if (candidate == null ||
                    candidate == self ||
                    !world.IsLivingCharacterDat(candidate))
                {
                    continue;
                }

                if (world.X(candidate) > rightmostX)
                {
                    rightmostX = world.X(candidate);
                    rightmostZ = world.Z(candidate);
                }
            }
            if (rightmostX < 0)
                return;

            ApplyMoveModeFromRightmost(
                world,
                self,
                rightmostX,
                rightmostZ,
                ref ai);
        }

        internal LF2Entity CurrentMoveModeCandidateAt(
            SimulationWorld world,
            int slot)
        {
            if (!world.TryGetRuntimeSlotReadOnlyView(
                    slot,
                    out RuntimeSlotTable.ReadOnlySlotView view) ||
                !view.Claimed ||
                view.Entity == null ||
                !world.IsActiveForCurrentPassInternal(view.Entity))
            {
                return null;
            }
            return view.Entity;
        }

        internal static void ApplyMoveModeFromRightmost(
            SimulationWorld world,
            LF2Entity self,
            int rightmostX,
            int rightmostZ,
            ref AiInputContext ai)
        {
            if (world.X(self) > rightmostX &&
                world.X(self) + Math.Abs(world.Z(self) - rightmostZ) / 2 -
                    rightmostX > 200)
            {
                ai.MoveMode = 1;
            }
            if (world.X(self) > rightmostX + 400)
                ai.MoveMode = 2;
        }

        private RuntimeSlotTable _runtimeSlots => world.RuntimeSlotsForServices;
        private LF2Entity[] aiInputSlots
        {
            get => Slots;
            set => Slots = value;
        }
        private LooseQuadtreeBroadphase aiInputSpatialBroadphase => SpatialBroadphase;
        private LooseQuadtreeBroadphase aiInputGroundSpatialBroadphase =>
            GroundSpatialBroadphase;
        private bool aiInputSpatialReady
        {
            get => SpatialReady;
            set => SpatialReady = value;
        }
        private bool aiInputGroundSpatialReady
        {
            get => GroundSpatialReady;
            set => GroundSpatialReady = value;
        }
        private bool aiInputAirSpatialReady
        {
            get => AirSpatialReady;
            set => AirSpatialReady = value;
        }
        private bool aiInputGroundTeamPartitionsValid => GroundTeamPartitionsValid;
        private int aiInputAirRoleCount => AirRoleCount;
        private bool aiInputAirRoleCountValid => AirRoleCountValid;
        private uint[] aiInputGroundGenerationBySlot
        {
            get => GroundGenerationBySlot;
            set => GroundGenerationBySlot = value;
        }
        private int[] aiInputGroundXBySlot => GroundXBySlot;
        private int[] aiInputGroundZBySlot => GroundZBySlot;
        private ulong aiInputSlotSnapshotOccupancyEpoch
        {
            get => SlotSnapshotOccupancyEpoch;
            set => SlotSnapshotOccupancyEpoch = value;
        }
        private AiNearestSlotFacts[] aiNearestFactsBySlot
        {
            get => NearestFactsBySlot;
            set => NearestFactsBySlot = value;
        }
        private uint aiNearestFactsActiveVersion
        {
            get => NearestFactsActiveVersion;
            set => NearestFactsActiveVersion = value;
        }
        private List<int> aiPhase1TargetSlots => Phase1TargetSlots;
        private bool aiPhase1TargetSlotsValid => Phase1TargetSlotsValid;
        private List<int> aiSpecialScanSlots => SpecialScanSlots;
        private Dictionary<int, AiTeamHpSummary> aiTeamHpSummaries => TeamHpSummaries;
        private bool aiTeamHpSummaryValid => TeamHpSummaryValid;
        private bool aiMoveModeFirst10Valid => MoveModeFirst10Valid;
        private int aiMoveModeTopSlot => MoveModeTopSlot;
        private int aiMoveModeSecondSlot => MoveModeSecondSlot;
        private bool ForceFullAiMoveModeScanForDiagnostics
        {
            get => ForceFullMoveModeScan;
            set => ForceFullMoveModeScan = value;
        }
        private bool ForceFullAiNearestScanForDiagnostics
        {
            get => ForceFullNearestScan;
            set => ForceFullNearestScan = value;
        }
        private bool ForceFullAiPhase1TargetScanForDiagnostics
        {
            get => ForceFullPhase1TargetScan;
            set => ForceFullPhase1TargetScan = value;
        }
        private bool ForceFullAiSameTeamScanForDiagnostics
        {
            get => ForceFullSameTeamScan;
            set => ForceFullSameTeamScan = value;
        }
        private bool ForceLegacyAiNearestFilterForDiagnostics
        {
            get => ForceLegacyNearestFilter;
            set => ForceLegacyNearestFilter = value;
        }
        private bool ForceLegacyAiNearestQueryForDiagnostics
        {
            get => ForceLegacyNearestQuery;
            set => ForceLegacyNearestQuery = value;
        }
        private bool EnableAiNearestBestFirstShadowForDiagnostics
        {
            get => EnableNearestBestFirstShadow;
            set => EnableNearestBestFirstShadow = value;
        }
        private int AiNearestAirPassCountForDiagnostics => NearestAirPassCount;
        private DeterministicRng Rng => world.Rng;
        private ulong RuntimeSlotOccupancyEpochForServices =>
            world.RuntimeSlotOccupancyEpochForServices;

        private void BuildAiInputSlotSnapshot() => world.BuildAiInputSlotSnapshot();
        private void ClearAiInputSlotSnapshot() => world.ClearAiInputSlotSnapshot();
        private int Team(LF2Entity entity) => world.Team(entity);
        private int Hp(LF2Entity entity) => world.Hp(entity);
        private int Hp3(LF2Entity entity) => world.Hp3(entity);
        private int X(LF2Entity entity) => world.X(entity);
        private int Z(LF2Entity entity) => world.Z(entity);
        private int Slot(LF2Entity entity) => world.Slot(entity);
        private bool IsLivingCharacterDat(LF2Entity entity) =>
            world.IsLivingCharacterDat(entity);
        private static bool IsAiSpecialScanObjectId(int objectId) =>
            IsSpecialScanObjectId(objectId);
        private bool ResolveAiSameTeamSummaryExcludingSelf(
            LF2Entity self,
            int selfTeam,
            out int otherCount,
            out int otherMinHp) =>
            world.ResolveAiSameTeamSummaryExcludingSelf(
                self,
                selfTeam,
                out otherCount,
                out otherMinHp);
        private void AiUpdateMoveModeScan(
            LF2Entity self,
            ref AiInputContext ai) =>
            world.AiUpdateMoveModeScan(self, ref ai);
        private int FindNearestAiTargetSlot(
            LF2Entity self,
            AiInputContext ai,
            out int bestDist,
            out bool sameZLane) =>
            FindNearestTargetSlot(world, self, ai, out bestDist, out sameZLane);
        private int FindNearestAiTargetSlotBrute(
            LF2Entity self,
            AiInputContext ai,
            out int bestDist,
            out bool sameZLane) =>
            FindNearestTargetSlotBrute(world, self, ai, out bestDist, out sameZLane);
        private int FindNearestGroundAiTargetSlotBrute(
            LF2Entity self,
            int inputPhase,
            out int bestDist) =>
            FindNearestGroundTargetSlotBrute(world, self, inputPhase, out bestDist);
        private bool TryFindNearestAiTargetSlotBestFirst(
            LF2Entity self,
            AiInputContext ai,
            out int selected,
            out int bestDist,
            out bool sameZLane,
            bool allowAirRoleFastPath = false) =>
            TryFindNearestTargetSlotBestFirst(
                world,
                self,
                ai,
                out selected,
                out bestDist,
                out sameZLane,
                allowAirRoleFastPath);
        private bool TryFindNearestAiTargetSlotSpatial(
            LF2Entity self,
            AiInputContext ai,
            out int selected,
            out int bestDist,
            out bool sameZLane) =>
            TryFindNearestTargetSlotSpatial(
                world,
                self,
                ai,
                out selected,
                out bestDist,
                out sameZLane);
        private bool TryFindNearestGroundInSingleAllowedTeamPartition(
            LF2Entity self,
            int inputPhase,
            ref AiNearestPointFilter filter,
            BattleAiInputDetailDiagnostics diagnostics,
            out bool handled,
            out RuntimeEntityHandle nearestHandle,
            out int nearestDistance,
            out int visitedRecords) =>
            TryFindNearestGroundInSingleAllowedTeamPartition(
                world,
                self,
                inputPhase,
                ref filter,
                diagnostics,
                out handled,
                out nearestHandle,
                out nearestDistance,
                out visitedRecords);
        private AiNearestPointFilter CreateAiNearestPointFilter(
            LF2Entity self,
            int inputPhase,
            bool air) =>
            CreateNearestPointFilter(world, self, inputPhase, air);
        private bool TryCreateAiNearestSnapshotStamp(
            out AiNearestSnapshotStamp stamp) =>
            TryCreateNearestSnapshotStamp(
                world.RuntimeSlotOccupancyEpochForServices,
                out stamp);
        private bool IsAiNearestSnapshotStampCurrent(
            in AiNearestSnapshotStamp stamp) =>
            IsNearestSnapshotStampCurrent(
                stamp,
                world.RuntimeSlotOccupancyEpochForServices);
        private void InvalidateAiAirRoleSnapshot() => InvalidateAirRoleSnapshot();
        private void InvalidateAiGroundTeamPartitions() =>
            InvalidateGroundTeamPartitions();
        private void ObserveAiAirSpatialRoleMutation(LF2Entity entity) =>
            ObserveAirSpatialRoleMutation(world, entity);
        private void ObserveAiGroundSpatialRoleMutation(LF2Entity entity) =>
            ObserveGroundSpatialRoleMutation(world, entity);
        private void ObserveAiTeamHpSummaryMutation(LF2Entity entity) =>
            ObserveTeamHpSummaryMutation(world, entity);
        private bool TryGetCurrentRuntimeHandle(
            int slot,
            LF2Entity entity,
            out RuntimeEntityHandle handle) =>
            world.TryGetCurrentRuntimeHandle(slot, entity, out handle);
        private void Register(LF2Entity entity) => world.Register(entity);
        private void Unregister(LF2Entity entity) => world.Unregister(entity);
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
                ObserveAiGroundSpatialRoleMutation(candidate);
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
                AiNearestPointFilter filter = CreateAiNearestPointFilter(
                    self,
                    inputPhase,
                    false);
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
        internal void CaptureAiNearestFactsTargetForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceLiveFacts,
            bool forceBrute,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            bool oldFilter = ForceLegacyAiNearestFilterForDiagnostics;
            bool oldFull = ForceFullAiNearestScanForDiagnostics;
            bool oldPhase1 = ForceFullAiPhase1TargetScanForDiagnostics;
            bool oldQuery = ForceLegacyAiNearestQueryForDiagnostics;
            BuildAiInputSlotSnapshot();
            try
            {
                if (forceLiveFacts)
                    ForceLegacyAiNearestFilterForDiagnostics = true;
                ForceFullAiNearestScanForDiagnostics = forceBrute;
                ForceFullAiPhase1TargetScanForDiagnostics = true;
                ForceLegacyAiNearestQueryForDiagnostics = false;
                var ai = new AiInputContext { InputPhase = inputPhase };
                selected = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out bestDist,
                    out sameZLane);
            }
            finally
            {
                ForceLegacyAiNearestFilterForDiagnostics = oldFilter;
                ForceFullAiNearestScanForDiagnostics = oldFull;
                ForceFullAiPhase1TargetScanForDiagnostics = oldPhase1;
                ForceLegacyAiNearestQueryForDiagnostics = oldQuery;
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiNearestSnapshotStampRejectsMutationForSelfCheck(
            int mutationKind)
        {
            BuildAiInputSlotSnapshot();
            LF2Entity[] oldSlots = aiInputSlots;
            uint[] oldGenerations = aiInputGroundGenerationBySlot;
            AiNearestSlotFacts[] oldFacts = aiNearestFactsBySlot;
            ulong oldEpoch = aiInputSlotSnapshotOccupancyEpoch;
            uint oldVersion = aiNearestFactsActiveVersion;
            try
            {
                if (!TryCreateAiNearestSnapshotStamp(
                        out AiNearestSnapshotStamp stamp))
                {
                    return false;
                }

                switch (mutationKind)
                {
                    case 0:
                        aiInputSlotSnapshotOccupancyEpoch =
                            oldEpoch == ulong.MaxValue ? 1UL : oldEpoch + 1UL;
                        break;
                    case 1:
                        aiNearestFactsActiveVersion =
                            oldVersion == uint.MaxValue ? 1u : oldVersion + 1u;
                        break;
                    case 2:
                        aiInputSlots = (LF2Entity[])oldSlots.Clone();
                        break;
                    case 3:
                        aiInputGroundGenerationBySlot =
                            (uint[])oldGenerations.Clone();
                        break;
                    case 4:
                        aiNearestFactsBySlot =
                            (AiNearestSlotFacts[])oldFacts.Clone();
                        break;
                    default:
                        return false;
                }

                return !IsAiNearestSnapshotStampCurrent(in stamp);
            }
            finally
            {
                aiInputSlots = oldSlots;
                aiInputGroundGenerationBySlot = oldGenerations;
                aiNearestFactsBySlot = oldFacts;
                aiInputSlotSnapshotOccupancyEpoch = oldEpoch;
                aiNearestFactsActiveVersion = oldVersion;
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiNearestFactsValidationFallbackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int invalidationKind,
            out bool fastAborted)
        {
            fastAborted = false;
            BuildAiInputSlotSnapshot();
            bool oldDormant = candidate?.Runtime?.OidMergeDormant ?? false;
            bool oldPending = candidate?.Runtime?.PendingFlushDestroy ?? false;
            try
            {
                int slot = Slot(candidate);
                if (slot < 0 || slot >= aiNearestFactsBySlot.Length)
                    return false;

                AiNearestSlotFacts facts = aiNearestFactsBySlot[slot];
                switch (invalidationKind)
                {
                    case 0:
                        facts.SnapshotVersion = facts.SnapshotVersion == uint.MaxValue
                            ? 1u
                            : facts.SnapshotVersion + 1u;
                        aiNearestFactsBySlot[slot] = facts;
                        break;
                    case 1:
                        facts.HandleGeneration = facts.HandleGeneration == uint.MaxValue
                            ? 1u
                            : facts.HandleGeneration + 1u;
                        aiNearestFactsBySlot[slot] = facts;
                        break;
                    case 2:
                        facts.Entity = self;
                        aiNearestFactsBySlot[slot] = facts;
                        break;
                    case 3:
                        candidate.Runtime.OidMergeDormant = true;
                        ObserveAiTeamHpSummaryMutation(candidate);
                        break;
                    case 4:
                        candidate.Runtime.PendingFlushDestroy = true;
                        ObserveAiTeamHpSummaryMutation(candidate);
                        break;
                    default:
                        return false;
                }

                return AiNearestFastAbortAndFallbackMatchesFullForSelfCheck(
                    self,
                    inputPhase,
                    out fastAborted);
            }
            finally
            {
                if (candidate?.Runtime != null)
                {
                    candidate.Runtime.OidMergeDormant = oldDormant;
                    candidate.Runtime.PendingFlushDestroy = oldPending;
                }
                ClearAiInputSlotSnapshot();
            }
        }
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
            AiNearestPointFilter filter = CreateAiNearestPointFilter(
                    self,
                    inputPhase,
                    false);
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

    }
}
