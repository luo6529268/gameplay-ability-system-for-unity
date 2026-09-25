#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q07NarutoHeldAirActionProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_NarutoHeldAirAction.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-NARUTO-HELD-AIR-ACTION-001";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const int CaptureWidth = 960;
        private static DateTime startedUtc;
        private static bool pauseRequested;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public bool capturePresentation;
            public bool captureCamera;
            public bool naturalPickup;
        }

        [Serializable]
        private sealed class TickRow
        {
            public int tick;
            public string input;
            public int action;
            public int state;
            public int pic;
            public int y;
            public int holderLink;
            public int weaponLink;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public string scope;
            public string contentRoot;
            public string runId;
            public int startTick;
            public int endTick;
            public int holderSlot;
            public int weaponSlot;
            public int rosterSlot;
            public int objectCountBefore;
            public int objectCountAfter;
            public int claimedSlotsBefore;
            public int claimedSlotsAfter;
            public int poolBefore;
            public int poolAfter;
            public bool pickupAccepted;
            public bool naturalPickupRequested;
            public int pickupAttackAttempts;
            public List<int> pickupAttackActions = new List<int>(8);
            public int holderLinkAfterPickup;
            public int weaponLinkAfterPickup;
            public bool heldReferenceMatchesAfterPickup;
            public int targetSlotAfterPickup;
            public int weaponHolderSlotAfterPickup;
            public bool reachedAirborne;
            public bool reachedAction30;
            public bool fixtureUnregistered;
            public bool rosterRestored;
            public bool catalogFound;
            public string catalogSourcePath;
            public Rect catalogPixelRect;
            public bool catalogCentralBindingValid;
            public int publicationTick;
            public int publicationCommandCount;
            public int matchingEntityCommands;
            public int publishedStableId;
            public int publishedVisualDataId;
            public int publishedPic;
            public Vector2 publishedSize;
            public bool submissionAcquired;
            public string cameraPngPath;
            public int cameraRoiX;
            public int cameraRoiY;
            public int cameraRoiWidth;
            public int cameraRoiHeight;
            public int cameraRoiNonClearPixels;
            public bool cameraRestored;
            public List<TickRow> ticks = new List<TickRow>(48);
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            string path = ProjectPath(RequestPath);
            if (!File.Exists(path))
            {
                Reset();
                return;
            }
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
                Reset();
                return;
            }
            if (string.IsNullOrEmpty(request.runId) ||
                !request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'))
            {
                Finish(request, new Report { status = "FAIL", message = "Invalid runId." });
                return;
            }
            if (startedUtc == default)
                startedUtc = DateTime.UtcNow;
            if (DateTime.UtcNow - startedUtc > TimeSpan.FromMinutes(5))
            {
                Finish(request, new Report { status = "FAIL", message = "Timed out." });
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5)
                return;
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Finish(request, new Report { status = "FAIL", message =
                    driver.DedicatedSimulationWorkerFailureForDiagnostics.ToString() });
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (!pauseRequested)
            {
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
            Run(request, driver, world);
        }

        private static void Run(Request request, SimulationTickDriver driver,
            SimulationWorld world)
        {
            LF2Character holder = null;
            LF2Weapon weapon = null;
            BattleSlotRuntimeState originalRoster = null;
            int rosterSlot = -1;
            LF2ObjectPool pool = LF2ObjectPool.Instance;
            var report = new Report
            {
                runId = request.runId,
                scope = "Formal OID2/OID120 production kind2 pickup plus discrete Driver input; no physical keyboard or formal EXE pixels",
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot,
                startTick = driver.CurrentTickIndex,
                objectCountBefore = world.ObjectCount,
                claimedSlotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics,
                poolBefore = pool.ActiveObjectCountForAcceptance,
                holderSlot = -1,
                weaponSlot = -1,
                rosterSlot = -1,
            };
            try
            {
                Require(SceneManager.GetActiveScene().name == "NTSD_Battle", "Wrong active Scene.");
                Require(report.contentRoot == FormalRoot, "Formal content root is not selected.");
                var holderData = world.RuntimeCharacterConfigs.Resolve(2);
                var weaponData = world.RuntimeCharacterConfigs.Resolve(120);
                Require(holderData?.characterData != null &&
                    weaponData?.characterData != null, "Formal OID2/OID120 catalog is unavailable.");
                report.holderSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                report.weaponSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    report.holderSlot + 1, 1000);
                Require(report.holderSlot >= 50 && report.weaponSlot > report.holderSlot,
                    "No two free runtime slots.");

                holder = new LF2Character();
                holder.ModuleInitialize();
                holder.ObjectId = 2;
                holder.Name = "Q07NarutoHeldAirHolder";
                holder.FrameCache.Load(holderData);
                holder.SetRequiredRuntimeSlot(report.holderSlot);
                world.Register(holder);
                holder.ImmediateFrame(0);
                holder.Initialize(500, 500);
                holder.ClearBattleEntryInputState();
                NTSD28NativeComboStateMachine.InitializeNativeHistory(holder.Runtime);
                holder.AiControlled = false;
                holder.Team = 3;
                holder.RelationTeam = 3;
                holder.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                holder.Runtime.SyncIntegerPosition();

                weapon = new LF2Weapon();
                weapon.ObjectId = 120;
                weapon.Name = "Q07NarutoHeldAirWeapon";
                weapon.SetWeaponType(1);
                weapon.FrameCache.Load(weaponData);
                weapon.SetRequiredRuntimeSlot(report.weaponSlot);
                world.Register(weapon);
                weapon.ImmediateFrame(weaponData.characterData.frames.First(
                    frame => frame.state == LF2States.WeaponOnGround).frameId);
                weapon.Health.HP = 100;
                weapon.Runtime.SetPosition(190, 0, world.Runtime.Stage.ZMin + 50);
                weapon.Runtime.SyncIntegerPosition();

                rosterSlot = Array.FindIndex(world.Runtime.Roster.Slots,
                    slot => slot == null || !slot.Active);
                Require(rosterSlot >= 0, "No free human roster slot.");
                originalRoster = world.Runtime.Roster.Slots[rosterSlot];
                world.Runtime.Roster.Slots[rosterSlot] = new BattleSlotRuntimeState
                {
                    Active = true, IsHuman = true, CharacterId = 2, Team = 3,
                    RuntimeSlotIndex = holder.Runtime.SlotIndex,
                    StableId = holder.Runtime.StableId,
                };
                report.rosterSlot = rosterSlot;

                report.naturalPickupRequested = request.naturalPickup;
                if (request.naturalPickup)
                {
                    for (int attempt = 0;
                        attempt < 4 && holder.GetHeldWeapon() != weapon;
                        attempt++)
                    {
                        for (int wait = 0; wait < 60 && holder.Frame.N != 0; wait++)
                            Tick(driver, report, holder, weapon, rosterSlot,
                                "natural-pickup-return", SimulationInputButtons.None);
                        Require(holder.Frame.N == 0,
                            "Standing did not return before natural pickup attack.");
                        report.pickupAttackAttempts++;
                        for (int heldTick = 0;
                            heldTick < 2 && holder.GetHeldWeapon() != weapon;
                            heldTick++)
                        {
                            Tick(driver, report, holder, weapon, rosterSlot,
                                "natural-pickup-attack", SimulationInputButtons.Jump);
                            report.pickupAttackActions.Add(holder.Frame.N);
                        }
                    }
                    report.pickupAccepted = holder.GetHeldWeapon() == weapon;
                }
                else
                {
                    InteractionArea pickup = holderData.characterData.frames.First(
                        frame => frame.frameId == 60).itrs.First(itr => itr.kind == 2);
                    report.pickupAccepted = new LF2CharacterInteractionResolver(holder)
                        .TryApplyPreInteraction(pickup, weapon);
                }
                report.holderLinkAfterPickup = holder.Runtime.LinkState;
                report.weaponLinkAfterPickup = weapon.Runtime.LinkState;
                report.heldReferenceMatchesAfterPickup = holder.GetHeldWeapon() == weapon;
                report.targetSlotAfterPickup = holder.Runtime.TargetSlotIndex;
                report.weaponHolderSlotAfterPickup = weapon.Runtime.HolderStableId;
                Require(report.pickupAccepted && holder.Runtime.LinkState % 100 == 1 &&
                    weapon.Runtime.LinkState == -1 && holder.GetHeldWeapon() == weapon &&
                    holder.Runtime.TargetSlotIndex == weapon.Runtime.SlotIndex &&
                    weapon.Runtime.HolderStableId == holder.Runtime.SlotIndex,
                    "Production pickup relation did not bind OID120.");
                for (int i = 0; i < 30 && holder.Frame.N != 0; i++)
                    Tick(driver, report, holder, weapon, rosterSlot,
                        "pickup-return", SimulationInputButtons.None);
                Require(holder.Frame.N == 0, "Pickup did not return to standing.");

                Tick(driver, report, holder, weapon, rosterSlot,
                    "native-jump", SimulationInputButtons.Defend);
                for (int i = 0; i < 20 && !report.reachedAirborne; i++)
                {
                    report.reachedAirborne = holder.Frame.D?.state == LF2States.Jump &&
                        holder.Runtime.YInt < 0;
                    if (!report.reachedAirborne)
                        Tick(driver, report, holder, weapon, rosterSlot,
                            "air-wait", SimulationInputButtons.None);
                }
                Require(report.reachedAirborne, "Jump input did not reach airborne state4.");
                for (int i = 0; i < 3 && !report.reachedAction30; i++)
                {
                    Tick(driver, report, holder, weapon, rosterSlot,
                        "native-air-attack", SimulationInputButtons.Jump,
                        request.capturePresentation || request.captureCamera);
                    report.reachedAction30 = holder.Frame.N == 30;
                }
                Require(report.reachedAction30, "Held airborne attack did not reach action30.");
                Require(holder.Frame.D?.pic == 97, "Formal Naruto action30 pic is not 97.");
                if (request.capturePresentation || request.captureCamera)
                    CapturePublication(world, driver, holder, report,
                        request.captureCamera, request.runId);
                report.status = "PASS";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                if (rosterSlot >= 0)
                {
                    world.Runtime.Roster.Slots[rosterSlot] = originalRoster;
                    report.rosterRestored = ReferenceEquals(
                        world.Runtime.Roster.Slots[rosterSlot], originalRoster);
                }
                if (weapon?.RegisteredWorldForSimulation == world)
                    world.Unregister(weapon);
                if (holder?.RegisteredWorldForSimulation == world)
                    world.Unregister(holder);
                report.fixtureUnregistered =
                    (weapon == null || weapon.RegisteredWorldForSimulation == null) &&
                    (holder == null || holder.RegisteredWorldForSimulation == null);
                report.endTick = driver.CurrentTickIndex;
                report.objectCountAfter = world.ObjectCount;
                report.claimedSlotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                report.poolAfter = pool.ActiveObjectCountForAcceptance;
                if (!report.fixtureUnregistered || !report.rosterRestored ||
                    report.objectCountAfter != report.objectCountBefore ||
                    report.claimedSlotsAfter != report.claimedSlotsBefore ||
                    report.poolAfter != report.poolBefore)
                {
                    report.status = "FAIL";
                    report.message += " Cleanup count or binding mismatch.";
                }
                Finish(request, report);
            }
        }

        private static void Tick(SimulationTickDriver driver, Report report,
            LF2Character holder, LF2Weapon weapon, int rosterSlot,
            string label, SimulationInputButtons buttons,
            bool buildPresentation = false)
        {
            int tick = driver.CurrentTickIndex + 1;
            var frame = new FrameInputSet(tick,
                new[] { new SimulationPlayerInput(rosterSlot, buttons) });
            Require(driver.StepOneTick(frame, ignorePaused: true,
                buildPresentation: buildPresentation), "Driver rejected tick " + tick);
            report.ticks.Add(new TickRow
            {
                tick = tick,
                input = label + ":" + buttons,
                action = holder.Frame.N,
                state = holder.Frame.D?.state ?? -1,
                pic = holder.Frame.D?.pic ?? -1,
                y = holder.Runtime.YInt,
                holderLink = holder.Runtime.LinkState,
                weaponLink = weapon.Runtime.LinkState,
            });
        }

        private static void CapturePublication(SimulationWorld world,
            SimulationTickDriver driver, LF2Character holder, Report report,
            bool captureCamera, string runId)
        {
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            int visualDataId = LF2Entity.ResolveCurrentDataObjectId(holder);
            int effectivePic = holder.GetRenderPicIndex();
            BattleSpriteEntry entry = null;
            report.catalogFound = manager != null &&
                manager.TryGetSpriteEntry(visualDataId, effectivePic,
                    out entry);
            Require(report.catalogFound && entry != null,
                "Reached action30/pic97 has no production sprite catalog entry.");
            report.catalogSourcePath = entry.SourceSheetPath;
            report.catalogPixelRect = entry.PixelRect;
            report.catalogCentralBindingValid = entry.CentralBinding.IsValid;
            Require(visualDataId == 2 && effectivePic == 97 &&
                entry.PixelRect == new Rect(560f, 801f, 79f, 79f) &&
                report.catalogCentralBindingValid,
                "Formal Naruto pic97 catalog binding differs from the source cell.");

            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
            Require(plan.IsValid && !plan.IsStale &&
                plan.Owner == BattlePixelFrameOwner.Central &&
                plan.SimulationTick == driver.CurrentTickIndex &&
                plan.CapturedFrame?.CommandsMaterialized == true &&
                plan.Submission != null,
                "Current CentralOnly presentation plan was not materialized.");
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null, "Battle world camera is unavailable.");
            report.submissionAcquired =
                BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    camera, CameraRenderType.Base, camera.cameraType, true,
                    out BattleCentralSubmission.BattleCentralSubmissionLease lease);
            Require(report.submissionAcquired,
                "Current central submission could not be acquired.");
            BattleRenderCommand targetCommand = default;
            using (lease)
            {
                BattlePresentationFrame frame = plan.CapturedFrame;
                report.publicationTick = plan.SimulationTick;
                report.publicationCommandCount = frame.CommandCount;
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (command.Type != BattleRenderCommandType.Entity ||
                        command.StableId != holder.Runtime.StableId ||
                        command.VisualDataId != 2 || command.EffectivePic != 97)
                        continue;
                    report.matchingEntityCommands++;
                    report.publishedStableId = command.StableId;
                    report.publishedVisualDataId = command.VisualDataId;
                    report.publishedPic = command.EffectivePic;
                    report.publishedSize = command.Size;
                    targetCommand = command;
                }
            }
            Require(report.matchingEntityCommands == 1 &&
                report.publishedSize == new Vector2(79f, 79f),
                "Expected exactly one 79x79 Naruto pic97 central entity command.");
            if (captureCamera)
                CaptureCamera(camera, targetCommand, runId, report);
        }

        private static void CaptureCamera(Camera camera,
            BattleRenderCommand command, string runId, Report report)
        {
            int height = Mathf.Max(1, Mathf.RoundToInt(CaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            float widthWorld = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                               NTSDRenderSpace.BattleVisualScale;
            float heightWorld = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * widthWorld;
            float bottom = command.Position.y - command.Pivot.y * heightWorld;
            Vector3 lower = camera.WorldToViewportPoint(
                new Vector3(left, bottom, command.Position.z));
            Vector3 upper = camera.WorldToViewportPoint(new Vector3(
                left + widthWorld, bottom + heightWorld, command.Position.z));
            int x0 = Mathf.Clamp(Mathf.FloorToInt(
                Mathf.Min(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(
                Mathf.Max(lower.x, upper.x) * CaptureWidth), 0, CaptureWidth);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(
                Mathf.Min(lower.y, upper.y) * height), 0, height);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(
                Mathf.Max(lower.y, upper.y) * height), 0, height);
            report.cameraRoiX = x0;
            report.cameraRoiY = y0;
            report.cameraRoiWidth = x1 - x0;
            report.cameraRoiHeight = y1 - y0;
            Require(report.cameraRoiWidth > 0 && report.cameraRoiHeight > 0,
                "Naruto pic97 command projects outside the world camera.");

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
                readback = new Texture2D(CaptureWidth, height,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, CaptureWidth, height),
                    0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++)
                    {
                        Color32 pixel = pixels[y * CaptureWidth + x];
                        if (pixel.r != 0 || pixel.g != 0 || pixel.b != 0)
                            report.cameraRoiNonClearPixels++;
                    }
                report.cameraPngPath = ResultRoot + "/" + runId + ".png";
                string output = ProjectPath(report.cameraPngPath);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, readback.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                saved.Restore(camera);
                report.cameraRestored = saved.Matches(camera) &&
                                        RenderTexture.active == previousActive;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
            Require(report.cameraRestored &&
                report.cameraRoiNonClearPixels > 0,
                "Naruto pic97 camera ROI has no nonclear pixel or camera was not restored.");
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

            public bool Matches(Camera camera)
            {
                return camera.targetTexture == targetTexture &&
                       camera.cullingMask == cullingMask &&
                       camera.clearFlags == clearFlags &&
                       camera.backgroundColor == backgroundColor &&
                       camera.allowHDR == allowHdr &&
                       camera.allowMSAA == allowMsaa;
            }
        }

        private static void Finish(Request request, Report report)
        {
            string path = ProjectPath(ResultRoot + "/" + request.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path))
                File.WriteAllText(path, JsonUtility.ToJson(report, true));
            request.requested = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
            Reset();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Reset()
        {
            startedUtc = default;
            pauseRequested = false;
            stableTick = -1;
            stableUpdates = 0;
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
#endif
