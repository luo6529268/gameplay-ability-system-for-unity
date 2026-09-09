#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NUnit.Framework;

using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28BattlePassOrderEditorTests
    {
        [Test]
        public void CanonicalOrder_ContainsAllFiftySevenCheckpointsInStableIndexOrder()
        {
            Assert.That(NTSD28BattlePassOrder.Count, Is.EqualTo(57));

            for (int index = 0; index < NTSD28BattlePassOrder.Count; index++)
            {
                NTSD28BattlePassDescriptor descriptor =
                    NTSD28BattlePassOrder.GetAt(index);
                Assert.That(descriptor.Id, Is.EqualTo((NTSD28BattlePassId)index));
                Assert.That(NTSD28BattlePassOrder.IndexOf(descriptor.Id), Is.EqualTo(index));
            }
        }

        [Test]
        public void Domains_PreserveSessionCoreNestedTailAndCompletedSnapshotBoundaries()
        {
            AssertDomainRange(0, 5, NTSD28BattlePassDomain.SessionPreCore);
            AssertDomainRange(6, 11, NTSD28BattlePassDomain.Core);
            AssertDomainRange(12, 13, NTSD28BattlePassDomain.PhysicsSlot);
            AssertDomainRange(14, 31, NTSD28BattlePassDomain.Core);
            AssertDomainRange(32, 47, NTSD28BattlePassDomain.SlotTail);
            AssertDomainRange(48, 48, NTSD28BattlePassDomain.Core);
            AssertDomainRange(49, 55, NTSD28BattlePassDomain.SessionPostCore);
            AssertDomainRange(56, 56, NTSD28BattlePassDomain.PresentationHandoff);

            NTSD28BattlePassDescriptor snapshot = NTSD28BattlePassOrder.GetAt(56);
            Assert.That(snapshot.Id, Is.EqualTo(NTSD28BattlePassId.PresentationSnapshotBuild));
            Assert.That(
                snapshot.Traversal,
                Is.EqualTo(NTSD28BattleTraversalKind.CompletedTickSnapshot));
            Assert.That(
                snapshot.HasFlag(NTSD28BattlePassFlags.CompletedTickHandoff),
                Is.True);
        }

        [Test]
        public void PhysicsAndDeadResourceNormalize_AreNestedPerSlotBetweenTeleportAndRevival()
        {
            NTSD28BattlePassId[] expected =
            {
                NTSD28BattlePassId.CoreFrameMotion,
                NTSD28BattlePassId.CoreTeleport,
                NTSD28BattlePassId.SlotPhysics,
                NTSD28BattlePassId.SlotDeadCharacterResourceNormalize,
                NTSD28BattlePassId.CoreRevival,
            };

            int first = NTSD28BattlePassOrder.IndexOf(expected[0]);
            Assert.That(first, Is.EqualTo(10));
            for (int offset = 0; offset < expected.Length; offset++)
            {
                NTSD28BattlePassDescriptor descriptor =
                    NTSD28BattlePassOrder.GetAt(first + offset);
                Assert.That(descriptor.Id, Is.EqualTo(expected[offset]));
            }

            NTSD28BattlePassDescriptor physics = NTSD28BattlePassOrder.GetAt(first + 2);
            NTSD28BattlePassDescriptor normalize = NTSD28BattlePassOrder.GetAt(first + 3);
            Assert.That(physics.Domain, Is.EqualTo(NTSD28BattlePassDomain.PhysicsSlot));
            Assert.That(normalize.Domain, Is.EqualTo(NTSD28BattlePassDomain.PhysicsSlot));
            Assert.That(
                physics.Traversal,
                Is.EqualTo(NTSD28BattleTraversalKind.NestedAscendingPhysicsSlotStep));
            Assert.That(normalize.NestedSlotTailStep, Is.EqualTo(1));
        }

        [Test]
        public void CollisionActionSnapshot_IsAfterPhysicsAndBeforeCandidateBuild()
        {
            int physics = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.SlotPhysics);
            int collisionSnapshot = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreCollisionActionSnapshot);
            int candidateBuild = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreCandidateBuild);
            int previousActionCommit = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.SlotPreviousActionCommit);

            Assert.That(physics, Is.LessThan(collisionSnapshot));
            Assert.That(collisionSnapshot, Is.LessThan(candidateBuild));
            Assert.That(candidateBuild, Is.LessThan(previousActionCommit));
            Assert.That(
                NTSD28BattlePassOrder.GetAt(collisionSnapshot).HasFlag(
                    NTSD28BattlePassFlags.ImmutableSnapshot),
                Is.True);
            Assert.That(
                NTSD28BattlePassOrder.GetAt(previousActionCommit).HasFlag(
                    NTSD28BattlePassFlags.ImmutableSnapshot),
                Is.False);
        }

        [Test]
        public void HeldRefill_RunsTwiceAndSecondPrecedesStageThenImpulse()
        {
            int first = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreHeldRefillBeforeGeometry);
            int hit = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreTypeZeroHitConsume);
            int impulse = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreHorizontalImpulseFinalize);
            int second = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreHeldRefillAfterHits);

            Assert.That(first, Is.LessThan(hit));
            int stage = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreStageSettlement);

            Assert.That(hit, Is.LessThan(second));
            Assert.That(second, Is.LessThan(stage));
            Assert.That(stage, Is.LessThan(impulse));
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void RandomWeaponDrop_UserExceptionRemainsBetweenTypeSeparatedHitLoops()
        {
            int typeZeroHit = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreTypeZeroHitConsume);
            int randomDrop = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreRandomWeaponDrop);
            int nonTypeZeroHit = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreNonTypeZeroHitConsume);
            int catchAdvance = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreCatchRelationAdvance);
            NTSD28BattlePassDescriptor descriptor =
                NTSD28BattlePassOrder.GetAt(randomDrop);

            Assert.That(randomDrop, Is.EqualTo(typeZeroHit + 1));
            Assert.That(nonTypeZeroHit, Is.EqualTo(randomDrop + 1));
            Assert.That(catchAdvance, Is.EqualTo(nonTypeZeroHit + 1));
            Assert.That(
                descriptor.Traversal,
                Is.EqualTo(NTSD28BattleTraversalKind.UserAcceptedException));
            Assert.That(
                descriptor.HasFlag(NTSD28BattlePassFlags.UserAcceptedException),
                Is.True);
        }

        [Test]
        public void SlotTail_IsOneNestedAscendingTransactionWithSixteenOrderedSteps()
        {
            NTSD28BattlePassId[] expected =
            {
                NTSD28BattlePassId.SlotDefinitionTransition,
                NTSD28BattlePassId.SlotSpecialStateCloneMaterialize,
                NTSD28BattlePassId.SlotNativeResourcePreDisplay,
                NTSD28BattlePassId.SlotDisplayValues,
                NTSD28BattlePassId.SlotNativeResourcePostDisplay,
                NTSD28BattlePassId.SlotComputerStateRefresh,
                NTSD28BattlePassId.SlotFrameStep,
                NTSD28BattlePassId.SlotReactionTimers,
                NTSD28BattlePassId.SlotArmorRecovery,
                NTSD28BattlePassId.SlotAttackerRestDecrement,
                NTSD28BattlePassId.SlotFrameZeroOpoint,
                NTSD28BattlePassId.SlotState18BrokenWeaponParticles,
                NTSD28BattlePassId.SlotPreviousActionCommit,
                NTSD28BattlePassId.SlotWeaponPieceFragments,
                NTSD28BattlePassId.SlotPendingLifecycleResolve,
                NTSD28BattlePassId.SlotHealing,
            };

            int first = NTSD28BattlePassOrder.IndexOf(expected[0]);
            Assert.That(first, Is.EqualTo(32));
            for (int step = 0; step < expected.Length; step++)
            {
                NTSD28BattlePassDescriptor descriptor =
                    NTSD28BattlePassOrder.GetAt(first + step);
                Assert.That(descriptor.Id, Is.EqualTo(expected[step]));
                Assert.That(descriptor.Domain, Is.EqualTo(NTSD28BattlePassDomain.SlotTail));
                Assert.That(
                    descriptor.Traversal,
                    Is.EqualTo(NTSD28BattleTraversalKind.NestedAscendingLiveSlotStep));
                Assert.That(descriptor.NestedSlotTailStep, Is.EqualTo(step));
                Assert.That(
                    descriptor.HasFlag(NTSD28BattlePassFlags.NestedSlotTail),
                    Is.True);
            }
        }

        [Test]
        public void SessionFunctionKeyEffects_RunAfterCoreAndBeforeCompletedSnapshot()
        {
            int comboExpire = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreComboExpire);
            int objectEffect = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.SessionFunctionKeyObjectEffect);
            int fullMp = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.SessionFunctionKeyFullMpEffect);
            int snapshot = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.PresentationSnapshotBuild);

            Assert.That(comboExpire, Is.LessThan(objectEffect));
            Assert.That(fullMp, Is.EqualTo(objectEffect + 1));
            Assert.That(fullMp, Is.LessThan(snapshot));
            Assert.That(snapshot, Is.EqualTo(NTSD28BattlePassOrder.Count - 1));
        }

        [Test]
        public void QueryApi_FailsClosedForInvalidIndexesAndPassIds()
        {
            Assert.That(NTSD28BattlePassOrder.TryGet(-1, out _), Is.False);
            Assert.That(
                NTSD28BattlePassOrder.TryGet(NTSD28BattlePassOrder.Count, out _),
                Is.False);
            Assert.That(
                NTSD28BattlePassOrder.IndexOf((NTSD28BattlePassId)255),
                Is.EqualTo(-1));
            Assert.That(
                NTSD28BattlePassOrder.IsBefore(
                    (NTSD28BattlePassId)255,
                    NTSD28BattlePassId.CoreInputPhaseAdvance),
                Is.False);
        }

        [Test]
        public void QueryApi_RemainsAllocationFreeAfterWarmup()
        {
            _ = NTSD28BattlePassOrder.GetAt(0);
            _ = NTSD28BattlePassOrder.TryGet(1, out _);
            _ = NTSD28BattlePassOrder.IndexOf(
                NTSD28BattlePassId.CoreCollisionActionSnapshot);
            _ = NTSD28BattlePassOrder.IsBefore(
                NTSD28BattlePassId.CoreFrameMotion,
                NTSD28BattlePassId.SlotPhysics);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                int order = index % NTSD28BattlePassOrder.Count;
                NTSD28BattlePassDescriptor descriptor =
                    NTSD28BattlePassOrder.GetAt(order);
                NTSD28BattlePassOrder.TryGet(order, out NTSD28BattlePassDescriptor copy);
                checksum ^= (int)descriptor.Id;
                checksum ^= (int)copy.Domain << 8;
                checksum ^= NTSD28BattlePassOrder.IndexOf(copy.Id) << 16;
                if (NTSD28BattlePassOrder.IsBefore(
                        NTSD28BattlePassId.CoreFrameMotion,
                        NTSD28BattlePassId.PresentationSnapshotBuild))
                {
                    checksum ^= index;
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(checksum, Is.Not.EqualTo(int.MinValue));
        }

        private static void AssertDomainRange(
            int first,
            int last,
            NTSD28BattlePassDomain expected)
        {
            for (int index = first; index <= last; index++)
            {
                Assert.That(
                    NTSD28BattlePassOrder.GetAt(index).Domain,
                    Is.EqualTo(expected));
            }
        }
    }
}
#endif
