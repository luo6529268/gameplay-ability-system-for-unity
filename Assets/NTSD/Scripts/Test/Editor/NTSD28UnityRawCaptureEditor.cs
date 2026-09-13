using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NTSD.Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NTSD.EditorTools
{
    [InitializeOnLoad]
    public static class NTSD28UnityRawCaptureEditor
    {
        internal const string CaptureSchema = "ntsd28-unity-raw-capture-v2";
        internal const string EvidenceClass =
            "UNITY_CURRENT_RUNTIME_DIAGNOSTIC_ONLY";
        internal const string DomainCaptureSchema =
            "ntsd28-logan-b0-domain-raw-v1";
        internal const string DomainEvidenceClass =
            "UNITY_DIAGNOSTIC_ONLY";
        internal const string InputRngCaptureSchema =
            "ntsd28-logan-b2-input-rng-joint-raw-v3";
        internal const string DefaultScenario =
            "Tools/NTSD28AuthorityTrace/Scenarios/neutral-common-two-entity.json";
        internal const string OriginalAuthorityScenario =
            "Tools/NTSD28AuthorityTrace/Scenarios/neutral-two-entity.json";
        internal const string InputScenario =
            "Tools/NTSD28AuthorityTrace/Scenarios/input-common-two-entity.json";
        internal const string StandingAttackRngScenario =
            "Tools/NTSD28AuthorityTrace/Scenarios/input-standing-attack-rng.json";
        internal const string AiRngScenario =
            "Tools/NTSD28AuthorityTrace/Scenarios/input-ai-one-entity-rng.json";
        internal const string RenderPhaseScenario =
            "Tools/NTSD28AuthorityTrace/Scenarios/render-phase-two-entity.json";
        internal const string DefaultOutput =
            "Temp/NTSD28UnityTrace/neutral-common-two-entity.raw.jsonl";

        private const string RequestFile =
            "Temp/NTSD28UnityTrace/unity-raw-capture.request.json";
        private const string ResultFile =
            "Temp/NTSD28UnityTrace/unity-raw-capture.result";
        private const string ProductionDataIndex = "Assets/NTSD/Config/data.txt";
        private const string ProductionConfigRoot = "Assets/NTSD/Config";
        private const string ExporterSource =
            "Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs";
        private const string DatPassword =
            "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
        private const string FormalAuthorityExeSha256 =
            "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
        private const string LegacyScenarioReferenceSha256 =
            "5EDA51440039099069D041E5BD13FDE8BE9FD8C7B20983985B1712A59719D86B";
        private const int Stage23Width = 1330;
        private const int Stage23ZMin = 542;
        private const int Stage23ZMax = 712;

        private static readonly string RequestAbsolutePath =
            ProjectPath(RequestFile);
        private static bool requestRunInProgress;

        static NTSD28UnityRawCaptureEditor()
        {
            EditorApplication.update += PollRequest;
        }

        [MenuItem("Tools/NTSD/2.8 Alignment/Run Unity Common Raw Capture")]
        public static void RunDefaultCapture()
        {
            RunAndWriteResult(DefaultScenario, DefaultOutput);
        }

        internal static string RunLoganScenarioForTests(string runtimeRoot, string scenarioPath, string outputPath)
        {
            return RunScenario(scenarioPath, outputPath, null, null, runtimeRoot);
        }

        internal static void WithLoganScenarioForReplayTests(
            string runtimeRoot, string scenarioPath, BattleRuntimeProfile profile, int totalTicks,
            Action<SimulationTickDriver, FrameInputSet[], LockstepSessionIdentity> verify)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Replay validation requires Edit Mode.");
            string path = ProjectPath(scenarioPath);
            var scenario = JsonUtility.FromJson<UnityRawScenario>(File.ReadAllText(path, Encoding.UTF8));
            ValidateScenario(scenario);
            if (totalTicks < scenario.ticks || totalTicks > 120)
                throw new ArgumentOutOfRangeException(nameof(totalTicks));
            var original = BuildFrameInputs(scenario);
            var inputs = new FrameInputSet[totalTicks];
            int[] slots = scenario.combatants.Select(value => value.slot).OrderBy(value => value).ToArray();
            for (int index = 0; index < inputs.Length; index++)
                inputs[index] = index < original.Length ? original[index] : new FrameInputSet(index + 1,
                    slots.Select(slot => new SimulationPlayerInput(slot, SimulationInputButtons.None)).ToArray());
            using var dataScope = new UnityCurrentDatScope(scenario.combatants.Select(value => value.oid).ToArray(), runtimeRoot);
            ulong fixtureIdentity = LoganContentIdentity.ForDecodeContract(ComputeFileSha256(path),
                "NTSD28_Q05_SCENARIO_FIXTURE_V1").CatalogFingerprint;
            LockstepSessionIdentity identity = dataScope.Catalog.ContentIdentity.CreateLocalValidationSessionIdentity(
                0x51305UL, unchecked((uint)scenario.seed), fixtureIdentity, slots);
            SimulationWorld observedWorld;
            BattleLogicReferencePool observedPool;
            using (var driverScope = new TemporarySimulationDriverScope())
            {
                SimulationTickDriver driver = driverScope.Driver;
                int capacity = profile == BattleRuntimeProfile.Authority400
                    ? BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity : BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity;
                var settings = new BattleRuntimeWorldSettings(profile, capacity,
                    profile == BattleRuntimeProfile.Authority400 ? capacity : BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities);
                if (!driver.TryConfigureEmptyDiagnosticWorld(settings, BattleAiExecutionProfile.DataOrientedCanonical, out string reason))
                    throw new InvalidOperationException(reason);
                observedWorld = driver.World;
                observedPool = new BattleLogicReferencePool();
                observedWorld.BindLogicReferencePool(observedPool);
                ConfigureWorldAndRoster(observedWorld, dataScope.Configs, scenario, true);
                observedWorld.PrepareRuntimeDataCatalogForBattle(dataScope.Catalog.Entries
                    .Select(entry => new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                    id => dataScope.Configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper) ? wrapper : null);
                driver.ApplySettings(new LockstepSimulationSettings
                {
                    driveMode = SimulationDriveMode.Manual,
                    enableFrameChecksum = true,
                });
                driver.SetPaused(false);
                observedWorld.SetLogicOnlyEntityMaterialization(true);
                dataScope.AssertInputsCurrent();
                verify(driver, inputs, identity);
                dataScope.AssertInputsCurrent();
            }
            if (observedWorld.ObjectCount != 0 || observedWorld.ClaimedRuntimeSlotCountForDiagnostics != 0 || observedPool.ActiveCount != 0)
                throw new InvalidOperationException("Replay validation shutdown retained objects=" + observedWorld.ObjectCount +
                    ", slots=" + observedWorld.ClaimedRuntimeSlotCountForDiagnostics + ", borrowers=" + observedPool.ActiveCount);
        }

        internal static string RunScenarioForTests(
            string scenarioPath,
            string outputPath)
        {
            return RunScenario(scenarioPath, outputPath, null, null);
        }

        internal static string RunScenarioForTests(
            string scenarioPath,
            string outputPath,
            string domainOutputPath)
        {
            return RunScenario(
                scenarioPath,
                outputPath,
                domainOutputPath,
                null);
        }

        internal static string RunScenarioForTests(
            string scenarioPath,
            string outputPath,
            string domainOutputPath,
            string inputRngOutputPath)
        {
            return RunScenario(
                scenarioPath,
                outputPath,
                domainOutputPath,
                inputRngOutputPath);
        }

        private static void PollRequest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                requestRunInProgress || EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                !File.Exists(RequestAbsolutePath))
            {
                return;
            }

            requestRunInProgress = true;
            try
            {
                string json = File.ReadAllText(RequestAbsolutePath, Encoding.UTF8);
                CaptureRequest request =
                    JsonUtility.FromJson<CaptureRequest>(json) ??
                    new CaptureRequest();
                string scenario = string.IsNullOrWhiteSpace(request.scenarioPath)
                    ? DefaultScenario
                    : request.scenarioPath;
                string output = string.IsNullOrWhiteSpace(request.outputPath)
                    ? DefaultOutput
                    : request.outputPath;
                RunAndWriteResult(
                    scenario,
                    output,
                    request.domainOutputPath,
                    request.inputRngOutputPath,
                    request.loganRuntimeRoot);
            }
            finally
            {
                try
                {
                    File.Delete(RequestAbsolutePath);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        $"[NTSD28UnityRawCapture] Request cleanup failed: {exception.Message}");
                }
                requestRunInProgress = false;
            }
        }

        private static void RunAndWriteResult(
            string scenarioPath,
            string outputPath,
            string domainOutputPath = null,
            string inputRngOutputPath = null,
            string loganRuntimeRoot = null)
        {
            string resultPath = ProjectPath(ResultFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(resultPath) ?? ProjectPath("Temp"));
            try
            {
                string resolvedOutput = RunScenario(
                    scenarioPath,
                    outputPath,
                    domainOutputPath,
                    inputRngOutputPath,
                    loganRuntimeRoot);
                File.WriteAllText(
                    resultPath,
                    $"PASS{Environment.NewLine}{resolvedOutput}",
                    new UTF8Encoding(false));
                Debug.Log(
                    $"[NTSD28UnityRawCapture] Capture written: {resolvedOutput}");
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    resultPath,
                    $"FAIL{Environment.NewLine}{exception}",
                    new UTF8Encoding(false));
                Debug.LogError(
                    $"[NTSD28UnityRawCapture] Capture failed: {exception}");
            }
        }

        private static string RunScenario(
            string scenarioPath,
            string outputPath,
            string domainOutputPath,
            string inputRngOutputPath,
            string loganRuntimeRoot = null)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "NTSD 2.8 Unity raw capture must run outside Play Mode.");
            }

            string resolvedScenarioPath = ProjectPath(scenarioPath);
            string resolvedOutputPath = ProjectPath(outputPath);
            string resolvedDomainOutputPath = string.IsNullOrWhiteSpace(
                domainOutputPath)
                ? null
                : ProjectPath(domainOutputPath);
            string resolvedInputRngOutputPath = string.IsNullOrWhiteSpace(
                inputRngOutputPath)
                ? null
                : ProjectPath(inputRngOutputPath);
            RequireDistinctOutputPaths(
                resolvedOutputPath,
                resolvedDomainOutputPath,
                resolvedInputRngOutputPath);
            UnityRawScenario scenario = JsonUtility.FromJson<UnityRawScenario>(
                File.ReadAllText(resolvedScenarioPath, Encoding.UTF8));
            ValidateScenario(scenario);

            Directory.CreateDirectory(
                Path.GetDirectoryName(resolvedOutputPath) ?? ProjectPath("Temp"));
            if (resolvedDomainOutputPath != null)
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(resolvedDomainOutputPath) ??
                    ProjectPath("Temp"));
            }
            if (resolvedInputRngOutputPath != null)
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(resolvedInputRngOutputPath) ??
                    ProjectPath("Temp"));
            }

            FrameInputSet[] frameInputs = BuildFrameInputs(scenario);

            int[] requestedObjectIds = scenario.combatants
                .Select(combatant => combatant.oid)
                .Distinct()
                .OrderBy(value => value)
                .ToArray();

            using var dataScope = new UnityCurrentDatScope(requestedObjectIds, loganRuntimeRoot);
            dataScope.AssertInputsCurrent();
            int[] missingObjectIds = requestedObjectIds
                .Where(objectId => !dataScope.Configs.ContainsKey(objectId))
                .ToArray();
            if (missingObjectIds.Length != 0)
            {
                throw new InvalidOperationException(
                    "Unity current content is missing required object ids: " +
                    string.Join(",", missingObjectIds));
            }

            using var driverScope = new TemporarySimulationDriverScope();
            SimulationTickDriver driver = driverScope.Driver;
            SimulationWorld world = driver.World;
            ConfigureWorldAndRoster(world, dataScope.Configs, scenario, dataScope.IsLogan);
            var directRngCalls = new NativeRandomDirectCallRecorder();
            world.NativeRandom.SetDiagnosticCallObserver(directRngCalls);
            world.SetAcceptedAiRandomTraceObserverForDiagnostics(
                directRngCalls);

            driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                enableFrameChecksum = true,
                captureFullFrameSnapshotForDiagnostics = false,
            });
            driver.SetPaused(false);

            using var writer = new StreamWriter(
                resolvedOutputPath,
                false,
                new UTF8Encoding(false));
            StreamWriter domainWriter = resolvedDomainOutputPath == null
                ? null
                : new StreamWriter(
                    resolvedDomainOutputPath,
                    false,
                    new UTF8Encoding(false));
            StreamWriter inputRngWriter = resolvedInputRngOutputPath == null
                ? null
                : new StreamWriter(
                    resolvedInputRngOutputPath,
                    false,
                    new UTF8Encoding(false));
            try
            {
                writer.WriteLine(BuildHeaderJson(
                    resolvedScenarioPath,
                    scenario,
                    world.RuntimeSlotCapacityForDiagnostics,
                    dataScope.Content));
                List<DomainOccupant> previousOccupants =
                    CaptureDomainOccupants(world);
                ulong previousRngCalls = world.Rng.CallCount;
                NTSD28NativeRandomScalarState previousNativeRandom =
                    world.NativeRandom.CaptureScalarState();
                domainWriter?.WriteLine(BuildDomainHeaderJson(
                    resolvedScenarioPath,
                    scenario,
                    world,
                    previousOccupants));
                inputRngWriter?.WriteLine(BuildInputRngHeaderJson(
                    resolvedScenarioPath,
                    scenario,
                    world,
                    previousNativeRandom));

                for (int tick = 1; tick <= scenario.ticks; tick++)
                {
                    FrameInputSet frameInput = frameInputs[tick - 1];
                    directRngCalls.Clear();
                    long exactCharacterCountBefore =
                        world.BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics
                            .ExactCharacterCount;
                    if (!driver.StepOneTick(
                            frameInput,
                            ignorePaused: true,
                            buildPresentation: false))
                    {
                        throw new InvalidOperationException(
                            $"Unity simulation did not advance completed tick {tick}.");
                    }

                    long exactCharacterCountAfter =
                        world.BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics
                            .ExactCharacterCount;
                    long exactCharacterDelta =
                        exactCharacterCountAfter - exactCharacterCountBefore;
                    if (exactCharacterDelta != scenario.combatants.Length)
                    {
                        throw new InvalidOperationException(
                            $"Unity tick {tick} did not reach the exact character " +
                            $"frame-tick boundary: expected {scenario.combatants.Length}, " +
                            $"actual {exactCharacterDelta}.");
                    }

                    writer.WriteLine(
                        NTSD28UnityEntityRawCapture.CaptureTickJson(world, tick));
                    writer.Flush();
                    if (domainWriter != null)
                    {
                        List<DomainOccupant> currentOccupants =
                            CaptureDomainOccupants(world);
                        domainWriter.WriteLine(BuildDomainTickJson(
                            world,
                            tick,
                            frameInput,
                            previousRngCalls,
                            previousOccupants,
                            currentOccupants));
                        domainWriter.Flush();
                        previousRngCalls = world.Rng.CallCount;
                        previousOccupants = currentOccupants;
                    }
                    if (inputRngWriter != null)
                    {
                        NTSD28NativeRandomScalarState currentNativeRandom =
                            world.NativeRandom.CaptureScalarState();
                        inputRngWriter.WriteLine(BuildInputRngTickJson(
                            world,
                            tick,
                            previousNativeRandom,
                            currentNativeRandom,
                            directRngCalls));
                        inputRngWriter.Flush();
                        previousNativeRandom = currentNativeRandom;
                    }
                }
            }
            finally
            {
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(null);
                world.NativeRandom.SetDiagnosticCallObserver(null);
                domainWriter?.Dispose();
                inputRngWriter?.Dispose();
            }

            try
            {
                dataScope.AssertInputsCurrent();
            }
            catch
            {
                writer.WriteLine("INVALID_CONTENT_INPUTS_CHANGED");
                writer.Flush();
                throw;
            }
            return resolvedOutputPath;
        }

        private static void RequireDistinctOutputPaths(params string[] paths)
        {
            string[] present = paths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToArray();
            for (int left = 0; left < present.Length; left++)
            {
                for (int right = left + 1; right < present.Length; right++)
                {
                    if (string.Equals(
                            present[left],
                            present[right],
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException(
                            "Entity, domain, and B2 input/RNG raw outputs must " +
                            "be distinct files.");
                    }
                }
            }
        }

        private static void ValidateScenario(UnityRawScenario scenario)
        {
            if (scenario == null)
                throw new InvalidDataException("Scenario JSON is empty.");
            if (!string.Equals(
                    scenario.schema,
                    "ntsd28-scenario/1.0",
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException("Scenario schema mismatch.");
            }
            if (!string.Equals(
                    scenario.referenceExeSha256,
                    LegacyScenarioReferenceSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Scenario legacy reference identity mismatch.");
            }
            if (!IsSha256(scenario.dataSha256))
                throw new InvalidDataException("Scenario data SHA-256 is invalid.");
            if (!string.Equals(scenario.mode, "versus", StringComparison.Ordinal))
                throw new InvalidDataException("Only the neutral versus baseline is supported.");
            if (scenario.ticks != 3 || scenario.emitInitial ||
                scenario.battleMode != 0 || scenario.stageId != 23 ||
                scenario.difficultyLevel4A0C30 != 1)
            {
                throw new InvalidDataException(
                    "Scenario must preserve the frozen 3-tick Stage 23 baseline.");
            }
            if (scenario.combatants == null || scenario.combatants.Length != 2)
                throw new InvalidDataException("Scenario must contain two combatants.");

            int[] slots = scenario.combatants
                .Select(combatant => combatant.slot)
                .OrderBy(value => value)
                .ToArray();
            if (!slots.SequenceEqual(new[] { 0, 1 }))
                throw new InvalidDataException("Combatant slots must be exactly 0 and 1.");

            foreach (UnityRawCombatant combatant in scenario.combatants)
            {
                if (combatant.oid <= 0 || combatant.team <= 0 ||
                    combatant.hp <= 0 || combatant.baseHp <= 0 ||
                    combatant.mp < 0 || combatant.facing < 0 ||
                    combatant.facing > 1 || combatant.z < Stage23ZMin ||
                    combatant.z > Stage23ZMax)
                {
                    throw new InvalidDataException(
                        $"Combatant slot {combatant.slot} is outside the frozen baseline contract.");
                }
            }

            ValidateInputs(scenario, slots);
        }

        private static void ValidateInputs(
            UnityRawScenario scenario,
            IReadOnlyCollection<int> occupiedSlots)
        {
            var occupied = new HashSet<int>(occupiedSlots);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (UnityRawInput input in
                     scenario.inputs ?? Array.Empty<UnityRawInput>())
            {
                if (input == null || input.tick < 0 ||
                    input.tick >= scenario.ticks ||
                    !occupied.Contains(input.slot))
                {
                    throw new InvalidDataException(
                        "Input event has an invalid tick or combatant slot.");
                }

                if (!seen.Add($"{input.tick}:{input.slot}"))
                {
                    throw new InvalidDataException(
                        "Input events must be unique by tick and slot.");
                }

                foreach (string key in input.keys ?? Array.Empty<string>())
                    _ = ParseInputKey(key);
            }
        }

        private static FrameInputSet[] BuildFrameInputs(UnityRawScenario scenario)
        {
            var byTickAndSlot = (scenario.inputs ?? Array.Empty<UnityRawInput>())
                .ToDictionary(input => (input.tick, input.slot));
            UnityRawCombatant[] orderedCombatants = scenario.combatants
                .OrderBy(combatant => combatant.slot)
                .ToArray();
            var result = new FrameInputSet[scenario.ticks];
            for (int scenarioTick = 0; scenarioTick < scenario.ticks; scenarioTick++)
            {
                var players = new SimulationPlayerInput[orderedCombatants.Length];
                for (int index = 0; index < orderedCombatants.Length; index++)
                {
                    int playerSlot = orderedCombatants[index].slot;
                    SimulationInputButtons buttons = SimulationInputButtons.None;
                    if (byTickAndSlot.TryGetValue(
                            (scenarioTick, playerSlot),
                            out UnityRawInput input))
                    {
                        foreach (string key in input.keys ?? Array.Empty<string>())
                            buttons |= ParseInputKey(key);
                    }

                    players[index] = new SimulationPlayerInput(
                        playerSlot,
                        buttons);
                }

                int completedTick = scenarioTick + 1;
                result[scenarioTick] = new FrameInputSet(completedTick, players);
            }

            return result;
        }

        internal static SimulationInputButtons ParsePhysicalInputKeyForTests(
            string key)
        {
            return ParseInputKey(key);
        }

        private static SimulationInputButtons ParseInputKey(string key)
        {
            return key switch
            {
                "D" => SimulationInputButtons.Right,
                "A" => SimulationInputButtons.Left,
                "W" => SimulationInputButtons.Up,
                "S" => SimulationInputButtons.Down,
                // Alignment contract: NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001.
                // FrameInput retains the project's crossed J/K/L carrier names.
                "J" => SimulationInputButtons.Jump,
                "K" => SimulationInputButtons.Defend,
                "L" => SimulationInputButtons.Attack,
                _ => throw new InvalidDataException(
                    $"Input event contains unknown key: {key ?? "<null>"}"),
            };
        }

        private static void ConfigureWorldAndRoster(
            SimulationWorld world,
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs,
            UnityRawScenario scenario,
            bool loganContent)
        {
            world.ResetRuntimeState();
            if (world.AiExecutionProfile !=
                BattleAiExecutionProfile.DataOrientedCanonical)
            {
                world.ConfigureAiExecutionProfile(
                    BattleAiExecutionProfile.DataOrientedCanonical);
            }
            world.Rng.Seed(unchecked((uint)scenario.seed));
            world.NativeRandom.ResetForDirectBattle(
                unchecked((uint)scenario.seed));
            world.SetExplicitStageRuntimeSnapshotForTesting(
                Stage23Width,
                Stage23ZMin,
                Stage23ZMax,
                0,
                0);
            world.SetNeedClearInput(false);

            BattleRuntimeState runtime = world.Runtime;
            runtime.Match.LocalGameModeId = 0;
            runtime.Match.BattleGameModeId = scenario.battleMode;
            runtime.Match.BackgroundId = scenario.stageId;
            runtime.Match.Difficulty = scenario.difficultyLevel4A0C30;
            runtime.Match.Seed = scenario.seed;
            runtime.StageProgression.StageSeriesIdx = scenario.stageId;
            runtime.StageProgression.WaveIdx = -1;
            runtime.StageProgression.Round = 0;
            runtime.StageProgression.RoundMax = 0;
            runtime.StageProgressionValid = false;
            runtime.Flow.AiPhaseGate = 0;
            runtime.Roster.Reset();

            foreach (UnityRawCombatant source in
                     scenario.combatants.OrderBy(value => value.slot))
            {
                LF2Character character = CreateCharacter(
                    world,
                    configs[source.oid],
                    source,
                    loganContent);
                BattleSlotRuntimeState rosterSlot =
                    runtime.Roster.Slots[source.slot];
                bool aiControlled =
                    source.nativeAi || source.nativeComputerState1b8 > 0;
                rosterSlot.Active = true;
                rosterSlot.IsHuman = !aiControlled;
                rosterSlot.CharacterId = source.oid;
                rosterSlot.Team = source.team;
                rosterSlot.InputId = source.slot;
                rosterSlot.AiId = aiControlled ? source.slot : -1;
                rosterSlot.RuntimeSlotIndex = character.Runtime.SlotIndex;
                rosterSlot.StableId = character.Runtime.StableId;
                runtime.Roster.ActiveSlotCount++;
            }
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            LF2CharacterDataWrapper wrapper,
            UnityRawCombatant source,
            bool loganContent)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = source.oid;
            character.SetRequiredRuntimeSlot(source.slot);
            WritePosition(character.Runtime, source.x, source.y, source.z);
            character.ModuleBind(wrapper, source.oid, world);
            if (character.Match != world)
            {
                throw new InvalidOperationException(
                    $"Scenario oid {source.oid} did not register into the capture world.");
            }

            int baseMaxMp = loganContent
                ? wrapper.characterData.NativeMetadata.Stats.Int32OrDefault("max_mp", source.mp)
                : source.mp;
            character.Initialize(source.hp, baseMaxMp);
            if (loganContent)
            {
                character.Runtime.MP = source.mp;
                character.Runtime.PP = source.mp;
            }
            character.Runtime.HPBound = source.baseHp;
            character.Runtime.HP3 = source.baseHp;
            character.Team = source.team;
            character.RelationTeam = source.team;
            character.AiControlled =
                source.nativeAi || source.nativeComputerState1b8 > 0;
            character.HP2Orig = source.reviveLives30c;
            character.HPOrig = source.reviveNextLives310;
            character.RespawnCount = source.reviveNextHp314;
            character.HitStun = source.renderPhase008;
            character.OwnerEntityIndex = source.slot;
            character.Unk344 = 0;
            character.SwitchDir(source.facing == 1 ? "left" : "right");
            WritePosition(character.Runtime, source.x, source.y, source.z);
            character.Runtime.Vx = 0.0;
            character.Runtime.Vy = 0.0;
            character.Runtime.Vz = 0.0;
            character.RefreshRuntimeSnapshot();
            return character;
        }

        private static void WritePosition(
            NTSDEntityRuntime runtime,
            int x,
            int y,
            int z)
        {
            runtime.X = x;
            runtime.Y = y;
            runtime.Z = z;
            runtime.XInt = x;
            runtime.YInt = y;
            runtime.ZInt = z;
        }

        private static string BuildHeaderJson(
            string scenarioPath,
            UnityRawScenario scenario,
            int slotCapacity,
            Dictionary<string, object> content)
        {
            string assemblyPath = typeof(NTSD28UnityRawCaptureEditor)
                .Assembly.Location;
            return BattleCanonicalJson.Serialize(DictionaryOf(
                ("bindingStatus", (object)DictionaryOf(
                    ("candidate", (object)NTSD28UnityEntityRawCapture.CandidateBindings),
                    ("candidateCount", NTSD28UnityEntityRawCapture.CandidateBindings.Length),
                    ("fieldCount", NTSD28UnityEntityRawCapture.FieldCount),
                    ("missing", (object)NTSD28UnityEntityRawCapture.MissingBindings),
                    ("missingCount", NTSD28UnityEntityRawCapture.MissingBindings.Length),
                    ("verifiedCount",
                        NTSD28UnityEntityRawCapture.VerifiedBindingCount))),
                ("certificateEligible", false),
                ("evidenceClass", EvidenceClass),
                ("expectedTickCount", scenario.ticks),
                ("exporterSourceSha256", ComputeFileSha256(ProjectPath(ExporterSource))),
                ("firstCompletedTick", 1),
                ("formalAuthorityExeSha256", FormalAuthorityExeSha256),
                ("kind", "header"),
                ("scenarioDataSha256", scenario.dataSha256.ToUpperInvariant()),
                ("scenarioFileSha256", ComputeFileSha256(scenarioPath)),
                ("scenarioId", Path.GetFileNameWithoutExtension(scenarioPath)),
                ("scenarioReferenceExeSha256", scenario.referenceExeSha256.ToUpperInvariant()),
                ("schema", CaptureSchema),
                ("slotCapacity", slotCapacity),
                ("unityAssemblySha256", ComputeFileSha256(assemblyPath)),
                ("runtimeAssemblySha256", ComputeFileSha256(typeof(NTSD28UnityEntityRawCapture).Assembly.Location)),
                ("content", (object)content)));
        }

        private static string BuildDomainHeaderJson(
            string scenarioPath,
            UnityRawScenario scenario,
            SimulationWorld world,
            IReadOnlyList<DomainOccupant> initialOccupants)
        {
            return BattleCanonicalJson.Serialize(DictionaryOf(
                ("certificateEligible", false),
                ("evidenceClass", DomainEvidenceClass),
                ("expectedTickCount", scenario.ticks),
                ("firstCompletedTick", 1),
                ("formalExeSha256", FormalAuthorityExeSha256),
                ("initialOccupants", (object)ProjectDomainOccupants(
                    initialOccupants)),
                ("initialRngTotalCalls", (object)DictionaryOf(
                    ("authorityCrt", null),
                    ("authoritySynchronized", null),
                    ("unityDeterministic", world.Rng.CallCount))),
                ("kind", "header"),
                ("lifecycleDeltaProvenance", "snapshot-derived"),
                ("perCallTraceAvailability", (object)DictionaryOf(
                    ("authorityCrt", "missing"),
                    ("authoritySynchronized", "missing"),
                    ("unityDeterministic", "missing"))),
                ("producer", "unity-diagnostic"),
                ("scenarioId", Path.GetFileNameWithoutExtension(scenarioPath)),
                ("schema", DomainCaptureSchema),
                ("slotCapacity", world.RuntimeSlotCapacityForDiagnostics),
                ("streamAvailability", (object)DictionaryOf(
                    ("authorityCrt", "missing"),
                    ("authoritySynchronized", "missing"),
                    ("unityDeterministic", "available")))));
        }

        private static string BuildDomainTickJson(
            SimulationWorld world,
            int completedTick,
            FrameInputSet frameInput,
            ulong previousRngCalls,
            IReadOnlyList<DomainOccupant> previousOccupants,
            IReadOnlyList<DomainOccupant> currentOccupants)
        {
            ulong currentRngCalls = world.Rng.CallCount;
            if (currentRngCalls < previousRngCalls)
            {
                throw new InvalidOperationException(
                    "Unity RNG call total moved backwards during domain capture.");
            }

            object[] players = frameInput.Players
                .Select(player => (object)DictionaryOf(
                    ("heldMask", (byte)ProjectPhysicalCanonicalButtons(
                        player.Buttons)),
                    ("playerSlot", player.PlayerSlot)))
                .ToArray();
            return BattleCanonicalJson.Serialize(DictionaryOf(
                ("aiAcceptedTrace", (object)DictionaryOf(
                    ("committed",
                        world.AiDecisionIndexedCanonicalCommittedCountForDiagnostics),
                    ("eligible",
                        world.AiDecisionIndexedCanonicalEligibleCountForDiagnostics),
                    ("fallback",
                        world.AiDecisionIndexedCanonicalFallbackCountForDiagnostics),
                    ("firstFallbackReason",
                        world.AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics
                            .ToString()),
                    ("oracleMismatch",
                        world.AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics))),
                ("completedTick", completedTick),
                ("input", (object)DictionaryOf(
                    ("captureBoundary", "applied-to-tick"),
                    ("players", (object)players))),
                ("kind", "tick"),
                ("lifecycleDelta", (object)DictionaryOf(
                    ("events", (object)DeriveLifecycleEvents(
                        previousOccupants,
                        currentOccupants)),
                    ("provenance", "snapshot-derived"))),
                ("rng", (object)DictionaryOf(
                    ("authorityCrt", (object)MissingRngStream()),
                    ("authoritySynchronized", (object)MissingRngStream()),
                    ("unityDeterministic", (object)DictionaryOf(
                        ("availability", "available"),
                        ("calls", null),
                        ("state", world.Rng.State),
                        ("tickCallCount", currentRngCalls - previousRngCalls),
                        ("totalCalls", currentRngCalls))))),
                ("slots", (object)DictionaryOf(
                    ("capacity", world.RuntimeSlotCapacityForDiagnostics),
                    ("occupants", (object)ProjectDomainOccupants(
                        currentOccupants))))));
        }

        private static string BuildInputRngHeaderJson(
            string scenarioPath,
            UnityRawScenario scenario,
            SimulationWorld world,
            NTSD28NativeRandomScalarState initialRandom)
        {
            return BattleCanonicalJson.Serialize(DictionaryOf(
                ("certificateEligible", false),
                ("evidenceClass", "UNITY_DIAGNOSTIC_ONLY"),
                ("expectedTickCount", scenario.ticks),
                ("firstCompletedTick", 1),
                ("formalExeSha256", FormalAuthorityExeSha256),
                ("initialInputPhase", world.InputPhase),
                ("initialRng", (object)ProjectInitialNativeRandom(
                    initialRandom)),
                ("keyOrder", (object)new object[]
                {
                    "W", "S", "A", "D", "J", "K", "L",
                }),
                ("kind", "header"),
                ("perCallTraceAvailability", (object)DictionaryOf(
                    ("crt", "available-completed-ticks"),
                    ("synchronized", "available-completed-ticks"))),
                ("perCallTraceScope", (object)DictionaryOf(
                    ("aiCursorExcluded", false),
                    ("humanScenarioRequired", false),
                    ("initializationExcluded", true))),
                ("producer", "unity-diagnostic"),
                ("scenarioId", Path.GetFileNameWithoutExtension(scenarioPath)),
                ("schema", InputRngCaptureSchema)));
        }

        private static string BuildInputRngTickJson(
            SimulationWorld world,
            int completedTick,
            NTSD28NativeRandomScalarState previousRandom,
            NTSD28NativeRandomScalarState currentRandom,
            NativeRandomDirectCallRecorder directCalls)
        {
            if (currentRandom.CrtCalls < previousRandom.CrtCalls ||
                currentRandom.SynchronizedCalls <
                previousRandom.SynchronizedCalls)
            {
                throw new InvalidOperationException(
                    "Unity native RNG calls moved backwards during B2 capture.");
            }

            return BattleCanonicalJson.Serialize(DictionaryOf(
                ("completedTick", completedTick),
                ("entities", (object)ProjectExactInputEntities(world)),
                ("inputPhase", world.InputPhase),
                ("kind", "tick"),
                ("rng", (object)DictionaryOf(
                    ("crt", (object)DictionaryOf(
                        ("calls", (object)ProjectCrtCalls(
                            directCalls.CrtCalls)),
                        ("state", currentRandom.CrtState),
                        ("tickCallCount",
                            currentRandom.CrtCalls - previousRandom.CrtCalls),
                        ("totalCalls", currentRandom.CrtCalls))),
                    ("synchronized", (object)DictionaryOf(
                        ("calls", (object)ProjectSynchronizedCalls(
                            directCalls.SynchronizedCalls)),
                        ("state", (object)ProjectSynchronizedState(
                            currentRandom)),
                        ("tickCallCount",
                            currentRandom.SynchronizedCalls -
                            previousRandom.SynchronizedCalls),
                        ("totalCalls", currentRandom.SynchronizedCalls)))))));
        }

        private static object[] ProjectCrtCalls(
            IReadOnlyList<NTSD28NativeCrtCall> calls)
        {
            return calls.Select(call => (object)DictionaryOf(
                ("result", call.Result),
                ("stateAfter", call.StateAfter),
                ("totalCalls", call.TotalCalls))).ToArray();
        }

        private static object[] ProjectSynchronizedCalls(
            IReadOnlyList<NTSD28NativeSynchronizedCall> calls)
        {
            return calls.Select(call => (object)DictionaryOf(
                ("callSite", call.CallSite),
                ("counterAfter", call.CounterAfter),
                ("indexAfter", call.IndexAfter),
                ("result", call.Result),
                ("totalCalls", call.TotalCalls),
                ("upperBound", call.UpperBound))).ToArray();
        }

        private static SortedDictionary<string, object>
            ProjectInitialNativeRandom(NTSD28NativeRandomScalarState value)
        {
            SortedDictionary<string, object> synchronized =
                ProjectSynchronizedState(value);
            synchronized.Add("totalCalls", value.SynchronizedCalls);
            return DictionaryOf(
                ("crt", (object)DictionaryOf(
                    ("state", value.CrtState),
                    ("totalCalls", value.CrtCalls))),
                ("synchronized", (object)synchronized));
        }

        private static SortedDictionary<string, object>
            ProjectSynchronizedState(NTSD28NativeRandomScalarState value)
        {
            return DictionaryOf(
                ("counter", value.SynchronizedCounter),
                ("index", value.SynchronizedIndex),
                ("lastCallSite", value.LastSynchronizedCallSite),
                ("tableHash64", value.SynchronizedTableHash.ToString("X16")));
        }

        private static object[] ProjectExactInputEntities(
            SimulationWorld world)
        {
            var entities = new List<object>(
                world.ClaimedRuntimeSlotCountForDiagnostics);
            for (int slot = 0;
                 slot < world.RuntimeSlotCapacityForDiagnostics;
                 slot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed)
                {
                    continue;
                }

                LF2Entity entity = view.Entity;
                NTSDEntityRuntime runtime = entity?.Runtime;
                NTSD28InputProxyBlock input = runtime?.NativeInputProxy;
                if (entity == null || runtime == null || input == null ||
                    !input.HasCanonicalStorage ||
                    runtime.InputHistory == null ||
                    runtime.InputHistory.Length != 6 ||
                    runtime.InputRemapIndices13C == null ||
                    runtime.InputRemapIndices13C.Length !=
                    NTSDEntityRuntime.NativeInputRemapCount)
                {
                    throw new InvalidOperationException(
                        $"Runtime slot {slot} has no canonical B2 input state.");
                }

                entities.Add(DictionaryOf(
                    ("allocationEpoch", view.AllocationEpoch),
                    ("input", (object)ProjectExactInput(runtime, input)),
                    ("objectId", entity.ObjectId),
                    ("slot", slot)));
            }
            return entities.ToArray();
        }

        private static SortedDictionary<string, object> ProjectExactInput(
            NTSDEntityRuntime runtime,
            NTSD28InputProxyBlock input)
        {
            return DictionaryOf(
                ("boundState", runtime.BoundState198),
                ("comboState", (object)input.ComboState
                    .Select(value => (object)(int)value)
                    .ToArray()),
                ("currentMask", ProjectNativeInputMask(input.Current)),
                ("defendReentryCooldown", input.DefendReentryCooldown),
                ("edgeWindow", (object)DictionaryOf(
                    ("attack", input.EdgeWindow[0]),
                    ("jump", input.EdgeWindow[1]),
                    ("defend", input.EdgeWindow[2]),
                    ("right", input.EdgeWindow[3]),
                    ("left", input.EdgeWindow[4]),
                    ("up", input.EdgeWindow[5]),
                    ("down", input.EdgeWindow[6]))),
                ("globalRecordState", runtime.InputGlobalRecordState20),
                ("keyHistory", (object)new object[]
                {
                    runtime.InputHistory[1],
                    runtime.InputHistory[2],
                    runtime.InputHistory[3],
                    runtime.InputHistory[4],
                    runtime.InputHistory[5],
                }),
                ("lastAction", runtime.InputLastAction144),
                ("previousMask", ProjectNativeInputMask(input.Previous)),
                ("proxyTail", (int)input.ProxyTail),
                ("remapIndices", (object)runtime.InputRemapIndices13C
                    .Select(value => (object)(int)value)
                    .ToArray()),
                ("remapState", runtime.InputRemapState138),
                ("runAccumulator", runtime.AnimSub));
        }

        private static int ProjectNativeInputMask(byte[] values)
        {
            if (values == null || values.Length != 7)
            {
                throw new InvalidOperationException(
                    "Native input button storage is not seven bytes.");
            }

            int result = 0;
            if (values[3] != 0) result |= 1 << 0;
            if (values[2] != 0) result |= 1 << 1;
            if (values[0] != 0) result |= 1 << 2;
            if (values[1] != 0) result |= 1 << 3;
            if (values[4] != 0) result |= 1 << 4;
            if (values[5] != 0) result |= 1 << 5;
            if (values[6] != 0) result |= 1 << 6;
            return result;
        }

        private static SimulationInputButtons ProjectPhysicalCanonicalButtons(
            SimulationInputButtons crossedButtons)
        {
            const SimulationInputButtons directions =
                SimulationInputButtons.Right |
                SimulationInputButtons.Left |
                SimulationInputButtons.Up |
                SimulationInputButtons.Down;
            SimulationInputButtons result = crossedButtons & directions;
            if ((crossedButtons & SimulationInputButtons.Jump) != 0)
                result |= SimulationInputButtons.Attack;
            if ((crossedButtons & SimulationInputButtons.Defend) != 0)
                result |= SimulationInputButtons.Jump;
            if ((crossedButtons & SimulationInputButtons.Attack) != 0)
                result |= SimulationInputButtons.Defend;
            return result;
        }

        private static SortedDictionary<string, object> MissingRngStream()
        {
            return DictionaryOf(
                ("availability", "missing"),
                ("calls", null),
                ("state", null),
                ("tickCallCount", null),
                ("totalCalls", null));
        }

        private static List<DomainOccupant> CaptureDomainOccupants(
            SimulationWorld world)
        {
            var result = new List<DomainOccupant>(
                world.ClaimedRuntimeSlotCountForDiagnostics);
            for (int slot = 0;
                 slot < world.RuntimeSlotCapacityForDiagnostics;
                 slot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed)
                {
                    continue;
                }

                if (view.Entity == null || view.AllocationEpoch == 0)
                {
                    throw new InvalidOperationException(
                        $"Claimed domain slot {slot} has no entity or epoch.");
                }

                result.Add(new DomainOccupant(
                    slot,
                    view.AllocationEpoch,
                    view.Entity.ObjectId));
            }

            return result;
        }

        private static object[] ProjectDomainOccupants(
            IReadOnlyList<DomainOccupant> occupants)
        {
            return occupants.Select(occupant => (object)DictionaryOf(
                    ("allocationEpoch", occupant.AllocationEpoch),
                    ("objectId", occupant.ObjectId),
                    ("slot", occupant.Slot)))
                .ToArray();
        }

        private static object[] DeriveLifecycleEvents(
            IReadOnlyList<DomainOccupant> previous,
            IReadOnlyList<DomainOccupant> current)
        {
            Dictionary<int, DomainOccupant> previousBySlot = previous
                .ToDictionary(occupant => occupant.Slot);
            Dictionary<int, DomainOccupant> currentBySlot = current
                .ToDictionary(occupant => occupant.Slot);
            int[] slots = previousBySlot.Keys
                .Concat(currentBySlot.Keys)
                .Distinct()
                .OrderBy(slot => slot)
                .ToArray();
            var result = new List<object>();
            foreach (int slot in slots)
            {
                bool hadPrevious = previousBySlot.TryGetValue(
                    slot,
                    out DomainOccupant oldValue);
                bool hasCurrent = currentBySlot.TryGetValue(
                    slot,
                    out DomainOccupant newValue);
                if (!hadPrevious)
                {
                    result.Add(ProjectLifecycleEvent(
                        "birth",
                        slot,
                        null,
                        newValue.AllocationEpoch,
                        null,
                        newValue.ObjectId));
                    continue;
                }

                if (!hasCurrent)
                {
                    result.Add(ProjectLifecycleEvent(
                        "death",
                        slot,
                        oldValue.AllocationEpoch,
                        null,
                        oldValue.ObjectId,
                        null));
                    continue;
                }

                if (oldValue.AllocationEpoch == newValue.AllocationEpoch)
                {
                    if (oldValue.ObjectId != newValue.ObjectId)
                    {
                        throw new InvalidOperationException(
                            "Domain occupant changed without allocation epoch.");
                    }
                    continue;
                }

                if (newValue.AllocationEpoch <= oldValue.AllocationEpoch)
                {
                    throw new InvalidOperationException(
                        "Domain allocation epoch is not monotonic.");
                }

                result.Add(ProjectLifecycleEvent(
                    "reuse",
                    slot,
                    oldValue.AllocationEpoch,
                    newValue.AllocationEpoch,
                    oldValue.ObjectId,
                    newValue.ObjectId));
            }

            return result.ToArray();
        }

        private static object ProjectLifecycleEvent(
            string kind,
            int slot,
            ulong? previousAllocationEpoch,
            ulong? currentAllocationEpoch,
            int? previousObjectId,
            int? currentObjectId)
        {
            return DictionaryOf(
                ("currentAllocationEpoch", currentAllocationEpoch),
                ("currentObjectId", currentObjectId),
                ("kind", kind),
                ("previousAllocationEpoch", previousAllocationEpoch),
                ("previousObjectId", previousObjectId),
                ("slot", slot));
        }

        private static string ComputeFileSha256(string path)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha256.ComputeHash(stream))
                .Replace("-", string.Empty);
        }

        private static bool IsSha256(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 64)
                return false;
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if (!((character >= '0' && character <= '9') ||
                      (character >= 'a' && character <= 'f') ||
                      (character >= 'A' && character <= 'F')))
                {
                    return false;
                }
            }
            return true;
        }

        private static string ProjectPath(string path)
        {
            return Path.GetFullPath(
                Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(Environment.CurrentDirectory, path));
        }

        private static SortedDictionary<string, object> DictionaryOf(
            params (string Key, object Value)[] values)
        {
            var result = new SortedDictionary<string, object>(
                StringComparer.Ordinal);
            foreach ((string key, object value) in values)
                result.Add(key, value);
            return result;
        }

        private sealed class NativeRandomDirectCallRecorder :
            INTSD28NativeRandomCallObserver
        {
            internal readonly List<NTSD28NativeCrtCall> CrtCalls = new();
            internal readonly List<NTSD28NativeSynchronizedCall>
                SynchronizedCalls = new();

            internal void Clear()
            {
                CrtCalls.Clear();
                SynchronizedCalls.Clear();
            }

            public void OnCrtNext(NTSD28NativeCrtCall call)
            {
                CrtCalls.Add(call);
            }

            public void OnSynchronizedNext(
                NTSD28NativeSynchronizedCall call)
            {
                SynchronizedCalls.Add(call);
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
                host = new GameObject("__NTSD28_UnityRawCaptureDriver")
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
                if (Driver != null)
                {
                    BattleRuntimeShutdownReport report =
                        Driver.ShutdownBattleRuntime();
                    if (report.Status != BattleRuntimeShutdownStatus.Failed)
                    {
                        Driver.CompleteBattleRuntimeShutdownAfterMapCleanup(
                            true);
                    }
                }
                SetInstance(null);
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
                SetInstance(previousInstance);
            }

            private static void SetInstance(SimulationTickDriver value)
            {
                MethodInfo setter = InstanceProperty?.GetSetMethod(true);
                if (setter == null)
                {
                    throw new MissingMethodException(
                        "SimulationTickDriver singleton setter was not found.");
                }
                setter.Invoke(null, new object[] { value });
            }
        }

        private sealed class UnityCurrentDatScope : IDisposable
        {
            private readonly GameDataManager dataManager;
            private readonly CharacterAnimtorManager animationManager;
            private readonly bool ownsDataManager;
            private readonly bool ownsAnimationManager;
            private readonly FieldInfo cachedConfigField;
            private readonly FieldInfo objectLookupField;
            private readonly FieldInfo objectsByTypeField;
            private readonly FieldInfo backgroundLookupField;
            private readonly FieldInfo frameConfigField;
            private readonly object originalCachedConfig;
            private readonly object originalObjectLookup;
            private readonly object originalFrameConfig;
            private readonly List<DictionaryEntry> originalObjectsByType;
            private readonly List<DictionaryEntry> originalBackgroundLookup;

            private readonly LoganObjectCatalog loganCatalog;
            private readonly FieldInfo registryField;
            private readonly object originalRegistry;
            private readonly string originalPublishedKey;
            private readonly LoganContentIdentity originalPublishedIdentity;

            public UnityCurrentDatScope(IReadOnlyCollection<int> requestedObjectIds, string loganRuntimeRoot)
            {
                // Prepare source values before any singleton publication is changed.
                if (!string.IsNullOrWhiteSpace(loganRuntimeRoot))
                {
                    loganCatalog = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(loganRuntimeRoot));
                    Configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(loganCatalog);
                    Content = NTSD28TraceContentIdentity.FromLoganRaw(loganCatalog.DefinitionFingerprint);
                }
                else
                {
                    Content = NTSD28TraceContentIdentity.CaptureLegacy(ProjectPath(ProductionConfigRoot), ProjectPath(ProductionDataIndex));
                }
                ownsDataManager = !GameDataManager.HasInstance;
                dataManager = GameDataManager.Instance ??
                    throw new InvalidOperationException(
                        "GameDataManager singleton is unavailable.");
                if (ownsDataManager)
                    dataManager.gameObject.hideFlags = HideFlags.HideAndDontSave;

                ownsAnimationManager = !CharacterAnimtorManager.HasInstance;
                animationManager = CharacterAnimtorManager.Instance ??
                    throw new InvalidOperationException(
                        "CharacterAnimtorManager singleton is unavailable.");
                if (ownsAnimationManager)
                    animationManager.gameObject.hideFlags = HideFlags.HideAndDontSave;

                const BindingFlags flags =
                    BindingFlags.Instance | BindingFlags.NonPublic;
                cachedConfigField = RequireField(
                    typeof(GameDataManager), "cachedConfig", flags);
                objectLookupField = RequireField(
                    typeof(GameDataManager), "objectLookup", flags);
                objectsByTypeField = RequireField(
                    typeof(GameDataManager), "objectsByTypeLookup", flags);
                backgroundLookupField = RequireField(
                    typeof(GameDataManager), "backgroundLookup", flags);
                frameConfigField = RequireField(
                    typeof(CharacterAnimtorManager),
                    "TotalCharacterFrameConfig",
                    flags);

                originalCachedConfig = cachedConfigField.GetValue(dataManager);
                originalObjectLookup = objectLookupField.GetValue(dataManager);
                originalFrameConfig = frameConfigField.GetValue(animationManager);
                IDictionary objectsByType =
                    (IDictionary)objectsByTypeField.GetValue(dataManager);
                IDictionary backgroundLookup =
                    (IDictionary)backgroundLookupField.GetValue(dataManager);
                originalObjectsByType = Snapshot(objectsByType);
                originalBackgroundLookup = Snapshot(backgroundLookup);

                registryField = RequireField(typeof(GameDataManager), "objectRegistryIndices", flags);
                originalRegistry = registryField.GetValue(dataManager);
                originalPublishedKey = dataManager.PublishedVisualContentKey;
                originalPublishedIdentity = dataManager.PublishedLoganContentIdentity;
                try
                {
                    cachedConfigField.SetValue(dataManager, null);
                    objectLookupField.SetValue(dataManager, null);
                    objectsByType.Clear();
                    backgroundLookup.Clear();
                    registryField.SetValue(dataManager, new Dictionary<int, int>());
                    if (loganCatalog == null)
                    {
                        dataManager.LoadDataFile(ProjectPath(ProductionDataIndex));
                        Configs = LoadRequestedConfigs(requestedObjectIds);
                    }
                    else
                    {
                        var config = new GameDataConfig();
                        var lookup = new Dictionary<int, ObjectDefinition>();
                        var registry = new Dictionary<int, int>();
                        foreach (LoganObjectCatalog.Entry entry in loganCatalog.Entries)
                        {
                            var definition = new ObjectDefinition(entry.Id, entry.Type, entry.DatPath);
                            config.objects.Add(definition);
                            lookup.Add(entry.Id, definition);
                            registry.Add(entry.Id, entry.RegistryIndex);
                            if (!objectsByType.Contains(entry.Type))
                                objectsByType[entry.Type] = new List<ObjectDefinition>();
                            ((List<ObjectDefinition>)objectsByType[entry.Type]).Add(definition);
                        }
                        cachedConfigField.SetValue(dataManager, config);
                        objectLookupField.SetValue(dataManager, lookup);
                        registryField.SetValue(dataManager, registry);
                    }
                    typeof(GameDataManager).GetProperty("PublishedVisualContentKey").SetValue(dataManager, null);
                    typeof(GameDataManager).GetProperty("PublishedLoganContentIdentity").SetValue(dataManager, loganCatalog?.ContentIdentity);
                    frameConfigField.SetValue(animationManager, Configs);
                    AssertInputsCurrent();
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public Dictionary<int, LF2CharacterDataWrapper> Configs { get; }
            internal Dictionary<string, object> Content { get; }
            internal bool IsLogan => loganCatalog != null;
            internal LoganObjectCatalog Catalog => loganCatalog;

            internal void AssertInputsCurrent()
            {
                Dictionary<string, object> current = loganCatalog == null
                    ? NTSD28TraceContentIdentity.CaptureLegacy(ProjectPath(ProductionConfigRoot), ProjectPath(ProductionDataIndex))
                    : NTSD28TraceContentIdentity.FromLoganRaw(LoganObjectCatalog.Read(loganCatalog.Source).DefinitionFingerprint);
                if (!Equals(current["semanticSha256"], Content["semanticSha256"]))
                    throw new InvalidDataException("Capture content inputs changed during loading or simulation.");
            }

            public void Dispose()
            {
                if (animationManager != null)
                {
                    if (ownsAnimationManager)
                    {
                        UnityEngine.Object.DestroyImmediate(
                            animationManager.gameObject);
                    }
                    else
                    {
                        frameConfigField.SetValue(
                            animationManager,
                            originalFrameConfig);
                    }
                }

                if (dataManager != null)
                {
                    if (ownsDataManager)
                    {
                        UnityEngine.Object.DestroyImmediate(dataManager.gameObject);
                    }
                    else
                    {
                        cachedConfigField.SetValue(dataManager, originalCachedConfig);
                        objectLookupField.SetValue(dataManager, originalObjectLookup);
                        registryField.SetValue(dataManager, originalRegistry);
                        typeof(GameDataManager).GetProperty("PublishedVisualContentKey").SetValue(dataManager, originalPublishedKey);
                        typeof(GameDataManager).GetProperty("PublishedLoganContentIdentity").SetValue(dataManager, originalPublishedIdentity);
                        Restore(
                            (IDictionary)objectsByTypeField.GetValue(dataManager),
                            originalObjectsByType);
                        Restore(
                            (IDictionary)backgroundLookupField.GetValue(dataManager),
                            originalBackgroundLookup);
                    }
                }
            }

            private Dictionary<int, LF2CharacterDataWrapper> LoadRequestedConfigs(
                IReadOnlyCollection<int> requestedObjectIds)
            {
                MethodInfo buildMethod = typeof(CharacterAnimtorManager).GetMethod(
                    "BuildCharacterDataFromDat",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (buildMethod == null)
                {
                    throw new MissingMethodException(
                        "CharacterAnimtorManager.BuildCharacterDataFromDat was not found.");
                }

                string configRoot = ProjectPath(ProductionConfigRoot)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var result = new Dictionary<int, LF2CharacterDataWrapper>();
                foreach (int objectId in requestedObjectIds.OrderBy(value => value))
                {
                    ObjectDefinition definition = dataManager.GetObjectById(objectId);
                    if (definition == null)
                        continue;

                    string datPath = Path.ChangeExtension(
                        ProjectPath(definition.file),
                        ".dat");
                    if (!IsUnderRoot(datPath, configRoot))
                    {
                        throw new InvalidDataException(
                            $"Unity object {objectId} resolves outside Assets/NTSD/Config.");
                    }
                    if (!File.Exists(datPath))
                    {
                        throw new FileNotFoundException(
                            $"Unity DAT for object {objectId} was not found.",
                            datPath);
                    }

                    string datText = Lf2DatDecryptor.DecryptFile(
                        datPath,
                        DatPassword);
                    Lf2DatFile datFile = new Lf2DatParserV2().Parse(
                        datText,
                        datPath);
                    if (datFile == null || datFile.Frames == null ||
                        datFile.Frames.Count == 0)
                    {
                        throw new InvalidDataException(
                            $"Unity DAT for object {objectId} has no parsed frames.");
                    }

                    LF2CharacterData characterData = buildMethod.Invoke(
                        animationManager,
                        new object[] { datFile, Path.GetDirectoryName(datPath) })
                        as LF2CharacterData;
                    if (characterData == null)
                    {
                        throw new InvalidDataException(
                            $"Unity DAT conversion failed for object {objectId}.");
                    }
                    if (characterData.type_sub == 0)
                        characterData.type_sub = objectId;
                    result.Add(
                        objectId,
                        new LF2CharacterDataWrapper(objectId, characterData));
                }
                return result;
            }

            private static bool IsUnderRoot(string candidate, string root)
            {
                string prefix = root + Path.DirectorySeparatorChar;
                return candidate.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase);
            }

            private static FieldInfo RequireField(
                Type owner,
                string name,
                BindingFlags flags)
            {
                return owner.GetField(name, flags) ??
                    throw new MissingFieldException(owner.FullName, name);
            }

            private static List<DictionaryEntry> Snapshot(IDictionary source)
            {
                var result = new List<DictionaryEntry>(source.Count);
                foreach (DictionaryEntry entry in source)
                    result.Add(entry);
                return result;
            }

            private static void Restore(
                IDictionary destination,
                IReadOnlyList<DictionaryEntry> snapshot)
            {
                destination.Clear();
                for (int index = 0; index < snapshot.Count; index++)
                {
                    DictionaryEntry entry = snapshot[index];
                    destination.Add(entry.Key, entry.Value);
                }
            }
        }

        [Serializable]
        private sealed class CaptureRequest
        {
            public string scenarioPath;
            public string outputPath;
            public string domainOutputPath;
            public string inputRngOutputPath;
            public string loganRuntimeRoot;
        }

        [Serializable]
        private sealed class UnityRawScenario
        {
            public string schema;
            public string referenceExeSha256;
            public string dataSha256;
            public string mode;
            public int seed;
            public int ticks;
            public bool emitInitial;
            public int battleMode;
            public int stageId;
            public int difficultyLevel4A0C30;
            public UnityRawCombatant[] combatants;
            public UnityRawInput[] inputs;
        }

        [Serializable]
        private sealed class UnityRawCombatant
        {
            public int slot;
            public int oid;
            public int team;
            public int x;
            public int y;
            public int z;
            public int hp;
            public int baseHp;
            public int mp;
            public int facing;
            public int reviveLives30c;
            public int reviveNextLives310;
            public int reviveNextHp314;
            public int renderPhase008;
            public bool nativeAi;
            public int nativeComputerState1b8;
        }

        [Serializable]
        private sealed class UnityRawInput
        {
            public int tick;
            public int slot;
            public string[] keys;
        }

        private sealed class DomainOccupant
        {
            public DomainOccupant(
                int slot,
                ulong allocationEpoch,
                int objectId)
            {
                Slot = slot;
                AllocationEpoch = allocationEpoch;
                ObjectId = objectId;
            }

            public int Slot { get; }
            public ulong AllocationEpoch { get; }
            public int ObjectId { get; }
        }
    }
}
