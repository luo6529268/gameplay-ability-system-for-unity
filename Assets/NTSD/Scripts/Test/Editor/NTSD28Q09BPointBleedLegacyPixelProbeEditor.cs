#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q09BPointBleedLegacyPixelProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string GameConfigPath =
            "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath =
            "Temp/NTSD28_Q09_BPointBleedLegacy.request.json";
        private const string NaturalRequestPath =
            "Temp/NTSD28_Q09_BPointNaturalLegacy.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-BPOINT-BLEED-LEGACY-001";
        private const int CaptureWidth = 1920;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public bool naturalHit;
            public string runId;
            public long startedUtcTicks;
        }

        [Serializable]
        private sealed class Report
        {
            public string status = "FAIL";
            public string error;
            public string runId;
            public string scope;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public string backend;
            public int sourceOid = 9;
            public int frame;
            public int bpointCount;
            public int highHp;
            public int lowHp;
            public int highMarkCount;
            public int lowMarkCount;
            public int bodyCommandIndex = -1;
            public int markCommandIndex = -1;
            public int activeMarkRenderers;
            public int rejectedMarks;
            public int changedRedPixels;
            public int preparedMarkersBefore;
            public int preparedMarkersAfterRepeatedRender;
            public int preparedMarkersAfterReentry;
            public int staleActiveMarkers = -1;
            public bool stalePublishedFrameRetained;
            public bool adjacentMotionSampled;
            public int adjacentPreviousTick = -1;
            public int adjacentCurrentTick = -1;
            public float expectedMotionOffsetX;
            public float observedMotionOffsetX;
            public int projectedXMin;
            public int projectedXMax;
            public int projectedYMin;
            public int projectedYMax;
            public string highImage;
            public string lowImage;
            public int objectsBefore;
            public int objectsAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int borrowersBefore;
            public int borrowersAfter;
            public bool bodyVisible;
            public bool cameraRestored;
            public bool fixtureFreed;
            public bool naturalHit;
            public int initialNaturalHp;
            public int finalNaturalHp;
            public int firstNaturalDamageTick = -1;
            public int naturalMarkTick = -1;
            public int naturalTicks;
            public int appliedPhysicalAttackTicks;
            public bool authoredAttackSeen;
        }

        private static Request request;
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool pool;
        private static LF2Character fixture;
        private static LF2Character naturalAttacker;
        private static LF2ObjectRenderer fixtureRenderer;
        private static Keyboard naturalKeyboard;
        private static Camera camera;
        private static Color32[] highPixels;
        private static BattleRenderCommand markCommand;
        private static int phase;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int expectedFrameTick;
        private static int expectedNaturalTick = -1;
        private static int naturalSteps;
        private static long observationStartedUtcTicks;
        private static bool pauseCaptured;
        private static bool savedPaused;

        static NTSD28Q09BPointBleedLegacyPixelProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged -= RetireProbeConfig;
            EditorApplication.playModeStateChanged += RetireProbeConfig;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SelectLegacyBeforeSceneLoad()
        {
            Request pending = ReadRequest();
            if (pending == null || !pending.requested || !ValidRunId(pending.runId) ||
                File.Exists(ResultPath(pending.runId)))
                return;

            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            if (source == null || source.BattleContentRuntimeRoot != FormalRoot ||
                (GameConfig.Instance != null && GameConfig.Instance != source))
                return;
            SetGameConfigInstance(null);
            GameConfig copy = UnityEngine.Object.Instantiate(source);
            copy.name = source.name + "(Q09BPointLegacyProbe)";
            copy.hideFlags = HideFlags.DontSave;
            copy.BattlePresentationBackendName =
                nameof(BattlePresentationBackendMode.LegacyOnly);
            GameConfig.Instance = copy;
        }

        private static void RetireProbeConfig(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            if (source == null)
                return;
            GameConfig current = GameConfig.Instance;
            if (current != null && current != source && !IsOwnedClone(current, source))
                return;
            GameConfig[] loaded = Resources.FindObjectsOfTypeAll<GameConfig>();
            if (IsOwnedClone(current, source))
                SetGameConfigInstance(null);
            foreach (GameConfig candidate in loaded)
            {
                if (IsOwnedClone(candidate, source))
                    UnityEngine.Object.DestroyImmediate(candidate);
            }
            if (GameConfig.Instance == null)
                GameConfig.Instance = source;
        }

        private static bool IsOwnedClone(GameConfig candidate, GameConfig source)
        {
            return candidate != null && source != null &&
                   !AssetDatabase.Contains(candidate) &&
                   candidate.name == source.name + "(Q09BPointLegacyProbe)" &&
                   (candidate.hideFlags & HideFlags.DontSave) == HideFlags.DontSave &&
                   candidate.BattlePresentationBackendName ==
                       nameof(BattlePresentationBackendMode.LegacyOnly) &&
                   candidate.BattleContentRuntimeRoot == source.BattleContentRuntimeRoot;
        }

        private static void SetGameConfigInstance(GameConfig value)
        {
            FieldInfo field = typeof(GameConfig).GetField("_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
                throw new InvalidOperationException("GameConfig singleton field changed.");
            field.SetValue(null, value);
        }

        private static Request ReadRequest()
        {
            Request natural = ReadRequestFile(NaturalRequestPath);
            if (natural != null && (natural.requested || natural.running))
            {
                natural.naturalHit = true;
                return natural;
            }
            return ReadRequestFile(RequestPath);
        }

        private static Request ReadRequestFile(string relativePath)
        {
            string path = ProjectPath(relativePath);
            if (!File.Exists(path))
                return null;
            try
            {
                return JsonUtility.FromJson<Request>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            Request pending = ReadRequest();
            if (pending == null || (!pending.requested && !pending.running))
                return;
            if (!EditorApplication.isPlaying)
            {
                if (!pending.requested || pending.running ||
                    EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (!ValidRunId(pending.runId) || scene.path != ScenePath ||
                    scene.isDirty || File.Exists(ResultPath(pending.runId)))
                {
                    WritePreflightFailure(pending);
                    return;
                }
                EditorApplication.EnterPlaymode();
                return;
            }

            if (request == null)
            {
                request = pending;
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(ProjectPath(RequestPathFor(request)),
                    JsonUtility.ToJson(request));
                report = new Report
                {
                    runId = request.runId,
                    naturalHit = request.naturalHit,
                    scope = request.naturalHit
                        ? "Formal Ita natural Naruto physical-J HP threshold, full Driver LegacyOnly body and same-frame marker camera A/B. Formal EXE same-view pixels remain separate."
                        : "Formal Ita controlled HP500/166, original Battle Scene LegacyOnly body and bleed camera pixels. Natural hit and formal EXE same-view pixels remain separate.",
                };
            }
            try
            {
                long start = observationStartedUtcTicks == 0
                    ? request.startedUtcTicks : observationStartedUtcTicks;
                TimeSpan limit = observationStartedUtcTicks == 0
                    ? TimeSpan.FromMinutes(8) : TimeSpan.FromMinutes(3);
                Require(DateTime.UtcNow - new DateTime(start, DateTimeKind.Utc) < limit,
                    "Bounded formal-content or pixel observation timed out.");
                if (phase == 0)
                    Prepare();
                else if (phase == 1)
                    ObserveHigh();
                else if (phase == 2)
                    ObserveLow();
                else if (phase == 3)
                    ObserveNaturalHit();
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
                Cleanup();
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
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            report.backend = driver.PresentationBackendMode.ToString();
            Require(report.contentRoot == FormalRoot &&
                    driver.PresentationBackendMode ==
                        BattlePresentationBackendMode.LegacyOnly &&
                    world.BattlePresentation.Mode ==
                        BattlePresentationBackendMode.LegacyOnly &&
                    GameConfig.Instance != null &&
                    !AssetDatabase.Contains(GameConfig.Instance) &&
                    !world.UsesLogicOnlyEntityMaterialization,
                "Formal LegacyOnly production backend was not selected before Battle preparation.");
            Require(CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                "Formal content has not been published.");
            camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.isActiveAndEnabled,
                "Original Battle world camera is unavailable.");
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

            pool = LF2ObjectPool.TryGetInstance();
            Require(pool != null, "Legacy object pool is unavailable.");
            report.sceneHashBefore = HashFile(ProjectPath(ScenePath));
            report.objectsBefore = world.ObjectCount;
            report.slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.borrowersBefore = pool.ActiveObjectCountForAcceptance;
            int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
            Require(slot >= 50, "No free formal Ita fixture slot.");
            GameObject bodyObject = pool.Get(out fixtureRenderer);
            fixture = LF2ReferencePool.Instance.Get(LF2ObjectType.Character, 9)
                as LF2Character;
            Require(bodyObject != null && fixtureRenderer != null && fixture != null,
                "Production Legacy body pools did not materialize Ita.");
            fixture.Controller.SetInputID(709);
            fixture.InjectDependencies(bodyObject.transform, fixtureRenderer.transform,
                "Q09_Legacy_BPoint_Ita");
            fixture.ModuleInitialize();
            fixture.SetRequiredRuntimeSlot(slot);
            fixtureRenderer.SetLogicObject(fixture, null);
            fixture.ModuleBind(world.RuntimeCharacterConfigs.Resolve(9), 9, world);
            fixture.Initialize(500, 500);
            fixture.AiControlled = false;
            fixture.ImmediateFrame(0);
            fixture.Team = request.naturalHit ? 2 : 1;
            fixture.RelationTeam = request.naturalHit ? 2 : 1;
            if (request.naturalHit)
            {
                Require(world.TryResolveRosterInputEntity(0, out LF2Entity p1),
                    "No bound P1 input entity is available for natural Legacy hit.");
                naturalAttacker = p1 as LF2Character;
                Require(naturalAttacker?.ObjectId == 2 &&
                        (naturalAttacker.Controller as CharacterInputModule)?.AttackAction?.enabled == true &&
                        naturalAttacker.Runtime.SourceRulePositionInitialized,
                    "The saved Battle Scene has no ready P1 Naruto attack actor.");
                naturalKeyboard = Keyboard.current;
                Require(naturalKeyboard != null,
                    "No Input System keyboard is available for natural Legacy hit.");
                int direction = naturalAttacker.Runtime.IsFacingLeft ? -1 : 1;
                fixture.Runtime.SetPosition(
                    naturalAttacker.Runtime.XInt + direction * 40,
                    naturalAttacker.Runtime.YInt,
                    naturalAttacker.Runtime.ZInt);
                fixture.Runtime.SetSourceRulePosition(
                    naturalAttacker.Runtime.SourceRuleXInt + direction * 40,
                    naturalAttacker.Runtime.SourceRuleZInt);
                fixture.Health.HP = 180;
                fixture.Runtime.SyncSourceRuleIntegerPosition();
            }
            else
            {
                fixture.Runtime.SetPosition(1100, 0, world.Runtime.Stage.ZMin + 50);
            }
            fixture.Runtime.SetVelocity(0, 0, 0);
            fixture.Runtime.SyncIntegerPosition();
            fixture.RefreshRuntimeSnapshot();
            report.frame = fixture.Frame.N;
            report.bpointCount = fixture.Frame.D?.BloodPoints?.Count ?? 0;
            report.highHp = fixture.Runtime.HP;
            Require(report.frame == 0 && report.bpointCount == 1 &&
                    report.highHp == (request.naturalHit ? 180 : 500),
                "Current formal Ita standing bpoint fixture is not selected.");
            observationStartedUtcTicks = DateTime.UtcNow.Ticks;
            if (request.naturalHit)
            {
                report.initialNaturalHp = fixture.Runtime.HP;
                InputSystem.QueueStateEvent(naturalKeyboard, new KeyboardState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(naturalKeyboard, new KeyboardState(Key.J));
                InputSystem.Update();
                phase = 3;
                return;
            }
            expectedFrameTick = driver.CurrentTickIndex + 100;
            stableUpdates = 0;
            world.RenderDispatchAll(expectedFrameTick);
            phase = 1;
        }

        private static void ObserveHigh()
        {
            if (!ReadyFrame(out BattlePresentationFrame frame))
                return;
            report.highMarkCount = CountCommands(frame,
                fixture.Runtime.SlotIndex, BattleRenderCommandType.BleedMark,
                out _);
            Require(report.highMarkCount == 0,
                "High-HP formal Ita emitted a Legacy bleed mark.");
            report.bodyVisible = fixtureRenderer.GetComponent<SpriteRenderer>() is
                SpriteRenderer body && body.enabled && body.sprite != null &&
                body.gameObject.activeInHierarchy;
            Require(report.bodyVisible, "Legacy formal Ita body has no live SpriteRenderer.");
            highPixels = Capture("high", out report.highImage);
            fixture.Health.HP = 166;
            fixture.RefreshRuntimeSnapshot();
            report.lowHp = fixture.Runtime.HP;
            Require(report.lowHp <= fixture.Runtime.HP3 / 3,
                "Controlled HP did not cross the formal base-HP threshold.");
            expectedFrameTick++;
            stableUpdates = 0;
            world.RenderDispatchAll(expectedFrameTick);
            phase = 2;
        }

        private static void ObserveNaturalHit()
        {
            Require(driver.IsPaused && ReferenceEquals(driver.World, world) &&
                    !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Paused production World changed during natural Legacy hit.");
            if (expectedNaturalTick < 0)
            {
                if (naturalSteps >= 80)
                {
                    report.status = "NATURAL_HIT_OR_LEGACY_MARK_NOT_OBSERVED";
                    report.error = "Bounded 80-tick original Battle observation ended.";
                    Cleanup();
                    return;
                }
                expectedNaturalTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true,
                    buildPresentation: true),
                    "Production Driver rejected a natural Legacy input tick.");
                return;
            }
            if (driver.CurrentTickIndex < expectedNaturalTick ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            Require(driver.CurrentTickIndex == expectedNaturalTick,
                "Production Driver advanced more than one requested natural Legacy tick.");
            naturalSteps++;
            report.naturalTicks = naturalSteps;
            expectedNaturalTick = -1;
            if (naturalSteps == 2)
            {
                InputSystem.QueueStateEvent(naturalKeyboard, new KeyboardState());
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
            report.authoredAttackSeen |= naturalAttacker.Frame.N == 60 ||
                naturalAttacker.Frame.N == 62 || naturalAttacker.Frame.N == 65 ||
                naturalAttacker.Frame.N == 513;
            report.finalNaturalHp = fixture.Health.HP;
            if (report.firstNaturalDamageTick < 0 &&
                report.finalNaturalHp < report.initialNaturalHp)
                report.firstNaturalDamageTick = driver.CurrentTickIndex;

            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            BattlePixelFramePlan plan = published?.TickIndex == driver.CurrentTickIndex
                ? BattleCentralRenderSystem.PrepareFrame(world)
                : default;
            BattlePresentationFrame frame = plan.IsValid && !plan.IsStale
                ? plan.CapturedFrame : null;
            if (report.firstNaturalDamageTick < 0 ||
                report.finalNaturalHp > fixture.Runtime.HP3 / 3 ||
                fixture.Frame.D?.BloodPoints?.Count != 1 ||
                frame == null || !frame.CommandsMaterialized)
                return;
            int slot = fixture.Runtime.SlotIndex;
            int bodyCount = CountCommands(frame, slot,
                BattleRenderCommandType.Entity, out int bodyIndex);
            int markCount = CountCommands(frame, slot,
                BattleRenderCommandType.BleedMark, out int markIndex);
            if (bodyCount != 1 || markCount != 1 || markIndex <= bodyIndex)
                return;

            report.bodyCommandIndex = bodyIndex;
            report.markCommandIndex = markIndex;
            report.lowMarkCount = markCount;
            report.naturalMarkTick = driver.CurrentTickIndex;
            report.frame = fixture.Frame.N;
            report.bpointCount = fixture.Frame.D.BloodPoints.Count;
            report.lowHp = report.finalNaturalHp;
            markCommand = frame.GetCommand(markIndex);
            Require(report.appliedPhysicalAttackTicks > 0 &&
                    report.authoredAttackSeen && report.finalNaturalHp > 0 &&
                    report.initialNaturalHp > fixture.Runtime.HP3 / 3 &&
                    Mathf.RoundToInt(markCommand.Size.x) == 1 &&
                    Mathf.RoundToInt(markCommand.Size.y) == 3,
                "Natural hit, HP threshold or formal bpoint geometry failed.");

            BattleEntityOverlayRenderer owner =
                driver.GetComponent<BattleEntityOverlayRenderer>();
            Require(owner != null, "Prepared Legacy bleed owner is missing.");
            owner.RenderAll(world);
            SpriteRenderer active = null;
            report.activeMarkRenderers = 0;
            foreach (SpriteRenderer renderer in
                     owner.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.name != "BattleBleedMark" ||
                    !renderer.gameObject.activeInHierarchy)
                    continue;
                report.activeMarkRenderers++;
                active = renderer;
            }
            report.rejectedMarks = owner.RejectedBleedMarkCountForDiagnostics;
            Require(report.activeMarkRenderers == 1 && active != null &&
                    report.rejectedMarks == 0 &&
                    active.sortingLayerID == markCommand.SortingLayerId &&
                    active.sortingOrder == markCommand.SortOrder &&
                    active.color == markCommand.Color,
                "Natural Legacy command was not materialized as one matching mark.");
            Require(camera != null && camera.isActiveAndEnabled &&
                    fixtureRenderer.GetComponent<SpriteRenderer>() is
                        SpriteRenderer body && body.enabled && body.sprite != null,
                "Natural Legacy body or world camera is unavailable.");

            active.gameObject.SetActive(false);
            try
            {
                highPixels = Capture("high", out report.highImage);
            }
            finally
            {
                active.gameObject.SetActive(true);
            }
            Color32[] markedPixels = Capture("low", out report.lowImage);
            ComparePixels(highPixels, markedPixels);
            Require(report.changedRedPixels > 0,
                "Naturally triggered Legacy mark contributed no camera pixel.");
            report.status = "PASS_NATURAL_HIT_LEGACY_PIXEL";
            Cleanup();
        }

        private static void ObserveLow()
        {
            if (!ReadyFrame(out BattlePresentationFrame frame))
                return;
            int slot = fixture.Runtime.SlotIndex;
            report.lowMarkCount = CountCommands(frame, slot,
                BattleRenderCommandType.BleedMark, out int markIndex);
            report.bodyCommandIndex = CountCommands(frame, slot,
                BattleRenderCommandType.Entity, out int bodyIndex) == 1
                ? bodyIndex : -1;
            report.markCommandIndex = markIndex;
            Require(report.lowMarkCount == 1 &&
                    markIndex > report.bodyCommandIndex &&
                    report.bodyCommandIndex >= 0,
                "Legacy bleed command is not after the formal Ita body.");
            markCommand = frame.GetCommand(markIndex);
            BattleEntityOverlayRenderer owner =
                driver.GetComponent<BattleEntityOverlayRenderer>();
            Require(owner != null, "Legacy bleed owner was not prepared by Driver.");
            report.rejectedMarks = owner.RejectedBleedMarkCountForDiagnostics;
            Require(report.rejectedMarks == 0,
                "Prepared Legacy bleed capacity rejected a formal mark.");
            SpriteRenderer active = null;
            foreach (SpriteRenderer renderer in
                     owner.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.name != "BattleBleedMark" ||
                    !renderer.gameObject.activeInHierarchy)
                    continue;
                report.activeMarkRenderers++;
                active = renderer;
            }
            Require(report.activeMarkRenderers == 1 && active != null &&
                    active.sprite != null &&
                    active.sprite.texture == Texture2D.whiteTexture &&
                    active.color == markCommand.Color &&
                    active.sortingLayerID == markCommand.SortingLayerId &&
                    active.sortingOrder == markCommand.SortOrder &&
                    active.gameObject.layer == LayerMask.NameToLayer("Battle") &&
                    Vector3.Distance(active.transform.localScale,
                        new Vector3(
                            markCommand.Size.x * NTSDRenderSpace.BattleVisualScale,
                            markCommand.Size.y * NTSDRenderSpace.BattleVisualScale,
                            1f)) < 0.0001f &&
                    Vector3.Distance(active.transform.position,
                        markCommand.Position) < 0.0001f,
                "Published Legacy mark was not materialized with matching geometry/order.");
            Color32[] lowPixels = Capture("low", out report.lowImage);
            ComparePixels(highPixels, lowPixels);
            Require(report.changedRedPixels > 0,
                "Legacy mark contributed no independently attributable red camera pixel.");

            report.preparedMarkersBefore = CountPreparedMarkers(owner);
            owner.RenderAll(world);
            report.preparedMarkersAfterRepeatedRender =
                CountPreparedMarkers(owner);
            Require(report.preparedMarkersBefore > 0 &&
                    report.preparedMarkersAfterRepeatedRender ==
                    report.preparedMarkersBefore &&
                    CountActiveMarkers(owner) == 1,
                "Repeated sealed Legacy materialization created or lost a mark.");

            owner.StopForBattleShutdown();
            owner.StopForBattleShutdown();
            owner.PrepareBattleCapacity(report.preparedMarkersBefore);
            report.preparedMarkersAfterReentry = CountPreparedMarkers(owner);
            owner.RenderAll(world);
            Require(report.preparedMarkersAfterReentry ==
                    report.preparedMarkersBefore &&
                    CountActiveMarkers(owner) == 1,
                "Legacy mark was not reused after idempotent stop/re-prepare.");

            BattlePresentationFrame staleFrame = ValidateAdjacentMotion(owner);

            fixture.FreeEntityLikeExe();
            fixture = null;
            fixtureRenderer = null;
            world.FlushPendingDestroyForDiagnostics();
            report.stalePublishedFrameRetained =
                ReferenceEquals(world.BattlePresentation.PublishedFrame, staleFrame);
            owner.RenderAll(world);
            report.staleActiveMarkers = CountActiveMarkers(owner);
            Require(report.stalePublishedFrameRetained &&
                    report.staleActiveMarkers == 0,
                "A published Legacy bleed command survived its entity handle.");

            report.status = "PASS_CONTROLLED_LEGACY_PIXEL_MOTION_STALE_HANDLE";
            Cleanup();
        }

        private static BattlePresentationFrame ValidateAdjacentMotion(
            BattleEntityOverlayRenderer owner)
        {
            int slot = fixture.Runtime.SlotIndex;
            double sourceX = fixture.Runtime.X;
            double sourceZ = fixture.Runtime.Z;
            fixture.Runtime.SetSourceRulePosition(sourceX, sourceZ);
            fixture.Runtime.SyncSourceRuleIntegerPosition();
            fixture.RefreshRuntimeSnapshot();
            int baselineTick = ++expectedFrameTick;
            world.RenderDispatchAll(baselineTick);
            Require(CountCommands(world.BattlePresentation.PublishedFrame, slot,
                    BattleRenderCommandType.BleedMark, out _) == 1,
                "Adjacent Legacy motion baseline has no formal Ita mark.");

            fixture.Runtime.SetPosition(
                fixture.Runtime.X + 20.0 * world.FixedViewRunDistanceScale,
                fixture.Runtime.Y, fixture.Runtime.Z);
            fixture.Runtime.SetSourceRulePosition(sourceX + 20.0, sourceZ);
            fixture.Runtime.SyncSourceRuleIntegerPosition();
            fixture.Runtime.SyncIntegerPosition();
            fixture.RefreshRuntimeSnapshot();
            int movedTick = ++expectedFrameTick;
            world.RenderDispatchAll(movedTick);
            BattlePresentationFrame moved = world.BattlePresentation.PublishedFrame;
            report.adjacentPreviousTick = moved.PreviousMotionTickIndex;
            report.adjacentCurrentTick = moved.TickIndex;
            bool hasMark = CountCommands(moved, slot,
                BattleRenderCommandType.BleedMark, out int index) == 1;
            Require(moved.PreviousMotionTickIndex == baselineTick &&
                    moved.TickIndex == movedTick && hasMark,
                "Adjacent Legacy motion frame was not published.");

            BattleRenderCommand command = moved.GetCommand(index);
            var display = new BattlePresentationDisplayMotion();
            display.Prepare(moved, 0.5,
                world.FixedViewRunDistanceScale,
                world.FixedViewRunVerticalDistanceScale);
            report.adjacentMotionSampled =
                display.TryGet(command.Handle, out BattlePresentationMotionDelta delta);
            Vector3 expectedOffset = BattlePresentationDisplayMotion.ToWorldBody(delta);
            report.expectedMotionOffsetX = expectedOffset.x;
            Require(report.adjacentMotionSampled &&
                    Mathf.Abs(expectedOffset.x) > 0.0001f,
                "The adjacent formal Ita source movement was not accepted.");

            FieldInfo clockWorld = typeof(BattleCentralRenderSystem).GetField(
                "displayClockWorld", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo clockAlpha = typeof(BattleCentralRenderSystem).GetField(
                "lastResolvedDisplayAlpha", BindingFlags.Static | BindingFlags.NonPublic);
            Require(clockWorld != null && clockAlpha != null,
                "Presentation display-clock diagnostic fields changed.");
            object savedWorld = clockWorld.GetValue(null);
            object savedAlpha = clockAlpha.GetValue(null);
            try
            {
                clockWorld.SetValue(null, world);
                clockAlpha.SetValue(null, 0.5d);
                owner.RenderAll(world);
                SpriteRenderer active = null;
                foreach (SpriteRenderer renderer in
                         owner.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (renderer.name == "BattleBleedMark" &&
                        renderer.gameObject.activeInHierarchy)
                        active = renderer;
                }
                Require(CountActiveMarkers(owner) == 1 && active != null,
                    "Adjacent Legacy mark did not materialize.");
                report.observedMotionOffsetX =
                    active.transform.position.x - command.Position.x;
                Require(Vector3.Distance(active.transform.position,
                            command.Position + expectedOffset) < 0.0001f &&
                        Mathf.Abs(report.observedMotionOffsetX -
                            report.expectedMotionOffsetX) < 0.0001f,
                    "Adjacent Legacy mark missed the accepted body display delta.");
            }
            finally
            {
                clockAlpha.SetValue(null, savedAlpha);
                clockWorld.SetValue(null, savedWorld);
            }
            return moved;
        }

        private static int CountPreparedMarkers(BattleEntityOverlayRenderer owner)
        {
            int count = 0;
            foreach (SpriteRenderer renderer in
                     owner.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.name == "BattleBleedMark")
                    count++;
            return count;
        }

        private static int CountActiveMarkers(BattleEntityOverlayRenderer owner)
        {
            int count = 0;
            foreach (SpriteRenderer renderer in
                     owner.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.name == "BattleBleedMark" &&
                    renderer.gameObject.activeInHierarchy)
                    count++;
            return count;
        }

        private static bool ReadyFrame(out BattlePresentationFrame frame)
        {
            frame = world?.BattlePresentation.PublishedFrame;
            Require(driver.IsPaused && ReferenceEquals(driver.World, world) &&
                    driver.CurrentTickIndex == stableTick &&
                    !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Paused Battle World changed during Legacy capture.");
            if (frame == null || frame.TickIndex != expectedFrameTick ||
                !frame.CommandsMaterialized)
                return false;
            return ++stableUpdates >= 3;
        }

        private static int CountCommands(BattlePresentationFrame frame, int slot,
            BattleRenderCommandType type, out int lastIndex)
        {
            int count = 0;
            lastIndex = -1;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.RuntimeSlot != slot || command.Type != type)
                    continue;
                count++;
                lastIndex = index;
            }
            return count;
        }

        private static Color32[] Capture(string suffix, out string imagePath)
        {
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            var target = new RenderTexture(CaptureWidth, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var saved = new CameraState(camera);
            RenderTexture priorActive = RenderTexture.active;
            Texture2D readback = null;
            imagePath = null;
            try
            {
                target.Create();
                camera.cullingMask = LayerMask.GetMask("Battle");
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.white;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, height,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0, 0, CaptureWidth, height),
                    0, 0, false);
                readback.Apply(false, false);
                imagePath = ResultRoot + "/" + request.runId + "-" + suffix + ".png";
                string fullPath = ProjectPath(imagePath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllBytes(fullPath, readback.EncodeToPNG());
                return readback.GetPixels32();
            }
            finally
            {
                RenderTexture.active = priorActive;
                saved.Restore(camera);
                report.cameraRestored = saved.Matches(camera) &&
                    (suffix == "high" || report.cameraRestored);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void ComparePixels(Color32[] high, Color32[] low)
        {
            Require(high != null && low != null && high.Length == low.Length &&
                    report.cameraRestored,
                "Legacy camera A/B was incomplete.");
            int height = high.Length / CaptureWidth;
            float width = markCommand.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                          NTSDRenderSpace.BattleVisualScale;
            float markHeight = markCommand.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                               NTSDRenderSpace.BattleVisualScale;
            float left = markCommand.Position.x - markCommand.Pivot.x * width;
            float bottom = markCommand.Position.y - markCommand.Pivot.y * markHeight;
            Vector3 a = camera.WorldToViewportPoint(new Vector3(left, bottom,
                markCommand.Position.z));
            Vector3 b = camera.WorldToViewportPoint(new Vector3(left + width,
                bottom + markHeight, markCommand.Position.z));
            int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x, b.x) * CaptureWidth) - 1,
                0, CaptureWidth);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x, b.x) * CaptureWidth) + 1,
                0, CaptureWidth);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y, b.y) * height) - 1,
                0, height);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y, b.y) * height) + 1,
                0, height);
            report.projectedXMin = x0;
            report.projectedXMax = x1;
            report.projectedYMin = y0;
            report.projectedYMax = y1;
            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++)
                {
                    int index = y * CaptureWidth + x;
                    Color32 before = high[index];
                    Color32 after = low[index];
                    if (after.r < 170 || after.g > 90 || after.b > 90 ||
                        after.r - before.r < 45)
                        continue;
                    report.changedRedPixels++;
                }
            }
        }

        private static void Cleanup()
        {
            try
            {
                if (naturalKeyboard != null)
                {
                    InputSystem.QueueStateEvent(naturalKeyboard, new KeyboardState());
                    InputSystem.Update();
                }
                if (fixture != null && world != null)
                {
                    fixture.FreeEntityLikeExe();
                    world.FlushPendingDestroyForDiagnostics();
                    world.RenderDispatchAll(driver.CurrentTickIndex, true);
                }
                else if (fixtureRenderer != null && pool != null)
                {
                    pool.Release(fixtureRenderer);
                }
                report.fixtureFreed = fixture == null ||
                    fixture.RegisteredWorldForSimulation == null;
                if (world != null && report.sceneHashBefore != null)
                {
                    report.objectsAfter = world.ObjectCount;
                    report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                    report.borrowersAfter = pool.ActiveObjectCountForAcceptance;
                    report.sceneHashAfter = HashFile(ProjectPath(ScenePath));
                    if (!report.fixtureFreed ||
                        report.objectsAfter != report.objectsBefore ||
                        report.slotsAfter != report.slotsBefore ||
                        report.borrowersAfter != report.borrowersBefore ||
                        report.sceneHashAfter != report.sceneHashBefore)
                    {
                        report.status = "FAIL";
                        report.error += " Fixture, pool or Scene was not restored.";
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
                Finish();
            }
        }

        private static void WritePreflightFailure(Request pending)
        {
            string path = ResultPath(pending.runId);
            if (ValidRunId(pending.runId) && !File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(new Report
                {
                    status = "FAIL",
                    runId = pending.runId,
                    error = "A clean original Battle Scene and unique runId are required.",
                }, true));
            }
            pending.requested = false;
            pending.running = false;
            File.WriteAllText(ProjectPath(RequestPathFor(pending)),
                JsonUtility.ToJson(pending));
        }

        private static void Finish()
        {
            if (request != null && ValidRunId(request.runId))
            {
                string path = ResultPath(request.runId);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!File.Exists(path))
                    File.WriteAllText(path, JsonUtility.ToJson(report, true));
                request.requested = false;
                request.running = false;
                File.WriteAllText(ProjectPath(RequestPathFor(request)),
                    JsonUtility.ToJson(request));
            }
            request = null;
            report = null;
            driver = null;
            world = null;
            pool = null;
            fixture = null;
            naturalAttacker = null;
            fixtureRenderer = null;
            naturalKeyboard = null;
            camera = null;
            highPixels = null;
            phase = 0;
            expectedNaturalTick = -1;
            naturalSteps = 0;
            stableTick = -1;
            stableUpdates = 0;
            observationStartedUtcTicks = 0;
            pauseCaptured = false;
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private readonly struct CameraState
        {
            private readonly int cullingMask;
            private readonly CameraClearFlags clearFlags;
            private readonly Color background;
            private readonly bool hdr;
            private readonly bool msaa;
            private readonly RenderTexture target;

            public CameraState(Camera source)
            {
                cullingMask = source.cullingMask;
                clearFlags = source.clearFlags;
                background = source.backgroundColor;
                hdr = source.allowHDR;
                msaa = source.allowMSAA;
                target = source.targetTexture;
            }

            public void Restore(Camera source)
            {
                source.targetTexture = target;
                source.cullingMask = cullingMask;
                source.clearFlags = clearFlags;
                source.backgroundColor = background;
                source.allowHDR = hdr;
                source.allowMSAA = msaa;
            }

            public bool Matches(Camera source) =>
                source.targetTexture == target &&
                source.cullingMask == cullingMask &&
                source.clearFlags == clearFlags &&
                source.backgroundColor == background &&
                source.allowHDR == hdr &&
                source.allowMSAA == msaa;
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

        private static string RequestPathFor(Request value) =>
            value != null && value.naturalHit ? NaturalRequestPath : RequestPath;

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
