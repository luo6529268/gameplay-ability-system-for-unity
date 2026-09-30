#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q08KoMissingOwnerCreditEditorTests
    {
        [Test]
        public void MissingFirstOwner_KeepsPhysicalAttackerAsCredit()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            attacker.Runtime.OwnerSlotIndex = 9;

            Assert.That(BattleDamageWriter.ResolveNativeStandardHitCredit(world, attacker),
                Is.SameAs(attacker));
        }

        [Test]
        public void MissingSecondOwner_KeepsLastLiveOwnerAsCredit()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            LF2Character firstOwner = Create(world, 1);
            attacker.Runtime.OwnerSlotIndex = 1;
            firstOwner.Runtime.OwnerSlotIndex = 9;

            Assert.That(BattleDamageWriter.ResolveNativeStandardHitCredit(world, attacker),
                Is.SameAs(firstOwner));
        }

        [Test]
        public void RedirectedSourceWithMissingOwner_KeepsRedirectedSource()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            LF2Character source = Create(world, 2);
            attacker.Runtime.Kind4SourceCount92 = 1;
            attacker.Runtime.CatchSourceSlot90 = 2;
            source.Runtime.OwnerSlotIndex = 9;

            Assert.That(BattleDamageWriter.ResolveNativeStandardHitCredit(world, attacker),
                Is.SameAs(source));
        }

        [Test]
        public void MissingRedirectedSource_HasNoCredit()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            attacker.Runtime.Kind4SourceCount92 = 1;
            attacker.Runtime.CatchSourceSlot90 = 9;

            Assert.That(BattleDamageWriter.ResolveNativeStandardHitCredit(world, attacker),
                Is.Null);
        }

        [Test]
        public void ValidTwoHopChain_UsesSecondOwner()
        {
            var world = new SimulationWorld();
            LF2Character attacker = Create(world, 0);
            LF2Character firstOwner = Create(world, 1);
            LF2Character secondOwner = Create(world, 2);
            attacker.Runtime.OwnerSlotIndex = 1;
            firstOwner.Runtime.OwnerSlotIndex = 2;

            Assert.That(BattleDamageWriter.ResolveNativeStandardHitCredit(world, attacker),
                Is.SameAs(secondOwner));
        }

        private static LF2Character Create(SimulationWorld world, int slot)
        {
            var entity = new LF2Character { ObjectId = 8500 + slot };
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }
    }
}
#endif
