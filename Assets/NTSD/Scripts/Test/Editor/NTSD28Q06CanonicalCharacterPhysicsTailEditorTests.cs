#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06CanonicalCharacterPhysicsTailEditorTests
    {
        private sealed class Case
        {
            internal string Name;
            internal int Action = 170, State = 12, Floor, HitG, Phase, Environment, Pending;
            internal double Y = -1, Vx, Vy = 1, Vz;
            internal int ExpectedAction = 230, ExpectedCounter, Hp = 100;
            internal double ExpectedVx, ExpectedVy;
        }

        // Parameters and native expectations are retained from the B4 contact,
        // ordinary-landing, airborne and environment-credit source contracts.
        private static IEnumerable<Case> Cases()
        {
            yield return new Case { Name = "already-contact-soft-pending", Floor = -10, Y = -10, Vx = 6, Vy = 0,
                Pending = 1, ExpectedVx = 5.0 / 3.0 };
            yield return new Case { Name = "cross-soft-back", Action = 187, Vx = 6, ExpectedAction = 231, ExpectedVx = 2 };
            yield return new Case { Name = "strict-hard-front", Vx = 9.0001, ExpectedAction = 185, ExpectedVx = 7, ExpectedVy = -3.5, ExpectedCounter = 9 };
            yield return new Case { Name = "hard-back", Action = 187, Y = -12, Vy = 12, ExpectedAction = 191, ExpectedVy = -3.5, ExpectedCounter = 9 };
            yield return new Case { Name = "hard-pending-back", Action = 187, Y = -12, Vx = 10, Vy = 12, Vz = 2,
                Pending = 2, ExpectedAction = 300, ExpectedVx = 10, ExpectedVy = -49, ExpectedCounter = 9 };
            yield return new Case { Name = "state18-pending", Action = 200, State = 18, Vx = 3, Pending = 1,
                ExpectedAction = 301, ExpectedVx = 3, ExpectedVy = -1.5, ExpectedCounter = 9 };
            yield return new Case { Name = "ordinary-negative-floor", Action = 300, State = 4, HitG = 630,
                Floor = -10, Y = -12, Vx = 6, Vy = 3, ExpectedAction = 630, ExpectedVx = 2 };
            yield return new Case { Name = "ordinary-action212-priority", Action = 212, State = 4, HitG = 630,
                Vx = 9, Vy = 2, ExpectedAction = 215, ExpectedVx = 3 };
            yield return new Case { Name = "ordinary-state100-priority", Action = 300, State = 100, HitG = 630,
                Vx = 6, Vy = 2, ExpectedAction = 94, ExpectedVx = 2 };
            yield return new Case { Name = "airborne-upcoming-phase6", Y = -20, Vy = 0, Phase = 5, Environment = -1,
                ExpectedAction = 182, ExpectedVy = NTSDGlobal.Gameplay.Gravity, ExpectedCounter = 9 };
            yield return new Case { Name = "airborne-upcoming-phase5", Y = -20, Vy = 0, Phase = 4, Environment = -1,
                ExpectedAction = 181, ExpectedVy = NTSDGlobal.Gameplay.Gravity, ExpectedCounter = 9 };
            yield return new Case { Name = "state18-airborne", Action = 204, State = 18, Floor = -20, Y = -25, Vy = 1,
                ExpectedAction = 205, ExpectedVy = 1 + NTSDGlobal.Gameplay.Gravity, ExpectedCounter = 9 };
            yield return new Case { Name = "environment-credit-before-contact", Environment = -10 };
            yield return new Case { Name = "environment-lethal-normalization", Environment = -10, Hp = 5 };
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ActualPhysicsPassMatchesNativeTailAndLegacy(BattleRuntimeProfile profile)
        {
            foreach (Case c in Cases())
            {
                string legacy = Run(c, profile, BattleEcsCharacterFrameAdvancePassMode.Legacy);
                string canonical = Run(c, profile, BattleEcsCharacterFrameAdvancePassMode.DataOriented);
                Assert.That(canonical, Is.EqualTo(legacy), c.Name + " canonical versus Legacy");
            }
        }

        private static string Run(Case c, BattleRuntimeProfile profile, BattleEcsCharacterFrameAdvancePassMode mode)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(mode);
                var definitions = new Dictionary<int, LF2CharacterDataWrapper>
                {
                    [77] = Definition(77, c.Action, c.State, c.HitG),
                    [78] = Definition(78, 0, 0, 0)
                };
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(77, 0, "physics-victim.dat"),
                    new ObjectDefinition(78, 0, "physics-credit.dat")
                }, id => definitions[id]);
                LF2Entity victim = Create(world, 77, 0, c.Action);
                LF2Entity credit = Create(world, 78, 1, 0);
                var r = victim.Runtime;
                r.CollisionYReference = c.Floor;
                r.SetPosition(300, c.Y, 100);
                r.SetVelocity(c.Vx, c.Vy, c.Vz);
                r.SyncIntegerPosition();
                r.HP = c.Hp;
                r.HPBound = 80;
                r.PP = 50;
                r.EnvironmentState320 = c.Environment;
                r.CatchSourceSlot90 = 1;
                r.EnvironmentSourceSlot160 = 0;
                victim.AttackingCounter = 9;
                if (c.Pending != 0)
                {
                    r.StatusDx1C0 = c.Pending == 2 ? 560 : 2;
                    r.StatusDy1C4 = c.Pending == 2 ? 501 : 2;
                    r.StatusDz1C8 = c.Pending == 2 ? 5 : 2;
                    r.StatusGain1CC = 1;
                    r.StatusHitFacing1D0 = 0;
                    r.StatusPickedAction1D4 = 300;
                    r.StatusPickingAction1D8 = 301;
                }
                credit.Runtime.SetPosition(500, 0, 100);
                credit.Runtime.SyncIntegerPosition();
                world.Runtime.NativeWorldClock.ResourcePhase12 = c.Phase;
                world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                var diagnostics = world.BattleEcsCharacterFrameAdvancePassDiagnosticsForDiagnostics;
                Assert.That(diagnostics.Mode, Is.EqualTo(mode));
                if (mode == BattleEcsCharacterFrameAdvancePassMode.DataOriented)
                    Assert.That(diagnostics.ExactCharacterCount, Is.EqualTo(2), c.Name + " exact production dispatch");
                else
                    Assert.That(diagnostics.CompatibilityFallbackCount, Is.EqualTo(2), c.Name + " Legacy dispatch");
                Assert.That(victim.Frame.N, Is.EqualTo(c.ExpectedAction), c.Name);
                Assert.That(victim.AttackingCounter, Is.EqualTo(c.ExpectedCounter), c.Name);
                Assert.That(r.Vx, Is.EqualTo(c.ExpectedVx).Within(1e-10), c.Name);
                Assert.That(r.Vy, Is.EqualTo(c.ExpectedVy).Within(1e-10), c.Name);
                if (!c.Name.Contains("airborne")) Assert.That(r.Y, Is.EqualTo(c.Floor), c.Name);
                if (c.Pending != 0)
                {
                    bool deferred = c.Name == "already-contact-soft-pending";
                    Assert.That(r.StatusGain1CC, Is.EqualTo(deferred ? 1 : 0), c.Name);
                    Assert.That(r.StatusDx1C0, Is.EqualTo(deferred ? 2 : 0), c.Name);
                    Assert.That(r.StatusDy1C4, Is.EqualTo(deferred ? 2 : 0), c.Name);
                    Assert.That(r.StatusDz1C8, Is.EqualTo(deferred ? 2 : 0), c.Name);
                    if (c.Pending == 2) Assert.That(r.Vz, Is.EqualTo(7), c.Name);
                }
                if (c.Environment == -10)
                {
                    Assert.That(r.HP, Is.EqualTo(c.Hp - 10), c.Name);
                    Assert.That(r.HPBound, Is.EqualTo(c.Hp <= 10 ? 0 : 70), c.Name);
                    Assert.That(r.PP, Is.EqualTo(c.Hp <= 10 ? 0 : 50), c.Name);
                    Assert.That(r.InputHpConsumedTotal34C, Is.EqualTo(10), c.Name);
                    Assert.That(r.EnvironmentState320, Is.EqualTo(1), c.Name);
                    Assert.That(credit.Runtime.InputScoreTotal348, Is.EqualTo(10), c.Name);
                    Assert.That(credit.Runtime.KnockoutCount358, Is.EqualTo(c.Hp <= 10 ? 1 : 0), c.Name);
                }
                Assert.That(world.NativeResourcePhase12, Is.EqualTo(c.Phase), c.Name + " physics does not advance resource phase");
                return JsonConvert.SerializeObject(new
                {
                    raw = NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1),
                    r.StatusDx1C0, r.StatusDy1C4, r.StatusDz1C8, r.StatusGain1CC,
                    r.StatusPickedAction1D4, r.StatusPickingAction1D8, r.EnvironmentState320,
                    r.InputHpConsumedTotal34C, credit.Runtime.InputScoreTotal348, credit.Runtime.KnockoutCount358
                });
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        private static LF2Entity Create(SimulationWorld world, int id, int slot, int action)
        {
            var entity = world.LogicEntityFactory.Create(new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                preserveActionZero = true, relationTeam = 0,
                opoint = new ObjectPoint { oid = id, action = action }
            }, out _);
            Assert.That(entity, Is.TypeOf<LF2Character>());
            entity.AiControlled = false;
            return entity;
        }

        private static LF2CharacterDataWrapper Definition(int id, int current, int state, int hitG)
        {
            var data = new LF2CharacterData { name = "CanonicalPhysicsTail" };
            foreach (int action in new[] { current, 0, 94, 170, 180, 181, 182, 183, 185, 187, 188, 191, 200, 204, 205, 212, 215, 219, 230, 231, 300, 301, 630 }.Distinct())
                data.frames.Add(new LF2FrameData
                {
                    frameId = action, state = action == current ? state : 0,
                    hit_g = action == current ? hitG : 0, wait = 100, next = action
                });
            return new LF2CharacterDataWrapper(id, data);
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06CanonicalPhysicsPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_CanonicalPhysicsPlay.request";
        static NTSD28Q06CanonicalPhysicsPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var sceneWorld = driver?.World;
            if (sceneWorld == null || driver.CurrentTickIndex < 5 || !sceneWorld.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            string status = "FAIL", error = null;
            int physicsPasses = 0;
            try
            {
                var fixture = new NTSD28Q06CanonicalCharacterPhysicsTailEditorTests();
                foreach (var profile in new[] { BattleRuntimeProfile.Authority400, BattleRuntimeProfile.MobileExtended })
                {
                    fixture.ActualPhysicsPassMatchesNativeTailAndLegacy(profile);
                    physicsPasses += 28;
                }
                Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_CanonicalPhysicsPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, physicsPasses, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene paused and preserved; 14 synthetic physics cases, two profiles and two actual world physics paths. No physical input or deployed content claim; separate shutdown required."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
