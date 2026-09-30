#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
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
