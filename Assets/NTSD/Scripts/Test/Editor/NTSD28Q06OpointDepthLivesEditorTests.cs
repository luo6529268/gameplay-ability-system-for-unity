#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q06OpointDepthLivesEditorTests
    {
        [TestCase(false, true, true, false)]
        [TestCase(false, false, true, false)]
        [TestCase(true, true, true, false)]
        [TestCase(true, false, true, false)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, false, false)]
        [TestCase(true, true, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, true, true)]
        [TestCase(false, false, true, true)]
        [TestCase(true, true, true, true)]
        [TestCase(true, false, true, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, false, false, true)]
        [TestCase(true, true, false, true)]
        [TestCase(true, false, false, true)]
        public void LateOpointRelativeBirthUsesConfiguredViewRatio(
            bool componentMaterializer,
            bool faceRight,
            bool configuredView,
            bool heldKind)
        {
            var parentData = new LF2CharacterData { type_sub = 1 };
            parentData.frames.Add(new LF2FrameData
            {
                frameId = 0, wait = 100, next = 0, centerx = 10,
                opoint = new BattleObjectPointValue(
                    heldKind ? 2 : 1, 20, 0, 0, 2, 0, 777, 0, z: 3),
            });
            var childData = new LF2CharacterData { type_sub = 3 };
            childData.frames.Add(new LF2FrameData { frameId = 0, wait = 100, next = 0 });
            var parent = new LF2CharacterDataWrapper(888, parentData);
            var child = new LF2CharacterDataWrapper(777, childData);
            var world = new SimulationWorld();
            GameObject componentHost = null;
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(888, 1, "parent.dat"),
                new ObjectDefinition(777, 3, "child.dat"),
            }, id => id == 888 ? parent : child);
            try
            {
                var source = new LF2Weapon { ObjectId = 888 };
                source.FrameCache.Load(parent);
                source.Frame.D = source.FrameCache.GetNativeFrameDataById(0);
                source.Trans.SyncDirectFrameData(100, 0, 0);
                source.SetRequiredRuntimeSlot(20);
                world.Register(source);
                source.Runtime.X = 200;
                source.Runtime.XInt = 200;
                source.Runtime.Z = 250;
                source.Runtime.ZInt = 250;
                source.Runtime.Dir = faceRight ? "right" : "left";
                source.Health.HP = 500;
                source.Health.PP = 500;

                if (componentMaterializer)
                {
                    componentHost = new GameObject("LateOpointRatioFixture")
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    componentHost.SetActive(false);
                    var factory = componentHost.AddComponent<LF2ObjectPointFactory>();
                    typeof(LF2ObjectPointFactory).GetMethod(
                        "ProcessOpointSpawnCoreForStructuralWriter",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(factory, new object[] { source });
                }
                else
                {
                    world.StructuralWriter.ProcessLateOpointSegment(
                        world.ResolveLateObjectPointStructuralMaterializerForModule(),
                        source,
                        1);
                }

                var spawned = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(spawned, Is.Not.Null);
                double scaleX = configuredView ? 2048.0 / 1333.0 : 1.0;
                double scaleZ = configuredView ? 1152.0 / 730.0 : 1.0;
                double expectedX = 200 + (faceRight ? 10 : -10) * scaleX;
                double expectedZ = 250 + 4 * scaleZ;
                Assert.That(spawned.Runtime.X,
                    Is.EqualTo(heldKind ? expectedX : (int)expectedX).Within(0.000001));
                Assert.That(spawned.Runtime.XInt, Is.EqualTo((int)expectedX));
                Assert.That(spawned.Runtime.Z,
                    Is.EqualTo(heldKind ? expectedZ : (int)expectedZ).Within(0.000001));
                Assert.That(spawned.Runtime.ZInt, Is.EqualTo((int)expectedZ));
                Assert.That(Math.Abs(spawned.Runtime.Vx), Is.EqualTo(2.0));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason),
                    Is.True, reason);
                if (componentHost != null)
                    UnityEngine.Object.DestroyImmediate(componentHost);
            }
        }

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
