using System;

using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the allocation-free live runtime-slot traversal used by battle passes.
    /// The world keeps one instance; each enumeration only creates value-type cursors.
    /// </summary>
    internal sealed class SimulationEntityTraversal
    {
        private readonly SimulationWorld world;
        private readonly RuntimeSlotTable runtimeSlots;

        internal SimulationEntityTraversal(
            SimulationWorld world,
            RuntimeSlotTable runtimeSlots)
        {
            this.world = world;
            this.runtimeSlots = runtimeSlots;
        }

        internal ActiveEntityEnumerable ActiveEntities =>
            new ActiveEntityEnumerable(world, runtimeSlots);

        internal DeferredMutationScope BeginDeferredMutation()
        {
            return new DeferredMutationScope(world);
        }

        internal readonly struct ActiveEntityEnumerable
        {
            private readonly SimulationWorld world;
            private readonly RuntimeSlotTable runtimeSlots;

            internal ActiveEntityEnumerable(
                SimulationWorld world,
                RuntimeSlotTable runtimeSlots)
            {
                this.world = world;
                this.runtimeSlots = runtimeSlots;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(world, runtimeSlots);
            }
        }

        internal struct Enumerator
        {
            private readonly SimulationWorld world;
            private readonly RuntimeSlotTable runtimeSlots;
            private int nextRuntimeSlot;

            internal Enumerator(
                SimulationWorld world,
                RuntimeSlotTable runtimeSlots)
            {
                this.world = world;
                this.runtimeSlots = runtimeSlots;
                nextRuntimeSlot = 0;
                Current = null;
            }

            public LF2Entity Current { get; private set; }

            public bool MoveNext()
            {
                while (nextRuntimeSlot < runtimeSlots.LogicalCapacity)
                {
                    LF2Entity entity =
                        runtimeSlots.GetCurrentOccupant(nextRuntimeSlot++);
                    if (entity == null ||
                        !world.IsActiveForCurrentPassInternal(entity))
                    {
                        continue;
                    }

                    Current = entity;
                    return true;
                }

                Current = null;
                return false;
            }
        }

        internal readonly struct DeferredMutationScope : IDisposable
        {
            private readonly SimulationWorld world;

            internal DeferredMutationScope(SimulationWorld world)
            {
                this.world = world;
                world.BeginDeferredEntityMutationPass();
            }

            public void Dispose()
            {
                world.EndDeferredEntityMutationPass();
            }
        }
    }
}
