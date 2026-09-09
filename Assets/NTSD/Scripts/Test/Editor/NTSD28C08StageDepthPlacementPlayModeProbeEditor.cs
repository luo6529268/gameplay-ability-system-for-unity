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
    public static class NTSD28C08StageDepthPlacementPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/B3/Run C08 Stage Depth Placement Play Probe";
        private const string ResultPath =
            "Temp/NTSD28_B3_C08_StageDepthPlacement.result.json";
        private const string RequestPath =
            "Temp/NTSD28_B3_C08_StageDepthPlacement.request";

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
            StageObserverCharacter probe = null;
            try
            {
                driver.SetPaused(true);
                int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                Require(slot >= 50, "No transient runtime slot is available.");

                int stageZMin = world.Runtime?.Stage?.ZMin ?? 180;
                int stageZMax = world.Runtime?.Stage?.ZMax ?? 350;
                Require(stageZMax >= stageZMin,
                    $"Production stage Z range is invalid: {stageZMin}..{stageZMax}.");
                int initialZ = stageZMax + 150;
                probe = CreateProbe(world, slot, 10850, initialZ);

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the C08 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.slot = slot;
                report.stageZMin = stageZMin;
                report.stageZMax = stageZMax;
                report.initialZ = initialZ;
                report.postSerialCount = probe.PostSerialCount;
                report.observedZInSerial = probe.ObservedZInSerial;
                report.finalZ = probe.Runtime.Z;
                report.finalZInt = probe.Runtime.ZInt;

                Require(driver.CurrentTickIndex == expectedTick,
                    "Production tick index did not advance exactly once.");
                Require(probe.PostSerialCount == 1,
                    "Production serial remainder did not observe the probe exactly once.");
                Require(probe.ObservedZInSerial == stageZMax,
                    $"Serial remainder observed Z={probe.ObservedZInSerial}, expected clamped Z={stageZMax}.");
                Require(probe.Runtime.Z == stageZMax && probe.Runtime.ZInt == stageZMax,
                    "Production tick did not preserve the first C08 depth clamp.");

                report.status = "PASS";
                report.message =
                    "Production C08 clamped the active type-0 character before serial remainder.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                if (probe != null)
                    world.Unregister(probe);
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

        private static StageObserverCharacter CreateProbe(
            SimulationWorld world,
            int slot,
            int objectId,
            double z)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>(),
            };
            var data = new LF2CharacterData
            {
                name = "C08Play_" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            var probe = new StageObserverCharacter();
            probe.ModuleInitialize();
            probe.Name = data.name;
            probe.ObjectId = objectId;
            probe.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.Initialize(500, 500);
            probe.SetRequiredRuntimeSlot(slot);
            world.Register(probe);
            probe.PS.z = z;
            probe.Runtime.SetPosition(0.0, 0.0, z);
            probe.Runtime.SyncIntegerPosition();
            return probe;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Write(Report report)
        {
            string path = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                ResultPath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C08StageDepthPlacementPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C08StageDepthPlacementPlayProbe] " + report.message);
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

        private sealed class StageObserverCharacter : LF2Character
        {
            internal int PostSerialCount { get; private set; }
            internal double ObservedZInSerial { get; private set; }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedZInSerial = Runtime.Z;
            }
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int slot;
            public int stageZMin;
            public int stageZMax;
            public int initialZ;
            public int postSerialCount;
            public double observedZInSerial;
            public double finalZ;
            public int finalZInt;
            public bool cleanupPassed;
        }
    }
}
#endif
