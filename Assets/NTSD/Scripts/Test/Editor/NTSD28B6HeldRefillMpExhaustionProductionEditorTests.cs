#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B6HeldRefillMpExhaustionProductionEditorTests
    {
        [Test]
        public void HpRefillExhaustion_UsesOneRandomXZeroYAndPreservesZ()
        {
            RefillScope scope = CreateScope(122, weaponHp: 1);
            scope.Holder.Health.HP = 100;
            scope.Holder.Health.HPBound = 200;
            scope.Holder.Health.HP3 = 500;
            scope.Holder.Health.PP = 100;
            scope.Weapon.Runtime.Vz = 6.5;

            int expectedVx = PredictSingleExhaustionVx(scope.World);
            ulong callsBefore = scope.World.Rng.CallCount;

            WeaponActResult result = scope.Run();

            Assert.That(result.ForceDrop, Is.True);
            Assert.That(scope.World.Rng.CallCount, Is.EqualTo(callsBefore + 1));
            Assert.That(scope.Weapon.Health.HP, Is.Zero);
            Assert.That(scope.Holder.Health.HPBound, Is.EqualTo(202));
            Assert.That(scope.Holder.Health.HP, Is.EqualTo(104));
            Assert.That(scope.Holder.Health.PP, Is.EqualTo(105));
            AssertNativeExhaustion(scope, expectedVx, 6.5);
        }

        [Test]
        public void MpRefillExhaustion_UsesOneRandomXZeroYAndPreservesZ()
        {
            RefillScope scope = CreateScope(123, weaponHp: 2);
            scope.Holder.Health.PP = 200;
            scope.Weapon.Health.PP = 400;
            scope.Weapon.Runtime.OrdinaryCreditGate2F4 = -1;
            scope.Weapon.Runtime.Vz = -4.25;

            int expectedVx = PredictSingleExhaustionVx(scope.World);
            ulong callsBefore = scope.World.Rng.CallCount;

            WeaponActResult result = scope.Run();

            Assert.That(result.ForceDrop, Is.True);
            Assert.That(scope.World.Rng.CallCount, Is.EqualTo(callsBefore + 1));
            Assert.That(scope.Weapon.Health.HP, Is.Zero);
            Assert.That(scope.Holder.Health.PP, Is.EqualTo(203));
            Assert.That(scope.Weapon.Health.PP, Is.EqualTo(400));
            AssertNativeExhaustion(scope, expectedVx, -4.25);
        }

        [TestCase(0, -2)]
        [TestCase(-4, -6)]
        public void MpRefill_NonPositiveEntryStillProcessesAndExhausts(
            int initialHp,
            int expectedHp)
        {
            RefillScope scope = CreateScope(123, initialHp);
            scope.Holder.Health.PP = 70;
            scope.Weapon.Health.PP = 123;
            scope.Weapon.Runtime.OrdinaryCreditGate2F4 = -1;
            scope.Weapon.Runtime.Vz = 9.0;
            int expectedVx = PredictSingleExhaustionVx(scope.World);

            WeaponActResult result = scope.Run();

            Assert.That(result.ForceDrop, Is.True);
            Assert.That(scope.Weapon.Health.HP, Is.EqualTo(expectedHp));
            Assert.That(scope.Holder.Health.PP, Is.EqualTo(73));
            Assert.That(scope.Weapon.Health.PP, Is.EqualTo(123));
            AssertNativeExhaustion(scope, expectedVx, 9.0);
        }

        [Test]
        public void MpRefill_NonnegativeOrdinaryGateCapsChildNotHolder()
        {
            RefillScope scope = CreateScope(123, weaponHp: 10);
            scope.Holder.Health.PP = 200;
            scope.Weapon.Health.PP = 500;
            scope.Weapon.Runtime.OrdinaryCreditGate2F4 = 0;
            scope.Weapon.KillCount = -1;
            ulong callsBefore = scope.World.Rng.CallCount;

            WeaponActResult result = scope.Run();

            Assert.That(result.ForceDrop, Is.False);
            Assert.That(scope.World.Rng.CallCount, Is.EqualTo(callsBefore));
            Assert.That(scope.Weapon.Health.HP, Is.EqualTo(8));
            Assert.That(scope.Holder.Health.PP, Is.EqualTo(203));
            Assert.That(scope.Weapon.Health.PP, Is.EqualTo(150));
            Assert.That(scope.Holder.Runtime.LinkState, Is.EqualTo(1));
            Assert.That(scope.Weapon.Runtime.LinkState, Is.EqualTo(-1));
        }

        [Test]
        public void MpRefill_NegativeOrdinaryGateIgnoresLegacyKillCount()
        {
            RefillScope scope = CreateScope(123, weaponHp: 10);
            scope.Holder.Health.PP = 200;
            scope.Weapon.Health.PP = 500;
            scope.Weapon.Runtime.OrdinaryCreditGate2F4 = -1;
            scope.Weapon.KillCount = 0;
            ulong callsBefore = scope.World.Rng.CallCount;

            WeaponActResult result = scope.Run();

            Assert.That(result.ForceDrop, Is.False);
            Assert.That(scope.World.Rng.CallCount, Is.EqualTo(callsBefore));
            Assert.That(scope.Weapon.Health.HP, Is.EqualTo(8));
            Assert.That(scope.Holder.Health.PP, Is.EqualTo(203));
            Assert.That(scope.Weapon.Health.PP, Is.EqualTo(500));
        }

        [Test]
        public void MpRefill_NonExhaustingPassKeepsRelationAndDoesNotDrawRng()
        {
            RefillScope scope = CreateScope(123, weaponHp: 20);
            scope.Holder.Health.PP = 499;
            scope.Weapon.Health.PP = 150;
            scope.Weapon.Runtime.OrdinaryCreditGate2F4 = 0;
            scope.Weapon.Runtime.Vz = 3.75;
            ulong callsBefore = scope.World.Rng.CallCount;

            WeaponActResult result = scope.Run();

            Assert.That(result.ForceDrop, Is.False);
            Assert.That(scope.World.Rng.CallCount, Is.EqualTo(callsBefore));
            Assert.That(scope.Weapon.Health.HP, Is.EqualTo(18));
            Assert.That(scope.Holder.Health.PP, Is.EqualTo(500));
            Assert.That(scope.Weapon.Health.PP, Is.EqualTo(150));
            Assert.That(scope.Weapon.Runtime.Vz, Is.EqualTo(3.75));
            Assert.That(scope.Holder.Runtime.LinkState, Is.EqualTo(1));
            Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.EqualTo(50));
            Assert.That(scope.Weapon.Runtime.LinkState, Is.EqualTo(-1));
            Assert.That(scope.Weapon.Runtime.HolderStableId, Is.Zero);
        }

        private static void AssertNativeExhaustion(
            RefillScope scope,
            int expectedVx,
            double expectedVz)
        {
            Assert.That(scope.Weapon.Runtime.Vx, Is.EqualTo(expectedVx));
            Assert.That(scope.Weapon.Runtime.Vy, Is.Zero);
            Assert.That(scope.Weapon.Runtime.Vz, Is.EqualTo(expectedVz));
            Assert.That(scope.Holder.Frame.N, Is.Zero);
            Assert.That(scope.Weapon.Frame.N, Is.Zero);
            Assert.That(scope.Holder.AttackingCounter, Is.Zero);
            Assert.That(scope.Weapon.AttackingCounter, Is.Zero);
            Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
            Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.Zero);
            Assert.That(scope.Weapon.Runtime.LinkState, Is.Zero);
            Assert.That(scope.Weapon.Runtime.HolderStableId, Is.Zero);
            Assert.That(scope.Weapon.Runtime.WeaponFlightCounter, Is.Zero);
        }

        private static int PredictSingleExhaustionVx(SimulationWorld world)
        {
            var expected = new DeterministicRng(world.Rng.State);
            return expected.NextInt(0, 7) - 3;
        }

        private static RefillScope CreateScope(int objectId, int weaponHp)
        {
            LF2FrameData holderFrame0 = Frame(0, LF2States.Standing);
            LF2FrameData holderFrame17 = Frame(17, 17);
            LF2FrameData weaponFrame0 = Frame(0, LF2States.Standing);
            LF2FrameData weaponFrame20 = Frame(20, LF2States.WeaponOnHand);
            var holderData = new LF2CharacterData
            {
                name = "B6HeldRefillHolder",
                type_sub = 0,
                frames = new List<LF2FrameData>
                {
                    holderFrame0,
                    holderFrame17,
                },
            };
            var weaponData = new LF2CharacterData
            {
                name = $"B6HeldRefill{objectId}",
                type_sub = objectId,
                weapon_hp = 31,
                frames = new List<LF2FrameData>
                {
                    weaponFrame0,
                    weaponFrame20,
                },
            };
            var holderWrapper = new LF2CharacterDataWrapper(9100, holderData);
            var weaponWrapper = new LF2CharacterDataWrapper(objectId, weaponData);
            var resolver = new RuntimeCharacterConfigResolver(id =>
                id == objectId ? weaponWrapper : holderWrapper);
            var world = new SimulationWorld(resolver);

            var holder = new ProbeHolder
            {
                Name = holderData.name,
                ObjectId = 9100,
            };
            holder.FrameCache.Load(holderWrapper);
            holder.SetRequiredRuntimeSlot(0);
            world.Register(holder);
            holder.ImmediateFrame(17);
            holder.Health.HP = 500;
            holder.Health.HPBound = 500;
            holder.Health.HP3 = 500;
            holder.Health.PP = 100;
            holder.AttackingCounter = 9;

            var weapon = new ProbeWeapon
            {
                Name = weaponData.name,
                ObjectId = objectId,
            };
            weapon.SetWeaponType((int)LF2ObjectType.Drink);
            weapon.FrameCache.Load(weaponWrapper);
            weapon.SetRequiredRuntimeSlot(50);
            world.Register(weapon);
            weapon.ImmediateFrame(20);
            weapon.Health.HP = weaponHp;
            weapon.Health.HPBound = weaponHp;
            weapon.Health.HP3 = 500;
            weapon.Health.PP = 500;
            weapon.Runtime.WeaponFlightCounter = 31;
            weapon.AttackingCounter = 8;

            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = 50;
            holder.Runtime.HeldWeaponStableId = 50;
            weapon.Runtime.LinkState = -1;
            weapon.Runtime.HolderStableId = 0;

            return new RefillScope(world, holder, weapon);
        }

        private static LF2FrameData Frame(int frameId, int state)
        {
            LF2FrameData frame = new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
                centerx = 20,
                centery = 30,
                itrs = new List<InteractionArea>(),
            };
            frame.wpoints = new List<WeaponPoint> { new WeaponPoint() };
            return frame;
        }

        private sealed class ProbeHolder : LF2Character
        {
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
        }

        private readonly struct RefillScope
        {
            internal RefillScope(
                SimulationWorld world,
                ProbeHolder holder,
                ProbeWeapon weapon)
            {
                World = world;
                Holder = holder;
                Weapon = weapon;
            }

            internal SimulationWorld World { get; }
            internal ProbeHolder Holder { get; }
            internal ProbeWeapon Weapon { get; }

            internal WeaponActResult Run()
            {
                return Weapon.Act(Holder, default, Vector3.zero);
            }
        }
    }
}
#endif
