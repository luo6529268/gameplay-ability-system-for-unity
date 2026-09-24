using System;
using System.IO;

using NUnit.Framework;
using UnityEditor;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.DatParser;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28FixedViewRunRatioEditorTests
    {
        [TestCase(1, 3, 4)]
        [TestCase(2, 4, 5)]
        public void IndexedHitFa5MovedTarget_CharacterizesCurrentHistoryFirstDifference(
            int targetMoveTicks,
            int formalVx,
            int currentUnityVx)
        {
            string contentRoot = Path.GetFullPath(
                "Assets/NTSD/Content/LoganRuntime");
            string sourcePath = Path.Combine(contentRoot, "decoded_dat/w/e.dat");
            var sourceDefinition = new LF2CharacterDataWrapper(219,
                CharacterAnimtorManager.BuildCharacterDataFromSource(
                    File.ReadAllText(sourcePath), sourcePath,
                    BattleContentSource.ForLoganRuntime(contentRoot)));
            var targetData = new LF2CharacterData { type_sub = 0 };
            targetData.frames.Add(new LF2FrameData
            {
                frameId = 0, state = 0, wait = 100, next = 0,
            });
            var targetDefinition = new LF2CharacterDataWrapper(77, targetData);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(219, 3, "w/e.dat"),
                new ObjectDefinition(77, 0, "ratio-ally.dat")
            }, id => id == 219 ? sourceDefinition : id == 77 ? targetDefinition : null);
            try
            {
                var source = new LF2SpecialAttack { ObjectId = 219 };
                source.FrameCache.Load(sourceDefinition);
                source.Frame.D = source.FrameCache.GetNativeFrameDataById(51);
                Assert.That(source.Frame.D?.hit_Fa, Is.EqualTo(5));
                source.Trans.SyncDirectFrameData(1, 52, 51);
                source.SetRequiredRuntimeSlot(20);
                source.RelationTeam = 1;
                source.Health.HP = 500;
                world.Register(source);
                source.Runtime.SetPosition(100, 0, 200);
                source.Runtime.SyncIntegerPosition();

                var target = new LF2Character { ObjectId = 77 };
                target.ModuleInitialize();
                target.FrameCache.Load(targetDefinition);
                target.Initialize(500, 500);
                target.Frame.D = target.FrameCache.GetNativeFrameDataById(0);
                target.Trans.SyncDirectFrameData(100, 0, 0);
                target.SetRequiredRuntimeSlot(1);
                target.RelationTeam = 1;
                world.Register(target);
                target.Runtime.SetPosition(251, 0, 200);
                target.Runtime.SyncIntegerPosition();

                for (int tick = 0; tick < targetMoveTicks; tick++)
                {
                    target.Runtime.Vx = 48;
                    var motion = new CharacterMechanicsContext(target.Runtime,
                        null, 0f, 0f, 0.0, world.FixedViewRunDistanceScale,
                        world.FixedViewRunVerticalDistanceScale);
                    new CharacterMechanics().StepBattleLogic(motion);
                    target.Runtime.SyncIntegerPosition();
                }

                int formalGap = 151 + targetMoveTicks * 48;
                Assert.That(formalGap / 50, Is.EqualTo(formalVx));
                Assert.That(target.GetRuntimeXInt() - source.GetRuntimeXInt(),
                    Is.EqualTo(targetMoveTicks == 1 ? 224 : 298));
                source.RunFrameLogicBeforeAdvance();
                var child = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(child, Is.Not.Null);
                Assert.That(child.ObjectId, Is.EqualTo(219));
                // Characterization only: the current mixed-coordinate result is a known open D-024 difference.
                Assert.That(child.Runtime.Vx, Is.EqualTo(currentUnityVx));
                Assert.That(currentUnityVx, Is.GreaterThan(formalVx));
                double beforeX = child.Runtime.X;
                CharacterMechanics.StepNonCharacterBattleLogic(child.Runtime, 0,
                    world.FixedViewRunDistanceScale,
                    world.FixedViewRunVerticalDistanceScale);
                Assert.That((child.Runtime.X - beforeX) / 2048.0,
                    Is.EqualTo(currentUnityVx / 1333.0).Within(1e-12));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var reason),
                    Is.True, reason);
            }
        }

        [TestCase(false, 151)]
        [TestCase(true, 151)]
        [TestCase(false, -151)]
        [TestCase(true, -151)]
        [TestCase(true, 224)]
        public void IndexedHitFa5ChildUsesNativeIntegerVelocityAndOneViewFactor(
            bool configuredView,
            int targetDeltaX)
        {
            string contentRoot = Path.GetFullPath(
                "Assets/NTSD/Content/LoganRuntime");
            string sourcePath = Path.Combine(contentRoot, "decoded_dat/w/e.dat");
            var sourceDefinition = new LF2CharacterDataWrapper(219,
                CharacterAnimtorManager.BuildCharacterDataFromSource(
                    File.ReadAllText(sourcePath), sourcePath,
                    BattleContentSource.ForLoganRuntime(contentRoot)));
            var targetData = new LF2CharacterData { type_sub = 0 };
            targetData.frames.Add(new LF2FrameData
            {
                frameId = 0, state = 0, wait = 100, next = 0,
            });
            var targetDefinition = new LF2CharacterDataWrapper(77, targetData);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(219, 3, "w/e.dat"),
                new ObjectDefinition(77, 0, "ratio-ally.dat")
            }, id => id == 219 ? sourceDefinition : id == 77 ? targetDefinition : null);
            try
            {
                var source = new LF2SpecialAttack { ObjectId = 219 };
                source.FrameCache.Load(sourceDefinition);
                source.Frame.D = source.FrameCache.GetNativeFrameDataById(51);
                Assert.That(source.Frame.D?.hit_Fa, Is.EqualTo(5));
                source.Trans.SyncDirectFrameData(1, 52, 51);
                source.SetRequiredRuntimeSlot(20);
                source.RelationTeam = 1;
                source.Health.HP = 500;
                world.Register(source);
                source.Runtime.SetPosition(100, 0, 200);
                source.Runtime.SyncIntegerPosition();

                var target = new LF2Character { ObjectId = 77 };
                target.ModuleInitialize();
                target.FrameCache.Load(targetDefinition);
                target.Initialize(500, 500);
                target.Frame.D = target.FrameCache.GetNativeFrameDataById(0);
                target.Trans.SyncDirectFrameData(100, 0, 0);
                target.SetRequiredRuntimeSlot(1);
                target.RelationTeam = 1;
                world.Register(target);
                target.Runtime.SetPosition(100 + targetDeltaX, 0, 200);
                target.Runtime.SyncIntegerPosition();

                source.RunFrameLogicBeforeAdvance();

                var child = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(child, Is.Not.Null);
                Assert.That(child.ObjectId, Is.EqualTo(219));
                double expectedRawVx = targetDeltaX / 50;
                Assert.That(child.Runtime.Vx, Is.EqualTo(expectedRawVx));
                double beforeX = child.Runtime.X;
                CharacterMechanics.StepNonCharacterBattleLogic(child.Runtime, 0,
                    world.FixedViewRunDistanceScale,
                    world.FixedViewRunVerticalDistanceScale);
                double expectedStep = expectedRawVx *
                    (configuredView ? 2048.0 / 1333.0 : 1.0);
                Assert.That(child.Runtime.X - beforeX,
                    Is.EqualTo(expectedStep).Within(0.000001));
                Assert.That(Math.Abs(child.Runtime.X - beforeX) /
                    (configuredView ? 2048.0 : 1333.0),
                    Is.EqualTo(Math.Abs(expectedRawVx) / 1333.0).Within(1e-12));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var reason),
                    Is.True, reason);
            }
        }

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
