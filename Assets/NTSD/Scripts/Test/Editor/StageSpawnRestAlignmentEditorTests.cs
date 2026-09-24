#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("StageSpawnRestAlignment")]
    public sealed class StageSpawnRestAlignmentEditorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void StageFactoryAndColdBirthCarryRawAbsoluteSource(bool cold)
        {
            SimulationWorld world = CreateSourceBirthWorld();
            try
            {
                var spawn = new BattleStageSpawnValue(31983, 0, 137, 1, 300, -20, 0.0, 0);
                object module = typeof(SimulationWorld).GetField("stageWaveModule", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(world);
                MethodInfo spawnMethod = module.GetType().GetMethod(cold ? "TrySpawnStageCharacterDirect" : "TrySpawnStageEntityWithFactory",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var entity = (LF2Entity)spawnMethod.Invoke(module, new object[] { spawn, 300, -20, 250, "right", 137, 20 });
                Assert.That(entity, Is.Not.Null);
                AssertSourceMatchesBattle(entity);
                Assert.That(entity.Runtime.SourceRuleX, Is.EqualTo(300));
                Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(250));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void StageFinalPlacementKeepsRawAbsoluteSource(bool reserve)
        {
            SimulationWorld world = CreateSourceBirthWorld();
            try
            {
                int slot;
                if (reserve)
                {
                    Assert.That(world.TrySpawnResultsReserveEntry(31983, 0, 137, 20), Is.True);
                    slot = 20;
                }
                else
                {
                    var spawn = new BattleStageSpawnValue(31983, 0, 137, 1, 300, -20, 0.0, 0);
                    object module = typeof(SimulationWorld).GetField("stageWaveModule", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(world);
                    slot = (int)module.GetType().GetMethod("SpawnStageImmediateEntrySlot", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(module, new object[] { spawn });
                }
                Assert.That(slot, Is.GreaterThanOrEqualTo(20));
                AssertSourceMatchesBattle(world.FindEntityByRuntimeSlotForQuery(slot));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        private static SimulationWorld CreateSourceBirthWorld()
        {
            var data = new LF2CharacterData
            {
                name = "SourceBirthStage", type_sub = 0,
                frames = new List<LF2FrameData> { new LF2FrameData { frameId = 0, state = 0, wait = 1000, next = 0 } }
            };
            var wrapper = new LF2CharacterDataWrapper(31983, data);
            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(id => id == 31983 ? wrapper : null));
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(31983, 0, "source-stage-fixture.dat") },
                id => id == 31983 ? wrapper : null);
            world.Runtime.Stage.SetSceneSnapshot(800, 180, 350, 0, 0);
            world.ConfigureFixedViewRunDistance(2048, 1152);
            return world;
        }

        private static void AssertSourceMatchesBattle(LF2Entity entity)
        {
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Runtime.SourceRulePositionInitialized, Is.True);
            Assert.That(entity.Runtime.SourceRuleX, Is.EqualTo(entity.Runtime.X));
            Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(entity.Runtime.Z));
            Assert.That(entity.Runtime.SourceRuleXInt, Is.EqualTo(entity.Runtime.XInt));
            Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo(entity.Runtime.ZInt));
        }

        [Test]
        public void ReusedSlotResetTransaction_ClearsARestVictimRowAndAttackerColumn()
        {
            var store = new RuntimeRestStore(64);
            Assert.That(store.SetARest(20, 7), Is.True);
            Assert.That(store.SetARest(21, 5), Is.True);
            Assert.That(store.SetVRest(20, 21, 9), Is.True);
            Assert.That(store.SetVRest(21, 20, 11), Is.True);
            Assert.That(store.SetVRest(22, 23, 13), Is.True);

            Assert.That(
                store.TryResetSlotAndAcquireBinding(
                    20,
                    out RuntimeRestBindingHandle stageSpawnLease),
                Is.True);

            Assert.That(store.IsBindingValid(stageSpawnLease), Is.True);
            Assert.That(store.GetARest(20), Is.Zero);
            Assert.That(store.GetVRest(20, 21), Is.Zero);
            Assert.That(store.GetVRest(21, 20), Is.Zero);
            Assert.That(store.GetARest(21), Is.EqualTo(5));
            Assert.That(store.GetVRest(22, 23), Is.EqualTo(13));
            Assert.That(store.ReleaseBinding(stageSpawnLease), Is.True);
        }

        [Test]
        public void ConflictingLeaseResetTransaction_RejectsWithoutMutationOrLeaseInvalidation()
        {
            var store = new RuntimeRestStore(64);
            Assert.That(store.SetARest(20, 7), Is.True);
            Assert.That(store.SetVRest(20, 21, 9), Is.True);
            Assert.That(store.SetVRest(21, 20, 11), Is.True);
            Assert.That(
                store.TryAcquireBinding(20, out RuntimeRestBindingHandle foreignLease),
                Is.True);

            Assert.That(
                store.TryResetSlotAndAcquireBinding(20, out _),
                Is.False);

            Assert.That(store.IsBindingValid(foreignLease), Is.True);
            Assert.That(store.GetARest(20), Is.EqualTo(7));
            Assert.That(store.GetVRest(20, 21), Is.EqualTo(9));
            Assert.That(store.GetVRest(21, 20), Is.EqualTo(11));
            Assert.That(store.ReleaseBinding(foreignLease), Is.True);
        }
    }
}
#endif
