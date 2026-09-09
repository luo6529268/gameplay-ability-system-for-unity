#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28B6HeldRefillMpExhaustionPlayModeProbeEditor
    {
        private static readonly string RequestPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "Temp/NTSD28_B6_HeldRefillMpExhaustion.request"));
        private static readonly string ResultPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "Temp/NTSD28_B6_HeldRefillMpExhaustion.result.json"));
        private static SimulationTickDriver driver;
        private static bool pauseRequested;
        private static string unrelatedError;
        private static double deadline;
        private static SpawnAudit spawnAudit;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            Application.logMessageReceived -= ObserveLog;
            Application.logMessageReceived += ObserveLog;
        }

        private static void ObserveLog(string condition, string stackTrace, LogType type)
        {
            if (EditorApplication.isPlaying && File.Exists(RequestPath) &&
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
            {
                unrelatedError = condition;
            }
        }

        private static void Poll()
        {
            if (!File.Exists(RequestPath) || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (!EditorApplication.isPlaying)
                return;
            driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;
            if (!pauseRequested)
            {
                driver.SetPaused(true);
                pauseRequested = true;
                deadline = EditorApplication.timeSinceStartup + 20;
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                EditorApplication.timeSinceStartup < deadline)
                return;

            var report = new Report();
            SimulationWorld world = driver.World;
            report.startTick = driver.CurrentTickIndex;
            report.baselineObjects = world.ObjectCount;
            report.baselineSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            try
            {
                Require(string.IsNullOrEmpty(unrelatedError), "New Console error: " + unrelatedError);
                Require(SceneManager.GetActiveScene().name == "NTSD_Battle", "Wrong active scene.");
                Require(!driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                    "Worker did not become idle.");
                Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                    "Dedicated worker failure.");
                Require(world.StructuralEventSinkForServices == null,
                    "An existing structural observer must not be replaced.");
                spawnAudit = new SpawnAudit(world, report);
                world.SetStructuralEventSinkForDiagnostics(spawnAudit, driver.CurrentTickIndex, "B6-refill-probe");
                LF2Entity sceneCharacter = null;
                for (int slot = 0; slot < 20 && sceneCharacter == null; slot++)
                {
                    LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                    if (candidate?.GetCurrentDataObjectTypeForSimulation() == 0)
                        sceneCharacter = candidate;
                }
                Require(sceneCharacter != null, "No production roster character is available.");
                report.holderOid = sceneCharacter.ObjectId;
                LF2CharacterDataWrapper holderData = world.RuntimeCharacterConfigs.Resolve(report.holderOid);
                Require(holderData?.characterData != null, "Production holder DAT is unavailable.");
                RunCase(world, holderData, report, "milk_sequence", 122, 12, -1, 400, 0, 100, 6.5);
                RunCase(world, holderData, report, "juice_sequence_negative_gate", 123, 10, -1, 500, 0, 200, -4.25);
                RunCase(world, holderData, report, "juice_sequence_zero_gate", 123, 10, 0, 500, -1, 200, 3.75);
                RunCase(world, holderData, report, "juice_zero_hp", 123, 0, 0, 400, -1, 70, 9.0);
                RunCase(world, holderData, report, "juice_negative_hp", 123, -4, -1, 123, 0, 70, -7.5);
                RunCase(world, holderData, report, "juice_cap_boundary", 123, 4, 1, 150, -1, 499, 5.25);
                Require(string.IsNullOrEmpty(unrelatedError), "New Console error: " + unrelatedError);
                report.status = "PASS";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                report.endTick = driver.CurrentTickIndex;
                report.finalObjects = world.ObjectCount;
                report.finalSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
                report.activeAmbientSpawns = spawnAudit?.ActiveCount ?? 0;
                report.cleanupPassed = report.finalObjects == report.baselineObjects + report.activeAmbientSpawns &&
                    report.finalSlots == report.baselineSlots + report.activeAmbientSpawns;
                if (ReferenceEquals(world.StructuralEventSinkForServices, spawnAudit))
                    world.SetStructuralEventSinkForDiagnostics(null, driver.CurrentTickIndex, "B6-refill-probe-end");
                if (!report.cleanupPassed)
                {
                    report.status = "FAIL";
                    report.message += " Object/slot baseline was not restored.";
                }
                File.WriteAllText(ResultPath, JsonUtility.ToJson(report, true));
                File.Delete(RequestPath);
                EditorApplication.update -= Poll;
                Application.logMessageReceived -= ObserveLog;
                // Keep the host paused until its ordinary ordered Play shutdown.
                EditorApplication.delayCall += ExitPlay;
            }
        }

        private static void RunCase(SimulationWorld world, LF2CharacterDataWrapper holderData,
            Report report, string label, int oid, int initialHp, int gate, int childPp,
            int killCount, int holderPp, double vz)
        {
            LF2Character holder = null;
            ObservedDrink child = null;
            var row = new CaseReport { label = label, oid = oid, initialHp = initialHp };
            report.cases.Add(row);
            int countBefore = world.ObjectCount;
            int slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            int ambientBefore = spawnAudit.ActiveCount;
            try
            {
                LF2CharacterDataWrapper childData = world.RuntimeCharacterConfigs.Resolve(oid);
                Require(childData?.characterData != null && childData.characterData.type_sub == oid,
                    label + ": current production drink DAT is unavailable.");
                row.holderOid = oid == 122 ? 1 : 2;
                holderData = world.RuntimeCharacterConfigs.Resolve(row.holderOid);
                Require(holderData?.characterData != null, label + ": source character DAT unavailable.");
                LF2FrameData drinking = holderData.characterData.frames.FirstOrDefault(frame =>
                    frame.state == 17 && frame.PrimaryWeaponPoint.Kind != 3 &&
                    frame.PrimaryWeaponPoint.Dvx == 0 && childData.characterData.frames.Any(
                        item => item.frameId == frame.PrimaryWeaponPoint.WeaponAct));
                Require(drinking != null, label + ": current DAT drinking frame is unavailable.");
                row.drinkingAction = drinking.frameId;
                row.attachmentAction = oid == 122 ? 31 : 20;
                int holderSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                int childSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(holderSlot + 1, 1000);
                Require(holderSlot >= 50 && childSlot > holderSlot, "No two free transient slots.");

                holder = new LF2Character();
                holder.ModuleInitialize();
                holder.ObjectId = row.holderOid;
                holder.Name = "B6RefillPlayHolder_" + label;
                holder.FrameCache.Load(holderData);
                holder.SetRequiredRuntimeSlot(holderSlot);
                world.Register(holder);
                holder.ImmediateFrame(0);
                holder.Initialize(500, 500);
                holder.AiControlled = false;
                holder.Team = 3;
                holder.RelationTeam = 3;
                holder.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                holder.Runtime.SyncIntegerPosition();

                child = new ObservedDrink { ObjectId = oid, Name = "B6RefillPlayDrink_" + label };
                child.SetWeaponType((int)LF2ObjectType.Drink);
                child.FrameCache.Load(childData);
                child.SetRequiredRuntimeSlot(childSlot);
                world.Register(child);
                child.ImmediateFrame(row.attachmentAction);
                child.Health.HP = Math.Max(initialHp, 1);
                child.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                child.Runtime.SyncIntegerPosition();
                holder.AttachOpointHeldObject(child);
                row.pickupAccepted = holder.GetHeldWeapon() == child;
                Require(row.pickupAccepted && holder.Runtime.TargetSlotIndex == childSlot &&
                    child.Runtime.HolderStableId == holderSlot && child.Runtime.LinkState < 0,
                    label + ": production OPoint kind2 relation attachment failed.");
                row.holderSlot = holderSlot;
                row.childSlot = childSlot;
                holder.ImmediateFrame(drinking.frameId);
                holder.FrameDelay = 1000;
                holder.Health.HP = 100;
                holder.Health.HPBound = 200;
                holder.Health.HP3 = 500;
                holder.Health.PP = holderPp;
                holder.AttackingCounter = 9;
                child.Health.HP = initialHp;
                child.Health.HPBound = Math.Max(initialHp, 1);
                child.Health.PP = childPp;
                child.Runtime.OrdinaryCreditGate2F4 = gate;
                child.KillCount = killCount;
                child.Runtime.WeaponFlightCounter = 31;
                child.AttackingCounter = 8;
                child.Runtime.SetVelocity(12, -8, vz);
                child.PS.zz = 17;
                child.Row = row;

                for (int index = 0; index < 20 && !row.exhausted; index++)
                {
                    Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                        label + ": production driver rejected tick.");
                    Require(string.IsNullOrEmpty(row.failure), row.failure);
                    Require(string.IsNullOrEmpty(unrelatedError), "New Console error: " + unrelatedError);
                }
                Require(row.exhausted, label + ": refill did not reach exhaustion within 20 ticks.");
                Require(row.samples.Count == (oid == 122 ? initialHp : Math.Max(1, (initialHp + 1) / 2)),
                    label + ": unexpected number of refill consumptions.");
                row.passed = true;
            }
            finally
            {
                if (child?.RegisteredWorldForSimulation == world)
                    world.Unregister(child);
                if (holder?.RegisteredWorldForSimulation == world)
                    world.Unregister(holder);
                int ambientDelta = spawnAudit.ActiveCount - ambientBefore;
                row.cleanupPassed = world.ObjectCount == countBefore + ambientDelta &&
                    world.ClaimedRuntimeSlotCountForDiagnostics == slotsBefore + ambientDelta &&
                    (child == null || child.RegisteredWorldForSimulation == null) &&
                    (holder == null || holder.RegisteredWorldForSimulation == null);
                Require(row.cleanupPassed, label + ": owned entity cleanup failed.");
            }
        }

        private sealed class ObservedDrink : LF2Weapon
        {
            internal CaseReport Row;

            public override WeaponActResult Act(LF2Entity holder, BattleWeaponPointValue wpoint, Vector3 holdpoint)
            {
                if (Row == null || holder.Frame.D.state != 17)
                    return base.Act(holder, wpoint, holdpoint);
                var sample = new Sample
                {
                    tick = Match.CurrentTickIndex,
                    hpBefore = Health.HP, holderHpBefore = holder.Health.HP,
                    holderBoundBefore = holder.Health.HPBound, holderPpBefore = holder.Health.PP,
                    childPpBefore = Health.PP, gate = Runtime.OrdinaryCreditGate2F4,
                    killCount = KillCount, vxBefore = Runtime.Vx, vyBefore = Runtime.Vy,
                    vzBefore = Runtime.Vz, zzBefore = PS.zz, rngBefore = Match.Rng.CallCount,
                    rngStateBefore = Match.Rng.State,
                };
                var predictor = new DeterministicRng(Match.Rng.State);
                sample.expectedExhaustVx = predictor.NextInt(0, 7) - 3;
                WeaponActResult result = base.Act(holder, wpoint, holdpoint);
                sample.hpAfter = Health.HP;
                sample.holderHpAfter = holder.Health.HP;
                sample.holderBoundAfter = holder.Health.HPBound;
                sample.holderPpAfter = holder.Health.PP;
                sample.childPpAfter = Health.PP;
                sample.vxAfter = Runtime.Vx;
                sample.vyAfter = Runtime.Vy;
                sample.vzAfter = Runtime.Vz;
                sample.zzAfter = PS.zz;
                sample.rngAfter = Match.Rng.CallCount;
                sample.rngStateAfter = Match.Rng.State;
                sample.forceDrop = result.ForceDrop;
                sample.holderAction = holder.Frame.N;
                sample.childAction = Frame.N;
                sample.holderCounter = holder.AttackingCounter;
                sample.childCounter = AttackingCounter;
                sample.holderLink = holder.Runtime.LinkState;
                sample.childLink = Runtime.LinkState;
                sample.holderTarget = holder.Runtime.TargetSlotIndex;
                sample.childHolder = Runtime.HolderStableId;
                sample.weaponHp = Runtime.WeaponFlightCounter;
                Row.samples.Add(sample);
                try
                {
                    int expectedHp = sample.hpBefore - (ObjectId == 122 ? 1 : 2);
                    Require(sample.hpAfter == expectedHp, Row.label + ": child HP arithmetic.");
                    int expectedPp = ObjectId == 123 ? Math.Min(sample.holderPpBefore + 3, 500) :
                        (expectedHp % 6 == 0 ? Math.Min(sample.holderPpBefore + 5, 500) : sample.holderPpBefore);
                    Require(sample.holderPpAfter == expectedPp, Row.label + ": holder MP arithmetic/cap target.");
                    int expectedChildPp = ObjectId == 123 && sample.gate >= 0 && sample.childPpBefore > 150
                        ? 150 : sample.childPpBefore;
                    Require(sample.childPpAfter == expectedChildPp, Row.label + ": exact child MP cap.");
                    Require(KillCount == sample.killCount && Runtime.OrdinaryCreditGate2F4 == sample.gate,
                        Row.label + ": gate/sentinel mutated.");
                    if (ObjectId == 122)
                    {
                        int bound = expectedHp % 5 == 0 ? Math.Min(sample.holderBoundBefore + 2, holder.Health.HP3)
                            : sample.holderBoundBefore;
                        int hp = expectedHp % 5 == 0 ? Math.Min(sample.holderHpBefore + 4, bound) : sample.holderHpBefore;
                        Require(sample.holderBoundAfter == bound && sample.holderHpAfter == hp,
                            Row.label + ": existing HP-refill subset.");
                    }
                    bool exhausted = expectedHp <= 0;
                    Require(result.ForceDrop == exhausted, Row.label + ": exhaustion decision.");
                    Require(sample.rngAfter - sample.rngBefore == (exhausted ? 1UL : 0UL),
                        Row.label + ": RNG call delta.");
                    if (exhausted)
                    {
                        Require(sample.vxAfter == sample.expectedExhaustVx && sample.vyAfter == 0 &&
                            sample.vzAfter == sample.vzBefore && sample.vzBefore != 0 && sample.zzAfter == 0,
                            Row.label + ": exhaustion motion/zz contract.");
                        Require(sample.holderAction == 0 && sample.childAction == 0 &&
                            sample.holderCounter == 0 && sample.childCounter == 0 &&
                            sample.holderLink == 0 && sample.childLink == 0 &&
                            sample.holderTarget == 0 && sample.childHolder == 0 && sample.weaponHp == 0,
                            Row.label + ": exhaustion action/counter/relation/weaponHP reset.");
                        Row.exhausted = true;
                    }
                    else
                    {
                        Require(sample.holderLink != 0 && sample.childLink < 0 &&
                            sample.childHolder == holder.Runtime.SlotIndex &&
                            sample.holderTarget == Runtime.SlotIndex, Row.label + ": premature relation release.");
                    }
                    sample.passed = true;
                }
                catch (Exception exception)
                {
                    Row.failure = exception.Message;
                }
                return result;
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private sealed class SpawnAudit : IBattleParityStructuralEventSink
        {
            private readonly SimulationWorld world;
            private readonly Report report;
            private readonly List<LF2Entity> ambient = new List<LF2Entity>();

            internal SpawnAudit(SimulationWorld world, Report report)
            {
                this.world = world;
                this.report = report;
            }

            internal int ActiveCount => ambient.Count(entity => entity.RegisteredWorldForSimulation == world &&
                ReferenceEquals(world.FindEntityByRuntimeSlotForQuery(entity.Runtime.SlotIndex), entity));

            public void Record(BattleParityStructuralEvent structuralEvent)
            {
                if (structuralEvent.Action != "allocate")
                    return;
                string stack = Environment.StackTrace;
                if (!stack.Contains("BattleRandomWeaponDropModule.RunNormalDrop"))
                    return;
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(structuralEvent.Slot);
                Require(entity != null, "Observed random-drop allocation has no occupant.");
                ambient.Add(entity);
                report.ambientSpawns.Add(new AmbientSpawn
                {
                    tick = world.CurrentTickIndex, slot = structuralEvent.Slot, oid = entity.ObjectId,
                    source = "BattleRandomWeaponDropModule.RunNormalDrop (observed allocation call stack)",
                });
            }
        }

        private static void ExitPlay()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        [Serializable]
        private sealed class Report
        {
            public string status, message;
            public int startTick, endTick, holderOid, baselineObjects, finalObjects, baselineSlots, finalSlots;
            public bool cleanupPassed;
            public int activeAmbientSpawns;
            public List<AmbientSpawn> ambientSpawns = new List<AmbientSpawn>();
            public List<CaseReport> cases = new List<CaseReport>();
        }

        [Serializable]
        private sealed class AmbientSpawn
        {
            public int tick, slot, oid;
            public string source;
        }

        [Serializable]
        private sealed class CaseReport
        {
            public string label, failure;
            public int oid, initialHp, holderOid, drinkingAction, attachmentAction, holderSlot, childSlot;
            public bool pickupAccepted, exhausted, passed, cleanupPassed;
            public List<Sample> samples = new List<Sample>();
        }

        [Serializable]
        private sealed class Sample
        {
            public int tick, hpBefore, hpAfter, holderHpBefore, holderHpAfter, holderBoundBefore, holderBoundAfter;
            public int holderPpBefore, holderPpAfter, childPpBefore, childPpAfter, gate, killCount, expectedExhaustVx;
            public int holderAction, childAction, holderCounter, childCounter, holderLink, childLink, holderTarget, childHolder, weaponHp;
            public double vxBefore, vyBefore, vzBefore, zzBefore, vxAfter, vyAfter, vzAfter, zzAfter;
            public ulong rngBefore, rngAfter;
            public uint rngStateBefore, rngStateAfter;
            public bool forceDrop, passed;
        }
    }
}
#endif
