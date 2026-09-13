#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6WpointKind3ReleaseProductionEditorTests
    {
        private static IEnumerable<TestCaseData> ReleaseCases()
        {
            foreach (bool real in new[] { false, true })
            foreach (int type in new[] { 1, 2, 4, 6 })
            foreach (bool left in new[] { false, true })
            foreach (int variant in new[] { 0, 1, 2, 3, 4 })
                yield return new TestCaseData(real, type, left, variant)
                    .SetName($"Kind3_{(real ? "Real" : "Generic")}_T{type}_{(left ? "Left" : "Right")}_DV{variant}");
        }

        [TestCaseSource(nameof(ReleaseCases))]
        public void Kind3_UsesExactContinuationAndFinalWrites(
            bool real, int type, bool left, int variant)
        {
            WeaponPoint point = Point(variant);
            using (Scope scope = CreateScope(real, type, point, 0, 50, left))
            {
                AssertRelease(scope, point, type == 2 && point.dvx != 0 ? 1 : 0);
            }
        }

        [TestCase(false, 0, 399)]
        [TestCase(false, 399, 0)]
        [TestCase(true, 0, 399)]
        [TestCase(true, 399, 0)]
        public void Kind3_PreservesHistoricalSlotsCatchOwnerAndGeneration(
            bool real, int holderSlot, int childSlot)
        {
            WeaponPoint point = Point(0);
            using (Scope scope = CreateScope(real, 1, point, holderSlot, childSlot, false))
                AssertRelease(scope, point, 0);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Kind3_DamagedForceDropStillContinues(bool real)
        {
            WeaponPoint point = Point(0);
            using (Scope scope = CreateScope(real, 1, point, 0, 50, false,
                childState: LF2States.Falling))
            {
                AssertRelease(scope, point, 0, expectedLegacyDraws: 1);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NonKind3_Type2RetainsLegacyPrefix(bool real)
        {
            WeaponPoint point = Point(4);
            point.kind = 1;
            using (Scope scope = CreateScope(real, 2, point, 0, 50, false))
            {
                ulong oldCalls = scope.World.Rng.CallCount;
                ulong nativeCalls = scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls;
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.World.Rng.CallCount, Is.EqualTo(oldCalls + 1));
                Assert.That(scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls,
                    Is.EqualTo(nativeCalls));
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
            }
        }

        [TestCase(122, false)]
        [TestCase(122, true)]
        [TestCase(123, false)]
        [TestCase(123, true)]
        public void Refill_ExhaustionAloneStopsKind3(int oid, bool exhausted)
        {
            WeaponPoint point = Point(0);
            using (Scope scope = CreateScope(true, 6, point, 0, 50, false,
                holderState: 17, childOid: oid))
            {
                scope.Child.Health.HP = exhausted ? (oid == 122 ? 1 : 2) : 20;
                scope.Child.Health.HPBound = 100;
                scope.Child.Health.PP = 400;
                scope.Holder.Health.HP = 100;
                scope.Holder.Health.HPBound = 200;
                scope.Holder.Health.PP = 100;
                scope.Child.Runtime.Vz = 6.5;
                if (!exhausted)
                {
                    AssertRelease(scope, point, 0);
                    return;
                }

                var legacy = new DeterministicRng(scope.World.Rng.State);
                int expectedKick = legacy.NextInt(0, 7) - 3;
                ulong legacyBefore = scope.World.Rng.CallCount;
                ulong nativeBefore = scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls;
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls,
                    Is.EqualTo(nativeBefore), "exhaustion must not append kind3 draws");
                Assert.That(scope.World.Rng.CallCount, Is.EqualTo(legacyBefore + 1),
                    "the existing exhaustion kick keeps its one legacy draw");
                Assert.That(scope.Child.Frame.N, Is.Zero);
                Assert.That(scope.Holder.Frame.N, Is.Zero);
                Assert.That(scope.Child.Runtime.Vx, Is.EqualTo(expectedKick));
                Assert.That(scope.Child.Runtime.Vy, Is.Zero);
                Assert.That(scope.Child.Runtime.Vz, Is.EqualTo(6.5));
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Child.Runtime.HolderStableId, Is.Zero);
                Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.Zero);
                scope.World.HeldObjectProcessAll(7);
                Assert.That(scope.World.Rng.CallCount, Is.EqualTo(legacyBefore + 1));
                Assert.That(scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls,
                    Is.EqualTo(nativeBefore));
            }
        }

        private static void AssertRelease(
            Scope scope, WeaponPoint point, int prefix, int expectedLegacyDraws = 0)
        {
            var expected = new NTSD28NativeRandom();
            expected.RestoreSynchronized(scope.World.NativeRandom.CaptureSynchronizedState());
            var sites = new List<uint>();
            var bounds = new List<int>();
            var values = new List<int>();
            if (prefix != 0)
            {
                sites.Add(0x0041865Eu);
                bounds.Add(6);
                values.Add(expected.SynchronizedNext(0x0041865Eu, 6));
            }
            uint[] tailSites = { 0x00418726u, 0x0041873Au, 0x00418756u, 0x00418772u };
            int[] tailBounds = { 6, 7, 4, 5 };
            for (int i = 0; i < tailSites.Length; i++)
            {
                sites.Add(tailSites[i]);
                bounds.Add(tailBounds[i]);
                values.Add(expected.SynchronizedNext(tailSites[i], tailBounds[i]));
            }
            int action = values[prefix];
            double vx = point.dvx != 0 ? point.dvx : values[prefix + 1] - 3;
            double vy = point.dvy != 0 ? point.dvy : -values[prefix + 2];
            double vz = point.dvz != 0 ? point.dvz : values[prefix + 3] - 2;
            ulong oldCalls = scope.World.Rng.CallCount;
            ulong nativeCalls = scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls;
            Assert.That(scope.World.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                scope.ChildSlot, out RuntimeSlotTable.ReadOnlySlotView before), Is.True);
            int objectCount = scope.World.ObjectCount;
            var observer = new Recorder(scope.Holder, scope.Child);
            scope.World.NativeRandom.SetDiagnosticCallObserver(observer);

            scope.World.HeldObjectProcessAll(7);

            Assert.That(observer.Calls.Count, Is.EqualTo(prefix + 4),
                "kind3 must use the native synchronized stream, including its optional type2 prefix");
            CollectionAssert.AreEqual(sites, observer.Calls.Select(call => call.CallSite).ToArray());
            CollectionAssert.AreEqual(bounds, observer.Calls.Select(call => call.UpperBound).ToArray());
            CollectionAssert.AreEqual(values, observer.Calls.Select(call => call.Result).ToArray());
            Assert.That(observer.TailSawReleasedLinks, Is.True);
            Assert.That(scope.World.Rng.CallCount, Is.EqualTo(oldCalls + (ulong)expectedLegacyDraws));
            Assert.That(scope.Child.Frame.N, Is.EqualTo(action));
            Assert.That(scope.Child.Runtime.Vx, Is.EqualTo(vx), "authored X is not facing-flipped");
            Assert.That(scope.Child.Runtime.Vy, Is.EqualTo(vy));
            Assert.That(scope.Child.Runtime.Vz, Is.EqualTo(vz));
            if (point.dvz == 0)
            {
                Assert.That(scope.Child.Runtime.Vz, Is.InRange(-2.0, 2.0));
                Assert.That(scope.Child.Runtime.Vz, Is.EqualTo(Math.Truncate(scope.Child.Runtime.Vz)));
            }
            Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
            Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
            Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.EqualTo(scope.ChildSlot));
            Assert.That(scope.Child.Runtime.HolderStableId, Is.EqualTo(scope.HolderSlot));
            Assert.That(scope.Child.Runtime.CatchSourceSlot90, Is.EqualTo(82));
            Assert.That(scope.Child.Runtime.CaughtSlotIndex, Is.EqualTo(81));
            Assert.That(scope.Child.Runtime.CaughtDuration, Is.EqualTo(23));
            Assert.That(scope.Child.Runtime.OwnerSlotIndex, Is.EqualTo(17));
            Assert.That(typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0, Is.True);
            Assert.That(scope.World.ObjectCount, Is.EqualTo(objectCount));
            Assert.That(scope.World.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                scope.ChildSlot, out RuntimeSlotTable.ReadOnlySlotView after), Is.True);
            Assert.That(after.Claimed, Is.True);
            Assert.That(after.Entity, Is.SameAs(scope.Child));
            Assert.That(after.Generation, Is.EqualTo(before.Generation));

            scope.World.HeldObjectProcessAll(7);

            Assert.That(observer.Calls.Count, Is.EqualTo(prefix + 4),
                "the second held pass must skip the released relation");
            Assert.That(scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls,
                Is.EqualTo(nativeCalls + (ulong)prefix + 4UL));
        }

        internal static WeaponPoint Point(int variant)
        {
            return new WeaponPoint
            {
                kind = 3,
                weaponact = 20,
                cover = 1,
                x = 39,
                y = 30,
                dvx = variant == 1 || variant == 4 ? 100 : 0,
                dvy = variant == 2 || variant == 4 ? -1 : 0,
                dvz = variant == 3 || variant == 4 ? 9 : 0,
            };
        }

        private static Scope CreateScope(bool real, int type, WeaponPoint point,
            int holderSlot, int childSlot, bool left,
            int holderState = 0, int childState = LF2States.WeaponOnHand, int childOid = 9000)
        {
            LF2CharacterData holderData = Data(0, holderState, point);
            LF2CharacterData childData = Data(childOid == 122 || childOid == 123 ? childOid : type,
                childState, new WeaponPoint());
            var holderWrapper = new LF2CharacterDataWrapper(9100, holderData);
            var childWrapper = new LF2CharacterDataWrapper(childOid, childData);
            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(
                oid => oid == childOid ? childWrapper : holderWrapper));
            var holder = new LF2Character { ObjectId = 9100, Name = "Kind3Holder" };
            holder.FrameCache.Load(holderWrapper);
            holder.SetRequiredRuntimeSlot(holderSlot);
            world.Register(holder);
            holder.ImmediateFrame(0);
            holder.Initialize(500, 500);
            holder.SwitchDir(left ? "left" : "right");
            holder.Runtime.SetPosition(100, 0, 80);
            holder.Runtime.SyncIntegerPosition();
            LF2Entity child;
            if (real)
            {
                var weapon = new LF2Weapon { ObjectId = childOid, Name = "Kind3Real" };
                weapon.SetWeaponType(type);
                child = weapon;
            }
            else
            {
                child = new GenericHeld(type) { ObjectId = childOid, Name = "Kind3Generic" };
            }
            child.FrameCache.Load(childWrapper);
            child.SetRequiredRuntimeSlot(childSlot);
            world.Register(child);
            child.ImmediateFrame(20);
            if (child.Health != null)
            {
                child.Health.HP = 100;
                child.Health.HPBound = 100;
                child.Health.HP3 = 500;
            }
            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = childSlot;
            holder.Runtime.HeldWeaponStableId = childSlot;
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = holderSlot;
            child.Runtime.OwnerSlotIndex = 17;
            child.Runtime.CatchSourceSlot90 = 82;
            child.Runtime.CaughtSlotIndex = 81;
            child.Runtime.CaughtDuration = 23;
            child.Runtime.SetVelocity(12, -8, 6.5);
            return new Scope(world, holder, child, holderSlot, childSlot);
        }

        private static LF2CharacterData Data(int type, int state, WeaponPoint point)
        {
            var data = new LF2CharacterData
            {
                name = "Kind3Fixture",
                type_sub = type,
                weapon_hp = 31,
                frames = new List<LF2FrameData>(),
            };
            for (int id = 0; id <= 40; id++)
            {
                data.frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = id == 0 || id == 20 ? state : 0,
                    wait = 100,
                    next = id,
                    centerx = 39,
                    centery = 79,
                    wpoints = new List<WeaponPoint> { point },
                });
            }
            return data;
        }

        private sealed class GenericHeld : LF2Entity
        {
            private readonly int type;
            internal GenericHeld(int type) { this.type = type; }
            public override void Init(NTSD.Animation.LF2Tasks.LF2TaskBase task, LF2ObjectRenderer renderer) { }
            public override void Reset() { }
            public override LF2ObjectType ObjectTypeEnum => (LF2ObjectType)type;
            public override int GetCurrentDataObjectTypeForSimulation() => type;
        }

        internal sealed class Recorder : INTSD28NativeRandomCallObserver
        {
            private readonly LF2Entity holder;
            private readonly LF2Entity child;
            internal readonly List<NTSD28NativeSynchronizedCall> Calls =
                new List<NTSD28NativeSynchronizedCall>();
            internal bool TailSawReleasedLinks;
            internal Recorder(LF2Entity holder, LF2Entity child)
            {
                this.holder = holder;
                this.child = child;
            }
            public void OnCrtNext(NTSD28NativeCrtCall call) { }
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                Calls.Add(call);
                if (call.CallSite == 0x00418726u)
                    TailSawReleasedLinks = holder.Runtime.LinkState == 0 && child.Runtime.LinkState == 0;
            }
        }

        private sealed class Scope : IDisposable
        {
            internal readonly SimulationWorld World;
            internal readonly LF2Character Holder;
            internal readonly LF2Entity Child;
            internal readonly int HolderSlot;
            internal readonly int ChildSlot;
            internal Scope(SimulationWorld world, LF2Character holder, LF2Entity child,
                int holderSlot, int childSlot)
            {
                World = world;
                Holder = holder;
                Child = child;
                HolderSlot = holderSlot;
                ChildSlot = childSlot;
            }
            public void Dispose()
            {
                World.NativeRandom.SetDiagnosticCallObserver(null);
                World.Unregister(Child);
                World.Unregister(Holder);
            }
        }
    }
}
#endif

#if UNITY_EDITOR
namespace NTSD.Test.Editor
{
    using System.Linq;
    using NTSD.Animation;
    using NTSD.Animation.LF2Objects;
    using NTSD.Simulation;

    internal static class NTSD28B6Kind3PlayProbe
    {
        private const string Request = "Temp/Goal11_Kind3_Play.request";
        private const string Result = "Temp/Goal11_Kind3_Play.result.json";
        private static SimulationTickDriver driver;
        private static bool pausing;
        private static double deadline;
        private static string consoleError;

        [UnityEditor.InitializeOnLoadMethod]
        private static void Register()
        {
            UnityEditor.EditorApplication.update -= Poll;
            UnityEditor.EditorApplication.update += Poll;
            UnityEngine.Application.logMessageReceived -= Log;
            UnityEngine.Application.logMessageReceived += Log;
        }

        private static void Log(string message, string stack, UnityEngine.LogType type)
        {
            if (System.IO.File.Exists(Request) && UnityEditor.EditorApplication.isPlaying &&
                (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception ||
                 type == UnityEngine.LogType.Assert))
                consoleError = message;
        }

        private static void Poll()
        {
            if (!System.IO.File.Exists(Request) || !UnityEditor.EditorApplication.isPlaying ||
                UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating)
                return;
            driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;
            if (!pausing)
            {
                driver.SetPaused(true);
                pausing = true;
                deadline = UnityEditor.EditorApplication.timeSinceStartup + 20;
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                UnityEditor.EditorApplication.timeSinceStartup < deadline)
                return;

            var report = new PlayReport();
            SimulationWorld world = driver.World;
            LF2Character holder = null;
            ObservedWeapon child = null;
            BattleSlotRuntimeState originalRosterSlot = null;
            int scopedPlayer = -1;
            var originalEntities = new System.Collections.Generic.HashSet<LF2Entity>();
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity != null)
                    originalEntities.Add(entity);
            }
            report.startTick = driver.CurrentTickIndex;
            report.baselineObjects = world.ObjectCount;
            try
            {
                Require(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NTSD_Battle", "Wrong scene.");
                Require(!driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                    driver.DedicatedSimulationWorkerFailureForDiagnostics == null, "Worker gate failed.");
                Require(string.IsNullOrEmpty(consoleError), consoleError);
                LF2CharacterDataWrapper holderData = world.RuntimeCharacterConfigs.Resolve(7);
                LF2CharacterDataWrapper childData = world.RuntimeCharacterConfigs.Resolve(120);
                Require(holderData?.characterData != null && childData?.characterData != null, "Current DAT unavailable.");
                LF2FrameData authored = holderData.characterData.frames.First(frame => frame.frameId == 255);
                Require(authored.PrimaryWeaponPoint.Kind == 3 && authored.PrimaryWeaponPoint.Dvx == 100 &&
                    authored.PrimaryWeaponPoint.Dvy == -1 && authored.PrimaryWeaponPoint.Dvz == 0, "Rock Lee255 witness drift.");
                report.holderSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                report.childSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(report.holderSlot + 1, 1000);
                Require(report.holderSlot >= 50 && report.childSlot > report.holderSlot, "No free scoped slots.");
                holder = new LF2Character();
                holder.ModuleInitialize();
                holder.ObjectId = 7;
                holder.Name = "Goal11_RockLee";
                holder.FrameCache.Load(holderData);
                holder.SetRequiredRuntimeSlot(report.holderSlot);
                world.Register(holder);
                holder.ImmediateFrame(0);
                holder.Initialize(500, 500);
                holder.ClearBattleEntryInputState();
                NTSD28NativeComboStateMachine.InitializeNativeHistory(holder.Runtime);
                holder.AiControlled = false;
                holder.Team = 3;
                holder.RelationTeam = 3;
                holder.SwitchDir("left");
                holder.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                holder.Runtime.SyncIntegerPosition();
                scopedPlayer = System.Array.FindIndex(world.Runtime.Roster.Slots, slot => slot == null || !slot.Active);
                Require(scopedPlayer >= 0, "No inactive roster slot for scoped input.");
                originalRosterSlot = world.Runtime.Roster.Slots[scopedPlayer];
                world.Runtime.Roster.Slots[scopedPlayer] = new BattleSlotRuntimeState
                {
                    Active = true, IsHuman = true, CharacterId = 7, Team = 3,
                    RuntimeSlotIndex = holder.Runtime.SlotIndex, StableId = holder.Runtime.StableId,
                };
                report.playerSlot = scopedPlayer;

                child = new ObservedWeapon { ObjectId = 120, Name = "Goal11_Kunai", Holder = holder, Report = report };
                child.SetWeaponType(1);
                child.FrameCache.Load(childData);
                child.SetRequiredRuntimeSlot(report.childSlot);
                world.Register(child);
                child.ImmediateFrame(childData.characterData.frames.First(frame => frame.state == LF2States.WeaponOnGround).frameId);
                child.Health.HP = 100;
                child.Runtime.SetPosition(190, 0, world.Runtime.Stage.ZMin + 50);
                child.Runtime.SyncIntegerPosition();
                InteractionArea pickup = holderData.characterData.frames.First(frame => frame.frameId == 60)
                    .itrs.First(itr => itr.kind == 2);
                report.pickupAccepted = new LF2CharacterInteractionResolver(holder).TryApplyPreInteraction(pickup, child);
                Require(report.pickupAccepted && holder.Runtime.LinkState == 1 && child.Runtime.LinkState == -1 &&
                    holder.GetHeldWeapon() == child, "Production current-DAT ground pickup failed.");
                report.pickupAction = holder.Frame.N;
                report.trigger = "Current Lee frame60 ITR kind2 through production pickup consumer; temporary human roster binding; native Defend+Down+Attack in one standing-tick packet via current legacy carriers FrameInputSet.Attack|Down|Jump; real driver ticks. Current defend110 has hit_Da=0, so the chord is routed from standing before entering defend. Restore roster on cleanup; no physical keyboard or Scene/DAT changes.";
                world.NativeRandom.SetDiagnosticCallObserver(new Observer(world, report, holder, child));
                for (int i = 0; i < 30 && holder.Frame.N != 0; i++)
                    Tick(holder, child, report, "pickup-return", NTSD.Input.FuncKeyMask.None);
                Require(holder.Frame.N == 0 && child.Runtime.LinkState == -1, "Pickup did not return to standing with held child.");
                NTSD.Input.FuncKeyMask[] keys = { NTSD.Input.FuncKeyMask.def | NTSD.Input.FuncKeyMask.down | NTSD.Input.FuncKeyMask.att };
                foreach (NTSD.Input.FuncKeyMask key in keys)
                {
                    for (int i = 0; i < 2 && !report.released; i++)
                        Tick(holder, child, report, key.ToString(), key);
                }
                for (int i = 0; i < 5 && !report.released; i++)
                    Tick(holder, child, report, "release-wait", NTSD.Input.FuncKeyMask.None);
                Require(report.released, "Input did not reach Rock Lee255 kind3 release.");
                Require(report.calls.Count(call => call.kind3) == 4, "Kind3 did not draw exactly four times.");
                Require(string.IsNullOrEmpty(consoleError), consoleError);
                report.status = "PASS";
            }
            catch (System.Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                if (scopedPlayer >= 0)
                {
                    world.Runtime.Roster.Slots[scopedPlayer] = originalRosterSlot;
                    report.rosterRestored = ReferenceEquals(world.Runtime.Roster.Slots[scopedPlayer], originalRosterSlot);
                }
                if (child?.RegisteredWorldForSimulation == world)
                    world.Unregister(child);
                if (holder?.RegisteredWorldForSimulation == world)
                    world.Unregister(holder);
                report.endTick = driver.CurrentTickIndex;
                report.finalObjects = world.ObjectCount;
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                    if (entity != null && !originalEntities.Contains(entity))
                        report.ambientSlots.Add(slot);
                }
                report.cleanupPassed = (child == null || child.RegisteredWorldForSimulation == null) &&
                    (holder == null || holder.RegisteredWorldForSimulation == null) &&
                    world.ObjectCount == report.baselineObjects + report.ambientSlots.Count;
                if (!report.cleanupPassed)
                    report.status = "FAIL";
                System.IO.File.WriteAllText(Result, UnityEngine.JsonUtility.ToJson(report, true));
                System.IO.File.Delete(Request);
                UnityEditor.EditorApplication.update -= Poll;
                UnityEngine.Application.logMessageReceived -= Log;
                UnityEditor.EditorApplication.delayCall += UnityEditor.EditorApplication.ExitPlaymode;
            }
        }

        private static void Tick(LF2Character holder, LF2Entity child, PlayReport report, string label, NTSD.Input.FuncKeyMask key)
        {
            int tick = driver.CurrentTickIndex + 1;
            // Native bank roles are 4=attack, 5=jump, 6=defend; current legacy carriers are Jump, Defend, Attack.
            SimulationInputButtons buttons = SimulationInputButtons.None;
            if ((key & NTSD.Input.FuncKeyMask.def) != 0)
                buttons |= SimulationInputButtons.Attack;
            if ((key & NTSD.Input.FuncKeyMask.down) != 0)
                buttons |= SimulationInputButtons.Down;
            if ((key & NTSD.Input.FuncKeyMask.att) != 0)
                buttons |= SimulationInputButtons.Jump;
            var frame = new FrameInputSet(tick, new[] { new SimulationPlayerInput(report.playerSlot, buttons) });
            ulong before = driver.World.NativeRandom.CaptureScalarState().SynchronizedCalls;
            Require(driver.StepOneTick(frame, ignorePaused: true, buildPresentation: false), "Driver rejected tick.");
            report.ticks.Add(new TickRow { tick = tick, input = label, frameInputButtons = buttons.ToString(), holderAction = holder.Frame.N,
                childAction = child.Frame.N, holderLink = holder.Runtime.LinkState, childLink = child.Runtime.LinkState,
                nativeDraws = (int)(driver.World.NativeRandom.CaptureScalarState().SynchronizedCalls - before),
                vx = child.Runtime.Vx, vy = child.Runtime.Vy, vz = child.Runtime.Vz });
        }

        private sealed class ObservedWeapon : LF2Weapon
        {
            internal LF2Character Holder;
            internal PlayReport Report;
            private bool releasePending;
            public override WeaponActResult Act(LF2Entity holder, BattleWeaponPointValue point, UnityEngine.Vector3 position)
            {
                if (point.Kind == 3 && holder.Frame.N == 255)
                {
                    releasePending = true;
                    Report.releaseTick = Match.CurrentTickIndex;
                    Report.authoredX = point.Dvx;
                    Report.authoredY = point.Dvy;
                    Report.authoredZ = point.Dvz;
                    Report.releaseFacing = holder.Runtime.Dir;
                }
                return base.Act(holder, point, position);
            }
            protected override void RefreshRuntimeFromEntity()
            {
                base.RefreshRuntimeFromEntity();
                if (!releasePending || Runtime.LinkState != 0 || Report.released)
                    return;
                releasePending = false;
                var calls = Report.calls.Where(call => call.kind3).ToArray();
                Require(calls.Length == 4 && calls[0].site == "0x00418726" && calls[1].site == "0x0041873A" &&
                    calls[2].site == "0x00418756" && calls[3].site == "0x00418772", "Release call order.");
                Report.releaseAction = Frame.N;
                Report.finalX = Runtime.Vx;
                Report.finalY = Runtime.Vy;
                Report.finalZ = Runtime.Vz;
                Require(Frame.N == calls[0].value && Runtime.Vx == 100 && Runtime.Vy == -1 &&
                    Runtime.Vz == calls[3].value - 2 && Holder.Runtime.LinkState == 0 &&
                    Holder.Runtime.TargetSlotIndex == Runtime.SlotIndex && Runtime.HolderStableId == Holder.Runtime.SlotIndex,
                    "Release final authored motion or historical relation differs.");
                Report.released = true;
            }
        }

        private sealed class Observer : INTSD28NativeRandomCallObserver
        {
            private readonly SimulationWorld world;
            private readonly PlayReport report;
            private readonly LF2Entity holder, child;
            internal Observer(SimulationWorld world, PlayReport report, LF2Entity holder, LF2Entity child)
            { this.world = world; this.report = report; this.holder = holder; this.child = child; }
            public void OnCrtNext(NTSD28NativeCrtCall call) { }
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                bool kind3 = call.CallSite == 0x00418726 || call.CallSite == 0x0041873A ||
                    call.CallSite == 0x00418756 || call.CallSite == 0x00418772;
                report.calls.Add(new CallRow { tick = world.CurrentTickIndex, site = "0x" + call.CallSite.ToString("X8"),
                    bound = call.UpperBound, value = call.Result, kind3 = kind3,
                    holderLink = holder.Runtime.LinkState, childLink = child.Runtime.LinkState });
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new System.InvalidOperationException(message);
        }

        [System.Serializable]
        private sealed class PlayReport
        {
            public string status, message, trigger, releaseFacing;
            public int startTick, endTick, holderSlot, childSlot, playerSlot, pickupAction, releaseTick, releaseAction;
            public int authoredX, authoredY, authoredZ, baselineObjects, finalObjects;
            public double finalX, finalY, finalZ;
            public bool pickupAccepted, released, cleanupPassed, rosterRestored;
            public System.Collections.Generic.List<TickRow> ticks = new System.Collections.Generic.List<TickRow>();
            public System.Collections.Generic.List<CallRow> calls = new System.Collections.Generic.List<CallRow>();
            public System.Collections.Generic.List<int> ambientSlots = new System.Collections.Generic.List<int>();
        }
        [System.Serializable]
        private sealed class TickRow
        {
            public int tick, holderAction, childAction, holderLink, childLink, nativeDraws;
            public string input, frameInputButtons;
            public double vx, vy, vz;
        }
        [System.Serializable]
        private sealed class CallRow
        {
            public int tick, bound, value, holderLink, childLink;
            public string site;
            public bool kind3;
        }
    }
}
#endif
