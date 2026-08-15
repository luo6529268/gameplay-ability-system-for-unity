using System;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using UnityEngine;

namespace NTSD.Test
{
    [Serializable]
    public sealed class BattleSinglePlayerRuntimeValidationReport
    {
        public string runId;
        public string status;
        public string failure;
        public string unityVersion;
        public string platform;
        public string scriptingBackend;
        public string sourceChecksum;
        public string restoredChecksum;
        public string replayChecksum;
        public int restoredSlot;
        public int restoredStableId;
        public uint restoredGeneration;
        public bool pureValueTransferPassed;
        public bool restoreReplayPassed;
    }

    internal static class BattleSinglePlayerRuntimeValidation
    {
        private const int TransferObjectId = 31997;
        private const int TransferSlot = 3;

        internal static BattleSinglePlayerRuntimeValidationReport Run(string runId)
        {
            var report = new BattleSinglePlayerRuntimeValidationReport
            {
                runId = runId ?? string.Empty,
                status = "Running",
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
#if ENABLE_IL2CPP
                scriptingBackend = "IL2CPP",
#else
                scriptingBackend = "Mono",
#endif
                restoredSlot = -1,
            };

            try
            {
                ValidatePureValueSnapshotTransfer(report);
                ValidateRestoreAndReplay(report);
                report.status = "Passed";
            }
            catch (Exception exception)
            {
                report.status = "Failed";
                report.failure = exception.ToString();
            }

            return report;
        }

        private static void ValidatePureValueSnapshotTransfer(
            BattleSinglePlayerRuntimeValidationReport report)
        {
            LockstepSessionIdentity identity = CreateIdentity();
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            };
            var data = new LF2CharacterData();
            data.frames.Add(frame);
            var wrapper = new LF2CharacterDataWrapper(TransferObjectId, data);
            var definitions = new[]
            {
                new ObjectDefinition(
                    TransferObjectId,
                    (int)LF2ObjectType.Character,
                    "snapshot-transfer.dat"),
            };

            var source = new SimulationWorld();
            source.PrepareRuntimeDataCatalogForBattle(
                definitions,
                id => id == TransferObjectId ? wrapper : null);
            source.SetLogicOnlyEntityMaterialization(true);
            OPointCreateTask task = source.LogicReferencePool.Fetch<OPointCreateTask>();
            task.targetWorld = source;
            task.requiredRuntimeSlot = TransferSlot;
            task.opoint = new ObjectPoint
            {
                oid = TransferObjectId,
                kind = 1,
                action = 0,
            };
            task.dir = "right";
            task.preserveActionZero = true;
            task.useDirectRuntimePosition = true;
            task.skipPostInitZOffset = true;
            LF2Entity sourceEntity;
            BattleLogicEntityCreationFailure creationFailure;
            try
            {
                sourceEntity = source.LogicEntityFactory.Create(task, out creationFailure);
            }
            finally
            {
                source.LogicReferencePool.Recycle(task);
            }
            Require(
                sourceEntity != null,
                "Pure-value source entity creation failed: " + creationFailure);

            sourceEntity.Runtime.X = 123.25;
            sourceEntity.Runtime.Y = -7.5;
            sourceEntity.Runtime.Z = 211.75;
            sourceEntity.Runtime.HP = 333;
            sourceEntity.Runtime.PP = 207;
            sourceEntity.Runtime.ComboCountAtk = 4;
            source.Runtime.Flow.AiRand15 = 11;
            source.Rng.Seed(917u);
            source.Rng.NextInt(0, 97);

            BattleLockstepChecksumSnapshot sourceChecksum =
                source.CaptureLockstepChecksumSnapshot(0);
            report.sourceChecksum = sourceChecksum.OverallChecksum;
            BattleStateSnapshotBuffer snapshot =
                source.CreateBattleStateSnapshotBufferForBootstrap();
            Require(
                source.TryCaptureBattleStateSnapshot(identity, 0, snapshot),
                "Pure-value source snapshot capture failed.");
            int expectedStableId = sourceEntity.Runtime.StableId;
            snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();

            var destination = new SimulationWorld();
            destination.PrepareRuntimeDataCatalogForBattle(
                definitions,
                id => id == TransferObjectId ? wrapper : null);
            destination.SetLogicOnlyEntityMaterialization(true);
            Require(
                destination.TryRestoreBattleStateSnapshot(
                    identity,
                    snapshot,
                    out BattleStateSnapshotRestoreFailure restoreFailure),
                "Pure-value destination restore failed: " + restoreFailure);
            Require(
                destination.TryGetRuntimeSlotReadOnlyView(
                    TransferSlot,
                    out RuntimeSlotTable.ReadOnlySlotView restoredView),
                "Restored runtime slot is unavailable.");
            Require(restoredView.Claimed, "Restored runtime slot is not claimed.");
            Require(
                restoredView.Entity is LF2Character,
                "Restored entity is not an LF2Character.");
            Require(
                !ReferenceEquals(restoredView.Entity, sourceEntity),
                "Pure-value transfer reused the source CLR entity shell.");
            Require(restoredView.Entity.Renderer == null,
                "Logic-only restored entity unexpectedly owns a Unity renderer.");
            Require(restoredView.Entity.Runtime.StableId == expectedStableId,
                "Restored stable id differs from the snapshot.");
            Require(restoredView.Entity.Runtime.X == 123.25,
                "Restored X differs from the snapshot.");
            Require(restoredView.Entity.Runtime.Y == -7.5,
                "Restored Y differs from the snapshot.");
            Require(restoredView.Entity.Runtime.Z == 211.75,
                "Restored Z differs from the snapshot.");
            Require(restoredView.Entity.Runtime.HP == 333,
                "Restored HP differs from the snapshot.");
            Require(restoredView.Entity.Runtime.PP == 207,
                "Restored PP differs from the snapshot.");
            Require(restoredView.Entity.Runtime.ComboCountAtk == 4,
                "Restored combo statistic differs from the snapshot.");
            Require(destination.Runtime.Flow.AiRand15 == 11,
                "Restored world flow differs from the snapshot.");
            Require(destination.Rng.State == snapshot.Core.RngState,
                "Restored RNG state differs from the snapshot.");
            Require(destination.Rng.CallCount == snapshot.Core.RngCallCount,
                "Restored RNG call count differs from the snapshot.");

            BattleLockstepChecksumSnapshot restoredChecksum =
                destination.CaptureLockstepChecksumSnapshot(0);
            report.restoredChecksum = restoredChecksum.OverallChecksum;
            Require(
                string.Equals(
                    report.sourceChecksum,
                    report.restoredChecksum,
                    StringComparison.Ordinal),
                "Pure-value transfer changed the lockstep checksum.");

            report.restoredSlot = TransferSlot;
            report.restoredStableId = restoredView.Entity.Runtime.StableId;
            report.restoredGeneration = restoredView.Generation;
            report.pureValueTransferPassed = true;
        }

        private static void ValidateRestoreAndReplay(
            BattleSinglePlayerRuntimeValidationReport report)
        {
            GameObject host = null;
            try
            {
                host = new GameObject("BattleSinglePlayerRuntimeValidationDriver");
                SimulationTickDriver driver = host.AddComponent<SimulationTickDriver>();
                driver.RecreateWorld();
                driver.SetPaused(true);
                driver.ApplySettings(new LockstepSimulationSettings
                {
                    driveMode = SimulationDriveMode.Manual,
                    enableFrameChecksum = true,
                });

                LockstepSessionIdentity identity = CreateIdentity();
                var session = new BattleLockstepSession(
                    driver,
                    identity,
                    0,
                    16,
                    16,
                    snapshotIntervalTicks: 2,
                    snapshotCapacity: 4);
                for (int tick = 1; tick <= 6; tick++)
                {
                    Require(
                        session.TryAdvanceManual(
                            CreateNeutralFrame(tick),
                            buildPresentation: false),
                        "Manual lockstep advance failed at tick " + tick +
                        ": " + session.LastReason);
                }
                Require(
                    session.SnapshotRing.TryGet(
                        2,
                        out BattleStateSnapshotBuffer tickTwoSnapshot),
                    "Snapshot ring did not retain tick 2.");
                Require(
                    session.ChecksumHistory.TryGet(
                        6,
                        out LockstepChecksumHistoryEntry expected),
                    "Checksum history did not retain tick 6.");
                Require(
                    session.TryRestoreAndReplay(tickTwoSnapshot),
                    "Restore and replay failed: " + session.LastReason);
                Require(session.CurrentTick == 6,
                    "Restore and replay did not return to tick 6.");
                Require(driver.CurrentTickIndex == 6,
                    "Driver did not return to tick 6 after replay.");
                Require(driver.HasFrameChecksum,
                    "Driver did not publish a checksum after replay.");
                Require(driver.LastFrameChecksumValue == expected.StateChecksum,
                    "Restore and replay changed the final checksum.");

                report.replayChecksum = expected.StateChecksum.ToString("X16");
                report.restoreReplayPassed = true;
            }
            finally
            {
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static LockstepSessionIdentity CreateIdentity()
        {
            return new LockstepSessionIdentity(
                LockstepSessionIdentity.CurrentSchemaVersion,
                0x1234UL,
                99U,
                0xCA7UL,
                0x57A6EUL,
                new[] { 5, 2 });
        }

        private static FrameInputSet CreateNeutralFrame(int tick)
        {
            return new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(2, SimulationInputButtons.None),
                new SimulationPlayerInput(5, SimulationInputButtons.None),
            });
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }

    internal static class BattleSinglePlayerRuntimeValidationBootstrap
    {
        private const string EnableArgument = "--ntsd-u7-runtime-validation";
        private const string OutputArgument = "--ntsd-u7-output";
        private const string RunIdArgument = "--ntsd-u7-run-id";

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void TryRun()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (!HasFlag(arguments, EnableArgument))
                return;

            string outputPath = FindValue(arguments, OutputArgument);
            string runId = FindValue(arguments, RunIdArgument);
            BattleSinglePlayerRuntimeValidationReport report =
                BattleSinglePlayerRuntimeValidation.Run(runId);
            try
            {
                if (string.IsNullOrWhiteSpace(outputPath))
                    throw new InvalidOperationException("U7 output path is missing.");
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(
                    outputPath,
                    JsonUtility.ToJson(report, true),
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Debug.LogError("[BattleSinglePlayerRuntimeValidation] Report write failed: " + exception);
                Application.Quit(2);
                return;
            }

            if (string.Equals(report.status, "Passed", StringComparison.Ordinal))
            {
                Debug.Log("[BattleSinglePlayerRuntimeValidation] PASS");
                Application.Quit(0);
            }
            else
            {
                Debug.LogError(
                    "[BattleSinglePlayerRuntimeValidation] FAIL: " + report.failure);
                Application.Quit(1);
            }
        }

        private static bool HasFlag(string[] arguments, string name)
        {
            if (arguments == null)
                return false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        arguments[index],
                        name + "=true",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static string FindValue(string[] arguments, string name)
        {
            if (arguments == null)
                return string.Empty;
            string prefix = name + "=";
            for (int index = 0; index < arguments.Length; index++)
            {
                string argument = arguments[index];
                if (argument != null &&
                    argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return argument.Substring(prefix.Length);
                }
                if (string.Equals(argument, name, StringComparison.OrdinalIgnoreCase) &&
                    index + 1 < arguments.Length)
                {
                    return arguments[index + 1];
                }
            }
            return string.Empty;
        }
#endif
    }
}
