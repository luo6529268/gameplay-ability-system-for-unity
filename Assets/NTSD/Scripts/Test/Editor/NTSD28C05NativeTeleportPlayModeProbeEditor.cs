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
    public static class NTSD28C05NativeTeleportPlayModeProbeEditor
    {
        private const int IsolatedTeammateGroup = 50515152;
        private const string MenuPath =
            "NTSD/Battle Diagnostics/B3/Run C05 Native Teleport Play Probe";
        private const string ResultPath =
            "Temp/NTSD28_B3_C05_NativeTeleport.result.json";

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
            ProbeEntity source = null;
            ProbeCharacter target = null;
            try
            {
                driver.SetPaused(true);
                int sourceSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                int targetSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    sourceSlot + 1,
                    1000);
                Require(sourceSlot >= 50 && targetSlot > sourceSlot,
                    "No two free transient slots are available.");

                source = Create(world, sourceSlot, 10400, objectType: 5, relationTeam: 1);
                target = CreateCharacter(world, targetSlot, 10401, relationTeam: 2);
                source.SetFrame(0);
                SetPosition(source, 100, -100, 100);
                SetPosition(target, 300, -200, 130);
                target.PS.groundY = -31;
                source.Runtime.SetVelocity(4, -2, 3);
                world.Runtime.Flow.FrameToggle = 0;

                int state400Tick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the state400 tick.");
                report.observedFrameToggle = world.FrameToggle;
                report.observedTick = driver.CurrentTickIndex;
                report.observedX = source.Runtime.XInt;
                report.observedY = source.Runtime.YInt;
                report.observedZ = source.Runtime.ZInt;
                report.observedVx = source.Runtime.Vx;
                report.observedVy = source.Runtime.Vy;
                report.observedVz = source.Runtime.Vz;
                Require(driver.CurrentTickIndex == state400Tick &&
                        source.Runtime.XInt == 180 && source.Runtime.YInt == -31 &&
                        source.Runtime.ZInt == 131 && HasZeroMotion(source),
                    "state400 did not run through the full production C05 tick.");
                report.state400Passed = true;

                source.SetFrame(1);
                source.RelationTeam = IsolatedTeammateGroup;
                target.RelationTeam = IsolatedTeammateGroup;
                source.Runtime.Dir = "left";
                SetPosition(source, 100, -100, 100);
                SetPosition(target, 500, -200, 500);
                target.PS.groundY = -29;
                source.Runtime.SetVelocity(7, -4, 2);

                int state401Tick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the state401 tick.");
                report.state401ObservedX = source.Runtime.XInt;
                report.state401ObservedY = source.Runtime.YInt;
                report.state401ObservedZ = source.Runtime.ZInt;
                report.state401ObservedVx = source.Runtime.Vx;
                report.state401ObservedVy = source.Runtime.Vy;
                report.state401ObservedVz = source.Runtime.Vz;
                report.state401TargetX = target.Runtime.XInt;
                report.state401TargetY = target.Runtime.YInt;
                report.state401TargetZ = target.Runtime.ZInt;
                Require(driver.CurrentTickIndex == state401Tick &&
                        source.Runtime.XInt == target.Runtime.XInt + 60 &&
                        source.Runtime.YInt == -29 &&
                        source.Runtime.ZInt == target.Runtime.ZInt + 1 &&
                        HasZeroMotion(source),
                    "state401 did not select the farthest teammate in production C05.");
                report.state401Passed = true;
                report.startTick = state400Tick;
                report.endTick = state401Tick;
                report.sourceSlot = sourceSlot;
                report.targetSlot = targetSlot;
                report.status = "PASS";
                report.message =
                    "Production state400/state401 teleport, FrameToggle independence, collision-Y and zero-motion passed.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                if (source != null)
                    world.Unregister(source);
                if (target != null)
                    world.Unregister(target);
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

        private static ProbeEntity Create(
            SimulationWorld world,
            int slot,
            int oid,
            int objectType,
            int relationTeam)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, 400),
                Frame(1, 401),
            };
            var data = new LF2CharacterData
            {
                name = "C05Play_" + oid,
                type_sub = objectType,
                frames = frames,
            };
            var entity = new ProbeEntity
            {
                Name = data.name,
                ObjectId = oid,
                RelationTeam = relationTeam,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(oid, data));
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private static ProbeCharacter CreateCharacter(
            SimulationWorld world,
            int slot,
            int oid,
            int relationTeam)
        {
            var data = new LF2CharacterData
            {
                name = "C05PlayCharacter_" + oid,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0),
                },
            };
            var entity = new ProbeCharacter();
            entity.ModuleInitialize();
            entity.Name = data.name;
            entity.ObjectId = oid;
            entity.RelationTeam = relationTeam;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(oid, data));
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
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

        private static void SetPosition(LF2Entity entity, int x, int y, int z)
        {
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
        }

        private static bool HasZeroMotion(LF2Entity entity)
        {
            return entity.Runtime.Vx == 0.0 &&
                   entity.Runtime.Vy == 0.0 &&
                   entity.Runtime.Vz == 0.0;
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
                Debug.Log("[NTSD28C05NativeTeleportPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C05NativeTeleportPlayProbe] " + report.message);
        }

        private sealed class ProbeEntity : LF2OtherObject
        {
            internal void SetFrame(int frameId)
            {
                Frame.N = frameId;
                Frame.D = FrameCache.GetFrameDataById(frameId);
            }

            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(
                int tickIndex,
                bool applyFrameMotionTail = true)
            {
            }

            public override void SimTU(int tickIndex)
            {
            }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }

            public override void SimFrameTick(int tickIndex)
            {
            }
        }

        private sealed class ProbeCharacter : LF2Character
        {
            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(
                int tickIndex,
                bool applyFrameMotionTail = true)
            {
            }

            public override void SimTransit(int tickIndex)
            {
            }

            public override void SimFrameTick(int tickIndex)
            {
            }
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int sourceSlot;
            public int targetSlot;
            public int observedFrameToggle;
            public int observedTick;
            public int observedX;
            public int observedY;
            public int observedZ;
            public double observedVx;
            public double observedVy;
            public double observedVz;
            public int state401ObservedX;
            public int state401ObservedY;
            public int state401ObservedZ;
            public double state401ObservedVx;
            public double state401ObservedVy;
            public double state401ObservedVz;
            public int state401TargetX;
            public int state401TargetY;
            public int state401TargetZ;
            public bool state400Passed;
            public bool state401Passed;
            public bool cleanupPassed;
        }
    }
}
#endif
