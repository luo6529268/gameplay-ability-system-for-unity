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
            public string mode;
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
            public bool registered;
            public bool activeForPass;
            public bool physicsStatePresent;
            public int timerAfterFirst;
            public int currentDataType;
            public int worldTickAfterFirst;
            public int stageWidth;
            public int stageZMin;
            public int stageZMax;
            public long passVisitsBefore;
            public long passVisitsAfter;
            public int driverSteps;
            public int prematureRemovalTick;
            public bool fastModeActive;
            public float activeHostIntervalSeconds;
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void LoadFormalFrames(LF2CharacterDataWrapper definition)
            {
                FrameCache.Load(definition);
                ImmediateFrame(definition.characterData.frames[0].frameId);
            }
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
            if (driver?.World == null ||
                driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                manager?.LoadedBoundaryDefinition == null)
            {
                if (playRequestedAt != default &&
                    (DateTime.UtcNow - playRequestedAt).TotalSeconds < 360)
                    return;
                Finish(new Report { scene = scene.path,
                    error = $"Play readiness timeout: driver={driver != null}, " +
                        $"world={driver?.World != null}, lifecycle={driver?.LifecycleState}, " +
                        $"manager={manager != null}, " +
                        $"map={manager?.LoadedBoundaryDefinition != null}." }, true);
                return;
            }

            running = true;
            string mode = File.ReadAllText(RequestPath).Trim();
            var report = new Report { scene = scene.path, mode = mode };
            try
            {
                Run(driver, manager, report,
                    mode == "driver" || mode == "fast-driver",
                    mode == "fast-driver");
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
            BoundaryWallManager manager, Report report,
            bool fullDriver, bool fastDriver)
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
            bool wasFast = driver.IsFastMode;
            int baselineCount = world.ObjectCount;
            int baselineSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            ProbeWeapon weapon = null;
            try
            {
                driver.SetPaused(true);
                if (fastDriver)
                {
                    driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F5);
                    driver.ProcessHostControlCommandsForDiagnostics();
                    report.fastModeActive = driver.IsFastMode;
                    report.activeHostIntervalSeconds = driver.ActiveHostIntervalSeconds;
                    Require(report.fastModeActive &&
                        Math.Abs(report.activeHostIntervalSeconds - 0.003f) < 0.00001f,
                        "F5 host route did not select the 3 ms cadence.");
                }
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
                weapon = new ProbeWeapon();
                weapon.ObjectId = 150;
                weapon.SetWeaponType((int)LF2ObjectType.HeavyWeapon);
                weapon.SetRequiredRuntimeSlot(slot);
                world.Register(weapon);
                if (fullDriver)
                {
                    LF2CharacterDataWrapper formal = world.RuntimeCharacterConfigs.Resolve(150);
                    Require(formal?.characterData?.frames != null &&
                        formal.characterData.frames.Count > 0,
                        "Formal OID150 frames are unavailable.");
                    weapon.LoadFormalFrames(formal);
                    weapon.FrameDelay = 1000;
                    weapon.Health.HP = 1000;
                    weapon.Runtime.SetVelocity(0, 0, 0);
                }
                report.registered = world.FindEntityByRuntimeSlotForQuery(slot) == weapon;
                report.currentDataType = weapon.GetCurrentDataObjectTypeForSimulation();
                report.activeForPass = world.IsActiveForCurrentPassInternal(weapon);
                report.physicsStatePresent = weapon.PS != null;
                report.stageWidth = world.Runtime.Stage.BaseStageWidthPx;
                report.stageZMin = world.StageZMin;
                report.stageZMax = world.StageZMax;
                report.passVisitsBefore = world.BattleEcsCharacterPreFrameBoundsPassDiagnosticsForDiagnostics.SlotVisitCount;
                Require(report.registered, "Play World rejected transient registration.");
                Require(report.currentDataType != (int)LF2ObjectType.Character,
                    "Probe OID resolved to character DAT type.");
                weapon.Runtime.SetPosition(5000, 0, insidePixel.y);
                if (fullDriver)
                {
                    Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                        "Driver rejected the first tick.");
                    report.driverSteps = 1;
                }
                else
                {
                    world.AdvanceBattleFlowTick(report.startTick);
                    world.ApplyPreFrameBoundsAll();
                }
                report.timerAfterFirst = weapon.Runtime.OutsideWalkableSinceTick;
                report.worldTickAfterFirst = world.CurrentTickIndex;
                report.passVisitsAfter = world.BattleEcsCharacterPreFrameBoundsPassDiagnosticsForDiagnostics.SlotVisitCount;
                Require(weapon.Runtime.OutsideWalkableSinceTick == report.startTick,
                    "Outside timer did not start at the Play tick.");

                report.beforeRemovalTick = report.startTick + 303;
                if (fullDriver)
                {
                    while (driver.CurrentTickIndex < report.beforeRemovalTick)
                    {
                        Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                            "Driver rejected a tick before the threshold.");
                        report.driverSteps++;
                        if (world.FindEntityByRuntimeSlotForQuery(slot) != weapon)
                        {
                            report.prematureRemovalTick = driver.CurrentTickIndex;
                            throw new InvalidOperationException(
                                "Object left the World before the walkable TTL threshold.");
                        }
                    }
                }
                else
                {
                    world.AdvanceBattleFlowTick(report.beforeRemovalTick);
                    world.ApplyPreFrameBoundsAll();
                }
                report.presentAt303 = world.FindEntityByRuntimeSlotForQuery(slot) == weapon;
                Require(report.presentAt303, "Object disappeared before 10 logic seconds.");

                report.removalTick = report.startTick + 304;
                if (fullDriver)
                {
                    Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                        "Driver rejected the removal tick.");
                    report.driverSteps++;
                }
                else
                {
                    world.AdvanceBattleFlowTick(report.removalTick);
                    world.ApplyPreFrameBoundsAll();
                }
                report.removedAt304 = world.FindEntityByRuntimeSlotForQuery(slot) == null;
                Require(report.removedAt304, "Object remained after 304 elapsed ticks.");
                report.status = "PASS";
            }
            finally
            {
                if (weapon != null && weapon.RegisteredWorldForSimulation == world)
                    world.Unregister(weapon);
                if (driver.IsFastMode != wasFast)
                {
                    driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F5);
                    driver.ProcessHostControlCommandsForDiagnostics();
                }
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
