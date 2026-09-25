#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q07Oid434NaturalBindingProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q09_Oid434CameraPixelV3.request.json";
        private const string NaturalRoot =
            "Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-OID434-CAMERA-PIXEL-001";
        private const int CaptureWidth = 2048;
        private static readonly List<LF2Entity> Entities = new List<LF2Entity>(64);
        private static readonly HashSet<string> EarlierNaturalResults =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static bool naturalStarted;
        private static int initialTick = -1;
        private static int previousTick = -1;
        private static int sampledTicks;
        private static int firstGap = -1;
        private static bool followupStarted;
        private static bool followupInitialPaused;
        private static int followupInitialTick;
        private static int followupPreviousTick;
        private static Keyboard followupKeyboard;
        private static LF2Character followupActor;
        private static bool centralCapturePending;
        private static int centralCaptureStartedFrame;
        private static Report report;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public long startedUtcTicks;
            public bool followupAttack;
            public bool captureCentralCommand;
            public bool captureCameraPixels;
        }

        [Serializable]
        private sealed class ProjectedCommandRect
        {
            public int index;
            public string type;
            public int sortOrder;
            public int visualDataId;
            public int effectivePic;
            public string sourceSheetPath;
            public float sourceRectX;
            public float sourceRectY;
            public float sourceRectWidth;
            public float sourceRectHeight;
            public float uvX;
            public float uvY;
            public float uvWidth;
            public float uvHeight;
            public bool flipX;
            public bool flipY;
            public int x;
            public int y;
            public int width;
            public int height;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string runId;
            public string scenePath;
            public bool sceneDirty;
            public int initialTick;
            public int lastTick;
            public int sampledTicks;
            public int firstTickGap;
            public int firstOid434Tick = -1;
            public int firstOid434Action = -1;
            public int oid434EntityFrames;
            public int visibleTick = -1;
            public int stableId;
            public int slot;
            public int objectId;
            public int action;
            public int renderPic;
            public bool spriteResolved;
            public int visualDataId;
            public int effectivePic;
            public string sourceSheetPath;
            public float pixelWidth;
            public float pixelHeight;
            public bool centralBindingValid;
            public bool legacySpritePresent;
            public string naturalResultPath;
            public string naturalStatus;
            public int naturalSkillTick;
            public bool followupAttack;
            public bool captureCentralCommand;
            public bool captureCameraPixels;
            public int followupQueuedAfterTick = -1;
            public int followupFirstInputTick = -1;
            public int followupAction25Tick = -1;
            public int followupAction100Tick = -1;
            public int followupFirstAttackAction = -1;
            public int followupLastActorAction = -1;
            public int followupSampledTicks;
            public int followupFirstTickGap = -1;
            public bool planValid;
            public string planOwner;
            public string planRequestedMode;
            public int planSimulationTick = -1;
            public int planDisplayTick = -1;
            public int planGeneration;
            public bool submissionReady;
            public int submissionTick = -1;
            public int frozenFrameTick = -1;
            public bool commandsMaterialized;
            public int commandCount;
            public int matchingEntityCommandCount;
            public float matchingCommandWidth;
            public float matchingCommandHeight;
            public int matchingCommandSortOrder;
            public bool worldCameraEnabled;
            public bool cameraLeaseAccepted;
            public int cameraLeaseTick = -1;
            public int cameraLeaseGeneration;
            public int cameraDrawCountBefore;
            public int cameraDrawCountAfter;
            public int cameraLastSubmissionDrawCount;
            public int captureWidth;
            public int captureHeight;
            public int targetRegionArea;
            public int targetRegionNonClear;
            public int targetExclusiveArea;
            public int targetExclusiveNonClear;
            public int targetX;
            public int targetY;
            public int targetWidth;
            public int targetHeight;
            public float targetUvX;
            public float targetUvY;
            public float targetUvWidth;
            public float targetUvHeight;
            public bool targetFlipX;
            public ProjectedCommandRect[] otherCommandRects;
            public string imagePath;
            public string scope;
        }

        static NTSD28Q07Oid434NaturalBindingProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static bool ValidRunId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 80)
                return false;
            foreach (char ch in value)
            {
                if (!char.IsLetterOrDigit(ch) && ch != '-' && ch != '_')
                    return false;
            }
            return true;
        }

        private static void Poll()
        {
            string requestPath = ProjectPath(RequestPath);
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(requestPath))
                return;
            Request request;
            SimulationTickDriver driver = null;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(requestPath));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || (!request.requested && !request.running))
                return;

            string output = ValidRunId(request.runId)
                ? ProjectPath(Path.Combine(ResultRoot, request.runId + ".json"))
                : null;
            if (request.requested && !request.running)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (output == null || File.Exists(output) ||
                    (request.captureCentralCommand && !request.followupAttack) ||
                    (request.captureCameraPixels && !request.captureCentralCommand) ||
                    scene.path != BattleScene || scene.isDirty)
                {
                    request.requested = false;
                    File.WriteAllText(requestPath, JsonUtility.ToJson(request));
                    Debug.LogError("[Q07 OID434 Natural Binding] Invalid request or unsaved Battle Scene.");
                    return;
                }
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(requestPath, JsonUtility.ToJson(request));
                EditorApplication.EnterPlaymode();
                return;
            }
            if (!EditorApplication.isPlaying)
                return;

            if (report == null)
            {
                report = new Report
                {
                    runId = request.runId,
                    followupAttack = request.followupAttack,
                    captureCentralCommand = request.captureCentralCommand,
                    captureCameraPixels = request.captureCameraPixels,
                    scenePath = SceneManager.GetActiveScene().path,
                    sceneDirty = SceneManager.GetActiveScene().isDirty,
                    firstTickGap = -1,
                    scope = request.captureCameraPixels
                        ? "Original Battle Scene controlled Camera.Render GPU readback; no ordinary screen-frame or formal EXE pixel parity."
                        : "Original Battle Scene natural physical-device route and scoped presentation observation."
                };
            }
            try
            {
                if (DateTime.UtcNow - new DateTime(request.startedUtcTicks, DateTimeKind.Utc) >
                    TimeSpan.FromSeconds(120))
                    throw new TimeoutException("Natural OID434 observation did not complete within 120 seconds.");
                driver = SimulationTickDriver.Instance;
                if (driver?.World == null || driver.CurrentTickIndex < 5)
                    return;
                if (!naturalStarted)
                {
                    var owner = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
                    var actor = typeof(BattleTestBootstrap).GetField("firstPlayerLf2",
                        BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner) as LF2Character;
                    if (actor?.ObjectId != 2 || actor.Runtime?.HP <= 0 ||
                        actor.Health?.PP < 200 || actor.Runtime.HitStop != 0 ||
                        actor.FrameCache?.GetNativeFrameDataById(actor.Frame.N)?.State != 0 ||
                        driver.World.OneTuInput)
                        return;
                    EarlierNaturalResults.Clear();
                    if (Directory.Exists(ProjectPath(NaturalRoot)))
                    {
                        foreach (string file in Directory.GetFiles(ProjectPath(NaturalRoot),
                            "natural-after254-*.json"))
                            EarlierNaturalResults.Add(file);
                    }
                    initialTick = previousTick = driver.CurrentTickIndex;
                    report.initialTick = initialTick;
                    naturalStarted = true;
                    NTSD28UserRasenganPhysicalPlayProbeEditor.RunNaturalAfter254FromMenu();
                    return;
                }

                if (followupStarted)
                {
                    StepFollowup(driver, requestPath, request, output);
                    return;
                }

                int tick = driver.CurrentTickIndex;
                // The natural probe restores free-run before writing its result.
                // Once that result exists, later wall-clock ticks are outside this witness.
                if (tick != previousTick && !NaturalResultAvailable())
                {
                    if (tick - previousTick != 1 && firstGap < 0)
                        firstGap = tick;
                    previousTick = tick;
                    sampledTicks++;
                    driver.World.GetAllEntities(Entities);
                    foreach (LF2Entity entity in Entities)
                    {
                        if (entity?.ObjectId != 434)
                            continue;
                        report.oid434EntityFrames++;
                        if (report.firstOid434Tick < 0)
                        {
                            report.firstOid434Tick = tick;
                            report.firstOid434Action = entity.Frame?.N ?? -1;
                        }
                        if (entity.Frame?.N != 396)
                            continue;
                        if (report.visibleTick >= 0)
                            continue;
                        report.visibleTick = tick;
                        report.stableId = entity.Runtime.StableId;
                        report.slot = entity.Runtime.SlotIndex;
                        report.objectId = entity.ObjectId;
                        report.action = entity.Frame.N;
                        report.renderPic = entity.GetRenderPicIndex();
                        report.spriteResolved = entity.TryResolveCurrentSpriteEntry(
                            out BattleSpriteEntry entry);
                        if (entry != null)
                        {
                            report.visualDataId = entry.Key.VisualDataId;
                            report.effectivePic = entry.Key.EffectivePic;
                            report.sourceSheetPath = entry.SourceSheetPath;
                            report.pixelWidth = entry.PixelWidth;
                            report.pixelHeight = entry.PixelHeight;
                            report.centralBindingValid = entry.CentralBinding.IsValid;
                            report.legacySpritePresent = entry.LegacySprite != null;
                        }
                    }
                }
                report.lastTick = tick;
                report.sampledTicks = sampledTicks;
                report.firstTickGap = firstGap;
                string naturalFile = null;
                if (Directory.Exists(ProjectPath(NaturalRoot)))
                {
                    foreach (string file in Directory.GetFiles(ProjectPath(NaturalRoot),
                        "natural-after254-*.json"))
                    {
                        if (!EarlierNaturalResults.Contains(file))
                        {
                            naturalFile = file;
                            break;
                        }
                    }
                }
                if (naturalFile == null)
                    return;
                JObject natural = JObject.Parse(File.ReadAllText(naturalFile));
                report.naturalResultPath = naturalFile;
                report.naturalStatus = (string)natural["status"];
                report.naturalSkillTick = (int?)natural["skillTick"] ?? -1;
                if (request.followupAttack && report.naturalStatus == "PASS" &&
                    report.firstTickGap < 0 && report.naturalSkillTick >= initialTick)
                {
                    var owner = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
                    followupActor = typeof(BattleTestBootstrap).GetField("firstPlayerLf2",
                        BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner) as LF2Character;
                    followupKeyboard = Keyboard.current;
                    if (followupActor?.ObjectId != 2 || followupActor.Runtime?.HP <= 0 ||
                        followupActor.FrameCache?.GetNativeFrameDataById(followupActor.Frame.N)?.State != 0 ||
                        followupKeyboard == null || driver.World.OneTuInput ||
                        (followupActor.Controller as CharacterInputModule)?.AttackAction?.enabled != true)
                        throw new InvalidOperationException("Natural after254 tail is not standing and ready for physical J.");
                    followupInitialPaused = driver.IsPaused;
                    driver.SetPaused(true);
                    followupInitialTick = followupPreviousTick = driver.CurrentTickIndex;
                    report.followupQueuedAfterTick = followupInitialTick;
                    InputSystem.QueueStateEvent(followupKeyboard, new KeyboardState(Key.J));
                    followupStarted = true;
                    return;
                }
                bool matched = report.scenePath == BattleScene && !report.sceneDirty &&
                    report.naturalStatus == "PASS" && report.naturalSkillTick >= initialTick &&
                    report.visibleTick >= report.naturalSkillTick &&
                    report.firstTickGap < 0 && report.renderPic == 36 &&
                    report.spriteResolved && report.visualDataId == 434 &&
                    report.effectivePic == 36 &&
                    report.sourceSheetPath?.Replace('\\', '/').EndsWith(
                        "c/nar/a/ras.png", StringComparison.OrdinalIgnoreCase) == true &&
                    report.pixelWidth == 48f && report.pixelHeight == 48f &&
                    report.centralBindingValid;
                report.status = matched ? "PASS" : "FAIL";
                if (!matched)
                    report.error = "Natural result or action396 entity Sprite binding failed the scoped contract.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
                if (followupStarted)
                {
                    InputSystem.QueueStateEvent(followupKeyboard, new KeyboardState());
                    InputSystem.Update();
                    driver.SetPaused(followupInitialPaused);
                }
            }
            WriteAndExit(requestPath, request, output);
        }

        private static void StepFollowup(SimulationTickDriver driver,
            string requestPath, Request request, string output)
        {
            try
            {
                if (centralCapturePending)
                {
                    ObserveCentralCommand(driver, request);
                    return;
                }
                if (driver.CurrentTickIndex == followupPreviousTick &&
                    !driver.StepOneTick(ignorePaused: true))
                    throw new InvalidOperationException("Follow-up controlled physical-input tick did not advance.");
                int tick = driver.CurrentTickIndex;
                if (tick - followupPreviousTick != 1 && report.followupFirstTickGap < 0)
                    report.followupFirstTickGap = tick;
                followupPreviousTick = tick;
                report.followupSampledTicks++;
                report.lastTick = tick;
                SimulationPlayerInput applied = default;
                FrameInputSet frame = driver.LastAppliedFrameInput;
                if (frame?.Players != null)
                {
                    foreach (SimulationPlayerInput player in frame.Players)
                    {
                        if (player.PlayerSlot == 0)
                        {
                            applied = player;
                            break;
                        }
                    }
                }
                if ((applied.Buttons & SimulationInputButtons.Jump) != 0 &&
                    report.followupFirstInputTick < 0)
                    report.followupFirstInputTick = tick;
                report.followupLastActorAction = followupActor.Frame?.N ?? -1;
                if (report.followupFirstInputTick >= 0 &&
                    report.followupFirstAttackAction < 0 &&
                    (report.followupLastActorAction == 20 || report.followupLastActorAction == 25))
                    report.followupFirstAttackAction = report.followupLastActorAction;
                if (followupActor.Frame?.N == 25 && report.followupAction25Tick < 0)
                    report.followupAction25Tick = tick;
                driver.World.GetAllEntities(Entities);
                foreach (LF2Entity entity in Entities)
                {
                    if (entity?.ObjectId != 434)
                        continue;
                    if (entity.Frame?.N == 100 && report.followupAction100Tick < 0)
                        report.followupAction100Tick = tick;
                    if (entity.Frame?.N != 396 || report.visibleTick >= 0)
                        continue;
                    report.visibleTick = tick;
                    report.stableId = entity.Runtime.StableId;
                    report.slot = entity.Runtime.SlotIndex;
                    report.objectId = entity.ObjectId;
                    report.action = entity.Frame.N;
                    report.renderPic = entity.GetRenderPicIndex();
                    report.spriteResolved = entity.TryResolveCurrentSpriteEntry(
                        out BattleSpriteEntry entry);
                    if (entry != null)
                    {
                        report.visualDataId = entry.Key.VisualDataId;
                        report.effectivePic = entry.Key.EffectivePic;
                        report.sourceSheetPath = entry.SourceSheetPath;
                        report.pixelWidth = entry.PixelWidth;
                        report.pixelHeight = entry.PixelHeight;
                        report.centralBindingValid = entry.CentralBinding.IsValid;
                        report.legacySpritePresent = entry.LegacySprite != null;
                    }
                }
                if (tick == followupInitialTick + 2)
                    InputSystem.QueueStateEvent(followupKeyboard, new KeyboardState());
                if (report.visibleTick < 0 && tick < followupInitialTick + 20)
                    return;
                bool entityMatched = report.scenePath == BattleScene && !report.sceneDirty &&
                    report.naturalStatus == "PASS" && report.firstTickGap < 0 &&
                    report.followupFirstTickGap < 0 &&
                    report.followupFirstInputTick > report.followupQueuedAfterTick &&
                    (report.followupFirstAttackAction == 20 ||
                     report.followupFirstAttackAction == 25) &&
                    report.followupAction100Tick >= report.followupFirstInputTick &&
                    report.visibleTick >= report.followupAction100Tick &&
                    report.renderPic == 36 && report.spriteResolved &&
                    report.visualDataId == 434 && report.effectivePic == 36 &&
                    report.sourceSheetPath?.Replace('\\', '/').EndsWith(
                        "c/nar/a/ras.png", StringComparison.OrdinalIgnoreCase) == true &&
                    report.pixelWidth == 48f && report.pixelHeight == 48f &&
                    report.centralBindingValid;
                if (entityMatched && request.captureCentralCommand)
                {
                    centralCapturePending = true;
                    centralCaptureStartedFrame = Time.frameCount;
                    return;
                }
                report.status = entityMatched ? "PASS" : "FAIL";
                if (!entityMatched)
                    report.error = "Physical attack tail did not reach or bind OID434/action396 under the scoped contract.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }
            finally
            {
                if (report.status != null)
                {
                    InputSystem.QueueStateEvent(followupKeyboard, new KeyboardState());
                    InputSystem.Update();
                    driver.SetPaused(followupInitialPaused);
                    WriteAndExit(requestPath, request, output);
                }
            }
        }

        private static void ObserveCentralCommand(SimulationTickDriver driver, Request request)
        {
            if (driver.CurrentTickIndex != report.visibleTick)
                throw new InvalidOperationException("World tick advanced before central command observation.");
            if (Time.frameCount <= centralCaptureStartedFrame)
                return;
            BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            report.planValid = plan.IsValid;
            report.planOwner = plan.Owner.ToString();
            report.planRequestedMode = plan.RequestedMode.ToString();
            report.planSimulationTick = plan.SimulationTick;
            report.planDisplayTick = plan.DisplayTick;
            report.planGeneration = plan.Generation;
            report.submissionReady = plan.Submission != null &&
                !plan.Submission.IsRetired && plan.UsesCentralPixels;
            report.submissionTick = plan.Submission?.TickIndex ?? -1;
            BattlePresentationFrame frame = plan.Submission?.CapturedFrame;
            report.frozenFrameTick = frame?.TickIndex ?? -1;
            report.commandsMaterialized = frame?.CommandsMaterialized ?? false;
            report.commandCount = frame?.CommandCount ?? 0;
            report.matchingEntityCommandCount = 0;
            int targetCommandIndex = -1;
            BattleRenderCommand targetCommand = default;
            if (frame != null && frame.CommandsMaterialized)
            {
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (command.Type != BattleRenderCommandType.Entity ||
                        command.StableId != report.stableId ||
                        command.RuntimeSlot != report.slot ||
                        command.VisualDataId != 434 || command.EffectivePic != 36)
                        continue;
                    report.matchingEntityCommandCount++;
                    targetCommandIndex = index;
                    targetCommand = command;
                    report.matchingCommandWidth = command.Size.x;
                    report.matchingCommandHeight = command.Size.y;
                    report.matchingCommandSortOrder = command.SortOrder;
                }
            }
            if ((plan.DisplayTick != report.visibleTick || frame == null ||
                 !frame.CommandsMaterialized) &&
                Time.frameCount < centralCaptureStartedFrame + 20)
                return;
            bool matched = plan.IsValid && plan.UsesCentralPixels &&
                plan.RequestedMode == BattlePresentationBackendMode.CentralOnly &&
                plan.DisplayTick == report.visibleTick &&
                report.submissionReady && report.submissionTick == report.visibleTick &&
                ReferenceEquals(plan.CapturedFrame, frame) &&
                report.frozenFrameTick == report.visibleTick &&
                report.commandsMaterialized && report.matchingEntityCommandCount == 1 &&
                report.matchingCommandWidth == 48f &&
                report.matchingCommandHeight == 48f;
            if (matched && request.captureCameraPixels)
                matched = CaptureCameraPixels(plan, targetCommand, targetCommandIndex);
            report.status = matched ? "PASS" : "FAIL";
            if (!matched)
                report.error = request.captureCameraPixels
                    ? "Current target command did not produce an attributable isolated camera pixel and executed submission."
                    : "Target OID434/pic36 Entity command was not found in the current frozen central submission.";
        }

        private static bool CaptureCameraPixels(BattlePixelFramePlan plan,
            BattleRenderCommand targetCommand, int targetCommandIndex)
        {
            Camera camera = NTSDRenderSpace.WorldCamera;
            report.worldCameraEnabled = camera != null && camera.enabled &&
                camera.gameObject.activeInHierarchy;
            if (!report.worldCameraEnabled || targetCommandIndex < 0)
                return false;
            report.cameraLeaseAccepted =
                BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    camera, CameraRenderType.Base, camera.cameraType, true,
                    out BattleCentralSubmission.BattleCentralSubmissionLease lease);
            if (!report.cameraLeaseAccepted)
                return false;
            using (lease)
            {
                report.cameraLeaseTick = lease.TickIndex;
                report.cameraLeaseGeneration = lease.Generation;
            }
            if (report.cameraLeaseTick != plan.DisplayTick ||
                report.cameraLeaseGeneration != plan.Generation)
                return false;

            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            report.captureWidth = CaptureWidth;
            report.captureHeight = height;
            RectInt target = ProjectCommandBounds(camera, targetCommand, CaptureWidth, height);
            report.targetX = target.x;
            report.targetY = target.y;
            report.targetWidth = target.width;
            report.targetHeight = target.height;
            report.targetUvX = targetCommand.NormalizedUv.x;
            report.targetUvY = targetCommand.NormalizedUv.y;
            report.targetUvWidth = targetCommand.NormalizedUv.width;
            report.targetUvHeight = targetCommand.NormalizedUv.height;
            report.targetFlipX = targetCommand.FlipX;
            var otherBounds = new List<RectInt>(plan.CapturedFrame.CommandCount - 1);
            var otherRectRows = new List<ProjectedCommandRect>(
                plan.CapturedFrame.CommandCount - 1);
            for (int index = 0; index < plan.CapturedFrame.CommandCount; index++)
            {
                if (index == targetCommandIndex)
                    continue;
                BattleRenderCommand otherCommand = plan.CapturedFrame.GetCommand(index);
                RectInt bounds = ProjectCommandBounds(camera, otherCommand,
                    CaptureWidth, height);
                if (bounds.width > 0 && bounds.height > 0)
                {
                    otherBounds.Add(bounds);
                    otherRectRows.Add(new ProjectedCommandRect
                    {
                        index = index,
                        type = otherCommand.Type.ToString(),
                        sortOrder = otherCommand.SortOrder,
                        visualDataId = otherCommand.VisualDataId,
                        effectivePic = otherCommand.EffectivePic,
                        uvX = otherCommand.NormalizedUv.x,
                        uvY = otherCommand.NormalizedUv.y,
                        uvWidth = otherCommand.NormalizedUv.width,
                        uvHeight = otherCommand.NormalizedUv.height,
                        flipX = otherCommand.FlipX,
                        flipY = otherCommand.FlipY,
                        x = bounds.x,
                        y = bounds.y,
                        width = bounds.width,
                        height = bounds.height,
                    });
                    ProjectedCommandRect row = otherRectRows[otherRectRows.Count - 1];
                    if (otherCommand.Type == BattleRenderCommandType.Entity &&
                        plan.CapturedFrame.BoundCatalogForAcceptance != null &&
                        plan.CapturedFrame.BoundCatalogForAcceptance.TryGet(
                            otherCommand.VisualDataId, otherCommand.EffectivePic,
                            out BattleSpriteEntry otherEntry) && otherEntry != null)
                    {
                        row.sourceSheetPath = otherEntry.SourceSheetPath;
                        row.sourceRectX = otherEntry.PixelRect.x;
                        row.sourceRectY = otherEntry.PixelRect.y;
                        row.sourceRectWidth = otherEntry.PixelRect.width;
                        row.sourceRectHeight = otherEntry.PixelRect.height;
                    }
                }
            }
            report.otherCommandRects = otherRectRows.ToArray();

            var targetTexture = new RenderTexture(CaptureWidth, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            RenderTexture previousActive = RenderTexture.active;
            var savedCamera = new CameraState(camera);
            Texture2D readback = null;
            try
            {
                targetTexture.Create();
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.white;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = targetTexture;
                report.cameraDrawCountBefore = BattleCentralRenderSystem.Diagnostics.SubmissionCount;
                camera.Render();
                report.cameraDrawCountAfter = BattleCentralRenderSystem.Diagnostics.SubmissionCount;
                report.cameraLastSubmissionDrawCount =
                    BattleCentralRenderSystem.Diagnostics.LastSubmissionDrawCount;
                RenderTexture.active = targetTexture;
                readback = new Texture2D(CaptureWidth, height, TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, height), 0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                report.targetRegionArea = target.width * target.height;
                for (int y = target.yMin; y < target.yMax; y++)
                for (int x = target.xMin; x < target.xMax; x++)
                {
                    Color32 pixel = pixels[y * CaptureWidth + x];
                    bool nonClear = pixel.r < 250 || pixel.g < 250 || pixel.b < 250;
                    if (nonClear)
                        report.targetRegionNonClear++;
                    bool overlapsOtherCommand = false;
                    foreach (RectInt other in otherBounds)
                    {
                        if (x >= other.xMin && x < other.xMax &&
                            y >= other.yMin && y < other.yMax)
                        {
                            overlapsOtherCommand = true;
                            break;
                        }
                    }
                    if (overlapsOtherCommand)
                        continue;
                    report.targetExclusiveArea++;
                    if (nonClear)
                        report.targetExclusiveNonClear++;
                }
                string relativeImage = ResultRoot + "/" + report.runId + ".png";
                string absoluteImage = ProjectPath(relativeImage);
                Directory.CreateDirectory(Path.GetDirectoryName(absoluteImage));
                using (var stream = new FileStream(absoluteImage, FileMode.CreateNew, FileAccess.Write))
                {
                    byte[] image = readback.EncodeToPNG();
                    stream.Write(image, 0, image.Length);
                }
                report.imagePath = relativeImage;
                return report.cameraDrawCountAfter > report.cameraDrawCountBefore &&
                    report.cameraLastSubmissionDrawCount > 0 &&
                    report.targetExclusiveArea > 0 && report.targetExclusiveNonClear > 0;
            }
            finally
            {
                RenderTexture.active = previousActive;
                savedCamera.Restore(camera);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                targetTexture.Release();
                UnityEngine.Object.DestroyImmediate(targetTexture);
            }
        }

        private static RectInt ProjectCommandBounds(Camera camera, BattleRenderCommand command,
            int width, int height)
        {
            float worldWidth = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                NTSDRenderSpace.BattleVisualScale;
            float worldHeight = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * worldWidth;
            float bottom = command.Position.y - command.Pivot.y * worldHeight;
            Vector3 lower = camera.WorldToViewportPoint(new Vector3(left, bottom, command.Position.z));
            Vector3 upper = camera.WorldToViewportPoint(new Vector3(
                left + worldWidth, bottom + worldHeight, command.Position.z));
            int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.x, upper.x) * width), 0, width);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.x, upper.x) * width), 0, width);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.y, upper.y) * height), 0, height);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.y, upper.y) * height), 0, height);
            return new RectInt(x0, y0, x1 - x0, y1 - y0);
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
        }

        private static bool NaturalResultAvailable()
        {
            if (!Directory.Exists(ProjectPath(NaturalRoot)))
                return false;
            foreach (string file in Directory.GetFiles(ProjectPath(NaturalRoot),
                "natural-after254-*.json"))
            {
                if (!EarlierNaturalResults.Contains(file))
                    return true;
            }
            return false;
        }

        private static void WriteAndExit(string requestPath, Request request, string output)
        {
            Directory.CreateDirectory(ProjectPath(ResultRoot));
            try
            {
                using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(report, true));
            }
            finally
            {
                request.running = false;
                File.WriteAllText(requestPath, JsonUtility.ToJson(request));
                EditorApplication.ExitPlaymode();
            }
        }
    }
}
#endif
