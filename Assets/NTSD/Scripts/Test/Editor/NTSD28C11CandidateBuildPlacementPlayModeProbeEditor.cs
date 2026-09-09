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
    public static class NTSD28C11CandidateBuildPlacementPlayModeProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_B3_C11_CandidateBuildPlacement.request";
        private const string ResultPath = "Temp/NTSD28_B3_C11_CandidateBuildPlacement.result.json";

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

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C11 Candidate Build Placement Play Probe")]
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
            C11Observer probe = null;
            try
            {
                driver.SetPaused(true);
                int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                Require(slot >= 50, "No transient runtime slot is available.");
                probe = CreateProbe(world, slot, 11150);
                probe.AttackExempt = 3;
                probe.ItrRest.Arest = 3;

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the C11 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.slot = slot;
                report.postSerialCount = probe.PostSerialCount;
                report.attackExemptInSerial = probe.ObservedAttackExemptInSerial;
                report.arestInSerial = probe.ObservedArestInSerial;
                report.pairVisitCountInSerial = probe.ObservedPairVisitCountInSerial;
                Require(driver.CurrentTickIndex == expectedTick,
                    "Production tick index did not advance exactly once.");
                Require(probe.PostSerialCount == 1 &&
                        probe.ObservedAttackExemptInSerial == 0 &&
                        probe.ObservedArestInSerial == 0 &&
                        probe.ObservedPairVisitCountInSerial >= 1,
                    "Serial remainder did not observe the completed C11 rest/pair transaction.");

                report.status = "PASS";
                report.message = "Production C11 rest/pair/candidate transaction completed before serial remainder.";
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
                report.cleanupPassed = world.ObjectCount == baselineObjects &&
                    world.ClaimedRuntimeSlotCountForDiagnostics == baselineClaimed;
                if (!report.cleanupPassed)
                {
                    report.status = "FAIL";
                    report.message += " Cleanup did not restore object/slot counts.";
                }
                Write(report);
            }
        }

        private static C11Observer CreateProbe(SimulationWorld world, int slot, int objectId)
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
                name = "C11Play_" + objectId,
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { frame },
            };
            var probe = new C11Observer(world);
            probe.Name = data.name;
            probe.ObjectId = objectId;
            probe.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.SetRequiredRuntimeSlot(slot);
            world.Register(probe);
            return probe;
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
                Debug.Log("[NTSD28C11CandidateBuildPlacementPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C11CandidateBuildPlacementPlayProbe] " + report.message);
        }

        private static void ExitPlayModeAfterRequest()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private sealed class C11Observer : LF2OtherObject
        {
            private readonly SimulationWorld world;

            internal C11Observer(SimulationWorld world)
            {
                this.world = world;
            }

            internal int PostSerialCount { get; private set; }
            internal int ObservedAttackExemptInSerial { get; private set; }
            internal int ObservedArestInSerial { get; private set; }
            internal int ObservedPairVisitCountInSerial { get; private set; }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex) => true;

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedAttackExemptInSerial = AttackExempt;
                ObservedArestInSerial = ItrRest.Arest;
                ObservedPairVisitCountInSerial = world.LastCollisionPairVRestEligibilityVisitCount;
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
            public int postSerialCount;
            public int attackExemptInSerial;
            public int arestInSerial;
            public int pairVisitCountInSerial;
            public bool cleanupPassed;
        }
    }
}
#endif
