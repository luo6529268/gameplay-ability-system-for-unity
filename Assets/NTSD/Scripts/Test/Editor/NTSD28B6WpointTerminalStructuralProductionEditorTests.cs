#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6WpointTerminalStructuralProductionEditorTests
    {
        private static IEnumerable<TestCaseData> TerminalCases()
        {
            foreach (bool real in new[] { false, true })
            foreach (int action in new[] { 1000, 1777 })
            foreach (bool missing in new[] { false, true })
            foreach (bool renderer in new[] { false, true })
            foreach (int childSlot in new[] { 0, 399 })
            foreach (bool overlap in new[] { false, true })
                yield return new TestCaseData(real, action, missing, renderer, childSlot, overlap)
                    .SetName($"Terminal_{(real ? "Real" : "Generic")}_{action}_Missing{missing}_Renderer{renderer}_Slot{childSlot}_Overlap{overlap}");
        }

        [TestCaseSource(nameof(TerminalCases))]
        public void Terminal_FreesBeforeChildWritesAndRng(bool real, int action, bool missing,
            bool renderer, int childSlot, bool overlap)
        {
            try
            {
                using (Scope scope = Create(real, action, missing, renderer, childSlot, overlap))
                    AssertTerminal(scope);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.ToString());
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Terminal_Type2DoesNotConsumeDvxPrefixOrKind3(bool real)
        {
            using (Scope scope = Create(real, 1000, true, false, 399, true, type: 2))
                AssertTerminal(scope);
        }

        [TestCase(122, false, false)]
        [TestCase(122, false, true)]
        [TestCase(122, true, false)]
        [TestCase(122, true, true)]
        [TestCase(123, false, false)]
        [TestCase(123, false, true)]
        [TestCase(123, true, false)]
        [TestCase(123, true, true)]
        public void Terminal_RefillRunsFirstAndExhaustionRetainsEntity(int oid, bool exhausted, bool missing)
        {
            using (Scope scope = Create(true, 1000, missing, false, 399, true, type: 6, oid: oid, holderState: 17))
            {
                int decrement = oid == 122 ? 1 : 2;
                scope.Child.Health.HP = exhausted ? decrement : (oid == 122 ? 11 : 12);
                scope.Holder.Health.HP = 100;
                scope.Holder.Health.HPBound = 200;
                scope.Holder.Health.HP3 = 500;
                scope.Holder.Health.PP = 10;
                int beforeHp = scope.Child.Health.HP;
                var before = scope.World.StructuralWriterDiagnosticsForDiagnostics;
                ulong legacy = scope.World.Rng.CallCount;
                ulong native = NativeCalls(scope.World);
                var predictor = new DeterministicRng(scope.World.Rng.State);
                int kick = predictor.NextInt(0, 7) - 3;

                scope.World.HeldObjectProcessAll(7);

                var after = scope.World.StructuralWriterDiagnosticsForDiagnostics;
                Assert.That(after.FreeCount - before.FreeCount, Is.EqualTo(exhausted ? 0 : 1));
                Assert.That(after.GenerationReleaseCount - before.GenerationReleaseCount, Is.EqualTo(exhausted ? 0 : 1));
                Assert.That(NativeCalls(scope.World), Is.EqualTo(native), "No terminal/kind3 synchronized draws.");
                Assert.That(scope.World.Rng.CallCount - legacy, Is.EqualTo(exhausted ? 1UL : 0UL));
                if (exhausted)
                {
                    Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.ChildHandle, out LF2Entity live), Is.True);
                    Assert.That(live.Health.HP, Is.Zero);
                    Assert.That(live.Frame.N, Is.Zero);
                    Assert.That(scope.Holder.Frame.N, Is.Zero);
                    Assert.That(live.Runtime.LinkState, Is.Zero);
                    Assert.That(live.Runtime.Vx, Is.EqualTo(kick));
                    Assert.That(live.Runtime.Vy, Is.Zero);
                    Assert.That(live.Runtime.Vz, Is.EqualTo(6.5));
                    scope.World.HeldObjectProcessAll(7);
                    Assert.That(scope.World.StructuralWriterDiagnosticsForDiagnostics.FreeCount, Is.EqualTo(before.FreeCount));
                }
                else
                {
                    Assert.That(scope.Audit.RemovalHp, Is.EqualTo(beforeHp - decrement), "Refill must run before terminal free.");
                    Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.ChildHandle, out _), Is.False);
                    Assert.That(scope.Audit.RefreshAfterRemoved, Is.Zero);
                    Assert.That(scope.Audit.UnregisterCalls, Is.EqualTo(1));
                    Assert.That(scope.Holder.Health.PP, Is.EqualTo(oid == 122 ? 10 : 13));
                    if (oid == 122)
                    {
                        Assert.That(scope.Holder.Health.HP, Is.EqualTo(104));
                        Assert.That(scope.Holder.Health.HPBound, Is.EqualTo(202));
                    }
                }
                Assert.That(after.DestroyCount, Is.EqualTo(before.DestroyCount));
            }
        }

        [TestCase(0)]
        [TestCase(399)]
        public void Terminal_UsesAtomicThirdPartyCleanupAndSafeSlotReuse(int childSlot)
        {
            using (Scope scope = Create(true, 1000, false, false, childSlot, true))
            {
                LF2Character heldObserver = scope.AddCharacter(2);
                LF2Character catchObserver = scope.AddCharacter(3);
                LF2Character encodedObserver = scope.AddCharacter(4);
                LF2Character plainObserver = scope.AddCharacter(5);
                heldObserver.Runtime.LinkState = -1;
                heldObserver.Runtime.HolderStableId = childSlot;
                catchObserver.Runtime.CaughtSlotIndex = childSlot;
                catchObserver.Runtime.CaughtDuration = 77;
                encodedObserver.Runtime.CatchSourceSlot90 = 0x2000 + childSlot;
                encodedObserver.Runtime.CatcherSlotIndex = 91;
                encodedObserver.Runtime.CaughtDuration = 88;
                encodedObserver.Runtime.OwnerSlotIndex = 17;
                plainObserver.Runtime.CatchSourceSlot90 = childSlot;
                plainObserver.Runtime.CatcherSlotIndex = childSlot;
                plainObserver.Runtime.CaughtDuration = 99;

                scope.World.HeldObjectProcessAll(7);

                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.ChildHandle, out _), Is.False);
                Assert.That(heldObserver.Runtime.LinkState, Is.Zero);
                Assert.That(heldObserver.Runtime.HolderStableId, Is.Zero);
                Assert.That(catchObserver.Runtime.CaughtSlotIndex, Is.EqualTo(-1));
                Assert.That(catchObserver.Runtime.CaughtDuration, Is.Zero);
                Assert.That(encodedObserver.Runtime.CatchSourceSlot90, Is.EqualTo(-1));
                Assert.That(encodedObserver.Runtime.CatcherSlotIndex, Is.EqualTo(91));
                Assert.That(encodedObserver.Runtime.CaughtDuration, Is.Zero);
                Assert.That(encodedObserver.Runtime.OwnerSlotIndex, Is.EqualTo(17));
                Assert.That(typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0, Is.True);
                Assert.That(plainObserver.Runtime.CatchSourceSlot90, Is.EqualTo(-1));
                Assert.That(plainObserver.Runtime.CatcherSlotIndex, Is.EqualTo(-1));
                Assert.That(plainObserver.Runtime.CaughtDuration, Is.Zero);
                LF2Character replacement = scope.AddCharacter(childSlot);
                Assert.That(scope.World.TryGetCurrentRuntimeHandleForDiagnostics(childSlot, replacement, out RuntimeEntityHandle handle), Is.True);
                Assert.That(handle.Generation, Is.GreaterThan(scope.ChildHandle.Generation));
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(handle, out _), Is.True);
                Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
            }
        }

        [Test]
        public void Terminal_SelfLinkedHolderIsNotRefreshedAfterFree()
        {
            using (Scope scope = Create(false, 1000, false, false, 399, false))
            {
                scope.Child.FrameCache.Load(scope.Holder.FrameCache.Wrapper);
                scope.Child.DirectWriteHeldFramePreserveWaitCounter(0);
                scope.Child.Runtime.HolderStableId = 399;
                scope.Child.Runtime.TargetSlotIndex = 399;
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.ChildHandle, out _), Is.False);
                Assert.That(scope.Audit.RefreshAfterRemoved, Is.Zero);
                Assert.That(scope.Audit.UnregisterCalls, Is.EqualTo(1));
            }
        }

        [Test]
        public void Terminal_TraceDistinguishesNextChildAndSecondHeldPass()
        {
            using (Scope scope = Create(false, 1000, false, false, 50, true))
            {
                LF2Character secondHolder = scope.AddCharacter(1, terminal: true);
                LF2Character thirdHolder = scope.AddCharacter(2, terminal: false);
                LF2Entity second = scope.AddHeld(secondHolder, 51);
                LF2Entity third = scope.AddHeld(thirdHolder, 52);
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.World.FindEntityByRuntimeSlotForQuery(50), Is.Null);
                Assert.That(scope.World.FindEntityByRuntimeSlotForQuery(51), Is.Null);
                Assert.That(scope.World.FindEntityByRuntimeSlotForQuery(52), Is.SameAs(third));
                thirdHolder.Frame.D.wpoints[0].weaponact = 1000;
                scope.World.HeldObjectProcessAll(7);
                BattleParityStructuralEvent[] terminal = scope.Events.Events.Where(item => item.Action == "held-terminal").ToArray();
                CollectionAssert.AreEqual(new[] { 50, 51, 52 }, terminal.Select(item => item.Slot).ToArray());
                CollectionAssert.AreEqual(new[] { "held-refill:C09", "held-refill:C09", "held-refill:C20" }, terminal.Select(item => item.Pass).ToArray());
                CollectionAssert.AreEqual(new[] { 50, 51, 52 }, terminal.Select(item => item.CursorSlot).ToArray());
                Assert.That(scope.Events.Events.Count(item => item.Action == "free"), Is.EqualTo(3));
                Assert.That(scope.Events.Events.Where(item => item.Action == "free").All(item => item.Tick == 7), Is.True);
                Assert.That(scope.Audit.RefreshAfterRemoved, Is.Zero);
            }
        }

        [Test]
        public void Terminal_LogicOnlyPoolReturnsSameShellWithNewGeneration()
        {
            using (Scope scope = Create(false, 1000, false, false, 399, false))
            {
                scope.World.Unregister(scope.Child);
                var pooled = (LF2Weapon)scope.World.LogicReferencePool.Get(LF2ObjectType.LightWeapon, 9000);
                scope.BindHeld(pooled, scope.Holder, 399);
                Assert.That(scope.World.TryGetCurrentRuntimeHandleForDiagnostics(399, pooled, out RuntimeEntityHandle oldHandle), Is.True);
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(oldHandle, out _), Is.False);
                ILF2Object fetched = scope.World.LogicReferencePool.Get(LF2ObjectType.LightWeapon, 9000);
                Assert.That(fetched, Is.SameAs(pooled));
                var replacement = (LF2Weapon)fetched;
                scope.BindHeld(replacement, scope.Holder, 399);
                Assert.That(scope.World.TryGetCurrentRuntimeHandleForDiagnostics(399, replacement, out RuntimeEntityHandle newHandle), Is.True);
                Assert.That(newHandle.Generation, Is.GreaterThan(oldHandle.Generation));
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(oldHandle, out _), Is.False);
            }
        }

        [Test]
        public void Terminal_FullTickFreesOnceBeforeSerialAndDoesNotRepeatAtC20()
        {
            using (Scope scope = Create(true, 1000, false, false, 399, true))
            {
                var before = scope.World.StructuralWriterDiagnosticsForDiagnostics;
                new NTSDBattleTickSystem(scope.World).RunReleaseTick(41, buildPresentation: false);
                var after = scope.World.StructuralWriterDiagnosticsForDiagnostics;
                Assert.That(after.FreeCount - before.FreeCount, Is.EqualTo(1));
                Assert.That(after.GenerationReleaseCount - before.GenerationReleaseCount, Is.EqualTo(1));
                Assert.That(scope.Audit.RefreshAfterRemoved, Is.Zero);
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.ChildHandle, out _), Is.False);
                Assert.That(scope.Events.Events.Single(item => item.Action == "held-terminal").Pass, Is.EqualTo("held-refill:C09"));
                Assert.That(scope.Events.Events.Single(item => item.Action == "free").Tick, Is.EqualTo(41));
            }
        }

        private static void AssertTerminal(Scope scope)
        {
            var before = scope.World.StructuralWriterDiagnosticsForDiagnostics;
            string state = State(scope.Child);
            ulong legacy = scope.World.Rng.CallCount;
            ulong native = NativeCalls(scope.World);
            long sounds = scope.World.QueuedSoundEventCountForDiagnostics;
            scope.World.HeldObjectProcessAll(7);
            var after = scope.World.StructuralWriterDiagnosticsForDiagnostics;
            Assert.That(after.FreeCount - before.FreeCount, Is.EqualTo(1), "Terminal must use the production Free owner exactly once.");
            Assert.That(after.UnregisterCount - before.UnregisterCount, Is.EqualTo(1));
            Assert.That(after.GenerationReleaseCount - before.GenerationReleaseCount, Is.EqualTo(1));
            Assert.That(after.DestroyCount, Is.EqualTo(before.DestroyCount));
            Assert.That(scope.Audit.DestroyCalls, Is.Zero);
            Assert.That(scope.World.QueuedSoundEventCountForDiagnostics, Is.EqualTo(sounds));
            Assert.That(scope.Audit.UnregisterCalls, Is.EqualTo(1));
            Assert.That(scope.Audit.RemovalState, Is.EqualTo(state), "Terminal must precede child frame/facing/hold/pose/motion/weaponHP writes.");
            Assert.That(scope.Audit.RefreshAfterRemoved, Is.Zero, "Do not refresh the released/pool-returned shell.");
            Assert.That(scope.World.Rng.CallCount, Is.EqualTo(legacy));
            Assert.That(NativeCalls(scope.World), Is.EqualTo(native));
            Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.ChildHandle, out _), Is.False);
            Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
            Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.Zero);
            Assert.That(scope.Holder.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
            scope.Presentation?.AssertReturned();
            scope.World.HeldObjectProcessAll(7);
            Assert.That(scope.World.StructuralWriterDiagnosticsForDiagnostics.FreeCount, Is.EqualTo(after.FreeCount));
            Assert.That(NativeCalls(scope.World), Is.EqualTo(native));
        }

        private static ulong NativeCalls(SimulationWorld world) => world.NativeRandom.CaptureScalarState().SynchronizedCalls;
        private static string State(LF2Entity entity) => FormattableString.Invariant(
            $"{entity.Frame.N}/{entity.Runtime.Dir}/{entity.FrameDelay}/{entity.Trans?.WaitCounter}/{entity.Runtime.X}/{entity.Runtime.Y}/{entity.Runtime.Z}/{entity.Runtime.XInt}/{entity.Runtime.YInt}/{entity.Runtime.ZInt}/{entity.Runtime.Zz}/{entity.Runtime.Vx}/{entity.Runtime.Vy}/{entity.Runtime.Vz}/{entity.Runtime.WeaponFlightCounter}");

        private static Scope Create(bool real, int action, bool missing, bool renderer, int childSlot, bool overlap,
            int type = 1, int oid = 9000, int holderState = 0)
        {
            var scope = new Scope(oid, type);
            int holderSlot = childSlot == 0 ? 1 : 0;
            scope.Holder = scope.AddCharacter(holderSlot, terminal: true, state: holderState, action: action, overlap: overlap);
            scope.Child = real ? (LF2Entity)new WeaponProbe(scope.Audit) : new GenericProbe(scope.Audit);
            if (scope.Child is LF2Weapon weapon)
            {
                weapon.SetWeaponType(type);
                weapon.WeaponBrokenSound = "Goal12ForbiddenBrokenSound";
            }
            scope.BindHeld(scope.Child, scope.Holder, childSlot);
            if (missing)
                scope.Child.DirectWriteHeldFramePreserveWaitCounter(999);
            scope.Child.SwitchDir("right");
            scope.Child.FrameDelay = 13;
            scope.Child.Runtime.SetPosition(91, -17, 72);
            scope.Child.Runtime.SetVelocity(12, -8, 6.5);
            scope.Child.Runtime.WeaponFlightCounter = 31;
            Assert.That(scope.World.TryGetCurrentRuntimeHandleForDiagnostics(childSlot, scope.Child, out scope.ChildHandle), Is.True);
            if (renderer)
                scope.Presentation = new RendererScope(scope.Child);
            scope.World.SetStructuralEventSinkForDiagnostics(scope.Events, 7, "fixture");
            return scope;
        }

        private static LF2CharacterDataWrapper Data(int oid, int type, int state, WeaponPoint point)
        {
            var data = new LF2CharacterData { name = "TerminalFixture", type_sub = type, weapon_hp = 31,
                frames = new List<LF2FrameData>() };
            foreach (int frame in new[] { 0, 1, 2, 3, 4, 5, 20, 40, 1000, 1777 })
                data.frames.Add(new LF2FrameData { frameId = frame, state = state, wait = 100, next = frame,
                    centerx = 39, centery = 79, wpoints = new List<WeaponPoint> { point } });
            return new LF2CharacterDataWrapper(oid, data);
        }

        private sealed class Audit
        {
            internal bool Removed;
            internal int UnregisterCalls, DestroyCalls, RefreshAfterRemoved, RemovalHp;
            internal string RemovalState;
            internal void BeforeUnregister(LF2Entity entity)
            {
                UnregisterCalls++;
                RemovalState = State(entity);
                RemovalHp = entity.Health?.HP ?? int.MinValue;
            }
        }

        private sealed class WeaponProbe : LF2Weapon
        {
            private readonly Audit audit;
            internal WeaponProbe(Audit audit) { this.audit = audit; }
            public override void UnregisterFromWorld() { audit.BeforeUnregister(this); base.UnregisterFromWorld(); }
            public override void OnRemoved(SimContext context) { base.OnRemoved(context); audit.Removed = true; }
            public override void Destroy() { audit.DestroyCalls++; base.Destroy(); }
            protected override void RefreshRuntimeFromEntity()
            {
                if (audit?.Removed == true) audit.RefreshAfterRemoved++;
                base.RefreshRuntimeFromEntity();
            }
        }

        private sealed class GenericProbe : LF2Entity
        {
            private readonly Audit audit;
            internal GenericProbe(Audit audit) { this.audit = audit; }
            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
            public override void Reset() { }
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.LightWeapon;
            public override void UnregisterFromWorld() { audit.BeforeUnregister(this); base.UnregisterFromWorld(); }
            public override void OnRemoved(SimContext context) { base.OnRemoved(context); audit.Removed = true; }
            public override void Destroy() { audit.DestroyCalls++; base.Destroy(); }
            protected override void RefreshRuntimeFromEntity()
            {
                if (audit?.Removed == true) audit.RefreshAfterRemoved++;
                base.RefreshRuntimeFromEntity();
            }
        }

        private sealed class Scope : IDisposable
        {
            internal readonly Dictionary<int, LF2CharacterDataWrapper> Definitions = new Dictionary<int, LF2CharacterDataWrapper>();
            internal readonly SimulationWorld World;
            internal readonly Audit Audit = new Audit();
            internal readonly BattleParityStructuralEventBuffer Events = new BattleParityStructuralEventBuffer(400);
            internal LF2Character Holder;
            internal LF2Entity Child;
            internal RuntimeEntityHandle ChildHandle;
            internal RendererScope Presentation;
            private readonly int oid;
            private readonly List<RuntimeEntityHandle> owned = new List<RuntimeEntityHandle>();
            internal Scope(int oid, int type)
            {
                this.oid = oid;
                Definitions[oid] = Data(oid, oid == 122 || oid == 123 ? oid : type, LF2States.WeaponOnHand, new WeaponPoint());
                World = new SimulationWorld(new RuntimeCharacterConfigResolver(id => Definitions.TryGetValue(id, out var value) ? value : null));
                World.SetLogicOnlyEntityMaterialization(true);
            }
            internal LF2Character AddCharacter(int slot, bool terminal = false, int state = 0, int action = 1000, bool overlap = false)
            {
                int id = 9200 + slot;
                Definitions[id] = Data(id, 0, state, new WeaponPoint { weaponact = terminal ? action : 20,
                    kind = overlap ? 3 : 1, dvx = overlap ? 100 : 0, dvy = overlap ? -1 : 0,
                    dvz = overlap ? 9 : 0, x = 42, y = 33 });
                var character = new LF2Character { ObjectId = id, Name = "TerminalHolder" };
                character.FrameCache.Load(Definitions[id]);
                character.SetRequiredRuntimeSlot(slot);
                World.Register(character);
                character.ImmediateFrame(0);
                character.Initialize(500, 500);
                character.SwitchDir("left");
                character.FrameDelay = 100;
                Track(character);
                return character;
            }
            internal LF2Entity AddHeld(LF2Character holder, int slot)
            {
                var child = new GenericProbe(new Audit());
                BindHeld(child, holder, slot);
                return child;
            }
            internal void BindHeld(LF2Entity child, LF2Character holder, int slot)
            {
                child.ObjectId = oid;
                child.FrameCache.Load(Definitions[oid]);
                child.SetRequiredRuntimeSlot(slot);
                World.Register(child);
                child.DirectWriteHeldFramePreserveWaitCounter(20);
                if (child.Health != null)
                    child.Health.HP = 100;
                holder.Runtime.LinkState = 1;
                holder.Runtime.TargetSlotIndex = slot;
                holder.Runtime.HeldWeaponStableId = slot;
                holder.HeldWeaponReferenceInternal = child;
                child.Runtime.LinkState = -1;
                child.Runtime.HolderStableId = holder.Runtime.SlotIndex;
                Track(child);
            }
            private void Track(LF2Entity entity)
            {
                Assert.That(World.TryGetCurrentRuntimeHandleForDiagnostics(entity.Runtime.SlotIndex, entity, out RuntimeEntityHandle handle), Is.True);
                owned.Add(handle);
            }
            public void Dispose()
            {
                World.SetStructuralEventSinkForDiagnostics(null, 0, "fixture-end");
                foreach (RuntimeEntityHandle handle in owned)
                    if (World.TryResolveRuntimeHandleForDiagnostics(handle, out LF2Entity entity))
                        World.Unregister(entity);
                Presentation?.Dispose();
            }
        }

        private sealed class RendererScope : IDisposable
        {
            private static readonly FieldInfo InstanceField = typeof(MMSingleton<LF2ObjectPool>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            private static readonly FieldInfo LogicField = typeof(LF2ObjectRenderer).GetField("_logicObject", BindingFlags.Instance | BindingFlags.NonPublic);
            private readonly object previous;
            private readonly GameObject host, borrowed;
            private readonly LF2ObjectPool pool;
            private readonly LF2ObjectRenderer renderer;
            private readonly int availableBefore;
            internal RendererScope(LF2Entity child)
            {
                previous = InstanceField.GetValue(null);
                InstanceField.SetValue(null, null);
                host = new GameObject("TerminalFixturePool") { hideFlags = HideFlags.HideAndDontSave };
                pool = host.AddComponent<LF2ObjectPool>();
                if (!pool.IsRuntimeStateValidForAcceptance)
                    typeof(LF2ObjectPool).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(pool, null);
                InstanceField.SetValue(null, pool);
                borrowed = pool.Get(out renderer);
                LogicField.SetValue(renderer, child);
                typeof(LF2Entity).GetProperty(nameof(LF2Entity.Renderer)).SetValue(child, renderer);
                availableBefore = pool.AvailableObjectCountForAcceptance;
            }
            internal void AssertReturned()
            {
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
                Assert.That(pool.AvailableObjectCountForAcceptance, Is.EqualTo(availableBefore + 1));
                Assert.That(borrowed.activeSelf, Is.False);
                Assert.That(renderer.LogicObject, Is.Null);
            }
            public void Dispose()
            {
                pool.BeginBattleShutdown();
                pool.ReleaseAllActiveForShutdown(out _, out _, out _);
                pool.CompleteBattleQuiesce(out _);
                UnityEngine.Object.DestroyImmediate(host);
                InstanceField.SetValue(null, previous);
            }
        }
    }
}
#endif

#if UNITY_EDITOR
namespace NTSD.Test.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NTSD.Animation;
    using NTSD.Animation.LF2Objects;
    using NTSD.Simulation;
    using NTSD.Simulation.Ecs;
    using UnityEditor;
    using UnityEngine;

    internal static class NTSD28B6TerminalPlayProbe
    {
        private const string Request = "Temp/Goal12_Terminal_Play.request";
        private const string Result = "Temp/Goal12_Terminal_Play.result.json";
        private static SimulationTickDriver driver;
        private static bool pausing;
        private static double deadline;
        private static string consoleError;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            Application.logMessageReceived -= Log;
            Application.logMessageReceived += Log;
        }

        private static void Log(string message, string stack, LogType type)
        {
            if (File.Exists(Request) && EditorApplication.isPlaying &&
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                consoleError = message;
        }

        private static void Poll()
        {
            if (!File.Exists(Request) || !EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;
            if (!pausing)
            {
                driver.SetPaused(true);
                pausing = true;
                deadline = EditorApplication.timeSinceStartup + 20;
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics && EditorApplication.timeSinceStartup < deadline)
                return;

            SimulationWorld world = driver.World;
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            var report = new Report { startTick = driver.CurrentTickIndex, baselineObjects = world.ObjectCount,
                baselineLogicBorrowers = world.LogicReferencePool.ActiveCount,
                baselineRenderBorrowers = pool?.ActiveObjectCountForAcceptance ?? -1 };
            LF2Character holder = null;
            LF2Weapon child = null;
            RuntimeEntityHandle holderHandle = RuntimeEntityHandle.Invalid;
            RuntimeEntityHandle childHandle = RuntimeEntityHandle.Invalid;
            LF2ObjectRenderer renderer = null;
            GameObject borrowed = null;
            bool childRegistered = false;
            try
            {
                Require(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NTSD_Battle", "Wrong scene.");
                Require(!driver.DedicatedSimulationWorkerTickInFlightForDiagnostics && driver.DedicatedSimulationWorkerFailureForDiagnostics == null, "Worker gate failed.");
                Require(string.IsNullOrEmpty(consoleError), consoleError);
                Require(pool != null && pool.IsRuntimeStateValidForAcceptance, "Production renderer pool unavailable.");
                Require(world.StructuralEventSinkForServices == null, "Do not replace an existing structural observer.");
                LF2CharacterDataWrapper holderData = world.RuntimeCharacterConfigs.Resolve(7);
                LF2CharacterDataWrapper childData = world.RuntimeCharacterConfigs.Resolve(123);
                Require(holderData?.characterData != null && childData?.characterData != null, "Current DAT unavailable.");
                LF2FrameData terminal = holderData.characterData.frames.First(frame => frame.frameId == 254);
                Require(terminal.PrimaryWeaponPoint.WeaponAct == 1000 && terminal.PrimaryWeaponPoint.Dvx == 100,
                    "Current Rock Lee254 terminal witness drift.");
                report.holderSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                report.childSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(report.holderSlot + 1, 1000);
                Require(report.holderSlot >= 50 && report.childSlot > report.holderSlot, "No free scoped slots.");

                holder = new LF2Character { ObjectId = 7, Name = "Goal12_RockLee" };
                holder.ModuleInitialize();
                holder.FrameCache.Load(holderData);
                holder.SetRequiredRuntimeSlot(report.holderSlot);
                world.Register(holder);
                holder.ImmediateFrame(0);
                holder.Initialize(500, 500);
                holder.AiControlled = false;
                holder.Team = 3;
                holder.RelationTeam = 3;
                holder.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                holder.Runtime.SyncIntegerPosition();
                Require(world.TryGetCurrentRuntimeHandleForDiagnostics(report.holderSlot, holder, out holderHandle), "Holder handle missing.");

                child = (LF2Weapon)world.LogicReferencePool.Get(LF2ObjectType.Drink, 123);
                Require(child != null, "Prepared logic pool has no drink shell.");
                child.FrameCache.Load(childData);
                child.SetRequiredRuntimeSlot(report.childSlot);
                world.Register(child);
                childRegistered = true;
                child.DirectWriteHeldFramePreserveWaitCounter(20);
                child.Health.HP = 100;
                child.Runtime.WeaponFlightCounter = 31;
                child.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                child.Runtime.SyncIntegerPosition();
                Require(world.TryGetCurrentRuntimeHandleForDiagnostics(report.childSlot, child, out childHandle), "Child handle missing.");
                report.childGeneration = childHandle.Generation;

                borrowed = pool.Get(out renderer);
                Require(borrowed != null && renderer != null, "Prepared renderer pool exhausted.");
                typeof(LF2ObjectRenderer).GetField("_logicObject", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(renderer, child);
                typeof(LF2Entity).GetProperty(nameof(LF2Entity.Renderer)).SetValue(child, renderer);
                holder.AttachOpointHeldObject(child);
                report.heldAccepted = holder.GetHeldWeapon() == child && child.Runtime.LinkState < 0 &&
                    holder.Runtime.TargetSlotIndex == report.childSlot && child.Runtime.HolderStableId == report.holderSlot;
                Require(report.heldAccepted, "Current OPoint-kind2 production held relation failed.");
                holder.ImmediateFrame(254);
                report.trigger = "Current OID7 RockLee254 selected via scoped ImmediateFrame after production AttachOpointHeldObject of current OID123; real driver full tick, actual renderer and logic pool borrowers. This is an action-injected terminal witness, not physical input or the whole charge skill.";
                report.holderActionBefore = holder.Frame.N;
                report.childActionBefore = child.Frame.N;
                report.weaponAct = terminal.PrimaryWeaponPoint.WeaponAct;
                report.dvx = terminal.PrimaryWeaponPoint.Dvx;
                var sink = new Trace(world, report, childHandle);
                world.SetStructuralEventSinkForDiagnostics(sink, driver.CurrentTickIndex, "terminal-probe-start");
                ulong nativeBefore = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                ulong legacyBefore = world.Rng.CallCount;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false), "Driver rejected terminal tick.");
                report.nativeDrawsWholeTick = (int)(world.NativeRandom.CaptureScalarState().SynchronizedCalls - nativeBefore);
                report.legacyDrawsWholeTick = (int)(world.Rng.CallCount - legacyBefore);
                report.endTick = driver.CurrentTickIndex;
                report.oldHandleInvalid = !world.TryResolveRuntimeHandleForDiagnostics(childHandle, out _);
                report.rendererReturned = pool.ActiveObjectCountForAcceptance == report.baselineRenderBorrowers &&
                    !borrowed.activeSelf && renderer.LogicObject == null;
                report.logicReturned = world.LogicReferencePool.ActiveCount == report.baselineLogicBorrowers;
                Require(report.terminalRequests == 1 && report.freeEvents == 1 && report.freeDelta == 1 &&
                    report.childUnregisterDelta == 1 && report.generationReleaseDelta == 1 && report.destroyDelta == 0,
                    "Terminal structural counts differ.");
                Require(report.terminalPass == "held-refill:C09" && report.freePass == "held-refill:C09" &&
                    report.terminalTick == report.freeTick && report.terminalTick == driver.CurrentTickIndex,
                    "Terminal/free tick or pass differs.");
                Require(report.nativeDrawsTerminal == 0 && report.legacyDrawsTerminal == 0 && report.soundsTerminal == 0,
                    "Terminal produced RNG or audio.");
                Require(report.oldHandleInvalid && report.handleInvalidAtFree && report.rendererReturned && report.logicReturned,
                    "Terminal did not return its generation/renderer/logic shell.");
                Require(string.IsNullOrEmpty(consoleError), consoleError);
                report.status = "PASS";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                world.SetStructuralEventSinkForDiagnostics(null, driver.CurrentTickIndex, "terminal-probe-end");
                if (world.TryResolveRuntimeHandleForDiagnostics(childHandle, out LF2Entity liveChild))
                    world.StructuralWriter.Free(liveChild);
                if (!childRegistered && child != null)
                    world.LogicReferencePool.Release(child);
                if (borrowed != null && borrowed.activeSelf && renderer != null && renderer.LogicObject == null)
                    pool.Release(renderer);
                if (world.TryResolveRuntimeHandleForDiagnostics(holderHandle, out LF2Entity liveHolder))
                    world.Unregister(liveHolder);
                report.finalObjects = world.ObjectCount;
                report.finalLogicBorrowers = world.LogicReferencePool.ActiveCount;
                report.finalRenderBorrowers = pool?.ActiveObjectCountForAcceptance ?? -1;
                report.cleanupPassed = report.finalObjects == report.baselineObjects &&
                    report.finalLogicBorrowers == report.baselineLogicBorrowers &&
                    report.finalRenderBorrowers == report.baselineRenderBorrowers &&
                    !world.TryResolveRuntimeHandleForDiagnostics(childHandle, out _) &&
                    !world.TryResolveRuntimeHandleForDiagnostics(holderHandle, out _);
                if (!report.cleanupPassed)
                    report.status = "FAIL";
                File.WriteAllText(Result, JsonUtility.ToJson(report, true));
                File.Delete(Request);
                EditorApplication.update -= Poll;
                Application.logMessageReceived -= Log;
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private sealed class Trace : IBattleParityStructuralEventSink
        {
            private readonly SimulationWorld world;
            private readonly Report report;
            private readonly RuntimeEntityHandle childHandle;
            private BattleStructuralWriterDiagnostics before;
            private ulong native, legacy;
            private long sounds;
            internal Trace(SimulationWorld world, Report report, RuntimeEntityHandle childHandle)
            { this.world = world; this.report = report; this.childHandle = childHandle; }
            public void Record(BattleParityStructuralEvent item)
            {
                var diagnostics = world.StructuralWriterDiagnosticsForDiagnostics;
                report.events.Add(new TraceRow
                {
                    tick = item.Tick, pass = item.Pass, action = item.Action,
                    cursor = item.CursorSlot, actor = item.ActorSlot, slot = item.Slot,
                    before = item.Before, after = item.After, epoch = item.LifecycleEpoch,
                    command = diagnostics.LastCommandType.ToString(), ordinal = diagnostics.LastAuthorityOrdinal,
                    freeCount = diagnostics.FreeCount, unregisterCount = diagnostics.UnregisterCount,
                    generationReleaseCount = diagnostics.GenerationReleaseCount, destroyCount = diagnostics.DestroyCount,
                });
                if (item.Slot != report.childSlot)
                    return;
                if (item.Action == "held-terminal")
                {
                    report.terminalRequests++;
                    report.terminalPass = item.Pass;
                    report.terminalTick = item.Tick;
                    before = world.StructuralWriterDiagnosticsForDiagnostics;
                    native = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                    legacy = world.Rng.CallCount;
                    sounds = world.QueuedSoundEventCountForDiagnostics;
                    if (world.TryResolveRuntimeHandleForDiagnostics(childHandle, out LF2Entity active))
                        report.childActionAtTerminal = active.Frame.N;
                }
                if (item.Action == "free")
                {
                    report.freeEvents++;
                    report.freePass = item.Pass;
                    report.freeTick = item.Tick;
                    var after = world.StructuralWriterDiagnosticsForDiagnostics;
                    report.freeDelta = (int)(after.FreeCount - before.FreeCount);
                    report.childUnregisterDelta = (int)(after.UnregisterCount - before.UnregisterCount);
                    report.generationReleaseDelta = (int)(after.GenerationReleaseCount - before.GenerationReleaseCount);
                    report.destroyDelta = (int)(after.DestroyCount - before.DestroyCount);
                    report.nativeDrawsTerminal = (int)(world.NativeRandom.CaptureScalarState().SynchronizedCalls - native);
                    report.legacyDrawsTerminal = (int)(world.Rng.CallCount - legacy);
                    report.soundsTerminal = (int)(world.QueuedSoundEventCountForDiagnostics - sounds);
                    report.handleInvalidAtFree = !world.TryResolveRuntimeHandleForDiagnostics(childHandle, out _);
                }
            }
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }

        [Serializable]
        private sealed class Report
        {
            public string status, message, trigger, terminalPass, freePass;
            public int startTick, endTick, holderSlot, childSlot, holderActionBefore, childActionBefore, childActionAtTerminal;
            public int weaponAct, dvx, terminalRequests, freeEvents, terminalTick, freeTick;
            public uint childGeneration;
            public int freeDelta, childUnregisterDelta, generationReleaseDelta, destroyDelta;
            public int nativeDrawsTerminal, legacyDrawsTerminal, soundsTerminal, nativeDrawsWholeTick, legacyDrawsWholeTick;
            public int baselineObjects, finalObjects, baselineLogicBorrowers, finalLogicBorrowers, baselineRenderBorrowers, finalRenderBorrowers;
            public bool heldAccepted, oldHandleInvalid, handleInvalidAtFree, rendererReturned, logicReturned, cleanupPassed;
            public List<TraceRow> events = new List<TraceRow>();
        }

        [Serializable]
        private sealed class TraceRow
        {
            public int tick, cursor, actor, slot, ordinal;
            public string pass, action, before, after, command;
            public ulong epoch;
            public long freeCount, unregisterCount, generationReleaseCount, destroyCount;
        }
    }
}
#endif
