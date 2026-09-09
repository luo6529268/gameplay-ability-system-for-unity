#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Animation;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B5HitCandidatePairSnapshotCarrierEditorTests
    {
        [Test]
        public void Snapshot_RoundTripsEveryAuthorityPayload()
        {
            BattleHitCandidatePairSnapshot snapshot = CreateSnapshot(1);

            Assert.That(snapshot.Valid, Is.True);
            Assert.That(snapshot.AttackerObjectId, Is.EqualTo(101));
            Assert.That(snapshot.TargetObjectId, Is.EqualTo(201));
            Assert.That(snapshot.AttackerObjectType, Is.EqualTo(301));
            Assert.That(snapshot.TargetObjectType, Is.EqualTo(401));
            Assert.That(snapshot.AttackerBattleGroup, Is.EqualTo(501));
            Assert.That(snapshot.TargetBattleGroup, Is.EqualTo(601));
            Assert.That(snapshot.AttackerAction, Is.EqualTo(701));
            Assert.That(snapshot.TargetAction, Is.EqualTo(801));
            Assert.That(snapshot.AttackerCurrentState, Is.EqualTo(901));
            Assert.That(snapshot.TargetCurrentState, Is.EqualTo(1001));
            Assert.That(snapshot.AttackerPreviousState, Is.EqualTo(1101));
            Assert.That(snapshot.TargetPreviousState, Is.EqualTo(1201));
            Assert.That(snapshot.AttackerTickState, Is.EqualTo(1301));
            Assert.That(snapshot.TargetTickState, Is.EqualTo(1401));
            Assert.That(snapshot.AttackerFacing, Is.True);
            Assert.That(snapshot.TargetFacing, Is.False);
            Assert.That(snapshot.LinkedHolderPresent, Is.True);
            Assert.That(snapshot.LinkedHolderBattleGroup, Is.EqualTo(1501));
        }

        [Test]
        public void DefaultSnapshot_IsInvalidAndValueEqualityIsExact()
        {
            BattleHitCandidatePairSnapshot invalid = default;
            BattleHitCandidatePairSnapshot first = CreateSnapshot(3);
            BattleHitCandidatePairSnapshot same = CreateSnapshot(3);
            BattleHitCandidatePairSnapshot different = CreateSnapshot(4);

            Assert.That(invalid.Valid, Is.False);
            Assert.That(first, Is.EqualTo(same));
            Assert.That(first == same, Is.True);
            Assert.That(first != different, Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(same.GetHashCode()));
        }

        [Test]
        public void SceneQueryAndStoreEntries_PreserveSnapshotByValue()
        {
            BattleHitCandidatePairSnapshot snapshot = CreateSnapshot(5);
            var hit = new SceneQueryHit(
                null,
                17,
                23,
                7,
                null,
                true,
                false,
                snapshot);
            var entry = new CollisionCandidateStoreEntry(
                17,
                new RuntimeEntityHandle(17, 9),
                23,
                7,
                null,
                true,
                false,
                snapshot);

            Assert.That(hit.PairSnapshot, Is.EqualTo(snapshot));
            Assert.That(entry.PairSnapshot, Is.EqualTo(snapshot));
        }

        [Test]
        public void FixedStore_RoundTripsSnapshotWithoutWarmAllocation()
        {
            var diagnostics = new CollisionCandidateStoreDiagnostics();
            var store = new CollisionCandidateStore(diagnostics);
            var attacker = new RuntimeEntityHandle(2, 7);
            var target = new RuntimeEntityHandle(3, 11);
            BattleHitCandidatePairSnapshot snapshot = CreateSnapshot(9);
            var entry = new CollisionCandidateStoreEntry(
                3,
                target,
                41,
                5,
                null,
                false,
                true,
                snapshot);

            Assert.That(store.PrepareCapacity(8), Is.True);
            Assert.That(WarmRoundTrip(store, attacker, in entry, snapshot), Is.True);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool allMatched = true;
            for (int index = 0; index < 4096; index++)
                allMatched &= WarmRoundTrip(store, attacker, in entry, snapshot);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allMatched, Is.True);
            Assert.That(allocated, Is.Zero);
            Assert.That(diagnostics.FirstMismatchReason,
                Is.EqualTo(CollisionCandidateStoreMismatchReason.None));
        }

        private static bool WarmRoundTrip(
            CollisionCandidateStore store,
            RuntimeEntityHandle attacker,
            in CollisionCandidateStoreEntry entry,
            BattleHitCandidatePairSnapshot expected)
        {
            if (!store.BeginBuild(8) ||
                !store.TryBeginAttacker(attacker) ||
                !store.TryWriteAt(attacker, 0, in entry) ||
                !store.CompleteBuild() ||
                !store.TryGetVisibleCandidate(
                    attacker,
                    0,
                    out CollisionCandidateStoreEntry actual))
            {
                store.EndTickVisibility();
                return false;
            }

            bool matched = actual.PairSnapshot == expected;
            store.EndTickVisibility();
            return matched;
        }

        private static BattleHitCandidatePairSnapshot CreateSnapshot(int delta)
        {
            return new BattleHitCandidatePairSnapshot(
                true,
                100 + delta,
                200 + delta,
                300 + delta,
                400 + delta,
                500 + delta,
                600 + delta,
                700 + delta,
                800 + delta,
                900 + delta,
                1000 + delta,
                1100 + delta,
                1200 + delta,
                1300 + delta,
                1400 + delta,
                (delta & 1) != 0,
                (delta & 1) == 0,
                true,
                1500 + delta);
        }
    }
}
#endif
