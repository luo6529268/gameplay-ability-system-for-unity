#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Kind4AtomicProductionIntegrationEditorTests
    {
        [Test]
        public void CandidateProducer_PositiveEnvironmentIncrementsPerOverlappingBody()
        {
            CreateScenario(2, LF2States.Standing, 50, out SimulationWorld world,
                out LF2Character attacker, out _);
            attacker.Runtime.EnvironmentState320 = 1;

            Collect(world);

            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(2));
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void CandidateProducer_NonPositiveEnvironmentDoesNotIncrement(int environment)
        {
            CreateScenario(2, LF2States.Standing, 50, out SimulationWorld world,
                out LF2Character attacker, out _);
            attacker.Runtime.EnvironmentState320 = environment;

            Collect(world);

            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(2));
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.Zero);
        }

        [Test]
        public void CandidateProducer_UsesUnsignedSixteenBitWrap()
        {
            CreateScenario(1, LF2States.Standing, 50, out SimulationWorld world,
                out LF2Character attacker, out _);
            attacker.Runtime.EnvironmentState320 = 1;
            attacker.Runtime.Kind4SourceCount92 = 0xFFFF;

            Collect(world);

            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.Zero);
        }

        [Test]
        public void CandidateProducer_IncrementsBeforeLowFallSelectionReject()
        {
            CreateScenario(1, LF2States.Falling, 20, out SimulationWorld world,
                out LF2Character attacker, out _);
            attacker.Runtime.EnvironmentState320 = 1;

            Collect(world);

            Assert.That(attacker.Runtime.HitCandidateCount, Is.Zero);
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(1));
        }

        [TestCase(1, 0, 0)]
        [TestCase(0, 99, 4)]
        [TestCase(-1, 99, 4)]
        public void RuntimeItrConversion_UsesEnvironmentAndIgnoresWeaponCount(
            int environment,
            int weaponCount,
            int expectedKind)
        {
            CreateScenario(1, LF2States.Standing, 50, out _,
                out LF2Character attacker, out LF2Character target);
            attacker.Runtime.EnvironmentState320 = environment;
            attacker.WeaponCount = weaponCount;
            InteractionArea source = attacker.GetCollisionFrameData().itrs[0];

            InteractionArea resolved = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                attacker.GetCollisionFrameData(),
                source,
                out _,
                out _);

            Assert.That(resolved.kind, Is.EqualTo(expectedKind));
        }

        [Test]
        public void RuntimeItrConversion_ReversesDvxWhenMotionOpposesFacing()
        {
            CreateScenario(1, LF2States.Standing, 50, out _,
                out LF2Character attacker, out LF2Character target);
            attacker.Runtime.EnvironmentState320 = 1;
            attacker.WeaponCount = 0;
            attacker.Runtime.SetVelocity(-3, 0, 0);
            InteractionArea source = attacker.GetCollisionFrameData().itrs[0];
            source.dvx = 5;

            InteractionArea resolved = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                attacker.GetCollisionFrameData(),
                source,
                out _,
                out _);

            Assert.That(resolved.kind, Is.Zero);
            Assert.That(resolved.dvx, Is.EqualTo(-5));
        }

        [TestCase(1, 0, true)]
        [TestCase(0, 99, false)]
        [TestCase(-1, 99, false)]
        public void HeavyHeldReleaseGate_UsesEnvironmentAndIgnoresWeaponCount(
            int environment,
            int weaponCount,
            bool expectedRelease)
        {
            CreateScenario(1, LF2States.Standing, 50, out SimulationWorld world,
                out LF2Character attacker, out LF2Character target);
            DamageTestCharacter held = DamageEntity(
                world, 8957, 2, LF2ObjectType.HeavyWeapon);
            attacker.Runtime.EnvironmentState320 = environment;
            attacker.WeaponCount = weaponCount;
            target.Runtime.LinkState = 2;
            target.Runtime.TargetSlotIndex = 2;
            held.Runtime.LinkState = -2;
            held.Runtime.HolderStableId = 1;
            InteractionArea source = attacker.GetCollisionFrameData().itrs[0];

            _ = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                attacker.GetCollisionFrameData(),
                source,
                out _,
                out bool releaseHeavyHeldTarget);

            Assert.That(releaseHeavyHeldTarget, Is.EqualTo(expectedRelease));
        }

        [Test]
        public void HitPlanRuntimeProjection_UsesEnvironmentInsteadOfWeaponCount()
        {
            string source = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs"));
            int method = source.IndexOf("private static void ProjectRuntimeItr(");
            Assert.That(method, Is.GreaterThanOrEqualTo(0),
                "ProjectRuntimeItr source method was not found.");
            int nextMethod = source.IndexOf("\n        private ", method + 1);
            Assert.That(nextMethod, Is.GreaterThan(method),
                "ProjectRuntimeItr source method boundary was not found.");
            string body = source.Substring(method, nextMethod - method);

            Assert.That(body, Does.Contain("attacker.Runtime.EnvironmentState320 > 0"));
            Assert.That(body, Does.Not.Contain("attacker.WeaponCount > 0"));
        }

        [Test]
        public void CandidateSelectionSource_HasNoKind4WeaponCountAuthorityBranch()
        {
            string source = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs"));
            int method = source.IndexOf(
                "private bool AcceptReleaseSelectFlagCandidate(");
            Assert.That(method, Is.GreaterThanOrEqualTo(0));
            int nextMethod = source.IndexOf("\n        private ", method + 1);
            Assert.That(nextMethod, Is.GreaterThan(method));
            string body = source.Substring(method, nextMethod - method);

            Assert.That(body, Does.Not.Contain("WeaponCount"));
        }

        [Test]
        public void HitPlanShadowCompare_Kind4CountAndRedirectedScoreMatchActualWriter()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8952, 0, LF2ObjectType.Character);
            DamageTestCharacter victim = DamageEntity(
                world, 8953, 1, LF2ObjectType.Character);
            DamageTestCharacter source = DamageEntity(
                world, 8954, 2, LF2ObjectType.SpecialAttack);
            DamageTestCharacter owner1 = DamageEntity(
                world, 8955, 3, LF2ObjectType.SpecialAttack);
            DamageTestCharacter owner2 = DamageEntity(
                world, 8956, 4, LF2ObjectType.Character);
            source.Runtime.OwnerSlotIndex = 3;
            owner1.Runtime.OwnerSlotIndex = 4;
            attacker.Runtime.EnvironmentState320 = 1;
            attacker.Runtime.CatchSourceSlot90 = 2;
            attacker.Frame.D.itrs.Add(new InteractionArea
            {
                kind = 4,
                injury = 10,
                fall = 1,
                dvx = 1,
                arest = 10,
                vrest = 1,
                x = -100,
                y = -20,
                w = 240,
                h = 40,
                zwidth = 15,
            });
            victim.Frame.D.bodies.Add(new BodyBox
            {
                kind = 0,
                x = 0,
                y = -10,
                w = 10,
                h = 20,
            });
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(1));
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.ShadowCompare);

            world.PostInteractionTickAll(3204);

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            Assert.That(diagnostics.CurrentTickPlanValid, Is.True);
            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
            Assert.That(owner2.Runtime.InputScoreTotal348, Is.EqualTo(10));
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.Zero);
        }

        [TestCase(true, 1, 0, true)]
        [TestCase(true, 0, 99, false)]
        [TestCase(false, 1, 0, true)]
        [TestCase(false, 0, 99, false)]
        public void DirectCharacterHitFallbacks_UseEnvironmentAndIgnoreWeaponCount(
            bool concreteCharacterRoute,
            int environment,
            int weaponCount,
            bool expectedApplied)
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 9000, 0, LF2ObjectType.Other);
            DamageTestCharacter victim = DamageEntity(
                world, 9001, 1, LF2ObjectType.Character);
            attacker.Runtime.EnvironmentState320 = environment;
            attacker.WeaponCount = weaponCount;
            InteractionArea interaction = DamageInteraction();
            interaction.kind = 4;

            bool applied = concreteCharacterRoute
                ? victim.Hit(interaction, attacker, Vector3.zero, default)
                : LF2CharacterDatHitResolver.TryResolveHit(
                    victim,
                    interaction,
                    attacker,
                    Vector3.zero,
                    default);

            Assert.That(applied, Is.EqualTo(expectedApplied));
            Assert.That(victim.Health.HP,
                Is.EqualTo(expectedApplied ? 490 : 500));
        }

        [Test]
        public void StandardDamage_PendingKind4RedirectsScoreThroughTwoOwnersAndConsumesOnce()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8960, 0, LF2ObjectType.Character);
            DamageTestCharacter source = DamageEntity(
                world, 8961, 1, LF2ObjectType.SpecialAttack);
            DamageTestCharacter owner1 = DamageEntity(
                world, 8962, 2, LF2ObjectType.SpecialAttack);
            DamageTestCharacter owner2 = DamageEntity(
                world, 8963, 3, LF2ObjectType.Character);
            DamageTestCharacter victim = DamageEntity(
                world, 8964, 4, LF2ObjectType.Character);
            source.Runtime.OwnerSlotIndex = 2;
            owner1.Runtime.OwnerSlotIndex = 3;
            attacker.Runtime.CatchSourceSlot90 = 0x10001;
            attacker.Runtime.Kind4SourceCount92 = 2;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(source.Runtime.InputScoreTotal348, Is.Zero);
            Assert.That(owner1.Runtime.InputScoreTotal348, Is.Zero);
            Assert.That(owner2.Runtime.InputScoreTotal348, Is.EqualTo(10));
            Assert.That(attacker.Runtime.InputScoreTotal348, Is.Zero);
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(1));
        }

        [Test]
        public void StandardDamage_ZeroLowWordStartsAtPhysicalAttackerOwnerChain()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8965, 0, LF2ObjectType.Character);
            DamageTestCharacter owner = DamageEntity(
                world, 8966, 1, LF2ObjectType.Character);
            DamageTestCharacter victim = DamageEntity(
                world, 8967, 2, LF2ObjectType.Character);
            attacker.Runtime.OwnerSlotIndex = 1;
            attacker.Runtime.CatchSourceSlot90 = 99;
            attacker.Runtime.Kind4SourceCount92 = 0x10000;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(owner.Runtime.InputScoreTotal348, Is.EqualTo(10));
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(0x10000));
        }

        [Test]
        public void StandardDamage_MissingRedirectedSourceFailsClosedButConsumesCount()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8968, 0, LF2ObjectType.Character);
            DamageTestCharacter victim = DamageEntity(
                world, 8969, 1, LF2ObjectType.Character);
            attacker.Runtime.CatchSourceSlot90 = 99;
            attacker.Runtime.Kind4SourceCount92 = 1;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(victim.Health.HP, Is.EqualTo(490));
            Assert.That(attacker.Runtime.InputScoreTotal348, Is.Zero);
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.Zero);
        }

        [Test]
        public void StandardDamage_NonDefaultCreditGateSuppressesScoreButConsumesCount()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8970, 0, LF2ObjectType.Character);
            DamageTestCharacter source = DamageEntity(
                world, 8971, 1, LF2ObjectType.Character);
            DamageTestCharacter victim = DamageEntity(
                world, 8972, 2, LF2ObjectType.Character);
            attacker.Runtime.CatchSourceSlot90 = 1;
            attacker.Runtime.Kind4SourceCount92 = 1;
            victim.Runtime.OrdinaryCreditGate2F4 = 0;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(source.Runtime.InputScoreTotal348, Is.Zero);
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.Zero);
        }

        [Test]
        public void ReducedDamage_UsesRedirectedCreditAndConsumesOnce()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8973, 0, LF2ObjectType.Character);
            DamageTestCharacter source = DamageEntity(
                world, 8974, 1, LF2ObjectType.Character);
            DamageTestCharacter victim = DamageEntity(
                world, 8975, 2, LF2ObjectType.Character);
            attacker.Runtime.CatchSourceSlot90 = 1;
            attacker.Runtime.Kind4SourceCount92 = 2;

            world.DamageWriter.ApplyAlternateDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                DamageInteraction());

            Assert.That(source.Runtime.InputScoreTotal348, Is.EqualTo(1));
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(1));
        }

        [TestCase(LF2ObjectType.LightWeapon)]
        [TestCase(LF2ObjectType.HeavyWeapon)]
        [TestCase(LF2ObjectType.SpecialAttack)]
        [TestCase(LF2ObjectType.ThrowWeapon)]
        [TestCase(LF2ObjectType.Other)]
        public void SuccessfulOrdinaryDamage_TypesOneThroughFiveConsumeOnce(
            LF2ObjectType targetType)
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8980 + (int)targetType, 0, LF2ObjectType.Character);
            DamageTestCharacter victim = DamageEntity(
                world, 8990 + (int)targetType, 1, targetType);
            attacker.Runtime.Kind4SourceCount92 = 2;

            bool applied = targetType == LF2ObjectType.LightWeapon ||
                targetType == LF2ObjectType.HeavyWeapon ||
                targetType == LF2ObjectType.ThrowWeapon
                ? world.DamageWriter.ApplyWeaponDamage(
                    world, attacker, victim, DamageInteraction())
                : world.DamageWriter.ApplySpecialAttackDamage(
                    world, attacker, victim, DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(1));
        }

        [Test]
        public void DrinkAndUnsupportedDamage_DoNotConsumePendingKind4Count()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8996, 0, LF2ObjectType.Character);
            DamageTestCharacter drink = DamageEntity(
                world, 8997, 1, LF2ObjectType.Drink);
            attacker.Runtime.Kind4SourceCount92 = 2;

            bool drinkApplied = world.DamageWriter.ApplyWeaponDamage(
                world, attacker, drink, DamageInteraction());
            InteractionArea unsupported = DamageInteraction();
            unsupported.kind = 6;
            bool unsupportedApplied = world.DamageWriter.ApplySpecialAttackDamage(
                world, attacker, drink, unsupported);

            Assert.That(drinkApplied, Is.True);
            Assert.That(unsupportedApplied, Is.False);
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(2));
        }

        [Test]
        public void MatchedProjectilePairEarlyBranch_DoesNotConsumePendingKind4Count()
        {
            var world = new SimulationWorld();
            DamageTestCharacter attacker = DamageEntity(
                world, 8998, 0, LF2ObjectType.SpecialAttack);
            DamageTestCharacter victim = DamageEntity(
                world, 8999, 1, LF2ObjectType.SpecialAttack);
            attacker.Frame.D.state = LF2States.ObjectFlying;
            victim.Frame.D.state = LF2States.ObjectFlying;
            attacker.Runtime.Kind4SourceCount92 = 2;

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world, attacker, victim, DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.Kind4SourceCount92, Is.EqualTo(2));
        }

        private static void Collect(SimulationWorld world)
        {
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            world.EndCollisionCandidateConsumption();
        }

        private static void CreateScenario(
            int bodyCount,
            int targetState,
            int fall,
            out SimulationWorld world,
            out LF2Character attacker,
            out LF2Character target)
        {
            var itr = new InteractionArea
            {
                kind = 4,
                effect = 0,
                vrest = 1,
                fall = fall,
                injury = 10,
                dvx = 1,
                x = -100,
                y = -20,
                w = 240,
                h = 40,
                zwidth = 15,
            };
            LF2FrameData attackerFrame = Frame(LF2States.Standing);
            attackerFrame.itrs.Add(itr);
            LF2FrameData targetFrame = Frame(targetState);
            for (int index = 0; index < bodyCount; index++)
            {
                targetFrame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = index,
                    y = -10,
                    w = 10,
                    h = 20,
                });
            }

            world = new SimulationWorld();
            attacker = Character("Kind4Attacker", 8950, attackerFrame);
            target = Character("Kind4Target", 8951, targetFrame);
            Register(world, attacker, 0, 1);
            Register(world, target, 1, 2);
        }

        private static LF2FrameData Frame(int state)
        {
            return new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
            };
        }

        private static InteractionArea DamageInteraction()
        {
            return new InteractionArea
            {
                kind = 0,
                injury = 10,
                fall = 1,
                dvx = 1,
                arest = 10,
                vrest = 1,
            };
        }

        private static DamageTestCharacter DamageEntity(
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
                    state = LF2States.Standing,
                    wait = 100,
                    next = id,
                });
            }

            var entity = new DamageTestCharacter(objectType)
            {
                Name = $"Kind4Damage{objectId}",
                ObjectId = objectId,
            };
            entity.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = entity.Name,
                    type_sub = objectId,
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.RelationTeam = slot + 1;
            entity.Team = slot + 1;
            entity.Unk344 = 1;
            world.Register(entity);
            return entity;
        }

        private static LF2Character Character(
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 0,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new TestCharacter { Name = name, ObjectId = objectId };
            character.ModuleInitialize();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            return character;
        }

        private static void Register(
            SimulationWorld world,
            LF2Character entity,
            int slot,
            int team)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class TestCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }

        private sealed class DamageTestCharacter : LF2Character
        {
            private readonly LF2ObjectType objectType;

            internal DamageTestCharacter(LF2ObjectType objectType)
            {
                this.objectType = objectType;
            }

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)objectType;
        }
    }
}
#endif
