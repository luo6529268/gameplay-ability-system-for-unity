#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    [Category("NTSD28_B6_HeldInjury")]
    public sealed class NTSD28B6HeldInjuryCaughtActEventProductionEditorTests
    {
        [Test]
        public void AppliedEvent_ProducesOnlyAfterCompleteSettlementAndOnlyOnce()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 30,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out TypedCharacter victim);
            var observer = new SettlementObserver(catcher);
            Register(
                world,
                observer,
                "B6CaughtActSettlementObserver",
                9750,
                2,
                new List<LF2FrameData> { Frame(0, LF2States.Standing, null) },
                0);
            EnableCombo(world, facing: 1);

            world.PreInteractionTickAll(0);

            Assert.That(observer.ObservedBeforeConsumer, Is.True);
            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.EqualTo(1));
            Assert.That(catcher.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(1UL));
            Assert.That(victim.Health.HP, Is.EqualTo(70));

            world.PreInteractionTickAll(1);

            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.EqualTo(1),
                "frame-counter/AttackingCounter gate must suppress a second event");
            Assert.That(victim.Health.HP, Is.EqualTo(70));
        }

        [TestCase(false, 1, 1)]
        [TestCase(true, 0, 1)]
        [TestCase(true, 1, 0)]
        [TestCase(true, 1, 2)]
        public void FormalTupleGate_RequiresRecordBoundOneAndCaughtActOne(
            bool recordPresent,
            int bound,
            int caughtAct)
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 30,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out TypedCharacter victim);
            world.Runtime.NativeCombo.RestoreForSnapshot(
                recordPresent,
                bound,
                1,
                50,
                caughtAct);

            world.PreInteractionTickAll(0);

            Assert.That(victim.Health.HP, Is.EqualTo(70),
                "held accounting remains independent of the combo tuple");
            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [Test]
        public void FacingReverse_CreditsCaughtTypeZeroEntity()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 30,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out TypedCharacter victim);
            EnableCombo(world, facing: 0);

            world.PreInteractionTickAll(0);

            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(victim.Runtime.NativeComboHitCount1E0, Is.EqualTo(1));
            Assert.That(victim.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(1UL));
        }

        [Test]
        public void NonTypeZeroCatcher_CreditsExactlyOneLiveOwnerHop()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 30,
                LF2ObjectType.SpecialAttack,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out _);
            TypedCharacter directOwner = CreateNeutral(
                world,
                2,
                LF2ObjectType.Other,
                "B6CaughtActDirectOwner");
            TypedCharacter secondOwner = CreateNeutral(
                world,
                3,
                LF2ObjectType.Character,
                "B6CaughtActSecondOwner");
            catcher.Runtime.OwnerSlotIndex = 2;
            directOwner.Runtime.OwnerSlotIndex = 3;
            EnableCombo(world, facing: 1);

            world.PreInteractionTickAll(0);

            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(directOwner.Runtime.NativeComboHitCount1E0, Is.EqualTo(1));
            Assert.That(secondOwner.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [Test]
        public void NonTypeZeroCaught_RejectsEventBeforeFacingSelection()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 30,
                LF2ObjectType.Character,
                LF2ObjectType.LightWeapon,
                out TypedCharacter catcher,
                out TypedCharacter caught);
            EnableCombo(world, facing: 1);

            world.PreInteractionTickAll(0);

            Assert.That(caught.Health.HP, Is.EqualTo(70));
            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.Zero);
            Assert.That(caught.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [TestCase(0, 0)]
        [TestCase(-15, 0)]
        [TestCase(30, 1)]
        public void NonAppliedInjury_DoesNotCreateEvent(
            int injury,
            int initialAttackingCounter)
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out TypedCharacter victim);
            catcher.AttackingCounter = initialAttackingCounter;
            EnableCombo(world, facing: 1);

            world.PreInteractionTickAll(0);

            Assert.That(victim.Health.HP, Is.EqualTo(100));
            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [Test]
        public void InvalidVactionPreflight_DoesNotCreateEvent()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 30,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out TypedCharacter victim,
                hurtable: 0,
                victimAction: 999);
            EnableCombo(world, facing: 1);

            world.PreInteractionTickAll(0);

            Assert.That(victim.Frame.N, Is.EqualTo(999));
            Assert.That(victim.Health.HP, Is.EqualTo(100));
            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [Test]
        public void DirectSettlementWriter_DoesNotCrossPipelineEventBoundary()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 30,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out TypedCharacter victim);
            EnableCombo(world, facing: 1);

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(victim.Health.HP, Is.EqualTo(70));
            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.Zero);
        }

        [Test]
        public void TwoAppliedEvents_AreBothConsumedAfterSettlement()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 10,
                LF2ObjectType.SpecialAttack,
                LF2ObjectType.Character,
                out TypedCharacter firstCatcher,
                out _);
            CreatePair(
                world,
                2,
                injury: 20,
                LF2ObjectType.SpecialAttack,
                LF2ObjectType.Character,
                out TypedCharacter secondCatcher,
                out _);
            TypedCharacter owner = CreateNeutral(
                world,
                4,
                LF2ObjectType.Character,
                "B6CaughtActSharedOwner");
            firstCatcher.Runtime.OwnerSlotIndex = 4;
            secondCatcher.Runtime.OwnerSlotIndex = 4;
            EnableCombo(world, facing: 1);

            world.PreInteractionTickAll(0);

            Assert.That(owner.Runtime.NativeComboHitCount1E0, Is.EqualTo(2));
            Assert.That(owner.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(1UL));
        }

        [Test]
        public void WarmedFullSettlementEventPass_AllocatesZeroManagedMemory()
        {
            SimulationWorld world = CreateWorld();
            CreatePair(
                world,
                0,
                injury: 1,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                out TypedCharacter catcher,
                out TypedCharacter victim);
            EnableCombo(world, facing: 1);
            for (int index = 0; index < 32; index++)
            {
                ResetWarmIteration(catcher, victim);
                world.PreInteractionTickAll(index);
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                ResetWarmIteration(catcher, victim);
                world.PreInteractionTickAll(index + 32);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(catcher.Runtime.NativeComboHitCount1E0, Is.EqualTo(4128));
        }

        private static void ResetWarmIteration(
            TypedCharacter catcher,
            TypedCharacter victim)
        {
            catcher.AttackingCounter = 0;
            catcher.FrameDelay = 0;
            victim.FrameDelay = 0;
            victim.Health.HP = 10000;
            victim.Health.HPBound = 10000;
        }

        private static SimulationWorld CreateWorld()
        {
            return new SimulationWorld
            {
                ForceLegacyPreInteractionForDiagnostics = true,
                ForceLegacyPreInteractionParticipantFilteringForDiagnostics = true,
            };
        }

        private static void EnableCombo(SimulationWorld world, int facing)
        {
            world.Runtime.NativeCombo.RestoreForSnapshot(
                true,
                1,
                facing,
                50,
                1);
        }

        private static void CreatePair(
            SimulationWorld world,
            int catcherSlot,
            int injury,
            LF2ObjectType catcherType,
            LF2ObjectType caughtType,
            out TypedCharacter catcher,
            out TypedCharacter victim,
            int hurtable = 2,
            int victimAction = 132)
        {
            catcher = new TypedCharacter(catcherType);
            Register(
                world,
                catcher,
                "B6CaughtActCatcher" + catcherSlot,
                9730 + catcherSlot,
                catcherSlot,
                new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, null),
                    Frame(
                        343,
                        LF2States.Catching,
                        new CatchPoint
                        {
                            kind = 1,
                            vaction = victimAction,
                            hurtable = hurtable,
                            injury = injury,
                            x = 17,
                            y = 23,
                        }),
                },
                343);
            victim = new TypedCharacter(caughtType);
            Register(
                world,
                victim,
                "B6CaughtActVictim" + (catcherSlot + 1),
                9731 + catcherSlot,
                catcherSlot + 1,
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
            catcher.Runtime.CaughtSlotIndex = catcherSlot + 1;
            victim.Runtime.CatchSourceSlot90 = catcherSlot;
            victim.Runtime.CatcherSlotIndex = catcherSlot;
            catcher.AttackingCounter = 0;
            catcher.FrameDelay = 0;
            victim.FrameDelay = 0;
            catcher.Runtime.OwnerSlotIndex = -1;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
        }

        private static TypedCharacter CreateNeutral(
            SimulationWorld world,
            int slot,
            LF2ObjectType type,
            string name)
        {
            var entity = new TypedCharacter(type);
            Register(
                world,
                entity,
                name,
                9760 + slot,
                slot,
                new List<LF2FrameData> { Frame(0, LF2States.Standing, null) },
                0);
            return entity;
        }

        private static void Register(
            SimulationWorld world,
            TypedCharacter entity,
            string name,
            int objectId,
            int slot,
            List<LF2FrameData> frames,
            int frameId)
        {
            entity.ModuleInitialize();
            entity.Name = name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = name,
                    type_sub = (int)entity.Type,
                    frames = frames,
                }));
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

        private class TypedCharacter : LF2Character
        {
            internal TypedCharacter(LF2ObjectType type)
            {
                Type = type;
            }

            internal LF2ObjectType Type { get; }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)Type;
            }
        }

        private sealed class SettlementObserver : TypedCharacter
        {
            private readonly TypedCharacter observed;

            internal SettlementObserver(TypedCharacter observed)
                : base(LF2ObjectType.Character)
            {
                this.observed = observed;
            }

            internal bool ObservedBeforeConsumer { get; private set; }

            public override void RunWeaponSyncHeldStep10()
            {
                ObservedBeforeConsumer =
                    observed.Runtime.NativeComboHitCount1E0 == 0;
                base.RunWeaponSyncHeldStep10();
            }
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B6HeldInjuryCaughtActEventRequestRunner :
        ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B6-HeldInjuryCaughtActEvent-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B6-HeldInjuryCaughtActEvent-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.NTSD28B6HeldInjuryCaughtActEventProductionEditorTests";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(4096);
        private static NTSD28B6HeldInjuryCaughtActEventRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B6HeldInjuryCaughtActEventRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null || EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }
            if (File.Exists(ResultPath)) File.Delete(ResultPath);
            File.Delete(RequestPath);
            activeCallbacks = new NTSD28B6HeldInjuryCaughtActEventRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            activeApi.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[] { FocusedTestClass },
            }) { runSynchronously = false });
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            FailureDetails.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string text = $"state={result.ResultState}\npassed={result.PassCount}\n" +
                $"failed={result.FailCount}\nskipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\nmessage={result.Message}\n" +
                FailureDetails;
            File.WriteAllText(ResultPath, text, new UTF8Encoding(false));
            activeApi.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result?.Test == null || result.Test.IsSuite || result.FailCount <= 0)
                return;
            FailureDetails.Append("--- failure ---\n");
            FailureDetails.Append("test=").Append(result.FullName).Append('\n');
            FailureDetails.Append("state=").Append(result.ResultState).Append('\n');
            FailureDetails.Append("message=").Append(result.Message).Append('\n');
            FailureDetails.Append("stack=").Append(result.StackTrace).Append('\n');
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
