#if UNITY_EDITOR
using System;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Editor-only witness for CentralOnly current, last-good stale, and replacement
    /// URP ownership. Cold ownership remains covered by the exact isolated self-check
    /// because resetting the live global renderer would violate the scene restore contract.
    /// Alignment contract: R8-CENTRALOWN-001.
    /// </summary>
    public static class BattleCentralFailClosedOwnershipPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run Central Fail-Closed Ownership Play Probe";
        private const string PreparePlayMenuPath =
            "NTSD/Battle Diagnostics/R8/Prepare Central Ownership Probe Play";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01G_R07C_CentralFailClosedOwnership.result.json";
        private const string RequestRelativePath =
            "Temp/NTSD_R8_WP01G_R07C_CentralFailClosedOwnership.request";
        private const int ReadyTimeoutEditorUpdates = 12000;
        private const int CaptureWidth = 640;
        private static readonly Color32 ClearColor = new Color32(255, 0, 255, 255);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static Camera worldCamera;
        private static OwnershipReport report;
        private static int editorUpdates;
        private static int requestReadyEditorUpdates;
        private static bool previousPaused;
        private static bool running;
        private static bool preparePlayResetArmed;

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!preparePlayResetArmed)
                return;
            if (state != PlayModeStateChange.ExitingEditMode &&
                state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            BattleCentralRenderSystem.ResetRuntime();
            if (state == PlayModeStateChange.EnteredPlayMode)
                preparePlayResetArmed = false;
        }

        private static void PollRequest()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || running)
            {
                return;
            }

            string requestPath = ProjectPath(RequestRelativePath);
            if (!File.Exists(requestPath))
            {
                requestReadyEditorUpdates = 0;
                return;
            }

            SimulationTickDriver currentDriver = SimulationTickDriver.Instance;
            Camera currentWorldCamera = NTSDRenderSpace.WorldCamera ?? Camera.main;
            SimulationWorld currentWorld = currentDriver?.World;
            if (currentWorld == null || currentWorldCamera == null ||
                !currentDriver.AllocationGate.IsSealed || !currentWorld.RuntimeCapacity.IsSealed)
            {
                requestReadyEditorUpdates++;
                if (requestReadyEditorUpdates <= ReadyTimeoutEditorUpdates)
                    return;

                File.Delete(requestPath);
                requestReadyEditorUpdates = 0;
                WriteImmediateFailure(
                    "Timed out waiting for the production driver, world, world camera, and battle seal " +
                    "before starting the CentralOnly ownership probe.");
                return;
            }

            File.Delete(requestPath);
            requestReadyEditorUpdates = 0;
            RunFromMenu();
        }

        [MenuItem(MenuPath)]
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
            worldCamera = NTSDRenderSpace.WorldCamera ?? Camera.main;
            if (driver == null || world == null || worldCamera == null)
            {
                WriteImmediateFailure("The production driver, world, or world camera is unavailable.");
                return;
            }

            previousPaused = driver.IsPaused;
            report = new OwnershipReport
            {
                status = "RUNNING",
                evidenceScope = "CURRENT_STALE_REPLACEMENT_PLAY;COLD_EXACT_SELFCHECK_ONLY",
                coldPlayExecuted = false,
                coldEvidence = "BattleRuntimeSelfCheck.CheckCentralPixelOwnershipContracts",
                startTick = driver.CurrentTickIndex,
                workerPath = driver.DedicatedSimulationWorkerActiveForDiagnostics,
            };
            running = true;
            EditorApplication.update += Observe;
        }

        [MenuItem(PreparePlayMenuPath)]
        public static void PrepareAndEnterPlayMode()
        {
            if (EditorApplication.isPlaying)
            {
                RunFromMenu();
                return;
            }

            StopObservation();
            ResetState();
            preparePlayResetArmed = true;
            BattleCentralRenderSystem.ResetRuntime();
            EditorApplication.EnterPlaymode();
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null || worldCamera == null)
            {
                FinishFailure("Play Mode or a required production runtime object ended before capture.");
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
                world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly ||
                BattleCentralRenderSystem.RegisteredFeatureCount <= 0)
            {
                return;
            }
            editorUpdates++;
            if (editorUpdates > ReadyTimeoutEditorUpdates)
            {
                FinishFailure("Timed out waiting for a current CentralOnly submission after scene readiness.");
                return;
            }
            if (!IsCurrentCentralPlan(plan))
            {
                if (world.BattlePresentation.PublishedFrame == null)
                    world.RenderDispatchAll(driver.CurrentTickIndex);
                plan = BattleCentralRenderSystem.PrepareFrame(world);
                if (!IsCurrentCentralPlan(plan))
                {
                    BattleRenderingDiagnosticReport diagnostics =
                        BattleCentralRenderSystem.CaptureDiagnosticReport();
                    FinishFailure(
                        $"PrepareFrame refused current central ownership: planReason='{plan.Reason}', " +
                        $"diagnosticReason='{diagnostics?.RefusalReason}', " +
                        $"publishedFrame={world.BattlePresentation.PublishedFrame != null}, " +
                        $"publishedTick={world.BattlePresentation.PublishedFrame?.TickIndex ?? -1}, " +
                        $"featureCount={BattleCentralRenderSystem.RegisteredFeatureCount}, " +
                        $"textureMaterial={BattleCentralRenderSystem.RegisteredFeatureMaterial != null}, " +
                        $"arrayMaterial={BattleCentralRenderSystem.RegisteredFeatureArrayMaterial != null}.");
                    return;
                }
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            BattleRenderingDiagnosticReport readiness =
                BattleCentralRenderSystem.CaptureDiagnosticReport();
            if (readiness == null || readiness.SourceCommandCount <= 0 ||
                readiness.ResolvedCommandCount <= 0 || readiness.SegmentCount <= 0)
            {
                return;
            }

            try
            {
                ExecuteCapture(plan);
            }
            catch (Exception exception)
            {
                FinishFailure("Unhandled R07C ownership exception: " + exception);
            }
        }

        private static void ExecuteCapture(BattlePixelFramePlan currentPlan)
        {
            report.baselineObjectCount = world.ObjectCount;
            report.baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.baselineFeatureCount = BattleCentralRenderSystem.RegisteredFeatureCount;
            report.baselineFeatureInstanceId =
                BattleCentralRenderSystem.RegisteredFeature != null
                    ? BattleCentralRenderSystem.RegisteredFeature.GetInstanceID()
                    : 0;
            report.baselineTextureMaterialInstanceId = InstanceId(
                BattleCentralRenderSystem.RegisteredFeatureMaterial);
            report.baselineArrayMaterialInstanceId = InstanceId(
                BattleCentralRenderSystem.RegisteredFeatureArrayMaterial);
            report.baselineDrawMode = BattleCentralRenderSystem.RegisteredFeatureDrawMode.ToString();
            int checksumTick = driver.CurrentTickIndex;
            ulong checksumBefore = CaptureChecksum(checksumTick);
            report.checksumBefore = checksumBefore.ToString("X16");

            report.current = CapturePlanEvidence("current", currentPlan);
            report.current.pixels = CaptureCentralPixels("current");
            Require(report.current.pixels.nonClearPixelCount > 0,
                "The current CentralOnly submission produced no isolated Game-camera pixels.");

            int staleSimulationTick = currentPlan.SimulationTick + 1;
            BattlePixelFramePlan stalePlan =
                BattleCentralRenderSystem.PublishStaleCentralPlanForSelfCheck(
                    world,
                    staleSimulationTick);
            report.stale = CapturePlanEvidence("stale", stalePlan);
            report.stale.pixels = CaptureCentralPixels("stale");
            Require(stalePlan.IsValid && stalePlan.Owner == BattlePixelFrameOwner.Central &&
                    stalePlan.SuppressesLegacyMaterializers && stalePlan.IsStale &&
                    stalePlan.SimulationTick == staleSimulationTick &&
                    stalePlan.DisplayTick == currentPlan.DisplayTick &&
                    stalePlan.Generation == currentPlan.Generation &&
                    ReferenceEquals(stalePlan.Submission, currentPlan.Submission) &&
                    !string.IsNullOrEmpty(stalePlan.Reason),
                "Transient failure did not retain the last-good central submission contract.");
            Require(report.stale.pixels.nonClearPixelCount > 0 &&
                    report.stale.pixels.pixelHash == report.current.pixels.pixelHash,
                "The stale frame did not preserve the exact last-good isolated pixels.");

            BattleCentralSubmission oldSubmission = stalePlan.Submission;
            BattleCentralSubmission.BattleCentralSubmissionLease retainedLease = default;
            Require(oldSubmission != null && oldSubmission.TryAcquire(out retainedLease),
                "The stale last-good submission could not provide a protected read lease.");
            report.staleRetainedLeaseAccepted = true;
            try
            {
                int replacementTick = staleSimulationTick;
                world.RenderDispatchAll(replacementTick);
                BattlePixelFramePlan replacementPlan =
                    BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                report.replacement = CapturePlanEvidence("replacement", replacementPlan);
                Require(replacementPlan.IsValid &&
                        replacementPlan.Owner == BattlePixelFrameOwner.Central &&
                        replacementPlan.SuppressesLegacyMaterializers &&
                        !replacementPlan.IsStale &&
                        replacementPlan.SimulationTick == replacementTick &&
                        replacementPlan.DisplayTick == replacementTick &&
                        replacementPlan.Generation > stalePlan.Generation &&
                        !ReferenceEquals(replacementPlan.Submission, oldSubmission),
                    "Replacement did not publish a new current central generation.");
                report.oldSubmissionRetiredDuringReplacement = oldSubmission.IsRetired;
                report.oldSubmissionRejectedNewLease = !oldSubmission.TryAcquire(out _);
                Require(report.oldSubmissionRetiredDuringReplacement &&
                        report.oldSubmissionRejectedNewLease && retainedLease.IsValid,
                    "Replacement did not retire the old submission while preserving its held lease.");

                PixelEvidence replacementPixels = CaptureCentralPixels("replacement");
                report.replacement = CapturePlanEvidence("replacement", replacementPlan);
                report.replacement.pixels = replacementPixels;
                Require(report.replacement.pixels.nonClearPixelCount > 0 &&
                        report.replacement.submissionLeaseAccepted &&
                        report.replacement.leaseSegmentCount > 0 &&
                        report.replacement.drawCount > 0,
                    "The replacement CentralOnly submission produced no isolated pixels.");
            }
            finally
            {
                retainedLease.Dispose();
            }

            report.oldSubmissionLeaseReleased = oldSubmission.ReadLeaseCount == 0;
            Require(report.oldSubmissionLeaseReleased,
                "The retired old submission retained a read lease after replacement.");

            ulong checksumAfter = CaptureChecksum(checksumTick);
            report.checksumAfter = checksumAfter.ToString("X16");
            report.checksumUnchanged = checksumBefore == checksumAfter;
            Require(report.checksumUnchanged,
                "Presentation-only ownership transitions changed the battle checksum.");

            CaptureCleanupState();
            Require(report.cleanupRestored,
                "R07C did not preserve world counts, feature/material/draw-mode registration, or CentralOnly mode.");
            report.status = "PASS";
            report.message =
                "current/stale/replacement real URP Play passed; cold remains exact self-check only.";
            WriteResult(report);
            Debug.Log(
                $"[BattleCentralFailClosedOwnershipProbe] PASS: current={report.current.pixels.nonClearPixelCount}, " +
                $"stale={report.stale.pixels.nonClearPixelCount}, replacement={report.replacement.pixels.nonClearPixelCount}, " +
                $"cold={report.coldEvidence}.");
            RestorePauseAndStop();
        }

        private static PlanEvidence CapturePlanEvidence(string state, BattlePixelFramePlan plan)
        {
            bool acquired = BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                worldCamera,
                CameraRenderType.Base,
                CameraType.Game,
                true,
                out BattleCentralSubmission.BattleCentralSubmissionLease lease);
            int leaseTick = acquired ? lease.TickIndex : -1;
            int leaseGeneration = acquired ? lease.Generation : 0;
            int leaseSegments = acquired ? lease.Backend?.SegmentCount ?? 0 : 0;
            lease.Dispose();

            BattleRenderingDiagnosticReport diagnostics =
                BattleCentralRenderSystem.CaptureDiagnosticReport();
            return new PlanEvidence
            {
                state = state,
                valid = plan.IsValid,
                requestedMode = plan.RequestedMode.ToString(),
                owner = plan.Owner.ToString(),
                simulationTick = plan.SimulationTick,
                displayTick = plan.DisplayTick,
                generation = plan.Generation,
                stale = plan.IsStale,
                reason = plan.Reason,
                hasSubmission = plan.Submission != null,
                legacySuppressed = plan.SuppressesLegacyMaterializers &&
                                   BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(world) &&
                                   !SimulationWorld.RequiresLegacySpriteRendererCapacityGuard(plan),
                submissionLeaseAccepted = acquired,
                leaseTick = leaseTick,
                leaseGeneration = leaseGeneration,
                leaseSegmentCount = leaseSegments,
                sourceCommandCount = diagnostics?.SourceCommandCount ?? 0,
                resolvedCommandCount = diagnostics?.ResolvedCommandCount ?? 0,
                segmentCount = diagnostics?.SegmentCount ?? 0,
                drawCount = diagnostics?.SubmissionDrawCount ?? 0,
                refusalReason = diagnostics?.RefusalReason ?? string.Empty,
            };
        }

        private static PixelEvidence CaptureCentralPixels(string state)
        {
            int height = Mathf.Max(
                1,
                Mathf.RoundToInt(CaptureWidth /
                                 (worldCamera.aspect > 0f ? worldCamera.aspect : 16f / 9f)));
            var saved = new CameraState(worldCamera);
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
                worldCamera.cullingMask = 0;
                worldCamera.clearFlags = CameraClearFlags.SolidColor;
                worldCamera.backgroundColor = ClearColor;
                worldCamera.allowHDR = false;
                worldCamera.allowMSAA = false;
                worldCamera.targetTexture = target;
                worldCamera.Render();

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
                int nonClear = 0;
                for (int index = 0; index < pixels.Length; index++)
                {
                    Color32 pixel = pixels[index];
                    if (pixel.r != ClearColor.r || pixel.g != ClearColor.g ||
                        pixel.b != ClearColor.b || pixel.a != ClearColor.a)
                    {
                        nonClear++;
                    }
                }

                string imageRelativePath = $"Temp/NTSD_R8_WP01G_R07C_{state}.png";
                File.WriteAllBytes(ProjectPath(imageRelativePath), readback.EncodeToPNG());
                return new PixelEvidence
                {
                    width = CaptureWidth,
                    height = height,
                    nonClearPixelCount = nonClear,
                    pixelHash = ComputePixelHash(pixels),
                    imagePath = imageRelativePath,
                };
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(worldCamera);
                DestroyImmediateSafe(readback);
                target.Release();
                DestroyImmediateSafe(target);
            }
        }

        private static bool IsCurrentCentralPlan(BattlePixelFramePlan plan)
        {
            return plan.IsValid && ReferenceEquals(plan.World, world) &&
                   plan.RequestedMode == BattlePresentationBackendMode.CentralOnly &&
                   plan.Owner == BattlePixelFrameOwner.Central && !plan.IsStale &&
                   plan.Submission != null &&
                   BattleCentralRenderSystem.ShouldUseCentralPixels(world);
        }

        private static ulong CaptureChecksum(int tickIndex)
        {
            return world.CaptureRuntimeChecksum64(tickIndex, FrameInputSet.Empty(tickIndex));
        }

        private static void CaptureCleanupState()
        {
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.finalObjectCount = world?.ObjectCount ?? -1;
            report.finalClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.finalFeatureCount = BattleCentralRenderSystem.RegisteredFeatureCount;
            report.finalFeatureInstanceId =
                BattleCentralRenderSystem.RegisteredFeature != null
                    ? BattleCentralRenderSystem.RegisteredFeature.GetInstanceID()
                    : 0;
            report.finalTextureMaterialInstanceId = InstanceId(
                BattleCentralRenderSystem.RegisteredFeatureMaterial);
            report.finalArrayMaterialInstanceId = InstanceId(
                BattleCentralRenderSystem.RegisteredFeatureArrayMaterial);
            report.finalDrawMode = BattleCentralRenderSystem.RegisteredFeatureDrawMode.ToString();
            report.cleanupRestored =
                report.finalObjectCount == report.baselineObjectCount &&
                report.finalClaimedSlots == report.baselineClaimedSlots &&
                report.finalFeatureCount == report.baselineFeatureCount &&
                report.finalFeatureInstanceId == report.baselineFeatureInstanceId &&
                report.finalTextureMaterialInstanceId == report.baselineTextureMaterialInstanceId &&
                report.finalArrayMaterialInstanceId == report.baselineArrayMaterialInstanceId &&
                report.finalDrawMode == report.baselineDrawMode &&
                world?.BattlePresentation.Mode == BattlePresentationBackendMode.CentralOnly;
        }

        private static void FinishFailure(string message)
        {
            report ??= new OwnershipReport();
            report.status = "FAIL";
            report.message = message ?? string.Empty;
            CaptureCleanupStateIfAvailable();
            WriteResult(report);
            Debug.LogError("[BattleCentralFailClosedOwnershipProbe] FAIL: " + report.message);
            RestorePauseAndStop();
        }

        private static void CaptureCleanupStateIfAvailable()
        {
            if (driver == null || world == null)
                return;
            CaptureCleanupState();
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new OwnershipReport
            {
                status = "FAIL",
                message = message ?? string.Empty,
            });
            Debug.LogError("[BattleCentralFailClosedOwnershipProbe] FAIL: " + message);
        }

        private static void RestorePauseAndStop()
        {
            if (driver != null)
                driver.SetPaused(previousPaused);
            StopObservation();
        }

        private static void WriteResult(OwnershipReport value)
        {
            File.WriteAllText(ProjectPath(ResultRelativePath), JsonUtility.ToJson(value, true));
        }

        private static string ProjectPath(string relativePath)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Path.GetTempPath());
            return path;
        }

        private static int InstanceId(UnityEngine.Object value)
        {
            return value != null ? value.GetInstanceID() : 0;
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

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void DestroyImmediateSafe(UnityEngine.Object value)
        {
            if (value != null)
                UnityEngine.Object.DestroyImmediate(value);
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
            worldCamera = null;
            report = null;
            editorUpdates = 0;
            requestReadyEditorUpdates = 0;
            previousPaused = false;
            running = false;
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
                if (camera == null)
                    return;
                camera.targetTexture = targetTexture;
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = backgroundColor;
                camera.allowHDR = allowHdr;
                camera.allowMSAA = allowMsaa;
            }
        }

        [Serializable]
        private sealed class OwnershipReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public string evidenceScope = string.Empty;
            public bool coldPlayExecuted;
            public string coldEvidence = string.Empty;
            public int startTick;
            public int endTick;
            public bool workerPath;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int baselineFeatureCount;
            public int baselineFeatureInstanceId;
            public int baselineTextureMaterialInstanceId;
            public int baselineArrayMaterialInstanceId;
            public string baselineDrawMode = string.Empty;
            public int finalFeatureCount;
            public int finalFeatureInstanceId;
            public int finalTextureMaterialInstanceId;
            public int finalArrayMaterialInstanceId;
            public string finalDrawMode = string.Empty;
            public bool cleanupRestored;
            public string checksumBefore = string.Empty;
            public string checksumAfter = string.Empty;
            public bool checksumUnchanged;
            public bool staleRetainedLeaseAccepted;
            public bool oldSubmissionRetiredDuringReplacement;
            public bool oldSubmissionRejectedNewLease;
            public bool oldSubmissionLeaseReleased;
            public PlanEvidence current;
            public PlanEvidence stale;
            public PlanEvidence replacement;
        }

        [Serializable]
        private sealed class PlanEvidence
        {
            public string state = string.Empty;
            public bool valid;
            public string requestedMode = string.Empty;
            public string owner = string.Empty;
            public int simulationTick;
            public int displayTick;
            public int generation;
            public bool stale;
            public string reason = string.Empty;
            public bool hasSubmission;
            public bool legacySuppressed;
            public bool submissionLeaseAccepted;
            public int leaseTick;
            public int leaseGeneration;
            public int leaseSegmentCount;
            public int sourceCommandCount;
            public int resolvedCommandCount;
            public int segmentCount;
            public int drawCount;
            public string refusalReason = string.Empty;
            public PixelEvidence pixels;
        }

        [Serializable]
        private sealed class PixelEvidence
        {
            public int width;
            public int height;
            public int nonClearPixelCount;
            public string pixelHash = string.Empty;
            public string imagePath = string.Empty;
        }
    }
}
#endif
