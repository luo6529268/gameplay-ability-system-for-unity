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
        public void ProtectedOid122_CurrentlyMissesFormalHundredPixelMargins(
            double initialX, double formalX)
        {
            var weapon = NewWeapon(122, 1, initialX);

            bool destroyed = weapon.ApplyPreFrameXBounds(800, 0);

            Assert.That(destroyed, Is.False);
            Assert.That(weapon.Runtime.X, Is.EqualTo(initialX));
            Assert.That(weapon.Runtime.X, Is.Not.EqualTo(formalX));
        }

        private static LF2Weapon NewWeapon(int oid, int participantClass, double x)
        {
            var weapon = new LF2Weapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.ObjectId = oid;
            weapon.Unk344 = participantClass;
            weapon.Runtime.SetPosition(x, 0, 250);
            weapon.Runtime.SyncIntegerPosition();
            return weapon;
        }
    }
}
#endif
