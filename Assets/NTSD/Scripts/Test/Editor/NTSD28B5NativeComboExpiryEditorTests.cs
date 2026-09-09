#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5NativeComboExpiryEditorTests
    {
        [Test]
        public void Expiry_MissingRecordAndNegativeRespondAreNoOps()
        {
            SimulationWorld world = World(out LF2Character entity);
            world.Runtime.NativeWorldClock.FrameSequence = 100UL;
            entity.Runtime.NativeComboHitCount1E0 = 3;
            entity.Runtime.NativeComboHitLastTick1E4 = 1UL;

            Assert.That(BattleNativeComboExpiryModule.Expire(world), Is.Zero);
            Assert.That(entity.Runtime.NativeComboHitCount1E0, Is.EqualTo(3));
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 0, 1, -1, 0);
            Assert.That(BattleNativeComboExpiryModule.Expire(world), Is.Zero);
            Assert.That(entity.Runtime.NativeComboHitCount1E0, Is.EqualTo(3));
            Assert.That(entity.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(1UL));
        }

        [Test]
        public void Expiry_UsesInclusiveElapsedBoundaryAndPreservesLastTick()
        {
            var world = new SimulationWorld();
            LF2Character before = Register(world, 0);
            LF2Character boundary = Register(world, 1);
            LF2Character future = Register(world, 2);
            LF2Character zero = Register(world, 3);
            LF2Character negative = Register(world, 4);
            world.Runtime.NativeWorldClock.FrameSequence = 100UL;
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 0, 7, 50, 9);
            Set(before, 2, 51UL);
            Set(boundary, 3, 50UL);
            Set(future, 4, 101UL);
            Set(zero, 0, 0UL);
            Set(negative, -2, 0UL);

            Assert.That(BattleNativeComboExpiryModule.Expire(world), Is.EqualTo(1));
            Assert.That(before.Runtime.NativeComboHitCount1E0, Is.EqualTo(2));
            Assert.That(boundary.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(boundary.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(50UL));
            Assert.That(future.Runtime.NativeComboHitCount1E0, Is.EqualTo(4));
            Assert.That(zero.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(negative.Runtime.NativeComboHitCount1E0, Is.EqualTo(-2));
        }

        [Test]
        public void Expiry_ZeroRespondExpiresAHitFromTheCurrentProcessTick()
        {
            SimulationWorld world = World(out LF2Character entity);
            world.Runtime.NativeWorldClock.FrameSequence = 25UL;
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 0, 1, 0, 0);
            Set(entity, 1, 25UL);

            Assert.That(BattleNativeComboExpiryModule.Expire(world), Is.EqualTo(1));
            Assert.That(entity.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(entity.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(25UL));
        }

        [Test]
        public void TickSystem_PlacesSingleExpiryAfterC25AndBeforeResidualTail()
        {
            string path = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Scripts",
                "Simulation",
                "Core",
                "NTSDBattleTickSystem.cs");
            string source = File.ReadAllText(path);
            const string hook = "ExpireNativeComboEntries();";
            int late = source.IndexOf(
                "LateEntityUpdate(tickIndex);",
                StringComparison.Ordinal);
            int expiry = source.IndexOf(hook, StringComparison.Ordinal);
            int residual = source.IndexOf(
                "FrameAdvanceAll(tickIndex);",
                late + 1,
                StringComparison.Ordinal);

            Assert.That(Count(source, hook), Is.EqualTo(1));
            Assert.That(late, Is.GreaterThanOrEqualTo(0));
            Assert.That(expiry, Is.GreaterThan(late));
            Assert.That(residual, Is.GreaterThan(expiry));
        }

        [Test]
        public void WarmExpiry_AllocatesZero()
        {
            SimulationWorld world = World(out LF2Character entity);
            world.Runtime.NativeWorldClock.FrameSequence = 100UL;
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 0, 0, 50, 0);
            Set(entity, 1, 100UL);
            BattleNativeComboExpiryModule.Expire(world);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int expired = 0;
            for (int index = 0; index < 4096; index++)
            {
                entity.Runtime.NativeComboHitCount1E0 = 1;
                expired += BattleNativeComboExpiryModule.Expire(world);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(expired, Is.Zero,
                "elapsed zero remains below respond fifty in every warm iteration");
            Assert.That(allocated, Is.Zero);
        }

        private static SimulationWorld World(out LF2Character entity)
        {
            var world = new SimulationWorld();
            entity = Register(world, 0);
            return world;
        }

        private static LF2Character Register(SimulationWorld world, int slot)
        {
            var entity = new LF2Character { ObjectId = 7200 + slot };
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private static void Set(LF2Character entity, int count, ulong lastTick)
        {
            entity.Runtime.NativeComboHitCount1E0 = count;
            entity.Runtime.NativeComboHitLastTick1E4 = lastTick;
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
    }
}
#endif
