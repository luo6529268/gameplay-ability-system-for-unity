using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28AiTargetPrefixRandomEditorTests
    {
        [Test]
        public void CachedActiveNonCharacter_Consumes13BeforeTypeRejectThen14()
        {
            var owner = CreateOwnerWithTable(1, 1);
            AiDecisionSnapshot snapshot = CreateSnapshot(3, selfState: 2);
            SetRow(snapshot, 1, objectId: 200, dataType: 0, state: 0,
                x: 50, team: 2, vx: 0.0);
            SetRow(snapshot, 2, objectId: 100, dataType: 1, state: 0,
                x: 30, team: 2, vx: 0.0);
            snapshot.Input.Unk360 = 2;
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            Assert.That(witness.FinalSelectedSlot, Is.EqualTo(1));
            Assert.That(witness.Input.Unk360, Is.EqualTo(1));
            AssertSites(snapshot, witness, 0x13u, 0x14u);
        }

        [Test]
        public void Common14_DoesNotTreatBoundaryFlagsAsNativeForceAttack()
        {
            var owner = CreateOwnerWithTable(12);
            AiDecisionSnapshot snapshot = CreateSnapshot(2, selfState: 2);
            SetRow(snapshot, 1, objectId: 200, dataType: 0, state: 0,
                x: 50, team: 2, vx: 0.0);
            snapshot.Input.BoundaryFlags = 1;
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            AssertSites(snapshot, witness, 0x14u);
            Assert.That(witness.Input.KeyJump, Is.Zero,
                "legacy KeyJump is native attack and must not be armed by boundary flags");
        }

        [Test]
        public void State3000_SubjectState7_Consumes14ButNot15()
        {
            var owner = CreateOwnerWithTable(1, 1);
            AiDecisionSnapshot snapshot = CreateSnapshot(2, selfState: 7);
            SetRow(snapshot, 1, objectId: 300, dataType: 1, state: 3000,
                x: 100, team: 2, vx: -1.0);
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            AssertSites(snapshot, witness, 0x14u);
            Assert.That(witness.Input.KeyAttack, Is.Zero);
        }

        [Test]
        public void State3000_Consumes15After14AndRaisesNativeDefend()
        {
            var owner = CreateOwnerWithTable(1, 1);
            AiDecisionSnapshot snapshot = CreateSnapshot(2, selfState: 2);
            SetRow(snapshot, 1, objectId: 300, dataType: 1, state: 3000,
                x: 100, team: 2, vx: -1.0);
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            AssertSites(snapshot, witness, 0x14u, 0x15u);
            Assert.That(witness.Input.KeyAttack, Is.EqualTo(1),
                "legacy KeyAttack maps to native defend");
        }

        [TestCase(300, 0x18u)]
        [TestCase(100, 0x19u)]
        public void AbnormalTarget_ConsumesDirectionalSiteAfter14(
            int targetX,
            uint expectedSite)
        {
            var owner = CreateOwnerWithTable(1, 1);
            AiDecisionSnapshot snapshot = CreateSnapshot(2, selfState: 2);
            snapshot.Rows.X[0] = 200;
            SetRow(snapshot, 1, objectId: 301, dataType: 0, state: 14,
                x: targetX, team: 2, vx: 0.0);
            snapshot.World.StageZMin = -200;
            snapshot.World.StageZMax = 200;
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            AssertSites(snapshot, witness, 0x14u, expectedSite);
        }

        [Test]
        public void WarmCommonPrefixEvaluationCommit_AllocatesZeroManagedBytes()
        {
            var owner = new NTSD28NativeRandom(0x55667788u);
            AiDecisionSnapshot snapshot = CreateSnapshot(2, selfState: 2);
            SetRow(snapshot, 1, objectId: 200, dataType: 0, state: 0,
                x: 50, team: 2, vx: 0.0);
            AiDecisionWitness witness = default;
            for (int index = 0; index < 16; index++)
            {
                EvaluateAndCommit(owner, snapshot, ref witness);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool passed = true;
            for (int index = 0; index < 4096; index++)
            {
                passed &= EvaluateAndCommit(owner, snapshot, ref witness);
            }
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

        private static NTSD28NativeRandom CreateOwnerWithTable(
            params byte[] leadingValues)
        {
            var state = new NTSD28SynchronizedRandomState();
            for (int index = 0; index < leadingValues.Length; index++)
            {
                state.Table[index + 1] = leadingValues[index];
            }
            var owner = new NTSD28NativeRandom(1u);
            owner.RestoreSynchronized(state);
            return owner;
        }

        private static AiDecisionSnapshot CreateSnapshot(
            int capacity,
            int selfState)
        {
            const ulong epoch = 17UL;
            var snapshot = new AiDecisionSnapshot(capacity);
            snapshot.Reset(epoch);
            SetRow(snapshot, 0, objectId: 1, dataType: 0, state: selfState,
                x: 0, team: 1, vx: 0.0);
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
            AiDecisionSnapshot snapshot,
            int slot,
            int objectId,
            int dataType,
            int state,
            int x,
            int team,
            double vx)
        {
            AiSensingSnapshot rows = snapshot.Rows;
            rows.Included[slot] = true;
            rows.Generation[slot] = 1;
            rows.Identity[slot] = 1000 + slot;
            rows.ObjectId[slot] = objectId;
            rows.DataObjectType[slot] = dataType;
            rows.State[slot] = state;
            rows.X[slot] = x;
            rows.Y[slot] = 0;
            rows.Z[slot] = 0;
            rows.Vx[slot] = vx;
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
                    Is.EqualTo(expected[index]),
                    $"site[{index}]");
            }
        }
    }
}
