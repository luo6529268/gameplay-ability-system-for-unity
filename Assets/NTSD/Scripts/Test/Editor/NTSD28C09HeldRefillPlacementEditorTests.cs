#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C09HeldRefillPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_SerialRemainderObservesFirstHeldPassWrite()
        {
            var world = new SimulationWorld();
            CreateHeldFixture(world, 50, 51, out _, out SerialObserverHeld child);

            new NTSDBattleTickSystem(world).RunReleaseTick(41, buildPresentation: false);

            Assert.That(child.PostSerialCount, Is.EqualTo(1));
            Assert.That(child.ObservedFrameInSerial, Is.EqualTo(5));
            Assert.That(child.Frame.N, Is.EqualTo(5));
        }

        [Test]
        public void FullTick_RecordsFirstHeldPassAfterC08BeforeSerialAndKeepsSecondOccurrence()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(42, buildPresentation: false);

            Assert.That(diagnostics.TryGetLastPhaseAt(8, out BattleTickPhase clamp), Is.True);
            Assert.That(clamp, Is.EqualTo(BattleTickPhase.StageBounds));
            Assert.That(diagnostics.TryGetLastPhaseAt(9, out BattleTickPhase firstHeld), Is.True);
            Assert.That(firstHeld, Is.EqualTo(BattleTickPhase.HeldProcess));
            Assert.That(diagnostics.TryGetLastPhaseAt(10, out BattleTickPhase snapshot), Is.True);
            Assert.That(snapshot, Is.EqualTo(BattleTickPhase.CollisionSnapshot));
            Assert.That(diagnostics.TryGetLastPhaseAt(11, out BattleTickPhase pairVRest), Is.True);
            Assert.That(pairVRest, Is.EqualTo(BattleTickPhase.PairVRest));
            Assert.That(diagnostics.TryGetLastPhaseAt(12, out BattleTickPhase candidate), Is.True);
            Assert.That(candidate, Is.EqualTo(BattleTickPhase.CandidateCollect));
            Assert.That(diagnostics.TryGetLastPhaseAt(13, out BattleTickPhase fusion), Is.True);
            Assert.That(fusion, Is.EqualTo(BattleTickPhase.RuntimeMaintenance));
            Assert.That(diagnostics.TryGetLastPhaseAt(14, out BattleTickPhase weaponCount), Is.True);
            Assert.That(weaponCount, Is.EqualTo(BattleTickPhase.ActiveWeaponCount));
            Assert.That(diagnostics.TryGetLastPhaseAt(15, out BattleTickPhase hit), Is.True);
            Assert.That(hit, Is.EqualTo(BattleTickPhase.CharacterHitConsumePostInteraction));
            Assert.That(diagnostics.TryGetLastPhaseAt(27, out BattleTickPhase serial), Is.True);
            Assert.That(serial, Is.EqualTo(BattleTickPhase.FrameAdvance));

            int heldOccurrences = 0;
            for (int index = 0; index < diagnostics.LastPhaseSequenceCount; index++)
            {
                if (diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase phase) &&
                    phase == BattleTickPhase.HeldProcess)
                {
                    heldOccurrences++;
                }
            }

            Assert.That(heldOccurrences, Is.EqualTo(2));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(33));
        }

        private static void CreateHeldFixture(
            SimulationWorld world,
            int holderSlot,
            int childSlot,
            out PassiveHolder holder,
            out SerialObserverHeld child)
        {
            LF2FrameData holderFrame = Frame(0, 0, 20, 30);
            holderFrame.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint
                {
                    x = 10,
                    y = 20,
                    weaponact = 5,
                    cover = 2,
                },
            };
            var holderData = new LF2CharacterData
            {
                name = "C09Holder",
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { holderFrame },
            };

            LF2FrameData childFrame0 = Frame(0, 0, 8, 10);
            childFrame0.wpoints = new List<WeaponPoint> { new WeaponPoint() };
            LF2FrameData childFrame5 = Frame(5, 0, 8, 10);
            childFrame5.wpoints = new List<WeaponPoint>
            {
                new WeaponPoint { x = 3, y = 4 },
            };
            var childData = new LF2CharacterData
            {
                name = "C09Held",
                type_sub = (int)LF2ObjectType.LightWeapon,
                frames = new List<LF2FrameData> { childFrame0, childFrame5 },
            };

            holder = new PassiveHolder();
            Bind(holder, holderSlot, 10900, holderData);
            world.Register(holder);
            holder.Runtime.SetPosition(100.0, 50.0, 200.0);
            holder.Runtime.SyncIntegerPosition();
            holder.SwitchDir("right");

            child = new SerialObserverHeld();
            Bind(child, childSlot, 10901, childData);
            world.Register(child);

            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = childSlot;
            holder.Runtime.HeldWeaponStableId = childSlot;
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = holderSlot;
        }

        private static LF2FrameData Frame(
            int id,
            int state,
            int centerX,
            int centerY)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
                centerx = centerX,
                centery = centerY,
                itrs = new List<InteractionArea>(),
            };
        }

        private static void Bind(
            LF2Entity entity,
            int slot,
            int objectId,
            LF2CharacterData data)
        {
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.SetRequiredRuntimeSlot(slot);
        }

        private sealed class PassiveHolder : LF2OtherObject
        {
            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }
        }

        private sealed class SerialObserverHeld : LF2OtherObject
        {
            internal int PostSerialCount { get; private set; }
            internal int ObservedFrameInSerial { get; private set; } = -1;

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedFrameInSerial = Frame.N;
            }
        }
    }
}
#endif
