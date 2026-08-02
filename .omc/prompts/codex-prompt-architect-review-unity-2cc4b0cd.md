---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs"
  - "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressHarness.cs"
  - "Temp/NTSD_ProductionEntityStress.dispersed-ai-optimized-production-notiming-20260726.json"
  - "Temp/NTSD_ProductionEntityStress.dispersed-ai-flat-shadow-gate-20260727.json"
  - "Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-a-quadtree-latefast-20260727.json"
  - "Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-b-flat-latefast-20260727.json"
timestamp: "2026-07-26T17:06:49.384Z"
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
        private SpatialAabbXZ aiInputSpatialPreferredRoot;
        private uint aiInputSpatialSourcePendingVersion;
        private uint aiInputSpatialSourceActiveVersion;
        private const int AiNearestFlatCellShift = 7;
        private const int AiNearestFlatCellSize = 1 << AiNearestFlatCellShift;
        private int[] aiNearestFlatXBySlot;
        private int[] aiNearestFlatYBySlot;
        private int[] aiNearestFlatZBySlot;
        private int[] aiNearestFlatHpBySlot;
        private int[] aiNearestFlatTeamBySlot;
        private int[] aiNearestFlatStateBySlot;
        private int[] aiNearestFlatDataObjectTypeBySlot;
        private double[] aiNearestFlatVxBySlot;
        private uint[] aiNearestFlatGenerationBySlot;
        private bool[] aiNearestFlatIncludedBySlot;
        private bool[] aiNearestFlatActiveBySlot;
        private bool[] aiNearestFlatGroundRoleBySlot;
        private bool[] aiNearestFlatAirRoleBySlot;
        private int[] aiNearestFlatGroundCellBySlot;
        private int[] aiNearestFlatGroundNextBySlot;
        private int[] aiNearestFlatGroundPreviousBySlot;
        private int[] aiNearestFlatAirCellBySlot;
        private int[] aiNearestFlatAirNextBySlot;
        private int[] aiNearestFlatAirPreviousBySlot;
        private bool[] aiNearestFlatCellUsed;
        private int[] aiNearestFlatCellX;
        private int[] aiNearestFlatCellZ;
        private int[] aiNearestFlatGroundHeadByCell;
        private int[] aiNearestFlatAirHeadByCell;
        private int[] aiNearestFlatUsedCellIndices;
        private int aiNearestFlatUsedCellCount;
        private int aiNearestFlatCellMask;
        private uint aiNearestFlatActiveVersion;
        private ulong aiNearestFlatOccupancyEpoch;
        private bool aiNearestFlatReady;

        // Diagnostic A/B switch. Production uses the compact slot list built from the same snapshot.
        internal bool ForceFullAiSpecialScanForDiagnostics { get; set; }
        internal bool ForceFullAiPhase1TargetScanForDiagnostics { get; set; }
        internal bool ForceFullAiSameTeamScanForDiagnostics { get; set; }
        internal bool ForceFullAiMoveModeScanForDiagnostics { get; set; }
        internal bool ForceFullAiNearestScanForDiagnostics { get; set; }
        internal bool ForceLegacyAiNearestQueryForDiagnostics { get; set; }
        internal bool ForceLiveAiNearestFactsForDiagnostics { get; set; }
        public bool ForceLegacyAiNearestFilterForDiagnostics { get; set; }
        internal bool ForceEagerAiGlobalSpatialBuildForDiagnostics { get; set; }
        public bool ForceLegacyAiSnapshotBuildForDiagnostics { get; set; }
        public bool ForceLegacyCharacterInputPassForDiagnostics { get; set; }
        public bool ForceAiNearestFlatQueryForDiagnostics { get; set; }
        public long CharacterInputNoAiSnapshotSkipCountForDiagnostics { get; private set; }
        public long CharacterInputNoAiProofFallbackCountForDiagnostics { get; private set; }
        public long CharacterInputDirectPassCountForDiagnostics { get; private set; }
        public long CharacterInputLegacyPassCountForDiagnostics { get; private set; }
        internal long AiSnapshotFastBuildCountForDiagnostics { get; private set; }
        internal long AiSnapshotLegacyBuildCountForDiagnostics { get; private set; }
        internal long AiSnapshotSlotVisitCountForDiagnostics { get; private set; }
        internal long AiSnapshotEpochFailureCountForDiagnostics { get; private set; }
        internal long AiSnapshotHandleFailureCountForDiagnostics { get; private set; }
        internal long AiSnapshotCoordinateFailureCountForDiagnostics { get; private set; }
        internal long AiSnapshotFallbackCountForDiagnostics { get; private set; }
#if UNITY_INCLUDE_TESTS
        internal Action<int> AiSnapshotSlotVisitHookForSelfCheck { get; set; }
        internal Action<int> CharacterInputNoAiProofSlotVisitHookForSelfCheck { get; set; }
#endif
        internal int AiGroundFilteredQueryCountForDiagnostics { get; private set; }
        // Self-check A/B switch for the OID-specialized AI decision dispatch.
        internal bool ForceLegacyAiOidDecisionDispatchForDiagnostics { get; set; }
        internal int AiOidDecisionDispatchGateForSelfCheck { get; private set; } = -1;
        // Independent A/B switch for OID1's close-decision and caller-prewrite tail.
        internal bool ForceLegacyAiOid1TailForDiagnostics { get; set; }
        internal bool CaptureAiOid1TailWitnessForDiagnostics { get; set; }
        internal int AiOid1CloseDecisionInvocationCountForDiagnostics { get; private set; }
        internal int AiOid1CallerPrewriteInvocationCountForDiagnostics { get; private set; }
        internal bool EnableAiNearestBestFirstShadowForDiagnostics { get; set; }
        public bool EnableAiNearestFlatShadowForDiagnostics { get; set; }
        internal int AiSameTeamSummaryFallbackCountForDiagnostics { get; private set; }
        internal int AiNearestBestFirstShadowMismatchCountForDiagnostics { get; private set; }
        internal string AiNearestBestFirstFirstShadowMismatchForDiagnostics { get; private set; }
        public int AiNearestFlatShadowQueryCountForDiagnostics { get; private set; }
        public int AiNearestFlatShadowUnavailableCountForDiagnostics { get; private set; }
        public int AiNearestFlatShadowMismatchCountForDiagnostics { get; private set; }
        public string AiNearestFlatFirstShadowMismatchForDiagnostics { get; private set; }
        public int AiNearestFlatGroundSequentialFallbackCountForDiagnostics { get; private set; }
        public int AiNearestFlatFormalQueryCountForDiagnostics { get; private set; }
        public int AiNearestFlatFormalFallbackCountForDiagnostics { get; private set; }
        internal int AiNearestAirPassCountForDiagnostics { get; private set; }
        internal int AiGlobalSpatialSynchronizeCountForDiagnostics { get; private set; }
        internal bool AiGlobalSpatialBuildPendingForDiagnostics =>
            aiInputSpatialSourcePendingVersion != 0 &&
            aiInputSpatialSourcePendingVersion == aiInputSpatialSourceActiveVersion;

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

        private enum CharacterInputAiParticipationProof
        {
            Invalid = 0,
            NoAiCharacter = 1,
            HasAiCharacter = 2,
        }

        private struct AiGroundNearestSnapshotFilter : IIncrementalPointNearestFilter
        {
            public AiNearestSnapshotStamp Stamp;
            public LF2Entity SelfEntity;
            public int SelfSlot;
            public int SelfX;
            public int SelfTeam;
            public int InputPhase;

            public IncrementalPointFilterDecision Evaluate(
                RuntimeEntityHandle handle)
            {
                int slot = handle.Slot;
                if (!handle.IsValid ||
                    slot < 0 ||
                    slot >= Stamp.SlotCount ||
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
                    !facts.Included ||
                    !facts.Active ||
                    !ReferenceEquals(facts.Entity, candidate))
                {
                    return IncrementalPointFilterDecision.Abort;
                }

                return IsGroundAiTargetCandidate(
                        SelfEntity,
                        SelfSlot,
                        SelfX,
                        SelfTeam,
                        in facts,
                        InputPhase)
                    ? IncrementalPointFilterDecision.Accept
                    : IncrementalPointFilterDecision.Reject;
            }
        }

        private struct AiAirNearestSnapshotFilter : IIncrementalPointNearestFilter
        {
            public AiNearestSnapshotStamp Stamp;
            public LF2Entity SelfEntity;
            public int SelfSlot;
            public int SelfTeam;
            public int InputPhase;

            public IncrementalPointFilterDecision Evaluate(
                RuntimeEntityHandle handle)
            {
                int slot = handle.Slot;
                if (!handle.IsValid ||
                    slot < 0 ||
                    slot >= Stamp.SlotCount ||
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
                    !facts.Included ||
                    !facts.Active ||
                    !ReferenceEquals(facts.Entity, candidate))
                {
                    return IncrementalPointFilterDecision.Abort;
                }

                return IsAirAiTargetCandidate(
                        SelfEntity,
                        SelfSlot,
                        SelfTeam,
                        in facts,
                        InputPhase)
                    ? IncrementalPointFilterDecision.Accept
                    : IncrementalPointFilterDecision.Reject;
            }
        }

        private struct AiNearestPointFilter : IIncrementalPointNearestFilter
        {
            public SimulationWorld World;
            public LF2Entity SelfEntity;
            public int SelfSlot;
            public int SelfX;
            public int SelfY;
            public int SelfZ;
            public int SelfTeam;
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
                if (World.ForceLiveAiNearestFactsForDiagnostics)
                {
                    if (candidate?.Runtime == null ||
                        candidate.Runtime.SlotIndex != slot)
                    {
                        return IncrementalPointFilterDecision.Abort;
                    }

                    bool liveAccepted = Air
                        ? IsAirAiTargetCandidate(
                            SelfEntity,
                            candidate,
                            InputPhase)
                        : IsGroundAiTargetCandidate(
                            SelfEntity,
                            candidate,
                            InputPhase);
                    return liveAccepted
                        ? IncrementalPointFilterDecision.Accept
                        : IncrementalPointFilterDecision.Reject;
                }

                if (World.aiNearestFactsBySlot == null ||
                    slot >= World.aiNearestFactsBySlot.Length ||
                    World.aiNearestFactsActiveVersion == 0)
                {
                    return IncrementalPointFilterDecision.Abort;
                }

                AiNearestSlotFacts facts = World.aiNearestFactsBySlot[slot];
                if (facts.SnapshotVersion !=
                        World.aiNearestFactsActiveVersion ||
                    facts.OccupancyEpoch !=
                        World.aiInputSlotSnapshotOccupancyEpoch ||
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
        }

        private void BuildAiInputSlotSnapshot()
        {
            aiInputSlotSnapshotOccupancyEpoch = 0;
            aiNearestFactsActiveVersion = 0;
            aiInputSpatialSourceActiveVersion = 0;
            aiInputSpatialSourcePendingVersion = 0;
            aiInputSpatialReady = false;
            uint factsVersion = AdvanceAiNearestFactsVersion();
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            AiSameTeamSummaryFallbackCountForDiagnostics = 0;
            AiGlobalSpatialSynchronizeCountForDiagnostics = 0;
            AiGroundFilteredQueryCountForDiagnostics = 0;
            ResetAiNearestAirPassCountForSelfCheck();
            EnsureAiTeamHpSnapshotCapacity();
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);
            Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
            ulong occupancyEpochBefore;
            if (ForceLegacyAiSnapshotBuildForDiagnostics)
            {
                occupancyEpochBefore = RuntimeSlotOccupancyEpochForServices;
                BuildAiInputSlotSnapshotLegacy();
                AiSnapshotLegacyBuildCountForDiagnostics++;
            }
            else
            {
                occupancyEpochBefore = RuntimeSlotOccupancyEpochForServices;
                if (TryBuildAiInputSlotSnapshotFromRuntimeSlots(
                        occupancyEpochBefore))
                {
                    AiSnapshotFastBuildCountForDiagnostics++;
                }
                else
                {
                    AiSnapshotFallbackCountForDiagnostics++;
                    Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
                    _entityScratch.Clear();
                    occupancyEpochBefore =
                        RuntimeSlotOccupancyEpochForServices;
                    BuildAiInputSlotSnapshotLegacy();
                    AiSnapshotLegacyBuildCountForDiagnostics++;
                }
            }
            bool factsProven = CaptureAiNearestFactsSnapshot(
                factsVersion,
                occupancyEpochBefore);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotSlotSnapshot);

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);
            BuildAiSnapshotIndices();
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotIndexBuild);

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);
            if (ForceAiNearestFlatQueryForDiagnostics)
                ResetAiInputSpatialSnapshotForFlatFormalQuery();
            else
                SynchronizeAiInputSpatialSnapshot(factsVersion);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.SnapshotQuadtreeSync);

            ulong occupancyEpochAfter =
                RuntimeSlotOccupancyEpochForServices;
            if (occupancyEpochBefore == occupancyEpochAfter)
            {
                aiInputSlotSnapshotOccupancyEpoch = occupancyEpochAfter;
                if (factsProven)
                    aiNearestFactsActiveVersion = factsVersion;
                if (aiInputSpatialSourcePendingVersion == factsVersion)
                {
                    aiInputSpatialSourceActiveVersion = factsVersion;
                    if (ForceEagerAiGlobalSpatialBuildForDiagnostics)
                        EnsureAiInputGlobalSpatialReady();
                }
                BuildAiNearestFlatShadowIndex();
            }
            else
            {
                InvalidateAiNearestFlatShadowIndex();
                InvalidateAiGlobalSpatialSource(true);
            }
        }

        private CharacterInputAiParticipationProof ProveCharacterInputAiParticipation()
        {
            if (aiInputSlotSnapshotOccupancyEpoch != 0 ||
                aiNearestFactsActiveVersion != 0 ||
                aiInputSpatialSourceActiveVersion != 0 ||
                aiInputSpatialSourcePendingVersion != 0)
            {
                return CharacterInputAiParticipationProof.Invalid;
            }

            ulong expectedOccupancyEpoch = RuntimeSlotOccupancyEpochForServices;
            int capacity = RuntimeSlotCapacity;
            if (capacity != aiInputSlots.Length)
                return CharacterInputAiParticipationProof.Invalid;

            for (int slot = 0; slot < capacity; slot++)
            {
#if UNITY_INCLUDE_TESTS
                CharacterInputNoAiProofSlotVisitHookForSelfCheck?.Invoke(slot);
#endif
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(slot);
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return CharacterInputAiParticipationProof.Invalid;
                    continue;
                }

                LF2Entity entity = view.Entity;
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != slot)
                {
                    return CharacterInputAiParticipationProof.Invalid;
                }

                var handle = new RuntimeEntityHandle(slot, view.Generation);
                if (!handle.IsValid ||
                    !_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    return CharacterInputAiParticipationProof.Invalid;
                }

                if (!IsActiveForCurrentPass(entity))
                    continue;

                if (entity.AiControlled &&
                    entity.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character)
                {
                    return RuntimeSlotOccupancyEpochForServices ==
                           expectedOccupancyEpoch
                        ? CharacterInputAiParticipationProof.HasAiCharacter
                        : CharacterInputAiParticipationProof.Invalid;
                }
            }

            return RuntimeSlotOccupancyEpochForServices ==
                   expectedOccupancyEpoch
                ? CharacterInputAiParticipationProof.NoAiCharacter
                : CharacterInputAiParticipationProof.Invalid;
        }

        private bool TrySkipAiSnapshotForCharacterInput()
        {
            if (ForceLegacyCharacterInputPassForDiagnostics)
                return false;

            CharacterInputAiParticipationProof proof =
                ProveCharacterInputAiParticipation();
            if (proof == CharacterInputAiParticipationProof.NoAiCharacter)
            {
                CharacterInputNoAiSnapshotSkipCountForDiagnostics++;
                ResetAiInputDiagnosticsForNoAiCharacterPass();
                return true;
            }

            if (proof == CharacterInputAiParticipationProof.Invalid)
                CharacterInputNoAiProofFallbackCountForDiagnostics++;
            return false;
        }

        private void ResetAiInputDiagnosticsForNoAiCharacterPass()
        {
            AiSameTeamSummaryFallbackCountForDiagnostics = 0;
            AiGlobalSpatialSynchronizeCountForDiagnostics = 0;
            AiGroundFilteredQueryCountForDiagnostics = 0;
            ResetAiNearestAirPassCountForSelfCheck();
        }

        private bool TryBuildAiInputSlotSnapshotFromRuntimeSlots(
            ulong expectedOccupancyEpoch)
        {
            int visitedSlots = 0;
            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                visitedSlots++;
#if UNITY_INCLUDE_TESTS
                AiSnapshotSlotVisitHookForSelfCheck?.Invoke(slot);
#endif
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(slot);
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                    {
                        AiSnapshotSlotVisitCountForDiagnostics +=
                            visitedSlots;
                        AiSnapshotHandleFailureCountForDiagnostics++;
                        return false;
                    }
                    continue;
                }

                LF2Entity entity = view.Entity;
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != slot)
                {
                    AiSnapshotSlotVisitCountForDiagnostics +=
                        visitedSlots;
                    AiSnapshotHandleFailureCountForDiagnostics++;
                    return false;
                }

                var handle = new RuntimeEntityHandle(slot, view.Generation);
                if (!handle.IsValid ||
                    !_runtimeSlots.TryResolve(
                        handle,
                        out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    AiSnapshotSlotVisitCountForDiagnostics +=
                        visitedSlots;
                    AiSnapshotHandleFailureCountForDiagnostics++;
                    return false;
                }

                if (!IsActiveForCurrentPass(entity))
                    continue;

                NTSDEntityRuntime runtime = entity.Runtime;
                if (runtime.XInt == int.MaxValue ||
                    runtime.ZInt == int.MaxValue ||
                    double.IsNaN(runtime.X) ||
                    double.IsInfinity(runtime.X) ||
                    double.IsNaN(runtime.Y) ||
                    double.IsInfinity(runtime.Y) ||
                    double.IsNaN(runtime.Z) ||
                    double.IsInfinity(runtime.Z))
                {
                    AiSnapshotSlotVisitCountForDiagnostics +=
                        visitedSlots;
                    AiSnapshotCoordinateFailureCountForDiagnostics++;
                    return false;
                }

                aiInputSlots[slot] = entity;
            }

            if (RuntimeSlotOccupancyEpochForServices !=
                expectedOccupancyEpoch)
            {
                AiSnapshotSlotVisitCountForDiagnostics += visitedSlots;
                AiSnapshotEpochFailureCountForDiagnostics++;
                return false;
            }

            AiSnapshotSlotVisitCountForDiagnostics += visitedSlots;
            return true;
        }

        private void BuildAiInputSlotSnapshotLegacy()
        {
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
        }

        private void ClearAiInputSlotSnapshot()
        {
            aiInputSlotSnapshotOccupancyEpoch = 0;
            aiNearestFactsActiveVersion = 0;
            InvalidateAiNearestFlatShadowIndex();
            InvalidateAiGlobalSpatialSource(false);
            Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
            if (aiNearestFactsBySlot != null)
                Array.Clear(aiNearestFactsBySlot, 0, aiNearestFactsBySlot.Length);
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

        private void BuildAiNearestFlatShadowIndex()
        {
            InvalidateAiNearestFlatShadowIndex();
            if ((!EnableAiNearestFlatShadowForDiagnostics &&
                 !ForceAiNearestFlatQueryForDiagnostics) ||
                aiNearestFactsActiveVersion == 0 ||
                aiInputSlotSnapshotOccupancyEpoch == 0 ||
                RuntimeSlotOccupancyEpochForServices !=
                    aiInputSlotSnapshotOccupancyEpoch)
            {
                return;
            }

            EnsureAiNearestFlatShadowCapacity();
            ResetAiNearestFlatCellTable();
            Array.Fill(aiNearestFlatGroundCellBySlot, -1);
            Array.Fill(aiNearestFlatGroundNextBySlot, -1);
            Array.Fill(aiNearestFlatGroundPreviousBySlot, -1);
            Array.Fill(aiNearestFlatAirCellBySlot, -1);
            Array.Fill(aiNearestFlatAirNextBySlot, -1);
            Array.Fill(aiNearestFlatAirPreviousBySlot, -1);

            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                ref readonly AiNearestSlotFacts facts =
                    ref aiNearestFactsBySlot[slot];
                if (!facts.Included)
                {
                    ClearAiNearestFlatSlotFacts(slot);
                    continue;
                }

                if (facts.SnapshotVersion != aiNearestFactsActiveVersion ||
                    facts.OccupancyEpoch !=
                        aiInputSlotSnapshotOccupancyEpoch ||
                    facts.Slot != slot ||
                    facts.HandleGeneration == 0 ||
                    !facts.Active ||
                    facts.Entity == null ||
                    !ReferenceEquals(facts.Entity, aiInputSlots[slot]))
                {
                    InvalidateAiNearestFlatShadowIndex();
                    return;
                }

                CopyAiNearestFlatSlotFacts(slot, in facts);
                if (facts.GroundRole &&
                    !TryAttachAiNearestFlatSlot(slot, false))
                {
                    InvalidateAiNearestFlatShadowIndex();
                    return;
                }
                if (facts.AirRole &&
                    !TryAttachAiNearestFlatSlot(slot, true))
                {
                    InvalidateAiNearestFlatShadowIndex();
                    return;
                }
            }

            aiNearestFlatActiveVersion = aiNearestFactsActiveVersion;
            aiNearestFlatOccupancyEpoch =
                aiInputSlotSnapshotOccupancyEpoch;
            aiNearestFlatReady = true;
        }

        private void EnsureAiNearestFlatShadowCapacity()
        {
            int slotCount = aiInputSlots.Length;
            if (aiNearestFlatXBySlot?.Length != slotCount)
            {
                aiNearestFlatXBySlot = new int[slotCount];
                aiNearestFlatYBySlot = new int[slotCount];
                aiNearestFlatZBySlot = new int[slotCount];
                aiNearestFlatHpBySlot = new int[slotCount];
                aiNearestFlatTeamBySlot = new int[slotCount];
                aiNearestFlatStateBySlot = new int[slotCount];
                aiNearestFlatDataObjectTypeBySlot = new int[slotCount];
                aiNearestFlatVxBySlot = new double[slotCount];
                aiNearestFlatGenerationBySlot = new uint[slotCount];
                aiNearestFlatIncludedBySlot = new bool[slotCount];
                aiNearestFlatActiveBySlot = new bool[slotCount];
                aiNearestFlatGroundRoleBySlot = new bool[slotCount];
                aiNearestFlatAirRoleBySlot = new bool[slotCount];
                aiNearestFlatGroundCellBySlot = new int[slotCount];
                aiNearestFlatGroundNextBySlot = new int[slotCount];
                aiNearestFlatGroundPreviousBySlot = new int[slotCount];
                aiNearestFlatAirCellBySlot = new int[slotCount];
                aiNearestFlatAirNextBySlot = new int[slotCount];
                aiNearestFlatAirPreviousBySlot = new int[slotCount];
            }

            long requiredLong = Math.Max(16L, (long)slotCount * 4L);
            int required = requiredLong >= (1L << 30)
                ? 1 << 30
                : (int)requiredLong;
            int cellCapacity = 16;
            while (cellCapacity < required)
                cellCapacity <<= 1;
            if (aiNearestFlatCellUsed?.Length == cellCapacity)
                return;

            aiNearestFlatCellUsed = new bool[cellCapacity];
            aiNearestFlatCellX = new int[cellCapacity];
            aiNearestFlatCellZ = new int[cellCapacity];
            aiNearestFlatGroundHeadByCell = new int[cellCapacity];
            aiNearestFlatAirHeadByCell = new int[cellCapacity];
            aiNearestFlatUsedCellIndices = new int[cellCapacity];
            aiNearestFlatUsedCellCount = 0;
            aiNearestFlatCellMask = cellCapacity - 1;
        }

        private void ResetAiNearestFlatCellTable()
        {
            for (int i = 0; i < aiNearestFlatUsedCellCount; i++)
            {
                int cellIndex = aiNearestFlatUsedCellIndices[i];
                aiNearestFlatCellUsed[cellIndex] = false;
                aiNearestFlatGroundHeadByCell[cellIndex] = -1;
                aiNearestFlatAirHeadByCell[cellIndex] = -1;
            }
            aiNearestFlatUsedCellCount = 0;
        }

        private void ClearAiNearestFlatSlotFacts(int slot)
        {
            aiNearestFlatXBySlot[slot] = 0;
            aiNearestFlatYBySlot[slot] = 0;
            aiNearestFlatZBySlot[slot] = 0;
            aiNearestFlatHpBySlot[slot] = 0;
            aiNearestFlatTeamBySlot[slot] = 0;
            aiNearestFlatStateBySlot[slot] = 0;
            aiNearestFlatDataObjectTypeBySlot[slot] = 0;
            aiNearestFlatVxBySlot[slot] = 0.0;
            aiNearestFlatGenerationBySlot[slot] = 0;
            aiNearestFlatIncludedBySlot[slot] = false;
            aiNearestFlatActiveBySlot[slot] = false;
            aiNearestFlatGroundRoleBySlot[slot] = false;
            aiNearestFlatAirRoleBySlot[slot] = false;
        }

        private void CopyAiNearestFlatSlotFacts(
            int slot,
            in AiNearestSlotFacts facts)
        {
            aiNearestFlatXBySlot[slot] = facts.X;
            aiNearestFlatYBySlot[slot] = facts.Y;
            aiNearestFlatZBySlot[slot] = facts.Z;
            aiNearestFlatHpBySlot[slot] = facts.Hp;
            aiNearestFlatTeamBySlot[slot] = facts.Team;
            aiNearestFlatStateBySlot[slot] = facts.State;
            aiNearestFlatDataObjectTypeBySlot[slot] =
                facts.DataObjectType;
            aiNearestFlatVxBySlot[slot] = facts.Vx;
            aiNearestFlatGenerationBySlot[slot] =
                facts.HandleGeneration;
            aiNearestFlatIncludedBySlot[slot] = facts.Included;
            aiNearestFlatActiveBySlot[slot] = facts.Active;
            aiNearestFlatGroundRoleBySlot[slot] = facts.GroundRole;
            aiNearestFlatAirRoleBySlot[slot] = facts.AirRole;
        }

        private int FindAiNearestFlatCell(
            int cellX,
            int cellZ,
            bool create)
        {
            if (aiNearestFlatCellMask <= 0)
                return -1;

            uint hash = unchecked(
                (uint)cellX * 73856093u ^
                (uint)cellZ * 19349663u);
            int cellIndex = (int)(hash & (uint)aiNearestFlatCellMask);
            for (int probe = 0;
                 probe <= aiNearestFlatCellMask;
                 probe++)
            {
                if (!aiNearestFlatCellUsed[cellIndex])
                {
                    if (!create ||
                        aiNearestFlatUsedCellCount >=
                            aiNearestFlatUsedCellIndices.Length)
                    {
                        return -1;
                    }

                    aiNearestFlatCellUsed[cellIndex] = true;
                    aiNearestFlatCellX[cellIndex] = cellX;
                    aiNearestFlatCellZ[cellIndex] = cellZ;
                    aiNearestFlatGroundHeadByCell[cellIndex] = -1;
                    aiNearestFlatAirHeadByCell[cellIndex] = -1;
                    aiNearestFlatUsedCellIndices[
                        aiNearestFlatUsedCellCount++] = cellIndex;
                    return cellIndex;
                }

                if (aiNearestFlatCellX[cellIndex] == cellX &&
                    aiNearestFlatCellZ[cellIndex] == cellZ)
                {
                    return cellIndex;
                }
                cellIndex = (cellIndex + 1) &
                            aiNearestFlatCellMask;
            }
            return -1;
        }

        private bool TryAttachAiNearestFlatSlot(
            int slot,
            bool air)
        {
            int cellX =
                aiNearestFlatXBySlot[slot] >> AiNearestFlatCellShift;
            int cellZ =
                aiNearestFlatZBySlot[slot] >> AiNearestFlatCellShift;
            int cellIndex = FindAiNearestFlatCell(cellX, cellZ, true);
            if (cellIndex < 0)
                return false;

            if (air)
            {
                int previousHead =
                    aiNearestFlatAirHeadByCell[cellIndex];
                aiNearestFlatAirCellBySlot[slot] = cellIndex;
                aiNearestFlatAirPreviousBySlot[slot] = -1;
                aiNearestFlatAirNextBySlot[slot] = previousHead;
                if (previousHead >= 0)
                    aiNearestFlatAirPreviousBySlot[previousHead] = slot;
                aiNearestFlatAirHeadByCell[cellIndex] = slot;
            }
            else
            {
                int previousHead =
                    aiNearestFlatGroundHeadByCell[cellIndex];
                aiNearestFlatGroundCellBySlot[slot] = cellIndex;
                aiNearestFlatGroundPreviousBySlot[slot] = -1;
                aiNearestFlatGroundNextBySlot[slot] = previousHead;
                if (previousHead >= 0)
                    aiNearestFlatGroundPreviousBySlot[previousHead] = slot;
                aiNearestFlatGroundHeadByCell[cellIndex] = slot;
            }
            return true;
        }

        private bool TryDetachAiNearestFlatSlot(
            int slot,
            bool air)
        {
            int cellIndex = air
                ? aiNearestFlatAirCellBySlot[slot]
                : aiNearestFlatGroundCellBySlot[slot];
            if (cellIndex < 0 ||
                cellIndex >= aiNearestFlatCellUsed.Length ||
                !aiNearestFlatCellUsed[cellIndex])
            {
                return false;
            }

            int previous = air
                ? aiNearestFlatAirPreviousBySlot[slot]
                : aiNearestFlatGroundPreviousBySlot[slot];
            int next = air
                ? aiNearestFlatAirNextBySlot[slot]
                : aiNearestFlatGroundNextBySlot[slot];
            if (previous >= 0)
            {
                if (air)
                    aiNearestFlatAirNextBySlot[previous] = next;
                else
                    aiNearestFlatGroundNextBySlot[previous] = next;
            }
            else if (air)
            {
                if (aiNearestFlatAirHeadByCell[cellIndex] != slot)
                    return false;
                aiNearestFlatAirHeadByCell[cellIndex] = next;
            }
            else
            {
                if (aiNearestFlatGroundHeadByCell[cellIndex] != slot)
                    return false;
                aiNearestFlatGroundHeadByCell[cellIndex] = next;
            }

            if (next >= 0)
            {
                if (air)
                    aiNearestFlatAirPreviousBySlot[next] = previous;
                else
                    aiNearestFlatGroundPreviousBySlot[next] = previous;
            }

            if (air)
            {
                aiNearestFlatAirCellBySlot[slot] = -1;
                aiNearestFlatAirNextBySlot[slot] = -1;
                aiNearestFlatAirPreviousBySlot[slot] = -1;
            }
            else
            {
                aiNearestFlatGroundCellBySlot[slot] = -1;
                aiNearestFlatGroundNextBySlot[slot] = -1;
                aiNearestFlatGroundPreviousBySlot[slot] = -1;
            }
            return true;
        }

        private void ObserveAiNearestFlatShadowMutation(
            LF2Entity entity)
        {
            if (!aiNearestFlatReady)
                return;
            if (!IsAiNearestFlatShadowIndexCurrent() ||
                entity?.Runtime == null)
            {
                InvalidateAiNearestFlatShadowIndex();
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
                InvalidateAiNearestFlatShadowIndex();
                return;
            }

            ref readonly AiNearestSlotFacts facts =
                ref aiNearestFactsBySlot[slot];
            if (facts.SnapshotVersion !=
                    aiNearestFactsActiveVersion ||
                facts.OccupancyEpoch !=
                    aiInputSlotSnapshotOccupancyEpoch ||
                facts.HandleGeneration != handle.Generation ||
                facts.Slot != slot ||
                !facts.Included ||
                !facts.Active ||
                !ReferenceEquals(facts.Entity, entity))
            {
                InvalidateAiNearestFlatShadowIndex();
                return;
            }

            if (aiNearestFlatGroundRoleBySlot[slot] &&
                !TryDetachAiNearestFlatSlot(slot, false))
            {
                InvalidateAiNearestFlatShadowIndex();
                return;
            }
            if (aiNearestFlatAirRoleBySlot[slot] &&
                !TryDetachAiNearestFlatSlot(slot, true))
            {
                InvalidateAiNearestFlatShadowIndex();
                return;
            }

            CopyAiNearestFlatSlotFacts(slot, in facts);
            if (facts.GroundRole &&
                !TryAttachAiNearestFlatSlot(slot, false))
            {
                InvalidateAiNearestFlatShadowIndex();
                return;
            }
            if (facts.AirRole &&
                !TryAttachAiNearestFlatSlot(slot, true))
            {
                InvalidateAiNearestFlatShadowIndex();
            }
        }

        private bool IsAiNearestFlatShadowIndexCurrent()
        {
            return (EnableAiNearestFlatShadowForDiagnostics ||
                    ForceAiNearestFlatQueryForDiagnostics) &&
                   aiNearestFlatReady &&
                   aiNearestFlatActiveVersion != 0 &&
                   aiNearestFlatActiveVersion ==
                       aiNearestFactsActiveVersion &&
                   aiNearestFlatOccupancyEpoch != 0 &&
                   aiNearestFlatOccupancyEpoch ==
                       aiInputSlotSnapshotOccupancyEpoch &&
                   RuntimeSlotOccupancyEpochForServices ==
                       aiNearestFlatOccupancyEpoch &&
                   aiNearestFlatXBySlot?.Length ==
                       aiInputSlots.Length;
        }

        private void InvalidateAiNearestFlatShadowIndex()
        {
            aiNearestFlatReady = false;
            aiNearestFlatActiveVersion = 0;
            aiNearestFlatOccupancyEpoch = 0;
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
            ObserveAiNearestFactsMutation(entity);
            ObserveAiNearestFlatShadowMutation(entity);
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
            if (entity?.Runtime == null || !IsActiveForCurrentPass(entity))
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
            InvalidateAiGlobalSpatialSource(true);
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
                return;
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

        private void ResetAiInputSpatialSnapshotForFlatFormalQuery()
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
            InvalidateAiGlobalSpatialSource(true);
            aiInputGroundSpatialBroadphase.ResetIncremental();
            aiInputGroundSpatialReady = false;
            InvalidateAiGroundTeamPartitions();
            InvalidateAiAirRoleSnapshot();
        }

        private void SynchronizeAiInputSpatialSnapshot(uint sourceVersion)
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
                    InvalidateAiGlobalSpatialSource(true);
                    aiInputGroundSpatialBroadphase.ResetIncremental();
                    InvalidateAiGroundTeamPartitions();
                    InvalidateAiAirRoleSnapshot();
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
                    partition.Entries.Add(entry);
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
                InvalidateAiGlobalSpatialSource(true);
                aiInputGroundSpatialBroadphase.ResetIncremental();
                InvalidateAiGroundTeamPartitions();
                InvalidateAiAirRoleSnapshot();
                aiInputGroundSpatialReady = false;
                return;
            }

            var preferredRoot = new SpatialAabbXZ(minX, minZ, maxX, maxZ);
            // The authority-visible slot snapshot is complete here. Keep the all-role
            // source for fail-closed fallback, but avoid synchronizing its duplicate
            // tree unless a query cannot be handled by a role/team-specific index.
            aiInputSpatialPreferredRoot = preferredRoot;
            aiInputSpatialSourcePendingVersion = sourceVersion;
            SynchronizeAiGroundTeamPartitions(preferredRoot);

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

        private bool EnsureAiInputGlobalSpatialReady()
        {
            if (aiInputSpatialReady)
                return true;
            if (aiInputSpatialSourceActiveVersion == 0 ||
                aiInputSpatialSourcePendingVersion !=
                    aiInputSpatialSourceActiveVersion ||
                aiInputSlotSnapshotOccupancyEpoch == 0 ||
                RuntimeSlotOccupancyEpochForServices !=
                    aiInputSlotSnapshotOccupancyEpoch ||
                !aiInputSpatialPreferredRoot.IsValid)
            {
                InvalidateAiGlobalSpatialSource(true);
                return false;
            }

            aiInputSpatialSourcePendingVersion = 0;
            AiGlobalSpatialSynchronizeCountForDiagnostics++;
            SpatialSynchronizeResult result = aiInputSpatialBroadphase.Synchronize(
                aiInputSpatialEntries,
                aiInputSpatialPreferredRoot);
            aiInputSpatialReady = result.Succeeded &&
                                  result.IndexedCount == aiInputSpatialEntries.Count;
            if (aiInputSpatialReady)
                return true;

            InvalidateAiGlobalSpatialSource(true);
            InvalidateAiGroundTeamPartitions();
            return false;
        }

        private void InvalidateAiGlobalSpatialSource(bool resetIndex)
        {
            if (resetIndex)
                aiInputSpatialBroadphase.ResetIncremental();
            aiInputSpatialReady = false;
            aiInputSpatialPreferredRoot = default;
            aiInputSpatialSourcePendingVersion = 0;
            aiInputSpatialSourceActiveVersion = 0;
        }

        private bool TryQueryAiInputSlots(in SpatialAabbXZ bounds, out List<int> slots)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.RecordSpatialQuery();
            slots = aiInputSpatialSlots;
            slots.Clear();
            if (!bounds.IsValid || !EnsureAiInputGlobalSpatialReady())
                return false;

            aiInputSpatialHandles.Clear();
            try
            {
                aiInputSpatialBroadphase.QueryHandles(bounds, aiInputSpatialHandles);
                diagnostics?.RecordQueriedHandleVisits(aiInputSpatialHandles.Count);
            }
            catch
            {
                InvalidateAiGlobalSpatialSource(true);
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
                    InvalidateAiGlobalSpatialSource(true);
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
            bool hasHistoryGate = input.HasInputHistoryGate();
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
                                   (hasHistoryGate && objOid != 0x7A);
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

            if (hasHistoryGate && input.LinkState > 0)
            {
                LF2Entity held = AiAt(input.TargetSlotIndex);
                if (held != null && (held.ObjectId == 0x7A || held.ObjectId == 0x7B))
                { input.PrevJump = 0; input.KeyJump = 1; input.ApplyInputEdges(); return; }
            }

            bool coordinateAllowsSpecial = !hasHistoryGate || AiPostCacheCoordinateAllowsSpecial(self);
            if (coordinateAllowsSpecial && (targetState == 0x3EC || targetState == 0x7D4))
            {
                if (hasHistoryGate && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240) &&
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

            bool c8Allowed = (hasHistoryGate && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240)) ||
                             (targetState != 14 && Abs(Y(target)) <= 2);
            if (c8Allowed && targetOid == 0xC8)
            {
                if (X(target) > X(self) + 7) input.KeyRight = 1; else if (X(target) < X(self) - 7) input.KeyLeft = 1;
                if (Z(target) > Z(self) + 2) input.KeyDown = 1; else if (Z(target) < Z(self) - 2) input.KeyUp = 1;
                input.ApplyInputEdges(); return;
            }

            bool oidDecisionGate = Rand(ai.Rand5 + 1) == 0;
            AiOidDecisionDispatchGateForSelfCheck = oidDecisionGate ? 1 : 0;
            if (oidDecisionGate)
            {
                bool decided = ForceLegacyAiOidDecisionDispatchForDiagnostics
                    ? AiUpdateLegacyOidDecisionChain(
                        self,
                        target,
                        ai,
                        bestDist,
                        specialObjectProximity,
                        sameZLane,
                        targetState)
                    : AiUpdateSpecializedOidDecisionChain(
                        self,
                        target,
                        ai,
                        bestDist,
                        specialObjectProximity,
                        sameZLane,
                        targetState);
                if (decided)
                { input.ApplyInputEdges(); return; }
            }

            bool followUpDecided = ForceLegacyAiOidDecisionDispatchForDiagnostics
                ? AiUpdateLegacyOidFollowUpChain(self, target, targetState)
                : AiUpdateSpecializedOidFollowUpChain(self, target, targetState);
            if (followUpDecided)
            { input.ApplyInputEdges(); return; }

            bool closeOrFree = !hasHistoryGate || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
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

            if (selfOid != 1)
                AiProcessSubCallerPrewrite(self, target, ai, selfState, targetState);
            else if (ForceLegacyAiOid1TailForDiagnostics)
                InvokeAiProcessSubCallerPrewrite(self, target, ai, selfState, targetState);
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

        private bool TryFindNearestAiTargetSlotFlatShadow(
            LF2Entity self,
            AiInputContext ai,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            selected = -1;
            bestDist = 10000;
            sameZLane = false;
            if (!IsAiNearestFlatShadowIndexCurrent() ||
                self?.Runtime == null)
            {
                return false;
            }

            int selfSlot = Slot(self);
            if (selfSlot < 0 ||
                selfSlot >= aiInputSlots.Length ||
                !ReferenceEquals(aiInputSlots[selfSlot], self) ||
                !aiNearestFlatIncludedBySlot[selfSlot] ||
                !aiNearestFlatActiveBySlot[selfSlot] ||
                !TryGetCurrentRuntimeHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle selfHandle) ||
                aiNearestFlatGenerationBySlot[selfSlot] !=
                    selfHandle.Generation)
            {
                InvalidateAiNearestFlatShadowIndex();
                return false;
            }

            int selfX = aiNearestFlatXBySlot[selfSlot];
            int selfZ = aiNearestFlatZBySlot[selfSlot];
            int selfTeam = aiNearestFlatTeamBySlot[selfSlot];
            if (!TryFindNearestAiFlatRole(
                    selfSlot,
                    selfX,
                    selfZ,
                    selfTeam,
                    ai.InputPhase,
                    false,
                    10000,
                    10000,
                    10000,
                    out int groundSelected,
                    out bestDist))
            {
                return false;
            }

            selected = groundSelected;
            sameZLane =
                selected >= 0 &&
                SaturatingAbsDifferenceLong(
                    aiNearestFlatZBySlot[selected],
                    selfZ) < 15;
            if (aiNearestFlatStateBySlot[selfSlot] != 9)
            {
                if (!TryFindNearestAiFlatRole(
                        selfSlot,
                        selfX,
                        selfZ,
                        selfTeam,
                        ai.InputPhase,
                        true,
                        10000,
                        250,
                        40,
                        out int airSelected,
                        out _))
                {
                    return false;
                }
                if (airSelected >= 0)
                    selected = airSelected;
            }

            if (!IsAiNearestFlatShadowIndexCurrent())
                return false;
            return true;
        }

        private bool TryFindNearestAiFlatRole(
            int selfSlot,
            int selfX,
            int selfZ,
            int selfTeam,
            int inputPhase,
            bool air,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            out int selected,
            out int nearestDistance)
        {
            selected = -1;
            nearestDistance = maxDistanceExclusive;
            int originCellX = selfX >> AiNearestFlatCellShift;
            int originCellZ = selfZ >> AiNearestFlatCellShift;
            int maxAxisDistance = Math.Max(
                maxAbsXExclusive,
                maxAbsZExclusive);
            int maxRing =
                (maxAxisDistance + AiNearestFlatCellSize - 1) /
                AiNearestFlatCellSize + 1;
            for (int ring = 0; ring <= maxRing; ring++)
            {
                if (!TryVisitAiNearestFlatRing(
                        originCellX,
                        originCellZ,
                        ring,
                        selfSlot,
                        selfX,
                        selfZ,
                        selfTeam,
                        inputPhase,
                        air,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive,
                        ref selected,
                        ref nearestDistance))
                {
                    InvalidateAiNearestFlatShadowIndex();
                    return false;
                }

                if (!air && ring == 4 && selected < 0)
                {
                    AiNearestFlatGroundSequentialFallbackCountForDiagnostics++;
                    FindNearestAiFlatRoleSequential(
                        selfSlot,
                        selfX,
                        selfZ,
                        selfTeam,
                        inputPhase,
                        false,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive,
                        ref selected,
                        ref nearestDistance);
                    return true;
                }

                long minimumOutside =
                    MinimumDistanceOutsideAiNearestFlatRing(
                        selfX,
                        selfZ,
                        originCellX,
                        originCellZ,
                        ring);
                if (minimumOutside >= maxDistanceExclusive ||
                    minimumOutside > nearestDistance)
                {
                    break;
                }
            }
            return true;
        }

        private bool TryVisitAiNearestFlatRing(
            int originCellX,
            int originCellZ,
            int ring,
            int selfSlot,
            int selfX,
            int selfZ,
            int selfTeam,
            int inputPhase,
            bool air,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            ref int selected,
            ref int nearestDistance)
        {
            if (ring == 0)
            {
                return TryVisitAiNearestFlatCell(
                    originCellX,
                    originCellZ,
                    selfSlot,
                    selfX,
                    selfZ,
                    selfTeam,
                    inputPhase,
                    air,
                    maxDistanceExclusive,
                    maxAbsXExclusive,
                    maxAbsZExclusive,
                    ref selected,
                    ref nearestDistance);
            }

            int minCellX = originCellX - ring;
            int maxCellX = originCellX + ring;
            int minCellZ = originCellZ - ring;
            int maxCellZ = originCellZ + ring;
            for (int cellX = minCellX; cellX <= maxCellX; cellX++)
            {
                if (!TryVisitAiNearestFlatCell(
                        cellX,
                        minCellZ,
                        selfSlot,
                        selfX,
                        selfZ,
                        selfTeam,
                        inputPhase,
                        air,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive,
                        ref selected,
                        ref nearestDistance) ||
                    !TryVisitAiNearestFlatCell(
                        cellX,
                        maxCellZ,
                        selfSlot,
                        selfX,
                        selfZ,
                        selfTeam,
                        inputPhase,
                        air,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive,
                        ref selected,
                        ref nearestDistance))
                {
                    return false;
                }
            }

            for (int cellZ = minCellZ + 1;
                 cellZ < maxCellZ;
                 cellZ++)
            {
                if (!TryVisitAiNearestFlatCell(
                        minCellX,
                        cellZ,
                        selfSlot,
                        selfX,
                        selfZ,
                        selfTeam,
                        inputPhase,
                        air,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive,
                        ref selected,
                        ref nearestDistance) ||
                    !TryVisitAiNearestFlatCell(
                        maxCellX,
                        cellZ,
                        selfSlot,
                        selfX,
                        selfZ,
                        selfTeam,
                        inputPhase,
                        air,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive,
                        ref selected,
                        ref nearestDistance))
                {
                    return false;
                }
            }
            return true;
        }

        private bool TryVisitAiNearestFlatCell(
            int cellX,
            int cellZ,
            int selfSlot,
            int selfX,
            int selfZ,
            int selfTeam,
            int inputPhase,
            bool air,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            ref int selected,
            ref int nearestDistance)
        {
            long cellMinX = (long)cellX * AiNearestFlatCellSize;
            long cellMinZ = (long)cellZ * AiNearestFlatCellSize;
            long lowerX = DistanceToClosedInterval(
                selfX,
                cellMinX,
                cellMinX + AiNearestFlatCellSize - 1L);
            if (lowerX >= maxAbsXExclusive)
                return true;
            long lowerZ = DistanceToClosedInterval(
                selfZ,
                cellMinZ,
                cellMinZ + AiNearestFlatCellSize - 1L);
            if (lowerZ >= maxAbsZExclusive)
                return true;
            long lowerBound = lowerX + lowerZ;
            if (lowerBound >= maxDistanceExclusive ||
                lowerBound > nearestDistance)
            {
                return true;
            }

            int cellIndex = FindAiNearestFlatCell(
                cellX,
                cellZ,
                false);
            if (cellIndex < 0)
                return true;

            int slot = air
                ? aiNearestFlatAirHeadByCell[cellIndex]
                : aiNearestFlatGroundHeadByCell[cellIndex];
            int visited = 0;
            while (slot >= 0)
            {
                if (slot >= aiInputSlots.Length ||
                    ++visited > aiInputSlots.Length)
                {
                    return false;
                }

                int next = air
                    ? aiNearestFlatAirNextBySlot[slot]
                    : aiNearestFlatGroundNextBySlot[slot];
                int memberCell = air
                    ? aiNearestFlatAirCellBySlot[slot]
                    : aiNearestFlatGroundCellBySlot[slot];
                bool role = air
                    ? aiNearestFlatAirRoleBySlot[slot]
                    : aiNearestFlatGroundRoleBySlot[slot];
                if (memberCell != cellIndex || !role)
                    return false;

                if (IsAiNearestFlatCandidateAllowed(
                        slot,
                        selfSlot,
                        selfX,
                        selfTeam,
                        inputPhase,
                        air))
                {
                    ConsiderAiNearestFlatCandidate(
                        slot,
                        selfX,
                        selfZ,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive,
                        ref selected,
                        ref nearestDistance);
                }
                slot = next;
            }
            return true;
        }

        private void FindNearestAiFlatRoleSequential(
            int selfSlot,
            int selfX,
            int selfZ,
            int selfTeam,
            int inputPhase,
            bool air,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            ref int selected,
            ref int nearestDistance)
        {
            for (int slot = 0;
                 slot < aiInputSlots.Length;
                 slot++)
            {
                if (!IsAiNearestFlatCandidateAllowed(
                        slot,
                        selfSlot,
                        selfX,
                        selfTeam,
                        inputPhase,
                        air))
                {
                    continue;
                }

                ConsiderAiNearestFlatCandidate(
                    slot,
                    selfX,
                    selfZ,
                    maxDistanceExclusive,
                    maxAbsXExclusive,
                    maxAbsZExclusive,
                    ref selected,
                    ref nearestDistance);
            }
        }

        private void ConsiderAiNearestFlatCandidate(
            int slot,
            int selfX,
            int selfZ,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            ref int selected,
            ref int nearestDistance)
        {
            long deltaX = SaturatingAbsDifferenceLong(
                aiNearestFlatXBySlot[slot],
                selfX);
            long deltaZ = SaturatingAbsDifferenceLong(
                aiNearestFlatZBySlot[slot],
                selfZ);
            long distance = deltaX + deltaZ;
            if (deltaX < maxAbsXExclusive &&
                deltaZ < maxAbsZExclusive &&
                distance < maxDistanceExclusive &&
                (distance < nearestDistance ||
                 (distance == nearestDistance &&
                  (selected < 0 || slot < selected))))
            {
                nearestDistance = (int)distance;
                selected = slot;
            }
        }

        private bool IsAiNearestFlatCandidateAllowed(
            int slot,
            int selfSlot,
            int selfX,
            int selfTeam,
            int inputPhase,
            bool air)
        {
            if (slot == selfSlot ||
                !aiNearestFlatIncludedBySlot[slot] ||
                !aiNearestFlatActiveBySlot[slot] ||
                aiNearestFlatHpBySlot[slot] <= 0 ||
                !TeamCandidateAllowed(
                    selfTeam,
                    aiNearestFlatTeamBySlot[slot],
                    inputPhase))
            {
                return false;
            }

            if (air)
                return aiNearestFlatAirRoleBySlot[slot];
            if (!aiNearestFlatGroundRoleBySlot[slot] ||
                aiNearestFlatStateBySlot[slot] == 14 ||
                SaturatingAbsValueLong(
                    aiNearestFlatYBySlot[slot]) > 2)
            {
                return false;
            }

            if (aiNearestFlatDataObjectTypeBySlot[slot] == 0)
                return true;
            if (aiNearestFlatStateBySlot[slot] != 3000)
                return false;
            int candidateX = aiNearestFlatXBySlot[slot];
            if (candidateX > selfX)
                return aiNearestFlatVxBySlot[slot] < 0.001;
            if (candidateX < selfX)
                return aiNearestFlatVxBySlot[slot] > 0.001;
            return false;
        }

        private static long MinimumDistanceOutsideAiNearestFlatRing(
            int pointX,
            int pointZ,
            int originCellX,
            int originCellZ,
            int ring)
        {
            long leftOutsideMax =
                ((long)originCellX - ring) *
                AiNearestFlatCellSize - 1L;
            long rightOutsideMin =
                ((long)originCellX + ring + 1L) *
                AiNearestFlatCellSize;
            long bottomOutsideMax =
                ((long)originCellZ - ring) *
                AiNearestFlatCellSize - 1L;
            long topOutsideMin =
                ((long)originCellZ + ring + 1L) *
                AiNearestFlatCellSize;
            long left = (long)pointX - leftOutsideMax;
            long right = rightOutsideMin - pointX;
            long bottom = (long)pointZ - bottomOutsideMax;
            long top = topOutsideMin - pointZ;
            return Math.Min(Math.Min(left, right), Math.Min(bottom, top));
        }

        private static long DistanceToClosedInterval(
            long value,
            long minimum,
            long maximum)
        {
            if (value < minimum)
                return minimum - value;
            if (value > maximum)
                return value - maximum;
            return 0;
        }

        private static long SaturatingAbsDifferenceLong(
            int first,
            int second)
        {
            long delta = (long)first - second;
            return delta < 0 ? -delta : delta;
        }

        private static long SaturatingAbsValueLong(int value)
        {
            return value < 0 ? -(long)value : value;
        }

        private void RunAiNearestFlatShadow(
            LF2Entity self,
            AiInputContext ai,
            bool formalSucceeded,
            int formalSelected,
            int formalBestDist,
            bool formalSameZLane)
        {
            if (!EnableAiNearestFlatShadowForDiagnostics)
                return;

            AiNearestFlatShadowQueryCountForDiagnostics++;
            uint rngStateBefore = Rng?.State ?? 0;
            ulong rngCallsBefore = Rng?.CallCount ?? 0;
            ulong inputSignatureBefore =
                CaptureAiNearestInputSignature(self?.Runtime);
            bool shadowSucceeded =
                TryFindNearestAiTargetSlotFlatShadow(
                    self,
                    ai,
                    out int shadowSelected,
                    out int shadowBestDist,
                    out bool shadowSameZLane);
            uint rngStateAfter = Rng?.State ?? 0;
            ulong rngCallsAfter = Rng?.CallCount ?? 0;
            ulong inputSignatureAfter =
                CaptureAiNearestInputSignature(self?.Runtime);
            if (!shadowSucceeded)
            {
                AiNearestFlatShadowUnavailableCountForDiagnostics++;
                return;
            }

            if (formalSucceeded != shadowSucceeded ||
                formalSelected != shadowSelected ||
                formalBestDist != shadowBestDist ||
                formalSameZLane != shadowSameZLane ||
                rngStateBefore != rngStateAfter ||
                rngCallsBefore != rngCallsAfter ||
                inputSignatureBefore != inputSignatureAfter)
            {
                AiNearestFlatShadowMismatchCountForDiagnostics++;
                if (AiNearestFlatFirstShadowMismatchForDiagnostics == null)
                {
                    AiNearestFlatFirstShadowMismatchForDiagnostics =
                        $"self={Slot(self)} phase={ai.InputPhase} " +
                        $"formal={formalSucceeded}/{formalSelected}/" +
                        $"{formalBestDist}/{formalSameZLane} " +
                        $"flat={shadowSucceeded}/{shadowSelected}/" +
                        $"{shadowBestDist}/{shadowSameZLane} " +
                        $"rng={rngStateBefore}:{rngCallsBefore}->" +
                        $"{rngStateAfter}:{rngCallsAfter} " +
                        $"input={inputSignatureBefore}->" +
                        $"{inputSignatureAfter}";
                }
            }
        }

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
                    int indexedSelected =
                        FindNearestAiPhase1TargetSlotIndexed(
                        self,
                        out bestDist,
                        out sameZLane);
                    RunAiNearestFlatShadow(
                        self,
                        ai,
                        true,
                        indexedSelected,
                        bestDist,
                        sameZLane);
                    return indexedSelected;
                }
                finally
                {
                    diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
                }
            }

            if (ForceAiNearestFlatQueryForDiagnostics)
            {
                AiNearestFlatFormalQueryCountForDiagnostics++;
                bool flatSucceeded = TryFindNearestAiTargetSlotFlatShadow(
                    self,
                    ai,
                    out int flatSelected,
                    out bestDist,
                    out sameZLane);
                if (flatSucceeded)
                {
                    RunAiNearestFlatShadow(
                        self,
                        ai,
                        true,
                        flatSelected,
                        bestDist,
                        sameZLane);
                    return flatSelected;
                }

                AiNearestFlatFormalFallbackCountForDiagnostics++;
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics?.RecordBruteFallback();
                return FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out bestDist,
                    out sameZLane);
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

            if (formalSucceeded)
            {
                RunAiNearestFlatShadow(
                    self,
                    ai,
                    true,
                    selected,
                    bestDist,
                    sameZLane);
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
            return new AiNearestPointFilter
            {
                World = this,
                SelfEntity = self,
                SelfSlot = runtime?.SlotIndex ?? -1,
                SelfX = runtime?.XInt ?? 0,
                SelfY = runtime?.YInt ?? 0,
                SelfZ = runtime?.ZInt ?? 0,
                SelfTeam = runtime?.RelationTeam ?? 0,
                InputPhase = inputPhase,
                Air = air,
            };
        }

        private bool TryCreateAiNearestSnapshotStamp(
            out AiNearestSnapshotStamp stamp)
        {
            stamp = default;
            LF2Entity[] slots = aiInputSlots;
            uint[] generationBySlot = aiInputGroundGenerationBySlot;
            AiNearestSlotFacts[] factsBySlot = aiNearestFactsBySlot;
            ulong occupancyEpoch = aiInputSlotSnapshotOccupancyEpoch;
            uint factsVersion = aiNearestFactsActiveVersion;
            int slotCount = slots?.Length ?? 0;
            if (slots == null ||
                occupancyEpoch == 0 ||
                factsVersion == 0 ||
                generationBySlot == null ||
                factsBySlot == null ||
                generationBySlot.Length != slotCount ||
                factsBySlot.Length != slotCount ||
                RuntimeSlotOccupancyEpochForServices != occupancyEpoch)
            {
                return false;
            }

            stamp = new AiNearestSnapshotStamp
            {
                Slots = slots,
                GenerationBySlot = generationBySlot,
                FactsBySlot = factsBySlot,
                OccupancyEpoch = occupancyEpoch,
                FactsVersion = factsVersion,
                SlotCount = slotCount,
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
                   aiInputSlotSnapshotOccupancyEpoch ==
                       stamp.OccupancyEpoch &&
                   RuntimeSlotOccupancyEpochForServices ==
                       stamp.OccupancyEpoch &&
                   aiNearestFactsActiveVersion == stamp.FactsVersion;
        }

        private AiGroundNearestSnapshotFilter
            CreateAiGroundNearestSnapshotFilter(
                LF2Entity self,
                int inputPhase,
                in AiNearestSnapshotStamp stamp)
        {
            NTSDEntityRuntime runtime = self?.Runtime;
            return new AiGroundNearestSnapshotFilter
            {
                Stamp = stamp,
                SelfEntity = self,
                SelfSlot = runtime?.SlotIndex ?? -1,
                SelfX = runtime?.XInt ?? 0,
                SelfTeam = runtime?.RelationTeam ?? 0,
                InputPhase = inputPhase,
            };
        }

        private AiAirNearestSnapshotFilter
            CreateAiAirNearestSnapshotFilter(
                LF2Entity self,
                int inputPhase,
                in AiNearestSnapshotStamp stamp)
        {
            NTSDEntityRuntime runtime = self?.Runtime;
            return new AiAirNearestSnapshotFilter
            {
                Stamp = stamp,
                SelfEntity = self,
                SelfSlot = runtime?.SlotIndex ?? -1,
                SelfTeam = runtime?.RelationTeam ?? 0,
                InputPhase = inputPhase,
            };
        }

        private bool TryFindNearestGroundBestFirstFiltered<TFilter>(
            LF2Entity self,
            int inputPhase,
            ref TFilter filter,
            BattleAiInputDetailDiagnostics diagnostics,
            out RuntimeEntityHandle nearestHandle,
            out int nearestDistance)
            where TFilter : struct, IIncrementalPointNearestFilter
        {
            bool partitionSucceeded =
                TryFindNearestGroundInSingleAllowedTeamPartition(
                    self,
                    inputPhase,
                    ref filter,
                    diagnostics,
                    out bool partitionHandled,
                    out nearestHandle,
                    out nearestDistance,
                    out _);
            bool succeeded = partitionHandled && partitionSucceeded;
            if (partitionHandled)
                return succeeded;

            diagnostics?.RecordSpatialQuery();
            LooseQuadtreeBroadphase groundBroadphase =
                aiInputGroundSpatialReady
                    ? aiInputGroundSpatialBroadphase
                    : aiInputSpatialBroadphase;
            if (!aiInputGroundSpatialReady &&
                !EnsureAiInputGlobalSpatialReady())
            {
                nearestHandle = RuntimeEntityHandle.Invalid;
                nearestDistance = 10000;
                return false;
            }

            succeeded = groundBroadphase.TryFindNearestPointManhattan(
                X(self),
                Z(self),
                10000,
                10000,
                10000,
                ref filter,
                out nearestHandle,
                out nearestDistance,
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
                if (!EnsureAiInputGlobalSpatialReady())
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
                        out nearestDistance,
                        out visitedRecords);
                diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                diagnostics?.RecordCandidateVisits(visitedRecords);
            }

            return succeeded;
        }

        private bool TryFindNearestGroundBestFirstFiltered(
            LF2Entity self,
            int inputPhase,
            BattleAiInputDetailDiagnostics diagnostics,
            out RuntimeEntityHandle nearestHandle,
            out int nearestDistance)
        {
            if (ForceLegacyAiNearestFilterForDiagnostics ||
                ForceLiveAiNearestFactsForDiagnostics)
            {
                AiNearestPointFilter legacyFilter =
                    CreateAiNearestPointFilter(
                        self,
                        inputPhase,
                        false);
                return TryFindNearestGroundBestFirstFiltered(
                    self,
                    inputPhase,
                    ref legacyFilter,
                    diagnostics,
                    out nearestHandle,
                    out nearestDistance);
            }

            if (!TryCreateAiNearestSnapshotStamp(
                    out AiNearestSnapshotStamp stamp))
            {
                nearestHandle = RuntimeEntityHandle.Invalid;
                nearestDistance = 10000;
                return false;
            }

            AiGroundNearestSnapshotFilter filter =
                CreateAiGroundNearestSnapshotFilter(
                    self,
                    inputPhase,
                    in stamp);
            bool succeeded = TryFindNearestGroundBestFirstFiltered(
                self,
                inputPhase,
                ref filter,
                diagnostics,
                out nearestHandle,
                out nearestDistance);
            return IsAiNearestSnapshotStampCurrent(in stamp) && succeeded;
        }

        private bool TryFindNearestAirBestFirstFiltered<TFilter>(
            LF2Entity self,
            ref TFilter filter,
            BattleAiInputDetailDiagnostics diagnostics,
            out RuntimeEntityHandle nearestHandle)
            where TFilter : struct, IIncrementalPointNearestFilter
        {
            diagnostics?.RecordSpatialQuery();
            LooseQuadtreeBroadphase airBroadphase = aiInputAirSpatialReady
                ? aiInputAirSpatialBroadphase
                : aiInputSpatialBroadphase;
            if (!aiInputAirSpatialReady &&
                !EnsureAiInputGlobalSpatialReady())
            {
                nearestHandle = RuntimeEntityHandle.Invalid;
                return false;
            }

            bool succeeded = airBroadphase.TryFindNearestPointManhattan(
                X(self),
                Z(self),
                10000,
                250,
                40,
                ref filter,
                out nearestHandle,
                out _,
                out int visitedRecords);
            diagnostics?.RecordQueriedHandleVisits(visitedRecords);
            diagnostics?.RecordCandidateVisits(visitedRecords);
            if (!succeeded &&
                ReferenceEquals(airBroadphase, aiInputAirSpatialBroadphase))
            {
                ResetAiAirSpatialIndex();
                if (!EnsureAiInputGlobalSpatialReady())
                    return false;
                diagnostics?.RecordSpatialQuery();
                succeeded =
                    aiInputSpatialBroadphase.TryFindNearestPointManhattan(
                        X(self),
                        Z(self),
                        10000,
                        250,
                        40,
                        ref filter,
                        out nearestHandle,
                        out _,
                        out visitedRecords);
                diagnostics?.RecordQueriedHandleVisits(visitedRecords);
                diagnostics?.RecordCandidateVisits(visitedRecords);
            }

            return succeeded;
        }

        private bool TryFindNearestAirBestFirstFiltered(
            LF2Entity self,
            int inputPhase,
            BattleAiInputDetailDiagnostics diagnostics,
            out RuntimeEntityHandle nearestHandle)
        {
            if (ForceLegacyAiNearestFilterForDiagnostics ||
                ForceLiveAiNearestFactsForDiagnostics)
            {
                AiNearestPointFilter legacyFilter =
                    CreateAiNearestPointFilter(
                        self,
                        inputPhase,
                        true);
                return TryFindNearestAirBestFirstFiltered(
                    self,
                    ref legacyFilter,
                    diagnostics,
                    out nearestHandle);
            }

            if (!TryCreateAiNearestSnapshotStamp(
                    out AiNearestSnapshotStamp stamp))
            {
                nearestHandle = RuntimeEntityHandle.Invalid;
                return false;
            }

            AiAirNearestSnapshotFilter filter =
                CreateAiAirNearestSnapshotFilter(
                    self,
                    inputPhase,
                    in stamp);
            bool succeeded = TryFindNearestAirBestFirstFiltered(
                self,
                ref filter,
                diagnostics,
                out nearestHandle);
            return IsAiNearestSnapshotStampCurrent(in stamp) && succeeded;
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
            if (!aiInputGroundTeamPartitionsValid &&
                !aiInputGroundSpatialReady &&
                !EnsureAiInputGlobalSpatialReady())
                return false;

            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
            try
            {
                bool succeeded = TryFindNearestGroundBestFirstFiltered(
                    self,
                    ai.InputPhase,
                    diagnostics,
                    out RuntimeEntityHandle nearestHandle,
                    out bestDist);
                if (!succeeded)
                {
                    InvalidateAiGlobalSpatialSource(true);
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
                    InvalidateAiGlobalSpatialSource(true);
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
                bool succeeded = TryFindNearestAirBestFirstFiltered(
                    self,
                    ai.InputPhase,
                    diagnostics,
                    out RuntimeEntityHandle nearestAirHandle);
                if (!succeeded)
                {
                    InvalidateAiGlobalSpatialSource(true);
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
                    InvalidateAiGlobalSpatialSource(true);
                    return false;
                }
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.FindNearestAir);
            }
            return true;
        }

        private bool TryFindNearestGroundInSingleAllowedTeamPartition<TFilter>(
            LF2Entity self,
            int inputPhase,
            ref TFilter filter,
            BattleAiInputDetailDiagnostics diagnostics,
            out bool handled,
            out RuntimeEntityHandle nearestHandle,
            out int nearestDistance,
            out int visitedRecords)
            where TFilter : struct, IIncrementalPointNearestFilter
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
                AiGroundFilteredQueryCountForDiagnostics++;
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

        private static bool IsAirAiTargetCandidate(LF2Entity self, LF2Entity candidate, int inputPhase)
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
                if (!EnsureAiInputGlobalSpatialReady())
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
                if (!EnsureAiInputGlobalSpatialReady())
                    return false;

                allowedPartitionCount = CountAllowedGroundTeamPartitions(
                    Team(self),
                    inputPhase,
                    out _);
                AiNearestPointFilter filter =
                    CreateAiNearestPointFilter(
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
                ObserveAiNearestFactsMutation(candidate);
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
        internal string CaptureAiSnapshotBuildWitnessForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceLegacy)
        {
            bool previousLegacy =
                ForceLegacyAiSnapshotBuildForDiagnostics;
            ForceLegacyAiSnapshotBuildForDiagnostics = forceLegacy;
            uint rngStateBefore = Rng?.State ?? 0;
            ulong rngCallsBefore = Rng?.CallCount ?? 0;
            BuildAiInputSlotSnapshot();
            try
            {
                var builder = new System.Text.StringBuilder(4096);
                builder.Append("slots:");
                for (int slot = 0; slot < aiInputSlots.Length; slot++)
                {
                    LF2Entity entity = aiInputSlots[slot];
                    if (entity == null)
                        continue;

                    TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle);
                    AiNearestSlotFacts facts =
                        aiNearestFactsBySlot[slot];
                    builder.Append(slot).Append('/')
                        .Append(entity.StableId).Append('/')
                        .Append(entity.ObjectId).Append('/')
                        .Append(handle.Generation).Append('/')
                        .Append(facts.X).Append('/')
                        .Append(facts.Y).Append('/')
                        .Append(facts.Z).Append('/')
                        .Append(facts.Hp).Append('/')
                        .Append(facts.Team).Append('/')
                        .Append(facts.State).Append('/')
                        .Append(facts.DataObjectType).Append('/')
                        .Append(facts.GroundRole ? 1 : 0).Append('/')
                        .Append(facts.AirRole ? 1 : 0).Append(';');
                }

                builder.Append("|special:");
                AppendAiSnapshotSlotsForSelfCheck(
                    builder,
                    aiSpecialScanSlots);
                builder.Append("|phase1:")
                    .Append(aiPhase1TargetSlotsValid ? 1 : 0)
                    .Append(':');
                AppendAiSnapshotSlotsForSelfCheck(
                    builder,
                    aiPhase1TargetSlots);
                builder.Append("|move:")
                    .Append(aiMoveModeFirst10Valid ? 1 : 0)
                    .Append('/')
                    .Append(aiMoveModeTopSlot).Append('/')
                    .Append(aiMoveModeTopX).Append('/')
                    .Append(aiMoveModeTopZ).Append('/')
                    .Append(aiMoveModeSecondSlot).Append('/')
                    .Append(aiMoveModeSecondX).Append('/')
                    .Append(aiMoveModeSecondZ);

                builder.Append("|indices:")
                    .Append(aiTeamHpSummaryValid ? 1 : 0).Append('/')
                    .Append(aiInputGroundSpatialReady ? 1 : 0).Append('/')
                    .Append(aiInputGroundTeamPartitionsValid ? 1 : 0)
                    .Append('/')
                    .Append(aiInputAirSpatialReady ? 1 : 0).Append('/')
                    .Append(aiInputAirRoleCountValid ? 1 : 0).Append('/')
                    .Append(aiInputAirRoleCount);
                for (int slot = 0; slot < aiInputSlots.Length; slot++)
                {
                    if (aiInputSlots[slot] == null)
                        continue;
                    builder.Append(';').Append(slot).Append('=')
                        .Append(aiTeamHpSnapshotEligible[slot] ? 1 : 0)
                        .Append(',')
                        .Append(aiTeamHpSnapshotTeams[slot]).Append(',')
                        .Append(aiTeamHpSnapshotValues[slot]).Append(',')
                        .Append(aiPhase1TeamBySlot[slot]).Append(',')
                        .Append(aiPhase1GenerationBySlot[slot]).Append(',')
                        .Append(aiInputGroundRoleBySlot[slot] ? 1 : 0)
                        .Append(',')
                        .Append(aiInputGroundTeamBySlot[slot]).Append(',')
                        .Append(aiInputGroundGenerationBySlot[slot]).Append(',')
                        .Append(aiInputAirRoleBySlot[slot] ? 1 : 0);
                }

                builder.Append("|partitions:")
                    .Append(aiInputActiveGroundTeamPartitions.Count);
                for (int i = 0;
                     i < aiInputActiveGroundTeamPartitions.Count;
                     i++)
                {
                    AiGroundTeamPartition partition =
                        aiInputActiveGroundTeamPartitions[i];
                    builder.Append(';')
                        .Append(partition.Team).Append('=')
                        .Append(partition.Entries.Count);
                }

                var ai = new AiInputContext
                {
                    InputPhase = inputPhase,
                };
                int nearest = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out int bestDist,
                    out bool sameZ);
                builder.Append("|nearest:")
                    .Append(nearest).Append('/')
                    .Append(bestDist).Append('/')
                    .Append(sameZ ? 1 : 0);
                builder.Append("|rng:")
                    .Append(rngStateBefore).Append('/')
                    .Append(rngCallsBefore).Append("->")
                    .Append(Rng?.State ?? 0).Append('/')
                    .Append(Rng?.CallCount ?? 0);
                builder.Append("|checksum:")
                    .Append(
                        CaptureExtendedChecksumSnapshot(0)
                            .OverallChecksum);
                return builder.ToString();
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                ForceLegacyAiSnapshotBuildForDiagnostics =
                    previousLegacy;
            }
        }

        internal long MeasureAiSnapshotBuildAllocationsForSelfCheck(
            int iterations)
        {
            if (iterations <= 0)
                return 0;

            bool previousLegacy =
                ForceLegacyAiSnapshotBuildForDiagnostics;
            ForceLegacyAiSnapshotBuildForDiagnostics = false;
            try
            {
                BuildAiInputSlotSnapshot();
                ClearAiInputSlotSnapshot();
                long before =
                    GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < iterations; i++)
                {
                    BuildAiInputSlotSnapshot();
                    ClearAiInputSlotSnapshot();
                }
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                ForceLegacyAiSnapshotBuildForDiagnostics =
                    previousLegacy;
            }
        }

        private static void AppendAiSnapshotSlotsForSelfCheck(
            System.Text.StringBuilder builder,
            List<int> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i != 0)
                    builder.Append(',');
                builder.Append(slots[i]);
            }
        }

        internal void CaptureAiNearestFactsTargetForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceLiveFacts,
            bool forceBrute,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            bool previousLive = ForceLiveAiNearestFactsForDiagnostics;
            bool previousFull = ForceFullAiNearestScanForDiagnostics;
            bool previousPhase1 = ForceFullAiPhase1TargetScanForDiagnostics;
            bool previousLegacy = ForceLegacyAiNearestQueryForDiagnostics;
            BuildAiInputSlotSnapshot();
            try
            {
                ForceLiveAiNearestFactsForDiagnostics = forceLiveFacts;
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
                ForceLiveAiNearestFactsForDiagnostics = previousLive;
                ForceFullAiNearestScanForDiagnostics = previousFull;
                ForceFullAiPhase1TargetScanForDiagnostics = previousPhase1;
                ForceLegacyAiNearestQueryForDiagnostics = previousLegacy;
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiNearestFlatMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out int flatSelected,
            out int flatBestDist,
            out bool flatSameZLane)
        {
            bool previous =
                EnableAiNearestFlatShadowForDiagnostics;
            flatSelected = -1;
            flatBestDist = 10000;
            flatSameZLane = false;
            EnableAiNearestFlatShadowForDiagnostics = true;
            BuildAiInputSlotSnapshot();
            try
            {
                var ai = new AiInputContext
                {
                    InputPhase = inputPhase,
                };
                if (!TryFindNearestAiTargetSlotFlatShadow(
                        self,
                        ai,
                        out flatSelected,
                        out flatBestDist,
                        out flatSameZLane))
                {
                    return false;
                }

                int bruteSelected = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteBestDist,
                    out bool bruteSameZLane);
                return flatSelected == bruteSelected &&
                       flatBestDist == bruteBestDist &&
                       flatSameZLane == bruteSameZLane;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                EnableAiNearestFlatShadowForDiagnostics = previous;
            }
        }

        internal bool AiNearestFlatObservedMutationMatchesBruteForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int x,
            int y,
            int z,
            int hp,
            int team)
        {
            bool previous =
                EnableAiNearestFlatShadowForDiagnostics;
            EnableAiNearestFlatShadowForDiagnostics = true;
            BuildAiInputSlotSnapshot();
            try
            {
                candidate.Runtime.SetPosition(x, y, z);
                candidate.Runtime.SyncIntegerPosition();
                candidate.Runtime.HP = hp;
                candidate.Team = team;
                candidate.RelationTeam = team;
                candidate.Runtime.RelationTeam = team;
                ObserveAiTeamHpSummaryMutation(candidate);

                var ai = new AiInputContext
                {
                    InputPhase = inputPhase,
                };
                if (!TryFindNearestAiTargetSlotFlatShadow(
                        self,
                        ai,
                        out int flatSelected,
                        out int flatBestDist,
                        out bool flatSameZLane))
                {
                    return false;
                }
                int bruteSelected = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteBestDist,
                    out bool bruteSameZLane);
                return flatSelected == bruteSelected &&
                       flatBestDist == bruteBestDist &&
                       flatSameZLane == bruteSameZLane;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                EnableAiNearestFlatShadowForDiagnostics = previous;
            }
        }

        internal bool AiNearestFlatRejectsSlotReuseForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase)
        {
            bool previous =
                EnableAiNearestFlatShadowForDiagnostics;
            EnableAiNearestFlatShadowForDiagnostics = true;
            BuildAiInputSlotSnapshot();
            try
            {
                Unregister(candidate);
                var ai = new AiInputContext
                {
                    InputPhase = inputPhase,
                };
                return !TryFindNearestAiTargetSlotFlatShadow(
                    self,
                    ai,
                    out _,
                    out _,
                    out _);
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                EnableAiNearestFlatShadowForDiagnostics = previous;
            }
        }

        internal long MeasureAiNearestFlatQueryAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            bool previous =
                EnableAiNearestFlatShadowForDiagnostics;
            EnableAiNearestFlatShadowForDiagnostics = true;
            BuildAiInputSlotSnapshot();
            try
            {
                var ai = new AiInputContext
                {
                    InputPhase = inputPhase,
                };
                if (!TryFindNearestAiTargetSlotFlatShadow(
                        self,
                        ai,
                        out _,
                        out _,
                        out _))
                {
                    return -1;
                }

                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < iterations; i++)
                {
                    if (!TryFindNearestAiTargetSlotFlatShadow(
                            self,
                            ai,
                            out _,
                            out _,
                            out _))
                    {
                        return -1;
                    }
                }
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                EnableAiNearestFlatShadowForDiagnostics = previous;
            }
        }

        internal long MeasureAiNearestFlatFormalQueryAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            bool previous = ForceAiNearestFlatQueryForDiagnostics;
            ForceAiNearestFlatQueryForDiagnostics = true;
            BuildAiInputSlotSnapshot();
            try
            {
                var ai = new AiInputContext
                {
                    InputPhase = inputPhase,
                };
                FindNearestAiTargetSlot(self, ai, out _, out _);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < iterations; i++)
                    FindNearestAiTargetSlot(self, ai, out _, out _);
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                ForceAiNearestFlatQueryForDiagnostics = previous;
            }
        }

        internal bool AiNearestFlatFormalForcedFallbackMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase)
        {
            bool previous = ForceAiNearestFlatQueryForDiagnostics;
            ForceAiNearestFlatQueryForDiagnostics = true;
            BuildAiInputSlotSnapshot();
            try
            {
                var ai = new AiInputContext
                {
                    InputPhase = inputPhase,
                };
                int fallbackCountBefore =
                    AiNearestFlatFormalFallbackCountForDiagnostics;
                uint rngStateBefore = Rng?.State ?? 0;
                ulong rngCallsBefore = Rng?.CallCount ?? 0;
                InvalidateAiNearestFlatShadowIndex();
                int formalSelected = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out int formalBestDist,
                    out bool formalSameZLane);
                int bruteSelected = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteBestDist,
                    out bool bruteSameZLane);
                return AiNearestFlatFormalFallbackCountForDiagnostics ==
                           fallbackCountBefore + 1 &&
                       formalSelected == bruteSelected &&
                       formalBestDist == bruteBestDist &&
                       formalSameZLane == bruteSameZLane &&
                       rngStateBefore == (Rng?.State ?? 0) &&
                       rngCallsBefore == (Rng?.CallCount ?? 0);
            }
            finally
            {
                ClearAiInputSlotSnapshot();
                ForceAiNearestFlatQueryForDiagnostics = previous;
            }
        }

        internal void BeginAiNearestFactsSnapshotForSelfCheck()
        {
            BuildAiInputSlotSnapshot();
        }

        internal void EndAiNearestFactsSnapshotForSelfCheck()
        {
            ClearAiInputSlotSnapshot();
        }

#if UNITY_INCLUDE_TESTS
        internal bool AiNearestSnapshotStampRejectsMutationForSelfCheck(
            int mutationKind)
        {
            BuildAiInputSlotSnapshot();
            LF2Entity[] previousSlots = aiInputSlots;
            uint[] previousGenerationBySlot =
                aiInputGroundGenerationBySlot;
            AiNearestSlotFacts[] previousFactsBySlot =
                aiNearestFactsBySlot;
            ulong previousOccupancyEpoch =
                aiInputSlotSnapshotOccupancyEpoch;
            uint previousFactsVersion = aiNearestFactsActiveVersion;
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
                            previousOccupancyEpoch == ulong.MaxValue
                                ? 1UL
                                : previousOccupancyEpoch + 1UL;
                        break;
                    case 1:
                        aiNearestFactsActiveVersion =
                            previousFactsVersion == uint.MaxValue
                                ? 1u
                                : previousFactsVersion + 1u;
                        break;
                    case 2:
                        aiInputSlots =
                            (LF2Entity[])previousSlots.Clone();
                        break;
                    case 3:
                        aiInputGroundGenerationBySlot =
                            (uint[])previousGenerationBySlot.Clone();
                        break;
                    case 4:
                        aiNearestFactsBySlot =
                            (AiNearestSlotFacts[])previousFactsBySlot.Clone();
                        break;
                    default:
                        return false;
                }

                return !IsAiNearestSnapshotStampCurrent(in stamp);
            }
            finally
            {
                aiInputSlots = previousSlots;
                aiInputGroundGenerationBySlot =
                    previousGenerationBySlot;
                aiNearestFactsBySlot = previousFactsBySlot;
                aiInputSlotSnapshotOccupancyEpoch =
                    previousOccupancyEpoch;
                aiNearestFactsActiveVersion = previousFactsVersion;
                ClearAiInputSlotSnapshot();
            }
        }
#endif

        internal void ObserveAiNearestFactsMutationForSelfCheck(
            LF2Entity entity)
        {
            ObserveAiTeamHpSummaryMutation(entity);
        }

        internal bool AiNearestFactsMatchLiveForSelfCheck(
            LF2Entity entity)
        {
            if (aiNearestFactsActiveVersion == 0 ||
                entity?.Runtime == null)
            {
                return false;
            }

            int slot = entity.Runtime.SlotIndex;
            if (slot < 0 ||
                slot >= aiNearestFactsBySlot.Length ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                !TryCaptureAiNearestSlotFacts(
                    entity,
                    slot,
                    handle.Generation,
                    aiNearestFactsActiveVersion,
                    aiInputSlotSnapshotOccupancyEpoch,
                    out AiNearestSlotFacts live))
            {
                return false;
            }

            AiNearestSlotFacts facts = aiNearestFactsBySlot[slot];
            return ReferenceEquals(facts.Entity, live.Entity) &&
                   facts.HandleGeneration == live.HandleGeneration &&
                   facts.SnapshotVersion == live.SnapshotVersion &&
                   facts.OccupancyEpoch == live.OccupancyEpoch &&
                   facts.Slot == live.Slot &&
                   facts.X == live.X &&
                   facts.Y == live.Y &&
                   facts.Z == live.Z &&
                   facts.Hp == live.Hp &&
                   facts.Team == live.Team &&
                   facts.State == live.State &&
                   facts.DataObjectType == live.DataObjectType &&
                   facts.Vx.Equals(live.Vx) &&
                   facts.Active == live.Active &&
                   facts.Included == live.Included &&
                   facts.GroundRole == live.GroundRole &&
                   facts.AirRole == live.AirRole;
        }

        internal void CaptureCurrentAiNearestFactsTargetForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceLiveFacts,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            bool previousLive = ForceLiveAiNearestFactsForDiagnostics;
            bool previousFull = ForceFullAiNearestScanForDiagnostics;
            bool previousPhase1 = ForceFullAiPhase1TargetScanForDiagnostics;
            bool previousLegacy = ForceLegacyAiNearestQueryForDiagnostics;
            try
            {
                ForceLiveAiNearestFactsForDiagnostics = forceLiveFacts;
                ForceFullAiNearestScanForDiagnostics = false;
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
                ForceLiveAiNearestFactsForDiagnostics = previousLive;
                ForceFullAiNearestScanForDiagnostics = previousFull;
                ForceFullAiPhase1TargetScanForDiagnostics = previousPhase1;
                ForceLegacyAiNearestQueryForDiagnostics = previousLegacy;
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
            bool previousDormant =
                candidate?.Runtime?.OidMergeDormant ?? false;
            bool previousPending =
                candidate?.Runtime?.PendingFlushDestroy ?? false;
            try
            {
                int slot = Slot(candidate);
                if (slot < 0 ||
                    slot >= aiNearestFactsBySlot.Length)
                {
                    return false;
                }

                AiNearestSlotFacts facts = aiNearestFactsBySlot[slot];
                switch (invalidationKind)
                {
                    case 0:
                        facts.SnapshotVersion =
                            facts.SnapshotVersion == uint.MaxValue
                                ? 1u
                                : facts.SnapshotVersion + 1u;
                        aiNearestFactsBySlot[slot] = facts;
                        break;
                    case 1:
                        facts.HandleGeneration =
                            facts.HandleGeneration == uint.MaxValue
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
                    candidate.Runtime.OidMergeDormant = previousDormant;
                    candidate.Runtime.PendingFlushDestroy = previousPending;
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

        internal bool AiGlobalSpatialLazyPartitionMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out bool pendingBeforeQuery,
            out int synchronizeCount,
            out int selectedSlot)
        {
            pendingBeforeQuery = false;
            synchronizeCount = -1;
            selectedSlot = -1;
            BuildAiInputSlotSnapshot();
            try
            {
                pendingBeforeQuery =
                    AiGlobalSpatialBuildPendingForDiagnostics;
                var ai = new AiInputContext { InputPhase = inputPhase };
                bool succeeded = TryFindNearestAiTargetSlotBestFirst(
                    self,
                    ai,
                    out selectedSlot,
                    out int selectedDistance,
                    out bool selectedSameZ,
                    true);
                int bruteSlot = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                synchronizeCount =
                    AiGlobalSpatialSynchronizeCountForDiagnostics;
                return pendingBeforeQuery &&
                       succeeded &&
                       synchronizeCount == 0 &&
                       AiGlobalSpatialBuildPendingForDiagnostics &&
                       selectedSlot == bruteSlot &&
                       selectedDistance == bruteDistance &&
                       selectedSameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGlobalSpatialLazyFirstUseMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out bool pendingBeforeQuery,
            out int synchronizeCount,
            out int selectedSlot)
        {
            pendingBeforeQuery = false;
            synchronizeCount = -1;
            selectedSlot = -1;
            BuildAiInputSlotSnapshot();
            try
            {
                pendingBeforeQuery =
                    AiGlobalSpatialBuildPendingForDiagnostics;
                var ai = new AiInputContext { InputPhase = inputPhase };
                bool succeeded = TryFindNearestAiTargetSlotSpatial(
                    self,
                    ai,
                    out selectedSlot,
                    out int selectedDistance,
                    out bool selectedSameZ);
                int bruteSlot = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                synchronizeCount =
                    AiGlobalSpatialSynchronizeCountForDiagnostics;
                return pendingBeforeQuery &&
                       succeeded &&
                       synchronizeCount == 1 &&
                       !AiGlobalSpatialBuildPendingForDiagnostics &&
                       selectedSlot == bruteSlot &&
                       selectedDistance == bruteDistance &&
                       selectedSameZ == bruteSameZ;
            }
            finally
            {
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGlobalSpatialMutationBeforeFirstUseFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            bool mutateZ,
            out bool pendingBeforeMutation,
            out int synchronizeCount)
        {
            pendingBeforeMutation = false;
            synchronizeCount = -1;
            bool previousForceLegacy =
                ForceLegacyAiNearestQueryForDiagnostics;
            BuildAiInputSlotSnapshot();
            try
            {
                pendingBeforeMutation =
                    AiGlobalSpatialBuildPendingForDiagnostics;
                if (mutateZ)
                {
                    candidate.Runtime.Z += 7;
                    candidate.Runtime.ZInt += 7;
                }
                else
                {
                    candidate.Runtime.X += 7;
                    candidate.Runtime.XInt += 7;
                }
                ObserveAiTeamHpSummaryMutation(candidate);

                var ai = new AiInputContext { InputPhase = inputPhase };
                ForceLegacyAiNearestQueryForDiagnostics = true;
                int selected = FindNearestAiTargetSlot(
                    self,
                    ai,
                    out int selectedDistance,
                    out bool selectedSameZ);
                int brute = FindNearestAiTargetSlotBrute(
                    self,
                    ai,
                    out int bruteDistance,
                    out bool bruteSameZ);
                synchronizeCount =
                    AiGlobalSpatialSynchronizeCountForDiagnostics;
                return pendingBeforeMutation &&
                       !AiGlobalSpatialBuildPendingForDiagnostics &&
                       !aiInputSpatialReady &&
                       synchronizeCount == 0 &&
                       selected == brute &&
                       selectedDistance == bruteDistance &&
                       selectedSameZ == bruteSameZ;
            }
            finally
            {
                ForceLegacyAiNearestQueryForDiagnostics =
                    previousForceLegacy;
                ClearAiInputSlotSnapshot();
            }
        }

        internal bool AiGlobalSpatialGenerationReuseRefusesBuildForSelfCheck(
            LF2Entity candidate,
            out bool generationChanged,
            out int synchronizeCount)
        {
            generationChanged = false;
            synchronizeCount = -1;
            BuildAiInputSlotSnapshot();
            int slot = candidate?.Runtime?.SlotIndex ?? -1;
            RuntimeEntityHandle originalHandle = RuntimeEntityHandle.Invalid;
            bool released = false;
            try
            {
                if (!AiGlobalSpatialBuildPendingForDiagnostics ||
                    !TryGetCurrentRuntimeHandle(
                        slot,
                        candidate,
                        out originalHandle) ||
                    !_runtimeSlots.Release(originalHandle))
                {
                    return false;
                }

                released = true;
                bool ensured = EnsureAiInputGlobalSpatialReady();
                synchronizeCount =
                    AiGlobalSpatialSynchronizeCountForDiagnostics;
                if (!_runtimeSlots.TryClaim(
                        slot,
                        candidate,
                        out RuntimeEntityHandle replacementHandle))
                {
                    return false;
                }
                released = false;
                generationChanged =
                    replacementHandle.Generation != originalHandle.Generation;
                return generationChanged &&
                       !ensured &&
                       synchronizeCount == 0 &&
                       !AiGlobalSpatialBuildPendingForDiagnostics &&
                       !aiInputSpatialReady;
            }
            finally
            {
                if (released)
                    _runtimeSlots.TryClaim(slot, candidate, out _);
                ClearAiInputSlotSnapshot();
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
                if (!aiInputGroundSpatialReady ||
                    !EnsureAiInputGlobalSpatialReady())
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
            AiNearestPointFilter filter =
                CreateAiNearestPointFilter(
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

        private static bool TeamCandidateAllowed(LF2Entity self, LF2Entity candidate, int inputPhase)
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

        private bool AiUpdateLegacyOidDecisionChain(
            LF2Entity self,
            LF2Entity target,
            AiInputContext ai,
            int bestDist,
            bool specialObjectProximity,
            bool sameZLane,
            int targetState)
        {
            return AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                   AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane) ||
                   AiUpdateOid1ComboDecision(self, target, targetState) ||
                   AiUpdateCloseOid1Decision(self, target) ||
                   AiUpdateOid4ComboDecision(self, target) ||
                   AiUpdateOid5ComboDecision(self, target);
        }

        private bool AiUpdateSpecializedOidDecisionChain(
            LF2Entity self,
            LF2Entity target,
            AiInputContext ai,
            int bestDist,
            bool specialObjectProximity,
            bool sameZLane,
            int targetState)
        {
            switch (self.ObjectId)
            {
                case 1:
                    return AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                           AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane) ||
                           AiUpdateOid1ComboDecision(self, target, targetState) ||
                           AiUpdateCloseOid1TailDecision(self, target);
                case 21:
                    return AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                           AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane) ||
                           AiUpdateOid1ComboDecision(self, target, targetState) ||
                           AiUpdateCloseOid1Decision(self, target);
                case 2:
                    return AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                           AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane);
                case 4:
                    return AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                           AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane) ||
                           AiUpdateOid4ComboDecision(self, target);
                case 5:
                    return AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                           AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane) ||
                           AiUpdateOid5ComboDecision(self, target);
                case 10:
                    return AiUpdateOid4ComboDecision(self, target);
                case 17:
                    return AiUpdateOid1ComboDecision(self, target, targetState) ||
                           AiUpdateCloseOid1Decision(self, target);
                case 19:
                    return AiUpdateOid4ComboDecision(self, target) ||
                           AiUpdateOid5ComboDecision(self, target);
                default:
                    return false;
            }
        }

        private bool AiUpdateLegacyOidFollowUpChain(
            LF2Entity self,
            LF2Entity target,
            int targetState)
        {
            return AiUpdateOid33_19_16PredictedDuaDecision(self, target, targetState) ||
                   AiUpdateOid52_1_2_21PreLabel591Decision(self, target, targetState) ||
                   AiUpdateLabel591Oid51_2_18_7Decision(self, target);
        }

        private bool AiUpdateSpecializedOidFollowUpChain(
            LF2Entity self,
            LF2Entity target,
            int targetState)
        {
            switch (self.ObjectId)
            {
                case 33:
                case 19:
                case 16:
                    return AiUpdateOid33_19_16PredictedDuaDecision(self, target, targetState);
                case 52:
                case 1:
                case 21:
                    return AiUpdateOid52_1_2_21PreLabel591Decision(self, target, targetState);
                case 2:
                    return AiUpdateOid52_1_2_21PreLabel591Decision(self, target, targetState) ||
                           AiUpdateLabel591Oid51_2_18_7Decision(self, target);
                case 51:
                case 18:
                case 7:
                    return AiUpdateLabel591Oid51_2_18_7Decision(self, target);
                default:
                    return false;
            }
        }

        private bool AiUpdateCloseOid1TailDecision(LF2Entity self, LF2Entity target)
        {
            if (!ForceLegacyAiOid1TailForDiagnostics)
            {
                int frame = Frame(self);
                if (frame < 260 || frame > 289)
                    return false;
            }

            return InvokeAiUpdateCloseOid1Decision(self, target);
        }

        private bool InvokeAiUpdateCloseOid1Decision(LF2Entity self, LF2Entity target)
        {
            if (CaptureAiOid1TailWitnessForDiagnostics && self?.ObjectId == 1)
                AiOid1CloseDecisionInvocationCountForDiagnostics++;
            return AiUpdateCloseOid1Decision(self, target);
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

        private void InvokeAiProcessSubCallerPrewrite(
            LF2Entity self,
            LF2Entity target,
            AiInputContext ai,
            int selfState,
            int targetState)
        {
            if (CaptureAiOid1TailWitnessForDiagnostics && self?.ObjectId == 1)
                AiOid1CallerPrewriteInvocationCountForDiagnostics++;
            AiProcessSubCallerPrewrite(self, target, ai, selfState, targetState);
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
        private enum LateOpointGuardOutcome
        {
            Unclassified = 0,
            NoRole = 1,
            InvalidFirst = 2,
            AttackingRejected = 3,
            CharacterFrameDelayRejected = 4,
            Eligible = 5,
        }

        internal static System.Func<SimulationWorld, LF2Entity, LF2Entity> RespawnEffectSpawnOverride;
        internal int LastCollisionPairVRestEligibilityVisitCount { get; private set; }
        private uint[] collisionSnapshotCapturedGenerationBySlot;
        private uint[] collisionSnapshotGenerationBySlot;
        private byte[] collisionSnapshotClaimedBySlot;
        private int collisionSnapshotProofTick = -1;
        private int collisionSnapshotProofSlotCapacity;
        private int collisionSnapshotProofClaimedCount;
        private ulong collisionSnapshotProofOccupancyEpoch;
        private bool collisionSnapshotProofComplete;
        public bool ForceLegacyLateRuntimeSnapshotsForDiagnostics { get; set; }
        public bool ForceLegacyLateRuntimeSlotLookupForDiagnostics { get; set; }
        public int LastLateFrameTickRuntimeSnapshotProofCountForDiagnostics { get; private set; }
        public int LastSkippedLateFrameTickRuntimeSnapshotCountForDiagnostics { get; private set; }
        public int LastSkippedLateDeathRuntimeSnapshotCountForDiagnostics { get; private set; }
        public int LastSkippedLateTailRuntimeSnapshotCountForDiagnostics { get; private set; }
        public int LastLateOpointNoRoleCountForDiagnostics { get; private set; }
        public int LastLateOpointInvalidFirstCountForDiagnostics { get; private set; }
        public int LastLateOpointAttackingRejectedCountForDiagnostics { get; private set; }
        public int LastLateOpointCharacterFrameDelayRejectedCountForDiagnostics { get; private set; }
        public int LastLateOpointEligibleCountForDiagnostics { get; private set; }
        public int LastLateOpointFactoryInvokedCountForDiagnostics { get; private set; }
        public bool ForceLegacyLateOpointFactoryGuardForDiagnostics { get; set; }
        public bool ForceLegacyPreInteractionForDiagnostics { get; set; }
        public int LastPreInteractionScannedCountForDiagnostics { get; private set; }
        public int LastPreInteractionExecutedCountForDiagnostics { get; private set; }
        public int LastPreInteractionProofSkipCountForDiagnostics { get; private set; }
        public int LastPreInteractionSnapshotSkipCountForDiagnostics { get; private set; }
        public int LastPreInteractionFailClosedCountForDiagnostics { get; private set; }
        public int LastPreInteractionCpointCheckProofSkipCountForDiagnostics { get; private set; }
        public int LastPreInteractionMismatchTailProofSkipCountForDiagnostics { get; private set; }
        public bool LastPreInteractionWholePassProofSucceededForDiagnostics { get; private set; }
        public int LastPreInteractionWholePassParticipantCountForDiagnostics { get; private set; }
        private readonly List<RuntimeEntityHandle> earlyState500Handles =
            new List<RuntimeEntityHandle>(16);
        private readonly List<RuntimeEntityHandle> earlyState501Handles =
            new List<RuntimeEntityHandle>(16);
        public bool ForceLegacyEarlyFrameAdvanceForDiagnostics { get; set; }
        public int LastEarlyTeleportRefreshCountForDiagnostics { get; private set; }
        public int LastEarlyTeleportSnapshotSkipCountForDiagnostics { get; private set; }
        public bool LastEarlyStateHandlePathUsedForDiagnostics { get; private set; }
        public int LastEarlyStateHandleFallbackCountForDiagnostics { get; private set; }

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
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            FlushQueuedObjectPointTasks(factory);
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
            bool skippedAiSnapshot = TrySkipAiSnapshotForCharacterInput();
            if (!skippedAiSnapshot)
                BuildAiInputSlotSnapshot();
            detailDiagnostics?.EndPhase(
                BattleTickDetailPhase.CharacterInputSnapshotBuild);
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.CharacterInputEntityInputPass);
            try
            {
                if (ForceLegacyCharacterInputPassForDiagnostics)
                {
                    CharacterInputLegacyPassCountForDiagnostics++;
                    RunDeferredMutationEntityPass(entity =>
                    {
                        RunCharacterInputEntity(
                            entity,
                            tickIndex,
                            aiDetailDiagnostics);
                    });
                }
                else
                {
                    CharacterInputDirectPassCountForDiagnostics++;
                    RunCharacterInputEntityPassDirect(
                        tickIndex,
                        aiDetailDiagnostics);
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputEntityInputPass);
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
                if (!skippedAiSnapshot)
                    ClearAiInputSlotSnapshot();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
            }
        }

        private void RunCharacterInputEntityPassDirect(
            int tickIndex,
            BattleAiInputDetailDiagnostics aiDetailDiagnostics)
        {
            _ticking = true;
            try
            {
                // Keep the same live ascending scan contract as
                // RunDeferredMutationEntityPass/ForEachEntityByRuntimeSlot.
                for (int runtimeSlot = 0;
                     runtimeSlot < RuntimeSlotCapacity;
                     runtimeSlot++)
                {
                    LF2Entity entity =
                        _runtimeSlots.GetCurrentOccupant(runtimeSlot);
                    if (entity == null || !IsActiveForCurrentPass(entity))
                        continue;

                    RunCharacterInputEntity(
                        entity,
                        tickIndex,
                        aiDetailDiagnostics);
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private void RunCharacterInputEntity(
            LF2Entity entity,
            int tickIndex,
            BattleAiInputDetailDiagnostics aiDetailDiagnostics)
        {
            if (entity.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                return;
            }

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
            LastEarlyTeleportRefreshCountForDiagnostics = 0;
            LastEarlyTeleportSnapshotSkipCountForDiagnostics = 0;
            LastEarlyStateHandlePathUsedForDiagnostics = false;
            LastEarlyStateHandleFallbackCountForDiagnostics = 0;

            if (ForceLegacyEarlyFrameAdvanceForDiagnostics)
            {
                RunEarlyFrameAdvanceSpecialsLegacy();
                return;
            }

            bool teleportGate = FrameToggle != 0;
            bool handleSnapshotValid = TryBuildEarlyStateHandleSnapshot(
                out ulong occupancyEpoch,
                out int logicalCapacity);
            if (!handleSnapshotValid)
            {
                // A partial slot-table proof cannot stand in for the authority's
                // complete active snapshot. Rebuild the exact legacy view before
                // running any entity callbacks.
                GetActiveEntitiesByRuntimeSlot(_entityScratch);
                earlyState500Handles.Clear();
                earlyState501Handles.Clear();
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                bool mutated =
                    entity.RunEarlyTeleportSpecialsPhaseWithMutationReport(
                        _entityScratch,
                        teleportGate);
                if (!IsActiveForCurrentPass(entity))
                    continue;
                if (mutated)
                {
                    LastEarlyTeleportRefreshCountForDiagnostics++;
                    RefreshRuntimeSnapshot(entity);
                }
                else
                {
                    LastEarlyTeleportSnapshotSkipCountForDiagnostics++;
                }
            }

            if (handleSnapshotValid &&
                ValidateEarlyStateHandleSnapshot(
                    occupancyEpoch,
                    logicalCapacity))
            {
                LastEarlyStateHandlePathUsedForDiagnostics = true;
                if (!RunEarlyStateHandles(
                        earlyState500Handles,
                        500,
                        occupancyEpoch,
                        logicalCapacity) ||
                    !RunEarlyStateHandles(
                        earlyState501Handles,
                        501,
                        occupancyEpoch,
                        logicalCapacity))
                {
                    LastEarlyStateHandlePathUsedForDiagnostics = false;
                    LastEarlyStateHandleFallbackCountForDiagnostics++;
                    RunEarlyState500Specials(_entityScratch);
                    RunEarlyState501Specials(_entityScratch);
                }
            }
            else
            {
                LastEarlyStateHandleFallbackCountForDiagnostics++;
                RunEarlyState500Specials(_entityScratch);
                RunEarlyState501Specials(_entityScratch);
            }

            earlyState500Handles.Clear();
            earlyState501Handles.Clear();
            _entityScratch.Clear();
        }

        private void RunEarlyFrameAdvanceSpecialsLegacy()
        {
            bool teleportGate = FrameToggle != 0;
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                entity.RunEarlyTeleportSpecialsPhase(
                    _entityScratch,
                    teleportGate);
                if (!IsActiveForCurrentPass(entity))
                    continue;
                LastEarlyTeleportRefreshCountForDiagnostics++;
                RefreshRuntimeSnapshot(entity);
            }

            RunEarlyState500Specials(_entityScratch);
            RunEarlyState501Specials(_entityScratch);
            _entityScratch.Clear();
        }

        private bool TryBuildEarlyStateHandleSnapshot(
            out ulong occupancyEpoch,
            out int logicalCapacity)
        {
            _entityScratch.Clear();
            earlyState500Handles.Clear();
            earlyState501Handles.Clear();

            occupancyEpoch = _runtimeSlots.OccupancyEpoch;
            logicalCapacity = _runtimeSlots.LogicalCapacity;
            for (int runtimeSlot = 0;
                 runtimeSlot < logicalCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return false;
                    continue;
                }

                LF2Entity entity = view.Entity;
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != runtimeSlot)
                {
                    return false;
                }

                var handle =
                    new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (!_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    return false;
                }

                if (!IsActiveForCurrentPass(entity))
                    continue;

                _entityScratch.Add(entity);
                int state = entity.Frame?.D?.state ?? -1;
                if (state == 500)
                    earlyState500Handles.Add(handle);
                else if (state == 501)
                    earlyState501Handles.Add(handle);
            }

            return occupancyEpoch == _runtimeSlots.OccupancyEpoch &&
                   logicalCapacity == _runtimeSlots.LogicalCapacity;
        }

        private bool ValidateEarlyStateHandleSnapshot(
            ulong occupancyEpoch,
            int logicalCapacity)
        {
            if (occupancyEpoch != _runtimeSlots.OccupancyEpoch ||
                logicalCapacity != _runtimeSlots.LogicalCapacity)
            {
                return false;
            }

            return ValidateEarlyStateHandles(earlyState500Handles, 500) &&
                   ValidateEarlyStateHandles(earlyState501Handles, 501) &&
                   occupancyEpoch == _runtimeSlots.OccupancyEpoch;
        }

        private bool ValidateEarlyStateHandles(
            List<RuntimeEntityHandle> handles,
            int expectedState)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                if (!TryResolveEarlyStateHandle(
                        handles[i],
                        expectedState,
                        out _))
                {
                    return false;
                }
            }

            return true;
        }

        private bool RunEarlyStateHandles(
            List<RuntimeEntityHandle> handles,
            int expectedState,
            ulong occupancyEpoch,
            int logicalCapacity)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                if (occupancyEpoch != _runtimeSlots.OccupancyEpoch ||
                    logicalCapacity != _runtimeSlots.LogicalCapacity ||
                    !TryResolveEarlyStateHandle(
                        handles[i],
                        expectedState,
                        out LF2Entity entity))
                {
                    return false;
                }

                if (expectedState == 500)
                    RunEarlyState500Special(entity);
                else
                    RunEarlyState501Special(entity, _entityScratch);
            }

            return occupancyEpoch == _runtimeSlots.OccupancyEpoch &&
                   logicalCapacity == _runtimeSlots.LogicalCapacity;
        }

        private bool TryResolveEarlyStateHandle(
            RuntimeEntityHandle handle,
            int expectedState,
            out LF2Entity entity)
        {
            entity = null;
            if (!handle.IsValid ||
                !_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                resolved == null ||
                resolved.Runtime == null ||
                resolved.Runtime.SlotIndex != handle.Slot ||
                !IsActiveForCurrentPass(resolved) ||
                resolved.Frame?.D?.state != expectedState)
            {
                return false;
            }

            entity = resolved;
            return true;
        }

        private void RunEarlyState500Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                RunEarlyState500Special(entity);
            }
        }

        private void RunEarlyState500Special(LF2Entity entity)
        {
            LF2FrameData frame = entity?.Frame?.D;
            if (frame == null || frame.state != 500)
                return;

            if (entity.TransformTargetObjectId == -1 ||
                entity.TransformOriginalObjectId >= 0)
            {
                // BMD-023: state=500 reset branch must mirror baseline SetFrameImmediate:
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);
            }
        }

        private void RunEarlyState501Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                RunEarlyState501Special(entity, entities);
            }
        }

        private void RunEarlyState501Special(
            LF2Entity entity,
            List<LF2Entity> activeEntities)
        {
            LF2FrameData frame = entity?.Frame?.D;
            if (frame == null ||
                frame.state != 501 ||
                entity.TransformTargetObjectId <= -1)
            {
                return;
            }

            LF2CharacterDataWrapper wrapper =
                LF2Entity.ResolveRuntimeCharacterConfig(
                    entity.TransformTargetObjectId);
            if (wrapper == null)
                return;

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
                return;

            for (int j = 0; j < activeEntities.Count; j++)
            {
                LF2Entity child = activeEntities[j];
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
                child.DirectWriteRawFramePreserveWaitCounter(
                    child.Runtime != null && child.Runtime.YInt < 0
                        ? 212
                        : 0);
                RefreshRuntimeSnapshot(child);
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
            PrepareCollisionSnapshotProofStorage();
            collisionSnapshotProofComplete = false;
            collisionSnapshotProofTick = -1;
            collisionSnapshotProofOccupancyEpoch = 0;
            System.Array.Clear(
                collisionSnapshotCapturedGenerationBySlot,
                0,
                collisionSnapshotCapturedGenerationBySlot.Length);
            bool captureFailed = false;
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
                int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                if (!_runtimeSlots.IsAddressable(runtimeSlot) ||
                    !_runtimeSlots.TryGetCurrentHandle(
                        runtimeSlot,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    captureFailed = true;
                    return;
                }

                collisionSnapshotCapturedGenerationBySlot[runtimeSlot] =
                    handle.Generation;
            });
            CaptureCollisionSnapshotProof(captureFailed);
        }

        private void PrepareCollisionSnapshotProofStorage()
        {
            int slotCapacity = RuntimeSlotCapacity;
            if (collisionSnapshotCapturedGenerationBySlot != null &&
                collisionSnapshotCapturedGenerationBySlot.Length == slotCapacity)
            {
                return;
            }

            collisionSnapshotCapturedGenerationBySlot = new uint[slotCapacity];
            collisionSnapshotGenerationBySlot = new uint[slotCapacity];
            collisionSnapshotClaimedBySlot = new byte[slotCapacity];
        }

        private void CaptureCollisionSnapshotProof(bool captureFailed)
        {
            int currentTick = CurrentTickIndex;
            int slotCapacity = RuntimeSlotCapacity;
            ulong occupancyEpoch = _runtimeSlots.OccupancyEpoch;
            int claimedCount = 0;
            bool complete = !captureFailed && occupancyEpoch != 0;

            for (int runtimeSlot = 0; runtimeSlot < slotCapacity; runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                collisionSnapshotGenerationBySlot[runtimeSlot] = view.Generation;
                collisionSnapshotClaimedBySlot[runtimeSlot] =
                    view.Claimed ? (byte)1 : (byte)0;
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        complete = false;
                    continue;
                }

                claimedCount++;
                LF2Entity entity = view.Entity;
                var handle = new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime?.SlotIndex != runtimeSlot ||
                    !_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    complete = false;
                    continue;
                }

                if (!IsActiveForCurrentPass(entity) ||
                    IsCollisionCandidateSuppressedForSnapshotProof(
                        entity,
                        currentTick))
                {
                    continue;
                }

                if (collisionSnapshotCapturedGenerationBySlot[runtimeSlot] !=
                    view.Generation)
                {
                    complete = false;
                }
            }

            if (_runtimeSlots.OccupancyEpoch != occupancyEpoch ||
                _runtimeSlots.ClaimedCount != claimedCount)
            {
                complete = false;
            }

            collisionSnapshotProofTick = currentTick;
            collisionSnapshotProofSlotCapacity = slotCapacity;
            collisionSnapshotProofClaimedCount = claimedCount;
            collisionSnapshotProofOccupancyEpoch = occupancyEpoch;
            collisionSnapshotProofComplete = complete;
        }

        internal bool TryPopulateFrozenZeroAttackItrParticipants(
            List<LF2Entity> destination)
        {
            if (destination == null)
                return false;

            destination.Clear();
            if (!collisionSnapshotProofComplete ||
                collisionSnapshotProofTick != CurrentTickIndex ||
                collisionSnapshotProofSlotCapacity != RuntimeSlotCapacity ||
                collisionSnapshotProofOccupancyEpoch == 0 ||
                collisionSnapshotProofOccupancyEpoch !=
                    _runtimeSlots.OccupancyEpoch ||
                collisionSnapshotProofClaimedCount !=
                    _runtimeSlots.ClaimedCount)
            {
                return false;
            }

            int currentTick = CurrentTickIndex;
            int claimedCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < collisionSnapshotProofSlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                bool expectedClaimed =
                    collisionSnapshotClaimedBySlot[runtimeSlot] != 0;
                if (view.Claimed != expectedClaimed ||
                    view.Generation !=
                        collisionSnapshotGenerationBySlot[runtimeSlot])
                {
                    destination.Clear();
                    return false;
                }

                if (!view.Claimed)
                {
                    if (view.Entity != null)
                    {
                        destination.Clear();
                        return false;
                    }
                    continue;
                }

                claimedCount++;
                LF2Entity entity = view.Entity;
                var handle = new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime?.SlotIndex != runtimeSlot ||
                    !_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    destination.Clear();
                    return false;
                }

                if (!IsActiveForCurrentPass(entity))
                    continue;

                destination.Add(entity);
                if (entity.PS == null ||
                    IsCollisionCandidateSuppressedForSnapshotProof(
                        entity,
                        currentTick))
                {
                    continue;
                }

                LF2FrameData currentFrame =
                    entity.Frame != null &&
                    entity.FrameCache?.HasFrame(entity.Frame.N) == true
                        ? entity.Frame.D
                        : null;
                LF2FrameData collisionFrame = entity.GetCollisionFrameData();
                if (HasReleaseAttackItr(currentFrame) &&
                    HasReleaseAttackItr(collisionFrame))
                {
                    destination.Clear();
                    return false;
                }

                if (collisionSnapshotCapturedGenerationBySlot[runtimeSlot] !=
                    view.Generation)
                {
                    destination.Clear();
                    return false;
                }
            }

            if (claimedCount != collisionSnapshotProofClaimedCount ||
                _runtimeSlots.OccupancyEpoch !=
                    collisionSnapshotProofOccupancyEpoch)
            {
                destination.Clear();
                return false;
            }

            return true;
        }

        private static bool IsCollisionCandidateSuppressedForSnapshotProof(
            LF2Entity entity,
            int currentTick)
        {
            int untilTick =
                entity?.Runtime?.SuppressCollisionCandidateUntilTick ?? 0;
            return untilTick > 0 && currentTick < untilTick;
        }

        private static bool HasReleaseAttackItr(LF2FrameData frame)
        {
            if (frame?.itrs == null)
                return false;

            for (int itrIndex = 0; itrIndex < frame.itrs.Count; itrIndex++)
            {
                if (frame.itrs[itrIndex] != null)
                    return true;
            }

            return false;
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
            bool captureDetailPhases = detailDiagnostics != null;

            void BeginDetailPhase(BattleTickDetailPhase phase)
            {
                if (captureDetailPhases)
                    detailDiagnostics.BeginPhase(phase);
            }

            void EndDetailPhase(BattleTickDetailPhase phase)
            {
                if (captureDetailPhases)
                    detailDiagnostics.EndPhase(phase);
            }

            LastLateFrameTickRuntimeSnapshotProofCountForDiagnostics = 0;
            LastSkippedLateFrameTickRuntimeSnapshotCountForDiagnostics = 0;
            LastSkippedLateDeathRuntimeSnapshotCountForDiagnostics = 0;
            LastSkippedLateTailRuntimeSnapshotCountForDiagnostics = 0;
            LastLateOpointNoRoleCountForDiagnostics = 0;
            LastLateOpointInvalidFirstCountForDiagnostics = 0;
            LastLateOpointAttackingRejectedCountForDiagnostics = 0;
            LastLateOpointCharacterFrameDelayRejectedCountForDiagnostics = 0;
            LastLateOpointEligibleCountForDiagnostics = 0;
            LastLateOpointFactoryInvokedCountForDiagnostics = 0;
            // The production object-point factory is pass-stable. Resolve it lazily so an
            // empty LateEntityUpdateAll invocation retains the existing no-auto-create behavior.
            LF2ObjectPointFactory opointFactory = null;
            bool opointFactoryResolved = false;
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < RuntimeSlotCapacity; runtimeSlot++)
                {
                    LF2Entity obj = ForceLegacyLateRuntimeSlotLookupForDiagnostics
                        ? FindEntityByRuntimeSlotCurrent(runtimeSlot)
                        : _runtimeSlots.GetCurrentOccupant(runtimeSlot);

                    if (obj == null)
                        continue;
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);
                    obj.RunStateSpecialPreCollision();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityStateSpecial);
                        continue;
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityRecovery);
                    obj.RunPreCollisionRecoveryPhase(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                    {
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityRecovery);
                        continue;
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityRecovery);

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);
                    bool frameTickSnapshotRefreshRequired = true;
                    if (obj.Runtime == null ||
                        tickIndex >= obj.Runtime.SuppressLateFrameTickUntilTick)
                    {
                        if (ForceLegacyLateRuntimeSnapshotsForDiagnostics)
                        {
                            obj.SimFrameTick(tickIndex);
                        }
                        else
                        {
                            frameTickSnapshotRefreshRequired =
                                obj.RunLateFrameTickAndReportSnapshotRefreshRequired(
                                    tickIndex,
                                    out bool snapshotProofEvaluated);
                            if (snapshotProofEvaluated)
                            {
                                LastLateFrameTickRuntimeSnapshotProofCountForDiagnostics++;
                            }
                        }
                    }
                    if (!IsActiveForCurrentPass(obj))
                    {
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityFrameTick);
                        continue;
                    }
                    if (!ForceLegacyLateRuntimeSnapshotsForDiagnostics &&
                        !frameTickSnapshotRefreshRequired)
                    {
                        LastSkippedLateFrameTickRuntimeSnapshotCountForDiagnostics++;
                    }
                    else
                    {
                        RefreshLateRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.FrameTick,
                            detailDiagnostics);
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityCollision);
                    obj.SimEntityCollision(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                    {
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityCollision);
                        continue;
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityCollision);

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);
                    bool exitedLateFrameTick = HandleLateFrameTickExit(obj);
                    if (exitedLateFrameTick)
                    {
                        if (obj is LF2SpecialAttack)
                            FlushQueuedObjectPointTasks(
                                ResolveLateObjectPointFactory(
                                    ref opointFactory,
                                    ref opointFactoryResolved));
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityFrameExit);
                        continue;
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);
                    obj.RunLateDeathOpointPreCleanupPhase();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityDeathOpoint);
                        continue;
                    }
                    if (CanSkipRedundantLateRuntimeSnapshot(obj))
                    {
                        LastSkippedLateDeathRuntimeSnapshotCountForDiagnostics++;
                    }
                    else
                    {
                        RefreshLateRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.DeathOpoint,
                            detailDiagnostics);
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);
                    ResolveLateObjectPointFactory(
                        ref opointFactory,
                        ref opointFactoryResolved);
                    LF2FrameData opointFrame = obj.Frame?.D;
                    bool frameHasOpoint = opointFrame != null &&
                        ((opointFrame.opoints != null && opointFrame.opoints.Count > 0) ||
                         opointFrame.opoint.HasValue);
                    LateOpointGuardOutcome opointGuardOutcome =
                        ObserveLateOpointGuardOutcome(
                            obj,
                            opointFrame,
                            frameHasOpoint);
                    bool invokeOpointFactory =
                        opointFactory != null &&
                        frameHasOpoint &&
                        (ForceLegacyLateOpointFactoryGuardForDiagnostics ||
                         opointGuardOutcome == LateOpointGuardOutcome.Eligible);
                    if (invokeOpointFactory)
                    {
                        LastLateOpointFactoryInvokedCountForDiagnostics++;
                        opointFactory.ProcessOpointSpawnAlignedToCpp(obj);
                    }
                    if (!IsActiveForCurrentPass(obj))
                    {
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityOpointProcess);
                        continue;
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    bool completedLateCleanup = obj.TryRunLatePostOpointCleanupPhase();
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    if (completedLateCleanup)
                    {
                        BeginDetailPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        FlushQueuedObjectPointTasks(
                            ResolveLateObjectPointFactory(
                                ref opointFactory,
                                ref opointFactoryResolved));
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    obj.RunLateTailBeforePrevFrame();
                    FlushQueuedObjectPointTasks(
                        ResolveLateObjectPointFactory(
                            ref opointFactory,
                            ref opointFactoryResolved));
                    if (!IsActiveForCurrentPass(obj))
                    {
                        EndDetailPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    if (CanSkipRedundantLateRuntimeSnapshot(obj))
                    {
                        LastSkippedLateTailRuntimeSnapshotCountForDiagnostics++;
                    }
                    else
                    {
                        RefreshLateRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.TailAndQueuedFlush,
                            detailDiagnostics);
                    }
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    BeginDetailPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                    obj.MirrorLatePrevFrame();
                    EndDetailPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                }
            }
            finally
            {
                _ticking = false;
                BeginDetailPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
                EndDetailPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
            }
        }

        private static LF2ObjectPointFactory ResolveLateObjectPointFactory(
            ref LF2ObjectPointFactory factory,
            ref bool resolved)
        {
            if (!resolved)
            {
                factory = LF2ObjectPointFactory.Instance;
                resolved = true;
            }

            return factory;
        }

        private static void FlushQueuedObjectPointTasks(LF2ObjectPointFactory factory)
        {
            if (factory != null && factory.HasQueuedTasks)
                factory.FlushTasks();
        }

        private bool CanSkipRedundantLateRuntimeSnapshot(LF2Entity entity)
        {
            return !ForceLegacyLateRuntimeSnapshotsForDiagnostics &&
                   entity != null &&
                   entity.GetType() == typeof(LF2Character);
        }

        private LateOpointGuardOutcome ObserveLateOpointGuardOutcome(
            LF2Entity spawner,
            LF2FrameData frame,
            bool frameHasOpoint)
        {
            if (spawner == null || spawner.PS == null)
                return LateOpointGuardOutcome.Unclassified;

            if (frame == null || !frameHasOpoint)
            {
                LastLateOpointNoRoleCountForDiagnostics++;
                return LateOpointGuardOutcome.NoRole;
            }

            bool hasList = frame.opoints != null && frame.opoints.Count > 0;
            ObjectPoint firstOp = hasList ? frame.opoints[0] : frame.opoint.Value;
            if (firstOp.kind <= 0 || firstOp.oid <= 0)
            {
                LastLateOpointInvalidFirstCountForDiagnostics++;
                return LateOpointGuardOutcome.InvalidFirst;
            }

            if (spawner.AttackingCounter != 0)
            {
                LastLateOpointAttackingRejectedCountForDiagnostics++;
                return LateOpointGuardOutcome.AttackingRejected;
            }

            if (spawner.FrameDelay != 0 &&
                spawner.GetCurrentDataObjectTypeForSimulation() ==
                (int)LF2ObjectType.Character)
            {
                LastLateOpointCharacterFrameDelayRejectedCountForDiagnostics++;
                return LateOpointGuardOutcome.CharacterFrameDelayRejected;
            }

            LastLateOpointEligibleCountForDiagnostics++;
            return LateOpointGuardOutcome.Eligible;
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
            LastPreInteractionScannedCountForDiagnostics = 0;
            LastPreInteractionExecutedCountForDiagnostics = 0;
            LastPreInteractionProofSkipCountForDiagnostics = 0;
            LastPreInteractionSnapshotSkipCountForDiagnostics = 0;
            LastPreInteractionFailClosedCountForDiagnostics = 0;
            LastPreInteractionCpointCheckProofSkipCountForDiagnostics = 0;
            LastPreInteractionMismatchTailProofSkipCountForDiagnostics = 0;
            LastPreInteractionWholePassProofSucceededForDiagnostics = false;
            LastPreInteractionWholePassParticipantCountForDiagnostics = 0;

            _ticking = true;
            try
            {
                if (!ForceLegacyPreInteractionForDiagnostics &&
                    TryProveWholePreInteractionPassNoOp(
                        tickIndex,
                        out int participantCount))
                {
                    LastPreInteractionWholePassProofSucceededForDiagnostics = true;
                    LastPreInteractionWholePassParticipantCountForDiagnostics =
                        participantCount;
                    LastPreInteractionScannedCountForDiagnostics = participantCount;
                    LastPreInteractionProofSkipCountForDiagnostics =
                        participantCount * 3;
                    LastPreInteractionSnapshotSkipCountForDiagnostics =
                        participantCount * 3;
                    LastPreInteractionCpointCheckProofSkipCountForDiagnostics =
                        participantCount;
                    LastPreInteractionMismatchTailProofSkipCountForDiagnostics =
                        participantCount;
                    return;
                }

                GetActiveEntitiesByRuntimeSlot(_entityScratch);
                if (_entityScratch.Count == 0) return;

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
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

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunCpointMismatchTailStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                _entityScratch.Clear();

                // Keep the authority live ascending scan without allocating a
                // tick-capturing delegate. Newborns above the cursor join this
                // pass, while a recycled lower slot waits for the next pass.
                for (int runtimeSlot = 0;
                     runtimeSlot < _runtimeSlots.LogicalCapacity;
                     runtimeSlot++)
                {
                    LF2Entity entity = _runtimeSlots.GetCurrentOccupant(runtimeSlot);
                    if (entity == null)
                        continue;
                    if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunWeaponSyncHeldStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }
            }
            finally
            {
                _entityScratch.Clear();
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private bool TryProveWholePreInteractionPassNoOp(
            int tickIndex,
            out int participantCount)
        {
            participantCount = 0;
            int logicalCapacity = _runtimeSlots.LogicalCapacity;
            int claimedCount = _runtimeSlots.ClaimedCount;
            ulong occupancyEpoch = _runtimeSlots.OccupancyEpoch;
            long pendingDestroyEpoch =
                NTSDEntityRuntime.PendingFlushDestroyMutationEpochForDiagnostics;
            int pendingUnregisterCount = _pendingUnregister.Count;

            for (int runtimeSlot = 0; runtimeSlot < logicalCapacity; runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return false;
                    continue;
                }

                LF2Entity entity = view.Entity;
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != runtimeSlot)
                {
                    return false;
                }

                var handle =
                    new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (!_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    return false;
                }

                if (entity.Runtime.SuppressPreInteractionUntilTick > tickIndex ||
                    !IsActiveForCurrentPass(entity))
                {
                    continue;
                }

                participantCount++;
                if (!TryProveNeutralPreInteractionParticipant(entity))
                    return false;
            }

            return logicalCapacity == _runtimeSlots.LogicalCapacity &&
                   claimedCount == _runtimeSlots.ClaimedCount &&
                   occupancyEpoch == _runtimeSlots.OccupancyEpoch &&
                   pendingDestroyEpoch ==
                       NTSDEntityRuntime
                           .PendingFlushDestroyMutationEpochForDiagnostics &&
                   pendingUnregisterCount == _pendingUnregister.Count;
        }

        private static bool TryProveNeutralPreInteractionParticipant(
            LF2Entity entity)
        {
            if (entity == null || entity.GetType() != typeof(LF2Character))
                return false;
            if (!entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
                return false;

            var character = (LF2Character)entity;
            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime.LinkState != 0 ||
                runtime.TargetSlotIndex != -1 ||
                runtime.HeldWeaponStableId != -1 ||
                character.HeldWeaponReferenceInternal != null)
            {
                return false;
            }

            CatchPoint collisionCpoint =
                character.GetCollisionFrameData()?.cpoint;
            if (collisionCpoint != null &&
                (collisionCpoint.kind == 1 || collisionCpoint.kind == 2))
            {
                return false;
            }

            CatchPoint currentCpoint = character.Frame?.D?.cpoint;
            return currentCpoint == null ||
                   (currentCpoint.kind != 1 && currentCpoint.kind != 2);
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

        internal string CaptureLateRuntimeSlotLookupBoundaryForSelfCheck(
            bool forceLegacyLookup,
            bool enableDetailDiagnostics)
        {
            const int inactiveSlot = DynamicRuntimeSlotStart;
            const int releasedSlot = DynamicRuntimeSlotStart + 1;
            const int spawnerSlot = DynamicRuntimeSlotStart + 2;
            const int higherBirthSlot = DynamicRuntimeSlotStart + 3;
            const int pendingSlot = DynamicRuntimeSlotStart + 4;
            var visitOrder = new List<int>();
            var inactive = new LateRuntimeSlotLookupProbe(1, visitOrder);
            var released = new LateRuntimeSlotLookupProbe(2, visitOrder);
            var spawner = new LateRuntimeSlotLookupProbe(3, visitOrder);
            var lowerReuse = new LateRuntimeSlotLookupProbe(4, visitOrder);
            var higherBirth = new LateRuntimeSlotLookupProbe(5, visitOrder);
            var pending = new LateRuntimeSlotLookupProbe(6, visitOrder);

            ForceLegacyLateRuntimeSlotLookupForDiagnostics = forceLegacyLookup;
            inactive.SetRequiredRuntimeSlot(inactiveSlot);
            released.SetRequiredRuntimeSlot(releasedSlot);
            spawner.SetRequiredRuntimeSlot(spawnerSlot);
            pending.SetRequiredRuntimeSlot(pendingSlot);
            Register(inactive);
            Register(released);
            Register(spawner);
            Register(pending);
            inactive.Runtime.OidMergeDormant = true;
            pending.Runtime.PendingFlushDestroy = true;

            released.SetAction(() => Unregister(released));
            spawner.SetAction(() =>
            {
                lowerReuse.SetRequiredRuntimeSlot(releasedSlot);
                higherBirth.SetRequiredRuntimeSlot(higherBirthSlot);
                Register(lowerReuse);
                Register(higherBirth);
            });
            pending.SetFreeAction(() => Unregister(pending));

            BattleTickDetailPhaseDiagnostics diagnostics = null;
            if (enableDetailDiagnostics)
            {
                diagnostics = EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
                diagnostics.BeginTick(1);
            }

            LateEntityUpdateAll(1);
            BattleParityFrameSnapshot snapshot = CaptureParityFrameSnapshot(1);
            return string.Join(
                "|",
                string.Join(",", visitOrder),
                snapshot.OverallChecksum,
                ObjectCount,
                lowerReuse.StateSpecialCount,
                inactive.StateSpecialCount,
                pending.StateSpecialCount,
                pending.FreeCount,
                pending.Runtime.SlotIndex,
                diagnostics?.LastTickIndex ?? -1);
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

        private sealed class LateRuntimeSlotLookupProbe : LF2Entity
        {
            private readonly int traceId;
            private readonly List<int> visitOrder;
            private System.Action action;
            private System.Action freeAction;

            internal int StateSpecialCount { get; private set; }
            internal int FreeCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum =>
                LF2ObjectType.Character;
            internal override bool UsesDynamicRuntimeSlot() => true;

            internal LateRuntimeSlotLookupProbe(int traceId, List<int> visitOrder)
            {
                this.traceId = traceId;
                this.visitOrder = visitOrder;
                Name = "LateRuntimeSlotLookupProbe" + traceId;
                ObjectId = traceId;
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

            internal void SetAction(System.Action value)
            {
                action = value;
            }

            internal void SetFreeAction(System.Action value)
            {
                freeAction = value;
            }

            public override void RunStateSpecialPreCollision()
            {
                StateSpecialCount++;
                visitOrder.Add(traceId);
                action?.Invoke();
            }

            internal override void RunPreCollisionRecoveryPhase(int tickIndex)
            {
            }

            public override void SimFrameTick(int tickIndex)
            {
            }

            public override void SimEntityCollision(int tickIndex)
            {
            }

            internal override void RunLateDeathOpointPreCleanupPhase()
            {
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                return false;
            }

            internal override void RunLateTailBeforePrevFrame()
            {
            }

            public override void MirrorLatePrevFrame()
            {
            }

            public override void FreeEntityLikeExe()
            {
                FreeCount++;
                freeAction?.Invoke();
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


--- File: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Input;
using NTSD.Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 所有战斗实体的最底层公共基类。
    /// 
    /// 你可以把它理解成“所有战斗对象共享的骨架”：
    /// 1. 统一持有 Runtime、Frame、Effect、Renderer 等核心数据。
    /// 2. 定义所有实体都可能会参与的生命周期入口。
    /// 3. 让角色、武器、技能体、特效体可以共享同一套基础框架。
    /// 
    /// 简单理解项目分层：
    /// - LF2Entity：最底层实体框架
    /// - LF2LivingObject：更像战斗单位的公共能力
    /// - LF2Character / LF2WeaponBase / LF2SpecialAttack：具体对象类型
    /// </summary>
    public abstract class LF2Entity : ILF2Entity
    {
        protected static readonly List<LF2Entity> N30HistoryGateScratch = new List<LF2Entity>(32);
        private readonly NTSDInputStateModule sharedCharacterDatInputModule = new NTSDInputStateModule();
        private int requiredRuntimeSlot = -1;
        internal static System.Func<int, LF2CharacterDataWrapper> RuntimeCharacterConfigResolverOverride;


        /// <summary>对象名称。</summary>
        public string Name { get; set; }

        /// <summary>实体稳定 ID。</summary>
        public int StableId
        {
            get => Runtime.StableId;
            protected set => Runtime.StableId = value;
        }

        /// <summary>对象 ID。</summary>
        public int ObjectId
        {
            get => Runtime.ObjectId;
            set => Runtime.ObjectId = value;
        }

        /// <summary>队伍 ID。</summary>
        public int Team
        {
            get => Runtime.Team;
            set => Runtime.Team = value;
        }

        public virtual int RelationTeam
        {
            get => Runtime.RelationTeam;
            set => Runtime.RelationTeam = value;
        }

        /// <summary>生成者 StableId；-1 表示无生成者。</summary>
        public int OwnerId
        {
            get => Runtime.OwnerStableId;
            set => Runtime.OwnerStableId = value;
        }

        /// <summary>被抓取状态。</summary>
        public int GrabbedBy
        {
            get => Runtime.GrabbedBy;
            set => Runtime.GrabbedBy = value;
        }

        /// <summary>kind==2 的 tracker 标记。</summary>
        public int TrackerFlag
        {
            get => Runtime.TrackerFlag;
            set => Runtime.TrackerFlag = value;
        }

        /// <summary>kind==2 的 tracker 父对象引用。</summary>
        public LF2Entity TrackerParent { get; set; }

        /// <summary>当前命中的 itr 槽位索引，用于 spark 计时。</summary>
        public int CurrentItrIndex { get; set; }

        /// <summary>对象类型整数值，由子类 ObjectTypeEnum 决定。</summary>
        public int ObjectType => (int)ObjectTypeEnum;

        /// <summary>对象类型枚举，由子类实现。</summary>
        public abstract LF2ObjectType ObjectTypeEnum { get; }

        /// <summary>
        /// 逻辑真值运行时。
        /// 大部分真正参与战斗结算的位置、速度、状态字段，最终都应该落在这里。
        /// </summary>
        public NTSDEntityRuntime Runtime { get; } = new NTSDEntityRuntime();

        public PhysicsState PS { get; protected set; } = new PhysicsState();

        private static readonly DeterministicRng FallbackRng = new DeterministicRng(0x4E545344u);

        /// <summary>C++ release 实体类型值。</summary>
        public virtual int ReleaseEntityType => ObjectType;

        public virtual bool CountsAsRandomWeaponDropCandidate()
            => GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character;

        internal int RequiredRuntimeSlot => requiredRuntimeSlot;

        public void SetRequiredRuntimeSlot(int runtimeSlot)
        {
            requiredRuntimeSlot = runtimeSlot;
        }

        internal void ClearRequiredRuntimeSlot()
        {
            requiredRuntimeSlot = -1;
        }

        /// <summary>当前对象正在执行哪一帧逻辑，以及上一帧/碰撞快照帧等辅助信息。</summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>当前对象对应的 DAT 帧数据缓存。</summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        /// <summary>帧切换控制器。负责 wait/next/frame jump 等帧推进细节。</summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>效果状态。</summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>Sprite 资源引用。</summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>渲染器引用。</summary>
        public LF2ObjectRenderer Renderer { get; protected set; }

        /// <summary>成功注册后所属的战斗世界。</summary>
        private SimulationWorld registeredWorld;

        public SimulationWorld Match => registeredWorld ?? SimulationTickDriver.Instance?.World;



        /// <summary>帧延迟计数器。大于 0 或小于 0 时，都会影响本帧是否真正推进。</summary>
        public int FrameDelay
        {
            get => Runtime.FrameDelay;
            set => Runtime.FrameDelay = value;
        }

        /// <summary>投掷后的同帧保护帧号，命中当前 frame 时直接跳过 frame advance / frame tick。</summary>
        public int ThrowFrameGuard
        {
            get => Runtime.ThrowFrameGuard;
            set => Runtime.ThrowFrameGuard = value;
        }

        /// <summary>C++ release Entity::attacking，帧等待/攻击状态计数器。</summary>
        public int AttackingCounter
        {
            get => Runtime.AttackingCounter;
            set => Runtime.AttackingCounter = value;
        }

        /// <summary>命中停帧/锁定计数。可以理解成“这一小段时间内对象被短暂停住”。</summary>
        public int HitStun
        {
            get => Runtime.HitStop;
            set => Runtime.HitStop = value;
        }

        /// <summary>累计击退 X 速度。</summary>
        public double KnockbackVx
        {
            get => Runtime.KnockbackVx;
            set => Runtime.KnockbackVx = value;
        }

        /// <summary>累计击退 Y 速度。</summary>
        public double KnockbackVy
        {
            get => Runtime.KnockbackVy;
            set => Runtime.KnockbackVy = value;
        }

        /// <summary>累计击退 Z 速度。</summary>
        public double KnockbackVz
        {
            get => Runtime.KnockbackVz;
            set => Runtime.KnockbackVz = value;
        }

        /// <summary>震屏计时器。</summary>
        public int ShakeTimer
        {
            get => Runtime.ShakeTimer;
            set => Runtime.ShakeTimer = value;
        }

        /// <summary>攻击豁免计数器；角色类改用 HitCounters 存储。</summary>
        public virtual int AttackExempt
        {
            get => Runtime.AttackExempt;
            set => Runtime.AttackExempt = value;
        }

        public virtual int HitStateCount
        {
            get => Runtime.HitStateCount;
            set => Runtime.HitStateCount = value;
        }

        public virtual int HitConfirmCounter
        {
            get => Runtime.HitConfirmEa;
            set => Runtime.HitConfirmEa = value;
        }

        /// <summary>生成者实体索引，opoint 生成时写入。</summary>
        public int OwnerEntityIndex
        {
            get => Runtime.OwnerSlotIndex;
            set => Runtime.OwnerSlotIndex = value;
        }

        /// <summary>发射/生成计数。</summary>
        public int ShotCount
        {
            get => Runtime.ShotCount;
            set => Runtime.ShotCount = value;
        }

        /// <summary>C++ release ai_controlled 标记；角色生成后由输入准备阶段消费。</summary>
        public bool AiControlled
        {
            get => Runtime.AiControlled;
            set => Runtime.AiControlled = value;
        }

        /// <summary>itr 攻击冷却跟踪器。</summary>
        public virtual LF2ItrRestTracker ItrRest { get; protected set; } = null;

        /// <summary>生命和资源状态。</summary>
        public virtual LF2Health Health { get; protected set; } = null;

        /// <summary>HP 恢复计时器。</summary>
        public virtual int HealTimer
        {
            get => Runtime.HealTimer;
            set => Runtime.HealTimer = value;
        }

        public virtual int CatchTimer
        {
            get => Runtime.CatchTimer;
            set => Runtime.CatchTimer = value;
        }

        /// <summary>C++ release kill_count；-1 表示普通实体，&gt;=0 表示关联的生成者/归属槽。</summary>
        public int KillCount
        {
            get => Runtime.KillCount;
            set => Runtime.KillCount = value;
        }

        /// <summary>C++ release combo_count_vic；累计承受的连击伤害统计。</summary>
        public int ComboCountVic
        {
            get => Runtime.ComboCountVic;
            set => Runtime.ComboCountVic = value;
        }

        /// <summary>C++ release combo_count_atk；累计造成的连击伤害统计。</summary>
        public int ComboCountAtk
        {
            get => Runtime.ComboCountAtk;
            set => Runtime.ComboCountAtk = value;
        }

        /// <summary>C++ release kill_stat；击杀统计。</summary>
        public int KillStat
        {
            get => Runtime.KillStat;
            set => Runtime.KillStat = value;
        }

        /// <summary>C# authority Entity.Unk344；索引 1..2 指向全局击杀/伤害统计槽。</summary>
        public int Unk344
        {
            get => Runtime.Unk344;
            set => Runtime.Unk344 = value;
        }

        /// <summary>C++ release weapon_count；角色受笛子命中时可为负，武器侧用于飞行/笛子累计。</summary>
        public int WeaponCount
        {
            get => Runtime.WeaponCount;
            set => Runtime.WeaponCount = value;
        }

        /// <summary>C++ release fall_damage_div；落地持续扣血分支的伤害缩放除数。</summary>
        public int FallDamageDiv
        {
            get => Runtime.FallDamageDiv;
            set => Runtime.FallDamageDiv = value;
        }

        /// <summary>C++ release 原始 HP 备份字段。</summary>
        public int HPOrig
        {
            get => Runtime.HPOrig;
            set => Runtime.HPOrig = value;
        }

        /// <summary>C++ release 原始 HP2/残机备份字段。</summary>
        public int HP2Orig
        {
            get => Runtime.HP2Orig;
            set => Runtime.HP2Orig = value;
        }

        /// <summary>C++ release 复活血量配置字段；0 表示走普通复活次数路径。</summary>
        public int RespawnCount
        {
            get => Runtime.RespawnCount;
            set => Runtime.RespawnCount = value;
        }

        /// <summary>C# 基线 presentation `PpDisplay`；输入扣费与帧推进回退维护的 PP 表现层累计面。</summary>
        public int PpDisplay
        {
            get => Runtime.PpDisplay;
            set => Runtime.PpDisplay = value;
        }

        protected bool IsPpModeEnabled()
        {
            return Match?.PpMode ?? NTSDGlobal.MPEnabled;
        }

        public int HitCount
        {
            get => Runtime.HitCount;
            set => Runtime.HitCount = value;
        }

        public int HitConfirm2
        {
            get => Runtime.HitConfirm2;
            set => Runtime.HitConfirm2 = value;
        }

        public virtual int FallCounter
        {
            get => Runtime.Fall;
            set => Runtime.Fall = value;
        }

        public int TransformOriginalObjectId
        {
            get => Runtime.TransformOriginalObjectId;
            set => Runtime.TransformOriginalObjectId = value;
        }

        public int TransformTargetObjectId
        {
            get => Runtime.TransformTargetObjectId;
            set => Runtime.TransformTargetObjectId = value;
        }

        public int CaughtSlotIndex
        {
            get => Runtime.CaughtSlotIndex;
            set => Runtime.CaughtSlotIndex = value;
        }

        public int CatcherSlotIndex
        {
            get => Runtime.CatcherSlotIndex;
            set => Runtime.CatcherSlotIndex = value;
        }

        public int HolderCopySlot
        {
            get => Runtime.HolderCopySlotIndex;
            set => Runtime.HolderCopySlotIndex = value;
        }

        public int RelationOwnerSlot
        {
            get => Runtime.RelationOwnerSlotIndex;
            set => Runtime.RelationOwnerSlotIndex = value;
        }

        public int SpawnerEntityIndex
        {
            get => Runtime.SpawnerSlotIndex;
            set => Runtime.SpawnerSlotIndex = value;
        }

        private bool _hasForcedRuntimeIntPosition;
        internal bool HasForcedRuntimeIntPositionForDiagnostics =>
            _hasForcedRuntimeIntPosition;



        /// <summary>可选的旧版阴影 SpriteRenderer，由渲染适配器注入。</summary>
        public SpriteRenderer ShadowRenderer { get; private set; }

        /// <summary>注入可选的旧版阴影渲染器引用。</summary>
        public void SetShadowRenderer(SpriteRenderer sr)
        {
            ShadowRenderer = sr;
            Sprite?.InitializeShadow(sr);
        }

        /// <summary>更新阴影位置和显示状态。</summary>
        public void UpdateShadow(int renderFrame = 0)
        {
            if (Runtime == null) return;

            bool hide = ShouldHideShadowForPresentation(
                Frame?.D,
                Runtime.LinkState,
                ObjectId,
                Runtime.HitStop);

            if (hide)
                Sprite?.HideShadow();
            else
                Sprite?.ShowShadow();

            if (ShadowRenderer == null)
                return;

            // A sorting layer wins over sortingOrder in Unity. Keep shadows in the
            // same layer as entities and sparks so the compact presentation order
            // can interleave Shadow(A), Entity(A), Shadow(B), Entity(B).
            ShadowRenderer.sortingLayerName = "Object";
            if (Sprite == null)
                ShadowRenderer.enabled = !hide;
            if (!hide)
            {
                ShadowRenderer.sortingOrder = GetPresentationRenderSortingOrder(
                    SimulationWorld.PresentationShadowSubOrder);
                var t = ShadowRenderer.transform;

                // C# 基准工程先计算阴影绘制矩形：
                // left = x + renderOffsetX - cameraX - shadowW / 2
                // top  = z - shadowH / 2
                // Unity shadow uses a center pivot, so converting the rect back
                // to its center cancels shadowW/shadowH exactly. Keep this fixed
                // center-pivot contract independent of runtime Sprite metrics.
                int cameraX = Match?.ReleaseCameraX ?? 0;
                int renderOffsetX = (int)GetRenderOffsetX();
                float shadowCenterX = GetRuntimeXInt() + renderOffsetX - cameraX;
                float shadowCenterY = GetRenderZInt();
                Vector3 worldPos = NTSDRenderSpace.ScreenPixelToWorld(shadowCenterX, shadowCenterY, t.position.z);
                t.position = NTSDRenderSpace.SnapWorldPosition(worldPos);
            }

            Match?.RecordLegacyShadowProbe(this, ShadowRenderer);
        }

        internal void UpdateShadowManagedState()
        {
            if (Runtime == null)
                return;

            bool hide = ShouldHideShadowForPresentation(
                Frame?.D,
                Runtime.LinkState,
                ObjectId,
                Runtime.HitStop);
            Sprite?.SetShadowVisibleManagedOnly(!hide);
        }

        internal static bool ShouldHideShadowForPresentation(
            LF2FrameData currentFrame,
            int linkState,
            int objectId,
            int hitStop)
        {
            int state = currentFrame?.state ?? -1;
            return currentFrame == null
                || state == 3005
                || state == 9997
                || linkState < 0
                || objectId == 223
                || objectId == 224
                || !LF2ObjectRenderer.ShouldDrawShadowForHitStop(hitStop);
        }



        /// <summary>命中记录数量，对齐 C# 基线 Entity.HitRecordCount。</summary>
        public int HitRecordCount { get; private set; } = 0;

        /// <summary>最大命中记录数量，对齐 C# 基线的 10 槽。</summary>
        public const int MaxHitRecordSlots = 10;

        private readonly int[] _hitRecordDamage = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordX = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordZ = new int[MaxHitRecordSlots];
        private readonly int[] _hitRecordLastAdvanceTick = new int[MaxHitRecordSlots];

        /// <summary>追加一条命中记录，供 SparkRenderer 按 C# 基线渲染。</summary>
        public void AddHitRecord(int age, int anchorX, int anchorZ)
        {
            if (HitRecordCount >= MaxHitRecordSlots)
                return;

            int slot = HitRecordCount++;
            _hitRecordDamage[slot] = age;
            _hitRecordX[slot] = anchorX;
            _hitRecordZ[slot] = anchorZ;
            _hitRecordLastAdvanceTick[slot] = int.MinValue;
        }

        /// <summary>记录一次 kind 0 命中；由受击对象调用。</summary>
        internal void RecordKind0Hit(LF2Entity attacker, InteractionArea itr)
        {
            if (attacker == null || itr == null)
                return;

            int attackerZ = attacker.Runtime.ZInt;
            int victimZ = Runtime.ZInt;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int victimSlot = Runtime.SlotIndex;
            LF2Entity recordOwner = attackerZ > victimZ ||
                                    (attackerZ == victimZ && attackerSlot > victimSlot)
                ? attacker
                : this;

            if (recordOwner.HitRecordCount >= MaxHitRecordSlots)
                return;

            int sparkPhase = itr.effect == 1 ? 1 : 0;
            int timer = itr.fall > 60 ? sparkPhase * 20 : sparkPhase * 20 + 10;
            LF2FrameData attackerFrame = attacker.GetFrameDataById(attacker.Frame?.N ?? 0) ?? attacker.Frame?.D;
            int attackerCenterX = attackerFrame?.centerx ?? 0;
            int attackerCenterY = attackerFrame?.centery ?? 0;
            int attackerX = attacker.Runtime.XInt;
            int attackerY = attacker.Runtime.YInt;
            int victimX = Runtime.XInt;
            int victimY = Runtime.YInt;

            int hitX;
            if (attacker.Dirh() > 0)
            {
                hitX = attackerX - attackerCenterX + itr.x + itr.w;
                if (hitX > victimX)
                    hitX = victimX;
            }
            else
            {
                hitX = attackerX + attackerCenterX - itr.x - itr.w;
                if (hitX < victimX)
                    hitX = victimX;
            }

            int hitYOffset = attackerY + (itr.h / 2) + itr.y - attackerCenterY;
            int lowerY = victimY - attackerCenterY;
            if (hitYOffset < lowerY)
                hitYOffset = (lowerY + hitYOffset) >> 1;
            else if (hitYOffset > victimY)
                hitYOffset = (victimY + hitYOffset) >> 1;

            int hitZ = attackerZ + hitYOffset + BattleRandInt(0, 9) - 4;
            hitX += BattleRandInt(0, 9) - 4;
            recordOwner.AddHitRecord(timer, hitX, hitZ);
        }

        /// <summary>读取指定命中记录年龄。</summary>
        public int GetHitRecordAge(int slotIndex) => _hitRecordDamage[slotIndex];

        /// <summary>读取指定命中记录 X 锚点。</summary>
        public int GetHitRecordX(int slotIndex) => _hitRecordX[slotIndex];

        /// <summary>读取指定命中记录 Z 锚点。</summary>
        public int GetHitRecordZ(int slotIndex) => _hitRecordZ[slotIndex];

        /// <summary>命中记录成功渲染后推进年龄。</summary>
        public void AdvanceHitRecord(int slotIndex, int tickIndex)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount)
                return;

            if (_hitRecordLastAdvanceTick[slotIndex] == tickIndex)
                return;

            _hitRecordDamage[slotIndex]++;
            _hitRecordLastAdvanceTick[slotIndex] = tickIndex;
        }

        internal void AdvanceHitRecordFromPresentation(int slotIndex, int expectedAge)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount ||
                _hitRecordDamage[slotIndex] != expectedAge)
            {
                return;
            }

            _hitRecordDamage[slotIndex]++;
        }

        internal bool RemoveHitRecordTailFromPresentation(
            int slotIndex,
            int expectedCount,
            int expectedAge)
        {
            if (HitRecordCount != expectedCount ||
                slotIndex != HitRecordCount - 1 ||
                slotIndex < 0 ||
                _hitRecordDamage[slotIndex] != expectedAge)
            {
                return false;
            }

            RemoveHitRecord(slotIndex);
            return true;
        }

        /// <summary>仅当该记录位于尾槽时移除，对齐 C# 基线尾槽回收规则。</summary>
        public bool RemoveHitRecordIfTail(int slotIndex)
        {
            if (slotIndex != HitRecordCount - 1)
                return false;

            RemoveHitRecord(slotIndex);
            return true;
        }

        private void RemoveHitRecord(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= HitRecordCount)
                return;

            int tail = HitRecordCount - 1;
            if (slotIndex < tail)
            {
                System.Array.Copy(_hitRecordDamage, slotIndex + 1, _hitRecordDamage, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordX, slotIndex + 1, _hitRecordX, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordZ, slotIndex + 1, _hitRecordZ, slotIndex, tail - slotIndex);
                System.Array.Copy(_hitRecordLastAdvanceTick, slotIndex + 1, _hitRecordLastAdvanceTick, slotIndex, tail - slotIndex);
            }

            _hitRecordDamage[tail] = 0;
            _hitRecordX[tail] = 0;
            _hitRecordZ[tail] = 0;
            _hitRecordLastAdvanceTick[tail] = 0;
            HitRecordCount--;
        }

        protected void ResetSpark()
        {
            HitRecordCount = 0;
            System.Array.Clear(_hitRecordDamage, 0, _hitRecordDamage.Length);
            System.Array.Clear(_hitRecordX, 0, _hitRecordX.Length);
            System.Array.Clear(_hitRecordZ, 0, _hitRecordZ.Length);
            System.Array.Clear(_hitRecordLastAdvanceTick, 0, _hitRecordLastAdvanceTick.Length);
        }



        /// <summary>Unity 保留的状态事件入口；具体行为以 C++ release 运行时为准。</summary>
        protected virtual bool StateExitEvent() => false;
        protected virtual bool StateEntryEvent() => false;
        protected virtual bool FrameEvent() => false;
        protected virtual bool TransitEvent() => false;
        protected virtual bool TUEvent() => false;
        protected virtual bool DieEvent() => false;
        protected virtual bool DestroyEvent() => false;

        /// <summary>获取当前状态。</summary>
        public virtual int GetState() => Frame.D?.state ?? 0;

        public virtual void SwitchDir(string dir)
        {
            string nextDir = dir == "left" ? "left" : "right";
            Runtime.Dir = nextDir;
            if (PS != null)
                PS.dir = nextDir;
            Sprite?.SwitchLR(nextDir);
        }

        public virtual int Dirh() => Runtime.Dir == "left" ? -1 : 1;

        public virtual int Dirv() => 1;

        protected virtual string CalculateDirection(int facing, string parentDir)
        {
            int face = facing >= 20 ? facing % 10 : facing;
            if (face == 0) return parentDir;
            if (face == 1) return parentDir == "right" ? "left" : "right";
            if (face >= 2 && face <= 10) return "right";
            if (face >= 11 && face <= 19) return "left";
            return parentDir;
        }



        /// <summary>受到 itr kind=10/11 时的受力处理，角色和武器共用。</summary>
        public virtual void FluteForce()
        {
            if (Runtime == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);

            float lowLevel = -140f;
            float midLevel = -160f;
            float highLevel = -180f;

            Effect.Super = true;
            Runtime.Vx = 0;
            Runtime.Vz = 0;

            if (Runtime.Y > lowLevel)
                Runtime.Vy = (Runtime.Vy <= 0) ? -7.5f : -Runtime.Vy / 2f;
            else if (Runtime.Y <= lowLevel && Runtime.Y > midLevel)
                Runtime.Vy -= mass / 2f;
            else if (Runtime.Y <= midLevel && Runtime.Y > highLevel)
                Runtime.Vy += mass / 2f;

            switch ((LF2ObjectType)GetCurrentDataObjectType())
            {
                case LF2ObjectType.Character:
                    if (Frame.N >= 55) ImmediateFrame(40);
                    break;
                case LF2ObjectType.HeavyWeapon:
                    if (Frame.N >= 5) ImmediateFrame(1);
                    break;
            }
        }



        /// <summary>写入实体位置。</summary>
        public void SetPos(double x, double y, double z)
        {
            Runtime.SetPosition(x, y, z);
        }

        /// <summary>创建武器破碎碎片特效。</summary>
        public virtual void BrokenEffectCreate(int id, int num = 8)
        {
            SpawnBrokenWeaponFragments(id);
        }

        protected void SpawnBrokenWeaponFragments(int sourceOid)
        {
            int count = BrokenWeaponFragmentCount(sourceOid);
            if (count <= 0 || Runtime == null) return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            for (int i = 0; i < count; i++)
            {
                int x = (int)Runtime.X + RandInt(0, 7) - 3;
                int y = (int)Runtime.Y + RandInt(0, 7) - 3;
                float vx = RandInt(0, 11) - 5f;
                float vy = BrokenWeaponFragmentVy(sourceOid, i);
                int frame = BrokenWeaponFragmentFrame(sourceOid, i);

                var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 999,
                    kind = 0,
                    action = frame,
                    facing = Runtime.Dir == "right" ? 0 : 1,
                    x = 0,
                    y = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0
                };
                task.parent = null;
                task.team = Team;
                task.pos = new Vector3(x, y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = Runtime.Dir;
                task.useDirectVelocity = true;
                task.directVx = vx;
                task.directVy = vy;
                task.directVz = 0f;
                task.releaseSpawnSemantic = LF2Tasks.ReleaseSpawnSemantic.BrokenFragment;
                factory.EnqueueCreateObject(task);
            }
        }

        private int BrokenWeaponFragmentCount(int oid)
        {
            if (oid == 101 || oid == 218) return 7;
            if (oid == 100 || oid == 213 || oid == 217) return 5;
            if (oid == 201 || oid == 120 || oid == 124) return 3;
            if (oid == 150) return 13;
            if (oid == 151) return 15;
            if (oid == 121) return 4;
            if (oid == 122 || oid == 123) return 9;
            return 0;
        }

        private float BrokenWeaponFragmentVy(int oid, int fragmentIndex)
        {
            if (oid == 150 || oid == 151 || oid == 213)
                return -(RandInt(0, 20) / 2f) - 8f;

            if (oid == 100 || oid == 101 || oid == 201 || oid == 120 || oid == 121 ||
                oid == 122 || oid == 123 || oid == 124 || oid == 217 || oid == 218)
            {
                if ((oid == 122 || oid == 123) && fragmentIndex >= 3)
                    return -(RandInt(0, 18) / 2f) - 4f;

                return -(RandInt(0, 8) / 2f) - 6f;
            }

            return 0f;
        }

        private int BrokenWeaponFragmentFrame(int oid, int fragmentIndex)
        {
            if (oid == 150) return RandInt(0, 4) + (fragmentIndex < 5 ? 0 : 4);
            if (oid == 100) return RandInt(0, 4) + (fragmentIndex < 2 ? 10 : 14);
            if (oid == 213) return RandInt(0, 4) + (fragmentIndex < 2 ? 150 : 154);
            if (oid == 101)
            {
                if (fragmentIndex < 5) return RandInt(0, 2) * 4 + RandInt(0, 4) + 20;
                return RandInt(0, 4) + 30;
            }
            if (oid == 151)
            {
                if (fragmentIndex < 2) return RandInt(0, 4) + 40;
                if (fragmentIndex < 5) return RandInt(0, 4) + 44;
                if (fragmentIndex < 8) return RandInt(0, 4) + 50;
                return RandInt(0, 4) + 54;
            }
            if (oid == 120) return RandInt(0, 4) + (fragmentIndex < 2 ? 54 : 30);
            if (oid == 124) return RandInt(0, 4) + 170;
            if (oid == 121) return RandInt(0, 4) + 60;
            if (oid == 122)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 70;
                if (fragmentIndex < 3) return RandInt(0, 4) + 80;
                return RandInt(0, 4) + 74;
            }
            if (oid == 123)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 160;
                if (fragmentIndex < 3) return RandInt(0, 4) + 164;
                return RandInt(0, 4) + 74;
            }
            if (oid == 217 || oid == 218) return RandInt(0, 4) + 174;
            return 0;
        }

        /// <summary>正式战斗随机数入口，对应 C++ release 的 ntsd_rand()。</summary>
        public int BattleRandInt(int minInclusive, int maxExclusive)
            => RandInt(minInclusive, maxExclusive);

        protected int RandInt(int minInclusive, int maxExclusive)
        {
            var rng = Match?.Rng;
            if (rng != null) return rng.NextInt(minInclusive, maxExclusive);
            return FallbackRng.NextInt(minInclusive, maxExclusive);
        }

        /// <summary>检查 itr arest 冷却是否允许攻击。</summary>
        public bool ItrArestTest() => ItrRest == null || ItrRest.Arest <= 0;

        internal static int ResolveArestCooldown(int arest, int vrest)
        {
            return arest < 4 && vrest == 0 ? 4 : arest;
        }

        /// <summary>命中后更新 arest 冷却。</summary>
        public void ItrArestUpdate(InteractionArea itr)
        {
            if (ItrRest == null) return;
            if (itr == null || SuppressesGenericArest(itr.kind)) return;

            ItrRest.Arest = ResolveArestCooldown(itr.arest, itr.vrest);
        }

        /// <summary>检查指定攻击者的 vrest 冷却是否结束。</summary>
        public bool ItrVrestTest(int uid) => ItrRest == null || !ItrRest.HasVrest(uid);

        /// <summary>更新指定攻击者的 vrest 冷却。</summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            if (SuppressesGenericVrest(itr.kind)) return;
            if (itr.vrest > 0)
                ItrRest.SetVrest(attackerUid, itr.vrest);
        }

        /// <summary>更新击飞路径的 vrest 冷却，固定写 45。</summary>
        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr)
        {
            ItrVrestUpdate(attackerUid, itr);
        }

        private static bool SuppressesGenericArest(int kind)
        {
            return kind == 8 || kind == 10 || kind == 11 || kind == 14 || kind == 15 || kind == 16;
        }

        private static bool SuppressesGenericVrest(int kind)
        {
            return kind == 8 || kind == 10 || kind == 11 || kind == 14 || kind == 15;
        }

        public bool ItrVrestTest(int uid, bool releaseRuntimeSlot) => ItrVrestTest(uid);

        public void ItrVrestUpdate(int attackerUid, InteractionArea itr, bool releaseRuntimeSlot)
            => ItrVrestUpdate(attackerUid, itr);

        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr, bool releaseRuntimeSlot)
            => ItrVrestUpdateKnockdown(attackerUid, itr);

        protected bool TryApplyKind6HitConfirm(InteractionArea itr, LF2Entity target)
        {
            if (itr?.kind != 6 || target == null || target == this)
                return false;
            if (target.Runtime == null || target.Frame?.D == null)
                return false;
            if (target.Health != null && target.Health.HP <= 0)
                return false;
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr))
                return false;
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(this, target))
                return false;
            if (!BruteForceSceneQuery.RuntimeConsumeItrAllowed(this, itr, target))
                return false;

            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true))
                return false;

            target.HitConfirmCounter = 3;
            return true;
        }

        internal bool TryApplyKind6HitConfirmForCharacterDatInteraction(InteractionArea itr, LF2Entity target)
            => TryApplyKind6HitConfirm(itr, target);

        protected void ApplyKind14DirectionalBlockFrom(LF2Entity attacker)
        {
            if (attacker?.Runtime == null || Runtime == null)
                return;

            int attackerX = attacker.Runtime.XInt;
            int attackerZ = attacker.Runtime.ZInt;
            int victimX = Runtime.XInt;
            int victimZ = Runtime.ZInt;

            if (attackerX > victimX + 5 && (Runtime.Vx > 0.0 || KnockbackVx > 0.0))
                Runtime.XBoundPositive = true;
            else if (attackerX < victimX - 5 && (Runtime.Vx < 0.0 || KnockbackVx < 0.0))
                Runtime.XBoundNegative = true;

            if (attackerZ > victimZ + 2 && (Runtime.Vz > 0.0 || KnockbackVz > 0.0))
                Runtime.ZBoundPositive = true;
            else if (attackerZ < victimZ - 2 && (Runtime.Vz < 0.0 || KnockbackVz < 0.0))
                Runtime.ZBoundNegative = true;
        }

        /// <summary>立即写入指定帧，绕过 wait 推进。</summary>
        // 这是最直接的硬切帧入口：
        // 当前帧会立刻变成目标帧，不等待 FrameTransistor 下一拍再处理。
        public virtual void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;
            if (FrameCache?.HasFrame(frameId) != true) return;
            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            Frame.N = frameId;
            Frame.D = targetFrame;
            AttackingCounter = 0;

            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
        }

        /// <summary>按帧 ID 获取帧数据。</summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
            => FrameCache?.GetFrameDataById(frameId);

        /// <summary>请求跳转到指定帧。</summary>
        // 对外的标准跳帧入口，默认 wait=0。
        public virtual void TransitionToFrame(int frameId)
            => TransitionToFrame(frameId, 0);

        /// <summary>请求跳转到指定帧。</summary>
        // 和 ImmediateFrame 的区别在于：这里是把请求交给 FrameTransistor，
        // 让它按正式 frame_tick 顺序在后续推进里消费。
        public virtual void TransitionToFrame(int frameId, int wait = 0)
        {
            if (Trans == null)
                return;

            Trans.SetNext(frameId);
            Trans.SetWait(wait);
        }

        /// <summary>获取碰撞用 sprite 宽度，单位为像素。</summary>
        public virtual float GetSpriteWidthPxForCollision() => 0f;



        public abstract void Reset();
        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>从 SimulationWorld 注销自身。</summary>
        public virtual void UnregisterFromWorld()
        {
            registeredWorld?.Unregister(this);
        }

        /// <summary>销毁当前对象的可视表现。</summary>
        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>FrameTransistor 检测到 next=1000 时调用，子类可实现销毁逻辑。</summary>
        public virtual void OnTransitDestroy()
        {
            DestroyEvent();
            Destroy();
            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            else
            {
                UnregisterFromWorld();
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        // FrameTransistor 真正执行换帧时，会先走到这里。
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans, Trans?.WaitCounter ?? 0);
        }

        /// <summary>帧转换回调，子类实现具体帧切换逻辑。</summary>
        // 需要额外参考 oldLock 或保留更细对齐语义时，子类实现这个重载。
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock) { }



        public int SimOrder => SimOrderConstants.GetSimOrderByObjectType(ObjectTypeEnum);

        public virtual void OnAdded(SimContext ctx)
        {
            registeredWorld = ctx?.World;
            RefreshRuntimeSnapshot();
        }

        public virtual void OnRemoved(SimContext ctx)
        {
            if (ReferenceEquals(registeredWorld, ctx?.World))
                registeredWorld = null;
            TrackerParent = null;
            Runtime.SlotIndex = -1;
        }

        internal LF2Entity ResolveTrackerParentFromRuntime()
        {
            int selfSlot = Runtime?.SlotIndex ?? -1;
            int parentSlot = Runtime?.HolderStableId ?? -1;
            if ((Runtime?.LinkState ?? 0) >= 0 || selfSlot < 0 || parentSlot < 0)
            {
                TrackerParent = null;
                return null;
            }

            LF2Entity parent = Match?.FindEntityByRuntimeSlotForQuery(parentSlot);
            if (parent == null && (TrackerParent?.Runtime?.SlotIndex ?? -1) == parentSlot)
                parent = TrackerParent;

            if (parent?.Runtime == null || parent.Runtime.LinkState <= 0 ||
                parent.Runtime.TargetSlotIndex != selfSlot)
            {
                TrackerParent = null;
                return null;
            }

            TrackerParent = parent;
            return parent;
        }

        public virtual void SimTransit(int tickIndex) { }
        public virtual void SimTU(int tickIndex) { }
        public virtual void SimPostInteraction(int tickIndex)
        {
            if (!UsesCharacterDatInteractionPhase())
                return;

            LF2CharacterDatInteractionResolver.TryConsumeUnifiedStep7CandidateSequence(this);
        }
        public virtual void SimObjectInteraction(int tickIndex) { }
        public virtual void SimPreInteraction(int tickIndex) { }
        public virtual void SimEntityCollision(int tickIndex) { }
        public virtual void SimFrameTick(int tickIndex) { }

        internal bool RunLateFrameTickAndReportSnapshotRefreshRequired(
            int tickIndex,
            out bool snapshotProofEvaluated)
        {
            SimFrameTick(tickIndex);

            // Production LF2Character's SimFrameTick and runtime refresh both
            // delegate to the base implementations. Unknown subclasses or
            // other entity families may add non-runtime-backed state, so they
            // retain the complete legacy snapshot.
            snapshotProofEvaluated = GetType() == typeof(LF2Character);
            return !snapshotProofEvaluated ||
                   !IsCompleteRuntimeSnapshotCurrentForExactCharacter();
        }

        /// <summary>模拟后期更新，默认刷新渲染深度。</summary>
        public virtual void SimLateTick(int tickIndex)
        {
            Sprite?.SetZ(GetRenderSortingOrder());
        }

        public virtual void RunFrameLogicBeforeAdvance()
        {
            RunCurrentDatFrameLogicBeforeAdvance();
        }

        private void RunCurrentDatFrameLogicBeforeAdvance()
        {
            int hitFa = Frame?.D?.hit_Fa ?? 0;
            if (Runtime == null || (hitFa != 1 && hitFa != 2 && hitFa != 3 && hitFa != 4 && hitFa != 5 && hitFa != 6 && hitFa != 7 && hitFa != 8 && hitFa != 9 && hitFa != 10 && hitFa != 11 && hitFa != 12 && hitFa != 13 && hitFa != 14))
                return;

            if (hitFa == 1)
            {
                RunHitFa1FrameLogic();
                return;
            }

            if (hitFa == 3)
            {
                RunHitFa3FrameLogic();
                return;
            }

            if (hitFa == 2 || hitFa == 4 || hitFa == 12 || hitFa == 14)
            {
                RunHitFa2Or4Or12Or14FrameLogic(hitFa);
                return;
            }

            if (hitFa == 10)
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 1.1f;
                else
                    Runtime.Vx += 1.1f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -30.0, 30.0);
                if (Runtime.Y > 3f)
                    Runtime.Y = 3f;

                SwitchDir(Runtime.Vx > 0f ? "right" : "left");
                Runtime.YInt = (int)Runtime.Y;
                return;
            }

            if (hitFa == 6 || hitFa == 9)
            {
                RunHitFa6Or9FrameLogic(hitFa);
                return;
            }

            if (hitFa == 8)
            {
                RunHitFa8FrameLogic();
                return;
            }

            if (hitFa == 11)
            {
                RunHitFa11FrameLogic();
                return;
            }

            if (hitFa == 13)
            {
                RunHitFa13FrameLogic();
                return;
            }

            if (hitFa == 5)
            {
                RunHitFa5FrameLogic();
                return;
            }

            RunHitFa7FrameLogic();
        }

        private void RunHitFa1FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(1);
            if (target == null || target.Health == null || target.Health.HP <= 0)
            {
                if (Health != null)
                    Health.HP = 0;
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 1);
            int selfZ = GetFrameLogicTargetZInt(this, 1);

            if (targetX > selfX)
                Runtime.Vx += 0.85f;
            if (targetX < selfX)
                Runtime.Vx -= 0.85f;
            if (targetZ > selfZ + 7)
                Runtime.Vz += 0.3f;
            if (targetZ < selfZ - 7)
                Runtime.Vz -= 0.3f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f鈫?.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 10f < target.Runtime.Y)
                    Runtime.Y += 1.2f;
                if (Runtime.Y + 10f > target.Runtime.Y)
                    Runtime.Y -= 1.2f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -13.0, 13.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.0, 2.0);
            if (Runtime.Y > 1f)
                Runtime.Y = 1f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private void RunHitFa3FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(3);
            if (target == null)
            {
                if (Health != null)
                    Health.HP = 0;

                return;
            }

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa3NoTargetDrift();
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 3);
            int selfZ = GetFrameLogicTargetZInt(this, 3);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 10)
                Runtime.Vz += 0.17f;
            if (targetZ < selfZ - 10)
                Runtime.Vz -= 0.17f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -16.0, 16.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.4, 2.4);
        }

        private void RunHitFa8FrameLogic()
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null || Match == null)
                return;

            var enemies = new List<int>(8);
            CollectActiveEnemyCharacterSlots(enemies);

            int count = 3;
            if (enemies.Count > 4)
                count = (enemies.Count - 3) / 2 + 3;

            if (ResolveRuntimeCharacterConfig(225)?.characterData == null)
            {
                Runtime.PendingFlushDestroy = true;
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int freeSlot = FindFirstAvailableFrameLogicSlot();
                if (freeSlot < 0)
                    break;

                double directVx = RandInt(0, 21) - 11;
                double directVy = 3.0 - RandInt(0, 24) * 0.25;
                double directVz = 3.0 - RandInt(0, 24) * 0.25;
                int ownerSlot = enemies.Count > 0
                    ? enemies[RandInt(0, enemies.Count)]
                    : GetRuntimeSlotOrNegative(this);

                OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 225,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = Runtime.Dir;
                task.dvz = 0f;
                task.useDirectRuntimePosition = true;
                task.directX = Runtime.X;
                task.directY = Runtime.Y;
                task.directZ = Runtime.Z;
                task.useDirectVelocity = true;
                task.directVx = directVx;
                task.directVy = directVy;
                task.directVz = directVz;
                task.ownerEntityIndex = ownerSlot;
                task.requiredRuntimeSlot = freeSlot;
                FillHitFa8SpawnTask(task);
                LF2Entity spawned;
                try
                {
                    spawned = factory.CreateObjectImmediate(task);
                }
                finally
                {
                    referencePool.Recycle(task);
                }

                if (spawned == null || spawned.Runtime?.SlotIndex != freeSlot)
                    break;
            }

            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa6Or9FrameLogic(int hitFa)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null || Match == null)
                return;

            var enemies = new List<int>(8);
            CollectActiveEnemyCharacterSlots(enemies);

            int max = hitFa == 9 ? 10 : 7;
            int maxPerLaterPass = hitFa == 9 ? 4 : 0;
            int attemptCount = 0;
            int loopCount = 0;
            int lastFreeSlot = -1;

            do
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (!(attemptCount < maxPerLaterPass || loopCount == 0))
                        continue;

                    int enemySlot = enemies[i];
                    LF2Entity target = Match.FindEntityByRuntimeSlotForQuery(enemySlot);
                    if (target == null)
                        continue;

                    attemptCount++;
                    lastFreeSlot = FindFirstAvailableFrameLogicSlot();
                    if (lastFreeSlot < 0)
                    {
                        if (attemptCount >= max)
                            break;
                        continue;
                    }

                    int oid = hitFa == 9 ? RandInt(0, 2) + 221 : 220;
                    if (ResolveRuntimeCharacterConfig(oid)?.characterData == null)
                    {
                        if (attemptCount >= max)
                            break;
                        continue;
                    }

                    double vx;
                    double vy;
                    if (hitFa == 6)
                    {
                        vx = (target.GetRuntimeXInt() - GetRuntimeXInt()) / 50.0;
                        vy = -4.0 - RandInt(0, 4);
                    }
                    else
                    {
                        vx = RandInt(0, 21) - 11;
                        vy = -2.0 - RandInt(0, 40) * 0.1666666666666667;
                    }

                    OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                    task.opoint = new ObjectPoint
                    {
                        oid = oid,
                        kind = 0,
                        action = 0,
                        dvx = 0,
                        dvy = 0,
                        dvz = 0,
                        facing = 0,
                    };
                    task.parent = this;
                    task.team = Team;
                    task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                    task.z = (float)Runtime.Z;
                    task.dir = "right";
                    task.dvz = 0f;
                    task.useDirectRuntimePosition = true;
                    task.directX = Runtime.X;
                    task.directY = Runtime.Y;
                    task.directZ = Runtime.Z;
                    task.useDirectVelocity = true;
                    task.directVx = vx;
                    task.directVy = vy;
                    task.directVz = 0f;
                    task.ownerEntityIndex = enemySlot;
                    task.requiredRuntimeSlot = lastFreeSlot;
                    FillHitFa8SpawnTask(task);
                    LF2Entity spawned;
                    try
                    {
                        spawned = factory.CreateObjectImmediate(task);
                    }
                    finally
                    {
                        referencePool.Recycle(task);
                    }

                    if (spawned == null || spawned.Runtime?.SlotIndex != lastFreeSlot)
                    {
                        lastFreeSlot = -1;
                        break;
                    }

                    if (attemptCount >= max)
                        break;
                }

                loopCount++;
            } while (hitFa == 9 &&
                     attemptCount < maxPerLaterPass &&
                     attemptCount > 0 &&
                     lastFreeSlot != -1 &&
                     attemptCount < max);

            Runtime.PendingFlushDestroy = true;
        }

        private void CollectActiveEnemyCharacterSlots(List<int> slots)
        {
            slots.Clear();
            if (Match == null)
                return;

            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int slot = 0; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                LF2Entity candidate = Match.FindEntityByRuntimeSlotForQuery(slot);
                if (IsDeadLikeFrameLogicTarget(candidate) ||
                    !IsCharacterFrameLogicTarget(candidate) ||
                    ResolveFrameLogicRelationIdentity(candidate) == selfTeam)
                {
                    continue;
                }

                slots.Add(slot);
            }
        }

        private int FindFirstAvailableFrameLogicSlot()
        {
            return Match?.FindFirstFreeFrameLogicRuntimeSlot() ?? -1;
        }

        private static LF2Entity PublishFrameLogicObjectImmediate(
            LF2ObjectPointFactory factory,
            LF2ReferencePool referencePool,
            OPointCreateTask task,
            int requiredSlot)
        {
            if (factory == null || referencePool == null || task == null || requiredSlot < 0)
                return null;

            task.requiredRuntimeSlot = requiredSlot;
            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(task);
            }
            finally
            {
                referencePool.Recycle(task);
            }

            return spawned?.Runtime?.SlotIndex == requiredSlot ? spawned : null;
        }

        private void RunHitFa2Or4Or12Or14FrameLogic(int hitFa)
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(hitFa);
            NTSDEntityRuntime rawTargetRuntime = hitFa == 4 && target == null
                ? Match?.GetRawRuntimeSlotState(OwnerEntityIndex)
                : null;
            bool rawSlotTarget = rawTargetRuntime != null;

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            bool targetHasHp = target != null
                ? target.Health != null && target.Health.HP > 0
                : rawTargetRuntime != null && rawTargetRuntime.HP > 0;
            if (hitFa == 4 && targetHasHp)
            {
                int dx = (target?.GetRuntimeXInt() ?? rawTargetRuntime.XInt) - GetRuntimeXInt();
                int dy = (target?.GetRuntimeYInt() ?? rawTargetRuntime.YInt) - GetRuntimeYInt();
                int dz = (target != null ? GetFrameLogicZInt(target) : rawTargetRuntime.ZInt) - GetFrameLogicZInt(this);
                if (dx > -30 && dx < 30 && dy > 0 && dy < 80 && dz > -10 && dz < 10)
                {
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                    SetFrameTickDirect(60);
                    if (target != null)
                        target.CatchTimer = 100;
                    else
                        rawTargetRuntime.CatchTimer = 100;
                    return;
                }
            }

            if (target == null && !rawSlotTarget)
            {
                if (hitFa != 4 && Health != null)
                {
                    Health.HP = 0;
                    return;
                }

                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            int targetX = target?.GetRuntimeXInt() ?? rawTargetRuntime?.XInt ?? 0;
            int selfX = GetRuntimeXInt();
            int targetZ = target != null ? GetFrameLogicTargetZInt(target, hitFa) : rawTargetRuntime?.ZInt ?? 0;
            int selfZ = GetFrameLogicTargetZInt(this, hitFa);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 5)
                Runtime.Vz += 0.4f;
            if (targetZ < selfZ - 5)
                Runtime.Vz -= 0.4f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f鈫?.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (target != null && IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 40f < target.Runtime.Y)
                    Runtime.Y += 1f;
                if (Runtime.Y + 40f > target.Runtime.Y)
                    Runtime.Y -= 1f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            if (hitFa == 14)
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -1.5, 1.5);
            else
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();

            if (hitFa == 14)
            {
                double absVx = System.Math.Abs(Runtime.Vx);
                int curFrame = Frame?.N ?? -1;
                if (absVx >= 8f)
                {
                    if (curFrame > 40)
                        SetFrameTickDirect(curFrame - 50);
                }
                else if (curFrame < 10)
                {
                    SetFrameTickDirect(curFrame + 50);
                }
            }
        }

        private void RunHitFa7FrameLogic()
        {
            if (Match != null)
                SpawnHitFa7Clone();

            LF2Entity target = null;
            int targetSlot = Runtime.OwnerSlotIndex;
            if (Match != null && targetSlot >= 0)
                target = Match.FindEntityByRuntimeSlotForQuery(targetSlot) ??
                         Match.FindEntityByRuntimeSlotIncludingPending(targetSlot);

            bool rawSlotTarget = target == null && IsReferenceRuntimeSlot(targetSlot);
            bool valid = (target != null || rawSlotTarget) && Health != null && Health.HP > 0;
            if (valid)
            {
                int targetX = target?.GetRuntimeXInt() ?? 0;
                if (targetX > GetRuntimeXInt())
                {
                    Runtime.Vx += 0.7f;
                    Runtime.Vx += 0.7f;
                }
                else if (targetX < GetRuntimeXInt())
                {
                    Runtime.Vx -= 0.7f;
                    Runtime.Vx -= 0.7f;
                }

                int targetZ = target?.Runtime?.ZInt ?? 0;
                int selfZ = Runtime.ZInt;
                if (targetZ > selfZ + 5)
                    Runtime.Vz += 0.4f;
                if (targetZ < selfZ - 5)
                    Runtime.Vz -= 0.4f;

                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
                if (Runtime.Y > 1.4f)
                    Runtime.Y = 1.4f;
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);
            }
            else
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 2f;
                else
                    Runtime.Vx += 2f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.YInt = -25;
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }
            }

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private bool IsReferenceRuntimeSlot(int runtimeSlot)
        {
            return Match != null &&
                   runtimeSlot >= 0 &&
                   runtimeSlot < Match.MaxRuntimeSlotsForServices;
        }

        private void RunHitFa13FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null)
                return;

            var enemies = new List<int>(8);
            CollectActiveEnemyCharacterSlots(enemies);

            int freeSlot = FindFirstAvailableFrameLogicSlot();
            if (freeSlot < 0)
            {
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int spawnOid = 228;
            if (ResolveRuntimeCharacterConfig(spawnOid)?.characterData == null)
            {
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int chosenTarget = enemies.Count == 0
                ? GetRuntimeSlotOrNegative(this)
                : enemies[RandInt(0, enemies.Count)];

            int spawnYInt = Runtime.YInt + RandInt(0, 7) - 3;
            double spawnVz = 3.0 - RandInt(0, 24) * 0.25 + Runtime.Vz;
            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = spawnOid,
                kind = 0,
                action = 0,
                dvx = 0,
                dvy = 0,
                dvz = 0,
                facing = 0,
            };
            task.parent = this;
            task.team = Team;
            task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = Runtime.Dir;
            task.dvz = 0f;
            task.useDirectRuntimePosition = true;
            task.directX = Runtime.X;
            task.directY = Runtime.Y;
            task.directZ = Runtime.Z;
            task.useDirectVelocity = true;
            task.directVx = Runtime.Vx;
            task.directVy = 0.1;
            task.directVz = spawnVz;
            task.ownerEntityIndex = chosenTarget;
            FillHitFa13SpawnTask(task);
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = spawnYInt;
            task.initialRuntimeZ = Runtime.ZInt;
            PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);

            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa5FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null)
                return;

            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int allySlot = 0; allySlot < Match.MaxRuntimeSlotsForServices; allySlot++)
            {
                LF2Entity ally = Match.FindEntityByRuntimeSlotForQuery(allySlot);
                if (IsDeadLikeFrameLogicTarget(ally))
                    continue;
                if (!IsCharacterFrameLogicTarget(ally))
                    continue;
                if (ResolveFrameLogicRelationIdentity(ally) != selfTeam)
                    continue;

                int freeSlot = FindFirstAvailableFrameLogicSlot();
                if (freeSlot < 0)
                    continue;
                if (ResolveRuntimeCharacterConfig(219)?.characterData == null)
                    continue;

                OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 219,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = "right";
                task.dvz = 0f;
                task.useDirectRuntimePosition = true;
                task.directX = Runtime.X;
                task.directY = Runtime.Y;
                task.directZ = Runtime.Z;
                task.useDirectVelocity = true;
                task.directVx = (ally.GetRuntimeXInt() - GetRuntimeXInt()) / 50.0;
                task.directVy = 0.0;
                task.directVz = 0.0;
                task.ownerEntityIndex = allySlot;
                FillHitFa13SpawnTask(task);
                task.initialRuntimeX = Runtime.XInt;
                task.initialRuntimeY = Runtime.YInt;
                task.initialRuntimeZ = Runtime.ZInt;
                PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);
            }

            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa11FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null)
                return;

            (int oid, int frameId, int xOff, int yOff, int zOff, double vzDelta, int facing)[] spawns =
            {
                (211, 109,    0,    0,  0,  0.0, 2),
                (221,  81,    0, -100,  0,  0.0, 2),
                (212, 100,   80,   -3,  0, -7.0, 0),
                (212, 100,  100,   -3,  0,  0.0, 0),
                (212, 100,   80,   -3,  0,  7.0, 0),
                (212, 100,  -80,   -3,  0, -7.0, 1),
                (212, 100, -100,   -3,  0,  0.0, 1),
                (212, 100,  -80,   -3,  0,  7.0, 1),
                (211,  50,  -30,   -1, -5,  0.0, 1),
                (211,  50,   30,   -1, -5,  0.0, 1),
                (211,  50,  -30,   -1,  2,  0.0, 0),
                (211,  50,   30,   -1,  2,  0.0, 0),
                (211,  50,    0,   -1, -9,  0.0, 1),
                (211,  50,    0,   -1,  6,  0.0, 0),
            };

            for (int i = 0; i < spawns.Length; i++)
            {
                var spawn = spawns[i];
                if (ResolveRuntimeCharacterConfig(spawn.oid)?.characterData == null)
                    continue;

                int freeSlot = FindFirstAvailableFrameLogicSlot();
                if (freeSlot < 0)
                    break;

                string spawnDir = spawn.facing == 2
                    ? Runtime.Dir
                    : spawn.facing == 0 ? "right" : "left";
                int spawnX = Runtime.XInt + spawn.xOff;
                int spawnY = Runtime.YInt + spawn.yOff;
                int spawnZ = Runtime.ZInt + spawn.zOff;
                OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = spawn.oid,
                    kind = 0,
                    action = spawn.frameId,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3(spawnX, spawnY, spawnZ);
                task.z = spawnZ;
                task.dir = spawnDir;
                task.dvz = 0f;
                task.useDirectRuntimePosition = true;
                task.directX = spawnX;
                task.directY = spawnY;
                task.directZ = spawnZ;
                task.useDirectVelocity = true;
                task.directVx = Runtime.Vx;
                task.directVy = Runtime.Vy;
                task.directVz = Runtime.Vz + spawn.vzDelta;
                FillHitFa13SpawnTask(task);
                task.initialRuntimeX = spawnX;
                task.initialRuntimeY = spawnY;
                task.initialRuntimeZ = spawnZ;
                PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);
            }

            Runtime.PendingFlushDestroy = true;
            ResolveFrameLogicTargetByHitFa(11);

            if (OwnerEntityIndex < 0)
            {
                if (Health != null)
                    Health.HP = 0;
                return;
            }

            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");

        }

        private void SpawnHitFa7Clone()
        {
            if (Match == null || FrameCache?.Wrapper?.characterData == null)
                return;

            int freeSlot = FindFirstAvailableFrameLogicSlot();
            if (freeSlot < 0)
                return;

            int cloneOid = FrameCache.Wrapper.characterId;
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null || ResolveRuntimeCharacterConfig(cloneOid)?.characterData == null)
                return;

            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = cloneOid,
                kind = 0,
                action = 40,
                dvx = 0,
                dvy = 0,
                dvz = 0,
                facing = 0,
            };
            task.team = Team;
            task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = "right";
            task.useDirectRuntimePosition = true;
            task.directX = Runtime.X;
            task.directY = Runtime.Y;
            task.directZ = Runtime.Z;
            task.useDirectVelocity = true;
            task.directVx = 0.0;
            task.directVy = 0.0;
            task.directVz = 0.0;
            FillHitFa13SpawnTask(task);
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = Runtime.YInt;
            task.initialRuntimeZ = Runtime.ZInt;
            PublishFrameLogicObjectImmediate(factory, referencePool, task, freeSlot);
        }

        private void FillHitFa13SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = (int)task.pos.x;
            task.initialRuntimeY = (int)task.pos.y;
            task.initialRuntimeZ = (int)task.pos.z;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
        }

        private void FillHitFa8SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = Runtime.YInt;
            task.initialRuntimeZ = Runtime.ZInt;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
        }

        private LF2Entity ResolveFrameLogicTargetByHitFa(int hitFa)
        {
            if (Match == null)
                return null;

            if (hitFa == 4)
            {
                return OwnerEntityIndex >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(OwnerEntityIndex) ??
                      Match.FindEntityByRuntimeSlotIncludingPending(OwnerEntityIndex)
                    : null;
            }

            int selfTeam = ResolveFrameLogicRelationIdentity();
            int holderTeam = -1;
            if (SpawnerEntityIndex >= 0)
            {
                LF2Entity spawner = Match.FindEntityByRuntimeSlotForQuery(SpawnerEntityIndex);
                if (spawner != null)
                    holderTeam = ResolveFrameLogicRelationIdentity(spawner);
            }

            int currentTargetSlot = OwnerEntityIndex;
            bool needScan = true;
            LF2Entity target = currentTargetSlot >= 0
                ? Match.FindEntityByRuntimeSlotForQuery(currentTargetSlot)
                : null;

            if (target != null)
            {
                bool valid = !IsDeadLikeFrameLogicTarget(target) &&
                             IsCharacterFrameLogicTarget(target) &&
                             target.GetState() != LF2States.Lying &&
                             Mathf.Abs(target.HitStun) <= 2f &&
                             ResolveFrameLogicRelationIdentity(target) != selfTeam;
                if (valid && holderTeam != ResolveFrameLogicRelationIdentity(target))
                    needScan = false;
                if (!valid)
                    target = null;
            }

            if (needScan)
            {
                var allObjects = new List<LF2Entity>(16);
                Match.GetAllEntities(allObjects);

                int bestDist = 10000;
                int bestSlot = -1;
                for (int i = 0; i < allObjects.Count; i++)
                {
                    LF2Entity obj = allObjects[i];
                    if (obj == null || ReferenceEquals(obj, this))
                        continue;
                    if (IsDeadLikeFrameLogicTarget(obj))
                        continue;
                    if (!IsCharacterFrameLogicTarget(obj))
                        continue;

                    int objTeam = ResolveFrameLogicRelationIdentity(obj);
                    if (objTeam == selfTeam)
                        continue;
                    if (holderTeam >= 0 && objTeam == holderTeam)
                        continue;
                    if ((obj.GetState() == LF2States.Lying || Mathf.Abs(obj.HitStun) > 2f) && currentTargetSlot != -1)
                        continue;

                    int dist = Mathf.Abs(obj.GetRuntimeXInt() - GetRuntimeXInt()) +
                               Mathf.Abs(GetFrameLogicTargetZInt(obj, hitFa) - GetFrameLogicTargetZInt(this, hitFa));
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestSlot = GetRuntimeSlotOrNegative(obj);
                    }
                }

                OwnerEntityIndex = bestSlot;
                target = bestSlot >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(bestSlot)
                    : null;
            }

            return target;
        }

        private int ResolveFrameLogicRelationIdentity()
        {
            return ResolveFrameLogicRelationIdentity(this);
        }

        private static int ResolveFrameLogicRelationIdentity(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }

        private static bool IsCharacterFrameLogicTarget(LF2Entity entity)
        {
            return entity?.GetCurrentDataObjectType() == (int)LF2ObjectType.Character;
        }

        private static bool IsDeadLikeFrameLogicTarget(LF2Entity entity)
        {
            if (entity == null)
                return true;
            if (entity is LF2LivingObject living && living.Dead)
                return true;

            return entity.Health == null || entity.Health.HP <= 0;
        }

        private static int GetRuntimeSlotOrNegative(LF2Entity entity)
        {
            return entity?.Runtime?.SlotIndex ?? -1;
        }

        private void ApplyHitFa2Or4Or12Or14NoTargetCatch(int hitFa)
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();
        }

        private void ApplyHitFa3NoTargetDrift()
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
        }

        private void ApplyHitFa2FrameSelection()
        {
            double absVx = System.Math.Abs(Runtime.Vx);
            int curFrame = Frame?.N ?? -1;
            if (absVx > 14f)
            {
                if (curFrame != 5 && curFrame != 6)
                    SetFrameTickDirect(5);
            }
            else if (absVx > 7f)
            {
                if (curFrame != 3 && curFrame != 4)
                    SetFrameTickDirect(3);
            }
            else
            {
                if (curFrame != 1 && curFrame != 2)
                    SetFrameTickDirect(1);
            }
        }

        private static int GetFrameLogicZInt(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            if (entity.GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                entity.Runtime != null &&
                System.Math.Abs(entity.Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (int)(entity.Runtime.Z - entity.Runtime.Type3VisualZOffset);
            }

            return entity.Runtime?.ZInt ?? 0;
        }

        private static int GetFrameLogicTargetZInt(LF2Entity entity, int hitFa)
        {
            if (hitFa == 1 || hitFa == 3 || hitFa == 7 || hitFa == 12 || hitFa == 14)
                return entity?.Runtime?.ZInt ?? 0;

            return GetFrameLogicZInt(entity);
        }

        internal virtual bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame)
        {
            return frame != null &&
                   frame.hit_Fa > 0 &&
                   GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character;
        }

        internal bool SupportsPostInteractionPhase() => UsesCharacterDatInteractionPhase();

        internal bool SupportsObjectInteractionPhase() => !UsesCharacterDatInteractionPhase();

        protected bool UsesCharacterDatInteractionPhase()
            => GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;

        internal virtual bool UsesDynamicRuntimeSlot() => false;

        internal virtual bool IsStageBoundedCharacter()
            => GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;

        internal virtual bool ShouldContributeToReleaseCamera() => false;

        internal virtual void ApplyPreFrameZBounds(float zMin, float zMax)
        {
            if (Runtime == null)
                return;

            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (currentDataType == (int)LF2ObjectType.SpecialAttack)
            {
                double logicZ = Runtime.Z - Runtime.Type3VisualZOffset;
                logicZ = System.Math.Clamp(logicZ, zMin - 1.0, zMax + 1.0);
                Runtime.Z = logicZ + Runtime.Type3VisualZOffset;
            }
            else if (currentDataType == (int)LF2ObjectType.Character)
            {
                Runtime.Z = System.Math.Clamp(Runtime.Z, zMin, zMax);
            }
            else
            {
                Runtime.Z = System.Math.Clamp(Runtime.Z, zMin - 1.0, zMax + 1.0);
            }

            Runtime.ZInt = (int)Runtime.Z;
        }

        // C++ PreFrame keeps the background width separate from the phase-only character override.
        internal virtual bool ApplyPreFrameXBounds(float baseStageWidth, int xMaxOverride)
        {
            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (currentDataType == (int)LF2ObjectType.SpecialAttack)
            {
                if (Runtime.X < -300f || Runtime.X > baseStageWidth + 300f)
                {
                    FreeEntityLikeExe();
                    return true;
                }
            }
            else if (currentDataType == (int)LF2ObjectType.Character)
            {
                int slotIndex = Runtime?.SlotIndex ?? StableId;
                if (slotIndex >= 20)
                {
                    if (Runtime.X < -100f)
                        Runtime.X = -100f;
                    if (Runtime.X > baseStageWidth + 100f)
                        Runtime.X = baseStageWidth + 100f;
                }
                else
                {
                    if (RelationTeam == 5)
                    {
                        if (Runtime.X < -300f)
                            Runtime.X = -300f;
                    }
                    else if (Runtime.X < 0f)
                    {
                        Runtime.X = 0f;
                    }

                    if (Runtime.X > baseStageWidth)
                        Runtime.X = baseStageWidth;

                    if (xMaxOverride > 0 &&
                        Runtime.X > xMaxOverride &&
                        RelationTeam != 5 &&
                        HitStun == 0)
                    {
                        Runtime.X = xMaxOverride;
                    }
                }
            }
            else if ((ObjectId == 122 || ObjectId == 123) && Unk344 > 0)
            {
                if (Runtime.X < 10f)
                    Runtime.X = 10f;
                if (Runtime.X > baseStageWidth - 10f)
                    Runtime.X = baseStageWidth - 10f;
            }
            else if (Runtime.YInt == 0 && (Runtime.X < 0f || Runtime.X > baseStageWidth))
            {
                FreeEntityLikeExe();
                return true;
            }

            Runtime.XInt = (int)Runtime.X;
            return false;
        }

        /// <summary>
        /// pre-collision 阶段的公共 state 特判。
        /// 对齐参考 C# `RunStateSpecialPreCollision`：
        /// - state 4000..4999：切换到 `state - 4000` 对应对象并进入 frame 0
        /// - state 8000..8999：切换到 `state - 8000` 对应对象并进入 frame 0，同时写入 140 hit stop
        ///
        /// 这里仍然保持 Unity 当前架构边界：
        /// 只切换 `ObjectId + FrameCache`，不在这里改运行时 C# 实例类型。
        /// </summary>
        public virtual void RunStateSpecialPreCollision()
        {
            LF2FrameData frameData = Frame?.D;
            if (frameData == null)
                return;

            int state = frameData.state;
            if (state == 9995 && GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
            {
                ApplyStateDataTransform(50, false);
                return;
            }

            if (state >= 4000 && state < 5000)
            {
                ApplyStateDataTransform(state - 4000, false);
                return;
            }

            if (state >= 8000 && state < 9000)
                ApplyStateDataTransform(state - 8000, true);
        }

        internal virtual void RunPreCollisionRecoveryPhase(int tickIndex)
        {
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character || Health == null)
                return;

            BattleFlowRuntimeState flow = Match?.Runtime?.Flow;
            bool stepWaitGate = flow != null && flow.BattleStepMode == 1 && flow.BattleStepGate != 1;
            bool period12 = tickIndex % NTSDGlobal.Gameplay.HpRecoverPeriod == 0;
            if (Health.HP > 0 && Health.HP < Health.HPBound && period12 && !stepWaitGate)
                Health.HP++;

            if (WeaponCount < 0 && period12 && !stepWaitGate)
            {
                int injury = NTSDGlobal.Gameplay.NegativeWeaponCountInjury;
                if (FallDamageDiv > 0)
                    injury = NTSDGlobal.Gameplay.NegativeWeaponCountScaledInjury / FallDamageDiv;

                Health.HP -= injury;
                Health.HPBound -= injury / NTSDGlobal.Gameplay.NegativeWeaponCountHpBoundDivisor;
                if (Health.HP < 0)
                    Health.HP = 0;
                if (Health.HPBound < 0)
                    Health.HPBound = 0;
                ComboCountVic += 9;
            }

            if (tickIndex % NTSDGlobal.Gameplay.PpRecoverPeriod != 0)
                return;
            if (KillCount != -1 && Health.PP >= NTSDGlobal.Gameplay.PpRecoverLowLimit)
                return;
            if (Health.PP >= NTSDGlobal.Gameplay.PpRecoverCap || HitStun < 0 || stepWaitGate)
                return;

            int hpForRate = System.Math.Min(Health.HP, NTSDGlobal.Gameplay.PpRecoverCap);
            if (ObjectId == 51 || ObjectId == 52)
                hpForRate /= 2;

            Health.PP += ((NTSDGlobal.Gameplay.PpRecoverCap - hpForRate) /
                          NTSDGlobal.Gameplay.PpRecoverHpRateDivisor) + 1;
        }

        /// <summary>
        /// 冷却递减后的输入消费阶段。
        /// 参考 C# 基准工程这里按当前 DAT `ObjType == 0` 分发角色输入；
        /// Unity 当前由 `LF2Character` 覆盖完整角色输入链；
        /// 对于“当前 DAT 已是 Character，但 CLR 运行时实例不是 LF2Character”的实体，
        /// 这里至少要补齐共享输入快照、基础 combo/direct frame jump，
        /// 以及不依赖完整角色 resolver 的 standing/walking 三个基础动作入口。
        /// </summary>
        internal virtual void RunHumanInputPollPhase(int tickIndex)
        {
            if (Runtime == null || AiControlled)
                return;

            UpdateSharedRuntimeInputSnapshotForSimulation(tickIndex);
        }

        internal virtual void ClearBattleEntryInputState()
        {
            Runtime?.ResetInputState();
            sharedCharacterDatInputModule.Reset();
        }

        internal virtual void RunCharacterInputPhase(int tickIndex)
        {
            if (Runtime == null || Runtime.LinkState < 0)
                return;

            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            if (AiControlled)
                Match?.PrepareAiInputBasic(this, tickIndex);

            if (this is LF2Character)
                return;

            RunSharedCharacterDatFrameJumpInputPhase();
            RunSharedCharacterDatStandingActionInputPhase();
            ApplyNonCharacterFrameVelocityForFrameAdvance();
        }

        /// <summary>
        /// Combined compatibility entry for focused resolver self-checks. Production ticks call
        /// RunHumanInputPollPhase and RunCharacterInputPhase at separate C# authority phases.
        /// </summary>
        internal virtual void RunPostCooldownInputPhase(int tickIndex)
        {
            if (!AiControlled)
                RunHumanInputPollPhase(tickIndex);
            RunCharacterInputPhase(tickIndex);
        }

        protected bool UsesSharedCharacterDatShellRouting()
        {
            return Runtime != null &&
                   this is not LF2Character &&
                   GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        /// <summary>
        /// 按当前运行时壳类型解析共享输入控制器。
        /// 这里不要求一定是 `LF2Character`，因为 transform 后的 current DAT character
        /// 仍然可能挂在 `LF2OtherObject` / `LF2SpecialAttack` / `LF2WeaponBase` 壳上。
        /// </summary>
        internal bool TryGetSharedInputControllerForSimulation(out ILF2Controller controller)
        {
            controller = null;

            if (this is LF2LivingObject living)
                controller = living.Controller;
            else if (this is LF2WeaponBase weapon)
                controller = weapon.Controller;

            return controller?.InputBuffer != null;
        }

        internal virtual void EnsureSharedCharacterDatControllerForSimulation()
        {
        }

        /// <summary>
        /// 把共享 controller 的输入缓冲滚入运行时输入快照。
        /// 结果菜单、battle-entry 清输入后的重新采样、post-cooldown 输入消费都可以复用这条入口。
        /// </summary>
        internal void UpdateSharedRuntimeInputSnapshotForSimulation(int tickIndex)
        {
            Runtime.RollInputFromCurrent();
            Runtime.TickInputCooldowns();

            if (!TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
                return;

            UpdateSharedRuntimeInputSnapshotFromBuffer(controller.InputBuffer, tickIndex);
        }

        private void UpdateSharedRuntimeInputSnapshotFromBuffer(SimInputBuffer inputBuffer, int tickIndex)
        {
            if (inputBuffer == null || !inputBuffer.TryDequeueAll(tickIndex, out System.Collections.Generic.List<SimInputEvent> events))
                return;

            for (int i = 0; i < events.Count; i++)
                ApplySharedRuntimeInputEvent(events[i].key, events[i].down);
        }

        private void RunSharedCharacterDatFrameJumpInputPhase()
        {
            if (Runtime == null)
                return;

            sharedCharacterDatInputModule.SyncFromRuntime(Runtime);
            sharedCharacterDatInputModule.ApplyFrameInput(this);
        }

        /// <summary>
        /// shared character-DAT 的最小 standing/walking 动作桥。
        /// 这里只补不依赖 `LF2CharacterActionResolver` 的基础 walk-run/attack/jump/defend 入口，
        /// 不扩到 running/dash/catching/held-weapon/release 全动作解析。
        /// </summary>
        private void RunSharedCharacterDatStandingActionInputPhase()
        {
            if (Runtime == null || this is LF2Character)
                return;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            ApplySharedCharacterDatSpecialStateLaneControl();

            if (TryRunSharedCharacterDatJumpAttackInputPhase())
                return;
            if (TryRunSharedCharacterDatCrouchInputPhase())
                return;
            if (TryRunSharedCharacterDatDefensiveRecoveryInputPhase())
                return;
            if (TryRunSharedCharacterDatRunningInputPhase())
                return;
            if (TryRunSharedCharacterDatDashAttackInputPhase())
                return;

            if ((Frame?.N ?? -1) == LF2StandardFrames.Defend)
            {
                // 参考 C# `ApplyCharacterInput(...)`：
                // frame 110 会先按左右输入刷新 facing，然后再继续走 standing-like 输入消费。
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("right");
                else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                    SwitchDir("left");
            }

            int state = Frame?.D?.state ?? -1;
            if (state != LF2States.Standing && state != LF2States.Walking)
                return;

            if (TryRunSharedCharacterDatHeavyWalkInputPhase())
                return;

            ApplySharedCharacterDatWalkRunMovement();

            if (TryRunSharedCharacterDatStandingAttackAction())
                return;
            if (TryRunSharedCharacterDatStandingJumpAction())
                return;

            TryRunSharedCharacterDatStandingDefendAction();
        }

        private bool TryRunSharedCharacterDatStandingAttackAction()
        {
            if (!IsSharedCharacterDatAttackInputReadyInternal())
                return false;

            int linkState = Runtime?.LinkState ?? 0;
            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            if (HitConfirmCounter > 0 &&
                linkState == 0 &&
                FrameCache?.HasFrame(LF2StandardFrames.SuperPunch) == true &&
                TryCharacterDatInputFrameJump(LF2StandardFrames.SuperPunch))
            {
                return true;
            }

            if (linkState == 0)
            {
                bool usePunch = BattleRandInt(0, 2) == 0;
                int primary = usePunch ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                int fallback = usePunch ? LF2StandardFrames.Punch4 : LF2StandardFrames.Punch;
                return TryRunSharedCharacterDatStandingActionFrame(primary, fallback);
            }

            if (linkState == 101)
            {
                int primary = HasAnyDirectionInputForSharedCharacterDat()
                    ? LF2StandardFrames.LightWeaponThw
                    : RandomSharedCharacterDatWeaponAttackFrame();
                int fallback = primary == LF2StandardFrames.LightWeaponThw
                    ? 0
                    : LF2StandardFrames.LightWeaponThw;
                return TryRunSharedCharacterDatStandingActionFrame(primary, fallback);
            }

            if (linkState == 2)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.HeavyWeaponThw);

            if (linkState % 100 == 1)
                return TryRunSharedCharacterDatStandingActionFrame(RandomSharedCharacterDatWeaponAttackFrame());

            if (linkState == 4)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.LightWeaponThw);

            if (linkState == 6)
                return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.SkyLgtWpThw);

            return false;
        }

        private bool TryRunSharedCharacterDatStandingJumpAction()
        {
            if (!IsSharedCharacterDatJumpInputReadyInternal())
                return false;

            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.Jumping);
        }

        private bool TryRunSharedCharacterDatStandingDefendAction()
        {
            if (!IsSharedCharacterDatDefendInputReadyInternal(requireDefendLockOpen: true))
                return false;

            Runtime.AnimSub = 0;
            AttackingCounter = 0;
            return TryRunSharedCharacterDatStandingActionFrame(LF2StandardFrames.Defend);
        }

        private bool TryRunSharedCharacterDatHeavyWalkInputPhase()
        {
            if (Runtime == null)
                return false;

            int state = Frame?.D?.state ?? -1;
            if (Runtime.LinkState != 2 || (state != LF2States.Standing && state != LF2States.Walking))
                return false;

            ApplySharedCharacterDatHeavyWalkMovement();

            if (IsSharedCharacterDatAttackInputReadyInternal() &&
                FrameCache?.HasFrame(LF2StandardFrames.HeavyWeaponThw) == true)
            {
                Runtime.AnimSub = 0;
                AttackingCounter = 0;
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.HeavyWeaponThw);
            }

            return true;
        }

        private void ApplySharedCharacterDatWalkRunMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null || Runtime.YInt != 0)
                return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1)
                rate = 1;

            int animSub = Runtime.AnimSub;
            if (animSub > 0)
                Runtime.AnimSub--;
            else if (animSub < 0)
                Runtime.AnimSub++;

            bool handled = false;
            bool vxSet = false;
            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
            {
                handled = true;
                if (Runtime.Dir == "left")
                    Runtime.AnimSub = 0;

                SwitchDir("right");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);
                Runtime.Vx = characterData.walking_speed;
                vxSet = true;

                if (Runtime.PrevRight == 0)
                    Runtime.AnimSub += 10;
                if (Runtime.AnimSub >= 11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.RunningStart);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (!handled && Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
            {
                if (Runtime.Dir == "right")
                    Runtime.AnimSub = 0;

                SwitchDir("left");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);
                Runtime.Vx = -characterData.walking_speed;
                vxSet = true;

                if (Runtime.PrevLeft == 0)
                    Runtime.AnimSub -= 10;
                if (Runtime.AnimSub <= -11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.RunningStart);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
            {
                if (!vxSet)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);

                Runtime.Vz = -characterData.walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
            else if (Runtime.KeyDown != 0 && Runtime.KeyUp == 0)
            {
                if (!vxSet)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.WalkingStart);

                Runtime.Vz = characterData.walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
        }

        private void ApplySharedCharacterDatHeavyWalkMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null || Runtime.YInt != 0)
                return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1)
                rate = 1;

            int animSub = Runtime.AnimSub;
            if (animSub > 0)
                Runtime.AnimSub--;
            else if (animSub < 0)
                Runtime.AnimSub++;

            if ((Frame?.N ?? -1) < LF2StandardFrames.HeavyObjWalk0)
                SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);

            bool hasHorizontalMove = false;
            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
            {
                hasHorizontalMove = true;
                if (Runtime.Dir == "left")
                    Runtime.AnimSub = 0;

                SwitchDir("right");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);
                Runtime.Vx = characterData.heavy_walking_speed;

                if (Runtime.PrevRight == 0)
                    Runtime.AnimSub += 10;
                if (Runtime.AnimSub >= 11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjRun);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
            {
                hasHorizontalMove = true;
                if (Runtime.Dir == "right")
                    Runtime.AnimSub = 0;

                SwitchDir("left");
                StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);
                Runtime.Vx = -characterData.heavy_walking_speed;

                if (Runtime.PrevLeft == 0)
                    Runtime.AnimSub -= 10;
                if (Runtime.AnimSub <= -11)
                {
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.HeavyObjRun);
                    Runtime.AnimCounter = 0;
                    Runtime.AnimSub = 0;
                }
            }

            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
            {
                if (!hasHorizontalMove)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);

                Runtime.Vz = -characterData.heavy_walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
            else if (Runtime.KeyDown != 0 && Runtime.KeyUp == 0)
            {
                if (!hasHorizontalMove)
                    StepSharedCharacterDatWalkAnimation(rate, LF2StandardFrames.HeavyObjWalk0);

                Runtime.Vz = characterData.heavy_walking_speedz;
                Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
        }

        private bool TryRunSharedCharacterDatStandingActionFrame(int primaryFrameId, int fallbackFrameId = 0)
        {
            if (TryCharacterDatInputFrameJump(primaryFrameId))
                return true;

            if (fallbackFrameId > 0)
                return TryCharacterDatInputFrameJump(fallbackFrameId);

            return false;
        }

        /// <summary>
        /// shared character-DAT 的最小 jump attack 输入桥。
        /// 参考正式 C++ release `state_jumping`，这里只补无持有态空中 `key_jump -> frame 80`。
        /// </summary>
        private bool TryRunSharedCharacterDatJumpAttackInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Jump || Runtime.YInt >= 0)
                return false;
            if (Runtime.KeyJump == 0)
                return false;

            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                SwitchDir("right");
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                SwitchDir("left");

            int linkState = Runtime.LinkState;
            if (linkState == 0)
            {
                AttackingCounter = 0;
                if (!TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.JumpAttack, clampOnOverdraw: true))
                    return false;

                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.JumpAttack);
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if (linkState % 100 == 1)
            {
                AttackingCounter = 0;
                SetSharedCharacterDatInputFrameDirect(
                    hasDirection ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
                return true;
            }

            if (linkState == 4 || linkState == 6)
            {
                SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.SkyLgtWpThw);
                return true;
            }

            return false;
        }

        /// <summary>
        /// shared character-DAT 的最小 running 输入桥。
        /// 当前补 stop-running、run attack、running defend、running jump，
        /// 以及 release 风格的共享 held running 分支。
        /// </summary>
        private bool TryRunSharedCharacterDatRunningInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Running)
                return false;
            if (Runtime.LinkState == 2)
            {
                ApplySharedCharacterDatHeavyRunningMovement();

                if (IsSharedCharacterDatAttackInputReadyInternal())
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.HeavyWeaponThw);

                return true;
            }

            ApplySharedCharacterDatRunningMovement();

            if (IsSharedCharacterDatAttackInputReadyInternal())
            {
                int linkState = Runtime.LinkState;
                bool hasDirection = HasAnyDirectionInputForSharedCharacterDat();

                if (linkState % 100 == 1)
                {
                    SetSharedCharacterDatInputFrameDirect(
                        hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.RunWeaponAtck);
                }
                else if (linkState == 4)
                {
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.LightWeaponThw);
                }
                else if (linkState == 6)
                {
                    SetSharedCharacterDatInputFrameDirect(
                        hasDirection ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.SkyLgtWpThw);
                }
                else if (TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.RunAttack))
                {
                    SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.RunAttack);
                }
            }

            if (IsSharedCharacterDatDefendInputReadyInternal())
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.Rowing2);

            if (IsSharedCharacterDatJumpInputReadyInternal())
            {
                LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
                if (characterData == null)
                    return true;

                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashForward);
                Runtime.AnimSub = 0;
                Runtime.Vx = Runtime.Dir == "right"
                    ? characterData.dash_distance
                    : -characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
            }

            return true;
        }

        /// <summary>
        /// shared character-DAT 的最小 normal running 基线移动。
        /// 这里只补跑动帧推进、速度写入、斜向 lane 速度和反向 stop-running 前置帧维护，
        /// 不覆盖后续的 stop-running / dash / run-attack 分支。
        /// </summary>
        private void ApplySharedCharacterDatRunningMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null)
                return;

            AttackingCounter = 0;

            int rate = characterData.running_frame_rate;
            if (rate < 1)
                rate = 1;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 4);
            Runtime.AnimCounter = animCounter;

            int frameId = LF2StandardFrames.RunningStart + (animCounter / rate);
            if ((animCounter / rate) >= 3)
                frameId = LF2StandardFrames.Running1;

            if (Runtime.Dir == "right")
            {
                Runtime.Vx = characterData.running_speed;
                if (Runtime.KeyLeft != 0)
                    frameId = LF2StandardFrames.StopRunning;
            }
            else
            {
                Runtime.Vx = -characterData.running_speed;
                if (Runtime.KeyRight != 0)
                    frameId = LF2StandardFrames.StopRunning;
            }

            ApplySharedCharacterDatRunLane(characterData.running_speedz);
            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void ApplySharedCharacterDatHeavyRunningMovement()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null || Runtime == null)
                return;

            AttackingCounter = 0;

            int rate = characterData.running_frame_rate;
            if (rate < 1)
                rate = 1;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 4);
            Runtime.AnimCounter = animCounter;

            int frameId = LF2StandardFrames.HeavyObjRun + (animCounter / rate);
            if ((animCounter / rate) >= 3)
                frameId = LF2StandardFrames.TreeJump0;

            if (Runtime.Dir == "right")
            {
                Runtime.Vx = characterData.heavy_running_speed;
                if (Runtime.KeyLeft != 0)
                    frameId = LF2StandardFrames.TreeJump2;
            }
            else
            {
                Runtime.Vx = -characterData.heavy_running_speed;
                if (Runtime.KeyRight != 0)
                    frameId = LF2StandardFrames.TreeJump2;
            }

            bool upPressed = Runtime.KeyUp != 0 && Runtime.KeyDown == 0;
            bool downPressed = Runtime.KeyDown != 0 && Runtime.KeyUp == 0;
            if (upPressed)
            {
                Runtime.Vz = -characterData.heavy_running_speedz;
                Runtime.Vx *= 5.0 / 6.0;
            }
            else if (downPressed)
            {
                Runtime.Vz = characterData.heavy_running_speedz;
                Runtime.Vx *= 5.0 / 6.0;
            }

            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void ApplySharedCharacterDatRunLane(float speedZ)
        {
            if (Runtime == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
            {
                Runtime.Vz = -speedZ;
                Runtime.Vx *= 5.0 / 6.0;
            }
            else if (downPressed && !upPressed)
            {
                Runtime.Vz = speedZ;
                Runtime.Vx *= 5.0 / 6.0;
            }
        }

        /// <summary>
        /// shared character-DAT 的最小 crouch 输入桥。
        /// 这里只补 `frame 215` 的 defend / crouch-dash 分支。
        /// release `ApplyFrame215Landing(...)` 的 dash branch 没有 `LinkState` gate，
        /// 所以 transformed character-DAT 的 non-LF2Character shell 在 held 路径下也必须能进 dash。
        /// </summary>
        private bool TryRunSharedCharacterDatCrouchInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.N ?? -1) != LF2StandardFrames.Crouch)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return false;

            bool handled = false;
            if (IsSharedCharacterDatDefendInputReadyInternal())
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.Rowing2);
                handled = true;
            }

            bool jumpReady = IsSharedCharacterDatJumpInputReadyInternal();
            bool rightPressed = Runtime.KeyRight != 0;
            bool leftPressed = Runtime.KeyLeft != 0;

            if ((rightPressed || Runtime.Vx > 0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(
                    Runtime.Dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                Runtime.AnimSub = 0;
                Runtime.Vx = characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                handled = true;
            }
            else if ((leftPressed || Runtime.Vx < -0.001f) && jumpReady)
            {
                QueueBattleSound("SFX_017");
                SetSharedCharacterDatInputFrameDirect(
                    Runtime.Dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
                Runtime.AnimSub = 0;
                Runtime.Vx = -characterData.dash_distance;
                Runtime.Vy = characterData.dash_height;
                ApplySharedCharacterDatDashLane(characterData.dash_distancez);
                handled = true;
            }

            ApplySharedCharacterDatDashLane(characterData.dash_distancez);

            return handled;
        }

        /// <summary>
        /// shared character-DAT 的最小倒地 recovery 输入桥。
        /// 这里只补 `FallingFront2/FallingBack2 + KeyDefend + CdJump` 的 recovery 分支。
        /// </summary>
        private bool TryRunSharedCharacterDatDefensiveRecoveryInputPhase()
        {
            if (Runtime == null)
                return false;

            int frameId = Frame?.N ?? -1;
            if (frameId != LF2StandardFrames.FallingFront2 && frameId != LF2StandardFrames.FallingBack2)
                return false;
            if (WeaponCount < 0 || !IsSharedCharacterDatJumpInputReadyInternal() || Health?.HP <= 0)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            bool backward = Runtime.Dir == "right" ? Runtime.Vx <= 0f : Runtime.Vx >= 0f;
            SetSharedCharacterDatInputFrameDirect(
                backward ? LF2StandardFrames.Rowing : LF2StandardFrames.RowingBack);
            AttackingCounter = 0;

            if (characterData == null)
                return true;

            if (Runtime.Vy > characterData.rowing_height)
                Runtime.Vy = characterData.rowing_height;

            float rowingDistance = characterData.rowing_distance;
            if (Runtime.Vx > -1f && Runtime.Vx < 1f)
                Runtime.Vx = Runtime.Dir == "left" ? rowingDistance : -rowingDistance;
            else
                Runtime.Vx = Runtime.Vx > 0f ? rowingDistance : -rowingDistance;

            return true;
        }

        /// <summary>
        /// shared character-DAT 的最小 dash attack 输入桥。
        /// 这里按正式 C++ release `state_dash` 只补已确认的最小 held 分支：
        /// 无持有态 `DashAttack`、`linkState % 100 == 1 -> DashWeaponAtck`、
        /// `linkState == 4/6 && hasDirection -> SkyLgtWpThw`。
        /// </summary>
        private bool TryRunSharedCharacterDatDashAttackInputPhase()
        {
            if (Runtime == null)
                return false;
            if ((Frame?.D?.state ?? -1) != LF2States.Dash)
                return false;

            ApplySharedCharacterDatDashFrameMaintenance();

            if (Runtime.KeyJump == 0)
                return false;

            bool dashForward = (Runtime.Dir == "right" && Runtime.Vx > 0f) ||
                               (Runtime.Dir == "left" && Runtime.Vx < 0f);
            if (!dashForward)
                return false;

            int linkState = Runtime.LinkState;
            if (linkState == 0)
            {
                if (!TrySpendSharedCharacterDatFramePpCost(LF2StandardFrames.DashAttack))
                    return false;

                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashAttack);
                return true;
            }

            if (linkState % 100 == 1)
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.DashWeaponAtck);
                Runtime.Vy -= 1f;
                AttackingCounter = 0;
                return true;
            }

            bool hasDirection = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
            if ((linkState == 4 || linkState == 6) && hasDirection)
            {
                SetSharedCharacterDatInputFrameDirect(LF2StandardFrames.SkyLgtWpThw);
                Runtime.Vy -= 1f;
                AttackingCounter = 0;
                return true;
            }

            return false;
        }

        private void ApplySharedCharacterDatDashFrameMaintenance()
        {
            if (Runtime == null)
                return;

            if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                SwitchDir("right");
            else if (Runtime.KeyLeft != 0 && Runtime.KeyRight == 0)
                SwitchDir("left");

            bool facingRight = Runtime.Dir == "right";
            if (facingRight)
            {
                if (Frame.N != LF2StandardFrames.DashBack2 && Runtime.Vx < 0f)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (Runtime.Vx > 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward);
            }
            else
            {
                if (Runtime.Vx > 0f && Frame.N != LF2StandardFrames.DashBack2)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (Runtime.Vx < 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetSharedCharacterDatMoveFrameDirect(LF2StandardFrames.DashForward);
            }
        }

        private bool HasAnyDirectionInputForSharedCharacterDat()
        {
            return Runtime != null &&
                   (Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0);
        }

        private int RandomSharedCharacterDatWeaponAttackFrame()
        {
            return BattleRandInt(0, 2) == 0
                ? LF2StandardFrames.NormalWeaponAtck
                : LF2StandardFrames.NormalWeaponAtck2;
        }

        private void StepSharedCharacterDatWalkAnimation(int rate, int frameBase)
        {
            if (Runtime == null)
                return;

            int animCounter = Runtime.AnimCounter;
            animCounter = (animCounter + 1) % (rate * 6);
            Runtime.AnimCounter = animCounter;

            int fi = animCounter / rate;
            int frameId = fi < 4 ? frameBase + fi : frameBase + (6 - fi);
            SetSharedCharacterDatMoveFrameDirect(frameId);
        }

        private void SetSharedCharacterDatMoveFrameDirect(int frameId)
        {
            if (Frame == null || FrameCache == null || Runtime == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return;

            Frame.N = frameId;
            Runtime.FrameWaitCounter = 0;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            Runtime.NextFrame = Frame.D.next;
        }

        private bool SetSharedCharacterDatInputFrameDirect(int frameId)
        {
            if (Frame == null || FrameCache == null || Runtime == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            Frame.N = frameId;
            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            Runtime.NextFrame = targetFrame.next;
            return true;
        }

        private void ApplySharedCharacterDatSpecialStateLaneControl()
        {
            if (Runtime == null || GetRuntimeYInt() != 0)
                return;

            int state = Frame?.D?.state ?? -1;
            if (state != LF2States.DeepSpecific && state != LF2States.FirenSpecific)
                return;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
                Runtime.Vz = -characterData.running_speedz;
            else if (downPressed && !upPressed)
                Runtime.Vz = characterData.running_speedz;
        }

        private void ApplySharedCharacterDatDashLane(float dashDistanceZ)
        {
            if (Runtime == null)
                return;

            bool upPressed = Runtime.KeyUp != 0;
            bool downPressed = Runtime.KeyDown != 0;
            if (upPressed && !downPressed)
                Runtime.Vz = -dashDistanceZ;
            else if (downPressed && !upPressed)
                Runtime.Vz = dashDistanceZ;
        }

        protected bool TrySpendSharedCharacterDatFramePpCost(int frameId, bool clampOnOverdraw = false)
        {
            if (!IsPpModeEnabled() || Health == null)
                return true;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            int ppCost = targetFrame.mp;
            if (!clampOnOverdraw && Health.PP < ppCost)
                return false;

            Health.PP -= ppCost;
            if (Health.PP >= 0)
            {
                SpendPpDisplay(ppCost);
            }
            else
            {
                Health.PP = 0;
            }

            return true;
        }

        private void ApplySharedRuntimeInputEvent(FuncKeyMask key, bool down, bool forceFreshEdge = false)
        {
            if (forceFreshEdge && down)
                ForceSharedRuntimePreviousState(key);

            switch (key)
            {
                case FuncKeyMask.right: Runtime.KeyRight = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.left: Runtime.KeyLeft = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.up: Runtime.KeyUp = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.down: Runtime.KeyDown = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.att: Runtime.KeyAttack = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.jump: Runtime.KeyJump = down ? (byte)1 : (byte)0; break;
                case FuncKeyMask.def: Runtime.KeyDefend = down ? (byte)1 : (byte)0; break;
            }

            if (!down)
                return;

            // shared character-DAT 输入镜像也要保持 reference 的交叉 cooldown 语义：
            // attack -> CdDefend, defend -> CdJump, jump -> CdAttack。
            switch (key)
            {
                case FuncKeyMask.right:
                    if (Runtime.PrevRight == 0)
                    {
                        Runtime.CdRight = 5;
                        Runtime.PushInputHistory(6);
                    }
                    break;
                case FuncKeyMask.left:
                    if (Runtime.PrevLeft == 0)
                    {
                        Runtime.CdLeft = 5;
                        Runtime.PushInputHistory(4);
                    }
                    break;
                case FuncKeyMask.up:
                    if (Runtime.PrevUp == 0)
                    {
                        Runtime.CdUp = 5;
                        Runtime.PushInputHistory(8);
                    }
                    break;
                case FuncKeyMask.down:
                    if (Runtime.PrevDown == 0)
                    {
                        Runtime.CdDown = 5;
                        Runtime.PushInputHistory(2);
                    }
                    break;
                case FuncKeyMask.att:
                    if (Runtime.PrevAttack == 0)
                    {
                        Runtime.CdDefend = 5;
                        Runtime.PushInputHistory(9);
                    }
                    break;
                case FuncKeyMask.jump:
                    if (Runtime.PrevJump == 0)
                    {
                        Runtime.CdAttack = 5;
                        Runtime.PushInputHistory(5);
                    }
                    break;
                case FuncKeyMask.def:
                    if (Runtime.PrevDefend == 0)
                    {
                        Runtime.CdJump = 5;
                        Runtime.PushInputHistory(0);
                    }
                    break;
            }
        }

        private void ForceSharedRuntimePreviousState(FuncKeyMask key)
        {
            switch (key)
            {
                case FuncKeyMask.right: Runtime.PrevRight = 0; break;
                case FuncKeyMask.left: Runtime.PrevLeft = 0; break;
                case FuncKeyMask.up: Runtime.PrevUp = 0; break;
                case FuncKeyMask.down: Runtime.PrevDown = 0; break;
                case FuncKeyMask.att: Runtime.PrevAttack = 0; break;
                case FuncKeyMask.jump: Runtime.PrevJump = 0; break;
                case FuncKeyMask.def: Runtime.PrevDefend = 0; break;
            }
        }

        /// <summary>
        /// 供“当前 DAT 是 Character”的通用输入消费链使用的 DJA guard。
        /// 这层判断只依赖共享 runtime / frame 数据，不要求 CLR 类型真的是 LF2Character。
        /// </summary>
        internal bool ShouldHoldCharacterDatDjaInputGuard(int targetFrame)
        {
            if (ObjectId != 6 || targetFrame != 300 || Health == null || Health.HP <= 177)
                return false;

            return Match?.Runtime?.Flow?.DjaGuardGlobal44F224 == 0;
        }

        internal bool CanEnterCharacterDatInputFrameJump()
        {
            return TransformOriginalObjectId == -1 && Runtime.LinkState != 2;
        }

        /// <summary>
        /// 通用输入跳帧入口。
        /// 参考 C# `DoFrameJump(...)`，用于当前 DAT 已经是 Character 的任意实体。
        /// </summary>
        internal bool TryCharacterDatInputFrameJump(int frameId)
        {
            bool flipFacing = false;
            if (frameId < 0)
            {
                frameId = -frameId;
                flipFacing = true;
            }

            if (frameId == 999)
                frameId = 0;

            if (FrameCache?.HasFrame(frameId) != true || Health == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            bool ppMode = IsPpModeEnabled();
            if (ppMode)
            {
                int ppCost = targetFrame.mp % 1000;
                int hpCost = (targetFrame.mp / 1000) * 10;
                if (Health.PP < ppCost || Health.HP <= hpCost)
                    return false;

                Health.HP -= hpCost;
                Health.PP -= ppCost;
                ComboCountVic += hpCost;
                SpendPpDisplay(ppCost);
            }

            if (flipFacing && ppMode)
                SwitchDir(Runtime.Dir == "right" ? "left" : "right");

            return SetSharedCharacterDatInputFrameDirect(frameId);
        }

        /// <summary>
        /// 判断当前实体是否满足 N30 晚阶段输入触发条件。
        /// 这里按“当前 DAT 是否还是角色”判断，而不是按 CLR 子类判断。
        /// </summary>
        internal bool TryResolveLateN30InputTriggerCode(out int frameVal)
        {
            frameVal = 0;

            int slotIndex = Runtime?.SlotIndex ?? -1;
            if (slotIndex < 0 || slotIndex >= 10)
                return false;
            if (Health == null || Health.HP <= 0)
                return false;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return false;

            int[] history = Runtime?.InputHistory;
            if (history == null || history.Length < 6)
                return false;

            int a = history[2];
            int b = history[3];
            int c = history[4];
            int d = history[5];
            if (a == 9 && b == 0 && c == 9 && d == 0) frameVal = 100;
            else if (a == 9 && b == 9 && c == 9 && d == 9) frameVal = 102;
            else if (a == 9 && b == 5 && c == 9 && d == 5) frameVal = 104;

            return frameVal != 0;
        }

        /// <summary>
        /// 处理当前 DAT 仍是角色对象时的晚阶段 N30 输入触发。
        /// 参考实现按 slot + 当前 DAT 类型参与，所以不能只挂在 LF2Character 上。
        /// </summary>
        private void RunLateCharacterDatInputTrigger()
        {
            if (!TryResolveLateN30InputTriggerCode(out int frameVal))
                return;

            Runtime?.ClearInputHistoryTail();

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            int slotIndex = Runtime?.SlotIndex ?? -1;
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            ConfigureLateN30SpawnTask(task, slotIndex, frameVal);

            LF2Entity spawned = factory.CreateObjectImmediate(task);
            if (spawned == null)
                return;

            ApplyLateN30HistoryGateBroadcast(frameVal, spawned);
        }

        /// <summary>
        /// 统一写入晚阶段 N30 触发生成 998 效果时的运行时身份。
        /// Unity 侧同阵营筛选已经以 `RelationTeam -> Team` 作为当前真值，
        /// 所以这里的 effect 任务也必须沿用同一套来源，不能继续把 `team` 留成 0。
        /// </summary>
        private void ApplyLateN30SpawnIdentity(OPointCreateTask task, int slotIndex)
        {
            if (task == null)
                return;

            int sourceTeam = ResolveN30HistoryGateTeam(this);
            task.team = sourceTeam;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = sourceTeam;
            task.holderCopySlot = -1;
            task.spawnerEntityIndex = slotIndex;
        }

        /// <summary>
        /// 晚阶段 N30 生成的 `oid=998` 属于立即特效路径。
        /// 这类 task 的 `z` 已经直接编码了参考实现最终可见 Z，
        /// 不能再吃工厂通用的 post-init `Z+1` 抬高。
        /// </summary>
        private void ConfigureLateN30SpawnTask(OPointCreateTask task, int slotIndex, int frameVal)
        {
            if (task == null)
                return;

            task.opoint = new ObjectPoint { oid = 998, kind = 0, action = frameVal, facing = 0 };
            task.parent = null;
            ApplyLateN30SpawnIdentity(task, slotIndex);
            task.pos = new Vector3(GetRuntimeXInt(), 0f, GetRenderZInt());
            task.z = GetRenderZInt();
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = GetRuntimeXInt();
            task.initialRuntimeY = 0;
            task.initialRuntimeZ = GetRenderZInt();
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;
        }

        /// <summary>
        /// N30 晚阶段除了生成 998 效果外，100 写入同 Unk364 角色的
        /// 随机坐标，102 打开 history gate，104 关闭 history gate。
        /// </summary>
        internal void ApplyLateN30HistoryGateBroadcast(int frameVal, LF2Entity spawned = null)
        {
            if (frameVal != 100 && frameVal != 102 && frameVal != 104)
                return;

            SimulationWorld world = Match;
            if (world == null)
                return;

            int sourceTeam = frameVal == 100 ? RelationTeam : ResolveN30HistoryGateTeam(this);
            if (sourceTeam == 0 && frameVal != 100)
                return;

            // C# authority writes the spawned effect's integer coordinates, then
            // consumes exactly two RNG values for every eligible same-Unk364
            // living character when triggerCode=100.
            int spawnX = spawned?.Runtime?.XInt ?? Runtime?.XInt ?? 0;
            int spawnZ = spawned?.Runtime?.ZInt ?? Runtime?.ZInt ?? 0;

            bool enabled = frameVal == 102;
            N30HistoryGateScratch.Clear();
            world.GetAllEntities(N30HistoryGateScratch);

            try
            {
                for (int i = 0; i < N30HistoryGateScratch.Count; i++)
                {
                    LF2Entity teammate = N30HistoryGateScratch[i];
                    if (teammate == null || teammate.Runtime == null || teammate.Health == null)
                        continue;
                    if (teammate.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                        continue;
                    if (teammate.Health.HP <= 0)
                        continue;
                    int teammateTeam = frameVal == 100 ? teammate.RelationTeam : ResolveN30HistoryGateTeam(teammate);
                    if (teammateTeam != sourceTeam)
                        continue;

                    if (frameVal == 100)
                    {
                        teammate.Runtime.Unk3FC = spawnX + (world.Rng.NextRaw() % 0x51) - 0x28;
                        teammate.Runtime.Unk400 = spawnZ + (world.Rng.NextRaw() % 0x51) - 0x28;
                    }
                    else
                    {
                        teammate.Runtime.SetInputHistoryGate(enabled);
                    }
                }
            }
            finally
            {
                N30HistoryGateScratch.Clear();
            }
        }

        private static int ResolveN30HistoryGateTeam(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }

        /// <summary>
        /// 早期 state 400/401 传送特判入口。
        /// C++ release 只要求 source active 且有当前 frame；候选 target 才要求 Character DAT。
        /// source 不能按 CLR 类型或当前 DAT 类型提前排除。
        /// </summary>
        internal virtual void RunEarlyTeleportSpecialsPhase(System.Collections.Generic.List<LF2Entity> entities, bool frameToggleGate)
        {
            if (frameToggleGate || entities == null || Health == null)
                return;

            int state = Frame?.D?.state ?? -1;
            bool toEnemy = state == LF2States.TeleportToEnemy;
            bool toTeammate = state == LF2States.TeleportToTeammate;
            if (!toEnemy && !toTeammate)
                return;

            LF2Entity best = null;
            int bestDistance = toEnemy ? 10000 : -1;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity target = entities[i];
                if (target == null || target.Health == null)
                    continue;
                if (target.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;
                if (target.Health.HP <= 0)
                    continue;
                if (toEnemy && target.RelationTeam == RelationTeam)
                    continue;
                if (toTeammate && target.RelationTeam != RelationTeam)
                    continue;

                int distance = Mathf.Abs(target.GetRenderZInt() - GetRenderZInt()) +
                               Mathf.Abs(target.GetRuntimeXInt() - GetRuntimeXInt());
                if (toEnemy && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
                else if (toTeammate && distance > bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            if (best == null)
            {
                Runtime.Y = 0f;
                Runtime.YInt = 0;
                Runtime.Vx = 0f;
                Runtime.Vy = 0f;
                Runtime.Vz = 0f;
                return;
            }

            int offset = toEnemy ? 120 : 60;
            int nextZ = best.GetRenderZInt() + 1;
            int nextX = Runtime.Dir == "right"
                ? best.GetRuntimeXInt() - offset
                : best.GetRuntimeXInt() + offset;

            Runtime.Z = nextZ;
            Runtime.ZInt = nextZ;
            Runtime.X = nextX;
            Runtime.XInt = nextX;
            Runtime.Y = 0f;
            Runtime.YInt = 0;
            Runtime.Vx = 0f;
            Runtime.Vy = 0f;
            Runtime.Vz = 0f;
        }

        internal bool RunEarlyTeleportSpecialsPhaseWithMutationReport(
            System.Collections.Generic.List<LF2Entity> entities,
            bool frameToggleGate)
        {
            // LF2Character's production override delegates directly to this base
            // implementation. Unknown derived shells remain fail-closed because an
            // override may add writes outside the teleport runtime fields.
            bool canReportExactly = GetType() == typeof(LF2Character);
            int state = Frame?.D?.state ?? -1;
            bool stalePublishedSnapshot =
                canReportExactly &&
                !IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp();
            bool writesTeleportRuntime =
                canReportExactly &&
                !frameToggleGate &&
                entities != null &&
                Health != null &&
                (state == LF2States.TeleportToEnemy ||
                 state == LF2States.TeleportToTeammate);

            RunEarlyTeleportSpecialsPhase(entities, frameToggleGate);
            return !canReportExactly ||
                   stalePublishedSnapshot ||
                   writesTeleportRuntime;
        }

        internal virtual void RunLateDeathOpointPreCleanupPhase()
        {
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;
            if (Health == null || Health.HP > 0 || Runtime == null)
                return;

            DropHeldObjectForCurrentDatDeath();

            int frameId = Frame?.N ?? -1;
            if (frameId < 12 || frameId == 110 || frameId == 111)
                EnterCurrentDatDeathBounceFrame();

            if (Runtime.YInt == 0 && Runtime.Y == 0.0 && Runtime.Vy == 0.0 && KnockbackVy == 0.0)
            {
                int currentFrame = Frame?.N ?? -1;
                bool groundDeathFrame =
                    (currentFrame >= 180 && currentFrame <= 189 && currentFrame != 184) ||
                    (currentFrame >= 212 && currentFrame <= 214);
                if (groundDeathFrame)
                    EnterCurrentDatDeathBounceFrame();
            }
        }

        internal virtual bool TryRunLatePostOpointCleanupPhase()
        {
            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character || Runtime == null ||
                Runtime.WeaponFlightCounter >= 0)
            {
                return false;
            }

            Runtime.WeaponFlightCounter = 0;
            QueueBattleSound(FrameCache?.Wrapper?.characterData?.weapon_broken_sound);
            Runtime.PendingFlushDestroy = true;
            return true;
        }

        private void DropHeldObjectForCurrentDatDeath()
        {
            if (this is LF2Character character)
            {
                character.ForceDropHeldWeaponForLateDeathInternal();
                return;
            }

            int holderSlot = Runtime?.SlotIndex ?? -1;
            int heldSlot = Runtime?.ResolveActiveHeldSlotIndex() ?? -1;
            LF2Entity held = heldSlot >= 0
                ? Match?.FindEntityByRuntimeSlotForQuery(heldSlot) ??
                  Match?.FindEntityByRuntimeSlotIncludingPending(heldSlot)
                : null;

            Runtime.LinkState = 0;
            Runtime.TargetSlotIndex = -1;
            Runtime.HeldWeaponStableId = -1;
            if (held?.Runtime == null || held.Runtime.HolderStableId != holderSlot)
                return;

            held.Runtime.LinkState = 0;
            held.Runtime.HolderStableId = -1;
            held.HolderCopySlot = 99;
        }

        private void EnterCurrentDatDeathBounceFrame()
        {
            DirectWriteRawFramePreserveWaitCounter(186);
            Runtime.Vy = -3.0;
            KnockbackVy = -3.0;
            Runtime.Y = -1.0;
            Runtime.YInt = -1;
        }

        internal virtual void RunLateTailBeforePrevFrame()
        {
            RunLateCharacterDatInputTrigger();
            SpawnLateTransitionEffects();
        }

        public virtual void MirrorLatePrevFrame()
        {
            if (Frame != null)
                Frame.Prev = Frame.N;
        }

        private void SpawnLateTransitionEffects()
        {
            LF2FrameData prevFrame = GetFrameDataById(Frame?.Prev ?? 0);
            LF2FrameData currentFrame = Frame?.D;
            if (prevFrame == null || currentFrame == null)
                return;

            int prevState = prevFrame.state;
            int currentState = currentFrame.state;
            bool shouldSpawnBranch1 =
                (prevState == 13 || (Frame?.Prev ?? 0) == 200) &&
                currentState != 13 && (Frame?.N ?? 0) != 200;
            bool shouldSpawnBranch2 = prevState == 18 || prevState == 19;
            if (!shouldSpawnBranch1 && !shouldSpawnBranch2)
                return;

            bool spawned = false;
            bool hasEffectResources = LF2ObjectPointFactory.Instance != null &&
                                      ResolveRuntimeCharacterConfig(999) != null;
            int availableSlots = 0;
            bool availableSlotsCalculated = false;

            if (hasEffectResources && shouldSpawnBranch1)
            {
                availableSlots = CountAvailableTransitionEffectSlots();
                availableSlotsCalculated = true;
                Match?.QueueSound("SFX_066", Runtime.XInt);
                spawned |= SpawnTransitionEffectBranch1(ref availableSlots);
            }

            if (!shouldSpawnBranch2)
                return;

            int count = 0;
            if (currentState != 18 && currentState != 19)
                count = 7;
            else if (BattleRandInt(0, 4) == 0)
                count = 1;

            if (count > 0)
            {
                if (hasEffectResources && !availableSlotsCalculated)
                {
                    availableSlots = CountAvailableTransitionEffectSlots();
                    availableSlotsCalculated = true;
                }

                spawned |= SpawnTransitionEffectBranch2(count, ref availableSlots);
            }

            if (spawned)
                RefreshRuntimeSnapshot();
        }

        private bool SpawnTransitionEffectBranch1(ref int availableSlots)
        {
            int initialSlots = availableSlots;
            for (int n = 0; n < 15; n++)
            {
                if (availableSlots <= 0)
                    break;

                double y = Runtime.Y - BattleRandInt(0, 29);
                double x = Runtime.X + BattleRandInt(0, 39) - 19.0;
                double vy = -(BattleRandInt(0, 20) / 2.0) - 8.0;
                double vx = Runtime.Vx * 0.5 + BattleRandInt(0, 11) - 5.0;
                int frameId = n < 2 ? 120 : n < 5 ? 130 : n < 9 ? 125 : 135;
                SpawnTransitionEffect(
                    frameId,
                    x,
                    y,
                    vx,
                    vy);
                availableSlots--;
            }

            return availableSlots < initialSlots;
        }

        private bool SpawnTransitionEffectBranch2(int count, ref int availableSlots)
        {
            int initialSlots = availableSlots;
            for (int n = 0; n < count; n++)
            {
                if (availableSlots <= 0)
                    break;

                double y = Runtime.Y - BattleRandInt(0, 29);
                double x = Runtime.X + BattleRandInt(0, 59) - 29.0;
                double vx = Runtime.Vx + BattleRandInt(0, 11) - 5.0;
                int frameId = 140 + BattleRandInt(0, 1);
                SpawnTransitionEffect(
                    frameId,
                    x,
                    y,
                    vx,
                    -1.0);
                availableSlots--;
            }

            return availableSlots < initialSlots;
        }

        private int CountAvailableTransitionEffectSlots()
        {
            if (Match == null)
                return 350;

            int available = 0;
            for (int slot = Match.DynamicRuntimeSlotStartForServices;
                 slot < Match.MaxRuntimeSlotsForServices;
                 slot++)
            {
                if (Match.FindEntityByRuntimeSlotForQuery(slot) == null)
                    available++;
            }

            return available;
        }

        private void SpawnTransitionEffect(int frameId, double x, double y, double vx, double vy)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = 999,
                kind = 0,
                action = frameId,
                facing = Runtime.Dir == "right" ? 0 : 1,
                x = 0,
                y = 0,
                dvx = 0,
                dvy = 0,
                dvz = 0,
            };
            task.parent = null;
            task.team = Team;
            task.relationTeam = RelationTeam != 0 ? RelationTeam : Team;
            task.useExplicitRelationIdentity = true;
            task.holderCopySlot = -1;
            task.pos = new Vector3((float)x, (float)y, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = Runtime.Dir;
            task.useDirectRuntimePosition = true;
            task.directX = x;
            task.directY = y;
            task.directZ = Runtime.Z;
            task.useDirectVelocity = true;
            task.directVx = vx;
            task.directVy = vy;
            task.directVz = 0.0;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.TransitionEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = Runtime.XInt;
            task.initialRuntimeY = Runtime.YInt;
            task.initialRuntimeZ = Runtime.ZInt;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
            task.skipPostInitZOffset = true;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;

            factory.EnqueueCreateObject(task);
        }

        public virtual void FreeEntityLikeExe()
        {
            Sprite?.Hide();
            Sprite?.HideShadow();
            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            else
            {
                UnregisterFromWorld();
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        public virtual void DirectWriteFramePreserveWaitCounter(int frameId)
        {
            SetFrameTickDirect(frameId);
        }

        internal void DirectWriteRawFramePreserveWaitCounter(int frameId)
        {
            if (Frame == null)
                return;

            Frame.N = frameId;
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
            if (Runtime != null)
                Runtime.Frame = frameId;
        }

        internal void DirectWriteHeldFramePreserveWaitCounter(int frameId)
        {
            if (Frame == null)
                return;

            Frame.N = frameId;
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
        }

        public virtual void DirectWriteFrameImmediateWaitReset(int frameId)
        {
            SetFrameTickImmediateRawDirect(frameId);
        }

        internal void SetFrameLogicRawFramePreserveAttacking(int frameId)
        {
            SetFrameTickDirect(frameId);
        }

        private void ApplyStateDataTransform(int targetObjectId, bool applyHitStop140)
        {
            if (targetObjectId < 0)
                return;

            LF2CharacterDataWrapper wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            Runtime.WeaponFlightCounter = wrapper.characterData?.weapon_hp ?? 0;
            DirectWriteRawFramePreserveWaitCounter(0);

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            if (applyHitStop140)
                HitStun = 140;

            RefreshRuntimeSnapshot();
        }

        internal static LF2CharacterDataWrapper ResolveRuntimeCharacterConfig(int targetObjectId)
        {
            LF2CharacterDataWrapper overrideWrapper = RuntimeCharacterConfigResolverOverride?.Invoke(targetObjectId);
            if (overrideWrapper != null)
                return overrideWrapper;

            return CharacterAnimtorManager.Instance?.GetCharacterConfig(targetObjectId);
        }

        internal bool TryApplyRuntimeIdentity(
            int targetObjectId,
            int targetFrameId,
            bool resetWaitCounter,
            out LF2CharacterDataWrapper wrapper)
        {
            wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return false;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            WeaponCount = wrapper.characterData?.weapon_hp ?? 0;

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            Frame.N = targetFrameId;
            Frame.D = FrameCache.GetFrameDataById(targetFrameId);
            if (Frame.D != null)
            {
                int waitCounter = resetWaitCounter ? 0 : (Trans?.WaitCounter ?? 0);
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
            }

            RefreshRuntimeSnapshot();
            return true;
        }

        internal bool TryReloadCurrentFrameDataForRuntimeIdentity(int targetObjectId)
        {
            LF2CharacterDataWrapper wrapper = ResolveRuntimeCharacterConfig(targetObjectId);
            if (wrapper == null)
                return false;

            ObjectId = targetObjectId;
            FrameCache.Load(wrapper);
            WeaponCount = wrapper.characterData?.weapon_hp ?? 0;

            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                EnsureSharedCharacterDatControllerForSimulation();

            Frame.D = FrameCache.GetFrameDataById(Frame.N);
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);

            RefreshRuntimeSnapshot();
            return true;
        }

        public virtual int GetCurrentDataObjectTypeForSimulation() => ResolveCurrentDataObjectType(this);

        public virtual int GetCurrentDataObjectType() => GetCurrentDataObjectTypeForSimulation();

        /// <summary>
        /// 参考 C# release 的 `ObjTypeRules.ToRuntimeObjType(...)`：
        /// 运行时粗分类只区分“角色”与“非角色”。
        /// Unity 内部仍然保留完整 DAT type 供大多数逻辑使用，
        /// 这里只在 runtime 身份快照/校验层复用 release 语义。
        /// </summary>
        public static int ResolveReferenceRuntimeObjTypeFromDataType(int currentDataType)
        {
            return currentDataType == (int)LF2ObjectType.Character ? 0 : 1;
        }

        /// <summary>
        /// 按当前 DAT 包装器解析对象 type。
        /// C# 基准工程 EntityCategoryResolver 使用 CharData.ObjType，而不是实体子类类型；
        /// Unity 的对象池类型只决定实例来自哪个池，战斗判定必须读取当前 DAT type。
        /// </summary>
        public static int ResolveCurrentDataObjectType(LF2Entity entity)
        {
            if (entity == null)
                return -1;

            int wrapperOid = ResolveCurrentDataObjectId(entity);
            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(wrapperOid);
            return definition?.type ?? entity.ReleaseEntityType;
        }

        /// <summary>
        /// 按当前 DAT 包装器解析对象 oid；没有当前包装器时回退到实体的正式 runtime 身份。
        /// </summary>
        public static int ResolveCurrentDataObjectId(LF2Entity entity)
        {
            return entity?.FrameCache?.Wrapper?.characterId ?? entity?.ObjectId ?? -1;
        }

        public virtual bool ShouldDeferInitialRuntimeSnapshot() => false;

        public virtual LF2FrameData GetCollisionFrameData()
        {
            if (Frame == null || FrameCache == null)
                return null;

            if (FrameCache.HasFrame(Frame.Prev2) && Frame.Prev2D != null)
                return Frame.Prev2D;

            if (FrameCache.HasFrame(Frame.N) && Frame.D != null)
                return Frame.D;

            return null;
        }

        public virtual void CaptureCollisionFrameSnapshot()
        {
            SyncCollisionSnapshotToCurrentFrame();
        }

        internal void SyncCollisionSnapshotToCurrentFrame()
        {
            if (Frame == null)
                return;

            Frame.Prev2 = Frame.N;
            Frame.Prev2D = Frame.D;
            Runtime.PrevFrame2 = Frame.Prev2;
        }

        internal bool ReloadCurrentFrameDataFromWrapper()
        {
            if (Frame == null || FrameCache == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(Frame.N);
            if (targetFrame == null)
                return false;

            Frame.D = targetFrame;
            Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            RefreshRuntimeSnapshot();
            return true;
        }

        public virtual int GetRenderPicIndex()
        {
            int pic = Frame?.D?.pic ?? -1;
            return pic >= 0 ? pic + Runtime.RenderPicOffset : pic;
        }

        public virtual float GetDisplayZ()
        {
            if (GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                Runtime != null &&
                System.Math.Abs(Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (float)(Runtime.Z - Runtime.Type3VisualZOffset);
            }

            return GetRenderZInt();
        }

        public virtual int GetRenderSortingOrder()
        {
            return GetPresentationRenderSortingOrder(SimulationWorld.PresentationEntitySubOrder);
        }

        public int GetHitRecordRenderSortingOrder()
        {
            return GetPresentationRenderSortingOrder(SimulationWorld.PresentationHitRecordSubOrder);
        }

        private int GetPresentationRenderSortingOrder(int subOrder)
        {
            return Match != null
                ? Match.GetPresentationRenderSortingOrder(this, subOrder)
                : subOrder;
        }

        /// <summary>
        /// Renderer-facing entity sub-order. draw_entity position may use its
        /// display Z offset, while release draw ordering remains ZInt/slot.
        /// </summary>
        public int GetDisplayRenderSortingOrder(float displayZ, float zOffset)
        {
            return GetRenderSortingOrder();
        }

        public virtual float GetSpriteWidthPxForRender()
        {
            float width = TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
                ? entry.PixelWidth
                : 0f;
            if (width <= 0f)
                width = GetSpriteWidthPxForCollision();
            return width;
        }

        public virtual float GetSpriteHeightPxForRender()
        {
            return TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
                ? entry.PixelHeight
                : 0f;
        }

        public bool TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
        {
            entry = null;
            int effectivePic = GetRenderPicIndex();
            if (effectivePic < 0 || effectivePic == 999)
                return false;

            int visualDataId = ResolveCurrentDataObjectId(this);
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            return manager != null &&
                   manager.TryGetSpriteEntry(visualDataId, effectivePic, out entry);
        }

        public virtual int GetRuntimeXInt()
        {
            return Runtime.XInt != 0 ? Runtime.XInt : ReleaseInt(Runtime.X);
        }

        public virtual int GetRuntimeYInt()
        {
            return Runtime.YInt != 0 ? Runtime.YInt : ReleaseInt(Runtime.Y);
        }

        public virtual int GetRenderZInt()
        {
            return Runtime.ZInt != 0 ? Runtime.ZInt : ReleaseInt(Runtime.Z);
        }

        public virtual int GetCollisionZInt() => GetCollisionZInt(GetCollisionFrameData());

        public virtual int GetCollisionZInt(LF2FrameData frame)
        {
            if (GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack && Runtime != null)
            {
                if (System.Math.Abs(Runtime.Type3VisualZOffset) > 0.0001)
                    return ReleaseInt(Runtime.Z - Runtime.Type3VisualZOffset);

                if (frame != null && frame.hit_j > 0)
                    return ReleaseInt(Runtime.Z - (frame.hit_j - 50));
            }

            return GetRenderZInt();
        }

        public virtual float GetRenderOffsetX() => Runtime.RenderOffsetX;

        public void QueueBattleSound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            Match?.QueueSound(soundId, GetRuntimeXInt());
        }

        public virtual int ResolveReleaseNeutralHolderSlotOrImplicitZero()
        {
            int slot = HolderCopySlot;
            return slot >= 0 ? slot : 0;
        }

        public virtual int ResolveReleaseNegativeLinkHolderSlotOrImplicitZero()
        {
            int slot = Runtime.HolderStableId;
            if (slot < 0)
                slot = HolderCopySlot;
            return slot >= 0 ? slot : 0;
        }

        protected virtual float ResolveCurrentSpriteFileWidthPx()
        {
            return TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry)
                ? entry.PixelWidth
                : 0f;
        }

        protected virtual bool ShouldRenderAboveCharacters()
        {
            int semantic = Runtime?.SpawnSemantic ?? 0;
            return semantic == (int)ReleaseSpawnSemantic.ImmediateEffect ||
                   semantic == (int)ReleaseSpawnSemantic.TransitionEffect;
        }

        protected virtual bool IsBlockedByReleaseLinkOrCaughtCpoint()
        {
            return Runtime.LinkState < 0;
        }

        protected virtual void ApplyReleaseSceneQueryConsumeEffects(SceneQueryHit hitInfo)
        {
            if (hitInfo.ZeroAttackerHpOnConsume && Health != null)
                Health.HP = 0;

            if (hitInfo.ReleaseHeavyHeldTargetOnConsume && hitInfo.Target != null)
                ApplyHeavyHeldTargetReleaseConsumeEffect(hitInfo.Target);
        }

        internal void ApplyReleaseSceneQueryConsumeEffectsForCharacterDatInteraction(SceneQueryHit hitInfo)
            => ApplyReleaseSceneQueryConsumeEffects(hitInfo);

        /// <summary>
        /// C++ release `HitResolve.PreprocessCandidate` 中，重武器附着目标在特定 kind=0 命中前会先断开 2/-2 双向附着，
        /// 并把附着子物体切到随机落地帧、写入一个轻微下落速度。
        /// 这里补的是那条“命中前消费语义”，不是普通 held release。
        /// </summary>
        private void ApplyHeavyHeldTargetReleaseConsumeEffect(LF2Entity holderTarget)
        {
            if (holderTarget?.Runtime == null)
                return;

            int holderSlot = holderTarget.Runtime.SlotIndex;
            int heldTargetSlot = holderTarget.Runtime.ResolveActiveHeldSlotIndex();
            if (heldTargetSlot < 0)
            {
                holderTarget.Runtime.LinkState = 0;
                return;
            }

            LF2Entity heldTarget = holderTarget.Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot);
            if (heldTarget?.Runtime == null ||
                !heldTarget.Runtime.IsActivelyHeldBySlot(holderSlot) ||
                heldTarget.Runtime.LinkState != -2)
            {
                holderTarget.Runtime.LinkState = 0;
                return;
            }

            int attackerSlot = Runtime?.SlotIndex ?? -1;
            if (attackerSlot >= 0)
                holderTarget.ItrRest?.SetVrest(attackerSlot, 45);

            holderTarget.ItrRest?.SetVrest(heldTargetSlot, 30);
            holderTarget.Runtime.LinkState = 0;
            heldTarget.Runtime.LinkState = 0;
            heldTarget.ImmediateFrame(heldTarget.BattleRandInt(0, 6));
            heldTarget.Runtime.Vy = -1f;
            heldTarget.RefreshRuntimeSnapshot();
            holderTarget.RefreshRuntimeSnapshot();
        }

        public virtual void ApplySignedCpointFrame(int frameId)
        {
            if (frameId == 0)
                return;

            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            SetFrameTickDirect(frameId);
        }

        public virtual void ApplySignedImmediateFrameWaitReset(int frameId)
        {
            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            DirectWriteFrameImmediateWaitReset(frameId);
        }

        public virtual void ResetPooledEntityState()
        {
            _hasForcedRuntimeIntPosition = false;
            requiredRuntimeSlot = -1;
            Runtime.PendingFlushDestroy = false;
            Runtime.TransformOriginalObjectId = -1;
            Runtime.TransformTargetObjectId = -1;
            Runtime.RenderOffsetX = 0f;
        }

        public virtual void ApplyForcedRuntimeIntPosition(int x, int y, int z)
        {
            Runtime.XInt = x;
            Runtime.YInt = y;
            Runtime.ZInt = z;
            _hasForcedRuntimeIntPosition = true;
        }

        public virtual void ClearForcedRuntimeIntPosition()
        {
            _hasForcedRuntimeIntPosition = false;
        }

        public virtual void ConsumeForcedRuntimeIntPosition()
        {
            _hasForcedRuntimeIntPosition = false;
            RefreshRuntimeIntPosition();
        }

        internal bool TrySyncStageBoundsRuntimeZOnly()
        {
            // The production character is the only normal consumer of the release
            // stage-Z clamp. Its PhysicsState and Health are bound directly to this
            // Runtime, and its RefreshRuntimeFromEntity override has no additional
            // side effects. Unknown subclasses may add such side effects, so they
            // deliberately fail closed to the complete virtual refresh path.
            if (GetType() != typeof(LF2Character) || Runtime == null || PS == null)
                return false;

            if (!_hasForcedRuntimeIntPosition)
                Runtime.ZInt = (int)Runtime.Z;

            return true;
        }

        public virtual void ReleaseForcedRuntimeIntPositionAfterFirstPresentation(int tickIndex)
        {
            if (tickIndex >= Runtime.FirstPresentationTick)
                ConsumeForcedRuntimeIntPosition();
        }

        public virtual void RunCpointCheckStep10()
        {
            // step10 cpoint 维护是 battle loop 的交互阶段逻辑。
            // 它读取的是 collision snapshot / runtime link / cpoint 数据，
            // 不属于角色本地 `DispatchCurrentStateEvent(...)` 的 state 事件。
            LF2FrameData catcherFrame = GetCollisionFrameData();
            CatchPoint cpoint = catcherFrame?.cpoint;
            if (cpoint == null || cpoint.kind != 1 || FrameDelay < 0)
                return;

            LF2Entity victim = Match?.FindEntityByRuntimeSlotForQuery(CaughtSlotIndex);
            if (victim == null || victim.Frame == null)
            {
                DirectWriteFrameImmediateWaitReset(0);
                return;
            }

            LF2FrameData victimFrame = victim.GetCollisionFrameData();
            if (victim.CatcherSlotIndex != (Runtime?.SlotIndex ?? -1) ||
                victimFrame?.cpoint == null ||
                victimFrame.cpoint.kind != 2)
            {
                DirectWriteFrameImmediateWaitReset(0);
                return;
            }

            if (catcherFrame.state == LF2States.Catching)
                SyncCaughtByCpointStep10(victim, catcherFrame, cpoint);

            if (cpoint.decrease > 0)
            {
                Runtime.CaughtDuration -= cpoint.decrease;
            }
            else if (cpoint.decrease < 0)
            {
                Runtime.CaughtDuration += cpoint.decrease;
                if (Runtime.CaughtDuration < 0)
                {
                    DirectWriteFrameImmediateWaitReset(0);
                    victim.DirectWriteFrameImmediateWaitReset(181);
                    HitCount = 1;
                    victim.HitCount = 1;
                    victim.KnockbackVx = GetReleaseXInt() > victim.GetReleaseXInt() ? -4f : 4f;
                    victim.KnockbackVy = -3f;
                    victim.Runtime.Vx = victim.KnockbackVx;
                    victim.Runtime.Vy = victim.KnockbackVy;
                    return;
                }
            }

            RunCpointActionSelectionStep10(cpoint, victim);

            if (cpoint.throwvx != 0)
                ApplyCpointThrowStep10(cpoint, victim, catcherFrame);

            ApplyCpointDirControlStep10(cpoint);
        }

        public virtual void RunCpointMismatchTailStep10()
        {
            // 这里是 step10 的 mismatch 收尾，
            // 仍然属于 pass 级交互维护，不是 frame/TU/state_entry 一类本地事件。
            CatchPoint cpoint = Frame?.D?.cpoint;
            if (cpoint == null || cpoint.kind != 2)
                return;

            bool valid = false;
            LF2Entity catcher = Match?.FindEntityByRuntimeSlotForQuery(CatcherSlotIndex);
            if (catcher != null && catcher.CaughtSlotIndex == (Runtime?.SlotIndex ?? -1))
            {
                CatchPoint catcherCpoint = catcher.Frame?.D?.cpoint;
                valid = catcherCpoint != null && catcherCpoint.kind == 1;
            }

            if (valid)
                return;

            SetCpointRawFramePreserveWait(212);
            Runtime.Vy = -3f;
            if (Runtime.Y > -2f)
                Runtime.Y = -2f;
            RefreshRuntimeSnapshot();
        }

        public virtual void RunWeaponSyncHeldStep10()
        {
            LF2FrameData currentFrame = Frame?.D;
            CatchPoint cpoint = currentFrame?.cpoint;
            if (currentFrame == null || cpoint == null || cpoint.kind != 1 || currentFrame.state != LF2States.Catching)
                return;

            LF2Entity victim = Match?.FindEntityByRuntimeSlotForQuery(CaughtSlotIndex);
            if (victim == null || victim.CatcherSlotIndex != (Runtime?.SlotIndex ?? -1))
                return;

            LF2FrameData victimFrame = victim.Frame?.D;
            if (victimFrame?.cpoint == null || victimFrame.cpoint.kind != 2)
                return;

            SyncCaughtByCpointStep10(victim, currentFrame, cpoint);
        }

        public virtual void ClearHitCandidateCarriers()
        {
            HitConfirm2 = 0;
        }

        protected virtual void RunCpointActionSelectionStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            if (Runtime == null || cpoint == null || victimEntity == null)
                return;

            bool attackReady = IsSharedCharacterDatAttackInputReadyInternal();
            bool jumpReady = IsSharedCharacterDatJumpInputReadyInternal();

            if (attackReady && cpoint.aaction != 0)
            {
                bool dirOk = (Runtime.KeyLeft == 0 && Runtime.KeyRight == 0) || cpoint.taction == 0;
                if (dirOk)
                    ApplySharedCpointActionStep10(cpoint.aaction, victimEntity);
            }

            if (attackReady && cpoint.taction != 0)
            {
                bool anyDir = Runtime.KeyLeft != 0 || Runtime.KeyRight != 0 || Runtime.KeyUp != 0 || Runtime.KeyDown != 0;
                if (anyDir)
                    ApplySharedCpointActionStep10(cpoint.taction, victimEntity);
            }

            if (jumpReady && cpoint.jaction != 0)
                ApplySharedCpointActionStep10(cpoint.jaction, victimEntity);
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            ApplyCpointThrowStep10(cpoint, victimEntity, null);
        }

        protected virtual void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity, LF2FrameData throwFrameSnapshot)
        {
            if (cpoint == null || victimEntity == null)
                return;

            LF2FrameData sourceThrowFrame = throwFrameSnapshot ?? Frame?.D;
            int sourceNextFrameId = sourceThrowFrame?.next ?? 0;
            LF2FrameData sourceNextFrame = FrameCache?.HasFrame(sourceNextFrameId) == true
                ? FrameCache.GetFrameDataById(sourceNextFrameId)
                : null;

            if (cpoint.throwinjury == -1 && HasStep10ThrowTransformVictimData(victimEntity))
            {
                ApplyCpointThrowTransformToSelfAndOwnedObjects(victimEntity);
            }

            if (cpoint.throwinjury > 0)
                victimEntity.WeaponCount = cpoint.throwinjury;

            LF2FrameData throwFrame = throwFrameSnapshot ??
                FrameCache?.GetFrameDataById(Frame?.N ?? 0) ??
                Frame?.D;

            int centerX = throwFrame?.centerx ?? 0;
            int centerY = throwFrame?.centery ?? 0;
            int y = GetReleaseYInt() - centerY + cpoint.y;
            int x = Runtime.Dir == "right"
                ? GetReleaseXInt() - centerX + cpoint.x
                : centerX - cpoint.x + GetReleaseXInt();

            victimEntity.Runtime.X = x;
            victimEntity.Runtime.Y = y;

            int nextFrame = throwFrame?.next ?? 0;
            SetCpointRawFramePreserveWait(nextFrame, sourceNextFrame);
            SetCpointRawPrevFrame2(nextFrame, sourceNextFrame);
            AttackingCounter = 0;

            victimEntity.Runtime.Vx = Runtime.Dir == "right" ? cpoint.throwvx : -cpoint.throwvx;
            victimEntity.Runtime.Vy = cpoint.throwvy;
            SetVictimThrowVzStep10(cpoint, victimEntity);

            victimEntity.SetCpointRawFramePreserveWait(cpoint.vaction);
            victimEntity.SetCpointRawPrevFrame2(cpoint.vaction);
        }

        protected void ApplyCpointThrowTransformToSelfAndOwnedObjects(LF2Entity victimEntity)
        {
            if (victimEntity == null)
                return;

            LF2CharacterDataWrapper victimConfig = ResolveRuntimeCharacterConfig(victimEntity.ObjectId);
            if (victimConfig == null)
                return;

            TransformOriginalObjectId = ObjectId;
            TransformTargetObjectId = victimEntity.ObjectId;
            FrameCache.Load(victimConfig);
            ObjectId = victimEntity.ObjectId;
            WeaponCount = victimConfig.characterData?.weapon_hp ?? 0;
            SetCpointRawFramePreserveWait(0);
            Frame.PN = Frame.N;
            EnsureSharedCharacterDatControllerForSimulation();
            PropagateCpointThrowTransformToOwnedObjects(victimConfig, victimEntity.ObjectId);
        }

        protected virtual void SetVictimThrowVzStep10(CatchPoint cpoint, LF2Entity victim)
        {
            if (cpoint == null || victim == null)
                return;

            victim.Runtime.Vz = 0f;
            if (Runtime.KeyUp != 0 && Runtime.KeyDown == 0)
                victim.Runtime.Vz = -cpoint.throwvz;
            else if (Runtime.KeyUp == 0 && Runtime.KeyDown != 0)
                victim.Runtime.Vz = cpoint.throwvz;
        }

        protected virtual void ApplyCpointDirControlStep10(CatchPoint cpoint)
        {
            if (Runtime == null || cpoint == null || AttackingCounter != 2)
                return;

            if (cpoint.dircontrol == 1)
            {
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("right");
                else if (Runtime.KeyRight == 0 && Runtime.KeyLeft != 0)
                    SwitchDir("left");
            }
            else if (cpoint.dircontrol == -1)
            {
                if (Runtime.KeyRight != 0 && Runtime.KeyLeft == 0)
                    SwitchDir("left");
                else if (Runtime.KeyRight == 0 && Runtime.KeyLeft != 0)
                    SwitchDir("right");
            }
        }

        protected virtual void ApplyCpointHeldInjuryStep10(LF2Entity victimEntity, int injury)
        {
            if (victimEntity == null || victimEntity.Health == null)
                return;

            if (injury > 0)
            {
                int actualInjury = injury;
                if (victimEntity.FallDamageDiv > 0)
                    actualInjury = injury * 100 / victimEntity.FallDamageDiv;

                if (victimEntity.Health.HP > 0 &&
                    actualInjury >= victimEntity.Health.HP &&
                    victimEntity.KillCount == -1)
                {
                    LF2Entity holder = Match?.FindEntityByRuntimeSlotForQuery(HolderCopySlot);
                    if (holder != null)
                        holder.KillStat++;

                }

                victimEntity.Health.HP -= actualInjury;
                victimEntity.Health.HPBound -= actualInjury / 3;
                victimEntity.ComboCountVic += actualInjury;
                AttackingCounter = 1;
                FrameDelay = 2;
                victimEntity.FrameDelay = -3;
                LF2Entity comboHolder = Match?.FindEntityByRuntimeSlotForQuery(HolderCopySlot);
                if (comboHolder != null)
                    comboHolder.ComboCountAtk += actualInjury;

                return;
            }

            victimEntity.Health.HP += injury;
            victimEntity.Health.HPBound += injury / 3;
            AttackingCounter = 1;
        }

        internal bool HasStep10ThrowTransformVictimData(LF2Entity victimEntity)
        {
            return victimEntity?.FrameCache?.Wrapper?.characterData != null;
        }

        /// <summary>
        /// shared character-DAT 的攻击输入入口。
        /// 这里使用的是参考 C# 当前已落地的交叉 cooldown 语义：
        /// `KeyJump + CdAttack` 才表示这一拍要走 attack 输入分支。
        /// 把读取位置收束到单点，是为了后续如果还要细调输入链，
        /// 只需要改这一层，不必回头散改 step10 / shared character-DAT 调用点。
        /// </summary>
        protected virtual bool IsSharedCharacterDatAttackInputReadyInternal()
        {
            return Runtime.KeyJump != 0 && Runtime.CdAttack > 0;
        }

        /// <summary>
        /// shared character-DAT 的跳跃输入入口。
        /// 对齐参考 C# 的交叉 cooldown 语义：
        /// `KeyDefend + CdJump` 表示 jump 输入分支。
        /// </summary>
        protected virtual bool IsSharedCharacterDatJumpInputReadyInternal()
        {
            return Runtime.KeyDefend != 0 && Runtime.CdJump > 0;
        }

        /// <summary>
        /// shared character-DAT 的防御输入入口。
        /// 对齐参考 C# 的交叉 cooldown 语义：
        /// `KeyAttack + CdDefend` 表示 defend 输入分支。
        /// </summary>
        protected virtual bool IsSharedCharacterDatDefendInputReadyInternal(bool requireDefendLockOpen = false)
        {
            if (Runtime.KeyAttack == 0 || Runtime.CdDefend <= 0)
                return false;

            return !requireDefendLockOpen || Runtime.CdDefendLock <= 0;
        }

        private void ApplySharedCpointActionStep10(int actionFrame, LF2Entity victim)
        {
            if (victim == null)
                return;

            ApplySignedImmediateFrameWaitReset(actionFrame);
            int victimAction = Frame?.D?.cpoint?.vaction ?? 0;
            victim.DirectWriteFrameImmediateWaitReset(victimAction);
            victim.AttackingCounter = 0;
            AttackingCounter = 0;
        }

        internal void ApplySignedCpointActionFramePreserveWait(int frameId)
        {
            if (frameId < 0)
            {
                SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                frameId = -frameId;
            }

            SetCpointRawFramePreserveWait(frameId);
        }

        private void PropagateCpointThrowTransformToOwnedObjects(LF2CharacterDataWrapper wrapper, int targetObjectId)
        {
            var objects = new List<LF2Entity>();
            Match?.GetAllEntities(objects);
            int selfSlotIndex = Runtime?.SlotIndex ?? -1;
            if (selfSlotIndex < 0)
                return;

            for (int i = 0; i < objects.Count; i++)
            {
                LF2Entity entity = objects[i];
                if (entity == null || entity == this)
                    continue;
                if (!(Match?.IsActiveForCurrentPassInternal(entity) ?? false))
                    continue;
                if (entity.KillCount != selfSlotIndex)
                    continue;

                entity.FrameCache.Load(wrapper);
                entity.ObjectId = targetObjectId;
                entity.WeaponCount = wrapper.characterData?.weapon_hp ?? 0;
                entity.EnsureSharedCharacterDatControllerForSimulation();

                if (!entity.ReloadCurrentFrameDataFromWrapper())
                    entity.RefreshRuntimeSnapshot();
            }
        }

        protected virtual void SyncCpointHeldPositionStep10(LF2Entity victimEntity, LF2FrameData catcherFrame, CatchPoint catcherCpoint)
        {
            if (victimEntity == null || catcherFrame == null || catcherCpoint == null)
                return;

            int catcherX = GetReleaseXInt();
            int catcherY = GetReleaseYInt();
            int catcherZ = GetReleaseZInt();
            int dx = Runtime.Dir == "right"
                ? catcherX - catcherFrame.centerx + catcherCpoint.x
                : catcherFrame.centerx - catcherCpoint.x + catcherX;
            int dy = catcherY - catcherFrame.centery + catcherCpoint.y;

            LF2FrameData victimCurrentFrame = victimEntity.Frame?.D;
            int victimCpointX = victimCurrentFrame?.cpoint?.x ?? 0;
            int victimCpointY = victimCurrentFrame?.cpoint?.y ?? 0;
            int victimCenterX = victimCurrentFrame?.centerx ?? 0;
            int victimCenterY = victimCurrentFrame?.centery ?? 0;

            victimEntity.Runtime.X = victimEntity.Runtime.Dir == "right"
                ? victimCenterX - victimCpointX + dx
                : victimCpointX - victimCenterX + dx;
            victimEntity.Runtime.Y = victimCenterY - victimCpointY + dy;
            victimEntity.Runtime.Z = catcherZ;

            int coverDiv = catcherCpoint.cover / 10;
            int coverRem = catcherCpoint.cover % 10;
            if (coverRem != 0)
            {
                victimEntity.Runtime.Z += 1f;
                victimEntity.Runtime.Y -= 1f;
            }
            else
            {
                victimEntity.Runtime.Z -= 1f;
                victimEntity.Runtime.Y += 1f;
            }

            if (coverDiv == 1)
                victimEntity.SwitchDir(Runtime.Dir);
            else if (coverDiv == 2)
                victimEntity.SwitchDir(Runtime.Dir == "right" ? "left" : "right");

            victimEntity.RefreshRuntimeSnapshot();
        }

        private void SyncCaughtByCpointStep10(LF2Entity victim, LF2FrameData catcherFrame, CatchPoint cpoint)
        {
            if (victim == null || cpoint == null)
                return;

            if ((cpoint.hurtable == 0 || (victim.FrameDelay == 0 && cpoint.hurtable == 1)) &&
                cpoint.vaction != 0)
            {
                victim.DirectWriteFrameImmediateWaitReset(cpoint.vaction);
            }

            if (victim.Frame?.N < 0)
            {
                victim.SwitchDir(victim.Runtime.Dir == "left" ? "right" : "left");
                victim.SetCpointRawFramePreserveWait(-victim.Frame.N);
            }

            int injury = cpoint.injury;
            if (injury != 0 && AttackingCounter == 0)
                ApplyCpointHeldInjuryStep10(victim, injury);

            SyncCpointHeldPositionStep10(victim, catcherFrame, cpoint);
        }

        internal void SetCpointRawFramePreserveWait(int frameId)
            => SetCpointRawFramePreserveWait(frameId, null);

        internal void SetCpointRawFramePreserveWait(int frameId, LF2FrameData sourceFrame)
        {
            if (Frame == null || FrameCache == null)
                return;
            bool sourceFrameMatches = sourceFrame != null && sourceFrame.frameId == frameId;
            if (frameId >= 0 && !FrameCache.HasFrame(frameId) && !sourceFrameMatches)
                return;

            LF2FrameData targetFrame = sourceFrameMatches
                ? sourceFrame
                : FrameCache.GetFrameDataById(frameId);
            Frame.N = frameId;
            Frame.D = targetFrame;
            if (targetFrame != null)
                Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, Trans?.WaitCounter ?? 0);
            RefreshRuntimeSnapshot();
        }

        internal void SetCpointRawPrevFrame2(int frameId)
            => SetCpointRawPrevFrame2(frameId, null);

        internal void SetCpointRawPrevFrame2(int frameId, LF2FrameData sourceFrame)
        {
            if (Frame == null)
                return;

            Frame.Prev2 = frameId;
            Frame.Prev2D = sourceFrame != null && sourceFrame.frameId == frameId
                ? sourceFrame
                : FrameCache?.GetFrameDataById(frameId);
            Runtime.PrevFrame2 = frameId;
        }

        private int GetReleaseXInt()
        {
            return Runtime.XInt;
        }

        private int GetReleaseYInt()
        {
            return Runtime.YInt;
        }

        private int GetReleaseZInt()
        {
            return Runtime.ZInt;
        }

        // 当 FrameTransistor 发现“当前 frame 已经不是 waitCounter 记录的那一帧”时，会先通知这里。
        public virtual void OnFrameTickFrameChangedFromWaitCounter()
        {
            int frameId = Frame?.N ?? -1;
            string soundId = Frame?.D?.sound;
            if (frameId < 0 || frameId >= LF2FrameCache.MaxFrameIdExclusive || string.IsNullOrWhiteSpace(soundId))
                return;

            Match?.QueueSound(soundId, Runtime.XInt);
        }

        // FrameTransistor 在真正比较 wait 之前，会先进这里。
        // 公共计数器衰减和某些早退条件，都在这一层统一处理。
        public virtual bool OnFrameTickBeforeWaitAdvance(int previousFrame)
        {
            if (Frame?.D == null)
                return false;

            RunReleaseFrameTickCounters();

            if (Frame.D.cpoint != null && Frame.D.cpoint.kind == 2)
                return false;

            return ApplyObjectSpecificFrameTickBeforeWaitAdvance();
        }

        // FrameTransistor 决定要换帧时，通过这个钩子把目标帧请求交给实体自身处理。
        public virtual void OnFrameTickTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            OnFrameTransit(targetFrameId, switchDirAfterTrans);
        }

        // 真正换帧成功后，才会走到这个后置钩子。
        public virtual void OnFrameTickAfterWaitAdvance(int previousFrame, bool allowJumpInit)
        {
            ApplyCommonCaughtExitHitStop(previousFrame);
            ApplyCommonFrameTickPpDisplayPostAdvance();
        }

        // next=999 的最终落点由实体自己决定，不同对象可以有不同语义。
        public virtual int ResolveFrameTickNext999Target(out bool allowJumpInit)
        {
            allowJumpInit = false;
            return 0;
        }

        protected virtual bool ApplyObjectSpecificFrameTickBeforeWaitAdvance() => true;

        /// <summary>
        /// C# 基准工程 FrameTick.Tick 的公共计数器衰减段。
        /// 该段位于 cpoint kind=2 早退之前，所有实体都要按同一顺序执行。
        /// </summary>
        private void RunReleaseFrameTickCounters()
        {
            // AttackExempt is now decremented in RunCommonFrameTick before LinkState guard (BMD-062)

            if (HitStun > 0)
                HitStun--;
            else if (HitStun < 0)
                HitStun++;

            if (FallCounter > 0)
                FallCounter--;

            if (HitStateCount > 0)
                HitStateCount--;

            if (HitConfirmCounter > 0)
                HitConfirmCounter--;
        }

        protected virtual void ApplyCommonCaughtExitHitStop(int previousFrameId)
        {
            LF2FrameData previousFrame = FrameCache?.GetFrameDataById(previousFrameId);
            if (previousFrame == null || previousFrame.state != LF2States.Lying)
                return;

            if ((Frame?.D?.state ?? 0) == LF2States.Frozen)
                return;

            if (RelationTeam == 5 || Unk344 != 0)
            {
                if ((Match?.Difficulty ?? 2) == 2)
                    return;

                int gameMode = Match?.BattleGameModeId ?? 0;
                bool oidSkip = (gameMode == 1 || gameMode == 4) &&
                               ObjectId / 5 == 3 &&
                               ObjectId != 38;
                if (oidSkip)
                    return;
            }

            HitStun = 15;
        }

        protected virtual bool IsFrameTickLeftPressed() => Runtime?.KeyLeft != 0;

        protected virtual bool IsFrameTickRightPressed() => Runtime?.KeyRight != 0;

        protected virtual bool IsFrameTickUpPressed() => Runtime?.KeyUp != 0;

        protected virtual bool IsFrameTickDownPressed() => Runtime?.KeyDown != 0;

        protected virtual int GetFrameTickCdUp() => Runtime?.CdUp ?? 0;

        protected virtual int GetFrameTickCdDown() => Runtime?.CdDown ?? 0;

        protected virtual void ApplyFrame212JumpInit()
        {
            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            if (characterData == null)
                return;

            Runtime.Vy = characterData.jump_height;
            if (IsFrameTickRightPressed() && !IsFrameTickLeftPressed())
                Runtime.Vx = characterData.jump_distance;
            else if (IsFrameTickLeftPressed() && !IsFrameTickRightPressed())
                Runtime.Vx = -characterData.jump_distance;

            if (IsFrameTickUpPressed() && !IsFrameTickDownPressed())
                Runtime.Vz = -characterData.jump_distancez;
            else if (IsFrameTickDownPressed() && !IsFrameTickUpPressed())
                Runtime.Vz = characterData.jump_distancez;
        }

        /// <summary>
        /// 对齐参考 `FrameTick` 的负 mp 帧推进后处理。
        /// 当前只收敛已确认的 PP 真值与 PpDisplay 累计面，不扩展到 HUD 刷新。
        /// </summary>
        protected void ApplyCommonFrameTickPpDisplayPostAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Health == null || !IsPpModeEnabled())
                return;
            if ((Frame?.N ?? -1) >= LF2FrameCache.MaxFrameIdExclusive)
                return;
            int mpDelta = frame.mp;
            if (mpDelta >= 0)
                return;

            if (Health.PP < mpDelta)
            {
                SetFrameTickImmediateRawDirect(frame.hit_d);
                frame = Frame?.D;
                if (frame == null)
                    return;
            }
            else
            {
                Health.PP += mpDelta;
                SpendPpDisplay(-mpDelta);
            }

            int turnNext = frame.hit_d;
            if (turnNext <= 0 || GetRuntimeYInt() != 0)
                return;

            bool left = Runtime?.KeyLeft != 0;
            bool right = Runtime?.KeyRight != 0;
            if (left && !right && Runtime?.Dir == "right")
                SetFrameTickImmediateRawDirect(turnNext);
            else if (right && !left && Runtime?.Dir == "left")
                SetFrameTickImmediateRawDirect(turnNext);
        }

        protected bool TryEnterReleaseFrameAdvanceAfterDelay()
        {
            if (ThrowFrameGuard >= 0 && ThrowFrameGuard == (Frame?.N ?? -1))
                return false;

            if (FrameDelay > 0)
            {
                FrameDelay--;
                return false;
            }

            if (FrameDelay < 0)
            {
                FrameDelay++;
                return false;
            }

            return true;
        }

        protected void RunSharedCharacterDatFrameAdvanceAsCharacter(int tickIndex, bool consumeForcedRuntimeIntPosition = true)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return;

            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return;

            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return;

            float mass = NTSDGlobal.Default.Machanics.Mass;
            var mechanics = new CharacterMechanics();
            var context = new CharacterMechanicsContext(
                Runtime,
                Frame?.D,
                GetSpriteWidthPxForCollision(),
                mass,
                NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                point =>
                {
                    SimulationWorld world = Match;
                    return world == null || world.IsGroundPointWalkable(point);
                });

            MechanicsStepResult stepResult = mechanics.Step(context);
            if (ShouldResolveCharacterLanding(stepResult))
                ApplySharedCharacterDatLandingIfNeeded(stepResult.verticalVelocityBeforeLanding);

            Runtime.SyncIntegerPosition();
            PromoteSharedCharacterDatState12AirborneFrameIfNeeded(tickIndex);
            PromoteSharedCharacterDatBurningAirborneFrame205IfNeeded();
            ResetWeaponCountOutsideState12FrameAdvanceTail();

            if (consumeForcedRuntimeIntPosition)
                ConsumeForcedRuntimeIntPosition();
        }

        protected bool ShouldResolveCharacterLanding(MechanicsStepResult stepResult)
        {
            return stepResult.landed;
        }

        protected bool RunSharedNonCharacterDatFrameAdvance(bool consumeForcedRuntimeIntPosition = true)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return false;
            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return false;
            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return false;

            ApplyNonCharacterFrameVelocityForFrameAdvance();

            int dataType = GetCurrentDataObjectTypeForSimulation();
            LF2FrameData frame = Frame?.D;
            if (Runtime == null || frame == null)
                return false;

            if (dataType == (int)LF2ObjectType.ThrowWeapon || ObjectId == 120)
                Runtime.X += Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;
            if (ObjectId == 101)
                Runtime.X -= Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;

            if (dataType == (int)LF2ObjectType.SpecialAttack && frame.hit_j > 0)
            {
                double visualZ = frame.hit_j - 50;
                Runtime.Z += visualZ;
                Runtime.Type3VisualZOffset += visualZ;
            }

            if ((dataType == (int)LF2ObjectType.ThrowWeapon || dataType == (int)LF2ObjectType.Drink) &&
                frame.state == 1000 &&
                System.Math.Abs(Runtime.Vx) > 9.0)
            {
                SetFrameTickDirect(40);
                frame = Frame?.D ?? frame;
            }

            double gravity = ResolveCurrentDatWeaponGravity(dataType, frame.state);
            bool landed = CharacterMechanics.WeaponDynamics(Runtime, gravity, out double landingVy);
            ApplyCurrentDatNonCharacterLanding(dataType, frame, landingVy, landed);
            ResetWeaponCountOutsideState12FrameAdvanceTail();

            Runtime.SyncIntegerPosition();
            if (consumeForcedRuntimeIntPosition)
                ConsumeForcedRuntimeIntPosition();
            RefreshRuntimeSnapshot();
            return true;
        }

        protected bool ApplyCurrentDatNonCharacterLanding(
            int dataType,
            LF2FrameData landingFrame,
            double landingVy,
            bool crossedGround)
        {
            if (Runtime == null || landingFrame == null)
                return false;

            LF2CharacterData characterData = FrameCache?.Wrapper?.characterData;
            int dropHurt = characterData?.weapon_drop_hurt ?? 0;
            string dropSound = characterData?.weapon_drop_sound;
            int state = landingFrame.state;

            if (dataType == (int)LF2ObjectType.LightWeapon)
            {
                if (!crossedGround || landingVy <= 0.0001)
                    return true;

                Runtime.WeaponFlightCounter -= dropHurt;
                Runtime.Y = 0.0;
                if (landingVy <= 9.9)
                {
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(state == LF2States.WeaponThrowing ? 70 : 60);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }
                else if (state == LF2States.WeaponThrowing)
                {
                    Runtime.Vy = -8.0;
                    SetFrameTickRawDirect(7);
                    SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                    Runtime.Vx *= 0.5;
                    QueueBattleSound(dropSound);
                }
                else
                {
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(60);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }

                return true;
            }

            if (dataType == (int)LF2ObjectType.HeavyWeapon)
            {
                if (!crossedGround)
                    return true;

                Runtime.WeaponFlightCounter -= 1;
                Runtime.Y = 0.0;
                if (landingVy > 9.0)
                {
                    QueueBattleSound(dropSound);
                    Runtime.Vy = -5.0;
                    SwitchDir(Runtime.Dir == "left" ? "right" : "left");
                    Runtime.Vx *= 0.5;
                }
                else
                {
                    Runtime.WeaponFlightCounter -= dropHurt;
                    if (Runtime.WeaponFlightCounter < 0)
                        Runtime.WeaponFlightCounter = 0;
                    Runtime.Vy = 0.0;
                    SetFrameTickRawDirect(20);
                    Runtime.Vx *= 0.5;
                    AttackingCounter = 0;
                }

                return true;
            }

            if (dataType == (int)LF2ObjectType.ThrowWeapon ||
                dataType == (int)LF2ObjectType.Drink)
            {
                if (!crossedGround || landingVy <= 0.0001)
                    return true;

                Runtime.WeaponFlightCounter -= dropHurt;
                if (dataType == (int)LF2ObjectType.Drink && Health != null && Health.HP <= 0)
                    Runtime.WeaponFlightCounter = -1;

                Runtime.Y = 0.0;
                bool highSpeed = landingVy > 8.5 || Runtime.Vx < -10.0 || Runtime.Vx > 10.0;
                bool bounceState = state == LF2States.WeaponThrowing || state == LF2States.WeaponInSky;
                if (highSpeed && bounceState)
                {
                    Runtime.Vy = landingVy * -0.7;
                    if (Runtime.Vy < -10.0)
                        Runtime.Vy = -10.0;
                    Runtime.Vx *= 0.7;
                    SetFrameTickRawDirect(0);
                    QueueBattleSound(dropSound);
                }
                else
                {
                    Runtime.Vy = 0.0;
                    Runtime.Vx *= 0.7;
                    SetFrameTickRawDirect(state == LF2States.WeaponThrowing ? 70 : 60);
                    AttackingCounter = 0;
                }

                return true;
            }

            if (ObjectId == 999 && crossedGround)
            {
                Runtime.Y = 0.0;
                Runtime.Vy = 0.0;
                Runtime.Vx = 0.0;
                SetFrameTickRawDirect(101);
                AttackingCounter = 0;
                return true;
            }

            return false;
        }

        private double ResolveCurrentDatWeaponGravity(int dataType, int state)
        {
            if (dataType == (int)LF2ObjectType.SpecialAttack)
                return 0.0;
            if (dataType == (int)LF2ObjectType.Drink)
                return NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
            if (dataType == (int)LF2ObjectType.ThrowWeapon)
                return 0.85;
            if (state != LF2States.WeaponThrowing)
                return NTSDGlobal.Gameplay.WeaponGravityDefault;

            switch (ObjectId)
            {
                case 124:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub7C;
                case 120:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub78;
                case 101:
                    return NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
                default:
                    return NTSDGlobal.Gameplay.WeaponGravityDefault1002;
            }
        }

        private void ApplySharedCharacterDatLandingIfNeeded(double landedVy) // P0-f-2b B2-1: float→double
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return;

            if (frame.state == LF2States.Falling || frame.state == LF2States.Burning)
            {
                QueueBattleSound("SFX_006");
                ApplySharedCharacterDatLandingWeaponCountDamage();

                if (landedVy <= 11.0 &&
                    Runtime.Vx <= 9.0 &&
                    Runtime.Vx >= -9.0 &&
                    frame.state != LF2States.Burning)
                {
                    Runtime.Y = 0.0;
                    Runtime.Vy = 0.0;
                    Runtime.Vx *= 0.3333333333333333;
                    AttackingCounter = 0;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack
                        ? LF2StandardFrames.LyingBack
                        : LF2StandardFrames.Lying);
                }
                else
                {
                    Runtime.Y = 0.0;
                    Runtime.Vy = -3.5;
                    if (Runtime.Vx > 7.0)
                        Runtime.Vx = 7.0;
                    if (Runtime.Vx < -7.0)
                        Runtime.Vx = -7.0;
                    ImmediateFrame(Frame.N >= LF2StandardFrames.FallingBack && frame.state != LF2States.Burning
                        ? LF2StandardFrames.FallingBack5
                        : LF2StandardFrames.FallingFront5);
                }

                return;
            }

            if (frame.state == LF2States.Frozen && landedVy > 0.0001)
            {
                Runtime.Y = 0.0;

                if (landedVy <= 17.0 && Runtime.Vx <= 9.0 && Runtime.Vx >= -9.0)
                {
                    Runtime.Vx *= 0.3333333333333333;
                    Runtime.Vy = 0.0;
                    return;
                }

                int injury = FallDamageDiv == 0 ? 10 : 1000 / FallDamageDiv;
                if (Health != null)
                    Health.HP -= injury;

                Runtime.Vy = -3.5;
                if (Runtime.Vx > 7.0)
                    Runtime.Vx = 7.0;
                if (Runtime.Vx < -7.0)
                    Runtime.Vx = -7.0;
                ImmediateFrame(LF2StandardFrames.FallingFront5);
                return;
            }

            Runtime.Y = 0.0;
            Runtime.Vy = 0.0;
            Runtime.Vx *= 0.3333333333333333;
            AttackingCounter = 0;

            int landingFrame;
            if (frame.state == LF2States.CustomSkill1)
                landingFrame = 94;
            else if (Frame.N == LF2StandardFrames.JumpingAir || frame.state == LF2States.Rowing)
                landingFrame = LF2StandardFrames.Crouch;
            else
                landingFrame = LF2StandardFrames.Crouch2;

            ImmediateFrame(landingFrame);
        }

        private void ApplySharedCharacterDatLandingWeaponCountDamage()
        {
            if (WeaponCount == 0 || Health == null)
                return;

            int damage = WeaponCount < 0 ? -WeaponCount : WeaponCount;
            if (FallDamageDiv > 0)
                damage = damage * 100 / FallDamageDiv;

            Health.HP -= damage;
            Health.HPBound -= damage;
            WeaponCount = 0;
        }

        private void PromoteSharedCharacterDatState12AirborneFrameIfNeeded(int tickIndex)
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                return;

            if (Runtime == null || Runtime.Y >= 0f)
                return;

            int frameId = Frame.N;
            double vy = Runtime.Vy;

            if (frameId < LF2StandardFrames.FallingFront5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingFront3);

                PromoteSharedCharacterDatState12NegativeWeaponCountCadenceOverride(tickIndex);
            }
            else if (frameId > LF2StandardFrames.FallingFront5 && frameId < LF2StandardFrames.FallingBack5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingBack3);
            }
        }

        private void PromoteSharedCharacterDatState12NegativeWeaponCountCadenceOverride(int tickIndex)
        {
            if (WeaponCount >= 0)
                return;

            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                return;

            if (Runtime == null || Runtime.Y >= 0f || Runtime.Vy >= 12f)
                return;

            int cadencePhase = (tickIndex - 1) % 12;
            if (cadencePhase < 0)
                cadencePhase += 12;

            SetFrameTickDirect(cadencePhase >= 6
                ? LF2StandardFrames.FallingFront2
                : LF2StandardFrames.FallingFront1);
        }

        private void PromoteSharedCharacterDatBurningAirborneFrame205IfNeeded()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Burning)
                return;

            if (Frame.N >= LF2StandardFrames.Fire2)
                return;

            if (Runtime == null || Runtime.Y >= 0f || Runtime.Vy <= 1.0f)
                return;

            SetFrameTickDirect(LF2StandardFrames.Fire2);
        }

        protected void ResetWeaponCountOutsideState12FrameAdvanceTail()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != LF2States.Falling)
                WeaponCount = 0;
        }

        protected void SetFrameTickDirect(int frameId)
        {
            SetFrameTickDirect(frameId, Trans?.WaitCounter ?? 0);
        }

        protected void SetFrameTickDirect(int frameId, int waitCounter)
        {
            if (Frame == null || FrameCache == null)
                return;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            Frame.N = frameId;
            Frame.D = targetFrame;
            if (Runtime != null)
                Runtime.Frame = frameId;
            if (targetFrame != null)
                Trans?.SyncDirectFrameData(targetFrame.wait, targetFrame.next, waitCounter);
        }

        /// <summary>
        /// 处理参考命中特效里的编码 effect 段。
        /// 5000..5999 表示直接扣 PP，6000..6999 表示直接写目标帧。
        /// 这两段只改逻辑真值，不属于 PpDisplay 的输入/表现累计来源。
        /// </summary>
        internal bool ApplyCommonEncodedHitEffectRange(int effectNum)
        {
            if (effectNum >= 5000 && effectNum < 6000)
            {
                if (Health != null)
                {
                    int nextPp = Health.PP - (effectNum - 5000);
                    Health.PP = nextPp < 0 ? 0 : nextPp;
                }

                return true;
            }

            if (effectNum >= 6000 && effectNum < 7000)
            {
                DirectWriteFramePreserveWaitCounter(effectNum - 6000);
                return true;
            }

            return false;
        }

        protected virtual bool RunCommonFrameTick()
        {
            if (ThrowFrameGuard >= 0 && ThrowFrameGuard == (Frame?.N ?? -1))
                return false;

            int dataType = GetCurrentDataObjectTypeForSimulation();
            if (FrameDelay != 0 && dataType != (int)LF2ObjectType.SpecialAttack)
                return false;

            if (AttackExempt > 0)
                AttackExempt--;

            if ((Runtime?.LinkState ?? 0) < 0)
                return false;

            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return false;
            if (frame.cpoint != null && frame.cpoint.kind == 2)
                return false;

            if (dataType == (int)LF2ObjectType.SpecialAttack && frame.hit_a > 0 && Health != null)
            {
                Health.HP -= frame.hit_a;
                if (Health.HP <= 0)
                {
                    Health.HP = 0;
                    SetFrameTickImmediateRawDirect(frame.hit_d);
                    frame = Frame?.D;
                    if (frame == null)
                        return false;
                }
            }

            RunReleaseFrameTickCounters();

            int waitCounter = Trans?.WaitCounter ?? 0;
            if ((Frame?.N ?? 0) != waitCounter)
            {
                OnFrameTickFrameChangedFromWaitCounter();
                AttackingCounter = 0;
            }

            AttackingCounter++;

            int state = frame.state;
            bool suppressJumpInit = false;
            if (state == 0 && GetRuntimeYInt() < 0)
            {
                SetFrameTickImmediateRawDirect(212);
                suppressJumpInit = true;
                frame = Frame?.D;
                if (frame == null)
                    return false;
                state = frame.state;
            }

            if (dataType == (int)LF2ObjectType.HeavyWeapon &&
                state == LF2States.HeavyWeaponInSky &&
                GetRuntimeYInt() == 0 &&
                System.Math.Abs(Runtime.Vx) < 0.1)
            {
                return false;
            }

            if (state == LF2States.Lying && Health != null && Health.HP <= 0)
            {
                if ((KillCount >= 0 || RelationTeam == 5 || (Runtime?.SlotIndex ?? -1) >= 20) && HitStun <= 0)
                    HitStun = 30;
                AttackingCounter = 0;
            }

            if (state == LF2States.HeavyWeaponInSky)
                SwitchDir(Runtime.Vx > 0f ? "right" : "left");

            int wait = Trans?.Wait ?? frame.wait;
            if (AttackingCounter > wait)
            {
                int next = Trans?.Next ?? frame.next;
                AttackingCounter = 0;
                if (next != 0)
                {
                    bool allowJumpInit = true;
                    int targetFrame = next;
                    if (targetFrame == 999)
                    {
                        bool to212 = GetRuntimeYInt() != 0 && dataType == (int)LF2ObjectType.Character;
                        targetFrame = to212 ? 212 : 0;
                        suppressJumpInit = to212;
                        allowJumpInit = false;
                    }
                    else if (targetFrame < 0)
                    {
                        targetFrame = -targetFrame;
                        SwitchDir(Runtime?.Dir == "left" ? "right" : "left");
                    }

                    int previousFrame = waitCounter;
                    SetFrameTickImmediateRawDirect(targetFrame);
                    int frameAfterTransit = Frame?.N ?? targetFrame;
                    if (frameAfterTransit < 0 || frameAfterTransit >= LF2FrameCache.MaxFrameIdExclusive || Frame?.D == null)
                        return false;

                    ApplyCommonCaughtExitHitStop(previousFrame);
                    if (frameAfterTransit == 212 && allowJumpInit && !suppressJumpInit)
                        ApplyFrame212JumpInit();
                    ApplyCommonFrameTickPpDisplayPostAdvance();
                }
            }

            int currentFrame = Frame?.N ?? -1;
            if (currentFrame == 110 || currentFrame == 114)
                Runtime.CdDefendLock = 3;
            if (currentFrame == 202)
                HitStun = 20;

            LF2FrameData currentData = Frame?.D;
            if (currentData != null)
                Trans?.SyncWaitCounterFrame(currentFrame);

            return true;
        }

        internal bool RunCommonFrameTickFromTransistor()
        {
            return RunCommonFrameTick();
        }

        private void SetFrameTickRawDirect(int frameId)
        {
            if (Frame == null)
                return;

            Frame.N = frameId;
            Frame.D = FrameCache?.GetFrameDataById(frameId);
            if (Runtime != null)
                Runtime.Frame = frameId;
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, Trans?.WaitCounter ?? 0);
        }

        private void SetFrameTickImmediateRawDirect(int frameId)
        {
            SetFrameTickRawDirect(frameId);
            if (Runtime != null)
                Runtime.FrameWaitCounter = 0;
        }

        protected void SpendPpDisplay(int ppCost)
        {
            if (ppCost > 0 && Runtime != null)
                Runtime.PpDisplay += ppCost;
        }

        protected void RefundPpDisplay(int ppDelta)
        {
            if (ppDelta > 0 && Runtime != null)
                Runtime.PpDisplay -= ppDelta;
        }

        protected void ApplyNonCharacterFrameVelocityForFrameAdvance()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || Runtime == null)
                return;

            double vx = Runtime.Vx;
            ApplyFrameAxisVelocity(frame.dvx, ref vx, Dirh());
            Runtime.Vx = vx;

            if (frame.dvy > 500)
                Runtime.Vy = frame.dvy - 550;
            else if (frame.dvy != 0)
                Runtime.Vy += frame.dvy;

            if (frame.dvz > 500)
            {
                Runtime.Vz = frame.dvz - 550;
                return;
            }

            if (frame.dvz == 0)
                return;

            if (IsFrameTickUpPressed() && GetFrameTickCdUp() >= GetFrameTickCdDown())
                Runtime.Vz = -frame.dvz;
            if (IsFrameTickDownPressed() && GetFrameTickCdDown() >= GetFrameTickCdUp())
                Runtime.Vz = frame.dvz;
        }

        private static void ApplyFrameAxisVelocity(int value, ref double velocity, int direction) // P0-f: double sim velocity
        {
            if (value > 500)
            {
                velocity = value - 550;
                return;
            }

            if (value == 550)
            {
                velocity = 0f;
                return;
            }

            if (value > 0)
            {
                float target = value * direction;
                if (direction >= 0)
                {
                    if (velocity < target)
                        velocity = target;
                }
                else if (velocity > target)
                {
                    velocity = target;
                }

                return;
            }

            if (value >= 0)
                return;

            float negativeTarget = value * direction;
            if (direction >= 0)
            {
                if (velocity > negativeTarget)
                    velocity = negativeTarget;
            }
            else if (velocity < negativeTarget)
            {
                velocity = negativeTarget;
            }
        }



        /// <summary>分配稳定 ID。</summary>
        protected void AllocateStableId()
        {
            StableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;
            Runtime.StableId = StableId;
        }

        /// <summary>重置稳定 ID。</summary>
        protected void ResetStableId()
        {
            StableId = 0;
            Runtime.StableId = 0;
        }

        /// <summary>写入运行时槽位索引。</summary>
        public void SetRuntimeSlotIndex(int slotIndex)
        {
            Runtime.SlotIndex = slotIndex;
        }

        /// <summary>刷新 Runtime 中的派生字段和非位置状态。</summary>
        public void RefreshRuntimeSnapshot()
        {
            RefreshRuntimeFromEntity();
        }

        internal bool IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp()
        {
            if (Runtime == null)
                return false;

            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (Runtime.ObjType != ResolveReferenceRuntimeObjTypeFromDataType(currentDataType) ||
                Runtime.EntityType != currentDataType ||
                Runtime.Frame != (Frame?.N ?? 0) ||
                Runtime.WaitCounter != (Trans?.WaitCounter ?? 0) ||
                Runtime.NextFrame != (Trans?.Next ?? 0))
            {
                return false;
            }

            return _hasForcedRuntimeIntPosition ||
                   (Runtime.XInt == (int)Runtime.X &&
                    Runtime.YInt == (int)Runtime.Y &&
                    Runtime.ZInt == (int)Runtime.Z);
        }

        private bool IsCompleteRuntimeSnapshotCurrentForExactCharacter()
        {
            if (GetType() != typeof(LF2Character) ||
                Runtime == null ||
                Health == null)
            {
                return false;
            }

            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (Runtime.StableId != StableId ||
                Runtime.ObjectId != ObjectId ||
                Runtime.ObjType !=
                    ResolveReferenceRuntimeObjTypeFromDataType(
                        currentDataType) ||
                Runtime.EntityType != currentDataType ||
                Runtime.Team != Team ||
                Runtime.OwnerSlotIndex != OwnerEntityIndex ||
                Runtime.OwnerStableId != OwnerId ||
                Runtime.GrabbedBy != GrabbedBy ||
                Runtime.TrackerFlag != TrackerFlag ||
                Runtime.Frame != (Frame?.N ?? 0) ||
                Runtime.WaitCounter != (Trans?.WaitCounter ?? 0) ||
                Runtime.NextFrame != (Trans?.Next ?? 0) ||
                Runtime.AttackingCounter != AttackingCounter ||
                Runtime.FrameDelay != FrameDelay ||
                Runtime.HitStop != HitStun ||
                Runtime.AttackExempt != AttackExempt ||
                Runtime.HealTimer != HealTimer ||
                Runtime.KillCount != KillCount ||
                Runtime.ShotCount != ShotCount ||
                Runtime.HPOrig != HPOrig ||
                Runtime.HP2Orig != HP2Orig ||
                Runtime.RespawnCount != RespawnCount)
            {
                return false;
            }

            if (!_hasForcedRuntimeIntPosition &&
                (Runtime.XInt != (int)Runtime.X ||
                 Runtime.YInt != (int)Runtime.Y ||
                 Runtime.ZInt != (int)Runtime.Z))
            {
                return false;
            }

            return Runtime.HP == Health.HP &&
                   Runtime.MP == Health.MP &&
                   Runtime.PP == Health.PP &&
                   Runtime.PPMax == Health.MaxPP &&
                   Runtime.PPBound == Health.PPBound &&
                   Runtime.HPLost == Health.HPLost &&
                   Runtime.HPBound == Health.HPBound &&
                   Runtime.MPMax == Health.MaxMP;
        }

        protected virtual void RefreshRuntimeFromEntity()
        {
            int currentDataType = GetCurrentDataObjectTypeForSimulation();

            Runtime.StableId = StableId;
            Runtime.ObjectId = ObjectId;
            Runtime.ObjType = ResolveReferenceRuntimeObjTypeFromDataType(currentDataType);
            Runtime.EntityType = currentDataType;
            Runtime.Team = Team;
            Runtime.OwnerSlotIndex = OwnerEntityIndex;
            Runtime.OwnerStableId = OwnerId;
            Runtime.GrabbedBy = GrabbedBy;
            Runtime.TrackerFlag = TrackerFlag;
            Runtime.Frame = Frame?.N ?? 0;
            Runtime.WaitCounter = Trans?.WaitCounter ?? 0;
            Runtime.NextFrame = Trans?.Next ?? 0;
            Runtime.AttackingCounter = AttackingCounter;
            Runtime.FrameDelay = FrameDelay;
            Runtime.HitStop = HitStun;
            Runtime.AttackExempt = AttackExempt;
            Runtime.HealTimer = HealTimer;
            Runtime.KillCount = KillCount;
            Runtime.ShotCount = ShotCount;
            Runtime.HPOrig = HPOrig;
            Runtime.HP2Orig = HP2Orig;
            Runtime.RespawnCount = RespawnCount;

            if (!_hasForcedRuntimeIntPosition)
                RefreshRuntimeIntPosition();

            if (Health != null)
            {
                Runtime.HP = Health.HP;
                Runtime.MP = Health.MP;
                Runtime.PP = Health.PP;
                Runtime.PPMax = Health.MaxPP;
                Runtime.PPBound = Health.PPBound;
                Runtime.HPLost = Health.HPLost;
                Runtime.HPBound = Health.HPBound;
                Runtime.MPMax = Health.MaxMP;
            }
        }

        private void RefreshRuntimeIntPosition()
        {
            Runtime.SyncIntegerPosition();
        }

        /// <summary>
        /// C# 基准工程的 Physics.SyncIntegers 使用 (int) 强制转换。
        /// 这里必须截断而不是四舍五入，否则阴影、碰撞和 opoint 的整数坐标会持续偏移。
        /// </summary>
        private int ReleaseInt(double value) // P0-f: truncate double directly (baseline (int)X); float callers widen
        {
            return (int)value;
        }

    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressHarness.cs ---
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NTSD.Simulation.Spatial;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace NTSD.Animation.Rendering.Editor
{
    public enum ProductionEntityStressMode
    {
        Smoke50,
        Dispersed1000,
        Concentrated1000,
    }

    internal enum ProductionEntityStressInputMode
    {
        Ai,
        None,
    }

    [Serializable]
    public sealed class ProductionEntityStressRequest
    {
        public string action = "dispersed";
        public string inputMode = "ai";
        public int warmupTicks = 30;
        public int sampleTicks = 300;
        public int spawnBatchSize = 25;
        public int maxCatchUpTicksPerFrame = 4;
        public int maxBacklogTicks = 8;
        public bool enablePhaseTiming;
        public bool enablePresentationTiming;
        public bool enableDetailPhaseTiming;
        public string formalCollectorMode = "configured";
        public bool forceRoleAwareTreeForDiagnostics;
        public bool forceRoleAwareDirectForDiagnostics;
        public bool forceLegacyZeroAttackItrShortCircuitForDiagnostics;
        public bool forceLegacyPreInteractionForDiagnostics;
        public bool forceLegacyCharacterInputPassForDiagnostics;
        public bool forceLegacyAiSnapshotBuildForDiagnostics;
        public bool forceLegacyAiNearestFilterForDiagnostics;
        public bool enableAiNearestFlatShadowForDiagnostics;
        public bool forceAiNearestFlatQueryForDiagnostics;
        public bool forceLegacyLateRuntimeSnapshotsForDiagnostics;
        public string outputPath = "Temp/NTSD_ProductionEntityStress.dispersed.json";
    }

    internal readonly struct ProductionEntityStressConfig
    {
        internal ProductionEntityStressConfig(
            ProductionEntityStressMode mode,
            ProductionEntityStressInputMode inputMode,
            int warmupTicks,
            int sampleTicks,
            int spawnBatchSize,
            int maxCatchUpTicksPerFrame,
            int maxBacklogTicks,
            bool enablePhaseTiming,
            bool enablePresentationTiming,
            bool enableDetailPhaseTiming,
            CollisionFormalCollectorMode formalCollectorMode,
            bool forceRoleAwareTreeForDiagnostics,
            bool forceRoleAwareDirectForDiagnostics,
            bool forceLegacyZeroAttackItrShortCircuitForDiagnostics,
            bool forceLegacyPreInteractionForDiagnostics,
            bool forceLegacyCharacterInputPassForDiagnostics,
            bool forceLegacyAiSnapshotBuildForDiagnostics,
            bool forceLegacyAiNearestFilterForDiagnostics,
            bool enableAiNearestFlatShadowForDiagnostics,
            bool forceAiNearestFlatQueryForDiagnostics,
            bool forceLegacyLateRuntimeSnapshotsForDiagnostics,
            string outputPath)
        {
            Mode = mode;
            InputMode = inputMode;
            EntityCount = mode == ProductionEntityStressMode.Smoke50 ? 50 : 1000;
            WarmupTicks = Math.Max(0, warmupTicks);
            SampleTicks = Math.Max(1, sampleTicks);
            SpawnBatchSize = Math.Max(1, Math.Min(100, spawnBatchSize));
            MaxCatchUpTicksPerFrame = Math.Max(1, maxCatchUpTicksPerFrame);
            MaxBacklogTicks = Math.Max(MaxCatchUpTicksPerFrame, maxBacklogTicks);
            EnablePhaseTiming = enablePhaseTiming;
            EnablePresentationTiming = enablePresentationTiming;
            EnableDetailPhaseTiming = enableDetailPhaseTiming;
            FormalCollectorMode = formalCollectorMode;
            ForceRoleAwareTreeForDiagnostics = forceRoleAwareTreeForDiagnostics;
            ForceRoleAwareDirectForDiagnostics = forceRoleAwareDirectForDiagnostics;
            ForceLegacyZeroAttackItrShortCircuitForDiagnostics =
                forceLegacyZeroAttackItrShortCircuitForDiagnostics;
            ForceLegacyPreInteractionForDiagnostics =
                forceLegacyPreInteractionForDiagnostics;
            ForceLegacyCharacterInputPassForDiagnostics =
                forceLegacyCharacterInputPassForDiagnostics;
            ForceLegacyAiSnapshotBuildForDiagnostics =
                forceLegacyAiSnapshotBuildForDiagnostics;
            ForceLegacyAiNearestFilterForDiagnostics =
                forceLegacyAiNearestFilterForDiagnostics;
            EnableAiNearestFlatShadowForDiagnostics =
                enableAiNearestFlatShadowForDiagnostics;
            ForceAiNearestFlatQueryForDiagnostics =
                forceAiNearestFlatQueryForDiagnostics;
            ForceLegacyLateRuntimeSnapshotsForDiagnostics =
                forceLegacyLateRuntimeSnapshotsForDiagnostics;
            OutputPath = outputPath ?? string.Empty;
        }

        internal ProductionEntityStressMode Mode { get; }
        internal ProductionEntityStressInputMode InputMode { get; }
        internal int EntityCount { get; }
        internal int WarmupTicks { get; }
        internal int SampleTicks { get; }
        internal int SpawnBatchSize { get; }
        internal int MaxCatchUpTicksPerFrame { get; }
        internal int MaxBacklogTicks { get; }
        internal bool EnablePhaseTiming { get; }
        internal bool EnablePresentationTiming { get; }
        internal bool EnableDetailPhaseTiming { get; }
        internal CollisionFormalCollectorMode FormalCollectorMode { get; }
        internal bool ForceRoleAwareTreeForDiagnostics { get; }
        internal bool ForceRoleAwareDirectForDiagnostics { get; }
        internal bool ForceLegacyZeroAttackItrShortCircuitForDiagnostics { get; }
        internal bool ForceLegacyPreInteractionForDiagnostics { get; }
        internal bool ForceLegacyCharacterInputPassForDiagnostics { get; }
        internal bool ForceLegacyAiSnapshotBuildForDiagnostics { get; }
        internal bool ForceLegacyAiNearestFilterForDiagnostics { get; }
        internal bool EnableAiNearestFlatShadowForDiagnostics { get; }
        internal bool ForceAiNearestFlatQueryForDiagnostics { get; }
        internal bool ForceLegacyLateRuntimeSnapshotsForDiagnostics { get; }
        internal string OutputPath { get; }
        internal bool AutoCleanup => Mode == ProductionEntityStressMode.Smoke50;

        internal static ProductionEntityStressConfig FromRequest(
            ProductionEntityStressRequest request,
            string projectRoot)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            ProductionEntityStressMode mode;
            switch (action)
            {
                case "smoke":
                case "smoke50":
                    mode = ProductionEntityStressMode.Smoke50;
                    break;
                case "dispersed":
                case "dispersed1000":
                    mode = ProductionEntityStressMode.Dispersed1000;
                    break;
                case "concentrated":
                case "concentrated1000":
                    mode = ProductionEntityStressMode.Concentrated1000;
                    break;
                default:
                    throw new ArgumentException(
                        $"Unknown production stress action '{request.action}'.",
                        nameof(request));
            }

            string outputPath = string.IsNullOrWhiteSpace(request.outputPath)
                ? $"Temp/NTSD_ProductionEntityStress.{action}.json"
                : request.outputPath;
            outputPath = Path.IsPathRooted(outputPath)
                ? Path.GetFullPath(outputPath)
                : Path.GetFullPath(Path.Combine(projectRoot, outputPath));

            int warmupTicks = mode == ProductionEntityStressMode.Smoke50
                ? Math.Min(Math.Max(0, request.warmupTicks), 5)
                : request.warmupTicks;
            int sampleTicks = mode == ProductionEntityStressMode.Smoke50
                ? Math.Min(Math.Max(1, request.sampleTicks), 30)
                : request.sampleTicks;
            CollisionFormalCollectorMode formalCollectorMode =
                ParseFormalCollectorMode(request.formalCollectorMode);
            return new ProductionEntityStressConfig(
                mode,
                ParseInputMode(request.inputMode),
                warmupTicks,
                sampleTicks,
                request.spawnBatchSize,
                request.maxCatchUpTicksPerFrame,
                request.maxBacklogTicks,
                request.enablePhaseTiming,
                request.enablePresentationTiming,
                request.enableDetailPhaseTiming,
                formalCollectorMode,
                request.forceRoleAwareTreeForDiagnostics,
                request.forceRoleAwareDirectForDiagnostics,
                request.forceLegacyZeroAttackItrShortCircuitForDiagnostics,
                request.forceLegacyPreInteractionForDiagnostics,
                request.forceLegacyCharacterInputPassForDiagnostics,
                request.forceLegacyAiSnapshotBuildForDiagnostics,
                request.forceLegacyAiNearestFilterForDiagnostics,
                request.enableAiNearestFlatShadowForDiagnostics,
                request.forceAiNearestFlatQueryForDiagnostics,
                request.forceLegacyLateRuntimeSnapshotsForDiagnostics,
                outputPath);
        }

        internal static ProductionEntityStressInputMode ParseInputMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                case "ai":
                    return ProductionEntityStressInputMode.Ai;
                case "none":
                    return ProductionEntityStressInputMode.None;
                default:
                    throw new ArgumentException(
                        $"Unknown production stress input mode '{value}'. Expected ai or none.",
                        nameof(value));
            }
        }

        internal static string FormatInputMode(ProductionEntityStressInputMode mode)
        {
            switch (mode)
            {
                case ProductionEntityStressInputMode.Ai:
                    return "ai";
                case ProductionEntityStressInputMode.None:
                    return "none";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        internal static CollisionFormalCollectorMode ParseFormalCollectorMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "configured":
                    return CollisionFormalCollectorMode.Configured;
                case "legacy":
                    return CollisionFormalCollectorMode.ForceLegacyUnionAabb;
                case "role":
                    return CollisionFormalCollectorMode.ForceRoleAware;
                case "brute":
                    return CollisionFormalCollectorMode.ForceBruteForce;
                default:
                    throw new ArgumentException(
                        $"Unknown formal collector mode '{value}'. " +
                        "Expected configured, legacy, role, or brute.",
                        nameof(value));
            }
        }

        internal static string FormatFormalCollectorMode(CollisionFormalCollectorMode mode)
        {
            switch (mode)
            {
                case CollisionFormalCollectorMode.Configured:
                    return "configured";
                case CollisionFormalCollectorMode.ForceLegacyUnionAabb:
                    return "legacy";
                case CollisionFormalCollectorMode.ForceRoleAware:
                    return "role";
                case CollisionFormalCollectorMode.ForceBruteForce:
                    return "brute";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }

    [Serializable]
    public sealed class ProductionEntityStressMetricSummary
    {
        public bool available;
        public string unit;
        public string source;
        public string unavailableReason;
        public int sampleCount;
        public double average;
        public double maximum;
        public double p95;
        public double p99;
    }

    [Serializable]
    public sealed class ProductionEntityStressPhaseTimingSummary
    {
        public string phase;
        public ProductionEntityStressMetricSummary timing;
    }

    [Serializable]
    public sealed class ProductionEntityStressLateRuntimeSnapshotTimingSummary
    {
        public string stage;
        public long callCount;
        public ProductionEntityStressMetricSummary timing;
    }

    [Serializable]
    public sealed class ProductionEntityStressAiInputDetailCounters
    {
        public bool available;
        public string unavailableReason;
        public long aiCount;
        public long spatialQueryCount;
        public long queriedHandleVisits;
        public long candidateVisits;
        public long radiusExpansions;
        public long bruteFallbackCount;
        public long bruteSlotVisits;
        public long phase1ListVisits;
        public long refreshCount;
        public long[] radiusHistogram =
            new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
    }

    [Serializable]
    public sealed class ProductionEntityStressLateOpointCounters
    {
        public int sampledLatePasses;
        public long opointNoRoleSum;
        public int opointNoRolePeak;
        public long invalidFirstSum;
        public int invalidFirstPeak;
        public long attackingRejectedSum;
        public int attackingRejectedPeak;
        public long characterFrameDelayRejectedSum;
        public int characterFrameDelayRejectedPeak;
        public long eligibleSum;
        public int eligiblePeak;
        public long factoryInvokedSum;
        public int factoryInvokedPeak;
    }

    [Serializable]
    public sealed class ProductionEntityStressTeardownReport
    {
        public bool attempted;
        public bool restored;
        public bool activeStateRestored;
        public bool driverStateRestored;
        public bool loggingStateRestored;
        public bool forceRoleAwareDirectForDiagnosticsRestored;
        public bool forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored;
        public bool forceLegacyPreInteractionForDiagnosticsRestored;
        public bool forceLegacyCharacterInputPassForDiagnosticsRestored;
        public bool forceLegacyAiSnapshotBuildForDiagnosticsRestored;
        public bool forceLegacyAiNearestFilterForDiagnosticsRestored;
        public bool enableAiNearestFlatShadowForDiagnosticsRestored;
        public bool forceAiNearestFlatQueryForDiagnosticsRestored;
        public bool forceLegacyLateRuntimeSnapshotsForDiagnosticsRestored;
        public int activeGameObjectsBefore;
        public int activeGameObjectsAfter;
        public int worldObjectsBefore;
        public int worldObjectsAfter;
        public int worldEntitiesBefore;
        public int worldEntitiesAfter;
        public int claimedSlotsBefore;
        public int claimedSlotsAfter;
        public int objectPoolActiveBeforeRun;
        public int objectPoolActiveAfter;
        public int objectPoolAvailableBeforeRun;
        public int objectPoolAvailableAfter;
        public int retainedInactiveObjectPoolCapacityBeforeRun;
        public int retainedInactiveObjectPoolCapacityAfter;
        public int retainedInactiveObjectPoolCapacityDelta;
        public string retainedInactiveObjectPoolCapacityPolicy;
        public int referencePoolActiveBeforeRun;
        public int referencePoolActiveAfter;
        public int cleanupExceptionCount;
        public string cleanupExceptions;
        public string evidence;
    }

    [Serializable]
    public sealed class ProductionEntityStressLoggingPolicyReport
    {
        public string originalFilterLogType;
        public string runningFilterLogType;
        public string policy;
        public bool applied;
        public bool restored;
    }

    [Serializable]
    public sealed class ProductionEntityStressReport
    {
        public string schema = "ntsd-production-entity-stress/v1";
        public string status;
        public string mode;
        public string inputMode;
        public string startedUtc;
        public string updatedUtc;
        public string unityVersion;
        public string platform;
        public string scene;
        public string stressRootName;
        public string outputPath;
        public string failure;
        public bool harnessValidity;
        public string performanceVerdict = "EvidenceOnlyNoThreshold";
        public int requestedEntityCount;
        public int selectedCharacterOid;
        public int totalEntitiesCreated;
        public int lifecycleReplacements;
        public int activeGameObjectCount;
        public int stressRootChildCount;
        public int worldObjectCount;
        public int worldEntityCount;
        public int peakWorldEntityCount;
        public int claimedRuntimeSlotCount;
        public string runtimeProfile;
        public int runtimeSlotCapacity;
        public string broadphaseBackend;
        public string formalCollectorRequestedMode;
        public string formalCollectorMode;
        public bool forceRoleAwareTreeForDiagnosticsRequested;
        public bool forceRoleAwareTreeForDiagnosticsEffective;
        public bool forceRoleAwareDirectForDiagnosticsRequested;
        public bool forceRoleAwareDirectForDiagnosticsEffective;
        public bool forceLegacyZeroAttackItrShortCircuitForDiagnosticsRequested;
        public bool forceLegacyZeroAttackItrShortCircuitForDiagnosticsEffective;
        public bool forceLegacyPreInteractionForDiagnosticsRequested;
        public bool forceLegacyPreInteractionForDiagnosticsEffective;
        public bool forceLegacyCharacterInputPassForDiagnosticsRequested;
        public bool forceLegacyCharacterInputPassForDiagnosticsEffective;
        public bool forceLegacyAiSnapshotBuildForDiagnosticsRequested;
        public bool forceLegacyAiSnapshotBuildForDiagnosticsEffective;
        public bool forceLegacyAiNearestFilterForDiagnosticsRequested;
        public bool forceLegacyAiNearestFilterForDiagnosticsEffective;
        public bool enableAiNearestFlatShadowForDiagnosticsRequested;
        public bool enableAiNearestFlatShadowForDiagnosticsEffective;
        public bool forceAiNearestFlatQueryForDiagnosticsRequested;
        public bool forceAiNearestFlatQueryForDiagnosticsEffective;
        public bool forceLegacyLateRuntimeSnapshotsForDiagnosticsRequested;
        public bool forceLegacyLateRuntimeSnapshotsForDiagnosticsEffective;
        public int aiNearestFlatShadowQueryCount;
        public int aiNearestFlatShadowUnavailableCount;
        public int aiNearestFlatShadowMismatchCount;
        public int aiNearestFlatGroundSequentialFallbackCount;
        public int aiNearestFlatFormalQueryCount;
        public int aiNearestFlatFormalFallbackCount;
        public string aiNearestFlatFirstShadowMismatch;
        public int lateFrameTickRuntimeSnapshotProofCountPeak;
        public int skippedLateFrameTickRuntimeSnapshotCountPeak;
        public int skippedLateDeathRuntimeSnapshotCountPeak;
        public int skippedLateTailRuntimeSnapshotCountPeak;
        public int formalCollectorBodyEntries;
        public int formalCollectorItrQueries;
        public int logicTicksExecuted;
        public int warmupTicksCompleted;
        public int sampledLogicTicks;
        public int sampledUnityFrames;
        public int framesWithCatchUp;
        public int maximumCatchUpTicksInFrame;
        public int currentBacklogTicks;
        public int maximumBacklogTicks;
        public int droppedBacklogTicks;
        public long aiControlledEntityTicks;
        public long collisionCandidateCountSum;
        public int collisionCandidateCountPeak;
        public long broadphasePairCountSum;
        public int broadphasePairCountPeak;
        public int broadphaseFallbackParticipantPeak;
        public int broadphaseAbortedTicks;
        public int broadphaseLastIndexedCount;
        public int damageStatTotal;
        public int killStatTotal;
        public bool opointCounterAvailable;
        public int observedOpointCreates;
        public string opointCounterReason;
        public ProductionEntityStressMetricSummary logicTickMilliseconds;
        public ProductionEntityStressMetricSummary logicTickWithPresentationMilliseconds;
        public ProductionEntityStressMetricSummary logicTickWithoutPresentationMilliseconds;
        public ProductionEntityStressMetricSummary unityFrameMilliseconds;
        public ProductionEntityStressMetricSummary logicTickAllocatedBytes;
        public bool phaseTimingEnabled;
        public string phaseTimingSource;
        public List<ProductionEntityStressPhaseTimingSummary> phaseTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public ProductionEntityStressMetricSummary phaseTimingUnattributedMilliseconds;
        public bool presentationTimingEnabled;
        public string presentationTimingSource;
        public string presentationTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> presentationTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public bool detailPhaseTimingEnabled;
        public string detailPhaseTimingSource;
        public string detailPhaseTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> detailPhaseTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public string aiInputDetailTimingSource;
        public string aiInputDetailTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> aiInputDetailTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public string lateRuntimeSnapshotTimingSource;
        public string lateRuntimeSnapshotTimingUnavailableReason;
        public List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>
            lateRuntimeSnapshotTimings =
                new List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>();
        public ProductionEntityStressAiInputDetailCounters aiInputDetailCounters =
            new ProductionEntityStressAiInputDetailCounters();
        public ProductionEntityStressLateOpointCounters lateOpointCounters =
            new ProductionEntityStressLateOpointCounters();
        public ProductionEntityStressLoggingPolicyReport loggingPolicy =
            new ProductionEntityStressLoggingPolicyReport();
        public ProductionEntityStressTeardownReport teardown =
            new ProductionEntityStressTeardownReport();
    }

    internal sealed class ProductionEntityStressLoggingPolicy
    {
        private readonly Func<LogType> getFilterLogType;
        private readonly Action<LogType> setFilterLogType;
        private bool applied;
        private LogType originalFilterLogType;

        internal ProductionEntityStressLoggingPolicy()
            : this(
                () => Debug.unityLogger.filterLogType,
                value => Debug.unityLogger.filterLogType = value)
        {
        }

        internal ProductionEntityStressLoggingPolicy(
            Func<LogType> getFilterLogType,
            Action<LogType> setFilterLogType)
        {
            this.getFilterLogType = getFilterLogType ?? throw new ArgumentNullException(nameof(getFilterLogType));
            this.setFilterLogType = setFilterLogType ?? throw new ArgumentNullException(nameof(setFilterLogType));
        }

        internal void Apply(ProductionEntityStressLoggingPolicyReport report)
        {
            if (applied)
                return;

            originalFilterLogType = getFilterLogType();
            setFilterLogType(LogType.Error);
            applied = true;
            UpdateReport(report);
        }

        internal void Restore(ProductionEntityStressLoggingPolicyReport report)
        {
            if (applied)
            {
                setFilterLogType(originalFilterLogType);
                applied = false;
            }
            UpdateReport(report);
            if (report != null)
                report.restored = getFilterLogType() == originalFilterLogType;
        }

        private void UpdateReport(ProductionEntityStressLoggingPolicyReport report)
        {
            if (report == null)
                return;

            report.originalFilterLogType = originalFilterLogType.ToString();
            report.runningFilterLogType = LogType.Error.ToString();
            report.policy = "Suppress Log and Warning during the stress run while retaining Error.";
            report.applied = applied;
        }
    }

    internal static class ProductionEntityStressStatistics
    {
        internal static ProductionEntityStressMetricSummary Summarize(
            IReadOnlyList<double> values,
            string unit,
            string source)
        {
            var result = new ProductionEntityStressMetricSummary
            {
                available = values != null && values.Count > 0,
                unit = unit ?? string.Empty,
                source = source ?? string.Empty,
                unavailableReason = values == null || values.Count == 0
                    ? "No completed samples."
                    : string.Empty,
                sampleCount = values?.Count ?? 0,
            };
            if (!result.available)
                return result;

            var sorted = new double[values.Count];
            double sum = 0d;
            double maximum = double.MinValue;
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i];
                sorted[i] = value;
                sum += value;
                maximum = Math.Max(maximum, value);
            }
            Array.Sort(sorted);
            result.average = sum / values.Count;
            result.maximum = maximum;
            result.p95 = Percentile(sorted, 0.95d);
            result.p99 = Percentile(sorted, 0.99d);
            return result;
        }

        internal static double Percentile(IReadOnlyList<double> sortedValues, double percentile)
        {
            if (sortedValues == null || sortedValues.Count == 0)
                return 0d;
            double rank = Math.Max(0d, Math.Min(1d, percentile)) * (sortedValues.Count - 1);
            int lower = (int)Math.Floor(rank);
            int upper = Math.Min(sortedValues.Count - 1, lower + 1);
            double blend = rank - lower;
            return sortedValues[lower] + (sortedValues[upper] - sortedValues[lower]) * blend;
        }
    }

    internal sealed class ProductionEntityStressPhaseTimingCollector
    {
        internal const string Source =
            "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; " +
            "diagnostic evidence only, with no performance threshold.";

        private readonly List<double>[] phaseSamples;
        private readonly List<double> unattributedSamples = new List<double>(512);

        internal ProductionEntityStressPhaseTimingCollector()
        {
            phaseSamples = new List<double>[BattleTickPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }

        internal void CaptureAfterTick(
            BattleTickPhaseDiagnostics diagnostics,
            double outerMilliseconds,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled ||
                completedTickCount <= warmupTickCount)
            {
                return;
            }

            double timestampToMilliseconds =
                1000d / BattleTickPhaseDiagnostics.TimestampFrequency;
            double phaseSumMilliseconds = 0d;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double phaseMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattleTickPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], phaseMilliseconds);
                phaseSumMilliseconds += phaseMilliseconds;
            }

            AddRollingSample(
                unattributedSamples,
                Math.Max(0d, outerMilliseconds - phaseSumMilliseconds));
            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.phaseTimings.Clear();
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattleTickPhase phase = (BattleTickPhase)i;
                report.phaseTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleTickPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }

            report.phaseTimingUnattributedMilliseconds =
                ProductionEntityStressStatistics.Summarize(
                    unattributedSamples,
                    "ms",
                    "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings");
        }

        internal static void PopulateDisabledReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.phaseTimingEnabled = false;
            report.phaseTimingSource = string.Empty;
            if (report.phaseTimings == null)
                report.phaseTimings = new List<ProductionEntityStressPhaseTimingSummary>();
            else
                report.phaseTimings.Clear();
            report.phaseTimingUnattributedMilliseconds = new ProductionEntityStressMetricSummary
            {
                available = false,
                unit = "ms",
                unavailableReason =
                    "Disabled by request; set enablePhaseTiming to true to collect battle tick phase timings.",
            };
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal sealed class ProductionEntityStressPresentationTimingCollector
    {
        internal const string Source =
            "Opt-in coarse Stopwatch timestamps around presentation publication boundaries; " +
            "independent from battle tick and per-entity detail diagnostics.";

        private readonly List<double>[] phaseSamples;
        private long lastCapturedSequence;

        internal ProductionEntityStressPresentationTimingCollector()
        {
            phaseSamples = new List<double>[BattlePresentationPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }

        internal void CaptureAfterTick(
            BattlePresentationPhaseDiagnostics diagnostics,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled)
            {
                return;
            }

            long completedSequence = diagnostics.CompletedSampleSequence;
            if (completedSequence <= 0 || completedSequence == lastCapturedSequence)
                return;

            lastCapturedSequence = completedSequence;
            if (completedTickCount <= warmupTickCount)
                return;

            double timestampToMilliseconds =
                1000d / BattlePresentationPhaseDiagnostics.TimestampFrequency;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double elapsedMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattlePresentationPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], elapsedMilliseconds);
            }

            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            if (report.presentationTimings == null)
            {
                report.presentationTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.presentationTimings.Clear();
            }

            report.presentationTimingEnabled = true;
            report.presentationTimingSource = Source;
            report.presentationTimingUnavailableReason = SampleCount > 0
                ? string.Empty
                : "No completed presentation timing samples.";
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattlePresentationPhase phase = (BattlePresentationPhase)i;
                report.presentationTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattlePresentationPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }
        }

        internal static void PopulateDisabledReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.presentationTimingEnabled = false;
            report.presentationTimingSource = string.Empty;
            report.presentationTimingUnavailableReason =
                "Disabled by request; set enablePresentationTiming to true to collect " +
                "coarse presentation publication timings.";
            if (report.presentationTimings == null)
            {
                report.presentationTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.presentationTimings.Clear();
            }
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal sealed class ProductionEntityStressLogicTickTimingCollector
    {
        private const string Source =
            "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick";

        private readonly List<double> allSamples = new List<double>(512);
        private readonly List<double> withPresentationSamples = new List<double>(512);
        private readonly List<double> withoutPresentationSamples = new List<double>(512);

        internal int AllSampleCount => allSamples.Count;
        internal int WithPresentationSampleCount => withPresentationSamples.Count;
        internal int WithoutPresentationSampleCount => withoutPresentationSamples.Count;

        internal void AddSample(double elapsedMilliseconds, bool buildPresentation)
        {
            AddRollingSample(allSamples, elapsedMilliseconds);
            AddRollingSample(
                buildPresentation ? withPresentationSamples : withoutPresentationSamples,
                elapsedMilliseconds);
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.logicTickMilliseconds = ProductionEntityStressStatistics.Summarize(
                allSamples,
                "ms",
                Source);
            report.logicTickWithPresentationMilliseconds = ProductionEntityStressStatistics.Summarize(
                withPresentationSamples,
                "ms",
                Source + "; buildPresentation=true");
            report.logicTickWithoutPresentationMilliseconds = ProductionEntityStressStatistics.Summarize(
                withoutPresentationSamples,
                "ms",
                Source + "; buildPresentation=false");
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal sealed class ProductionEntityStressDetailPhaseTimingCollector
    {
        internal const string Source =
            "Independent Stopwatch timestamps accumulated inside CharacterInput and " +
            "LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried " +
            "from the preceding render pass into the next logic-tick sample. Nested diagnostic " +
            "evidence only, with no performance threshold.";
        internal const string AiInputSource =
            "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; " +
            "nested diagnostic evidence only, with no performance threshold.";
        internal const string LateRuntimeSnapshotSource =
            "Independent Stopwatch timestamps around individual LateEntityUpdate " +
            "RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, " +
            "not source-code line numbers; nested diagnostic evidence only, with no " +
            "performance threshold.";

        private readonly List<double>[] phaseSamples;
        private List<double>[] aiInputPhaseSamples;
        private List<double>[] lateRuntimeSnapshotSamples;
        private readonly long[] radiusHistogram =
            new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
        private readonly long[] lateRuntimeSnapshotCallCounts =
            new long[(int)BattleLateRuntimeSnapshotStage.Count];
        private long aiCount;
        private long spatialQueryCount;
        private long queriedHandleVisits;
        private long candidateVisits;
        private long radiusExpansions;
        private long bruteFallbackCount;
        private long bruteSlotVisits;
        private long phase1ListVisits;
        private long refreshCount;

        internal ProductionEntityStressDetailPhaseTimingCollector()
        {
            phaseSamples = new List<double>[BattleTickDetailPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }
        internal int AiInputSampleCount { get; private set; }
        internal bool AiInputPhaseSamplesAllocatedForDiagnostics =>
            aiInputPhaseSamples != null;
        internal int LateRuntimeSnapshotSampleCount { get; private set; }
        internal bool LateRuntimeSnapshotSamplesAllocatedForDiagnostics =>
            lateRuntimeSnapshotSamples != null;

        internal void CaptureAfterTick(
            BattleTickDetailPhaseDiagnostics diagnostics,
            BattleAiInputDetailDiagnostics aiDiagnostics,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled ||
                completedTickCount <= warmupTickCount)
            {
                return;
            }

            double timestampToMilliseconds =
                1000d / BattleTickDetailPhaseDiagnostics.TimestampFrequency;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double phaseMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattleTickDetailPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], phaseMilliseconds);
            }

            if (aiDiagnostics != null && aiDiagnostics.Enabled)
            {
                EnsureAiInputPhaseSamples();
                double aiTimestampToMilliseconds =
                    1000d / BattleAiInputDetailDiagnostics.TimestampFrequency;
                for (int i = 0; i < aiInputPhaseSamples.Length; i++)
                {
                    double phaseMilliseconds = aiDiagnostics.GetLastElapsedTimestampTicks(
                        (BattleAiInputDetailPhase)i) * aiTimestampToMilliseconds;
                    AddRollingSample(aiInputPhaseSamples[i], phaseMilliseconds);
                }

                aiCount += aiDiagnostics.AiCount;
                spatialQueryCount += aiDiagnostics.SpatialQueryCount;
                queriedHandleVisits += aiDiagnostics.QueriedHandleVisits;
                candidateVisits += aiDiagnostics.CandidateVisits;
                radiusExpansions += aiDiagnostics.RadiusExpansions;
                bruteFallbackCount += aiDiagnostics.BruteFallbackCount;
                bruteSlotVisits += aiDiagnostics.BruteSlotVisits;
                phase1ListVisits += aiDiagnostics.Phase1ListVisits;
                refreshCount += aiDiagnostics.RefreshCount;
                for (int i = 0; i < radiusHistogram.Length; i++)
                    radiusHistogram[i] += aiDiagnostics.GetRadiusHistogramValue(i);
                AiInputSampleCount++;
            }

            EnsureLateRuntimeSnapshotSamples();
            for (int i = 0; i < lateRuntimeSnapshotSamples.Length; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                double elapsedMilliseconds = diagnostics
                    .GetLastLateRuntimeSnapshotElapsedTimestampTicks(stage) *
                    timestampToMilliseconds;
                AddRollingSample(lateRuntimeSnapshotSamples[i], elapsedMilliseconds);
                lateRuntimeSnapshotCallCounts[i] +=
                    diagnostics.GetLastLateRuntimeSnapshotCallCount(stage);
            }
            LateRuntimeSnapshotSampleCount++;

            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            if (report.detailPhaseTimings == null)
            {
                report.detailPhaseTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.detailPhaseTimings.Clear();
            }
            if (report.aiInputDetailTimings == null)
            {
                report.aiInputDetailTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.aiInputDetailTimings.Clear();
            }
            if (report.lateRuntimeSnapshotTimings == null)
            {
                report.lateRuntimeSnapshotTimings =
                    new List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>();
            }
            else
            {
                report.lateRuntimeSnapshotTimings.Clear();
            }
            if (report.aiInputDetailCounters == null)
            {
                report.aiInputDetailCounters =
                    new ProductionEntityStressAiInputDetailCounters();
            }
            else if (report.aiInputDetailCounters.radiusHistogram == null ||
                     report.aiInputDetailCounters.radiusHistogram.Length !=
                     BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount)
            {
                report.aiInputDetailCounters.radiusHistogram =
                    new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
            }
            if (!report.detailPhaseTimingEnabled)
            {
                report.detailPhaseTimingSource = string.Empty;
                report.detailPhaseTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.";
                report.aiInputDetailCounters.available = false;
                report.aiInputDetailCounters.unavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail counters.";
                report.aiInputDetailTimingSource = string.Empty;
                report.aiInputDetailTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail timings.";
                report.lateRuntimeSnapshotTimingSource = string.Empty;
                report.lateRuntimeSnapshotTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect " +
                    "LateEntityUpdate RefreshRuntimeSnapshot timings.";
                return;
            }

            report.detailPhaseTimingSource = Source;
            report.detailPhaseTimingUnavailableReason = string.Empty;
            report.aiInputDetailTimingSource = AiInputSource;
            report.aiInputDetailTimingUnavailableReason = AiInputSampleCount > 0
                ? string.Empty
                : "No completed AI input detail timing samples.";
            report.aiInputDetailCounters.available = AiInputSampleCount > 0;
            report.aiInputDetailCounters.unavailableReason = AiInputSampleCount > 0
                ? string.Empty
                : "No completed AI input detail timing samples.";
            report.aiInputDetailCounters.aiCount = aiCount;
            report.aiInputDetailCounters.spatialQueryCount = spatialQueryCount;
            report.aiInputDetailCounters.queriedHandleVisits = queriedHandleVisits;
            report.aiInputDetailCounters.candidateVisits = candidateVisits;
            report.aiInputDetailCounters.radiusExpansions = radiusExpansions;
            report.aiInputDetailCounters.bruteFallbackCount = bruteFallbackCount;
            report.aiInputDetailCounters.bruteSlotVisits = bruteSlotVisits;
            report.aiInputDetailCounters.phase1ListVisits = phase1ListVisits;
            report.aiInputDetailCounters.refreshCount = refreshCount;
            Array.Copy(radiusHistogram, report.aiInputDetailCounters.radiusHistogram, radiusHistogram.Length);
            report.lateRuntimeSnapshotTimingSource = LateRuntimeSnapshotSource;
            report.lateRuntimeSnapshotTimingUnavailableReason =
                LateRuntimeSnapshotSampleCount > 0
                    ? string.Empty
                    : "No completed LateEntityUpdate RefreshRuntimeSnapshot timing samples.";
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattleTickDetailPhase phase = (BattleTickDetailPhase)i;
                report.detailPhaseTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleTickDetailPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }

            for (int i = 0; i < BattleAiInputDetailDiagnostics.PhaseCount; i++)
            {
                BattleAiInputDetailPhase phase = (BattleAiInputDetailPhase)i;
                report.aiInputDetailTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleAiInputDetailDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        aiInputPhaseSamples?[i],
                        "ms",
                        AiInputSource),
                });
            }

            for (int i = 0; i < (int)BattleLateRuntimeSnapshotStage.Count; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                report.lateRuntimeSnapshotTimings.Add(
                    new ProductionEntityStressLateRuntimeSnapshotTimingSummary
                    {
                        stage = BattleTickDetailPhaseDiagnostics
                            .GetLateRuntimeSnapshotStageName(stage),
                        callCount = lateRuntimeSnapshotCallCounts[i],
                        timing = ProductionEntityStressStatistics.Summarize(
                            lateRuntimeSnapshotSamples?[i],
                            "ms",
                            LateRuntimeSnapshotSource),
                    });
            }
        }

        private void EnsureAiInputPhaseSamples()
        {
            if (aiInputPhaseSamples != null)
                return;

            aiInputPhaseSamples =
                new List<double>[BattleAiInputDetailDiagnostics.PhaseCount];
            for (int i = 0; i < aiInputPhaseSamples.Length; i++)
                aiInputPhaseSamples[i] = new List<double>(512);
        }

        private void EnsureLateRuntimeSnapshotSamples()
        {
            if (lateRuntimeSnapshotSamples != null)
                return;

            lateRuntimeSnapshotSamples =
                new List<double>[(int)BattleLateRuntimeSnapshotStage.Count];
            for (int i = 0; i < lateRuntimeSnapshotSamples.Length; i++)
                lateRuntimeSnapshotSamples[i] = new List<double>(512);
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal static class ProductionEntityStressPhaseTimingLifecycle
    {
        internal static void Disable(SimulationWorld world)
        {
            world?.DisableBattleTickPhaseDiagnosticsForDiagnostics();
            world?.DisableBattlePresentationPhaseDiagnosticsForDiagnostics();
            world?.DisableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            world?.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
        }
    }

    internal static class ProductionEntityStressRunStatusPolicy
    {
        internal static string ResolveCleanupStatus(
            string currentStatus,
            string reason,
            bool restored)
        {
            if (!string.Equals(reason, "runner-destroyed", StringComparison.Ordinal))
                return currentStatus;

            return restored ? "InterruptedCleanly" : "InterruptedWithResidue";
        }
    }

    internal static class ProductionEntityStressPopulationPolicy
    {
        internal static bool Evaluate(
            int requestedEntityCount,
            int activeGameObjectCount,
            int rootChildCount,
            int worldObjectCount,
            int worldEntityCount,
            int claimedRuntimeSlotCount)
        {
            return requestedEntityCount > 0 &&
                   activeGameObjectCount == requestedEntityCount &&
                   rootChildCount == requestedEntityCount &&
                   worldObjectCount == requestedEntityCount * 2 &&
                   worldEntityCount == requestedEntityCount &&
                   claimedRuntimeSlotCount == requestedEntityCount;
        }
    }

    internal static class ProductionEntityStressTeardownPolicy
    {
        internal static bool IsRestored(
            int activeGameObjects,
            int worldObjects,
            int worldEntities,
            int claimedSlots,
            int objectPoolActive,
            int objectPoolAvailable,
            int referencePoolActive,
            int objectPoolActiveBaseline,
            int objectPoolAvailableBaseline,
            int referencePoolActiveBaseline)
        {
            _ = objectPoolAvailable;
            _ = objectPoolAvailableBaseline;
            return activeGameObjects == 0 &&
                   worldObjects == 0 &&
                   worldEntities == 0 &&
                   claimedSlots == 0 &&
                   objectPoolActive == objectPoolActiveBaseline &&
                   referencePoolActive == referencePoolActiveBaseline;
        }

        internal static int CountActiveStressRootGameObjects(Transform stressRoot)
        {
            if (stressRoot == null)
                return 0;

            int count = 0;
            for (int i = 0; i < stressRoot.childCount; i++)
            {
                GameObject child = stressRoot.GetChild(i)?.gameObject;
                if (child != null && child.activeInHierarchy)
                    count++;
            }
            return count;
        }

        internal static string BuildEvidence(
            string reason,
            ProductionEntityStressTeardownReport teardown)
        {
            if (teardown == null)
                return string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "reason={0}; restored={1}; activeCleanupRestored={2}; driverRestored={3}; " +
                "loggerRestored={4}; cleanupExceptions={5}; activeGO={6}->{7}; " +
                "worldObjects={8}->{9}; worldEntities={10}->{11}; claimed={12}->{13}; " +
                "objectPoolActive={14}->{15}; referencePoolActive={16}->{17}; " +
                "retainedInactiveObjectPoolCapacity={18}->{19} (delta={20}; " +
                "doesNotAffectRestored=True); preInteractionSwitchRestored={21}; " +
                "characterInputPassSwitchRestored={22}; " +
                "aiSnapshotBuildSwitchRestored={23}; aiNearestFilterSwitchRestored={24}; " +
                "aiNearestFlatShadowSwitchRestored={25}; " +
                "aiNearestFlatFormalSwitchRestored={26}; " +
                "legacyLateRuntimeSnapshotsSwitchRestored={27}; " +
                "zeroAttackItrShortCircuitSwitchRestored={28}; " +
                "roleAwareDirectSwitchRestored={29}",
                reason,
                teardown.restored,
                teardown.activeStateRestored,
                teardown.driverStateRestored,
                teardown.loggingStateRestored,
                teardown.cleanupExceptionCount,
                teardown.activeGameObjectsBefore,
                teardown.activeGameObjectsAfter,
                teardown.worldObjectsBefore,
                teardown.worldObjectsAfter,
                teardown.worldEntitiesBefore,
                teardown.worldEntitiesAfter,
                teardown.claimedSlotsBefore,
                teardown.claimedSlotsAfter,
                teardown.objectPoolActiveBeforeRun,
                teardown.objectPoolActiveAfter,
                teardown.referencePoolActiveBeforeRun,
                teardown.referencePoolActiveAfter,
                teardown.retainedInactiveObjectPoolCapacityBeforeRun,
                teardown.retainedInactiveObjectPoolCapacityAfter,
                teardown.retainedInactiveObjectPoolCapacityDelta,
                teardown.forceLegacyPreInteractionForDiagnosticsRestored,
                teardown.forceLegacyCharacterInputPassForDiagnosticsRestored,
                teardown.forceLegacyAiSnapshotBuildForDiagnosticsRestored,
                teardown.forceLegacyAiNearestFilterForDiagnosticsRestored,
                teardown.enableAiNearestFlatShadowForDiagnosticsRestored,
                teardown.forceAiNearestFlatQueryForDiagnosticsRestored,
                teardown.forceLegacyLateRuntimeSnapshotsForDiagnosticsRestored,
                teardown.forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored,
                teardown.forceRoleAwareDirectForDiagnosticsRestored);
        }
    }

    internal sealed class ProductionEntityStressCleanupJournal
    {
        private readonly List<string> failures = new List<string>();

        internal int FailureCount => failures.Count;

        internal bool Attempt(string phase, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
                return true;
            }
            catch (Exception exception)
            {
                failures.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1}: {2}",
                    phase ?? "cleanup",
                    exception.GetType().Name,
                    exception.Message));
                return false;
            }
        }

        internal string FormatFailures()
        {
            return string.Join(" | ", failures);
        }
    }

    internal static class ProductionEntityStressDerivedObservationPolicy
    {
        internal static bool TryRecord(
            RuntimeEntityHandle handle,
            ISet<RuntimeEntityHandle> harnessOwned,
            ISet<RuntimeEntityHandle> observedDerived)
        {
            return handle.IsValid &&
                   (harnessOwned == null || !harnessOwned.Contains(handle)) &&
                   observedDerived != null &&
                   observedDerived.Add(handle);
        }
    }

    internal sealed class ProductionEntityStressAbDiagnosticsLifecycle
    {
        private BruteForceSceneQuery sceneQuery;
        private SimulationWorld world;
        private bool previousForceRoleAwareTree;
        private bool previousForceRoleAwareDirect;
        private bool previousForceLegacyZeroAttackItrShortCircuit;
        private bool previousForceLegacyPreInteraction;
        private bool previousForceLegacyCharacterInputPass;
        private bool previousForceLegacyAiSnapshotBuild;
        private bool previousForceLegacyAiNearestFilter;
        private bool previousEnableAiNearestFlatShadow;
        private bool previousForceAiNearestFlatQuery;
        private bool previousForceLegacyLateRuntimeSnapshots;

        private bool roleAwareTreeApplied;
        private bool roleAwareDirectApplied;
        private bool zeroAttackItrShortCircuitApplied;
        private bool preInteractionApplied;
        private bool characterInputPassApplied;
        private bool aiSnapshotBuildApplied;
        private bool aiNearestFilterApplied;
        private bool aiNearestFlatShadowApplied;
        private bool aiNearestFlatQueryApplied;
        private bool lateRuntimeSnapshotsApplied;
        private bool roleAwareDirectRestored = true;
        private bool zeroAttackItrShortCircuitRestored = true;
        private bool preInteractionRestored = true;
        private bool characterInputPassRestored = true;
        private bool aiSnapshotBuildRestored = true;
        private bool aiNearestFilterRestored = true;
        private bool aiNearestFlatShadowRestored = true;
        private bool aiNearestFlatQueryRestored = true;
        private bool lateRuntimeSnapshotsRestored = true;

        internal bool Applied =>
            roleAwareTreeApplied ||
            roleAwareDirectApplied ||
            zeroAttackItrShortCircuitApplied ||
            preInteractionApplied ||
            characterInputPassApplied ||
            aiSnapshotBuildApplied ||
            aiNearestFilterApplied ||
            aiNearestFlatShadowApplied ||
            aiNearestFlatQueryApplied ||
            lateRuntimeSnapshotsApplied;
        internal bool EffectiveForceRoleAwareTree =>
            sceneQuery != null && sceneQuery.ForceRoleAwareTreeForDiagnostics;
        internal bool EffectiveForceRoleAwareDirect =>
            sceneQuery != null && sceneQuery.ForceRoleAwareDirectForDiagnostics;
        internal bool EffectiveForceLegacyZeroAttackItrShortCircuit =>
            sceneQuery != null &&
            sceneQuery.ForceLegacyZeroAttackItrShortCircuitForDiagnostics;
        internal bool EffectiveForceLegacyPreInteraction =>
            world != null && world.ForceLegacyPreInteractionForDiagnostics;
        internal bool EffectiveForceLegacyCharacterInputPass =>
            world != null && world.ForceLegacyCharacterInputPassForDiagnostics;
        internal bool EffectiveForceLegacyAiSnapshotBuild =>
            world != null && world.ForceLegacyAiSnapshotBuildForDiagnostics;
        internal bool EffectiveForceLegacyAiNearestFilter =>
            world != null && world.ForceLegacyAiNearestFilterForDiagnostics;
        internal bool EffectiveEnableAiNearestFlatShadow =>
            world != null && world.EnableAiNearestFlatShadowForDiagnostics;
        internal bool EffectiveForceAiNearestFlatQuery =>
            world != null && world.ForceAiNearestFlatQueryForDiagnostics;
        internal bool EffectiveForceLegacyLateRuntimeSnapshots =>
            world != null && world.ForceLegacyLateRuntimeSnapshotsForDiagnostics;
        internal bool RoleAwareDirectRestored => roleAwareDirectRestored;
        internal bool ZeroAttackItrShortCircuitRestored =>
            zeroAttackItrShortCircuitRestored;
        internal bool PreInteractionRestored => preInteractionRestored;
        internal bool CharacterInputPassRestored => characterInputPassRestored;
        internal bool AiSnapshotBuildRestored => aiSnapshotBuildRestored;
        internal bool AiNearestFilterRestored => aiNearestFilterRestored;
        internal bool AiNearestFlatShadowRestored => aiNearestFlatShadowRestored;
        internal bool AiNearestFlatQueryRestored => aiNearestFlatQueryRestored;
        internal bool LateRuntimeSnapshotsRestored => lateRuntimeSnapshotsRestored;

        internal void Apply(
            SimulationWorld targetWorld,
            bool forceRoleAwareTree,
            bool forceRoleAwareDirect,
            bool forceLegacyZeroAttackItrShortCircuit,
            bool forceLegacyPreInteraction,
            bool forceLegacyCharacterInputPass,
            bool forceLegacyAiSnapshotBuild,
            bool forceLegacyAiNearestFilter,
            bool enableAiNearestFlatShadow,
            bool forceAiNearestFlatQuery,
            bool forceLegacyLateRuntimeSnapshots)
        {
            if (Applied)
                throw new InvalidOperationException("A/B diagnostic switches are already applied.");
            if (targetWorld?.SceneQuery is not BruteForceSceneQuery targetSceneQuery)
            {
                throw new InvalidOperationException(
                    "Production stress requires BruteForceSceneQuery A/B diagnostics.");
            }

            world = targetWorld;
            sceneQuery = targetSceneQuery;
            previousForceRoleAwareTree = sceneQuery.ForceRoleAwareTreeForDiagnostics;
            previousForceRoleAwareDirect =
                sceneQuery.ForceRoleAwareDirectForDiagnostics;
            previousForceLegacyZeroAttackItrShortCircuit =
                sceneQuery.ForceLegacyZeroAttackItrShortCircuitForDiagnostics;
            previousForceLegacyPreInteraction =
                world.ForceLegacyPreInteractionForDiagnostics;
            previousForceLegacyCharacterInputPass =
                world.ForceLegacyCharacterInputPassForDiagnostics;
            previousForceLegacyAiSnapshotBuild =
                world.ForceLegacyAiSnapshotBuildForDiagnostics;
            previousForceLegacyAiNearestFilter =
                world.ForceLegacyAiNearestFilterForDiagnostics;
            previousEnableAiNearestFlatShadow =
                world.EnableAiNearestFlatShadowForDiagnostics;
            previousForceAiNearestFlatQuery =
                world.ForceAiNearestFlatQueryForDiagnostics;
            previousForceLegacyLateRuntimeSnapshots =
                world.ForceLegacyLateRuntimeSnapshotsForDiagnostics;
            sceneQuery.ForceRoleAwareTreeForDiagnostics = forceRoleAwareTree;
            roleAwareTreeApplied = true;
            sceneQuery.ForceRoleAwareDirectForDiagnostics = forceRoleAwareDirect;
            roleAwareDirectApplied = true;
            roleAwareDirectRestored = false;
            sceneQuery.ForceLegacyZeroAttackItrShortCircuitForDiagnostics =
                forceLegacyZeroAttackItrShortCircuit;
            zeroAttackItrShortCircuitApplied = true;
            zeroAttackItrShortCircuitRestored = false;
            world.ForceLegacyPreInteractionForDiagnostics =
                forceLegacyPreInteraction;
            preInteractionApplied = true;
            preInteractionRestored = false;
            world.ForceLegacyCharacterInputPassForDiagnostics =
                forceLegacyCharacterInputPass;
            characterInputPassApplied = true;
            characterInputPassRestored = false;
            world.ForceLegacyAiSnapshotBuildForDiagnostics =
                forceLegacyAiSnapshotBuild;
            aiSnapshotBuildApplied = true;
            aiSnapshotBuildRestored = false;
            world.ForceLegacyAiNearestFilterForDiagnostics =
                forceLegacyAiNearestFilter;
            aiNearestFilterApplied = true;
            aiNearestFilterRestored = false;
            world.EnableAiNearestFlatShadowForDiagnostics =
                enableAiNearestFlatShadow;
            aiNearestFlatShadowApplied = true;
            aiNearestFlatShadowRestored = false;
            world.ForceAiNearestFlatQueryForDiagnostics =
                forceAiNearestFlatQuery;
            aiNearestFlatQueryApplied = true;
            aiNearestFlatQueryRestored = false;
            world.ForceLegacyLateRuntimeSnapshotsForDiagnostics =
                forceLegacyLateRuntimeSnapshots;
            lateRuntimeSnapshotsApplied = true;
            lateRuntimeSnapshotsRestored = false;
        }

        internal void RestoreRoleAwareTree()
        {
            if (!roleAwareTreeApplied)
                return;

            try
            {
                if (sceneQuery != null)
                    sceneQuery.ForceRoleAwareTreeForDiagnostics = previousForceRoleAwareTree;
            }
            finally
            {
                roleAwareTreeApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreZeroAttackItrShortCircuit()
        {
            if (!zeroAttackItrShortCircuitApplied)
                return;

            try
            {
                if (sceneQuery == null)
                    throw new InvalidOperationException(
                        "Zero-attack ITR short-circuit diagnostic query is unavailable during restoration.");

                sceneQuery.ForceLegacyZeroAttackItrShortCircuitForDiagnostics =
                    previousForceLegacyZeroAttackItrShortCircuit;
                zeroAttackItrShortCircuitRestored =
                    sceneQuery.ForceLegacyZeroAttackItrShortCircuitForDiagnostics ==
                    previousForceLegacyZeroAttackItrShortCircuit;
            }
            finally
            {
                zeroAttackItrShortCircuitApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreRoleAwareDirect()
        {
            if (!roleAwareDirectApplied)
                return;

            try
            {
                if (sceneQuery == null)
                    throw new InvalidOperationException(
                        "Role-aware direct diagnostic query is unavailable during restoration.");

                sceneQuery.ForceRoleAwareDirectForDiagnostics =
                    previousForceRoleAwareDirect;
                roleAwareDirectRestored =
                    sceneQuery.ForceRoleAwareDirectForDiagnostics ==
                    previousForceRoleAwareDirect;
            }
            finally
            {
                roleAwareDirectApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestorePreInteraction()
        {
            if (!preInteractionApplied)
                return;

            try
            {
                if (world == null)
                    throw new InvalidOperationException(
                        "PreInteraction diagnostic world is unavailable during restoration.");

                world.ForceLegacyPreInteractionForDiagnostics =
                    previousForceLegacyPreInteraction;
                preInteractionRestored =
                    world.ForceLegacyPreInteractionForDiagnostics ==
                    previousForceLegacyPreInteraction;
            }
            finally
            {
                preInteractionApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreAiSnapshotBuild()
        {
            if (!aiSnapshotBuildApplied)
                return;

            try
            {
                if (world == null)
                    throw new InvalidOperationException(
                        "AI snapshot-build diagnostic world is unavailable during restoration.");

                world.ForceLegacyAiSnapshotBuildForDiagnostics =
                    previousForceLegacyAiSnapshotBuild;
                aiSnapshotBuildRestored =
                    world.ForceLegacyAiSnapshotBuildForDiagnostics ==
                    previousForceLegacyAiSnapshotBuild;
            }
            finally
            {
                aiSnapshotBuildApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreCharacterInputPass()
        {
            if (!characterInputPassApplied)
                return;

            try
            {
                if (world == null)
                    throw new InvalidOperationException(
                        "CharacterInput diagnostic world is unavailable during restoration.");

                world.ForceLegacyCharacterInputPassForDiagnostics =
                    previousForceLegacyCharacterInputPass;
                characterInputPassRestored =
                    world.ForceLegacyCharacterInputPassForDiagnostics ==
                    previousForceLegacyCharacterInputPass;
            }
            finally
            {
                characterInputPassApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreAiNearestFilter()
        {
            if (!aiNearestFilterApplied)
                return;

            try
            {
                if (world == null)
                    throw new InvalidOperationException(
                        "AI nearest-filter diagnostic world is unavailable during restoration.");

                world.ForceLegacyAiNearestFilterForDiagnostics =
                    previousForceLegacyAiNearestFilter;
                aiNearestFilterRestored =
                    world.ForceLegacyAiNearestFilterForDiagnostics ==
                    previousForceLegacyAiNearestFilter;
            }
            finally
            {
                aiNearestFilterApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreAiNearestFlatShadow()
        {
            if (!aiNearestFlatShadowApplied)
                return;

            try
            {
                if (world == null)
                    throw new InvalidOperationException(
                        "AI nearest flat-shadow diagnostic world is unavailable during restoration.");

                world.EnableAiNearestFlatShadowForDiagnostics =
                    previousEnableAiNearestFlatShadow;
                aiNearestFlatShadowRestored =
                    world.EnableAiNearestFlatShadowForDiagnostics ==
                    previousEnableAiNearestFlatShadow;
            }
            finally
            {
                aiNearestFlatShadowApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreAiNearestFlatQuery()
        {
            if (!aiNearestFlatQueryApplied)
                return;

            try
            {
                if (world == null)
                    throw new InvalidOperationException(
                        "AI nearest flat-query diagnostic world is unavailable during restoration.");

                world.ForceAiNearestFlatQueryForDiagnostics =
                    previousForceAiNearestFlatQuery;
                aiNearestFlatQueryRestored =
                    world.ForceAiNearestFlatQueryForDiagnostics ==
                    previousForceAiNearestFlatQuery;
            }
            finally
            {
                aiNearestFlatQueryApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        internal void RestoreLateRuntimeSnapshots()
        {
            if (!lateRuntimeSnapshotsApplied)
                return;

            try
            {
                if (world == null)
                    throw new InvalidOperationException(
                        "Late runtime-snapshot diagnostic world is unavailable during restoration.");

                world.ForceLegacyLateRuntimeSnapshotsForDiagnostics =
                    previousForceLegacyLateRuntimeSnapshots;
                lateRuntimeSnapshotsRestored =
                    world.ForceLegacyLateRuntimeSnapshotsForDiagnostics ==
                    previousForceLegacyLateRuntimeSnapshots;
            }
            finally
            {
                lateRuntimeSnapshotsApplied = false;
                ReleaseWorldWhenFullyRestored();
            }
        }

        private void ReleaseWorldWhenFullyRestored()
        {
            if (!Applied)
            {
                world = null;
                sceneQuery = null;
            }
        }
    }

    public sealed class ProductionEntityStressRunner : MonoBehaviour
    {
        internal const int MaximumRetainedSamples = 4096;
        private const int MaximumCleanupPasses = 8;

        private readonly List<LF2Character> entities = new List<LF2Character>(1000);
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(1050);
        private readonly HashSet<RuntimeEntityHandle> harnessOwnedHandles =
            new HashSet<RuntimeEntityHandle>();
        private readonly HashSet<RuntimeEntityHandle> observedDerivedHandles =
            new HashSet<RuntimeEntityHandle>();
        private readonly ProductionEntityStressLogicTickTimingCollector logicTickTimingCollector =
            new ProductionEntityStressLogicTickTimingCollector();
        private readonly List<double> frameSamples = new List<double>(512);
        private readonly List<double> allocationSamples = new List<double>(512);
        private readonly ProductionEntityStressLoggingPolicy loggingPolicy =
            new ProductionEntityStressLoggingPolicy();
        private readonly ProductionEntityStressAbDiagnosticsLifecycle abDiagnosticsLifecycle =
            new ProductionEntityStressAbDiagnosticsLifecycle();
        private ProductionEntityStressPhaseTimingCollector phaseTimingCollector;
        private ProductionEntityStressPresentationTimingCollector presentationTimingCollector;
        private readonly ProductionEntityStressDetailPhaseTimingCollector detailPhaseTimingCollector =
            new ProductionEntityStressDetailPhaseTimingCollector();

        private ProductionEntityStressConfig config;
        private ProductionEntityStressReport report;
        private SimulationTickDriver driver;
        private SimulationWorld world;
        private LF2ObjectPool objectPool;
        private LF2ReferencePool referencePool;
        private LF2ObjectPointFactory objectPointFactory;
        private BattleTickPhaseDiagnostics phaseTimingDiagnostics;
        private BattlePresentationPhaseDiagnostics presentationTimingDiagnostics;
        private BattleTickDetailPhaseDiagnostics detailPhaseTimingDiagnostics;
        private BattleAiInputDetailDiagnostics aiInputDetailDiagnostics;
        private LockstepSimulationSettings previousSettings;
        private bool previousPaused;
        private int objectPoolActiveBaseline;
        private int objectPoolAvailableBaseline;
        private int referencePoolActiveBaseline;
        private int selectedCharacterOid;
        private int frameCounter;
        private float accumulator;
        private bool configured;
        private bool cleaned;
        private bool cleanupInProgress;
        private bool driverConfigurationChanged;

        public static ProductionEntityStressRunner Active { get; private set; }
        public ProductionEntityStressReport Report => report;

        internal static ProductionEntityStressRunner StartRun(ProductionEntityStressConfig runConfig)
        {
            if (!Application.isPlaying)
                throw new InvalidOperationException("Production entity stress requires Play Mode.");
            if (Active != null)
                throw new InvalidOperationException("A production entity stress run is already active.");

            var root = new GameObject($"NTSD Production Entity Stress [{runConfig.Mode}]");
            var runner = root.AddComponent<ProductionEntityStressRunner>();
            Active = runner;
            try
            {
                runner.Configure(runConfig);
                return runner;
            }
            catch
            {
                runner.CleanupInternal("start-failed", true);
                UnityEngine.Object.Destroy(root);
                throw;
            }
        }

        internal static bool AreProductionServicesReady()
        {
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            return SimulationTickDriver.Instance?.World != null &&
                   LF2ObjectPool.Instance != null &&
                   LF2ReferencePool.Instance != null &&
                   LF2ObjectPointFactory.Instance != null &&
                   manager != null &&
                   dataManager != null &&
                   dataManager.IsLoaded() &&
                   TrySelectLoadedCharacter(manager, dataManager, out _);
        }

        internal static Vector3 BuildSpawnPosition(
            ProductionEntityStressMode mode,
            int index,
            int entityCount)
        {
            if (mode == ProductionEntityStressMode.Concentrated1000)
            {
                int column = index % 20;
                int row = (index / 20) % 10;
                return new Vector3(385f + column * 1.5f, 0f, 242f + row * 1.5f);
            }

            int columns = mode == ProductionEntityStressMode.Smoke50 ? 10 : 40;
            int rows = Math.Max(1, (entityCount + columns - 1) / columns);
            int xIndex = index % columns;
            int zIndex = index / columns;
            float x = 20f + xIndex * (760f / Math.Max(1, columns - 1));
            float z = 185f + zIndex * (160f / Math.Max(1, rows - 1));
            return new Vector3(x, 0f, z);
        }

        private void Configure(ProductionEntityStressConfig runConfig)
        {
            config = runConfig;
            report = new ProductionEntityStressReport
            {
                status = "Starting",
                mode = config.Mode.ToString(),
                inputMode = ProductionEntityStressConfig.FormatInputMode(config.InputMode),
                startedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                updatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                scene = gameObject.scene.name,
                stressRootName = gameObject.name,
                outputPath = config.OutputPath,
                requestedEntityCount = config.EntityCount,
                opointCounterAvailable = true,
                opointCounterReason =
                    "Runtime-derived observable proxy: unique active non-harness runtime handles observed " +
                    "after each logic tick. It is not a production opoint creation counter.",
                phaseTimingEnabled = config.EnablePhaseTiming,
                phaseTimingSource = config.EnablePhaseTiming
                    ? ProductionEntityStressPhaseTimingCollector.Source
                    : string.Empty,
                presentationTimingEnabled = config.EnablePresentationTiming,
                presentationTimingSource = config.EnablePresentationTiming
                    ? ProductionEntityStressPresentationTimingCollector.Source
                    : string.Empty,
                presentationTimingUnavailableReason = config.EnablePresentationTiming
                    ? string.Empty
                    : "Disabled by request; set enablePresentationTiming to true to collect " +
                      "coarse presentation publication timings.",
                detailPhaseTimingEnabled = config.EnableDetailPhaseTiming,
                detailPhaseTimingSource = config.EnableDetailPhaseTiming
                    ? ProductionEntityStressDetailPhaseTimingCollector.Source
                    : string.Empty,
                detailPhaseTimingUnavailableReason = config.EnableDetailPhaseTiming
                    ? string.Empty
                    : "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.",
            };
            loggingPolicy.Apply(report.loggingPolicy);

            driver = SimulationTickDriver.Instance;
            objectPool = LF2ObjectPool.Instance;
            referencePool = LF2ReferencePool.Instance;
            objectPointFactory = LF2ObjectPointFactory.Instance;
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            if (driver == null || objectPool == null || referencePool == null ||
                objectPointFactory == null || manager == null || dataManager == null)
            {
                throw new InvalidOperationException(
                    "Required production services are missing. Load a battle scene before starting the stress harness.");
            }
            if (driver.World == null || driver.World.ObjectCount != 0 ||
                driver.World.ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new InvalidOperationException(
                    "The production stress harness requires an empty SimulationWorld.");
            }
            if (objectPool.ActiveObjectCountForAcceptance != 0 || referencePool.ActiveCount != 0)
            {
                throw new InvalidOperationException(
                    "The production stress harness requires both production pools to have zero active objects.");
            }
            if (!TrySelectLoadedCharacter(manager, dataManager, out selectedCharacterOid))
            {
                throw new InvalidOperationException(
                    "No loaded type-0 character with DAT frames and visible sprites is available.");
            }

            previousSettings = CloneSettings(driver.Settings);
            previousPaused = driver.IsPaused;
            objectPoolActiveBaseline = objectPool.ActiveObjectCountForAcceptance;
            objectPoolAvailableBaseline = objectPool.AvailableObjectCountForAcceptance;
            referencePoolActiveBaseline = referencePool.ActiveCount;

            var settings = new BattleRuntimeWorldSettings(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities,
                CollisionBroadphaseBackend.LooseQuadtree);
            driverConfigurationChanged = true;
            if (!driver.TryConfigureEmptyDiagnosticWorld(settings, out string failureReason))
                throw new InvalidOperationException(failureReason);

            world = driver.World;
            if (world.RuntimeProfileForDiagnostics != BattleRuntimeProfile.MobileExtended ||
                world.RuntimeSlotCapacityForDiagnostics != BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity ||
                world.CollisionBroadphaseForDiagnostics != CollisionBroadphaseBackend.LooseQuadtree)
            {
                throw new InvalidOperationException(
                    "Diagnostic world did not apply MobileExtended(1050) + LooseQuadtree before registration.");
            }
            BruteForceSceneQuery stressSceneQuery =
                ApplyFormalCollectorModeForDiagnostics(world, config.FormalCollectorMode);
            report.formalCollectorRequestedMode =
                ProductionEntityStressConfig.FormatFormalCollectorMode(config.FormalCollectorMode);
            report.formalCollectorMode =
                ProductionEntityStressConfig.FormatFormalCollectorMode(
                    ResolveAppliedFormalCollectorModeForDiagnostics(
                        world,
                        stressSceneQuery));
            report.forceRoleAwareTreeForDiagnosticsRequested =
                config.ForceRoleAwareTreeForDiagnostics;
            abDiagnosticsLifecycle.Apply(
                world,
                config.ForceRoleAwareTreeForDiagnostics,
                config.ForceRoleAwareDirectForDiagnostics,
                config.ForceLegacyZeroAttackItrShortCircuitForDiagnostics,
                config.ForceLegacyPreInteractionForDiagnostics,
                config.ForceLegacyCharacterInputPassForDiagnostics,
                config.ForceLegacyAiSnapshotBuildForDiagnostics,
                config.ForceLegacyAiNearestFilterForDiagnostics,
                config.EnableAiNearestFlatShadowForDiagnostics,
                config.ForceAiNearestFlatQueryForDiagnostics,
                config.ForceLegacyLateRuntimeSnapshotsForDiagnostics);
            report.forceRoleAwareTreeForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceRoleAwareTree;
            report.forceRoleAwareDirectForDiagnosticsRequested =
                config.ForceRoleAwareDirectForDiagnostics;
            report.forceRoleAwareDirectForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceRoleAwareDirect;
            report.forceLegacyZeroAttackItrShortCircuitForDiagnosticsRequested =
                config.ForceLegacyZeroAttackItrShortCircuitForDiagnostics;
            report.forceLegacyZeroAttackItrShortCircuitForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceLegacyZeroAttackItrShortCircuit;
            report.forceLegacyPreInteractionForDiagnosticsRequested =
                config.ForceLegacyPreInteractionForDiagnostics;
            report.forceLegacyPreInteractionForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceLegacyPreInteraction;
            report.forceLegacyCharacterInputPassForDiagnosticsRequested =
                config.ForceLegacyCharacterInputPassForDiagnostics;
            report.forceLegacyCharacterInputPassForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceLegacyCharacterInputPass;
            report.forceLegacyAiSnapshotBuildForDiagnosticsRequested =
                config.ForceLegacyAiSnapshotBuildForDiagnostics;
            report.forceLegacyAiSnapshotBuildForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceLegacyAiSnapshotBuild;
            report.forceLegacyAiNearestFilterForDiagnosticsRequested =
                config.ForceLegacyAiNearestFilterForDiagnostics;
            report.forceLegacyAiNearestFilterForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceLegacyAiNearestFilter;
            report.enableAiNearestFlatShadowForDiagnosticsRequested =
                config.EnableAiNearestFlatShadowForDiagnostics;
            report.enableAiNearestFlatShadowForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveEnableAiNearestFlatShadow;
            report.forceAiNearestFlatQueryForDiagnosticsRequested =
                config.ForceAiNearestFlatQueryForDiagnostics;
            report.forceAiNearestFlatQueryForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceAiNearestFlatQuery;
            report.forceLegacyLateRuntimeSnapshotsForDiagnosticsRequested =
                config.ForceLegacyLateRuntimeSnapshotsForDiagnostics;
            report.forceLegacyLateRuntimeSnapshotsForDiagnosticsEffective =
                abDiagnosticsLifecycle.EffectiveForceLegacyLateRuntimeSnapshots;
            report.formalCollectorBodyEntries =
                stressSceneQuery.LastRoleAwareBodyEntryCountForDiagnostics;
            report.formalCollectorItrQueries =
                stressSceneQuery.LastRoleAwareItrQueryCountForDiagnostics;
            ProductionEntityStressPhaseTimingLifecycle.Disable(world);
            phaseTimingDiagnostics = null;
            presentationTimingDiagnostics = null;
            detailPhaseTimingDiagnostics = null;
            aiInputDetailDiagnostics = null;
            phaseTimingCollector = null;
            presentationTimingCollector = null;
            if (config.EnablePhaseTiming)
            {
                phaseTimingDiagnostics = world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
                phaseTimingCollector = new ProductionEntityStressPhaseTimingCollector();
            }
            if (config.EnablePresentationTiming)
            {
                presentationTimingDiagnostics =
                    world.EnableBattlePresentationPhaseDiagnosticsForDiagnostics();
                presentationTimingCollector =
                    new ProductionEntityStressPresentationTimingCollector();
            }
            if (config.EnableDetailPhaseTiming)
            {
                detailPhaseTimingDiagnostics =
                    world.EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
                aiInputDetailDiagnostics =
                    world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            }
            if (!config.EnablePhaseTiming)
                ProductionEntityStressPhaseTimingCollector.PopulateDisabledReport(report);
            if (!config.EnablePresentationTiming)
            {
                ProductionEntityStressPresentationTimingCollector
                    .PopulateDisabledReport(report);
            }

            driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                useUnscaledTime = true,
                maxCatchUpTicksPerFrame = config.MaxCatchUpTicksPerFrame,
                maxBacklogTicks = config.MaxBacklogTicks,
                inputDelayTicks = 0,
                requireInputFrameReady = false,
                enableFrameChecksum = false,
            });
            driver.SetPaused(true);
            configured = true;
            report.selectedCharacterOid = selectedCharacterOid;
            RefreshReportCounts();
            WriteReport();
        }

        private void Update()
        {
            if (!configured || cleaned)
                return;

            try
            {
                double frameMs = Time.unscaledDeltaTime * 1000d;
                if (entities.Count < config.EntityCount)
                {
                    SpawnBatch(config.SpawnBatchSize);
                    if (entities.Count == config.EntityCount)
                    {
                        accumulator = 0f;
                        report.status = "Running";
                        ValidatePeakPopulation();
                        WriteReport();
                    }
                    return;
                }

                RemoveReleasedEntities();
                if (entities.Count < config.EntityCount)
                {
                    SpawnBatch(Math.Min(config.SpawnBatchSize, config.EntityCount - entities.Count));
                    return;
                }

                AddRollingSample(frameSamples, frameMs);
                report.sampledUnityFrames++;
                accumulator += Time.unscaledDeltaTime;
                float maximumAccumulator = SimulationConstants.SIM_DT * config.MaxBacklogTicks;
                if (accumulator > maximumAccumulator)
                {
                    report.droppedBacklogTicks += Mathf.FloorToInt(
                        (accumulator - maximumAccumulator) / SimulationConstants.SIM_DT);
                    accumulator = maximumAccumulator;
                }

                int ticksThisFrame = 0;
                while (accumulator >= SimulationConstants.SIM_DT &&
                       ticksThisFrame < config.MaxCatchUpTicksPerFrame)
                {
                    accumulator -= SimulationConstants.SIM_DT;
                    bool buildPresentation = SimulationTickDriver.IsFinalCatchUpTick(
                        accumulator,
                        ticksThisFrame,
                        config.MaxCatchUpTicksPerFrame);
                    StepMeasuredTick(buildPresentation);
                    ticksThisFrame++;
                }

                if (ticksThisFrame > 1)
                    report.framesWithCatchUp++;
                report.maximumCatchUpTicksInFrame = Math.Max(
                    report.maximumCatchUpTicksInFrame,
                    ticksThisFrame);
                report.currentBacklogTicks = Mathf.FloorToInt(
                    accumulator / SimulationConstants.SIM_DT);
                report.maximumBacklogTicks = Math.Max(
                    report.maximumBacklogTicks,
                    report.currentBacklogTicks);

                frameCounter++;
                if (frameCounter % 30 == 0)
                    WriteReport();

                if (config.AutoCleanup && report.sampledLogicTicks >= config.SampleTicks)
                    StopAndCleanup("smoke-complete");
            }
            catch (Exception exception)
            {
                report.status = "Failed";
                report.failure = exception.ToString();
                CleanupInternal("exception", true);
                ProductionEntityStressPaths.WriteTerminalResult(false, config.OutputPath, exception.Message);
                Debug.LogError($"[ProductionEntityStress] Failed: {exception}");
            }
        }

        internal void StopAndCleanup(string reason)
        {
            if (cleaned)
                return;
            bool smokeCompleted = config.AutoCleanup &&
                                  report.sampledLogicTicks >= config.SampleTicks &&
                                  string.IsNullOrEmpty(report.failure);
            CleanupInternal(reason, true);
            bool success = report.harnessValidity && report.teardown.restored &&
                           (!config.AutoCleanup || smokeCompleted);
            report.status = config.AutoCleanup
                ? (success ? "SmokePassed" : "SmokeFailed")
                : (success ? "StoppedCleanly" : "StoppedWithResidue");
            WriteReport();
            ProductionEntityStressPaths.WriteTerminalResult(
                success,
                config.OutputPath,
                report.teardown.evidence);
            Debug.Log($"[ProductionEntityStress] {report.status}: {config.OutputPath}");
            Destroy(gameObject);
        }

        private void StepMeasuredTick(bool buildPresentation = true)
        {
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            long timestamp = Stopwatch.GetTimestamp();
            bool stepped = driver.StepOneTick(
                ignorePaused: true,
                buildPresentation: buildPresentation);
            double elapsedMs = (Stopwatch.GetTimestamp() - timestamp) * 1000d / Stopwatch.Frequency;
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            if (!stepped)
                throw new InvalidOperationException("Production SimulationTickDriver refused a manual tick.");

            report.logicTicksExecuted++;
            phaseTimingCollector?.CaptureAfterTick(
                phaseTimingDiagnostics,
                elapsedMs,
                report.logicTicksExecuted,
                config.WarmupTicks);
            presentationTimingCollector?.CaptureAfterTick(
                presentationTimingDiagnostics,
                report.logicTicksExecuted,
                config.WarmupTicks);
            if (config.EnableDetailPhaseTiming)
            {
                detailPhaseTimingCollector.CaptureAfterTick(
                    detailPhaseTimingDiagnostics,
                    aiInputDetailDiagnostics,
                    report.logicTicksExecuted,
                    config.WarmupTicks);
            }
            if (report.logicTicksExecuted <= config.WarmupTicks)
            {
                report.warmupTicksCompleted = report.logicTicksExecuted;
            }
            else
            {
                logicTickTimingCollector.AddSample(elapsedMs, buildPresentation);
                AddRollingSample(allocationSamples, allocatedBytes);
                report.sampledLogicTicks++;
                CaptureLateOpointCountersForReport(report, world);
            }

            CaptureProductionCounters();
        }

        private void CaptureProductionCounters()
        {
            long candidateCount = 0;
            int aiCount = 0;
            for (int i = 0; i < entities.Count; i++)
            {
                LF2Character entity = entities[i];
                if (!IsActive(entity))
                    continue;
                candidateCount += Math.Max(0, entity.Runtime.HitCandidateCount);
                if (entity.AiControlled)
                    aiCount++;
            }
            report.aiControlledEntityTicks += aiCount;
            report.collisionCandidateCountSum += candidateCount;
            report.collisionCandidateCountPeak = Math.Max(
                report.collisionCandidateCountPeak,
                candidateCount > int.MaxValue ? int.MaxValue : (int)candidateCount);

            if (world.SceneQuery is BruteForceSceneQuery sceneQuery)
            {
                report.formalCollectorMode =
                    ProductionEntityStressConfig.FormatFormalCollectorMode(
                        sceneQuery.LastFormalCollectorModeForDiagnostics);
                report.formalCollectorBodyEntries =
                    sceneQuery.LastRoleAwareBodyEntryCountForDiagnostics;
                report.formalCollectorItrQueries =
                    sceneQuery.LastRoleAwareItrQueryCountForDiagnostics;
                int pairCount = sceneQuery.LastFormalPairCountForDiagnostics;
                report.broadphasePairCountSum += pairCount;
                report.broadphasePairCountPeak = Math.Max(report.broadphasePairCountPeak, pairCount);
                report.broadphaseFallbackParticipantPeak = Math.Max(
                    report.broadphaseFallbackParticipantPeak,
                    sceneQuery.LastFormalFallbackParticipantCountForDiagnostics);
                if (sceneQuery.LastFormalCollectionAbortedForDiagnostics)
                    report.broadphaseAbortedTicks++;
                SpatialSynchronizeResult sync = sceneQuery.LastFormalSynchronizeResultForDiagnostics;
                report.broadphaseLastIndexedCount = sync.IndexedCount;
            }

            report.damageStatTotal = Sum(world.DamageStats);
            report.killStatTotal = Sum(world.KillStats);
            ObserveRuntimeEntitySnapshot();
        }

        internal static void CaptureLateOpointCountersForReport(
            ProductionEntityStressReport targetReport,
            SimulationWorld targetWorld)
        {
            if (targetReport == null || targetWorld == null)
                return;

            ProductionEntityStressLateOpointCounters counters =
                targetReport.lateOpointCounters;
            if (counters == null)
            {
                counters = new ProductionEntityStressLateOpointCounters();
                targetReport.lateOpointCounters = counters;
            }

            counters.sampledLatePasses++;
            Accumulate(
                ref counters.opointNoRoleSum,
                ref counters.opointNoRolePeak,
                targetWorld.LastLateOpointNoRoleCountForDiagnostics);
            Accumulate(
                ref counters.invalidFirstSum,
                ref counters.invalidFirstPeak,
                targetWorld.LastLateOpointInvalidFirstCountForDiagnostics);
            Accumulate(
                ref counters.attackingRejectedSum,
                ref counters.attackingRejectedPeak,
                targetWorld.LastLateOpointAttackingRejectedCountForDiagnostics);
            Accumulate(
                ref counters.characterFrameDelayRejectedSum,
                ref counters.characterFrameDelayRejectedPeak,
                targetWorld.LastLateOpointCharacterFrameDelayRejectedCountForDiagnostics);
            Accumulate(
                ref counters.eligibleSum,
                ref counters.eligiblePeak,
                targetWorld.LastLateOpointEligibleCountForDiagnostics);
            Accumulate(
                ref counters.factoryInvokedSum,
                ref counters.factoryInvokedPeak,
                targetWorld.LastLateOpointFactoryInvokedCountForDiagnostics);
        }

        private static void Accumulate(ref long sum, ref int peak, int value)
        {
            sum += value;
            peak = Math.Max(peak, value);
        }

        internal static BruteForceSceneQuery ApplyFormalCollectorModeForDiagnostics(
            SimulationWorld targetWorld,
            CollisionFormalCollectorMode mode)
        {
            if (targetWorld?.SceneQuery is not BruteForceSceneQuery sceneQuery)
            {
                throw new InvalidOperationException(
                    "Production stress requires BruteForceSceneQuery diagnostics.");
            }

            sceneQuery.FormalCollectorMode = mode;
            return sceneQuery;
        }

        internal static CollisionFormalCollectorMode
            ResolveAppliedFormalCollectorModeForDiagnostics(
                SimulationWorld targetWorld,
                BruteForceSceneQuery sceneQuery)
        {
            if (targetWorld == null)
                throw new ArgumentNullException(nameof(targetWorld));
            if (sceneQuery == null)
                throw new ArgumentNullException(nameof(sceneQuery));
            if (!ReferenceEquals(targetWorld.SceneQuery, sceneQuery))
            {
                throw new ArgumentException(
                    "The scene query does not belong to the supplied stress world.",
                    nameof(sceneQuery));
            }

            if (sceneQuery.FormalCollectorMode != CollisionFormalCollectorMode.Configured)
                return sceneQuery.FormalCollectorMode;

            return targetWorld.CollisionBroadphaseForDiagnostics ==
                   CollisionBroadphaseBackend.LooseQuadtree
                ? CollisionFormalCollectorMode.ForceRoleAware
                : CollisionFormalCollectorMode.ForceBruteForce;
        }

        private void SpawnBatch(int count)
        {
            int remaining = config.EntityCount - entities.Count;
            int spawnCount = Math.Min(Math.Max(0, count), remaining);
            for (int i = 0; i < spawnCount; i++)
            {
                int placementIndex = entities.Count;
                LF2Character entity = SpawnCharacter(placementIndex);
                if (entity == null)
                    throw new InvalidOperationException(
                        $"Production creation chain failed at entity {placementIndex}/{config.EntityCount}.");
                entities.Add(entity);
                report.totalEntitiesCreated++;
            }
            RefreshReportCounts();
        }

        private LF2Character SpawnCharacter(int placementIndex)
        {
            Vector3 position = BuildSpawnPosition(config.Mode, placementIndex, config.EntityCount);
            int team = (placementIndex & 1) + 1;
            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                kind = 1,
                oid = selectedCharacterOid,
                action = 0,
                facing = placementIndex & 1,
            };
            task.team = team;
            task.relationTeam = team;
            task.useExplicitRelationIdentity = true;
            task.dir = (placementIndex & 1) == 0 ? "right" : "left";
            task.useDirectRuntimePosition = true;
            task.directX = position.x;
            task.directY = position.y;
            task.directZ = position.z;
            task.useDirectVelocity = true;
            task.directVx = 0d;
            task.directVy = 0d;
            task.directVz = 0d;
            task.preserveActionZero = true;
            task.skipPostInitZOffset = true;

            LF2Entity spawned;
            try
            {
                spawned = objectPointFactory.CreateObjectImmediate(task);
            }
            finally
            {
                referencePool.Recycle(task);
            }

            if (spawned is not LF2Character character)
            {
                spawned?.OnTransitDestroy();
                return null;
            }
            if (!IsActive(character))
            {
                character.OnTransitDestroy();
                return null;
            }
            GameObject entityRoot = character.Renderer?.transform.parent?.gameObject;
            if (entityRoot == null)
            {
                character.OnTransitDestroy();
                return null;
            }
            entityRoot.name = string.Format(
                CultureInfo.InvariantCulture,
                "StressEntity_{0:D4}_Slot_{1:D4}_Team_{2}",
                report.totalEntitiesCreated,
                character.Runtime.SlotIndex,
                team);
            entityRoot.transform.SetParent(transform, true);
            character.AiControlled = config.InputMode == ProductionEntityStressInputMode.Ai;
            character.Renderer.ForceRefreshPresentation();
            if (world.TryGetCurrentRuntimeHandleForDiagnostics(
                    character.Runtime.SlotIndex,
                    character,
                    out RuntimeEntityHandle handle))
            {
                harnessOwnedHandles.Add(handle);
            }
            return character;
        }

        private void RemoveReleasedEntities()
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (IsActive(entities[i]))
                    continue;
                entities.RemoveAt(i);
                report.lifecycleReplacements++;
            }
        }

        private void ValidatePeakPopulation()
        {
            RefreshReportCounts();
            report.harnessValidity =
                ProductionEntityStressPopulationPolicy.Evaluate(
                    config.EntityCount,
                    report.activeGameObjectCount,
                    report.stressRootChildCount,
                    report.worldObjectCount,
                    report.worldEntityCount,
                    report.claimedRuntimeSlotCount) &&
                world.RuntimeProfileForDiagnostics == BattleRuntimeProfile.MobileExtended &&
                world.RuntimeSlotCapacityForDiagnostics == BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity &&
                world.CollisionBroadphaseForDiagnostics == CollisionBroadphaseBackend.LooseQuadtree;
            if (!report.harnessValidity)
            {
                throw new InvalidOperationException(
                    "Peak population validation failed; see structured count fields in the report.");
            }
        }

        private void CleanupInternal(string reason, bool restoreDriver)
        {
            if (cleaned || cleanupInProgress)
                return;

            cleanupInProgress = true;
            report ??= new ProductionEntityStressReport();
            var journal = new ProductionEntityStressCleanupJournal();
            try
            {
                ProductionEntityStressPhaseTimingLifecycle.Disable(world);
                report.teardown.attempted = true;
                journal.Attempt("capture-before-state", () =>
                {
                    RefreshReportCounts();
                    report.teardown.activeGameObjectsBefore = report.activeGameObjectCount;
                    report.teardown.worldObjectsBefore = world?.ObjectCount ?? 0;
                    report.teardown.worldEntitiesBefore = CountWorldEntities();
                    report.teardown.claimedSlotsBefore =
                        world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
                    report.teardown.objectPoolActiveBeforeRun = objectPoolActiveBaseline;
                    report.teardown.objectPoolAvailableBeforeRun = objectPoolAvailableBaseline;
                    report.teardown.referencePoolActiveBeforeRun = referencePoolActiveBaseline;
                });

                CleanupActiveRuntimeEntities(journal);
                journal.Attempt("clear-entity-tracking", entities.Clear);
                journal.Attempt(
                    "restore-role-aware-tree-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreRoleAwareTree);
                journal.Attempt(
                    "restore-role-aware-direct-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreRoleAwareDirect);
                journal.Attempt(
                    "restore-zero-attack-itr-short-circuit-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreZeroAttackItrShortCircuit);
                journal.Attempt(
                    "restore-preinteraction-diagnostic-switch",
                    abDiagnosticsLifecycle.RestorePreInteraction);
                journal.Attempt(
                    "restore-character-input-pass-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreCharacterInputPass);
                journal.Attempt(
                    "restore-ai-snapshot-build-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreAiSnapshotBuild);
                journal.Attempt(
                    "restore-ai-nearest-filter-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreAiNearestFilter);
                journal.Attempt(
                    "restore-ai-nearest-flat-shadow-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreAiNearestFlatShadow);
                journal.Attempt(
                    "restore-ai-nearest-flat-query-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreAiNearestFlatQuery);
                journal.Attempt(
                    "restore-late-runtime-snapshots-diagnostic-switch",
                    abDiagnosticsLifecycle.RestoreLateRuntimeSnapshots);
                report.teardown.forceRoleAwareDirectForDiagnosticsRestored =
                    abDiagnosticsLifecycle.RoleAwareDirectRestored;
                report.teardown.forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored =
                    abDiagnosticsLifecycle.ZeroAttackItrShortCircuitRestored;
                report.teardown.forceLegacyPreInteractionForDiagnosticsRestored =
                    abDiagnosticsLifecycle.PreInteractionRestored;
                report.teardown.forceLegacyCharacterInputPassForDiagnosticsRestored =
                    abDiagnosticsLifecycle.CharacterInputPassRestored;
                report.teardown.forceLegacyAiSnapshotBuildForDiagnosticsRestored =
                    abDiagnosticsLifecycle.AiSnapshotBuildRestored;
                report.teardown.forceLegacyAiNearestFilterForDiagnosticsRestored =
                    abDiagnosticsLifecycle.AiNearestFilterRestored;
                report.teardown.enableAiNearestFlatShadowForDiagnosticsRestored =
                    abDiagnosticsLifecycle.AiNearestFlatShadowRestored;
                report.teardown.forceAiNearestFlatQueryForDiagnosticsRestored =
                    abDiagnosticsLifecycle.AiNearestFlatQueryRestored;
                report.teardown.forceLegacyLateRuntimeSnapshotsForDiagnosticsRestored =
                    abDiagnosticsLifecycle.LateRuntimeSnapshotsRestored;

                bool afterStateCaptured = journal.Attempt("capture-after-state", () =>
                {
                    report.teardown.activeGameObjectsAfter = CountActiveEntityGameObjects();
                    report.teardown.worldObjectsAfter = world?.ObjectCount ?? 0;
                    report.teardown.worldEntitiesAfter = CountWorldEntities();
                    report.teardown.claimedSlotsAfter =
                        world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
                    report.teardown.objectPoolActiveAfter =
                        objectPool?.ActiveObjectCountForAcceptance ?? -1;
                    report.teardown.objectPoolAvailableAfter =
                        objectPool?.AvailableObjectCountForAcceptance ?? -1;
                    report.teardown.referencePoolActiveAfter = referencePool?.ActiveCount ?? -1;
                });
                report.teardown.retainedInactiveObjectPoolCapacityBeforeRun =
                    report.teardown.objectPoolAvailableBeforeRun;
                report.teardown.retainedInactiveObjectPoolCapacityAfter =
                    report.teardown.objectPoolAvailableAfter;
                report.teardown.retainedInactiveObjectPoolCapacityDelta =
                    report.teardown.retainedInactiveObjectPoolCapacityAfter -
                    report.teardown.retainedInactiveObjectPoolCapacityBeforeRun;
                report.teardown.retainedInactiveObjectPoolCapacityPolicy =
                    "Informational inactive cache capacity only; it is not active cleanup residue " +
                    "and the stress harness does not trim it.";
                report.teardown.activeStateRestored = afterStateCaptured &&
                    ProductionEntityStressTeardownPolicy.IsRestored(
                        report.teardown.activeGameObjectsAfter,
                        report.teardown.worldObjectsAfter,
                        report.teardown.worldEntitiesAfter,
                        report.teardown.claimedSlotsAfter,
                        report.teardown.objectPoolActiveAfter,
                        report.teardown.objectPoolAvailableAfter,
                        report.teardown.referencePoolActiveAfter,
                        objectPoolActiveBaseline,
                        objectPoolAvailableBaseline,
                        referencePoolActiveBaseline);

                report.teardown.driverStateRestored = !driverConfigurationChanged;
                if (driverConfigurationChanged && restoreDriver && driver != null)
                {
                    bool recreated = journal.Attempt("restore-driver-world", () =>
                    {
                        driver.RecreateWorld();
                        world = driver.World;
                    });
                    bool settingsApplied = previousSettings == null ||
                                           journal.Attempt(
                                               "restore-driver-settings",
                                               () => driver.ApplySettings(previousSettings));
                    bool pauseRestored = journal.Attempt(
                        "restore-driver-pause",
                        () => driver.SetPaused(previousPaused));
                    bool stateVerified = journal.Attempt("verify-driver-state", () =>
                    {
                        if (driver.World == null || driver.IsPaused != previousPaused ||
                            !SettingsMatch(driver.Settings, previousSettings))
                        {
                            throw new InvalidOperationException(
                                "SimulationTickDriver did not match its captured pre-run state.");
                        }
                    });
                    report.teardown.driverStateRestored =
                        recreated && settingsApplied && pauseRestored && stateVerified;
                }
                else if (driverConfigurationChanged)
                {
                    journal.Attempt("restore-driver-state", () =>
                    {
                        throw new InvalidOperationException(
                            restoreDriver
                                ? "SimulationTickDriver was unavailable during cleanup."
                                : "SimulationTickDriver restoration was not permitted in the current lifecycle state.");
                    });
                }

                journal.Attempt("restore-logger", () => loggingPolicy.Restore(report.loggingPolicy));
                report.teardown.loggingStateRestored = report.loggingPolicy.restored;
                report.teardown.cleanupExceptionCount = journal.FailureCount;
                report.teardown.cleanupExceptions = journal.FormatFailures();
                report.teardown.restored = report.teardown.activeStateRestored &&
                                           report.teardown.driverStateRestored &&
                                           report.teardown.loggingStateRestored &&
                                           report.teardown.forceRoleAwareDirectForDiagnosticsRestored &&
                                           report.teardown.forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored &&
                                           report.teardown.forceLegacyPreInteractionForDiagnosticsRestored &&
                                           report.teardown.forceLegacyCharacterInputPassForDiagnosticsRestored &&
                                           report.teardown.forceLegacyAiSnapshotBuildForDiagnosticsRestored &&
                                           report.teardown.forceLegacyAiNearestFilterForDiagnosticsRestored &&
                                           report.teardown.enableAiNearestFlatShadowForDiagnosticsRestored &&
                                           report.teardown.forceAiNearestFlatQueryForDiagnosticsRestored &&
                                           report.teardown.forceLegacyLateRuntimeSnapshotsForDiagnosticsRestored &&
                                           report.teardown.cleanupExceptionCount == 0;
                report.teardown.evidence =
                    ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                journal.Attempt("refresh-report-metrics", RefreshReportMetrics);

                if (journal.FailureCount != report.teardown.cleanupExceptionCount)
                {
                    report.teardown.cleanupExceptionCount = journal.FailureCount;
                    report.teardown.cleanupExceptions = journal.FormatFailures();
                    report.teardown.restored = false;
                    report.teardown.evidence =
                        ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                }

                report.status = ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    report.status,
                    reason,
                    report.teardown.restored);
                journal.Attempt("write-report", WriteReport);
                if (journal.FailureCount != report.teardown.cleanupExceptionCount)
                {
                    report.teardown.cleanupExceptionCount = journal.FailureCount;
                    report.teardown.cleanupExceptions = journal.FormatFailures();
                    report.teardown.restored = false;
                    report.teardown.evidence =
                        ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                }
            }
            finally
            {
                ProductionEntityStressPhaseTimingLifecycle.Disable(world);
                cleaned = true;
                cleanupInProgress = false;
                if (ReferenceEquals(Active, this))
                    Active = null;
            }
        }


        private void OnDestroy()
        {
            if (!cleaned)
                CleanupInternal("runner-destroyed", restoreDriver: Application.isPlaying);
            if (ReferenceEquals(Active, this))
                Active = null;
        }

        private void RefreshReportCounts()
        {
            if (report == null)
                return;
            report.activeGameObjectCount = CountActiveEntityGameObjects();
            report.stressRootChildCount = transform != null ? transform.childCount : 0;
            report.worldObjectCount = world?.ObjectCount ?? 0;
            report.worldEntityCount = CountWorldEntities();
            report.claimedRuntimeSlotCount = world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
            if (world != null)
            {
                report.aiNearestFlatShadowQueryCount = Math.Max(
                    report.aiNearestFlatShadowQueryCount,
                    world.AiNearestFlatShadowQueryCountForDiagnostics);
                report.aiNearestFlatShadowUnavailableCount = Math.Max(
                    report.aiNearestFlatShadowUnavailableCount,
                    world.AiNearestFlatShadowUnavailableCountForDiagnostics);
                report.aiNearestFlatShadowMismatchCount = Math.Max(
                    report.aiNearestFlatShadowMismatchCount,
                    world.AiNearestFlatShadowMismatchCountForDiagnostics);
                report.aiNearestFlatGroundSequentialFallbackCount = Math.Max(
                    report.aiNearestFlatGroundSequentialFallbackCount,
                    world.AiNearestFlatGroundSequentialFallbackCountForDiagnostics);
                report.aiNearestFlatFormalQueryCount = Math.Max(
                    report.aiNearestFlatFormalQueryCount,
                    world.AiNearestFlatFormalQueryCountForDiagnostics);
                report.aiNearestFlatFormalFallbackCount = Math.Max(
                    report.aiNearestFlatFormalFallbackCount,
                    world.AiNearestFlatFormalFallbackCountForDiagnostics);
                report.lateFrameTickRuntimeSnapshotProofCountPeak = Math.Max(
                    report.lateFrameTickRuntimeSnapshotProofCountPeak,
                    world.LastLateFrameTickRuntimeSnapshotProofCountForDiagnostics);
                report.skippedLateFrameTickRuntimeSnapshotCountPeak = Math.Max(
                    report.skippedLateFrameTickRuntimeSnapshotCountPeak,
                    world.LastSkippedLateFrameTickRuntimeSnapshotCountForDiagnostics);
                report.skippedLateDeathRuntimeSnapshotCountPeak = Math.Max(
                    report.skippedLateDeathRuntimeSnapshotCountPeak,
                    world.LastSkippedLateDeathRuntimeSnapshotCountForDiagnostics);
                report.skippedLateTailRuntimeSnapshotCountPeak = Math.Max(
                    report.skippedLateTailRuntimeSnapshotCountPeak,
                    world.LastSkippedLateTailRuntimeSnapshotCountForDiagnostics);
                if (string.IsNullOrEmpty(report.aiNearestFlatFirstShadowMismatch) &&
                    !string.IsNullOrEmpty(world.AiNearestFlatFirstShadowMismatchForDiagnostics))
                {
                    report.aiNearestFlatFirstShadowMismatch =
                        world.AiNearestFlatFirstShadowMismatchForDiagnostics;
                }
            }
            if (!report.teardown.attempted)
            {
                report.runtimeProfile = world?.RuntimeProfileForDiagnostics.ToString() ?? string.Empty;
                report.runtimeSlotCapacity = world?.RuntimeSlotCapacityForDiagnostics ?? 0;
                report.broadphaseBackend = world?.CollisionBroadphaseForDiagnostics.ToString() ?? string.Empty;
            }
        }

        private void RefreshReportMetrics()
        {
            RefreshReportCounts();
            logicTickTimingCollector.PopulateReport(report);
            report.unityFrameMilliseconds = ProductionEntityStressStatistics.Summarize(
                frameSamples,
                "ms",
                "Time.unscaledDeltaTime for visible Play Mode frames");
            report.logicTickAllocatedBytes = ProductionEntityStressStatistics.Summarize(
                allocationSamples,
                "bytes",
                "GC.GetAllocatedBytesForCurrentThread around production logic tick");
            if (config.EnablePhaseTiming)
                phaseTimingCollector?.PopulateReport(report);
            else
                ProductionEntityStressPhaseTimingCollector.PopulateDisabledReport(report);
            if (config.EnablePresentationTiming)
            {
                presentationTimingCollector?.PopulateReport(report);
            }
            else
            {
                ProductionEntityStressPresentationTimingCollector
                    .PopulateDisabledReport(report);
            }
            detailPhaseTimingCollector.PopulateReport(report);
            report.updatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }

        private void WriteReport()
        {
            if (report == null || string.IsNullOrWhiteSpace(config.OutputPath))
                return;
            RefreshReportMetrics();
            string directory = Path.GetDirectoryName(config.OutputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(
                config.OutputPath,
                JsonUtility.ToJson(report, true),
                new UTF8Encoding(false));
        }

        private int CountActiveEntityGameObjects()
        {
            return ProductionEntityStressTeardownPolicy.CountActiveStressRootGameObjects(transform);
        }

        private int CountWorldEntities()
        {
            if (world == null)
                return 0;
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch);
            return entityScratch.Count;
        }

        private void ObserveRuntimeEntitySnapshot()
        {
            if (world == null)
                return;

            world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch);
            report.peakWorldEntityCount = Math.Max(report.peakWorldEntityCount, entityScratch.Count);
            for (int i = 0; i < entityScratch.Count; i++)
            {
                LF2Entity entity = entityScratch[i];
                if (entity?.Runtime == null ||
                    !world.TryGetCurrentRuntimeHandleForDiagnostics(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    continue;
                }

                if (ProductionEntityStressDerivedObservationPolicy.TryRecord(
                        handle,
                        harnessOwnedHandles,
                        observedDerivedHandles))
                {
                    report.observedOpointCreates++;
                }
            }
        }

        private void CleanupActiveRuntimeEntities(ProductionEntityStressCleanupJournal journal)
        {
            if (world == null)
                return;

            int previousRemaining = -1;
            for (int pass = 0; pass < MaximumCleanupPasses; pass++)
            {
                bool captured = journal.Attempt(
                    $"capture-world-entities-pass-{pass}",
                    () => world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch));
                if (!captured)
                    break;

                int remaining = entityScratch.Count;
                if (remaining == 0 || remaining >= previousRemaining && previousRemaining >= 0)
                    break;

                previousRemaining = remaining;
                for (int i = 0; i < entityScratch.Count; i++)
                {
                    LF2Entity entity = entityScratch[i];
                    if (entity == null)
                        continue;
                    journal.Attempt(
                        $"release-world-entity-pass-{pass}-index-{i}",
                        entity.OnTransitDestroy);
                }

                journal.Attempt(
                    $"flush-world-destroy-pass-{pass}",
                    world.FlushPendingDestroyForDiagnostics);
            }

            journal.Attempt("flush-world-destroy-final", world.FlushPendingDestroyForDiagnostics);
        }

        private static bool SettingsMatch(
            LockstepSimulationSettings current,
            LockstepSimulationSettings expected)
        {
            if (current == null || expected == null)
                return current == expected;

            return current.driveMode == expected.driveMode &&
                   current.useUnscaledTime == expected.useUnscaledTime &&
                   current.maxCatchUpTicksPerFrame == expected.maxCatchUpTicksPerFrame &&
                   current.maxBacklogTicks == expected.maxBacklogTicks &&
                   current.inputDelayTicks == expected.inputDelayTicks &&
                   current.requireInputFrameReady == expected.requireInputFrameReady &&
                   current.enableFrameChecksum == expected.enableFrameChecksum;
        }

        private static bool TrySelectLoadedCharacter(
            CharacterAnimtorManager manager,
            GameDataManager dataManager,
            out int oid)
        {
            List<int> ids = manager.GetAllLoadedCharacterIds();
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
            {
                int candidate = ids[i];
                ObjectDefinition definition = dataManager.GetObjectById(candidate);
                LF2CharacterDataWrapper config = manager.GetCharacterConfig(candidate);
                if (definition == null || definition.type != (int)LF2ObjectType.Character ||
                    config?.characterData?.frames == null || config.characterData.frames.Count == 0 ||
                    !manager.TryGetSprites(candidate, out List<Sprite> sprites) || sprites == null ||
                    sprites.Count == 0)
                {
                    continue;
                }
                oid = candidate;
                return true;
            }
            oid = -1;
            return false;
        }

        private static LockstepSimulationSettings CloneSettings(LockstepSimulationSettings source)
        {
            if (source == null)
                return new LockstepSimulationSettings();
            return new LockstepSimulationSettings
            {
                driveMode = source.driveMode,
                useUnscaledTime = source.useUnscaledTime,
                maxCatchUpTicksPerFrame = source.maxCatchUpTicksPerFrame,
                maxBacklogTicks = source.maxBacklogTicks,
                inputDelayTicks = source.inputDelayTicks,
                requireInputFrameReady = source.requireInputFrameReady,
                enableFrameChecksum = source.enableFrameChecksum,
            };
        }

        private static bool IsActive(LF2Character entity)
        {
            return entity != null && entity.Renderer != null && entity.Runtime != null &&
                   entity.Runtime.SlotIndex >= 0;
        }

        private static int Sum(int[] values)
        {
            if (values == null)
                return 0;
            long sum = 0;
            for (int i = 0; i < values.Length; i++)
                sum += values[i];
            return sum > int.MaxValue ? int.MaxValue : (int)sum;
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal static class ProductionEntityStressPaths
    {
        internal const string RequestFile = "Temp/NTSD_ProductionEntityStress.request.json";
        internal const string ResultFile = "Temp/NTSD_ProductionEntityStress.result";

        internal static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        internal static string ProjectPath(string path)
        {
            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(ProjectRoot, path));
        }

        internal static void WriteTerminalResult(bool success, string reportPath, string evidence)
        {
            string path = ProjectPath(ResultFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            string content = (success ? "PASS" : "FAIL") + "\n" +
                             (reportPath ?? string.Empty) + "\n" +
                             (evidence ?? string.Empty);
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
    }
}
#endif


--- File: Temp/NTSD_ProductionEntityStress.dispersed-ai-optimized-production-notiming-20260726.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "inputMode": "ai",
    "startedUtc": "2026-07-26T14:40:47.9429611Z",
    "updatedUtc": "2026-07-26T14:42:55.2143448Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-ai-optimized-production-notiming-20260726.json",
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
    "formalCollectorRequestedMode": "role",
    "formalCollectorMode": "role",
    "forceRoleAwareTreeForDiagnosticsRequested": false,
    "forceRoleAwareTreeForDiagnosticsEffective": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRequested": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsEffective": false,
    "forceLegacyPreInteractionForDiagnosticsRequested": false,
    "forceLegacyPreInteractionForDiagnosticsEffective": false,
    "forceLegacyCharacterInputPassForDiagnosticsRequested": false,
    "forceLegacyCharacterInputPassForDiagnosticsEffective": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsRequested": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsEffective": false,
    "forceLegacyAiNearestFilterForDiagnosticsRequested": false,
    "forceLegacyAiNearestFilterForDiagnosticsEffective": false,
    "formalCollectorBodyEntries": 1995,
    "formalCollectorItrQueries": 3,
    "logicTicksExecuted": 2260,
    "warmupTicksCompleted": 10,
    "sampledLogicTicks": 2250,
    "sampledUnityFrames": 565,
    "framesWithCatchUp": 565,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 1200,
    "aiControlledEntityTicks": 2260000,
    "collisionCandidateCountSum": 21080,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 544007,
    "broadphasePairCountPeak": 23262,
    "broadphaseFallbackParticipantPeak": 154,
    "broadphaseAbortedTicks": 0,
    "broadphaseLastIndexedCount": 0,
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
        "sampleCount": 2250,
        "average": 42.806532355555578,
        "maximum": 115.7769,
        "p95": 69.27364499999996,
        "p99": 84.26183699999997
    },
    "logicTickWithPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=true",
        "unavailableReason": "",
        "sampleCount": 563,
        "average": 55.82273268206038,
        "maximum": 115.7769,
        "p95": 79.99497,
        "p99": 98.236894
    },
    "logicTickWithoutPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=false",
        "unavailableReason": "",
        "sampleCount": 1687,
        "average": 38.462655186722027,
        "maximum": 113.9725,
        "p95": 60.189119999999999,
        "p99": 72.38665199999997
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 565,
        "average": 221.03847059528386,
        "maximum": 915.7537817955017,
        "p95": 302.4958908557891,
        "p99": 352.9199099540713
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 2250,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": false,
    "phaseTimingSource": "",
    "phaseTimings": [],
    "phaseTimingUnattributedMilliseconds": {
        "available": false,
        "unit": "ms",
        "source": "",
        "unavailableReason": "Disabled by request; set enablePhaseTiming to true to collect battle tick phase timings.",
        "sampleCount": 0,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "presentationTimingEnabled": false,
    "presentationTimingSource": "",
    "presentationTimingUnavailableReason": "Disabled by request; set enablePresentationTiming to true to collect coarse presentation publication timings.",
    "presentationTimings": [],
    "detailPhaseTimingEnabled": false,
    "detailPhaseTimingSource": "",
    "detailPhaseTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.",
    "detailPhaseTimings": [],
    "aiInputDetailTimingSource": "",
    "aiInputDetailTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail timings.",
    "aiInputDetailTimings": [],
    "lateRuntimeSnapshotTimingSource": "",
    "lateRuntimeSnapshotTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect LateEntityUpdate RefreshRuntimeSnapshot timings.",
    "lateRuntimeSnapshotTimings": [],
    "aiInputDetailCounters": {
        "available": false,
        "unavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail counters.",
        "aiCount": 0,
        "spatialQueryCount": 0,
        "queriedHandleVisits": 0,
        "candidateVisits": 0,
        "radiusExpansions": 0,
        "bruteFallbackCount": 0,
        "bruteSlotVisits": 0,
        "phase1ListVisits": 0,
        "refreshCount": 0,
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
    "lateOpointCounters": {
        "sampledLatePasses": 2250,
        "opointNoRoleSum": 1278961,
        "opointNoRolePeak": 968,
        "invalidFirstSum": 0,
        "invalidFirstPeak": 0,
        "attackingRejectedSum": 713954,
        "attackingRejectedPeak": 414,
        "characterFrameDelayRejectedSum": 0,
        "characterFrameDelayRejectedPeak": 0,
        "eligibleSum": 257085,
        "eligiblePeak": 168,
        "factoryInvokedSum": 257085,
        "factoryInvokedPeak": 168
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
        "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored": true,
        "forceLegacyPreInteractionForDiagnosticsRestored": true,
        "forceLegacyCharacterInputPassForDiagnosticsRestored": true,
        "forceLegacyAiSnapshotBuildForDiagnosticsRestored": true,
        "forceLegacyAiNearestFilterForDiagnosticsRestored": true,
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
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1000 (delta=990; doesNotAffectRestored=True); preInteractionSwitchRestored=True; characterInputPassSwitchRestored=True; aiSnapshotBuildSwitchRestored=True; aiNearestFilterSwitchRestored=True; zeroAttackItrShortCircuitSwitchRestored=True"
    }
}

--- File: Temp/NTSD_ProductionEntityStress.dispersed-ai-flat-shadow-gate-20260727.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "inputMode": "ai",
    "startedUtc": "2026-07-26T16:40:50.9752458Z",
    "updatedUtc": "2026-07-26T16:42:31.3884141Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-ai-flat-shadow-gate-20260727.json",
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
    "forceRoleAwareTreeForDiagnosticsRequested": false,
    "forceRoleAwareTreeForDiagnosticsEffective": false,
    "forceRoleAwareDirectForDiagnosticsRequested": false,
    "forceRoleAwareDirectForDiagnosticsEffective": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRequested": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsEffective": false,
    "forceLegacyPreInteractionForDiagnosticsRequested": false,
    "forceLegacyPreInteractionForDiagnosticsEffective": false,
    "forceLegacyCharacterInputPassForDiagnosticsRequested": false,
    "forceLegacyCharacterInputPassForDiagnosticsEffective": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsRequested": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsEffective": false,
    "forceLegacyAiNearestFilterForDiagnosticsRequested": false,
    "forceLegacyAiNearestFilterForDiagnosticsEffective": false,
    "enableAiNearestFlatShadowForDiagnosticsRequested": true,
    "enableAiNearestFlatShadowForDiagnosticsEffective": true,
    "aiNearestFlatShadowQueryCount": 783000,
    "aiNearestFlatShadowUnavailableCount": 0,
    "aiNearestFlatShadowMismatchCount": 0,
    "aiNearestFlatGroundSequentialFallbackCount": 391000,
    "aiNearestFlatFirstShadowMismatch": "",
    "formalCollectorBodyEntries": 1990,
    "formalCollectorItrQueries": 7,
    "logicTicksExecuted": 784,
    "warmupTicksCompleted": 15,
    "sampledLogicTicks": 769,
    "sampledUnityFrames": 196,
    "framesWithCatchUp": 196,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 1979,
    "aiControlledEntityTicks": 784000,
    "collisionCandidateCountSum": 18634,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 463209,
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
        "sampleCount": 769,
        "average": 100.51178907672292,
        "maximum": 265.2581,
        "p95": 179.23008,
        "p99": 229.8317479999994
    },
    "logicTickWithPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=true",
        "unavailableReason": "",
        "sampleCount": 193,
        "average": 96.30491917098448,
        "maximum": 236.3683,
        "p95": 184.35275999999988,
        "p99": 210.93701199999979
    },
    "logicTickWithoutPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=false",
        "unavailableReason": "",
        "sampleCount": 576,
        "average": 101.92138263888906,
        "maximum": 265.2581,
        "p95": 178.7259,
        "p99": 237.118425
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 196,
        "average": 487.8722751627163,
        "maximum": 2472.7783203125,
        "p95": 800.8423894643784,
        "p99": 1148.8088846206758
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 769,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": false,
    "phaseTimingSource": "",
    "phaseTimings": [],
    "phaseTimingUnattributedMilliseconds": {
        "available": false,
        "unit": "ms",
        "source": "",
        "unavailableReason": "Disabled by request; set enablePhaseTiming to true to collect battle tick phase timings.",
        "sampleCount": 0,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "presentationTimingEnabled": false,
    "presentationTimingSource": "",
    "presentationTimingUnavailableReason": "Disabled by request; set enablePresentationTiming to true to collect coarse presentation publication timings.",
    "presentationTimings": [],
    "detailPhaseTimingEnabled": false,
    "detailPhaseTimingSource": "",
    "detailPhaseTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.",
    "detailPhaseTimings": [],
    "aiInputDetailTimingSource": "",
    "aiInputDetailTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail timings.",
    "aiInputDetailTimings": [],
    "lateRuntimeSnapshotTimingSource": "",
    "lateRuntimeSnapshotTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect LateEntityUpdate RefreshRuntimeSnapshot timings.",
    "lateRuntimeSnapshotTimings": [],
    "aiInputDetailCounters": {
        "available": false,
        "unavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail counters.",
        "aiCount": 0,
        "spatialQueryCount": 0,
        "queriedHandleVisits": 0,
        "candidateVisits": 0,
        "radiusExpansions": 0,
        "bruteFallbackCount": 0,
        "bruteSlotVisits": 0,
        "phase1ListVisits": 0,
        "refreshCount": 0,
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
    "lateOpointCounters": {
        "sampledLatePasses": 769,
        "opointNoRoleSum": 516757,
        "opointNoRolePeak": 968,
        "invalidFirstSum": 0,
        "invalidFirstPeak": 0,
        "attackingRejectedSum": 181020,
        "attackingRejectedPeak": 403,
        "characterFrameDelayRejectedSum": 0,
        "characterFrameDelayRejectedPeak": 0,
        "eligibleSum": 71223,
        "eligiblePeak": 166,
        "factoryInvokedSum": 71223,
        "factoryInvokedPeak": 166
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
        "forceRoleAwareDirectForDiagnosticsRestored": true,
        "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored": true,
        "forceLegacyPreInteractionForDiagnosticsRestored": true,
        "forceLegacyCharacterInputPassForDiagnosticsRestored": true,
        "forceLegacyAiSnapshotBuildForDiagnosticsRestored": true,
        "forceLegacyAiNearestFilterForDiagnosticsRestored": true,
        "enableAiNearestFlatShadowForDiagnosticsRestored": true,
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
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1000 (delta=990; doesNotAffectRestored=True); preInteractionSwitchRestored=True; characterInputPassSwitchRestored=True; aiSnapshotBuildSwitchRestored=True; aiNearestFilterSwitchRestored=True; aiNearestFlatShadowSwitchRestored=True; zeroAttackItrShortCircuitSwitchRestored=True; roleAwareDirectSwitchRestored=True"
    }
}

--- File: Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-a-quadtree-latefast-20260727.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "inputMode": "ai",
    "startedUtc": "2026-07-26T16:59:49.9535808Z",
    "updatedUtc": "2026-07-26T17:00:39.6357995Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-ai-gate-a-quadtree-latefast-20260727.json",
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
    "forceRoleAwareTreeForDiagnosticsRequested": false,
    "forceRoleAwareTreeForDiagnosticsEffective": false,
    "forceRoleAwareDirectForDiagnosticsRequested": false,
    "forceRoleAwareDirectForDiagnosticsEffective": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRequested": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsEffective": false,
    "forceLegacyPreInteractionForDiagnosticsRequested": false,
    "forceLegacyPreInteractionForDiagnosticsEffective": false,
    "forceLegacyCharacterInputPassForDiagnosticsRequested": false,
    "forceLegacyCharacterInputPassForDiagnosticsEffective": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsRequested": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsEffective": false,
    "forceLegacyAiNearestFilterForDiagnosticsRequested": false,
    "forceLegacyAiNearestFilterForDiagnosticsEffective": false,
    "enableAiNearestFlatShadowForDiagnosticsRequested": false,
    "enableAiNearestFlatShadowForDiagnosticsEffective": false,
    "forceAiNearestFlatQueryForDiagnosticsRequested": false,
    "forceAiNearestFlatQueryForDiagnosticsEffective": false,
    "forceLegacyLateRuntimeSnapshotsForDiagnosticsRequested": false,
    "forceLegacyLateRuntimeSnapshotsForDiagnosticsEffective": false,
    "aiNearestFlatShadowQueryCount": 0,
    "aiNearestFlatShadowUnavailableCount": 0,
    "aiNearestFlatShadowMismatchCount": 0,
    "aiNearestFlatGroundSequentialFallbackCount": 0,
    "aiNearestFlatFormalQueryCount": 0,
    "aiNearestFlatFormalFallbackCount": 0,
    "aiNearestFlatFirstShadowMismatch": "",
    "lateFrameTickRuntimeSnapshotProofCountPeak": 1000,
    "skippedLateFrameTickRuntimeSnapshotCountPeak": 1000,
    "skippedLateDeathRuntimeSnapshotCountPeak": 1000,
    "skippedLateTailRuntimeSnapshotCountPeak": 1000,
    "formalCollectorBodyEntries": 1965,
    "formalCollectorItrQueries": 3,
    "logicTicksExecuted": 584,
    "warmupTicksCompleted": 20,
    "sampledLogicTicks": 564,
    "sampledUnityFrames": 146,
    "framesWithCatchUp": 146,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 756,
    "aiControlledEntityTicks": 584000,
    "collisionCandidateCountSum": 17987,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 446012,
    "broadphasePairCountPeak": 23262,
    "broadphaseFallbackParticipantPeak": 154,
    "broadphaseAbortedTicks": 0,
    "broadphaseLastIndexedCount": 0,
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
        "sampleCount": 564,
        "average": 62.97981542553186,
        "maximum": 206.5526,
        "p95": 109.73657500000002,
        "p99": 145.311742
    },
    "logicTickWithPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=true",
        "unavailableReason": "",
        "sampleCount": 141,
        "average": 80.75394326241141,
        "maximum": 206.5526,
        "p95": 134.2659,
        "p99": 166.66423999999999
    },
    "logicTickWithoutPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=false",
        "unavailableReason": "",
        "sampleCount": 423,
        "average": 57.05510614657211,
        "maximum": 146.4207,
        "p95": 96.26286999999999,
        "p99": 127.71775399999992
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 146,
        "average": 320.997685398141,
        "maximum": 845.4647064208984,
        "p95": 486.06083542108538,
        "p99": 678.8667201995863
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 564,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": false,
    "phaseTimingSource": "",
    "phaseTimings": [],
    "phaseTimingUnattributedMilliseconds": {
        "available": false,
        "unit": "ms",
        "source": "",
        "unavailableReason": "Disabled by request; set enablePhaseTiming to true to collect battle tick phase timings.",
        "sampleCount": 0,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "presentationTimingEnabled": false,
    "presentationTimingSource": "",
    "presentationTimingUnavailableReason": "Disabled by request; set enablePresentationTiming to true to collect coarse presentation publication timings.",
    "presentationTimings": [],
    "detailPhaseTimingEnabled": false,
    "detailPhaseTimingSource": "",
    "detailPhaseTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.",
    "detailPhaseTimings": [],
    "aiInputDetailTimingSource": "",
    "aiInputDetailTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail timings.",
    "aiInputDetailTimings": [],
    "lateRuntimeSnapshotTimingSource": "",
    "lateRuntimeSnapshotTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect LateEntityUpdate RefreshRuntimeSnapshot timings.",
    "lateRuntimeSnapshotTimings": [],
    "aiInputDetailCounters": {
        "available": false,
        "unavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail counters.",
        "aiCount": 0,
        "spatialQueryCount": 0,
        "queriedHandleVisits": 0,
        "candidateVisits": 0,
        "radiusExpansions": 0,
        "bruteFallbackCount": 0,
        "bruteSlotVisits": 0,
        "phase1ListVisits": 0,
        "refreshCount": 0,
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
    "lateOpointCounters": {
        "sampledLatePasses": 564,
        "opointNoRoleSum": 405364,
        "opointNoRolePeak": 968,
        "invalidFirstSum": 0,
        "invalidFirstPeak": 0,
        "attackingRejectedSum": 112936,
        "attackingRejectedPeak": 370,
        "characterFrameDelayRejectedSum": 0,
        "characterFrameDelayRejectedPeak": 0,
        "eligibleSum": 45700,
        "eligiblePeak": 166,
        "factoryInvokedSum": 45700,
        "factoryInvokedPeak": 166
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
        "forceRoleAwareDirectForDiagnosticsRestored": true,
        "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored": true,
        "forceLegacyPreInteractionForDiagnosticsRestored": true,
        "forceLegacyCharacterInputPassForDiagnosticsRestored": true,
        "forceLegacyAiSnapshotBuildForDiagnosticsRestored": true,
        "forceLegacyAiNearestFilterForDiagnosticsRestored": true,
        "enableAiNearestFlatShadowForDiagnosticsRestored": true,
        "forceAiNearestFlatQueryForDiagnosticsRestored": true,
        "forceLegacyLateRuntimeSnapshotsForDiagnosticsRestored": true,
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
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1000 (delta=990; doesNotAffectRestored=True); preInteractionSwitchRestored=True; characterInputPassSwitchRestored=True; aiSnapshotBuildSwitchRestored=True; aiNearestFilterSwitchRestored=True; aiNearestFlatShadowSwitchRestored=True; aiNearestFlatFormalSwitchRestored=True; legacyLateRuntimeSnapshotsSwitchRestored=True; zeroAttackItrShortCircuitSwitchRestored=True; roleAwareDirectSwitchRestored=True"
    }
}

--- File: Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-b-flat-latefast-20260727.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "inputMode": "ai",
    "startedUtc": "2026-07-26T17:03:30.5314801Z",
    "updatedUtc": "2026-07-26T17:04:15.8792900Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-ai-gate-b-flat-latefast-20260727.json",
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
    "forceRoleAwareTreeForDiagnosticsRequested": false,
    "forceRoleAwareTreeForDiagnosticsEffective": false,
    "forceRoleAwareDirectForDiagnosticsRequested": false,
    "forceRoleAwareDirectForDiagnosticsEffective": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRequested": false,
    "forceLegacyZeroAttackItrShortCircuitForDiagnosticsEffective": false,
    "forceLegacyPreInteractionForDiagnosticsRequested": false,
    "forceLegacyPreInteractionForDiagnosticsEffective": false,
    "forceLegacyCharacterInputPassForDiagnosticsRequested": false,
    "forceLegacyCharacterInputPassForDiagnosticsEffective": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsRequested": false,
    "forceLegacyAiSnapshotBuildForDiagnosticsEffective": false,
    "forceLegacyAiNearestFilterForDiagnosticsRequested": false,
    "forceLegacyAiNearestFilterForDiagnosticsEffective": false,
    "enableAiNearestFlatShadowForDiagnosticsRequested": false,
    "enableAiNearestFlatShadowForDiagnosticsEffective": false,
    "forceAiNearestFlatQueryForDiagnosticsRequested": true,
    "forceAiNearestFlatQueryForDiagnosticsEffective": true,
    "forceLegacyLateRuntimeSnapshotsForDiagnosticsRequested": false,
    "forceLegacyLateRuntimeSnapshotsForDiagnosticsEffective": false,
    "aiNearestFlatShadowQueryCount": 0,
    "aiNearestFlatShadowUnavailableCount": 0,
    "aiNearestFlatShadowMismatchCount": 0,
    "aiNearestFlatGroundSequentialFallbackCount": 0,
    "aiNearestFlatFormalQueryCount": 216000,
    "aiNearestFlatFormalFallbackCount": 0,
    "aiNearestFlatFirstShadowMismatch": "",
    "lateFrameTickRuntimeSnapshotProofCountPeak": 1000,
    "skippedLateFrameTickRuntimeSnapshotCountPeak": 1000,
    "skippedLateDeathRuntimeSnapshotCountPeak": 1000,
    "skippedLateTailRuntimeSnapshotCountPeak": 1000,
    "formalCollectorBodyEntries": 1971,
    "formalCollectorItrQueries": 9,
    "logicTicksExecuted": 432,
    "warmupTicksCompleted": 20,
    "sampledLogicTicks": 412,
    "sampledUnityFrames": 108,
    "framesWithCatchUp": 108,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 743,
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
        "sampleCount": 412,
        "average": 70.1808390776699,
        "maximum": 175.5978,
        "p95": 129.337765,
        "p99": 164.641056
    },
    "logicTickWithPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=true",
        "unavailableReason": "",
        "sampleCount": 103,
        "average": 85.10500873786406,
        "maximum": 172.8305,
        "p95": 147.58407,
        "p99": 167.515272
    },
    "logicTickWithoutPresentationMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick; buildPresentation=false",
        "unavailableReason": "",
        "sampleCount": 309,
        "average": 65.20611585760514,
        "maximum": 175.5978,
        "p95": 117.12024,
        "p99": 155.11696000000007
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 108,
        "average": 381.1120871040556,
        "maximum": 1631.4597129821777,
        "p95": 611.7664366960524,
        "p99": 1511.7771351337365
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 412,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": false,
    "phaseTimingSource": "",
    "phaseTimings": [],
    "phaseTimingUnattributedMilliseconds": {
        "available": false,
        "unit": "ms",
        "source": "",
        "unavailableReason": "Disabled by request; set enablePhaseTiming to true to collect battle tick phase timings.",
        "sampleCount": 0,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "presentationTimingEnabled": false,
    "presentationTimingSource": "",
    "presentationTimingUnavailableReason": "Disabled by request; set enablePresentationTiming to true to collect coarse presentation publication timings.",
    "presentationTimings": [],
    "detailPhaseTimingEnabled": false,
    "detailPhaseTimingSource": "",
    "detailPhaseTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.",
    "detailPhaseTimings": [],
    "aiInputDetailTimingSource": "",
    "aiInputDetailTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail timings.",
    "aiInputDetailTimings": [],
    "lateRuntimeSnapshotTimingSource": "",
    "lateRuntimeSnapshotTimingUnavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect LateEntityUpdate RefreshRuntimeSnapshot timings.",
    "lateRuntimeSnapshotTimings": [],
    "aiInputDetailCounters": {
        "available": false,
        "unavailableReason": "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail counters.",
        "aiCount": 0,
        "spatialQueryCount": 0,
        "queriedHandleVisits": 0,
        "candidateVisits": 0,
        "radiusExpansions": 0,
        "bruteFallbackCount": 0,
        "bruteSlotVisits": 0,
        "phase1ListVisits": 0,
        "refreshCount": 0,
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
    "lateOpointCounters": {
        "sampledLatePasses": 412,
        "opointNoRoleSum": 320979,
        "opointNoRolePeak": 968,
        "invalidFirstSum": 0,
        "invalidFirstPeak": 0,
        "attackingRejectedSum": 64232,
        "attackingRejectedPeak": 332,
        "characterFrameDelayRejectedSum": 0,
        "characterFrameDelayRejectedPeak": 0,
        "eligibleSum": 26789,
        "eligiblePeak": 146,
        "factoryInvokedSum": 26789,
        "factoryInvokedPeak": 146
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
        "forceRoleAwareDirectForDiagnosticsRestored": true,
        "forceLegacyZeroAttackItrShortCircuitForDiagnosticsRestored": true,
        "forceLegacyPreInteractionForDiagnosticsRestored": true,
        "forceLegacyCharacterInputPassForDiagnosticsRestored": true,
        "forceLegacyAiSnapshotBuildForDiagnosticsRestored": true,
        "forceLegacyAiNearestFilterForDiagnosticsRestored": true,
        "enableAiNearestFlatShadowForDiagnosticsRestored": true,
        "forceAiNearestFlatQueryForDiagnosticsRestored": true,
        "forceLegacyLateRuntimeSnapshotsForDiagnosticsRestored": true,
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
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1000 (delta=990; doesNotAffectRestored=True); preInteractionSwitchRestored=True; characterInputPassSwitchRestored=True; aiSnapshotBuildSwitchRestored=True; aiNearestFilterSwitchRestored=True; aiNearestFlatShadowSwitchRestored=True; aiNearestFlatFormalSwitchRestored=True; legacyLateRuntimeSnapshotsSwitchRestored=True; zeroAttackItrShortCircuitSwitchRestored=True; roleAwareDirectSwitchRestored=True"
    }
}

[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# Architect review: Unity NTSD 1000 full-AI feasibility gate

Review the current code and fresh performance evidence. The unique battle-logic authority is:
`J:\QQFile\NTSD2.4\ntsd_release_C#`.

Scope:
- 1000 real production GameObjects
- all AI, formal input/collision/hit/opoint/lifecycle
- fixed 30 Hz battle logic
- C# slot order and same-tick visibility must remain unchanged
- T8 and Android are excluded

Fresh evidence:
- Earlier best production run:
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-optimized-production-notiming-20260726.json`
  average 42.807 ms/tick.
- Flat shadow gate:
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-flat-shadow-gate-20260727.json`
  783,000 queries, 0 unavailable, 0 mismatch, 391,000 sequential ground fallbacks,
  clean teardown.
- Gate A (quadtree, Late fast):
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-a-quadtree-latefast-20260727.json`
  average 62.980 ms/tick under current machine load.
- Gate B (formal flat, no quadtree sync, Late fast):
  `Temp/NTSD_ProductionEntityStress.dispersed-ai-gate-b-flat-latefast-20260727.json`
  early 220-tick snapshot average 54.647 ms/tick; terminal rolling report average
  70.181 ms/tick after load drift; 216,000 formal flat queries, 0 fallback, clean teardown.
- Full EditMode run job `31cf5315cdc64b75bbe03101ff86de10`:
  301 executed, 296 passed, 5 failures not in the new flat/Late tests.
- Fresh Unity compile after final wiring: Console 0 errors.

Please answer:
1. Does the evidence justify stopping further micro-optimization and concluding that the
   current GameObject/per-entity main-thread architecture cannot reliably meet 33.3 ms
   for this 1000-full-AI workload while preserving exact C# order?
2. Review the new flat formal path and Late snapshot proof for correctness hazards.
3. Which changes should remain default-off diagnostics, which are safe to retain, and
   should the unproven Late snapshot proof be disabled by default or retracted?
4. Give severity-rated blockers and a concise architecture migration boundary
   (data-oriented hot loop / ECS), without proposing more micro-optimizations.

Do not edit files. Produce an evidence-backed review with exact file/method references.
