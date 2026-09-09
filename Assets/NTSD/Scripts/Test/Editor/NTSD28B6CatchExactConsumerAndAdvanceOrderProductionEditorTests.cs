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
using UnityEngine;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    [Category("NTSD28_B6_CatchAdvance")]
    public sealed class NTSD28B6CatchExactConsumerAndAdvanceOrderProductionEditorTests
    {
        [Test]
        public void MixedAdvance_LowCaughtBeforeHighThrowerRetainsVactionThisTick()
        {
            SimulationWorld world = CreateThrowPair(
                catcherSlot: 511,
                caughtSlot: 0,
                out LF2Character catcher,
                out LF2Character caught);

            world.PreInteractionTickAll(7001);

            Assert.That(catcher.Frame.N, Is.EqualTo(344));
            Assert.That(caught.Frame.N, Is.EqualTo(132));
        }

        [Test]
        public void MixedAdvance_LowThrowerBeforeHighCaughtFallsAsOrphanThisTick()
        {
            SimulationWorld world = CreateThrowPair(
                catcherSlot: 0,
                caughtSlot: 511,
                out LF2Character catcher,
                out LF2Character caught);

            world.PreInteractionTickAll(7002);

            Assert.That(catcher.Frame.N, Is.EqualTo(344));
            Assert.That(caught.Frame.N, Is.EqualTo(212));
            Assert.That(caught.Runtime.Vy, Is.EqualTo(-3));
        }

        [Test]
        public void Kind1Reciprocal_UsesExactSourceWhenCompatDisagrees()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint { kind = 1, decrease = 1, vaction = 132 },
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.CatchSourceSlot90 = 0;
            caught.Runtime.CatcherSlotIndex = 77;
            catcher.Runtime.CaughtDuration = 10;

            catcher.RunCpointCheckStep10();

            Assert.That(catcher.Frame.N, Is.EqualTo(343));
            Assert.That(catcher.Runtime.CaughtDuration, Is.EqualTo(9));
        }

        [Test]
        public void Kind2Validation_UsesExactSourceWhenCompatDisagrees()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint { kind = 1, vaction = 132 },
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.CatchSourceSlot90 = 0;
            caught.Runtime.CatcherSlotIndex = 77;

            caught.RunCpointMismatchTailStep10();

            Assert.That(caught.Frame.N, Is.EqualTo(132));
        }

        [Test]
        public void Kind2Validation_DoesNotDecodeAttributionTaggedExactSource()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint { kind = 1, vaction = 132 },
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.CatchSourceSlot90 = 0x2000;
            caught.Runtime.CatcherSlotIndex = 0;

            caught.RunCpointMismatchTailStep10();

            Assert.That(caught.Frame.N, Is.EqualTo(212));
            Assert.That(caught.Runtime.Vy, Is.EqualTo(-3));
        }

        [Test]
        public void Settlement_UsesExactSourceWhenCompatDisagrees()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint
                {
                    kind = 1,
                    vaction = 132,
                    hurtable = 2,
                    x = 17,
                    y = 23,
                    cover = 11,
                },
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.CatchSourceSlot90 = 0;
            caught.Runtime.CatcherSlotIndex = 77;
            caught.Runtime.SetPosition(900, 901, 902);
            caught.Runtime.SyncIntegerPosition();

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(caught.Runtime.X, Is.Not.EqualTo(900));
            Assert.That(caught.Runtime.Y, Is.Not.EqualTo(901));
            Assert.That(caught.Runtime.Z, Is.Not.EqualTo(902));
        }

        [Test]
        public void ReciprocalMismatch_IsTerminalBeforeThrowAndDirControl()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint
                {
                    kind = 1,
                    throwvx = 5,
                    throwvy = -7,
                    throwvz = 9,
                    vaction = 132,
                    dircontrol = 1,
                    x = 17,
                    y = 23,
                },
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.CatchSourceSlot90 = 77;
            caught.Runtime.CatcherSlotIndex = 77;
            catcher.AttackingCounter = 2;
            catcher.Runtime.KeyRight = 1;
            catcher.SwitchDir("left");
            caught.Runtime.SetVelocity(31, 32, 33);
            double caughtX = caught.Runtime.X;
            double caughtY = caught.Runtime.Y;

            catcher.RunCpointCheckStep10();

            Assert.That(catcher.Frame.N, Is.Zero);
            Assert.That(catcher.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(catcher.AttackingCounter, Is.EqualTo(2));
            Assert.That(caught.Frame.N, Is.EqualTo(132));
            Assert.That(caught.Runtime.X, Is.EqualTo(caughtX));
            Assert.That(caught.Runtime.Y, Is.EqualTo(caughtY));
            Assert.That(caught.Runtime.Vx, Is.EqualTo(31));
            Assert.That(caught.Runtime.Vy, Is.EqualTo(32));
            Assert.That(caught.Runtime.Vz, Is.EqualTo(33));
        }

        [Test]
        public void NegativeDecreaseRelease_UsesFrameCounterCarrierAndIsTerminal()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint
                {
                    kind = 1,
                    decrease = -5,
                    throwvx = 9,
                    throwvy = -8,
                    throwvz = 7,
                    vaction = 132,
                    dircontrol = 1,
                    x = 17,
                    y = 23,
                },
                out LF2Character catcher,
                out LF2Character caught);
            catcher.Runtime.CaughtDuration = 1;
            catcher.AttackingCounter = 2;
            caught.AttackingCounter = 3;
            catcher.HitCount = 11;
            caught.HitCount = 12;
            catcher.Runtime.KeyRight = 1;
            catcher.SwitchDir("left");
            catcher.Runtime.SetPosition(0, 0, 0);
            caught.Runtime.SetPosition(10, 0, 0);
            catcher.Runtime.SyncIntegerPosition();
            caught.Runtime.SyncIntegerPosition();
            ulong rngCalls = world.Rng.CallCount;

            catcher.RunCpointCheckStep10();

            Assert.That(catcher.Runtime.CaughtDuration, Is.EqualTo(-4));
            Assert.That(catcher.Frame.N, Is.Zero);
            Assert.That(caught.Frame.N, Is.EqualTo(181));
            Assert.That(catcher.AttackingCounter, Is.EqualTo(1));
            Assert.That(caught.AttackingCounter, Is.EqualTo(1));
            Assert.That(catcher.HitCount, Is.EqualTo(11));
            Assert.That(caught.HitCount, Is.EqualTo(12));
            Assert.That(caught.Runtime.Vx, Is.EqualTo(4));
            Assert.That(caught.Runtime.Vy, Is.EqualTo(-3));
            Assert.That(catcher.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(catcher.Runtime.CaughtSlotIndex, Is.EqualTo(1));
            Assert.That(caught.Runtime.CatchSourceSlot90, Is.Zero);
            Assert.That(world.Rng.CallCount, Is.EqualTo(rngCalls));
        }

        [TestCase(0, 10)]
        [TestCase(2, 8)]
        [TestCase(-2, 8)]
        public void NonTerminalDecrease_PreservesNormalContinuation(
            int decrease,
            int expectedTimeout)
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint { kind = 1, decrease = decrease, vaction = 132 },
                out LF2Character catcher,
                out LF2Character caught);
            catcher.Runtime.CaughtDuration = 10;

            catcher.RunCpointCheckStep10();

            Assert.That(catcher.Runtime.CaughtDuration, Is.EqualTo(expectedTimeout));
            Assert.That(catcher.Frame.N, Is.EqualTo(343));
            Assert.That(caught.Frame.N, Is.EqualTo(132));
            Assert.That(catcher.AttackingCounter, Is.Zero);
            Assert.That(caught.AttackingCounter, Is.Zero);
        }

        [Test]
        public void SuccessfulExactReciprocal_PreservesNormalThrowPath()
        {
            SimulationWorld world = CreateThrowPair(
                0,
                1,
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.Vz = 37;

            catcher.RunCpointCheckStep10();

            Assert.That(catcher.Frame.N, Is.EqualTo(344));
            Assert.That(caught.Frame.N, Is.EqualTo(132));
            Assert.That(caught.Runtime.Vx, Is.EqualTo(5));
            Assert.That(caught.Runtime.Vy, Is.EqualTo(-6));
            Assert.That(caught.Runtime.Vz, Is.EqualTo(37));
        }

        [Test]
        public void SuccessfulExactReciprocal_PreservesNormalDirControlPath()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint { kind = 1, dircontrol = 1, vaction = 132 },
                out LF2Character catcher,
                out LF2Character caught);
            catcher.AttackingCounter = 2;
            catcher.Runtime.KeyRight = 1;
            catcher.SwitchDir("left");

            catcher.RunCpointCheckStep10();

            Assert.That(catcher.Runtime.Dir, Is.EqualTo("right"));
            Assert.That(catcher.Frame.N, Is.EqualTo(343));
            Assert.That(caught.Frame.N, Is.EqualTo(132));
        }

        [Test]
        public void SuccessfulExactReciprocal_PreservesInputSelectedKind1Transition()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint
                {
                    kind = 1,
                    aaction = 350,
                    vaction = 132,
                },
                out LF2Character catcher,
                out LF2Character caught);
            catcher.Runtime.KeyJump = 1;
            catcher.Runtime.CdAttack = 5;

            catcher.RunCpointCheckStep10();

            Assert.That(catcher.Frame.N, Is.EqualTo(350));
            Assert.That(caught.Frame.N, Is.EqualTo(133));
            Assert.That(catcher.AttackingCounter, Is.Zero);
            Assert.That(caught.AttackingCounter, Is.Zero);
        }

        [Test]
        public void LifecycleCleanup_PreventsSameSlotCatchSourceAba()
        {
            SimulationWorld world = CreatePair(
                0,
                1,
                new CatchPoint { kind = 1, vaction = 132 },
                out LF2Character catcher,
                out LF2Character caught);
            world.Unregister(catcher);
            LF2Character replacement = CreateEntity(
                world,
                "B6AdvanceReplacement",
                9702,
                0,
                new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, null, 0),
                },
                0);

            caught.RunCpointMismatchTailStep10();

            Assert.That(replacement.Runtime.SlotIndex, Is.Zero);
            Assert.That(caught.Runtime.CatchSourceSlot90, Is.EqualTo(-1));
            Assert.That(caught.Runtime.CatcherSlotIndex, Is.EqualTo(-1));
            Assert.That(caught.Frame.N, Is.EqualTo(212));
        }

        [Test]
        public void WarmedMixedAdvanceAndSettlement_DoNotAllocateManagedMemory()
        {
            SimulationWorld world = CreatePair(
                0,
                511,
                new CatchPoint
                {
                    kind = 1,
                    vaction = 132,
                    hurtable = 2,
                    x = 17,
                    y = 23,
                },
                out LF2Character catcher,
                out LF2Character caught);
            for (int index = 0; index < 32; index++)
                world.PreInteractionTickAll(8000 + index);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 512; index++)
                world.PreInteractionTickAll(9000 + index);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(catcher.Frame.N, Is.EqualTo(343));
            Assert.That(caught.Frame.N, Is.EqualTo(132));
        }

        private static SimulationWorld CreateThrowPair(
            int catcherSlot,
            int caughtSlot,
            out LF2Character catcher,
            out LF2Character caught)
        {
            return CreatePair(
                catcherSlot,
                caughtSlot,
                new CatchPoint
                {
                    kind = 1,
                    decrease = 0,
                    throwvx = 5,
                    throwvy = -6,
                    vaction = 132,
                    x = 17,
                    y = 23,
                },
                out catcher,
                out caught);
        }

        private static SimulationWorld CreatePair(
            int catcherSlot,
            int caughtSlot,
            CatchPoint catcherPoint,
            out LF2Character catcher,
            out LF2Character caught)
        {
            var world = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 512);
            LF2FrameData catcher0 = Frame(0, LF2States.Standing, null, 0);
            LF2FrameData catcher343 = Frame(343, LF2States.Catching, catcherPoint, 344);
            LF2FrameData catcher344 = Frame(344, LF2States.Standing, null, 344);
            LF2FrameData catcher350 = Frame(
                350,
                LF2States.Catching,
                new CatchPoint { kind = 1, vaction = 133 },
                350);
            LF2FrameData caught0 = Frame(0, LF2States.Standing, null, 0);
            LF2FrameData caught132 = Frame(
                132,
                LF2States.BeingCaught,
                new CatchPoint { kind = 2, x = 5, y = 9 },
                132);
            LF2FrameData caught133 = Frame(
                133,
                LF2States.BeingCaught,
                new CatchPoint { kind = 2, x = 6, y = 10 },
                133);
            LF2FrameData caught181 = Frame(181, LF2States.Falling, null, 181);
            LF2FrameData caught212 = Frame(212, LF2States.Falling, null, 212);
            catcher = CreateEntity(
                world,
                "B6AdvanceCatcher",
                9700,
                catcherSlot,
                new List<LF2FrameData>
                {
                    catcher0,
                    catcher343,
                    catcher344,
                    catcher350,
                },
                343);
            caught = CreateEntity(
                world,
                "B6AdvanceCaught",
                9701,
                caughtSlot,
                new List<LF2FrameData>
                {
                    caught0,
                    caught132,
                    caught133,
                    caught181,
                    caught212,
                },
                132);
            catcher.Runtime.SetPosition(0, 10, 3);
            caught.Runtime.SetPosition(10, 20, 3);
            catcher.Runtime.SyncIntegerPosition();
            caught.Runtime.SyncIntegerPosition();
            catcher.Runtime.CaughtSlotIndex = caughtSlot;
            caught.Runtime.CatchSourceSlot90 = catcherSlot;
            caught.Runtime.CatcherSlotIndex = catcherSlot;
            catcher.Runtime.CaughtDuration = 300;
            return world;
        }

        private static LF2Character CreateEntity(
            SimulationWorld world,
            string name,
            int objectId,
            int slot,
            List<LF2FrameData> frames,
            int frameId)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = frames,
            };
            var entity = new LF2Character();
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

        private static LF2FrameData Frame(
            int id,
            int state,
            CatchPoint cpoint,
            int next)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 1,
                next = next,
                centerx = 39,
                centery = 79,
                cpoint = cpoint,
            };
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28B6CatchExactConsumerAndAdvanceOrderPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B6-CatchExactConsumerAdvance-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B6-CatchExactConsumerAdvance-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B6CatchExactConsumerAndAdvanceOrderPlayRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (running || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || !File.Exists(RequestPath))
            {
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            running = true;
            File.Delete(RequestPath);
            if (File.Exists(ResultPath)) File.Delete(ResultPath);
            try
            {
                var tests =
                    new NTSD28B6CatchExactConsumerAndAdvanceOrderProductionEditorTests();
                tests.MixedAdvance_LowCaughtBeforeHighThrowerRetainsVactionThisTick();
                tests.MixedAdvance_LowThrowerBeforeHighCaughtFallsAsOrphanThisTick();
                tests.Kind1Reciprocal_UsesExactSourceWhenCompatDisagrees();
                tests.Kind2Validation_UsesExactSourceWhenCompatDisagrees();
                tests.Kind2Validation_DoesNotDecodeAttributionTaggedExactSource();
                tests.Settlement_UsesExactSourceWhenCompatDisagrees();
                tests.ReciprocalMismatch_IsTerminalBeforeThrowAndDirControl();
                tests.NegativeDecreaseRelease_UsesFrameCounterCarrierAndIsTerminal();
                tests.NonTerminalDecrease_PreservesNormalContinuation(0, 10);
                tests.NonTerminalDecrease_PreservesNormalContinuation(2, 8);
                tests.NonTerminalDecrease_PreservesNormalContinuation(-2, 8);
                tests.SuccessfulExactReciprocal_PreservesNormalThrowPath();
                tests.SuccessfulExactReciprocal_PreservesNormalDirControlPath();
                tests.SuccessfulExactReciprocal_PreservesInputSelectedKind1Transition();
                tests.LifecycleCleanup_PreventsSameSlotCatchSourceAba();
                tests.WarmedMixedAdvanceAndSettlement_DoNotAllocateManagedMemory();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=16\n" +
                    "mixedSlotPolarity=exact\n" +
                    "exactConsumers=3\n" +
                    "terminalFences=2\n" +
                    "normalContinuation=preserved\n" +
                    "sameSlotAba=blocked\n" +
                    "warmedAllocationBytes=0\n" +
                    "sceneMutation=none\n",
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    ResultPath,
                    "state=Failed\n" + exception,
                    new UTF8Encoding(false));
            }
            finally
            {
                EditorApplication.delayCall += ExitPlayMode;
            }
        }

        private static void ExitPlayMode()
        {
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
            running = false;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
