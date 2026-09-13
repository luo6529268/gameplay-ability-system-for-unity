#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;

using NTSD.Animation.LF2Objects;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    [Category("NTSD28_B9")]
    public sealed class NTSD28B9NtsdSpecOscillateProducerRetirementEditorTests
    {
        [TestCase(0, 0)]
        [TestCase(2, 0)]
        [TestCase(0, 7)]
        [TestCase(2, -3)]
        public void EffectCreate_PreservesExistingOscillateAndEffectPayload(
            int effectNumber,
            int initialOscillate)
        {
            var entity = new LF2Character();
            entity.Effect.Oscillate = initialOscillate;

            entity.EffectCreate(
                effectNumber,
                duration: 12,
                dvx: 1.5f,
                dvy: -2.25f);

            Assert.That(entity.Effect.Num, Is.EqualTo(effectNumber));
            Assert.That(entity.Effect.Stuck, Is.True);
            Assert.That(entity.Effect.Dvx, Is.EqualTo(1.5f));
            Assert.That(entity.Effect.Dvy, Is.EqualTo(-2.25f));
            Assert.That(entity.Effect.TimeIn, Is.Zero);
            Assert.That(entity.Effect.TimeOut, Is.EqualTo(12));
            Assert.That(
                entity.Effect.Oscillate,
                Is.EqualTo(initialOscillate),
                "EffectCreate must not derive Oscillate from the retired old effect ID table.");
        }

        [TestCase(7)]
        [TestCase(-3)]
        public void EffectCreate_HigherPriorityRetainsOscillateAndExistingMotion(
            int initialOscillate)
        {
            var entity = new LF2Character();
            entity.Effect.Num = 1;
            entity.Effect.Dvx = 3.5f;
            entity.Effect.Dvy = -4.5f;
            entity.Effect.Stuck = true;
            entity.Effect.Oscillate = initialOscillate;
            entity.Effect.TimeIn = 4;
            entity.Effect.TimeOut = 5;

            entity.EffectCreate(2, duration: 9);

            Assert.That(entity.Effect.Num, Is.EqualTo(2));
            Assert.That(entity.Effect.Stuck, Is.True);
            Assert.That(entity.Effect.Dvx, Is.EqualTo(3.5f));
            Assert.That(entity.Effect.Dvy, Is.EqualTo(-4.5f));
            Assert.That(entity.Effect.TimeIn, Is.Zero);
            Assert.That(entity.Effect.TimeOut, Is.EqualTo(9));
            Assert.That(entity.Effect.Oscillate, Is.EqualTo(initialOscillate));
        }

        [TestCase(0, 0)]
        [TestCase(2, 7)]
        public void EffectCreate_ExtendsExistingDurationWithoutDerivingOscillate(
            int effectNumber,
            int initialOscillate)
        {
            var entity = new LF2Character();
            entity.Effect.Num = effectNumber;
            entity.Effect.Oscillate = initialOscillate;
            entity.Effect.TimeIn = -1;
            entity.Effect.TimeOut = 3;

            entity.EffectCreate(effectNumber, duration: 5);

            Assert.That(entity.Effect.Num, Is.EqualTo(effectNumber));
            Assert.That(entity.Effect.TimeIn, Is.EqualTo(-1));
            Assert.That(entity.Effect.TimeOut, Is.EqualTo(5));
            Assert.That(entity.Effect.Oscillate, Is.EqualTo(initialOscillate));
        }

        [Test]
        public void EffectCreate_LowerPriorityLeavesEveryEffectFieldUntouched()
        {
            var entity = new LF2Character();
            entity.Effect.Num = 3;
            entity.Effect.Dvx = 3.5f;
            entity.Effect.Dvy = -4.5f;
            entity.Effect.Stuck = true;
            entity.Effect.Oscillate = 7;
            entity.Effect.Blink = true;
            entity.Effect.Super = true;
            entity.Effect.TimeIn = 4;
            entity.Effect.TimeOut = 5;
            entity.Effect.OscillateDirection = -1;
            entity.Effect.BlinkCounter = 6;

            entity.EffectCreate(2, duration: 9, dvx: 8f, dvy: 9f);

            Assert.That(entity.Effect.Num, Is.EqualTo(3));
            Assert.That(entity.Effect.Dvx, Is.EqualTo(3.5f));
            Assert.That(entity.Effect.Dvy, Is.EqualTo(-4.5f));
            Assert.That(entity.Effect.Stuck, Is.True);
            Assert.That(entity.Effect.Oscillate, Is.EqualTo(7));
            Assert.That(entity.Effect.Blink, Is.True);
            Assert.That(entity.Effect.Super, Is.True);
            Assert.That(entity.Effect.TimeIn, Is.EqualTo(4));
            Assert.That(entity.Effect.TimeOut, Is.EqualTo(5));
            Assert.That(entity.Effect.OscillateDirection, Is.EqualTo(-1));
            Assert.That(entity.Effect.BlinkCounter, Is.EqualTo(6));
        }

        [Test]
        public void EffectCreate_ProducerBodyHasNoRetiredNtsdSpecLookup()
        {
            string source = File.ReadAllText(SourcePath());
            const string startMarker = "public virtual void EffectCreate(";
            const string endMarker = "public virtual void VisualEffectCreate(";
            int start = source.IndexOf(startMarker, StringComparison.Ordinal);
            int end = source.IndexOf(
                endMarker,
                start < 0 ? 0 : start + startMarker.Length,
                StringComparison.Ordinal);

            Assert.That(start, Is.GreaterThanOrEqualTo(0), startMarker);
            Assert.That(end, Is.GreaterThan(start), endMarker);
            string producer = source.Substring(start, end - start);

            Assert.That(producer, Does.Not.Contain("NTSDSpec"));
            Assert.That(producer, Does.Not.Contain("GetOscillateOrDefault"));
            Assert.That(producer, Does.Not.Contain("EffectNumToId"));
        }

        private static string SourcePath()
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.Combine(
                root ?? string.Empty,
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs");
        }
    }
}
#endif
