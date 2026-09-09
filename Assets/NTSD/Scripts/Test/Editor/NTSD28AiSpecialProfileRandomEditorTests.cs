using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28AiSpecialProfileRandomEditorTests
    {
        [Test]
        public void GatePositive_ConsumesOnly3CAndContinuesOrdinary()
        {
            var owner = CreateOwnerWithTable(1);
            AiSensingSnapshot rows = CreateRows(selfOid: 19, targetState: 0);
            AiDecisionInputState input = default;
            var sites = new uint[4];
            var rng = CreateStream(owner, sites);

            bool stopped = AiDecisionKernel.TryApplyNativeSpecialProfile(
                rows, 0, 1, aiRand5: 5, ref input, ref rng);

            Assert.That(stopped, Is.False);
            Assert.That(rng.DrawCount, Is.EqualTo(1));
            Assert.That(sites[0], Is.EqualTo(0x3Cu));
            Assert.That(input.ComboDua, Is.Zero);
        }

        [Test]
        public void GateZeroNonOid33_DoesNotConsume6C()
        {
            var owner = CreateOwnerWithTable(5);
            AiSensingSnapshot rows = CreateRows(selfOid: 19, targetState: 0);
            AiDecisionInputState input = default;
            var sites = new uint[4];
            var rng = CreateStream(owner, sites);

            bool stopped = AiDecisionKernel.TryApplyNativeSpecialProfile(
                rows, 0, 1, aiRand5: 5, ref input, ref rng);

            Assert.That(stopped, Is.False);
            Assert.That(rng.DrawCount, Is.EqualTo(1));
            Assert.That(sites[0], Is.EqualTo(0x3Cu));
        }

        [Test]
        public void Oid33ZeroGates_Consume3C6CAndArmComboIndex2()
        {
            var owner = CreateOwnerWithTable(5, 3);
            AiSensingSnapshot rows = CreateRows(selfOid: 33, targetState: 0);
            AiDecisionInputState input = default;
            var sites = new uint[4];
            var rng = CreateStream(owner, sites);

            bool stopped = AiDecisionKernel.TryApplyNativeSpecialProfile(
                rows, 0, 1, aiRand5: 5, ref input, ref rng);

            Assert.That(stopped, Is.True);
            Assert.That(rng.DrawCount, Is.EqualTo(2));
            Assert.That(sites[0], Is.EqualTo(0x3Cu));
            Assert.That(sites[1], Is.EqualTo(0x6Cu));
            Assert.That(input.ComboDua, Is.EqualTo(3),
                "router combo index2 is hit_Ua/legacy ComboDua");
        }

        [Test]
        public void TargetState16_AllowsComboEvenWhen6CIsNonzero()
        {
            var owner = CreateOwnerWithTable(5, 1);
            AiSensingSnapshot rows = CreateRows(selfOid: 33, targetState: 16);
            AiDecisionInputState input = default;
            var rng = CreateStream(owner, new uint[4]);

            Assert.That(AiDecisionKernel.TryApplyNativeSpecialProfile(
                rows, 0, 1, aiRand5: 5, ref input, ref rng), Is.True);
            Assert.That(input.ComboDua, Is.EqualTo(3));
        }

        [Test]
        public void FullCandidateTrace_Is14Then3C6CWithoutLegacySurplus()
        {
            var owner = CreateOwnerWithTable(1, 4, 2);
            AiDecisionSnapshot snapshot = CreateFullSnapshot();
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            Assert.That(witness.Exit, Is.EqualTo(AiDecisionExit.FirstDecision));
            Assert.That(witness.Input.ComboDua, Is.EqualTo(3));
            Assert.That(witness.RngDrawCount, Is.EqualTo(3));
            Assert.That(snapshot.RngTraceCallSites[0], Is.EqualTo(0x14u));
            Assert.That(snapshot.RngTraceCallSites[1], Is.EqualTo(0x3Cu));
            Assert.That(snapshot.RngTraceCallSites[2], Is.EqualTo(0x6Cu));
        }

        [Test]
        public void WarmNativeSpecialProfile_AllocatesZeroManagedBytes()
        {
            var owner = new NTSD28NativeRandom(0x55667788u);
            AiSensingSnapshot rows = CreateRows(selfOid: 33, targetState: 16);
            AiDecisionInputState input = default;
            for (int index = 0; index < 16; index++)
                EvaluateAndCommit(owner, rows, ref input);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool committed = true;
            for (int index = 0; index < 4096; index++)
                committed &= EvaluateAndCommit(owner, rows, ref input);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(committed, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static bool EvaluateAndCommit(
            NTSD28NativeRandom owner,
            AiSensingSnapshot rows,
            ref AiDecisionInputState input)
        {
            var rng = new AiDecisionRandomStream(
                owner.CaptureSynchronizedCursor());
            AiDecisionKernel.TryApplyNativeSpecialProfile(
                rows, 0, 1, aiRand5: 5, ref input, ref rng);
            return owner.TryCommitSynchronizedCursor(
                rng.CaptureSynchronizedCursor());
        }

        private static AiDecisionRandomStream CreateStream(
            NTSD28NativeRandom owner,
            uint[] sites)
        {
            return new AiDecisionRandomStream(
                owner.CaptureSynchronizedCursor(),
                captureTrace: true,
                callSites: sites,
                moduli: new int[sites.Length],
                rawValues: new int[sites.Length],
                values: new int[sites.Length]);
        }

        private static NTSD28NativeRandom CreateOwnerWithTable(
            params byte[] leadingValues)
        {
            var state = new NTSD28SynchronizedRandomState();
            for (int index = 0; index < leadingValues.Length; index++)
                state.Table[index + 1] = leadingValues[index];
            var owner = new NTSD28NativeRandom(1u);
            owner.RestoreSynchronized(state);
            return owner;
        }

        private static AiSensingSnapshot CreateRows(
            int selfOid,
            int targetState)
        {
            var rows = new AiSensingSnapshot(2);
            rows.Reset(31UL);
            SetRow(rows, 0, selfOid, 0, 0, 300, 0);
            SetRow(rows, 1, 2, targetState, 50, 300, 0);
            return rows;
        }

        private static AiDecisionSnapshot CreateFullSnapshot()
        {
            const ulong epoch = 37UL;
            var snapshot = new AiDecisionSnapshot(2);
            snapshot.Reset(epoch);
            SetRow(snapshot.Rows, 0, 33, 0, 0, 300, 1);
            SetRow(snapshot.Rows, 1, 2, 0, 50, 300, 2);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.OccupancyEpoch = epoch;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1000;
            snapshot.Input.Unk400 = -1000;
            snapshot.World.Difficulty = 1;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = -100;
            snapshot.World.StageZMax = 100;
            return snapshot;
        }

        private static void SetRow(
            AiSensingSnapshot rows,
            int slot,
            int oid,
            int state,
            int x,
            int pp,
            int team)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = 1;
            rows.Identity[slot] = 1000 + slot;
            rows.ObjectId[slot] = oid;
            rows.DataObjectType[slot] = 0;
            rows.State[slot] = state;
            rows.X[slot] = x;
            rows.Y[slot] = 0;
            rows.Z[slot] = 0;
            rows.Vx[slot] = 0.0;
            rows.Facing[slot] = 0;
            rows.Hp[slot] = 500;
            rows.Hp3[slot] = 500;
            rows.HpMax[slot] = 500;
            rows.Pp[slot] = pp;
            rows.Team[slot] = team;
            rows.CachedTargetSlot[slot] = -1;
            rows.CoordinateTargetX[slot] = -1000;
        }
    }
}
