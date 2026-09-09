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
    [Category("NTSD28_B6_CatchSettlement")]
    public sealed class NTSD28B6CatchSettlementVactionPreflightProductionEditorTests
    {
        [TestCase(517, 0)]
        [TestCase(133, 1)]
        [TestCase(134, 2)]
        public void InvalidPostVactionFrame_IsTerminalBeforeSettlementTail(
            int vaction,
            int targetVariant)
        {
            SimulationWorld world = CreatePair(
                vaction,
                hurtable: 0,
                injury: 30,
                targetVariant,
                out LF2Character catcher,
                out LF2Character caught);
            caught.Health.HP = 400;
            caught.Health.HPBound = 400;
            caught.Runtime.SetPosition(900, 901, 902);
            caught.Runtime.SyncIntegerPosition();
            catcher.AttackingCounter = 0;
            catcher.FrameDelay = 0;
            caught.FrameDelay = 0;

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(caught.Frame.N, Is.EqualTo(vaction));
            Assert.That(caught.Health.HP, Is.EqualTo(400));
            Assert.That(caught.Health.HPBound, Is.EqualTo(400));
            Assert.That(catcher.AttackingCounter, Is.Zero);
            Assert.That(catcher.FrameDelay, Is.Zero);
            Assert.That(caught.FrameDelay, Is.Zero);
            Assert.That(caught.Runtime.X, Is.EqualTo(900));
            Assert.That(caught.Runtime.Y, Is.EqualTo(901));
            Assert.That(caught.Runtime.Z, Is.EqualTo(902));
            Assert.That(catcher.Runtime.CaughtSlotIndex, Is.EqualTo(1));
            Assert.That(caught.Runtime.CatchSourceSlot90, Is.Zero);
        }

        [Test]
        public void NegativeVaction_CommitsAbsoluteActionAndFlipsFacingBeforeValidTail()
        {
            SimulationWorld world = CreatePair(
                -130,
                hurtable: 0,
                injury: 0,
                targetVariant: 3,
                out LF2Character catcher,
                out LF2Character caught);
            caught.SwitchDir("right");
            caught.Runtime.SetPosition(900, 901, 902);
            caught.Runtime.SyncIntegerPosition();

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(caught.Frame.N, Is.EqualTo(130));
            Assert.That(caught.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(caught.Runtime.X, Is.Not.EqualTo(900));
            Assert.That(caught.Runtime.Y, Is.Not.EqualTo(901));
            Assert.That(caught.Runtime.Z, Is.Not.EqualTo(902));
        }

        [Test]
        public void ZeroVaction_CommitsActionZeroBeforeValidTail()
        {
            SimulationWorld world = CreatePair(
                0,
                hurtable: 0,
                injury: 0,
                targetVariant: 4,
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.SetPosition(900, 901, 902);
            caught.Runtime.SyncIntegerPosition();

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(caught.Frame.N, Is.Zero);
            Assert.That(caught.Frame.D?.PrimaryCatchPoint.Kind, Is.EqualTo(2));
            Assert.That(caught.Runtime.X, Is.Not.EqualTo(900));
        }

        [Test]
        public void ValidKind2PostVaction_PreservesSettlementTail()
        {
            SimulationWorld world = CreatePair(
                130,
                hurtable: 0,
                injury: 0,
                targetVariant: 3,
                out LF2Character catcher,
                out LF2Character caught);
            caught.Runtime.SetPosition(900, 901, 902);
            caught.Runtime.SyncIntegerPosition();

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(caught.Frame.N, Is.EqualTo(130));
            Assert.That(caught.Frame.D?.PrimaryCatchPoint.Kind, Is.EqualTo(2));
            Assert.That(caught.Runtime.X, Is.Not.EqualTo(900));
            Assert.That(caught.Runtime.Y, Is.Not.EqualTo(901));
            Assert.That(caught.Runtime.Z, Is.Not.EqualTo(902));
        }

        [Test]
        public void HurtableGateNotTaken_DoesNotPreflightUnusedInvalidVaction()
        {
            SimulationWorld world = CreatePair(
                517,
                hurtable: 1,
                injury: 0,
                targetVariant: 0,
                out LF2Character catcher,
                out LF2Character caught);
            caught.FrameDelay = 5;
            caught.Runtime.SetPosition(900, 901, 902);
            caught.Runtime.SyncIntegerPosition();

            catcher.RunWeaponSyncHeldStep10();

            Assert.That(caught.Frame.N, Is.EqualTo(132));
            Assert.That(caught.Frame.D?.PrimaryCatchPoint.Kind, Is.EqualTo(2));
            Assert.That(caught.Runtime.X, Is.Not.EqualTo(900));
        }

        [Test]
        public void WarmedInvalidPostVactionPreflight_DoesNotAllocateManagedMemory()
        {
            SimulationWorld world = CreatePair(
                517,
                hurtable: 0,
                injury: 0,
                targetVariant: 0,
                out LF2Character catcher,
                out LF2Character caught);
            for (int index = 0; index < 32; index++)
            {
                caught.DirectWriteRawFramePreserveWaitCounter(132);
                catcher.RunWeaponSyncHeldStep10();
            }

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                caught.DirectWriteRawFramePreserveWaitCounter(132);
                catcher.RunWeaponSyncHeldStep10();
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(caught.Frame.N, Is.EqualTo(517));
        }

        private static SimulationWorld CreatePair(
            int vaction,
            int hurtable,
            int injury,
            int targetVariant,
            out LF2Character catcher,
            out LF2Character caught)
        {
            var world = new SimulationWorld();
            var catcherFrames = new List<LF2FrameData>
            {
                Frame(0, LF2States.Standing, null),
                Frame(
                    343,
                    LF2States.Catching,
                    new CatchPoint
                    {
                        kind = 1,
                        vaction = vaction,
                        hurtable = hurtable,
                        injury = injury,
                        x = 17,
                        y = 23,
                        cover = 0,
                    }),
            };
            var caughtFrames = new List<LF2FrameData>
            {
                Frame(
                    132,
                    LF2States.BeingCaught,
                    new CatchPoint { kind = 2, x = 5, y = 9 }),
            };
            if (targetVariant == 1)
            {
                caughtFrames.Add(Frame(133, LF2States.BeingCaught, null));
            }
            else if (targetVariant == 2)
            {
                caughtFrames.Add(Frame(
                    134,
                    LF2States.BeingCaught,
                    new CatchPoint { kind = 1, x = 6, y = 10 }));
            }
            else if (targetVariant == 3)
            {
                caughtFrames.Add(Frame(
                    130,
                    LF2States.BeingCaught,
                    new CatchPoint { kind = 2, x = 6, y = 10 }));
            }
            else if (targetVariant == 4)
            {
                caughtFrames.Add(Frame(
                    0,
                    LF2States.BeingCaught,
                    new CatchPoint { kind = 2, x = 7, y = 11 }));
            }

            catcher = CreateEntity(world, "B6SettlementCatcher", 9710, 0, catcherFrames, 343);
            caught = CreateEntity(world, "B6SettlementCaught", 9711, 1, caughtFrames, 132);
            catcher.Runtime.SetPosition(100, 200, 300);
            caught.Runtime.SetPosition(110, 210, 300);
            catcher.Runtime.SyncIntegerPosition();
            caught.Runtime.SyncIntegerPosition();
            catcher.Runtime.CaughtSlotIndex = 1;
            caught.Runtime.CatchSourceSlot90 = 0;
            caught.Runtime.CatcherSlotIndex = 0;
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

    [InitializeOnLoad]
    internal static class NTSD28B6CatchSettlementVactionPreflightPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B6-CatchSettlementVactionPreflight-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B6-CatchSettlementVactionPreflight-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B6CatchSettlementVactionPreflightPlayRunner()
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
                    new NTSD28B6CatchSettlementVactionPreflightProductionEditorTests();
                tests.InvalidPostVactionFrame_IsTerminalBeforeSettlementTail(517, 0);
                tests.InvalidPostVactionFrame_IsTerminalBeforeSettlementTail(133, 1);
                tests.InvalidPostVactionFrame_IsTerminalBeforeSettlementTail(134, 2);
                tests.NegativeVaction_CommitsAbsoluteActionAndFlipsFacingBeforeValidTail();
                tests.ZeroVaction_CommitsActionZeroBeforeValidTail();
                tests.ValidKind2PostVaction_PreservesSettlementTail();
                tests.HurtableGateNotTaken_DoesNotPreflightUnusedInvalidVaction();
                tests.WarmedInvalidPostVactionPreflight_DoesNotAllocateManagedMemory();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=8\n" +
                    "invalidTargets=3\n" +
                    "signedAndZero=exact\n" +
                    "validContinuation=preserved\n" +
                    "hurtableGate=preserved\n" +
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
