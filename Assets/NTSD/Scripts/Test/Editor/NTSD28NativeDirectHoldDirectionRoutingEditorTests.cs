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
    public sealed class NTSD28NativeDirectHoldDirectionRoutingEditorTests
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
        private const int EdgeRight = 3;
        private const int EdgeLeft = 4;
        private const int EdgeUp = 5;
        private const int EdgeDown = 6;

        [Test]
        public void ThreeButton_StrictOldestAgeWinsAndTieDoesNotRoute()
        {
            LF2FrameData current = Frame(0);
            current.hit_a = 20;
            current.hit_d = 30;
            current.hit_j = 40;
            using CharacterScope winner = CreateScope(
                current,
                Frame(20),
                Frame(30),
                Frame(40));
            winner.Character.Runtime.AttackingCounter = 9;
            winner.Input.EdgeWindow[EdgeAttack] = 5;
            winner.Input.EdgeWindow[EdgeDefend] = 4;
            winner.Input.EdgeWindow[EdgeJump] = 3;

            NTSD28NativeFieldRoutingResult result =
                winner.Writer.RouteNativeThreeButtonFields(winner.Character);

            Assert.That(result.AttemptCount, Is.EqualTo(1));
            Assert.That(result.AppliedCount, Is.EqualTo(1));
            Assert.That(winner.Character.Frame.N, Is.EqualTo(20));
            Assert.That(winner.Input.EdgeWindow[EdgeAttack], Is.Zero);
            Assert.That(winner.Input.EdgeWindow[EdgeDefend], Is.EqualTo(4));
            Assert.That(winner.Input.EdgeWindow[EdgeJump], Is.EqualTo(3));
            Assert.That(winner.Character.Runtime.AttackingCounter, Is.EqualTo(9));

            current = Frame(0);
            current.hit_a = 20;
            current.hit_j = 40;
            using CharacterScope tied = CreateScope(current, Frame(20), Frame(40));
            tied.Input.EdgeWindow[EdgeAttack] = 5;
            tied.Input.EdgeWindow[EdgeJump] = 5;

            result = tied.Writer.RouteNativeThreeButtonFields(tied.Character);

            Assert.That(result.AttemptCount, Is.Zero);
            Assert.That(tied.Character.Frame.N, Is.Zero);
            Assert.That(tied.Input.EdgeWindow[EdgeAttack], Is.EqualTo(5));
            Assert.That(tied.Input.EdgeWindow[EdgeJump], Is.EqualTo(5));
        }

        [Test]
        public void ThreeButton_RejectedFieldConsumesWinningAgeAndPreservesCounter()
        {
            LF2FrameData current = Frame(0);
            current.hit_a = 20;
            using CharacterScope scope = CreateScope(current, Frame(20));
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Character.Runtime.AttackingCounter = 8;
            scope.Input.EdgeWindow[EdgeAttack] = 5;

            NTSD28NativeFieldRoutingResult result =
                scope.Writer.RouteNativeThreeButtonFields(scope.Character);

            Assert.That(result.AttemptCount, Is.EqualTo(1));
            Assert.That(result.AppliedCount, Is.Zero);
            Assert.That(scope.Character.Frame.N, Is.Zero);
            Assert.That(scope.Input.EdgeWindow[EdgeAttack], Is.Zero);
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(8));
        }

        [Test]
        public void ThreeButton_HoldFieldsChainThroughCurrentFrames()
        {
            LF2FrameData frame0 = Frame(0);
            frame0.hold_a = 20;
            LF2FrameData frame20 = Frame(20);
            frame20.hold_d = 30;
            LF2FrameData frame30 = Frame(30);
            frame30.hold_j = 40;
            using CharacterScope scope = CreateScope(
                frame0,
                frame20,
                frame30,
                Frame(40));
            scope.Input.Previous[KeyAttack] = 1;
            scope.Input.Previous[KeyDefend] = 1;
            scope.Input.Previous[KeyJump] = 1;
            scope.Character.Runtime.AttackingCounter = 7;

            NTSD28NativeFieldRoutingResult result =
                scope.Writer.RouteNativeThreeButtonFields(scope.Character);

            Assert.That(result.AttemptCount, Is.EqualTo(3));
            Assert.That(result.AppliedCount, Is.EqualTo(3));
            Assert.That(scope.Character.Frame.N, Is.EqualTo(40));
            Assert.That(scope.Input.Previous[KeyAttack], Is.Zero);
            Assert.That(scope.Input.Previous[KeyDefend], Is.Zero);
            Assert.That(scope.Input.Previous[KeyJump], Is.Zero);
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(7));
        }

        [Test]
        public void ThreeButton_HoldsCompareAgainstOriginalEdgeSnapshot()
        {
            LF2FrameData frame0 = Frame(0);
            frame0.hit_a = 20;
            LF2FrameData frame20 = Frame(20);
            frame20.hold_d = 30;
            using CharacterScope scope = CreateScope(frame0, frame20, Frame(30));
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Input.Previous[KeyDefend] = 1;

            NTSD28NativeFieldRoutingResult result =
                scope.Writer.RouteNativeThreeButtonFields(scope.Character);

            Assert.That(result.AttemptCount, Is.EqualTo(1));
            Assert.That(scope.Character.Frame.N, Is.EqualTo(20));
            Assert.That(scope.Input.EdgeWindow[EdgeAttack], Is.Zero);
            Assert.That(scope.Input.Previous[KeyDefend], Is.EqualTo(1));
        }

        [Test]
        public void Direction_HorizontalUsesFacingAndConsumesWinningEdge()
        {
            LF2FrameData current = Frame(0);
            current.hit_f = 20;
            current.hit_b = 30;
            using CharacterScope right = CreateScope(current, Frame(20), Frame(30));
            right.Character.SwitchDir("right");
            right.Input.EdgeWindow[EdgeRight] = 5;
            right.Input.EdgeWindow[EdgeLeft] = 4;

            NTSD28NativeFieldRoutingResult rightResult =
                right.Writer.RouteNativeDirectionFields(right.Character);

            Assert.That(rightResult.AttemptCount, Is.EqualTo(1));
            Assert.That(right.Character.Frame.N, Is.EqualTo(20));
            Assert.That(right.Input.EdgeWindow[EdgeRight], Is.Zero);

            current = Frame(0);
            current.hit_f = 20;
            current.hit_b = 30;
            using CharacterScope left = CreateScope(current, Frame(20), Frame(30));
            left.Character.SwitchDir("left");
            left.Input.EdgeWindow[EdgeRight] = 5;
            left.Input.EdgeWindow[EdgeLeft] = 4;

            NTSD28NativeFieldRoutingResult leftResult =
                left.Writer.RouteNativeDirectionFields(left.Character);

            Assert.That(leftResult.AttemptCount, Is.EqualTo(1));
            Assert.That(left.Character.Frame.N, Is.EqualTo(30));
            Assert.That(left.Input.EdgeWindow[EdgeRight], Is.Zero);
        }

        [Test]
        public void Direction_RejectedHorizontalFieldStillConsumesWinningEdge()
        {
            LF2FrameData current = Frame(0);
            current.hit_f = 20;
            using CharacterScope scope = CreateScope(current, Frame(20));
            scope.Character.Runtime.InputActionLock130 = 1;
            scope.Input.EdgeWindow[EdgeRight] = 5;

            NTSD28NativeFieldRoutingResult result =
                scope.Writer.RouteNativeDirectionFields(scope.Character);

            Assert.That(result.AttemptCount, Is.EqualTo(1));
            Assert.That(result.AppliedCount, Is.Zero);
            Assert.That(scope.Input.EdgeWindow[EdgeRight], Is.Zero);
            Assert.That(scope.Character.Frame.N, Is.Zero);
        }

        [Test]
        public void Direction_DepthHitCleanupIsAsymmetric()
        {
            LF2FrameData current = Frame(0);
            current.hit_uz = 70;
            current.hit_dz = 80;
            using CharacterScope up = CreateScope(current, Frame(70), Frame(80));
            up.Input.Previous[KeyUp] = 1;
            up.Input.EdgeWindow[EdgeUp] = 5;

            up.Writer.RouteNativeDirectionFields(up.Character);

            Assert.That(up.Character.Frame.N, Is.EqualTo(70));
            Assert.That(up.Input.Previous[KeyUp], Is.Zero);
            Assert.That(up.Input.EdgeWindow[EdgeUp], Is.EqualTo(5));

            current = Frame(0);
            current.hit_uz = 70;
            current.hit_dz = 80;
            using CharacterScope down = CreateScope(current, Frame(70), Frame(80));
            down.Input.Previous[KeyDown] = 1;
            down.Input.EdgeWindow[EdgeDown] = 5;

            down.Writer.RouteNativeDirectionFields(down.Character);

            Assert.That(down.Character.Frame.N, Is.EqualTo(80));
            Assert.That(down.Input.Previous[KeyDown], Is.EqualTo(1));
            Assert.That(down.Input.EdgeWindow[EdgeDown], Is.Zero);
        }

        [Test]
        public void Direction_HeldDepthCleanupIsAsymmetric()
        {
            LF2FrameData current = Frame(0);
            current.hold_uz = 70;
            current.hold_dz = 80;
            using CharacterScope up = CreateScope(current, Frame(70), Frame(80));
            up.Input.Previous[KeyUp] = 1;

            up.Writer.RouteNativeDirectionFields(up.Character);

            Assert.That(up.Character.Frame.N, Is.EqualTo(70));
            Assert.That(up.Input.Previous[KeyUp], Is.EqualTo(1));
            Assert.That(up.Input.EdgeWindow[EdgeUp], Is.Zero);

            current = Frame(0);
            current.hold_uz = 70;
            current.hold_dz = 80;
            using CharacterScope down = CreateScope(current, Frame(70), Frame(80));
            down.Input.Previous[KeyDown] = 1;

            down.Writer.RouteNativeDirectionFields(down.Character);

            Assert.That(down.Character.Frame.N, Is.EqualTo(80));
            Assert.That(down.Input.Previous[KeyDown], Is.Zero);
            Assert.That(down.Input.EdgeWindow[EdgeDown], Is.Zero);
        }

        [Test]
        public void Direction_HeldHorizontalReevaluatesFacingAfterHitAction()
        {
            LF2FrameData frame0 = Frame(0);
            frame0.hit_f = -20;
            LF2FrameData frame20 = Frame(20);
            frame20.hold_f = 30;
            using CharacterScope scope = CreateScope(frame0, frame20, Frame(30));
            scope.Character.SwitchDir("right");
            scope.Input.EdgeWindow[EdgeRight] = 5;
            scope.Input.Previous[KeyLeft] = 1;

            NTSD28NativeFieldRoutingResult result =
                scope.Writer.RouteNativeDirectionFields(scope.Character);

            Assert.That(result.AttemptCount, Is.EqualTo(2));
            Assert.That(result.AppliedCount, Is.EqualTo(2));
            Assert.That(scope.Character.Frame.N, Is.EqualTo(30));
            Assert.That(scope.Character.Runtime.Dir, Is.EqualTo("left"));
            Assert.That(scope.Input.Previous[KeyLeft], Is.Zero);
        }

        [Test]
        public void ZeroFieldsAndNonCharacterTypeKeepExactRoutingContract()
        {
            using CharacterScope zero = CreateScope(Frame(0));
            zero.Input.EdgeWindow[EdgeAttack] = 5;
            zero.Input.EdgeWindow[EdgeRight] = 4;

            NTSD28NativeFieldRoutingResult three =
                zero.Writer.RouteNativeThreeButtonFields(zero.Character);
            NTSD28NativeFieldRoutingResult direction =
                zero.Writer.RouteNativeDirectionFields(zero.Character);

            Assert.That(three.AttemptCount + direction.AttemptCount, Is.Zero);
            Assert.That(zero.Input.EdgeWindow[EdgeAttack], Is.EqualTo(5));
            Assert.That(zero.Input.EdgeWindow[EdgeRight], Is.EqualTo(4));

            LF2FrameData current = Frame(0);
            current.hit_a = 20;
            using CharacterScope nonCharacter = CreateScope(current, Frame(20));
            nonCharacter.Character.Runtime.ObjType = 3;
            nonCharacter.Input.EdgeWindow[EdgeAttack] = 5;

            NTSD28NativeFieldRoutingResult routed =
                nonCharacter.Writer.RouteNativeThreeButtonFields(nonCharacter.Character);

            Assert.That(routed.AppliedCount, Is.EqualTo(1));
            Assert.That(nonCharacter.Character.Frame.N, Is.EqualTo(20));
        }

        [Test]
        public void DataOrientedSecondPassRoutesThreeButtonThenDirectionSameTick()
        {
            LF2FrameData frame0 = Frame(0);
            frame0.hit_a = 20;
            LF2FrameData frame20 = Frame(20);
            frame20.hit_f = 30;
            using CharacterScope scope = CreateScope(frame0, frame20, Frame(30));
            scope.Input.EdgeWindow[EdgeAttack] = 5;
            scope.Input.EdgeWindow[EdgeRight] = 4;
            scope.Character.Runtime.AttackingCounter = 6;

            scope.World.CharacterInputAll(2);

            Assert.That(scope.Character.Frame.N, Is.EqualTo(30));
            Assert.That(scope.Character.Runtime.InputLastAction144, Is.EqualTo(30));
            Assert.That(scope.Input.EdgeWindow[EdgeAttack], Is.Zero);
            Assert.That(scope.Input.EdgeWindow[EdgeRight], Is.Zero);
            Assert.That(scope.Character.Runtime.AttackingCounter, Is.EqualTo(6));
        }

        [Test]
        public void WarmDirectHoldDirectionRoutesAllocateZeroManagedBytes()
        {
            using CharacterScope scope = CreateScope(Frame(0));
            NTSDEntityRuntime runtime = scope.Character.Runtime;

            scope.Writer.RouteNativeThreeButtonFields(scope.Character);
            scope.Writer.RouteNativeDirectionFields(scope.Character);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int attempts = 0;
            for (int index = 0; index < 4096; index++)
            {
                attempts += scope.Writer
                    .RouteNativeThreeButtonFields(scope.Character).AttemptCount;
                attempts += scope.Writer
                    .RouteNativeDirectionFields(scope.Character).AttemptCount;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(attempts, Is.Zero);
            Assert.That(allocated, Is.Zero);
            Assert.That(runtime.NativeInputProxy.HasCanonicalStorage, Is.True);
        }

        private static LF2FrameData Frame(int frameId)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = 3,
                wait = 100,
                next = frameId,
            };
        }

        private static CharacterScope CreateScope(params LF2FrameData[] frames)
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.Runtime.Flow.InputPhase = 0;
            var data = new LF2CharacterData
            {
                name = "NativeDirectHoldDirection",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>(frames),
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = 803;
            character.FrameCache.Load(new LF2CharacterDataWrapper(803, data));
            character.WriteCurrentFrameId(frames[0].frameId);
            character.Frame.D = character.FrameCache.GetFrameDataById(frames[0].frameId);
            character.Frame.PN = frames[0].frameId;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(0);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Controller = new EmptyController();
            character.AiControlled = false;
            world.Register(character);
            return new CharacterScope(world, character);
        }

        private sealed class CharacterScope : IDisposable
        {
            internal CharacterScope(SimulationWorld world, LF2Character character)
            {
                World = world;
                Character = character;
            }

            internal SimulationWorld World { get; }
            internal LF2Character Character { get; }
            internal BattleCharacterActionWriter Writer => World.CharacterActionWriter;
            internal NTSD28InputProxyBlock Input => Character.Runtime.NativeInputProxy;

            public void Dispose()
            {
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
