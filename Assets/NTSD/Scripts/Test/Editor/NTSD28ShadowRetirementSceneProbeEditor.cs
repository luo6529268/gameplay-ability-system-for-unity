#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
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
    internal static class NTSD28ShadowRetirementSceneProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string OutputRoot = "artifacts/diagnostics/NTSD28-BATTLE-SHADOW-RETIREMENT-20261006/";
        private const string SessionKey = "NTSD.ShadowRetirement.Scene.01";
        private static Report report;
        private static SimulationTickDriver driver;
        private static LF2Entity rock;
        private static int stableUpdates;

        [Serializable]
        private sealed class Report
        {
            public string status = "RUNNING", phase = "STARTUP", startedUtc, error;
            public string sceneHashBefore, sceneHashAfter;
            public int startTick, elapsedTicks, p2HpBefore, p2HpAfter;
            public int peakFragmentShadows, finalFragmentShadows, finalSkillShadows;
            public int finalBelowScreenFragmentShadows;
            public int belowScreenFragments, rockSlot, remainingObjects = -1, remainingSlots = -1, remainingBorrowers = -1;
            public bool skillSpawned, skillHit, skillRemoved, rockRemoved, centralSubmitted, sceneClean, orderedShutdown;
        }

        static NTSD28ShadowRetirementSceneProbeEditor()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlay;
        }

        [MenuItem("NTSD/Validation/Battle Shadow Retirement Scene Probe")]
        private static void Start()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling &&
                !EditorApplication.isUpdating, "Editor must be idle.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == ScenePath && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires one saved original Battle Scene.");
            Require(!File.Exists(OutputRoot + "original-scene-01.json"), "Refuses to overwrite evidence.");
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
                if (report.phase == "STARTUP")
                {
                    if (++stableUpdates < 5) return;
                    Initialize();
                    report.phase = "TICKS";
                }
                if (report.phase == "TICKS")
                {
                    if (report.elapsedTicks == 20)
                    {
                        Require(rock != null && rock.Runtime.SlotIndex == report.rockSlot, "Stone disappeared before break.");
                        rock.Runtime.WeaponFlightCounter = -1;
                    }
                    int tick = driver.CurrentTickIndex + 1;
                    Require(driver.StepOneTick(new FrameInputSet(tick, new[]
                    {
                        new SimulationPlayerInput(0, SimulationInputButtons.None),
                        new SimulationPlayerInput(1, SimulationInputButtons.None)
                    }), ignorePaused: true, buildPresentation: true), "Directed tick rejected.");
                    report.elapsedTicks++;
                    Observe();
                    if (report.elapsedTicks == 25) CaptureMesh("original-scene-active-fragments.png");
                    if (report.elapsedTicks >= 300) { report.phase = "FINAL"; stableUpdates = 0; }
                    Save();
                    return;
                }
                if (++stableUpdates < 5) return;
                Observe();
                Require(report.centralSubmitted && report.skillSpawned && report.skillHit && report.skillRemoved &&
                    report.rockRemoved && report.peakFragmentShadows > 0 && report.belowScreenFragments > 0 &&
                    report.finalBelowScreenFragmentShadows == 0 && report.finalSkillShadows == 0,
                    "Reported lifecycle or shadow acceptance failed.");
                CaptureMesh("original-scene-after-retirement.png");
                report.status = "PASS";
                Exit();
            }
            catch (Exception exception)
            {
                report.status = "FAIL"; report.error = exception.ToString(); Exit();
            }
        }

        private static void Initialize()
        {
            var world = driver.World;
            var p1 = world.FindEntityByRuntimeSlotForQuery(0);
            var p2 = world.FindEntityByRuntimeSlotForQuery(1);
            Require(p1 != null && p2 != null, "Original scene requires P1/P2.");
            report.startTick = driver.CurrentTickIndex;
            Require(p1.TryApplyRuntimeIdentity(99, 278, true, out _), "Sage identity unavailable.");
            p1.Runtime.X = 500; p1.Runtime.Y = 0; p1.Runtime.Dir = "right";
            p1.Runtime.SyncIntegerPosition();
            p2.Runtime.X = 700; p2.Runtime.Y = 0; p2.Runtime.Z = p1.Runtime.Z;
            p2.Runtime.SyncIntegerPosition();
            foreach (var actor in new[] { p1, p2 })
            {
                actor.Runtime.SourceRulePositionInitialized = true;
                actor.Runtime.SourceRuleX = world.SpatialProjection.ViewToSourceX(actor.Runtime.X);
                actor.Runtime.SourceRuleZ = world.SpatialProjection.ViewToSourceZ(actor.Runtime.Z);
                actor.Runtime.SyncSourceRuleIntegerPosition();
            }
            report.p2HpBefore = p2.Health.HP;
            int slot = 50;
            while (slot < world.MaxRuntimeSlotsForServices && world.FindEntityByRuntimeSlotForQuery(slot) != null) slot++;
            Require(slot < world.MaxRuntimeSlotsForServices, "No free slot for stone.");
            report.rockSlot = slot;
            var task = new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = slot, preserveActionZero = true,
                nativeWeaponPieceSpawn = true, relationTeam = 1, dir = "right",
                useDirectRuntimePosition = true, directX = 1000, directY = -80, directZ = p1.Runtime.Z,
                opoint = new ObjectPoint { oid = 150, action = 0 }
            };
            rock = world.LogicEntityFactory.Create(task, out var failure);
            Require(rock != null, "Stone spawn: " + failure);
        }

        private static void Observe()
        {
            var world = driver.World;
            var entities = new List<LF2Entity>();
            world.GetAllEntities(entities);
            report.skillSpawned |= entities.Any(entity => entity.ObjectId == 518 && entity.Frame.N >= 350 && entity.Frame.N <= 356);
            var p2 = world.FindEntityByRuntimeSlotForQuery(1);
            report.p2HpAfter = p2?.Health.HP ?? -1;
            report.skillHit |= report.p2HpAfter < report.p2HpBefore;
            report.skillRemoved = report.skillSpawned && !entities.Any(entity => entity.ObjectId == 518 &&
                ((entity.Frame.N >= 350 && entity.Frame.N <= 356) || (entity.Frame.N >= 179 && entity.Frame.N <= 185)));
            report.rockRemoved = !ReferenceEquals(world.FindEntityByRuntimeSlotForQuery(report.rockSlot), rock);
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
            Require(plan.UsesCentralPixels && plan.CapturedFrame != null, "Central plan unavailable.");
            report.centralSubmitted |= BattleCentralRenderSystem.Diagnostics.SubmittedPixelsLastFrame;
            int fragments = 0, skills = 0, belowScreenShadows = 0;
            for (int index = 0; index < plan.CapturedFrame.CommandCount; index++)
            {
                BattleRenderCommand command = plan.CapturedFrame.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Shadow) continue;
                var owner = world.FindEntityByRuntimeSlotForQuery(command.RuntimeSlot);
                Require(owner != null, "Shadow command retained a removed slot.");
                if (owner.ObjectId == 999) fragments++;
                if (owner.ObjectId == 999 && owner.Runtime.YInt > 1500) belowScreenShadows++;
                if (owner.ObjectId == 518) skills++;
            }
            report.peakFragmentShadows = Math.Max(report.peakFragmentShadows, fragments);
            report.finalFragmentShadows = fragments;
            report.finalSkillShadows = skills;
            report.finalBelowScreenFragmentShadows = belowScreenShadows;
            report.belowScreenFragments = entities.Count(entity => entity.ObjectId == 999 && entity.Runtime.YInt > 1500);
        }

        private static void CaptureMesh(string name)
        {
            var plan = BattleCentralRenderSystem.PrepareFrame(driver.World);
            var backend = plan.Submission.Backend;
            var camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.orthographic, "World camera unavailable.");
            var target = new RenderTexture(1024, 576, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(1024, 576, TextureFormat.RGBA32, false, true);
            var buffer = new CommandBuffer();
            var properties = new MaterialPropertyBlock();
            var previous = RenderTexture.active;
            try
            {
                target.Create(); buffer.SetRenderTarget(target);
                buffer.ClearRenderTarget(false, true, new Color32(153, 123, 75, 255));
                float halfY = camera.orthographicSize, halfX = halfY * camera.aspect;
                Vector3 position = camera.transform.position;
                buffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(position.x - halfX,
                    position.x + halfX, position.y - halfY, position.y + halfY, -100, 100));
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    var segment = backend.GetSegment(index); properties.Clear();
                    properties.SetTexture(segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ?
                        "_MainTexArray" : "_MainTex", segment.Texture);
                    buffer.DrawMesh(backend.GetChunkMesh(segment.ChunkIndex), Matrix4x4.identity,
                        segment.Material, segment.SubMeshIndex, 0, properties);
                }
                Graphics.ExecuteCommandBuffer(buffer);
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, 1024, 576), 0, 0); readback.Apply();
                using var stream = new FileStream(OutputRoot + name, FileMode.CreateNew);
                byte[] bytes = readback.EncodeToPNG(); stream.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                RenderTexture.active = previous; buffer.Release(); target.Release();
                UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(readback);
            }
        }

        private static void OnPlay(PlayModeStateChange state)
        {
            Restore(); if (report == null) return;
            if (state == PlayModeStateChange.ExitingPlayMode && report.phase != "EXITING")
            { report.status = "FAIL"; report.error = "Play stopped before completion."; report.phase = "EXITING"; Save(); }
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING") Finish();
        }

        private static void Exit()
        {
            if (driver?.World != null)
            {
                var shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (var bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap)) continue;
                        bootstrap.DisablePresentation(); mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }
                report.remainingObjects = shutdown.RemainingWorldObjects;
                report.remainingSlots = shutdown.RemainingRuntimeSlots;
                report.remainingBorrowers = shutdown.RemainingPoolBorrowers;
                report.orderedShutdown = shutdown.IsComplete && driver.World == null;
                if (!report.orderedShutdown)
                { report.status = "FAIL"; report.error += " Shutdown: " + shutdown.FailureReason; report.phase = "SHUTDOWN_FAILED"; Save(); return; }
            }
            report.phase = "EXITING"; Save(); EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            Scene scene = SceneManager.GetActiveScene(); report.sceneHashAfter = Hash();
            report.sceneClean = scene.path == ScenePath && !scene.isDirty && report.sceneHashBefore == report.sceneHashAfter;
            if (!report.sceneClean) { report.status = "FAIL"; report.error += " Scene changed."; }
            using var stream = new FileStream(OutputRoot + "original-scene-01.json", FileMode.CreateNew);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(report, true)); stream.Write(bytes, 0, bytes.Length);
            SessionState.EraseString(SessionKey); report = null; driver = null; rock = null; stableUpdates = 0;
        }

        private static void Require(bool condition, string error) { if (!condition) throw new InvalidOperationException(error); }
        private static string Hash()
        { using var sha = SHA256.Create(); using var file = File.OpenRead(ScenePath); return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", string.Empty); }
        private static void Save() => SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
        private static void Restore()
        { if (report != null) return; string saved = SessionState.GetString(SessionKey, string.Empty); if (!string.IsNullOrEmpty(saved)) report = JsonUtility.FromJson<Report>(saved); }
    }
}
#endif
