#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q04MassFrictionPlayProbeEditor
    {
        [MenuItem("NTSD/Battle Diagnostics/Q04/Run Mass Friction Play Probe")]
        public static void Run()
        {
            var report = new Report();
            SimulationTickDriver driver = UnityEngine.Object.FindObjectOfType<SimulationTickDriver>();
            SimulationWorld world = driver?.World;
            if (!EditorApplication.isPlaying || world == null)
            {
                report.message = "Existing Play battle world required.";
                Write(report);
                return;
            }
            bool paused = driver.IsPaused;
            int baseline = world.ObjectCount;
            int slots = world.ClaimedRuntimeSlotCountForDiagnostics;
            var entities = new List<LF2Character>();
            try
            {
                driver.SetPaused(true);
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false), "Stage warmup tick rejected");
                double startZ = (world.StageZMin + world.StageZMax) * 0.5;
                Require(startZ - world.StageZMin > 5, "Stage depth too narrow for movement probe");
                string[] cases = { "forward", "reverse", "stationary" };
                double[] startVx = { 5, -5, 0 };
                double[] startVz = { -5, 5, 0 };
                double[] groundedVx = { 4, -4, 0 };
                double[] groundedVz = { -4, 4, 0 };
                for (int i = 0; i < cases.Length; i++)
                {
                    int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                    Require(slot >= 50, "No transient slot");
                    LF2Character entity = NTSD28Q04MassFrictionGateEditorTests.CreateCharacter(world, slot);
                    entities.Add(entity);
                    entity.Runtime.SetPosition(400 + i * 100, 0, startZ);
                    entity.Runtime.SetVelocity(startVx[i], 0, startVz[i]);
                    entity.Runtime.SyncIntegerPosition();
                }
                report.startTick = driver.CurrentTickIndex;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false), "Driver rejected tick");
                report.endTick = driver.CurrentTickIndex;
                Require(report.endTick == report.startTick + 1, "Expected one tick");
                for (int i = 0; i < entities.Count; i++)
                {
                    NTSDEntityRuntime state = entities[i].Runtime;
                    report.rows.Add(new Row { objectId = entities[i].ObjectId, caseName = cases[i], x = state.X, y = state.Y,
                        z = state.Z, vx = state.Vx, vy = state.Vy, vz = state.Vz });
                    Require(state.X == 400 + i * 100 + startVx[i] && state.Z == startZ + startVz[i] &&
                        state.Vx == groundedVx[i] && state.Vz == groundedVz[i],
                        "Grounded production tick differs for motion case " + cases[i]);
                }
                // A just-landed body must not receive the grounded-start friction gate.
                for (int i = 0; i < entities.Count; i++)
                {
                    entities[i].Runtime.SetPosition(400 + i * 100, -1, startZ);
                    entities[i].Runtime.SetVelocity(5, 2, -5);
                    entities[i].Runtime.SyncIntegerPosition();
                }
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false), "Driver rejected landing tick");
                for (int i = 0; i < entities.Count; i++)
                {
                    NTSDEntityRuntime state = entities[i].Runtime;
                    report.landingRows.Add(new Row { objectId = entities[i].ObjectId, caseName = cases[i], x = state.X, y = state.Y,
                        z = state.Z, vx = state.Vx, vy = state.Vy, vz = state.Vz });
                    Require(state.Y == 0, "Landing did not reach floor");
                    Require(state.Vx == entities[2].Runtime.Vx && state.Vy == entities[2].Runtime.Vy &&
                        state.Vz == entities[2].Runtime.Vz, "Identical landing inputs produced different results");
                }
                report.status = "PASS";
                report.message = "Actual driver forward/reverse/stationary and landing ticks after mass carrier removal; same object ID avoids unrelated definition behavior, injected logic characters in existing Play world.";
            }
            catch (Exception ex) { report.message = ex.ToString(); }
            finally
            {
                foreach (LF2Character entity in entities) world.Unregister(entity);
                driver.SetPaused(paused);
                report.cleanupPassed = world.ObjectCount == baseline &&
                    world.ClaimedRuntimeSlotCountForDiagnostics == slots;
                if (!report.cleanupPassed) { report.status = "FAIL"; report.message += " Cleanup count mismatch."; }
                Write(report);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Write(Report report)
        {
            File.WriteAllText("Temp/NTSD28_Q04_MassFriction.result.json", JsonUtility.ToJson(report, true));
        }

        [Serializable] private sealed class Row
        {
            public int objectId;
            public string caseName;
            public double x, y, z, vx, vy, vz;
        }
        [Serializable] private sealed class Report
        {
            public string status = "FAIL";
            public string message;
            public int startTick, endTick;
            public bool cleanupPassed;
            public List<Row> rows = new List<Row>();
            public List<Row> landingRows = new List<Row>();
        }
    }
}
#endif
