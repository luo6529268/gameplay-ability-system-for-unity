#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q10Orasengan078NaturalVoiceProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string GameConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ModeAsset = "Assets/NTSD/Resources/ProjectBattleModeConfig.asset";
        private const string FormalWav = "Assets/NTSD/Content/LoganRuntime/vfs/data/078.wav";
        private const string LegacyWav = "Assets/NTSD/Sound/data/078.wav";
        private const string ResultPath =
            "artifacts/diagnostics/NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001/naruto-078-scene-01.json";
        private const string ResidualPath =
            "artifacts/diagnostics/NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001/naruto-078-postplay-01.json";
        private const string SessionKey = "NTSD.Q10.Orasengan078NaturalVoice.01";
        private const string MenuPath = "NTSD/Validation/Q10/Naruto 078 Natural Voice Scene Probe";
        private const string ResidualMenuPath =
            "NTSD/Validation/Q10/Naruto 078 Post Play Residual";
        private const int TargetTicks = 55;

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character naruto;
        private static LF2Character opponent;
        private static NTSDSoundPlayer soundPlayer;
        private static AudioClip formalClip;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class TickRow
        {
            public int tick;
            public int globalTick;
            public int submittedLegacyButtons;
            public int inputPhase;
            public int narutoAction;
            public int narutoPp;
            public int narutoRuntimeMp;
            public int combo1;
            public int opponentAction;
            public int pending078Count;
            public int pendingSoundCount;
            public string pendingSounds;
            public long pooledPlayDelta;
            public int formalVoiceCount;
            public int formalPlayingVoiceCount;
            public uint crtState;
            public ulong crtCalls;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId = "naruto-078-scene-01";
            public string status = "RUNNING";
            public string phase = "STARTUP";
            public string error = string.Empty;
            public string startedUtc;
            public string contentRoot;
            public string formalSourcePath;
            public string legacySourcePath;
            public int formalClipSamples;
            public int formalClipChannels;
            public int formalClipFrequency;
            public int legacyClipSamples;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public int startTick;
            public int endTick;
            public int first078PendingTick = -1;
            public int first078VoiceTick = -1;
            public string battleHashBefore;
            public string menuHashBefore;
            public string gameConfigHashBefore;
            public string modeHashBefore;
            public string formalWavHashBefore;
            public string legacyWavHashBefore;
            public string battleHashAfter;
            public string menuHashAfter;
            public string gameConfigHashAfter;
            public string modeHashAfter;
            public string formalWavHashAfter;
            public string legacyWavHashAfter;
            public List<TickRow> ticks = new List<TickRow>();
        }

        [Serializable]
        private sealed class ResidualReport
        {
            public string status;
            public string sceneHashInPlayResult;
            public string sceneHashNow;
            public bool sceneDirty;
            public int sceneRootCount;
            public int sceneDriverCount;
            public int sceneDriverWithWorldCount;
            public int scenePoolCount;
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [MenuItem(MenuPath)]
        private static void StartFromMenu()
        {
            Restore();
            Require(report == null &&
                string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)) &&
                !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "The original Editor must be idle with no Q10 probe running.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                    "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                    StringComparison.OrdinalIgnoreCase), "This probe requires the original project.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Expected one saved original Battle Scene.");
            Require(!File.Exists(ProjectPath(ResultPath)), "Refusing to overwrite Q10 result.");
            report = new Report
            {
                startedUtc = DateTime.UtcNow.ToString("O"),
                battleHashBefore = Hash(BattleScene),
                menuHashBefore = Hash(MenuScene),
                gameConfigHashBefore = Hash(GameConfigAsset),
                modeHashBefore = Hash(ModeAsset),
                formalWavHashBefore = Hash(FormalWav),
                legacyWavHashBefore = Hash(LegacyWav)
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        [MenuItem(ResidualMenuPath)]
        private static void CheckPostPlayResidual()
        {
            Restore();
            Require(report == null && !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Residual check requires an idle EditMode Editor.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && SceneManager.sceneCount == 1,
                "Residual check requires the original Battle Scene.");
            string priorPath = ProjectPath(ResultPath);
            string resultPath = ProjectPath(ResidualPath);
            Require(File.Exists(priorPath) && !File.Exists(resultPath),
                "Prior result missing or residual result already exists.");
            Report prior = JsonUtility.FromJson<Report>(File.ReadAllText(priorPath));
            Require(prior != null && prior.phase == "DONE" && prior.exitedPlay,
                "Prior Q10 Play result is incomplete.");
            SimulationTickDriver[] drivers =
                Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                    .Where(value => value != null && !EditorUtility.IsPersistent(value) &&
                        value.gameObject.scene == scene).ToArray();
            LF2ObjectPool[] pools = Resources.FindObjectsOfTypeAll<LF2ObjectPool>()
                .Where(value => value != null && !EditorUtility.IsPersistent(value) &&
                    value.gameObject.scene == scene).ToArray();
            var residual = new ResidualReport
            {
                sceneHashInPlayResult = prior.battleHashAfter,
                sceneHashNow = Hash(BattleScene),
                sceneDirty = scene.isDirty,
                sceneRootCount = scene.rootCount,
                sceneDriverCount = drivers.Length,
                sceneDriverWithWorldCount = drivers.Count(value => value.World != null),
                scenePoolCount = pools.Length
            };
            residual.status = prior.sceneCleanAfter && !residual.sceneDirty &&
                residual.sceneHashInPlayResult == residual.sceneHashNow &&
                residual.sceneDriverCount == 1 && residual.sceneDriverWithWorldCount == 0 &&
                residual.scenePoolCount == 0 ? "SCOPED_PASS" : "INCONCLUSIVE";
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            using (var stream = new FileStream(resultPath, FileMode.CreateNew,
                       FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(residual, true));
        }

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string Hash(string relative)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(ProjectPath(relative)))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void SaveSession() =>
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));

        private static void Restore()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null || report.phase != "STARTUP" ||
                !EditorApplication.isPlaying || report.configuredBeforeStart ||
                scene.path != BattleScene) return;
            try
            {
                BattleTestBootstrap[] bootstraps =
                    Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                        .Where(value => value != null && value.isActiveAndEnabled &&
                            value.gameObject.scene == scene && !EditorUtility.IsPersistent(value))
                        .ToArray();
                Require(bootstraps.Length == 1, "Expected one active BattleTestBootstrap.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField(
                    "overrideCharacterIds", BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap roster override is unavailable.");
                field.SetValue(bootstraps[0], new[] { 2, 7 });
                report.configuredBeforeStart = true;
                SaveSession();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null) return;
            if (state == PlayModeStateChange.EnteredPlayMode &&
                report.phase == "STARTUP" && !report.configuredBeforeStart)
                Fail("Play clone was not configured before bootstrap Start.");
            if (state == PlayModeStateChange.ExitingPlayMode &&
                report.phase != "EXITING")
            {
                report.status = "INTERRUPTED";
                report.error = "Play ended before the Q10 probe completed.";
                report.phase = "EXITING";
                SaveSession();
            }
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
                Finish();
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            Restore();
            if (report == null) return;
            try
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                    report.phase != "EXITING" &&
                    DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() >
                    TimeSpan.FromSeconds(10))
                {
                    report.status = "INTERRUPTED";
                    report.error = "Play ended without completing the Q10 probe.";
                    report.phase = "EXITING";
                    SaveSession();
                }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "Naruto 078 Scene probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected Q10 probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart,
                "Play clone was not configured before bootstrap Start.");
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 3) return;
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity first) &&
                first is LF2Character && first.ObjectId == 2,
                "Naruto OID2 roster slot0 is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character && second.ObjectId == 7,
                "OID7 roster slot1 is missing.");
            naruto = (LF2Character)first;
            opponent = (LF2Character)second;
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use formal battle content.");
            SetInitialCharacter(naruto, 500, 650, 1);
            SetInitialCharacter(opponent, 1200, 650, 2);
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null,
                "Native world clock is unavailable.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            Require(world.BattleGameModeId == 0 && world.Difficulty == 0,
                "Battle mode or difficulty differs from formal mode0.");
            world.NativeRandom.ResetFromSeed(682973786u);
            soundPlayer = UnityEngine.Object.FindObjectOfType<NTSDSoundPlayer>();
            Require(soundPlayer != null && soundPlayer.BattleCatalogSealedForDiagnostics,
                "Production battle sound catalog is not ready.");
            formalClip = CaptureCue(true, out string formalPath);
            AudioClip legacyClip = CaptureCue(false, out string legacyPath);
            report.formalSourcePath = formalPath;
            report.legacySourcePath = legacyPath;
            report.formalClipSamples = formalClip.samples;
            report.formalClipChannels = formalClip.channels;
            report.formalClipFrequency = formalClip.frequency;
            report.legacyClipSamples = legacyClip.samples;
            Require(formalPath.Replace('\\', '/').Contains(
                    "/NTSD/Content/LoganRuntime/vfs/data/078.wav") &&
                legacyPath.Replace('\\', '/').Contains("/NTSD/Sound/data/078.wav") &&
                formalClip.samples == 54104 && formalClip.channels == 1 &&
                formalClip.frequency == 22050,
                "Formal battle or legacy nonbattle 078 cue binding differs.");
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            SaveSession();
        }

        private static void SetInitialCharacter(LF2Character character, int sourceX,
            int sourceZ, int team)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(0);
            character.Runtime.MP = character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.Runtime.HP2Orig = 1;
            character.Runtime.RespawnCount = 0;
            character.HitStun = 0;
            character.AttackExempt = 0;
            character.ItrRest.Reset();
            character.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(sourceZ));
            AppManager.SyncParticipantBirthPosition(character, sourceX, sourceZ);
            character.RelationTeam = team;
            world.Runtime.Roster.Slots[team - 1].Team = team;
            Require(character.Frame.N == 0 &&
                character.Runtime.SourceRuleXInt == sourceX &&
                character.Runtime.YInt == 0 &&
                character.Runtime.SourceRuleZInt == sourceZ,
                "Initial action or source-rule position differs.");
        }

        private static AudioClip CaptureCue(bool battle, out string sourcePath)
        {
            MethodInfo getCue = typeof(NTSDSoundPlayer).GetMethod(
                "GetOrPrepareCue", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(getCue != null, "Common sound cue resolver is unavailable.");
            object cue = getCue.Invoke(soundPlayer, new object[] { @"data\078.wav", battle });
            Require(cue != null, "078 cue was not prepared.");
            Type type = cue.GetType();
            sourcePath = (string)type.GetField("SourcePath").GetValue(cue);
            AudioClip[] clips = (AudioClip[])type.GetField("Clips").GetValue(cue);
            Require(clips != null && clips.Length == 1 && clips[0] != null,
                "078 cue AudioClip is missing after battle prewarm.");
            return clips[0];
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                driver.CurrentTickIndex == report.endTick,
                "Production World or paused tick boundary changed.");
            if (report.ticks.Count == TargetTicks)
            {
                report.status = "MEASURED_COMPARE_PENDING";
                report.phase = "EXITING";
                SaveSession();
                EditorApplication.ExitPlaymode();
                return;
            }

            int tick = report.ticks.Count + 1;
            int next = driver.CurrentTickIndex + 1;
            // Alignment contract: NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001.
            // Formal defend/right/jump enter the legacy packet as Attack/Right/Defend.
            SimulationInputButtons buttons = tick <= 2
                ? SimulationInputButtons.Attack
                : tick <= 4
                    ? SimulationInputButtons.Right
                    : tick <= 6 || tick == 34 || tick == 35
                        ? SimulationInputButtons.Defend
                        : SimulationInputButtons.None;
            long playedBefore = soundPlayer.PooledOneShotPlayCountForDiagnostics;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, buttons),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            var pending = world.PendingSounds.Select(value =>
                value.Cue.Replace('\\', '/') + "@" + value.WorldX + "@" + value.Tick)
                .ToArray();
            int pending078 = world.PendingSounds.Count(value =>
                value.Cue.Replace('\\', '/') == "data/078.wav");
            AudioSource[] voices = soundPlayer.GetComponentsInChildren<AudioSource>(true);
            int formalVoiceCount = voices.Count(value => value != null &&
                value.clip == formalClip);
            int playingVoiceCount = voices.Count(value => value != null &&
                value.clip == formalClip && value.isPlaying);
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var row = new TickRow
            {
                tick = tick,
                globalTick = driver.CurrentTickIndex,
                submittedLegacyButtons = (int)buttons,
                inputPhase = world.InputPhase,
                narutoAction = naruto.Frame.N,
                narutoPp = naruto.Health?.PP ?? -1,
                narutoRuntimeMp = naruto.Runtime.MP,
                combo1 = naruto.Runtime.NativeInputProxy.ComboState[1],
                opponentAction = opponent.Frame.N,
                pending078Count = pending078,
                pendingSoundCount = pending.Length,
                pendingSounds = string.Join(";", pending),
                pooledPlayDelta = soundPlayer.PooledOneShotPlayCountForDiagnostics - playedBefore,
                formalVoiceCount = formalVoiceCount,
                formalPlayingVoiceCount = playingVoiceCount,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls
            };
            if (pending078 > 0 && report.first078PendingTick < 0)
                report.first078PendingTick = tick;
            if (playingVoiceCount > 0 && report.first078VoiceTick < 0)
                report.first078VoiceTick = tick;
            report.ticks.Add(row);
            report.endTick = driver.CurrentTickIndex;
            SaveSession();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q10 Naruto 078 Scene] " + message);
                return;
            }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            SaveSession();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = true;
            report.battleHashAfter = Hash(BattleScene);
            report.menuHashAfter = Hash(MenuScene);
            report.gameConfigHashAfter = Hash(GameConfigAsset);
            report.modeHashAfter = Hash(ModeAsset);
            report.formalWavHashAfter = Hash(FormalWav);
            report.legacyWavHashAfter = Hash(LegacyWav);
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                SceneManager.sceneCount == 1 &&
                report.battleHashAfter == report.battleHashBefore &&
                report.menuHashAfter == report.menuHashBefore &&
                report.gameConfigHashAfter == report.gameConfigHashBefore &&
                report.modeHashAfter == report.modeHashBefore &&
                report.formalWavHashAfter == report.formalWavHashBefore &&
                report.legacyWavHashAfter == report.legacyWavHashBefore;
            if (!report.sceneCleanAfter && report.status == "MEASURED_COMPARE_PENDING")
                report.status = "MEASURED_SCENE_CHANGED";
            report.phase = "DONE";
            string resultPath = ProjectPath(ResultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            using (var stream = new FileStream(resultPath, FileMode.CreateNew,
                       FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(report, true));
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            naruto = opponent = null;
            soundPlayer = null;
            formalClip = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
