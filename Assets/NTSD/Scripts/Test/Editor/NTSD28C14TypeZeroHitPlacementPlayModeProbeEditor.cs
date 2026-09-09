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
    public static class NTSD28C14TypeZeroHitPlacementPlayModeProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_B3_C14_TypeZeroHitPlacement.request";
        private const string ResultPath =
            "Temp/NTSD28_B3_C14_TypeZeroHitPlacement.result.json";

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

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C14 Type-Zero Hit Placement Play Probe")]
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
            bool previousForceLegacyEmpty =
                world.ForceLegacyEmptyCharacterHitConsumeForDiagnostics;
            bool diagnosticsAlreadyEnabled =
                world.ActiveBattleTickPhaseDiagnosticsForDiagnostics?.Enabled ==
                true;
            int baselineObjects = world.ObjectCount;
            int baselineClaimed =
                world.ClaimedRuntimeSlotCountForDiagnostics;
            TypeZeroObserver observer = null;
            try
            {
                driver.SetPaused(true);
                world.ForceLegacyEmptyCharacterHitConsumeForDiagnostics = true;
                BattleTickPhaseDiagnostics diagnostics =
                    diagnosticsAlreadyEnabled
                        ? world.ActiveBattleTickPhaseDiagnosticsForDiagnostics
                        : world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

                int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    50,
                    world.RuntimeSlotCapacityForDiagnostics);
                Require(slot >= 50, "No dynamic runtime slot is available.");
                observer = CreateObserver(world, slot);

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(
                        ignorePaused: true,
                        buildPresentation: false),
                    "Production driver rejected the C14 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.observerSlot = slot;
                report.postSerialCount = observer.PostSerialCount;
                report.hitExecutionsSeenInSerial =
                    observer.HitExecutionsSeenInSerial;
                report.finalHitExecutions =
                    world.LastCharacterHitConsumeExecutedCountForDiagnostics;
                report.phaseCount = diagnostics.LastPhaseSequenceCount;
                report.phase14 = PhaseAt(diagnostics, 14);
                report.phase15 = PhaseAt(diagnostics, 15);
                report.phase16 = PhaseAt(diagnostics, 16);
                report.phase17 = PhaseAt(diagnostics, 17);
                report.phase18 = PhaseAt(diagnostics, 18);

                Require(driver.CurrentTickIndex == expectedTick &&
                        observer.PostSerialCount == 1 &&
                        observer.HitExecutionsSeenInSerial > 0 &&
                        report.finalHitExecutions > 0 &&
                        report.phaseCount == 34 &&
                        report.phase14 == "ActiveWeaponCount" &&
                        report.phase15 ==
                            "CharacterHitConsumePostInteraction" &&
                        report.phase16 == "FrameAdvance" &&
                        report.phase17 == "RandomWeaponDrop" &&
                        report.phase18 == "ObjectHitConsume",
                    "Production serial remainder did not observe the completed C14 caller and phase order.");

                report.status = "PASS";
                report.message =
                    "Production C14 type-zero hit caller completed before serial remainder.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                if (observer != null)
                    world.Unregister(observer);
                world.ForceLegacyEmptyCharacterHitConsumeForDiagnostics =
                    previousForceLegacyEmpty;
                if (!diagnosticsAlreadyEnabled)
                    world.DisableBattleTickPhaseDiagnosticsForDiagnostics();
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

        private static TypeZeroObserver CreateObserver(
            SimulationWorld world,
            int slot)
        {
            var observer = new TypeZeroObserver();
            observer.ModuleInitialize();
            observer.Name = "C14Play_TypeZeroObserver";
            observer.ObjectId = 13410;
            observer.RelationTeam = 987656;
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
            observer.FrameCache.Load(new LF2CharacterDataWrapper(
                observer.ObjectId,
                new LF2CharacterData
                {
                    name = observer.Name,
                    frames = new List<LF2FrameData> { frame },
                }));
            observer.Frame.N = 0;
            observer.Frame.PN = 0;
            observer.Frame.D = frame;
            observer.Initialize(500, 500);
            observer.SetRequiredRuntimeSlot(slot);
            world.Register(observer);
            observer.Runtime.SetPosition(200.0, 0.0, 300.0);
            observer.Runtime.SyncIntegerPosition();
            observer.RefreshRuntimeSnapshot();
            return observer;
        }

        private static string PhaseAt(
            BattleTickPhaseDiagnostics diagnostics,
            int index)
        {
            return diagnostics.TryGetLastPhaseAt(
                    index,
                    out BattleTickPhase phase)
                ? BattleTickPhaseDiagnostics.GetPhaseName(phase)
                : string.Empty;
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
                Debug.Log("[NTSD28C14TypeZeroHitPlacementPlayProbe] PASS");
            else
                Debug.LogError(
                    "[NTSD28C14TypeZeroHitPlacementPlayProbe] " +
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

        private sealed class TypeZeroObserver : LF2Character
        {
            internal int PostSerialCount { get; private set; }
            internal int HitExecutionsSeenInSerial { get; private set; } = -1;

            public override int GetCurrentDataObjectTypeForSimulation()
                => (int)LF2ObjectType.Character;

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

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                HitExecutionsSeenInSerial =
                    Match?.LastCharacterHitConsumeExecutedCountForDiagnostics ??
                    -1;
            }
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int observerSlot;
            public int postSerialCount;
            public int hitExecutionsSeenInSerial;
            public int finalHitExecutions;
            public int phaseCount;
            public string phase14;
            public string phase15;
            public string phase16;
            public string phase17;
            public string phase18;
            public bool cleanupPassed;
        }
    }
}
#endif
