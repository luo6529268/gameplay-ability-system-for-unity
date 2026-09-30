#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4DerivedWeaponReferenceEditorTests
    {
        [Test]
        public void ExistingWeaponFrameLogicSelfCheckDoesNotSelectBeforePhysics()
        {
            MethodInfo check = typeof(BattleRuntimeSelfCheck).GetMethod(
                "CheckFrameLifecycleWeaponFrameLogicContracts",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(check, Is.Not.Null);
            Assert.DoesNotThrow(() => check.Invoke(null, null));
        }

        [TestCase(LF2ObjectType.ThrowWeapon, 20.0, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, -20.0, false)]
        [TestCase(LF2ObjectType.Drink, 20.0, false)]
        [TestCase(LF2ObjectType.Drink, -20.0, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, 20.0, true)]
        [TestCase(LF2ObjectType.Drink, -20.0, true)]
        public void FastState1000PhysicsSelectsAction40WithoutResettingCounters(
            LF2ObjectType dataType, double vx, bool sharedFallback)
        {
            ProbeWeapon weapon = CreateFastWeapon(dataType, sharedFallback);
            PrepareImpact(weapon, 0, -20.0, vx, 0.0);
            weapon.AttackingCounter = 7;
            weapon.Trans.SyncDirectFrameData(100, 0, 13);

            Assert.That(weapon.Frame.D.hit_Fa, Is.Zero,
                "The physics rule must work without the pre-frame hit_Fa gate.");
            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(40));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(vx));
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-20.0));
            Assert.That(weapon.AttackingCounter, Is.EqualTo(7));
            Assert.That(weapon.Trans.WaitCounter, Is.EqualTo(13));
        }

        [TestCase(LF2ObjectType.ThrowWeapon, 10.0, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, -10.0, false)]
        [TestCase(LF2ObjectType.Drink, 10.0, false)]
        [TestCase(LF2ObjectType.Drink, -10.0, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, 10.0, true)]
        [TestCase(LF2ObjectType.Drink, -10.0, true)]
        public void GroundFrictionReachesExactThresholdBeforeActionSelection(
            LF2ObjectType dataType, double vx, bool sharedFallback)
        {
            ProbeWeapon weapon = CreateFastWeapon(dataType, sharedFallback);
            PrepareImpact(weapon, 0, 0.0, vx, 0.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            double identityExtra = dataType == LF2ObjectType.ThrowWeapon
                ? vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor
                : 0.0;
            Assert.That(weapon.Runtime.X, Is.EqualTo(vx + identityExtra));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(vx > 0 ? 9.0 : -9.0));
            Assert.That(weapon.Frame.N, Is.Zero);
        }

        [TestCase(LF2ObjectType.ThrowWeapon, 9.0, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, -9.0, false)]
        [TestCase(LF2ObjectType.Drink, 9.0, false)]
        [TestCase(LF2ObjectType.Drink, -9.0, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, 9.0, true)]
        [TestCase(LF2ObjectType.Drink, -9.0, true)]
        public void AirborneExactSpeedThresholdKeepsCurrentAction(
            LF2ObjectType dataType, double vx, bool sharedFallback)
        {
            ProbeWeapon weapon = CreateFastWeapon(dataType, sharedFallback);
            PrepareImpact(weapon, 0, -20.0, vx, 0.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.Zero);
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(vx));
        }

        [TestCase(20.0)]
        [TestCase(-20.0)]
        public void FastType1State1000DoesNotSelectAction40(double vx)
        {
            ProbeWeapon weapon = CreateFastWeapon(LF2ObjectType.LightWeapon, false);
            PrepareImpact(weapon, 0, -20.0, vx, 0.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);
            Assert.That(weapon.Frame.N, Is.Zero);
        }

        [TestCase(LF2ObjectType.ThrowWeapon, false)]
        [TestCase(LF2ObjectType.Drink, false)]
        [TestCase(LF2ObjectType.ThrowWeapon, true)]
        [TestCase(LF2ObjectType.Drink, true)]
        public void LandingOverridesFastSelectionUsingOriginalState1000(
            LF2ObjectType dataType, bool sharedFallback)
        {
            ProbeWeapon weapon = CreateFastWeapon(dataType, sharedFallback);
            weapon.Health.HP = 100;
            PrepareImpact(weapon, 0, -1.0, 10.0, 2.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(60),
                "Landing uses entry state 1000, not the selected action40 state 1002.");
            Assert.That(weapon.Runtime.Y, Is.Zero);
            Assert.That(weapon.Runtime.Vy, Is.Zero);
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(7.0).Within(1e-12));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(65));
        }

        [Test]
        public void StagedFormalOid600WithoutHitFaSelectsAction40InPhysics()
        {
            string path = Path.Combine(Application.dataPath,
                "NTSD/Content/LoganRuntime/decoded_dat/w/6.dat");
            Assert.That(File.Exists(path), Is.True, "Formal staged OID600 DAT is required.");
            var parsed = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(path));
            var data = new LF2CharacterData();
            foreach (var frame in parsed.Frames)
                data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
            var weapon = new ProbeWeapon { ObjectId = 600 };
            weapon.ConfigureType((int)LF2ObjectType.ThrowWeapon);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(600, data));
            weapon.ImmediateFrame(0);
            weapon.SwitchDir("right");
            PrepareImpact(weapon, 0, -20.0, 20.0, 0.0);

            Assert.That(weapon.Frame.D.state, Is.EqualTo(1000));
            Assert.That(weapon.Frame.D.hit_Fa, Is.Zero);
            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(40));
            Assert.That(weapon.Frame.D.state, Is.EqualTo(1002));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(20.0));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(0.85).Within(1e-12));
        }

        private static ProbeWeapon CreateFastWeapon(
            LF2ObjectType dataType, bool sharedFallback)
        {
            ProbeWeapon weapon = CreateWeapon(dataType, 1000, 35);
            weapon.FrameCache.GetFrameDataById(40).state = 1002;
            if (sharedFallback)
            {
                weapon.ConfigureType((int)LF2ObjectType.LightWeapon);
                weapon.Runtime.EntityType = (int)dataType;
                Assert.That(weapon.PoolWeaponTypeForSnapshot,
                    Is.EqualTo((int)LF2ObjectType.LightWeapon));
                Assert.That(weapon.GetCurrentDataObjectTypeForSimulation(),
                    Is.EqualTo((int)dataType),
                    "A type4/6 DAT identity on a type1 pool carrier must enter shared physics.");
            }
            return weapon;
        }

        [Test]
        public void PooledType1SettlesAtNegativeReference()
        {
            ProbeWeapon weapon = CreateWeapon(
                LF2ObjectType.LightWeapon,
                LF2States.WeaponThrowing,
                3);
            PrepareImpact(weapon, -2, -3.0, 8.0, 3.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(70));
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.Zero);
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(97));
        }

        [Test]
        public void PooledType2ThresholdEqualityCopiesNegativeReferenceIntoVy()
        {
            ProbeWeapon weapon = CreateWeapon(
                LF2ObjectType.HeavyWeapon,
                LF2States.HeavyWeaponInSky,
                10);
            PrepareImpact(weapon, -2, -3.0, 6.0, 9.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(20));
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(3.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(89));
        }

        [Test]
        public void PooledType4ExactReferenceContactDoesNotApplyGravity()
        {
            ProbeWeapon weapon = CreateWeapon(LF2ObjectType.ThrowWeapon, 0, 35);
            PrepareImpact(weapon, -2, -4.0, 4.0, 2.0);
            weapon.AttackingCounter = 7;

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.Zero);
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(2.0));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(100));
            Assert.That(weapon.AttackingCounter, Is.EqualTo(7));
        }

        [Test]
        public void PooledDeadType6SettlesAtNegativeReference()
        {
            ProbeWeapon weapon = CreateWeapon(LF2ObjectType.Drink, 0, 35);
            weapon.Health.HP = 0;
            PrepareImpact(weapon, -2, -3.0, 0.0, 2.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(60));
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.Zero);
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(-1));
        }

        [Test]
        public void PooledWeaponFrameCpointKind2SuppressesPhysics()
        {
            ProbeWeapon weapon = CreateWeapon(LF2ObjectType.LightWeapon, 0, 3, cpointKind: 2);
            PrepareImpact(weapon, 0, -10.0, 4.0, 2.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Runtime.X, Is.Zero);
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(2.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(100));
        }

        private static void PrepareImpact(
            ProbeWeapon weapon,
            int collisionYReference,
            double y,
            double vx,
            double vy)
        {
            weapon.Runtime.CollisionYReference = collisionYReference;
            weapon.Runtime.SetPosition(0.0, y, 0.0);
            weapon.Runtime.SetVelocity(vx, vy, 0.0);
            weapon.Runtime.WeaponFlightCounter = 100;
            weapon.Runtime.SyncIntegerPosition();
        }

        private static ProbeWeapon CreateWeapon(
            LF2ObjectType dataType,
            int state,
            int dropHurt,
            int cpointKind = 0)
        {
            LF2FrameData current = Frame(0, state);
            if (cpointKind != 0)
                current.cpoint = new CatchPoint { kind = cpointKind };

            var data = new LF2CharacterData
            {
                name = "B4DerivedWeaponReference",
                weapon_drop_hurt = dropHurt,
                frames = new List<LF2FrameData>
                {
                    current,
                    Frame(20, LF2States.HeavyWeaponOnGround),
                    Frame(40, state),
                    Frame(60, LF2States.WeaponOnGround),
                    Frame(70, LF2States.WeaponOnGround),
                },
            };
            var weapon = new ProbeWeapon { ObjectId = 8303 };
            weapon.ConfigureType((int)dataType);
            weapon.FrameCache.Load(
                new LF2CharacterDataWrapper(weapon.ObjectId, data));
            weapon.ImmediateFrame(0);
            weapon.SwitchDir("right");
            return weapon;
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
            };
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void ConfigureType(int dataType)
            {
                SetWeaponType(dataType);
            }
        }
    }
}
#endif
