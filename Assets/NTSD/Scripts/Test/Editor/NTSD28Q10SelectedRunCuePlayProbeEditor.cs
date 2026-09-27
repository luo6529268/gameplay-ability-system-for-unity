#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
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
    public static class NTSD28Q10SelectedRunCuePlayProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/Q10/Run Selected Naruto Run Cue Play Probe";
        private const string StereoMenuPath =
            "NTSD/Battle Diagnostics/Q10/Run Selected Naruto Stereo Event Probe";
        private static readonly string DefaultResultPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-Q10-SELECTED-RUN-CUE-PLAY-001", "play-result.json"));
        private static readonly string StereoResultPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-Q10-STEREO-SELECTED-EVENT-PROBE-001", "play-result.json"));
        private static string resultPath = DefaultResultPath;

        private static SimulationTickDriver driver;
        private static NTSDSoundPlayer soundPlayer;
        private static LF2Character actor;
        private static Keyboard keyboard;
        private static ForwardingRecorder recorder;
        private static Report report;
        private static int phase;
        private static int phaseStartTick;
        private static double deadline;
        private static bool previousPaused;
        private static bool previousStressSuppression;
        private static bool loadedBattle;
        private static bool finishing;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            RunCore(false);
        }

        [MenuItem(StereoMenuPath)]
        public static void RunStereoEventProbe()
        {
            RunCore(true);
        }

        private static void RunCore(bool captureStereoParameters)
        {
            EditorApplication.update -= Observe;
            if (finishing)
                return;

            resultPath = captureStereoParameters ? StereoResultPath : DefaultResultPath;
            report = new Report
            {
                status = "RUNNING",
                initialScene = SceneManager.GetActiveScene().path,
                captureStereoParameters = captureStereoParameters,
            };
            previousStressSuppression =
                BattleTestBootstrap.SuppressEntityCreationForProductionStress;
            Save();
            if (!EditorApplication.isPlaying ||
                SceneManager.GetActiveScene().name != "NTSD_Menu")
            {
                Finish(false, "Original Menu Scene must be active in Play Mode.");
                return;
            }

            driver = null;
            soundPlayer = null;
            actor = null;
            keyboard = null;
            recorder = null;
            loadedBattle = false;
            phase = 0;
            deadline = EditorApplication.timeSinceStartup + 180.0;
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
                Check(EditorApplication.isPlaying,
                    "Play Mode ended during the probe.");
                Check(EditorApplication.timeSinceStartup <= deadline,
                    "Timed out waiting for Naruto running cues; phase=" + phase + ".");

                if (phase == 0)
                {
                    driver = SimulationTickDriver.Instance;
                    soundPlayer = AppManager.Instance?.SoundPlayer;
                    keyboard = Keyboard.current;
                    if (driver?.LifecycleState != BattleRuntimeLifecycleState.Running ||
                        driver.World?.RuntimeDataCatalog?.IsReady != true ||
                        driver.CurrentTickIndex < 2 || soundPlayer == null ||
                        keyboard == null)
                        return;

                    report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
                    Check(!string.IsNullOrEmpty(report.contentRoot) &&
                          report.contentRoot.Replace('\\', '/').EndsWith(
                              "Assets/NTSD/Content/LoganRuntime",
                              StringComparison.Ordinal),
                        "Selected formal battle content root is absent.");
                    actor = driver.World.FindEntityByRuntimeSlotForQuery(0)
                        as LF2Character;
                    Check(actor?.ObjectId == 2 &&
                          actor.Runtime.SourceRulePositionInitialized,
                        "Formal Naruto P1/source position is unavailable.");
                    CharacterInputModule input = actor.Controller as CharacterInputModule;
                    Check(input?.MoveAction?.enabled == true,
                        "Production P1 movement action is disabled.");
                    Check(soundPlayer.BattleCatalogSealedForDiagnostics,
                        "Battle sound catalog has not been sealed.");
                    Check(PreparedClipExists("data\\003.wav") &&
                          PreparedClipExists("data\\004.wav"),
                        "Selected running cues do not have preloaded Unity clips.");

                    report.startTick = driver.CurrentTickIndex;
                    report.startX = actor.Runtime.XInt;
                    report.initialDispatchedCount =
                        driver.DispatchedSoundEventCountForDiagnostics;
                    report.initialPooledPlayCount =
                        soundPlayer.PooledOneShotPlayCountForDiagnostics;
                    report.initialRejectedCueCount =
                        soundPlayer.RejectedUnpreparedCueCountForDiagnostics;
                    previousPaused = driver.IsPaused;
                    recorder = new ForwardingRecorder(driver, soundPlayer);
                    driver.SetSoundPresentationSinkForDiagnostics(recorder);
                    phaseStartTick = driver.CurrentTickIndex;
                    QueueKeys(Key.D);
                    phase = 1;
                    deadline = EditorApplication.timeSinceStartup + 35.0;
                    return;
                }

                int tick = driver.CurrentTickIndex;
                if (phase == 1 && tick >= phaseStartTick + 2)
                {
                    QueueKeys();
                    phaseStartTick = tick;
                    phase = 2;
                    return;
                }
                if (phase == 2 && tick >= phaseStartTick + 2)
                {
                    QueueKeys(Key.D);
                    phaseStartTick = tick;
                    phase = 3;
                    return;
                }
                if (phase != 3)
                    return;

                report.endX = actor.Runtime.XInt;
                if (tick - phaseStartTick > 150)
                    throw new InvalidOperationException(
                        "Physical double-tap D did not produce both declared running cues in 150 ticks.");
                if (!recorder.HasBothRunningCues)
                    return;

                report.events.AddRange(recorder.Events);
                report.endTick = tick;
                report.finalDispatchedCount =
                    driver.DispatchedSoundEventCountForDiagnostics;
                report.finalPooledPlayCount =
                    soundPlayer.PooledOneShotPlayCountForDiagnostics;
                report.finalRejectedCueCount =
                    soundPlayer.RejectedUnpreparedCueCountForDiagnostics;
                Check(report.endX != report.startX,
                    "Physical movement did not move Naruto.");
                Check(recorder.RunningCuesOnDistinctAcceptedTicks,
                    "Running cues were not published on distinct matching ticks.");
                Check(recorder.RunningCuesStartedPreparedVoices,
                    "A selected running cue did not start its prepared pooled voice.");
                Check(report.finalRejectedCueCount == report.initialRejectedCueCount,
                    "The selected battle rejected an unprepared cue.");
                Finish(true, string.Empty);
            }
            catch (Exception exception)
            {
                if (recorder != null)
                    report.events.AddRange(recorder.Events);
                Finish(false, exception.ToString());
            }
        }

        private static bool PreparedClipExists(string cue)
        {
            return soundPlayer.TryGetPreparedSingleFileWrapperForDiagnostics(
                       cue, out AudioClip[] clips) &&
                   clips?.Length > 0 && clips[0] != null;
        }

        private static void QueueKeys(params Key[] keys)
        {
            Check(keyboard != null, "Keyboard disappeared during the probe.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
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
                if (driver != null)
                {
                    driver.SetSoundPresentationSinkForDiagnostics(null);
                    if (driver.LifecycleState == BattleRuntimeLifecycleState.Running)
                        driver.SetPaused(previousPaused);
                }
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
                actor = null;
                keyboard = null;
                recorder = null;
                finishing = false;
            }
        }

        private static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private sealed class ForwardingRecorder : ISimulationSoundPresentationSink
        {
            private readonly SimulationTickDriver observedDriver;
            private readonly NTSDSoundPlayer player;
            private readonly int mainThreadId;
            public readonly List<CueRecord> Events = new List<CueRecord>();

            public ForwardingRecorder(
                SimulationTickDriver observedDriver,
                NTSDSoundPlayer player)
            {
                this.observedDriver = observedDriver;
                this.player = player;
                mainThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            public bool HasBothRunningCues =>
                FindFirst("data\\003.wav") != null &&
                FindFirst("data\\004.wav") != null;

            public bool RunningCuesOnDistinctAcceptedTicks
            {
                get
                {
                    CueRecord first = FindFirst("data\\003.wav");
                    CueRecord second = FindFirst("data\\004.wav");
                    return first != null && second != null &&
                           first.eventTick < second.eventTick &&
                           first.eventTick == first.callbackTick &&
                           second.eventTick == second.callbackTick &&
                           first.onMainThread && second.onMainThread &&
                           first.worldEventPresent && second.worldEventPresent;
                }
            }

            public bool RunningCuesStartedPreparedVoices
            {
                get
                {
                    CueRecord first = FindFirst("data\\003.wav");
                    CueRecord second = FindFirst("data\\004.wav");
                    return first != null && second != null &&
                           first.poolCountAfter > first.poolCountBefore &&
                           second.poolCountAfter > second.poolCountBefore &&
                           first.preparedClipAssigned &&
                           second.preparedClipAssigned;
                }
            }

            public void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds)
            {
                var batch = new List<CueRecord>();
                long before = player.PooledOneShotPlayCountForDiagnostics;
                for (int i = 0; i < sounds.Count; i++)
                {
                    PendingSoundEvent sound = sounds[i];
                    if (!IsRunningCue(sound.Cue))
                        continue;
                    bool inWorld = false;
                    IReadOnlyList<PendingSoundEvent> pending =
                        observedDriver.World.PendingSounds;
                    for (int j = 0; j < pending.Count; j++)
                    {
                        if (pending[j].Tick == sound.Tick &&
                            string.Equals(pending[j].Cue, sound.Cue,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            inWorld = true;
                            break;
                        }
                    }
                    batch.Add(new CueRecord
                    {
                        cue = sound.Cue,
                        eventTick = sound.Tick,
                        callbackTick = observedDriver.CurrentTickIndex,
                        eventWorldX = sound.WorldX,
                        actorSourceRuleX = actor.Runtime.SourceRuleXInt,
                        actorPhysicalX = actor.Runtime.XInt,
                        sourceRulePositionInitialized =
                            actor.Runtime.SourceRulePositionInitialized,
                        worldEventPresent = inWorld,
                        onMainThread =
                            Thread.CurrentThread.ManagedThreadId == mainThreadId,
                        poolCountBefore = before,
                    });
                }

                player.PresentSounds(sounds);
                long after = player.PooledOneShotPlayCountForDiagnostics;
                AudioSource[] voices = player.GetComponentsInChildren<AudioSource>(true);
                foreach (CueRecord record in batch)
                {
                    record.poolCountAfter = after;
                    if (player.TryGetPreparedSingleFileWrapperForDiagnostics(
                            record.cue, out AudioClip[] clips) &&
                        clips?.Length > 0 && clips[0] != null)
                    {
                        foreach (AudioSource voice in voices)
                        {
                            if (voice != null && voice.clip == clips[0])
                            {
                                record.preparedClipAssigned = true;
                                record.voiceIsPlaying = voice.isPlaying;
                                record.voiceChannels = voice.clip.channels;
                                record.voiceSpatialBlend = voice.spatialBlend;
                                record.voicePanStereo = voice.panStereo;
                                record.voiceVolume = voice.volume;
                                break;
                            }
                        }
                    }
                    Events.Add(record);
                }
            }

            private CueRecord FindFirst(string cue)
            {
                foreach (CueRecord record in Events)
                {
                    if (string.Equals(record.cue, cue,
                            StringComparison.OrdinalIgnoreCase))
                        return record;
                }
                return null;
            }

            private static bool IsRunningCue(string cue)
            {
                return string.Equals(cue, "data\\003.wav",
                           StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(cue, "data\\004.wav",
                           StringComparison.OrdinalIgnoreCase);
            }
        }

        [Serializable]
        private sealed class CueRecord
        {
            public string cue;
            public int eventTick;
            public int callbackTick;
            public int eventWorldX;
            public int actorSourceRuleX;
            public int actorPhysicalX;
            public bool sourceRulePositionInitialized;
            public bool worldEventPresent;
            public bool onMainThread;
            public long poolCountBefore;
            public long poolCountAfter;
            public bool preparedClipAssigned;
            public bool voiceIsPlaying;
            public int voiceChannels;
            public float voiceSpatialBlend;
            public float voicePanStereo;
            public float voiceVolume;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string initialScene;
            public string contentRoot;
            public bool captureStereoParameters;
            public int startTick;
            public int endTick;
            public int startX;
            public int endX;
            public long initialDispatchedCount;
            public long finalDispatchedCount;
            public long initialPooledPlayCount;
            public long finalPooledPlayCount;
            public long initialRejectedCueCount;
            public long finalRejectedCueCount;
            public bool battleUnloaded;
            public List<CueRecord> events = new List<CueRecord>();
        }
    }
}
#endif
