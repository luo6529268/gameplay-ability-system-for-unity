#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleStateSnapshotRestoreEditorTests
    {
        [Test]
        public void ExactInPlaceRestoreReinstatesCanonicalEntityAndWorldState()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var character = new LF2Character
            {
                ObjectId = 7,
            };
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);
            character.Runtime.X = 112.5;
            character.Runtime.Y = -8.25;
            character.Runtime.Z = 240.75;
            character.Runtime.HP = 321;
            character.Runtime.PP = 207;
            character.Runtime.ComboCountAtk = 4;
            scope.Driver.World.Runtime.Flow.AiRand15 = 11;
            scope.Driver.World.Rng.Seed(917u);
            scope.Driver.World.Rng.NextInt(0, 97);

            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);
            ulong expectedRngState = snapshot.Core.RngState;
            ulong expectedRngCalls = snapshot.Core.RngCallCount;

            character.Runtime.X = 999.0;
            character.Runtime.Y = 888.0;
            character.Runtime.Z = 777.0;
            character.Runtime.HP = 1;
            character.Runtime.PP = 2;
            character.Runtime.ComboCountAtk = 99;
            scope.Driver.World.Runtime.Flow.AiRand15 = 1;
            scope.Driver.World.Rng.NextInt(0, 97);

            Assert.That(
                scope.Driver.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());
            Assert.That(scope.Driver.CurrentTickIndex, Is.Zero);
            Assert.That(character.Runtime.X, Is.EqualTo(112.5));
            Assert.That(character.Runtime.Y, Is.EqualTo(-8.25));
            Assert.That(character.Runtime.Z, Is.EqualTo(240.75));
            Assert.That(character.Runtime.HP, Is.EqualTo(321));
            Assert.That(character.Runtime.PP, Is.EqualTo(207));
            Assert.That(character.Runtime.ComboCountAtk, Is.EqualTo(4));
            Assert.That(scope.Driver.World.Runtime.Flow.AiRand15, Is.EqualTo(11));
            Assert.That(scope.Driver.World.Rng.State, Is.EqualTo(expectedRngState));
            Assert.That(scope.Driver.World.Rng.CallCount, Is.EqualTo(expectedRngCalls));
            Assert.That(character.ItrRest.IsBound, Is.True);
        }

        [Test]
        public void ExactRestoreReinstatesRelationsRestAndPendingEvents()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var character = new LF2Character { ObjectId = 7 };
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);
            var weapon = new LF2Weapon { ObjectId = 100 };
            weapon.SetRequiredRuntimeSlot(4);
            scope.Driver.World.Register(weapon);
            var attacker = new LF2Character { ObjectId = 8 };
            attacker.SetRequiredRuntimeSlot(5);
            scope.Driver.World.Register(attacker);

            character.TrackerParent = weapon;
            character.Catching = attacker;
            character.Attacker = attacker;
            character.HeldWeaponReferenceInternal = weapon;
            Assert.That(
                scope.Driver.World.RuntimeRestStoreForServices.SetARest(3, 7),
                Is.True);
            Assert.That(
                scope.Driver.World.RuntimeRestStoreForServices.SetVRest(5, 3, 11),
                Is.True);
            scope.Driver.World.PendingSounds.Add(
                new PendingSoundEvent("SFX_SNAPSHOT", 42, -17));

            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);

            character.TrackerParent = null;
            character.Catching = null;
            character.Attacker = null;
            character.HeldWeaponReferenceInternal = null;
            scope.Driver.World.RuntimeRestStoreForServices.SetARest(3, 0);
            scope.Driver.World.RuntimeRestStoreForServices.SetVRest(5, 3, 0);
            scope.Driver.World.PendingSounds.Clear();

            Assert.That(
                scope.Driver.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());
            Assert.That(character.TrackerParent, Is.SameAs(weapon));
            Assert.That(character.Catching, Is.SameAs(attacker));
            Assert.That(character.Attacker, Is.SameAs(attacker));
            Assert.That(character.HeldWeaponReferenceInternal, Is.SameAs(weapon));
            Assert.That(
                scope.Driver.World.RuntimeRestStoreForServices.GetARest(3),
                Is.EqualTo(7));
            Assert.That(
                scope.Driver.World.RuntimeRestStoreForServices.GetVRest(5, 3),
                Is.EqualTo(11));
            Assert.That(scope.Driver.World.PendingSounds, Has.Count.EqualTo(1));
            Assert.That(scope.Driver.World.PendingSounds[0].Cue,
                Is.EqualTo("SFX_SNAPSHOT"));
            Assert.That(scope.Driver.World.PendingSounds[0].WorldX, Is.EqualTo(42));
            Assert.That(scope.Driver.World.PendingSounds[0].Tick, Is.EqualTo(-17));
        }

        [Test]
        public void PureValueSnapshotRebuildsEntityShellInFreshWorld()
        {
            const int oid = 31997;
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            };
            var data = new LF2CharacterData();
            data.frames.Add(frame);
            var wrapper = new LF2CharacterDataWrapper(oid, data);
            var definitions = new[]
            {
                new ObjectDefinition(oid, (int)LF2ObjectType.Character,
                    "snapshot-transfer.dat"),
            };

            var source = new SimulationWorld();
            source.PrepareRuntimeDataCatalogForBattle(
                definitions,
                id => id == oid ? wrapper : null);
            source.SetLogicOnlyEntityMaterialization(true);
            OPointCreateTask task = source.LogicReferencePool.Fetch<OPointCreateTask>();
            task.targetWorld = source;
            task.requiredRuntimeSlot = 3;
            task.opoint = new ObjectPoint
            {
                oid = oid,
                kind = 1,
                action = 0,
            };
            task.dir = "right";
            task.preserveActionZero = true;
            task.useDirectRuntimePosition = true;
            task.skipPostInitZOffset = true;
            LF2Entity sourceEntity = source.LogicEntityFactory.Create(
                task,
                out BattleLogicEntityCreationFailure creationFailure);
            source.LogicReferencePool.Recycle(task);
            Assert.That(sourceEntity, Is.Not.Null, creationFailure.ToString());
            sourceEntity.Runtime.X = 123.25;
            sourceEntity.Runtime.Y = -7.5;
            sourceEntity.Runtime.Z = 211.75;
            sourceEntity.Runtime.HP = 333;

            BattleStateSnapshotBuffer snapshot =
                source.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(
                source.TryCaptureBattleStateSnapshot(identity, 0, snapshot),
                Is.True);
            int expectedStableId = sourceEntity.Runtime.StableId;
            snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();

            var destination = new SimulationWorld();
            destination.PrepareRuntimeDataCatalogForBattle(
                definitions,
                id => id == oid ? wrapper : null);
            destination.SetLogicOnlyEntityMaterialization(true);
            Assert.That(
                destination.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());
            Assert.That(
                destination.TryGetRuntimeSlotReadOnlyView(
                    3,
                    out RuntimeSlotTable.ReadOnlySlotView restoredView),
                Is.True);
            Assert.That(restoredView.Claimed, Is.True);
            Assert.That(restoredView.Entity, Is.TypeOf<LF2Character>());
            Assert.That(restoredView.Entity, Is.Not.SameAs(sourceEntity));
            Assert.That(restoredView.Entity.Renderer, Is.Null);
            Assert.That(restoredView.Entity.Runtime.StableId,
                Is.EqualTo(expectedStableId));
            Assert.That(restoredView.Entity.Runtime.X, Is.EqualTo(123.25));
            Assert.That(restoredView.Entity.Runtime.Y, Is.EqualTo(-7.5));
            Assert.That(restoredView.Entity.Runtime.Z, Is.EqualTo(211.75));
            Assert.That(restoredView.Entity.Runtime.HP, Is.EqualTo(333));
            Assert.That(restoredView.Generation,
                Is.EqualTo(snapshot.RuntimeSlots.GetSlot(3).Generation));
        }

        [Test]
        public void WarmExactRestoreDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);
            Assert.That(
                scope.Driver.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure warmFailure),
                Is.True,
                warmFailure.ToString());

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 128; index++)
            {
                if (!scope.Driver.TryRestoreBattleStateSnapshot(
                        identity,
                        snapshot,
                        out _))
                {
                    Assert.Fail($"Restore failed at iteration {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void RestoreRebuildsClaimedSlotsAndGenerationsAfterLifecycleChanges()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var restored = new LF2Character { ObjectId = 7 };
            restored.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(restored);
            Assert.That(
                scope.Driver.World.TryGetCurrentRuntimeHandle(
                    3,
                    restored,
                    out RuntimeEntityHandle snapshotHandle),
                Is.True);

            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);

            scope.Driver.World.Unregister(restored);
            var future = new LF2Character { ObjectId = 8 };
            future.SetRequiredRuntimeSlot(4);
            scope.Driver.World.Register(future);
            Assert.That(
                scope.Driver.World.TryGetCurrentRuntimeHandle(
                    4,
                    future,
                    out RuntimeEntityHandle futureHandle),
                Is.True);

            Assert.That(
                scope.Driver.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());

            Assert.That(scope.Driver.World.ObjectCount, Is.EqualTo(1));
            Assert.That(scope.Driver.World.ClaimedRuntimeSlotCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                scope.Driver.World.TryGetRuntimeSlotReadOnlyView(
                    3,
                    out RuntimeSlotTable.ReadOnlySlotView restoredView),
                Is.True);
            Assert.That(restoredView.Claimed, Is.True);
            Assert.That(restoredView.Entity, Is.SameAs(restored));
            Assert.That(restoredView.Generation, Is.EqualTo(snapshotHandle.Generation));
            Assert.That(restored.Runtime.SlotIndex, Is.EqualTo(3));
            Assert.That(restored.ItrRest.IsBound, Is.True);
            Assert.That(
                scope.Driver.World.TryResolveRuntimeHandle(
                    snapshotHandle,
                    out LF2Entity resolved),
                Is.True);
            Assert.That(resolved, Is.SameAs(restored));
            Assert.That(
                scope.Driver.World.TryResolveRuntimeHandle(futureHandle, out _),
                Is.False);
            Assert.That(future.Runtime.SlotIndex, Is.EqualTo(-1));
        }

        [Test]
        public void IdentityMismatchFailsBeforeMutatingWorld()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var character = new LF2Character
            {
                ObjectId = 7,
            };
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);
            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);

            character.Runtime.StableId++;
            character.Runtime.X = 901.0;
            scope.Driver.World.Runtime.Flow.AiRand15 = 13;

            Assert.That(
                scope.Driver.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.False);
            Assert.That(failure,
                Is.EqualTo(BattleStateSnapshotRestoreFailure.EntityIdentityMismatch));
            Assert.That(character.Runtime.X, Is.EqualTo(901.0));
            Assert.That(scope.Driver.World.Runtime.Flow.AiRand15, Is.EqualTo(13));
        }

        [Test]
        public void RestoreAndReplayReachesOriginalChecksumWithoutRewritingHistory()
        {
            using var scope = new DriverScope();
            scope.Driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                enableFrameChecksum = true,
            });
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                16,
                16,
                snapshotIntervalTicks: 2,
                snapshotCapacity: 4);

            for (int tick = 1; tick <= 6; tick++)
            {
                Assert.That(
                    session.TryAdvanceManual(
                        CanonicalNeutralFrame(tick),
                        buildPresentation: false),
                    Is.True,
                    $"tick={tick}, reason={session.LastReason}");
            }
            Assert.That(
                session.SnapshotRing.TryGet(
                    2,
                    out BattleStateSnapshotBuffer tickTwoSnapshot),
                Is.True);
            Assert.That(
                session.ChecksumHistory.TryGet(
                    6,
                    out LockstepChecksumHistoryEntry expected),
                Is.True);
            int journalCount = session.Journal.Count;
            int frameHistoryCount = session.FrameHistory.Count;
            int checksumHistoryCount = session.ChecksumHistory.Count;
            int snapshotCount = session.SnapshotRing.Count;

            Assert.That(
                session.TryRestoreAndReplay(tickTwoSnapshot),
                Is.True,
                session.LastReason.ToString());

            Assert.That(session.CurrentTick, Is.EqualTo(6));
            Assert.That(scope.Driver.CurrentTickIndex, Is.EqualTo(6));
            Assert.That(scope.Driver.LastAppliedFrameInput.TickIndex, Is.EqualTo(6));
            Assert.That(scope.Driver.HasFrameChecksum, Is.True);
            Assert.That(scope.Driver.LastFrameChecksumValue,
                Is.EqualTo(expected.StateChecksum));
            Assert.That(session.Journal.Count, Is.EqualTo(journalCount));
            Assert.That(session.FrameHistory.Count, Is.EqualTo(frameHistoryCount));
            Assert.That(session.ChecksumHistory.Count, Is.EqualTo(checksumHistoryCount));
            Assert.That(session.SnapshotRing.Count, Is.EqualTo(snapshotCount));
            Assert.That(session.Status, Is.EqualTo(LockstepSessionStatus.Advanced));
            Assert.That(session.LastReason, Is.EqualTo(LockstepProtocolReason.None));
        }

        [Test]
        public void RestoreAndReplayRecoversEntityRemovedAfterSnapshot()
        {
            using var scope = new DriverScope();
            scope.Driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                enableFrameChecksum = true,
            });
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 16, 16);
            var character = new LF2Character { ObjectId = 7 };
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);
            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);

            for (int tick = 1; tick <= 3; tick++)
            {
                Assert.That(
                    session.TryAdvanceManual(
                        CanonicalNeutralFrame(tick),
                        buildPresentation: false),
                    Is.True,
                    $"tick={tick}, reason={session.LastReason}");
            }
            Assert.That(
                session.ChecksumHistory.TryGet(
                    3,
                    out LockstepChecksumHistoryEntry expected),
                Is.True);
            scope.Driver.World.Unregister(character);
            Assert.That(scope.Driver.World.ObjectCount, Is.Zero);

            Assert.That(
                session.TryRestoreAndReplay(snapshot),
                Is.True,
                session.LastReason.ToString());

            Assert.That(session.CurrentTick, Is.EqualTo(3));
            Assert.That(scope.Driver.World.ObjectCount, Is.EqualTo(1));
            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(3));
            Assert.That(scope.Driver.LastFrameChecksumValue,
                Is.EqualTo(expected.StateChecksum));
        }

        [Test]
        public void ReplayWithoutRetainedChecksumFailsBeforeRestore()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                8,
                8,
                snapshotIntervalTicks: 1,
                snapshotCapacity: 4);

            Assert.That(
                session.TryAdvanceManual(
                    CanonicalNeutralFrame(1),
                    buildPresentation: false),
                Is.True);
            Assert.That(
                session.SnapshotRing.TryGet(
                    1,
                    out BattleStateSnapshotBuffer tickOneSnapshot),
                Is.True);
            Assert.That(
                session.TryAdvanceManual(
                    CanonicalNeutralFrame(2),
                    buildPresentation: false),
                Is.True);

            Assert.That(session.TryRestoreAndReplay(tickOneSnapshot), Is.False);
            Assert.That(session.LastReason,
                Is.EqualTo(LockstepProtocolReason.ReplayHistoryUnavailable));
            Assert.That(scope.Driver.CurrentTickIndex, Is.EqualTo(2));
        }

        private static FrameInputSet CanonicalNeutralFrame(int tick)
        {
            return new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(2, SimulationInputButtons.None),
                new SimulationPlayerInput(5, SimulationInputButtons.None),
            });
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("BattleStateSnapshotRestoreTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                Driver.RecreateWorld();
                Driver.SetPaused(true);
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
#endif
