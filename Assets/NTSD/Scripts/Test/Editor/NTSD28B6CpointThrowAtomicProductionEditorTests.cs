#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6CpointThrowAtomicProductionEditorTests
    {
        [Test]
        public void PositiveThrowInjury_RoutesDisplayEnvironmentAndPreservesLegacyWeaponCount()
        {
            using (ThrowScope scope = CreateScope(throwInjury: 20))
            {
                scope.Catcher.Runtime.MP = 100;
                scope.Caught.Runtime.MP = 100;
                scope.Caught.Runtime.Vz = 6f;
                scope.Caught.WeaponCount = 73;

                scope.RunThrow(up: false, down: false);

                Assert.That(scope.Caught.Runtime.DisplayScoreStep1F4, Is.EqualTo(2));
                Assert.That(scope.Caught.Runtime.DisplayDamageStep1FC, Is.EqualTo(2));
                Assert.That(scope.Caught.Runtime.DisplayCurrentHpStep204, Is.EqualTo(2));
                Assert.That(scope.Caught.Runtime.DisplayEffectiveMaxHpStep20C, Is.EqualTo(1));
                Assert.That(scope.Caught.Runtime.EnvironmentState320, Is.EqualTo(20));
                Assert.That(
                    scope.Caught.Runtime.EnvironmentSourceSlot160,
                    Is.EqualTo(scope.Caught.Runtime.SlotIndex));
                Assert.That(scope.Caught.WeaponCount, Is.EqualTo(73));
                Assert.That(scope.Caught.Runtime.Vz, Is.EqualTo(6f));
            }
        }

        [TestCase(false, false, 7f)]
        [TestCase(true, true, 7f)]
        [TestCase(true, false, -3f)]
        [TestCase(false, true, 3f)]
        public void DepthInput_OverwritesVzOnlyWhenExactlyOneDirectionIsHeld(
            bool up,
            bool down,
            float expectedVz)
        {
            using (ThrowScope scope = CreateScope(throwInjury: 0))
            {
                scope.Caught.Runtime.Vz = 7f;

                scope.RunThrow(up, down);

                Assert.That(scope.Caught.Runtime.Vz, Is.EqualTo(expectedVz));
            }
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void NonPositiveThrowInjury_DoesNotWriteResourceEnvironmentOrWeaponCount(
            int throwInjury)
        {
            using (ThrowScope scope = CreateScope(throwInjury))
            {
                scope.Catcher.Runtime.MP = 100;
                scope.Caught.Runtime.MP = 100;
                scope.Caught.Runtime.EnvironmentState320 = 41;
                scope.Caught.Runtime.EnvironmentSourceSlot160 = 42;
                scope.Caught.Runtime.DisplayScoreStep1F4 = 44;
                scope.Caught.WeaponCount = 43;

                scope.RunThrow(up: false, down: false);

                Assert.That(scope.Catcher.Runtime.MP, Is.EqualTo(100));
                Assert.That(scope.Caught.Runtime.MP, Is.EqualTo(100));
                Assert.That(scope.Caught.Runtime.EnvironmentState320, Is.EqualTo(41));
                Assert.That(scope.Caught.Runtime.EnvironmentSourceSlot160, Is.EqualTo(42));
                Assert.That(scope.Caught.Runtime.DisplayScoreStep1F4, Is.EqualTo(44));
                Assert.That(scope.Caught.WeaponCount, Is.EqualTo(43));
            }
        }

        [Test]
        public void MissingResourceOwner_SkipsDisplayButStillArmsEnvironment()
        {
            using (ThrowScope scope = CreateScope(throwInjury: 20))
            {
                scope.Catcher.Runtime.OwnerSlotIndex = 399;
                scope.Caught.Runtime.DisplayScoreStep1F4 = 44;

                scope.RunThrow(up: false, down: false);

                Assert.That(scope.Caught.Runtime.DisplayScoreStep1F4, Is.EqualTo(44));
                Assert.That(scope.Caught.Runtime.EnvironmentState320, Is.EqualTo(20));
                Assert.That(
                    scope.Caught.Runtime.EnvironmentSourceSlot160,
                    Is.EqualTo(scope.Caught.Runtime.SlotIndex));
            }
        }

        [TestCase("right", 1.5f, -2.25f, 0.125f)]
        [TestCase("left", 1.5f, -2.25f, 0.125f)]
        [TestCase("right", 0.0000001f, -0.0000002f, 0.0000003f)]
        public void Float32ThrowValuesReachRuntimeWithoutIntegerTruncation(string direction, float vx, float vy, float vz)
        {
            var point = new CatchPoint
            {
                kind = 1, x = 50, y = 60, vaction = 132,
                throwvx = vx, throwvy = vy, throwvz = vz,
            };
            using (ThrowScope scope = CreateScope(0, point))
            {
                scope.Catcher.SwitchDir(direction);
                scope.RunThrow(up: false, down: true);
                Assert.That(scope.Caught.Runtime.Vx, Is.EqualTo(direction == "right" ? (double)vx : -(double)vx));
                Assert.That(scope.Caught.Runtime.Vy, Is.EqualTo((double)vy));
                Assert.That(scope.Caught.Runtime.Vz, Is.EqualTo((double)vz));
            }
        }

        private static ThrowScope CreateScope(int throwInjury, CatchPoint suppliedPoint = null)
        {
            CatchPoint catcherPoint = new CatchPoint
            {
                kind = 1,
                x = 50,
                y = 60,
                vaction = 132,
                throwvx = 8,
                throwvy = -4,
                throwvz = 3,
                throwinjury = throwInjury,
            };
            if (suppliedPoint != null) catcherPoint = suppliedPoint;
            LF2CharacterData catcherData = new LF2CharacterData
            {
                name = "B6ThrowCatcher",
                type_sub = (int)LF2ObjectType.Character,
                definition_attacking = 200,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, null),
                    Frame(100, LF2States.Catching, 110, catcherPoint),
                    Frame(110, LF2States.Standing, 110, null),
                },
            };
            CatchPoint caughtPoint = new CatchPoint
            {
                kind = 2,
                x = 20,
                y = 30,
            };
            LF2CharacterData caughtData = new LF2CharacterData
            {
                name = "B6ThrowCaught",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, null),
                    Frame(130, LF2States.BeingCaught, 130, caughtPoint),
                    Frame(132, LF2States.BeingCaught, 132, caughtPoint),
                },
            };
            return new ThrowScope(
                new ProbeCharacter("B6ThrowCatcher", 91001, catcherData),
                new ProbeCharacter("B6ThrowCaught", 91002, caughtData));
        }

        private static LF2FrameData Frame(
            int frameId,
            int state,
            int next,
            CatchPoint cpoint)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = next,
                pic = 999,
                centerx = 39,
                centery = 79,
                cpoint = cpoint,
            };
        }

        private sealed class ThrowScope : System.IDisposable
        {
            private readonly SimulationWorld world;

            internal ThrowScope(ProbeCharacter catcher, ProbeCharacter caught)
            {
                world = new SimulationWorld();
                Catcher = catcher;
                Caught = caught;
                world.Register(Catcher);
                world.Register(Caught);
                Catcher.ImmediateFrame(100);
                Caught.ImmediateFrame(130);
                Catcher.CaughtSlotIndex = Caught.Runtime.SlotIndex;
                Caught.Runtime.CatchSourceSlot90 = Catcher.Runtime.SlotIndex;
                Caught.CatcherSlotIndex = Catcher.Runtime.SlotIndex;
                Catcher.Runtime.CaughtDuration = 300;
                Catcher.FrameDelay = 0;
                Catcher.Runtime.MPMax = 500;
                Caught.Runtime.MPMax = 500;
            }

            internal ProbeCharacter Catcher { get; }
            internal ProbeCharacter Caught { get; }

            internal void RunThrow(bool up, bool down)
            {
                Catcher.Runtime.KeyUp = up ? (byte)1 : (byte)0;
                Catcher.Runtime.KeyDown = down ? (byte)1 : (byte)0;
                world.CaptureCollisionFrameSnapshotsAll();
                Catcher.RunCpointCheckStep10();
            }

            public void Dispose()
            {
                if (Caught.Match == world)
                    world.Unregister(Caught);
                if (Catcher.Match == world)
                    world.Unregister(Catcher);
                world.FlushPendingDestroyForDiagnostics();
            }
        }

        private sealed class ProbeCharacter : LF2Character
        {
            internal ProbeCharacter(
                string characterName,
                int objectId,
                LF2CharacterData data)
            {
                Name = characterName;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                ImmediateFrame(0);
                Runtime.SetPosition(0.0, 0.0, 0.0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 500;
                Health.HPBound = 500;
                Health.HP3 = 500;
                KillCount = -1;
            }
        }
    }

}
#endif
