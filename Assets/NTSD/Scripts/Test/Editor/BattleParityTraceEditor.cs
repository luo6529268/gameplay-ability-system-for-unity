using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NTSD.EditorTools
{
    [InitializeOnLoad]
    public static class BattleParityTraceEditor
    {
        private const string RequestFile = "Temp/NTSDParity/unity-trace.request.json";
        private const string ResultFile = "Temp/NTSDParity/unity-trace.result";
        private const string DefaultScenario = "Tools/NTSDParity/scenario.sample.json";
        private const string DefaultOutput = "Temp/NTSDParity/unity-trace-final.jsonl";
        private const string ProductionDataFixture = "production";
        private const string AuthorityDiagnosticDataFixture = "authority-dat-diagnostic";
        private const string DatPassword = "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
        private const int KnownButtonMask = 0x7F;

        private static bool requestRunInProgress;

        static BattleParityTraceEditor()
        {
            EditorApplication.update += PollRequest;
        }

        [MenuItem("Tools/NTSD/Battle Parity/Run Sample Trace")]
        public static void RunSampleTrace()
        {
            RunAndWriteResult(
                DefaultScenario,
                DefaultOutput,
                "compact",
                ProductionDataFixture,
                exitBatchMode: false);
        }

        public static void RunFromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            string scenarioPath = ReadArgument(args, "-ntsdParityScenario") ?? DefaultScenario;
            string outputPath = ReadArgument(args, "-ntsdParityOutput") ?? DefaultOutput;
            string detail = ReadArgument(args, "-ntsdParityDetail") ?? "compact";
            string dataFixture = ReadArgument(args, "-ntsdParityDataFixture") ?? ProductionDataFixture;
            RunAndWriteResult(
                scenarioPath,
                outputPath,
                detail,
                dataFixture,
                exitBatchMode: Application.isBatchMode);
        }

        private static void PollRequest()
        {
            if (requestRunInProgress || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string requestPath = ProjectPath(RequestFile);
            if (!File.Exists(requestPath))
                return;

            requestRunInProgress = true;
            try
            {
                string json = File.ReadAllText(requestPath, Encoding.UTF8);
                TraceRequest request = JsonUtility.FromJson<TraceRequest>(json) ?? new TraceRequest();
                string scenarioPath = string.IsNullOrWhiteSpace(request.scenarioPath)
                    ? DefaultScenario
                    : request.scenarioPath;
                string outputPath = string.IsNullOrWhiteSpace(request.outputPath)
                    ? DefaultOutput
                    : request.outputPath;
                string detail = string.IsNullOrWhiteSpace(request.detail) ? "compact" : request.detail;
                string dataFixture = string.IsNullOrWhiteSpace(request.dataFixture)
                    ? ProductionDataFixture
                    : request.dataFixture;
                RunAndWriteResult(scenarioPath, outputPath, detail, dataFixture, exitBatchMode: false);
            }
            finally
            {
                try
                {
                    File.Delete(requestPath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BattleParityTraceEditor] Failed to delete request: {ex.Message}");
                }
                requestRunInProgress = false;
            }
        }

        private static void RunAndWriteResult(
            string scenarioPath,
            string outputPath,
            string detail,
            string dataFixture,
            bool exitBatchMode)
        {
            string resultPath = ProjectPath(ResultFile);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath) ?? ProjectPath("Temp"));
            try
            {
                string resolvedOutput = RunScenario(scenarioPath, outputPath, detail, dataFixture);
                File.WriteAllText(resultPath, $"PASS{Environment.NewLine}{resolvedOutput}", new UTF8Encoding(false));
                Debug.Log($"[BattleParityTraceEditor] Trace written: {resolvedOutput}");
                if (exitBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                File.WriteAllText(resultPath, $"FAIL{Environment.NewLine}{ex}", new UTF8Encoding(false));
                Debug.LogError($"[BattleParityTraceEditor] Trace failed: {ex}");
                if (exitBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        private static string RunScenario(
            string scenarioPath,
            string outputPath,
            string detail,
            string dataFixture)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Battle parity trace must run outside Play Mode.");
            if (detail != "compact" && detail != "full")
                throw new ArgumentException("Trace detail must be 'compact' or 'full'.", nameof(detail));
            if (dataFixture != ProductionDataFixture && dataFixture != AuthorityDiagnosticDataFixture)
                throw new ArgumentException(
                    $"Data fixture must be '{ProductionDataFixture}' or '{AuthorityDiagnosticDataFixture}'.",
                    nameof(dataFixture));

            string resolvedScenarioPath = ProjectPath(scenarioPath);
            string resolvedOutputPath = ProjectPath(outputPath);
            BattleTraceScenario scenario = JsonUtility.FromJson<BattleTraceScenario>(
                File.ReadAllText(resolvedScenarioPath, Encoding.UTF8));
            NormalizeOptionalFrameCounterProbe(scenario);
            ValidateScenario(scenario);

            string indexPath = Path.Combine(Path.GetFullPath(scenario.gameRoot), "data", "data.txt");
            if (!File.Exists(indexPath))
                throw new FileNotFoundException("Authority data index not found.", indexPath);

            Directory.CreateDirectory(Path.GetDirectoryName(resolvedOutputPath) ?? ProjectPath("Temp"));
            string manifestSha256 = ResolveBattleLogicManifestSha256(
                scenario.gameRoot,
                indexPath,
                dataFixture);

            using var driverScope = new TemporarySimulationDriverScope();
            using var dataScope = new ExternalDatScope(scenario.gameRoot, indexPath);
            SimulationTickDriver driver = driverScope.Driver;
            SimulationWorld world = driver.World;
            ConfigureWorldAndRoster(world, dataScope, scenario);
            BattleParityStructuralEventBuffer structuralEvents =
                RunStructuralWitnessFixture(scenario.structuralWitness);

            var provider = new ScenarioFrameInputProvider(scenario);
            driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                enableFrameChecksum = true,
            });
            driver.SetFrameInputProvider(provider);
            driver.SetPaused(false);

            using var writer = new StreamWriter(resolvedOutputPath, false, new UTF8Encoding(false));
            writer.WriteLine(BuildHeaderJson(
                resolvedScenarioPath,
                scenario,
                dataScope.LoadedCount,
                manifestSha256,
                world,
                detail,
                dataFixture));

            bool full = detail == "full";
            for (int tick = 1; tick <= scenario.ticks; tick++)
            {
                if (!driver.StepOneTick(ignorePaused: true))
                    throw new InvalidOperationException($"Unity simulation did not advance tick {tick}.");

                ValidateParityRuntimeContracts(world, scenario, tick);
                ApplyFrameCounterProbe(world, scenario.frameCounterProbe, tick);
                BattleParityFrameSnapshot snapshot = full ||
                    scenario.frameCounterProbe != null ||
                    structuralEvents != null
                    ? world.CaptureParityFrameSnapshot(
                        tick,
                        driver.LastAppliedFrameInput,
                        includeFullDomains: true,
                        structuralEvents?.CaptureTick(tick))
                    : driver.LastFrameSnapshot;
                if (snapshot == null || snapshot.Tick != tick)
                    throw new InvalidOperationException($"Unity parity snapshot missing for tick {tick}.");
                writer.WriteLine(snapshot.ToJson(full));
                writer.Flush();
            }

            return resolvedOutputPath;
        }

        internal static BattleParityStructuralEventBuffer RunStructuralWitnessFixture(
            string witnessId)
        {
            if (string.IsNullOrWhiteSpace(witnessId))
                return null;

            var events = new BattleParityStructuralEventBuffer(400);
            if (string.Equals(witnessId, "W03", StringComparison.Ordinal))
            {
                var world = new SimulationWorld();
                world.SetStructuralEventSinkForDiagnostics(events, 0, "fixture-setup");

                var mutator = new BattleTraceStructuralMutationEntity(100);
                mutator.SetRequiredRuntimeSlot(0);
                var slot1 = new BattleTraceStructuralEntity(101);
                slot1.SetRequiredRuntimeSlot(1);
                var slot2 = new BattleTraceStructuralEntity(102);
                slot2.SetRequiredRuntimeSlot(2);
                world.Register(mutator);
                world.Register(slot1);
                world.Register(slot2);

                world.LateEntityUpdateAll(1);
                world.LateEntityUpdateAll(2);

                if (mutator.Replacement?.Runtime?.SlotIndex != 0 ||
                    mutator.HighNewborn?.Runtime?.SlotIndex != 3 ||
                    mutator.Replacement.TickCount != 1 ||
                    mutator.HighNewborn.TickCount != 2)
                {
                    throw new InvalidOperationException(
                        "Unity W03 structural fixture did not hit high-slot same-pass and low-slot next-pass semantics.");
                }
                return events;
            }

            if (string.Equals(witnessId, "W04", StringComparison.Ordinal))
            {
                var world = new SimulationWorld();
                world.SetStructuralEventSinkForDiagnostics(events, 1, "allocator");

                var general = new BattleTraceStructuralEntity(200);
                world.Register(general);

                int stageSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(20, 400);
                var stage = new BattleTraceStructuralEntity(201);
                stage.Runtime.SpawnSemantic = (int)ReleaseSpawnSemantic.StageSpawnAt;
                stage.SetRequiredRuntimeSlot(stageSlot);
                world.Register(stage);

                var dynamicEntity = new LF2OtherObject();
                dynamicEntity.Runtime.StableId = 202;
                world.Register(dynamicEntity);

                var high = new BattleTraceStructuralEntity(203);
                high.SetRequiredRuntimeSlot(399);
                world.Register(high);

                if (general.Runtime.SlotIndex != 0 ||
                    stage.Runtime.SlotIndex != 20 ||
                    dynamicEntity.Runtime.SlotIndex != 50 ||
                    high.Runtime.SlotIndex != 399)
                {
                    throw new InvalidOperationException(
                        "Unity W04 structural fixture did not hit allocator starts 0/20/50 and Authority400 slot 399.");
                }
                return events;
            }

            if (string.Equals(witnessId, "W07", StringComparison.Ordinal))
            {
                var world = new SimulationWorld();
                world.SetStructuralEventSinkForDiagnostics(events, 0, "fixture-setup");

                var holder = new BattleTraceStructuralEntity(300);
                holder.SetRequiredRuntimeSlot(0);
                var target = new BattleTraceStructuralEntity(301);
                target.SetRequiredRuntimeSlot(1);
                world.Register(holder);
                world.Register(target);

                world.ValidateHeldLinksAll(1);

                holder.Runtime.LinkState = 1;
                holder.Runtime.TargetSlotIndex = 1;
                holder.Runtime.HeldWeaponStableId = 1;
                target.Runtime.LinkState = 0;
                target.Runtime.HolderStableId = 0;
                world.ValidateHeldLinksAll(2);

                target.Runtime.HolderStableId = 2;
                world.ValidateHeldLinksAll(3);

                if (holder.Runtime.LinkState != 0 ||
                    holder.Runtime.TargetSlotIndex != -1 ||
                    holder.Runtime.HeldWeaponStableId != -1 ||
                    target.Runtime.HolderStableId != 2 ||
                    target.Runtime.LinkState != 0)
                {
                    throw new InvalidOperationException(
                        "Unity W07 structural fixture did not preserve target reverse fields while clearing the mismatched holder link.");
                }
                return events;
            }

            throw new ArgumentException(
                $"Unsupported structuralWitness '{witnessId}'. Expected W03, W04, or W07.",
                nameof(witnessId));
        }

        private static void ApplyFrameCounterProbe(
            SimulationWorld world,
            BattleTraceFrameCounterProbe probe,
            int tick)
        {
            if (probe == null || (tick != probe.prepareTick && tick != probe.immediateTick))
                return;

            LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(probe.runtimeSlot);
            if (entity?.Runtime == null || entity.Frame == null || entity.Trans == null)
                throw new InvalidOperationException($"frameCounterProbe slot {probe.runtimeSlot} is not active.");
            if (entity.GetFrameDataById(probe.targetFrame) == null)
            {
                throw new InvalidOperationException(
                    $"frameCounterProbe target frame {probe.targetFrame} is unavailable at slot {probe.runtimeSlot}.");
            }

            entity.Trans.SyncWaitCounterFrame(probe.waitCounter);
            entity.Runtime.FrameWaitCounter = probe.frameWaitCounter;
            int expectedFrame = entity.Frame.N;
            if (tick == probe.immediateTick)
            {
                entity.DirectWriteFrameImmediateWaitReset(probe.targetFrame);
                expectedFrame = probe.targetFrame;
            }

            int expectedFrameWaitCounter = tick == probe.immediateTick ? 0 : probe.frameWaitCounter;
            if (entity.Frame.N != expectedFrame ||
                entity.Runtime.WaitCounter != probe.waitCounter ||
                entity.Runtime.FrameWaitCounter != expectedFrameWaitCounter)
            {
                throw new InvalidOperationException(
                    $"Unity frameCounterProbe contract failed at tick {tick}: " +
                    $"frame={entity.Frame.N}, frameWaitCounter={entity.Runtime.FrameWaitCounter}, " +
                    $"waitCounter={entity.Runtime.WaitCounter}.");
            }
        }

        private static void ValidateParityRuntimeContracts(
            SimulationWorld world,
            BattleTraceScenario scenario,
            int tick)
        {
            BattleRuntimeState runtime = world?.Runtime;
            if (runtime?.Flow == null || runtime.Results == null)
                throw new InvalidOperationException("Unity parity runtime state is incomplete.");
            if (!runtime.Flow.HumanInputPolledExternally)
                throw new InvalidOperationException($"Unity parity input was not marked externally polled at tick {tick}.");

            if (tick <= 1 || scenario.mode != 1)
                return;

            int[] expectedTeams = (scenario.slots ?? Array.Empty<BattleTraceSlot>())
                .Where(slot => slot.active)
                .Select(slot => slot.team == 0 ? 10 + slot.playerSlot : slot.team)
                .Where(team => team != 0)
                .Distinct()
                .Take(2)
                .ToArray();
            if (expectedTeams.Length < 2)
                return;

            BattleResultsRuntimeState results = runtime.Results;
            if (!results.HadBoth ||
                results.TeamCount != 2 ||
                results.TeamIds == null ||
                results.TeamIds.Length != 2 ||
                results.TeamIds[0] != expectedTeams[0] ||
                results.TeamIds[1] != expectedTeams[1])
            {
                throw new InvalidOperationException(
                    $"Unity battle results contract mismatch at tick {tick}: " +
                    $"hadBoth={results.HadBoth}, teamCount={results.TeamCount}, " +
                    $"teamIds=[{string.Join(",", results.TeamIds ?? Array.Empty<int>())}].");
            }
        }

        private static void ConfigureWorldAndRoster(
            SimulationWorld world,
            ExternalDatScope dataScope,
            BattleTraceScenario scenario)
        {
            world.ResetRuntimeState();
            world.Rng.Seed(unchecked((uint)scenario.seed));

            BattleRuntimeState runtime = world.Runtime;
            runtime.Match.LocalGameModeId = 0;
            runtime.Match.BattleGameModeId = scenario.mode;
            runtime.Match.BackgroundId = scenario.randomStage;
            runtime.Match.Difficulty = scenario.difficulty;
            runtime.Match.Seed = 0;
            runtime.StageProgression.StageSeriesIdx = scenario.stage;
            runtime.StageProgression.WaveIdx = -1;
            runtime.StageProgression.Round = 0;
            runtime.StageProgression.RoundMax = 0;
            runtime.StageProgressionValid = false;
            runtime.Flow.AiPhaseGate = scenario.mode == 2 ? 1 : 0;

            ResolveBackgroundBounds(scenario.gameRoot, dataScope.DataManager, scenario.stage,
                out int stageWidth, out int zMin, out int zMax);
            world.SetExplicitStageRuntimeSnapshotForTesting(stageWidth, zMin, zMax, 0, 0);
            world.SetNeedClearInput(true);

            runtime.Roster.Reset();
            BattleTraceSlot[] slots = scenario.slots ?? Array.Empty<BattleTraceSlot>();
            foreach (BattleTraceSlot source in slots.OrderBy(value => value.playerSlot))
            {
                if (!source.active)
                    continue;
                if (!dataScope.Configs.TryGetValue(source.oid, out LF2CharacterDataWrapper wrapper))
                    throw new InvalidOperationException($"Scenario oid {source.oid} was not loaded from gameRoot.");

                int battleTeam = source.team == 0 ? 10 + source.playerSlot : source.team;
                BattleSlotRuntimeState rosterSlot = runtime.Roster.Slots[source.playerSlot];
                rosterSlot.Active = true;
                rosterSlot.IsHuman = !source.ai;
                rosterSlot.CharacterId = source.oid;
                rosterSlot.Team = battleTeam;
                rosterSlot.InputId = source.playerSlot;
                rosterSlot.AiId = source.ai ? source.playerSlot : -1;

                int xRange = stageWidth / 2;
                int x = stageWidth / 4 + (xRange > 0 ? world.Rng.NextRaw() % xRange : 0);
                int zRange = zMax - zMin;
                int z = (zRange > 0 ? world.Rng.NextRaw() % zRange : 0) + zMin;
                LF2Character character = CreateCharacter(world, wrapper, source, battleTeam, x, z);
                rosterSlot.RuntimeSlotIndex = character.Runtime.SlotIndex;
                rosterSlot.StableId = character.Runtime.StableId;
                runtime.Roster.ActiveSlotCount++;
            }
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            LF2CharacterDataWrapper wrapper,
            BattleTraceSlot slot,
            int battleTeam,
            int x,
            int z)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = slot.oid;
            character.Runtime.X = x;
            character.Runtime.Y = 0.0;
            character.Runtime.Z = z;
            character.Runtime.XInt = x;
            character.Runtime.YInt = 0;
            character.Runtime.ZInt = z;
            character.ModuleBind(wrapper, slot.oid);
            if (character.Match != world)
                throw new InvalidOperationException($"Scenario oid {slot.oid} did not register into runner world.");

            character.Initialize(500, 500);
            character.Team = battleTeam;
            character.RelationTeam = battleTeam;
            character.AiControlled = slot.ai;
            character.RespawnCount = 0;
            character.HitStun = 75;
            character.Runtime.X = x;
            character.Runtime.Y = 0.0;
            character.Runtime.Z = z;
            character.Runtime.XInt = x;
            character.Runtime.YInt = 0;
            character.Runtime.ZInt = z;
            character.Runtime.Vx = 0.1;
            character.Runtime.Vy = 0.0;
            character.Runtime.Vz = 0.1;
            character.RefreshRuntimeSnapshot();
            return character;
        }

        private static void ResolveBackgroundBounds(
            string gameRoot,
            GameDataManager dataManager,
            int stage,
            out int width,
            out int zMin,
            out int zMax)
        {
            width = 800;
            zMin = 180;
            zMax = 350;
            BackgroundDefinition background = dataManager.GetBackgroundById(stage);
            if (background == null || string.IsNullOrWhiteSpace(background.file))
                return;

            string path = ResolveGameAssetPath(gameRoot, background.file);
            path = Path.ChangeExtension(path, ".dat");
            if (!File.Exists(path))
                return;

            string text = Lf2DatDecryptor.DecryptFile(path, DatPassword);
            Match widthMatch = Regex.Match(text ?? string.Empty, @"\bwidth\s*:\s*(-?\d+)", RegexOptions.IgnoreCase);
            Match zMatch = Regex.Match(text ?? string.Empty, @"\bzboundary\s*:\s*(-?\d+)\s+(-?\d+)", RegexOptions.IgnoreCase);
            if (widthMatch.Success)
                width = int.Parse(widthMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            if (zMatch.Success)
            {
                zMin = int.Parse(zMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                zMax = int.Parse(zMatch.Groups[2].Value, CultureInfo.InvariantCulture);
            }
        }

        private static string BuildHeaderJson(
            string scenarioPath,
            BattleTraceScenario scenario,
            int loadedChars,
            string manifestSha256,
            SimulationWorld world,
            string detail,
            string dataFixture)
        {
            object header = DictionaryOf(
                ("buttonMask", (object)DictionaryOf(
                    ("attack", (object)(int)SimulationInputButtons.Attack),
                    ("defend", (int)SimulationInputButtons.Defend),
                    ("down", (int)SimulationInputButtons.Down),
                    ("jump", (int)SimulationInputButtons.Jump),
                    ("left", (int)SimulationInputButtons.Left),
                    ("right", (int)SimulationInputButtons.Right),
                    ("up", (int)SimulationInputButtons.Up))),
                ("dataFixture", dataFixture),
                ("detail", detail),
                ("expectedTicks", scenario.ticks),
                ("kind", "header"),
                ("loadedChars", loadedChars),
                ("manifest", DictionaryOf(
                    ("battleLogicSha256", (object)manifestSha256),
                    ("domain", "battle-logic"),
                    ("schema", "ntsd-resolved-dat-manifest-v2"))),
                ("maxRuntimeSlots", 400),
                ("rngAfterBootstrap", DictionaryOf(
                    ("callCount", (object)world.Rng.CallCount),
                    ("seed", world.Rng.State))),
                ("scenario", ProjectScenario(scenario)),
                ("scenarioName", Path.GetFileName(scenarioPath)),
                ("schema", string.IsNullOrWhiteSpace(scenario.structuralWitness)
                    ? "ntsd-battle-trace-v3"
                    : "ntsd-battle-trace-v4"),
                ("stageFixture", DictionaryOf(
                    ("campaignCount", (object)0),
                    ("loaded", false),
                    ("name", null),
                    ("sha256", null))));
            return BattleCanonicalJson.Serialize(header);
        }

        private static object ProjectScenario(BattleTraceScenario scenario)
        {
            object[] slots = (scenario.slots ?? Array.Empty<BattleTraceSlot>())
                .Select(slot => DictionaryOf(
                    ("active", (object)slot.active),
                    ("ai", slot.ai),
                    ("oid", slot.oid),
                    ("playerSlot", slot.playerSlot),
                    ("team", slot.team)))
                .Cast<object>()
                .ToArray();
            object[] inputs = (scenario.inputs ?? Array.Empty<BattleTraceTickInput>())
                .Select(input => DictionaryOf(
                    ("players", (object)(input.players ?? Array.Empty<BattleTracePlayerInput>())
                        .Select(player => DictionaryOf(
                            ("buttonMask", (object)player.buttonMask),
                            ("playerSlot", player.playerSlot)))
                        .Cast<object>()
                        .ToArray()),
                    ("tick", input.tick)))
                .Cast<object>()
                .ToArray();
            SortedDictionary<string, object> result = DictionaryOf(
                ("difficulty", (object)scenario.difficulty),
                ("frameCounterProbe", ProjectFrameCounterProbe(scenario.frameCounterProbe)),
                ("inputs", inputs),
                ("mode", scenario.mode),
                ("randomStage", scenario.randomStage),
                ("seed", scenario.seed),
                ("slots", slots),
                ("stage", scenario.stage),
                ("ticks", scenario.ticks));
            if (!string.IsNullOrWhiteSpace(scenario.structuralWitness))
                result["structuralWitness"] = scenario.structuralWitness;
            return result;
        }

        private static object ProjectFrameCounterProbe(BattleTraceFrameCounterProbe probe)
        {
            if (probe == null)
                return null;

            return DictionaryOf(
                ("frameWaitCounter", (object)probe.frameWaitCounter),
                ("immediateTick", probe.immediateTick),
                ("prepareTick", probe.prepareTick),
                ("runtimeSlot", probe.runtimeSlot),
                ("targetFrame", probe.targetFrame),
                ("waitCounter", probe.waitCounter));
        }

        private static string ResolveBattleLogicManifestSha256(
            string gameRoot,
            string authorityIndex,
            string dataFixture)
        {
            string projectRoot = ProjectPath(string.Empty);
            string toolProject = Path.Combine(projectRoot, "Tools", "NTSDParity", "NTSDParity.csproj");
            string reportPath = Path.Combine(projectRoot, "Temp", "NTSDParity", "unity-runner-data-audit.json");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? projectRoot);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = string.Join(" ", new[]
                {
                    "run",
                    "--project", Quote(toolProject),
                    "--", "data-audit",
                    "--authority-root", Quote(Path.GetFullPath(gameRoot)),
                    "--authority-index", Quote(authorityIndex),
                    "--unity-root", Quote(projectRoot),
                    "--output", Quote(reportPath),
                }),
                WorkingDirectory = projectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start NTSDParity data audit.");
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(180000) || process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"NTSDParity data audit failed. exit={process.ExitCode}\n{stdout}\n{stderr}");

            DataAuditEnvelope report = JsonUtility.FromJson<DataAuditEnvelope>(
                File.ReadAllText(reportPath, Encoding.UTF8));
            string manifest = dataFixture == AuthorityDiagnosticDataFixture
                ? report?.manifest?.authorityBattleLogicSha256
                : report?.manifest?.unityBattleLogicSha256;
            if (string.IsNullOrWhiteSpace(manifest))
                throw new InvalidDataException("Unity battle-logic manifest missing from data audit report.");
            return manifest;
        }

        private static void NormalizeOptionalFrameCounterProbe(BattleTraceScenario scenario)
        {
            BattleTraceFrameCounterProbe probe = scenario?.frameCounterProbe;
            if (probe != null &&
                probe.runtimeSlot == 0 &&
                probe.prepareTick == 0 &&
                probe.immediateTick == 0 &&
                probe.waitCounter == 0 &&
                probe.frameWaitCounter == 0 &&
                probe.targetFrame == 0)
            {
                scenario.frameCounterProbe = null;
            }
        }

        private static void ValidateScenario(BattleTraceScenario scenario)
        {
            if (scenario == null)
                throw new InvalidDataException("Scenario JSON deserialized to null.");
            if (scenario.ticks <= 0)
                throw new ArgumentException("Scenario ticks must be positive.");
            if (string.IsNullOrWhiteSpace(scenario.gameRoot))
                throw new ArgumentException("Scenario gameRoot is required.");
            if (!string.IsNullOrWhiteSpace(scenario.stageFixture))
                throw new NotSupportedException("Explicit stage fixtures are not implemented by the Unity runner yet.");

            var rosterSlots = new HashSet<int>();
            foreach (BattleTraceSlot slot in scenario.slots ?? Array.Empty<BattleTraceSlot>())
            {
                if (slot.playerSlot < 0 || slot.playerSlot >= 8)
                    throw new ArgumentOutOfRangeException(nameof(slot.playerSlot), slot.playerSlot, "Player slot must be 0..7.");
                if (!rosterSlots.Add(slot.playerSlot))
                    throw new ArgumentException($"Scenario player slot {slot.playerSlot} is duplicated.");
            }

            var inputPlayers = new HashSet<(int Tick, int PlayerSlot)>();
            foreach (BattleTraceTickInput input in scenario.inputs ?? Array.Empty<BattleTraceTickInput>())
            {
                if (input.tick <= 0 || input.tick > scenario.ticks)
                    throw new ArgumentOutOfRangeException(nameof(input.tick), input.tick, "Input tick is outside scenario range.");
                foreach (BattleTracePlayerInput player in input.players ?? Array.Empty<BattleTracePlayerInput>())
                {
                    if (player.playerSlot < 0 || player.playerSlot >= 8)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(player.playerSlot),
                            player.playerSlot,
                            "Input player slot must be 0..7.");
                    }
                    if ((player.buttonMask & ~KnownButtonMask) != 0)
                    {
                        throw new ArgumentException(
                            $"Scenario input tick {input.tick} player {player.playerSlot} has unknown button bits 0x{player.buttonMask:X}.");
                    }
                    if (!inputPlayers.Add((input.tick, player.playerSlot)))
                    {
                        throw new ArgumentException(
                            $"Scenario input tick {input.tick} duplicates player slot {player.playerSlot}.");
                    }
                }
            }

            BattleTraceFrameCounterProbe probe = scenario.frameCounterProbe;
            if (probe != null)
            {
                if (probe.runtimeSlot < 0 || probe.runtimeSlot >= 400)
                    throw new ArgumentOutOfRangeException(nameof(probe.runtimeSlot));
                if (probe.prepareTick <= 0 || probe.immediateTick != probe.prepareTick + 1 ||
                    probe.immediateTick > scenario.ticks)
                {
                    throw new ArgumentException(
                        "frameCounterProbe must use consecutive prepare/immediate ticks inside the scenario.");
                }
                if (probe.waitCounter == 0 || probe.frameWaitCounter == 0 ||
                    probe.frameWaitCounter == probe.waitCounter)
                    throw new ArgumentException("frameCounterProbe counters must be nonzero and distinguishable.");
                if (probe.targetFrame < 0 || probe.targetFrame >= 400)
                    throw new ArgumentOutOfRangeException(nameof(probe.targetFrame));
            }

            if (!string.IsNullOrWhiteSpace(scenario.structuralWitness) &&
                scenario.structuralWitness != "W03" &&
                scenario.structuralWitness != "W04" &&
                scenario.structuralWitness != "W07")
            {
                throw new ArgumentException(
                    "structuralWitness must be W03, W04, or W07 when present.");
            }
        }

        private static SortedDictionary<string, object> DictionaryOf(params (string Key, object Value)[] items)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
            foreach ((string key, object value) in items)
                result[key] = value;
            return result;
        }

        private static string ResolveGameAssetPath(string gameRoot, string indexedPath)
        {
            if (Path.IsPathRooted(indexedPath))
                return Path.GetFullPath(indexedPath);
            string normalized = indexedPath.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(gameRoot, normalized));
        }

        private static string ProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Path.GetFullPath(Directory.GetCurrentDirectory());
            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (int i = 0; i + 1 < args.Length; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        private static string Quote(string value)
        {
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

        private sealed class ScenarioFrameInputProvider : ISimulationFrameInputProvider
        {
            private readonly Dictionary<int, FrameInputSet> inputs;

            public ScenarioFrameInputProvider(BattleTraceScenario scenario)
            {
                int[] activeHumanSlots = (scenario.slots ?? Array.Empty<BattleTraceSlot>())
                    .Where(slot => slot.active && !slot.ai)
                    .Select(slot => slot.playerSlot)
                    .Distinct()
                    .OrderBy(playerSlot => playerSlot)
                    .ToArray();
                var heldButtons = activeHumanSlots.ToDictionary(
                    playerSlot => playerSlot,
                    _ => SimulationInputButtons.None);
                Dictionary<int, BattleTracePlayerInput[]> updatesByTick =
                    (scenario.inputs ?? Array.Empty<BattleTraceTickInput>())
                    .GroupBy(item => item.tick)
                    .ToDictionary(
                        group => group.Key,
                        group => group.SelectMany(item => item.players ?? Array.Empty<BattleTracePlayerInput>()).ToArray());

                inputs = new Dictionary<int, FrameInputSet>(scenario.ticks);
                for (int tick = 1; tick <= scenario.ticks; tick++)
                {
                    if (updatesByTick.TryGetValue(tick, out BattleTracePlayerInput[] updates))
                    {
                        for (int i = 0; i < updates.Length; i++)
                        {
                            BattleTracePlayerInput update = updates[i];
                            if (heldButtons.ContainsKey(update.playerSlot))
                                heldButtons[update.playerSlot] = (SimulationInputButtons)update.buttonMask;
                        }
                    }

                    var players = new SimulationPlayerInput[activeHumanSlots.Length];
                    for (int i = 0; i < activeHumanSlots.Length; i++)
                    {
                        int playerSlot = activeHumanSlots[i];
                        players[i] = new SimulationPlayerInput(playerSlot, heldButtons[playerSlot]);
                    }
                    inputs[tick] = new FrameInputSet(tick, players);
                }
            }

            public bool IsFrameInputReady(int tickIndex) => true;

            public FrameInputSet GetFrameInput(int tickIndex)
            {
                return inputs.TryGetValue(tickIndex, out FrameInputSet input)
                    ? input
                    : FrameInputSet.Empty(tickIndex);
            }
        }

        private sealed class TemporarySimulationDriverScope : IDisposable
        {
            private static readonly PropertyInfo InstanceProperty =
                typeof(SingletonBehaviour<SimulationTickDriver>).GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static);

            private readonly SimulationTickDriver previousInstance;
            private readonly GameObject host;

            public TemporarySimulationDriverScope()
            {
                previousInstance = SimulationTickDriver.Instance;
                host = new GameObject("__NTSD_BattleParityTraceDriver")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                SetInstance(Driver);
                Driver.RecreateWorld();
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                SetInstance(null);
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
                SetInstance(previousInstance);
            }

            private static void SetInstance(SimulationTickDriver value)
            {
                MethodInfo setter = InstanceProperty?.GetSetMethod(nonPublic: true);
                if (setter == null)
                    throw new MissingMethodException("SimulationTickDriver singleton setter was not found.");
                setter.Invoke(null, new object[] { value });
            }
        }

        private sealed class ExternalDatScope : IDisposable
        {
            private readonly FieldInfo objectLookupField;
            private readonly FieldInfo cachedConfigField;
            private readonly FieldInfo frameConfigField;
            private readonly object originalObjectLookup;
            private readonly object originalCachedConfig;
            private readonly object originalFrameConfig;

            public ExternalDatScope(string gameRoot, string indexPath)
            {
                DataManager = GameDataManager.Instance
                    ?? throw new InvalidOperationException("GameDataManager singleton is unavailable.");
                CharacterAnimtorManager animationManager = CharacterAnimtorManager.Instance
                    ?? throw new InvalidOperationException("CharacterAnimtorManager singleton is unavailable.");

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                objectLookupField = typeof(GameDataManager).GetField("objectLookup", flags);
                cachedConfigField = typeof(GameDataManager).GetField("cachedConfig", flags);
                frameConfigField = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", flags);
                if (objectLookupField == null || cachedConfigField == null || frameConfigField == null)
                    throw new MissingFieldException("External DAT cache fields were not found.");

                originalObjectLookup = objectLookupField.GetValue(DataManager);
                originalCachedConfig = cachedConfigField.GetValue(DataManager);
                originalFrameConfig = frameConfigField.GetValue(animationManager);

                objectLookupField.SetValue(DataManager, null);
                cachedConfigField.SetValue(DataManager, null);
                DataManager.LoadDataFile(indexPath);

                Configs = LoadAllConfigs(gameRoot, DataManager, animationManager);
                frameConfigField.SetValue(animationManager, Configs);
                LoadedCount = Configs.Count;
            }

            public GameDataManager DataManager { get; }
            public Dictionary<int, LF2CharacterDataWrapper> Configs { get; }
            public int LoadedCount { get; }

            public void Dispose()
            {
                CharacterAnimtorManager animationManager = CharacterAnimtorManager.Instance;
                if (animationManager != null)
                    frameConfigField.SetValue(animationManager, originalFrameConfig);
                objectLookupField.SetValue(DataManager, originalObjectLookup);
                cachedConfigField.SetValue(DataManager, originalCachedConfig);
            }

            private static Dictionary<int, LF2CharacterDataWrapper> LoadAllConfigs(
                string gameRoot,
                GameDataManager dataManager,
                CharacterAnimtorManager animationManager)
            {
                MethodInfo buildMethod = typeof(CharacterAnimtorManager).GetMethod(
                    "BuildCharacterDataFromDat",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (buildMethod == null)
                    throw new MissingMethodException("CharacterAnimtorManager.BuildCharacterDataFromDat was not found.");

                var result = new Dictionary<int, LF2CharacterDataWrapper>();
                foreach (ObjectDefinition definition in dataManager.GetAllObjects().OrderBy(value => value.id))
                {
                    string datPath = Path.ChangeExtension(
                        ResolveGameAssetPath(gameRoot, definition.file),
                        ".dat");
                    if (!File.Exists(datPath))
                        continue;

                    string datText = Lf2DatDecryptor.DecryptFile(datPath, DatPassword);
                    Lf2DatFile datFile = new Lf2DatParserV2().Parse(datText, datPath);
                    if (datFile == null || datFile.Frames == null || datFile.Frames.Count == 0)
                        continue;

                    LF2CharacterData data = buildMethod.Invoke(
                        animationManager,
                        new object[] { datFile, Path.GetDirectoryName(datPath) }) as LF2CharacterData;
                    if (data == null)
                        continue;
                    if (data.type_sub == 0)
                        data.type_sub = definition.id;
                    result[definition.id] = new LF2CharacterDataWrapper(definition.id, data);
                }
                return result;
            }
        }

        [Serializable]
        private sealed class TraceRequest
        {
            public string scenarioPath;
            public string outputPath;
            public string detail = "compact";
            public string dataFixture = ProductionDataFixture;
        }

        private class BattleTraceStructuralEntity : LF2Entity
        {
            public BattleTraceStructuralEntity(int stableId)
            {
                StableId = stableId;
            }

            public int TickCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

            public override void SimFrameTick(int tickIndex)
            {
                TickCount++;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
            {
                Renderer = renderer;
            }
        }

        private sealed class BattleTraceStructuralMutationEntity :
            BattleTraceStructuralEntity
        {
            private bool mutated;

            public BattleTraceStructuralMutationEntity(int stableId)
                : base(stableId)
            {
            }

            public BattleTraceStructuralEntity Replacement { get; private set; }
            public BattleTraceStructuralEntity HighNewborn { get; private set; }

            public override void SimFrameTick(int tickIndex)
            {
                base.SimFrameTick(tickIndex);
                if (mutated)
                    return;

                mutated = true;
                Match.Unregister(this);
                Replacement = new BattleTraceStructuralEntity(1000);
                Match.Register(Replacement);
                HighNewborn = new BattleTraceStructuralEntity(1001);
                HighNewborn.SetRequiredRuntimeSlot(3);
                Match.Register(HighNewborn);
            }
        }

        [Serializable]
        private sealed class BattleTraceScenario
        {
            public int seed;
            public string gameRoot;
            public int mode = 1;
            public int difficulty = 1;
            public int stage;
            public int randomStage;
            public string stageFixture;
            public int ticks;
            public BattleTraceSlot[] slots;
            public BattleTraceTickInput[] inputs;
            public BattleTraceFrameCounterProbe frameCounterProbe;
            public string structuralWitness;
        }

        [Serializable]
        private sealed class BattleTraceFrameCounterProbe
        {
            public int runtimeSlot;
            public int prepareTick;
            public int immediateTick;
            public int waitCounter;
            public int frameWaitCounter;
            public int targetFrame;
        }

        [Serializable]
        private sealed class BattleTraceSlot
        {
            public int playerSlot;
            public int oid;
            public int team = 1;
            public bool active;
            public bool ai;
        }

        [Serializable]
        private sealed class BattleTraceTickInput
        {
            public int tick;
            public BattleTracePlayerInput[] players;
        }

        [Serializable]
        private sealed class BattleTracePlayerInput
        {
            public int playerSlot;
            public int buttonMask;
        }

        [Serializable]
        private sealed class DataAuditEnvelope
        {
            public DataAuditManifest manifest;
        }

        [Serializable]
        private sealed class DataAuditManifest
        {
            public string authorityBattleLogicSha256;
            public string unityBattleLogicSha256;
        }
    }
}
