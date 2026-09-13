#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ResourceAttackerResolverEditorTests
    {
        [Test]
        public void NegativeOwner_ReturnsPhysicalAttacker()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            attacker.Runtime.OwnerSlotIndex = -1;

            LF2Entity resolved = BattleDamageWriter
                .ResolveNativeHitResourceAttacker(world, 0);

            Assert.That(resolved, Is.SameAs(attacker));
        }

        [Test]
        public void SelfOwner_ReturnsPhysicalAttacker()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            attacker.Runtime.OwnerSlotIndex = 0;

            LF2Entity resolved = BattleDamageWriter
                .ResolveNativeHitResourceAttacker(world, 0);

            Assert.That(resolved, Is.SameAs(attacker));
        }

        [Test]
        public void OneOwnerHop_ReturnsOwner()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            LF2Character owner = Create(world, 1);
            attacker.Runtime.OwnerSlotIndex = 1;
            owner.Runtime.OwnerSlotIndex = -1;

            LF2Entity resolved = BattleDamageWriter
                .ResolveNativeHitResourceAttacker(world, 0);

            Assert.That(resolved, Is.SameAs(owner));
        }

        [Test]
        public void TwoOwnerHops_ReturnRootOwner()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            LF2Character first = Create(world, 1);
            LF2Character root = Create(world, 2);
            attacker.Runtime.OwnerSlotIndex = 1;
            first.Runtime.OwnerSlotIndex = 2;
            root.Runtime.OwnerSlotIndex = -1;

            LF2Entity resolved = BattleDamageWriter
                .ResolveNativeHitResourceAttacker(world, 0);

            Assert.That(resolved, Is.SameAs(root));
        }

        [Test]
        public void ThirdOwnerHop_IsDeliberatelyNotFollowed()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            LF2Character first = Create(world, 1);
            LF2Character second = Create(world, 2);
            _ = Create(world, 3);
            attacker.Runtime.OwnerSlotIndex = 1;
            first.Runtime.OwnerSlotIndex = 2;
            second.Runtime.OwnerSlotIndex = 3;

            LF2Entity resolved = BattleDamageWriter
                .ResolveNativeHitResourceAttacker(world, 0);

            Assert.That(resolved, Is.SameAs(second));
        }

        [Test]
        public void MissingDeclaredOwnerAtEitherHop_FailsClosed()
        {
            var firstWorld = new SimulationWorld();
            LF2Character firstAttacker = Create(firstWorld, 0);
            firstAttacker.Runtime.OwnerSlotIndex = 9;

            Assert.That(BattleDamageWriter.ResolveNativeHitResourceAttacker(
                firstWorld, 0), Is.Null);

            var secondWorld = new SimulationWorld();
            LF2Character secondAttacker = Create(secondWorld, 0);
            LF2Character firstOwner = Create(secondWorld, 1);
            secondAttacker.Runtime.OwnerSlotIndex = 1;
            firstOwner.Runtime.OwnerSlotIndex = 9;

            Assert.That(BattleDamageWriter.ResolveNativeHitResourceAttacker(
                secondWorld, 0), Is.Null);
        }

        [Test]
        public void StableHolderAndRelationFields_CannotRedirectOwnerTraversal()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            _ = Create(world, 1);
            attacker.Runtime.OwnerSlotIndex = -1;
            attacker.Runtime.OwnerStableId = 1;
            attacker.Runtime.HolderStableId = 1;
            attacker.Runtime.RelationOwnerSlotIndex = 1;

            LF2Entity resolved = BattleDamageWriter
                .ResolveNativeHitResourceAttacker(world, 0);

            Assert.That(resolved, Is.SameAs(attacker));
        }

        private static LF2Character Create(SimulationWorld world, int slot)
        {
            var entity = new LF2Character { ObjectId = 8400 + slot };
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }
    }
}
#endif
