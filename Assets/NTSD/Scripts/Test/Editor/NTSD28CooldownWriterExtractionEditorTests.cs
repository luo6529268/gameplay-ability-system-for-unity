#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28CooldownWriterExtractionEditorTests
    {
        [Test]
        public void CandidatePrelude_ClearsCanonicalAndMirrorWhenFrameHasNoItr()
        {
            var world = new SimulationWorld();
            LF2OtherObject entity = CreateOther(world, 50, 960, hasItr: false);
            entity.AttackExempt = 3;
            entity.ItrRest.Arest = 3;

            world.CaptureCollisionFrameSnapshotsAll();

            Assert.That(entity.AttackExempt, Is.Zero);
            Assert.That(entity.ItrRest.Arest, Is.Zero);
        }

        [Test]
        public void CandidatePrelude_RetainsAndSynchronizesRestWhenFrameCanHit()
        {
            var world = new SimulationWorld();
            LF2OtherObject entity = CreateOther(world, 50, 961, hasItr: true);
            entity.AttackExempt = 3;
            entity.ItrRest.Arest = 1;

            world.CaptureCollisionFrameSnapshotsAll();

            Assert.That(entity.AttackExempt, Is.EqualTo(3));
            Assert.That(entity.ItrRest.Arest, Is.EqualTo(3));
        }

        [Test]
        public void LateFrameTick_SynchronizesMirrorAfterCanonicalDecrement()
        {
            var world = new SimulationWorld();
            LF2OtherObject entity = CreateOther(world, 50, 962, hasItr: true);
            entity.AttackExempt = 1;
            entity.ItrRest.Arest = 1;
            entity.FrameDelay = 0;

            world.LateEntityUpdateAll(2);

            Assert.That(entity.AttackExempt, Is.Zero);
            Assert.That(entity.ItrRest.Arest, Is.Zero);
        }

        [Test]
        public void InputClearPartial_DoesNotAdvanceAttackerRest()
        {
            var world = new SimulationWorld();
            LF2OtherObject entity = CreateOther(world, 50, 963, hasItr: true);
            entity.AttackExempt = 1;
            entity.ItrRest.Arest = 1;
            world.SetNeedClearInput(true);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                2,
                buildPresentation: false);

            Assert.That(entity.AttackExempt, Is.EqualTo(1));
            Assert.That(entity.ItrRest.Arest, Is.EqualTo(1));
        }

        private static LF2OtherObject CreateOther(
            SimulationWorld world,
            int slot,
            int objectId,
            bool hasItr)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                itrs = hasItr
                    ? new List<InteractionArea> { new InteractionArea() }
                    : new List<InteractionArea>(),
            };
            var data = new LF2CharacterData
            {
                name = $"CooldownExtraction_{objectId}",
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { frame },
            };
            var entity = new LF2OtherObject
            {
                Name = data.name,
                ObjectId = objectId,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }
    }
}
#endif
