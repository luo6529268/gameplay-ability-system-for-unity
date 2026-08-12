#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsCooldownPassEditorTests
    {
        [Test]
        public void DefaultMode_UsesPromotedDataWriterAndCannotSwitchAfterResetBoundary()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsCooldownPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCooldownPassMode.DataOriented));

            world.AdvanceBattleFlowTick(1);
            Assert.Throws<InvalidOperationException>(() =>
                world.ConfigureBattleEcsCooldownPassForDiagnostics(
                    BattleEcsCooldownPassMode.Legacy));
        }

        [Test]
        public void ShadowCompare_ExactlyMatchesLegacyCooldownWriter()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(
                50,
                1001,
                CreateFrame(0, hasItr: false, holderAttacking: null));
            Register(world, entity, 50);
            entity.ItrRest.Arest = 3;
            entity.AttackExempt = 7;
            world.ConfigureBattleEcsCooldownPassForDiagnostics(
                BattleEcsCooldownPassMode.ShadowCompare);

            world.RunBattleEcsCooldownPass(1);

            BattleEcsCooldownPassDiagnostics diagnostics =
                world.BattleEcsCooldownPassDiagnosticsForDiagnostics;
            Assert.That(entity.ItrRest.Arest, Is.EqualTo(2));
            Assert.That(entity.AttackExempt, Is.Zero);
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.ValidationCount, Is.EqualTo(1));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.TrackerFallbackCount, Is.Zero);
            Assert.That(diagnostics.IsClean, Is.True);
        }

        [Test]
        public void DataOrientedWriter_MatchesLegacyForCooldownAndHeldWeaponContracts()
        {
            ContractFixture legacy = CreateContractFixture(
                BattleEcsCooldownPassMode.Legacy);
            ContractFixture dataOriented = CreateContractFixture(
                BattleEcsCooldownPassMode.DataOriented);

            legacy.World.RunBattleEcsCooldownPass(1);
            dataOriented.World.RunBattleEcsCooldownPass(1);

            for (int i = 0; i < legacy.Entities.Length; i++)
            {
                Assert.That(
                    dataOriented.Entities[i].ItrRest.Arest,
                    Is.EqualTo(legacy.Entities[i].ItrRest.Arest),
                    $"ARest mismatch at contract entity {i}");
                Assert.That(
                    dataOriented.Entities[i].AttackExempt,
                    Is.EqualTo(legacy.Entities[i].AttackExempt),
                    $"AttackExempt mismatch at contract entity {i}");
            }

            Assert.That(dataOriented.Entities[0].ItrRest.Arest, Is.EqualTo(2));
            Assert.That(dataOriented.Entities[0].AttackExempt, Is.Zero,
                "a frame without itr clears AttackExempt");
            Assert.That(dataOriented.Entities[1].AttackExempt, Is.EqualTo(7),
                "an ordinary hittable frame keeps AttackExempt");
            Assert.That(dataOriented.Entities[3].AttackExempt, Is.Zero,
                "held state 1001 clears when the holder wpoint is not attacking");
            Assert.That(dataOriented.Entities[5].AttackExempt, Is.EqualTo(7),
                "held state 1001 keeps the value while the holder wpoint attacks");

            BattleEcsCooldownPassDiagnostics diagnostics =
                dataOriented.World.BattleEcsCooldownPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(6));
            Assert.That(diagnostics.TrackerFallbackCount, Is.Zero);
        }

        [Test]
        public void DataOrientedWriter_SkipsDormantAndPendingDestroySlots()
        {
            var world = new SimulationWorld();
            LF2Character active = CreateCharacter(
                50,
                2001,
                CreateFrame(0, hasItr: false, holderAttacking: null));
            LF2Character dormant = CreateCharacter(
                51,
                2002,
                CreateFrame(0, hasItr: false, holderAttacking: null));
            LF2Character pending = CreateCharacter(
                52,
                2003,
                CreateFrame(0, hasItr: false, holderAttacking: null));
            Register(world, active, 50);
            Register(world, dormant, 51);
            Register(world, pending, 52);
            active.ItrRest.Arest = dormant.ItrRest.Arest = pending.ItrRest.Arest = 4;
            active.AttackExempt = dormant.AttackExempt = pending.AttackExempt = 6;
            dormant.Runtime.OidMergeDormant = true;
            pending.Runtime.PendingFlushDestroy = true;
            world.ConfigureBattleEcsCooldownPassForDiagnostics(
                BattleEcsCooldownPassMode.DataOriented);

            world.RunBattleEcsCooldownPass(1);

            Assert.That(active.ItrRest.Arest, Is.EqualTo(3));
            Assert.That(active.AttackExempt, Is.Zero);
            Assert.That(dormant.ItrRest.Arest, Is.EqualTo(4));
            Assert.That(dormant.AttackExempt, Is.EqualTo(6));
            Assert.That(pending.ItrRest.Arest, Is.EqualTo(4));
            Assert.That(pending.AttackExempt, Is.EqualTo(6));
            Assert.That(
                world.BattleEcsCooldownPassDiagnosticsForDiagnostics.SlotVisitCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Extended1000_WarmedDataOrientedCooldownPassDoesNotAllocate()
        {
            const int capacity = 1050;
            const int firstSlot = 50;
            const int entityCount = 1000;
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                capacity);
            LF2FrameData frame = CreateFrame(
                0,
                hasItr: true,
                holderAttacking: null);
            var data = new LF2CharacterData
            {
                name = "cooldown-extended-1000",
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var wrapper = new LF2CharacterDataWrapper(3000, data);

            for (int slot = firstSlot; slot < capacity; slot++)
            {
                LF2Character entity = CreateCharacter(slot, 3000 + slot, frame, wrapper);
                Register(world, entity, slot);
                entity.ItrRest.Arest = 3 + (slot & 3);
                entity.AttackExempt = 0;
            }

            world.ConfigureBattleEcsCooldownPassForDiagnostics(
                BattleEcsCooldownPassMode.DataOriented);
            world.RunBattleEcsCooldownPass(1);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.RunBattleEcsCooldownPass(2);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            BattleEcsCooldownPassDiagnostics diagnostics =
                world.BattleEcsCooldownPassDiagnosticsForDiagnostics;
            Assert.That(allocated, Is.Zero,
                "the warmed 1000-entity cooldown writer must not allocate managed memory");
            Assert.That(diagnostics.RunCount, Is.EqualTo(2));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(entityCount * 2));
            Assert.That(diagnostics.TrackerFallbackCount, Is.Zero);
        }

        private static ContractFixture CreateContractFixture(
            BattleEcsCooldownPassMode mode)
        {
            var world = new SimulationWorld();
            LF2Character noItr = CreateCharacter(
                50,
                4001,
                CreateFrame(0, hasItr: false, holderAttacking: null));
            LF2Character ordinaryItr = CreateCharacter(
                51,
                4002,
                CreateFrame(0, hasItr: true, holderAttacking: null));
            LF2Character inactiveHolder = CreateCharacter(
                52,
                4003,
                CreateFrame(0, hasItr: false, holderAttacking: 0));
            LF2Character heldByInactiveHolder = CreateCharacter(
                53,
                4004,
                CreateFrame(LF2States.WeaponOnHand, hasItr: true, holderAttacking: null));
            LF2Character activeHolder = CreateCharacter(
                54,
                4005,
                CreateFrame(0, hasItr: false, holderAttacking: 1));
            LF2Character heldByActiveHolder = CreateCharacter(
                55,
                4006,
                CreateFrame(LF2States.WeaponOnHand, hasItr: true, holderAttacking: null));
            LF2Character[] entities =
            {
                noItr,
                ordinaryItr,
                inactiveHolder,
                heldByInactiveHolder,
                activeHolder,
                heldByActiveHolder,
            };

            for (int i = 0; i < entities.Length; i++)
            {
                Register(world, entities[i], 50 + i);
                entities[i].ItrRest.Arest = 3 + i;
                entities[i].AttackExempt = 7;
            }

            heldByInactiveHolder.Runtime.LinkState = -1;
            heldByInactiveHolder.Runtime.HolderStableId = inactiveHolder.Runtime.SlotIndex;
            heldByActiveHolder.Runtime.LinkState = -1;
            heldByActiveHolder.Runtime.HolderStableId = activeHolder.Runtime.SlotIndex;
            world.ConfigureBattleEcsCooldownPassForDiagnostics(mode);
            return new ContractFixture(world, entities);
        }

        private static LF2FrameData CreateFrame(
            int state,
            bool hasItr,
            int? holderAttacking)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 100,
                next = 0,
            };
            if (hasItr)
            {
                frame.itrs.Add(new InteractionArea
                {
                    kind = 0,
                    x = 0,
                    y = 0,
                    w = 10,
                    h = 10,
                });
            }
            if (holderAttacking.HasValue)
            {
                frame.wpoints.Add(new WeaponPoint
                {
                    attacking = holderAttacking.Value,
                });
            }
            return frame;
        }

        private static LF2Character CreateCharacter(
            int slot,
            int stableId,
            LF2FrameData frame,
            LF2CharacterDataWrapper sharedWrapper = null)
        {
            LF2CharacterDataWrapper wrapper = sharedWrapper;
            if (wrapper == null)
            {
                var data = new LF2CharacterData
                {
                    name = $"cooldown-{stableId}",
                    type_sub = 1,
                    frames = new List<LF2FrameData> { frame },
                };
                wrapper = new LF2CharacterDataWrapper(stableId, data);
            }

            var character = new LF2Character();
            character.ModuleInitialize();
            character.Runtime.StableId = stableId;
            character.Runtime.ObjectId = wrapper.characterId;
            character.Runtime.EntityType = (int)LF2ObjectType.Character;
            character.FrameCache.Load(wrapper);
            character.Frame.D = frame;
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(slot);
            return character;
        }

        private static void Register(
            SimulationWorld world,
            LF2Character entity,
            int requiredSlot)
        {
            entity.SetRequiredRuntimeSlot(requiredSlot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(requiredSlot));
            Assert.That(entity.ItrRest.IsBound, Is.True);
        }

        private sealed class ContractFixture
        {
            public ContractFixture(SimulationWorld world, LF2Character[] entities)
            {
                World = world;
                Entities = entities;
            }

            public SimulationWorld World { get; }
            public LF2Character[] Entities { get; }
        }
    }
}
#endif
