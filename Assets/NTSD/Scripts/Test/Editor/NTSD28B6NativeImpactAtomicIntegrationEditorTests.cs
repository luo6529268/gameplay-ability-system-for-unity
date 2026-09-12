#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6NativeImpactAtomicIntegrationEditorTests
    {
        [Test, Combinatorial]
        public void CurrentTypeDispatchMatrix([Values(0, 1, 2, 3, 4, 6)] int type,
            [Values(10, 11, 17, 18)] int kind, [Values(0, 1, 2)] int route)
        {
            using (var s = new Scenario(type, route == 1))
            {
                s.Itr.kind = kind;
                bool expected = type != 3 && (kind != 17 || type == 0) && (kind != 18 || type != 0);
                bool applied = s.Dispatch(route);
                Assert.That(applied, Is.EqualTo(expected));
                s.AssertResult(expected);
            }
        }

        [Test, Combinatorial]
        public void CharacterOwnerPreflightRejectsWithoutAnyMutation([Values(0, 1, 2, 3)] int route,
            [Values(0, 1)] int invalidHop)
        {
            using (var s = new Scenario(0, route == 1))
            {
                if (invalidHop == 0) s.Attacker.Runtime.OwnerSlotIndex = 398;
                else s.Owner.Runtime.OwnerSlotIndex = 398;
                Assert.That(s.Dispatch(route), Is.False);
                s.AssertResult(false);
            }
        }

        [Test, Combinatorial]
        public void Kind11ReadsEnvironmentAndNeverWeaponCount([Values(0, 1, 2)] int type,
            [Values(-1, 0, 1)] int environment, [Values(-20, 0, 77)] int weaponCount)
        {
            using (var s = new Scenario(type, false))
            {
                s.Itr.kind = 11;
                s.Target.Runtime.EnvironmentState320 = environment;
                s.Target.WeaponCount = weaponCount;
                s.CaptureBefore();
                bool accepted = environment < 0;
                Assert.That(s.Dispatch(0), Is.EqualTo(accepted));
                s.AssertResult(accepted);
            }
        }

        [Test, Combinatorial]
        public void NativeObjectImmunityAndYStep([Values(1, 2, 4, 6)] int type,
            [Values(123, 201, 202)] int oid, [Values(-3, -2, 0)] int y)
        {
            using (var s = new Scenario(type, true, oid))
            {
                s.Target.Runtime.YInt = y;
                s.Target.Runtime.Y = y - 0.75;
                s.CaptureBefore();
                bool accepted = type == 2 || oid == 123;
                Assert.That(s.Dispatch(0), Is.EqualTo(accepted));
                s.AssertResult(accepted);
            }
        }

        [TestCase(0)]
        [TestCase(233)]
        [TestCase(-888)]
        public void LegacyCharacterUsesLiteralRespondAndSharedTransaction(int respond)
        {
            using (var s = new Scenario(0, false))
            {
                s.Itr.respond = respond;
                Assert.That(s.Dispatch(3), Is.True);
                s.AssertResult(true);
            }
        }

        [Test, Combinatorial]
        public void CapturedCandidateAndHitPlanShareImpact([Values(10, 11, 17, 18)] int kind,
            [Values(0, 1, 2, 3, 4, 6)] int type,
            [Values(BattleHitExecutionPlanMode.ShadowCompare, BattleHitExecutionPlanMode.DataOriented)] BattleHitExecutionPlanMode mode)
        {
            using (var s = new Scenario(type, false))
            {
                s.Itr.kind = kind;
                s.Attacker.Frame.D.itrs.Add(s.Itr);
                s.World.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                var query = (BruteForceSceneQuery)s.World.SceneQuery;
                query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                s.World.CaptureCollisionFrameSnapshotsAll();
                s.World.CollectCollisionCandidatesAll();
                Assert.That(query.TryGetCollisionCandidateRange(s.Attacker, out CollisionCandidateRange range), Is.True);
                Assert.That(range.Count, Is.EqualTo(1));
                Assert.That(range.TryGet(0, out SceneQueryHit candidate), Is.True);
                Assert.That(candidate.ResolveCurrentTarget(s.World), Is.SameAs(s.Target));
                s.CaptureBefore();
                s.World.PostInteractionTickAll(12);
                bool accepted = type != 3 && (kind != 17 || type == 0) && (kind != 18 || type != 0);
                BattleHitExecutionPlanDiagnostics d = s.World.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                TestContext.WriteLine($"kind={s.Itr.kind} attackerFrame={s.Attacker.Frame.N} env={s.Target.Runtime.EnvironmentState320} " +
                    $"valid={d.CurrentTickPlanValid} failures={d.FailureCount} mismatches={d.ObservationMismatchCount} " +
                    $"writers={d.ObservedWriterEffectCount} mask={d.LastWriterEffectDifferenceMask}");
                s.AssertResult(accepted);
                Assert.That(d.CurrentTickPlanValid, Is.True);
                Assert.That(d.FailureCount, Is.Zero);
                Assert.That(d.ObservationMismatchCount, Is.Zero);
                if (mode == BattleHitExecutionPlanMode.ShadowCompare)
                    Assert.That(d.ObservedWriterEffectCount, Is.EqualTo(1));
                Assert.That(d.LastWriterEffectDifferenceMask, Is.Zero);
            }
        }

        internal sealed class Scenario : IDisposable
        {
            internal readonly SimulationWorld World = new SimulationWorld();
            internal readonly LF2Character Attacker;
            internal readonly LF2Entity Owner;
            internal readonly LF2Entity Credit;
            internal readonly LF2Entity Target;
            internal readonly InteractionArea Itr = new InteractionArea { kind = 10, fall = 80, respond = 0, x = -20, y = -20, w = 40, h = 40, zwidth = 40 };
            private int environment, catchSource, impactSource, action, yInt, weaponCount;
            private double y, vx, vy, vz, pendingY;
            private uint rngState;
            private ulong rngCalls;

            internal Scenario(int type, bool generic, int oid = 123)
            {
                Attacker = (LF2Character)Create(World, 0, 0, 36, false);
                Owner = Create(World, 19, 0, 9036, true);
                Credit = Create(World, 399, 0, 9037, true);
                Target = Create(World, 1, type, oid, generic);
                Owner.Runtime.SetPosition(1000, 0, 0);
                Credit.Runtime.SetPosition(2000, 0, 0);
                Attacker.Runtime.OwnerSlotIndex = 19;
                Owner.Runtime.OwnerSlotIndex = 399;
                Target.Runtime.EnvironmentState320 = -9;
                Target.Runtime.CatchSourceSlot90 = 733;
                Target.Runtime.ImpactSourceSlot164 = 734;
                Target.Runtime.Vx = 13.7;
                Target.Runtime.Vy = -5.9;
                Target.Runtime.Vz = -17.3;
                Target.Runtime.YInt = -3;
                Target.Runtime.Y = -3.75;
                Target.KnockbackVx = 71;
                Target.KnockbackVy = 72;
                Target.KnockbackVz = 73;
                Target.WeaponCount = 77;
                Target.Runtime.FrameDelay = 31;
                Target.Runtime.FrameWaitCounter = 17;
                Target.Runtime.HP = 401;
                Target.Runtime.HPBound = 499;
                Target.ComboCountAtk = 42;
                Target.ComboCountVic = 43;
                CaptureBefore();
            }

            internal void CaptureBefore()
            {
                var r = Target.Runtime;
                environment = r.EnvironmentState320; catchSource = r.CatchSourceSlot90;
                impactSource = r.ImpactSourceSlot164; action = Target.Frame.N;
                yInt = r.YInt; y = r.Y; vx = r.Vx; vy = r.Vy; vz = r.Vz;
                pendingY = Target.KnockbackVy; weaponCount = Target.WeaponCount;
                rngState = World.Rng.State; rngCalls = World.Rng.CallCount;
            }

            internal bool Dispatch(int route)
            {
                if (route == 3)
                    return new LF2CharacterHitResolver((LF2Character)Target).ResolveHit(Itr, Attacker, Vector3.zero, default);
                if (route == 2)
                {
                    if (Target is LF2Character c) return c.Hit(Itr, Attacker, Vector3.zero, default);
                    if (Target is LF2Weapon w) return w.Hit(Itr, Attacker);
                    if (Target is LF2SpecialAttack special) return special.Hit(Itr, Attacker);
                }
                return World.DamageWriter.TryApplyCurrentDatTargetHit(World, Attacker, Target, Itr, Vector3.zero);
            }

            internal void AssertResult(bool accepted)
            {
                var r = Target.Runtime;
                int type = Target.GetCurrentDataObjectTypeForSimulation();
                bool character = type == 0;
                Assert.That(r.EnvironmentState320, Is.EqualTo(accepted && character ? -20 : environment));
                Assert.That(r.CatchSourceSlot90, Is.EqualTo(accepted && character ? 0x2000 + 399 : catchSource));
                Assert.That(r.ImpactSourceSlot164, Is.EqualTo(accepted && character ? 0 : impactSource));
                Assert.That(Target.Frame.N, Is.EqualTo(!accepted ? action : character ? Itr.respond == 0 ? 182 : Itr.respond : 0));
                Bits(r.Vx, accepted ? vx / 1.07 : vx);
                Bits(r.Vz, accepted ? vz / 1.07 : vz);
                Bits(Target.KnockbackVx, accepted ? vx / 1.07 : 71);
                Bits(Target.KnockbackVz, accepted ? vz / 1.07 : 73);
                bool clamp = accepted && yInt >= -2;
                bool adjust = accepted && !clamp && vy > -6;
                Bits(r.Y, clamp ? -2 : y);
                Assert.That(r.YInt, Is.EqualTo(clamp ? -2 : yInt));
                double expectedVy = clamp ? -6 : adjust ? vy - (character ? 3.0 : 2.3) : vy;
                Bits(r.Vy, expectedVy);
                Bits(Target.KnockbackVy, adjust ? expectedVy : pendingY);
                Assert.That(Target.WeaponCount, Is.EqualTo(weaponCount));
                Assert.That(r.HP, Is.EqualTo(401));
                Assert.That(r.HPBound, Is.EqualTo(499));
                Assert.That(r.FrameDelay, Is.EqualTo(31));
                Assert.That(r.FrameWaitCounter, Is.EqualTo(17));
                Assert.That(Target.ComboCountAtk, Is.EqualTo(42));
                Assert.That(Target.ComboCountVic, Is.EqualTo(43));
                Assert.That(World.Rng.State, Is.EqualTo(rngState));
                Assert.That(World.Rng.CallCount, Is.EqualTo(rngCalls));
            }

            private static void Bits(double actual, double expected)
            {
                Assert.That(BitConverter.DoubleToInt64Bits(actual), Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
            }

            private static LF2Entity Create(SimulationWorld world, int slot, int type, int oid, bool generic)
            {
                LF2Entity e;
                if (generic) e = new GenericShell(type);
                else if (type == 0) { var c = new LF2Character(); c.ModuleInitialize(); e = c; }
                else if (type == 3) e = new LF2SpecialAttack();
                else { var w = new LF2Weapon(); w.SetWeaponType(type); e = w; }
                var f = new LF2FrameData { frameId = 20, state = 0, wait = 10000, next = 20 };
                if (slot == 0 || slot == 1)
                    f.bodies.Add(new BattleBodyBoxValue(-10, -10, 20, 20));
                var frames = new List<LF2FrameData> { f };
                foreach (int id in new[] { 0, 182, 233 }) frames.Add(new LF2FrameData { frameId = id, state = 0, wait = 10000, next = id });
                e.ObjectId = oid;
                e.FrameCache.Load(new LF2CharacterDataWrapper(oid, new LF2CharacterData { name = "ImpactFixture", type_sub = type, frames = frames }));
                e.Frame.D = f; e.Frame.N = 20; e.Frame.Prev2D = f;
                if (e is LF2Character initialized) initialized.Initialize(500, 500);
                e.SetRequiredRuntimeSlot(slot); world.Register(e);
                e.Team = slot == 1 ? 8 : 7; e.RelationTeam = e.Team;
                e.SwitchDir("right"); e.Runtime.SetPosition(0, 0, 0);
                e.Runtime.SpecialHitLatch0EB = false;
                e.RefreshRuntimeSnapshot();
                return e;
            }

            public void Dispose()
            {
                World.EndCollisionCandidateConsumption();
                World.Unregister(Target); World.Unregister(Credit); World.Unregister(Owner); World.Unregister(Attacker);
            }
        }

        private sealed class GenericShell : LF2SpecialAttack
        {
            private readonly int type;
            internal GenericShell(int type) { this.type = type; }
            public override int GetCurrentDataObjectTypeForSimulation() => type;
        }
    }

    internal sealed class Goal18TestResultCapture : ICallbacks
    {
        private static TestRunnerApi api;
        [InitializeOnLoadMethod]
        private static void Register()
        {
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.hideFlags = HideFlags.HideAndDontSave;
            api.RegisterCallbacks(new Goal18TestResultCapture());
        }

        public void RunStarted(ITestAdaptor test) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            File.WriteAllText("Temp/Goal18_LastTestResults.xml", result.ToXml().OuterXml);
        }
    }

    internal static class Goal18ImpactPlayProbe
    {
        private const string Request = "Temp/Goal18_Play.request";
        private static bool paused;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!File.Exists(Request) || !EditorApplication.isPlaying || EditorApplication.isCompiling)
                return;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;
            if (!paused) { driver.SetPaused(true); paused = true; return; }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
            var report = new PlayReport();
            SimulationWorld world = driver.World;
            int beforeCount = world.ObjectCount;
            var originalRandom = world.NativeRandom.CaptureState();
            uint originalSeed = world.NativeRandom.CaptureScalarState().TableSeed;
            try
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "NTSD_Battle")
                    throw new InvalidOperationException("Wrong scene");
                foreach (int kind in new[] { 10, 11 }) Run(driver, kind, report);
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            finally
            {
                world.NativeRandom.ResetFromSeed(originalSeed);
                world.NativeRandom.Restore(originalRandom);
                report.cleanup = world.ObjectCount == beforeCount;
                if (!report.cleanup) report.status = "FAIL";
                File.WriteAllText("Temp/Goal18_Play.result.json", JsonUtility.ToJson(report, true));
                File.Delete(Request);
                EditorApplication.update -= Poll;
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private static PlayRow Run(SimulationTickDriver driver, int kind, PlayReport report)
        {
            SimulationWorld world = driver.World;
            var row = new PlayRow { kind = kind, seed = 424242, input = "FrameInputSet.Empty" };
            report.rows.Add(row);
            var sourceData = world.RuntimeCharacterConfigs.Resolve(36);
            var targetData = world.RuntimeCharacterConfigs.Resolve(16);
            LF2FrameData sourceFrame = sourceData.characterData.frames.First(f => f.itrs.Any(i => i.kind == kind));
            InteractionArea itr = sourceFrame.itrs.First(i => i.kind == kind);
            row.sourceAction = sourceFrame.frameId;
            row.sourceState = sourceFrame.state;
            row.respond = itr.respond;
            row.itrIndex = sourceFrame.itrs.IndexOf(itr);
            row.itrJson = JsonUtility.ToJson(itr);
            LF2Character source = null;
            ObservedTarget target = null;
            var query = (BruteForceSceneQuery)world.SceneQuery;
            Action originalObserver = query.BeforeCollisionCandidateStoreFinalCompareForSelfCheck;
            try
            {
                source = new LF2Character { ObjectId = 36, Name = "Goal18_Tayuya_CurrentDAT" };
                source.ModuleInitialize(); source.FrameCache.Load(sourceData);
                source.SetRequiredRuntimeSlot(world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000));
                world.Register(source); source.Initialize(500, 500);
                source.ClearBattleEntryInputState(); source.AiControlled = false;
                source.ImmediateFrame(sourceFrame.frameId);
                source.Runtime.OwnerSlotIndex = source.Runtime.SlotIndex;
                source.Team = 3; source.RelationTeam = 3; source.SwitchDir("right");
                source.Runtime.SetPosition(200, 0, world.Runtime.Stage.ZMin + 50);
                source.Runtime.SyncIntegerPosition();
                source.Runtime.SetVelocity(0, 0, 0); source.FrameDelay = 1000;
                target = new ObservedTarget { ObjectId = 16, Name = "Goal18_ImpactTarget", Row = row, Source = source };
                target.ModuleInitialize(); target.FrameCache.Load(targetData);
                target.SetRequiredRuntimeSlot(world.FindFirstFreeRuntimeSlotForDiagnostics(source.Runtime.SlotIndex + 1, 1000));
                world.Register(target); target.Initialize(500, 500);
                target.ClearBattleEntryInputState(); target.AiControlled = false;
                target.ImmediateFrame(0); target.Team = 8; target.RelationTeam = 8;
                target.Runtime.OwnerSlotIndex = target.Runtime.SlotIndex;
                target.SwitchDir("right");
                // Place the current target body at the center of the actual Tayuya ITR.
                BattleBodyBoxValue body = target.Frame.D.bodies[0];
                double targetX = kind == 11
                    ? source.Runtime.X - sourceFrame.centerx + itr.x + itr.w + target.Frame.D.centerx - body.X - body.W - 1
                    : source.Runtime.X + itr.x + itr.w / 2 - sourceFrame.centerx + target.Frame.D.centerx - body.X - body.W / 2;
                target.Runtime.SetPosition(targetX,
                    kind == 11 ? -3.75 : 0, source.Runtime.Z);
                target.Runtime.SyncIntegerPosition();
                target.Runtime.SetVelocity(kind == 11 ? 13.7 : 0, -5.9, kind == 11 ? -17.3 : 0);
                target.Runtime.EnvironmentState320 = kind == 11 ? -1 : 0;
                target.WeaponCount = 77;
                target.FrameDelay = 1000;
                row.sourceSlot = source.Runtime.SlotIndex; row.targetSlot = target.Runtime.SlotIndex;
                source.RefreshRuntimeSnapshot(); target.RefreshRuntimeSnapshot();
                query.BeforeCollisionCandidateStoreFinalCompareForSelfCheck = () =>
                {
                    originalObserver?.Invoke();
                    if (query.TryGetCollisionCandidateRange(source, out CollisionCandidateRange range))
                    {
                        for (int i = 0; i < range.Count; i++)
                        {
                            if (range.TryGet(i, out SceneQueryHit candidate) && candidate.ResolveCurrentTarget(world) == target &&
                                candidate.ItrIndex == row.itrIndex)
                                row.candidate = true;
                        }
                    }
                    row.before = Capture(target, world);
                    row.sourceX = source.Runtime.XInt;
                    row.sourceY = source.Runtime.YInt;
                    row.sourceZ = source.Runtime.ZInt;
                };
                world.NativeRandom.ResetFromSeed((uint)row.seed);
                row.tick = driver.CurrentTickIndex + 1;
                row.armed = true;
                if (!driver.StepOneTick(new FrameInputSet(row.tick, Array.Empty<SimulationPlayerInput>()),
                    ignorePaused: true, buildPresentation: false)) throw new InvalidOperationException("Driver refused tick");
                row.armed = false;
                row.afterTick = Capture(target, world);
                if (!row.candidate || row.before == null || row.after == null)
                    throw new InvalidOperationException("Missing actual Tayuya candidate or post-hit observation");
                if (kind == 11 && (row.candidateIndices.Count != 1 || row.candidateIndices[0] != row.itrIndex))
                    throw new InvalidOperationException("Kind11 witness also overlaps another ITR");
                if (row.after.environment != -20 || row.after.catchSource != 0x2000 + row.sourceSlot ||
                    row.after.impactSource != row.sourceSlot || row.after.weaponCount != 77)
                    throw new InvalidOperationException("Impact transaction mismatch");
                if (row.before.pp != row.after.pp || row.before.sourceArest != row.after.sourceArest ||
                    row.before.targetArest != row.after.targetArest || row.before.targetVrest != row.after.targetVrest)
                    throw new InvalidOperationException("Impact changed PP or rest");
                row.status = "PASS";
            }
            finally
            {
                row.armed = false;
                query.BeforeCollisionCandidateStoreFinalCompareForSelfCheck = originalObserver;
                if (target != null) world.Unregister(target);
                if (source != null) world.Unregister(source);
            }
            return row;
        }

        private sealed class ObservedTarget : LF2Character
        {
            internal PlayRow Row;
            internal LF2Entity Source;
            public override void SimPostInteraction(int tickIndex)
            {
                if (Row.armed && Row.before != null)
                {
                    if (Match.SceneQuery.TryGetCollisionCandidateRange(Source, out CollisionCandidateRange range))
                    {
                        for (int i = 0; i < range.Count; i++)
                            if (range.TryGet(i, out SceneQueryHit candidate) && candidate.ResolveCurrentTarget(Match) == this)
                            {
                                Row.candidateIndices.Add(candidate.ItrIndex);
                                if (candidate.ItrIndex == Row.itrIndex) Row.candidate = true;
                            }
                    }
                    Row.after = Capture(this, Match);
                }
                base.SimPostInteraction(tickIndex);
            }
        }

        private static PlayState Capture(LF2Entity entity, SimulationWorld world)
        {
            var r = entity.Runtime;
            LF2Entity source = ((ObservedTarget)entity).Source;
            return new PlayState
            {
                action = entity.Frame.N, state = entity.GetState(), yInt = r.YInt, y = r.Y, vx = r.Vx, vy = r.Vy, vz = r.Vz,
                xInt = r.XInt, zInt = r.ZInt, x = r.X, z = r.Z,
                px = r.KnockbackVx, py = r.KnockbackVy, pz = r.KnockbackVz,
                environment = r.EnvironmentState320, catchSource = r.CatchSourceSlot90, impactSource = r.ImpactSourceSlot164,
                hp = r.HP, hpBound = r.HPBound, weaponCount = entity.WeaponCount,
                pp = entity.Health.PP, sourceArest = source.ItrRest?.Arest ?? 0,
                targetArest = entity.ItrRest?.Arest ?? 0,
                targetVrest = entity.ItrRest?.GetVrest(source.Runtime.SlotIndex) ?? 0,
                delay = r.FrameDelay, wait = r.FrameWaitCounter, rng = world.Rng.State, rngCalls = world.Rng.CallCount,
                vxBits = BitConverter.DoubleToInt64Bits(r.Vx), vyBits = BitConverter.DoubleToInt64Bits(r.Vy),
                vzBits = BitConverter.DoubleToInt64Bits(r.Vz), pyBits = BitConverter.DoubleToInt64Bits(r.KnockbackVy),
            };
        }

        [Serializable] private sealed class PlayReport { public string status, error; public bool cleanup; public List<PlayRow> rows = new List<PlayRow>(); }
        [Serializable] internal sealed class PlayRow
        {
            public string status, input, itrJson;
            public int kind, seed, tick, sourceAction, sourceState, sourceSlot, targetSlot, respond, itrIndex;
            public int sourceX, sourceY, sourceZ;
            public bool candidate;
            public List<int> candidateIndices = new List<int>();
            [NonSerialized] public bool armed;
            public PlayState before, after, afterTick;
        }
        [Serializable] internal sealed class PlayState
        {
            public int action, state, yInt, environment, catchSource, impactSource, hp, hpBound, weaponCount, delay, wait;
            public int xInt, zInt;
            public int pp, sourceArest, targetArest, targetVrest;
            public double x, z;
            public double y, vx, vy, vz, px, py, pz;
            public long vxBits, vyBits, vzBits, pyBits;
            public uint rng;
            public ulong rngCalls;
        }
    }
}
#endif
