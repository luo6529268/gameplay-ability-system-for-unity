using System;
using NUnit.Framework;
using NTSD.Simulation;
using UnityEngine.Scripting;

namespace NTSD.Test.Editor
{
    public sealed class BattleManagedMemoryBoundaryEditorTests
    {
        [Test]
        public void BattleWindow_UsesEditorObservationMode_AndPreservesGcMode()
        {
            GarbageCollector.Mode originalMode = GarbageCollector.GCMode;
            var boundary = new BattleManagedMemoryBoundary();

            try
            {
                boundary.CompleteLoadingAndOpenBattleWindow();

                Assert.That(boundary.BattleWindowOpen, Is.True);
                Assert.That(boundary.ManagedCollectionControlSupported, Is.False);
                Assert.That(boundary.ManagedCollectionDisabled, Is.False);
                Assert.That(GarbageCollector.GCMode, Is.EqualTo(originalMode));
            }
            finally
            {
                boundary.CloseBattleWindow();
            }

            Assert.That(GarbageCollector.GCMode, Is.EqualTo(originalMode));
        }

        [Test]
        public void BattleWindow_RecordsAllocationSeparatelyFromCollection()
        {
            GarbageCollector.Mode originalMode = GarbageCollector.GCMode;
            long allocationSnapshot = 1024;
            var boundary = new BattleManagedMemoryBoundary(() => allocationSnapshot);

            try
            {
                boundary.CompleteLoadingAndOpenBattleWindow();
                boundary.BeginTick();
                allocationSnapshot += 4096;
                boundary.ObserveAfterTick(17);

                Assert.That(boundary.HasAllocationViolation, Is.True);
                Assert.That(boundary.AllocatedBytes, Is.EqualTo(4096));
                Assert.That(boundary.AllocationViolationCount, Is.EqualTo(1));
                Assert.That(boundary.FirstAllocationTick, Is.EqualTo(17));
                Assert.That(boundary.HasCollectionViolation, Is.False);
            }
            finally
            {
                boundary.CloseBattleWindow();
            }

            Assert.That(GarbageCollector.GCMode, Is.EqualTo(originalMode));
        }

        [Test]
        public void BattleWindow_SeparatesTickDriverUpdateAndPresentationAllocations()
        {
            long allocationSnapshot = 1000;
            var boundary = new BattleManagedMemoryBoundary(() => allocationSnapshot);

            try
            {
                boundary.CompleteLoadingAndOpenBattleWindow();
                boundary.BeginDriverUpdate();
                allocationSnapshot += 100;

                boundary.BeginTick();
                allocationSnapshot += 50;
                boundary.ObserveAfterTick(23);

                allocationSnapshot += 25;
                boundary.ObserveAfterDriverUpdate(23);

                boundary.BeginPresentation();
                allocationSnapshot += 200;
                boundary.ObserveAfterPresentation(23);

                Assert.That(boundary.AllocatedBytes, Is.EqualTo(50));
                Assert.That(boundary.DriverUpdateAllocatedBytes, Is.EqualTo(125));
                Assert.That(boundary.DriverUpdateAllocationViolationCount, Is.EqualTo(1));
                Assert.That(boundary.FirstDriverUpdateAllocationTick, Is.EqualTo(23));
                Assert.That(boundary.PresentationAllocatedBytes, Is.EqualTo(200));
                Assert.That(boundary.PresentationAllocationViolationCount, Is.EqualTo(1));
                Assert.That(boundary.FirstPresentationAllocationTick, Is.EqualTo(23));
            }
            finally
            {
                boundary.CloseBattleWindow();
            }
        }

        [Test]
        public void BattleWindow_RecordsPlayerLoopEnvelopeSeparately()
        {
            long allocationSnapshot = 5000;
            var boundary = new BattleManagedMemoryBoundary(() => allocationSnapshot);

            try
            {
                boundary.CompleteLoadingAndOpenBattleWindow();
                boundary.BeginPlayerLoopFrame(31);
                allocationSnapshot += 640;
                boundary.ObserveAfterPlayerLoopFrame(31);

                Assert.That(boundary.HasPlayerLoopAllocationViolation, Is.True);
                Assert.That(boundary.PlayerLoopAllocatedBytes, Is.EqualTo(640));
                Assert.That(boundary.PlayerLoopAllocationViolationCount, Is.EqualTo(1));
                Assert.That(boundary.FirstPlayerLoopAllocationTick, Is.EqualTo(31));
                Assert.That(boundary.PlayerLoopEnvelopeHardGateSupported, Is.False,
                    "Editor callbacks share the PlayerLoop main thread, so the envelope is " +
                    "observational in Editor and becomes a hard gate in Player builds.");
            }
            finally
            {
                boundary.CloseBattleWindow();
            }
        }

        [Test]
        public void FormalBattleSettings_DisableAllocatingFullSnapshotDiagnostics()
        {
            var settings = new LockstepSimulationSettings
            {
                enableFrameChecksum = true,
                captureFullFrameSnapshotForDiagnostics = true,
            };

            bool changed = settings.DisableAllocatingDiagnosticsForFormalBattle();

            Assert.That(changed, Is.True);
            Assert.That(settings.enableFrameChecksum, Is.True);
            Assert.That(settings.captureFullFrameSnapshotForDiagnostics, Is.False);
            Assert.That(
                settings.DisableAllocatingDiagnosticsForFormalBattle(),
                Is.False);
        }

    }
}
