#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06HeldQueryRelationEditorTests
    {
        [TestCase(-1)]
        [TestCase(0)]
        public void NonHolderQueriesPreserveNativeRelationship(int link)
        {
            var world = new SimulationWorld();
            var parent = new LF2Character { ObjectId = 77 };
            var subject = new LF2Character { ObjectId = 78 };
            try
            {
                world.Register(parent);
                world.Register(subject);
                subject.Runtime.LinkState = link;
                subject.Runtime.TargetSlotIndex = 0;
                subject.Runtime.HolderStableId = parent.Runtime.SlotIndex;
                subject.HeldWeaponReferenceInternal = parent;
                Assert.That(subject.GetHeldWeapon(), Is.Null);
                Assert.That(subject.HeldWeaponReferenceInternal, Is.Null);
                AssertRelationship(subject, link, 0, parent.Runtime.SlotIndex);
                subject.RunWeaponSyncHeldStep10();
                AssertRelationship(subject, link, 0, parent.Runtime.SlotIndex);
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void PositiveReciprocalHolderStillResolvesChild()
        {
            var world = new SimulationWorld();
            var parent = new LF2Character { ObjectId = 77 };
            var child = new LF2Character { ObjectId = 78 };
            try
            {
                world.Register(parent);
                world.Register(child);
                parent.Runtime.LinkState = 1;
                parent.Runtime.TargetSlotIndex = child.Runtime.SlotIndex;
                child.Runtime.LinkState = -1;
                child.Runtime.HolderStableId = parent.Runtime.SlotIndex;
                Assert.That(parent.GetHeldWeapon(), Is.SameAs(child));
                parent.RunWeaponSyncHeldStep10();
                Assert.That(parent.GetHeldWeapon(), Is.SameAs(child));
                Assert.That(parent.Runtime.LinkState, Is.EqualTo(1));
                Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void PositiveInvalidTargetKeepsExistingStaleHandling()
        {
            var world = new SimulationWorld();
            var parent = new LF2Character { ObjectId = 77 };
            try
            {
                world.Register(parent);
                parent.Runtime.LinkState = 1;
                parent.Runtime.TargetSlotIndex = 399;
                Assert.That(parent.GetHeldWeapon(), Is.Null);
                Assert.That(parent.Runtime.LinkState, Is.Zero);
                Assert.That(parent.Runtime.TargetSlotIndex, Is.EqualTo(-1));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void SlotReuseDoesNotInheritManagedHeldCache()
        {
            var world = new SimulationWorld();
            var holder = new LF2Character { ObjectId = 77 };
            var oldChild = new LF2OtherObject { ObjectId = 78 };
            var newborn = new LF2OtherObject { ObjectId = 79 };
            try
            {
                holder.SetRequiredRuntimeSlot(0);
                oldChild.SetRequiredRuntimeSlot(50);
                newborn.SetRequiredRuntimeSlot(50);
                world.Register(holder);
                world.Register(oldChild);
                holder.Runtime.LinkState = 1;
                holder.Runtime.TargetSlotIndex = 50;
                holder.Runtime.HeldWeaponStableId = 50;
                holder.HeldWeaponReferenceInternal = oldChild;
                oldChild.Runtime.LinkState = -1;
                oldChild.Runtime.HolderStableId = 0;
                Assert.That(holder.GetHeldWeapon(), Is.SameAs(oldChild));
                world.Unregister(oldChild);
                Assert.That(holder.Runtime.TargetSlotIndex, Is.Zero);
                Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
                world.Register(newborn);
                Assert.That(holder.GetHeldWeapon(), Is.Null);
                Assert.That(holder.HeldWeaponReferenceInternal, Is.Null);
                Assert.That(holder.Runtime.LinkState, Is.Zero);
                Assert.That(holder.Runtime.TargetSlotIndex, Is.Zero);
                holder.RunWeaponSyncHeldStep10();
                Assert.That(holder.Runtime.TargetSlotIndex, Is.Zero);
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        private static void AssertRelationship(LF2Character entity, int link, int child, int parent)
        {
            Assert.That(entity.Runtime.LinkState, Is.EqualTo(link));
            Assert.That(entity.Runtime.TargetSlotIndex, Is.EqualTo(child));
            Assert.That(entity.Runtime.HolderStableId, Is.EqualTo(parent));
        }
    }
}
#endif
