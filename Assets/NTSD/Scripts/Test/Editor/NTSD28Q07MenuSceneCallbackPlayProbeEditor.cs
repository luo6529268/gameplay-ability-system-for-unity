#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.UI;
using NTSD.UI.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q07MenuSceneCallbackPlayProbeEditor
    {
        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool restorePending;
            public bool expectEmptyRootReject;
            public bool geometrySelfCheckAudit;
            public string runId;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string phase;
            public string message;
            public bool expectEmptyRootReject;
            public bool geometrySelfCheckAudit;
            public bool geometrySelfCheckPassed;
            public string sourceKey;
            public string shutdownStage;
            public int selectedCharacterId;
            public int worldObjects;
            public int finalPoolBorrowers;
            public int pendingLoadingTextsAfterPrewarm;
            public bool prewarmed;
            public bool modeSelected;
            public bool roleConfirmed;
            public bool battleSceneLoaded;
            public bool battleRunning;
            public bool samePublishedKeys;
            public bool stayedStopped;
            public bool returnedToMenu;
            public bool ordinaryResultAudit;
            public bool collisionResultAudit;
            public bool physicalAttackAudit;
            public bool resultStateInjected;
            public bool ordinarySelectionActive;
            public bool oldDriverDestroyed;
            public int collisionTick;
            public int fixtureSlot;
            public int victimHpAfterCollision;
            public int timerAfterCollision;
            public int timerFollowingTick;
            public int timerAt80;
            public int timerAt101;
            public int outputTimerAt350;
            public int transitionAt350;
            public ulong livingGroupsBeforeCollision;
            public int localGameModeId;
            public int battleGameModeId;
            public bool physicalInputSeen;
            public bool authoredPunchFrameSeen;
            public int physicalAttackTick;
            public int attackerFrameAtKo;
            public bool nativeKnockoutEventFound;
            public int nativeKnockoutSourceSlot;
            public int nativeKnockoutCreditSlot;
            public int nativeKnockoutFourOwnerSlot;
            public int nativeKnockoutSourceType;
        }

        private const string MenuScenePath = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string BattleScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string FormalFingerprint =
            "27CAE01489909C46A5145A5867988CCD8C10E20FEF165D847B7AC7A6DE2DE02D";
        private static bool running;
        private static string Root => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(Root, "Temp/NTSD28_Q07_MenuSceneCallback.request.json");
        private static string OrdinaryResultRequestPath => Path.Combine(Root,
            "Temp/NTSD28_Q08_OrdinaryRealMenuResult.request.json");
        private static string CollisionResultRequestPath => Path.Combine(Root,
            "Temp/NTSD28_Q08_CollisionRealMenuResult.request.json");
        private static string PhysicalAttackRequestPath => Path.Combine(Root,
            "Temp/NTSD28_Q08_PhysicalAttackRealMenuKo.request.json");
        private static string GeometryRequestPath => Path.Combine(Root,
            "Temp/NTSD28_Q07_FormalGeometryPlay.request.json");
        private static string ResultRoot => Path.Combine(Root,
            "artifacts/diagnostics/NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001");
        private static string OrdinaryResultRoot => Path.Combine(Root,
            "artifacts/diagnostics/NTSD28-Q08-ORDINARY-REAL-MENU-RESULT-WITNESS-001");
        private static string CollisionResultRoot => Path.Combine(Root,
            "artifacts/diagnostics/NTSD28-Q08-COLLISION-TO-ORDINARY-REAL-MENU-WITNESS-001");
        private static string PhysicalAttackResultRoot => Path.Combine(Root,
            "artifacts/diagnostics/NTSD28-Q08-PHYSICAL-ATTACK-REAL-MENU-KO-001");
        private static string GeometryResultRoot => Path.Combine(Root,
            "artifacts/diagnostics/NTSD28-Q07-SELFCHECK-FORMAL-GEOMETRY-001");

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            string requestPath = RequestPath;
            if (File.Exists(OrdinaryResultRequestPath))
            {
                try
                {
                    Request ordinaryRequest = JsonUtility.FromJson<Request>(
                        File.ReadAllText(OrdinaryResultRequestPath));
                    if (ordinaryRequest != null &&
                        (ordinaryRequest.requested || ordinaryRequest.restorePending))
                        requestPath = OrdinaryResultRequestPath;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (File.Exists(CollisionResultRequestPath))
            {
                try
                {
                    Request collisionRequest = JsonUtility.FromJson<Request>(
                        File.ReadAllText(CollisionResultRequestPath));
                    if (collisionRequest != null &&
                        (collisionRequest.requested || collisionRequest.restorePending))
                        requestPath = CollisionResultRequestPath;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (File.Exists(PhysicalAttackRequestPath))
            {
                try
                {
                    Request physicalRequest = JsonUtility.FromJson<Request>(
                        File.ReadAllText(PhysicalAttackRequestPath));
                    if (physicalRequest != null &&
                        (physicalRequest.requested || physicalRequest.restorePending))
                        requestPath = PhysicalAttackRequestPath;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (File.Exists(GeometryRequestPath))
            {
                try
                {
                    Request geometryRequest = JsonUtility.FromJson<Request>(
                        File.ReadAllText(GeometryRequestPath));
                    if (geometryRequest != null &&
                        (geometryRequest.requested || geometryRequest.restorePending))
                        requestPath = GeometryRequestPath;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (!File.Exists(requestPath))
                return;
            bool ordinaryResultAudit = requestPath == OrdinaryResultRequestPath;
            bool collisionResultAudit = requestPath == CollisionResultRequestPath;
            bool physicalAttackAudit = requestPath == PhysicalAttackRequestPath;
            bool geometryRequestAudit = requestPath == GeometryRequestPath;
            string resultRoot = geometryRequestAudit ? GeometryResultRoot :
                physicalAttackAudit ? PhysicalAttackResultRoot :
                collisionResultAudit ? CollisionResultRoot :
                ordinaryResultAudit ? OrdinaryResultRoot : ResultRoot;
            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(requestPath));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null) return;
            if (!EditorApplication.isPlaying && request.restorePending &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (SceneManager.GetActiveScene().path == MenuScenePath &&
                    !SceneManager.GetActiveScene().isDirty)
                {
                    EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
                    request.restorePending = false;
                    File.WriteAllText(requestPath, JsonUtility.ToJson(request));
                }
                return;
            }
            if (!request.requested || EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isPlaying) return;
            string output = Path.Combine(
                resultRoot,
                request.runId + ".json");
            if (!EditorApplication.isPlaying)
            {
                try
                {
                    Require(ValidRunId(request.runId), "Invalid run ID.");
                    Require(!File.Exists(output), "Result already exists: " + output);
                    Require(SceneManager.GetActiveScene().path == BattleScenePath &&
                        !SceneManager.GetActiveScene().isDirty, "Expected a saved Battle Scene before Menu Play.");
                    Require(EditorBuildSettings.scenes.Any(scene =>
                        scene.enabled && scene.path == BattleScenePath),
                        "Additive Battle Scene is absent from enabled Editor Build Settings scenes.");
                    EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
                    request.restorePending = true;
                    File.WriteAllText(requestPath, JsonUtility.ToJson(request));
                    EditorApplication.EnterPlaymode();
                }
                catch (Exception error)
                {
                    request.requested = false;
                    File.WriteAllText(requestPath, JsonUtility.ToJson(request));
                    if (ValidRunId(request.runId) && !File.Exists(output))
                    {
                        Directory.CreateDirectory(
                            resultRoot);
                        File.WriteAllText(output, JsonUtility.ToJson(new Report
                        {
                            runId = request.runId,
                            status = "FAIL",
                            phase = "edit-preflight",
                            message = error.ToString()
                        }, true));
                    }
                }
                return;
            }
            request.requested = false;
            File.WriteAllText(requestPath, JsonUtility.ToJson(request));
            running = true;
            Run(request.runId, output, request.expectEmptyRootReject,
                ordinaryResultAudit, collisionResultAudit, physicalAttackAudit,
                geometryRequestAudit && request.geometrySelfCheckAudit).Forget();
        }

        private static async UniTask Run(string runId, string output,
            bool expectEmptyRootReject, bool ordinaryResultAudit,
            bool collisionResultAudit, bool physicalAttackAudit,
            bool geometrySelfCheckAudit)
        {
            var report = new Report
            {
                runId = runId,
                status = "RUNNING",
                phase = "menu",
                expectEmptyRootReject = expectEmptyRootReject,
                geometrySelfCheckAudit = geometrySelfCheckAudit,
                ordinaryResultAudit = ordinaryResultAudit,
                collisionResultAudit = collisionResultAudit,
                physicalAttackAudit = physicalAttackAudit
            };
            GameConfig selectedConfig = null;
            string previousRoot = null;
            try
            {
                Require(SceneManager.GetActiveScene().path == MenuScenePath,
                    "Play did not start from the real Menu Scene.");
                await UniTask.NextFrame();
                var main = UnityEngine.Object.FindObjectOfType<MainMenuController>(true);
                var menu = UnityEngine.Object.FindObjectOfType<MenuUIController>(true);
                var loading = UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true);
                Require(main != null && menu != null && loading != null &&
                    main._GameStartButton != null && EventSystem.current != null,
                    "Menu Scene callback components are missing.");
                selectedConfig = GameConfig.Instance;
                Require(selectedConfig != null && selectedConfig.BattleContentRuntimeRoot ==
                    "Assets/NTSD/Content/LoganRuntime", "Menu did not select the serialized formal root.");
                if (expectEmptyRootReject)
                {
                    previousRoot = selectedConfig.BattleContentRuntimeRoot;
                    selectedConfig.BattleContentRuntimeRoot = string.Empty;
                }

                report.phase = "game-start";
                main._GameStartButton.OnPointerDown(new PointerEventData(EventSystem.current));
                await WaitUntil(() => loading.IsPrewarmed, 240f, "Menu loading prewarm timed out.");
                report.prewarmed = true;
                var manager = CharacterAnimtorManager.TryGetInstance();
                if (expectEmptyRootReject)
                    Require(manager != null && manager.PublishedLoganContentIdentity == null &&
                        CharacterAnimtorManager.ConfiguredContentRoot.Length == 0,
                        "Empty-root Menu prewarm did not retain its legacy content branch.");
                else
                    Require(manager?.PublishedLoganContentIdentity?.SemanticFingerprint == FormalFingerprint,
                        "Menu loading did not publish the current project-mode formal content identity.");
                var pendingTexts = (Queue<string>)typeof(LoadingPrewarmController)
                    .GetField("pendingTexts", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(loading);
                report.pendingLoadingTextsAfterPrewarm = pendingTexts.Count;

                report.phase = "mode";
                await WaitUntil(() => UnityEngine.Object.FindObjectOfType<SelectGameModeController>(true)
                    ?.gameObject.activeInHierarchy == true,
                    Mathf.Clamp(pendingTexts.Count * 0.2f + 15f, 30f, 600f),
                    "VS mode panel did not appear after loading progress drained.");
                var mode = UnityEngine.Object.FindObjectOfType<SelectGameModeController>(true);
                var modeOptions = mode.GetComponentInChildren<MenuOptionList>(true);
                Require(modeOptions != null && modeOptions.CurrentIndex == 0, "VS mode option is unavailable.");
                modeOptions.OnConfirm();
                report.modeSelected = true;

                report.phase = "role";
                await WaitUntil(() => UnityEngine.Object.FindObjectOfType<CharacterSelectionController>(true)
                    ?.gameObject.activeInHierarchy == true, 10f, "Character panel did not appear.");
                var selection = UnityEngine.Object.FindObjectOfType<CharacterSelectionController>(true);
                Require(selection.PlayerSlots.Count > 0 && selection.PlayerSlots[0] != null,
                    "The first role slot is unavailable.");
                SelectRoleItem slot = selection.PlayerSlots[0];
                slot.RefreshAvailableCharacters();
                slot.OnJoin();
                for (int i = 0; i < 400 && slot.SelectedCharacterId != 2; i++)
                    slot.OnNavigateCharacter(1);
                Require(slot.SelectedCharacterId == 2, "Formal Naruto OID2 is not selectable in the Menu.");
                slot.OnConfirmCharacter();
                slot.OnConfirmTeam();
                Require(slot.State == SelectRoleState.Confirmed, "Naruto role confirmation failed.");
                report.selectedCharacterId = slot.SelectedCharacterId;
                report.roleConfirmed = true;

                if (collisionResultAudit || physicalAttackAudit)
                {
                    Require(selection.PlayerSlots.Count > 1 &&
                        selection.PlayerSlots[1] != null,
                        "The real Menu has no second participant slot.");
                    SelectRoleItem opponentSlot = selection.PlayerSlots[1];
                    opponentSlot.RefreshAvailableCharacters();
                    opponentSlot.OnJoin();
                    for (int i = 0; i < 400 && opponentSlot.SelectedCharacterId != 2; i++)
                        opponentSlot.OnNavigateCharacter(1);
                    Require(opponentSlot.SelectedCharacterId == 2,
                        "The second Menu slot cannot select formal Naruto OID2.");
                    opponentSlot.OnConfirmCharacter();
                    opponentSlot.OnNavigateTeam(1);
                    opponentSlot.OnConfirmTeam();
                    Require(opponentSlot.State == SelectRoleState.Confirmed &&
                        opponentSlot.GetFinalTeam() == 2,
                        "The second Menu slot did not confirm opposing team 2.");
                }

                report.phase = "cmc";
                await WaitUntil(() => selection.Step == CharacterSelectionStep.ComputerCount,
                    15f, "Character countdown did not reach CMC.");
                var cmc = UnityEngine.Object.FindObjectOfType<CMCRootController>(true);
                var cmcOptions = cmc?.GetComponentInChildren<MenuOptionList>(true);
                Require(cmcOptions != null && cmcOptions.CurrentIndex == 0,
                    "Zero-computer CMC confirmation is unavailable.");
                cmcOptions.OnConfirm();

                report.phase = "settings";
                await WaitUntil(() => selection.Step == CharacterSelectionStep.SettingBattleBg,
                    10f, "Battle settings did not appear.");
                var settings = UnityEngine.Object.FindObjectOfType<SettingBattleBgController>(true);
                var settingsOptions = settings?.GetComponentInChildren<MenuOptionList>(true);
                Require(settingsOptions != null && settingsOptions.CurrentIndex == 0,
                    "Fight confirmation is unavailable.");
                settingsOptions.OnConfirm();

                report.phase = "battle";
                if (expectEmptyRootReject)
                {
                    await WaitUntil(() =>
                    {
                        Scene scene = SceneManager.GetSceneByName("NTSD_Battle");
                        return scene.IsValid() && scene.isLoaded;
                    }, 120f, "Empty-root Menu did not request the additive Battle Scene.");
                    report.battleSceneLoaded = true;
                    await WaitUntil(() =>
                        SimulationTickDriver.Instance?.LifecycleState == BattleRuntimeLifecycleState.Stopped ||
                        AppManager.Instance?.State == AppFlowState.BattleRunning,
                        30f, "Empty-root Battle entry reached neither rejection nor BattleRunning.");
                    SimulationTickDriver rejectedDriver = SimulationTickDriver.Instance;
                    report.battleRunning = AppManager.Instance.State == AppFlowState.BattleRunning;
                    report.stayedStopped = rejectedDriver != null &&
                        rejectedDriver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                        rejectedDriver.World == null;
                    Require(!report.battleRunning && report.stayedStopped,
                        "Empty-root legacy content entered Battle or left a live World.");
                    report.phase = "unload";
                    AsyncOperation rejectedUnload = AppManager.Instance.UnloadBattle();
                    Require(rejectedUnload != null, "Rejected Battle Scene could not be unloaded.");
                    await rejectedUnload.ToUniTask();
                    await UniTask.NextFrame();
                    report.returnedToMenu = !SceneManager.GetSceneByName("NTSD_Battle").isLoaded &&
                        AppManager.Instance.State == AppFlowState.MenuMain;
                    var rejectedPool = LF2ObjectPool.TryGetInstance();
                    report.finalPoolBorrowers = rejectedPool == null ? -1 :
                        rejectedPool.ActiveObjectCountForAcceptance +
                        rejectedPool.ActiveSpriteCountForAcceptance;
                    Require(report.returnedToMenu && report.finalPoolBorrowers == 0,
                        "Rejected Battle entry did not return to Menu with zero pool borrowers.");
                    report.shutdownStage = "Stopped";
                    report.status = "PASS";
                    report.message = "Legacy Menu prewarm completed, but empty-root Battle entry was rejected before BattleRunning.";
                    return;
                }
                await WaitUntil(() => AppManager.Instance != null &&
                    AppManager.Instance.State == AppFlowState.BattleRunning, 120f,
                    "Menu callback chain did not reach BattleRunning.");
                report.battleRunning = true;
                Scene battle = SceneManager.GetSceneByName("NTSD_Battle");
                report.battleSceneLoaded = battle.IsValid() && battle.isLoaded;
                Require(report.battleSceneLoaded, "Battle Scene was not additively loaded.");
                Require(AppManager.Instance.CurrentMatchConfig?.players?.Any(p =>
                    p.use && p.characterId == 2) == true, "The selected Naruto did not enter MatchConfig.");
                SimulationTickDriver driver = SimulationTickDriver.Instance;
                Require(driver?.World != null && driver.World.ObjectCount > 0,
                    "Menu-started battle has no live World actors.");
                report.worldObjects = driver.World.ObjectCount;
                manager = CharacterAnimtorManager.TryGetInstance();
                report.sourceKey = await manager.ValidateConfiguredContentForBattleAsync();
                report.samePublishedKeys = !string.IsNullOrEmpty(report.sourceKey) &&
                    GameDataManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey &&
                    CharacterUIResourceManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey;
                Require(report.samePublishedKeys &&
                    manager.PublishedLoganContentIdentity?.SemanticFingerprint == FormalFingerprint,
                    "Menu-started battle does not have three matching formal publication owners.");

                if (geometrySelfCheckAudit)
                {
                    report.phase = "formal-geometry-selfcheck";
                    Require(!expectEmptyRootReject && GameConfig.Instance != null &&
                        CharacterAnimtorManager.ConfiguredContentRoot.Length != 0,
                        "Formal geometry Play check requires a configured battle content root.");
                    MethodInfo geometryCheck = typeof(BattleRuntimeSelfCheck).GetMethod(
                        "CheckDeployableResolvedGeometryRisks",
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null, Type.EmptyTypes, null);
                    Require(geometryCheck != null,
                        "No-argument formal geometry SelfCheck entry is unavailable.");
                    geometryCheck.Invoke(null, null);
                    report.geometrySelfCheckPassed = true;
                }

                if (collisionResultAudit)
                {
                    report.phase = "collision-result";
                    await RunCollisionResultAudit(driver, report);
                    return;
                }
                if (physicalAttackAudit)
                {
                    report.phase = "physical-attack-result";
                    await RunPhysicalAttackResultAudit(driver, report);
                    return;
                }

                if (ordinaryResultAudit)
                {
                    report.phase = "ordinary-result";
                    Require(driver.LifecycleState == BattleRuntimeLifecycleState.Running,
                        "The real Battle driver is not running before result transition.");
                    BattleResultsRuntimeState results = driver.World.Runtime.Results;
                    results.NativeResultPhase = 3;
                    results.NativeResultOutputTimer = 350;
                    results.NativeTransitionState = 2;
                    report.resultStateInjected = true;
                    await WaitUntil(() => AppManager.Instance != null &&
                        AppManager.Instance.State == AppFlowState.MenuSelectCharacter,
                        30f, "Ordinary result did not return the real Menu to character selection.");
                    Scene oldBattle = SceneManager.GetSceneByName("NTSD_Battle");
                    report.battleSceneLoaded = oldBattle.IsValid() && oldBattle.isLoaded;
                    var activeSelection = UnityEngine.Object.FindObjectOfType<
                        CharacterSelectionController>(true);
                    report.ordinarySelectionActive = activeSelection != null &&
                        activeSelection.gameObject.activeInHierarchy;
                    report.oldDriverDestroyed = driver == null &&
                        SimulationTickDriver.Instance == null;
                    var resultPool = LF2ObjectPool.TryGetInstance();
                    report.finalPoolBorrowers = resultPool == null ? -1 :
                        resultPool.ActiveObjectCountForAcceptance +
                        resultPool.ActiveSpriteCountForAcceptance;
                    report.returnedToMenu = !report.battleSceneLoaded &&
                        AppManager.Instance.State == AppFlowState.MenuSelectCharacter;
                    Require(report.returnedToMenu && report.ordinarySelectionActive &&
                        report.oldDriverDestroyed && report.finalPoolBorrowers == 0,
                        "Ordinary result left Battle, selection or pool ownership incomplete.");
                    report.shutdownStage = "DriverDestroyedAfterOrderedUnload";
                    report.status = "PASS";
                    report.message = "Real Menu callback battle returned through the production ordinary result host to character selection.";
                    return;
                }

                report.phase = "unload";
                AsyncOperation unload = AppManager.Instance.UnloadBattle();
                Require(unload != null, "Ordered battle unload refused to start.");
                await unload.ToUniTask();
                await UniTask.NextFrame();
                await UniTask.NextFrame();
                report.stayedStopped = driver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                    driver.World == null;
                var pool = LF2ObjectPool.TryGetInstance();
                report.finalPoolBorrowers = pool == null ? -1 :
                    pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
                report.returnedToMenu = !SceneManager.GetSceneByName("NTSD_Battle").isLoaded &&
                    AppManager.Instance.State == AppFlowState.MenuMain;
                Require(report.stayedStopped && report.finalPoolBorrowers == 0 &&
                    report.returnedToMenu, "Battle unload left runtime or Menu state behind.");
                report.shutdownStage = "Stopped";
                report.status = "PASS";
                report.message = "Actual Menu Scene component callbacks reached formal-content additive battle and ordered Menu return.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.message = error.ToString();
            }
            finally
            {
                if (selectedConfig != null && previousRoot != null)
                    selectedConfig.BattleContentRuntimeRoot = previousRoot;
                Directory.CreateDirectory(
                    geometrySelfCheckAudit ? GeometryResultRoot :
                    physicalAttackAudit ? PhysicalAttackResultRoot :
                    collisionResultAudit ? CollisionResultRoot :
                    ordinaryResultAudit ? OrdinaryResultRoot : ResultRoot);
                File.WriteAllText(output, JsonUtility.ToJson(report, true));
                running = false;
                EditorApplication.delayCall += () =>
                {
                    if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
                };
            }
        }

        private static async UniTask RunPhysicalAttackResultAudit(
            SimulationTickDriver driver, Report report)
        {
            Require(driver.LifecycleState == BattleRuntimeLifecycleState.Running,
                "Physical attack audit entered without a running Battle driver.");
            driver.SetPaused(true);
            SimulationWorld world = driver.World;
            Require(world != null && world.Runtime.Match.LocalGameModeId == 0 &&
                world.BattleGameModeId == 1,
                "Physical attack audit requires the project's live VS World.");
            LF2Character attacker = world.FindEntityByRuntimeSlotForQuery(0) as LF2Character;
            LF2Character victim = world.FindEntityByRuntimeSlotForQuery(1) as LF2Character;
            CharacterInputModule input = attacker?.Controller as CharacterInputModule;
            Keyboard keyboard = Keyboard.current;
            Require(attacker?.ObjectId == 2 && victim?.ObjectId == 2 &&
                attacker.RelationTeam == 1 && victim.RelationTeam == 2 &&
                attacker.Health.HP > 0 && victim.Health.HP > 0 &&
                input?.AttackAction?.enabled == true && keyboard != null &&
                world.Runtime.Results.NativeResultTimer == 0,
                "Real opposing Naruto participants or the P1 physical attack action are unavailable.");
            report.livingGroupsBeforeCollision =
                (1UL << attacker.RelationTeam) | (1UL << victim.RelationTeam);
            report.localGameModeId = world.Runtime.Match.LocalGameModeId;
            report.battleGameModeId = world.BattleGameModeId;
            int attackerStableId = attacker.Runtime.StableId;
            int victimStableId = victim.Runtime.StableId;
            int direction = attacker.Runtime.IsFacingLeft ? -1 : 1;
            victim.Runtime.SetPosition(attacker.Runtime.XInt + direction * 40,
                attacker.Runtime.YInt, attacker.Runtime.ZInt);
            victim.Runtime.SyncIntegerPosition();
            victim.Health.HP = 10;
            victim.Health.HPBound = 10;
            victim.Health.HP3 = 10;
            victim.HP2Orig = 1;
            victim.HitStun = 0;
            victim.AttackExempt = 0;
            victim.Runtime.LinkState = 0;
            victim.ItrRest.Reset();
            victim.RefreshRuntimeSnapshot();

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
                InputSystem.Update();
                for (int count = 0; count < 40; count++)
                {
                    Require(ReferenceEquals(world, driver.World) &&
                        attacker.Runtime.StableId == attackerStableId &&
                        victim.Runtime.StableId == victimStableId,
                        "World or actual Menu participant identity changed during the attack.");
                    Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                        "Live Driver rejected a physical-attack tick.");
                    int tick = driver.CurrentTickIndex;
                    if (count == 1)
                    {
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                        InputSystem.Update();
                    }
                    FrameInputSet applied = driver.LastAppliedFrameInput;
                    if (applied?.Players != null)
                    {
                        for (int index = 0; index < applied.Players.Count; index++)
                        {
                            SimulationPlayerInput player = applied.Players[index];
                            if (player.PlayerSlot == 0 &&
                                (player.Buttons & SimulationInputButtons.Jump) != 0)
                            {
                                report.physicalInputSeen = true;
                                if (report.physicalAttackTick == 0)
                                    report.physicalAttackTick = tick;
                            }
                        }
                    }
                    report.authoredPunchFrameSeen |= attacker.Frame.N == 60 ||
                        attacker.Frame.N == 62;
                    if (victim.Health.HP > 0)
                        continue;
                    report.collisionTick = tick;
                    report.victimHpAfterCollision = victim.Health.HP;
                    report.attackerFrameAtKo = attacker.Frame.N;
                    report.timerAfterCollision = world.Runtime.Results.NativeResultTimer;
                    NativeKnockoutEvent knockout = world.NativeKnockoutEvents.FirstOrDefault(hit =>
                        hit.VictimSlot == victim.Runtime.SlotIndex &&
                        hit.BattleTimeTick == tick);
                    report.nativeKnockoutEventFound =
                        world.NativeKnockoutEvents.Any(hit =>
                            hit.VictimSlot == victim.Runtime.SlotIndex &&
                            hit.BattleTimeTick == tick);
                    if (report.nativeKnockoutEventFound)
                    {
                        report.nativeKnockoutSourceSlot = knockout.SourceSlot;
                        report.nativeKnockoutCreditSlot = knockout.CreditSlot;
                        report.nativeKnockoutFourOwnerSlot = knockout.FourOwnerSlot;
                        report.nativeKnockoutSourceType = knockout.SourceObjectType;
                    }
                    break;
                }
            }
            finally
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }

            Require(report.physicalInputSeen && report.authoredPunchFrameSeen &&
                report.collisionTick > 0 && report.victimHpAfterCollision <= 0 &&
                report.timerAfterCollision == 0 &&
                (report.attackerFrameAtKo == 62 || report.attackerFrameAtKo == 513) &&
                report.nativeKnockoutEventFound &&
                (report.nativeKnockoutCreditSlot == attacker.Runtime.SlotIndex ||
                 report.nativeKnockoutFourOwnerSlot == attacker.Runtime.SlotIndex),
                "Physical J did not produce an authored Naruto punch KO and native event.");
            Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                "Live Driver rejected the first physical-attack result tick.");
            report.timerFollowingTick = world.Runtime.Results.NativeResultTimer;
            Require(report.timerFollowingTick == 1,
                "Physical-attack result timer did not begin on the following tick.");
            for (int timer = 2; timer <= 350; timer++)
            {
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Live Driver rejected physical-attack result timer " + timer + ".");
                BattleResultsRuntimeState results = world.Runtime.Results;
                if (timer == 80) report.timerAt80 = results.NativeResultOutputTimer;
                if (timer == 101) report.timerAt101 = results.NativeResultOutputTimer;
            }
            report.outputTimerAt350 = world.Runtime.Results.NativeResultOutputTimer;
            report.transitionAt350 = world.Runtime.Results.NativeTransitionState;
            Require(report.timerAt80 == 80 && report.timerAt101 == 101 &&
                report.outputTimerAt350 == 350 && report.transitionAt350 == 2,
                "Physical-attack result milestones or ordinary transition differ.");

            await WaitUntil(() => AppManager.Instance != null &&
                AppManager.Instance.State == AppFlowState.MenuSelectCharacter,
                30f, "Physical-attack ordinary result did not return to selection.");
            Scene battle = SceneManager.GetSceneByName("NTSD_Battle");
            var selection = UnityEngine.Object.FindObjectOfType<
                CharacterSelectionController>(true);
            report.ordinarySelectionActive = selection != null &&
                selection.gameObject.activeInHierarchy;
            report.oldDriverDestroyed = driver == null &&
                SimulationTickDriver.Instance == null;
            var pool = LF2ObjectPool.TryGetInstance();
            report.finalPoolBorrowers = pool == null ? -1 :
                pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
            report.battleSceneLoaded = battle.IsValid() && battle.isLoaded;
            report.returnedToMenu = !report.battleSceneLoaded &&
                AppManager.Instance.State == AppFlowState.MenuSelectCharacter;
            Require(report.returnedToMenu && report.ordinarySelectionActive &&
                report.oldDriverDestroyed && report.finalPoolBorrowers == 0,
                "Physical-attack result left Battle or pool ownership incomplete.");
            report.shutdownStage = "DriverDestroyedAfterOrderedUnload";
            report.status = "PASS";
            report.message = "Real P1 physical J reached an authored Naruto punch, KO and production selection return.";
        }

        private static async UniTask RunCollisionResultAudit(
            SimulationTickDriver driver, Report report)
        {
            Require(driver.LifecycleState == BattleRuntimeLifecycleState.Running,
                "Collision audit entered without a running Battle driver.");
            driver.SetPaused(true);
            SimulationWorld world = driver.World;
            Require(world != null, "Collision audit has no live World.");
            report.localGameModeId = world.Runtime.Match.LocalGameModeId;
            report.battleGameModeId = world.BattleGameModeId;
            Require(report.localGameModeId == 0 && report.battleGameModeId == 1,
                "Collision audit requires the project's VS local-0/battle-1 mode pair.");
            LF2Character first = world.FindEntityByRuntimeSlotForQuery(0) as LF2Character;
            LF2Character victim = world.FindEntityByRuntimeSlotForQuery(1) as LF2Character;
            Require(first != null && victim != null &&
                first.RelationTeam == 1 && victim.RelationTeam == 2 &&
                first.Health.HP > 0 && victim.Health.HP > 0,
                "The two real Menu participants are not living opposing groups.");
            Require(world.Runtime.Results.NativeResultTimer == 0 &&
                world.Runtime.Results.NativeTransitionState == 0,
                "Result flow advanced before the controlled collision.");
            report.livingGroupsBeforeCollision =
                (1UL << first.RelationTeam) | (1UL << victim.RelationTeam);

            int fixtureSlot = world.RuntimeSlotCapacityForDiagnostics - 1;
            while (fixtureSlot > 50 &&
                world.FindEntityByRuntimeSlotForQuery(fixtureSlot) != null)
                fixtureSlot--;
            Require(fixtureSlot > 50, "No free runtime slot for the lethal fixture.");
            MethodInfo create =
                typeof(NTSD28Q08CombatLethalPrecombatTimingEditorTests).GetMethod(
                    "CreateCombatant", BindingFlags.Static | BindingFlags.NonPublic);
            Require(create != null, "The existing full-tick lethal fixture is missing.");
            LF2Character attacker = create.Invoke(null, new object[]
            {
                world, fixtureSlot, 1, 7100, victim.Runtime.XInt, true, 500
            }) as LF2Character;
            Require(attacker != null && attacker.Frame.D.itrs.Count == 1 &&
                victim.Frame.D.bodies.Count > 0,
                "Controlled attacker or formal victim collision geometry is unavailable.");
            report.fixtureSlot = fixtureSlot;
            InteractionArea itr = attacker.Frame.D.itrs[0];
            itr.x = -500;
            itr.y = -500;
            itr.w = 1000;
            itr.h = 1000;
            itr.zwidth = 1000;
            itr.hasGeometry = true;
            attacker.Runtime.SetPosition(victim.Runtime.XInt, 0, victim.Runtime.ZInt);
            attacker.Runtime.SyncIntegerPosition();
            attacker.RefreshRuntimeSnapshot();
            victim.Health.HP = 20;
            victim.Health.HPBound = 20;
            victim.Health.HP3 = 20;
            victim.HP2Orig = 1;
            victim.HitStun = 0;
            victim.AttackExempt = 0;
            victim.Runtime.LinkState = 0;
            victim.ItrRest.Reset();
            victim.RefreshRuntimeSnapshot();

            report.collisionTick = driver.CurrentTickIndex + 1;
            Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                "The live Driver rejected the full collision tick.");
            report.victimHpAfterCollision = victim.Health.HP;
            report.timerAfterCollision = world.Runtime.Results.NativeResultTimer;
            Require(report.victimHpAfterCollision <= 0 &&
                report.timerAfterCollision == 0 &&
                world.NativeKnockoutEvents.Any(hit =>
                    hit.VictimSlot == victim.Runtime.SlotIndex &&
                    hit.BattleTimeTick == report.collisionTick),
                "The collision tick did not KO the real second participant before result timing.");

            Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                "The live Driver rejected the first result tick.");
            report.timerFollowingTick = world.Runtime.Results.NativeResultTimer;
            Require(report.timerFollowingTick == 1,
                "Result timer did not begin on the tick after collision-caused KO.");
            for (int timer = 2; timer <= 350; timer++)
            {
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "The live Driver rejected result timer " + timer + ".");
                BattleResultsRuntimeState results = world.Runtime.Results;
                if (timer == 80) report.timerAt80 = results.NativeResultOutputTimer;
                if (timer == 101) report.timerAt101 = results.NativeResultOutputTimer;
            }
            report.outputTimerAt350 = world.Runtime.Results.NativeResultOutputTimer;
            report.transitionAt350 = world.Runtime.Results.NativeTransitionState;
            Require(report.timerAt80 == 80 && report.timerAt101 == 101 &&
                report.outputTimerAt350 == 350 && report.transitionAt350 == 2,
                "The native result milestones or ordinary transition differ after KO.");

            await WaitUntil(() => AppManager.Instance != null &&
                AppManager.Instance.State == AppFlowState.MenuSelectCharacter,
                30f, "Collision-driven ordinary result did not return to selection.");
            Scene battle = SceneManager.GetSceneByName("NTSD_Battle");
            var selection = UnityEngine.Object.FindObjectOfType<
                CharacterSelectionController>(true);
            report.ordinarySelectionActive = selection != null &&
                selection.gameObject.activeInHierarchy;
            report.oldDriverDestroyed = driver == null &&
                SimulationTickDriver.Instance == null;
            var pool = LF2ObjectPool.TryGetInstance();
            report.finalPoolBorrowers = pool == null ? -1 :
                pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
            report.battleSceneLoaded = battle.IsValid() && battle.isLoaded;
            report.returnedToMenu = !report.battleSceneLoaded &&
                AppManager.Instance.State == AppFlowState.MenuSelectCharacter;
            Require(report.returnedToMenu && report.ordinarySelectionActive &&
                report.oldDriverDestroyed && report.finalPoolBorrowers == 0,
                "Collision-driven result left Battle or pool ownership incomplete.");
            report.shutdownStage = "DriverDestroyedAfterOrderedUnload";
            report.status = "PASS";
            report.message = "Two real Menu teams, controlled collision KO, native result timer and production selection return passed.";
        }

        private static async UniTask WaitUntil(Func<bool> predicate, float seconds, string failure)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
                await UniTask.NextFrame();
            Require(predicate(), failure);
        }

        private static bool ValidRunId(string id)
        {
            return !string.IsNullOrEmpty(id) && id.All(c => char.IsLetterOrDigit(c) || c == '-');
        }

        private static void Require(bool condition, string failure)
        {
            if (!condition) throw new InvalidOperationException(failure);
        }
    }
}
#endif
