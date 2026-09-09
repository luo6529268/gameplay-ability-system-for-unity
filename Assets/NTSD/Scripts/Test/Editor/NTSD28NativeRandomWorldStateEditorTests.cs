#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeRandomWorldStateEditorTests
    {
        private const uint DefaultSeed = 0x4E545344u;

        [Test]
        public void WorldConstructionAndReset_SeedNativeRandomWithAdapterDefault()
        {
            var expected = new NTSD28NativeRandom();
            expected.ResetFromSeed(DefaultSeed);
            var world = new SimulationWorld();

            AssertStateEqual(
                expected.CaptureScalarState(),
                world.NativeRandom.CaptureScalarState());

            world.NativeRandom.CrtNext();
            world.NativeRandom.SynchronizedNext(0x51u, 17);
            world.ResetRuntimeState();

            AssertStateEqual(
                expected.CaptureScalarState(),
                world.NativeRandom.CaptureScalarState());
        }

        [Test]
        public void LockstepBootstrap_SeedsLegacyAndNativeRandomFromIdentity()
        {
            LockstepStartBarrier barrier = CreateBarrier(0x51A7u);
            var world = new SimulationWorld();
            var expected = new NTSD28NativeRandom();
            expected.ResetForDirectBattle(barrier.Identity.Seed);

            InProcessBattleWorldBootstrap.PrepareWorldForHost(barrier, world);

            Assert.That(world.Rng.State, Is.EqualTo(barrier.Identity.Seed));
            Assert.That(world.Runtime.Match.Seed,
                Is.EqualTo(unchecked((int)barrier.Identity.Seed)));
            AssertStateEqual(
                expected.CaptureScalarState(),
                world.NativeRandom.CaptureScalarState());
        }

        [Test]
        public void CoreScalarSnapshot_CapturesNativeRandomWithoutWarmAllocation()
        {
            LockstepStartBarrier barrier = CreateBarrier(0x12345678u);
            var world = new SimulationWorld();
            InProcessBattleWorldBootstrap.PrepareWorldForHost(barrier, world);
            world.NativeRandom.CrtNext();
            world.NativeRandom.SynchronizedNext(0x82u, 97);
            NTSD28NativeRandomScalarState expected =
                world.NativeRandom.CaptureScalarState();

            var snapshot = new BattleWorldCoreScalarSnapshot(
                world,
                barrier.Identity);

            AssertStateEqual(expected, snapshot.NativeRandomState);
            _ = new BattleWorldCoreScalarSnapshot(world, barrier.Identity);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                _ = new BattleWorldCoreScalarSnapshot(world, barrier.Identity);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void BattleStateSnapshot_RestoresNativeRandomAcrossWorldsAndWarmWithoutAllocation()
        {
            LockstepStartBarrier barrier = CreateBarrier(0xCAFEBABEu);
            var source = new SimulationWorld();
            InProcessBattleWorldBootstrap.PrepareWorldForHost(barrier, source);
            source.NativeRandom.CrtNext();
            source.NativeRandom.SynchronizedNext(0x90u, 17);
            source.NativeRandom.SynchronizedNext(0x91u, 31);
            NTSD28NativeRandomScalarState expected =
                source.NativeRandom.CaptureScalarState();
            BattleStateSnapshotBuffer snapshot =
                source.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(
                source.TryCaptureBattleStateSnapshot(
                    barrier.Identity,
                    0,
                    snapshot),
                Is.True);

            var destination = new SimulationWorld();
            Assert.That(
                destination.TryRestoreBattleStateSnapshot(
                    barrier.Identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());
            AssertStateEqual(
                expected,
                destination.NativeRandom.CaptureScalarState());

            destination.NativeRandom.CrtNext();
            Assert.That(
                destination.TryRestoreBattleStateSnapshot(
                    barrier.Identity,
                    snapshot,
                    out failure),
                Is.True,
                failure.ToString());
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 128; index++)
            {
                if (!destination.TryRestoreBattleStateSnapshot(
                        barrier.Identity,
                        snapshot,
                        out _))
                {
                    Assert.Fail($"Warm restore failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            AssertStateEqual(
                expected,
                destination.NativeRandom.CaptureScalarState());
        }

        [Test]
        public void RuntimeChecksum_TracksBothNativeStreamsWithoutWarmAllocation()
        {
            var world = new SimulationWorld();
            uint legacyState = world.Rng.State;
            ulong legacyCalls = world.Rng.CallCount;
            ulong before = world.CaptureRuntimeChecksum64(0, null);

            world.NativeRandom.CrtNext();
            ulong afterCrt = world.CaptureRuntimeChecksum64(0, null);
            world.NativeRandom.SynchronizedNext(0xA0u, 13);
            ulong afterSynchronized = world.CaptureRuntimeChecksum64(0, null);

            Assert.That(afterCrt, Is.Not.EqualTo(before));
            Assert.That(afterSynchronized, Is.Not.EqualTo(afterCrt));
            Assert.That(world.Rng.State, Is.EqualTo(legacyState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(legacyCalls));

            _ = world.CaptureRuntimeChecksum64(0, null);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long allocationStart = GC.GetAllocatedBytesForCurrentThread();
            ulong actual = 0;
            for (int index = 0; index < 256; index++)
            {
                actual = world.CaptureRuntimeChecksum64(0, null);
            }
            long allocated =
                GC.GetAllocatedBytesForCurrentThread() - allocationStart;
            Assert.That(actual, Is.EqualTo(afterSynchronized));
            Assert.That(allocated, Is.Zero);
        }

        private static LockstepStartBarrier CreateBarrier(uint seed)
        {
            var identity = new LockstepSessionIdentity(
                LockstepSessionIdentity.CurrentSchemaVersion,
                0xB2000001UL,
                seed,
                0xCA7A10UL,
                0x57A6EUL,
                new[] { 0, 1 });
            return new LockstepStartBarrier(
                identity,
                0xC0DE0001UL,
                1,
                BattleRuntimeProfilePolicy.Create(
                    BattleRuntimeProfile.Authority400));
        }

        private static void AssertStateEqual(
            NTSD28NativeRandomScalarState expected,
            NTSD28NativeRandomScalarState actual)
        {
            Assert.That(actual.CrtState, Is.EqualTo(expected.CrtState));
            Assert.That(actual.CrtCalls, Is.EqualTo(expected.CrtCalls));
            Assert.That(actual.TableSeed, Is.EqualTo(expected.TableSeed));
            Assert.That(actual.SynchronizedCounter,
                Is.EqualTo(expected.SynchronizedCounter));
            Assert.That(actual.SynchronizedIndex,
                Is.EqualTo(expected.SynchronizedIndex));
            Assert.That(actual.SynchronizedCalls,
                Is.EqualTo(expected.SynchronizedCalls));
            Assert.That(actual.LastSynchronizedCallSite,
                Is.EqualTo(expected.LastSynchronizedCallSite));
            Assert.That(actual.SynchronizedTableHash,
                Is.EqualTo(expected.SynchronizedTableHash));
        }
    }
}
#endif
