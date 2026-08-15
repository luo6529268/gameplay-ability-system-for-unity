using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class RoleAwareFormalParticipantBufferEditorTests
    {
        [Test]
        public void Rebuild_OverwritesCanonicalPrefixAndClearsRemovedTail()
        {
            var buffer = new RoleAwareFormalParticipantBuffer(2);
            var first = new LF2Character();
            var second = new LF2Character();

            buffer.BeginBuild();
            Add(buffer, first, 0);
            Add(buffer, second, 1);
            buffer.CompleteBuild();

            buffer.BeginBuild();
            Add(buffer, second, 1);
            buffer.CompleteBuild();

            Assert.That(buffer.Count, Is.EqualTo(1));
            Assert.That(buffer[0].Entity, Is.SameAs(second));
        }

        [Test]
        public void RefIndexer_UpdatesTheStoredCanonicalParticipant()
        {
            var buffer = new RoleAwareFormalParticipantBuffer(1);
            var entity = new LF2Character();
            buffer.BeginBuild();
            Add(buffer, entity, 3);
            buffer.CompleteBuild();

            ref RoleAwareFormalParticipant participant = ref buffer[0];
            participant.HasAttackItr = true;

            Assert.That(buffer[0].HasAttackItr, Is.True);
            Assert.That(buffer[0].Handle.Slot, Is.EqualTo(3));
        }

        [Test]
        public void WarmedSameSizeRebuild_AllocatesNoManagedMemory()
        {
            var buffer = new RoleAwareFormalParticipantBuffer(4);
            var entity = new LF2Character();
            var participant = CreateParticipant(entity, 0);

            Rebuild(buffer, in participant);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 4096; iteration++)
                Rebuild(buffer, in participant);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static void Rebuild(
            RoleAwareFormalParticipantBuffer buffer,
            in RoleAwareFormalParticipant participant)
        {
            buffer.BeginBuild();
            buffer.Add(in participant);
            buffer.CompleteBuild();
        }

        private static void Add(
            RoleAwareFormalParticipantBuffer buffer,
            LF2Entity entity,
            int slot)
        {
            RoleAwareFormalParticipant participant = CreateParticipant(entity, slot);
            buffer.Add(in participant);
        }

        private static RoleAwareFormalParticipant CreateParticipant(
            LF2Entity entity,
            int slot)
        {
            return new RoleAwareFormalParticipant(
                entity,
                null,
                null,
                new RuntimeEntityHandle(slot, 1));
        }
    }
}
