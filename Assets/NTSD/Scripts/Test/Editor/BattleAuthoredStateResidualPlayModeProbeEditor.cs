#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Production-pool/full-tick witness for authored state2000 and state8xxx.
    /// Alignment contract: R8-AUTHOREDSTATE-PLAY-001.
    /// </summary>
    public static class BattleAuthoredStateResidualPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run Authored State Residual Play Probe";
        private const string RequestRelativePath =
            "Temp/NTSD_R8_WP01G_R11_AuthoredStateResidual.request";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01G_R11_AuthoredStateResidual.result.json";
        private const int State2000Oid = 150;
        private const int State8xxxOid = 32;
        private const int DynamicSlotStart = 50;
        private const int TimeoutEditorUpdates = 2400;

        private static readonly List<LF2Entity> BaselineEntities =
            new List<LF2Entity>(128);
        private static readonly List<LF2Entity> EntityScratch =
            new List<LF2Entity>(128);
        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(16);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPointFactory factory;
        private static LF2ObjectPool objectPool;
        private static ProbeReport report;
        private static bool previousPaused;
        private static bool running;
        private static bool pauseRequested;
        private static int editorUpdates;
        private static int baselineObjectCount;
        private static int baselineClaimedSlots;
        private static int baselineObjectPoolActive;
        private static int baselineLogicPoolActive;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
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
                return;
            File.Delete(requestPath);
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
            factory = LF2ObjectPointFactory.Instance;
            objectPool = LF2ObjectPool.Instance;
            if (driver == null || world == null || factory == null ||
                objectPool == null || LF2ReferencePool.Instance == null ||
                world.RuntimeDataCatalog?.IsReady != true ||
                CharacterAnimtorManager.Instance?.SpriteCatalog == null)
            {
                WriteImmediateFailure("Production driver, catalog, factory, or pools are not ready.");
                return;
            }

            report = new ProbeReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
            };
            previousPaused = driver.IsPaused;
            running = true;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                Fail("Play Mode or production world ended before completion.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > TimeoutEditorUpdates)
            {
                Fail("Timed out waiting for a safe production tick boundary.");
                return;
            }

            if (!pauseRequested)
            {
                if (driver.CurrentTickIndex < 5 ||
                    driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                {
                    return;
                }

                driver.SetPaused(true);
                pauseRequested = true;
                CaptureBaseline();
                return;
            }

            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            try
            {
                ExecuteProbe();
                FinishSuccess();
            }
            catch (Exception exception)
            {
                Fail(exception.ToString());
            }
        }

        private static void ExecuteProbe()
        {
            LF2CharacterDataWrapper weaponData =
                world.RuntimeDataCatalog.GetCharacterConfig(State2000Oid);
            LF2CharacterDataWrapper transformData =
                world.RuntimeDataCatalog.GetCharacterConfig(State8xxxOid);
            Require(weaponData?.characterData != null,
                "Official OID150 weapon DAT is not loaded.");
            Require(transformData?.characterData != null,
                "Official OID32 Hunter DAT is not loaded.");
            Require(FindFrame(weaponData.characterData, 0)?.state == 2000,
                "Official OID150 frame0 is not authored state2000.");
            Require(FindFrame(transformData.characterData, 0)?.state == 8032,
                "Official OID32 frame0 is not authored state8032.");

            LF2Entity positive = Spawn(State2000Oid, 0, 120, -120, 210);
            LF2Entity negative = Spawn(State2000Oid, 0, 620, -120, 310);
            LF2Entity transformed = Spawn(State8xxxOid, 0, 360, 0, 260);
            positive.Runtime.SetVelocity(3.0, 0.0, 0.0);
            negative.Runtime.SetVelocity(-3.0, 0.0, 0.0);
            transformed.Runtime.SetVelocity(0.0, 0.0, 0.0);
            positive.SwitchDir("left");
            negative.SwitchDir("right");

            RuntimeEntityHandle transformedHandle = RequireHandle(transformed);
            int firstTick = driver.CurrentTickIndex + 1;
            Require(driver.StepOneTick(
                    FrameInputSet.Empty(firstTick),
                    ignorePaused: true,
                    buildPresentation: true),
                "The first production full tick was rejected.");

            Require(positive.Runtime.Dir == "right" && negative.Runtime.Dir == "left",
                $"state2000 facing mismatch: positive={positive.Runtime.Dir}, negative={negative.Runtime.Dir}.");
            report.positiveFacing = positive.Runtime.Dir;
            report.negativeFacing = negative.Runtime.Dir;

            Require(
                LF2Entity.ResolveCurrentDataObjectId(transformed) == State8xxxOid &&
                transformed.Frame.N == 0 &&
                transformed.Runtime.RenderPicOffset == 140,
                "state8032 did not establish current DAT/frame0/render offset140 after the full tick.");
            int rawPic = transformed.Frame.D.pic;
            int effectivePic = transformed.GetRenderPicIndex();
            Require(effectivePic == rawPic + 140,
                $"state8032 effective pic mismatch: raw={rawPic}, effective={effectivePic}.");

            int secondTick = driver.CurrentTickIndex + 1;
            Require(driver.StepOneTick(
                    FrameInputSet.Empty(secondTick),
                    ignorePaused: true,
                    buildPresentation: true),
                "The second production full tick was rejected.");

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            Require(TryFindSnapshot(frame, transformedHandle, out BattlePresentationEntitySnapshot snapshot),
                "The transformed production entity has no Central snapshot.");
            report.state8xxxCurrentDat = snapshot.CurrentDatObjectId;
            report.state8xxxFrame = transformed.Frame.N;
            report.state8xxxOffset = transformed.Runtime.RenderPicOffset;
            report.state8xxxRawPic = transformed.Frame.D.pic;
            report.state8xxxEffectivePic = snapshot.EffectivePic;
            report.snapshotState = snapshot.State;
            report.snapshotHasCurrentFrame = snapshot.HasCurrentFrame;
            report.snapshotEntityVisible = snapshot.EntityVisible;
            report.snapshotHasCatalogKey = snapshot.HasCatalogKey;
            BattleSpriteCatalog catalog = frame.BoundCatalogForAcceptance;
            BattleSpriteEntry entry = null;
            report.catalogLookupMatched = catalog != null &&
                catalog.TryGet(snapshot.CurrentDatObjectId, snapshot.EffectivePic, out entry);
            world.BattlePresentation.MaterializeCommands(frame, null);
            report.commandCount = frame.CommandCount;
            Require(TryFindEntityCommand(frame, transformedHandle, out BattleRenderCommand command),
                $"The transformed production entity has no Central body command: state={snapshot.State}, " +
                $"visible={snapshot.EntityVisible}, hasFrame={snapshot.HasCurrentFrame}, " +
                $"hasCatalog={snapshot.HasCatalogKey}, dat={snapshot.CurrentDatObjectId}, " +
                $"pic={snapshot.EffectivePic}, commands={frame.CommandCount}.");
            Require(report.catalogLookupMatched,
                "The transformed effective-pic has no bound catalog entry.");
            Require(
                snapshot.CurrentDatObjectId == State8xxxOid &&
                snapshot.EffectivePic == transformed.GetRenderPicIndex() &&
                command.VisualDataId == State8xxxOid &&
                command.EffectivePic == snapshot.EffectivePic &&
                RectApproximately(command.NormalizedUv, entry.NormalizedUv),
                "The Central state8xxx snapshot/command/catalog mapping does not match.");

            report.firstTick = firstTick;
            report.secondTick = secondTick;
            report.centralCommandMatched = true;
        }

        private static LF2Entity Spawn(int oid, int action, int x, int y, int z)
        {
            int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                DynamicSlotStart,
                world.RuntimeSlotCapacityForDiagnostics);
            Require(slot >= DynamicSlotStart, $"No free production runtime slot for OID{oid}.");

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                kind = 1,
                oid = oid,
                action = action,
                facing = 0,
            };
            task.targetWorld = world;
            task.requiredRuntimeSlot = slot;
            task.team = 0;
            task.dir = "right";
            task.preserveActionZero = true;
            task.skipPostInitZOffset = true;
            task.useDirectRuntimePosition = true;
            task.directX = x;
            task.directY = y;
            task.directZ = z;
            LF2Entity entity;
            try
            {
                entity = factory.CreateObjectImmediate(task);
            }
            finally
            {
                LF2ReferencePool.Instance.Recycle(task);
            }

            Require(entity != null && entity.Runtime.SlotIndex == slot,
                $"Production factory failed to create OID{oid} at slot{slot}.");
            entity.ImmediateFrame(action);
            entity.FrameDelay = 0;
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private static LF2FrameData FindFrame(LF2CharacterData data, int frameId)
        {
            if (data?.frames == null)
                return null;
            for (int index = 0; index < data.frames.Count; index++)
            {
                if (data.frames[index]?.frameId == frameId)
                    return data.frames[index];
            }
            return null;
        }

        private static RuntimeEntityHandle RequireHandle(LF2Entity entity)
        {
            Require(world.TryGetCurrentRuntimeHandleForDiagnostics(
                    entity.Runtime.SlotIndex,
                    entity,
                    out RuntimeEntityHandle handle) && handle.IsValid,
                $"OID{entity.ObjectId} has no current runtime handle.");
            return handle;
        }

        private static bool TryFindSnapshot(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattlePresentationEntitySnapshot snapshot)
        {
            if (frame != null)
            {
                for (int index = 0; index < frame.EntityCount; index++)
                {
                    BattlePresentationEntitySnapshot candidate = frame.GetEntity(index);
                    if (candidate.Handle == handle)
                    {
                        snapshot = candidate;
                        return true;
                    }
                }
            }
            snapshot = default;
            return false;
        }

        private static bool TryFindEntityCommand(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattleRenderCommand command)
        {
            if (frame != null)
            {
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand candidate = frame.GetCommand(index);
                    if (candidate.Handle == handle &&
                        candidate.Type == BattleRenderCommandType.Entity)
                    {
                        command = candidate;
                        return true;
                    }
                }
            }
            command = default;
            return false;
        }

        private static bool RectApproximately(Rect left, Rect right)
        {
            return Math.Abs(left.x - right.x) < 0.00001f &&
                   Math.Abs(left.y - right.y) < 0.00001f &&
                   Math.Abs(left.width - right.width) < 0.00001f &&
                   Math.Abs(left.height - right.height) < 0.00001f;
        }

        private static void CaptureBaseline()
        {
            world.GetAllEntities(BaselineEntities);
            baselineObjectCount = world.ObjectCount;
            baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            baselineObjectPoolActive = objectPool.ActiveObjectCountForAcceptance;
            baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineRngState = world.Rng.State;
            baselineRngCalls = world.Rng.CallCount;
            BaselineSounds.Clear();
            BaselineSounds.AddRange(world.PendingSounds);
        }

        private static void Cleanup()
        {
            if (world == null)
                return;
            try
            {
                world.GetAllEntities(EntityScratch);
                for (int index = EntityScratch.Count - 1; index >= 0; index--)
                {
                    LF2Entity entity = EntityScratch[index];
                    if (entity != null && !BaselineEntities.Contains(entity) &&
                        entity.Match == world && entity.Runtime?.SlotIndex >= 0)
                    {
                        entity.FreeEntityLikeExe();
                    }
                }
                world.FlushPendingDestroyForDiagnostics();
                world.PendingSounds.Clear();
                world.PendingSounds.AddRange(BaselineSounds);
                world.Rng.RestoreState(baselineRngState, baselineRngCalls);
                world.RenderDispatchAll(driver?.CurrentTickIndex ?? 0, true);
            }
            catch (Exception exception)
            {
                report.cleanupErrors += exception.Message + ";";
            }
            if (driver != null && EditorApplication.isPlaying)
                driver.SetPaused(previousPaused);
        }

        private static void FinishSuccess()
        {
            report.status = "PASS";
            report.message = "Authored state2000 facing and state8xxx full-tick Central mapping passed.";
            report.endTick = driver.CurrentTickIndex;
            Cleanup();
            CaptureFinalState();
            Require(report.cleanupCompleted, "Cleanup did not restore the production baseline: " + report.cleanupErrors);
            WriteResult(report);
            Debug.Log("[BattleAuthoredStateResidualPlayModeProbe] PASS");
            StopObservation();
        }

        private static void Fail(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.message = message;
            report.endTick = driver?.CurrentTickIndex ?? -1;
            Cleanup();
            CaptureFinalState();
            WriteResult(report);
            Debug.LogError("[BattleAuthoredStateResidualPlayModeProbe] FAIL: " + message);
            StopObservation();
        }

        private static void CaptureFinalState()
        {
            if (report == null)
                return;
            report.finalObjectCount = world?.ObjectCount ?? -1;
            report.finalClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.finalObjectPoolActive = objectPool?.ActiveObjectCountForAcceptance ?? -1;
            report.finalLogicPoolActive = LF2ReferencePool.Instance?.ActiveCount ?? -1;
            report.cleanupCompleted = string.IsNullOrEmpty(report.cleanupErrors) &&
                report.finalObjectCount == baselineObjectCount &&
                report.finalClaimedSlots == baselineClaimedSlots &&
                report.finalObjectPoolActive == baselineObjectPoolActive &&
                report.finalLogicPoolActive == baselineLogicPoolActive;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeReport { status = "FAIL", message = message });
            Debug.LogError("[BattleAuthoredStateResidualPlayModeProbe] FAIL: " + message);
        }

        private static void WriteResult(ProbeReport value)
        {
            string path = ProjectPath(ResultRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
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
            factory = null;
            objectPool = null;
            report = null;
            previousPaused = false;
            running = false;
            pauseRequested = false;
            editorUpdates = 0;
            baselineObjectCount = 0;
            baselineClaimedSlots = 0;
            baselineObjectPoolActive = 0;
            baselineLogicPoolActive = 0;
            baselineRngState = 0;
            baselineRngCalls = 0;
            BaselineEntities.Clear();
            EntityScratch.Clear();
            BaselineSounds.Clear();
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public int startTick;
            public int endTick;
            public int firstTick;
            public int secondTick;
            public string positiveFacing = string.Empty;
            public string negativeFacing = string.Empty;
            public int state8xxxCurrentDat;
            public int state8xxxFrame;
            public int state8xxxOffset;
            public int state8xxxRawPic;
            public int state8xxxEffectivePic;
            public int snapshotState;
            public bool snapshotHasCurrentFrame;
            public bool snapshotEntityVisible;
            public bool snapshotHasCatalogKey;
            public bool catalogLookupMatched;
            public int commandCount;
            public bool centralCommandMatched;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
        }
    }
}
#endif
