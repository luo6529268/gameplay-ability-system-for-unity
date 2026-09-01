using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.App.Editor
{
    [InitializeOnLoad]
    internal static class BattleRuntimeEditorShutdownBridge
    {
        // Alignment contract: BATTLE-RUNTIME-ORDERED-SHUTDOWN-001.
        static BattleRuntimeEditorShutdownBridge()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            AppManager app = AppManager.Instance;
            if (app != null)
            {
                app.TryShutdownBattleRuntimeBeforeSceneDestroy(out _);
                return;
            }

            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver == null)
                return;

            BattleRuntimeShutdownReport report = driver.ShutdownBattleRuntime();
            if (!report.RuntimeStagesCompleted)
                return;

            bool runtimeMapCleared = true;
            BattleBootstrap[] bootstraps =
                Resources.FindObjectsOfTypeAll<BattleBootstrap>();
            for (int index = 0; index < bootstraps.Length; index++)
            {
                BattleBootstrap bootstrap = bootstraps[index];
                if (bootstrap == null ||
                    EditorUtility.IsPersistent(bootstrap) ||
                    !bootstrap.gameObject.scene.IsValid())
                {
                    continue;
                }

                bootstrap.DisablePresentation();
                runtimeMapCleared &= bootstrap.IsRuntimeMapCleared;
            }

            driver.CompleteBattleRuntimeShutdownAfterMapCleanup(runtimeMapCleared);
        }
    }
}
