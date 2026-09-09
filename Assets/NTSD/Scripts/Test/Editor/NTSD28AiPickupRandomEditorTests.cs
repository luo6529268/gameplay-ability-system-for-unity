using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28AiPickupRandomEditorTests
    {
        [Test]
        public void NativeSensing_Selects1000AndIgnoresLegacy1004()
        {
            AiDecisionSnapshot snapshot = CreatePickupSnapshot(
                selfX: 0,
                pickupX: 300,
                pickupState: 1000,
                capacity: 22);
            SetRow(snapshot.Rows, 21, objectId: 101, dataType: 1,
                state: 1004, x: 200, z: 0, team: 0);

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot.Rows,
                0,
                0,
                1,
                600,
                false,
                AiDecisionEvaluationPolicy.FullScan,
                useNative28PickupState: true,
                out AiSensingSpecialResult result), Is.True);

            Assert.That(result.SelectedSlot, Is.EqualTo(20));
        }

        [Test]
        public void LegacySensing_Keeps1004Gate()
        {
            AiDecisionSnapshot snapshot = CreatePickupSnapshot(
                selfX: 0,
                pickupX: 300,
                pickupState: 1004,
                capacity: 21);

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot.Rows,
                0,
                0,
                1,
                600,
                false,
                AiDecisionEvaluationPolicy.FullScan,
                out AiSensingSpecialResult result), Is.True);

            Assert.That(result.SelectedSlot, Is.EqualTo(20));
        }

        [TestCase(0, 300, 0x17u)]
        [TestCase(500, 100, 0x16u)]
        public void NativePickupFarX_ConsumesDirectionalSiteAfter14(
            int selfX,
            int pickupX,
            uint expectedSite)
        {
            var owner = new NTSD28NativeRandom(0x13579BDFu);
            AiDecisionSnapshot snapshot = CreatePickupSnapshot(
                selfX,
                pickupX,
                pickupState: 1000,
                capacity: 21);
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            Assert.That(witness.FinalSelectedSlot, Is.EqualTo(20));
            Assert.That(witness.Exit, Is.EqualTo(AiDecisionExit.SpecialTarget));
            AssertSites(snapshot, witness, 0x14u, expectedSite);
        }

        [Test]
        public void NativePickupWithin250_Consumes14ButNotDirectionalSite()
        {
            var owner = new NTSD28NativeRandom(0x2468ACE0u);
            AiDecisionSnapshot snapshot = CreatePickupSnapshot(
                selfX: 0,
                pickupX: 200,
                pickupState: 1000,
                capacity: 21);
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            AssertSites(snapshot, witness, 0x14u);
        }

        [Test]
        public void WarmPickupEvaluationCommit_AllocatesZeroManagedBytes()
        {
            var owner = new NTSD28NativeRandom(0x55667788u);
            AiDecisionSnapshot snapshot = CreatePickupSnapshot(
                selfX: 0,
                pickupX: 300,
                pickupState: 1000,
                capacity: 21);
            AiDecisionWitness witness = default;
            for (int index = 0; index < 16; index++)
                EvaluateAndCommit(owner, snapshot, ref witness);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool passed = true;
            for (int index = 0; index < 4096; index++)
                passed &= EvaluateAndCommit(owner, snapshot, ref witness);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(passed, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static bool EvaluateAndCommit(
            NTSD28NativeRandom owner,
            AiDecisionSnapshot snapshot,
            ref AiDecisionWitness witness)
        {
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());
            if (!AiDecisionKernel.TryEvaluate(
                    snapshot,
                    AiDecisionEvaluationPolicy.FullScan,
                    false,
                    ref witness) ||
                !witness.TryGetSynchronizedRngCursor(
                    out NTSD28SynchronizedRandomCursor candidate))
            {
                return false;
            }
            return owner.TryCommitSynchronizedCursor(candidate);
        }

        private static AiDecisionSnapshot CreatePickupSnapshot(
            int selfX,
            int pickupX,
            int pickupState,
            int capacity)
        {
            const ulong epoch = 23UL;
            var snapshot = new AiDecisionSnapshot(capacity);
            snapshot.Reset(epoch);
            SetRow(snapshot.Rows, 0, objectId: 1, dataType: 0,
                state: 2, x: selfX, z: 0, team: 1);
            int primaryX = selfX == 0 ? 500 : 0;
            SetRow(snapshot.Rows, 1, objectId: 2, dataType: 0,
                state: 0, x: primaryX, z: 100, team: 2);
            SetRow(snapshot.Rows, 20, objectId: 100, dataType: 1,
                state: pickupState, x: pickupX, z: 0, team: 0);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.OccupancyEpoch = epoch;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1000;
            snapshot.Input.Unk400 = -1000;
            snapshot.World.Difficulty = 1;
            snapshot.World.InputPhase = 0;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = -100;
            snapshot.World.StageZMax = 100;
            return snapshot;
        }

        private static void SetRow(
            AiSensingSnapshot rows,
            int slot,
            int objectId,
            int dataType,
            int state,
            int x,
            int z,
            int team)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = 1;
            rows.Identity[slot] = 1000 + slot;
            rows.ObjectId[slot] = objectId;
            rows.DataObjectType[slot] = dataType;
            rows.State[slot] = state;
            rows.X[slot] = x;
            rows.Y[slot] = 0;
            rows.Z[slot] = z;
            rows.Vx[slot] = 0.0;
            rows.Hp[slot] = 500;
            rows.Hp3[slot] = 500;
            rows.HpMax[slot] = 500;
            rows.Pp[slot] = 300;
            rows.Team[slot] = team;
            rows.CachedTargetSlot[slot] = -1;
            rows.CoordinateTargetX[slot] = -1000;
        }

        private static void AssertSites(
            AiDecisionSnapshot snapshot,
            in AiDecisionWitness witness,
            params uint[] expected)
        {
            Assert.That(witness.RngDrawCount, Is.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(snapshot.RngTraceCallSites[index],
                    Is.EqualTo(expected[index]));
            }
        }
    }
}
