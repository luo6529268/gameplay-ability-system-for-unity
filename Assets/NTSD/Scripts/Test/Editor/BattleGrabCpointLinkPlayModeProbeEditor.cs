#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Live-world joint S4 probe for grab, CPoint, held injury and link residue.
    /// Alignment contract: R8-GRABPLAY-001.
    /// </summary>
    public static class BattleGrabCpointLinkPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/验证/R8/运行抓取CPoint关系Play探针";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01C_03_GrabCpointLink.result.json";
        private const int TickTimeoutEditorUpdates = 1800;
        private const int ProbeDataIdBase = 7800;

        private static readonly List<LF2Entity> OwnedEntities =
            new List<LF2Entity>(16);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool objectPool;
        private static ProbeResult result;
        private static int[] baselineKillStats;
        private static int[] baselineDamageStats;
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
                RequireNeutralBaselineRelations();
                result.validGrab = RunValidGrabHeldInjuryMatrix();
                result.mismatchThrow = RunReciprocalMismatchThrowMatrix();
                result.escapeDirControl = RunEscapeDirControlMatrix();
                result.linkResidue = RunLinkResidueMatrix();
                FinishSuccess();
            }
            catch (Exception exception)
            {
                Fail("Unhandled probe exception: " + exception);
            }
        }

        private static ValidGrabEvidence RunValidGrabHeldInjuryMatrix()
        {
            ProbeCharacter statHolder = null;
            ProbeCharacter catcher = null;
            ProbeCharacter victim = null;
            int[] killBefore = CloneStats(world.KillStats);
            int[] damageBefore = CloneStats(world.DamageStats);
            try
            {
                statHolder = new ProbeCharacter(
                    "R8C03_StatHolder",
                    ProbeDataIdBase,
                    BuildNeutralData("R8C03_StatHolder"));
                CatchPoint catcherPoint = BuildCatcherPoint(
                    decrease: 1,
                    injury: 30,
                    vaction: 130,
                    cover: 11);
                catcher = new ProbeCharacter(
                    "R8C03_ValidCatcher",
                    ProbeDataIdBase + 1,
                    BuildCatcherData("R8C03_ValidCatcher", catcherPoint));
                victim = new ProbeCharacter(
                    "R8C03_ValidVictim",
                    ProbeDataIdBase + 2,
                    BuildVictimData("R8C03_ValidVictim"));
                RegisterOwned(statHolder);
                RegisterOwned(catcher);
                RegisterOwned(victim);

                catcher.SetPosition(100, -10, 200);
                victim.SetPosition(140, 0, 200);
                catcher.SwitchDir("right");
                victim.SwitchDir("left");
                catcher.SetCurrentAndPrev2(0, 0);
                victim.SetCurrentAndPrev2(0, 0);
                catcher.Runtime.FrameWaitCounter = 71;
                victim.Runtime.FrameWaitCounter = 73;

                var grabResolver = new LF2CharacterInteractionResolver(catcher);
                bool grabAccepted = grabResolver.TryApplyPreInteraction(
                    new InteractionArea
                    {
                        kind = 3,
                        catchingact = new[] { 100, 100 },
                        caughtact = new[] { 130, 130 },
                    },
                    victim);
                Require(grabAccepted, "valid kind3 grab was rejected");
                int catcherSlot = RequireSlot(catcher, "valid catcher");
                int victimSlot = RequireSlot(victim, "valid victim");
                int holderSlot = RequireSlot(statHolder, "stat holder");
                Require(
                    catcher.CaughtSlotIndex == victimSlot &&
                    victim.CatcherSlotIndex == catcherSlot &&
                    catcher.Runtime.CaughtDuration == 300,
                    "valid grab reciprocal relationship mismatch");

                catcher.SetPrev2(100);
                victim.SetPrev2(130);
                catcher.HolderCopySlot = holderSlot;
                catcher.AttackingCounter = 0;
                catcher.FrameDelay = 0;
                victim.FrameDelay = 0;
                victim.Health.HP = 20;
                victim.Health.HPBound = 100;
                victim.Health.HP3 = 100;
                victim.KillCount = -1;
                victim.Unk344 = 1;
                victim.FallDamageDiv = 100;
                statHolder.KillStat = 0;
                statHolder.ComboCountAtk = 0;
                catcher.RefreshRuntimeSnapshot();
                victim.RefreshRuntimeSnapshot();
                statHolder.RefreshRuntimeSnapshot();

                int expectedX;
                int expectedY;
                int expectedZ;
                ResolveHeldPositionExpected(
                    catcher,
                    victim,
                    catcherPoint,
                    out expectedX,
                    out expectedY,
                    out expectedZ);

                var rows = new List<PassEvidence>(4);
                int passTick = driver.CurrentTickIndex + 100;
                world.HeldObjectProcessAll(passTick);
                rows.Add(CapturePass("first-held", catcher, victim, statHolder));
                Require(victim.Health.HP == 20 && catcher.Runtime.CaughtDuration == 300,
                    "first-held ran CPoint injury or decrease too early");

                world.PreInteractionTickAll(passTick);
                rows.Add(CapturePass("cpoint-weapon-sync", catcher, victim, statHolder));
                Require(catcher.Runtime.CaughtDuration == 299,
                    "valid CPoint decrease did not run exactly once");
                Require(victim.Health.HP == -10 && victim.Health.HPBound == 90 &&
                        victim.ComboCountVic == 30,
                    "held injury HP/HPBound/combo mismatch");
                Require(catcher.AttackingCounter == 1 && catcher.FrameDelay == 2 &&
                        victim.FrameDelay == -3,
                    "weapon-sync injury phase fields mismatch");
                Require(statHolder.KillStat == 1 && statHolder.ComboCountAtk == 30,
                    "holder-local held injury statistics mismatch");
                Require(world.KillStats[1] == killBefore[1] + 1 &&
                        world.DamageStats[1] == damageBefore[1] + 30,
                    "world held injury statistics mismatch");
                Require(victim.Runtime.XInt == expectedX &&
                        victim.Runtime.YInt == expectedY &&
                        victim.Runtime.ZInt == expectedZ,
                    $"held CPoint position mismatch: " +
                    $"{victim.Runtime.XInt}/{victim.Runtime.YInt}/{victim.Runtime.ZInt} != " +
                    $"{expectedX}/{expectedY}/{expectedZ}");
                Require(catcher.Runtime.FrameWaitCounter == 71 &&
                        victim.Runtime.FrameWaitCounter == 73,
                    "CPoint raw frame writer changed FrameWaitCounter");

                world.ValidateHeldLinksAll(passTick);
                rows.Add(CapturePass("positive-link-validation", catcher, victim, statHolder));
                world.HeldObjectProcessAll(passTick);
                rows.Add(CapturePass("second-held", catcher, victim, statHolder));
                Require(victim.Health.HP == -10 && victim.ComboCountVic == 30 &&
                        statHolder.KillStat == 1 && statHolder.ComboCountAtk == 30 &&
                        world.KillStats[1] == killBefore[1] + 1 &&
                        world.DamageStats[1] == damageBefore[1] + 30,
                    "post-CPoint link/second-held duplicated held injury or stats");

                return new ValidGrabEvidence
                {
                    grabAccepted = true,
                    catcherSlot = catcherSlot,
                    victimSlot = victimSlot,
                    holderSlot = holderSlot,
                    reciprocalEstablished = true,
                    caughtDurationBefore = 300,
                    caughtDurationAfter = catcher.Runtime.CaughtDuration,
                    victimHpBefore = 20,
                    victimHpAfter = victim.Health.HP,
                    victimHpBoundAfter = victim.Health.HPBound,
                    victimComboAfter = victim.ComboCountVic,
                    holderKillAfter = statHolder.KillStat,
                    holderComboAfter = statHolder.ComboCountAtk,
                    killStatDelta = world.KillStats[1] - killBefore[1],
                    damageStatDelta = world.DamageStats[1] - damageBefore[1],
                    expectedX = expectedX,
                    expectedY = expectedY,
                    expectedZ = expectedZ,
                    actualX = victim.Runtime.XInt,
                    actualY = victim.Runtime.YInt,
                    actualZ = victim.Runtime.ZInt,
                    frameWaitPreserved = catcher.Runtime.FrameWaitCounter == 71 &&
                                         victim.Runtime.FrameWaitCounter == 73,
                    passes = rows.ToArray(),
                };
            }
            finally
            {
                RestoreStats(world.KillStats, killBefore);
                RestoreStats(world.DamageStats, damageBefore);
                UnregisterOwned(victim, "valid-victim");
                UnregisterOwned(catcher, "valid-catcher");
                UnregisterOwned(statHolder, "valid-holder");
            }
        }

        private static MismatchThrowEvidence RunReciprocalMismatchThrowMatrix()
        {
            ProbeCharacter catcher = null;
            ProbeCharacter victim = null;
            try
            {
                CatchPoint cpoint = BuildCatcherPoint(
                    decrease: 5,
                    injury: 0,
                    vaction: 132,
                    cover: 0);
                cpoint.aaction = 120;
                cpoint.throwvx = 8;
                cpoint.throwvy = -4;
                cpoint.throwvz = 3;
                cpoint.throwinjury = 7;
                catcher = new ProbeCharacter(
                    "R8C03_MismatchCatcher",
                    ProbeDataIdBase + 3,
                    BuildCatcherData("R8C03_MismatchCatcher", cpoint));
                victim = new ProbeCharacter(
                    "R8C03_MismatchVictim",
                    ProbeDataIdBase + 4,
                    BuildVictimData("R8C03_MismatchVictim"));
                RegisterOwned(catcher);
                RegisterOwned(victim);

                catcher.SetPosition(100, -10, 200);
                victim.SetPosition(160, 0, 200);
                catcher.SwitchDir("right");
                victim.SwitchDir("right");
                catcher.SetCurrentAndPrev2(100, 100);
                victim.SetCurrentAndPrev2(130, 130);
                catcher.CaughtSlotIndex = RequireSlot(victim, "mismatch victim");
                victim.CatcherSlotIndex = -1;
                catcher.Runtime.CaughtDuration = 9;
                catcher.Runtime.KeyJump = 1;
                catcher.Runtime.CdAttack = 1;
                catcher.Runtime.KeyUp = 1;
                catcher.AttackingCounter = 2;
                catcher.Runtime.FrameWaitCounter = 81;
                victim.Runtime.FrameWaitCounter = 83;
                catcher.RefreshRuntimeSnapshot();
                victim.RefreshRuntimeSnapshot();

                int passTick = driver.CurrentTickIndex + 200;
                world.PreInteractionTickAll(passTick);
                Require(catcher.Frame.N == 110 && catcher.Frame.Prev2 == 110,
                    $"mismatch fallback throw source/next mismatch: " +
                    $"frame={catcher.Frame.N}, prev2={catcher.Frame.Prev2}");
                Require(victim.Frame.N == 132 && victim.Frame.Prev2 == 132,
                    "mismatch throw victim action/prev2 mismatch");
                Require(catcher.Runtime.CaughtDuration == 9,
                    "mismatch branch incorrectly ran decrease");
                Require(Nearly(victim.Runtime.Vx, 8.0) &&
                        Nearly(victim.Runtime.Vy, -4.0) &&
                        Nearly(victim.Runtime.Vz, -3.0),
                    "mismatch throw velocity mismatch");
                Require(victim.Runtime.XInt == 140 && victim.Runtime.YInt == 30,
                    $"mismatch fallback frame0 geometry mismatch: " +
                    $"{victim.Runtime.XInt}/{victim.Runtime.YInt}");
                Require(catcher.AttackingCounter == 0 && victim.WeaponCount == 7,
                    "mismatch throw tail side-effect mismatch");
                Require(catcher.Runtime.FrameWaitCounter == 81 &&
                        victim.Runtime.FrameWaitCounter == 83,
                    "mismatch throw changed FrameWaitCounter");

                return new MismatchThrowEvidence
                {
                    catcherSlot = RequireSlot(catcher, "mismatch catcher"),
                    victimSlot = RequireSlot(victim, "mismatch victim"),
                    reciprocalMismatch = true,
                    catcherFrame = catcher.Frame.N,
                    catcherPrev2 = catcher.Frame.Prev2,
                    victimFrame = victim.Frame.N,
                    victimPrev2 = victim.Frame.Prev2,
                    caughtDuration = catcher.Runtime.CaughtDuration,
                    victimX = victim.Runtime.XInt,
                    victimY = victim.Runtime.YInt,
                    victimVx = victim.Runtime.Vx,
                    victimVy = victim.Runtime.Vy,
                    victimVz = victim.Runtime.Vz,
                    actionSkipped = catcher.Frame.N != 120,
                    throwTailRan = catcher.Frame.N == 110 && victim.Frame.N == 132,
                    frameWaitPreserved = catcher.Runtime.FrameWaitCounter == 81 &&
                                         victim.Runtime.FrameWaitCounter == 83,
                };
            }
            finally
            {
                UnregisterOwned(victim, "mismatch-victim");
                UnregisterOwned(catcher, "mismatch-catcher");
            }
        }

        private static EscapeDirControlEvidence RunEscapeDirControlMatrix()
        {
            ProbeCharacter catcher = null;
            ProbeCharacter victim = null;
            try
            {
                CatchPoint cpoint = BuildCatcherPoint(
                    decrease: -5,
                    injury: 0,
                    vaction: 130,
                    cover: 0);
                cpoint.aaction = 120;
                cpoint.dircontrol = 1;
                catcher = new ProbeCharacter(
                    "R8C03_EscapeCatcher",
                    ProbeDataIdBase + 5,
                    BuildCatcherData("R8C03_EscapeCatcher", cpoint));
                victim = new ProbeCharacter(
                    "R8C03_EscapeVictim",
                    ProbeDataIdBase + 6,
                    BuildVictimData("R8C03_EscapeVictim"));
                RegisterOwned(catcher);
                RegisterOwned(victim);

                catcher.SetPosition(100, -10, 200);
                victim.SetPosition(140, 0, 200);
                catcher.SwitchDir("left");
                victim.SwitchDir("right");
                catcher.SetCurrentAndPrev2(100, 100);
                victim.SetCurrentAndPrev2(130, 130);
                int catcherSlot = RequireSlot(catcher, "escape catcher");
                int victimSlot = RequireSlot(victim, "escape victim");
                catcher.CaughtSlotIndex = victimSlot;
                victim.CatcherSlotIndex = catcherSlot;
                catcher.Runtime.CaughtDuration = 2;
                catcher.AttackingCounter = 2;
                catcher.Runtime.KeyRight = 1;
                catcher.Runtime.KeyLeft = 0;
                catcher.Runtime.KeyJump = 1;
                catcher.Runtime.CdAttack = 1;
                catcher.Runtime.FrameWaitCounter = 91;
                victim.Runtime.FrameWaitCounter = 93;
                catcher.RefreshRuntimeSnapshot();
                victim.RefreshRuntimeSnapshot();

                int passTick = driver.CurrentTickIndex + 300;
                world.PreInteractionTickAll(passTick);
                int immediateVictimHitCount = victim.HitCount;
                Require(catcher.Frame.N == 0 && victim.Frame.N == 181,
                    "negative-duration escape frame mismatch");
                Require(catcher.Runtime.CaughtDuration == -3 &&
                        catcher.Runtime.Dir == "right",
                    "negative-duration escape/dircontrol tail mismatch");
                Require(catcher.HitCount == 1 && victim.HitCount == 1 &&
                        Nearly(victim.KnockbackVx, 4.0) &&
                        Nearly(victim.KnockbackVy, -3.0),
                    "negative-duration escape hit/knockback mismatch");
                Require(catcher.Frame.N != 120,
                    "negative-duration escape incorrectly ran action selection");
                Require(catcher.Runtime.FrameWaitCounter == 91 &&
                        victim.Runtime.FrameWaitCounter == 93,
                    "negative-duration escape changed FrameWaitCounter");

                double immediateKnockbackVx = victim.KnockbackVx;
                double immediateKnockbackVy = victim.KnockbackVy;

                world.RunBattleEcsFramePostProcessPass();
                Require(catcher.HitCount == 0 && victim.HitCount == 0 &&
                        Nearly(victim.Runtime.Vx, 4.0) &&
                        Nearly(victim.Runtime.Vy, -3.0),
                    "FramePostProcess did not consume escape hit/knockback state");

                return new EscapeDirControlEvidence
                {
                    catcherSlot = catcherSlot,
                    victimSlot = victimSlot,
                    catcherFrame = catcher.Frame.N,
                    victimFrame = victim.Frame.N,
                    caughtDuration = catcher.Runtime.CaughtDuration,
                    directionAfterTail = catcher.Runtime.Dir,
                    immediateVictimHitCount = immediateVictimHitCount,
                    postProcessVictimHitCount = victim.HitCount,
                    knockbackVx = immediateKnockbackVx,
                    knockbackVy = immediateKnockbackVy,
                    runtimeVxAfterPost = victim.Runtime.Vx,
                    runtimeVyAfterPost = victim.Runtime.Vy,
                    actionSkipped = catcher.Frame.N != 120,
                    frameWaitPreserved = catcher.Runtime.FrameWaitCounter == 91 &&
                                         victim.Runtime.FrameWaitCounter == 93,
                };
            }
            finally
            {
                UnregisterOwned(victim, "escape-victim");
                UnregisterOwned(catcher, "escape-catcher");
            }
        }

        private static LinkResidueEvidence RunLinkResidueMatrix()
        {
            ProbeCharacter positiveHolder = null;
            ProbeCharacter positiveTarget = null;
            ProbeCharacter negativeHolder = null;
            ProbeCharacter negativeChild = null;
            try
            {
                positiveHolder = NewNeutral("R8C03_PositiveHolder", 7);
                positiveTarget = NewNeutral("R8C03_PositiveTarget", 8);
                negativeHolder = NewNeutral("R8C03_NegativeHolder", 9);
                negativeChild = NewNeutral("R8C03_NegativeChild", 10);
                RegisterOwned(positiveHolder);
                RegisterOwned(positiveTarget);
                RegisterOwned(negativeHolder);
                RegisterOwned(negativeChild);

                int positiveHolderSlot = RequireSlot(positiveHolder, "positive holder");
                int positiveTargetSlot = RequireSlot(positiveTarget, "positive target");
                positiveHolder.Runtime.LinkState = 5;
                positiveHolder.Runtime.TargetSlotIndex = positiveTargetSlot;
                positiveHolder.Runtime.HeldWeaponStableId = positiveTargetSlot;
                positiveTarget.Runtime.LinkState = -5;
                positiveTarget.Runtime.HolderStableId = -1;
                positiveHolder.RefreshRuntimeSnapshot();
                positiveTarget.RefreshRuntimeSnapshot();

                int passTick = driver.CurrentTickIndex + 400;
                world.ValidateHeldLinksAll(passTick);
                Require(positiveHolder.Runtime.LinkState == 0 &&
                        positiveHolder.Runtime.TargetSlotIndex == positiveTargetSlot &&
                        positiveHolder.Runtime.HeldWeaponStableId == positiveTargetSlot &&
                        positiveTarget.Runtime.HolderStableId == -1 &&
                        positiveTarget.Runtime.LinkState == -5,
                    "invalid positive link did not preserve forward/reverse residue");
                int positiveLinkAfter = positiveHolder.Runtime.LinkState;
                int positiveTargetSlotAfter = positiveHolder.Runtime.TargetSlotIndex;
                int positiveHeldSlotAfter = positiveHolder.Runtime.HeldWeaponStableId;
                int positiveReverseHolderAfter = positiveTarget.Runtime.HolderStableId;
                int positiveTargetLinkAfter = positiveTarget.Runtime.LinkState;

                int negativeHolderSlot = RequireSlot(negativeHolder, "negative holder");
                int negativeChildSlot = RequireSlot(negativeChild, "negative child");
                negativeHolder.Runtime.TargetSlotIndex = -1;
                negativeChild.Runtime.LinkState = -4;
                negativeChild.Runtime.HolderStableId = negativeHolderSlot;
                negativeHolder.RefreshRuntimeSnapshot();
                negativeChild.RefreshRuntimeSnapshot();
                world.HeldObjectProcessAll(passTick);
                int holderAfterFirst = negativeChild.Runtime.HolderStableId;
                world.HeldObjectProcessAll(passTick);
                Require(negativeChild.Runtime.LinkState == 0 &&
                        holderAfterFirst == negativeHolderSlot &&
                        negativeChild.Runtime.HolderStableId == negativeHolderSlot &&
                        negativeHolder.Runtime.TargetSlotIndex == -1,
                    "invalid negative link did not preserve HolderStableId across both held scans");

                return new LinkResidueEvidence
                {
                    positiveHolderSlot = positiveHolderSlot,
                    positiveTargetSlot = positiveTargetSlot,
                    positiveLinkAfter = positiveLinkAfter,
                    positiveTargetSlotAfter = positiveTargetSlotAfter,
                    positiveHeldSlotAfter = positiveHeldSlotAfter,
                    positiveReverseHolderAfter = positiveReverseHolderAfter,
                    positiveTargetLinkAfter = positiveTargetLinkAfter,
                    negativeHolderSlot = negativeHolderSlot,
                    negativeChildSlot = negativeChildSlot,
                    negativeLinkAfterFirst = 0,
                    negativeHolderAfterFirst = holderAfterFirst,
                    negativeLinkAfterSecond = negativeChild.Runtime.LinkState,
                    negativeHolderAfterSecond = negativeChild.Runtime.HolderStableId,
                };
            }
            finally
            {
                UnregisterOwned(negativeChild, "negative-child");
                UnregisterOwned(negativeHolder, "negative-holder");
                UnregisterOwned(positiveTarget, "positive-target");
                UnregisterOwned(positiveHolder, "positive-holder");
            }
        }

        private static PassEvidence CapturePass(
            string pass,
            ProbeCharacter catcher,
            ProbeCharacter victim,
            ProbeCharacter holder)
        {
            return new PassEvidence
            {
                pass = pass,
                catcherFrame = catcher.Frame?.N ?? -1,
                victimFrame = victim.Frame?.N ?? -1,
                caughtDuration = catcher.Runtime.CaughtDuration,
                victimHp = victim.Health?.HP ?? 0,
                victimHpBound = victim.Health?.HPBound ?? 0,
                victimCombo = victim.ComboCountVic,
                catcherAttacking = catcher.AttackingCounter,
                holderKill = holder.KillStat,
                holderCombo = holder.ComboCountAtk,
                worldKillStat1 = world.KillStats[1],
                worldDamageStat1 = world.DamageStats[1],
            };
        }

        private static void ResolveHeldPositionExpected(
            ProbeCharacter catcher,
            ProbeCharacter victim,
            CatchPoint cpoint,
            out int x,
            out int y,
            out int z)
        {
            LF2FrameData catcherFrame = catcher.Frame.D;
            LF2FrameData victimFrame = victim.Frame.D;
            int dx = catcher.Runtime.Dir == "right"
                ? catcher.Runtime.XInt - catcherFrame.centerx + cpoint.x
                : catcherFrame.centerx - cpoint.x + catcher.Runtime.XInt;
            int dy = catcher.Runtime.YInt - catcherFrame.centery + cpoint.y;
            x = victim.Runtime.Dir == "right"
                ? victimFrame.centerx - victimFrame.cpoint.x + dx
                : victimFrame.cpoint.x - victimFrame.centerx + dx;
            y = victimFrame.centery - victimFrame.cpoint.y + dy;
            z = catcher.Runtime.ZInt;
            if (cpoint.cover % 10 != 0)
            {
                z++;
                y--;
            }
            else
            {
                z--;
                y++;
            }
        }

        private static CatchPoint BuildCatcherPoint(
            int decrease,
            int injury,
            int vaction,
            int cover)
        {
            return new CatchPoint
            {
                kind = 1,
                x = 50,
                y = 60,
                decrease = decrease,
                injury = injury,
                vaction = vaction,
                hurtable = 0,
                cover = cover,
            };
        }

        private static LF2CharacterData BuildCatcherData(
            string name,
            CatchPoint cpoint)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 110, null, 10, 20),
                    Frame(100, LF2States.Catching, 100, cpoint, 39, 79),
                    Frame(110, LF2States.Standing, 110, null, 39, 79),
                    Frame(120, LF2States.Catching, 120, cpoint, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildVictimData(string name)
        {
            CatchPoint victimPoint = new CatchPoint
            {
                kind = 2,
                x = 20,
                y = 30,
            };
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, null, 39, 79),
                    Frame(130, LF2States.BeingCaught, 130, victimPoint, 39, 79),
                    Frame(132, LF2States.BeingCaught, 132, null, 39, 79),
                    Frame(181, LF2States.Falling, 181, null, 39, 79),
                    Frame(212, LF2States.Falling, 212, null, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildNeutralData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, null, 39, 79),
                },
            };
        }

        private static LF2FrameData Frame(
            int frameId,
            int state,
            int next,
            CatchPoint cpoint,
            int centerX,
            int centerY)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = next,
                pic = 999,
                centerx = centerX,
                centery = centerY,
                cpoint = cpoint,
            };
        }

        private static ProbeCharacter NewNeutral(string name, int offset)
        {
            return new ProbeCharacter(
                name,
                ProbeDataIdBase + offset,
                BuildNeutralData(name));
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
        }

        private static void RequireNeutralBaselineRelations()
        {
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null)
                    continue;
                if (entity.Runtime.LinkState != 0 ||
                    entity.CaughtSlotIndex >= 0 ||
                    entity.CatcherSlotIndex >= 0)
                {
                    throw new InvalidOperationException(
                        $"Live baseline relation is not neutral at slot {slot}: " +
                        $"link={entity.Runtime.LinkState}, caught={entity.CaughtSlotIndex}, " +
                        $"catcher={entity.CatcherSlotIndex}.");
                }
            }
        }

        private static void RegisterOwned(ProbeCharacter entity)
        {
            world.Register(entity);
            RequireSlot(entity, entity?.Name ?? "entity");
            OwnedEntities.Add(entity);
        }

        private static int RequireSlot(LF2Entity entity, string label)
        {
            int slot = entity?.Runtime?.SlotIndex ?? -1;
            if (slot < 0)
                throw new InvalidOperationException(label + " has no runtime slot.");
            return slot;
        }

        private static void UnregisterOwned(LF2Entity entity, string label)
        {
            if (entity == null)
                return;
            OwnedEntities.Remove(entity);
            if (entity.Match != world || entity.Runtime?.SlotIndex < 0)
                return;
            try
            {
                world.Unregister(entity);
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                if (result != null)
                    result.cleanupErrors += label + ":" + exception.Message + ";";
            }
        }

        private static void CleanupOwnedEntities()
        {
            if (world == null)
                return;
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
                    result.cleanupErrors +=
                        (entity?.Name ?? "entity") + ":" + exception.Message + ";";
                }
            }
            OwnedEntities.Clear();
            try
            {
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                result.cleanupErrors += "flush:" + exception.Message + ";";
            }
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

        private static void FinishSuccess()
        {
            result.status = "PASS";
            result.message =
                "Live grab, CPoint/weapon-sync, escape/throw tails and link residue passed.";
            result.endTick = driver.CurrentTickIndex;
            CleanupOwnedEntities();
            RestoreStats(world.KillStats, baselineKillStats);
            RestoreStats(world.DamageStats, baselineDamageStats);
            CaptureFinalState();
            Require(result.cleanupCompleted,
                "Probe cleanup did not restore the live-world baseline: " +
                result.cleanupErrors);
            WriteResult(result);
            Debug.Log(
                $"[BattleGrabCpointLinkPlayModeProbe] PASS: tick={result.startTick}, " +
                $"passes={result.validGrab?.passes?.Length ?? 0}.");
            StopObservation();
        }

        private static void Fail(string message)
        {
            result ??= new ProbeResult();
            result.status = "FAIL";
            result.message = message;
            result.endTick = driver?.CurrentTickIndex ?? -1;
            CleanupOwnedEntities();
            if (world != null)
            {
                RestoreStats(world.KillStats, baselineKillStats);
                RestoreStats(world.DamageStats, baselineDamageStats);
            }
            CaptureFinalState();
            WriteResult(result);
            Debug.LogError("[BattleGrabCpointLinkPlayModeProbe] FAIL: " + message);
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
            result.cleanupCompleted =
                baselineCaptured &&
                string.IsNullOrEmpty(result.cleanupErrors) &&
                result.finalObjectCount == result.baselineObjectCount &&
                result.finalClaimedSlots == result.baselineClaimedSlots &&
                result.finalObjectPoolActive == result.baselineObjectPoolActive &&
                result.finalLogicPoolActive == result.baselineLogicPoolActive &&
                result.globalStatsRestored;
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

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static bool Nearly(double actual, double expected)
        {
            return Math.Abs(actual - expected) <= 0.0001;
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
            Debug.LogError("[BattleGrabCpointLinkPlayModeProbe] FAIL: " + message);
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
            previousPaused = false;
            pauseRequested = false;
            baselineCaptured = false;
            running = false;
            editorUpdates = 0;
            OwnedEntities.Clear();
        }

        private sealed class ProbeCharacter : LF2Character
        {
            public ProbeCharacter(string probeName, int dataId, LF2CharacterData data)
            {
                Name = probeName;
                ObjectId = dataId;
                FrameCache.Load(new LF2CharacterDataWrapper(dataId, data));
                ImmediateFrame(0);
                Runtime.SetPosition(0.0, 0.0, 0.0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 500;
                Health.HPBound = 500;
                Health.HP3 = 500;
                KillCount = -1;
            }

            public void SetPosition(int x, int y, int z)
            {
                Runtime.SetPosition(x, y, z);
                Runtime.SyncIntegerPosition();
            }

            public void SetCurrentAndPrev2(int current, int prev2)
            {
                ImmediateFrame(current);
                SetCpointRawPrevFrame2(prev2);
            }

            public void SetPrev2(int frameId)
            {
                SetCpointRawPrevFrame2(frameId);
            }
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
            public bool globalStatsRestored;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public ValidGrabEvidence validGrab;
            public MismatchThrowEvidence mismatchThrow;
            public EscapeDirControlEvidence escapeDirControl;
            public LinkResidueEvidence linkResidue;
        }

        [Serializable]
        private sealed class PassEvidence
        {
            public string pass = string.Empty;
            public int catcherFrame;
            public int victimFrame;
            public int caughtDuration;
            public int victimHp;
            public int victimHpBound;
            public int victimCombo;
            public int catcherAttacking;
            public int holderKill;
            public int holderCombo;
            public int worldKillStat1;
            public int worldDamageStat1;
        }

        [Serializable]
        private sealed class ValidGrabEvidence
        {
            public bool grabAccepted;
            public int catcherSlot;
            public int victimSlot;
            public int holderSlot;
            public bool reciprocalEstablished;
            public int caughtDurationBefore;
            public int caughtDurationAfter;
            public int victimHpBefore;
            public int victimHpAfter;
            public int victimHpBoundAfter;
            public int victimComboAfter;
            public int holderKillAfter;
            public int holderComboAfter;
            public int killStatDelta;
            public int damageStatDelta;
            public int expectedX;
            public int expectedY;
            public int expectedZ;
            public int actualX;
            public int actualY;
            public int actualZ;
            public bool frameWaitPreserved;
            public PassEvidence[] passes = Array.Empty<PassEvidence>();
        }

        [Serializable]
        private sealed class MismatchThrowEvidence
        {
            public int catcherSlot;
            public int victimSlot;
            public bool reciprocalMismatch;
            public int catcherFrame;
            public int catcherPrev2;
            public int victimFrame;
            public int victimPrev2;
            public int caughtDuration;
            public int victimX;
            public int victimY;
            public double victimVx;
            public double victimVy;
            public double victimVz;
            public bool actionSkipped;
            public bool throwTailRan;
            public bool frameWaitPreserved;
        }

        [Serializable]
        private sealed class EscapeDirControlEvidence
        {
            public int catcherSlot;
            public int victimSlot;
            public int catcherFrame;
            public int victimFrame;
            public int caughtDuration;
            public string directionAfterTail = string.Empty;
            public int immediateVictimHitCount;
            public int postProcessVictimHitCount;
            public double knockbackVx;
            public double knockbackVy;
            public double runtimeVxAfterPost;
            public double runtimeVyAfterPost;
            public bool actionSkipped;
            public bool frameWaitPreserved;
        }

        [Serializable]
        private sealed class LinkResidueEvidence
        {
            public int positiveHolderSlot;
            public int positiveTargetSlot;
            public int positiveLinkAfter;
            public int positiveTargetSlotAfter;
            public int positiveHeldSlotAfter;
            public int positiveReverseHolderAfter;
            public int positiveTargetLinkAfter;
            public int negativeHolderSlot;
            public int negativeChildSlot;
            public int negativeLinkAfterFirst;
            public int negativeHolderAfterFirst;
            public int negativeLinkAfterSecond;
            public int negativeHolderAfterSecond;
        }
    }
}
#endif
