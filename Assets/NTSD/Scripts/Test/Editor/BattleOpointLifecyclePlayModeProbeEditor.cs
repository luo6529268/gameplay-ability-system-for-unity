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
    /// <summary>
    /// Explicit Editor-only S4 probe for production opoint birth, scan-cursor
    /// visibility, runtime generation invalidation, and same-slot reuse.
    /// </summary>
    public static class BattleOpointLifecyclePlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/验证/R8/运行Opoint出生与生命周期Play探针";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01C_01_OpointLifecycle.result.json";
        private const int CharacterOid = 33;
        private const int WeaponOid = 120;
        private const int SpecialAttackOid = 203;
        private const int OtherOid = 999;
        private const int TickTimeoutEditorUpdates = 600;

        private static readonly int[] TargetOids =
        {
            CharacterOid,
            WeaponOid,
            SpecialAttackOid,
            OtherOid,
        };

        private static readonly List<LF2Entity> SnapshotBefore =
            new List<LF2Entity>(128);
        private static readonly List<LF2Entity> SnapshotAfter =
            new List<LF2Entity>(128);
        private static readonly List<LF2Entity> OwnedSpawns =
            new List<LF2Entity>(8);
        private static readonly List<BirthEvidence> BirthEvidenceRows =
            new List<BirthEvidence>(4);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPointFactory factory;
        private static LF2ObjectPool objectPool;
        private static ProbeEntity highProducer;
        private static ProbeEntity lowProducer;
        private static ProbeEntity lowFiller;
        private static LF2Entity highSpawn;
        private static LF2Entity lowSpawn;
        private static RuntimeEntityHandle previousBirthHandle;
        private static RuntimeEntityHandle highHandle;
        private static RuntimeEntityHandle lowHandle;
        private static ProbeResult result;
        private static ProbePhase phase;
        private static int expectedTick;
        private static int phaseEditorUpdates;
        private static int highAttackingAfterCreationTick;
        private static int lowAttackingAfterCreationTick;
        private static bool previousPaused;
        private static bool workerWasActive;
        private static bool running;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            ResetProbeState();

            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            factory = LF2ObjectPointFactory.Instance;
            objectPool = LF2ObjectPool.Instance;
            if (driver == null || world == null || factory == null ||
                objectPool == null || LF2ReferencePool.Instance == null ||
                GameDataManager.Instance == null ||
                CharacterAnimtorManager.Instance == null)
            {
                WriteImmediateFailure(
                    "The production driver, world, catalog managers, factory, or pools are not ready.");
                return;
            }

            string catalogFailure = ValidateTargetCatalog();
            if (!string.IsNullOrEmpty(catalogFailure))
            {
                WriteImmediateFailure(catalogFailure);
                return;
            }

            previousPaused = driver.IsPaused;
            workerWasActive = driver.DedicatedSimulationWorkerActiveForDiagnostics;
            result = new ProbeResult
            {
                status = "RUNNING",
                message = string.Empty,
                startTick = driver.CurrentTickIndex,
                workerWasActive = workerWasActive,
                baselineObjectCount = world.ObjectCount,
                baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics,
                baselineObjectPoolActive = objectPool.ActiveObjectCountForAcceptance,
                baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount,
            };

            driver.SetPaused(true);
            phase = ProbePhase.WaitingForQuiescence;
            phaseEditorUpdates = 0;
            running = true;
            EditorApplication.update += Observe;
            Debug.Log(
                "[BattleOpointLifecyclePlayModeProbe] Waiting for the production " +
                "simulation worker to reach an idle boundary.");
        }

        private static void Observe()
        {
            if (!running)
                return;

            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                Fail("Play Mode or the production world ended before the probe completed.");
                return;
            }

            phaseEditorUpdates++;
            if (phaseEditorUpdates > TickTimeoutEditorUpdates)
            {
                Fail($"Timed out in phase {phase}.");
                return;
            }

            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Fail(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }

            try
            {
                switch (phase)
                {
                    case ProbePhase.WaitingForQuiescence:
                        if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                            return;
                        ExecuteDirectBirthMatrix();
                        StartHighSlotWitness();
                        break;

                    case ProbePhase.WaitingForHighTick:
                        if (!TickCompleted())
                            return;
                        CompleteHighSlotWitness();
                        StartLowSlotWitness();
                        break;

                    case ProbePhase.WaitingForLowCreationTick:
                        if (!TickCompleted())
                            return;
                        CompleteLowCreationTick();
                        ScheduleTick(ProbePhase.WaitingForLowConsumerTick);
                        break;

                    case ProbePhase.WaitingForLowConsumerTick:
                        if (!TickCompleted())
                            return;
                        CompleteLowConsumerTick();
                        FinishSuccess();
                        break;
                }
            }
            catch (Exception exception)
            {
                Fail($"Unhandled probe exception in phase {phase}: {exception}");
            }
        }

        private static void ExecuteDirectBirthMatrix()
        {
            highProducer = new ProbeEntity("R8OpointHighProducer", hasOpoint: true);
            world.Register(highProducer);
            RequireRegistered(highProducer, "high producer");

            previousBirthHandle = RuntimeEntityHandle.Invalid;
            for (int index = 0; index < TargetOids.Length; index++)
            {
                int oid = TargetOids[index];
                highProducer.SetSpawnOid(oid);
                CaptureSnapshot(SnapshotBefore);
                int objectCountBefore = world.ObjectCount;
                int poolActiveBefore = objectPool.ActiveObjectCountForAcceptance;
                int logicActiveBefore = LF2ReferencePool.Instance.ActiveCount;

                // Alignment contract: R8-OPLIFE-001. Invoke the production late-opoint
                // materializer at an idle live-world boundary so birth fields are
                // observed before the newborn receives a later scan-cursor visit.
                factory.ProcessOpointSpawn(highProducer);

                LF2Entity spawned = FindSingleNewSpawn(oid, SnapshotBefore);
                RuntimeEntityHandle handle = RequireHandle(spawned, $"birth oid {oid}");
                ObjectDefinition definition = GameDataManager.Instance.GetObjectById(oid);
                int actualType = spawned.GetCurrentDataObjectTypeForSimulation();
                bool expectedClrType = IsExpectedClrType(spawned, definition.type);
                bool birthFrameCorrect = spawned.Frame != null &&
                    spawned.Frame.N == 0 &&
                    spawned.Runtime.Frame == 0;
                bool prev2Correct = spawned.Frame != null &&
                    spawned.Frame.Prev2 == 0 &&
                    spawned.Runtime.PrevFrame2 == 0;
                bool slotReused = !previousBirthHandle.IsValid ||
                    handle.Slot == previousBirthHandle.Slot;
                bool generationAdvanced = !previousBirthHandle.IsValid ||
                    handle.Generation != previousBirthHandle.Generation;

                var row = new BirthEvidence
                {
                    oid = oid,
                    expectedObjectType = definition.type,
                    actualObjectType = actualType,
                    expectedClrType = ExpectedClrTypeName(definition.type),
                    actualClrType = spawned.GetType().Name,
                    clrTypeMatched = expectedClrType,
                    producerSlot = highProducer.Runtime.SlotIndex,
                    slot = handle.Slot,
                    generation = handle.Generation,
                    frame = spawned.Frame?.N ?? -1,
                    runtimeFrame = spawned.Runtime?.Frame ?? -1,
                    framePrev2 = spawned.Frame?.Prev2 ?? -1,
                    runtimePrev2 = spawned.Runtime?.PrevFrame2 ?? -1,
                    spawnSemantic = spawned.Runtime?.SpawnSemantic ?? -1,
                    rendererPresent = spawned.Renderer != null,
                    objectCountDelta = world.ObjectCount - objectCountBefore,
                    objectPoolActiveDelta =
                        objectPool.ActiveObjectCountForAcceptance - poolActiveBefore,
                    logicPoolActiveDelta =
                        LF2ReferencePool.Instance.ActiveCount - logicActiveBefore,
                    slotReused = slotReused,
                    generationAdvanced = generationAdvanced,
                    birthFrameCorrect = birthFrameCorrect,
                    prev2Correct = prev2Correct,
                };

                if (actualType != definition.type || !expectedClrType ||
                    !birthFrameCorrect || !prev2Correct || !slotReused ||
                    !generationAdvanced || row.objectCountDelta != 1)
                {
                    throw new InvalidOperationException(
                        $"Birth contract mismatch for oid {oid}: " +
                        $"type={actualType}/{definition.type}, " +
                        $"clr={spawned.GetType().Name}/{row.expectedClrType}, " +
                        $"frame={row.frame}/{row.runtimeFrame}, " +
                        $"prev2={row.framePrev2}/{row.runtimePrev2}, " +
                        $"handle={handle}, previous={previousBirthHandle}, " +
                        $"objectDelta={row.objectCountDelta}.");
                }

                TrackOwnedSpawn(spawned);
                RuntimeEntityHandle releasedHandle = handle;
                ReleaseOwnedSpawn(spawned);
                row.oldHandleRejectedAfterRelease =
                    !world.TryResolveRuntimeHandleForDiagnostics(releasedHandle, out _);
                row.objectCountRestoredAfterRelease =
                    world.ObjectCount == objectCountBefore;
                row.objectPoolActiveRestoredAfterRelease =
                    objectPool.ActiveObjectCountForAcceptance == poolActiveBefore;
                row.logicPoolActiveRestoredAfterRelease =
                    LF2ReferencePool.Instance.ActiveCount == logicActiveBefore;
                if (!row.oldHandleRejectedAfterRelease ||
                    !row.objectCountRestoredAfterRelease ||
                    !row.objectPoolActiveRestoredAfterRelease ||
                    !row.logicPoolActiveRestoredAfterRelease)
                {
                    throw new InvalidOperationException(
                        $"Release contract mismatch for oid {oid}: " +
                        $"oldRejected={row.oldHandleRejectedAfterRelease}, " +
                        $"objectRestored={row.objectCountRestoredAfterRelease}, " +
                        $"renderPoolRestored={row.objectPoolActiveRestoredAfterRelease}, " +
                        $"logicPoolRestored={row.logicPoolActiveRestoredAfterRelease}.");
                }

                BirthEvidenceRows.Add(row);
                previousBirthHandle = releasedHandle;
            }
        }

        private static void StartHighSlotWitness()
        {
            highProducer.SetSpawnOid(OtherOid);
            CaptureSnapshot(SnapshotBefore);
            ScheduleTick(ProbePhase.WaitingForHighTick);
        }

        private static void CompleteHighSlotWitness()
        {
            highProducer.AttackingCounter = 1;
            highSpawn = FindSingleNewSpawn(OtherOid, SnapshotBefore);
            TrackOwnedSpawn(highSpawn);
            highHandle = RequireHandle(highSpawn, "high-slot newborn");
            highAttackingAfterCreationTick = highSpawn.AttackingCounter;
            bool highSlot = highHandle.Slot > highProducer.Runtime.SlotIndex;
            bool samePassVisited = highAttackingAfterCreationTick > 0;
            result.highSlot = new CursorEvidence
            {
                producerSlot = highProducer.Runtime.SlotIndex,
                spawnSlot = highHandle.Slot,
                generation = highHandle.Generation,
                creationTick = expectedTick,
                attackingAfterCreationTick = highAttackingAfterCreationTick,
                attackingAfterConsumerTick = highAttackingAfterCreationTick,
                expectedSamePassVisit = true,
                observedExpectedVisit = highSlot && samePassVisited,
            };
            if (!result.highSlot.observedExpectedVisit)
            {
                throw new InvalidOperationException(
                    $"High-slot newborn was not consumed later in its creation pass: " +
                    $"producer={highProducer.Runtime.SlotIndex}, spawn={highHandle}, " +
                    $"attacking={highAttackingAfterCreationTick}.");
            }

            ReleaseOwnedSpawn(highSpawn);
            highSpawn = null;
            UnregisterProbeEntity(ref highProducer);
        }

        private static void StartLowSlotWitness()
        {
            lowFiller = new ProbeEntity("R8OpointLowFiller", hasOpoint: false);
            lowProducer = new ProbeEntity("R8OpointLowProducer", hasOpoint: true);
            world.Register(lowFiller);
            world.Register(lowProducer);
            RequireRegistered(lowFiller, "low filler");
            RequireRegistered(lowProducer, "low producer");
            int releasedLowSlot = lowFiller.Runtime.SlotIndex;
            if (releasedLowSlot >= lowProducer.Runtime.SlotIndex)
            {
                throw new InvalidOperationException(
                    $"Low-slot precondition failed: filler={releasedLowSlot}, " +
                    $"producer={lowProducer.Runtime.SlotIndex}.");
            }

            world.Unregister(lowFiller);
            lowFiller = null;
            lowProducer.SetSpawnOid(OtherOid);
            CaptureSnapshot(SnapshotBefore);
            ScheduleTick(ProbePhase.WaitingForLowCreationTick);
        }

        private static void CompleteLowCreationTick()
        {
            lowProducer.AttackingCounter = 1;
            lowSpawn = FindSingleNewSpawn(OtherOid, SnapshotBefore);
            TrackOwnedSpawn(lowSpawn);
            lowHandle = RequireHandle(lowSpawn, "low-slot newborn");
            lowAttackingAfterCreationTick = lowSpawn.AttackingCounter;
            bool lowSlot = lowHandle.Slot < lowProducer.Runtime.SlotIndex;
            bool deferred = lowAttackingAfterCreationTick == 0;
            if (!lowSlot || !deferred)
            {
                throw new InvalidOperationException(
                    $"Low-slot newborn was not deferred at the creation-pass cursor: " +
                    $"producer={lowProducer.Runtime.SlotIndex}, spawn={lowHandle}, " +
                    $"attacking={lowAttackingAfterCreationTick}.");
            }
        }

        private static void CompleteLowConsumerTick()
        {
            if (!world.TryResolveRuntimeHandleForDiagnostics(lowHandle, out LF2Entity resolved) ||
                !ReferenceEquals(resolved, lowSpawn))
            {
                throw new InvalidOperationException(
                    $"Low-slot newborn handle {lowHandle} did not survive to its consumer tick.");
            }

            int attackingAfterConsumerTick = lowSpawn.AttackingCounter;
            result.lowSlot = new CursorEvidence
            {
                producerSlot = lowProducer.Runtime.SlotIndex,
                spawnSlot = lowHandle.Slot,
                generation = lowHandle.Generation,
                creationTick = expectedTick - 1,
                consumerTick = expectedTick,
                attackingAfterCreationTick = lowAttackingAfterCreationTick,
                attackingAfterConsumerTick = attackingAfterConsumerTick,
                expectedSamePassVisit = false,
                observedExpectedVisit = attackingAfterConsumerTick > 0,
            };
            if (!result.lowSlot.observedExpectedVisit)
            {
                throw new InvalidOperationException(
                    $"Low-slot newborn did not execute on the next tick: " +
                    $"handle={lowHandle}, creationAttacking={lowAttackingAfterCreationTick}, " +
                    $"consumerAttacking={attackingAfterConsumerTick}.");
            }

            ReleaseOwnedSpawn(lowSpawn);
            lowSpawn = null;
            UnregisterProbeEntity(ref lowProducer);
        }

        private static void ScheduleTick(ProbePhase waitingPhase)
        {
            phaseEditorUpdates = 0;
            expectedTick = driver.CurrentTickIndex + 1;
            if (workerWasActive)
            {
                if (!driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                        buildPresentation: true))
                {
                    throw new InvalidOperationException(
                        "The paused production worker rejected the diagnostic tick: " +
                        driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
                }
                phase = waitingPhase;
                return;
            }

            if (!driver.StepOneTick(ignorePaused: true, buildPresentation: true))
            {
                throw new InvalidOperationException(
                    $"The production driver rejected manual tick {expectedTick}.");
            }
            phase = waitingPhase;
        }

        private static bool TickCompleted()
        {
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return false;
            return driver.CurrentTickIndex >= expectedTick;
        }

        private static void FinishSuccess()
        {
            result.status = "PASS";
            result.message =
                "Live production opoint birth, high/low scan cursor, release, and generation reuse passed.";
            result.endTick = driver.CurrentTickIndex;
            result.births = BirthEvidenceRows.ToArray();
            CleanupProbeObjects();
            CaptureFinalCounts();
            WriteResult(result);
            Debug.Log(
                $"[BattleOpointLifecyclePlayModeProbe] PASS: " +
                $"births={result.births.Length}, ticks={result.startTick}->{result.endTick}.");
            StopObservation();
        }

        private static void Fail(string message)
        {
            if (!running)
            {
                WriteImmediateFailure(message);
                return;
            }

            result ??= new ProbeResult();
            result.status = "FAIL";
            result.message = message;
            result.endTick = driver?.CurrentTickIndex ?? -1;
            result.births = BirthEvidenceRows.ToArray();
            CleanupProbeObjects();
            CaptureFinalCounts();
            WriteResult(result);
            Debug.LogError($"[BattleOpointLifecyclePlayModeProbe] FAIL: {message}");
            StopObservation();
        }

        private static string ValidateTargetCatalog()
        {
            for (int index = 0; index < TargetOids.Length; index++)
            {
                int oid = TargetOids[index];
                ObjectDefinition definition = GameDataManager.Instance.GetObjectById(oid);
                LF2CharacterDataWrapper config =
                    CharacterAnimtorManager.Instance.GetCharacterConfig(oid);
                if (definition == null || config?.characterData == null)
                    return $"Production catalog is missing oid {oid} definition or frame config.";
            }
            return string.Empty;
        }

        private static void CaptureSnapshot(List<LF2Entity> destination)
        {
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(destination);
        }

        private static LF2Entity FindSingleNewSpawn(
            int oid,
            List<LF2Entity> before)
        {
            CaptureSnapshot(SnapshotAfter);
            LF2Entity found = null;
            for (int index = 0; index < SnapshotAfter.Count; index++)
            {
                LF2Entity candidate = SnapshotAfter[index];
                if (candidate == null || candidate.ObjectId != oid ||
                    before.Contains(candidate) || candidate == highProducer ||
                    candidate == lowProducer || candidate == lowFiller)
                {
                    continue;
                }

                if (found != null)
                {
                    throw new InvalidOperationException(
                        $"Multiple new oid {oid} entities appeared at one probe boundary: " +
                        $"{found.Runtime?.SlotIndex}, {candidate.Runtime?.SlotIndex}.");
                }
                found = candidate;
            }

            if (found == null)
                throw new InvalidOperationException($"No new production oid {oid} entity was found.");
            return found;
        }

        private static RuntimeEntityHandle RequireHandle(LF2Entity entity, string label)
        {
            if (entity?.Runtime == null ||
                !world.TryGetCurrentRuntimeHandleForDiagnostics(
                    entity.Runtime.SlotIndex,
                    entity,
                    out RuntimeEntityHandle handle) ||
                !handle.IsValid)
            {
                throw new InvalidOperationException($"{label} has no valid runtime handle.");
            }
            return handle;
        }

        private static void RequireRegistered(ProbeEntity entity, string label)
        {
            if (entity?.Runtime == null || entity.Runtime.SlotIndex < 0 ||
                !world.TryGetCurrentRuntimeHandleForDiagnostics(
                    entity.Runtime.SlotIndex,
                    entity,
                    out RuntimeEntityHandle handle) ||
                !handle.IsValid)
            {
                throw new InvalidOperationException($"The {label} failed to register.");
            }
        }

        private static bool IsExpectedClrType(LF2Entity entity, int objectType)
        {
            return objectType switch
            {
                (int)LF2ObjectType.Character => entity is LF2Character,
                (int)LF2ObjectType.LightWeapon => entity is LF2WeaponBase,
                (int)LF2ObjectType.SpecialAttack => entity is LF2SpecialAttack,
                (int)LF2ObjectType.Other => entity is LF2OtherObject,
                _ => false,
            };
        }

        private static string ExpectedClrTypeName(int objectType)
        {
            return objectType switch
            {
                (int)LF2ObjectType.Character => nameof(LF2Character),
                (int)LF2ObjectType.LightWeapon => nameof(LF2WeaponBase),
                (int)LF2ObjectType.SpecialAttack => nameof(LF2SpecialAttack),
                (int)LF2ObjectType.Other => nameof(LF2OtherObject),
                _ => "Unsupported",
            };
        }

        private static void TrackOwnedSpawn(LF2Entity entity)
        {
            if (entity != null && !OwnedSpawns.Contains(entity))
                OwnedSpawns.Add(entity);
        }

        private static void ReleaseOwnedSpawn(LF2Entity entity)
        {
            if (entity == null)
                return;
            OwnedSpawns.Remove(entity);
            if (entity.Match == world && entity.Runtime?.SlotIndex >= 0)
                entity.FreeEntityLikeExe();
            world?.FlushPendingDestroyForDiagnostics();
        }

        private static void UnregisterProbeEntity(ref ProbeEntity entity)
        {
            ProbeEntity current = entity;
            entity = null;
            if (current?.Match == world && current.Runtime?.SlotIndex >= 0)
                world.Unregister(current);
            world?.FlushPendingDestroyForDiagnostics();
        }

        private static void CleanupProbeObjects()
        {
            if (world == null)
                return;

            for (int index = OwnedSpawns.Count - 1; index >= 0; index--)
            {
                LF2Entity entity = OwnedSpawns[index];
                try
                {
                    if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                        entity.FreeEntityLikeExe();
                }
                catch (Exception exception)
                {
                    result.cleanupErrors += $"spawn:{exception.Message};";
                }
            }
            OwnedSpawns.Clear();

            TryUnregisterProbeEntity(ref highProducer, "highProducer");
            TryUnregisterProbeEntity(ref lowProducer, "lowProducer");
            TryUnregisterProbeEntity(ref lowFiller, "lowFiller");
            try
            {
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                result.cleanupErrors += $"flush:{exception.Message};";
            }
        }

        private static void TryUnregisterProbeEntity(
            ref ProbeEntity entity,
            string label)
        {
            ProbeEntity current = entity;
            entity = null;
            if (current?.Match != world || current.Runtime?.SlotIndex < 0)
                return;
            try
            {
                world.Unregister(current);
            }
            catch (Exception exception)
            {
                result.cleanupErrors += $"{label}:{exception.Message};";
            }
        }

        private static void CaptureFinalCounts()
        {
            if (result == null)
                return;
            result.finalObjectCount = world?.ObjectCount ?? -1;
            result.finalClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            result.finalObjectPoolActive = objectPool?.ActiveObjectCountForAcceptance ?? -1;
            result.finalLogicPoolActive = LF2ReferencePool.Instance?.ActiveCount ?? -1;
            result.cleanupCompleted = string.IsNullOrEmpty(result.cleanupErrors);
        }

        private static void WriteImmediateFailure(string message)
        {
            var failure = new ProbeResult
            {
                status = "FAIL",
                message = message,
                startTick = driver?.CurrentTickIndex ?? -1,
                endTick = driver?.CurrentTickIndex ?? -1,
                births = Array.Empty<BirthEvidence>(),
            };
            WriteResult(failure);
            Debug.LogError($"[BattleOpointLifecyclePlayModeProbe] FAIL: {message}");
        }

        private static void WriteResult(ProbeResult probeResult)
        {
            string path = ResultPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(probeResult, true));
        }

        private static string ResultPath()
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                ResultRelativePath));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            if (driver != null && EditorApplication.isPlaying)
                driver.SetPaused(previousPaused);
            running = false;
        }

        private static void ResetProbeState()
        {
            driver = null;
            world = null;
            factory = null;
            objectPool = null;
            highProducer = null;
            lowProducer = null;
            lowFiller = null;
            highSpawn = null;
            lowSpawn = null;
            previousBirthHandle = RuntimeEntityHandle.Invalid;
            highHandle = RuntimeEntityHandle.Invalid;
            lowHandle = RuntimeEntityHandle.Invalid;
            result = null;
            phase = ProbePhase.None;
            expectedTick = -1;
            phaseEditorUpdates = 0;
            highAttackingAfterCreationTick = 0;
            lowAttackingAfterCreationTick = 0;
            previousPaused = false;
            workerWasActive = false;
            SnapshotBefore.Clear();
            SnapshotAfter.Clear();
            OwnedSpawns.Clear();
            BirthEvidenceRows.Clear();
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public bool workerWasActive;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int baselineObjectPoolActive;
            public int baselineLogicPoolActive;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public BirthEvidence[] births;
            public CursorEvidence highSlot;
            public CursorEvidence lowSlot;
        }

        [Serializable]
        private sealed class BirthEvidence
        {
            public int oid;
            public int expectedObjectType;
            public int actualObjectType;
            public string expectedClrType;
            public string actualClrType;
            public bool clrTypeMatched;
            public int producerSlot;
            public int slot;
            public long generation;
            public int frame;
            public int runtimeFrame;
            public int framePrev2;
            public int runtimePrev2;
            public int spawnSemantic;
            public bool rendererPresent;
            public int objectCountDelta;
            public int objectPoolActiveDelta;
            public int logicPoolActiveDelta;
            public bool slotReused;
            public bool generationAdvanced;
            public bool birthFrameCorrect;
            public bool prev2Correct;
            public bool oldHandleRejectedAfterRelease;
            public bool objectCountRestoredAfterRelease;
            public bool objectPoolActiveRestoredAfterRelease;
            public bool logicPoolActiveRestoredAfterRelease;
        }

        [Serializable]
        private sealed class CursorEvidence
        {
            public int producerSlot;
            public int spawnSlot;
            public long generation;
            public int creationTick;
            public int consumerTick;
            public int attackingAfterCreationTick;
            public int attackingAfterConsumerTick;
            public bool expectedSamePassVisit;
            public bool observedExpectedVisit;
        }

        private sealed class ProbeEntity : LF2OtherObject
        {
            private readonly LF2FrameData probeFrame;

            public ProbeEntity(string probeName, bool hasOpoint)
            {
                Name = probeName;
                ObjectId = 739;
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                probeFrame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 100,
                    next = 0,
                    pic = 0,
                    centerx = 0,
                    centery = 0,
                };
                if (hasOpoint)
                    SetProbeOpoint(OtherOid);
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)LF2ObjectType.Other,
                        frames = new List<LF2FrameData> { probeFrame },
                    }));
                Frame.D = probeFrame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Frame.Prev2 = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Runtime.SetPosition(500, 0, 300);
                Runtime.SyncIntegerPosition();
                PS.dir = "right";
            }

            public void SetSpawnOid(int oid)
            {
                SetProbeOpoint(oid);
                Frame.D = probeFrame;
                AttackingCounter = 0;
            }

            public override void SimFrameTick(int tickIndex)
            {
            }

            private void SetProbeOpoint(int oid)
            {
                probeFrame.opoint = new ObjectPoint
                {
                    kind = 1,
                    oid = oid,
                    action = 0,
                    facing = 0,
                };
            }
        }

        private enum ProbePhase
        {
            None,
            WaitingForQuiescence,
            WaitingForHighTick,
            WaitingForLowCreationTick,
            WaitingForLowConsumerTick,
        }
    }
}
#endif
