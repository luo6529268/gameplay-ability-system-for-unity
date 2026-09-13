#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.EditorTools;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q05LoganReplayEditorTests
    {
        private const string Root = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string Output = "Temp/NTSD28Q05Replay";

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void ActualLoganInputCheckpointReplaysEveryRetainedTick(BattleRuntimeProfile profile, bool attackScenario)
        {
            string scenario = attackScenario ? NTSD28UnityRawCaptureEditor.StandingAttackRngScenario : NTSD28UnityRawCaptureEditor.InputScenario;
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(Root, scenario, profile, 24, (driver, frames, identity) =>
            {
                var world = driver.World;
                if (attackScenario)
                {
                    ReplayAttackThroughLogicExecutor(world, frames, identity, profile);
                    return;
                }
                var session = new BattleLockstepSession(driver, identity, 0, 64, 64);
                var checkpoint = session.CreateBattleStateSnapshotBufferForBootstrap();
                var actor = world.FindEntityByRuntimeSlotForQuery(0);
                int rawSlot = world.RuntimeSlotCapacity - 1;
                var trace = new List<object>();
                var states = new HashSet<string>();
                for (int tick = 1; tick <= frames.Length; tick++)
                {
                    Assert.That(session.TryAdvanceManual(frames[tick - 1], buildPresentation: false), Is.True, session.LastReason.ToString());
                    if (tick == 2)
                    {
                        actor.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = 17;
                        var raw = world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot);
                        raw.ObjectAiExcludedGroupSourceSlot2F8 = 137;
                        raw.InputHistory[4] = 53;
                        Assert.That(session.TryCaptureBattleStateSnapshot(checkpoint), Is.True);
                    }
                    states.Add(actor.Runtime.Frame + ":" + actor.Runtime.X + ":" + actor.Runtime.Y);
                    trace.Add(new { tick, action = actor.Runtime.Frame, x = actor.Runtime.X, y = actor.Runtime.Y,
                        checksum = driver.LastFrameChecksumValue.ToString("X16") });
                }
                Assert.That(states.Count, Is.GreaterThan(1), "The scenario must exercise actual action or movement state.");
                string expected = world.CaptureLockstepChecksumSnapshot(24, frames[23]).OverallChecksum;
                actor.Runtime.X += 10000;
                actor.Runtime.HP = 1;
                actor.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = -1;
                var corruptedRaw = world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot);
                corruptedRaw.ObjectAiExcludedGroupSourceSlot2F8 = -1;
                corruptedRaw.InputHistory[4] = 0;
                world.Rng.NextInt(0, 97);
                bool replayed = session.TryRestoreAndReplay(checkpoint);
                Write(profile + "-" + attackScenario + "-input", new { replayed, reason = session.LastReason.ToString(),
                    stoppedTick = driver.CurrentTickIndex, checkpoint = checkpoint.CapturedTick,
                    contentIdentity = identity.CatalogFingerprint.ToString("X16"), distinctStates = states.Count, trace });
                Assert.That(replayed, Is.True, "tick=" + driver.CurrentTickIndex + " " + session.LastReason);
                Assert.That(world.CaptureLockstepChecksumSnapshot(24, frames[23]).OverallChecksum, Is.EqualTo(expected));
                Assert.That(world.FindEntityByRuntimeSlotForQuery(0).Runtime.ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(17));
                Assert.That(world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot).ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(137));
                Assert.That(world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot).InputHistory[4], Is.EqualTo(53));
            });
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void RealDefinitionPoolReuseAndPriorGenerationRestoreAreIsolated(BattleRuntimeProfile profile)
        {
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(Root, NTSD28UnityRawCaptureEditor.DefaultScenario,
                profile, 3, (driver, frames, identity) =>
            {
                var world = driver.World;
                Assert.That(driver.StepOneTick(frames[0], true, false), Is.True);
                int slot = world.DynamicRuntimeSlotStartForServices;
                LF2Entity first = SpawnOther(world, slot);
                first.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = 37;
                Assert.That(world.TryGetRuntimeSlotReadOnlyView(slot, out var firstView), Is.True);
                var oldHandle = new RuntimeEntityHandle(slot, firstView.Generation);
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 1, snapshot), Is.True);
                string expected = world.CaptureLockstepChecksumSnapshot(1, frames[0]).OverallChecksum;
                var wrongContent = new LockstepSessionIdentity(identity.SchemaVersion, identity.SessionId,
                    identity.Seed, identity.CatalogFingerprint ^ 1UL, identity.StageFingerprint, identity.CanonicalPlayerSlots);
                Assert.That(driver.TryRestoreBattleStateSnapshot(wrongContent, snapshot, out var rejected), Is.False);
                Assert.That(rejected, Is.EqualTo(BattleStateSnapshotRestoreFailure.IdentityMismatch));
                Assert.That(world.CaptureLockstepChecksumSnapshot(1, frames[0]).OverallChecksum, Is.EqualTo(expected));
                first.UnregisterFromWorld();
                first.Reset();
                world.LogicReferencePool.Release(first);
                LF2Entity second = SpawnOther(world, slot);
                Assert.That(second, Is.SameAs(first), "The private test pool must actually reuse the object.");
                Assert.That(second.Runtime.ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(-1));
                Assert.That(world.TryGetRuntimeSlotReadOnlyView(slot, out var futureView), Is.True);
                var futureHandle = new RuntimeEntityHandle(slot, futureView.Generation);
                Assert.That(futureHandle.Generation, Is.GreaterThan(oldHandle.Generation));
                Assert.That(world.TryResolveRuntimeHandle(oldHandle, out _), Is.False);
                snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
                bool restored = driver.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure);
                Write(profile + "-pool", new { restored, failure = failure.ToString(), oldGeneration = oldHandle.Generation,
                    futureGeneration = futureHandle.Generation, poolActive = world.LogicReferencePool.ActiveCount });
                Assert.That(restored, Is.True, failure.ToString());
                Assert.That(world.TryResolveRuntimeHandle(oldHandle, out LF2Entity recovered), Is.True);
                Assert.That(world.TryResolveRuntimeHandle(futureHandle, out _), Is.False);
                Assert.That(recovered.Runtime.ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(37));
                Assert.That(world.CaptureLockstepChecksumSnapshot(1, frames[0]).OverallChecksum, Is.EqualTo(expected));
            });
        }

        private static void ReplayAttackThroughLogicExecutor(SimulationWorld world, FrameInputSet[] frames,
            LockstepSessionIdentity identity, BattleRuntimeProfile profile)
        {
            var executor = new BattleWorldSimulationTickExecutor(world, new NTSDBattleTickSystem(world), true);
            var stage = BattleSimulationStageSnapshot.Capture(world.Runtime.Stage);
            var expected = new ulong[frames.Length];
            var checkpoint = world.CreateBattleStateSnapshotBufferForBootstrap();
            var structuralEvents = new BattleParityStructuralEventBuffer(world.RuntimeSlotCapacity);
            world.SetStructuralEventSinkForDiagnostics(structuralEvents, 0, "Q05-replay");
            long initialRegistrations = world.StructuralWriterDiagnosticsForDiagnostics.RegisterCount;
            int maximumObjects = 0;
            for (int index = 0; index < frames.Length; index++)
            {
                expected[index] = executor.Execute(new BattleSimulationTickRequest(frames[index], false, stage)).StateChecksum;
                maximumObjects = Math.Max(maximumObjects, world.ObjectCount);
                if (index == 1)
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 2, checkpoint), Is.True);
            }
            long spawns = world.StructuralWriterDiagnosticsForDiagnostics.SpawnCount;
            long registrations = world.StructuralWriterDiagnosticsForDiagnostics.RegisterCount - initialRegistrations;
            Write(profile + "-attack-structural", new { maximumObjects, spawns, registrations, events = structuralEvents.Events });
            world.SetStructuralEventSinkForDiagnostics(null, 24, "Q05-replay");
            Assert.That(spawns, Is.GreaterThan(0), "The attack must reach the OPoint spawn entry.");
            Assert.That(registrations, Is.GreaterThan(0), "A generated object must actually register.");
            Assert.That(structuralEvents.Events.Any(birth => birth.Action == "allocate" &&
                structuralEvents.Events.Any(death => death.Action == "unregister-flush" &&
                    death.Slot == birth.Slot && death.Tick >= birth.Tick)), Is.True,
                "The generated object must complete its registered lifetime.");
            string fullExpected = world.CaptureLockstepChecksumSnapshot(24, frames[23]).OverallChecksum;
            world.FindEntityByRuntimeSlotForQuery(0).Runtime.HP = 1;
            checkpoint.RuntimeSlots.ClearLocalEntityShellsForTransfer();
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, checkpoint, out var failure), Is.True, failure.ToString());
            var replayEvents = new BattleParityStructuralEventBuffer(world.RuntimeSlotCapacity);
            world.SetStructuralEventSinkForDiagnostics(replayEvents, 2, "Q05-replay");
            var replay = new List<object>();
            for (int index = 2; index < frames.Length; index++)
            {
                ulong actual = executor.Execute(new BattleSimulationTickRequest(frames[index], false, stage)).StateChecksum;
                replay.Add(new { tick = index + 1, expected = expected[index].ToString("X16"), actual = actual.ToString("X16") });
                Write(profile + "-attack-executor", new { maximumObjects, spawns, replay });
                Assert.That(actual, Is.EqualTo(expected[index]), "Replay tick " + (index + 1));
            }
            Assert.That(world.CaptureLockstepChecksumSnapshot(24, frames[23]).OverallChecksum, Is.EqualTo(fullExpected));
            Assert.That(replayEvents.Events.Select(EventKey), Is.EqualTo(structuralEvents.Events
                .Where(value => value.Tick > 2).Select(EventKey)), "Restored structural lifecycle must repeat in order.");
            world.SetStructuralEventSinkForDiagnostics(null, 24, "Q05-replay");
        }

        private static string EventKey(BattleParityStructuralEvent value) =>
            value.Tick + ":" + value.Pass + ":" + value.Action + ":" + value.Slot + ":" + value.Before + ":" + value.After;

        private static LF2Entity SpawnOther(SimulationWorld world, int slot)
        {
            var wrapper = world.RuntimeDataCatalog.GetCharacterConfig(124);
            Assert.That(wrapper.characterData.frames.Any(frame => frame.frameId == 40), Is.True);
            var task = world.LogicReferencePool.Fetch<OPointCreateTask>();
            task.targetWorld = world;
            task.requiredRuntimeSlot = slot;
            task.opoint = new ObjectPoint { oid = 124, kind = 1, action = 40 };
            task.dir = "right";
            task.useDirectRuntimePosition = true;
            task.directX = 630;
            task.directY = -30;
            task.directZ = 650;
            task.skipPostInitZOffset = true;
            try
            {
                var entity = world.LogicEntityFactory.Create(task, out var failure);
                Assert.That(entity, Is.Not.Null, failure.ToString());
                return entity;
            }
            finally { world.LogicReferencePool.Recycle(task); }
        }

        private static void Write(string name, object value)
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output, name + ".json"), JsonConvert.SerializeObject(value, Formatting.Indented));
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q05ReplayPlayProbe
    {
        private const string RequestPath = "Temp/NTSD28_Q05_ReplayPlay.request";
        private const string ResultPath = "Temp/NTSD28_Q05_ReplayPlay.result.json";

        static NTSD28Q05ReplayPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(RequestPath) || File.ReadAllText(RequestPath).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5 || !driver.World.IsBattleSnapshotBoundaryReady) return;
            File.WriteAllText(RequestPath, "running");
            Run(driver).Forget();
        }

        private static async UniTask Run(SimulationTickDriver driver)
        {
            var report = new Report { tick = driver.CurrentTickIndex };
            bool safelyStopped = false;
            try
            {
                driver.SetPaused(true);
                SimulationWorld world = driver.World;
                BattleLogicReferencePool logicPool = world.LogicReferencePool;
                LF2ObjectPool renderPool = LF2ObjectPool.TryGetInstance();
                Assert.That(renderPool, Is.Not.Null);
                report.objectsBefore = world.ObjectCount;
                LF2Entity actor = Enumerable.Range(0, world.RuntimeSlotCapacity)
                    .Select(world.FindEntityByRuntimeSlotForQuery).First(value => value?.Renderer != null);
                var renderer = actor.Renderer;
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, snapshot), Is.True);
                report.configuration = new
                {
                    lifecycle = driver.LifecycleState.ToString(),
                    worldProfile = world.RuntimeProfileForServices.ToString(),
                    snapshotProfile = snapshot.Core.RuntimeProfile.ToString(),
                    worldCapacity = world.RuntimeSlotCapacity,
                    snapshotCapacity = snapshot.Core.RuntimeSlotCapacity,
                    worldBroadphase = world.CollisionBroadphaseForServices.ToString(),
                    snapshotBroadphase = snapshot.Core.CollisionBroadphase.ToString(),
                    snapshotObjects = snapshot.Core.ObjectCount,
                    snapshotClaimed = snapshot.Core.ClaimedRuntimeSlotCount,
                    slotClaimed = snapshot.RuntimeSlots.ClaimedCount,
                    capacities = new[] { snapshot.RuntimeSlots.SlotCapacity, snapshot.EntityRuntime.SlotCapacity,
                        snapshot.EntityBaseShell.SlotCapacity, snapshot.LivingShell.SlotCapacity,
                        snapshot.CharacterShell.SlotCapacity, snapshot.WeaponShell.SlotCapacity,
                        snapshot.SpecialOtherShell.SlotCapacity, snapshot.Rest.LogicalCapacity },
                    pendingUnregister = world.BattleBuffersForServices.PendingUnregister.Count,
                    pendingDestroy = world.BattleBuffersForServices.PendingSlotReleasedDestroy.Count,
                    snapshotUnregister = snapshot.PendingEvents.PendingUnregisterCount,
                    snapshotDestroy = snapshot.PendingEvents.PendingSlotReleasedDestroyCount,
                    soundCapacity = world.BattleBuffersForServices.PendingSounds.Capacity,
                    snapshotSoundCount = snapshot.PendingEvents.SoundCount,
                };
                var input = new FrameInputSet(report.tick, Array.Empty<SimulationPlayerInput>());
                string expected = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum;
                int borrowers = logicPool.ActiveCount;
                actor.Runtime.X += 1000;
                actor.Runtime.HP = 1;
                actor.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = 137;
                Assert.That(driver.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                Assert.That(world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum, Is.EqualTo(expected));
                Assert.That(actor.Renderer, Is.SameAs(renderer));
                Assert.That(logicPool.ActiveCount, Is.EqualTo(borrowers));
                report.objectsAfterRestore = world.ObjectCount;
                Assert.That(report.objectsAfterRestore, Is.EqualTo(report.objectsBefore));
                report.restoreAndRendererRetention = true;

                var app = UnityEngine.Object.FindObjectOfType<AppManager>();
                BattleRuntimeShutdownReport shutdown;
                if (app != null)
                {
                    Assert.That(app.TryShutdownBattleRuntimeBeforeSceneDestroy(out shutdown), Is.True, shutdown.FailureReason);
                }
                else
                {
                    shutdown = driver.ShutdownBattleRuntime();
                    Assert.That(shutdown.Status, Is.Not.EqualTo(BattleRuntimeShutdownStatus.Failed), shutdown.FailureReason);
                    var bootstraps = UnityEngine.Object.FindObjectsOfType<BattleBootstrap>(true);
                    foreach (var bootstrap in bootstraps) bootstrap.DisablePresentation();
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(bootstraps.All(value => value.IsRuntimeMapCleared));
                }
                Assert.That(shutdown.IsComplete, Is.True, shutdown.FailureReason);
                safelyStopped = true;
                await UniTask.NextFrame();
                await UniTask.NextFrame();
                report.worldObjectsAfterShutdown = world.ObjectCount;
                report.slotsAfterShutdown = world.ClaimedRuntimeSlotCountForDiagnostics;
                report.logicBorrowersAfterShutdown = logicPool.ActiveCount;
                report.renderBorrowersAfterShutdown = renderPool.ActiveObjectCountForAcceptance + renderPool.ActiveSpriteCountForAcceptance;
                Assert.That(report.worldObjectsAfterShutdown + report.slotsAfterShutdown +
                    report.logicBorrowersAfterShutdown + report.renderBorrowersAfterShutdown, Is.Zero);
                Assert.That(driver.World, Is.Null);
                Assert.That(driver.LifecycleState, Is.EqualTo(BattleRuntimeLifecycleState.Stopped));
                Assert.That(renderPool.IsQuiescedForDiagnostics && !renderPool.AcceptingRequestsForDiagnostics, Is.True);
                report.remainedStoppedAfterTwoFrames = true;
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            finally
            {
                File.WriteAllText(ResultPath, JsonConvert.SerializeObject(report, Formatting.Indented));
                File.WriteAllText(RequestPath, "done");
                if (safelyStopped) EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private sealed class Report
        {
            public string status, error;
            public object configuration;
            public string scope = "Current Unity legacy content, real Scene in-place snapshot restore and existing ordered shutdown; no physical input or Logan image parity claim.";
            public int tick, objectsBefore, objectsAfterRestore, worldObjectsAfterShutdown, slotsAfterShutdown;
            public int logicBorrowersAfterShutdown, renderBorrowersAfterShutdown;
            public bool restoreAndRendererRetention, remainedStoppedAfterTwoFrames;
        }
    }
}
#endif
