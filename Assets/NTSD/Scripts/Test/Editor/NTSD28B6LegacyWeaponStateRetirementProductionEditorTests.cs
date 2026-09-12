#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6LegacyWeaponStateRetirementProductionEditorTests
    {
        private const string WeaponDatRelativePath =
            "Assets/NTSD/Config/chars/weapon9.dat";

        private const string FrameLogicRelativePath =
            "Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs";

        private const string HeldStateRelativePath =
            "Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs";

        private const string WeaponBaseRelativePath =
            "Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs";

        private const string FocusedReportRelativePath =
            "Temp/Goal20_R3_FocusedWitness.json";

        [Test]
        public void ProductionSources_RetireParallelWeaponStateReaderAndWriters()
        {
            string frameLogic = ReadProjectFile(FrameLogicRelativePath);
            string heldState = ReadProjectFile(HeldStateRelativePath);
            string weaponBase = ReadProjectFile(WeaponBaseRelativePath);

            StringAssert.DoesNotContain(
                "GetRuntimeWeaponState",
                frameLogic,
                "frame logic must consume actual current-frame state");
            StringAssert.DoesNotContain(
                "ResolveRuntimeWeaponState",
                weaponBase,
                "the parallel WeaponState resolver is retired");
            Assert.That(
                Regex.Matches(frameLogic, @"Runtime\.WeaponState\s*=").Count,
                Is.Zero,
                "frame logic must not mutate the reserved WeaponState carrier");
            Assert.That(
                Regex.Matches(heldState, @"Runtime\.WeaponState\s*=").Count,
                Is.Zero,
                "held/drop/throw logic must not mutate the reserved WeaponState carrier");
            Assert.That(
                Regex.Matches(weaponBase, @"Runtime\.WeaponState\s*=").Count,
                Is.EqualTo(1),
                "WeaponState remains only as the Reset-time reserved zero");
            StringAssert.Contains("Runtime.WeaponState = 0;", weaponBase);
        }

        [Test]
        public void Oid124Action40Through55_KeepActualStateAndHitFaLoop()
        {
            LF2CharacterData data = LoadWeapon9Data();
            var observed = new List<FrameObservation>();

            for (int frameId = 40; frameId <= 55; frameId++)
            {
                LF2FrameData frame = data.frames.FirstOrDefault(
                    value => value != null && value.frameId == frameId);
                Assert.That(frame, Is.Not.Null, $"weapon9.dat is missing frame {frameId}");
                observed.Add(new FrameObservation
                {
                    frame = frameId,
                    state = frame.state,
                    hitFa = frame.hit_Fa,
                    next = frame.next,
                });
            }

            WriteJson(
                "Temp/Goal20_R3_Oid124FrameLoop.json",
                new FrameLoopReport
                {
                    oid = 124,
                    source = WeaponDatRelativePath,
                    frames = observed.ToArray(),
                });

            for (int index = 0; index < observed.Count; index++)
            {
                Assert.That(
                    observed[index].state,
                    Is.EqualTo(LF2States.WeaponThrowing),
                    $"OID124 action {observed[index].frame} must use actual state1002");
                Assert.That(
                    observed[index].hitFa,
                    Is.EqualTo(12),
                    $"OID124 action {observed[index].frame} must retain hit_Fa=12");
                int expectedNext = observed[index].frame == 55
                    ? 40
                    : observed[index].frame + 1;
                Assert.That(
                    observed[index].next,
                    Is.EqualTo(expectedNext),
                    $"OID124 action {observed[index].frame} must preserve its current-frame loop");
            }
        }

        [Test]
        public void Oid124HitFa12_UsesActualFrameStateWithoutCarrierPrelude()
        {
            SimulationWorld world = new SimulationWorld();
            world.NativeRandom.ResetFromSeed(424242);
            LF2CharacterData weaponData = LoadWeapon9Data();
            LF2Weapon weapon = CreateWeapon(weaponData, frameId: 40, weaponType: 4);
            LF2Character target = CreateTarget();
            var observations = new List<RuntimeObservation>();

            weapon.SetRequiredRuntimeSlot(0);
            target.SetRequiredRuntimeSlot(1);
            weapon.Team = 1;
            weapon.RelationTeam = 1;
            target.Team = 2;
            target.RelationTeam = 2;
            weapon.ObjectAiTargetSlot3F8 = 1;
            weapon.Runtime.SetPosition(200.0, -100.0, 0.0);
            target.Runtime.SetPosition(1000.0, -60.0, 0.0);
            weapon.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            weapon.Runtime.Vx = 30.0;
            weapon.Runtime.WeaponState = 0;

            world.Register(weapon);
            world.Register(target);
            try
            {
                for (int tick = 1; tick <= 2; tick++)
                {
                    weapon.RunFrameLogicBeforeAdvance();
                    observations.Add(new RuntimeObservation
                    {
                        tick = tick,
                        frame = weapon.Frame.N,
                        state = weapon.Frame.D?.state ?? -1,
                        hitFa = weapon.Frame.D?.hit_Fa ?? -1,
                        reservedWeaponState = weapon.Runtime.WeaponState,
                        vx = weapon.Runtime.Vx,
                        vy = weapon.Runtime.Vy,
                        vz = weapon.Runtime.Vz,
                        targetSlot = weapon.ObjectAiTargetSlot3F8,
                        synchronizedRngCalls = world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                        checksum = world.CaptureRuntimeChecksum64(tick, null).ToString("X16"),
                    });
                }
            }
            finally
            {
                world.Unregister(target);
                world.Unregister(weapon);
            }

            string firstDifference = FindFirstDifference(observations);
            WriteJson(
                FocusedReportRelativePath,
                new FocusedWitnessReport
                {
                    status = string.IsNullOrEmpty(firstDifference) ? "PASS" : "RED",
                    source = WeaponDatRelativePath,
                    firstDifference = firstDifference,
                    ticks = observations.ToArray(),
                });

            bool matched = observations.Count == 2 &&
                           observations[0].frame == 40 &&
                           observations[1].frame == 40 &&
                           observations[0].state == LF2States.WeaponThrowing &&
                           observations[1].state == LF2States.WeaponThrowing &&
                           observations[0].hitFa == 12 &&
                           observations[1].hitFa == 12 &&
                           observations[0].reservedWeaponState == 0 &&
                           observations[1].reservedWeaponState == 0 &&
                           Nearly(observations[0].vx, 14.0) &&
                           Nearly(observations[1].vx, 14.0);
            Assert.That(
                matched,
                Is.True,
                $"OID124 first-difference witness: {firstDifference}; " +
                $"tick1={Format(observations, 0)}, tick2={Format(observations, 1)}");
        }

        [Test]
        public void HeldFollowThrowDropAndDamagedDrop_KeepReservedCarrierZero()
        {
            HeldScope followScope = CreateHeldScope(LF2States.WeaponOnHand, frameId: 0);
            HeldScope damagedScope = CreateHeldScope(LF2States.Falling, frameId: 0);
            int followState;
            int throwState;
            int dropState;
            int damagedState;
            bool thrown;
            bool forceDropped;
            try
            {
                WeaponActResult follow = followScope.Weapon.Act(
                    followScope.Holder,
                    new BattleWeaponPointValue(1, 0, 0, 0, 0, 40, 0, 0, 0),
                    Vector3.zero);
                followState = followScope.Weapon.Runtime.WeaponState;

                WeaponActResult thrownResult = followScope.Weapon.Act(
                    followScope.Holder,
                    new BattleWeaponPointValue(1, 0, 0, 0, 0, 40, 12, -4, 0),
                    Vector3.zero);
                throwState = followScope.Weapon.Runtime.WeaponState;
                thrown = thrownResult.Thrown;

                HeldScope dropScope = CreateHeldScope(LF2States.WeaponOnHand, frameId: 0);
                try
                {
                    dropScope.Weapon.Drop(3.0, -2.0);
                    dropState = dropScope.Weapon.Runtime.WeaponState;
                }
                finally
                {
                    dropScope.Dispose();
                }

                WeaponActResult damagedResult = damagedScope.Weapon.Act(
                    damagedScope.Holder,
                    new BattleWeaponPointValue(1, 0, 0, 0, 0, 0, 0, 0, 0),
                    Vector3.zero);
                damagedState = damagedScope.Weapon.Runtime.WeaponState;
                forceDropped = damagedResult.ForceDrop;
            }
            finally
            {
                followScope.Dispose();
                damagedScope.Dispose();
            }

            WriteJson(
                "Temp/Goal20_R3_HeldCarrier.json",
                new HeldCarrierReport
                {
                    follow = followState,
                    thrown = throwState,
                    drop = dropState,
                    damagedDrop = damagedState,
                    thrownResult = thrown,
                    forceDroppedResult = forceDropped,
                });

            Assert.That(
                followState == 0 &&
                throwState == 0 &&
                dropState == 0 &&
                damagedState == 0 &&
                thrown &&
                forceDropped,
                Is.True,
                $"held carrier states follow={followState}, throw={throwState}, " +
                $"drop={dropState}, damagedDrop={damagedState}; " +
                $"thrown={thrown}, forceDropped={forceDropped}");
        }

        [Test]
        public void ReservedCarrier_RemainsInCanonicalCopyChecksumParityAndSnapshot()
        {
            var source = new NTSDEntityRuntime
            {
                WeaponState = 0,
            };
            var copied = new NTSDEntityRuntime
            {
                WeaponState = 777,
            };
            Assert.That(source.TryCopyCanonicalStateTo(copied), Is.True);
            Assert.That(copied.WeaponState, Is.Zero);

            SimulationWorld world = new SimulationWorld();
            var weapon = new LF2Weapon { ObjectId = 124, Name = "R3CarrierSurface" };
            weapon.SetWeaponType(4);
            weapon.SetRequiredRuntimeSlot(3);
            world.Register(weapon);
            try
            {
                weapon.Runtime.WeaponState = 0;

                var identity = new LockstepSessionIdentity(
                    LockstepSessionIdentity.CurrentSchemaVersion,
                    0xB6030001UL,
                    424242U,
                    0UL,
                    0UL,
                    new[] { 0 });
                var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                    world.RuntimeSlotCapacityForDiagnostics);
                Assert.That(
                    world.TryCaptureWorldEntityRuntimeSnapshot(identity, 3, snapshot),
                    Is.True);
                var snapshotCopy = new NTSDEntityRuntime();
                Assert.That(snapshot.TryCopyEntityRuntime(3, snapshotCopy), Is.True);
                Assert.That(snapshotCopy.WeaponState, Is.Zero);
                Assert.That(
                    snapshot.SchemaVersion,
                    Is.EqualTo(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion));

                string parityZero = world.CaptureParityFrameSnapshot(3).OverallChecksum;
                ulong checksumZero = world.CaptureRuntimeChecksum64(3, null);
                weapon.Runtime.WeaponState = 777;
                string parityChanged = world.CaptureParityFrameSnapshot(3).OverallChecksum;
                ulong checksumChanged = world.CaptureRuntimeChecksum64(3, null);
                Assert.That(checksumChanged, Is.Not.EqualTo(checksumZero));
                Assert.That(parityChanged, Is.Not.EqualTo(parityZero));

                weapon.Runtime.WeaponState = 0;
                Assert.That(
                    world.CaptureParityFrameSnapshot(3).OverallChecksum,
                    Is.EqualTo(parityZero));
                Assert.That(world.CaptureRuntimeChecksum64(3, null), Is.EqualTo(checksumZero));
            }
            finally
            {
                world.Unregister(weapon);
            }
        }

        internal static void RunCurrentPlayWitness(SimulationWorld world)
        {
            Assert.That(Application.isPlaying, Is.True);
            var report = new RealPlayReport
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                beforeObjects = world.ObjectCount,
            };
            int weaponSlot = Enumerable.Range(20, 380).Reverse().First(i => world.FindEntityByRuntimeSlotForQuery(i) == null);
            int targetSlot = Enumerable.Range(0, 20).First(i => world.FindEntityByRuntimeSlotForQuery(i) == null);
            var weapon = CreateWeapon(world.RuntimeCharacterConfigs.Resolve(124).characterData, 40, 4);
            var target = CreateTarget();
            target.FrameCache.Load(world.RuntimeCharacterConfigs.Resolve(7));
            target.ImmediateFrame(0);
            var rows = new List<RuntimeObservation>();
            bool weaponRegistered = false, targetRegistered = false;
            try
            {
                weapon.SetRequiredRuntimeSlot(weaponSlot); target.SetRequiredRuntimeSlot(targetSlot);
                weapon.Team = weapon.RelationTeam = 1; target.Team = target.RelationTeam = 2;
                weapon.Runtime.SetPosition(200, -100, 0); target.Runtime.SetPosition(1000, -60, 0);
                weapon.Runtime.SyncIntegerPosition(); target.Runtime.SyncIntegerPosition();
                weapon.Runtime.SetVelocity(30, 0, 0); weapon.ObjectAiTargetSlot3F8 = targetSlot;
                world.Register(weapon); weaponRegistered = true;
                world.Register(target); targetRegistered = true;
                world.NativeRandom.ResetFromSeed(424242);
                for (int tick = 1; tick <= 2; tick++)
                {
                    weapon.RunFrameLogicBeforeAdvance();
                    rows.Add(new RuntimeObservation { tick = tick, frame = weapon.Frame.N,
                        state = weapon.Frame.D.state, hitFa = weapon.Frame.D.hit_Fa,
                        reservedWeaponState = weapon.Runtime.WeaponState,
                        vx = weapon.Runtime.Vx, vy = weapon.Runtime.Vy, vz = weapon.Runtime.Vz,
                        targetSlot = weapon.ObjectAiTargetSlot3F8,
                        synchronizedRngCalls = world.NativeRandom.CaptureScalarState().SynchronizedCalls });
                    Assert.That(weapon.Runtime.WeaponState, Is.Zero);
                    Assert.That(weapon.Frame.D.state, Is.EqualTo(1002));
                    Assert.That(weapon.Frame.D.hit_Fa, Is.EqualTo(12));
                    Assert.That(weapon.Runtime.Vx, Is.EqualTo(14));
                }
                report.status = "PASS";
            }
            catch (Exception e) { report.status = "FAIL"; report.error = e.ToString(); throw; }
            finally
            {
                if (targetRegistered) world.Unregister(target);
                if (weaponRegistered) world.Unregister(weapon);
                report.afterObjects = world.ObjectCount; report.weaponSlot = weaponSlot; report.targetSlot = targetSlot;
                report.ticks = rows.ToArray();
                if (report.afterObjects != report.beforeObjects) report.status = "FAIL";
                WriteJson("Temp/Goal20_R3_PlayWitness.json", report);
                Assert.That(report.afterObjects, Is.EqualTo(report.beforeObjects));
            }
        }

        [Serializable]
        private sealed class RealPlayReport
        {
            public string status, error, scene;
            public int beforeObjects, afterObjects, weaponSlot, targetSlot;
            public RuntimeObservation[] ticks;
            public uint seed = 424242;
            public string boundary = "Current runtime OID124/action40 and current OID7/action0 in real NTSD_Battle world; two explicit pre-frame-advance calls, no full-tick/physics or physical input claim.";
        }

        private static LF2CharacterData LoadWeapon9Data()
        {
            string path = ProjectPath(WeaponDatRelativePath);
            Assert.That(File.Exists(path), Is.True, $"missing current content {path}");
            Lf2DatFile parsed = new Lf2DatParserV2().Parse(
                Lf2DatDecryptor.DecryptFile(path),
                path);
            List<LF2FrameData> frames = Lf2DatConverter.ConvertAllFrames(parsed);
            Assert.That(frames, Is.Not.Null.And.Not.Empty);
            return new LF2CharacterData
            {
                name = "weapon9.dat",
                type_sub = (int)LF2ObjectType.ThrowWeapon,
                weapon_hp = 500,
                frames = frames,
            };
        }

        private static LF2Weapon CreateWeapon(
            LF2CharacterData data,
            int frameId,
            int weaponType)
        {
            var weapon = new LF2Weapon
            {
                ObjectId = 124,
                Name = "R3Oid124Weapon",
            };
            weapon.SetWeaponType(weaponType);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(124, data));
            weapon.ImmediateFrame(frameId);
            weapon.Health.HP = 500;
            weapon.Health.HPBound = 500;
            weapon.Health.HP3 = 500;
            weapon.SwitchDir("right");
            return weapon;
        }

        private static LF2Character CreateTarget()
        {
            var target = new LF2Character
            {
                ObjectId = 7,
                Name = "R3Oid124Target",
            };
            var data = new LF2CharacterData
            {
                name = target.Name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 10000,
                        next = 0,
                        pic = 0,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            target.FrameCache.Load(new LF2CharacterDataWrapper(7, data));
            target.ImmediateFrame(0);
            target.Health.HP = 500;
            target.Health.HPBound = 500;
            target.Health.HP3 = 500;
            target.HitStun = 0;
            return target;
        }

        private static HeldScope CreateHeldScope(int weaponState, int frameId)
        {
            var world = new SimulationWorld();
            var holder = new LF2Character
            {
                ObjectId = 9001,
                Name = "R3HeldHolder",
            };
            var holderData = new LF2CharacterData
            {
                name = holder.Name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 10000,
                        next = 0,
                        pic = 0,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            holder.FrameCache.Load(new LF2CharacterDataWrapper(9001, holderData));
            holder.SetRequiredRuntimeSlot(0);
            world.Register(holder);
            holder.ImmediateFrame(0);
            holder.Team = 1;
            holder.RelationTeam = 1;
            holder.Runtime.SetPosition(100.0, -10.0, 0.0);
            holder.Runtime.SyncIntegerPosition();
            holder.SwitchDir("right");

            LF2CharacterData weaponData = new LF2CharacterData
            {
                name = "R3HeldWeapon",
                type_sub = (int)LF2ObjectType.ThrowWeapon,
                weapon_hp = 500,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = weaponState,
                        wait = 10000,
                        next = 0,
                        pic = 0,
                        centerx = 20,
                        centery = 20,
                    },
                    new LF2FrameData
                    {
                        frameId = 40,
                        state = LF2States.WeaponThrowing,
                        wait = 10000,
                        next = 40,
                        pic = 0,
                        centerx = 20,
                        centery = 20,
                    },
                },
            };
            var weapon = new LF2Weapon
            {
                ObjectId = 124,
                Name = "R3HeldWeapon",
            };
            weapon.SetWeaponType(4);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(124, weaponData));
            weapon.SetRequiredRuntimeSlot(1);
            world.Register(weapon);
            weapon.ImmediateFrame(frameId);
            weapon.Health.HP = 500;
            weapon.Health.HPBound = 500;
            weapon.Health.HP3 = 500;
            weapon.Runtime.SetPosition(120.0, -10.0, 0.0);
            weapon.Runtime.SyncIntegerPosition();
            weapon.SwitchDir("right");
            holder.HoldWeapon(weapon);
            return new HeldScope(world, holder, weapon);
        }

        private static string FindFirstDifference(
            IReadOnlyList<RuntimeObservation> observations)
        {
            if (observations == null || observations.Count < 2)
                return "missing tick observations";
            if (observations[0].reservedWeaponState != 0)
                return "tick1 checksum: reserved WeaponState changed to " +
                       observations[0].reservedWeaponState;
            if (observations[1].reservedWeaponState != 0)
                return "tick2 checksum: reserved WeaponState changed to " +
                       observations[1].reservedWeaponState;
            if (!Nearly(observations[0].vx, 14.0))
                return $"tick1 motion: Vx={observations[0].vx} expected 14";
            if (!Nearly(observations[1].vx, 14.0))
                return $"tick2 motion: Vx={observations[1].vx} expected 14 (legacy halving)";
            return string.Empty;
        }

        private static string Format(
            IReadOnlyList<RuntimeObservation> observations,
            int index)
        {
            if (observations == null || index < 0 || index >= observations.Count)
                return "missing";
            RuntimeObservation value = observations[index];
            return $"frame={value.frame},state={value.state},hitFa={value.hitFa}," +
                   $"reserved={value.reservedWeaponState},vx={value.vx},checksum={value.checksum}";
        }

        private static bool Nearly(double left, double right)
        {
            return Math.Abs(left - right) < 0.000001;
        }

        private static string ReadProjectFile(string relativePath)
        {
            string path = ProjectPath(relativePath);
            Assert.That(File.Exists(path), Is.True, $"missing source path {path}");
            return File.ReadAllText(path);
        }

        private static string ProjectPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(
                projectRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static void WriteJson(string relativePath, object value)
        {
            string path = ProjectPath(relativePath);
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
        }

        [Serializable]
        private sealed class FrameLoopReport
        {
            public int oid;
            public string source;
            public FrameObservation[] frames;
        }

        [Serializable]
        private sealed class FrameObservation
        {
            public int frame;
            public int state;
            public int hitFa;
            public int next;
        }

        [Serializable]
        private sealed class FocusedWitnessReport
        {
            public string status;
            public string source;
            public string firstDifference;
            public uint seed = 424242;
            public string input = "empty";
            public string window = "non-character hit_Fa pre-frame-advance subpass; no full tick/physics claim";
            public RuntimeObservation[] ticks;
        }

        [Serializable]
        private sealed class RuntimeObservation
        {
            public int tick;
            public int frame;
            public int state;
            public int hitFa;
            public int reservedWeaponState;
            public double vx, vy, vz;
            public int targetSlot;
            public ulong synchronizedRngCalls;
            public string checksum;
        }

        [Serializable]
        private sealed class HeldCarrierReport
        {
            public int follow;
            public int thrown;
            public int drop;
            public int damagedDrop;
            public bool thrownResult;
            public bool forceDroppedResult;
        }

        private sealed class HeldScope : IDisposable
        {
            private bool disposed;

            public HeldScope(
                SimulationWorld world,
                LF2Character holder,
                LF2Weapon weapon)
            {
                World = world;
                Holder = holder;
                Weapon = weapon;
            }

            public SimulationWorld World { get; }
            public LF2Character Holder { get; }
            public LF2Weapon Weapon { get; }

            public void Dispose()
            {
                if (disposed)
                    return;
                disposed = true;
                World.Unregister(Weapon);
                World.Unregister(Holder);
            }
        }
    }
}
#endif
