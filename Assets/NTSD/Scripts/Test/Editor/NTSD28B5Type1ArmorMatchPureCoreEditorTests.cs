#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type1ArmorMatchPureCoreEditorTests
    {
        [Test]
        public void UnsupportedArmorType_ReturnsUnresolved()
        {
            LF2ArmorData armor = BaseArmor();
            armor.type = 0;

            object result = Resolve(armor, new InteractionArea(), 1, 0, 0,
                0, 4, 0, 0, true);

            AssertDecision(result, "Unresolved", "UnsupportedArmorType");
        }

        [Test]
        public void NonzeroKindAbsent_RejectsBeforeFacingBypass()
        {
            LF2ArmorData armor = BaseArmor();
            armor.facing = 1;
            var interaction = new InteractionArea { kind = 9 };

            object result = Resolve(armor, interaction, 1, 1, 1,
                0, 4, 0, 0, true);

            AssertDecision(result, "CandidateRejected", "KindAbsent");
        }

        [Test]
        public void ListedNonzeroKind_ContinuesThroughMatch()
        {
            LF2ArmorData armor = BaseArmor();
            armor.kinds.Add(9);

            object result = Resolve(
                armor,
                new InteractionArea { kind = 9 },
                1, 0, 0, 0, 4, 0, 0, true);

            AssertDecision(result, "Applies", "None");
        }

        [TestCase(1, 1, 1, "FrontOnlyBackHit")]
        [TestCase(1, 0, 1, "None")]
        [TestCase(2, 0, 1, "BackOnlyFrontHit")]
        [TestCase(2, 1, 1, "None")]
        [TestCase(0, 1, 1, "None")]
        [TestCase(3, 1, 1, "None")]
        public void FacingFilter_UsesNativeEqualityRules(
            int armorFacing,
            int attackerFacing,
            int defenderFacing,
            string expectedReason)
        {
            LF2ArmorData armor = BaseArmor();
            armor.facing = armorFacing;

            object result = Resolve(armor, new InteractionArea(), 1,
                attackerFacing, defenderFacing, 0, 4, 0, 0, true);

            AssertDecision(
                result,
                expectedReason == "None" ? "Applies" : "Bypassed",
                expectedReason);
        }

        [Test]
        public void RatioDelay_UsesStrictLessThanBoundary()
        {
            LF2ArmorData armor = BaseArmor();
            armor.ratio = 14;
            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    0, 4, 15, 0, true),
                "Bypassed",
                "RecoveryDelayExceedsRatio");

            armor.ratio = 15;
            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    0, 4, 15, 0, true),
                "Applies",
                "None");
        }

        [Test]
        public void Bdefend_BypassesOnlyAtExactInteractionHundred()
        {
            LF2ArmorData armor = BaseArmor();
            var interaction = new InteractionArea { bdefend = 100 };
            AssertDecision(
                Resolve(armor, interaction, 1, 0, 0,
                    0, 4, 0, 0, true),
                "Bypassed",
                "BdefendThreshold");

            armor.bdefend = 101;
            AssertDecision(
                Resolve(armor, interaction, 1, 0, 0,
                    0, 4, 0, 0, true),
                "Applies",
                "None");

            armor.bdefend = -1;
            interaction.bdefend = 99;
            AssertDecision(
                Resolve(armor, interaction, 1, 0, 0,
                    0, 4, 0, 0, true),
                "Applies",
                "None");
        }

        [Test]
        public void FallAndInjuryThresholds_UseStrictGreaterBypass()
        {
            LF2ArmorData armor = BaseArmor();
            armor.fall = 5;
            var interaction = new InteractionArea { fall = 6 };
            AssertDecision(
                Resolve(armor, interaction, 1, 0, 0,
                    0, 4, 0, 0, true),
                "Bypassed",
                "FallThreshold");

            interaction.fall = 5;
            armor.injury = 5;
            AssertDecision(
                Resolve(armor, interaction, 1, 0, 0,
                    0, 4, 0, 6, true),
                "Bypassed",
                "InjuryThreshold");

            AssertDecision(
                Resolve(armor, interaction, 1, 0, 0,
                    0, 4, 0, 5, true),
                "Applies",
                "None");
        }

        [Test]
        public void EffectBypass_PrecedesAttackerObjectIdBypass()
        {
            LF2ArmorData armor = BaseArmor();
            armor.effects.Add(20);
            armor.ids.Add(99);
            var interaction = new InteractionArea { effect = 20 };

            AssertDecision(
                Resolve(armor, interaction, 99, 0, 0,
                    0, 4, 0, 0, true),
                "Bypassed",
                "EffectListed");

            interaction.effect = 0;
            AssertDecision(
                Resolve(armor, interaction, 99, 0, 0,
                    0, 4, 0, 0, true),
                "Bypassed",
                "AttackerObjectIdListed");
        }

        [Test]
        public void FrameRangeAndExplicitState_AreInclusiveOrConditions()
        {
            LF2ArmorData armor = BaseArmor();
            armor.frame_ranges.Add(new LF2ArmorFrameRange
            {
                first = 20,
                last = 30,
            });

            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    20, 0, 0, 0, true),
                "Applies",
                "None");
            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    100, 4, 0, 0, true),
                "Applies",
                "None");
            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    31, 0, 0, 0, true),
                "Bypassed",
                "OutsideActiveSet");
        }

        [Test]
        public void ExplicitStates_SuppressSystemFallback()
        {
            LF2ArmorData armor = BaseArmor();

            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    0, 0, 0, 0, true),
                "Bypassed",
                "OutsideActiveSet");
        }

        [TestCase(7, "Applies", "None")]
        [TestCase(8, "Bypassed", "OutsideActiveSet")]
        [TestCase(11, "Bypassed", "OutsideActiveSet")]
        [TestCase(12, "Bypassed", "OutsideActiveSet")]
        [TestCase(13, "Bypassed", "OutsideActiveSet")]
        [TestCase(14, "Bypassed", "OutsideActiveSet")]
        [TestCase(16, "Bypassed", "OutsideActiveSet")]
        [TestCase(18, "Bypassed", "OutsideActiveSet")]
        public void EmptyStates_UsesVersioned2833InvalidStateFallback(
            int defenderState,
            string expectedDecision,
            string expectedReason)
        {
            LF2ArmorData armor = BaseArmor();
            armor.states.Clear();

            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    0, defenderState, 0, 0, true),
                expectedDecision,
                expectedReason);
        }

        [Test]
        public void EmptyStatesWithoutSystemRules_ReturnsUnresolved()
        {
            LF2ArmorData armor = BaseArmor();
            armor.states.Clear();

            AssertDecision(
                Resolve(armor, new InteractionArea(), 1, 0, 0,
                    0, 7, 0, 0, false),
                "Unresolved",
                "SystemInvalidStatesUnavailable");
        }

        [Test]
        public void WarmResolve_AllocatesNoManagedMemory()
        {
            LF2ArmorData armor = BaseArmor();
            armor.frame_ranges.Add(new LF2ArmorFrameRange
            {
                first = 20,
                last = 30,
            });
            armor.kinds.Add(9);
            armor.effects.Add(20);
            armor.ids.Add(99);
            var interaction = new InteractionArea();
            _ = BattleType1ArmorMatchResolver.Resolve(
                armor, interaction, 1, 0, 0, 0, 4, 0, 0, true);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 17;
            for (int index = 0; index < 4096; index++)
            {
                interaction.kind = index % 11 == 0 ? 9 : 0;
                interaction.effect = index % 13 == 0 ? 20 : 0;
                BattleType1ArmorMatchResult result =
                    BattleType1ArmorMatchResolver.Resolve(
                        armor,
                        interaction,
                        index % 17 == 0 ? 99 : 1,
                        index & 1,
                        (index >> 1) & 1,
                        index & 63,
                        index & 31,
                        index & 15,
                        index & 127,
                        true);
                checksum = unchecked(
                    checksum * 31 + (int)result.Decision);
                checksum = unchecked(
                    checksum * 31 + (int)result.Reason);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static LF2ArmorData BaseArmor()
        {
            var armor = new LF2ArmorData
            {
                type = 1,
                ratio = 15,
                decrease = 50,
                fall = -1,
                bdefend = -1,
                injury = -1,
            };
            armor.states.Add(4);
            return armor;
        }

        private static object Resolve(
            LF2ArmorData armor,
            InteractionArea interaction,
            int attackerObjectId,
            int attackerFacing,
            int defenderFacing,
            int defenderAction,
            int defenderState,
            int defenderArmorDelay,
            int effectiveInjury,
            bool systemRulesAvailable)
        {
            Type resolver = Type.GetType(
                "NTSD.Simulation.Ecs.BattleType1ArmorMatchResolver, Assembly-CSharp");
            Assert.That(resolver, Is.Not.Null);
            MethodInfo method = resolver.GetMethod(
                "Resolve",
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(
                null,
                new object[]
                {
                    armor,
                    interaction,
                    attackerObjectId,
                    attackerFacing,
                    defenderFacing,
                    defenderAction,
                    defenderState,
                    defenderArmorDelay,
                    effectiveInjury,
                    systemRulesAvailable,
                });
        }

        private static void AssertDecision(
            object result,
            string expectedDecision,
            string expectedReason)
        {
            Assert.That(
                result.GetType().GetProperty("Decision").GetValue(result)
                    .ToString(),
                Is.EqualTo(expectedDecision));
            Assert.That(
                result.GetType().GetProperty("Reason").GetValue(result)
                    .ToString(),
                Is.EqualTo(expectedReason));
        }
    }
}
#endif
