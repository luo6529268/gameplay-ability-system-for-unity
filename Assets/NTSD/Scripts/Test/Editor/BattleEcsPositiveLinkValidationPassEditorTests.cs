#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsPositiveLinkValidationPassEditorTests
    {
        [Test]
        public void DefaultMode_IsDataOrientedAndCannotSwitchAfterResetBoundary()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsPositiveLinkValidationPassModeForDiagnostics,
                Is.EqualTo(BattleEcsPositiveLinkValidationPassMode.DataOriented));

            world.AdvanceBattleFlowTick(1);
            Assert.Throws<InvalidOperationException>(() =>
                world.ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
                    BattleEcsPositiveLinkValidationPassMode.Legacy));
        }

        [Test]
        public void ShadowCompare_MatchesLegacyForKeptAndClearedLinks()
        {
            SimulationWorld world = CreateContractWorld(out LF2Character[] entities);
            world.ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
                BattleEcsPositiveLinkValidationPassMode.ShadowCompare);

            world.ValidateHeldLinksAll(9);

            AssertContractState(entities);
            BattleEcsPositiveLinkValidationPassDiagnostics diagnostics =
                world.BattleEcsPositiveLinkValidationPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.ValidationCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(4));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.IsClean, Is.True);
        }

        [Test]
        public void DataOrientedWriter_MatchesLegacyContractAndPreservesReverseFields()
        {
            SimulationWorld legacy = CreateContractWorld(out LF2Character[] legacyEntities);
            SimulationWorld data = CreateContractWorld(out LF2Character[] dataEntities);
            data.ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
                BattleEcsPositiveLinkValidationPassMode.DataOriented);

            legacy.ValidateHeldLinksAll(11);
            data.ValidateHeldLinksAll(11);

            for (int i = 0; i < legacyEntities.Length; i++)
            {
                Assert.That(
                    dataEntities[i].Runtime.LinkState,
                    Is.EqualTo(legacyEntities[i].Runtime.LinkState),
                    $"LinkState mismatch at entity {i}");
                Assert.That(
                    dataEntities[i].Runtime.TargetSlotIndex,
                    Is.EqualTo(legacyEntities[i].Runtime.TargetSlotIndex),
                    $"TargetSlotIndex mismatch at entity {i}");
                Assert.That(
                    dataEntities[i].Runtime.HeldWeaponStableId,
                    Is.EqualTo(legacyEntities[i].Runtime.HeldWeaponStableId),
                    $"HeldWeaponStableId mismatch at entity {i}");
                Assert.That(
                    dataEntities[i].Runtime.HolderStableId,
                    Is.EqualTo(legacyEntities[i].Runtime.HolderStableId),
                    $"reverse HolderStableId changed at entity {i}");
            }

            AssertContractState(dataEntities);
            BattleEcsPositiveLinkValidationPassDiagnostics diagnostics =
                data.BattleEcsPositiveLinkValidationPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(4));
            Assert.That(diagnostics.KeptCount, Is.EqualTo(1));
            Assert.That(diagnostics.ClearedCount, Is.EqualTo(3));
        }

        [Test]
        public void DataOrientedWriter_UsesSameTickLiveLinkInsteadOfPreviousShadow()
        {
            var world = new SimulationWorld();
            LF2Character holder = Register(world, 0, 100);
            LF2Character target = Register(world, 1, 101);
            world.CaptureBattleEcsShadowForDiagnostics(0);
            world.ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
                BattleEcsPositiveLinkValidationPassMode.DataOriented);

            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = 1;
            holder.Runtime.HeldWeaponStableId = 1;
            target.Runtime.HolderStableId = 77;
            world.ValidateHeldLinksAll(1);

            Assert.That(holder.Runtime.LinkState, Is.Zero);
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(1));
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(1));
            Assert.That(target.Runtime.HolderStableId, Is.EqualTo(77));
            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void PositiveLinkIndex_TracksWritesAndRejectsReleasedGeneration()
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
        public void DataOrientedWriter_ConsumesIndexedLinksInRuntimeSlotOrder()
        {
            var world = new SimulationWorld();
            LF2Character high = Register(world, 9, 190);
            LF2Character low = Register(world, 2, 191);
            SetPositiveLink(high, 99);
            SetPositiveLink(low, 99);
            var events = new BattleParityStructuralEventBuffer(16);
            world.SetStructuralEventSinkForDiagnostics(events, 0, "fixture-setup");
            world.ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
                BattleEcsPositiveLinkValidationPassMode.DataOriented);

            world.ValidateHeldLinksAll(21);

            Assert.That(events.Events.Count, Is.EqualTo(2));
            Assert.That(events.Events[0].ActorSlot, Is.EqualTo(2));
            Assert.That(events.Events[1].ActorSlot, Is.EqualTo(9));
            Assert.That(world.PositiveLinkIndexCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void DataOrientedWriter_EmitsAuthorityStructuralWitness()
        {
            var world = new SimulationWorld();
            LF2Character holder = Register(world, 0, 200);
            LF2Character target = Register(world, 1, 201);
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = 1;
            holder.Runtime.HeldWeaponStableId = 1;
            target.Runtime.HolderStableId = 2;
            var events = new BattleParityStructuralEventBuffer(16);
            world.SetStructuralEventSinkForDiagnostics(events, 0, "fixture-setup");
            world.ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
                BattleEcsPositiveLinkValidationPassMode.DataOriented);

            world.ValidateHeldLinksAll(17);

            Assert.That(events.Events.Count, Is.EqualTo(1));
            BattleParityStructuralEvent value = events.Events[0];
            Assert.That(value.Tick, Is.EqualTo(17));
            Assert.That(value.Pass, Is.EqualTo("positive-link-validation"));
            Assert.That(value.Action, Is.EqualTo("link-validation"));
            Assert.That(value.Outcome, Is.EqualTo("cleared"));
            Assert.That(value.Reason, Is.EqualTo("holder-mismatch"));
            Assert.That(value.AfterLinkState, Is.Zero);
            Assert.That(value.AfterTargetSlot, Is.EqualTo(1));
            Assert.That(value.AfterHeldWeaponSlot, Is.EqualTo(1));
            Assert.That(value.TargetBeforeHolderSlot, Is.EqualTo(2));
            Assert.That(value.TargetAfterHolderSlot, Is.EqualTo(2));
        }

        [Test]
        public void Extended1000_WarmedDataOrientedWriterDoesNotAllocate()
        {
            const int capacity = 1050;
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                capacity);
            for (int slot = 50; slot < capacity; slot++)
                Register(world, slot, 1000 + slot);
            world.ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
                BattleEcsPositiveLinkValidationPassMode.DataOriented);
            world.ValidateHeldLinksAll(1);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.ValidateHeldLinksAll(2);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            BattleEcsPositiveLinkValidationPassDiagnostics diagnostics =
                world.BattleEcsPositiveLinkValidationPassDiagnosticsForDiagnostics;
            Assert.That(allocated, Is.Zero);
            Assert.That(diagnostics.RunCount, Is.EqualTo(2));
            Assert.That(diagnostics.SlotVisitCount, Is.Zero);
        }

        private static SimulationWorld CreateContractWorld(
            out LF2Character[] entities)
        {
            var world = new SimulationWorld();
            entities = new[]
            {
                Register(world, 0, 300),
                Register(world, 1, 301),
                Register(world, 2, 302),
                Register(world, 3, 303),
                Register(world, 4, 304),
                Register(world, 5, 305),
                Register(world, 6, 306),
            };

            SetPositiveLink(entities[0], 1);
            entities[1].Runtime.HolderStableId = 0;

            SetPositiveLink(entities[2], 3);
            entities[3].Runtime.HolderStableId = 99;

            SetPositiveLink(entities[4], 399);

            SetPositiveLink(entities[5], 6);
            entities[6].Runtime.HolderStableId = 5;
            entities[6].Runtime.PendingFlushDestroy = true;
            return world;
        }

        private static void AssertContractState(LF2Character[] entities)
        {
            Assert.That(entities[0].Runtime.LinkState, Is.EqualTo(1));
            Assert.That(entities[0].Runtime.TargetSlotIndex, Is.EqualTo(1));
            Assert.That(entities[0].Runtime.HeldWeaponStableId, Is.EqualTo(1));

            AssertInvalidatedPreservingForwardFields(entities[2], 3, 3);
            Assert.That(entities[3].Runtime.HolderStableId, Is.EqualTo(99));
            AssertInvalidatedPreservingForwardFields(entities[4], 399, 399);
            AssertInvalidatedPreservingForwardFields(entities[5], 6, 6);
            Assert.That(entities[6].Runtime.HolderStableId, Is.EqualTo(5));
        }

        private static void SetPositiveLink(
            LF2Character holder,
            int targetSlot)
        {
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = targetSlot;
            holder.Runtime.HeldWeaponStableId = targetSlot;
        }

        private static void AssertInvalidatedPreservingForwardFields(
            LF2Character holder,
            int targetSlot,
            int heldWeaponStableId)
        {
            Assert.That(holder.Runtime.LinkState, Is.Zero);
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(targetSlot));
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(heldWeaponStableId));
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
