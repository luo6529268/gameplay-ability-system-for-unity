#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NTSD.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q09NameplateNaturalPlayProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/Q09/Run Natural Nameplate Viewport Probe";
        private const string GameViewMenuPath =
            "NTSD/Battle Diagnostics/Q09/Run Natural Nameplate Game View Capture";
        private const string Words5GameViewMenuPath =
            "NTSD/Battle Diagnostics/Q09/Run WORDS5 Nameplate Game View Capture";
        private const string OneTickInputMenuPath =
            "NTSD/Battle Diagnostics/Q07/Run Menu D Dynamic One Tick Trace";
        private const string PlayerLoopOneTickMenuPath =
            "NTSD/Battle Diagnostics/Q07/Run Menu D PlayerLoop One Tick Trace";
        private static readonly string ResultPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-Q09-NAMEPLATE-VIEWPORT-CLAMP-AUDIT-20260927",
            "original-battle-natural-nameplate-play.json"));
        private static readonly string GameViewOutputFolder = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-336B44-Q09-NATURAL-GAMEVIEW-TICK-001"));
        private static readonly string WordsGameViewOutputFolder = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-336B44-Q01-WORDS-GAMEVIEW-001"));
        private static readonly string Words5GameViewOutputFolder = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-336B44-Q09-WORDS5-GAMEVIEW-001"));

        private static readonly List<LF2Entity> Entities = new List<LF2Entity>(16);
        private static readonly List<InputTraceRow> InputTrace = new List<InputTraceRow>(12);
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static Keyboard keyboard;
        private static Key moveKey;
        private static bool moveRight;
        private static bool pausedByProbe;
        private static bool ownsMenuBattle;
        private static int phase;
        private static int startTick;
        private static int firstX;
        private static int directionTicks;
        private static int lastObservedTick;
        private static double deadline;
        private static Report report;
        private static string activeResultPath;
        private static string screenCapturePath;
        private static bool captureGameView;
        private static bool captureWordsOnly;
        private static int requestedBattleGroup;
        private static bool singleTickInputTrace;
        private static bool playerLoopOneTickTrace;
        private static int dynamicUpdates;
        private static int queuedAtDynamicUpdate;
        private static readonly MethodInfo DynamicInputUpdate = typeof(InputSystem).GetMethod(
            "Update", BindingFlags.Static | BindingFlags.NonPublic, null,
            new[] { typeof(InputUpdateType) }, null);

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            Start(false, false);
        }

        [MenuItem(GameViewMenuPath)]
        public static void RunGameViewCapture()
        {
            Start(true, false);
        }

        public static void RunWordsGameViewCapture()
        {
            Start(true, true);
        }

        [MenuItem(Words5GameViewMenuPath)]
        public static void RunWords5GameViewCapture()
        {
            Start(true, true, false, false, 5);
        }

        [MenuItem(OneTickInputMenuPath)]
        public static void RunMenuDDynamicOneTickTrace()
        {
            Start(false, false, true);
        }

        [MenuItem(PlayerLoopOneTickMenuPath)]
        public static void RunMenuDPlayerLoopOneTickTrace()
        {
            Start(false, false, true, true);
        }

        private static void Start(bool captureScreen, bool wordsOnly,
            bool oneTickInput = false, bool playerLoopInput = false,
            int playerBattleGroup = 1)
        {
            EditorApplication.update -= Observe;
            InputSystem.onAfterUpdate -= OnInputUpdate;
            ReleaseKey();
            captureGameView = captureScreen;
            captureWordsOnly = wordsOnly;
            requestedBattleGroup = playerBattleGroup;
            singleTickInputTrace = oneTickInput;
            playerLoopOneTickTrace = playerLoopInput;
            dynamicUpdates = 0;
            queuedAtDynamicUpdate = 0;
            if (playerLoopInput)
                InputSystem.onAfterUpdate += OnInputUpdate;
            string runId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" +
                Guid.NewGuid().ToString("N");
            activeResultPath = playerLoopInput
                ? Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath,
                    "..", "artifacts", "diagnostics",
                    "NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001")),
                    "playerloop-one-tick-" + runId + ".json")
                : oneTickInput
                ? Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath,
                    "..", "artifacts", "diagnostics",
                    "NTSD28-336B44-Q07-MENU-D-DYNAMIC-ONE-TICK-001")),
                    "one-tick-" + runId + ".json")
                : captureScreen
                ? Path.Combine(playerBattleGroup == 5
                        ? Words5GameViewOutputFolder
                        : wordsOnly ? WordsGameViewOutputFolder : GameViewOutputFolder,
                    "natural-nameplate-" + runId + ".json")
                : ResultPath;
            if ((captureScreen || oneTickInput) && (File.Exists(activeResultPath) ||
                                  File.Exists(Path.ChangeExtension(activeResultPath, ".png"))))
                throw new IOException("The natural Game View output path already exists.");
            screenCapturePath = null;
            report = new Report
            {
                status = "RUNNING",
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
                playerLoopOneTick = playerLoopInput,
                requestedBattleGroup = playerBattleGroup,
            };
            Save();
            if (!EditorApplication.isPlaying)
            {
                Finish("FAIL", "Play Mode is not active.");
                return;
            }

            driver = null;
            world = null;
            actor = null;
            keyboard = null;
            phase = 0;
            startTick = -1;
            lastObservedTick = -1;
            directionTicks = 0;
            InputTrace.Clear();
            pausedByProbe = false;
            ownsMenuBattle = SceneManager.GetActiveScene().name == "NTSD_Menu";
            deadline = EditorApplication.timeSinceStartup + 180.0;
            if (ownsMenuBattle)
                PrepareMenuBattle().Forget();
            else
                EditorApplication.update += Observe;
        }

        private static void OnInputUpdate()
        {
            if (InputState.currentUpdateType == InputUpdateType.Dynamic)
                dynamicUpdates++;
        }

        private static async UniTask PrepareMenuBattle()
        {
            try
            {
                Check(!SceneManager.GetActiveScene().isDirty,
                    "The saved Menu Scene must be clean before the probe.");
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
                await UniTask.NextFrame();
                LoadingPrewarmController loading =
                    UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true);
                Check(loading != null, "The Menu prewarm controller is missing.");
                await loading.PrewarmOnceAsync();
                CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                Check(manager?.GetCharacterConfig(2) != null &&
                      manager.GetCharacterConfig(7) != null,
                    "The selected formal character definitions were not prewarmed.");
                Check(!string.IsNullOrEmpty(
                        await manager.ValidateConfiguredContentForBattleAsync()),
                    "The formal battle content identity is unavailable.");

                var match = new MatchConfig
                {
                    seed = 2833,
                    gameMode = new GameModeConfig { battleGameModeId = 0 },
                };
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = true, characterId = 2,
                    team = requestedBattleGroup, inputId = 1,
                });
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = true, characterId = 7,
                    team = 2, inputId = 2,
                });
                AppManager app = AppManager.Instance;
                Check(app != null, "The Menu AppManager is unavailable.");
                app.SetMatchConfig(match);
                AsyncOperation load = app.LoadBattleAdditive();
                Check(load != null, "The additive Battle load was refused.");
                await load.ToUniTask();
                deadline = EditorApplication.timeSinceStartup + 120.0;
                EditorApplication.update += Observe;
            }
            catch (Exception exception)
            {
                Finish("FAIL", "Menu-to-Battle setup: " + exception);
            }
        }

        private static void Observe()
        {
            try
            {
                if (!EditorApplication.isPlaying)
                    throw new InvalidOperationException("Play Mode ended during the probe.");
                if (EditorApplication.timeSinceStartup > deadline)
                    throw new TimeoutException("Battle or natural movement did not reach the viewport gate.");

                if (phase == 0)
                {
                    driver = SimulationTickDriver.Instance;
                    if (driver == null ||
                        driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                        driver.CurrentTickIndex < 2 ||
                        driver.World?.BattlePresentation.PublishedFrame == null)
                        return;

                    world = driver.World;
                    Check(world.BattlePresentation.Mode ==
                        BattlePresentationBackendMode.CentralOnly,
                        "The live Battle Scene is not CentralOnly.");
                    Check(!string.IsNullOrEmpty(report.contentRoot) &&
                        report.contentRoot.Replace('\\', '/').EndsWith(
                            "Assets/NTSD/Content/LoganRuntime", StringComparison.Ordinal),
                        "The selected LoganRuntime content root is absent.");
                    Entities.Clear();
                    world.GetAllEntities(Entities);
                    foreach (LF2Entity candidate in Entities)
                    {
                        if (candidate.Runtime.SlotIndex == 0)
                        {
                            actor = candidate as LF2Character;
                            break;
                        }
                    }

                    Check(actor != null && actor.Runtime.SourceRulePositionInitialized,
                        "The original P1 character/source position is unavailable.");
                    CharacterInputModule input = actor.Controller as CharacterInputModule;
                    Check(input?.MoveAction?.enabled == true,
                        "The production P1 movement action is disabled.");
                    keyboard = Keyboard.current;
                    Check(keyboard != null, "No Input System keyboard is available.");
                    if (!captureGameView)
                        Check(world.Runtime.SlotLabels.BattleSlotLabels[0, 0] != '\0',
                            "The selected battle slot has no nameplate label.");

                    firstX = actor.Runtime.XInt;
                    moveRight = firstX <= 1050;
                    moveKey = playerLoopOneTickTrace ? Key.D :
                        moveRight ? Key.D : Key.A;
                    startTick = driver.CurrentTickIndex;
                    lastObservedTick = startTick;
                    report.startTick = startTick;
                    report.initialX = firstX;
                    report.moveKey = moveKey.ToString();
                    if (singleTickInputTrace)
                    {
                        if (playerLoopOneTickTrace)
                            QueuePlayerLoopOneTickInput(input);
                        else
                            RunOneTickInputTrace(input);
                        return;
                    }
                    QueueKey(moveKey);
                    phase = 1;
                    deadline = EditorApplication.timeSinceStartup + 35.0;
                    return;
                }

                if (playerLoopOneTickTrace && phase == 4)
                {
                    CompletePlayerLoopOneTickInput();
                    return;
                }

                if (phase == 1)
                {
                    int tick = driver.CurrentTickIndex;
                    if (tick == lastObservedTick)
                        return;
                    lastObservedTick = tick;
                    directionTicks++;
                    report.directionTicks = directionTicks;
                    report.lastX = actor.Runtime.XInt;
                    if (InputTrace.Count < 12)
                        CaptureInputTrace(tick);
                    Check(tick - startTick <= 180,
                        "P1 did not reach the nameplate viewport gate in 180 logic ticks.");
                    bool bodyVisible = LF2ObjectRenderer.ShouldDrawEntityForHitStop(
                        actor.Runtime.HitStop);
                    bool waitForWords = captureWordsOnly &&
                        (directionTicks < 2 ||
                         world.Runtime.SlotLabels.BattleSlotLabels[0, 0] == '\0' ||
                         !bodyVisible);
                    bool waitForViewport = !captureWordsOnly &&
                        (directionTicks < 2 ||
                         Math.Abs(actor.Runtime.XInt - firstX) < 8 ||
                         actor.Runtime.XInt <= 794 ||
                         (moveRight && actor.Runtime.XInt < 850) ||
                         (!moveRight && actor.Runtime.XInt > 1100) ||
                         !bodyVisible);
                    if (waitForWords || waitForViewport)
                    {
                        QueueKey(moveKey);
                        return;
                    }

                    driver.SetPaused(true);
                    pausedByProbe = true;
                    phase = 2;
                    return;
                }

                if (phase == 3)
                {
                    if (!File.Exists(screenCapturePath) ||
                        new FileInfo(screenCapturePath).Length < 24)
                        return;
                    Texture2D image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(screenCapturePath);
                        if (!ImageConversion.LoadImage(image, bytes, false))
                            return;
                        report.screenCaptureWidth = image.width;
                        report.screenCaptureHeight = image.height;
                    }
                    catch (IOException)
                    {
                        return;
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(image);
                    }
                    Check(report.screenCaptureWidth > 0 &&
                          report.screenCaptureHeight > 0,
                        "The actual Game View screenshot is empty.");
                    report.tickAfterScreenCapture = driver.CurrentTickIndex;
                    Check(driver.IsPaused &&
                          report.tickAfterScreenCapture == report.captureTick,
                        "The logic tick changed while the Game View screenshot was captured.");
                    Finish("PASS", "The same-tick command and Unity Game View screen capture were saved.");
                    return;
                }

                if (!driver.IsPaused ||
                    driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                    return;
                Capture();
                if (!captureGameView)
                {
                    Finish("PASS", "Physical P1 movement and same-tick central nameplate command matched the camera-derived clamp.");
                    return;
                }
                report.requestedScreenWidth = Screen.width;
                report.requestedScreenHeight = Screen.height;
                Check(report.requestedScreenWidth > 0 &&
                      report.requestedScreenHeight > 0,
                    "The Game View screen dimensions are unavailable.");
                screenCapturePath = Path.ChangeExtension(activeResultPath, ".png");
                Check(!File.Exists(screenCapturePath),
                    "The natural Game View screenshot path already exists.");
                report.screenCapturePath = screenCapturePath;
                ScreenCapture.CaptureScreenshot(screenCapturePath);
                phase = 3;
                deadline = EditorApplication.timeSinceStartup + 30.0;
            }
            catch (Exception exception)
            {
                Finish("FAIL", exception.ToString());
            }
        }

        private static void Capture()
        {
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            BattlePresentationFrame captured = plan.CapturedFrame;
            Check(plan.Owner == BattlePixelFrameOwner.Central &&
                  published != null && captured != null,
                "The central published/captured frame is unavailable.");
            report.captureTick = driver.CurrentTickIndex;
            report.publishedTick = published.TickIndex;
            report.planTick = captured.TickIndex;
            Check(report.captureTick == report.publishedTick &&
                  report.publishedTick == report.planTick,
                "The body and nameplate do not belong to the same completed logic tick.");

            BattlePresentationEntitySnapshot entity = default;
            bool hasEntity = false;
            for (int index = 0; index < captured.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot candidate = captured.GetEntity(index);
                if (candidate.RuntimeSlot != 0)
                    continue;
                entity = candidate;
                hasEntity = true;
                break;
            }
            Check(hasEntity, "The P1 actor snapshot is missing.");

            BattleRenderCommand body = default;
            BattleRenderCommand label = default;
            bool hasBody = false;
            bool hasLabel = false;
            int requestedWordGlyphCommandCount = 0;
            var slotCommands = new List<string>();
            for (int index = 0; index < captured.CommandCount; index++)
            {
                BattleRenderCommand command = captured.GetCommand(index);
                if (command.RuntimeSlot != 0)
                    continue;
                slotCommands.Add(command.Type + "/" + command.MotionAnchor);
                if (command.Type == BattleRenderCommandType.OverlayGlyph &&
                    command.SpriteDescriptor.HasLogicalResourceKey)
                {
                    BattleVisualResourceKey key =
                        command.SpriteDescriptor.LogicalResourceKey;
                    if (key.IsCommonWordGlyph &&
                        key.CommonWordSheetIndex == requestedBattleGroup &&
                        key.CommonWordCharCode ==
                            world.Runtime.SlotLabels.BattleSlotLabels[0, 0])
                        requestedWordGlyphCommandCount++;
                }
                if (!hasBody && command.Type == BattleRenderCommandType.Entity)
                {
                    body = command;
                    hasBody = true;
                }
                if (!hasLabel && command.Type == BattleRenderCommandType.OverlayGlyph &&
                    command.MotionAnchor == BattlePresentationMotionAnchor.Ground)
                {
                    label = command;
                    hasLabel = true;
                }
            }
            report.actorXInt = entity.XInt;
            report.actorBattleGroup = entity.RelationTeam;
            report.requestedWordGlyphCommandCount =
                requestedWordGlyphCommandCount;
            report.entityHasCurrentFrame = entity.HasCurrentFrame;
            report.entityVisible = entity.EntityVisible;
            report.actorState = entity.State;
            report.actorEffectivePic = entity.EffectivePic;
            report.actorVisualDataId = entity.VisualDataId;
            report.actorHitStop = entity.HitStop;
            report.actorSnapshotHasCatalogKey = entity.HasCatalogKey;
            report.boundCatalogCount =
                captured.BoundCatalogForAcceptance?.Count ?? 0;
            report.boundCatalogHasActorSprite =
                captured.BoundCatalogForAcceptance != null &&
                captured.BoundCatalogForAcceptance.TryGet(
                    entity.VisualDataId, entity.EffectivePic, out _);
            report.commandCount = captured.CommandCount;
            report.entityCount = captured.EntityCount;
            report.hasBodyCommand = hasBody;
            report.hasLabelCommand = hasLabel;
            report.slot0Commands = string.Join(",", slotCommands);
            if (captureGameView)
                CaptureVisibleMotion(captured);
            int selectedSheet = entity.RelationTeam >= 1 && entity.RelationTeam <= 5
                ? entity.RelationTeam
                : 0;
            report.selectedWordSheet = selectedSheet;
            report.selectedLabelChar =
                world.Runtime.SlotLabels.BattleSlotLabels[0, 0].ToString();
            report.hasSelectedWordBinding =
                captured.CommonVisualCatalog != null &&
                captured.CommonVisualCatalog.TryGetWordGlyph(
                    selectedSheet,
                    world.Runtime.SlotLabels.BattleSlotLabels[0, 0],
                    out _);
            if (requestedBattleGroup == 5)
                Check(entity.RelationTeam == 5 && selectedSheet == 5 &&
                      report.hasSelectedWordBinding &&
                      requestedWordGlyphCommandCount > 0,
                    "The ordinary group-5 nameplate did not publish a bound WORDS5 glyph.");
            Check(hasBody, "The selected actor body command is missing.");
            if (!hasLabel)
                Check(captureGameView && SceneManager.GetActiveScene().name == "NTSD_Battle",
                    "The first nameplate glyph command is missing.");

            Camera camera = NTSDRenderSpace.WorldCamera;
            NTSDRenderSpace.ViewportTransformSnapshot viewport =
                NTSDRenderSpace.CaptureViewportTransform();
            Check(camera != null && camera.enabled && camera.orthographic &&
                  camera.aspect > 0f && viewport.UnitsPerPixelX > 0f,
                "The active orthographic world camera or viewport mapping is unavailable.");
            float halfWidth = camera.orthographicSize * camera.aspect;
            int visibleLeft = Mathf.CeilToInt(
                (camera.transform.position.x - halfWidth - viewport.Left) /
                viewport.UnitsPerPixelX);
            int visibleRight = Mathf.FloorToInt(
                (camera.transform.position.x + halfWidth - viewport.Left) /
                viewport.UnitsPerPixelX);
            Check(visibleRight > visibleLeft &&
                  entity.XInt >= visibleLeft && entity.XInt < visibleRight &&
                  (captureWordsOnly || entity.XInt > 794),
                captureWordsOnly
                    ? "The actor is outside the actual camera viewport."
                    : "The actor did not remain visible beyond the old 794-pixel clip.");

            report.actorXInt = entity.XInt;
            report.actorSourceX = actor.Runtime.SourceRuleX;
            report.visibleLeft = visibleLeft;
            report.visibleRight = visibleRight;
            report.actualBodyWorldX = body.Position.x;
            report.cameraWorldX = camera.transform.position.x;
            report.cameraOrthographicSize = camera.orthographicSize;
            report.cameraAspect = camera.aspect;
            report.cameraPixelX = camera.pixelRect.x;
            report.cameraPixelY = camera.pixelRect.y;
            report.cameraPixelWidth = camera.pixelWidth;
            report.cameraPixelHeight = camera.pixelHeight;
            report.viewportLeft = viewport.Left;
            report.viewportTop = viewport.Top;
            report.viewportUnitsPerPixelX = viewport.UnitsPerPixelX;
            report.viewportUnitsPerPixelY = viewport.UnitsPerPixelY;
            Vector3 bodyScreen = camera.WorldToScreenPoint(body.Position);
            report.bodyScreenX = bodyScreen.x;
            report.bodyScreenY = bodyScreen.y;
            if (!hasLabel)
                return;

            int labelLength = 0;
            while (labelLength < BattleEntityOverlayLayout.SlotLabelCharacterCapacity &&
                   world.Runtime.SlotLabels.BattleSlotLabels[0, labelLength] != '\0')
                labelLength++;
            Check(labelLength > 0, "The published P1 nameplate label is empty.");
            int expectedX = entity.XInt + (int)entity.RenderOffsetX -
                ((BattleEntityOverlayLayout.GlyphAdvance * labelLength) >> 1) -
                entity.CameraX;
            if (expectedX < visibleLeft)
                expectedX = visibleLeft;
            int maxX = visibleRight -
                BattleEntityOverlayLayout.GlyphAdvance * labelLength - 1;
            if (expectedX > maxX)
                expectedX = maxX;
            Vector3 expected = viewport.ScreenPixelToWorld(
                expectedX, entity.ZInt + entity.RenderShadowOffset10C + 3, 0f);

            report.labelLength = labelLength;
            report.expectedLabelPixelX = expectedX;
            report.expectedLabelWorldX = expected.x;
            report.actualLabelWorldX = label.Position.x;
            Vector3 labelScreen = camera.WorldToScreenPoint(label.Position);
            report.labelScreenX = labelScreen.x;
            report.labelScreenY = labelScreen.y;
            Check(Mathf.Abs(label.Position.x - expected.x) < 0.00001f &&
                  Mathf.Abs(label.Position.y - expected.y) < 0.00001f,
                "The natural same-tick nameplate command differs from the formal viewport formula.");
        }

        private static void CaptureVisibleMotion(BattlePresentationFrame frame)
        {
            var rows = new List<string>();
            for (int index = 0; index < frame.MotionStateCount; index++)
            {
                BattlePresentationMotionState current = frame.GetMotionState(index);
                bool hasBody = false;
                for (int commandIndex = 0; commandIndex < frame.CommandCount; commandIndex++)
                {
                    BattleRenderCommand command = frame.GetCommand(commandIndex);
                    if (command.Type == BattleRenderCommandType.Entity &&
                        command.Handle.Equals(current.Handle))
                    {
                        hasBody = true;
                        break;
                    }
                }
                if (!hasBody)
                    continue;

                report.visibleMotionCount++;
                bool hasPrevious = false;
                BattlePresentationMotionState previous = default;
                for (int previousIndex = 0;
                     previousIndex < frame.PreviousMotionStateCount; previousIndex++)
                {
                    BattlePresentationMotionState candidate =
                        frame.GetPreviousMotionState(previousIndex);
                    if (candidate.Handle.Slot != current.Handle.Slot)
                        continue;
                    previous = candidate;
                    hasPrevious = true;
                    break;
                }
                if (!hasPrevious)
                {
                    rows.Add($"slot={current.Handle.Slot},oid={current.ObjectId}," +
                        $"currentSource={current.HasSourceRulePosition},status=NoPreviousSlot");
                    continue;
                }

                BattlePresentationMotionSampleStatus status =
                    BattlePresentationMotionSampler.Sample(
                        previous, current, frame.PreviousMotionTickIndex,
                        frame.TickIndex, 0.5, 1.0, 1.0, out _);
                if (status != BattlePresentationMotionSampleStatus.NonAdjacentTicks)
                    report.adjacentVisibleMotionCount++;
                if (status == BattlePresentationMotionSampleStatus.SourcePositionUnavailable)
                    report.visibleMissingSourceCount++;
                rows.Add($"slot={current.Handle.Slot},oid={current.ObjectId}," +
                    $"previousSource={previous.HasSourceRulePosition}," +
                    $"currentSource={current.HasSourceRulePosition},status={status}");
            }
            report.visibleMotionSamples = string.Join("|", rows);
        }

        private static void QueueKey(Key key)
        {
            if (keyboard == null)
                return;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
        }

        private static void QueuePlayerLoopOneTickInput(CharacterInputModule input)
        {
            driver.SetPaused(true);
            pausedByProbe = true;
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            report.keyboardDeviceId = keyboard.deviceId;
            report.moveActionMap = input.MoveAction.actionMap?.name;
            var controls = new List<string>(input.MoveAction.controls.Count);
            foreach (InputControl control in input.MoveAction.controls)
                controls.Add(control.path + "#" + control.device.deviceId);
            report.moveActionControls = string.Join("|", controls);
            report.tickBefore = driver.CurrentTickIndex;
            queuedAtDynamicUpdate = dynamicUpdates;
            report.queuedDynamicUpdate = queuedAtDynamicUpdate;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            EditorApplication.QueuePlayerLoopUpdate();
            phase = 4;
            deadline = EditorApplication.timeSinceStartup + 10.0;
        }

        private static void CompletePlayerLoopOneTickInput()
        {
            if (dynamicUpdates <= queuedAtDynamicUpdate)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                return;
            }
            Check(driver.IsPaused &&
                  !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                  driver.CurrentTickIndex == report.tickBefore,
                "The paused Driver changed before the queued Dynamic input was observed.");
            CharacterInputModule input = actor.Controller as CharacterInputModule;
            report.observedDynamicUpdate = dynamicUpdates;
            report.keyboardPressedBeforeTick = keyboard.dKey.isPressed;
            report.moveActionXBeforeTick = input.MoveAction.ReadValue<Vector2>().x;
            report.currentMoveXBeforeTick = input.CurrentMoveInput.x;
            report.activeControlBeforeTick = input.MoveAction.activeControl?.path;
            Check(driver.StepOneTick(ignorePaused: true, buildPresentation: true),
                "The single production Driver tick was rejected.");
            report.tickAfter = driver.CurrentTickIndex;
            Check(report.tickAfter == report.tickBefore + 1,
                "The PlayerLoop input trace skipped or repeated a logic tick.");
            report.lastX = actor.Runtime.XInt;
            CaptureInputTrace(report.tickAfter);
            bool canonicalRight = InputTrace.Count == 1 &&
                (InputTrace[0].heldButtons & (int)SimulationInputButtons.Right) != 0;
            bool inputDelivered = report.keyboardPressedBeforeTick &&
                report.moveActionXBeforeTick > 0.5f && canonicalRight;
            Finish(inputDelivered ? "PASS" : "FIRST_DIFFERENCE",
                inputDelivered
                    ? "Queued D reached the Dynamic device, P1 MoveAction and one complete battle tick."
                    : "The queued PlayerLoop D input did not traverse the full keyboard-to-canonical chain; see tick fields.");
        }

        private static void RunOneTickInputTrace(CharacterInputModule input)
        {
            driver.SetPaused(true);
            pausedByProbe = true;
            report.keyboardDeviceId = keyboard.deviceId;
            report.moveActionMap = input.MoveAction.actionMap?.name;
            var controls = new List<string>(input.MoveAction.controls.Count);
            foreach (InputControl control in input.MoveAction.controls)
                controls.Add(control.path + "#" + control.device.deviceId);
            report.moveActionControls = string.Join("|", controls);
            InputSettings settings = InputSystem.settings;
            InputSettings.BackgroundBehavior previousBackground =
                settings.backgroundBehavior;
            InputSettings.EditorInputBehaviorInPlayMode previousEditor =
                settings.editorInputBehaviorInPlayMode;
            bool previousRunInBackground = Application.runInBackground;
            try
            {
                settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                settings.editorInputBehaviorInPlayMode =
                    InputSettings.EditorInputBehaviorInPlayMode
                        .AllDeviceInputAlwaysGoesToGameView;
                Application.runInBackground = true;
                Check(DynamicInputUpdate != null,
                    "The typed Dynamic Input System update is unavailable.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(moveKey));
                keyboard.MakeCurrent();
                DynamicInputUpdate.Invoke(null, new object[] { InputUpdateType.Dynamic });
                report.keyboardPressedBeforeTick = keyboard[moveKey].isPressed;
                report.moveActionXBeforeTick = input.MoveAction.ReadValue<Vector2>().x;
                report.currentMoveXBeforeTick = input.CurrentMoveInput.x;
                report.activeControlBeforeTick = input.MoveAction.activeControl?.path;
                report.tickBefore = driver.CurrentTickIndex;
                Check(driver.StepOneTick(ignorePaused: true, buildPresentation: true),
                    "The single production Driver tick was rejected.");
                report.tickAfter = driver.CurrentTickIndex;
                Check(report.tickAfter == report.tickBefore + 1,
                    "The single input trace skipped or repeated a logic tick.");
                report.lastX = actor.Runtime.XInt;
                CaptureInputTrace(report.tickAfter);
            }
            finally
            {
                settings.backgroundBehavior = previousBackground;
                settings.editorInputBehaviorInPlayMode = previousEditor;
                Application.runInBackground = previousRunInBackground;
            }
            Finish("PASS", "Typed Dynamic keyboard state and one complete Driver tick were captured.");
        }

        private static void CaptureInputTrace(int tick)
        {
            CharacterInputModule input = actor.Controller as CharacterInputModule;
            FrameInputSet frameInput = driver.LastAppliedFrameInput;
            SimulationPlayerInput playerInput = default;
            if (frameInput?.Players != null)
            {
                for (int index = 0; index < frameInput.Players.Count; index++)
                {
                    if (frameInput.Players[index].PlayerSlot != 0)
                        continue;
                    playerInput = frameInput.Players[index];
                    break;
                }
            }
            InputTrace.Add(new InputTraceRow
            {
                tick = tick,
                keyboardKeyPressed = keyboard != null && keyboard[moveKey].isPressed,
                moveActionEnabled = input?.MoveAction?.enabled == true,
                moveActionX = input?.MoveAction?.enabled == true
                    ? input.MoveAction.ReadValue<Vector2>().x : 0f,
                currentMoveX = input?.CurrentMoveInput.x ?? 0f,
                frameInputTick = frameInput?.TickIndex ?? -1,
                playerSlot = playerInput.PlayerSlot,
                heldButtons = (int)playerInput.Buttons,
                pressedButtons = (int)playerInput.PressedButtons,
                releasedButtons = (int)playerInput.ReleasedButtons,
                keyRight = actor.Runtime.KeyRight,
                cdRight = actor.Runtime.CdRight,
                frame = actor.Frame?.N ?? -1,
                state = actor.Frame?.D?.state ?? -1,
                sourceX = actor.Runtime.SourceRuleX,
                x = actor.Runtime.X,
                xInt = actor.Runtime.XInt,
                vx = actor.Runtime.Vx
            });
        }

        private static void ReleaseKey()
        {
            if (keyboard == null)
                return;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Array.Empty<Key>()));
            InputSystem.Update();
            keyboard = null;
        }

        private static void Finish(string status, string reason)
        {
            EditorApplication.update -= Observe;
            InputSystem.onAfterUpdate -= OnInputUpdate;
            report.status = status;
            report.reason = reason;
            report.inputTrace = InputTrace.ToArray();
            Save();
            ReleaseKey();
            if (pausedByProbe && driver != null &&
                EditorApplication.isPlaying &&
                driver.LifecycleState == BattleRuntimeLifecycleState.Running)
                driver.SetPaused(false);
            pausedByProbe = false;
            if (status == "PASS")
                Debug.Log("[NTSD28Q09NameplateNaturalPlayProbe] PASS: " + reason);
            else
                Debug.LogError("[NTSD28Q09NameplateNaturalPlayProbe] FAIL: " + reason);
            if (ownsMenuBattle && EditorApplication.isPlaying)
                CleanupMenuBattle().Forget();
            driver = null;
            world = null;
            actor = null;
        }

        private static async UniTask CleanupMenuBattle()
        {
            try
            {
                AppManager app = AppManager.Instance;
                if (app != null && SceneManager.GetSceneByName("NTSD_Battle").isLoaded)
                {
                    AsyncOperation unload = app.UnloadBattle();
                    if (unload != null)
                        await unload.ToUniTask();
                }
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.reason += "\nCleanup: " + exception;
                Save();
            }
            finally
            {
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
                ownsMenuBattle = false;
                EditorApplication.delayCall += () =>
                {
                    if (EditorApplication.isPlaying)
                        EditorApplication.ExitPlaymode();
                };
            }
        }

        private static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(activeResultPath));
            File.WriteAllText(activeResultPath, JsonUtility.ToJson(report, true));
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string reason;
            public string scene;
            public string contentRoot;
            public int requestedBattleGroup;
            public string moveKey;
            public int keyboardDeviceId;
            public string moveActionMap;
            public string moveActionControls;
            public bool playerLoopOneTick;
            public int queuedDynamicUpdate;
            public int observedDynamicUpdate;
            public bool keyboardPressedBeforeTick;
            public float moveActionXBeforeTick;
            public float currentMoveXBeforeTick;
            public string activeControlBeforeTick;
            public int tickBefore;
            public int tickAfter;
            public int startTick;
            public int captureTick;
            public int publishedTick;
            public int planTick;
            public int tickAfterScreenCapture;
            public int directionTicks;
            public int initialX;
            public int lastX;
            public int actorXInt;
            public int actorBattleGroup;
            public double actorSourceX;
            public bool entityHasCurrentFrame;
            public bool entityVisible;
            public int actorState;
            public int actorEffectivePic;
            public int actorVisualDataId;
            public int actorHitStop;
            public bool actorSnapshotHasCatalogKey;
            public int boundCatalogCount;
            public bool boundCatalogHasActorSprite;
            public int entityCount;
            public int commandCount;
            public bool hasBodyCommand;
            public bool hasLabelCommand;
            public string slot0Commands;
            public int visibleMotionCount;
            public int adjacentVisibleMotionCount;
            public int visibleMissingSourceCount;
            public string visibleMotionSamples;
            public int selectedWordSheet;
            public string selectedLabelChar;
            public bool hasSelectedWordBinding;
            public int requestedWordGlyphCommandCount;
            public int visibleLeft;
            public int visibleRight;
            public int labelLength;
            public int expectedLabelPixelX;
            public float expectedLabelWorldX;
            public float actualLabelWorldX;
            public float actualBodyWorldX;
            public float cameraWorldX;
            public float cameraOrthographicSize;
            public float cameraAspect;
            public float cameraPixelX;
            public float cameraPixelY;
            public int cameraPixelWidth;
            public int cameraPixelHeight;
            public float viewportLeft;
            public float viewportTop;
            public float viewportUnitsPerPixelX;
            public float viewportUnitsPerPixelY;
            public int requestedScreenWidth;
            public int requestedScreenHeight;
            public int screenCaptureWidth;
            public int screenCaptureHeight;
            public float labelScreenX;
            public float labelScreenY;
            public float bodyScreenX;
            public float bodyScreenY;
            public string screenCapturePath;
            public InputTraceRow[] inputTrace;
        }

        [Serializable]
        private sealed class InputTraceRow
        {
            public int tick;
            public bool keyboardKeyPressed;
            public bool moveActionEnabled;
            public float moveActionX;
            public float currentMoveX;
            public int frameInputTick;
            public int playerSlot;
            public int heldButtons;
            public int pressedButtons;
            public int releasedButtons;
            public int keyRight;
            public int cdRight;
            public int frame;
            public int state;
            public double sourceX;
            public double x;
            public int xInt;
            public double vx;
        }
    }
}
#endif
