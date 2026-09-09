#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28InputProxyControlLifecycleEditorTests
    {
        [Test]
        public void ConfirmedMimicStatus_WritesPayloadOnly()
        {
            var target = new NTSDEntityRuntime
            {
                InputProxyCounter14C = 9,
                InputProxySourceSlot178 = 12,
                InputProxyEnabled17C = 1,
            };

            NTSD28InputProxyControlLifecycle.ApplyConfirmedMimicCounter(target, -3);

            Assert.That(target.InputProxyCounter14C, Is.EqualTo(-3));
            Assert.That(target.InputProxySourceSlot178, Is.EqualTo(12));
            Assert.That(target.InputProxyEnabled17C, Is.EqualTo(1));
        }

        [Test]
        public void ConfirmedHit_EnablesPositiveType0MimicOnce()
        {
            var attacker = new NTSDEntityRuntime { ObjType = 0, SlotIndex = 7 };
            var target = new NTSDEntityRuntime
            {
                ObjType = 0,
                InputProxyCounter14C = 5,
                InputProxySourceSlot178 = -1,
                InputProxyEnabled17C = 0,
            };

            bool enabled = NTSD28InputProxyControlLifecycle
                .TryEnableFromConfirmedHit(attacker, target);
            bool second = NTSD28InputProxyControlLifecycle
                .TryEnableFromConfirmedHit(
                    new NTSDEntityRuntime { ObjType = 0, SlotIndex = 9 },
                    target);

            Assert.That(enabled, Is.True);
            Assert.That(second, Is.False);
            Assert.That(target.InputProxyCounter14C, Is.EqualTo(5));
            Assert.That(target.InputProxySourceSlot178, Is.EqualTo(7));
            Assert.That(target.InputProxyEnabled17C, Is.EqualTo(1));
        }

        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 5)]
        [TestCase(0, 0, 0)]
        public void ConfirmedHit_RejectsWrongTypesOrNonPositiveCounter(
            int attackerType,
            int targetType,
            int counter)
        {
            var attacker = new NTSDEntityRuntime
            {
                ObjType = attackerType,
                SlotIndex = 7,
            };
            var target = new NTSDEntityRuntime
            {
                ObjType = targetType,
                InputProxyCounter14C = counter,
                InputProxySourceSlot178 = 4,
                InputProxyEnabled17C = 2,
            };

            bool enabled = NTSD28InputProxyControlLifecycle
                .TryEnableFromConfirmedHit(attacker, target);

            Assert.That(enabled, Is.False);
            Assert.That(target.InputProxySourceSlot178, Is.EqualTo(4));
            Assert.That(target.InputProxyEnabled17C, Is.EqualTo(2));
        }

        [Test]
        public void PositiveHpBody_DecrementsAndExpiresInSameTail()
        {
            var runtime = new NTSDEntityRuntime
            {
                InputProxyCounter14C = 1,
                InputProxySourceSlot178 = 8,
                InputProxyEnabled17C = 1,
            };

            NTSD28InputProxyControlLifecycle.AdvanceReactionTail(
                runtime,
                nativeEntityBodySkipped: false,
                currentHp: 1);

            Assert.That(runtime.InputProxyCounter14C, Is.Zero);
            Assert.That(runtime.InputProxyEnabled17C, Is.Zero);
            Assert.That(runtime.InputProxySourceSlot178, Is.EqualTo(8));
        }

        [TestCase(true, 10)]
        [TestCase(false, 0)]
        [TestCase(false, -1)]
        public void SkippedOrDeadBody_FreezesPositiveCounter(
            bool bodySkipped,
            int hp)
        {
            var runtime = new NTSDEntityRuntime
            {
                InputProxyCounter14C = 2,
                InputProxySourceSlot178 = 8,
                InputProxyEnabled17C = 1,
            };

            NTSD28InputProxyControlLifecycle.AdvanceReactionTail(
                runtime,
                bodySkipped,
                hp);

            Assert.That(runtime.InputProxyCounter14C, Is.EqualTo(2));
            Assert.That(runtime.InputProxyEnabled17C, Is.EqualTo(1));
        }

        [TestCase(true, 10)]
        [TestCase(false, 0)]
        public void ExpiryCleanup_IsUnconditionalWhenCounterAlreadyNonPositive(
            bool bodySkipped,
            int hp)
        {
            var runtime = new NTSDEntityRuntime
            {
                InputProxyCounter14C = 0,
                InputProxySourceSlot178 = 8,
                InputProxyEnabled17C = 1,
            };

            NTSD28InputProxyControlLifecycle.AdvanceReactionTail(
                runtime,
                bodySkipped,
                hp);

            Assert.That(runtime.InputProxyEnabled17C, Is.Zero);
            Assert.That(runtime.InputProxySourceSlot178, Is.EqualTo(8));
        }

        [Test]
        public void ExpiryCleanup_OnlyRecognizesEnabledValueOne()
        {
            var runtime = new NTSDEntityRuntime
            {
                InputProxyCounter14C = 0,
                InputProxySourceSlot178 = 8,
                InputProxyEnabled17C = 2,
            };

            NTSD28InputProxyControlLifecycle.AdvanceReactionTail(
                runtime,
                nativeEntityBodySkipped: true,
                currentHp: 0);

            Assert.That(runtime.InputProxyEnabled17C, Is.EqualTo(2));
        }

        [Test]
        public void WarmLifecycleCalls_AllocateZeroManagedBytes()
        {
            var attacker = new NTSDEntityRuntime { ObjType = 0, SlotIndex = 7 };
            var target = new NTSDEntityRuntime { ObjType = 0 };
            NTSD28InputProxyControlLifecycle.ApplyConfirmedMimicCounter(target, 2);
            NTSD28InputProxyControlLifecycle.TryEnableFromConfirmedHit(attacker, target);
            NTSD28InputProxyControlLifecycle.AdvanceReactionTail(
                target,
                nativeEntityBodySkipped: false,
                currentHp: 1);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                NTSD28InputProxyControlLifecycle.ApplyConfirmedMimicCounter(target, 2);
                NTSD28InputProxyControlLifecycle.TryEnableFromConfirmedHit(attacker, target);
                NTSD28InputProxyControlLifecycle.AdvanceReactionTail(
                    target,
                    nativeEntityBodySkipped: false,
                    currentHp: 1);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }
    }
}
#endif
