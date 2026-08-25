#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Input;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Production-catalog/full-tick witness for OID7/8 merge, dormant ownership,
    /// canonical DJA release, OID51 split, and CentralOnly visibility.
    /// Alignment contract: R8-MERGESPLIT-001.
    /// </summary>
    public static class BattleOid5152MergeSplitPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run OID7-8-51 Merge Split Play Probe";
        private const string RequestRelativePath =
            "Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.request";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.result.json";
        private const int TimeoutEditorUpdates = 12000;
        private const int CooldownTicksPerEditorUpdate = 32;
        private const int SelfOid = 7;
        private const int PartnerOid = 8;
        private const int MergedOid = 51;
        private const int SelfFrame = 9;
        private const int SplitFrame = 112;
        private const int SplitObservedFrame = SplitFrame + 1;
        private const int MergedFrame = 290;
        private const int RelationTeam = 5152;
        private const int FixtureX = 520;
        private const int FixtureSelfVz = 1;

        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(16);
        private static readonly HashSet<RuntimeEntityHandle> BaselineHandles =
            new HashSet<RuntimeEntityHandle>();
        private static readonly List<LF2Entity> EntityScratch =
            new List<LF2Entity>(64);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool objectPool;
        private static CharacterAnimtorManager characterManager;
        private static LF2Character self;
        private static LF2Character partner;
        private static RuntimeEntityHandle selfHandle;
        private static RuntimeEntityHandle partnerHandle;
        private static ProbeReport report;
        private static RosterSnapshot rosterSnapshot;
        private static ProbePhase phase;
        private static int editorUpdates;
        private static int expectedTick;
        private static int completionEditorUpdate;
        private static int stableTick;
        private static int stableUpdates;
        private static int fixtureZ;
        private static SimulationInputButtons queuedPhysicalButtons;
        private static int oid51HitJa;
        private static int cooldownTicksAdvanced;
        private static int baselineObjectCount;
        private static int baselineClaimedSlots;
        private static int baselineObjectPoolActive;
        private static int baselineLogicPoolActive;
        private static int[] baselineKillStats;
        private static int[] baselineDamageStats;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;
        private static BattleStructuralWriterDiagnostics structuralBefore;
        private static BattleStructuralWriterDiagnostics structuralBeforeSplit;
        private static bool previousPaused;
        private static bool workerPath;
        private static bool pauseRequested;
        private static bool baselineCaptured;
        private static bool running;
        private static bool requestForcedDriverUnpause;
        private static string lastCatalogReadinessFailure;

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || running)
            {
                return;
            }

            if (EditorApplication.isPaused)
            {
                EditorApplication.isPaused = false;
                return;
            }

            SimulationTickDriver currentDriver = SimulationTickDriver.Instance;
            if (currentDriver?.World == null)
                return;
            if (currentDriver.IsPaused)
            {
                requestForcedDriverUnpause = true;
                currentDriver.SetPaused(false);
                return;
            }
            if (currentDriver.CurrentTickIndex < 5)
                return;

            string requestPath = ProjectPath(RequestRelativePath);
            if (!File.Exists(requestPath))
                return;

            File.Delete(requestPath);
            RunFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            bool restoreDriverPause = requestForcedDriverUnpause;
            StopObservation();
            ResetState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            objectPool = LF2ObjectPool.Instance;
            characterManager = CharacterAnimtorManager.Instance;
            if (driver == null || world == null || objectPool == null ||
                characterManager == null || LF2ReferencePool.Instance == null ||
                GameDataManager.Instance == null)
            {
                WriteImmediateFailure("Production driver, world, catalog, or pools are unavailable.");
                return;
            }
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly)
            {
                WriteImmediateFailure("R08 requires the protected CentralOnly backend.");
                return;
            }

            previousPaused = restoreDriverPause || driver.IsPaused;
            requestForcedDriverUnpause = false;
            workerPath = driver.DedicatedSimulationWorkerActiveForDiagnostics;
            report = new ProbeReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                workerPath = workerPath,
            };
            phase = ProbePhase.WaitingForSafeBoundary;
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
            if (++editorUpdates > TimeoutEditorUpdates)
            {
                Fail("Timed out while waiting for the R08 full-tick witness.");
                return;
            }
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Fail("Production worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }

            // A different Editor-only request poller may transiently clear the
            // driver pause even when it has no request. This probe owns the safe
            // boundary while running and was registered after those pollers.
            if (pauseRequested && !driver.IsPaused)
                driver.SetPaused(true);

            try
            {
                switch (phase)
                {
                    case ProbePhase.WaitingForSafeBoundary:
                        TryStart();
                        break;
                    case ProbePhase.WaitingForMergeTick:
                        ObserveMergeTick();
                        break;
                    case ProbePhase.WaitingForMergePlan:
                        ObserveMergePlan();
                        break;
                    case ProbePhase.WaitingForDefendTick:
                        ObserveComboTick(1, FuncKeyMask.def, ProbePhase.WaitingForJumpTick);
                        break;
                    case ProbePhase.WaitingForJumpTick:
                        ObserveComboTick(2, FuncKeyMask.jump, ProbePhase.WaitingForAttackTick);
                        break;
                    case ProbePhase.WaitingForAttackTick:
                        ObserveAttackTick();
                        break;
                    case ProbePhase.WaitingForCooldownDrain:
                        AdvanceCooldownDrain();
                        break;
                    case ProbePhase.WaitingForSplitTick:
                        ObserveSplitTick();
                        break;
                    case ProbePhase.WaitingForSplitPlan:
                        ObserveSplitPlan();
                        break;
                }
            }
            catch (Exception exception)
            {
                Fail("Unhandled R08 probe exception: " + exception);
            }
        }

        private static void TryStart()
        {
            if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0)
                return;

            string catalogFailure = ValidateProductionCatalog();
            if (!string.IsNullOrEmpty(catalogFailure))
            {
                if (!string.Equals(
                        lastCatalogReadinessFailure,
                        catalogFailure,
                        StringComparison.Ordinal))
                {
                    lastCatalogReadinessFailure = catalogFailure;
                    Debug.LogWarning(
                        "[BattleOid5152MergeSplitProbe] Waiting for catalog readiness: " +
                        catalogFailure);
                }
                return;
            }
            lastCatalogReadinessFailure = string.Empty;

            if (!pauseRequested)
            {
                driver.SetPaused(true);
                pauseRequested = true;
                return;
            }
            if (!driver.IsPaused || driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
            {
                stableUpdates = 0;
                return;
            }
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 4)
                return;

            CaptureBaseline();
            BuildFixture();
            report.beforeMerge = CaptureState("before-merge", self);
            ScheduleTick(ProbePhase.WaitingForMergeTick, true);
        }

        private static string ValidateProductionCatalog()
        {
            int[] required = { SelfOid, PartnerOid, MergedOid };
            for (int index = 0; index < required.Length; index++)
            {
                int oid = required[index];
                ObjectDefinition definition = GameDataManager.Instance.GetObjectById(oid);
                LF2CharacterDataWrapper wrapper = characterManager.GetCharacterConfig(oid);
                if (definition == null || wrapper?.characterData?.frames == null)
                    return $"Production catalog is missing OID{oid}.";
            }

            LF2FrameData frame7 = characterManager.GetCharacterConfig(SelfOid).characterData
                .frames.Find(value => value != null && value.frameId == SelfFrame);
            LF2FrameData frame8 = characterManager.GetCharacterConfig(PartnerOid).characterData
                .frames.Find(value => value != null && value.frameId == SelfFrame);
            LF2FrameData frame51 = characterManager.GetCharacterConfig(MergedOid).characterData
                .frames.Find(value => value != null && value.frameId == MergedFrame);
            if (frame7?.state != 2 || frame8?.state != 2 || frame51 == null)
                return "Formal OID7/8 state2 or OID51 frame290 is unavailable.";
            oid51HitJa = frame51.hit_ja;
            return string.Empty;
        }

        private static void CaptureBaseline()
        {
            baselineCaptured = true;
            baselineObjectCount = world.ObjectCount;
            baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            baselineObjectPoolActive = objectPool.ActiveObjectCountForAcceptance;
            baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineKillStats = Clone(world.KillStats);
            baselineDamageStats = Clone(world.DamageStats);
            baselineRngState = world.Rng.State;
            baselineRngCalls = world.Rng.CallCount;
            structuralBefore = world.StructuralWriterDiagnosticsForDiagnostics;
            BaselineSounds.Clear();
            BaselineSounds.AddRange(world.PendingSounds);
            BaselineHandles.Clear();
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(EntityScratch);
            for (int index = 0; index < EntityScratch.Count; index++)
            {
                LF2Entity entity = EntityScratch[index];
                if (entity?.Runtime != null &&
                    world.TryGetCurrentRuntimeHandleForDiagnostics(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    BaselineHandles.Add(handle);
                }
            }
            EntityScratch.Clear();
            rosterSnapshot = new RosterSnapshot(world.Runtime.Roster);

            report.baselineObjectCount = baselineObjectCount;
            report.baselineClaimedSlots = baselineClaimedSlots;
            report.baselineObjectPoolActive = baselineObjectPoolActive;
            report.baselineLogicPoolActive = baselineLogicPoolActive;
            report.baselineRuntimeHandleCount = BaselineHandles.Count;
        }

        private static void BuildFixture()
        {
            int selfSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(0, 10);
            int partnerSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(10, 20);
            Require(selfSlot >= 0 && partnerSlot >= 10,
                $"No free R08 low slots: self={selfSlot}, partner={partnerSlot}.");

            self = SpawnCharacter(SelfOid, selfSlot, "R08_OID7_Self");
            partner = SpawnCharacter(PartnerOid, partnerSlot, "R08_OID8_Partner");
            BindHumanRoster(self);

            self.RelationTeam = RelationTeam;
            partner.RelationTeam = RelationTeam;
            self.Team = RelationTeam;
            partner.Team = RelationTeam;
            self.Health.HP = 80;
            self.Health.HPBound = 100;
            self.Health.HP3 = 250;
            partner.Health.HP = 70;
            partner.Health.HPBound = 90;
            partner.Health.HP3 = 250;
            int stageZMin = world.Runtime?.Stage?.ZMin ?? 180;
            int stageZMax = world.Runtime?.Stage?.ZMax ?? 350;
            Require(stageZMax - stageZMin >= 8,
                $"Production stage Z range is too narrow for the merge fixture: {stageZMin}..{stageZMax}.");
            fixtureZ = stageZMin + ((stageZMax - stageZMin) / 2) - 2;
            report.fixtureZ = fixtureZ;
            report.stageZMin = stageZMin;
            report.stageZMax = stageZMax;
            self.ImmediateFrame(SelfFrame);
            partner.ImmediateFrame(SelfFrame);
            SetPosition(self, FixtureX, fixtureZ);
            SetPosition(partner, FixtureX + 20, fixtureZ + 4);
            self.Runtime.SetVelocity(3d, -2d, FixtureSelfVz);
            partner.Runtime.SetVelocity(-3d, -4d, -1d);
            self.RefreshRuntimeSnapshot();
            partner.RefreshRuntimeSnapshot();

            selfHandle = RequireHandle(self, "OID7 self");
            partnerHandle = RequireHandle(partner, "OID8 partner");
            report.selfSlot = selfSlot;
            report.partnerSlot = partnerSlot;
            report.selfGeneration = selfHandle.Generation;
            report.partnerGeneration = partnerHandle.Generation;
            report.afterFixtureObjectCount = world.ObjectCount;
            report.afterFixtureClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.oid51HitJa = oid51HitJa;
            structuralBefore = world.StructuralWriterDiagnosticsForDiagnostics;
        }

        private static LF2Character SpawnCharacter(int oid, int runtimeSlot, string name)
        {
            GameObject entityObject = objectPool.Get(out LF2ObjectRenderer renderer);
            LF2Character character = LF2ReferencePool.Instance.Get(
                LF2ObjectType.Character,
                oid) as LF2Character;
            Require(character != null && entityObject != null && renderer != null,
                $"Production pools could not create OID{oid}.");

            character.Controller.SetInputID(7000 + oid);
            character.InjectDependencies(entityObject.transform, renderer.transform, name);
            character.ModuleInitialize();
            character.SetRequiredRuntimeSlot(runtimeSlot);
            renderer.SetLogicObject(character, null);
            character.ModuleBind(characterManager.GetCharacterConfig(oid), oid, world);
            character.Initialize(100, 100);
            character.AiControlled = false;
            return character;
        }

        private static void BindHumanRoster(LF2Character character)
        {
            BattleRosterRuntimeState roster = world.Runtime.Roster;
            int rosterIndex = -1;
            for (int index = 0; index < roster.Slots.Length; index++)
            {
                if (!roster.Slots[index].Active)
                {
                    rosterIndex = index;
                    break;
                }
            }
            Require(rosterIndex >= 0, "No free production roster slot for R08 human input.");

            BattleSlotRuntimeState slot = roster.Slots[rosterIndex];
            slot.Active = true;
            slot.IsHuman = true;
            slot.CharacterId = SelfOid;
            slot.Team = RelationTeam;
            slot.InputId = 7000 + SelfOid;
            slot.AiId = -1;
            slot.RuntimeSlotIndex = character.Runtime.SlotIndex;
            slot.StableId = character.Runtime.StableId;
            roster.ActiveSlotCount++;
            report.rosterSlot = rosterIndex;
        }

        private static void ObserveMergeTick()
        {
            if (!TickCompleted())
                return;

            Require(self.ObjectId == MergedOid && self.Frame.N == MergedFrame,
                $"Full tick did not merge OID7 into OID51/frame290: oid={self.ObjectId}, frame={self.Frame.N}.");
            report.afterMerge = CaptureState("after-merge", self);
            report.dormant = CaptureState("dormant-partner", partner);
            report.mergeObservedObjectCount = world.ObjectCount;
            report.mergeObservedClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            BattleStructuralWriterDiagnostics structuralAfter =
                world.StructuralWriterDiagnosticsForDiagnostics;
            report.mergeSpawnDelta = structuralAfter.SpawnCount - structuralBefore.SpawnCount;
            report.mergeRegisterDelta = structuralAfter.RegisterCount - structuralBefore.RegisterCount;
            report.mergeLastStructuralOid = structuralAfter.LastOid;
            report.mergeLastStructuralSourceSlot = structuralAfter.LastSource.Slot;
            Require(partner.Runtime.OidMergeDormant &&
                    world.ObjectCount == report.afterFixtureObjectCount - 1,
                "Full tick did not make OID8 dormant and decrement ObjectCount exactly once: " +
                $"dormant={partner.Runtime.OidMergeDormant}, objectCount={world.ObjectCount}, " +
                $"expected={report.afterFixtureObjectCount - 1}.");
            Require(self.Runtime.Unk328 == 1 && self.Runtime.Unk32C == report.partnerSlot &&
                    self.Runtime.Unk330 == SelfOid && self.Runtime.Unk334 == PartnerOid &&
                    self.Runtime.Unk338 == 4500 && self.Health.PP == 500,
                "Merged identity metadata/cooldown/PP does not match C++.");
            Require(self.Health.HP == 150 && self.Health.HPBound == 190 &&
                    self.Runtime.XInt == FixtureX + 10 &&
                    self.Runtime.ZInt == fixtureZ + 2 + FixtureSelfVz &&
                    Math.Abs(self.Runtime.Vz) < 0.0001,
                "Merged HP/HPBound/midpoint plus same-tick C++ physics does not match.");
            Require(Math.Abs(self.Runtime.Vx) < 0.0001 && Math.Abs(partner.Runtime.Vy) < 0.0001,
                "Merge did not clear self.vx and partner.vy.");
            Require(world.TryResolveRuntimeHandleForDiagnostics(selfHandle, out LF2Entity resolvedSelf) &&
                    ReferenceEquals(resolvedSelf, self) &&
                    world.TryResolveRuntimeHandleForDiagnostics(partnerHandle, out LF2Entity resolvedPartner) &&
                    ReferenceEquals(resolvedPartner, partner),
                "Merge changed slot generation or invalidated the dormant partner handle.");
            Require(world.FindFirstFreeRuntimeSlotForDiagnostics(report.partnerSlot, report.partnerSlot + 1) < 0,
                "Dormant partner slot became allocator-visible.");

            completionEditorUpdate = editorUpdates;
            phase = ProbePhase.WaitingForMergePlan;
        }

        private static void ObserveMergePlan()
        {
            if (!CentralPlanReady(expectedTick, completionEditorUpdate))
                return;

            BattleCentralEntityDiagnostic selfBody = BattleCentralRenderSystem
                .CaptureEntityDiagnostic(world, selfHandle, BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic partnerBody = BattleCentralRenderSystem
                .CaptureEntityDiagnostic(world, partnerHandle, BattleRenderCommandType.Entity);
            report.mergedBodySubmitted = selfBody.HasSnapshot && selfBody.HasCommand &&
                selfBody.HasResolvedResource && selfBody.Submitted;
            report.dormantBodySuppressed = !partnerBody.HasCommand && !partnerBody.Submitted;
            Require(report.mergedBodySubmitted && report.dormantBodySuppressed,
                $"Central merge visibility mismatch: self={selfBody.Reason}, partner={partnerBody.Reason}.");

            if (oid51HitJa != 0)
            {
                report.releaseMode = "canonical-dja";
                QueuePhysicalDjaStep(FuncKeyMask.att);
                ScheduleTick(ProbePhase.WaitingForDefendTick, false);
                return;
            }

            report.releaseMode = "cooldown-4500";
            phase = ProbePhase.WaitingForCooldownDrain;
        }

        private static void ObserveComboTick(
            int expectedCombo,
            FuncKeyMask nextPhysicalStep,
            ProbePhase nextPhase)
        {
            if (!TickCompleted())
                return;
            Require(self.ObjectId == MergedOid && partner.Runtime.OidMergeDormant,
                "Merged identity split before the DJA release completed.");
            Require(self.Runtime.ComboDja == expectedCombo,
                $"DJA progress mismatch: expected={expectedCombo}, actual={self.Runtime.ComboDja}.");
            QueuePhysicalDjaStep(nextPhysicalStep);
            ScheduleTick(nextPhase, false);
        }

        private static void ObserveAttackTick()
        {
            if (!TickCompleted())
                return;
            Require(self.ObjectId == MergedOid && partner.Runtime.OidMergeDormant,
                "DJA action tick split during CharacterInput instead of the next maintenance.");
            Require(self.Runtime.Unk338 == 0,
                $"Canonical DJA did not clear merged Unk338: {self.Runtime.Unk338}.");
            report.afterDja = CaptureState("after-dja-release", self);
            report.djaReleasePassed = true;
            ScheduleTick(ProbePhase.WaitingForSplitTick, true);
        }

        private static void AdvanceCooldownDrain()
        {
            for (int index = 0; index < CooldownTicksPerEditorUpdate; index++)
            {
                Require(self.ObjectId == MergedOid && partner.Runtime.OidMergeDormant,
                    "Merged identity split before the formal cooldown reached zero.");
                int cooldownBefore = self.Runtime.Unk338;
                Require(cooldownBefore > 0,
                    $"Merged cooldown reached an invalid pre-maintenance value: {cooldownBefore}.");

                if (cooldownBefore == 1)
                {
                    report.preSplitObjectCount = world.ObjectCount;
                    report.preSplitClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
                    report.preSplitSelf = CaptureState("pre-split-self", self);
                    report.preSplitPartner = CaptureState("pre-split-partner", partner);
                    structuralBeforeSplit = world.StructuralWriterDiagnosticsForDiagnostics;
                }

                expectedTick = driver.CurrentTickIndex + 1;
                bool buildPresentation = cooldownBefore == 1;
                bool accepted = driver.StepOneTick(
                    FrameInputSet.Empty(expectedTick),
                    ignorePaused: true,
                    buildPresentation: buildPresentation);
                Require(accepted, "Production driver rejected an R08 cooldown-drain tick.");
                cooldownTicksAdvanced++;

                if (cooldownBefore > 1)
                {
                    Require(self.ObjectId == MergedOid && partner.Runtime.OidMergeDormant &&
                            self.Runtime.Unk338 == cooldownBefore - 1,
                        "Cooldown drain changed merged ownership or skipped a C++ decrement.");
                    continue;
                }

                report.cooldownTicksAdvanced = cooldownTicksAdvanced;
                report.cooldownReleasePassed = true;
                completionEditorUpdate = editorUpdates;
                phase = ProbePhase.WaitingForSplitTick;
                return;
            }
        }

        private static void ObserveSplitTick()
        {
            if (!TickCompleted())
                return;

            report.afterSplitSelf = CaptureState("after-split-self", self);
            report.afterSplitPartner = CaptureState("after-split-partner", partner);
            report.splitObservedObjectCount = world.ObjectCount;
            report.splitObservedClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
            BattleStructuralWriterDiagnostics structuralAfterSplit =
                world.StructuralWriterDiagnosticsForDiagnostics;
            report.splitSpawnDelta =
                structuralAfterSplit.SpawnCount - structuralBeforeSplit.SpawnCount;
            report.splitRegisterDelta =
                structuralAfterSplit.RegisterCount - structuralBeforeSplit.RegisterCount;
            report.splitLastStructuralOid = structuralAfterSplit.LastOid;
            report.splitLastStructuralSourceSlot = structuralAfterSplit.LastSource.Slot;

            Require(self.ObjectId == SelfOid && partner.ObjectId == PartnerOid &&
                    !partner.Runtime.OidMergeDormant,
                "Next full maintenance did not restore the original OID pair: " +
                $"selfOid={self.ObjectId}, partnerOid={partner.ObjectId}, " +
                $"partnerDormant={partner.Runtime.OidMergeDormant}.");
            Require(self.Runtime.SlotIndex == report.selfSlot &&
                    partner.Runtime.SlotIndex == report.partnerSlot &&
                    self.Runtime.Unk338 == 900,
                "Split changed original slots or missed self cooldown900.");
            Require(world.ObjectCount == report.preSplitObjectCount + 1 &&
                    world.ClaimedRuntimeSlotCountForDiagnostics == report.preSplitClaimedSlots,
                "Split did not restore exactly one dormant object while preserving claimed slots: " +
                $"preSplit={report.preSplitObjectCount}, actual={world.ObjectCount}, " +
                $"preClaimed={report.preSplitClaimedSlots}, " +
                $"actualClaimed={world.ClaimedRuntimeSlotCountForDiagnostics}, " +
                $"spawnDelta={report.splitSpawnDelta}, registerDelta={report.splitRegisterDelta}, " +
                $"lastOid={report.splitLastStructuralOid}, " +
                $"lastSourceSlot={report.splitLastStructuralSourceSlot}.");
            int expectedSplitHp = report.preSplitSelf.hp / 2;
            int expectedSplitHpBound = report.preSplitSelf.hpBound / 2;
            Require(self.Frame.N == SplitObservedFrame &&
                    partner.Frame.N == SplitObservedFrame &&
                    self.Frame.D?.state == 8 && partner.Frame.D?.state == 8 &&
                    self.Health.HP == expectedSplitHp &&
                    partner.Health.HP == expectedSplitHp &&
                    self.Health.HPBound == expectedSplitHpBound &&
                    partner.Health.HPBound == expectedSplitHpBound &&
                    self.Health.PP == 0 && partner.Health.PP == 0,
                "Split tick-end frame/current-half-health/PP mismatch: " +
                $"frames={self.Frame.N}/{partner.Frame.N}, " +
                $"states={self.Frame.D?.state}/{partner.Frame.D?.state}, " +
                $"hp={self.Health.HP}/{partner.Health.HP} expected={expectedSplitHp}, " +
                $"bounds={self.Health.HPBound}/{partner.Health.HPBound} " +
                $"expected={expectedSplitHpBound}.");
            Require(self.Runtime.XInt == partner.Runtime.XInt &&
                    self.Runtime.YInt == 0 && partner.Runtime.YInt == 0 &&
                    self.Runtime.ZInt == partner.Runtime.ZInt &&
                    Math.Abs(self.Runtime.Vx) < 0.0001 &&
                    Math.Abs(partner.Runtime.Vx) < 0.0001,
                "Split position/velocity reset mismatch.");
            Require(world.TryResolveRuntimeHandleForDiagnostics(selfHandle, out _) &&
                    world.TryResolveRuntimeHandleForDiagnostics(partnerHandle, out _),
                "Split replaced an original runtime generation.");

            completionEditorUpdate = editorUpdates;
            phase = ProbePhase.WaitingForSplitPlan;
        }

        private static void ObserveSplitPlan()
        {
            if (!CentralPlanReady(expectedTick, completionEditorUpdate))
                return;

            BattleCentralEntityDiagnostic selfBody = BattleCentralRenderSystem
                .CaptureEntityDiagnostic(world, selfHandle, BattleRenderCommandType.Entity);
            BattleCentralEntityDiagnostic partnerBody = BattleCentralRenderSystem
                .CaptureEntityDiagnostic(world, partnerHandle, BattleRenderCommandType.Entity);
            report.splitBodiesSubmitted = selfBody.HasCommand && selfBody.HasResolvedResource &&
                partnerBody.HasCommand && partnerBody.HasResolvedResource;
            Require(report.splitBodiesSubmitted,
                $"Central split visibility mismatch: self={selfBody.Reason}, partner={partnerBody.Reason}.");
            report.mergeSplitPassed = true;
            FinishSuccess();
        }

        private static void QueuePhysicalDjaStep(FuncKeyMask physicalKey)
        {
            queuedPhysicalButtons = physicalKey switch
            {
                FuncKeyMask.att => SimulationInputButtons.Attack,
                FuncKeyMask.def => SimulationInputButtons.Defend,
                FuncKeyMask.jump => SimulationInputButtons.Jump,
                _ => throw new InvalidOperationException(
                    $"Unsupported physical DJA key for canonical frame input: {physicalKey}."),
            };
        }

        private static void ScheduleTick(ProbePhase nextPhase, bool buildPresentation)
        {
            expectedTick = driver.CurrentTickIndex + 1;
            FrameInputSet canonicalInput = null;
            if (queuedPhysicalButtons != SimulationInputButtons.None)
            {
                canonicalInput = new FrameInputSet(
                    expectedTick,
                    new[]
                    {
                        new SimulationPlayerInput(report.rosterSlot, queuedPhysicalButtons),
                    });
                queuedPhysicalButtons = SimulationInputButtons.None;
            }

            bool accepted = canonicalInput != null
                ? driver.StepOneTick(canonicalInput, ignorePaused: true, buildPresentation: buildPresentation)
                : workerPath
                    ? driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(buildPresentation)
                    : driver.StepOneTick(ignorePaused: true, buildPresentation: buildPresentation);
            Require(accepted,
                workerPath
                    ? "Production worker rejected R08 tick: " +
                      driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics
                    : "Production synchronous driver rejected R08 tick.");
            phase = nextPhase;
        }

        private static bool TickCompleted()
        {
            return !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                   driver.CurrentTickIndex >= expectedTick;
        }

        private static bool CentralPlanReady(int tick, int completedUpdate)
        {
            if (editorUpdates <= completedUpdate + 1 ||
                driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return false;
            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            if (!plan.IsValid || plan.SimulationTick != tick ||
                plan.CapturedFrame?.TickIndex != tick)
            {
                plan = BattleCentralRenderSystem.PrepareFrame(world);
            }
            return plan.IsValid && plan.Owner == BattlePixelFrameOwner.Central &&
                   !plan.IsStale && plan.Submission != null &&
                   plan.CapturedFrame?.CommandsMaterialized == true &&
                   plan.SimulationTick == tick;
        }

        private static RuntimeEntityHandle RequireHandle(LF2Entity entity, string label)
        {
            if (!world.TryGetCurrentRuntimeHandleForDiagnostics(
                    entity.Runtime.SlotIndex,
                    entity,
                    out RuntimeEntityHandle handle) || !handle.IsValid)
            {
                throw new InvalidOperationException(label + " has no valid runtime handle.");
            }
            return handle;
        }

        private static void SetPosition(LF2Entity entity, int x, int z)
        {
            entity.Runtime.SetPosition(x, 0, z);
            entity.Runtime.SyncIntegerPosition();
        }

        private static EntityEvidence CaptureState(string checkpoint, LF2Entity entity)
        {
            return new EntityEvidence
            {
                checkpoint = checkpoint,
                tick = driver?.CurrentTickIndex ?? -1,
                oid = entity?.ObjectId ?? -1,
                slot = entity?.Runtime?.SlotIndex ?? -1,
                generation = ResolveGeneration(entity),
                frame = entity?.Frame?.N ?? -1,
                state = entity?.Frame?.D?.state ?? -1,
                dormant = entity?.Runtime?.OidMergeDormant == true,
                hp = entity?.Health?.HP ?? 0,
                hpBound = entity?.Health?.HPBound ?? 0,
                pp = entity?.Health?.PP ?? 0,
                xInt = entity?.Runtime?.XInt ?? 0,
                yInt = entity?.Runtime?.YInt ?? 0,
                zInt = entity?.Runtime?.ZInt ?? 0,
                vx = entity?.Runtime?.Vx ?? 0,
                vy = entity?.Runtime?.Vy ?? 0,
                vz = entity?.Runtime?.Vz ?? 0,
                unk328 = entity?.Runtime?.Unk328 ?? -1,
                unk32C = entity?.Runtime?.Unk32C ?? -1,
                unk330 = entity?.Runtime?.Unk330 ?? 0,
                unk334 = entity?.Runtime?.Unk334 ?? 0,
                unk338 = entity?.Runtime?.Unk338 ?? 0,
                comboDja = entity?.Runtime?.ComboDja ?? 0,
                relationTeam = entity?.RelationTeam ?? 0,
                direction = entity?.Runtime?.Dir ?? string.Empty,
            };
        }

        private static void FinishSuccess()
        {
            report.status = "PASS";
            report.message =
                "Production OID7/8 merge, dormant slot/generation/central suppression, " +
                report.releaseMode + " release, OID51 split, restored central visibility, and cleanup passed.";
            report.endTick = driver.CurrentTickIndex;
            Cleanup();
            CaptureFinalState();
            Require(report.cleanupCompleted,
                "R08 cleanup did not restore baseline: " + report.cleanupErrors);
            WriteResult(report);
            Debug.Log($"[BattleOid5152MergeSplitProbe] PASS: slots={report.selfSlot}/{report.partnerSlot}, " +
                $"ticks={report.startTick}->{report.endTick}.");
            StopObservation();
        }

        private static uint ResolveGeneration(LF2Entity entity)
        {
            if (entity?.Runtime == null || world == null)
                return 0;
            return world.TryGetCurrentRuntimeHandleForDiagnostics(
                entity.Runtime.SlotIndex,
                entity,
                out RuntimeEntityHandle handle)
                ? handle.Generation
                : 0;
        }

        private static void Fail(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.message = message;
            report.endTick = driver?.CurrentTickIndex ?? -1;
            Cleanup();
            CaptureFinalState();
            WriteResult(report);
            Debug.LogError("[BattleOid5152MergeSplitProbe] FAIL: " + message);
            StopObservation();
        }

        private static void Cleanup()
        {
            if (world == null)
                return;
            Release(ref self);
            Release(ref partner);
            try
            {
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                AppendCleanupError("flush", exception);
            }
            CleanupPostBaselineEntities();
            rosterSnapshot?.Restore(world.Runtime.Roster);
            Restore(world.KillStats, baselineKillStats);
            Restore(world.DamageStats, baselineDamageStats);
            world.PendingSounds.Clear();
            world.PendingSounds.AddRange(BaselineSounds);
            world.Rng.RestoreState(baselineRngState, baselineRngCalls);
            try
            {
                world.RenderDispatchAll(driver?.CurrentTickIndex ?? 0, true);
                BattleCentralRenderSystem.PrepareFrame(world);
            }
            catch (Exception exception)
            {
                AppendCleanupError("presentation", exception);
            }
        }

        private static void CleanupPostBaselineEntities()
        {
            const int maximumPasses = 8;
            for (int pass = 0; pass < maximumPasses; pass++)
            {
                try
                {
                    world.GetActiveRuntimeEntitySnapshotForDiagnostics(EntityScratch);
                }
                catch (Exception exception)
                {
                    AppendCleanupError($"capture-post-baseline-{pass}", exception);
                    return;
                }

                bool releasedAny = false;
                for (int index = EntityScratch.Count - 1; index >= 0; index--)
                {
                    LF2Entity entity = EntityScratch[index];
                    if (entity?.Runtime == null ||
                        !world.TryGetCurrentRuntimeHandleForDiagnostics(
                            entity.Runtime.SlotIndex,
                            entity,
                            out RuntimeEntityHandle handle) ||
                        BaselineHandles.Contains(handle))
                    {
                        continue;
                    }

                    try
                    {
                        entity.FreeEntityLikeExe();
                        report.postBaselineEntitiesReleased++;
                        releasedAny = true;
                    }
                    catch (Exception exception)
                    {
                        AppendCleanupError(entity.Name ?? $"post-baseline-{handle}", exception);
                    }
                }
                EntityScratch.Clear();

                try
                {
                    world.FlushPendingDestroyForDiagnostics();
                }
                catch (Exception exception)
                {
                    AppendCleanupError($"flush-post-baseline-{pass}", exception);
                    return;
                }

                if (!releasedAny)
                    return;
            }

            world.GetActiveRuntimeEntitySnapshotForDiagnostics(EntityScratch);
            for (int index = 0; index < EntityScratch.Count; index++)
            {
                LF2Entity entity = EntityScratch[index];
                if (entity?.Runtime != null &&
                    world.TryGetCurrentRuntimeHandleForDiagnostics(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle) &&
                    !BaselineHandles.Contains(handle))
                {
                    EntityScratch.Clear();
                    AppendCleanupError(
                        "post-baseline-max-passes",
                        new InvalidOperationException(
                            "Post-baseline entities remained after cleanup pass limit."));
                    return;
                }
            }
            EntityScratch.Clear();
        }

        private static void Release(ref LF2Character character)
        {
            LF2Character current = character;
            character = null;
            if (current == null)
                return;
            try
            {
                if (current.Match == world && current.Runtime?.SlotIndex >= 0)
                    current.FreeEntityLikeExe();
            }
            catch (Exception exception)
            {
                AppendCleanupError(current.Name, exception);
            }
        }

        private static void CaptureFinalState()
        {
            if (report == null)
                return;
            report.finalObjectCount = world?.ObjectCount ?? -1;
            report.finalClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.finalObjectPoolActive = objectPool?.ActiveObjectCountForAcceptance ?? -1;
            report.finalLogicPoolActive = LF2ReferencePool.Instance?.ActiveCount ?? -1;
            report.rngRestored = world?.Rng != null && world.Rng.State == baselineRngState &&
                world.Rng.CallCount == baselineRngCalls;
            report.cleanupCompleted = baselineCaptured && string.IsNullOrEmpty(report.cleanupErrors) &&
                report.finalObjectCount == baselineObjectCount &&
                report.finalClaimedSlots == baselineClaimedSlots &&
                report.finalObjectPoolActive == baselineObjectPoolActive &&
                report.finalLogicPoolActive == baselineLogicPoolActive && report.rngRestored;
        }

        private static void AppendCleanupError(string label, Exception exception)
        {
            if (report != null)
                report.cleanupErrors += label + ":" + exception.Message + ";";
        }

        private static int[] Clone(int[] source)
        {
            return source == null ? null : (int[])source.Clone();
        }

        private static void Restore(int[] destination, int[] source)
        {
            if (destination == null || source == null)
                return;
            Array.Copy(source, destination, Math.Min(source.Length, destination.Length));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void WriteResult(ProbeReport value)
        {
            string path = ProjectPath(ResultRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeReport { status = "FAIL", message = message });
            Debug.LogError("[BattleOid5152MergeSplitProbe] FAIL: " + message);
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            if (driver != null && EditorApplication.isPlaying)
                driver.SetPaused(previousPaused);
            running = false;
        }

        private static void ResetState()
        {
            driver = null;
            world = null;
            objectPool = null;
            characterManager = null;
            self = null;
            partner = null;
            selfHandle = default;
            partnerHandle = default;
            report = null;
            rosterSnapshot = null;
            phase = ProbePhase.WaitingForSafeBoundary;
            editorUpdates = 0;
            expectedTick = 0;
            completionEditorUpdate = 0;
            stableTick = 0;
            stableUpdates = 0;
            fixtureZ = 0;
            queuedPhysicalButtons = SimulationInputButtons.None;
            oid51HitJa = 0;
            cooldownTicksAdvanced = 0;
            baselineObjectCount = 0;
            baselineClaimedSlots = 0;
            baselineObjectPoolActive = 0;
            baselineLogicPoolActive = 0;
            baselineKillStats = null;
            baselineDamageStats = null;
            baselineRngState = 0;
            baselineRngCalls = 0;
            structuralBefore = default;
            structuralBeforeSplit = default;
            previousPaused = false;
            workerPath = false;
            pauseRequested = false;
            baselineCaptured = false;
            running = false;
            lastCatalogReadinessFailure = string.Empty;
            BaselineSounds.Clear();
            BaselineHandles.Clear();
            EntityScratch.Clear();
        }

        private sealed class RosterSnapshot
        {
            private readonly SlotSnapshot[] slots;
            private readonly int activeSlotCount;

            public RosterSnapshot(BattleRosterRuntimeState roster)
            {
                activeSlotCount = roster.ActiveSlotCount;
                slots = new SlotSnapshot[roster.Slots.Length];
                for (int index = 0; index < slots.Length; index++)
                    slots[index] = new SlotSnapshot(roster.Slots[index]);
            }

            public void Restore(BattleRosterRuntimeState roster)
            {
                roster.ActiveSlotCount = activeSlotCount;
                for (int index = 0; index < slots.Length; index++)
                    slots[index].Restore(roster.Slots[index]);
            }
        }

        private readonly struct SlotSnapshot
        {
            private readonly bool active;
            private readonly bool human;
            private readonly int characterId;
            private readonly int team;
            private readonly int inputId;
            private readonly int aiId;
            private readonly int runtimeSlot;
            private readonly int stableId;

            public SlotSnapshot(BattleSlotRuntimeState slot)
            {
                active = slot.Active;
                human = slot.IsHuman;
                characterId = slot.CharacterId;
                team = slot.Team;
                inputId = slot.InputId;
                aiId = slot.AiId;
                runtimeSlot = slot.RuntimeSlotIndex;
                stableId = slot.StableId;
            }

            public void Restore(BattleSlotRuntimeState slot)
            {
                slot.Active = active;
                slot.IsHuman = human;
                slot.CharacterId = characterId;
                slot.Team = team;
                slot.InputId = inputId;
                slot.AiId = aiId;
                slot.RuntimeSlotIndex = runtimeSlot;
                slot.StableId = stableId;
            }
        }

        private enum ProbePhase
        {
            WaitingForSafeBoundary,
            WaitingForMergeTick,
            WaitingForMergePlan,
            WaitingForDefendTick,
            WaitingForJumpTick,
            WaitingForAttackTick,
            WaitingForCooldownDrain,
            WaitingForSplitTick,
            WaitingForSplitPlan,
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public int startTick;
            public int endTick;
            public bool workerPath;
            public int selfSlot;
            public int partnerSlot;
            public uint selfGeneration;
            public uint partnerGeneration;
            public int rosterSlot;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int baselineObjectPoolActive;
            public int baselineLogicPoolActive;
            public int baselineRuntimeHandleCount;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public int postBaselineEntitiesReleased;
            public int afterFixtureObjectCount;
            public int afterFixtureClaimedSlots;
            public int fixtureZ;
            public int stageZMin;
            public int stageZMax;
            public int oid51HitJa;
            public string releaseMode = string.Empty;
            public int cooldownTicksAdvanced;
            public int mergeObservedObjectCount;
            public int mergeObservedClaimedSlots;
            public long mergeSpawnDelta;
            public long mergeRegisterDelta;
            public int mergeLastStructuralOid;
            public int mergeLastStructuralSourceSlot;
            public int preSplitObjectCount;
            public int preSplitClaimedSlots;
            public int splitObservedObjectCount;
            public int splitObservedClaimedSlots;
            public long splitSpawnDelta;
            public long splitRegisterDelta;
            public int splitLastStructuralOid;
            public int splitLastStructuralSourceSlot;
            public bool mergedBodySubmitted;
            public bool dormantBodySuppressed;
            public bool djaReleasePassed;
            public bool cooldownReleasePassed;
            public bool splitBodiesSubmitted;
            public bool mergeSplitPassed;
            public bool rngRestored;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public EntityEvidence beforeMerge;
            public EntityEvidence afterMerge;
            public EntityEvidence dormant;
            public EntityEvidence afterDja;
            public EntityEvidence preSplitSelf;
            public EntityEvidence preSplitPartner;
            public EntityEvidence afterSplitSelf;
            public EntityEvidence afterSplitPartner;
        }

        [Serializable]
        private sealed class EntityEvidence
        {
            public string checkpoint = string.Empty;
            public int tick;
            public int oid;
            public int slot;
            public uint generation;
            public int frame;
            public int state;
            public bool dormant;
            public int hp;
            public int hpBound;
            public int pp;
            public int xInt;
            public int yInt;
            public int zInt;
            public double vx;
            public double vy;
            public double vz;
            public int unk328;
            public int unk32C;
            public int unk330;
            public int unk334;
            public int unk338;
            public int comboDja;
            public int relationTeam;
            public string direction = string.Empty;
        }
    }
}
#endif
