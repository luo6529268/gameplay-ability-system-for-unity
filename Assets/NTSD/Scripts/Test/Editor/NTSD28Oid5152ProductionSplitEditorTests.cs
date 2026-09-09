#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Oid5152ProductionSplitEditorTests
    {
        [Test]
        public void ProductionFusion_UsesPreDecrementTimerAndC25hCommitsAfterward()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildWrappers();
            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            try
            {
                SimulationWorld world = CreateMergeCandidate(
                    resolver,
                    wrappers,
                    out LF2Character self,
                    out _);
                self.Runtime.Unk338 = 1;

                world.Oid5152FusionScanAll(1);

                Assert.That(self.ObjectId, Is.EqualTo(7));
                Assert.That(self.Runtime.Unk338, Is.EqualTo(1));

                world.AdvanceOid5152ReactionTimerForDiagnostics(self);
                Assert.That(self.Runtime.Unk338, Is.Zero);

                world.Oid5152FusionScanAll(2);
                Assert.That(self.ObjectId, Is.EqualTo(51));
                Assert.That(self.Runtime.Unk338, Is.EqualTo(4500));

                world.AdvanceOid5152ReactionTimerForDiagnostics(self);
                Assert.That(self.Runtime.Unk338, Is.EqualTo(4499));
            }
            finally
            {
                resolver.SetOverrideForSelfCheck(null);
            }
        }

        [Test]
        public void CombinedDirectCompatibility_StillDecrementsBeforeFusion()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildWrappers();
            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            try
            {
                SimulationWorld world = CreateMergeCandidate(
                    resolver,
                    wrappers,
                    out LF2Character self,
                    out _);
                self.Runtime.Unk338 = 1;

                world.Oid5152RuntimeMaintenanceAll(1);

                Assert.That(self.ObjectId, Is.EqualTo(51));
                Assert.That(self.Runtime.Unk338, Is.EqualTo(4500));
            }
            finally
            {
                resolver.SetOverrideForSelfCheck(null);
            }
        }

        [Test]
        public void LateEntityTail_AdvancesPositiveTimerAfterFrameStep()
        {
            var world = new SimulationWorld();
            LF2OtherObject entity = CreateOther(world, 50, 970);
            entity.Runtime.Unk338 = 1;
            entity.FrameDelay = 0;

            world.LateEntityUpdateAll(2);

            Assert.That(entity.Runtime.Unk338, Is.Zero);
        }

        [Test]
        public void InputClearPartial_DoesNotAdvanceFusionTimer()
        {
            var world = new SimulationWorld();
            LF2OtherObject entity = CreateOther(world, 50, 971);
            entity.Runtime.Unk338 = 3;
            world.SetNeedClearInput(true);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                2,
                buildPresentation: false);

            Assert.That(entity.Runtime.Unk338, Is.EqualTo(3));
        }

        private static SimulationWorld CreateMergeCandidate(
            RuntimeCharacterConfigResolver resolver,
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> wrappers,
            out LF2Character self,
            out LF2Character partner)
        {
            var world = new SimulationWorld(resolver);
            self = CreateCharacter(wrappers[7], 7, 0);
            partner = CreateCharacter(wrappers[8], 8, 11);
            world.Register(self);
            world.Register(partner);
            self.ImmediateFrame(10);
            partner.ImmediateFrame(10);
            self.RelationTeam = 3;
            partner.RelationTeam = 3;
            self.Health.HP = 100;
            self.Health.HPBound = 100;
            self.Health.HP3 = 500;
            partner.Health.HP = 100;
            partner.Health.HPBound = 100;
            self.Runtime.SetPosition(120f, 0f, 5f);
            partner.Runtime.SetPosition(100f, 0f, 5f);
            self.Runtime.SyncIntegerPosition();
            partner.Runtime.SyncIntegerPosition();
            return world;
        }

        private static LF2Character CreateCharacter(
            LF2CharacterDataWrapper wrapper,
            int objectId,
            int slot)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = "Oid5152ProductionSplit_" + objectId;
            character.ObjectId = objectId;
            character.FrameCache.Load(wrapper);
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(slot);
            return character;
        }

        private static LF2OtherObject CreateOther(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>(),
            };
            var data = new LF2CharacterData
            {
                name = "Oid5152Timer_" + objectId,
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

        private static Dictionary<int, LF2CharacterDataWrapper> BuildWrappers()
        {
            return new Dictionary<int, LF2CharacterDataWrapper>
            {
                [7] = new LF2CharacterDataWrapper(7, BuildBaseData("Oid7")),
                [8] = new LF2CharacterDataWrapper(8, BuildBaseData("Oid8")),
                [51] = new LF2CharacterDataWrapper(51, BuildMergedData()),
            };
        }

        private static LF2CharacterData BuildBaseData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0),
                    Frame(10, 2),
                    Frame(112, 0),
                },
            };
        }

        private static LF2CharacterData BuildMergedData()
        {
            return new LF2CharacterData
            {
                name = "Oid51",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0),
                    Frame(112, 0),
                    Frame(290, 2),
                },
            };
        }

        private static LF2FrameData Frame(int frameId, int state)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
                centerx = 39,
                centery = 79,
                itrs = new List<InteractionArea>(),
            };
        }
    }
}
#endif
