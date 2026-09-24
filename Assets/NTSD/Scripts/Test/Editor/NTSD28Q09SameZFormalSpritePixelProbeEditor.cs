#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q09SameZFormalSpritePixelProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q09_SameZFormalSpritePixel.request";
        private const string ResultPath = "Temp/NTSD28_Q09_SameZFormalSpritePixel.result.json";
        private const string NaturalRequestPath = "Temp/NTSD28_Q09_SameZNaturalTickPixel.request";
        private const int CaptureWidth = 1920;
        private const int FixtureX = 1100;
        private const int ReferenceX = 1400;
        private const int FixtureZ = 240;
        private const int HiddenX = -10000;
        private static readonly Color32 Clear = new Color32(255, 255, 255, 255);
        private static readonly List<LF2Entity> Before = new List<LF2Entity>(128);
        private static readonly List<LF2Entity> After = new List<LF2Entity>(128);
        private static readonly List<LF2Entity> Owned = new List<LF2Entity>(4);
        private static readonly List<PendingSoundEvent> Sounds = new List<PendingSoundEvent>(16);

        private static bool running;
        private static bool requestMode;
        private static bool naturalTickMode;
        private static string naturalVariant;
        private static bool pauseCaptured;
        private static bool previousPaused;
        private static bool baselineCaptured;
        private static double deadline;
        private static int stableTick;
        private static int stableUpdates;
        private static int realTick;
        private static int expectedGeneration;
        private static int captureIndex;
        private static int baselineObjects;
        private static int baselineSlots;
        private static int baselineRenderers;
        private static int baselineLogicPool;
        private static uint rngState;
        private static ulong rngCalls;
        private static NTSD28NativeRandomScalarState nativeRng;
        private static string sceneHash;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPointFactory factory;
        private static LF2ObjectPool objectPool;
        private static ProbeProducer producer;
        private static LF2Entity a;
        private static LF2Entity b;
        private static RuntimeEntityHandle aHandle;
        private static RuntimeEntityHandle bHandle;
        private static Camera camera;
        private static Color32[][] captures;
        private static int captureHeight;
        private static RectInt overlapBounds;
        private static Report report;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        [MenuItem("NTSD/Battle Diagnostics/Q09/Run Same Z Formal Sprite Pixel Probe")]
        public static void Run()
        {
            if (!running)
                Start(false, false);
        }

        [MenuItem("NTSD/Battle Diagnostics/Q09/Run Same Z Natural Tick Both Capture")]
        public static void RunNaturalTick()
        {
            if (!running)
                Start(false, true, "both");
        }

        [MenuItem("NTSD/Battle Diagnostics/Q09/Inspect Same Z Scene Dirty State")]
        public static void InspectSceneDirtyState()
        {
            Scene scene = SceneManager.GetSceneByName("NTSD_Battle");
            var details = new List<string>();
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Transform member in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (EditorUtility.IsDirty(member.gameObject))
                            details.Add(member.name + "/GameObject");
                        foreach (Component component in member.GetComponents<Component>())
                        {
                            if (component == null)
                                continue;
                            if (EditorUtility.IsDirty(component))
                                details.Add(member.name + "/" + component.GetType().Name);
                            if (component is Camera sceneCamera)
                                details.Add(member.name + "/Camera.enabled=" +
                                    sceneCamera.enabled);
                        }
                    }
                }
            }
            var diagnostic = new SceneDirtyDiagnostic
            {
                sceneValid = scene.IsValid(),
                sceneLoaded = scene.IsValid() && scene.isLoaded,
                sceneDirty = scene.IsValid() && scene.isDirty,
                entries = details.ToArray(),
            };
            string output = ProjectPath("Temp/NTSD28_Q09_SameZSceneDirty.json");
            File.WriteAllText(output, JsonUtility.ToJson(diagnostic, true));
        }

        private static void Start(bool fromRequest, bool naturalTick, string variant = null)
        {
            running = true;
            requestMode = fromRequest;
            naturalTickMode = naturalTick;
            string requestedVariant = string.IsNullOrWhiteSpace(variant)
                ? "both" : variant.Trim().ToLowerInvariant();
            naturalVariant = naturalTick &&
                (requestedVariant == "baseline" || requestedVariant == "a" ||
                 requestedVariant == "b" || requestedVariant == "both")
                ? requestedVariant : naturalTick ? "invalid" : string.Empty;
            pauseCaptured = false;
            baselineCaptured = false;
            stableTick = -1;
            stableUpdates = 0;
            captureIndex = -1;
            driver = null;
            world = null;
            factory = null;
            objectPool = null;
            producer = null;
            a = null;
            b = null;
            camera = null;
            captures = new Color32[4][];
            Owned.Clear();
            deadline = EditorApplication.timeSinceStartup + 180.0;
            report = new Report { status = "FAIL", evidenceScope = naturalTick
                ? "One original Battle Scene complete production tick CentralOnly GPU capture; four separate Play variants required for pixel oracle"
                : "Original Battle Scene controlled presentation publication and world-camera GPU readback; not a natural full battle tick or formal EXE pixel A/B" };
            try
            {
                Require(!naturalTick || naturalVariant == "baseline" ||
                        naturalVariant == "a" || naturalVariant == "b" ||
                        naturalVariant == "both",
                    "Natural capture variant must be baseline, a, b, or both.");
                report.variant = naturalVariant;
                Scene scene = SceneManager.GetSceneByName("NTSD_Battle");
                Require(scene.IsValid() && scene.isLoaded && !scene.isDirty,
                    "The saved original NTSD_Battle Scene must be loaded and clean.");
                report.scenePath = scene.path;
                sceneHash = HashFile(ProjectPath(scene.path));
                report.sceneHashBefore = sceneHash;
            }
            catch (Exception exception)
            {
                Finish("Scene precondition: " + exception.Message);
            }
        }

        private static void Update()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (!running)
            {
                if (File.Exists(ProjectPath(NaturalRequestPath)))
                    Start(true, true, File.ReadAllText(ProjectPath(NaturalRequestPath)));
                else if (File.Exists(ProjectPath(RequestPath)))
                    Start(true, false);
                return;
            }
            try
            {
                Require(EditorApplication.timeSinceStartup < deadline,
                    "Timed out waiting for " + report.waitingFor + ".");
                if (!EditorApplication.isPlaying)
                {
                    Require(!pauseCaptured, "Play ended before cleanup.");
                    Require(requestMode, "Menu invocation requires existing Battle Scene Play.");
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        EditorApplication.EnterPlaymode();
                    return;
                }
                Require(SceneManager.GetSceneByName("NTSD_Battle").isLoaded,
                    "Original Battle Scene unloaded during probe.");
                if (requestMode && File.Exists(ProjectPath(ActiveRequestPath)))
                    File.Delete(ProjectPath(ActiveRequestPath));
                if (captureIndex == -1)
                {
                    Prepare();
                    return;
                }
                if (captureIndex == -2)
                {
                    ObserveNaturalTick();
                    return;
                }
                ObserveCapture();
            }
            catch (Exception exception)
            {
                Finish(exception.ToString());
            }
        }

        private static void Prepare()
        {
            report.waitingFor = "production World, formal catalog and idle worker";
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5)
                return;
            Require(driver.PresentationBackendMode == BattlePresentationBackendMode.CentralOnly,
                "Production presentation backend is not CentralOnly.");
            factory = LF2ObjectPointFactory.Instance;
            objectPool = LF2ObjectPool.Instance;
            Require(factory != null && objectPool != null && LF2ReferencePool.Instance != null,
                "Production OPoint factory or pools are unavailable.");
            Require(CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                "Formal runtime content has not been published.");
            camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.enabled && camera.gameObject.activeInHierarchy,
                "The original world camera is absent or disabled.");
            if (!pauseCaptured)
            {
                previousPaused = driver.IsPaused;
                pauseCaptured = true;
                driver.SetPaused(true);
                deadline = EditorApplication.timeSinceStartup + 15.0;
                return;
            }
            Require(ReferenceEquals(driver.World, world), "Production World changed during pause.");
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                "Dedicated simulation worker failed.");
            if (!driver.IsPaused || driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 4)
                return;
            realTick = driver.CurrentTickIndex;
            report.realTick = realTick;
            CaptureBaseline();
            SpawnFixtures();
            if (naturalTickMode)
                BeginNaturalTick();
            else
                QueueCapture(0);
        }

        private static string ActiveRequestPath => naturalTickMode
            ? NaturalRequestPath : RequestPath;

        private static string ActiveResultPath => naturalTickMode
            ? "Temp/NTSD28_Q09_SameZNaturalTickPixel." + naturalVariant + ".result.json"
            : ResultPath;

        private static string ActiveArtifactFolder => naturalTickMode
            ? "NTSD28-Q09-SAME-Z-NATURAL-TICK-PIXEL-WITNESS-001"
            : "NTSD28-Q09-SAME-Z-FORMAL-SPRITE-PIXEL-WITNESS-001";

        private static void BeginNaturalTick()
        {
            int ax = naturalVariant == "a" || naturalVariant == "both"
                ? FixtureX : ReferenceX;
            int bx = naturalVariant == "b" || naturalVariant == "both"
                ? FixtureX : ReferenceX;
            a.SetPos(ax, 0, FixtureZ);
            b.SetPos(bx, 0, FixtureZ);
            a.Runtime.SyncIntegerPosition();
            b.Runtime.SyncIntegerPosition();
            a.RefreshRuntimeSnapshot();
            b.RefreshRuntimeSnapshot();
            report.naturalTick = realTick + 1;
            report.workerPath = driver.DedicatedSimulationWorkerActiveForDiagnostics;
            captureIndex = -2;
            report.waitingFor = "complete production tick and its central LateUpdate plan";
            deadline = EditorApplication.timeSinceStartup + 15.0;
            bool accepted = report.workerPath
                ? driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(true)
                : driver.StepOneTick(ignorePaused: true, buildPresentation: true);
            Require(accepted, "Production driver rejected the complete diagnostic tick: " +
                driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
        }

        private static void ObserveNaturalTick()
        {
            Require(driver.IsPaused && ReferenceEquals(driver.World, world),
                "Battle pause or World changed during complete tick witness.");
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                "Dedicated worker failed during complete tick witness.");
            if (driver.CurrentTickIndex != report.naturalTick ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            if (!plan.IsValid || plan.SimulationTick != report.naturalTick ||
                plan.Owner != BattlePixelFrameOwner.Central || plan.IsStale ||
                plan.Submission == null || plan.CapturedFrame == null ||
                !plan.CapturedFrame.CommandsMaterialized)
                return;
            Require(plan.CapturedFrame.TickIndex == report.naturalTick,
                "Complete tick plan and captured frame tick differ.");
            Require(a?.Match == world && b?.Match == world &&
                    a.Runtime.SlotIndex == aHandle.Slot &&
                    b.Runtime.SlotIndex == bHandle.Slot,
                "Both formal weapons must survive the complete tick in their slots.");
            report.aFrame = a.Frame.N;
            report.bFrame = b.Frame.N;
            report.aPic = a.GetRenderPicIndex();
            report.bPic = b.GetRenderPicIndex();
            report.aZ = a.Runtime.ZInt;
            report.bZ = b.Runtime.ZInt;
            report.aRuntimeX = a.Runtime.XInt;
            report.bRuntimeX = b.Runtime.XInt;
            captureHeight = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            if (naturalVariant == "both")
                ValidateCompositeCommands(plan.CapturedFrame);
            else
                ValidateReferenceCommands(plan.CapturedFrame);
            captureIndex = 3;
            captures[3] = CapturePixels(camera);
            report.postRngState = world.Rng.State.ToString();
            report.postRngCalls = world.Rng.CallCount.ToString();
            report.postNativeCrtState = world.NativeRandom.CaptureScalarState().CrtState.ToString();
            report.status = "PASS_CAPTURE";
            report.message = "Actual complete-tick CentralOnly GPU capture for variant " +
                naturalVariant + "; four-variant pixel comparison remains separate.";
            Finish(null);
        }

        private static void ValidateReferenceCommands(BattlePresentationFrame frame)
        {
            BattleCentralEntityDiagnostic ad = BattleCentralRenderSystem.CaptureEntityDiagnostic(
                world, aHandle, BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic bd = BattleCentralRenderSystem.CaptureEntityDiagnostic(
                world, bHandle, BattleRenderCommandType.Entity);
            Require(ad.HasCommand && ad.HasResolvedResource && ad.Submitted &&
                    bd.HasCommand && bd.HasResolvedResource && bd.Submitted,
                "Reference variant formal body commands were not resolved and submitted.");
            BattleRenderCommand ac = default;
            BattleRenderCommand bc = default;
            bool foundA = false;
            bool foundB = false;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Entity)
                    continue;
                if (command.RuntimeSlot == aHandle.Slot)
                {
                    ac = command;
                    foundA = true;
                    report.aCommandIndex = index;
                    report.aCommandPosition = command.Position.ToString();
                }
                if (command.RuntimeSlot == bHandle.Slot)
                {
                    bc = command;
                    foundB = true;
                    report.bCommandIndex = index;
                    report.bCommandPosition = command.Position.ToString();
                }
            }
            Require(foundA && foundB,
                "Both formal body commands must exist in the reference captured frame.");
            if (naturalVariant == "a" || naturalVariant == "b")
            {
                Require(!ProjectCommandBounds(camera, ac).Overlaps(
                        ProjectCommandBounds(camera, bc)),
                    "Fixture and distant-reference body bounds overlap on screen.");
            }
        }

        private static void CaptureBaseline()
        {
            baselineObjects = world.ObjectCount;
            baselineSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            baselineRenderers = objectPool.ActiveObjectCountForAcceptance;
            baselineLogicPool = LF2ReferencePool.Instance.ActiveCount;
            rngState = world.Rng.State;
            rngCalls = world.Rng.CallCount;
            nativeRng = world.NativeRandom.CaptureScalarState();
            report.baselineRngState = rngState.ToString();
            report.baselineRngCalls = rngCalls.ToString();
            report.baselineNativeCrtState = nativeRng.CrtState.ToString();
            Sounds.Clear();
            Sounds.AddRange(world.PendingSounds);
            baselineCaptured = true;
            report.baselineObjects = baselineObjects;
            report.baselineSlots = baselineSlots;
            report.baselineRenderers = baselineRenderers;
            report.baselineLogicPool = baselineLogicPool;
        }

        private static void SpawnFixtures()
        {
            producer = new ProbeProducer();
            world.Register(producer);
            Require(producer.Runtime.SlotIndex >= 0, "Probe OPoint producer has no slot.");
            a = Spawn(120);
            b = Spawn(121);
            Require(a.Frame.N == 0 && a.GetRenderPicIndex() == 0 &&
                    b.Frame.N == 0 && b.GetRenderPicIndex() == 0,
                "Formal OID120/121 are not on frame0/pic0 at birth.");
            aHandle = Handle(a);
            bHandle = Handle(b);
            Require(aHandle.Slot != bHandle.Slot && aHandle.IsValid && bHandle.IsValid,
                "Formal sprites lack distinct valid handles.");
            a.SetPos(HiddenX, 0, FixtureZ);
            b.SetPos(HiddenX, 0, FixtureZ);
            a.Runtime.SyncIntegerPosition();
            b.Runtime.SyncIntegerPosition();
            a.Runtime.SetVelocity(0d, 0d, 0d);
            b.Runtime.SetVelocity(0d, 0d, 0d);
            a.RefreshRuntimeSnapshot();
            b.RefreshRuntimeSnapshot();
            producer.ClearOpoint();
            report.aSlot = aHandle.Slot;
            report.bSlot = bHandle.Slot;
            report.aGeneration = aHandle.Generation;
            report.bGeneration = bHandle.Generation;
        }

        private static LF2Entity Spawn(int oid)
        {
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(Before);
            producer.SetSpawnOid(oid);
            try
            {
                factory.ProcessOpointSpawn(producer);
            }
            finally
            {
                world.GetActiveRuntimeEntitySnapshotForDiagnostics(After);
                for (int index = 0; index < After.Count; index++)
                {
                    LF2Entity candidate = After[index];
                    if (candidate != null && !Before.Contains(candidate) &&
                        !Owned.Contains(candidate))
                        Owned.Add(candidate);
                }
            }
            LF2Entity found = null;
            for (int index = 0; index < Owned.Count; index++)
            {
                LF2Entity candidate = Owned[index];
                if (candidate == null || candidate.ObjectId != oid || Before.Contains(candidate))
                    continue;
                Require(found == null, "Multiple new formal OID" + oid + " objects appeared.");
                found = candidate;
            }
            Require(found != null, "Production OPoint factory did not spawn OID" + oid + ".");
            return found;
        }

        private static RuntimeEntityHandle Handle(LF2Entity entity)
        {
            Require(world.TryGetCurrentRuntimeHandleForDiagnostics(
                entity.Runtime.SlotIndex, entity, out RuntimeEntityHandle handle),
                "Spawned formal entity has no runtime handle.");
            return handle;
        }

        private static void QueueCapture(int index)
        {
            captureIndex = index;
            a.SetPos(index == 1 || index == 3 ? FixtureX : HiddenX, 0, FixtureZ);
            b.SetPos(index == 2 || index == 3 ? FixtureX : HiddenX, 0, FixtureZ);
            a.Runtime.SyncIntegerPosition();
            b.Runtime.SyncIntegerPosition();
            a.RefreshRuntimeSnapshot();
            b.RefreshRuntimeSnapshot();
            expectedGeneration = world.CurrentPixelFramePlan.Generation;
            world.RenderDispatchAll(realTick + 100 + index);
            report.waitingFor = "natural LateUpdate central plan for capture " + index;
            deadline = EditorApplication.timeSinceStartup + 12.0;
        }

        private static void ObserveCapture()
        {
            Require(driver.IsPaused && driver.CurrentTickIndex == realTick,
                "Battle logic advanced during controlled presentation capture.");
            Require(!driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Dedicated worker started during controlled capture.");
            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            int expectedTick = realTick + 100 + captureIndex;
            if (!plan.IsValid || plan.Generation == expectedGeneration ||
                plan.SimulationTick != expectedTick || plan.Owner != BattlePixelFrameOwner.Central ||
                plan.IsStale || plan.Submission == null || plan.CapturedFrame == null ||
                !plan.CapturedFrame.CommandsMaterialized)
                return;
            Require(plan.CapturedFrame.TickIndex == expectedTick,
                "Central plan and captured frame tick differ.");
            if (captureIndex == 3)
                ValidateCompositeCommands(plan.CapturedFrame);
            captures[captureIndex] = CapturePixels(camera);
            if (captureIndex < 3)
            {
                QueueCapture(captureIndex + 1);
                return;
            }
            ComparePixels();
            report.status = "PASS";
            report.message = "Formal OID120/121 controlled central GPU overlap follows equal-Z physical-slot painter order.";
            Finish(null);
        }

        private static void ValidateCompositeCommands(BattlePresentationFrame frame)
        {
            BattleCentralEntityDiagnostic ad = BattleCentralRenderSystem.CaptureEntityDiagnostic(
                world, aHandle, BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic bd = BattleCentralRenderSystem.CaptureEntityDiagnostic(
                world, bHandle, BattleRenderCommandType.Entity);
            Require(ad.HasCommand && ad.HasResolvedResource && ad.Submitted &&
                    bd.HasCommand && bd.HasResolvedResource && bd.Submitted,
                "Formal body commands were not resolved and actually submitted.");
            int ai = -1;
            int bi = -1;
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Entity)
                    continue;
                if (command.RuntimeSlot == aHandle.Slot)
                    ai = index;
                if (command.RuntimeSlot == bHandle.Slot)
                    bi = index;
            }
            Require(ai >= 0 && bi >= 0 && ai != bi,
                "Both formal sprite commands are not in the frozen command frame.");
            Require((aHandle.Slot > bHandle.Slot) == (ai < bi),
                "Equal-Z formal body command order does not paint greater slot first.");
            report.aCommandIndex = ai;
            report.bCommandIndex = bi;
            report.aCommandPosition = frame.GetCommand(ai).Position.ToString();
            report.bCommandPosition = frame.GetCommand(bi).Position.ToString();
            report.aCommandPivot = frame.GetCommand(ai).Pivot.ToString();
            report.bCommandPivot = frame.GetCommand(bi).Pivot.ToString();
            report.aCommandSize = frame.GetCommand(ai).Size.ToString();
            report.bCommandSize = frame.GetCommand(bi).Size.ToString();
            RectInt aBounds = ProjectCommandBounds(camera, frame.GetCommand(ai));
            RectInt bBounds = ProjectCommandBounds(camera, frame.GetCommand(bi));
            overlapBounds = RectFromLimits(
                Mathf.Max(aBounds.xMin, bBounds.xMin),
                Mathf.Max(aBounds.yMin, bBounds.yMin),
                Mathf.Min(aBounds.xMax, bBounds.xMax),
                Mathf.Min(aBounds.yMax, bBounds.yMax));
            Require(overlapBounds.width > 0 && overlapBounds.height > 0,
                "Formal sprite command screen bounds do not intersect.");
            report.overlapBounds = overlapBounds.ToString();
            report.aZ = a.Runtime.ZInt;
            report.bZ = b.Runtime.ZInt;
            Require(report.aZ == report.bZ,
                "Formal sprite fixture depths differ.");
            report.laterPainter = aHandle.Slot < bHandle.Slot ? "OID120" : "OID121";
            report.compositePlanGeneration = world.CurrentPixelFramePlan.Generation;
        }

        private static RectInt ProjectCommandBounds(Camera source, BattleRenderCommand command)
        {
            float width = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                NTSDRenderSpace.BattleVisualScale;
            float height = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * width;
            float bottom = command.Position.y - command.Pivot.y * height;
            Vector3 lower = source.WorldToViewportPoint(
                new Vector3(left, bottom, command.Position.z));
            Vector3 upper = source.WorldToViewportPoint(
                new Vector3(left + width, bottom + height, command.Position.z));
            return RectFromLimits(
                Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.x, upper.x) * CaptureWidth),
                    0, CaptureWidth),
                Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.y, upper.y) * captureHeight),
                    0, captureHeight),
                Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.x, upper.x) * CaptureWidth),
                    0, CaptureWidth),
                Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.y, upper.y) * captureHeight),
                    0, captureHeight));
        }

        private static RectInt RectFromLimits(int xMin, int yMin, int xMax, int yMax)
        {
            return new RectInt(xMin, yMin,
                Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));
        }

        private static Color32[] CapturePixels(Camera source)
        {
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (source.aspect > 0f ? source.aspect : 16f / 9f)));
            if (captureHeight == 0)
                captureHeight = height;
            Require(captureHeight == height, "Camera aspect changed between captures.");
            var target = new RenderTexture(CaptureWidth, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var saved = new CameraState(source);
            RenderTexture previousActive = RenderTexture.active;
            Texture2D readback = null;
            try
            {
                target.Create();
                source.cullingMask = 0;
                source.clearFlags = CameraClearFlags.SolidColor;
                source.backgroundColor = Color.white;
                source.allowHDR = false;
                source.allowMSAA = false;
                source.targetTexture = target;
                source.Render();
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, height, TextureFormat.RGBA32,
                    false, true);
                readback.ReadPixels(new Rect(0, 0, CaptureWidth, height), 0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                int nonClear = 0;
                for (int index = 0; index < pixels.Length; index++)
                    if (!Near(pixels[index], Clear, 2))
                        nonClear++;
                report.captureNonClearCounts[captureIndex] = nonClear;
                string fileName = naturalTickMode
                    ? "independent-" + naturalVariant + ".png"
                    : "capture-" + captureIndex + ".png";
                string imagePath = ProjectPath("artifacts/diagnostics/" +
                    ActiveArtifactFolder + "/" + fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                File.WriteAllBytes(imagePath, readback.EncodeToPNG());
                return pixels;
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(source);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void ComparePixels()
        {
            Color32[] baseline = captures[0];
            Color32[] onlyA = captures[1];
            Color32[] onlyB = captures[2];
            Color32[] both = captures[3];
            Require(baseline != null && onlyA != null && onlyB != null && both != null &&
                    baseline.Length == onlyA.Length && onlyA.Length == onlyB.Length &&
                    onlyB.Length == both.Length,
                "Controlled GPU captures are missing or inconsistent.");
            bool aLater = aHandle.Slot < bHandle.Slot;
            int discriminating = 0;
            int matching = 0;
            int opposing = 0;
            int mixed = 0;
            for (int index = 0; index < both.Length; index++)
            {
                int x = index % CaptureWidth;
                int y = index / CaptureWidth;
                if (!overlapBounds.Contains(new Vector2Int(x, y)))
                    continue;
                if (!Near(baseline[index], Clear, 2) ||
                    Near(onlyA[index], Clear, 2) || Near(onlyB[index], Clear, 2) ||
                    Near(onlyA[index], onlyB[index], 12))
                    continue;
                discriminating++;
                Color32 expected = aLater ? onlyA[index] : onlyB[index];
                Color32 earlier = aLater ? onlyB[index] : onlyA[index];
                if (Near(both[index], expected, 2))
                {
                    matching++;
                    if (matching == 1)
                    {
                        report.sampleX = x;
                        report.sampleY = y;
                        report.aOnlyRgb = Rgb(onlyA[index]);
                        report.bOnlyRgb = Rgb(onlyB[index]);
                        report.compositeRgb = Rgb(both[index]);
                        report.baselineRgb = Rgb(baseline[index]);
                    }
                }
                else if (Near(both[index], earlier, 2))
                    opposing++;
                else
                    mixed++;
            }
            report.discriminatingOverlapPixels = discriminating;
            report.matchingOverlapPixels = matching;
            report.opposingOverlapPixels = opposing;
            report.mixedOverlapPixels = mixed;
            Require(matching > 0 && opposing == 0,
                "Weapon-body overlap pixel comparison failed: candidates=" +
                discriminating + ", later=" + matching + ", earlier=" + opposing +
                ", mixed=" + mixed + ".");
        }

        private static bool Near(Color32 left, Color32 right, int tolerance)
        {
            return Math.Abs(left.r - right.r) <= tolerance &&
                   Math.Abs(left.g - right.g) <= tolerance &&
                   Math.Abs(left.b - right.b) <= tolerance;
        }

        private static string Rgb(Color32 color)
        {
            return color.r + "," + color.g + "," + color.b;
        }

        private static void Finish(string error)
        {
            if (report == null)
                return;
            if (!string.IsNullOrEmpty(error))
            {
                report.status = "FAIL";
                report.message = error;
            }
            try
            {
                Cleanup();
                if (sceneHash != null)
                {
                    report.sceneHashAtFinish = HashFile(ProjectPath(report.scenePath));
                    report.sceneDiskUnchanged = report.sceneHashAtFinish == sceneHash;
                    Require(report.sceneDiskUnchanged,
                        "Original Battle Scene disk hash changed during probe.");
                }
                Require(!baselineCaptured || report.cleanupPassed,
                    "Probe cleanup did not restore all recorded World baselines.");
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message += " Cleanup: " + exception;
            }
            finally
            {
                try
                {
                    if (requestMode && File.Exists(ProjectPath(ActiveRequestPath)))
                        File.Delete(ProjectPath(ActiveRequestPath));
                    string path = ProjectPath(ActiveResultPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, JsonUtility.ToJson(report, true));
                    Debug.Log("[NTSD28Q09SameZFormalSpritePixel] " + report.status +
                        ": " + report.message);
                }
                finally
                {
                    bool exit = requestMode;
                    running = false;
                    requestMode = false;
                    naturalTickMode = false;
                    naturalVariant = null;
                    pauseCaptured = false;
                    baselineCaptured = false;
                    driver = null;
                    world = null;
                    factory = null;
                    objectPool = null;
                    producer = null;
                    a = null;
                    b = null;
                    camera = null;
                    captures = null;
                    report = null;
                    captureHeight = 0;
                    if (exit && EditorApplication.isPlaying)
                        EditorApplication.delayCall += () => EditorApplication.ExitPlaymode();
                }
            }
        }

        private static void Cleanup()
        {
            string cleanupError = null;
            try
            {
                if (world != null && baselineCaptured)
                {
                    for (int index = Owned.Count - 1; index >= 0; index--)
                    {
                        try { FreeOwned(Owned[index]); }
                        catch (Exception exception)
                        {
                            cleanupError += " owned[" + index + "]:" + exception.Message;
                        }
                    }
                    Owned.Clear();
                    try
                    {
                        if (producer?.Match == world && producer.Runtime?.SlotIndex >= 0)
                            world.Unregister(producer);
                        world.FlushPendingDestroyForDiagnostics();
                    }
                    catch (Exception exception) { cleanupError += " producer:" + exception.Message; }
                    try
                    {
                        world.PendingSounds.Clear();
                        world.PendingSounds.AddRange(Sounds);
                        world.Rng.RestoreState(rngState, rngCalls);
                        Require(world.NativeRandom.TryRestoreScalarState(nativeRng),
                            "Native CRT state could not be restored.");
                        world.RenderDispatchAll(driver.CurrentTickIndex);
                    }
                    catch (Exception exception) { cleanupError += " state:" + exception.Message; }
                    report.finalObjects = world.ObjectCount;
                    report.finalSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
                    report.finalRenderers = objectPool.ActiveObjectCountForAcceptance;
                    report.finalLogicPool = LF2ReferencePool.Instance.ActiveCount;
                    report.cleanupPassed = string.IsNullOrEmpty(cleanupError) &&
                        report.finalObjects == baselineObjects &&
                        report.finalSlots == baselineSlots &&
                        report.finalRenderers == baselineRenderers &&
                        report.finalLogicPool == baselineLogicPool &&
                        world.Rng.State == rngState && world.Rng.CallCount == rngCalls &&
                        world.NativeRandom.CaptureScalarState().CrtState == nativeRng.CrtState &&
                        SoundsEqual(world.PendingSounds, Sounds);
                }
            }
            finally
            {
                if (pauseCaptured && driver != null && EditorApplication.isPlaying)
                {
                    driver.SetPaused(previousPaused);
                    report.pauseRestored = driver.IsPaused == previousPaused;
                    report.cleanupPassed &= report.pauseRestored;
                }
            }
            if (!string.IsNullOrEmpty(cleanupError))
                throw new InvalidOperationException("Cleanup errors:" + cleanupError);
        }

        private static void FreeOwned(LF2Entity entity)
        {
            if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                entity.FreeEntityLikeExe();
        }

        private static bool SoundsEqual(IList<PendingSoundEvent> left,
            IList<PendingSoundEvent> right)
        {
            if (left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
                if (left[index].Cue != right[index].Cue ||
                    left[index].WorldX != right[index].WorldX ||
                    left[index].Tick != right[index].Tick)
                    return false;
            return true;
        }

        private static string HashFile(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static string ProjectPath(string relative)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private sealed class ProbeProducer : LF2OtherObject
        {
            private readonly LF2FrameData frame;

            internal ProbeProducer()
            {
                Name = "Q09_SameZ_FormalSprite_Producer";
                ObjectId = 9700;
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                frame = new LF2FrameData
                {
                    frameId = 0, state = 0, wait = 10000, next = 0,
                    pic = 999, centerx = 0, centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)LF2ObjectType.Other,
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.SetPosition(FixtureX, 0, FixtureZ);
                Runtime.SyncIntegerPosition();
                PS.dir = "right";
            }

            internal void SetSpawnOid(int oid)
            {
                frame.opoint = new ObjectPoint { kind = 1, oid = oid, action = 0, facing = 0 };
                Frame.D = frame;
                AttackingCounter = 0;
            }

            internal void ClearOpoint()
            {
                frame.opoint = null;
                frame.opoints?.Clear();
            }

            public override void SimFrameTick(int tickIndex) { }
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
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public string evidenceScope;
            public string waitingFor;
            public string scenePath;
            public string sceneHashBefore;
            public string sceneHashAtFinish;
            public bool sceneDiskUnchanged;
            public int realTick;
            public int naturalTick;
            public string variant;
            public bool workerPath;
            public string baselineRngState;
            public string baselineRngCalls;
            public string baselineNativeCrtState;
            public string postRngState;
            public string postRngCalls;
            public string postNativeCrtState;
            public int aFrame;
            public int bFrame;
            public int aPic;
            public int bPic;
            public int aRuntimeX;
            public int bRuntimeX;
            public int aSlot;
            public int bSlot;
            public long aGeneration;
            public long bGeneration;
            public int aZ;
            public int bZ;
            public int aCommandIndex;
            public int bCommandIndex;
            public int compositePlanGeneration;
            public string laterPainter;
            public int discriminatingOverlapPixels;
            public int matchingOverlapPixels;
            public int opposingOverlapPixels;
            public int mixedOverlapPixels;
            public int sampleX;
            public int sampleY;
            public string baselineRgb;
            public string aOnlyRgb;
            public string bOnlyRgb;
            public string compositeRgb;
            public string aCommandPosition;
            public string bCommandPosition;
            public string aCommandPivot;
            public string bCommandPivot;
            public string aCommandSize;
            public string bCommandSize;
            public string overlapBounds;
            public int[] captureNonClearCounts = new int[4];
            public int baselineObjects;
            public int baselineSlots;
            public int baselineRenderers;
            public int baselineLogicPool;
            public int finalObjects;
            public int finalSlots;
            public int finalRenderers;
            public int finalLogicPool;
            public bool pauseRestored;
            public bool cleanupPassed;
        }

        [Serializable]
        private sealed class SceneDirtyDiagnostic
        {
            public bool sceneValid;
            public bool sceneLoaded;
            public bool sceneDirty;
            public string[] entries;
        }
    }
}
#endif
