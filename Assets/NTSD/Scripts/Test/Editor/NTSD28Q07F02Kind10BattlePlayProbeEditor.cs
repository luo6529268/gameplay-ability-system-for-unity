#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07F02Kind10BattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string RequestGlob = "NTSD28_Q07_F02Kind10Battle.request*.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.F02Kind10Battle";
        // FrameInputSet uses legacy physical-action names; its Jump bit reaches
        // the formal attack slot through the existing native input bridge.
        private const SimulationInputButtons FormalAttackButton =
            SimulationInputButtons.Jump;
        private static readonly string[] ProtectedPaths =
        {
            BattleScene,
            MenuScene,
            "Assets/NTSD/Config/GameConfig/GameConfig.asset",
            "Assets/NTSD/Resources/ProjectBattleModeConfig.asset"
        };

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static readonly LF2Entity[] actors = new LF2Entity[3];
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string mode;
            public string runId;
            public bool editorIdleConfirmed;
            public int sourceZ;
        }

        [Serializable]
        private sealed class FileHash
        {
            public string path;
            public string sha256;
        }

        [Serializable]
        private sealed class EntitySample
        {
            public int slot;
            public int oid;
            public int objectType;
            public int owner;
            public int interaction;
            public int parent;
            public int child;
            public double vx;
            public double vy;
            public double vz;
            public int action;
            public int state;
            public int counter;
            public int sourceX;
            public int sourceY;
            public int sourceZ;
            public int weaponFlightCounter;
            public double sourceRuleX;
            public double sourceRuleZ;
            public double viewX;
            public double viewY;
            public double viewZ;
            public int hp;
            public int hold;
            public int catchTarget;
            public int catchSource;
            public int team;
            public bool aiControlled;
            public byte legacyAttack;
            public byte legacyJump;
            public byte legacyDefend;
            public byte nativeAttack;
            public byte nativeJump;
            public byte nativeDefend;
        }

        [Serializable]
        private sealed class TickSample
        {
            public string inputReason;
            public int relativeTick;
            public int globalTick;
            public int inputPhase;
            public int slot0Buttons;
            public int slot1Buttons;
            public uint crtState;
            public ulong crtCalls;
            public int customCounter;
            public int customIndex;
            public ulong customCalls;
            public List<EntitySample> entities = new List<EntitySample>();
        }

        [Serializable]
        private sealed class Report
        {
            public int revision;
            public bool ownsPlay;
            public bool openedBattle;
            public bool noLiveDriverWorldAfterExit;
            public int groundTick = -1;
            public int pickupTick = -1;
            public int attackForPickup;
            public int neutralAfterPickup;
            public bool pickupAttempted;
            public bool throwAttempted;
            public string resetContract = "Initial actors HP/MP/PP, action, velocity, source/view position, owner/team/link, input history; Flow FrameToggle/InputPhase=0; NativeWorldClock.Reset; NativeRandom seed 0x28A55A5A; difficulty0. Global Driver tick is retained.";
            public string evidenceLimit = "Tick-tail capture only: kind10 attribution and intra-tick action40->41 remain unpaired until separately observed. View fields are simulation projection, not GPU pixels. Physical keyboard is not exercised.";
            public string runId;
            public string mode;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string initialScene;
            public string finalScene;
            public bool initialSceneDirty;
            public bool finalSceneDirty;
            public bool editorPlaying;
            public bool editorCompiling;
            public bool editorUpdating;
            public int sceneCount;
            public bool configuredBeforeStart;
            public bool enteredPlay;
            public bool exitedPlay;
            public bool protectedHashesStable;
            public string contentRoot;
            public int battleMode;
            public int difficulty;
            public int startTick;
            public int endTick;
            public int initialSourceZ;
            public double sourceToViewXOne;
            public double sourceToViewZOne;
            public List<FileHash> before = new List<FileHash>();
            public List<FileHash> after = new List<FileHash>();
            public List<TickSample> samples = new List<TickSample>();
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

        private static List<FileHash> HashProtectedFiles()
        {
            var result = new List<FileHash>(ProtectedPaths.Length);
            foreach (string relative in ProtectedPaths)
            {
                using (SHA256 hash = SHA256.Create())
                using (FileStream stream = File.OpenRead(PathInProject(relative)))
                {
                    result.Add(new FileHash
                    {
                        path = relative,
                        sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "")
                    });
                }
            }
            return result;
        }

        private static void Save()
        {
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
            string directory = PathInProject(ResultRoot + report.runId);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, (++report.revision).ToString("D5") + ".json");
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(report, true));
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
        }

        private static void Restore()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, "");
            if (!string.IsNullOrEmpty(saved)) report = JsonUtility.FromJson<Report>(saved);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null || report.mode != "run" || report.phase != "STARTUP" ||
                !EditorApplication.isPlaying || report.configuredBeforeStart ||
                scene.path != BattleScene) return;
            try
            {
                BattleTestBootstrap[] matches = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        value.gameObject.scene == scene && !EditorUtility.IsPersistent(value)).ToArray();
                Require(matches.Length == 1, "Expected one active BattleTestBootstrap in Play clone.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField("overrideCharacterIds",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap overrideCharacterIds is missing.");
                field.SetValue(matches[0], new[] { 2, 36 });
                report.configuredBeforeStart = true;
                Save();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null || report.mode != "run") return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                report.enteredPlay = true;
                Save();
            }
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
                if (report.phase == "OPENING") { EnterBattlePlay(); return; }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "F02 kind10 Scene probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void TryStart()
        {
            Request request = null;
            foreach (string path in Directory.GetFiles(PathInProject("Temp"), RequestGlob)
                .OrderByDescending(File.GetLastWriteTimeUtc))
            {
                Request candidate = JsonUtility.FromJson<Request>(File.ReadAllText(path));
                if (candidate == null || !candidate.requested ||
                    string.IsNullOrEmpty(candidate.runId) ||
                    Directory.Exists(PathInProject(ResultRoot + candidate.runId))) continue;
                request = candidate;
                break;
            }
            if (request == null) return;
            if (SessionState.GetString(SessionKey + ".ConsumedRun", "") == request.runId) return;
            SessionState.SetString(SessionKey + ".ConsumedRun", request.runId);
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 80 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require(request.mode == "preflight" || request.mode == "run", "Invalid request mode.");
            Require(request.sourceZ == 0 || request.sourceZ >= 180 && request.sourceZ <= 542,
                "Source Z is outside the declared diagnostic domain.");
            Require(!Directory.Exists(PathInProject(ResultRoot + request.runId)),
                "Refusing to overwrite existing result.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase), "Only original project Editor is allowed.");
            Scene scene = SceneManager.GetActiveScene();
            report = new Report
            {
                runId = request.runId,
                mode = request.mode,
                status = "RUNNING",
                phase = "PREFLIGHT",
                startedUtc = DateTime.UtcNow.ToString("O"),
                initialSourceZ = request.sourceZ == 0 ? 542 : request.sourceZ,
                initialScene = scene.path,
                initialSceneDirty = scene.isDirty,
                editorPlaying = EditorApplication.isPlayingOrWillChangePlaymode,
                editorCompiling = EditorApplication.isCompiling,
                editorUpdating = EditorApplication.isUpdating,
                sceneCount = SceneManager.sceneCount,
                before = HashProtectedFiles()
            };
            Save();
            if (request.mode == "preflight")
            {
                report.status = "CAPTURED";
                report.phase = "DONE";
                report.finalScene = scene.path;
                report.finalSceneDirty = scene.isDirty;
                report.after = HashProtectedFiles();
                report.protectedHashesStable = HashesMatch();
                Save();
                Clear();
                return;
            }
            Require(request.editorIdleConfirmed, "Run requires current explicit Editor idle confirmation.");
            Require(!report.editorPlaying && !scene.isDirty && SceneManager.sceneCount == 1 &&
                (scene.path == MenuScene || scene.path == BattleScene),
                "Run requires one clean saved Menu or Battle Scene in Edit Mode.");
            Require(report.before != null && report.before.Count == ProtectedPaths.Length,
                "Protected file baseline is incomplete.");
            report.phase = "OPENING";
            Save();
            if (scene.path == MenuScene)
            {
                EditorSceneManager.OpenScene(BattleScene, OpenSceneMode.Single);
                report.openedBattle = true;
                Save();
            }
        }

        private static void EnterBattlePlay()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Editor entered Play before Scene preflight completed.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Expected sole clean saved Battle Scene.");
            Require(HashesMatch(HashProtectedFiles(), report.before),
                "Protected disk file changed before Battle Play.");
            report.phase = "STARTUP";
            Save();
            report.ownsPlay = true;
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
            int[] ids = { 2, 36 };
            int[] sourceX = { 200, 530 };
            int[] teams = { 1, 2 };
            for (int slot = 0; slot < 2; slot++)
            {
                Require(world.TryResolveRosterInputEntity(slot, out LF2Entity entity) &&
                    entity is LF2Character, "Expected character roster slot " + slot);
                actors[slot] = (LF2Character)entity;
                Require(actors[slot].ObjectId == ids[slot],
                    "Incorrect formal character in slot " + slot);
            }
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            for (int slot = 0; slot < 2; slot++)
            {
                SetInitialActor((LF2Character)actors[slot], sourceX[slot]);
                actors[slot].ImmediateFrame(slot == 1 ? 243 : 0);
                SetInitialIdentity(actors[slot], slot, teams[slot]);
                actors[slot].RelationTeam = teams[slot];
                world.Runtime.Roster.Slots[slot].Team = teams[slot];
            }
            Require(world.FindEntityByRuntimeSlotForQuery(2) == null,
                "Formal weapon slot2 is already occupied; refusing to displace an entity.");
            var data = world.RuntimeCharacterConfigs.Resolve(600);
            Require(data?.characterData != null, "Formal OID600 runtime config is unavailable.");
            var weapon = new LF2Weapon { ObjectId = 600, Name = "Q07F02Kind10Weapon" };
            weapon.SetWeaponType(4);
            weapon.FrameCache.Load(data);
            weapon.SetRequiredRuntimeSlot(2);
            world.Register(weapon);
            actors[2] = weapon;
            weapon.ImmediateFrame(0);
            weapon.Health.HP = 250;
            weapon.Runtime.WeaponFlightCounter = data.characterData.weapon_hp;
            Require(weapon.Runtime.WeaponFlightCounter == 250,
                "Formal OID600 weapon_hp durability was not initialized.");
            weapon.Runtime.MP = 0;
            weapon.SwitchDir("right");
            weapon.Runtime.SetVelocity(0, 0, 0);
            weapon.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(190), -20,
                world.SpatialProjection.SourceToViewZ(report.initialSourceZ));
            weapon.Runtime.SetSourceRulePosition(190, report.initialSourceZ);
            weapon.Runtime.SyncIntegerPosition();
            weapon.Runtime.SyncSourceRuleIntegerPosition();
            SetInitialIdentity(weapon, 2, 1);
            Require(weapon.Runtime.SlotIndex == 2 && weapon.Frame.N == 0 &&
                weapon.GetCurrentDataObjectTypeForSimulation() == 4 &&
                weapon.Runtime.SourceRuleXInt == 190 && weapon.Runtime.YInt == -20 &&
                weapon.Runtime.SourceRuleZInt == report.initialSourceZ,
                "Weapon formal initial state mismatch.");
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null, "Native world clock unavailable.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            world.NativeRandom.ResetFromSeed(0x28A55A5Au);
            report.battleMode = world.BattleGameModeId;
            report.difficulty = world.Difficulty;
            Require(report.battleMode == 0 && report.difficulty == 0,
                "Unexpected battle mode or difficulty.");
            report.sourceToViewXOne = world.SpatialProjection.SourceDeltaToViewX(1);
            report.sourceToViewZOne = world.SpatialProjection.SourceDeltaToViewZ(1);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.samples.Add(CaptureTick(0, SimulationInputButtons.None, "initial"));
            report.phase = "MEASURING";
            Save();
        }

        private static void SetInitialActor(LF2Character actor, int sourceX)
        {
            actor.Initialize(500, 500);
            actor.ImmediateFrame(0);
            actor.Runtime.MP = 500;
            actor.Runtime.PP = 500;
            actor.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(actor.Runtime);
            actor.SwitchDir("right");
            actor.Runtime.Vx = actor.Runtime.Vy = actor.Runtime.Vz = 0;
            actor.Runtime.HP2Orig = 1;
            actor.Runtime.RespawnCount = 0;
            actor.HitStun = 0;
            actor.AttackExempt = 0;
            actor.ItrRest.Reset();
            actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(report.initialSourceZ));
            AppManager.SyncParticipantBirthPosition(actor, sourceX, report.initialSourceZ);
            Require(actor.Frame.N == 0 && actor.Runtime.SourceRuleXInt == sourceX &&
                actor.Runtime.YInt == 0 && actor.Runtime.SourceRuleZInt == report.initialSourceZ,
                "Initial action or source-rule position was not established.");
        }

        private static EntitySample Capture(LF2Entity entity, int slot)
        {
            return new EntitySample
            {
                slot = slot,
                oid = entity.ObjectId,
                objectType = entity.GetCurrentDataObjectTypeForSimulation(),
                owner = entity.Runtime.OwnerSlotIndex,
                interaction = entity.Runtime.LinkState,
                parent = entity.Runtime.HolderStableId,
                child = entity.Runtime.TargetSlotIndex,
                vx = entity.Runtime.Vx,
                vy = entity.Runtime.Vy,
                vz = entity.Runtime.Vz,
                action = entity.Frame.N,
                state = entity.Frame.D.state,
                counter = entity.AttackingCounter,
                sourceX = entity.Runtime.SourceRuleXInt,
                sourceY = entity.Runtime.YInt,
                sourceZ = entity.Runtime.SourceRuleZInt,
                weaponFlightCounter = entity.Runtime.WeaponFlightCounter,
                sourceRuleX = entity.Runtime.SourceRuleX,
                sourceRuleZ = entity.Runtime.SourceRuleZ,
                viewX = entity.Runtime.X,
                viewY = entity.Runtime.Y,
                viewZ = entity.Runtime.Z,
                hp = entity.Runtime.HP,
                hold = entity.Runtime.FrameDelay,
                catchTarget = entity.Runtime.CaughtSlotIndex,
                catchSource = entity.Runtime.CatchSourceSlot90,
                team = entity.RelationTeam,
                aiControlled = entity.AiControlled,
                legacyAttack = entity.Runtime.KeyAttack,
                legacyJump = entity.Runtime.KeyJump,
                legacyDefend = entity.Runtime.KeyDefend,
                nativeAttack = entity.Runtime.NativeInputProxy.Current[4],
                nativeJump = entity.Runtime.NativeInputProxy.Current[5],
                nativeDefend = entity.Runtime.NativeInputProxy.Current[6]
            };
        }

        private static void SetInitialIdentity(LF2Entity entity, int slot, int team)
        {
            entity.AiControlled = false;
            entity.Team = entity.RelationTeam = team;
            entity.Runtime.OwnerSlotIndex = slot;
            entity.Runtime.LinkState = 0;
            entity.Runtime.HolderStableId = 0;
            entity.Runtime.TargetSlotIndex = 0;
            Require(entity.Runtime.OwnerSlotIndex == slot && entity.RelationTeam == team &&
                entity.Runtime.LinkState == 0 && entity.Runtime.HolderStableId == 0 &&
                entity.Runtime.TargetSlotIndex == 0, "Formal initial identity mismatch.");
        }

        private static TickSample CaptureTick(int tick, SimulationInputButtons buttons, string reason)
        {
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var sample = new TickSample
            {
                relativeTick = tick,
                globalTick = driver.CurrentTickIndex,
                inputPhase = world.InputPhase,
                slot0Buttons = (int)buttons,
                slot1Buttons = 0,
                inputReason = reason,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls
            };
            for (int slot = 0; slot < 3; slot++)
            {
                LF2Entity occupant = world.FindEntityByRuntimeSlotForQuery(slot);
                Require(ReferenceEquals(occupant, actors[slot]),
                    "Entity replaced or retired at slot " + slot +
                    "; prior durability=" + actors[slot].Runtime.WeaponFlightCounter +
                    "; occupant=" + (occupant == null ? "none" : occupant.ObjectId.ToString()));
                sample.entities.Add(Capture(actors[slot], slot));
            }
            return sample;
        }

        private static void MeasureOneTick()
        {
            Require(driver != null && ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.samples.Count == 46) { CompleteMeasurement(); return; }
            int tick = report.samples.Count;
            LF2Entity actor = actors[0];
            LF2Entity weapon = actors[2];
            if (weapon.Frame.D.state == 1004 && report.groundTick < 0)
                report.groundTick = tick - 1;
            if (!report.pickupAttempted && report.groundTick >= 0 && actor.Frame.D.state == 0)
            {
                report.pickupAttempted = true;
                report.attackForPickup = 2;
            }
            SimulationInputButtons buttons = SimulationInputButtons.None;
            string reason = "none";
            if (report.attackForPickup > 0)
            {
                buttons = FormalAttackButton;
                --report.attackForPickup;
                reason = "pickup_attack";
            }
            else if (report.pickupTick >= 0 && !report.throwAttempted &&
                report.neutralAfterPickup >= 2 && actor.Runtime.LinkState == 4 &&
                (actor.Frame.D.state == 0 || actor.Frame.D.state == 1))
            {
                buttons = FormalAttackButton;
                report.throwAttempted = true;
                reason = "light_throw_attack";
            }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, buttons),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            Require(driver.CurrentTickIndex == next, "Driver did not advance exactly one tick.");
            if (report.pickupTick < 0 && actor.Runtime.LinkState == 4 &&
                actor.Runtime.TargetSlotIndex == 2) report.pickupTick = tick;
            if (report.pickupTick >= 0 && reason == "none") ++report.neutralAfterPickup;
            report.samples.Add(CaptureTick(tick, buttons, reason));
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            report.status = report.samples.Count == 46 &&
                report.samples.All(value => value.entities.Count == 3)
                ? "CAPTURED" : "INCOMPLETE";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static bool HashesMatch() => HashesMatch(report.after, report.before);

        private static bool HashesMatch(List<FileHash> current, List<FileHash> baseline)
        {
            return current != null && baseline != null &&
                current.Count == ProtectedPaths.Length &&
                baseline.Count == ProtectedPaths.Length &&
                current.Zip(baseline, (left, right) =>
                    left.path == right.path && left.sha256 == right.sha256).All(equal => equal);
        }

        private static void Fail(string message)
        {
            if (report == null) { Debug.LogError("[Q07 F02 kind10 Scene] " + message); return; }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (report.ownsPlay && EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = report.enteredPlay;
            report.noLiveDriverWorldAfterExit = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                .All(value => value == null || value.World == null);
            if (report.enteredPlay && !report.noLiveDriverWorldAfterExit)
            {
                report.status = "FAIL";
                report.error += " Live Driver World remains after exit.";
            }
            Scene scene = SceneManager.GetActiveScene();
            bool battleClean = !report.enteredPlay ||
                (scene.path == BattleScene && !scene.isDirty &&
                    SceneManager.sceneCount == 1);
            report.after = HashProtectedFiles();
            report.protectedHashesStable = HashesMatch();
            if (!battleClean || !report.protectedHashesStable)
            {
                report.status = "FAIL";
                report.error += " Battle Scene or protected disk file changed.";
            }
            if (report.openedBattle && !scene.isDirty && battleClean && report.initialScene == MenuScene)
                EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
            scene = SceneManager.GetActiveScene();
            report.finalScene = scene.path;
            report.finalSceneDirty = scene.isDirty;
            if (report.finalScene != report.initialScene ||
                (report.enteredPlay && scene.isDirty))
            {
                report.status = "FAIL";
                report.error += " Initial clean Editor Scene was not restored.";
            }
            report.after = HashProtectedFiles();
            report.protectedHashesStable = HashesMatch();
            if (!report.protectedHashesStable)
            {
                report.status = "FAIL";
                report.error += " Protected file changed during restoration.";
            }
            report.phase = "DONE";
            Save();
            Clear();
        }

        private static void Clear()
        {
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            for (int slot = 0; slot < actors.Length; slot++) actors[slot] = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
