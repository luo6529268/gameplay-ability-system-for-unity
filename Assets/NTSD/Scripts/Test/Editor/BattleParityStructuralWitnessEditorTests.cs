using System.Linq;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Tests.Editor
{
    public sealed class BattleParityStructuralWitnessEditorTests
    {
        [Test]
        public void W03_RealLatePassEmitsHighSamePassLowNextPassAndLifecycleEvents()
        {
            BattleParityStructuralEventBuffer buffer =
                BattleParityTraceEditor.RunStructuralWitnessFixture("W03");

            BattleParityStructuralEvent[] tick1 = buffer.CaptureTick(1).ToArray();
            BattleParityStructuralEvent[] tick2 = buffer.CaptureTick(2).ToArray();
            Assert.That(tick1.Any(value =>
                value.Action == "scan" && value.Slot == 3), Is.True);
            Assert.That(tick1.Any(value =>
                value.Action == "allocate" && value.Slot == 0 &&
                value.LifecycleEpoch == 2), Is.True);
            Assert.That(tick1.Any(value =>
                value.Action == "unregister-deferred" && value.Slot == 0), Is.True);
            Assert.That(tick1.Any(value =>
                value.Action == "unregister-flush" && value.Slot == 0), Is.True);
            Assert.That(tick2.Any(value =>
                value.Action == "scan" && value.Slot == 0 &&
                value.LifecycleEpoch == 2), Is.True);
        }

        [Test]
        public void W04_RealRegistryEmitsAllocatorBandsAndAuthorityTail()
        {
            BattleParityStructuralEventBuffer buffer =
                BattleParityTraceEditor.RunStructuralWitnessFixture("W04");
            BattleParityStructuralEvent[] events = buffer.CaptureTick(1).ToArray();

            Assert.That(events.Any(value =>
                value.Action == "search" && value.SourceKind == "general" &&
                value.SearchStart == 0 && value.Slot == 0), Is.True);
            Assert.That(events.Any(value =>
                value.Action == "search" && value.SourceKind == "stage" &&
                value.SearchStart == 20 && value.Slot == 20), Is.True);
            Assert.That(events.Any(value =>
                value.Action == "search" && value.SourceKind == "dynamic" &&
                value.SearchStart == 50 && value.Slot == 50), Is.True);
            Assert.That(events.Any(value => value.Slot == 399), Is.True);
            Assert.That(events.All(value => value.Slot <= 399), Is.True);
        }

        [Test]
        public void W07_RealPositiveLinkValidationClearsOnlyLinkStateAndPreservesRelationFields()
        {
            BattleParityStructuralEventBuffer buffer =
                BattleParityTraceEditor.RunStructuralWitnessFixture("W07");

            BattleParityStructuralEvent kept = buffer.CaptureTick(2).Single(value =>
                value.Action == "link-validation");
            Assert.That(kept.Outcome, Is.EqualTo("kept"));
            Assert.That(kept.Reason, Is.EqualTo("reciprocal"));
            Assert.That(kept.BeforeLinkState, Is.EqualTo(1));
            Assert.That(kept.BeforeTargetSlot, Is.EqualTo(1));
            Assert.That(kept.BeforeHeldWeaponSlot, Is.EqualTo(1));
            Assert.That(kept.AfterLinkState, Is.EqualTo(1));
            Assert.That(kept.AfterTargetSlot, Is.EqualTo(1));
            Assert.That(kept.AfterHeldWeaponSlot, Is.EqualTo(1));
            Assert.That(kept.TargetActive, Is.True);
            Assert.That(kept.ObservedHolderSlot, Is.EqualTo(0));

            BattleParityStructuralEvent cleared = buffer.CaptureTick(3).Single(value =>
                value.Action == "link-validation");
            Assert.That(cleared.Outcome, Is.EqualTo("cleared"));
            Assert.That(cleared.Reason, Is.EqualTo("holder-mismatch"));
            Assert.That(cleared.AfterLinkState, Is.EqualTo(0));
            Assert.That(cleared.AfterTargetSlot, Is.EqualTo(1));
            Assert.That(cleared.AfterHeldWeaponSlot, Is.EqualTo(1));
            Assert.That(cleared.TargetBeforeHolderSlot, Is.EqualTo(2));
            Assert.That(cleared.TargetAfterHolderSlot, Is.EqualTo(2));
            Assert.That(cleared.TargetBeforeLinkState, Is.EqualTo(0));
            Assert.That(cleared.TargetAfterLinkState, Is.EqualTo(0));
        }

        [Test]
        public void W07_SinkOffDoesNotMaterializeStructuralLinkEvents()
        {
            var world = new SimulationWorld();
            var detachedSink = new BattleParityStructuralEventBuffer(400);
            world.SetStructuralEventSinkForDiagnostics(detachedSink, 0, "fixture-setup");
            world.SetStructuralEventSinkForDiagnostics(null, 0, "sink-off");

            var holder = new LinkValidationProbe(310);
            holder.SetRequiredRuntimeSlot(0);
            var target = new LinkValidationProbe(311);
            target.SetRequiredRuntimeSlot(1);
            world.Register(holder);
            world.Register(target);

            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = 1;
            holder.Runtime.HeldWeaponStableId = 1;
            target.Runtime.HolderStableId = 0;
            world.ValidateHeldLinksAll(1);

            Assert.That(detachedSink.Events, Is.Empty);
            Assert.That(holder.Runtime.LinkState, Is.EqualTo(1));
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(1));
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(1));
        }

        private sealed class LinkValidationProbe : NTSD.Animation.LF2Objects.LF2Entity
        {
            public LinkValidationProbe(int stableId)
            {
                StableId = stableId;
            }

            public override NTSD.Animation.LF2Objects.LF2ObjectType ObjectTypeEnum =>
                NTSD.Animation.LF2Objects.LF2ObjectType.Character;

            public override void Reset()
            {
            }

            public override void Init(
                NTSD.Animation.LF2Tasks.LF2TaskBase task,
                NTSD.Animation.LF2Objects.LF2ObjectRenderer renderer)
            {
                Renderer = renderer;
            }
        }
    }
}
