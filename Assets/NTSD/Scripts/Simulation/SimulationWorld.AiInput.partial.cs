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
            new Dictionary<int, AiGroundTeamPartition>(2);
        private readonly List<AiGroundTeamPartition> aiInputActiveGroundTeamPartitions =
            new List<AiGroundTeamPartition>(2);
        private readonly AiGroundTeamPartition[] aiInputGroundTeamPartitionPool =
        {
            new AiGroundTeamPartition(),
            new AiGroundTeamPartition(),
        };
        private bool aiInputGroundTeamPartitionOverflow;
        private readonly LooseQuadtreeBroadphase aiInputAirSpatialBroadphase =
            new LooseQuadtreeBroadphase();
        private readonly List<IncrementalSpatialEntry> aiInputAirSpatialEntries =
            new List<IncrementalSpatialEntry>(32);
        private readonly List<int> aiSpecialScanSlots = new List<int>(32);
        private readonly List<int> aiPhase1TargetSlots = new List<int>(32);
        private int[] aiPhase1TeamBySlot;
        private uint[] aiPhase1GenerationBySlot;
        private bool aiPhase1TargetSlotsValid;
        private bool[] aiMoveModeFirst10Present = new bool[10];
        private bool[] aiMoveModeFirst10Eligible = new bool[10];
        private uint[] aiMoveModeFirst10Generation = new uint[10];
        private int[] aiMoveModeFirst10Hp = new int[10];
        private int[] aiMoveModeFirst10X = new int[10];
        private int[] aiMoveModeFirst10Z = new int[10];
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
        private AiNearestSlotFacts[] aiNearestFactsBySlot;
        private uint aiNearestFactsVersionCounter;
        private uint aiNearestFactsActiveVersion;
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
        public bool ForceLegacyAiNearestFilterForDiagnostics { get; set; }
        internal bool EnableAiNearestBestFirstShadowForDiagnostics { get; set; }
        public int AiSameTeamSummaryFallbackCountForDiagnostics { get; private set; }
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

        private struct AiNearestSlotFacts
        {
            public LF2Entity Entity;
            public uint HandleGeneration;
            public uint SnapshotVersion;
            public ulong OccupancyEpoch;
            public int Slot;
            public int X;
            public int Y;
            public int Z;
            public int Hp;
            public int Team;
            public int State;
            public int DataObjectType;
            public double Vx;
            public bool Active;
            public bool Included;
            public bool GroundRole;
            public bool AirRole;
        }

        private struct AiNearestSnapshotStamp
        {
            public LF2Entity[] Slots;
            public uint[] GenerationBySlot;
            public AiNearestSlotFacts[] FactsBySlot;
            public ulong OccupancyEpoch;
            public uint FactsVersion;
            public int SlotCount;
        }

        private struct AiNearestPointFilter : IIncrementalPointNearestFilter
        {
            public SimulationWorld World;
            public AiNearestSnapshotStamp Stamp;
            public LF2Entity SelfEntity;
            public int SelfSlot;
            public int SelfX;
            public int SelfTeam;
            public int InputPhase;
            public bool Air;
            public bool UseSnapshotFacts;

            public IncrementalPointFilterDecision Evaluate(RuntimeEntityHandle handle)
            {
                int slot = handle.Slot;
                if (World == null || !handle.IsValid)
                    return IncrementalPointFilterDecision.Abort;

                if (UseSnapshotFacts)
                {
                    if (!World.IsAiNearestSnapshotStampCurrent(in Stamp) ||
                        slot < 0 ||
                        slot >= Stamp.SlotCount ||
                        handle.Generation != Stamp.GenerationBySlot[slot])
                    {
                        return IncrementalPointFilterDecision.Abort;
                    }

                    LF2Entity candidate = Stamp.Slots[slot];
                    ref readonly AiNearestSlotFacts facts = ref Stamp.FactsBySlot[slot];
                    if (facts.SnapshotVersion != Stamp.FactsVersion ||
                        facts.OccupancyEpoch != Stamp.OccupancyEpoch ||
                        facts.Slot != slot ||
                        facts.HandleGeneration != handle.Generation ||
                        !facts.Included ||
                        !facts.Active ||
                        !ReferenceEquals(facts.Entity, candidate))
                    {
                        return IncrementalPointFilterDecision.Abort;
                    }

                    bool accepted = Air
                        ? IsAirAiTargetCandidate(
                            SelfEntity,
                            SelfSlot,
                            SelfTeam,
                            in facts,
                            InputPhase)
                        : IsGroundAiTargetCandidate(
                            SelfEntity,
                            SelfSlot,
                            SelfX,
                            SelfTeam,
                            in facts,
                            InputPhase);
                    return accepted
                        ? IncrementalPointFilterDecision.Accept
                        : IncrementalPointFilterDecision.Reject;
                }

                if (World.aiInputSlotSnapshotOccupancyEpoch == 0 ||
                    World.RuntimeSlotOccupancyEpochForServices !=
                        World.aiInputSlotSnapshotOccupancyEpoch ||
                    slot < 0 ||
                    slot >= World.aiInputSlots.Length ||
                    World.aiInputGroundGenerationBySlot == null ||
                    slot >= World.aiInputGroundGenerationBySlot.Length ||
                    handle.Generation != World.aiInputGroundGenerationBySlot[slot])
                {
                    return IncrementalPointFilterDecision.Abort;
                }

                LF2Entity live = World.aiInputSlots[slot];
                if (live?.Runtime == null || live.Runtime.SlotIndex != slot)
                    return IncrementalPointFilterDecision.Abort;

                bool liveAccepted = Air
                    ? World.IsAirAiTargetCandidate(SelfEntity, live, InputPhase)
                    : World.IsGroundAiTargetCandidate(SelfEntity, live, InputPhase);
                return liveAccepted
                    ? IncrementalPointFilterDecision.Accept
                    : IncrementalPointFilterDecision.Reject;
            }
        }
        private void BuildAiInputSlotSnapshot()
        {
            aiInputSlotSnapshotOccupancyEpoch = 0;
            aiNearestFactsActiveVersion = 0;
            bool useSoACandidate = aiSensingMode == AiSensingMode.SoAAiSensing;
            uint factsVersion = useSoACandidate ? 0 : AdvanceAiNearestFactsVersion();
            int runtimeCapacityBefore = RuntimeSlotCapacity;
            ulong occupancyEpochBefore = RuntimeSlotOccupancyEpochForServices;
            BattleAiInputDetailDiagnostics diagnostics = ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            AiSameTeamSummaryFallbackCountForDiagnostics = 0;
            ResetAiNearestAirPassCountForSelfCheck();
            if (aiUnifiedSnapshotExecutionMode ==
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority &&
                TryPrepareAiUnifiedSnapshotExecutionPass(diagnostics))
            {
                return;
            }
            if (!useSoACandidate)
                EnsureAiTeamHpSnapshotCapacity();
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);
            if (useSoACandidate)
            {
                CaptureAiSoACandidateFusedSnapshot(
                    runtimeCapacityBefore,
                    occupancyEpochBefore);
            }
            else
            {
                Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
                GetAllEntities(_entityScratch);
                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    int slot = entity?.Runtime?.SlotIndex ?? -1;
                    if (slot >= 0 &&
                        slot < aiInputSlots.Length &&
                        IsActiveForCurrentPass(entity))
                    {
                        aiInputSlots[slot] = entity;
                    }
                }
                _entityScratch.Clear();
                if (aiSensingMode == AiSensingMode.SoAShadowAiSensing)
                    CaptureAiSoASensingShadowSnapshot(occupancyEpochBefore);
            }
            bool factsProven = !useSoACandidate &&
                               CaptureAiNearestFactsSnapshot(
                                   factsVersion,
                                   occupancyEpochBefore);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);
            if (useSoACandidate)
            {
                BuildAiCandidateSnapshotProducts();
            }
            else
            {
                BuildAiSnapshotIndices();
            }
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);
            if (!useSoACandidate)
                SynchronizeAiInputSpatialSnapshot();
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);
            ulong occupancyEpochAfter = RuntimeSlotOccupancyEpochForServices;
            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing ||
                aiSensingMode == AiSensingMode.SoAAiSensing)
                ObserveAiSoASensingSnapshotBuildEpoch(occupancyEpochBefore, occupancyEpochAfter);
            if (occupancyEpochBefore == occupancyEpochAfter)
            {
                aiInputSlotSnapshotOccupancyEpoch = occupancyEpochAfter;
                if (factsProven) aiNearestFactsActiveVersion = factsVersion;
            }
            PrepareAiUnifiedSnapshotShadowPass(diagnostics);
        }

        private void ClearAiInputSlotSnapshot()
        {
            bool usedUnifiedAuthority =
                AiUnifiedSnapshotExecutionFallbackForbidden;
            bool preserveRollingSnapshot =
                aiUnifiedSnapshotExecutionMode ==
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority &&
                aiUnifiedSnapshotPublishedState != null &&
                battleAiUnifiedRowPublisher.Active;
            if (preserveRollingSnapshot)
                SuspendAiUnifiedSnapshotExecutionPass();
            else
                EndAiUnifiedSnapshotExecutionPass();
            if (usedUnifiedAuthority)
                RestoreAiUnifiedSnapshotLegacyConsumerBuffers();
            EndAiUnifiedSnapshotShadowPass();
            bool useSoACandidate = aiSensingMode == AiSensingMode.SoAAiSensing;
            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing ||
                aiSensingMode == AiSensingMode.SoAAiSensing)
                ClearAiSoASensingShadowSnapshot();
            aiInputSlotSnapshotOccupancyEpoch = 0;
            aiNearestFactsActiveVersion = 0;
            if (useSoACandidate)
            {
                Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
                ResetAiMoveModeFirst10Snapshot();
                aiTeamHpSummaryValid = false;
                aiPhase1TargetSlotsValid = false;
                aiInputSpatialReady = false;
                aiInputGroundSpatialReady = false;
                aiInputGroundTeamPartitionsValid = false;
                aiInputAirSpatialReady = false;
                aiInputAirRoleCount = 0;
                aiInputAirRoleCountValid = false;
                return;
            }
            if (aiNearestFactsBySlot != null)
                Array.Clear(aiNearestFactsBySlot, 0, aiNearestFactsBySlot.Length);
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
            AiLegacySnapshotIndexBuildCountForDiagnostics++;
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

        // Candidate captures the shared first-ten move-mode snapshot in the fused
        // runtime-slot scan. In particular, do not populate Legacy team summaries,
        // special lists, phase-1 targets, nearest facts, or any quadtree state here.
        private void BuildAiCandidateSnapshotProducts()
        {
            aiTeamHpSummaryValid = false;
            aiPhase1TargetSlotsValid = false;
            aiInputSpatialReady = false;
            aiInputGroundSpatialReady = false;
            aiInputGroundTeamPartitionsValid = false;
            aiInputAirSpatialReady = false;
            aiInputAirRoleCount = 0;
            aiInputAirRoleCountValid = false;
        }

        private void EnsureAiTeamHpSnapshotCapacity()
        {
            if (aiTeamHpSnapshotEligible?.Length == aiInputSlots.Length &&
                aiInputGroundRoleBySlot?.Length == aiInputSlots.Length &&
                aiInputGroundTeamBySlot?.Length == aiInputSlots.Length &&
                aiInputGroundGenerationBySlot?.Length == aiInputSlots.Length &&
                aiInputAirRoleBySlot?.Length == aiInputSlots.Length &&
                aiNearestFactsBySlot?.Length == aiInputSlots.Length &&
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
            aiNearestFactsBySlot = new AiNearestSlotFacts[aiInputSlots.Length];
            aiPhase1TeamBySlot = new int[aiInputSlots.Length];
            aiPhase1GenerationBySlot = new uint[aiInputSlots.Length];
        }

        private uint AdvanceAiNearestFactsVersion()
        {
            unchecked
            {
                aiNearestFactsVersionCounter++;
                if (aiNearestFactsVersionCounter == 0)
                    aiNearestFactsVersionCounter = 1;
            }
            return aiNearestFactsVersionCounter;
        }

        private bool CaptureAiNearestFactsSnapshot(
            uint snapshotVersion,
            ulong occupancyEpoch)
        {
            AiLegacyNearestFactsBuildCountForDiagnostics++;
            Array.Clear(
                aiNearestFactsBySlot,
                0,
                aiNearestFactsBySlot.Length);
            bool proven = true;
            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                LF2Entity entity = aiInputSlots[slot];
                if (entity == null)
                    continue;

                if (!TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                    !TryCaptureAiNearestSlotFacts(
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

                aiNearestFactsBySlot[slot] = facts;
            }
            return proven;
        }

        private bool TryCaptureAiNearestSlotFacts(
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
                slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], entity) ||
                entity.Runtime.SlotIndex != slot ||
                !IsActiveForCurrentPass(entity))
            {
                return false;
            }

            NTSDEntityRuntime runtime = entity.Runtime;
            int state = State(entity);
            int dataObjectType =
                entity.GetCurrentDataObjectTypeForSimulation();
            int y = runtime.YInt;
            bool airRole = state == 14 || Abs(y) > 2;
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
                GroundRole =
                    !airRole &&
                    (dataObjectType == 0 || state == 3000),
                AirRole = airRole,
            };
            return true;
        }

        private void ObserveAiNearestFactsMutation(LF2Entity entity)
        {
            if (aiNearestFactsActiveVersion == 0)
                return;
            if (RuntimeSlotOccupancyEpochForServices !=
                aiInputSlotSnapshotOccupancyEpoch ||
                entity?.Runtime == null)
            {
                aiNearestFactsActiveVersion = 0;
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (slot < 0 ||
                slot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle))
            {
                aiNearestFactsActiveVersion = 0;
                return;
            }

            AiNearestSlotFacts previous = aiNearestFactsBySlot[slot];
            if (previous.SnapshotVersion !=
                    aiNearestFactsActiveVersion ||
                previous.OccupancyEpoch !=
                    aiInputSlotSnapshotOccupancyEpoch ||
                previous.HandleGeneration != handle.Generation ||
                !previous.Included ||
                !previous.Active ||
                !ReferenceEquals(previous.Entity, entity) ||
                !TryCaptureAiNearestSlotFacts(
                    entity,
                    slot,
                    handle.Generation,
                    aiNearestFactsActiveVersion,
                    aiInputSlotSnapshotOccupancyEpoch,
                    out AiNearestSlotFacts current))
            {
                aiNearestFactsActiveVersion = 0;
                return;
            }

            aiNearestFactsBySlot[slot] = current;
        }

        private void ObserveAiTeamHpSummaryMutation(LF2Entity entity)
        {
            AiLegacySnapshotMutationCountForDiagnostics++;
            ObserveAiNearestFactsMutation(entity);
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

        // Candidate owns nearest/special state in the SoA rows.  The sole Legacy-era
        // product still shared by CreateAiInputContext is the first-ten move-mode
        // snapshot, so do not touch facts, team summaries, phase-one lists, or trees.
        private void ObserveAiCandidateCharacterInputMutation(LF2Entity entity)
        {
            ObserveAiMoveModeFirst10Mutation(entity);
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
            aiInputGroundTeamPartitions.Clear();
            aiInputGroundTeamPartitionOverflow = false;
            aiInputGroundTeamPartitionsValid = false;
        }

        private AiGroundTeamPartition GetAiGroundTeamPartition(int team)
        {
            if (!aiInputGroundTeamPartitions.TryGetValue(team, out AiGroundTeamPartition partition))
            {
                int partitionIndex = aiInputActiveGroundTeamPartitions.Count;
                if (partitionIndex >= aiInputGroundTeamPartitionPool.Length)
                {
                    aiInputGroundTeamPartitionOverflow = true;
                    return null;
                }

                partition = aiInputGroundTeamPartitionPool[partitionIndex];
                partition.ResetForTeam(team);
                aiInputGroundTeamPartitions.Add(team, partition);
                aiInputActiveGroundTeamPartitions.Add(partition);
            }
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
            if (aiInputGroundTeamPartitionOverflow)
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
            AiLegacyQuadtreeSyncCountForDiagnostics++;
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
                    AiGroundTeamPartition partition =
                        GetAiGroundTeamPartition(Team(entity));
                    partition?.Entries.Add(entry);
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
            if (aiDecisionExecutionMode == AiDecisionExecutionMode.IndexedCanonical)
            {
                if (!TryPrepareAiDecisionIndexedCanonical(self, tickIndex))
                    PrepareAiInputBasicLegacyCore(self, tickIndex);
                return;
            }

            if (aiDecisionShadowMode == AiDecisionShadowMode.Disabled)
            {
                PrepareAiInputBasicLegacyCore(self, tickIndex);
                return;
            }

            bool aiDecisionShadowComparisonStarted =
                BeginAiDecisionShadowComparison(self, tickIndex);
            try
            {
                PrepareAiInputBasicLegacyCore(self, tickIndex);
            }
            finally
            {
                CompleteAiDecisionShadowComparison(
                    aiDecisionShadowComparisonStarted);
            }
        }

        private void PrepareAiInputBasicLegacyCore(LF2Entity self, int tickIndex)
        {
            aiDecisionLegacyCharacterDecisionPosition = 0;
            aiSoADecisionRemainderUseRowsForCurrentInput = false;
            aiSoADecisionRemainderAttemptedForCurrentInput = false;
            aiSoADecisionRemainderRandomBoundaryPassed = false;
            aiSoADecisionRemainderHardFailureRecordedForCurrentInput = false;
            aiSoADecisionRowContext = default;
            bool compareSoAShadow = aiSensingMode == AiSensingMode.SoAShadowAiSensing;
            bool useSoACandidate = aiSensingMode == AiSensingMode.SoAAiSensing;
            if (compareSoAShadow)
                BeginAiSoASensingShadowComparison(self, tickIndex);
            else if (useSoACandidate)
                EnsureAiSensingModeAvailableBeforeTick();

            // Alignment contract R3-AI-LIFE-001: the C++ caller reaches AI input
            // before death/respawn cleanup, including an active zero-HP character DAT.
            if (self?.Runtime == null)
                return;

            NTSDEntityRuntime input = self.Runtime;
            BattleAiInputDetailDiagnostics decisionDiagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (input.Unk3FC > -1000)
            {
                RollAndClearAiKeys(input);
                MoveTowardCoordinate(self, CreateCoordinateAiInputContext());
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: false,
                    postSpecialRngCallsBefore: 0);
                return;
            }

            ulong contextRngCallsBefore = decisionDiagnostics != null
                ? Rng?.CallCount ?? 0
                : 0;
            decisionDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.ContextMoveMode);
            decisionDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.ContextMoveMode);
            AiInputContext ai = CreateAiInputContext(self, tickIndex);
            decisionDiagnostics?.EndPhase(
                BattleAiInputDetailPhase.ContextMoveMode);
            if (decisionDiagnostics != null)
            {
                decisionDiagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    ResolveAiInputDetailRngCallDelta(
                        contextRngCallsBefore,
                        Rng?.CallCount ?? 0));
            }

            int selectedSlot;
            int bestDist;
            bool sameZLane;
            bool candidateUsesLegacyForThisInput =
                useSoACandidate && aiSoACandidatePassLatchedToLegacy;
            if (useSoACandidate && !candidateUsesLegacyForThisInput)
            {
                if (TryRunAiSoACandidateNearest(
                        self,
                        ai.InputPhase,
                        out AiSoANearestResult nearest))
                {
                    selectedSlot = nearest.SelectedSlot;
                    bestDist = nearest.BestDist;
                    sameZLane = nearest.SameZLane;
                }
                else
                {
                    LatchAiSoACandidateToLegacyBeforeRandom();
                    candidateUsesLegacyForThisInput = true;
                    AiSoACandidateLegacyNearestScanCountForDiagnostics++;
                    selectedSlot = FindNearestAiTargetSlotBrute(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
            }
            else
            {
                if (useSoACandidate)
                {
                    AiSoACandidateLegacyNearestScanCountForDiagnostics++;
                    selectedSlot = FindNearestAiTargetSlotBrute(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
                else
                {
                    selectedSlot = FindNearestAiTargetSlot(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
            }
            if (compareSoAShadow)
                CompareAiSoASensingInitial(self, tickIndex, selectedSlot, bestDist, sameZLane);
            decisionDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.CachedTargetRetention);
            decisionDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.CachedTargetRetention);
            int savedTargetSlot = input.Unk360;
            LF2Entity cached = AiAt(savedTargetSlot);
            decisionDiagnostics?.RecordPhaseSlotVisits(
                BattleAiInputDetailPhase.CachedTargetRetention,
                1);
            bool decisionRemainderRequested =
                aiSoADecisionRemainderEnabledForSelfCheck &&
                aiSensingMode == AiSensingMode.SoAAiSensing;
            if (decisionRemainderRequested)
                AiSoADecisionRemainderEligibleAttemptCountForDiagnostics++;
            if (decisionRemainderRequested &&
                !TryBindAiSoADecisionRowContext(
                    self,
                    selectedSlot,
                    savedTargetSlot,
                    cached))
            {
                if (!candidateUsesLegacyForThisInput)
                {
                    candidateUsesLegacyForThisInput = true;
                    AiSoACandidateLegacyNearestScanCountForDiagnostics++;
                    selectedSlot = FindNearestAiTargetSlotBrute(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
            }
            uint cacheRngStateBefore = Rng?.State ?? 0;
            ulong cacheRngCallsBefore = Rng?.CallCount ?? 0;
            bool cachedTargetEligible = IsLivingCharacterDat(cached);
            bool cacheRandomCalled = false;
            int cacheRoll = 0;
            if (cachedTargetEligible)
            {
                cacheRandomCalled = true;
                cacheRoll = Rand(30);
                if (cacheRoll > 0)
                    selectedSlot = savedTargetSlot;
                else
                    input.Unk360 = selectedSlot;
            }
            else
            {
                input.Unk360 = selectedSlot;
            }
            uint cacheRngStateAfter = Rng?.State ?? 0;
            ulong cacheRngCallsAfter = Rng?.CallCount ?? 0;
            if (compareSoAShadow)
            {
                ContinueAiSoASensingShadowComparisonAfterCache(
                    self,
                    tickIndex,
                    cachedTargetEligible,
                    cacheRandomCalled,
                    cacheRoll,
                    cacheRngStateBefore,
                    cacheRngCallsBefore,
                    cacheRngStateAfter,
                    cacheRngCallsAfter,
                    selectedSlot);
            }
            decisionDiagnostics?.EndPhase(
                BattleAiInputDetailPhase.CachedTargetRetention);
            if (decisionDiagnostics != null)
            {
                decisionDiagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.CachedTargetRetention,
                    ResolveAiInputDetailRngCallDelta(
                        cacheRngCallsBefore,
                        cacheRngCallsAfter));
            }

            if (selectedSlot < 0)
            {
                if (compareSoAShadow)
                    CompleteAiSoASensingComparisonWithoutSpecial(self, tickIndex);
                RollAndClearAiKeys(input);
                AiPostNoTargetFallback(self, cached, ai);
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: false,
                    postSpecialRngCallsBefore: 0);
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

            bool runLegacySpecialScan = !useSoACandidate || candidateUsesLegacyForThisInput;
            if (useSoACandidate && !runLegacySpecialScan)
            {
                if (TryRunAiSoACandidateSpecial(
                        self,
                        ai.InputPhase,
                        selectedSlot,
                        bestDist,
                        sameZLane,
                        out AiSoASpecialResult special))
                {
                    selectedSlot = special.SelectedSlot;
                    specialBestDist = special.BestDist;
                    int flags = special.Flags;
                    specialObjectProximity = (flags & AiSoASpecialProximity) != 0;
                    specialLeft = (flags & AiSoASpecialLeft) != 0;
                    specialRight = (flags & AiSoASpecialRight) != 0;
                    specialUp = (flags & AiSoASpecialUp) != 0;
                    specialDown = (flags & AiSoASpecialDown) != 0;
                    specialGuard7A = (flags & AiSoASpecialGuard7A) != 0;
                    specialGuard7B = (flags & AiSoASpecialGuard7B) != 0;
                    specialForce7AGround = (flags & AiSoASpecialForce7AGround) != 0;
                    specialC8ThreatSeen = (flags & AiSoASpecialC8ThreatSeen) != 0;
                    specialPostSelectionSeen =
                        (flags & AiSoASpecialPostSelectionSeen) != 0;
                }
                else
                {
                    LatchAiSoACandidateToLegacyAfterRandom();
                    LatchAiSoADecisionRemainderToLegacy();
                    runLegacySpecialScan =
                        !aiSoADecisionRemainderUseRowsForCurrentInput;
                }
            }

            if (runLegacySpecialScan)
            {
                if (useSoACandidate)
                    AiSoACandidateLegacySpecialScanCountForDiagnostics++;

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

                // Candidate does not build the Legacy compact special list.  Once a
                // Candidate query fails, retain the authoritative slot order through
                // the full 20..capacity scan rather than rebuilding Legacy products.
                bool fullLegacySpecialScan =
                    ForceFullAiSpecialScanForDiagnostics || useSoACandidate;
                int specialScanCount = fullLegacySpecialScan
                    ? aiInputSlots.Length - 20
                    : aiSpecialScanSlots.Count;
                for (int specialScanIndex = 0; specialScanIndex < specialScanCount; specialScanIndex++)
                {
                    int i = fullLegacySpecialScan
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
                            threat = (ObjectId(self) == 2 || ObjectId(self) == 34) && lowHpWindow && Team(obj) == Team(self);
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
            }
            if (compareSoAShadow)
            {
                CompareAiSoASensingPostSpecial(
                    self,
                    tickIndex,
                    selectedSlot,
                    specialBestDist,
                    specialObjectProximity,
                    specialLeft,
                    specialRight,
                    specialUp,
                    specialDown,
                    specialGuard7A,
                    specialGuard7B,
                    specialForce7AGround,
                    specialC8ThreatSeen,
                    specialPostSelectionSeen);
            }
            TrackAiSoADecisionSelectedRow(selectedSlot);
            ulong postSpecialRngCallsBefore = decisionDiagnostics != null
                ? Rng?.CallCount ?? 0
                : 0;
            decisionDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.PostSpecialMainDecision);
            decisionDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.PostSpecialMainDecision);
            input.Unk360 = selectedSlot;
            RollAndClearAiKeys(input);
            LF2Entity target = AiAt(selectedSlot);
            decisionDiagnostics?.RecordPhaseSlotVisits(
                BattleAiInputDetailPhase.PostSpecialMainDecision,
                1);
            if (target == null)
            {
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }
            int selfState = State(self);
            int targetState = State(target);
            int targetOid = ObjectId(target);

            if (X(target) > X(self) && Facing(self) == 1) input.KeyRight = 1;
            if (X(target) < X(self) && Facing(self) == 0) input.KeyLeft = 1;
            if (selfState == 2) { if (Facing(self) == 1) input.KeyRight = 1; else input.KeyLeft = 1; }

            int blockRoll = Rand(ai.Rand5 + 8);
            if (blockRoll == 0 && HasBoundaryBlock(self))
            { input.PrevJump = 0; input.KeyJump = 1; }

            if (AiPreUpdateTarget3000SideEffect(self, target, selfState, targetState, ai))
            {
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (HasInputHistoryGate(self) && LinkState(self) > 0)
            {
                LF2Entity held = AiAt(TargetSlot(self));
                decisionDiagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.PostSpecialMainDecision,
                    1);
                if (held != null && (ObjectId(held) == 0x7A || ObjectId(held) == 0x7B))
                {
                    input.PrevJump = 0;
                    input.KeyJump = 1;
                    ApplyAiInputEdgesWithDiagnostics(
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
            }

            bool coordinateAllowsSpecial = !HasInputHistoryGate(self) || AiPostCacheCoordinateAllowsSpecial(self);
            if (coordinateAllowsSpecial && (targetState == 0x3EC || targetState == 0x7D4))
            {
                if (HasInputHistoryGate(self) && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240) &&
                    targetOid != 0x7A && targetOid != 0x7B)
                {
                    ApplyAiInputEdgesWithDiagnostics(
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
                MoveTowardTarget(self, target, ai, selfState);
                if (Abs(Z(target) - Z(self)) <= 3 && Abs(X(target) - X(self)) <= 6) { input.PrevJump = 0; input.KeyJump = 1; }
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (targetState == 14 || Abs(Y(target)) > 2)
            {
                if (X(target) > ai.StageTargetX - 30)
                {
                    input.KeyLeft = 1;
                    input.PrevLeft = 0;
                    ApplyAiInputEdgesWithDiagnostics(
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
                if (X(target) < 30)
                {
                    input.KeyRight = 1;
                    input.PrevRight = 0;
                    ApplyAiInputEdgesWithDiagnostics(
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
                if (Abs(Z(target) - Z(self)) <= 45 || Abs(X(target) - X(self)) <= 350)
                {
                    if (X(target) > X(self)) { input.KeyLeft = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevLeft = 0; }
                    else { input.KeyRight = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevRight = 0; }
                    if (Z(target) < Z(self) || Z(target) < StageZMin + 10) input.KeyDown = 1; else input.KeyUp = 1;
                }
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            bool c8Allowed = (HasInputHistoryGate(self) && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240)) ||
                             (targetState != 14 && Abs(Y(target)) <= 2);
            if (c8Allowed && targetOid == 0xC8)
            {
                if (X(target) > X(self) + 7) input.KeyRight = 1; else if (X(target) < X(self) - 7) input.KeyLeft = 1;
                if (Z(target) > Z(self) + 2) input.KeyDown = 1; else if (Z(target) < Z(self) - 2) input.KeyUp = 1;
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (Rand(ai.Rand5 + 1) == 0)
            {
                int characterDecisionPosition = 0;
                if (AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity))
                {
                    characterDecisionPosition = 1;
                }
                else if (AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane))
                {
                    characterDecisionPosition = 2;
                }
                else if (AiUpdateOid1ComboDecision(self, target, targetState))
                {
                    characterDecisionPosition = 3;
                }
                else if (AiUpdateCloseOid1Decision(self, target))
                {
                    characterDecisionPosition = 4;
                }
                else if (AiUpdateOid4ComboDecision(self, target))
                {
                    characterDecisionPosition = 5;
                }
                else if (AiUpdateOid5ComboDecision(self, target))
                {
                    characterDecisionPosition = 6;
                }
                else if (RunLegacyAiCharacterDecisionModule(
                             self,
                             target,
                             targetState,
                             bestDist,
                             sameZLane,
                             ai,
                             out int matchedPosition))
                {
                    characterDecisionPosition = matchedPosition;
                }

                if (characterDecisionPosition != 0)
                {
                    aiDecisionLegacyCharacterDecisionPosition = characterDecisionPosition;
                    ApplyAiInputEdgesWithDiagnostics(
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
            }

            bool closeOrFree = !HasInputHistoryGate(self) || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
            int selfOid = ObjectId(self);
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

            if (LinkState(self) > 0 && !AiProcessHelper(self, target, ai, selfState, targetState, sameZLane, specialObjectProximity))
            {
                ApplyAiInputEdgesWithDiagnostics(
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (Rand(ai.Difficulty * 7 + 10) == 0 && (targetState == 3 || targetState / 100 == 3) &&
                Abs(Z(target) - Z(self)) < 9 && ((Facing(target) == 0 && X(target) < X(self)) || (Facing(target) == 1 && X(target) > X(self))))
                input.KeyAttack = 1;
            if (closeOrFree && Rand(2 * (ai.Rand5 + 10)) < 3 && Rand(20) < 3 && targetState != 14) input.KeyDefend = 1;
            bool selfGroup = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if ((!selfGroup || targetState == 16) && Abs(X(target) - 2 * (int)Vx(self) - X(self)) < 50 &&
                Abs(Z(target) - Z(self)) < 5 && Rand(ai.Rand3 + 3) == 0 && targetState != 14) input.KeyJump = 1;

            AiProcessSubCallerPrewrite(self, target, ai, selfState, targetState);
            AiProcessSubLabel435PressurePrewrite(self, target, ai, selfState, targetState);
            AiProcessSubHelper(self, target, ai, targetState, specialLeft, specialRight);
            ApplyAiInputEdgesWithDiagnostics(
                input,
                decisionDiagnostics,
                completePostSpecialMainDecision: true,
                postSpecialRngCallsBefore);
        }

        private bool RunLegacyAiCharacterDecisionModule(
            LF2Entity self,
            LF2Entity target,
            int targetState,
            int nearestTargetDistance,
            bool sameZLane,
            in AiInputContext ai,
            out int matchedPosition)
        {
            matchedPosition = 0;
            if (self?.Runtime == null || target?.Runtime == null)
                return false;

            AiDecisionSnapshot snapshot = null;
            AiDecisionAvailability availability = AiDecisionAvailability.SnapshotMissing;
            if (aiDecisionSharedPassAvailable && aiDecisionSharedSnapshot != null)
            {
                availability = CaptureAiDecisionSharedOwnedSnapshot(
                    self,
                    aiDecisionSharedSnapshot);
                if (availability == AiDecisionAvailability.Available)
                    snapshot = aiDecisionSharedSnapshot;
            }

            if (snapshot == null)
            {
                snapshot = aiCharacterDecisionLegacyFallbackSnapshot;
                if (snapshot == null || snapshot.Rows.Capacity != RuntimeSlotCapacity)
                {
                    throw new InvalidOperationException(
                        "Legacy AI character-decision fallback snapshot is not prepared for the current runtime capacity.");
                }

                availability = CaptureAiDecisionShadowSnapshot(self, snapshot);
            }

            if (availability != AiDecisionAvailability.Available)
            {
                throw new InvalidOperationException(
                    $"Legacy AI character-decision snapshot capture failed after the decision RNG boundary: {availability}.");
            }

            AiSensingSnapshot rows = snapshot.Rows;
            int selfSlot = self.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if ((uint)selfSlot >= (uint)rows.Capacity ||
                (uint)targetSlot >= (uint)rows.Capacity ||
                !rows.Included[selfSlot] ||
                !rows.Included[targetSlot])
            {
                throw new InvalidOperationException(
                    "Legacy AI character-decision snapshot does not contain the current self/target pair.");
            }

            AiDecisionInputState decisionInput = snapshot.Input;
            bool captureTrace = aiDecisionLegacyRngRecording;
            var random = new AiDecisionRandomStream(
                Rng.State,
                Rng.CallCount,
                captureTrace,
                captureTrace ? aiCharacterDecisionLegacyRngModuli : null,
                captureTrace ? aiCharacterDecisionLegacyRngRaw : null,
                captureTrace ? aiCharacterDecisionLegacyRngValues : null);
            var context = new AiCharacterDecisionContext(
                ai.MoveMode,
                ai.StageTargetX);

            bool matched = snapshot.CharacterDecisionModule.TryEvaluatePositions7Through39(
                rows,
                selfSlot,
                targetSlot,
                targetState,
                nearestTargetDistance,
                sameZLane,
                in context,
                ref decisionInput,
                ref random,
                out matchedPosition,
                out _);

            Rng.RestoreState(random.State, random.Calls);
            if (random.DrawCount > 0 && aiSoADecisionRemainderUseRowsForCurrentInput)
                aiSoADecisionRemainderRandomBoundaryPassed = true;

            if (captureTrace)
            {
                int capturedDrawCount = Math.Min(
                    random.DrawCount,
                    aiCharacterDecisionLegacyRngModuli.Length);
                for (int index = 0; index < capturedDrawCount; index++)
                {
                    RecordAiDecisionShadowLegacyRng(
                        aiCharacterDecisionLegacyRngModuli[index],
                        aiCharacterDecisionLegacyRngRaw[index],
                        aiCharacterDecisionLegacyRngValues[index]);
                }
                if (random.TraceOverflow || random.DrawCount > capturedDrawCount)
                    aiDecisionLegacyRngOverflow = true;
            }

            battleCharacterInputWriter.CommitAiDecisionState(
                self.Runtime,
                decisionInput);
            return matched;
        }

        private void ApplyAiInputEdgesWithDiagnostics(
            NTSDEntityRuntime input,
            BattleAiInputDetailDiagnostics diagnostics,
            bool completePostSpecialMainDecision,
            ulong postSpecialRngCallsBefore)
        {
            diagnostics?.RecordPhaseCall(BattleAiInputDetailPhase.InputEdges);
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.InputEdges);
            battleCharacterInputWriter.ApplyInputEdges(input);
            CompleteAiSoADecisionRemainderInput();
            diagnostics?.EndPhase(BattleAiInputDetailPhase.InputEdges);
            if (!completePostSpecialMainDecision || diagnostics == null)
                return;

            diagnostics.RecordPhaseRngCalls(
                BattleAiInputDetailPhase.PostSpecialMainDecision,
                ResolveAiInputDetailRngCallDelta(
                    postSpecialRngCallsBefore,
                    Rng?.CallCount ?? 0));
            diagnostics.EndPhase(
                BattleAiInputDetailPhase.PostSpecialMainDecision);
        }

        private static ulong ResolveAiInputDetailRngCallDelta(
            ulong before,
            ulong after)
        {
            return after >= before ? after - before : 0;
        }

        private AiInputContext CreateAiInputContext(LF2Entity self, int tickIndex)
        {
            int inputPhase = InputPhase;
            int difficulty = Difficulty;
            bool forceZero = AiPhaseGate == 1;
            if (!forceZero && inputPhase == 1 && Team(self) != 5)
                forceZero = Slot(self) < 20 || ObjectId(self) < 30;
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

        private bool TryBindAiSoADecisionRowContext(
            LF2Entity self,
            int selectedSlot,
            int cachedSlot,
            LF2Entity cached)
        {
            aiSoADecisionRemainderAttemptedForCurrentInput = true;
            AiSoASensingRows rows = aiSoASensingRows;
            var context = new AiDecisionRowContext
            {
                Rows = rows,
                Slots = aiInputSlots,
                OccupancyEpoch = aiSoASensingSnapshotEpoch,
            };
            bool captured = TryCaptureAiSoADecisionRowIdentity(
                                rows,
                                self?.Runtime?.SlotIndex ?? -1,
                                self,
                                out context.Self) &&
                            TryCaptureAiSoADecisionRowIdentity(
                                rows,
                                selectedSlot,
                                AiAt(selectedSlot),
                                out context.Selected) &&
                            TryCaptureAiSoADecisionRowIdentity(
                                rows,
                                cachedSlot,
                                cached,
                                out context.Cached);
            if (!captured ||
                !ValidateAiSoADecisionRowContext(ref context, terminalGuard: false))
            {
                LatchAiSoADecisionRemainderToLegacy();
                return false;
            }

            context.Bound = true;
            aiSoADecisionRowContext = context;
            aiSoADecisionRemainderUseRowsForCurrentInput = true;
            AiSoADecisionRemainderContextBindCountForDiagnostics++;
            return true;
        }

        private static bool TryCaptureAiSoADecisionRowIdentity(
            AiSoASensingRows rows,
            int slot,
            LF2Entity entity,
            out AiDecisionRowIdentity identity)
        {
            identity = new AiDecisionRowIdentity
            {
                Entity = entity,
                Slot = slot,
            };
            if (rows == null)
                return false;
            if (slot < 0 || slot >= rows.Capacity)
                return entity == null;

            identity.Included = rows.Included[slot];
            identity.Generation = rows.Generation[slot];
            identity.StableId = rows.Identity[slot];
            return true;
        }

        private bool ValidateAiSoADecisionRowContext(
            ref AiDecisionRowContext context,
            bool terminalGuard)
        {
            AiSoADecisionRemainderGatewayValidationCountForDiagnostics++;
            bool mutationActive =
                aiSoADecisionRemainderMutationKindForSelfCheck != 0 &&
                aiSoADecisionRemainderMutationAfterRandomForSelfCheck == terminalGuard &&
                (!terminalGuard || aiSoADecisionRemainderRandomBoundaryPassed);
            if ((terminalGuard &&
                 aiSoADecisionRemainderRandomBoundaryPassed &&
                 aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck) ||
                (aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck &&
                 !terminalGuard) ||
                (mutationActive &&
                 aiSoADecisionRemainderMutationKindForSelfCheck == 1) ||
                !aiSoADecisionRemainderEnabledForSelfCheck ||
                aiSensingMode != AiSensingMode.SoAAiSensing ||
                aiSoACandidatePassLatchedToLegacy ||
                !aiSoASensingSnapshotValid ||
                aiSoASensingPassInvalidated ||
                context.Rows == null ||
                context.Slots == null ||
                !ReferenceEquals(context.Rows, aiSoASensingRows) ||
                !ReferenceEquals(context.Slots, aiInputSlots) ||
                context.Rows.Capacity != context.Slots.Length ||
                RuntimeSlotCapacity != context.Rows.Capacity ||
                context.OccupancyEpoch != aiSoASensingSnapshotEpoch ||
                aiInputSlotSnapshotOccupancyEpoch != context.OccupancyEpoch ||
                RuntimeSlotOccupancyEpochForServices != context.OccupancyEpoch)
            {
                return false;
            }

            return ValidateAiSoADecisionRowIdentity(
                       ref context,
                       context.Self,
                       mutationActive &&
                       aiSoADecisionRemainderMutationKindForSelfCheck == 2) &&
                   ValidateAiSoADecisionRowIdentity(
                       ref context,
                       context.Selected,
                       mutationActive &&
                       aiSoADecisionRemainderMutationKindForSelfCheck == 3) &&
                   ValidateAiSoADecisionRowIdentity(
                       ref context,
                       context.Cached,
                       mutationActive &&
                       aiSoADecisionRemainderMutationKindForSelfCheck == 4);
        }

        private bool ValidateAiSoADecisionRowIdentity(
            ref AiDecisionRowContext context,
            AiDecisionRowIdentity identity,
            bool forceMismatch)
        {
            AiSoADecisionRemainderRowVisitCountForDiagnostics++;
            if (forceMismatch)
                return false;
            if (identity.Slot < 0 || identity.Slot >= context.Rows.Capacity)
                return identity.Entity == null && !identity.Included;
            if (!identity.Included)
            {
                return identity.Entity == null &&
                       context.Slots[identity.Slot] == null &&
                       TryGetRuntimeSlotReadOnlyView(
                           identity.Slot,
                           out RuntimeSlotTable.ReadOnlySlotView emptyView) &&
                       emptyView.RuntimeSlot == identity.Slot &&
                       !emptyView.Claimed &&
                       emptyView.Entity == null;
            }

            return identity.Entity?.Runtime != null &&
                   ReferenceEquals(context.Slots[identity.Slot], identity.Entity) &&
                   identity.Entity.Runtime.SlotIndex == identity.Slot &&
                   identity.Entity.Runtime.StableId == identity.StableId &&
                   TryGetCurrentRuntimeHandle(
                       identity.Slot,
                       identity.Entity,
                       out RuntimeEntityHandle handle) &&
                   handle.Generation == identity.Generation;
        }

        private void TrackAiSoADecisionSelectedRow(int selectedSlot)
        {
            if (!aiSoADecisionRemainderUseRowsForCurrentInput)
                return;

            TryCaptureAiSoADecisionRowIdentity(
                aiSoADecisionRowContext.Rows,
                selectedSlot,
                AiAt(selectedSlot),
                out aiSoADecisionRowContext.Selected);
        }

        private bool TryGetAiSoADecisionRemainderRow(
            LF2Entity entity,
            out AiSoASensingRows rows,
            out int slot)
        {
            // The slot snapshot and SoA rows are published at one occupancy epoch.
            // Registry mutations are deferred for this pass, and the lease is guarded
            // at bind/end, so property reads must not repeat global row validation.
            rows = null;
            slot = -1;
            if (!aiSoADecisionRemainderUseRowsForCurrentInput ||
                !aiSoADecisionRowContext.Bound)
            {
                return false;
            }

            rows = aiSoADecisionRowContext.Rows;
            if (entity?.Runtime == null || rows == null)
                return false;

            slot = entity.Runtime.SlotIndex;
            return slot >= 0 &&
                   slot < rows.Capacity &&
                   rows.Included[slot];
        }

        private void LatchAiSoADecisionRemainderToLegacy()
        {
            if (!aiSoADecisionRemainderAttemptedForCurrentInput)
                return;
            if (AiUnifiedSnapshotExecutionFallbackForbidden)
            {
                ThrowAiUnifiedSnapshotExecutionHardBreach(
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare,
                    "AI decision remainder attempted fallback after unified snapshot commit.");
            }

            if (aiSoADecisionRemainderRandomBoundaryPassed)
            {
                RecordAiSoADecisionRemainderHardFailure();
                return;
            }

            aiSoADecisionRemainderAttemptedForCurrentInput = false;
            aiSoADecisionRemainderUseRowsForCurrentInput = false;
            aiSoADecisionRowContext = default;
            AiSoADecisionRemainderFallbackCountForDiagnostics++;
            AiSoADecisionRemainderPreRandomFailureCountForDiagnostics++;
        }

        private void RecordAiSoADecisionRemainderHardFailure()
        {
            if (aiSoADecisionRemainderHardFailureRecordedForCurrentInput)
                return;

            aiSoADecisionRemainderHardFailureRecordedForCurrentInput = true;
            AiSoADecisionRemainderPostRandomFailureCountForDiagnostics++;
            AiSoADecisionRemainderHardFailureCountForDiagnostics++;
            if (AiUnifiedSnapshotExecutionFallbackForbidden)
            {
                ThrowAiUnifiedSnapshotExecutionHardBreach(
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare,
                    "AI decision remainder failed after the random boundary under unified snapshot authority.");
            }
        }

        private void CompleteAiSoADecisionRemainderInput()
        {
            if (aiSoADecisionRemainderAttemptedForCurrentInput &&
                aiSoADecisionRemainderUseRowsForCurrentInput)
            {
                if (!ValidateAiSoADecisionRowContext(
                        ref aiSoADecisionRowContext,
                        terminalGuard: true))
                {
                    if (aiSoADecisionRemainderRandomBoundaryPassed)
                    {
                        RecordAiSoADecisionRemainderHardFailure();
                    }
                    else
                    {
                        AiSoADecisionRemainderFallbackCountForDiagnostics++;
                        AiSoADecisionRemainderPreRandomFailureCountForDiagnostics++;
                    }
                }
                else if (!aiSoADecisionRemainderHardFailureRecordedForCurrentInput)
                {
                    AiSoADecisionRemainderAppliedCountForDiagnostics++;
                }
            }

            aiSoADecisionRemainderAttemptedForCurrentInput = false;
            aiSoADecisionRemainderUseRowsForCurrentInput = false;
            aiSoADecisionRemainderRandomBoundaryPassed = false;
            aiSoADecisionRemainderHardFailureRecordedForCurrentInput = false;
            aiSoADecisionRowContext = default;
        }

        private int StageZMin => Runtime?.Stage?.ZMin ?? 180;
        private int StageZMax => Runtime?.Stage?.ZMax ?? 350;
        private int Rand(int modulus)
        {
            int random = Rng.NextRaw();
            int value = random % Math.Max(1, modulus);
            if (aiDecisionLegacyRngRecording)
                RecordAiDecisionShadowLegacyRng(modulus, random, value);
            if (aiSoADecisionRemainderUseRowsForCurrentInput &&
                !aiSoADecisionRemainderRandomBoundaryPassed)
            {
                aiSoADecisionRemainderRandomBoundaryPassed = true;
            }
            return value;
        }
        private LF2Entity AiAt(int slot) => slot >= 0 && slot < aiInputSlots.Length ? aiInputSlots[slot] : null;
        private int X(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.X[slot] : e.Runtime.XInt;
        private int Y(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Y[slot] : e.Runtime.YInt;
        private int Z(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Z[slot] : e.Runtime.ZInt;
        private int Hp(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Hp[slot] : e.Runtime.HP;
        private int Hp3(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Hp3[slot] : e.Runtime.HP3;
        private int HpMax(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.HpMax[slot] : e.Runtime.HPBound;
        private int Pp(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Pp[slot] : e.Runtime.PP;
        private int Team(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Team[slot] : e.Runtime.RelationTeam;
        private int Slot(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out _, out int slot) ? slot : e.Runtime.SlotIndex;
        private int Frame(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Frame[slot] : e.Runtime.Frame;
        private int HitJ(LF2Entity e) =>
            TryGetAiSoADecisionRemainderRow(
                e,
                out AiSoASensingRows rows,
                out int slot)
                ? rows.HitJ[slot]
                : CaptureAiCurrentFrameHitJ(e, e?.Runtime?.Frame ?? 0);
        private int State(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.State[slot] : e.GetState();
        private int Facing(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Facing[slot] : (e.Runtime.Dir == "left" ? 1 : 0);
        private int ObjectId(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.ObjectId[slot] : e.ObjectId;
        private int LinkState(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.LinkState[slot] : e.Runtime.LinkState;
        private int TargetSlot(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.TargetSlot[slot] : e.Runtime.TargetSlotIndex;
        private int HitStop(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.HitStop[slot] : e.Runtime.HitStop;
        private double Vx(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.Vx[slot] : e.Runtime.Vx;
        private bool HasInputHistoryGate(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.InputHistoryGate[slot] : e.Runtime.HasInputHistoryGate();
        private bool HasBoundaryBlock(LF2Entity e) => TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.BoundaryFlags[slot] != 0 : e.Runtime.ZBoundNegative || e.Runtime.ZBoundPositive || e.Runtime.XBoundNegative || e.Runtime.XBoundPositive;
        private static int Abs(int value) => Math.Abs(value);
        private int Distance(LF2Entity a, LF2Entity b) => Abs(X(b) - X(a)) + Abs(Z(b) - Z(a));
        private bool IsCharacterDat(LF2Entity e) => e != null && (TryGetAiSoADecisionRemainderRow(e, out AiSoASensingRows rows, out int slot) ? rows.DataObjectType[slot] : e.GetCurrentDataObjectTypeForSimulation()) == 0;
        private bool IsLivingCharacterDat(LF2Entity e) => IsCharacterDat(e) && Hp(e) > 0;

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

        private AiNearestPointFilter CreateAiNearestPointFilter(
            LF2Entity self,
            int inputPhase,
            bool air)
        {
            NTSDEntityRuntime runtime = self?.Runtime;
            var filter = new AiNearestPointFilter
            {
                World = this,
                SelfEntity = self,
                SelfSlot = runtime?.SlotIndex ?? -1,
                SelfX = runtime?.XInt ?? 0,
                SelfTeam = runtime?.RelationTeam ?? 0,
                InputPhase = inputPhase,
                Air = air,
                UseSnapshotFacts = !ForceLegacyAiNearestFilterForDiagnostics,
            };
            if (filter.UseSnapshotFacts &&
                TryCreateAiNearestSnapshotStamp(out AiNearestSnapshotStamp stamp))
            {
                filter.Stamp = stamp;
            }
            return filter;
        }

        private bool TryCreateAiNearestSnapshotStamp(
            out AiNearestSnapshotStamp stamp)
        {
            stamp = default;
            LF2Entity[] slots = aiInputSlots;
            uint[] generations = aiInputGroundGenerationBySlot;
            AiNearestSlotFacts[] facts = aiNearestFactsBySlot;
            ulong epoch = aiInputSlotSnapshotOccupancyEpoch;
            uint version = aiNearestFactsActiveVersion;
            int count = slots?.Length ?? 0;
            if (slots == null ||
                epoch == 0 ||
                version == 0 ||
                generations == null ||
                facts == null ||
                generations.Length != count ||
                facts.Length != count ||
                RuntimeSlotOccupancyEpochForServices != epoch)
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

        private bool IsAiNearestSnapshotStampCurrent(
            in AiNearestSnapshotStamp stamp)
        {
            return stamp.OccupancyEpoch != 0 &&
                   stamp.FactsVersion != 0 &&
                   ReferenceEquals(stamp.Slots, aiInputSlots) &&
                   ReferenceEquals(
                       stamp.GenerationBySlot,
                       aiInputGroundGenerationBySlot) &&
                   ReferenceEquals(stamp.FactsBySlot, aiNearestFactsBySlot) &&
                   stamp.SlotCount == aiInputSlots.Length &&
                   stamp.SlotCount == aiInputGroundGenerationBySlot.Length &&
                   stamp.SlotCount == aiNearestFactsBySlot.Length &&
                   aiInputSlotSnapshotOccupancyEpoch == stamp.OccupancyEpoch &&
                   RuntimeSlotOccupancyEpochForServices == stamp.OccupancyEpoch &&
                   aiNearestFactsActiveVersion == stamp.FactsVersion;
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
                AiNearestPointFilter filter = CreateAiNearestPointFilter(
                    self,
                    ai.InputPhase,
                    false);
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
                AiNearestPointFilter filter = CreateAiNearestPointFilter(
                    self,
                    ai.InputPhase,
                    true);
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

        private bool IsGroundAiTargetCandidate(LF2Entity self, LF2Entity candidate, int inputPhase)
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

        private static bool IsGroundAiTargetCandidate(
            LF2Entity self,
            int selfSlot,
            int selfX,
            int selfTeam,
            in AiNearestSlotFacts candidate,
            int inputPhase)
        {
            if (!candidate.Included ||
                !candidate.Active ||
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

            return TeamCandidateAllowed(
                       selfTeam,
                       candidate.Team,
                       inputPhase) &&
                   candidate.Hp > 0 &&
                   candidate.State != 14 &&
                   Abs(candidate.Y) <= 2;
        }

        private bool IsAirAiTargetCandidate(LF2Entity self, LF2Entity candidate, int inputPhase)
        {
            return candidate != null &&
                   candidate != self &&
                   TeamCandidateAllowed(self, candidate, inputPhase) &&
                   Hp(candidate) > 0 &&
                   (State(candidate) == 14 || Abs(Y(candidate)) > 2);
        }

        private static bool IsAirAiTargetCandidate(
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
                   TeamCandidateAllowed(
                       selfTeam,
                       candidate.Team,
                       inputPhase) &&
                   candidate.Hp > 0 &&
                   candidate.AirRole;
        }

        private bool IsAirAiSpatialRole(LF2Entity candidate)
        {
            return candidate != null &&
                   (State(candidate) == 14 || Abs(Y(candidate)) > 2);
        }

        private bool IsGroundAiSpatialRole(LF2Entity candidate)
        {
            if (candidate == null || State(candidate) == 14 || Abs(Y(candidate)) > 2)
                return false;

            return IsCharacterDat(candidate) || State(candidate) == 3000;
        }

        private SpatialAabbXZ AroundAiPoint(LF2Entity entity, int radiusX, int radiusZ)
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

        private bool TeamCandidateAllowed(LF2Entity self, LF2Entity candidate, int inputPhase)
        {
            return TeamCandidateAllowed(
                Team(self),
                Team(candidate),
                inputPhase);
        }

        private static bool TeamCandidateAllowed(
            int selfTeam,
            int candidateTeam,
            int inputPhase)
        {
            if (candidateTeam != selfTeam)
            {
                if (inputPhase != 1) return true;
                if (selfTeam == 5) return true;
            }
            if (candidateTeam != 5) return false;
            if (inputPhase != 1) return false;
            return candidateTeam != selfTeam;
        }

        private void AiUpdateMoveModeScan(LF2Entity self, ref AiInputContext ai)
        {
            if (ai.InputPhase != 1 || Team(self) == 5)
                return;

            if (aiSoADecisionRemainderUseRowsForCurrentInput)
            {
                AiUpdateMoveModeScanRows(self, ref ai);
                return;
            }

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

        private void AiUpdateMoveModeScanRows(
            LF2Entity self,
            ref AiInputContext ai)
        {
            AiSoASensingRows rows = aiSoASensingRows;
            int selfSlot = Slot(self);
            int rightmostX = -1;
            int rightmostZ = 0;
            int count = Math.Min(10, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics?
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
                ApplyAiMoveModeFromRightmost(
                    self,
                    rightmostX,
                    rightmostZ,
                    ref ai);
            }
        }

        private bool IsAiMoveModeSnapshotSelfCurrent(LF2Entity self)
        {
            if (self?.Runtime == null)
                return false;

            ActiveBattleAiInputDetailDiagnosticsForDiagnostics?
                .RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    1);
            int slot = Slot(self);
            if (aiSensingMode == AiSensingMode.SoAAiSensing)
            {
                return slot >= 0 &&
                       slot < aiInputSlots.Length &&
                       aiSoASensingRows != null &&
                       slot < aiSoASensingRows.Capacity &&
                       ReferenceEquals(aiInputSlots[slot], self) &&
                       aiSoASensingRows.Included[slot] &&
                       self.Runtime.StableId == aiSoASensingRows.Identity[slot] &&
                       TryGetCurrentRuntimeHandle(
                           slot,
                           self,
                           out RuntimeEntityHandle candidateHandle) &&
                       candidateHandle.Generation == aiSoASensingRows.Generation[slot];
            }

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
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            for (int i = 0; i < 10; i++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    1);
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

        private void ApplyAiMoveModeFromRightmost(
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
                bool close = !HasInputHistoryGate(self) || (Abs(Z(self) - Z(savedTarget)) <= 150 && Abs(X(self) - X(savedTarget)) <= 240);
                if (close && ai.MoveMode == 1) self.Runtime.KeyLeft = 1;
            }
            if ((ObjectId(self) == 7 && Frame(self) >= 255 && Frame(self) <= 261) ||
                (ObjectId(self) == 9 && Frame(self) >= 280 && Frame(self) <= 290) ||
                (ObjectId(self) == 32 && Frame(self) >= 240 && Frame(self) <= 245))
                self.Runtime.KeyAttack = 1;
        }

        private void RollAndClearAiKeys(NTSDEntityRuntime input)
        {
            battleCharacterInputWriter.RollAndClearCurrentKeys(input);
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

        private bool AiPostCacheCoordinateAllowsSpecial(LF2Entity self)
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
                ((X(target) > X(self) && X(target) < X(self) + 200 && Vx(target) < 0.0) ||
                 (X(target) < X(self) && X(target) > X(self) - 200 && Vx(target) > 0.0)))
            { self.Runtime.PrevAttack = 0; self.Runtime.KeyAttack = 1; }
            if (X(target) > X(self) && Facing(self) == 1) self.Runtime.KeyRight = 1;
            if (X(target) < X(self) && Facing(self) == 0) self.Runtime.KeyLeft = 1;
            return true;
        }

        private bool AiUpdateFirstDecision(LF2Entity self, LF2Entity target, int nearestTargetDist, bool specialObjectProximity)
        {
            int oid = ObjectId(self);
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (Rand(10) == 0 && Pp(self) > 85 &&
                ((Hp(self) < HpMax(self) - 70 && Hp(self) < 450) || (Hp(self) < (3 * HpMax(self)) / 5 && Hp(self) >= 140)))
            { self.Runtime.ComboDdj = 3; return true; }
            if (nearestTargetDist < 10000 && Rand(30) == 0 && Pp(self) > 250) { self.Runtime.ComboDua = 3; return true; }
            int targetOid = ObjectId(target);
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
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (diagnostics == null)
            {
                return AiUpdateTeammateGuardDecisionCore(
                    self,
                    ai,
                    nearestTargetDist,
                    sameZLane,
                    null);
            }

            diagnostics.RecordPhaseCall(
                BattleAiInputDetailPhase.Teammate20Scan);
            ulong rngCallsBefore = Rng?.CallCount ?? 0;
            diagnostics.BeginPhase(BattleAiInputDetailPhase.Teammate20Scan);
            try
            {
                return AiUpdateTeammateGuardDecisionCore(
                    self,
                    ai,
                    nearestTargetDist,
                    sameZLane,
                    diagnostics);
            }
            finally
            {
                diagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.Teammate20Scan,
                    ResolveAiInputDetailRngCallDelta(
                        rngCallsBefore,
                        Rng?.CallCount ?? 0));
                diagnostics.EndPhase(BattleAiInputDetailPhase.Teammate20Scan);
            }
        }

        private bool AiUpdateTeammateGuardDecisionCore(
            LF2Entity self,
            AiInputContext ai,
            int nearestTargetDist,
            bool sameZLane,
            BattleAiInputDetailDiagnostics diagnostics)
        {
            int oid = ObjectId(self);
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (self.Runtime.LinkState != 0 && Frame(self) >= 9) return false;
            bool hpWindow = (Hp(self) >= HpMax(self) - 70 || Hp(self) >= 140) &&
                            (Hp(self) >= (3 * HpMax(self)) / 5 || Hp(self) < 140);
            if (!hpWindow || sameZLane) return false;
            if (aiSoADecisionRemainderUseRowsForCurrentInput)
            {
                return AiUpdateTeammateGuardDecisionRows(
                    self,
                    nearestTargetDist,
                    diagnostics);
            }
            return AiUpdateTeammateGuardDecisionLegacy(
                self,
                nearestTargetDist,
                diagnostics,
                0);
        }

        private bool AiUpdateTeammateGuardDecisionLegacy(
            LF2Entity self,
            int nearestTargetDist,
            BattleAiInputDetailDiagnostics diagnostics,
            int startSlot)
        {
            for (int i = startSlot; i < 20; i++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Teammate20Scan,
                    1);
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

        private bool AiUpdateTeammateGuardDecisionRows(
            LF2Entity self,
            int nearestTargetDist,
            BattleAiInputDetailDiagnostics diagnostics)
        {
            AiSoASensingRows rows = aiSoASensingRows;
            int selfSlot = Slot(self);
            int selfTeam = Team(self);
            int selfX = X(self);
            int selfZ = Z(self);
            int count = Math.Min(20, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Teammate20Scan,
                    1);
                if (!rows.Included[slot] ||
                    slot == selfSlot ||
                    rows.Team[slot] == 0 ||
                    rows.Team[slot] != selfTeam ||
                    Abs(rows.X[slot] - selfX) >= 250 ||
                    Abs(rows.Z[slot] - selfZ) >= 60 ||
                    Pp(self) <= 350)
                {
                    continue;
                }

                int hp = rows.Hp[slot];
                int hpMax = rows.HpMax[slot];
                bool lowHp = (hp < hpMax - 90 && hp < 140) ||
                             (hp < (3 * hpMax) / 5 && hp >= 140);
                int distance = Abs(rows.X[slot] - selfX) +
                               Abs(rows.Z[slot] - selfZ);
                if (!lowHp || hp <= 0 || distance >= nearestTargetDist / 3)
                    continue;

                int deltaX = rows.X[slot] - selfX;
                if (deltaX > 0 && Facing(self) == 1 && Abs(deltaX) >= 5)
                {
                    self.Runtime.KeyRight = 1;
                    self.Runtime.KeyLeft = 0;
                    return true;
                }
                if (deltaX < 0 && Facing(self) != 1 && Abs(deltaX) >= 5)
                {
                    self.Runtime.KeyRight = 0;
                    self.Runtime.KeyLeft = 1;
                    return true;
                }

                self.Runtime.ComboDuj = 3;
                return true;
            }

            return false;
        }

        private bool AiUpdateOid1ComboDecision(LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = ObjectId(self);
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
            int oid = ObjectId(self);
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
            int oid = ObjectId(self);
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
            int oid = ObjectId(self);
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
            int oid = ObjectId(self);
            int predictedTargetX = X(target) + 2 * (int)Vx(target);
            if (Pp(self) < 150) input.ComboDja = 3;
            if (Abs(X(target) - 2 * (int)Vx(self) - X(self)) < 80 && Abs(Z(target) - Z(self)) < 5 &&
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
            bool specialOid = AiSpecialOidForSubGate(ObjectId(self));
            if (LinkState(self) == 0 && targetState == 16 && specialOid &&
                Abs(X(target) - 2 * (int)input.Vx - X(self)) < 350 && Abs(Z(target) - Z(self)) < 5 && Rand(ai.Rand3 + 3) == 0)
            {
                if ((X(target) > X(self) && Facing(self) == 0) || (X(target) <= X(self) && Facing(self) == 1)) input.KeyJump = 1;
            }
            if (LinkState(self) != 0 || targetState == 16 || !specialOid) return;
            bool closeTrigger = X(target) - X(self) < 100 && Abs(Z(target) - Z(self)) < 80 && Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger && selfState != 7)
            {
                if (Abs(X(target) - 2 * (int)input.Vx - X(self)) < 300 && Abs(Z(target) - Z(self)) < 5 &&
                    Rand(ai.Rand3 + 3) == 0 && targetState != 14 &&
                    ((X(target) > X(self) && Facing(self) == 0) || (X(target) <= X(self) && Facing(self) == 1))) input.KeyJump = 1;
            }
            else if (selfState != 7)
            {
                bool closeWindow = !HasInputHistoryGate(self) || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
                ApplyPressureRetreat(self, target, ai, closeWindow);
                if (closeWindow && Rand(17) == 0) input.KeyDefend = 1;
            }
        }

        private void AiProcessSubLabel435PressurePrewrite(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState)
        {
            NTSDEntityRuntime input = self.Runtime;
            bool specialOid = AiSpecialOidForSubGate(ObjectId(self));
            if (targetState != 16 && specialOid && LinkState(self) == 0) return;
            bool pressure = Hp(target) > Hp(self) * 2 || (Hp(self) <= 100 && Hp3(self) > 100);
            if (!pressure || ai.InputPhase != 1 || !IsCharacterDat(target) || Slot(self) < 20 || Team(self) == 5) return;
            bool closeTrigger = X(target) - X(self) < 100 && Abs(Z(target) - Z(self)) < 80 && Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger || selfState == 7) return;
            bool closeWindow = !HasInputHistoryGate(self) || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
            ApplyPressureRetreat(self, target, ai, closeWindow);
            if (closeWindow && Rand(17) == 0) input.KeyDefend = 1;
        }

        private void ApplyPressureRetreat(LF2Entity self, LF2Entity target, AiInputContext ai, bool closeWindow)
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
            int heldSlot = TargetSlot(self);
            if (heldSlot < 0 || heldSlot >= aiInputSlots.Length) return true;
            LF2Entity held = AiAt(heldSlot);
            int heldOid = held != null ? ObjectId(held) : -1;
            bool lineCover = false;
            BattleAiInputDetailDiagnostics heldScanDiagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            heldScanDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.Held20Scan);
            ulong heldScanRngCallsBefore = heldScanDiagnostics != null
                ? Rng?.CallCount ?? 0
                : 0;
            heldScanDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.Held20Scan);
            if (aiSoADecisionRemainderUseRowsForCurrentInput)
            {
                lineCover = HasAiSoADecisionHeldLineCoverRows(
                    self,
                    target,
                    heldScanDiagnostics);
            }
            else
            {
                lineCover = HasAiSoADecisionHeldLineCoverLegacy(
                    self,
                    target,
                    heldScanDiagnostics,
                    0,
                    false);
            }
            heldScanDiagnostics?.EndPhase(
                BattleAiInputDetailPhase.Held20Scan);
            if (heldScanDiagnostics != null)
            {
                heldScanDiagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.Held20Scan,
                    ResolveAiInputDetailRngCallDelta(
                        heldScanRngCallsBefore,
                        Rng?.CallCount ?? 0));
            }
            if (selfState == 2 && Rand(ai.Rand3 + 5) == 0)
            { if (lineCover) input.KeyDefend = 1; else input.KeyJump = 1; }

            int vxTwice = 2 * (int)Vx(self);
            if (heldOid == 100 || heldOid == 101 || heldOid == 120 || heldOid == 121 || heldOid == 124)
            {
                if (Abs(X(target) - vxTwice - X(self)) < 10000 && Abs(Z(target) - Z(self)) < 6 && Rand(ai.Rand3 + 3) == 0 && targetState != 14)
                    input.KeyJump = 1;
                if (heldOid == 124 && Rand(ai.Rand15 + 30) == 0) input.KeyJump = 1;
                if (Rand(ai.Rand3 + 5) == 0)
                {
                    bool close = !HasInputHistoryGate(self) || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
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

            battleCharacterInputWriter.ClearCurrentKeys(input);
            if (selfState == 17 && sameZLane && !specialObjectProximity && HitStop(self) != 0)
            { input.KeyAttack = 1; return false; }
            if (HasInputHistoryGate(self) && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240)) return false;
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
            if (specialObjectProximity || (ObjectId(self) != 2 && ObjectId(self) != 34) || Pp(self) <= 150 || Rand(ai.Rand3 + 3) <= 0)
            { input.KeyJump = 1; return false; }
            if (X(target) > X(self)) input.ComboDrj = 3; else input.ComboDlj = 3;
            return true;
        }

        private bool HasAiSoADecisionHeldLineCoverRows(
            LF2Entity self,
            LF2Entity target,
            BattleAiInputDetailDiagnostics diagnostics)
        {
            AiSoASensingRows rows = aiSoASensingRows;
            int selfSlot = Slot(self);
            int selfTeam = Team(self);
            int selfX = X(self);
            int selfZ = Z(self);
            int targetX = X(target);
            int targetTeam = Team(target);
            bool lineCover = false;
            int count = Math.Min(20, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Held20Scan,
                    1);
                if (!rows.Included[slot] ||
                    slot == selfSlot ||
                    rows.Team[slot] == 0 ||
                    targetTeam != selfTeam ||
                    rows.Hp[slot] <= 0 ||
                    rows.State[slot] == 14 ||
                    Abs(rows.Y[slot]) > 2)
                {
                    continue;
                }

                int candidateX = rows.X[slot];
                if (Abs(rows.Z[slot] - selfZ) < 15 &&
                    ((selfX < candidateX && candidateX < targetX) ||
                     (targetX < candidateX && candidateX < selfX)))
                {
                    lineCover = true;
                }
            }

            return lineCover;
        }

        private bool HasAiSoADecisionHeldLineCoverLegacy(
            LF2Entity self,
            LF2Entity target,
            BattleAiInputDetailDiagnostics diagnostics,
            int startSlot,
            bool lineCover)
        {
            for (int slot = startSlot; slot < 20; slot++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Held20Scan,
                    1);
                LF2Entity candidate = AiAt(slot);
                if (candidate == null ||
                    candidate == self ||
                    Team(candidate) == 0 ||
                    Team(target) != Team(self) ||
                    Hp(candidate) <= 0 ||
                    State(candidate) == 14 ||
                    Abs(Y(candidate)) > 2)
                {
                    continue;
                }

                if (Abs(Z(candidate) - Z(self)) < 15 &&
                    ((X(self) < X(candidate) && X(candidate) < X(target)) ||
                     (X(target) < X(candidate) && X(candidate) < X(self))))
                {
                    lineCover = true;
                }
            }

            return lineCover;
        }
    }
}
