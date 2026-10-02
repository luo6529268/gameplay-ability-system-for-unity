#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly string ResultPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-Q09-NAMEPLATE-VIEWPORT-CLAMP-AUDIT-20260927",
            "original-battle-natural-nameplate-play.json"));
        private static readonly string GameViewOutputFolder = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-336B44-Q09-NATURAL-GAMEVIEW-TICK-001"));

        private static readonly List<LF2Entity> Entities = new List<LF2Entity>(16);
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

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            Start(false);
        }

        [MenuItem(GameViewMenuPath)]
        public static void RunGameViewCapture()
        {
            Start(true);
        }

        private static void Start(bool captureScreen)
        {
            EditorApplication.update -= Observe;
            ReleaseKey();
            captureGameView = captureScreen;
            string runId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" +
                Guid.NewGuid().ToString("N");
            activeResultPath = captureScreen
                ? Path.Combine(GameViewOutputFolder, "natural-nameplate-" + runId + ".json")
                : ResultPath;
            if (captureScreen && (File.Exists(activeResultPath) ||
                                  File.Exists(Path.ChangeExtension(activeResultPath, ".png"))))
                throw new IOException("The natural Game View output path already exists.");
            screenCapturePath = null;
            report = new Report
            {
                status = "RUNNING",
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
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
            pausedByProbe = false;
            ownsMenuBattle = SceneManager.GetActiveScene().name == "NTSD_Menu";
            deadline = EditorApplication.timeSinceStartup + 180.0;
            if (ownsMenuBattle)
                PrepareMenuBattle().Forget();
            else
                EditorApplication.update += Observe;
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
                    team = 1, inputId = 1,
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
                    moveKey = moveRight ? Key.D : Key.A;
                    startTick = driver.CurrentTickIndex;
                    lastObservedTick = startTick;
                    report.startTick = startTick;
                    report.initialX = firstX;
                    report.moveKey = moveKey.ToString();
                    QueueKey(moveKey);
                    phase = 1;
                    deadline = EditorApplication.timeSinceStartup + 35.0;
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
                    Check(tick - startTick <= 180,
                        "P1 did not reach the nameplate viewport gate in 180 logic ticks.");
                    if (directionTicks < 2 ||
                        Math.Abs(actor.Runtime.XInt - firstX) < 8 ||
                        actor.Runtime.XInt <= 794 ||
                        (moveRight && actor.Runtime.XInt < 850) ||
                        (!moveRight && actor.Runtime.XInt > 1100) ||
                        !LF2ObjectRenderer.ShouldDrawEntityForHitStop(actor.Runtime.HitStop))
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
            var slotCommands = new List<string>();
            for (int index = 0; index < captured.CommandCount; index++)
            {
                BattleRenderCommand command = captured.GetCommand(index);
                if (command.RuntimeSlot != 0)
                    continue;
                slotCommands.Add(command.Type + "/" + command.MotionAnchor);
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
            int selectedSheet = entity.RelationTeam >= 1 && entity.RelationTeam <= 4
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
                  entity.XInt > 794 && entity.XInt < visibleRight,
                "The actor did not remain visible beyond the old 794-pixel clip.");

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
            report.status = status;
            report.reason = reason;
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
            public string moveKey;
            public int startTick;
            public int captureTick;
            public int publishedTick;
            public int planTick;
            public int tickAfterScreenCapture;
            public int directionTicks;
            public int initialX;
            public int lastX;
            public int actorXInt;
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
        }
    }
}
#endif
