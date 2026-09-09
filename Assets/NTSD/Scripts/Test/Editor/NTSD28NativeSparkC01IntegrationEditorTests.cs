#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28NativeSparkC01IntegrationEditorTests
    {
        [Test]
        public void ExistingRecord_AdvancesAtC01WithoutVisualCatalog()
        {
            SimulationWorld world = CreateWorld(out SparkTickFixtureEntity entity);
            entity.AddHitRecord(0, 10, 20);
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(entity.HitRecordCount, Is.EqualTo(1));
            Assert.That(entity.GetHitRecordAge(0), Is.EqualTo(1));
            Assert.That(diagnostics.TryGetLastPhaseAt(0, out BattleTickPhase first), Is.True);
            Assert.That(first, Is.EqualTo(BattleTickPhase.BattleFlow));
            Assert.That(diagnostics.TryGetLastPhaseAt(1, out BattleTickPhase second), Is.True);
            Assert.That(second, Is.EqualTo(BattleTickPhase.NativeSparkAdvance));
            Assert.That(diagnostics.TryGetLastPhaseAt(2, out BattleTickPhase third), Is.True);
            Assert.That(third, Is.EqualTo(BattleTickPhase.HumanInput));
        }

        [Test]
        public void TerminalTail_IsPoppedAtC01WithoutVisualCatalog()
        {
            SimulationWorld world = CreateWorld(out SparkTickFixtureEntity entity);
            entity.AddHitRecord(9, 10, 20);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(entity.HitRecordCount, Is.Zero);
        }

        [Test]
        public void RecordCreatedDuringSameTick_RemainsAtBaseId()
        {
            SimulationWorld world = CreateWorld(out SparkTickFixtureEntity entity);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        1,
                        (int)LF2ObjectType.Other,
                        "native-spark-c01-test.dat"),
                },
                _ => null,
                BattleHitRecordLifecycleCatalog.Available);
            entity.AddRecordDuringSimTu = true;

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(entity.HitRecordCount, Is.EqualTo(1));
            Assert.That(entity.GetHitRecordAge(0), Is.Zero);
            Assert.That(entity.RecordAddedTick, Is.EqualTo(1));
        }

        [Test]
        public void PresentationCapture_FreezesPostC01StateWithoutSecondAdvance()
        {
            SimulationWorld world = CreateWorld(out SparkTickFixtureEntity entity);
            entity.AddHitRecord(0, 10, 20);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: true);

            Assert.That(entity.GetHitRecordAge(0), Is.EqualTo(1));
            Assert.That(
                world.BattlePresentation.PublishedHitRecordCycle,
                Is.Not.Null);
            Assert.That(
                world.BattlePresentation.PublishedHitRecordCycle.HitRecordCount,
                Is.EqualTo(1));
            Assert.That(
                world.BattlePresentation.PublishedHitRecordCycle.GetHitRecord(0).Age,
                Is.EqualTo(1));
        }

        [Test]
        public void PresentationAcknowledgement_IsReadOnlyAndClosesLegacyWriteback()
        {
            SimulationWorld world = CreateWorld(out SparkTickFixtureEntity entity);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        1,
                        (int)LF2ObjectType.Other,
                        "native-spark-c01-test.dat"),
                },
                _ => null,
                BattleHitRecordLifecycleCatalog.Available);
            world.BattlePresentation.SetMode(BattlePresentationBackendMode.CentralOnly);
            entity.AddHitRecord(5, 10, 20);
            world.BattlePresentation.BeginSimulationWorkerFrame(world, tickIndex: 1);

            Assert.That(
                world.BattlePresentation.AcknowledgePublishedHitRecordCycle(),
                Is.True);
            Assert.That(entity.GetHitRecordAge(0), Is.EqualTo(5));
            Assert.That(
                world.BattlePresentation.AcknowledgePublishedHitRecordCycle(),
                Is.False);
            Assert.That(
                world.BattlePresentation.FinalizePublishedHitRecordCycle(world),
                Is.False,
                "A production acknowledgement must prevent compatibility writeback from re-entering the cycle.");
            Assert.That(entity.GetHitRecordAge(0), Is.EqualTo(5));
        }

        [Test]
        public void WorkerAndSynchronousTick_UseTheSameC01Owner()
        {
            SimulationWorld syncWorld = CreateWorld(out SparkTickFixtureEntity syncEntity);
            SimulationWorld workerWorld = CreateWorld(out SparkTickFixtureEntity workerEntity);
            syncEntity.AddHitRecord(0, 10, 20);
            workerEntity.AddHitRecord(0, 10, 20);

            new NTSDBattleTickSystem(syncWorld).RunReleaseTick(
                1,
                buildPresentation: false);
            BattleTickCompletion completion =
                new NTSDBattleTickSystem(workerWorld).RunSimulationWorkerTick(
                    1,
                    buildPresentation: false);

            Assert.That(completion, Is.EqualTo(BattleTickCompletion.FullReturn));
            Assert.That(syncEntity.GetHitRecordAge(0), Is.EqualTo(1));
            Assert.That(workerEntity.GetHitRecordAge(0), Is.EqualTo(1));
        }

        private static SimulationWorld CreateWorld(
            out SparkTickFixtureEntity entity)
        {
            var world = new SimulationWorld();
            entity = new SparkTickFixtureEntity(9101);
            entity.SetRequiredRuntimeSlot(20);
            world.Register(entity);
            return world;
        }

        private sealed class SparkTickFixtureEntity : LF2Entity
        {
            public SparkTickFixtureEntity(int stableId)
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

            public bool AddRecordDuringSimTu { get; set; }
            public int RecordAddedTick { get; private set; } = -1;

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Other;
            }

            public override void SimTU(int tickIndex)
            {
                if (!AddRecordDuringSimTu || RecordAddedTick >= 0)
                    return;

                AddHitRecord(0, 30, 40);
                RecordAddedTick = tickIndex;
            }

            public override void Reset()
            {
                ResetSpark();
                AddRecordDuringSimTu = false;
                RecordAddedTick = -1;
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }
    }
}
#endif
