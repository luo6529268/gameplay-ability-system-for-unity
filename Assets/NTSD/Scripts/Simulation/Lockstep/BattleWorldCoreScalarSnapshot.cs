using System;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public readonly struct BattleWorldMatchScalarSnapshot
    {
        internal BattleWorldMatchScalarSnapshot(BattleMatchRuntimeState state)
        {
            LocalGameModeId = state?.LocalGameModeId ?? 0;
            BattleGameModeId = state?.BattleGameModeId ?? 0;
            BackgroundId = state?.BackgroundId ?? -1;
            Difficulty = state?.Difficulty ?? 2;
            StageIdx = state?.StageIdx ?? 0;
            RandomStage = state?.RandomStage ?? 0;
            RuntimeStageCount = state?.RuntimeStageCount ?? 0;
            Seed = state?.Seed ?? 0;
            PpMode = state?.PpMode ?? true;
        }

        public int LocalGameModeId { get; }
        public int BattleGameModeId { get; }
        public int BackgroundId { get; }
        public int Difficulty { get; }
        public int StageIdx { get; }
        public int RandomStage { get; }
        public int RuntimeStageCount { get; }
        public int Seed { get; }
        public bool PpMode { get; }
    }

    public readonly struct BattleWorldStageScalarSnapshot
    {
        internal BattleWorldStageScalarSnapshot(BattleStageRuntimeState state)
        {
            BaseStageWidthPx = state?.BaseStageWidthPx ?? 800;
            StageWidthPx = state?.StageWidthPx ?? 800;
            ZMin = state?.ZMin ?? 180;
            ZMax = state?.ZMax ?? 350;
            PerspectiveNear = state?.PerspectiveNear ?? 0;
            PerspectiveFar = state?.PerspectiveFar ?? 0;
            BoundLeft = state?.BoundLeft ?? 0;
            BoundRight = state?.BoundRight ?? 800;
            XMaxOverride = state?.XMaxOverride ?? 0;
            CameraMaxOverride = state?.CameraMaxOverride ?? 0;
        }

        public int BaseStageWidthPx { get; }
        public int StageWidthPx { get; }
        public int ZMin { get; }
        public int ZMax { get; }
        public int PerspectiveNear { get; }
        public int PerspectiveFar { get; }
        public int BoundLeft { get; }
        public int BoundRight { get; }
        public int XMaxOverride { get; }
        public int CameraMaxOverride { get; }
    }

    public readonly struct BattleWorldProgressionScalarSnapshot
    {
        internal BattleWorldProgressionScalarSnapshot(BattleRuntimeState runtime)
        {
            BattleStageProgressionState state = runtime?.StageProgression;
            StageSeriesIdx = state?.StageSeriesIdx ?? 0;
            WaveIdx = state?.WaveIdx ?? -1;
            Round = state?.Round ?? 0;
            RoundMax = state?.RoundMax ?? 0;
            StageProgressionValid = runtime?.StageProgressionValid ?? false;
            StageSpawnWaveApplied = runtime?.StageSpawnWaveApplied ?? -1;
            StageSpawnWaveDeferredEntryApplied =
                runtime?.StageSpawnWaveDeferredEntryApplied ?? -1;
            StageSpawnRuntimeWave = runtime?.StageSpawnRuntimeWave ?? -1;
        }

        public int StageSeriesIdx { get; }
        public int WaveIdx { get; }
        public int Round { get; }
        public int RoundMax { get; }
        public bool StageProgressionValid { get; }
        public int StageSpawnWaveApplied { get; }
        public int StageSpawnWaveDeferredEntryApplied { get; }
        public int StageSpawnRuntimeWave { get; }
    }

    public readonly struct BattleWorldFlowScalarSnapshot
    {
        internal BattleWorldFlowScalarSnapshot(BattleFlowRuntimeState state)
        {
            CurrentTickIndex = state?.CurrentTickIndex ?? 0;
            SparkRenderFrame = state?.SparkRenderFrame ?? 0;
            AiPhaseGate = state?.AiPhaseGate ?? 0;
            InputPhase = state?.InputPhase ?? 0;
            FrameMod12 = state?.FrameMod12 ?? 0;
            FrameToggle = state?.FrameToggle ?? 0;
            AiDifficulty = state?.AiDifficulty ?? 0;
            AiRand3 = state?.AiRand3 ?? 0;
            AiRand5 = state?.AiRand5 ?? 0;
            AiRand15 = state?.AiRand15 ?? 0;
            AiRand20 = state?.AiRand20 ?? 0;
            AiMoveMode = state?.AiMoveMode ?? 0;
            AiStageTargetX = state?.AiStageTargetX ?? 0;
            BattleExitCountdown = state?.BattleExitCountdown ?? 0;
            RouteOutRequest = state?.RouteOutRequest ?? 0;
            Mode2Request = state?.Mode2Request ?? 0;
            BattleStepMode = state?.BattleStepMode ?? 0;
            BattleStepGate = state?.BattleStepGate ?? 0;
            DjaGuardGlobal44F224 = state?.DjaGuardGlobal44F224 ?? 0;
            HumanInputPolledExternally = state?.HumanInputPolledExternally ?? false;
            NeedClearInput = state?.NeedClearInput ?? false;
        }

        public int CurrentTickIndex { get; }
        public int SparkRenderFrame { get; }
        public int AiPhaseGate { get; }
        public int InputPhase { get; }
        public int FrameMod12 { get; }
        public int FrameToggle { get; }
        public int AiDifficulty { get; }
        public int AiRand3 { get; }
        public int AiRand5 { get; }
        public int AiRand15 { get; }
        public int AiRand20 { get; }
        public int AiMoveMode { get; }
        public int AiStageTargetX { get; }
        public int BattleExitCountdown { get; }
        public int RouteOutRequest { get; }
        public int Mode2Request { get; }
        public int BattleStepMode { get; }
        public int BattleStepGate { get; }
        public int DjaGuardGlobal44F224 { get; }
        public bool HumanInputPolledExternally { get; }
        public bool NeedClearInput { get; }
    }

    /// <summary>
    /// Immutable allocation-free capture of the scalar world domains. This is an
    /// incremental U7 schema product, not a complete restorable battle snapshot.
    /// Entity, slot, rest, roster, result, stage-buffer and event payloads are not
    /// represented yet, so no restore API is exposed for this type.
    /// </summary>
    public readonly struct BattleWorldCoreScalarSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        internal BattleWorldCoreScalarSnapshot(
            SimulationWorld world,
            LockstepSessionIdentity identity)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            SchemaVersion = CurrentSchemaVersion;
            ProtocolSchemaVersion = identity.SchemaVersion;
            IdentityFingerprint = identity.IdentityFingerprint;
            RuntimeProfile = world.RuntimeProfileForServices;
            RuntimeSlotCapacity = world.MaxRuntimeSlotsForServices;
            CollisionBroadphase = world.CollisionBroadphaseForServices;
            ObjectCount = world.ObjectCount;
            ClaimedRuntimeSlotCount = world.ClaimedRuntimeSlotCountForServices;
            Match = new BattleWorldMatchScalarSnapshot(world.Runtime?.Match);
            Stage = new BattleWorldStageScalarSnapshot(world.Runtime?.Stage);
            Progression = new BattleWorldProgressionScalarSnapshot(world.Runtime);
            Flow = new BattleWorldFlowScalarSnapshot(world.Runtime?.Flow);
            RngState = world.Rng?.State ?? 0U;
            RngCallCount = world.Rng?.CallCount ?? 0UL;
            ReleaseCameraX = world.ReleaseCameraX;
            ReleaseCameraVelocity = world.ReleaseCameraVelocityForServices;
            NextAutoStableId = world.NextAutoStableIdForServices;
        }

        public int SchemaVersion { get; }
        public int ProtocolSchemaVersion { get; }
        public ulong IdentityFingerprint { get; }
        public BattleRuntimeProfile RuntimeProfile { get; }
        public int RuntimeSlotCapacity { get; }
        public CollisionBroadphaseBackend CollisionBroadphase { get; }
        public int ObjectCount { get; }
        public int ClaimedRuntimeSlotCount { get; }
        public BattleWorldMatchScalarSnapshot Match { get; }
        public BattleWorldStageScalarSnapshot Stage { get; }
        public BattleWorldProgressionScalarSnapshot Progression { get; }
        public BattleWorldFlowScalarSnapshot Flow { get; }
        public uint RngState { get; }
        public ulong RngCallCount { get; }
        public int ReleaseCameraX { get; }
        public int ReleaseCameraVelocity { get; }
        public int NextAutoStableId { get; }
    }

    internal sealed class BattleWorldCoreScalarSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldCoreScalarSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal BattleWorldCoreScalarSnapshot Capture(
            LockstepSessionIdentity identity)
        {
            return new BattleWorldCoreScalarSnapshot(world, identity);
        }
    }
}
