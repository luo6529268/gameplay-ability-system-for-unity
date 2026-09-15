#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6CpointKind2HurtConsumerRetirementEditorTests
    {
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public void StandardDamage_PreservesNativeAction220(bool actual, bool sameFacing)
        {
            Assert.That(RunExistingDamageFixture(actual, sameFacing, 1), Is.EqualTo(220));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void KnockdownControl_PreservesFallingAction(bool actual)
        {
            Assert.That(RunExistingDamageFixture(actual, false, 61), Is.EqualTo(LF2StandardFrames.FallingFront));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Type3Tail_DoesNotReadPreviousCpointAsAction(bool sameFacing)
        {
            var world = new SimulationWorld();
            LF2SpecialAttack attacker = Entity(world, 50, 901);
            LF2SpecialAttack catcher = Entity(world, 51, 902);
            LF2SpecialAttack target = Entity(world, 52, 903);
            target.CatcherSlotIndex = catcher.Runtime.SlotIndex;
            catcher.CaughtSlotIndex = target.Runtime.SlotIndex;
            target.Runtime.PrevFrame2 = 5;
            target.AttackingCounter = 17;
            target.Runtime.Bdefend = 7;
            target.HitStateCount = 241;
            target.Runtime.OwnerSlotIndex = 29;
            target.SwitchDir("right");
            attacker.SwitchDir(sameFacing ? "right" : "left");
            try
            {
                MethodInfo tail = typeof(BattleDamageWriter).GetMethod("ApplySpecialObjectHurtTail", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.That(tail, Is.Not.Null);
                tail.Invoke(null, new object[] { world, attacker, target, new InteractionArea { kind = 0, fall = 1 } });
                Assert.That(target.Frame.N, Is.Zero);
                Assert.That(target.Runtime.Frame, Is.Zero);
                Assert.That(target.AttackingCounter, Is.EqualTo(17));
                Assert.That(target.Health.HP, Is.EqualTo(100));
                Assert.That(target.FallCounter, Is.EqualTo(1));
                Assert.That(target.Runtime.Bdefend, Is.EqualTo(45));
                Assert.That(target.HitStateCount, Is.EqualTo(241));
                Assert.That(target.Runtime.OwnerSlotIndex, Is.EqualTo(29));
                Assert.That(target.CatcherSlotIndex, Is.EqualTo(catcher.Runtime.SlotIndex));
                Assert.That(catcher.CaughtSlotIndex, Is.EqualTo(target.Runtime.SlotIndex));
            }
            finally
            {
                world.Unregister(target);
                world.Unregister(catcher);
                world.Unregister(attacker);
            }
        }

        [TestCase("Simulation/Ecs/Writers/BattleDamageWriter.cs")]
        [TestCase("Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs")]
        [TestCase("Animation/LF2Objects/LF2CharacterDatHitResolver.cs")]
        [TestCase("Animation/LF2Objects/LF2CharacterHitResolver.cs")]
        public void ProductionSource_HasNoRetiredHurtConsumer(string relativePath)
        {
            string source = File.ReadAllText(Path.Combine(Application.dataPath, "NTSD/Scripts", relativePath));
            Assert.That(source, Does.Not.Contain("ApplyCaughtVictimHurtFrame"));
            Assert.That(source, Does.Not.Contain("ResolveCaughtVictimHurtAction"));
            Assert.That(source, Does.Not.Contain("TryResolveCaughtVictimHurtFrame"));
        }

        [Test]
        public void ContentCarrier_ControlPreservesCanonicalAndAliasValues()
        {
            var point = new CatchPoint { kind = 2, injury = 310, cover = 320, fronthurtact = 230, backhurtact = 232 };
            BattleCatchPointValue value = BattleCatchPointValueAdapter.FromLegacy(point);
            Assert.That(value.Injury, Is.EqualTo(310));
            Assert.That(value.Cover, Is.EqualTo(320));
            Assert.That(point.fronthurtact, Is.EqualTo(230));
            Assert.That(point.backhurtact, Is.EqualTo(232));
        }

        private static int RunExistingDamageFixture(bool actual, bool sameFacing, int fall)
        {
            MethodInfo run = typeof(BattleRuntimeSelfCheck).GetMethod("RunCaughtCharacterDamageTailCase", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(run, Is.Not.Null);
            return (int)run.Invoke(null, new object[] { actual, sameFacing, sameFacing ? 1 : 0, fall });
        }

        private static LF2SpecialAttack Entity(SimulationWorld world, int slot, int oid)
        {
            var frames = new List<LF2FrameData>();
            foreach (int number in new[] { 0, 5, 310, 320 })
            {
                frames.Add(new LF2FrameData
                {
                    frameId = number, state = 3000, wait = 1000, next = number,
                    cpoint = number == 5 ? new CatchPoint { kind = 2, injury = 310, cover = 320 } : null,
                });
            }
            var entity = new LF2SpecialAttack { ObjectId = oid, Name = "HurtRetirement" + oid };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(oid, new LF2CharacterData { type_sub = 3, frames = frames }));
            entity.Frame.D = frames[0];
            entity.Frame.N = 0;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            return entity;
        }
    }
}
#endif
