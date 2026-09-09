#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;

using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering.Editor;
using NTSD.EditorTools;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6PositiveLinkValidationRetirementProductionEditorTests
    {
        private const BindingFlags DeclaredSurface =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        [Test]
        public void BattleTickPhaseAndWorldOwner_DoNotExposePositiveValidation()
        {
            Assert.That(
                Enum.GetNames(typeof(BattleTickPhase)),
                Does.Not.Contain("HeldLinkValidation"));
            Assert.That(BattleTickPhaseDiagnostics.PhaseCount, Is.EqualTo(33));

            Type worldType = typeof(SimulationWorld);
            Assert.That(
                worldType.GetProperty(
                    "BattleEcsPositiveLinkValidationPassModeForDiagnostics",
                    DeclaredSurface),
                Is.Null);
            Assert.That(
                worldType.GetProperty(
                    "BattleEcsPositiveLinkValidationPassDiagnosticsForDiagnostics",
                    DeclaredSurface),
                Is.Null);
            Assert.That(
                worldType.GetMethod(
                    "ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics",
                    DeclaredSurface),
                Is.Null);
            Assert.That(
                worldType.GetMethod(
                    "RestoreBattleEcsPositiveLinkValidationPassForDiagnostics",
                    DeclaredSurface),
                Is.Null);
        }

        [Test]
        public void RetiredCompatibilityEntry_PreservesSyntheticMismatchAndEmitsNoEvent()
        {
            var world = new SimulationWorld();
            LF2Character holder = Register(world, 0, 98100);
            LF2Character target = Register(world, 1, 98101);
            holder.Runtime.LinkState = 7;
            holder.Runtime.TargetSlotIndex = 1;
            holder.Runtime.HeldWeaponStableId = 1;
            target.Runtime.HolderStableId = 99;
            var events = new BattleParityStructuralEventBuffer(16);
            world.SetStructuralEventSinkForDiagnostics(events, 0, "fixture-setup");

#pragma warning disable CS0618
            world.ValidateHeldLinksAll(71);
#pragma warning restore CS0618

            Assert.That(holder.Runtime.LinkState, Is.EqualTo(7));
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(1));
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(1));
            Assert.That(target.Runtime.HolderStableId, Is.EqualTo(99));
            Assert.That(events.Events, Is.Empty);
        }

        [Test]
        public void RetiredCompatibilityEntry_Warmed4096CallsAllocateZeroBytes()
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

        [Test]
        public void StressRequestConfigAndReport_DoNotExposeRetiredPassSurface()
        {
            Assert.That(
                typeof(ProductionEntityStressRequest).GetField(
                    "positiveLinkValidationMode",
                    DeclaredSurface),
                Is.Null);
            Assert.That(
                typeof(ProductionEntityStressConfig).GetProperty(
                    "PositiveLinkValidationMode",
                    DeclaredSurface),
                Is.Null);
            Assert.That(
                typeof(ProductionEntityStressConfig).GetMethod(
                    "ParsePositiveLinkValidationMode",
                    DeclaredSurface),
                Is.Null);
            Assert.That(
                typeof(ProductionEntityStressConfig).GetMethod(
                    "FormatPositiveLinkValidationMode",
                    DeclaredSurface),
                Is.Null);

            string[] reportFields = typeof(ProductionEntityStressReport)
                .GetFields(DeclaredSurface)
                .Select(field => field.Name)
                .Where(name => name.StartsWith(
                    "positiveLinkValidation",
                    StringComparison.Ordinal))
                .ToArray();
            Assert.That(reportFields, Is.Empty);
        }

        [Test]
        public void ProductionOwnershipInventory_DoesNotCountRetiredPass()
        {
            Assert.That(
                Enum.GetNames(typeof(BattleProductionOwnershipDomain)),
                Does.Not.Contain("PositiveLinkValidation"));
            Assert.That(
                Enum.GetNames(typeof(BattleProductionOwnershipFailure)),
                Does.Not.Contain("PositiveLinkValidation"));
            Assert.That(
                BattleProductionOwnershipInventory.ExpectedCanonicalOwnerCount,
                Is.EqualTo(8));

            var inventory = new BattleProductionOwnershipInventory();
            Assert.That(
                Enumerable.Range(0, inventory.Count)
                    .Select(index => inventory.GetEntry(index).Domain.ToString()),
                Does.Not.Contain("PositiveLinkValidation"));
        }

        [Test]
        public void LifecycleRelease_RemainsTheAtomicPositiveRelationCleanupOwner()
        {
            var world = new SimulationWorld();
            LF2Character removed = Register(world, 2, 98200);
            LF2Character holder = Register(world, 3, 98201);
            holder.Runtime.LinkState = 5;
            holder.Runtime.TargetSlotIndex = 2;
            holder.Runtime.HeldWeaponStableId = 2;
            holder.HeldWeaponReferenceInternal = removed;

            world.Unregister(removed);

            Assert.That(holder.Runtime.LinkState, Is.Zero);
            Assert.That(holder.Runtime.TargetSlotIndex, Is.Zero);
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
            Assert.That(holder.HeldWeaponReferenceInternal, Is.Null);
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
