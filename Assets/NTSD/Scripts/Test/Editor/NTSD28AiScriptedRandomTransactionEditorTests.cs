using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28AiScriptedRandomTransactionEditorTests
    {
        [Test]
        public void ScriptedLeft_ConsumesOnlySite11AndReturnsCandidate()
        {
            var owner = new NTSD28NativeRandom(0x12345678u);
            var oracle = new NTSD28NativeRandom(0x12345678u);
            AiDecisionSnapshot snapshot = CreateScriptedSnapshot(700, 400);
            snapshot.SetSynchronizedRngCursor(
                owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);

            Assert.That(witness.Exit, Is.EqualTo(AiDecisionExit.Coordinate));
            Assert.That(witness.Input.KeyLeft, Is.EqualTo(1));
            Assert.That(snapshot.RngTraceCallSites[0], Is.EqualTo(0x11u));
            Assert.That(snapshot.RngTraceModuli[0], Is.EqualTo(9));
            Assert.That(witness.RngDrawCount, Is.EqualTo(1));
            Assert.That(witness.TryGetSynchronizedRngCursor(
                out NTSD28SynchronizedRandomCursor candidate), Is.True);
            Assert.That(candidate.Next(0x20u, 17),
                Is.EqualTo(AdvanceOracleAndNext(oracle, 0x11u, 9, 0x20u, 17)));
            Assert.That(owner.CaptureScalarState().SynchronizedCalls, Is.Zero);
        }

        [Test]
        public void ScriptedRight_ConsumesOnlySite12AndCommitsExplicitly()
        {
            var owner = new NTSD28NativeRandom(0x2468ACE0u);
            var oracle = new NTSD28NativeRandom(0x2468ACE0u);
            AiDecisionSnapshot snapshot = CreateScriptedSnapshot(0, 400);
            snapshot.SetSynchronizedRngCursor(
                owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);
            Assert.That(snapshot.RngTraceCallSites[0], Is.EqualTo(0x12u));
            Assert.That(witness.TryGetSynchronizedRngCursor(
                out NTSD28SynchronizedRandomCursor candidate), Is.True);
            Assert.That(owner.TryCommitSynchronizedCursor(candidate), Is.True);

            oracle.SynchronizedNext(0x12u, 9);
            NTSD28NativeRandomScalarState actual = owner.CaptureScalarState();
            NTSD28NativeRandomScalarState expected = oracle.CaptureScalarState();
            Assert.That(actual.SynchronizedCounter,
                Is.EqualTo(expected.SynchronizedCounter));
            Assert.That(actual.SynchronizedIndex,
                Is.EqualTo(expected.SynchronizedIndex));
            Assert.That(actual.SynchronizedCalls,
                Is.EqualTo(expected.SynchronizedCalls));
            Assert.That(actual.LastSynchronizedCallSite, Is.EqualTo(0x12u));
        }

        [Test]
        public void ScriptedNearTarget_DoesNotConsumeCandidate()
        {
            var owner = new NTSD28NativeRandom(31u);
            AiDecisionSnapshot snapshot = CreateScriptedSnapshot(500, 400);
            snapshot.SetSynchronizedRngCursor(
                owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);
            Assert.That(witness.RngDrawCount, Is.Zero);
            Assert.That(witness.TryGetSynchronizedRngCursor(
                out NTSD28SynchronizedRandomCursor candidate), Is.True);
            Assert.That(candidate.Calls, Is.Zero);
            Assert.That(owner.TryCommitSynchronizedCursor(candidate), Is.True);
            Assert.That(owner.CaptureScalarState().SynchronizedCalls, Is.Zero);
        }

        [Test]
        public void SnapshotCopyAndReset_PreserveThenClearCandidate()
        {
            var owner = new NTSD28NativeRandom(41u);
            AiDecisionSnapshot source = CreateScriptedSnapshot(700, 400);
            source.SetSynchronizedRngCursor(
                owner.CaptureSynchronizedCursor());
            AiDecisionSnapshot copy = CreateScriptedSnapshot(0, 400);

            copy.CopyOwnedFrom(source);

            Assert.That(copy.HasSynchronizedRngCursor, Is.True);
            Assert.That(copy.TryGetSynchronizedRngCursor(out _), Is.True);
            copy.ResetOwned(9UL);
            Assert.That(copy.HasSynchronizedRngCursor, Is.False);
            Assert.That(copy.TryGetSynchronizedRngCursor(out _), Is.False);
        }

        [Test]
        public void OwnerReset_RejectsStaleWitnessCandidate()
        {
            var owner = new NTSD28NativeRandom(43u);
            AiDecisionSnapshot snapshot = CreateScriptedSnapshot(700, 400);
            snapshot.SetSynchronizedRngCursor(
                owner.CaptureSynchronizedCursor());
            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness),
                Is.True);
            Assert.That(witness.TryGetSynchronizedRngCursor(
                out NTSD28SynchronizedRandomCursor candidate), Is.True);

            owner.ResetFromSeed(47u);

            Assert.That(owner.TryCommitSynchronizedCursor(candidate), Is.False);
            Assert.That(owner.CaptureScalarState().SynchronizedCalls, Is.Zero);
        }

        [Test]
        public void WarmScriptedEvaluationCommit_AllocatesZeroManagedBytes()
        {
            var owner = new NTSD28NativeRandom(0x55667788u);
            AiDecisionSnapshot snapshot = CreateScriptedSnapshot(700, 400);
            AiDecisionWitness witness = default;
            for (int index = 0; index < 16; index++)
            {
                snapshot.SetSynchronizedRngCursor(
                    owner.CaptureSynchronizedCursor());
                Assert.That(AiDecisionKernel.TryEvaluate(
                    snapshot,
                    AiDecisionEvaluationPolicy.FullScan,
                    false,
                    ref witness), Is.True);
                Assert.That(witness.TryGetSynchronizedRngCursor(
                    out NTSD28SynchronizedRandomCursor warm), Is.True);
                Assert.That(owner.TryCommitSynchronizedCursor(warm), Is.True);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool passed = true;
            for (int index = 0; index < 4096; index++)
            {
                snapshot.SetSynchronizedRngCursor(
                    owner.CaptureSynchronizedCursor());
                passed &= AiDecisionKernel.TryEvaluate(
                    snapshot,
                    AiDecisionEvaluationPolicy.FullScan,
                    false,
                    ref witness);
                passed &= witness.TryGetSynchronizedRngCursor(
                    out NTSD28SynchronizedRandomCursor candidate);
                passed &= owner.TryCommitSynchronizedCursor(candidate);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(passed, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static int AdvanceOracleAndNext(
            NTSD28NativeRandom oracle,
            uint firstSite,
            int firstBound,
            uint secondSite,
            int secondBound)
        {
            oracle.SynchronizedNext(firstSite, firstBound);
            return oracle.SynchronizedNext(secondSite, secondBound);
        }

        private static AiDecisionSnapshot CreateScriptedSnapshot(
            int x,
            int targetX)
        {
            const ulong epoch = 7UL;
            var snapshot = new AiDecisionSnapshot(1);
            snapshot.Reset(epoch);
            snapshot.Rows.Included[0] = true;
            snapshot.Rows.Generation[0] = 1;
            snapshot.Rows.Identity[0] = 1000;
            snapshot.Rows.ObjectId[0] = 1;
            snapshot.Rows.DataObjectType[0] = 0;
            snapshot.Rows.Hp[0] = 500;
            snapshot.Rows.X[0] = x;
            snapshot.Rows.Z[0] = 0;
            snapshot.Rows.State[0] = 2;
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.OccupancyEpoch = epoch;
            snapshot.Input.Unk3FC = targetX;
            snapshot.Input.Unk400 = 0;
            snapshot.World.FlowRand3 = 6;
            return snapshot;
        }
    }
}
