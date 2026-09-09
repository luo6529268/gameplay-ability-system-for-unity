#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25FHJTimerOwnerEditorTests
    {
        [Test]
        public void C25f_RefreshesOnlyLowSlotTypeZeroThenC25hDecaysSameTick()
        {
            var world = new SimulationWorld();
            LF2Character low = CreateCharacter(world, 0, 9100, 7002);
            LF2Character zero = CreateCharacter(world, 9, 9101, 7000);
            LF2Character high = CreateCharacter(world, 10, 9102, 7002);
            LF2OtherObject nonCharacter = CreateOther(world, 1, 9103, 7009);
            low.Runtime.NativeComputerState1B8 = 9;
            zero.Runtime.NativeComputerState1B8 = 9;
            high.Runtime.NativeComputerState1B8 = 4;
            nonCharacter.Runtime.NativeComputerState1B8 = 4;

            world.LateEntityUpdateAll(1);

            Assert.That(low.Runtime.NativeComputerState1B8, Is.EqualTo(1));
            Assert.That(zero.Runtime.NativeComputerState1B8, Is.Zero);
            Assert.That(high.Runtime.NativeComputerState1B8, Is.EqualTo(3));
            Assert.That(nonCharacter.Runtime.NativeComputerState1B8, Is.EqualTo(3));
        }

        [Test]
        public void C25h_AdvancesBodyBankAndPositiveHpStatusExactlyOnce()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9110, 0);
            entity.HitStun = 5;
            entity.FallCounter = 4;
            entity.Runtime.Bdefend = 3;
            entity.HitConfirmCounter = 2;
            entity.HitStateCount = 9;
            entity.Runtime.Unk338 = 2;
            SetTimerBank(entity.Runtime, 2);
            SetPositiveHpStatus(entity.Runtime, 2);

            world.LateEntityUpdateAll(1);

            Assert.That(entity.HitStun, Is.EqualTo(4));
            Assert.That(entity.FallCounter, Is.EqualTo(3));
            Assert.That(entity.Runtime.Bdefend, Is.EqualTo(2));
            Assert.That(entity.HitConfirmCounter, Is.EqualTo(1));
            Assert.That(entity.HitStateCount, Is.EqualTo(9));
            Assert.That(entity.Runtime.Unk338, Is.EqualTo(1));
            AssertTimerBank(entity.Runtime, 1);
            AssertPositiveHpStatus(entity.Runtime, 1);
        }

        [Test]
        public void C25h_HeldOrdinaryFreezesBodyButNotBankOrExpiryCleanup()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9120, 0);
            entity.FrameDelay = 2;
            entity.HitStun = 5;
            entity.FallCounter = 4;
            entity.Runtime.Bdefend = 3;
            entity.HitConfirmCounter = 2;
            entity.AttackExempt = 4;
            entity.Runtime.Unk338 = 2;
            SetTimerBank(entity.Runtime, 2);
            SetPositiveHpStatus(entity.Runtime, 2);
            entity.Runtime.JoinTimer148 = 0;
            entity.Runtime.JoinOverrideActive170 = 1;
            entity.Runtime.JoinOriginalBattleGroup174 = 7;
            entity.RelationTeam = 9;
            entity.Runtime.InputProxyCounter14C = 0;
            entity.Runtime.InputProxyEnabled17C = 1;

            world.LateEntityUpdateAll(1);

            Assert.That(entity.HitStun, Is.EqualTo(5));
            Assert.That(entity.FallCounter, Is.EqualTo(4));
            Assert.That(entity.Runtime.Bdefend, Is.EqualTo(3));
            Assert.That(entity.HitConfirmCounter, Is.EqualTo(2));
            Assert.That(entity.AttackExempt, Is.EqualTo(4));
            Assert.That(entity.Runtime.Unk338, Is.EqualTo(1));
            AssertTimerBank(entity.Runtime, 1, joinTimer: 0);
            Assert.That(entity.Runtime.InputActionLock130, Is.EqualTo(2));
            Assert.That(entity.Runtime.InputRemapState138, Is.EqualTo(2));
            Assert.That(entity.Runtime.InputProxyCounter14C, Is.Zero);
            Assert.That(entity.Runtime.WeakTimer12C, Is.EqualTo(2));
            Assert.That(entity.Runtime.DelayTimer134, Is.EqualTo(2));
            Assert.That(entity.Runtime.PoisonTimer120, Is.EqualTo(2));
            Assert.That(entity.Runtime.JoinOverrideActive170, Is.Zero);
            Assert.That(entity.RelationTeam, Is.EqualTo(7));
            Assert.That(entity.Runtime.InputProxyEnabled17C, Is.Zero);
        }

        [Test]
        public void C25hAndJ_TypeThreeIgnoreMotionHoldGate()
        {
            var world = new SimulationWorld();
            LF2SpecialAttack entity = CreateSpecial(world, 0, 9130, 0);
            entity.FrameDelay = 2;
            entity.HitStun = 5;
            entity.FallCounter = 4;
            entity.Runtime.Bdefend = 3;
            entity.HitConfirmCounter = 2;
            entity.AttackExempt = 4;
            SetPositiveHpStatus(entity.Runtime, 2);

            world.LateEntityUpdateAll(1);

            Assert.That(entity.HitStun, Is.EqualTo(4));
            Assert.That(entity.FallCounter, Is.EqualTo(3));
            Assert.That(entity.Runtime.Bdefend, Is.EqualTo(2));
            Assert.That(entity.HitConfirmCounter, Is.EqualTo(1));
            Assert.That(entity.AttackExempt, Is.EqualTo(3));
            AssertPositiveHpStatus(entity.Runtime, 1);
        }

        [Test]
        public void C25h_NegativeRelationFreezesBodyButStillCleansExpiredJoinAndProxy()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9140, 0);
            entity.Runtime.LinkState = -1;
            entity.HitStun = -4;
            entity.FallCounter = 4;
            entity.Runtime.BoundState198 = -3;
            entity.Runtime.NativeTimer1BC = -2;
            entity.Runtime.JoinTimer148 = 0;
            entity.Runtime.JoinOverrideActive170 = 1;
            entity.Runtime.JoinOriginalBattleGroup174 = 6;
            entity.RelationTeam = 8;
            entity.Runtime.InputProxyCounter14C = 0;
            entity.Runtime.InputProxyEnabled17C = 1;

            world.LateEntityUpdateAll(1);

            Assert.That(entity.HitStun, Is.EqualTo(-4));
            Assert.That(entity.FallCounter, Is.EqualTo(4));
            Assert.That(entity.Runtime.BoundState198, Is.EqualTo(-3));
            Assert.That(entity.Runtime.NativeTimer1BC, Is.EqualTo(-2));
            Assert.That(entity.Runtime.JoinOverrideActive170, Is.Zero);
            Assert.That(entity.RelationTeam, Is.EqualTo(6));
            Assert.That(entity.Runtime.InputProxyEnabled17C, Is.Zero);
        }

        [Test]
        public void C25h_NonPositiveHpStillAdvancesReactionAndBankButFreezesStatus()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9145, 0);
            entity.Health.HP = 0;
            entity.HitStun = 5;
            entity.FallCounter = 4;
            entity.Runtime.Bdefend = 3;
            entity.HitConfirmCounter = 2;
            SetTimerBank(entity.Runtime, 2);
            SetPositiveHpStatus(entity.Runtime, 2);

            world.LateEntityUpdateAll(1);

            Assert.That(entity.HitStun, Is.EqualTo(4));
            Assert.That(entity.FallCounter, Is.EqualTo(3));
            Assert.That(entity.Runtime.Bdefend, Is.EqualTo(2));
            Assert.That(entity.HitConfirmCounter, Is.EqualTo(1));
            AssertTimerBank(entity.Runtime, 1);
            AssertPositiveHpStatus(entity.Runtime, 2);
        }

        [TestCase(0, 7, 200, 500, 7, 193)]
        [TestCase(2, 25, 200, 500, 50, 150)]
        [TestCase(4, 10, 200, 500, 50, 150)]
        [TestCase(0, 500, 100, 500, 500, 0)]
        [TestCase(1, 500, 100, 500, 500, 1)]
        public void C25h_PoisonUsesPostDecrementCadenceAndNativeDamageRule(
            int poisonType,
            int strength,
            int hp,
            int baseMaxMp,
            int expectedDamage,
            int expectedHp)
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9150 + poisonType, 0);
            entity.Health.HP = hp;
            entity.Runtime.MPMax = baseMaxMp;
            entity.Runtime.PoisonTimer120 = 33;
            entity.Runtime.PoisonType124 = poisonType;
            entity.Runtime.PoisonStrength128 = strength;
            entity.Runtime.InputHpConsumedTotal34C = 11;

            world.LateEntityUpdateAll(1);

            Assert.That(entity.Runtime.PoisonTimer120, Is.EqualTo(32));
            Assert.That(entity.Runtime.InputHpConsumedTotal34C,
                Is.EqualTo(11 + expectedDamage));
            Assert.That(entity.Health.HP, Is.EqualTo(expectedHp));
        }

        [Test]
        public void C25h_RenderPhasePreservesNewFifteenButDecaysAction202Write()
        {
            var world = new SimulationWorld();
            LF2Character caughtExit = CreateCharacter(
                world,
                0,
                9160,
                14,
                wait: 0,
                next: 1,
                extraFrame: Frame(1, 0, 100, 1));
            LF2Character action202 = CreateCharacter(world, 1, 9161, 0,
                frameId: 202);
            caughtExit.Trans.SyncDirectFrameData(0, 1, 0);

            world.LateEntityUpdateAll(1);

            Assert.That(caughtExit.Frame.N, Is.EqualTo(1));
            Assert.That(caughtExit.HitStun, Is.EqualTo(15));
            Assert.That(action202.HitStun, Is.EqualTo(19));
        }

        [Test]
        public void C25h_DeadPrimaryState14RearmsThirtyAfterTowardZeroMove()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9170, 14);
            entity.Health.HP = 0;
            entity.Runtime.HP2Orig = 2;
            entity.HitStun = 1;

            world.LateEntityUpdateAll(1);

            Assert.That(entity.HitStun, Is.EqualTo(30));
        }

        [Test]
        public void C25j_IsRelationIndependentAndProductionSerialDoesNotRecoverTwice()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9180, 0);
            entity.Runtime.LinkState = -1;
            entity.AttackExempt = 4;
            entity.ItrRest.Arest = 4;
            entity.FallCounter = 4;
            entity.Runtime.Bdefend = 4;

            world.LateEntityUpdateAll(1);

            Assert.That(entity.AttackExempt, Is.EqualTo(3));
            Assert.That(entity.ItrRest.Arest, Is.EqualTo(3));
            Assert.That(entity.FallCounter, Is.EqualTo(4));
            Assert.That(entity.Runtime.Bdefend, Is.EqualTo(4));

            entity.Runtime.LinkState = 0;
            world.LateEntityUpdateAll(2);
            world.SerialTickAll(
                2,
                nativeFrameMotionAlreadyApplied: true,
                nativePhysicsAlreadyApplied: true);

            Assert.That(entity.FallCounter, Is.EqualTo(3));
            Assert.That(entity.Runtime.Bdefend, Is.EqualTo(3));
        }

        [Test]
        public void DirectCompatibilityRetainsLegacyFrameAndSerialCounterBehavior()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(world, 0, 9190, 0);
            entity.HitStun = 5;
            entity.FallCounter = 4;
            entity.Runtime.Bdefend = 4;
            entity.HitConfirmCounter = 3;
            entity.HitStateCount = 2;
            entity.AttackExempt = 6;

            entity.SimFrameTick(1);

            Assert.That(entity.HitStun, Is.EqualTo(4));
            Assert.That(entity.FallCounter, Is.EqualTo(3));
            Assert.That(entity.HitConfirmCounter, Is.EqualTo(2));
            Assert.That(entity.HitStateCount, Is.EqualTo(1));
            Assert.That(entity.AttackExempt, Is.EqualTo(5));

            entity.RunTuCoreForSelfCheck();
            entity.RunTuCoreForSelfCheck();

            Assert.That(entity.Runtime.Bdefend, Is.EqualTo(3));
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int state,
            int wait = 100,
            int next = 0,
            int frameId = 0,
            LF2FrameData extraFrame = null)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(frameId, state, wait, next == 0 ? frameId : next),
            };
            if (extraFrame != null)
                frames.Add(extraFrame);
            var data = Data("C25fhj_" + objectId, LF2ObjectType.Character, frames);
            var entity = new LF2Character();
            entity.ModuleInitialize();
            InitializeEntity(entity, world, slot, objectId, data, frameId);
            entity.Initialize(500, 500);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            entity.Health.PP = 500;
            return entity;
        }

        private static LF2OtherObject CreateOther(
            SimulationWorld world,
            int slot,
            int objectId,
            int state)
        {
            var data = Data(
                "C25fhj_other_" + objectId,
                LF2ObjectType.Other,
                new List<LF2FrameData> { Frame(0, state, 100, 0) });
            var entity = new LF2OtherObject();
            InitializeEntity(entity, world, slot, objectId, data, 0);
            entity.Health.HP = 500;
            entity.Runtime.MPMax = 500;
            return entity;
        }

        private static LF2SpecialAttack CreateSpecial(
            SimulationWorld world,
            int slot,
            int objectId,
            int state)
        {
            var data = Data(
                "C25fhj_special_" + objectId,
                LF2ObjectType.SpecialAttack,
                new List<LF2FrameData> { Frame(0, state, 100, 0) });
            var entity = new LF2SpecialAttack();
            InitializeEntity(entity, world, slot, objectId, data, 0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Runtime.MPMax = 500;
            return entity;
        }

        private static void InitializeEntity(
            LF2Entity entity,
            SimulationWorld world,
            int slot,
            int objectId,
            LF2CharacterData data,
            int frameId)
        {
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(frameId);
            entity.Frame.N = frameId;
            entity.Frame.PN = frameId;
            entity.Frame.Prev = frameId;
            entity.Frame.Prev2 = frameId;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = frameId;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.RefreshRuntimeSnapshot();
        }

        private static LF2CharacterData Data(
            string name,
            LF2ObjectType objectType,
            List<LF2FrameData> frames)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)objectType,
                frames = frames,
            };
        }

        private static LF2FrameData Frame(
            int frameId,
            int state,
            int wait,
            int next)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = wait,
                next = next,
                itrs = new List<InteractionArea>(),
            };
        }

        private static void SetTimerBank(NTSDEntityRuntime runtime, int value)
        {
            runtime.BoundState198 = value;
            runtime.InputDoubleCost19C = value;
            runtime.HitResourceInjuryDouble1A0 = value;
            runtime.MpRegenBonusTimer1A4 = value;
            runtime.EffectiveMaxRegenDouble1A8 = value;
            runtime.HpRegenDouble1AC = value;
            runtime.FullRestoreTimer1B0 = value;
            runtime.InputCostWaived1B4 = value;
            runtime.NativeComputerState1B8 = value;
            runtime.NativeTimer1BC = value;
        }

        private static void AssertTimerBank(
            NTSDEntityRuntime runtime,
            int expected,
            int? joinTimer = null)
        {
            Assert.That(runtime.BoundState198, Is.EqualTo(expected));
            Assert.That(runtime.InputDoubleCost19C, Is.EqualTo(expected));
            Assert.That(runtime.HitResourceInjuryDouble1A0, Is.EqualTo(expected));
            Assert.That(runtime.MpRegenBonusTimer1A4, Is.EqualTo(expected));
            Assert.That(runtime.EffectiveMaxRegenDouble1A8, Is.EqualTo(expected));
            Assert.That(runtime.HpRegenDouble1AC, Is.EqualTo(expected));
            Assert.That(runtime.FullRestoreTimer1B0, Is.EqualTo(expected));
            Assert.That(runtime.InputCostWaived1B4, Is.EqualTo(expected));
            Assert.That(runtime.NativeComputerState1B8, Is.EqualTo(expected));
            Assert.That(runtime.NativeTimer1BC, Is.EqualTo(expected));
            if (joinTimer.HasValue)
                Assert.That(runtime.JoinTimer148, Is.EqualTo(joinTimer.Value));
        }

        private static void SetPositiveHpStatus(
            NTSDEntityRuntime runtime,
            int value)
        {
            runtime.InputActionLock130 = value;
            runtime.InputRemapState138 = value;
            runtime.InputProxyCounter14C = value;
            runtime.WeakTimer12C = value;
            runtime.DelayTimer134 = value;
            runtime.JoinTimer148 = value;
            runtime.PoisonTimer120 = value;
        }

        private static void AssertPositiveHpStatus(
            NTSDEntityRuntime runtime,
            int expected)
        {
            Assert.That(runtime.InputActionLock130, Is.EqualTo(expected));
            Assert.That(runtime.InputRemapState138, Is.EqualTo(expected));
            Assert.That(runtime.InputProxyCounter14C, Is.EqualTo(expected));
            Assert.That(runtime.WeakTimer12C, Is.EqualTo(expected));
            Assert.That(runtime.DelayTimer134, Is.EqualTo(expected));
            Assert.That(runtime.JoinTimer148, Is.EqualTo(expected));
            Assert.That(runtime.PoisonTimer120, Is.EqualTo(expected));
        }
    }
}
#endif
