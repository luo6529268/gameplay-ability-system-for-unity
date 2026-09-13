#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05GrabTrackerCarrierEditorTests
    {
        [TestCase(typeof(NTSDEntityRuntime), "GrabbedBy")]
        [TestCase(typeof(NTSDEntityRuntime), "TrackerFlag")]
        [TestCase(typeof(LF2Entity), "GrabbedBy")]
        [TestCase(typeof(LF2Entity), "TrackerFlag")]
        [TestCase(typeof(BattleEcsLinkStore), "GrabbedBy")]
        [TestCase(typeof(BattleEcsLinkStore), "TrackerFlag")]
        public void RetiredFlagHasNoRuntimeOrDerivedCarrier(Type type, string name)
        {
            Assert.That(type.GetMember(name, BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic), Is.Empty, type.Name + "." + name);
        }

        [Test]
        public void ActualTrackerParentAndIndependentOwnersStillRoundTrip()
        {
            var world = new SimulationWorld();
            var parent = new LF2Character();
            var child = new LF2Character();
            parent.SetRequiredRuntimeSlot(3);
            child.SetRequiredRuntimeSlot(4);
            world.Register(parent);
            world.Register(child);
            child.TrackerParent = parent;
            child.Runtime.OwnerSlotIndex = 3;
            child.Runtime.SpawnerSlotIndex = 27;
            child.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = 31;
            var identity = NTSD.Test.StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            Assert.That(snapshot.EntityBaseShell.GetState(4).TrackerParentHandle.Slot, Is.EqualTo(3));
            child.TrackerParent = null;
            child.Runtime.OwnerSlotIndex = -1;
            child.Runtime.SpawnerSlotIndex = -1;
            child.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = -1;
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out BattleStateSnapshotRestoreFailure failure), Is.True, failure.ToString());
            Assert.That(child.TrackerParent, Is.SameAs(parent));
            Assert.That(child.Runtime.OwnerSlotIndex, Is.EqualTo(3));
            Assert.That(child.Runtime.SpawnerSlotIndex, Is.EqualTo(27));
            Assert.That(child.Runtime.ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(31));
        }
    }
}
#endif
