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
    internal static class NTSD28BattleAudioSceneProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string GameConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ModeAsset = "Assets/NTSD/Resources/ProjectBattleModeConfig.asset";
        private const string FormalP3Wav = "Assets/NTSD/Content/LoganRuntime/vfs/c/nar/w/p3.wav";
        private const string FormalA7Wav = "Assets/NTSD/Content/LoganRuntime/vfs/c/nar/w/a7.wav";
        private const string ResultPath =
            "artifacts/diagnostics/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006/battle-audio-scene-03.json";
        private const string SessionKey = "NTSD28.BattleAudio.SceneProbe.03";
        private const string MenuPath = "NTSD/Validation/Audio/Battle Audio Alignment Scene Probe";
        private const int NaturalTickCount = 55;
        private const int CollisionTickLimit = 12;
        private const double StartupTimeoutSeconds = 600.0;
        private const double TotalTimeoutSeconds = 720.0;
        private const float NormalLogicInterval = 0.033f;

        private static readonly int[] ControlledActions = { 180, 213 };
        private static readonly string[] ControlledCues =
        {
            @"c\nar\w\p3.wav",
            @"c\nar\w\a7.wav",
        };
        private const string NaturalSkillCue = @"data\078.wav";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character naruto;
        private static LF2Character lee;
        private static NTSDSoundPlayer soundPlayer;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static bool shutdownAttempted;
        private static MethodInfo getOrPrepareCue;
        private static FieldInfo cueSourcePath;
        private static FieldInfo cueClips;
        private static FieldInfo cueIsFormalBattleFile;
        private static readonly Dictionary<string, CueMetadata> CueCache =
            new Dictionary<string, CueMetadata>(StringComparer.OrdinalIgnoreCase);

        [Serializable]
        private sealed class PendingCueRow
        {
            public int order;
            public string cue;
            public int worldX;
            public int tick;
            public bool formalBattleFile;
            public string sourcePath;
            public int clipCount;
            public int clipSamples;
            public int clipChannels;
            public int clipFrequency;
            public int assignedVoiceCount;
            public int playingVoiceCount;
        }

        [Serializable]
        private sealed class VoiceRow
        {
            public string name;
            public bool active;
            public bool isPlaying;
            public string clipName;
            public string sourcePath;
            public int clipSamples = -1;
            public int clipChannels = -1;
            public int clipFrequency = -1;
        }

        [Serializable]
        private sealed class AudioTickRow
        {
            public string stage;
            public int relativeTick;
            public int globalTick;
            public int submittedP1Buttons;
            public int submittedP2Buttons;
            public int narutoAction;
            public int narutoSourceX;
            public int narutoSourceZ;
            public int leeAction;
            public int leeSourceX;
            public int leeSourceZ;
            public int leeHpBefore = -1;
            public int leeHpAfter = -1;
            public int narutoHitCountBefore = -1;
            public int narutoHitCountAfter = -1;
            public int leeHitStun;
            public bool collisionObserved;
            public bool hurtOrBaseSoundObserved;
            public bool collisionHurtSoundWitness;
            public string expectedCue;
            public bool expectedCueQueued;
            public bool expectedVoiceAssigned;
            public bool expectedVoicePlaying;
            public int pendingSoundCount;
            public string pendingCueOrder;
            public long pooledPlayDelta;
            public bool battleCatalogSealed;
            public long rejectedUnpreparedCueCount;
            public long failedPreparedCueLoadCount;
            public long skippedMissingBattleCueFileCount;
            public long voiceLimitDropCount;
            public long dispatchedSoundEventCount;
            public long rejectedPublishedSoundEventCount;
            public int audioSourcePoolCount;
            public int audioSourceAssignedCount;
            public int audioSourcePlayingCount;
            public List<PendingCueRow> pendingCues = new List<PendingCueRow>();
            public List<VoiceRow> voices = new List<VoiceRow>();
        }

        [Serializable]
        private sealed class ControlledFrameRow
        {
            public int action;
            public string expectedCue;
            public int globalTick;
            public bool queued;
            public bool voiceAssigned;
            public bool voicePlaying;
            public long pooledPlayDelta;
            public int clipSamples = -1;
            public int clipChannels = -1;
            public int clipFrequency = -1;
            public string sourcePath;
            public AudioTickRow tick;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId = "battle-audio-scene-03";
            public string status = "RUNNING";
            public string phase = "STARTUP";
            public string error = string.Empty;
            public string startedUtc;
            public bool configuredBeforeStart;
            public int startupHeartbeatIndex;
            public double startupElapsedSeconds;
            public int startupLastTick = -1;
            public string startupLifecycle;
            public bool startupHasWorld;
            public int startupPreparedCueCount;
            public bool startupCatalogSealed;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public string contentRoot;
            public int startTick = -1;
            public int endTick = -1;
            public float activeHostIntervalSeconds;
            public double sourceToViewScaleX;
            public int preparedCueCount;
            public int battleVoiceCount;
            public bool battleCatalogSealed;
            public long initialRejectedUnpreparedCueCount;
            public long initialFailedPreparedCueLoadCount;
            public long initialSkippedMissingBattleCueFileCount;
            public long initialVoiceLimitDropCount;
            public long initialDispatchedSoundEventCount;
            public long initialRejectedPublishedSoundEventCount;
            public string naturalSkillCue = "data/078.wav";
            public bool naturalSkillCueQueued;
            public bool naturalSkillVoiceAssigned;
            public bool naturalSkillVoicePlaying;
            public int naturalSkillCueTick = -1;
            public string naturalSkillSourcePath;
            public int naturalSkillClipSamples = -1;
            public int naturalSkillClipChannels = -1;
            public int naturalSkillClipFrequency = -1;
            public List<AudioTickRow> naturalTicks = new List<AudioTickRow>();
            public List<ControlledFrameRow> controlledFrames = new List<ControlledFrameRow>();
            public List<AudioTickRow> collisionTicks = new List<AudioTickRow>();
            public int controlledFrameIndex;
            public string controlledFrameScope =
                "Controlled only: ImmediateFrame(180/213) one-tick cue/voice witnesses; not natural hurt or dash evidence.";
            public bool collisionConfigured;
            public bool collisionWitness;
            public string collisionStatus = "PENDING";
            public string collisionNote = string.Empty;
            public string battleHashBefore;
            public string menuHashBefore;
            public string gameConfigHashBefore;
            public string modeHashBefore;
            public string formalP3HashBefore;
            public string formalA7HashBefore;
            public string battleHashAfter;
            public string menuHashAfter;
            public string gameConfigHashAfter;
            public string modeHashAfter;
            public string formalP3HashAfter;
            public string formalA7HashAfter;
            public bool shutdownAttempted;
            public bool orderedShutdownComplete;
            public string shutdownStatus;
            public string shutdownStage;
            public string shutdownFailure;
            public bool worldDetached;
            public bool poolPresent;
            public bool poolQuiesced;
            public bool poolAcceptingRequests;
            public int poolActiveBorrowers = -1;
            public int poolActiveSprites = -1;
            public int shutdownRemainingWorldObjects = -1;
            public int shutdownRemainingRuntimeSlots = -1;
            public int shutdownRemainingPoolBorrowers = -1;
            public int audioSourcesAfterShutdown = -1;
            public int playingAudioSourcesAfterShutdown = -1;
            public bool postExitSoundPlayerPresent;
            public int postExitAudioSourceCount = -1;
            public int postExitPlayingAudioSourceCount = -1;
            public bool postExitVoiceCleanup;
        }

        private sealed class CueMetadata
        {
            public string sourcePath;
            public bool formalBattleFile;
            public AudioClip[] clips;
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
                "The original Editor must be idle with no Battle audio scene probe running.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                    "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                    StringComparison.OrdinalIgnoreCase),
                "This probe requires the original Unity GAS project.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Expected one saved original Battle Scene and no additive scenes.");
            Require(!File.Exists(ProjectPath(ResultPath)),
                "Refusing to overwrite the Battle audio scene result.");

            report = new Report
            {
                startedUtc = DateTime.UtcNow.ToString("O"),
                battleHashBefore = Hash(BattleScene),
                menuHashBefore = Hash(MenuScene),
                gameConfigHashBefore = Hash(GameConfigAsset),
                modeHashBefore = Hash(ModeAsset),
                formalP3HashBefore = Hash(FormalP3Wav),
                formalA7HashBefore = Hash(FormalA7Wav),
            };
            shutdownAttempted = false;
            CueCache.Clear();
            SaveSession();
            EditorApplication.EnterPlaymode();
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
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void SaveSession()
        {
            if (report != null)
                SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
        }

        private static void Restore()
        {
            if (report != null)
                return;
            string saved = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null || report.phase != "STARTUP" ||
                !EditorApplication.isPlaying || report.configuredBeforeStart ||
                scene.path != BattleScene)
                return;

            try
            {
                BattleTestBootstrap[] bootstraps =
                    Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                        .Where(value => value != null && value.isActiveAndEnabled &&
                            !EditorUtility.IsPersistent(value) && value.gameObject.scene == scene)
                        .ToArray();
                Require(bootstraps.Length == 1,
                    "Expected exactly one active BattleTestBootstrap in the Play clone.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField(
                    "overrideCharacterIds", BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap roster override is unavailable.");
                field.SetValue(bootstraps[0], new[] { 2, 7 });
                report.configuredBeforeStart = true;
                SaveSession();
            }
            catch (Exception error)
            {
                Fail(error.ToString());
            }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null)
                return;
            if (state == PlayModeStateChange.EnteredPlayMode &&
                report.phase == "STARTUP" && !report.configuredBeforeStart)
            {
                Fail("Play clone was not configured before BattleTestBootstrap.Start.");
            }
            else if (state == PlayModeStateChange.ExitingPlayMode && report.phase != "EXITING")
            {
                report.status = "FAIL";
                report.error = "Play ended before the Battle audio scene probe completed.";
                report.phase = "EXITING";
                SaveSession();
            }
            else if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
            {
                Finish();
            }
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            Restore();
            if (report == null || report.phase == "SHUTDOWN_FAILED")
                return;

            try
            {
                DateTime started = DateTime.Parse(report.startedUtc).ToUniversalTime();
                double elapsed = (DateTime.UtcNow - started).TotalSeconds;
                if (elapsed > TotalTimeoutSeconds)
                    throw new TimeoutException("Battle audio scene probe exceeded the twelve minute deadline.");
                if (report.phase == "STARTUP" && elapsed > StartupTimeoutSeconds)
                    throw new TimeoutException("Battle audio scene probe startup exceeded 600 seconds.");

                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        Finish();
                    return;
                }
                if (!EditorApplication.isPlaying)
                    return;

                switch (report.phase)
                {
                    case "STARTUP":
                        WaitForRoster();
                        break;
                    case "NATURAL":
                        MeasureNaturalTick();
                        break;
                    case "CONTROLLED":
                        MeasureControlledFrame();
                        break;
                    case "COLLISION":
                        MeasureCollisionTick();
                        break;
                    case "SHUTDOWN":
                        CompleteAndExit();
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected probe phase " + report.phase + ".");
                }
            }
            catch (Exception error)
            {
                Fail(error.ToString());
            }
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart,
                "Play clone roster override was not applied before bootstrap.");
            driver = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                .FirstOrDefault(value => value != null && value.isActiveAndEnabled &&
                    !EditorUtility.IsPersistent(value));
            world = driver?.World;
            double elapsed = (DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime()).TotalSeconds;
            if (elapsed >= report.startupHeartbeatIndex * 30.0)
            {
                report.startupHeartbeatIndex++;
                report.startupElapsedSeconds = elapsed;
                report.startupLastTick = driver == null ? -1 : driver.CurrentTickIndex;
                report.startupLifecycle = driver?.LifecycleState.ToString();
                report.startupHasWorld = world != null;
                NTSDSoundPlayer startupPlayer = Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>()
                    .FirstOrDefault(value => value != null && !EditorUtility.IsPersistent(value));
                report.startupPreparedCueCount = startupPlayer?.PreparedCueCountForDiagnostics ?? 0;
                report.startupCatalogSealed = startupPlayer?.BattleCatalogSealedForDiagnostics ?? false;
                SaveSession();
                string progress = ProjectPath(ResultPath.Replace(".json",
                    "-startup-" + report.startupHeartbeatIndex.ToString("00") + ".json"));
                using (var stream = new FileStream(progress, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    writer.Write(JsonUtility.ToJson(report, true));
            }
            if (world == null || driver.CurrentTickIndex < 5)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 3)
                return;

            Require(world.TryResolveRosterInputEntity(0, out LF2Entity first) &&
                first is LF2Character && first.ObjectId == 2,
                "Naruto OID2 is missing from roster slot 0.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character && second.ObjectId == 7,
                "Lee OID7 is missing from roster slot 1.");
            naruto = (LF2Character)first;
            lee = (LF2Character)second;
            soundPlayer = Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>()
                .FirstOrDefault(value => value != null && value.isActiveAndEnabled &&
                    !EditorUtility.IsPersistent(value));
            Require(soundPlayer != null && soundPlayer.BattleCatalogSealedForDiagnostics,
                "Production battle sound catalog is not sealed after prewarm.");
            Require(Mathf.Abs(driver.ActiveHostIntervalSeconds - NormalLogicInterval) < 0.0005f,
                "Probe requires the normal 33 ms production logic interval.");

            GameConfig config = Resources.FindObjectsOfTypeAll<GameConfig>()
                .FirstOrDefault(value => value != null);
            report.contentRoot = config?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use formal battle content.");
            Require(world.Runtime?.NativeWorldClock != null,
                "Formal World native clock is unavailable.");

            SetInitialCharacter(naruto, 500, 650, 1);
            SetInitialCharacter(lee, 1200, 650, 2);
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            Require(world.BattleGameModeId == 0 && world.Difficulty == 0,
                "Battle mode or difficulty differs from the formal mode0 probe contract.");
            world.NativeRandom.ResetFromSeed(682973786u);

            InitializeCueReflection();
            CueMetadata naturalSkill = ResolveBattleCue(NaturalSkillCue);
            RequireCueReady(NaturalSkillCue);
            report.naturalSkillSourcePath = naturalSkill.sourcePath;
            AudioClip naturalClip = naturalSkill.clips.FirstOrDefault(value => value != null);
            if (naturalClip != null)
            {
                report.naturalSkillClipSamples = naturalClip.samples;
                report.naturalSkillClipChannels = naturalClip.channels;
                report.naturalSkillClipFrequency = naturalClip.frequency;
            }
            RequireCueReady(ControlledCues[0]);
            RequireCueReady(ControlledCues[1]);
            report.activeHostIntervalSeconds = driver.ActiveHostIntervalSeconds;
            report.sourceToViewScaleX = world.SpatialProjection.SourceToViewX(1) -
                                        world.SpatialProjection.SourceToViewX(0);
            report.preparedCueCount = soundPlayer.PreparedCueCountForDiagnostics;
            report.battleVoiceCount = soundPlayer.OneShotVoiceCountForDiagnostics;
            report.battleCatalogSealed = soundPlayer.BattleCatalogSealedForDiagnostics;
            report.initialRejectedUnpreparedCueCount =
                soundPlayer.RejectedUnpreparedCueCountForDiagnostics;
            report.initialFailedPreparedCueLoadCount =
                soundPlayer.FailedPreparedCueLoadCountForDiagnostics;
            report.initialSkippedMissingBattleCueFileCount =
                soundPlayer.SkippedMissingBattleCueFileCountForDiagnostics;
            report.initialVoiceLimitDropCount = soundPlayer.OneShotVoiceLimitDropCountForDiagnostics;
            report.initialDispatchedSoundEventCount = driver.DispatchedSoundEventCountForDiagnostics;
            report.initialRejectedPublishedSoundEventCount =
                driver.RejectedPublishedSoundEventCountForDiagnostics;
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "NATURAL";
            SaveSession();
        }

        private static void InitializeCueReflection()
        {
            getOrPrepareCue = typeof(NTSDSoundPlayer).GetMethod(
                "GetOrPrepareCue", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(getOrPrepareCue != null, "Production battle cue resolver is unavailable.");
            Type cueType = getOrPrepareCue.ReturnType;
            cueSourcePath = cueType.GetField("SourcePath", BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            cueClips = cueType.GetField("Clips", BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            cueIsFormalBattleFile = cueType.GetField("IsFormalBattleFile", BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            Require(cueSourcePath != null && cueClips != null && cueIsFormalBattleFile != null,
                "Production battle cue metadata fields are unavailable.");
        }

        private static void RequireCueReady(string cue)
        {
            CueMetadata metadata = ResolveBattleCue(cue);
            Require(metadata != null && metadata.formalBattleFile &&
                metadata.clips != null && metadata.clips.Any(value => value != null),
                "Formal prewarmed cue is unavailable: " + cue);
        }

        private static CueMetadata ResolveBattleCue(string cue)
        {
            string key = NormalizeCue(cue);
            if (CueCache.TryGetValue(key, out CueMetadata cached))
                return cached;
            if (soundPlayer == null || getOrPrepareCue == null)
                return null;

            object prepared = getOrPrepareCue.Invoke(soundPlayer, new object[] { cue, true });
            if (prepared == null)
                return null;
            CueMetadata metadata = new CueMetadata
            {
                sourcePath = cueSourcePath.GetValue(prepared) as string,
                clips = cueClips.GetValue(prepared) as AudioClip[],
                formalBattleFile = (bool)cueIsFormalBattleFile.GetValue(prepared),
            };
            CueCache[key] = metadata;
            return metadata;
        }

        private static void SetInitialCharacter(
            LF2Character character, int sourceX, int sourceZ, int team)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(0);
            character.Health.HP = 500;
            character.Health.HPBound = 500;
            character.Health.HP3 = 500;
            character.Health.HPLost = 0;
            character.Runtime.MP = 500;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.AiControlled = false;
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.Runtime.HP2Orig = 1;
            character.Runtime.RespawnCount = 0;
            character.HitStun = 0;
            character.AttackExempt = 0;
            character.ItrRest.Reset();
            character.Runtime.NativeSoundActionLatch = -1;
            character.Runtime.SetPosition(
                world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(sourceZ));
            AppManager.SyncParticipantBirthPosition(character, sourceX, sourceZ);
            character.RelationTeam = team;
            world.Runtime.Roster.Slots[team - 1].Team = team;
            Require(character.Frame.N == 0 && character.Runtime.SourceRuleXInt == sourceX &&
                character.Runtime.SourceRuleZInt == sourceZ && character.Runtime.HP == 500,
                "Initial character reset did not preserve source coordinates or HP.");
        }

        private static void MeasureNaturalTick()
        {
            RequireProductionBoundary();
            if (report.naturalTicks.Count == NaturalTickCount)
            {
                report.phase = "CONTROLLED";
                SaveSession();
                return;
            }

            int relativeTick = report.naturalTicks.Count + 1;
            SimulationInputButtons buttons = relativeTick <= 2
                ? SimulationInputButtons.Attack
                : relativeTick <= 4
                    ? SimulationInputButtons.Right
                    : relativeTick <= 6 || relativeTick == 34 || relativeTick == 35
                        ? SimulationInputButtons.Defend
                        : SimulationInputButtons.None;
            AudioTickRow row = StepProductionTick(
                "NATURAL_DJA", relativeTick, buttons, SimulationInputButtons.None,
                NaturalSkillCue);
            PendingCueRow naturalSkill = row.pendingCues.FirstOrDefault(value =>
                NormalizeCue(value.cue) == NormalizeCue(NaturalSkillCue));
            if (naturalSkill != null)
            {
                report.naturalSkillCueQueued = true;
                report.naturalSkillVoiceAssigned |= naturalSkill.assignedVoiceCount > 0;
                report.naturalSkillVoicePlaying |= naturalSkill.playingVoiceCount > 0;
                if (report.naturalSkillCueTick < 0)
                    report.naturalSkillCueTick = row.globalTick;
            }
            report.naturalTicks.Add(row);
            report.endTick = row.globalTick;
            SaveSession();
        }

        private static void MeasureControlledFrame()
        {
            RequireProductionBoundary();
            if (report.controlledFrameIndex >= ControlledActions.Length)
            {
                report.phase = "COLLISION";
                SaveSession();
                return;
            }

            int index = report.controlledFrameIndex;
            int action = ControlledActions[index];
            string expectedCue = ControlledCues[index];
            SetInitialCharacter(naruto, 500, 650, 1);
            SetInitialCharacter(lee, 1200, 650, 2);
            naruto.ImmediateFrame(action);
            naruto.Runtime.NativeSoundActionLatch = -1;
            lee.Runtime.NativeSoundActionLatch = -1;
            world.PendingSounds.Clear();
            AudioTickRow row = StepProductionTick(
                "CONTROLLED_FRAME", index + 1, SimulationInputButtons.None,
                SimulationInputButtons.None, expectedCue);
            PendingCueRow expected = row.pendingCues.FirstOrDefault(value =>
                NormalizeCue(value.cue) == NormalizeCue(expectedCue));
            ControlledFrameRow controlled = new ControlledFrameRow
            {
                action = action,
                expectedCue = expectedCue.Replace('\\', '/'),
                globalTick = row.globalTick,
                queued = expected != null,
                voiceAssigned = expected != null && expected.assignedVoiceCount > 0,
                voicePlaying = expected != null && expected.playingVoiceCount > 0,
                pooledPlayDelta = row.pooledPlayDelta,
                tick = row,
            };
            CueMetadata metadata = ResolveBattleCue(expectedCue);
            if (metadata != null)
            {
                controlled.sourcePath = metadata.sourcePath;
                AudioClip clip = metadata.clips?.FirstOrDefault(value => value != null);
                if (clip != null)
                {
                    controlled.clipSamples = clip.samples;
                    controlled.clipChannels = clip.channels;
                    controlled.clipFrequency = clip.frequency;
                }
            }
            report.controlledFrames.Add(controlled);
            report.controlledFrameIndex++;
            report.endTick = row.globalTick;
            Require(controlled.queued && controlled.voiceAssigned && controlled.voicePlaying &&
                controlled.pooledPlayDelta > 0,
                "Controlled frame " + action + " did not queue and play " + expectedCue + ".");
            SaveSession();
        }

        private static void MeasureCollisionTick()
        {
            RequireProductionBoundary();
            if (!report.collisionConfigured)
            {
                SetInitialCharacter(naruto, 500, 650, 1);
                SetInitialCharacter(lee, 548, 650, 2);
                naruto.ImmediateFrame(60);
                naruto.Runtime.NativeSoundActionLatch = -1;
                lee.ImmediateFrame(0);
                lee.Runtime.NativeSoundActionLatch = -1;
                lee.SwitchDir("left");
                world.PendingSounds.Clear();
                report.collisionConfigured = true;
                report.collisionStatus = "RUNNING";
                SaveSession();
                return;
            }

            if (report.collisionTicks.Count >= CollisionTickLimit || report.collisionWitness)
            {
                report.collisionStatus = report.collisionWitness ? "PASS" : "INCONCLUSIVE";
                if (!report.collisionWitness)
                    report.collisionNote =
                        "No real collision plus hurt/base cue was observed in the 12 tick budget; no geometry, threshold, or damage value was changed.";
                report.phase = "SHUTDOWN";
                SaveSession();
                return;
            }

            int beforeHp = lee.Health?.HP ?? lee.Runtime.HP;
            int beforeHitCount = naruto.HitCount;
            AudioTickRow row = StepProductionTick(
                "COLLISION_HURT", report.collisionTicks.Count + 1,
                SimulationInputButtons.None, SimulationInputButtons.None);
            row.leeHpBefore = beforeHp;
            row.leeHpAfter = lee.Health?.HP ?? lee.Runtime.HP;
            row.narutoHitCountBefore = beforeHitCount;
            row.narutoHitCountAfter = naruto.HitCount;
            row.leeHitStun = lee.HitStun;
            row.collisionObserved = row.leeHpAfter < beforeHp ||
                naruto.HitCount > beforeHitCount || lee.HitStun != 0;
            row.hurtOrBaseSoundObserved = row.pendingCues.Any(value => IsHurtOrBaseCue(value.cue));
            row.collisionHurtSoundWitness = row.collisionObserved && row.pooledPlayDelta > 0 &&
                row.pendingCues.Any(value => IsHurtOrBaseCue(value.cue) &&
                    value.assignedVoiceCount > 0 && value.playingVoiceCount > 0);
            report.collisionTicks.Add(row);
            report.collisionWitness |= row.collisionHurtSoundWitness;
            report.endTick = row.globalTick;
            SaveSession();
        }

        private static bool IsHurtOrBaseCue(string cue)
        {
            string normalized = NormalizeCue(cue);
            return normalized == "sfx_001" || normalized == "sfx_002" ||
                normalized == "sfx_006" || normalized == "sfx_032" ||
                normalized == "sfx_033";
        }

        private static AudioTickRow StepProductionTick(
            string stage, int relativeTick, SimulationInputButtons p1,
            SimulationInputButtons p2, string expectedCue = null)
        {
            int nextTick = driver.CurrentTickIndex + 1;
            long playedBefore = soundPlayer.PooledOneShotPlayCountForDiagnostics;
            var input = new FrameInputSet(nextTick, new[]
            {
                new SimulationPlayerInput(0, p1),
                new SimulationPlayerInput(1, p2),
            });
            // Alignment contract: production StepOneTick owns sound publication and voice playback.
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + nextTick + ".");
            AudioTickRow row = CaptureTick(stage, relativeTick, p1, p2, playedBefore, expectedCue);
            return row;
        }

        private static AudioTickRow CaptureTick(
            string stage, int relativeTick, SimulationInputButtons p1,
            SimulationInputButtons p2, long playedBefore, string expectedCue)
        {
            var row = new AudioTickRow
            {
                stage = stage,
                relativeTick = relativeTick,
                globalTick = driver.CurrentTickIndex,
                submittedP1Buttons = (int)p1,
                submittedP2Buttons = (int)p2,
                narutoAction = naruto.Frame.N,
                narutoSourceX = naruto.Runtime.SourceRuleXInt,
                narutoSourceZ = naruto.Runtime.SourceRuleZInt,
                leeAction = lee.Frame.N,
                leeSourceX = lee.Runtime.SourceRuleXInt,
                leeSourceZ = lee.Runtime.SourceRuleZInt,
                expectedCue = expectedCue?.Replace('\\', '/'),
                pooledPlayDelta = soundPlayer.PooledOneShotPlayCountForDiagnostics - playedBefore,
                battleCatalogSealed = soundPlayer.BattleCatalogSealedForDiagnostics,
                rejectedUnpreparedCueCount = soundPlayer.RejectedUnpreparedCueCountForDiagnostics,
                failedPreparedCueLoadCount = soundPlayer.FailedPreparedCueLoadCountForDiagnostics,
                skippedMissingBattleCueFileCount = soundPlayer.SkippedMissingBattleCueFileCountForDiagnostics,
                voiceLimitDropCount = soundPlayer.OneShotVoiceLimitDropCountForDiagnostics,
                dispatchedSoundEventCount = driver.DispatchedSoundEventCountForDiagnostics,
                rejectedPublishedSoundEventCount = driver.RejectedPublishedSoundEventCountForDiagnostics,
            };
            AudioSource[] voices = soundPlayer.GetComponentsInChildren<AudioSource>(true);
            row.audioSourcePoolCount = voices.Length;
            row.audioSourceAssignedCount = voices.Count(value => value != null && value.clip != null);
            row.audioSourcePlayingCount = voices.Count(value => value != null && value.isPlaying);
            for (int index = 0; index < voices.Length; index++)
            {
                AudioSource voice = voices[index];
                if (voice == null || (voice.clip == null && !voice.isPlaying))
                    continue;
                CueMetadata metadata = FindMetadataForClip(voice.clip);
                row.voices.Add(new VoiceRow
                {
                    name = voice.name,
                    active = voice.gameObject.activeInHierarchy,
                    isPlaying = voice.isPlaying,
                    clipName = voice.clip?.name,
                    sourcePath = metadata?.sourcePath,
                    clipSamples = voice.clip?.samples ?? -1,
                    clipChannels = voice.clip?.channels ?? -1,
                    clipFrequency = voice.clip?.frequency ?? -1,
                });
            }

            for (int index = 0; index < world.PendingSounds.Count; index++)
            {
                PendingSoundEvent pending = world.PendingSounds[index];
                CueMetadata metadata = ResolveBattleCue(pending.Cue);
                int assigned = CountVoicesForCue(voices, metadata, false);
                int playing = CountVoicesForCue(voices, metadata, true);
                AudioClip clip = metadata?.clips?.FirstOrDefault(value => value != null);
                row.pendingCues.Add(new PendingCueRow
                {
                    order = index,
                    cue = pending.Cue.Replace('\\', '/'),
                    worldX = pending.WorldX,
                    tick = pending.Tick,
                    formalBattleFile = metadata?.formalBattleFile ?? false,
                    sourcePath = metadata?.sourcePath,
                    clipCount = metadata?.clips?.Length ?? 0,
                    clipSamples = clip?.samples ?? -1,
                    clipChannels = clip?.channels ?? -1,
                    clipFrequency = clip?.frequency ?? -1,
                    assignedVoiceCount = assigned,
                    playingVoiceCount = playing,
                });
            }
            row.pendingSoundCount = row.pendingCues.Count;
            row.pendingCueOrder = string.Join(";", row.pendingCues.Select(value =>
                value.order + ":" + value.cue + "@" + value.worldX + "#" + value.tick));
            if (!string.IsNullOrEmpty(expectedCue))
            {
                PendingCueRow expected = row.pendingCues.FirstOrDefault(value =>
                    NormalizeCue(value.cue) == NormalizeCue(expectedCue));
                row.expectedCueQueued = expected != null;
                row.expectedVoiceAssigned = expected != null && expected.assignedVoiceCount > 0;
                row.expectedVoicePlaying = expected != null && expected.playingVoiceCount > 0;
            }
            return row;
        }

        private static CueMetadata FindMetadataForClip(AudioClip clip)
        {
            if (clip == null)
                return null;
            foreach (CueMetadata metadata in CueCache.Values)
            {
                if (metadata?.clips != null && metadata.clips.Any(value => value == clip))
                    return metadata;
            }
            return null;
        }

        private static int CountVoicesForCue(
            AudioSource[] voices, CueMetadata metadata, bool playingOnly)
        {
            if (metadata?.clips == null)
                return 0;
            int count = 0;
            for (int index = 0; index < voices.Length; index++)
            {
                AudioSource voice = voices[index];
                if (voice == null || (playingOnly && !voice.isPlaying) || voice.clip == null)
                    continue;
                if (metadata.clips.Any(value => value == voice.clip))
                    count++;
            }
            return count;
        }

        private static void RequireProductionBoundary()
        {
            Require(driver != null && world != null && soundPlayer != null && driver.IsPaused,
                "Probe lost the paused production Driver, World, or SoundPlayer.");
            Require(ReferenceEquals(driver.World, world) &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                driver.LifecycleState == BattleRuntimeLifecycleState.Running,
                "Production World or Driver lifecycle changed at the tick boundary.");
        }

        private static void CompleteAndExit()
        {
            if (!TryOrderedShutdown())
                return;
            bool naturalSkillWitness = report.naturalSkillCueQueued &&
                report.naturalSkillVoiceAssigned && report.naturalSkillVoicePlaying;
            report.status = report.collisionWitness && naturalSkillWitness
                ? "PASS"
                : "INCONCLUSIVE";
            if (!naturalSkillWitness)
                report.error += " Natural DJA skill cue was not both queued and observed on a playing production voice; PASS is withheld.";
            report.phase = "EXITING";
            SaveSession();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static bool TryOrderedShutdown()
        {
            if (shutdownAttempted)
                return report?.orderedShutdownComplete == true;
            shutdownAttempted = true;
            if (report != null)
                report.shutdownAttempted = true;
            try
            {
                if (driver == null)
                    throw new InvalidOperationException("Production Driver was unavailable for shutdown.");
                LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in
                        Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap) ||
                            !bootstrap.gameObject.scene.IsValid())
                            continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }

                report.shutdownStatus = shutdown.Status.ToString();
                report.shutdownStage = shutdown.CompletedStage.ToString();
                report.shutdownFailure = shutdown.FailureReason;
                report.shutdownRemainingWorldObjects = shutdown.RemainingWorldObjects;
                report.shutdownRemainingRuntimeSlots = shutdown.RemainingRuntimeSlots;
                report.shutdownRemainingPoolBorrowers = shutdown.RemainingPoolBorrowers;
                report.worldDetached = driver.World == null;
                report.poolPresent = pool != null;
                report.poolQuiesced = pool == null || pool.IsQuiescedForDiagnostics;
                report.poolAcceptingRequests = pool != null && pool.AcceptingRequestsForDiagnostics;
                report.poolActiveBorrowers = pool == null
                    ? -1
                    : pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
                report.poolActiveSprites = pool?.ActiveSpriteCountForAcceptance ?? -1;
                report.audioSourcesAfterShutdown = soundPlayer == null
                    ? -1
                    : soundPlayer.GetComponentsInChildren<AudioSource>(true).Length;
                report.playingAudioSourcesAfterShutdown = soundPlayer == null
                    ? -1
                    : soundPlayer.GetComponentsInChildren<AudioSource>(true)
                        .Count(value => value != null && value.isPlaying);
                report.orderedShutdownComplete = shutdown.IsComplete && report.worldDetached &&
                    report.poolQuiesced && !report.poolAcceptingRequests &&
                    (report.poolActiveBorrowers < 0 || report.poolActiveBorrowers == 0);
                if (!report.orderedShutdownComplete)
                {
                    report.status = "FAIL";
                    report.error = "Ordered shutdown did not reach World-unbound and pool-zero postconditions.";
                    report.phase = "SHUTDOWN_FAILED";
                    SaveSession();
                    WriteBlockedEvidence();
                    return false;
                }
                return true;
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = "Ordered shutdown exception: " + error;
                report.phase = "SHUTDOWN_FAILED";
                SaveSession();
                WriteBlockedEvidence();
                return false;
            }
        }

        private static void WriteBlockedEvidence()
        {
            string output = ProjectPath(ResultPath);
            if (File.Exists(output))
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(report, true));
            Debug.LogError("[Battle Audio Scene Probe] Ordered shutdown blocked; probe stopped without unloading the Scene.");
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Battle Audio Scene Probe] " + message);
                return;
            }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            if (EditorApplication.isPlaying && driver != null && !shutdownAttempted &&
                !TryOrderedShutdown())
                return;
            if (report.phase == "SHUTDOWN_FAILED")
                return;
            SaveSession();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            try
            {
                report.exitedPlay = true;
                report.battleHashAfter = Hash(BattleScene);
                report.menuHashAfter = Hash(MenuScene);
                report.gameConfigHashAfter = Hash(GameConfigAsset);
                report.modeHashAfter = Hash(ModeAsset);
                report.formalP3HashAfter = Hash(FormalP3Wav);
                report.formalA7HashAfter = Hash(FormalA7Wav);
                Scene scene = SceneManager.GetActiveScene();
                NTSDSoundPlayer postExitSoundPlayer = Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>()
                    .FirstOrDefault(value => value != null && !EditorUtility.IsPersistent(value) &&
                        value.gameObject.scene == scene);
                AudioSource[] postExitVoices = postExitSoundPlayer == null
                    ? Array.Empty<AudioSource>()
                    : postExitSoundPlayer.GetComponentsInChildren<AudioSource>(true);
                report.postExitSoundPlayerPresent = postExitSoundPlayer != null;
                report.postExitAudioSourceCount = postExitVoices.Length;
                report.postExitPlayingAudioSourceCount = postExitVoices.Count(
                    value => value != null && value.isPlaying);
                report.postExitVoiceCleanup = report.postExitPlayingAudioSourceCount == 0;
                report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                    SceneManager.sceneCount == 1 &&
                    report.battleHashAfter == report.battleHashBefore &&
                    report.menuHashAfter == report.menuHashBefore &&
                    report.gameConfigHashAfter == report.gameConfigHashBefore &&
                    report.modeHashAfter == report.modeHashBefore &&
                    report.formalP3HashAfter == report.formalP3HashBefore &&
                    report.formalA7HashAfter == report.formalA7HashBefore;
                if (!report.sceneCleanAfter)
                {
                    report.status = "FAIL";
                    report.error += " Scene or protected asset hash changed.";
                }
                if (!report.postExitVoiceCleanup)
                {
                    report.status = "FAIL";
                    report.error += " AudioSource voices remained playing after EditMode exit.";
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error += " Finish verification exception: " + error;
            }
            report.phase = "DONE";
            string output = ProjectPath(ResultPath);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                using (var stream = new FileStream(output, FileMode.CreateNew,
                           FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    writer.Write(JsonUtility.ToJson(report, true));
                SessionState.EraseString(SessionKey);
                report = null;
                driver = null;
                world = null;
                naruto = lee = null;
                soundPlayer = null;
                CueCache.Clear();
                stableTick = -1;
                stableUpdates = 0;
                shutdownAttempted = false;
            }
            catch (Exception error)
            {
                Debug.LogError("[Battle Audio Scene Probe] Result write failed: " + error);
                SaveSession();
            }
        }

        private static string NormalizeCue(string cue)
        {
            return (cue ?? string.Empty).Replace('\\', '/').ToLowerInvariant();
        }
    }
}
#endif
