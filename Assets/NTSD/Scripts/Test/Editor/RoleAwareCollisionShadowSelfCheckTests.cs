#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class RoleAwareCollisionShadowSelfCheckTests
    {
        [Test]
        public void Shadow_DefaultsOff_AndOnlyEmitsAttackToBodyPairs()
        {
            var world = new SimulationWorld();
            LF2FrameData attackerFrame = MakeFrame(
                new InteractionArea { kind = 0, x = 100, y = -10, w = 20, h = 20, zwidth = 15 },
                new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 });
            LF2Character attacker = CreateCharacter("RoleShadow_Attacker", 1, attackerFrame);
            LF2Character target = CreateCharacter(
                "RoleShadow_Target",
                2,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            RegisterPair(world, attacker, target);

            var query = (BruteForceSceneQuery)world.SceneQuery;
            Assert.That(query.ShadowBroadphaseDiagnosticsEnabled, Is.False);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.RoleAwareShadowDiagnostics.RebuildCount, Is.Zero);
            world.EndCollisionCandidateConsumption();

            query.ShadowBroadphaseDiagnosticsEnabled = true;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            RoleAwareCollisionShadowDiagnostics diagnostics = query.RoleAwareShadowDiagnostics;
            Assert.That(diagnostics.ParticipantCount, Is.EqualTo(2));
            Assert.That(diagnostics.BodyCount, Is.EqualTo(2));
            Assert.That(diagnostics.IndexedBodyCount, Is.EqualTo(2));
            Assert.That(diagnostics.AttackItrCount, Is.EqualTo(1));
            Assert.That(diagnostics.BrutePairCount, Is.Zero);
            Assert.That(diagnostics.QuadtreePairCount, Is.Zero);
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.CollectionAborted, Is.False);
            world.EndCollisionCandidateConsumption();

            attackerFrame.itrs[0].x = -10;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(diagnostics.BrutePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.QuadtreePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.FirstDifferencePair, Is.EqualTo(-1));
            Assert.That(diagnostics.FirstDifference,
                Is.EqualTo(RoleAwareCollisionShadowDifference.None));
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void Shadow_NonIndexableRoleBoundsFallbackConservatively()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleShadow_DegenerateItr",
                1,
                MakeFrame(
                    new InteractionArea { kind = 0, x = 0, y = 0, w = 0, h = 20, zwidth = 15 },
                    null));
            LF2Character target = CreateCharacter(
                "RoleShadow_FallbackTarget",
                2,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            RegisterPair(world, attacker, target);

            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.ShadowBroadphaseDiagnosticsEnabled = true;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            RoleAwareCollisionShadowDiagnostics diagnostics = query.RoleAwareShadowDiagnostics;
            Assert.That(diagnostics.FallbackAttackItrCount, Is.EqualTo(1));
            Assert.That(diagnostics.BrutePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.QuadtreePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void Shadow_ExceptionCannotChangeFormalCollection_AndNextRunResetsAbort()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleShadow_AbortAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                        injury = 10,
                        dvx = 1,
                        arest = 4,
                        vrest = 1,
                    },
                    null));
            LF2Character target = CreateCharacter(
                "RoleShadow_AbortTarget",
                2,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            RegisterPair(world, attacker, target);

            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.ShadowBroadphaseDiagnosticsEnabled = true;
            query.ThrowDuringRoleAwareShadowForSelfCheck = true;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.TryGetCollisionCandidateSequence(
                attacker,
                out List<SceneQueryHit> candidates), Is.True);
            Assert.That(query.RoleAwareShadowDiagnostics.CollectionAborted, Is.True);
            Assert.That(query.RoleAwareShadowDiagnostics.MismatchCount, Is.Zero);
            Assert.That(query.RoleAwareShadowDiagnostics.FirstDifferencePair, Is.EqualTo(-1));
            Assert.That(query.FormalCollectionAborted, Is.False);
            Assert.That(candidates.Count, Is.EqualTo(1));
            world.EndCollisionCandidateConsumption();

            query.ThrowDuringRoleAwareShadowForSelfCheck = false;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.RoleAwareShadowDiagnostics.CollectionAborted, Is.False);
            Assert.That(query.RoleAwareShadowDiagnostics.ParticipantCount, Is.EqualTo(2));
            Assert.That(query.RoleAwareShadowDiagnostics.MismatchCount, Is.Zero);
            Assert.That(query.RoleAwareShadowDiagnostics.FirstDifferencePair, Is.EqualTo(-1));
            Assert.That(query.FormalCollectionAborted, Is.False);
            world.EndCollisionCandidateConsumption();
        }

        private static LF2FrameData MakeFrame(InteractionArea itr, BodyBox body)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (itr != null)
                frame.itrs.Add(itr);
            if (body != null)
                frame.bodies.Add(body);
            return frame;
        }

        private static LF2Character CreateCharacter(
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new ShadowSelfCheckController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            return character;
        }

        private static void RegisterPair(
            SimulationWorld world,
            LF2Character attacker,
            LF2Character target)
        {
            world.Register(attacker);
            Configure(attacker, 1);
            world.Register(target);
            Configure(target, 2);
        }

        private static void Configure(LF2Entity entity, int team)
        {
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.FrameDelay = 0;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class ShadowSelfCheckController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }

    public sealed class RoleAwareCollisionFormalCollectorSelfCheckTests
    {
        private const uint CollectionSeed = 0x41C64E6Du;

        [Test]
        public void Formal_DefaultConfiguredBruteForceBackendRemainsBrute()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_DefaultAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character target = CreateCharacter(
                "RoleFormal_DefaultTarget",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            Register(world, attacker, 0, 1, 0);
            Register(world, target, 1, 2, 0);

            BruteForceSceneQuery query = GetQuery(world);
            Assert.That(query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            RunCollection(world, query, CollisionFormalCollectorMode.Configured, attacker);
            Assert.That(query.LastFormalCollectorModeForDiagnostics,
                Is.EqualTo(CollisionFormalCollectorMode.ForceBruteForce));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_ConfiguredLooseReusesStationaryRoleAwareIndexAndMatchesBrute()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.Authority400,
                BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity,
                CollisionBroadphaseBackend.LooseQuadtree);
            LF2Character attacker = CreateCharacter(
                "RoleFormal_StationaryAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -20,
                        y = -20,
                        w = 40,
                        h = 40,
                        zwidth = 15,
                    },
                    null));
            LF2Character target = CreateCharacter(
                "RoleFormal_StationaryTarget",
                2,
                MakeFrame(
                    null,
                    new BodyBox
                    {
                        kind = 0,
                        x = -20,
                        y = -20,
                        w = 40,
                        h = 40,
                    }));
            Register(world, attacker, 0, 1, 0);
            Register(world, target, 1, 2, 0);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun initial = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.Configured,
                CollectionSeed,
                attacker);
            var initialSync = query.LastFormalSynchronizeResultForDiagnostics;
            Assert.That(
                query.LastFormalCollectorModeForDiagnostics,
                Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
            Assert.That(initialSync.Succeeded, Is.True);
            Assert.That(initialSync.FullRebuild, Is.True);
            Assert.That(initialSync.IndexedCount, Is.EqualTo(1));

            CandidateRun stationary = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.Configured,
                CollectionSeed,
                attacker);
            var stationarySync = query.LastFormalSynchronizeResultForDiagnostics;
            Assert.That(
                query.LastFormalCollectorModeForDiagnostics,
                Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
            Assert.That(stationarySync.Succeeded, Is.True);
            Assert.That(stationarySync.FullRebuild, Is.False);
            Assert.That(stationarySync.InsertedCount, Is.Zero);
            Assert.That(stationarySync.UpdatedInPlaceCount, Is.Zero);
            Assert.That(stationarySync.MigratedCount, Is.Zero);
            Assert.That(stationarySync.RemovedCount, Is.Zero);
            Assert.That(stationarySync.IndexedCount, Is.EqualTo(1));
            AssertRunsEqual(initial, stationary);

            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                CollectionSeed,
                attacker);
            AssertRunsEqual(brute, stationary);
            Assert.That(stationary.CollectionAborted, Is.False);
            Assert.That(stationary.Counts, Is.EqualTo(new[] { 1 }));
            Assert.That(stationary.RngCalls, Is.EqualTo(brute.RngCalls));
            Assert.That(stationary.RngState, Is.EqualTo(brute.RngState));
        }

        [Test]
        public void Formal_RoleAwareMatchesLegacyExactSequenceCountAndRng_WithAuthorityOrder()
        {
            var world = new SimulationWorld();
            LF2Character registeredFirstHighSlot = CreateCharacter(
                "RoleFormal_HighSlot",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -25,
                        y = -10,
                        w = 50,
                        h = 20,
                        zwidth = 15,
                    },
                    new BodyBox { kind = 0, x = 5, y = -10, w = 20, h = 20 }));
            LF2Character registeredSecondLowSlot = CreateCharacter(
                "RoleFormal_LowSlot",
                2,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -25,
                        y = -10,
                        w = 50,
                        h = 20,
                        zwidth = 15,
                    },
                    new BodyBox { kind = 0, x = -5, y = -10, w = 20, h = 20 }));

            Register(world, registeredFirstHighSlot, 9, 1, -192);
            Register(world, registeredSecondLowSlot, 2, 2, -91);
            registeredFirstHighSlot.Runtime.SetPosition(0, 0, 0);
            registeredFirstHighSlot.Runtime.SyncIntegerPosition();
            registeredSecondLowSlot.Runtime.SetPosition(0, 0, 0);
            registeredSecondLowSlot.Runtime.SyncIntegerPosition();

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun legacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceLegacyUnionAabb,
                registeredSecondLowSlot,
                registeredFirstHighSlot);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                registeredSecondLowSlot,
                registeredFirstHighSlot);

            AssertRunsEqual(legacy, role);
            Assert.That(role.Sequences[0].Count, Is.EqualTo(1));
            Assert.That(role.Sequences[0][0].TargetSlot, Is.EqualTo(9));
            Assert.That(role.Sequences[0][0].BodyX, Is.EqualTo(5));
            Assert.That(role.Sequences[1].Count, Is.EqualTo(1));
            Assert.That(role.Sequences[1][0].TargetSlot, Is.EqualTo(2));
            Assert.That(role.Sequences[1][0].BodyX, Is.EqualTo(-5));
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareBodyEntryCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastRoleAwareItrQueryCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_RoleAwareMatchesBruteForTwentyCapAndRoleOnlyParticipants()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_CapAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -20,
                        y = -20,
                        w = 40,
                        h = 40,
                        zwidth = 15,
                    },
                    null));
            Register(world, attacker, 30, 1, 0);

            var targets = new List<LF2Character>();
            for (int slot = 0; slot < 21; slot++)
            {
                LF2Character target = CreateCharacter(
                    $"RoleFormal_CapTarget_{slot}",
                    100 + slot,
                    MakeFrame(
                        null,
                        new BodyBox
                        {
                            kind = 0,
                            x = slot - 10,
                            y = -10,
                            w = 5,
                            h = 20,
                        }));
                Register(world, target, slot, 2, 0);
                targets.Add(target);
            }

            LF2Character itrOnlyFarAway = CreateCharacter(
                "RoleFormal_ItrOnlyFarAway",
                200,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            Register(world, itrOnlyFarAway, 31, 3, 10000);

            // It has neither an attack nor a body. Legacy union-AABB marks that
            // participant unindexable; role-aware diagnostics must retain the
            // same fact without manufacturing an impossible collision pair.
            LF2Character inertFarAway = CreateCharacter(
                "RoleFormal_InertFarAway",
                201,
                MakeFrame(null, null));
            Register(world, inertFarAway, 32, 4, 20000);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);

            AssertRunsEqual(brute, role);
            Assert.That(role.Sequences[0].Count, Is.EqualTo(20));
            for (int candidateIndex = 0; candidateIndex < 20; candidateIndex++)
            {
                Assert.That(
                    role.Sequences[0][candidateIndex].TargetSlot,
                    Is.EqualTo(candidateIndex));
            }
            Assert.That(
                role.Sequences[0].Exists(hit => hit.TargetSlot == targets[20].Runtime.SlotIndex),
                Is.False);
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(21));
            Assert.That(query.LastFormalFallbackParticipantCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_NonIndexableRoleBoundsFallbackMatchesLegacy()
        {
            var world = new SimulationWorld();
            LF2Character degenerateItr = CreateCharacter(
                "RoleFormal_DegenerateItr",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = 0,
                        y = 0,
                        w = 0,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character validBody = CreateCharacter(
                "RoleFormal_ValidBody",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character degenerateBody = CreateCharacter(
                "RoleFormal_DegenerateBody",
                3,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = 0, y = -10, w = 0, h = 20 }));
            LF2Character validItr = CreateCharacter(
                "RoleFormal_ValidItr",
                4,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            Register(world, degenerateItr, 0, 1, 0);
            Register(world, validBody, 1, 2, 0);
            Register(world, degenerateBody, 2, 2, 0);
            Register(world, validItr, 3, 3, 0);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun legacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceLegacyUnionAabb,
                degenerateItr,
                validItr);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                degenerateItr,
                validItr);

            AssertRunsEqual(legacy, role);
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(4));
            Assert.That(query.LastFormalFallbackParticipantCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Formal_EqualDistanceKind1TieMatchesLegacyRngAndReplacement(int seed)
        {
            LF2FrameData attackerFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 1,
                    vrest = 1,
                    x = 0,
                    y = -10,
                    w = 30,
                    h = 20,
                    zwidth = 15,
                },
                null);
            attackerFrame.itrs.Add(new InteractionArea
            {
                kind = 1,
                vrest = 1,
                x = 0,
                y = -10,
                w = 30,
                h = 20,
                zwidth = 15,
            });
            LF2FrameData targetFrame = MakeFrame(
                null,
                new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 });
            targetFrame.state = LF2States.Injured2;

            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_Kind1Attacker",
                1,
                attackerFrame);
            LF2Character target = CreateCharacter(
                "RoleFormal_Kind1Target",
                2,
                targetFrame);
            Register(world, target, 0, 2, 10);
            Register(world, attacker, 5, 1, 0);
            attacker.Runtime.KeyRight = 1;

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun legacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceLegacyUnionAabb,
                unchecked((uint)seed),
                attacker);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                unchecked((uint)seed),
                attacker);

            var expectedRng = new DeterministicRng(unchecked((uint)seed));
            bool tieReplaced = expectedRng.NextInt(0, 2) == 0;
            AssertRunsEqual(legacy, role);
            Assert.That(role.RngCalls, Is.EqualTo(1));
            Assert.That(role.RngState, Is.EqualTo(expectedRng.State));
            Assert.That(
                role.Sequences[0].Count,
                Is.EqualTo(tieReplaced ? 2 : 1));
            Assert.That(role.Sequences[0][0].ItrIndex, Is.EqualTo(0));
            if (tieReplaced)
                Assert.That(role.Sequences[0][1].ItrIndex, Is.EqualTo(1));
            Assert.That(role.CollectionAborted, Is.False);
        }

        [Test]
        public void Formal_MixedIndexableAndDegenerateRolesMatchBruteWhereLegacyMisses()
        {
            // Authority CollisionCollect keeps zero-width authored rectangles and
            // uses strict endpoint comparisons. A zero-width interval therefore
            // still overlaps when its coordinate lies strictly inside the other
            // interval. The role collector must conservatively fall back per role.
            AssertMixedDegenerateRoleParity(
                mixedParticipantIsAttacker: true,
                expectedBodyX: 40);
            AssertMixedDegenerateRoleParity(
                mixedParticipantIsAttacker: false,
                expectedBodyX: 50);
        }

        [TestCase(17)]
        [TestCase(911)]
        [TestCase(20260725)]
        public void Formal_RandomizedRolesMatchBruteAcrossIndexableAndDegenerateBounds(int seed)
        {
            const int participantCount = 32;
            var random = new System.Random(seed);
            var world = new SimulationWorld();
            var entitiesBySlot = new LF2Character[participantCount];
            var attackers = new List<LF2Entity>(participantCount);
            var registrationOrder = new List<int>(participantCount);
            int expectedFallbackParticipants = 0;

            for (int slot = 0; slot < participantCount; slot++)
            {
                bool hasItr = random.Next(0, 4) != 0;
                bool hasBody = random.Next(0, 4) != 0;
                bool degenerateItr = hasItr && random.Next(0, 5) == 0;
                bool degenerateBody = hasBody && random.Next(0, 5) == 0;
                if (slot == 0)
                {
                    hasItr = true;
                    hasBody = false;
                    degenerateItr = true;
                }
                else if (slot == 1)
                {
                    hasItr = false;
                    hasBody = true;
                    degenerateBody = false;
                }
                else if (slot == 2)
                {
                    hasItr = true;
                    hasBody = false;
                    degenerateItr = false;
                }
                else if (slot == 3)
                {
                    hasItr = false;
                    hasBody = true;
                    degenerateBody = true;
                }

                InteractionArea itr = hasItr
                    ? new InteractionArea
                    {
                        kind = 0,
                        vrest = random.Next(0, 2),
                        x = random.Next(-15, 16),
                        y = random.Next(-15, 1),
                        w = degenerateItr ? 0 : random.Next(6, 25),
                        h = random.Next(6, 25),
                        zwidth = random.Next(1, 24),
                        injury = random.Next(0, 50),
                        dvx = random.Next(-3, 4),
                        effect = random.Next(0, 2),
                    }
                    : null;
                BodyBox body = hasBody
                    ? new BodyBox
                    {
                        kind = 0,
                        x = random.Next(-15, 16),
                        y = random.Next(-15, 1),
                        w = degenerateBody ? 0 : random.Next(6, 25),
                        h = random.Next(6, 25),
                    }
                    : null;
                if (degenerateItr || degenerateBody || (!hasItr && !hasBody))
                    expectedFallbackParticipants++;

                LF2Character entity = CreateCharacter(
                    $"RoleFormal_Random_{seed}_{slot}",
                    1000 + slot,
                    MakeFrame(itr, body));
                entitiesBySlot[slot] = entity;
                registrationOrder.Add(slot);
                if (hasItr)
                    attackers.Add(entity);
            }

            for (int i = registrationOrder.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(0, i + 1);
                (registrationOrder[i], registrationOrder[swapIndex]) =
                    (registrationOrder[swapIndex], registrationOrder[i]);
            }

            for (int orderIndex = 0;
                 orderIndex < registrationOrder.Count;
                 orderIndex++)
            {
                int slot = registrationOrder[orderIndex];
                LF2Character entity = entitiesBySlot[slot];
                int x = random.Next(-120, 121);
                int z = random.Next(-20, 21);
                Register(world, entity, slot, (slot % 3) + 1, x);
                entity.Runtime.SetPosition(x, 0, z);
                entity.Runtime.SyncIntegerPosition();
            }

            uint collectionSeed = unchecked((uint)(seed * 397) ^ 0xA341316Cu);
            BruteForceSceneQuery query = GetQuery(world);
            LF2Entity[] trackedAttackers = attackers.ToArray();
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                collectionSeed,
                trackedAttackers);
            CandidateRun legacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceLegacyUnionAabb,
                collectionSeed,
                trackedAttackers);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                collectionSeed,
                trackedAttackers);

            string diagnostics = FormatRandomizedParityDiagnostics(
                seed,
                entitiesBySlot,
                brute,
                legacy,
                role);
            AssertRunsEqual(brute, role, diagnostics);
            Assert.That(brute.CollectionAborted, Is.False, diagnostics);
            Assert.That(legacy.CollectionAborted, Is.False);
            Assert.That(role.CollectionAborted, Is.False, diagnostics);
            Assert.That(
                role.FallbackParticipantCount,
                Is.EqualTo(expectedFallbackParticipants),
                diagnostics);
            Assert.That(role.BodyEntryCount, Is.GreaterThan(0), diagnostics);
            Assert.That(role.ItrQueryCount, Is.GreaterThan(0), diagnostics);
        }

        [Test]
        [Timeout(30000)]
        public void Formal_ThousandSyntheticConfiguredLooseDropsPairsAndMatchesBrute()
        {
            const int participantCount = 1000;
            const int attackOffsetParticipants = 500;
            const int spacing = 20;
            LF2FrameData sharedFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 0,
                    vrest = 1,
                    x = attackOffsetParticipants * spacing,
                    y = -5,
                    w = 10,
                    h = 10,
                    zwidth = 15,
                },
                new BodyBox { kind = 0, x = 0, y = -5, w = 10, h = 10 });
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                1100,
                CollisionBroadphaseBackend.LooseQuadtree);
            var participants = new LF2Entity[participantCount];
            for (int slot = 0; slot < participantCount; slot++)
            {
                LF2Character entity = CreateCharacter(
                    $"RoleFormal_Thousand_{slot}",
                    1,
                    sharedFrame);
                int team = slot < attackOffsetParticipants ? 1 : 2;
                Register(world, entity, slot, team, slot * spacing);
                participants[slot] = entity;
            }

            BruteForceSceneQuery query = GetQuery(world);
            int gameDataManagerLogCount = 0;
            string firstGameDataManagerLog = null;
            Application.LogCallback captureTypeResolutionLogs =
                (condition, stackTrace, type) =>
                {
                    if (string.IsNullOrEmpty(condition) ||
                        condition.IndexOf(
                            "GameDataManager",
                            StringComparison.Ordinal) < 0)
                    {
                        return;
                    }

                    gameDataManagerLogCount++;
                    if (firstGameDataManagerLog == null)
                        firstGameDataManagerLog = $"{type}: {condition}";
            };
            CandidateRun brute;
            CandidateRun configured;
            Application.logMessageReceived += captureTypeResolutionLogs;
            try
            {
                brute = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceBruteForce,
                    CollectionSeed,
                    participants);
                configured = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.Configured,
                    CollectionSeed,
                    participants);
            }
            finally
            {
                Application.logMessageReceived -= captureTypeResolutionLogs;
            }

            AssertRunsEqual(brute, configured);
            Assert.That(
                query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(
                query.LastFormalCollectorModeForDiagnostics,
                Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
            Assert.That(
                gameDataManagerLogCount,
                Is.Zero,
                firstGameDataManagerLog);
            Assert.That(configured.CollectionAborted, Is.False);
            Assert.That(configured.FallbackParticipantCount, Is.Zero);
            Assert.That(
                configured.FormalPairCount,
                Is.EqualTo(attackOffsetParticipants));
            int bruteAuthorityPairCount =
                participantCount * (participantCount - 1) / 2;
            Assert.That(
                configured.FormalPairCount * 50,
                Is.LessThan(bruteAuthorityPairCount));
            Assert.That(
                configured.BodyEntryCount,
                Is.EqualTo(participantCount));
            Assert.That(
                configured.ItrQueryCount,
                Is.EqualTo(participantCount));
            Assert.That(configured.RngCalls, Is.Zero);
        }

        [Test]
        public void Formal_GenerationReuseAndMidCollectionFailureRetainParity()
        {
            var world = new SimulationWorld();
            LF2Character otherTarget = CreateCharacter(
                "RoleFormal_GenerationOther",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character oldTarget = CreateCharacter(
                "RoleFormal_GenerationOld",
                3,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character attacker = CreateCharacter(
                "RoleFormal_GenerationAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 0,
                        x = -30,
                        y = -10,
                        w = 60,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            Register(world, otherTarget, 0, 2, -10);
            Register(world, oldTarget, 1, 2, 10);
            Register(world, attacker, 2, 1, 0);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    1,
                    oldTarget,
                    out RuntimeEntityHandle oldHandle),
                Is.True);

            BruteForceSceneQuery query = GetQuery(world);
            RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);
            world.Unregister(oldTarget);
            Assert.That(
                world.TryResolveRuntimeHandleForDiagnostics(oldHandle, out _),
                Is.False);

            LF2Character replacement = CreateCharacter(
                "RoleFormal_GenerationReplacement",
                4,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            Register(world, replacement, 1, 2, 10);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    1,
                    replacement,
                    out RuntimeEntityHandle replacementHandle),
                Is.True);
            Assert.That(replacementHandle, Is.Not.EqualTo(oldHandle));

            uint replacementWinsSeed = FindSeedWithFirstTieReplacement();
            CandidateRun legacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceLegacyUnionAabb,
                replacementWinsSeed,
                attacker);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                replacementWinsSeed,
                attacker);
            AssertRunsEqual(legacy, role);
            Assert.That(role.CollectionAborted, Is.False);
            Assert.That(role.TargetHandles[0], Has.Count.EqualTo(1));
            Assert.That(role.TargetHandles[0][0], Is.EqualTo(replacementHandle));

            query.ThrowAfterRoleAwareFormalPairCountForSelfCheck = 2;
            CandidateRun recovered;
            try
            {
                recovered = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceRoleAware,
                    replacementWinsSeed,
                    attacker);
            }
            finally
            {
                query.ThrowAfterRoleAwareFormalPairCountForSelfCheck = -1;
            }

            AssertRunsEqual(legacy, recovered);
            Assert.That(recovered.CollectionAborted, Is.True);
            Assert.That(recovered.RngCalls, Is.EqualTo(1));
            Assert.That(recovered.TargetHandles[0][0], Is.EqualTo(replacementHandle));
            Assert.That(
                world.TryResolveRuntimeHandleForDiagnostics(oldHandle, out _),
                Is.False);
        }

        [Test]
        public void Formal_ExceptionRestoresRngAndCandidatesThenRunsFullBruteFallback()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_RollbackAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 0,
                        x = -30,
                        y = -10,
                        w = 60,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character leftTarget = CreateCharacter(
                "RoleFormal_RollbackLeft",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character rightTarget = CreateCharacter(
                "RoleFormal_RollbackRight",
                3,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            Register(world, leftTarget, 0, 2, -10);
            Register(world, rightTarget, 1, 2, 10);
            Register(world, attacker, 2, 1, 0);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker);

            query.ThrowAfterRoleAwareFormalPairCountForSelfCheck = 2;
            CandidateRun recovered = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);
            query.ThrowAfterRoleAwareFormalPairCountForSelfCheck = -1;

            AssertRunsEqual(brute, recovered);
            Assert.That(brute.RngCalls, Is.GreaterThan(0));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.True);
            Assert.That(query.LastFormalSynchronizeResultForDiagnostics.Succeeded, Is.False);
        }

        private static uint FindSeedWithFirstTieReplacement()
        {
            for (uint seed = 1; seed < 1024; seed++)
            {
                var rng = new DeterministicRng(seed);
                if (rng.NextInt(0, 2) == 0)
                    return seed;
            }

            Assert.Fail("Could not find a deterministic tie-replacement seed.");
            return 0;
        }

        private static void AssertMixedDegenerateRoleParity(
            bool mixedParticipantIsAttacker,
            int expectedBodyX)
        {
            var world = new SimulationWorld();
            LF2Character mixed;
            LF2Character other;
            LF2Entity trackedAttacker;
            if (mixedParticipantIsAttacker)
            {
                mixed = CreateCharacter(
                    "RoleFormal_MixedDegenerateItr",
                    1200,
                    MakeFrame(
                        new InteractionArea
                        {
                            kind = 0,
                            vrest = 1,
                            x = 50,
                            y = -10,
                            w = 0,
                            h = 20,
                            zwidth = 15,
                        },
                        new BodyBox
                        {
                            kind = 0,
                            x = 0,
                            y = -10,
                            w = 10,
                            h = 20,
                        }));
                other = CreateCharacter(
                    "RoleFormal_ValidBodyTarget",
                    1201,
                    MakeFrame(
                        null,
                        new BodyBox
                        {
                            kind = 0,
                            x = 40,
                            y = -10,
                            w = 20,
                            h = 20,
                        }));
                trackedAttacker = mixed;
            }
            else
            {
                other = CreateCharacter(
                    "RoleFormal_ValidItrAttacker",
                    1202,
                    MakeFrame(
                        new InteractionArea
                        {
                            kind = 0,
                            vrest = 1,
                            x = 40,
                            y = -10,
                            w = 20,
                            h = 20,
                            zwidth = 15,
                        },
                        null));
                mixed = CreateCharacter(
                    "RoleFormal_MixedDegenerateBody",
                    1203,
                    MakeFrame(
                        new InteractionArea
                        {
                            kind = 0,
                            vrest = 1,
                            x = 100,
                            y = -10,
                            w = 10,
                            h = 20,
                            zwidth = 15,
                        },
                        new BodyBox
                        {
                            kind = 0,
                            x = 50,
                            y = -10,
                            w = 0,
                            h = 20,
                        }));
                trackedAttacker = other;
            }

            Register(world, mixed, 0, 1, 0);
            Register(world, other, 1, 2, 0);
            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                trackedAttacker);
            CandidateRun legacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceLegacyUnionAabb,
                trackedAttacker);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                trackedAttacker);
            string context =
                $"mixedParticipantIsAttacker={mixedParticipantIsAttacker}\n" +
                FormatParticipantDiagnostics(new[] { mixed, other }) +
                FormatRunDiagnostics("brute", brute) +
                FormatRunDiagnostics("legacy", legacy) +
                FormatRunDiagnostics("role", role);

            AssertRunsEqual(brute, role, context);
            Assert.That(brute.Counts, Is.EqualTo(new[] { 1 }), context);
            Assert.That(brute.Sequences[0][0].BodyX, Is.EqualTo(expectedBodyX), context);
            Assert.That(legacy.Counts, Is.EqualTo(new[] { 0 }), context);
            Assert.That(legacy.FormalPairCount, Is.Zero, context);
            Assert.That(role.FormalPairCount, Is.EqualTo(1), context);
            Assert.That(role.FallbackParticipantCount, Is.EqualTo(1), context);
        }

        private static CandidateRun RunCollection(
            SimulationWorld world,
            BruteForceSceneQuery query,
            CollisionFormalCollectorMode mode,
            params LF2Entity[] attackers)
        {
            return RunCollection(world, query, mode, CollectionSeed, attackers);
        }

        private static CandidateRun RunCollection(
            SimulationWorld world,
            BruteForceSceneQuery query,
            CollisionFormalCollectorMode mode,
            uint seed,
            params LF2Entity[] attackers)
        {
            query.FormalCollectorMode = mode;
            world.Rng.Seed(seed);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            var sequences = new List<List<SceneQueryHit>>(attackers.Length);
            var targetHandles = new List<List<RuntimeEntityHandle>>(attackers.Length);
            var counts = new List<int>(attackers.Length);
            var attackerSlots = new List<int>(attackers.Length);
            var formalRuntimeSlotPairKeys = new List<long>();
            query.CopyLastFormalRuntimeSlotPairKeysForSelfCheck(
                formalRuntimeSlotPairKeys);
            for (int attackerIndex = 0; attackerIndex < attackers.Length; attackerIndex++)
            {
                LF2Entity attacker = attackers[attackerIndex];
                attackerSlots.Add(attacker.Runtime?.SlotIndex ?? -1);
                Assert.That(
                    query.TryGetCollisionCandidateSequence(
                        attacker,
                        out List<SceneQueryHit> sequence),
                    Is.True);
                sequences.Add(new List<SceneQueryHit>(sequence));
                var handles = new List<RuntimeEntityHandle>(sequence.Count);
                for (int candidateIndex = 0;
                     candidateIndex < sequence.Count;
                     candidateIndex++)
                {
                    SceneQueryHit candidate = sequence[candidateIndex];
                    Assert.That(
                        world.TryGetCurrentRuntimeHandleForDiagnostics(
                            candidate.TargetSlot,
                            candidate.Target,
                            out RuntimeEntityHandle targetHandle),
                        Is.True);
                    handles.Add(targetHandle);
                }
                targetHandles.Add(handles);
                counts.Add(attacker.Runtime.HitCandidateCount);
            }

            var result = new CandidateRun(
                sequences,
                targetHandles,
                counts,
                world.Rng.State,
                world.Rng.CallCount,
                query.LastFormalPairCountForDiagnostics,
                query.LastFormalFallbackParticipantCountForDiagnostics,
                query.LastFormalCollectionAbortedForDiagnostics,
                query.LastRoleAwareBodyEntryCountForDiagnostics,
                query.LastRoleAwareItrQueryCountForDiagnostics,
                formalRuntimeSlotPairKeys,
                attackerSlots);
            world.EndCollisionCandidateConsumption();
            return result;
        }

        private static void AssertRunsEqual(
            CandidateRun expected,
            CandidateRun actual,
            string diagnostics = null)
        {
            Assert.That(actual.RngState, Is.EqualTo(expected.RngState), diagnostics);
            Assert.That(actual.RngCalls, Is.EqualTo(expected.RngCalls), diagnostics);
            Assert.That(
                actual.Sequences.Count,
                Is.EqualTo(expected.Sequences.Count),
                diagnostics);
            Assert.That(
                actual.TargetHandles.Count,
                Is.EqualTo(expected.TargetHandles.Count),
                diagnostics);
            Assert.That(actual.Counts, Is.EqualTo(expected.Counts), diagnostics);
            for (int attackerIndex = 0;
                 attackerIndex < expected.Sequences.Count;
                 attackerIndex++)
            {
                List<SceneQueryHit> expectedSequence = expected.Sequences[attackerIndex];
                List<SceneQueryHit> actualSequence = actual.Sequences[attackerIndex];
                Assert.That(
                    actualSequence.Count,
                    Is.EqualTo(expectedSequence.Count),
                    diagnostics);
                Assert.That(
                    actual.TargetHandles[attackerIndex],
                    Is.EqualTo(expected.TargetHandles[attackerIndex]),
                    diagnostics);
                for (int candidateIndex = 0;
                     candidateIndex < expectedSequence.Count;
                     candidateIndex++)
                {
                    SceneQueryHit expectedHit = expectedSequence[candidateIndex];
                    SceneQueryHit actualHit = actualSequence[candidateIndex];
                    Assert.That(
                        actualHit.TargetSlot,
                        Is.EqualTo(expectedHit.TargetSlot),
                        diagnostics);
                    Assert.That(
                        actualHit.ItrIndex,
                        Is.EqualTo(expectedHit.ItrIndex),
                        diagnostics);
                    Assert.That(
                        actualHit.BodyX,
                        Is.EqualTo(expectedHit.BodyX),
                        diagnostics);
                    Assert.That(
                        actualHit.ZeroAttackerHpOnConsume,
                        Is.EqualTo(expectedHit.ZeroAttackerHpOnConsume),
                        diagnostics);
                    Assert.That(
                        actualHit.ReleaseHeavyHeldTargetOnConsume,
                        Is.EqualTo(expectedHit.ReleaseHeavyHeldTargetOnConsume),
                        diagnostics);
                    Assert.That(
                        actualHit.RuntimeItr,
                        Is.SameAs(expectedHit.RuntimeItr),
                        diagnostics);
                    AssertItrFieldsEqual(
                        expectedHit.RuntimeItr,
                        actualHit.RuntimeItr,
                        diagnostics);
                }
            }
        }

        private static void AssertItrFieldsEqual(
            InteractionArea expected,
            InteractionArea actual,
            string diagnostics = null)
        {
            if (expected == null || actual == null)
            {
                Assert.That(actual, Is.SameAs(expected), diagnostics);
                return;
            }

            Assert.That(actual.kind, Is.EqualTo(expected.kind), diagnostics);
            Assert.That(actual.x, Is.EqualTo(expected.x), diagnostics);
            Assert.That(actual.y, Is.EqualTo(expected.y), diagnostics);
            Assert.That(actual.w, Is.EqualTo(expected.w), diagnostics);
            Assert.That(actual.h, Is.EqualTo(expected.h), diagnostics);
            Assert.That(actual.zwidth, Is.EqualTo(expected.zwidth), diagnostics);
            Assert.That(actual.dvx, Is.EqualTo(expected.dvx), diagnostics);
            Assert.That(actual.dvy, Is.EqualTo(expected.dvy), diagnostics);
            Assert.That(actual.dvz, Is.EqualTo(expected.dvz), diagnostics);
            Assert.That(actual.injury, Is.EqualTo(expected.injury), diagnostics);
            Assert.That(actual.fall, Is.EqualTo(expected.fall), diagnostics);
            Assert.That(actual.vaction, Is.EqualTo(expected.vaction), diagnostics);
            Assert.That(actual.arest, Is.EqualTo(expected.arest), diagnostics);
            Assert.That(actual.vrest, Is.EqualTo(expected.vrest), diagnostics);
            Assert.That(actual.effect, Is.EqualTo(expected.effect), diagnostics);
            Assert.That(actual.kill, Is.EqualTo(expected.kill), diagnostics);
            Assert.That(actual.bdefend, Is.EqualTo(expected.bdefend), diagnostics);
            Assert.That(actual.attacking, Is.EqualTo(expected.attacking), diagnostics);
            Assert.That(actual.throwvz, Is.EqualTo(expected.throwvz), diagnostics);
            Assert.That(actual.respond, Is.EqualTo(expected.respond), diagnostics);
            Assert.That(actual.pickingact, Is.EqualTo(expected.pickingact), diagnostics);
            Assert.That(actual.pickedact, Is.EqualTo(expected.pickedact), diagnostics);
            Assert.That(actual.throwvx, Is.EqualTo(expected.throwvx), diagnostics);
            Assert.That(actual.throwvy, Is.EqualTo(expected.throwvy), diagnostics);
            Assert.That(actual.throwinjury, Is.EqualTo(expected.throwinjury), diagnostics);
            Assert.That(actual.catchingact, Is.EqualTo(expected.catchingact), diagnostics);
            Assert.That(actual.caughtact, Is.EqualTo(expected.caughtact), diagnostics);
            Assert.That(actual.catchingact2, Is.EqualTo(expected.catchingact2), diagnostics);
            Assert.That(actual.caughtact2, Is.EqualTo(expected.caughtact2), diagnostics);
        }

        private static string FormatRandomizedParityDiagnostics(
            int seed,
            LF2Entity[] participants,
            CandidateRun brute,
            CandidateRun legacy,
            CandidateRun role)
        {
            var builder = new StringBuilder(8192);
            builder.Append("seed=").Append(seed).AppendLine();
            builder.Append(FormatParticipantDiagnostics(participants));
            builder.Append(FormatRunDiagnostics("brute", brute));
            builder.Append(FormatRunDiagnostics("legacy", legacy));
            builder.Append(FormatRunDiagnostics("role", role));
            AppendPairSetDifference(
                builder,
                "legacy-minus-role",
                legacy.FormalRuntimeSlotPairKeys,
                role.FormalRuntimeSlotPairKeys);
            AppendPairSetDifference(
                builder,
                "role-minus-legacy",
                role.FormalRuntimeSlotPairKeys,
                legacy.FormalRuntimeSlotPairKeys);
            return builder.ToString();
        }

        private static string FormatParticipantDiagnostics(LF2Entity[] participants)
        {
            var builder = new StringBuilder(4096);
            builder.AppendLine("participants:");
            for (int participantIndex = 0;
                 participantIndex < participants.Length;
                 participantIndex++)
            {
                LF2Entity entity = participants[participantIndex];
                if (entity == null)
                {
                    builder.Append("  [").Append(participantIndex).AppendLine("] null");
                    continue;
                }

                LF2FrameData frame = entity.GetCollisionFrameData();
                int x = entity.Runtime?.XInt ?? 0;
                int y = entity.Runtime?.YInt ?? 0;
                int z = entity.Runtime?.ZInt ?? 0;
                bool left = entity.PS?.dir == "left";
                builder.Append("  slot=")
                    .Append(entity.Runtime?.SlotIndex ?? -1)
                    .Append(" team=")
                    .Append(entity.RelationTeam)
                    .Append(" pos=(")
                    .Append(x).Append(',').Append(y).Append(',').Append(z)
                    .Append(") dir=")
                    .Append(left ? "left" : "right");

                if (frame == null)
                {
                    builder.AppendLine(" frame=null");
                    continue;
                }

                builder.Append(" frame=").Append(frame.frameId);
                if (frame.itrs != null)
                {
                    for (int itrIndex = 0; itrIndex < frame.itrs.Count; itrIndex++)
                    {
                        InteractionArea itr = frame.itrs[itrIndex];
                        if (itr == null)
                        {
                            builder.Append(" itr[").Append(itrIndex).Append("]=null");
                            continue;
                        }

                        ResolveWorldX(
                            x,
                            frame.centerx,
                            itr.x,
                            itr.w,
                            left,
                            out int x1,
                            out int x2);
                        int y1 = y - frame.centery + itr.y;
                        int y2 = y1 + itr.h;
                        int zHalf = itr.zwidth > 0 ? itr.zwidth : 15;
                        builder.Append(" itr[").Append(itrIndex).Append("]={k=")
                            .Append(itr.kind)
                            .Append(",local=")
                            .Append(itr.x).Append(',').Append(itr.y).Append(',')
                            .Append(itr.w).Append(',').Append(itr.h)
                            .Append(",world=")
                            .Append(x1).Append(',').Append(y1).Append("..")
                            .Append(x2).Append(',').Append(y2)
                            .Append(",z=")
                            .Append(z - zHalf).Append("..").Append(z + zHalf)
                            .Append(",indexableAabb=")
                            .Append(x1 < x2 && z - zHalf < z + zHalf)
                            .Append('}');
                    }
                }

                if (frame.bodies != null)
                {
                    for (int bodyIndex = 0; bodyIndex < frame.bodies.Count; bodyIndex++)
                    {
                        BodyBox body = frame.bodies[bodyIndex];
                        if (body == null)
                        {
                            builder.Append(" body[").Append(bodyIndex).Append("]=null");
                            continue;
                        }

                        ResolveWorldX(
                            x,
                            frame.centerx,
                            body.x,
                            body.w,
                            left,
                            out int x1,
                            out int x2);
                        int y1 = y - frame.centery + body.y;
                        int y2 = y1 + body.h;
                        builder.Append(" body[").Append(bodyIndex).Append("]={local=")
                            .Append(body.x).Append(',').Append(body.y).Append(',')
                            .Append(body.w).Append(',').Append(body.h)
                            .Append(",world=")
                            .Append(x1).Append(',').Append(y1).Append("..")
                            .Append(x2).Append(',').Append(y2)
                            .Append(",z=")
                            .Append(z).Append("..").Append(z + 1)
                            .Append(",indexableAabb=")
                            .Append(x1 < x2)
                            .Append('}');
                    }
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static void ResolveWorldX(
            int entityX,
            int centerX,
            int localX,
            int width,
            bool left,
            out int x1,
            out int x2)
        {
            if (!left)
            {
                x1 = entityX - centerX + localX;
                x2 = x1 + width;
                return;
            }

            x2 = entityX + centerX - localX;
            x1 = x2 - width;
        }

        private static string FormatRunDiagnostics(string label, CandidateRun run)
        {
            var builder = new StringBuilder(2048);
            builder.Append(label)
                .Append(": rng=").Append(run.RngState)
                .Append('/').Append(run.RngCalls)
                .Append(" pairs=").Append(run.FormalPairCount)
                .Append(" fallback=").Append(run.FallbackParticipantCount)
                .Append(" aborted=").Append(run.CollectionAborted)
                .Append(" bodies=").Append(run.BodyEntryCount)
                .Append(" itrQueries=").Append(run.ItrQueryCount)
                .Append(" pairKeys=");
            AppendPairKeys(builder, run.FormalRuntimeSlotPairKeys);
            builder.AppendLine();
            for (int attackerIndex = 0;
                 attackerIndex < run.Sequences.Count;
                 attackerIndex++)
            {
                builder.Append("  attacker=")
                    .Append(run.AttackerSlots[attackerIndex])
                    .Append(" count=")
                    .Append(run.Counts[attackerIndex])
                    .Append(" candidates=");
                List<SceneQueryHit> sequence = run.Sequences[attackerIndex];
                for (int candidateIndex = 0;
                     candidateIndex < sequence.Count;
                     candidateIndex++)
                {
                    if (candidateIndex > 0)
                        builder.Append(',');
                    SceneQueryHit hit = sequence[candidateIndex];
                    RuntimeEntityHandle handle =
                        run.TargetHandles[attackerIndex][candidateIndex];
                    builder.Append(handle)
                        .Append("/itr").Append(hit.ItrIndex)
                        .Append("/bodyX").Append(hit.BodyX);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        private static void AppendPairSetDifference(
            StringBuilder builder,
            string label,
            List<long> first,
            List<long> second)
        {
            builder.Append(label).Append('=');
            bool wroteAny = false;
            for (int firstIndex = 0; firstIndex < first.Count; firstIndex++)
            {
                long key = first[firstIndex];
                if (second.BinarySearch(key) >= 0)
                    continue;
                if (wroteAny)
                    builder.Append(',');
                AppendPairKey(builder, key);
                wroteAny = true;
            }
            if (!wroteAny)
                builder.Append("[]");
            builder.AppendLine();
        }

        private static void AppendPairKeys(StringBuilder builder, List<long> pairKeys)
        {
            builder.Append('[');
            for (int pairIndex = 0; pairIndex < pairKeys.Count; pairIndex++)
            {
                if (pairIndex > 0)
                    builder.Append(',');
                AppendPairKey(builder, pairKeys[pairIndex]);
            }
            builder.Append(']');
        }

        private static void AppendPairKey(StringBuilder builder, long pairKey)
        {
            builder.Append((int)(pairKey >> 32))
                .Append('-')
                .Append((int)(pairKey & 0xffffffffL));
        }

        private static BruteForceSceneQuery GetQuery(SimulationWorld world)
        {
            Assert.That(world.SceneQuery, Is.TypeOf<BruteForceSceneQuery>());
            return (BruteForceSceneQuery)world.SceneQuery;
        }

        private static LF2FrameData MakeFrame(InteractionArea itr, BodyBox body)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (itr != null)
                frame.itrs.Add(itr);
            if (body != null)
                frame.bodies.Add(body);
            return frame;
        }

        private static LF2Character CreateCharacter(
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new FormalSelfCheckCharacter();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new FormalSelfCheckController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            return character;
        }

        private static void Register(
            SimulationWorld world,
            LF2Entity entity,
            int requiredSlot,
            int team,
            int x)
        {
            entity.SetRequiredRuntimeSlot(requiredSlot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(requiredSlot));
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.FrameDelay = 0;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class CandidateRun
        {
            public CandidateRun(
                List<List<SceneQueryHit>> sequences,
                List<List<RuntimeEntityHandle>> targetHandles,
                List<int> counts,
                uint rngState,
                ulong rngCalls,
                int formalPairCount,
                int fallbackParticipantCount,
                bool collectionAborted,
                int bodyEntryCount,
                int itrQueryCount,
                List<long> formalRuntimeSlotPairKeys,
                List<int> attackerSlots)
            {
                Sequences = sequences;
                TargetHandles = targetHandles;
                Counts = counts;
                RngState = rngState;
                RngCalls = rngCalls;
                FormalPairCount = formalPairCount;
                FallbackParticipantCount = fallbackParticipantCount;
                CollectionAborted = collectionAborted;
                BodyEntryCount = bodyEntryCount;
                ItrQueryCount = itrQueryCount;
                FormalRuntimeSlotPairKeys = formalRuntimeSlotPairKeys;
                AttackerSlots = attackerSlots;
            }

            public List<List<SceneQueryHit>> Sequences { get; }
            public List<List<RuntimeEntityHandle>> TargetHandles { get; }
            public List<int> Counts { get; }
            public uint RngState { get; }
            public ulong RngCalls { get; }
            public int FormalPairCount { get; }
            public int FallbackParticipantCount { get; }
            public bool CollectionAborted { get; }
            public int BodyEntryCount { get; }
            public int ItrQueryCount { get; }
            public List<long> FormalRuntimeSlotPairKeys { get; }
            public List<int> AttackerSlots { get; }
        }

        private sealed class FormalSelfCheckCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }

        private sealed class FormalSelfCheckController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
