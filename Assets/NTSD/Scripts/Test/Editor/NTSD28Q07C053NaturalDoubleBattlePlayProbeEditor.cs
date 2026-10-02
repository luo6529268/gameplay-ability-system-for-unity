#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C053PerHitWriter02.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001/";
        private const string Q10RequestPath = "Temp/NTSD28_Q10_C053FormalWav.request.json";
        private const string Q10ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q10-C053-FORMAL-WAV-DEPLOY-001/";
        private const string SessionKey = "NTSD.Q07.C053PerHitWriter02";
        private const string RunId = "ank580-ank580-jira500-per-hit-writer-02";
        private const string Q10RunId = "ank580-ank580-jira500-formal-wav-03";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character anko;
        private static LF2Character secondAnko;
        private static LF2Character jiraiya;
        private static SimulationWorld shadowConfiguredWorld;
        private static NTSDSoundPlayer soundPlayer;
        private static AudioClip formal020Clip;
        private static AudioClip formal067Clip;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
        }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int globalTick;
            public int ankoAction;
            public int secondAnkoAction;
            public int jiraiyaAction;
            public int ankoX;
            public int secondAnkoX;
            public int jiraiyaX;
            public int attackerCount;
            public int owner0Count;
            public int owner2Count;
            public string attackers;
            public int attackerAction50Count;
            public int attackerAction55Count;
            public int attackerSlot = -1;
            public int attackerAction = -1;
            public int attackerX;
            public int attackerY;
            public int attackerZ;
            public int childSlot = -1;
            public int childAction = -1;
            public int childWaitCounter = -1;
            public int childX;
            public int childY;
            public int childZ;
            public int childHp;
            public int victimRestFromAttacker;
            public int victimRestFromSecondAttacker;
            public string targetHitPlanEntries;
            public int targetHitPlanEntryCount;
            public string targetHitWriterEntries;
            public int targetHitWriterEntryCount;
            public long observationMismatchCount;
            public long hitPlanFailureCount;
            public bool hitPlanValid;
            public long observedDispositionCount;
            public long observedConsumeEffectsCount;
            public long observedWriterEffectCount;
            public string firstHitPlanFailureReason;
            public int firstHitPlanFailureAttackerSlot;
            public int firstHitPlanFailureCandidateOrdinal;
            public ulong consumeEffectsDifferenceMask;
            public ulong firstBodyResponseDifferenceMask;
            public ulong writerEffectDifferenceMask;
            public string firstSoundEffectDifference;
            public ulong lifecycleEffectDifferenceMask;
            public int pendingSoundCount;
            public string pendingSoundEvents;
            public long queuedSoundEventCount;
            public long rejectedSoundEventCount;
            public long q10PlayedSoundCount;
            public int q10Formal020VoiceCount;
            public int q10Formal067VoiceCount;
            public uint crtState;
            public ulong crtCalls;
            public int customCounter;
            public int customIndex;
            public ulong customCalls;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public int startTick;
            public int endTick;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public int childBirthTick = -1;
            public int attackerBirthTick = -1;
            public int difficulty;
            public int battleMode;
            public int shadowConfigCount;
            public int q10Formal020Samples;
            public int q10Formal067Samples;
            public List<TickRow> ticks = new List<TickRow>();
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
            string path = PathInProject(ResultPath(report.runId));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static string ResultPath(string runId) =>
            (runId == Q10RunId ? Q10ResultRoot : ResultRoot) + runId + ".json";

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
                field.SetValue(matches[0], new[] { 65, 702, 65 });
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
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C053 natural double Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP")
                {
                    ConfigureShadowAtResetBoundary();
                    WaitForRoster();
                    return;
                }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void TryStart()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                !string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                    "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                    StringComparison.OrdinalIgnoreCase)) return;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != BattleScene || scene.isDirty || SceneManager.sceneCount != 1)
                return;
            string requestFile = null;
            Request request = null;
            string expectedRunId = null;
            foreach (var choice in new[]
            {
                (Path: Q10RequestPath, Id: Q10RunId),
                (Path: RequestPath, Id: RunId),
            })
            {
                string candidatePath = PathInProject(choice.Path);
                if (!File.Exists(candidatePath)) continue;
                Request candidate = JsonUtility.FromJson<Request>(File.ReadAllText(candidatePath));
                if (candidate == null || !candidate.requested) continue;
                requestFile = candidatePath;
                request = candidate;
                expectedRunId = choice.Id;
                break;
            }
            if (request == null) return;
            Require(request.runId == expectedRunId, "Unexpected C053 natural double run ID.");
            Require(!File.Exists(PathInProject(ResultPath(expectedRunId))),
                "Refusing to overwrite an existing C053 natural result.");
            request.requested = false;
            File.WriteAllText(requestFile, JsonUtility.ToJson(request, true));
            report = new Report { runId = expectedRunId, phase = "STARTUP", status = "RUNNING",
                startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene() };
            Save();
            EditorApplication.EnterPlaymode();
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart, "Play clone was not configured before Start.");
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5) return;
            Require(ReferenceEquals(world, shadowConfiguredWorld) &&
                world.BattleHitExecutionPlanModeForDiagnostics ==
                    BattleHitExecutionPlanMode.ShadowCompare,
                "Read-only ShadowCompare was not configured before the first battle tick.");
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
                first is LF2Character, "OID65 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "OID702 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(2, out LF2Entity third) &&
                third is LF2Character, "Second OID65 roster entity is missing.");
            anko = (LF2Character)first;
            jiraiya = (LF2Character)second;
            secondAnko = (LF2Character)third;
            Require(anko.ObjectId == 65 && jiraiya.ObjectId == 702 &&
                secondAnko.ObjectId == 65,
                "Play clone roster is not formal OID65/702/65 trio.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            Require(FindEntity(808, out _) == null && FindEntity(875, out _) == null,
                "Natural child or attacker already occupies the World.");
            SetInitialCharacter(anko, 511, 580);
            SetInitialCharacter(jiraiya, 553, 500);
            SetInitialCharacter(secondAnko, 511, 580);
            anko.RelationTeam = 1;
            jiraiya.RelationTeam = 2;
            secondAnko.RelationTeam = 1;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Roster.Slots[2].Team = 1;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            NTSD28NativeWorldClockState nativeClock = world.Runtime.NativeWorldClock;
            Require(nativeClock != null, "Native world clock is unavailable.");
            nativeClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            report.difficulty = world.Difficulty;
            report.battleMode = world.BattleGameModeId;
            Require(report.battleMode == 0 && report.difficulty == 0,
                "Controlled mode/difficulty does not match source mode0.");
            world.NativeRandom.ResetFromSeed(682973786u);
            if (report.runId == Q10RunId)
            {
                soundPlayer = UnityEngine.Object.FindObjectOfType<NTSDSoundPlayer>();
                Require(soundPlayer != null,
                    "Active battle sound player was not found.");
                Require(soundPlayer.BattleCatalogSealedForDiagnostics,
                    "Battle sound catalog was not sealed before the natural hit chain.");
                formal020Clip = CaptureFormalBattleClip(@"data\020.wav", 16413);
                formal067Clip = CaptureFormalBattleClip(@"data\067.wav", 31170);
                report.q10Formal020Samples = formal020Clip.samples;
                report.q10Formal067Samples = formal067Clip.samples;
            }
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            Save();
        }

        private static AudioClip CaptureFormalBattleClip(string soundId, int samples)
        {
            MethodInfo getCue = typeof(NTSDSoundPlayer).GetMethod(
                "GetOrPrepareCue", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(getCue != null, "Battle cue resolver is missing.");
            object cue = getCue.Invoke(soundPlayer, new object[] { soundId, true });
            Require(cue != null, "Formal battle cue was not prepared: " + soundId);
            Type cueType = cue.GetType();
            string sourcePath = (string)cueType.GetField("SourcePath").GetValue(cue);
            Require(sourcePath.Replace('\\', '/').Contains(
                    "/NTSD/Content/LoganRuntime/vfs/data/"),
                "Battle cue did not resolve to formal runtime: " + soundId);
            AudioClip[] clips = (AudioClip[])cueType.GetField("Clips").GetValue(cue);
            Require(clips != null && clips.Length == 1 && clips[0] != null &&
                clips[0].samples == samples,
                "Formal battle cue clip missing or sample count differs: " + soundId);
            return clips[0];
        }

        private static void ConfigureShadowAtResetBoundary()
        {
            SimulationTickDriver[] activeDrivers =
                Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        !EditorUtility.IsPersistent(value)).ToArray();
            if (activeDrivers.Length == 0 || activeDrivers[0].World == null) return;
            Require(activeDrivers.Length == 1, "Expected one active production Driver.");
            SimulationWorld candidate = activeDrivers[0].World;
            if (ReferenceEquals(candidate, shadowConfiguredWorld)) return;
            Require(candidate.CurrentTickIndex == 0 &&
                activeDrivers[0].CurrentTickIndex == 0,
                "ShadowCompare World was first observed after the reset boundary.");
            candidate.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            shadowConfiguredWorld = candidate;
            report.shadowConfigCount++;
            Save();
        }

        private static void SetInitialCharacter(LF2Character character, int action, int sourceX)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(action);
            character.Runtime.MP = 500;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.HitStun = 0;
            character.AttackExempt = 0;
            character.ItrRest.Reset();
            character.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(character, sourceX, 400);
            Require(character.Frame.N == action && character.Runtime.SourceRuleXInt == sourceX,
                "Character initial action or source X differs.");
        }

        private static LF2Entity FindEntity(int oid, out int slot)
        {
            for (int index = 50; index < world.RuntimeSlotCapacityForDiagnostics; index++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(index);
                if (entity != null && entity.ObjectId == oid)
                {
                    slot = index;
                    return entity;
                }
            }
            slot = -1;
            return null;
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.ticks.Count == 12) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            long q10PlayedBefore = report.runId == Q10RunId
                ? soundPlayer.PooledOneShotPlayCountForDiagnostics
                : 0;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None),
                new SimulationPlayerInput(2, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            LF2Entity child = FindEntity(808, out int childSlot);
            LF2Entity attacker = FindEntity(875, out int attackerSlot);
            int attackerCount = 0;
            int action50Count = 0;
            int action55Count = 0;
            int owner0Count = 0;
            int owner2Count = 0;
            int secondAttackerSlot = -1;
            var attackerRows = new List<string>();
            for (int index = 50; index < world.RuntimeSlotCapacityForDiagnostics; index++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(index);
                if (entity == null || entity.ObjectId != 875) continue;
                attackerCount++;
                if (entity.OwnerEntityIndex == 0) owner0Count++;
                if (entity.OwnerEntityIndex == 2)
                {
                    owner2Count++;
                    if (secondAttackerSlot < 0) secondAttackerSlot = index;
                }
                attackerRows.Add(index + ":" + entity.OwnerEntityIndex + ":" +
                    entity.Frame.N + ":" + entity.Runtime.SourceRuleXInt);
                if (entity.Frame.N == 50) action50Count++;
                if (entity.Frame.N == 55) action55Count++;
            }
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            AudioSource[] q10Voices = report.runId == Q10RunId
                ? soundPlayer.GetComponentsInChildren<AudioSource>(true)
                : Array.Empty<AudioSource>();
            var soundRows = new List<string>();
            foreach (var sound in world.PendingSounds)
                soundRows.Add(sound.Cue + "@" + sound.WorldX + "@" + sound.Tick);
            BattleHitExecutionPlanDiagnostics hitPlan =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            var targetEntries = new List<string>();
            var writerRows = new List<string>();
            if (child != null)
            {
                for (int index = 0;
                     world.TryGetBattleHitExecutionPlanEntryForDiagnostics(
                         index, out BattleHitExecutionPlanEntryView entry);
                     index++)
                {
                    if (entry.TargetSlot != childSlot) continue;
                    targetEntries.Add(entry.AttackerHandle.Slot + ":" +
                        (int)entry.Pass + ":" + entry.TargetSlot + ":" +
                        entry.ItrKind + ":" + entry.ExpectedDisposition + ":" +
                        entry.ObservedDisposition + ":" +
                        entry.PreprocessObserved + ":" +
                        entry.ConsumeEffectsObserved + ":" +
                        entry.FirstBodyResponseAttemptObserved + ":" +
                        entry.ExpectedResolvedItrKind + ":" +
                        entry.ObservedResolvedItrKind + ":" +
                        entry.ExpectedConsumeEffectsFingerprint + ":" +
                        entry.ObservedConsumeEffectsFingerprint);
                    if (entry.WriterEffectObserved)
                        writerRows.Add(entry.AttackerHandle.Slot + ":" +
                            entry.ObservedWriterTargetFrame + ":" +
                            entry.ObservedWriterTargetWaitCounter + ":" +
                            entry.ObservedWriterTargetHp);
                }
            }
            var row = new TickRow
            {
                relativeTick = report.ticks.Count + 1,
                globalTick = driver.CurrentTickIndex,
                ankoAction = anko.Frame.N,
                secondAnkoAction = secondAnko.Frame.N,
                jiraiyaAction = jiraiya.Frame.N,
                ankoX = anko.Runtime.SourceRuleXInt,
                secondAnkoX = secondAnko.Runtime.SourceRuleXInt,
                jiraiyaX = jiraiya.Runtime.SourceRuleXInt,
                attackerCount = attackerCount,
                owner0Count = owner0Count,
                owner2Count = owner2Count,
                attackers = string.Join(";", attackerRows),
                attackerAction50Count = action50Count,
                attackerAction55Count = action55Count,
                attackerSlot = attackerSlot,
                attackerAction = attacker?.Frame.N ?? -1,
                attackerX = attacker?.Runtime.SourceRuleXInt ?? 0,
                attackerY = attacker?.Runtime.YInt ?? 0,
                attackerZ = attacker?.Runtime.SourceRuleZInt ?? 0,
                childSlot = childSlot,
                childAction = child?.Frame.N ?? -1,
                childWaitCounter = child?.Trans.WaitCounter ?? -1,
                childX = child?.Runtime.SourceRuleXInt ?? 0,
                childY = child?.Runtime.YInt ?? 0,
                childZ = child?.Runtime.SourceRuleZInt ?? 0,
                childHp = child?.Runtime.HP ?? 0,
                victimRestFromAttacker = child == null || attacker == null ? 0 :
                    world.GetRawRestVrest(childSlot, attackerSlot),
                victimRestFromSecondAttacker = child == null || secondAttackerSlot < 0 ? 0 :
                    world.GetRawRestVrest(childSlot, secondAttackerSlot),
                targetHitPlanEntries = string.Join(";", targetEntries),
                targetHitPlanEntryCount = targetEntries.Count,
                targetHitWriterEntries = string.Join(";", writerRows),
                targetHitWriterEntryCount = writerRows.Count,
                observationMismatchCount = hitPlan.ObservationMismatchCount,
                hitPlanFailureCount = hitPlan.FailureCount,
                hitPlanValid = hitPlan.CurrentTickPlanValid,
                observedDispositionCount = hitPlan.ObservedDispositionCount,
                observedConsumeEffectsCount = hitPlan.ObservedConsumeEffectsCount,
                observedWriterEffectCount = hitPlan.ObservedWriterEffectCount,
                firstHitPlanFailureReason = hitPlan.FirstFailureReason.ToString(),
                firstHitPlanFailureAttackerSlot = hitPlan.FirstFailureAttackerSlot,
                firstHitPlanFailureCandidateOrdinal = hitPlan.FirstFailureCandidateOrdinal,
                consumeEffectsDifferenceMask = hitPlan.LastConsumeEffectsDifferenceMask,
                firstBodyResponseDifferenceMask =
                    hitPlan.LastFirstBodyResponseDifferenceMask,
                writerEffectDifferenceMask = hitPlan.LastWriterEffectDifferenceMask,
                firstSoundEffectDifference = hitPlan.FirstSoundEffectDifference,
                lifecycleEffectDifferenceMask = hitPlan.LastLifecycleEffectDifferenceMask,
                pendingSoundCount = world.PendingSounds.Count,
                pendingSoundEvents = string.Join(";", soundRows),
                queuedSoundEventCount = world.QueuedSoundEventCountForDiagnostics,
                rejectedSoundEventCount = world.RejectedSoundEventCountForDiagnostics,
                q10PlayedSoundCount = report.runId == Q10RunId
                    ? soundPlayer.PooledOneShotPlayCountForDiagnostics - q10PlayedBefore
                    : 0,
                q10Formal020VoiceCount = q10Voices.Count(value =>
                    value != null && value.clip == formal020Clip),
                q10Formal067VoiceCount = q10Voices.Count(value =>
                    value != null && value.clip == formal067Clip),
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls
            };
            if (child != null && report.childBirthTick < 0)
                report.childBirthTick = row.relativeTick;
            if (attacker != null && report.attackerBirthTick < 0)
                report.attackerBirthTick = row.relativeTick;
            report.ticks.Add(row);
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            TickRow seventh = report.ticks.Single(value => value.relativeTick == 7);
            bool matched = report.childBirthTick == 1 && report.attackerBirthTick == 4 &&
                seventh.attackerCount == 2 && seventh.owner0Count == 1 &&
                seventh.owner2Count == 1 && seventh.childAction == 156 &&
                seventh.childHp == 450 && seventh.victimRestFromAttacker > 0 &&
                seventh.victimRestFromSecondAttacker > 0 &&
                seventh.targetHitPlanEntryCount == 2 &&
                seventh.targetHitPlanEntries.StartsWith("51:2:50:0:Damage:Damage:") &&
                seventh.targetHitPlanEntries.Contains(";52:2:50:0:Damage:Damage:") &&
                seventh.targetHitWriterEntryCount == 2 &&
                seventh.targetHitWriterEntries.StartsWith("51:156:153:475") &&
                seventh.targetHitWriterEntries.Contains(";52:156:153:450") &&
                seventh.childWaitCounter == 156 &&
                seventh.observationMismatchCount == 0 &&
                seventh.hitPlanFailureCount == 0 && seventh.hitPlanValid &&
                seventh.writerEffectDifferenceMask == 0 &&
                string.IsNullOrEmpty(seventh.firstSoundEffectDifference);
            if (report.runId == Q10RunId)
            {
                TickRow first = report.ticks[0];
                matched = matched && report.q10Formal020Samples == 16413 &&
                    report.q10Formal067Samples == 31170 &&
                    first.pendingSoundCount == 3 &&
                    first.q10PlayedSoundCount >= 2 &&
                    first.q10Formal020VoiceCount >= 1 &&
                    first.q10Formal067VoiceCount >= 1 &&
                    seventh.q10Formal020VoiceCount >= 1;
            }
            report.status = matched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
            if (!matched)
                report.error = "Per-hit writer or Q10 formal audio witness differs; inspect tick rows.";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C053 natural Play] " + message);
                return;
            }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
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
            anko = null;
            secondAnko = null;
            jiraiya = null;
            shadowConfiguredWorld = null;
            soundPlayer = null;
            formal020Clip = null;
            formal067Clip = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
