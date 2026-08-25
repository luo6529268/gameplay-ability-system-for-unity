#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Live-world S4 probe for the HP-zero AI boundary, state14 death countdown,
    /// integer-coordinate respawn, stored-count respawn effect, and free branch.
    /// Alignment contract: R8-DEATHPLAY-001.
    /// </summary>
    public static class BattleDeathRespawnAiIntegerPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Validation/R8/Run Death Respawn AI Integer Play Probe";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01C_05_DeathRespawnAiInteger.result.json";
        private const int TickTimeoutEditorUpdates = 1800;
        private const int ProbeOidBase = 8300;
        private const int NoCountRelation = 770077;
        private const int StoredRelation = 880088;
        private const int FreeRelation = 990099;

        private static readonly List<LF2Entity> OwnedFixtures =
            new List<LF2Entity>(8);
        private static readonly List<LF2Entity> OwnedSpawnedEntities =
            new List<LF2Entity>(4);
        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(16);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool objectPool;
        private static ProbeResult result;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;
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
                workerWasActive =
                    driver.DedicatedSimulationWorkerActiveForDiagnostics,
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
            catch (TargetInvocationException exception)
            {
                Fail(
                    "Production input invocation failed: " +
                    (exception.InnerException ?? exception));
            }
            catch (Exception exception)
            {
                Fail("Unhandled probe exception: " + exception);
            }
        }

        private static MatrixEvidence ExecuteMatrix()
        {
            FixtureSet fixtures = BuildFixtures();
            int tick = driver.CurrentTickIndex + 500;
            var states = new List<StateEvidence>(8)
            {
                State("death-checkpoint", tick, fixtures.noCount),
            };

            InvokeKnownCharacterInput(fixtures.noCount, tick);
            Require(
                fixtures.noCount.Runtime.PrevJump == 1 &&
                fixtures.noCount.Runtime.KeyJump == 0 &&
                fixtures.noCount.Runtime.CdAttack == 5 &&
                fixtures.noCount.Frame.N == 14,
                "HP=0 AI did not run the pre-cleanup input roll/clear contract.");
            states.Add(State("after-ai-input", tick, fixtures.noCount));

            fixtures.noCount.SimFrameTick(tick);
            Require(
                fixtures.noCount.HitStun == 30 &&
                fixtures.noCount.AttackingCounter == 0,
                "state14 HP<=0 did not arm HitStun=30 and clear attacking.");
            states.Add(State("after-state14-arm", tick, fixtures.noCount));

            int countdownTicks = 0;
            while (fixtures.noCount.HitStun > 4 && countdownTicks < 40)
            {
                countdownTicks++;
                fixtures.noCount.SimFrameTick(tick + countdownTicks);
            }
            Require(
                countdownTicks == 26 && fixtures.noCount.HitStun == 4,
                $"state14 hit-stop countdown mismatch: ticks={countdownTicks}, " +
                $"hitStun={fixtures.noCount.HitStun}.");
            states.Add(State(
                "respawn-gate-ready",
                tick + countdownTicks,
                fixtures.noCount));

            fixtures.noCount.Runtime.LinkState = 7;
            fixtures.noCount.HolderCopySlot = 123;
            fixtures.noCount.Runtime.TargetSlotIndex = 124;

            int noCountSlot = fixtures.noCount.Runtime.SlotIndex;
            int storedSlot = fixtures.stored.Runtime.SlotIndex;
            int freeSlot = fixtures.free.Runtime.SlotIndex;
            int expectedAverageX =
                (fixtures.allyA.Runtime.XInt + fixtures.allyB.Runtime.XInt) / 2;
            int expectedAverageZ =
                (fixtures.allyA.Runtime.ZInt + fixtures.allyB.Runtime.ZInt) / 2;
            var expectedRng = new DeterministicRng(world.Rng.State);
            int expectedX = expectedAverageX + expectedRng.NextInt(0, 51) - 26;
            int expectedZ = expectedAverageZ + expectedRng.NextInt(0, 31) - 16;
            ulong rngCallsBeforeCleanup = world.Rng.CallCount;

            world.PostFrameAdvanceDeathCleanupAll(tick + countdownTicks);

            Require(
                fixtures.noCount.HP2Orig == 2 &&
                fixtures.noCount.Health.HP == 180 &&
                fixtures.noCount.Health.HPBound == 180 &&
                fixtures.noCount.Health.PP == 500 &&
                fixtures.noCount.Frame.N == 212 &&
                fixtures.noCount.HitStun == 20 &&
                fixtures.noCount.Runtime.YInt == -300 &&
                Math.Abs(fixtures.noCount.Runtime.Vy) < 0.0001,
                "no-count respawn post-state mismatch.");
            Require(
                fixtures.noCount.Runtime.XInt == expectedX &&
                fixtures.noCount.Runtime.ZInt == expectedZ &&
                world.Rng.CallCount == rngCallsBeforeCleanup + 2,
                $"no-count respawn integer/RNG mismatch: expected=({expectedX}," +
                $"{expectedZ}) actual=({fixtures.noCount.Runtime.XInt}," +
                $"{fixtures.noCount.Runtime.ZInt}) calls=" +
                $"{world.Rng.CallCount - rngCallsBeforeCleanup}.");
            Require(
                fixtures.allyA.Runtime.XInt == 100 &&
                fixtures.allyB.Runtime.XInt == 160,
                "respawn scan resynchronized stale integer coordinates.");
            Require(
                fixtures.noCount.RelationTeam == NoCountRelation &&
                fixtures.noCount.Runtime.LinkState == 7 &&
                fixtures.noCount.HolderCopySlot == 123 &&
                fixtures.noCount.Runtime.TargetSlotIndex == 124,
                "no-count respawn wrote relation/link/holder/target outside C++ writers.");
            states.Add(State(
                "after-no-count-respawn",
                tick + countdownTicks,
                fixtures.noCount));

            Require(
                fixtures.stored.HP2Orig == 6 &&
                fixtures.stored.HPOrig == 0 &&
                fixtures.stored.Health.PP == 0 &&
                fixtures.stored.Health.HP == 80 &&
                fixtures.stored.Health.HPBound == 80 &&
                fixtures.stored.Health.HP3 == 80 &&
                fixtures.stored.RespawnCount == 0 &&
                fixtures.stored.RelationTeam == 1 &&
                fixtures.stored.Runtime.RenderPicOffset == 0x8C &&
                fixtures.stored.Frame.N == 0xDB &&
                fixtures.stored.FrameDelay == 0xA &&
                fixtures.stored.AttackingCounter == 0,
                "stored-count respawn post-state mismatch.");
            Require(
                fixtures.stored.Runtime.LinkState == 8 &&
                fixtures.stored.HolderCopySlot == 125 &&
                fixtures.stored.Runtime.TargetSlotIndex == 126,
                "stored-count respawn wrote link/holder/target outside C++ writers.");

            LF2Entity respawnEffect = FindRespawnEffect(storedSlot);
            Require(respawnEffect != null,
                "stored-count respawn did not create production OID998 effect.");
            OwnedSpawnedEntities.Add(respawnEffect);
            Require(
                respawnEffect.ObjectId == 998 &&
                respawnEffect.Frame.N == 6 &&
                respawnEffect.Runtime.XInt == 77 &&
                respawnEffect.Runtime.YInt == -12 &&
                respawnEffect.Runtime.ZInt == 20 &&
                respawnEffect.RelationTeam == 1 &&
                respawnEffect.SpawnerEntityIndex == storedSlot,
                "OID998 respawn effect field/position contract mismatch.");
            states.Add(State(
                "after-stored-count-respawn",
                tick + countdownTicks,
                fixtures.stored));

            Require(
                world.FindEntityByRuntimeSlotForQuery(freeSlot) == null &&
                fixtures.free.Runtime.SlotIndex == -1,
                "HP2Orig<2 free branch did not unregister and release its slot.");
            states.Add(new StateEvidence
            {
                checkpoint = "after-free",
                tick = tick + countdownTicks,
                slot = freeSlot,
                active = false,
                frame = 14,
                hp = 0,
            });

            return new MatrixEvidence
            {
                tick = tick,
                noCountSlot = noCountSlot,
                storedSlot = storedSlot,
                freeSlot = freeSlot,
                effectSlot = respawnEffect.Runtime.SlotIndex,
                countdownTicks = countdownTicks,
                expectedAverageX = expectedAverageX,
                expectedAverageZ = expectedAverageZ,
                expectedRespawnX = expectedX,
                expectedRespawnZ = expectedZ,
                actualRespawnX = fixtures.noCount.Runtime.XInt,
                actualRespawnZ = fixtures.noCount.Runtime.ZInt,
                rngCalls = world.Rng.CallCount - rngCallsBeforeCleanup,
                aiInputExecuted = true,
                noCountPassed = true,
                storedCountPassed = true,
                freePassed = true,
                relationLinkBoundaryPassed = true,
                effectPassed = true,
                states = states.ToArray(),
            };
        }

        private static FixtureSet BuildFixtures()
        {
            int oid = ProbeOidBase;
            var fixtures = new FixtureSet
            {
                noCount = RegisterOwned(new ProbeCharacter(
                    "R8C05_NoCount", oid++)),
                allyA = RegisterOwned(new ProbeCharacter(
                    "R8C05_AllyA", oid++)),
                allyB = RegisterOwned(new ProbeCharacter(
                    "R8C05_AllyB", oid++)),
                stored = RegisterOwned(new ProbeCharacter(
                    "R8C05_Stored", 0x1E)),
                free = RegisterOwned(new ProbeCharacter(
                    "R8C05_Free", oid++)),
            };

            fixtures.noCount.RelationTeam = NoCountRelation;
            fixtures.noCount.KillCount = 0;
            fixtures.noCount.AiControlled = true;
            fixtures.noCount.DirectWriteFramePreserveWaitCounter(14);
            fixtures.noCount.Health.HP = 0;
            fixtures.noCount.Health.HP3 = 180;
            fixtures.noCount.Health.HPBound = 60;
            fixtures.noCount.Health.PP = 12;
            fixtures.noCount.HP2Orig = 3;
            fixtures.noCount.HitStun = 0;
            fixtures.noCount.AttackingCounter = 9;
            fixtures.noCount.SetPosition(40, 0, 5, true);
            fixtures.noCount.Runtime.SetVelocity(0, -7, 0);
            fixtures.noCount.Runtime.KeyJump = 1;
            fixtures.noCount.Runtime.PrevJump = 0;
            fixtures.noCount.Runtime.CdAttack = 5;
            fixtures.noCount.Runtime.Unk3FC = 40;
            fixtures.noCount.Runtime.Unk400 = 5;

            fixtures.allyA.RelationTeam = NoCountRelation;
            fixtures.allyB.RelationTeam = NoCountRelation;
            fixtures.allyA.SetPosition(100, 0, 40, true);
            fixtures.allyB.SetPosition(160, 0, 20, true);
            fixtures.allyA.SetPosition(1100, 0, 1040, false);
            fixtures.allyB.SetPosition(2160, 0, 2020, false);

            fixtures.stored.RelationTeam = StoredRelation;
            fixtures.stored.KillCount = 0;
            fixtures.stored.DirectWriteFramePreserveWaitCounter(14);
            fixtures.stored.Health.HP = 0;
            fixtures.stored.Health.PP = 77;
            fixtures.stored.Health.HPBound = 10;
            fixtures.stored.Health.HP3 = 10;
            fixtures.stored.HPOrig = 6;
            fixtures.stored.HP2Orig = 4;
            fixtures.stored.RespawnCount = 80;
            fixtures.stored.AttackingCounter = 9;
            fixtures.stored.HitStun = 3;
            fixtures.stored.Runtime.LinkState = 8;
            fixtures.stored.HolderCopySlot = 125;
            fixtures.stored.Runtime.TargetSlotIndex = 126;
            fixtures.stored.SetPosition(77, -12, 19, true);

            fixtures.free.RelationTeam = FreeRelation;
            fixtures.free.KillCount = 0;
            fixtures.free.DirectWriteFramePreserveWaitCounter(14);
            fixtures.free.Health.HP = 0;
            fixtures.free.HP2Orig = 1;
            fixtures.free.HitStun = 2;
            fixtures.free.SetPosition(33, 0, 12, true);

            return fixtures;
        }

        private static void InvokeKnownCharacterInput(
            LF2Character character,
            int tick)
        {
            MethodInfo method = typeof(LF2Character).GetMethod(
                "RunCharacterInputPhaseForKnownCharacterDat",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(method != null,
                "The production known-character input entry is unavailable.");
            method.Invoke(character, new object[] { tick });
        }

        private static LF2Entity FindRespawnEffect(int spawnerSlot)
        {
            for (int slot = 0;
                 slot < world.RuntimeSlotCapacityForDiagnostics;
                 slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity != null && entity.ObjectId == 998 &&
                    entity.SpawnerEntityIndex == spawnerSlot)
                {
                    return entity;
                }
            }
            return null;
        }

        private static T RegisterOwned<T>(T entity)
            where T : LF2Entity
        {
            world.Register(entity);
            Require(entity.Runtime?.SlotIndex >= 0,
                (entity?.Name ?? "entity") + " has no runtime slot.");
            OwnedFixtures.Add(entity);
            return entity;
        }

        private static void CaptureBaseline()
        {
            baselineCaptured = true;
            result.startTick = driver.CurrentTickIndex;
            result.baselineObjectCount = world.ObjectCount;
            result.baselineClaimedSlots =
                world.ClaimedRuntimeSlotCountForDiagnostics;
            result.baselineObjectPoolActive =
                objectPool.ActiveObjectCountForAcceptance;
            result.baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineRngState = world.Rng.State;
            baselineRngCalls = world.Rng.CallCount;
            BaselineSounds.Clear();
            BaselineSounds.AddRange(world.PendingSounds);
        }

        private static void CleanupOwnedEntities()
        {
            if (world == null)
                return;

            for (int index = OwnedSpawnedEntities.Count - 1; index >= 0; index--)
            {
                LF2Entity entity = OwnedSpawnedEntities[index];
                try
                {
                    if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                        entity.FreeEntityLikeExe();
                }
                catch (Exception exception)
                {
                    AppendCleanupError(entity?.Name ?? "spawned", exception);
                }
            }
            OwnedSpawnedEntities.Clear();

            for (int index = OwnedFixtures.Count - 1; index >= 0; index--)
            {
                LF2Entity entity = OwnedFixtures[index];
                try
                {
                    if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                        world.Unregister(entity);
                }
                catch (Exception exception)
                {
                    AppendCleanupError(entity?.Name ?? "fixture", exception);
                }
            }
            OwnedFixtures.Clear();

            try
            {
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                AppendCleanupError("flush", exception);
            }

            world.PendingSounds.Clear();
            world.PendingSounds.AddRange(BaselineSounds);
            world.Rng.RestoreState(baselineRngState, baselineRngCalls);
        }

        private static void FinishSuccess()
        {
            result.status = "PASS";
            result.message =
                "Live death, HP-zero AI, state14, integer respawn, stored " +
                "effect, relation/link boundary and free matrices passed.";
            result.endTick = driver.CurrentTickIndex;
            CleanupOwnedEntities();
            CaptureFinalState();
            Require(result.cleanupCompleted,
                "Probe cleanup did not restore the live-world baseline: " +
                result.cleanupErrors);
            WriteResult(result);
            Debug.Log(
                $"[BattleDeathRespawnAiIntegerPlayModeProbe] PASS: " +
                $"tick={result.startTick}, respawn=({result.matrix.actualRespawnX}," +
                $"{result.matrix.actualRespawnZ}).");
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
            Debug.LogError(
                "[BattleDeathRespawnAiIntegerPlayModeProbe] FAIL: " + message);
            StopObservation();
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
            result.rngRestored = world?.Rng != null &&
                world.Rng.State == baselineRngState &&
                world.Rng.CallCount == baselineRngCalls;
            result.pendingSoundsRestored =
                PendingSoundsEqual(world?.PendingSounds, BaselineSounds);
            result.cleanupCompleted =
                baselineCaptured &&
                string.IsNullOrEmpty(result.cleanupErrors) &&
                result.finalObjectCount == result.baselineObjectCount &&
                result.finalClaimedSlots == result.baselineClaimedSlots &&
                result.finalObjectPoolActive == result.baselineObjectPoolActive &&
                result.finalLogicPoolActive == result.baselineLogicPoolActive &&
                result.rngRestored &&
                result.pendingSoundsRestored;
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

        private static StateEvidence State(
            string checkpoint,
            int tick,
            LF2Entity entity)
        {
            return new StateEvidence
            {
                checkpoint = checkpoint,
                tick = tick,
                slot = entity.Runtime?.SlotIndex ?? -1,
                active = entity.Match == world && entity.Runtime?.SlotIndex >= 0,
                frame = entity.Frame?.N ?? -1,
                state = entity.Frame?.D?.state ?? -1,
                hp = entity.Health?.HP ?? 0,
                hpBound = entity.Health?.HPBound ?? 0,
                pp = entity.Health?.PP ?? 0,
                hitStun = entity.HitStun,
                attacking = entity.AttackingCounter,
                xInt = entity.Runtime?.XInt ?? 0,
                yInt = entity.Runtime?.YInt ?? 0,
                zInt = entity.Runtime?.ZInt ?? 0,
                keyJump = entity.Runtime?.KeyJump ?? 0,
                prevJump = entity.Runtime?.PrevJump ?? 0,
                relation = entity.RelationTeam,
                link = entity.Runtime?.LinkState ?? 0,
                holder = entity.HolderCopySlot,
                target = entity.Runtime?.TargetSlotIndex ?? -1,
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void AppendCleanupError(string label, Exception exception)
        {
            if (result != null)
                result.cleanupErrors += label + ":" + exception.Message + ";";
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
                "[BattleDeathRespawnAiIntegerPlayModeProbe] FAIL: " + message);
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
            baselineRngState = 0;
            baselineRngCalls = 0;
            previousPaused = false;
            pauseRequested = false;
            baselineCaptured = false;
            running = false;
            editorUpdates = 0;
            OwnedFixtures.Clear();
            OwnedSpawnedEntities.Clear();
            BaselineSounds.Clear();
        }

        private sealed class ProbeCharacter : LF2Character
        {
            public ProbeCharacter(string name, int objectId)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(
                    objectId,
                    BuildCharacterData(name)));
                ImmediateFrame(0);
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 100;
                Health.HPBound = 100;
                Health.HP3 = 100;
                KillCount = -1;
            }

            public void SetPosition(int x, int y, int z, bool syncInteger)
            {
                Runtime.SetPosition(x, y, z);
                if (syncInteger)
                    Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }
        }

        private static LF2CharacterData BuildCharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing),
                    Frame(6, LF2States.Standing),
                    Frame(14, LF2States.Lying),
                    Frame(212, LF2States.Jump),
                    Frame(219, LF2States.Standing),
                },
            };
        }

        private static LF2FrameData Frame(int frameId, int state)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = frameId,
                pic = 999,
                centerx = 0,
                centery = 0,
            };
        }

        private sealed class FixtureSet
        {
            public ProbeCharacter noCount;
            public ProbeCharacter allyA;
            public ProbeCharacter allyB;
            public ProbeCharacter stored;
            public ProbeCharacter free;
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
            public bool rngRestored;
            public bool pendingSoundsRestored;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public MatrixEvidence matrix;
        }

        [Serializable]
        private sealed class MatrixEvidence
        {
            public int tick;
            public int noCountSlot;
            public int storedSlot;
            public int freeSlot;
            public int effectSlot;
            public int countdownTicks;
            public int expectedAverageX;
            public int expectedAverageZ;
            public int expectedRespawnX;
            public int expectedRespawnZ;
            public int actualRespawnX;
            public int actualRespawnZ;
            public ulong rngCalls;
            public bool aiInputExecuted;
            public bool noCountPassed;
            public bool storedCountPassed;
            public bool freePassed;
            public bool relationLinkBoundaryPassed;
            public bool effectPassed;
            public StateEvidence[] states = Array.Empty<StateEvidence>();
        }

        [Serializable]
        private sealed class StateEvidence
        {
            public string checkpoint = string.Empty;
            public int tick;
            public int slot;
            public bool active;
            public int frame;
            public int state;
            public int hp;
            public int hpBound;
            public int pp;
            public int hitStun;
            public int attacking;
            public int xInt;
            public int yInt;
            public int zInt;
            public int keyJump;
            public int prevJump;
            public int relation;
            public int link;
            public int holder;
            public int target;
        }
    }
}
#endif
