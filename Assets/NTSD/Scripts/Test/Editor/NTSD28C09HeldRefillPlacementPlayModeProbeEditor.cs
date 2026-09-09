#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class NTSD28C09HeldRefillPlacementPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/B3/Run C09 Held Refill Placement Play Probe";
        private const string RequestPath =
            "Temp/NTSD28_B3_C09_HeldRefillPlacement.request";
        private const string ResultPath =
            "Temp/NTSD28_B3_C09_HeldRefillPlacement.result.json";

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string requestPath = ProjectPath(RequestPath);
            if (!File.Exists(requestPath))
                return;

            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            SimulationTickDriver current = SimulationTickDriver.Instance;
            if (current?.World == null || current.CurrentTickIndex < 5)
                return;

            File.Delete(requestPath);
            string resultPath = ProjectPath(ResultPath);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
            Run();
            EditorApplication.delayCall += ExitPlayModeAfterRequest;
        }

        [MenuItem(MenuPath)]
        public static void Run()
        {
            var report = new Report();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                report.status = "FAIL";
                report.message = "Play Mode production world is unavailable.";
                Write(report);
                return;
            }

            bool previousPaused = driver.IsPaused;
            int baselineObjects = world.ObjectCount;
            int baselineClaimed = world.ClaimedRuntimeSlotCountForDiagnostics;
            PassiveHolder holder = null;
            SerialObserverHeld child = null;
            try
            {
                driver.SetPaused(true);
                int holderSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                int childSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    holderSlot + 1,
                    1000);
                Require(holderSlot >= 50 && childSlot > holderSlot,
                    "No two ordered transient runtime slots are available.");

                CreateHeldFixture(
                    world,
                    holderSlot,
                    childSlot,
                    out holder,
                    out child);
                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the C09 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.holderSlot = holderSlot;
                report.childSlot = childSlot;
                report.postSerialCount = child.PostSerialCount;
                report.observedFrameInSerial = child.ObservedFrameInSerial;
                report.observedXInSerial = child.ObservedXInSerial;
                report.observedYInSerial = child.ObservedYInSerial;
                report.observedZInSerial = child.ObservedZInSerial;
                report.finalFrame = child.Frame.N;
                report.finalX = child.Runtime.XInt;
                report.finalY = child.Runtime.YInt;
                report.finalZ = child.Runtime.ZInt;

                Require(driver.CurrentTickIndex == expectedTick,
                    "Production tick index did not advance exactly once.");
                Require(child.PostSerialCount == 1,
                    "Production serial remainder did not observe the held child exactly once.");
                Require(child.ObservedFrameInSerial == 5,
                    $"Serial remainder observed held frame {child.ObservedFrameInSerial}, expected C09 frame 5.");
                Require(child.ObservedXInSerial == 95 &&
                        child.ObservedYInSerial == 47 &&
                        child.ObservedZInSerial == 199,
                    $"Serial remainder observed unexpected C09 held pose ({child.ObservedXInSerial},{child.ObservedYInSerial},{child.ObservedZInSerial}).");

                report.status = "PASS";
                report.message =
                    "Production C09 published the first held-frame/pose write before serial remainder.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                if (child != null)
                    world.Unregister(child);
                if (holder != null)
                    world.Unregister(holder);
                driver.SetPaused(previousPaused);
                report.cleanupPassed =
                    world.ObjectCount == baselineObjects &&
                    world.ClaimedRuntimeSlotCountForDiagnostics == baselineClaimed;
                if (!report.cleanupPassed)
                {
                    report.status = "FAIL";
                    report.message += " Cleanup did not restore object/slot counts.";
                }
                Write(report);
            }
        }

        private static void CreateHeldFixture(
            SimulationWorld world,
            int holderSlot,
            int childSlot,
            out PassiveHolder holder,
            out SerialObserverHeld child)
        {
            LF2FrameData holderFrame = Frame(0, 20, 30);
            holderFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint
                {
                    x = 10,
                    y = 20,
                    weaponact = 5,
                    cover = 2,
                },
            };
            var holderData = new LF2CharacterData
            {
                name = "C09PlayHolder",
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { holderFrame },
            };

            LF2FrameData childFrame0 = Frame(0, 8, 10);
            childFrame0.wpoints = new List<WeaponPoint> { new WeaponPoint() };
            LF2FrameData childFrame5 = Frame(5, 8, 10);
            childFrame5.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { x = 3, y = 4 },
            };
            var childData = new LF2CharacterData
            {
                name = "C09PlayHeld",
                type_sub = (int)LF2ObjectType.LightWeapon,
                frames = new List<LF2FrameData> { childFrame0, childFrame5 },
            };

            holder = new PassiveHolder();
            Bind(holder, holderSlot, 10950, holderData);
            world.Register(holder);
            holder.Runtime.SetPosition(100.0, 50.0, 200.0);
            holder.Runtime.SyncIntegerPosition();
            holder.SwitchDir("right");

            child = new SerialObserverHeld();
            Bind(child, childSlot, 10951, childData);
            world.Register(child);

            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = childSlot;
            holder.Runtime.HeldWeaponStableId = childSlot;
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = holderSlot;
        }

        private static LF2FrameData Frame(int id, int centerX, int centerY)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = 0,
                wait = 100,
                next = id,
                centerx = centerX,
                centery = centerY,
                itrs = new List<InteractionArea>(),
            };
        }

        private static void Bind(
            LF2Entity entity,
            int slot,
            int objectId,
            LF2CharacterData data)
        {
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.SetRequiredRuntimeSlot(slot);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Write(Report report)
        {
            File.WriteAllText(ProjectPath(ResultPath), JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C09HeldRefillPlacementPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C09HeldRefillPlacementPlayProbe] " + report.message);
        }

        private static void ExitPlayModeAfterRequest()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                relativePath));
        }

        private sealed class PassiveHolder : LF2OtherObject
        {
            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }
        }

        private sealed class SerialObserverHeld : LF2OtherObject
        {
            internal int PostSerialCount { get; private set; }
            internal int ObservedFrameInSerial { get; private set; } = -1;
            internal int ObservedXInSerial { get; private set; }
            internal int ObservedYInSerial { get; private set; }
            internal int ObservedZInSerial { get; private set; }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedFrameInSerial = Frame.N;
                ObservedXInSerial = Runtime.XInt;
                ObservedYInSerial = Runtime.YInt;
                ObservedZInSerial = Runtime.ZInt;
            }
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int holderSlot;
            public int childSlot;
            public int postSerialCount;
            public int observedFrameInSerial;
            public int observedXInSerial;
            public int observedYInSerial;
            public int observedZInSerial;
            public int finalFrame;
            public int finalX;
            public int finalY;
            public int finalZ;
            public bool cleanupPassed;
        }
    }
}
#endif
