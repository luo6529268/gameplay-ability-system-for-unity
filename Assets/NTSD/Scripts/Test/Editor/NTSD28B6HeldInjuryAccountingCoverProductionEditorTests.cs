#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    [Category("NTSD28_B6_HeldInjury")]
    public sealed class NTSD28B6HeldInjuryAccountingCoverProductionEditorTests
    {
        [Test]
        public void RawInjury_UsesCanonicalWritesAndIgnoresLegacyScaleAndStats()
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 0,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            LF2Character legacyHolder = CreateEntity(
                world,
                "B6HeldLegacyHolder",
                9722,
                2,
                LF2ObjectType.Character,
                new List<LF2FrameData> { Frame(0, LF2States.Standing, null) },
                0);
            catcher.Runtime.OwnerSlotIndex = -1;
            catcher.FallDamageDiv = 200;
            victim.Runtime.IncomingDamageScale340 = 0;
            SetCanonicalSentinels(catcher, victim);
            SetLegacySentinels(world, legacyHolder, victim);

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(victim.Health.HP, Is.EqualTo(70));
            Assert.That(victim.Health.HPBound, Is.EqualTo(90));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(37));
            Assert.That(catcher.Runtime.InputScoreTotal348, Is.EqualTo(41));
            Assert.That(catcher.Runtime.KnockoutCount358, Is.EqualTo(13));
            AssertDisplay(victim, 3, 3, 3, 1);
            AssertLegacySentinels(world, legacyHolder, victim);
        }

        [Test]
        public void ScaledInjury_UsesIncomingDamageScaleButRawDisplayLead()
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 0,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            catcher.Runtime.OwnerSlotIndex = -1;
            catcher.FallDamageDiv = 0;
            victim.Runtime.IncomingDamageScale340 = 200;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(victim.Health.HP, Is.EqualTo(85));
            Assert.That(victim.Health.HPBound, Is.EqualTo(95));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(15));
            Assert.That(catcher.Runtime.InputScoreTotal348, Is.EqualTo(15));
            AssertDisplay(victim, 3, 3, 3, 1);
        }

        [Test]
        public void DirectOwnerCredit_DoesNotFollowSecondOwnerHop()
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 0,
                catcherType: LF2ObjectType.SpecialAttack,
                out LF2Character catcher,
                out LF2Character victim);
            LF2Character directOwner = CreateEntity(
                world,
                "B6HeldDirectOwner",
                9723,
                2,
                LF2ObjectType.Character,
                new List<LF2FrameData> { Frame(0, LF2States.Standing, null) },
                0);
            LF2Character secondOwner = CreateEntity(
                world,
                "B6HeldSecondOwner",
                9724,
                3,
                LF2ObjectType.Character,
                new List<LF2FrameData> { Frame(0, LF2States.Standing, null) },
                0);
            catcher.Runtime.OwnerSlotIndex = directOwner.Runtime.SlotIndex;
            directOwner.Runtime.OwnerSlotIndex = secondOwner.Runtime.SlotIndex;
            directOwner.Runtime.InputScoreTotal348 = 10;
            directOwner.Runtime.KnockoutCount358 = 20;
            secondOwner.Runtime.InputScoreTotal348 = 30;
            secondOwner.Runtime.KnockoutCount358 = 40;
            victim.Health.HP = 20;
            victim.Runtime.OrdinaryCreditGate2F4 = -1;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(directOwner.Runtime.InputScoreTotal348, Is.EqualTo(40));
            Assert.That(directOwner.Runtime.KnockoutCount358, Is.EqualTo(21));
            Assert.That(secondOwner.Runtime.InputScoreTotal348, Is.EqualTo(30));
            Assert.That(secondOwner.Runtime.KnockoutCount358, Is.EqualTo(40));
        }

        [Test]
        public void MissingOwner_Type0FallsBackToCatcherSelfButSkipsDisplayLead()
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 0,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            catcher.Runtime.OwnerSlotIndex = 99;
            catcher.Runtime.InputScoreTotal348 = 5;
            victim.Runtime.DisplayScoreStep1F4 = 91;
            victim.Runtime.DisplayDamageStep1FC = 92;
            victim.Runtime.DisplayCurrentHpStep204 = 93;
            victim.Runtime.DisplayEffectiveMaxHpStep20C = 94;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(catcher.Runtime.InputScoreTotal348, Is.EqualTo(35));
            AssertDisplay(victim, 91, 92, 93, 94);
        }

        [Test]
        public void MissingOwner_NonType0HasNoScoreOrKnockoutCredit()
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 0,
                catcherType: LF2ObjectType.SpecialAttack,
                out LF2Character catcher,
                out LF2Character victim);
            catcher.Runtime.OwnerSlotIndex = 99;
            catcher.Runtime.InputScoreTotal348 = 5;
            catcher.Runtime.KnockoutCount358 = 6;
            victim.Health.HP = 20;
            victim.Runtime.OrdinaryCreditGate2F4 = -1;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(victim.Health.HP, Is.EqualTo(-10));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(30));
            Assert.That(catcher.Runtime.InputScoreTotal348, Is.EqualTo(5));
            Assert.That(catcher.Runtime.KnockoutCount358, Is.EqualTo(6));
        }

        [TestCase(20, -1, 1)]
        [TestCase(20, 7, 0)]
        [TestCase(0, -1, 0)]
        public void KnockoutGate_UsesLiveHpDamageAndOrdinaryCreditGate(
            int initialHp,
            int ordinaryCreditGate,
            int expectedKnockoutDelta)
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 0,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            catcher.Runtime.OwnerSlotIndex = -1;
            catcher.Runtime.InputScoreTotal348 = 10;
            catcher.Runtime.KnockoutCount358 = 20;
            victim.Health.HP = initialHp;
            victim.Runtime.OrdinaryCreditGate2F4 = ordinaryCreditGate;
            victim.KillCount = ordinaryCreditGate == -1 ? 99 : -1;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(victim.Health.HP, Is.EqualTo(initialHp - 30));
            Assert.That(catcher.Runtime.InputScoreTotal348, Is.EqualTo(40));
            Assert.That(
                catcher.Runtime.KnockoutCount358,
                Is.EqualTo(20 + expectedKnockoutDelta));
        }

        [TestCase(0, 2, -3)]
        [TestCase(1, 7, -3)]
        [TestCase(2, 2, 8)]
        [TestCase(3, 7, 8)]
        [TestCase(10, 2, -3)]
        [TestCase(11, 2, -3)]
        public void CoverValue_ExcludesOnlyExactTimerValues(
            int cover,
            int expectedCatcherDelay,
            int expectedVictimDelay)
        {
            SimulationWorld world = CreatePair(
                injury: 1,
                cover,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            catcher.FrameDelay = 7;
            victim.FrameDelay = 8;
            catcher.AttackingCounter = 0;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(catcher.AttackingCounter, Is.EqualTo(1));
            Assert.That(catcher.FrameDelay, Is.EqualTo(expectedCatcherDelay));
            Assert.That(victim.FrameDelay, Is.EqualTo(expectedVictimDelay));
        }

        [Test]
        public void NegativeInjury_IsFailClosedForAccountingAndTimers()
        {
            SimulationWorld world = CreatePair(
                injury: -15,
                cover: 0,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            catcher.FrameDelay = 7;
            victim.FrameDelay = 8;
            catcher.AttackingCounter = 0;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Runtime.InputHpConsumedTotal34C = 9;
            catcher.Runtime.InputScoreTotal348 = 10;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(victim.Health.HP, Is.EqualTo(100));
            Assert.That(victim.Health.HPBound, Is.EqualTo(100));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(9));
            Assert.That(catcher.Runtime.InputScoreTotal348, Is.EqualTo(10));
            Assert.That(catcher.AttackingCounter, Is.Zero);
            Assert.That(catcher.FrameDelay, Is.EqualTo(7));
            Assert.That(victim.FrameDelay, Is.EqualTo(8));
        }

        [Test]
        public void LegacyStatAndHPLostSentinels_AreNeverWritten()
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 0,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            LF2Character legacyHolder = CreateEntity(
                world,
                "B6HeldLegacySentinel",
                9725,
                2,
                LF2ObjectType.Character,
                new List<LF2FrameData> { Frame(0, LF2States.Standing, null) },
                0);
            victim.Health.HP = 20;
            victim.Runtime.OrdinaryCreditGate2F4 = -1;
            SetLegacySentinels(world, legacyHolder, victim);

            catcher.RunWeaponSyncHeldStep10();

            AssertLegacySentinels(world, legacyHolder, victim);
        }

        [Test]
        public void WarmedHeldAccountingAndCover_DoesNotAllocateManagedMemory()
        {
            SimulationWorld world = CreatePair(
                injury: 30,
                cover: 1,
                catcherType: LF2ObjectType.Character,
                out LF2Character catcher,
                out LF2Character victim);
            for (int index = 0; index < 32; index++)
            {
                ResetWarmIteration(catcher, victim);
                catcher.RunWeaponSyncHeldStep10();
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                ResetWarmIteration(catcher, victim);
                catcher.RunWeaponSyncHeldStep10();
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(30));
            Assert.That(catcher.Runtime.InputScoreTotal348, Is.EqualTo(30));
        }

        private static void ResetWarmIteration(
            LF2Character catcher,
            LF2Character victim)
        {
            catcher.AttackingCounter = 0;
            catcher.FrameDelay = 7;
            victim.FrameDelay = 8;
            victim.Health.HP = 500;
            victim.Health.HPBound = 500;
            victim.Runtime.InputHpConsumedTotal34C = 0;
            catcher.Runtime.InputScoreTotal348 = 0;
        }

        private static void SetCanonicalSentinels(
            LF2Character catcher,
            LF2Character victim)
        {
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Runtime.InputHpConsumedTotal34C = 7;
            catcher.Runtime.InputScoreTotal348 = 11;
            catcher.Runtime.KnockoutCount358 = 13;
            victim.Runtime.OrdinaryCreditGate2F4 = -1;
        }

        private static void SetLegacySentinels(
            SimulationWorld world,
            LF2Character holder,
            LF2Character victim)
        {
            holder.KillStat = 41;
            holder.ComboCountAtk = 42;
            victim.ComboCountVic = 43;
            victim.Health.HPLost = 44;
            victim.KillCount = -1;
            victim.Unk344 = 1;
            world.KillStats[1] = 45;
            world.DamageStats[1] = 46;
        }

        private static void AssertLegacySentinels(
            SimulationWorld world,
            LF2Character holder,
            LF2Character victim)
        {
            Assert.That(holder.KillStat, Is.EqualTo(41));
            Assert.That(holder.ComboCountAtk, Is.EqualTo(42));
            Assert.That(victim.ComboCountVic, Is.EqualTo(43));
            Assert.That(victim.Health.HPLost, Is.EqualTo(44));
            Assert.That(world.KillStats[1], Is.EqualTo(45));
            Assert.That(world.DamageStats[1], Is.EqualTo(46));
        }

        private static void AssertDisplay(
            LF2Character victim,
            int score,
            int damage,
            int hp,
            int hpBound)
        {
            Assert.That(victim.Runtime.DisplayScoreStep1F4, Is.EqualTo(score));
            Assert.That(victim.Runtime.DisplayDamageStep1FC, Is.EqualTo(damage));
            Assert.That(victim.Runtime.DisplayCurrentHpStep204, Is.EqualTo(hp));
            Assert.That(victim.Runtime.DisplayEffectiveMaxHpStep20C, Is.EqualTo(hpBound));
        }

        private static SimulationWorld CreatePair(
            int injury,
            int cover,
            LF2ObjectType catcherType,
            out LF2Character catcher,
            out LF2Character victim)
        {
            var world = new SimulationWorld();
            catcher = CreateEntity(
                world,
                "B6HeldAccountingCatcher",
                9720,
                0,
                catcherType,
                new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, null),
                    Frame(
                        343,
                        LF2States.Catching,
                        new CatchPoint
                        {
                            kind = 1,
                            vaction = 132,
                            hurtable = 2,
                            injury = injury,
                            x = 17,
                            y = 23,
                            cover = cover,
                        }),
                },
                343);
            victim = CreateEntity(
                world,
                "B6HeldAccountingVictim",
                9721,
                1,
                LF2ObjectType.Character,
                new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, null),
                    Frame(
                        132,
                        LF2States.BeingCaught,
                        new CatchPoint { kind = 2, x = 5, y = 9 }),
                },
                132);
            catcher.Runtime.SetPosition(100, 200, 300);
            victim.Runtime.SetPosition(110, 210, 300);
            catcher.Runtime.SyncIntegerPosition();
            victim.Runtime.SyncIntegerPosition();
            catcher.Runtime.CaughtSlotIndex = 1;
            victim.Runtime.CatchSourceSlot90 = 0;
            victim.Runtime.CatcherSlotIndex = 0;
            catcher.AttackingCounter = 0;
            catcher.FrameDelay = 0;
            victim.FrameDelay = 0;
            catcher.Runtime.OwnerSlotIndex = -1;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            return world;
        }

        private static LF2Character CreateEntity(
            SimulationWorld world,
            string name,
            int objectId,
            int slot,
            LF2ObjectType objectType,
            List<LF2FrameData> frames,
            int frameId)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = (int)objectType,
                frames = frames,
            };
            LF2Character entity = objectType == LF2ObjectType.Character
                ? new LF2Character()
                : new TypedCharacter(objectType);
            entity.ModuleInitialize();
            entity.Name = name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            LF2FrameData frame = entity.FrameCache.GetFrameDataById(frameId);
            entity.Frame.D = frame;
            entity.Frame.N = frameId;
            entity.Frame.PN = frameId;
            entity.Frame.Prev = frameId;
            entity.Frame.Prev2 = frameId;
            entity.Frame.Prev2D = frame;
            entity.Runtime.PrevFrame2 = frameId;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(slot));
            return entity;
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType currentDataType;

            internal TypedCharacter(LF2ObjectType currentDataType)
            {
                this.currentDataType = currentDataType;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
                => (int)currentDataType;
        }

        private static LF2FrameData Frame(
            int id,
            int state,
            CatchPoint cpoint)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 1,
                next = id,
                centerx = 39,
                centery = 79,
                cpoint = cpoint,
            };
        }
    }
}
#endif
