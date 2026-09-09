#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C12FusionBarrierPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_SerialRemainderObservesC12FusionResult()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildWrappers();
            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null);
            try
            {
                var world = new SimulationWorld(resolver);
                FusionObserver self = CreateCharacter<FusionObserver>(world, wrappers[7], 7, 0, 120);
                _ = CreateCharacter<PassiveCharacter>(world, wrappers[8], 8, 11, 100);

                new NTSDBattleTickSystem(world).RunReleaseTick(71, buildPresentation: false);

                Assert.That(self.PostSerialCount, Is.EqualTo(1));
                Assert.That(self.ObservedOidInSerial, Is.EqualTo(51));
                Assert.That(self.ObjectId, Is.EqualTo(51));
                Assert.That(self.Runtime.Unk338, Is.EqualTo(4499));
            }
            finally
            {
                resolver.SetOverrideForSelfCheck(null);
            }
        }

        [Test]
        public void FullTick_RecordsC12AfterCandidateBeforeSerial()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(72, buildPresentation: false);

            Assert.That(diagnostics.TryGetLastPhaseAt(12, out BattleTickPhase candidate), Is.True);
            Assert.That(candidate, Is.EqualTo(BattleTickPhase.CandidateCollect));
            Assert.That(diagnostics.TryGetLastPhaseAt(13, out BattleTickPhase fusion), Is.True);
            Assert.That(fusion, Is.EqualTo(BattleTickPhase.RuntimeMaintenance));
            Assert.That(diagnostics.TryGetLastPhaseAt(14, out BattleTickPhase weaponCount), Is.True);
            Assert.That(weaponCount, Is.EqualTo(BattleTickPhase.ActiveWeaponCount));
            Assert.That(diagnostics.TryGetLastPhaseAt(15, out BattleTickPhase hit), Is.True);
            Assert.That(hit, Is.EqualTo(BattleTickPhase.CharacterHitConsumePostInteraction));
            Assert.That(diagnostics.TryGetLastPhaseAt(28, out BattleTickPhase serial), Is.True);
            Assert.That(serial, Is.EqualTo(BattleTickPhase.FrameAdvance));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        private static T CreateCharacter<T>(
            SimulationWorld world,
            LF2CharacterDataWrapper wrapper,
            int objectId,
            int slot,
            double x)
            where T : LF2Character, new()
        {
            var character = new T();
            character.ModuleInitialize();
            character.Name = "C12_" + objectId;
            character.ObjectId = objectId;
            character.RelationTeam = 3;
            character.FrameCache.Load(wrapper);
            character.Frame.N = 10;
            character.Frame.PN = 10;
            character.Frame.D = character.FrameCache.GetFrameDataById(10);
            character.Initialize(500, 500);
            character.Health.HP = 100;
            character.Health.HPBound = 100;
            character.Health.HP3 = 500;
            character.Runtime.SetPosition(x, 0.0, 200.0);
            character.Runtime.SyncIntegerPosition();
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            character.Frame.N = 10;
            character.Frame.PN = 10;
            character.Frame.D = character.FrameCache.GetFrameDataById(10);
            character.Runtime.SetPosition(x, 0.0, 200.0);
            character.Runtime.SyncIntegerPosition();
            return character;
        }

        private static Dictionary<int, LF2CharacterDataWrapper> BuildWrappers()
        {
            return new Dictionary<int, LF2CharacterDataWrapper>
            {
                [7] = new LF2CharacterDataWrapper(7, BaseData("C12Oid7")),
                [8] = new LF2CharacterDataWrapper(8, BaseData("C12Oid8")),
                [51] = new LF2CharacterDataWrapper(51, new LF2CharacterData
                {
                    name = "C12Oid51",
                    frames = new List<LF2FrameData> { Frame(0, 0), Frame(290, 2) },
                }),
            };
        }

        private static LF2CharacterData BaseData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0),
                    Frame(9, 2),
                    Frame(10, 2),
                    Frame(11, 2),
                    Frame(12, 2),
                },
            };
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
                centerx = 39,
                centery = 79,
                itrs = new List<InteractionArea>(),
            };
        }

        private sealed class FusionObserver : LF2Character
        {
            internal int PostSerialCount { get; private set; }
            internal int ObservedOidInSerial { get; private set; } = -1;

            internal override bool RunNativePhysicsForWorldPass(int tickIndex) => true;

            internal override void RunCharacterInputProducerPhaseForKnownCharacterDat(
                int tickIndex)
            {
            }

            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(
                int tickIndex,
                bool applyFrameMotionTail = true)
            {
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedOidInSerial = ObjectId;
            }
        }

        private sealed class PassiveCharacter : LF2Character
        {
            internal override bool RunNativePhysicsForWorldPass(int tickIndex) => true;

            internal override void RunCharacterInputProducerPhaseForKnownCharacterDat(
                int tickIndex)
            {
            }

            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(
                int tickIndex,
                bool applyFrameMotionTail = true)
            {
            }
        }
    }
}
#endif
