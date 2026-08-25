#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Live-world S4 probe for collision collection, ordered consumption,
    /// damage/stat writers, vrest, and attacker-abort gates.
    /// Alignment contract: R8-HITPLAY-001.
    /// </summary>
    public static class BattleCollisionHitDamagePlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/验证/R8/运行碰撞命中伤害Abort Play探针";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json";
        private const int TickTimeoutEditorUpdates = 1800;
        private const int MaximumBaselineEntities = 64;
        private const int ProbeOidBase = 8200;

        private static readonly List<LF2Entity> OwnedEntities =
            new List<LF2Entity>(32);
        private static readonly List<LF2Entity> BaselineEntities =
            new List<LF2Entity>(16);
        private static readonly List<RestEntry> BaselineRestEntries =
            new List<RestEntry>(256);
        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(32);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool objectPool;
        private static ProbeResult result;
        private static int[] baselineKillStats;
        private static int[] baselineDamageStats;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;
        private static BattleHitExecutionPlanMode baselineHitPlanMode;
        private static bool previousPaused;
        private static bool pauseRequested;
        private static bool baselineCaptured;
        private static bool running;
        private static int editorUpdates;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            ResetProbeState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            objectPool = LF2ObjectPool.Instance;
            if (driver == null || world == null || objectPool == null ||
                LF2ReferencePool.Instance == null)
            {
                WriteImmediateFailure(
                    "The production driver, world, or pools are unavailable.");
                return;
            }

            previousPaused = driver.IsPaused;
            result = new ProbeResult
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                workerWasActive = driver.DedicatedSimulationWorkerActiveForDiagnostics,
            };
            running = true;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                Fail("Play Mode or the production world ended before completion.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > TickTimeoutEditorUpdates)
            {
                Fail("Timed out waiting for a safe live-world boundary.");
                return;
            }
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Fail(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }

            if (!pauseRequested)
            {
                if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0 ||
                    world.ClaimedRuntimeSlotCountForDiagnostics <= 0)
                {
                    return;
                }

                driver.SetPaused(true);
                pauseRequested = true;
                CaptureBaseline();
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            try
            {
                result.matrix = ExecuteMatrix();
                FinishSuccess();
            }
            catch (Exception exception)
            {
                Fail("Unhandled probe exception: " + exception);
            }
        }

        private static MatrixEvidence ExecuteMatrix()
        {
            FixtureSet fixtures = BuildFixtures();
            int tick = driver.CurrentTickIndex + 400;
            world.CaptureCollisionFrameSnapshotsAll();
            world.TickCollisionPairVRestAll();
            world.CollectCollisionCandidatesAll();

            RequireBaselineHasNoCandidates();
            int totalCandidates = 0;
            totalCandidates += RequireCandidateOrder(
                fixtures.characterAttacker,
                fixtures.characterVictim);
            totalCandidates += RequireCandidateOrder(
                fixtures.hitConfirmAttacker,
                fixtures.hitConfirmFirst,
                fixtures.hitConfirmSecond);
            totalCandidates += RequireCandidateOrder(
                fixtures.caughtAttacker,
                fixtures.caughtFirst,
                fixtures.caughtSecond);
            totalCandidates += RequireCandidateOrder(
                fixtures.effectAttacker,
                fixtures.effectFirst,
                fixtures.effectSecond);
            totalCandidates += RequireCandidateOrder(
                fixtures.rawAttacker,
                fixtures.rawTarget);
            totalCandidates += RequireCandidateOrder(
                fixtures.weaponAttacker,
                fixtures.weaponTarget);
            totalCandidates += RequireCandidateOrder(
                fixtures.specialAttacker,
                fixtures.specialTarget);
            Require(totalCandidates == 10,
                "The source-derived matrix did not freeze exactly ten candidates.");

            fixtures.hitConfirmAttacker.HitConfirm2 = 1;
            fixtures.effectFirst.DirectWriteRawFramePreserveWaitCounter(18);
            Require(
                fixtures.effectFirst.Frame.Prev == 0 &&
                fixtures.effectFirst.Frame.D?.state == LF2States.Burning,
                "effect21 witness did not preserve prev state 0 while changing current state to 18");

            int objectHpBeforeCharacterPass = fixtures.weaponTarget.Health.HP +
                fixtures.specialTarget.Health.HP;
            world.PostInteractionTickAll(tick);

            Require(fixtures.characterVictim.Health.HP == -5 &&
                    fixtures.characterVictim.Health.HPBound == 97 &&
                    fixtures.characterVictim.ComboCountVic == 10,
                "character damage vital/combo writes mismatch");
            Require(fixtures.characterHolder.KillStat == 1 &&
                    fixtures.characterHolder.ComboCountAtk == 10 &&
                    world.KillStats[1] == baselineKillStats[1] + 1 &&
                    world.DamageStats[1] == baselineDamageStats[1] + 10,
                "character damage kill/combo/global-stat ownership mismatch");
            Require(
                world.GetRawRestVrest(
                    fixtures.characterVictim.Runtime.SlotIndex,
                    fixtures.characterAttacker.Runtime.SlotIndex) == 3,
                "character damage vrest mismatch");

            Require(fixtures.hitConfirmFirst.Health.HP == 100 &&
                    fixtures.hitConfirmSecond.Health.HP == 100,
                "HitConfirm2 did not abort the entire attacker before writers");
            Require(fixtures.caughtFirst.Health.HP == 100 &&
                    fixtures.caughtSecond.Health.HP < 100,
                "caught/hurtable gate did not skip only the first candidate");
            Require(
                world.GetRawRestVrest(
                    fixtures.caughtFirst.Runtime.SlotIndex,
                    fixtures.caughtAttacker.Runtime.SlotIndex) == 0 &&
                world.GetRawRestVrest(
                    fixtures.caughtSecond.Runtime.SlotIndex,
                    fixtures.caughtAttacker.Runtime.SlotIndex) > 0,
                "caught/hurtable gate vrest polarity mismatch");
            Require(fixtures.effectFirst.Health.HP == 100 &&
                    fixtures.effectSecond.Health.HP == 100,
                "effect21 state18 did not abort the entire attacker before writers");
            Require(fixtures.rawTarget.Frame.N == 182 &&
                    fixtures.rawTarget.Runtime.Frame == 182 &&
                    fixtures.rawTarget.Frame.PN == 41 &&
                    fixtures.rawTarget.AttackingCounter == 9 &&
                    fixtures.rawTarget.Trans.WaitCounter == 73,
                "kind10 raw-frame response changed PN/attacking/wait or missed frame182");
            Require(
                fixtures.weaponTarget.Health.HP + fixtures.specialTarget.Health.HP ==
                objectHpBeforeCharacterPass,
                "object attackers were consumed during the character pass");

            uint rngBeforeRandomWeapon = world.Rng.State;
            ulong rngCallsBeforeRandomWeapon = world.Rng.CallCount;
            int objectsBeforeRandomWeapon = world.ObjectCount;
            world.RandomWeaponDropTickAll(tick);
            Require(world.ObjectCount == objectsBeforeRandomWeapon &&
                    world.Rng.State == rngBeforeRandomWeapon &&
                    world.Rng.CallCount == rngCallsBeforeRandomWeapon,
                "random-weapon boundary changed state despite four active weapon fixtures");

            world.ObjectInteractionTickAll(tick);

            Require(fixtures.weaponTarget.Health.HP == 80 &&
                    fixtures.weaponTarget.Health.HPBound == 94 &&
                    fixtures.weaponTarget.ComboCountVic == 20 &&
                    fixtures.weaponTarget.Runtime.WeaponFlightCounter == 90,
                "weapon victim scaled vital/raw durability mismatch");
            Require(world.DamageStats[2] == baselineDamageStats[2] + 20 &&
                    world.GetRawRestVrest(
                        fixtures.weaponTarget.Runtime.SlotIndex,
                        fixtures.weaponAttacker.Runtime.SlotIndex) == 3 &&
                    fixtures.weaponTarget.HitConfirm2 == 1,
                "weapon tail damage-stat/vrest/HitConfirm2 mismatch");

            Require(fixtures.specialTarget.Health.HP == 90 &&
                    fixtures.specialTarget.Health.HPBound == 97 &&
                    fixtures.specialTarget.ComboCountVic == 10,
                "special target vital/combo mismatch");
            Require(world.DamageStats[1] == baselineDamageStats[1] + 20 &&
                    world.KillStats[1] == baselineKillStats[1] + 1 &&
                    world.GetRawRestVrest(
                        fixtures.specialTarget.Runtime.SlotIndex,
                        fixtures.specialAttacker.Runtime.SlotIndex) == 3 &&
                    fixtures.specialTarget.HitConfirm2 == 1,
                "special target stat/vrest/HitConfirm2 or type0-only kill exclusion mismatch");

            BattleHitExecutionPlanDiagnostics diagnostics =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
            bool comparisonAvailable =
                diagnostics.Mode == BattleHitExecutionPlanMode.ShadowCompare;
            if (comparisonAvailable)
            {
                Require(diagnostics.CurrentTickPlanValid &&
                        diagnostics.ObservationMismatchCount == 0 &&
                        diagnostics.FailureCount == 0,
                    "ShadowCompare detected a collision/hit first-difference: " +
                    DescribeDiagnostics(diagnostics));
            }

            world.EndCollisionCandidateConsumption();
            return new MatrixEvidence
            {
                tick = tick,
                totalCandidates = totalCandidates,
                character = new HitEvidence
                {
                    attackerSlot = fixtures.characterAttacker.Runtime.SlotIndex,
                    targetSlot = fixtures.characterVictim.Runtime.SlotIndex,
                    hp = fixtures.characterVictim.Health.HP,
                    hpBound = fixtures.characterVictim.Health.HPBound,
                    combo = fixtures.characterVictim.ComboCountVic,
                    vrest = world.GetRawRestVrest(
                        fixtures.characterVictim.Runtime.SlotIndex,
                        fixtures.characterAttacker.Runtime.SlotIndex),
                    frame = fixtures.characterVictim.Frame.N,
                },
                weapon = new HitEvidence
                {
                    attackerSlot = fixtures.weaponAttacker.Runtime.SlotIndex,
                    targetSlot = fixtures.weaponTarget.Runtime.SlotIndex,
                    hp = fixtures.weaponTarget.Health.HP,
                    hpBound = fixtures.weaponTarget.Health.HPBound,
                    combo = fixtures.weaponTarget.ComboCountVic,
                    vrest = world.GetRawRestVrest(
                        fixtures.weaponTarget.Runtime.SlotIndex,
                        fixtures.weaponAttacker.Runtime.SlotIndex),
                    frame = fixtures.weaponTarget.Frame.N,
                    durability = fixtures.weaponTarget.Runtime.WeaponFlightCounter,
                    hitConfirm2 = fixtures.weaponTarget.HitConfirm2,
                },
                special = new HitEvidence
                {
                    attackerSlot = fixtures.specialAttacker.Runtime.SlotIndex,
                    targetSlot = fixtures.specialTarget.Runtime.SlotIndex,
                    hp = fixtures.specialTarget.Health.HP,
                    hpBound = fixtures.specialTarget.Health.HPBound,
                    combo = fixtures.specialTarget.ComboCountVic,
                    vrest = world.GetRawRestVrest(
                        fixtures.specialTarget.Runtime.SlotIndex,
                        fixtures.specialAttacker.Runtime.SlotIndex),
                    frame = fixtures.specialTarget.Frame.N,
                    hitConfirm2 = fixtures.specialTarget.HitConfirm2,
                },
                hitConfirmAbort = new GateEvidence
                {
                    attackerSlot = fixtures.hitConfirmAttacker.Runtime.SlotIndex,
                    firstTargetSlot = fixtures.hitConfirmFirst.Runtime.SlotIndex,
                    secondTargetSlot = fixtures.hitConfirmSecond.Runtime.SlotIndex,
                    firstHp = fixtures.hitConfirmFirst.Health.HP,
                    secondHp = fixtures.hitConfirmSecond.Health.HP,
                    attackerAborted = true,
                },
                caughtGate = new GateEvidence
                {
                    attackerSlot = fixtures.caughtAttacker.Runtime.SlotIndex,
                    firstTargetSlot = fixtures.caughtFirst.Runtime.SlotIndex,
                    secondTargetSlot = fixtures.caughtSecond.Runtime.SlotIndex,
                    firstHp = fixtures.caughtFirst.Health.HP,
                    secondHp = fixtures.caughtSecond.Health.HP,
                    firstSkippedOnly = true,
                },
                effect21Abort = new GateEvidence
                {
                    attackerSlot = fixtures.effectAttacker.Runtime.SlotIndex,
                    firstTargetSlot = fixtures.effectFirst.Runtime.SlotIndex,
                    secondTargetSlot = fixtures.effectSecond.Runtime.SlotIndex,
                    firstHp = fixtures.effectFirst.Health.HP,
                    secondHp = fixtures.effectSecond.Health.HP,
                    attackerAborted = true,
                },
                rawFrame = new RawFrameEvidence
                {
                    attackerSlot = fixtures.rawAttacker.Runtime.SlotIndex,
                    targetSlot = fixtures.rawTarget.Runtime.SlotIndex,
                    frame = fixtures.rawTarget.Frame.N,
                    pn = fixtures.rawTarget.Frame.PN,
                    attacking = fixtures.rawTarget.AttackingCounter,
                    waitCounter = fixtures.rawTarget.Trans.WaitCounter,
                },
                randomWeaponBoundaryNoOp = true,
                hitPlanMode = diagnostics.Mode.ToString(),
                hitPlanComparisonAvailable = comparisonAvailable,
                hitPlanValid = !comparisonAvailable ||
                    diagnostics.CurrentTickPlanValid,
                hitPlanObservedCandidates = diagnostics.ObservedCandidateCount,
                hitPlanAbortTerminations = diagnostics.ObservedAbortTerminationCount,
                hitPlanMismatches = diagnostics.ObservationMismatchCount,
                passes = new[]
                {
                    new PassEvidence
                    {
                        pass = "collect",
                        candidateCount = totalCandidates,
                        characterHp = 5,
                        weaponHp = 100,
                        specialHp = 100,
                    },
                    new PassEvidence
                    {
                        pass = "character-consume",
                        candidateCount = totalCandidates,
                        characterHp = fixtures.characterVictim.Health.HP,
                        weaponHp = 100,
                        specialHp = 100,
                    },
                    new PassEvidence
                    {
                        pass = "object-consume",
                        candidateCount = totalCandidates,
                        characterHp = fixtures.characterVictim.Health.HP,
                        weaponHp = fixtures.weaponTarget.Health.HP,
                        specialHp = fixtures.specialTarget.Health.HP,
                    },
                },
            };
        }

        private static FixtureSet BuildFixtures()
        {
            var fixtures = new FixtureSet();
            int oid = ProbeOidBase;

            fixtures.characterHolder = RegisterOwned(new ProbeCharacter(
                "R8C04_CharacterHolder", oid++, null, false));
            fixtures.characterAttacker = RegisterOwned(new ProbeCharacter(
                "R8C04_CharacterAttacker", oid++, AttackItr(0, 10, 3, 0), false));
            fixtures.characterVictim = RegisterOwned(new ProbeCharacter(
                "R8C04_CharacterVictim", oid++, null, true));
            ConfigurePair(fixtures.characterAttacker, fixtures.characterVictim, 100000);
            fixtures.characterHolder.SetPosition(105000, 0, 0);
            fixtures.characterAttacker.HolderCopySlot =
                fixtures.characterHolder.Runtime.SlotIndex;
            fixtures.characterVictim.Health.HP = 5;
            fixtures.characterVictim.Health.HPBound = 100;
            fixtures.characterVictim.Health.HP3 = 100;
            fixtures.characterVictim.KillCount = -1;
            fixtures.characterVictim.Unk344 = 1;
            fixtures.characterVictim.FallDamageDiv = 100;

            fixtures.hitConfirmAttacker = RegisterOwned(new ProbeCharacter(
                "R8C04_HitConfirmAttacker", oid++, AttackItr(0, 10, 3, 0), false));
            fixtures.hitConfirmFirst = RegisterOwned(new ProbeCharacter(
                "R8C04_HitConfirmFirst", oid++, null, true));
            fixtures.hitConfirmSecond = RegisterOwned(new ProbeCharacter(
                "R8C04_HitConfirmSecond", oid++, null, true));
            ConfigureTriple(
                fixtures.hitConfirmAttacker,
                fixtures.hitConfirmFirst,
                fixtures.hitConfirmSecond,
                110000);

            fixtures.caughtAttacker = RegisterOwned(new ProbeCharacter(
                "R8C04_CaughtAttacker", oid++, AttackItr(0, 10, 3, 0), false));
            fixtures.caughtFirst = RegisterOwned(new ProbeCharacter(
                "R8C04_CaughtFirst", oid++, null, true, new CatchPoint { kind = 2 }));
            fixtures.caughtSecond = RegisterOwned(new ProbeCharacter(
                "R8C04_CaughtSecond", oid++, null, true));
            fixtures.caughtCatcher = RegisterOwned(new ProbeCharacter(
                "R8C04_CaughtCatcher",
                oid++,
                null,
                false,
                new CatchPoint { kind = 1, hurtable = 0 }));
            ConfigureTriple(
                fixtures.caughtAttacker,
                fixtures.caughtFirst,
                fixtures.caughtSecond,
                120000);
            fixtures.caughtCatcher.SetPosition(125000, 0, 0);
            fixtures.caughtFirst.CatcherSlotIndex =
                fixtures.caughtCatcher.Runtime.SlotIndex;
            fixtures.caughtCatcher.CaughtSlotIndex =
                fixtures.caughtAttacker.Runtime.SlotIndex;

            InteractionArea effectItr = AttackItr(0, 10, 3, 21);
            fixtures.effectAttacker = RegisterOwned(new ProbeCharacter(
                "R8C04_Effect21Attacker", oid++, effectItr, false));
            fixtures.effectFirst = RegisterOwned(new ProbeCharacter(
                "R8C04_Effect21First", oid++, null, true));
            fixtures.effectSecond = RegisterOwned(new ProbeCharacter(
                "R8C04_Effect21Second", oid++, null, true));
            ConfigureTriple(
                fixtures.effectAttacker,
                fixtures.effectFirst,
                fixtures.effectSecond,
                130000);

            fixtures.rawAttacker = RegisterOwned(new ProbeCharacter(
                "R8C04_RawKind10Attacker", oid++, AttackItr(10, 0, 3, 0), false));
            fixtures.rawTarget = RegisterOwned(new ProbeCharacter(
                "R8C04_RawKind10Target", oid++, null, true));
            ConfigurePair(fixtures.rawAttacker, fixtures.rawTarget, 140000);
            fixtures.rawTarget.Frame.PN = 41;
            fixtures.rawTarget.AttackingCounter = 9;
            fixtures.rawTarget.Trans.SetWait(fixtures.rawTarget.Frame.D.wait, 73);

            fixtures.weaponAttacker = RegisterOwned(new ProbeWeapon(
                "R8C04_WeaponAttacker",
                oid++,
                (int)LF2ObjectType.LightWeapon,
                AttackItr(0, 10, 3, 0),
                false));
            fixtures.weaponTarget = RegisterOwned(new ProbeWeapon(
                "R8C04_WeaponTarget",
                oid++,
                (int)LF2ObjectType.ThrowWeapon,
                null,
                true));
            ConfigurePair(fixtures.weaponAttacker, fixtures.weaponTarget, 150000);
            fixtures.weaponTarget.Health.HP = 100;
            fixtures.weaponTarget.Health.HPBound = 100;
            fixtures.weaponTarget.Health.HP3 = 100;
            fixtures.weaponTarget.FallDamageDiv = 50;
            fixtures.weaponTarget.Unk344 = 2;
            fixtures.weaponTarget.Runtime.WeaponFlightCounter = 100;

            fixtures.weaponDummyA = RegisterOwned(new ProbeWeapon(
                "R8C04_WeaponDummyA",
                oid++,
                (int)LF2ObjectType.LightWeapon,
                null,
                false));
            fixtures.weaponDummyB = RegisterOwned(new ProbeWeapon(
                "R8C04_WeaponDummyB",
                oid++,
                (int)LF2ObjectType.HeavyWeapon,
                null,
                false));
            fixtures.weaponDummyA.SetPosition(160000, 0, 0);
            fixtures.weaponDummyB.SetPosition(161000, 0, 0);

            fixtures.specialAttacker = RegisterOwned(new ProbeSpecialAttack(
                "R8C04_SpecialAttacker", oid++, AttackItr(0, 10, 3, 0), false));
            fixtures.specialTarget = RegisterOwned(new ProbeSpecialAttack(
                "R8C04_SpecialTarget", oid++, null, true));
            ConfigurePair(fixtures.specialAttacker, fixtures.specialTarget, 170000);
            fixtures.specialTarget.Health.HP = 100;
            fixtures.specialTarget.Health.HPBound = 100;
            fixtures.specialTarget.Health.HP3 = 100;
            fixtures.specialTarget.Unk344 = 1;

            return fixtures;
        }

        private static InteractionArea AttackItr(
            int kind,
            int injury,
            int vrest,
            int effect)
        {
            return new InteractionArea
            {
                kind = kind,
                x = -30,
                y = -10,
                w = 60,
                h = 20,
                zwidth = 15,
                injury = injury,
                fall = 10,
                dvx = 1,
                dvy = 0,
                arest = 2,
                vrest = vrest,
                effect = effect,
            };
        }

        private static void ConfigurePair(
            LF2Entity attacker,
            LF2Entity target,
            int x)
        {
            attacker.Team = 1;
            attacker.RelationTeam = 1;
            target.Team = 2;
            target.RelationTeam = 2;
            SetPosition(attacker, x, 0, 0);
            SetPosition(target, x + 10, 0, 0);
        }

        private static void SetPosition(LF2Entity entity, int x, int y, int z)
        {
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
        }

        private static void ConfigureTriple(
            ProbeCharacter attacker,
            ProbeCharacter first,
            ProbeCharacter second,
            int x)
        {
            ConfigurePair(attacker, first, x);
            second.Team = 3;
            second.RelationTeam = 3;
            second.SetPosition(x + 10, 0, 0);
        }

        private static int RequireCandidateOrder(
            LF2Entity attacker,
            params LF2Entity[] expectedTargets)
        {
            Require(
                world.SceneQuery.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange candidates),
                attacker.Name + " has no frozen candidate range");
            Require(candidates.Count == expectedTargets.Length,
                $"{attacker.Name} candidate count {candidates.Count} != " +
                expectedTargets.Length);
            for (int index = 0; index < expectedTargets.Length; index++)
            {
                Require(candidates.TryGet(index, out SceneQueryHit hit),
                    $"{attacker.Name} candidate {index} is unreadable");
                LF2Entity actual = hit.ResolveCurrentTarget(world);
                Require(ReferenceEquals(actual, expectedTargets[index]),
                    $"{attacker.Name} candidate {index} slot order mismatch: " +
                    $"actual={actual?.Runtime?.SlotIndex ?? -1}, " +
                    $"expected={expectedTargets[index].Runtime.SlotIndex}");
            }

            return candidates.Count;
        }

        private static void RequireBaselineHasNoCandidates()
        {
            for (int index = 0; index < BaselineEntities.Count; index++)
            {
                LF2Entity entity = BaselineEntities[index];
                if (entity?.Runtime == null)
                    continue;
                Require(entity.Runtime.HitCandidateCount == 0,
                    $"Baseline entity slot {entity.Runtime.SlotIndex} has a live candidate; " +
                    "the probe refuses to consume or mutate an active battle interaction.");
            }
        }

        private static T RegisterOwned<T>(T entity)
            where T : LF2Entity
        {
            world.Register(entity);
            Require(entity.Runtime?.SlotIndex >= 0,
                (entity?.Name ?? "entity") + " has no runtime slot");
            OwnedEntities.Add(entity);
            return entity;
        }

        private static void CaptureBaseline()
        {
            baselineCaptured = true;
            result.startTick = driver.CurrentTickIndex;
            result.baselineObjectCount = world.ObjectCount;
            result.baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            result.baselineObjectPoolActive = objectPool.ActiveObjectCountForAcceptance;
            result.baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineKillStats = CloneStats(world.KillStats);
            baselineDamageStats = CloneStats(world.DamageStats);
            baselineRngState = world.Rng.State;
            baselineRngCalls = world.Rng.CallCount;
            baselineHitPlanMode =
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics.Mode;
            BaselineSounds.Clear();
            BaselineSounds.AddRange(world.PendingSounds);
            BaselineEntities.Clear();
            for (int slot = 0;
                 slot < world.RuntimeSlotCapacityForDiagnostics;
                 slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity != null)
                    BaselineEntities.Add(entity);
            }
            if (BaselineEntities.Count > MaximumBaselineEntities)
            {
                throw new InvalidOperationException(
                    $"The live world has {BaselineEntities.Count} entities; " +
                    $"this certification probe is limited to {MaximumBaselineEntities} " +
                    "so it cannot snapshot baseline pair rests safely.");
            }

            BaselineRestEntries.Clear();
            for (int victimIndex = 0;
                 victimIndex < BaselineEntities.Count;
                 victimIndex++)
            {
                int victimSlot = BaselineEntities[victimIndex].Runtime.SlotIndex;
                for (int attackerIndex = 0;
                     attackerIndex < BaselineEntities.Count;
                     attackerIndex++)
                {
                    int attackerSlot =
                        BaselineEntities[attackerIndex].Runtime.SlotIndex;
                    if (victimSlot == attackerSlot)
                        continue;
                    BaselineRestEntries.Add(new RestEntry
                    {
                        victimSlot = victimSlot,
                        attackerSlot = attackerSlot,
                        value = world.GetRawRestVrest(victimSlot, attackerSlot),
                    });
                }
            }
        }

        private static void CleanupOwnedEntities()
        {
            if (world == null)
                return;
            try
            {
                world.EndCollisionCandidateConsumption();
            }
            catch (Exception exception)
            {
                AppendCleanupError("candidate-end", exception);
            }

            for (int index = OwnedEntities.Count - 1; index >= 0; index--)
            {
                LF2Entity entity = OwnedEntities[index];
                try
                {
                    if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                        world.Unregister(entity);
                }
                catch (Exception exception)
                {
                    AppendCleanupError(entity?.Name ?? "entity", exception);
                }
            }
            OwnedEntities.Clear();
            try
            {
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                AppendCleanupError("flush", exception);
            }

            RestoreStats(world.KillStats, baselineKillStats);
            RestoreStats(world.DamageStats, baselineDamageStats);
            world.PendingSounds.Clear();
            world.PendingSounds.AddRange(BaselineSounds);
            world.Rng.RestoreState(baselineRngState, baselineRngCalls);
            try
            {
                RuntimeRestStore store = world.RuntimeRestStoreForServices;
                for (int index = 0; index < BaselineRestEntries.Count; index++)
                {
                    RestEntry entry = BaselineRestEntries[index];
                    if (!store.SetVRest(
                            entry.victimSlot,
                            entry.attackerSlot,
                            entry.value))
                    {
                        result.cleanupErrors +=
                            $"rest:{entry.victimSlot}/{entry.attackerSlot};";
                    }
                }
            }
            catch (Exception exception)
            {
                AppendCleanupError("rest-restore", exception);
            }
        }

        private static void AppendCleanupError(string label, Exception exception)
        {
            if (result != null)
                result.cleanupErrors += label + ":" + exception.Message + ";";
        }

        private static void FinishSuccess()
        {
            result.status = "PASS";
            result.message =
                "Live collision collect, ordered hit consumption, damage/stat, " +
                "durability, vrest and abort matrices passed.";
            result.endTick = driver.CurrentTickIndex;
            result.producedSoundCount = world.PendingSounds.Count - BaselineSounds.Count;
            result.rngCallsDuringMatrix = world.Rng.CallCount - baselineRngCalls;
            CleanupOwnedEntities();
            CaptureFinalState();
            Require(result.cleanupCompleted,
                "Probe cleanup did not restore the live-world baseline: " +
                result.cleanupErrors);
            WriteResult(result);
            Debug.Log(
                $"[BattleCollisionHitDamagePlayModeProbe] PASS: " +
                $"tick={result.startTick}, candidates={result.matrix?.totalCandidates ?? 0}.");
            StopObservation();
        }

        private static void Fail(string message)
        {
            result ??= new ProbeResult();
            result.status = "FAIL";
            result.message = message;
            result.endTick = driver?.CurrentTickIndex ?? -1;
            CleanupOwnedEntities();
            CaptureFinalState();
            WriteResult(result);
            Debug.LogError("[BattleCollisionHitDamagePlayModeProbe] FAIL: " + message);
            StopObservation();
        }

        private static void CaptureFinalState()
        {
            if (result == null)
                return;
            result.finalObjectCount = world?.ObjectCount ?? -1;
            result.finalClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            result.finalObjectPoolActive = objectPool?.ActiveObjectCountForAcceptance ?? -1;
            result.finalLogicPoolActive = LF2ReferencePool.Instance?.ActiveCount ?? -1;
            result.globalStatsRestored =
                ArraysEqual(world?.KillStats, baselineKillStats) &&
                ArraysEqual(world?.DamageStats, baselineDamageStats);
            result.rngRestored = world?.Rng != null &&
                world.Rng.State == baselineRngState &&
                world.Rng.CallCount == baselineRngCalls;
            result.pendingSoundsRestored =
                PendingSoundsEqual(world?.PendingSounds, BaselineSounds);
            result.restsRestored = BaselineRestsEqual();
            result.hitPlanModeRestored = world != null &&
                world.BattleHitExecutionPlanDiagnosticsForDiagnostics.Mode ==
                baselineHitPlanMode;
            result.cleanupCompleted =
                baselineCaptured &&
                string.IsNullOrEmpty(result.cleanupErrors) &&
                result.finalObjectCount == result.baselineObjectCount &&
                result.finalClaimedSlots == result.baselineClaimedSlots &&
                result.finalObjectPoolActive == result.baselineObjectPoolActive &&
                result.finalLogicPoolActive == result.baselineLogicPoolActive &&
                result.globalStatsRestored &&
                result.rngRestored &&
                result.pendingSoundsRestored &&
                result.restsRestored &&
                result.hitPlanModeRestored;
        }

        private static bool BaselineRestsEqual()
        {
            if (world == null)
                return false;
            for (int index = 0; index < BaselineRestEntries.Count; index++)
            {
                RestEntry entry = BaselineRestEntries[index];
                if (world.GetRawRestVrest(
                        entry.victimSlot,
                        entry.attackerSlot) != entry.value)
                {
                    return false;
                }
            }
            return true;
        }

        private static int[] CloneStats(int[] source)
        {
            return source != null ? (int[])source.Clone() : Array.Empty<int>();
        }

        private static void RestoreStats(int[] destination, int[] source)
        {
            if (destination == null || source == null)
                return;
            Array.Copy(source, destination, Math.Min(source.Length, destination.Length));
        }

        private static bool ArraysEqual(int[] left, int[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            for (int index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                    return false;
            }
            return true;
        }

        private static bool PendingSoundsEqual(
            IList<PendingSoundEvent> left,
            IList<PendingSoundEvent> right)
        {
            if (left == null || right == null || left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
            {
                if (left[index].Cue != right[index].Cue ||
                    left[index].WorldX != right[index].WorldX ||
                    left[index].Tick != right[index].Tick)
                {
                    return false;
                }
            }
            return true;
        }

        private static string DescribeDiagnostics(
            BattleHitExecutionPlanDiagnostics diagnostics)
        {
            return
                $"mode={diagnostics.Mode}, tick={diagnostics.CapturedTick}, " +
                $"planned={diagnostics.PlannedCandidateCount}, " +
                $"observed={diagnostics.ObservedCandidateCount}, " +
                $"abort={diagnostics.ObservedAbortTerminationCount}, " +
                $"skipped={diagnostics.SkippedCandidateCountAfterAbort}, " +
                $"mismatch={diagnostics.ObservationMismatchCount}, " +
                $"failures={diagnostics.FailureCount}, " +
                $"first={diagnostics.FirstFailureReason}/" +
                $"{diagnostics.FirstFailureAttackerSlot}/" +
                $"{diagnostics.FirstFailureCandidateOrdinal}";
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeResult
            {
                status = "FAIL",
                message = message,
                startTick = driver?.CurrentTickIndex ?? -1,
                endTick = driver?.CurrentTickIndex ?? -1,
            });
            Debug.LogError(
                "[BattleCollisionHitDamagePlayModeProbe] FAIL: " + message);
        }

        private static void WriteResult(ProbeResult probeResult)
        {
            string path = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                ResultRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(probeResult, true));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            if (driver != null && EditorApplication.isPlaying)
                driver.SetPaused(previousPaused);
            running = false;
        }

        private static void ResetProbeState()
        {
            driver = null;
            world = null;
            objectPool = null;
            result = null;
            baselineKillStats = null;
            baselineDamageStats = null;
            baselineRngState = 0;
            baselineRngCalls = 0;
            baselineHitPlanMode = BattleHitExecutionPlanMode.Disabled;
            previousPaused = false;
            pauseRequested = false;
            baselineCaptured = false;
            running = false;
            editorUpdates = 0;
            OwnedEntities.Clear();
            BaselineEntities.Clear();
            BaselineRestEntries.Clear();
            BaselineSounds.Clear();
        }

        private sealed class ProbeCharacter : LF2Character
        {
            public ProbeCharacter(
                string name,
                int objectId,
                InteractionArea itr,
                bool hasBody,
                CatchPoint cpoint = null)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(
                    objectId,
                    BuildCharacterData(name, itr, hasBody, cpoint)));
                ImmediateFrame(0);
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 100;
                Health.HPBound = 100;
                Health.HP3 = 100;
                KillCount = -1;
            }

            public void SetPosition(int x, int y, int z)
            {
                Runtime.SetPosition(x, y, z);
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            public ProbeWeapon(
                string name,
                int objectId,
                int weaponType,
                InteractionArea itr,
                bool hasBody)
            {
                Name = name;
                ObjectId = objectId;
                SetWeaponType(weaponType);
                FrameCache.Load(new LF2CharacterDataWrapper(
                    objectId,
                    BuildWeaponData(name, weaponType, itr, hasBody)));
                ImmediateFrame(0);
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 100;
                Health.HPBound = 100;
                Health.HP3 = 100;
                KillCount = -1;
            }

            public void SetPosition(int x, int y, int z)
            {
                Runtime.SetPosition(x, y, z);
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }
        }

        private sealed class ProbeSpecialAttack : LF2SpecialAttack
        {
            public ProbeSpecialAttack(
                string name,
                int objectId,
                InteractionArea itr,
                bool hasBody)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(
                    objectId,
                    BuildSpecialData(name, itr, hasBody)));
                ImmediateFrame(0);
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 100;
                Health.HPBound = 100;
                Health.HP3 = 100;
                KillCount = -1;
            }

            public void SetPosition(int x, int y, int z)
            {
                Runtime.SetPosition(x, y, z);
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }
        }

        private static LF2CharacterData BuildCharacterData(
            string name,
            InteractionArea itr,
            bool hasBody,
            CatchPoint cpoint)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, LF2States.Standing, hasBody, itr, cpoint),
                Frame(18, LF2States.Burning, hasBody, null, cpoint),
                Frame(19, LF2States.FirenSpecific, hasBody, null, cpoint),
                Frame(20, LF2States.Standing, hasBody, null, cpoint),
                Frame(30, LF2States.Standing, hasBody, null, cpoint),
                Frame(35, LF2States.Falling, hasBody, null, cpoint),
                Frame(36, LF2States.Falling, hasBody, null, cpoint),
                Frame(180, LF2States.Falling, hasBody, null, cpoint),
                Frame(181, LF2States.Falling, hasBody, null, cpoint),
                Frame(182, LF2States.Falling, hasBody, null, cpoint),
                Frame(186, LF2States.Falling, hasBody, null, cpoint),
                Frame(200, LF2States.Falling, hasBody, null, cpoint),
                Frame(203, LF2States.Falling, hasBody, null, cpoint),
            };
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.Character,
                frames = frames,
            };
        }

        private static LF2CharacterData BuildWeaponData(
            string name,
            int weaponType,
            InteractionArea itr,
            bool hasBody)
        {
            var frames = new List<LF2FrameData>(227);
            for (int frameId = 0; frameId <= 226; frameId++)
            {
                frames.Add(Frame(
                    frameId,
                    LF2States.Standing,
                    hasBody && frameId == 0,
                    frameId == 0 ? itr : null,
                    null));
            }
            return new LF2CharacterData
            {
                name = name,
                type_sub = weaponType,
                weapon_hp = 100,
                weapon_drop_hurt = 3,
                frames = frames,
            };
        }

        private static LF2CharacterData BuildSpecialData(
            string name,
            InteractionArea itr,
            bool hasBody)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, LF2States.Standing, hasBody, itr, null),
                Frame(20, LF2States.Standing, false, null, null),
                Frame(30, LF2States.Standing, false, null, null),
                Frame(33, LF2States.Standing, false, null, null),
                Frame(40, LF2States.Standing, false, null, null),
                Frame(200, LF2States.Standing, false, null, null),
                Frame(203, LF2States.Standing, false, null, null),
            };
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = frames,
            };
        }

        private static LF2FrameData Frame(
            int frameId,
            int state,
            bool hasBody,
            InteractionArea itr,
            CatchPoint cpoint)
        {
            var frame = new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = frameId,
                pic = 999,
                centerx = 0,
                centery = 0,
                cpoint = cpoint,
            };
            if (hasBody)
            {
                frame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            }
            if (itr != null)
                frame.itrs.Add(itr);
            return frame;
        }

        private sealed class FixtureSet
        {
            public ProbeCharacter characterHolder;
            public ProbeCharacter characterAttacker;
            public ProbeCharacter characterVictim;
            public ProbeCharacter hitConfirmAttacker;
            public ProbeCharacter hitConfirmFirst;
            public ProbeCharacter hitConfirmSecond;
            public ProbeCharacter caughtAttacker;
            public ProbeCharacter caughtFirst;
            public ProbeCharacter caughtSecond;
            public ProbeCharacter caughtCatcher;
            public ProbeCharacter effectAttacker;
            public ProbeCharacter effectFirst;
            public ProbeCharacter effectSecond;
            public ProbeCharacter rawAttacker;
            public ProbeCharacter rawTarget;
            public ProbeWeapon weaponAttacker;
            public ProbeWeapon weaponTarget;
            public ProbeWeapon weaponDummyA;
            public ProbeWeapon weaponDummyB;
            public ProbeSpecialAttack specialAttacker;
            public ProbeSpecialAttack specialTarget;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public int startTick;
            public int endTick;
            public bool workerWasActive;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int baselineObjectPoolActive;
            public int baselineLogicPoolActive;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public int producedSoundCount;
            public ulong rngCallsDuringMatrix;
            public bool globalStatsRestored;
            public bool rngRestored;
            public bool pendingSoundsRestored;
            public bool restsRestored;
            public bool hitPlanModeRestored;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public MatrixEvidence matrix;
        }

        [Serializable]
        private sealed class MatrixEvidence
        {
            public int tick;
            public int totalCandidates;
            public HitEvidence character;
            public HitEvidence weapon;
            public HitEvidence special;
            public GateEvidence hitConfirmAbort;
            public GateEvidence caughtGate;
            public GateEvidence effect21Abort;
            public RawFrameEvidence rawFrame;
            public bool randomWeaponBoundaryNoOp;
            public string hitPlanMode = string.Empty;
            public bool hitPlanComparisonAvailable;
            public bool hitPlanValid;
            public long hitPlanObservedCandidates;
            public long hitPlanAbortTerminations;
            public long hitPlanMismatches;
            public PassEvidence[] passes = Array.Empty<PassEvidence>();
        }

        [Serializable]
        private sealed class HitEvidence
        {
            public int attackerSlot;
            public int targetSlot;
            public int hp;
            public int hpBound;
            public int combo;
            public int vrest;
            public int frame;
            public int durability;
            public int hitConfirm2;
        }

        [Serializable]
        private sealed class GateEvidence
        {
            public int attackerSlot;
            public int firstTargetSlot;
            public int secondTargetSlot;
            public int firstHp;
            public int secondHp;
            public bool attackerAborted;
            public bool firstSkippedOnly;
        }

        [Serializable]
        private sealed class RawFrameEvidence
        {
            public int attackerSlot;
            public int targetSlot;
            public int frame;
            public int pn;
            public int attacking;
            public int waitCounter;
        }

        [Serializable]
        private sealed class PassEvidence
        {
            public string pass = string.Empty;
            public int candidateCount;
            public int characterHp;
            public int weaponHp;
            public int specialHp;
        }

        private struct RestEntry
        {
            public int victimSlot;
            public int attackerSlot;
            public int value;
        }
    }
}
#endif
