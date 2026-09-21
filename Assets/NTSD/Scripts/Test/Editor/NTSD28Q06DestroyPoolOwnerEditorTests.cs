#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06DestroyPoolOwnerEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-DESTROY-POOL-OWNER-001/";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void DestroyReturnsToOriginalWorldPool(int type)
            => Verify(type, false);

        internal static void VerifyForPlay(int type) => Verify(type, true);

        private static void Verify(int type, bool renderer)
        {
            var world = CreateWorld(type);
            var other = CreateWorld(type);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            other.SetLogicOnlyEntityMaterialization(!renderer);
            LF2Entity entity = null;
            try
            {
                entity = Spawn(world, renderer);
                var control = Spawn(other, renderer);
                if (renderer)
                {
                    Assert.That(entity.Renderer, Is.Not.Null);
                    Assert.That(control.Renderer, Is.Not.Null);
                }
                Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(1));
                Assert.That(other.LogicReferencePool.ActiveCount, Is.EqualTo(1));
                world.StructuralWriter.Destroy(entity);
                world.FlushPendingDestroyForDiagnostics();
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + (renderer ? "renderer-" : "type-") + type + ".json", JsonConvert.SerializeObject(new
                {
                    type, originalPoolActive = world.LogicReferencePool.ActiveCount,
                    controlPoolActive = other.LogicReferencePool.ActiveCount,
                    sourceSlotRemoved = world.FindEntityByRuntimeSlotForQuery(20) == null,
                    controlSlotPreserved = ReferenceEquals(other.FindEntityByRuntimeSlotForQuery(20), control)
                }, Formatting.Indented));
                Assert.That(world.FindEntityByRuntimeSlotForQuery(20), Is.Null);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero, "Original owner still holds destroyed entity");
                Assert.That(other.LogicReferencePool.ActiveCount, Is.EqualTo(1));
                Assert.That(other.FindEntityByRuntimeSlotForQuery(20), Is.SameAs(control));
                world.StructuralWriter.Destroy(entity);
                world.FlushPendingDestroyForDiagnostics();
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
                Assert.That(other.LogicReferencePool.ActiveCount, Is.EqualTo(1));
            }
            finally
            {
                if (renderer)
                {
                    world.FindEntityByRuntimeSlotForQuery(20)?.FreeEntityLikeExe();
                    other.FindEntityByRuntimeSlotForQuery(20)?.FreeEntityLikeExe();
                }
                // Reclaim this fixture's leaked borrow after a RED assertion; never hide it above.
                if (entity != null)
                    world.LogicReferencePool.Release(entity);
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var firstFailure), Is.True, firstFailure);
                other.BeginBattleShutdown();
                Assert.That(other.TryShutdownAndClearLogicState(out _, out var secondFailure), Is.True, secondFailure);
            }
        }

        private static SimulationWorld CreateWorld(int type)
        {
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            string root = Path.GetFullPath(Output + "fixture-runtime");
            const string dat = "<bmp_begin>\nname: Owner\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", "31980.dat"), BattleContentSource.ForLoganRuntime(root));
            var wrapper = new LF2CharacterDataWrapper(31980, data);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(31980, type, "owner.dat") },
                id => id == 31980 ? wrapper : null);
            return world;
        }

        private static LF2Entity Spawn(SimulationWorld world, bool renderer)
        {
            var task = new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = 20, dir = "right", preserveActionZero = true,
                opoint = new ObjectPoint { oid = 31980, action = 0 }
            };
            BattleLogicEntityCreationFailure failure = BattleLogicEntityCreationFailure.None;
            var entity = renderer
                ? LF2ObjectPointFactory.Instance.MaterializeObjectForStructuralWriter(task)
                : world.LogicEntityFactory.Create(task, out failure);
            Assert.That(entity, Is.Not.Null, failure.ToString());
            return entity;
        }
    }
}
#endif
