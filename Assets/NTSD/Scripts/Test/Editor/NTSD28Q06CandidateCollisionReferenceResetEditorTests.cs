#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06CandidateCollisionReferenceResetEditorTests
    {
        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void CandidateEntryClearsEveryActiveSlotWithoutFrameOrPair(
            BattleRuntimeProfile profile, bool directQuery)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            LF2Entity[] entities = { new LF2Character(), new LF2Weapon(), new LF2OtherObject() };
            int[] references = { -37, 41, 0 };
            try
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    entity.SetRequiredRuntimeSlot(i * 7);
                    world.Register(entity);
                    entity.Runtime.CollisionYReference = references[i];
                    entity.Runtime.Vy = 2.5;
                    Assert.That(world.FindEntityByRuntimeSlotForQuery(i * 7), Is.SameAs(entity));
                }
                for (int pass = 0; pass < 2; pass++)
                {
                    ((BruteForceSceneQuery)world.SceneQuery).SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(pass == 1);
                    if (directQuery)
                        ((BruteForceSceneQuery)world.SceneQuery).CollectCollisionCandidates();
                    else
                        world.CollectCollisionCandidatesAll();
                    foreach (var entity in entities)
                    {
                        Assert.That(entity.Runtime.CollisionYReference, Is.Zero);
                        Assert.That(entity.Runtime.Vy, Is.EqualTo(2.5));
                        entity.Runtime.CollisionYReference = -19;
                    }
                    world.EndCollisionCandidateConsumption();
                }
            }
            finally
            {
                foreach (var entity in entities)
                    world.Unregister(entity);
            }
        }

        [Test]
        public void ActivePendingLifecycleIsClearedButDormantAndRemovedArePreserved()
        {
            var world = new SimulationWorld();
            LF2Entity[] entities = { new LF2Character(), new LF2Character(), new LF2OtherObject() };
            try
            {
                world.CollectCollisionCandidatesAll();
                for (int i = 0; i < entities.Length; i++)
                {
                    entities[i].SetRequiredRuntimeSlot(i * 3);
                    world.Register(entities[i]);
                    entities[i].Runtime.CollisionYReference = -27;
                }
                entities[0].Runtime.NativeLifecycleResolutionPending = true;
                entities[1].Runtime.OidMergeDormant = true;
                entities[2].Runtime.PendingFlushDestroy = true;
                world.CollectCollisionCandidatesAll();
                Assert.That(entities[0].Runtime.CollisionYReference, Is.Zero);
                Assert.That(entities[1].Runtime.CollisionYReference, Is.EqualTo(-27));
                Assert.That(entities[2].Runtime.CollisionYReference, Is.EqualTo(-27));
            }
            finally
            {
                foreach (var entity in entities)
                {
                    entity.Runtime.OidMergeDormant = false;
                    entity.Runtime.PendingFlushDestroy = false;
                    world.Unregister(entity);
                }
            }
        }
    }
}
#endif
