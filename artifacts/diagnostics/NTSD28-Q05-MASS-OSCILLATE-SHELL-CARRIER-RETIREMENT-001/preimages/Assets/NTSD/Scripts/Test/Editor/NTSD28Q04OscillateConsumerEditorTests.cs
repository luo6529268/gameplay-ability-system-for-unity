#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q04OscillateConsumerEditorTests
    {
        [TestCase(7)]
        [TestCase(-3)]
        [TestCase(0)]
        public void ReservedAmplitudeDoesNotMoveSpriteOrToggleDirection(int amplitude)
        {
            Probe entity = Create(amplitude);
            entity.RunEffects();
            AssertReserved(entity, amplitude);
            Assert.That(entity.Effect.TimeOut, Is.EqualTo(8));
        }

        [TestCase(7)]
        [TestCase(-3)]
        [TestCase(0)]
        public void BlinkRunsIndependentlyOfReservedAmplitude(int amplitude)
        {
            Probe entity = Create(amplitude);
            entity.Effect.Blink = true;
            entity.RunEffects();
            Assert.That(entity.Sprite.EntityVisible, Is.False);
            Assert.That(entity.Effect.BlinkCounter, Is.EqualTo(1));
            entity.RunEffects();
            Assert.That(entity.Sprite.EntityVisible, Is.False);
            entity.RunEffects();
            Assert.That(entity.Sprite.EntityVisible, Is.True);
            Assert.That(entity.Effect.BlinkCounter, Is.EqualTo(3));
            AssertReserved(entity, amplitude);
        }

        [TestCase(7)]
        [TestCase(-3)]
        [TestCase(0)]
        public void TimeoutRetainsOtherCleanupWithoutResettingSpriteOffset(int amplitude)
        {
            Probe entity = Create(amplitude);
            entity.Effect.TimeOut = 0;
            entity.Effect.Blink = true;
            entity.Effect.Stuck = true;
            entity.Effect.Super = true;
            entity.RunEffects();
            Assert.That(entity.Effect.Num, Is.EqualTo(-99));
            Assert.That(entity.Effect.TimeOut, Is.EqualTo(-1));
            Assert.That(entity.Effect.Blink || entity.Effect.Stuck || entity.Effect.Super, Is.False);
            Assert.That(entity.Effect.BlinkCounter, Is.Zero);
            Assert.That(entity.Sprite.EntityVisible, Is.True);
            AssertReserved(entity, amplitude);
        }

        [TestCase(7)]
        [TestCase(-3)]
        [TestCase(0)]
        public void DeferredVelocityStillAppliesOnlyAtMinusOne(int amplitude)
        {
            Probe entity = Create(amplitude);
            entity.Effect.TimeOut = -1;
            entity.Effect.Dvx = 1.5f;
            entity.Effect.Dvy = -2.25f;
            entity.Runtime.Vx = 9;
            entity.Runtime.Vy = 10;
            entity.RunEffects();
            Assert.That((entity.Runtime.Vx, entity.Runtime.Vy), Is.EqualTo((1.5, -2.25)));
            Assert.That((entity.Effect.Dvx, entity.Effect.Dvy), Is.EqualTo((0f, 0f)));
            entity.Runtime.Vx = 11;
            entity.Runtime.Vy = 12;
            entity.RunEffects();
            Assert.That((entity.Runtime.Vx, entity.Runtime.Vy), Is.EqualTo((11.0, 12.0)));
            AssertReserved(entity, amplitude);
        }

        [TestCase(0)]
        [TestCase(2)]
        public void NonnegativeTimeInRemainsAnEarlyReturn(int timeIn)
        {
            Probe entity = Create(7);
            entity.Effect.TimeIn = timeIn;
            entity.Effect.Blink = true;
            entity.RunEffects();
            Assert.That(entity.Effect.TimeOut, Is.EqualTo(9));
            Assert.That(entity.Effect.BlinkCounter, Is.Zero);
            Assert.That(entity.Sprite.EntityVisible, Is.True);
            AssertReserved(entity, 7);
        }

        private static Probe Create(int amplitude)
        {
            var entity = new Probe();
            entity.ModuleInitialize();
            entity.SetSprite(new LF2Sprite());
            entity.Sprite.SetXY(11, 9);
            entity.Effect.TimeIn = -1;
            entity.Effect.TimeOut = 9;
            entity.Effect.Num = 2;
            entity.Effect.Oscillate = amplitude;
            entity.Effect.OscillateDirection = 1;
            return entity;
        }

        private static void AssertReserved(Probe entity, int amplitude)
        {
            Assert.That(entity.Sprite.LocalOffsetPixels, Is.EqualTo(new Vector2(11, 9)));
            Assert.That(entity.Effect.Oscillate, Is.EqualTo(amplitude), "Carrier remains until Q05");
            Assert.That(entity.Effect.OscillateDirection, Is.EqualTo(1));
        }

        private sealed class Probe : LF2Character
        {
            public void SetSprite(LF2Sprite sprite) => Sprite = sprite;
            public void RunEffects() => ProcessEffects();
        }
    }
}
#endif
