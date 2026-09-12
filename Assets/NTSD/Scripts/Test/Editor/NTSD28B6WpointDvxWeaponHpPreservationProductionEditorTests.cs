#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6WpointDvxWeaponHpPreservationProductionEditorTests
    {
        private static IEnumerable<TestCaseData> RealCases()
        {
            foreach (int type in new[] { 1, 2, 4, 6 })
            foreach (bool left in new[] { false, true })
            foreach (bool kind3 in new[] { false, true })
                yield return new TestCaseData(type, left, kind3);
        }
        [TestCaseSource(nameof(RealCases))]
        public void RealDvx_PreservesDamagedHpExceptKind3Overlap(int type, bool left, bool kind3)
        {
            using (var scope = new Scope(true, type, left, kind3))
            {
                AssertRelease(scope, type, left, kind3);
                Assert.That(scope.Child.Runtime.WeaponFlightCounter, Is.EqualTo(kind3 ? 31 : 7),
                    "Only kind3 overlap retains the shared definition-HP callback.");
                Assert.That(((ObservedWeapon)scope.Child).CallbackCount, Is.EqualTo(kind3 ? 1 : 0));
            }
        }
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        public void GenericDvx_AlreadyPreservesHpAndLegacyType2Rng(int type)
        {
            using (var scope = new Scope(false, type, true, false))
            {
                AssertRelease(scope, type, true, false);
                Assert.That(scope.Child.Runtime.WeaponFlightCounter, Is.EqualTo(7));
            }
        }
        [TestCase(122, false)]
        [TestCase(122, true)]
        [TestCase(123, false)]
        [TestCase(123, true)]
        public void Refill_ExhaustionPriorityAndKind3ControlRemain(int oid, bool exhausted)
        {
            using (var scope = new Scope(true, 6, false, true, oid, 17))
            {
                int decrement = oid == 122 ? 1 : 2;
                scope.Child.Health.HP = exhausted ? decrement : 20;
                ulong native = scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls;
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.Child.Runtime.WeaponFlightCounter, Is.EqualTo(exhausted ? 0 : 31));
                Assert.That(((ObservedWeapon)scope.Child).CallbackCount, Is.EqualTo(exhausted ? 0 : 1));
                Assert.That(scope.Child.Health.HP, Is.EqualTo(exhausted ? 0 : 20 - decrement));
                Assert.That(scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls - native, Is.EqualTo(exhausted ? 0UL : 4UL));
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
            }
        }
        private static void AssertRelease(Scope scope, int type, bool left, bool kind3)
        {
            var legacy = new DeterministicRng(scope.World.Rng.State);
            var native = new NTSD28NativeRandom();
            native.RestoreSynchronized(scope.World.NativeRandom.CaptureSynchronizedState());
            int expectedFrame = 40;
            if (type == 2)
                expectedFrame = kind3 ? native.SynchronizedNext(0x0041865E, 6) : legacy.NextInt(0, 6);
            if (kind3)
            {
                expectedFrame = native.SynchronizedNext(0x00418726, 6);
                native.SynchronizedNext(0x0041873A, 7);
                native.SynchronizedNext(0x00418756, 4);
                native.SynchronizedNext(0x00418772, 5);
            }
            ulong legacyBefore = scope.World.Rng.CallCount;
            ulong nativeBefore = scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls;
            int releaseTickBefore = scope.Child.Runtime.ReleaseTick;
            scope.World.HeldObjectProcessAll(7);
            Assert.That(scope.Child.Frame.N, Is.EqualTo(expectedFrame));
            Assert.That(scope.Child.Runtime.Vx, Is.EqualTo(kind3 ? 70 : left ? -70 : 70));
            Assert.That(scope.Child.Runtime.Vy, Is.EqualTo(-7));
            Assert.That(scope.Child.Runtime.Vz, Is.EqualTo(kind3 ? 9 : -9));
            Assert.That(scope.World.Rng.CallCount - legacyBefore, Is.EqualTo(type == 2 && !kind3 ? 1UL : 0UL));
            Assert.That(scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls - nativeBefore, Is.EqualTo(kind3 ? (type == 2 ? 5UL : 4UL) : 0UL));
            Assert.That(scope.Child.Runtime.ReleaseTick, Is.EqualTo(releaseTickBefore));
            Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
            Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
            Assert.That(scope.Holder.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
            Assert.That(scope.Child.Runtime.OwnerSlotIndex, Is.EqualTo(17));
            Assert.That(scope.Child.Runtime.CatchSourceSlot90, Is.EqualTo(82));
            Assert.That(scope.World.FindEntityByRuntimeSlotForQuery(50), Is.SameAs(scope.Child));
        }
        private sealed class ObservedWeapon : LF2Weapon
        {
            internal int CallbackCount;
            protected override void OnThrown() { CallbackCount++; base.OnThrown(); }
        }
        private sealed class Generic : LF2Entity
        {
            private readonly int type;
            internal Generic(int type) { this.type = type; }
            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
            public override void Reset() { }
            public override LF2ObjectType ObjectTypeEnum => (LF2ObjectType)type;
            public override int GetCurrentDataObjectTypeForSimulation() => type;
        }
        private sealed class Scope : IDisposable
        {
            internal readonly SimulationWorld World;
            internal readonly LF2Character Holder;
            internal readonly LF2Entity Child;
            internal Scope(bool real, int type, bool left, bool kind3, int oid = 9000, int state = 0)
            {
                var holderData = Data(9100, 0, state, new WeaponPoint { kind = kind3 ? 3 : 1, weaponact = 20, dvx = 70, dvy = -7, dvz = 9, cover = 1 });
                var childData = Data(oid, oid == 122 || oid == 123 ? oid : type, 1001, new WeaponPoint());
                World = new SimulationWorld(new RuntimeCharacterConfigResolver(id => id == oid ? childData : holderData));
                Holder = new LF2Character { ObjectId = 9100 };
                Holder.FrameCache.Load(holderData);
                Holder.SetRequiredRuntimeSlot(0);
                World.Register(Holder);
                Holder.ImmediateFrame(0);
                Holder.Initialize(500, 500);
                Holder.SwitchDir(left ? "left" : "right");
                Holder.Runtime.KeyUp = 1;
                Holder.Runtime.KeyDown = 0;
                Holder.Runtime.SetPosition(200, 0, 100);
                Holder.Runtime.SyncIntegerPosition();
                Child = real ? (LF2Entity)new ObservedWeapon { ObjectId = oid } : new Generic(type) { ObjectId = oid };
                if (Child is LF2Weapon weapon) weapon.SetWeaponType(type);
                Child.FrameCache.Load(childData);
                Child.SetRequiredRuntimeSlot(50);
                World.Register(Child);
                Child.DirectWriteHeldFramePreserveWaitCounter(20);
                if (Child.Health != null) Child.Health.HP = 100;
                Child.Runtime.WeaponFlightCounter = 7;
                Child.Runtime.ReleaseTick = 99;
                Child.Runtime.OwnerSlotIndex = 17;
                Child.Runtime.CatchSourceSlot90 = 82;
                Child.Runtime.Vz = 6.5;
                Holder.Runtime.LinkState = 1;
                Holder.Runtime.TargetSlotIndex = 50;
                Holder.Runtime.HeldWeaponStableId = 50;
                Holder.HeldWeaponReferenceInternal = Child;
                Child.Runtime.LinkState = -1;
                Child.Runtime.HolderStableId = 0;
            }
            public void Dispose() { World.Unregister(Child); World.Unregister(Holder); }
            private static LF2CharacterDataWrapper Data(int oid, int type, int state, WeaponPoint point)
            {
                var data = new LF2CharacterData { type_sub = type, weapon_hp = 31, frames = new List<LF2FrameData>() };
                for (int id = 0; id <= 40; id++)
                    data.frames.Add(new LF2FrameData { frameId = id, state = state, wait = 100, next = id,
                        wpoints = new List<WeaponPoint> { point } });
                return new LF2CharacterDataWrapper(oid, data);
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
    using NTSD.Animation;
    using NTSD.Animation.LF2Objects;
    using NTSD.Simulation;
    using UnityEditor;
    using UnityEngine;

    internal static class NTSD28B6DvxHpPlayProbe
    {
        private const string Request = "Temp/Goal13b_Pkg2_Play.request";
        private const string Result = "Temp/Goal13b_Pkg2_Play.result.json";
        private static bool pausing;
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
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) consoleError = message;
        }
        private static void Poll()
        {
            if (!File.Exists(Request) || !EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5) return;
            if (!pausing) { driver.SetPaused(true); pausing = true; return; }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
            SimulationWorld world = driver.World;
            var report = new Report { beforeObjects = world.ObjectCount, beforeLogic = world.LogicReferencePool.ActiveCount,
                beforeRender = LF2ObjectPool.TryGetInstance()?.ActiveObjectCountForAcceptance ?? -1 };
            var owned = new List<RuntimeEntityHandle>();
            try
            {
                Require(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NTSD_Battle", "Wrong scene.");
                Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null, "Worker failure.");
                LF2CharacterDataWrapper holderData = world.RuntimeCharacterConfigs.Resolve(16);
                LF2CharacterDataWrapper childData = world.RuntimeCharacterConfigs.Resolve(120);
                LF2FrameData throwFrame = holderData.characterData.frames.First(f => f.frameId == 254);
                Require(throwFrame.PrimaryWeaponPoint.Kind == 1 && throwFrame.PrimaryWeaponPoint.WeaponAct == 40 && throwFrame.PrimaryWeaponPoint.Dvx == 100,
                    "Current Gaara254 nonkind3 witness drift.");
                int holderSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                var holder = new LF2Character { ObjectId = 16, Name = "Goal13b_Gaara" };
                holder.ModuleInitialize();
                holder.FrameCache.Load(holderData);
                holder.SetRequiredRuntimeSlot(holderSlot);
                world.Register(holder);
                Track(world, owned, holder);
                holder.ImmediateFrame(0);
                holder.Initialize(500, 500);
                holder.AiControlled = false;
                holder.Team = 3;
                holder.RelationTeam = 3;
                holder.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                holder.Runtime.SyncIntegerPosition();

                int childSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(holderSlot + 1, 1000);
                var child = new Weapon { ObjectId = 120, Report = report };
                child.SetWeaponType(1);
                child.FrameCache.Load(childData);
                child.SetRequiredRuntimeSlot(childSlot);
                world.Register(child);
                Track(world, owned, child);
                child.PrepareHealth();
                child.DirectWriteHeldFramePreserveWaitCounter(64);
                report.definitionHp = childData.characterData.weapon_hp;
                report.beforeDamageHp = child.Runtime.WeaponFlightCounter;
                Require(report.definitionHp > 1 && report.beforeDamageHp == report.definitionHp, "No positive weapon durability fixture.");
                report.damageApplied = world.DamageWriter.ApplyWeaponDamage(world, holder, child,
                    new InteractionArea { kind = 0, injury = 1 });
                report.afterDamageHp = child.Runtime.WeaponFlightCounter;
                Require(report.damageApplied && report.afterDamageHp == report.definitionHp - 1, "Production damage did not reduce durability by one.");

                // Place the damaged weapon on its declared ground frame for the production pickup consumer.
                child.DirectWriteHeldFramePreserveWaitCounter(64);
                child.Runtime.SetPosition(holder.Runtime.X, 0, holder.Runtime.Z);
                child.Runtime.SetVelocity(0, 0, 0);
                child.Runtime.SyncIntegerPosition();
                holder.ImmediateFrame(60);
                InteractionArea pickup = holder.Frame.D.itrs.First(i => i.kind == 2);
                report.pickupAccepted = new LF2CharacterInteractionResolver(holder).TryApplyPreInteraction(pickup, child);
                report.afterPickupHp = child.Runtime.WeaponFlightCounter;
                Require(report.pickupAccepted && holder.GetHeldWeapon() == child && child.Runtime.LinkState < 0,
                    "Current Gaara60 production pickup failed.");
                Require(report.afterPickupHp == report.afterDamageHp, "Pickup changed damaged weapon HP.");
                holder.ImmediateFrame(254);
                holder.FrameDelay = 1000;
                report.holderSlot = holderSlot;
                report.childSlot = childSlot;
                report.armed = true;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false), "Driver rejected DVX tick.");
                report.endTick = driver.CurrentTickIndex;
                report.afterTickHp = child.Runtime.WeaponFlightCounter;
                Require(report.releaseObserved && report.afterDvxHp == report.afterDamageHp && report.beforeDvxHp == report.afterDamageHp,
                    "Held DVX did not preserve damaged HP.");
                Require(report.callbackCount == 0 && report.releaseFrame == 40 && report.releaseLink == 0, "DVX callback/frame/relation changed.");
                Require(report.afterTickHp == report.afterDamageHp, "Whole tick changed damaged HP.");
                Require(string.IsNullOrEmpty(consoleError), consoleError);
                report.status = "PASS";
            }
            catch (Exception exception) { report.status = "FAIL"; report.message = exception.ToString(); }
            finally
            {
                foreach (RuntimeEntityHandle handle in owned)
                    if (world.TryResolveRuntimeHandleForDiagnostics(handle, out LF2Entity entity)) world.Unregister(entity);
                report.afterObjects = world.ObjectCount;
                report.afterLogic = world.LogicReferencePool.ActiveCount;
                report.afterRender = LF2ObjectPool.TryGetInstance()?.ActiveObjectCountForAcceptance ?? -1;
                report.cleanup = report.beforeObjects == report.afterObjects && report.beforeLogic == report.afterLogic && report.beforeRender == report.afterRender;
                if (!report.cleanup) report.status = "FAIL";
                File.WriteAllText(Result, JsonUtility.ToJson(report, true));
                File.Delete(Request);
                EditorApplication.update -= Poll;
                Application.logMessageReceived -= Log;
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }
        private static void Track(SimulationWorld world, List<RuntimeEntityHandle> owned, LF2Entity entity)
        {
            Require(world.TryGetCurrentRuntimeHandleForDiagnostics(entity.Runtime.SlotIndex, entity, out RuntimeEntityHandle handle), "No handle.");
            owned.Add(handle);
        }
        private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
        private sealed class Weapon : LF2Weapon
        {
            internal Report Report;
            internal void PrepareHealth() { InitializeHealth(); }
            protected override void OnThrown() { Report.callbackCount++; base.OnThrown(); }
            public override WeaponActResult Act(LF2Entity holder, BattleWeaponPointValue point, Vector3 holdpoint)
            {
                bool observe = Report.armed && point.Kind == 1 && point.Dvx != 0;
                if (observe) Report.beforeDvxHp = Runtime.WeaponFlightCounter;
                WeaponActResult result = base.Act(holder, point, holdpoint);
                if (observe)
                {
                    Report.releaseObserved = result.Thrown;
                    Report.afterDvxHp = Runtime.WeaponFlightCounter;
                    Report.releaseFrame = Frame.N;
                    Report.releaseLink = Runtime.LinkState;
                    Report.releaseTick = Match.CurrentTickIndex;
                    Report.vx = Runtime.Vx;
                    Report.vy = Runtime.Vy;
                    Report.vz = Runtime.Vz;
                }
                return result;
            }
        }
        [Serializable] private sealed class Report
        {
            public string status, message;
            public string trigger = "Current Gaara16/254 nonkind3 WP40 DVX100 with Kunai120; production damage writer injury1, reposition damaged child to current ground64, current Gaara60 ITR-kind2 pickup consumer, explicit throw action and real driver tick. No physical keyboard or collision-detection claim.";
            public int definitionHp, beforeDamageHp, afterDamageHp, afterPickupHp, beforeDvxHp, afterDvxHp, afterTickHp;
            public int holderSlot, childSlot, releaseTick, endTick, releaseFrame, releaseLink, callbackCount;
            public int beforeObjects, afterObjects, beforeLogic, afterLogic, beforeRender, afterRender;
            public double vx, vy, vz;
            public bool armed, damageApplied, pickupAccepted, releaseObserved, cleanup;
        }
    }
}
#endif
