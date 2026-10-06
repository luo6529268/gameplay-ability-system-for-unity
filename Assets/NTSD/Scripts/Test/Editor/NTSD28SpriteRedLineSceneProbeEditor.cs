#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28SpriteRedLineSceneProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string OutputRoot = "artifacts/diagnostics/NTSD28-BATTLE-SPRITE-RED-LINE-20261006/";
        private const string ResultPath = OutputRoot + "original-battle-scene-01.json";
        private const string SessionKey = "NTSD.RedLine.Scene.01";
        private static Report report;
        private static SimulationTickDriver driver;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Report
        {
            public string status = "RUNNING";
            public string phase = "STARTUP";
            public string startedUtc;
            public string error;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string pipeline;
            public string bindingMode;
            public int visualId;
            public int pic;
            public int tick;
            public int gpuPlacements;
            public int maxGutter;
            public int meshStride;
            public bool centralSubmitted;
            public bool orderedShutdown;
            public bool sceneClean;
            public int remainingObjects = -1;
            public int remainingSlots = -1;
            public int remainingBorrowers = -1;
        }

        static NTSD28SpriteRedLineSceneProbeEditor()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlay;
        }

        [MenuItem("NTSD/Validation/Character Red Line Scene Probe")]
        private static void Start()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling &&
                !EditorApplication.isUpdating, "Editor must be idle.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == ScenePath && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires one saved original Battle Scene.");
            Require(!File.Exists(ResultPath), "Refuses to overwrite evidence.");
            report = new Report { startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = Hash() };
            Save();
            EditorApplication.EnterPlaymode();
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            Restore();
            if (report == null || report.phase == "SHUTDOWN_FAILED") return;
            try
            {
                Require((DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime()).TotalSeconds < 600,
                    "Probe deadline exceeded.");
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                if (!EditorApplication.isPlaying) return;
                driver = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                    .FirstOrDefault(value => value != null && value.isActiveAndEnabled && !EditorUtility.IsPersistent(value));
                if (driver?.World == null || driver.CurrentTickIndex < 5) return;
                if (!driver.IsPaused) { driver.SetPaused(true); return; }
                if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
                if (stableTick != driver.CurrentTickIndex) { stableTick = driver.CurrentTickIndex; stableUpdates = 0; return; }
                if (++stableUpdates < 5) return;
                BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(driver.World);
                if (!plan.UsesCentralPixels || plan.CapturedFrame == null) return;
                Require(BattleCentralRenderSystem.Diagnostics.SubmittedPixelsLastFrame, "Original central pixels were not submitted.");
                report.centralSubmitted = true;
                report.tick = driver.CurrentTickIndex;
                report.pipeline = GraphicsSettings.currentRenderPipeline?.GetType().FullName;
                BattlePresentationFrame frame = plan.CapturedFrame;
                int commandIndex = -1;
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand candidate = frame.GetCommand(index);
                    if (candidate.Type == BattleRenderCommandType.Entity && candidate.VisualDataId == 2 &&
                        candidate.EffectivePic >= 0 && candidate.EffectivePic <= 3)
                    { commandIndex = index; break; }
                }
                Require(commandIndex >= 0, "Original scene lacks standing Naruto; no synthetic replacement allowed.");
                BattleRenderCommand command = frame.GetCommand(commandIndex);
                report.visualId = command.VisualDataId;
                report.pic = command.EffectivePic;
                CaptureActualMesh(plan.Submission.Backend, commandIndex, command.Position);
                CaptureGameView();
                Require(report.maxGutter == 0 && report.gpuPlacements == 21, "Actual central mesh sampled the external grid.");
                report.status = "PASS";
                Exit();
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.error = exception.ToString();
                Exit();
            }
        }

        private static void CaptureActualMesh(BattleDynamicMeshBackend backend, int commandIndex, Vector3 position)
        {
            var target = new RenderTexture(128, 160, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(128, 160, TextureFormat.RGBA32, false, true);
            var buffer = new CommandBuffer();
            var properties = new MaterialPropertyBlock();
            RenderTexture previous = RenderTexture.active;
            try
            {
                target.Create();
                for (int offset = 0; offset <= 20; offset++)
                {
                    buffer.Clear();
                    buffer.SetRenderTarget(target);
                    buffer.ClearRenderTarget(false, true, new Color32(32, 64, 96, 255));
                    float left = position.x - 64 * NTSDRenderSpace.UnitsPerPixelX;
                    float bottom = position.y - (16 + offset * 0.05f) * NTSDRenderSpace.UnitsPerPixelY;
                    buffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(left,
                        left + 128 * NTSDRenderSpace.UnitsPerPixelX, bottom,
                        bottom + 160 * NTSDRenderSpace.UnitsPerPixelY, -100, 100));
                    for (int index = 0; index < backend.SegmentCount; index++)
                    {
                        BattleCentralRenderSegment segment = backend.GetSegment(index);
                        properties.Clear();
                        properties.SetTexture(segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ?
                            "_MainTexArray" : "_MainTex", segment.Texture);
                        if (commandIndex >= segment.FirstCommandIndex && commandIndex < segment.FirstCommandIndex + segment.CommandCount)
                        {
                            report.bindingMode = segment.BindingMode.ToString();
                            report.meshStride = backend.GetChunkMesh(segment.ChunkIndex).GetVertexBufferStride(0);
                        }
                        buffer.DrawMesh(backend.GetChunkMesh(segment.ChunkIndex), Matrix4x4.identity,
                            segment.Material, segment.SubMeshIndex, 0, properties);
                    }
                    Graphics.ExecuteCommandBuffer(buffer);
                    RenderTexture.active = target;
                    readback.ReadPixels(new Rect(0, 0, 128, 160), 0, 0);
                    readback.Apply();
                    int gutter = 0;
                    int content = 0;
                    foreach (Color32 pixel in readback.GetPixels32())
                    {
                        if (pixel.a > 240 && Math.Abs(pixel.r - 201) <= 4 && Math.Abs(pixel.g - 47) <= 4 && pixel.b < 4) gutter++;
                        if (pixel.r > 200 && pixel.g > 180 && pixel.b < 80) content++;
                    }
                    Require(content > 20, "Actual Naruto mesh was not visible in GPU output.");
                    report.gpuPlacements++;
                    report.maxGutter = Math.Max(report.maxGutter, gutter);
                    if (offset == 10) SaveBytes(OutputRoot + "original-central-mesh-half-pixel.png", readback.EncodeToPNG());
                }
            }
            finally
            {
                RenderTexture.active = previous;
                buffer.Release();
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(readback);
            }
        }

        private static void CaptureGameView()
        {
            var target = new RenderTexture(Screen.width, Screen.height, 0);
            var readback = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            try
            {
                target.Create();
                ScreenCapture.CaptureScreenshotIntoRenderTexture(target);
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                readback.Apply();
                SaveBytes(OutputRoot + "original-battle-game.png", readback.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(readback);
            }
        }

        private static void OnPlay(PlayModeStateChange state)
        {
            Restore();
            if (report == null) return;
            if (state == PlayModeStateChange.ExitingPlayMode && report.phase != "EXITING")
            {
                report.status = "FAIL";
                report.error = "Play stopped before probe completion.";
                report.phase = "EXITING";
                Save();
            }
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING") Finish();
        }

        private static void Exit()
        {
            if (driver?.World != null)
            {
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap)) continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }
                report.remainingObjects = shutdown.RemainingWorldObjects;
                report.remainingSlots = shutdown.RemainingRuntimeSlots;
                report.remainingBorrowers = shutdown.RemainingPoolBorrowers;
                report.orderedShutdown = shutdown.IsComplete && driver.World == null;
                if (!report.orderedShutdown)
                {
                    report.status = "FAIL";
                    report.error += " Ordered shutdown failed: " + shutdown.FailureReason;
                    report.phase = "SHUTDOWN_FAILED";
                    Save();
                    return;
                }
            }
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            Scene scene = SceneManager.GetActiveScene();
            report.sceneHashAfter = Hash();
            report.sceneClean = scene.path == ScenePath && !scene.isDirty && report.sceneHashBefore == report.sceneHashAfter;
            if (!report.sceneClean) { report.status = "FAIL"; report.error += " Scene changed during Play."; }
            SaveBytes(ResultPath, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(report, true)));
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            stableTick = -1;
            stableUpdates = 0;
        }

        private static void Require(bool condition, string error)
        {
            if (!condition) throw new InvalidOperationException(error);
        }

        private static void SaveBytes(string path, byte[] bytes)
        {
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string Hash()
        {
            using var sha = SHA256.Create();
            using var file = File.OpenRead(ScenePath);
            return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", string.Empty);
        }

        private static void Save() => SessionState.SetString(SessionKey, JsonUtility.ToJson(report));

        private static void Restore()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(saved)) report = JsonUtility.FromJson<Report>(saved);
        }
    }
}
#endif
