#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06FusionFeatureProjectionEditorTests
    {
        private static void Configure(SimulationWorld world, bool first, bool second)
        {
            var method = typeof(SimulationWorld).GetMethod("ConfigureFusionFeatureGates", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Missing explicit global feature configuration");
            method.Invoke(world, new object[] { first, second });
        }

        private static SimulationWorld World(out LF2Entity entity)
        {
            var wrapper = NTSD28Q06OpointWeaponHpEditorTests.Wrapper(5, false, 0);
            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(_ => wrapper));
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(777, 5, "feature.dat") }, _ => wrapper);
            entity = world.LogicEntityFactory.Create(new OPointCreateTask
            {
                targetWorld = world, dir = "right", preserveActionZero = true,
                opoint = new ObjectPoint { oid = 777, kind = 1, action = 0, hp = 41 }
            }, out var failure);
            Assert.That(entity, Is.Not.Null, failure.ToString());
            return world;
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ExplicitConfigurationProjectsOnlyFirstFlagAndCanReverse(bool first, bool second)
        {
            var world = World(out var entity);
            try
            {
                var raw = world.GetRawRuntimeSlotState(entity.Runtime.SlotIndex);
                Configure(world, first, second);
                Assert.That(world.Runtime.FusionFirstFeatureGate4A8428, Is.EqualTo(first));
                Assert.That(world.Runtime.FusionSecondFeatureGate4A842C, Is.EqualTo(second));
                Assert.That(entity.Runtime.FeatureGate4A8428, Is.EqualTo(first));
                Assert.That(raw.FeatureGate4A8428, Is.EqualTo(first));
                Configure(world, !first, second);
                Assert.That(entity.Runtime.FeatureGate4A8428, Is.EqualTo(!first));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void SuspendedPartnerAndEmptyRawSlotDoNotReceiveProjection()
        {
            var world = World(out var entity);
            try
            {
                entity.Runtime.OidMergeDormant = true;
                var empty = world.GetRawRuntimeSlotState(399);
                empty.FeatureGate4A8428 = false;
                Configure(world, true, true);
                Assert.That(entity.Runtime.FeatureGate4A8428, Is.False);
                Assert.That(world.GetRawRuntimeSlotState(entity.Runtime.SlotIndex).FeatureGate4A8428, Is.False);
                Assert.That(empty.FeatureGate4A8428, Is.False);
                entity.Runtime.OidMergeDormant = false;
                Configure(world, true, false);
                Assert.That(entity.Runtime.FeatureGate4A8428, Is.True);
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CommonTickProjectsGlobalsForMainAndWorkerEntry(bool worker)
        {
            var world = World(out var entity);
            try
            {
                world.Runtime.FusionFirstFeatureGate4A8428 = true;
                Assert.That(entity.Runtime.FeatureGate4A8428, Is.False);
                var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                world.ApplyFrameInputSet(input);
                var tick = new NTSDBattleTickSystem(world);
                if (worker) tick.RunSimulationWorkerTick(1, false);
                else tick.RunReleaseTick(1, false);
                Assert.That(entity.Runtime.FeatureGate4A8428, Is.True);
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void BirthAfterProjectionRetainsDefaultUntilNextBoundary()
        {
            var world = World(out var first);
            try
            {
                Configure(world, true, false);
                var child = world.LogicEntityFactory.Create(new OPointCreateTask
                {
                    targetWorld = world, dir = "right", preserveActionZero = true,
                    opoint = new ObjectPoint { oid = 777, kind = 1, action = 0, hp = 41 }
                }, out var failure);
                Assert.That(child, Is.Not.Null, failure.ToString());
                Assert.That(first.Runtime.FeatureGate4A8428, Is.True);
                Assert.That(child.Runtime.FeatureGate4A8428, Is.False);
                world.ApplyFrameInputSet(new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                Assert.That(child.Runtime.FeatureGate4A8428, Is.True);
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void RawScenarioUsesSameNamedGlobalConfigFields(bool first, bool second)
        {
            var type = typeof(NTSD.EditorTools.NTSD28UnityRawCaptureEditor);
            var scenarioType = type.GetNestedType("UnityRawScenario", BindingFlags.NonPublic);
            string json = "{\"seed\":1,\"combatants\":[],\"fusionFirstFeatureGate4A8428\":" +
                (first ? "true" : "false") + ",\"fusionSecondFeatureGate4A842C\":" + (second ? "true" : "false") + "}";
            object scenario = JsonUtility.FromJson(json, scenarioType);
            var world = new SimulationWorld();
            try
            {
                type.GetMethod("ConfigureWorldAndRoster", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] { world, new Dictionary<int, LF2CharacterDataWrapper>(), scenario, true });
                Assert.That(world.Runtime.FusionFirstFeatureGate4A8428, Is.EqualTo(first));
                Assert.That(world.Runtime.FusionSecondFeatureGate4A842C, Is.EqualTo(second));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }
    }
}
#endif
