#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Unity.Collections;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07F03NaturalMarkerBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_F03NaturalMarkerBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-F03-NATURAL-SCENE-001/";
        private const string AudioResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q10-C032-NATURAL-VOICE-001/";
        private const string AudioRunId = "tay36-a243-x550-c032-voice-01";
        private const string MonoPcmRunIdV1 = "tay36-a243-x550-c032-mono-pcm-01";
        private const string MonoPcmRunIdV2 = "tay36-a243-x550-c032-mono-pcm-02";
        private const string MonoPcmRunIdV3 = "tay36-a243-x550-c032-mono-pcm-03";
        private const string MonoPcmRunId = "tay36-a243-x550-c032-mono-pcm-04";
        private const string MonoPcmResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q10-C032-NATURAL-MONO-PCM-001/";
        private const string SessionKey = "NTSD.Q07.F03NaturalMarkerBattlePlay";
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character target;
        private static NTSDSoundPlayer soundPlayer;
        private static MonoPcmCapture pcmCapture;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
        }

        [Serializable]
        private sealed class EntitySample
        {
            public int slot;
            public int oid;
            public int type;
            public int action;
            public int state;
            public int counter;
            public int x;
            public int y;
            public int z;
            public double vx;
            public double vy;
            public double vz;
            public int hp;
            public int owner;
            public int team;
            public int environmentState320;
        }

        [Serializable]
        private sealed class Sample
        {
            public int relativeTick;
            public int globalTick;
            public uint crtState;
            public ulong crtCalls;
            public int customCounter;
            public int customIndex;
            public ulong customCalls;
            public uint lastCallSite;
            public List<EntitySample> entities = new List<EntitySample>();
            public List<SoundSample> sounds = new List<SoundSample>();
        }

        [Serializable]
        private sealed class SoundSample
        {
            public string cue;
            public int worldX;
            public int tick;
        }

        [Serializable]
        private sealed class VoiceSample
        {
            public int relativeTick;
            public string cue;
            public int worldX;
            public long poolBefore;
            public long poolAfter;
            public int channels;
            public int frequency;
            public int samples;
            public bool assigned;
            public bool playing;
        }

        [Serializable]
        private sealed class AudioSourceWitness
        {
            public string gameObject;
            public string parent;
            public string clip;
            public string mixerGroup;
            public float volume;
            public float pan;
            public int timeSamples;
            public bool loop;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string contentRoot;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public int startTick;
            public int endTick;
            public int inputPhase;
            public int resourcePhase12Before;
            public int resourcePhase3Before;
            public ulong frameSequenceBefore;
            public int resourcePhase12AtStart;
            public int resourcePhase3AtStart;
            public ulong frameSequenceAtStart;
            public int difficultyBefore;
            public int difficultyEffective;
            public int battleMode;
            public int localMode;
            public int aiPhaseGate;
            public int primarySlot = -1;
            public int firstBirthRelativeTick = -1;
            public int initialY;
            public bool configuredBeforeStart;
            public bool audioVoiceProbe;
            public bool monoPcmProbe;
            public string pcmMixerGroup;
            public int pcmOtherPlayingSources;
            public int pcmMutedOtherSources;
            public int pcmOtherUnmutedDuringCapture;
            public bool pcmMuteRestored;
            public List<AudioSourceWitness> pcmOtherSources = new List<AudioSourceWitness>();
            public int pcmOutputSampleRate;
            public int pcmOutputFrames;
            public double pcmLeftRms;
            public double pcmRightRms;
            public double pcmLeftRightRatio;
            public bool pcmCaptureStopped;
            public string pcmError;
            public bool sceneCleanAfter;
            public bool exitedPlay;
            public List<Sample> samples = new List<Sample>();
            public List<VoiceSample> voices = new List<VoiceSample>();
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

        private static string PathInProject(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string HashScene()
        {
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(PathInProject(BattleScene)))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
        }

        private static void Save()
        {
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
            string path = PathInProject(ResultRootForRunId(report.runId) + report.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static string ResultRootForRunId(string runId) =>
            runId == MonoPcmRunId || runId == MonoPcmRunIdV3 ||
            runId == MonoPcmRunIdV2 ||
            runId == MonoPcmRunIdV1 ? MonoPcmResultRoot :
            runId == AudioRunId ? AudioResultRoot : ResultRoot;

        private static void Restore()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, "");
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null || report.phase != "STARTUP" || !EditorApplication.isPlaying ||
                report.configuredBeforeStart || scene.path != BattleScene) return;
            try
            {
                BattleTestBootstrap[] matches = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        value.gameObject.scene == scene && !EditorUtility.IsPersistent(value)).ToArray();
                Require(matches.Length == 1, "Expected one active BattleTestBootstrap in Play clone.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField("overrideCharacterIds",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap overrideCharacterIds is missing.");
                field.SetValue(matches[0], new[] { 36, 2 });
                report.configuredBeforeStart = true;
                Save();
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
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
                Finish();
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                Restore();
                if (report == null) { TryStart(); return; }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() < TimeSpan.FromMinutes(10),
                    "Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                if (report.phase == "CAPTURING_PCM") { WaitForMonoPcm(); return; }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void TryStart()
        {
            string requestFile = PathInProject(RequestPath);
            if (!File.Exists(requestFile)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(requestFile, JsonUtility.ToJson(request, true));
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 80 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require(request.runId == "tay36-a243-x550-natural-scene-03" ||
                request.runId == AudioRunId || request.runId == MonoPcmRunId ||
                request.runId == MonoPcmRunIdV3 || request.runId == MonoPcmRunIdV2 ||
                request.runId == MonoPcmRunIdV1,
                "Unexpected controlled C032 runId.");
            Require(!File.Exists(PathInProject(ResultRootForRunId(request.runId) +
                request.runId + ".json")),
                "Refusing to overwrite an existing result.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Editor must be in Edit Mode.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase), "Only the original project Editor is allowed.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires sole clean saved NTSD_Battle Scene.");
            report = new Report { runId = request.runId, phase = "STARTUP", status = "RUNNING",
                startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene(),
                initialY = 0, audioVoiceProbe = request.runId == AudioRunId ||
                    request.runId == MonoPcmRunId || request.runId == MonoPcmRunIdV3 ||
                    request.runId == MonoPcmRunIdV2 ||
                    request.runId == MonoPcmRunIdV1,
                monoPcmProbe = request.runId == MonoPcmRunId ||
                    request.runId == MonoPcmRunIdV3 || request.runId == MonoPcmRunIdV2 ||
                    request.runId == MonoPcmRunIdV1 };
            Save();
            EditorApplication.EnterPlaymode();
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart, "Play clone was not configured before Start.");
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
                first is LF2Character, "Tayuya roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "Second Naruto roster entity is missing.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            soundPlayer = AppManager.Instance?.SoundPlayer;
            Require(actor.ObjectId == 36 && target.ObjectId == 2,
                "Play clone roster is not formal OID36/2 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            if (report.audioVoiceProbe)
                Require(soundPlayer != null && soundPlayer.BattleCatalogSealedForDiagnostics,
                    "Production battle sound player or sealed catalog is unavailable.");
            SetInitialActor(actor, 243, 500, 500, 0);
            SetInitialActor(target, 0, 550, 500, 0);
            actor.RelationTeam = 1;
            target.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            NTSD28NativeWorldClockState nativeClock = world.Runtime.NativeWorldClock;
            Require(nativeClock != null, "Native world clock is unavailable.");
            report.resourcePhase12Before = nativeClock.ResourcePhase12;
            report.resourcePhase3Before = nativeClock.ResourcePhase3;
            report.frameSequenceBefore = nativeClock.FrameSequence;
            nativeClock.Reset();
            report.resourcePhase12AtStart = nativeClock.ResourcePhase12;
            report.resourcePhase3AtStart = nativeClock.ResourcePhase3;
            report.frameSequenceAtStart = nativeClock.FrameSequence;
            report.difficultyBefore = world.Difficulty;
            world.Runtime.Match.Difficulty = 0;
            report.difficultyEffective = world.Difficulty;
            report.battleMode = world.BattleGameModeId;
            report.localMode = world.LocalGameModeId;
            report.aiPhaseGate = world.AiPhaseGate;
            world.NativeRandom.ResetFromSeed(682973786u);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.inputPhase = world.InputPhase;
            report.phase = "MEASURING";
            Save();
        }

        private static void SetInitialActor(LF2Character actor, int action, int sourceX, int hp, int initialY)
        {
            actor.Initialize(hp, 500);
            actor.ImmediateFrame(action);
            actor.Runtime.MP = 500;
            actor.Runtime.PP = 500;
            actor.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(actor.Runtime);
            actor.SwitchDir("right");
            actor.Runtime.Vx = actor.Runtime.Vy = actor.Runtime.Vz = 0;
            actor.HitStun = 0;
            actor.AttackExempt = 0;
            actor.ItrRest.Reset();
            actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), initialY,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(actor, sourceX, 400);
            Require(actor.Frame.N == action && actor.Runtime.SourceRuleXInt == sourceX &&
                actor.Runtime.YInt == initialY && actor.Runtime.EnvironmentState320 == 0,
                "Initial action, Y, environment marker or source-rule position was not established.");
        }

        private static EntitySample Capture(LF2Entity entity, int slot)
        {
            return new EntitySample
            {
                slot = slot,
                oid = entity.ObjectId,
                type = entity.Runtime.EntityType,
                action = entity.Frame.N,
                state = entity.Frame.D.state,
                counter = entity.AttackingCounter,
                x = entity.Runtime.SourceRuleXInt,
                y = entity.Runtime.YInt,
                z = entity.Runtime.SourceRuleZInt,
                vx = entity.Runtime.Vx,
                vy = entity.Runtime.Vy,
                vz = entity.Runtime.Vz,
                hp = entity.Runtime.HP,
                owner = entity.Runtime.OwnerSlotIndex,
                team = entity.RelationTeam,
                environmentState320 = entity.Runtime.EnvironmentState320
            };
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.samples.Count == 128) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            long poolBefore = report.audioVoiceProbe
                ? soundPlayer.PooledOneShotPlayCountForDiagnostics : 0;
            if (report.monoPcmProbe && report.samples.Count == 59)
            {
                pcmCapture = new MonoPcmCapture(report);
                pcmCapture.Start();
            }
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var sample = new Sample
            {
                relativeTick = report.samples.Count + 1,
                globalTick = driver.CurrentTickIndex,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls,
                lastCallSite = rng.LastSynchronizedCallSite
            };
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null || slot > 1) continue;
                EntitySample captured = Capture(entity, slot);
                sample.entities.Add(captured);
            }
            foreach (PendingSoundEvent sound in world.PendingSounds)
            {
                sample.sounds.Add(new SoundSample
                {
                    cue = sound.Cue,
                    worldX = sound.WorldX,
                    tick = sound.Tick
                });
            }
            if (report.audioVoiceProbe)
            {
                AudioSource landingVoice = CaptureLandingVoice(sample, poolBefore);
                if (report.monoPcmProbe && sample.relativeTick == 60)
                {
                    Require(landingVoice != null && landingVoice.isPlaying,
                        "First natural landing voice is not playing.");
                    pcmCapture.MarkCue(landingVoice);
                    report.phase = "CAPTURING_PCM";
                }
            }
            report.samples.Add(sample);
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static AudioSource CaptureLandingVoice(Sample sample, long poolBefore)
        {
            SoundSample landing = sample.sounds.FirstOrDefault(value =>
                string.Equals(value.cue?.Replace('\\', '/'), "data/016.wav",
                    StringComparison.OrdinalIgnoreCase));
            if (landing == null) return null;
            MethodInfo getCue = typeof(NTSDSoundPlayer).GetMethod("GetOrPrepareCue",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(getCue != null, "Production battle cue resolver is unavailable.");
            object prepared = getCue.Invoke(soundPlayer, new object[] { landing.cue, true });
            FieldInfo clipsField = prepared?.GetType().GetField("Clips");
            AudioClip[] clips = clipsField?.GetValue(prepared) as AudioClip[];
            AudioClip clip = clips != null && clips.Length > 0 ? clips[0] : null;
            Require(clip != null, "Formal landing channel6 clip is unavailable after event.");
            var voice = new VoiceSample
            {
                relativeTick = sample.relativeTick,
                cue = landing.cue,
                worldX = landing.worldX,
                poolBefore = poolBefore,
                poolAfter = soundPlayer.PooledOneShotPlayCountForDiagnostics,
                channels = clip.channels,
                frequency = clip.frequency,
                samples = clip.samples
            };
            AudioSource landingSource = null;
            foreach (AudioSource source in soundPlayer.GetComponentsInChildren<AudioSource>(true))
            {
                if (source == null || source.clip != clip) continue;
                if (report.monoPcmProbe && !source.isPlaying) continue;
                voice.assigned = true;
                voice.playing = source.isPlaying;
                landingSource = source;
                break;
            }
            report.voices.Add(voice);
            return landingSource;
        }

        private static void WaitForMonoPcm()
        {
            Require(pcmCapture != null && driver.IsPaused &&
                report.samples.Count == 60 && driver.CurrentTickIndex == report.endTick,
                "PCM capture advanced the production simulation.");
            if (pcmCapture.FramesSinceCue < 9) return;
            pcmCapture.Stop();
            pcmCapture = null;
            Require(string.IsNullOrEmpty(report.pcmError),
                "Natural mono PCM capture failed: " + report.pcmError);
            Require(report.pcmMutedOtherSources == report.pcmOtherPlayingSources &&
                report.pcmOtherUnmutedDuringCapture == 0 && report.pcmMuteRestored,
                "Another audible AudioSource contaminates isolated mono landing PCM.");
            Require(report.pcmOutputFrames > 0 && report.pcmRightRms > 0,
                "Natural mono landing PCM is silent or empty.");
            double expectedRatio = 94.0 / 6.0;
            Require(Math.Abs(report.pcmLeftRightRatio / expectedRatio - 1.0) <= 0.20,
                "Natural mono landing PCM differs from formal L94/R6 matrix.");
            report.phase = "MEASURING";
            Save();
        }

        private static void CompleteMeasurement()
        {
            EntitySample at1 = report.samples[0].entities.FirstOrDefault(value => value.slot == 1);
            EntitySample at60 = report.samples[59].entities.FirstOrDefault(value => value.slot == 1);
            EntitySample at65 = report.samples[64].entities.FirstOrDefault(value => value.slot == 1);
            EntitySample at66 = report.samples[65].entities.FirstOrDefault(value => value.slot == 1);
            bool voicePass = !report.audioVoiceProbe ||
                (report.voices.Count == 2 &&
                 report.voices[0].relativeTick == 60 && report.voices[0].worldX == 373 &&
                 report.voices[1].relativeTick == 66 && report.voices[1].worldX == 360 &&
                 report.voices.All(value => value.poolAfter > value.poolBefore &&
                     value.channels == 1 && value.frequency == 22100 &&
                     value.samples == 8158 && value.assigned && value.playing));
            report.status = voicePass &&
                (!report.monoPcmProbe ||
                 report.pcmCaptureStopped && report.pcmOutputFrames > 0 &&
                 report.pcmOtherUnmutedDuringCapture == 0 &&
                 report.pcmMuteRestored) &&
                at1 != null && at1.oid == 2 && at1.action == 182 &&
                at1.state == 12 && at1.environmentState320 == -20 &&
                at60 != null && at60.action == 185 && at60.state == 12 &&
                at60.environmentState320 == 1 && at65 != null && at65.state == 12 &&
                at65.environmentState320 == 1 && at66 != null && at66.action == 230 &&
                at66.state == 14 && at66.environmentState320 == 0 &&
                report.samples.Count == 128 &&
                report.samples.All(value => value.entities.Count == 2) ? "PASS" : "DIFFERENCE";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            pcmCapture?.Stop();
            pcmCapture = null;
            if (report == null) { Debug.LogError("[Q07 F03 natural marker Play] " + message); return; }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.ExitPlaymode();
            else if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            pcmCapture?.Stop();
            pcmCapture = null;
            report.exitedPlay = true;
            report.sceneHashAfter = HashScene();
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                report.sceneHashAfter == report.sceneHashBefore;
            if (!report.sceneCleanAfter)
            {
                report.status = "FAIL";
                report.error += " Saved Battle Scene or active Scene state changed.";
            }
            report.phase = "DONE";
            Save();
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            actor = null;
            target = null;
            soundPlayer = null;
            stableTick = -1;
            stableUpdates = 0;
        }

        private sealed class MonoPcmCapture
        {
            private readonly Report capturedReport;
            private int previousCaptureFramerate;
            private bool previousRunInBackground;
            private bool started;
            private bool settingsChanged;
            private bool cueSeen;
            private AudioSource targetVoice;
            private readonly List<KeyValuePair<AudioSource, bool>> mutedSources =
                new List<KeyValuePair<AudioSource, bool>>();
            private double leftSquares;
            private double rightSquares;
            public int FramesSinceCue { get; private set; }

            public MonoPcmCapture(Report result)
            {
                capturedReport = result;
            }

            public void Start()
            {
                previousCaptureFramerate = Time.captureFramerate;
                previousRunInBackground = Application.runInBackground;
                Time.captureFramerate = 30;
                Application.runInBackground = true;
                settingsChanged = true;
                try
                {
                    Require(AudioSettings.speakerMode == AudioSpeakerMode.Stereo,
                        "Mono PCM witness requires stereo software output.");
                    Require(AudioRenderer.Start(), "Another AudioRenderer capture is active.");
                    started = true;
                    capturedReport.pcmOutputSampleRate = AudioSettings.outputSampleRate;
                    EditorApplication.update += OnUpdate;
                }
                catch
                {
                    Stop();
                    throw;
                }
            }

            public void MarkCue(AudioSource voice)
            {
                cueSeen = true;
                targetVoice = voice;
                capturedReport.pcmMixerGroup = voice.outputAudioMixerGroup != null
                    ? voice.outputAudioMixerGroup.name : "<master>";
                foreach (AudioSource source in UnityEngine.Object.FindObjectsOfType<AudioSource>(true))
                {
                    if (source != null && source != voice && source.isPlaying)
                    {
                        capturedReport.pcmOtherPlayingSources++;
                        capturedReport.pcmOtherSources.Add(new AudioSourceWitness
                        {
                            gameObject = source.gameObject.name,
                            parent = source.transform.parent != null
                                ? source.transform.parent.name : "",
                            clip = source.clip != null ? source.clip.name : "",
                            mixerGroup = source.outputAudioMixerGroup != null
                                ? source.outputAudioMixerGroup.name : "<master>",
                            volume = source.volume,
                            pan = source.panStereo,
                            timeSamples = source.timeSamples,
                            loop = source.loop
                        });
                        mutedSources.Add(new KeyValuePair<AudioSource, bool>(source, source.mute));
                        source.mute = true;
                        capturedReport.pcmMutedOtherSources++;
                    }
                }
            }

            private void OnUpdate()
            {
                if (!started) return;
                try
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    int frames = AudioRenderer.GetSampleCountForCaptureFrame();
                    if (frames <= 0) return;
                    Require(frames <= 65536, "AudioRenderer returned oversized frame.");
                    using (var buffer = new NativeArray<float>(frames * 2, Allocator.Temp))
                    {
                        Require(AudioRenderer.Render(buffer),
                            "AudioRenderer failed to render mono landing PCM.");
                        if (!cueSeen) return;
                        foreach (AudioSource source in
                                 UnityEngine.Object.FindObjectsOfType<AudioSource>(true))
                        {
                            if (source != null && source != targetVoice &&
                                source.isPlaying && !source.mute)
                                capturedReport.pcmOtherUnmutedDuringCapture++;
                        }
                        FramesSinceCue++;
                        if (FramesSinceCue < 2 || FramesSinceCue > 7) return;
                        for (int frame = 0; frame < frames; frame++)
                        {
                            float left = buffer[frame * 2];
                            float right = buffer[frame * 2 + 1];
                            leftSquares += left * left;
                            rightSquares += right * right;
                        }
                        capturedReport.pcmOutputFrames += frames;
                    }
                }
                catch (Exception exception)
                {
                    capturedReport.pcmError = exception.ToString();
                    Stop();
                    FramesSinceCue = 9;
                }
            }

            public void Stop()
            {
                EditorApplication.update -= OnUpdate;
                if (started)
                {
                    AudioRenderer.Stop();
                    started = false;
                }
                foreach (KeyValuePair<AudioSource, bool> source in mutedSources)
                {
                    if (source.Key != null)
                        source.Key.mute = source.Value;
                }
                capturedReport.pcmMuteRestored = mutedSources.TrueForAll(source =>
                    source.Key == null || source.Key.mute == source.Value);
                mutedSources.Clear();
                if (settingsChanged)
                {
                    Time.captureFramerate = previousCaptureFramerate;
                    Application.runInBackground = previousRunInBackground;
                    settingsChanged = false;
                }
                capturedReport.pcmCaptureStopped = true;
                if (capturedReport.pcmOutputFrames <= 0) return;
                capturedReport.pcmLeftRms =
                    Math.Sqrt(leftSquares / capturedReport.pcmOutputFrames);
                capturedReport.pcmRightRms =
                    Math.Sqrt(rightSquares / capturedReport.pcmOutputFrames);
                if (capturedReport.pcmRightRms > 0)
                    capturedReport.pcmLeftRightRatio =
                        capturedReport.pcmLeftRms / capturedReport.pcmRightRms;
            }
        }
    }
}
#endif
