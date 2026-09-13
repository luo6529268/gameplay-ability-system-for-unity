#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6LegacyTrackerRetirementEditorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void FactoryFixture_ReachesKind2Relation(bool presentation)
        {
            WithSpawn(presentation, (parent, child) =>
            {
                AssertRelation(parent, child);
                bool carrierAbsent = typeof(LF2Entity).GetMember("TrackerFlag").Length == 0;
                Assert.That(carrierAbsent, Is.True);
                File.WriteAllText($"Temp/Goal20_R1_Reachability_{presentation}.json",
                    $"{{\"retiredFlagCarrierAbsent\":{carrierAbsent.ToString().ToLowerInvariant()},\"parentCachePresent\":{(child.TrackerParent != null).ToString().ToLowerInvariant()},\"parentLink\":{parent.Runtime.LinkState},\"childLink\":{child.Runtime.LinkState}}}");
            });
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Kind2Factory_PreservesRelationWithoutTrackerFlag(bool presentation)
        {
            WithSpawn(presentation, (parent, child) =>
            {
                AssertRelation(parent, child);
                Assert.That(typeof(LF2Entity).GetMember("TrackerFlag"), Is.Empty);
                Assert.That(parent.TrackerParent, Is.Null);
                Assert.That(child.TrackerParent, Is.Null);
            });
        }

        [Test]
        public void UnregisteredManagedCache_CannotSupplyRelationParent()
        {
            var parent = new Probe();
            var child = new Probe();
            parent.Runtime.SlotIndex = 77;
            child.Runtime.SlotIndex = 99;
            parent.Runtime.LinkState = 1;
            parent.Runtime.TargetSlotIndex = 99;
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = 77;
            child.TrackerParent = parent;
            Assert.That(child.ResolveLinkedParentFromRuntime(), Is.Null);
        }

        private static void AssertRelation(Probe parent, Probe child)
        {
            Assert.That(parent.Runtime.LinkState, Is.EqualTo(1));
            Assert.That(parent.Runtime.TargetSlotIndex, Is.EqualTo(99));
            Assert.That(parent.Runtime.HeldWeaponStableId, Is.EqualTo(99));
            Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(77));
            Assert.That(child.Team, Is.EqualTo(parent.Team));
        }

        private static void WithSpawn(bool presentation, System.Action<Probe, Probe> assert)
        {
            var parent = new Probe();
            var child = new Probe();
            parent.Runtime.SlotIndex = 77;
            child.Runtime.SlotIndex = 99;
            parent.Team = 4;
            var op = new ObjectPoint { kind = 2, oid = 213, action = 0 };
            GameObject host = null;
            try
            {
                if (presentation)
                {
                    host = new GameObject("Goal20R1FactoryFixture");
                    host.hideFlags = HideFlags.HideAndDontSave;
                    host.SetActive(false);
                    var factory = host.AddComponent<LF2ObjectPointFactory>();
                    typeof(LF2ObjectPointFactory).GetMethod("PostInitLiving",
                        BindingFlags.NonPublic | BindingFlags.Instance).Invoke(factory,
                        new object[] { child, parent, op, 3, 0f, false });
                }
                else
                {
                    BattleLogicEntityFactory.PostInitLiving(child, parent, op, 3, 0f, false);
                }
                assert(parent, child);
            }
            finally
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }
        }

        private sealed class Probe : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.SpecialAttack;
            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
            public override void Reset() { Runtime.Reset(); }
            public Probe()
            {
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                PS.BindRuntime(Runtime);
            }
        }
    }
}
#endif
