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
    /// Explicit Editor-only S4 probe for the live pickup, held-pose, throw,
    /// release, and weapon-landing writers.
    /// </summary>
    public static class BattleHeldWeaponLifecyclePlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/验证/R8/运行拾取持有投掷落地Play探针";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01C_02_HeldWeaponLifecycle.result.json";
        private const int TickTimeoutEditorUpdates = 1800;
        private const int HolderOid = 2;
        private const int SpawnerSentinel = 8765;
        private const int PickerSentinel = 7654;

        private static readonly WeaponSpec[] Specs =
        {
            new WeaponSpec(120, (int)LF2ObjectType.LightWeapon, 1, 115, -1, true),
            new WeaponSpec(150, (int)LF2ObjectType.HeavyWeapon, 2, 116, -2, false),
            new WeaponSpec(121, (int)LF2ObjectType.ThrowWeapon, 4, 115, -4, true),
            new WeaponSpec(122, (int)LF2ObjectType.Drink, 6, 115, -6, true),
        };

        private static readonly List<LF2Entity> OwnedEntities =
            new List<LF2Entity>(12);
        private static readonly List<WeaponEvidence> EvidenceRows =
            new List<WeaponEvidence>(4);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool objectPool;
        private static ProbeResult result;
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
                LF2ReferencePool.Instance == null || GameDataManager.Instance == null)
            {
                WriteImmediateFailure(
                    "The production driver, world, data catalog, or pools are not ready.");
                return;
            }

            string catalogFailure = ValidateCatalog();
            if (!string.IsNullOrEmpty(catalogFailure))
            {
                WriteImmediateFailure(catalogFailure);
                return;
            }

            previousPaused = driver.IsPaused;
            result = new ProbeResult
            {
                status = "RUNNING",
                startTick = -1,
            };

            editorUpdates = 0;
            running = true;
            EditorApplication.update += Observe;
            Debug.Log(
                "[BattleHeldWeaponLifecyclePlayModeProbe] Waiting for the " +
                "production simulation worker to reach an idle boundary.");
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
                ExecuteWeaponMatrix();
                ExecuteNoImmediateLandingHitWitness();
                FinishSuccess();
            }
            catch (Exception exception)
            {
                Fail("Unhandled probe exception: " + exception);
            }
        }

        private static void ExecuteWeaponMatrix()
        {
            for (int index = 0; index < Specs.Length; index++)
            {
                WeaponSpec spec = Specs[index];
                ProbeCharacter holder = null;
                ProbeWeapon weapon = null;
                try
                {
                    holder = new ProbeCharacter(
                        "R8HeldHolder_" + spec.DataType,
                        cover: index % 2);
                    weapon = new ProbeWeapon(
                        "R8HeldWeapon_" + spec.Oid,
                        spec.Oid,
                        spec.DataType);
                    RegisterOwned(holder);
                    RegisterOwned(weapon);

                    WeaponEvidence row = RunPickupHeldThrowLanding(
                        holder,
                        weapon,
                        spec,
                        index % 2);
                    EvidenceRows.Add(row);
                }
                finally
                {
                    UnregisterOwned(weapon, "weapon-" + spec.Oid);
                    UnregisterOwned(holder, "holder-" + spec.Oid);
                }
            }
        }

        private static WeaponEvidence RunPickupHeldThrowLanding(
            ProbeCharacter holder,
            ProbeWeapon weapon,
            WeaponSpec spec,
            int cover)
        {
            int holderSlot = RequireSlot(holder, "holder");
            int weaponSlot = RequireSlot(weapon, "weapon");
            holder.Team = 3;
            holder.RelationTeam = 3;
            weapon.Team = 0;
            weapon.RelationTeam = 0;
            holder.AttackingCounter = 9;
            weapon.ImmediateFrame(
                spec.DataType == (int)LF2ObjectType.HeavyWeapon ? 20 : 60);

            LF2CharacterInteractionResolver pickupResolver =
                new LF2CharacterInteractionResolver(holder);
            bool picked = pickupResolver.TryApplyPreInteraction(
                new InteractionArea { kind = 2 },
                weapon);
            int expectedLink = spec.HolderLink;
            Require(picked, $"type {spec.DataType} kind2 pickup was rejected");
            Require(holder.Frame.N == spec.PickupFrame,
                $"type {spec.DataType} pickup frame {holder.Frame.N} != {spec.PickupFrame}");
            Require(holder.Runtime.LinkState == expectedLink &&
                    weapon.Runtime.LinkState == spec.TargetLink,
                $"type {spec.DataType} pickup link mismatch: " +
                $"holder={holder.Runtime.LinkState}, weapon={weapon.Runtime.LinkState}");
            Require(holder.Runtime.TargetSlotIndex == weaponSlot &&
                    holder.Runtime.HeldWeaponStableId == weaponSlot &&
                    weapon.Runtime.HolderStableId == holderSlot &&
                    weapon.HolderCopySlot == holderSlot,
                $"type {spec.DataType} pickup slot relationship mismatch");
            Require(holder.Runtime.PickupCount == 1 &&
                    holder.AttackingCounter == 0 &&
                    weapon.RelationTeam == holder.RelationTeam &&
                    ReferenceEquals(holder.GetHeldWeapon(), weapon),
                $"type {spec.DataType} pickup side-effect mismatch");

            holder.Runtime.SetPosition(100.0, -10.0, 200.0);
            holder.Runtime.SyncIntegerPosition();
            holder.SwitchDir("right");
            holder.FrameDelay = 7;
            weapon.FrameDelay = -3;

            WeaponPoint posePoint = holder.Frame.D.wpoints[0];
            bool poseRan = holder.ReleaseHeldObjectByWPoint(
                weapon,
                posePoint,
                out WeaponActResult poseResult);
            int expectedHeldX = 126;
            int expectedHeldY = cover == 0 ? -16 : -14;
            int expectedHeldZ = cover == 0 ? 201 : 199;
            Require(poseRan && !poseResult.Thrown && !poseResult.ForceDrop,
                $"type {spec.DataType} held pose did not remain held");
            Require(weapon.Frame.N == 0 &&
                    weapon.FrameDelay == 7 &&
                    weapon.Runtime.Dir == "right" &&
                    weapon.Runtime.XInt == expectedHeldX &&
                    weapon.Runtime.YInt == expectedHeldY &&
                    weapon.Runtime.ZInt == expectedHeldZ,
                $"type {spec.DataType} held pose mismatch: " +
                $"frame={weapon.Frame.N}, delay={weapon.FrameDelay}, " +
                $"pos={weapon.Runtime.XInt}/{weapon.Runtime.YInt}/{weapon.Runtime.ZInt}");

            holder.ImmediateFrame(117);
            holder.FrameDelay = 9;
            holder.Runtime.KeyUp = 1;
            holder.Runtime.KeyDown = 0;
            weapon.SpawnerEntityIndex = SpawnerSentinel;
            weapon.PickerStableId = PickerSentinel;
            weapon.Runtime.SetVelocity(0.0, 0.0, 0.0);

            WeaponPoint throwPoint = holder.Frame.D.wpoints[0];
            bool throwRan = holder.ReleaseHeldObjectByWPoint(
                weapon,
                throwPoint,
                out WeaponActResult throwResult);
            bool frameMatched = spec.DataType == (int)LF2ObjectType.HeavyWeapon
                ? weapon.Frame.N >= 0 && weapon.Frame.N < 6
                : weapon.Frame.N == 40;
            int expectedSpawner = spec.StampSpawner
                ? holderSlot
                : SpawnerSentinel;
            Require(throwRan && throwResult.Thrown && frameMatched,
                $"type {spec.DataType} throw frame mismatch: {weapon.Frame.N}");
            Require(Nearly(weapon.Runtime.Vx, 12.0) &&
                    Nearly(weapon.Runtime.Vy, -4.0) &&
                    Nearly(weapon.Runtime.Vz, -3.0),
                $"type {spec.DataType} throw velocity mismatch: " +
                $"{weapon.Runtime.Vx}/{weapon.Runtime.Vy}/{weapon.Runtime.Vz}");
            Require(holder.Runtime.LinkState == 0 &&
                    holder.Runtime.HeldWeaponStableId == -1 &&
                    holder.Runtime.TargetSlotIndex == weaponSlot &&
                    weapon.Runtime.LinkState == 0 &&
                    weapon.Runtime.HolderStableId == holderSlot &&
                    weapon.HolderCopySlot == holderSlot,
                $"type {spec.DataType} throw relationship teardown mismatch");
            Require(weapon.FrameDelay == 9 &&
                    weapon.SpawnerEntityIndex == expectedSpawner &&
                    weapon.PickerStableId == PickerSentinel,
                $"type {spec.DataType} throw writer mismatch: " +
                $"delay={weapon.FrameDelay}, spawner={weapon.SpawnerEntityIndex}, " +
                $"picker={weapon.PickerStableId}");

            int observedThrowFrame = weapon.Frame.N;
            int observedSpawner = weapon.SpawnerEntityIndex;
            int observedPicker = weapon.PickerStableId;
            LandingEvidence landing = RunLandingWitness(weapon, spec);
            return new WeaponEvidence
            {
                oid = spec.Oid,
                dataType = spec.DataType,
                holderSlot = holderSlot,
                weaponSlot = weaponSlot,
                pickupAccepted = picked,
                pickupFrame = spec.PickupFrame,
                holderLinkAfterPickup = expectedLink,
                weaponLinkAfterPickup = spec.TargetLink,
                pickupCount = holder.Runtime.PickupCount,
                cover = cover,
                heldFrame = 0,
                heldFrameDelay = 7,
                heldX = expectedHeldX,
                heldY = expectedHeldY,
                heldZ = expectedHeldZ,
                throwFrame = observedThrowFrame,
                throwFrameMatched = frameMatched,
                throwFrameDelay = 9,
                throwVx = 12.0,
                throwVy = -4.0,
                throwVz = -3.0,
                spawnerAfterThrow = observedSpawner,
                expectedSpawnerAfterThrow = expectedSpawner,
                pickerAfterThrow = observedPicker,
                relationshipsReleased = true,
                landing = landing,
            };
        }

        private static LandingEvidence RunLandingWitness(
            ProbeWeapon weapon,
            WeaponSpec spec)
        {
            weapon.Runtime.WeaponFlightCounter = 100;
            weapon.Runtime.SetPosition(300.0, -1.0, 250.0);
            double landingVy;
            int expectedFrame;
            double expectedVx;
            double expectedVy;
            int expectedFlightCounter;
            int expectedAttacking;

            if (spec.DataType == (int)LF2ObjectType.HeavyWeapon)
            {
                weapon.ImmediateFrame(0);
                weapon.Runtime.Vx = 8.0;
                landingVy = 5.0;
                expectedFrame = 20;
                expectedVx = 4.0;
                expectedVy = 0.0;
                expectedFlightCounter = 96;
                expectedAttacking = 0;
            }
            else if (spec.DataType == (int)LF2ObjectType.ThrowWeapon)
            {
                weapon.ImmediateFrame(40);
                weapon.Runtime.Vx = 8.0;
                landingVy = 12.0;
                expectedFrame = 0;
                expectedVx = 5.6;
                expectedVy = -8.4;
                expectedFlightCounter = 97;
                expectedAttacking = 1;
            }
            else
            {
                weapon.ImmediateFrame(40);
                weapon.Runtime.Vx = 8.0;
                landingVy = 5.0;
                expectedFrame = 70;
                expectedVx = spec.DataType == (int)LF2ObjectType.LightWeapon
                    ? 4.0
                    : 5.6;
                expectedVy = 0.0;
                expectedFlightCounter = 97;
                expectedAttacking = 0;
            }

            weapon.AttackingCounter = 1;
            LF2FrameData landingFrame = weapon.Frame.D;
            bool handled = weapon.InvokeCurrentDatLanding(
                spec.DataType,
                landingFrame,
                landingVy,
                crossedGround: true);
            Require(handled, $"type {spec.DataType} landing writer rejected the fixture");
            Require(weapon.Frame.N == expectedFrame &&
                    Nearly(weapon.Runtime.Y, 0.0) &&
                    Nearly(weapon.Runtime.Vx, expectedVx) &&
                    Nearly(weapon.Runtime.Vy, expectedVy) &&
                    weapon.Runtime.WeaponFlightCounter == expectedFlightCounter &&
                    weapon.AttackingCounter == expectedAttacking,
                $"type {spec.DataType} landing mismatch: frame={weapon.Frame.N}, " +
                $"posY={weapon.Runtime.Y}, velocity={weapon.Runtime.Vx}/{weapon.Runtime.Vy}, " +
                $"flight={weapon.Runtime.WeaponFlightCounter}, attacking={weapon.AttackingCounter}");

            return new LandingEvidence
            {
                inputVy = landingVy,
                outputFrame = weapon.Frame.N,
                outputVx = weapon.Runtime.Vx,
                outputVy = weapon.Runtime.Vy,
                outputY = weapon.Runtime.Y,
                outputFlightCounter = weapon.Runtime.WeaponFlightCounter,
                outputAttacking = weapon.AttackingCounter,
                matched = true,
            };
        }

        private static void ExecuteNoImmediateLandingHitWitness()
        {
            ProbeWeapon landingWeapon = null;
            ProbeCharacter target = null;
            try
            {
                landingWeapon = ProbeWeapon.CreateBurningLandingProbe();
                target = new ProbeCharacter("R8LandingOverlapTarget", cover: 0);
                RegisterOwned(landingWeapon);
                RegisterOwned(target);
                landingWeapon.Runtime.SetPosition(500.0, 0.0, 300.0);
                landingWeapon.Runtime.SetVelocity(10.0, 18.0, 0.0);
                target.Runtime.SetPosition(500.0, 0.0, 300.0);
                target.Runtime.SyncIntegerPosition();
                target.Health.HP = 333;
                target.Health.HPBound = 444;
                int targetFrameBefore = target.Frame.N;

                landingWeapon.InvokeOnLanded();
                result.noImmediateHit =
                    target.Health.HP == 333 &&
                    target.Health.HPBound == 444 &&
                    target.Frame.N == targetFrameBefore;
                result.overlapTargetHpBefore = 333;
                result.overlapTargetHpAfter = target.Health.HP;
                result.overlapTargetFrameBefore = targetFrameBefore;
                result.overlapTargetFrameAfter = target.Frame.N;
                Require(result.noImmediateHit,
                    "Weapon landing directly mutated an overlapping target.");
            }
            finally
            {
                UnregisterOwned(target, "landing-target");
                UnregisterOwned(landingWeapon, "landing-weapon");
            }
        }

        private static string ValidateCatalog()
        {
            for (int index = 0; index < Specs.Length; index++)
            {
                WeaponSpec spec = Specs[index];
                ObjectDefinition definition =
                    GameDataManager.Instance.GetObjectById(spec.Oid);
                if (definition == null)
                    return $"Production data.txt catalog is missing oid {spec.Oid}.";
                if (definition.type != spec.DataType)
                {
                    return $"Production oid {spec.Oid} type is {definition.type}, " +
                           $"expected {spec.DataType}.";
                }
            }
            return string.Empty;
        }

        private static void CaptureBaseline()
        {
            result.startTick = driver.CurrentTickIndex;
            result.workerWasActive =
                driver.DedicatedSimulationWorkerActiveForDiagnostics;
            result.baselineObjectCount = world.ObjectCount;
            result.baselineClaimedSlots =
                world.ClaimedRuntimeSlotCountForDiagnostics;
            result.baselineObjectPoolActive =
                objectPool.ActiveObjectCountForAcceptance;
            result.baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineCaptured = true;
        }

        private static void FinishSuccess()
        {
            result.status = "PASS";
            result.message =
                "Live pickup, held pose, type-specific throw, landing, and " +
                "no-immediate-hit writers passed.";
            result.endTick = driver.CurrentTickIndex;
            result.weapons = EvidenceRows.ToArray();
            CleanupOwnedEntities();
            CaptureFinalState();
            Require(result.cleanupCompleted,
                "Probe cleanup did not restore the live-world baseline: " +
                result.cleanupErrors);
            WriteResult(result);
            Debug.Log(
                $"[BattleHeldWeaponLifecyclePlayModeProbe] PASS: " +
                $"weapons={result.weapons.Length}, tick={result.startTick}.");
            StopObservation();
        }

        private static void Fail(string message)
        {
            result ??= new ProbeResult();
            result.status = "FAIL";
            result.message = message;
            result.endTick = driver?.CurrentTickIndex ?? -1;
            result.weapons = EvidenceRows.ToArray();
            CleanupOwnedEntities();
            CaptureFinalState();
            WriteResult(result);
            Debug.LogError(
                "[BattleHeldWeaponLifecyclePlayModeProbe] FAIL: " + message);
            StopObservation();
        }

        private static void RegisterOwned(LF2Entity entity)
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

        private static void CaptureFinalState()
        {
            if (result == null)
                return;
            result.finalObjectCount = world?.ObjectCount ?? -1;
            result.finalClaimedSlots =
                world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            result.finalObjectPoolActive =
                objectPool?.ActiveObjectCountForAcceptance ?? -1;
            result.finalLogicPoolActive =
                LF2ReferencePool.Instance?.ActiveCount ?? -1;
            result.cleanupCompleted =
                baselineCaptured &&
                string.IsNullOrEmpty(result.cleanupErrors) &&
                result.finalObjectCount == result.baselineObjectCount &&
                result.finalClaimedSlots == result.baselineClaimedSlots &&
                result.finalObjectPoolActive == result.baselineObjectPoolActive &&
                result.finalLogicPoolActive == result.baselineLogicPoolActive;
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
            ProbeResult failure = new ProbeResult
            {
                status = "FAIL",
                message = message,
                startTick = driver?.CurrentTickIndex ?? -1,
                endTick = driver?.CurrentTickIndex ?? -1,
                weapons = Array.Empty<WeaponEvidence>(),
            };
            WriteResult(failure);
            Debug.LogError(
                "[BattleHeldWeaponLifecyclePlayModeProbe] FAIL: " + message);
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
            previousPaused = false;
            pauseRequested = false;
            baselineCaptured = false;
            running = false;
            editorUpdates = 0;
            OwnedEntities.Clear();
            EvidenceRows.Clear();
        }

        private readonly struct WeaponSpec
        {
            public WeaponSpec(
                int oid,
                int dataType,
                int holderLink,
                int pickupFrame,
                int targetLink,
                bool stampSpawner)
            {
                Oid = oid;
                DataType = dataType;
                HolderLink = holderLink;
                PickupFrame = pickupFrame;
                TargetLink = targetLink;
                StampSpawner = stampSpawner;
            }

            public int Oid { get; }
            public int DataType { get; }
            public int HolderLink { get; }
            public int PickupFrame { get; }
            public int TargetLink { get; }
            public bool StampSpawner { get; }
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status;
            public string message;
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
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public bool noImmediateHit;
            public int overlapTargetHpBefore;
            public int overlapTargetHpAfter;
            public int overlapTargetFrameBefore;
            public int overlapTargetFrameAfter;
            public WeaponEvidence[] weapons;
        }

        [Serializable]
        private sealed class WeaponEvidence
        {
            public int oid;
            public int dataType;
            public int holderSlot;
            public int weaponSlot;
            public bool pickupAccepted;
            public int pickupFrame;
            public int holderLinkAfterPickup;
            public int weaponLinkAfterPickup;
            public int pickupCount;
            public int cover;
            public int heldFrame;
            public int heldFrameDelay;
            public int heldX;
            public int heldY;
            public int heldZ;
            public int throwFrame;
            public bool throwFrameMatched;
            public int throwFrameDelay;
            public double throwVx;
            public double throwVy;
            public double throwVz;
            public int spawnerAfterThrow;
            public int expectedSpawnerAfterThrow;
            public int pickerAfterThrow;
            public bool relationshipsReleased;
            public LandingEvidence landing;
        }

        [Serializable]
        private sealed class LandingEvidence
        {
            public double inputVy;
            public int outputFrame;
            public double outputVx;
            public double outputVy;
            public double outputY;
            public int outputFlightCounter;
            public int outputAttacking;
            public bool matched;
        }

        private sealed class ProbeCharacter : LF2Character
        {
            public ProbeCharacter(string probeName, int cover)
            {
                Name = probeName;
                ObjectId = HolderOid;
                LF2CharacterData data = new LF2CharacterData
                {
                    name = probeName,
                    type_sub = (int)LF2ObjectType.Character,
                    frames = new List<LF2FrameData>
                    {
                        HolderFrame(0, cover, false),
                        HolderFrame(115, cover, false),
                        HolderFrame(116, cover, false),
                        HolderFrame(117, cover, true),
                    },
                };
                FrameCache.Load(new LF2CharacterDataWrapper(HolderOid, data));
                ImmediateFrame(0);
                Runtime.SetPosition(100.0, -10.0, 200.0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 500;
                Health.HPBound = 500;
                Health.HP3 = 500;
            }

            private static LF2FrameData HolderFrame(
                int frameId,
                int cover,
                bool throwing)
            {
                LF2FrameData frame = new LF2FrameData
                {
                    frameId = frameId,
                    state = LF2States.Standing,
                    wait = 10000,
                    next = frameId,
                    pic = 0,
                    centerx = 39,
                    centery = 79,
                };
                frame.wpoints = new List<WeaponPoint>
                {
                    new WeaponPoint
                    {
                        kind = 1,
                        x = 50,
                        y = 60,
                        weaponact = 0,
                        cover = cover,
                        dvx = throwing ? 12 : 0,
                        dvy = throwing ? -4 : 0,
                        dvz = throwing ? 3 : 0,
                    },
                };
                return frame;
            }
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            public ProbeWeapon(
                string probeName,
                int oid,
                int dataType)
            {
                Name = probeName;
                ObjectId = oid;
                SetWeaponType(dataType);
                LF2CharacterData data = BuildWeaponData(probeName, dataType);
                FrameCache.Load(new LF2CharacterDataWrapper(oid, data));
                ImmediateFrame(0);
                Runtime.SetPosition(120.0, 0.0, 200.0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 500;
                Health.HPBound = 500;
                Health.HP3 = 500;
            }

            public bool InvokeCurrentDatLanding(
                int dataType,
                LF2FrameData frame,
                double landingVy,
                bool crossedGround)
            {
                return ApplyCurrentDatNonCharacterLanding(
                    dataType,
                    frame,
                    landingVy,
                    crossedGround);
            }

            public void InvokeOnLanded()
            {
                base.OnLanded();
            }

            public static ProbeWeapon CreateBurningLandingProbe()
            {
                ProbeWeapon weapon = new ProbeWeapon(
                    "R8NoImmediateLandingHitWeapon",
                    7900,
                    (int)LF2ObjectType.Character);
                LF2FrameData burning = new LF2FrameData
                {
                    frameId = 0,
                    state = LF2States.Burning,
                    wait = 10000,
                    next = 0,
                    pic = 0,
                    centerx = 20,
                    centery = 20,
                };
                burning.bodies.Add(new BodyBox
                {
                    x = 0,
                    y = 0,
                    w = 40,
                    h = 40,
                });
                burning.itrs.Add(new InteractionArea
                {
                    kind = 0,
                    x = 0,
                    y = 0,
                    w = 40,
                    h = 40,
                    injury = 50,
                });
                LF2CharacterData data = new LF2CharacterData
                {
                    name = weapon.Name,
                    type_sub = (int)LF2ObjectType.Character,
                    weapon_drop_hurt = 10,
                    frames = new List<LF2FrameData> { burning },
                };
                weapon.FrameCache.Load(new LF2CharacterDataWrapper(7900, data));
                weapon.ImmediateFrame(0);
                weapon.Health.HP = 1000;
                return weapon;
            }

            private static LF2CharacterData BuildWeaponData(
                string name,
                int dataType)
            {
                List<LF2FrameData> frames = new List<LF2FrameData>();
                for (int frameId = 0; frameId < 6; frameId++)
                {
                    frames.Add(WeaponFrame(
                        frameId,
                        dataType == (int)LF2ObjectType.HeavyWeapon
                            ? LF2States.HeavyWeaponInSky
                            : LF2States.WeaponOnHand));
                }
                frames.Add(WeaponFrame(7, LF2States.WeaponInSky));
                frames.Add(WeaponFrame(20, LF2States.HeavyWeaponOnGround));
                frames.Add(WeaponFrame(40, LF2States.WeaponThrowing));
                frames.Add(WeaponFrame(60, LF2States.WeaponOnGround));
                frames.Add(WeaponFrame(70, LF2States.WeaponJustOnGround));
                return new LF2CharacterData
                {
                    name = name,
                    type_sub = dataType,
                    weapon_hp = 100,
                    weapon_drop_hurt = 3,
                    frames = frames,
                };
            }

            private static LF2FrameData WeaponFrame(int frameId, int state)
            {
                LF2FrameData frame = new LF2FrameData
                {
                    frameId = frameId,
                    state = state,
                    wait = 10000,
                    next = frameId,
                    pic = 0,
                    centerx = 20,
                    centery = 20,
                };
                frame.wpoints = new List<WeaponPoint>
                {
                    new WeaponPoint
                    {
                        kind = 2,
                        x = 5,
                        y = 6,
                        weaponact = 0,
                    },
                };
                return frame;
            }
        }
    }
}
#endif
