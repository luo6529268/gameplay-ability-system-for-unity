#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsPositiveLinkValidationPassEditorTests
    {
        [Test]
        public void CompatibilityEntry_IsObsoleteNoOpAndEmitsNoStructuralEvent()
        {
            MethodInfo entry = typeof(SimulationWorld).GetMethod(
                nameof(SimulationWorld.ValidateHeldLinksAll));
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.GetCustomAttribute<ObsoleteAttribute>(), Is.Not.Null);

            var world = new SimulationWorld();
            LF2Character holder = Register(world, 0, 100);
            LF2Character target = Register(world, 1, 101);
            holder.Runtime.LinkState = 3;
            holder.Runtime.TargetSlotIndex = 1;
            holder.Runtime.HeldWeaponStableId = 1;
            target.Runtime.HolderStableId = 77;
            var events = new BattleParityStructuralEventBuffer(16);
            world.SetStructuralEventSinkForDiagnostics(events, 0, "fixture-setup");

#pragma warning disable CS0618
            world.ValidateHeldLinksAll(1);
#pragma warning restore CS0618

            Assert.That(holder.Runtime.LinkState, Is.EqualTo(3));
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(1));
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(1));
            Assert.That(target.Runtime.HolderStableId, Is.EqualTo(77));
            Assert.That(events.Events, Is.Empty);
        }

        [Test]
        public void PositiveLinkIndex_StillTracksRelationWritesAndLifecycleRelease()
        {
            var world = new SimulationWorld();
            LF2Character released = Register(world, 8, 180);

            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.Zero);
            released.Runtime.LinkState = 2;
            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.EqualTo(1));
            released.Runtime.LinkState = 0;
            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.Zero);
            released.Runtime.LinkState = 1;
            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.EqualTo(1));

            world.Unregister(released);

            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.Zero);
            LF2Character replacement = Register(world, 8, 181);
            Assert.That(replacement.Runtime.LinkState, Is.Zero);
            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void WarmedCompatibilityEntry_DoesNotAllocate()
        {
            var world = new SimulationWorld();
#pragma warning disable CS0618
            world.ValidateHeldLinksAll(1);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
                world.ValidateHeldLinksAll(index + 2);
#pragma warning restore CS0618
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
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
