#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28OriginalCommonBattleSceneProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string ResultPath = "artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/original-common-fix-01/battle-scene-01.json";
        private const string SessionKey = "NTSD.OriginalCommon.Scene.01";
        private static Report report;
        private static SimulationTickDriver driver;
        private static LF2Character p1;
        private static LF2Character p2;
        private static LF2Entity clone;
        private static NTSDSoundPlayer player;
        private static double lastStep;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static readonly List<LF2Entity> entities = new List<LF2Entity>();

        [Serializable]
        private sealed class Report
        {
            public string status = "RUNNING";
            public string phase = "STARTUP";
            public string error;
            public string startedUtc;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public bool configured;
            public int tick;
            public int attackTick;
            public int caughtTick;
            public int consumedTick;
            public int cloneSlot = -1;
            public int cloneOwner = -1;
            public int holdCueEvents;
            public int persistentVoiceId;
            public int maxSameCueVoices;
            public bool repeatedVoicePlaying;
            public bool heldSkillVisible;
            public bool skillHiddenAfterHit;
            public bool orderedShutdown;
            public int remainingObjects = -1;
            public int remainingSlots = -1;
            public int remainingBorrowers = -1;
            public int generatedClipCount;
            public bool exitedPlay;
            public bool sceneClean;
            public int postExitAudioSources;
            public int postExitCopies;
            public long rejected;
            public long dropped;
            public string setup = "Play-only Naruto/Naruto roster; P2 authored272 produces real clones; one clone positioned near P1, others parked; P1 discrete D/F/J then Attack via production Manual Driver. Original native removal timing not captured.";
            public List<Row> rows = new List<Row>();
        }

        [Serializable]
        private sealed class Row
        {
            public int tick;
            public int key;
            public int p1Action;
            public int cloneAction = -1;
            public int ballAction = -1;
            public int ballLink;
            public int ballSlot = -1;
            public int sameCueVoices;
            public int playingCueVoices;
            public int voiceId;
            public int voiceSample;
            public int channels;
            public float volume;
            public float pan;
        }

        static NTSD28OriginalCommonBattleSceneProbeEditor()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlay;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [MenuItem("NTSD/Validation/Original Common Audio And Clone Scene Probe")]
        private static void Start()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating, "Editor is not idle.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == ScenePath && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires one saved original Battle Scene.");
            Require(!File.Exists(ResultPath), "Refuses to overwrite original probe evidence.");
            report = new Report { startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = Hash() };
            Save();
            EditorApplication.EnterPlaymode();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null || report.configured || !EditorApplication.isPlaying || scene.path != ScenePath)
                return;
            try
            {
                BattleTestBootstrap bootstrap = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Single(value => value != null && value.isActiveAndEnabled && !EditorUtility.IsPersistent(value));
                typeof(BattleTestBootstrap).GetField("overrideCharacterIds", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(bootstrap, new[] { 2, 2 });
                report.configured = true;
                Save();
            }
            catch (Exception exception) { Fail(exception.ToString()); }
        }

        private static void OnPlay(PlayModeStateChange state)
        {
            Restore();
            if (report == null)
                return;
            if (state == PlayModeStateChange.ExitingPlayMode && report.phase != "EXITING")
            {
                report.status = "FAIL";
                report.error = "Play stopped before probe completion.";
                report.phase = "EXITING";
                Save();
            }
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
                Finish();
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
                Require((DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime()).TotalSeconds < 720,
                    "Probe deadline exceeded.");
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        Finish();
                    return;
                }
                if (!EditorApplication.isPlaying)
                    return;
                if (report.phase == "STARTUP") { Setup(); return; }
                if (EditorApplication.timeSinceStartup - lastStep < 0.033)
                    return;
                lastStep = EditorApplication.timeSinceStartup;
                Step();
            }
            catch (Exception exception) { Fail(exception.ToString()); }
        }

        private static void Setup()
        {
            driver = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                .FirstOrDefault(value => value != null && value.isActiveAndEnabled && !EditorUtility.IsPersistent(value));
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (stableTick != driver.CurrentTickIndex) { stableTick = driver.CurrentTickIndex; stableUpdates = 0; return; }
            if (++stableUpdates < 3)
                return;
            Require(report.configured, "Play roster was not configured before Start.");
            SimulationWorld world = driver.World;
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity first) && first.ObjectId == 2 &&
                world.TryResolveRosterInputEntity(1, out LF2Entity second) && second.ObjectId == 2,
                "Naruto/Naruto roster did not initialize.");
            world.TryResolveRosterInputEntity(1, out LF2Entity other);
            p1 = (LF2Character)first;
            p2 = (LF2Character)other;
            player = Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>()
                .FirstOrDefault(value => value != null && value.isActiveAndEnabled && !EditorUtility.IsPersistent(value));
            Require(player != null && player.BattleCatalogSealedForDiagnostics, "Battle sound prewarm did not seal.");
            driver.ApplySettings(new LockstepSimulationSettings { driveMode = SimulationDriveMode.Manual });
            driver.SetPaused(true);
            ResetActor(p1, 500, 450, 1);
            ResetActor(p2, 1500, 450, 2);
            p2.ImmediateFrame(272);
            world.Runtime.FunctionKeys.ResetForBattle(true);
            world.Runtime.Flow.FrameToggle = world.Runtime.Flow.InputPhase = 0;
            world.Runtime.NativeWorldClock.Reset();
            world.NativeRandom.ResetFromSeed(682973786u);
            world.PendingSounds.Clear();
            report.phase = "HOLD";
            Save();
        }

        private static void ResetActor(LF2Character actor, int x, int z, int team)
        {
            actor.Initialize(500, 500);
            actor.ImmediateFrame(0);
            actor.Runtime.MP = actor.Runtime.PP = 500;
            actor.Runtime.HP2Orig = 1;
            actor.Runtime.RespawnCount = 0;
            actor.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(actor.Runtime);
            actor.AiControlled = false;
            actor.ItrRest.Reset();
            actor.SwitchDir("right");
            actor.Runtime.SetPosition(driver.World.SpatialProjection.SourceToViewX(x), 0,
                driver.World.SpatialProjection.SourceToViewZ(z));
            AppManager.SyncParticipantBirthPosition(actor, x, z);
            actor.RelationTeam = team;
            driver.World.Runtime.Roster.Slots[team - 1].Team = team;
        }

        private static void Step()
        {
            SimulationWorld world = driver.World;
            int relative = ++report.tick;
            Require(relative < 210, "Rasengan or clone did not complete within bounded ticks.");
            SimulationInputButtons buttons = relative <= 2 ? SimulationInputButtons.Attack :
                relative <= 4 ? SimulationInputButtons.Right : relative <= 6 ? SimulationInputButtons.Defend :
                report.attackTick > 0 && relative - report.attackTick < 2 ? SimulationInputButtons.Jump :
                SimulationInputButtons.None;
            int tick = driver.CurrentTickIndex + 1;
            Require(driver.StepOneTick(new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(0, buttons),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            }), ignorePaused: true, buildPresentation: true), "Production tick was rejected.");
            world.GetAllEntities(entities);
            LF2Entity ball = entities.FirstOrDefault(value => value.ObjectId == 434);
            foreach (LF2Entity entity in entities.Where(value => value.ObjectId == 33))
                if (entity is LF2Character character) character.AiControlled = false;
            if (report.attackTick == 0 && relative >= 70 && report.holdCueEvents >= 10 && ball != null)
            {
                clone = entities.FirstOrDefault(value => value.ObjectId == 33 && value.Runtime.OwnerSlotIndex == p2.Runtime.SlotIndex);
                if (clone != null)
                {
                    report.cloneSlot = clone.Runtime.SlotIndex;
                    report.cloneOwner = clone.Runtime.OwnerSlotIndex;
                    foreach (LF2Entity entity in entities.Where(value => value.ObjectId == 33))
                    {
                        int x = ReferenceEquals(entity, clone) ? 700 : 1700;
                        entity.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(x), 0,
                            world.SpatialProjection.SourceToViewZ(450));
                        entity.Runtime.SetSourceRulePosition(x, 450);
                        entity.Runtime.SyncIntegerPosition();
                        entity.Runtime.SyncSourceRuleIntegerPosition();
                    }
                    report.attackTick = relative + 1;
                    report.phase = "ATTACK";
                }
            }
            var row = new Row { tick = relative, key = (int)buttons, p1Action = p1.Frame.N,
                cloneAction = entities.Contains(clone) ? clone.Frame.N : -1,
                ballAction = ball?.Frame.N ?? -1, ballLink = ball?.Runtime.LinkState ?? 0,
                ballSlot = ball?.Runtime.SlotIndex ?? -1 };
            CaptureVoice(row, world);
            if (ball != null)
                report.heldSkillVisible |= ball.Frame.D?.pic < 999;
            if (report.attackTick > 0 && p1.Frame.D?.state == LF2States.Catching && report.caughtTick == 0)
                report.caughtTick = relative;
            if (report.caughtTick > 0 && ball == null)
            {
                report.consumedTick = relative;
                var plan = BattleCentralRenderSystem.PrepareFrame(world);
                Require(plan.IsValid && !plan.IsStale, "No current render snapshot after skill consumption.");
                report.skillHiddenAfterHit = !Enumerable.Range(0, plan.CapturedFrame.EntityCount)
                    .Any(index => plan.CapturedFrame.GetEntity(index).CurrentDatObjectId == 434 &&
                        plan.CapturedFrame.GetEntity(index).EntityVisible);
            }
            report.rows.Add(row);
            Save();
            if (report.consumedTick > 0)
            {
                Require(report.repeatedVoicePlaying && report.maxSameCueVoices == 1 && report.skillHiddenAfterHit,
                    "Audio retrigger or consumed skill presentation witness was missing.");
                report.rejected = player.RejectedUnpreparedCueCountForDiagnostics;
                report.dropped = player.OneShotVoiceLimitDropCountForDiagnostics;
                Require(report.rejected == 0 && report.dropped == 0, "Battle playback rejected or dropped events.");
                report.status = "PASS";
                Exit();
            }
        }

        private static void CaptureVoice(Row row, SimulationWorld world)
        {
            if (!world.PendingSounds.Any(value => value.Cue == @"data\053.wav"))
                return;
            string[] identities = (string[])typeof(NTSDSoundPlayer).GetField("battleVoiceIdentities", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player);
            AudioSource[] voices = (AudioSource[])typeof(NTSDSoundPlayer).GetField("oneShotVoices", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player);
            for (int index = 0; index < identities.Length; index++)
            {
                if (identities[index] != @"data\053.wav" || voices[index] == null)
                    continue;
                AudioSource voice = voices[index];
                row.sameCueVoices++;
                row.playingCueVoices += voice.isPlaying ? 1 : 0;
                row.voiceId = voice.GetInstanceID();
                row.voiceSample = voice.timeSamples;
                row.channels = voice.clip.channels;
                row.volume = voice.volume;
                row.pan = voice.panStereo;
            }
            Require(row.sameCueVoices == 1 && row.channels == 2, "Persistent cue layered multiple voices or lost mono adapter.");
            if (report.persistentVoiceId == 0) report.persistentVoiceId = row.voiceId;
            Require(report.persistentVoiceId == row.voiceId, "Persistent cue replaced its voice instead of retriggering.");
            report.holdCueEvents++;
            report.maxSameCueVoices = Math.Max(report.maxSameCueVoices, row.sameCueVoices);
            report.repeatedVoicePlaying |= row.playingCueVoices == 1;
        }

        private static void Exit()
        {
            if (driver?.World != null)
            {
                report.generatedClipCount = ((System.Collections.ICollection)typeof(NTSDSoundPlayer)
                    .GetField("ownedBattlePlaybackClips", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).Count;
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap)) continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }
                report.remainingObjects = shutdown.RemainingWorldObjects;
                report.remainingSlots = shutdown.RemainingRuntimeSlots;
                report.remainingBorrowers = shutdown.RemainingPoolBorrowers;
                report.orderedShutdown = shutdown.IsComplete && driver.World == null;
                if (!report.orderedShutdown)
                {
                    report.status = "FAIL";
                    report.error += " Ordered shutdown failed: " + shutdown.FailureReason;
                    report.phase = "SHUTDOWN_FAILED";
                    Save();
                    return;
                }
            }
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string error)
        {
            if (report == null) { Debug.LogError(error); return; }
            report.status = "FAIL";
            report.error = error;
            Exit();
        }

        private static void Finish()
        {
            Scene scene = SceneManager.GetActiveScene();
            report.exitedPlay = true;
            report.sceneHashAfter = Hash();
            report.sceneClean = scene.path == ScenePath && !scene.isDirty && report.sceneHashBefore == report.sceneHashAfter;
            foreach (NTSDSoundPlayer remaining in Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>())
            {
                if (remaining == null || EditorUtility.IsPersistent(remaining)) continue;
                report.postExitAudioSources += remaining.GetComponentsInChildren<AudioSource>().Length;
                report.postExitCopies += ((System.Collections.ICollection)typeof(NTSDSoundPlayer)
                    .GetField("ownedBattlePlaybackClips", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(remaining)).Count;
            }
            if (!report.sceneClean || report.postExitAudioSources != 0 || report.postExitCopies != 0)
            {
                report.status = "FAIL";
                report.error += " Scene or audio resources did not restore after Play.";
            }
            using (var stream = new FileStream(ResultPath, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream)) writer.Write(JsonUtility.ToJson(report, true));
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            player = null;
            p1 = p2 = null;
            clone = null;
            entities.Clear();
            stableTick = -1;
            stableUpdates = 0;
        }

        private static void Require(bool condition, string error)
        {
            if (!condition) throw new InvalidOperationException(error);
        }
        private static string Hash()
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream file = File.OpenRead(ScenePath))
                return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", string.Empty);
        }
        private static void Save() => SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
        private static void Restore()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(saved)) report = JsonUtility.FromJson<Report>(saved);
        }
    }
}
#endif
