#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test
{
    [InitializeOnLoad]
    public static class NTSD28Q10SakuraNaturalStereoPlayModeTests
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string MenuPath =
            "NTSD/Battle Diagnostics/Q10/Run Sakura Natural Stereo Voice Probe";
        private const string KimMenuPath =
            "NTSD/Battle Diagnostics/Q10/Run Kimimaro Natural Stereo Voice Probe";
        private const string SessionKey = "NTSD.Q10.SakuraNaturalStereoVoice";
        private const string Cue = @"c\saku\w\tra.wav";
        private const string KimCue = @"c\kim\w\j1.wav";
        private static bool running;

        static NTSD28Q10SakuraNaturalStereoPlayModeTests()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuPath)]
        public static void Run()
        {
            Start("Sakura");
        }

        [MenuItem(KimMenuPath)]
        public static void RunKimimaro()
        {
            Start("Kimimaro");
        }

        private static void Start(string variant)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                !string.IsNullOrEmpty(SessionState.GetString(SessionKey, "")))
                throw new InvalidOperationException("Sakura probe is already active.");
            Require(SceneManager.GetActiveScene().path == BattleScene &&
                    !SceneManager.GetActiveScene().isDirty,
                "Original saved Battle Scene must be active and clean.");

            var report = new Report
            {
                runId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"),
                status = "STARTING",
                variant = variant,
            };
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
            Save(report);
            try
            {
                EditorSceneManager.OpenScene(MenuScene);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.error = "Start: " + exception;
                Save(report);
                SessionState.EraseString(SessionKey);
                if (!SceneManager.GetActiveScene().isDirty)
                    EditorSceneManager.OpenScene(BattleScene);
                throw;
            }
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || running)
                return;
            string saved = SessionState.GetString(SessionKey, "");
            if (string.IsNullOrEmpty(saved))
                return;
            Report report = JsonUtility.FromJson<Report>(saved);
            if (report == null || report.status != "STARTING")
                return;
            running = true;
            report.status = "RUNNING";
            Save(report);
            RunInPlayAsync(report).Forget();
        }

        private static async UniTaskVoid RunInPlayAsync(Report report)
        {
            SimulationTickDriver driver = null;
            NTSDSoundPlayer soundPlayer = null;
            Keyboard keyboard = null;
            bool wasPaused = false;
            bool kimimaro = report.variant == "Kimimaro";
            string expectedCue = kimimaro ? KimCue : Cue;
            try
            {
                await UniTask.WaitUntil(() =>
                    UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true) != null &&
                    AppManager.Instance != null).Timeout(TimeSpan.FromSeconds(120));
                LoadingPrewarmController loading =
                    UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true);
                await loading.PrewarmOnceAsync().Timeout(TimeSpan.FromSeconds(180));

                var match = new MatchConfig
                {
                    seed = 682973786,
                    gameMode = new GameModeConfig { battleGameModeId = 0 },
                };
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = true, characterId = kimimaro ? 507 : 1,
                    team = 1, inputId = 1,
                });
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = true, characterId = 7,
                    team = 2, inputId = 2,
                });
                AppManager app = AppManager.Instance;
                Require(app != null, "Menu AppManager is unavailable.");
                app.SetMatchConfig(match);
                AsyncOperation load = app.LoadBattleAdditive();
                Require(load != null, "Additive Battle load was refused.");
                await load.ToUniTask().Timeout(TimeSpan.FromSeconds(180));

                await UniTask.WaitUntil(() =>
                {
                    driver = SimulationTickDriver.Instance;
                    return driver?.LifecycleState == BattleRuntimeLifecycleState.Running &&
                           driver.World?.RuntimeDataCatalog?.IsReady == true &&
                           driver.CurrentTickIndex >= 2;
                }).Timeout(TimeSpan.FromSeconds(120));
                soundPlayer = app.SoundPlayer;
                keyboard = Keyboard.current;
                Require(soundPlayer != null && keyboard != null,
                    "Battle sound player or physical keyboard is unavailable.");
                Require(soundPlayer.BattleCatalogSealedForDiagnostics,
                    "Battle audio catalog was not sealed.");
                report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
                Require(report.contentRoot?.Replace('\\', '/').EndsWith(
                    "Assets/NTSD/Content/LoganRuntime", StringComparison.Ordinal) == true,
                    "Production battle content root is not LoganRuntime.");
                wasPaused = driver.IsPaused;
                driver.SetPaused(true);

                SimulationWorld world = driver.World;
                LF2Character actor = world.FindEntityByRuntimeSlotForQuery(0) as LF2Character;
                LF2Character lee = world.FindEntityByRuntimeSlotForQuery(1) as LF2Character;
                Require(actor?.ObjectId == (kimimaro ? 507 : 1) && lee?.ObjectId == 7,
                    "Expected selected formal P1 and Lee P2.");
                Queue(keyboard);
                for (int neutralTick = 1; neutralTick <= 100; neutralTick++)
                {
                    Require(driver.StepOneTick(ignorePaused: true,
                        buildPresentation: false),
                        "Production Driver refused a neutral preparation tick.");
                    if (neutralTick < 30 || actor.Frame.N != 0 ||
                        world.InputPhase != 0)
                        continue;
                    report.neutralPreparationTicks = neutralTick;
                    break;
                }
                Require(report.neutralPreparationTicks > 0,
                    "Neutral physical input did not return P1 to action0/input phase0.");
                report.initialTick = driver.CurrentTickIndex;
                report.inputPhase = world.InputPhase;
                PlaceAtSource(world, actor, 500, 650);
                PlaceAtSource(world, lee, 1200, 650);
                actor.Health.HP = kimimaro ? 500 : 100;
                actor.Health.HPBound = kimimaro ? 500 : 100;
                actor.Health.HP3 = kimimaro ? 500 : 100;
                actor.Runtime.MP = 500;
                actor.Health.PP = 500;
                actor.RefreshRuntimeSnapshot();
                lee.RefreshRuntimeSnapshot();
                Require(actor.TryGetSharedInputControllerForSimulation(
                    out ILF2Controller sharedController),
                    "Selected P1 has no shared input controller.");
                ILocalFrameInputSource localInput =
                    sharedController as ILocalFrameInputSource;
                Require(localInput != null,
                    "Selected P1 controller has no local frame input source.");
                CharacterInputModule physicalInput =
                    sharedController as CharacterInputModule;
                report.inputControllerType = sharedController.GetType().FullName;
                Require(physicalInput?.MoveAction?.enabled == true &&
                        physicalInput.AttackAction?.enabled == true &&
                        physicalInput.JumpAction?.enabled == true &&
                        physicalInput.DefendAction?.enabled == true &&
                        physicalInput.DefendAction.actionMap?.name == "Player_1",
                    "Selected P1 physical action map is unavailable.");
                Require(PreparedFormalClip(soundPlayer, expectedCue) != null,
                    "Production battle prewarm omitted selected formal stereo cue.");

                var sink = new ForwardingSink(driver, soundPlayer, report, expectedCue);
                Queue(keyboard);
                driver.SetSoundPresentationSinkForDiagnostics(sink);
                for (int relativeTick = 1; relativeTick <= 70; relativeTick++)
                {
                    int expectedButtons;
                    if (relativeTick <= 2)
                    {
                        Queue(keyboard, Key.L);
                        expectedButtons = (int)SimulationInputButtons.Attack;
                    }
                    else if (relativeTick <= 4)
                    {
                        Queue(keyboard, Key.W);
                        expectedButtons = (int)SimulationInputButtons.Up;
                    }
                    else if (relativeTick <= 6)
                    {
                        Queue(keyboard, kimimaro ? Key.K : Key.J);
                        expectedButtons = kimimaro
                            ? (int)SimulationInputButtons.Defend
                            : (int)SimulationInputButtons.Jump;
                    }
                    else if (!kimimaro && report.action172Tick > 0 &&
                             relativeTick == report.action172Tick + 1)
                    {
                        Queue(keyboard, Key.K);
                        expectedButtons = (int)SimulationInputButtons.Defend;
                    }
                    else
                    {
                        Queue(keyboard);
                        expectedButtons = (int)SimulationInputButtons.None;
                    }

                    try
                    {
                        await UniTask.WaitUntil(() =>
                            (int)localInput.CaptureHeldSimulationButtons() ==
                            expectedButtons).Timeout(TimeSpan.FromSeconds(5));
                    }
                    catch (TimeoutException)
                    {
                        report.waitRelativeTick = relativeTick;
                        report.waitExpectedButtons = expectedButtons;
                        report.waitHeldButtons =
                            (int)localInput.CaptureHeldSimulationButtons();
                        report.waitKeyboardL = keyboard.lKey.isPressed;
                        report.waitKeyboardW = keyboard.wKey.isPressed;
                        report.waitKeyboardK = keyboard.kKey.isPressed;
                        report.waitDefendAction =
                            physicalInput.DefendAction.IsPressed();
                        report.waitJumpAction =
                            physicalInput.JumpAction.IsPressed();
                        throw;
                    }

                    var row = new TickRow
                    {
                        relativeTick = relativeTick,
                        keyboardLBeforeStep = keyboard.lKey.isPressed,
                        defendActionBeforeStep =
                            physicalInput?.DefendAction?.IsPressed() == true,
                        heldButtonsBeforeStep =
                            (int)localInput.CaptureHeldSimulationButtons(),
                    };
                    Require(driver.StepOneTick(ignorePaused: true,
                        buildPresentation: false), "Production Driver refused a tick.");
                    row.action = actor.Frame.N;
                    row.hp = actor.Health.HP;
                    row.mp = actor.Runtime.MP;
                    row.pp = actor.Health.PP;
                    row.sourceX = actor.Runtime.SourceRuleXInt;
                    row.sourceZ = actor.Runtime.SourceRuleZInt;
                    row.appliedButtons = AppliedP1Buttons(driver.LastAppliedFrameInput);
                    row.heldButtonsAfterStep =
                        (int)localInput.CaptureHeldSimulationButtons();
                    report.ticks.Add(row);
                    Require(row.appliedButtons == expectedButtons,
                        $"Physical P1 key mapping differs at tick {relativeTick}: " +
                        $"expected bits {expectedButtons}, applied {row.appliedButtons}.");
                    if (row.action == 172 && report.action172Tick == 0)
                        report.action172Tick = relativeTick;
                    if (sink.matched)
                        break;
                }

                if (kimimaro)
                {
                    Require(report.ticks.Count >= 6 &&
                            report.ticks[1].action == 110 &&
                            report.ticks[5].action == 311 &&
                            report.ticks[5].pp == 250,
                        "Physical L/W/K sequence did not reach formal action311/current PP250.");
                }
                else
                {
                    Require(report.action172Tick > 0,
                        "Physical L/W/J sequence did not reach Sakura action172.");
                }
                Require(sink.matched,
                    "Physical sequence did not publish selected formal cue.");
                Require(report.worldPendingMatched,
                    "Published cue was not present in World pending sounds.");
                Require(report.eventTick == report.callbackTick,
                    "Cue callback tick differs from event tick.");
                if (kimimaro)
                    Require(report.eventTick == report.initialTick + 6,
                        "Kimimaro formal cue was not published at relative tick6.");
                Require(report.actionAtEvent == (kimimaro ? 311 : 340),
                    "Formal cue did not occur at the selected tick-end action.");
                Require(report.clipChannels == 2 &&
                        report.clipSamples == (kimimaro ? 80454 : 123466),
                    "Prepared battle clip lost formal stereo/frame count.");
                Require(report.poolAfter > report.poolBefore &&
                        report.voiceAssigned && report.voicePlaying,
                    "Natural cue did not start its prepared pooled voice.");
                report.status = "PASS";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.error = exception.ToString();
            }
            finally
            {
                try
                {
                    if (keyboard != null)
                        Queue(keyboard);
                    if (driver != null)
                    {
                        driver.SetSoundPresentationSinkForDiagnostics(null);
                        if (driver.LifecycleState == BattleRuntimeLifecycleState.Running)
                            driver.SetPaused(wasPaused);
                    }
                }
                catch (Exception exception)
                {
                    report.status = "FAIL";
                    report.error += "\nCleanup: " + exception;
                }
                Save(report);
                SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
                if (EditorApplication.isPlaying)
                    EditorApplication.ExitPlaymode();
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            string saved = SessionState.GetString(SessionKey, "");
            if (string.IsNullOrEmpty(saved))
                return;
            Report report = JsonUtility.FromJson<Report>(saved);
            try
            {
                Require(!SceneManager.GetActiveScene().isDirty,
                    "Post-Play Editor Scene is dirty; preserving it.");
                if (SceneManager.GetActiveScene().path != BattleScene)
                    EditorSceneManager.OpenScene(BattleScene);
                report.sceneRestored = true;
                if (report.status == "STARTING" || report.status == "RUNNING")
                {
                    report.status = "FAIL";
                    report.error = "Play exited before selected stereo probe completed.";
                }
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.error += "\nRestore: " + exception;
            }
            finally
            {
                Save(report);
                SessionState.EraseString(SessionKey);
                running = false;
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
        private static void PlaceAtSource(
            SimulationWorld world, LF2Character character, int x, int z)
        {
            character.Runtime.SetPosition(
                world.SpatialProjection.SourceToViewX(x), 0,
                world.SpatialProjection.SourceToViewZ(z));
            character.Runtime.SyncIntegerPosition();
            character.Runtime.SetSourceRulePosition(x, z);
            character.Runtime.SyncSourceRuleIntegerPosition();
        }

        private static int AppliedP1Buttons(FrameInputSet input)
        {
            if (input?.Players == null)
                return -1;
            foreach (SimulationPlayerInput player in input.Players)
            {
                if (player.PlayerSlot == 0)
                    return (int)player.Buttons;
            }
            return -1;
        }

        private static AudioClip PreparedFormalClip(
            NTSDSoundPlayer soundPlayer, string cue)
        {
            MethodInfo getCue = typeof(NTSDSoundPlayer).GetMethod(
                "GetOrPrepareCue", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(getCue, Is.Not.Null);
            object prepared = getCue.Invoke(soundPlayer, new object[] { cue, true });
            Assert.That(prepared, Is.Not.Null);
            FieldInfo clipsField = prepared.GetType().GetField("Clips");
            Assert.That(clipsField, Is.Not.Null);
            AudioClip[] clips = clipsField.GetValue(prepared) as AudioClip[];
            return clips?.Length > 0 ? clips[0] : null;
        }

        private static void Queue(Keyboard keyboard, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private static void Save(Report report)
        {
            string diagnosticId = report.variant == "Kimimaro"
                ? "NTSD28-336B44-Q10-KIM-NATURAL-VOICE-001"
                : "NTSD28-336B44-Q10-SAKURA-NATURAL-VOICE-001";
            string root = Path.GetFullPath(Path.Combine(Application.dataPath,
                "..", "artifacts", "diagnostics", diagnosticId));
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "play-" + report.runId + ".json"),
                JsonUtility.ToJson(report, true));
        }

        private sealed class ForwardingSink : ISimulationSoundPresentationSink
        {
            private readonly SimulationTickDriver driver;
            private readonly NTSDSoundPlayer soundPlayer;
            private readonly Report report;
            private readonly string cue;
            public bool matched;

            public ForwardingSink(
                SimulationTickDriver driver, NTSDSoundPlayer soundPlayer,
                Report report, string cue)
            {
                this.driver = driver;
                this.soundPlayer = soundPlayer;
                this.report = report;
                this.cue = cue;
            }

            public void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds)
            {
                foreach (PendingSoundEvent sound in sounds)
                {
                    if (sound.Cue == null ||
                        !string.Equals(sound.Cue.Replace('\\', '/'),
                            cue.Replace('\\', '/'),
                            StringComparison.OrdinalIgnoreCase))
                        continue;
                    matched = true;
                    report.cue = sound.Cue;
                    report.eventTick = sound.Tick;
                    report.callbackTick = driver.CurrentTickIndex;
                    report.actionAtEvent =
                        (driver.World.FindEntityByRuntimeSlotForQuery(0)
                            as LF2Character)?.Frame.N ?? -1;
                    report.poolBefore = soundPlayer.PooledOneShotPlayCountForDiagnostics;
                    foreach (PendingSoundEvent pending in driver.World.PendingSounds)
                    {
                        if (pending.Tick == sound.Tick &&
                            string.Equals(pending.Cue, sound.Cue,
                                StringComparison.OrdinalIgnoreCase))
                            report.worldPendingMatched = true;
                    }
                }

                soundPlayer.PresentSounds(sounds);
                if (!matched)
                    return;
                report.poolAfter = soundPlayer.PooledOneShotPlayCountForDiagnostics;
                AudioClip clip = PreparedFormalClip(soundPlayer, cue);
                if (clip == null)
                    return;
                report.clipChannels = clip.channels;
                report.clipSamples = clip.samples;
                foreach (AudioSource voice in
                         soundPlayer.GetComponentsInChildren<AudioSource>(true))
                {
                    if (voice == null || voice.clip != clip)
                        continue;
                    report.voiceAssigned = true;
                    report.voicePlaying = voice.isPlaying;
                    report.voicePan = voice.panStereo;
                    report.voiceVolume = voice.volume;
                    break;
                }
            }
        }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int action;
            public int hp;
            public int mp;
            public int pp;
            public int sourceX;
            public int sourceZ;
            public int appliedButtons;
            public bool keyboardLBeforeStep;
            public bool defendActionBeforeStep;
            public int heldButtonsBeforeStep;
            public int heldButtonsAfterStep;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string variant;
            public string status;
            public string error;
            public string contentRoot;
            public string inputControllerType;
            public bool sceneRestored;
            public int initialTick;
            public int neutralPreparationTicks;
            public int inputPhase;
            public int action172Tick;
            public string cue;
            public int eventTick;
            public int callbackTick;
            public int actionAtEvent;
            public bool worldPendingMatched;
            public long poolBefore;
            public long poolAfter;
            public int clipChannels;
            public int clipSamples;
            public int waitRelativeTick;
            public int waitExpectedButtons;
            public int waitHeldButtons;
            public bool waitKeyboardL;
            public bool waitKeyboardW;
            public bool waitKeyboardK;
            public bool waitDefendAction;
            public bool waitJumpAction;
            public bool voiceAssigned;
            public bool voicePlaying;
            public float voicePan;
            public float voiceVolume;
            public List<TickRow> ticks = new List<TickRow>();
        }
    }
}
#endif
