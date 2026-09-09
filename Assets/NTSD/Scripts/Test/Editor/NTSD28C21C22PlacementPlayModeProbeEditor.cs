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
    public static class NTSD28C21C22PlacementPlayModeProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_B3_C21_C22_Placement.request";
        private const string ResultPath = "Temp/NTSD28_B3_C21_C22_Placement.result.json";

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

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C21 C22 Placement Play Probe")]
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
            bool diagnosticsAlreadyEnabled =
                world.ActiveBattleTickPhaseDiagnosticsForDiagnostics?.Enabled == true;
            int baselineObjects = world.ObjectCount;
            int baselineClaimed = world.ClaimedRuntimeSlotCountForDiagnostics;
            C21C22Observer observer = null;
            try
            {
                driver.SetPaused(true);
                BattleTickPhaseDiagnostics diagnostics =
                    diagnosticsAlreadyEnabled
                        ? world.ActiveBattleTickPhaseDiagnosticsForDiagnostics
                        : world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
                int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    50,
                    world.RuntimeSlotCapacityForDiagnostics);
                Require(slot >= 50, "No dynamic runtime slot is available.");
                observer = CreateObserver(world, slot);
                observer.Runtime.X = -200.0;
                observer.Runtime.Vx = 0.0;
                observer.HitCount = 1;
                observer.KnockbackVx = 10.0;

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(
                        ignorePaused: true,
                        buildPresentation: false),
                    "Production driver rejected the C21/C22 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.observerSlot = slot;
                report.postSerialCount = observer.PostSerialCount;
                report.xSeenInSerial = observer.XSeenInSerial;
                report.vxSeenInSerial = observer.VxSeenInSerial;
                report.hitCountSeenInSerial = observer.HitCountSeenInSerial;
                report.knockbackVxSeenInSerial = observer.KnockbackVxSeenInSerial;
                report.phaseCount = diagnostics.LastPhaseSequenceCount;
                report.phase23 = PhaseAt(diagnostics, 23);
                report.phase24 = PhaseAt(diagnostics, 24);
                report.phase25 = PhaseAt(diagnostics, 25);
                report.phase26 = PhaseAt(diagnostics, 26);
                report.phase27 = PhaseAt(diagnostics, 27);
                report.phase28 = PhaseAt(diagnostics, 28);
                report.phase29 = PhaseAt(diagnostics, 29);

                Require(driver.CurrentTickIndex == expectedTick &&
                        observer.PostSerialCount == 1 &&
                        observer.XSeenInSerial == -100.0 &&
                        observer.VxSeenInSerial == 10.0 &&
                        observer.HitCountSeenInSerial == 0 &&
                        observer.KnockbackVxSeenInSerial == 0.0 &&
                        report.phaseCount == 34 &&
                        report.phase23 == "PreFrameBounds" &&
                        report.phase24 == "FramePostProcess" &&
                        report.phase25 == "NativeResourceTick" &&
                        report.phase26 == "NativeFrameTick" &&
                        report.phase27 == "FrameAdvance" &&
                        report.phase28 == "Stage" &&
                        report.phase29 == "RenderDispatch",
                    "C21/C22 writes or phase placement are incorrect.");

                report.status = "PASS";
                report.message =
                    "C21 bounds and C22 impulse finalize before the temporary serial tail proxy.";
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
                if (!diagnosticsAlreadyEnabled)
                    world.DisableBattleTickPhaseDiagnosticsForDiagnostics();
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

        private static C21C22Observer CreateObserver(SimulationWorld world, int slot)
        {
            var observer = new C21C22Observer();
            observer.ModuleInitialize();
            observer.Name = "C21C22PlayObserver";
            observer.ObjectId = 13421;
            observer.RelationTeam = 0;
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

        private static string PhaseAt(BattleTickPhaseDiagnostics diagnostics, int index)
        {
            return diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase phase)
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
            File.WriteAllText(ProjectPath(ResultPath), JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C21C22PlacementPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C21C22PlacementPlayProbe] " + report.message);
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

        private sealed class C21C22Observer : LF2Character
        {
            internal int PostSerialCount { get; private set; }
            internal double XSeenInSerial { get; private set; } = double.NaN;
            internal double VxSeenInSerial { get; private set; } = double.NaN;
            internal int HitCountSeenInSerial { get; private set; } = -1;
            internal double KnockbackVxSeenInSerial { get; private set; } = double.NaN;

            internal override bool RunNativePhysicsForWorldPass(int tickIndex) => true;

            internal override void RunCharacterInputProducerPhaseForKnownCharacterDat(int tickIndex)
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
                XSeenInSerial = Runtime.X;
                VxSeenInSerial = Runtime.Vx;
                HitCountSeenInSerial = HitCount;
                KnockbackVxSeenInSerial = KnockbackVx;
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
            public double xSeenInSerial;
            public double vxSeenInSerial;
            public int hitCountSeenInSerial;
            public double knockbackVxSeenInSerial;
            public int phaseCount;
            public string phase23;
            public string phase24;
            public string phase25;
            public string phase26;
            public string phase27;
            public string phase28;
            public string phase29;
            public bool cleanupPassed;
        }
    }
}
#endif
