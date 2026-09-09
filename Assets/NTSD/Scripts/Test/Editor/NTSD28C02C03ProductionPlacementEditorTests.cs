#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C02C03ProductionPlacementEditorTests
    {
        [Test]
        public void ProductionProducerScan_ExecutesNonCharacterHitFaExactlyOnce()
        {
            var world = new SimulationWorld();
            ProducerProbeOther entity = CreateProbe(slot: 50, objectId: 950);
            world.Register(entity);

            world.NativeProducerSampleAndInputRouteAll(2);

            Assert.That(entity.FrameLogicRunCount, Is.EqualTo(1));
        }

        [Test]
        public void DirectCharacterInputEntry_RemainsCharacterOnly()
        {
            var world = new SimulationWorld();
            ProducerProbeOther entity = CreateProbe(slot: 50, objectId: 951);
            world.Register(entity);

            world.CharacterInputAll(2);

            Assert.That(entity.FrameLogicRunCount, Is.Zero);
        }

        private static ProducerProbeOther CreateProbe(int slot, int objectId)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                hit_Fa = 1,
            };
            var data = new LF2CharacterData
            {
                name = $"C02ProducerProbe_{objectId}",
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { frame },
            };
            var entity = new ProducerProbeOther
            {
                Name = data.name,
                ObjectId = objectId,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.SetRequiredRuntimeSlot(slot);
            return entity;
        }

        private sealed class ProducerProbeOther : LF2OtherObject
        {
            internal int FrameLogicRunCount { get; private set; }

            public override void RunFrameLogicBeforeAdvance()
            {
                FrameLogicRunCount++;
            }
        }
    }
}
#endif
