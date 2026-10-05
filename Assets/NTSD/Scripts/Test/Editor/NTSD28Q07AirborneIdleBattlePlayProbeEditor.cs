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
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07AirborneIdleBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_AirborneIdleBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/";
        private const string SessionKey = "NTSD.Q07.AirborneIdleBattlePlay";
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character target;
        private static LF2Weapon heldWeapon;
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
        }

        [Serializable]
        private sealed class VisualSample
        {
            public int relativeTick;
            public int globalTick;
            public int publishedTick;
            public int planTick;
            public int slot;
            public int action;
            public int sourceY;
            public int sourceZ;
            public int bodyCommands;
            public int shadowCommands;
            public int renderFps;
            public double displayAlpha;
            public float viewportHeightPixels;
            public float bodyWorldY;
            public float shadowWorldY;
            public float bodyMinusShadowPixels;
        }

        [Serializable]
        private sealed class InterpolationSample
        {
            public int previousMotionTick;
            public double previousPreciseY;
            public double currentPreciseY;
            public double previousPreciseZ;
            public double currentPreciseZ;
            public double sampledRoundedY;
            public double sourceDeltaY;
            public double expectedBodyDeltaPixels;
            public double observedBodyDeltaPixels;
            public double observedShadowDeltaPixels;
            public VisualSample partial;
            public VisualSample complete;
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
            public int difficultyBefore;
            public int difficultyEffective;
            public int battleMode;
            public int localMode;
            public int aiPhaseGate;
            public int primarySlot = -1;
            public int firstBirthRelativeTick = -1;
            public bool configuredBeforeStart;
            public bool sceneCleanAfter;
            public bool exitedPlay;
            public List<Sample> samples = new List<Sample>();
            public List<VisualSample> visualSamples = new List<VisualSample>();
            public List<InterpolationSample> interpolationSamples = new List<InterpolationSample>();
            public HeldAnchorSample heldAnchor;
        }

        [Serializable]
        private sealed class HeldAnchorSample
        {
            public int globalTick, publishedTick, planTick, holderCommands, weaponCommands;
            public int holderAction, weaponAction, holderSourceX, weaponSourceX;
            public int holderSourceZ, weaponSourceZ, holderY, weaponY, weaponSlot;
            public int holderLink, targetSlot, weaponLink, holderSlot;
            public double horizontalScale, verticalScale, displayAlpha;
            public Vector3 holderCommandPosition, weaponCommandPosition;
            public Vector2 holderPointWorld, weaponPointWorld, differencePixels;
        }

        private static bool IsHeldAnchorProbe =>
            report != null && report.runId.StartsWith("d024-held-anchor-", StringComparison.Ordinal);

        private static bool IsVerticalProbe =>
            report != null && report.runId.StartsWith("d024-vertical-", StringComparison.Ordinal);

        private static bool IsInterpolationProbe =>
            report != null && report.runId.StartsWith("d024-vertical-r120-", StringComparison.Ordinal);

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
            string path = PathInProject(ResultRoot + report.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

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
                field.SetValue(matches[0], IsHeldAnchorProbe ? new[] { 2, 7 } : new[] { 84, 2 });
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
            Require(!File.Exists(PathInProject(ResultRoot + request.runId + ".json")),
                "Refusing to overwrite an existing result.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Editor must be in Edit Mode.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase), "Only the original project Editor is allowed.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires sole clean saved NTSD_Battle Scene.");
            report = new Report { runId = request.runId, phase = "STARTUP", status = "RUNNING",
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
                first is LF2Character, "First Guren roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "Second Naruto roster entity is missing.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            Require(IsHeldAnchorProbe ? actor.ObjectId == 2 && target.ObjectId == 7 :
                actor.ObjectId == 84 && target.ObjectId == 2,
                "Play clone roster is not formal OID84/2 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            if (IsHeldAnchorProbe)
            {
                SetHeldAnchorInitialActor(actor, 200);
                SetHeldAnchorInitialActor(target, 1200);
                var data = world.RuntimeCharacterConfigs.Resolve(120);
                Require(data?.characterData != null, "Formal OID120 data is unavailable.");
                heldWeapon = new LF2Weapon();
                heldWeapon.ObjectId = 120;
                heldWeapon.SetWeaponType(1);
                heldWeapon.FrameCache.Load(data);
                heldWeapon.SetRequiredRuntimeSlot(50);
                world.Register(heldWeapon);
                heldWeapon.ImmediateFrame(64);
                heldWeapon.Health.HP = 100;
                heldWeapon.Runtime.SetPosition(190, 0, 542);
                heldWeapon.Runtime.SyncIntegerPosition();
                heldWeapon.Runtime.SetSourceRulePosition(190, 542);
                heldWeapon.Runtime.SyncSourceRuleIntegerPosition();
            }
            else
            {
                SetInitialActor(actor, 388, 500, 500);
                SetInitialActor(target, 0, 1100, 500);
            }
            actor.RelationTeam = 1;
            target.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
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

        private static void SetInitialActor(LF2Character actor, int action, int sourceX, int hp)
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
            actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(actor, sourceX, 400);
            Require(actor.Frame.N == action && actor.Runtime.SourceRuleXInt == sourceX,
                "Initial action or source-rule position was not established.");
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
                team = entity.RelationTeam
            };
        }

        private static void SetHeldAnchorInitialActor(LF2Character character, int x)
        {
            SetInitialActor(character, 0, x, 500);
            character.AiControlled = false;
            character.Runtime.SetPosition(x, 0, 542);
            character.Runtime.SyncIntegerPosition();
            AppManager.SyncParticipantBirthPosition(character, x, 542);
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.samples.Count == (IsHeldAnchorProbe ? 2 : 32)) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, IsHeldAnchorProbe ?
                    SimulationInputButtons.Jump : SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
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
                if (entity == null || (slot > 1 && entity.ObjectId != 619 && entity.ObjectId != 85 &&
                    !(IsHeldAnchorProbe && ReferenceEquals(entity, heldWeapon)))) continue;
                EntitySample captured = Capture(entity, slot);
                sample.entities.Add(captured);
                if (report.primarySlot < 0 && captured.oid == 85)
                {
                    report.primarySlot = slot;
                    report.firstBirthRelativeTick = sample.relativeTick;
                }
            }
            report.samples.Add(sample);
            report.endTick = driver.CurrentTickIndex;
            if (IsHeldAnchorProbe && sample.relativeTick == 2)
                CaptureHeldAnchorPresentation();
            if (IsInterpolationProbe && sample.relativeTick == 23)
                CaptureInterpolationPresentation(sample);
            if (IsVerticalProbe && (sample.relativeTick == 21 || sample.relativeTick == 23))
                CaptureVerticalPresentation(sample);
            Save();
        }

        private static void CaptureVerticalPresentation(Sample sample)
        {
            report.visualSamples.Add(CapturePresentation(sample, 30));
        }

        private static void CaptureHeldAnchorPresentation()
        {
            Require(actor.Frame.N == 115 && heldWeapon.Frame.N == 24 &&
                actor.GetHeldWeapon() == heldWeapon && actor.Runtime.TargetSlotIndex == 50 &&
                heldWeapon.Runtime.LinkState == -1 && heldWeapon.Runtime.HolderStableId == actor.Runtime.SlotIndex,
                "Two-tick natural pickup did not reach the formal reciprocal 115/24 relation.");
            BattlePixelFramePlan plan;
            double alpha;
            int renderFps = world.BattlePresentationRenderFps;
            float interval = world.BattlePresentationLogicIntervalSeconds;
            try
            {
                world.ConfigureBattlePresentationDisplayPolicy(30, interval);
                plan = BattleCentralRenderSystem.PrepareFrame(world);
                alpha = BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world);
            }
            finally
            {
                world.ConfigureBattlePresentationDisplayPolicy(renderFps, interval);
            }
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            Require(published != null && published.TickIndex == report.endTick &&
                plan.IsValid && !plan.IsStale && plan.SimulationTick == report.endTick &&
                plan.CapturedFrame != null && plan.CapturedFrame.CommandsMaterialized &&
                plan.CapturedFrame.TickIndex == report.endTick && Math.Abs(alpha - 1.0) < 1e-6,
                "Current complete-tick central commands are unavailable.");
            var evidence = new HeldAnchorSample
            {
                globalTick = report.endTick, publishedTick = published.TickIndex,
                planTick = plan.SimulationTick, displayAlpha = alpha,
                horizontalScale = world.SpatialProjection.HorizontalScale,
                verticalScale = world.SpatialProjection.VerticalScale,
                holderAction = actor.Frame.N, weaponAction = heldWeapon.Frame.N,
                holderSourceX = actor.Runtime.SourceRuleXInt, weaponSourceX = heldWeapon.Runtime.SourceRuleXInt,
                holderSourceZ = actor.Runtime.SourceRuleZInt, weaponSourceZ = heldWeapon.Runtime.SourceRuleZInt,
                holderY = actor.Runtime.YInt, weaponY = heldWeapon.Runtime.YInt,
                holderLink = actor.Runtime.LinkState, targetSlot = actor.Runtime.TargetSlotIndex,
                weaponLink = heldWeapon.Runtime.LinkState, holderSlot = heldWeapon.Runtime.HolderStableId,
                weaponSlot = heldWeapon.Runtime.SlotIndex
            };
            report.heldAnchor = evidence;
            for (int index = 0; index < plan.CapturedFrame.CommandCount; index++)
            {
                BattleRenderCommand command = plan.CapturedFrame.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Entity) continue;
                if (command.RuntimeSlot == actor.Runtime.SlotIndex)
                {
                    evidence.holderCommands++;
                    evidence.holderCommandPosition = command.Position;
                    evidence.holderPointWorld = CommandWeaponPoint(command, actor.Frame.D);
                }
                else if (command.RuntimeSlot == heldWeapon.Runtime.SlotIndex)
                {
                    evidence.weaponCommands++;
                    evidence.weaponCommandPosition = command.Position;
                    evidence.weaponPointWorld = CommandWeaponPoint(command, heldWeapon.Frame.D);
                }
            }
            evidence.differencePixels = new Vector2(
                (evidence.weaponPointWorld.x - evidence.holderPointWorld.x) / NTSDRenderSpace.UnitsPerPixelX,
                -(evidence.weaponPointWorld.y - evidence.holderPointWorld.y) / NTSDRenderSpace.UnitsPerPixelY);
            Require(evidence.horizontalScale > 1.0 && evidence.verticalScale > 1.0 &&
                evidence.holderCommands == 1 && evidence.weaponCommands == 1 &&
                evidence.holderSourceX == 201 && evidence.weaponSourceX == 214 &&
                evidence.weaponSourceZ - evidence.holderSourceZ == 1 &&
                evidence.holderY == 0 && evidence.weaponY == 7 &&
                Math.Abs(evidence.differencePixels.x) < 0.0003 &&
                Math.Abs(evidence.differencePixels.y) < 0.0003,
                "Natural source-rule alignment or actual central WPoint contact differs.");
        }

        private static Vector2 CommandWeaponPoint(BattleRenderCommand command, LF2FrameData frame)
        {
            float x = command.FlipX ? command.Size.x - frame.PrimaryWeaponPoint.X :
                frame.PrimaryWeaponPoint.X;
            return new Vector2(
                command.Position.x + (x - command.Pivot.x * command.Size.x) *
                    NTSDRenderSpace.BattleVisualScale * NTSDRenderSpace.UnitsPerPixelX,
                command.Position.y + (command.Size.y - frame.PrimaryWeaponPoint.Y -
                    command.Pivot.y * command.Size.y) *
                    NTSDRenderSpace.BattleVisualScale * NTSDRenderSpace.UnitsPerPixelY);
        }

        private static VisualSample CapturePresentation(Sample sample, int requestedRenderFps)
        {
            EntitySample entity = sample.entities.FirstOrDefault(value => value.oid == 85 &&
                value.slot == report.primarySlot);
            Require(entity != null, "Expected natural OID85 at vertical capture tick.");
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.orthographic, "World camera is unavailable.");
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            int renderFps = world.BattlePresentationRenderFps;
            float logicInterval = world.BattlePresentationLogicIntervalSeconds;
            BattlePixelFramePlan plan;
            double alpha;
            try
            {
                world.ConfigureBattlePresentationDisplayPolicy(requestedRenderFps, logicInterval);
                plan = BattleCentralRenderSystem.PrepareFrame(world);
                alpha = BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world);
            }
            finally
            {
                world.ConfigureBattlePresentationDisplayPolicy(renderFps, logicInterval);
            }
            Require(published != null && published.TickIndex == sample.globalTick &&
                plan.IsValid && !plan.IsStale && plan.SimulationTick == sample.globalTick &&
                plan.CapturedFrame != null && plan.CapturedFrame.CommandsMaterialized &&
                plan.CapturedFrame.TickIndex == sample.globalTick,
                "Current tick central presentation commands are unavailable.");
            var visual = new VisualSample
            {
                relativeTick = sample.relativeTick,
                globalTick = sample.globalTick,
                publishedTick = published.TickIndex,
                planTick = plan.SimulationTick,
                slot = entity.slot,
                action = entity.action,
                sourceY = entity.y,
                sourceZ = entity.z,
                renderFps = requestedRenderFps,
                displayAlpha = alpha,
                viewportHeightPixels = 2f * camera.orthographicSize /
                    NTSDRenderSpace.UnitsPerPixelY
            };
            BattlePresentationFrame commands = plan.CapturedFrame;
            for (int index = 0; index < commands.CommandCount; index++)
            {
                BattleRenderCommand command = commands.GetCommand(index);
                if (command.RuntimeSlot != entity.slot) continue;
                if (command.Type == BattleRenderCommandType.Entity)
                {
                    visual.bodyCommands++;
                    visual.bodyWorldY = command.Position.y;
                }
                else if (command.Type == BattleRenderCommandType.Shadow)
                {
                    visual.shadowCommands++;
                    visual.shadowWorldY = command.Position.y;
                }
            }
            Require(visual.bodyCommands == 1 && visual.shadowCommands == 1 &&
                (requestedRenderFps > 30 || Math.Abs(alpha - 1.0) < 1e-6),
                "Expected one body and shadow command at the requested display policy.");
            visual.bodyMinusShadowPixels =
                (visual.bodyWorldY - visual.shadowWorldY) / NTSDRenderSpace.UnitsPerPixelY;
            return visual;
        }

        private static void CaptureInterpolationPresentation(Sample sample)
        {
            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            Require(frame != null && frame.PreviousMotionTickIndex + 1 == sample.globalTick,
                "Expected adjacent published motion ticks.");
            BattlePresentationMotionState previous = default;
            BattlePresentationMotionState current = default;
            bool foundPrevious = false;
            bool foundCurrent = false;
            for (int index = 0; index < frame.PreviousMotionStateCount; index++)
            {
                BattlePresentationMotionState value = frame.GetPreviousMotionState(index);
                if (value.Handle.Slot != report.primarySlot) continue;
                previous = value;
                foundPrevious = true;
            }
            for (int index = 0; index < frame.MotionStateCount; index++)
            {
                BattlePresentationMotionState value = frame.GetMotionState(index);
                if (value.Handle.Slot != report.primarySlot) continue;
                current = value;
                foundCurrent = true;
            }
            Require(foundPrevious && foundCurrent && previous.Handle.Equals(current.Handle) &&
                previous.ObjectId == 85 && current.ObjectId == 85 &&
                previous.HasSourceRulePosition && current.HasSourceRulePosition,
                "OID85 adjacent motion identity is unavailable.");

            VisualSample partial = CapturePresentation(sample, 120);
            VisualSample complete = CapturePresentation(sample, 30);
            double roundedY = Math.Round(previous.PreciseY +
                (current.PreciseY - previous.PreciseY) * partial.displayAlpha,
                MidpointRounding.AwayFromZero);
            double deltaY = roundedY - Math.Round(current.PreciseY,
                MidpointRounding.AwayFromZero);
            var interpolation = new InterpolationSample
            {
                previousMotionTick = frame.PreviousMotionTickIndex,
                previousPreciseY = previous.PreciseY,
                currentPreciseY = current.PreciseY,
                previousPreciseZ = previous.PreciseZ,
                currentPreciseZ = current.PreciseZ,
                sampledRoundedY = roundedY,
                sourceDeltaY = deltaY,
                expectedBodyDeltaPixels = -deltaY * world.SpatialProjection.VerticalScale,
                observedBodyDeltaPixels =
                    (partial.bodyWorldY - complete.bodyWorldY) / NTSDRenderSpace.UnitsPerPixelY,
                observedShadowDeltaPixels =
                    (partial.shadowWorldY - complete.shadowWorldY) / NTSDRenderSpace.UnitsPerPixelY,
                partial = partial,
                complete = complete,
            };
            report.interpolationSamples.Add(interpolation);
            Require(partial.displayAlpha > 0.0 && partial.displayAlpha < 1.0 && deltaY != 0.0,
                "Natural display clock did not produce a discriminating intermediate alpha.");
            Require(Math.Abs(previous.PreciseZ - current.PreciseZ) < 1e-10 &&
                Math.Abs(interpolation.observedShadowDeltaPixels) < 0.0003 &&
                Math.Abs(interpolation.observedBodyDeltaPixels -
                    interpolation.expectedBodyDeltaPixels) < 0.0003,
                "Intermediate body/ground commands differ from native rounding and shared Y projection.");
        }

        private static void CompleteMeasurement()
        {
            if (IsHeldAnchorProbe)
            {
                Require(report.samples.Count == 2 && report.heldAnchor != null,
                    "Held-anchor evidence is incomplete.");
                report.status = "PASS";
                report.phase = "EXITING";
                Save();
                EditorApplication.ExitPlaymode();
                return;
            }
            EntitySample born = report.samples[20].entities
                .FirstOrDefault(value => value.slot == report.primarySlot);
            EntitySample next = report.samples[21].entities
                .FirstOrDefault(value => value.slot == report.primarySlot);
            EntitySample producer = report.samples[20].entities
                .FirstOrDefault(value => value.oid == 619 && value.action == 268);
            report.status = report.firstBirthRelativeTick == 21 && report.primarySlot >= 50 &&
                born != null && next != null && producer != null && producer.y == -40 &&
                born.oid == 85 && born.type == 0 && born.action == 212 && born.state == 4 &&
                born.counter == 1 && born.y == -22 && Math.Abs(born.vy) < 1e-8 &&
                next.action == 212 && next.counter == 0 && next.y == -22 &&
                Math.Abs(next.vy - 1.7) < 1e-8 ? "PASS" : "DIFFERENCE";
            if (IsVerticalProbe && (report.visualSamples.Count != 2 ||
                report.visualSamples[0].relativeTick != 21 ||
                report.visualSamples[1].relativeTick != 23 ||
                report.visualSamples[0].action != 212 ||
                report.visualSamples[1].action != 212 ||
                report.visualSamples[0].sourceZ != report.visualSamples[1].sourceZ))
                report.status = "DIFFERENCE";
            if (IsInterpolationProbe && report.interpolationSamples.Count != 1)
                report.status = "DIFFERENCE";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null) { Debug.LogError("[Q07 Airborne idle Play] " + message); return; }
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
            heldWeapon = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
