#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28D024NonCharacterXBoundaryWitnessEditorTests
    {
        [Test]
        public void GroundedOrdinaryWeapon_CurrentlyMissesFormalLeftCullThreshold()
        {
            var weapon = NewWeapon(150, 0, 50);

            bool destroyed = weapon.ApplyPreFrameXBounds(800, 0);

            Assert.That(destroyed, Is.False,
                "Characterization: Unity currently keeps X=50; formal playable culls it.");
        }

        [TestCase(50.0, 100.0)]
        [TestCase(790.0, 700.0)]
        public void ProtectedOid122_UsesFormalHundredPixelMargins(
            double initialX, double formalX)
        {
            var weapon = NewWeapon(122, 1, initialX);
            weapon.Runtime.SetSourceRulePosition(initialX, 250);
            weapon.Runtime.SyncSourceRuleIntegerPosition();

            bool destroyed = weapon.ApplyPreFrameXBounds(800, 0);

            Assert.That(destroyed, Is.False);
            Assert.That(weapon.Runtime.X, Is.EqualTo(formalX));
            Assert.That(weapon.Runtime.XInt, Is.EqualTo((int)formalX));
            Assert.That(weapon.Runtime.SourceRuleX, Is.EqualTo(formalX));
            Assert.That(weapon.Runtime.SourceRuleXInt, Is.EqualTo((int)formalX));
        }

        [Test]
        public void ProtectedOid123_ClampsIndependentSourceAndPhysicalPositions()
        {
            var weapon = NewWeapon(123, 2, 790);
            weapon.Runtime.SetSourceRulePosition(50, 250);
            weapon.Runtime.SyncSourceRuleIntegerPosition();

            Assert.That(weapon.ApplyPreFrameXBounds(800, 0), Is.False);
            Assert.That(weapon.Runtime.X, Is.EqualTo(700));
            Assert.That(weapon.Runtime.SourceRuleX, Is.EqualTo(100));
            Assert.That(weapon.Runtime.SourceRuleXInt, Is.EqualTo(100));
        }

        [Test]
        public void ProtectedClamp_PreservesNonParticipantAndNarrowStageBehavior()
        {
            var nonParticipant = NewWeapon(122, 0, 50);
            var narrow = NewWeapon(123, 1, 190);

            Assert.That(nonParticipant.ApplyPreFrameXBounds(800, 0), Is.False);
            Assert.That(nonParticipant.Runtime.X, Is.EqualTo(50));
            Assert.That(narrow.ApplyPreFrameXBounds(150, 0), Is.False);
            Assert.That(narrow.Runtime.X, Is.EqualTo(100));
        }

        private static LF2Weapon NewWeapon(int oid, int participantClass, double x)
        {
            var weapon = new LF2Weapon();
            weapon.SetWeaponType(oid == 122 || oid == 123
                ? (int)LF2ObjectType.Drink
                : (int)LF2ObjectType.LightWeapon);
            weapon.ObjectId = oid;
            weapon.Unk344 = participantClass;
            weapon.Runtime.SetPosition(x, 0, 250);
            weapon.Runtime.SyncIntegerPosition();
            return weapon;
        }
    }
}
#endif
