#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C06NestedPhysicsProductionEditorTests
    {
        [Test]
        public void NestedPhysics_NormalizesLowerSlotBeforeHigherSlotPhysics()
        {
            var world = new SimulationWorld();
            PhysicsProbeCharacter lower = CreateProbe(world, 0, 10600, (int)LF2ObjectType.Character);
            PhysicsProbeCharacter higher = CreateProbe(world, 1, 10601, (int)LF2ObjectType.Character);
            lower.OnPhysics = () =>
            {
                lower.Health.HP = -5;
                lower.Health.HPBound = 340;
                lower.Health.PP = 229;
            };
            higher.OnPhysics = () =>
            {
                Assert.That(lower.Health.HP, Is.EqualTo(-5));
                Assert.That(lower.Health.HPBound, Is.Zero);
                Assert.That(lower.Health.PP, Is.Zero);
            };

            world.NativePhysicsAndDeadCharacterResourceNormalizeAll(11);

            Assert.That(lower.NativePhysicsCount, Is.EqualTo(1));
            Assert.That(higher.NativePhysicsCount, Is.EqualTo(1));
        }

        [Test]
        public void DeadCharacterNormalizer_WritesOnlyEffectiveHpAndNativeMpFields()
        {
            var world = new SimulationWorld();
            PhysicsProbeCharacter dead = CreateProbe(world, 0, 10602, (int)LF2ObjectType.Character);
            dead.Health.HP = -5;
            dead.Health.HPBound = 340;
            dead.Health.HP3 = 500;
            dead.Health.PP = 229;
            dead.Health.MaxPP = 480;
            dead.Health.MP = 17;
            dead.Health.MaxMP = 19;

            world.NativePhysicsAndDeadCharacterResourceNormalizeAll(12);

            Assert.That(dead.Health.HP, Is.EqualTo(-5));
            Assert.That(dead.Health.HPBound, Is.Zero);
            Assert.That(dead.Health.HP3, Is.EqualTo(500));
            Assert.That(dead.Health.PP, Is.Zero);
            Assert.That(dead.Health.MaxPP, Is.EqualTo(480));
            Assert.That(dead.Health.MP, Is.EqualTo(17));
            Assert.That(dead.Health.MaxMP, Is.EqualTo(19));
        }

        [Test]
        public void DeadCharacterNormalizer_UsesCurrentDatTypeAndPreservesLivingEntities()
        {
            var world = new SimulationWorld();
            PhysicsProbeCharacter living = CreateProbe(world, 0, 10603, (int)LF2ObjectType.Character);
            PhysicsProbeCharacter nonCharacterDat = CreateProbe(world, 1, 10604, (int)LF2ObjectType.SpecialAttack);
            living.Health.HP = 1;
            living.Health.HPBound = 340;
            living.Health.PP = 229;
            nonCharacterDat.Health.HP = -5;
            nonCharacterDat.Health.HPBound = 341;
            nonCharacterDat.Health.PP = 230;

            world.NativePhysicsAndDeadCharacterResourceNormalizeAll(13);

            Assert.That(living.Health.HPBound, Is.EqualTo(340));
            Assert.That(living.Health.PP, Is.EqualTo(229));
            Assert.That(nonCharacterDat.Health.HPBound, Is.EqualTo(341));
            Assert.That(nonCharacterDat.Health.PP, Is.EqualTo(230));
        }

        [Test]
        public void FullProductionTick_RunsOnePhysicsOwnerThenTheSerialRemainder()
        {
            var world = new SimulationWorld();
            PhysicsProbeCharacter probe = CreateProbe(world, 0, 10605, (int)LF2ObjectType.Character);

            new NTSDBattleTickSystem(world).RunReleaseTick(14, buildPresentation: false);

            Assert.That(probe.NativePhysicsCount, Is.EqualTo(1));
            Assert.That(probe.PostNativePhysicsCount, Is.EqualTo(1));
            Assert.That(probe.LegacyTransitCount, Is.Zero);
            Assert.That(probe.LegacyTuCount, Is.Zero);
        }

        [Test]
        public void DirectSerialTick_RetainsLegacyTransitAndTuCompatibility()
        {
            var world = new SimulationWorld();
            PhysicsProbeCharacter probe = CreateProbe(world, 0, 10606, (int)LF2ObjectType.Character);

            world.SerialTickAll(15);

            Assert.That(probe.NativePhysicsCount, Is.Zero);
            Assert.That(probe.PostNativePhysicsCount, Is.Zero);
            Assert.That(probe.LegacyTransitCount, Is.EqualTo(1));
            Assert.That(probe.LegacyTuCount, Is.EqualTo(1));
        }

        [Test]
        public void FullTick_RecordsNestedPhysicsBeforeRevivalStageBoundsHeldAndSerialRemainder()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(16, buildPresentation: false);

            Assert.That(diagnostics.TryGetLastPhaseAt(5, out BattleTickPhase teleport), Is.True);
            Assert.That(teleport, Is.EqualTo(BattleTickPhase.NativeTeleport));
            Assert.That(diagnostics.TryGetLastPhaseAt(6, out BattleTickPhase physics), Is.True);
            Assert.That(physics, Is.EqualTo(BattleTickPhase.NestedPhysics));
            Assert.That(diagnostics.TryGetLastPhaseAt(7, out BattleTickPhase revival), Is.True);
            Assert.That(revival, Is.EqualTo(BattleTickPhase.Revival));
            Assert.That(diagnostics.TryGetLastPhaseAt(8, out BattleTickPhase firstClamp), Is.True);
            Assert.That(firstClamp, Is.EqualTo(BattleTickPhase.StageBounds));
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
            Assert.That(diagnostics.TryGetLastPhaseAt(28, out BattleTickPhase serial), Is.True);
            Assert.That(serial, Is.EqualTo(BattleTickPhase.FrameAdvance));
        }

        private static PhysicsProbeCharacter CreateProbe(
            SimulationWorld world,
            int slot,
            int objectId,
            int dataType)
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
                name = "C06_" + objectId,
                type_sub = dataType,
                frames = new List<LF2FrameData> { frame },
            };
            var probe = new PhysicsProbeCharacter();
            probe.ModuleInitialize();
            probe.Name = data.name;
            probe.ObjectId = objectId;
            probe.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.Initialize(500, 500);
            probe.ForcedDataType = dataType;
            probe.SetRequiredRuntimeSlot(slot);
            world.Register(probe);
            return probe;
        }

        private sealed class PhysicsProbeCharacter : LF2Character
        {
            internal System.Action OnPhysics { get; set; }
            internal int NativePhysicsCount { get; private set; }
            internal int PostNativePhysicsCount { get; private set; }
            internal int LegacyTransitCount { get; private set; }
            internal int LegacyTuCount { get; private set; }
            internal int ForcedDataType { get; set; } = (int)LF2ObjectType.Character;

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return ForcedDataType;
            }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                NativePhysicsCount++;
                OnPhysics?.Invoke();
                return true;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                Assert.That(nativePhysicsCompleted, Is.True);
                PostNativePhysicsCount++;
            }

            public override void SimTransit(int tickIndex)
            {
                LegacyTransitCount++;
            }

            public override void SimTU(int tickIndex)
            {
                LegacyTuCount++;
            }
        }
    }
}
#endif
