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
    public static class NTSD28C06NestedPhysicsPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/B3/Run C06 Nested Physics Play Probe";
        private const string ResultPath =
            "Temp/NTSD28_B3_C06_NestedPhysics.result.json";

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
            LF2Character character = null;
            ObserverCharacter observer = null;
            try
            {
                driver.SetPaused(true);
                int slot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                int observerSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    slot + 1,
                    1000);
                Require(slot >= 50 && observerSlot > slot,
                    "No two ordered transient runtime slots are available.");

                character = CreateCharacter(world, slot, 10650);
                observer = CreateObserver(world, observerSlot, 10651, character);
                character.Health.HP = -5;
                character.Health.HPBound = 340;
                character.Health.HP3 = 500;
                character.Health.PP = 229;
                character.Health.MaxPP = 480;
                character.Health.MP = 17;
                character.Health.MaxMP = 19;
                character.Runtime.SetPosition(200, 0, 200);
                character.Runtime.SyncIntegerPosition();

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the C06 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.slot = slot;
                report.observerSlot = observerSlot;
                report.hp = character.Health.HP;
                report.hpBound = character.Health.HPBound;
                report.hp3 = character.Health.HP3;
                report.pp = character.Health.PP;
                report.ppMax = character.Health.MaxPP;
                report.mp = character.Health.MP;
                report.mpMax = character.Health.MaxMP;
                report.c06ObservedHpBound = observer.ObservedHpBound;
                report.c06ObservedPp = observer.ObservedPp;
                Require(driver.CurrentTickIndex == expectedTick,
                    "Production tick index did not advance exactly once.");
                Require(observer.Observed && report.c06ObservedHpBound == 0 &&
                        report.c06ObservedPp == 0,
                    "Higher-slot physics did not observe lower-slot C06 normalization.");
                Require(report.hp == -5 && report.hp3 == 500 &&
                        report.ppMax == 480 && report.mp == 17 && report.mpMax == 19,
                    "C06 or a later pass changed a field outside the native normalizer contract.");

                report.status = "PASS";
                report.message =
                    "Higher-slot physics observed dead type0 HPBound/PP cleared inside C06; unrelated fields were preserved.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                if (character != null)
                    world.Unregister(character);
                if (observer != null)
                    world.Unregister(observer);
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

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>(),
            };
            var data = new LF2CharacterData
            {
                name = "C06Play_" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            var entity = new LF2Character();
            entity.ModuleInitialize();
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private static ObserverCharacter CreateObserver(
            SimulationWorld world,
            int slot,
            int objectId,
            LF2Character subject)
        {
            LF2Character bound = CreateCharacter(world, slot, objectId);
            world.Unregister(bound);

            var observer = new ObserverCharacter(subject);
            observer.ModuleInitialize();
            observer.Name = bound.Name;
            observer.ObjectId = objectId;
            observer.FrameCache.Load(bound.FrameCache.Wrapper);
            observer.Frame.N = 0;
            observer.Frame.PN = 0;
            observer.Frame.D = observer.FrameCache.GetFrameDataById(0);
            observer.Initialize(500, 500);
            observer.SetRequiredRuntimeSlot(slot);
            world.Register(observer);
            return observer;
        }

        private sealed class ObserverCharacter : LF2Character
        {
            private readonly LF2Character subject;

            internal ObserverCharacter(LF2Character subject)
            {
                this.subject = subject;
            }

            internal bool Observed { get; private set; }
            internal int ObservedHpBound { get; private set; }
            internal int ObservedPp { get; private set; }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                Observed = true;
                ObservedHpBound = subject.Health.HPBound;
                ObservedPp = subject.Health.PP;
                return true;
            }
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
                Debug.Log("[NTSD28C06NestedPhysicsPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C06NestedPhysicsPlayProbe] " + report.message);
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int slot;
            public int observerSlot;
            public int hp;
            public int hpBound;
            public int hp3;
            public int pp;
            public int ppMax;
            public int mp;
            public int mpMax;
            public int c06ObservedHpBound;
            public int c06ObservedPp;
            public bool cleanupPassed;
        }
    }
}
#endif
