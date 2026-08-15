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
    public sealed class CharacterInputLiveSlotLoopEditorTests
    {
        [Test]
        public void CharacterInputPass_DefaultsToDataOrientedForExactAiCharacters()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsCharacterInputPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterInputPassMode.DataOriented));
        }

        [Test]
        public void CharacterInputPass_HumanCharacterFailsClosedToLocalInputPath()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 480);
            character.AiControlled = false;

            world.CharacterInputAll(2);

            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.Zero);
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.EqualTo(1));
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .UnityCompatibilityShellCount,
                Is.EqualTo(1));
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .UnexpectedFallbackCount,
                Is.Zero);
        }

        [Test]
        public void CharacterInputPass_UnknownDerivedCharacterFailsClosedToVirtualPath()
        {
            var world = new SimulationWorld();
            world.ConfigureBattleEcsCharacterInputPassForDiagnostics(
                BattleEcsCharacterInputPassMode.DataOriented);
            DerivedCharacter character = InitializeCharacter(
                new DerivedCharacter(),
                0,
                481);
            world.Register(character);

            world.CharacterInputAll(2);

            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .ExactCharacterCount,
                Is.Zero);
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.EqualTo(1));
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .UnityCompatibilityShellCount,
                Is.EqualTo(1));
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .UnexpectedFallbackCount,
                Is.Zero);
        }

        [Test]
        public void CharacterInputPass_LegacyModeCountsUnexpectedFallback()
        {
            var world = new SimulationWorld();
            world.ConfigureBattleEcsCharacterInputPassForDiagnostics(
                BattleEcsCharacterInputPassMode.Legacy);
            LF2Character character = RegisterCharacter(world, 0, 482);
            character.AiControlled = true;

            world.CharacterInputAll(2);

            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .UnityCompatibilityShellCount,
                Is.Zero);
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .UnexpectedFallbackCount,
                Is.EqualTo(1));
        }

        [Test]
        public void WarmedCharacterInputPass_AllocatesNoManagedMemory()
        {
            var world = new SimulationWorld();
            world.ConfigureBattleEcsCharacterInputPassForDiagnostics(
                BattleEcsCharacterInputPassMode.DataOriented);
            for (int slot = 0; slot < 32; slot++)
            {
                LF2Character character = RegisterCharacter(world, slot, 600 + slot);
                character.AiControlled = true;
            }

            for (int index = 0; index < 8; index++)
                world.CharacterInputAll(index + 2);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 64; index++)
                world.CharacterInputAll(index + 10);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.Zero);
        }

        [Test]
        public void CharacterInputAll_LiveAscendingSlots_AdmitsHighNewbornAndDefersRecycledLowSlot()
        {
            var world = new SimulationWorld();
            LF2Character original = RegisterCharacter(world, 0, 100);
            LF2Character slotOne = RegisterCharacter(world, 1, 101);
            LF2Character slotTwo = RegisterCharacter(world, 2, 102);
            LF2Character replacement = null;
            LF2Character highNewborn = null;
            var visited = new List<LF2Entity>(4);

            SetMutationHook(world, (activeWorld, entity) =>
            {
                visited.Add(entity);
                if (!ReferenceEquals(entity, original))
                    return;

                activeWorld.Unregister(original);
                replacement = CreateCharacter(0, 200);
                activeWorld.Register(replacement);
                highNewborn = CreateCharacter(3, 203);
                activeWorld.Register(highNewborn);
            });

            world.CharacterInputAll(2);

            CollectionAssert.AreEqual(
                new LF2Entity[] { original, slotOne, slotTwo, highNewborn },
                visited);
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(0));
            Assert.That(highNewborn.Runtime.SlotIndex, Is.EqualTo(3));
            CollectionAssert.DoesNotContain(visited, replacement);
        }

        [Test]
        public void CharacterInputAll_MutationThrows_StillFlushesDeferredUnregisterAndRestoresTicking()
        {
            var world = new SimulationWorld();
            LF2Character trigger = RegisterCharacter(world, 0, 300);
            LF2Character removed = RegisterCharacter(world, 1, 301);

            SetMutationHook(world, (activeWorld, entity) =>
            {
                if (!ReferenceEquals(entity, trigger))
                    return;

                activeWorld.Unregister(removed);
                throw new InvalidOperationException("character-input-mutation-probe");
            });

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => world.CharacterInputAll(2));
            Assert.That(exception.Message, Is.EqualTo("character-input-mutation-probe"));

            SetMutationHook(world, null);
            LF2Character replacement = CreateCharacter(1, 401);
            Assert.DoesNotThrow(() => world.Register(replacement));
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(1));
            Assert.DoesNotThrow(() => world.CharacterInputAll(3));
        }

        [Test]
        public void RegisteredAiActionResolver_ConsumesAndCommitsRuntimeProgressDirectly()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateFrameJumpCharacter(0, 500);
            character.AiControlled = true;
            world.Register(character);
            character.Runtime.CdAttack = 5;
            character.Runtime.CdRight = 4;
            character.Runtime.ComboDra = 2;

            bool resolved = world.CharacterInputActionResolver
                .ApplyFrameInputFromRuntimeProgress(
                    character,
                    world.CharacterInputWriter);

            Assert.That(resolved, Is.True);
            Assert.That(character.Frame.N, Is.EqualTo(1));
            Assert.That(character.Runtime.Frame, Is.EqualTo(1));
            Assert.That(character.Runtime.CdAttack, Is.EqualTo(0));
            Assert.That(character.Runtime.CdRight, Is.EqualTo(0));
            Assert.That(character.Runtime.ComboDra, Is.EqualTo(2));
        }

        [Test]
        public void DataOrientedAiActionResolver_SkipsUnchangedProgressCommit()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = RegisterCharacter(world, 0, 509);

            bool resolved = world.CharacterInputActionResolver
                .ApplyFrameInputFromRuntimeProgress(
                    character,
                    world.CharacterInputWriter);

            Assert.That(resolved, Is.False);
            Assert.That(
                world.LastCharacterInputProgressCommitCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.LastCharacterInputProgressCommitSkipCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void DataOrientedAiActionResolver_CommitsChangedProgress()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = CreateFrameJumpCharacter(0, 511);
            character.AiControlled = true;
            world.Register(character);
            world.CharacterInputWriter.CommitProgressState(
                character.Runtime,
                new AiDecisionInputState { CdAttack = 5 });

            bool resolved = world.CharacterInputActionResolver
                .ApplyFrameInputFromRuntimeProgress(
                    character,
                    world.CharacterInputWriter);

            Assert.That(resolved, Is.True);
            Assert.That(character.Frame.N, Is.EqualTo(1));
            Assert.That(character.Runtime.CdAttack, Is.Zero);
            Assert.That(
                world.LastCharacterInputProgressCommitCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                world.LastCharacterInputProgressCommitSkipCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void RegisteredCharacterActionWriter_OwnsInputFrameJumpTransaction()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateFrameJumpCharacter(0, 501);
            world.Register(character);

            bool jumped = world.CharacterActionWriter
                .TryCharacterDatInputFrameJump(character, 1);

            Assert.That(jumped, Is.True);
            Assert.That(character.Frame.N, Is.EqualTo(1));
            Assert.That(character.Runtime.Frame, Is.EqualTo(1));
            Assert.That(character.Frame.D, Is.SameAs(
                character.FrameCache.GetFrameDataById(1)));
        }

        [Test]
        public void RegisteredCharacterActionWriter_AppliesAuthorityFrameVelocityTail()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 502);
            LF2FrameData frame = character.Frame.D;
            frame.dvx = 8;
            frame.dvy = 2;
            frame.dvz = 3;
            character.Runtime.SetVelocity(1.0, -4.0, 9.0);
            character.Runtime.KeyUp = 1;
            character.Runtime.KeyDown = 1;
            character.Runtime.CdUp = 4;
            character.Runtime.CdDown = 5;

            Assert.That(
                world.CharacterActionWriter.TryApplyExactCharacterFrameVelocityTail(character),
                Is.True);

            Assert.That(character.Runtime.Vx, Is.EqualTo(8.0));
            Assert.That(character.Runtime.Vy, Is.EqualTo(-2.0));
            Assert.That(character.Runtime.Vz, Is.EqualTo(3.0));

            character.Runtime.Dir = "left";
            character.Runtime.SetVelocity(2.0, 7.0, 11.0);
            frame.dvx = -6;
            frame.dvy = 550;
            frame.dvz = 547;

            Assert.That(
                world.CharacterActionWriter.TryApplyExactCharacterFrameVelocityTail(character),
                Is.True);

            Assert.That(character.Runtime.Vx, Is.EqualTo(6.0));
            Assert.That(character.Runtime.Vy, Is.Zero);
            Assert.That(character.Runtime.Vz, Is.EqualTo(-3.0));
        }

        [Test]
        public void RegisteredCharacterActionWriter_FrameVelocityTailIsAllocationFreeAfterWarmup()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 503);
            character.Frame.D.dvx = 4;
            character.Frame.D.dvy = 1;
            character.Frame.D.dvz = 2;
            character.Runtime.KeyUp = 1;
            character.Runtime.CdUp = 5;

            for (int index = 0; index < 128; index++)
                world.CharacterActionWriter.TryApplyExactCharacterFrameVelocityTail(character);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
                world.CharacterActionWriter.TryApplyExactCharacterFrameVelocityTail(character);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
        }

        [Test]
        public void RegisteredCharacterActionWriter_DerivedCharacterFailsClosed()
        {
            var world = new SimulationWorld();
            DerivedCharacter character = InitializeCharacter(
                new DerivedCharacter(),
                0,
                504);
            world.Register(character);
            character.Frame.D.dvx = 8;
            character.Runtime.Vx = 1.0;

            bool applied = world.CharacterActionWriter
                .TryApplyExactCharacterFrameVelocityTail(character);

            Assert.That(applied, Is.False);
            Assert.That(character.Runtime.Vx, Is.EqualTo(1.0));
        }

        [Test]
        public void RegisteredCharacter_UsesWorldOwnedReleaseResolverWithoutLocalAllocation()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 505);

            Assert.That(character.HasCompatibilityActionResolverForDiagnostics, Is.False);

            world.CharacterActionWriter.ProcessReleaseInput(character);

            Assert.That(character.HasCompatibilityActionResolverForDiagnostics, Is.False);
        }

        [Test]
        public void RegisteredCharacters_WorldOwnedReleaseResolverRebindsWithoutStateLeak()
        {
            var world = new SimulationWorld();
            LF2Character first = RegisterCharacter(world, 0, 507);
            LF2Character second = RegisterCharacter(world, 1, 508);

            Assert.DoesNotThrow(() =>
            {
                world.CharacterActionWriter.ProcessReleaseInput(first);
                world.CharacterActionWriter.ProcessReleaseInput(second);
                world.CharacterActionWriter.ProcessReleaseInput(first);
            });

            Assert.That(first.HasCompatibilityActionResolverForDiagnostics, Is.False);
            Assert.That(second.HasCompatibilityActionResolverForDiagnostics, Is.False);
        }

        [Test]
        public void RegisteredCharacter_WorldOwnedReleaseResolverIsAllocationFreeAfterWarmup()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 510);

            for (int index = 0; index < 128; index++)
                world.CharacterActionWriter.ProcessReleaseInput(character);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
                world.CharacterActionWriter.ProcessReleaseInput(character);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(character.HasCompatibilityActionResolverForDiagnostics, Is.False);
        }

        [Test]
        public void UnregisteredCharacter_LazilyCreatesCompatibilityReleaseResolver()
        {
            LF2Character character = InitializeCharacter(
                new LF2Character(),
                0,
                506);

            Assert.That(character.HasCompatibilityActionResolverForDiagnostics, Is.False);

            character.ProcessReleaseInputCompatibility();

            Assert.That(character.HasCompatibilityActionResolverForDiagnostics, Is.True);
        }

        [Test]
        public void FrameTransitGateways_KeepFrameAndRuntimeFrameAtomicForEveryShellType()
        {
            LF2Entity[] entities =
            {
                new LF2Character(),
                new LF2Weapon(),
                new LF2SpecialAttack(),
                new LF2OtherObject(),
            };

            for (int index = 0; index < entities.Length; index++)
            {
                LF2Entity entity = entities[index];
                int objectId = 520 + index;
                BindTwoFrameData(entity, objectId);

                entity.ImmediateFrame(0);
                entity.OnFrameTransit(1, switchDirAfterTrans: false);

                Assert.That(entity.Frame.N, Is.EqualTo(1), entity.GetType().Name);
                Assert.That(entity.Runtime.Frame, Is.EqualTo(1), entity.GetType().Name);
            }
        }

        [Test]
        public void FrameMotionStore_TracksAiProjectionMutationsAtOriginalWritePoint()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 525);
            NTSDEntityRuntime runtime = character.Runtime;

            runtime.SetPosition(12.5, 34.25, 56.75);
            runtime.SyncIntegerPosition();
            runtime.SetVelocity(-7.5, 8.25, -9.75);
            character.SwitchDir("left");
            runtime.Frame = 11;
            character.Frame.D = new LF2FrameData { state = 1004 };
            runtime.HitStop = 28;

            Assert.That(
                world.TryGetFrameMotionStateForDiagnostics(character, out BattleFrameMotionStateView state),
                Is.True);
            Assert.That(state.XInt, Is.EqualTo(12));
            Assert.That(state.YInt, Is.EqualTo(34));
            Assert.That(state.ZInt, Is.EqualTo(56));
            Assert.That(state.Vx, Is.EqualTo(-7.5));
            Assert.That(state.Facing, Is.EqualTo(1));
            Assert.That(state.Frame, Is.EqualTo(11));
            Assert.That(state.State, Is.EqualTo(1004));
            Assert.That(state.HitStop, Is.EqualTo(28));
        }

        [Test]
        public void FrameMotionStore_RejectsReleasedGenerationGhost()
        {
            var world = new SimulationWorld();
            LF2Character released = RegisterCharacter(world, 0, 526);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    released,
                    out RuntimeEntityHandle releasedHandle),
                Is.True);

            world.Unregister(released);
            LF2Character replacement = RegisterCharacter(world, 0, 527);
            released.Runtime.SetPosition(999.0, 998.0, 997.0);
            released.Runtime.Frame = 996;
            released.Frame.D = new LF2FrameData { state = 995 };

            Assert.That(
                world.TryGetFrameMotionStateForDiagnostics(
                    replacement,
                    out BattleFrameMotionStateView state),
                Is.True);
            Assert.That(state.Handle.Slot, Is.EqualTo(releasedHandle.Slot));
            Assert.That(state.Handle.Generation, Is.Not.EqualTo(releasedHandle.Generation));
            Assert.That(state.XInt, Is.EqualTo(replacement.Runtime.XInt));
            Assert.That(state.YInt, Is.EqualTo(replacement.Runtime.YInt));
            Assert.That(state.ZInt, Is.EqualTo(replacement.Runtime.ZInt));
            Assert.That(state.Frame, Is.EqualTo(replacement.Runtime.Frame));
            Assert.That(state.State, Is.EqualTo(replacement.GetState()));
        }

        [Test]
        public void FrameMotionStore_HotMutationPath_DoesNotAllocate()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 528);
            NTSDEntityRuntime runtime = character.Runtime;

            MutateFrameMotion(runtime, 1);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 2; index < 258; index++)
                MutateFrameMotion(runtime, index);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(
                world.TryGetFrameMotionStateForDiagnostics(character, out BattleFrameMotionStateView state),
                Is.True);
            Assert.That(state.Frame, Is.EqualTo(257));
            Assert.That(state.XInt, Is.EqualTo(257));
        }

        [Test]
        public void RelationLinkStore_TracksLowFrequencyMutationsAtOriginalWritePoint()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 529);
            NTSDEntityRuntime runtime = character.Runtime;

            runtime.RelationTeam = 4;
            runtime.LinkState = 101;
            runtime.KillCount = 27;
            runtime.TargetSlotIndex = 38;

            Assert.That(
                world.TryGetRelationLinkStateForDiagnostics(
                    character,
                    out BattleRelationLinkStateView state),
                Is.True);
            Assert.That(state.RelationTeam, Is.EqualTo(4));
            Assert.That(state.LinkState, Is.EqualTo(101));
            Assert.That(state.KillCount, Is.EqualTo(27));
            Assert.That(state.TargetSlot, Is.EqualTo(38));
        }

        [Test]
        public void RelationLinkStore_RejectsReleasedGenerationGhost()
        {
            var world = new SimulationWorld();
            LF2Character released = RegisterCharacter(world, 0, 530);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    released,
                    out RuntimeEntityHandle releasedHandle),
                Is.True);

            world.Unregister(released);
            LF2Character replacement = RegisterCharacter(world, 0, 531);
            released.Runtime.RelationTeam = 999;
            released.Runtime.LinkState = 998;
            released.Runtime.KillCount = 997;
            released.Runtime.TargetSlotIndex = 996;

            Assert.That(
                world.TryGetRelationLinkStateForDiagnostics(
                    replacement,
                    out BattleRelationLinkStateView state),
                Is.True);
            Assert.That(state.Handle.Slot, Is.EqualTo(releasedHandle.Slot));
            Assert.That(state.Handle.Generation, Is.Not.EqualTo(releasedHandle.Generation));
            Assert.That(state.RelationTeam, Is.EqualTo(replacement.Runtime.RelationTeam));
            Assert.That(state.LinkState, Is.EqualTo(replacement.Runtime.LinkState));
            Assert.That(state.KillCount, Is.EqualTo(replacement.Runtime.KillCount));
            Assert.That(state.TargetSlot, Is.EqualTo(replacement.Runtime.TargetSlotIndex));
        }

        [Test]
        public void RelationLinkStore_HotMutationPath_DoesNotAllocate()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 532);
            NTSDEntityRuntime runtime = character.Runtime;

            MutateRelationLink(runtime, 1);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 2; index < 258; index++)
                MutateRelationLink(runtime, index);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(
                world.TryGetRelationLinkStateForDiagnostics(
                    character,
                    out BattleRelationLinkStateView state),
                Is.True);
            Assert.That(state.RelationTeam, Is.EqualTo(257));
            Assert.That(state.LinkState, Is.EqualTo(-257));
            Assert.That(state.KillCount, Is.EqualTo(514));
            Assert.That(state.TargetSlot, Is.EqualTo(771));
        }

        [Test]
        public void VitalStore_TracksHealthWrapperMutationsAtOriginalWritePoint()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 533);

            character.Health.HP = 432;
            character.Health.HPBound = 421;
            character.Health.HP3 = 498;
            character.Health.PP = 317;

            Assert.That(
                world.TryGetVitalStateForDiagnostics(
                    character,
                    out BattleVitalStateView state),
                Is.True);
            Assert.That(state.Hp, Is.EqualTo(432));
            Assert.That(state.HpBound, Is.EqualTo(421));
            Assert.That(state.Hp3, Is.EqualTo(498));
            Assert.That(state.Pp, Is.EqualTo(317));
        }

        [Test]
        public void VitalStore_RejectsReleasedGenerationGhost()
        {
            var world = new SimulationWorld();
            LF2Character released = RegisterCharacter(world, 0, 534);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    released,
                    out RuntimeEntityHandle releasedHandle),
                Is.True);

            world.Unregister(released);
            LF2Character replacement = RegisterCharacter(world, 0, 535);
            released.Runtime.HP = 1;
            released.Runtime.HPBound = 2;
            released.Runtime.HP3 = 3;
            released.Runtime.PP = 4;

            Assert.That(
                world.TryGetVitalStateForDiagnostics(
                    replacement,
                    out BattleVitalStateView state),
                Is.True);
            Assert.That(state.Handle.Slot, Is.EqualTo(releasedHandle.Slot));
            Assert.That(state.Handle.Generation, Is.Not.EqualTo(releasedHandle.Generation));
            Assert.That(state.Hp, Is.EqualTo(replacement.Runtime.HP));
            Assert.That(state.HpBound, Is.EqualTo(replacement.Runtime.HPBound));
            Assert.That(state.Hp3, Is.EqualTo(replacement.Runtime.HP3));
            Assert.That(state.Pp, Is.EqualTo(replacement.Runtime.PP));
        }

        [Test]
        public void VitalStore_HotMutationPath_DoesNotAllocate()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 536);
            NTSDEntityRuntime runtime = character.Runtime;

            MutateVitals(runtime, 1);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 2; index < 258; index++)
                MutateVitals(runtime, index);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(
                world.TryGetVitalStateForDiagnostics(
                    character,
                    out BattleVitalStateView state),
                Is.True);
            Assert.That(state.Hp, Is.EqualTo(257));
            Assert.That(state.HpBound, Is.EqualTo(514));
            Assert.That(state.Hp3, Is.EqualTo(771));
            Assert.That(state.Pp, Is.EqualTo(1028));
        }

        [Test]
        public void DataOrientedCharacterInputStore_IsCanonicalForAiCapture()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = RegisterCharacter(world, 0, 502);
            var input = new AiDecisionInputState
            {
                History0 = 1,
                History5 = 6,
                KeyRight = 1,
                PrevLeft = 1,
                CdAttack = 5,
                ComboDra = 2,
            };

            world.CharacterInputWriter.CommitAiDecisionState(
                character.Runtime,
                input);
            character.Runtime.KeyRight = 0;
            character.Runtime.CdAttack = 0;

            Assert.That(
                world.CharacterInputWriter.TryCaptureCanonicalState(
                    character.Runtime,
                    out AiDecisionInputState captured),
                Is.True);
            Assert.That(captured.KeyRight, Is.EqualTo(1));
            Assert.That(captured.PrevLeft, Is.EqualTo(1));
            Assert.That(captured.CdAttack, Is.EqualTo(5));
            Assert.That(captured.ComboDra, Is.EqualTo(2));
            Assert.That(captured.History0, Is.EqualTo(1));
            Assert.That(captured.History5, Is.EqualTo(6));
        }

        [Test]
        public void DataOrientedCharacterInputStore_PublishesAiProjectionOnlyWhenChanged()
        {
            const int capacity = 1;
            const uint generation = 1;
            var publisher = new BattleAiUnifiedRowPublisher(capacity);
            var included = new[] { true };
            var generations = new[] { generation };
            var dataObjectType = new[] { (int)LF2ObjectType.Character };
            var inputHistoryGate = new bool[capacity];
            var x = new int[capacity];
            var y = new int[capacity];
            var z = new int[capacity];
            var hp = new[] { 500 };
            var hp3 = new int[capacity];
            var hpMax = new[] { 500 };
            var pp = new int[capacity];
            var team = new int[capacity];
            var state = new int[capacity];
            var frame = new int[capacity];
            var linkState = new int[capacity];
            var killCount = new int[capacity];
            var cachedTargetSlot = new[] { -1 };
            var coordinateTargetX = new[] { -1000 };
            var vx = new double[capacity];
            var facing = new int[capacity];
            var targetSlot = new int[capacity];
            var hitStop = new int[capacity];
            var rowSensingBoundaryFlags = new int[capacity];
            var publishedSensingBoundaryFlags = new int[capacity];
            var publishedDecisionBoundaryFlags = new int[capacity];
            publisher.BeginPass(
                1,
                included,
                generations,
                dataObjectType,
                inputHistoryGate,
                x,
                y,
                z,
                hp,
                hp3,
                hpMax,
                pp,
                team,
                state,
                frame,
                linkState,
                killCount,
                cachedTargetSlot,
                coordinateTargetX,
                vx,
                facing,
                targetSlot,
                hitStop,
                rowSensingBoundaryFlags,
                publishedSensingBoundaryFlags,
                publishedDecisionBoundaryFlags);

            var runtime = new NTSDEntityRuntime
            {
                SlotIndex = 0,
                Unk360 = -1,
                Unk3FC = -1000,
            };
            var store = new BattleCharacterInputStore(capacity, publisher);
            store.Bind(runtime, new RuntimeEntityHandle(0, generation));
            Assert.That(
                store.TryCaptureCommon(runtime, out AiDecisionInputState input),
                Is.True);

            store.ResetAiProjectionPublicationDiagnostics();
            store.CommitFull(runtime, input, true);
            Assert.That(
                store.LastAiProjectionPublicationCountForDiagnostics,
                Is.Zero);
            Assert.That(
                store.LastAiProjectionPublicationSkipCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                publisher.HasPendingValues(0, generation),
                Is.False);
            Assert.That(
                publisher.TryCommitPending(0, generation, out _, out _),
                Is.True);

            input.Unk360 = 7;
            store.CommitFull(runtime, input, true);
            Assert.That(
                store.LastAiProjectionPublicationCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                store.LastAiProjectionPublicationSkipCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                publisher.HasPendingValues(0, generation),
                Is.True);
            Assert.That(
                publisher.TryCommitPending(0, generation, out _, out _),
                Is.True);
            Assert.That(
                publisher.HasPendingValues(0, generation),
                Is.False);
            Assert.That(cachedTargetSlot[0], Is.EqualTo(7));

            publisher.EndPass();
        }

        [Test]
        public void DataOrientedCharacterInputStore_HumanFullCommitPreservesAiHistory()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = RegisterCharacter(world, 0, 505);
            world.CharacterInputWriter.CommitAiDecisionState(
                character.Runtime,
                new AiDecisionInputState
                {
                    History0 = 1,
                    History5 = 9,
                    KeyRight = 1,
                    CdAttack = 5,
                });

            world.CharacterInputWriter.CommitFullState(
                character.Runtime,
                new AiDecisionInputState
                {
                    KeyLeft = 1,
                    CdAttack = 2,
                });

            Assert.That(
                world.CharacterInputWriter.TryCaptureCanonicalState(
                    character.Runtime,
                    out AiDecisionInputState captured),
                Is.True);
            Assert.That(captured.History0, Is.EqualTo(1));
            Assert.That(captured.History5, Is.EqualTo(9));
            Assert.That(captured.KeyRight, Is.Zero);
            Assert.That(captured.KeyLeft, Is.EqualTo(1));
            Assert.That(captured.CdAttack, Is.EqualTo(2));
        }

        [Test]
        public void DataOrientedCharacterInputStore_OwnsAiTargetAndResetPreservesIt()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character character = RegisterCharacter(world, 0, 506);
            world.CharacterInputWriter.CommitAiDecisionState(
                character.Runtime,
                new AiDecisionInputState
                {
                    Unk360 = 7,
                    Unk3FC = 400,
                    Unk400 = 220,
                });
            world.CharacterInputWriter.ResetInputState(character.Runtime);
            character.Runtime.Unk360 = -1;
            character.Runtime.Unk3FC = -1000;
            character.Runtime.Unk400 = -1000;

            Assert.That(
                world.CharacterInputWriter.TryCaptureCanonicalState(
                    character.Runtime,
                    out AiDecisionInputState captured),
                Is.True);
            Assert.That(captured.Unk360, Is.EqualTo(7));
            Assert.That(captured.Unk3FC, Is.EqualTo(400));
            Assert.That(captured.Unk400, Is.EqualTo(220));

            world.AiInputWriter.SetCoordinateTarget(
                character.Runtime,
                500,
                300);
            Assert.That(
                world.CharacterInputWriter.TryCaptureCanonicalState(
                    character.Runtime,
                    out captured),
                Is.True);
            Assert.That(captured.Unk360, Is.EqualTo(7));
            Assert.That(captured.Unk3FC, Is.EqualTo(500));
            Assert.That(captured.Unk400, Is.EqualTo(300));
            Assert.That(character.Runtime.Unk3FC, Is.EqualTo(500));
            Assert.That(character.Runtime.Unk400, Is.EqualTo(300));
            Assert.That(
                world.CharacterInputWriter.TryCaptureAiProjection(
                    character.Runtime,
                    out BattleCharacterInputAiProjection projection),
                Is.True);
            Assert.That(projection.InputHistoryGate, Is.False);
            Assert.That(projection.CachedTargetSlot, Is.EqualTo(7));
            Assert.That(projection.CoordinateTargetX, Is.EqualTo(500));
        }

        [Test]
        public void DataOrientedCharacterInputStore_OwnsBoundaryPublishAndConsume()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character victim = RegisterCharacter(world, 0, 507);
            LF2Character attacker = RegisterCharacter(world, 1, 508);
            victim.Runtime.Vx = 2.0;
            attacker.Runtime.XInt = victim.Runtime.XInt + 10;

            Assert.That(
                world.BoundaryWriter.TryApplyKind14DirectionalBlock(
                    attacker,
                    victim),
                Is.True);
            victim.Runtime.XBoundPositive = false;
            Assert.That(
                world.CharacterInputWriter.TryCaptureCanonicalState(
                    victim.Runtime,
                    out AiDecisionInputState captured),
                Is.True);
            Assert.That(captured.BoundaryFlags & 1, Is.EqualTo(1));
            Assert.That(
                world.CharacterInputWriter.TryCaptureAiProjection(
                    victim.Runtime,
                    out BattleCharacterInputAiProjection projection),
                Is.True);
            Assert.That(projection.DecisionBoundaryFlags, Is.EqualTo(1));

            world.BoundaryWriter.SyncConsumedFlags(victim.Runtime);
            Assert.That(
                world.CharacterInputWriter.TryCaptureCanonicalState(
                    victim.Runtime,
                    out captured),
                Is.True);
            Assert.That(captured.BoundaryFlags, Is.Zero);
            Assert.That(
                world.CharacterInputWriter.TryCaptureAiProjection(
                    victim.Runtime,
                    out projection),
                Is.True);
            Assert.That(projection.DecisionBoundaryFlags, Is.Zero);
        }

        [Test]
        public void DataOrientedCharacterInputStore_RejectsReleasedGenerationGhost()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character first = RegisterCharacter(world, 0, 503);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    first,
                    out RuntimeEntityHandle releasedHandle),
                Is.True);
            world.CharacterInputWriter.CommitAiDecisionState(
                first.Runtime,
                new AiDecisionInputState { KeyRight = 1 });

            world.Unregister(first);
            LF2Character replacement = RegisterCharacter(world, 0, 504);
            world.CharacterInputWriter.Release(releasedHandle);

            Assert.That(
                world.CharacterInputWriter.TryCaptureCanonicalState(
                    replacement.Runtime,
                    out AiDecisionInputState captured),
                Is.True);
            Assert.That(captured.KeyRight, Is.EqualTo(0));
        }

        [Test]
        public void CanonicalAiProjectionReaders_RequireCurrentSlotGeneration()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            LF2Character first = RegisterCharacter(world, 0, 531);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    first,
                    out RuntimeEntityHandle firstHandle),
                Is.True);

            Assert.That(
                world.IdentityWriter.TryCaptureAiProjection(
                    firstHandle,
                    out BattleIdentityAiProjection identity),
                Is.True);
            Assert.That(identity.StableId, Is.EqualTo(first.Runtime.StableId));
            Assert.That(identity.ObjectId, Is.EqualTo(first.ObjectId));
            Assert.That(
                identity.DataObjectType,
                Is.EqualTo(first.GetCurrentDataObjectTypeForSimulation()));
            Assert.That(
                world.FrameMotionWriter.TryCaptureAiProjection(
                    firstHandle,
                    out BattleFrameMotionAiProjection frameMotion),
                Is.True);
            Assert.That(frameMotion.X, Is.EqualTo(first.Runtime.XInt));
            Assert.That(
                world.CharacterInputWriter.TryCaptureAiProjection(
                    firstHandle,
                    out BattleCharacterInputAiProjection input),
                Is.True);
            Assert.That(input.CachedTargetSlot, Is.EqualTo(first.Runtime.Unk360));
            Assert.That(input.DecisionBoundaryFlags, Is.Zero);
            Assert.That(
                world.RelationLinkWriter.TryCaptureAiProjection(
                    firstHandle,
                    out BattleRelationLinkAiProjection relationLink),
                Is.True);
            Assert.That(relationLink.RelationTeam, Is.EqualTo(first.Runtime.RelationTeam));
            Assert.That(
                world.VitalWriter.TryCaptureAiProjection(
                    firstHandle,
                    out BattleVitalAiProjection vital),
                Is.True);
            Assert.That(vital.Hp, Is.EqualTo(first.Runtime.HP));

            world.Unregister(first);
            RegisterCharacter(world, 0, 532);

            Assert.That(
                world.IdentityWriter.TryCaptureAiProjection(
                    firstHandle,
                    out identity),
                Is.False);
            Assert.That(
                world.FrameMotionWriter.TryCaptureAiProjection(
                    firstHandle,
                    out frameMotion),
                Is.False);
            Assert.That(
                world.CharacterInputWriter.TryCaptureAiProjection(
                    firstHandle,
                    out input),
                Is.False);
            Assert.That(
                world.RelationLinkWriter.TryCaptureAiProjection(
                    firstHandle,
                    out relationLink),
                Is.False);
            Assert.That(
                world.VitalWriter.TryCaptureAiProjection(
                    firstHandle,
                    out vital),
                Is.False);
        }

        [Test]
        public void IdentityStore_TracksObjectIdMutationAtOriginalWritePoint()
        {
            var world = new SimulationWorld();
            LF2Character character = RegisterCharacter(world, 0, 533);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    character,
                    out RuntimeEntityHandle handle),
                Is.True);

            character.ObjectId = 733;

            Assert.That(
                world.IdentityWriter.TryCaptureAiProjection(
                    handle,
                    out BattleIdentityAiProjection identity),
                Is.True);
            Assert.That(identity.StableId, Is.EqualTo(character.Runtime.StableId));
            Assert.That(identity.ObjectId, Is.EqualTo(733));
            Assert.That(
                identity.DataObjectType,
                Is.EqualTo(character.GetCurrentDataObjectTypeForSimulation()));
        }

        [Test]
        public void IdentityStore_TracksFrameCacheDataTypeMutationAtOriginalWritePoint()
        {
            var world = new SimulationWorld();
            var character = InitializeCharacter(
                new MutableDataTypeCharacter
                {
                    CurrentDataObjectType = (int)LF2ObjectType.Character,
                },
                0,
                534);
            world.Register(character);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    0,
                    character,
                    out RuntimeEntityHandle handle),
                Is.True);

            character.CurrentDataObjectType = (int)LF2ObjectType.HeavyWeapon;
            character.FrameCache.Load(character.FrameCache.Wrapper);

            Assert.That(
                world.IdentityWriter.TryCaptureAiProjection(
                    handle,
                    out BattleIdentityAiProjection identity),
                Is.True);
            Assert.That(
                identity.DataObjectType,
                Is.EqualTo((int)LF2ObjectType.HeavyWeapon));
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            LF2Character character = CreateCharacter(slot, objectId);
            world.Register(character);
            return character;
        }

        private static void MutateFrameMotion(NTSDEntityRuntime runtime, int value)
        {
            runtime.SetPosition(value + 0.25, value + 0.5, value + 0.75);
            runtime.SyncIntegerPosition();
            runtime.SetVelocity(-value, value, value * 0.5);
            runtime.Dir = (value & 1) == 0 ? "right" : "left";
            runtime.Frame = value;
            runtime.PrevFrame2 = value - 1;
            runtime.WaitCounter = value + 1;
            runtime.FrameWaitCounter = value + 2;
            runtime.NextFrame = value + 3;
            runtime.FrameDelay = value + 4;
            runtime.HitStop = value + 5;
            runtime.AttackingCounter = value + 6;
        }

        private static void MutateRelationLink(NTSDEntityRuntime runtime, int value)
        {
            runtime.RelationTeam = value;
            runtime.LinkState = -value;
            runtime.KillCount = value * 2;
            runtime.TargetSlotIndex = value * 3;
        }

        private static void MutateVitals(NTSDEntityRuntime runtime, int value)
        {
            runtime.HP = value;
            runtime.HPBound = value * 2;
            runtime.HP3 = value * 3;
            runtime.PP = value * 4;
        }

        private static LF2Character CreateCharacter(int slot, int objectId)
        {
            return InitializeCharacter(new LF2Character(), slot, objectId);
        }

        private static T InitializeCharacter<T>(
            T character,
            int slot,
            int objectId)
            where T : LF2Character
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = $"CharacterInputLiveSlot_{slot}_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(slot);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Runtime.SetPosition(slot * 20, 0, 0);
            character.Runtime.SyncIntegerPosition();
            character.Controller = new EmptyController();
            character.AiControlled = false;
            return character;
        }

        private static LF2Character CreateFrameJumpCharacter(int slot, int objectId)
        {
            var root = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
                hit_a = 1,
            };
            var target = new LF2FrameData
            {
                frameId = 1,
                state = 3,
                wait = 100,
                next = 1,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = $"CharacterInputAction_{slot}_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { root, target },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(slot);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Runtime.SetPosition(0, 0, 0);
            character.Runtime.SyncIntegerPosition();
            character.Controller = new EmptyController();
            return character;
        }

        private static void BindTwoFrameData(LF2Entity entity, int objectId)
        {
            var first = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            var second = new LF2FrameData
            {
                frameId = 1,
                state = 0,
                wait = 100,
                next = 1,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = $"FrameTransitGateway_{objectId}",
                type_sub = (int)entity.ObjectTypeEnum,
                frames = new List<LF2FrameData> { first, second },
            };

            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
        }

        private static void SetMutationHook(
            SimulationWorld world,
            Action<SimulationWorld, LF2Entity> hook)
        {
            world.SetCharacterInputPassMutationOverrideForSelfCheck(hook);
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }

        private sealed class DerivedCharacter : LF2Character
        {
        }

        private sealed class MutableDataTypeCharacter : LF2Character
        {
            internal int CurrentDataObjectType { get; set; }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return CurrentDataObjectType;
            }
        }

    }
}
#endif
