#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Spatial;
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

        [Test]
        public void CollisionRoleZeroItrFastPath_DefaultOff_DoesNotApply()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(
                "RoleZero_DefaultOff",
                3010,
                MakeFrame(null, null));
            RegisterPair(world, entity, CreateCharacter("RoleZero_DefaultOffTarget", 3011,
                MakeFrame(null, null)));
            var query = (BruteForceSceneQuery)world.SceneQuery;
            ConfigureRoleZeroItrStoreOnly(query);

            Assert.That(query.CollisionRoleZeroItrFastPathEnabled, Is.False);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.CollisionRoleZeroItrFastPathAppliedCountForDiagnostics, Is.Zero);
            Assert.That(query.LastRoleAwareCheapInputValidationCountForDiagnostics, Is.EqualTo(1),
                "default-off must retain the existing role-aware validation path");
            Assert.That(query.LastRoleAwareDirectTickCountForDiagnostics, Is.EqualTo(1));
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void CollisionRoleZeroItrFastPath_NullItrClearsCarriersAndBuildsEmptyStore()
        {
            var world = new SimulationWorld();
            LF2FrameData nullItrFrame = MakeFrame(null, null);
            nullItrFrame.itrs.Add(null);
            LF2Character entity = CreateCharacter("RoleZero_NullItr", 3020, nullItrFrame);
            LF2Character target = CreateCharacter("RoleZero_NullItrTarget", 3021,
                MakeFrame(null, null));
            RegisterPair(world, entity, target);
            entity.Runtime.HitCandidateCount = 7;
            entity.Runtime.HitCandidateNearestDistance = 1;
            var query = (BruteForceSceneQuery)world.SceneQuery;
            ConfigureRoleZeroItrStoreOnly(query);
            query.SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(true);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            Assert.That(query.CollisionRoleZeroItrFastPathAppliedCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.CollisionRoleZeroItrFastPathZeroItrCountForDiagnostics, Is.EqualTo(1));
            Assert.That(entity.Runtime.HitCandidateCount, Is.Zero);
            Assert.That(entity.Runtime.HitCandidateNearestDistance, Is.EqualTo(1000));
            Assert.That(query.LastRoleAwareParticipantCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastRoleAwareCheapInputValidationCountForDiagnostics, Is.Zero);
            Assert.That(query.LastRoleAwareDirectTickCountForDiagnostics, Is.Zero);
            Assert.That(query.CollisionCandidateStoreAuthorityAppliedForCurrentTickForDiagnostics,
                Is.True);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    entity.Runtime.SlotIndex,
                    entity,
                    out RuntimeEntityHandle entityHandle),
                Is.True);
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    entityHandle,
                    out int candidateCount),
                Is.True);
            Assert.That(candidateCount, Is.Zero);
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void CollisionRoleZeroItrFastPath_ValidItrAtHighestSlotFallsBack()
        {
            var world = new SimulationWorld();
            LF2Character target = CreateCharacter("RoleZero_HighTarget", 3030,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character attacker = CreateCharacter("RoleZero_HighAttacker", 3031,
                MakeFrame(new InteractionArea { kind = 0, x = -10, y = -10, w = 20, h = 20, zwidth = 15 }, null));
            RegisterAtSlot(world, target, 0, 2, 0);
            RegisterAtSlot(world, attacker, world.RuntimeSlotCapacityForDiagnostics - 1, 1, 0);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            ConfigureRoleZeroItrStoreOnly(query);
            query.SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(true);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            Assert.That(query.CollisionRoleZeroItrFastPathAppliedCountForDiagnostics, Is.Zero);
            Assert.That(query.CollisionRoleZeroItrFastPathFallbackCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareItrQueryCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareCheapInputValidationCountForDiagnostics, Is.EqualTo(1));
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void CollisionRoleZeroItrFastPath_DegenerateAuthoredItrFallsBackAndMatchesOriginalCollector()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleZero_DegenerateAttacker",
                3040,
                MakeFrame(
                    new InteractionArea { kind = 0, x = 0, y = -10, w = 0, h = 20, zwidth = 15 },
                    null));
            LF2Character target = CreateCharacter(
                "RoleZero_DegenerateTarget",
                3041,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            RegisterPair(world, attacker, target);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            ConfigureRoleZeroItrStoreOnly(query);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.TryGetCollisionCandidateSequence(
                attacker,
                out List<SceneQueryHit> originalCandidates), Is.True);
            int originalCandidateCount = originalCandidates.Count;
            int originalRuntimeCandidateCount = attacker.Runtime.HitCandidateCount;
            world.EndCollisionCandidateConsumption();

            query.SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(true);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            Assert.That(query.CollisionRoleZeroItrFastPathAppliedCountForDiagnostics, Is.Zero);
            Assert.That(query.CollisionRoleZeroItrFastPathFallbackCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareCheapInputValidationCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareTreeTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.TryGetLastRoleAwareParticipantFlagsForSelfCheck(
                attacker,
                out _,
                out _,
                out bool hasAttackItr,
                out bool hasFallbackAttackItr), Is.True);
            Assert.That(hasAttackItr, Is.True);
            Assert.That(hasFallbackAttackItr, Is.True);
            Assert.That(query.TryGetCollisionCandidateSequence(
                attacker,
                out List<SceneQueryHit> fallbackCandidates), Is.True);
            Assert.That(fallbackCandidates.Count, Is.EqualTo(originalCandidateCount));
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(originalRuntimeCandidateCount));
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void CollisionRoleZeroItrFastPath_CandidateStoreShadowDiagnosticsFallsBack()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter(
                "RoleZero_CandidateStoreShadow",
                3045,
                MakeFrame(null, null));
            RegisterPair(world, entity, CreateCharacter(
                "RoleZero_CandidateStoreShadowTarget",
                3046,
                MakeFrame(null, null)));
            var query = (BruteForceSceneQuery)world.SceneQuery;
            ConfigureRoleZeroItrStoreOnly(query);
            query.CollisionCandidateStoreShadowDiagnosticsEnabled = true;
            query.SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(true);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            Assert.That(query.CollisionRoleZeroItrFastPathAppliedCountForDiagnostics, Is.Zero);
            Assert.That(query.CollisionRoleZeroItrFastPathFallbackCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareCheapInputValidationCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareDirectTickCountForDiagnostics, Is.EqualTo(1));
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void CollisionRoleZeroItrFastPath_WarmedCollectionAllocatesNothing()
        {
            var world = new SimulationWorld();
            LF2Character entity = CreateCharacter("RoleZero_Allocation", 3050,
                MakeFrame(null, null));
            RegisterAtSlot(world, entity, 0, 1, 0);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            ConfigureRoleZeroItrStoreOnly(query);
            query.SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(true);
            world.CaptureCollisionFrameSnapshotsAll();

            Assert.That(
                query.MeasureWarmedCollisionRoleZeroItrFastPathAllocationsForSelfCheck(128),
                Is.Zero);
        }

        private static void ConfigureRoleZeroItrStoreOnly(BruteForceSceneQuery query)
        {
            query.CollisionCandidateStoreAuthorityEnabled = true;
            query.CollisionCandidateStoreLegacyOracleInterval = 0;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
        }

        private static void RegisterAtSlot(
            SimulationWorld world,
            LF2Entity entity,
            int slot,
            int team,
            int x)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            Configure(entity, team);
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SyncIntegerPosition();
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
            query.ForceRoleAwareTreeForDiagnostics = true;
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
            registeredFirstHighSlot.GetCollisionFrameData().bodies.Add(
                new BodyBox { kind = 0, x = 35, y = -10, w = 10, h = 20 });
            registeredFirstHighSlot.GetCollisionFrameData().itrs.Insert(0, null);
            registeredSecondLowSlot.GetCollisionFrameData().itrs.Insert(0, null);

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
            query.ForceRoleAwareDirectForDiagnostics = true;
            CandidateRun direct = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                registeredSecondLowSlot,
                registeredFirstHighSlot);
            long directComparisons =
                query.LastRoleAwareDirectComparisonCountForDiagnostics;
            long directAllocations =
                query.MeasureWarmedRoleAwareDirectAllocationsForSelfCheck(32);
            long exactRectCacheAllocations =
                query.MeasureWarmedRoleAwareExactRectCacheAllocationsForSelfCheck(32);
            query.ForceRoleAwareDirectForDiagnostics = false;
            query.ForceRoleAwareTreeForDiagnostics = true;
            CandidateRun tree = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                registeredSecondLowSlot,
                registeredFirstHighSlot);

            AssertRunsEqual(legacy, direct);
            AssertRunsEqual(direct, tree);
            Assert.That(direct.Sequences[0].Count, Is.EqualTo(1));
            Assert.That(direct.Sequences[0][0].TargetSlot, Is.EqualTo(9));
            Assert.That(direct.Sequences[0][0].ItrIndex, Is.EqualTo(1));
            Assert.That(direct.Sequences[0][0].BodyX, Is.EqualTo(5));
            Assert.That(direct.Sequences[1].Count, Is.EqualTo(1));
            Assert.That(direct.Sequences[1][0].TargetSlot, Is.EqualTo(2));
            Assert.That(direct.Sequences[1][0].ItrIndex, Is.EqualTo(1));
            Assert.That(direct.Sequences[1][0].BodyX, Is.EqualTo(-5));
            Assert.That(directComparisons, Is.EqualTo(4));
            Assert.That(directAllocations, Is.Zero);
            Assert.That(exactRectCacheAllocations, Is.Zero);
            Assert.That(
                query.LastRoleAwareExactItrRectBuildCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                query.LastRoleAwareExactBodyRectBuildCountForDiagnostics,
                Is.EqualTo(3));
            Assert.That(
                query.LastRoleAwareExactDirectionCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                query.LastRoleAwareExactItrVisitCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                query.LastRoleAwareExactBodyOverlapCheckCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(query.LastRoleAwareDirectTickCountForDiagnostics, Is.Zero);
            Assert.That(query.LastRoleAwareTreeTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.TotalRoleAwareDirectTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.TotalRoleAwareTreeTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareBodyEntryCountForDiagnostics, Is.EqualTo(3));
            Assert.That(query.LastRoleAwareItrQueryCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_WarmedRoleAwareCollectDoesNotAllocateParticipantObjects()
        {
            const int participantCount = 128;
            const int measuredIterations = 16;
            var world = new SimulationWorld();
            LF2FrameData sharedBodyFrame = MakeFrame(
                null,
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            for (int slot = 0; slot < participantCount; slot++)
            {
                LF2Character participant = CreateCharacter(
                    $"RoleFormal_Allocation_{slot}",
                    3000 + slot,
                    sharedBodyFrame);
                Register(world, participant, slot, (slot % 2) + 1, slot * 30);
            }

            BruteForceSceneQuery query = GetQuery(world);
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            query.ForceRoleAwareDirectForDiagnostics = true;
            world.CaptureCollisionFrameSnapshotsAll();

            long allocatedBytes =
                query.MeasureWarmedRoleAwareCollectAllocationsForSelfCheck(
                    measuredIterations);

            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
            Assert.That(
                query.LastRoleAwareParticipantCountForDiagnostics,
                Is.EqualTo(participantCount));
            Assert.That(
                allocatedBytes,
                Is.LessThan(4096L),
                "The warmed full role-aware collection must not allocate one " +
                "participant object per entity; only unrelated collection overhead is allowed.");
        }

        [Test]
        public void Formal_RoleAwareParticipantFlagsMatchLegacyBuildForAllBodyPaths()
        {
            LF2FrameData noBodyFrame = MakeFrame(null, null);
            LF2FrameData validBodyFrame = MakeFrame(
                null,
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            LF2FrameData invalidBodyFrame = MakeFrame(
                null,
                new BodyBox
                {
                    kind = 0,
                    x = 20,
                    y = -10,
                    w = 0,
                    h = 20,
                });
            var world = new SimulationWorld();
            LF2Character noBody = CreateCharacter(
                "RoleFormal_FlagsNoBody",
                3200,
                noBodyFrame);
            LF2Character fast = CreateCharacter(
                "RoleFormal_FlagsFast",
                3201,
                validBodyFrame);
            LF2Character fallback = CreateCharacter(
                "RoleFormal_FlagsFallback",
                3202,
                validBodyFrame);
            LF2Character invalidBounds = CreateCharacter(
                "RoleFormal_FlagsInvalidBounds",
                3203,
                invalidBodyFrame);
            Register(world, noBody, 0, 1, 0);
            Register(world, fast, 1, 2, 0);
            Register(world, fallback, 2, 2, 999999995);
            Register(world, invalidBounds, 3, 2, 0);

            BruteForceSceneQuery query = GetQuery(world);
            query.ForceLegacyRoleBodyBuildForDiagnostics = true;
            RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                noBody,
                fast,
                fallback,
                invalidBounds);
            bool[] noBodyLegacy = ReadRoleAwareParticipantFlags(query, noBody);
            bool[] fastLegacy = ReadRoleAwareParticipantFlags(query, fast);
            bool[] fallbackLegacy = ReadRoleAwareParticipantFlags(query, fallback);
            bool[] invalidBoundsLegacy =
                ReadRoleAwareParticipantFlags(query, invalidBounds);

            query.ForceLegacyRoleBodyBuildForDiagnostics = false;
            RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                noBody,
                fast,
                fallback,
                invalidBounds);

            Assert.That(
                ReadRoleAwareParticipantFlags(query, noBody),
                Is.EqualTo(noBodyLegacy));
            Assert.That(
                ReadRoleAwareParticipantFlags(query, fast),
                Is.EqualTo(fastLegacy));
            Assert.That(
                ReadRoleAwareParticipantFlags(query, fallback),
                Is.EqualTo(fallbackLegacy));
            Assert.That(
                ReadRoleAwareParticipantFlags(query, invalidBounds),
                Is.EqualTo(invalidBoundsLegacy));
            Assert.That(noBodyLegacy, Is.EqualTo(new[] { false, false, false, false }));
            Assert.That(fastLegacy, Is.EqualTo(new[] { true, false, false, false }));
            Assert.That(fallbackLegacy, Is.EqualTo(new[] { true, false, false, false }));
            Assert.That(invalidBoundsLegacy, Is.EqualTo(new[] { true, true, false, false }));
            Assert.That(
                query.LastRoleAwareBodyTemplateFallbackCountForDiagnostics,
                Is.EqualTo(2));
        }

        [TestCase(false, false, 0)]
        [TestCase(false, true, 0)]
        [TestCase(true, false, 0)]
        [TestCase(true, true, 1)]
        public void Formal_CachedExactMatchesLegacyForCurrentAndCollisionRoleMatrix(
            bool currentHasRoles,
            bool collisionHasRoles,
            int expectedCandidateCount)
        {
            LF2FrameData attackerCurrentFrame = MakeFrame(
                currentHasRoles
                    ? new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -20,
                        y = -20,
                        w = 40,
                        h = 40,
                        zwidth = 15,
                    }
                    : null,
                null);
            LF2FrameData attackerCollisionFrame = MakeFrame(
                collisionHasRoles
                    ? new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -20,
                        y = -20,
                        w = 40,
                        h = 40,
                        zwidth = 15,
                    }
                    : null,
                null);
            LF2FrameData targetCurrentFrame = MakeFrame(
                null,
                currentHasRoles
                    ? new BodyBox
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                    }
                    : null);
            LF2FrameData targetCollisionFrame = MakeFrame(
                null,
                collisionHasRoles
                    ? new BodyBox
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                    }
                    : null);
            attackerCurrentFrame.frameId = 0;
            attackerCollisionFrame.frameId = 1;
            targetCurrentFrame.frameId = 0;
            targetCollisionFrame.frameId = 1;

            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacterWithFrames(
                "RoleFormal_CurrentCollisionAttacker",
                3300,
                new List<LF2FrameData>
                {
                    attackerCurrentFrame,
                    attackerCollisionFrame,
                });
            LF2Character target = CreateCharacterWithFrames(
                "RoleFormal_CurrentCollisionTarget",
                3301,
                new List<LF2FrameData>
                {
                    targetCurrentFrame,
                    targetCollisionFrame,
                });
            Register(world, attacker, 0, 1, 0);
            Register(world, target, 1, 2, 0);
            BruteForceSceneQuery query = GetQuery(world);
            query.ForceRoleAwareDirectForDiagnostics = true;
            Action overrideCollisionFrames = () =>
            {
                OverrideCollisionFrame(attacker, 1);
                OverrideCollisionFrame(target, 1);
            };

            query.ForceLegacyRoleAwareExactPrefilterForDiagnostics = true;
            CandidateRun legacyExact = RunCollectionWithCollisionSnapshotOverride(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                overrideCollisionFrames,
                attacker);
            query.ForceLegacyRoleAwareExactPrefilterForDiagnostics = false;
            CandidateRun cachedExact = RunCollectionWithCollisionSnapshotOverride(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                overrideCollisionFrames,
                attacker);

            AssertRunsEqual(legacyExact, cachedExact);
            Assert.That(
                cachedExact.Counts,
                Is.EqualTo(new[] { expectedCandidateCount }));
        }

        [Test]
        public void Formal_CachedExactMatchesLegacyForKind5MixedAndFullHeightClamp()
        {
            LF2FrameData kind5OnlyFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 5,
                    vrest = 1,
                    x = -20,
                    y = -20,
                    w = 40,
                    h = 40,
                    zwidth = 15,
                },
                null);
            LF2FrameData mixedFrame = MakeFrame(
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
                null);
            mixedFrame.itrs.Add(new InteractionArea
            {
                kind = 5,
                vrest = 1,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
                zwidth = 15,
            });
            LF2FrameData fullHeightFrame = MakeFrame(
                null,
                new BodyBox
                {
                    kind = 0,
                    x = -200,
                    y = int.MinValue,
                    w = 900,
                    h = 999,
                });

            var world = new SimulationWorld();
            LF2Character holder = CreateCharacter(
                "RoleFormal_Kind5Holder",
                3310,
                MakeFrame(null, null));
            LF2Character kind5Only = CreateCharacter(
                "RoleFormal_Kind5Only",
                3311,
                kind5OnlyFrame);
            LF2Character target = CreateCharacter(
                "RoleFormal_FullHeightTarget",
                3312,
                fullHeightFrame);
            LF2Character mixed = CreateCharacter(
                "RoleFormal_Kind5Mixed",
                3313,
                mixedFrame);
            Register(world, holder, 0, 1, 999999900);
            Register(world, kind5Only, 1, 1, 999999900);
            Register(world, target, 2, 2, 999999900);
            Register(world, mixed, 3, 1, 999999900);
            target.PS.dir = "left";
            target.Runtime.Dir = "left";

            BruteForceSceneQuery query = GetQuery(world);
            query.ForceRoleAwareDirectForDiagnostics = true;
            query.ForceLegacyRoleAwareExactPrefilterForDiagnostics = true;
            CandidateRun legacyExact = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                kind5Only,
                mixed);
            query.ForceLegacyRoleAwareExactPrefilterForDiagnostics = false;
            CandidateRun cachedExact = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                kind5Only,
                mixed);

            AssertRunsEqual(legacyExact, cachedExact);
            Assert.That(cachedExact.Counts, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(cachedExact.CollectionAborted, Is.False);
            Assert.That(
                query.LastRoleAwareExactItrRectBuildCountForDiagnostics,
                Is.EqualTo(3));
            Assert.That(
                query.LastRoleAwareExactBodyRectBuildCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareExactDirectionCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                query.LastRoleAwareExactItrVisitCountForDiagnostics,
                Is.EqualTo(3));
            Assert.That(
                query.LastRoleAwareExactBodyOverlapCheckCountForDiagnostics,
                Is.EqualTo(3));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void Formal_ForcedDirectAndTreeMatchForZeroOneAndManyItrs(
            int itrCount)
        {
            LF2FrameData attackerFrame = MakeFrame(null, null);
            for (int itrIndex = 0; itrIndex < itrCount; itrIndex++)
            {
                attackerFrame.itrs.Add(new InteractionArea
                {
                    kind = itrIndex == 1 ? 1 : 0,
                    vrest = 1,
                    x = -20 + itrIndex,
                    y = -10,
                    w = 40,
                    h = 20,
                    zwidth = 15,
                });
            }

            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                $"RoleDirect_Itrs_{itrCount}",
                1400 + itrCount,
                attackerFrame);
            LF2Character target = CreateCharacter(
                $"RoleDirect_Target_{itrCount}",
                1410 + itrCount,
                MakeFrame(
                    null,
                    new BodyBox
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                    }));
            Register(world, target, 7, 2, 0);
            Register(world, attacker, 3, 1, 0);

            BruteForceSceneQuery query = GetQuery(world);
            LF2Entity[] trackedAttackers =
                itrCount == 0 ? Array.Empty<LF2Entity>() : new LF2Entity[] { attacker };
            query.ForceRoleAwareDirectForDiagnostics = true;
            CandidateRun direct = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                trackedAttackers);
            long directComparisons =
                query.LastRoleAwareDirectComparisonCountForDiagnostics;
            query.ForceRoleAwareDirectForDiagnostics = false;
            query.ForceRoleAwareTreeForDiagnostics = true;
            CandidateRun tree = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                trackedAttackers);

            AssertRunsEqual(direct, tree);
            Assert.That(direct.ItrQueryCount, Is.EqualTo(itrCount));
            Assert.That(direct.BodyEntryCount, Is.EqualTo(1));
            Assert.That(directComparisons, Is.EqualTo(itrCount));
            Assert.That(query.LastRoleAwareTreeTickCountForDiagnostics, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Formal_EpochGuardedExactLoopMatchesLegacyPerPairValidationWithRng(
            bool forceTree)
        {
            CreateExactLoopTieFixture(
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);
            query.ForceRoleAwareDirectForDiagnostics = !forceTree;
            query.ForceRoleAwareTreeForDiagnostics = forceTree;

            CandidateRun legacyValidation;
            CandidateRun epochGuarded;
            try
            {
                query.ForceLegacyPerPairValidationForDiagnostics = true;
                legacyValidation = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceRoleAware,
                    CollectionSeed,
                    attacker);
                query.ForceLegacyPerPairValidationForDiagnostics = false;
                epochGuarded = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceRoleAware,
                    CollectionSeed,
                    attacker);
            }
            finally
            {
                query.ForceLegacyPerPairValidationForDiagnostics = false;
                query.ForceRoleAwareDirectForDiagnostics = false;
                query.ForceRoleAwareTreeForDiagnostics = false;
            }

            AssertRunsEqual(legacyValidation, epochGuarded);
            Assert.That(epochGuarded.RngCalls, Is.GreaterThan(0));
            Assert.That(query.ForceLegacyPerPairValidationForDiagnostics, Is.False);
        }

        [TestCase(false, 0)]
        [TestCase(true, 0)]
        [TestCase(false, 1)]
        [TestCase(true, 1)]
        [TestCase(false, 2)]
        [TestCase(true, 2)]
        [TestCase(false, 3)]
        [TestCase(true, 3)]
        public void Formal_EpochGuardedDirectionalCallsMatchLegacyValidation(
            bool forceTree,
            int scenario)
        {
            CreateExactLoopDirectionalFixture(
                scenario,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character first,
                out LF2Character second);
            query.ForceRoleAwareDirectForDiagnostics = !forceTree;
            query.ForceRoleAwareTreeForDiagnostics = forceTree;

            CandidateRun legacyValidation;
            CandidateRun epochGuarded;
            try
            {
                query.ForceLegacyPerPairValidationForDiagnostics = true;
                legacyValidation = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceRoleAware,
                    CollectionSeed,
                    first,
                    second);
                query.ForceLegacyPerPairValidationForDiagnostics = false;
                epochGuarded = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceRoleAware,
                    CollectionSeed,
                    first,
                    second);
            }
            finally
            {
                query.ForceLegacyPerPairValidationForDiagnostics = false;
                query.ForceRoleAwareDirectForDiagnostics = false;
                query.ForceRoleAwareTreeForDiagnostics = false;
            }

            AssertRunsEqual(legacyValidation, epochGuarded);
            Assert.That(query.ForceLegacyPerPairValidationForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_OccupancyEpochMutationAbortsAndRestoresBruteRngAndCandidates()
        {
            CreateExactLoopTieFixture(
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                CollectionSeed,
                attacker);
            LF2Character lateInert = CreateCharacter(
                "RoleExact_EpochLateInert",
                1899,
                MakeFrame(null, null));
            bool mutated = false;
            query.ForceRoleAwareDirectForDiagnostics = true;
            query.AfterRoleAwareExactPairForSelfCheck = _ =>
            {
                if (mutated)
                    return;

                mutated = true;
                Register(world, lateInert, 3, 4, 10000);
            };

            CandidateRun recovered;
            try
            {
                recovered = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceRoleAware,
                    CollectionSeed,
                    attacker);
            }
            finally
            {
                query.AfterRoleAwareExactPairForSelfCheck = null;
                query.ForceRoleAwareDirectForDiagnostics = false;
            }

            Assert.That(mutated, Is.True);
            AssertRunsEqual(brute, recovered);
            Assert.That(recovered.CollectionAborted, Is.True);
            Assert.That(recovered.RngCalls, Is.GreaterThan(0));
            Assert.That(lateInert.Runtime.SlotIndex, Is.EqualTo(3));
            Assert.That(query.AfterRoleAwareExactPairForSelfCheck, Is.Null);
        }

        [TestCase(1024, 256, true)]
        [TestCase(1417, 185, false)]
        [Timeout(30000)]
        public void Formal_DirectCostThresholdIsInclusive(
            int itrEntryCount,
            int bodyUnionEntryCount,
            bool expectDirect)
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                bodyUnionEntryCount + 8,
                CollisionBroadphaseBackend.LooseQuadtree);
            LF2FrameData attackerFrame = MakeFrame(null, null);
            for (int itrIndex = 0; itrIndex < itrEntryCount; itrIndex++)
            {
                attackerFrame.itrs.Add(new InteractionArea
                {
                    kind = 0,
                    vrest = 1,
                    x = -20,
                    y = -10,
                    w = 40,
                    h = 20,
                    zwidth = 15,
                });
            }
            LF2Character attacker = CreateCharacter(
                $"RoleDirect_ThresholdAttacker_{itrEntryCount}_{bodyUnionEntryCount}",
                1500,
                attackerFrame);
            LF2FrameData targetFrame = MakeFrame(
                null,
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            for (int bodyIndex = 0;
                 bodyIndex < bodyUnionEntryCount;
                 bodyIndex++)
            {
                LF2Character target = CreateCharacter(
                    $"RoleDirect_ThresholdTarget_{bodyIndex}",
                    1600 + bodyIndex,
                    targetFrame);
                Register(world, target, bodyIndex, 2, bodyIndex * 1000);
            }
            Register(world, attacker, bodyUnionEntryCount, 1, 0);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun run = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);

            long expectedCost =
                (long)itrEntryCount * bodyUnionEntryCount;
            Assert.That(run.BodyEntryCount, Is.EqualTo(bodyUnionEntryCount));
            Assert.That(run.ItrQueryCount, Is.EqualTo(itrEntryCount));
            Assert.That(
                expectedCost,
                Is.EqualTo(expectDirect ? 262144L : 262145L));
            Assert.That(
                query.LastRoleAwareDirectTickCountForDiagnostics,
                Is.EqualTo(expectDirect ? 1 : 0));
            Assert.That(
                query.LastRoleAwareTreeTickCountForDiagnostics,
                Is.EqualTo(expectDirect ? 0 : 1));
            Assert.That(
                query.LastRoleAwareDirectCostForDiagnostics,
                Is.EqualTo(expectedCost));
            Assert.That(
                query.LastRoleAwareSweepDirectTickCountForDiagnostics,
                Is.EqualTo(expectDirect ? 1 : 0));
            Assert.That(run.CollectionAborted, Is.False);
        }

        [Test]
        public void Formal_SharedFrameBodyTemplateMatchesForcedLegacyAfterMovementDirectionAndType3Z()
        {
            LF2FrameData sharedFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 1,
                    vrest = 1,
                    x = -50,
                    y = -20,
                    w = 100,
                    h = 40,
                    zwidth = 40,
                },
                new BodyBox
                {
                    kind = 0,
                    x = -20,
                    y = -20,
                    w = 10,
                    h = 40,
                });
            sharedFrame.centerx = 5;
            sharedFrame.bodies.Add(new BodyBox
            {
                kind = 0,
                x = 10,
                y = -20,
                w = 30,
                h = 40,
            });

            var world = new SimulationWorld();
            LF2Character right = CreateCharacter(
                "RoleTemplate_Right",
                1300,
                sharedFrame);
            LF2Character left = CreateCharacter(
                "RoleTemplate_Left",
                1301,
                sharedFrame);
            LF2Character type3 = CreateCharacter(
                "RoleTemplate_Type3",
                1302,
                sharedFrame,
                true);
            Register(world, right, 0, 1, 90);
            Register(world, left, 1, 2, 100);
            Register(world, type3, 2, 3, 110);
            right.Runtime.SetPosition(90, 0, 200);
            left.Runtime.SetPosition(100, 0, 200);
            type3.Runtime.SetPosition(110, 0, 206);
            right.Runtime.SyncIntegerPosition();
            left.Runtime.SyncIntegerPosition();
            type3.Runtime.SyncIntegerPosition();
            left.PS.dir = "left";
            left.Runtime.Dir = "left";
            type3.Runtime.Type3VisualZOffset = 6;

            BruteForceSceneQuery query = GetQuery(world);
            query.ForceLegacyRoleBodyBuildForDiagnostics = true;
            CandidateRun forcedLegacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                right,
                left,
                type3);
            query.ForceLegacyRoleBodyBuildForDiagnostics = false;
            CandidateRun templated = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                right,
                left,
                type3);
            query.ForceRoleAwareTreeForDiagnostics = true;
            CandidateRun templatedTree = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                right,
                left,
                type3);
            query.ForceRoleAwareTreeForDiagnostics = false;

            AssertRunsEqual(forcedLegacy, templated);
            AssertRunsEqual(templated, templatedTree);
            Assert.That(templated.BodyEntryCount, Is.EqualTo(6));
            Assert.That(
                query.LastRoleAwareBodyTemplateBuildCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareBodyTemplateHitCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                query.LastRoleAwareBodyTemplateFallbackCountForDiagnostics,
                Is.Zero);
            Assert.That(
                query.TryGetLastRoleAwareBodyBoundsForSelfCheck(
                    right,
                    out SpatialAabbXZ rightBounds),
                Is.True);
            Assert.That(rightBounds, Is.EqualTo(new SpatialAabbXZ(65, 200, 125, 201)));
            Assert.That(
                query.TryGetLastRoleAwareBodyBoundsForSelfCheck(
                    left,
                    out SpatialAabbXZ leftBounds),
                Is.True);
            Assert.That(leftBounds, Is.EqualTo(new SpatialAabbXZ(65, 200, 125, 201)));
            Assert.That(
                query.TryGetLastRoleAwareBodyBoundsForSelfCheck(
                    type3,
                    out SpatialAabbXZ type3Bounds),
                Is.True);
            Assert.That(type3Bounds, Is.EqualTo(new SpatialAabbXZ(85, 200, 145, 201)));

            right.Runtime.SetPosition(120, 0, 210);
            left.Runtime.SetPosition(150, 0, 210);
            type3.Runtime.SetPosition(135, 0, 220);
            right.Runtime.SyncIntegerPosition();
            left.Runtime.SyncIntegerPosition();
            type3.Runtime.SyncIntegerPosition();
            right.PS.dir = "left";
            right.Runtime.Dir = "left";
            left.PS.dir = "right";
            left.Runtime.Dir = "right";
            type3.Runtime.Type3VisualZOffset = 10;

            query.ForceLegacyRoleBodyBuildForDiagnostics = true;
            CandidateRun movedForcedLegacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                right,
                left,
                type3);
            query.ForceLegacyRoleBodyBuildForDiagnostics = false;
            CandidateRun movedTemplated = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                right,
                left,
                type3);

            AssertRunsEqual(movedForcedLegacy, movedTemplated);
            Assert.That(movedTemplated.BodyEntryCount, Is.EqualTo(6));
            Assert.That(
                query.TryGetLastRoleAwareBodyBoundsForSelfCheck(
                    right,
                    out rightBounds),
                Is.True);
            Assert.That(rightBounds, Is.EqualTo(new SpatialAabbXZ(85, 210, 145, 211)));
            Assert.That(
                query.TryGetLastRoleAwareBodyBoundsForSelfCheck(
                    left,
                    out leftBounds),
                Is.True);
            Assert.That(leftBounds, Is.EqualTo(new SpatialAabbXZ(125, 210, 185, 211)));
            Assert.That(
                query.TryGetLastRoleAwareBodyBoundsForSelfCheck(
                    type3,
                    out type3Bounds),
                Is.True);
            Assert.That(type3Bounds, Is.EqualTo(new SpatialAabbXZ(110, 210, 170, 211)));
        }

        [Test]
        public void Formal_DegenerateTemplateAndClampTranslationFallbackLocallyWithParity()
        {
            LF2FrameData degenerateFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 0,
                    x = -100,
                    y = -20,
                    w = 200,
                    h = 40,
                    zwidth = 40,
                },
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -20,
                    w = 20,
                    h = 40,
                });
            degenerateFrame.bodies.Add(new BodyBox
            {
                kind = 0,
                x = 20,
                y = -20,
                w = 0,
                h = 40,
            });

            var degenerateWorld = new SimulationWorld();
            LF2Character first = CreateCharacter(
                "RoleTemplate_DegenerateFirst",
                1310,
                degenerateFrame);
            LF2Character second = CreateCharacter(
                "RoleTemplate_DegenerateSecond",
                1311,
                degenerateFrame);
            Register(degenerateWorld, first, 0, 1, 0);
            Register(degenerateWorld, second, 1, 2, 5);
            BruteForceSceneQuery degenerateQuery = GetQuery(degenerateWorld);

            degenerateQuery.ForceLegacyRoleBodyBuildForDiagnostics = true;
            CandidateRun degenerateLegacy = RunCollection(
                degenerateWorld,
                degenerateQuery,
                CollisionFormalCollectorMode.ForceRoleAware,
                first,
                second);
            degenerateQuery.ForceLegacyRoleBodyBuildForDiagnostics = false;
            CandidateRun degenerateTemplated = RunCollection(
                degenerateWorld,
                degenerateQuery,
                CollisionFormalCollectorMode.ForceRoleAware,
                first,
                second);

            AssertRunsEqual(degenerateLegacy, degenerateTemplated);
            Assert.That(degenerateTemplated.BodyEntryCount, Is.EqualTo(2));
            Assert.That(
                degenerateQuery.LastRoleAwareBodyTemplateBuildCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                degenerateQuery.LastRoleAwareBodyTemplateHitCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                degenerateQuery.LastRoleAwareBodyTemplateFallbackCountForDiagnostics,
                Is.EqualTo(2));

            LF2FrameData clampFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 0,
                    x = -20,
                    y = -20,
                    w = 40,
                    h = 40,
                    zwidth = 40,
                },
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -20,
                    w = 20,
                    h = 40,
                });
            var clampWorld = new SimulationWorld();
            LF2Character safe = CreateCharacter(
                "RoleTemplate_ClampSafe",
                1320,
                clampFrame);
            LF2Character edge = CreateCharacter(
                "RoleTemplate_ClampEdge",
                1321,
                clampFrame);
            Register(clampWorld, safe, 0, 1, 0);
            Register(clampWorld, edge, 1, 2, 999999995);
            BruteForceSceneQuery clampQuery = GetQuery(clampWorld);

            clampQuery.ForceLegacyRoleBodyBuildForDiagnostics = true;
            CandidateRun clampLegacy = RunCollection(
                clampWorld,
                clampQuery,
                CollisionFormalCollectorMode.ForceRoleAware,
                safe,
                edge);
            clampQuery.ForceLegacyRoleBodyBuildForDiagnostics = false;
            CandidateRun clampTemplated = RunCollection(
                clampWorld,
                clampQuery,
                CollisionFormalCollectorMode.ForceRoleAware,
                safe,
                edge);

            AssertRunsEqual(clampLegacy, clampTemplated);
            Assert.That(clampTemplated.BodyEntryCount, Is.EqualTo(2));
            Assert.That(
                clampQuery.LastRoleAwareBodyTemplateBuildCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                clampQuery.LastRoleAwareBodyTemplateHitCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                clampQuery.LastRoleAwareBodyTemplateFallbackCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void CandidateStore_CapOrderFieldsEndAndZeroAllocation()
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
            Assert.That(query.CollisionCandidateStoreShadowDiagnosticsEnabled, Is.False);
            Assert.That(query.CollisionCandidateStoreRuntimeCapacityForDiagnostics, Is.Zero,
                "the default-disabled shadow must not allocate its fixed slab");
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
            long directComparisons =
                query.LastRoleAwareDirectComparisonCountForDiagnostics;
            query.ForceRoleAwareTreeForDiagnostics = true;
            CandidateRun tree = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);

            AssertRunsEqual(brute, role);
            AssertRunsEqual(role, tree);
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
            Assert.That(directComparisons, Is.EqualTo(42));
            Assert.That(query.LastFormalFallbackParticipantCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);

            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.Rng.Seed(CollectionSeed);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange legacyRange),
                Is.True);
            Assert.That(legacyRange.Count, Is.EqualTo(20));
            Assert.That(legacyRange.TryGet(0, out _), Is.True);
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.RangeReadCount,
                Is.Zero,
                "authority-off range reads must not touch the candidate store");
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.EntryReadCount,
                Is.Zero,
                "authority-off entry reads must not touch the candidate store");
            world.EndCollisionCandidateConsumption();

            query.CollisionCandidateStoreShadowDiagnosticsEnabled = true;
            query.CollisionCandidateStoreAuthorityEnabled = true;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.Rng.Seed(CollectionSeed);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                query.CollisionCandidateStoreRuntimeCapacityForDiagnostics,
                Is.EqualTo(world.RuntimeSlotCapacityForDiagnostics));
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    attacker.Runtime.SlotIndex,
                    attacker,
                    out RuntimeEntityHandle attackerHandle),
                Is.True);
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    attackerHandle,
                    out int storeCount),
                Is.True);
            Assert.That(storeCount, Is.EqualTo(20));
            Assert.That(
                query.CollisionCandidateStoreAuthorityAppliedForCurrentTickForDiagnostics,
                Is.True);
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange authorityRange),
                Is.True);
            Assert.That(authorityRange.Count, Is.EqualTo(20));
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange secondConsumerRange),
                Is.True);
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange thirdConsumerRange),
                Is.True);
            Assert.That(secondConsumerRange.Count, Is.EqualTo(20));
            Assert.That(thirdConsumerRange.Count, Is.EqualTo(20));
            Assert.That(secondConsumerRange.TryGet(19, out SceneQueryHit secondTail), Is.True);
            Assert.That(thirdConsumerRange.TryGet(19, out SceneQueryHit thirdTail), Is.True);
            Assert.That(secondTail.TargetSlot, Is.EqualTo(thirdTail.TargetSlot),
                "all formal consumers must see the same complete step6 window");
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    itrOnlyFarAway,
                    out CollisionCandidateRange emptyAuthorityRange),
                Is.True);
            Assert.That(emptyAuthorityRange.Count, Is.Zero,
                "an initialized attacker row with no candidates must remain empty");
            for (int candidateIndex = 0; candidateIndex < storeCount; candidateIndex++)
            {
                Assert.That(
                    query.TryGetCollisionCandidateStoreEntryForSelfCheck(
                        attackerHandle,
                        candidateIndex,
                        out CollisionCandidateStoreEntry entry),
                    Is.True);
                SceneQueryHit expected = tree.Sequences[0][candidateIndex];
                Assert.That(entry.TargetSlot, Is.EqualTo(expected.TargetSlot));
                Assert.That(
                    entry.TargetHandle,
                    Is.EqualTo(tree.TargetHandles[0][candidateIndex]));
                Assert.That(entry.BodyX, Is.EqualTo(expected.BodyX));
                Assert.That(entry.ItrIndex, Is.EqualTo(expected.ItrIndex));
                Assert.That(entry.RuntimeItr, Is.SameAs(expected.RuntimeItr));
                Assert.That(
                    entry.ZeroAttackerHpOnConsume,
                    Is.EqualTo(expected.ZeroAttackerHpOnConsume));
                Assert.That(
                    entry.ReleaseHeavyHeldTargetOnConsume,
                    Is.EqualTo(expected.ReleaseHeavyHeldTargetOnConsume));
                Assert.That(
                    authorityRange.TryGet(candidateIndex, out SceneQueryHit authorityHit),
                    Is.True);
                Assert.That(authorityHit.TargetSlot, Is.EqualTo(expected.TargetSlot));
                Assert.That(authorityHit.Target, Is.SameAs(expected.Target));
                Assert.That(authorityHit.BodyX, Is.EqualTo(expected.BodyX));
                Assert.That(authorityHit.ItrIndex, Is.EqualTo(expected.ItrIndex));
                Assert.That(authorityHit.RuntimeItr, Is.SameAs(expected.RuntimeItr));
                Assert.That(
                    authorityHit.ZeroAttackerHpOnConsume,
                    Is.EqualTo(expected.ZeroAttackerHpOnConsume));
                Assert.That(
                    authorityHit.ReleaseHeavyHeldTargetOnConsume,
                    Is.EqualTo(expected.ReleaseHeavyHeldTargetOnConsume));
            }
            Assert.That(
                query.MeasureWarmedCollisionCandidateStoreShadowAllocationsForSelfCheck(32),
                Is.Zero);
            Assert.That(
                query.MeasureWarmedCollisionCandidateStoreAuthorityAllocationsForSelfCheck(
                    attacker,
                    32),
                Is.Zero);
            Assert.That(query.CollisionCandidateStoreShadowDiagnostics.MismatchCount, Is.Zero);
            Assert.That(query.CollisionCandidateStoreShadowDiagnostics.InvalidCount, Is.Zero);
            Assert.That(
                query.CollisionCandidateStoreShadowDiagnostics.FirstMismatchReason,
                Is.EqualTo(CollisionCandidateStoreMismatchReason.None));
            world.EndCollisionCandidateConsumption();
            Assert.That(authorityRange.Count, Is.Zero);
            Assert.That(authorityRange.TryGet(0, out _), Is.False);
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    attackerHandle,
                    out _),
                Is.False);
        }

        [Test]
        public void CandidateStoreAuthority_StrictListRuntimeStoreCountLocksWholeTickToLegacy()
        {
            var world = new SimulationWorld();
            InteractionArea itr = new InteractionArea
            {
                kind = 0,
                vrest = 1,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
                zwidth = 15,
            };
            LF2Character attacker = CreateCharacter(
                "CandidateStoreAuthority_StrictCountAttacker",
                1,
                MakeFrame(itr, null));
            LF2Character target = CreateCharacter(
                "CandidateStoreAuthority_StrictCountTarget",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            Register(world, attacker, 0, 1, 0);
            Register(world, target, 1, 2, 0);

            BruteForceSceneQuery query = GetQuery(world);
            query.CollisionCandidateStoreShadowDiagnosticsEnabled = true;
            query.CollisionCandidateStoreAuthorityEnabled = true;
            bool appendedLegacyTail = false;
            query.BeforeCollisionCandidateStoreFinalCompareForSelfCheck = () =>
            {
                var extra = new SceneQueryHit(target, 1234, 0, itr);
                appendedLegacyTail =
                    query.TryAppendCollisionCandidateLegacyOracleForSelfCheck(
                        attacker,
                        in extra);
            };

            try
            {
                world.CaptureCollisionFrameSnapshotsAll();
                world.CollectCollisionCandidatesAll();
            }
            finally
            {
                query.BeforeCollisionCandidateStoreFinalCompareForSelfCheck = null;
            }

            Assert.That(appendedLegacyTail, Is.True);
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            Assert.That(
                query.CollisionCandidateStoreShadowDiagnostics.FirstMismatchReason,
                Is.EqualTo(CollisionCandidateStoreMismatchReason.CandidateCountMismatch));
            Assert.That(
                query.CollisionCandidateStoreAuthorityAppliedForCurrentTickForDiagnostics,
                Is.False);
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.LegacyFallbackTickCount,
                Is.EqualTo(1));
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.FirstFailureReason,
                Is.EqualTo(CollisionCandidateStoreAuthorityFailureReason.StoreNotComplete));
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange fallbackRange),
                Is.True);
            Assert.That(fallbackRange.Count, Is.EqualTo(2),
                "strict mismatch must choose the whole legacy oracle, not a store prefix");
            Assert.That(fallbackRange.TryGet(1, out SceneQueryHit legacyTail), Is.True);
            Assert.That(legacyTail.BodyX, Is.EqualTo(1234));
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.RangeReadCount,
                Is.Zero,
                "a tick locked to legacy must not perform authority reads");
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.EntryReadCount,
                Is.Zero);

            world.EndCollisionCandidateConsumption();
            Assert.That(fallbackRange.Count, Is.Zero);
            Assert.That(fallbackRange.TryGet(0, out _), Is.False);
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
            query.ForceRoleAwareDirectForDiagnostics = true;
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                degenerateItr,
                validItr);

            AssertRunsEqual(legacy, role);
            Assert.That(query.LastRoleAwareDirectTickCountForDiagnostics, Is.Zero);
            Assert.That(query.LastRoleAwareTreeTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareDirectComparisonCountForDiagnostics, Is.Zero);
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
            query.ForceRoleAwareDirectForDiagnostics = true;
            CandidateRun direct = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                unchecked((uint)seed),
                attacker);
            query.ForceRoleAwareDirectForDiagnostics = false;
            query.ForceRoleAwareTreeForDiagnostics = true;
            CandidateRun tree = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                unchecked((uint)seed),
                attacker);

            var expectedRng = new DeterministicRng(unchecked((uint)seed));
            bool tieReplaced = expectedRng.NextInt(0, 2) == 0;
            AssertRunsEqual(legacy, direct);
            AssertRunsEqual(direct, tree);
            Assert.That(direct.RngCalls, Is.EqualTo(1));
            Assert.That(direct.RngState, Is.EqualTo(expectedRng.State));
            Assert.That(
                direct.Sequences[0].Count,
                Is.EqualTo(tieReplaced ? 2 : 1));
            Assert.That(direct.Sequences[0][0].ItrIndex, Is.EqualTo(0));
            if (tieReplaced)
                Assert.That(direct.Sequences[0][1].ItrIndex, Is.EqualTo(1));
            Assert.That(direct.CollectionAborted, Is.False);
        }

        [Test]
        public void Formal_PreLoopExactCacheInvalidationFallsBackAndRestoresRng()
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
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            targetFrame.state = LF2States.Injured2;

            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_PreLoopInvalidationAttacker",
                3320,
                attackerFrame);
            LF2Character target = CreateCharacter(
                "RoleFormal_PreLoopInvalidationTarget",
                3321,
                targetFrame);
            Register(world, target, 0, 2, 10);
            Register(world, attacker, 5, 1, 0);
            attacker.Runtime.KeyRight = 1;

            BruteForceSceneQuery query = GetQuery(world);
            query.ForceRoleAwareDirectForDiagnostics = true;
            query.BeforeRoleAwareFormalInputValidationForSelfCheck = () =>
            {
                target.Runtime.SetPosition(11, 0, 0);
                target.Runtime.SyncIntegerPosition();
            };
            CandidateRun fallback = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                1u,
                attacker);
            int cheapValidationCount =
                query.LastRoleAwareCheapInputValidationCountForDiagnostics;
            int fullValidationCount =
                query.LastRoleAwareFullInputValidationCountForDiagnostics;
            query.BeforeRoleAwareFormalInputValidationForSelfCheck = null;
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                1u,
                attacker);

            AssertRunsEqual(brute, fallback);
            Assert.That(fallback.CollectionAborted, Is.True);
            Assert.That(fallback.RngCalls, Is.EqualTo(1));
            Assert.That(fallback.RngState, Is.EqualTo(brute.RngState));
            Assert.That(cheapValidationCount, Is.EqualTo(1));
            Assert.That(fullValidationCount, Is.EqualTo(1));
        }

        [Test]
        public void Formal_InputValidationRoutesPreserveCandidatesRngAndWarmedAllocations()
        {
            CreateExactLoopTieFixture(
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);
            query.ForceRoleAwareDirectForDiagnostics = true;

            CandidateRun cheap = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                attacker);
            Assert.That(
                query.LastRoleAwareCheapInputValidationCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareFullInputValidationCountForDiagnostics,
                Is.Zero);

            query.ForceFullRoleAwareFormalInputValidationForDiagnostics = true;
            CandidateRun forcedFull = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                attacker);
            Assert.That(
                query.LastRoleAwareCheapInputValidationCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareFullInputValidationCountForDiagnostics,
                Is.EqualTo(1));

            query.ForceFullRoleAwareFormalInputValidationForDiagnostics = false;
            int hookCalls = 0;
            query.BeforeRoleAwareFormalInputValidationForSelfCheck = () => hookCalls++;
            CandidateRun hooked;
            try
            {
                hooked = RunCollection(
                    world,
                    query,
                    CollisionFormalCollectorMode.ForceRoleAware,
                    CollectionSeed,
                    attacker);
            }
            finally
            {
                query.BeforeRoleAwareFormalInputValidationForSelfCheck = null;
            }

            Assert.That(hookCalls, Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareCheapInputValidationCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareFullInputValidationCountForDiagnostics,
                Is.EqualTo(1));
            AssertRunsEqual(cheap, forcedFull);
            AssertRunsEqual(cheap, hooked);
            Assert.That(cheap.CollectionAborted, Is.False);

            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.CaptureCollisionFrameSnapshotsAll();
            long allocatedBytes =
                query.MeasureWarmedRoleAwareCollectAllocationsForSelfCheck(16);
            Assert.That(
                query.LastRoleAwareCheapInputValidationCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareFullInputValidationCountForDiagnostics,
                Is.Zero);
            Assert.That(
                allocatedBytes,
                Is.LessThan(65536L),
                "Default cheap validation must not add warmed per-tick allocation.");
        }

        [Test]
        public void Formal_NoFrameInertAtNonZeroLeftPositionDoesNotAbortRoleAware()
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
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            targetFrame.state = LF2States.Injured2;

            var world = new SimulationWorld();
            LF2Character target = CreateCharacter(
                "RoleFormal_InertBoundaryTarget",
                3330,
                targetFrame);
            LF2Character attacker = CreateCharacter(
                "RoleFormal_InertBoundaryAttacker",
                3331,
                attackerFrame);
            LF2Character inert = CreateCharacter(
                "RoleFormal_InertBoundaryNoFrame",
                3332,
                MakeFrame(null, null));
            Register(world, target, 0, 2, 10);
            Register(world, attacker, 1, 1, 0);
            Register(world, inert, 2, 3, 123);
            attacker.Runtime.KeyRight = 1;
            inert.Frame.D = null;
            inert.Frame.Prev2D = null;
            inert.Runtime.SetPosition(123, 0, 205);
            inert.Runtime.SyncIntegerPosition();
            inert.PS.dir = "left";
            inert.Runtime.Dir = "left";

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                1u,
                attacker);
            query.ForceRoleAwareDirectForDiagnostics = true;
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                1u,
                attacker);

            AssertRunsEqual(brute, role);
            Assert.That(role.CollectionAborted, Is.False);
            Assert.That(role.RngCalls, Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareParticipantCountForDiagnostics,
                Is.EqualTo(3));
            Assert.That(
                query.LastRoleAwareInertParticipantCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void Formal_LazyExactCacheBuildsOnlyRolesRequiredByActualPairs()
        {
            LF2FrameData pairedFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 0,
                    vrest = 1,
                    x = -20,
                    y = -10,
                    w = 40,
                    h = 20,
                    zwidth = 15,
                },
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            var world = new SimulationWorld();
            LF2Character first = CreateCharacter(
                "RoleFormal_LazyFirst",
                3340,
                pairedFrame);
            LF2Character second = CreateCharacter(
                "RoleFormal_LazySecond",
                3341,
                pairedFrame);
            LF2Character noPair = CreateCharacter(
                "RoleFormal_LazyNoPair",
                3342,
                pairedFrame);
            LF2Character inert = CreateCharacter(
                "RoleFormal_LazyInert",
                3343,
                MakeFrame(null, null));
            Register(world, first, 0, 1, 0);
            Register(world, second, 1, 2, 5);
            Register(world, noPair, 2, 3, 1000);
            Register(world, inert, 3, 4, 2000);

            BruteForceSceneQuery query = GetQuery(world);
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            query.ForceRoleAwareDirectForDiagnostics = true;
            world.CaptureCollisionFrameSnapshotsAll();
            long allocatedBytes =
                query.MeasureWarmedRoleAwareCollectAllocationsForSelfCheck(16);

            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(1));
            AssertExactCacheCounts(
                query,
                first,
                1,
                1,
                1,
                1,
                true,
                true);
            AssertExactCacheCounts(
                query,
                second,
                1,
                1,
                1,
                1,
                true,
                true);
            AssertExactCacheCounts(
                query,
                noPair,
                0,
                0,
                0,
                0,
                false,
                false);
            AssertExactCacheCounts(
                query,
                inert,
                0,
                0,
                0,
                0,
                false,
                false);
            Assert.That(
                allocatedBytes,
                Is.LessThan(65536L),
                "Warmed collection may retain existing candidate-list overhead, " +
                "but lazy exact-role tracking must not allocate per participant/pair.");
        }

        [TestCase(15, 0)]
        [TestCase(-15, 0)]
        [TestCase(14, 1)]
        [TestCase(-14, 1)]
        public void Formal_LazyExactCachePreservesStrictZBoundaryAgainstLegacy(
            int targetZ,
            int expectedCandidateCount)
        {
            LF2FrameData attackerFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 1,
                    vrest = 1,
                    x = -20,
                    y = -10,
                    w = 40,
                    h = 20,
                    zwidth = 15,
                },
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            LF2FrameData targetFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 0,
                    vrest = 1,
                    x = -20,
                    y = -10,
                    w = 40,
                    h = 20,
                    zwidth = 100,
                },
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            targetFrame.state = LF2States.Injured2;
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_StrictZAttacker",
                3350,
                attackerFrame);
            LF2Character target = CreateCharacter(
                "RoleFormal_StrictZTarget",
                3351,
                targetFrame);
            Register(world, attacker, 0, 1, 0);
            Register(world, target, 1, 2, 0);
            attacker.Runtime.KeyLeft = 1;
            attacker.Runtime.KeyRight = 0;
            target.Runtime.SetPosition(0, 0, targetZ);
            target.Runtime.SyncIntegerPosition();

            BruteForceSceneQuery query = GetQuery(world);
            query.ForceRoleAwareDirectForDiagnostics = true;
            query.ForceLegacyRoleAwareExactPrefilterForDiagnostics = true;
            CandidateRun legacy = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);
            query.ForceLegacyRoleAwareExactPrefilterForDiagnostics = false;
            CandidateRun cached = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);

            AssertRunsEqual(legacy, cached, $"targetZ={targetZ}");
            Assert.That(cached.FormalPairCount, Is.EqualTo(1));
            Assert.That(cached.Counts, Is.EqualTo(new[] { expectedCandidateCount }));
            Assert.That(cached.CollectionAborted, Is.False);
        }

        [Test]
        public void Formal_FallbackRoleListsMatchOldPredicateResetAndStayWarm()
        {
            var world = new SimulationWorld();
            LF2Character fallbackAttack = CreateCharacter(
                "RoleFormal_FallbackListAttack",
                3360,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = 0,
                        y = -10,
                        w = 0,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character fallbackBody = CreateCharacter(
                "RoleFormal_FallbackListBody",
                3361,
                MakeFrame(
                    null,
                    new BodyBox
                    {
                        kind = 0,
                        x = 0,
                        y = -10,
                        w = 0,
                        h = 20,
                    }));
            LF2Character dualFallback = CreateCharacter(
                "RoleFormal_FallbackListDual",
                3362,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = 0,
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
                        w = 0,
                        h = 20,
                    }));
            LF2Character exact = CreateCharacter(
                "RoleFormal_FallbackListExact",
                3363,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -20,
                        y = -10,
                        w = 40,
                        h = 20,
                        zwidth = 15,
                    },
                    new BodyBox
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                    }));
            LF2Character bodyOnly = CreateCharacter(
                "RoleFormal_FallbackListBodyOnly",
                3364,
                MakeFrame(
                    null,
                    new BodyBox
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                    }));
            LF2Character inert = CreateCharacter(
                "RoleFormal_FallbackListInert",
                3365,
                MakeFrame(null, null));
            Register(world, fallbackAttack, 0, 1, 0);
            Register(world, fallbackBody, 1, 2, 0);
            Register(world, dualFallback, 2, 3, 0);
            Register(world, exact, 3, 4, 0);
            Register(world, bodyOnly, 4, 5, 0);
            Register(world, inert, 5, 6, 0);

            BruteForceSceneQuery query = GetQuery(world);
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.CaptureCollisionFrameSnapshotsAll();
            long fullCollectionAllocations =
                query.MeasureWarmedRoleAwareCollectAllocationsForSelfCheck(16);
            var oldPredicateKeys = new List<long>();
            var roleListKeys = new List<long>();
            query.CopyLastRoleAwareFallbackPairKeysForSelfCheck(
                oldPredicateKeys,
                roleListKeys);
            query.GetLastRoleAwareFallbackOrdinalCountsForSelfCheck(
                out int bodyCount,
                out int fallbackAttackCount,
                out int exactAttackCount,
                out int fallbackBodyCount);

            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
            Assert.That(roleListKeys, Is.EqualTo(oldPredicateKeys));
            Assert.That(roleListKeys, Has.Count.EqualTo(8));
            Assert.That(
                roleListKeys.Contains(((long)2 << 32) | 2L),
                Is.False);
            Assert.That(
                roleListKeys.Contains(((long)3 << 32) | 4L),
                Is.False);
            Assert.That(bodyCount, Is.EqualTo(4));
            Assert.That(fallbackAttackCount, Is.EqualTo(2));
            Assert.That(exactAttackCount, Is.EqualTo(1));
            Assert.That(fallbackBodyCount, Is.EqualTo(2));
            Assert.That(
                query.MeasureWarmedRoleAwareFallbackPairAllocationsForSelfCheck(64),
                Is.Zero);
            Assert.That(
                fullCollectionAllocations,
                Is.LessThan(65536L),
                "Warmed role-aware collection must not allocate ordinal lists per tick.");

            world.Unregister(fallbackAttack);
            world.Unregister(fallbackBody);
            world.Unregister(dualFallback);
            world.Unregister(bodyOnly);
            world.Unregister(inert);
            RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                exact);
            query.GetLastRoleAwareFallbackOrdinalCountsForSelfCheck(
                out bodyCount,
                out fallbackAttackCount,
                out exactAttackCount,
                out fallbackBodyCount);
            query.CopyLastRoleAwareFallbackPairKeysForSelfCheck(
                oldPredicateKeys,
                roleListKeys);

            Assert.That(bodyCount, Is.EqualTo(1));
            Assert.That(fallbackAttackCount, Is.Zero);
            Assert.That(exactAttackCount, Is.EqualTo(1));
            Assert.That(fallbackBodyCount, Is.Zero);
            Assert.That(oldPredicateKeys, Is.Empty);
            Assert.That(roleListKeys, Is.Empty);
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
        public void Formal_SweepMatchesNestedForStrictXEdgeCases()
        {
            var world = new SimulationWorld();
            LF2FrameData duplicateItrFrame = MakeFrame(
                new InteractionArea
                {
                    kind = 0,
                    vrest = 1,
                    x = 0,
                    y = -10,
                    w = 10,
                    h = 20,
                    zwidth = 15,
                },
                new BodyBox { kind = 0, x = 0, y = -10, w = 10, h = 20 });
            duplicateItrFrame.itrs.Add(new InteractionArea
            {
                kind = 0,
                vrest = 1,
                x = 0,
                y = -10,
                w = 10,
                h = 20,
                zwidth = 15,
            });
            LF2Character duplicateItr = CreateCharacter(
                "RoleSweep_DuplicateItr",
                4100,
                duplicateItrFrame);
            LF2Character touchingBody = CreateCharacter(
                "RoleSweep_TouchingBody",
                4101,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = 10, y = -10, w = 10, h = 20 }));
            LF2Character containedBody = CreateCharacter(
                "RoleSweep_ContainedBody",
                4102,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = 2, y = -10, w = 4, h = 20 }));
            LF2Character wideBody = CreateCharacter(
                "RoleSweep_WideBody",
                4103,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -100, y = -10, w = 200, h = 20 }));
            LF2Character sameStartDual = CreateCharacter(
                "RoleSweep_SameStartDual",
                4104,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = 0,
                        y = -10,
                        w = 10,
                        h = 20,
                        zwidth = 15,
                    },
                    new BodyBox { kind = 0, x = 0, y = -10, w = 10, h = 20 }));
            LF2Character zRejectedBody = CreateCharacter(
                "RoleSweep_ZRejectedBody",
                4105,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = 0, y = -10, w = 10, h = 20 }));

            Register(world, duplicateItr, 0, 1, 0);
            Register(world, touchingBody, 1, 2, 0);
            Register(world, containedBody, 2, 2, 0);
            Register(world, wideBody, 3, 2, 0);
            Register(world, sameStartDual, 4, 3, 0);
            Register(world, zRejectedBody, 5, 2, 0);
            zRejectedBody.Runtime.SetPosition(0, 0, 100);
            zRejectedBody.Runtime.SyncIntegerPosition();

            BruteForceSceneQuery query = GetQuery(world);
            LF2Entity[] attackers = { duplicateItr, sameStartDual };
            query.ForceRoleAwareNestedDirectForDiagnostics = true;
            CandidateRun nested = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                attackers);
            Assert.That(
                query.LastRoleAwareNestedDirectTickCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareSweepDirectTickCountForDiagnostics,
                Is.Zero);

            query.ForceRoleAwareSweepDirectForDiagnostics = true;
            CandidateRun sweep = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                attackers);

            AssertRunsEqual(nested, sweep);
            Assert.That(
                sweep.FormalRuntimeSlotPairKeys,
                Is.EqualTo(nested.FormalRuntimeSlotPairKeys));
            Assert.That(
                sweep.FormalRuntimeSlotPairKeys,
                Has.No.Member(RuntimeSlotPairKey(0, 1)),
                "Strict half-open X must reject endpoint-only contact.");
            Assert.That(
                sweep.FormalRuntimeSlotPairKeys,
                Has.Member(RuntimeSlotPairKey(0, 2)),
                "Contained body must be discovered.");
            Assert.That(
                sweep.FormalRuntimeSlotPairKeys,
                Has.Member(RuntimeSlotPairKey(0, 3)),
                "Wide containing body must be discovered.");
            Assert.That(
                sweep.FormalRuntimeSlotPairKeys,
                Has.Member(RuntimeSlotPairKey(0, 4)),
                "Equal-start opposite roles must meet exactly once before dedup.");
            Assert.That(
                sweep.FormalRuntimeSlotPairKeys.Exists(
                    key => (int)(key >> 32) == 5 || (int)key == 5),
                Is.False,
                "X candidates rejected on Z must not become authority pairs.");
            Assert.That(
                query.LastRoleAwareSweepDirectTickCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareNestedDirectTickCountForDiagnostics,
                Is.Zero);
            Assert.That(
                query.LastRoleAwareSweepXCandidateCountForDiagnostics,
                Is.GreaterThan(query.LastFormalPairCountForDiagnostics));
            Assert.That(
                query.LastRoleAwareSweepFullOverlapCheckCountForDiagnostics,
                Is.EqualTo(query.LastRoleAwareSweepXCandidateCountForDiagnostics));
        }

        [Test]
        public void Formal_AdaptiveDirectUsesNestedBelowAndSweepAtCrossover()
        {
            var smallWorld = new SimulationWorld();
            for (int slot = 0; slot < 2; slot++)
            {
                LF2Character entity = CreateCharacter(
                    $"RoleSweep_Small_{slot}",
                    4150 + slot,
                    MakeFrame(
                        new InteractionArea
                        {
                            kind = 0,
                            x = 0,
                            y = -10,
                            w = 10,
                            h = 20,
                            zwidth = 15,
                        },
                        new BodyBox { kind = 0, x = 0, y = -10, w = 10, h = 20 }));
                Register(smallWorld, entity, slot, slot + 1, slot * 100);
            }
            BruteForceSceneQuery smallQuery = GetQuery(smallWorld);
            RunCollection(
                smallWorld,
                smallQuery,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                Array.Empty<LF2Entity>());
            Assert.That(
                smallQuery.LastRoleAwareDirectCostForDiagnostics,
                Is.LessThan(BruteForceSceneQuery.RoleAwareSweepDirectCrossover));
            Assert.That(
                smallQuery.LastRoleAwareNestedDirectTickCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                smallQuery.LastRoleAwareSweepDirectTickCountForDiagnostics,
                Is.Zero);

            const int mediumParticipantCount = 91;
            var mediumWorld = new SimulationWorld();
            for (int slot = 0; slot < mediumParticipantCount; slot++)
            {
                LF2Character entity = CreateCharacter(
                    $"RoleSweep_Medium_{slot}",
                    4160 + slot,
                    MakeFrame(
                        new InteractionArea
                        {
                            kind = 0,
                            x = 0,
                            y = -10,
                            w = 10,
                            h = 20,
                            zwidth = 15,
                        },
                        new BodyBox { kind = 0, x = 0, y = -10, w = 10, h = 20 }));
                Register(mediumWorld, entity, slot, (slot % 4) + 1, slot * 100);
            }
            BruteForceSceneQuery mediumQuery = GetQuery(mediumWorld);
            RunCollection(
                mediumWorld,
                mediumQuery,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                Array.Empty<LF2Entity>());
            Assert.That(
                mediumQuery.LastRoleAwareDirectCostForDiagnostics,
                Is.GreaterThanOrEqualTo(
                    BruteForceSceneQuery.RoleAwareSweepDirectCrossover));
            Assert.That(
                mediumQuery.LastRoleAwareDirectCostForDiagnostics,
                Is.LessThanOrEqualTo(
                    BruteForceSceneQuery.RoleAwareDirectComparisonThreshold));
            Assert.That(
                mediumQuery.LastRoleAwareSweepDirectTickCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                mediumQuery.LastRoleAwareNestedDirectTickCountForDiagnostics,
                Is.Zero);
            Assert.That(
                mediumQuery.LastRoleAwareSweepXCandidateCountForDiagnostics,
                Is.EqualTo(mediumParticipantCount),
                "Separated medium fixtures should retain only self X candidates, " +
                "which AddAuthorityOrdinalPair rejects.");
        }

        [TestCase(19)]
        [TestCase(733)]
        [TestCase(20260801)]
        public void Formal_SweepMatchesNestedForDeterministicRandomFixtures(int seed)
        {
            const int participantCount = 48;
            var random = new System.Random(seed);
            var world = new SimulationWorld();
            var attackers = new LF2Entity[participantCount];
            for (int slot = 0; slot < participantCount; slot++)
            {
                var itr = new InteractionArea
                {
                    kind = 0,
                    vrest = random.Next(0, 2),
                    x = random.Next(-25, 26),
                    y = -10,
                    w = random.Next(1, 51),
                    h = 20,
                    zwidth = random.Next(1, 31),
                };
                var body = new BodyBox
                {
                    kind = 0,
                    x = random.Next(-25, 26),
                    y = -10,
                    w = random.Next(1, 51),
                    h = 20,
                };
                LF2FrameData frame = MakeFrame(itr, body);
                if (slot % 5 == 0)
                {
                    frame.itrs.Add(new InteractionArea
                    {
                        kind = itr.kind,
                        vrest = itr.vrest,
                        x = itr.x,
                        y = itr.y,
                        w = itr.w,
                        h = itr.h,
                        zwidth = itr.zwidth,
                    });
                }
                LF2Character entity = CreateCharacter(
                    $"RoleSweep_Random_{seed}_{slot}",
                    4200 + slot,
                    frame);
                int x = random.Next(-250, 251);
                int z = random.Next(-40, 41);
                Register(world, entity, slot, (slot % 4) + 1, x);
                entity.Runtime.SetPosition(x, 0, z);
                entity.Runtime.SyncIntegerPosition();
                attackers[slot] = entity;
            }

            BruteForceSceneQuery query = GetQuery(world);
            uint collectionSeed = unchecked((uint)seed * 2654435761u);
            query.ForceRoleAwareNestedDirectForDiagnostics = true;
            CandidateRun nested = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                collectionSeed,
                attackers);
            query.ForceRoleAwareSweepDirectForDiagnostics = true;
            CandidateRun sweep = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                collectionSeed,
                attackers);

            AssertRunsEqual(nested, sweep, $"seed={seed}");
            Assert.That(
                sweep.FormalRuntimeSlotPairKeys,
                Is.EqualTo(nested.FormalRuntimeSlotPairKeys),
                $"seed={seed}");
            Assert.That(sweep.CollectionAborted, Is.False);
            Assert.That(query.LastRoleAwareSweepDirectTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(
                query.LastRoleAwareSweepFullOverlapCheckCountForDiagnostics,
                Is.LessThan(query.LastRoleAwareDirectCostForDiagnostics));

            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.CaptureCollisionFrameSnapshotsAll();
            long allocatedBytes =
                query.MeasureWarmedRoleAwareDirectAllocationsForSelfCheck(64);
            Assert.That(
                allocatedBytes,
                Is.LessThanOrEqualTo(256L),
                "Warmed event sweep must reuse event, active, and position buffers.");
        }

        [Test]
        public void Formal_ForcedSweepKeepsDegenerateRolesOnTreeFallbackPath()
        {
            var world = new SimulationWorld();
            LF2Character invalidItr = CreateCharacter(
                "RoleSweep_InvalidItr",
                4300,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        x = 0,
                        y = -10,
                        w = 0,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character validItr = CreateCharacter(
                "RoleSweep_ValidItr",
                4301,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        x = 0,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character invalidBody = CreateCharacter(
                "RoleSweep_InvalidBody",
                4302,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = 0, y = -10, w = 0, h = 20 }));
            LF2Character validBody = CreateCharacter(
                "RoleSweep_ValidBody",
                4303,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = 0, y = -10, w = 20, h = 20 }));
            Register(world, invalidItr, 0, 1, 0);
            Register(world, validItr, 1, 1, 0);
            Register(world, invalidBody, 2, 2, 0);
            Register(world, validBody, 3, 2, 0);

            BruteForceSceneQuery query = GetQuery(world);
            LF2Entity[] attackers = { invalidItr, validItr };
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                CollectionSeed,
                attackers);
            query.ForceRoleAwareSweepDirectForDiagnostics = true;
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                CollectionSeed,
                attackers);

            AssertRunsEqual(brute, role);
            Assert.That(role.CollectionAborted, Is.False);
            Assert.That(query.LastRoleAwareSweepDirectTickCountForDiagnostics, Is.Zero);
            Assert.That(query.LastRoleAwareNestedDirectTickCountForDiagnostics, Is.Zero);
            Assert.That(query.LastRoleAwareTreeTickCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareSweepXCandidateCountForDiagnostics, Is.Zero);
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
        public void CandidateStore_GenerationTargetReuseFaultIsolationAndGrow()
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
            query.ForceRoleAwareDirectForDiagnostics = true;
            CandidateRun direct = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                replacementWinsSeed,
                attacker);
            query.ForceRoleAwareDirectForDiagnostics = false;
            query.ForceRoleAwareTreeForDiagnostics = true;
            CandidateRun tree = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                replacementWinsSeed,
                attacker);
            AssertRunsEqual(legacy, direct);
            AssertRunsEqual(direct, tree);
            Assert.That(direct.CollectionAborted, Is.False);
            Assert.That(direct.TargetHandles[0], Has.Count.EqualTo(1));
            Assert.That(direct.TargetHandles[0][0], Is.EqualTo(replacementHandle));

            query.ForceRoleAwareTreeForDiagnostics = false;
            query.ForceRoleAwareDirectForDiagnostics = true;
            query.CollisionCandidateStoreShadowDiagnosticsEnabled = true;
            query.CollisionCandidateStoreAuthorityEnabled = true;
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
            Assert.That(query.CollisionCandidateStoreShadowDiagnostics.BuildTickCount,
                Is.EqualTo(2),
                "the failed formal write set must be discarded before the brute fallback rebuild");
            Assert.That(query.CollisionCandidateStoreShadowDiagnostics.MismatchCount,
                Is.Zero);
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.AppliedTickCount,
                Is.EqualTo(1),
                "formal abort must rebuild once from the brute authority and then expose that completed store");
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.LegacyFallbackTickCount,
                Is.Zero);

            query.ThrowAfterCollisionCandidateStoreAppendCountForSelfCheck = 1;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.Rng.Seed(replacementWinsSeed);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                query.CollisionCandidateStoreAuthorityAppliedForCurrentTickForDiagnostics,
                Is.False,
                "an incomplete store must lock the whole tick to the legacy oracle");
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.LegacyFallbackTickCount,
                Is.EqualTo(1));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False,
                "a shadow append failure must not abort formal authority");
            Assert.That(world.Rng.State, Is.EqualTo(recovered.RngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(recovered.RngCalls));
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    attacker,
                    out List<SceneQueryHit> faultAuthority),
                Is.True);
            Assert.That(
                faultAuthority.ConvertAll(hit => hit.TargetSlot),
                Is.EqualTo(recovered.Sequences[0].ConvertAll(hit => hit.TargetSlot)));
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    attacker.Runtime.SlotIndex,
                    attacker,
                    out RuntimeEntityHandle attackerHandle),
                Is.True);
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    attackerHandle,
                    out _),
                Is.False,
                "a partial slab must remain invisible after an injected failure");
            Assert.That(query.CollisionCandidateStoreShadowDiagnostics.InvalidCount,
                Is.EqualTo(1));
            Assert.That(query.CollisionCandidateStoreShadowDiagnostics.MismatchCount,
                Is.EqualTo(1));
            Assert.That(
                query.CollisionCandidateStoreShadowDiagnostics.FirstMismatchReason,
                Is.EqualTo(CollisionCandidateStoreMismatchReason.UnexpectedShadowException));
            world.EndCollisionCandidateConsumption();

            query.ThrowAfterCollisionCandidateStoreAppendCountForSelfCheck = -1;
            world.Rng.Seed(replacementWinsSeed);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                query.CollisionCandidateStoreAuthorityAppliedForCurrentTickForDiagnostics,
                Is.True);
            Assert.That(
                query.TryGetCollisionCandidateStoreEntryForSelfCheck(
                    attackerHandle,
                    0,
                    out CollisionCandidateStoreEntry storedBeforeReuse),
                Is.True);
            Assert.That(storedBeforeReuse.TargetHandle, Is.EqualTo(replacementHandle));
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    attacker,
                    out List<SceneQueryHit> liveAuthority),
                Is.True);
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange storeAuthorityRange),
                Is.True);
            Assert.That(storeAuthorityRange.Count, Is.EqualTo(liveAuthority.Count));

            world.Unregister(replacement);
            LF2Character newborn = CreateCharacter(
                "RoleFormal_GenerationNewborn",
                5,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            Register(world, newborn, 1, 2, 10);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    1,
                    newborn,
                    out RuntimeEntityHandle newbornHandle),
                Is.True);
            Assert.That(newbornHandle, Is.Not.EqualTo(replacementHandle));
            Assert.That(liveAuthority[0].ResolveCurrentTarget(world), Is.SameAs(newborn));
            Assert.That(
                storeAuthorityRange.TryGet(0, out SceneQueryHit storeAuthorityAfterTargetReuse),
                Is.True);
            Assert.That(storeAuthorityAfterTargetReuse.TargetSlot, Is.EqualTo(1));
            Assert.That(storeAuthorityAfterTargetReuse.Target, Is.SameAs(newborn),
                "target generation is diagnostic-only; authority follows the current slot occupant");
            Assert.That(
                query.TryGetCollisionCandidateStoreEntryForSelfCheck(
                    attackerHandle,
                    0,
                    out CollisionCandidateStoreEntry storedAfterReuse),
                Is.True);
            Assert.That(storedAfterReuse.TargetSlot, Is.EqualTo(1));
            Assert.That(storedAfterReuse.TargetHandle, Is.EqualTo(replacementHandle),
                "target generation is diagnostic-only and must not gate slot-based authority");

            world.Unregister(attacker);
            LF2Character attackerNewborn = CreateCharacter(
                "RoleFormal_GenerationAttackerNewborn",
                6,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -30,
                        y = -10,
                        w = 60,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            Register(world, attackerNewborn, 2, 1, 0);
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    2,
                    attackerNewborn,
                    out RuntimeEntityHandle attackerNewbornHandle),
                Is.True);
            Assert.That(attackerNewbornHandle, Is.Not.EqualTo(attackerHandle));
            Assert.That(storeAuthorityRange.TryGet(0, out _), Is.False,
                "an attacker range is generation-gated after same-slot reuse");
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    attackerHandle,
                    out _),
                Is.False,
                "the released attacker handle must be rejected before End");
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    attackerNewbornHandle,
                    out _),
                Is.False,
                "the same-slot newborn generation must not inherit the old row");
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    attackerNewborn,
                    out List<SceneQueryHit> newbornAuthorityBeforeEnd),
                Is.True);
            Assert.That(newbornAuthorityBeforeEnd, Is.Empty,
                "the newborn remains outside the current authoritative snapshot");
            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attackerNewborn,
                    out CollisionCandidateRange newbornAuthorityRange),
                Is.True);
            Assert.That(newbornAuthorityRange.Count, Is.Zero,
                "a step8 newborn must not trigger an immediate query or inherit a row");
            world.EndCollisionCandidateConsumption();
            Assert.That(storeAuthorityRange.Count, Is.Zero);
            Assert.That(newbornAuthorityRange.Count, Is.Zero);

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    2,
                    attackerNewborn,
                    out RuntimeEntityHandle currentAttackerNewbornHandle),
                Is.True);
            Assert.That(currentAttackerNewbornHandle, Is.EqualTo(attackerNewbornHandle));
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    attackerHandle,
                    out _),
                Is.False);
            Assert.That(
                query.TryGetCollisionCandidateStoreRowForSelfCheck(
                    attackerNewbornHandle,
                    out int attackerNewbornCount),
                Is.True);
            Assert.That(attackerNewbornCount, Is.GreaterThan(0));
            Assert.That(query.CollisionCandidateStoreShadowDiagnostics.MismatchCount,
                Is.EqualTo(1),
                "the injected fault remains diagnostic history after next-tick recovery");
            world.EndCollisionCandidateConsumption();

            var grownWorld = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                512);
            BruteForceSceneQuery grownQuery = GetQuery(grownWorld);
            grownQuery.CollisionCandidateStoreShadowDiagnosticsEnabled = true;
            Assert.That(grownQuery.CollisionCandidateStoreRuntimeCapacityForDiagnostics,
                Is.Zero);
            grownWorld.CaptureCollisionFrameSnapshotsAll();
            grownWorld.CollectCollisionCandidatesAll();
            Assert.That(grownQuery.CollisionCandidateStoreRuntimeCapacityForDiagnostics,
                Is.EqualTo(512));
            grownWorld.EndCollisionCandidateConsumption();

            LF2Character grownTarget = CreateCharacter(
                "CandidateStore_GrowTarget",
                7001,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character highSlotAttacker = CreateCharacter(
                "CandidateStore_HighSlotAttacker",
                7002,
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
            Register(grownWorld, grownTarget, 1, 2, 0);
            Register(grownWorld, highSlotAttacker, 600, 1, 0);
            int grownRuntimeCapacity = grownWorld.RuntimeSlotCapacityForDiagnostics;
            Assert.That(grownRuntimeCapacity, Is.GreaterThan(512));
            Assert.That(grownRuntimeCapacity, Is.GreaterThan(600),
                "the grown runtime must address the requested high slot");
            grownWorld.CaptureCollisionFrameSnapshotsAll();
            grownWorld.CollectCollisionCandidatesAll();
            Assert.That(
                grownWorld.TryGetCurrentRuntimeHandleForDiagnostics(
                    600,
                    highSlotAttacker,
                    out RuntimeEntityHandle highSlotHandle),
                Is.True);
            Assert.That(
                grownQuery.TryGetCollisionCandidateStoreRowForSelfCheck(
                    highSlotHandle,
                    out int highSlotCount),
                Is.True);
            Assert.That(highSlotCount, Is.EqualTo(1));
            Assert.That(grownQuery.CollisionCandidateStoreRuntimeCapacityForDiagnostics,
                Is.EqualTo(grownRuntimeCapacity));
            grownWorld.EndCollisionCandidateConsumption();

            Assert.That(
                grownQuery.TryBuildCollisionCandidateStoreCapacityForSelfCheck(int.MaxValue),
                Is.False);
            Assert.That(grownQuery.CollisionCandidateStoreRuntimeCapacityForDiagnostics,
                Is.EqualTo(grownRuntimeCapacity),
                "overflow rejection must retain the last valid slab");
            Assert.That(grownQuery.CollisionCandidateStoreShadowDiagnostics.InvalidCount,
                Is.EqualTo(1));
            Assert.That(
                grownQuery.CollisionCandidateStoreShadowDiagnostics.FirstMismatchReason,
                Is.EqualTo(CollisionCandidateStoreMismatchReason.RuntimeCapacityInvalid));
        }

        [Test]
        public void CandidateStoreAuthority_StoreOnlyProducerFaultFailsClosedWithoutCollectorRerun()
        {
            uint seed = FindSeedWithFirstTieReplacement();
            CreateStoreOnlyFaultScenario(
                out SimulationWorld expectedWorld,
                out BruteForceSceneQuery expectedQuery,
                out LF2Character expectedAttacker,
                out _,
                out _);
            expectedWorld.Rng.Seed(seed);
            expectedWorld.CaptureCollisionFrameSnapshotsAll();
            expectedWorld.CollectCollisionCandidatesAll();
            uint expectedRngState = expectedWorld.Rng.State;
            ulong expectedRngCalls = expectedWorld.Rng.CallCount;
            Assert.That(expectedRngCalls, Is.EqualTo(1));
            Assert.That(expectedQuery.LastFormalCollectionAbortedForDiagnostics, Is.False);
            Assert.That(expectedAttacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            expectedWorld.EndCollisionCandidateConsumption();

            CreateStoreOnlyFaultScenario(
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker,
                out InteractionArea itr,
                out PhysicsState.BattleVolume volume);
            Assert.That(query.QueryBodyHits(attacker, attacker.Frame.D, itr), Is.Not.Empty,
                "the geometry must produce immediate hits before the frozen window");
            query.ThrowAfterCollisionCandidateStoreAppendCountForSelfCheck = 1;
            world.Rng.Seed(seed);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            Assert.That(query.CollisionCandidateStoreOnlyForCurrentTickForDiagnostics, Is.True);
            Assert.That(
                query.CollisionCandidateStoreLegacyOracleSampledForCurrentTickForDiagnostics,
                Is.False);
            Assert.That(
                query.CollisionCandidateStoreAuthorityAppliedForCurrentTickForDiagnostics,
                Is.False);
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False,
                "a store producer fault must not route through formal collector fallback");
            Assert.That(
                query.CollisionCandidateStoreShadowDiagnostics.BuildTickCount,
                Is.EqualTo(1),
                "a store producer fault must not restart the collector/store build");
            Assert.That(world.Rng.State, Is.EqualTo(expectedRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(expectedRngCalls));
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.SampledOracleTickCount,
                Is.Zero);
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.StoreOnlyTickCount,
                Is.EqualTo(1));
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.StoreOnlyHardFailureCount,
                Is.EqualTo(1));
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.LegacyFallbackTickCount,
                Is.Zero);
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics
                    .LegacyListCreatedOrWrittenCount,
                Is.Zero);
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.FailureCount,
                Is.EqualTo(1));
            Assert.That(
                query.CollisionCandidateStoreAuthorityDiagnostics.FirstFailureReason,
                Is.EqualTo(
                    CollisionCandidateStoreAuthorityFailureReason
                        .StoreOnlyProducerUnavailable));

            Assert.That(
                query.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange failedRange),
                Is.True);
            Assert.That(failedRange.Count, Is.Zero);
            Assert.That(failedRange.TryGet(0, out _), Is.False);
            uint frozenWindowRngState = world.Rng.State;
            ulong frozenWindowRngCalls = world.Rng.CallCount;
            Assert.That(query.QueryBodyHits(volume, attacker), Is.Empty);
            Assert.That(query.QueryBodyHits(attacker, attacker.Frame.D, itr), Is.Empty);
            Assert.That(
                query.QueryBodyHits(attacker, attacker.Frame.D, itr, volume),
                Is.Empty);
            Assert.That(world.Rng.State, Is.EqualTo(frozenWindowRngState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(frozenWindowRngCalls),
                "frozen-window queries must not fall back to an immediate collector");

            world.EndCollisionCandidateConsumption();
            Assert.That(failedRange.Count, Is.Zero);
            Assert.That(failedRange.TryGet(0, out _), Is.False);
            query.ThrowAfterCollisionCandidateStoreAppendCountForSelfCheck = -1;
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

        private static void CreateExactLoopTieFixture(
            out SimulationWorld world,
            out BruteForceSceneQuery query,
            out LF2Character attacker)
        {
            world = new SimulationWorld();
            attacker = CreateCharacter(
                "RoleExact_TieAttacker",
                1880,
                MakeFrame(
                    MakeExactLoopItr(kind: 0, vrest: 0),
                    null));
            LF2FrameData targetFrame = MakeFrame(
                null,
                new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            LF2Character left = CreateCharacter(
                "RoleExact_TieLeft",
                1881,
                targetFrame);
            LF2Character right = CreateCharacter(
                "RoleExact_TieRight",
                1882,
                targetFrame);
            Register(world, left, 0, 2, -10);
            Register(world, right, 1, 2, 10);
            Register(world, attacker, 2, 1, 0);
            query = GetQuery(world);
        }

        private static void CreateExactLoopDirectionalFixture(
            int scenario,
            out SimulationWorld world,
            out BruteForceSceneQuery query,
            out LF2Character first,
            out LF2Character second)
        {
            InteractionArea firstItr;
            InteractionArea secondItr;
            switch (scenario)
            {
                case 0:
                    firstItr = MakeExactLoopItr(kind: 0, vrest: 1);
                    secondItr = null;
                    break;
                case 1:
                    firstItr = MakeExactLoopItr(kind: 0, vrest: 1);
                    secondItr = MakeExactLoopItr(kind: 0, vrest: 1);
                    break;
                case 2:
                    firstItr = null;
                    secondItr = MakeExactLoopItr(kind: 0, vrest: 1);
                    break;
                case 3:
                    firstItr = MakeExactLoopItr(kind: 5, vrest: 1);
                    secondItr = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario));
            }

            var body = new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            };
            world = new SimulationWorld();
            first = CreateCharacter(
                $"RoleExact_DirectionalFirst_{scenario}",
                1900 + scenario * 2,
                MakeFrame(firstItr, body));
            second = CreateCharacter(
                $"RoleExact_DirectionalSecond_{scenario}",
                1901 + scenario * 2,
                MakeFrame(secondItr, body));
            Register(world, first, 0, 1, 0);
            Register(world, second, 1, 2, 0);
            query = GetQuery(world);
        }

        private static InteractionArea MakeExactLoopItr(int kind, int vrest)
        {
            return new InteractionArea
            {
                kind = kind,
                vrest = vrest,
                x = -30,
                y = -10,
                w = 60,
                h = 20,
                zwidth = 15,
            };
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

        private static void AssertExactCacheCounts(
            BruteForceSceneQuery query,
            LF2Entity participant,
            int expectedCommonBuildCount,
            int expectedAttackBuildCount,
            int expectedBodyBuildCount,
            int expectedValidationCount,
            bool expectedAttackRequired,
            bool expectedBodyRequired)
        {
            Assert.That(
                query.TryGetLastRoleAwareExactCacheCountsForSelfCheck(
                    participant,
                    out int commonBuildCount,
                    out int attackBuildCount,
                    out int bodyBuildCount,
                    out int validationCount,
                    out bool attackRequired,
                    out bool bodyRequired),
                Is.True);
            Assert.That(commonBuildCount, Is.EqualTo(expectedCommonBuildCount));
            Assert.That(attackBuildCount, Is.EqualTo(expectedAttackBuildCount));
            Assert.That(bodyBuildCount, Is.EqualTo(expectedBodyBuildCount));
            Assert.That(validationCount, Is.EqualTo(expectedValidationCount));
            Assert.That(attackRequired, Is.EqualTo(expectedAttackRequired));
            Assert.That(bodyRequired, Is.EqualTo(expectedBodyRequired));
            Assert.That(commonBuildCount, Is.LessThanOrEqualTo(1));
            Assert.That(attackBuildCount, Is.LessThanOrEqualTo(1));
            Assert.That(bodyBuildCount, Is.LessThanOrEqualTo(1));
            Assert.That(validationCount, Is.LessThanOrEqualTo(1));
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
            return RunCollectionCore(
                world,
                query,
                mode,
                seed,
                null,
                attackers);
        }

        private static CandidateRun RunCollectionWithCollisionSnapshotOverride(
            SimulationWorld world,
            BruteForceSceneQuery query,
            CollisionFormalCollectorMode mode,
            uint seed,
            Action collisionSnapshotOverride,
            params LF2Entity[] attackers)
        {
            return RunCollectionCore(
                world,
                query,
                mode,
                seed,
                collisionSnapshotOverride,
                attackers);
        }

        private static CandidateRun RunCollectionCore(
            SimulationWorld world,
            BruteForceSceneQuery query,
            CollisionFormalCollectorMode mode,
            uint seed,
            Action collisionSnapshotOverride,
            params LF2Entity[] attackers)
        {
            query.FormalCollectorMode = mode;
            world.Rng.Seed(seed);
            world.CaptureCollisionFrameSnapshotsAll();
            collisionSnapshotOverride?.Invoke();
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

        private static bool[] ReadRoleAwareParticipantFlags(
            BruteForceSceneQuery query,
            LF2Entity entity)
        {
            Assert.That(
                query.TryGetLastRoleAwareParticipantFlagsForSelfCheck(
                    entity,
                    out bool hasBody,
                    out bool hasFallbackBody,
                    out bool hasAttackItr,
                    out bool hasFallbackAttackItr),
                Is.True);
            return new[]
            {
                hasBody,
                hasFallbackBody,
                hasAttackItr,
                hasFallbackAttackItr,
            };
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

        private static void CreateStoreOnlyFaultScenario(
            out SimulationWorld world,
            out BruteForceSceneQuery query,
            out LF2Character attacker,
            out InteractionArea itr,
            out PhysicsState.BattleVolume volume)
        {
            itr = new InteractionArea
            {
                kind = 0,
                vrest = 0,
                x = -30,
                y = -10,
                w = 60,
                h = 20,
                zwidth = 15,
            };
            var body = new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            };
            world = new SimulationWorld();
            LF2Character leftTarget = CreateCharacter(
                "CandidateStoreOnlyFault_LeftTarget",
                2100,
                MakeFrame(null, body));
            LF2Character rightTarget = CreateCharacter(
                "CandidateStoreOnlyFault_RightTarget",
                2101,
                MakeFrame(null, body));
            attacker = CreateCharacter(
                "CandidateStoreOnlyFault_Attacker",
                2102,
                MakeFrame(itr, null));
            Register(world, leftTarget, 0, 2, -10);
            Register(world, rightTarget, 1, 2, 10);
            Register(world, attacker, 2, 1, 0);
            query = GetQuery(world);
            query.CollisionCandidateStoreAuthorityEnabled = true;
            query.CollisionCandidateStoreLegacyOracleInterval = 0;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            query.ForceRoleAwareDirectForDiagnostics = true;
            volume = new PhysicsState.BattleVolume(
                -100f,
                -100f,
                0f,
                0f,
                0f,
                200f,
                200f,
                30f);
        }

        private static long RuntimeSlotPairKey(int firstSlot, int secondSlot)
        {
            uint min = (uint)Math.Min(firstSlot, secondSlot);
            uint max = (uint)Math.Max(firstSlot, secondSlot);
            return ((long)min << 32) | max;
        }

        private static LF2Character CreateCharacter(
            string name,
            int objectId,
            LF2FrameData frame,
            bool specialAttack = false)
        {
            return CreateCharacterWithFrames(
                name,
                objectId,
                new List<LF2FrameData> { frame },
                specialAttack);
        }

        private static LF2Character CreateCharacterWithFrames(
            string name,
            int objectId,
            List<LF2FrameData> frames,
            bool specialAttack = false)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = frames,
            };
            LF2Character character = specialAttack
                ? new FormalSelfCheckSpecialAttack()
                : new FormalSelfCheckCharacter();
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

        private static void OverrideCollisionFrame(
            LF2Entity entity,
            int collisionFrameId)
        {
            LF2FrameData collisionFrame =
                entity.FrameCache.GetFrameDataById(collisionFrameId);
            Assert.That(collisionFrame, Is.Not.Null);
            entity.Frame.Prev2 = collisionFrameId;
            entity.Frame.Prev2D = collisionFrame;
            entity.Runtime.PrevFrame2 = collisionFrameId;
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

        private sealed class FormalSelfCheckSpecialAttack : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.SpecialAttack;
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
