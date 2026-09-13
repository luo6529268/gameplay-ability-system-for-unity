#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05MassOscillateCarrierEditorTests
    {
        [TestCase(typeof(CharacterMechanicsContext), "mass")]
        [TestCase(typeof(LF2Character), "_mass")]
        [TestCase(typeof(LF2Character), "MassForFrameAdvance")]
        [TestCase(typeof(BattleCharacterShellSnapshot), "Mass")]
        [TestCase(typeof(LF2EffectState), "Oscillate")]
        [TestCase(typeof(LF2EffectState), "OscillateDirection")]
        [TestCase(typeof(BattleEntityBaseShellSnapshot), "EffectOscillate")]
        [TestCase(typeof(BattleEntityBaseShellSnapshot), "EffectOscillateDirection")]
        [TestCase(typeof(NTSDSpec.SpecEntry), "Mass")]
        [TestCase(typeof(NTSDSpec.SpecEntry), "Oscillate")]
        [TestCase(typeof(NTSDSpec), "GetMassOrDefault")]
        [TestCase(typeof(NTSDSpec), "GetOscillateOrDefault")]
        public void RetiredMemberIsAbsent(Type type, string name)
        {
            Assert.That(type.GetMember(name, BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic), Is.Empty, type.FullName + "." + name);
        }

        [Test]
        public void MechanicsContextHasOnlyCurrentInputsInTheirOriginalOrder()
        {
            var types = new[] { typeof(NTSDEntityRuntime), typeof(LF2FrameData), typeof(float), typeof(float), typeof(double) };
            ConstructorInfo constructor = typeof(CharacterMechanicsContext).GetConstructor(types);
            Assert.That(constructor, Is.Not.Null);
            var runtime = new NTSDEntityRuntime();
            var frame = new LF2FrameData();
            var context = (CharacterMechanicsContext)constructor.Invoke(new object[] { runtime, frame, 2.5f, 0.75f, 1.7d });
            Assert.That(context.Runtime, Is.SameAs(runtime));
            Assert.That(context.frameData, Is.SameAs(frame));
            Assert.That(context.spriteWidthPx, Is.EqualTo(2.5f));
            Assert.That(context.minSpeed, Is.EqualTo(0.75f));
            Assert.That(context.gravity, Is.EqualTo(1.7d));
            Assert.That(typeof(CharacterMechanicsContext).GetConstructors().Length, Is.EqualTo(1));
        }

        [Test]
        public void ValidEffectPayloadStillResetsCompletely()
        {
            var effect = new LF2EffectState
            {
                Num = 9, Dvx = 2, Dvy = -3, Stuck = true, Blink = true,
                Super = true, TimeIn = 8, TimeOut = 7, BlinkCounter = 6
            };
            effect.Reset();
            Assert.That(effect.Num, Is.EqualTo(-99));
            Assert.That((effect.Dvx, effect.Dvy), Is.EqualTo((0f, 0f)));
            Assert.That(effect.Stuck || effect.Blink || effect.Super, Is.False);
            Assert.That((effect.TimeIn, effect.TimeOut, effect.BlinkCounter), Is.EqualTo((0, 0, 0)));
        }
    }
}
#endif
