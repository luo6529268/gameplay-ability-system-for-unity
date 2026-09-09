#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4Type3Oid999ReferenceEditorTests
    {
        [Test]
        public void ParserAndConverterPreserveFrameHitG()
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(@"
<frame> 0 native
  pic: 0 state: 3006 wait: 1 next: 0 hit_g: 77
<frame_end>");

            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(dat.Frames[0]);

            Assert.That(frame.hit_g, Is.EqualTo(77));
            Assert.That(frame.rawProperties["hit_g"], Is.EqualTo("77"));
        }

        [Test]
        public void SharedType3SpecialStateWithHitGClampsAndClearsMotionWithoutCounterReset()
        {
            ProbeSpecial entity = CreateSpecial(8304, 3006, 77);
            Prepare(entity, -20, -21.0, 3.0, 2.0, 4.0, 5);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(77));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-20.0));
            Assert.That(entity.Runtime.Vx, Is.Zero);
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.Runtime.Vz, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.EqualTo(5));
        }

        [Test]
        public void SharedType3SpecialStateWithoutHitGOnlyClampsY()
        {
            ProbeSpecial entity = CreateSpecial(8304, 3006, 0);
            Prepare(entity, -20, -21.0, 3.0, 2.0, 4.0, 5);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.Zero);
            Assert.That(entity.Runtime.Y, Is.EqualTo(-20.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(3.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(2.0));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(4.0));
            Assert.That(entity.AttackingCounter, Is.EqualTo(5));
        }

        [Test]
        public void SharedType3RealOid999OverridesSpecialHelperWithNativeLaunch()
        {
            ProbeSpecial entity = CreateSpecial(999, 3006, 77);
            Prepare(entity, -20, -15.0, 3.0, 1.0, 4.0, 5);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(101));
            Assert.That(entity.Runtime.Y, Is.EqualTo(9.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(9.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(9.0));
            Assert.That(entity.Runtime.Vz, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void SharedType5RealOid999LaunchesAndPreservesPostFrictionVz()
        {
            ProbeOther entity = CreateOther(999, alias: 0);
            Prepare(entity, -20, -15.0, 3.0, 1.0, 4.0, 5);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(101));
            Assert.That(entity.Runtime.Y, Is.EqualTo(9.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(9.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(9.0));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(3.0));
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void Alias999WithoutRealOidUsesOrdinaryType5ReferenceCore()
        {
            ProbeOther entity = CreateOther(8305, alias: 999);
            Prepare(entity, -20, -15.0, 3.0, 1.0, 4.0, 5);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.Zero);
            Assert.That(entity.Runtime.Y, Is.EqualTo(-14.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(2.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(1.0));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(3.0));
            Assert.That(entity.AttackingCounter, Is.EqualTo(5));
        }

        [Test]
        public void DerivedType3UsesTheSameSpecialStateBody()
        {
            ProbeWeapon entity = CreateDerivedType3(8306, 3007, 88);
            Prepare(entity, -20, -21.0, 3.0, 2.0, 4.0, 6);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(88));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-20.0));
            Assert.That(entity.Runtime.Vx, Is.Zero);
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.Runtime.Vz, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.EqualTo(6));
        }

        private static void Prepare(
            LF2Entity entity,
            int reference,
            double y,
            double vx,
            double vy,
            double vz,
            int attackingCounter)
        {
            entity.Runtime.CollisionYReference = reference;
            entity.Runtime.SetPosition(0.0, y, 0.0);
            entity.Runtime.SetVelocity(vx, vy, vz);
            entity.Runtime.SyncIntegerPosition();
            entity.AttackingCounter = attackingCounter;
        }

        private static ProbeSpecial CreateSpecial(int objectId, int state, int hitG)
        {
            var entity = new ProbeSpecial { ObjectId = objectId };
            Load(entity, objectId, state, hitG, alias: 0);
            return entity;
        }

        private static ProbeOther CreateOther(int objectId, int alias)
        {
            var entity = new ProbeOther { ObjectId = objectId };
            Load(entity, objectId, 0, 0, alias);
            return entity;
        }

        private static ProbeWeapon CreateDerivedType3(int objectId, int state, int hitG)
        {
            var entity = new ProbeWeapon { ObjectId = objectId };
            entity.ConfigureType3();
            Load(entity, objectId, state, hitG, alias: 0);
            return entity;
        }

        private static void Load(
            LF2Entity entity,
            int objectId,
            int state,
            int hitG,
            int alias)
        {
            var data = new LF2CharacterData
            {
                name = "B4Type3Oid999Reference",
                type_sub = alias,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state, hitG),
                    Frame(77, state, 0),
                    Frame(88, state, 0),
                    Frame(101, 0, 0),
                },
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
        }

        private static LF2FrameData Frame(int id, int state, int hitG)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                hit_g = hitG,
                wait = 100,
                next = id,
            };
        }

        private sealed class ProbeSpecial : LF2SpecialAttack
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.SpecialAttack;
            }
        }

        private sealed class ProbeOther : LF2OtherObject
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Other;
            }
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void ConfigureType3()
            {
                SetWeaponType((int)LF2ObjectType.SpecialAttack);
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.SpecialAttack;
            }
        }
    }
}
#endif
