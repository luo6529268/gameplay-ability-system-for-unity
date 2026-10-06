#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NTSD.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28SageKnockbackSceneProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string SessionKey = "NTSD.SageKnockback.Scene.01";
        private static Report report;
        private static SimulationTickDriver driver;
        private static LF2Entity naruto;
        private static LF2Entity target;
        private static CharacterInputModule input;
        private static InputSettings originalInputSettings;
        private static InputSettings probeInputSettings;
        private static bool originalRunInBackground;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static readonly List<LF2Entity> entities = new List<LF2Entity>();

        [Serializable]
        private sealed class Report
        {
            public bool firstRecallOnly;
            public int attempt = 1;
            public int attemptStartTick;
            public int preparedTick;
            public int firstAttemptCompletedTick;
            public bool firstAttemptTransformed;
            public List<SageSample> sageSamples = new List<SageSample>();
            public string runId;
            public string phase;
            public string status;
            public string error;
            public string sceneHash;
            public string configHash;
            public double started;
            public int tick;
            public int skillPhase;
            public int skillPhaseTick;
            public int sageTick;
            public int battleMode;
            public int knockbacks;
            public int hitStartTick;
            public int hpBeforeHit;
            public bool waitingHit;
            public bool airborne;
            public bool sageVisible;
            public bool configured;
            public bool cleanAfter;
            public bool filesUnchanged;
            public bool shutdownComplete;
            public int remainingObjects;
            public int remainingSlots;
            public int remainingBorrowers;
            public double lastProgress;
            public double maxProjectionError;
            public List<string> rows = new List<string>();
        }

        [Serializable]
        private sealed class SageSample
        {
            public int tick;
            public int worldTick;
            public int buttons;
            public int attempt;
            public int mode;
            public int phase;
            public List<SageActor> actors = new List<SageActor>();
        }

        [Serializable]
        private sealed class SageActor
        {
            public int slot, oid, action, prev2, state, counter, owner, team, hp, pp, mp, hitStop;
            public int sourceXInt, sourceZInt, localResource, waived, doubleCost, costMultiplier;
            public double x, y, z, sourceX, sourceZ, vx, vy, vz;
            public bool sourceInitialized;
            public string dir;
        }

        static NTSD28SageKnockbackSceneProbeEditor()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlay;
            SceneManager.sceneLoaded += OnLoaded;
        }

        [MenuItem("NTSD/Validation/Battle/Naruto Sage and P2 Knockback Probe")]
        private static void Run()
        {
            Start(false);
        }

        [MenuItem("NTSD/Validation/Battle/Naruto First Sage Recall Probe")]
        private static void RunFirstRecall()
        {
            Start(true);
        }

        private static void Start(bool firstRecallOnly)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.isDirty ||
                scene.path != ScenePath || EditorApplication.isCompiling)
                throw new InvalidOperationException("Requires idle original saved Battle Scene.");
            report = new Report
            {
                runId = Guid.NewGuid().ToString("N"), phase = "STARTUP", status = "RUNNING",
                firstRecallOnly = firstRecallOnly,
                sceneHash = Hash(ScenePath),
                configHash = Hash("Assets/NTSD/Config/GameConfig/GameConfig.asset"),
                started = EditorApplication.timeSinceStartup
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        private static void Restore()
        {
            if (report == null)
                report = JsonUtility.FromJson<Report>(SessionState.GetString(SessionKey, "{}"));
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report?.phase != "STARTUP" || scene.path != ScenePath ||
                !EditorApplication.isPlaying || report.configured || report.firstRecallOnly)
                return;
            var bootstrap = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            typeof(BattleTestBootstrap).GetField("overrideCharacterIds",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(bootstrap, new[] { 2, 2 });
            report.configured = bootstrap != null;
            SaveSession();
        }

        private static void Update()
        {
            Restore();
            if (report == null || report.status != "RUNNING" || !EditorApplication.isPlaying)
                return;
            try
            {
                if (EditorApplication.timeSinceStartup - report.started > 720)
                    throw new TimeoutException("Owned probe exceeded startup/execution bound.");
                if (EditorApplication.timeSinceStartup - report.lastProgress > 30)
                {
                    report.lastProgress = EditorApplication.timeSinceStartup;
                    NTSD28SageKnockbackRegressionEditorTests.WriteNew("scene-progress", report);
                }
                if (report.phase == "STARTUP")
                {
                    driver = SimulationTickDriver.Instance;
                    if (driver?.World == null || driver.CurrentTickIndex < 5 ||
                        !driver.World.TryResolveRosterInputEntity(0, out naruto) ||
                        !driver.World.TryResolveRosterInputEntity(1, out target) ||
                        !((naruto as LF2Character)?.Controller is CharacterInputModule liveInput) ||
                        liveInput.AttackAction?.enabled != true)
                        return;
                    input = liveInput;
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
                    driver.ApplySettings(new LockstepSimulationSettings { driveMode = SimulationDriveMode.Manual });
                    driver.SetPaused(true);
                    originalInputSettings = InputSystem.settings;
                    originalRunInBackground = Application.runInBackground;
                    probeInputSettings = UnityEngine.Object.Instantiate(originalInputSettings);
                    probeInputSettings.hideFlags = HideFlags.HideAndDontSave;
                    probeInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    probeInputSettings.editorInputBehaviorInPlayMode =
                        InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.settings = probeInputSettings;
                    Application.runInBackground = true;
                    if (!report.firstRecallOnly)
                    {
                        ResetActor(naruto, 500, 650);
                        ResetActor(target, 1200, 650);
                        driver.World.Runtime.FunctionKeys.ResetForBattle(true);
                        driver.World.Runtime.Flow.FrameToggle = 0;
                        driver.World.Runtime.Flow.InputPhase = 0;
                        driver.World.Runtime.NativeWorldClock.Reset();
                        driver.World.NativeRandom.ResetFromSeed(682973786u);
                        driver.World.Runtime.Match.Difficulty = 0;
                    }
                    else
                    {
                        Require(naruto.ObjectId == 2, "Original P1 must be Naruto for this report.");
                        CaptureSageSample(SimulationInputButtons.None);
                    }
                    report.battleMode = driver.World.BattleGameModeId;
                    report.phase = "SAGE";
                    NTSD28SageKnockbackRegressionEditorTests.WriteNew("scene-ready", report);
                    SaveSession();
                    if (report.firstRecallOnly)
                    {
                        QueuePhysical(SimulationInputButtons.None);
                        return;
                    }
                }
                if (report.status != "RUNNING")
                    return;
                SimulationInputButtons desired = report.phase == "SAGE"
                    ? SageInput() : SimulationInputButtons.None;
                QueuePhysical(desired);
                SimulationInputButtons actual = ((ILocalFrameInputSource)input).CaptureHeldSimulationButtons();
                Require(actual == desired,
                    "Physical binding capture mismatch: " + actual + " vs " + desired);
                Step(actual);
                SaveSession();
            }
            catch (Exception exception)
            {
                report.error = exception.ToString();
                Finish(false);
            }
        }

        private static SimulationInputButtons SageInput()
        {
            int next = report.tick + 1;
            if (report.firstRecallOnly && report.skillPhase == 2 && report.preparedTick > 0 &&
                report.tick - report.preparedTick >= 60)
            {
                report.skillPhase = 3;
                report.skillPhaseTick = report.tick;
            }
            if (report.skillPhase == 0 && naruto.Frame.N == 414)
            {
                report.skillPhase = 1;
                report.skillPhaseTick = next;
            }
            if (report.skillPhase == 3 && naruto.Frame.N == 414)
            {
                report.skillPhase = 4;
                report.skillPhaseTick = next;
            }
            switch (report.skillPhase)
            {
                case 0: return NTSD28SageKnockbackRegressionEditorTests.SkillButton(next - report.attemptStartTick);
                case 1:
                    if (next - report.skillPhaseTick < 2) return SimulationInputButtons.Defend;
                    report.skillPhase = 2;
                    return SimulationInputButtons.None;
                case 3: return NTSD28SageKnockbackRegressionEditorTests.SkillButton(next - report.skillPhaseTick);
                case 2:
                    return report.firstRecallOnly && report.preparedTick > 0 &&
                        next - report.preparedTick <= 24
                        ? SimulationInputButtons.Down : SimulationInputButtons.None;
                case 4:
                    if (next - report.skillPhaseTick < 2) return SimulationInputButtons.Attack;
                    report.skillPhase = 5;
                    return SimulationInputButtons.None;
                default: return SimulationInputButtons.None;
            }
        }

        private static void Step(SimulationInputButtons buttons)
        {
            int tick = driver.CurrentTickIndex + 1;
            Require(driver.StepOneTick(new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(0, buttons),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            }), ignorePaused: true, buildPresentation: true), "Complete Driver tick was rejected.");
            report.tick++;
            SimulationWorld world = driver.World;
            world.GetAllEntities(entities);
            LF2Entity clone = entities.FirstOrDefault(value => value.ObjectId == 96);
            if (report.firstRecallOnly)
            {
                CaptureSageSample(buttons);
                Require(report.tick - report.attemptStartTick < 500,
                    "First/second natural Sage attempt exceeded the authored bound.");
            }
            if (report.phase == "SAGE")
            {
                if (report.skillPhase == 2 && clone != null &&
                    new[] { 323, 326, 327, 328, 329 }.Contains(clone.Frame.N))
                {
                    if (report.firstRecallOnly)
                    {
                        if (report.preparedTick == 0)
                            report.preparedTick = report.tick;
                    }
                    else
                    {
                        report.skillPhase = 3;
                        report.skillPhaseTick = report.tick;
                    }
                }
                report.rows.Add($"sage t{report.tick} key{(int)buttons} oid{naruto.ObjectId} a{naruto.Frame.N} clone{clone?.Frame.N} owner{clone?.Runtime.OwnerSlotIndex} mode{world.BattleGameModeId}");
                if (!report.firstRecallOnly)
                    Require(report.tick < 400, "Physical Sage skill did not complete within the authored preparation bound.");
                if (naruto.ObjectId == 99)
                {
                    report.sageTick = report.tick;
                    var plan = BattleCentralRenderSystem.PrepareFrame(world);
                    BattlePresentationFrame captured = plan.IsValid && !plan.IsStale ? plan.CapturedFrame : null;
                    if (captured != null)
                    {
                        for (int index = 0; index < captured.EntityCount; index++)
                        {
                            var snapshot = captured.GetEntity(index);
                            if (snapshot.Handle.Slot == naruto.Runtime.SlotIndex)
                                report.sageVisible = snapshot.CurrentDatObjectId == 99 && snapshot.EntityVisible;
                        }
                        report.sageVisible &= Enumerable.Range(0, captured.CommandCount).Any(index =>
                            captured.GetCommand(index).Handle.Slot == naruto.Runtime.SlotIndex &&
                            captured.GetCommand(index).Type == BattleRenderCommandType.Entity);
                    }
                    Require(report.sageVisible, "Sage logical identity changed but no Sage visible central body was materialized.");
                    if (report.firstRecallOnly)
                    {
                        report.firstAttemptTransformed = report.attempt == 1;
                        report.firstAttemptCompletedTick = report.attempt == 1
                            ? report.tick : report.firstAttemptCompletedTick;
                        Finish(true);
                        return;
                    }
                    naruto.ImmediateFrame(412);
                    naruto.Frame.Prev2 = 412;
                    naruto.Frame.Prev2D = naruto.Frame.D;
                    report.phase = "REVERT";
                }
                else if (report.firstRecallOnly && report.skillPhase == 5 &&
                    report.tick - report.skillPhaseTick > 60 && naruto.Frame.N < 4)
                {
                    if (report.attempt == 1)
                    {
                        report.firstAttemptCompletedTick = report.tick;
                        report.attempt = 2;
                        report.attemptStartTick = report.tick;
                        report.preparedTick = 0;
                        report.skillPhase = 0;
                        report.skillPhaseTick = 0;
                    }
                    else
                    {
                        Finish(true);
                    }
                }
            }
            else if (report.phase == "REVERT")
            {
                Require(naruto.ObjectId == 2, "Authored Sage revert did not return to Naruto.");
                report.phase = "KNOCKBACK";
                StartHit();
            }
            else if (report.phase == "KNOCKBACK")
            {
                MeasureTarget();
                if (report.waitingHit && target.Runtime.HP < report.hpBeforeHit)
                {
                    report.waitingHit = false;
                    report.knockbacks++;
                }
                report.airborne |= target.Runtime.YInt < -5;
                if (report.waitingHit)
                    Require(report.tick - report.hitStartTick < 12, "Uppercut did not hit P2.");
                if (!report.waitingHit && report.airborne && target.Runtime.YInt == 0 &&
                    target.HitStun == 0 && report.tick - report.hitStartTick > 120)
                {
                    if (report.knockbacks == 3)
                        Finish(true);
                    else
                        StartHit();
                }
                Require(report.tick - report.hitStartTick < 220, "P2 did not finish natural airborne recovery.");
            }
        }

        private static void StartHit()
        {
            double x = target.Runtime.SourceRuleX;
            double z = target.Runtime.SourceRuleZ;
            ResetActor(naruto, x, z);
            naruto.ImmediateFrame(411);
            naruto.Frame.PN = naruto.Frame.Prev = naruto.Frame.Prev2 = 411;
            naruto.Frame.Prev2D = naruto.Frame.D;
            naruto.Runtime.PrevFrame2 = 411;
            naruto.Trans.SyncWaitCounterFrame(411);
            report.hpBeforeHit = target.Runtime.HP;
            report.hitStartTick = report.tick;
            report.waitingHit = true;
            report.airborne = false;
        }

        private static void CaptureSageSample(SimulationInputButtons buttons)
        {
            driver.World.GetAllEntities(entities);
            var sample = new SageSample
            {
                tick = report.tick, worldTick = driver.CurrentTickIndex,
                buttons = (int)buttons, attempt = report.attempt,
                mode = driver.World.BattleGameModeId,
                phase = driver.World.Runtime.Flow.InputPhase
            };
            foreach (LF2Entity entity in entities)
            {
                if (entity.Runtime.SlotIndex != 0 && entity.ObjectId != 96)
                    continue;
                var r = entity.Runtime;
                sample.actors.Add(new SageActor
                {
                    slot = r.SlotIndex, oid = entity.ObjectId, action = entity.Frame.N,
                    prev2 = entity.Frame.Prev2, state = entity.Frame.D?.state ?? -1,
                    counter = r.AttackingCounter, owner = r.OwnerSlotIndex,
                    team = r.RelationTeam, hp = r.HP, pp = r.PP, mp = r.MP,
                    hitStop = r.HitStop, sourceInitialized = r.SourceRulePositionInitialized,
                    x = r.X, y = r.Y, z = r.Z, sourceX = r.SourceRuleX,
                    sourceZ = r.SourceRuleZ, sourceXInt = r.SourceRuleXInt,
                    sourceZInt = r.SourceRuleZInt, vx = r.Vx, vy = r.Vy, vz = r.Vz,
                    dir = r.Dir, localResource = r.InputLocalResourceEnabled49D034 ? 1 : 0,
                    waived = r.InputCostWaived1B4, doubleCost = r.InputDoubleCost19C,
                    costMultiplier = r.InputModeCostMultiplier30
                });
            }
            report.sageSamples.Add(sample);
        }

        private static void MeasureTarget()
        {
            var runtime = target.Runtime;
            BattleSpatialProjection projection = driver.World.SpatialProjection;
            double error = Math.Max(Math.Abs(runtime.X - projection.SourceToViewX(runtime.SourceRuleX)),
                Math.Abs(runtime.Z - projection.SourceToViewZ(runtime.SourceRuleZ)));
            report.maxProjectionError = Math.Max(report.maxProjectionError, error);
            Require(error < 1e-6, "P2 source/view position diverged after knockback: " + error);
            LF2FrameData frame = target.GetCollisionFrameData();
            foreach (BattleBodyBoxValue body in frame.bodies)
            {
                if (!NTSDHitboxGizmos.TryBuildBodyVolumeForDiagnostics(target, frame, body, out var volume))
                    continue;
                Rect rect = NTSDHitboxGizmos.GetScreenRectForDiagnostics(target, volume);
                Require(Math.Abs(rect.yMin - (volume.y + volume.vy + volume.z)) < 0.001,
                    "P2 debug rectangle lost current depth.");
            }
            report.rows.Add($"p2 t{report.tick} hits{report.knockbacks} a{target.Frame.N}/prev2{target.Frame.Prev2} xyz{runtime.X},{runtime.Y},{runtime.Z} source{runtime.SourceRuleX},{runtime.SourceRuleZ} hp{runtime.HP} err{error}");
        }

        private static void ResetActor(LF2Entity entity, double x, double z)
        {
            var runtime = entity.Runtime;
            runtime.SourceRuleX = x;
            runtime.SourceRuleZ = z;
            runtime.SourceRulePositionInitialized = true;
            runtime.X = driver.World.SpatialProjection.SourceToViewX(x);
            runtime.Y = 0;
            runtime.Z = driver.World.SpatialProjection.SourceToViewZ(z);
            runtime.Vx = runtime.Vy = runtime.Vz = 0;
            runtime.KnockbackVx = runtime.KnockbackVy = runtime.KnockbackVz = 0;
            runtime.MP = runtime.PP = 500;
            entity.HitStun = 0;
            entity.SwitchDir("right");
            entity.ImmediateFrame(0);
            entity.Frame.PN = entity.Frame.Prev = entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Trans.SyncWaitCounterFrame(0);
            runtime.AttackingCounter = 0;
            runtime.SyncIntegerPosition();
            runtime.SyncSourceRuleIntegerPosition();
        }

        private static void QueuePhysical(SimulationInputButtons buttons)
        {
            var keys = new List<Key>();
            if ((buttons & SimulationInputButtons.Attack) != 0)
                keys.Add(input.DefendAction.controls.OfType<KeyControl>().First().keyCode);
            if ((buttons & SimulationInputButtons.Jump) != 0)
                keys.Add(input.AttackAction.controls.OfType<KeyControl>().First().keyCode);
            if ((buttons & SimulationInputButtons.Defend) != 0)
                keys.Add(input.JumpAction.controls.OfType<KeyControl>().First().keyCode);
            if ((buttons & SimulationInputButtons.Up) != 0)
            {
                string path = input.MoveAction.bindings.First(value => value.isPartOfComposite &&
                    string.Equals(value.name, "up", StringComparison.OrdinalIgnoreCase)).effectivePath;
                keys.Add(((KeyControl)InputSystem.FindControl(path)).keyCode);
            }
            if ((buttons & SimulationInputButtons.Down) != 0)
            {
                string path = input.MoveAction.bindings.First(value => value.isPartOfComposite &&
                    string.Equals(value.name, "down", StringComparison.OrdinalIgnoreCase)).effectivePath;
                keys.Add(((KeyControl)InputSystem.FindControl(path)).keyCode);
            }
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(keys.ToArray()));
            InputSystem.Update();
        }

        private static void Finish(bool passed)
        {
            if (Keyboard.current != null)
            {
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                InputSystem.Update();
            }
            NTSD28SageKnockbackRegressionEditorTests.WriteNew("scene-before-shutdown", report);
            var shutdown = driver?.ShutdownBattleRuntime();
            if (shutdown.HasValue && shutdown.Value.RuntimeStagesCompleted)
            {
                bool mapCleared = true;
                foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                {
                    if (bootstrap == null || EditorUtility.IsPersistent(bootstrap) ||
                        !bootstrap.gameObject.scene.IsValid())
                        continue;
                    bootstrap.DisablePresentation();
                    mapCleared &= bootstrap.IsRuntimeMapCleared;
                }
                shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
            }
            if (originalInputSettings != null)
            {
                InputSystem.settings = originalInputSettings;
                Application.runInBackground = originalRunInBackground;
                originalInputSettings = null;
                UnityEngine.Object.DestroyImmediate(probeInputSettings);
                probeInputSettings = null;
            }
            if (shutdown.HasValue)
            {
                report.remainingObjects = shutdown.Value.RemainingWorldObjects;
                report.remainingSlots = shutdown.Value.RemainingRuntimeSlots;
                report.remainingBorrowers = shutdown.Value.RemainingPoolBorrowers;
            }
            report.shutdownComplete = shutdown.HasValue && shutdown.Value.IsComplete &&
                driver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                report.remainingObjects == 0 && report.remainingSlots == 0 && report.remainingBorrowers == 0;
            report.status = passed && report.shutdownComplete ? "PASS" : "FAIL";
            report.phase = "EXITING";
            SaveSession();
            if (report.shutdownComplete)
                EditorApplication.ExitPlaymode();
        }

        private static void OnPlay(PlayModeStateChange state)
        {
            Restore();
            if (report?.phase != "EXITING" || state != PlayModeStateChange.EnteredEditMode)
                return;
            report.cleanAfter = !SceneManager.GetActiveScene().isDirty;
            report.filesUnchanged = Hash(ScenePath) == report.sceneHash &&
                Hash("Assets/NTSD/Config/GameConfig/GameConfig.asset") == report.configHash;
            report.phase = "DONE";
            if (!report.cleanAfter || !report.filesUnchanged)
                report.status = "FAIL";
            NTSD28SageKnockbackRegressionEditorTests.WriteNew("scene", report);
            SaveSession();
        }

        private static void SaveSession() => SessionState.SetString(SessionKey, JsonUtility.ToJson(report));

        private static string Hash(string path)
        {
            using (SHA256 hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
