#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5NativeComboOrdinaryProducerEditorTests
    {
        [Test]
        public void DirectProducer_RequiresRecordBoundAndCurrentTypeZeroTarget()
        {
            SimulationWorld world = World(
                out TypedCharacter attacker,
                out TypedCharacter target);

            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(world, 0, 1),
                Is.False);
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 0, 1, 50, 0);
            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(world, 0, 1),
                Is.False);
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 0);
            target.Type = LF2ObjectType.LightWeapon;
            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(world, 0, 1),
                Is.False);

            Assert.That(attacker.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(target.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [Test]
        public void DirectProducer_FacingSelectsSourceAndStoresCurrentProcessTick()
        {
            SimulationWorld world = World(
                out TypedCharacter attacker,
                out TypedCharacter target);
            world.Runtime.NativeWorldClock.FrameSequence = 41UL;
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 0);

            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(world, 0, 1),
                Is.True);
            Assert.That(attacker.Runtime.NativeComboHitCount1E0, Is.EqualTo(1));
            Assert.That(attacker.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(42UL));
            Assert.That(target.Runtime.NativeComboHitCount1E0, Is.Zero);

            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 0, 50, 0);
            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(world, 0, 1),
                Is.True);
            Assert.That(target.Runtime.NativeComboHitCount1E0, Is.EqualTo(1));
            Assert.That(target.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(42UL));
        }

        [Test]
        public void DirectProducer_NonTypeZeroSourceUsesExactlyOneActiveOwnerHop()
        {
            SimulationWorld world = World(
                out _,
                out TypedCharacter target);
            var source = Register(world, 2, LF2ObjectType.Other);
            var owner = Register(world, 3, LF2ObjectType.Other);
            source.Runtime.OwnerSlotIndex = 3;
            owner.Runtime.OwnerSlotIndex = 0;
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 0);

            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(world, 2, 1),
                Is.True);
            Assert.That(source.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(owner.Runtime.NativeComboHitCount1E0, Is.EqualTo(1),
                "the native route stops after one owner hop even when that owner is non-type0");
            Assert.That(target.Runtime.NativeComboHitCount1E0, Is.Zero);

            source.Runtime.OwnerSlotIndex = 7;
            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(world, 2, 1),
                Is.False);
            Assert.That(owner.Runtime.NativeComboHitCount1E0, Is.EqualTo(1));
        }

        [Test]
        public void DirectProducer_PostDispatchSlotLookupFailsClosedForInactiveSourceOrTarget()
        {
            SimulationWorld sourceWorld = World(
                out TypedCharacter source,
                out TypedCharacter sourceTarget);
            sourceWorld.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 0);
            sourceWorld.Unregister(source);
            Assert.That(sourceWorld.FindEntityByRuntimeSlotForQuery(0), Is.Null);
            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(sourceWorld, 0, 1),
                Is.False);
            Assert.That(sourceTarget.Runtime.NativeComboHitCount1E0, Is.Zero);

            SimulationWorld targetWorld = World(
                out TypedCharacter targetAttacker,
                out TypedCharacter inactiveTarget);
            targetWorld.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 0);
            targetWorld.Unregister(inactiveTarget);
            Assert.That(targetWorld.FindEntityByRuntimeSlotForQuery(1), Is.Null);
            Assert.That(
                BattleNativeComboOrdinaryProducer.TryApply(targetWorld, 0, 1),
                Is.False);
            Assert.That(targetAttacker.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [Test]
        public void SharedRunnerBoundary_OnlySuccessfulDamageDispatchProduces()
        {
            SimulationWorld world = World(
                out TypedCharacter attacker,
                out _);
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 0);

            Assert.That(BattleHitCandidateSequenceRunner
                .TryProduceNativeComboAfterDispatch(
                    world,
                    BattleHitCandidateDisposition.Kind8,
                    true,
                    0,
                    1), Is.False);
            Assert.That(BattleHitCandidateSequenceRunner
                .TryProduceNativeComboAfterDispatch(
                    world,
                    BattleHitCandidateDisposition.Damage,
                    false,
                    0,
                    1), Is.False);
            Assert.That(attacker.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(BattleHitCandidateSequenceRunner
                .TryProduceNativeComboAfterDispatch(
                    world,
                    BattleHitCandidateDisposition.Damage,
                    true,
                    0,
                    1), Is.True);
            Assert.That(attacker.Runtime.NativeComboHitCount1E0, Is.EqualTo(1));
        }

        [Test]
        public void SharedRunner_HasOneHookAfterDispatchAndAfterFirstBodyEarlyReturn()
        {
            string path = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Scripts",
                "Animation",
                "LF2Objects",
                "BattleHitCandidateSequenceRunner.cs");
            string source = File.ReadAllText(path);
            const string hook = "TryProduceNativeComboAfterDispatch(";
            int firstBodyReturn = source.IndexOf(
                "if (firstBodyResponse.Applied)",
                StringComparison.Ordinal);
            int dispatch = source.IndexOf(
                "bool dispatched =",
                StringComparison.Ordinal);
            int hookIndex = source.IndexOf(hook, StringComparison.Ordinal);

            Assert.That(Count(source, hook), Is.EqualTo(2),
                "one production call plus the helper declaration must exist");
            Assert.That(firstBodyReturn, Is.GreaterThanOrEqualTo(0));
            Assert.That(dispatch, Is.GreaterThan(firstBodyReturn));
            Assert.That(hookIndex, Is.GreaterThan(dispatch));
        }

        [Test]
        public void WarmDirectProducer_AllocatesZeroAndAccumulatesEveryAppliedHit()
        {
            SimulationWorld world = World(out TypedCharacter attacker, out _);
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 0);
            BattleNativeComboOrdinaryProducer.TryApply(world, 0, 1);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
                BattleNativeComboOrdinaryProducer.TryApply(world, 0, 1);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(attacker.Runtime.NativeComboHitCount1E0, Is.EqualTo(4097));
            Assert.That(allocated, Is.Zero);
        }

        private static SimulationWorld World(
            out TypedCharacter attacker,
            out TypedCharacter target)
        {
            var world = new SimulationWorld();
            attacker = Register(world, 0, LF2ObjectType.Character);
            target = Register(world, 1, LF2ObjectType.Character);
            return world;
        }

        private static TypedCharacter Register(
            SimulationWorld world,
            int slot,
            LF2ObjectType type)
        {
            var entity = new TypedCharacter(type)
            {
                ObjectId = 7000 + slot,
            };
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private static int Count(string source, string value)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(
                       value,
                       offset,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }

            return count;
        }

        private sealed class TypedCharacter : LF2Character
        {
            internal TypedCharacter(LF2ObjectType type)
            {
                Type = type;
            }

            internal LF2ObjectType Type { get; set; }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)Type;
            }
        }
    }
}
#endif
