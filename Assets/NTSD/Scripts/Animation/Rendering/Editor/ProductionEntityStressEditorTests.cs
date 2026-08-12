#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.EditorTools;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class ProductionEntityStressEditorTests
    {
        private sealed class RingBufferTask : LF2TaskBase
        {
            public RingBufferTask(int id)
            {
                Id = id;
            }

            public int Id { get; }
            public override LF2TaskType TaskType => LF2TaskType.CreateObject;
        }

        [Test]
        public void TaskRingBuffer_PreservesFifoAcrossWrapAndResize()
        {
            var buffer = new LF2TaskRingBuffer(2);
            var first = new RingBufferTask(1);
            var second = new RingBufferTask(2);
            var third = new RingBufferTask(3);
            var fourth = new RingBufferTask(4);

            Assert.That(buffer.TryEnqueue(first), Is.True);
            Assert.That(buffer.TryEnqueue(second), Is.True);
            Assert.That(buffer.TryDequeue(out LF2TaskBase dequeued), Is.True);
            Assert.That(dequeued, Is.SameAs(first));

            Assert.That(buffer.TryEnqueue(third), Is.True);
            Assert.That(buffer.TryEnqueue(fourth), Is.True);
            Assert.That(buffer.Capacity, Is.EqualTo(4));

            Assert.That(buffer.TryDequeue(out dequeued), Is.True);
            Assert.That(dequeued, Is.SameAs(second));
            Assert.That(buffer.TryDequeue(out dequeued), Is.True);
            Assert.That(dequeued, Is.SameAs(third));
            Assert.That(buffer.TryDequeue(out dequeued), Is.True);
            Assert.That(dequeued, Is.SameAs(fourth));
            Assert.That(buffer.TryDequeue(out _), Is.False);
        }

        [Test]
        public void TaskRingBuffer_SealedCapacityRejectsWithoutAllocating()
        {
            var buffer = new LF2TaskRingBuffer(1);
            var accepted = new RingBufferTask(1);
            var rejected = new RingBufferTask(2);
            Assert.That(buffer.TryEnqueue(accepted), Is.True);
            buffer.SealCapacity();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            bool enqueued = buffer.TryEnqueue(rejected);
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(enqueued, Is.False);
            Assert.That(buffer.RejectedEnqueueCount, Is.EqualTo(1));
            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
            Assert.That(buffer.TryDequeue(out LF2TaskBase dequeued), Is.True);
            Assert.That(dequeued, Is.SameAs(accepted));
        }

        [Test]
        public void RuntimeCapacitySeal_PreMaterializesSlotsAndRejectsGrowthWithoutAllocating()
        {
            var slots = new RuntimeSlotTable(1050, 20, 50);
            var rest = new RuntimeRestStore(1050);
            var capacity = new SimulationRuntimeCapacityModule(slots, rest);
            capacity.Seal();

            Assert.That(slots.MaterializedPageCount, Is.EqualTo(5));
            Assert.That(rest.UsesDenseBattleStorage, Is.True);

            _ = slots.GetRawRuntime(1049);
            _ = capacity.TryAuthorizeGrowth();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            NTSDEntityRuntime runtime = slots.GetRawRuntime(1049);
            bool growthAuthorized = capacity.TryAuthorizeGrowth();
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(runtime, Is.Not.Null);
            Assert.That(growthAuthorized, Is.False);
            Assert.That(capacity.RejectedGrowthCount, Is.EqualTo(2));
            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
        }

        [Test]
        public void RuntimeRestStore_SealedHitAndTickPathDoesNotAllocate()
        {
            var rest = new RuntimeRestStore(1050);
            rest.PrepareForBattle();
            rest.SealCapacity();
            rest.SetVRest(100, 200, 3);
            rest.SetVRest(100, 200, 0);

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int attackerSlot = 200; attackerSlot < 264; attackerSlot++)
                rest.SetVRest(100, attackerSlot, 3);
            rest.TickVictim(100);
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(rest.VRestEntryCount, Is.EqualTo(64));
            Assert.That(rest.GetVRest(100, 200), Is.EqualTo(2));
            Assert.That(rest.RejectedVRestWriteCount, Is.Zero);
            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
        }

        [Test]
        public void RuntimeRestStore_ExtendedSparseSealedInsertRemoveAndTickDoesNotAllocate()
        {
            const int capacity = 4096;
            var rest = new RuntimeRestStore(capacity);
            rest.SetVRest(3100, 17, 4);
            rest.PrepareForBattle();
            rest.SealCapacity();

            Assert.That(rest.UsesDenseBattleStorage, Is.False);
            Assert.That(rest.UsesPreallocatedSparseBattleStorage, Is.True);
            Assert.That(
                rest.PreparedSparseVRestEntryCapacity,
                Is.GreaterThanOrEqualTo(capacity * 32));

            for (int attackerSlot = 2000; attackerSlot < 2064; attackerSlot++)
                rest.SetVRest(3000, attackerSlot, 3);
            rest.TickVictim(3000);

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int repeat = 0; repeat < 16; repeat++)
            {
                for (int attackerSlot = 2000; attackerSlot < 2064; attackerSlot++)
                    rest.SetVRest(3000, attackerSlot, 0);
                for (int attackerSlot = 2064; attackerSlot < 2128; attackerSlot++)
                    rest.SetVRest(3000, attackerSlot, 3);
                rest.TickVictim(3000);
                for (int attackerSlot = 2064; attackerSlot < 2128; attackerSlot++)
                    rest.SetVRest(3000, attackerSlot, 0);
                for (int attackerSlot = 2000; attackerSlot < 2064; attackerSlot++)
                    rest.SetVRest(3000, attackerSlot, 3);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(rest.GetVRest(3100, 17), Is.EqualTo(4));
            Assert.That(rest.VRestEntryCount, Is.EqualTo(65));
            Assert.That(rest.VRestRowCount, Is.EqualTo(2));
            Assert.That(rest.RejectedVRestWriteCount, Is.Zero);
            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
        }

        [Test]
        public void BattleBuffer_SealedSoundQueueUsesPreparedStorageAndRejectsOverflowWithoutAllocating()
        {
            var buffers = new SimulationBattleBufferModule(64);
            buffers.Seal();
            buffers.TryQueueSound(new PendingSoundEvent("warm", 0, 0));
            buffers.PendingSounds.Clear();

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1024; i++)
                buffers.TryQueueSound(new PendingSoundEvent("steady", i, 1));
            bool overflowAccepted = buffers.TryQueueSound(
                new PendingSoundEvent("overflow", 1024, 1));
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(buffers.PendingSounds, Has.Count.EqualTo(1024));
            Assert.That(overflowAccepted, Is.False);
            Assert.That(buffers.RejectedSoundEventCount, Is.EqualTo(1));
            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
        }

        [Test]
        public void ZeroGcGate_DefaultsOnAndCanBeExplicitlyDisabled()
        {
            ProductionEntityStressConfig required =
                ProductionEntityStressConfig.FromRequest(
                    new ProductionEntityStressRequest(),
                    ".");
            ProductionEntityStressConfig disabled =
                ProductionEntityStressConfig.FromRequest(
                    new ProductionEntityStressRequest
                    {
                        requireZeroGcAfterWarmup = false,
                    },
                    ".");

            Assert.That(required.RequireZeroGcAfterWarmup, Is.True);
            Assert.That(disabled.RequireZeroGcAfterWarmup, Is.False);
            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(required),
                Is.Not.EqualTo(
                    ProductionEntityStressFingerprint.BuildWorkload(disabled)));
        }

        [Test]
        public void ZeroGcGate_RejectsCollectionsEvenWhenAllocatedByteDeltaIsZero()
        {
            var gate = new ProductionEntityStressZeroGcGateAccumulator();
            var report = new ProductionEntityStressReport();

            gate.Record(
                logicTick: 31,
                allocatedBytes: 0L,
                generation0Collections: 0,
                generation1Collections: 0,
                generation2Collections: 0);
            gate.Populate(report, required: true);

            Assert.That(gate.HasPassed(required: true), Is.True);
            Assert.That(report.zeroGcGatePassed, Is.True);
            Assert.That(report.zeroGcGateObservedSteadyTickCount, Is.EqualTo(1));

            gate.Record(
                logicTick: 32,
                allocatedBytes: 0L,
                generation0Collections: 1,
                generation1Collections: 0,
                generation2Collections: 0);
            gate.Populate(report, required: true);

            Assert.That(gate.HasPassed(required: true), Is.False);
            Assert.That(report.zeroGcGatePassed, Is.False);
            Assert.That(report.zeroGcGateViolatingTickCount, Is.Zero);
            Assert.That(report.zeroGcGateCollectionViolatingTickCount, Is.EqualTo(1));
            Assert.That(report.zeroGcGateFirstCollectionLogicTick, Is.EqualTo(32));
            Assert.That(report.zeroGcGateGeneration0CollectionCount, Is.EqualTo(1));
        }

        [Test]
        public void ZeroGcGatePolicy_RejectsCollectionObservedOutsideMeasuredTick()
        {
            var report = new ProductionEntityStressReport
            {
                zeroGcGatePassed = true,
                zeroGcGateObservedSteadyTickCount = 1800,
                managedMemoryCollectionViolation = true,
            };

            Assert.That(
                ProductionEntityStressZeroGcGatePolicy.HasPassed(report, required: true),
                Is.False);

            report.managedMemoryCollectionViolation = false;
            Assert.That(
                ProductionEntityStressZeroGcGatePolicy.HasPassed(report, required: true),
                Is.True);
            Assert.That(
                ProductionEntityStressZeroGcGatePolicy.HasPassed(report, required: false),
                Is.True);
        }

        [Test]
        public void ZeroGcGatePolicy_PlayerLoopEnvelopeIsObservationalInEditorAndHardInPlayer()
        {
            var report = new ProductionEntityStressReport
            {
                zeroGcGatePassed = true,
                zeroGcGateObservedSteadyTickCount = 1800,
                managedMemoryPlayerLoopAllocationViolation = true,
                managedMemoryPlayerLoopEnvelopeHardGateSupported = false,
            };

            Assert.That(
                ProductionEntityStressZeroGcGatePolicy.HasPassed(report, required: true),
                Is.True,
                "Editor PlayerLoop observations include Editor-owned callbacks and remain evidence only.");

            report.managedMemoryPlayerLoopEnvelopeHardGateSupported = true;
            Assert.That(
                ProductionEntityStressZeroGcGatePolicy.HasPassed(report, required: true),
                Is.False,
                "A Player build must reject any allocation inside the full battle frame envelope.");
        }

        [Test]
        public void ProfilerFrameGcPolicy_StartsOnlyForFormalPostWarmupCandidate()
        {
            const int warmupTicks = 120;
            const int entityCount = 1000;
            float oneTick = SimulationConstants.SIM_DT;

            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                warmupTicks,
                warmupTicks,
                entityCount,
                entityCount,
                rosterMutationPending: false,
                saturationDrainActive: false,
                oneTick,
                sampledLogicTicks: 0,
                targetSampleTicks: 300,
                shouldAutoStopWhenSampled: true), Is.True);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                warmupTicks - 1,
                warmupTicks,
                entityCount,
                entityCount,
                rosterMutationPending: false,
                saturationDrainActive: false,
                oneTick,
                sampledLogicTicks: 0,
                targetSampleTicks: 300,
                shouldAutoStopWhenSampled: true), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                warmupTicks,
                warmupTicks,
                entityCount - 1,
                entityCount,
                rosterMutationPending: false,
                saturationDrainActive: false,
                oneTick,
                sampledLogicTicks: 0,
                targetSampleTicks: 300,
                shouldAutoStopWhenSampled: true), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                warmupTicks,
                warmupTicks,
                entityCount,
                entityCount,
                rosterMutationPending: true,
                saturationDrainActive: false,
                oneTick,
                sampledLogicTicks: 0,
                targetSampleTicks: 300,
                shouldAutoStopWhenSampled: true), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                warmupTicks,
                warmupTicks,
                entityCount,
                entityCount,
                rosterMutationPending: false,
                saturationDrainActive: true,
                oneTick,
                sampledLogicTicks: 0,
                targetSampleTicks: 300,
                shouldAutoStopWhenSampled: true), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                warmupTicks,
                warmupTicks,
                entityCount,
                entityCount,
                rosterMutationPending: false,
                saturationDrainActive: false,
                oneTick * 0.5f,
                sampledLogicTicks: 0,
                targetSampleTicks: 300,
                shouldAutoStopWhenSampled: true), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                warmupTicks,
                warmupTicks,
                entityCount,
                entityCount,
                rosterMutationPending: false,
                saturationDrainActive: false,
                oneTick,
                sampledLogicTicks: 300,
                targetSampleTicks: 300,
                shouldAutoStopWhenSampled: true), Is.False);
        }

        [Test]
        public void ProfilerFrameGcPolicy_AcceptsOnlyAllFormalLogicTickWindows()
        {
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.IsFormalWindow(
                logicTickCountAtStart: 120,
                sampledLogicTickCountAtStart: 0,
                nonSteadyLogicTickCountAtStart: 0,
                logicTickCountAtStop: 123,
                sampledLogicTickCountAtStop: 3,
                nonSteadyLogicTickCountAtStop: 0), Is.True);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.IsFormalWindow(
                logicTickCountAtStart: 120,
                sampledLogicTickCountAtStart: 0,
                nonSteadyLogicTickCountAtStart: 0,
                logicTickCountAtStop: 123,
                sampledLogicTickCountAtStop: 2,
                nonSteadyLogicTickCountAtStop: 1), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy.IsFormalWindow(
                logicTickCountAtStart: 120,
                sampledLogicTickCountAtStart: 0,
                nonSteadyLogicTickCountAtStart: 0,
                logicTickCountAtStop: 120,
                sampledLogicTickCountAtStop: 0,
                nonSteadyLogicTickCountAtStop: 0), Is.False);
        }

        [Test]
        public void ProfilerFrameGcPolicy_AlignsCompletedSamplesAndToleratesOneTrailingSample()
        {
            int aligned = ProductionEntityStressProfilerFrameSamplePolicy
                .ResolveAlignedCompletedSampleCount(
                    gcAllocatedSampleCount: 3,
                    gcAllocEventSampleCount: 2,
                    gcAllocatedWrapped: false,
                    gcAllocEventWrapped: false,
                    out int trailing,
                    out string reason);

            Assert.That(aligned, Is.EqualTo(2));
            Assert.That(trailing, Is.EqualTo(1));
            Assert.That(reason, Is.EqualTo(
                ProductionEntityStressProfilerFrameSamplePolicy.MisalignedRecorderReason));

            aligned = ProductionEntityStressProfilerFrameSamplePolicy
                .ResolveAlignedCompletedSampleCount(
                    gcAllocatedSampleCount: 4,
                    gcAllocEventSampleCount: 2,
                    gcAllocatedWrapped: false,
                    gcAllocEventWrapped: false,
                    out trailing,
                    out reason);
            Assert.That(aligned, Is.Zero);
            Assert.That(reason, Is.EqualTo(
                ProductionEntityStressProfilerFrameSamplePolicy.MisalignedRecorderReason));

            aligned = ProductionEntityStressProfilerFrameSamplePolicy
                .ResolveAlignedCompletedSampleCount(
                    gcAllocatedSampleCount: 2,
                    gcAllocEventSampleCount: 2,
                    gcAllocatedWrapped: true,
                    gcAllocEventWrapped: false,
                    out trailing,
                    out reason);
            Assert.That(aligned, Is.Zero);
            Assert.That(reason, Is.EqualTo(
                ProductionEntityStressProfilerFrameSamplePolicy.WrappedRecorderReason));
        }

        [Test]
        public void ProfilerFrameGcPolicy_UsesValueForBytesAndCountForEvents()
        {
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy
                .NormalizeGcAllocatedBytes(4096L), Is.EqualTo(4096d));
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy
                .NormalizeGcAllocEventCount(7L), Is.EqualTo(7d));
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy
                .NormalizeGcAllocatedBytes(-1L), Is.Zero);
            Assert.That(ProductionEntityStressProfilerFrameSamplePolicy
                .NormalizeGcAllocEventCount(-1L), Is.Zero);
        }

        [Test]
        public void ProfilerFrameGcLifecycle_StartsWhenEitherExactRecorderIsAvailable()
        {
            Assert.That(ProductionEntityStressProfilerFrameGcCollector.CanStart(
                disposed: false,
                active: false,
                gcAllocatedValid: true,
                gcAllocEventValid: false), Is.True);
            Assert.That(ProductionEntityStressProfilerFrameGcCollector.CanStart(
                disposed: false,
                active: false,
                gcAllocatedValid: false,
                gcAllocEventValid: true), Is.True);
            Assert.That(ProductionEntityStressProfilerFrameGcCollector.CanStart(
                disposed: false,
                active: false,
                gcAllocatedValid: false,
                gcAllocEventValid: false), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameGcCollector.CanStart(
                disposed: true,
                active: false,
                gcAllocatedValid: true,
                gcAllocEventValid: true), Is.False);
            Assert.That(ProductionEntityStressProfilerFrameGcCollector.CanStart(
                disposed: false,
                active: true,
                gcAllocatedValid: true,
                gcAllocEventValid: true), Is.False);
        }

        [Test]
        public void ProfilerFrameGcPolicy_ResolvesEachRecorderIndependently()
        {
            int count = ProductionEntityStressProfilerFrameSamplePolicy
                .ResolveCompletedSampleCount(
                    sampleCount: 3,
                    wrapped: false,
                    out int trailing,
                    out string reason);

            Assert.That(count, Is.EqualTo(3));
            Assert.That(trailing, Is.Zero);
            Assert.That(reason, Is.Empty);

            count = ProductionEntityStressProfilerFrameSamplePolicy
                .ResolveCompletedSampleCount(
                    sampleCount: 0,
                    wrapped: false,
                    out trailing,
                    out reason);
            Assert.That(count, Is.Zero);
            Assert.That(reason, Is.EqualTo(
                ProductionEntityStressProfilerFrameSamplePolicy.MissingCompletedSampleReason));
        }

        [Test]
        public void SoundPresentationSuppression_DefaultRunLeavesDriverUnchanged()
        {
            using var scope = new StressSoundDriverScope();
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest(),
                ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport();

            bool previous = ProductionEntityStressRunner
                .ApplySoundPresentationSuppressionForDiagnostics(
                    scope.Driver,
                    config,
                    report,
                    out long dispatchedBaseline,
                    out long suppressedBaseline);

            Assert.That(config.SimulationOnly, Is.False);
            Assert.That(config.SoundPresentationMode,
                Is.EqualTo(ProductionEntityStressSoundPresentationMode.Inherit));
            Assert.That(config.SuppressSoundPresentation, Is.False);
            Assert.That(previous, Is.False);
            Assert.That(scope.Driver.SuppressSoundPresentationForDiagnostics, Is.False);
            Assert.That(report.soundPresentationModeRequested, Is.EqualTo("inherit"));
            Assert.That(report.soundPresentationModeResolved, Is.EqualTo("dispatch"));
            Assert.That(report.soundPresentationSuppressionRequested, Is.False);
            Assert.That(report.soundPresentationSuppressionConfigured, Is.False);
            Assert.That(report.soundPresentationSuppressionApplied, Is.False);
            Assert.That(ProductionEntityStressRunner
                .RestoreSoundPresentationSuppressionForDiagnostics(
                    scope.Driver,
                    previous,
                    report,
                    dispatchedBaseline,
                    suppressedBaseline), Is.True);
            Assert.That(report.soundPresentationSuppressionRestored, Is.True);
        }

        [Test]
        public void SoundPresentationSuppression_SimulationOnlyAppliesAndReportsDeltas()
        {
            using var scope = new StressSoundDriverScope();
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest { simulationOnly = true },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig normalPresentationConfig =
                ProductionEntityStressConfig.FromRequest(
                    new ProductionEntityStressRequest { simulationOnly = false },
                    ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport();

            bool previous = ProductionEntityStressRunner
                .ApplySoundPresentationSuppressionForDiagnostics(
                    scope.Driver,
                    config,
                    report,
                    out long dispatchedBaseline,
                    out long suppressedBaseline);
            scope.Driver.World.Register(new StressSoundEmitter(scope.Driver.World));
            Assert.That(scope.Driver.StepOneTick(
                FrameInputSet.Empty(1),
                ignorePaused: true,
                buildPresentation: false), Is.True);
            scope.Driver.FlushPublishedSoundEventsForTesting();
            ProductionEntityStressRunner.CaptureSoundPresentationSuppressionForReport(
                report,
                scope.Driver,
                dispatchedBaseline,
                suppressedBaseline);

            Assert.That(previous, Is.False);
            Assert.That(config.SoundPresentationMode,
                Is.EqualTo(ProductionEntityStressSoundPresentationMode.Inherit));
            Assert.That(config.SuppressSoundPresentation, Is.True);
            Assert.That(scope.Driver.SuppressSoundPresentationForDiagnostics, Is.True);
            Assert.That(report.soundPresentationModeRequested, Is.EqualTo("inherit"));
            Assert.That(report.soundPresentationModeResolved, Is.EqualTo("suppress"));
            Assert.That(report.soundPresentationSuppressionRequested, Is.True);
            Assert.That(report.soundPresentationSuppressionConfigured, Is.True);
            Assert.That(report.soundPresentationSuppressionApplied, Is.True);
            Assert.That(
                ProductionEntityStressFingerprint.BuildImplementationConfig(config),
                Is.Not.EqualTo(ProductionEntityStressFingerprint
                    .BuildImplementationConfig(normalPresentationConfig)));
            Assert.That(report.soundPresentationDispatchedEventCountDelta, Is.Zero);
            Assert.That(report.soundPresentationSuppressedEventCountDelta, Is.EqualTo(1));
            Assert.That(ProductionEntityStressRunner
                .RestoreSoundPresentationSuppressionForDiagnostics(
                    scope.Driver,
                    previous,
                    report,
                    dispatchedBaseline,
                    suppressedBaseline), Is.True);
            Assert.That(scope.Driver.SuppressSoundPresentationForDiagnostics, Is.False);
        }

        [Test]
        public void SoundPresentationMode_ExplicitDispatchOverridesSimulationOnlyAndRestores()
        {
            using var scope = new StressSoundDriverScope();
            scope.Driver.SetSoundPresentationSuppressedForDiagnostics(true);
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    simulationOnly = true,
                    soundPresentationMode = "dispatch",
                },
                ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport();

            bool previous = ProductionEntityStressRunner
                .ApplySoundPresentationSuppressionForDiagnostics(
                    scope.Driver,
                    config,
                    report,
                    out long dispatchedBaseline,
                    out long suppressedBaseline);

            Assert.That(previous, Is.True);
            Assert.That(config.SimulationOnly, Is.True);
            Assert.That(config.SuppressSoundPresentation, Is.False);
            Assert.That(scope.Driver.SuppressSoundPresentationForDiagnostics, Is.False);
            Assert.That(report.soundPresentationModeRequested, Is.EqualTo("dispatch"));
            Assert.That(report.soundPresentationModeResolved, Is.EqualTo("dispatch"));
            Assert.That(report.soundPresentationSuppressionRequested, Is.False);
            Assert.That(ProductionEntityStressRunner
                .RestoreSoundPresentationSuppressionForDiagnostics(
                    scope.Driver,
                    previous,
                    report,
                    dispatchedBaseline,
                    suppressedBaseline), Is.True);
            Assert.That(scope.Driver.SuppressSoundPresentationForDiagnostics, Is.True);
        }

        [Test]
        public void SoundPresentationMode_ExplicitModesAreFingerprintDistinctNotWorkloadDistinct()
        {
            ProductionEntityStressConfig suppress = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    simulationOnly = false,
                    soundPresentationMode = " SuPpReSs ",
                },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig dispatch = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    simulationOnly = false,
                    soundPresentationMode = "dispatch",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(suppress.SoundPresentationMode,
                Is.EqualTo(ProductionEntityStressSoundPresentationMode.Suppress));
            Assert.That(suppress.SuppressSoundPresentation, Is.True);
            Assert.That(dispatch.SuppressSoundPresentation, Is.False);
            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(suppress),
                Is.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(dispatch)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildImplementationConfig(suppress),
                Is.Not.EqualTo(
                    ProductionEntityStressFingerprint.BuildImplementationConfig(dispatch)));
        }

        [Test]
        public void SoundPresentationMode_LegacyJsonDefaultsToInheritAndInvalidFailsFast()
        {
            ProductionEntityStressRequest legacyRequest =
                JsonUtility.FromJson<ProductionEntityStressRequest>("{}");
            ProductionEntityStressConfig legacy = ProductionEntityStressConfig.FromRequest(
                legacyRequest,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(legacyRequest.soundPresentationMode, Is.EqualTo("inherit"));
            Assert.That(legacy.SoundPresentationMode,
                Is.EqualTo(ProductionEntityStressSoundPresentationMode.Inherit));
            Assert.That(legacy.SuppressSoundPresentation, Is.False);

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    new ProductionEntityStressRequest
                    {
                        soundPresentationMode = "mute",
                    },
                    ProductionEntityStressPaths.ProjectRoot));
            Assert.That(exception.Message, Does.Contain("inherit, suppress, or dispatch"));
        }

        [Test]
        public void SoundPresentationSuppression_CleanupJournalRestoresAfterEarlierFailure()
        {
            using var scope = new StressSoundDriverScope();
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest { simulationOnly = true },
                ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport();
            bool previous = ProductionEntityStressRunner
                .ApplySoundPresentationSuppressionForDiagnostics(
                    scope.Driver,
                    config,
                    report,
                    out long dispatchedBaseline,
                    out long suppressedBaseline);
            var journal = new ProductionEntityStressCleanupJournal();

            Assert.That(journal.Attempt(
                "injected-earlier-cleanup-failure",
                () => throw new InvalidOperationException("injected")), Is.False);
            Assert.That(journal.Attempt(
                "restore-sound-presentation-suppression",
                () => ProductionEntityStressRunner
                    .RestoreSoundPresentationSuppressionForDiagnostics(
                        scope.Driver,
                        previous,
                        report,
                        dispatchedBaseline,
                        suppressedBaseline)), Is.True);

            Assert.That(journal.FailureCount, Is.EqualTo(1));
            Assert.That(scope.Driver.SuppressSoundPresentationForDiagnostics,
                Is.EqualTo(previous));
            Assert.That(report.soundPresentationSuppressionRestored, Is.True);
        }

        [Test]
        public void SkipLateRendererUpdate_LegacyRequestDefaultsOffAndCallsPresentation()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>("{}");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            var report = new ProductionEntityStressReport();

            bool previous = ProductionEntityStressRunner
                .ApplySkipLateRendererUpdateForDiagnostics(
                    world,
                    config,
                    report,
                    out long baseline);
            world.RenderDispatchAll(1, buildPresentation: false);
            ProductionEntityStressRunner.CaptureSkipLateRendererUpdateForReport(
                report,
                world,
                baseline);

            Assert.That(request.skipLateRendererUpdate, Is.False);
            Assert.That(config.SkipLateRendererUpdate, Is.False);
            Assert.That(world.SkipLateRendererUpdateForDiagnostics, Is.False);
            Assert.That(world.LateRendererUpdateInvocationCountForDiagnostics, Is.Zero);
            Assert.That(world.CentralOnlyRendererShellBypassCountForDiagnostics, Is.EqualTo(1));
            Assert.That(report.skipLateRendererUpdateRequested, Is.False);
            Assert.That(report.skipLateRendererUpdateConfigured, Is.False);
            Assert.That(report.skipLateRendererUpdateApplied, Is.False);
            Assert.That(report.skipLateRendererUpdateTickCount, Is.Zero);
            Assert.That(ProductionEntityStressRunner
                .RestoreSkipLateRendererUpdateForDiagnostics(
                    world,
                    previous,
                    report,
                    baseline), Is.True);
            Assert.That(report.skipLateRendererUpdateRestored, Is.True);
        }

        [Test]
        public void SkipLateRendererUpdate_RequestWithoutSimulationOnlyFailsFast()
        {
            var request = new ProductionEntityStressRequest
            {
                simulationOnly = false,
                skipLateRendererUpdate = true,
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("simulationOnly=true"));
        }

        [Test]
        public void SkipLateRendererUpdate_SimulationOnlySkipsOnlyLatePresentationCall()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    simulationOnly = true,
                    skipLateRendererUpdate = true,
                },
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            var report = new ProductionEntityStressReport();

            bool previous = ProductionEntityStressRunner
                .ApplySkipLateRendererUpdateForDiagnostics(
                    world,
                    config,
                    report,
                    out long baseline);
            world.RenderDispatchAll(2, buildPresentation: false);
            ProductionEntityStressRunner.CaptureSkipLateRendererUpdateForReport(
                report,
                world,
                baseline);

            Assert.That(world.LateRendererUpdateInvocationCountForDiagnostics, Is.Zero);
            Assert.That(world.SkippedLateRendererUpdateTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(report.skipLateRendererUpdateRequested, Is.True);
            Assert.That(report.skipLateRendererUpdateConfigured, Is.True);
            Assert.That(report.skipLateRendererUpdateApplied, Is.True);
            Assert.That(report.skipLateRendererUpdateTickCount, Is.EqualTo(1));
            Assert.That(ProductionEntityStressRunner
                .RestoreSkipLateRendererUpdateForDiagnostics(
                    world,
                    previous,
                    report,
                    baseline), Is.True);

            world.RenderDispatchAll(3, buildPresentation: false);
            Assert.That(world.LateRendererUpdateInvocationCountForDiagnostics, Is.Zero);
            Assert.That(world.CentralOnlyRendererShellBypassCountForDiagnostics, Is.EqualTo(1),
                "restoration must re-enable the CentralOnly host pass without scanning renderer shells");
        }

        [Test]
        public void SkipLateRendererUpdate_FullTickPreservesLogicChecksums()
        {
            var baselineWorld = new SimulationWorld();
            var skippedWorld = new SimulationWorld();
            baselineWorld.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            skippedWorld.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    simulationOnly = true,
                    skipLateRendererUpdate = true,
                },
                ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport();
            bool previous = ProductionEntityStressRunner
                .ApplySkipLateRendererUpdateForDiagnostics(
                    skippedWorld,
                    config,
                    report,
                    out long baseline);

            new NTSDBattleTickSystem(baselineWorld).RunReleaseTick(7, buildPresentation: false);
            new NTSDBattleTickSystem(skippedWorld).RunReleaseTick(7, buildPresentation: false);
            BattleParityFrameSnapshot expected =
                baselineWorld.CaptureParityFrameSnapshot(7);
            BattleParityFrameSnapshot actual =
                skippedWorld.CaptureParityFrameSnapshot(7);

            Assert.That(actual.Hashes.Input, Is.EqualTo(expected.Hashes.Input));
            Assert.That(actual.Hashes.Rng, Is.EqualTo(expected.Hashes.Rng));
            Assert.That(actual.Hashes.World, Is.EqualTo(expected.Hashes.World));
            Assert.That(actual.Hashes.Slots, Is.EqualTo(expected.Hashes.Slots));
            Assert.That(actual.Hashes.Events, Is.EqualTo(expected.Hashes.Events));
            Assert.That(actual.OverallChecksum, Is.EqualTo(expected.OverallChecksum));
            Assert.That(baselineWorld.LateRendererUpdateInvocationCountForDiagnostics,
                Is.Zero);
            Assert.That(baselineWorld.CentralOnlyRendererShellBypassCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(skippedWorld.LateRendererUpdateInvocationCountForDiagnostics,
                Is.Zero);
            Assert.That(skippedWorld.SkippedLateRendererUpdateTickCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(ProductionEntityStressRunner
                .RestoreSkipLateRendererUpdateForDiagnostics(
                    skippedWorld,
                    previous,
                    report,
                    baseline), Is.True);
        }

        [Test]
        public void SkipLateRendererUpdate_CleanupJournalRestoresAfterEarlierFailure()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    simulationOnly = true,
                    skipLateRendererUpdate = true,
                },
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var report = new ProductionEntityStressReport();
            bool previous = ProductionEntityStressRunner
                .ApplySkipLateRendererUpdateForDiagnostics(
                    world,
                    config,
                    report,
                    out long baseline);
            var journal = new ProductionEntityStressCleanupJournal();

            Assert.That(
                journal.Attempt(
                    "injected-earlier-cleanup-failure",
                    () => throw new InvalidOperationException("injected")),
                Is.False);
            Assert.That(
                journal.Attempt(
                    "restore-skip-late-renderer-update",
                    () => ProductionEntityStressRunner
                        .RestoreSkipLateRendererUpdateForDiagnostics(
                            world,
                            previous,
                            report,
                            baseline)),
                Is.True);

            Assert.That(journal.FailureCount, Is.EqualTo(1));
            Assert.That(world.SkipLateRendererUpdateForDiagnostics, Is.EqualTo(previous));
            Assert.That(report.skipLateRendererUpdateRestored, Is.True);
        }

        [Test]
        public void CollisionRoleZeroItrFastPath_DefaultOffLeavesQueryAndValidityUntouched()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest(),
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            var report = new ProductionEntityStressReport { harnessValidity = true };

            bool previous = ProductionEntityStressRunner
                .ApplyCollisionRoleZeroItrFastPathForDiagnostics(query, config, report);

            Assert.That(config.EnableCollisionRoleZeroItrFastPath, Is.False);
            Assert.That(query.CollisionRoleZeroItrFastPathEnabled, Is.False);
            Assert.That(report.collisionRoleZeroItrFastPathRequested, Is.False);
            Assert.That(report.collisionRoleZeroItrFastPathApplied, Is.False);
            Assert.That(ProductionEntityStressRunner
                .EvaluateCollisionRoleZeroItrFastPathValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Final), Is.True);
            Assert.That(ProductionEntityStressRunner
                .RestoreCollisionRoleZeroItrFastPathForDiagnostics(query, previous, report),
                Is.True);
        }

        [Test]
        public void CollisionRoleZeroItrFastPath_NoneStoreOnlyRoleCollectorAppliesEveryTickAndRestores()
        {
            var request = new ProductionEntityStressRequest
            {
                inputMode = "none",
                enableCollisionCandidateStoreAuthority = true,
                legacyOracleInterval = 0,
                formalCollectorMode = "role",
                enableCollisionRoleZeroItrFastPath = true,
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            var report = new ProductionEntityStressReport { harnessValidity = true };
            query.FormalCollectorMode = config.FormalCollectorMode;
            bool previousAuthority = ProductionEntityStressRunner
                .ApplyCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    config,
                    report,
                    out CollisionCandidateStoreAuthorityDiagnosticsSnapshot baseline,
                    out int previousInterval);
            bool previousFastPath = ProductionEntityStressRunner
                .ApplyCollisionRoleZeroItrFastPathForDiagnostics(query, config, report);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            world.EndCollisionCandidateConsumption();
            report.logicTicksExecuted = 1;
            ProductionEntityStressRunner.RecordExpectedCollisionRoleZeroItrFastPathForDiagnostics(
                report);
            ProductionEntityStressRunner.CaptureCollisionRoleZeroItrFastPathDiagnosticsForReport(
                report,
                query);

            Assert.That(report.collisionRoleZeroItrFastPathExpectedAppliedTickCount, Is.EqualTo(1));
            Assert.That(report.collisionRoleZeroItrFastPathAppliedCount, Is.EqualTo(1));
            Assert.That(report.collisionRoleZeroItrFastPathFallbackCount, Is.Zero);
            Assert.That(report.collisionRoleZeroItrFastPathInvalidCount, Is.Zero);
            Assert.That(report.collisionRoleZeroItrFastPathZeroItrCount, Is.EqualTo(1));
            Assert.That(ProductionEntityStressRunner
                .EvaluateCollisionRoleZeroItrFastPathValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Final), Is.True);

            Assert.That(ProductionEntityStressRunner
                .RestoreCollisionRoleZeroItrFastPathForDiagnostics(query, previousFastPath, report),
                Is.True);
            Assert.That(ProductionEntityStressRunner
                .RestoreCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    previousAuthority,
                    previousInterval,
                    report,
                    in baseline), Is.True);
            Assert.That(report.collisionRoleZeroItrFastPathRestored, Is.True);
            Assert.That(query.CollisionRoleZeroItrFastPathEnabled, Is.False);
            Assert.That(ProductionEntityStressRunner
                .EvaluateCollisionRoleZeroItrFastPathValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Teardown), Is.True);
        }

        [Test]
        public void CollisionCandidateStoreAuthority_DefaultOffDoesNotAffectValidity()
        {
            var request = new ProductionEntityStressRequest();
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                collisionCandidateStoreAuthorityFailureCount = 7,
            };

            bool previousShadow =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreShadowForDiagnostics(
                    query,
                    config,
                    report);
            bool previousAuthority =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    config,
                    report,
                    out CollisionCandidateStoreAuthorityDiagnosticsSnapshot baseline,
                    out int previousLegacyOracleInterval);

            Assert.That(request.enableCollisionCandidateStoreAuthority, Is.False);
            Assert.That(config.EnableCollisionCandidateStoreAuthority, Is.False);
            Assert.That(query.CollisionCandidateStoreAuthorityEnabled, Is.False);
            Assert.That(query.CollisionCandidateStoreShadowDiagnosticsEnabled, Is.False);
            Assert.That(report.collisionCandidateStoreAuthorityRequested, Is.False);
            Assert.That(report.collisionCandidateStoreAuthorityApplied, Is.False);
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.PreTick),
                Is.True);
            Assert.That(report.harnessValidity, Is.True,
                "disabled authority diagnostics must not affect harness validity");

            Assert.That(
                ProductionEntityStressRunner.RestoreCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    previousAuthority,
                    previousLegacyOracleInterval,
                    report,
                    in baseline),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.RestoreCollisionCandidateStoreShadowForDiagnostics(
                    query,
                    previousShadow,
                    report),
                Is.True);
        }

        [Test]
        public void CollisionCandidateStoreAuthority_OptInAppliesShadowAndReportsActualTicks()
        {
            var request = new ProductionEntityStressRequest
            {
                enableCollisionCandidateStoreAuthority = true,
                enableCollisionCandidateStoreShadow = false,
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            var report = new ProductionEntityStressReport { harnessValidity = true };

            bool previousShadow =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreShadowForDiagnostics(
                    query,
                    config,
                    report);
            bool previousAuthority =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    config,
                    report,
                    out CollisionCandidateStoreAuthorityDiagnosticsSnapshot baseline,
                    out int previousLegacyOracleInterval);

            Assert.That(config.EnableCollisionCandidateStoreAuthority, Is.True);
            Assert.That(query.CollisionCandidateStoreAuthorityEnabled, Is.True);
            Assert.That(query.CollisionCandidateStoreShadowDiagnosticsEnabled, Is.True,
                "authority opt-in must ensure the oracle/store build is enabled");
            Assert.That(report.collisionCandidateStoreShadowRequested, Is.False);
            Assert.That(report.collisionCandidateStoreShadowApplied, Is.False,
                "shadow and authority reporting must remain distinct");
            Assert.That(report.collisionCandidateStoreAuthorityRequested, Is.True);
            Assert.That(report.collisionCandidateStoreAuthorityConfigured, Is.True);
            Assert.That(report.collisionCandidateStoreAuthorityApplied, Is.False,
                "setting the switch is not evidence that any tick applied authority");
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.PreTick),
                Is.True,
                "requested authority with zero ticks is valid during peak/pre-tick validation");
            var zeroTickFinal = new ProductionEntityStressReport
            {
                harnessValidity = true,
                collisionCandidateStoreAuthorityRequested = true,
                collisionCandidateStoreAuthorityConfigured = true,
            };
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    zeroTickFinal,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.False,
                "final validation requires at least one actually applied authority tick");
            var zeroTickShadow = new ProductionEntityStressReport
            {
                harnessValidity = true,
                collisionCandidateStoreShadowRequested = true,
                collisionCandidateStoreShadowApplied = true,
            };
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreShadowValidityForReport(
                    zeroTickShadow,
                    CollisionCandidateStoreValidationPhase.PreTick),
                Is.True,
                "shadow configuration is valid before the first build tick");
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreShadowValidityForReport(
                    zeroTickShadow,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.False,
                "final shadow validation requires an actual build tick");

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            world.EndCollisionCandidateConsumption();
            // Mirror the real harness accounting for the one successful logic tick.
            // This empty-world fixture has no AI resolver reads or candidate entries.
            report.logicTicksExecuted = 1;
            report.aiControlledEntityTicks = 0;
            report.collisionCandidateConsumerEntityTicks = 0;
            report.collisionCandidateCountSum = 0;
            ProductionEntityStressRunner.RecordExpectedCollisionCandidateStoreCadenceForDiagnostics(
                report,
                config.LegacyOracleInterval,
                world.CurrentTickIndex);
            ProductionEntityStressRunner.CaptureCollisionCandidateStoreShadowDiagnosticsForReport(
                report,
                query);
            ProductionEntityStressRunner.CaptureCollisionCandidateStoreAuthorityDiagnosticsForReport(
                report,
                query,
                in baseline);

            Assert.That(report.collisionCandidateStoreAuthorityApplied, Is.True);
            Assert.That(report.collisionCandidateStoreAuthorityRequestedTickCount, Is.EqualTo(1));
            Assert.That(report.collisionCandidateStoreAuthorityAppliedTickCount, Is.EqualTo(1));
            Assert.That(report.collisionCandidateStoreAuthorityLegacyFallbackTickCount, Is.Zero);
            Assert.That(report.collisionCandidateStoreShadowBuildTickCount, Is.EqualTo(1));
            Assert.That(report.collisionCandidateStoreAuthoritySampledOracleTickCount, Is.EqualTo(1));
            Assert.That(report.collisionCandidateStoreAuthorityStoreOnlyTickCount, Is.Zero);
            Assert.That(report.collisionCandidateStoreAuthorityRangeReadCount, Is.Zero);
            Assert.That(report.collisionCandidateStoreAuthorityEntryReadCount, Is.Zero);
            Assert.That(report.collisionCandidateStoreAuthorityFailureCount, Is.Zero);
            Assert.That(
                report.collisionCandidateStoreAuthorityFirstFailureReason,
                Is.EqualTo(CollisionCandidateStoreAuthorityFailureReason.None.ToString()));
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.True);

            Assert.That(
                ProductionEntityStressRunner.RestoreCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    previousAuthority,
                    previousLegacyOracleInterval,
                    report,
                    in baseline),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.RestoreCollisionCandidateStoreShadowForDiagnostics(
                    query,
                    previousShadow,
                    report),
                Is.True);
            Assert.That(report.collisionCandidateStoreAuthorityRestored, Is.True);
            Assert.That(query.CollisionCandidateStoreAuthorityEnabled, Is.False);
            Assert.That(query.CollisionCandidateStoreShadowDiagnosticsEnabled, Is.False);
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Teardown),
                Is.True);

            var zeroApplied = new ProductionEntityStressReport
            {
                harnessValidity = true,
                collisionCandidateStoreAuthorityRequested = true,
                collisionCandidateStoreAuthorityConfigured = true,
            };
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    zeroApplied,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.False);
            Assert.That(zeroApplied.harnessValidity, Is.False);

            var failed = new ProductionEntityStressReport
            {
                harnessValidity = true,
                collisionCandidateStoreAuthorityRequested = true,
                collisionCandidateStoreAuthorityConfigured = true,
                collisionCandidateStoreAuthorityApplied = true,
                collisionCandidateStoreAuthorityAppliedTickCount = 1,
                collisionCandidateStoreAuthorityFailureCount = 1,
            };
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    failed,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.False);
            Assert.That(failed.harnessValidity, Is.False);
        }

        [Test]
        public void CollisionCandidateStoreAuthority_FinalValidityUsesConsumerTicksNotAiTicks()
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                logicTicksExecuted = 1,
                aiControlledEntityTicks = 0,
                collisionCandidateConsumerEntityTicks = 1,
                collisionCandidateCountSum = 0,
                collisionCandidateStoreAuthorityRequested = true,
                collisionCandidateStoreAuthorityConfigured = true,
                collisionCandidateStoreAuthorityApplied = true,
                collisionCandidateStoreLegacyOracleInterval = 0,
                collisionCandidateStoreAuthorityRequestedTickCount = 1,
                collisionCandidateStoreAuthorityAppliedTickCount = 1,
                collisionCandidateStoreAuthorityStoreOnlyTickCount = 1,
                collisionCandidateStoreAuthorityExpectedStoreOnlyTickCount = 1,
                collisionCandidateStoreShadowBuildTickCount = 1,
                collisionCandidateStoreAuthorityRangeReadCount = 1,
                collisionCandidateStoreAuthorityEntryReadCount = 0,
            };

            Assert.That(report.aiControlledEntityTicks, Is.Zero,
                "inputMode=none must not be treated as an absent candidate consumer");
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.True);

            report.harnessValidity = true;
            report.collisionCandidateConsumerEntityTicks = 0;
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.False,
                "range reads must still match the independent strict consumer count");
        }

        [TestCase(0, 0, 10)]
        [TestCase(1, 10, 0)]
        [TestCase(3, 4, 6)]
        public void CollisionCandidateStoreAuthority_CadenceUsesAllFrozenLogicTickIndices(
            int legacyOracleInterval,
            int expectedSampledOracleTicks,
            int expectedStoreOnlyTicks)
        {
            var request = new ProductionEntityStressRequest
            {
                enableCollisionCandidateStoreAuthority = true,
                legacyOracleInterval = legacyOracleInterval,
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            var report = new ProductionEntityStressReport { harnessValidity = true };
            bool previousShadow =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreShadowForDiagnostics(
                    query,
                    config,
                    report);
            bool previousAuthority =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    config,
                    report,
                    out CollisionCandidateStoreAuthorityDiagnosticsSnapshot baseline,
                    out int previousLegacyOracleInterval);

            for (int frozenTickIndex = 0; frozenTickIndex < 10; frozenTickIndex++)
            {
                world.AdvanceBattleFlowTick(frozenTickIndex);
                world.CaptureCollisionFrameSnapshotsAll();
                world.CollectCollisionCandidatesAll();
                world.EndCollisionCandidateConsumption();
                report.logicTicksExecuted++;
                if (frozenTickIndex < 4)
                    report.warmupTicksCompleted++;
                else
                    report.sampledLogicTicks++;
                ProductionEntityStressRunner
                    .RecordExpectedCollisionCandidateStoreCadenceForDiagnostics(
                        report,
                        config.LegacyOracleInterval,
                        world.CurrentTickIndex);
            }

            ProductionEntityStressRunner.CaptureCollisionCandidateStoreShadowDiagnosticsForReport(
                report,
                query);
            ProductionEntityStressRunner.CaptureCollisionCandidateStoreAuthorityDiagnosticsForReport(
                report,
                query,
                in baseline);

            Assert.That(report.logicTicksExecuted, Is.EqualTo(10));
            Assert.That(report.warmupTicksCompleted, Is.EqualTo(4));
            Assert.That(report.sampledLogicTicks, Is.EqualTo(6));
            Assert.That(
                report.collisionCandidateStoreAuthoritySampledOracleTickCount,
                Is.EqualTo(expectedSampledOracleTicks));
            Assert.That(
                report.collisionCandidateStoreAuthorityExpectedSampledOracleTickCount,
                Is.EqualTo(expectedSampledOracleTicks));
            Assert.That(
                report.collisionCandidateStoreAuthorityStoreOnlyTickCount,
                Is.EqualTo(expectedStoreOnlyTicks));
            Assert.That(
                report.collisionCandidateStoreAuthorityExpectedStoreOnlyTickCount,
                Is.EqualTo(expectedStoreOnlyTicks));
            Assert.That(report.collisionCandidateStoreShadowBuildTickCount, Is.EqualTo(10));
            Assert.That(
                ProductionEntityStressRunner.EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.Final),
                Is.True);

            Assert.That(
                ProductionEntityStressRunner.RestoreCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    previousAuthority,
                    previousLegacyOracleInterval,
                    report,
                    in baseline),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.RestoreCollisionCandidateStoreShadowForDiagnostics(
                    query,
                    previousShadow,
                    report),
                Is.True);
        }

        [Test]
        public void CollisionCandidateStoreAuthority_CleanupJournalRestoresAfterEarlierFailure()
        {
            var request = new ProductionEntityStressRequest
            {
                enableCollisionCandidateStoreAuthority = true,
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            var report = new ProductionEntityStressReport();
            bool previousShadow =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreShadowForDiagnostics(
                    query,
                    config,
                    report);
            bool previousAuthority =
                ProductionEntityStressRunner.ApplyCollisionCandidateStoreAuthorityForDiagnostics(
                    query,
                    config,
                    report,
                    out CollisionCandidateStoreAuthorityDiagnosticsSnapshot baseline);
            var journal = new ProductionEntityStressCleanupJournal();

            Assert.That(
                journal.Attempt(
                    "injected-earlier-cleanup-failure",
                    () => throw new InvalidOperationException("injected")),
                Is.False);
            Assert.That(
                journal.Attempt(
                    "restore-collision-candidate-store-authority",
                    () => ProductionEntityStressRunner.RestoreCollisionCandidateStoreAuthorityForDiagnostics(
                        query,
                        previousAuthority,
                        report,
                        in baseline)),
                Is.True);
            Assert.That(
                journal.Attempt(
                    "restore-collision-candidate-store-shadow",
                    () => ProductionEntityStressRunner.RestoreCollisionCandidateStoreShadowForDiagnostics(
                        query,
                        previousShadow,
                        report)),
                Is.True);

            Assert.That(journal.FailureCount, Is.EqualTo(1));
            Assert.That(report.collisionCandidateStoreAuthorityRestored, Is.True);
            Assert.That(query.CollisionCandidateStoreAuthorityEnabled, Is.EqualTo(previousAuthority));
            Assert.That(
                query.CollisionCandidateStoreShadowDiagnosticsEnabled,
                Is.EqualTo(previousShadow));
        }

        [Test]
        public void BatchExitLifecycle_WaitsForStableEditModeBeforeOpeningBattleScene()
        {
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false,
                    isCompiling: true,
                    isUpdating: false,
                    battleSceneActive: false,
                    requestPrepared: false,
                    enteredPlayMode: false,
                    requestDispatched: false),
                Is.EqualTo(ProductionEntityStressBatchAction.Wait));
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false,
                    isCompiling: false,
                    isUpdating: true,
                    battleSceneActive: false,
                    requestPrepared: false,
                    enteredPlayMode: false,
                    requestDispatched: false),
                Is.EqualTo(ProductionEntityStressBatchAction.Wait));
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false,
                    isCompiling: false,
                    isUpdating: false,
                    battleSceneActive: false,
                    requestPrepared: false,
                    enteredPlayMode: false,
                    requestDispatched: false),
                Is.EqualTo(ProductionEntityStressBatchAction.OpenBattleScene));
        }

        [Test]
        public void BatchExitLifecycle_PreparesRequestBeforeEnteringPlayMode()
        {
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false,
                    isCompiling: false,
                    isUpdating: false,
                    battleSceneActive: true,
                    requestPrepared: false,
                    enteredPlayMode: false,
                    requestDispatched: false),
                Is.EqualTo(
                    ProductionEntityStressBatchAction.PrepareRequestAndEnterPlayMode));
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false,
                    isCompiling: false,
                    isUpdating: false,
                    battleSceneActive: true,
                    requestPrepared: true,
                    enteredPlayMode: false,
                    requestDispatched: false),
                Is.EqualTo(ProductionEntityStressBatchAction.EnterPlayMode));
        }

        [Test]
        public void BatchExitLifecycle_DoesNotOpenSceneOrDispatchBeforeEnteredPlayMode()
        {
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: true,
                    isCompiling: false,
                    isUpdating: false,
                    battleSceneActive: false,
                    requestPrepared: true,
                    enteredPlayMode: false,
                    requestDispatched: false),
                Is.EqualTo(ProductionEntityStressBatchAction.Wait));
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: true,
                    isPlayingOrWillChangePlaymode: true,
                    isCompiling: false,
                    isUpdating: false,
                    battleSceneActive: false,
                    requestPrepared: true,
                    enteredPlayMode: false,
                    requestDispatched: false),
                Is.EqualTo(ProductionEntityStressBatchAction.Wait));
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: true,
                    isPlayingOrWillChangePlaymode: true,
                    isCompiling: false,
                    isUpdating: false,
                    battleSceneActive: false,
                    requestPrepared: true,
                    enteredPlayMode: true,
                    requestDispatched: false),
                Is.EqualTo(ProductionEntityStressBatchAction.DispatchRequest));
            Assert.That(
                ProductionEntityStressBatchExit.ResolveLifecycleAction(
                    isPlaying: true,
                    isPlayingOrWillChangePlaymode: true,
                    isCompiling: false,
                    isUpdating: false,
                    battleSceneActive: false,
                    requestPrepared: true,
                    enteredPlayMode: true,
                    requestDispatched: true),
                Is.EqualTo(ProductionEntityStressBatchAction.MonitorResult));
        }

        [Test]
        public void ProductionServiceWaitDeadline_UsesRealtimeInsteadOfEditorUpdateCount()
        {
            const double realtimeNow = 25d;
            double createdDeadline =
                ProductionEntityStressRequestProcessor.ResolveServiceWaitDeadline(
                    realtimeNow,
                    persistedDeadline: string.Empty);

            Assert.That(
                createdDeadline,
                Is.EqualTo(
                    realtimeNow +
                    ProductionEntityStressRequestProcessor
                        .ServiceWaitTimeoutSecondsForDiagnostics));
            Assert.That(
                ProductionEntityStressRequestProcessor.HasServiceWaitTimedOut(
                    createdDeadline - 0.001d,
                    createdDeadline),
                Is.False,
                "Any number of editor updates before the real-time deadline must keep waiting.");
            Assert.That(
                ProductionEntityStressRequestProcessor.HasServiceWaitTimedOut(
                    createdDeadline,
                    createdDeadline),
                Is.True);
        }

        [Test]
        public void ProductionServiceWaitDeadline_PreservesPersistedDeadlineAcrossDomainReload()
        {
            Assert.That(
                ProductionEntityStressRequestProcessor.ResolveServiceWaitDeadline(
                    realtimeNow: 500d,
                    persistedDeadline: "123.5"),
                Is.EqualTo(123.5d));
            Assert.That(
                ProductionEntityStressRequestProcessor.ResolveServiceWaitDeadline(
                    realtimeNow: 500d,
                    persistedDeadline: "not-a-number"),
                Is.EqualTo(
                    500d +
                    ProductionEntityStressRequestProcessor
                        .ServiceWaitTimeoutSecondsForDiagnostics));
        }

        [Test]
        public void ReloadRecoveryPolicy_AllowsOneCompleteActiveRunRetryThenFailsClosed()
        {
            Assert.That(
                ProductionEntityStressRequestProcessor.EvaluateReloadRecoveryDecision(
                    hasCompleteActiveState: false,
                    runnerActive: false,
                    recoveryCount: 0),
                Is.EqualTo(ProductionEntityStressReloadRecoveryDecision.None));
            Assert.That(
                ProductionEntityStressRequestProcessor.EvaluateReloadRecoveryDecision(
                    hasCompleteActiveState: true,
                    runnerActive: true,
                    recoveryCount: 0),
                Is.EqualTo(ProductionEntityStressReloadRecoveryDecision.RetryAfterCleanExit));
            Assert.That(
                ProductionEntityStressRequestProcessor.EvaluateReloadRecoveryDecision(
                    hasCompleteActiveState: true,
                    runnerActive: true,
                    recoveryCount: 1),
                Is.EqualTo(ProductionEntityStressReloadRecoveryDecision.TerminalFailure));
            Assert.That(
                ProductionEntityStressRequestProcessor.EvaluateReloadRecoveryDecision(
                    hasCompleteActiveState: false,
                    runnerActive: true,
                    recoveryCount: 0),
                Is.EqualTo(ProductionEntityStressReloadRecoveryDecision.TerminalFailure));
            Assert.That(
                ProductionEntityStressRequestProcessor.EvaluateReloadRecoveryDecision(
                    hasCompleteActiveState: true,
                    runnerActive: false,
                    recoveryCount: 0),
                Is.EqualTo(ProductionEntityStressReloadRecoveryDecision.TerminalFailure));
        }

        [Test]
        public void ReloadRecoveryTransition_ExitsThenReentersPlayMode()
        {
            Assert.That(
                ProductionEntityStressRequestProcessor.ResolveReloadRecoveryTransition(
                    recoveryPending: true,
                    isPlaying: true,
                    isPlayingOrWillChangePlaymode: true),
                Is.EqualTo(ProductionEntityStressReloadRecoveryTransition.ExitPlayMode));
            Assert.That(
                ProductionEntityStressRequestProcessor.ResolveReloadRecoveryTransition(
                    recoveryPending: true,
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false),
                Is.EqualTo(ProductionEntityStressReloadRecoveryTransition.EnterPlayMode));
            Assert.That(
                ProductionEntityStressRequestProcessor.ResolveReloadRecoveryTransition(
                    recoveryPending: true,
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: true),
                Is.EqualTo(ProductionEntityStressReloadRecoveryTransition.Wait));
        }

        [Test]
        public void ReloadRecoveryConfigJson_IsStableAndRejectsChangedRequest()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed100",
                inputMode = "ai",
                sampleTicks = 120,
                autoStopWhenSampled = false,
                outputPath = "Temp/reload-recovery.json",
            };
            string requestJson = JsonUtility.ToJson(request);
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            string first = ProductionEntityStressRequestProcessor.BuildActiveConfigJson(
                requestJson,
                config);
            string second = ProductionEntityStressRequestProcessor.BuildActiveConfigJson(
                requestJson,
                config);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(
                ProductionEntityStressRequestProcessor.IsCompleteActiveRunState(
                    requestJson,
                    first),
                Is.True);

            request.sampleTicks++;
            string changedRequestJson = JsonUtility.ToJson(request);
            Assert.That(
                ProductionEntityStressRequestProcessor.IsCompleteActiveRunState(
                    changedRequestJson,
                    first),
                Is.False);
        }

        [Test]
        public void LocalCatchUp_FourTicksBuildPresentationOnlyOnFinalTick()
        {
            float accumulator = SimulationConstants.SIM_DT * 4f;
            var flags = new bool[4];

            for (int tick = 0; tick < flags.Length; tick++)
            {
                accumulator -= SimulationConstants.SIM_DT;
                flags[tick] = SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LocalFreeRun,
                    requireInputFrameReady: false,
                    accumulator,
                    tick,
                    maxCatchUpTicks: 4);
            }

            CollectionAssert.AreEqual(
                new[] { false, false, false, true },
                flags);
        }

        [Test]
        public void LocalCatchUp_SingleTickBuildsPresentation()
        {
            float accumulator = SimulationConstants.SIM_DT;
            accumulator -= SimulationConstants.SIM_DT;

            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LocalFreeRun,
                    requireInputFrameReady: false,
                    accumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True);
        }

        [Test]
        public void InputReadyAndNonLocalModes_KeepEveryTickPresentationBuild()
        {
            float remainingAccumulator = SimulationConstants.SIM_DT * 3f;

            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LocalFreeRun,
                    requireInputFrameReady: true,
                    remainingAccumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True);
            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LockstepBuffered,
                    requireInputFrameReady: false,
                    remainingAccumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True);
            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.Manual,
                    requireInputFrameReady: false,
                    remainingAccumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True,
                "Manual driver ticks keep the public default build=true contract; " +
                "the stress harness opts into intermediate suppression explicitly.");
        }

        [Test]
        public void SmokeRequest_UsesFiftyEntitiesAndBoundedSampling()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                warmupTicks = 100,
                sampleTicks = 100,
                spawnBatchSize = 500,
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 2,
                outputPath = "Temp/smoke.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.Mode, Is.EqualTo(ProductionEntityStressMode.Smoke50));
            Assert.That(config.EntityCount, Is.EqualTo(50));
            Assert.That(config.WarmupTicks, Is.EqualTo(5));
            Assert.That(config.SampleTicks, Is.EqualTo(30));
            Assert.That(config.SpawnBatchSize, Is.EqualTo(100));
            Assert.That(config.MaxBacklogTicks, Is.EqualTo(4));
            Assert.That(
                config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(Path.IsPathRooted(config.OutputPath), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void InputMode_MissingOrEmptyDefaultsToAi(string inputMode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                inputMode = inputMode,
                outputPath = "Temp/input-default.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(
                ProductionEntityStressConfig.FormatInputMode(config.InputMode),
                Is.EqualTo("ai"));
        }

        [Test]
        public void InputMode_LegacyRequestWithoutFieldRemainsAi()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(
                    "{\"action\":\"smoke\",\"outputPath\":\"Temp/input-legacy.json\"}");

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(request.inputMode, Is.EqualTo("ai"));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
        }

        [Test]
        public void InputMode_NoneParsesAndDisablesAiPolicy()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed",
                inputMode = "none",
                outputPath = "Temp/input-none.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.None));
            Assert.That(
                config.InputMode == ProductionEntityStressInputMode.Ai,
                Is.False);
        }

        [Test]
        public void InputMode_MoveParsesWithoutEnablingAiPolicy()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed",
                inputMode = "move",
                outputPath = "Temp/input-move.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Move));
            Assert.That(
                ProductionEntityStressConfig.FormatInputMode(config.InputMode),
                Is.EqualTo("move"));
            Assert.That(
                config.InputMode == ProductionEntityStressInputMode.Ai,
                Is.False);
        }

        [Test]
        public void InputMode_UnknownValueIsRejected()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                inputMode = "human",
                outputPath = "Temp/input-invalid.json",
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("human"));
            Assert.That(exception.Message, Does.Contain("ai, move, or none"));
        }

        [Test]
        public void InputMode_ReportJsonRecordsActualMode()
        {
            var report = new ProductionEntityStressReport
            {
                inputMode = ProductionEntityStressConfig.FormatInputMode(
                    ProductionEntityStressInputMode.None),
            };

            string json = JsonUtility.ToJson(report);
            ProductionEntityStressReport roundTrip =
                JsonUtility.FromJson<ProductionEntityStressReport>(json);

            Assert.That(json, Does.Contain("\"inputMode\":\"none\""));
            Assert.That(roundTrip.inputMode, Is.EqualTo("none"));
        }

        [Test]
        public void FormalCollector_LegacyRequestDefaultsToConfigured()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(
                    "{\"action\":\"smoke\",\"outputPath\":\"Temp/formal-default.json\"}");

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);

            Assert.That(request.formalCollectorMode, Is.EqualTo("configured"));
            Assert.That(
                config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(
                query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(
                ProductionEntityStressRunner.ResolveAppliedFormalCollectorModeForDiagnostics(
                    world,
                    query),
                Is.EqualTo(CollisionFormalCollectorMode.ForceBruteForce));
        }

        [TestCase(
            CollisionBroadphaseBackend.LooseQuadtree,
            CollisionFormalCollectorMode.ForceRoleAware,
            "role")]
        [TestCase(
            CollisionBroadphaseBackend.BruteForce,
            CollisionFormalCollectorMode.ForceBruteForce,
            "brute")]
        public void FormalCollector_ConfiguredReportReflectsProductionBackend(
            CollisionBroadphaseBackend backend,
            CollisionFormalCollectorMode expectedAppliedMode,
            string expectedReportMode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                formalCollectorMode = "configured",
                outputPath = "Temp/formal-configured-report.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                backend);
            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);
            CollisionFormalCollectorMode applied =
                ProductionEntityStressRunner.ResolveAppliedFormalCollectorModeForDiagnostics(
                    world,
                    query);
            var report = new ProductionEntityStressReport
            {
                formalCollectorRequestedMode =
                    ProductionEntityStressConfig.FormatFormalCollectorMode(
                        config.FormalCollectorMode),
                formalCollectorMode =
                    ProductionEntityStressConfig.FormatFormalCollectorMode(applied),
            };

            Assert.That(config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(applied, Is.EqualTo(expectedAppliedMode));
            Assert.That(report.formalCollectorRequestedMode, Is.EqualTo("configured"));
            Assert.That(report.formalCollectorMode, Is.EqualTo(expectedReportMode));
        }

        [Test]
        public void FormalCollector_RoleRequestParsesAndAppliesToStressWorld()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                formalCollectorMode = "role",
                outputPath = "Temp/formal-role.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();

            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);

            Assert.That(
                config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
            Assert.That(
                query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
            Assert.That(
                ProductionEntityStressConfig.FormatFormalCollectorMode(
                    query.FormalCollectorMode),
                Is.EqualTo("role"));
        }

        [Test]
        public void RoleAwareBroadphase_LegacyRequestKeepsAdaptiveDefaultAndReportsDisabledForces()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(
                    "{\"action\":\"smoke\",\"outputPath\":\"Temp/role-broadphase-default.json\"}");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    CollisionFormalCollectorMode.ForceRoleAware);
            var report = new ProductionEntityStressReport();

            ProductionEntityStressRunner.ApplyRoleAwareBroadphaseDiagnosticsForDiagnostics(
                query,
                config,
                report);

            Assert.That(request.forceRoleAwareDirect, Is.False);
            Assert.That(request.forceRoleAwareTree, Is.False);
            Assert.That(request.forceRoleAwareNestedDirect, Is.False);
            Assert.That(request.forceRoleAwareSweepDirect, Is.False);
            Assert.That(config.ForceRoleAwareDirect, Is.False);
            Assert.That(config.ForceRoleAwareTree, Is.False);
            Assert.That(config.ForceRoleAwareNestedDirect, Is.False);
            Assert.That(config.ForceRoleAwareSweepDirect, Is.False);
            Assert.That(query.ForceRoleAwareDirectForDiagnostics, Is.False);
            Assert.That(query.ForceRoleAwareTreeForDiagnostics, Is.False);
            Assert.That(query.ForceRoleAwareNestedDirectForDiagnostics, Is.False);
            Assert.That(query.ForceRoleAwareSweepDirectForDiagnostics, Is.False);
            Assert.That(report.forceRoleAwareDirectRequested, Is.False);
            Assert.That(report.forceRoleAwareTreeRequested, Is.False);
            Assert.That(report.forceRoleAwareNestedDirectRequested, Is.False);
            Assert.That(report.forceRoleAwareSweepDirectRequested, Is.False);
            Assert.That(report.forceRoleAwareDirectApplied, Is.False);
            Assert.That(report.forceRoleAwareTreeApplied, Is.False);
            Assert.That(report.forceRoleAwareNestedDirectApplied, Is.False);
            Assert.That(report.forceRoleAwareSweepDirectApplied, Is.False);
            Assert.That(report.roleAwareDirectTickCount, Is.Zero);
            Assert.That(report.roleAwareTreeTickCount, Is.Zero);
            Assert.That(report.roleAwareNestedDirectTickCount, Is.Zero);
            Assert.That(report.roleAwareSweepDirectTickCount, Is.Zero);
            Assert.That(report.roleAwareSweepXCandidateCount, Is.Zero);
            Assert.That(report.roleAwareDirectComparisonCount, Is.Zero);
            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain("\"forceRoleAwareDirectRequested\":false"));
            Assert.That(json, Does.Contain("\"forceRoleAwareSweepDirectRequested\":false"));
            Assert.That(json, Does.Contain("\"roleAwareSweepXCandidateCount\":0"));
            Assert.That(json, Does.Contain("\"roleAwareDirectComparisonCount\":0"));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void RoleAwareBroadphase_ExplicitDirectOrTreeAppliesAndReports(
            bool forceDirect,
            bool forceTree)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                formalCollectorMode = "role",
                forceRoleAwareDirect = forceDirect,
                forceRoleAwareTree = forceTree,
                outputPath = "Temp/role-broadphase-explicit.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);
            var report = new ProductionEntityStressReport();

            ProductionEntityStressRunner.ApplyRoleAwareBroadphaseDiagnosticsForDiagnostics(
                query,
                config,
                report);

            Assert.That(config.ForceRoleAwareDirect, Is.EqualTo(forceDirect));
            Assert.That(config.ForceRoleAwareTree, Is.EqualTo(forceTree));
            Assert.That(query.ForceRoleAwareDirectForDiagnostics, Is.EqualTo(forceDirect));
            Assert.That(query.ForceRoleAwareTreeForDiagnostics, Is.EqualTo(forceTree));
            Assert.That(report.forceRoleAwareDirectRequested, Is.EqualTo(forceDirect));
            Assert.That(report.forceRoleAwareTreeRequested, Is.EqualTo(forceTree));
            Assert.That(report.forceRoleAwareDirectApplied, Is.EqualTo(forceDirect));
            Assert.That(report.forceRoleAwareTreeApplied, Is.EqualTo(forceTree));
        }

        [TestCase(true, false, false, "forceRoleAwareDirect")]
        [TestCase(false, true, false, "forceRoleAwareNestedDirect")]
        [TestCase(false, false, true, "forceRoleAwareSweepDirect")]
        [TestCase(true, true, false, "forceRoleAwareNestedDirect")]
        [TestCase(true, false, true, "forceRoleAwareSweepDirect")]
        public void RoleAwareBroadphase_TreeAndAnyDirectRouteAreRejectedClearly(
            bool forceDirect,
            bool forceNested,
            bool forceSweep,
            string conflictingField)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                forceRoleAwareDirect = forceDirect,
                forceRoleAwareTree = true,
                forceRoleAwareNestedDirect = forceNested,
                forceRoleAwareSweepDirect = forceSweep,
                outputPath = "Temp/role-broadphase-invalid.json",
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("forceRoleAwareTree"));
            Assert.That(exception.Message, Does.Contain(conflictingField));
            Assert.That(exception.Message, Does.Contain("mutually exclusive"));
        }

        [TestCase(false, true, false)]
        [TestCase(false, false, true)]
        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        public void RoleAwareBroadphase_ExplicitDirectRouteAppliesAndSerializes(
            bool forceDirect,
            bool forceNested,
            bool forceSweep)
        {
            string requestJson =
                "{\"action\":\"smoke\",\"formalCollectorMode\":\"role\"," +
                $"\"forceRoleAwareDirect\":{forceDirect.ToString().ToLowerInvariant()}," +
                $"\"forceRoleAwareNestedDirect\":{forceNested.ToString().ToLowerInvariant()}," +
                $"\"forceRoleAwareSweepDirect\":{forceSweep.ToString().ToLowerInvariant()}," +
                "\"outputPath\":\"Temp/role-direct-route.json\"}";
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(requestJson);
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);
            var report = new ProductionEntityStressReport();

            ProductionEntityStressRunner.ApplyRoleAwareBroadphaseDiagnosticsForDiagnostics(
                query,
                config,
                report);

            Assert.That(config.ForceRoleAwareDirect, Is.EqualTo(forceDirect));
            Assert.That(config.ForceRoleAwareNestedDirect, Is.EqualTo(forceNested));
            Assert.That(config.ForceRoleAwareSweepDirect, Is.EqualTo(forceSweep));
            Assert.That(
                query.ForceRoleAwareDirectForDiagnostics,
                Is.EqualTo(forceDirect));
            Assert.That(
                query.ForceRoleAwareNestedDirectForDiagnostics,
                Is.EqualTo(forceNested));
            Assert.That(
                query.ForceRoleAwareSweepDirectForDiagnostics,
                Is.EqualTo(forceSweep));
            Assert.That(
                report.forceRoleAwareNestedDirectRequested,
                Is.EqualTo(forceNested));
            Assert.That(
                report.forceRoleAwareSweepDirectRequested,
                Is.EqualTo(forceSweep));
            Assert.That(
                report.forceRoleAwareDirectApplied,
                Is.EqualTo(forceDirect));
            Assert.That(
                report.forceRoleAwareNestedDirectApplied,
                Is.EqualTo(forceNested));
            Assert.That(
                report.forceRoleAwareSweepDirectApplied,
                Is.EqualTo(forceSweep));
            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain("\"roleAwareNestedDirectTickCount\":0"));
            Assert.That(json, Does.Contain("\"roleAwareSweepDirectTickCount\":0"));
            Assert.That(json, Does.Contain("\"roleAwareSweepFullOverlapCheckCount\":0"));
        }

        [Test]
        public void RoleAwareBroadphase_NestedAndSweepTogetherAreRejectedClearly()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                forceRoleAwareNestedDirect = true,
                forceRoleAwareSweepDirect = true,
                outputPath = "Temp/role-direct-route-invalid.json",
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("forceRoleAwareNestedDirect"));
            Assert.That(exception.Message, Does.Contain("forceRoleAwareSweepDirect"));
            Assert.That(exception.Message, Does.Contain("mutually exclusive"));
        }

        [Test]
        public void RoleAwareBroadphase_ReportSerializesSweepCounters()
        {
            var report = new ProductionEntityStressReport
            {
                roleAwareNestedDirectTickCount = 3,
                roleAwareSweepDirectTickCount = 5,
                roleAwareLastSweepDirectTickCount = 1,
                roleAwareSweepXCandidateCount = 144,
                roleAwareLastSweepXCandidateCount = 21,
                roleAwareSweepFullOverlapCheckCount = 144,
                roleAwareLastSweepFullOverlapCheckCount = 21,
            };

            string json = JsonUtility.ToJson(report);

            Assert.That(json, Does.Contain("\"roleAwareNestedDirectTickCount\":3"));
            Assert.That(json, Does.Contain("\"roleAwareSweepDirectTickCount\":5"));
            Assert.That(json, Does.Contain("\"roleAwareSweepXCandidateCount\":144"));
            Assert.That(
                json,
                Does.Contain("\"roleAwareSweepFullOverlapCheckCount\":144"));
        }

        [Test]
        public void RoleAwareDirectCost_DefaultAndUnavailableTickRemainZeroInJson()
        {
            var report = new ProductionEntityStressReport
            {
                roleAwareDirectCostTickScope =
                    "All successful logic ticks including warmup; mirrors role-aware direct/tree total tick counters.",
            };

            ProductionEntityStressRunner.AccumulateRoleAwareDirectCostForReport(
                report,
                directCost: 999999L,
                available: false);

            Assert.That(report.roleAwareDirectCostObservedTickCount, Is.Zero);
            Assert.That(report.roleAwareDirectCostSum, Is.Zero);
            Assert.That(report.roleAwareDirectCostMax, Is.Zero);
            Assert.That(report.roleAwareDirectCostAbove32768TickCount, Is.Zero);
            Assert.That(report.roleAwareDirectCostAbove65536TickCount, Is.Zero);
            Assert.That(report.roleAwareDirectCostAbove131072TickCount, Is.Zero);
            Assert.That(report.roleAwareDirectCostAbove262144TickCount, Is.Zero);
            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain("\"roleAwareDirectCostObservedTickCount\":0"));
            Assert.That(json, Does.Contain("\"roleAwareDirectCostSum\":0"));
            Assert.That(json, Does.Contain("including warmup"));
        }

        [Test]
        public void RoleAwareDirectCost_AggregatesStrictThresholdBucketsAndJson()
        {
            var report = new ProductionEntityStressReport();
            long[] costs = { 32768L, 32769L, 65537L, 131073L, 262145L };
            for (int i = 0; i < costs.Length; i++)
            {
                ProductionEntityStressRunner.AccumulateRoleAwareDirectCostForReport(
                    report,
                    costs[i],
                    available: true);
            }

            Assert.That(report.roleAwareDirectCostObservedTickCount, Is.EqualTo(5));
            Assert.That(report.roleAwareDirectCostSum, Is.EqualTo(524292L));
            Assert.That(report.roleAwareDirectCostMax, Is.EqualTo(262145L));
            Assert.That(report.roleAwareDirectCostAbove32768TickCount, Is.EqualTo(4));
            Assert.That(report.roleAwareDirectCostAbove65536TickCount, Is.EqualTo(3));
            Assert.That(report.roleAwareDirectCostAbove131072TickCount, Is.EqualTo(2));
            Assert.That(report.roleAwareDirectCostAbove262144TickCount, Is.EqualTo(1));
            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain("\"roleAwareDirectCostSum\":524292"));
            Assert.That(json, Does.Contain("\"roleAwareDirectCostMax\":262145"));
            Assert.That(json, Does.Contain("\"roleAwareDirectCostAbove262144TickCount\":1"));
        }

        [Test]
        public void FormalCollector_UnknownValueIsRejected()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                formalCollectorMode = "adaptive",
                outputPath = "Temp/formal-invalid.json",
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("adaptive"));
            Assert.That(exception.Message, Does.Contain("configured, legacy, role, or brute"));
        }

        [Test]
        public void DetailTiming_LegacyRequestDefaultsToDisabledAndReportMarksItUnavailable()
        {
            ProductionEntityStressRequest request = JsonUtility.FromJson<ProductionEntityStressRequest>(
                "{\"action\":\"dispersed\",\"outputPath\":\"Temp/legacy.json\"}");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport
            {
                detailPhaseTimingEnabled = config.EnableDetailPhaseTiming,
            };

            new ProductionEntityStressDetailPhaseTimingCollector().PopulateReport(report);

            Assert.That(config.EnableDetailPhaseTiming, Is.False);
            Assert.That(report.detailPhaseTimingEnabled, Is.False);
            Assert.That(report.detailPhaseTimings, Is.Empty);
            Assert.That(report.detailPhaseTimingSource, Is.Empty);
            Assert.That(report.detailPhaseTimingUnavailableReason, Does.Contain("Disabled by request"));
            Assert.That(report.aiInputDetailTimings, Is.Empty);
            Assert.That(report.aiInputDetailTimingSource, Is.Empty);
            Assert.That(
                report.aiInputDetailTimingUnavailableReason,
                Does.Contain("Disabled by request"));
        }

        [Test]
        public void DetailTiming_LegacyReportJsonInitializesNewAiTimingFieldsAsUnavailable()
        {
            ProductionEntityStressReport report =
                JsonUtility.FromJson<ProductionEntityStressReport>(
                    "{\"schema\":\"ntsd-production-entity-stress/v1\"," +
                    "\"status\":\"StoppedCleanly\",\"detailPhaseTimingEnabled\":false}");

            new ProductionEntityStressDetailPhaseTimingCollector().PopulateReport(report);

            Assert.That(report.aiInputDetailTimings, Is.Not.Null);
            Assert.That(report.aiInputDetailTimings, Is.Empty);
            Assert.That(report.aiInputDetailCounters, Is.Not.Null);
            Assert.That(report.aiInputDetailCounters.available, Is.False);
            Assert.That(report.aiInputDetailTimingSource, Is.Empty);
            Assert.That(
                report.aiInputDetailTimingUnavailableReason,
                Does.Contain("Disabled by request"));
        }

        [Test]
        public void DetailTiming_RequestExplicitlyEnablesNestedDiagnostics()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed",
                enableDetailPhaseTiming = true,
                outputPath = "Temp/detail-timing.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.EnableDetailPhaseTiming, Is.True);
        }

        [Test]
        public void CandidateCollectDetailTiming_PhaseNamesAndCountAreStable()
        {
            BattleTickDetailPhase[] phases =
            {
                BattleTickDetailPhase.CandidateCollectCacheSetup,
                BattleTickDetailPhase.CandidateCollectParticipantBodyItrBuild,
                BattleTickDetailPhase.CandidateCollectInputValidation,
                BattleTickDetailPhase.CandidateCollectDirectBroadphase,
                BattleTickDetailPhase.CandidateCollectTreeBroadphase,
                BattleTickDetailPhase.CandidateCollectFallbackPairAdd,
                BattleTickDetailPhase.CandidateCollectSortDeduplicate,
                BattleTickDetailPhase.CandidateCollectPairExactLoop,
            };
            string[] expectedNames =
            {
                "CandidateCollect/CacheSetup",
                "CandidateCollect/ParticipantBodyItrBuild",
                "CandidateCollect/InputValidation",
                "CandidateCollect/DirectBroadphase",
                "CandidateCollect/TreeBroadphase",
                "CandidateCollect/FallbackPairAdd",
                "CandidateCollect/SortDeduplicate",
                "CandidateCollect/PairExactLoop",
            };

            Assert.That(BattleTickDetailPhaseDiagnostics.PhaseCount, Is.EqualTo(40));
            for (int phaseIndex = 0; phaseIndex < phases.Length; phaseIndex++)
            {
                Assert.That(
                    BattleTickDetailPhaseDiagnostics.GetPhaseName(phases[phaseIndex]),
                    Is.EqualTo(expectedNames[phaseIndex]));
            }
        }

        [Test]
        public void CandidateCollectDetailTiming_DefaultOffDoesNotAllocateOrCreateRecorder()
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;

            query.CollectCollisionCandidates();
            query.EndCollisionCandidateConsumption();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 32; iteration++)
            {
                query.CollectCollisionCandidates();
                query.EndCollisionCandidateConsumption();
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(
                world.BattleTickDetailPhaseDiagnosticsAllocatedForDiagnostics,
                Is.False);
            Assert.That(
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics,
                Is.Null);
        }

        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, true, false)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, false, true)]
        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, false, false)]
        public void CandidateCollectDetailTiming_EnablementPreservesSequenceAndRng(
            CollisionFormalCollectorMode collectorMode,
            bool forceDirect,
            bool forceTree)
        {
            CreateCandidateCollectTimingFixture(
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);

            CandidateCollectTimingRun baseline = RunCandidateCollectTimingFixture(
                world,
                query,
                attacker,
                collectorMode,
                forceDirect,
                forceTree);

            BattleTickDetailPhaseDiagnostics recorder =
                world.EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            recorder.BeginTick(77);
            CandidateCollectTimingRun timed = RunCandidateCollectTimingFixture(
                world,
                query,
                attacker,
                collectorMode,
                forceDirect,
                forceTree);

            AssertCandidateCollectTimingRunsEqual(baseline, timed);
            Assert.That(timed.RngCalls, Is.GreaterThan(0));
            Assert.That(recorder.LastTickIndex, Is.EqualTo(77));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.CandidateCollectCacheSetup),
                Is.GreaterThan(0));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.CandidateCollectPairExactLoop),
                Is.GreaterThan(0));

            if (collectorMode == CollisionFormalCollectorMode.ForceRoleAware)
            {
                BattleTickDetailPhase broadphase = forceDirect
                    ? BattleTickDetailPhase.CandidateCollectDirectBroadphase
                    : BattleTickDetailPhase.CandidateCollectTreeBroadphase;
                Assert.That(
                    recorder.GetLastElapsedTimestampTicks(broadphase),
                    Is.GreaterThan(0));
            }
        }

        [Test]
        public void CandidateListPool_ThousandEligibleAttackersReuseWarmCapacity()
        {
            const int entityCount = 1000;
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                CollisionBroadphaseBackend.LooseQuadtree);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            LF2FrameData attackerFrame = CreateCandidateCollectTimingFrame(
                new InteractionArea
                {
                    kind = 0,
                    vrest = 0,
                    x = -1,
                    y = -1,
                    w = 2,
                    h = 2,
                    zwidth = 1,
                },
                body: null);

            for (int entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                LF2Character entity = CreateCandidateCollectTimingCharacter(
                    "CandidatePool1000_" + entityIndex,
                    3000 + entityIndex,
                    attackerFrame);
                entity.SetRequiredRuntimeSlot(entityIndex);
                world.Register(entity);
            }
            Assert.That(
                world.ClaimedRuntimeSlotCountForDiagnostics,
                Is.EqualTo(entityCount));
            world.CaptureCollisionFrameSnapshotsAll();

            query.CollectCollisionCandidates();
            Assert.That(
                query.ActiveCandidateListCountForDiagnostics,
                Is.EqualTo(entityCount));
            Assert.That(
                query.CandidateListCreatedCountForDiagnostics,
                Is.EqualTo(entityCount));
            query.EndCollisionCandidateConsumption();
            Assert.That(query.CandidateListPoolCountForDiagnostics, Is.EqualTo(entityCount));

            long createdAfterWarmup = query.CandidateListCreatedCountForDiagnostics;
            long reusedBefore = query.CandidateListReusedCountForDiagnostics;
            query.CollectCollisionCandidates();

            Assert.That(
                query.CandidateListCreatedCountForDiagnostics,
                Is.EqualTo(createdAfterWarmup));
            Assert.That(
                query.CandidateListReusedCountForDiagnostics - reusedBefore,
                Is.EqualTo(entityCount));
            Assert.That(
                query.ActiveCandidateListCountForDiagnostics,
                Is.EqualTo(entityCount));
            query.EndCollisionCandidateConsumption();
            Assert.That(query.CandidateListPoolCountForDiagnostics, Is.EqualTo(entityCount));
        }

        [Test]
        public void CandidateListPool_EntityAndEligibilityChangesDoNotLeakOldCandidates()
        {
            CreateCandidateCollectTimingFixture(
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
            LF2FrameData attackerFrame = attacker.Frame.D;
            world.Rng.Seed(0xC011EC7u);
            world.CaptureCollisionFrameSnapshotsAll();
            query.CollectCollisionCandidates();
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    attacker,
                    out List<SceneQueryHit> firstCandidates),
                Is.True);
            var expectedCandidates = new List<SceneQueryHit>(firstCandidates);
            Assert.That(expectedCandidates, Is.Not.Empty);
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange oldRange),
                Is.True);
            Assert.That(oldRange.Count, Is.EqualTo(expectedCandidates.Count));
            Assert.That(oldRange.TryGet(0, out _), Is.True);
            long createdAfterWarmup = query.CandidateListCreatedCountForDiagnostics;
            query.EndCollisionCandidateConsumption();
            Assert.That(oldRange.Count, Is.Zero);
            Assert.That(oldRange.TryGet(0, out _), Is.False);

            world.Unregister(attacker);
            world.CaptureCollisionFrameSnapshotsAll();
            query.CollectCollisionCandidates();
            Assert.That(query.ActiveCandidateListCountForDiagnostics, Is.Zero);
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    attacker,
                    out List<SceneQueryHit> removedCandidates),
                Is.True);
            Assert.That(removedCandidates, Is.Empty);
            query.EndCollisionCandidateConsumption();

            LF2Character replacement = CreateCandidateCollectTimingCharacter(
                "CandidatePool_Replacement",
                3010,
                attackerFrame);
            RegisterCandidateCollectTimingEntity(world, replacement, 0, 1, 0);
            replacement.Runtime.SuppressCollisionCandidateUntilTick =
                world.CurrentTickIndex + 1;
            world.CaptureCollisionFrameSnapshotsAll();
            query.CollectCollisionCandidates();
            Assert.That(query.ActiveCandidateListCountForDiagnostics, Is.Zero);
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    replacement,
                    out List<SceneQueryHit> suppressedCandidates),
                Is.True);
            Assert.That(suppressedCandidates, Is.Empty);
            query.EndCollisionCandidateConsumption();

            replacement.Runtime.SuppressCollisionCandidateUntilTick = 0;
            world.Rng.Seed(0xC011EC7u);
            world.CaptureCollisionFrameSnapshotsAll();
            query.CollectCollisionCandidates();
            Assert.That(oldRange.Count, Is.Zero);
            Assert.That(oldRange.TryGet(0, out _), Is.False);
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    replacement,
                    out List<SceneQueryHit> replacementCandidates),
                Is.True);
            Assert.That(
                query.CandidateListCreatedCountForDiagnostics,
                Is.EqualTo(createdAfterWarmup));
            Assert.That(replacementCandidates, Has.Count.EqualTo(expectedCandidates.Count));
            for (int candidateIndex = 0;
                 candidateIndex < expectedCandidates.Count;
                 candidateIndex++)
            {
                Assert.That(
                    replacementCandidates[candidateIndex].TargetSlot,
                    Is.EqualTo(expectedCandidates[candidateIndex].TargetSlot));
                Assert.That(
                    replacementCandidates[candidateIndex].BodyX,
                    Is.EqualTo(expectedCandidates[candidateIndex].BodyX));
                Assert.That(
                    replacementCandidates[candidateIndex].ItrIndex,
                    Is.EqualTo(expectedCandidates[candidateIndex].ItrIndex));
            }
            query.EndCollisionCandidateConsumption();
        }

        [Test]
        public void CandidateListPool_ReusedListsPreserveSequenceAndRng()
        {
            CreateCandidateCollectTimingFixture(
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);

            CandidateCollectTimingRun baseline = RunCandidateCollectTimingFixture(
                world,
                query,
                attacker,
                CollisionFormalCollectorMode.ForceBruteForce,
                forceDirect: false,
                forceTree: false);
            long createdAfterWarmup = query.CandidateListCreatedCountForDiagnostics;
            long reusedBefore = query.CandidateListReusedCountForDiagnostics;
            CandidateCollectTimingRun reused = RunCandidateCollectTimingFixture(
                world,
                query,
                attacker,
                CollisionFormalCollectorMode.ForceBruteForce,
                forceDirect: false,
                forceTree: false);

            AssertCandidateCollectTimingRunsEqual(baseline, reused);
            Assert.That(
                query.CandidateListCreatedCountForDiagnostics,
                Is.EqualTo(createdAfterWarmup));
            Assert.That(
                query.CandidateListReusedCountForDiagnostics,
                Is.GreaterThan(reusedBefore));
        }

        [Test]
        public void LegacyRequest_DefaultsToProductionSafeDiagnosticsAndAiProtocol()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(
                    "{\"action\":\"dispersed\",\"outputPath\":\"Temp/legacy-safe.json\"}");

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.EnablePhaseTiming, Is.False);
            Assert.That(config.EnablePresentationTiming, Is.False);
            Assert.That(config.EnableDetailPhaseTiming, Is.False);
            Assert.That(config.SimulationOnly, Is.False);
            Assert.That(config.AutoStopWhenSampled, Is.False);
            Assert.That(config.ShouldAutoStopWhenSampled, Is.False);
            Assert.That(config.Seed, Is.EqualTo(0x4E545344u));
            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.LegacyAiSensing));
            Assert.That(config.AllowUnsafeAiSoACandidate, Is.False);
            Assert.That(request.aiSensingMode, Is.EqualTo("legacy"));
            Assert.That(request.allowUnsafeAiSoACandidate, Is.False);
            Assert.That(request.enableAiSoADecisionRemainder, Is.False);
            Assert.That(request.enableAiDecisionSoAShadow, Is.False);
            Assert.That(request.enableAiDecisionSharedShadow, Is.False);
            Assert.That(config.EnableAiDecisionSoAShadow, Is.False);
            Assert.That(config.EnableAiDecisionSharedShadow, Is.False);
            Assert.That(config.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.Legacy));
            Assert.That(config.AiDecisionFullOracleSampleInterval, Is.Zero);
            Assert.That(config.EnableUnifiedAiSnapshotShadow, Is.False);
            Assert.That(config.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.LegacySeparate));
            Assert.That(request.aiUnifiedSnapshotExecutionMode, Is.EqualTo("legacy"));
            Assert.That(
                config.RequestedAiDecisionShadowMode,
                Is.EqualTo(AiDecisionShadowMode.Disabled));
            Assert.That(request.writeFinalParitySnapshotJson, Is.False);
        }

        [Test]
        public void AiDecisionSoAShadow_RequestIsIndependentFromSensingCandidate()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    aiSensingMode = "legacy",
                    enableAiDecisionSoAShadow = true,
                    outputPath = "Temp/ai-decision-soa-shadow.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.LegacyAiSensing));
            Assert.That(config.EnableAiSoADecisionRemainder, Is.False);
            Assert.That(config.EnableAiDecisionSoAShadow, Is.True);
            Assert.That(config.EnableAiDecisionSharedShadow, Is.False);
            Assert.That(
                config.RequestedAiDecisionShadowMode,
                Is.EqualTo(AiDecisionShadowMode.Shadow));
        }

        [Test]
        public void AiDecisionSharedShadow_RequestSelectsExplicitSharedMode()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    aiSensingMode = "legacy",
                    enableAiDecisionSharedShadow = true,
                    outputPath = "Temp/ai-decision-shared-shadow.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.LegacyAiSensing));
            Assert.That(config.EnableAiDecisionSoAShadow, Is.False);
            Assert.That(config.EnableAiDecisionSharedShadow, Is.True);
            Assert.That(
                config.RequestedAiDecisionShadowMode,
                Is.EqualTo(AiDecisionShadowMode.SharedShadow));
        }

        [Test]
        public void AiDecisionShadow_RequestRejectsAmbiguousDeepAndSharedModes()
        {
            Assert.Throws<ArgumentException>(() => ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    enableAiDecisionSoAShadow = true,
                    enableAiDecisionSharedShadow = true,
                    outputPath = "Temp/ai-decision-shadow-ambiguous.json",
                },
                ProductionEntityStressPaths.ProjectRoot));
        }

        [Test]
        public void AiDecisionIndexedCanonical_RequestSelectsCandidateAndSampling()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "dispersed1000",
                    aiDecisionExecutionMode = "indexed-canonical",
                    aiDecisionFullOracleSampleInterval = 1000,
                    outputPath = "Temp/ai-decision-indexed-canonical.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.IndexedCanonical));
            Assert.That(config.AiDecisionFullOracleSampleInterval, Is.EqualTo(1000));
            Assert.That(config.RequestedAiDecisionShadowMode,
                Is.EqualTo(AiDecisionShadowMode.Disabled));
        }

        [Test]
        public void UnifiedAiSnapshotShadow_RequestSelectsOptInGateAObserver()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "dispersed1000",
                    aiSensingMode = "candidate",
                    allowUnsafeAiSoACandidate = true,
                    aiDecisionExecutionMode = "indexed-canonical",
                    enableUnifiedAiSnapshotShadow = true,
                    outputPath = "Temp/unified-ai-snapshot-shadow.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.EnableUnifiedAiSnapshotShadow, Is.True);
            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
            Assert.That(config.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.IndexedCanonical));
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_RequestSelectsOptInGateBExecution()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "dispersed1000",
                    aiSensingMode = "candidate",
                    allowUnsafeAiSoACandidate = true,
                    aiDecisionExecutionMode = "indexed-canonical",
                    aiUnifiedSnapshotExecutionMode = "unified-authority",
                    outputPath = "Temp/unified-ai-snapshot-authority.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.UnifiedAuthority));
            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
            Assert.That(config.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.IndexedCanonical));
            Assert.That(config.EnableUnifiedAiSnapshotShadow, Is.False);
        }

        [TestCase("legacy", "indexed-canonical")]
        [TestCase("candidate", "legacy")]
        public void UnifiedAiSnapshotAuthority_RejectsMissingPrerequisite(
            string aiSensingMode,
            string aiDecisionExecutionMode)
        {
            Assert.Throws<ArgumentException>(() => ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "dispersed1000",
                    aiSensingMode = aiSensingMode,
                    allowUnsafeAiSoACandidate = aiSensingMode == "candidate",
                    aiDecisionExecutionMode = aiDecisionExecutionMode,
                    aiUnifiedSnapshotExecutionMode = "unified-authority",
                    outputPath = "Temp/unified-ai-snapshot-authority-invalid.json",
                },
                ProductionEntityStressPaths.ProjectRoot));
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_RejectsGateAShadowCombination()
        {
            Assert.Throws<ArgumentException>(() => ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "dispersed1000",
                    aiSensingMode = "candidate",
                    allowUnsafeAiSoACandidate = true,
                    aiDecisionExecutionMode = "indexed-canonical",
                    enableUnifiedAiSnapshotShadow = true,
                    aiUnifiedSnapshotExecutionMode = "unified-authority",
                    outputPath = "Temp/unified-ai-snapshot-gates-ambiguous.json",
                },
                ProductionEntityStressPaths.ProjectRoot));
        }

        [Test]
        public void AiDecisionIndexedCanonical_RequestRejectsFullShadowCombination()
        {
            Assert.Throws<ArgumentException>(() => ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    aiDecisionExecutionMode = "indexed-canonical",
                    enableAiDecisionSharedShadow = true,
                    outputPath = "Temp/ai-decision-indexed-shadow-ambiguous.json",
                },
                ProductionEntityStressPaths.ProjectRoot));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AiDecisionShadow_InitialZeroCountersRemainValid(bool shared)
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                aiDecisionSoAShadowRequested = !shared,
                aiDecisionSharedShadowRequested = shared,
                aiDecisionSoAShadowApplied = !shared,
                aiDecisionSharedShadowApplied = shared,
                aiDecisionExecutionRequestedMode =
                    AiDecisionExecutionMode.Legacy.ToString(),
                aiUnifiedSnapshotExecutionRequestedMode =
                    AiUnifiedSnapshotExecutionMode.LegacySeparate.ToString(),
                logicTicksExecuted = 1,
            };

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiDecisionShadowValidityForReport(
                        report,
                        terminal: false),
                Is.True);
            Assert.That(report.harnessValidity, Is.True);
            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: false),
                Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.False,
                "an unrequested unified-authority mode must not claim authority success");
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackObserved, Is.False);
            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotRollbackContractForReport(
                        report,
                        terminal: true),
                Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackContractSatisfied,
                Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AiDecisionShadow_TerminalZeroComparisonsAreInvalid(bool shared)
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                aiDecisionSoAShadowRequested = !shared,
                aiDecisionSharedShadowRequested = shared,
                aiDecisionSoAShadowApplied = !shared,
                aiDecisionSharedShadowApplied = shared,
            };

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiDecisionShadowValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void AiDecisionShadow_TerminalCounterDecompositionMismatchIsInvalid()
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                aiDecisionSharedShadowRequested = true,
                aiDecisionSharedShadowApplied = true,
                logicTicksExecuted = 60,
                aiDecisionSoAShadowEligibleCount = 59000,
                aiDecisionSoAShadowAvailableCount = 58999,
                aiDecisionSoAShadowComparedCount = 58999,
                aiDecisionSharedShadowBuildCount = 59,
                aiDecisionSharedShadowRefreshCount = 59000,
            };

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiDecisionShadowValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [TestCase(58, 59000)]
        [TestCase(59, 58999)]
        public void AiDecisionSharedShadow_TerminalSharedAccountingMismatchIsInvalid(
            long buildCount,
            long refreshCount)
        {
            ProductionEntityStressReport report = CreateValidSharedShadowPressureReport();
            report.aiDecisionSharedShadowBuildCount = buildCount;
            report.aiDecisionSharedShadowRefreshCount = refreshCount;

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void AiDecisionSharedShadow_Current59000Over59TerminalCountersAreValid()
        {
            ProductionEntityStressReport report = CreateValidSharedShadowPressureReport();

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.True);
            Assert.That(report.harnessValidity, Is.True);
        }

        [Test]
        public void AiDecisionIndexedCanonical_CanonicalOnlyTerminalCountersAreValid()
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                logicTicksExecuted = 60,
                aiDecisionExecutionRequestedMode =
                    AiDecisionExecutionMode.IndexedCanonical.ToString(),
                aiDecisionExecutionEffectiveMode =
                    AiDecisionExecutionMode.IndexedCanonical.ToString(),
                aiDecisionIndexedCanonicalEligibleCount = 59000,
                aiDecisionIndexedCanonicalCommittedCount = 59000,
            };

            Assert.That(report.aiDecisionSoAShadowEligibleCount, Is.Zero);
            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.True,
                "Canonical-only evidence must not require disabled Deep/Shared shadow counters.");
            Assert.That(report.harnessValidity, Is.True);
        }

        [Test]
        public void AiDecisionIndexedCanonical_FallbackStillFailsTerminalValidity()
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                logicTicksExecuted = 60,
                aiDecisionExecutionRequestedMode =
                    AiDecisionExecutionMode.IndexedCanonical.ToString(),
                aiDecisionExecutionEffectiveMode =
                    AiDecisionExecutionMode.IndexedCanonical.ToString(),
                aiDecisionIndexedCanonicalEligibleCount = 59000,
                aiDecisionIndexedCanonicalCommittedCount = 58999,
                aiDecisionIndexedCanonicalFallbackCount = 1,
            };

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void UnifiedAiSnapshotShadow_ExactTerminalClosurePasses()
        {
            ProductionEntityStressReport report = CreateValidUnifiedCanonicalPressureReport();

            Assert.That(
                ProductionEntityStressRunner.ResolveExpectedUnifiedAiSnapshotObservedPassCount(60),
                Is.EqualTo(59));
            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.True);
            Assert.That(report.harnessValidity, Is.True);
        }

        [Test]
        public void UnifiedAiSnapshotShadow_CaptureBeforeDoesNotPoisonThenRestoredTerminalPasses()
        {
            ProductionEntityStressReport report = CreateValidUnifiedCanonicalPressureReport();
            report.unifiedAiSnapshotShadowRestored = false;
            report.teardown.attempted = true;
            report.teardown.restored = false;

            bool captureBeforeTerminal = ProductionEntityStressRunner
                .ShouldEvaluateAiDecisionShadowAsTerminalForReport(report);
            Assert.That(captureBeforeTerminal, Is.False);
            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    captureBeforeTerminal),
                Is.True);
            Assert.That(report.harnessValidity, Is.True);

            report.unifiedAiSnapshotShadowRestored = true;
            report.teardown.restored = true;
            bool restoredTerminal = ProductionEntityStressRunner
                .ShouldEvaluateAiDecisionShadowAsTerminalForReport(report);
            Assert.That(restoredTerminal, Is.True);
            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    restoredTerminal),
                Is.True);
            Assert.That(report.harnessValidity, Is.True);
        }

        [Test]
        public void UnifiedAiSnapshotShadow_TerminalMissingRestoreFails()
        {
            ProductionEntityStressReport report = CreateValidUnifiedCanonicalPressureReport();
            report.unifiedAiSnapshotShadowRestored = false;
            report.teardown.attempted = true;
            report.teardown.restored = true;

            bool terminal = ProductionEntityStressRunner
                .ShouldEvaluateAiDecisionShadowAsTerminalForReport(report);
            Assert.That(terminal, Is.True);
            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void UnifiedAiSnapshotShadow_MutationWitnessAndDerivedVisitClosurePasses()
        {
            ProductionEntityStressReport report =
                CreateValidUnifiedDualConsumerPressureReport();

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.True);
            Assert.That(report.unifiedAiSnapshotShadowMutationWitnessComparedCount,
                Is.EqualTo(2L * report.unifiedAiSnapshotShadowRefreshCount));
            Assert.That(report.unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount,
                Is.Zero);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain(
                "\"unifiedAiSnapshotShadowMutationWitnessComparedCount\":118000"));
            Assert.That(json, Does.Contain(
                "\"unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount\":0"));
            Assert.That(json, Does.Contain(
                "\"unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount\":"));
        }

        [Test]
        public void UnifiedAiSnapshotShadow_MissingMutationWitnessFailsTerminalClosure()
        {
            ProductionEntityStressReport report =
                CreateValidUnifiedDualConsumerPressureReport();
            report.unifiedAiSnapshotShadowMutationWitnessComparedCount--;

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void UnifiedAiSnapshotShadow_RefreshDerivedFullLoopFailsTerminalClosure()
        {
            ProductionEntityStressReport report =
                CreateValidUnifiedDualConsumerPressureReport();
            report.unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount = 1;

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void UnifiedAiSnapshotShadow_InitialDerivedVisitsAboveConservativeBoundFail()
        {
            ProductionEntityStressReport report =
                CreateValidUnifiedDualConsumerPressureReport();
            long maximumDerivedEntriesPerConsumerBuild =
                6L * report.runtimeSlotCapacity + 9L;
            report.unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount =
                report.unifiedAiSnapshotShadowBuildCount * 2L *
                maximumDerivedEntriesPerConsumerBuild + 1L;

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [TestCase("build")]
        [TestCase("refresh")]
        [TestCase("compare")]
        [TestCase("slot-visit")]
        [TestCase("exception")]
        public void UnifiedAiSnapshotShadow_AnyMissingExactEvidenceFails(string mutation)
        {
            ProductionEntityStressReport report = CreateValidUnifiedCanonicalPressureReport();
            switch (mutation)
            {
                case "build":
                    report.unifiedAiSnapshotShadowBuildCount--;
                    break;
                case "refresh":
                    report.unifiedAiSnapshotShadowRefreshCount--;
                    break;
                case "compare":
                    report.unifiedAiSnapshotShadowDecisionComparedCount--;
                    break;
                case "slot-visit":
                    report.unifiedAiSnapshotShadowSlotVisitCount--;
                    break;
                case "exception":
                    report.unifiedAiSnapshotShadowExceptionCount = 1;
                    report.unifiedAiSnapshotShadowFirstExceptionStage =
                        AiUnifiedSnapshotExceptionStage.Capture.ToString();
                    report.unifiedAiSnapshotShadowFirstExceptionType =
                        typeof(InvalidOperationException).FullName;
                    break;
                default:
                    Assert.Fail("Unknown unified shadow mutation: " + mutation);
                    break;
            }

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_ExactTerminalClosureAndReportFieldsPass()
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.True);
            Assert.That(report.harnessValidity, Is.True);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionRequestedMode\":\"UnifiedAuthority\""));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionBuildCount\":59"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionSlotVisitCount\":61950"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionRefreshCount\":59000"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionReadCount\":59000"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionPostCommitHardBreachCount\":0"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionLegacyCandidateRefreshCount\":0"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionLegacyDecisionSharedRefreshCount\":0"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionLegacyShadowRefreshCount\":0"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionLegacySnapshotMutationCount\":0"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionAuthoritySuccess\":true"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionRollbackObserved\":false"));
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "dispersed1000",
                    aiSensingMode = "candidate",
                    allowUnsafeAiSoACandidate = true,
                    aiDecisionExecutionMode = "indexed-canonical",
                    aiUnifiedSnapshotExecutionMode = "unified-authority",
                    outputPath = "Temp/unified-ai-snapshot-capture.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                CollisionBroadphaseBackend.LooseQuadtree);
            var report = new ProductionEntityStressReport
            {
                aiUnifiedSnapshotExecutionFirstFailureStage =
                    AiUnifiedSnapshotExceptionStage.None.ToString(),
                aiUnifiedSnapshotExecutionFirstFailureType = string.Empty,
            };
            LF2Character character = null;

            try
            {
                Assert.That(world.ObjectCount, Is.Zero);
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero);
                ProductionEntityStressRunner.ApplyAiSensingConfigurationForDiagnostics(
                    world,
                    config,
                    report);
                world.AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
                world.AiUnifiedSnapshotExecutionMode =
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority;
                world.ResetAiDecisionShadowDiagnostics();
                world.ResetAiUnifiedSnapshotShadowDiagnostics();
                world.ResetAiUnifiedSnapshotExecutionDiagnostics();

                LF2FrameData frame = CreateCandidateCollectTimingFrame(
                    itr: null,
                    body: null);
                frame.state = 2;
                character = CreateCandidateCollectTimingCharacter(
                    "UnifiedAuthorityCapture",
                    7,
                    frame);
                Assert.That(world.ObjectCount, Is.Zero,
                    "fixture creation must not implicitly register the character");
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero,
                    "fixture creation must not claim a runtime slot before explicit registration");
                Assert.That(character, Is.Not.Null);
                Assert.That(character.Runtime, Is.Not.Null);
                Assert.That(character.Frame?.D, Is.Not.Null);
                Assert.That(character.Controller, Is.Not.Null);
                character.AiControlled = true;
                character.Runtime.HP3 = 100;
                character.Runtime.PP = 0;
                character.Runtime.KillCount = -1;
                character.Runtime.Unk3FC = -1001;
                character.Runtime.Unk400 = -1001;
                RegisterCandidateCollectTimingEntity(world, character, 0, 1, 0);
                Assert.That(character.Runtime.SlotIndex, Is.EqualTo(0));

                world.CharacterInputAll(2);
                ProductionEntityStressRunner
                    .CaptureAiDecisionSoAShadowDiagnosticsForReport(report, world);

                Assert.That(report.aiUnifiedSnapshotExecutionBuildCount, Is.EqualTo(1));
                Assert.That(
                    report.aiUnifiedSnapshotExecutionSlotVisitCount,
                    Is.EqualTo(BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity));
                Assert.That(report.aiUnifiedSnapshotExecutionCommittedPassCount,
                    Is.EqualTo(1));
                Assert.That(report.aiUnifiedSnapshotExecutionRefreshCount, Is.EqualTo(1));
                Assert.That(report.aiUnifiedSnapshotExecutionReadCount, Is.EqualTo(1));
                Assert.That(report.aiUnifiedSnapshotExecutionLegacyFusedSensingBuildCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacyDecisionSharedBuildCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacyShadowBuildCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacyNearestFactsBuildCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacySnapshotIndexBuildCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacyQuadtreeSyncCount,
                    Is.Zero);
                Assert.That(
                    report.aiUnifiedSnapshotExecutionLegacyDecisionSharedRefreshCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacyShadowRefreshCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacySnapshotMutationCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionLegacyCandidateRefreshCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionPostCommitHardBreachCount,
                    Is.Zero);
                Assert.That(report.aiUnifiedSnapshotExecutionFirstFailureStage,
                    Is.EqualTo(AiUnifiedSnapshotExceptionStage.None.ToString()));
                Assert.That(report.aiUnifiedSnapshotExecutionFirstFailureType, Is.Empty);
            }
            finally
            {
                if (character != null)
                    world.Unregister(character);
                world.AiUnifiedSnapshotExecutionMode =
                    AiUnifiedSnapshotExecutionMode.LegacySeparate;
                ProductionEntityStressRunner
                    .CloseUnsafeAiSensingConfigurationForDiagnostics(world);
            }
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_DefaultLegacyModeDoesNotRequireGateBEvidence()
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                aiUnifiedSnapshotExecutionRequestedMode =
                    AiUnifiedSnapshotExecutionMode.LegacySeparate.ToString(),
            };

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.True);
            Assert.That(report.harnessValidity, Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.False);
        }

        [TestCase("build")]
        [TestCase("slot")]
        [TestCase("refresh")]
        [TestCase("read")]
        [TestCase("commit")]
        public void UnifiedAiSnapshotAuthority_MissingExactClosureEvidenceFails(
            string mutation)
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            switch (mutation)
            {
                case "build":
                    report.aiUnifiedSnapshotExecutionBuildCount--;
                    break;
                case "slot":
                    report.aiUnifiedSnapshotExecutionSlotVisitCount--;
                    break;
                case "refresh":
                    report.aiUnifiedSnapshotExecutionRefreshCount--;
                    break;
                case "read":
                    report.aiUnifiedSnapshotExecutionReadCount--;
                    break;
                case "commit":
                    report.aiUnifiedSnapshotExecutionCommittedPassCount--;
                    break;
                default:
                    Assert.Fail("Unknown Gate-B exact-closure mutation: " + mutation);
                    break;
            }

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [TestCase("precommit-failure")]
        [TestCase("precommit-fallback")]
        [TestCase("postcommit-breach")]
        [TestCase("first-failure")]
        public void UnifiedAiSnapshotAuthority_AnyFailureEvidencePreventsAuthoritySuccess(
            string failureEvidence)
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            switch (failureEvidence)
            {
                case "precommit-failure":
                    report.aiUnifiedSnapshotExecutionPreCommitFailureCount = 1;
                    break;
                case "precommit-fallback":
                    report.aiUnifiedSnapshotExecutionPreCommitFallbackCount = 1;
                    break;
                case "postcommit-breach":
                    report.aiUnifiedSnapshotExecutionPostCommitHardBreachCount = 1;
                    break;
                case "first-failure":
                    report.aiUnifiedSnapshotExecutionFirstFailureStage =
                        AiUnifiedSnapshotExceptionStage.Capture.ToString();
                    report.aiUnifiedSnapshotExecutionFirstFailureType =
                        typeof(InvalidOperationException).FullName;
                    break;
                default:
                    Assert.Fail("Unknown Gate-B failure evidence: " + failureEvidence);
                    break;
            }

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [TestCase("sensing")]
        [TestCase("decision")]
        [TestCase("shadow")]
        [TestCase("nearest")]
        [TestCase("index")]
        [TestCase("quadtree")]
        [TestCase("decision-refresh")]
        [TestCase("shadow-refresh")]
        [TestCase("legacy-mutation")]
        [TestCase("candidate-refresh")]
        public void UnifiedAiSnapshotAuthority_ReplacedLegacyPipelineActivityFailsClosure(
            string activity)
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            switch (activity)
            {
                case "sensing":
                    report.aiUnifiedSnapshotExecutionLegacyFusedSensingBuildCount = 1;
                    break;
                case "decision":
                    report.aiUnifiedSnapshotExecutionLegacyDecisionSharedBuildCount = 1;
                    break;
                case "shadow":
                    report.aiUnifiedSnapshotExecutionLegacyShadowBuildCount = 1;
                    break;
                case "nearest":
                    report.aiUnifiedSnapshotExecutionLegacyNearestFactsBuildCount = 1;
                    break;
                case "index":
                    report.aiUnifiedSnapshotExecutionLegacySnapshotIndexBuildCount = 1;
                    break;
                case "quadtree":
                    report.aiUnifiedSnapshotExecutionLegacyQuadtreeSyncCount = 1;
                    break;
                case "decision-refresh":
                    report.aiUnifiedSnapshotExecutionLegacyDecisionSharedRefreshCount = 1;
                    break;
                case "shadow-refresh":
                    report.aiUnifiedSnapshotExecutionLegacyShadowRefreshCount = 1;
                    break;
                case "legacy-mutation":
                    report.aiUnifiedSnapshotExecutionLegacySnapshotMutationCount = 1;
                    break;
                case "candidate-refresh":
                    report.aiUnifiedSnapshotExecutionLegacyCandidateRefreshCount = 1;
                    break;
                default:
                    Assert.Fail("Unknown Gate-B replaced pipeline activity: " + activity);
                    break;
            }

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_PreCommitRollbackContractIsValidWithoutAuthoritySuccess()
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            report.aiUnifiedSnapshotExecutionCommittedPassCount = 58;
            report.aiUnifiedSnapshotExecutionSlotVisitCount = 58L * 1050L;
            report.aiUnifiedSnapshotExecutionRefreshCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionReadCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionPreCommitFailureCount = 1;
            report.aiUnifiedSnapshotExecutionPreCommitFallbackCount = 1;
            report.aiUnifiedSnapshotExecutionLegacyFusedSensingBuildCount = 1;
            report.aiUnifiedSnapshotExecutionLegacyDecisionSharedBuildCount = 1;
            report.aiUnifiedSnapshotExecutionFirstFailureStage =
                AiUnifiedSnapshotExceptionStage.Capture.ToString();
            report.aiUnifiedSnapshotExecutionFirstFailureType =
                typeof(InvalidOperationException).FullName;

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.False);
            Assert.That(report.harnessValidity, Is.False);
            report.aiUnifiedSnapshotExecutionAuthoritySuccess = true;
            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotRollbackContractForReport(
                        report,
                        terminal: true),
                Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.False,
                "a valid rollback contract must still clear any stale authority-success state");
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackObserved, Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackContractSatisfied, Is.True);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionAuthoritySuccess\":false"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionRollbackObserved\":true"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionRollbackContractSatisfied\":true"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionPreCommitFailureCount\":1"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionPreCommitFallbackCount\":1"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionFirstFailureStage\":\"Capture\""));
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_PreCommitFallbackRequiresFirstFailure()
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            report.aiUnifiedSnapshotExecutionCommittedPassCount = 58;
            report.aiUnifiedSnapshotExecutionSlotVisitCount = 58L * 1050L;
            report.aiUnifiedSnapshotExecutionRefreshCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionReadCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionPreCommitFailureCount = 1;
            report.aiUnifiedSnapshotExecutionPreCommitFallbackCount = 1;

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotRollbackContractForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackObserved, Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackContractSatisfied, Is.False);
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_PostCommitBreachForbidsMixedFallback()
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            report.aiUnifiedSnapshotExecutionCommittedPassCount = 58;
            report.aiUnifiedSnapshotExecutionSlotVisitCount = 58L * 1050L;
            report.aiUnifiedSnapshotExecutionRefreshCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionReadCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionPreCommitFailureCount = 1;
            report.aiUnifiedSnapshotExecutionPreCommitFallbackCount = 1;
            report.aiUnifiedSnapshotExecutionPostCommitHardBreachCount = 1;
            report.aiUnifiedSnapshotExecutionFirstFailureStage =
                AiUnifiedSnapshotExceptionStage.RefreshCapture.ToString();
            report.aiUnifiedSnapshotExecutionFirstFailureType =
                typeof(InvalidOperationException).FullName;

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.False);
            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotRollbackContractForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackObserved, Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackContractSatisfied, Is.False);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionPostCommitHardBreachCount\":1"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionRollbackContractSatisfied\":false"));
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_TerminalRestoreFailureIsInvalid()
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            report.aiUnifiedSnapshotExecutionRestored = false;

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: false),
                Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.True);
            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
            Assert.That(report.aiUnifiedSnapshotExecutionAuthoritySuccess, Is.False);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionRestored\":false"));
            Assert.That(json, Does.Contain(
                "\"aiUnifiedSnapshotExecutionAuthoritySuccess\":false"));
        }

        [Test]
        public void UnifiedAiSnapshotAuthority_RollbackTerminalRestoreFailureInvalidatesContract()
        {
            ProductionEntityStressReport report =
                CreateValidAiUnifiedSnapshotAuthorityReport();
            report.aiUnifiedSnapshotExecutionRestored = false;
            report.aiUnifiedSnapshotExecutionCommittedPassCount = 58;
            report.aiUnifiedSnapshotExecutionSlotVisitCount = 58L * 1050L;
            report.aiUnifiedSnapshotExecutionRefreshCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionReadCount = 58L * 1000L;
            report.aiUnifiedSnapshotExecutionPreCommitFailureCount = 1;
            report.aiUnifiedSnapshotExecutionPreCommitFallbackCount = 1;
            report.aiUnifiedSnapshotExecutionFirstFailureStage =
                AiUnifiedSnapshotExceptionStage.Validate.ToString();
            report.aiUnifiedSnapshotExecutionFirstFailureType =
                typeof(InvalidOperationException).FullName;

            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotRollbackContractForReport(
                        report,
                        terminal: false),
                Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackContractSatisfied, Is.True);
            Assert.That(
                ProductionEntityStressRunner
                    .EvaluateAiUnifiedSnapshotRollbackContractForReport(
                        report,
                        terminal: true),
                Is.False);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackObserved, Is.True);
            Assert.That(report.aiUnifiedSnapshotExecutionRollbackContractSatisfied, Is.False);
        }

        [Test]
        public void AllocationRegionMetrics_SaturateAndNormalizeNegativeSamples()
        {
            ProductionEntityStressReport report = new ProductionEntityStressReport
            {
                allocationWriteReport = new ProductionEntityStressAllocationRegionMetrics
                {
                    sampleCount = long.MaxValue,
                    sumBytes = long.MaxValue - 4L,
                    maximumBytes = 8L,
                    lastBytes = 3L,
                },
            };

            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion.WriteReport,
                10L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion.WriteReport,
                -5L);

            AssertAllocationMetrics(
                report.allocationWriteReport,
                long.MaxValue,
                long.MaxValue,
                10L,
                0L);
        }

        [Test]
        public void AllocationRegionMetrics_AttributeSamplesToIndependentReportFields()
        {
            ProductionEntityStressReport report = new ProductionEntityStressReport();

            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion.PostTickTimingCollectors,
                101L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion.CaptureProductionCountersTotal,
                202L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersActiveEntityScan,
                303L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersSceneQueryDiagnostics,
                404L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersAiReportDiagnostics,
                505L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersObserveRuntimeEntitySnapshot,
                606L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion.WriteReport,
                707L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion.RunnerSteadyFrameOverhead,
                808L);
            ProductionEntityStressRunner.RecordAllocationBytesForReport(
                report,
                ProductionEntityStressAllocationRegion
                    .RefreshRosterAndCapacityDiagnostics,
                909L);

            AssertAllocationMetrics(
                report.allocationPostTickTimingCollectors,
                1L,
                101L,
                101L,
                101L);
            AssertAllocationMetrics(
                report.allocationCaptureProductionCountersTotal,
                1L,
                202L,
                202L,
                202L);
            AssertAllocationMetrics(
                report.allocationCaptureProductionCountersActiveEntityScan,
                1L,
                303L,
                303L,
                303L);
            AssertAllocationMetrics(
                report.allocationCaptureProductionCountersSceneQueryDiagnostics,
                1L,
                404L,
                404L,
                404L);
            AssertAllocationMetrics(
                report.allocationCaptureProductionCountersAiReportDiagnostics,
                1L,
                505L,
                505L,
                505L);
            AssertAllocationMetrics(
                report.allocationCaptureProductionCountersObserveRuntimeEntitySnapshot,
                1L,
                606L,
                606L,
                606L);
            AssertAllocationMetrics(
                report.allocationWriteReport,
                1L,
                707L,
                707L,
                707L);
            AssertAllocationMetrics(
                report.allocationRunnerSteadyFrameOverhead,
                1L,
                808L,
                808L,
                808L);
            AssertAllocationMetrics(
                report.allocationRefreshRosterAndCapacityDiagnostics,
                1L,
                909L,
                909L,
                909L);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain(
                "\"allocationPostTickTimingCollectors\""));
            Assert.That(json, Does.Contain("\"allocationWriteReport\""));
            Assert.That(json, Does.Contain("\"allocationRunnerSteadyFrameOverhead\""));
        }

        [Test]
        public void CpuRegionMetrics_SaturateAndNormalizeNegativeSamples()
        {
            ProductionEntityStressReport report = new ProductionEntityStressReport
            {
                cpuWriteReport = new ProductionEntityStressCpuRegionMetrics
                {
                    sampleCount = long.MaxValue,
                    sumMilliseconds = double.MaxValue - 4d,
                    maximumMilliseconds = 8d,
                    lastMilliseconds = 3d,
                },
            };

            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.WriteReport,
                10d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.WriteReport,
                -5d);

            AssertCpuMetrics(
                report.cpuWriteReport,
                long.MaxValue,
                double.MaxValue,
                10d,
                0d);
        }

        [Test]
        public void CpuRegionMetrics_AttributeSamplesToIndependentReportFields()
        {
            ProductionEntityStressReport report = new ProductionEntityStressReport();

            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.RunnerUpdateTotal,
                101d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.SpawnOrRemove,
                202d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.StepMeasuredTickTotal,
                303d);
            ProductionEntityStressRunner.RecordCpuElapsedTicksForReport(
                report,
                ProductionEntityStressCpuRegion.DriverStepOneTick,
                System.Diagnostics.Stopwatch.Frequency);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.PostTickTimingCollectors,
                505d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.CaptureProductionCountersTotal,
                606d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion
                    .CaptureProductionCountersActiveEntityScan,
                707d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion
                    .CaptureProductionCountersSceneQueryDiagnostics,
                808d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion
                    .CaptureProductionCountersAiReportDiagnostics,
                909d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion
                    .CaptureProductionCountersObserveRuntimeEntitySnapshot,
                1010d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.WriteReport,
                1111d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion.RunnerSteadyFrameOverhead,
                1212d);
            ProductionEntityStressRunner.RecordCpuMillisecondsForReport(
                report,
                ProductionEntityStressCpuRegion
                    .RefreshRosterAndCapacityDiagnostics,
                1313d);

            AssertCpuMetrics(report.cpuRunnerUpdateTotal, 1L, 101d, 101d, 101d);
            AssertCpuMetrics(report.cpuSpawnOrRemove, 1L, 202d, 202d, 202d);
            AssertCpuMetrics(
                report.cpuStepMeasuredTickTotal,
                1L,
                303d,
                303d,
                303d);
            AssertCpuMetrics(
                report.cpuDriverStepOneTick,
                1L,
                1000d,
                1000d,
                1000d);
            AssertCpuMetrics(
                report.cpuPostTickTimingCollectors,
                1L,
                505d,
                505d,
                505d);
            AssertCpuMetrics(
                report.cpuCaptureProductionCountersTotal,
                1L,
                606d,
                606d,
                606d);
            AssertCpuMetrics(
                report.cpuCaptureProductionCountersActiveEntityScan,
                1L,
                707d,
                707d,
                707d);
            AssertCpuMetrics(
                report.cpuCaptureProductionCountersSceneQueryDiagnostics,
                1L,
                808d,
                808d,
                808d);
            AssertCpuMetrics(
                report.cpuCaptureProductionCountersAiReportDiagnostics,
                1L,
                909d,
                909d,
                909d);
            AssertCpuMetrics(
                report.cpuCaptureProductionCountersObserveRuntimeEntitySnapshot,
                1L,
                1010d,
                1010d,
                1010d);
            AssertCpuMetrics(report.cpuWriteReport, 1L, 1111d, 1111d, 1111d);
            AssertCpuMetrics(
                report.cpuRunnerSteadyFrameOverhead,
                1L,
                1212d,
                1212d,
                1212d);
            AssertCpuMetrics(
                report.cpuRefreshRosterAndCapacityDiagnostics,
                1L,
                1313d,
                1313d,
                1313d);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain("\"cpuRunnerUpdateTotal\""));
            Assert.That(json, Does.Contain("\"cpuWriteReport\""));
            Assert.That(json, Does.Contain("\"cpuRunnerSteadyFrameOverhead\""));
        }

        private static void AssertAllocationMetrics(
            ProductionEntityStressAllocationRegionMetrics metrics,
            long expectedSampleCount,
            long expectedSumBytes,
            long expectedMaximumBytes,
            long expectedLastBytes)
        {
            Assert.That(metrics.sampleCount, Is.EqualTo(expectedSampleCount));
            Assert.That(metrics.sumBytes, Is.EqualTo(expectedSumBytes));
            Assert.That(metrics.maximumBytes, Is.EqualTo(expectedMaximumBytes));
            Assert.That(metrics.lastBytes, Is.EqualTo(expectedLastBytes));
        }

        private static void AssertCpuMetrics(
            ProductionEntityStressCpuRegionMetrics metrics,
            long expectedSampleCount,
            double expectedSumMilliseconds,
            double expectedMaximumMilliseconds,
            double expectedLastMilliseconds)
        {
            Assert.That(metrics.sampleCount, Is.EqualTo(expectedSampleCount));
            Assert.That(
                metrics.sumMilliseconds,
                Is.EqualTo(expectedSumMilliseconds));
            Assert.That(
                metrics.maximumMilliseconds,
                Is.EqualTo(expectedMaximumMilliseconds));
            Assert.That(
                metrics.lastMilliseconds,
                Is.EqualTo(expectedLastMilliseconds));
        }

        private static ProductionEntityStressReport
            CreateValidAiUnifiedSnapshotAuthorityReport()
        {
            const long observedPasses = 59;
            const long refreshAndReadCount = observedPasses * 1000;
            return new ProductionEntityStressReport
            {
                harnessValidity = true,
                logicTicksExecuted = 60,
                requestedEntityCount = 1000,
                runtimeSlotCapacity = 1050,
                aiUnifiedSnapshotExecutionRequestedMode =
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority.ToString(),
                aiUnifiedSnapshotExecutionEffectiveMode =
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority.ToString(),
                aiUnifiedSnapshotExecutionRestored = true,
                aiUnifiedSnapshotExecutionBuildCount = observedPasses,
                aiUnifiedSnapshotExecutionSlotVisitCount = observedPasses * 1050,
                aiUnifiedSnapshotExecutionRefreshCount = refreshAndReadCount,
                aiUnifiedSnapshotExecutionReadCount = refreshAndReadCount,
                aiUnifiedSnapshotExecutionCommittedPassCount = observedPasses,
                aiUnifiedSnapshotExecutionFirstFailureStage =
                    AiUnifiedSnapshotExceptionStage.None.ToString(),
                aiUnifiedSnapshotExecutionFirstFailureType = string.Empty,
            };
        }

        private static ProductionEntityStressReport CreateValidUnifiedCanonicalPressureReport()
        {
            const long observedPasses = 59;
            const long refreshCount = observedPasses * 1000;
            return new ProductionEntityStressReport
            {
                harnessValidity = true,
                logicTicksExecuted = 60,
                requestedEntityCount = 1000,
                runtimeSlotCapacity = 1050,
                aiSensingRequestedMode = "legacy",
                aiDecisionExecutionRequestedMode =
                    AiDecisionExecutionMode.IndexedCanonical.ToString(),
                aiDecisionExecutionEffectiveMode =
                    AiDecisionExecutionMode.IndexedCanonical.ToString(),
                aiDecisionIndexedCanonicalEligibleCount = refreshCount,
                aiDecisionIndexedCanonicalCommittedCount = refreshCount,
                unifiedAiSnapshotShadowRequested = true,
                unifiedAiSnapshotShadowApplied = true,
                unifiedAiSnapshotShadowRestored = true,
                unifiedAiSnapshotShadowBuildCount = observedPasses,
                unifiedAiSnapshotShadowSlotVisitCount = observedPasses * 1050,
                unifiedAiSnapshotShadowRefreshCount = refreshCount,
                unifiedAiSnapshotShadowFullComparisonSlotVisitCount =
                    observedPasses * 1050,
                unifiedAiSnapshotShadowRefreshComparisonSlotVisitCount = refreshCount,
                unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount =
                    observedPasses * 1050,
                unifiedAiSnapshotShadowMutationWitnessComparedCount = refreshCount,
                unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount = 0,
                unifiedAiSnapshotShadowSensingComparedCount = 0,
                unifiedAiSnapshotShadowDecisionComparedCount =
                    observedPasses + refreshCount,
                unifiedAiSnapshotShadowFirstMismatch =
                    AiUnifiedSnapshotMismatchKind.None.ToString(),
                unifiedAiSnapshotShadowFirstExceptionStage =
                    AiUnifiedSnapshotExceptionStage.None.ToString(),
                unifiedAiSnapshotShadowFirstExceptionType = string.Empty,
            };
        }

        private static ProductionEntityStressReport CreateValidUnifiedDualConsumerPressureReport()
        {
            ProductionEntityStressReport report =
                CreateValidUnifiedCanonicalPressureReport();
            long expectedConsumerComparisons =
                report.unifiedAiSnapshotShadowBuildCount +
                report.unifiedAiSnapshotShadowRefreshCount;
            report.aiSensingRequestedMode = "candidate";
            report.unifiedAiSnapshotShadowSensingComparedCount =
                expectedConsumerComparisons;
            report.unifiedAiSnapshotShadowFullComparisonSlotVisitCount *= 2L;
            report.unifiedAiSnapshotShadowRefreshComparisonSlotVisitCount *= 2L;
            report.unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount *= 2L;
            report.unifiedAiSnapshotShadowMutationWitnessComparedCount *= 2L;
            return report;
        }

        private static ProductionEntityStressReport CreateValidSharedShadowPressureReport()
        {
            return new ProductionEntityStressReport
            {
                harnessValidity = true,
                aiDecisionSharedShadowRequested = true,
                aiDecisionSharedShadowApplied = true,
                logicTicksExecuted = 60,
                aiDecisionSoAShadowEligibleCount = 59000,
                aiDecisionSoAShadowAvailableCount = 59000,
                aiDecisionSoAShadowComparedCount = 59000,
                aiDecisionSharedShadowBuildCount = 59,
                aiDecisionSharedShadowRefreshCount = 59000,
                aiDecisionIndexedEligibleCount = 59000,
                aiDecisionIndexedAvailableCount = 59000,
                aiDecisionIndexedComparedCount = 59000,
            };
        }

        [TestCase(1, 0)]
        [TestCase(0, 1)]
        public void AiDecisionSharedShadow_IndexedUnavailableOrMismatchIsInvalid(
            long unavailable,
            long mismatch)
        {
            ProductionEntityStressReport report = CreateValidSharedShadowPressureReport();
            report.aiDecisionIndexedUnavailableCount = unavailable;
            report.aiDecisionIndexedAvailableCount -= unavailable;
            report.aiDecisionIndexedMismatchCount = mismatch;

            Assert.That(
                ProductionEntityStressRunner.EvaluateAiDecisionShadowValidityForReport(
                    report,
                    terminal: true),
                Is.False);
            Assert.That(report.harnessValidity, Is.False);
        }

        [Test]
        public void AiSoADecisionRemainder_RequestApplyRestoreAndEnabledOnlyGate()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    aiSensingMode = "candidate",
                    allowUnsafeAiSoACandidate = true,
                    enableAiSoADecisionRemainder = true,
                    outputPath = "Temp/ai-soa-decision-remainder.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            var report = new ProductionEntityStressReport();
            ProductionEntityStressRunner.ApplyAiSensingConfigurationForDiagnostics(
                world,
                config,
                report);

            bool previous = ProductionEntityStressRunner
                .ApplyAiSoADecisionRemainderForDiagnostics(world, config, report);
            Assert.That(previous, Is.False);
            Assert.That(report.aiSoADecisionRemainderRequested, Is.True);
            Assert.That(report.aiSoADecisionRemainderApplied, Is.True);
            ProductionEntityStressRunner
                .AggregateAiSoADecisionRemainderDiagnosticsForReport(
                    report, 2, 2, 0, 0, 0, 0, 2, 4, 12);
            Assert.That(report.aiSoADecisionRemainderEligibleAttemptCount, Is.EqualTo(2));
            Assert.That(report.aiSoADecisionRemainderExpectedAppliedCount, Is.EqualTo(2));
            Assert.That(
                ProductionEntityStressRunner.AreAiSoADecisionRemainderDiagnosticsValid(
                    report),
                Is.True);

            report.aiSoADecisionRemainderEligibleAttemptCount = 3;
            report.aiSoADecisionRemainderExpectedAppliedCount = 3;
            Assert.That(
                ProductionEntityStressRunner.AreAiSoADecisionRemainderDiagnosticsValid(
                    report),
                Is.False,
                "completed outcomes must exactly partition eligible attempts");
            report.aiSoADecisionRemainderEligibleAttemptCount = 2;
            report.aiSoADecisionRemainderExpectedAppliedCount = 2;
            report.aiSoADecisionRemainderFallbackCount = 1;
            Assert.That(
                ProductionEntityStressRunner.AreAiSoADecisionRemainderDiagnosticsValid(
                    report),
                Is.False);
            report.aiSoADecisionRemainderFallbackCount = 0;
            ProductionEntityStressRunner.RestoreAiSoADecisionRemainderForDiagnostics(
                world,
                previous,
                report);
            Assert.That(report.aiSoADecisionRemainderRestored, Is.True);
            Assert.That(world.AiSoADecisionRemainderEnabledForDiagnostics, Is.False);
            ProductionEntityStressRunner.CloseUnsafeAiSensingConfigurationForDiagnostics(world);

            Assert.That(
                ProductionEntityStressRunner.AreAiSoADecisionRemainderDiagnosticsValid(
                    new ProductionEntityStressReport()),
                Is.True,
                "disabled/default reports must not be gated by decision remainder counters");
        }

        [Test]
        public void DefaultMenuRequest_PreservesLegacyPresentationAndStopBehavior()
        {
            ProductionEntityStressRequest request =
                ProductionEntityStressWindow.CreateDefaultRequest(
                    "dispersed",
                    "Temp/default-menu.json");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(request.simulationOnly, Is.False);
            Assert.That(request.autoStopWhenSampled, Is.False);
            Assert.That(request.enablePhaseTiming, Is.False);
            Assert.That(request.enablePresentationTiming, Is.False);
            Assert.That(request.enableDetailPhaseTiming, Is.False);
            Assert.That(request.aiSensingMode, Is.EqualTo("legacy"));
            Assert.That(request.allowUnsafeAiSoACandidate, Is.False);
            Assert.That(request.maxCatchUpTicksPerFrame, Is.EqualTo(1));
            Assert.That(config.MaxCatchUpTicksPerFrame, Is.EqualTo(1));
            Assert.That(config.ShouldAutoStopWhenSampled, Is.False);
        }

        [Test]
        public void AiSensingMode_ShadowAndSeedApplyBeforeRegistrationAndResetDiagnostics()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                aiSensingMode = "shadow",
                seed = 0x51A0u,
                outputPath = "Temp/ai-shadow.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                CollisionBroadphaseBackend.LooseQuadtree);
            var report = new ProductionEntityStressReport();

            Assert.Throws<NotSupportedException>(() =>
                world.AiSensingMode = AiSensingMode.SoAAiSensing);
            ProductionEntityStressRunner.ApplyAiSensingConfigurationForDiagnostics(
                world,
                config,
                report);

            Assert.That(world.ObjectCount, Is.Zero);
            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.SoAShadowAiSensing));
            Assert.That(world.Rng.State, Is.EqualTo(0x51A0u));
            Assert.That(world.AiSoASensingShadowQueryCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoASensingShadowInvalidationCountForDiagnostics, Is.Zero);
            Assert.That(report.seed, Is.EqualTo(0x51A0u));
            Assert.That(report.aiSensingRequestedMode, Is.EqualTo("shadow"));
            Assert.That(report.aiSensingEffectiveMode, Is.EqualTo("shadow"));
        }

        [Test]
        public void AiSensingMode_CandidateWithoutUnsafeOptInIsRejected()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(
                    "{\"action\":\"smoke\",\"aiSensingMode\":\"candidate\"," +
                    "\"outputPath\":\"Temp/ai-candidate-rejected.json\"}");

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(request.allowUnsafeAiSoACandidate, Is.False);
            Assert.That(exception.Message, Does.Contain("allowUnsafeAiSoACandidate=true"));
            Assert.That(exception.Message, Does.Contain("Diagnostic/Unsafe"));
        }

        [Test]
        public void AiSensingMode_CandidateExplicitOptInAppliesAndCleanupClosesGate()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                aiSensingMode = "candidate",
                allowUnsafeAiSoACandidate = true,
                seed = 0xCA11u,
                outputPath = "Temp/ai-candidate.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                CollisionBroadphaseBackend.LooseQuadtree);
            var report = new ProductionEntityStressReport();

            ProductionEntityStressRunner.ApplyAiSensingConfigurationForDiagnostics(
                world,
                config,
                report);

            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
            Assert.That(config.AllowUnsafeAiSoACandidate, Is.True);
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
            Assert.That(report.allowUnsafeAiSoACandidate, Is.True);
            Assert.That(report.aiSensingRequestedMode, Is.EqualTo("candidate"));
            Assert.That(report.aiSensingEffectiveMode, Is.EqualTo("candidate"));
            Assert.That(world.AiSoACandidateNearestQueryCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidateSpecialQueryCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidateLegacyNearestScanCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidateLegacySpecialScanCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidatePreRandomFailureCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoACandidatePostRandomFailureCountForDiagnostics, Is.Zero);
            Assert.That(world.AiLegacyNearestFactsBuildCountForDiagnostics, Is.Zero);
            Assert.That(world.AiLegacySnapshotIndexBuildCountForDiagnostics, Is.Zero);
            Assert.That(world.AiLegacyQuadtreeSyncCountForDiagnostics, Is.Zero);
            Assert.That(world.AiLegacySnapshotMutationCountForDiagnostics, Is.Zero);
            Assert.That(
                ProductionEntityStressRunner.AreAiSoACandidateFallbackDiagnosticsClean(report),
                Is.True,
                "A normal Candidate benchmark must not perform Legacy scans or report failures.");
            Assert.That(
                ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.SoAAiSensing),
                Is.True,
                "A normal Candidate benchmark must not build or mutate Legacy AI snapshots.");

            ProductionEntityStressRunner.CloseUnsafeAiSensingConfigurationForDiagnostics(world);

            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.LegacyAiSensing));
        }

        [Test]
        public void AiSensingMode_LegacyAndShadowApplicationCloseCandidateGateFirst()
        {
            var world = new SimulationWorld();
            ProductionEntityStressConfig candidate = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    aiSensingMode = "candidate",
                    allowUnsafeAiSoACandidate = true,
                    outputPath = "Temp/ai-candidate-first.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressRunner.ApplyAiSensingConfigurationForDiagnostics(
                world,
                candidate,
                targetReport: null);
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));

            foreach (string mode in new[] { "legacy", "shadow" })
            {
                ProductionEntityStressConfig safe = ProductionEntityStressConfig.FromRequest(
                    new ProductionEntityStressRequest
                    {
                        action = "smoke",
                        aiSensingMode = mode,
                        outputPath = $"Temp/ai-{mode}.json",
                    },
                    ProductionEntityStressPaths.ProjectRoot);
                ProductionEntityStressRunner.ApplyAiSensingConfigurationForDiagnostics(
                    world,
                    safe,
                    targetReport: null);
                Assert.That(
                    world.AiSensingMode,
                    Is.EqualTo(mode == "legacy"
                        ? AiSensingMode.LegacyAiSensing
                        : AiSensingMode.SoAShadowAiSensing));
            }
        }

        [TestCase("soa")]
        [TestCase("SoAAiSensing")]
        [TestCase("enabled")]
        public void AiSensingMode_RejectsFormalSoAAndUnknownValues(string mode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                aiSensingMode = mode,
                outputPath = "Temp/ai-invalid.json",
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("legacy or shadow"));
            Assert.That(exception.Message, Does.Contain("SoAAiSensing"));
        }

        [Test]
        public void CandidateDiagnostics_ReportJsonAndAggregationPreserveMaxima()
        {
            var report = new ProductionEntityStressReport
            {
                aiSoACandidateNearestQueryCount = 10,
                aiSoACandidateSpecialQueryCount = 2,
                aiSoACandidateGroundXRowVisitCount = 12,
                aiSoACandidateAirXRowVisitCount = 18,
                aiSoACandidateSpecialSlotVisitCount = 7,
                aiSoACandidateLegacyNearestScanCount = 1,
                aiSoACandidateLegacySpecialScanCount = 0,
                aiSoACandidatePreRandomFailureCount = 3,
                aiSoACandidatePostRandomFailureCount = 0,
            };

            ProductionEntityStressRunner.AggregateAiSoACandidateDiagnosticsForReport(
                report,
                nearestQueryCount: 7,
                specialQueryCount: 8,
                groundXRowVisitCount: 17,
                airXRowVisitCount: 11,
                specialSlotVisitCount: 9,
                legacyNearestScanCount: 0,
                legacySpecialScanCount: 4,
                preRandomFailureCount: 1,
                postRandomFailureCount: 5);

            Assert.That(report.aiSoACandidateNearestQueryCount, Is.EqualTo(10));
            Assert.That(report.aiSoACandidateSpecialQueryCount, Is.EqualTo(8));
            Assert.That(report.aiSoACandidateGroundXRowVisitCount, Is.EqualTo(17));
            Assert.That(report.aiSoACandidateAirXRowVisitCount, Is.EqualTo(18));
            Assert.That(report.aiSoACandidateSpecialSlotVisitCount, Is.EqualTo(9));
            Assert.That(report.aiSoACandidateLegacyNearestScanCount, Is.EqualTo(1));
            Assert.That(report.aiSoACandidateLegacySpecialScanCount, Is.EqualTo(4));
            Assert.That(report.aiSoACandidatePreRandomFailureCount, Is.EqualTo(3));
            Assert.That(report.aiSoACandidatePostRandomFailureCount, Is.EqualTo(5));
            Assert.That(
                ProductionEntityStressRunner.AreAiSoACandidateFallbackDiagnosticsClean(report),
                Is.False);

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain("\"schema\":\"ntsd-production-entity-stress/v2\""));
            Assert.That(json, Does.Contain("\"aiSoACandidateNearestQueryCount\":10"));
            Assert.That(json, Does.Contain("\"aiSoACandidateSpecialQueryCount\":8"));
            Assert.That(json, Does.Contain("\"aiSoACandidateGroundXRowVisitCount\":17"));
            Assert.That(json, Does.Contain("\"aiSoACandidateAirXRowVisitCount\":18"));
            Assert.That(json, Does.Contain("\"aiSoACandidateSpecialSlotVisitCount\":9"));
            Assert.That(json, Does.Contain("\"aiSoACandidateLegacyNearestScanCount\":1"));
            Assert.That(json, Does.Contain("\"aiSoACandidateLegacySpecialScanCount\":4"));
            Assert.That(json, Does.Contain("\"aiSoACandidatePreRandomFailureCount\":3"));
            Assert.That(json, Does.Contain("\"aiSoACandidatePostRandomFailureCount\":5"));
        }

        [Test]
        public void LegacyAiDiagnostics_ReportJsonAndAggregationPreserveMaxima()
        {
            var report = new ProductionEntityStressReport
            {
                aiLegacyNearestFactsBuildCount = 10,
                aiLegacySnapshotIndexBuildCount = 2,
                aiLegacyQuadtreeSyncCount = 1,
                aiLegacySnapshotMutationCount = 0,
            };

            ProductionEntityStressRunner.AggregateAiLegacyDiagnosticsForReport(
                report,
                nearestFactsBuildCount: 7,
                snapshotIndexBuildCount: 8,
                quadtreeSyncCount: 0,
                snapshotMutationCount: 4);

            Assert.That(report.aiLegacyNearestFactsBuildCount, Is.EqualTo(10));
            Assert.That(report.aiLegacySnapshotIndexBuildCount, Is.EqualTo(8));
            Assert.That(report.aiLegacyQuadtreeSyncCount, Is.EqualTo(1));
            Assert.That(report.aiLegacySnapshotMutationCount, Is.EqualTo(4));

            string json = JsonUtility.ToJson(report);
            Assert.That(json, Does.Contain("\"aiLegacyNearestFactsBuildCount\":10"));
            Assert.That(json, Does.Contain("\"aiLegacySnapshotIndexBuildCount\":8"));
            Assert.That(json, Does.Contain("\"aiLegacyQuadtreeSyncCount\":1"));
            Assert.That(json, Does.Contain("\"aiLegacySnapshotMutationCount\":4"));
        }

        [Test]
        public void LegacyAiDiagnostics_ModeValidityEnforcesCandidateZeroAndLegacyPositive()
        {
            var report = new ProductionEntityStressReport
            {
                harnessValidity = true,
                logicTicksExecuted = 1,
            };

            Assert.That(
                ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.SoAAiSensing),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.HasExecutedAiEntityInputPassForDiagnostics(report),
                Is.False,
                "CharacterInputAll intentionally skips the first logic tick.");
            if (!ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.LegacyAiSensing))
            {
                report.harnessValidity = false;
            }
            Assert.That(
                report.harnessValidity,
                Is.True,
                "The skipped first input tick must not permanently invalidate a Legacy run.");

            report.logicTicksExecuted = 2;
            Assert.That(
                ProductionEntityStressRunner.HasExecutedAiEntityInputPassForDiagnostics(report),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.SoAAiSensing),
                Is.True,
                "A final Candidate gate accepts only the untouched all-zero Legacy diagnostics.");
            Assert.That(
                ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.LegacyAiSensing),
                Is.False,
                "A formal Legacy run must exercise all four Legacy diagnostics.");

            ProductionEntityStressRunner.AggregateAiLegacyDiagnosticsForReport(
                report,
                nearestFactsBuildCount: 1,
                snapshotIndexBuildCount: 2,
                quadtreeSyncCount: 3,
                snapshotMutationCount: 4);

            Assert.That(
                ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.LegacyAiSensing),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.SoAAiSensing),
                Is.False,
                "Any Legacy work makes a Candidate formal run invalid.");
            Assert.That(
                ProductionEntityStressRunner.AreAiLegacyDiagnosticsValidForMode(
                    report,
                    AiSensingMode.SoAShadowAiSensing),
                Is.True,
                "Shadow mode intentionally runs both implementations and is not an A/B gate.");
        }

        [Test]
        public void SimulationOnly_DisablesPresentationForEverySampleTick()
        {
            Assert.That(
                ProductionEntityStressRunner.ResolveBuildPresentationForStressTick(
                    simulationOnly: true,
                    remainingAccumulator: 0f,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicksPerFrame: 4),
                Is.False);
            Assert.That(
                ProductionEntityStressRunner.ResolveBuildPresentationForStressTick(
                    simulationOnly: false,
                    remainingAccumulator: 0f,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicksPerFrame: 4),
                Is.True);
        }

        [Test]
        public void AutoStopWhenSampled_ExtendsCleanupToNonSmokeWithoutChangingDefault()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed",
                autoStopWhenSampled = true,
                outputPath = "Temp/auto-stop.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.AutoCleanup, Is.False);
            Assert.That(config.AutoStopWhenSampled, Is.True);
            Assert.That(config.ShouldAutoStopWhenSampled, Is.True);
        }

        [Test]
        public void TimingFlags_AreIndependentAndDisabledReportsPublishNoSamples()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed",
                enablePhaseTiming = true,
                enablePresentationTiming = true,
                outputPath = "Temp/timing-enabled.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport();

            Assert.That(config.EnablePhaseTiming, Is.True);
            Assert.That(config.EnablePresentationTiming, Is.True);
            Assert.That(config.EnableDetailPhaseTiming, Is.False);

            ProductionEntityStressPhaseTimingCollector.PopulateDisabledReport(report);
            ProductionEntityStressPresentationTimingCollector.PopulateDisabledReport(report);
            Assert.That(report.phaseTimingEnabled, Is.False);
            Assert.That(report.phaseTimings, Is.Empty);
            Assert.That(report.presentationTimingEnabled, Is.False);
            Assert.That(report.presentationTimings, Is.Empty);
        }

        [Test]
        public void PresentationTiming_EnabledCollectorPublishesCoarseBucketsOnce()
        {
            var recorder = new BattlePresentationPhaseDiagnostics();
            var collector = new ProductionEntityStressPresentationTimingCollector();
            recorder.SetEnabled(true);
            recorder.BeginTick(42);
            for (int i = 0; i < BattlePresentationPhaseDiagnostics.PhaseCount; i++)
            {
                BattlePresentationPhase phase = (BattlePresentationPhase)i;
                recorder.BeginPhase(phase);
                Thread.SpinWait(1000);
                recorder.EndPhase(phase);
            }
            recorder.CompleteTick(42);
            collector.CaptureAfterTick(recorder, completedTickCount: 1, warmupTickCount: 0);
            collector.CaptureAfterTick(recorder, completedTickCount: 2, warmupTickCount: 0);
            var report = new ProductionEntityStressReport();

            collector.PopulateReport(report);

            Assert.That(collector.SampleCount, Is.EqualTo(1));
            Assert.That(report.presentationTimingEnabled, Is.True);
            Assert.That(
                report.presentationTimings,
                Has.Count.EqualTo(BattlePresentationPhaseDiagnostics.PhaseCount));
        }

        [Test]
        public void PresentationTiming_NextTickDoesNotEraseLastCompletedSample()
        {
            var recorder = new BattlePresentationPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(7);
            recorder.BeginPhase(BattlePresentationPhase.PresentationPublishTotal);
            Thread.SpinWait(1000);
            recorder.EndPhase(BattlePresentationPhase.PresentationPublishTotal);
            recorder.CompleteTick(7);
            long completed = recorder.GetLastElapsedTimestampTicks(
                BattlePresentationPhase.PresentationPublishTotal);

            recorder.BeginTick(8);

            Assert.That(completed, Is.GreaterThan(0));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattlePresentationPhase.PresentationPublishTotal),
                Is.EqualTo(completed));
            Assert.That(recorder.LastCompletedTickIndex, Is.EqualTo(7));
            Assert.That(recorder.CompletedSampleSequence, Is.EqualTo(1));
        }

        [Test]
        public void LogicTickTimingCollector_SeparatesPresentationBuildSamplesInReport()
        {
            var collector = new ProductionEntityStressLogicTickTimingCollector();
            collector.AddSample(8d, buildPresentation: false);
            collector.AddSample(12d, buildPresentation: true);
            collector.AddSample(16d, buildPresentation: true);
            var report = new ProductionEntityStressReport();

            collector.PopulateReport(report);

            Assert.That(report.logicTickMilliseconds.sampleCount, Is.EqualTo(3));
            Assert.That(report.logicTickWithPresentationMilliseconds.sampleCount, Is.EqualTo(2));
            Assert.That(report.logicTickWithPresentationMilliseconds.average, Is.EqualTo(14d));
            Assert.That(report.logicTickWithoutPresentationMilliseconds.sampleCount, Is.EqualTo(1));
            Assert.That(report.logicTickWithoutPresentationMilliseconds.average, Is.EqualTo(8d));
        }

        [Test]
        public void FingerprintsAndFinalHashes_AreDeterministicAndSerializable()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                seed = 17u,
                aiSensingMode = "shadow",
                simulationOnly = true,
                outputPath = "Temp/fingerprint.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            string workload = ProductionEntityStressFingerprint.BuildWorkload(config);
            string implementationConfig =
                ProductionEntityStressFingerprint.BuildImplementationConfig(config);
            string roster = ProductionEntityStressFingerprint.BuildRoster(
                config.Mode,
                config.EntityCount,
                selectedCharacterOid: 7);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                CollisionBroadphaseBackend.LooseQuadtree);
            world.Rng.Seed(config.Seed);
            BattleExtendedChecksumSnapshot snapshot = world.CaptureExtendedChecksumSnapshot(9);
            BattleLockstepChecksumSnapshot lockstepSnapshot =
                world.CaptureLockstepChecksumSnapshot(9);
            var report = new ProductionEntityStressReport
            {
                workloadFingerprint = workload,
                implementationConfigFingerprint = implementationConfig,
                rosterFingerprint = roster,
                aiSoASensingShadowQueryCount = 3,
                aiSoASensingShadowInvalidationCount = 1,
                aiSoASensingShadowPurityMismatchCount = 0,
            };

            ProductionEntityStressParityReport.Populate(report, snapshot);
            ProductionEntityStressParityReport.PopulateLockstep(report, lockstepSnapshot);
            string json = JsonUtility.ToJson(report);

            Assert.That(workload, Is.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(config)));
            Assert.That(roster, Is.EqualTo(ProductionEntityStressFingerprint.BuildRoster(
                config.Mode,
                config.EntityCount,
                selectedCharacterOid: 7)));
            Assert.That(report.finalParitySnapshotSchema, Is.EqualTo(snapshot.Schema));
            Assert.That(report.finalParityInputHash, Is.EqualTo(snapshot.Hashes.Input));
            Assert.That(report.finalParityRngHash, Is.EqualTo(snapshot.Hashes.Rng));
            Assert.That(report.finalParityMetadataHash, Is.EqualTo(snapshot.Hashes.Metadata));
            Assert.That(report.finalParityWorldHash, Is.EqualTo(snapshot.Hashes.World));
            Assert.That(report.finalParitySlotsHash, Is.EqualTo(snapshot.Hashes.Slots));
            Assert.That(report.finalParityARestHash, Is.EqualTo(snapshot.Hashes.ARest));
            Assert.That(report.finalParityVRestHash, Is.EqualTo(snapshot.Hashes.VRest));
            Assert.That(report.finalParityStatsHash, Is.EqualTo(snapshot.Hashes.Stats));
            Assert.That(report.finalParityEventsHash, Is.EqualTo(snapshot.Hashes.Events));
            Assert.That(report.finalParityOverallHash, Is.EqualTo(snapshot.Hashes.Overall));
            Assert.That(report.finalLockstepSchema, Is.EqualTo(lockstepSnapshot.Schema));
            Assert.That(report.finalLockstepTick, Is.EqualTo(lockstepSnapshot.Tick));
            Assert.That(report.finalLockstepInputHash, Is.EqualTo(lockstepSnapshot.Hashes.Input));
            Assert.That(report.finalLockstepRngHash, Is.EqualTo(lockstepSnapshot.Hashes.Rng));
            Assert.That(report.finalLockstepMetadataHash, Is.EqualTo(lockstepSnapshot.Hashes.Metadata));
            Assert.That(report.finalLockstepWorldHash, Is.EqualTo(lockstepSnapshot.Hashes.World));
            Assert.That(report.finalLockstepSlotsHash, Is.EqualTo(lockstepSnapshot.Hashes.Slots));
            Assert.That(report.finalLockstepARestHash, Is.EqualTo(lockstepSnapshot.Hashes.ARest));
            Assert.That(report.finalLockstepVRestHash, Is.EqualTo(lockstepSnapshot.Hashes.VRest));
            Assert.That(report.finalLockstepStatsHash, Is.EqualTo(lockstepSnapshot.Hashes.Stats));
            Assert.That(report.finalLockstepEventsHash, Is.EqualTo(lockstepSnapshot.Hashes.Events));
            Assert.That(report.finalLockstepOverallHash, Is.EqualTo(lockstepSnapshot.Hashes.Overall));
            Assert.That(report.finalParitySnapshotSchema, Is.EqualTo(snapshot.Schema),
                "Adding the lockstep projection must not overwrite the existing full parity report.");
            Assert.That(json, Does.Contain("\"rosterFingerprint\""));
            Assert.That(json, Does.Contain("\"workloadFingerprint\""));
            Assert.That(json, Does.Contain("\"implementationConfigFingerprint\""));
            Assert.That(json, Does.Contain("\"aiSoASensingShadowQueryCount\":3"));
            Assert.That(json, Does.Contain("\"finalLockstepOverallHash\""));
        }

        [Test]
        public void Fingerprints_LegacyAndCandidateShareWorkloadButNotImplementationConfig()
        {
            var legacyRequest = new ProductionEntityStressRequest
            {
                action = "dispersed",
                warmupTicks = 7,
                sampleTicks = 11,
                seed = 123u,
                aiSensingMode = "legacy",
                outputPath = "Temp/fingerprint-legacy.json",
            };
            var candidateRequest = new ProductionEntityStressRequest
            {
                action = legacyRequest.action,
                warmupTicks = legacyRequest.warmupTicks,
                sampleTicks = legacyRequest.sampleTicks,
                seed = legacyRequest.seed,
                aiSensingMode = "candidate",
                allowUnsafeAiSoACandidate = true,
                outputPath = "Temp/fingerprint-candidate.json",
            };
            ProductionEntityStressConfig legacy = ProductionEntityStressConfig.FromRequest(
                legacyRequest,
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig candidate = ProductionEntityStressConfig.FromRequest(
                candidateRequest,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(candidate),
                Is.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(legacy)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildImplementationConfig(candidate),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildImplementationConfig(legacy)));
        }

        [Test]
        public void Fingerprints_FormalCollectorAndDirectRoutesAreImplementationDistinct()
        {
            ProductionEntityStressConfig configured = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    formalCollectorMode = "configured",
                    outputPath = "Temp/fingerprint-configured.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig adaptive = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    formalCollectorMode = "role",
                    outputPath = "Temp/fingerprint-adaptive.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig direct = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    formalCollectorMode = "role",
                    forceRoleAwareDirect = true,
                    outputPath = "Temp/fingerprint-direct.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig tree = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    formalCollectorMode = "role",
                    forceRoleAwareTree = true,
                    outputPath = "Temp/fingerprint-tree.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig nested = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    formalCollectorMode = "role",
                    forceRoleAwareDirect = true,
                    forceRoleAwareNestedDirect = true,
                    outputPath = "Temp/fingerprint-nested.json",
                },
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig sweep = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "smoke",
                    formalCollectorMode = "role",
                    forceRoleAwareDirect = true,
                    forceRoleAwareSweepDirect = true,
                    outputPath = "Temp/fingerprint-sweep.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            string configuredFingerprint =
                ProductionEntityStressFingerprint.BuildImplementationConfig(configured);
            string adaptiveFingerprint =
                ProductionEntityStressFingerprint.BuildImplementationConfig(adaptive);
            string directFingerprint =
                ProductionEntityStressFingerprint.BuildImplementationConfig(direct);
            string treeFingerprint =
                ProductionEntityStressFingerprint.BuildImplementationConfig(tree);
            string nestedFingerprint =
                ProductionEntityStressFingerprint.BuildImplementationConfig(nested);
            string sweepFingerprint =
                ProductionEntityStressFingerprint.BuildImplementationConfig(sweep);

            Assert.That(
                new HashSet<string>
                {
                    configuredFingerprint,
                    adaptiveFingerprint,
                    directFingerprint,
                    treeFingerprint,
                    nestedFingerprint,
                    sweepFingerprint,
                },
                Has.Count.EqualTo(6));
        }

        [TestCase("dispersed", 0, 1000)]
        [TestCase("dispersed", 100, 100)]
        [TestCase("dispersed", 300, 300)]
        [TestCase("dispersed", 500, 500)]
        [TestCase("dispersed100", 0, 100)]
        [TestCase("dispersed300", 300, 300)]
        [TestCase("dispersed500", 500, 500)]
        [TestCase("dispersed1000", 1000, 1000)]
        public void DispersedEntityLadder_AcceptsOnlyFrozenPopulationActions(
            string action,
            int entityCount,
            int expectedEntityCount)
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = action,
                    entityCount = entityCount,
                    outputPath = "Temp/dispersed-ladder.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.Mode, Is.EqualTo(ProductionEntityStressMode.Dispersed1000));
            Assert.That(config.EntityCount, Is.EqualTo(expectedEntityCount));
        }

        [Test]
        public void DispersedEntityLadder_RejectsUnsupportedAndConflictingCounts()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    new ProductionEntityStressRequest
                    {
                        action = "dispersed",
                        entityCount = 200,
                        outputPath = "Temp/dispersed-unsupported.json",
                    },
                    ProductionEntityStressPaths.ProjectRoot));
            Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    new ProductionEntityStressRequest
                    {
                        action = "dispersed100",
                        entityCount = 300,
                        outputPath = "Temp/dispersed-conflict.json",
                    },
                    ProductionEntityStressPaths.ProjectRoot));
        }

        [TestCase(100)]
        [TestCase(300)]
        [TestCase(500)]
        [TestCase(1000)]
        public void DispersedAiSimulationSmokeRequest_IsFrozenAndAutoStopping(int entityCount)
        {
            ProductionEntityStressRequest request =
                ProductionEntityStressWindow.CreateDispersedAiSimulationOnlySmokeRequest(
                    entityCount,
                    "Temp/dispersed-ai-sim-smoke.json",
                    sampleTicks: 10);
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.EntityCount, Is.EqualTo(entityCount));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(config.WarmupTicks, Is.EqualTo(30));
            Assert.That(config.SampleTicks, Is.EqualTo(10));
            Assert.That(config.SimulationOnly, Is.True);
            Assert.That(config.ShouldAutoStopWhenSampled, Is.True);
            Assert.That(config.SpawnBatchSize, Is.EqualTo(25));
            Assert.That(config.MaxCatchUpTicksPerFrame, Is.EqualTo(1));
            Assert.That(config.MaxBacklogTicks, Is.EqualTo(8));
            Assert.That(config.Seed, Is.EqualTo(0x4E545344u));
            Assert.That(
                ProductionEntityStressConfig.FormatAiSensingMode(config.AiSensingMode),
                Is.EqualTo("legacy"));
        }

        [Test]
        public void DispersedAiSimulationSmokeRequest_RejectsLongSampleRuns()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ProductionEntityStressWindow.CreateDispersedAiSimulationOnlySmokeRequest(
                    100,
                    "Temp/dispersed-ai-sim-smoke-invalid.json",
                    sampleTicks: 31));
        }

        [Test]
        public void Fingerprints_CoverSchedulingAndFastPathConfiguration()
        {
            var baselineRequest = new ProductionEntityStressRequest
            {
                action = "dispersed100",
                entityCount = 100,
                inputMode = "ai",
                simulationOnly = true,
                seed = 123u,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
                formalCollectorMode = "role",
                outputPath = "Temp/fingerprint-baseline.json",
            };
            var scheduledRequest = new ProductionEntityStressRequest
            {
                action = baselineRequest.action,
                entityCount = baselineRequest.entityCount,
                inputMode = baselineRequest.inputMode,
                simulationOnly = baselineRequest.simulationOnly,
                seed = baselineRequest.seed,
                spawnBatchSize = 26,
                maxCatchUpTicksPerFrame = baselineRequest.maxCatchUpTicksPerFrame,
                maxBacklogTicks = baselineRequest.maxBacklogTicks,
                formalCollectorMode = baselineRequest.formalCollectorMode,
                outputPath = "Temp/fingerprint-scheduled.json",
            };
            var fastPathRequest = new ProductionEntityStressRequest
            {
                action = baselineRequest.action,
                entityCount = baselineRequest.entityCount,
                inputMode = baselineRequest.inputMode,
                simulationOnly = baselineRequest.simulationOnly,
                seed = baselineRequest.seed,
                spawnBatchSize = baselineRequest.spawnBatchSize,
                maxCatchUpTicksPerFrame = baselineRequest.maxCatchUpTicksPerFrame,
                maxBacklogTicks = baselineRequest.maxBacklogTicks,
                formalCollectorMode = baselineRequest.formalCollectorMode,
                enableCollisionRoleZeroItrFastPath = true,
                outputPath = "Temp/fingerprint-fastpath.json",
            };
            ProductionEntityStressConfig baseline = ProductionEntityStressConfig.FromRequest(
                baselineRequest,
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig scheduled = ProductionEntityStressConfig.FromRequest(
                scheduledRequest,
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig fastPath = ProductionEntityStressConfig.FromRequest(
                fastPathRequest,
                ProductionEntityStressPaths.ProjectRoot);
            baselineRequest.inputMode = "none";
            ProductionEntityStressConfig noInput = ProductionEntityStressConfig.FromRequest(
                baselineRequest,
                ProductionEntityStressPaths.ProjectRoot);
            baselineRequest.inputMode = "ai";
            baselineRequest.seed++;
            ProductionEntityStressConfig differentSeed = ProductionEntityStressConfig.FromRequest(
                baselineRequest,
                ProductionEntityStressPaths.ProjectRoot);
            baselineRequest.seed--;
            baselineRequest.simulationOnly = false;
            ProductionEntityStressConfig withPresentation = ProductionEntityStressConfig.FromRequest(
                baselineRequest,
                ProductionEntityStressPaths.ProjectRoot);
            baselineRequest.simulationOnly = true;
            baselineRequest.formalCollectorMode = "brute";
            ProductionEntityStressConfig bruteCollector = ProductionEntityStressConfig.FromRequest(
                baselineRequest,
                ProductionEntityStressPaths.ProjectRoot);
            baselineRequest.formalCollectorMode = "role";
            baselineRequest.enableCollisionCandidateStoreAuthority = true;
            ProductionEntityStressConfig candidateStoreAuthority =
                ProductionEntityStressConfig.FromRequest(
                    baselineRequest,
                    ProductionEntityStressPaths.ProjectRoot);

            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(scheduled),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(baseline)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(noInput),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(baseline)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(differentSeed),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(baseline)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(withPresentation),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(baseline)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(bruteCollector),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(baseline)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildImplementationConfig(fastPath),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildImplementationConfig(baseline)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildImplementationConfig(candidateStoreAuthority),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildImplementationConfig(baseline)));
        }

        [TestCase("dispersed", ProductionEntityStressMode.Dispersed1000)]
        [TestCase("combat", ProductionEntityStressMode.Combat1000)]
        [TestCase("concentrated", ProductionEntityStressMode.Concentrated1000)]
        public void ProductionModes_RequestOneThousandEntities(
            string action,
            ProductionEntityStressMode expectedMode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = action,
                warmupTicks = 30,
                sampleTicks = 300,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
                outputPath = $"Temp/{action}.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.Mode, Is.EqualTo(expectedMode));
            Assert.That(config.EntityCount, Is.EqualTo(1000));
            Assert.That(config.AutoCleanup, Is.False);
        }

        [Test]
        public void CombatZeroGcRequest_UsesOneThousandAiAndLongFormalWindow()
        {
            ProductionEntityStressRequest request =
                ProductionEntityStressWindow.CreateCombatZeroGcRequest(
                    "Temp/combat1000-zero-gc.json");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.Mode, Is.EqualTo(ProductionEntityStressMode.Combat1000));
            Assert.That(config.EntityCount, Is.EqualTo(1000));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(config.WarmupTicks, Is.EqualTo(120));
            Assert.That(config.SampleTicks, Is.EqualTo(1800));
            Assert.That(config.ShouldAutoStopWhenSampled, Is.True);
            Assert.That(config.RequireZeroGcAfterWarmup, Is.True);
        }

        [TestCase("legacy", BattleAiExecutionProfile.LegacyCanonical)]
        [TestCase(
            "data-oriented-canonical",
            BattleAiExecutionProfile.DataOrientedCanonical)]
        public void CombatCapacityPressureSmokeRequest_ChangesOnlyTheAiExecutionProfile(
            string profile,
            BattleAiExecutionProfile expectedProfile)
        {
            ProductionEntityStressRequest request =
                ProductionEntityStressWindow.CreateCombatCapacityPressureSmokeRequest(
                    $"Temp/combat1000-{profile}.json",
                    profile);
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.Mode, Is.EqualTo(ProductionEntityStressMode.Combat1000));
            Assert.That(config.EntityCount, Is.EqualTo(1000));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(config.WarmupTicks, Is.EqualTo(30));
            Assert.That(config.SampleTicks, Is.EqualTo(180));
            Assert.That(config.SpawnBatchSize, Is.EqualTo(100));
            Assert.That(config.ShouldAutoStopWhenSampled, Is.True);
            Assert.That(config.RequireZeroGcAfterWarmup, Is.True);
            Assert.That(config.AiExecutionProfile, Is.EqualTo(expectedProfile));
            Assert.That(config.EnablePhaseTiming, Is.True);
            Assert.That(config.EnablePresentationTiming, Is.True);
            Assert.That(config.EnableDetailPhaseTiming, Is.True);
        }

        [Test]
        public void CombatPerformanceSmokeRequest_DisablesNestedTimingButKeepsTheRuntimeGate()
        {
            ProductionEntityStressRequest request =
                ProductionEntityStressWindow.CreateCombatPerformanceSmokeRequest(
                    "Temp/combat1000-data-oriented-performance.json",
                    "data-oriented-canonical");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(
                config.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
            Assert.That(config.EntityCount, Is.EqualTo(1000));
            Assert.That(config.WarmupTicks, Is.EqualTo(30));
            Assert.That(config.SampleTicks, Is.EqualTo(180));
            Assert.That(config.RequireZeroGcAfterWarmup, Is.True);
            Assert.That(config.EnablePhaseTiming, Is.False);
            Assert.That(config.EnablePresentationTiming, Is.False);
            Assert.That(config.EnableDetailPhaseTiming, Is.False);
        }

        [Test]
        public void CombatSteadyStateRequest_UsesLongDataOrientedZeroGcGateWithoutTimingOverhead()
        {
            ProductionEntityStressRequest request =
                ProductionEntityStressWindow.CreateCombatSteadyStateRequest(
                    "Temp/combat1000-data-oriented-steady-state.json",
                    "data-oriented-canonical");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(
                config.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
            Assert.That(config.EntityCount, Is.EqualTo(1000));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(config.WarmupTicks, Is.EqualTo(120));
            Assert.That(config.SampleTicks, Is.EqualTo(1800));
            Assert.That(config.SpawnBatchSize, Is.EqualTo(100));
            Assert.That(config.ShouldAutoStopWhenSampled, Is.True);
            Assert.That(config.RequireZeroGcAfterWarmup, Is.True);
            Assert.That(config.EnablePhaseTiming, Is.False);
            Assert.That(config.EnablePresentationTiming, Is.False);
            Assert.That(config.EnableDetailPhaseTiming, Is.False);
        }

        [Test]
        public void AutoStopWhenSampled_CatchUpStopsAtExactTargetSampleCount()
        {
            Assert.That(
                ProductionEntityStressRunner.ShouldExecuteCatchUpTickForStressSample(
                    shouldAutoStopWhenSampled: true,
                    sampledLogicTicks: 9,
                    targetSampleTicks: 10),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.ShouldExecuteCatchUpTickForStressSample(
                    shouldAutoStopWhenSampled: true,
                    sampledLogicTicks: 10,
                    targetSampleTicks: 10),
                Is.False);
            Assert.That(
                ProductionEntityStressRunner.ShouldExecuteCatchUpTickForStressSample(
                    shouldAutoStopWhenSampled: false,
                    sampledLogicTicks: 10,
                    targetSampleTicks: 10),
                Is.True);
            Assert.That(
                ProductionEntityStressRunner.ShouldExecuteCatchUpTickForStressSample(
                    shouldAutoStopWhenSampled: false,
                    sampledLogicTicks: 100000,
                    targetSampleTicks: 10),
                Is.True,
                "A visible run with autoStop=false must keep ticking after sampling completes.");
        }

        [Test]
        public void SpawnLayouts_SeparateDispersedAndConcentratedDomains()
        {
            Vector3 dispersedFirst = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Dispersed1000,
                0,
                1000);
            Vector3 dispersedLast = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Dispersed1000,
                999,
                1000);
            Vector3 concentratedFirst = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Concentrated1000,
                0,
                1000);
            Vector3 concentratedLast = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Concentrated1000,
                999,
                1000);

            Assert.That(Vector3.Distance(dispersedFirst, dispersedLast), Is.GreaterThan(700f));
            Assert.That(Vector3.Distance(concentratedFirst, concentratedLast), Is.LessThan(40f));
        }

        [Test]
        public void CombatSpawnLayout_CreatesAuthorityRangeGroupsWithoutOneCellPileup()
        {
            Vector3 first = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Combat1000,
                0,
                1000);
            Vector3 firstOpponent = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Combat1000,
                1,
                1000);
            Vector3 sameSideNextLane = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Combat1000,
                2,
                1000);
            Vector3 nextGroup = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Combat1000,
                20,
                1000);
            Vector3 last = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Combat1000,
                999,
                1000);

            Assert.That(Vector3.Distance(first, firstOpponent), Is.EqualTo(120f).Within(0.001f));
            Assert.That(Vector3.Distance(first, sameSideNextLane), Is.EqualTo(1f).Within(0.001f));
            Assert.That(Vector3.Distance(first, nextGroup), Is.EqualTo(160f).Within(0.001f));
            Assert.That(Vector3.Distance(first, last), Is.GreaterThan(700f));
        }

        [Test]
        public void MetricSummary_ComputesInterpolatedPercentiles()
        {
            var values = new List<double> { 1d, 2d, 3d, 4d, 5d };
            ProductionEntityStressMetricSummary summary =
                ProductionEntityStressStatistics.Summarize(values, "ms", "test");

            Assert.That(summary.available, Is.True);
            Assert.That(summary.sampleCount, Is.EqualTo(5));
            Assert.That(summary.average, Is.EqualTo(3d));
            Assert.That(summary.maximum, Is.EqualTo(5d));
            Assert.That(summary.p95, Is.EqualTo(4.8d).Within(0.0001d));
            Assert.That(summary.p99, Is.EqualTo(4.96d).Within(0.0001d));
        }

        [Test]
        public void BattleTickPhaseRecorder_OffDoesNotRecord_AndOnRecords()
        {
            var recorder = new BattleTickPhaseDiagnostics();

            recorder.BeginTick(10);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);

            Assert.That(recorder.Enabled, Is.False);
            Assert.That(recorder.LastTickIndex, Is.EqualTo(-1));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.CharacterInput),
                Is.EqualTo(0));

            recorder.SetEnabled(true);
            recorder.BeginTick(11);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);

            Assert.That(recorder.LastTickIndex, Is.EqualTo(11));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.CharacterInput),
                Is.GreaterThan(0));
        }

        [Test]
        public void BattleTickPhaseRecorder_BeginTickResetsLastValues()
        {
            var recorder = new BattleTickPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(20);
            recorder.BeginPhase(BattleTickPhase.FrameAdvance);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.FrameAdvance);
            Assert.That(recorder.GetLastPhaseSumTimestampTicks(), Is.GreaterThan(0));

            recorder.BeginTick(21);

            Assert.That(recorder.LastTickIndex, Is.EqualTo(21));
            Assert.That(recorder.GetLastPhaseSumTimestampTicks(), Is.EqualTo(0));
        }

        [Test]
        public void BattleTickPhaseRecorder_RepeatedPhaseAccumulatesWithinTick()
        {
            var recorder = new BattleTickPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(30);

            recorder.BeginPhase(BattleTickPhase.StageBounds);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.StageBounds);
            long firstElapsed = recorder.GetLastElapsedTimestampTicks(
                BattleTickPhase.StageBounds);

            recorder.BeginPhase(BattleTickPhase.StageBounds);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.StageBounds);

            Assert.That(firstElapsed, Is.GreaterThan(0));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.StageBounds),
                Is.GreaterThan(firstElapsed));
        }

        [Test]
        public void BattleTickSystem_InputClearEarlyReturnClosesItsPhase()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics recorder =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            world.SetNeedClearInput(true);

            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.RunReleaseTick(31);
            long inputClearElapsed = recorder.GetLastElapsedTimestampTicks(
                BattleTickPhase.InputClear);

            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.InputClear);

            Assert.That(world.NeedClearInput, Is.False);
            Assert.That(recorder.LastTickIndex, Is.EqualTo(31));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.InputClear),
                Is.EqualTo(inputClearElapsed),
                "EndPhase after the early return must be ignored because InputClear was closed.");
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.CharacterInput),
                Is.EqualTo(0));
        }

        [Test]
        public void BattleTickPhaseRecorder_PhaseNamesAndCountAreStable()
        {
            string[] expected =
            {
                "BattleFlow",
                "Cooldown",
                "HumanInput",
                "RuntimeMaintenance",
                "InputClear",
                "CharacterInput",
                "EarlyFrameAdvance",
                "FrameLogic",
                "FrameAdvance",
                "DeathCleanup",
                "StageBounds",
                "PreInteraction",
                "HeldLinkValidation",
                "HeldProcess",
                "CollisionSnapshot",
                "PairVRest",
                "CandidateCollect",
                "CharacterHitConsumePostInteraction",
                "RandomWeaponDrop",
                "ObjectHitConsume",
                "CandidateConsumptionEnd",
                "PreFrameBounds",
                "Stage",
                "RenderDispatch",
                "FramePostProcess",
                "LateEntityUpdate",
                "RandomWeaponDropTail",
                "EntityPostFrameTail",
                "BattleResults",
            };

            Assert.That(BattleTickPhaseDiagnostics.PhaseCount, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(
                    BattleTickPhaseDiagnostics.GetPhaseName((BattleTickPhase)i),
                    Is.EqualTo(expected[i]),
                    $"Phase id {i} changed its diagnostic contract.");
            }
        }

        [Test]
        public void PhaseTimingCollector_SamplesOnlyAfterWarmup()
        {
            var recorder = new BattleTickPhaseDiagnostics();
            var collector = new ProductionEntityStressPhaseTimingCollector();
            recorder.SetEnabled(true);

            recorder.BeginTick(1);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);
            collector.CaptureAfterTick(recorder, 10d, 1, 1);
            Assert.That(collector.SampleCount, Is.EqualTo(0));

            recorder.BeginTick(2);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);
            collector.CaptureAfterTick(recorder, 10d, 2, 1);

            var report = new ProductionEntityStressReport();
            collector.PopulateReport(report);
            Assert.That(collector.SampleCount, Is.EqualTo(1));
            Assert.That(report.phaseTimings, Has.Count.EqualTo(BattleTickPhaseDiagnostics.PhaseCount));
            Assert.That(
                report.phaseTimings[(int)BattleTickPhase.CharacterInput].timing.sampleCount,
                Is.EqualTo(1));
            Assert.That(report.phaseTimingUnattributedMilliseconds.sampleCount, Is.EqualTo(1));
        }

        [Test]
        public void PhaseTimingLifecycle_DisablesRecorderDuringCleanup()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics recorder =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            Assert.That(world.ActiveBattleTickPhaseDiagnosticsForDiagnostics, Is.SameAs(recorder));

            ProductionEntityStressPhaseTimingLifecycle.Disable(world);

            Assert.That(recorder.Enabled, Is.False);
            Assert.That(world.ActiveBattleTickPhaseDiagnosticsForDiagnostics, Is.Null);
        }

        [TestCase(true, "InterruptedCleanly")]
        [TestCase(false, "InterruptedWithResidue")]
        public void RunStatusPolicy_LabelsDirectDestroyAsInterrupted(
            bool restored,
            string expected)
        {
            Assert.That(
                ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    "Running",
                    "runner-destroyed",
                    restored),
                Is.EqualTo(expected));
        }

        [Test]
        public void RunStatusPolicy_PreservesNormalStopAndFailureStatuses()
        {
            Assert.That(
                ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    "Running",
                    "manual-stop",
                    true),
                Is.EqualTo("Running"));
            Assert.That(
                ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    "Failed",
                    "exception",
                    false),
                Is.EqualTo("Failed"));
        }

        [Test]
        public void ReplenishmentPolicy_SaturatedRosterGapDrainsInsteadOfAttemptingCreation()
        {
            ProductionEntityStressReplenishmentAction action =
                ProductionEntityStressReplenishmentPolicy.Evaluate(
                    baseRosterActiveCount: 999,
                    requestedEntityCount: 1000,
                    totalActiveRuntimeEntityCount: 1000,
                    totalClaimedRuntimeSlotCount: 1000,
                    maximumActiveRuntimeEntityCount: 1000,
                    currentSaturationDrainTicks: 0,
                    maximumSaturationDrainTicks: 3);

            Assert.That(action, Is.EqualTo(
                ProductionEntityStressReplenishmentAction.Drain));
        }

        [Test]
        public void ReplenishmentPolicy_RecoveryReopensOriginalCreationChain()
        {
            ProductionEntityStressReplenishmentAction action =
                ProductionEntityStressReplenishmentPolicy.Evaluate(
                    baseRosterActiveCount: 999,
                    requestedEntityCount: 1000,
                    totalActiveRuntimeEntityCount: 999,
                    totalClaimedRuntimeSlotCount: 999,
                    maximumActiveRuntimeEntityCount: 1000,
                    currentSaturationDrainTicks: 2,
                    maximumSaturationDrainTicks: 3);

            Assert.That(action, Is.EqualTo(
                ProductionEntityStressReplenishmentAction.Attempt));
        }

        [Test]
        public void ReplenishmentPolicy_TimeoutPublishesStructuredResult()
        {
            ProductionEntityStressReplenishmentAction action =
                ProductionEntityStressReplenishmentPolicy.Evaluate(
                    baseRosterActiveCount: 999,
                    requestedEntityCount: 1000,
                    totalActiveRuntimeEntityCount: 1000,
                    totalClaimedRuntimeSlotCount: 1000,
                    maximumActiveRuntimeEntityCount: 1000,
                    currentSaturationDrainTicks: 3,
                    maximumSaturationDrainTicks: 3);

            Assert.That(action, Is.EqualTo(
                ProductionEntityStressReplenishmentAction
                    .SaturationBlockedReplenishment));
            Assert.That(
                ProductionEntityStressReplenishmentPolicy.SaturationBlockedResult,
                Is.EqualTo("SaturationBlockedReplenishment"));
        }

        [Test]
        public void SamplePolicy_RequiresCompleteUnmutatedRosterAndNoPoolExpansion()
        {
            Assert.That(
                ProductionEntityStressSamplePolicy.IsSteadyStateSample(
                    31, 30, 1000, 1000, false, false),
                Is.True);
            Assert.That(
                ProductionEntityStressSamplePolicy.IsSteadyStateSample(
                    31, 30, 999, 1000, false, false),
                Is.False);
            Assert.That(
                ProductionEntityStressSamplePolicy.IsSteadyStateSample(
                    31, 30, 1000, 1000, true, false),
                Is.False);
            Assert.That(
                ProductionEntityStressSamplePolicy.IsSteadyStateSample(
                    31, 30, 1000, 1000, false, true),
                Is.False);
        }

        [Test]
        public void CatchUpCpuBudget_PredictsNextTickWithoutBlockingTheFirstTick()
        {
            Assert.That(
                ProductionEntityStressRunner.ShouldDeferCatchUpTickForCpuBudget(
                    cpuBudgetMs: 0d,
                    ticksAlreadyExecuted: 1,
                    elapsedCatchUpMs: 30d,
                    previousTickMs: 30d),
                Is.False,
                "A zero budget must preserve the throughput loop.");
            Assert.That(
                ProductionEntityStressRunner.ShouldDeferCatchUpTickForCpuBudget(
                    cpuBudgetMs: 33.333d,
                    ticksAlreadyExecuted: 0,
                    elapsedCatchUpMs: 0d,
                    previousTickMs: 30d),
                Is.False,
                "The first tick must always be allowed.");
            Assert.That(
                ProductionEntityStressRunner.ShouldDeferCatchUpTickForCpuBudget(
                    cpuBudgetMs: 33.333d,
                    ticksAlreadyExecuted: 1,
                    elapsedCatchUpMs: 10d,
                    previousTickMs: 10d),
                Is.False);
            Assert.That(
                ProductionEntityStressRunner.ShouldDeferCatchUpTickForCpuBudget(
                    cpuBudgetMs: 33.333d,
                    ticksAlreadyExecuted: 1,
                    elapsedCatchUpMs: 30d,
                    previousTickMs: 30d),
                Is.True,
                "A second heavy tick must be deferred instead of multiplying the frame stall.");
        }

        [Test]
        public void CatchUpCpuBudget_IsParsedReportedAndFingerprinted()
        {
            var throughputRequest = new ProductionEntityStressRequest
            {
                action = "dispersed100",
                entityCount = 100,
                catchUpCpuBudgetMs = 0f,
                outputPath = "Temp/catchup-throughput.json",
            };
            var interactiveRequest = new ProductionEntityStressRequest
            {
                action = throughputRequest.action,
                entityCount = throughputRequest.entityCount,
                catchUpCpuBudgetMs = 1000f / 30f,
                outputPath = "Temp/catchup-interactive.json",
            };

            ProductionEntityStressConfig throughput =
                ProductionEntityStressConfig.FromRequest(
                    throughputRequest,
                    ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig interactive =
                ProductionEntityStressConfig.FromRequest(
                    interactiveRequest,
                    ProductionEntityStressPaths.ProjectRoot);

            Assert.That(throughput.LimitsCatchUpByCpuBudget, Is.False);
            Assert.That(interactive.LimitsCatchUpByCpuBudget, Is.True);
            Assert.That(interactive.CatchUpCpuBudgetMs, Is.EqualTo(1000d / 30d).Within(0.001d));
            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(interactive),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(throughput)));
        }

        [Test]
        public void RunStatusPolicy_CleanupPreservesSaturationBlockedResult()
        {
            Assert.That(
                ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    ProductionEntityStressReplenishmentPolicy.SaturationBlockedResult,
                    "saturation-blocked-replenishment",
                    true),
                Is.EqualTo("SaturationBlockedReplenishment"));
        }

        [Test]
        public void RequestProcessor_OnlyEntersPlayModeForStartActions()
        {
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldEnterPlayMode("dispersed", false),
                Is.True);
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldEnterPlayMode("stop", false),
                Is.False);
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldEnterPlayMode("concentrated", true),
                Is.False);
        }

        [Test]
        public void PopulationPolicy_AccountsForRendererAndCharacterRegistrations()
        {
            Assert.That(
                ProductionEntityStressPopulationPolicy.Evaluate(
                    50,
                    50,
                    50,
                    100,
                    50,
                    50),
                Is.True);
            Assert.That(
                ProductionEntityStressPopulationPolicy.Evaluate(
                    50,
                    50,
                    50,
                    50,
                    50,
                    50),
                Is.False,
                "ObjectCount must include each LF2ObjectRenderer plus its LF2Character.");
        }

        [Test]
        public void BootstrapGate_RequiresSuppressedBootstrapAndProductionServices()
        {
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldSuppressBattleTestBootstrap("dispersed"),
                Is.True);
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldSuppressBattleTestBootstrap("stop"),
                Is.False);
            Assert.That(
                ProductionEntityStressRequestProcessor.IsReadyToStart(true, false, true),
                Is.False);
            Assert.That(
                ProductionEntityStressRequestProcessor.IsReadyToStart(true, true, true),
                Is.True);
            Assert.That(
                ProductionEntityStressRequestProcessor.IsReadyToStart(false, false, true),
                Is.True);
        }

        [Test]
        public void TeardownPolicy_RequiresActiveStateToReturnToBaseline()
        {
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 0, 0, 20, 0,
                    0, 12, 0),
                Is.True);
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 1, 0, 12, 0,
                    0, 12, 0),
                Is.False);
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 0, 0, 11, 0,
                    0, 12, 0),
                Is.True,
                "Retained inactive pool capacity is an allowed cache and is not active residue.");
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 0, 1, 1001, 0,
                    0, 12, 0),
                Is.False,
                "Active pooled objects still prevent restoration.");
        }

        [Test]
        public void CleanupJournal_ContinuesAfterAnIndividualCleanupFailure()
        {
            var order = new List<string>();
            var journal = new ProductionEntityStressCleanupJournal();

            bool failed = journal.Attempt("release-entity-0", () =>
            {
                order.Add("first");
                throw new InvalidOperationException("injected release failure");
            });
            bool continued = journal.Attempt("release-entity-1", () => order.Add("second"));
            bool restored = journal.Attempt("restore-driver", () => order.Add("restore"));

            Assert.That(failed, Is.False);
            Assert.That(continued, Is.True);
            Assert.That(restored, Is.True);
            Assert.That(order, Is.EqualTo(new[] { "first", "second", "restore" }));
            Assert.That(journal.FailureCount, Is.EqualTo(1));
            Assert.That(journal.FormatFailures(), Does.Contain("release-entity-0"));
            Assert.That(journal.FormatFailures(), Does.Contain("injected release failure"));
        }

        [Test]
        public void CleanupJournal_GateASettersRemainIndependentAfterOneFailure()
        {
            var restored = new List<string>();
            var journal = new ProductionEntityStressCleanupJournal();

            journal.Attempt("restore-ai-decision-execution-mode", () =>
            {
                restored.Add("decision-mode");
                throw new InvalidOperationException("injected decision mode setter failure");
            });
            journal.Attempt(
                "restore-ai-decision-oracle-interval",
                () => restored.Add("oracle-interval"));
            journal.Attempt(
                "restore-unified-ai-snapshot-shadow-mode",
                () => restored.Add("unified-shadow"));
            journal.Attempt(
                "disable-battle-tick-detail-timing",
                () => restored.Add("detail-timing"));

            Assert.That(
                restored,
                Is.EqualTo(new[]
                {
                    "decision-mode",
                    "oracle-interval",
                    "unified-shadow",
                    "detail-timing",
                }));
            Assert.That(journal.FailureCount, Is.EqualTo(1));
            Assert.That(
                journal.FormatFailures(),
                Does.Contain("restore-ai-decision-execution-mode"));
        }

        [Test]
        public void ActiveGameObjectAfterScan_UsesStressRootInsteadOfTrackingList()
        {
            var stressRoot = new GameObject("stress-root");
            try
            {
                var active = new GameObject("active-residue");
                active.transform.SetParent(stressRoot.transform);
                var inactive = new GameObject("retained-inactive");
                inactive.transform.SetParent(stressRoot.transform);
                inactive.SetActive(false);
                var trackingList = new List<GameObject> { active };
                trackingList.Clear();

                Assert.That(trackingList, Is.Empty);
                Assert.That(
                    ProductionEntityStressTeardownPolicy.CountActiveStressRootGameObjects(
                        stressRoot.transform),
                    Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(stressRoot);
            }
        }

        [Test]
        public void TeardownEvidence_SeparatesRetainedInactiveCapacityFromActiveCleanup()
        {
            var teardown = new ProductionEntityStressTeardownReport
            {
                restored = true,
                activeStateRestored = true,
                driverStateRestored = true,
                loggingStateRestored = true,
                objectPoolActiveBeforeRun = 0,
                objectPoolActiveAfter = 0,
                referencePoolActiveBeforeRun = 0,
                referencePoolActiveAfter = 0,
                retainedInactiveObjectPoolCapacityBeforeRun = 10,
                retainedInactiveObjectPoolCapacityAfter = 1001,
                retainedInactiveObjectPoolCapacityDelta = 991,
            };

            string evidence = ProductionEntityStressTeardownPolicy.BuildEvidence(
                "test-cleanup",
                teardown);

            Assert.That(evidence, Does.Contain("activeCleanupRestored=True"));
            Assert.That(evidence, Does.Contain("retainedInactiveObjectPoolCapacity=10->1001"));
            Assert.That(evidence, Does.Contain("doesNotAffectRestored=True"));
        }

        [Test]
        public void ActiveRuntimeEntitySnapshot_DeduplicatesSparseSlotsAndKeepsDerivedOrder()
        {
            CreateActiveRuntimeEntitySnapshotFixture(
                out SimulationWorld world,
                out LF2Character low,
                out LF2OtherObject derived,
                out LF2Character pending,
                out LF2Character high);
            var snapshot = new List<LF2Entity>
            {
                high,
                high,
                pending,
            };

            world.GetActiveRuntimeEntitySnapshotForDiagnostics(snapshot);

            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(4));
            Assert.That(snapshot, Has.Count.EqualTo(3));
            Assert.That(snapshot[0], Is.SameAs(low));
            Assert.That(snapshot[1], Is.SameAs(derived));
            Assert.That(snapshot[2], Is.SameAs(high));
            Assert.That(snapshot.Contains(pending), Is.False);
            Assert.That(
                derived.Runtime.SpawnSemantic,
                Is.EqualTo((int)NTSD.Animation.LF2Tasks.ReleaseSpawnSemantic.LateOpoint));
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    derived.Runtime.SlotIndex,
                    derived,
                    out RuntimeEntityHandle derivedHandle),
                Is.True);
            Assert.That(derivedHandle.IsValid, Is.True);

            world.GetActiveRuntimeEntitySnapshotForDiagnostics(snapshot);
            Assert.That(snapshot, Has.Count.EqualTo(3));
            Assert.That(snapshot[0], Is.SameAs(low));
            Assert.That(snapshot[1], Is.SameAs(derived));
            Assert.That(snapshot[2], Is.SameAs(high));
        }

        [Test]
        public void ActiveRuntimeEntitySnapshot_ReusesContainersWithoutSteadyStateAllocation()
        {
            CreateActiveRuntimeEntitySnapshotFixture(
                out SimulationWorld world,
                out LF2Character low,
                out LF2OtherObject derived,
                out _,
                out LF2Character high);
            var snapshot = new List<LF2Entity>(4);
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(snapshot);
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(snapshot);
            int warmedCapacity = snapshot.Capacity;

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(snapshot);
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

            Assert.That(allocatedBytes, Is.EqualTo(0L));
            Assert.That(snapshot.Capacity, Is.EqualTo(warmedCapacity));
            Assert.That(snapshot, Has.Count.EqualTo(3));
            Assert.That(snapshot[0], Is.SameAs(low));
            Assert.That(snapshot[1], Is.SameAs(derived));
            Assert.That(snapshot[2], Is.SameAs(high));
        }

        [Test]
        public void SimulationBucketTraversal_ObjectCountAndPairVRestAllocateNoManagedMemory()
        {
            CreateActiveRuntimeEntitySnapshotFixture(
                out SimulationWorld world,
                out _,
                out _,
                out _,
                out _);

            _ = world.ObjectCount;
            world.TickCollisionPairVRestAll();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 16; iteration++)
            {
                _ = world.ObjectCount;
                world.TickCollisionPairVRestAll();
            }
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

            Assert.That(allocatedBytes, Is.Zero);
        }

        [Test]
        public void DerivedObservationPolicy_ExcludesHarnessHandlesAndDeduplicatesGenerations()
        {
            var owned = new HashSet<RuntimeEntityHandle>
            {
                new RuntimeEntityHandle(50, 1),
            };
            var observed = new HashSet<RuntimeEntityHandle>();
            var derived = new RuntimeEntityHandle(51, 1);

            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(
                    new RuntimeEntityHandle(50, 1), owned, observed),
                Is.False);
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(derived, owned, observed),
                Is.True);
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(derived, owned, observed),
                Is.False);
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(
                    new RuntimeEntityHandle(51, 2), owned, observed),
                Is.True);
        }

        [Test]
        public void DerivedObservationPolicy_RejectsInvalidHandles()
        {
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(
                    RuntimeEntityHandle.Invalid,
                    new HashSet<RuntimeEntityHandle>(),
                    new HashSet<RuntimeEntityHandle>()),
                Is.False);
        }

        [Test]
        public void LoggingPolicy_SuppressesLogAndWarningThenRestoresTheOriginalFilter()
        {
            LogType currentFilter = LogType.Warning;
            var report = new ProductionEntityStressLoggingPolicyReport();
            var policy = new ProductionEntityStressLoggingPolicy(
                () => currentFilter,
                value => currentFilter = value);

            policy.Apply(report);
            policy.Apply(report);

            Assert.That(currentFilter, Is.EqualTo(LogType.Error));
            Assert.That(report.originalFilterLogType, Is.EqualTo(LogType.Warning.ToString()));
            Assert.That(report.runningFilterLogType, Is.EqualTo(LogType.Error.ToString()));
            Assert.That(report.applied, Is.True);
            Assert.That(report.restored, Is.False);
            Assert.That(report.policy, Does.Contain("Log and Warning"));

            policy.Restore(report);
            policy.Restore(report);

            Assert.That(currentFilter, Is.EqualTo(LogType.Warning));
            Assert.That(report.applied, Is.False);
            Assert.That(report.restored, Is.True);
        }

        private static void CreateCandidateCollectTimingFixture(
            out SimulationWorld world,
            out BruteForceSceneQuery query,
            out LF2Character attacker)
        {
            world = new SimulationWorld();
            LF2FrameData attackerFrame = CreateCandidateCollectTimingFrame(
                new InteractionArea
                {
                    kind = 0,
                    vrest = 0,
                    x = -40,
                    y = -20,
                    w = 80,
                    h = 40,
                    zwidth = 20,
                },
                body: null);
            LF2FrameData targetFrame = CreateCandidateCollectTimingFrame(
                itr: null,
                body: new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -20,
                    w = 20,
                    h = 40,
                });
            attacker = CreateCandidateCollectTimingCharacter(
                "CandidateTiming_Attacker",
                1800,
                attackerFrame);
            LF2Character left = CreateCandidateCollectTimingCharacter(
                "CandidateTiming_Left",
                1801,
                targetFrame);
            LF2Character right = CreateCandidateCollectTimingCharacter(
                "CandidateTiming_Right",
                1802,
                targetFrame);

            RegisterCandidateCollectTimingEntity(world, attacker, 0, 1, 0);
            RegisterCandidateCollectTimingEntity(world, left, 1, 2, -10);
            RegisterCandidateCollectTimingEntity(world, right, 2, 3, 10);
            query = (BruteForceSceneQuery)world.SceneQuery;
        }

        private static void CreateActiveRuntimeEntitySnapshotFixture(
            out SimulationWorld world,
            out LF2Character low,
            out LF2OtherObject derived,
            out LF2Character pending,
            out LF2Character high)
        {
            world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                256);
            LF2FrameData frame = CreateCandidateCollectTimingFrame(
                itr: null,
                body: null);
            low = CreateCandidateCollectTimingCharacter(
                "ActiveSnapshot_Low",
                1900,
                frame);
            pending = CreateCandidateCollectTimingCharacter(
                "ActiveSnapshot_Pending",
                1901,
                frame);
            high = CreateCandidateCollectTimingCharacter(
                "ActiveSnapshot_High",
                1902,
                frame);
            derived = new LF2OtherObject
            {
                Name = "ActiveSnapshot_Derived",
                ObjectId = 1903,
            };
            derived.Runtime.SpawnSemantic =
                (int)NTSD.Animation.LF2Tasks.ReleaseSpawnSemantic.LateOpoint;

            high.SetRequiredRuntimeSlot(220);
            world.Register(high);
            derived.SetRequiredRuntimeSlot(70);
            world.Register(derived);
            pending.SetRequiredRuntimeSlot(120);
            world.Register(pending);
            low.SetRequiredRuntimeSlot(3);
            world.Register(low);

            Assert.That(high.Runtime.SlotIndex, Is.EqualTo(220));
            Assert.That(derived.Runtime.SlotIndex, Is.EqualTo(70));
            Assert.That(pending.Runtime.SlotIndex, Is.EqualTo(120));
            Assert.That(low.Runtime.SlotIndex, Is.EqualTo(3));

            LogAssert.Expect(
                LogType.Warning,
                $"[SimulationWorld] Object already registered: " +
                $"SimOrder={low.SimOrder}, StableId={low.StableId}");
            world.Register(low);
            pending.Runtime.PendingFlushDestroy = true;
        }

        private static CandidateCollectTimingRun RunCandidateCollectTimingFixture(
            SimulationWorld world,
            BruteForceSceneQuery query,
            LF2Character attacker,
            CollisionFormalCollectorMode collectorMode,
            bool forceDirect,
            bool forceTree)
        {
            query.FormalCollectorMode = collectorMode;
            query.ForceRoleAwareDirectForDiagnostics = forceDirect;
            query.ForceRoleAwareTreeForDiagnostics = forceTree;
            world.Rng.Seed(0xC011EC7u);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    attacker,
                    out List<SceneQueryHit> candidates),
                Is.True);
            var result = new CandidateCollectTimingRun(
                new List<SceneQueryHit>(candidates),
                world.Rng.State,
                world.Rng.CallCount);
            world.EndCollisionCandidateConsumption();
            return result;
        }

        private static void AssertCandidateCollectTimingRunsEqual(
            CandidateCollectTimingRun expected,
            CandidateCollectTimingRun actual)
        {
            Assert.That(actual.RngState, Is.EqualTo(expected.RngState));
            Assert.That(actual.RngCalls, Is.EqualTo(expected.RngCalls));
            Assert.That(actual.Candidates, Has.Count.EqualTo(expected.Candidates.Count));
            for (int candidateIndex = 0;
                 candidateIndex < expected.Candidates.Count;
                 candidateIndex++)
            {
                SceneQueryHit expectedHit = expected.Candidates[candidateIndex];
                SceneQueryHit actualHit = actual.Candidates[candidateIndex];
                Assert.That(actualHit.TargetSlot, Is.EqualTo(expectedHit.TargetSlot));
                Assert.That(actualHit.BodyX, Is.EqualTo(expectedHit.BodyX));
                Assert.That(actualHit.ItrIndex, Is.EqualTo(expectedHit.ItrIndex));
                Assert.That(
                    actualHit.ZeroAttackerHpOnConsume,
                    Is.EqualTo(expectedHit.ZeroAttackerHpOnConsume));
                Assert.That(
                    actualHit.ReleaseHeavyHeldTargetOnConsume,
                    Is.EqualTo(expectedHit.ReleaseHeavyHeldTargetOnConsume));
            }
        }

        private static LF2FrameData CreateCandidateCollectTimingFrame(
            InteractionArea itr,
            BodyBox body)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (itr != null)
                frame.itrs.Add(itr);
            if (body != null)
                frame.bodies.Add(body);
            return frame;
        }

        private static LF2Character CreateCandidateCollectTimingCharacter(
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new CandidateCollectTimingController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            return character;
        }

        private static void RegisterCandidateCollectTimingEntity(
            SimulationWorld world,
            LF2Entity entity,
            int requiredSlot,
            int team,
            int x)
        {
            entity.SetRequiredRuntimeSlot(requiredSlot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(requiredSlot));
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.FrameDelay = 0;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class CandidateCollectTimingRun
        {
            public CandidateCollectTimingRun(
                List<SceneQueryHit> candidates,
                uint rngState,
                ulong rngCalls)
            {
                Candidates = candidates;
                RngState = rngState;
                RngCalls = rngCalls;
            }

            public List<SceneQueryHit> Candidates { get; }
            public uint RngState { get; }
            public ulong RngCalls { get; }
        }

        private sealed class StressSoundEmitter : LF2Entity
        {
            private readonly SimulationWorld world;

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            internal StressSoundEmitter(SimulationWorld world)
            {
                this.world = world;
                Name = "ProductionEntityStressSoundEmitter";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public override void SimTransit(int tickIndex)
            {
                world.QueueSound("SFX_STRESS_TEST", tickIndex);
            }

            public override void SimTU(int tickIndex) { }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class StressSoundDriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            internal StressSoundDriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);

                host = new GameObject("ProductionEntityStressSoundTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                Assert.That(Driver.TryConfigureEmptyDiagnosticWorld(
                    new BattleRuntimeWorldSettings(
                        BattleRuntimeProfile.Authority400,
                        BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity,
                        BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity),
                    out string failureReason), Is.True, failureReason);
                Driver.ApplySettings(new LockstepSimulationSettings
                {
                    driveMode = SimulationDriveMode.Manual,
                    requireInputFrameReady = false,
                    enableFrameChecksum = true,
                });
                Driver.SetPaused(true);
            }

            internal SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                Driver.World?.ResetRuntimeState();
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }

        private sealed class CandidateCollectTimingController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
