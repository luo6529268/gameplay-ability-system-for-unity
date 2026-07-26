#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattlePresentationBeginFrameReuseEditorTests
    {
        [Test]
        public void BeginFrame_ReusedSortMatchesReference_WithEmptyHitRecords()
        {
            var world = new SimulationWorld();
            List<PresentationFixtureEntity> entities = RegisterFixtures(
                world,
                (101, 7, 220),
                (102, 2, 180),
                (103, 9, 180),
                (104, 4, 200));

            AssertMatchesIndependentReference(world, entities, 10);
        }

        [Test]
        public void BeginFrame_ReusedSortMatchesReference_WithMultipleHitRecords()
        {
            var world = new SimulationWorld();
            List<PresentationFixtureEntity> entities = RegisterFixtures(
                world,
                (201, 8, 240),
                (202, 3, 170),
                (203, 6, 170),
                (204, 1, 210));
            entities[0].AddHitRecord(0, 100, 210);
            entities[0].AddHitRecord(11, 101, 211);
            entities[1].AddHitRecord(22, 102, 212);
            entities[3].AddHitRecord(31, 103, 213);
            entities[3].AddHitRecord(38, 104, 214);

            AssertMatchesIndependentReference(world, entities, 20);
        }

        [Test]
        public void BeginFrame_RecollectsAndFiltersDormantPendingAndFuturePresentation()
        {
            var world = new SimulationWorld();
            List<PresentationFixtureEntity> entities = RegisterFixtures(
                world,
                (301, 5, 230),
                (302, 2, 180),
                (303, 7, 190),
                (304, 1, 210));
            entities[0].AddHitRecord(0, 120, 220);
            entities[1].AddHitRecord(10, 121, 221);
            entities[2].AddHitRecord(20, 122, 222);
            entities[3].AddHitRecord(30, 123, 223);

            AssertMatchesIndependentReference(world, entities, 5);

            entities[0].Runtime.OidMergeDormant = true;
            entities[1].Runtime.PendingFlushDestroy = true;
            entities[2].Runtime.FirstPresentationTick = 7;
            AssertMatchesIndependentReference(world, entities, 6);
            Assert.That(world.BattlePresentation.PublishedFrame.EntityCount, Is.EqualTo(1));
            Assert.That(world.BattlePresentation.PublishedFrame.GetEntity(0).StableId, Is.EqualTo(304));
            Assert.That(world.BattlePresentation.PublishedHitRecordCycle.OwnerCount, Is.EqualTo(1));
            Assert.That(world.BattlePresentation.PublishedHitRecordCycle.GetOwner(0).StableId, Is.EqualTo(304));

            entities[0].Runtime.OidMergeDormant = false;
            entities[1].Runtime.PendingFlushDestroy = false;
            AssertMatchesIndependentReference(world, entities, 7);
        }

        [Test]
        public void BeginFrame_ExceptionClearsScratchAndNextCaptureDoesNotLeak()
        {
            var world = new SimulationWorld();
            List<PresentationFixtureEntity> entities = RegisterFixtures(
                world,
                (401, 3, 180),
                (402, 6, 220));
            entities[0].AddHitRecord(0, 130, 230);
            entities[0].ThrowOnRenderSortingOrder = true;

            Assert.Throws<System.InvalidOperationException>(() =>
                world.BattlePresentation.BeginFrame(world, 30));
            Assert.That(GetCoordinatorScratchCount(world.BattlePresentation), Is.Zero);

            entities[0].ThrowOnRenderSortingOrder = false;
            entities[0].Runtime.OidMergeDormant = true;
            AssertMatchesIndependentReference(world, entities, 31);
            Assert.That(GetCoordinatorScratchCount(world.BattlePresentation), Is.Zero);
            Assert.That(world.BattlePresentation.PublishedFrame.EntityCount, Is.EqualTo(1));
            Assert.That(world.BattlePresentation.PublishedFrame.GetEntity(0).StableId, Is.EqualTo(402));
        }

        private static void AssertMatchesIndependentReference(
            SimulationWorld world,
            List<PresentationFixtureEntity> source,
            int tickIndex)
        {
            List<PresentationFixtureEntity> referenceEntities =
                BuildReferenceTraversal(source, tickIndex);
            List<PresentationFixtureEntity> referenceHitOwners =
                BuildReferenceTraversal(source, tickIndex);
            referenceHitOwners.RemoveAll(entity => entity.HitRecordCount <= 0);

            world.BattlePresentation.BeginFrame(world, tickIndex);

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            BattleHitRecordPresentationCycle cycle =
                world.BattlePresentation.PublishedHitRecordCycle;
            Assert.That(frame, Is.Not.Null);
            Assert.That(cycle, Is.Not.Null);
            Assert.That(frame.EntityCount, Is.EqualTo(referenceEntities.Count));
            Assert.That(cycle.OwnerCount, Is.EqualTo(referenceHitOwners.Count));

            int expectedHitRecordCount = 0;
            for (int index = 0; index < referenceEntities.Count; index++)
            {
                PresentationFixtureEntity expected = referenceEntities[index];
                BattlePresentationEntitySnapshot actual = frame.GetEntity(index);
                Assert.That(actual.StableId, Is.EqualTo(expected.StableId));
                Assert.That(actual.RuntimeSlot, Is.EqualTo(expected.Runtime.SlotIndex));
                Assert.That(actual.ZInt, Is.EqualTo(expected.Runtime.ZInt));
                Assert.That(actual.HitRecordStart, Is.EqualTo(expectedHitRecordCount));
                Assert.That(actual.HitRecordCount, Is.EqualTo(expected.HitRecordCount));
                expectedHitRecordCount += expected.HitRecordCount;
            }
            Assert.That(frame.HitRecordCount, Is.EqualTo(expectedHitRecordCount));
            Assert.That(cycle.HitRecordCount, Is.EqualTo(expectedHitRecordCount));

            int cycleHitIndex = 0;
            for (int ownerIndex = 0; ownerIndex < referenceHitOwners.Count; ownerIndex++)
            {
                PresentationFixtureEntity expected = referenceHitOwners[ownerIndex];
                BattleHitRecordOwnerSnapshot actualOwner = cycle.GetOwner(ownerIndex);
                Assert.That(actualOwner.StableId, Is.EqualTo(expected.StableId));
                Assert.That(actualOwner.RuntimeSlot, Is.EqualTo(expected.Runtime.SlotIndex));
                Assert.That(actualOwner.ZInt, Is.EqualTo(expected.Runtime.ZInt));
                Assert.That(actualOwner.HitRecordStart, Is.EqualTo(cycleHitIndex));
                Assert.That(actualOwner.HitRecordCount, Is.EqualTo(expected.HitRecordCount));
                for (int hitIndex = 0; hitIndex < expected.HitRecordCount; hitIndex++)
                {
                    BattlePresentationHitRecordSnapshot actualHit =
                        cycle.GetHitRecord(cycleHitIndex++);
                    Assert.That(actualHit.Age, Is.EqualTo(expected.GetHitRecordAge(hitIndex)));
                    Assert.That(actualHit.AnchorX, Is.EqualTo(expected.GetHitRecordX(hitIndex)));
                    Assert.That(actualHit.AnchorZ, Is.EqualTo(expected.GetHitRecordZ(hitIndex)));
                }
            }

            List<ExpectedCommand> expectedCommands =
                BuildReferenceCommands(referenceEntities, frame.CommonVisualCatalog);
            Assert.That(frame.CommandCount, Is.EqualTo(expectedCommands.Count));
            for (int commandIndex = 0; commandIndex < expectedCommands.Count; commandIndex++)
            {
                ExpectedCommand expected = expectedCommands[commandIndex];
                BattleRenderCommand actual = frame.GetCommand(commandIndex);
                Assert.That(actual.Type, Is.EqualTo(BattleRenderCommandType.HitRecord));
                Assert.That(actual.StableId, Is.EqualTo(expected.StableId));
                Assert.That(actual.RuntimeSlot, Is.EqualTo(expected.RuntimeSlot));
                Assert.That(actual.LocalSequence, Is.EqualTo(expected.HitRecordIndex));
                Assert.That(actual.EffectivePic, Is.EqualTo(expected.EffectivePic));
            }
        }

        private static List<PresentationFixtureEntity> BuildReferenceTraversal(
            List<PresentationFixtureEntity> source,
            int tickIndex)
        {
            var result = new List<PresentationFixtureEntity>(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                PresentationFixtureEntity entity = source[index];
                if (entity?.Runtime == null ||
                    entity.Runtime.SlotIndex < 0 ||
                    entity.Runtime.OidMergeDormant ||
                    entity.Runtime.PendingFlushDestroy ||
                    tickIndex < entity.Runtime.FirstPresentationTick)
                {
                    continue;
                }
                result.Add(entity);
            }
            result.Sort(CompareReferenceOrder);
            return result;
        }

        private static int GetCoordinatorScratchCount(BattlePresentationCoordinator coordinator)
        {
            var field = typeof(BattlePresentationCoordinator).GetField(
                "entityScratch",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            var scratch = (List<LF2Entity>)field.GetValue(coordinator);
            return scratch.Count;
        }

        private static List<ExpectedCommand> BuildReferenceCommands(
            List<PresentationFixtureEntity> entities,
            BattleCommonVisualCatalog commonVisualCatalog)
        {
            var result = new List<ExpectedCommand>();
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++)
            {
                PresentationFixtureEntity entity = entities[entityIndex];
                for (int hitIndex = 0; hitIndex < entity.HitRecordCount; hitIndex++)
                {
                    if (!BattleCommonVisualCatalog.TryResolveSparkAge(
                            entity.GetHitRecordAge(hitIndex),
                            out int pic) ||
                        commonVisualCatalog?.TryGetSpark(pic, out _) != true)
                    {
                        continue;
                    }
                    result.Add(new ExpectedCommand(
                        entity.StableId,
                        entity.Runtime.SlotIndex,
                        hitIndex,
                        pic));
                }
            }
            return result;
        }

        private static int CompareReferenceOrder(
            PresentationFixtureEntity left,
            PresentationFixtureEntity right)
        {
            int zComparison = (left?.Runtime?.ZInt ?? int.MaxValue)
                .CompareTo(right?.Runtime?.ZInt ?? int.MaxValue);
            if (zComparison != 0)
                return zComparison;

            int slotComparison = (left?.Runtime?.SlotIndex ?? int.MaxValue)
                .CompareTo(right?.Runtime?.SlotIndex ?? int.MaxValue);
            if (slotComparison != 0)
                return slotComparison;

            return (left?.StableId ?? int.MaxValue).CompareTo(
                right?.StableId ?? int.MaxValue);
        }

        private static List<PresentationFixtureEntity> RegisterFixtures(
            SimulationWorld world,
            params (int stableId, int runtimeSlot, int zInt)[] fixtures)
        {
            var result = new List<PresentationFixtureEntity>(fixtures.Length);
            for (int index = 0; index < fixtures.Length; index++)
            {
                (int stableId, int runtimeSlot, int zInt) fixture = fixtures[index];
                var entity = new PresentationFixtureEntity(fixture.stableId, fixture.zInt);
                entity.SetRequiredRuntimeSlot(fixture.runtimeSlot);
                world.Register(entity);
                Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(fixture.runtimeSlot));
                result.Add(entity);
            }
            return result;
        }

        private readonly struct ExpectedCommand
        {
            public ExpectedCommand(
                int stableId,
                int runtimeSlot,
                int hitRecordIndex,
                int effectivePic)
            {
                StableId = stableId;
                RuntimeSlot = runtimeSlot;
                HitRecordIndex = hitRecordIndex;
                EffectivePic = effectivePic;
            }

            public int StableId { get; }
            public int RuntimeSlot { get; }
            public int HitRecordIndex { get; }
            public int EffectivePic { get; }
        }

        private sealed class PresentationFixtureEntity : LF2Entity
        {
            public PresentationFixtureEntity(int stableId, int zInt)
            {
                StableId = stableId;
                ObjectId = 10000 + stableId;
                Team = 0;
                RelationTeam = 0;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 1;
                Health.HPBound = 1;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                Frame.D = new LF2FrameData
                {
                    frameId = 0,
                    state = 3005,
                    pic = 999,
                    wait = 1000000,
                    next = 0,
                };
                Runtime.LinkState = 0;
                Runtime.X = stableId;
                Runtime.Y = 0;
                Runtime.Z = zInt;
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }

            public bool ThrowOnRenderSortingOrder { get; set; }

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Other;

            public override int GetRenderSortingOrder()
            {
                if (ThrowOnRenderSortingOrder)
                    throw new System.InvalidOperationException("Injected presentation capture failure.");
                return base.GetRenderSortingOrder();
            }

            public override void Reset()
            {
            }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
            {
            }
        }
    }
}
#endif
