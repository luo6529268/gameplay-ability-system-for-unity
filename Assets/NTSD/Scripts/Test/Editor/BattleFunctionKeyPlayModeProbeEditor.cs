#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Production fixed-tick witness for mode-configured F7/F8/F9 requests.
    /// Alignment contract: R8-FUNCTIONKEYMODE-001.
    /// </summary>
    public static class BattleFunctionKeyPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run F7 F8 F9 Play Probe";
        private const string RequestRelativePath =
            "Temp/NTSD_R8_WP01G_R12_FunctionKeys.request";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01G_R12_FunctionKeys.result.json";
        private const int ProbeOid = 150;
        private const int DynamicSlotStart = 50;
        private const int TimeoutEditorUpdates = 2400;

        private static readonly List<LF2Entity> BaselineEntities =
            new List<LF2Entity>(128);
        private static readonly List<LF2Entity> EntityScratch =
            new List<LF2Entity>(256);
        private static readonly List<EntityMutableState> BaselineMutableState =
            new List<EntityMutableState>(128);
        private static readonly List<LF2Entity> SpawnedWeapons =
            new List<LF2Entity>(128);
        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(16);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPointFactory factory;
        private static LF2ObjectPool objectPool;
        private static ProbeReport report;
        private static bool previousPaused;
        private static bool running;
        private static bool pauseRequested;
        private static int editorUpdates;
        private static int baselineObjectCount;
        private static int baselineClaimedSlots;
        private static int baselineObjectPoolActive;
        private static int baselineLogicPoolActive;
        private static int baselineExitCountdown;
        private static int baselineLocalGameModeId;
        private static int baselineBattleGameModeId;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || running)
            {
                return;
            }

            string requestPath = ProjectPath(RequestRelativePath);
            if (!File.Exists(requestPath))
                return;
            File.Delete(requestPath);
            RunFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            ResetState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            factory = LF2ObjectPointFactory.Instance;
            objectPool = LF2ObjectPool.Instance;
            if (driver == null || world == null || factory == null ||
                objectPool == null || LF2ReferencePool.Instance == null ||
                world.RuntimeDataCatalog?.IsReady != true || GameConfig.Instance == null)
            {
                WriteImmediateFailure("Production driver, config, catalog, factory, or pools are not ready.");
                return;
            }

            BattleMatchRuntimeState match = world.Runtime.Match;
            BattleFunctionKeyCommand allowed =
                GameConfig.Instance.ResolveBattleFunctionKeyCommands(
                    0,
                    1);
            BattleFunctionKeyCommand required =
                BattleFunctionKeyCommand.InitializeStats |
                BattleFunctionKeyCommand.SpawnAllWeapons |
                BattleFunctionKeyCommand.ClearWeaponPicker;
            if ((allowed & required) != required)
            {
                WriteImmediateFailure(
                    "Standard local battle mode 0/1 does not enable F7/F8/F9 in GameConfig.");
                return;
            }

            previousPaused = driver.IsPaused;
            report = new ProbeReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                localGameModeId = 0,
                battleGameModeId = 1,
            };
            running = true;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                Fail("Play Mode or production world ended before completion.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > TimeoutEditorUpdates)
            {
                Fail("Timed out waiting for a safe production tick boundary.");
                return;
            }

            if (!pauseRequested)
            {
                if (driver.CurrentTickIndex < 5 ||
                    driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                {
                    return;
                }

                driver.SetPaused(true);
                pauseRequested = true;
                CaptureBaseline();
                return;
            }

            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            try
            {
                ExecuteProbe();
                FinishSuccess();
            }
            catch (Exception exception)
            {
                Fail(exception.ToString());
            }
        }

        private static void ExecuteProbe()
        {
            LF2Entity statsProbe = SpawnProbeWeapon();
            statsProbe.Health.HP3 = 11;
            statsProbe.Health.HPBound = 22;
            statsProbe.Health.HP = 17;
            statsProbe.Health.PP = 9;
            world.SetBattleExitCountdown(77);

            driver.QueueBattleFunctionKeyCommandsForDiagnostics(
                BattleFunctionKeyCommand.InitializeStats);
            int f7Tick = Step();
            Require(
                statsProbe.Health.HP3 == 500 &&
                statsProbe.Health.HPBound == 500 &&
                statsProbe.Health.HP == 500 &&
                statsProbe.Health.PP == 500,
                "F7 did not apply HP3/HPBound/HP/PP=500 in entity postframe.");
            Require(world.InitStatsRequest == 0 && world.Mode2Request == 0 &&
                    world.Runtime.Flow.BattleExitCountdown == 0,
                "F7 request/exit-countdown clear boundary mismatch.");

            world.GetAllEntities(EntityScratch);
            var beforeF8 = new HashSet<LF2Entity>(EntityScratch);
            driver.QueueBattleFunctionKeyCommandsForDiagnostics(
                BattleFunctionKeyCommand.SpawnAllWeapons);
            int f8Tick = Step();
            world.GetAllEntities(EntityScratch);
            SpawnedWeapons.Clear();
            for (int index = 0; index < EntityScratch.Count; index++)
            {
                LF2Entity entity = EntityScratch[index];
                if (!beforeF8.Contains(entity) &&
                    entity?.CountsAsRandomWeaponDropCandidate() == true)
                {
                    SpawnedWeapons.Add(entity);
                }
            }
            Require(SpawnedWeapons.Count > 0,
                "F8 fixed-tick request did not use the production mode2 weapon spawn chain.");
            Require(world.Mode2Request == 0,
                "F8 mode2 request was not cleared after the postframe tail.");

            driver.QueueBattleFunctionKeyCommandsForDiagnostics(
                BattleFunctionKeyCommand.ClearWeaponPicker);
            int f9Tick = Step();
            int clearedCount = 0;
            int eligibleCount = 0;
            int transitionedCount = 0;
            for (int index = 0; index < SpawnedWeapons.Count; index++)
            {
                LF2Entity weapon = SpawnedWeapons[index];
                if (weapon == null || weapon.Match != world ||
                    weapon.Runtime?.SlotIndex < 0 ||
                    !weapon.CountsAsRandomWeaponDropCandidate())
                {
                    transitionedCount++;
                    continue;
                }

                eligibleCount++;
                if (weapon?.Runtime?.WeaponFlightCounter == -1)
                    clearedCount++;
            }
            Require(eligibleCount > 0 && clearedCount == eligibleCount,
                $"F9 cleared {clearedCount}/{eligibleCount} still-eligible weapon pickers; " +
                $"transitioned={transitionedCount}.");
            Require(world.Mode2Request == 0,
                "F9 mode2 request was not cleared after the postframe tail.");

            report.f7Tick = f7Tick;
            report.f8Tick = f8Tick;
            report.f9Tick = f9Tick;
            report.f8SpawnedWeaponCount = SpawnedWeapons.Count;
            report.f9EligibleWeaponCount = eligibleCount;
            report.f9TransitionedWeaponCount = transitionedCount;
            report.f9ClearedWeaponCount = clearedCount;
            report.f7StatsMatched = true;
            report.requestsCleared = true;
        }

        private static int Step()
        {
            int tick = driver.CurrentTickIndex + 1;
            Require(driver.StepOneTick(
                    FrameInputSet.Empty(tick),
                    ignorePaused: true,
                    buildPresentation: false),
                $"Production full tick {tick} was rejected.");
            return tick;
        }

        private static LF2Entity SpawnProbeWeapon()
        {
            int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                DynamicSlotStart,
                world.RuntimeSlotCapacityForDiagnostics);
            Require(slot >= DynamicSlotStart, "No free slot for the F7 production weapon witness.");
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                kind = 1,
                oid = ProbeOid,
                action = 0,
                facing = 0,
            };
            task.targetWorld = world;
            task.requiredRuntimeSlot = slot;
            task.dir = "right";
            task.preserveActionZero = true;
            task.skipPostInitZOffset = true;
            task.useDirectRuntimePosition = true;
            task.directX = 400;
            task.directY = -200;
            task.directZ = 260;
            LF2Entity entity;
            try
            {
                entity = factory.CreateObjectImmediate(task);
            }
            finally
            {
                LF2ReferencePool.Instance.Recycle(task);
            }
            Require(entity != null && entity.Runtime.SlotIndex == slot,
                "Production factory failed to create the F7 weapon witness.");
            return entity;
        }

        private static void CaptureBaseline()
        {
            world.GetAllEntities(BaselineEntities);
            BaselineMutableState.Clear();
            for (int index = 0; index < BaselineEntities.Count; index++)
                BaselineMutableState.Add(new EntityMutableState(BaselineEntities[index]));
            baselineObjectCount = world.ObjectCount;
            baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            baselineObjectPoolActive = objectPool.ActiveObjectCountForAcceptance;
            baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineExitCountdown = world.Runtime.Flow.BattleExitCountdown;
            baselineLocalGameModeId = world.Runtime.Match.LocalGameModeId;
            baselineBattleGameModeId = world.Runtime.Match.BattleGameModeId;
            world.Runtime.Match.LocalGameModeId = 0;
            world.Runtime.Match.BattleGameModeId = 1;
            baselineRngState = world.Rng.State;
            baselineRngCalls = world.Rng.CallCount;
            BaselineSounds.Clear();
            BaselineSounds.AddRange(world.PendingSounds);
        }

        private static void Cleanup()
        {
            if (world == null)
                return;
            try
            {
                world.GetAllEntities(EntityScratch);
                for (int index = EntityScratch.Count - 1; index >= 0; index--)
                {
                    LF2Entity entity = EntityScratch[index];
                    if (entity != null && !BaselineEntities.Contains(entity) &&
                        entity.Match == world && entity.Runtime?.SlotIndex >= 0)
                    {
                        entity.FreeEntityLikeExe();
                    }
                }
                world.FlushPendingDestroyForDiagnostics();
                for (int index = 0; index < BaselineMutableState.Count; index++)
                    BaselineMutableState[index].Restore(world);
                world.SetInitStatsRequest(0);
                world.SetMode2Request(0);
                world.SetBattleExitCountdown(baselineExitCountdown);
                world.Runtime.Match.LocalGameModeId = baselineLocalGameModeId;
                world.Runtime.Match.BattleGameModeId = baselineBattleGameModeId;
                world.PendingSounds.Clear();
                world.PendingSounds.AddRange(BaselineSounds);
                world.Rng.RestoreState(baselineRngState, baselineRngCalls);
                world.RenderDispatchAll(driver?.CurrentTickIndex ?? 0, true);
            }
            catch (Exception exception)
            {
                report.cleanupErrors += exception.Message + ";";
            }
            if (driver != null && EditorApplication.isPlaying)
                driver.SetPaused(previousPaused);
        }

        private static void FinishSuccess()
        {
            report.status = "PASS";
            report.message = "Mode-configured F7/F8/F9 fixed-tick production commands passed.";
            report.endTick = driver.CurrentTickIndex;
            Cleanup();
            CaptureFinalState();
            Require(report.cleanupCompleted, "Cleanup did not restore the production baseline: " + report.cleanupErrors);
            WriteResult(report);
            Debug.Log("[BattleFunctionKeyPlayModeProbe] PASS");
            StopObservation();
        }

        private static void Fail(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.message = message;
            report.endTick = driver?.CurrentTickIndex ?? -1;
            Cleanup();
            CaptureFinalState();
            WriteResult(report);
            Debug.LogError("[BattleFunctionKeyPlayModeProbe] FAIL: " + message);
            StopObservation();
        }

        private static void CaptureFinalState()
        {
            if (report == null)
                return;
            report.finalObjectCount = world?.ObjectCount ?? -1;
            report.finalClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.finalObjectPoolActive = objectPool?.ActiveObjectCountForAcceptance ?? -1;
            report.finalLogicPoolActive = LF2ReferencePool.Instance?.ActiveCount ?? -1;
            report.cleanupCompleted = string.IsNullOrEmpty(report.cleanupErrors) &&
                report.finalObjectCount == baselineObjectCount &&
                report.finalClaimedSlots == baselineClaimedSlots &&
                report.finalObjectPoolActive == baselineObjectPoolActive &&
                report.finalLogicPoolActive == baselineLogicPoolActive;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeReport { status = "FAIL", message = message });
            Debug.LogError("[BattleFunctionKeyPlayModeProbe] FAIL: " + message);
        }

        private static void WriteResult(ProbeReport value)
        {
            string path = ProjectPath(ResultRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            running = false;
        }

        private static void ResetState()
        {
            driver = null;
            world = null;
            factory = null;
            objectPool = null;
            report = null;
            previousPaused = false;
            running = false;
            pauseRequested = false;
            editorUpdates = 0;
            baselineObjectCount = 0;
            baselineClaimedSlots = 0;
            baselineObjectPoolActive = 0;
            baselineLogicPoolActive = 0;
            baselineExitCountdown = 0;
            baselineLocalGameModeId = 0;
            baselineBattleGameModeId = 0;
            baselineRngState = 0;
            baselineRngCalls = 0;
            BaselineEntities.Clear();
            EntityScratch.Clear();
            BaselineMutableState.Clear();
            SpawnedWeapons.Clear();
            BaselineSounds.Clear();
        }

        private readonly struct EntityMutableState
        {
            private readonly LF2Entity entity;
            private readonly int hp;
            private readonly int hpBound;
            private readonly int hp3;
            private readonly int pp;
            private readonly int weaponFlightCounter;

            internal EntityMutableState(LF2Entity source)
            {
                entity = source;
                hp = source?.Health?.HP ?? 0;
                hpBound = source?.Health?.HPBound ?? 0;
                hp3 = source?.Health?.HP3 ?? 0;
                pp = source?.Health?.PP ?? 0;
                weaponFlightCounter = source?.Runtime?.WeaponFlightCounter ?? 0;
            }

            internal void Restore(SimulationWorld activeWorld)
            {
                if (entity == null || entity.Match != activeWorld ||
                    entity.Runtime?.SlotIndex < 0 || entity.Health == null)
                {
                    return;
                }
                entity.Health.HP = hp;
                entity.Health.HPBound = hpBound;
                entity.Health.HP3 = hp3;
                entity.Health.PP = pp;
                entity.Runtime.WeaponFlightCounter = weaponFlightCounter;
                entity.RefreshRuntimeSnapshot();
            }
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public int startTick;
            public int endTick;
            public int localGameModeId;
            public int battleGameModeId;
            public int f7Tick;
            public int f8Tick;
            public int f9Tick;
            public bool f7StatsMatched;
            public int f8SpawnedWeaponCount;
            public int f9EligibleWeaponCount;
            public int f9TransitionedWeaponCount;
            public int f9ClearedWeaponCount;
            public bool requestsCleared;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
        }
    }
}
#endif
