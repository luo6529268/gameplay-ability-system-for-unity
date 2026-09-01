using System;
using System.Linq;
using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Tests.Editor
{
    [Category("RuntimeSlotLifecycleSeam")]
    public sealed class RuntimeSlotLifecycleSeamEditorTests
    {
        [Test]
        public void Authority400_ProvisionalClaimCommitsCanonicalEpochExactlyOnce()
        {
            var state = new RuntimeSlotLifecycleState(400, 20, 50);

            Assert.That(state.PeekLowest(0, 400), Is.Zero);
            Assert.That(
                state.TryBeginAllocateLowest(0, out RuntimeSlotAllocationTicket ticket),
                Is.True);
            Assert.That(ticket.Handle.Slot, Is.Zero);
            Assert.That(ticket.Handle.Generation, Is.EqualTo(1u));
            Assert.That(state.GetAllocationEpoch(0), Is.Zero);
            Assert.That(state.IsCommitted(0), Is.False);
            Assert.That(state.TryCommit(ticket, out _), Is.False);

            Assert.That(state.TryCompleteRequiredSideEffect(ticket, true), Is.True);
            Assert.That(
                state.TryCommit(ticket, out RuntimeSlotAllocationIdentity identity),
                Is.True);
            Assert.That(identity.Slot, Is.Zero);
            Assert.That(identity.AllocationEpoch, Is.EqualTo(1UL));
            Assert.That(state.GetAllocationEpoch(0), Is.EqualTo(1UL));
            Assert.That(state.IsCommitted(0), Is.True);
            Assert.That(state.TryCommit(ticket, out _), Is.False);

            Assert.That(state.Release(ticket.Handle), Is.True);
            Assert.That(state.GetAllocationEpoch(0), Is.EqualTo(1UL));
            Assert.That(state.IsClaimed(0), Is.False);
            Assert.That(
                state.TryBeginAllocateLowest(0, out RuntimeSlotAllocationTicket reused),
                Is.True);
            Assert.That(reused.Handle.Slot, Is.Zero);
            Assert.That(reused.Handle.Generation, Is.Not.EqualTo(ticket.Handle.Generation));
            Assert.That(state.TryCompleteRequiredSideEffect(reused, true), Is.True);
            Assert.That(state.TryCommit(reused, out RuntimeSlotAllocationIdentity reusedIdentity), Is.True);
            Assert.That(reusedIdentity.AllocationEpoch, Is.EqualTo(2UL));
        }

        [Test]
        public void FailedRequiredSideEffectAndRollback_DoNotAdvanceAllocationEpoch()
        {
            var state = new RuntimeSlotLifecycleState(400, 20, 50);

            Assert.That(
                state.TryBeginClaimRequired(20, out RuntimeSlotAllocationTicket rejected),
                Is.True);
            Assert.That(state.TryCompleteRequiredSideEffect(rejected, false), Is.False);
            Assert.That(state.TryCompleteRequiredSideEffect(rejected, true), Is.False);
            Assert.That(state.TryCommit(rejected, out _), Is.False);
            Assert.That(state.TryRollback(rejected), Is.True);
            Assert.That(state.GetAllocationEpoch(20), Is.Zero);
            Assert.That(state.IsClaimed(20), Is.False);
            Assert.That(state.Release(rejected.Handle), Is.False);

            Assert.That(
                state.TryBeginClaimRequired(20, out RuntimeSlotAllocationTicket accepted),
                Is.True);
            Assert.That(accepted.Handle.Generation, Is.Not.EqualTo(rejected.Handle.Generation));
            Assert.That(state.TryCompleteRequiredSideEffect(accepted, true), Is.True);
            Assert.That(state.TryCommit(accepted, out RuntimeSlotAllocationIdentity identity), Is.True);
            Assert.That(identity.AllocationEpoch, Is.EqualTo(1UL));

            Assert.That(
                state.TryBeginAllocateLowest(50, out RuntimeSlotAllocationTicket dynamic),
                Is.True);
            Assert.That(dynamic.Handle.Slot, Is.EqualTo(50));
            Assert.That(state.TryRollback(dynamic), Is.True);
            Assert.That(state.GetAllocationEpoch(50), Is.Zero);

            state.ResetFreshWorld();
            Assert.That(state.IsClaimed(20), Is.False);
            Assert.That(state.GetAllocationEpoch(20), Is.Zero);
            Assert.That(state.PeekLowest(20, 400), Is.EqualTo(20));
        }

        [Test]
        public void RuntimeSlotTable_PreservesGenerationWhileExposingCanonicalEpoch()
        {
            var table = new RuntimeSlotTable(400, 20, 50);
            var first = new LF2Character();
            var replacement = new LF2Character();

            Assert.That(table.TryClaim(50, first, out RuntimeEntityHandle firstHandle), Is.True);
            RuntimeSlotTable.ReadOnlySlotView firstView = table.GetReadOnlyView(50);
            Assert.That(firstView.Generation, Is.EqualTo(firstHandle.Generation));
            Assert.That(firstView.AllocationEpoch, Is.EqualTo(1UL));

            Assert.That(table.Release(firstHandle), Is.True);
            Assert.That(table.GetReadOnlyView(50).AllocationEpoch, Is.EqualTo(1UL));
            Assert.That(table.AllocateLowest(50, replacement, out RuntimeEntityHandle reused), Is.EqualTo(50));
            Assert.That(reused.Generation, Is.Not.EqualTo(firstHandle.Generation));
            Assert.That(table.GetReadOnlyView(50).AllocationEpoch, Is.EqualTo(2UL));
            Assert.That(table.TryResolve(firstHandle, out _), Is.False);
            Assert.That(table.TryResolve(reused, out LF2Entity resolved), Is.True);
            Assert.That(resolved, Is.SameAs(replacement));
        }

        [Test]
        public void StructuralBuffer_PreservesCanonicalEpochInsteadOfDerivingAnotherCounter()
        {
            var buffer = new BattleParityStructuralEventBuffer(400);
            buffer.Record(new BattleParityStructuralEvent
            {
                Tick = 1,
                Action = "allocate",
                Slot = 20,
                LifecycleEpoch = 7,
            });
            buffer.Record(new BattleParityStructuralEvent
            {
                Tick = 2,
                Action = "free",
                Slot = 20,
                LifecycleEpoch = 7,
            });

            Assert.That(buffer.Events.Select(value => value.LifecycleEpoch),
                Is.EqualTo(new ulong[] { 7, 7 }));
        }

        [Test]
        public void WarmedClaimCommitReleaseLoop_AllocatesZeroBytes()
        {
            var state = new RuntimeSlotLifecycleState(400, 20, 50);
            RunLifecycleCycles(state, 4);

            long before = GC.GetAllocatedBytesForCurrentThread();
            RunLifecycleCycles(state, 1024);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(state.GetAllocationEpoch(50), Is.EqualTo(1028UL));
        }

        private static void RunLifecycleCycles(
            RuntimeSlotLifecycleState state,
            int count)
        {
            for (int index = 0; index < count; index++)
            {
                if (!state.TryBeginAllocateLowest(
                        50,
                        out RuntimeSlotAllocationTicket ticket) ||
                    ticket.Handle.Slot != 50 ||
                    !state.TryCompleteRequiredSideEffect(ticket, true) ||
                    !state.TryCommit(ticket, out _) ||
                    !state.Release(ticket.Handle))
                {
                    throw new InvalidOperationException(
                        "Warmed slot lifecycle cycle failed.");
                }
            }
        }
    }
}
