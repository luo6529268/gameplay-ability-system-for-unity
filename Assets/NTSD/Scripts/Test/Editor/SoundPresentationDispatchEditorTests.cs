#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class SoundPresentationDispatchEditorTests
    {
        [Test]
        public void QueueSound_RecordsOrderedValueEventsWithoutSteadyStateAllocation()
        {
            var world = new SimulationWorld();
            world.AdvanceBattleFlowTick(17);
            world.PendingSounds.Capacity = 128;
            world.QueueSound("SFX_WARM", 0);
            world.PendingSounds.Clear();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 128; i++)
                world.QueueSound("SFX_STEADY", i);
            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

            Assert.That(allocatedBytes, Is.Zero);
            Assert.That(world.PendingSounds, Has.Count.EqualTo(128));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_STEADY"));
            Assert.That(world.PendingSounds[0].Tick, Is.EqualTo(17));
            Assert.That(world.PendingSounds[0].WorldX, Is.Zero);
            Assert.That(world.PendingSounds[127].WorldX, Is.EqualTo(127));
            Assert.That(world.QueuedSoundEventCountForDiagnostics, Is.EqualTo(129));
        }

        [Test]
        public void ParitySnapshot_PreservesSoundFieldsAndOrder()
        {
            var world = new SimulationWorld();
            world.AdvanceBattleFlowTick(23);
            world.QueueSound("SFX_FIRST", 120);
            world.QueueSound("SFX_SECOND", -7);

            BattleParityFrameSnapshot snapshot = world.CaptureParityFrameSnapshot(23);
            string json = snapshot.ToJson();

            Assert.That(json, Does.Contain(
                "\"pendingSounds\":[{\"cue\":\"sfx_first\",\"tick\":23,\"worldX\":120}," +
                "{\"cue\":\"sfx_second\",\"tick\":23,\"worldX\":-7}]"));
            Assert.That(world.PendingSounds[0].Cue, Is.EqualTo("SFX_FIRST"));
            Assert.That(world.PendingSounds[1].Cue, Is.EqualTo("SFX_SECOND"));
        }

        [Test]
        public void Driver_CatchUpDispatchesEveryTickAfterChecksumWithoutDropOrDuplication()
        {
            using var scope = new DriverScope();
            var sink = new RecordingSoundSink(scope.Driver);
            scope.Driver.SetSoundPresentationSinkForDiagnostics(sink);
            scope.Driver.World.Register(new TickSoundEmitter(scope.Driver.World));

            const int catchUpTickCount = 3;
            var presentationFlags = new bool[catchUpTickCount];
            for (int tick = 1; tick <= catchUpTickCount; tick++)
            {
                float remainingAccumulator =
                    SimulationConstants.SIM_DT * (catchUpTickCount - tick);
                bool buildPresentation =
                    SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                        SimulationDriveMode.LocalFreeRun,
                        requireInputFrameReady: false,
                        remainingAccumulator,
                        ticksAlreadyExecuted: tick - 1,
                        maxCatchUpTicks: 8);
                presentationFlags[tick - 1] = buildPresentation;
                Assert.That(scope.Driver.StepOneTick(
                    FrameInputSet.Empty(tick),
                    ignorePaused: true,
                    buildPresentation: buildPresentation), Is.True);
            }

            Assert.That(presentationFlags, Is.EqualTo(new[] { false, false, true }));
            Assert.That(sink.Batches, Has.Count.EqualTo(catchUpTickCount));
            Assert.That(sink.Batches[0], Has.Length.EqualTo(1));
            Assert.That(sink.Batches[0][0].Cue, Is.EqualTo("SFX_TICK_1"));
            Assert.That(sink.Batches[1][0].Cue, Is.EqualTo("SFX_TICK_2"));
            Assert.That(sink.Batches[2][0].Cue, Is.EqualTo("SFX_TICK_3"));
            Assert.That(sink.ChecksumWasReady, Is.EqualTo(new[] { true, true, true }));
            Assert.That(scope.Driver.DispatchedSoundEventCountForDiagnostics, Is.EqualTo(3));
            Assert.That(scope.Driver.SuppressedSoundEventCountForDiagnostics, Is.Zero);
            Assert.That(scope.Driver.World.QueuedSoundEventCountForDiagnostics, Is.EqualTo(3));
        }

        [Test]
        public void Driver_SuppressionKeepsLogicalEventAndChecksumWithoutCallingSink()
        {
            using var scope = new DriverScope();
            var sink = new RecordingSoundSink(scope.Driver);
            scope.Driver.SetSoundPresentationSinkForDiagnostics(sink);
            scope.Driver.SetSoundPresentationSuppressedForDiagnostics(true);
            scope.Driver.World.Register(new TickSoundEmitter(scope.Driver.World));

            Assert.That(scope.Driver.StepOneTick(
                FrameInputSet.Empty(1),
                ignorePaused: true,
                buildPresentation: false), Is.True);

            Assert.That(sink.Batches, Is.Empty);
            Assert.That(scope.Driver.World.PendingSounds, Has.Count.EqualTo(1));
            Assert.That(scope.Driver.World.PendingSounds[0].Cue, Is.EqualTo("SFX_TICK_1"));
            Assert.That(scope.Driver.LastChecksumSnapshot, Is.Not.Null);
            Assert.That(scope.Driver.LastChecksumSnapshot.ToJson(), Does.Contain("sfx_tick_1"));
            Assert.That(scope.Driver.DispatchedSoundEventCountForDiagnostics, Is.Zero);
            Assert.That(scope.Driver.SuppressedSoundEventCountForDiagnostics, Is.EqualTo(1));
            Assert.That(scope.Driver.World.QueuedSoundEventCountForDiagnostics, Is.EqualTo(1));
        }

        [Test]
        public void Driver_DispatchAndSuppressionProduceIdenticalLogicalSnapshot()
        {
            string dispatchedSnapshot;
            using (var dispatched = new DriverScope())
            {
                dispatched.Driver.SetSoundPresentationSinkForDiagnostics(
                    new RecordingSoundSink(dispatched.Driver));
                dispatched.Driver.World.Register(new TickSoundEmitter(dispatched.Driver.World));
                Assert.That(dispatched.Driver.StepOneTick(
                    FrameInputSet.Empty(1),
                    ignorePaused: true,
                    buildPresentation: false), Is.True);
                dispatchedSnapshot = dispatched.Driver.LastChecksumSnapshot.ToJson();
            }

            using var suppressed = new DriverScope();
            suppressed.Driver.SetSoundPresentationSinkForDiagnostics(
                new RecordingSoundSink(suppressed.Driver));
            suppressed.Driver.SetSoundPresentationSuppressedForDiagnostics(true);
            suppressed.Driver.World.Register(new TickSoundEmitter(suppressed.Driver.World));
            Assert.That(suppressed.Driver.StepOneTick(
                FrameInputSet.Empty(1),
                ignorePaused: true,
                buildPresentation: false), Is.True);

            Assert.That(suppressed.Driver.LastChecksumSnapshot.ToJson(),
                Is.EqualTo(dispatchedSnapshot));
            Assert.That(suppressed.Driver.World.PendingSounds, Has.Count.EqualTo(1));
            Assert.That(suppressed.Driver.World.PendingSounds[0].Cue,
                Is.EqualTo("SFX_TICK_1"));
        }

        [Test]
        public void PreparedSingleFileCue_IsBuiltOnceAndReusesWrapper()
        {
            var host = new GameObject("PreparedSoundCueTests")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            try
            {
                NTSDSoundPlayer player = host.AddComponent<NTSDSoundPlayer>();

                Assert.That(player.TryGetPreparedSingleFileWrapperForDiagnostics(
                    "__TEST_PREPARED__\\single.wav",
                    out AudioClip[] first), Is.True);
                Assert.That(player.TryGetPreparedSingleFileWrapperForDiagnostics(
                    "__TEST_PREPARED__\\single.wav",
                    out AudioClip[] second), Is.True);

                Assert.That(second, Is.SameAs(first));
                Assert.That(first, Has.Length.EqualTo(1));
                Assert.That(player.PreparedCueCountForDiagnostics, Is.EqualTo(1));
                Assert.That(player.PreparedCueBuildCountForDiagnostics, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void PreparedVoicePool_PlaybackSaturationAndUnknownCueRejection_DoNotAllocate()
        {
            const string preparedSoundId = "__TEST_PREPARED__\\pooled.wav";
            const string unknownSoundId = "__TEST_UNKNOWN__\\missing.wav";
            var host = new GameObject("PreparedSoundVoicePoolAllocationTests")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            AudioClip clip = null;
            try
            {
                NTSDSoundPlayer player = host.AddComponent<NTSDSoundPlayer>();
                clip = AudioClip.Create(
                    "PreparedSoundVoicePoolAllocationClip",
                    44100,
                    1,
                    44100,
                    false);
                PrepareLoadedCue(player, preparedSoundId, clip);
                InvokePrivate(player, "EnsureOneShotVoicePool");
                SetPrivateField(player, "cachedListenerTransform", host.transform);

                var sounds = new List<PendingSoundEvent>(1)
                {
                    new PendingSoundEvent(preparedSoundId, 0, 1),
                };

                player.PresentSounds(sounds);
                long playCountBefore = player.PooledOneShotPlayCountForDiagnostics;
                long dropCountBefore = player.OneShotVoiceLimitDropCountForDiagnostics;

                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                    player.PresentSounds(sounds);
                long playbackAllocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(playbackAllocatedBytes, Is.Zero);
                Assert.That(
                    player.PooledOneShotPlayCountForDiagnostics,
                    Is.GreaterThan(playCountBefore));
                Assert.That(
                    player.OneShotVoiceLimitDropCountForDiagnostics,
                    Is.GreaterThan(dropCountBefore));

                SetPrivateField(player, "battleCatalogSealed", true);
                var unknownSounds = new List<PendingSoundEvent>(1)
                {
                    new PendingSoundEvent(unknownSoundId, 0, 2),
                };
                long rejectionCountBefore =
                    player.RejectedUnpreparedCueCountForDiagnostics;

                _ = GC.GetAllocatedBytesForCurrentThread();
                before = GC.GetAllocatedBytesForCurrentThread();
                for (int iteration = 0; iteration < 256; iteration++)
                    player.PresentSounds(unknownSounds);
                long rejectionAllocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(rejectionAllocatedBytes, Is.Zero);
                Assert.That(
                    player.RejectedUnpreparedCueCountForDiagnostics - rejectionCountBefore,
                    Is.EqualTo(256));
            }
            finally
            {
                if (clip != null)
                    UnityEngine.Object.DestroyImmediate(clip);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static void PrepareLoadedCue(
            NTSDSoundPlayer player,
            string soundId,
            AudioClip clip)
        {
            Assert.That(player.TryGetPreparedSingleFileWrapperForDiagnostics(
                soundId,
                out AudioClip[] clips), Is.True);
            clips[0] = clip;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo preparedCuesField = typeof(NTSDSoundPlayer).GetField(
                "preparedCues",
                flags);
            Assert.That(preparedCuesField, Is.Not.Null);
            var preparedCues = preparedCuesField.GetValue(player) as IDictionary;
            Assert.That(preparedCues, Is.Not.Null);
            object preparedCue = preparedCues[soundId];
            Assert.That(preparedCue, Is.Not.Null);

            FieldInfo isLoadedField = preparedCue.GetType().GetField("IsLoaded");
            Assert.That(isLoadedField, Is.Not.Null);
            isLoadedField.SetValue(preparedCue, true);

            FieldInfo audioItemField = preparedCue.GetType().GetField("AudioItem");
            Assert.That(audioItemField, Is.Not.Null);
            var audioItem = audioItemField.GetValue(preparedCue) as AudioItem;
            Assert.That(audioItem, Is.Not.Null);
            audioItem.minTimeBetweenCall = 0f;
            audioItem.lastTimePlayed = -1000f;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo method = target.GetType().GetMethod(methodName, flags);
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, null);
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = target.GetType().GetField(fieldName, flags);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private sealed class TickSoundEmitter : LF2Entity
        {
            private readonly SimulationWorld world;

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public TickSoundEmitter(SimulationWorld world)
            {
                this.world = world;
                Name = "SoundPresentationDispatchEmitter";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public override void SimTransit(int tickIndex)
            {
                world.QueueSound("SFX_TICK_" + tickIndex, tickIndex * 10);
            }

            public override void SimTU(int tickIndex) { }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class RecordingSoundSink : ISimulationSoundPresentationSink
        {
            private readonly SimulationTickDriver driver;

            public RecordingSoundSink(SimulationTickDriver driver)
            {
                this.driver = driver;
            }

            public List<PendingSoundEvent[]> Batches { get; } =
                new List<PendingSoundEvent[]>();
            public List<bool> ChecksumWasReady { get; } = new List<bool>();

            public void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds)
            {
                var copy = new PendingSoundEvent[sounds.Count];
                for (int i = 0; i < sounds.Count; i++)
                    copy[i] = sounds[i];
                Batches.Add(copy);
                ChecksumWasReady.Add(
                    driver.LastChecksumSnapshot != null &&
                    driver.LastChecksumSnapshot.Tick == copy[0].Tick);
            }
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);

                host = new GameObject("SoundPresentationDispatchTests")
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
                    captureFullFrameSnapshotForDiagnostics = true,
                });
                Driver.SetPaused(true);
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                Driver.World?.ResetRuntimeState();
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
#endif
