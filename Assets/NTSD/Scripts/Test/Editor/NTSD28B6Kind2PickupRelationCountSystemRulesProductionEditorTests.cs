#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6Kind2PickupRelationCountSystemRulesProductionEditorTests
    {
        [TestCase(120, 999, 1, 101, -1)]
        [TestCase(124, 999, 1, 101, -1)]
        [TestCase(100, 120, 1, 1, -1)]
        [TestCase(124, 124, 4, 4, -4)]
        [TestCase(120, 120, 4, 4, -4)]
        [TestCase(150, 150, 2, 2, -2)]
        [TestCase(123, 123, 6, 6, -6)]
        public void CurrentDatIdentity_SelectsOnlyType1LockedPromotion(int currentOid, int clrOid, int type, int holderRelation, int childRelation)
        {
            using (var scope = new Scope(currentOid, clrOid, type, 0, 5))
            {
                Assert.That(scope.World.InteractionWriter.TryApplyPickup(scope.Holder, scope.Child, 2), Is.True);
                Assert.That(scope.Holder.Runtime.LinkState, Is.EqualTo(holderRelation));
                Assert.That(scope.Child.Runtime.LinkState, Is.EqualTo(childRelation));
                Assert.That(scope.Holder.Runtime.PickupCount, Is.EqualTo(6));
            }
        }

        [TestCase(0, 5, 6)]
        [TestCase(0, 2147483646, 2147483647)]
        [TestCase(1, 5, 5)]
        [TestCase(2, 5, 5)]
        [TestCase(4, 5, 5)]
        [TestCase(6, 5, 5)]
        [TestCase(101, 5, 5)]
        [TestCase(-1, 5, 5)]
        public void RelationCount_UsesPreWriteRelation(int oldRelation, int count, int expected)
        {
            using (var scope = new Scope(100, 100, 1, oldRelation, count))
            {
                Assert.That(scope.World.InteractionWriter.TryApplyPickup(scope.Holder, scope.Child, 2), Is.True);
                Assert.That(scope.Holder.Runtime.PickupCount, Is.EqualTo(expected), "+35C only increments on prior relation zero.");
                Assert.That(scope.Holder.Runtime.LinkState, Is.EqualTo(1));
            }
        }

        [TestCase("locked", true)]
        [TestCase("permuted", true)]
        [TestCase("empty", false)]
        [TestCase("unaudited", false)]
        [TestCase("alternate", false)]
        public void LockedTableAdmission_IsExplicitAndImmutable(string variant, bool expected)
        {
            int[] ids = Table(variant);
            object rules = Rules(variant, ids);
            PropertyInfo supported = rules.GetType().GetProperty("IsSupported", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That((bool)supported.GetValue(rules), Is.EqualTo(expected));
            if (ids.Length > 0) ids[0] = 999;
            Assert.That((bool)supported.GetValue(rules), Is.EqualTo(expected), "Admission must not retain a mutable caller array.");
        }

        [TestCase("empty")]
        [TestCase("unaudited")]
        [TestCase("alternate")]
        public void UnsupportedRules_DoNotPartiallyWritePickup(string variant)
        {
            using (var scope = new Scope(120, 120, 1, 0, 5))
            {
                object rules = Rules(variant, Table(variant));
                MethodInfo method = typeof(BattleInteractionWriter).GetMethod("TryApplyPickup", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new[] { typeof(LF2Entity), typeof(LF2Entity), typeof(int), rules.GetType() }, null);
                Assert.That(method, Is.Not.Null, "Explicit locked-rule admission seam is missing.");
                string before = State(scope);
                Assert.That(method.Invoke(scope.World.InteractionWriter, new[] { (object)scope.Holder, scope.Child, 2, rules }), Is.EqualTo(false));
                Assert.That(State(scope), Is.EqualTo(before));
            }
        }

        private static int[] Table(string variant) => variant == "empty" ? Array.Empty<int>() : variant == "alternate" ? new[] { 120, 999 } : variant == "permuted" ? new[] { 124, 120 } : new[] { 120, 124 };
        private static object Rules(string variant, int[] ids)
        {
            Type type = typeof(BattleInteractionWriter).Assembly.GetType("NTSD.Simulation.Ecs.BattleLockedPickupWeaponThrowRules");
            Assert.That(type, Is.Not.Null, "The named locked pickup rule contract is missing.");
            MethodInfo method = type.GetMethod("FromAuditedTable", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(null, new object[] { variant != "unaudited", ids });
        }
        private static string State(Scope s) => $"{s.Holder.Frame.N}/{s.Holder.AttackingCounter}/{s.Holder.Runtime.LinkState}/{s.Holder.Runtime.PickupCount}/{s.Holder.Runtime.TargetSlotIndex}/{s.Child.Runtime.LinkState}/{s.Child.Runtime.HolderStableId}/{s.Child.Runtime.OwnerSlotIndex}/{s.Child.Runtime.WeaponFlightCounter}/{s.World.Rng.CallCount}";
        private sealed class Weapon : LF2Weapon
        {
            private readonly int type;
            internal Weapon(int type) { this.type = type; SetWeaponType(type); }
            public override int GetCurrentDataObjectTypeForSimulation() => type;
        }
        private sealed class Scope : IDisposable
        {
            internal readonly SimulationWorld World;
            internal readonly LF2Character Holder;
            internal readonly LF2Entity Child;
            internal Scope(int currentOid, int clrOid, int type, int oldRelation, int count)
            {
                var holderData = Data(9100, 0);
                var childData = Data(currentOid, type);
                World = new SimulationWorld(new RuntimeCharacterConfigResolver(id => id == 9100 ? holderData : childData));
                Holder = new LF2Character { ObjectId = 9100 };
                Holder.FrameCache.Load(holderData);
                Holder.SetRequiredRuntimeSlot(3);
                World.Register(Holder);
                Holder.ImmediateFrame(0);
                Holder.Initialize(500, 500);
                Child = new Weapon(type) { ObjectId = clrOid };
                Child.FrameCache.Load(childData);
                Child.SetRequiredRuntimeSlot(9);
                World.Register(Child);
                Child.DirectWriteHeldFramePreserveWaitCounter(0);
                Child.Health.HP = 100;
                Child.Runtime.WeaponFlightCounter = 31;
                Child.Runtime.OwnerSlotIndex = 17;
                Holder.Runtime.LinkState = oldRelation;
                Holder.Runtime.PickupCount = count;
                Holder.AttackingCounter = 7;
            }
            public void Dispose() { World.Unregister(Child); World.Unregister(Holder); }
            private static LF2CharacterDataWrapper Data(int oid, int type)
            {
                var data = new LF2CharacterData { type_sub = type, frames = new List<LF2FrameData>() };
                foreach (int frame in new[] { 0, 115, 116 })
                    data.frames.Add(new LF2FrameData { frameId = frame, wait = 100, next = frame, state = type == 0 ? 0 : 1004 });
                return new LF2CharacterDataWrapper(oid, data);
            }
        }
    }
}
#endif
