#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Simulation;
using NTSD.Animation.LF2Objects;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28D024UnifiedSpatialProjectionEditorTests
    {
        [Test]
        public void WorldConfiguration_KeepsExistingScaleReadersOnOneProjection()
        {
            var world = new SimulationWorld();
            Assert.That(world.FixedViewRunDistanceScale, Is.EqualTo(1.0));
            Assert.That(world.FixedViewRunVerticalDistanceScale, Is.EqualTo(1.0));

            world.ConfigureFixedViewRunDistance(2048, 1152);
            Assert.That(world.SpatialProjection.HorizontalScale,
                Is.EqualTo(2048.0 / 1333.0));
            Assert.That(world.SpatialProjection.DepthScale,
                Is.EqualTo(1152.0 / 730.0));
            Assert.That(world.FixedViewRunDistanceScale,
                Is.EqualTo(world.SpatialProjection.HorizontalScale));
            Assert.That(world.FixedViewRunVerticalDistanceScale,
                Is.EqualTo(world.SpatialProjection.DepthScale));

            world.ConfigureFixedViewRunDistance(800, 550);
            Assert.That(world.SpatialProjection.HorizontalScale, Is.EqualTo(1.0));
            Assert.That(world.SpatialProjection.DepthScale, Is.EqualTo(1.0));
        }

        [TestCase(500.0)]
        [TestCase(1020.0)]
        [TestCase(-400.0)]
        public void CommonAnchor_MapsNearOverlapAndFarGapTogether(double anchorX)
        {
            BattleSpatialProjection projection =
                BattleSpatialProjection.FromReferenceViewport(2048, 1152);

            double hanItrLeft = projection.SourceToViewX(536, anchorX);
            double hanItrRight = projection.SourceToViewX(561, anchorX);
            double nearBodyLeft = projection.SourceToViewX(502, anchorX);
            double nearBodyRight = projection.SourceToViewX(545, anchorX);
            double farBodyLeft = projection.SourceToViewX(562, anchorX);

            Assert.That(hanItrLeft, Is.LessThan(nearBodyRight));
            Assert.That(hanItrRight, Is.GreaterThan(nearBodyLeft));
            Assert.That(nearBodyRight - hanItrLeft,
                Is.EqualTo(9.0 * 2048.0 / 1333.0).Within(1e-9));
            Assert.That(farBodyLeft - hanItrRight,
                Is.EqualTo(2048.0 / 1333.0).Within(1e-9));
            Assert.That(farBodyLeft, Is.GreaterThan(hanItrRight));

            foreach (double sourceX in new[] { -100.75, 500.0, 520.0, 580.0 })
            {
                double viewX = projection.SourceToViewX(sourceX, anchorX);
                Assert.That(projection.ViewToSourceX(viewX, anchorX),
                    Is.EqualTo(sourceX).Within(1e-9));
            }
        }

        [Test]
        public void DepthUsesItsOwnViewRatioAndRoundTripsAroundSharedAnchor()
        {
            BattleSpatialProjection projection =
                BattleSpatialProjection.FromReferenceViewport(2048, 1152);
            const double anchorZ = 265.0;
            Assert.That(projection.SourceDeltaToViewX(48),
                Is.EqualTo(48.0 * 2048.0 / 1333.0));
            Assert.That(projection.SourceDeltaToViewZ(48),
                Is.EqualTo(48.0 * 1152.0 / 730.0));
            Assert.That(projection.SourceDeltaToViewX(-48),
                Is.EqualTo(-48.0 * 2048.0 / 1333.0));
            Assert.That(projection.ViewToSourceZ(
                    projection.SourceToViewZ(180.5, anchorZ), anchorZ),
                Is.EqualTo(180.5).Within(1e-9));
        }

        [Test]
        public void GlobalHeightUsesViewHeightRatioWithoutChangingApprovedSpriteScale()
        {
            BattleSpatialProjection projection =
                BattleSpatialProjection.FromReferenceViewport(2048, 1152);
            double expectedHeightDelta = 2.0 * 1152.0 / 730.0;

            Assert.That(projection.SourceDeltaToViewY(2),
                Is.EqualTo(expectedHeightDelta).Within(1e-12));
            Assert.That(BattleSpatialProjection.Identity.SourceDeltaToViewY(2),
                Is.EqualTo(2.0));

            UnityEngine.Vector2 before = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500, -22, 402f, 0f, 0, 0, 21, false,
                60f, 80f, 30f, 60f, 1.5f, projection.VerticalScale);
            UnityEngine.Vector2 after = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                500, -20, 402f, 0f, 0, 0, 23, false,
                60f, 80f, 30f, 60f, 1.5f, projection.VerticalScale);
            Assert.That(after.y - before.y,
                Is.EqualTo(expectedHeightDelta).Within(1e-4));
            Assert.That(after.x, Is.EqualTo(before.x));
        }

        [Test]
        public void OrdinaryBodyYEndpointsProjectOnceAndFullHeightStaysSentinel()
        {
            var world = new SimulationWorld();
            world.ConfigureFixedViewRunDistance(2048, 1152);
            var normalBody = new BattleBodyBoxValue(-10, -5, 20, 10);
            var fullBody = new BattleBodyBoxValue(-101, int.MinValue, 900, 0);
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                centerx = 10,
                centery = 10,
                bodies = new List<BattleBodyBoxValue> { normalBody, fullBody },
            };
            var data = new LF2CharacterData
            {
                type_sub = 0,
                frames = new List<LF2FrameData> { frame },
            };
            var entity = new LF2Character { ObjectId = 8810 };
            entity.SetRequiredRuntimeSlot(0);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(8810, data));
            entity.ImmediateFrame(0);
            world.Register(entity);
            entity.Runtime.YInt = -20;

            Assert.That(BruteForceSceneQuery.TryBuildBodyBattleVolume(
                entity, frame, normalBody, out PhysicsState.BattleVolume normal), Is.True);
            double scale = 1152.0 / 730.0;
            int expectedTop = (int)Math.Truncate(-35.0 * scale);
            int expectedBottom = (int)Math.Truncate(-25.0 * scale);
            Assert.That(normal.y, Is.EqualTo(expectedTop));
            Assert.That(normal.h, Is.EqualTo(expectedBottom - expectedTop));

            Assert.That(BruteForceSceneQuery.TryBuildBodyBattleVolume(
                entity, frame, fullBody, out PhysicsState.BattleVolume full), Is.True);
            Assert.That(full.y, Is.EqualTo(-1000000000f));
            Assert.That(full.h, Is.EqualTo(2000000000f));
        }

        [Test]
        public void InstanceAnchorConversions_KeepCurrentZeroAndAllowOneSharedOrigin()
        {
            BattleSpatialProjection current =
                BattleSpatialProjection.FromReferenceViewport(2048, 1152);
            Assert.That(current.SharedAnchorX, Is.EqualTo(0.0));
            Assert.That(current.SharedAnchorZ, Is.EqualTo(0.0));
            Assert.That(current.SourceToViewX(520),
                Is.EqualTo(current.SourceToViewX(520, 0.0)));
            Assert.That(current.ViewToSourceZ(631.2328767123288),
                Is.EqualTo(current.ViewToSourceZ(631.2328767123288, 0.0)));

            BattleSpatialProjection shifted =
                BattleSpatialProjection.FromReferenceViewport(2048, 1152, 500, 400);
            Assert.That(shifted.SourceToViewX(500), Is.EqualTo(500));
            Assert.That(shifted.SourceToViewZ(400), Is.EqualTo(400));
            Assert.That(shifted.SourceToViewX(520),
                Is.EqualTo(shifted.SourceToViewX(520, 500)));
            Assert.That(shifted.ViewToSourceX(shifted.SourceToViewX(520)),
                Is.EqualTo(520).Within(1e-9));
            Assert.That(shifted.ViewToSourceZ(shifted.SourceToViewZ(410)),
                Is.EqualTo(410).Within(1e-9));
        }
    }
}
#endif
