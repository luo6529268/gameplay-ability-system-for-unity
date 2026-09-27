#if UNITY_EDITOR
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q10NativeBattleVolumePhysicalPlayProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/Q10/Run Native Battle Volume Physical Play Probe";
        private static readonly string ResultPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001", "play-result.json"));

        private static SimulationTickDriver driver;
        private static NTSDSoundPlayer soundPlayer;
        private static Keyboard keyboard;
        private static Report report;
        private static int phase;
        private static int targetFrame;
        private static int targetTick;
        private static double deadline;
        private static bool previousPaused;
        private static bool previousStressSuppression;
        private static bool loadedBattle;
        private static bool finishing;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            EditorApplication.update -= Observe;
            if (finishing)
                return;

            report = new Report
            {
                status = "RUNNING",
                initialScene = SceneManager.GetActiveScene().path,
            };
            Save();
            if (!EditorApplication.isPlaying ||
                SceneManager.GetActiveScene().name != "NTSD_Menu")
            {
                Finish(false, "Original Menu Scene must be active in Play Mode.");
                return;
            }

            driver = null;
            soundPlayer = null;
            keyboard = null;
            loadedBattle = false;
            phase = 0;
            deadline = EditorApplication.timeSinceStartup + 180.0;
            previousStressSuppression =
                BattleTestBootstrap.SuppressEntityCreationForProductionStress;
            PrepareMenuBattle().Forget();
        }

        private static async UniTask PrepareMenuBattle()
        {
            try
            {
                Check(!SceneManager.GetActiveScene().isDirty,
                    "The saved Menu Scene is dirty before the probe.");
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
                await UniTask.NextFrame();
                LoadingPrewarmController loading =
                    UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true);
                Check(loading != null, "Menu prewarm controller is missing.");
                await loading.PrewarmOnceAsync();
                CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                Check(manager?.GetCharacterConfig(2) != null &&
                      manager.GetCharacterConfig(7) != null,
                    "Selected formal character definitions were not prewarmed.");
                Check(!string.IsNullOrEmpty(
                        await manager.ValidateConfiguredContentForBattleAsync()),
                    "Formal battle content identity is unavailable.");

                var match = new MatchConfig
                {
                    seed = 2833,
                    gameMode = new GameModeConfig { battleGameModeId = 0 },
                };
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = true, characterId = 2,
                    team = 1, inputId = 1,
                });
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = true, characterId = 7,
                    team = 2, inputId = 2,
                });
                AppManager app = AppManager.Instance;
                Check(app != null, "Menu AppManager is unavailable.");
                app.SetMatchConfig(match);
                AsyncOperation load = app.LoadBattleAdditive();
                Check(load != null, "Additive Battle load was refused.");
                loadedBattle = true;
                await load.ToUniTask();
                deadline = EditorApplication.timeSinceStartup + 120.0;
                EditorApplication.update += Observe;
            }
            catch (Exception exception)
            {
                Finish(false, "Menu-to-Battle setup: " + exception);
            }
        }

        private static void Observe()
        {
            try
            {
                Check(EditorApplication.isPlaying, "Play Mode ended during the probe.");
                Check(EditorApplication.timeSinceStartup <= deadline,
                    "Timed out waiting for physical key phase " + phase + ".");

                if (phase == 0)
                {
                    driver = SimulationTickDriver.Instance;
                    soundPlayer = AppManager.Instance?.SoundPlayer;
                    keyboard = Keyboard.current;
                    if (driver?.LifecycleState != BattleRuntimeLifecycleState.Running ||
                        driver.World?.RuntimeDataCatalog?.IsReady != true ||
                        driver.CurrentTickIndex < 2 || soundPlayer == null ||
                        keyboard == null)
                    {
                        return;
                    }

                    report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
                    Check(!string.IsNullOrEmpty(report.contentRoot) &&
                          report.contentRoot.Replace('\\', '/').EndsWith(
                              "Assets/NTSD/Content/LoganRuntime", StringComparison.Ordinal),
                        "Selected formal battle content root is absent.");
                    report.startTick = driver.CurrentTickIndex;
                    report.initialPercent =
                        soundPlayer.NativeBattleVolumePercentForDiagnostics;
                    Check(report.initialPercent == 100,
                        "Battle SFX percent did not start at 100.");
                    previousPaused = driver.IsPaused;
                    driver.SetPaused(true);
                    targetTick = driver.CurrentTickIndex;
                    QueueKeys(Key.F11);
                    Advance(1);
                    return;
                }

                if (Time.frameCount < targetFrame)
                    return;

                switch (phase)
                {
                    case 1:
                        Check(driver.CurrentTickIndex == targetTick &&
                              soundPlayer.NativeBattleVolumePercentForDiagnostics == 100,
                            "Held F11 changed percent without a successful tick.");
                        Check(driver.NativeFunctionKeyContinuousHostCommandForDiagnostics ==
                              NTSD28NativeFunctionKeyHostCommand.VolumeDown,
                            "Physical F11 did not reach the continuous host command.");
                        report.pausedNoTickPassed = true;
                        QueueKeys(Key.F11, Key.F2);
                        Advance(2);
                        return;
                    case 2:
                        if (driver.CurrentTickIndex == targetTick)
                            return;
                        Check(driver.CurrentTickIndex == targetTick + 1 &&
                              driver.IsPaused &&
                              soundPlayer.NativeBattleVolumePercentForDiagnostics == 99,
                            "Physical F11+F2 did not lower battle volume once on one tick.");
                        report.downTick = driver.CurrentTickIndex;
                        report.downPercent =
                            soundPlayer.NativeBattleVolumePercentForDiagnostics;
                        QueueKeys();
                        targetTick = driver.CurrentTickIndex;
                        Advance(3);
                        return;
                    case 3:
                        Check(driver.CurrentTickIndex == targetTick &&
                              soundPlayer.NativeBattleVolumePercentForDiagnostics == 99,
                            "Released F11/F2 changed percent or advanced a tick.");
                        report.releaseAfterDownPassed = true;
                        QueueKeys(Key.F11, Key.F12);
                        Advance(4);
                        return;
                    case 4:
                        Check(driver.CurrentTickIndex == targetTick &&
                              soundPlayer.NativeBattleVolumePercentForDiagnostics == 99,
                            "F11+F12 changed percent while paused without a tick.");
                        Check(driver.NativeFunctionKeyContinuousHostCommandForDiagnostics ==
                              NTSD28NativeFunctionKeyHostCommand.VolumeUp,
                            "Physical F11+F12 did not preserve F12 priority.");
                        report.f12PriorityPassed = true;
                        QueueKeys(Key.F11, Key.F12, Key.F2);
                        Advance(5);
                        return;
                    case 5:
                        if (driver.CurrentTickIndex == targetTick)
                            return;
                        Check(driver.CurrentTickIndex == targetTick + 1 &&
                              driver.IsPaused &&
                              soundPlayer.NativeBattleVolumePercentForDiagnostics == 100,
                            "Physical F11+F12+F2 did not raise volume once on one tick.");
                        report.upTick = driver.CurrentTickIndex;
                        report.upPercent =
                            soundPlayer.NativeBattleVolumePercentForDiagnostics;
                        QueueKeys();
                        targetTick = driver.CurrentTickIndex;
                        Advance(6);
                        return;
                    case 6:
                        Check(driver.CurrentTickIndex == targetTick &&
                              soundPlayer.NativeBattleVolumePercentForDiagnostics == 100,
                            "Final key release changed percent or advanced a tick.");
                        report.finalReleasePassed = true;
                        Finish(true, string.Empty);
                        return;
                }
            }
            catch (Exception exception)
            {
                Finish(false, exception.ToString());
            }
        }

        private static void QueueKeys(params Key[] keys)
        {
            Check(keyboard != null, "Keyboard disappeared during the probe.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }

        private static void Advance(int nextPhase)
        {
            phase = nextPhase;
            targetFrame = Time.frameCount + 2;
            deadline = EditorApplication.timeSinceStartup + 15.0;
        }

        private static void Finish(bool passed, string message)
        {
            if (finishing)
                return;
            finishing = true;
            EditorApplication.update -= Observe;
            report.status = passed ? "PASS" : "FAIL";
            report.error = message;
            report.endTick = driver != null ? driver.CurrentTickIndex : -1;
            CleanupAsync().Forget();
        }

        private static async UniTask CleanupAsync()
        {
            try
            {
                if (keyboard != null)
                    QueueKeys();
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running)
                    driver.SetPaused(previousPaused);
                if (loadedBattle && EditorApplication.isPlaying)
                {
                    AsyncOperation unload = AppManager.Instance?.UnloadBattle();
                    Check(unload != null, "Loaded Battle could not be unloaded.");
                    await unload.ToUniTask();
                    report.battleUnloaded = true;
                }
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.error += "\nCLEANUP: " + exception;
            }
            finally
            {
                BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                    previousStressSuppression;
                Save();
                driver = null;
                soundPlayer = null;
                keyboard = null;
                finishing = false;
            }
        }

        private static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ResultPath));
            File.WriteAllText(ResultPath, JsonUtility.ToJson(report, true));
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string initialScene;
            public string contentRoot;
            public int startTick;
            public int initialPercent;
            public bool pausedNoTickPassed;
            public int downTick;
            public int downPercent;
            public bool releaseAfterDownPassed;
            public bool f12PriorityPassed;
            public int upTick;
            public int upPercent;
            public bool finalReleasePassed;
            public int endTick;
            public bool battleUnloaded;
        }
    }
}
#endif
