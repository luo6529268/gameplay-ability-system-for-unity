using NTSD.Simulation.Presentation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns optional per-world diagnostics. Diagnostic recorders remain lazy so a
    /// production world pays only for this stable module reference unless explicitly enabled.
    /// </summary>
    public sealed class SimulationDiagnosticsModule
    {
        private BattleTickPhaseDiagnostics battleTickPhaseDiagnostics;
        private BattleTickDetailPhaseDiagnostics battleTickDetailPhaseDiagnostics;
        private BattleAiInputDetailDiagnostics battleAiInputDetailDiagnostics;
        private BattlePresentationPhaseDiagnostics battlePresentationPhaseDiagnostics;

        public bool BattleTickDetailAllocated =>
            battleTickDetailPhaseDiagnostics != null;

        public bool BattleAiInputDetailAllocated =>
            battleAiInputDetailDiagnostics != null;

        public bool BattlePresentationPhaseAllocated =>
            battlePresentationPhaseDiagnostics != null;

        public BattleTickPhaseDiagnostics ActiveBattleTickPhase =>
            battleTickPhaseDiagnostics != null && battleTickPhaseDiagnostics.Enabled
                ? battleTickPhaseDiagnostics
                : null;

        public BattleTickDetailPhaseDiagnostics ActiveBattleTickDetailPhase =>
            battleTickDetailPhaseDiagnostics != null &&
            battleTickDetailPhaseDiagnostics.Enabled
                ? battleTickDetailPhaseDiagnostics
                : null;

        public BattleAiInputDetailDiagnostics ActiveBattleAiInputDetail =>
            battleAiInputDetailDiagnostics != null &&
            battleAiInputDetailDiagnostics.Enabled
                ? battleAiInputDetailDiagnostics
                : null;

        public BattlePresentationPhaseDiagnostics ActiveBattlePresentationPhase =>
            battlePresentationPhaseDiagnostics != null &&
            battlePresentationPhaseDiagnostics.Enabled
                ? battlePresentationPhaseDiagnostics
                : null;

        public void PrepareEnabledProfilerMarkers()
        {
            if (battleTickPhaseDiagnostics?.Enabled == true)
                BattleTickPhaseDiagnostics.PrepareProfilerMarkers();
            if (battleTickDetailPhaseDiagnostics?.Enabled == true)
                BattleTickDetailPhaseDiagnostics.PrepareProfilerMarkers();
        }

        public BattleTickPhaseDiagnostics EnableBattleTickPhase()
        {
            if (battleTickPhaseDiagnostics == null)
                battleTickPhaseDiagnostics = new BattleTickPhaseDiagnostics();
            battleTickPhaseDiagnostics.SetEnabled(true);
            return battleTickPhaseDiagnostics;
        }

        public void DisableBattleTickPhase()
        {
            battleTickPhaseDiagnostics?.SetEnabled(false);
        }

        public BattleTickDetailPhaseDiagnostics EnableBattleTickDetailPhase()
        {
            if (battleTickDetailPhaseDiagnostics == null)
            {
                battleTickDetailPhaseDiagnostics =
                    new BattleTickDetailPhaseDiagnostics();
            }
            battleTickDetailPhaseDiagnostics.SetEnabled(true);
            return battleTickDetailPhaseDiagnostics;
        }

        public void DisableBattleTickDetailPhase()
        {
            battleTickDetailPhaseDiagnostics?.SetEnabled(false);
        }

        public BattleAiInputDetailDiagnostics EnableBattleAiInputDetail()
        {
            if (battleAiInputDetailDiagnostics == null)
                battleAiInputDetailDiagnostics = new BattleAiInputDetailDiagnostics();
            battleAiInputDetailDiagnostics.SetEnabled(true);
            return battleAiInputDetailDiagnostics;
        }

        public void DisableBattleAiInputDetail()
        {
            battleAiInputDetailDiagnostics?.SetEnabled(false);
        }

        public BattlePresentationPhaseDiagnostics EnableBattlePresentationPhase()
        {
            if (battlePresentationPhaseDiagnostics == null)
            {
                battlePresentationPhaseDiagnostics =
                    new BattlePresentationPhaseDiagnostics();
            }
            battlePresentationPhaseDiagnostics.SetEnabled(true);
            return battlePresentationPhaseDiagnostics;
        }

        public void DisableBattlePresentationPhase()
        {
            battlePresentationPhaseDiagnostics?.SetEnabled(false);
        }
    }
}
