#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    [Category("Goal20_R5")]
    public sealed class NTSD28B6LegacyHolderCopyResidualRetirementEditorTests
    {
        [Test]
        public void ProductionSources_RetireHolderCopyBehaviorReadersAndWriters()
        {
            string objectPointFactory = Source(
                "Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs");
            string character = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs");
            string datHitResolver = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs");
            string hitResolver = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs");
            string weaponLinkResolver = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterWeaponLinkResolver.cs");
            string weaponBase = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs");
            string releaseFlow = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs");
            string entity = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs");
            string lateLifecycle = Source(
                "Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs");
            string oid5152 = Source(
                "Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs");
            string stage = Source(
                "Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs");
            string logicFactory = Source(
                "Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs");
            string damageWriter = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs");
            string hitPlan = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs");

            Assert.That(objectPointFactory,
                Does.Not.Contain("living.HolderCopySlot = parent.HolderCopySlot;"));
            Assert.That(character,
                Does.Not.Contain("HolderCopySlot = task.holderCopySlot;"));
            Assert.That(character,
                Does.Not.Contain("HolderCopySlot = task.parent.HolderCopySlot;"));
            Assert.That(entity,
                Does.Not.Contain("task.holderCopySlot = HolderCopySlot;"));
            Assert.That(entity,
                Does.Not.Contain("held.HolderCopySlot = 99;"));
            Assert.That(weaponLinkResolver,
                Does.Not.Contain("held.HolderCopySlot = _character.Runtime?.SlotIndex ?? -1;"));
            Assert.That(weaponLinkResolver,
                Does.Not.Contain("held.HolderCopySlot = _character.HolderCopySlot;"));
            Assert.That(weaponBase,
                Does.Not.Contain("HolderCopySlot = holderEntity?.Runtime?.SlotIndex ?? -1;"));
            Assert.That(weaponBase,
                Does.Not.Contain("HolderCopySlot = task.holderCopySlot;"));
            Assert.That(weaponBase,
                Does.Not.Contain("HolderCopySlot = task.parent.HolderCopySlot;"));
            Assert.That(releaseFlow,
                Does.Not.Contain("weapon.HolderCopySlot = -1;"));
            Assert.That(lateLifecycle,
                Does.Not.Contain("task.holderCopySlot = 99;"));
            Assert.That(lateLifecycle,
                Does.Not.Contain("spawned.HolderCopySlot = 99;"));
            Assert.That(oid5152,
                Does.Not.Contain("partner.HolderCopySlot = 99;"));
            Assert.That(stage,
                Does.Not.Contain("entity.HolderCopySlot = requiredRuntimeSlot;"));
            Assert.That(stage,
                Does.Not.Contain("entity.HolderCopySlot = entity.Runtime?.SlotIndex ?? -1;"));
            Assert.That(logicFactory,
                Does.Not.Contain("living.HolderCopySlot = parent.HolderCopySlot;"));
            Assert.That(datHitResolver,
                Does.Not.Contain("ResolveHolderCopyEntity"));
            Assert.That(hitResolver,
                Does.Not.Contain("ResolveHolderCopyEntity"));
            Assert.That(damageWriter,
                Does.Not.Contain("ResolveHolderCopyEntity("));
            Assert.That(hitPlan,
                Does.Not.Contain("int holderSlot = attacker?.HolderCopySlot ?? -1;"));
        }

        [Test]
        public void ReservedCarrierStructure_RemainsPresent()
        {
            string runtime = Source(
                "Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs");
            string task = Source(
                "Assets/NTSD/Scripts/Animation/LF2Tasks/OPointCreateTask.cs");
            string ecs = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs");
            string checksum = Source(
                "Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs");
            string parity = Source(
                "Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs");
            string hitPlan = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs");

            Assert.That(runtime, Does.Contain("public int HolderCopySlotIndex = 99;"));
            Assert.That(runtime,
                Does.Contain("destination.HolderCopySlotIndex = HolderCopySlotIndex;"));
            Assert.That(runtime, Does.Contain("HolderCopySlotIndex = 99;"));
            Assert.That(task, Does.Contain("public int holderCopySlot = -1;"));
            Assert.That(task, Does.Contain("holderCopySlot = -1;"));
            Assert.That(ecs, Does.Contain("HolderCopySlot = new int[capacity];"));
            Assert.That(ecs,
                Does.Contain("Links.HolderCopySlot[slot] = runtime.HolderCopySlotIndex;"));
            Assert.That(ecs,
                Does.Contain("hash.Add(runtime.HolderStableId); hash.Add(runtime.HolderCopySlotIndex);"));
            Assert.That(checksum,
                Does.Contain("builder.AddInt32(isDefault ? 99 : runtime.HolderCopySlotIndex);"));
            Assert.That(parity,
                Does.Contain("(\"holderCopy\", isDefault ? 99 : runtime.HolderCopySlotIndex),"));
            Assert.That(hitPlan,
                Does.Contain("TargetHolderCopySlot = target?.Runtime?.HolderCopySlotIndex ?? int.MinValue,"));
            Assert.That(hitPlan,
                Does.Contain("if (expected.TargetHolderCopySlot != actual.TargetHolderCopySlot) mask |= 1UL << 33;"));
        }

        [TestCase(false, 99)]
        [TestCase(true, 99)]
        [TestCase(false, -1)]
        [TestCase(true, -1)]
        [TestCase(false, 55)]
        [TestCase(true, 55)]
        public void HeldRelation_PreservesHolderCopySentinel(bool opoint, int initialCopy)
        {
            var holder = new LF2Character();
            LF2Entity child = opoint
                ? (LF2Entity)new LF2OtherObject()
                : new LF2Weapon();
            holder.Runtime.SlotIndex = 0;
            holder.HolderCopySlot = 73;
            child.Runtime.SlotIndex = 77;
            child.HolderCopySlot = initialCopy;

            if (child is LF2Weapon weapon)
                weapon.SetWeaponType(1);

            int holderCopyBefore = child.HolderCopySlot;
            var resolver = new LF2CharacterWeaponLinkResolver(holder);
            if (opoint)
                resolver.AttachOpointHeldObject(child);
            else
                resolver.HoldWeapon(child);

            Assert.That(child.HolderCopySlot, Is.EqualTo(holderCopyBefore));
            Assert.That(holder.Runtime.TargetSlotIndex,
                Is.EqualTo(child.Runtime.SlotIndex));
            Assert.That(holder.Runtime.HeldWeaponStableId,
                Is.EqualTo(child.Runtime.SlotIndex));
            Assert.That(child.Runtime.HolderStableId,
                Is.EqualTo(holder.Runtime.SlotIndex));
        }

        private static string Source(string relativePath)
        {
            string root = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName;
            return File.ReadAllText(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
