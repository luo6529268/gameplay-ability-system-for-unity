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
    public static class NTSD28C10CollisionActionSnapshotPlacementPlayModeProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_B3_C10_CollisionActionSnapshotPlacement.request";
        private const string ResultPath =
            "Temp/NTSD28_B3_C10_CollisionActionSnapshotPlacement.result.json";

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

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C10 Collision Action Snapshot Placement Play Probe")]
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
            SnapshotObserver probe = null;
            try
            {
                driver.SetPaused(true);
                int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                Require(slot >= 50, "No transient runtime slot is available.");
                probe = CreateProbe(world, slot, 11050);

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the C10 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.slot = slot;
                report.postSerialCount = probe.PostSerialCount;
                report.currentFrame = probe.Frame.N;
                report.collisionSnapshotFrame = probe.Frame.Prev2;
                report.runtimeCollisionSnapshotFrame = probe.Runtime.PrevFrame2;
                report.attackExemptSeenInSerial = probe.ObservedAttackExemptInSerial;
                report.finalAttackExempt = probe.AttackExempt;

                Require(driver.CurrentTickIndex == expectedTick,
                    "Production tick index did not advance exactly once.");
                Require(probe.PostSerialCount == 1 && probe.Frame.N == 5,
                    "Serial remainder did not publish current frame 5 exactly once.");
                Require(probe.Frame.Prev2 == 0 && probe.Runtime.PrevFrame2 == 0,
                    "C10 did not freeze action 0 before serial changed current frame to 5.");
                Require(probe.ObservedAttackExemptInSerial == 0 &&
                        probe.AttackExempt == 0,
                    "C11 rest prelude was not completed before serial remainder.");

                report.status = "PASS";
                report.message =
                    "Production C10 froze action and C11 rest prelude completed before serial remainder.";
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

        private static SnapshotObserver CreateProbe(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var data = new LF2CharacterData
            {
                name = "C10Play_" + objectId,
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { Frame(0), Frame(5) },
            };
            var probe = new SnapshotObserver();
            probe.Name = data.name;
            probe.ObjectId = objectId;
            probe.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.Runtime.Frame = 0;
            probe.AttackExempt = 3;
            probe.ItrRest.Arest = 3;
            probe.SetRequiredRuntimeSlot(slot);
            world.Register(probe);
            return probe;
        }

        private static LF2FrameData Frame(int id)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = 0,
                wait = 100,
                next = id,
                itrs = new List<InteractionArea>(),
            };
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
                Debug.Log("[NTSD28C10CollisionActionSnapshotPlacementPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C10CollisionActionSnapshotPlacementPlayProbe] " + report.message);
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

        private sealed class SnapshotObserver : LF2OtherObject
        {
            internal int PostSerialCount { get; private set; }
            internal int ObservedAttackExemptInSerial { get; private set; }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedAttackExemptInSerial = AttackExempt;
                Frame.N = 5;
                Frame.D = FrameCache.GetFrameDataById(5);
                Runtime.Frame = 5;
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
            public int currentFrame;
            public int collisionSnapshotFrame;
            public int runtimeCollisionSnapshotFrame;
            public int attackExemptSeenInSerial;
            public int finalAttackExempt;
            public bool cleanupPassed;
        }
    }
}
#endif
