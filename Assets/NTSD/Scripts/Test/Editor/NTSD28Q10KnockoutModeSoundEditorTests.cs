#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Load;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_Q10")]
    public sealed class NTSD28Q10KnockoutModeSoundEditorTests
    {
        [Test]
        public void SelectedModeCuesEnterBattleAudioPrewarmSet()
        {
            string root = Path.Combine(Application.dataPath, "NTSD", "Content", "LoganRuntime");
            LoganVisualContentCandidate candidate = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(root));
            Assert.That(candidate.Catalog.ModeComboInput.KnockoutFeed.StageTeam1DeathSoundPath,
                Is.EqualTo(@"data\m_ok.wav"));
            Assert.That(candidate.Catalog.ModeComboInput.KnockoutFeed.StageTeam5DeathSoundPath,
                Is.EqualTo(@"data\m_join.wav"));

            var host = new GameObject("Q10AudioPrewarmFixture");
            host.SetActive(false);
            try
            {
                var manager = host.AddComponent<CharacterAnimtorManager>();
                typeof(CharacterAnimtorManager).GetField("publishedLoganCandidate",
                    BindingFlags.Instance | BindingFlags.NonPublic).SetValue(manager, candidate);
                var sounds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                manager.CollectBattleSoundIds(sounds);
                Assert.That(sounds, Does.Contain(@"data\m_ok.wav"));
                Assert.That(sounds, Does.Contain(@"data\m_join.wav"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [UnityTest]
        public IEnumerator SelectedModeWavsResolveAndDecodeThroughBattleAudioPath()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var host = new GameObject("Q10AudioPathFixture");
                var player = host.AddComponent<NTSDSoundPlayer>();
                try
                {
                    foreach (string cue in new[] { @"data\m_ok.wav", @"data\m_join.wav" })
                    {
                        object prepared = typeof(NTSDSoundPlayer).GetMethod("GetOrPrepareCue",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(player, new object[] { cue });
                        Type preparedType = prepared.GetType();
                        string sourcePath = (string)preparedType.GetField("SourcePath").GetValue(prepared);
                        string expectedPath = Path.Combine(Application.dataPath, "NTSD", "Sound",
                            "data", Path.GetFileName(cue));
                        Assert.That(sourcePath.Replace('\\', '/'),
                            Is.EqualTo(expectedPath.Replace('\\', '/')));
                        Assert.That(File.Exists(sourcePath), Is.True);

                        string cacheKey = "Q10ModeSoundDecode::" + Guid.NewGuid().ToString("N");
                        AudioClip clip = null;
                        try
                        {
                            clip = await NTSD_ResourceLoader.Instance.LoadSingleAudioClipAsync(
                                cacheKey, sourcePath);
                            Assert.That(clip, Is.Not.Null, cue);
                            Assert.That(clip.samples, Is.GreaterThan(0), cue);
                        }
                        finally
                        {
                            NTSD_ResourceLoader.Instance.RemoveCache(cacheKey);
                            if (clip != null)
                                UnityEngine.Object.DestroyImmediate(clip);
                        }
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(host);
                }
            });
        }

        [UnityTest]
        public IEnumerator SelectedModeWavsRemainPreparedAfterBattleCatalogSeal()
        {
            return UniTask.ToCoroutine(async () =>
            {
                string root = Path.Combine(Application.dataPath, "NTSD", "Content", "LoganRuntime");
                LoganVisualContentCandidate candidate = LoganVisualContentCandidate.Capture(
                    BattleContentSource.ForLoganRuntime(root));
                var managerHost = new GameObject("Q10SealedAudioManagerFixture");
                managerHost.SetActive(false);
                var playerHost = new GameObject("Q10SealedAudioPlayerFixture");
                try
                {
                    var manager = managerHost.AddComponent<CharacterAnimtorManager>();
                    typeof(CharacterAnimtorManager).GetField("publishedLoganCandidate",
                        BindingFlags.Instance | BindingFlags.NonPublic).SetValue(manager, candidate);
                    var player = playerHost.AddComponent<NTSDSoundPlayer>();
                    await player.PrepareBattleCuesAsync(manager);

                    Assert.That(player.BattleCatalogSealedForDiagnostics, Is.True);
                    foreach (string cue in new[] { @"data\m_ok.wav", @"data\m_join.wav" })
                    {
                        Assert.That(player.TryGetPreparedSingleFileWrapperForDiagnostics(
                            cue, out AudioClip[] clips), Is.True, cue);
                        Assert.That(clips, Has.Length.EqualTo(1), cue);
                        Assert.That(clips[0], Is.Not.Null, cue);
                        Assert.That(clips[0].samples, Is.GreaterThan(0), cue);
                    }
                    Assert.That(player.RejectedUnpreparedCueCountForDiagnostics, Is.Zero);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(playerHost);
                    UnityEngine.Object.DestroyImmediate(managerHost);
                }
            });
        }

        [TestCase(1, 5, @"data\m_join.wav")]
        [TestCase(5, 1, @"data\m_ok.wav")]
        public void LethalTickQueuesSelectedVictimGroupSoundOnlyOnce(
            int attackerGroup,
            int victimGroup,
            string expectedCue)
        {
            SimulationWorld world = CreateLethalWorld(
                attackerGroup, victimGroup, battleMode: 1, feedPresent: true);
            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.ConfigureNativeKnockoutAudio(
                @"data\m_ok.wav", @"data\m_join.wav", true);

            tickSystem.RunReleaseTick(1, buildPresentation: false);

            Assert.That(world.NativeKnockoutEvents.Count, Is.EqualTo(1));
            Assert.That(CountCue(world, expectedCue), Is.EqualTo(1));
            PendingSoundEvent knockoutSound = FindCue(world, expectedCue);
            Assert.That(knockoutSound.WorldX, Is.EqualTo(10));
            Assert.That(knockoutSound.Tick, Is.EqualTo(1));

            tickSystem.RunReleaseTick(2, buildPresentation: false);

            Assert.That(world.NativeKnockoutEvents.Count, Is.EqualTo(1),
                "The native record remains in its lifetime window.");
            Assert.That(CountCue(world, expectedCue), Is.Zero,
                "A retained native record must not replay its sound.");
        }

        [TestCase(0, true, true)]
        [TestCase(1, false, true)]
        [TestCase(1, true, false)]
        public void NativeModeSoundHonorsBattleModeDisplayAndRecordPresence(
            int battleMode,
            bool displayEnabled,
            bool feedPresent)
        {
            SimulationWorld world = CreateLethalWorld(
                attackerGroup: 1,
                victimGroup: 5,
                battleMode,
                feedPresent);
            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.ConfigureNativeKnockoutAudio(
                @"data\m_ok.wav", @"data\m_join.wav", displayEnabled);

            tickSystem.RunReleaseTick(1, buildPresentation: false);

            Assert.That(world.NativeKnockoutEvents.Count, Is.EqualTo(1));
            Assert.That(CountCue(world, @"data\m_ok.wav"), Is.Zero);
            Assert.That(CountCue(world, @"data\m_join.wav"), Is.Zero);
        }

        private static int CountCue(SimulationWorld world, string cue)
        {
            int count = 0;
            foreach (PendingSoundEvent sound in world.PendingSounds)
            {
                if (string.Equals(sound.Cue, cue, StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }

        private static PendingSoundEvent FindCue(SimulationWorld world, string cue)
        {
            foreach (PendingSoundEvent sound in world.PendingSounds)
            {
                if (string.Equals(sound.Cue, cue, StringComparison.OrdinalIgnoreCase))
                    return sound;
            }
            Assert.Fail($"Expected battle cue {cue} was not queued.");
            return default;
        }

        private static SimulationWorld CreateLethalWorld(
            int attackerGroup,
            int victimGroup,
            int battleMode,
            bool feedPresent)
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = battleMode;
            world.Runtime.FunctionKeys.ResetForBattle(true);
            world.Runtime.NativeKnockoutFeed.RestoreForSnapshot(
                feedPresent, 70);

            MethodInfo create =
                typeof(NTSD28Q08CombatLethalPrecombatTimingEditorTests).GetMethod(
                    "CreateCombatant",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(create, Is.Not.Null);
            create.Invoke(null, new object[]
            {
                world, 0, attackerGroup, 7100, 0, true, 500
            });
            create.Invoke(null, new object[]
            {
                world, 1, victimGroup, 7101, 10, false, 20
            });
            return world;
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q10ModeSoundScenePlayProbe
    {
        private const string RequestPath = "Temp/NTSD28_Q10_ModeSoundPlay.request";
        private const string ResultPath =
            "artifacts/diagnostics/NTSD28-Q10-KNOCKOUT-MODE-SOUND-001/scene-play-audio-probe.json";
        private const string NaturalResultPath =
            "artifacts/diagnostics/NTSD28-Q10-KNOCKOUT-MODE-SOUND-001/scene-natural-ko-audio-probe.json";
        private const string BattleScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private static Result pendingNatural;

        [Serializable]
        private sealed class Result
        {
            public string status;
            public string error;
            public string scope = "Real Battle Scene audio presentation of synthetic Q10 cue events only.";
            public string scenePath;
            public int tick;
            public int preparedClips;
            public long playsBefore;
            public long playsAfter;
            public long rejectedBefore;
            public long rejectedAfter;
            public bool catalogSealed;
            public bool sceneDirtyAtEntry;
            public int initialBattleMode;
            public int battleMode;
            public bool feedPresentBefore;
            public int attackerSlot;
            public int victimSlot;
            public int victimHpAfterTick;
            public int knockoutCountBefore;
            public int knockoutCountAfter;
            public bool targetKnockoutRecorded;
            public bool targetCueQueued;
        }

        static NTSD28Q10ModeSoundScenePlayProbe()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(RequestPath))
                return;

            string request = File.ReadAllText(RequestPath).Trim();
            if ((request == "run" || request == "natural") &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                bool natural = request == "natural";
                var scene = SceneManager.GetActiveScene();
                if (scene.path != BattleScenePath || scene.isDirty)
                {
                    Save(new Result
                    {
                        status = "FAIL",
                        scenePath = scene.path,
                        sceneDirtyAtEntry = scene.isDirty,
                        error = "Expected a saved, active NTSD_Battle scene."
                    }, natural ? NaturalResultPath : ResultPath);
                    File.WriteAllText(RequestPath, "done");
                    return;
                }

                pendingNatural = null;
                File.WriteAllText(RequestPath,
                    (natural ? "natural-running:" : "running:") + DateTime.UtcNow.Ticks);
                EditorApplication.EnterPlaymode();
                return;
            }

            if (request.StartsWith("natural-running:", StringComparison.Ordinal) ||
                request.StartsWith("natural-dispatching:", StringComparison.Ordinal))
            {
                PollNatural(request);
                return;
            }

            if (!request.StartsWith("running:", StringComparison.Ordinal) ||
                !EditorApplication.isPlaying)
                return;

            var result = new Result { scenePath = SceneManager.GetActiveScene().path };
            try
            {
                long startedTicks = long.Parse(request.Substring("running:".Length));
                if ((DateTime.UtcNow - new DateTime(startedTicks, DateTimeKind.Utc))
                    .TotalSeconds > 120)
                    throw new TimeoutException("Battle scene audio probe readiness timeout.");

                SimulationTickDriver driver = SimulationTickDriver.Instance;
                NTSDSoundPlayer player = UnityEngine.Object.FindObjectOfType<NTSDSoundPlayer>();
                if (driver?.World == null || driver.CurrentTickIndex < 5 ||
                    player == null || !player.BattleCatalogSealedForDiagnostics)
                    return;

                result.tick = driver.CurrentTickIndex;
                result.catalogSealed = true;
                foreach (string cue in new[] { @"data\m_ok.wav", @"data\m_join.wav" })
                {
                    if (!player.TryGetPreparedSingleFileWrapperForDiagnostics(
                            cue, out AudioClip[] clips) || clips == null ||
                        clips.Length != 1 || clips[0] == null)
                        throw new InvalidOperationException("Battle cue not loaded: " + cue);
                    result.preparedClips++;
                }

                result.playsBefore = player.PooledOneShotPlayCountForDiagnostics;
                result.rejectedBefore = player.RejectedUnpreparedCueCountForDiagnostics;
                player.PresentSound(new PendingSoundEvent(@"data\m_ok.wav", 10, result.tick));
                player.PresentSound(new PendingSoundEvent(@"data\m_join.wav", 10, result.tick));
                result.playsAfter = player.PooledOneShotPlayCountForDiagnostics;
                result.rejectedAfter = player.RejectedUnpreparedCueCountForDiagnostics;
                if (result.playsAfter - result.playsBefore != 2 ||
                    result.rejectedAfter != result.rejectedBefore)
                    throw new InvalidOperationException("Selected cues were not played from the sealed catalog.");
                result.status = "PASS";
            }
            catch (Exception error)
            {
                result.status = "FAIL";
                result.error = error.ToString();
            }

            Save(result, ResultPath);
            File.WriteAllText(RequestPath, "done");
            EditorApplication.ExitPlaymode();
        }

        private static void PollNatural(string request)
        {
            if (!EditorApplication.isPlaying)
                return;

            Result result = pendingNatural ?? new Result
            {
                scope = "Real Battle Scene full lethal tick with two temporary test combatants and Play-only forced mode 1; no physical player input.",
                scenePath = SceneManager.GetActiveScene().path
            };
            try
            {
                bool dispatching = request.StartsWith("natural-dispatching:",
                    StringComparison.Ordinal);
                string prefix = dispatching ? "natural-dispatching:" : "natural-running:";
                long startedTicks = long.Parse(request.Substring(prefix.Length));
                if ((DateTime.UtcNow - new DateTime(startedTicks, DateTimeKind.Utc))
                    .TotalSeconds > 120)
                    throw new TimeoutException("Natural KO scene audio probe timeout.");

                SimulationTickDriver driver = SimulationTickDriver.Instance;
                SimulationWorld world = driver?.World;
                NTSDSoundPlayer player = UnityEngine.Object.FindObjectOfType<NTSDSoundPlayer>();
                if (world == null || player == null)
                    return;

                if (dispatching)
                {
                    if (pendingNatural == null)
                        throw new InvalidOperationException("Natural KO probe state was lost.");
                    result.playsAfter = player.PooledOneShotPlayCountForDiagnostics;
                    if (result.playsAfter == result.playsBefore)
                        return;
                    result.rejectedAfter = player.RejectedUnpreparedCueCountForDiagnostics;
                    if (result.rejectedAfter != result.rejectedBefore)
                        throw new InvalidOperationException("The formal KO cue was rejected after seal.");
                    result.status = "PASS";
                }
                else
                {
                    if (driver.CurrentTickIndex < 5 ||
                        !player.BattleCatalogSealedForDiagnostics)
                        return;
                    if (!driver.IsPaused)
                    {
                        driver.SetPaused(true);
                        return;
                    }

                    result.catalogSealed = true;
                    result.initialBattleMode = world.BattleGameModeId;
                    result.feedPresentBefore =
                        world.Runtime?.NativeKnockoutFeed?.RecordPresent == true;
                    if (!result.feedPresentBefore)
                        throw new InvalidOperationException(
                            "The selected formal KO feed was not published to the live World.");
                    world.Runtime.Match.BattleGameModeId = 1;
                    result.battleMode = world.BattleGameModeId;
                    if (result.battleMode != 1)
                        throw new InvalidOperationException(
                            "The controlled Play World did not accept mode 1.");
                    if (!player.TryGetPreparedSingleFileWrapperForDiagnostics(
                            @"data\m_join.wav", out AudioClip[] clips) ||
                        clips == null || clips.Length != 1 || clips[0] == null)
                        throw new InvalidOperationException("Formal group-5 KO cue is not loaded.");
                    result.preparedClips = 1;

                    int victimSlot = world.RuntimeSlotCapacityForDiagnostics - 1;
                    while (victimSlot > 50 &&
                        world.FindEntityByRuntimeSlotForQuery(victimSlot) != null)
                        victimSlot--;
                    int attackerSlot = victimSlot - 1;
                    while (attackerSlot > 50 &&
                        world.FindEntityByRuntimeSlotForQuery(attackerSlot) != null)
                        attackerSlot--;
                    if (attackerSlot <= 50 || attackerSlot == victimSlot)
                        throw new InvalidOperationException("No two free runtime slots for the KO fixture.");
                    result.attackerSlot = attackerSlot;
                    result.victimSlot = victimSlot;

                    MethodInfo create =
                        typeof(NTSD28Q08CombatLethalPrecombatTimingEditorTests).GetMethod(
                            "CreateCombatant", BindingFlags.Static | BindingFlags.NonPublic);
                    if (create == null)
                        throw new InvalidOperationException("The focused lethal fixture is missing.");
                    create.Invoke(null, new object[]
                    {
                        world, attackerSlot, 1, 7100, 2400, true, 500
                    });
                    var victim = (LF2Character)create.Invoke(null, new object[]
                    {
                        world, victimSlot, 5, 7101, 2410, false, 20
                    });
                    if (world.FindEntityByRuntimeSlotForQuery(attackerSlot) == null ||
                        world.FindEntityByRuntimeSlotForQuery(victimSlot) == null)
                        throw new InvalidOperationException("KO fixture registration was rejected.");

                    result.knockoutCountBefore = world.NativeKnockoutEvents.Count;
                    result.playsBefore = player.PooledOneShotPlayCountForDiagnostics;
                    result.rejectedBefore = player.RejectedUnpreparedCueCountForDiagnostics;
                    result.tick = driver.CurrentTickIndex + 1;
                    if (!driver.StepOneTick(ignorePaused: true, buildPresentation: false))
                        throw new InvalidOperationException("The paused live driver rejected the full lethal tick.");
                    result.victimHpAfterTick = victim.Health.HP;
                    result.knockoutCountAfter = world.NativeKnockoutEvents.Count;
                    result.targetKnockoutRecorded = world.NativeKnockoutEvents.Any(
                        hit => hit.VictimSlot == victimSlot &&
                            hit.BattleTimeTick == result.tick);
                    result.targetCueQueued = world.PendingSounds.Any(
                        sound => sound.Cue == @"data\m_join.wav" && sound.Tick == result.tick);
                    if (result.victimHpAfterTick > 0 ||
                        !result.targetKnockoutRecorded || !result.targetCueQueued)
                        throw new InvalidOperationException(
                            "The full scene tick did not produce lethal KO and selected mode cue.");

                    pendingNatural = result;
                    File.WriteAllText(RequestPath,
                        "natural-dispatching:" + DateTime.UtcNow.Ticks);
                    return;
                }
            }
            catch (Exception error)
            {
                result.status = "FAIL";
                result.error = error.ToString();
            }

            Save(result, NaturalResultPath);
            pendingNatural = null;
            File.WriteAllText(RequestPath, "done");
            EditorApplication.ExitPlaymode();
        }

        private static void Save(Result result, string path)
        {
            File.WriteAllText(path, JsonUtility.ToJson(result, true));
        }
    }
}
#endif
