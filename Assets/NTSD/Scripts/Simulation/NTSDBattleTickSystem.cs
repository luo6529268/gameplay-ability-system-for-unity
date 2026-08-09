using System;
using System.Diagnostics;
using Unity.Profiling;

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
        private static class PhaseProfilerMarkers
        {
            internal static readonly ProfilerMarker[] All = Create();

            private static ProfilerMarker[] Create()
            {
                var markers = new ProfilerMarker[(int)BattleTickPhase.Count];
                for (int index = 0; index < markers.Length; index++)
                {
                    BattleTickPhase phase = (BattleTickPhase)index;
                    markers[index] = new ProfilerMarker(
                        "NTSD.BattleTick." + GetPhaseName(phase));
                }
                return markers;
            }
        }

        private readonly long[] elapsedTimestampTicks = new long[(int)BattleTickPhase.Count];
        private BattleTickPhase activePhase = BattleTickPhase.Count;
        private long activePhaseTimestamp;

        public static int PhaseCount => (int)BattleTickPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastTickIndex { get; private set; } = -1;
        public bool HasActivePhaseForDiagnostics =>
            activePhase != BattleTickPhase.Count;
        public BattleTickPhase ActivePhaseForDiagnostics => activePhase;

        public void SetEnabled(bool enabled)
        {
            if (Enabled)
                EndActivePhase();
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

            EndActivePhase();
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            LastTickIndex = tickIndex;
        }

        public void BeginPhase(BattleTickPhase phase)
        {
            if (!Enabled || (uint)phase >= (uint)BattleTickPhase.Count)
                return;

            EndActivePhase();
            activePhase = phase;
            activePhaseTimestamp = Stopwatch.GetTimestamp();
            PhaseProfilerMarkers.All[(int)phase].Begin();
        }

        public void EndPhase(BattleTickPhase phase)
        {
            if (!Enabled || activePhase != phase)
                return;

            EndActivePhase();
        }

        public void EndTick()
        {
            if (!Enabled)
                return;

            EndActivePhase();
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

        private void EndActivePhase()
        {
            BattleTickPhase phase = activePhase;
            if ((uint)phase >= (uint)BattleTickPhase.Count)
                return;

            elapsedTimestampTicks[(int)phase] +=
                Stopwatch.GetTimestamp() - activePhaseTimestamp;
            PhaseProfilerMarkers.All[(int)phase].End();
            activePhase = BattleTickPhase.Count;
            activePhaseTimestamp = 0;
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
            world.BeginDataObjectTypeTickCache(tickIndex);
            try
            {
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
            finally
            {
                world.EndDataObjectTypeTickCache();
                detailDiagnostics?.EndTick();
                diagnostics?.EndTick();
            }
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
