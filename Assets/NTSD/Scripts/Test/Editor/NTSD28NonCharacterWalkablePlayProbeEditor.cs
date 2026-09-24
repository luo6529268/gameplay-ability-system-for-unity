#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.LevelEditor;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28NonCharacterWalkablePlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_D025_WalkablePlay.request";
        private const string ResultPath = "Temp/NTSD28_D025_WalkablePlay.result.json";
        private static DateTime playRequestedAt;
        private static bool running;

        [Serializable]
        private sealed class Report
        {
            public string status = "FAIL";
            public string error;
            public string scene;
            public string mapId;
            public int polygonCount;
            public int startTick;
            public int beforeRemovalTick;
            public int removalTick;
            public bool insideWalkable;
            public bool outsideWalkable;
            public bool presentAt303;
            public bool removedAt304;
            public bool cleanupPassed;
        }

        static NTSD28NonCharacterWalkablePlayProbeEditor()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || !File.Exists(RequestPath) ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            Scene scene = SceneManager.GetActiveScene();
            if (!EditorApplication.isPlaying)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                if (scene.name != "NTSD_Battle" || scene.isDirty)
                {
                    Finish(new Report { scene = scene.path,
                        error = "A clean, saved NTSD_Battle scene is required." }, false);
                    return;
                }

                playRequestedAt = DateTime.UtcNow;
                EditorApplication.EnterPlaymode();
                return;
            }

            if (playRequestedAt == default)
                playRequestedAt = DateTime.UtcNow;

            SimulationTickDriver driver = UnityEngine.Object.FindObjectOfType<SimulationTickDriver>();
            BoundaryWallManager manager = UnityEngine.Object.FindObjectOfType<BoundaryWallManager>();
            if (driver?.World == null || manager?.LoadedBoundaryDefinition == null)
            {
                if (playRequestedAt != default &&
                    (DateTime.UtcNow - playRequestedAt).TotalSeconds < 45)
                    return;
                Finish(new Report { scene = scene.path,
                    error = "Battle World or loaded map boundary was unavailable in Play." }, true);
                return;
            }

            running = true;
            var report = new Report { scene = scene.path };
            try
            {
                Run(driver, manager, report);
            }
            catch (Exception exception)
            {
                report.error = exception.ToString();
            }
            finally
            {
                Finish(report, true);
                running = false;
            }
        }

        private static void Run(SimulationTickDriver driver,
            BoundaryWallManager manager, Report report)
        {
            SimulationWorld world = driver.World;
            report.mapId = manager.LoadedBoundaryDefinition.MapId;
            Require(report.mapId == "Sunagakure", "Unexpected battle map.");
            report.polygonCount = manager.EnabledBoundaries.Count;
            Require(report.polygonCount > 0, "No enabled walkable boundary.");

            BoundaryWall boundary = manager.EnabledBoundaries[0];
            Require(boundary.Polygons.Count > 0, "No walkable polygon.");
            var vertices = new System.Collections.Generic.List<Vector2>();
            Require(boundary.TryGetWorldVertices(boundary.Polygons[0], vertices) &&
                    vertices.Count >= 3, "Invalid walkable polygon.");
            Vector2 center = Vector2.zero;
            for (int index = 0; index < vertices.Count; index++)
                center += vertices[index];
            center /= vertices.Count;
            Require(manager.IsPointWalkable(center), "Map polygon center is not walkable.");
            Vector2 insidePixel = NTSDRenderSpace.WorldToScreenPixel(
                new Vector3(center.x, center.y, 0));

            bool wasPaused = driver.IsPaused;
            int baselineCount = world.ObjectCount;
            int baselineSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            LF2Weapon weapon = null;
            try
            {
                driver.SetPaused(true);
                report.startTick = driver.CurrentTickIndex + 1;
                world.PrepareStageRuntimeSnapshotForTick(report.startTick);
                Require(world.TryIsGroundPixelWalkable(
                    insidePixel.x, insidePixel.y, out report.insideWalkable),
                    "Map snapshot was not captured on the Play host.");
                Require(world.TryIsGroundPixelWalkable(
                    5000, insidePixel.y, out report.outsideWalkable),
                    "Map snapshot disappeared before PreFrame.");
                Require(report.insideWalkable && !report.outsideWalkable,
                    "Snapshot differs from the loaded walkable map.");

                int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(70, 1000);
                Require(slot >= 70, "No transient slot is available.");
                weapon = new LF2Weapon();
                weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
                weapon.SetRequiredRuntimeSlot(slot);
                world.Register(weapon);
                weapon.Runtime.SetPosition(5000, 0, insidePixel.y);
                world.AdvanceBattleFlowTick(report.startTick);
                world.ApplyPreFrameBoundsAll();
                Require(weapon.Runtime.OutsideWalkableSinceTick == report.startTick,
                    "Outside timer did not start at the Play tick.");

                report.beforeRemovalTick = report.startTick + 303;
                world.AdvanceBattleFlowTick(report.beforeRemovalTick);
                world.ApplyPreFrameBoundsAll();
                report.presentAt303 = world.FindEntityByRuntimeSlotForQuery(slot) == weapon;
                Require(report.presentAt303, "Object disappeared before 10 logic seconds.");

                report.removalTick = report.startTick + 304;
                world.AdvanceBattleFlowTick(report.removalTick);
                world.ApplyPreFrameBoundsAll();
                report.removedAt304 = world.FindEntityByRuntimeSlotForQuery(slot) == null;
                Require(report.removedAt304, "Object remained after 304 elapsed ticks.");
                report.status = "PASS";
            }
            finally
            {
                if (weapon != null && weapon.RegisteredWorldForSimulation == world)
                    world.Unregister(weapon);
                driver.SetPaused(wasPaused);
                report.cleanupPassed = world.ObjectCount == baselineCount &&
                    world.ClaimedRuntimeSlotCountForDiagnostics == baselineSlots;
                if (!report.cleanupPassed)
                {
                    report.status = "FAIL";
                    report.error = "Transient entity cleanup count mismatch.";
                }
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Finish(Report report, bool exitPlay)
        {
            File.Delete(RequestPath);
            File.WriteAllText(ResultPath, JsonUtility.ToJson(report, true));
            if (exitPlay && EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }
    }
}
#endif
