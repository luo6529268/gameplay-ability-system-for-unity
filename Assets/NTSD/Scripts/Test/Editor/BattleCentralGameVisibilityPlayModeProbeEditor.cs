#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Captures the live CentralOnly submission chain for every claimed runtime slot.
    /// The probe is diagnostic-only and does not create or mutate battle entities.
    /// </summary>
    public static class BattleCentralGameVisibilityPlayModeProbeEditor
    {
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01D_06_GameVisibility.result.json";
        private const int ReadyTimeoutEditorUpdates = 900;

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static VisibilityReport report;
        private static int editorUpdates;
        private static bool previousPaused;
        private static bool running;

        [MenuItem("NTSD/Battle Diagnostics/R8/Run Central Game Visibility Play Probe")]
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
            if (driver == null || world == null)
            {
                WriteImmediateFailure("The production driver or world is unavailable.");
                return;
            }

            previousPaused = driver.IsPaused;
            report = new VisibilityReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                backendMode = world.BattlePresentation.Mode.ToString(),
            };
            running = true;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                FinishFailure("Play Mode or the production runtime ended before capture completed.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > ReadyTimeoutEditorUpdates)
            {
                FinishFailure("Timed out waiting for the live battle presentation.");
                return;
            }
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                FinishFailure(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }
            if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0 ||
                world.BattlePresentation.PublishedFrame == null)
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
                ExecuteCapture();
            }
            catch (Exception exception)
            {
                FinishFailure("Unhandled visibility capture exception: " + exception);
            }
        }

        private static void ExecuteCapture()
        {
            report.captureTick = driver.CurrentTickIndex;
            report.baselineObjectCount = world.ObjectCount;
            report.baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.runtimeSlotCapacity = world.RuntimeSlotCapacityForDiagnostics;
            BattlePresentationFrame publishedFrame = world.BattlePresentation.PublishedFrame;
            report.publishedFrameTick = publishedFrame?.TickIndex ?? -1;
            report.publishedFrameEntityCount = publishedFrame?.EntityCount ?? 0;
            report.publishedFrameCommandCount = publishedFrame?.CommandCount ?? 0;
            report.publishedFrameOrderMaterialized =
                publishedFrame?.PresentationOrderMaterialized == true;
            report.publishedFrameCommandsMaterialized = publishedFrame?.CommandsMaterialized == true;
            report.driverActiveAndEnabled = driver.isActiveAndEnabled;
            report.driverPausedBeforeCapture = previousPaused;
            report.workerActive = driver.DedicatedSimulationWorkerActiveForDiagnostics;
            report.workerTickInFlight = driver.DedicatedSimulationWorkerTickInFlightForDiagnostics;
            report.workerFailure = driver.DedicatedSimulationWorkerFailureForDiagnostics?.ToString() ?? string.Empty;
            report.workerIneligibilityReason =
                driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics ?? string.Empty;
            report.workerLastSubmissionFailureReason =
                driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics ?? string.Empty;
            report.pendingPublishedTick =
                BattleCentralRenderSystem.PendingPublishedTickForDiagnostics;
            report.lastMaterializedPublishedTick =
                BattleCentralRenderSystem.LastMaterializedPublishedTickForDiagnostics;
            report.unityFrameCount = Time.frameCount;
            BattlePixelFramePlan plan = BattleCentralRenderSystem.CurrentPixelFramePlan;
            report.currentPlanValid = plan.IsValid;
            report.currentPlanOwner = plan.Owner.ToString();
            report.currentPlanSimulationTick = plan.SimulationTick;
            report.currentPlanDisplayTick = plan.DisplayTick;
            report.currentPlanGeneration = plan.Generation;
            report.currentPlanStale = plan.IsStale;
            report.currentPlanReason = plan.Reason;
            report.currentPlanHasCapturedFrame = plan.CapturedFrame != null;
            report.currentPlanHasSubmission = plan.Submission != null;
            CaptureRenderingReport();
            CaptureCameraAndMaterials();
            CaptureAllSlotDiagnostics();
            report.firstDifference = ClassifyFirstDifference();
            report.status = "PASS";
            report.message =
                $"Captured {report.claimedSlotCount} claimed slot(s); firstDifference={report.firstDifference}.";
            CleanupAndFinish();
        }

        private static void CaptureRenderingReport()
        {
            BattleRenderingDiagnosticReport rendering =
                BattleCentralRenderSystem.CaptureDiagnosticReport();
            report.renderingReportAvailable = rendering != null;
            if (rendering == null)
                return;

            report.renderingReportJson = rendering.ToJson();
            report.requestedPixelMode = rendering.RequestedPixelMode.ToString();
            report.effectivePixelMode = rendering.EffectivePixelMode.ToString();
            report.sourceCommandCount = rendering.SourceCommandCount;
            report.resolvedCommandCount = rendering.ResolvedCommandCount;
            report.unresolvedCommandCount = rendering.UnresolvedCommandCount;
            report.unsupportedCategoryCount = rendering.UnsupportedCategoryCount;
            report.unsupportedRenderStateCount = rendering.UnsupportedRenderStateCount;
            report.activeChunkCount = rendering.ActiveChunkCount;
            report.segmentCount = rendering.SegmentCount;
            report.submissionDrawCount = rendering.SubmissionDrawCount;
            report.snapshotEntityCount = rendering.SnapshotEntityCount;
            report.generation = rendering.Generation;
            report.buildTick = rendering.BuildTick;
            report.simulationTick = rendering.SimulationTick;
            report.displayTick = rendering.DisplayTick;
            report.isStale = rendering.IsStale;
            report.submissionBuildCurrent = rendering.SubmissionBuildCurrent;
            report.refusalReason = rendering.RefusalReason;
            report.firstUnresolvedCommandIndex = rendering.FirstUnresolvedCommandIndex;
            report.firstUnresolvedCommandType = rendering.FirstUnresolvedCommandType.ToString();
            report.firstUnresolvedStatus = rendering.FirstUnresolvedStatus.ToString();
        }

        private static void CaptureCameraAndMaterials()
        {
            Camera camera = Camera.main;
            report.mainCameraFound = camera != null;
            report.battleLayer = LayerMask.NameToLayer("Battle");
            if (camera != null)
            {
                report.mainCameraName = camera.name;
                report.mainCameraEnabled = camera.enabled;
                report.mainCameraActive = camera.gameObject.activeInHierarchy;
                report.mainCameraCullingMask = camera.cullingMask;
                report.mainCameraIncludesBattleLayer =
                    report.battleLayer >= 0 &&
                    (camera.cullingMask & (1 << report.battleLayer)) != 0;
                report.mainCameraOrthographic = camera.orthographic;
                report.mainCameraOrthographicSize = camera.orthographicSize;
                report.mainCameraPosition = VectorText(camera.transform.position);
                report.mainCameraNearClip = camera.nearClipPlane;
                report.mainCameraFarClip = camera.farClipPlane;
            }

            Material textureMaterial =
                BattleCentralRenderSystem.RegisteredFeatureMaterialForAcceptance;
            Material arrayMaterial =
                BattleCentralRenderSystem.RegisteredFeatureArrayMaterialForAcceptance;
            report.featureTextureMaterialFound = textureMaterial != null;
            report.featureArrayMaterialFound = arrayMaterial != null;
            report.featureTextureMaterialValid =
                BattleSpriteMaterialContract.IsDeclaredCentralMaterial(textureMaterial, false);
            report.featureArrayMaterialValid =
                BattleSpriteMaterialContract.IsDeclaredCentralMaterial(arrayMaterial, true);
        }

        private static void CaptureAllSlotDiagnostics()
        {
            var rows = new List<SlotDiagnosticRow>(world.ClaimedRuntimeSlotCountForDiagnostics);
            var entityReasons = new Dictionary<BattleCentralEntityDiagnosticReason, int>();
            var shadowReasons = new Dictionary<BattleCentralEntityDiagnosticReason, int>();
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                BattleCentralEntityDiagnostic entity =
                    BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(
                        world,
                        slot,
                        BattleRenderCommandType.Entity);
                if (entity.Reason == BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle)
                    continue;
                BattleCentralEntityDiagnostic shadow =
                    BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(
                        world,
                        slot,
                        BattleRenderCommandType.Shadow);
                rows.Add(new SlotDiagnosticRow
                {
                    runtimeSlot = slot,
                    stableId = entity.StableId,
                    objectId = entity.ObjectId,
                    currentDatObjectId = entity.CurrentDatObjectId,
                    effectivePic = entity.EffectivePic,
                    frameId = entity.FrameId,
                    entityReason = entity.Reason.ToString(),
                    entityVisible = entity.EntityVisible,
                    entityHasSnapshot = entity.HasSnapshot,
                    entityHasCommand = entity.HasCommand,
                    entityHasResolvedResource = entity.HasResolvedResource,
                    entitySubmitted = entity.Submitted,
                    entityCommandIndex = entity.CommandIndex,
                    entitySegmentIndex = entity.SegmentIndex,
                    entityChunkIndex = entity.ChunkIndex,
                    entityBindingMode = entity.BindingMode.ToString(),
                    entityAtlasSlice = entity.AtlasSlice,
                    entityPosition = VectorText(entity.Position),
                    entityPivot = VectorText(entity.Pivot),
                    entitySortOrder = entity.SortOrder,
                    shadowReason = shadow.Reason.ToString(),
                    shadowVisible = shadow.ShadowVisible,
                    shadowHasSnapshot = shadow.HasSnapshot,
                    shadowHasCommand = shadow.HasCommand,
                    shadowHasResolvedResource = shadow.HasResolvedResource,
                    shadowSubmitted = shadow.Submitted,
                    shadowCommandIndex = shadow.CommandIndex,
                    shadowSegmentIndex = shadow.SegmentIndex,
                    shadowChunkIndex = shadow.ChunkIndex,
                    shadowPosition = VectorText(shadow.Position),
                    shadowSortOrder = shadow.SortOrder,
                });
                IncrementReason(entityReasons, entity.Reason);
                IncrementReason(shadowReasons, shadow.Reason);
            }

            report.claimedSlotCount = rows.Count;
            report.slots = rows.ToArray();
            report.entityReasons = BuildReasonRows(entityReasons);
            report.shadowReasons = BuildReasonRows(shadowReasons);
        }

        private static string ClassifyFirstDifference()
        {
            if (!report.renderingReportAvailable)
                return "NO_RENDERING_REPORT";
            if (report.snapshotEntityCount == 0)
                return "NO_SNAPSHOT_ENTITIES";
            if (report.sourceCommandCount == 0)
                return "NO_SOURCE_COMMANDS";
            if (report.unresolvedCommandCount > 0 ||
                report.resolvedCommandCount != report.sourceCommandCount)
            {
                return "RESOURCE_RESOLUTION_OR_COMMAND_SUPPORT";
            }
            if (report.activeChunkCount == 0 || report.segmentCount == 0)
                return "BACKEND_MESH_BUILD_EMPTY";
            if (!report.submissionBuildCurrent)
                return "SUBMISSION_BUILD_NOT_CURRENT";
            if (report.submissionDrawCount == 0)
                return "URP_SUBMISSION_NOT_EXECUTED";
            for (int index = 0; index < report.slots.Length; index++)
            {
                SlotDiagnosticRow row = report.slots[index];
                if (row.entityReason != nameof(BattleCentralEntityDiagnosticReason.None) &&
                    row.entityReason != nameof(BattleCentralEntityDiagnosticReason.PresentationVisibilityFalse))
                {
                    return "ENTITY_" + row.entityReason.ToUpperInvariant();
                }
                if (row.shadowReason != nameof(BattleCentralEntityDiagnosticReason.None) &&
                    row.shadowReason != nameof(BattleCentralEntityDiagnosticReason.PresentationVisibilityFalse))
                {
                    return "SHADOW_" + row.shadowReason.ToUpperInvariant();
                }
            }
            if (!report.mainCameraFound || !report.mainCameraEnabled || !report.mainCameraActive)
                return "MAIN_CAMERA_UNAVAILABLE";
            if (!report.mainCameraIncludesBattleLayer)
                return "MAIN_CAMERA_CULLS_BATTLE_LAYER";
            return "NO_DIAGNOSTIC_DIFFERENCE";
        }

        private static void IncrementReason(
            Dictionary<BattleCentralEntityDiagnosticReason, int> counts,
            BattleCentralEntityDiagnosticReason reason)
        {
            counts.TryGetValue(reason, out int count);
            counts[reason] = count + 1;
        }

        private static ReasonCountRow[] BuildReasonRows(
            Dictionary<BattleCentralEntityDiagnosticReason, int> counts)
        {
            var keys = new List<BattleCentralEntityDiagnosticReason>(counts.Keys);
            keys.Sort();
            var rows = new ReasonCountRow[keys.Count];
            for (int index = 0; index < keys.Count; index++)
            {
                BattleCentralEntityDiagnosticReason reason = keys[index];
                rows[index] = new ReasonCountRow
                {
                    reason = reason.ToString(),
                    count = counts[reason],
                };
            }
            return rows;
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
                report.message =
                    $"World counts changed: objects {report.baselineObjectCount}->{report.afterObjectCount}, " +
                    $"claimed {report.baselineClaimedSlots}->{report.afterClaimedSlots}.";
            }
            WriteResult(report);
            Debug.Log(
                $"[BattleCentralGameVisibilityProbe] {report.status}: slots={report.claimedSlotCount}, " +
                $"commands={report.sourceCommandCount}, resolved={report.resolvedCommandCount}, " +
                $"draws={report.submissionDrawCount}, firstDifference={report.firstDifference}.");
            StopObservation();
        }

        private static void FinishFailure(string message)
        {
            report ??= new VisibilityReport();
            report.status = "FAIL";
            report.message = message ?? string.Empty;
            if (driver != null)
                driver.SetPaused(previousPaused);
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.afterObjectCount = world?.ObjectCount ?? -1;
            report.afterClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            WriteResult(report);
            Debug.LogError("[BattleCentralGameVisibilityProbe] FAIL: " + report.message);
            StopObservation();
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new VisibilityReport { status = "FAIL", message = message ?? string.Empty });
            Debug.LogError("[BattleCentralGameVisibilityProbe] FAIL: " + message);
        }

        private static void WriteResult(VisibilityReport value)
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string path = Path.GetFullPath(Path.Combine(root, ResultRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Path.Combine(root, "Temp"));
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
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
            report = null;
            editorUpdates = 0;
            previousPaused = false;
            running = false;
        }

        private static string VectorText(Vector3 value)
        {
            return $"{value.x:R},{value.y:R},{value.z:R}";
        }

        private static string VectorText(Vector2 value)
        {
            return $"{value.x:R},{value.y:R}";
        }

        [Serializable]
        private sealed class VisibilityReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public string firstDifference = string.Empty;
            public string backendMode = string.Empty;
            public int startTick;
            public int captureTick;
            public int endTick;
            public int publishedFrameTick;
            public int publishedFrameEntityCount;
            public int publishedFrameCommandCount;
            public bool publishedFrameOrderMaterialized;
            public bool publishedFrameCommandsMaterialized;
            public bool driverActiveAndEnabled;
            public bool driverPausedBeforeCapture;
            public bool workerActive;
            public bool workerTickInFlight;
            public string workerFailure = string.Empty;
            public string workerIneligibilityReason = string.Empty;
            public string workerLastSubmissionFailureReason = string.Empty;
            public int pendingPublishedTick;
            public int lastMaterializedPublishedTick;
            public int unityFrameCount;
            public bool currentPlanValid;
            public string currentPlanOwner = string.Empty;
            public int currentPlanSimulationTick;
            public int currentPlanDisplayTick;
            public int currentPlanGeneration;
            public bool currentPlanStale;
            public string currentPlanReason = string.Empty;
            public bool currentPlanHasCapturedFrame;
            public bool currentPlanHasSubmission;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int afterObjectCount;
            public int afterClaimedSlots;
            public bool cleanupRestored;
            public int runtimeSlotCapacity;
            public int claimedSlotCount;
            public bool renderingReportAvailable;
            public string renderingReportJson = string.Empty;
            public string requestedPixelMode = string.Empty;
            public string effectivePixelMode = string.Empty;
            public int sourceCommandCount;
            public int resolvedCommandCount;
            public int unresolvedCommandCount;
            public int unsupportedCategoryCount;
            public int unsupportedRenderStateCount;
            public int activeChunkCount;
            public int segmentCount;
            public int submissionDrawCount;
            public int snapshotEntityCount;
            public int generation;
            public int buildTick;
            public int simulationTick;
            public int displayTick;
            public bool isStale;
            public bool submissionBuildCurrent;
            public string refusalReason = string.Empty;
            public int firstUnresolvedCommandIndex;
            public string firstUnresolvedCommandType = string.Empty;
            public string firstUnresolvedStatus = string.Empty;
            public int battleLayer;
            public bool mainCameraFound;
            public string mainCameraName = string.Empty;
            public bool mainCameraEnabled;
            public bool mainCameraActive;
            public int mainCameraCullingMask;
            public bool mainCameraIncludesBattleLayer;
            public bool mainCameraOrthographic;
            public float mainCameraOrthographicSize;
            public string mainCameraPosition = string.Empty;
            public float mainCameraNearClip;
            public float mainCameraFarClip;
            public bool featureTextureMaterialFound;
            public bool featureArrayMaterialFound;
            public bool featureTextureMaterialValid;
            public bool featureArrayMaterialValid;
            public ReasonCountRow[] entityReasons = Array.Empty<ReasonCountRow>();
            public ReasonCountRow[] shadowReasons = Array.Empty<ReasonCountRow>();
            public SlotDiagnosticRow[] slots = Array.Empty<SlotDiagnosticRow>();
        }

        [Serializable]
        private sealed class ReasonCountRow
        {
            public string reason = string.Empty;
            public int count;
        }

        [Serializable]
        private sealed class SlotDiagnosticRow
        {
            public int runtimeSlot;
            public int stableId;
            public int objectId;
            public int currentDatObjectId;
            public int effectivePic;
            public int frameId;
            public string entityReason = string.Empty;
            public bool entityVisible;
            public bool entityHasSnapshot;
            public bool entityHasCommand;
            public bool entityHasResolvedResource;
            public bool entitySubmitted;
            public int entityCommandIndex;
            public int entitySegmentIndex;
            public int entityChunkIndex;
            public string entityBindingMode = string.Empty;
            public int entityAtlasSlice;
            public string entityPosition = string.Empty;
            public string entityPivot = string.Empty;
            public int entitySortOrder;
            public string shadowReason = string.Empty;
            public bool shadowVisible;
            public bool shadowHasSnapshot;
            public bool shadowHasCommand;
            public bool shadowHasResolvedResource;
            public bool shadowSubmitted;
            public int shadowCommandIndex;
            public int shadowSegmentIndex;
            public int shadowChunkIndex;
            public string shadowPosition = string.Empty;
            public int shadowSortOrder;
        }
    }
}
#endif
