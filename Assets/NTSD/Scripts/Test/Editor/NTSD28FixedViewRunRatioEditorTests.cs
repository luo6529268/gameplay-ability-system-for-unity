using NUnit.Framework;
using UnityEditor;

using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28FixedViewRunRatioEditorTests
    {
        [Test]
        public void ProductionReference_MatchesFormalRunFractionWithoutEditingDat()
        {
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(
                "Assets/NTSD/Config/GameConfig/GameConfig.asset");
            Assert.That(config, Is.Not.Null);
            Assert.That(config.BattleFixedViewRunReferenceWidthPx, Is.EqualTo(2048));
            Assert.That(config.BattleFixedViewRunReferenceHeightPx, Is.EqualTo(1152));

            var world = new SimulationWorld();
            world.ConfigureFixedViewRunDistance(config.BattleFixedViewRunReferenceWidthPx,
                config.BattleFixedViewRunReferenceHeightPx);

            Assert.That(48.0 * world.FixedViewRunDistanceScale / 2048.0,
                Is.EqualTo(48.0 / 1333.0).Within(1e-12));
            Assert.That(29.0 * world.FixedViewRunDistanceScale / 2048.0,
                Is.EqualTo(29.0 / 1333.0).Within(1e-12));
            Assert.That(3.3 * world.FixedViewRunVerticalDistanceScale / 1152.0,
                Is.EqualTo(3.3 / 730.0).Within(1e-12));
        }

        [Test]
        public void UnconfiguredDiagnosticWorld_KeepsFormalLogicalPixels()
        {
            var world = new SimulationWorld();
            Assert.That(world.FixedViewRunDistanceScale, Is.EqualTo(1.0));
            Assert.That(world.FixedViewRunVerticalDistanceScale, Is.EqualTo(1.0));
            world.ConfigureFixedViewRunDistance(1333, 730);
            Assert.That(world.FixedViewRunDistanceScale, Is.EqualTo(1.0));
            Assert.That(world.FixedViewRunVerticalDistanceScale, Is.EqualTo(1.0));
        }

        [Test]
        public void ConfiguredCharacterStep_ScalesTravelButKeepsRawPostFrictionMotion()
        {
            var world = new SimulationWorld();
            world.ConfigureFixedViewRunDistance(2048, 1152);
            var runtime = new NTSDEntityRuntime
            {
                Vx = 18.0,
                Vz = 3.3,
            };
            var context = new CharacterMechanicsContext(runtime, null, 0f, 0f, 0.0,
                world.FixedViewRunDistanceScale, world.FixedViewRunVerticalDistanceScale);

            new CharacterMechanics().StepBattleLogic(context);

            Assert.That(runtime.X, Is.EqualTo(18.0 * 2048.0 / 1333.0).Within(1e-10));
            Assert.That(runtime.Z, Is.EqualTo(3.3 * 1152.0 / 730.0).Within(1e-10));
            Assert.That(runtime.Vx, Is.EqualTo(17.0).Within(1e-10));
            Assert.That(runtime.Vz, Is.EqualTo(2.3).Within(1e-10));
        }

        [Test]
        public void ConfiguredNonCharacterStep_ScalesTravelButKeepsRawPostFrictionMotion()
        {
            var world = new SimulationWorld();
            world.ConfigureFixedViewRunDistance(2048, 1152);
            var runtime = new NTSDEntityRuntime
            {
                Vx = -12.0,
                Vz = -4.5,
            };

            CharacterMechanics.StepNonCharacterBattleLogic(runtime, 0.0,
                world.FixedViewRunDistanceScale, world.FixedViewRunVerticalDistanceScale);

            Assert.That(runtime.X, Is.EqualTo(-12.0 * 2048.0 / 1333.0).Within(1e-10));
            Assert.That(runtime.Z, Is.EqualTo(-4.5 * 1152.0 / 730.0).Within(1e-10));
            Assert.That(runtime.Vx, Is.EqualTo(-11.0).Within(1e-10));
            Assert.That(runtime.Vz, Is.EqualTo(-3.5).Within(1e-10));
        }
    }
}
