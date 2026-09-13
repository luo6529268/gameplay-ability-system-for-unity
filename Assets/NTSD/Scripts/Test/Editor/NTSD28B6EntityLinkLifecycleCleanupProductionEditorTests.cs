#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Text;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6EntityLinkLifecycleCleanupProductionEditorTests
    {
        [TestCase(0)]
        [TestCase(399)]
        public void ImmediateUnregister_ClearsHeldAndCatchReferencesAndPreservesExcludedFields(
            int removedSlot)
        {
            var world = new SimulationWorld();
            int firstObserverSlot = removedSlot == 0 ? 1 : 0;
            LF2Character removed = Register(world, removedSlot, 91000 + removedSlot);
            LF2Character holder = Register(world, firstObserverSlot, 92001);
            LF2Character heldChild = Register(world, firstObserverSlot + 1, 92002);
            LF2Character catcher = Register(world, firstObserverSlot + 2, 92003);
            LF2Character caughtPlain = Register(world, firstObserverSlot + 3, 92004);
            LF2Character caughtEncoded = Register(world, firstObserverSlot + 4, 92005);
            LF2Character unrelated = Register(world, firstObserverSlot + 5, 92006);

            holder.Runtime.LinkState = 7;
            holder.Runtime.TargetSlotIndex = removedSlot;
            holder.Runtime.HeldWeaponStableId = removedSlot;
            holder.Runtime.ThrowFrameGuard = 71;
            holder.HeldWeaponReferenceInternal = removed;
            SetExcludedSentinels(holder, 81);

            heldChild.Runtime.LinkState = -7;
            heldChild.Runtime.HolderStableId = removedSlot;
            SetExcludedSentinels(heldChild, 82);

            catcher.Runtime.CaughtSlotIndex = removedSlot;
            catcher.Runtime.CatcherSlotIndex = 91;
            catcher.Runtime.CatchSourceSlot90 = 92;
            catcher.Runtime.CaughtDuration = 93;
            catcher.Catching = removed;
            SetExcludedSentinels(catcher, 83);

            caughtPlain.Runtime.CatchSourceSlot90 = removedSlot;
            caughtPlain.Runtime.CatcherSlotIndex = removedSlot;
            caughtPlain.Runtime.CaughtDuration = 94;
            caughtPlain.Catching = removed;
            SetExcludedSentinels(caughtPlain, 84);

            caughtEncoded.Runtime.CatchSourceSlot90 = 0x2000 + removedSlot;
            caughtEncoded.Runtime.CatcherSlotIndex = 97;
            caughtEncoded.Runtime.CaughtDuration = 95;
            caughtEncoded.Catching = removed;
            SetExcludedSentinels(caughtEncoded, 85);

            unrelated.Runtime.LinkState = 5;
            unrelated.Runtime.TargetSlotIndex = 377;
            unrelated.Runtime.HeldWeaponStableId = 378;
            unrelated.Runtime.HolderStableId = 379;
            unrelated.Runtime.CaughtSlotIndex = 380;
            unrelated.Runtime.CatcherSlotIndex = 381;
            unrelated.Runtime.CatchSourceSlot90 = 382;
            unrelated.Runtime.CaughtDuration = 383;
            SetExcludedSentinels(unrelated, 86);

            world.Unregister(removed);

            Assert.That(removed.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(holder.Runtime.LinkState, Is.Zero);
            Assert.That(holder.Runtime.TargetSlotIndex, Is.Zero);
            Assert.That(holder.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
            Assert.That(holder.Runtime.ThrowFrameGuard, Is.EqualTo(-1));
            Assert.That(holder.HeldWeaponReferenceInternal, Is.Null);
            AssertExcludedSentinels(holder, 81);
            AssertRelationRow(world, holder, 0, 0);

            Assert.That(heldChild.Runtime.LinkState, Is.Zero);
            Assert.That(heldChild.Runtime.HolderStableId, Is.Zero);
            AssertExcludedSentinels(heldChild, 82);
            AssertRelationRow(world, heldChild, 0, -1);

            Assert.That(catcher.Runtime.CaughtSlotIndex, Is.EqualTo(-1));
            Assert.That(catcher.Runtime.CaughtDuration, Is.Zero);
            Assert.That(catcher.Catching, Is.Null);
            Assert.That(catcher.Runtime.CatcherSlotIndex, Is.EqualTo(91));
            Assert.That(catcher.Runtime.CatchSourceSlot90, Is.EqualTo(92));
            AssertExcludedSentinels(catcher, 83);

            Assert.That(caughtPlain.Runtime.CatchSourceSlot90, Is.EqualTo(-1));
            Assert.That(caughtPlain.Runtime.CatcherSlotIndex, Is.EqualTo(-1));
            Assert.That(caughtPlain.Runtime.CaughtDuration, Is.Zero);
            Assert.That(caughtPlain.Catching, Is.Null);
            AssertExcludedSentinels(caughtPlain, 84);

            Assert.That(caughtEncoded.Runtime.CatchSourceSlot90, Is.EqualTo(-1));
            Assert.That(caughtEncoded.Runtime.CatcherSlotIndex, Is.EqualTo(97));
            Assert.That(caughtEncoded.Runtime.CaughtDuration, Is.Zero);
            Assert.That(caughtEncoded.Catching, Is.Null);
            AssertExcludedSentinels(caughtEncoded, 85);

            Assert.That(unrelated.Runtime.LinkState, Is.EqualTo(5));
            Assert.That(unrelated.Runtime.TargetSlotIndex, Is.EqualTo(377));
            Assert.That(unrelated.Runtime.HeldWeaponStableId, Is.EqualTo(378));
            Assert.That(unrelated.Runtime.HolderStableId, Is.EqualTo(379));
            Assert.That(unrelated.Runtime.CaughtSlotIndex, Is.EqualTo(380));
            Assert.That(unrelated.Runtime.CatcherSlotIndex, Is.EqualTo(381));
            Assert.That(unrelated.Runtime.CatchSourceSlot90, Is.EqualTo(382));
            Assert.That(unrelated.Runtime.CaughtDuration, Is.EqualTo(383));
            AssertExcludedSentinels(unrelated, 86);
        }

        [Test]
        public void RejectedRelease_HasNoRelationshipSideEffects()
        {
            using var logging = new DisabledLoggingScope();
            var world = new SimulationWorld();
            LF2Character removed = Register(world, 2, 93001);
            LF2Character observer = Register(world, 3, 93002);
            observer.Runtime.LinkState = 4;
            observer.Runtime.TargetSlotIndex = 2;
            observer.Runtime.HeldWeaponStableId = 2;
            observer.Runtime.CaughtSlotIndex = 2;
            observer.Runtime.CatchSourceSlot90 = 2;
            observer.Runtime.CatcherSlotIndex = 2;
            observer.Runtime.CaughtDuration = 77;

            removed.Runtime.SlotIndex = 99;
            world.Unregister(removed);

            Assert.That(observer.Runtime.LinkState, Is.EqualTo(4));
            Assert.That(observer.Runtime.TargetSlotIndex, Is.EqualTo(2));
            Assert.That(observer.Runtime.HeldWeaponStableId, Is.EqualTo(2));
            Assert.That(observer.Runtime.CaughtSlotIndex, Is.EqualTo(2));
            Assert.That(observer.Runtime.CatchSourceSlot90, Is.EqualTo(2));
            Assert.That(observer.Runtime.CatcherSlotIndex, Is.EqualTo(2));
            Assert.That(observer.Runtime.CaughtDuration, Is.EqualTo(77));
            Assert.That(world.FindEntityByRuntimeSlotForQuery(2), Is.SameAs(removed));

            removed.Runtime.SlotIndex = 2;
            world.Unregister(removed);
        }

        [Test]
        public void SameSlotReuse_CannotRetargetStaleHeldOrCatchRelations()
        {
            var world = new SimulationWorld();
            LF2Character removed = Register(world, 2, 94001);
            LF2Character observer = Register(world, 3, 94002);
            observer.Runtime.LinkState = 4;
            observer.Runtime.TargetSlotIndex = 2;
            observer.Runtime.HeldWeaponStableId = 2;
            observer.Runtime.CaughtSlotIndex = 2;
            observer.Runtime.CatchSourceSlot90 = 0x2002;
            observer.Runtime.CatcherSlotIndex = 2;
            observer.Runtime.CaughtDuration = 77;

            Assert.That(
                world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                    2,
                    out RuntimeSlotTable.ReadOnlySlotView before),
                Is.True);
            world.Unregister(removed);
            LF2Character replacement = Register(world, 2, 94003);
            Assert.That(
                world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                    2,
                    out RuntimeSlotTable.ReadOnlySlotView after),
                Is.True);

            Assert.That(after.Generation, Is.GreaterThan(before.Generation));
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(2));
            Assert.That(observer.Runtime.LinkState, Is.Zero);
            Assert.That(observer.Runtime.TargetSlotIndex, Is.Zero);
            Assert.That(observer.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
            Assert.That(observer.Runtime.CaughtSlotIndex, Is.EqualTo(-1));
            Assert.That(observer.Runtime.CatchSourceSlot90, Is.EqualTo(-1));
            Assert.That(observer.Runtime.CatcherSlotIndex, Is.EqualTo(-1));
            Assert.That(observer.Runtime.CaughtDuration, Is.Zero);
        }

        [Test]
        public void PendingDestroyAdmission_CleansRelationsBeforeReusingLowestSlot()
        {
            var world = new SimulationWorld();
            LF2Character removed = Register(world, 50, 95001);
            LF2Character observer = Register(world, 51, 95002);
            observer.Runtime.LinkState = -3;
            observer.Runtime.HolderStableId = 50;
            observer.Runtime.CatchSourceSlot90 = 50;
            observer.Runtime.CatcherSlotIndex = 50;
            observer.Runtime.CaughtDuration = 18;
            removed.Runtime.PendingFlushDestroy = true;

            LF2Character replacement = Register(world, 50, 95003);

            Assert.That(removed.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(50));
            Assert.That(observer.Runtime.LinkState, Is.Zero);
            Assert.That(observer.Runtime.HolderStableId, Is.Zero);
            Assert.That(observer.Runtime.CatchSourceSlot90, Is.EqualTo(-1));
            Assert.That(observer.Runtime.CatcherSlotIndex, Is.EqualTo(-1));
            Assert.That(observer.Runtime.CaughtDuration, Is.Zero);
        }

        [Test]
        public void DeferredUnregister_CleansRelationsBeforePendingBucketRemoval()
        {
            var world = new SimulationWorld();
            LF2Character removed = Register(world, 8, 96001);
            LF2Character observer = Register(world, 9, 96002);
            observer.Runtime.LinkState = 3;
            observer.Runtime.TargetSlotIndex = 8;
            observer.Runtime.HeldWeaponStableId = 8;
            observer.Runtime.CaughtSlotIndex = 8;
            observer.Runtime.CaughtDuration = 27;

            world.BeginDeferredEntityMutationPass();
            try
            {
                world.Unregister(removed);

                Assert.That(removed.Runtime.SlotIndex, Is.EqualTo(-1));
                Assert.That(observer.Runtime.LinkState, Is.Zero);
                Assert.That(observer.Runtime.TargetSlotIndex, Is.Zero);
                Assert.That(observer.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
                Assert.That(observer.Runtime.CaughtSlotIndex, Is.EqualTo(-1));
                Assert.That(observer.Runtime.CaughtDuration, Is.Zero);
            }
            finally
            {
                world.EndDeferredEntityMutationPass();
            }
        }

        [Test]
        public void Warmed4096SlotCleanup_DoesNotAllocateManagedMemory()
        {
            var slots = new RuntimeSlotTable(4096);
            var observer = new LF2Character();
            Assert.That(slots.TryClaim(4095, observer, out _), Is.True);

            observer.Runtime.LinkState = 1;
            observer.Runtime.TargetSlotIndex = 7;
            BattleEntityLinkLifecycleWriter.ClearReferencesToReleasedSlot(
                slots,
                7);

            observer.Runtime.LinkState = 1;
            observer.Runtime.TargetSlotIndex = 7;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            BattleEntityLinkLifecycleWriter.ClearReferencesToReleasedSlot(
                slots,
                7);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(
                allocated,
                Is.Zero,
                "the warmed 4096-slot lifecycle cleanup must not allocate managed memory");
        }

        private static LF2Character Register(
            SimulationWorld world,
            int slot,
            int stableId)
        {
            var entity = new LF2Character();
            entity.Runtime.StableId = stableId;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(slot));
            return entity;
        }

        private static void SetExcludedSentinels(LF2Character entity, int seed)
        {
            entity.Runtime.Kind4SourceCount92 = seed + 100;
            entity.Runtime.OwnerSlotIndex = seed + 200;
            entity.Runtime.SpawnerSlotIndex = seed + 300;
            entity.Runtime.Vx = seed + 400;
            entity.Runtime.Vy = seed + 500;
            entity.Runtime.Vz = seed + 600;
        }

        private static void AssertExcludedSentinels(LF2Character entity, int seed)
        {
            Assert.That(typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0, Is.True);
            Assert.That(entity.Runtime.Kind4SourceCount92, Is.EqualTo(seed + 100));
            Assert.That(entity.Runtime.OwnerSlotIndex, Is.EqualTo(seed + 200));
            Assert.That(entity.Runtime.SpawnerSlotIndex, Is.EqualTo(seed + 300));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(seed + 400));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(seed + 500));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(seed + 600));
        }

        private static void AssertRelationRow(
            SimulationWorld world,
            LF2Character entity,
            int expectedLinkState,
            int expectedTargetSlot)
        {
            Assert.That(
                world.TryGetRelationLinkStateForDiagnostics(
                    entity,
                    out BattleRelationLinkStateView row),
                Is.True);
            Assert.That(row.LinkState, Is.EqualTo(expectedLinkState));
            Assert.That(row.TargetSlot, Is.EqualTo(expectedTargetSlot));
        }

        private sealed class DisabledLoggingScope : IDisposable
        {
            private readonly bool original;

            internal DisabledLoggingScope()
            {
                original = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
            }

            public void Dispose()
            {
                Debug.unityLogger.logEnabled = original;
            }
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28B6EntityLinkLifecycleCleanupPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B6-EntityLinkLifecycleCleanup-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B6-EntityLinkLifecycleCleanup-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B6EntityLinkLifecycleCleanupPlayRunner()
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
                    new NTSD28B6EntityLinkLifecycleCleanupProductionEditorTests();
                tests.ImmediateUnregister_ClearsHeldAndCatchReferencesAndPreservesExcludedFields(0);
                tests.ImmediateUnregister_ClearsHeldAndCatchReferencesAndPreservesExcludedFields(399);
                tests.RejectedRelease_HasNoRelationshipSideEffects();
                tests.SameSlotReuse_CannotRetargetStaleHeldOrCatchRelations();
                tests.PendingDestroyAdmission_CleansRelationsBeforeReusingLowestSlot();
                tests.DeferredUnregister_CleansRelationsBeforePendingBucketRemoval();
                tests.Warmed4096SlotCleanup_DoesNotAllocateManagedMemory();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=7\n" +
                    "immediateDeferredPendingReuse=exact\n" +
                    "releaseFailureSideEffects=none\n" +
                    "warmed4096AllocationBytes=0\n" +
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
