#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeComboActionTransactionEditorTests
    {
        [Test]
        public void CurrentRemap_IsAtomicAndLeavesPreviousUntouched()
        {
            var runtime = new NTSDEntityRuntime { InputRemapState138 = 1 };
            for (int index = 0; index < 7; index++)
            {
                runtime.NativeInputProxy.Current[index] = (byte)(index + 1);
                runtime.NativeInputProxy.Previous[index] = (byte)(20 + index);
                runtime.InputRemapIndices13C[index] = (byte)(6 - index);
            }
            byte[] previous = runtime.NativeInputProxy.Previous.ToArray();

            NTSD28NativeInputRemapResult applied =
                NTSD28NativeInputPreprocessor.ApplyCurrentButtonRemap(runtime);

            Assert.That(applied, Is.EqualTo(NTSD28NativeInputRemapResult.Applied));
            Assert.That(runtime.NativeInputProxy.Current,
                Is.EqualTo(new byte[] { 7, 6, 5, 4, 3, 2, 1 }));
            Assert.That(runtime.NativeInputProxy.Previous, Is.EqualTo(previous));

            byte[] sampled = runtime.NativeInputProxy.Current.ToArray();
            runtime.InputRemapIndices13C[3] = 7;
            NTSD28NativeInputRemapResult rejected =
                NTSD28NativeInputPreprocessor.ApplyCurrentButtonRemap(runtime);

            Assert.That(rejected, Is.EqualTo(NTSD28NativeInputRemapResult.Invalid));
            Assert.That(runtime.NativeInputProxy.Current, Is.EqualTo(sampled));
        }

        [Test]
        public void CurrentRemap_DuplicateDestinationsUseNativeSourceOrder()
        {
            var runtime = new NTSDEntityRuntime { InputRemapState138 = 1 };
            for (int index = 0; index < 7; index++)
            {
                runtime.NativeInputProxy.Current[index] = (byte)(index + 1);
                runtime.InputRemapIndices13C[index] = 0;
            }

            NTSD28NativeInputRemapResult result =
                NTSD28NativeInputPreprocessor.ApplyCurrentButtonRemap(runtime);

            Assert.That(result, Is.EqualTo(NTSD28NativeInputRemapResult.Applied));
            Assert.That(runtime.NativeInputProxy.Current,
                Is.EqualTo(new byte[] { 7, 0, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void BoundGate_SuppressesOnlyNativeJumpRisingEdge()
        {
            var runtime = new NTSDEntityRuntime
            {
                BoundState198 = 4,
                InputGlobalRecordState20 = 0,
            };
            NTSD28NativeComboStateMachine.InitializeNativeHistory(runtime);
            runtime.NativeInputProxy.Current[4] = 1;
            runtime.NativeInputProxy.Current[5] = 1;

            bool suppress =
                NTSD28NativeInputPreprocessor.ShouldSuppressJumpEdge(runtime);
            NTSD28NativeComboStateMachine.ProcessSampledInput(runtime, suppress);

            Assert.That(suppress, Is.True);
            Assert.That(runtime.NativeInputProxy.EdgeWindow[0], Is.EqualTo(5));
            Assert.That(runtime.NativeInputProxy.EdgeWindow[1], Is.Zero);
            Assert.That(runtime.NativeInputProxy.Current[5], Is.EqualTo(1));
            Assert.That(runtime.NativeInputProxy.Previous[5], Is.Zero);

            runtime.InputGlobalRecordState20 = 1;
            Assert.That(NTSD28NativeInputPreprocessor.ShouldSuppressJumpEdge(runtime),
                Is.False);
            runtime.InputGlobalRecordState20 = 3;
            Assert.That(NTSD28NativeInputPreprocessor.ShouldSuppressJumpEdge(runtime),
                Is.False);
        }

        [Test]
        public void ActionTransaction_RejectsLockMinMagnitudeAndMissingSourceFrame()
        {
            using CharacterScope scope = CreateScope(Frame(0), Frame(20));
            NTSDEntityRuntime runtime = scope.Character.Runtime;
            runtime.InputActionLock130 = 1;

            NTSD28NativeActionAttempt locked = scope.Writer.ApplyNativeInputAction(
                scope.Character,
                20);

            Assert.That(locked.Attempted, Is.True);
            Assert.That(locked.Applied, Is.False);
            Assert.That(locked.Failure, Is.EqualTo(NTSD28NativeActionFailure.ActionLocked));
            Assert.That(scope.Character.Frame.N, Is.Zero);

            runtime.InputActionLock130 = 0;
            NTSD28NativeActionAttempt min = scope.Writer.ApplyNativeInputAction(
                scope.Character,
                int.MinValue);
            NTSD28NativeActionAttempt missing = scope.Writer.ApplyNativeInputAction(
                scope.Character,
                1000);

            Assert.That(min.Failure,
                Is.EqualTo(NTSD28NativeActionFailure.UnrepresentableMagnitude));
            Assert.That(missing.Failure,
                Is.EqualTo(NTSD28NativeActionFailure.SourceFrameMissing));
            NTSD28NativeActionAttempt implicitFrame = scope.Writer.ApplyNativeInputAction(scope.Character, 21);
            Assert.That(implicitFrame.Applied, Is.True);
            Assert.That(scope.Character.Frame.D, Is.SameAs(scope.Character.FrameCache.GetNativeFrameDataById(21)));

            runtime.InputLastAction144 = 123;
            runtime.AttackingCounter = 9;
            NTSD28NativeActionAttempt idle = scope.Writer.ApplyNativeInputAction(
                scope.Character,
                999);
            Assert.That(idle.Applied, Is.True);
            Assert.That(idle.ResolvedAction, Is.Zero);
            Assert.That(runtime.InputLastAction144, Is.Zero);
            Assert.That(runtime.AttackingCounter, Is.EqualTo(9));
        }

        [Test]
        public void ActionTransaction_StateRedirectUsesSourceFrameCostAndExactThresholds()
        {
            LF2FrameData redirect = Frame(271, state: 1150272, mp: 350);
            using CharacterScope scope = CreateScope(
                Frame(0), redirect, Frame(272), Frame(10, state: 1150011),
                Frame(11), Frame(20, state: 2200021), Frame(21));
            LF2Character character = scope.Character;
            character.Health.HP = 500;
            character.Health.PP = 500;

            NTSD28NativeActionAttempt hpRedirect =
                scope.Writer.ApplyNativeInputAction(character, 271);

            Assert.That(hpRedirect.Applied, Is.True);
            Assert.That(hpRedirect.ResolvedAction, Is.EqualTo(272));
            Assert.That(hpRedirect.MpCost, Is.EqualTo(350));
            Assert.That(character.Frame.N, Is.EqualTo(272));
            Assert.That(character.Health.PP, Is.EqualTo(150));
            Assert.That(character.Runtime.InputLastAction144, Is.EqualTo(272));

            SetCurrentFrame(character, 0);
            character.Health.HP = 150;
            Assert.That(scope.Writer.ApplyNativeInputAction(character, 10)
                .ResolvedAction, Is.EqualTo(10));

            SetCurrentFrame(character, 0);
            character.Health.PP = 200;
            Assert.That(scope.Writer.ApplyNativeInputAction(character, 20)
                .ResolvedAction, Is.EqualTo(21));
        }

        [Test]
        public void ActionTransaction_UndefinedRedirectWritesRawTargetAndKeepsSourceCost()
        {
            using CharacterScope scope = CreateScope(
                Frame(0),
                Frame(10, state: 1001777, mp: 25));
            LF2Character character = scope.Character;
            character.Health.HP = 500;
            character.Health.PP = 100;

            NTSD28NativeActionAttempt result =
                scope.Writer.ApplyNativeInputAction(character, 10);

            Assert.That(result.Applied, Is.True);
            Assert.That(result.ResolvedAction, Is.EqualTo(777));
            Assert.That(result.MpCost, Is.EqualTo(25));
            Assert.That(character.Health.PP, Is.EqualTo(75));
            Assert.That(character.Frame.N, Is.EqualTo(777));
            Assert.That(character.Frame.D, Is.SameAs(character.FrameCache.GetNativeFrameDataById(777)));
            Assert.That(character.Runtime.InputLastAction144, Is.EqualTo(777));
        }

        [Test]
        public void ActionTransaction_HpOnlyFieldBypassesCostAndExactHpGateRejects()
        {
            using CharacterScope hpOnly = CreateScope(
                Frame(0),
                Frame(20, hp: 90));
            hpOnly.Character.Health.HP = 100;
            hpOnly.Character.Health.HPBound = 100;

            NTSD28NativeActionAttempt bypassed =
                hpOnly.Writer.ApplyNativeInputAction(hpOnly.Character, 20);

            Assert.That(bypassed.Applied, Is.True);
            Assert.That(bypassed.HpCost, Is.Zero);
            Assert.That(hpOnly.Character.Health.HP, Is.EqualTo(100));
            Assert.That(hpOnly.Character.Health.HPBound, Is.EqualTo(100));
            Assert.That(hpOnly.Character.Runtime.InputHpConsumedTotal34C, Is.Zero);

            using CharacterScope exact = CreateScope(
                Frame(0),
                Frame(20, mp: 1000));
            exact.Character.Health.HP = 10;
            exact.Character.Health.PP = 100;

            NTSD28NativeActionAttempt rejected =
                exact.Writer.ApplyNativeInputAction(exact.Character, 20);

            Assert.That(rejected.Applied, Is.False);
            Assert.That(rejected.HpCost, Is.EqualTo(10));
            Assert.That(rejected.Failure,
                Is.EqualTo(NTSD28NativeActionFailure.ResourceRejected));
            Assert.That(exact.Character.Health.HP, Is.EqualTo(10));
            Assert.That(exact.Character.Frame.N, Is.Zero);
        }

        [Test]
        public void ActionTransaction_AppliesRecmpDoubleHpAndEffectiveMaxAccounting()
        {
            using CharacterScope scope = CreateScope(
                Frame(0),
                Frame(20, state: 3, mp: 2025, hp: 9));
            LF2Character character = scope.Character;
            character.FrameCache.Wrapper.characterData.recmp = 50;
            character.Health.HP = 100;
            character.Health.HPBound = 100;
            character.Health.PP = 30;
            character.Runtime.InputModeCostMultiplier30 = 25;
            character.Runtime.InputDoubleCost19C = 1;

            NTSD28NativeActionAttempt result =
                scope.Writer.ApplyNativeInputAction(character, 20);

            Assert.That(result.Applied, Is.True);
            Assert.That(result.MpCost, Is.EqualTo(24));
            Assert.That(result.HpCost, Is.EqualTo(29));
            Assert.That(result.EffectiveMaxHpCost, Is.EqualTo(3));
            Assert.That(character.Health.PP, Is.EqualTo(6));
            Assert.That(character.Health.HP, Is.EqualTo(71));
            Assert.That(character.Health.HPBound, Is.EqualTo(97));
            Assert.That(character.Runtime.InputMpConsumedTotal350, Is.EqualTo(24));
            Assert.That(character.Runtime.InputHpConsumedTotal34C, Is.EqualTo(29));
        }

        [Test]
        public void ActionTransaction_UsesModeWaiverAndF6Policies()
        {
            using CharacterScope mode = CreateScope(Frame(0), Frame(20, mp: 25));
            mode.Character.Health.PP = 30;
            mode.Character.Runtime.InputModeCostMultiplier30 = 50;
            NTSD28NativeActionAttempt scaled =
                mode.Writer.ApplyNativeInputAction(mode.Character, 20);
            Assert.That(scaled.MpCost, Is.EqualTo(12));
            Assert.That(mode.Character.Health.PP, Is.EqualTo(18));

            using CharacterScope waived = CreateScope(Frame(0), Frame(20, mp: 25));
            waived.Character.Health.PP = 0;
            waived.Character.Runtime.InputCostWaived1B4 = 1;
            NTSD28NativeActionAttempt waivedResult =
                waived.Writer.ApplyNativeInputAction(waived.Character, 20);
            Assert.That(waivedResult.Applied, Is.True);
            Assert.That(waivedResult.MpCost, Is.Zero);
            Assert.That(waived.Character.Health.PP, Is.Zero);

            using CharacterScope f6 = CreateScope(Frame(0), Frame(20, mp: 2025, hp: 9));
            f6.Character.Health.PP = 0;
            f6.Character.Health.HP = 100;
            f6.Character.Runtime.InputLocalResourceEnabled49D034 = false;
            NTSD28NativeActionAttempt f6Result =
                f6.Writer.ApplyNativeInputAction(f6.Character, 20);
            Assert.That(f6Result.Applied, Is.True);
            Assert.That(f6Result.MpCost, Is.Zero);
            Assert.That(f6Result.HpCost, Is.Zero);
            Assert.That(f6.Character.Health.PP, Is.Zero);
            Assert.That(f6.Character.Health.HP, Is.EqualTo(100));
        }

        [Test]
        public void ActionTransaction_UsesFallbackPriorityWithoutCostOrLastActionWrite()
        {
            using CharacterScope first = CreateScope(Frame(0), Frame(20, mp: 25));
            first.Character.Health.PP = 0;
            first.Character.Runtime.InputSpecialGate194 = 71;
            first.Character.Runtime.InputModeFallbackActionB8 = 91;
            first.Character.FrameCache.Wrapper.characterData.caughtact = 81;
            first.Character.Runtime.InputLastAction144 = 333;

            NTSD28NativeActionAttempt gate =
                first.Writer.ApplyNativeInputAction(first.Character, -20);

            Assert.That(gate.Applied, Is.True);
            Assert.That(gate.UsedFallback, Is.True);
            Assert.That(gate.ResolvedAction, Is.EqualTo(71));
            Assert.That(first.Character.Frame.N, Is.EqualTo(71));
            Assert.That(first.Character.Frame.D, Is.SameAs(first.Character.FrameCache.GetNativeFrameDataById(71)));
            Assert.That(first.Character.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(first.Character.Runtime.InputLastAction144, Is.EqualTo(333));
            Assert.That(first.Character.Runtime.InputMpConsumedTotal350, Is.Zero);

            using CharacterScope stats = CreateScope(Frame(0), Frame(20, mp: 25));
            stats.Character.Health.PP = 0;
            stats.Character.FrameCache.Wrapper.characterData.caughtact = 81;
            stats.Character.Runtime.InputModeFallbackActionB8 = 91;
            Assert.That(stats.Writer.ApplyNativeInputAction(stats.Character, 20)
                .ResolvedAction, Is.EqualTo(81));

            using CharacterScope mode = CreateScope(Frame(0), Frame(20, mp: 25));
            mode.Character.Health.PP = 0;
            mode.Character.Runtime.InputModeFallbackActionB8 = 91;
            Assert.That(mode.Writer.ApplyNativeInputAction(mode.Character, 20)
                .ResolvedAction, Is.EqualTo(91));

            using CharacterScope absent = CreateScope(Frame(0), Frame(20, mp: 25));
            absent.Character.Health.PP = 0;
            NTSD28NativeActionAttempt rejected =
                absent.Writer.ApplyNativeInputAction(absent.Character, 20);
            Assert.That(rejected.Applied, Is.False);
            Assert.That(rejected.Failure,
                Is.EqualTo(NTSD28NativeActionFailure.ResourceRejected));
            Assert.That(absent.Character.Frame.N, Is.Zero);
        }

        [Test]
        public void ActionTransaction_NegativeOrdinaryActionFlipsFacing()
        {
            using CharacterScope scope = CreateScope(Frame(0), Frame(30));
            scope.Character.SwitchDir("right");

            NTSD28NativeActionAttempt result =
                scope.Writer.ApplyNativeInputAction(scope.Character, -30);

            Assert.That(result.Applied, Is.True);
            Assert.That(result.ResolvedAction, Is.EqualTo(30));
            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(30));
        }

        [Test]
        public void ComboRoute_ReapplyClearsCounterAndConsumesExactAttemptBoundary()
        {
            LF2FrameData action = Frame(360);
            action.hit_aj = 360;
            using CharacterScope scope = CreateScope(Frame(0), action);
            SetCurrentFrame(scope.Character, 360);
            scope.Character.Runtime.AttackingCounter = 9;
            FillAttemptBoundary(scope.Character.Runtime);
            scope.Character.Runtime.NativeInputProxy.ComboState[6] = 1;

            NTSD28NativeComboActionResult result =
                scope.Writer.RouteNativeComboAction(scope.Character);

            Assert.That(result.Selected, Is.True);
            Assert.That(result.Attempted, Is.True);
            Assert.That(result.Applied, Is.True);
            Assert.That(result.Consumed, Is.True);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(360));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);
            AssertAttemptBoundaryCleared(scope.Character.Runtime);
        }

        [Test]
        public void ComboRoute_NonzeroRejectedConsumesButZeroFieldDoesNot()
        {
            LF2FrameData current = Frame(0);
            current.hit_aj = 20;
            using CharacterScope locked = CreateScope(current, Frame(20));
            locked.Character.Runtime.InputActionLock130 = 1;
            locked.Character.Runtime.AttackingCounter = 9;
            FillAttemptBoundary(locked.Character.Runtime);
            locked.Character.Runtime.NativeInputProxy.ComboState[6] = 1;

            NTSD28NativeComboActionResult rejected =
                locked.Writer.RouteNativeComboAction(locked.Character);

            Assert.That(rejected.Attempted, Is.True);
            Assert.That(rejected.Applied, Is.False);
            Assert.That(rejected.Consumed, Is.True);
            Assert.That(locked.Character.Frame.N, Is.Zero);
            Assert.That(locked.Character.Runtime.AttackingCounter, Is.EqualTo(9));
            AssertAttemptBoundaryCleared(locked.Character.Runtime);

            current = Frame(0);
            current.hit_aj = 0;
            using CharacterScope zero = CreateScope(current, Frame(20));
            zero.Character.Runtime.NativeInputProxy.ComboState[6] = 1;
            NTSD28NativeComboActionResult ignored =
                zero.Writer.RouteNativeComboAction(zero.Character);
            Assert.That(ignored.Selected, Is.True);
            Assert.That(ignored.Attempted, Is.False);
            Assert.That(ignored.Consumed, Is.False);
            Assert.That(zero.Character.Runtime.NativeInputProxy.ComboState[6],
                Is.EqualTo(1));
        }

        [Test]
        public void HorizontalCombo_WritesFacingOnlyForNonzeroField()
        {
            LF2FrameData current = Frame(0);
            current.hit_Fa = 20;
            using CharacterScope scope = CreateScope(current, Frame(20));
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.NativeInputProxy.ComboState[0] = 5;

            NTSD28NativeComboActionResult rejected =
                scope.Writer.RouteNativeComboAction(scope.Character);

            Assert.That(rejected.Consumed, Is.True);
            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("left"));

            current = Frame(0);
            current.hit_Fa = 0;
            using CharacterScope zero = CreateScope(current);
            zero.Character.SwitchDir("right");
            zero.Character.Runtime.NativeInputProxy.ComboState[0] = 5;
            zero.Writer.RouteNativeComboAction(zero.Character);
            Assert.That(zero.Character.Runtime.Dir, Is.EqualTo("right"));
        }

        [Test]
        public void HitJaSpecialFamily_Implements300FeatureAndLinkedTailGates()
        {
            LF2FrameData current = Frame(0);
            current.hit_ja = 300;
            using CharacterScope nativeSuccess = CreateScopeWithObjectId(
                6, current, Frame(300));
            nativeSuccess.Character.Health.HP = 200;
            nativeSuccess.Character.Runtime.AttackingCounter = 9;
            nativeSuccess.Character.Runtime.NativeInputProxy.ComboState[8] = 1;

            NTSD28NativeComboActionResult special =
                nativeSuccess.Writer.RouteNativeComboAction(nativeSuccess.Character);

            Assert.That(special.Consumed, Is.True);
            Assert.That(special.Applied, Is.False);
            Assert.That(nativeSuccess.Character.Frame.N, Is.Zero);
            Assert.That(nativeSuccess.Character.Runtime.AttackingCounter, Is.Zero);
            Assert.That(nativeSuccess.Character.Runtime.InputLastAction144, Is.Zero);

            current = Frame(0);
            current.hit_ja = 300;
            using CharacterScope feature = CreateScopeWithObjectId(6, current, Frame(300));
            feature.Character.Health.HP = 200;
            feature.Character.Runtime.FeatureGate4A8428 = true;
            feature.Character.Runtime.NativeInputProxy.ComboState[8] = 1;
            Assert.That(feature.Writer.RouteNativeComboAction(feature.Character).Applied,
                Is.True);
            Assert.That(feature.Character.Frame.N, Is.EqualTo(300));

            current = Frame(0);
            current.hit_ja = 360;
            using CharacterScope rejected = CreateScopeWithObjectId(6, current, Frame(360));
            rejected.Character.Runtime.InputLinkedDefinitionId324 = 99;
            rejected.Character.Runtime.InputSpecialGate194 = 1;
            rejected.Character.Runtime.Unk338 = 900;
            rejected.Character.Runtime.NativeInputProxy.ComboState[8] = 1;
            NTSD28NativeComboActionResult rejectedResult =
                rejected.Writer.RouteNativeComboAction(rejected.Character);
            Assert.That(rejectedResult.Consumed, Is.False);
            Assert.That(rejected.Character.Runtime.Unk338, Is.EqualTo(900));
            Assert.That(rejected.Character.Runtime.NativeInputProxy.ComboState[8],
                Is.EqualTo(1));

            current = Frame(0);
            current.hit_ja = 360;
            using CharacterScope consumed = CreateScopeWithObjectId(6, current, Frame(360));
            consumed.Character.Runtime.InputLinkedDefinitionId324 = 99;
            consumed.Character.Runtime.InputSpecialGate194 = 0;
            consumed.Character.Runtime.Unk328 = 1;
            consumed.Character.Runtime.Unk338 = 900;
            consumed.Character.Runtime.AttackingCounter = 8;
            consumed.Character.Runtime.NativeInputProxy.ComboState[8] = 1;
            NTSD28NativeComboActionResult consumedResult =
                consumed.Writer.RouteNativeComboAction(consumed.Character);
            Assert.That(consumedResult.Consumed, Is.True);
            Assert.That(consumed.Character.Runtime.Unk338, Is.Zero);
            Assert.That(consumed.Character.Runtime.AttackingCounter, Is.Zero);

            current = Frame(0);
            current.hit_ja = 0;
            using CharacterScope zeroTail = CreateScopeWithObjectId(6, current);
            zeroTail.Character.Runtime.Unk328 = 1;
            zeroTail.Character.Runtime.Unk338 = 901;
            zeroTail.Character.Runtime.AttackingCounter = 7;
            zeroTail.Character.Runtime.NativeInputProxy.ComboState[8] = 1;
            NTSD28NativeComboActionResult zeroTailResult =
                zeroTail.Writer.RouteNativeComboAction(zeroTail.Character);
            Assert.That(zeroTailResult.Attempted, Is.True);
            Assert.That(zeroTailResult.Applied, Is.False);
            Assert.That(zeroTailResult.Consumed, Is.True);
            Assert.That(zeroTail.Character.Runtime.Unk338, Is.Zero);
            Assert.That(zeroTail.Character.Runtime.AttackingCounter, Is.Zero);

            current = Frame(0);
            current.hit_ja = 360;
            using CharacterScope alias = CreateScopeWithObjectId(777, current, Frame(360));
            alias.Character.FrameCache.Wrapper.characterData.use_ai = 6;
            alias.Character.Runtime.InputLinkedDefinitionId324 = 99;
            alias.Character.Runtime.InputSpecialGate194 = 1;
            alias.Character.Runtime.NativeInputProxy.ComboState[8] = 1;
            Assert.That(alias.Writer.RouteNativeComboAction(alias.Character).Consumed,
                Is.False);
        }

        [Test]
        public void DataOrientedSecondPass_RoutesExactComboOnce_LegacyDoesNot()
        {
            LF2FrameData current = Frame(0);
            current.hit_aj = 20;
            using CharacterScope exact = CreateScope(current, Frame(20));
            exact.Character.Runtime.NativeInputProxy.ComboState[6] = 1;
            exact.Character.Runtime.AttackingCounter = 7;

            exact.World.CharacterInputAll(2);

            Assert.That(exact.Character.Frame.N, Is.EqualTo(20));
            Assert.That(exact.Character.Runtime.InputLastAction144, Is.EqualTo(20));
            Assert.That(exact.Character.Runtime.AttackingCounter, Is.Zero);
            Assert.That(exact.Character.Runtime.NativeInputProxy.ComboState,
                Is.All.Zero);

            current = Frame(0);
            current.hit_aj = 20;
            using CharacterScope legacy = CreateScope(
                BattleAiExecutionProfile.LegacyCanonical,
                803,
                current,
                Frame(20));
            legacy.Character.Runtime.NativeInputProxy.ComboState[6] = 1;

            legacy.World.CharacterInputAll(2);

            Assert.That(legacy.Character.Frame.N, Is.Zero);
            Assert.That(legacy.Character.Runtime.NativeInputProxy.ComboState[6],
                Is.EqualTo(1));
        }

        [Test]
        public void DataOrientedSecondPass_DoesNotReenterLegacyComboAfterNativeNonConsumption()
        {
            LF2FrameData zeroField = Frame(0);
            zeroField.hit_aj = 0;
            using CharacterScope zero = CreateScope(zeroField, Frame(20));
            zero.Character.Runtime.NativeInputProxy.ComboState[6] = 1;

            zero.World.CharacterInputAll(2);

            Assert.That(zero.Character.Frame.N, Is.Zero);
            Assert.That(zero.Character.Runtime.NativeInputProxy.ComboState[6],
                Is.EqualTo(1));
            Assert.That(zero.Character.Runtime.ComboDra, Is.Zero);
            Assert.That(zero.Character.Runtime.ComboDja, Is.Zero);

            LF2FrameData specialField = Frame(0);
            specialField.hit_ja = 360;
            using CharacterScope rejected = CreateScopeWithObjectId(
                6,
                specialField,
                Frame(360));
            rejected.Character.Runtime.InputLinkedDefinitionId324 = 99;
            rejected.Character.Runtime.InputSpecialGate194 = 1;
            rejected.Character.Runtime.NativeInputProxy.ComboState[8] = 1;

            rejected.World.CharacterInputAll(2);

            Assert.That(rejected.Character.Frame.N, Is.Zero);
            Assert.That(rejected.Character.Runtime.NativeInputProxy.ComboState[8],
                Is.EqualTo(1));
            Assert.That(rejected.Character.Runtime.ComboDja, Is.EqualTo(3));
        }

        [Test]
        public void WarmPreprocessTransactionAndComboRoute_AllocateZeroManagedBytes()
        {
            LF2FrameData current = Frame(0);
            current.hit_aj = 0;
            using CharacterScope scope = CreateScope(current);
            NTSDEntityRuntime runtime = scope.Character.Runtime;
            runtime.InputRemapState138 = 1;
            NTSD28NativeInputPreprocessor.ApplyCurrentButtonRemap(runtime);
            scope.Writer.ApplyNativeInputAction(scope.Character, 0);
            runtime.NativeInputProxy.ComboState[6] = 1;
            scope.Writer.RouteNativeComboAction(scope.Character);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool valid = true;
            for (int index = 0; index < 4096; index++)
            {
                valid &= NTSD28NativeInputPreprocessor.ApplyCurrentButtonRemap(runtime) ==
                         NTSD28NativeInputRemapResult.Applied;
                valid &= scope.Writer.ApplyNativeInputAction(scope.Character, 0).Applied;
                runtime.NativeInputProxy.ComboState[6] = 1;
                valid &= !scope.Writer.RouteNativeComboAction(scope.Character).Consumed;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(valid, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static LF2FrameData Frame(
            int frameId,
            int state = 0,
            int mp = 0,
            int hp = 0)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
                mp = mp,
                hp = hp,
            };
        }

        private static CharacterScope CreateScope(params LF2FrameData[] frames)
        {
            return CreateScope(
                BattleAiExecutionProfile.DataOrientedCanonical,
                803,
                frames);
        }

        private static CharacterScope CreateScopeWithObjectId(
            int objectId,
            params LF2FrameData[] frames)
        {
            return CreateScope(
                BattleAiExecutionProfile.DataOrientedCanonical,
                objectId,
                frames);
        }

        private static CharacterScope CreateScope(
            BattleAiExecutionProfile profile,
            int objectId,
            params LF2FrameData[] frames)
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(profile);
            world.Runtime.Flow.InputPhase = 0;
            var data = new LF2CharacterData
            {
                name = $"NativeAction_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>(frames),
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            SetCurrentFrame(character, frames[0].frameId);
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(0);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Controller = new EmptyController();
            character.AiControlled = false;
            world.Register(character);
            return new CharacterScope(world, character);
        }

        private static void SetCurrentFrame(LF2Character character, int frameId)
        {
            character.WriteCurrentFrameId(frameId);
            character.Frame.D = character.FrameCache.GetFrameDataById(frameId);
            character.Frame.PN = frameId;
            character.Runtime.NextFrame = character.Frame.D?.next ?? 0;
        }

        private static void FillAttemptBoundary(NTSDEntityRuntime runtime)
        {
            for (int index = 0; index < 7; index++)
            {
                runtime.NativeInputProxy.EdgeWindow[index] = (byte)(10 + index);
                runtime.NativeInputProxy.Previous[index] = 1;
                runtime.NativeInputProxy.Current[index] = 1;
            }
            runtime.NativeInputProxy.DefendReentryCooldown = 9;
            for (int index = 1; index < runtime.InputHistory.Length; index++)
                runtime.InputHistory[index] = index;
        }

        private static void AssertAttemptBoundaryCleared(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.NativeInputProxy.EdgeWindow.Take(6), Is.All.Zero);
            Assert.That(runtime.NativeInputProxy.EdgeWindow[6], Is.EqualTo(16));
            Assert.That(runtime.NativeInputProxy.DefendReentryCooldown, Is.Zero);
            Assert.That(runtime.NativeInputProxy.ComboState, Is.All.Zero);
            Assert.That(runtime.InputHistory.Skip(1), Is.All.EqualTo(-1));
            Assert.That(runtime.NativeInputProxy.Current, Is.All.EqualTo((byte)1));
            Assert.That(runtime.NativeInputProxy.Previous, Is.All.EqualTo((byte)1));
        }

        private sealed class CharacterScope : IDisposable
        {
            internal CharacterScope(SimulationWorld world, LF2Character character)
            {
                World = world;
                Character = character;
            }

            internal SimulationWorld World { get; }
            internal LF2Character Character { get; }
            internal BattleCharacterActionWriter Writer => World.CharacterActionWriter;

            public void Dispose()
            {
                World.Unregister(Character);
            }
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
    }
}
#endif
