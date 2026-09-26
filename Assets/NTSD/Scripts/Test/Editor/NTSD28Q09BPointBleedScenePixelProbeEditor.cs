#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
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
    internal static class NTSD28Q09BPointBleedScenePixelProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q09_BPointBleedScenePixel.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-BPOINT-BLEED-SCENE-PIXEL-001";
        private const int CaptureWidth = 1920;
        private static int phase;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static bool savedPaused;
        private static bool pauseCaptured;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character fixture;
        private static Camera camera;
        private static Color32[] highPixels;
        private static int highTick;
        private static int lowTick;
        private static int highGeneration;
        private static int lowGeneration;
        private static int markIndex;
        private static BattleRenderCommand markCommand;
        private static Report report;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public long startedUtcTicks;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string scope;
            public string runId;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public int sourceOid;
            public int sourceFrame;
            public int sourceBPointCount;
            public int highHp;
            public int lowHp;
            public int highCommandCount;
            public int lowCommandCount;
            public int markCommandIndex;
            public int bodyCommandIndex;
            public int markSlot;
            public int markWidth;
            public int markHeight;
            public int changedRedPixels;
            public int projectedPixels;
            public int projectedXMin;
            public int projectedXMax;
            public int projectedYMin;
            public int projectedYMax;
            public int sampleX;
            public int sampleY;
            public string highImage;
            public string lowImage;
            public int objectsBefore;
            public int objectsAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int borrowersBefore;
            public int borrowersAfter;
            public bool cameraStateRestored;
            public bool fixtureUnregistered;
        }

        static NTSD28Q09BPointBleedScenePixelProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            string path = ProjectPath(RequestPath);
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(path))
                return;
            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || (!request.requested && !request.running))
                return;
            if (request.requested && !request.running)
            {
                Scene scene = SceneManager.GetActiveScene();
                if (EditorApplication.isPlayingOrWillChangePlaymode ||
                    !ValidRunId(request.runId) || scene.path != ScenePath ||
                    scene.isDirty || File.Exists(ResultPath(request.runId)))
                {
                    Finish(request, new Report
                    {
                        status = "FAIL",
                        error = "Original clean Battle Scene, unique runId and idle Editor are required.",
                    });
                    return;
                }
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(path, JsonUtility.ToJson(request));
                EditorApplication.EnterPlaymode();
                return;
            }
            if (!request.running || !EditorApplication.isPlaying)
                return;

            if (report == null)
            {
                report = new Report
                {
                    status = "FAIL",
                    runId = request.runId,
                    sourceOid = 9,
                    scope = "Original Battle Scene formal Ita, controlled CentralOnly high/low-HP camera A/B. Natural input, Legacy and formal EXE pixels remain open.",
                };
            }
            try
            {
                Require(DateTime.UtcNow - new DateTime(request.startedUtcTicks,
                    DateTimeKind.Utc) < TimeSpan.FromMinutes(3),
                    "Scene Play timed out.");
                if (phase == 0)
                    Prepare();
                else if (phase == 1)
                    ObserveHigh(request);
                else if (phase == 2)
                    ObserveLow(request);
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
                CleanupAndFinish(request);
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
            Require(report.contentRoot == FormalRoot,
                "Formal content root is not selected.");
            Require(driver.PresentationBackendMode == BattlePresentationBackendMode.CentralOnly,
                "CentralOnly backend is required.");
            Require(CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                "Formal content has not been published.");
            camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.isActiveAndEnabled,
                "Original world camera is unavailable.");
            if (!pauseCaptured)
            {
                savedPaused = driver.IsPaused;
                pauseCaptured = true;
                driver.SetPaused(true);
                return;
            }
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
            LF2CharacterDataWrapper data = world.RuntimeCharacterConfigs.Resolve(9);
            Require(data?.characterData != null, "Formal Ita OID9 definition is unavailable.");
            int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
            Require(slot >= 50, "No free runtime slot for Ita fixture.");
            fixture = new LF2Character();
            fixture.ModuleInitialize();
            fixture.ObjectId = 9;
            fixture.Name = "Q09BPointIta";
            fixture.FrameCache.Load(data);
            fixture.SetRequiredRuntimeSlot(slot);
            world.Register(fixture);
            fixture.ImmediateFrame(0);
            fixture.Initialize(500, 500);
            fixture.AiControlled = false;
            fixture.Team = 1;
            fixture.RelationTeam = 1;
            fixture.Runtime.SetPosition(1100, 0, world.Runtime.Stage.ZMin + 50);
            fixture.Runtime.SetVelocity(0, 0, 0);
            fixture.Runtime.SyncIntegerPosition();
            fixture.RefreshRuntimeSnapshot();
            report.sourceFrame = fixture.Frame.N;
            report.sourceBPointCount = fixture.Frame.D?.BloodPoints?.Count ?? 0;
            Require(report.sourceFrame == 0 && report.sourceBPointCount > 0,
                "Formal Ita frame0 has no bpoint.");
            report.highHp = fixture.Runtime.HP;
            highTick = driver.CurrentTickIndex + 100;
            highGeneration = world.CurrentPixelFramePlan.Generation;
            world.RenderDispatchAll(highTick);
            phase = 1;
        }

        private static void ObserveHigh(Request request)
        {
            if (!ReadyPlan(highTick, highGeneration,
                out BattlePresentationFrame frame))
                return;
            int slot = fixture.Runtime.SlotIndex;
            report.highCommandCount = CountCommands(frame, slot,
                BattleRenderCommandType.BleedMark, out _);
            Require(report.highCommandCount == 0,
                "High-HP Ita unexpectedly emitted a bleed mark.");
            highPixels = Capture(request.runId, "high", out string highImage);
            report.highImage = highImage;
            fixture.Health.HP = 166;
            fixture.RefreshRuntimeSnapshot();
            report.lowHp = fixture.Runtime.HP;
            Require(report.lowHp <= fixture.Runtime.HP3 / 3,
                "Controlled HP did not cross the base-HP threshold.");
            lowTick = driver.CurrentTickIndex + 101;
            lowGeneration = world.CurrentPixelFramePlan.Generation;
            world.RenderDispatchAll(lowTick);
            phase = 2;
        }

        private static void ObserveLow(Request request)
        {
            if (!ReadyPlan(lowTick, lowGeneration,
                out BattlePresentationFrame frame))
                return;
            int slot = fixture.Runtime.SlotIndex;
            report.lowCommandCount = CountCommands(frame, slot,
                BattleRenderCommandType.BleedMark, out markIndex);
            Require(report.lowCommandCount == report.sourceBPointCount,
                "Low-HP mark count differs from formal bpoints.");
            report.bodyCommandIndex = FindCommandIndex(frame, slot,
                BattleRenderCommandType.Entity);
            Require(report.bodyCommandIndex >= 0 && markIndex > report.bodyCommandIndex,
                "Bleed mark must follow the visible body command.");
            markCommand = frame.GetCommand(markIndex);
            report.markCommandIndex = markIndex;
            report.markSlot = markCommand.RuntimeSlot;
            report.markWidth = Mathf.RoundToInt(markCommand.Size.x);
            report.markHeight = Mathf.RoundToInt(markCommand.Size.y);
            Require(report.markWidth == 1 && report.markHeight == 3,
                "Current formal bpoint defaults are not 1x3.");
            Color32[] lowPixels = Capture(request.runId, "low", out string lowImage);
            report.lowImage = lowImage;
            ComparePixels(highPixels, lowPixels);
            report.status = report.changedRedPixels > 0
                ? "PASS_CONTROLLED_CENTRAL_PIXEL"
                : "PIXEL_OWNERSHIP_UNPROVEN";
            CleanupAndFinish(request);
        }

        private static bool ReadyPlan(int tick, int generation,
            out BattlePresentationFrame frame)
        {
            frame = null;
            Require(driver.IsPaused && ReferenceEquals(driver.World, world) &&
                driver.CurrentTickIndex == stableTick &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Paused production World changed during controlled capture.");
            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            if (!plan.IsValid || plan.Generation == generation ||
                plan.SimulationTick != tick || plan.IsStale ||
                plan.Owner != BattlePixelFrameOwner.Central ||
                plan.Submission == null || plan.CapturedFrame == null ||
                !plan.CapturedFrame.CommandsMaterialized)
                return false;
            frame = plan.CapturedFrame;
            return true;
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

        private static int FindCommandIndex(BattlePresentationFrame frame,
            int slot, BattleRenderCommandType type)
        {
            CountCommands(frame, slot, type, out int index);
            return index;
        }

        private static Color32[] Capture(string runId, string suffix,
            out string relativePath)
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
            RenderTexture previousActive = RenderTexture.active;
            Texture2D readback = null;
            relativePath = null;
            try
            {
                target.Create();
                camera.cullingMask = 0;
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
                relativePath = ResultRoot + "/" + runId + "-" + suffix + ".png";
                string path = ProjectPath(relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, readback.EncodeToPNG());
                return readback.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(camera);
                report.cameraStateRestored = saved.Matches(camera) &&
                    (suffix == "high" || report.cameraStateRestored);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void ComparePixels(Color32[] high, Color32[] low)
        {
            Require(high != null && low != null && high.Length == low.Length &&
                report.cameraStateRestored,
                "Camera A/B capture was incomplete.");
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
            report.projectedPixels = (x1 - x0) * (y1 - y0);
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
                    if (report.changedRedPixels == 1)
                    {
                        report.sampleX = x;
                        report.sampleY = y;
                    }
                }
            }
        }

        private static void CleanupAndFinish(Request request)
        {
            try
            {
                if (fixture?.RegisteredWorldForSimulation == world)
                    world.Unregister(fixture);
                report.fixtureUnregistered = fixture == null ||
                    fixture.RegisteredWorldForSimulation == null;
                if (world != null && report.sceneHashBefore != null)
                {
                    report.objectsAfter = world.ObjectCount;
                    report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                    report.borrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
                    report.sceneHashAfter = HashFile(ProjectPath(ScenePath));
                    if (!report.fixtureUnregistered ||
                        report.objectsAfter != report.objectsBefore ||
                        report.slotsAfter != report.slotsBefore ||
                        report.borrowersAfter != report.borrowersBefore ||
                        report.sceneHashAfter != report.sceneHashBefore)
                    {
                        report.status = "FAIL";
                        report.error += " Fixture cleanup or Scene SHA mismatch.";
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
                Finish(request, report);
            }
        }

        private static void Finish(Request request, Report result)
        {
            if (ValidRunId(request.runId))
            {
                string output = ResultPath(request.runId);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!File.Exists(output))
                    File.WriteAllText(output, JsonUtility.ToJson(result, true));
            }
            request.requested = false;
            request.running = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
            phase = 0;
            stableTick = -1;
            stableUpdates = 0;
            pauseCaptured = false;
            driver = null;
            world = null;
            fixture = null;
            camera = null;
            highPixels = null;
            report = null;
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
