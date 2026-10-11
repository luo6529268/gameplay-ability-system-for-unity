#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class H07OwnedInputEvaluationEditorTests
    {
        private const int CanonicalFieldCount = 41;
        private const int MobileCapacity = 1050;
        private const int CharacterCount = 1000;
        private const int WarmTicks = 8;
        private const int SampleTicks = 32;
        private const int TotalTicks = WarmTicks + SampleTicks;
        private const BindingFlags StaticNonPublic =
            BindingFlags.Static | BindingFlags.NonPublic;
        private const BindingFlags InstancePublic =
            BindingFlags.Instance | BindingFlags.Public;
        private const string CostArtifactRelativePath =
            "artifacts/diagnostics/NTSD-H07-AI-DECISION-WORK-REVIEW-20261010/owned-input-evaluation/local-cost.json";

        private static readonly RegisterCharacterDelegate registerCharacter =
            CreateRegisterCharacterDelegate();
        private static readonly RuntimeCaptureDelegate captureRuntimeInput =
            CreateRuntimeCaptureDelegate();
        private static CanonicalFieldBinding[] canonicalBindings;

        private delegate LF2Character RegisterCharacterDelegate(
            SimulationWorld world,
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            bool aiControlled,
            int hitJ,
            int? secondHitJ);

        private delegate AiDecisionInputState RuntimeCaptureDelegate(
            NTSDEntityRuntime runtime);

        [Test]
        public void CanonicalOwnedInput_FocusedNativeRoute_FieldsRngAndChecksumRemainEqual()
        {
            SimulationWorld snapshotWorld = null;
            SimulationWorld directWorld = null;
            try
            {
                snapshotWorld = new SimulationWorld(
                    BattleRuntimeProfile.MobileExtended,
                    MobileCapacity);
                directWorld = new SimulationWorld(
                    BattleRuntimeProfile.MobileExtended,
                    MobileCapacity);
                ConfigureWorld(snapshotWorld, AiDecisionOwnedInputMode.SnapshotCopy);
                ConfigureWorld(directWorld, AiDecisionOwnedInputMode.CanonicalStoreDirect);

                Fixture snapshotFixture = RegisterFixture(snapshotWorld, 120, 4);
                Fixture directFixture = RegisterFixture(directWorld, 120, 4);
                SeedFocusedInput(snapshotWorld, snapshotFixture);
                SeedFocusedInput(directWorld, directFixture);
                AssertFixtureRegistration(snapshotWorld, snapshotFixture, 4);
                AssertFixtureRegistration(directWorld, directFixture, 4);
                AssertFixtureIdentities(
                    snapshotFixture.Identities,
                    directFixture.Identities,
                    "focused initial identity");

                for (int tick = 1; tick <= 6; tick++)
                {
                    snapshotWorld.Runtime.Flow.InputPhase = tick % 3;
                    directWorld.Runtime.Flow.InputPhase = tick % 3;
                    RouteCounters snapshotBefore = CaptureRouteCounters(snapshotWorld);
                    RouteCounters directBefore = CaptureRouteCounters(directWorld);
                    snapshotWorld.NativeProducerSampleAndInputRouteAll(tick);
                    directWorld.NativeProducerSampleAndInputRouteAll(tick);
                    AssertNativeProducerRoute(
                        snapshotWorld,
                        snapshotBefore,
                        CaptureRouteCounters(snapshotWorld),
                        4,
                        AiDecisionOwnedInputMode.SnapshotCopy,
                        "focused SnapshotCopy");
                    AssertNativeProducerRoute(
                        directWorld,
                        directBefore,
                        CaptureRouteCounters(directWorld),
                        4,
                        AiDecisionOwnedInputMode.CanonicalStoreDirect,
                        "focused CanonicalStoreDirect");

                    TickSnapshot snapshot = CaptureTickSnapshot(
                        snapshotWorld,
                        snapshotFixture,
                        tick);
                    TickSnapshot direct = CaptureTickSnapshot(
                        directWorld,
                        directFixture,
                        tick);
                    AssertTickSnapshotsEqual(snapshot, direct, tick, "focused mode parity");
                }
            }
            finally
            {
                CleanupWorlds(snapshotWorld, directWorld);
            }
        }

        [Test]
        public void CanonicalOwnedInput_ColdModeBoundaryAndResetAreExplicit()
        {
            var world = new SimulationWorld(BattleRuntimeProfile.MobileExtended, MobileCapacity);
            try
            {
                Assert.That(world.AiDecisionOwnedInputModeForDiagnostics, Is.EqualTo(AiDecisionOwnedInputMode.SnapshotCopy));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    world.ConfigureAiDecisionOwnedInputModeForDiagnostics((AiDecisionOwnedInputMode)99));
                ConfigureWorld(world, AiDecisionOwnedInputMode.CanonicalStoreDirect);
                world.ResetRuntimeState();
                Assert.That(world.AiDecisionOwnedInputModeForDiagnostics, Is.EqualTo(AiDecisionOwnedInputMode.CanonicalStoreDirect));
                world.ConfigureAiDecisionOwnedInputModeForDiagnostics(AiDecisionOwnedInputMode.SnapshotCopy);
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                Assert.That(world.AiDecisionOwnedInputModeForDiagnostics, Is.EqualTo(AiDecisionOwnedInputMode.SnapshotCopy));
                registerCharacter(world, 0, 1, 1, 0, 0, 0, 0, true, 0, null);
                Assert.Throws<InvalidOperationException>(() =>
                    world.ConfigureAiDecisionOwnedInputModeForDiagnostics(AiDecisionOwnedInputMode.CanonicalStoreDirect));
            }
            finally
            {
                CleanupWorlds(world);
            }
        }

        private static void SeedFocusedInput(SimulationWorld world, Fixture fixture)
        {
            FieldInfo[] fields = typeof(AiDecisionInputState).GetFields(InstancePublic);
            for (int slot = 0; slot < fixture.Characters.Length; slot++)
            {
                var runtime = fixture.Characters[slot].Runtime;
                object boxed = captureRuntimeInput(runtime);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].FieldType == typeof(byte))
                        fields[i].SetValue(boxed, (byte)((i + slot) & 1));
                    else if (fields[i].Name.StartsWith("History", StringComparison.Ordinal))
                        fields[i].SetValue(boxed, slot & 1);
                }
                world.CharacterInputWriter.CommitAiDecisionState(runtime, (AiDecisionInputState)boxed);
            }
        }

        [Test]
        public void CanonicalOwnedInput_FocusedOwnerReleaseRebindKeepsStoreOwnership()
        {
            SimulationWorld world = null;
            try
            {
                world = new SimulationWorld(
                    BattleRuntimeProfile.MobileExtended,
                    MobileCapacity);
                ConfigureWorld(world, AiDecisionOwnedInputMode.CanonicalStoreDirect);

                LF2Character first = registerCharacter(
                    world, 7, 701, 1, 840, 0, 10, 0, true, 0, null);
                NTSDEntityRuntime oldRuntime = first.Runtime;
                Assert.That(
                    world.CharacterInputWriter.CanEvaluateCanonicalDecision(oldRuntime),
                    Is.True);
                Assert.That(
                    world.TryGetCurrentRuntimeHandleForDiagnostics(
                        7,
                        first,
                        out RuntimeEntityHandle oldHandle),
                    Is.True);

                world.Unregister(first);
                Assert.That(world.ObjectCount, Is.Zero);
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero);
                Assert.That(
                    world.CharacterInputWriter.CanEvaluateCanonicalDecision(oldRuntime),
                    Is.False);

                LF2Character replacement = registerCharacter(
                    world, 7, 702, 2, 960, 0, 10, 0, true, 0, null);
                Assert.That(
                    world.CharacterInputWriter.CanEvaluateCanonicalDecision(oldRuntime),
                    Is.False);
                Assert.That(
                    world.CharacterInputWriter.CanEvaluateCanonicalDecision(replacement.Runtime),
                    Is.True);
                Assert.That(
                    world.TryGetCurrentRuntimeHandleForDiagnostics(
                        7,
                        replacement,
                        out RuntimeEntityHandle replacementHandle),
                    Is.True);
                Assert.That(
                    replacementHandle.Generation,
                    Is.Not.EqualTo(oldHandle.Generation));

                world.CharacterInputWriter.Release(oldHandle);
                Assert.That(
                    world.CharacterInputWriter.CanEvaluateCanonicalDecision(replacement.Runtime),
                    Is.True,
                    "a stale released handle must not release the replacement owner");

                RouteCounters before = CaptureRouteCounters(world);
                world.NativeProducerSampleAndInputRouteAll(1);
                AssertNativeProducerRoute(
                    world,
                    before,
                    CaptureRouteCounters(world),
                    1,
                    AiDecisionOwnedInputMode.CanonicalStoreDirect,
                    "focused release/rebind");
                TickSnapshot snapshot = CaptureTickSnapshot(
                    world,
                    new Fixture(world, new[] { replacement }),
                    1);
                Assert.That(snapshot.canonicalValues.Length, Is.EqualTo(CanonicalFieldCount));
                Assert.That(snapshot.runtimeValues.Length, Is.EqualTo(CanonicalFieldCount));
            }
            finally
            {
                CleanupWorlds(world);
            }
        }

        [Test]
        [Explicit("Finite comparison; fresh output required; never run in broad regression.")]
        public void CanonicalOwnedInput_RunFiniteNativeRouteCost()
        {
            string artifactPath = GetCostArtifactPath();
            if (File.Exists(artifactPath))
            {
                Assert.Fail(
                    "Refusing to overwrite existing H07 cost artifact: " + artifactPath);
            }

            var report = new CostReport
            {
                status = "INCONCLUSIVE",
                conclusion = "INCONCLUSIVE",
                characterCount = CharacterCount,
                capacity = MobileCapacity,
                warmTicks = WarmTicks,
                sampleTicks = SampleTicks,
                totalTicks = TotalTicks,
                stopwatchFrequency = Stopwatch.Frequency,
                scope =
                    "Exact production entry SimulationWorld.NativeProducerSampleAndInputRouteAll(tick): includeNonCharacterHitFa=true, applyFrameMotionTail=false; two-pass native producer/proxy/route over 1000 pure character fixture.",
                limitations =
                    "The fixture is synthetic and pure-character, so it does not exercise non-character hit-fa occupancy or Sakura scene data. It records the exact native entry but does not claim full production-scene equivalence. Local comparison only; any gain requires independent production-scene validation. Observer is outside measured pass; ABBA is not causal proof."
            };
            string failure = null;
            try
            {
                AiDecisionOwnedInputMode[] sequence =
                {
                    AiDecisionOwnedInputMode.SnapshotCopy,
                    AiDecisionOwnedInputMode.CanonicalStoreDirect,
                    AiDecisionOwnedInputMode.CanonicalStoreDirect,
                    AiDecisionOwnedInputMode.SnapshotCopy,
                };
                int[] spacings = { 120, 12 };
                bool sawSnapshotCopy = false;
                bool sawCanonicalStoreDirect = false;

                for (int layoutIndex = 0; layoutIndex < spacings.Length; layoutIndex++)
                {
                    var layout = new CostLayoutReport
                    {
                        spacing = spacings[layoutIndex],
                    };
                    report.layouts.Add(layout);
                    TickSnapshot[] baseline = null;
                    FixtureIdentity[] baselineIdentities = null;

                    for (int sequenceIndex = 0; sequenceIndex < sequence.Length; sequenceIndex++)
                    {
                        AiDecisionOwnedInputMode requestedMode = sequence[sequenceIndex];
                        sawSnapshotCopy |= requestedMode == AiDecisionOwnedInputMode.SnapshotCopy;
                        sawCanonicalStoreDirect |= requestedMode == AiDecisionOwnedInputMode.CanonicalStoreDirect;
                        var window = new CostWindowReport
                        {
                            window = layout.windows.Count + 1,
                            spacing = spacings[layoutIndex],
                            requestedMode = requestedMode.ToString(),
                            rawTicks = new long[SampleTicks],
                            allocatedBytes = new long[SampleTicks],
                        };
                        layout.windows.Add(window);

                        FixtureIdentity[] actualIdentities;
                        TickSnapshot[] current = ExecuteCostWindow(
                            window,
                            spacings[layoutIndex],
                            requestedMode,
                            baselineIdentities,
                            baseline,
                            out actualIdentities);
                        if (baseline == null)
                        {
                            baseline = current;
                            baselineIdentities = actualIdentities;
                        }
                        else
                        {
                            AssertFixtureIdentities(
                                baselineIdentities,
                                actualIdentities,
                                "cost initial identity");
                        }
                    }

                    Assert.That(sawSnapshotCopy, Is.True);
                    Assert.That(sawCanonicalStoreDirect, Is.True);
                }

                report.valid = true;
                bool signal = true;
                foreach (CostLayoutReport layout in report.layouts)
                {
                    var w = layout.windows;
                    double offMin = Math.Min(w[0].meanMs, w[3].meanMs);
                    layout.gainSignal = w[1].meanMs < offMin && w[2].meanMs < offMin &&
                        Math.Max(w[1].p95Ms, w[2].p95Ms) <= Math.Max(w[0].p95Ms, w[3].p95Ms) &&
                        Math.Max(w[1].p99Ms, w[2].p99Ms) <= Math.Max(w[0].p99Ms, w[3].p99Ms);
                    signal &= layout.gainSignal;
                }
                report.status = "COMPLETED";
                report.conclusion = signal ? "LOCAL_GAIN_SIGNAL" : "NO_DISCERNIBLE_GAIN";
                PersistCostReport(report, artifactPath);
            }
            catch (Exception exception)
            {
                failure = exception.ToString();
                report.valid = false;
                report.status = "ABORTED";
                report.failure = failure;
                try
                {
                    PersistCostReport(report, artifactPath);
                }
                catch (Exception persistException)
                {
                    TestContext.WriteLine(
                        "H07 abort artifact persistence failed: " + persistException);
                }

                throw;
            }
            finally
            {
                if (failure != null)
                    TestContext.WriteLine("H07 local cost stopped: " + failure);
            }
        }

        private static void ConfigureWorld(
            SimulationWorld world,
            AiDecisionOwnedInputMode mode)
        {
            Assert.That(
                world.AiDecisionOwnedInputModeForDiagnostics,
                Is.EqualTo(AiDecisionOwnedInputMode.SnapshotCopy),
                "a cold world must expose its actual initial mode before configuration");
            Assert.That(UnityEngine.Profiling.Profiler.enabled, Is.False);
            world.Runtime.Flow.InputPhase = 2;
            world.Rng.Seed(1314149188u);
            world.NativeRandom.ResetFromSeed(1314149188u);
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.ConfigureAiDecisionOwnedInputModeForDiagnostics(mode);
            Assert.That(world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics, Is.Null);
            Assert.That(world.EnableCharacterInputBoundaryTimingForDiagnostics, Is.False);
            Assert.That(world.NativeOwnedCompatibilitySkipEnabledForDiagnostics, Is.False);
            Assert.That(world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, Is.False);
            Assert.That(
                world.RuntimeProfileForDiagnostics,
                Is.EqualTo(BattleRuntimeProfile.MobileExtended));
            Assert.That(world.RuntimeSlotCapacityForDiagnostics, Is.EqualTo(MobileCapacity));
            Assert.That(
                world.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
            Assert.That(world.AiDecisionOwnedInputModeForDiagnostics, Is.EqualTo(mode));
            Assert.That(
                world.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.UnifiedAuthority));
            Assert.That(world.AiDecisionIndexedCanonicalFullOracleSampleInterval, Is.Zero);
        }

        private static Fixture RegisterFixture(
            SimulationWorld world,
            int spacing,
            int count)
        {
            var characters = new LF2Character[count];
            for (int slot = 0; slot < count; slot++)
            {
                characters[slot] = registerCharacter(
                    world,
                    slot,
                    1,
                    1 + (slot & 1),
                    checked(slot * spacing),
                    0,
                    (slot & 3) * 10,
                    0,
                    true,
                    0,
                    null);
            }

            return new Fixture(world, characters);
        }

        private static void AssertFixtureRegistration(
            SimulationWorld world,
            Fixture fixture,
            int expectedCount)
        {
            Assert.That(fixture.Characters.Length, Is.EqualTo(expectedCount));
            Assert.That(world.ObjectCount, Is.EqualTo(expectedCount));
            Assert.That(
                world.ClaimedRuntimeSlotCountForDiagnostics,
                Is.EqualTo(expectedCount));
            Assert.That(
                world.RuntimeSlotCapacityForDiagnostics,
                Is.EqualTo(MobileCapacity));
            for (int index = 0; index < fixture.Characters.Length; index++)
            {
                LF2Character character = fixture.Characters[index];
                Assert.That(character, Is.Not.Null);
                Assert.That(character.Runtime, Is.Not.Null);
                Assert.That(character.Runtime.SlotIndex, Is.EqualTo(index));
                Assert.That(character.AiControlled, Is.True);
            }
        }

        private static TickSnapshot[] ExecuteCostWindow(
            CostWindowReport report,
            int spacing,
            AiDecisionOwnedInputMode requestedMode,
            FixtureIdentity[] expectedIdentities,
            TickSnapshot[] baseline,
            out FixtureIdentity[] actualIdentities)
        {
            SimulationWorld world = null;
            Fixture fixture = null;
            TickSnapshot[] snapshots = new TickSnapshot[TotalTicks];
            GetCanonicalBindings();
            for (int i = 0; i < TotalTicks; i++)
                snapshots[i] = new TickSnapshot
                {
                    canonicalValues = new int[CharacterCount * CanonicalFieldCount],
                    runtimeValues = new int[CharacterCount * CanonicalFieldCount]
                };
            actualIdentities = null;
            Exception failure = null;
            try
            {
                world = new SimulationWorld(
                    BattleRuntimeProfile.MobileExtended,
                    MobileCapacity);
                ConfigureWorld(world, requestedMode);
                fixture = RegisterFixture(world, spacing, CharacterCount);
                AssertFixtureRegistration(world, fixture, CharacterCount);
                actualIdentities = fixture.Identities;
                report.capacity = world.RuntimeSlotCapacityForDiagnostics;
                report.registered = world.ObjectCount;
                report.actualMode = world.AiDecisionOwnedInputModeForDiagnostics.ToString();
                report.profile = world.RuntimeProfileForDiagnostics.ToString();
                report.aiProfile = world.AiExecutionProfile.ToString();
                report.unifiedMode = world.AiUnifiedSnapshotExecutionMode.ToString();
                report.inputPassMode = world.BattleEcsCharacterInputPassModeForDiagnostics.ToString();
                Assert.That(
                    world.AiDecisionOwnedInputModeForDiagnostics,
                    Is.EqualTo(requestedMode));
                Assert.That(
                    world.BattleEcsCharacterInputPassModeForDiagnostics,
                    Is.EqualTo(BattleEcsCharacterInputPassMode.DataOriented));
                Assert.That(
                    world.AiUnifiedSnapshotExecutionMode,
                    Is.EqualTo(AiUnifiedSnapshotExecutionMode.UnifiedAuthority));
                Assert.That(world.AiDecisionIndexedCanonicalFullOracleSampleInterval, Is.Zero);
                if (expectedIdentities != null)
                {
                    AssertFixtureIdentities(
                        expectedIdentities,
                        actualIdentities,
                        "cost registration");
                }

                Stopwatch stopwatch = new Stopwatch();
                for (int index = 0; index < TotalTicks; index++)
                {
                    int tick = index + 1;
                    RouteCounters before = CaptureRouteCounters(world);
                    long elapsedTicks = 0;
                    long allocated = 0;
                    if (index < WarmTicks)
                    {
                        world.NativeProducerSampleAndInputRouteAll(tick);
                    }
                    else
                    {
                        stopwatch.Restart();
                        long allocationBefore = GC.GetAllocatedBytesForCurrentThread();
                        world.NativeProducerSampleAndInputRouteAll(tick);
                        allocated = GC.GetAllocatedBytesForCurrentThread() - allocationBefore;
                        stopwatch.Stop();
                        elapsedTicks = stopwatch.ElapsedTicks;
                        report.rawTicks[index - WarmTicks] = elapsedTicks;
                        report.allocatedBytes[index - WarmTicks] = allocated;
                        report.allocatedTotalBytes += allocated;
                        report.completedSamples++;
                        if (allocated != 0) throw new AssertionException("Measured native input pass allocated " + allocated + " bytes; finite group stopped.");
                    }

                    RouteCounters after = CaptureRouteCounters(world);
                    AssertNativeProducerRoute(
                        world,
                        before,
                        after,
                        CharacterCount,
                        requestedMode,
                        "cost window " + report.window);
                    report.eligible += after.indexedEligible - before.indexedEligible;
                    report.commits += after.indexedCommitted - before.indexedCommitted;
                    report.fallback += after.indexedFallback - before.indexedFallback;
                    report.progressCommits += world.LastCharacterInputProgressCommitCountForDiagnostics;
                    report.progressSkips += world.LastCharacterInputProgressCommitSkipCountForDiagnostics;
                    report.oracleSamples = after.oracleSamples;
                    report.unifiedFallback += after.unifiedFallback - before.unifiedFallback;
                    snapshots[index] = CaptureTickSnapshot(world, fixture, tick, snapshots[index]);
                    TickSnapshot captured = snapshots[index];
                    report.ticks[index] = new TickEvidence
                    {
                        tick = tick,
                        checksum = captured.checksum,
                        rngState = captured.rngState,
                        rngCalls = captured.rngCalls,
                        nativeRandom = captured.nativeRandom,
                        canonicalHash = HashValues(captured.canonicalValues),
                        runtimeInputHash = HashValues(captured.runtimeValues)
                    };
                    if (baseline != null)
                    {
                        AssertTickSnapshotsEqual(
                            baseline[index],
                            snapshots[index],
                            tick,
                            "cost window " + report.window + " parity");
                        report.parityComparisons++;
                    }
                }

                report.meanMs = MeanMilliseconds(report.rawTicks);
                report.p95Ms = PercentileMilliseconds(report.rawTicks, 0.95);
                report.p99Ms = PercentileMilliseconds(report.rawTicks, 0.99);
                Assert.That(world.ObjectCount, Is.EqualTo(CharacterCount));
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(CharacterCount));
                Assert.That(world.RuntimeSlotCapacityForDiagnostics, Is.EqualTo(MobileCapacity));
                Assert.That(world.RuntimeSlotCapacityRejectCountForDiagnostics, Is.Zero);
                report.hardGates = true;
                report.parity = baseline == null || report.parityComparisons == TotalTicks;
            }
            catch (Exception exception)
            {
                failure = exception;
                report.hardGates = false;
                report.parity = false;
                report.failure = exception.ToString();
            }

            if (world != null)
            {
                try
                {
                    CleanupInfo cleanup = CleanupWorld(world);
                    report.cleanup = cleanup.Objects == 0 && cleanup.ClaimedSlots == 0;
                    report.cleanupObjects = cleanup.Objects;
                    report.cleanupSlots = cleanup.ClaimedSlots;
                    report.modeAfterReset = cleanup.ModeAfterReset;
                }
                catch (Exception cleanupException)
                {
                    report.cleanup = false;
                    report.failure = AppendFailure(report.failure, cleanupException);
                    if (failure == null)
                        failure = cleanupException;
                }
            }

            if (failure != null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }

            return snapshots;
        }

        private static RouteCounters CaptureRouteCounters(SimulationWorld world)
        {
            BattleEcsCharacterInputPassDiagnostics inputPass =
                world.BattleEcsCharacterInputPassDiagnosticsForDiagnostics;
            return new RouteCounters
            {
                indexedEligible = world.AiDecisionIndexedCanonicalEligibleCountForDiagnostics,
                indexedCommitted = world.AiDecisionIndexedCanonicalCommittedCountForDiagnostics,
                indexedFallback = world.AiDecisionIndexedCanonicalFallbackCountForDiagnostics,
                oracleSamples = world.AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics,
                unifiedFallback = world.AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics,
                unifiedFailure = world.AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics,
                inputPass = inputPass,
            };
        }

        private static void AssertNativeProducerRoute(
            SimulationWorld world,
            RouteCounters before,
            RouteCounters after,
            int expectedCharacterCount,
            AiDecisionOwnedInputMode expectedMode,
            string context)
        {
            Assert.That(
                world.AiDecisionOwnedInputModeForDiagnostics,
                Is.EqualTo(expectedMode),
                context + " actual owned-input mode");
            Assert.That(
                world.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical),
                context + " actual AI profile");
            Assert.That(
                world.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.UnifiedAuthority),
                context + " unified authority");
            Assert.That(
                world.AiDecisionIndexedCanonicalFullOracleSampleInterval,
                Is.Zero,
                context + " oracle interval");
            Assert.That(after.oracleSamples, Is.Zero, context + " oracle samples");
            Assert.That(
                after.indexedEligible - before.indexedEligible,
                Is.EqualTo(expectedCharacterCount),
                context + " indexed eligible");
            Assert.That(
                after.indexedCommitted - before.indexedCommitted,
                Is.EqualTo(expectedCharacterCount),
                context + " indexed commits");
            Assert.That(
                after.indexedFallback - before.indexedFallback,
                Is.Zero,
                context + " indexed fallback");
            Assert.That(
                after.unifiedFallback - before.unifiedFallback,
                Is.Zero,
                context + " unified precommit fallback");
            Assert.That(
                after.unifiedFailure - before.unifiedFailure,
                Is.Zero,
                context + " unified precommit failure");
            Assert.That(
                after.inputPass.Mode,
                Is.EqualTo(BattleEcsCharacterInputPassMode.DataOriented),
                context + " input pass mode");
            Assert.That(
                after.inputPass.RunCount - before.inputPass.RunCount,
                Is.EqualTo(expectedCharacterCount),
                context + " per-character input pass calls");
            Assert.That(
                after.inputPass.ExactCharacterCount - before.inputPass.ExactCharacterCount,
                Is.EqualTo(expectedCharacterCount),
                context + " exact character count");
            Assert.That(
                after.inputPass.CompatibilityFallbackCount - before.inputPass.CompatibilityFallbackCount,
                Is.Zero,
                context + " compatibility fallback");
            Assert.That(
                after.inputPass.UnityCompatibilityShellCount - before.inputPass.UnityCompatibilityShellCount,
                Is.Zero,
                context + " Unity compatibility shell");
            Assert.That(
                after.inputPass.UnexpectedFallbackCount - before.inputPass.UnexpectedFallbackCount,
                Is.Zero,
                context + " unexpected fallback");
        }

        private static TickSnapshot CaptureTickSnapshot(
            SimulationWorld world, Fixture fixture, int tick, TickSnapshot destination = null)
        {
            TickSnapshot row = destination ?? new TickSnapshot
            {
                canonicalValues = new int[fixture.Characters.Length * CanonicalFieldCount],
                runtimeValues = new int[fixture.Characters.Length * CanonicalFieldCount]
            };
            row.checksum = world.CaptureRuntimeChecksum64(tick, null);
            row.rngState = world.Rng.State;
            row.rngCalls = world.Rng.CallCount;
            row.nativeRandom = world.NativeRandom.CaptureScalarState();
            for (int i = 0; i < fixture.Characters.Length; i++)
            {
                var runtime = fixture.Characters[i].Runtime;
                if (!world.CharacterInputWriter.TryCaptureCanonicalState(runtime, out AiDecisionInputState canonical))
                    throw new AssertionException("Missing canonical owner at slot " + i);
                WriteInputValues(in canonical, row.canonicalValues, i * CanonicalFieldCount);
                AiDecisionInputState mirror = captureRuntimeInput(runtime);
                WriteInputValues(in mirror, row.runtimeValues, i * CanonicalFieldCount);
            }
            return row;
        }

        private static void WriteInputValues(in AiDecisionInputState input, int[] values, int offset)
        {
            values[offset++] = input.History0;
            values[offset++] = input.History1;
            values[offset++] = input.History2;
            values[offset++] = input.History3;
            values[offset++] = input.History4;
            values[offset++] = input.History5;
            values[offset++] = input.CdAttack;
            values[offset++] = input.CdJump;
            values[offset++] = input.CdDefend;
            values[offset++] = input.CdDefendLock;
            values[offset++] = input.CdRight;
            values[offset++] = input.CdLeft;
            values[offset++] = input.CdUp;
            values[offset++] = input.CdDown;
            values[offset++] = input.ComboDra;
            values[offset++] = input.ComboDla;
            values[offset++] = input.ComboDua;
            values[offset++] = input.ComboDda;
            values[offset++] = input.ComboDrj;
            values[offset++] = input.ComboDlj;
            values[offset++] = input.ComboDuj;
            values[offset++] = input.ComboDdj;
            values[offset++] = input.ComboDja;
            values[offset++] = input.PrevUp;
            values[offset++] = input.PrevDown;
            values[offset++] = input.PrevLeft;
            values[offset++] = input.PrevRight;
            values[offset++] = input.PrevJump;
            values[offset++] = input.PrevDefend;
            values[offset++] = input.PrevAttack;
            values[offset++] = input.KeyUp;
            values[offset++] = input.KeyDown;
            values[offset++] = input.KeyLeft;
            values[offset++] = input.KeyRight;
            values[offset++] = input.KeyAttack;
            values[offset++] = input.KeyJump;
            values[offset++] = input.KeyDefend;
            values[offset++] = input.Unk360;
            values[offset++] = input.Unk3FC;
            values[offset++] = input.Unk400;
            values[offset++] = input.BoundaryFlags;
        }

        private static void AssertTickSnapshotsEqual(
            TickSnapshot expected,
            TickSnapshot actual,
            int tick,
            string context)
        {
            Assert.That(actual.checksum, Is.EqualTo(expected.checksum), context + " checksum tick " + tick);
            Assert.That(actual.rngState, Is.EqualTo(expected.rngState), context + " legacy RNG state tick " + tick);
            Assert.That(actual.rngCalls, Is.EqualTo(expected.rngCalls), context + " legacy RNG calls tick " + tick);
            AssertNativeRandomEqual(expected.nativeRandom, actual.nativeRandom, context + " native RNG tick " + tick);
            AssertVectorEqual(expected.canonicalValues, actual.canonicalValues, "canonical", context, tick);
            AssertVectorEqual(expected.runtimeValues, actual.runtimeValues, "runtime", context, tick);
        }

        private static ulong HashValues(int[] values)
        {
            ulong hash = 14695981039346656037UL;
            unchecked
            {
                for (int i = 0; i < values.Length; i++)
                    hash = (hash ^ (uint)values[i]) * 1099511628211UL;
            }
            return hash;
        }

        private static void AssertVectorEqual(
            int[] expected,
            int[] actual,
            string vectorName,
            string context,
            int tick)
        {
            Assert.That(actual, Is.Not.Null, context + " " + vectorName + " vector");
            Assert.That(actual.Length, Is.EqualTo(expected.Length), context + " " + vectorName + " length");
            CanonicalFieldBinding[] bindings = GetCanonicalBindings();
            for (int index = 0; index < expected.Length; index++)
            {
                if (actual[index] == expected[index])
                    continue;
                int slot = index / bindings.Length;
                int field = index % bindings.Length;
                throw new AssertionException(
                    context + " " + vectorName + " mismatch tick " + tick +
                    ", slot " + slot + ", field " + bindings[field].Field.Name +
                    ": expected " + expected[index] + ", actual " + actual[index]);
            }
        }

        private static void AssertNativeRandomEqual(
            NTSD28NativeRandomScalarState expected,
            NTSD28NativeRandomScalarState actual,
            string context)
        {
            Assert.That(actual.CrtState, Is.EqualTo(expected.CrtState), context + " CRT state");
            Assert.That(actual.CrtCalls, Is.EqualTo(expected.CrtCalls), context + " CRT calls");
            Assert.That(actual.TableSeed, Is.EqualTo(expected.TableSeed), context + " table seed");
            Assert.That(actual.SynchronizedCounter, Is.EqualTo(expected.SynchronizedCounter), context + " synchronized counter");
            Assert.That(actual.SynchronizedIndex, Is.EqualTo(expected.SynchronizedIndex), context + " synchronized index");
            Assert.That(actual.SynchronizedCalls, Is.EqualTo(expected.SynchronizedCalls), context + " synchronized calls");
            Assert.That(actual.LastSynchronizedCallSite, Is.EqualTo(expected.LastSynchronizedCallSite), context + " last call site");
            Assert.That(actual.SynchronizedTableHash, Is.EqualTo(expected.SynchronizedTableHash), context + " table hash");
            Assert.That(actual.SynchronizedGeneration, Is.EqualTo(expected.SynchronizedGeneration), context + " generation");
        }

        private static void AssertFixtureIdentities(
            FixtureIdentity[] expected,
            FixtureIdentity[] actual,
            string context)
        {
            Assert.That(actual, Is.Not.Null, context + " actual identities");
            Assert.That(actual.Length, Is.EqualTo(expected.Length), context + " identity count");
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(actual[index].Slot, Is.EqualTo(expected[index].Slot), context + " slot " + index);
                Assert.That(actual[index].StableId, Is.EqualTo(expected[index].StableId), context + " stable id " + index);
                Assert.That(actual[index].ObjectId, Is.EqualTo(expected[index].ObjectId), context + " object id " + index);
                Assert.That(actual[index].Name, Is.EqualTo(expected[index].Name), context + " name " + index);
            }
        }

        private static CanonicalFieldBinding[] GetCanonicalBindings()
        {
            if (canonicalBindings != null)
                return canonicalBindings;

            FieldInfo[] fields = typeof(AiDecisionInputState).GetFields(InstancePublic);
            Array.Sort(fields, (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));
            if (fields.Length != CanonicalFieldCount)
            {
                throw new AssertionException(
                    "AiDecisionInputState field count changed: expected " +
                    CanonicalFieldCount + ", actual " + fields.Length);
            }

            var bindings = new CanonicalFieldBinding[fields.Length];
            for (int index = 0; index < fields.Length; index++)
            {
                FieldInfo field = fields[index];
                bindings[index] = new CanonicalFieldBinding(field, null, -1);
            }

            canonicalBindings = bindings;
            return canonicalBindings;
        }

        private static RegisterCharacterDelegate CreateRegisterCharacterDelegate()
        {
            MethodInfo method = typeof(AiDecisionSoAShadowEditorTests).GetMethod(
                "RegisterCharacter",
                StaticNonPublic);
            if (method == null)
                throw new MissingMethodException(
                    typeof(AiDecisionSoAShadowEditorTests).FullName,
                    "RegisterCharacter");
            return (RegisterCharacterDelegate)method.CreateDelegate(
                typeof(RegisterCharacterDelegate));
        }

        private static RuntimeCaptureDelegate CreateRuntimeCaptureDelegate()
        {
            MethodInfo method = typeof(BattleCharacterInputStore).GetMethod(
                "Capture",
                StaticNonPublic);
            if (method == null)
                throw new MissingMethodException(
                    typeof(BattleCharacterInputStore).FullName,
                    "Capture");
            return (RuntimeCaptureDelegate)method.CreateDelegate(
                typeof(RuntimeCaptureDelegate));
        }

        private static string GetCostArtifactPath()
        {
            return Path.GetFullPath(CostArtifactRelativePath);
        }

        private static void PersistCostReport(CostReport report, string artifactPath)
        {
            string directory = Path.GetDirectoryName(artifactPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            using (FileStream stream = new FileStream(
                       artifactPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(JsonConvert.SerializeObject(report, Formatting.Indented));
            }
        }

        private static CleanupInfo CleanupWorld(SimulationWorld world)
        {
            world.ResetRuntimeState();
            int objects = world.ObjectCount;
            int claimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            if (objects != 0 || claimedSlots != 0)
            {
                throw new AssertionException(
                    "ResetRuntimeState cleanup failed: objects=" + objects +
                    ", claimedSlots=" + claimedSlots);
            }

            return new CleanupInfo(
                objects,
                claimedSlots,
                world.AiDecisionOwnedInputModeForDiagnostics.ToString());
        }

        private static void CleanupWorlds(params SimulationWorld[] worlds)
        {
            Exception failure = null;
            for (int index = 0; index < worlds.Length; index++)
            {
                if (worlds[index] == null)
                    continue;
                try
                {
                    CleanupWorld(worlds[index]);
                }
                catch (Exception exception)
                {
                    if (failure == null)
                        failure = exception;
                }
            }

            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
        }

        private static string AppendFailure(string existing, Exception addition)
        {
            return string.IsNullOrEmpty(existing)
                ? addition.ToString()
                : existing + Environment.NewLine + addition;
        }

        private static double MeanMilliseconds(long[] rawTicks)
        {
            double total = 0.0;
            for (int index = 0; index < rawTicks.Length; index++)
                total += rawTicks[index] * 1000.0 / Stopwatch.Frequency;
            return total / rawTicks.Length;
        }

        private static double PercentileMilliseconds(long[] rawTicks, double percentile)
        {
            var milliseconds = new double[rawTicks.Length];
            for (int index = 0; index < rawTicks.Length; index++)
                milliseconds[index] = rawTicks[index] * 1000.0 / Stopwatch.Frequency;
            Array.Sort(milliseconds);
            double position = (milliseconds.Length - 1) * percentile;
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);
            if (lower == upper)
                return milliseconds[lower];
            double fraction = position - lower;
            return milliseconds[lower] +
                   (milliseconds[upper] - milliseconds[lower]) * fraction;
        }

        private sealed class Fixture
        {
            internal Fixture(SimulationWorld world, LF2Character[] characters)
            {
                World = world;
                Characters = characters;
                Identities = new FixtureIdentity[characters.Length];
                for (int index = 0; index < characters.Length; index++)
                {
                    LF2Character character = characters[index];
                    Identities[index] = new FixtureIdentity(
                        character.Runtime.SlotIndex,
                        character.Runtime.StableId,
                        character.Runtime.ObjectId,
                        character.Name);
                }
            }

            internal SimulationWorld World { get; }
            internal LF2Character[] Characters { get; }
            internal FixtureIdentity[] Identities { get; }
        }

        private readonly struct FixtureIdentity
        {
            internal FixtureIdentity(int slot, int stableId, int objectId, string name)
            {
                Slot = slot;
                StableId = stableId;
                ObjectId = objectId;
                Name = name;
            }

            internal int Slot { get; }
            internal int StableId { get; }
            internal int ObjectId { get; }
            internal string Name { get; }
        }

        private sealed class CanonicalFieldBinding
        {
            internal CanonicalFieldBinding(
                FieldInfo field,
                FieldInfo runtimeField,
                int historyIndex)
            {
                Field = field;
                RuntimeField = runtimeField;
                HistoryIndex = historyIndex;
            }

            internal FieldInfo Field { get; }
            internal FieldInfo RuntimeField { get; }
            internal int HistoryIndex { get; }
        }

        private sealed class RouteCounters
        {
            internal long indexedEligible;
            internal long indexedCommitted;
            internal long indexedFallback;
            internal long oracleSamples;
            internal long unifiedFallback;
            internal long unifiedFailure;
            internal BattleEcsCharacterInputPassDiagnostics inputPass;
        }

        private sealed class TickSnapshot
        {
            internal ulong checksum;
            internal int[] canonicalValues;
            internal int[] runtimeValues;
            internal uint rngState;
            internal ulong rngCalls;
            internal NTSD28NativeRandomScalarState nativeRandom;
        }

        private readonly struct CleanupInfo
        {
            internal CleanupInfo(int objects, int claimedSlots, string modeAfterReset)
            {
                Objects = objects;
                ClaimedSlots = claimedSlots;
                ModeAfterReset = modeAfterReset;
            }

            internal int Objects { get; }
            internal int ClaimedSlots { get; }
            internal string ModeAfterReset { get; }
        }

        private sealed class CostReport
        {
            public string status;
            public string conclusion;
            public bool valid;
            public int characterCount;
            public int capacity;
            public int warmTicks;
            public int sampleTicks;
            public int totalTicks;
            public long stopwatchFrequency;
            public string scope;
            public string limitations;
            public string failure;
            public List<CostLayoutReport> layouts = new List<CostLayoutReport>();
        }

        private sealed class CostLayoutReport
        {
            public int spacing;
            public bool gainSignal;
            public List<CostWindowReport> windows = new List<CostWindowReport>();
        }

        private sealed class CostWindowReport
        {
            public int window;
            public int spacing;
            public string requestedMode;
            public string actualMode;
            public string profile;
            public string aiProfile;
            public string unifiedMode;
            public TickEvidence[] ticks = new TickEvidence[TotalTicks];
            public string inputPassMode;
            public int rawTickCount;
            public int completedSamples;
            public long[] rawTicks;
            public long[] allocatedBytes;
            public long allocatedTotalBytes;
            public double meanMs;
            public double p95Ms;
            public double p99Ms;
            public long eligible;
            public long commits;
            public long fallback;
            public long unifiedFallback;
            public long progressCommits;
            public long progressSkips;
            public long oracleSamples;
            public int capacity;
            public int registered;
            public int parityComparisons;
            public bool parity;
            public bool hardGates;
            public bool cleanup;
            public int cleanupObjects;
            public int cleanupSlots;
            public string modeAfterReset;
            public string failure;

            public CostWindowReport()
            {
                rawTickCount = SampleTicks;
            }
        }

        private sealed class TickEvidence
        {
            public int tick;
            public ulong checksum, rngCalls, canonicalHash, runtimeInputHash;
            public uint rngState;
            public NTSD28NativeRandomScalarState nativeRandom;
        }
    }
}
#endif
