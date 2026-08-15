#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleWorldRosterResultsSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsRosterResultLabelAndStatisticValues()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleRuntimeState runtime = scope.Driver.World.Runtime;
            var destination = new BattleWorldRosterResultsSnapshotBuffer();

            BattleSlotRuntimeState slot = runtime.Roster.Slots[3];
            slot.Active = true;
            slot.IsHuman = false;
            slot.CharacterId = 17;
            slot.Team = 4;
            slot.InputId = 7;
            slot.AiId = 9;
            slot.RuntimeSlotIndex = 303;
            slot.StableId = 9003;
            runtime.Roster.ActiveSlotCount = 1;

            BattleResultsRuntimeState results = runtime.Results;
            results.Phase = 210;
            results.Cursor = 5;
            results.SettingsCursor = 4;
            results.TableCursor = 8;
            results.TableSide = 1;
            results.ResultSubcursor = 6;
            results.Timer = 77;
            results.Winner = 4;
            results.HadBoth = true;
            results.BattleEndPhase = 2;
            results.PendingWinner = 1;
            results.TeamCount = 2;
            results.TeamIds[0] = 4;
            results.TeamIds[1] = 7;
            results.PendingHostAction = BattleResultsRuntimeState.HostActionRematch;
            results.ResultMultiplier[1] = 150;
            results.ResultSelectedTroop[1] = 6;
            results.ResultSelectedIcon[1] = 3;
            results.ResultTableTop[1] = 2;
            results.ResultTableBottom[1] = 4;
            results.ResultTableSavedTop = 1;
            results.ResultTableSavedBottom = 3;
            results.ResultRow1Values[1, 10] = 31;
            results.ResultRow2Values[1, 10] = 32;
            results.ResultCommittedTotal[1, 10] = 63;
            results.ResultCommittedHp[1, 10] = 31;
            results.ResultBackupRow1Values[1, 10] = 41;
            results.ResultBackupRow2Values[1, 10] = 42;

            runtime.SlotLabels.BattleSlotLabels[2, 5] = 'N';
            runtime.SlotLabels.BattleSlotLabelState[2] = 3;
            runtime.KillStats[2] = 12;
            runtime.DamageStats[2] = 345;
            runtime.ReserveOwnerValid = true;
            runtime.ReserveCommittedTotal[1, 10] = 81;
            runtime.ReserveCommittedHp[1, 10] = 82;

            Assert.That(
                session.TryCaptureWorldRosterResultsSnapshot(destination),
                Is.True);

            slot.CharacterId = 99;
            results.Phase = 0;
            results.TeamIds[1] = -1;
            results.ResultRow1Values[1, 10] = 0;
            runtime.SlotLabels.BattleSlotLabels[2, 5] = '\0';
            runtime.KillStats[2] = 0;
            runtime.ReserveCommittedHp[1, 10] = 0;

            BattleRosterSlotSnapshot capturedSlot = destination.GetRosterSlot(3);
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldRosterResultsSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion, Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.EqualTo(0));
            Assert.That(destination.ActiveRosterSlotCount, Is.EqualTo(1));
            Assert.That(capturedSlot.Active, Is.True);
            Assert.That(capturedSlot.IsHuman, Is.False);
            Assert.That(capturedSlot.CharacterId, Is.EqualTo(17));
            Assert.That(capturedSlot.Team, Is.EqualTo(4));
            Assert.That(capturedSlot.InputId, Is.EqualTo(7));
            Assert.That(capturedSlot.AiId, Is.EqualTo(9));
            Assert.That(capturedSlot.RuntimeSlotIndex, Is.EqualTo(303));
            Assert.That(capturedSlot.StableId, Is.EqualTo(9003));
            Assert.That(destination.ResultsPhase, Is.EqualTo(210));
            Assert.That(destination.ResultsCursor, Is.EqualTo(5));
            Assert.That(destination.ResultsSettingsCursor, Is.EqualTo(4));
            Assert.That(destination.ResultsTableCursor, Is.EqualTo(8));
            Assert.That(destination.ResultsTableSide, Is.EqualTo(1));
            Assert.That(destination.ResultsSubcursor, Is.EqualTo(6));
            Assert.That(destination.ResultsTimer, Is.EqualTo(77));
            Assert.That(destination.ResultsWinner, Is.EqualTo(4));
            Assert.That(destination.ResultsHadBoth, Is.True);
            Assert.That(destination.ResultsBattleEndPhase, Is.EqualTo(2));
            Assert.That(destination.ResultsPendingWinner, Is.EqualTo(1));
            Assert.That(destination.ResultsTeamCount, Is.EqualTo(2));
            Assert.That(destination.GetResultTeamId(1), Is.EqualTo(7));
            Assert.That(destination.ResultsPendingHostAction,
                Is.EqualTo(BattleResultsRuntimeState.HostActionRematch));
            Assert.That(destination.GetResultMultiplier(1), Is.EqualTo(150));
            Assert.That(destination.GetResultSelectedTroop(1), Is.EqualTo(6));
            Assert.That(destination.GetResultSelectedIcon(1), Is.EqualTo(3));
            Assert.That(destination.GetResultTableTop(1), Is.EqualTo(2));
            Assert.That(destination.GetResultTableBottom(1), Is.EqualTo(4));
            Assert.That(destination.ResultsTableSavedTop, Is.EqualTo(1));
            Assert.That(destination.ResultsTableSavedBottom, Is.EqualTo(3));
            Assert.That(destination.GetResultRow1Value(1, 10), Is.EqualTo(31));
            Assert.That(destination.GetResultRow2Value(1, 10), Is.EqualTo(32));
            Assert.That(destination.GetResultCommittedTotal(1, 10), Is.EqualTo(63));
            Assert.That(destination.GetResultCommittedHp(1, 10), Is.EqualTo(31));
            Assert.That(destination.GetResultBackupRow1Value(1, 10), Is.EqualTo(41));
            Assert.That(destination.GetResultBackupRow2Value(1, 10), Is.EqualTo(42));
            Assert.That(destination.GetSlotLabel(2, 5), Is.EqualTo('N'));
            Assert.That(destination.GetSlotLabelState(2), Is.EqualTo(3));
            Assert.That(destination.GetKillStat(2), Is.EqualTo(12));
            Assert.That(destination.GetDamageStat(2), Is.EqualTo(345));
            Assert.That(destination.ReserveOwnerValid, Is.True);
            Assert.That(destination.GetReserveCommittedTotal(1, 10), Is.EqualTo(81));
            Assert.That(destination.GetReserveCommittedHp(1, 10), Is.EqualTo(82));
        }

        [Test]
        public void InvalidSourceBufferFailsWithoutPartiallyOverwritingDestination()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var destination = new BattleWorldRosterResultsSnapshotBuffer();
            BattleResultsRuntimeState results = scope.Driver.World.Runtime.Results;
            results.Phase = 210;

            Assert.That(
                session.TryCaptureWorldRosterResultsSnapshot(destination),
                Is.True);
            results.Phase = 240;
            results.TeamIds = new int[1];

            Assert.That(
                session.TryCaptureWorldRosterResultsSnapshot(destination),
                Is.False);
            Assert.That(destination.ResultsPhase, Is.EqualTo(210));
        }

        [Test]
        public void WarmRosterResultsCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var destination = new BattleWorldRosterResultsSnapshotBuffer();
            Assert.That(
                session.TryCaptureWorldRosterResultsSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldRosterResultsSnapshot(destination))
                {
                    Assert.Fail($"Roster/results capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
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
                host = new GameObject("BattleWorldRosterResultsSnapshotTests")
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
