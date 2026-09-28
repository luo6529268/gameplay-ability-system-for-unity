#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q09Etc998FallbackBattlePlayProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string GameConfigAssetPath =
            "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string RequestPath = "Temp/NTSD28_Q09_Etc998_Fallback_20260928.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P12-ETC998-UNITY-FALLBACK-001";
        private const string PixelResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P12-ETC998-LEGACY-TARGET-PIXEL-001";
        private const int MaxTicks = 4;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public bool legacyBoot;
        }

        [Serializable]
        private sealed class Sample
        {
            public string side;
            public int sourceX;
            public int hostSlot;
            public int hostFrameAtBirth = -1;
            public int hostHpAtBirth = -1;
            public int childSlot = -1;
            public int childOid = -1;
            public int childAction = -1;
            public int childState = -1;
            public int childOwner = -2;
            public int childSpawner = -1;
            public string physicalDir;
            public bool snapshotFound;
            public bool snapshotFlipX;
            public float spriteWidth;
            public float centerX;
            public float visualScale;
            public bool commandFound;
            public bool commandFlipX;
            public float commandWorldX;
            public float observedPivotX;
            public float expectedPivotX;
            public float pixelDifference;
            public bool legacyBodyFound;
            public bool legacyBodyEnabled;
            public bool legacyBodyFlipX;
            public float legacyWorldX;
            public float legacyWorldY;
            public int legacyTargetPixelCount;
            public int legacyPixelMinX = -1;
            public int legacyPixelMinY = -1;
            public int legacyPixelMaxX = -1;
            public int legacyPixelMaxY = -1;
            public int commandTick = -1;
            public bool childReleased;
        }

        [Serializable]
        private sealed class Report
        {
            public string status = "FAIL";
            public string error;
            public string runId;
            public string contentRoot;
            public string backendMode;
            public bool configIsRuntimeClone;
            public bool logicOnlyMaterialization;
            public bool dedicatedWorkerActive;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public int selectedEtcMode = -1;
            public int stepCount;
            public int worldTick;
            public int publishedTick = -1;
            public float cameraVisibleLeft;
            public float cameraVisibleRight;
            public float viewportLeft;
            public float viewportUnitsPerPixelX;
            public int objectsBefore;
            public int objectsAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int borrowersBefore;
            public int borrowersAfter;
            public bool hostsReleased;
            public bool pauseRestored;
            public int pixelTickBefore = -1;
            public int pixelTickAfter = -1;
            public bool pixelCameraRestored;
            public bool pixelBodiesRestored;
            public string bothBodiesPng;
            public string leftBodyOffPng;
            public string rightBodyOffPng;
            public List<Sample> samples = new List<Sample>();
        }

        private static Request request;
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character leftHost;
        private static LF2Character rightHost;
        private static readonly LF2Entity[] children = new LF2Entity[2];
        private static NTSDRenderSpace.ViewportTransformSnapshot viewport;
        private static int phase;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int expectedTick = -1;
        private static bool oldPaused;
        private static bool pauseCaptured;
        private static double deadline;

        static NTSD28Q09Etc998FallbackBattlePlayProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged -= RestoreProbeConfigAfterPlay;
            EditorApplication.playModeStateChanged += RestoreProbeConfigAfterPlay;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureOptInLegacyBeforeSceneLoad()
        {
            string path = ProjectPath(RequestPath);
            if (!File.Exists(path))
                return;
            Request pending;
            try
            {
                pending = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            }
            catch (Exception)
            {
                return;
            }
            if (pending == null || !pending.requested ||
                !ValidRunId(pending.runId) || File.Exists(ResultPath(pending.runId)))
                return;
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(
                GameConfigAssetPath);
            if (source == null)
                return;
            GameConfig current = GameConfig.Instance;
            if (current != null && current != source)
            {
                if (!IsProbeConfigClone(current, source))
                    return;
                SetGameConfigInstance(null);
                UnityEngine.Object.DestroyImmediate(current);
            }
            if (!pending.legacyBoot)
            {
                if (GameConfig.Instance == null)
                    GameConfig.Instance = source;
                return;
            }
            if (GameConfig.Instance == source)
                SetGameConfigInstance(null);
            GameConfig copy = UnityEngine.Object.Instantiate(source);
            copy.hideFlags = HideFlags.DontSave;
            copy.BattlePresentationBackendName =
                nameof(BattlePresentationBackendMode.LegacyOnly);
            GameConfig.Instance = copy;
        }

        private static void RestoreProbeConfigAfterPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(
                GameConfigAssetPath);
            GameConfig current = GameConfig.Instance;
            if (!IsProbeConfigClone(current, source))
                return;
            SetGameConfigInstance(null);
            GameConfig.Instance = source;
            UnityEngine.Object.DestroyImmediate(current);
        }

        private static bool IsProbeConfigClone(GameConfig current,
            GameConfig source)
        {
            return current != null && source != null &&
                   !AssetDatabase.Contains(current) &&
                   current.name == source.name + "(Clone)" &&
                   (current.hideFlags & HideFlags.DontSave) == HideFlags.DontSave &&
                   current.BattlePresentationBackendName ==
                       nameof(BattlePresentationBackendMode.LegacyOnly) &&
                   current.BattleContentRuntimeRoot == source.BattleContentRuntimeRoot;
        }

        private static void SetGameConfigInstance(GameConfig value)
        {
            FieldInfo field = typeof(GameConfig).GetField("_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(field != null, "GameConfig singleton backing field changed.");
            field.SetValue(null, value);
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(ProjectPath(RequestPath)))
                return;
            Request next;
            try
            {
                next = JsonUtility.FromJson<Request>(File.ReadAllText(ProjectPath(RequestPath)));
            }
            catch (IOException)
            {
                return;
            }
            if (next == null || (!next.requested && !next.running) ||
                !ValidRunId(next.runId) || File.Exists(ResultPath(next.runId)))
                return;
            if (!EditorApplication.isPlaying)
            {
                if (!next.requested || next.running ||
                    EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (scene.path != ScenePath || scene.isDirty)
                {
                    WriteImmediateFailure(next, "A clean saved Battle Scene is required.");
                    return;
                }
                EditorApplication.EnterPlaymode();
                return;
            }

            if (request == null)
            {
                request = next;
                request.requested = false;
                request.running = true;
                File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
                report = new Report { runId = request.runId };
                report.samples.Add(new Sample { side = "left", sourceX = 50, hostSlot = 8 });
                report.samples.Add(new Sample { side = "right", sourceX = 1329, hostSlot = 9 });
                deadline = EditorApplication.timeSinceStartup + 240.0;
            }
            try
            {
                Require(EditorApplication.timeSinceStartup < deadline,
                    "Battle Play probe timed out.");
                if (phase == 0)
                    Prepare();
                else if (phase == 1)
                    WaitForPauseAndSpawn();
                else
                    StepAndObserve();
            }
            catch (Exception error)
            {
                report.error = error.ToString();
                Finish();
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
                "Battle Scene changed during Play.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == ContentRoot,
                "Formal content root is not selected.");
            BattlePresentationBackendMode expectedBackend = request.legacyBoot
                ? BattlePresentationBackendMode.LegacyOnly
                : BattlePresentationBackendMode.CentralOnly;
            report.backendMode = driver.PresentationBackendMode.ToString();
            report.configIsRuntimeClone = GameConfig.Instance != null &&
                !AssetDatabase.Contains(GameConfig.Instance);
            report.logicOnlyMaterialization = world.UsesLogicOnlyEntityMaterialization;
            report.dedicatedWorkerActive =
                driver.DedicatedSimulationWorkerActiveForDiagnostics;
            Require(driver.PresentationBackendMode == expectedBackend &&
                    world.BattlePresentation.Mode == expectedBackend,
                "Requested Battle presentation backend was not selected before preparation.");
            if (request.legacyBoot)
                Require(report.configIsRuntimeClone &&
                        !report.logicOnlyMaterialization &&
                        !report.dedicatedWorkerActive,
                    "Legacy boot must use an in-memory config and non-worker materialization.");
            Require(CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                "Formal content publication is not ready.");
            report.selectedEtcMode = world.RuntimeDataCatalog.ProjectModeSnapshot.SelectedModeEtcMode;
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.orthographic && camera.aspect > 0f,
                "An active orthographic Battle World camera is required.");
            viewport = NTSDRenderSpace.CaptureViewportTransform();
            Require(viewport.UnitsPerPixelX > 0f,
                "Battle viewport conversion is unavailable.");
            report.viewportLeft = viewport.Left;
            report.viewportUnitsPerPixelX = viewport.UnitsPerPixelX;
            float halfWidth = camera.orthographicSize * camera.aspect;
            report.cameraVisibleLeft =
                (camera.transform.position.x - halfWidth - viewport.Left) /
                viewport.UnitsPerPixelX;
            report.cameraVisibleRight =
                (camera.transform.position.x + halfWidth - viewport.Left) /
                viewport.UnitsPerPixelX;
            oldPaused = driver.IsPaused;
            pauseCaptured = true;
            driver.SetPaused(true);
            phase = 1;
        }

        private static void WaitForPauseAndSpawn()
        {
            Require(ReferenceEquals(world, driver.World), "Production World changed.");
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                "Dedicated worker failed.");
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
            report.sceneHashBefore = HashFile(ProjectPath(ScenePath));
            report.objectsBefore = world.ObjectCount;
            report.slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.borrowersBefore = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            LF2CharacterDataWrapper data = world.RuntimeCharacterConfigs.Resolve(7);
            Require(data?.characterData != null, "Formal Lee OID7 is unavailable.");
            leftHost = CreateHost(data, report.samples[0]);
            rightHost = CreateHost(data, report.samples[1]);
            phase = 2;
        }

        private static LF2Character CreateHost(LF2CharacterDataWrapper data, Sample sample)
        {
            Require(world.FindEntityByRuntimeSlotForQuery(sample.hostSlot) == null,
                "Revival fixture slot is occupied: " + sample.hostSlot);
            var host = new LF2Character();
            host.ModuleInitialize();
            host.ObjectId = 7;
            host.Name = "Q09Etc998_" + sample.side;
            host.FrameCache.Load(data);
            host.SetRequiredRuntimeSlot(sample.hostSlot);
            world.Register(host);
            host.ImmediateFrame(230);
            host.Initialize(500, 500);
            host.AiControlled = false;
            host.Team = 1;
            host.RelationTeam = 1;
            host.Runtime.Unk360 = -1;
            host.Health.HP = 0;
            host.Health.HPBound = 10;
            host.Health.HP3 = 10;
            host.Health.PP = 77;
            host.HPOrig = 6;
            host.HP2Orig = 1;
            host.RespawnCount = 80;
            host.HitStun = 3;
            double physicalX = sample.sourceX * world.FixedViewRunDistanceScale;
            double physicalZ = Math.Max(world.Runtime.Stage.ZMin + 20,
                Math.Min(world.Runtime.Stage.ZMax - 20,
                    350 * world.FixedViewRunVerticalDistanceScale));
            host.Runtime.SetPosition(physicalX, -20, physicalZ);
            host.Runtime.SetSourceRulePosition(sample.sourceX, 350);
            host.Runtime.SetVelocity(0, 0, 0);
            host.Runtime.SyncIntegerPosition();
            host.Runtime.SyncSourceRuleIntegerPosition();
            host.RefreshRuntimeSnapshot();
            Require(host.Frame.N == 230 && host.Frame.D?.state == LF2States.Lying &&
                host.Runtime.SlotIndex == sample.hostSlot && host.Health.HP == 0,
                "Formal Lee revival fixture did not hold its initial gate.");
            return host;
        }

        private static void StepAndObserve()
        {
            Require(driver.IsPaused && ReferenceEquals(world, driver.World),
                "Paused production World changed.");
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                "Dedicated worker failed during the complete tick.");
            if (expectedTick < 0)
            {
                if (report.stepCount >= MaxTicks)
                {
                    report.status = "CHILD_OR_BODY_NOT_OBSERVED";
                    report.error = "Both OID998/state9997 bodies were not observed within four full ticks.";
                    Finish();
                    return;
                }
                expectedTick = driver.CurrentTickIndex + 1;
                bool accepted = driver.DedicatedSimulationWorkerActiveForDiagnostics
                    ? driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(true)
                    : driver.StepOneTick(ignorePaused: true, buildPresentation: true);
                Require(accepted, "Production Driver rejected the diagnostic tick: " +
                    driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
                return;
            }
            if (driver.CurrentTickIndex < expectedTick ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            Require(driver.CurrentTickIndex == expectedTick,
                "Production Driver advanced more than one requested tick.");
            report.stepCount++;
            report.worldTick = expectedTick;
            CaptureSamples();
            bool bothObserved = request.legacyBoot
                ? report.samples[0].legacyBodyFound &&
                  report.samples[1].legacyBodyFound
                : report.samples[0].commandFound &&
                  report.samples[1].commandFound;
            if (bothObserved)
            {
                if (request.legacyBoot)
                    CaptureLegacyTargetPixels();
                bool matching = true;
                for (int i = 0; i < report.samples.Count; i++)
                {
                    Sample sample = report.samples[i];
                    matching &= sample.childOid == 998 &&
                                sample.childAction == 6 &&
                                sample.childState == 9997 &&
                                sample.childOwner == -1 &&
                                sample.physicalDir == "right" &&
                                sample.snapshotFound && !sample.snapshotFlipX &&
                                sample.pixelDifference <= 0.75f;
                    if (request.legacyBoot)
                        matching &= sample.legacyBodyEnabled &&
                                    !sample.legacyBodyFlipX &&
                                    sample.legacyTargetPixelCount > 0;
                    else
                        matching &= !sample.commandFlipX;
                }
                if (request.legacyBoot)
                    matching &= report.pixelTickBefore == report.pixelTickAfter &&
                                report.pixelCameraRestored &&
                                report.pixelBodiesRestored;
                report.status = matching
                    ? request.legacyBoot
                        ? "PASS_LEGACY_TARGET_PIXELS"
                        : "PASS_CENTRAL_FALLBACK"
                    : "FIRST_DIFFERENCE";
                Finish();
                return;
            }
            expectedTick = -1;
        }

        private static void CaptureSamples()
        {
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            report.publishedTick = published?.TickIndex ?? -1;
            BattlePixelFramePlan plan = request.legacyBoot
                ? default
                : BattleCentralRenderSystem.PrepareFrame(world);
            BattlePresentationFrame commandFrame = request.legacyBoot
                ? null
                : plan.CapturedFrame;
            for (int sampleIndex = 0; sampleIndex < report.samples.Count; sampleIndex++)
            {
                Sample sample = report.samples[sampleIndex];
                if (request.legacyBoot ? sample.legacyBodyFound : sample.commandFound)
                    continue;
                LF2Entity found = null;
                for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                    if (candidate?.ObjectId == 998 &&
                        candidate.SpawnerEntityIndex == sample.hostSlot)
                    {
                        found = candidate;
                        break;
                    }
                }
                if (found == null)
                    continue;
                children[sampleIndex] = found;
                sample.hostFrameAtBirth = sampleIndex == 0 ? leftHost.Frame.N : rightHost.Frame.N;
                sample.hostHpAtBirth = sampleIndex == 0 ? leftHost.Health.HP : rightHost.Health.HP;
                sample.childSlot = found.Runtime.SlotIndex;
                sample.childOid = found.ObjectId;
                sample.childAction = found.Frame.N;
                sample.childState = (int)(found.Frame.D?.state ?? -1);
                sample.childOwner = found.Runtime.OwnerSlotIndex;
                sample.childSpawner = found.SpawnerEntityIndex;
                sample.physicalDir = found.Runtime.Dir;
                if (published == null)
                    continue;
                for (int rank = 0; rank < published.EntityCount; rank++)
                {
                    BattlePresentationEntitySnapshot entity = published.GetEntity(rank);
                    if (entity.RuntimeSlot != sample.childSlot || entity.ObjectId != 998)
                        continue;
                    sample.snapshotFound = true;
                    sample.snapshotFlipX = entity.FlipX;
                    sample.spriteWidth = found.GetSpriteWidthPxForRender();
                    sample.centerX = entity.CenterX;
                    sample.visualScale = NTSDRenderSpace.BattleVisualScale;
                    if (request.legacyBoot)
                    {
                        Require(found.Renderer != null,
                            "Natural owner-less effect has no Legacy renderer.");
                        found.Renderer.ForceRefreshPresentation();
                        SpriteRenderer body =
                            found.Renderer.GetComponent<SpriteRenderer>();
                        Require(body != null && body.sprite != null,
                            "Natural owner-less effect has no body SpriteRenderer/Sprite.");
                        Transform root = found.Renderer.transform.parent != null
                            ? found.Renderer.transform.parent
                            : found.Renderer.transform;
                        sample.legacyBodyFound = true;
                        sample.legacyBodyEnabled = body.enabled;
                        sample.legacyBodyFlipX = body.flipX;
                        sample.legacyWorldX = root.position.x;
                        sample.legacyWorldY = root.position.y;
                        sample.observedPivotX =
                            (root.position.x - viewport.Left) /
                            viewport.UnitsPerPixelX;
                        sample.expectedPivotX =
                            ComputeExpectedFallbackPivotX(entity, sample,
                                published.TickIndex);
                        sample.pixelDifference = Mathf.Abs(
                            sample.observedPivotX - sample.expectedPivotX);
                        break;
                    }
                    if (!plan.IsValid || plan.IsStale || commandFrame == null ||
                        !commandFrame.CommandsMaterialized || plan.Submission == null)
                        break;
                    for (int index = 0; index < commandFrame.CommandCount; index++)
                    {
                        BattleRenderCommand command = commandFrame.GetCommand(index);
                        if (command.RuntimeSlot != sample.childSlot ||
                            command.Type != BattleRenderCommandType.Entity)
                            continue;
                        sample.commandFound = true;
                        sample.commandFlipX = command.FlipX;
                        sample.commandWorldX = command.Position.x;
                        sample.commandTick = commandFrame.TickIndex;
                        sample.observedPivotX =
                            (command.Position.x - viewport.Left) / viewport.UnitsPerPixelX;
                        sample.expectedPivotX =
                            ComputeExpectedFallbackPivotX(entity, sample,
                                commandFrame.TickIndex);
                        sample.pixelDifference = Mathf.Abs(
                            sample.observedPivotX - sample.expectedPivotX);
                        break;
                    }
                    break;
                }
            }
        }

        private static float ComputeExpectedFallbackPivotX(
            BattlePresentationEntitySnapshot entity, Sample sample, int tickIndex)
        {
            float renderedWidth = sample.spriteWidth * sample.visualScale;
            int extraX = entity.FrameDelay < 0
                ? 6 * (tickIndex & 1) - 3 : 0;
            int screenX = entity.XInt + (int)entity.RenderOffsetX -
                entity.CameraX + extraX;
            float ordinaryPivotX = entity.FlipX
                ? screenX + sample.visualScale *
                  (sample.centerX - sample.spriteWidth * 0.5f)
                : screenX + sample.visualScale *
                  (sample.spriteWidth * 0.5f - sample.centerX);
            float maxLeft = Mathf.Max(report.cameraVisibleLeft,
                report.cameraVisibleRight - 1f - renderedWidth);
            float expectedLeft = Mathf.Clamp(
                ordinaryPivotX - renderedWidth * 0.5f,
                report.cameraVisibleLeft, maxLeft);
            return expectedLeft + renderedWidth * 0.5f +
                   entity.HeldVisualAttachmentOffsetPixels.x +
                   entity.LocalOffsetPixels.x * sample.visualScale;
        }

        private static void CaptureLegacyTargetPixels()
        {
            const int width = 1280;
            const int height = 720;
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.isActiveAndEnabled,
                "An active Battle World camera is required for GPU capture.");
            SpriteRenderer leftBody = children[0]?.Renderer?.GetComponent<SpriteRenderer>();
            SpriteRenderer rightBody = children[1]?.Renderer?.GetComponent<SpriteRenderer>();
            Require(leftBody != null && leftBody.enabled && leftBody.sprite != null &&
                    rightBody != null && rightBody.enabled && rightBody.sprite != null,
                "Both natural Legacy bodies must be visible before GPU capture.");

            RenderTexture originalTarget = camera.targetTexture;
            RenderTexture originalActive = RenderTexture.active;
            bool originalLeft = leftBody.enabled;
            bool originalRight = rightBody.enabled;
            report.pixelTickBefore = world.CurrentTickIndex;
            var target = new RenderTexture(width, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            Texture2D readback = null;
            try
            {
                target.Create();
                readback = new Texture2D(width, height,
                    TextureFormat.RGBA32, false, true);
                camera.targetTexture = target;
                Color32[] both = CaptureLegacyCameraPixels(camera, target,
                    readback, "both-on", out report.bothBodiesPng);
                leftBody.enabled = false;
                Color32[] leftOff = CaptureLegacyCameraPixels(camera, target,
                    readback, "left-off", out report.leftBodyOffPng);
                leftBody.enabled = originalLeft;
                rightBody.enabled = false;
                Color32[] rightOff = CaptureLegacyCameraPixels(camera, target,
                    readback, "right-off", out report.rightBodyOffPng);
                CountTargetPixels(both, leftOff, report.samples[0], width);
                CountTargetPixels(both, rightOff, report.samples[1], width);
                report.pixelTickAfter = world.CurrentTickIndex;
            }
            finally
            {
                leftBody.enabled = originalLeft;
                rightBody.enabled = originalRight;
                camera.targetTexture = originalTarget;
                RenderTexture.active = originalActive;
                report.pixelBodiesRestored = leftBody.enabled == originalLeft &&
                                             rightBody.enabled == originalRight;
                report.pixelCameraRestored = camera.targetTexture == originalTarget &&
                                             RenderTexture.active == originalActive;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Color32[] CaptureLegacyCameraPixels(Camera camera,
            RenderTexture target, Texture2D readback, string suffix,
            out string relativePath)
        {
            relativePath = PixelResultRoot + "/" + request.runId +
                           "-" + suffix + ".png";
            string output = ProjectPath(relativePath);
            Require(!File.Exists(output), "Refusing to overwrite Legacy camera PNG.");
            camera.Render();
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0f, 0f, target.width, target.height),
                0, 0, false);
            readback.Apply(false, false);
            Color32[] pixels = readback.GetPixels32();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, readback.EncodeToPNG());
            return pixels;
        }

        private static void CountTargetPixels(Color32[] both, Color32[] bodyOff,
            Sample sample, int width)
        {
            Require(both.Length == bodyOff.Length,
                "GPU captures have different pixel counts.");
            for (int index = 0; index < both.Length; index++)
            {
                Color32 a = both[index];
                Color32 b = bodyOff[index];
                if (a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a)
                    continue;
                int x = index % width;
                int y = index / width;
                if (sample.legacyTargetPixelCount++ == 0)
                {
                    sample.legacyPixelMinX = sample.legacyPixelMaxX = x;
                    sample.legacyPixelMinY = sample.legacyPixelMaxY = y;
                }
                else
                {
                    sample.legacyPixelMinX = Mathf.Min(sample.legacyPixelMinX, x);
                    sample.legacyPixelMinY = Mathf.Min(sample.legacyPixelMinY, y);
                    sample.legacyPixelMaxX = Mathf.Max(sample.legacyPixelMaxX, x);
                    sample.legacyPixelMaxY = Mathf.Max(sample.legacyPixelMaxY, y);
                }
            }
        }

        private static void Finish()
        {
            try
            {
                if (world != null)
                {
                    for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    {
                        LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                        if (candidate?.ObjectId == 998 &&
                            (candidate.SpawnerEntityIndex == 8 || candidate.SpawnerEntityIndex == 9))
                            candidate.FreeEntityLikeExe();
                    }
                    world.FlushPendingDestroyForDiagnostics();
                    for (int i = 0; i < children.Length; i++)
                    {
                        LF2Entity child = children[i];
                        report.samples[i].childReleased = child == null ||
                            world.FindEntityByRuntimeSlotForQuery(report.samples[i].childSlot) != child;
                    }
                    if (leftHost?.RegisteredWorldForSimulation == world)
                        world.Unregister(leftHost);
                    if (rightHost?.RegisteredWorldForSimulation == world)
                        world.Unregister(rightHost);
                    report.hostsReleased =
                        (leftHost == null || leftHost.RegisteredWorldForSimulation == null) &&
                        (rightHost == null || rightHost.RegisteredWorldForSimulation == null);
                    if (report.sceneHashBefore != null)
                    {
                        report.objectsAfter = world.ObjectCount;
                        report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                        report.borrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
                        report.sceneHashAfter = HashFile(ProjectPath(ScenePath));
                        if (report.sceneHashAfter != report.sceneHashBefore ||
                            report.objectsAfter != report.objectsBefore ||
                            report.slotsAfter != report.slotsBefore ||
                            report.borrowersAfter != report.borrowersBefore ||
                            !report.hostsReleased ||
                            !report.samples[0].childReleased ||
                            !report.samples[1].childReleased)
                        {
                            report.status = "FAIL_CLEANUP";
                            report.error += " Battle object/slot/pool or Scene cleanup mismatch.";
                        }
                    }
                }
                if (pauseCaptured && driver != null)
                {
                    driver.SetPaused(oldPaused);
                    report.pauseRestored = driver.IsPaused == oldPaused;
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL_CLEANUP";
                report.error += " Cleanup: " + error;
            }
            finally
            {
                string output = ResultPath(request.runId);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!File.Exists(output))
                    File.WriteAllText(output, JsonUtility.ToJson(report, true));
                request.requested = false;
                request.running = false;
                File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
                request = null;
                report = null;
                driver = null;
                world = null;
                leftHost = null;
                rightHost = null;
                children[0] = null;
                children[1] = null;
                phase = 0;
                stableTick = -1;
                stableUpdates = 0;
                expectedTick = -1;
                pauseCaptured = false;
                if (EditorApplication.isPlaying)
                    EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private static void WriteImmediateFailure(Request failed, string error)
        {
            string output = ResultPath(failed.runId);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!File.Exists(output))
                File.WriteAllText(output, JsonUtility.ToJson(new Report
                {
                    runId = failed.runId,
                    error = error,
                }, true));
            failed.requested = false;
            failed.running = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(failed));
        }

        private static bool ValidRunId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 80)
                return false;
            foreach (char ch in value)
                if (!char.IsLetterOrDigit(ch) && ch != '-' && ch != '_')
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
