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
    public static class NTSD28C13ActiveWeaponCountPlacementPlayModeProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_B3_C13_ActiveWeaponCountPlacement.request";
        private const string ResultPath =
            "Temp/NTSD28_B3_C13_ActiveWeaponCountPlacement.result.json";

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
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;

            File.Delete(requestPath);
            string resultPath = ProjectPath(ResultPath);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
            Run();
            EditorApplication.delayCall += ExitPlayModeAfterRequest;
        }

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C13 Active Weapon Count Placement Play Probe")]
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
            int baselineClaimed =
                world.ClaimedRuntimeSlotCountForDiagnostics;
            var added = new List<TypedCharacter>(7);
            try
            {
                driver.SetPaused(true);
                report.baselineActiveWeaponCount = CountExactWeapons(world);

                CountObserver observer = Create<CountObserver>(
                    world,
                    13200,
                    0,
                    FindFreeSlot(world),
                    100);
                added.Add(observer);
                for (int type = 1; type <= 6; type++)
                {
                    added.Add(Create<PassiveCharacter>(
                        world,
                        13200 + type,
                        type,
                        FindFreeSlot(world),
                        120 + type * 20));
                }

                report.expectedCount = report.baselineActiveWeaponCount + 4;
                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(
                        ignorePaused: true,
                        buildPresentation: false),
                    "Production driver rejected the C13 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.postSerialCount = observer.PostSerialCount;
                report.observedCountInSerial =
                    observer.ObservedCountInSerial;
                report.observedCapturedTickInSerial =
                    observer.ObservedCapturedTickInSerial;
                report.finalSnapshotCount =
                    world.ActiveWeaponObjectCountBeforeHitsForDiagnostics;
                report.finalCapturedTick =
                    world.ActiveWeaponObjectCountCapturedTickForDiagnostics;
                report.finalCurrentExactCount = CountExactWeapons(world);

                Require(driver.CurrentTickIndex == expectedTick &&
                        observer.PostSerialCount == 1 &&
                        observer.ObservedCountInSerial == report.expectedCount &&
                        observer.ObservedCapturedTickInSerial == expectedTick &&
                        report.finalSnapshotCount == report.expectedCount &&
                        report.finalCapturedTick == expectedTick &&
                        report.finalCurrentExactCount == report.expectedCount,
                    "Serial remainder did not observe the completed C13 active weapon count snapshot.");

                report.status = "PASS";
                report.message =
                    "Production C13 counted exact active types 1/2/4/6 before serial remainder.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                for (int index = added.Count - 1; index >= 0; index--)
                    world.Unregister(added[index]);
                driver.SetPaused(previousPaused);
                report.cleanupPassed =
                    world.ObjectCount == baselineObjects &&
                    world.ClaimedRuntimeSlotCountForDiagnostics ==
                        baselineClaimed;
                if (!report.cleanupPassed)
                {
                    report.status = "FAIL";
                    report.message +=
                        " Cleanup did not restore object/slot counts.";
                }
                Write(report);
            }
        }

        private static int FindFreeSlot(SimulationWorld world)
        {
            int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                50,
                world.RuntimeSlotCapacityForDiagnostics);
            Require(slot >= 50, "No dynamic runtime slot is available.");
            return slot;
        }

        private static int CountExactWeapons(SimulationWorld world)
        {
            int count = 0;
            for (int slot = 0;
                 slot < world.RuntimeSlotCapacityForDiagnostics;
                 slot++)
            {
                LF2Entity entity =
                    world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null)
                    continue;
                int type = entity.GetCurrentDataObjectTypeForSimulation();
                if (type == 1 || type == 2 || type == 4 || type == 6)
                    count++;
            }
            return count;
        }

        private static T Create<T>(
            SimulationWorld world,
            int oid,
            int dataType,
            int slot,
            double x)
            where T : TypedCharacter, new()
        {
            var entity = new T();
            entity.ModuleInitialize();
            entity.Name = "C13Play_" + oid;
            entity.ObjectId = oid;
            entity.CurrentDataType = dataType;
            entity.RelationTeam = 987655;
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                centerx = 39,
                centery = 79,
                itrs = new List<InteractionArea>(),
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                oid,
                new LF2CharacterData
                {
                    name = entity.Name,
                    type_sub = dataType,
                    frames = new List<LF2FrameData> { frame },
                }));
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = frame;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = frame;
            entity.Runtime.SetPosition(x, 0.0, 300.0);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Write(Report report)
        {
            File.WriteAllText(
                ProjectPath(ResultPath),
                JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C13ActiveWeaponCountPlacementPlayProbe] PASS");
            else
                Debug.LogError(
                    "[NTSD28C13ActiveWeaponCountPlacementPlayProbe] " +
                    report.message);
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

        private abstract class TypedCharacter : LF2Character
        {
            internal int CurrentDataType { get; set; }

            public override int GetCurrentDataObjectTypeForSimulation()
                => CurrentDataType;

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
                => true;

            internal override void RunCharacterInputProducerPhaseForKnownCharacterDat(
                int tickIndex)
            {
            }

            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(
                int tickIndex,
                bool applyFrameMotionTail = true)
            {
            }
        }

        private sealed class CountObserver : TypedCharacter
        {
            internal int PostSerialCount { get; private set; }
            internal int ObservedCountInSerial { get; private set; } = -1;
            internal int ObservedCapturedTickInSerial { get; private set; } = -1;

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedCountInSerial =
                    Match?.ActiveWeaponObjectCountBeforeHitsForDiagnostics ?? -1;
                ObservedCapturedTickInSerial =
                    Match?.ActiveWeaponObjectCountCapturedTickForDiagnostics ?? -1;
            }
        }

        private sealed class PassiveCharacter : TypedCharacter
        {
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int baselineActiveWeaponCount;
            public int expectedCount;
            public int postSerialCount;
            public int observedCountInSerial;
            public int observedCapturedTickInSerial;
            public int finalSnapshotCount;
            public int finalCapturedTick;
            public int finalCurrentExactCount;
            public bool cleanupPassed;
        }
    }
}
#endif
