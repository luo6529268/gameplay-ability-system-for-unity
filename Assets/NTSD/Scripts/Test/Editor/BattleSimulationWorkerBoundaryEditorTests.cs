using System;
using System.Reflection;
using System.Threading;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleSimulationWorkerBoundaryEditorTests
    {
        [Test]
        public void InputQueueCopiesCanonicalFramesAndPreservesOrder()
        {
            var queue = new Simulation.BattleSimulationInputQueue(2, 2);
            var first = new Simulation.FrameInputSet(
                10,
                new[]
                {
                    new Simulation.SimulationPlayerInput(
                        2,
                        Simulation.SimulationInputButtons.Left,
                        Simulation.SimulationInputButtons.Left),
                    new Simulation.SimulationPlayerInput(
                        5,
                        Simulation.SimulationInputButtons.Attack),
                });
            var second = Simulation.FrameInputSet.Empty(11);

            Assert.That(queue.TryEnqueue(first), Is.True);
            Assert.That(queue.TryEnqueue(second), Is.True);
            Assert.That(queue.TryEnqueue(Simulation.FrameInputSet.Empty(12)), Is.False);

            var destination = Simulation.FrameInputSet.Empty(0);
            var players = new Simulation.SimulationPlayerInput[2];
            Assert.That(queue.TryDequeue(destination, players), Is.True);
            Assert.That(destination.TickIndex, Is.EqualTo(10));
            Assert.That(destination.GetCanonicalHash64(), Is.EqualTo(first.GetCanonicalHash64()));
            Assert.That(queue.TryDequeue(destination, players), Is.True);
            Assert.That(destination.TickIndex, Is.EqualTo(11));
            Assert.That(queue.TryDequeue(destination, players), Is.False);
            queue.ResetWhenStopped();
        }

        [Test]
        public void WarmInputQueueRoundTripDoesNotAllocate()
        {
            var queue = new Simulation.BattleSimulationInputQueue(2, 1);
            var sourcePlayers = new Simulation.SimulationPlayerInput[1];
            sourcePlayers[0] = new Simulation.SimulationPlayerInput(
                1,
                Simulation.SimulationInputButtons.Right);
            var source = new Simulation.FrameInputSet(1, sourcePlayers);
            var destination = Simulation.FrameInputSet.Empty(0);
            var destinationPlayers = new Simulation.SimulationPlayerInput[1];

            Assert.That(queue.TryEnqueue(source), Is.True);
            Assert.That(queue.TryDequeue(destination, destinationPlayers), Is.True);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 2; tick <= 1025; tick++)
            {
                source.ResetPreallocated(tick, sourcePlayers, 1);
                if (!queue.TryEnqueue(source) ||
                    !queue.TryDequeue(destination, destinationPlayers))
                {
                    Assert.Fail("The warmed SPSC queue unexpectedly rejected a round trip.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.EqualTo(0));
        }

        [Test]
        public void InputQueueCopiesCompleteTickRequestWithoutSharingHostFrameStorage()
        {
            var queue = new Simulation.BattleSimulationInputQueue(2, 1);
            var sourcePlayers = new Simulation.SimulationPlayerInput[1];
            sourcePlayers[0] = new Simulation.SimulationPlayerInput(
                3,
                Simulation.SimulationInputButtons.Left |
                Simulation.SimulationInputButtons.Attack);
            var source = new Simulation.FrameInputSet(19, sourcePlayers);
            var stage = new Simulation.BattleSimulationStageSnapshot(
                1337,
                170,
                390,
                11,
                29);
            var destination = Simulation.FrameInputSet.Empty(0);
            var destinationPlayers = new Simulation.SimulationPlayerInput[1];

            Assert.That(
                queue.TryEnqueue(source, buildPresentation: false, in stage),
                Is.True);
            sourcePlayers[0] = new Simulation.SimulationPlayerInput(
                7,
                Simulation.SimulationInputButtons.None);
            Assert.That(
                queue.TryDequeue(destination, destinationPlayers, out var request),
                Is.True);

            Assert.That(request.FrameInput, Is.SameAs(destination));
            Assert.That(request.FrameInput.TickIndex, Is.EqualTo(19));
            Assert.That(request.FrameInput.Players[0].PlayerSlot, Is.EqualTo(3));
            Assert.That(
                request.FrameInput.Players[0].Buttons,
                Is.EqualTo(
                    Simulation.SimulationInputButtons.Left |
                    Simulation.SimulationInputButtons.Attack));
            Assert.That(request.BuildPresentation, Is.False);
            Assert.That(request.Stage.IsValid, Is.True);
            Assert.That(request.Stage.StageWidth, Is.EqualTo(1337));
            Assert.That(request.Stage.ZMin, Is.EqualTo(170));
            Assert.That(request.Stage.ZMax, Is.EqualTo(390));
            Assert.That(request.Stage.PerspectiveNear, Is.EqualTo(11));
            Assert.That(request.Stage.PerspectiveFar, Is.EqualTo(29));
        }

        [Test]
        public void PublicationBufferNeverReturnsTornState()
        {
            var buffer = new Simulation.BattleSimulationPublicationBuffer();
            Exception workerFailure = null;
            var worker = new Thread(() =>
            {
                try
                {
                    for (int tick = 1; tick <= 20000; tick++)
                    {
                        ulong inputHash = (ulong)tick * 17UL;
                        var publication = new Simulation.BattleSimulationTickPublication(
                            tick,
                            inputHash,
                            inputHash ^ 0xA55AA55AA55AA55AUL,
                            true,
                            false,
                            tick * 13L);
                        buffer.Publish(in publication);
                    }
                }
                catch (Exception exception)
                {
                    workerFailure = exception;
                }
            });
            worker.Start();

            long consumedSequence = 0;
            int lastTick = 0;
            while (worker.IsAlive || consumedSequence != buffer.PublishedSequence)
            {
                if (!buffer.TryReadLatest(ref consumedSequence, out var publication))
                {
                    Thread.SpinWait(8);
                    continue;
                }

                Assert.That(publication.TickIndex, Is.GreaterThan(lastTick));
                Assert.That(
                    publication.StateChecksum,
                    Is.EqualTo(publication.InputHash ^ 0xA55AA55AA55AA55AUL));
                Assert.That(publication.HasStateChecksum, Is.True);
                Assert.That(
                    publication.ExecutionElapsedTimestampTicks,
                    Is.EqualTo(publication.TickIndex * 13L));
                lastTick = publication.TickIndex;
            }
            worker.Join();

            Assert.That(workerFailure, Is.Null);
            Assert.That(lastTick, Is.EqualTo(20000));
        }

        [Test]
        public void ThreadOwnershipSeparatesUnityHostAndSimulationOwner()
        {
            var ownership = new Simulation.BattleSimulationThreadOwnership();
            Assert.That(ownership.IsMainThread, Is.True);
            Assert.That(ownership.SimulationThreadId, Is.EqualTo(0));

            bool bound = false;
            bool workerObservedOwnership = false;
            bool workerRejectedMainThreadRequirement = false;
            Exception workerFailure = null;
            var worker = new Thread(() =>
            {
                try
                {
                    bound = ownership.TryBindSimulationThread();
                    workerObservedOwnership = ownership.IsSimulationThread;
                    ownership.RequireSimulationThread();
                    try
                    {
                        ownership.RequireMainThread();
                    }
                    catch (InvalidOperationException)
                    {
                        workerRejectedMainThreadRequirement = true;
                    }
                }
                catch (Exception exception)
                {
                    workerFailure = exception;
                }
            });
            worker.Start();
            worker.Join();

            Assert.That(bound, Is.True);
            Assert.That(workerObservedOwnership, Is.True);
            Assert.That(workerRejectedMainThreadRequirement, Is.True);
            Assert.That(workerFailure, Is.Null);
            Assert.That(ownership.IsMainThread, Is.True);
            ownership.RequireMainThread();
            Assert.Throws<InvalidOperationException>(ownership.RequireSimulationThread);
        }

        [Test]
        public void CentralOnlyWorkerCapturePublishesLogicalFrameWithoutUnityResourceBinding()
        {
            var world = new Simulation.SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            Exception workerFailure = null;
            var worker = new Thread(() =>
            {
                try
                {
                    world.CaptureSimulationWorkerPresentationFrame(77);
                }
                catch (Exception exception)
                {
                    workerFailure = exception;
                }
            });

            worker.Start();
            worker.Join();

            Assert.That(workerFailure, Is.Null);
            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            Assert.That(frame, Is.Not.Null);
            Assert.That(frame.TickIndex, Is.EqualTo(77));
            Assert.That(frame.CommandsMaterialized, Is.False);
            Assert.That(frame.PresentationOrderMaterialized, Is.False);
            Assert.That(frame.BoundCatalog, Is.SameAs(BattleSpriteCatalog.Empty));
            Assert.That(frame.CommonVisualCatalog, Is.SameAs(BattleCommonVisualCatalog.Empty));
        }

        [Test]
        public void CompleteLogicOnlyCharacterTickRunsOffMainThreadAndPublishesFrame()
        {
            const int objectId = 31995;
            const int runtimeSlot = 50;
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 100,
                next = 0,
            };
            var characterData = new LF2CharacterData();
            characterData.frames.Add(frame);
            var wrapper = new LF2CharacterDataWrapper(objectId, characterData);
            var world = new Simulation.SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        objectId,
                        (int)LF2ObjectType.Character,
                        "logic-worker-character.dat"),
                },
                id => id == objectId ? wrapper : null);
            world.SetLogicOnlyEntityMaterialization(true);

            var character = new LF2Character();
            character.ModuleInitialize();
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.ModuleBind(wrapper, objectId, world);
            character.Initialize(500, 500);
            character.Runtime.AiControlled = false;
            character.Runtime.Team = 1;
            character.Runtime.RelationTeam = 1;
            character.Runtime.SetPosition(100, 0, 220);
            character.Runtime.SyncIntegerPosition();
            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(runtimeSlot));
            Assert.That(character.Renderer, Is.Null);
            Assert.That(character.ShadowRenderer, Is.Null);

            var tickSystem = new Simulation.NTSDBattleTickSystem(world);
            Exception workerFailure = null;
            var worker = new Thread(() =>
            {
                try
                {
                    tickSystem.RunSimulationWorkerTick(2, buildPresentation: true);
                }
                catch (Exception exception)
                {
                    workerFailure = exception;
                }
            });

            worker.Start();
            worker.Join();

            Assert.That(workerFailure, Is.Null);
            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(runtimeSlot));
            Assert.That(
                world.FindEntityByRuntimeSlotForQuery(runtimeSlot),
                Is.SameAs(character));
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            Assert.That(published, Is.Not.Null);
            Assert.That(published.TickIndex, Is.EqualTo(2));
            Assert.That(published.BoundCatalog, Is.SameAs(BattleSpriteCatalog.Empty));
            Assert.That(
                published.CommonVisualCatalog,
                Is.SameAs(BattleCommonVisualCatalog.Empty));

            character.UnregisterFromWorld();
            character.Reset();
        }

        [Test]
        public void DedicatedWorkerDirectionChangeMutatesLogicWithoutTouchingSpriteRenderer()
        {
            var host = new GameObject("WorkerDirectionPresentationBoundary")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            try
            {
                SpriteRenderer renderer = host.AddComponent<SpriteRenderer>();
                renderer.flipX = false;
                var character = new LF2Character();
                character.ModuleInitialize();
                character.Sprite.Initialize(renderer, null);
                character.SwitchDir("right");

                var executor = new DirectionSwitchTickExecutor(character);
                using var worker = new Simulation.DedicatedBattleSimulationWorker(
                    2,
                    1,
                    executor);
                worker.Start();
                Assert.That(worker.TrySubmit(Simulation.FrameInputSet.Empty(1)), Is.True);

                long consumedSequence = 0;
                Assert.That(
                    SpinWait.SpinUntil(
                        () => worker.PublishedSequence > consumedSequence ||
                              worker.Failure != null,
                        3000),
                    Is.True,
                    "the worker did not publish the direction-change tick");
                Assert.That(worker.Failure, Is.Null);
                Assert.That(
                    worker.TryReadLatest(ref consumedSequence, out var publication),
                    Is.True);
                Assert.That(publication.TickIndex, Is.EqualTo(1));
                Assert.That(character.Runtime.Dir, Is.EqualTo("left"));
                Assert.That(character.PS.dir, Is.EqualTo("left"));
                Assert.That(character.Sprite.Dir, Is.EqualTo("left"));
                Assert.That(
                    renderer.flipX,
                    Is.False,
                    "the simulation worker must not read or mutate UnityEngine.Object state");

                worker.AcknowledgePresentationConsumed(consumedSequence);
                worker.Stop();
                Assert.That(worker.Failure, Is.Null);

                character.SwitchDir("left");
                Assert.That(
                    renderer.flipX,
                    Is.True,
                    "the existing main-thread legacy presentation path must remain intact");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void DedicatedWorkerFullTickMaterializesLateOpointWithoutUnityFactory()
        {
            const int parentOid = 31993;
            const int childOid = 31994;
            const int parentSlot = 50;
            var parentFrame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 0,
                next = 0,
                centerx = 10,
                centery = 20,
                opoint = new ObjectPoint
                {
                    kind = 1,
                    oid = childOid,
                    action = 0,
                    x = 15,
                    y = 25,
                    facing = 0,
                },
            };
            var childFrame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 100,
                next = 0,
            };
            var parentData = new LF2CharacterData();
            parentData.frames.Add(parentFrame);
            var childData = new LF2CharacterData();
            childData.frames.Add(childFrame);
            var parentWrapper = new LF2CharacterDataWrapper(parentOid, parentData);
            var childWrapper = new LF2CharacterDataWrapper(childOid, childData);
            var world = new Simulation.SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        parentOid,
                        (int)LF2ObjectType.Character,
                        "logic-worker-opoint-parent.dat"),
                    new ObjectDefinition(
                        childOid,
                        (int)LF2ObjectType.Other,
                        "logic-worker-opoint-child.dat"),
                },
                oid => oid == parentOid
                    ? parentWrapper
                    : oid == childOid ? childWrapper : null);
            world.SetLogicOnlyEntityMaterialization(true);

            var parent = new LF2Character();
            parent.ModuleInitialize();
            parent.SetRequiredRuntimeSlot(parentSlot);
            parent.ModuleBind(parentWrapper, parentOid, world);
            parent.Initialize(500, 500);
            parent.Runtime.AiControlled = false;
            parent.Runtime.Team = 1;
            parent.Runtime.RelationTeam = 1;
            parent.Runtime.Dir = "right";
            parent.PS.dir = "right";
            parent.Runtime.SetPosition(100, 40, 220);
            parent.Runtime.SyncIntegerPosition();

            var tickSystem = new Simulation.NTSDBattleTickSystem(world);
            var executor = new Simulation.BattleWorldSimulationTickExecutor(
                world,
                tickSystem,
                captureChecksum: true);
            using var worker = new Simulation.DedicatedBattleSimulationWorker(
                2,
                1,
                executor);
            worker.Start();
            var stage = new Simulation.BattleSimulationStageSnapshot(
                1600,
                160,
                400,
                0,
                0);
            Assert.That(
                worker.TrySubmit(
                    Simulation.FrameInputSet.Empty(2),
                    buildPresentation: true,
                    in stage),
                Is.True);

            long consumedSequence = 0;
            Assert.That(
                SpinWait.SpinUntil(
                    () => worker.PublishedSequence > consumedSequence ||
                          worker.Failure != null,
                    3000),
                Is.True,
                "the worker did not publish the opoint tick");
            Assert.That(worker.Failure, Is.Null);
            Assert.That(
                worker.TryReadLatest(ref consumedSequence, out var publication),
                Is.True);
            Assert.That(publication.TickIndex, Is.EqualTo(2));
            Assert.That(publication.HasStateChecksum, Is.True);

            LF2Entity child = null;
            for (int slot = world.DynamicRuntimeSlotStartForServices;
                 slot < world.MaxRuntimeSlotsForServices;
                 slot++)
            {
                LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                if (candidate?.ObjectId == childOid)
                {
                    child = candidate;
                    break;
                }
            }
            Assert.That(
                child,
                Is.Not.Null,
                "the full worker tick must retain the late-opoint child in the runtime slot table");
            Assert.That(child.ObjectId, Is.EqualTo(childOid));
            Assert.That(child.Renderer, Is.Null);
            Assert.That(child.ShadowRenderer, Is.Null);
            Assert.That(child.Runtime.XInt, Is.EqualTo(105));
            Assert.That(child.Runtime.YInt, Is.EqualTo(45));
            Assert.That(child.Runtime.ZInt, Is.EqualTo(221));
            Assert.That(
                world.StructuralWriterDiagnosticsForDiagnostics.LastSpawnBoundary,
                Is.EqualTo(
                    Simulation.Ecs.BattleStructuralPlaybackBoundary
                        .CurrentEntityImmediate));
            Assert.That(
                world.StructuralWriterDiagnosticsForDiagnostics.LastSpawnSource.Slot,
                Is.EqualTo(parentSlot));

            worker.AcknowledgePresentationConsumed(consumedSequence);
            worker.Stop();
            Assert.That(worker.Failure, Is.Null);
            Assert.That(
                world.LogicObjectPointRuntime.PendingTaskCountForDiagnostics,
                Is.Zero);

            child.UnregisterFromWorld();
            child.Reset();
            world.LogicReferencePool.Release(child);
            parent.UnregisterFromWorld();
            parent.Reset();
        }

        [Test]
        public void DedicatedWorkerFullTickConsumesCanonicalHumanInput()
        {
            const int objectId = 31991;
            const int runtimeSlot = 0;
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 100,
                next = 0,
            };
            var characterData = new LF2CharacterData();
            characterData.frames.Add(frame);
            var wrapper = new LF2CharacterDataWrapper(objectId, characterData);
            var world = new Simulation.SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        objectId,
                        (int)LF2ObjectType.Character,
                        "logic-worker-human-input.dat"),
                },
                id => id == objectId ? wrapper : null);
            world.SetLogicOnlyEntityMaterialization(true);

            var character = new LF2Character();
            character.ModuleInitialize();
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.ModuleBind(wrapper, objectId, world);
            character.Initialize(500, 500);
            character.Runtime.AiControlled = false;
            character.Runtime.Team = 1;
            character.Runtime.RelationTeam = 1;
            character.Runtime.SetPosition(100, 0, 220);
            character.Runtime.SyncIntegerPosition();

            Simulation.BattleSlotRuntimeState rosterSlot =
                world.Runtime.Roster.Slots[0];
            rosterSlot.Active = true;
            rosterSlot.IsHuman = true;
            rosterSlot.CharacterId = objectId;
            rosterSlot.Team = 1;
            rosterSlot.RuntimeSlotIndex = runtimeSlot;
            rosterSlot.StableId = character.Runtime.StableId;
            world.Runtime.Roster.ActiveSlotCount = 1;

            var input = new Simulation.FrameInputSet(
                2,
                new[]
                {
                    new Simulation.SimulationPlayerInput(
                        0,
                        Simulation.SimulationInputButtons.Left |
                        Simulation.SimulationInputButtons.Attack,
                        Simulation.SimulationInputButtons.Left |
                        Simulation.SimulationInputButtons.Attack),
                });
            var tickSystem = new Simulation.NTSDBattleTickSystem(world);
            var executor = new Simulation.BattleWorldSimulationTickExecutor(
                world,
                tickSystem,
                captureChecksum: true);
            using var worker = new Simulation.DedicatedBattleSimulationWorker(
                2,
                1,
                executor);
            worker.Start();
            var stage = new Simulation.BattleSimulationStageSnapshot(
                1600,
                160,
                400,
                0,
                0);
            Assert.That(
                worker.TrySubmit(input, buildPresentation: true, in stage),
                Is.True);

            long consumedSequence = 0;
            Assert.That(
                SpinWait.SpinUntil(
                    () => worker.PublishedSequence > consumedSequence ||
                          worker.Failure != null,
                    3000),
                Is.True,
                "the worker did not publish the human-input tick");
            Assert.That(worker.Failure, Is.Null);
            Assert.That(
                worker.TryReadLatest(ref consumedSequence, out var publication),
                Is.True);
            Assert.That(publication.TickIndex, Is.EqualTo(2));
            Assert.That(publication.InputHash, Is.EqualTo(input.GetCanonicalHash64()));
            // C++ poll preserves this tick's held keys; only the previous held state
            // moves into Prev*. Cooldowns/history prove that the new press was consumed.
            Assert.That(character.Runtime.KeyLeft, Is.EqualTo(1));
            Assert.That(character.Runtime.KeyAttack, Is.EqualTo(1));
            Assert.That(character.Runtime.PrevLeft, Is.Zero);
            Assert.That(character.Runtime.PrevAttack, Is.Zero);
            Assert.That(character.Runtime.CdLeft, Is.EqualTo(5));
            Assert.That(character.Runtime.CdDefend, Is.EqualTo(5));
            Assert.That(character.Runtime.InputHistory[4], Is.EqualTo(4));
            Assert.That(character.Runtime.InputHistory[5], Is.EqualTo(9));

            worker.AcknowledgePresentationConsumed(consumedSequence);
            worker.Stop();
            Assert.That(worker.Failure, Is.Null);

            character.UnregisterFromWorld();
            character.Reset();
        }

        [Test]
        public void DedicatedWorkerFullTickFinalizesPendingLogicEntityLifecycle()
        {
            const int oid = 31992;
            const int runtimeSlot = 20;
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 100,
                next = 0,
            };
            var data = new LF2CharacterData();
            data.frames.Add(frame);
            var wrapper = new LF2CharacterDataWrapper(oid, data);
            var world = new Simulation.SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        oid,
                        (int)LF2ObjectType.Other,
                        "logic-worker-lifecycle.dat"),
                },
                candidate => candidate == oid ? wrapper : null);
            world.SetLogicOnlyEntityMaterialization(true);

            OPointCreateTask task =
                world.LogicReferencePool.Fetch<OPointCreateTask>();
            task.targetWorld = world;
            task.requiredRuntimeSlot = runtimeSlot;
            task.opoint = new ObjectPoint
            {
                oid = oid,
                kind = 1,
                action = 0,
                facing = 0,
            };
            task.dir = "right";
            task.useDirectRuntimePosition = true;
            task.directX = 12;
            task.directY = 0;
            task.directZ = 34;
            task.skipPostInitZOffset = true;
            LF2Entity entity = world.LogicEntityFactory.Create(
                task,
                out Simulation.BattleLogicEntityCreationFailure failure);
            world.LogicReferencePool.Recycle(task);
            Assert.That(
                failure,
                Is.EqualTo(Simulation.BattleLogicEntityCreationFailure.None));
            Assert.That(entity, Is.Not.Null);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(1));
            entity.Runtime.PendingFlushDestroy = true;

            var tickSystem = new Simulation.NTSDBattleTickSystem(world);
            var executor = new Simulation.BattleWorldSimulationTickExecutor(
                world,
                tickSystem,
                captureChecksum: true);
            using var worker = new Simulation.DedicatedBattleSimulationWorker(
                2,
                1,
                executor);
            worker.Start();
            var stage = new Simulation.BattleSimulationStageSnapshot(
                1600,
                160,
                400,
                0,
                0);
            Assert.That(
                worker.TrySubmit(
                    Simulation.FrameInputSet.Empty(2),
                    buildPresentation: false,
                    in stage),
                Is.True);

            long consumedSequence = 0;
            Assert.That(
                SpinWait.SpinUntil(
                    () => worker.PublishedSequence > consumedSequence ||
                          worker.Failure != null,
                    3000),
                Is.True,
                "the worker did not publish the lifecycle tick");
            Assert.That(worker.Failure, Is.Null);
            Assert.That(
                worker.TryReadLatest(ref consumedSequence, out var publication),
                Is.True);
            Assert.That(publication.TickIndex, Is.EqualTo(2));
            Assert.That(publication.HasPresentationFrame, Is.False);
            Assert.That(
                world.FindEntityByRuntimeSlotForQuery(runtimeSlot),
                Is.Null);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            Assert.That(
                world.StructuralWriterDiagnosticsForDiagnostics.FreeCount,
                Is.GreaterThanOrEqualTo(1));

            worker.AcknowledgePresentationConsumed(consumedSequence);
            worker.Stop();
            Assert.That(worker.Failure, Is.Null);
        }

        [Test]
        public void DedicatedWorkerPreservesTickOrderAndWaitsForPresentationConsumption()
        {
            var executor = new RecordingTickExecutor();
            using var worker = new Simulation.DedicatedBattleSimulationWorker(
                4,
                1,
                executor);
            worker.Start();

            Assert.That(worker.TrySubmit(CreateSinglePlayerFrame(1)), Is.True);
            Assert.That(worker.TrySubmit(CreateSinglePlayerFrame(2)), Is.True);
            Assert.That(worker.TrySubmit(CreateSinglePlayerFrame(3)), Is.True);

            long consumedSequence = 0;
            for (int expectedTick = 1; expectedTick <= 3; expectedTick++)
            {
                Assert.That(
                    SpinWait.SpinUntil(
                        () => worker.PublishedSequence > consumedSequence ||
                              worker.Failure != null,
                        2000),
                    Is.True,
                    "the dedicated worker did not publish within the timeout");
                Assert.That(worker.Failure, Is.Null);
                Assert.That(
                    worker.TryReadLatest(ref consumedSequence, out var publication),
                    Is.True);
                Assert.That(publication.TickIndex, Is.EqualTo(expectedTick));
                Assert.That(publication.InputHash, Is.EqualTo((ulong)expectedTick * 101UL));

                if (expectedTick < 3)
                {
                    Thread.Sleep(5);
                    Assert.That(
                        worker.PublishedSequence,
                        Is.EqualTo(consumedSequence),
                        "the worker must not overwrite a frame before the presentation host acknowledges it");
                }

                worker.AcknowledgePresentationConsumed(consumedSequence);
                Assert.That(
                    SpinWait.SpinUntil(
                        () => worker.IsPresentationConsumptionFinalized(consumedSequence) ||
                              worker.Failure != null,
                        2000),
                    Is.True,
                    "the worker did not finish the presentation-consumption mutation boundary");
            }

            worker.Stop();
            Assert.That(executor.ExecutionCount, Is.EqualTo(3));
            Assert.That(executor.PresentationConsumedCount, Is.EqualTo(3));
            Assert.That(
                executor.ExecutionThreadId,
                Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId));
            Assert.That(
                executor.PresentationConsumedThreadId,
                Is.EqualTo(executor.ExecutionThreadId));
            Assert.That(worker.PendingInputCount, Is.Zero);
            Assert.That(worker.IsRunning, Is.False);
        }

        [Test]
        public void RuntimeDataCatalogFreezesManagerLookupsBeforeWorkerExecution()
        {
            var characterData = new LF2CharacterData();
            var wrapper = new LF2CharacterDataWrapper(7, characterData);
            var definitions = new[]
            {
                new ObjectDefinition(7, 3, "data/test.dat"),
            };
            var catalog = new Simulation.BattleRuntimeDataCatalog();

            catalog.Prepare(definitions, id => id == 7 ? wrapper : null);
            catalog.Seal();

            Assert.That(catalog.IsReady, Is.True);
            Assert.That(catalog.IsSealedForBattle, Is.True);
            Assert.That(catalog.GetObjectDefinition(7), Is.SameAs(definitions[0]));
            Assert.That(catalog.GetCharacterConfig(7), Is.SameAs(wrapper));
            Assert.That(catalog.GetCharacterData(7), Is.SameAs(characterData));
            Assert.That(catalog.GetObjectDefinition(8), Is.Null);
            Assert.Throws<InvalidOperationException>(() =>
                catalog.Prepare(definitions, _ => wrapper));

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                _ = catalog.GetObjectDefinition(7);
                _ = catalog.GetCharacterConfig(7);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void LogicEntityFactoryCreatesRegisteredEntityOffMainThreadWithoutRenderer()
        {
            const int oid = 31996;
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            };
            var characterData = new LF2CharacterData();
            characterData.frames.Add(frame);
            var wrapper = new LF2CharacterDataWrapper(oid, characterData);
            var definitions = new[]
            {
                new ObjectDefinition(oid, (int)LF2ObjectType.Other, "logic-worker.dat"),
            };
            var world = new Simulation.SimulationWorld();
            world.PrepareRuntimeDataCatalogForBattle(
                definitions,
                id => id == oid ? wrapper : null);
            world.SetLogicOnlyEntityMaterialization(true);
            OPointCreateTask task = world.LogicReferencePool.Fetch<OPointCreateTask>();
            task.targetWorld = world;
            task.requiredRuntimeSlot = world.DynamicRuntimeSlotStartForServices;
            task.opoint = new ObjectPoint
            {
                oid = oid,
                kind = 1,
                action = 0,
                facing = 0,
            };
            task.dir = "right";
            task.useDirectRuntimePosition = true;
            task.directX = 12;
            task.directY = 0;
            task.directZ = 34;
            task.skipPostInitZOffset = true;

            LF2Entity created = null;
            Simulation.BattleLogicEntityCreationFailure failure = default;
            Exception workerFailure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    created = world.LogicEntityFactory.Create(task, out failure);
                }
                catch (Exception exception)
                {
                    workerFailure = exception;
                }
            });
            thread.Start();
            thread.Join();

            Assert.That(workerFailure, Is.Null);
            Assert.That(failure, Is.EqualTo(
                Simulation.BattleLogicEntityCreationFailure.None));
            Assert.That(created, Is.Not.Null);
            Assert.That(created.Renderer, Is.Null);
            Assert.That(created.ShadowRenderer, Is.Null);
            Assert.That(created.Runtime.SlotIndex, Is.EqualTo(
                world.DynamicRuntimeSlotStartForServices));
            Assert.That(created.ObjectId, Is.EqualTo(oid));
            Assert.That(created.Runtime.XInt, Is.EqualTo(12));
            Assert.That(created.Runtime.ZInt, Is.EqualTo(34));

            created.UnregisterFromWorld();
            created.Reset();
            world.LogicReferencePool.Release(created);
            world.LogicReferencePool.Recycle(task);
        }

        [Test]
        public void WorkerPresentationUsesSealedPureHitRecordLifecycleCatalog()
        {
            const int oid = 31995;
            var characterData = new LF2CharacterData();
            characterData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            });
            var wrapper = new LF2CharacterDataWrapper(oid, characterData);
            var world = new Simulation.SimulationWorld();
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        oid,
                        (int)LF2ObjectType.Other,
                        "worker-hit-record.dat"),
                },
                id => id == oid ? wrapper : null,
                Simulation.BattleHitRecordLifecycleCatalog.Available);
            world.BattlePresentation.SetMode(BattlePresentationBackendMode.CentralOnly);

            var entity = new WorkerHitRecordFixtureEntity(oid);
            entity.SetRequiredRuntimeSlot(20);
            world.Register(entity);
            entity.AddHitRecord(0, 12, 34);

            world.BattlePresentation.BeginSimulationWorkerFrame(world, tickIndex: 1);
            BattleHitRecordPresentationCycle cycle =
                world.BattlePresentation.PublishedHitRecordCycle;

            Assert.That(cycle, Is.Not.Null);
            Assert.That(cycle.CommonVisualCatalog, Is.SameAs(BattleCommonVisualCatalog.Empty));
            Assert.That(cycle.LifecycleCatalog.IsAvailable, Is.True);
            Assert.That(world.BattlePresentation.FinalizePublishedHitRecordCycle(world), Is.True);
            Assert.That(entity.GetHitRecordAge(0), Is.EqualTo(1));
            Assert.That(world.BattlePresentation.FinalizePublishedHitRecordCycle(world), Is.False);
        }

        [Test]
        public void BeginBattleAllocationSealIsStrictNoOpAfterCentralSubmissionPublished()
        {
            BattleCentralRenderSystem.ResetRuntime();
            using var scope = new DriverScope();
            Simulation.SimulationTickDriver driver = scope.Driver;
            Simulation.SimulationWorld world = driver.World;
            bool battleSealStarted = false;
            try
            {
                world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                driver.ApplySettings(new Simulation.LockstepSimulationSettings
                {
                    driveMode = Simulation.SimulationDriveMode.Manual,
                    requireInputFrameReady = false,
                });

                world.RenderDispatchAll(1);
                BattlePixelFramePlan preSealPublished =
                    BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                Assert.That(preSealPublished.Submission, Is.Not.Null);
                Assert.That(preSealPublished.Submission.IsRetired, Is.False);

                Assert.DoesNotThrow(driver.BeginBattleAllocationSeal);
                battleSealStarted = true;
                Assert.That(preSealPublished.Submission.IsRetired, Is.True);
                Assert.That(driver.AllocationGate.IsSealed, Is.True);
                Assert.That(world.RuntimeCapacity.IsSealed, Is.True);

                world.RenderDispatchAll(2);
                BattlePixelFramePlan postSealPublished =
                    BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                Assert.That(postSealPublished.Submission, Is.Not.Null);
                Assert.That(postSealPublished.Submission.IsRetired, Is.False);

                Assert.DoesNotThrow(driver.BeginBattleAllocationSeal);
                Assert.That(driver.AllocationGate.IsSealed, Is.True);
                Assert.That(world.RuntimeCapacity.IsSealed, Is.True);
                Assert.That(
                    BattleCentralRenderSystem.CurrentPixelFramePlan.Generation,
                    Is.EqualTo(postSealPublished.Generation));
                Assert.That(postSealPublished.Submission.IsRetired, Is.False);
            }
            finally
            {
                if (battleSealStarted)
                    driver.EndBattleAllocationSeal();
                BattleCentralRenderSystem.ResetRuntime();
            }
        }

        [Test]
        public void FormalLocalDriverSubmitsConsumesAndStopsDedicatedWorker()
        {
            using var scope = new DriverScope();
            Simulation.SimulationTickDriver driver = scope.Driver;
            Simulation.SimulationWorld world = driver.World;
            var characterData = new LF2CharacterData();
            characterData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            });
            var wrapper = new LF2CharacterDataWrapper(31997, characterData);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        31997,
                        (int)LF2ObjectType.Other,
                        "formal-worker.dat"),
                },
                id => id == 31997 ? wrapper : null);
            driver.SetFrameInputProvider(new EmptyFrameInputProvider());

            driver.BeginBattleAllocationSeal();
            Assert.That(
                driver.DedicatedSimulationWorkerActiveForDiagnostics,
                Is.True,
                driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics);
            Assert.That(world.UsesLogicOnlyEntityMaterialization, Is.True);
            Assert.That(world.RuntimeDataCatalog.IsSealedForBattle, Is.True);

            MethodInfo consumeMethod = typeof(Simulation.SimulationTickDriver).GetMethod(
                "ConsumeDedicatedSimulationWorkerPublication",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(consumeMethod, Is.Not.Null);

            Assert.That(
                driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                    buildPresentation: false),
                Is.True,
                "the formal driver rejected its first worker tick submission: " +
                driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
            Assert.That(driver.DedicatedSimulationWorkerTickInFlightForDiagnostics, Is.True);
            Assert.That(
                SpinWait.SpinUntil(
                    () =>
                    {
                        consumeMethod.Invoke(driver, null);
                        return driver.CurrentTickIndex == 1 ||
                               driver.DedicatedSimulationWorkerFailureForDiagnostics != null;
                    },
                    2000),
                Is.True,
                "the formal driver did not consume the worker publication within the timeout");

            Assert.That(driver.DedicatedSimulationWorkerFailureForDiagnostics, Is.Null);
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(1));
            Assert.That(driver.LastAppliedFrameInput.TickIndex, Is.EqualTo(1));
            Assert.That(
                SpinWait.SpinUntil(
                    () => !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics ||
                          driver.DedicatedSimulationWorkerFailureForDiagnostics != null,
                    2000),
                Is.True,
                "the formal driver exposed the tick as complete before the worker finalized presentation consumption");
            Assert.That(driver.DedicatedSimulationWorkerTickInFlightForDiagnostics, Is.False);
            Assert.That(
                driver.DedicatedSimulationWorkerLastExecutionElapsedTimestampTicksForDiagnostics,
                Is.GreaterThan(0L));

            driver.EndBattleAllocationSeal();
            Assert.That(driver.DedicatedSimulationWorkerActiveForDiagnostics, Is.False);
            Assert.That(world.UsesLogicOnlyEntityMaterialization, Is.False);
            Assert.That(world.RuntimeDataCatalog.IsSealedForBattle, Is.False);
        }

        [Test]
        public void FormalLocalDriverPublishesCentralFramesAcknowledgesAndAdvancesNextTick()
        {
            BattleCentralRenderSystem.ResetRuntime();
            using var scope = new DriverScope();
            Simulation.SimulationTickDriver driver = scope.Driver;
            Simulation.SimulationWorld world = driver.World;
            bool battleSealStarted = false;
            try
            {
                var characterData = new LF2CharacterData();
                characterData.frames.Add(new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    pic = 0,
                    wait = 1,
                    next = 0,
                });
                var wrapper = new LF2CharacterDataWrapper(31999, characterData);
                world.PrepareRuntimeDataCatalogForBattle(
                    new[]
                    {
                        new ObjectDefinition(
                            31999,
                            (int)LF2ObjectType.Other,
                            "formal-worker-central.dat"),
                    },
                    id => id == 31999 ? wrapper : null);
                world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                driver.SetFrameInputProvider(new EmptyFrameInputProvider());

                driver.BeginBattleAllocationSeal();
                battleSealStarted = true;
                Assert.That(
                    driver.DedicatedSimulationWorkerActiveForDiagnostics,
                    Is.True,
                    driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics);

                MethodInfo consumeMethod = typeof(Simulation.SimulationTickDriver).GetMethod(
                    "ConsumeDedicatedSimulationWorkerPublication",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo acknowledgeMethod = typeof(Simulation.SimulationTickDriver).GetMethod(
                    "AcknowledgeDedicatedSimulationWorkerPresentation",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(consumeMethod, Is.Not.Null);
                Assert.That(acknowledgeMethod, Is.Not.Null);

                Assert.That(
                    driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                        buildPresentation: true),
                    Is.True,
                    driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
                Assert.That(
                    SpinWait.SpinUntil(
                        () =>
                        {
                            consumeMethod.Invoke(driver, null);
                            return driver.CurrentTickIndex == 1 ||
                                   driver.DedicatedSimulationWorkerFailureForDiagnostics != null;
                        },
                        2000),
                    Is.True,
                    "tick 1 worker publication was not consumed");
                Assert.That(driver.DedicatedSimulationWorkerFailureForDiagnostics, Is.Null);

                BattlePresentationFrame firstPublication =
                    world.BattlePresentation.PublishedFrame;
                Assert.That(firstPublication, Is.Not.Null);
                Assert.That(firstPublication.TickIndex, Is.EqualTo(1));
                Assert.That(firstPublication.PresentationOrderMaterialized, Is.False);
                Assert.That(firstPublication.CommandsMaterialized, Is.False);
                Assert.That(
                    driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                        buildPresentation: true),
                    Is.False,
                    "the single-flight worker gate must reject tick 2 before tick 1 acknowledgement");

                BattlePixelFramePlan firstReadyPlan =
                    BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
                BattlePixelFramePlan firstMaterializedPlan =
                    BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(2301);
                Assert.That(firstReadyPlan.IsValid, Is.True);
                Assert.That(firstMaterializedPlan.Generation, Is.EqualTo(firstReadyPlan.Generation));
                Assert.That(firstMaterializedPlan.SimulationTick, Is.EqualTo(1));
                Assert.That(firstMaterializedPlan.DisplayTick, Is.EqualTo(1));
                Assert.That(firstMaterializedPlan.World, Is.SameAs(world));
                Assert.That(firstMaterializedPlan.CapturedFrame, Is.Not.SameAs(firstPublication));
                Assert.That(firstMaterializedPlan.CapturedFrame.CommandsMaterialized, Is.True);
                Assert.That(firstPublication.CommandsMaterialized, Is.False,
                    "the presentation host must not mutate the worker's frozen publication");

                acknowledgeMethod.Invoke(driver, null);
                Assert.That(
                    SpinWait.SpinUntil(
                        () => !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics ||
                              driver.DedicatedSimulationWorkerFailureForDiagnostics != null,
                        2000),
                    Is.True,
                    "tick 1 acknowledgement did not release the worker single-flight gate");
                Assert.That(driver.DedicatedSimulationWorkerFailureForDiagnostics, Is.Null);

                Assert.That(
                    driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
                        buildPresentation: true),
                    Is.True,
                    driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
                Assert.That(
                    SpinWait.SpinUntil(
                        () =>
                        {
                            consumeMethod.Invoke(driver, null);
                            return driver.CurrentTickIndex == 2 ||
                                   driver.DedicatedSimulationWorkerFailureForDiagnostics != null;
                        },
                        2000),
                    Is.True,
                    "tick 2 worker publication was not consumed");
                Assert.That(driver.DedicatedSimulationWorkerFailureForDiagnostics, Is.Null);

                BattlePresentationFrame secondPublication =
                    world.BattlePresentation.PublishedFrame;
                Assert.That(secondPublication, Is.Not.Null);
                Assert.That(secondPublication, Is.Not.SameAs(firstPublication));
                Assert.That(secondPublication.TickIndex, Is.EqualTo(2));
                Assert.That(secondPublication.CommandsMaterialized, Is.False);

                BattlePixelFramePlan secondReadyPlan =
                    BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
                BattlePixelFramePlan secondMaterializedPlan =
                    BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(2302);
                Assert.That(secondReadyPlan.IsValid, Is.True);
                Assert.That(secondMaterializedPlan.Generation, Is.EqualTo(secondReadyPlan.Generation));
                Assert.That(
                    secondMaterializedPlan.Generation,
                    Is.Not.EqualTo(firstMaterializedPlan.Generation));
                Assert.That(secondMaterializedPlan.SimulationTick, Is.EqualTo(2));
                Assert.That(secondMaterializedPlan.DisplayTick, Is.EqualTo(2));
                Assert.That(secondMaterializedPlan.CapturedFrame, Is.Not.SameAs(secondPublication));
                Assert.That(secondMaterializedPlan.CapturedFrame.CommandsMaterialized, Is.True);
                Assert.That(secondPublication.CommandsMaterialized, Is.False);

                acknowledgeMethod.Invoke(driver, null);
                Assert.That(
                    SpinWait.SpinUntil(
                        () => !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics ||
                              driver.DedicatedSimulationWorkerFailureForDiagnostics != null,
                        2000),
                    Is.True,
                    "tick 2 acknowledgement did not finalize presentation consumption");
                Assert.That(driver.DedicatedSimulationWorkerFailureForDiagnostics, Is.Null);
            }
            finally
            {
                if (battleSealStarted)
                    driver.EndBattleAllocationSeal();
                BattleCentralRenderSystem.ResetRuntime();
            }
        }

        [Test]
        public void FormalCentralManualBattleUsesSameLogicOnlySpawnBoundaryWithoutWorker()
        {
            using var scope = new DriverScope();
            Simulation.SimulationTickDriver driver = scope.Driver;
            Simulation.SimulationWorld world = driver.World;
            var characterData = new LF2CharacterData();
            characterData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            });
            var wrapper = new LF2CharacterDataWrapper(31998, characterData);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        31998,
                        (int)LF2ObjectType.Other,
                        "formal-manual.dat"),
                },
                id => id == 31998 ? wrapper : null);
            driver.ApplySettings(new Simulation.LockstepSimulationSettings
            {
                driveMode = Simulation.SimulationDriveMode.Manual,
                requireInputFrameReady = false,
                enableFrameChecksum = true,
            });

            driver.BeginBattleAllocationSeal();

            Assert.That(driver.DedicatedSimulationWorkerActiveForDiagnostics, Is.False);
            Assert.That(
                driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics,
                Is.EqualTo("drive-mode-is-not-local-free-run"));
            Assert.That(
                world.UsesLogicOnlyEntityMaterialization,
                Is.True,
                "CentralOnly must not let Unity presentation-pool capacity decide whether a logical opoint exists.");
            Assert.That(
                driver.StepOneTick(
                    Simulation.FrameInputSet.Empty(1),
                    ignorePaused: true,
                    buildPresentation: false),
                Is.True);
            Assert.That(
                world.UsesLogicOnlyEntityMaterialization,
                Is.True,
                "The synchronous step entry must preserve the sealed battle's logical materialization boundary.");

            driver.EndBattleAllocationSeal();
            Assert.That(world.UsesLogicOnlyEntityMaterialization, Is.False);
        }

        [Test]
        public void FormalCentralBattleRejectsWorkerWhenEntityKeepsUnityRendererBinding()
        {
            const int objectId = 31996;
            const int runtimeSlot = 50;
            using var scope = new DriverScope();
            Simulation.SimulationTickDriver driver = scope.Driver;
            Simulation.SimulationWorld world = driver.World;
            var characterData = new LF2CharacterData();
            characterData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            });
            var wrapper = new LF2CharacterDataWrapper(objectId, characterData);
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        objectId,
                        (int)LF2ObjectType.Other,
                        "renderer-bound-worker-gate.dat"),
                },
                id => id == objectId ? wrapper : null);
            driver.SetFrameInputProvider(new EmptyFrameInputProvider());

            var rendererHost = new GameObject("RendererBoundWorkerGate")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            rendererHost.SetActive(false);
            LF2ObjectRenderer renderer = rendererHost.AddComponent<LF2ObjectRenderer>();
            var entity = new WorkerHitRecordFixtureEntity(objectId);
            entity.SetRequiredRuntimeSlot(runtimeSlot);
            entity.BindRendererForWorkerGateTest(renderer);
            world.Register(entity);
            bool battleSealStarted = false;
            try
            {
                driver.BeginBattleAllocationSeal();
                battleSealStarted = true;

                Assert.That(driver.DedicatedSimulationWorkerActiveForDiagnostics, Is.False);
                Assert.That(
                    driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics,
                    Is.EqualTo("unity-presentation-bindings-are-still-attached"));
                Assert.That(world.UsesLogicOnlyEntityMaterialization, Is.True);
                Assert.That(driver.DedicatedSimulationWorkerFailureForDiagnostics, Is.Null);
            }
            finally
            {
                if (battleSealStarted)
                    driver.EndBattleAllocationSeal();
                entity.UnregisterFromWorld();
                UnityEngine.Object.DestroyImmediate(rendererHost);
            }
        }

        private static Simulation.FrameInputSet CreateSinglePlayerFrame(int tickIndex)
        {
            return new Simulation.FrameInputSet(
                tickIndex,
                new[]
                {
                    new Simulation.SimulationPlayerInput(
                        0,
                        Simulation.SimulationInputButtons.Attack),
                });
        }

        private sealed class RecordingTickExecutor :
            Simulation.IBattleSimulationTickExecutor
        {
            internal int ExecutionCount { get; private set; }
            internal int PresentationConsumedCount { get; private set; }
            internal int ExecutionThreadId { get; private set; }
            internal int PresentationConsumedThreadId { get; private set; }

            public Simulation.BattleSimulationTickPublication Execute(
                in Simulation.BattleSimulationTickRequest request)
            {
                Simulation.FrameInputSet frameInput = request.FrameInput;
                ExecutionCount++;
                ExecutionThreadId = Thread.CurrentThread.ManagedThreadId;
                ulong inputHash = (ulong)frameInput.TickIndex * 101UL;
                return new Simulation.BattleSimulationTickPublication(
                    frameInput.TickIndex,
                    inputHash,
                    inputHash ^ 0x12345678UL,
                    true,
                    request.BuildPresentation,
                    777L);
            }

            public void OnPresentationConsumed(
                in Simulation.BattleSimulationTickPublication publication)
            {
                PresentationConsumedCount++;
                PresentationConsumedThreadId = Thread.CurrentThread.ManagedThreadId;
            }
        }

        private sealed class WorkerHitRecordFixtureEntity : LF2Entity
        {
            public WorkerHitRecordFixtureEntity(int oid)
            {
                ObjectId = oid;
                StableId = oid;
                Team = 1;
                RelationTeam = 1;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                Frame.D = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    pic = 0,
                    wait = 1000,
                    next = 0,
                };
                RefreshRuntimeSnapshot();
            }

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Other;

            public override void Reset()
            {
            }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
            {
            }

            internal void BindRendererForWorkerGateTest(LF2ObjectRenderer renderer)
            {
                Renderer = renderer;
            }
        }

        private sealed class DirectionSwitchTickExecutor :
            Simulation.IBattleSimulationTickExecutor
        {
            private readonly LF2Character character;

            internal DirectionSwitchTickExecutor(LF2Character character)
            {
                this.character = character;
            }

            public Simulation.BattleSimulationTickPublication Execute(
                in Simulation.BattleSimulationTickRequest request)
            {
                character.SwitchDir("left");
                return new Simulation.BattleSimulationTickPublication(
                    request.FrameInput.TickIndex,
                    request.FrameInput.GetCanonicalHash64(),
                    0UL,
                    false,
                    request.BuildPresentation,
                    1L);
            }

            public void OnPresentationConsumed(
                in Simulation.BattleSimulationTickPublication publication)
            {
            }
        }

        private sealed class EmptyFrameInputProvider :
            Simulation.ISimulationFrameInputProvider
        {
            private readonly Simulation.FrameInputSet frame =
                Simulation.FrameInputSet.Empty(0);

            public bool IsFrameInputReady(int tickIndex) => true;

            public Simulation.FrameInputSet GetFrameInput(int tickIndex)
            {
                frame.ResetPreallocated(tickIndex, null);
                return frame;
            }
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly Simulation.SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(Simulation.SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as Simulation.SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("FormalBattleSimulationWorkerTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<Simulation.SimulationTickDriver>();
                Driver.RecreateWorld();
                Driver.SetPaused(true);
            }

            public Simulation.SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
