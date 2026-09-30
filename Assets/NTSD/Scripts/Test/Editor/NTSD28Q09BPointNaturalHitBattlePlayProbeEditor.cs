#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q09BPointNaturalHitBattlePlayProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q09_BPointNaturalHit.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-BPOINT-NATURAL-HIT-PLAY-001";
        private const int InitialHp = 180;
        private const int BleedThreshold = 166;
        private const int MaxTicks = 80;

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character attacker;
        private static LF2Character target;
        private static Keyboard keyboard;
        private static Report report;
        private static readonly StringBuilder TickTrace = new StringBuilder(8192);
        private static int phase;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int expectedTick = -1;
        private static int stepped;
        private static long activeStartedUtcTicks;
        private static bool savedPaused;
        private static bool pauseCaptured;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public long startedUtcTicks;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string scope;
            public string runId;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public int attackerOid;
            public int targetOid;
            public int targetSlot;
            public int initialHp;
            public int finalHp;
            public int firstDamageTick = -1;
            public int markTick = -1;
            public int markFrame = -1;
            public int appliedPhysicalAttackTicks;
            public bool authoredAttackSeen;
            public int markCount;
            public int bodyCommandIndex = -1;
            public int markCommandIndex = -1;
            public int markWidth;
            public int markHeight;
            public int completedTicks;
            public string tickTrace;
            public int objectsBefore;
            public int objectsAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int borrowersBefore;
            public int borrowersAfter;
            public bool targetUnregistered;
        }

        static NTSD28Q09BPointNaturalHitBattlePlayProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            string path = ProjectPath(RequestPath);
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(path))
                return;
            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || (!request.requested && !request.running))
                return;
            if (request.requested && !request.running)
            {
                Scene scene = SceneManager.GetActiveScene();
                if (EditorApplication.isPlayingOrWillChangePlaymode ||
                    !ValidRunId(request.runId) || scene.path != ScenePath ||
                    scene.isDirty || File.Exists(ResultPath(request.runId)))
                {
                    Finish(request, new Report
                    {
                        status = "FAIL",
                        error = "Original clean Battle Scene, unique runId and idle Editor are required.",
                    });
                    return;
                }
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(path, JsonUtility.ToJson(request));
                EditorApplication.EnterPlaymode();
                return;
            }
            if (!request.running || !EditorApplication.isPlaying)
                return;
            if (report == null)
            {
                report = new Report
                {
                    status = "FAIL",
                    runId = request.runId,
                    scope = "Original Battle Scene full Driver physical J against a temporary formal Ita; CentralOnly command witness, not formal EXE pixels.",
                };
            }
            try
            {
                if (phase == 0)
                {
                    Require(DateTime.UtcNow - new DateTime(request.startedUtcTicks,
                        DateTimeKind.Utc) < TimeSpan.FromMinutes(8),
                        "Original Battle Scene content bootstrap timed out.");
                    Prepare();
                }
                else
                {
                    Require(activeStartedUtcTicks > 0 &&
                        DateTime.UtcNow - new DateTime(activeStartedUtcTicks,
                            DateTimeKind.Utc) < TimeSpan.FromMinutes(3),
                        "Natural hit observation timed out.");
                    StepAndObserve(request);
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
                CleanupAndFinish(request);
            }
        }

        private static void Prepare()
        {
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 ||
                !world.IsBattleSnapshotBoundaryReady)
                return;
            Require(SceneManager.GetActiveScene().path == ScenePath,
                "Original Battle Scene changed during Play.");
            Require(GameConfig.Instance?.BattleContentRuntimeRoot == FormalRoot,
                "Formal content root is not selected.");
            Require(driver.PresentationBackendMode ==
                BattlePresentationBackendMode.CentralOnly,
                "CentralOnly backend is required.");
            Require(CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                "Formal content has not been published.");
            Require(!driver.DedicatedSimulationWorkerActiveForDiagnostics,
                "This physical-key diagnostic requires the current inline Driver.");
            if (!pauseCaptured)
            {
                savedPaused = driver.IsPaused;
                pauseCaptured = true;
                driver.SetPaused(true);
                return;
            }
            if (!driver.IsPaused ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 4)
                return;

            Require(world.TryResolveRosterInputEntity(0, out LF2Entity p1),
                "No bound P1 input entity is available.");
            attacker = p1 as LF2Character;
            Require(attacker?.ObjectId == 2 &&
                (attacker.Controller as CharacterInputModule)?.AttackAction?.enabled == true &&
                attacker.Runtime.SourceRulePositionInitialized,
                "The saved Battle Scene has no ready P1 Naruto attack actor.");
            keyboard = Keyboard.current;
            Require(keyboard != null, "No Input System keyboard is available.");
            LF2CharacterDataWrapper data = world.RuntimeCharacterConfigs.Resolve(9);
            Require(data?.characterData != null,
                "Formal Ita OID9 definition is unavailable.");
            int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
            Require(slot >= 50, "No free runtime slot for Ita fixture.");

            report.sceneHashBefore = HashFile(ProjectPath(ScenePath));
            report.objectsBefore = world.ObjectCount;
            report.slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.borrowersBefore = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            target = new LF2Character();
            target.ModuleInitialize();
            target.ObjectId = 9;
            target.Name = "Q09NaturalBPointIta";
            target.FrameCache.Load(data);
            target.SetRequiredRuntimeSlot(slot);
            world.Register(target);
            target.ImmediateFrame(0);
            target.Initialize(500, 500);
            target.AiControlled = false;
            target.Team = 2;
            target.RelationTeam = 2;
            int direction = attacker.Runtime.IsFacingLeft ? -1 : 1;
            int x = attacker.Runtime.XInt + direction * 40;
            int z = attacker.Runtime.ZInt;
            target.Runtime.SetPosition(x, attacker.Runtime.YInt, z);
            target.Runtime.SetSourceRulePosition(
                attacker.Runtime.SourceRuleXInt + direction * 40,
                attacker.Runtime.SourceRuleZInt);
            target.Runtime.SetVelocity(0, 0, 0);
            target.Runtime.SyncIntegerPosition();
            target.Runtime.SyncSourceRuleIntegerPosition();
            target.Health.HP = InitialHp;
            target.RefreshRuntimeSnapshot();
            Require(target.Frame.D?.BloodPoints?.Count == 1 &&
                target.Runtime.HP == InitialHp &&
                target.Runtime.HP3 / 3 == BleedThreshold,
                "The formal Ita threshold/standing bpoint fixture is incomplete.");

            report.attackerOid = attacker.ObjectId;
            report.targetOid = target.ObjectId;
            report.targetSlot = target.Runtime.SlotIndex;
            report.initialHp = target.Runtime.HP;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
            InputSystem.Update();
            activeStartedUtcTicks = DateTime.UtcNow.Ticks;
            phase = 1;
        }

        private static void StepAndObserve(Request request)
        {
            Require(driver.IsPaused && ReferenceEquals(driver.World, world),
                "Paused production World changed during the natural hit probe.");
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                "Dedicated worker failed during the natural hit probe.");
            if (expectedTick < 0)
            {
                if (stepped >= MaxTicks)
                {
                    report.status = "NATURAL_HIT_OR_VISIBLE_MARK_NOT_OBSERVED";
                    report.error = "Bounded 80-tick original Battle observation ended.";
                    CleanupAndFinish(request);
                    return;
                }
                expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true,
                    buildPresentation: true),
                    "Production Driver rejected a physical-input tick.");
                return;
            }
            if (driver.CurrentTickIndex < expectedTick ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            Require(driver.CurrentTickIndex == expectedTick,
                "Production Driver advanced more than one requested tick.");
            stepped++;
            report.completedTicks = stepped;
            expectedTick = -1;
            if (stepped == 2)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }

            FrameInputSet applied = driver.LastAppliedFrameInput;
            if (applied?.Players != null)
            {
                foreach (SimulationPlayerInput player in applied.Players)
                {
                    if (player.PlayerSlot == 0 &&
                        (player.Buttons & SimulationInputButtons.Jump) != 0)
                        report.appliedPhysicalAttackTicks++;
                }
            }
            report.authoredAttackSeen |= attacker.Frame.N == 60 ||
                attacker.Frame.N == 62 || attacker.Frame.N == 65 ||
                attacker.Frame.N == 513;
            report.finalHp = target.Health.HP;
            if (report.firstDamageTick < 0 && report.finalHp < InitialHp)
                report.firstDamageTick = driver.CurrentTickIndex;

            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            BattlePixelFramePlan plan = published?.TickIndex == driver.CurrentTickIndex
                ? BattleCentralRenderSystem.PrepareFrame(world)
                : default;
            BattlePresentationFrame commands = plan.IsValid && !plan.IsStale
                ? plan.CapturedFrame : null;
            int bodyIndex = commands != null && commands.CommandsMaterialized
                ? FindCommand(commands, report.targetSlot,
                    BattleRenderCommandType.Entity, out _) : -1;
            int markIndex = commands != null && commands.CommandsMaterialized
                ? FindCommand(commands, report.targetSlot,
                    BattleRenderCommandType.BleedMark, out BattleRenderCommand mark) : -1;
            int bpoints = target.Frame.D?.BloodPoints?.Count ?? 0;
            TickTrace.Append("tick=").Append(driver.CurrentTickIndex)
                .Append(" aFrame=").Append(attacker.Frame.N)
                .Append(" aX=").Append(attacker.Runtime.XInt)
                .Append(" tFrame=").Append(target.Frame.N)
                .Append(" tX=").Append(target.Runtime.XInt)
                .Append(" tHP=").Append(report.finalHp)
                .Append(" bpoints=").Append(bpoints)
                .Append(" published=").Append(published?.TickIndex ?? -1)
                .Append(" body=").Append(bodyIndex)
                .Append(" mark=").Append(markIndex).Append('\n');

            if (report.firstDamageTick < 0 || report.finalHp > BleedThreshold ||
                bpoints == 0 || bodyIndex < 0 || markIndex < 0)
                return;
            FindCommand(commands, report.targetSlot,
                BattleRenderCommandType.BleedMark, out BattleRenderCommand foundMark);
            report.markCount = CountCommands(commands, report.targetSlot,
                BattleRenderCommandType.BleedMark);
            report.bodyCommandIndex = bodyIndex;
            report.markCommandIndex = markIndex;
            report.markFrame = target.Frame.N;
            report.markTick = driver.CurrentTickIndex;
            report.markWidth = Mathf.RoundToInt(foundMark.Size.x);
            report.markHeight = Mathf.RoundToInt(foundMark.Size.y);
            Require(report.appliedPhysicalAttackTicks > 0 &&
                report.authoredAttackSeen && report.finalHp > 0 &&
                report.markCount == 1 && report.markCommandIndex > bodyIndex &&
                report.markWidth == 1 && report.markHeight == 3,
                "Physical attack, natural HP threshold, or body/mark order failed.");
            report.status = "PASS_NATURAL_HIT_CENTRAL_COMMAND";
            CleanupAndFinish(request);
        }

        private static int FindCommand(BattlePresentationFrame frame, int slot,
            BattleRenderCommandType type, out BattleRenderCommand matched)
        {
            matched = default;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.RuntimeSlot != slot || command.Type != type)
                    continue;
                matched = command;
                return index;
            }
            return -1;
        }

        private static int CountCommands(BattlePresentationFrame frame, int slot,
            BattleRenderCommandType type)
        {
            int count = 0;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.RuntimeSlot == slot && command.Type == type)
                    count++;
            }
            return count;
        }

        private static void CleanupAndFinish(Request request)
        {
            try
            {
                if (keyboard != null)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    InputSystem.Update();
                }
                if (target?.RegisteredWorldForSimulation == world)
                    world.Unregister(target);
                report.targetUnregistered = target == null ||
                    target.RegisteredWorldForSimulation == null;
                if (world != null && report.sceneHashBefore != null)
                {
                    report.objectsAfter = world.ObjectCount;
                    report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                    report.borrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
                    report.sceneHashAfter = HashFile(ProjectPath(ScenePath));
                    if (!report.targetUnregistered ||
                        report.objectsAfter != report.objectsBefore ||
                        report.slotsAfter != report.slotsBefore ||
                        report.borrowersAfter != report.borrowersBefore ||
                        report.sceneHashAfter != report.sceneHashBefore)
                    {
                        report.status = "FAIL";
                        report.error += " Fixture cleanup or Scene SHA mismatch.";
                    }
                }
                if (pauseCaptured && driver != null)
                    driver.SetPaused(savedPaused);
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error += " Cleanup failed: " + error;
            }
            finally
            {
                report.tickTrace = TickTrace.ToString();
                Finish(request, report);
            }
        }

        private static void Finish(Request request, Report result)
        {
            if (ValidRunId(request.runId))
            {
                string output = ResultPath(request.runId);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!File.Exists(output))
                    File.WriteAllText(output, JsonUtility.ToJson(result, true));
            }
            request.requested = false;
            request.running = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
            phase = 0;
            stableTick = -1;
            stableUpdates = 0;
            expectedTick = -1;
            stepped = 0;
            activeStartedUtcTicks = 0;
            pauseCaptured = false;
            driver = null;
            world = null;
            attacker = null;
            target = null;
            keyboard = null;
            report = null;
            TickTrace.Clear();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static bool ValidRunId(string runId)
        {
            if (string.IsNullOrEmpty(runId) || runId.Length > 80)
                return false;
            foreach (char value in runId)
                if (!char.IsLetterOrDigit(value) && value != '-' && value != '_')
                    return false;
            return true;
        }

        private static string ResultPath(string runId) =>
            ProjectPath(ResultRoot + "/" + runId + ".json");

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string HashFile(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
#endif
