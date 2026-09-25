#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28D024ProtectedXSceneWitnessEditor
    {
        private const string RequestPath = "Temp/NTSD28_D024_ProtectedXPlay.request";
        private const string ResultPrefix = "Temp/NTSD28_D024_ProtectedXPlay.";
        private static DateTime playRequestedAt;
        private static bool running;

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status = "FAIL";
            public string phase;
            public string error;
            public string scene;
            public int tickBefore;
            public int tickAfter;
            public int stageWidth;
            public double viewScale;
            public int leftSlot;
            public int rightSlot;
            public int leftFrameBefore;
            public int rightFrameBefore;
            public int leftFrameAfter;
            public int rightFrameAfter;
            public double leftPhysicalBefore;
            public double rightPhysicalBefore;
            public double leftSourceBefore;
            public double rightSourceBefore;
            public double leftPhysicalAfter;
            public double rightPhysicalAfter;
            public double leftSourceAfter;
            public double rightSourceAfter;
            public int leftSourceIntAfter;
            public int rightSourceIntAfter;
            public bool leftSurvived;
            public bool rightSurvived;
            public bool cleanupPassed;
            public int objectCountBefore;
            public int objectCountAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int rendererBorrowersBefore;
            public int rendererBorrowersAfter;
        }

        private sealed class ProbeDrink : LF2Weapon
        {
            internal void BindFormalFrame(LF2CharacterDataWrapper definition)
            {
                FrameCache.Load(definition);
                ImmediateFrame(definition.characterData.frames[0].frameId);
            }
        }

        static NTSD28D024ProtectedXSceneWitnessEditor()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || !File.Exists(RequestPath) ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            Scene scene = SceneManager.GetActiveScene();
            string runId = File.ReadAllText(RequestPath).Trim();
            if (string.IsNullOrEmpty(runId) ||
                !System.Text.RegularExpressions.Regex.IsMatch(runId, "^[A-Za-z0-9-]+$"))
            {
                Finish(new Report { runId = "invalid", scene = scene.path,
                    error = "Invalid diagnostic run ID." }, false);
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                if (scene.name != "NTSD_Battle" || scene.isDirty)
                {
                    Finish(new Report { runId = runId, scene = scene.path,
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
            if (driver?.World == null ||
                driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                driver.World.RuntimeCharacterConfigs?.Resolve(122) == null ||
                driver.World.RuntimeCharacterConfigs?.Resolve(123) == null)
            {
                if ((DateTime.UtcNow - playRequestedAt).TotalSeconds < 360)
                    return;
                Finish(new Report { runId = runId, scene = scene.path,
                    error = "Battle World or formal type6 content did not become ready." }, true);
                return;
            }

            running = true;
            var report = new Report { runId = runId, scene = scene.path };
            try
            {
                Run(driver, report);
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

        private static void Run(SimulationTickDriver driver, Report report)
        {
            SimulationWorld world = driver.World;
            bool wasPaused = driver.IsPaused;
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            report.rendererBorrowersBefore = pool?.ActiveObjectCountForAcceptance ?? -1;
            report.objectCountBefore = world.ObjectCount;
            report.slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            ProbeDrink left = null;
            ProbeDrink right = null;
            try
            {
                report.phase = "register";
                driver.SetPaused(true);
                report.stageWidth = world.Runtime.Stage.BaseStageWidthPx;
                report.viewScale = world.FixedViewRunDistanceScale;
                Require(report.stageWidth > 200 && report.viewScale > 1.0,
                    "Unexpected saved stage width or fixed-view ratio.");
                report.leftSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(70, 1000);
                Require(report.leftSlot >= 70, "No left transient slot.");
                left = RegisterDrink(world, 122, 1, report.leftSlot, 50);
                report.rightSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(70, 1000);
                Require(report.rightSlot >= 70 && report.rightSlot != report.leftSlot,
                    "No right transient slot.");
                right = RegisterDrink(world, 123, 2, report.rightSlot,
                    report.stageWidth - 10);
                report.leftFrameBefore = left.Frame?.N ?? -1;
                report.rightFrameBefore = right.Frame?.N ?? -1;
                report.leftPhysicalBefore = left.Runtime.X;
                report.rightPhysicalBefore = right.Runtime.X;
                report.leftSourceBefore = left.Runtime.SourceRuleX;
                report.rightSourceBefore = right.Runtime.SourceRuleX;
                report.tickBefore = driver.CurrentTickIndex;
                world.PrepareStageRuntimeSnapshotForTick(report.tickBefore + 1);

                report.phase = "full-driver";
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production Driver rejected the protected-object tick.");
                report.tickAfter = driver.CurrentTickIndex;
                report.leftSurvived =
                    world.FindEntityByRuntimeSlotForQuery(report.leftSlot) == left;
                report.rightSurvived =
                    world.FindEntityByRuntimeSlotForQuery(report.rightSlot) == right;
                report.leftFrameAfter = report.leftSurvived ? left.Frame?.N ?? -1 : -1;
                report.rightFrameAfter = report.rightSurvived ? right.Frame?.N ?? -1 : -1;
                if (!report.leftSurvived || !report.rightSurvived)
                    throw new InvalidOperationException(
                        "Formal type6 frame ended before both protected clamp results could be observed.");
                report.leftPhysicalAfter = left.Runtime.X;
                report.rightPhysicalAfter = right.Runtime.X;
                report.leftSourceAfter = left.Runtime.SourceRuleX;
                report.rightSourceAfter = right.Runtime.SourceRuleX;
                report.leftSourceIntAfter = left.Runtime.SourceRuleXInt;
                report.rightSourceIntAfter = right.Runtime.SourceRuleXInt;
                Require(report.tickAfter == report.tickBefore + 1 &&
                    report.leftPhysicalAfter == 100.0 &&
                    report.rightPhysicalAfter == report.stageWidth - 100.0 &&
                    report.leftSourceAfter == 100.0 &&
                    report.rightSourceAfter == report.stageWidth - 100.0 &&
                    report.leftSourceIntAfter == 100 &&
                    report.rightSourceIntAfter == report.stageWidth - 100,
                    "Full-Driver protected clamp differs from the formal margin.");
                report.status = "PASS";
            }
            finally
            {
                report.phase = "cleanup";
                if (left != null && left.RegisteredWorldForSimulation == world)
                    world.Unregister(left);
                if (right != null && right.RegisteredWorldForSimulation == world)
                    world.Unregister(right);
                driver.SetPaused(wasPaused);
                report.objectCountAfter = world.ObjectCount;
                report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                report.rendererBorrowersAfter = pool?.ActiveObjectCountForAcceptance ?? -1;
                report.cleanupPassed =
                    world.FindEntityByRuntimeSlotForQuery(report.leftSlot) == null &&
                    world.FindEntityByRuntimeSlotForQuery(report.rightSlot) == null &&
                    report.rendererBorrowersAfter == report.rendererBorrowersBefore;
                if (!report.cleanupPassed)
                {
                    report.status = "FAIL";
                    report.error = "Transient slot or renderer borrower remained after cleanup.";
                }
            }
        }

        private static ProbeDrink RegisterDrink(SimulationWorld world,
            int oid, int participantClass, int slot, double x)
        {
            LF2CharacterDataWrapper definition = world.RuntimeCharacterConfigs.Resolve(oid);
            Require(definition?.characterData?.frames != null &&
                definition.characterData.frames.Count > 0,
                "Formal type6 definition has no frame.");
            var drink = new ProbeDrink();
            drink.ObjectId = oid;
            drink.SetWeaponType((int)LF2ObjectType.Drink);
            drink.SetRequiredRuntimeSlot(slot);
            world.Register(drink);
            drink.BindFormalFrame(definition);
            drink.FrameDelay = 1000;
            drink.Health.HP = 1000;
            drink.Unk344 = participantClass;
            drink.Runtime.SetPosition(x, 0, 250);
            drink.Runtime.SyncIntegerPosition();
            drink.Runtime.SetSourceRulePosition(x, 250);
            drink.Runtime.SyncSourceRuleIntegerPosition();
            drink.Runtime.SetVelocity(0, 0, 0);
            return drink;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Finish(Report report, bool exitPlay)
        {
            File.Delete(RequestPath);
            File.WriteAllText(ResultPrefix + report.runId + ".json",
                JsonUtility.ToJson(report, true));
            if (exitPlay && EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }
    }
}
#endif
