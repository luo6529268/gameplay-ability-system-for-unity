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
    public static class NTSD28C07RevivalPlacementPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/B3/Run C07 Revival Placement Play Probe";
        private const string ResultPath =
            "Temp/NTSD28_B3_C07_RevivalPlacement.result.json";

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
            LF2Character host = null;
            SerialVisibilityProbe spawned = null;
            try
            {
                driver.SetPaused(true);
                int hostSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                int effectSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    hostSlot + 1,
                    1000);
                Require(hostSlot >= 50 && effectSlot > hostSlot,
                    "No two ordered transient slots are available.");

                host = CreateHost(world, hostSlot, 10750);
                int spawnCount = 0;
                world.SetRespawnEffectSpawnOverrideForSelfCheck((activeWorld, parent) =>
                {
                    spawnCount++;
                    spawned = CreateSpawned(activeWorld, effectSlot, 998);
                    return spawned;
                });

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the C07 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.hostSlot = hostSlot;
                report.effectSlot = effectSlot;
                report.spawnCount = spawnCount;
                report.spawnedSerialCount = spawned?.PostNativePhysicsSerialCount ?? 0;
                report.spawnedObservedPhysicsCompleted =
                    spawned?.ObservedNativePhysicsCompleted ?? true;
                report.hostFrame = host.Frame.N;
                report.hostHp = host.Health.HP;
                Require(driver.CurrentTickIndex == expectedTick,
                    "Production tick index did not advance exactly once.");
                Require(spawnCount == 1 && spawned != null,
                    "C07 production owner did not run exactly once.");
                Require(report.spawnedSerialCount == 1 &&
                        !report.spawnedObservedPhysicsCompleted,
                    "C07 newborn was not visible to serial remainder or incorrectly reran C06.");

                report.status = "PASS";
                report.message =
                    "Production C07 ran once after C06; its high-slot newborn entered serial remainder without rerunning C06.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                world.SetRespawnEffectSpawnOverrideForSelfCheck(null);
                if (spawned != null)
                    world.Unregister(spawned);
                if (host != null)
                    world.Unregister(host);
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

        private static LF2Character CreateHost(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var data = new LF2CharacterData
            {
                name = "C07PlayHost_" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Lying),
                    Frame(219, 0),
                },
            };
            var host = new LF2Character();
            host.ModuleInitialize();
            Bind(host, slot, objectId, data);
            host.Initialize(500, 500);
            host.Health.HP = 0;
            host.Health.HPBound = 10;
            host.Health.HP3 = 10;
            host.Health.PP = 77;
            host.HPOrig = 6;
            host.HP2Orig = 1;
            host.RespawnCount = 80;
            host.HitStun = 2;
            world.Register(host);
            return host;
        }

        private static SerialVisibilityProbe CreateSpawned(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var data = new LF2CharacterData
            {
                name = "C07PlayEffect_" + objectId,
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { Frame(0, 0) },
            };
            var spawned = new SerialVisibilityProbe();
            Bind(spawned, slot, objectId, data);
            world.Register(spawned);
            return spawned;
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
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
            string path = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                ResultPath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C07RevivalPlacementPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C07RevivalPlacementPlayProbe] " + report.message);
        }

        private sealed class SerialVisibilityProbe : LF2OtherObject
        {
            internal int PostNativePhysicsSerialCount { get; private set; }
            internal bool ObservedNativePhysicsCompleted { get; private set; }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostNativePhysicsSerialCount++;
                ObservedNativePhysicsCompleted = nativePhysicsCompleted;
            }
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int hostSlot;
            public int effectSlot;
            public int spawnCount;
            public int spawnedSerialCount;
            public bool spawnedObservedPhysicsCompleted;
            public int hostFrame;
            public int hostHp;
            public bool cleanupPassed;
        }
    }
}
#endif
