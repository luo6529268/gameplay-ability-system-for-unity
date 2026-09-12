#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Simulation.Presentation;
using NTSD.Animation.Rendering;
using UnityEditor;
using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6CompatWeaponActionSelectorEditorTests
    {
        // Expected values come from input_routing.cpp call sites, not the production selector.
        [TestCaseSource(nameof(Cases))]
        public void LinkedActionMatrix(BattleAiExecutionProfile profile, int relation,
            int state, int directions, int stats)
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(profile);
            world.NativeRandom.ResetFromSeed(1u);
            LF2Character character = Create(0, 840, Data());
            LF2Character linked = null;
            world.Register(character);
            try
            {
                if (stats != -1)
                {
                    LF2CharacterData data = Data();
                    if (stats != 0)
                    {
                        data.normal_attack1 = 120; data.normal_attack2 = 125;
                        data.light_throw = 145; data.weapon_drink = 155;
                        data.heavy_throw = 150; data.run_heavy_throw = 151;
                        data.run_attack = 135; data.jump_attack = 140;
                        data.sky_light_throw = 152;
                    }
                    linked = Create(1, 841, data);
                    world.Register(linked);
                    linked.Runtime.LinkState = -1;
                    linked.Runtime.HolderStableId = 0;
                }
                character.Runtime.LinkState = relation;
                character.Runtime.TargetSlotIndex = 1;
                character.Runtime.HeldWeaponStableId = 1;
                character.HeldWeaponReferenceInternal = linked;
                int initialFrame = state == 0 ? 0 : state == 2 ? 9 : state == 4 ? 212 : 213;
                character.WriteNativeInputActionUnchecked(initialFrame);
                var runtime = character.Runtime;
                runtime.Y = state >= 4 ? -10 : 0;
                runtime.YInt = (int)runtime.Y;
                runtime.Vx = 8;
                runtime.Vy = -3;
                runtime.Dir = "right";
                runtime.AnimCounter = 0;
                runtime.AnimSub = 0;
                runtime.AttackingCounter = 7;
                runtime.KeyJump = 1;
                runtime.CdAttack = 5;
                var input = runtime.NativeInputProxy;
                input.Current[4] = 1;
                input.EdgeWindow[0] = 5;
                if (directions >= 1) { input.Current[3] = 1; input.Previous[3] = 1; runtime.KeyRight = 1; runtime.PrevRight = 1; }
                if (directions >= 2) { input.Current[0] = 1; input.Previous[0] = 1; runtime.KeyUp = 1; runtime.PrevUp = 1; }
                if (directions == 3)
                {
                    input.Current[1] = input.Current[2] = 1;
                    input.Previous[1] = input.Previous[2] = 1;
                    runtime.KeyDown = runtime.KeyLeft = 1;
                    runtime.PrevDown = runtime.PrevLeft = 1;
                }
                int expected = Expected(relation, state, directions, stats == 1);
                uint site = state == 0 && relation == 0 ? 0x82u :
                    state == 0 && relation % 100 == 1 && !(relation == 101 && directions != 0)
                    ? (relation == 101 ? 0x83u : 0x84u) : 0u;
                if (profile == BattleAiExecutionProfile.DataOrientedCanonical)
                {
                    if (!world.CharacterActionWriter.RouteNativeGroundBuiltins(character))
                        world.CharacterActionWriter.RouteNativeAirDashRedirectBuiltins(character);
                }
                else
                    character.ProcessReleaseInput();
                {
                    if (expected >= 0) Assert.That(character.Frame.N, Is.EqualTo(expected), "action");
                    else Assert.That(character.Frame.N, Is.LessThan(20).Or.EqualTo(initialFrame), "out-of-domain keeps movement action");
                    var random = world.NativeRandom.CaptureScalarState();
                    Assert.That(random.SynchronizedCalls, Is.EqualTo(site == 0 ? 0ul : 1ul), "RNG count");
                    if (site != 0) Assert.That(random.LastSynchronizedCallSite, Is.EqualTo(site), "RNG site");
                    if (state == 0 || (state >= 4 && expected >= 0 && relation != 0))
                        Assert.That(runtime.AttackingCounter, Is.Zero, "frame counter");
                    if (state == 5 && expected >= 0 && relation != 0)
                        Assert.That(runtime.Vy, Is.EqualTo(-4.0), "dash motion.y -1");
                }
            }
            finally
            {
                if (linked != null) world.Unregister(linked);
                world.Unregister(character);
            }
        }

        [TestCaseSource(nameof(IndependentFields))]
        public void PureSelector_UsesOnlySelectedNonzeroField(int field, string name, int value, bool absent)
        {
            LF2CharacterData data = absent ? null : Data();
            if (data != null) typeof(LF2CharacterData).GetField(name).SetValue(data, value);
            int expected = absent || value == 0 ? 777 : value;
            Assert.That(BattleNativeLinkedWeaponActionResolver.Resolve(data,
                (BattleNativeLinkedWeaponActionField)field, 777), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> IndependentFields()
        {
            string[] fields = { "normal_attack1", "normal_attack2", "light_throw", "weapon_drink",
                "heavy_throw", "run_heavy_throw", "run_attack", "jump_attack", "sky_light_throw" };
            for (int i = 0; i < fields.Length; i++)
            foreach (int value in new[] { int.MinValue, -1, 0, 12345 })
            foreach (bool absent in new[] { false, true })
                yield return new TestCaseData(i, fields[i], value, absent);
        }

        [Test]
        public void PureSelector_WarmedZeroAllocationAndAllFallbackConstants()
        {
            int[] actual = { BattleNativeLinkedWeaponActionResolver.NormalAttack1Fallback,
                BattleNativeLinkedWeaponActionResolver.NormalAttack2Fallback,
                BattleNativeLinkedWeaponActionResolver.LightThrowFallback,
                BattleNativeLinkedWeaponActionResolver.WeaponDrinkFallback,
                BattleNativeLinkedWeaponActionResolver.HeavyThrowFallback,
                BattleNativeLinkedWeaponActionResolver.RunHeavyThrowFallback,
                BattleNativeLinkedWeaponActionResolver.RunAttackFallback,
                BattleNativeLinkedWeaponActionResolver.DashJumpAttackFallback,
                BattleNativeLinkedWeaponActionResolver.AirJumpAttackFallback,
                BattleNativeLinkedWeaponActionResolver.SkyLightThrowFallback };
            CollectionAssert.AreEqual(new[] {20,25,45,55,50,50,35,40,30,52}, actual);
            LF2CharacterData data = Data();
            data.jump_attack = -123;
            BattleNativeLinkedWeaponActionResolver.Resolve(data, BattleNativeLinkedWeaponActionField.JumpAttack, 40);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int action = 0;
            for (int i = 0; i < 4096; i++)
                action = BattleNativeLinkedWeaponActionResolver.Resolve(data, BattleNativeLinkedWeaponActionField.JumpAttack, 40);
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(bytes, Is.Zero);
            Assert.That(action, Is.EqualTo(-123));
        }

        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical)]
        public void StandingRngBothArms_ZeroAndNonzeroIgnoreOldBool(BattleAiExecutionProfile profile)
        {
            foreach (int oid in new[] { 100, 841 })
            foreach (int configured in new[] { 0, 1 })
            WithLinked(profile, 0, 1, oid, (world, character, linked) =>
            {
                Assert.That(NTSDSpec.IsWeaponAttackable(oid), Is.EqualTo(oid == 100));
                linked.FrameCache.Wrapper.characterData.normal_attack1 = configured * 120;
                linked.FrameCache.Wrapper.characterData.normal_attack2 = configured * 125;
                bool first = false, second = false;
                for (uint seed = 1; seed <= 32; seed++)
                {
                    world.NativeRandom.ResetFromSeed(seed);
                    int choice = world.NativeRandom.SynchronizedNext(0x84u, 2);
                    first |= choice == 0; second |= choice == 1;
                    world.NativeRandom.ResetFromSeed(seed);
                    character.WriteNativeInputActionUnchecked(0);
                    Attack(character, true);
                    Route(world, character, profile);
                    Assert.That(character.Frame.N, Is.EqualTo(choice == 0 ? (configured == 0 ? 20 : 120) : (configured == 0 ? 25 : 125)));
                    Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.EqualTo(1ul));
                    Assert.That(world.NativeRandom.CaptureScalarState().LastSynchronizedCallSite, Is.EqualTo(0x84u));
                }
                Assert.That(first && second, Is.True, "both synchronized selector arms exercised");
            });
        }

        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 0)]
        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 2)]
        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 4)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 0)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 2)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 4)]
        public void HeldAttack_UsesCallSiteCurrentAndBufferedGates(BattleAiExecutionProfile profile, int state)
        {
            WithLinked(profile, state, 1, 841, (world, character, linked) =>
            {
                Attack(character, false);
                Route(world, character, profile);
                Assert.That(character.Frame.N, Is.EqualTo(state == 0 ? 0 : state == 2 ? 35 : 30));
            });
        }

        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical)]
        public void Dash_RequiresCurrentJAndDirectWritePreservesWaitAndCostMetadata(BattleAiExecutionProfile profile)
        {
            WithLinked(profile, 5, 1, 841, (world, character, linked) =>
            {
                Attack(character, false);
                Route(world, character, profile);
                Assert.That(character.Frame.N, Is.EqualTo(213), "edge window alone cannot dash attack");
                linked.FrameCache.Wrapper.characterData.jump_attack = -321;
                Attack(character, true);
                character.Frame.PN = 888;
                character.Trans.SyncDirectFrameData(100, 213, 17);
                character.Runtime.InputLastAction144 = 333;
                character.Runtime.AnimCounter = 11;
                character.Runtime.InputActionLock130 = 1;
                Route(world, character, profile);
                Assert.That(character.Frame.N, Is.EqualTo(-321), "literal action, even without target frame");
                Assert.That(character.Frame.PN, Is.EqualTo(888));
                Assert.That(character.Trans.WaitCounter, Is.EqualTo(17));
                Assert.That(character.Runtime.InputLastAction144, Is.EqualTo(333));
                Assert.That(character.Runtime.AnimCounter, Is.EqualTo(11));
                Assert.That(character.Runtime.AttackingCounter, Is.Zero);
                Assert.That(character.Runtime.Vy, Is.EqualTo(-4.0));
            });
        }

        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 0)]
        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 2)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 0)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 2)]
        public void HeavyMovement_UsesSequenceFallbackPhasesAndControlCounter(BattleAiExecutionProfile profile, int state)
        {
            foreach (bool custom in new[] {false, true})
            WithLinked(profile, state, 2, 841, (world, character, linked) =>
            {
                LF2CharacterData data = character.FrameCache.Wrapper.characterData;
                if (custom)
                {
                    data.heavy_walking_frames.AddRange(new[] {12, 14});
                    data.heavy_running_frames.AddRange(new[] {16, 18});
                }
                int[] sequence = custom ? (state == 0 ? new[] {12,14} : new[] {16,18}) :
                    (state == 0 ? new[] {12,13,14,15,14,13} : new[] {16,17,18,17});
                int rate = custom ? 2 : 3;
                for (int counter = 0; counter < sequence.Length * rate; counter++)
                {
                    character.WriteNativeInputActionUnchecked(state == 0 ? 12 : 16);
                    character.Runtime.AnimCounter = counter;
                    character.Runtime.AnimSub = 0;
                    character.Runtime.KeyRight = character.Runtime.PrevRight = 1;
                    character.Runtime.NativeInputProxy.Current[3] = character.Runtime.NativeInputProxy.Previous[3] = 1;
                    Route(world, character, profile);
                    Assert.That(character.Frame.N, Is.EqualTo(sequence[((counter + 1) / rate) % sequence.Length]));
                    Assert.That(character.Runtime.AnimCounter, Is.EqualTo((counter + 1) % (sequence.Length * rate)));
                }
                character.Runtime.KeyRight = 0;
                character.Runtime.NativeInputProxy.Current[3] = 0;
                character.Runtime.AnimCounter = 11;
                character.WriteNativeInputActionUnchecked(state == 0 ? 12 : 16);
                Attack(character, true);
                Route(world, character, profile);
                Assert.That(character.Frame.N, Is.EqualTo(50));
                Assert.That(character.Runtime.AnimCounter, Is.EqualTo(state == 0 ? 0 : 11));
            });
        }

        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical)]
        public void HeldRunning_PreservesActionFrameCounter(BattleAiExecutionProfile profile)
        {
            WithLinked(profile, 2, 6, 122, (world, character, linked) =>
            {
                character.Runtime.AttackingCounter = 7;
                Attack(character, true);
                Route(world, character, profile);
                Assert.That(character.Frame.N, Is.EqualTo(55));
                Assert.That(character.Runtime.AttackingCounter, Is.EqualTo(7));
            });
        }

        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, -0.5, 0.0)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, -0.5, 0.0)]
        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 2.0, 3.0)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 2.0, 3.0)]
        public void HeldState4_UsesCollisionReference(BattleAiExecutionProfile profile, double y, double floor)
        {
            WithLinked(profile, 4, 6, 122, (world, character, linked) =>
            {
                character.Runtime.Y = y; character.Runtime.YInt = (int)y; character.PS.groundY = (float)floor;
                Attack(character, true); Route(world, character, profile);
                Assert.That(character.Frame.N, Is.EqualTo(52));
            });
        }

        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 1, 0)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 1, 0)]
        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 2, 0)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 2, 0)]
        [TestCase(BattleAiExecutionProfile.DataOrientedCanonical, 2, 2)]
        [TestCase(BattleAiExecutionProfile.LegacyCanonical, 2, 2)]
        public void HeavyStanding_BufferedJumpAndDefendLock(BattleAiExecutionProfile profile, int edge, int cooldown)
        {
            WithLinked(profile, 0, 2, 150, (world, character, linked) =>
            {
                character.Runtime.AnimCounter = 11;
                character.Runtime.NativeInputProxy.EdgeWindow[edge] = 5;
                if (edge == 1) character.Runtime.CdJump = 5; else character.Runtime.CdDefend = 5;
                character.Runtime.CdDefendLock = (byte)cooldown;
                character.Runtime.NativeInputProxy.DefendReentryCooldown = (byte)cooldown;
                Route(world, character, profile);
                Assert.That(character.Frame.N, Is.EqualTo(edge == 1 ? 210 : cooldown == 0 ? 110 : 12));
                Assert.That(character.Runtime.AnimCounter, Is.EqualTo(cooldown == 0 ? 0 : 11));
            });
        }

        private static void Attack(LF2Character character, bool current)
        {
            character.Runtime.KeyJump = current ? (byte)1 : (byte)0;
            character.Runtime.CdAttack = 5;
            character.Runtime.NativeInputProxy.Current[4] = current ? (byte)1 : (byte)0;
            character.Runtime.NativeInputProxy.EdgeWindow[0] = 5;
        }

        private static void Route(SimulationWorld world, LF2Character character, BattleAiExecutionProfile profile)
        {
            if (profile == BattleAiExecutionProfile.LegacyCanonical) character.ProcessReleaseInput();
            else if (!world.CharacterActionWriter.RouteNativeGroundBuiltins(character))
                world.CharacterActionWriter.RouteNativeAirDashRedirectBuiltins(character);
        }

        private static void WithLinked(BattleAiExecutionProfile profile, int state, int relation, int oid,
            Action<SimulationWorld, LF2Character, LF2Character> test)
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(profile);
            world.NativeRandom.ResetFromSeed(1u);
            LF2Character character = Create(0, 840, Data()), linked = Create(1, oid, Data());
            world.Register(character); world.Register(linked);
            try
            {
                character.Runtime.LinkState = relation; character.Runtime.TargetSlotIndex = 1;
                character.Runtime.HeldWeaponStableId = 1; character.HeldWeaponReferenceInternal = linked;
                linked.Runtime.LinkState = -1; linked.Runtime.HolderStableId = 0;
                character.WriteNativeInputActionUnchecked(state == 0 ? 0 : state == 2 ? 9 : state == 4 ? 212 : 213);
                character.Runtime.Y = state >= 4 ? -10 : 0;
                character.Runtime.YInt = (int)character.Runtime.Y;
                character.Runtime.Vx = 8; character.Runtime.Vy = -3; character.Runtime.Dir = "right";
                test(world, character, linked);
            }
            finally { world.Unregister(linked); world.Unregister(character); }
        }

        private static IEnumerable<TestCaseData> Cases()
        {
            foreach (var profile in new[] { BattleAiExecutionProfile.DataOrientedCanonical, BattleAiExecutionProfile.LegacyCanonical })
            foreach (int relation in new[] { 0, 1, 101, 2, 4, 6, 3 })
            foreach (int state in new[] { 0, 2, 4, 5 })
            foreach (int direction in new[] { 0, 1, 2, 3 })
            foreach (int stats in new[] { -1, 0, 1 })
                yield return new TestCaseData(profile, relation, state, direction, stats);
        }

        private static int Expected(int relation, int state, int directions, bool custom)
        {
            bool any = directions != 0;
            if (relation == 0) return state == 0 ? 65 : state == 2 ? 85 : state == 4 ? 80 : 90;
            if (relation == 2 && state < 4) return custom ? (state == 0 ? 150 : 151) : 50;
            if (state == 0)
            {
                if (relation == 101 && any) return custom ? 145 : 45;
                if (relation % 100 == 1) return custom ? 125 : 25;
                if (relation == 4 || (relation == 6 && any)) return custom ? 145 : 45;
                if (relation == 6) return custom ? 155 : 55;
            }
            if (state == 2)
            {
                if (relation % 100 == 1) return any ? (custom ? 145 : 45) : (custom ? 135 : 35);
                if (relation == 4 || (relation == 6 && any)) return custom ? 145 : 45;
                if (relation == 6) return custom ? 155 : 55;
            }
            if (state == 4)
            {
                if (relation % 100 == 1 && !any) return custom ? 140 : 30;
                if (relation % 100 == 1 || relation == 4 || relation == 6) return custom ? 152 : 52;
            }
            if (state == 5)
            {
                if (relation % 100 == 1) return custom ? 140 : 40;
                if ((relation == 4 && any && directions != 3) || (relation == 6 && any)) return custom ? 152 : 52;
            }
            return state == 2 ? 85 : -1;
        }

        private static LF2CharacterData Data()
        {
            var data = new LF2CharacterData { name = "Goal19Weapon", frames = new List<LF2FrameData>(),
                walking_frame_rate = 3, running_frame_rate = 3, walking_speed = 4,
                running_speed = 8, heavy_walking_speed = 3, heavy_running_speed = 6 };
            for (int i = 0; i <= 220; i++)
                data.frames.Add(new LF2FrameData { frameId = i, state = i == 0 || (i >= 12 && i <= 15) ? 0 :
                    i >= 5 && i <= 8 ? 1 : (i >= 9 && i <= 11) || (i >= 16 && i <= 18) ? 2 :
                    i == 212 ? 4 : i == 213 || i == 214 ? 5 : 3, wait = 100, next = i });
            return data;
        }

        private static LF2Character Create(int slot, int oid, LF2CharacterData data)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = oid;
            character.FrameCache.Load(new LF2CharacterDataWrapper(oid, data));
            character.WriteCurrentFrameId(0);
            character.Frame.D = data.frames[0];
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(slot);
            character.Runtime.ObjType = 0;
            character.AiControlled = false;
            character.PS.groundY = 0;
            return character;
        }
    }
    internal static class Goal19WeaponPlayProbe
    {
        private const string Request = "Temp/Goal19_W1_Play.request";
        private static bool paused;
        private static GameConfig configClone;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureProfile()
        {
            if (!File.Exists(Request)) return;
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/NTSD/Config/GameConfig/GameConfig.asset");
            configClone = UnityEngine.Object.Instantiate(source);
            configClone.hideFlags = HideFlags.HideAndDontSave;
            configClone.BattleAiExecutionProfileName = File.ReadAllText(Request).Trim();
            GameConfig.Instance = configClone;
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode && configClone != null)
                    UnityEngine.Object.DestroyImmediate(configClone);
            };
        }

        private static void Poll()
        {
            if (!File.Exists(Request) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5) return;
            if (!paused) { driver.SetPaused(true); paused = true; return; }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
            string requested = File.ReadAllText(Request).Trim();
            var report = new Report { profile = requested, input = "Frozen producer-boundary current J / directional snapshot, attack buffer5; real driver tick. No physical keyboard claim." };
            SimulationWorld world = driver.World;
            var baseline = new List<LF2Entity>();
            world.GetAllEntities(baseline);
            int count = world.ObjectCount;
            var random = world.NativeRandom.CaptureState();
            uint seed = world.NativeRandom.CaptureScalarState().TableSeed;
            try
            {
                Require(world.AiExecutionProfile.ToString() == requested, "Profile override not applied before world creation");
                Require(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NTSD_Battle", "Wrong scene");
                Require(driver.CurrentTickIndex <= 20, "Cannot establish shared tick20 boundary");
                while (driver.CurrentTickIndex < 20)
                    Require(driver.StepOneTick(new FrameInputSet(driver.CurrentTickIndex + 1, Array.Empty<SimulationPlayerInput>()), true, false), "Driver refused warm tick");
                foreach (int oid in new[] {122,123})
                foreach (int state in new[] {0,2,4,5})
                foreach (int direction in new[] {0,1,2,3})
                    Run(driver, oid, state, direction, report);
                report.status = "PASS";
            }
            catch (Exception e) { report.status = "FAIL"; report.error = e.ToString(); }
            finally
            {
                world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(null);
                world.SetCharacterInputPassMutationOverrideForSelfCheck(null);
                var entities = new List<LF2Entity>(); world.GetAllEntities(entities);
                foreach (LF2Entity entity in entities)
                    if (!baseline.Contains(entity) && entity.Match == world) entity.FreeEntityLikeExe();
                world.FlushPendingDestroyForDiagnostics();
                world.NativeRandom.ResetFromSeed(seed); world.NativeRandom.Restore(random);
                report.cleanup = world.ObjectCount == count;
                if (!report.cleanup) report.status = "FAIL";
                File.WriteAllText("Temp/Goal19_W1_Play_" + requested + ".json", JsonUtility.ToJson(report, true));
                File.Delete(Request);
                EditorApplication.update -= Poll;
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        private static void Run(SimulationTickDriver driver, int oid, int state, int direction, Report report)
        {
            SimulationWorld world = driver.World;
            var row = new Row { oid = oid, state = state, directions = direction, seed = 424242, tick = driver.CurrentTickIndex + 1 };
            report.rows.Add(row);
            LF2Character source = null; LF2Entity weapon = null;
            try
            {
                source = Spawn(world, 2) as LF2Character;
                weapon = Spawn(world, oid);
                Require(source != null && weapon is LF2WeaponBase, "Expected actual character and weapon runtime types");
                row.actorType = source.GetType().FullName; row.weaponType = weapon.GetType().FullName;
                row.actorRenderer = source.Renderer != null; row.weaponRenderer = weapon.Renderer != null;
                LF2CharacterData stats = weapon.FrameCache.Wrapper.characterData;
                row.allStatsZero = stats.normal_attack1 == 0 && stats.normal_attack2 == 0 && stats.light_throw == 0 &&
                    stats.weapon_drink == 0 && stats.heavy_throw == 0 && stats.run_heavy_throw == 0 &&
                    stats.run_attack == 0 && stats.jump_attack == 0 && stats.sky_light_throw == 0;
                Require(row.allStatsZero, "Current content nonzero: expected-value fixture must be reviewed");
                source.HoldWeapon(weapon);
                Require(source.Runtime.LinkState == 6, "Current weapon did not establish relation6");
                source.AiControlled = false;
                int frame = state == 0 ? 0 : state == 2 ? 9 : state == 4 ? 212 : 213;
                world.SetCharacterInputProducerPassMutationOverrideForSelfCheck((_, entity) =>
                {
                    entity.Runtime.KeyAttack = entity.Runtime.KeyJump = entity.Runtime.KeyDefend = 0;
                    entity.Runtime.CdAttack = entity.Runtime.CdJump = entity.Runtime.CdDefend = 0;
                    if (!ReferenceEquals(entity, source)) return;
                    source.ClearBattleEntryInputState();
                    source.WriteNativeInputActionUnchecked(frame);
                    source.Runtime.LinkState = 6;
                    source.Runtime.TargetSlotIndex = weapon.Runtime.SlotIndex;
                    source.Runtime.HeldWeaponStableId = weapon.Runtime.SlotIndex;
                    weapon.Runtime.LinkState = -1; weapon.Runtime.HolderStableId = source.Runtime.SlotIndex;
                    source.Runtime.Y = state >= 4 ? -20 : source.PS.groundY;
                    source.Runtime.YInt = (int)source.Runtime.Y;
                    source.Runtime.Vx = 8; source.Runtime.Vy = state >= 4 ? -3 : 0; source.Runtime.Dir = "right";
                    source.Runtime.AttackingCounter = 7; source.Runtime.AnimCounter = 0; source.Runtime.AnimSub = 0;
                    source.Runtime.KeyJump = 1; source.Runtime.CdAttack = 5;
                    source.Runtime.NativeInputProxy.EdgeWindow[0] = 5;
                    if (direction >= 1) source.Runtime.KeyRight = source.Runtime.PrevRight = 1;
                    if (direction >= 2) source.Runtime.KeyUp = source.Runtime.PrevUp = 1;
                    if (direction == 3) source.Runtime.KeyDown = source.Runtime.KeyLeft = source.Runtime.PrevDown = source.Runtime.PrevLeft = 1;
                    world.NativeRandom.ResetFromSeed(424242u);
                });
                world.SetCharacterInputPassMutationOverrideForSelfCheck((_, entity) =>
                {
                    if (!ReferenceEquals(entity, source)) return;
                    row.action = source.Frame.N; row.frameCounter = source.Runtime.AttackingCounter;
                    row.animSub = source.Runtime.AnimSub; row.control = source.Runtime.AnimCounter;
                    row.vy = source.Runtime.Vy;
                    row.rngCalls = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                    row.observed = true;
                });
                Require(driver.StepOneTick(new FrameInputSet(row.tick, Array.Empty<SimulationPlayerInput>()), true, true), "Driver refused witness tick");
                int expected = state < 4 ? (direction == 0 ? 55 : 45) : state == 4 ? 52 : direction == 0 ? 213 : 52;
                Require(row.observed && row.action == expected, "Current weapon action mismatch: " + row.action + " expected " + expected);
                BattlePresentationFrame framePublication = world.BattlePresentation.PublishedFrame;
                world.BattlePresentation.MaterializeCommands(framePublication, null);
                row.actorPublished = HasPublishedEntity(world, framePublication, source, out row.actorCommand, out row.actorSnapshot);
                row.weaponPublished = HasPublishedEntity(world, framePublication, weapon, out row.weaponCommand, out row.weaponSnapshot);
                Require(row.actorPublished && row.weaponPublished, "Central current-frame/catalog publication absent");
                if (state == 5 && direction != 0) Require(row.vy == -4.0 && row.frameCounter == 0, "Dash motion/counter mismatch");
                row.status = "PASS";
            }
            finally
            {
                world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(null);
                world.SetCharacterInputPassMutationOverrideForSelfCheck(null);
                if (source?.Match == world) source.FreeEntityLikeExe();
                if (weapon?.Match == world) weapon.FreeEntityLikeExe();
                world.FlushPendingDestroyForDiagnostics();
            }
        }

        private static bool HasPublishedEntity(SimulationWorld world, BattlePresentationFrame frame,
            LF2Entity entity, out bool command, out string diagnostic)
        {
            diagnostic = "no matching snapshot";
            command = false;
            if (frame == null || !world.TryGetCurrentRuntimeHandleForDiagnostics(entity.Runtime.SlotIndex,
                entity, out RuntimeEntityHandle handle)) return false;
            for (int i = 0; i < frame.CommandCount; i++)
                if (frame.GetCommand(i).Handle == handle && frame.GetCommand(i).Type == BattleRenderCommandType.Entity)
                    command = true;
            for (int i = 0; i < frame.EntityCount; i++)
            {
                var snapshot = frame.GetEntity(i);
                if (snapshot.Handle == handle)
                {
                    bool catalog = frame.BoundCatalogForAcceptance != null &&
                        frame.BoundCatalogForAcceptance.TryGet(snapshot.CurrentDatObjectId, snapshot.EffectivePic, out BattleSpriteEntry entry);
                    diagnostic = "frame=" + snapshot.FrameId + ",hasFrame=" + snapshot.HasCurrentFrame +
                        ",snapshotCatalog=" + snapshot.HasCatalogKey + ",resolvedCatalog=" + catalog +
                        ",dat=" + snapshot.CurrentDatObjectId + ",oid=" + entity.ObjectId +
                        ",pic=" + snapshot.EffectivePic + ",visible=" + snapshot.EntityVisible;
                    return snapshot.HasCurrentFrame && snapshot.CurrentDatObjectId == entity.ObjectId &&
                        (catalog || (snapshot.EffectivePic == 999 && !command));
                }
            }
            return false;
        }

        private static LF2Entity Spawn(SimulationWorld world, int oid)
        {
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            try
            {
                task.opoint = new ObjectPoint {kind = 1, oid = oid, action = 0, facing = 0};
                task.targetWorld = world;
                task.requiredRuntimeSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, world.RuntimeSlotCapacityForDiagnostics);
                task.team = 7; task.dir = "right"; task.preserveActionZero = true;
                task.skipPostInitZOffset = true; task.useDirectRuntimePosition = true;
                task.directX = 400; task.directY = 0; task.directZ = world.Runtime.Stage.ZMin + 50;
                LF2Entity entity = LF2ObjectPointFactory.Instance.CreateObjectImmediate(task);
                Require(entity != null, "Production spawn failed for " + oid);
                entity.FrameDelay = 1000;
                return entity;
            }
            finally { LF2ReferencePool.Instance.Recycle(task); }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        [Serializable] private sealed class Report
        {
            public string status, error, profile, input;
            public bool cleanup;
            public List<Row> rows = new List<Row>();
        }
        [Serializable] private sealed class Row
        {
            public string status, actorType, weaponType, actorSnapshot, weaponSnapshot;
            public int oid, state, directions, seed, tick, action, frameCounter, animSub, control;
            public bool observed, allStatsZero, actorRenderer, weaponRenderer, actorPublished, weaponPublished, actorCommand, weaponCommand;
            public double vy;
            public ulong rngCalls;
        }
    }

}
#endif
