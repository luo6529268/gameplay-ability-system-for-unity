#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleLockstepChecksumEditorTests
    {
        private static readonly ConstructorInfo BindingConstructor =
            typeof(BattleCommonVisualBinding).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleVisualResourceKey),
                    typeof(Sprite),
                    typeof(Texture2D),
                    typeof(Material),
                    typeof(Rect),
                    typeof(Rect),
                    typeof(Vector2),
                    typeof(Vector2),
                    typeof(BattleSpriteRenderState),
                },
                null);
        private static readonly ConstructorInfo CatalogConstructor =
            typeof(BattleCommonVisualCatalog).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleCommonVisualBinding),
                    typeof(BattleCommonVisualBinding[]),
                    typeof(Texture2D[]),
                    typeof(BattleCommonVisualBinding[][]),
                    typeof(string),
                },
                null);
        private static readonly MethodInfo ResetCycleMethod =
            typeof(BattleHitRecordPresentationCycle).GetMethod(
                "Reset",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo AddOwnerMethod =
            typeof(BattleHitRecordPresentationCycle).GetMethod(
                "AddOwner",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo AddHitRecordMethod =
            typeof(BattleHitRecordPresentationCycle).GetMethod(
                "AddHitRecord",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo PublishedCycleField =
            typeof(BattlePresentationCoordinator).GetField(
                "publishedHitRecordCycle",
                BindingFlags.Instance | BindingFlags.NonPublic);

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void PresentationFinalization_DoesNotChangeLockstepCoreChecksum(
            BattleRuntimeProfile profile)
        {
            SimulationWorld world = CreateWorld(profile);
            var entity = new LockstepFixtureEntity(7001);
            entity.SetRequiredRuntimeSlot(20);
            world.Register(entity);
            entity.AddHitRecord(0, 111, 222);

            IBattleChecksumSnapshot fullBefore = CaptureFull(world, profile, 41);
            BattleLockstepChecksumSnapshot lockstepBefore =
                world.CaptureLockstepChecksumSnapshot(41);
            string fullBeforeJson = fullBefore.ToJson();
            string lockstepBeforeJson = lockstepBefore.ToJson();

            PublishFinalizableCycle(world, entity, cycleId: 1, tickIndex: 41);
            Assert.That(world.BattlePresentation.FinalizePublishedHitRecordCycle(world), Is.True);
            Assert.That(world.BattlePresentation.FinalizePublishedHitRecordCycle(world), Is.False,
                "one published presentation cycle must finalize at most once");
            Assert.That(entity.GetHitRecordAge(0), Is.EqualTo(1));

            IBattleChecksumSnapshot fullAfter = CaptureFull(world, profile, 41);
            BattleLockstepChecksumSnapshot lockstepAfter =
                world.CaptureLockstepChecksumSnapshot(41);
            string fullAfterJson = fullAfter.ToJson();

            Assert.That(fullAfter.OverallChecksum, Is.Not.EqualTo(fullBefore.OverallChecksum),
                "the diagnostic/full snapshot must continue to witness presentation hit records");
            Assert.That(lockstepAfter.OverallChecksum, Is.EqualTo(lockstepBefore.OverallChecksum),
                "render-frame finalization cannot change the fixed-tick lockstep checksum");
            Assert.That(lockstepBefore.Schema, Is.EqualTo(BattleLockstepChecksumSnapshot.SchemaId));
            Assert.That(lockstepBefore.Schema, Is.Not.EqualTo(fullBefore.Schema));

            AssertFullSnapshotContainsHitRecords(fullBeforeJson);
            AssertFullSnapshotContainsHitRecords(fullAfterJson);
            Assert.That(lockstepBeforeJson, Does.Not.Contain("hitRecordCount"));
            Assert.That(lockstepBeforeJson, Does.Not.Contain("hitRecordDamage"));
            Assert.That(lockstepBeforeJson, Does.Not.Contain("hitRecordX"));
            Assert.That(lockstepBeforeJson, Does.Not.Contain("hitRecordZ"));
        }

        private static SimulationWorld CreateWorld(BattleRuntimeProfile profile)
        {
            return profile == BattleRuntimeProfile.Authority400
                ? new SimulationWorld()
                : new SimulationWorld(
                    BattleRuntimeProfile.MobileExtended,
                    BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
        }

        private static IBattleChecksumSnapshot CaptureFull(
            SimulationWorld world,
            BattleRuntimeProfile profile,
            int tickIndex)
        {
            return profile == BattleRuntimeProfile.Authority400
                ? world.CaptureParityFrameSnapshot(tickIndex)
                : world.CaptureExtendedChecksumSnapshot(tickIndex);
        }

        private static void PublishFinalizableCycle(
            SimulationWorld world,
            LF2Entity entity,
            int cycleId,
            int tickIndex)
        {
            Assert.That(BindingConstructor, Is.Not.Null);
            Assert.That(CatalogConstructor, Is.Not.Null);
            Assert.That(ResetCycleMethod, Is.Not.Null);
            Assert.That(AddOwnerMethod, Is.Not.Null);
            Assert.That(AddHitRecordMethod, Is.Not.Null);
            Assert.That(PublishedCycleField, Is.Not.Null);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    entity.Runtime.SlotIndex,
                    entity,
                    out RuntimeEntityHandle handle),
                Is.True);

            BattleCommonVisualCatalog catalog = CreateSparkCatalog();
            var cycle = new BattleHitRecordPresentationCycle();
            ResetCycleMethod.Invoke(cycle, new object[] { cycleId, tickIndex, catalog });
            AddOwnerMethod.Invoke(
                cycle,
                new object[]
                {
                    new BattleHitRecordOwnerSnapshot(
                        handle,
                        entity.StableId,
                        entity.Runtime.ZInt,
                        entity.Runtime.SlotIndex,
                        0,
                        0f,
                        0,
                        0,
                        1),
                });
            AddHitRecordMethod.Invoke(
                cycle,
                new object[] { new BattlePresentationHitRecordSnapshot(0, 111, 222) });
            PublishedCycleField.SetValue(world.BattlePresentation, cycle);
        }

        private static BattleCommonVisualCatalog CreateSparkCatalog()
        {
            var sparks = new BattleCommonVisualBinding[BattleCommonVisualCatalog.SparkFrameCount];
            var state = new BattleSpriteRenderState(
                Color.white,
                false,
                false,
                SpriteMaskInteraction.None,
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
            for (int pic = 0; pic < sparks.Length; pic++)
            {
                sparks[pic] = (BattleCommonVisualBinding)BindingConstructor.Invoke(
                    new object[]
                    {
                        BattleVisualResourceKey.CommonSpark(pic),
                        null,
                        null,
                        null,
                        BattleCommonVisualCatalog.GetSparkPixelRect(pic),
                        Rect.zero,
                        Vector2.zero,
                        BattleCommonVisualCatalog.GetSparkPivotNormalized(pic),
                        state,
                    });
            }

            return (BattleCommonVisualCatalog)CatalogConstructor.Invoke(
                new object[]
                {
                    null,
                    sparks,
                    Array.Empty<Texture2D>(),
                    Array.Empty<BattleCommonVisualBinding[]>(),
                    string.Empty,
                });
        }

        private static void AssertFullSnapshotContainsHitRecords(string json)
        {
            Assert.That(json, Does.Contain("hitRecordCount"));
            Assert.That(json, Does.Contain("hitRecordDamage"));
            Assert.That(json, Does.Contain("hitRecordX"));
            Assert.That(json, Does.Contain("hitRecordZ"));
        }

        private sealed class LockstepFixtureEntity : LF2Entity
        {
            public LockstepFixtureEntity(int stableId)
            {
                StableId = stableId;
                ObjectId = 10000 + stableId;
                Team = 1;
                RelationTeam = 1;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 500;
                Health.HPBound = 500;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                Frame.D = new LF2FrameData
                {
                    frameId = 0,
                    state = 3005,
                    pic = 0,
                    wait = 1000000,
                    next = 0,
                };
                Runtime.X = 10;
                Runtime.Y = 0;
                Runtime.Z = 20;
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Other;

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
