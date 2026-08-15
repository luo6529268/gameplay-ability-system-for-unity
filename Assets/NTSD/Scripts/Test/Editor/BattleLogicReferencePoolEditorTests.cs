#if UNITY_EDITOR
using NUnit.Framework;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class BattleLogicReferencePoolEditorTests
    {
        [Test]
        public void BattleCapacity_ReservesEveryConcreteEntityFamily()
        {
            AssertFamilyCapacity(LF2ObjectType.Character);
            AssertFamilyCapacity(LF2ObjectType.SpecialAttack);
            AssertFamilyCapacity(LF2ObjectType.Other);
            AssertFamilyCapacity(LF2ObjectType.HeavyWeapon);
        }

        [Test]
        public void WeaponFamily_ReusesOneSharedShellBudgetAndRetagsOnFetch()
        {
            var pool = new BattleLogicReferencePool();
            pool.PrepareBattleEntityShellCapacity(1);
            pool.SealBattleCapacity();

            ILF2Object heavy = pool.Get(LF2ObjectType.HeavyWeapon, 100);
            Assert.That(heavy, Is.Not.Null);
            Assert.That(heavy.ObjectTypeEnum, Is.EqualTo(LF2ObjectType.HeavyWeapon));
            pool.Release(heavy);

            ILF2Object drink = pool.Get(LF2ObjectType.Drink, 101);
            Assert.That(drink, Is.SameAs(heavy));
            Assert.That(drink.ObjectTypeEnum, Is.EqualTo(LF2ObjectType.Drink));
            Assert.That(drink.ObjectId, Is.EqualTo(101));
        }

        private static void AssertFamilyCapacity(LF2ObjectType objectType)
        {
            var pool = new BattleLogicReferencePool();
            pool.PrepareBattleEntityShellCapacity(3);
            pool.SealBattleCapacity();

            Assert.That(pool.Get(objectType, 1), Is.Not.Null);
            Assert.That(pool.Get(objectType, 2), Is.Not.Null);
            Assert.That(pool.Get(objectType, 3), Is.Not.Null);
            Assert.That(pool.Get(objectType, 4), Is.Null);
            Assert.That(pool.GetRejectedLogicObjectFetchCount(objectType), Is.EqualTo(1L));
        }
    }
}
#endif
