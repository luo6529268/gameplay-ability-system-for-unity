#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6Kind2PickupAtomicProductionIntegrationEditorTests
    {
        public enum ConsumerShell : byte
        {
            RealCharacter,
            GenericCurrentCharacterDat,
            SpecialAttack,
        }

        private readonly struct RelationCase
        {
            internal RelationCase(
                int targetType,
                int targetObjectId,
                int targetHp,
                int targetState,
                int expectedRelation,
                int expectedTargetRelation,
                int expectedAction,
                bool clearWeaponHp)
            {
                TargetType = targetType;
                TargetObjectId = targetObjectId;
                TargetHp = targetHp;
                TargetState = targetState;
                ExpectedRelation = expectedRelation;
                ExpectedTargetRelation = expectedTargetRelation;
                ExpectedAction = expectedAction;
                ClearWeaponHp = clearWeaponHp;
            }

            internal int TargetType { get; }
            internal int TargetObjectId { get; }
            internal int TargetHp { get; }
            internal int TargetState { get; }
            internal int ExpectedRelation { get; }
            internal int ExpectedTargetRelation { get; }
            internal int ExpectedAction { get; }
            internal bool ClearWeaponHp { get; }
        }

        private static IEnumerable<TestCaseData> RelationMatrixCases()
        {
            var relationCases = new[]
            {
                new RelationCase(1, 120, 100, LF2States.WeaponOnGround, 101, -1, 115, false),
                new RelationCase(1, 124, 100, LF2States.WeaponOnGround, 101, -1, 115, false),
                new RelationCase(1, 2001, 100, LF2States.WeaponOnGround, 1, -1, 115, false),
                new RelationCase(2, 1002, 100, LF2States.HeavyWeaponOnGround, 2, -2, 116, false),
                new RelationCase(4, 124, 100, LF2States.WeaponOnGround, 4, -4, 115, false),
                new RelationCase(6, 1006, 100, LF2States.WeaponOnGround, 6, -6, 115, false),
                new RelationCase(6, 1007, 0, LF2States.WeaponOnGround, 4, -4, 115, true),
                new RelationCase(6, 1008, -1, LF2States.WeaponOnGround, 4, -4, 115, true),
            };
            var modes = new[]
            {
                BattleHitExecutionPlanMode.ShadowCompare,
                BattleHitExecutionPlanMode.DataOriented,
            };

            foreach (ConsumerShell shell in Enum.GetValues(typeof(ConsumerShell)))
            {
                foreach (BattleHitExecutionPlanMode mode in modes)
                {
                    foreach (RelationCase relationCase in relationCases)
                    {
                        yield return new TestCaseData(
                                shell,
                                mode,
                                relationCase.TargetType,
                                relationCase.TargetObjectId,
                                relationCase.TargetHp,
                                relationCase.TargetState,
                                relationCase.ExpectedRelation,
                                relationCase.ExpectedTargetRelation,
                                relationCase.ExpectedAction,
                                relationCase.ClearWeaponHp)
                            .SetName(
                                $"{shell}_{mode}_type{relationCase.TargetType}_oid{relationCase.TargetObjectId}");
                    }
                }
            }
        }

        [Test, TestCaseSource(nameof(RelationMatrixCases))]
        public void PickupConsumer_ExecutesExactRelationAndTailAcrossShellsAndModes(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode,
            int targetType,
            int targetObjectId,
            int targetHp,
            int targetState,
            int expectedRelation,
            int expectedTargetRelation,
            int expectedAction,
            bool clearWeaponHp)
        {
            using (var scenario = CreatePickupScenario(
                       shell,
                       targetType,
                       targetObjectId,
                       targetHp,
                       targetState,
                       targetWpointAction: 0,
                       oldRelation: 0,
                       withOldChild: false))
            {
                CaptureCandidates(scenario, injectForNonCharacterShell: shell != ConsumerShell.RealCharacter);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));

                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);

                Assert.That(scenario.Attacker.Runtime.LinkState, Is.EqualTo(expectedRelation));
                Assert.That(scenario.Target.Runtime.LinkState, Is.EqualTo(expectedTargetRelation));
                Assert.That(scenario.Attacker.Runtime.TargetSlotIndex,
                    Is.EqualTo(scenario.Target.Runtime.SlotIndex));
                Assert.That(scenario.Attacker.Runtime.HeldWeaponStableId,
                    Is.EqualTo(scenario.Target.Runtime.SlotIndex));
                Assert.That(scenario.Target.Runtime.HolderStableId,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.Target.Runtime.OwnerSlotIndex,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.Target.HolderCopySlot,
                    Is.EqualTo(733),
                    "HolderCopySlot is outside the shared kind2 transaction.");
                Assert.That(scenario.Attacker.Runtime.PickupCount, Is.EqualTo(6));
                Assert.That(scenario.Attacker.AttackingCounter, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.Frame,
                    Is.EqualTo(expectedAction));
                Assert.That(scenario.Attacker.Runtime.FrameWaitCounter, Is.EqualTo(17));

                if (clearWeaponHp)
                    Assert.That(scenario.Target.Runtime.WeaponFlightCounter, Is.Zero);
                else
                    Assert.That(scenario.Target.Runtime.WeaponFlightCounter, Is.EqualTo(31));

                if (scenario.Attacker is LF2Character character)
                    Assert.That(character.GetHeldWeapon(), Is.SameAs(scenario.Target));

                AssertPlanModeResult(scenario, mode, expectedWriterEffects: 1);
            }
        }

        [Test]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.DataOriented)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.DataOriented)]
        public void PickupConsumer_State2004ReplacesOldChildWithoutFreeOrUnlink(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode)
        {
            using (var scenario = CreatePickupScenario(
                       shell,
                       targetType: 2,
                       targetObjectId: 1202,
                       targetHp: 100,
                       targetState: LF2States.HeavyWeaponOnGround,
                       targetWpointAction: 0,
                       oldRelation: 2,
                       withOldChild: true))
            {
                CaptureCandidates(scenario, injectForNonCharacterShell: shell != ConsumerShell.RealCharacter);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));

                int oldLink = scenario.OldChild.Runtime.LinkState;
                int oldHolder = scenario.OldChild.Runtime.HolderStableId;
                int oldHolderCopy = scenario.OldChild.HolderCopySlot;
                int oldOwner = scenario.OldChild.Runtime.OwnerSlotIndex;
                int oldTarget = scenario.OldChild.Runtime.TargetSlotIndex;
                int oldHeld = scenario.OldChild.Runtime.HeldWeaponStableId;
                bool oldPendingDestroy = scenario.OldChild.Runtime.PendingFlushDestroy;

                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);

                Assert.That(scenario.Attacker.Runtime.LinkState, Is.EqualTo(2));
                Assert.That(scenario.Attacker.Runtime.TargetSlotIndex,
                    Is.EqualTo(scenario.Target.Runtime.SlotIndex));
                Assert.That(scenario.Attacker.Runtime.HeldWeaponStableId,
                    Is.EqualTo(scenario.Target.Runtime.SlotIndex));
                Assert.That(scenario.Attacker.Runtime.PickupCount,
                    Is.EqualTo(5),
                    "The count gate observes the prior relation before replacement.");
                Assert.That(scenario.Attacker.Runtime.Frame,
                    Is.EqualTo(LF2StandardFrames.PickingHeavy));
                Assert.That(scenario.Attacker.Runtime.FrameWaitCounter, Is.EqualTo(17));
                Assert.That(scenario.Target.Runtime.LinkState, Is.EqualTo(-2));
                Assert.That(scenario.Target.Runtime.HolderStableId,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.Target.Runtime.OwnerSlotIndex,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.Target.HolderCopySlot, Is.EqualTo(733));

                Assert.That(scenario.OldChild.Runtime.LinkState, Is.EqualTo(oldLink));
                Assert.That(scenario.OldChild.Runtime.HolderStableId, Is.EqualTo(oldHolder));
                Assert.That(scenario.OldChild.HolderCopySlot, Is.EqualTo(oldHolderCopy));
                Assert.That(scenario.OldChild.Runtime.OwnerSlotIndex, Is.EqualTo(oldOwner));
                Assert.That(scenario.OldChild.Runtime.TargetSlotIndex, Is.EqualTo(oldTarget));
                Assert.That(scenario.OldChild.Runtime.HeldWeaponStableId, Is.EqualTo(oldHeld));
                Assert.That(scenario.OldChild.Runtime.PendingFlushDestroy,
                    Is.EqualTo(oldPendingDestroy));
                Assert.That(
                    scenario.World.FindEntityByRuntimeSlotForQuery(
                        scenario.OldChild.Runtime.SlotIndex),
                    Is.SameAs(scenario.OldChild),
                    "Replacement must not Free or unlink the old child.");

                if (scenario.Attacker is LF2Character character)
                    Assert.That(character.GetHeldWeapon(), Is.SameAs(scenario.Target));

                AssertPlanModeResult(scenario, mode, expectedWriterEffects: 1);
            }
        }

        [Test]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.DataOriented)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.DataOriented)]
        public void Kind7Candidate_IsClassifiedButProducesZeroPickupWrites(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode)
        {
            using (var scenario = CreatePickupScenario(
                       shell,
                       targetType: 1,
                       targetObjectId: 120,
                       targetHp: 100,
                       targetState: LF2States.WeaponOnGround,
                       targetWpointAction: 333,
                       oldRelation: 0,
                       withOldChild: false,
                       interactionKind: 7))
            {
                CaptureCandidates(scenario, injectForNonCharacterShell: shell != ConsumerShell.RealCharacter);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));

                int frameBefore = scenario.Attacker.Runtime.Frame;
                int frameWaitBefore = scenario.Attacker.Runtime.FrameWaitCounter;
                int relationBefore = scenario.Attacker.Runtime.LinkState;
                int countBefore = scenario.Attacker.Runtime.PickupCount;
                int targetRelationBefore = scenario.Target.Runtime.LinkState;
                int targetHolderBefore = scenario.Target.Runtime.HolderStableId;
                int targetOwnerBefore = scenario.Target.Runtime.OwnerSlotIndex;
                int targetHolderCopyBefore = scenario.Target.HolderCopySlot;

                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);

                Assert.That(scenario.Attacker.Runtime.Frame, Is.EqualTo(frameBefore));
                Assert.That(scenario.Attacker.Runtime.FrameWaitCounter,
                    Is.EqualTo(frameWaitBefore));
                Assert.That(scenario.Attacker.Runtime.LinkState, Is.EqualTo(relationBefore));
                Assert.That(scenario.Attacker.Runtime.PickupCount, Is.EqualTo(countBefore));
                Assert.That(scenario.Target.Runtime.LinkState,
                    Is.EqualTo(targetRelationBefore));
                Assert.That(scenario.Target.Runtime.HolderStableId,
                    Is.EqualTo(targetHolderBefore));
                Assert.That(scenario.Target.Runtime.OwnerSlotIndex,
                    Is.EqualTo(targetOwnerBefore));
                Assert.That(scenario.Target.HolderCopySlot,
                    Is.EqualTo(targetHolderCopyBefore));
                Assert.That(scenario.Target.Runtime.WeaponFlightCounter, Is.EqualTo(31));

                if (scenario.Attacker is LF2Character character)
                    Assert.That(character.GetHeldWeapon(), Is.Null);

                AssertPlanModeResult(scenario, mode, expectedWriterEffects: 0);
            }
        }

        [Test]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare, 0)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare, 333)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare, -888)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare, 1000)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented, 0)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented, 333)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented, -888)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented, 1000)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.ShadowCompare, 0)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.ShadowCompare, 333)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.ShadowCompare, -888)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.ShadowCompare, 1000)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.DataOriented, 0)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.DataOriented, 333)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.DataOriented, -888)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.DataOriented, 1000)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.ShadowCompare, 0)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.ShadowCompare, 333)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.ShadowCompare, -888)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.ShadowCompare, 1000)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.DataOriented, 0)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.DataOriented, 333)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.DataOriented, -888)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.DataOriented, 1000)]
        public void UnsupportedTarget_AppliesFrameCounterAndLiteralWpointTail(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode,
            int targetWpointAction)
        {
            using (var scenario = CreateUnsupportedTargetScenario(shell, targetWpointAction))
            {
                CaptureCandidates(scenario, injectForNonCharacterShell: shell != ConsumerShell.RealCharacter);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));

                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);

                Assert.That(scenario.Attacker.Runtime.LinkState, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.TargetSlotIndex, Is.EqualTo(-1));
                Assert.That(scenario.Attacker.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
                Assert.That(scenario.Attacker.Runtime.PickupCount, Is.EqualTo(5));
                Assert.That(scenario.Attacker.AttackingCounter, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.FrameWaitCounter, Is.EqualTo(17));
                Assert.That(scenario.Attacker.Runtime.Frame,
                    Is.EqualTo(targetWpointAction == 0 ? 0 : targetWpointAction));
                Assert.That(scenario.Target.Runtime.LinkState, Is.Zero);
                Assert.That(scenario.Target.Runtime.HolderStableId, Is.EqualTo(-1));
                Assert.That(scenario.Target.Runtime.OwnerSlotIndex, Is.EqualTo(91));
                Assert.That(scenario.Target.HolderCopySlot, Is.EqualTo(733));

                if (scenario.Attacker is LF2Character character)
                    Assert.That(character.GetHeldWeapon(), Is.Null);

                AssertPlanModeResult(scenario, mode, expectedWriterEffects: 1);
            }
        }

        [Test]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.GenericCurrentCharacterDat, BattleHitExecutionPlanMode.DataOriented)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.SpecialAttack, BattleHitExecutionPlanMode.DataOriented)]
        public void SameTickCandidates_CommitInCandidateOrderAndCountOnlyOnce(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode)
        {
            using (var scenario = CreatePickupScenario(
                       shell,
                       targetType: 1,
                       targetObjectId: 120,
                       targetHp: 100,
                       targetState: LF2States.WeaponOnGround,
                       targetWpointAction: 0,
                       oldRelation: 0,
                       withOldChild: false,
                       interactionKind: 2,
                       secondTarget: true))
            {
                CaptureCandidates(scenario, injectForNonCharacterShell: shell != ConsumerShell.RealCharacter);
                Assert.That(scenario.CandidateCount, Is.EqualTo(2));

                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);

                Assert.That(scenario.FirstTarget.Runtime.LinkState, Is.EqualTo(-1));
                Assert.That(scenario.SecondTarget.Runtime.LinkState, Is.EqualTo(-1));
                Assert.That(scenario.FirstTarget.Runtime.HolderStableId,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.SecondTarget.Runtime.HolderStableId,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.FirstTarget.Runtime.OwnerSlotIndex,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.SecondTarget.Runtime.OwnerSlotIndex,
                    Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.Attacker.Runtime.LinkState, Is.EqualTo(1));
                Assert.That(scenario.Attacker.Runtime.TargetSlotIndex,
                    Is.EqualTo(scenario.SecondTarget.Runtime.SlotIndex));
                Assert.That(scenario.Attacker.Runtime.HeldWeaponStableId,
                    Is.EqualTo(scenario.SecondTarget.Runtime.SlotIndex));
                Assert.That(scenario.Attacker.Runtime.PickupCount,
                    Is.EqualTo(6),
                    "The second candidate sees the first relation write.");
                Assert.That(scenario.FirstTarget.HolderCopySlot, Is.EqualTo(733));
                Assert.That(scenario.SecondTarget.HolderCopySlot, Is.EqualTo(733));

                if (scenario.Attacker is LF2Character character)
                    Assert.That(character.GetHeldWeapon(), Is.SameAs(scenario.SecondTarget));

                AssertPlanModeResult(scenario, mode, expectedWriterEffects: 2);
            }
        }

        [Test]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented)]
        public void CandidateGate_EffectAndVrestRejectBeforeConsumer(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode)
        {
            using (var effectScenario = CreatePickupScenario(
                       shell,
                       targetType: 1,
                       targetObjectId: 150,
                       targetHp: 100,
                       targetState: LF2States.WeaponOnGround,
                       targetWpointAction: 0,
                       oldRelation: 0,
                       withOldChild: false,
                       interactionKind: 2,
                       interactionEffect: 13))
            {
                CaptureCandidates(effectScenario, injectForNonCharacterShell: false);
                Assert.That(effectScenario.CandidateCount, Is.Zero,
                    "effect=13 is character-only and must reject a weapon target at collection.");
                effectScenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                effectScenario.World.PostInteractionTickAll(effectScenario.Tick);
                Assert.That(effectScenario.Attacker.Runtime.PickupCount, Is.EqualTo(5));
                Assert.That(effectScenario.Attacker.Runtime.LinkState, Is.Zero);
            }

            using (var restScenario = CreatePickupScenario(
                       shell,
                       targetType: 1,
                       targetObjectId: 151,
                       targetHp: 100,
                       targetState: LF2States.WeaponOnGround,
                       targetWpointAction: 0,
                       oldRelation: 0,
                       withOldChild: false,
                       interactionKind: 2))
            {
                restScenario.Target.ItrRest.SetVrest(
                    restScenario.Attacker.Runtime.SlotIndex,
                    9);
                CaptureCandidates(restScenario, injectForNonCharacterShell: false);
                Assert.That(restScenario.CandidateCount, Is.Zero,
                    "target-side vrest is a candidate gate, before any pickup writer.");
                restScenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                restScenario.World.PostInteractionTickAll(restScenario.Tick);
                Assert.That(restScenario.Attacker.Runtime.PickupCount, Is.EqualTo(5));
                Assert.That(restScenario.Attacker.Runtime.LinkState, Is.Zero);
            }
        }

        [Test]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented)]
        public void CandidateGate_PhaseSuppressionLeavesCapturedCandidateUnconsumed(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode)
        {
            using (var scenario = CreatePickupScenario(
                       shell,
                       targetType: 1,
                       targetObjectId: 152,
                       targetHp: 100,
                       targetState: LF2States.WeaponOnGround,
                       targetWpointAction: 0,
                       oldRelation: 0,
                       withOldChild: false))
            {
                CaptureCandidates(scenario, injectForNonCharacterShell: false);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));
                scenario.Attacker.Runtime.SuppressPostInteractionUntilTick = scenario.Tick + 1;

                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);

                Assert.That(scenario.Attacker.Runtime.PickupCount, Is.EqualTo(5));
                Assert.That(scenario.Attacker.Runtime.LinkState, Is.Zero);
                Assert.That(scenario.Target.Runtime.LinkState, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.Frame, Is.Zero);
            }
        }

        [Test]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(ConsumerShell.RealCharacter, BattleHitExecutionPlanMode.DataOriented)]
        public void CandidateGate_SpecialHitLatchStopsCharacterTargetBeforeTail(
            ConsumerShell shell,
            BattleHitExecutionPlanMode mode)
        {
            using (var scenario = CreateUnsupportedTargetScenario(shell, targetWpointAction: 333, targetType: 0))
            {
                CaptureCandidates(scenario, injectForNonCharacterShell: false);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));
                scenario.Attacker.Runtime.SpecialHitLatch0EB = true;

                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);

                Assert.That(scenario.Attacker.Runtime.SpecialHitLatch0EB, Is.True);
                Assert.That(scenario.Attacker.Runtime.PickupCount, Is.EqualTo(5));
                Assert.That(scenario.Attacker.Runtime.LinkState, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.Frame, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.FrameWaitCounter, Is.EqualTo(17));
                Assert.That(scenario.Target.Runtime.LinkState, Is.Zero);
                Assert.That(scenario.Target.Runtime.HolderStableId, Is.EqualTo(-1));
            }
        }

        [TestCase(LF2States.WeaponOnGround, 0, 1, 0, true)]
        [TestCase(LF2States.WeaponOnGround, 0, 1, 1, false)]
        [TestCase(LF2States.WeaponOnGround, 0, 0, 0, false)]
        [TestCase(LF2States.WeaponOnGround, 2, 1, 0, false)]
        [TestCase(LF2States.HeavyWeaponOnGround, 0, 1, 0, true)]
        [TestCase(LF2States.HeavyWeaponOnGround, 2, 1, 0, true)]
        [TestCase(LF2States.HeavyWeaponOnGround, 2, 1, 1, false)]
        [TestCase(LF2States.HeavyWeaponOnGround, 2, 0, 0, false)]
        public void RealCharacterCollector_UsesTargetStateRelationAndRisingJumpGate(
            int targetState,
            int oldRelation,
            int keyJump,
            int previousJump,
            bool expectedCandidate)
        {
            using (var scenario = CreatePickupScenario(
                       ConsumerShell.RealCharacter,
                       targetType: targetState == LF2States.HeavyWeaponOnGround ? 2 : 1,
                       targetObjectId: targetState == LF2States.HeavyWeaponOnGround ? 1602 : 1601,
                       targetHp: 100,
                       targetState: targetState,
                       targetWpointAction: 0,
                       oldRelation: oldRelation,
                       withOldChild: oldRelation != 0))
            {
                scenario.Attacker.Runtime.KeyJump = (byte)keyJump;
                scenario.Attacker.Runtime.PrevJump = (byte)previousJump;
                CaptureCandidates(scenario, injectForNonCharacterShell: false);
                Assert.That(
                    scenario.CandidateCount == 1,
                    Is.EqualTo(expectedCandidate),
                    $"targetState={targetState}, oldRelation={oldRelation}, " +
                    $"KeyJump={keyJump}, PrevJump={previousJump}");
            }
        }

        private static IEnumerable<TestCaseData> SupportedTailCases()
        {
            foreach (ConsumerShell shell in Enum.GetValues(typeof(ConsumerShell)))
            foreach (BattleHitExecutionPlanMode mode in new[] { BattleHitExecutionPlanMode.ShadowCompare, BattleHitExecutionPlanMode.DataOriented })
            foreach (int action in new[] { 0, 333, -888, 1000 })
                yield return new TestCaseData(shell, mode, action);
        }

        [TestCaseSource(nameof(SupportedTailCases))]
        public void SupportedTarget_WpointIsTheLiteralFinalAction(
            ConsumerShell shell, BattleHitExecutionPlanMode mode, int action)
        {
            using (var scenario = CreatePickupScenario(shell, 1, 120, 100, 1004, action, 0, false))
            {
                CaptureCandidates(scenario, shell != ConsumerShell.RealCharacter);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));
                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);
                Assert.That(scenario.Attacker.Runtime.LinkState, Is.EqualTo(101));
                Assert.That(scenario.Target.Runtime.LinkState, Is.EqualTo(-1));
                Assert.That(scenario.Attacker.Frame.N, Is.EqualTo(action == 0 ? 115 : action));
                Assert.That(scenario.Attacker.AttackingCounter, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.PickupCount, Is.EqualTo(6));
                Assert.That(scenario.Target.Runtime.OwnerSlotIndex, Is.EqualTo(scenario.Attacker.Runtime.SlotIndex));
                Assert.That(scenario.Target.HolderCopySlot, Is.EqualTo(733));
                if (scenario.Attacker is LF2Character character)
                    Assert.That(character.HeldWeaponReferenceInternal, Is.SameAs(scenario.Target));
                AssertPlanModeResult(scenario, mode, 1);
            }
        }

        private static IEnumerable<TestCaseData> MissingCurrentFrameCases()
        {
            foreach (ConsumerShell shell in Enum.GetValues(typeof(ConsumerShell)))
            foreach (BattleHitExecutionPlanMode mode in new[] { BattleHitExecutionPlanMode.ShadowCompare, BattleHitExecutionPlanMode.DataOriented })
            foreach (bool supportedType in new[] { false, true })
                yield return new TestCaseData(shell, mode, supportedType);
        }

        [TestCaseSource(nameof(MissingCurrentFrameCases))]
        public void MissingCurrentFrame_AfterCollectionStillAppliesTail(
            ConsumerShell shell, BattleHitExecutionPlanMode mode, bool supportedType)
        {
            using (var scenario = supportedType
                ? CreatePickupScenario(shell, 1, 120, 100, 1004, 333, 0, false)
                : CreateUnsupportedTargetScenario(shell, 333))
            {
                CaptureCandidates(scenario, shell != ConsumerShell.RealCharacter);
                Assert.That(scenario.CandidateCount, Is.EqualTo(1));
                scenario.Target.DirectWriteRawFramePreserveWaitCounter(-888);
                Assert.That(scenario.Target.Frame.D, Is.Null);
                scenario.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                Consume(scenario);
                Assert.That(scenario.Attacker.Frame.N, Is.EqualTo(supportedType ? 115 : 0));
                Assert.That(scenario.Attacker.AttackingCounter, Is.Zero);
                Assert.That(scenario.Attacker.Runtime.PickupCount, Is.EqualTo(supportedType ? 6 : 5));
                Assert.That(scenario.Attacker.Runtime.LinkState, Is.EqualTo(supportedType ? 101 : 0));
                Assert.That(scenario.Target.Frame.N, Is.EqualTo(-888));
                if (scenario.Attacker is LF2Character character)
                    Assert.That(character.HeldWeaponReferenceInternal, supportedType ? Is.SameAs(scenario.Target) : Is.Null);
                AssertPlanModeResult(scenario, mode, 1);
            }
        }

        private static void AssertPlanModeResult(
            PickupScenario scenario,
            BattleHitExecutionPlanMode mode,
            int expectedWriterEffects)
        {
            BattleHitExecutionPlanDiagnostics diagnostics =
                scenario.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.Mode, Is.EqualTo(mode));
            Assert.That(diagnostics.CurrentTickPlanValid, Is.True,
                DescribeDiagnostics(diagnostics));
            Assert.That(diagnostics.FailureCount, Is.Zero,
                DescribeDiagnostics(diagnostics));

            if (mode == BattleHitExecutionPlanMode.ShadowCompare)
            {
                Assert.That(diagnostics.ObservedWriterEffectCount,
                    Is.EqualTo(expectedWriterEffects),
                    DescribeDiagnostics(diagnostics));
                Assert.That(diagnostics.LastWriterEffectDifferenceMask,
                    Is.Zero,
                    DescribeDiagnostics(diagnostics));
                Assert.That(diagnostics.ObservationMismatchCount, Is.Zero,
                    DescribeDiagnostics(diagnostics));
            }
        }

        private static string DescribeDiagnostics(
            BattleHitExecutionPlanDiagnostics diagnostics)
        {
            return $"mode={diagnostics.Mode}, valid={diagnostics.CurrentTickPlanValid}, " +
                   $"failures={diagnostics.FailureCount}, " +
                   $"writerEffects={diagnostics.ObservedWriterEffectCount}, " +
                   $"writerMask={diagnostics.LastWriterEffectDifferenceMask}, " +
                   $"mismatches={diagnostics.ObservationMismatchCount}";
        }

        private static PickupScenario CreateUnsupportedTargetScenario(
            ConsumerShell shell,
            int targetWpointAction,
            int targetType = 3)
        {
            var world = new SimulationWorld();
            LF2Entity attacker = CreateAttacker(world, shell, 0, 7800);
            LF2Entity target = CreateCharacterTarget(
                world,
                slot: 1,
                objectId: 7801,
                state: LF2States.WeaponOnGround,
                wpointAction: targetWpointAction,
                currentType: targetType);
            return new PickupScenario(
                world,
                attacker,
                target,
                null,
                null,
                tick: 880,
                secondTarget: null);
        }

        private static PickupScenario CreatePickupScenario(
            ConsumerShell shell,
            int targetType,
            int targetObjectId,
            int targetHp,
            int targetState,
            int targetWpointAction,
            int oldRelation,
            bool withOldChild,
            int interactionKind = 2,
            int interactionEffect = 0,
            bool secondTarget = false)
        {
            var world = new SimulationWorld();
            LF2Entity attacker = CreateAttacker(world, shell, 0, 7700);
            LF2Weapon target = CreateWeapon(
                world,
                slot: 1,
                objectId: targetObjectId,
                weaponType: targetType,
                state: targetState,
                hp: targetHp,
                x: 0,
                wpointAction: targetWpointAction);
            LF2Weapon oldChild = null;
            if (withOldChild)
                oldChild = CreateWeapon(
                    world,
                    slot: 2,
                    objectId: 7799,
                    weaponType: 2,
                    state: LF2States.HeavyWeaponOnGround,
                    hp: 100,
                    x: 1000,
                    wpointAction: 0);

            if (withOldChild)
                AttachOldChildForReplacement(attacker, oldChild, oldRelation);
            else
                SetRelationDefaults(attacker, relation: oldRelation);

            LF2Weapon second = null;
            if (secondTarget)
            {
                second = CreateWeapon(
                    world,
                    slot: 2,
                    objectId: 7702,
                    weaponType: 1,
                    state: LF2States.WeaponOnGround,
                    hp: 100,
                    x: 20,
                    wpointAction: 0);
            }

            InteractionArea interaction = attacker.Frame.D.itrs[0];
            interaction.kind = interactionKind;
            interaction.effect = interactionEffect;
            return new PickupScenario(
                world,
                attacker,
                target,
                oldChild,
                interaction,
                tick: 881,
                secondTarget: second);
        }

        private static void Consume(PickupScenario scenario)
        {
            if (scenario.Attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                scenario.World.PostInteractionTickAll(scenario.Tick);
            else
                scenario.World.ObjectInteractionTickAll(scenario.Tick);
        }

        private static void CaptureCandidates(
            PickupScenario scenario,
            bool injectForNonCharacterShell)
        {
            var query = (BruteForceSceneQuery)scenario.World.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
            scenario.World.CaptureCollisionFrameSnapshotsAll();
            scenario.World.CollectCollisionCandidatesAll();

            if (injectForNonCharacterShell)
            {
                BattleHitCandidatePairSnapshot pairSnapshot = BattleHitCandidatePairSnapshotFactory.Capture(
                    scenario.Attacker, scenario.Target, scenario.World);
                Assert.That(pairSnapshot.Valid, Is.True);
                var hit = new SceneQueryHit(scenario.Target, 0, 0, pairSnapshot: pairSnapshot);
                Assert.That(
                    query.TryAppendCollisionCandidateLegacyOracleForSelfCheck(
                        scenario.Attacker,
                        in hit),
                    Is.True,
                    "The existing candidate-cache fixture must be available for " +
                    "non-LF2Character current-DAT shells.");
                scenario.Attacker.Runtime.HitCandidateCount = 1;
                if (scenario.SecondTarget != null)
                {
                    BattleHitCandidatePairSnapshot secondPairSnapshot = BattleHitCandidatePairSnapshotFactory.Capture(
                        scenario.Attacker, scenario.SecondTarget, scenario.World);
                    Assert.That(secondPairSnapshot.Valid, Is.True);
                    var secondHit = new SceneQueryHit(scenario.SecondTarget, 20, 0, pairSnapshot: secondPairSnapshot);
                    Assert.That(
                        query.TryAppendCollisionCandidateLegacyOracleForSelfCheck(
                            scenario.Attacker,
                            in secondHit),
                        Is.True);
                    scenario.Attacker.Runtime.HitCandidateCount = 2;
                }
            }

            Assert.That(
                scenario.World.SceneQuery.TryGetCollisionCandidateRange(
                    scenario.Attacker,
                    out CollisionCandidateRange candidates),
                Is.True);
            scenario.CandidateCount = candidates.Count;
        }

        private static LF2Entity CreateAttacker(
            SimulationWorld world,
            ConsumerShell shell,
            int slot,
            int objectId)
        {
            LF2FrameData current = Frame(
                frameId: 0,
                state: LF2States.Standing,
                body: false,
                wpointAction: 0);
            current.itrs.Add(new InteractionArea
            {
                kind = 2,
                x = -100,
                y = -20,
                w = 240,
                h = 40,
                zwidth = 15,
                arest = 0,
                vrest = 1,
            });
            var frames = new List<LF2FrameData>
            {
                current,
                Frame(7, LF2States.Standing, false, 0),
                Frame(LF2StandardFrames.PickingLight, LF2States.Standing, false, 0),
                Frame(LF2StandardFrames.PickingHeavy, LF2States.Standing, false, 0),
            };
            var data = new LF2CharacterData
            {
                name = "NTSD28B6PickupAtomicAttacker",
                type_sub = shell == ConsumerShell.SpecialAttack
                    ? (int)LF2ObjectType.SpecialAttack
                    : (int)LF2ObjectType.Character,
                frames = frames,
            };
            LF2Entity entity;
            switch (shell)
            {
                case ConsumerShell.RealCharacter:
                    entity = new LF2Character();
                    ((LF2Character)entity).ModuleInitialize();
                    break;
                case ConsumerShell.GenericCurrentCharacterDat:
                    entity = new GenericCurrentCharacterDatShell();
                    break;
                default:
                    entity = new LF2SpecialAttack();
                    break;
            }

            entity.Name = "NTSD28B6PickupAtomicAttacker";
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            SetFrame(entity, current);
            if (entity is LF2Character character)
                character.Initialize(500, 500);
            Register(world, entity, slot, team: 7, x: 0);
            entity.Runtime.LinkState = 0;
            entity.Runtime.TargetSlotIndex = -1;
            entity.Runtime.HeldWeaponStableId = -1;
            entity.Runtime.HolderStableId = -1;
            entity.Runtime.OwnerSlotIndex = 91;
            entity.Runtime.PickupCount = 5;
            entity.Runtime.FrameWaitCounter = 17;
            entity.AttackingCounter = 9;
            entity.Runtime.KeyJump = 1;
            entity.Runtime.PrevJump = 0;
            entity.Runtime.SpecialHitLatch0EB = false;
            return entity;
        }

        private static LF2Weapon CreateWeapon(
            SimulationWorld world,
            int slot,
            int objectId,
            int weaponType,
            int state,
            int hp,
            int x,
            int wpointAction)
        {
            var data = new LF2CharacterData
            {
                name = "NTSD28B6PickupAtomicWeapon" + objectId,
                type_sub = weaponType,
                weapon_hp = 31,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state, true, wpointAction),
                    Frame(115, state, false, 0),
                    Frame(116, state, false, 0),
                },
            };
            var weapon = new FixtureWeapon(weaponType);
            weapon.SetWeaponType(weaponType);
            weapon.Name = "NTSD28B6PickupAtomicWeapon" + objectId;
            weapon.ObjectId = objectId;
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            SetFrame(weapon, data.frames[0]);
            Register(world, weapon, slot, team: 8, x: x);
            weapon.Health.HP = hp;
            weapon.Health.HPBound = Math.Max(hp, 100);
            weapon.Health.HP3 = Math.Max(hp, 100);
            weapon.Runtime.WeaponFlightCounter = 31;
            weapon.Runtime.LinkState = 0;
            weapon.Runtime.HolderStableId = -1;
            weapon.Runtime.OwnerSlotIndex = 91;
            weapon.Runtime.TargetSlotIndex = -1;
            weapon.Runtime.HeldWeaponStableId = -1;
            weapon.HolderCopySlot = 733;
            weapon.RefreshRuntimeSnapshot();
            return weapon;
        }

        private static LF2Entity CreateCharacterTarget(
            SimulationWorld world,
            int slot,
            int objectId,
            int state,
            int wpointAction,
            int currentType = 0)
        {
            var data = new LF2CharacterData
            {
                name = "NTSD28B6PickupAtomicCharacterTarget",
                type_sub = currentType,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state, true, wpointAction),
                },
            };
            LF2Entity target = currentType == 0 ? new LF2Character() : new LF2SpecialAttack();
            if (target is LF2Character characterTarget)
                characterTarget.ModuleInitialize();
            target.Name = "NTSD28B6PickupAtomicCharacterTarget";
            target.ObjectId = objectId;
            target.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            SetFrame(target, data.frames[0]);
            if (target is LF2Character initializedCharacter)
                initializedCharacter.Initialize(500, 500);
            Register(world, target, slot, team: 8, x: 0);
            target.Health.HP = 100;
            target.Health.HPBound = 100;
            target.Health.HP3 = 100;
            target.Runtime.LinkState = 0;
            target.Runtime.HolderStableId = -1;
            target.Runtime.OwnerSlotIndex = 91;
            target.Runtime.TargetSlotIndex = -1;
            target.Runtime.HeldWeaponStableId = -1;
            target.HolderCopySlot = 733;
            target.RefreshRuntimeSnapshot();
            return target;
        }

        private static void AttachOldChildForReplacement(
            LF2Entity attacker,
            LF2Weapon oldChild,
            int relation)
        {
            if (attacker is LF2Character character)
                character.HoldWeapon(oldChild);
            else
            {
                attacker.Runtime.LinkState = relation;
                attacker.Runtime.TargetSlotIndex = oldChild.Runtime.SlotIndex;
                attacker.Runtime.HeldWeaponStableId = oldChild.Runtime.SlotIndex;
            }

            attacker.Runtime.LinkState = relation;
            attacker.Runtime.TargetSlotIndex = oldChild.Runtime.SlotIndex;
            attacker.Runtime.HeldWeaponStableId = oldChild.Runtime.SlotIndex;
            oldChild.Runtime.LinkState = -relation;
            oldChild.Runtime.HolderStableId = attacker.Runtime.SlotIndex;
            oldChild.HolderCopySlot = 73;
            oldChild.Runtime.OwnerSlotIndex = 91;
            oldChild.Runtime.TargetSlotIndex = 22;
            oldChild.Runtime.HeldWeaponStableId = 88;
        }

        private static void SetRelationDefaults(LF2Entity entity, int relation)
        {
            entity.Runtime.LinkState = relation;
            entity.Runtime.TargetSlotIndex = -1;
            entity.Runtime.HeldWeaponStableId = -1;
        }

        private static LF2FrameData Frame(
            int frameId,
            int state,
            bool body,
            int wpointAction)
        {
            var frame = new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = frameId,
                centerx = 0,
                centery = 0,
            };
            if (body)
            {
                frame.bodies.Add(new BattleBodyBoxValue(-10, -10, 20, 20));
            }
            if (wpointAction != 0 || frameId == 0)
            {
                frame.wpoints.Add(new WeaponPoint
                {
                    weaponact = wpointAction,
                });
            }
            return frame;
        }

        private static void SetFrame(LF2Entity entity, LF2FrameData frame)
        {
            entity.Frame.D = frame;
            entity.Frame.N = frame.frameId;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = frame.frameId;
            entity.Frame.Prev2D = frame;
            entity.Runtime.PrevFrame2 = frame.frameId;
        }

        private static void Register(
            SimulationWorld world,
            LF2Entity entity,
            int slot,
            int team,
            int x)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.SwitchDir("right");
            entity.RefreshRuntimeSnapshot();
        }

        private sealed class FixtureWeapon : LF2Weapon
        {
            private readonly int currentType;
            internal FixtureWeapon(int currentType) { this.currentType = currentType; }
            public override int GetCurrentDataObjectTypeForSimulation() => currentType;
        }

        private sealed class GenericCurrentCharacterDatShell : LF2SpecialAttack
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Character;
            }
        }

        private sealed class PickupScenario : IDisposable
        {
            internal PickupScenario(
                SimulationWorld world,
                LF2Entity attacker,
                LF2Entity target,
                LF2Weapon oldChild,
                InteractionArea interaction,
                int tick,
                LF2Weapon secondTarget)
            {
                World = world;
                Attacker = attacker;
                Target = target;
                OldChild = oldChild;
                Interaction = interaction;
                Tick = tick;
                SecondTarget = secondTarget;
            }

            internal SimulationWorld World { get; }
            internal LF2Entity Attacker { get; }
            internal LF2Entity Target { get; }
            internal LF2Weapon PickupTarget => (LF2Weapon)Target;
            internal LF2Weapon OldChild { get; }
            internal LF2Weapon FirstTarget => (LF2Weapon)Target;
            internal LF2Weapon SecondTarget { get; }
            internal InteractionArea Interaction { get; }
            internal int Tick { get; }
            internal int CandidateCount { get; set; }

            public void Dispose()
            {
                World.EndCollisionCandidateConsumption();
                if (SecondTarget != null)
                    World.Unregister(SecondTarget);
                if (OldChild != null)
                    World.Unregister(OldChild);
                if (Target != null)
                    World.Unregister(Target);
                if (Attacker != null)
                    World.Unregister(Attacker);
            }
        }
    }
}
#endif

#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
namespace NTSD.Test.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NTSD.Animation;
    using NTSD.Animation.LF2Objects;
    using NTSD.Animation.LF2Tasks;
    using NTSD.Simulation;
    using NTSD.Simulation.Ecs;
    using UnityEditor;
    using UnityEngine;

    internal static class NTSD28B6Kind2PickupAtomicPlayProbe
    {
        private const string Request = "Temp/Goal16_Play.request";
        private const string Result = "Temp/Goal16_Play.result.json";
        private static bool pausing;
        private static string consoleError;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            Application.logMessageReceived -= Log;
            Application.logMessageReceived += Log;
        }

        private static void Log(string message, string stack, LogType type)
        {
            if (File.Exists(Request) && EditorApplication.isPlaying &&
                (type == LogType.Error || type == LogType.Assert || type == LogType.Exception))
                consoleError = message;
        }

        private static void Poll()
        {
            if (!File.Exists(Request) || !EditorApplication.isPlaying ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;
            if (!pausing)
            {
                driver.SetPaused(true);
                pausing = true;
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            SimulationWorld world = driver.World;
            var report = new Report
            {
                beforeObjects = world.ObjectCount,
                beforeLogic = world.LogicReferencePool.ActiveCount,
                beforeRender = LF2ObjectPool.TryGetInstance()?.ActiveObjectCountForAcceptance ?? -1,
                hitPlanMode = world.BattleHitExecutionPlanModeForDiagnostics.ToString(),
            };
            NTSD28NativeRandomState originalRandom = world.NativeRandom.CaptureState();
            uint originalSeed = world.NativeRandom.CaptureScalarState().TableSeed;
            try
            {
                Require(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NTSD_Battle", "Wrong scene.");
                Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null, "Worker failure.");
                Require(world.StructuralEventSinkForServices == null, "An existing structural observer is installed.");
                foreach (bool replacement in new[] { false, true })
                    RunWitness(driver, report, replacement);
                Require(string.IsNullOrEmpty(consoleError), consoleError);
                report.status = "PASS";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                world.SetStructuralEventSinkForDiagnostics(null, driver.CurrentTickIndex, "pickup-probe-end");
                world.NativeRandom.ResetFromSeed(originalSeed);
                world.NativeRandom.Restore(originalRandom);
                report.afterObjects = world.ObjectCount;
                report.afterLogic = world.LogicReferencePool.ActiveCount;
                report.afterRender = LF2ObjectPool.TryGetInstance()?.ActiveObjectCountForAcceptance ?? -1;
                report.cleanup = report.beforeObjects == report.afterObjects &&
                    report.beforeLogic == report.afterLogic && report.beforeRender == report.afterRender;
                if (!report.cleanup) report.status = "FAIL";
                File.WriteAllText(Result, JsonUtility.ToJson(report, true));
                File.Delete(Request);
                EditorApplication.update -= Poll;
                Application.logMessageReceived -= Log;
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private static void RunWitness(SimulationTickDriver driver, Report report, bool replacement)
        {
            SimulationWorld world = driver.World;
            int samplingAlignmentTick = -1;
            if (!world.OneTuInput && world.InputPhase == 0)
            {
                samplingAlignmentTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(new FrameInputSet(samplingAlignmentTick,
                    Array.Empty<SimulationPlayerInput>()), ignorePaused: true, buildPresentation: false),
                    "Driver rejected non-sampling alignment tick.");
            }
            var row = new Witness
            {
                name = replacement ? "type2_state2004_replacement" : "oid120_type1",
                seed = 424242,
                holderOid = replacement ? 25 : 16,
                targetOid = replacement ? 150 : 120,
                targetType = replacement ? 2 : 1,
                targetGroundAction = replacement ? 20 : 64,
                samplingAlignmentTick = samplingAlignmentTick,
                inputPhaseBefore = world.InputPhase,
            };
            report.witnesses.Add(row);
            var owned = new List<RuntimeEntityHandle>();
            BattleSlotRuntimeState originalRosterSlot = null;
            int playerSlot = -1;
            BruteForceSceneQuery query = world.SceneQuery as BruteForceSceneQuery;
            Action originalCollectionObserver = query?.BeforeCollisionCandidateStoreFinalCompareForSelfCheck;
            try
            {
                LF2CharacterDataWrapper holderData = world.RuntimeCharacterConfigs.Resolve(row.holderOid);
                LF2CharacterDataWrapper targetData = world.RuntimeCharacterConfigs.Resolve(row.targetOid);
                Require(holderData?.characterData != null && targetData?.characterData != null, "Current definitions unavailable.");
                int pickupAction = replacement ? 250 : 60;
                LF2FrameData pickupFrame = holderData.characterData.frames.First(f => f.frameId == pickupAction);
                LF2FrameData groundFrame = targetData.characterData.frames.First(f => f.frameId == row.targetGroundAction);
                Require(pickupFrame.itrs.Any(i => i.kind == 2), "Selected holder frame no longer belongs to current kind2 corpus.");
                Require(groundFrame.state == (replacement ? 2004 : 1004), "Ground frame/state drift.");
                row.holderPickupAction = pickupAction;
                row.holderPickupState = pickupFrame.state;
                row.targetGroundState = groundFrame.state;
                row.targetWpointAction = groundFrame.PrimaryWeaponPoint.WeaponAct;

                row.holderSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                Require(row.holderSlot >= 50, "No scoped holder slot.");
                var holder = new LF2Character { ObjectId = row.holderOid, Name = "Goal16_PickupHolder" };
                holder.ModuleInitialize();
                holder.FrameCache.Load(holderData);
                holder.SetRequiredRuntimeSlot(row.holderSlot);
                world.Register(holder);
                Track(world, owned, holder);
                holder.ImmediateFrame(pickupAction);
                holder.Initialize(500, 500);
                holder.ClearBattleEntryInputState();
                NTSD28NativeComboStateMachine.InitializeNativeHistory(holder.Runtime);
                holder.AiControlled = false;
                holder.Team = 3;
                holder.RelationTeam = 3;
                holder.Runtime.OwnerSlotIndex = 17;
                holder.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                holder.Runtime.SetVelocity(0, 0, 0);
                holder.Runtime.SyncIntegerPosition();
                holder.SwitchDir("right");
                holder.FrameDelay = 1000;

                ObservedWeapon oldChild = null;
                if (replacement)
                {
                    row.oldChildSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(row.holderSlot + 1, 1000);
                    oldChild = AddWeapon(world, owned, 150, 2, row.oldChildSlot, 20, holder, row, true);
                    holder.Runtime.LinkState = 2;
                    holder.Runtime.TargetSlotIndex = row.oldChildSlot;
                    holder.Runtime.HeldWeaponStableId = row.oldChildSlot;
                    holder.HeldWeaponReferenceInternal = oldChild;
                    oldChild.Runtime.LinkState = -2;
                    oldChild.Runtime.HolderStableId = row.holderSlot;
                    oldChild.Runtime.OwnerSlotIndex = 19;
                    oldChild.RelationTeam = 3;
                    oldChild.Runtime.SetPosition(1000, 0, holder.Runtime.Z);
                    oldChild.Runtime.SyncIntegerPosition();
                }
                row.targetSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    replacement ? row.oldChildSlot + 1 : row.holderSlot + 1, 1000);
                ObservedWeapon target = AddWeapon(world, owned, row.targetOid, row.targetType,
                    row.targetSlot, row.targetGroundAction, holder, row, false);
                target.PriorChild = oldChild;
                playerSlot = Array.FindIndex(world.Runtime.Roster.Slots, replacement ? 3 : 2,
                    slot => slot == null || !slot.Active);
                Require(playerSlot >= 0, "No inactive roster slot for input fixture.");
                originalRosterSlot = world.Runtime.Roster.Slots[playerSlot];
                world.Runtime.Roster.Slots[playerSlot] = new BattleSlotRuntimeState
                {
                    Active = true, IsHuman = true, CharacterId = row.holderOid, Team = 3,
                    RuntimeSlotIndex = row.holderSlot, StableId = holder.Runtime.StableId,
                };
                row.playerSlot = playerSlot;
                holder.Runtime.PickupCount = 5;
                holder.AttackingCounter = 9;
                row.before = Capture(holder, target);
                Require(query != null && originalCollectionObserver == null, "Collector diagnostic hook unavailable.");
                query.BeforeCollisionCandidateStoreFinalCompareForSelfCheck = () =>
                {
                    row.collectionVisited = true;
                    row.collectedCount = holder.Runtime.HitCandidateCount;
                    row.collectionAttackCurrent = holder.Runtime.KeyJump;
                    row.collectionAttackPrevious = holder.Runtime.PrevJump;
                    row.inputPhaseAtCollection = world.InputPhase;
                    row.atCollection = Capture(holder, target);
                };

                world.NativeRandom.ResetFromSeed(row.seed);
                row.tick = driver.CurrentTickIndex + 1;
                row.armed = true;
                world.SetStructuralEventSinkForDiagnostics(new Trace(row), row.tick, "pickup-probe");
                var input = new FrameInputSet(row.tick, new[]
                {
                    new SimulationPlayerInput(playerSlot, SimulationInputButtons.Jump),
                });
                Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: false), "Driver rejected input tick.");
                row.armed = false;
                row.after = Capture(holder, target);
                row.live = world.FindEntityByRuntimeSlotForQuery(row.targetSlot) == target;
                row.heldReferenceMatches = ReferenceEquals(holder.HeldWeaponReferenceInternal, target);
                row.oldAfterTick = oldChild == null ? null : CaptureChild(oldChild);
                row.oldLive = oldChild == null || world.FindEntityByRuntimeSlotForQuery(row.oldChildSlot) == oldChild;
                Require(row.held.Count > 0, "Collector/input did not produce a held observation.");
                PairState settled = row.held[0];
                Require(row.observedCandidate && row.attackCurrent == 1 && row.attackPrevious == 0,
                    "Missing actual kind2 candidate or native Attack rising edge.");
                Require(settled.holderRelation == (replacement ? 2 : 101) &&
                    settled.targetRelation == (replacement ? -2 : -1), "Wrong settled relation.");
                Require(settled.count == (replacement ? 5 : 6), "Conditional +35C mismatch.");
                Require(settled.owner == row.holderSlot && settled.holderChild == row.targetSlot &&
                    settled.parent == row.holderSlot && settled.group == 3, "Physical owner/link/group mismatch.");
                Require(settled.counter == 0 && settled.action == (replacement ? 116 : 115), "Pickup tail/action mismatch.");
                Require(settled.holderCopy == 99, "Pickup wrote the legacy holder mirror.");
                Require(row.live && row.heldReferenceMatches && row.freeEvents == 0, "Live relation or no-free invariant failed.");
                if (replacement)
                {
                    Require(row.oldAfterC09 != null && row.oldLive && row.invalidPreserveEvents > 0, "Old child preserve path not observed.");
                    Require(row.oldAfterC09.Equals(row.oldAfterTick), "Replacement changed old child after first held pass.");
                }
                row.status = "PASS";
            }
            finally
            {
                row.armed = false;
                if (query != null)
                    query.BeforeCollisionCandidateStoreFinalCompareForSelfCheck = originalCollectionObserver;
                world.SetStructuralEventSinkForDiagnostics(null, driver.CurrentTickIndex, "pickup-probe-cleanup");
                if (playerSlot >= 0)
                    world.Runtime.Roster.Slots[playerSlot] = originalRosterSlot;
                foreach (RuntimeEntityHandle handle in owned)
                    if (world.TryResolveRuntimeHandleForDiagnostics(handle, out LF2Entity entity))
                        world.Unregister(entity);
            }
        }

        private static ObservedWeapon AddWeapon(SimulationWorld world, List<RuntimeEntityHandle> owned,
            int oid, int type, int slot, int action, LF2Character holder, Witness row, bool old)
        {
            var child = new ObservedWeapon { ObjectId = oid, Name = "Goal16_" + oid, Holder = holder, Row = row, Old = old };
            child.SetWeaponType(type);
            child.FrameCache.Load(world.RuntimeCharacterConfigs.Resolve(oid));
            child.SetRequiredRuntimeSlot(slot);
            world.Register(child);
            Track(world, owned, child);
            child.PrepareHealth();
            child.DirectWriteHeldFramePreserveWaitCounter(action);
            child.Health.HP = 100;
            child.Health.HPBound = 100;
            child.Team = 8;
            child.RelationTeam = 8;
            child.Runtime.OwnerSlotIndex = 27;
            child.Runtime.HolderCopySlotIndex = 99;
            child.Runtime.SetPosition(holder.Runtime.X, 0, holder.Runtime.Z);
            child.Runtime.SetVelocity(0, 0, 0);
            child.Runtime.SyncIntegerPosition();
            child.FrameDelay = 1000;
            return child;
        }

        private static void Track(SimulationWorld world, List<RuntimeEntityHandle> owned, LF2Entity entity)
        {
            Require(world.TryGetCurrentRuntimeHandleForDiagnostics(entity.Runtime.SlotIndex, entity, out RuntimeEntityHandle handle), "No current handle.");
            owned.Add(handle);
        }

        private static void Require(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException(message);
        }

        private static PairState Capture(LF2Character holder, LF2Entity target)
        {
            return new PairState
            {
                holderRelation = holder.Runtime.LinkState, targetRelation = target.Runtime.LinkState,
                count = holder.Runtime.PickupCount, owner = target.Runtime.OwnerSlotIndex,
                holderChild = holder.Runtime.TargetSlotIndex, parent = target.Runtime.HolderStableId,
                group = target.RelationTeam, action = holder.Frame.N, counter = holder.AttackingCounter,
                targetAction = target.Frame.N, weaponHp = target.Runtime.WeaponFlightCounter,
                holderCopy = target.HolderCopySlot,
                holderX = holder.Runtime.X, holderY = holder.Runtime.Y, holderZ = holder.Runtime.Z,
                targetX = target.Runtime.X, targetY = target.Runtime.Y, targetZ = target.Runtime.Z,
                holderOwner = holder.Runtime.OwnerSlotIndex,
            };
        }

        private static string CaptureChild(LF2Entity child)
        {
            return FormattableString.Invariant(
                $"{child.Frame.N}/{child.Runtime.LinkState}/{child.Runtime.HolderStableId}/{child.Runtime.OwnerSlotIndex}/{child.RelationTeam}/{child.Runtime.WeaponFlightCounter}/{child.Runtime.X}/{child.Runtime.Y}/{child.Runtime.Z}/{child.Runtime.Vx}/{child.Runtime.Vy}/{child.Runtime.Vz}");
        }

        private sealed class ObservedWeapon : LF2Weapon
        {
            internal LF2Character Holder;
            internal Witness Row;
            internal bool Old;
            internal LF2Entity PriorChild;
            private bool observingCandidate;
            internal void PrepareHealth() { InitializeHealth(); }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                int currentType = base.GetCurrentDataObjectTypeForSimulation();
                if (Row?.armed == true && !Old && !Row.observedCandidate && !observingCandidate)
                {
                    observingCandidate = true;
                    try
                    {
                        if (Match?.SceneQuery != null &&
                            Match.SceneQuery.TryGetCollisionCandidateRange(Holder, out CollisionCandidateRange range))
                        {
                            for (int i = 0; i < range.Count; i++)
                            {
                                if (!range.TryGet(i, out SceneQueryHit hit) || hit.ResolveCurrentTarget(Match) != this)
                                    continue;
                                Row.candidateCount = range.Count;
                                Row.observedCandidate = true;
                                Row.attackCurrent = Holder.Runtime.KeyJump;
                                Row.attackPrevious = Holder.Runtime.PrevJump;
                                Row.beforeConsumption = Capture(Holder, this);
                                if (PriorChild != null)
                                    Row.oldAfterC09 = CaptureChild(PriorChild);
                            }
                        }
                    }
                    finally { observingCandidate = false; }
                }
                return currentType;
            }

            public override WeaponActResult Act(LF2Entity holder, BattleWeaponPointValue point, Vector3 position)
            {
                if (Row.armed && !Old)
                {
                    Row.held.Add(Capture(Holder, this));
                    Row.attackCurrent = Holder.Runtime.KeyJump;
                    Row.attackPrevious = Holder.Runtime.PrevJump;
                    if (Match.SceneQuery.TryGetCollisionCandidateRange(Holder, out CollisionCandidateRange range))
                    {
                        Row.candidateCount = range.Count;
                        for (int i = 0; i < range.Count; i++)
                            if (range.TryGet(i, out SceneQueryHit hit) && hit.ResolveCurrentTarget(Match) == this)
                                Row.observedCandidate = true;
                    }
                }
                WeaponActResult result = base.Act(holder, point, position);
                if (Row.armed && Old)
                    Row.oldAfterC09 = CaptureChild(this);
                return result;
            }
        }

        private sealed class Trace : IBattleParityStructuralEventSink
        {
            private readonly Witness row;
            internal Trace(Witness row) { this.row = row; }
            public void Record(BattleParityStructuralEvent item)
            {
                if (item.Action == "free" && (item.Slot == row.targetSlot || item.Slot == row.oldChildSlot))
                    row.freeEvents++;
                if (item.Action == "link-validation" && item.Slot == row.oldChildSlot && item.Outcome == "preserved")
                    row.invalidPreserveEvents++;
            }
        }

        [Serializable] private sealed class Report
        {
            public string status, message, hitPlanMode;
            public string boundary = "Current Gaara16/60 to OID120/64; current Kakuzu25/250 to OID150/20 replacing an already-held type2. Scoped frame-delay1000 admission fixtures, cleared native input history, and distinct temporary human roster slots; advance an empty non-sampling tick when needed, then native Attack from FrameInputSet.Jump on the existing InputPhase0 through actual driver/collector/writer/held. Collection callback observes existing data only. No physical keyboard claim.";
            public int beforeObjects, afterObjects, beforeLogic, afterLogic, beforeRender, afterRender;
            public bool cleanup;
            public List<Witness> witnesses = new List<Witness>();
        }

        [Serializable] private sealed class Witness
        {
            public string name, status, oldAfterC09, oldAfterTick;
            public uint seed;
            public int tick, playerSlot, holderOid, targetOid, targetType, holderSlot, targetSlot, oldChildSlot = -1;
            public int holderPickupAction, holderPickupState, targetGroundAction, targetGroundState, targetWpointAction;
            public int candidateCount, attackCurrent, attackPrevious, freeEvents, invalidPreserveEvents;
            public int collectedCount, collectionAttackCurrent, collectionAttackPrevious;
            public int samplingAlignmentTick, inputPhaseBefore, inputPhaseAtCollection;
            public bool armed, observedCandidate, live, oldLive, heldReferenceMatches;
            public bool collectionVisited;
            public PairState before, atCollection, beforeConsumption, after;
            public List<PairState> held = new List<PairState>();
        }

        [Serializable] private sealed class PairState
        {
            public int holderRelation, targetRelation, count, owner, holderChild, parent, group, action, counter;
            public int targetAction, weaponHp, holderCopy;
            public int holderOwner;
            public double holderX, holderY, holderZ, targetX, targetY, targetZ;
        }
    }
}
#endif
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
namespace NTSD.Test.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.TestTools.TestRunner.Api;
    using UnityEngine;

    internal sealed class NTSD28B6PickupTestResultCapture : ICallbacks
    {
        private static TestRunnerApi api;
        private const string Request = "Temp/Goal16_TestCapture.request";
        internal const string Result = "Temp/Goal16_TestCapture.result.xml";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.hideFlags = HideFlags.HideAndDontSave;
            api.RegisterCallbacks(new NTSD28B6PickupTestResultCapture());
        }

        public void RunStarted(ITestAdaptor testsToRun) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            if (!File.Exists(Request)) return;
            File.WriteAllText(Result, result.ToXml().OuterXml);
            File.Delete(Request);
        }
    }
}
#endif
