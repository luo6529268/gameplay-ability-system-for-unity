using System;
using NUnit.Framework;
using NTSD.Animation.LF2Objects;

namespace NTSD.Test.Editor
{
    public sealed class CollisionSnapshotRuntimeSyncEditorTests
    {
        [Test]
        public void ExactCharacter_CaptureWritesOnlyAuthoritativePrevFrame2Cluster()
        {
            var character = new LF2Character();
            character.Frame.N = 77;
            character.Frame.D = character.FrameCache.GetFrameDataById(77);
            character.Runtime.Frame = 77;

            character.CaptureCollisionFrameSnapshot();
            bool refreshed = character.RefreshRuntimeSnapshotAfterCollisionSnapshot();

            Assert.That(refreshed, Is.False);
            Assert.That(character.Frame.Prev2, Is.EqualTo(77));
            Assert.That(character.Frame.Prev2D, Is.SameAs(character.Frame.D));
            Assert.That(character.Runtime.PrevFrame2, Is.EqualTo(77));
        }

        [Test]
        public void UnknownDerivedCharacter_FallsBackToVirtualSnapshotRefresh()
        {
            var character = new DerivedCharacter();
            int refreshCountBefore = character.RefreshCount;
            character.Frame.N = 77;
            character.Runtime.Frame = -1;

            character.CaptureCollisionFrameSnapshot();
            bool refreshed = character.RefreshRuntimeSnapshotAfterCollisionSnapshot();

            Assert.That(refreshed, Is.True);
            Assert.That(character.RefreshCount, Is.EqualTo(refreshCountBefore + 1));
            Assert.That(character.Runtime.Frame, Is.EqualTo(77));
            Assert.That(character.Runtime.PrevFrame2, Is.EqualTo(77));
        }

        [Test]
        public void WarmedExactCharacterPath_AllocatesNoManagedMemory()
        {
            var character = new LF2Character();
            character.RefreshRuntimeSnapshotAfterCollisionSnapshot();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4096; i++)
                character.RefreshRuntimeSnapshotAfterCollisionSnapshot();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private sealed class DerivedCharacter : LF2Character
        {
            public int RefreshCount { get; private set; }

            protected override void RefreshRuntimeFromEntity()
            {
                RefreshCount++;
                base.RefreshRuntimeFromEntity();
            }
        }
    }
}
