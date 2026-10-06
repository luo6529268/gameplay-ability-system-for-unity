#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Test
{
    public sealed class BattlePresentationCapacityPrewarmEditorTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
        private static readonly string[] MotionArrayNames =
        {
            "previousIndexBySlot", "previousGenerationBySlot", "sampledGenerationBySlot",
            "sampledHandleBySlot", "sampledDeltaBySlot",
        };

        [Test]
        public void CentralPrewarm_PreparesDisplayMotionForReservedRuntimeSlots()
        {
            MethodInfo prepare = typeof(BattleCentralRenderSystem).GetMethod(
                "PrepareBattleCapacity", PrivateStatic);
            MethodInfo endSeal = typeof(BattleCentralRenderSystem).GetMethod(
                "EndBattleCapacitySeal", PrivateStatic);
            try
            {
                prepare.Invoke(null, new object[] { 1050, 0, 0 });
                var motion = (BattlePresentationDisplayMotion)typeof(BattleCentralRenderSystem)
                    .GetField("DisplayMotion", PrivateStatic).GetValue(null);
                foreach (string name in MotionArrayNames)
                {
                    var storage = (Array)typeof(BattlePresentationDisplayMotion)
                        .GetField(name, PrivateInstance).GetValue(motion);
                    Assert.That(storage.Length, Is.GreaterThanOrEqualTo(1050), name);
                }
            }
            finally
            {
                endSeal.Invoke(null, null);
            }
        }

        [TestCase(1)]
        [TestCase(32)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void MeshPrewarm_PreparesWorstCaseDescriptorsInEveryRequiredChunk(int commandCapacity)
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(commandCapacity);
            int chunkCount = (commandCapacity + BattleDynamicMeshBackend.QuadsPerChunk - 1) /
                             BattleDynamicMeshBackend.QuadsPerChunk;
            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                SubMeshDescriptor[] descriptors = Descriptors(backend, chunkIndex);
                int expected = Math.Min(BattleDynamicMeshBackend.QuadsPerChunk,
                    commandCapacity - chunkIndex * BattleDynamicMeshBackend.QuadsPerChunk);
                Assert.That(descriptors, Is.Not.Null, "Managed descriptors must exist before Upload.");
                Assert.That(descriptors.Length, Is.GreaterThanOrEqualTo(expected));
                Assert.That(descriptors.Length, Is.LessThanOrEqualTo(BattleDynamicMeshBackend.QuadsPerChunk));
            }
            Assert.That(backend.ActiveChunkCount, Is.Zero);
            Assert.That(backend.SegmentCount, Is.Zero);
        }

        [Test]
        public void MeshPrewarm_PreservesDescriptorStorageAcrossNewSegmentHighWater()
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(32);
            SubMeshDescriptor[] storage = Descriptors(backend, 0);
            Assert.That(storage, Is.Not.Null);
            var frame = new BattlePresentationFrame();
            var resolver = new Resolver();
            AddCommands(frame, 1);
            backend.Build(frame, resolver, BattleCentralDrawMode.StrictOrderedDraw);
            Assert.That(Descriptors(backend, 0), Is.SameAs(storage));
            AddCommands(frame, 31);
            backend.Build(frame, resolver, BattleCentralDrawMode.StrictOrderedDraw);
            Assert.That(backend.SegmentCount, Is.EqualTo(32));
            Assert.That(Descriptors(backend, 0), Is.SameAs(storage));
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
            backend.PrepareCapacity(1);
            Assert.That(Descriptors(backend, 0), Is.SameAs(storage));
        }

        [Test]
        public void DisplayPrewarm_HighSlotInterpolationReusesAllStorageWithoutAllocation()
        {
            var motion = new BattlePresentationDisplayMotion();
            motion.PrepareCapacity(1050);
            var previous = new BattlePresentationFrame { TickIndex = 10 };
            var current = new BattlePresentationFrame { TickIndex = 11 };
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.SetSourceRulePosition(100, 100);
            var handle = new RuntimeEntityHandle(1049, 1);
            previous.AddMotionState(new BattlePresentationMotionState(handle, 2, runtime));
            runtime.SetSourceRulePosition(120, 100);
            current.AddMotionState(new BattlePresentationMotionState(handle, 2, runtime));
            current.CopyPreviousMotionStatesFrom(previous);
            var storage = new object[MotionArrayNames.Length];
            for (int index = 0; index < storage.Length; index++)
                storage[index] = typeof(BattlePresentationDisplayMotion)
                    .GetField(MotionArrayNames[index], PrivateInstance).GetValue(motion);

            motion.Prepare(current, 0.5, 1.0, 1.0);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 64; index++)
                motion.Prepare(current, 0.5, 1.0, 1.0);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

            Assert.That(allocated, Is.Zero, "Only the sampled lookup is measured, not full submission.");
            Assert.That(motion.SampledCount, Is.EqualTo(1));
            Assert.That(motion.TryGet(handle, out _), Is.True);
            for (int index = 0; index < storage.Length; index++)
                Assert.That(typeof(BattlePresentationDisplayMotion)
                    .GetField(MotionArrayNames[index], PrivateInstance).GetValue(motion),
                    Is.SameAs(storage[index]));
            Assert.That(current.GetMotionState(0).PreciseX, Is.EqualTo(120));
        }

        private static SubMeshDescriptor[] Descriptors(BattleDynamicMeshBackend backend, int chunkIndex)
        {
            var chunks = (Array)typeof(BattleDynamicMeshBackend)
                .GetField("chunks", PrivateInstance).GetValue(backend);
            object chunk = chunks.GetValue(chunkIndex);
            return (SubMeshDescriptor[])chunk.GetType()
                .GetField("subMeshDescriptors", PrivateInstance).GetValue(chunk);
        }

        private static void AddCommands(BattlePresentationFrame frame, int count)
        {
            for (int index = 0; index < count; index++)
                frame.AddCommand(new BattleRenderCommand(
                    BattleRenderCommandType.Entity, RuntimeEntityHandle.Invalid,
                    index, 0, index, 0, index, index, 0, index,
                    Vector3.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                    new Rect(0f, 0f, 1f, 1f), false, default));
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(null, null,
                    new Rect(0f, 0f, 1f, 1f), Vector2.one,
                    new Vector2(0.5f, 0.5f), Color.white, 0);
                return BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif
