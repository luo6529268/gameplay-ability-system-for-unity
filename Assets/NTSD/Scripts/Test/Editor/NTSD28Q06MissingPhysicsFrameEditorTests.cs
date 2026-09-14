#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06MissingPhysicsFrameEditorTests
    {
        [TestCase(BattleEcsCharacterFrameAdvancePassMode.Legacy)]
        [TestCase(BattleEcsCharacterFrameAdvancePassMode.DataOriented)]
        public void MissingFramePreservesPhysicsAfterPendingHoldAndLinkRules(BattleEcsCharacterFrameAdvancePassMode mode)
        {
            var differences = new List<string>();
            int cases = 0;
            for (int type = 0; type < 7; type++)
            foreach (int action in new[] { -998, 999, 1000 })
            foreach (int hold in new[] { -2, 0, 2 })
            foreach (bool pending in new[] { false, true })
            foreach (int link in new[] { -1, 0 })
            {
                var world = MakeWorld(type, action, false, out var entity);
                try
                {
                    world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(mode);
                    entity.FrameDelay = hold; entity.Runtime.LinkState = link;
                    entity.Runtime.NativeLifecycleResolutionPending = pending;
                    world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                    int expectedHold = pending ? hold : hold > 0 ? hold - 1 : hold < 0 ? hold + 1 : 0;
                    if (entity.FrameDelay != expectedHold || entity.Runtime.X != 100.25 || entity.Runtime.Y != -10 || entity.Runtime.Z != 200 ||
                        entity.Runtime.Vx != 2.5 || entity.Runtime.Vy != -.5 || entity.Runtime.Vz != .75)
                        differences.Add(type + "/" + action + "/" + hold + "/" + pending + "/" + link + " moved or hold order differs");
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
                cases++;
            }
            const string output = "artifacts/diagnostics/NTSD28-Q06-NATIVE-PHYSICS-MISSING-FRAME-GUARD-001/";
            Directory.CreateDirectory(output);
            File.WriteAllText(output + mode + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(252));
            Assert.That(differences, Is.Empty);
        }

        [TestCase(BattleEcsCharacterFrameAdvancePassMode.Legacy, 998, false)]
        [TestCase(BattleEcsCharacterFrameAdvancePassMode.Legacy, 999, true)]
        [TestCase(BattleEcsCharacterFrameAdvancePassMode.DataOriented, 998, false)]
        [TestCase(BattleEcsCharacterFrameAdvancePassMode.DataOriented, 999, true)]
        public void ImplicitAndDeclaredNativeFramesStillIntegrate(BattleEcsCharacterFrameAdvancePassMode mode, int action, bool declared)
        {
            var world = MakeWorld(0, action, declared, out var entity);
            try
            {
                world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(mode);
                world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                Assert.That(entity.Runtime.X, Is.Not.EqualTo(100.25));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        private static SimulationWorld MakeWorld(int type, int action, bool declared, out LF2Entity entity)
        {
            var data = new LF2CharacterData { type_sub = type };
            data.frames.Add(new LF2FrameData { frameId = 0, wait = 100 });
            if (declared) data.frames.Add(new LF2FrameData { frameId = action, wait = 100 });
            var wrapper = new LF2CharacterDataWrapper(77, data);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, type, "physics-admission.dat") }, _ => wrapper);
            entity = type == 0 ? new LF2Character() : type == 3 ? new LF2SpecialAttack() : type == 5 ? new LF2OtherObject() : new LF2Weapon();
            if (entity is LF2Weapon weapon) weapon.SetWeaponType(type);
            entity.ObjectId = 77; entity.FrameCache.Load(wrapper);
            entity.SetRequiredRuntimeSlot(20); world.Register(entity);
            entity.DirectWriteNativeRawFramePreserveWaitCounter(action);
            entity.Runtime.SetPosition(100.25, -10, 200);
            entity.Runtime.SyncIntegerPosition(); entity.Runtime.SetVelocity(2.5, -.5, .75);
            entity.Health.HP = 500;
            return world;
        }
    }
}
#endif
