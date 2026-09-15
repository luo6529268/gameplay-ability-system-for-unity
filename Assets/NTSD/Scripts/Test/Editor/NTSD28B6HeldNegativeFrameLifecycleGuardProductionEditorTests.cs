#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6HeldNegativeFrameLifecycleGuardProductionEditorTests
    {
        [TestCase(false, -888)]
        [TestCase(true, -888)]
        [TestCase(false, 857)]
        [TestCase(true, 857)]
        [TestCase(false, 900)]
        [TestCase(true, 900)]
        public void Held_RawActionSurvivesFullTick(bool real, int action)
        {
            using (var scope = new Scope(real, action))
            {
                var before = scope.World.StructuralWriterDiagnosticsForDiagnostics;
                new NTSDBattleTickSystem(scope.World).RunReleaseTick(41, buildPresentation: false);
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.Handle, out LF2Entity child), Is.True,
                    "Held action must remain active throughout its relation, including C25 frame exit.");
                Assert.That(child.Frame.N, Is.EqualTo(action));
                Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
                Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.EqualTo(50));
                Assert.That(scope.World.StructuralWriterDiagnosticsForDiagnostics.FreeCount, Is.EqualTo(before.FreeCount));
            }
        }

        [TestCase(false, -888, false)]
        [TestCase(true, -888, false)]
        [TestCase(false, -888, true)]
        [TestCase(true, -888, true)]
        [TestCase(false, 857, false)]
        [TestCase(true, 857, false)]
        public void OrdinaryOrReleased_UsesNativeActionLifetime(bool real, int action, bool released)
        {
            using (var scope = new Scope(real, action))
            {
                if (released)
                {
                    scope.Holder.Frame.D.wpoints[0].weaponact = 20;
                    scope.Holder.Frame.D.wpoints[0].kind = 3;
                    scope.World.HeldObjectProcessAll(7);
                    Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
                }
                else
                {
                    scope.Holder.Runtime.LinkState = 0;
                    scope.Holder.Runtime.TargetSlotIndex = -1;
                    scope.Child.Runtime.LinkState = 0;
                }
                scope.Child.FrameDelay = 0;
                scope.Child.DirectWriteHeldFramePreserveWaitCounter(action);
                var before = scope.World.StructuralWriterDiagnosticsForDiagnostics;
                new NTSDBattleTickSystem(scope.World).RunReleaseTick(41, buildPresentation: false);
                bool terminal = action < 0 || action >= 999;
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.Handle, out _), Is.EqualTo(!terminal));
                Assert.That(scope.World.StructuralWriterDiagnosticsForDiagnostics.FreeCount - before.FreeCount, Is.EqualTo(terminal ? 1 : 0));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OrdinaryNegativeActionWaitsWhileFrameDelayBlocksC25(bool real)
        {
            using (var scope = new Scope(real, -888))
            {
                scope.Holder.Runtime.LinkState = 0;
                scope.Holder.Runtime.TargetSlotIndex = -1;
                scope.Child.Runtime.LinkState = 0;
                scope.Child.FrameDelay = 100;
                scope.Child.DirectWriteHeldFramePreserveWaitCounter(-888);
                new NTSDBattleTickSystem(scope.World).RunReleaseTick(41, buildPresentation: false);
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.Handle, out _), Is.True);
                Assert.That(scope.Child.Frame.N, Is.EqualTo(-888));
                Assert.That(scope.Child.Runtime.NativeLifecycleResolutionPending, Is.False);
            }
        }

        [TestCase("missing")]
        [TestCase("out-of-range")]
        [TestCase("mismatch")]
        public void InvalidRelation_StillReportsBothHeldPassesAndPreserves(string kind)
        {
            using (var scope = new Scope(true, 20))
            {
                if (kind == "missing") scope.Child.Runtime.HolderStableId = 49;
                if (kind == "out-of-range") scope.Child.Runtime.HolderStableId = -1;
                if (kind == "mismatch") scope.Holder.Runtime.TargetSlotIndex = 51;
                int holderSlot = scope.Child.Runtime.HolderStableId;
                long before = scope.World.HeldInvalidReciprocalFailureCountForDiagnostics;
                new NTSDBattleTickSystem(scope.World).RunReleaseTick(41, buildPresentation: false);
                Assert.That(scope.World.HeldInvalidReciprocalFailureCountForDiagnostics - before, Is.EqualTo(2));
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.Handle, out LF2Entity child), Is.True);
                Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
                Assert.That(child.Runtime.HolderStableId, Is.EqualTo(holderSlot));
            }
        }

        [TestCase(1100)]
        [TestCase(1200)]
        public void UnmarkedHighActionDoesNotTriggerLegacyFrameGroupExit(int action)
        {
            using (var scope = new Scope(true, action))
            {
                scope.Child.DirectWriteHeldFramePreserveWaitCounter(action);
                var module = new BattleLateEntityLifecycleModule(scope.World);
                var method = typeof(BattleLateEntityLifecycleModule).GetMethod("HandleFrameTickExit", BindingFlags.NonPublic | BindingFlags.Instance);
                scope.Child.HitStun = 17;
                Assert.That(scope.Child.Runtime.NativeLifecycleResolutionPending, Is.False);
                Assert.That(method.Invoke(module, new object[] { scope.Child, null }), Is.EqualTo(false));
                Assert.That(scope.Child.Frame.N, Is.EqualTo(action));
                Assert.That(scope.Child.HitStun, Is.EqualTo(17));
                Assert.That(scope.World.TryResolveRuntimeHandleForDiagnostics(scope.Handle, out _), Is.True);
            }
        }

        private sealed class Generic : LF2Entity
        {
            public Generic() { Trans = new FrameTransistor(this); }
            public override void SimFrameTick(int tickIndex) { RunNativeC25FrameBodyForWorldPass(); }
            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
            public override void Reset() { }
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.LightWeapon;
        }
        private sealed class Scope : IDisposable
        {
            internal readonly SimulationWorld World;
            internal readonly LF2Character Holder;
            internal readonly LF2Entity Child;
            internal readonly RuntimeEntityHandle Handle;
            private readonly RuntimeEntityHandle holderHandle;
            internal Scope(bool real, int action)
            {
                var holderData = Data(9001, 0, 0, action);
                var childData = Data(9002, 1, 1001, 20);
                World = new SimulationWorld(new RuntimeCharacterConfigResolver(id => id == 9001 ? holderData : id == 9002 ? childData : null));
                World.SetLogicOnlyEntityMaterialization(true);
                Holder = new LF2Character { ObjectId = 9001 };
                Holder.FrameCache.Load(holderData);
                Holder.SetRequiredRuntimeSlot(0);
                World.Register(Holder);
                Holder.ImmediateFrame(0);
                Holder.Initialize(500, 500);
                Holder.FrameDelay = 1000;
                Holder.Runtime.SetPosition(200, 0, 100);
                Holder.Runtime.SyncIntegerPosition();
                Child = real ? (LF2Entity)new LF2Weapon { ObjectId = 9002 } : new Generic { ObjectId = 9002 };
                if (Child is LF2Weapon weapon) weapon.SetWeaponType(1);
                Child.FrameCache.Load(childData);
                Child.SetRequiredRuntimeSlot(50);
                World.Register(Child);
                Child.DirectWriteHeldFramePreserveWaitCounter(20);
                if (Child.Health != null) Child.Health.HP = 100;
                Child.Runtime.WeaponFlightCounter = 31;
                Child.FrameDelay = 100;
                Child.Runtime.SetPosition(200, -10, 100);
                Child.Runtime.SyncIntegerPosition();
                Holder.Runtime.LinkState = 1;
                Holder.Runtime.TargetSlotIndex = 50;
                Holder.Runtime.HeldWeaponStableId = 50;
                Holder.HeldWeaponReferenceInternal = Child;
                Child.Runtime.LinkState = -1;
                Child.Runtime.HolderStableId = 0;
                World.TryGetCurrentRuntimeHandleForDiagnostics(50, Child, out Handle);
                World.TryGetCurrentRuntimeHandleForDiagnostics(0, Holder, out holderHandle);
            }
            public void Dispose()
            {
                if (World.TryResolveRuntimeHandleForDiagnostics(Handle, out LF2Entity child)) World.Unregister(child);
                if (World.TryResolveRuntimeHandleForDiagnostics(holderHandle, out LF2Entity holder)) World.Unregister(holder);
            }
            private static LF2CharacterDataWrapper Data(int oid, int type, int state, int action)
            {
                var data = new LF2CharacterData { type_sub = type, weapon_hp = 31, frames = new List<LF2FrameData>() };
                foreach (int id in new[] { 0, 20, 40 })
                    data.frames.Add(new LF2FrameData { frameId = id, state = state, wait = 100, next = id,
                        wpoints = new List<WeaponPoint> { new WeaponPoint { kind = 1, weaponact = action } } });
                return new LF2CharacterDataWrapper(oid, data);
            }
        }
    }
}
#endif
