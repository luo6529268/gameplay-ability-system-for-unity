#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NTSD.Test
{
    public sealed class BattleAuxiliaryPresentationCapacitySealEditorTests
    {
        private const BindingFlags InternalInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags InternalStatic = BindingFlags.Static | BindingFlags.NonPublic;

        [TestCase(false, 17)]
        [TestCase(true, 17)]
        [TestCase(false, 4096)]
        [TestCase(true, 1365)]
        public void AuxiliarySeal_RejectsWholeOverflowBeforeChangingStorage(bool health, int limit)
        {
            using var fixture = new Fixture();
            using IDisposable owner = NewBackend(health);
            Prepare(owner, limit);
            Invoke(owner, "SealCapacity", limit);
            var valid = Frame(limit, !health, health);
            Build(owner, valid, fixture.Sprite, true);
            int mutation = Mutation(owner);
            object vertices = Field(owner, "vertices");
            Mesh mesh = Mesh(owner);
            Vector3 firstVertex = mesh.vertices[0];
            var overflow = Frame(limit + 1, !health, health);

            Assert.That(CanBuild(owner, overflow, fixture.Sprite, true), Is.False);
            Assert.Throws<InvalidOperationException>(() => Build(owner, overflow, fixture.Sprite, true));
            Assert.That(Mutation(owner), Is.EqualTo(mutation));
            Assert.That(BuiltFrame(owner), Is.SameAs(valid));
            Assert.That(Field(owner, "vertices"), Is.SameAs(vertices));
            Assert.That(Mesh(owner), Is.SameAs(mesh));
            Assert.That(Mesh(owner).vertices[0], Is.EqualTo(firstVertex));
            Assert.That(ActiveCount(owner), Is.EqualTo(limit));
            Assert.Throws<InvalidOperationException>(() => Prepare(owner, limit));
            Invoke(owner, "UnsealCapacity");
            Prepare(owner, limit);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AuxiliarySeal_CountsOnlyEligibleCommands(bool health)
        {
            using var fixture = new Fixture();
            using IDisposable owner = NewBackend(health);
            Prepare(owner, 1);
            Invoke(owner, "SealCapacity", 1);
            var frame = Frame(1, !health, health);
            frame.AddCommand(Command(10, false, false, 100));
            frame.AddCommand(Command(11, true, true, 100, BattleRenderCommandType.Shadow));
            if (health)
                frame.AddCommand(Command(12, false, true, 0));
            Build(owner, frame, fixture.Sprite, true);
            Assert.That(CanBuild(owner, frame, fixture.Sprite, true), Is.True);
            Assert.That(ActiveCount(owner), Is.EqualTo(1));
            Assert.That(frame.EntityCount, Is.Zero);
            Assert.That(frame.CommandCount, Is.GreaterThan(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AuxiliarySeal_ZeroLimitAllowsDisabledButRejectsVisibleOutput(bool health)
        {
            using var fixture = new Fixture();
            using IDisposable owner = NewBackend(health);
            Prepare(owner, 0);
            Invoke(owner, "SealCapacity", 0);
            var frame = Frame(1, !health, health);
            Assert.That(CanBuild(owner, frame, fixture.Sprite, false), Is.True);
            Build(owner, frame, fixture.Sprite, false);
            Assert.That(ActiveCount(owner), Is.Zero);
            if (!health)
            {
                Assert.That(CanBuild(owner, frame, null, true), Is.True);
                Build(owner, frame, null, true);
            }
            Assert.That(CanBuild(owner, frame, fixture.Sprite, true), Is.False);
            Assert.Throws<InvalidOperationException>(() => Build(owner, frame, fixture.Sprite, true));
            Invoke(owner, "UnsealCapacity");
            Prepare(owner, 1);
            Build(owner, frame, fixture.Sprite, true);
            Assert.That(ActiveCount(owner), Is.EqualTo(1));
        }

        [Test]
        public void HealthDirectBuild_RejectsLogicalOverflowBeforeMutation()
        {
            using var health = new BattleHealthBarBatchBackend();
            health.PrepareCapacity(17);
            Invoke(health, "SealCapacity", 17);
            var instances = new BattleHealthBarInstance[18];
            for (int index = 0; index < instances.Length; index++)
                instances[index] = new BattleHealthBarInstance(Vector2.zero, 0f, 50, 75, 100);
            health.Build(instances, 17, BattleHealthBarStyle.Default);
            int mutation = health.MutationVersion;
            object runtimeInstances = Field(health, "runtimeInstances");
            Assert.Throws<InvalidOperationException>(() => health.Build(instances, 18, BattleHealthBarStyle.Default));
            Assert.That(health.MutationVersion, Is.EqualTo(mutation));
            Assert.That(health.ActiveBarCount, Is.EqualTo(17));
            Assert.That(Field(health, "runtimeInstances"), Is.SameAs(runtimeInstances));
            Assert.That(health.Capacity, Is.EqualTo(32));
        }

        [Test]
        public void CentralSeal_SealsBothAuxiliaryOwnersAndEndUnsealsThem()
        {
            InvokeStatic("EndBattleCapacitySeal");
            BattleCentralRenderSystem.ResetRuntime();
            try
            {
                InvokeStatic("PrepareBattleCapacity", 17, 64, 0);
                var slots = (BattleCentralSubmission[])StaticField("SlotSubmissions");
                Assert.That(slots.Length, Is.EqualTo(2));
                foreach (BattleCentralSubmission slot in slots)
                {
                    Assert.That(Field(slot.FootMarkerBackend, "capacitySealed"), Is.True);
                    Assert.That(Field(slot.HealthBackend, "capacitySealed"), Is.True);
                    Assert.Throws<InvalidOperationException>(() => slot.FootMarkerBackend.PrepareCapacity(18));
                    Assert.Throws<InvalidOperationException>(() => slot.HealthBackend.PrepareCapacity(18));
                }
                InvokeStatic("EndBattleCapacitySeal");
                foreach (BattleCentralSubmission slot in slots)
                {
                    Assert.That(Field(slot.FootMarkerBackend, "capacitySealed"), Is.False);
                    Assert.That(Field(slot.HealthBackend, "capacitySealed"), Is.False);
                }
            }
            finally
            {
                InvokeStatic("EndBattleCapacitySeal");
                BattleCentralRenderSystem.ResetRuntime();
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void CentralAuxiliaryOverflow_RejectsWholeSubmissionAndPreservesLease(bool health, bool hasLastGood)
        {
            using var fixture = new Fixture();
            InvokeStatic("EndBattleCapacitySeal");
            BattleCentralRenderSystem.ResetRuntime();
            var world = new SimulationWorld();
            object oldFootEnabled = StaticField("runtimeFootMarkersEnabled");
            object oldFootSprite = StaticField("runtimeFootMarkerSprite");
            object oldHealthEnabled = StaticField("runtimeHealthBarsEnabled");
            IDisposable lease = null;
            try
            {
                SetStatic("runtimeFootMarkersEnabled", !health);
                SetStatic("runtimeFootMarkerSprite", fixture.Sprite);
                SetStatic("runtimeHealthBarsEnabled", health);
                world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                InvokeStatic("PrepareBattleCapacity", 1, 8, 0);
                world.BattlePresentation.BeginFrame(world, 10);
                BattlePixelFramePlan good = default;
                if (hasLastGood)
                {
                    good = BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                    object[] arguments = { null };
                    Assert.That((bool)Invoke(good.Submission, "TryAcquire", arguments), Is.True);
                    lease = (IDisposable)arguments[0];
                }
                world.BattlePresentation.BeginFrame(world, 11);
                BattlePresentationFrame overflow = world.BattlePresentation.PublishedFrame;
                overflow.AddCommand(Command(1, !health, health, 100));
                overflow.AddCommand(Command(2, !health, health, 100));
                var refused = (BattlePixelFramePlan)InvokeStatic("PrepareFrameImmediate", world, 0.5);
                Assert.That(refused.Reason, Does.Contain("capacity"));
                Assert.That(refused.IsStale, Is.True);
                Assert.That(refused.SuppressesLegacyMaterializers, Is.True);
                Assert.That(refused.Submission, Is.SameAs(good.Submission));
                if (hasLastGood)
                {
                    Assert.That(good.Submission.ReadLeaseCount, Is.EqualTo(1));
                    Assert.That(good.Submission.IsRetired, Is.False);
                    Assert.That(refused.DisplayTick, Is.EqualTo(10));
                    Assert.That(good.CapturedFrame.CommandCount, Is.Zero);
                }
            }
            finally
            {
                lease?.Dispose();
                InvokeStatic("EndBattleCapacitySeal");
                BattleCentralRenderSystem.ResetRuntime();
                world.ResetRuntimeState();
                SetStatic("runtimeFootMarkersEnabled", oldFootEnabled);
                SetStatic("runtimeFootMarkerSprite", oldFootSprite);
                SetStatic("runtimeHealthBarsEnabled", oldHealthEnabled);
            }
        }

        [Test]
        public void AuxiliarySealedBuild_AlternatingFramesReuseStorageAndAllocateZeroBytes()
        {
            using var fixture = new Fixture();
            using var foot = new BattleFootMarkerBatchBackend();
            using var health = new BattleHealthBarBatchBackend();
            foot.PrepareCapacity(1);
            health.PrepareCapacity(1);
            Invoke(foot, "SealCapacity", 1);
            Invoke(health, "SealCapacity", 1);
            var first = Frame(1, true, true);
            var second = new BattlePresentationFrame();
            second.AddCommand(Command(20, true, true, 100));
            foot.BuildFromFrame(first, fixture.Sprite, BattleFootMarkerStyle.Default, true);
            health.BuildFromFrame(first, BattleHealthBarStyle.Default, true);
            foot.BuildFromFrame(second, fixture.Sprite, BattleFootMarkerStyle.Default, true);
            health.BuildFromFrame(second, BattleHealthBarStyle.Default, true);
            object footVertices = Field(foot, "vertices");
            object healthVertices = Field(health, "vertices");
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 64; index++)
            {
                BattlePresentationFrame frame = (index & 1) == 0 ? first : second;
                foot.BuildFromFrame(frame, fixture.Sprite, BattleFootMarkerStyle.Default, true);
                health.BuildFromFrame(frame, BattleHealthBarStyle.Default, true);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            Assert.That(Field(foot, "vertices"), Is.SameAs(footVertices));
            Assert.That(Field(health, "vertices"), Is.SameAs(healthVertices));
            Assert.That(first.GetCommand(0).CurrentHealth, Is.EqualTo(40));
            Assert.That(second.GetCommand(0).Position.x, Is.EqualTo(20f));
        }

        private static IDisposable NewBackend(bool health)
        {
            return health ? (IDisposable)new BattleHealthBarBatchBackend() : new BattleFootMarkerBatchBackend();
        }

        private static void Prepare(object owner, int count)
        {
            if (owner is BattleHealthBarBatchBackend health)
                health.PrepareCapacity(count);
            else
                ((BattleFootMarkerBatchBackend)owner).PrepareCapacity(count);
        }

        private static void Build(object owner, BattlePresentationFrame frame, Sprite sprite, bool enabled)
        {
            if (owner is BattleHealthBarBatchBackend health)
                health.BuildFromFrame(frame, BattleHealthBarStyle.Default, enabled);
            else
                ((BattleFootMarkerBatchBackend)owner).BuildFromFrame(frame, sprite, BattleFootMarkerStyle.Default, enabled);
        }

        private static bool CanBuild(object owner, BattlePresentationFrame frame, Sprite sprite, bool enabled)
        {
            return owner is BattleHealthBarBatchBackend
                ? (bool)Invoke(owner, "CanBuildFrame", frame, enabled)
                : (bool)Invoke(owner, "CanBuildFrame", frame, sprite, enabled);
        }

        private static Mesh Mesh(object owner) => owner is BattleHealthBarBatchBackend health
            ? health.Mesh : ((BattleFootMarkerBatchBackend)owner).Mesh;
        private static int Mutation(object owner) => owner is BattleHealthBarBatchBackend health
            ? health.MutationVersion : ((BattleFootMarkerBatchBackend)owner).MutationVersion;
        private static int ActiveCount(object owner) => owner is BattleHealthBarBatchBackend health
            ? health.ActiveBarCount : ((BattleFootMarkerBatchBackend)owner).ActiveMarkerCount;
        private static BattlePresentationFrame BuiltFrame(object owner) => owner is BattleHealthBarBatchBackend health
            ? health.BuiltFrame : ((BattleFootMarkerBatchBackend)owner).BuiltFrame;

        private static object Field(object owner, string name)
        {
            FieldInfo field = owner.GetType().GetField(name, InternalInstance);
            Assert.That(field, Is.Not.Null, name);
            return field.GetValue(owner);
        }

        private static object Invoke(object owner, string name, params object[] arguments)
        {
            MethodInfo method = owner.GetType().GetMethod(name, InternalInstance);
            Assert.That(method, Is.Not.Null, name);
            return method.Invoke(owner, arguments);
        }

        private static object InvokeStatic(string name, params object[] arguments)
        {
            return typeof(BattleCentralRenderSystem).GetMethod(name, InternalStatic).Invoke(null, arguments);
        }

        private static object StaticField(string name)
        {
            return typeof(BattleCentralRenderSystem).GetField(name, InternalStatic).GetValue(null);
        }

        private static void SetStatic(string name, object value)
        {
            typeof(BattleCentralRenderSystem).GetField(name, InternalStatic).SetValue(null, value);
        }

        private static BattlePresentationFrame Frame(int count, bool foot, bool health)
        {
            var frame = new BattlePresentationFrame();
            for (int index = 0; index < count; index++)
                frame.AddCommand(Command(index, foot, health, 100));
            return frame;
        }

        private static BattleRenderCommand Command(int index, bool foot, bool health, int maximumHealth,
            BattleRenderCommandType type = BattleRenderCommandType.Entity)
        {
            return new BattleRenderCommand(type, RuntimeEntityHandle.Invalid, index, 0, index, 0,
                index, index, 0, index, new Vector3(index, 0f, 0f), Vector2.one,
                new Vector2(0.5f, 0.5f), new Rect(0f, 0f, 1f, 1f), false, default,
                health, 40, 80, maximumHealth, new Vector2(index, 3f), true,
                stableFootAnchorWorld: new Vector2(index, 0f), hasStableFootAnchor: true,
                showSelfFootMarker: foot, footMarkerScale: 1f);
        }

        private sealed class Fixture : IDisposable
        {
            private readonly Texture2D texture = new Texture2D(4, 4);
            public Sprite Sprite { get; }

            public Fixture()
            {
                Sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 1f);
            }

            public void Dispose()
            {
                Object.DestroyImmediate(Sprite);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
#endif
