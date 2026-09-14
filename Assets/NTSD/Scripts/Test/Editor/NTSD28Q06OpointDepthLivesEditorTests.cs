#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06OpointDepthLivesEditorTests
    {
        [TestCase(200.75, 200, -7)]
        [TestCase(200.75, 200, 0)]
        [TestCase(200.75, 200, 9)]
        [TestCase(-3.75, -3, -7)]
        [TestCase(-3.75, -3, 0)]
        [TestCase(-3.75, -3, 9)]
        public void LateOpointUsesNativeIntegerDepthAndFreshLives(double preciseZ, int integerZ, int pointZ)
        {
            var parentData = new LF2CharacterData { type_sub = 1 };
            parentData.frames.Add(new LF2FrameData
            {
                frameId = 0, wait = 100, next = 0,
                opoint = new BattleObjectPointValue(1, 0, 0, 0, 0, 0, 777, 0, z: pointZ),
            });
            var childData = new LF2CharacterData { type_sub = 3 };
            childData.frames.Add(new LF2FrameData { frameId = 0, wait = 100, next = 0 });
            var parent = new LF2CharacterDataWrapper(888, parentData);
            var child = new LF2CharacterDataWrapper(777, childData);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(888, 1, "parent.dat"), new ObjectDefinition(777, 3, "child.dat"),
            }, id => id == 888 ? parent : child);
            try
            {
                var source = new LF2Weapon { ObjectId = 888 };
                source.FrameCache.Load(parent);
                source.Frame.D = source.FrameCache.GetNativeFrameDataById(0);
                source.Trans.SyncDirectFrameData(100, 0, 0);
                source.SetRequiredRuntimeSlot(20); world.Register(source);
                source.Runtime.Z = preciseZ; source.Runtime.ZInt = integerZ;
                source.Health.HP = 500; source.Health.PP = 500;
                world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), source, 1);
                var spawned = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(spawned, Is.Not.Null);
                Assert.That(spawned.Runtime.Z, Is.EqualTo(integerZ + pointZ + 1));
                Assert.That(spawned.Runtime.ZInt, Is.EqualTo(integerZ + pointZ + 1));
                Assert.That(spawned.Runtime.HP2Orig, Is.EqualTo(1));
                Assert.That(spawned.Runtime.HPOrig, Is.Zero);
                Assert.That(spawned.Runtime.RespawnCount, Is.Zero);
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }
    }
}
#endif
