#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattlePresentationCapacitySealEditorTests
    {
        private const BindingFlags InternalInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags InternalStatic = BindingFlags.Static | BindingFlags.NonPublic;

        [TestCase(17)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void MeshSeal_RejectsWholeOverLimitFrameBeforeChangingGeometry(int capacity)
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(capacity);
            Invoke(backend, "SealCapacity", capacity);
            var frame = new BattlePresentationFrame();
            AddCommands(frame, capacity);
            backend.Build(frame, new Resolver(), BattleCentralDrawMode.StrictOrderedDraw);
            int mutation = (int)Field(backend, "mutationVersion");
            object chunks = Field(backend, "chunks");
            var overflow = new BattlePresentationFrame();
            AddCommands(overflow, capacity + 1);

            Assert.That((bool)Invoke(backend, "CanBuildFrame", overflow), Is.False);
            Assert.Throws<InvalidOperationException>(() => backend.Build(overflow, new Resolver()));
            Assert.That(Field(backend, "mutationVersion"), Is.EqualTo(mutation));
            Assert.That(Field(backend, "chunks"), Is.SameAs(chunks));
            Assert.That(Field(backend, "builtFrame"), Is.SameAs(frame));
            Assert.That(backend.SegmentCount, Is.EqualTo(capacity));
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
        }

        [Test]
        public void MeshSeal_ZeroCapacityAndUnsealDoNotUseChunkSizeAsLimit()
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(0);
            Invoke(backend, "SealCapacity", 0);
            var frame = new BattlePresentationFrame();
            Assert.That((bool)Invoke(backend, "CanBuildFrame", frame), Is.True);
            AddCommands(frame, 1);
            Assert.That((bool)Invoke(backend, "CanBuildFrame", frame), Is.False);
            Assert.Throws<InvalidOperationException>(() => backend.PrepareCapacity(1));
            Invoke(backend, "UnsealCapacity");
            backend.PrepareCapacity(1);
            backend.Build(frame, new Resolver());
            Assert.That(backend.SegmentCount, Is.EqualTo(1));
        }

        [TestCase(false, 0.5)]
        [TestCase(true, 0.5)]
        [TestCase(false, 1.0)]
        [TestCase(true, 1.0)]
        public void MotionSeal_RejectsEitherGenerationBeyondLogicalSlotLimit(bool overflowCurrent, double alpha)
        {
            var display = new BattlePresentationDisplayMotion();
            display.PrepareCapacity(17);
            Invoke(display, "SealCapacity", 17);
            BattlePresentationFrame valid = MotionFrame(16, 16);
            display.Prepare(valid, 0.5, 1.0, 1.0);
            int generation = (int)Field(display, "generation");
            object storage = Field(display, "previousIndexBySlot");
            BattlePresentationFrame invalid = MotionFrame(overflowCurrent ? 16 : 17, overflowCurrent ? 17 : 16);

            Assert.That((bool)Invoke(display, "CanPrepare", invalid), Is.False);
            Assert.Throws<InvalidOperationException>(() => display.Prepare(invalid, alpha, 1.0, 1.0));
            Assert.That(Field(display, "generation"), Is.EqualTo(generation));
            Assert.That(Field(display, "previousIndexBySlot"), Is.SameAs(storage));
            Assert.That(display.TryGet(new RuntimeEntityHandle(16, 1), out _), Is.True);
        }

        [Test]
        public void MotionSeal_AcceptedVaryingSamplesAndPreflightAllocateZeroBytes()
        {
            var display = new BattlePresentationDisplayMotion();
            display.PrepareCapacity(17);
            Invoke(display, "SealCapacity", 17);
            var frame = MotionFrame(16, 16);
            display.Prepare(frame, 0.25, 1.0, 1.0);
            object storage = Field(display, "sampledDeltaBySlot");
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 64; index++)
                display.Prepare(frame, 0.1 + index / 100.0, 1.0, 1.0);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            Assert.That(Field(display, "sampledDeltaBySlot"), Is.SameAs(storage));
            Assert.That(display.SampledCount, Is.EqualTo(1));
            Assert.That(frame.GetMotionState(0).PreciseX, Is.EqualTo(120));
            Assert.Throws<InvalidOperationException>(() => display.PrepareCapacity(33));
            Invoke(display, "UnsealCapacity");
            display.PrepareCapacity(33);
        }

        [TestCase("EntityCount")]
        [TestCase("HitRecordCount")]
        [TestCase("CommandCount")]
        [TestCase("MotionStateCount")]
        [TestCase("PreviousMotionStateCount")]
        public void SubmissionSeal_RejectsCountOverflowBeforeCopyingAnySnapshot(string countName)
        {
            using var mesh = new BattleDynamicMeshBackend();
            using var foot = new BattleFootMarkerBatchBackend();
            using var health = new BattleHealthBarBatchBackend();
            var submission = (BattleCentralSubmission)Activator.CreateInstance(typeof(BattleCentralSubmission),
                InternalInstance, null, new object[] { mesh, foot, health }, null);
            Invoke(submission, "PrepareCapacity", 1, 1, 1);
            Invoke(submission, "SealCapacity");
            var source = new BattlePresentationFrame { TickIndex = 10 };
            var captured = (BattlePresentationFrame)Invoke(submission, "CaptureFrame", source, null);
            var overflow = new BattlePresentationFrame { TickIndex = 11 };
            PropertyInfo countProperty = typeof(BattlePresentationFrame).GetProperty(countName);
            Assert.That(countProperty.GetSetMethod(true), Is.Not.Null, "Fixture requires the existing private counter setter.");
            countProperty.SetValue(overflow, 2);
            Assert.That((bool)Invoke(submission, "CanCaptureFrame", overflow), Is.False);
            var exception = Assert.Throws<TargetInvocationException>(() =>
                Invoke(submission, "CaptureFrame", overflow, null));
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(captured.TickIndex, Is.EqualTo(10));
            Assert.That(captured.CommandCount, Is.Zero);
            Assert.That(captured.EntityCapacity, Is.GreaterThanOrEqualTo(1));
            Assert.That(submission.IsRetired, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CentralSeal_RejectsWholeSubmissionAndPreservesLastGoodReadLease(bool hasLastGood)
        {
            BattleCentralRenderSystem.ResetRuntime();
            InvokeStatic("EndBattleCapacitySeal");
            var world = new SimulationWorld();
            IDisposable lease = null;
            try
            {
                world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                InvokeStatic("PrepareBattleCapacity", 1, 1, 0);
                world.BattlePresentation.BeginFrame(world, 10);
                BattlePixelFramePlan good = default;
                if (hasLastGood)
                {
                    good = BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                    object[] acquireArgs = { null };
                    Assert.That((bool)InvokeWithArgs(good.Submission, "TryAcquire", acquireArgs), Is.True);
                    lease = (IDisposable)acquireArgs[0];
                    Assert.That(good.Submission.ReadLeaseCount, Is.EqualTo(1));
                    var resize = Assert.Throws<TargetInvocationException>(() =>
                        InvokeStatic("PrepareBattleCapacity", 2, 2, 0));
                    Assert.That(resize.InnerException, Is.TypeOf<InvalidOperationException>());
                }
                world.BattlePresentation.BeginFrame(world, 11);
                AddCommands(world.BattlePresentation.PublishedFrame, 2);
                var refused = (BattlePixelFramePlan)InvokeStatic("PrepareFrameImmediate", world, 0.5);
                Assert.That(refused.IsStale, Is.True);
                Assert.That(refused.SimulationTick, Is.EqualTo(11));
                Assert.That(refused.SuppressesLegacyMaterializers, Is.True);
                Assert.That(refused.Reason, Does.Contain("capacity"));
                Assert.That(refused.Submission, Is.SameAs(good.Submission));
                if (hasLastGood)
                {
                    Assert.That(refused.DisplayTick, Is.EqualTo(10));
                    Assert.That(good.Submission.IsRetired, Is.False);
                    Assert.That(good.Submission.ReadLeaseCount, Is.EqualTo(1));
                    Assert.That(good.CapturedFrame.CommandCount, Is.Zero);
                }
            }
            finally
            {
                lease?.Dispose();
                InvokeStatic("EndBattleCapacitySeal");
                BattleCentralRenderSystem.ResetRuntime();
                world.ResetRuntimeState();
            }
        }

        private static object Field(object owner, string name)
        {
            return owner.GetType().GetField(name, InternalInstance).GetValue(owner);
        }

        private static object Invoke(object owner, string name, params object[] arguments)
        {
            return InvokeWithArgs(owner, name, arguments);
        }

        private static object InvokeWithArgs(object owner, string name, object[] arguments)
        {
            MethodInfo method = owner.GetType().GetMethod(name, InternalInstance);
            Assert.That(method, Is.Not.Null, name);
            return method.Invoke(owner, arguments);
        }

        private static object InvokeStatic(string name, params object[] arguments)
        {
            return typeof(BattleCentralRenderSystem).GetMethod(name, InternalStatic).Invoke(null, arguments);
        }

        private static BattlePresentationFrame MotionFrame(int priorSlot, int currentSlot)
        {
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.SetSourceRulePosition(100, 100);
            var prior = new BattlePresentationFrame { TickIndex = 10 };
            prior.AddMotionState(new BattlePresentationMotionState(new RuntimeEntityHandle(priorSlot, 1), 2, runtime));
            runtime.SetSourceRulePosition(120, 100);
            var current = new BattlePresentationFrame { TickIndex = 11 };
            current.AddMotionState(new BattlePresentationMotionState(new RuntimeEntityHandle(currentSlot, 1), 2, runtime));
            current.CopyPreviousMotionStatesFrom(prior);
            return current;
        }

        private static void AddCommands(BattlePresentationFrame frame, int count)
        {
            for (int index = 0; index < count; index++)
                frame.AddCommand(new BattleRenderCommand(BattleRenderCommandType.Entity, RuntimeEntityHandle.Invalid,
                    index, 0, index, 0, index, index, 0, index, Vector3.zero, Vector2.one,
                    new Vector2(0.5f, 0.5f), new Rect(0f, 0f, 1f, 1f), false, default));
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(null, null, new Rect(0f, 0f, 1f, 1f),
                    Vector2.one, new Vector2(0.5f, 0.5f), Color.white, 0);
                return BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif
