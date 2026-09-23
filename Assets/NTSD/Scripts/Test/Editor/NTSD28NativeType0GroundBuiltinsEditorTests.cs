#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeType0GroundBuiltinsEditorTests
    {
        private const int KeyUp = 0;
        private const int KeyDown = 1;
        private const int KeyLeft = 2;
        private const int KeyRight = 3;
        private const int KeyAttack = 4;
        private const int KeyJump = 5;
        private const int KeyDefend = 6;

        private const int EdgeAttack = 0;
        private const int EdgeJump = 1;
        private const int EdgeDefend = 2;

        [Test]
        public void Movement_UsesCustomSequenceAndSharesWalkRunCounter()
        {
            LF2CharacterData customData = Data(
                Frame(0, 0),
                Frame(500, 1),
                Frame(501, 1),
                Frame(650, 2),
                Frame(651, 2),
                Frame(652, 2));
            customData.walking_frame_rate = 3;
            customData.running_frame_rate = 3;
            customData.walking_frames.AddRange(new[] { 500, 501 });
            customData.running_frames.AddRange(new[] { 650, 651, 652 });
            using (CharacterScope custom = CreateScope(customData))
            {
                custom.Input.Current[KeyRight] = 1;
                custom.Input.Previous[KeyRight] = 1;

                Assert.That(custom.Writer.RouteNativeGroundBuiltins(custom.Character),
                    Is.True);
                Assert.That(custom.Character.Frame.N, Is.EqualTo(500));

                SetFrame(custom.Character, 650);
                custom.Character.Runtime.AnimCounter = 2;
                custom.Writer.RouteNativeGroundBuiltins(custom.Character);
                Assert.That(custom.Character.Frame.N, Is.EqualTo(651));
            }

            LF2CharacterData fallbackData = StandardGroundData();
            fallbackData.walking_frame_rate = 3;
            fallbackData.running_frame_rate = 3;
            using CharacterScope fallback = CreateScope(fallbackData);
            fallback.Input.Current[KeyUp] = 1;
            fallback.Input.Previous[KeyUp] = 1;
            for (int tick = 0; tick < 5; tick++)
                fallback.Writer.RouteNativeGroundBuiltins(fallback.Character);
            Assert.That(fallback.Character.Runtime.AnimCounter, Is.EqualTo(5));

            SetFrame(fallback.Character, 9);
            fallback.Input.Current[KeyUp] = 0;
            fallback.Input.Current[KeyRight] = 1;
            fallback.Writer.RouteNativeGroundBuiltins(fallback.Character);
            Assert.That(fallback.Character.Runtime.AnimCounter, Is.EqualTo(6));
            Assert.That(fallback.Character.Frame.N, Is.EqualTo(11));
        }

        [Test]
        public void FrameCache_AddressesFormalAuthorityMaximumAndRejectsExclusiveBoundary()
        {
            LF2FrameData maximum = Frame(856, 3);
            LF2FrameData exclusive = Frame(857, 3);
            LF2CharacterData data = Data(maximum, exclusive);
            var cache = new LF2FrameCache();

            cache.Load(new LF2CharacterDataWrapper(856, data));

            Assert.That(LF2FrameCache.MaxFrameIdExclusive, Is.EqualTo(857));
            Assert.That(cache.HasFrame(856), Is.True);
            Assert.That(cache.GetFrameDataById(856), Is.SameAs(maximum));
            Assert.That(cache.HasFrame(857), Is.False);
            Assert.That(cache.GetFrameDataById(857), Is.Null);
        }

        [Test]
        public void StandingActions_RequireCurrentButtonAndPositiveBuffer()
        {
            using CharacterScope scope = CreateScope(StandardGroundData());
            scope.World.NativeRandom.ResetFromSeed(1u);
            scope.Character.Runtime.AnimSub = 6;
            scope.Character.Runtime.AttackingCounter = 7;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 4;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(65));
            Assert.That(scope.Character.Runtime.AnimSub, Is.Zero);
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);
            NTSD28NativeRandomScalarState random =
                scope.World.NativeRandom.CaptureScalarState();
            Assert.That(random.SynchronizedCalls, Is.EqualTo(1UL));
            Assert.That(random.LastSynchronizedCallSite, Is.EqualTo(0x82u));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Input.Previous[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 2;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.Zero);
            Assert.That(scope.World.NativeRandom.CaptureScalarState()
                .SynchronizedCalls, Is.EqualTo(1UL));
        }

        [Test]
        public void DefendAndSpecialGroundPrelude_MatchNativeGatesAndOrder()
        {
            using CharacterScope scope = CreateScope(StandardGroundData());
            scope.Input.Current[KeyDefend] = 1;
            scope.Input.EdgeWindow[EdgeDefend] = 3;
            scope.Input.DefendReentryCooldown = 1;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.Zero);

            scope.Input.DefendReentryCooldown = 0;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(110));

            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Input.Current[KeyRight] = 1;
            scope.Input.Current[KeyLeft] = 1;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("left"));

            SetFrame(scope.Character, 19);
            scope.Character.Runtime.Y = 0.0;
            scope.Character.PS.groundY = 0f;
            scope.Character.Runtime.Vz = 9.0;
            scope.Input.Clear();
            scope.Input.Current[KeyUp] = 1;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(-3.3).Within(0.000001));

            SetFrame(scope.Character, 301);
            scope.Input.Current[KeyUp] = 0;
            scope.Input.Current[KeyDown] = 1;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(3.3).Within(0.000001));
        }

        [Test]
        public void WalkAndRun_ApplyNativeDoubleTapAndDiagonalScaling()
        {
            using CharacterScope simultaneous = CreateScope(StandardGroundData());
            simultaneous.World.NativeRandom.ResetFromSeed(1u);
            simultaneous.Character.Runtime.AnimSub = 5;
            simultaneous.Input.Current[KeyRight] = 1;
            simultaneous.Input.Current[KeyAttack] = 1;
            simultaneous.Input.EdgeWindow[EdgeAttack] = 5;

            simultaneous.Writer.RouteNativeGroundBuiltins(simultaneous.Character);

            Assert.That(simultaneous.Character.Frame.N, Is.EqualTo(65));
            Assert.That(simultaneous.Character.Runtime.Vx, Is.EqualTo(4.0).Within(0.000001));
            Assert.That(simultaneous.Character.Runtime.AnimSub, Is.Zero);

            using CharacterScope pureRun = CreateScope(StandardGroundData());
            pureRun.Character.Runtime.AnimSub = 5;
            pureRun.Input.Current[KeyRight] = 1;
            pureRun.Writer.RouteNativeGroundBuiltins(pureRun.Character);
            Assert.That(pureRun.Character.Frame.N, Is.EqualTo(9));
            Assert.That(pureRun.Character.Runtime.Vx, Is.EqualTo(18.0).Within(0.000001));
            Assert.That(pureRun.Character.Runtime.AnimCounter, Is.EqualTo(1));

            using CharacterScope walkDiagonal = CreateScope(StandardGroundData());
            walkDiagonal.Input.Current[KeyRight] = 1;
            walkDiagonal.Input.Current[KeyUp] = 1;
            walkDiagonal.Input.Previous[KeyRight] = 1;
            walkDiagonal.Writer.RouteNativeGroundBuiltins(walkDiagonal.Character);
            Assert.That(walkDiagonal.Character.Runtime.Vx,
                Is.EqualTo(4.0 / 1.4).Within(0.000001));
            Assert.That(walkDiagonal.Character.Runtime.Vz,
                Is.EqualTo(-2.0).Within(0.000001));

            SetFrame(walkDiagonal.Character, 9);
            walkDiagonal.Input.Current[KeyUp] = 0;
            walkDiagonal.Input.Current[KeyDown] = 1;
            walkDiagonal.Writer.RouteNativeGroundBuiltins(walkDiagonal.Character);
            Assert.That(walkDiagonal.Character.Runtime.Vx,
                Is.EqualTo(18.0 / 1.2).Within(0.000001));
            Assert.That(walkDiagonal.Character.Runtime.Vz,
                Is.EqualTo(3.3).Within(0.000001));
        }

        [Test]
        public void ConfiguredFixedViewScalesNormalAndHeavyRunningWithoutChangingAuthoredData()
        {
            LF2CharacterData data = StandardGroundData();
            using CharacterScope scope = CreateScope(data, LinkedData());
            scope.World.ConfigureFixedViewRunDistance(2048, 1152);

            SetFrame(scope.Character, 9);
            scope.Character.SwitchDir("right");
            scope.Input.Current[KeyRight] = 1;
            scope.Input.Current[KeyUp] = 1;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(18.0 / 1.2).Within(1e-10));
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(-3.3).Within(1e-6));

            SetFrame(scope.Character, 16);
            scope.Input.Clear();
            scope.Character.Runtime.LinkState = 2;
            scope.Character.Runtime.TargetSlotIndex = 1;
            scope.Input.Current[KeyLeft] = 1;
            scope.Input.Current[KeyDown] = 1;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(-12.0 / 1.2).Within(1e-10));
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(4.5).Within(1e-6));
            Assert.That(data.running_speed, Is.EqualTo(18f));
            Assert.That(data.heavy_running_speed, Is.EqualTo(12f));
        }

        [Test]
        public void NativeDirectGroundActions_BypassLockCostAndMirrorWithExactRestarts()
        {
            using CharacterScope scope = CreateScope(StandardGroundData());
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.InputLastAction144 = 77;
            scope.Character.Runtime.AttackingCounter = 9;
            scope.Character.Runtime.AnimCounter = 4;
            scope.Character.Runtime.AnimSub = 6;
            scope.Character.Health.PP = 10;
            scope.Input.Current[KeyUp] = 1;
            scope.Input.Previous[KeyUp] = 1;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.InRange(5, 8));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(9));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(10));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Input.Current[KeyJump] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;
            scope.Character.Runtime.AnimSub = 6;
            scope.Character.Runtime.AttackingCounter = 8;
            int counterBefore = scope.Character.Runtime.AnimCounter;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(210));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);
            Assert.That(scope.Character.Runtime.AnimSub, Is.Zero);
            Assert.That(scope.Character.Runtime.AnimCounter, Is.EqualTo(counterBefore));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));

            SetFrame(scope.Character, 9);
            scope.Input.Clear();
            scope.Input.EdgeWindow[EdgeDefend] = 5;
            scope.Character.Runtime.AttackingCounter = 6;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(102));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(6));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));
        }

        [Test]
        public void NativeGroundResourcePolicies_ClampStandAndGateRunAttack()
        {
            using CharacterScope scope = CreateScope(StandardGroundData());
            scope.World.NativeRandom.ResetFromSeed(1u);
            scope.Character.Health.PP = 10;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(65));
            Assert.That(scope.Character.Health.PP, Is.Zero);
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350, Is.Zero);

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.HitConfirmEa = 2;
            scope.Character.Health.PP = 10;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(70));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(10));

            SetFrame(scope.Character, 9);
            scope.Input.Clear();
            scope.Character.Runtime.HitConfirmEa = 0;
            scope.Character.Health.PP = 20;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.InRange(9, 11));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(20));

            SetFrame(scope.Character, 9);
            scope.Input.Clear();
            scope.Character.Health.PP = 40;
            scope.Character.Runtime.InputMpConsumedTotal350 = 0;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(85));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(10));
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350,
                Is.EqualTo(30));
        }

        [Test]
        public void StandingLinkedSelectors_UseExactStatsRngSitesAndFallbacks()
        {
            LF2CharacterData linkedData = LinkedData();
            using CharacterScope scope = CreateScope(StandardGroundData(), linkedData);
            scope.World.NativeRandom.ResetFromSeed(1u);
            scope.Character.Runtime.LinkState = 101;
            scope.Character.Runtime.TargetSlotIndex = 1;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(121));
            Assert.That(scope.World.NativeRandom.CaptureScalarState()
                .LastSynchronizedCallSite, Is.EqualTo(0x83u));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Input.Current[KeyRight] = 1;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            ulong calls = scope.World.NativeRandom.CaptureScalarState().SynchronizedCalls;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(145));
            Assert.That(scope.World.NativeRandom.CaptureScalarState()
                .SynchronizedCalls, Is.EqualTo(calls));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.LinkState = 201;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(
                scope.Character.Frame.N,
                Is.EqualTo(120).Or.EqualTo(121));
            Assert.That(scope.World.NativeRandom.CaptureScalarState()
                .LastSynchronizedCallSite, Is.EqualTo(0x84u));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.LinkState = 6;
            scope.Character.Runtime.TargetSlotIndex = -1;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(55));
        }

        [Test]
        public void RelationState2_UsesHeavyMovementAndLinkedThrowSelectors()
        {
            using CharacterScope scope = CreateScope(StandardGroundData(), LinkedData());
            scope.Character.Runtime.LinkState = 2;
            scope.Character.Runtime.TargetSlotIndex = 1;
            scope.Input.Current[KeyRight] = 1;
            scope.Input.Previous[KeyRight] = 1;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.InRange(12, 15));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(6.0).Within(0.000001));

            SetFrame(scope.Character, 12);
            scope.Input.Clear();
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(150));

            SetFrame(scope.Character, 16);
            scope.Input.Clear();
            scope.Input.Current[KeyLeft] = 1;
            scope.Input.Current[KeyUp] = 1;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.InRange(16, 18));
            Assert.That(scope.Character.Runtime.Vx,
                Is.EqualTo(-12.0 / 1.2).Within(0.000001));
            Assert.That(scope.Character.Runtime.Vz,
                Is.EqualTo(-4.5).Within(0.000001));

            SetFrame(scope.Character, 16);
            scope.Input.Clear();
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(160));
        }

        [Test]
        public void RunningLinkedSelectorsAndJumpMotionMatchNativeBranches()
        {
            using CharacterScope scope = CreateScope(StandardGroundData(), LinkedData());
            scope.Character.Runtime.LinkState = 101;
            scope.Character.Runtime.TargetSlotIndex = 1;
            SetFrame(scope.Character, 9);
            scope.Input.EdgeWindow[EdgeAttack] = 5;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(135));

            SetFrame(scope.Character, 9);
            scope.Input.Clear();
            scope.Input.Current[KeyRight] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(145));

            SetFrame(scope.Character, 9);
            scope.Input.Clear();
            scope.Character.Runtime.LinkState = 0;
            scope.Character.SwitchDir("left");
            scope.Character.Runtime.AnimCounter = 5;
            scope.Character.Runtime.AnimSub = 17;
            scope.Input.Current[KeyUp] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;
            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(213));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(-15.0).Within(0.000001));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-13.0).Within(0.000001));
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(-3.75).Within(0.000001));
            Assert.That(scope.Character.Runtime.AnimCounter, Is.EqualTo(6));
            Assert.That(scope.Character.Runtime.AnimSub, Is.Zero);
        }

        [Test]
        public void GroundCore_IsNotTypeGatedAndRejectsUnownedState()
        {
            using CharacterScope scope = CreateScope(StandardGroundData());
            scope.Character.Runtime.ObjType = 3;
            scope.Input.Current[KeyUp] = 1;
            scope.Input.Previous[KeyUp] = 1;
            Assert.That(scope.Writer.RouteNativeGroundBuiltins(scope.Character),
                Is.True);
            Assert.That(scope.Character.Frame.N, Is.InRange(5, 8));

            SetFrame(scope.Character, 300);
            scope.Character.Runtime.Vx = 7.0;
            scope.Input.Clear();
            Assert.That(scope.Writer.RouteNativeGroundBuiltins(scope.Character),
                Is.False);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(300));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(7.0));
        }

        [Test]
        public void WarmGroundRoute_AllocatesZeroManagedBytes()
        {
            using CharacterScope scope = CreateScope(StandardGroundData());
            SetFrame(scope.Character, 19);
            scope.Character.Runtime.Y = 0.0;
            scope.Character.PS.groundY = 0f;
            scope.Input.Current[KeyUp] = 1;

            scope.Writer.RouteNativeGroundBuiltins(scope.Character);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int owned = 0;
            for (int index = 0; index < 4096; index++)
            {
                if (scope.Writer.RouteNativeGroundBuiltins(scope.Character))
                    owned++;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(owned, Is.EqualTo(4096));
            Assert.That(allocated, Is.Zero);
        }

        private static LF2CharacterData StandardGroundData()
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, 0),
                Frame(5, 1), Frame(6, 1), Frame(7, 1), Frame(8, 1),
                Frame(9, 2), Frame(10, 2), Frame(11, 2),
                Frame(12, 0), Frame(13, 0), Frame(14, 0), Frame(15, 0),
                Frame(16, 2), Frame(17, 2), Frame(18, 2),
                Frame(19, 19), Frame(20, 3), Frame(25, 3),
                Frame(35, 3), Frame(45, 3), Frame(50, 3), Frame(55, 3),
                Frame(60, 3, 30), Frame(65, 3, 30), Frame(70, 3, 30),
                Frame(80, 3, 30), Frame(85, 3, 30),
                Frame(102, 3, 30), Frame(110, 7, 30),
                Frame(120, 3), Frame(121, 3), Frame(130, 3),
                Frame(135, 3), Frame(145, 3), Frame(150, 3),
                Frame(152, 3), Frame(155, 3), Frame(160, 3),
                Frame(210, 4, 30), Frame(213, 5), Frame(218, 3),
                Frame(301, 301), Frame(300, 3),
            };
            LF2CharacterData data = Data(frames.ToArray());
            data.walking_frame_rate = 3;
            data.walking_speed = 4f;
            data.walking_speedz = 2f;
            data.running_frame_rate = 3;
            data.running_speed = 18f;
            data.running_speedz = 3.3f;
            data.heavy_walking_speed = 6f;
            data.heavy_walking_speedz = 3f;
            data.heavy_running_speed = 12f;
            data.heavy_running_speedz = 4.5f;
            data.dash_distance = 15f;
            data.dash_height = -13f;
            data.dash_distancez = 3.75f;
            return data;
        }

        private static LF2CharacterData LinkedData()
        {
            LF2CharacterData data = Data(Frame(0, 0));
            data.normal_attack1 = 120;
            data.normal_attack2 = 121;
            data.light_throw = 145;
            data.weapon_drink = 155;
            data.heavy_throw = 150;
            data.run_heavy_throw = 160;
            data.run_attack = 135;
            data.jump_attack = 130;
            data.sky_light_throw = 152;
            return data;
        }

        private static LF2CharacterData Data(params LF2FrameData[] frames)
        {
            return new LF2CharacterData
            {
                name = "NativeGroundBuiltins",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>(frames),
            };
        }

        private static LF2FrameData Frame(int id, int state, int mp = 0)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
                mp = mp,
            };
        }

        private static CharacterScope CreateScope(
            LF2CharacterData data,
            LF2CharacterData linkedData = null)
        {
            var world = new SimulationWorld();
            world.NativeRandom.ResetFromSeed(1u);
            LF2Character character = CreateCharacter(data, 0, 840);
            world.Register(character);
            LF2Character linked = null;
            if (linkedData != null)
            {
                linked = CreateCharacter(linkedData, 1, 841);
                world.Register(linked);
            }
            return new CharacterScope(world, character, linked);
        }

        private static LF2Character CreateCharacter(
            LF2CharacterData data,
            int slot,
            int objectId)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.WriteCurrentFrameId(data.frames[0].frameId);
            character.Frame.D = data.frames[0];
            character.Frame.PN = data.frames[0].frameId;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(slot);
            character.Team = slot + 1;
            character.RelationTeam = slot + 1;
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Controller = new EmptyController();
            character.AiControlled = false;
            character.PS.groundY = 0f;
            return character;
        }

        private static void SetFrame(LF2Character character, int frameId)
        {
            Assert.That(character.WriteNativeInputActionUnchecked(frameId), Is.True);
            Assert.That(character.Frame.D, Is.Not.Null);
        }

        private sealed class CharacterScope : IDisposable
        {
            internal CharacterScope(
                SimulationWorld world,
                LF2Character character,
                LF2Character linked)
            {
                World = world;
                Character = character;
                Linked = linked;
            }

            internal SimulationWorld World { get; }
            internal LF2Character Character { get; }
            internal LF2Character Linked { get; }
            internal BattleCharacterActionWriter Writer => World.CharacterActionWriter;
            internal NTSD28InputProxyBlock Input => Character.Runtime.NativeInputProxy;

            public void Dispose()
            {
                if (Linked != null)
                    World.Unregister(Linked);
                World.Unregister(Character);
            }
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
