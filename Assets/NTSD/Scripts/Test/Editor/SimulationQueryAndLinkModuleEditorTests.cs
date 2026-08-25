#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class SimulationQueryAndLinkModuleEditorTests
    {
        [Test]
        public void HeldObjectProcess_OutOfRangeNegativeHolderRetainsHolderSlotAcrossBothPasses()
        {
            var world = new SimulationWorld();
            LF2Character child = Register(world, 20, 300);
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = 400;

            world.HeldObjectProcessAll(1);
            Assert.That(child.Runtime.LinkState, Is.Zero);
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(400));

            world.HeldObjectProcessAll(2);
            Assert.That(child.Runtime.LinkState, Is.Zero);
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(400));
        }

        [Test]
        public void HeldObjectProcess_ActiveHolderMismatchPreservesBothRelationFields()
        {
            var world = new SimulationWorld();
            LF2Character holder = Register(world, 30, 301);
            LF2Character child = Register(world, 31, 302);
            holder.Runtime.TargetSlotIndex = 32;
            child.Runtime.LinkState = -2;
            child.Runtime.HolderStableId = 30;

            world.HeldObjectProcessAll(1);

            Assert.That(child.Runtime.LinkState, Is.Zero);
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(30));
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(32));
        }

        private static LF2Character Register(
            SimulationWorld world,
            int slot,
            int stableId)
        {
            var entity = new LF2Character();
            entity.Runtime.StableId = stableId;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(slot));
            return entity;
        }
    }
}
#endif
