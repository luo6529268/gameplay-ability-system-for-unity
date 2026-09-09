#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5LegacyKind16RetirementEditorTests
    {
        [Test]
        public void CandidateDisposition_RejectsKind16ButKeepsKind15AndEffect16Damage()
        {
            var victim = new TypedCharacter(LF2ObjectType.Character);

            Assert.That(
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    victim,
                    new InteractionArea { kind = 16 },
                    consumeGateAccepted: true),
                Is.EqualTo(BattleHitCandidateDisposition.Unsupported));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    victim,
                    new InteractionArea { kind = 15 },
                    consumeGateAccepted: true),
                Is.EqualTo(BattleHitCandidateDisposition.Kind15));
            Assert.That(
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    victim,
                    new InteractionArea { kind = 0, effect = 16 },
                    consumeGateAccepted: true),
                Is.EqualTo(BattleHitCandidateDisposition.Damage));
        }

        [Test]
        public void ActualCharacterHit_Kind16IsUnsupportedAndDoesNotMutate()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8460, 0, LF2ObjectType.Character);
            TypedCharacter victim = CreateEntity(world, 8461, 1, LF2ObjectType.Character);
            victim.Health.HP = 70;
            victim.Health.HPBound = 100;
            victim.AttackingCounter = 5;
            victim.Runtime.SetVelocity(2.0, -1.0, 3.0);

            bool resolved = victim.Hit(
                new InteractionArea { kind = 16, injury = 40, vrest = 12 },
                attacker,
                Vector3.zero,
                default);

            AssertKind16NoMutation(resolved, victim, world);
        }

        [Test]
        public void SharedCharacterResolver_Kind16IsUnsupportedAndDoesNotMutate()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8462, 0, LF2ObjectType.Other);
            TypedCharacter victim = CreateEntity(world, 8463, 1, LF2ObjectType.Character);
            victim.Health.HP = 70;
            victim.Health.HPBound = 100;
            victim.AttackingCounter = 5;
            victim.Runtime.SetVelocity(2.0, -1.0, 3.0);

            bool resolved = LF2CharacterDatHitResolver.TryResolveHit(
                victim,
                new InteractionArea { kind = 16, injury = 40, vrest = 12 },
                attacker,
                Vector3.zero,
                default);

            AssertKind16NoMutation(resolved, victim, world);
        }

        [Test]
        public void GenericWeaponDispatch_Kind16IsUnsupportedAndKind15StillSeparates()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8464, 0, LF2ObjectType.Character);
            TypedCharacter victim = CreateEntity(world, 8465, 1, LF2ObjectType.LightWeapon);
            attacker.Runtime.SetPosition(0.0, -5.0, 0.0);
            victim.Runtime.SetPosition(10.0, -5.0, 5.0);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SyncIntegerPosition();
            victim.Runtime.SetVelocity(2.0, -1.0, 3.0);

            bool kind16Resolved = world.DamageWriter.TryApplyCurrentDatTargetHit(
                world,
                attacker,
                victim,
                new InteractionArea { kind = 16, injury = 40 },
                Vector3.zero);

            AssertKind16NoMutation(kind16Resolved, victim, world);

            bool kind15Resolved = world.DamageWriter.TryApplyCurrentDatTargetHit(
                world,
                attacker,
                victim,
                new InteractionArea { kind = 15 },
                Vector3.zero);

            Assert.That(kind15Resolved, Is.True);
            Assert.That(victim.Runtime.Vx, Is.EqualTo(1.0));
            Assert.That(victim.Runtime.Vz, Is.EqualTo(2.5));
        }

        [Test]
        public void ActualWeaponHit_Kind16IsUnsupportedAndDoesNotMutate()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8466, 0, LF2ObjectType.Character);
            ProbeWeapon victim = CreateWeapon(world, 8467, 1, LF2ObjectType.LightWeapon);
            victim.Health.HP = 70;
            victim.Health.HPBound = 100;
            victim.Runtime.SetVelocity(2.0, -1.0, 3.0);

            bool resolved = victim.Hit(
                new InteractionArea { kind = 16, injury = 40 },
                attacker);

            Assert.That(resolved, Is.False);
            Assert.That(victim.Health.HP, Is.EqualTo(70));
            Assert.That(victim.Health.HPBound, Is.EqualTo(100));
            Assert.That(victim.Frame.N, Is.EqualTo(10));
            Assert.That(victim.Runtime.Vx, Is.EqualTo(2.0));
            Assert.That(victim.Runtime.Vy, Is.EqualTo(-1.0));
            Assert.That(victim.Runtime.Vz, Is.EqualTo(3.0));
            Assert.That(world.DamageStats[1], Is.Zero);
            Assert.That(world.KillStats[1], Is.Zero);
        }

        private static void AssertKind16NoMutation(
            bool resolved,
            LF2Entity victim,
            SimulationWorld world)
        {
            Assert.That(resolved, Is.False);
            Assert.That(victim.Health.HP, Is.EqualTo(70));
            Assert.That(victim.Health.HPBound, Is.EqualTo(100));
            Assert.That(victim.Frame.N, Is.EqualTo(10));
            Assert.That(victim.Runtime.Frame, Is.EqualTo(10));
            Assert.That(victim.AttackingCounter, Is.EqualTo(5));
            Assert.That(victim.Runtime.Vx, Is.EqualTo(2.0));
            Assert.That(victim.Runtime.Vy, Is.EqualTo(-1.0));
            Assert.That(victim.Runtime.Vz, Is.EqualTo(3.0));
            Assert.That(victim.ComboCountVic, Is.Zero);
            Assert.That(world.DamageStats[1], Is.Zero);
            Assert.That(world.KillStats[1], Is.Zero);
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int objectId,
            int slot,
            LF2ObjectType objectType)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = 0,
                    wait = 100,
                    next = id,
                });
            }

            var entity = new TypedCharacter(objectType)
            {
                ObjectId = objectId,
            };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5LegacyKind16Retirement",
                    frames = frames,
                }));
            entity.ImmediateFrame(10);
            entity.Health.HP = 70;
            entity.Health.HPBound = 100;
            entity.AttackingCounter = 5;
            entity.Unk344 = 1;
            world.Register(entity);
            entity.RelationTeam = slot + 1;
            return entity;
        }

        private static ProbeWeapon CreateWeapon(
            SimulationWorld world,
            int objectId,
            int slot,
            LF2ObjectType objectType)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 20; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = 0,
                    wait = 100,
                    next = id,
                });
            }

            var weapon = new ProbeWeapon
            {
                ObjectId = objectId,
            };
            weapon.ConfigureType((int)objectType);
            weapon.SetRequiredRuntimeSlot(slot);
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5LegacyKind16ActualWeapon",
                    frames = frames,
                }));
            weapon.ImmediateFrame(10);
            weapon.Health.HP = 70;
            weapon.Health.HPBound = 100;
            weapon.Unk344 = 1;
            world.Register(weapon);
            weapon.RelationTeam = slot + 1;
            return weapon;
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType objectType;

            internal TypedCharacter(LF2ObjectType objectType)
            {
                this.objectType = objectType;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)objectType;
            }
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void ConfigureType(int objectType)
            {
                SetWeaponType(objectType);
            }
        }
    }
}
#endif
