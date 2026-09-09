#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type1ArmorBreakVerticalOrderEditorTests
    {
        [Test]
        public void ActualSource_OrdersBreakBeforeDeferredVerticalAndPostHit()
        {
            string source = ReadSource(
                "Simulation/Ecs/Writers/BattleDamageWriter.cs");
            string method = Slice(
                source,
                "internal bool ApplyStandardCharacterDamage(",
                "internal bool ApplyWeaponDamage(");

            StringAssert.Contains(
                "ApplyStandardFall(\n                attacker,\n                victim,\n                victimHitCounters,\n                itr,\n                brokenArmor != null)",
                method);
            int breakIndex = method.IndexOf(
                "CompleteNativeBrokenArmorFallback(victim, brokenArmor);");
            int verticalIndex = method.IndexOf(
                "ApplyStandardVerticalKnockback(",
                breakIndex + 1);
            int postHitIndex = method.IndexOf(
                "ApplyNativeType3AttackerPostHitAction(attacker);",
                breakIndex + 1);
            Assert.That(breakIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(verticalIndex, Is.GreaterThan(breakIndex));
            Assert.That(postHitIndex, Is.GreaterThan(verticalIndex));
        }

        [Test]
        public void HitPlanSource_OrdersBreakBeforeVerticalAndPostHit()
        {
            string source = ReadSource(
                "Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs");
            string method = Slice(
                source,
                "private static bool ProjectStandardCharacterDamageWriterEffect(",
                "private static void ProjectNativeEffectActionOverride(");

            int breakIndex = method.IndexOf(
                "projection.TargetRuntimeArmorHp++;");
            int verticalIndex = method.IndexOf(
                "projection.TargetKnockbackVy +=",
                breakIndex + 1);
            int postHitIndex = method.IndexOf(
                "ProjectNativeType3AttackerPostHitAction(attacker, ref projection);",
                breakIndex + 1);
            Assert.That(breakIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(verticalIndex, Is.GreaterThan(breakIndex));
            Assert.That(postHitIndex, Is.GreaterThan(verticalIndex));
        }

        [Test]
        public void BrokenKnockdown_FinalStateRemainsDeterministic()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 21, 0, null);
            LF2ArmorData armor = Armor();
            TypedCharacter target = CreateCharacter(world, 22, 1, armor);
            target.Health.HP = 10;
            target.Runtime.RuntimeArmorHp118 = 20;
            double verticalBefore = target.KnockbackVy;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 20,
                    arest = 10,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.Zero);
            Assert.That(
                target.KnockbackVy,
                Is.EqualTo(verticalBefore - 7.0).Within(0.000000001));
            Assert.That(
                target.Frame.N,
                Is.EqualTo(LF2StandardFrames.FallingFront)
                    .Or.EqualTo(LF2StandardFrames.FallingBack));
        }

        private static string ReadSource(string relativePath)
        {
            return File.ReadAllText(
                Path.Combine(Application.dataPath, "NTSD/Scripts", relativePath));
        }

        private static string Slice(
            string source,
            string startMarker,
            string endMarker)
        {
            int start = source.IndexOf(startMarker);
            int end = source.IndexOf(endMarker, start + startMarker.Length);
            Assert.That(start, Is.GreaterThanOrEqualTo(0));
            Assert.That(end, Is.GreaterThan(start));
            return source.Substring(start, end - start);
        }

        private static LF2ArmorData Armor()
        {
            return new LF2ArmorData
            {
                type = 1,
                decrease = 50,
                fall = -1,
                bdefend = -1,
                injury = -1,
                hp = 100,
                action = 90,
                delay = -1,
            };
        }

        private static TypedCharacter CreateCharacter(
            SimulationWorld world,
            int objectId,
            int slot,
            LF2ArmorData armor)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = LF2States.Standing,
                    wait = 100,
                    next = id,
                });
            }

            var data = new LF2CharacterData
            {
                type_sub = objectId,
                armors = armor == null
                    ? new List<LF2ArmorData>()
                    : new List<LF2ArmorData> { armor },
                frames = frames,
            };
            var entity = new TypedCharacter { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            world.Register(entity);
            entity.RelationTeam = slot + 1;
            return entity;
        }

        private sealed class TypedCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Character;
            }
        }
    }
}
#endif
