#if UNITY_EDITOR
using System;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q07Oid32EntityPixelProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_Oid32EntityPixel.request.json";
        private const string Oid32ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-OID32-UNITY-ENTITY-PIXEL-001";
        private const string Oid30ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-OID30-UNITY-BOUNDARY-PIXEL-001";
        private static string ResultRoot => targetOid30 ? Oid30ResultRoot : Oid32ResultRoot;
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const int CaptureWidth = 960;
        private static bool targetOid30;
        private static int Oid => targetOid30 ? 30 : 32;
        private static int Frame => targetOid30 ? 31 : 95;
        private static int Pic => targetOid30 ? 81 : 64;

        private static DateTime startedAtUtc;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int expectedTick = -1;
        private static bool pauseRequested;
        private static bool baselinePaused;
        private static LF2Character fixture;
        private static SimulationWorld world;
        private static SimulationTickDriver driver;
        private static LF2ObjectPool pool;
        private static Report report;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool postExitPending;
            public string runId;
            public string target;
        }

        [Serializable]
        private sealed class PostExitReport
        {
            public string runId;
            public bool isPlaying;
            public int derivedTextureCount;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public string scope;
            public string target;
            public string semanticFingerprint;
            public bool catalogFound;
            public string catalogSourcePath;
            public int catalogTextureWidth;
            public int catalogTextureHeight;
            public bool catalogCentralBindingValid;
            public bool catalogLegacySpritePresent;
            public int tick;
            public int objectCountBefore;
            public int objectCountAfter;
            public int claimedSlotsBefore;
            public int claimedSlotsAfter;
            public int objectPoolBefore;
            public int objectPoolAfter;
            public int logicPoolBefore;
            public int logicPoolAfter;
            public int stableId;
            public int runtimeSlot;
            public int frame;
            public int renderPic;
            public int commandCount;
            public int entityCommandCount;
            public string oid32CommandSummary;
            public string targetCommandSummary;
            public int planTick;
            public int roiWidth;
            public int roiHeight;
            public int roiWhitePixels;
            public int roiNonClearPixels;
            public string pngPath;
            public string cleanupError;
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            string path = ProjectPath(RequestPath);
            if (!File.Exists(path))
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
            if (request == null || !request.requested)
            {
                if (request?.postExitPending == true &&
                    !EditorApplication.isPlayingOrWillChangePlaymode)
                    CapturePostExit(request);
                Reset();
                return;
            }
            if (string.IsNullOrEmpty(request.runId) ||
                !System.Linq.Enumerable.All(request.runId,
                    value => char.IsLetterOrDigit(value) || value == '-'))
            {
                Finish(request, "FAIL", "Invalid runId.");
                return;
            }
            if (!string.IsNullOrEmpty(request.target) &&
                request.target != "oid32-frame95" &&
                request.target != "oid30-frame31")
            {
                Finish(request, "FAIL", "Unsupported boundary pixel target.");
                return;
            }
            targetOid30 = request.target == "oid30-frame31";
            if (startedAtUtc == default)
                startedAtUtc = DateTime.UtcNow;
            if (DateTime.UtcNow - startedAtUtc > TimeSpan.FromMinutes(5))
            {
                Finish(request, "FAIL", "Timed out before controlled pixel capture.");
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }
            try
            {
                Observe(request);
            }
            catch (Exception exception)
            {
                Finish(request, "FAIL", exception.ToString());
            }
        }

        private static void Observe(Request request)
        {
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (driver == null || world == null || driver.CurrentTickIndex < 5)
                return;
            if (GameConfig.Instance?.BattleContentRuntimeRoot != FormalRoot)
                throw new InvalidOperationException("The formal content root is not selected.");
            CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
            if (manager?.PublishedLoganContentIdentity == null)
                return;
            if (string.IsNullOrEmpty(manager.PublishedLoganContentIdentity.SemanticFingerprint))
                throw new InvalidOperationException("No published formal content fingerprint.");
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly)
                throw new InvalidOperationException("CentralOnly is not active.");
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            if (!pauseRequested)
            {
                baselinePaused = driver.IsPaused;
                driver.SetPaused(true);
                pauseRequested = true;
                return;
            }
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

            if (report == null)
            {
                pool = LF2ObjectPool.Instance;
                report = new Report
                {
                    scope = "Controlled OID" + Oid + "/frame" + Frame +
                            " Unity production entity and central camera, not natural input or root-EXE GPU parity",
                    target = targetOid30 ? "oid30-frame31" : "oid32-frame95",
                    semanticFingerprint = manager.PublishedLoganContentIdentity.SemanticFingerprint,
                    tick = driver.CurrentTickIndex,
                    objectCountBefore = world.ObjectCount,
                    claimedSlotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics,
                    objectPoolBefore = pool.ActiveObjectCountForAcceptance,
                    logicPoolBefore = LF2ReferencePool.Instance.ActiveCount,
                };
                report.catalogFound = manager.TryGetSpriteEntry(Oid, Pic,
                    out BattleSpriteEntry catalogEntry);
                if (catalogEntry != null)
                {
                    report.catalogSourcePath = catalogEntry.SourceSheetPath;
                    report.catalogTextureWidth = catalogEntry.SharedTexture?.width ?? 0;
                    report.catalogTextureHeight = catalogEntry.SharedTexture?.height ?? 0;
                    report.catalogCentralBindingValid = catalogEntry.CentralBinding.IsValid;
                    report.catalogLegacySpritePresent = catalogEntry.LegacySprite != null;
                }
                if (targetOid30 && (!report.catalogFound ||
                    report.catalogTextureWidth != 79 || report.catalogTextureHeight != 79 ||
                    !report.catalogCentralBindingValid ||
                    !report.catalogLegacySpritePresent ||
                    string.IsNullOrEmpty(report.catalogSourcePath) ||
                    report.catalogSourcePath.IndexOf("NTSD28NativeClampCells",
                        StringComparison.OrdinalIgnoreCase) < 0))
                    throw new InvalidOperationException("OID30/pic81 formal clamped catalog binding is missing.");
                Spawn(manager);
                expectedTick = driver.CurrentTickIndex + 1;
                bool accepted = driver.DedicatedSimulationWorkerActiveForDiagnostics
                    ? driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(true)
                    : driver.StepOneTick(ignorePaused: true, buildPresentation: true);
                if (!accepted)
                    throw new InvalidOperationException("Production Driver rejected the fixture activation tick.");
                return;
            }

            if (driver.CurrentTickIndex < expectedTick)
                return;
            report.tick = driver.CurrentTickIndex;

            world.RenderDispatchAll(driver.CurrentTickIndex, true);
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
            if (!plan.IsValid || plan.IsStale || plan.Owner != BattlePixelFrameOwner.Central ||
                plan.CapturedFrame?.CommandsMaterialized != true || plan.Submission == null)
                return;
            Camera camera = NTSDRenderSpace.WorldCamera;
            if (camera == null || !BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    camera, CameraRenderType.Base, camera.cameraType, true,
                    out BattleCentralSubmission.BattleCentralSubmissionLease lease))
                return;

            BattleRenderCommand command = default;
            using (lease)
            {
                BattlePresentationFrame frame = plan.CapturedFrame;
                report.commandCount = frame.CommandCount;
                report.planTick = plan.SimulationTick;
                report.entityCommandCount = 0;
                report.oid32CommandSummary = string.Empty;
                report.targetCommandSummary = string.Empty;
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand current = frame.GetCommand(index);
                    if (current.Type == BattleRenderCommandType.Entity &&
                        current.VisualDataId == Oid)
                    {
                        string summary = current.StableId + ":" +
                                         current.EffectivePic + ";";
                        report.targetCommandSummary += summary;
                        if (!targetOid30)
                            report.oid32CommandSummary += summary;
                    }
                    if (current.Type != BattleRenderCommandType.Entity ||
                        current.StableId != fixture.Runtime.StableId ||
                        current.VisualDataId != Oid || current.EffectivePic != Pic)
                        continue;
                    command = current;
                    report.entityCommandCount++;
                }
            }
            if (report.entityCommandCount != 1)
                throw new InvalidOperationException("Expected exactly one OID" + Oid +
                                                    "/pic" + Pic + " entity command, found " +
                                                    report.entityCommandCount + ".");
            Capture(camera, command, request.runId);
            Finish(request,
                report.roiWhitePixels > 0 ? "PASS" : "FAIL",
                report.roiWhitePixels > 0
                    ? "Controlled OID" + Oid + "/pic" + Pic +
                      " central command produced white camera pixels."
                    : "OID" + Oid + "/pic" + Pic +
                      " command exists, but the projected ROI has no white pixels.");
        }

        private static void Spawn(CharacterAnimtorManager manager)
        {
            int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 200);
            if (slot < 0)
                throw new InvalidOperationException("No free runtime slot for OID" + Oid + " fixture.");
            GameObject entityObject = pool.Get(out LF2ObjectRenderer renderer);
            fixture = LF2ReferencePool.Instance.Get(LF2ObjectType.Character, Oid) as LF2Character;
            if (entityObject == null || renderer == null || fixture == null)
                throw new InvalidOperationException("Production pools could not create OID" + Oid + ".");
            fixture.Controller.SetInputID(7032);
            fixture.InjectDependencies(entityObject.transform, renderer.transform,
                "Q07_OID" + Oid + "_Pixel");
            fixture.ModuleInitialize();
            fixture.SetRequiredRuntimeSlot(slot);
            renderer.SetLogicObject(fixture, null);
            fixture.ModuleBind(manager.GetCharacterConfig(Oid), Oid, world);
            fixture.Initialize(100, 100);
            fixture.AiControlled = false;
            fixture.ImmediateFrame(Frame);
            fixture.Runtime.SetPosition(600, 0, 270);
            fixture.Runtime.SyncIntegerPosition();
            fixture.RefreshRuntimeSnapshot();
            report.stableId = fixture.Runtime.StableId;
            report.runtimeSlot = fixture.Runtime.SlotIndex;
            report.frame = fixture.Frame.N;
            report.renderPic = fixture.GetRenderPicIndex();
            if (report.frame != Frame || report.renderPic != Pic)
                throw new InvalidOperationException("OID" + Oid + " fixture did not reach frame" +
                                                    Frame + "/pic" + Pic + ".");
        }

        private static void Capture(Camera camera, BattleRenderCommand command, string runId)
        {
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            float widthWorld = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                               NTSDRenderSpace.BattleVisualScale;
            float heightWorld = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * widthWorld;
            float bottom = command.Position.y - command.Pivot.y * heightWorld;
            Vector3 lower = camera.WorldToViewportPoint(new Vector3(left, bottom, command.Position.z));
            Vector3 upper = camera.WorldToViewportPoint(new Vector3(
                left + widthWorld, bottom + heightWorld, command.Position.z));
            int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.y, upper.y) * height), 0, height);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.y, upper.y) * height), 0, height);
            report.roiWidth = x1 - x0;
            report.roiHeight = y1 - y0;
            if (report.roiWidth <= 0 || report.roiHeight <= 0)
                throw new InvalidOperationException("OID" + Oid +
                                                    " projected ROI is outside the camera.");

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
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                readback = new Texture2D(CaptureWidth, height, TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, height), 0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++)
                    {
                        Color32 pixel = pixels[y * CaptureWidth + x];
                        if (pixel.r == 255 && pixel.g == 255 && pixel.b == 255)
                            report.roiWhitePixels++;
                        if (pixel.r != 0 || pixel.g != 0 || pixel.b != 0)
                            report.roiNonClearPixels++;
                    }
                report.pngPath = ResultRoot + "/" + runId + ".png";
                string output = ProjectPath(report.pngPath);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, readback.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(camera);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void Finish(Request request, string status, string message)
        {
            report = report ?? new Report();
            report.status = status;
            report.message = message;
            if (fixture != null)
            {
                try
                {
                    fixture.FreeEntityLikeExe();
                    world.FlushPendingDestroyForDiagnostics();
                    world.RenderDispatchAll(driver.CurrentTickIndex, true);
                    BattleCentralRenderSystem.PrepareFrame(world);
                }
                catch (Exception exception)
                {
                    report.cleanupError = exception.ToString();
                    report.status = "FAIL";
                }
            }
            if (world != null && pool != null && report.scope != null)
            {
                report.objectCountAfter = world.ObjectCount;
                report.claimedSlotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                report.objectPoolAfter = pool?.ActiveObjectCountForAcceptance ?? -1;
                report.logicPoolAfter = LF2ReferencePool.Instance?.ActiveCount ?? -1;
                if (report.objectCountAfter != report.objectCountBefore ||
                    report.claimedSlotsAfter != report.claimedSlotsBefore ||
                    report.objectPoolAfter != report.objectPoolBefore ||
                    report.logicPoolAfter != report.logicPoolBefore)
                {
                    report.status = "FAIL";
                    report.cleanupError += " Baseline world/pool counts were not restored.";
                }
            }
            if (pauseRequested && driver != null && EditorApplication.isPlaying)
                driver.SetPaused(baselinePaused);
            string output = ProjectPath(ResultRoot + "/" + request.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!File.Exists(output))
                File.WriteAllText(output, JsonUtility.ToJson(report, true));
            request.requested = false;
            request.postExitPending = true;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
            Reset();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Reset()
        {
            startedAtUtc = default;
            stableTick = -1;
            stableUpdates = 0;
            expectedTick = -1;
            pauseRequested = false;
            baselinePaused = false;
            fixture = null;
            world = null;
            driver = null;
            pool = null;
            report = null;
            targetOid30 = false;
        }

        private static void CapturePostExit(Request request)
        {
            int count = 0;
            Texture2D[] textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            for (int index = 0; index < textures.Length; index++)
                if (textures[index] != null &&
                    textures[index].name.StartsWith("native_clamp_", StringComparison.Ordinal))
                    count++;
            var result = new PostExitReport
            {
                runId = request.runId,
                isPlaying = EditorApplication.isPlaying,
                derivedTextureCount = count,
            };
            string resultRoot = request.target == "oid30-frame31"
                ? Oid30ResultRoot : Oid32ResultRoot;
            string output = ProjectPath(resultRoot + "/" + request.runId + "-post-exit.json");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!File.Exists(output))
                File.WriteAllText(output, JsonUtility.ToJson(result, true));
            request.postExitPending = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
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
    }
}
#endif
