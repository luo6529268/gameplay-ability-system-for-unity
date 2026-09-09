#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type0EncodedStatusEditorTests
    {
        [Test]
        public void ZeroFields_DoNotConsumeSynchronizedRandomOrMutateCarriers()
        {
            var random = new NTSD28NativeRandom(123u);
            var target = new NTSDEntityRuntime
            {
                WeakTimer12C = 77,
                StatusDx1C0 = 88,
            };
            ulong before = random.CaptureScalarState().SynchronizedCalls;

            BattleDamageWriter.ApplyConfirmedInputStatuses(
                random,
                target,
                new InteractionArea());

            Assert.That(random.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(before));
            Assert.That(target.WeakTimer12C, Is.EqualTo(77));
            Assert.That(target.StatusDx1C0, Is.EqualTo(88));
        }

        [Test]
        public void GuaranteedPayloads_UseExactTwentyCallOrderAndWriteAllCarriers()
        {
            var random = new NTSD28NativeRandom(456u);
            var observer = new CallObserver();
            random.SetDiagnosticCallObserver(observer);
            var target = new NTSDEntityRuntime();
            var itr = new InteractionArea
            {
                poison = 123400,
                weak = 101,
                bound = 102,
                facing = 103,
                manacle = 104,
                delay = 105,
                join = 106,
                mimic = 107,
                dx = 108,
                dy = 109,
                dz = 110,
                gain = 111,
                confus = 112,
            };

            BattleDamageWriter.ApplyConfirmedInputStatuses(random, target, itr);

            Assert.That(target.PoisonTimer120, Is.EqualTo(12));
            Assert.That(target.PoisonType124, Is.Zero);
            Assert.That(target.PoisonStrength128, Is.EqualTo(34));
            Assert.That(target.WeakTimer12C, Is.EqualTo(101));
            Assert.That(target.BoundState198, Is.EqualTo(102));
            Assert.That(target.InputDoubleCost19C, Is.EqualTo(103));
            Assert.That(target.InputActionLock130, Is.EqualTo(104));
            Assert.That(target.DelayTimer134, Is.EqualTo(105));
            Assert.That(target.JoinTimer148, Is.EqualTo(106));
            Assert.That(target.InputProxyCounter14C, Is.EqualTo(107));
            Assert.That(target.StatusDx1C0, Is.EqualTo(108));
            Assert.That(target.StatusDy1C4, Is.EqualTo(109));
            Assert.That(target.StatusDz1C8, Is.EqualTo(110));
            Assert.That(target.StatusGain1CC, Is.EqualTo(111));
            Assert.That(target.InputRemapState138, Is.EqualTo(112));
            CollectionAssert.AreEquivalent(
                new byte[] { 0, 1, 2, 3, 4, 5, 6 },
                target.InputRemapIndices13C);

            uint[] firstSites =
            {
                0x0041649C, 0x004164D5, 0x004164FC, 0x00416523,
                0x00416547, 0x0041656B, 0x00416592, 0x004165B9,
                0x004165E0, 0x0041660A, 0x00416634, 0x0041665E,
                0x00416685,
            };
            Assert.That(observer.SynchronizedCalls.Count, Is.EqualTo(20));
            for (int index = 0; index < firstSites.Length; index++)
            {
                Assert.That(observer.SynchronizedCalls[index].CallSite,
                    Is.EqualTo(firstSites[index]), "call " + index);
                Assert.That(observer.SynchronizedCalls[index].UpperBound,
                    Is.EqualTo(100), "bound " + index);
            }
            for (int index = firstSites.Length; index < 20; index++)
            {
                Assert.That(observer.SynchronizedCalls[index].CallSite,
                    Is.EqualTo(0x004166B2u), "remap call " + index);
                Assert.That(observer.SynchronizedCalls[index].UpperBound,
                    Is.EqualTo(7), "remap bound " + index);
            }
        }

        [Test]
        public void EncodedChance_UsesRollGreaterOrEqualFailureAndPreservesOldValue()
        {
            const uint seed = 789u;
            const int encoded = 50077;
            var oracle = new NTSD28NativeRandom(seed);
            int roll = oracle.SynchronizedNext(0x004164D5u, 100);
            var random = new NTSD28NativeRandom(seed);
            var target = new NTSDEntityRuntime { WeakTimer12C = 999 };

            BattleDamageWriter.ApplyConfirmedInputStatuses(
                random,
                target,
                new InteractionArea { weak = encoded });

            Assert.That(target.WeakTimer12C,
                Is.EqualTo(roll < 50 ? 77 : 999));
            Assert.That(random.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(1));
        }

        [Test]
        public void ProductionStandardCharacterDamage_InvokesEncodedProducer()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(8330, 0);
            LF2Character victim = CreateCharacter(8331, 1);
            attacker.SetRequiredRuntimeSlot(0);
            victim.SetRequiredRuntimeSlot(1);
            world.Register(attacker);
            world.Register(victim);
            ulong before = world.NativeRandom.CaptureScalarState()
                .SynchronizedCalls;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                new InteractionArea
                {
                    kind = 9,
                    delay = 42,
                });

            Assert.That(applied, Is.True);
            Assert.That(victim.Runtime.DelayTimer134, Is.EqualTo(42));
            Assert.That(world.NativeRandom.CaptureScalarState()
                .SynchronizedCalls, Is.EqualTo(before + 1));
        }

        private static LF2Character CreateCharacter(int objectId, int slot)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = 0,
                    wait = 100,
                    next = id,
                });
            }
            var entity = new LF2Character { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5Type0EncodedStatus",
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            return entity;
        }

        private sealed class CallObserver : INTSD28NativeRandomCallObserver
        {
            internal readonly List<NTSD28NativeSynchronizedCall>
                SynchronizedCalls = new List<NTSD28NativeSynchronizedCall>();

            public void OnCrtNext(NTSD28NativeCrtCall call)
            {
            }

            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                SynchronizedCalls.Add(call);
            }
        }
    }
}
#endif
