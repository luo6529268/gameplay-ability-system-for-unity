#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6LegacyReleaseTickRetirementEditorTests
    {
        private const int Sentinel = -1;

        [Test]
        public void ProductionSources_RetireDynamicReleaseTickWriters()
        {
            string releaseFlow = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs");
            string heldWriter = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs");

            Assert.That(Count(releaseFlow, "Runtime.ReleaseTick ="), Is.Zero);
            Assert.That(Count(heldWriter, "Runtime.ReleaseTick ="), Is.Zero);
            Assert.That(releaseFlow, Does.Not.Contain("CurrentTickIndex"));
            Assert.That(heldWriter, Does.Not.Contain("CurrentTickIndex"));
            Assert.That(releaseFlow, Does.Contain("bool stampReleaseTick = false"));
            Assert.That(heldWriter, Does.Contain("bool stampReleaseTick = false"));
        }

        [Test]
        public void ReservedCarrier_CopyFingerprintChecksumAndParityStructureRemain()
        {
            var sourceRuntime = new NTSDEntityRuntime();
            var destinationRuntime = new NTSDEntityRuntime();
            sourceRuntime.ReleaseTick = -1;

            Assert.That(sourceRuntime.TryCopyCanonicalStateTo(destinationRuntime), Is.True);
            Assert.That(destinationRuntime.ReleaseTick, Is.EqualTo(-1));

            BattleRuntimeFingerprint before = BattleRuntimeFingerprint.Compute(sourceRuntime);
            sourceRuntime.ReleaseTick = 23;
            BattleRuntimeFingerprint after = BattleRuntimeFingerprint.Compute(sourceRuntime);
            Assert.That(after.Equals(before), Is.False);

            sourceRuntime.Reset();
            Assert.That(sourceRuntime.ReleaseTick, Is.EqualTo(-1));

            string runtime = Source(
                "Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs");
            string ecs = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs");
            string checksum = Source(
                "Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs");
            string parity = Source(
                "Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs");

            Assert.That(runtime, Does.Contain("public int ReleaseTick = -1;"));
            Assert.That(runtime, Does.Contain("destination.ReleaseTick = ReleaseTick;"));
            Assert.That(runtime, Does.Contain("ReleaseTick = -1;"));
            Assert.That(ecs, Does.Contain("hash.Add(runtime.ReleaseTick);"));
            Assert.That(checksum, Does.Contain(
                "builder.AddInt32(runtime?.ReleaseTick ?? -1);"));
            Assert.That(parity, Does.Contain(
                "(\"releaseTick\", runtime?.ReleaseTick ?? -1)"));
        }

        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(4, false)]
        [TestCase(6, false)]
        public void RealDvx_ProductionPath_PreservesReleaseTickAndRelation(
            int type,
            bool left)
        {
            using (Scope scope = CreateScope(
                       real: true,
                       type: type,
                       kind3: false,
                       left: left))
            {
                scope.World.HeldObjectProcessAll(7);

                Assert.That(scope.Child.Runtime.ReleaseTick, Is.EqualTo(Sentinel));
                Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Holder.Runtime.HeldWeaponStableId, Is.EqualTo(-1));
                Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.EqualTo(scope.ChildSlot));
                Assert.That(scope.Child.Runtime.HolderStableId, Is.EqualTo(scope.HolderSlot));
                Assert.That(scope.Child.Runtime.Vy, Is.EqualTo(-7));
                Assert.That(scope.Child.Runtime.Vz, Is.EqualTo(-9));
                Assert.That(scope.Child.Runtime.Vx, Is.EqualTo(left ? -70 : 70));
            }
        }

        [Test]
        public void GenericKind3_ProductionPath_PreservesReleaseTickAndRelation()
        {
            using (Scope scope = CreateScope(
                       real: false,
                       type: 3,
                       kind3: true,
                       left: false,
                       childOid: 213))
            {
                scope.World.HeldObjectProcessAll(7);

                Assert.That(scope.Child.Runtime.ReleaseTick, Is.EqualTo(Sentinel));
                Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.EqualTo(scope.ChildSlot));
                Assert.That(scope.Child.Runtime.HolderStableId, Is.EqualTo(scope.HolderSlot));
                Assert.That(scope.Child.Runtime.Vx, Is.EqualTo(70));
                Assert.That(scope.Child.Runtime.Vy, Is.EqualTo(-7));
                Assert.That(scope.Child.Runtime.Vz, Is.EqualTo(9));
            }
        }

        [Test]
        public void RealKind3_ProductionPath_PreservesReleaseTickAndRelation()
        {
            using (Scope scope = CreateScope(
                       real: true,
                       type: 6,
                       kind3: true,
                       left: false))
            {
                scope.World.HeldObjectProcessAll(7);

                Assert.That(scope.Child.Runtime.ReleaseTick, Is.EqualTo(Sentinel));
                Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.EqualTo(scope.ChildSlot));
                Assert.That(scope.Child.Runtime.HolderStableId, Is.EqualTo(scope.HolderSlot));
                Assert.That(scope.Child.Runtime.Vx, Is.EqualTo(70));
                Assert.That(scope.Child.Runtime.Vy, Is.EqualTo(-7));
                Assert.That(scope.Child.Runtime.Vz, Is.EqualTo(9));
            }
        }

        [TestCase(122, 1)]
        [TestCase(123, 2)]
        public void RefillConsume_ProductionPath_PreservesReleaseTickAndExhaustion(
            int objectId,
            int decrement)
        {
            using (Scope scope = CreateScope(
                       real: true,
                       type: 6,
                       kind3: true,
                       left: false,
                       childOid: objectId,
                       holderState: 17))
            {
                scope.Child.Health.HP = decrement;
                scope.Child.Health.HPBound = 100;
                scope.Child.Health.PP = 400;
                scope.Holder.Health.HP = 100;
                scope.Holder.Health.HPBound = 200;
                scope.Holder.Health.PP = 100;

                ulong legacyCalls = scope.World.Rng.CallCount;
                scope.World.HeldObjectProcessAll(7);

                Assert.That(scope.Child.Runtime.ReleaseTick, Is.EqualTo(Sentinel));
                Assert.That(scope.Child.Health.HP, Is.Zero);
                Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.Zero);
                Assert.That(scope.Child.Runtime.HolderStableId, Is.Zero);
                Assert.That(scope.World.Rng.CallCount, Is.EqualTo(legacyCalls + 1));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DamagedFrame_ProductionPath_PreservesReleaseTick(bool real)
        {
            using (Scope scope = CreateScope(
                       real: real,
                       type: 3,
                       kind3: false,
                       left: false,
                       childState: LF2States.Falling))
            {
                scope.Holder.Runtime.Vx = 9;
                scope.Holder.Runtime.Vy = -5;
                scope.Holder.Runtime.Vz = 2;
                scope.World.HeldObjectProcessAll(7);

                Assert.That(scope.Child.Runtime.ReleaseTick, Is.EqualTo(Sentinel));
                Assert.That(scope.Holder.Runtime.LinkState, Is.Zero);
                Assert.That(scope.Child.Runtime.LinkState, Is.Zero);
            }
        }

        [Test]
        public void NoRelease_ProductionPath_PreservesReleaseTickAndHeldRelation()
        {
            using (Scope scope = CreateScope(
                       real: false,
                       type: 3,
                       kind3: false,
                       left: false,
                       dvx: 0))
            {
                scope.World.HeldObjectProcessAll(7);

                Assert.That(scope.Child.Runtime.ReleaseTick, Is.EqualTo(Sentinel));
                Assert.That(scope.Holder.Runtime.LinkState, Is.EqualTo(1));
                Assert.That(scope.Child.Runtime.LinkState, Is.EqualTo(-1));
                Assert.That(scope.Holder.Runtime.TargetSlotIndex, Is.EqualTo(scope.ChildSlot));
                Assert.That(scope.Child.Runtime.HolderStableId, Is.EqualTo(scope.HolderSlot));
            }
        }

        private static Scope CreateScope(
            bool real,
            int type,
            bool kind3,
            bool left,
            int childOid = 9000,
            int holderState = 0,
            int childState = LF2States.WeaponOnHand,
            int dvx = 70)
        {
            WeaponPoint holderPoint = new WeaponPoint
            {
                kind = kind3 ? 3 : 1,
                weaponact = 20,
                dvx = dvx,
                dvy = -7,
                dvz = 9,
                cover = 1,
            };
            LF2CharacterDataWrapper holderData = Data(9100, 0, holderState, holderPoint);
            LF2CharacterDataWrapper childData = Data(
                childOid,
                childOid == 122 || childOid == 123 ? childOid : type,
                childState,
                new WeaponPoint());

            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(
                id => id == childOid ? childData : holderData));
            var holder = new LF2Character { ObjectId = 9100 };
            holder.FrameCache.Load(holderData);
            holder.SetRequiredRuntimeSlot(7);
            world.Register(holder);
            holder.ImmediateFrame(0);
            holder.Initialize(500, 500);
            holder.SwitchDir(left ? "left" : "right");
            holder.Runtime.KeyUp = 1;
            holder.Runtime.KeyDown = 0;
            holder.Runtime.SetPosition(200, 0, 100);
            holder.Runtime.SyncIntegerPosition();

            LF2Entity child = real
                ? (LF2Entity)new LF2Weapon { ObjectId = childOid }
                : new GenericHeld(type) { ObjectId = childOid };
            if (child is LF2Weapon weapon)
                weapon.SetWeaponType(type);
            child.FrameCache.Load(childData);
            child.SetRequiredRuntimeSlot(50);
            world.Register(child);
            child.DirectWriteHeldFramePreserveWaitCounter(20);
            if (child.Health != null)
                child.Health.HP = 100;
            child.Runtime.ReleaseTick = Sentinel;
            child.Runtime.WeaponFlightCounter = 7;
            child.Runtime.OwnerSlotIndex = 17;
            child.Runtime.CatchSourceSlot90 = 82;
            child.Runtime.Vz = 6.5;

            holder.Runtime.LinkState = 1;
            holder.Runtime.TargetSlotIndex = 50;
            holder.Runtime.HeldWeaponStableId = 50;
            holder.HeldWeaponReferenceInternal = child;
            child.Runtime.LinkState = -1;
            child.Runtime.HolderStableId = 7;
            return new Scope(world, holder, child, 7, 50);
        }

        private static LF2CharacterDataWrapper Data(
            int objectId,
            int type,
            int state,
            WeaponPoint point)
        {
            var data = new LF2CharacterData
            {
                type_sub = type,
                weapon_hp = 31,
                frames = new List<LF2FrameData>(),
            };
            for (int id = 0; id <= 40; id++)
            {
                data.frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = state,
                    wait = 100,
                    next = id,
                    wpoints = new List<WeaponPoint> { point },
                });
            }
            return new LF2CharacterDataWrapper(objectId, data);
        }

        private static string Source(string relativePath)
        {
            string root = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName;
            return File.ReadAllText(Path.Combine(root ?? string.Empty, relativePath));
        }

        private static int Count(string source, string value)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }
            return count;
        }

        private sealed class GenericHeld : LF2Entity
        {
            private readonly int type;

            internal GenericHeld(int type)
            {
                this.type = type;
            }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
            public override void Reset() { Runtime.Reset(); }
            public override LF2ObjectType ObjectTypeEnum => (LF2ObjectType)type;
            public override int GetCurrentDataObjectTypeForSimulation() => type;
        }

        private sealed class Scope : IDisposable
        {
            internal readonly SimulationWorld World;
            internal readonly LF2Character Holder;
            internal readonly LF2Entity Child;
            internal readonly int HolderSlot;
            internal readonly int ChildSlot;

            internal Scope(
                SimulationWorld world,
                LF2Character holder,
                LF2Entity child,
                int holderSlot,
                int childSlot)
            {
                World = world;
                Holder = holder;
                Child = child;
                HolderSlot = holderSlot;
                ChildSlot = childSlot;
            }

            public void Dispose()
            {
                World.Unregister(Child);
                World.Unregister(Holder);
            }
        }
    }
}
namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class Goal20ReleaseTickCurrentPlayProbe
    {
        private const string Request = "Temp/Goal20_R4_CurrentPlay.request";
        static Goal20ReleaseTickCurrentPlayProbe() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            if (!File.Exists(Request) || !EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            string phase = File.ReadAllText(Request).Trim();
            if (phase != "RED" && phase != "GREEN") return;
            var driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
            File.WriteAllText(Request, "running");
            var world = driver.World;
            var report = new Report { phase = phase, tick = driver.CurrentTickIndex, beforeObjects = world.ObjectCount };
            var random = world.NativeRandom.CaptureState();
            uint seed = world.NativeRandom.CaptureScalarState().TableSeed;
            try
            {
                Run(world, report, 16, 254, 120, 1);
                Run(world, report, 7, 255, 120, 1);
                Run(world, report, 51, 396, 213, 3);
                Run(world, report, 51, 399, 213, 3);
                if (phase == "GREEN")
                    NTSD28B6LegacyWeaponStateRetirementProductionEditorTests.RunCurrentPlayWitness(world);
                bool released = report.rows.All(r => r.holderLinkAfter == 0 && r.childLinkAfter == 0);
                bool reserved = report.rows.All(r => r.releaseTickAfter == -1);
                bool legacy = report.rows.All(r => r.releaseTickBefore == -1 && r.releaseTickAfter >= 0);
                report.status = released && (phase == "RED" ? legacy : reserved) ? (phase == "RED" ? "RED_EXPECTED" : "PASS") : "FAIL";
            }
            catch (Exception e) { report.status = "FAIL"; report.error = e.ToString(); }
            finally
            {
                world.NativeRandom.ResetFromSeed(seed); world.NativeRandom.Restore(random);
                report.afterObjects = world.ObjectCount;
                if (report.afterObjects != report.beforeObjects) { report.status = "FAIL"; report.error += " Object count mismatch."; }
                File.WriteAllText("Temp/Goal20_R4_CurrentPlay_" + phase + ".json", JsonUtility.ToJson(report, true));
                File.WriteAllText(Request, "done");
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private static void Run(SimulationWorld world, Report report, int sourceOid, int action, int childOid, int type)
        {
            var source = world.RuntimeCharacterConfigs.Resolve(sourceOid);
            var target = world.RuntimeCharacterConfigs.Resolve(childOid);
            var frame = source.characterData.frames.First(f => f.frameId == action);
            WeaponPoint point = frame.wpoints[0];
            int initialAction = type == 3 ? 0 : target.characterData.frames.First(f => f.state == 1001).frameId;
            Assert.That(target.characterData.frames.Any(f => f.frameId == point.weaponact), Is.True);
            int holderSlot = Enumerable.Range(0, 20).First(i => world.FindEntityByRuntimeSlotForQuery(i) == null);
            int childSlot = Enumerable.Range(20, 380).Reverse().First(i => world.FindEntityByRuntimeSlotForQuery(i) == null);
            var holder = new LF2Character { ObjectId = sourceOid, Name = "Goal20R4CurrentHolder" };
            LF2Entity child = type == 3 ? (LF2Entity)new LF2SpecialAttack { ObjectId = childOid } : new LF2Weapon { ObjectId = childOid };
            bool holderRegistered = false, childRegistered = false;
            try
            {
                holder.ModuleInitialize(); holder.FrameCache.Load(source);
                holder.SetRequiredRuntimeSlot(holderSlot); world.Register(holder); holderRegistered = true;
                holder.Initialize(500, 500); holder.ImmediateFrame(action); holder.SwitchDir("right");
                if (child is LF2Weapon weapon) weapon.SetWeaponType(type);
                child.FrameCache.Load(target); child.SetRequiredRuntimeSlot(childSlot); world.Register(child); childRegistered = true;
                child.ImmediateFrame(initialAction); child.Health.HP = 100; child.Health.HPBound = 100;
                holder.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50); holder.Runtime.SyncIntegerPosition();
                child.Runtime.SetPosition(200, -10, world.Runtime.Stage.ZMin + 50); child.Runtime.SyncIntegerPosition();
                holder.AttachOpointHeldObject(child);
                holder.FrameDelay = child.FrameDelay = 1000; holder.AttackingCounter = child.AttackingCounter = 9;
                var row = new Row { sourceOid = sourceOid, sourceAction = action, sourceState = frame.state, childOid = childOid,
                    initialAction = initialAction, kind = point.kind, weaponAct = point.weaponact, dvx = point.dvx, dvy = point.dvy, dvz = point.dvz,
                    releaseTickBefore = child.Runtime.ReleaseTick };
                world.NativeRandom.ResetFromSeed(424242);
                world.HeldObjectProcessAll(world.CurrentTickIndex);
                row.releaseTickAfter = child.Runtime.ReleaseTick; row.holderLinkAfter = holder.Runtime.LinkState; row.childLinkAfter = child.Runtime.LinkState;
                row.childActionAfter = child.Frame.N; row.vx = child.Runtime.Vx; row.vy = child.Runtime.Vy; row.vz = child.Runtime.Vz;
                row.randomCalls = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                report.rows.Add(row);
            }
            finally
            {
                if (childRegistered) world.Unregister(child);
                if (holderRegistered) world.Unregister(holder);
            }
        }
        [Serializable] private sealed class Report
        {
            public string phase, status, error;
            public int tick, beforeObjects, afterObjects;
            public List<Row> rows = new List<Row>();
            public string boundary = "Current runtime-config Gaara16/254, RockLee7/255, Sasori51/396,399; production AttachOpointHeldObject setup and HeldObjectProcessAll in paused real NTSD_Battle world. Seed424242 per case; no physical-input or full-skill claim.";
        }
        [Serializable] private sealed class Row
        {
            public int sourceOid, sourceAction, sourceState, childOid, initialAction, kind, weaponAct, dvx, dvy, dvz;
            public int releaseTickBefore, releaseTickAfter, holderLinkAfter, childLinkAfter, childActionAfter;
            public double vx, vy, vz;
            public ulong randomCalls;
        }
    }
}
#endif
