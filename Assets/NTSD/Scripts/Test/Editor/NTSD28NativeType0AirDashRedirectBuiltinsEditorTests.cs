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
    public sealed class NTSD28NativeType0AirDashRedirectBuiltinsEditorTests
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
        public void State4_UsesBufferedAttackAirGateFacingAndClampedResourcePolicy()
        {
            using CharacterScope scope = CreateScope(AirData());
            scope.Character.Runtime.Y = -10.0;
            scope.Character.Health.PP = 20;
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.InputLastAction144 = 77;
            scope.Character.Runtime.AttackingCounter = 9;
            scope.Input.EdgeWindow[EdgeAttack] = 5;

            Assert.That(scope.Writer.RouteNativeAirDashRedirectBuiltins(
                scope.Character), Is.True);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(80));
            Assert.That(scope.Character.Health.PP, Is.Zero);
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350, Is.Zero);
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.Y = -10.0;
            scope.Character.Health.PP = 200;
            scope.Character.Runtime.InputMpConsumedTotal350 = 0;
            scope.Character.SwitchDir("left");
            scope.Input.Current[KeyRight] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(80));
            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("right"));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(175));
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350,
                Is.EqualTo(25));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.Y = 0.0;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.Zero);
        }

        [Test]
        public void State4_LinkedSelectorsUseExactFamiliesFallbacksAndRestart()
        {
            using CharacterScope scope = CreateScope(AirData(), LinkedData());
            scope.Character.Runtime.Y = -10.0;
            scope.Character.Runtime.LinkState = 101;
            scope.Character.Runtime.TargetSlotIndex = 1;
            scope.Character.Runtime.AttackingCounter = 7;
            scope.Input.EdgeWindow[EdgeAttack] = 5;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(305));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.Y = -10.0;
            scope.Input.Current[KeyRight] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(306));
            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("right"));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.Y = -10.0;
            scope.Character.Runtime.LinkState = 4;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(306));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.Y = -10.0;
            scope.Character.Runtime.LinkState = 101;
            scope.Character.Runtime.TargetSlotIndex = -1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(30));

            SetFrame(scope.Character, 0);
            scope.Input.Clear();
            scope.Character.Runtime.Y = -10.0;
            scope.Input.Current[KeyLeft] = 1;
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(52));
        }

        [Test]
        public void State5_HeldForwardAttackNormalizesAndUsesGatedAdjustedResource()
        {
            using CharacterScope scope = CreateScope(AirData());
            SetFrame(scope.Character, 5);
            scope.Character.SwitchDir("left");
            scope.Character.Runtime.Vx = -14.0;
            scope.Character.Health.PP = 200;
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.InputLastAction144 = 77;
            scope.Character.Runtime.AttackingCounter = 9;
            scope.Input.Current[KeyAttack] = 1;
            scope.Input.Previous[KeyAttack] = 1;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(90));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(175));
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350,
                Is.EqualTo(25));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(9));

            SetFrame(scope.Character, 5);
            scope.Input.Clear();
            scope.Character.SwitchDir("left");
            scope.Character.Runtime.Vx = -14.0;
            scope.Character.Health.PP = 20;
            scope.Character.Runtime.InputMpConsumedTotal350 = 0;
            scope.Input.Current[KeyAttack] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(213));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(20));
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350, Is.Zero);

            SetFrame(scope.Character, 5);
            scope.Input.Clear();
            scope.Character.SwitchDir("left");
            scope.Character.Runtime.Vx = -14.0;
            scope.Character.Health.PP = 200;
            scope.Character.Runtime.InputLocalResourceEnabled49D034 = false;
            scope.Input.Current[KeyAttack] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(90));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(200));

            SetFrame(scope.Character, 5);
            scope.Input.Clear();
            scope.Character.Runtime.InputLocalResourceEnabled49D034 = true;
            scope.Character.SwitchDir("left");
            scope.Character.Runtime.Vx = 14.0;
            scope.Input.Current[KeyAttack] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(214));

            SetFrame(scope.Character, 216);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 14.0;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(216));

            SetFrame(scope.Character, 217);
            scope.Character.Runtime.Vx = -14.0;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(217));
        }

        [Test]
        public void State5_LinkedFamiliesUseCurrentAttackDirectionRulesAndVerticalKick()
        {
            using CharacterScope scope = CreateScope(AirData(), LinkedData());
            SetFrame(scope.Character, 5);
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 5.0;
            scope.Character.Runtime.Vy = -2.0;
            scope.Character.Runtime.LinkState = 101;
            scope.Character.Runtime.TargetSlotIndex = 1;
            scope.Character.Runtime.AttackingCounter = 9;
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.InputLastAction144 = 77;
            scope.Input.Current[KeyAttack] = 1;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(305));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-3.0));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));

            SetFrame(scope.Character, 5);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 5.0;
            scope.Character.Runtime.Vy = -4.0;
            scope.Character.Runtime.LinkState = 4;
            scope.Input.Current[KeyAttack] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(213));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-4.0));

            SetFrame(scope.Character, 5);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 5.0;
            scope.Character.Runtime.Vy = -4.0;
            scope.Input.Current[KeyRight] = 1;
            scope.Input.Current[KeyAttack] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(306));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-5.0));

            SetFrame(scope.Character, 5);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 5.0;
            scope.Character.Runtime.Vy = -6.0;
            scope.Character.Runtime.LinkState = 6;
            scope.Input.Current[KeyUp] = 1;
            scope.Input.Current[KeyAttack] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(306));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-7.0));

            SetFrame(scope.Character, 5);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 5.0;
            scope.Character.Runtime.Vy = -8.0;
            scope.Character.Runtime.LinkState = 4;
            scope.Input.Current[KeyUp] = 1;
            scope.Input.Current[KeyDown] = 1;
            scope.Input.Current[KeyLeft] = 1;
            scope.Input.Current[KeyRight] = 1;
            scope.Input.Current[KeyAttack] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(213));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-8.0));
        }

        [Test]
        public void Action215_DashWritesBmpMotionAndReentersState5InSameTick()
        {
            using CharacterScope scope = CreateScope(AirData());
            SetFrame(scope.Character, 215);
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.AttackingCounter = 9;
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.InputLastAction144 = 77;
            scope.Character.Health.PP = 10;
            scope.Input.Current[KeyRight] = 1;
            scope.Input.Current[KeyUp] = 1;
            scope.Input.Current[KeyJump] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(213));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(15.0));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-13.0));
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(-3.75));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(9));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(10));

            SetFrame(scope.Character, 215);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.AttackingCounter = 6;
            scope.Character.Runtime.Vx = 0.0;
            scope.Character.Runtime.Vy = 0.0;
            scope.Character.Runtime.Vz = 0.0;
            scope.Input.Current[KeyLeft] = 1;
            scope.Input.Current[KeyDown] = 1;
            scope.Input.Current[KeyJump] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(213));
            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(-15.0));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-13.0));
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(3.75));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(6));
        }

        [Test]
        public void Action215_DefendAndStaleWindowsKeepDirectAndDepthSemantics()
        {
            using CharacterScope scope = CreateScope(AirData());
            SetFrame(scope.Character, 215);
            scope.Character.Runtime.AttackingCounter = 7;
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.InputLastAction144 = 77;
            scope.Character.Health.PP = 10;
            scope.Input.Current[KeyDefend] = 1;
            scope.Input.EdgeWindow[EdgeDefend] = 5;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(102));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(7));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(10));

            SetFrame(scope.Character, 215);
            scope.Input.Clear();
            scope.Character.Runtime.AttackingCounter = 2;
            scope.Character.Runtime.Vx = -8.0;
            scope.Character.Runtime.Vy = 0.0;
            scope.Character.Runtime.Vz = 0.0;
            scope.Input.EdgeWindow[EdgeJump] = 2;
            scope.Input.EdgeWindow[EdgeDefend] = 2;
            scope.Input.Current[KeyLeft] = 1;
            scope.Input.Current[KeyDown] = 1;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(215));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(-8.0));
            Assert.That(scope.Character.Runtime.Vy, Is.Zero);
            Assert.That(scope.Character.Runtime.Vz, Is.EqualTo(3.75));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(2));
        }

        [Test]
        public void States85And86OrientThenOnlyForward85Advances()
        {
            using CharacterScope scope = CreateScope(AirData());
            SetFrame(scope.Character, 503);
            scope.Character.SwitchDir("left");
            scope.Character.Runtime.Vx = 10.0;
            scope.Input.Current[KeyRight] = 1;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("right"));
            Assert.That(scope.Character.Frame.N, Is.EqualTo(503));

            SetFrame(scope.Character, 501);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 10.0;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(502));

            SetFrame(scope.Character, 501);
            scope.Character.Runtime.Vx = -10.0;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(501));
        }

        [Test]
        public void Rowing182And188UseRawFamilyCostMotionThresholdAndNoAccumulator()
        {
            using CharacterScope scope = CreateScope(AirData());
            SetFrame(scope.Character, 182);
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = 0.5;
            scope.Character.Runtime.Vy = 3.0;
            scope.Character.Runtime.AttackingCounter = 9;
            scope.Character.Runtime.InputLastAction144 = 77;
            scope.Character.Health.PP = 200;
            scope.Input.Current[KeyJump] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;

            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(108));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(-30.0));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-2.0));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(175));
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350, Is.Zero);
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(77));

            SetFrame(scope.Character, 188);
            scope.Input.Clear();
            scope.Character.SwitchDir("left");
            scope.Character.Runtime.Vx = -5.0;
            scope.Character.Runtime.Vy = -4.0;
            scope.Character.Runtime.AttackingCounter = 8;
            scope.Input.Current[KeyJump] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(108));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(-30.0));
            Assert.That(scope.Character.Runtime.Vy, Is.EqualTo(-4.0));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(145));
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350, Is.Zero);
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.Zero);

            SetFrame(scope.Character, 182);
            scope.Input.Clear();
            scope.Character.SwitchDir("right");
            scope.Character.Runtime.Vx = -2.0;
            scope.Character.Runtime.Vy = -4.0;
            scope.Character.Health.PP = 200;
            scope.Input.Current[KeyJump] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(100));
            Assert.That(scope.Character.Runtime.Vx, Is.EqualTo(-30.0));
        }

        [Test]
        public void RowingRedirectRejectsEachNativeGateWithoutSpendingOrRestarting()
        {
            using CharacterScope scope = CreateScope(AirData());

            PrepareRowing(scope, 188, 20);
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            AssertRowingRejected(scope, 188, 20);

            PrepareRowing(scope, 182, 200);
            scope.Input.ComboState[8] = 1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            AssertRowingRejected(scope, 182, 200);

            PrepareRowing(scope, 182, 200);
            scope.Character.Runtime.EnvironmentState320 = -1;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            AssertRowingRejected(scope, 182, 200);

            PrepareRowing(scope, 182, 200);
            scope.Character.Health.HP = 0;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            AssertRowingRejected(scope, 182, 200);

            scope.Character.Health.HP = 500;
            PrepareRowing(scope, 182, 200);
            scope.Input.Current[KeyJump] = 0;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            AssertRowingRejected(scope, 182, 200);

            PrepareRowing(scope, 182, 200);
            scope.Input.EdgeWindow[EdgeJump] = 0;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            AssertRowingRejected(scope, 182, 200);
        }

        [Test]
        public void AirCore_IsNotTypeGated_OwnsOnlyExactDomain_AndAllocatesZeroWarmBytes()
        {
            using CharacterScope scope = CreateScope(AirData());
            scope.Character.Runtime.ObjType = 3;
            scope.Character.Runtime.Y = -10.0;
            scope.Input.EdgeWindow[EdgeAttack] = 5;

            Assert.That(scope.Writer.RouteNativeAirDashRedirectBuiltins(
                scope.Character), Is.True);
            Assert.That(scope.Character.Frame.N, Is.EqualTo(80));

            SetFrame(scope.Character, 30);
            scope.Character.Runtime.AnimSub = 17;
            Assert.That(scope.Writer.RouteNativeAirDashRedirectBuiltins(
                scope.Character), Is.False);
            Assert.That(scope.Character.Runtime.AnimSub, Is.EqualTo(17));

            SetFrame(scope.Character, 503);
            scope.Input.Clear();
            scope.Character.Runtime.AnimSub = 17;
            scope.Writer.RouteNativeAirDashRedirectBuiltins(scope.Character);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int owned = 0;
            for (int index = 0; index < 4096; index++)
            {
                if (scope.Writer.RouteNativeAirDashRedirectBuiltins(
                    scope.Character))
                {
                    owned++;
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(owned, Is.EqualTo(4096));
            Assert.That(allocated, Is.Zero);
        }

        private static void PrepareRowing(
            CharacterScope scope,
            int action,
            int pp)
        {
            SetFrame(scope.Character, action);
            scope.Input.Clear();
            scope.Character.Health.HP = 500;
            scope.Character.Health.PP = pp;
            scope.Character.Runtime.EnvironmentState320 = 0;
            scope.Character.Runtime.AttackingCounter = 9;
            scope.Character.Runtime.Vx = 0.5;
            scope.Character.Runtime.Vy = 3.0;
            scope.Input.Current[KeyJump] = 1;
            scope.Input.EdgeWindow[EdgeJump] = 5;
        }

        private static void AssertRowingRejected(
            CharacterScope scope,
            int action,
            int pp)
        {
            Assert.That(scope.Character.Frame.N, Is.EqualTo(action));
            Assert.That(scope.Character.Health.PP, Is.EqualTo(pp));
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(9));
            Assert.That(scope.Character.Runtime.InputMpConsumedTotal350, Is.Zero);
        }

        private static LF2CharacterData AirData()
        {
            LF2CharacterData data = Data(
                Frame(0, 4),
                Frame(5, 5),
                Frame(30, 3), Frame(40, 3), Frame(52, 3),
                Frame(80, 3, 25), Frame(90, 3, 25),
                Frame(100, 6, 25), Frame(102, 3, 25),
                Frame(108, 6, 30), Frame(130, 3), Frame(152, 3),
                Frame(182, 12), Frame(188, 12),
                Frame(213, 5, 30), Frame(214, 5, 30),
                Frame(215, 15), Frame(216, 5), Frame(217, 5),
                Frame(305, 3), Frame(306, 3),
                Frame(501, 85), Frame(502, 85), Frame(503, 86));
            data.dash_distance = 15f;
            data.dash_height = -13f;
            data.dash_distancez = 3.75f;
            data.rowing_height = -2f;
            data.rowing_distance = 30f;
            return data;
        }

        private static LF2CharacterData LinkedData()
        {
            LF2CharacterData data = Data(Frame(0, 0));
            data.jump_attack = 305;
            data.sky_light_throw = 306;
            return data;
        }

        private static LF2CharacterData Data(params LF2FrameData[] frames)
        {
            return new LF2CharacterData
            {
                name = "NativeAirDashRedirectBuiltins",
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
            LF2Character character = CreateCharacter(data, 0, 850);
            world.Register(character);
            LF2Character linked = null;
            if (linkedData != null)
            {
                linked = CreateCharacter(linkedData, 1, 851);
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
