#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Text;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class SimulationQueryAndLinkModuleEditorTests
    {
        [Test]
        public void HeldObjectProcess_OutOfRangeNegativeHolderRetainsHolderSlotAcrossBothPasses()
        {
            var world = new SimulationWorld();
            LF2Character child = Register(world, 20, 300);
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = 400;

            world.HeldObjectProcessAll(1);
            Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(400));
            Assert.That(
                world.LastHeldInvalidReciprocalFailureCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                world.HeldInvalidReciprocalFailureCountForDiagnostics,
                Is.EqualTo(1));

            world.HeldObjectProcessAll(2);
            Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(400));
            Assert.That(
                world.LastHeldInvalidReciprocalFailureCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                world.HeldInvalidReciprocalFailureCountForDiagnostics,
                Is.EqualTo(2));
        }

        [Test]
        public void HeldObjectProcess_ActiveHolderMismatchPreservesBothRelationFields()
        {
            var world = new SimulationWorld();
            LF2Character holder = Register(world, 30, 301);
            LF2Character child = Register(world, 31, 302);
            holder.Runtime.TargetSlotIndex = 32;
            child.Runtime.LinkState = -2;
            child.Runtime.HolderStableId = 30;

            world.HeldObjectProcessAll(1);

            Assert.That(child.Runtime.LinkState, Is.EqualTo(-2));
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(30));
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(32));
            Assert.That(
                world.HeldInvalidReciprocalFailureCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void SlotZeroMismatch_PreservesBothSidesMotionAndRng()
        {
            var world = new SimulationWorld();
            LF2Character holder = Register(world, 0, 303);
            LF2Character child = Register(world, 1, 304);
            holder.Runtime.LinkState = 5;
            holder.Runtime.TargetSlotIndex = 2;
            holder.Runtime.Vx = 11f;
            child.Runtime.LinkState = -5;
            child.Runtime.HolderStableId = 0;
            child.Runtime.HeldWeaponStableId = 73;
            child.Runtime.OwnerSlotIndex = 74;
            child.Runtime.SpawnerSlotIndex = 75;
            child.Runtime.Vx = 12f;
            child.Runtime.Vy = 13f;
            child.Runtime.Vz = 14f;
            ulong rngBefore = world.Rng.CallCount;

            world.HeldObjectProcessAll(3);

            Assert.That(child.Runtime.LinkState, Is.EqualTo(-5));
            Assert.That(child.Runtime.HolderStableId, Is.Zero);
            Assert.That(child.Runtime.HeldWeaponStableId, Is.EqualTo(73));
            Assert.That(child.Runtime.OwnerSlotIndex, Is.EqualTo(74));
            Assert.That(child.Runtime.SpawnerSlotIndex, Is.EqualTo(75));
            Assert.That(child.Runtime.Vx, Is.EqualTo(12f));
            Assert.That(child.Runtime.Vy, Is.EqualTo(13f));
            Assert.That(child.Runtime.Vz, Is.EqualTo(14f));
            Assert.That(holder.Runtime.LinkState, Is.EqualTo(5));
            Assert.That(holder.Runtime.TargetSlotIndex, Is.EqualTo(2));
            Assert.That(holder.Runtime.Vx, Is.EqualTo(11f));
            Assert.That(world.Rng.CallCount, Is.EqualTo(rngBefore));
        }

        [Test]
        public void ExtendedHighOutOfRangeParent_PreservesNegativeRelation()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                512);
            LF2Character child = Register(world, 511, 305);
            child.Runtime.LinkState = -7;
            child.Runtime.HolderStableId = 512;

            world.HeldObjectProcessAll(4);

            Assert.That(child.Runtime.LinkState, Is.EqualTo(-7));
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(512));
            Assert.That(
                world.LastHeldInvalidReciprocalFailureCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void LifecycleCleanup_PreventsInvalidFailureAndSameSlotAba()
        {
            var world = new SimulationWorld();
            LF2Character holder = Register(world, 12, 306);
            LF2Character child = Register(world, 13, 307);
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = 13;
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = 12;

            world.Unregister(holder);
            LF2Character replacement = Register(world, 12, 308);
            replacement.Runtime.TargetSlotIndex = 13;
            world.HeldObjectProcessAll(5);

            Assert.That(child.Runtime.LinkState, Is.Zero);
            Assert.That(child.Runtime.HolderStableId, Is.Zero);
            Assert.That(
                world.LastHeldInvalidReciprocalFailureCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.HeldInvalidReciprocalFailureCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void InvalidRelations_EmitReasonedPreservedTraceOnlyWithSink()
        {
            var world = new SimulationWorld();
            var events = new BattleParityStructuralEventBuffer(400);
            world.SetStructuralEventSinkForDiagnostics(
                events,
                0,
                "fixture-setup");
            LF2Character outOfRange = Register(world, 20, 309);
            LF2Character inactive = Register(world, 21, 310);
            LF2Character holder = Register(world, 30, 311);
            LF2Character mismatch = Register(world, 31, 312);
            outOfRange.Runtime.LinkState = -1;
            outOfRange.Runtime.HolderStableId = 400;
            inactive.Runtime.LinkState = -1;
            inactive.Runtime.HolderStableId = 22;
            holder.Runtime.TargetSlotIndex = 32;
            mismatch.Runtime.LinkState = -2;
            mismatch.Runtime.HolderStableId = 30;

            world.HeldObjectProcessAll(6);

            BattleParityStructuralEvent[] invalid = events.Events
                .Where(value =>
                    value.Pass == "negative-held-validation" &&
                    value.Action == "link-validation")
                .ToArray();
            Assert.That(invalid.Length, Is.EqualTo(3));
            Assert.That(
                invalid.Select(value => value.Reason),
                Is.EquivalentTo(new[]
                {
                    "parent-out-of-range",
                    "parent-inactive",
                    "reciprocal-mismatch",
                }));
            Assert.That(invalid.All(value =>
                value.Outcome == "preserved" &&
                value.Before == value.After), Is.True);
            Assert.That(
                world.LastHeldInvalidReciprocalFailureCountForDiagnostics,
                Is.EqualTo(3));
        }

        [Test]
        public void WarmedSinkOffInvalidPass_DoesNotAllocateManagedMemory()
        {
            var world = new SimulationWorld();
            LF2Character child = Register(world, 20, 313);
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = 400;
            world.HeldObjectProcessAll(7);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.HeldObjectProcessAll(8);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
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
    }

    [InitializeOnLoad]
    internal static class NTSD28B6HeldInvalidReciprocalPreservePlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B6-HeldInvalidReciprocalPreserve-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B6-HeldInvalidReciprocalPreserve-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B6HeldInvalidReciprocalPreservePlayRunner()
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
                var tests = new SimulationQueryAndLinkModuleEditorTests();
                tests.HeldObjectProcess_OutOfRangeNegativeHolderRetainsHolderSlotAcrossBothPasses();
                tests.HeldObjectProcess_ActiveHolderMismatchPreservesBothRelationFields();
                tests.SlotZeroMismatch_PreservesBothSidesMotionAndRng();
                tests.ExtendedHighOutOfRangeParent_PreservesNegativeRelation();
                tests.LifecycleCleanup_PreventsInvalidFailureAndSameSlotAba();
                tests.InvalidRelations_EmitReasonedPreservedTraceOnlyWithSink();
                tests.WarmedSinkOffInvalidPass_DoesNotAllocateManagedMemory();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=7\n" +
                    "missingMismatchPreserve=exact\n" +
                    "slotZeroHighLifecycleTrace=exact\n" +
                    "rngMutation=none\n" +
                    "warmedSinkOffAllocationBytes=0\n" +
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
