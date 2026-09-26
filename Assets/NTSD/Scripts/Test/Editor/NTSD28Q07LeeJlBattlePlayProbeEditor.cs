#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07LeeJlBattlePlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_LeeJlBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-LEE-JL-BATTLE-PLAY-001";
        private const string PixelResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001";
        private const string ComposedResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-LEE-CHILD-COMPOSED-WORLD-CAMERA-001";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const int CaptureWidth = 960;
        private static DateTime startedUtc;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static bool running;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public bool captureCamera;
            public bool captureComposedCamera;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string error;
            public string scenePath;
            public string contentRoot;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public int startTick;
            public int endTick;
            public int leeSlot = -1;
            public int narutoSlot = -1;
            public int leeRosterSlot = -1;
            public int narutoRosterSlot = -1;
            public int firstOid204Tick = -1;
            public int firstOid204Count;
            public int firstOid204SourceInitializedCount;
            public int firstOid204RendererCount;
            public int firstOid204CentralCommands;
            public bool centralPlanValid;
            public bool logicOnlyMaterialization;
            public bool stopped;
            public int borrowersAfter = -1;
            public bool captureCamera;
            public bool captureComposedCamera;
            public int baselineTick = -1;
            public string baselinePngPath;
            public string postPngPath;
            public bool cameraRestored = true;
            public string catalogSourcePath;
            public Rect catalogPixelRect;
            public int publishedVisualDataId;
            public int publishedPic;
            public int captureWidth;
            public int captureHeight;
            public int roiX;
            public int roiY;
            public int roiWidth;
            public int roiHeight;
            public int roiNonblackPixels;
            public int roiChangedPixels;
            public int roiChangedNonblackPixels;
            public List<string> leeFrames = new List<string>(45);
            public List<string> births = new List<string>(24);
        }

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
            string requestFile = ProjectPath(RequestPath);
            if (!File.Exists(requestFile))
            {
                Reset();
                return;
            }
            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || !request.requested)
                return;
            if (string.IsNullOrEmpty(request.runId) ||
                !request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'))
            {
                Finish(request, new Report { status = "FAIL", error = "Invalid runId." });
                return;
            }
            if (request.captureCamera && request.captureComposedCamera)
            {
                Finish(request, new Report { status = "FAIL",
                    error = "Select one camera capture mode." });
                return;
            }
            string resultPath = ProjectPath(
                GetResultRoot(request) + "/" + request.runId + ".json");
            if (File.Exists(resultPath))
            {
                Debug.LogError("[Q07 Lee J,L Play] Refusing to overwrite " + resultPath);
                File.WriteAllText(requestFile, JsonUtility.ToJson(new Request
                {
                    requested = false,
                    runId = request.runId,
                    captureCamera = request.captureCamera,
                    captureComposedCamera = request.captureComposedCamera,
                }));
                return;
            }
            if (startedUtc == default)
                startedUtc = DateTime.UtcNow;
            if (DateTime.UtcNow - startedUtc > TimeSpan.FromMinutes(4))
            {
                Finish(request, new Report { status = "FAIL", error = "Startup timeout." });
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                Scene scene = SceneManager.GetActiveScene();
                if (scene.name != "NTSD_Battle" || scene.isDirty)
                {
                    Finish(request, new Report { status = "FAIL",
                        error = "Requires clean saved NTSD_Battle scene." });
                    return;
                }
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5)
                return;
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Finish(request, new Report { status = "FAIL", error =
                    driver.DedicatedSimulationWorkerFailureForDiagnostics.ToString() });
                EditorApplication.ExitPlaymode();
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                stableUpdates = 0;
                return;
            }
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 4)
                return;
            running = true;
            File.WriteAllText(requestFile, JsonUtility.ToJson(new Request
            {
                requested = false,
                runId = request.runId,
                captureCamera = request.captureCamera,
                captureComposedCamera = request.captureComposedCamera,
            }));
            Run(request, driver, world);
        }

        private static void Run(Request request, SimulationTickDriver driver,
            SimulationWorld world)
        {
            var report = new Report
            {
                runId = request.runId,
                status = "RUNNING",
                captureCamera = request.captureCamera,
                captureComposedCamera = request.captureComposedCamera,
                scenePath = SceneManager.GetActiveScene().path,
                sceneHashBefore = HashFile(ProjectPath("Assets/NTSD/Scene/NTSD_Battle.unity")),
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
                startTick = driver.CurrentTickIndex,
                logicOnlyMaterialization = world.UsesLogicOnlyEntityMaterialization,
            };
            try
            {
                Require(report.scenePath == "Assets/NTSD/Scene/NTSD_Battle.unity",
                    "Wrong active scene.");
                Require(report.contentRoot == FormalRoot,
                    "Formal content root is not selected.");
                var leeConfig = world.RuntimeCharacterConfigs.Resolve(7);
                var narutoConfig = world.RuntimeCharacterConfigs.Resolve(2);
                Require(leeConfig?.characterData != null &&
                    narutoConfig?.characterData != null,
                    "Lee or Naruto formal definition is unavailable.");
                report.leeSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                report.narutoSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    report.leeSlot + 1, 1000);
                Require(report.leeSlot >= 50 && report.narutoSlot > report.leeSlot,
                    "No two free runtime slots.");
                BattleSlotRuntimeState[] slots = world.Runtime.Roster.Slots;
                report.leeRosterSlot = Array.FindIndex(slots,
                    slot => slot == null || !slot.Active);
                report.narutoRosterSlot = Array.FindIndex(slots,
                    report.leeRosterSlot + 1, slot => slot == null || !slot.Active);
                Require(report.leeRosterSlot >= 0 &&
                    report.narutoRosterSlot > report.leeRosterSlot,
                    "No two free human roster slots.");

                LF2Character lee = CreateCharacter(world, leeConfig, 7,
                    report.leeSlot, 3, 500, 650);
                LF2Character naruto = CreateCharacter(world, narutoConfig, 2,
                    report.narutoSlot, 4, 1200, 650);
                slots[report.leeRosterSlot] = new BattleSlotRuntimeState
                {
                    Active = true, IsHuman = true, CharacterId = 7, Team = 3,
                    RuntimeSlotIndex = lee.Runtime.SlotIndex,
                    StableId = lee.Runtime.StableId,
                };
                slots[report.narutoRosterSlot] = new BattleSlotRuntimeState
                {
                    Active = true, IsHuman = true, CharacterId = 2, Team = 4,
                    RuntimeSlotIndex = naruto.Runtime.SlotIndex,
                    StableId = naruto.Runtime.StableId,
                };
                var initialIds = new HashSet<int>();
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                    if (entity != null)
                        initialIds.Add(entity.Runtime.StableId);
                }
                var observedBirthIds = new HashSet<int>();
                Color32[] baselinePixels = null;
                int firstJTick = driver.CurrentTickIndex + 2;
                if ((firstJTick & 1) != 0)
                    firstJTick++;
                for (int index = 0; index < 45; index++)
                {
                    int nextTick = driver.CurrentTickIndex + 1;
                    SimulationInputButtons leeButtons = nextTick == firstJTick
                        ? SimulationInputButtons.Jump
                        : nextTick == firstJTick + 1 ||
                          nextTick == firstJTick + 2
                            ? SimulationInputButtons.Attack
                            : SimulationInputButtons.None;
                    var frame = new FrameInputSet(nextTick, new[]
                    {
                        new SimulationPlayerInput(report.leeRosterSlot, leeButtons),
                        new SimulationPlayerInput(report.narutoRosterSlot,
                            SimulationInputButtons.None),
                    });
                    Require(driver.StepOneTick(frame, ignorePaused: true,
                        buildPresentation: true),
                        "Full Driver rejected tick " + nextTick);
                    report.endTick = driver.CurrentTickIndex;
                    report.leeFrames.Add(index + ":" + nextTick + ":" +
                        world.InputPhase + ":" + leeButtons + ":" +
                        lee.Frame.N + ":" + lee.Runtime.PP);
                    if ((request.captureCamera || request.captureComposedCamera) &&
                        baselinePixels == null &&
                        lee.Frame.N == 146)
                    {
                        BattlePixelFramePlan baselinePlan =
                            BattleCentralRenderSystem.PrepareFrame(world);
                        Require(baselinePlan.IsValid && !baselinePlan.IsStale &&
                            baselinePlan.SimulationTick == nextTick &&
                            baselinePlan.CapturedFrame?.CommandsMaterialized == true,
                            "Baseline central plan is unavailable.");
                        report.baselineTick = nextTick;
                        baselinePixels = CaptureCameraFrame(request.runId,
                            "before", report);
                    }
                    int oid204Count = 0;
                    var childIds = new HashSet<int>();
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    {
                        LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                        if (entity == null || initialIds.Contains(entity.Runtime.StableId))
                            continue;
                        if (observedBirthIds.Add(entity.Runtime.StableId) &&
                            report.births.Count < 24)
                        {
                            report.births.Add(nextTick + ":" + slot + ":" +
                                entity.ObjectId + ":" + entity.OwnerEntityIndex + ":" +
                                entity.Runtime.X + ":" + entity.Runtime.Y + ":" +
                                entity.Runtime.Z + ":" +
                                entity.Runtime.SourceRulePositionInitialized + ":" +
                                entity.Runtime.SourceRuleXInt + ":" +
                                entity.Runtime.SourceRuleZInt);
                        }
                        if (entity.ObjectId != 204 ||
                            entity.OwnerEntityIndex != report.leeSlot)
                            continue;
                        oid204Count++;
                        childIds.Add(entity.Runtime.StableId);
                        if (report.firstOid204Tick < 0)
                        {
                            if (entity.Runtime.SourceRulePositionInitialized)
                                report.firstOid204SourceInitializedCount++;
                            if (entity.Renderer != null)
                                report.firstOid204RendererCount++;
                        }
                    }
                    if (oid204Count <= 0 || report.firstOid204Tick >= 0)
                        continue;
                    report.firstOid204Tick = nextTick;
                    report.firstOid204Count = oid204Count;
                    BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
                    report.centralPlanValid = plan.IsValid && !plan.IsStale &&
                        plan.SimulationTick == driver.CurrentTickIndex &&
                        plan.CapturedFrame?.CommandsMaterialized == true;
                    if (report.centralPlanValid)
                    {
                        BattlePresentationFrame presentation = plan.CapturedFrame;
                        BattleRenderCommand firstChildCommand = default;
                        bool firstChildCommandFound = false;
                        for (int commandIndex = 0;
                            commandIndex < presentation.CommandCount; commandIndex++)
                        {
                            BattleRenderCommand command =
                                presentation.GetCommand(commandIndex);
                            if (command.Type == BattleRenderCommandType.Entity &&
                                childIds.Contains(command.StableId))
                            {
                                report.firstOid204CentralCommands++;
                                if (!firstChildCommandFound)
                                {
                                    firstChildCommand = command;
                                    firstChildCommandFound = true;
                                }
                            }
                        }
                        if (request.captureCamera || request.captureComposedCamera)
                        {
                            Require(firstChildCommandFound && baselinePixels != null,
                                "Child command or pre-birth camera frame is unavailable.");
                            CaptureChildPixels(request.runId, report,
                                firstChildCommand, baselinePixels);
                        }
                    }
                }
                Require(report.firstOid204Tick >= 0 &&
                    report.firstOid204Count == 5,
                    "Lee J,L did not naturally create five owned OID204 children.");
                Require(report.firstOid204SourceInitializedCount == 5,
                    "One or more children lack initialized source position.");
                Require(report.logicOnlyMaterialization
                    ? report.firstOid204RendererCount == 0 &&
                      report.centralPlanValid && report.firstOid204CentralCommands == 5
                    : report.firstOid204RendererCount == 5,
                    "Child presentation carrier is incomplete.");
                report.status = "PASS";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }
            finally
            {
                try
                {
                    BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                    bool mapCleared = true;
                    BattleBootstrap[] bootstraps =
                        Resources.FindObjectsOfTypeAll<BattleBootstrap>();
                    foreach (BattleBootstrap bootstrap in bootstraps)
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap) ||
                            !bootstrap.gameObject.scene.IsValid())
                            continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    if (shutdown.RuntimeStagesCompleted)
                        shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(
                            mapCleared);
                    report.stopped = shutdown.IsComplete;
                    LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                    report.borrowersAfter = pool == null ? 0 :
                        pool.ActiveObjectCountForAcceptance +
                        pool.ActiveSpriteCountForAcceptance;
                    if (!report.stopped || report.borrowersAfter != 0)
                    {
                        report.status = "FAIL";
                        report.error += "\nOrdered shutdown or pool borrowers failed.";
                    }
                }
                catch (Exception cleanupError)
                {
                    report.status = "FAIL";
                    report.error += "\nCleanup: " + cleanupError;
                }
                report.sceneHashAfter = HashFile(ProjectPath(
                    "Assets/NTSD/Scene/NTSD_Battle.unity"));
                if (report.sceneHashAfter != report.sceneHashBefore)
                {
                    report.status = "FAIL";
                    report.error += "\nSaved Battle Scene hash changed.";
                }
                Finish(request, report);
                EditorApplication.delayCall += () =>
                {
                    if (EditorApplication.isPlaying)
                        EditorApplication.ExitPlaymode();
                };
            }
        }

        private static LF2Character CreateCharacter(SimulationWorld world,
            LF2CharacterDataWrapper config, int oid, int runtimeSlot,
            int team, int x, int z)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = oid;
            character.Name = "Q07LeeJlPlay" + oid;
            character.FrameCache.Load(config);
            character.SetRequiredRuntimeSlot(runtimeSlot);
            world.Register(character);
            character.ImmediateFrame(0);
            character.Initialize(500, 500);
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.AiControlled = false;
            character.Team = team;
            character.RelationTeam = team;
            character.OwnerEntityIndex = runtimeSlot;
            character.Runtime.SetPosition(x, 0, z);
            AppManager.SyncParticipantBirthPosition(character, x, z);
            return character;
        }

        private static void CaptureChildPixels(string runId, Report report,
            BattleRenderCommand command, Color32[] baselinePixels)
        {
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleSpriteEntry entry = null;
            Require(manager != null && manager.TryGetSpriteEntry(
                command.VisualDataId, command.EffectivePic,
                out entry) && entry != null &&
                entry.CentralBinding.IsValid,
                "Published OID204 command has no formal sprite binding.");
            report.catalogSourcePath = entry.SourceSheetPath;
            report.catalogPixelRect = entry.PixelRect;
            report.publishedVisualDataId = command.VisualDataId;
            report.publishedPic = command.EffectivePic;

            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.enabled && camera.gameObject.activeInHierarchy,
                "Saved Battle world camera is unavailable.");
            float widthWorld = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                               NTSDRenderSpace.BattleVisualScale;
            float heightWorld = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * widthWorld;
            float bottom = command.Position.y - command.Pivot.y * heightWorld;
            Vector3 lower = camera.WorldToViewportPoint(
                new Vector3(left, bottom, command.Position.z));
            Vector3 upper = camera.WorldToViewportPoint(new Vector3(
                left + widthWorld, bottom + heightWorld, command.Position.z));
            int x0 = Mathf.Clamp(Mathf.FloorToInt(
                Mathf.Min(lower.x, upper.x) * report.captureWidth),
                0, report.captureWidth);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(
                Mathf.Max(lower.x, upper.x) * report.captureWidth),
                0, report.captureWidth);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(
                Mathf.Min(lower.y, upper.y) * report.captureHeight),
                0, report.captureHeight);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(
                Mathf.Max(lower.y, upper.y) * report.captureHeight),
                0, report.captureHeight);
            report.roiX = x0;
            report.roiY = y0;
            report.roiWidth = x1 - x0;
            report.roiHeight = y1 - y0;
            Require(report.roiWidth > 0 && report.roiHeight > 0,
                "OID204 command projects outside the saved Battle camera.");

            Color32[] postPixels = CaptureCameraFrame(runId, "after", report);
            Require(baselinePixels.Length == postPixels.Length,
                "Camera capture dimensions changed between birth frames.");
            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++)
                {
                    int index = y * report.captureWidth + x;
                    Color32 before = baselinePixels[index];
                    Color32 after = postPixels[index];
                    bool nonblack = after.r != 0 || after.g != 0 || after.b != 0;
                    bool changed = before.r != after.r || before.g != after.g ||
                                   before.b != after.b;
                    if (nonblack)
                        report.roiNonblackPixels++;
                    if (changed)
                        report.roiChangedPixels++;
                    if (changed && nonblack)
                        report.roiChangedNonblackPixels++;
                }
            }
            Require(report.cameraRestored && report.roiNonblackPixels > 0 &&
                report.roiChangedNonblackPixels > 0,
                "OID204 ROI has no changed visible pixel or camera was not restored.");
        }

        private static Color32[] CaptureCameraFrame(string runId,
            string name, Report report)
        {
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null, "Battle world camera is unavailable.");
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            Require(report.captureHeight == 0 || report.captureHeight == height,
                "Camera aspect changed between before and after capture.");
            report.captureWidth = CaptureWidth;
            report.captureHeight = height;
            Require(BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                camera, CameraRenderType.Base, camera.cameraType, true,
                out BattleCentralSubmission.BattleCentralSubmissionLease lease),
                "Current central camera submission is unavailable.");
            using (lease)
            {
            }

            var target = new RenderTexture(CaptureWidth, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            RenderTexture previousActive = RenderTexture.active;
            var saved = new CameraState(camera);
            Texture2D readback = null;
            try
            {
                target.Create();
                if (!report.captureComposedCamera)
                {
                    camera.cullingMask = 0;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = Color.black;
                    camera.allowHDR = false;
                    camera.allowMSAA = false;
                }
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, height,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, height),
                    0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                string relative = (report.captureComposedCamera
                    ? ComposedResultRoot : PixelResultRoot) +
                    "/" + runId + "-" + name + ".png";
                string output = ProjectPath(relative);
                Require(!File.Exists(output), "Refusing to overwrite camera capture.");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, readback.EncodeToPNG());
                if (name == "before")
                    report.baselinePngPath = relative;
                else
                    report.postPngPath = relative;
                return pixels;
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(camera);
                report.cameraRestored &= saved.Matches(camera) &&
                                         RenderTexture.active == previousActive;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private readonly struct CameraState
        {
            private readonly int cullingMask;
            private readonly CameraClearFlags clearFlags;
            private readonly Color backgroundColor;
            private readonly bool allowHdr;
            private readonly bool allowMsaa;
            private readonly RenderTexture targetTexture;

            public CameraState(Camera camera)
            {
                cullingMask = camera.cullingMask;
                clearFlags = camera.clearFlags;
                backgroundColor = camera.backgroundColor;
                allowHdr = camera.allowHDR;
                allowMsaa = camera.allowMSAA;
                targetTexture = camera.targetTexture;
            }

            public void Restore(Camera camera)
            {
                camera.targetTexture = targetTexture;
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = backgroundColor;
                camera.allowHDR = allowHdr;
                camera.allowMSAA = allowMsaa;
            }

            public bool Matches(Camera camera)
            {
                return camera.targetTexture == targetTexture &&
                       camera.cullingMask == cullingMask &&
                       camera.clearFlags == clearFlags &&
                       camera.backgroundColor == backgroundColor &&
                       camera.allowHDR == allowHdr &&
                       camera.allowMSAA == allowMsaa;
            }
        }

        private static void Finish(Request request, Report report)
        {
            File.WriteAllText(ProjectPath(RequestPath),
                JsonUtility.ToJson(new Request
                {
                    requested = false,
                    runId = request?.runId,
                    captureCamera = request?.captureCamera ?? false,
                    captureComposedCamera = request?.captureComposedCamera ?? false,
                }));
            if (request != null && !string.IsNullOrEmpty(request.runId) &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'))
            {
                string path = ProjectPath(GetResultRoot(request) +
                    "/" + request.runId + ".json");
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, JsonUtility.ToJson(report, true));
                }
            }
            Reset();
        }

        private static void Reset()
        {
            startedUtc = default;
            stableTick = -1;
            stableUpdates = 0;
            running = false;
        }

        private static string GetResultRoot(Request request)
        {
            if (request.captureComposedCamera)
                return ComposedResultRoot;
            return request.captureCamera ? PixelResultRoot : ResultRoot;
        }

        private static string ProjectPath(string relative) => Path.Combine(
            Directory.GetParent(Application.dataPath).FullName, relative);

        private static string HashFile(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = System.Security.Cryptography.SHA256.Create())
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
