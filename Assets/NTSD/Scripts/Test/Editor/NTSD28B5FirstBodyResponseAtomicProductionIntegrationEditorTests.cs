#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Extensions;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28B5FirstBodyResponseAtomicProductionIntegrationEditorTests
    {
        [Test]
        public void SharedRunner_IsTheSingleFourShellIntegrationPoint()
        {
            string runner = ReadSource(
                "NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs");

            Assert.That(Count(runner, "BattleFirstBodyResponseWriter.TryApply("),
                Is.EqualTo(1));
            StringAssert.Contains(
                "PrepareBattleHitExecutionPlanLegacyFirstBodyResponseAttemptObservation",
                runner);
            StringAssert.Contains(
                "ObserveBattleHitExecutionPlanLegacyFirstBodyResponseAttempt",
                runner);
            foreach (string path in new[]
                     {
                         "NTSD/Scripts/Animation/LF2Objects/LF2CharacterInteractionResolver.cs",
                         "NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatInteractionResolver.cs",
                         "NTSD/Scripts/Animation/LF2Objects/LF2WeaponInteractionResolver.cs",
                         "NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs",
                     })
            {
                StringAssert.Contains(
                    "BattleHitCandidateSequenceRunner.TryConsume",
                    ReadSource(path),
                    path);
            }
        }

        [Test]
        public void ActualActionRange_WritesGroupAndHoldButPreservesFrameCounter()
        {
            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld world,
                out LF2Entity attacker,
                out TypedCharacter target,
                out InteractionArea interaction,
                firstBodyRespond: -1);
            attacker.RelationTeam = 47;
            attacker.FrameDelay = 9;
            target.FrameDelay = 8;
            target.Runtime.FrameWaitCounter = 123;
            target.AttackingCounter = 57;

            BattleFirstBodyResponseAttemptResult result =
                BattleFirstBodyResponseWriter.TryApply(
                    world,
                    attacker,
                    target,
                    interaction);

            Assert.That(result.Eligible, Is.True);
            Assert.That(result.Applied, Is.True);
            Assert.That(result.Response.Kind,
                Is.EqualTo(BattleFirstBodyResponseKind.ActionRange));
            Assert.That(target.Frame.N, Is.EqualTo(33));
            Assert.That(target.Runtime.Frame, Is.EqualTo(33));
            Assert.That(target.Runtime.FrameWaitCounter, Is.EqualTo(123));
            Assert.That(target.AttackingCounter, Is.EqualTo(57));
            Assert.That(target.RelationTeam, Is.EqualTo(47));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
        }

        [Test]
        public void ActualEncoded_WritesActionsResetsCountersAndUsesOnlyManualStats()
        {
            CreatePair(
                firstBodyKind: Encode(0, 123, 456, 6),
                out SimulationWorld world,
                out LF2Entity attacker,
                out TypedCharacter target,
                out InteractionArea interaction,
                firstBodyRespond: 91,
                injury: 37);
            attacker.Runtime.FrameWaitCounter = 88;
            target.Runtime.FrameWaitCounter = 77;
            attacker.AttackingCounter = 61;
            target.AttackingCounter = 62;
            attacker.Runtime.InputScoreTotal348 = 5;
            target.Runtime.InputHpConsumedTotal34C = 7;
            target.Health.HP = 20;
            target.Health.HPBound = 333;
            target.ComboCountVic = 11;
            int pendingSoundCount = world.PendingSounds.Count;

            BattleFirstBodyResponseAttemptResult result =
                BattleFirstBodyResponseWriter.TryApply(
                    world,
                    attacker,
                    target,
                    interaction);

            Assert.That(result.Applied, Is.True);
            Assert.That(result.RollConsumed, Is.False);
            Assert.That(target.Frame.N, Is.EqualTo(123));
            Assert.That(attacker.Frame.N, Is.EqualTo(456));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(attacker.AttackingCounter, Is.Zero);
            Assert.That(target.Runtime.FrameWaitCounter, Is.EqualTo(77));
            Assert.That(attacker.Runtime.FrameWaitCounter, Is.EqualTo(88));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(target.Health.HP, Is.Zero);
            Assert.That(target.Runtime.InputHpConsumedTotal34C, Is.EqualTo(44));
            Assert.That(attacker.Runtime.InputScoreTotal348, Is.EqualTo(42));
            Assert.That(target.Health.HPBound, Is.EqualTo(333));
            Assert.That(target.ComboCountVic, Is.EqualTo(11));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(pendingSoundCount));
        }

        [Test]
        public void ActualEncodedChanceFailure_CommitsOneSynchronizedDrawAndNoWrites()
        {
            CreatePair(
                firstBodyKind: Encode(1, 123, 456, 7),
                out SimulationWorld world,
                out LF2Entity attacker,
                out TypedCharacter target,
                out InteractionArea interaction);
            uint seed = FindSeed(world.NativeRandom, 1, expectedPass: false);
            world.NativeRandom.ResetFromSeed(seed);
            NTSD28NativeRandomScalarState before =
                world.NativeRandom.CaptureScalarState();
            int targetFrame = target.Frame.N;
            int attackerFrame = attacker.Frame.N;

            BattleFirstBodyResponseAttemptResult result =
                BattleFirstBodyResponseWriter.TryApply(
                    world,
                    attacker,
                    target,
                    interaction);
            NTSD28NativeRandomScalarState after =
                world.NativeRandom.CaptureScalarState();

            Assert.That(result.Eligible, Is.True);
            Assert.That(result.Applied, Is.False);
            Assert.That(result.RollConsumed, Is.True);
            Assert.That(result.Response.Kind,
                Is.EqualTo(BattleFirstBodyResponseKind.EncodedChanceRejected));
            Assert.That(after.SynchronizedCalls,
                Is.EqualTo(before.SynchronizedCalls + 1));
            Assert.That(after.LastSynchronizedCallSite, Is.EqualTo(1u));
            Assert.That(target.Frame.N, Is.EqualTo(targetFrame));
            Assert.That(attacker.Frame.N, Is.EqualTo(attackerFrame));
        }

        [Test]
        public void ActualEncodedChanceSuccess_CommitsOneSynchronizedDrawAndWrites()
        {
            CreatePair(
                firstBodyKind: Encode(1, 123, 456, 7),
                out SimulationWorld world,
                out LF2Entity attacker,
                out TypedCharacter target,
                out InteractionArea interaction);
            uint seed = FindSeed(world.NativeRandom, 1, expectedPass: true);
            attacker.AttackingCounter = 61;
            target.AttackingCounter = 62;
            attacker.Runtime.FrameWaitCounter = 88;
            target.Runtime.FrameWaitCounter = 77;
            world.NativeRandom.ResetFromSeed(seed);
            NTSD28NativeRandomScalarState before =
                world.NativeRandom.CaptureScalarState();

            BattleFirstBodyResponseAttemptResult result =
                BattleFirstBodyResponseWriter.TryApply(
                    world,
                    attacker,
                    target,
                    interaction);
            NTSD28NativeRandomScalarState after =
                world.NativeRandom.CaptureScalarState();

            Assert.That(result.Applied, Is.True);
            Assert.That(result.RollConsumed, Is.True);
            Assert.That(result.Response.Kind,
                Is.EqualTo(BattleFirstBodyResponseKind.EncodedApplied));
            Assert.That(after.SynchronizedCalls,
                Is.EqualTo(before.SynchronizedCalls + 1));
            Assert.That(after.LastSynchronizedCallSite, Is.EqualTo(1u));
            Assert.That(target.Frame.N, Is.EqualTo(123));
            Assert.That(attacker.Frame.N, Is.EqualTo(456));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(attacker.AttackingCounter, Is.Zero);
            Assert.That(target.Runtime.FrameWaitCounter, Is.EqualTo(77));
            Assert.That(attacker.Runtime.FrameWaitCounter, Is.EqualTo(88));
        }

        [Test]
        public void CharacterReducedDefenseAndActiveType1Armor_AreExcluded()
        {
            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld defenseWorld,
                out LF2Entity defenseAttacker,
                out TypedCharacter defenseTarget,
                out InteractionArea defenseInteraction);
            defenseAttacker.SwitchDir("right");
            defenseTarget.SwitchDir("left");
            defenseTarget.ImmediateFrame(7);
            defenseTarget.Frame.D.primaryBodyKindForEffectSuppression = 1033;

            BattleFirstBodyResponseAttemptResult defense =
                BattleFirstBodyResponseWriter.TryApply(
                    defenseWorld,
                    defenseAttacker,
                    defenseTarget,
                    defenseInteraction);

            LF2ArmorData activeArmor = Armor(type: 1);
            activeArmor.hp = 100;
            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld armorWorld,
                out LF2Entity armorAttacker,
                out TypedCharacter armorTarget,
                out InteractionArea armorInteraction,
                armor: activeArmor);
            armorTarget.Runtime.RuntimeArmorHp118 = 100;
            BattleFirstBodyResponseAttemptResult armor =
                BattleFirstBodyResponseWriter.TryApply(
                    armorWorld,
                    armorAttacker,
                    armorTarget,
                    armorInteraction);

            Assert.That(defense.Eligible, Is.False);
            Assert.That(defense.Applied, Is.False);
            Assert.That(armor.Eligible, Is.False);
            Assert.That(armor.Applied, Is.False);
        }

        [Test]
        public void Type1BypassResourceAndBrokenFallback_AllEnterResponse()
        {
            LF2ArmorData bypassArmor = Armor(type: 1);
            bypassArmor.effects.Add(20);
            AssertFallbackApplies(bypassArmor, new InteractionArea
            {
                kind = 0,
                injury = 20,
                effect = 20,
                arest = 10,
                vrest = 0,
            });

            LF2ArmorData resourceArmor = Armor(type: 1);
            resourceArmor.mp = -7;
            AssertFallbackApplies(resourceArmor, Hit(20), pp: 6);

            LF2ArmorData brokenArmor = Armor(type: 1);
            brokenArmor.hp = 100;
            AssertFallbackApplies(
                brokenArmor,
                Hit(20),
                runtimeArmorHp: 20,
                expectBrokenArmor: true);
        }

        [Test]
        public void NonCharacterSelectedType0ArmorExcludesButNoArmorEntersResponse()
        {
            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld selectedWorld,
                out LF2Entity selectedAttacker,
                out TypedCharacter selectedTarget,
                out InteractionArea selectedInteraction,
                targetType: LF2ObjectType.SpecialAttack,
                armor: Armor(type: 0));
            BattleFirstBodyResponseAttemptResult selected =
                BattleFirstBodyResponseWriter.TryApply(
                    selectedWorld,
                    selectedAttacker,
                    selectedTarget,
                    selectedInteraction);

            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld plainWorld,
                out LF2Entity plainAttacker,
                out TypedCharacter plainTarget,
                out InteractionArea plainInteraction,
                targetType: LF2ObjectType.SpecialAttack);
            BattleFirstBodyResponseAttemptResult plain =
                BattleFirstBodyResponseWriter.TryApply(
                    plainWorld,
                    plainAttacker,
                    plainTarget,
                    plainInteraction);

            Assert.That(selected.Eligible, Is.False);
            Assert.That(selectedTarget.Frame.N, Is.Zero);
            Assert.That(plain.Eligible, Is.True);
            Assert.That(plain.Applied, Is.True);
            Assert.That(plainTarget.Frame.N, Is.EqualTo(33));
        }

        [Test]
        public void SharedRunner_SuccessSkipsConsumeDispatchAndRemainingCandidate()
        {
            var world = new SimulationWorld();
            InteractionArea interaction = Hit(10);
            TypedCharacter attacker = CreateEntity(
                world, 0, 9200, LF2ObjectType.Character, 1, interaction, false);
            TypedCharacter first = CreateEntity(
                world, 1, 9201, LF2ObjectType.Character, 2, null, true, 1033);
            TypedCharacter skipped = CreateEntity(
                world, 2, 9202, LF2ObjectType.Character, 3, null, true);
            var consumer = new RecordingConsumer(attacker);
            CollisionCandidateRange candidates = Collect(world, attacker);

            bool consumed = BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                consumer,
                in candidates);

            Assert.That(consumed, Is.True);
            Assert.That(candidates.Count, Is.EqualTo(2));
            Assert.That(first.Frame.N, Is.EqualTo(33));
            Assert.That(skipped.Frame.N, Is.Zero);
            Assert.That(consumer.ConsumeEffectCount, Is.Zero);
            Assert.That(consumer.DispatchCount, Is.Zero);
        }

        [Test]
        public void SharedRunner_ChanceFailureContinuesConsumeAndOrdinaryDispatch()
        {
            var world = new SimulationWorld();
            uint seed = FindSeed(world.NativeRandom, 1, expectedPass: false);
            world.NativeRandom.ResetFromSeed(seed);
            InteractionArea interaction = Hit(10);
            TypedCharacter attacker = CreateEntity(
                world, 0, 9210, LF2ObjectType.Character, 1, interaction, false);
            TypedCharacter target = CreateEntity(
                world, 1, 9211, LF2ObjectType.Character, 2, null, true,
                Encode(1, 123, 456, 7));
            var consumer = new RecordingConsumer(attacker);
            CollisionCandidateRange candidates = Collect(world, attacker);

            BattleHitCandidateSequenceRunner.TryConsumeCaptured(consumer, in candidates);

            Assert.That(target.Frame.N, Is.Zero);
            Assert.That(consumer.ConsumeEffectCount, Is.EqualTo(1));
            Assert.That(consumer.DispatchCount, Is.EqualTo(1));
            Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(1));
        }

        [Test]
        public void SharedRunner_UnrecognizedFirstBodyContinuesOrdinaryDispatch()
        {
            var world = new SimulationWorld();
            InteractionArea interaction = Hit(10);
            TypedCharacter attacker = CreateEntity(
                world, 0, 9215, LF2ObjectType.Character, 1, interaction, false);
            TypedCharacter target = CreateEntity(
                world, 1, 9216, LF2ObjectType.Character, 2, null, true);
            var consumer = new RecordingConsumer(attacker);
            CollisionCandidateRange candidates = Collect(world, attacker);

            bool consumed = BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                consumer,
                in candidates);

            Assert.That(consumed, Is.True);
            Assert.That(target.Frame.N, Is.Zero);
            Assert.That(consumer.ConsumeEffectCount, Is.EqualTo(1));
            Assert.That(consumer.DispatchCount, Is.EqualTo(1));
        }

        [Test]
        public void FrozenCriminalOid300_FirstBodyResponsePrecedesRedirectTail()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world,
                0,
                9240,
                LF2ObjectType.Character,
                7,
                Hit(10),
                false);
            TypedCharacter criminal = CreateFrozenCriminalEntity(world, 1, 30);
            var consumer = new RecordingConsumer(attacker);
            CollisionCandidateRange candidates = Collect(world, attacker);

            bool consumed = BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                consumer,
                in candidates);

            Assert.That(consumed, Is.True);
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(criminal.Frame.N, Is.EqualTo(33));
            Assert.That(criminal.RelationTeam, Is.EqualTo(1));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(criminal.FrameDelay, Is.EqualTo(-3));
            Assert.That(consumer.ConsumeEffectCount, Is.Zero);
            Assert.That(consumer.DispatchCount, Is.Zero);
        }

        [Test]
        public void FrozenCriminalOid300_ShadowCompareProjectsResponseAndAbort()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world,
                0,
                9241,
                LF2ObjectType.Character,
                7,
                Hit(10),
                false);
            TypedCharacter criminal = CreateFrozenCriminalEntity(world, 1, 30);
            var consumer = new RecordingConsumer(attacker);
            _ = Collect(world, attacker);
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            world.CaptureBattleHitExecutionPlanPass(
                704,
                BattleHitExecutionPass.Character);
            Assert.That(world.BeginBattleHitExecutionPlanLegacyObservation(
                704,
                BattleHitExecutionPass.Character), Is.True);
            Assert.That(world.SceneQuery.TryGetCollisionCandidateRange(
                attacker,
                out CollisionCandidateRange candidates), Is.True);

            BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                consumer,
                in candidates);
            world.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(criminal.Frame.N, Is.EqualTo(33));
            Assert.That(diagnostics.CurrentTickPlanValid, Is.True);
            Assert.That(diagnostics.ObservedFirstBodyResponseAttemptCount,
                Is.EqualTo(1));
            Assert.That(diagnostics.ObservedAbortTerminationCount, Is.EqualTo(1));
            Assert.That(diagnostics.SkippedCandidateCountAfterAbort, Is.Zero);
            Assert.That(diagnostics.LastFirstBodyResponseDifferenceMask, Is.Zero);
            Assert.That(diagnostics.ObservationMismatchCount, Is.Zero);
        }

        [Test]
        public void ShadowCompare_FirstBodySuccessSkipsOnlySameAttackerAndStaysValid()
        {
            var world = new SimulationWorld();
            InteractionArea interaction = Hit(10);
            TypedCharacter attacker = CreateEntity(
                world, 0, 9220, LF2ObjectType.Character, 1, interaction, false);
            _ = CreateEntity(
                world, 1, 9221, LF2ObjectType.Character, 2, null, true, 1033);
            _ = CreateEntity(
                world, 2, 9222, LF2ObjectType.Character, 3, null, true);
            var consumer = new RecordingConsumer(attacker);
            _ = Collect(world, attacker);
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);
            world.CaptureBattleHitExecutionPlanPass(
                701,
                BattleHitExecutionPass.Character);
            Assert.That(world.BeginBattleHitExecutionPlanLegacyObservation(
                701,
                BattleHitExecutionPass.Character), Is.True);
            Assert.That(world.SceneQuery.TryGetCollisionCandidateRange(
                attacker,
                out CollisionCandidateRange candidates), Is.True);

            BattleHitCandidateSequenceRunner.TryConsumeCaptured(consumer, in candidates);
            world.EndBattleHitExecutionPlanLegacyObservation();

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(
                diagnostics.CurrentTickPlanValid,
                Is.True,
                $"failure={diagnostics.FirstFailureReason}, " +
                $"mismatches={diagnostics.ObservationMismatchCount}, " +
                $"attempts={diagnostics.ObservedFirstBodyResponseAttemptCount}, " +
                $"aborts={diagnostics.ObservedAbortTerminationCount}, " +
                $"skipped={diagnostics.SkippedCandidateCountAfterAbort}");
            Assert.That(diagnostics.ObservedFirstBodyResponseAttemptCount,
                Is.EqualTo(1));
            Assert.That(diagnostics.ObservedAbortTerminationCount, Is.EqualTo(1));
            Assert.That(diagnostics.SkippedCandidateCountAfterAbort, Is.EqualTo(1));
            Assert.That(diagnostics.LastFirstBodyResponseDifferenceMask, Is.Zero);
            Assert.That(diagnostics.ObservationMismatchCount, Is.Zero);
        }

        [Test]
        public void SharedRunner_ResponseAbortSkipsSameAttackerButContinuesNextAttacker()
        {
            var world = new SimulationWorld();
            InteractionArea interaction = Hit(10);
            TypedCharacter firstAttacker = CreateEntity(
                world, 0, 9230, LF2ObjectType.Character, 1, interaction, false);
            TypedCharacter firstTarget = CreateEntity(
                world, 1, 9231, LF2ObjectType.Character, 2, null, true, 1033);
            TypedCharacter skippedTarget = CreateEntity(
                world, 2, 9232, LF2ObjectType.Character, 3, null, true);
            TypedCharacter secondAttacker = CreateEntity(
                world, 3, 9233, LF2ObjectType.Character, 4, Hit(10), false);
            TypedCharacter secondTarget = CreateEntity(
                world, 4, 9234, LF2ObjectType.Character, 5, null, true, 1000);
            SetPosition(secondAttacker, 1000);
            SetPosition(secondTarget, 1000);
            CollisionCandidateRange firstCandidates = Collect(world, firstAttacker);
            if (!world.SceneQuery.TryGetCollisionCandidateRange(
                    secondAttacker,
                    out CollisionCandidateRange secondCandidates))
            {
                Assert.Fail("second attacker must own a captured candidate range");
            }
            var firstConsumer = new RecordingConsumer(firstAttacker);
            var secondConsumer = new RecordingConsumer(secondAttacker);

            bool firstConsumed = BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                firstConsumer,
                in firstCandidates);
            bool secondConsumed = BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                secondConsumer,
                in secondCandidates);

            Assert.That(firstConsumed, Is.True);
            Assert.That(secondConsumed, Is.True);
            Assert.That(firstCandidates.Count, Is.EqualTo(2));
            Assert.That(secondCandidates.Count, Is.EqualTo(1));
            Assert.That(firstTarget.Frame.N, Is.EqualTo(33));
            Assert.That(skippedTarget.Frame.N, Is.Zero);
            Assert.That(secondTarget.Frame.N, Is.Zero);
            Assert.That(secondAttacker.FrameDelay, Is.EqualTo(3));
            Assert.That(secondTarget.FrameDelay, Is.EqualTo(-3));
            Assert.That(secondTarget.RelationTeam, Is.EqualTo(1));
            Assert.That(firstConsumer.ConsumeEffectCount, Is.Zero);
            Assert.That(firstConsumer.DispatchCount, Is.Zero);
            Assert.That(secondConsumer.ConsumeEffectCount, Is.Zero);
            Assert.That(secondConsumer.DispatchCount, Is.Zero);
        }

        [Test]
        public void DataOrientedScheduling_MatchesLegacyFirstBodyResponseWrites()
        {
            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld legacyWorld,
                out LF2Entity legacyAttacker,
                out TypedCharacter legacyTarget,
                out _);
            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld dataWorld,
                out LF2Entity dataAttacker,
                out TypedCharacter dataTarget,
                out _);
            legacyAttacker.RelationTeam = 81;
            dataAttacker.RelationTeam = 81;
            _ = Collect(legacyWorld, legacyAttacker);
            _ = Collect(dataWorld, dataAttacker);
            dataWorld.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.DataOriented);

            legacyWorld.PostInteractionTickAll(703);
            dataWorld.PostInteractionTickAll(703);

            Assert.That(dataTarget.Frame.N, Is.EqualTo(legacyTarget.Frame.N));
            Assert.That(dataTarget.Runtime.Frame,
                Is.EqualTo(legacyTarget.Runtime.Frame));
            Assert.That(dataTarget.RelationTeam,
                Is.EqualTo(legacyTarget.RelationTeam));
            Assert.That(dataAttacker.FrameDelay,
                Is.EqualTo(legacyAttacker.FrameDelay));
            Assert.That(dataTarget.FrameDelay,
                Is.EqualTo(legacyTarget.FrameDelay));
            BattleHitExecutionPlanDiagnostics diagnostics =
                dataWorld.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.Mode,
                Is.EqualTo(BattleHitExecutionPlanMode.DataOriented));
            Assert.That(diagnostics.CurrentTickPlanValid, Is.True);
            Assert.That(diagnostics.FailureCount, Is.Zero);
        }

        [Test]
        public void WarmActualActionResponse_AllocatesNoManagedMemory()
        {
            CreatePair(
                firstBodyKind: 1000,
                out SimulationWorld world,
                out LF2Entity attacker,
                out TypedCharacter target,
                out InteractionArea interaction);
            _ = BattleFirstBodyResponseWriter.TryApply(
                world, attacker, target, interaction);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int appliedCount = 0;
            for (int index = 0; index < 4096; index++)
            {
                BattleFirstBodyResponseAttemptResult result =
                    BattleFirstBodyResponseWriter.TryApply(
                        world,
                        attacker,
                        target,
                        interaction);
                if (result.Applied)
                    appliedCount++;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(appliedCount, Is.EqualTo(4096));
            Assert.That(allocated, Is.Zero);
        }

        private static void AssertFallbackApplies(
            LF2ArmorData armor,
            InteractionArea interaction,
            int pp = 500,
            int runtimeArmorHp = 100,
            bool expectBrokenArmor = false)
        {
            CreatePair(
                firstBodyKind: 1033,
                out SimulationWorld world,
                out LF2Entity attacker,
                out TypedCharacter target,
                out InteractionArea resolvedInteraction,
                armor: armor,
                interactionOverride: interaction);
            target.Health.PP = pp;
            target.Runtime.RuntimeArmorHp118 = runtimeArmorHp;

            BattleFirstBodyResponseAttemptResult result =
                BattleFirstBodyResponseWriter.TryApply(
                    world,
                    attacker,
                    target,
                    resolvedInteraction);

            Assert.That(result.Eligible, Is.True);
            Assert.That(result.Applied, Is.True);
            Assert.That(result.BrokenArmorBeforeResponse,
                Is.EqualTo(expectBrokenArmor));
            Assert.That(target.Runtime.RuntimeArmorHp118,
                Is.EqualTo(expectBrokenArmor ? -1 : runtimeArmorHp));
            Assert.That(target.Frame.N, Is.EqualTo(33));
        }

        private static void SetPosition(LF2Entity entity, int x)
        {
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private static void CreatePair(
            int firstBodyKind,
            out SimulationWorld world,
            out LF2Entity attacker,
            out TypedCharacter target,
            out InteractionArea interaction,
            int firstBodyRespond = 0,
            int injury = 20,
            LF2ObjectType targetType = LF2ObjectType.Character,
            LF2ArmorData armor = null,
            InteractionArea interactionOverride = null)
        {
            world = new SimulationWorld();
            interaction = interactionOverride ?? Hit(injury);
            attacker = CreateEntity(
                world, 0, 9100, LF2ObjectType.Character, 1,
                interaction, false);
            target = CreateEntity(
                world, 1, 9101, targetType, 2,
                null, true, firstBodyKind, firstBodyRespond, armor);
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int slot,
            int objectId,
            LF2ObjectType type,
            int group,
            InteractionArea interaction,
            bool hasBody,
            int firstBodyKind = 0,
            int firstBodyRespond = 0,
            LF2ArmorData armor = null)
        {
            var current = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
                primaryBodyKindForEffectSuppression = firstBodyKind,
                primaryBodyRespondForHitResponse = firstBodyRespond,
            };
            if (interaction != null)
                current.itrs.Add(interaction);
            if (hasBody)
                current.bodies.Add(new BattleBodyBoxValue(0, -10, 20, 20));
            var frames = new List<LF2FrameData>
            {
                current,
                Frame(7, LF2States.Defending),
                Frame(33),
                Frame(123),
                Frame(456),
            };
            var data = new LF2CharacterData
            {
                name = "B5FirstBodyAtomic",
                type_sub = objectId,
                frames = frames,
                armors = armor == null
                    ? new List<LF2ArmorData>()
                    : new List<LF2ArmorData> { armor },
            };
            var entity = new TypedCharacter(type)
            {
                ObjectId = objectId,
            };
            entity.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = current;
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = current;
            entity.Initialize(500, 500);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.PP = 500;
            entity.FrameDelay = 0;
            entity.RelationTeam = group;
            entity.Team = group;
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            world.Register(entity);
            return entity;
        }

        private static TypedCharacter CreateFrozenCriminalEntity(
            SimulationWorld world,
            int slot,
            int frameId)
        {
            string path = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Config",
                "chars",
                "criminal.dat");
            Lf2DatFile dat = new Lf2DatParserV2().Parse(
                File.ReadAllText(path),
                path);
            var frames = new List<LF2FrameData>(dat.Frames.Count);
            for (int index = 0; index < dat.Frames.Count; index++)
                frames.Add(Lf2DatConverter.ConvertToFrameData(dat.Frames[index]));
            var data = new LF2CharacterData
            {
                name = "criminal",
                type_sub = 300,
                frames = frames,
            };
            var entity = new TypedCharacter(LF2ObjectType.Other)
            {
                ObjectId = 300,
            };
            entity.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(300, data));
            entity.ImmediateFrame(frameId);
            entity.Initialize(500, 500);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.PP = 500;
            entity.RelationTeam = 8;
            entity.Team = 8;
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            world.Register(entity);
            return entity;
        }

        private static LF2FrameData Frame(
            int id,
            int state = LF2States.Standing)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
            };
        }

        private static CollisionCandidateRange Collect(
            SimulationWorld world,
            LF2Entity attacker)
        {
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(world.SceneQuery.TryGetCollisionCandidateRange(
                attacker,
                out CollisionCandidateRange candidates), Is.True);
            return candidates;
        }

        private static InteractionArea Hit(int injury)
        {
            return new InteractionArea
            {
                kind = 0,
                injury = injury,
                effect = 0,
                fall = 0,
                dvx = 0,
                dvy = 0,
                arest = 10,
                vrest = 1,
                x = -100,
                y = -20,
                w = 240,
                h = 40,
                zwidth = 15,
            };
        }

        private static LF2ArmorData Armor(int type)
        {
            return new LF2ArmorData
            {
                type = type,
                ratio = 0,
                decrease = 50,
                mp = 0,
                fall = -1,
                bdefend = -1,
                injury = -1,
                hp = 0,
                delay = -1,
            };
        }

        private static uint FindSeed(
            NTSD28NativeRandom random,
            int chance,
            bool expectedPass)
        {
            for (uint seed = 1; seed < 10000; seed++)
            {
                random.ResetFromSeed(seed);
                NTSD28SynchronizedRandomCursor cursor =
                    random.CaptureSynchronizedCursor();
                bool passed = cursor.Next((uint)chance, 100) < chance;
                if (passed == expectedPass)
                    return seed;
            }

            Assert.Fail("No deterministic seed found for requested chance result.");
            return 0;
        }

        private static int Encode(
            int chance,
            int targetAction,
            int attackerAction,
            int effect)
        {
            return 1000000000 + chance * 10000000 +
                   targetAction * 10000 + attackerAction * 10 + effect;
        }

        private static string ReadSource(string relativePath)
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, relativePath));
        }

        private static int Count(string source, string value)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(value, offset,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }

            return count;
        }

        private sealed class RecordingConsumer : IBattleHitCandidateConsumer
        {
            internal RecordingConsumer(LF2Entity attacker)
            {
                Attacker = attacker;
            }

            public LF2Entity Attacker { get; }
            internal int ConsumeEffectCount { get; private set; }
            internal int DispatchCount { get; private set; }

            public void ApplyConsumeEffects(in SceneQueryHit hit)
            {
                ConsumeEffectCount++;
            }

            public void BeforeDispatch(int itrIndex)
            {
            }

            public bool Dispatch(
                INTSDItrKindService kindService,
                InteractionArea itr,
                LF2Entity target)
            {
                DispatchCount++;
                return true;
            }
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType type;

            internal TypedCharacter(LF2ObjectType type)
            {
                this.type = type;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)type;
            }
        }
    }
}
#endif
