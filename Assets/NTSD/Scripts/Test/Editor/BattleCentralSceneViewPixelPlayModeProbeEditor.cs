#if UNITY_EDITOR
using System;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Captures isolated central pixels through Unity's real Play Mode SceneView camera.
    /// Alignment contract: R8-SPRITEMAP-007.
    /// </summary>
    public static class BattleCentralSceneViewPixelPlayModeProbeEditor
    {
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01D_07_SceneViewPixels.result.json";
        private const string ImageRelativePath =
            "Temp/R8-WP01D-07/R8-WP01D-07-sceneview-central-isolated.png";
        private const int ReadyTimeoutEditorUpdates = 900;
        private const int CaptureWidth = 960;
        private static readonly Color IsolatedClearColor = Color.white;

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static SceneView sceneView;
        private static Camera sceneCamera;
        private static Camera worldCamera;
        private static SceneViewPixelReport report;
        private static int editorUpdates;
        private static bool previousPaused;
        private static bool running;

        [MenuItem("NTSD/Battle Diagnostics/R8/Run SceneView Central Pixel Play Probe")]
        public static void RunFromMenu()
        {
            StopObservation();
            ResetState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            sceneView = SceneView.lastActiveSceneView;
            sceneCamera = sceneView != null ? sceneView.camera : null;
            worldCamera = NTSDRenderSpace.WorldCamera;
            if (driver == null || world == null)
            {
                WriteImmediateFailure("The production driver or world is unavailable.");
                return;
            }
            if (sceneView == null || sceneCamera == null)
            {
                WriteImmediateFailure("No active Unity SceneView camera is available.");
                return;
            }
            if (worldCamera == null)
            {
                WriteImmediateFailure("The bound battle world camera is unavailable.");
                return;
            }

            previousPaused = driver.IsPaused;
            report = new SceneViewPixelReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                sceneViewName = sceneView.titleContent?.text ?? string.Empty,
                sceneCameraName = sceneCamera.name,
                sceneCameraType = sceneCamera.cameraType.ToString(),
                worldCameraName = worldCamera.name,
                imagePath = ImageRelativePath,
            };
            running = true;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null ||
                sceneCamera == null || worldCamera == null)
            {
                FinishFailure("Play Mode or a required runtime camera ended before capture completed.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > ReadyTimeoutEditorUpdates)
            {
                FinishFailure("Timed out waiting for a current central SceneView submission.");
                return;
            }
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                FinishFailure(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }

            BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0 ||
                !plan.IsValid || plan.Owner != BattlePixelFrameOwner.Central ||
                plan.Submission == null || plan.IsStale ||
                BattleCentralRenderSystem.PendingPublishedTickForDiagnostics !=
                BattleCentralRenderSystem.LastMaterializedPublishedTickForDiagnostics)
            {
                return;
            }
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            try
            {
                ExecuteCapture(plan);
            }
            catch (Exception exception)
            {
                FinishFailure("Unhandled SceneView pixel capture exception: " + exception);
            }
        }

        private static void ExecuteCapture(BattlePixelFramePlan plan)
        {
            report.captureTick = driver.CurrentTickIndex;
            report.baselineObjectCount = world.ObjectCount;
            report.baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.planValid = plan.IsValid;
            report.planOwner = plan.Owner.ToString();
            report.planSimulationTick = plan.SimulationTick;
            report.planDisplayTick = plan.DisplayTick;
            report.planGeneration = plan.Generation;
            report.planStale = plan.IsStale;
            report.pendingPublishedTick = BattleCentralRenderSystem.PendingPublishedTickForDiagnostics;
            report.lastMaterializedPublishedTick =
                BattleCentralRenderSystem.LastMaterializedPublishedTickForDiagnostics;
            report.sceneCameraType = sceneCamera.cameraType.ToString();
            report.sceneCameraGateAccepted = BattleCentralRenderSystem.CanRenderCamera(
                sceneCamera,
                CameraRenderType.Base,
                worldCamera);

            report.sceneSubmissionLeaseAccepted =
                BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    sceneCamera,
                    CameraRenderType.Base,
                    sceneCamera.cameraType,
                    true,
                    out BattleCentralSubmission.BattleCentralSubmissionLease lease);
            if (report.sceneSubmissionLeaseAccepted)
            {
                report.leaseTick = lease.TickIndex;
                report.leaseGeneration = lease.Generation;
                report.leaseSegmentCount = lease.Backend?.SegmentCount ?? 0;
                lease.Dispose();
            }

            BattleRenderingDiagnosticReport before = BattleCentralRenderSystem.CaptureDiagnosticReport();
            report.sourceCommandCount = before?.SourceCommandCount ?? 0;
            report.resolvedCommandCount = before?.ResolvedCommandCount ?? 0;
            report.segmentCount = before?.SegmentCount ?? 0;
            report.drawCountBefore = before?.SubmissionDrawCount ?? 0;

            CaptureSceneViewPixels();

            BattleRenderingDiagnosticReport after = BattleCentralRenderSystem.CaptureDiagnosticReport();
            report.drawCountAfter = after?.SubmissionDrawCount ?? 0;
            report.firstDifference = ClassifyFirstDifference();
            report.status = report.firstDifference == "NO_DIAGNOSTIC_DIFFERENCE" ? "PASS" : "FAIL";
            report.message =
                $"SceneView central pixels={report.nonClearPixelCount}; " +
                $"gate={report.sceneCameraGateAccepted}; lease={report.sceneSubmissionLeaseAccepted}; " +
                $"firstDifference={report.firstDifference}.";
            CleanupAndFinish();
        }

        private static void CaptureSceneViewPixels()
        {
            float aspect = worldCamera.aspect > 0f ? worldCamera.aspect : 16f / 9f;
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth / aspect));
            report.captureWidth = CaptureWidth;
            report.captureHeight = height;
            report.worldCameraPosition = VectorText(worldCamera.transform.position);
            report.worldCameraRotation = VectorText(worldCamera.transform.eulerAngles);
            report.worldCameraOrthographic = worldCamera.orthographic;
            report.worldCameraOrthographicSize = worldCamera.orthographicSize;

            var saved = new CameraState(sceneCamera);
            var target = new RenderTexture(
                CaptureWidth,
                height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            Texture2D readback = null;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                target.Create();
                sceneCamera.transform.SetPositionAndRotation(
                    worldCamera.transform.position,
                    worldCamera.transform.rotation);
                sceneCamera.orthographic = worldCamera.orthographic;
                sceneCamera.orthographicSize = worldCamera.orthographicSize;
                sceneCamera.fieldOfView = worldCamera.fieldOfView;
                sceneCamera.nearClipPlane = worldCamera.nearClipPlane;
                sceneCamera.farClipPlane = worldCamera.farClipPlane;
                sceneCamera.aspect = aspect;
                sceneCamera.rect = new Rect(0f, 0f, 1f, 1f);
                sceneCamera.cullingMask = 0;
                sceneCamera.clearFlags = CameraClearFlags.SolidColor;
                sceneCamera.backgroundColor = IsolatedClearColor;
                sceneCamera.allowHDR = false;
                sceneCamera.allowMSAA = false;
                sceneCamera.targetTexture = target;
                sceneCamera.ResetProjectionMatrix();
                sceneCamera.Render();

                RenderTexture.active = target;
                readback = new Texture2D(
                    CaptureWidth,
                    height,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    filterMode = FilterMode.Point,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, height), 0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                CountPixels(pixels, out int nonTransparent, out int nonClear);
                report.nonTransparentPixelCount = nonTransparent;
                report.nonClearPixelCount = nonClear;
                report.pixelHash = ComputePixelHash(pixels);

                string root = ProjectRoot();
                string imagePath = Path.GetFullPath(Path.Combine(root, ImageRelativePath));
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath) ?? Path.Combine(root, "Temp"));
                File.WriteAllBytes(imagePath, readback.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(sceneCamera);
                DestroyImmediateSafe(readback);
                target.Release();
                DestroyImmediateSafe(target);
            }
        }

        private static string ClassifyFirstDifference()
        {
            if (sceneCamera.cameraType != CameraType.SceneView)
                return "CAMERA_NOT_SCENEVIEW";
            if (!report.planValid || report.planOwner != BattlePixelFrameOwner.Central.ToString() ||
                report.planStale)
            {
                return "CENTRAL_PLAN_NOT_CURRENT";
            }
            if (!report.sceneCameraGateAccepted)
                return "SCENEVIEW_CAMERA_GATE_REJECTED";
            if (!report.sceneSubmissionLeaseAccepted)
                return "SCENEVIEW_SUBMISSION_LEASE_REJECTED";
            if (report.sourceCommandCount <= 0 || report.resolvedCommandCount <= 0 ||
                report.segmentCount <= 0 || report.leaseSegmentCount <= 0)
            {
                return "CENTRAL_SUBMISSION_EMPTY";
            }
            if (report.nonClearPixelCount <= 0)
                return "SCENEVIEW_CENTRAL_PIXELS_EMPTY";
            return "NO_DIAGNOSTIC_DIFFERENCE";
        }

        private static void CleanupAndFinish()
        {
            if (driver != null)
                driver.SetPaused(previousPaused);
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.afterObjectCount = world?.ObjectCount ?? -1;
            report.afterClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.cleanupRestored =
                report.afterObjectCount == report.baselineObjectCount &&
                report.afterClaimedSlots == report.baselineClaimedSlots;
            if (!report.cleanupRestored)
            {
                report.status = "FAIL";
                report.message +=
                    $" World counts changed: objects {report.baselineObjectCount}->{report.afterObjectCount}, " +
                    $"claimed {report.baselineClaimedSlots}->{report.afterClaimedSlots}.";
            }
            WriteResult(report);
            if (report.status == "PASS")
            {
                Debug.Log(
                    $"[BattleCentralSceneViewPixelProbe] PASS: pixels={report.nonClearPixelCount}, " +
                    $"commands={report.resolvedCommandCount}, segments={report.segmentCount}.");
            }
            else
            {
                Debug.LogError("[BattleCentralSceneViewPixelProbe] FAIL: " + report.message);
            }
            StopObservation();
        }

        private static void FinishFailure(string message)
        {
            report ??= new SceneViewPixelReport();
            report.status = "FAIL";
            report.message = message ?? string.Empty;
            if (driver != null)
                driver.SetPaused(previousPaused);
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.afterObjectCount = world?.ObjectCount ?? -1;
            report.afterClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            WriteResult(report);
            Debug.LogError("[BattleCentralSceneViewPixelProbe] FAIL: " + report.message);
            StopObservation();
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new SceneViewPixelReport { status = "FAIL", message = message ?? string.Empty });
            Debug.LogError("[BattleCentralSceneViewPixelProbe] FAIL: " + message);
        }

        private static void WriteResult(SceneViewPixelReport value)
        {
            string root = ProjectRoot();
            string path = Path.GetFullPath(Path.Combine(root, ResultRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Path.Combine(root, "Temp"));
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
        }

        private static void CountPixels(
            Color32[] pixels,
            out int nonTransparent,
            out int nonClear)
        {
            nonTransparent = 0;
            nonClear = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                if (pixel.a > 1)
                    nonTransparent++;
                if (pixel.r < 254 || pixel.g < 254 || pixel.b < 254 || pixel.a < 254)
                    nonClear++;
            }
        }

        private static string ComputePixelHash(Color32[] pixels)
        {
            const ulong offsetBasis = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offsetBasis;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                hash = (hash ^ pixel.r) * prime;
                hash = (hash ^ pixel.g) * prime;
                hash = (hash ^ pixel.b) * prime;
                hash = (hash ^ pixel.a) * prime;
            }
            return hash.ToString("X16");
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            running = false;
        }

        private static void ResetState()
        {
            driver = null;
            world = null;
            sceneView = null;
            sceneCamera = null;
            worldCamera = null;
            report = null;
            editorUpdates = 0;
            previousPaused = false;
            running = false;
        }

        private static string ProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static string VectorText(Vector3 value)
        {
            return $"{value.x:R},{value.y:R},{value.z:R}";
        }

        private static void DestroyImmediateSafe(UnityEngine.Object value)
        {
            if (value != null)
                UnityEngine.Object.DestroyImmediate(value);
        }

        private readonly struct CameraState
        {
            private readonly Vector3 position;
            private readonly Quaternion rotation;
            private readonly bool orthographic;
            private readonly float orthographicSize;
            private readonly float fieldOfView;
            private readonly float nearClipPlane;
            private readonly float farClipPlane;
            private readonly float aspect;
            private readonly Rect rect;
            private readonly int cullingMask;
            private readonly CameraClearFlags clearFlags;
            private readonly Color backgroundColor;
            private readonly bool allowHdr;
            private readonly bool allowMsaa;
            private readonly RenderTexture targetTexture;

            public CameraState(Camera camera)
            {
                position = camera.transform.position;
                rotation = camera.transform.rotation;
                orthographic = camera.orthographic;
                orthographicSize = camera.orthographicSize;
                fieldOfView = camera.fieldOfView;
                nearClipPlane = camera.nearClipPlane;
                farClipPlane = camera.farClipPlane;
                aspect = camera.aspect;
                rect = camera.rect;
                cullingMask = camera.cullingMask;
                clearFlags = camera.clearFlags;
                backgroundColor = camera.backgroundColor;
                allowHdr = camera.allowHDR;
                allowMsaa = camera.allowMSAA;
                targetTexture = camera.targetTexture;
            }

            public void Restore(Camera camera)
            {
                if (camera == null)
                    return;
                camera.targetTexture = targetTexture;
                camera.transform.SetPositionAndRotation(position, rotation);
                camera.orthographic = orthographic;
                camera.orthographicSize = orthographicSize;
                camera.fieldOfView = fieldOfView;
                camera.nearClipPlane = nearClipPlane;
                camera.farClipPlane = farClipPlane;
                camera.aspect = aspect;
                camera.rect = rect;
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = backgroundColor;
                camera.allowHDR = allowHdr;
                camera.allowMSAA = allowMsaa;
                camera.ResetProjectionMatrix();
            }
        }

        [Serializable]
        private sealed class SceneViewPixelReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public string firstDifference = string.Empty;
            public int startTick;
            public int captureTick;
            public int endTick;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int afterObjectCount;
            public int afterClaimedSlots;
            public bool cleanupRestored;
            public string sceneViewName = string.Empty;
            public string sceneCameraName = string.Empty;
            public string sceneCameraType = string.Empty;
            public string worldCameraName = string.Empty;
            public string worldCameraPosition = string.Empty;
            public string worldCameraRotation = string.Empty;
            public bool worldCameraOrthographic;
            public float worldCameraOrthographicSize;
            public bool planValid;
            public string planOwner = string.Empty;
            public int planSimulationTick;
            public int planDisplayTick;
            public int planGeneration;
            public bool planStale;
            public int pendingPublishedTick;
            public int lastMaterializedPublishedTick;
            public bool sceneCameraGateAccepted;
            public bool sceneSubmissionLeaseAccepted;
            public int leaseTick;
            public int leaseGeneration;
            public int leaseSegmentCount;
            public int sourceCommandCount;
            public int resolvedCommandCount;
            public int segmentCount;
            public int drawCountBefore;
            public int drawCountAfter;
            public int captureWidth;
            public int captureHeight;
            public int nonTransparentPixelCount;
            public int nonClearPixelCount;
            public string pixelHash = string.Empty;
            public string imagePath = string.Empty;
        }
    }
}
#endif
