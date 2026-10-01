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
        private const string Q07DdjScenarioSchema = "ntsd28-q07-ddj/1.0";
        private const string Q07SasukeNeedleScenarioSchema =
            "ntsd28-q07-sasuke-needle/1.0";
        private const string Q07SasukeNeedleTargetHitScenarioSchema =
            "ntsd28-q07-sasuke-needle-target-hit/1.0";
        private const string Q07SasukeArmorTargetScenarioSchema =
            "ntsd28-q07-sasuke-armor-target/1.0";
        private const string Q08SelectedArmorLethalScenarioSchema =
            "ntsd28-q08-selected-armor-lethal/1.0";
        private const string R06NaturalRevivalScenarioSchema =
            "ntsd28-r06-natural-revival/1.0";
        private const string Q08SelectedArmorResultMilestonesScenarioSchema =
            "ntsd28-q08-selected-armor-result-milestones/1.0";
        private const string Q08NarutoCloneStageGateScenarioSchema =
            "ntsd28-q08-naruto-clone-stage-gate/1.0";
        private const string Q07HidanCatchScenarioSchema =
            "ntsd28-q07-hidan-catch/1.0";
        private const string Q07HidanNaturalCatchScenarioSchema =
            "ntsd28-q07-hidan-catch-natural/1.0";
        private const string Q07C050Bdy50ScenarioSchema =
            "ntsd28-336b44-q07-c050-bdy50/1.0";
        private const string Q07C051Effect23ScenarioSchema =
            "ntsd28-336b44-q07-c051-effect23/1.0";
        private const string Q09HidanFrame430ScenarioSchema =
            "ntsd28-q09-hidan-frame430-natural/1.0";
        private const string Q07LeeJlChildScenarioSchema =
            "ntsd28-q07-lee-jl-child/1.0";
        private const string Q07NarutoPunchScenarioSchema =
            "ntsd28-q07-naruto-punch/1.0";
        private const string Q07HeldWeaponMotionScenarioSchema =
            "ntsd28-q07-naruto-held-weapon-motion/1.0";
        private const string Q07N30LateInputScenarioSchema =
            "ntsd28-q07-n30-late-input/1.0";
        private const string Q07HitFa5FullDriverScenarioSchema =
            "ntsd28-q07-hitfa5-full-driver/1.0";
        private const string Q07FusionFullDriverScenarioSchema =
            "ntsd28-q07-fusion-full-driver/1.0";
        private const string Q07RevivalPeerFullDriverScenarioSchema =
            "ntsd28-q07-revival-peer-full-driver/1.0";
        private const string Q07KnockoutEventCaptureSchema =
            "ntsd28-q07-knockout-events/1.0";
        private const string Q08ResultFlowCaptureSchema =
            "ntsd28-q08-result-flow/1.0";
        private const string Q08TerminalHostFreezeCaptureSchema =
            "ntsd28-q08-terminal-host-freeze/1.0";
        private const string Q08State12KoFullTickScenarioSchema =
            "ntsd28-q08-state12-ko-full-tick/1.0";
        private const string Q08NegativeEnvironmentKoFullTickScenarioSchema =
            "ntsd28-q08-negative-environment-ko-full-tick/1.0";
        private const string Q08HeldCpointKoFullTickScenarioSchema =
            "ntsd28-q08-held-cpoint-ko-full-tick/1.0";
        private const int Q07DdjSeed = 0x28A55A5A;
        private const int Q07LeeJlChildSeed = 682973786;
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
            Action<SimulationTickDriver, FrameInputSet[], LockstepSessionIdentity> verify,
            bool useProjectMode = false)
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
            using var dataScope = new UnityCurrentDatScope(
                scenario.combatants.Select(value => value.oid).ToArray(),
                runtimeRoot, useProjectMode);
            ulong fixtureIdentity = LoganContentIdentity.ForDecodeContract(ComputeFileSha256(path),
                "NTSD28_Q05_SCENARIO_FIXTURE_V1").CatalogFingerprint;
            LockstepSessionIdentity identity = dataScope.Catalog.ContentIdentity.CreateLocalValidationSessionIdentity(
                0x51305UL, unchecked((uint)scenario.seed), fixtureIdentity, slots);
            using var poolScope = useProjectMode ? new TemporaryObjectPoolScope() : null;
            SimulationWorld observedWorld;
            BattleLogicReferencePool observedPool;
            var driverScope = new TemporarySimulationDriverScope();
            using (driverScope)
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
                ConfigureWorldAndRoster(observedWorld, dataScope.Configs, scenario, true,
                    dataScope.Catalog);
                observedWorld.PrepareRuntimeDataCatalogForBattle(dataScope.Catalog.Entries
                    .Select(entry => new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                    id => dataScope.Configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper) ? wrapper : null,
                    loganCatalog: dataScope.Catalog);
                driver.ApplySettings(new LockstepSimulationSettings
                {
                    driveMode = SimulationDriveMode.Manual,
                    enableFrameChecksum = true,
                });
                driver.SetPaused(false);
                if (useProjectMode)
                    driver.BeginBattleAllocationSeal();
                observedWorld.SetLogicOnlyEntityMaterialization(true);
                dataScope.AssertInputsCurrent();
                verify(driver, inputs, identity);
                dataScope.AssertInputsCurrent();
            }
            if (observedWorld.ObjectCount != 0 || observedWorld.ClaimedRuntimeSlotCountForDiagnostics != 0 || observedPool.ActiveCount != 0)
                throw new InvalidOperationException("Replay validation shutdown retained objects=" + observedWorld.ObjectCount +
                    ", slots=" + observedWorld.ClaimedRuntimeSlotCountForDiagnostics + ", borrowers=" + observedPool.ActiveCount +
                    ", shutdown=" + driverScope.LastShutdownReport.Status +
                    ", stage=" + driverScope.LastShutdownReport.CompletedStage +
                    ", reason=" + driverScope.LastShutdownReport.FailureReason);
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
            string inputRngOutputPath,
            string domainVersion = null)
        {
            return RunScenario(
                scenarioPath,
                outputPath,
                domainOutputPath,
                inputRngOutputPath,
                domainVersion: domainVersion);
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
                    request.loganRuntimeRoot,
                    request.domainVersion,
                    request.resultPath,
                    request.knockoutOutputPath,
                    request.resultFlowOutputPath,
                    request.terminalFreezeOutputPath);
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
            string loganRuntimeRoot = null,
            string domainVersion = null,
            string requestedResultPath = null,
            string knockoutOutputPath = null,
            string resultFlowOutputPath = null,
            string terminalFreezeOutputPath = null)
        {
            string resultPath = ProjectPath(string.IsNullOrWhiteSpace(requestedResultPath)
                ? ResultFile : requestedResultPath);
            Directory.CreateDirectory(
                Path.GetDirectoryName(resultPath) ?? ProjectPath("Temp"));
            if (!string.IsNullOrWhiteSpace(requestedResultPath) && File.Exists(resultPath))
            {
                Debug.LogError("[NTSD28UnityRawCapture] Refusing to overwrite result: " + resultPath);
                return;
            }
            if (!string.IsNullOrWhiteSpace(knockoutOutputPath) &&
                string.Equals(ProjectPath(knockoutOutputPath), resultPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[NTSD28UnityRawCapture] KO output and result paths must differ.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(resultFlowOutputPath) &&
                string.Equals(ProjectPath(resultFlowOutputPath), resultPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[NTSD28UnityRawCapture] Result-flow output and result paths must differ.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(terminalFreezeOutputPath) &&
                string.Equals(ProjectPath(terminalFreezeOutputPath), resultPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[NTSD28UnityRawCapture] Terminal output and result paths must differ.");
                return;
            }
            try
            {
                string resolvedOutput = RunScenario(
                    scenarioPath,
                    outputPath,
                    domainOutputPath,
                    inputRngOutputPath,
                    loganRuntimeRoot,
                    domainVersion,
                    knockoutOutputPath,
                    resultFlowOutputPath,
                    terminalFreezeOutputPath);
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
            string loganRuntimeRoot = null,
            string domainVersion = null,
            string knockoutOutputPath = null,
            string resultFlowOutputPath = null,
            string terminalFreezeOutputPath = null)
        {
            if (domainVersion != null &&
                ((domainVersion != "v1" && domainVersion != "v2") ||
                 string.IsNullOrWhiteSpace(domainOutputPath)))
            {
                throw new InvalidDataException(
                    "Explicit domainVersion must be v1 or v2 and requires domainOutputPath.");
            }
            domainVersion ??= "v1";
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
            string resolvedKnockoutOutputPath = string.IsNullOrWhiteSpace(
                knockoutOutputPath)
                ? null
                : ProjectPath(knockoutOutputPath);
            string resolvedResultFlowOutputPath = string.IsNullOrWhiteSpace(
                resultFlowOutputPath)
                ? null
                : ProjectPath(resultFlowOutputPath);
            string resolvedTerminalFreezeOutputPath = string.IsNullOrWhiteSpace(
                terminalFreezeOutputPath)
                ? null
                : ProjectPath(terminalFreezeOutputPath);
            RequireDistinctOutputPaths(
                resolvedOutputPath,
                resolvedDomainOutputPath,
                resolvedInputRngOutputPath,
                resolvedKnockoutOutputPath,
                resolvedResultFlowOutputPath,
                resolvedTerminalFreezeOutputPath);
            UnityRawScenario scenario = JsonUtility.FromJson<UnityRawScenario>(
                File.ReadAllText(resolvedScenarioPath, Encoding.UTF8));
            ValidateScenario(scenario);
            if (resolvedTerminalFreezeOutputPath != null &&
                (scenario.schema != Q08SelectedArmorResultMilestonesScenarioSchema ||
                 scenario.ticks != 366))
                throw new InvalidDataException(
                    "Terminal freeze capture requires the exact Q08 366-call scenario.");
            if (resolvedTerminalFreezeOutputPath != null &&
                File.Exists(resolvedTerminalFreezeOutputPath))
                throw new IOException("Refusing to overwrite terminal freeze output: " +
                                      resolvedTerminalFreezeOutputPath);
            bool q08NarutoCloneStageGate =
                scenario.schema == Q08NarutoCloneStageGateScenarioSchema;
            if (scenario.hasSelectedStageGateOverride &&
                (!q08NarutoCloneStageGate || scenario.selectedStageGateOverride < 0))
                throw new InvalidDataException(
                    "Stage-gate override requires the Q08 Naruto clone schema and a nonnegative value.");
            string cloneStageSourceOutputPath = q08NarutoCloneStageGate
                ? resolvedOutputPath + ".stage-source.jsonl"
                : null;
            if (cloneStageSourceOutputPath != null &&
                File.Exists(cloneStageSourceOutputPath))
                throw new IOException("Refusing to overwrite clone stage-source output: " +
                                      cloneStageSourceOutputPath);
            bool formalQ07Scenario = scenario.schema == Q07DdjScenarioSchema ||
                scenario.schema == Q07SasukeNeedleScenarioSchema ||
                scenario.schema == Q07SasukeNeedleTargetHitScenarioSchema ||
                scenario.schema == Q07SasukeArmorTargetScenarioSchema ||
                scenario.schema == Q08SelectedArmorLethalScenarioSchema ||
                scenario.schema == Q08SelectedArmorResultMilestonesScenarioSchema ||
                scenario.schema == Q07HidanCatchScenarioSchema ||
                scenario.schema == Q07HidanNaturalCatchScenarioSchema ||
                scenario.schema == Q07C050Bdy50ScenarioSchema ||
                scenario.schema == Q07C051Effect23ScenarioSchema ||
                scenario.schema == Q09HidanFrame430ScenarioSchema ||
                scenario.schema == Q07LeeJlChildScenarioSchema ||
                q08NarutoCloneStageGate ||
                scenario.schema == Q07NarutoPunchScenarioSchema ||
                scenario.schema == Q07HeldWeaponMotionScenarioSchema ||
                scenario.schema == Q07N30LateInputScenarioSchema;
            if (resolvedKnockoutOutputPath != null &&
                scenario.schema != Q07NarutoPunchScenarioSchema &&
                scenario.schema != Q08SelectedArmorLethalScenarioSchema &&
                scenario.schema != Q08SelectedArmorResultMilestonesScenarioSchema)
                throw new InvalidDataException("KO-event capture requires an approved formal scenario.");
            if (resolvedKnockoutOutputPath != null && File.Exists(resolvedKnockoutOutputPath))
                throw new IOException("Refusing to overwrite KO-event output: " + resolvedKnockoutOutputPath);
            if (resolvedResultFlowOutputPath != null &&
                scenario.schema != Q08SelectedArmorLethalScenarioSchema &&
                scenario.schema != Q08SelectedArmorResultMilestonesScenarioSchema)
                throw new InvalidDataException("Result-flow capture requires the exact Q08 selected-armor lethal scenario.");
            if (resolvedResultFlowOutputPath != null && File.Exists(resolvedResultFlowOutputPath))
                throw new IOException("Refusing to overwrite result-flow output: " + resolvedResultFlowOutputPath);
            if (formalQ07Scenario && string.IsNullOrWhiteSpace(loganRuntimeRoot))
                throw new InvalidDataException("Q07 formal trace requires the selected Logan runtime root.");

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
            if (resolvedKnockoutOutputPath != null)
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(resolvedKnockoutOutputPath) ??
                    ProjectPath("Temp"));
            }
            if (resolvedResultFlowOutputPath != null)
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(resolvedResultFlowOutputPath) ??
                    ProjectPath("Temp"));
            }
            if (resolvedTerminalFreezeOutputPath != null)
                Directory.CreateDirectory(
                    Path.GetDirectoryName(resolvedTerminalFreezeOutputPath) ??
                    ProjectPath("Temp"));

            FrameInputSet[] frameInputs = BuildFrameInputs(scenario);

            int[] requestedObjectIds = scenario.combatants
                .Select(combatant => combatant.oid)
                .Distinct()
                .OrderBy(value => value)
                .ToArray();

            using var dataScope = new UnityCurrentDatScope(
                requestedObjectIds, loganRuntimeRoot, formalQ07Scenario);
            dataScope.AssertInputsCurrent();
            if (formalQ07Scenario &&
                !string.Equals(dataScope.Catalog?.CatalogSha256,
                    scenario.dataSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Formal staged catalog SHA-256 differs: expected {scenario.dataSha256}, " +
                    $"actual {dataScope.Catalog?.CatalogSha256}.");
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
            bool requiresLogicOnlyMaterialization = formalQ07Scenario ||
                dataScope.Catalog?.Entries.Any(entry =>
                requestedObjectIds.Contains(entry.Id) &&
                entry.Type == (int)LF2ObjectType.SpecialAttack) == true;
            ConfigureWorldAndRoster(world, dataScope.Configs, scenario, dataScope.IsLogan,
                dataScope.Catalog);
            if (dataScope.IsLogan)
                world.PrepareRuntimeDataCatalogForBattle(dataScope.Catalog.Entries
                    .Select(entry => new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                    id => dataScope.Configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper) ? wrapper : null,
                    loganCatalog: dataScope.Catalog);
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
            if (requiresLogicOnlyMaterialization)
            {
                driver.BeginBattleAllocationSeal();
                world.SetLogicOnlyEntityMaterialization(true);
            }
            if (q08NarutoCloneStageGate && scenario.hasSelectedStageGateOverride)
                world.Runtime.SelectedModeStageGate50 =
                    scenario.selectedStageGateOverride;

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
            StreamWriter knockoutWriter = resolvedKnockoutOutputPath == null
                ? null
                : new StreamWriter(
                    resolvedKnockoutOutputPath,
                    false,
                    new UTF8Encoding(false));
            StreamWriter resultFlowWriter = resolvedResultFlowOutputPath == null
                ? null
                : new StreamWriter(
                    resolvedResultFlowOutputPath,
                    false,
                    new UTF8Encoding(false));
            StreamWriter terminalFreezeWriter =
                resolvedTerminalFreezeOutputPath == null
                    ? null
                    : new StreamWriter(
                        resolvedTerminalFreezeOutputPath,
                        false,
                        new UTF8Encoding(false));
            StreamWriter cloneStageSourceWriter = cloneStageSourceOutputPath == null
                ? null
                : new StreamWriter(
                    cloneStageSourceOutputPath,
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
                    previousOccupants,
                    domainVersion));
                inputRngWriter?.WriteLine(BuildInputRngHeaderJson(
                    resolvedScenarioPath,
                    scenario,
                    world,
                    previousNativeRandom));
                knockoutWriter?.WriteLine(BattleCanonicalJson.Serialize(DictionaryOf(
                    ("kind", "header"),
                    ("schema", Q07KnockoutEventCaptureSchema),
                    ("scenarioFileSha256", ComputeFileSha256(resolvedScenarioPath)),
                    ("formalAuthorityExeSha256", FormalAuthorityExeSha256),
                    ("expectedTickCount", scenario.ticks))));
                resultFlowWriter?.WriteLine(BattleCanonicalJson.Serialize(DictionaryOf(
                    ("kind", "header"),
                    ("schema", Q08ResultFlowCaptureSchema),
                    ("scenarioFileSha256", ComputeFileSha256(resolvedScenarioPath)),
                    ("formalAuthorityExeSha256", FormalAuthorityExeSha256),
                    ("expectedTickCount", scenario.ticks))));
                terminalFreezeWriter?.WriteLine(BattleCanonicalJson.Serialize(DictionaryOf(
                    ("kind", "header"),
                    ("schema", Q08TerminalHostFreezeCaptureSchema),
                    ("certificateEligible", false),
                    ("scenarioFileSha256", ComputeFileSha256(resolvedScenarioPath)),
                    ("formalAuthorityExeSha256", FormalAuthorityExeSha256),
                    ("sampledCalls", (object)new[] { 365, 366 }))));
                cloneStageSourceWriter?.WriteLine(BattleCanonicalJson.Serialize(DictionaryOf(
                    ("kind", "header"),
                    ("schema", Q08NarutoCloneStageGateScenarioSchema),
                    ("scenarioFileSha256", ComputeFileSha256(resolvedScenarioPath)),
                    ("formalAuthorityExeSha256", FormalAuthorityExeSha256),
                    ("expectedTickCount", scenario.ticks))));

                int exactCharacterCountExpected = scenario.combatants.Count(combatant =>
                    dataScope.Catalog == null || dataScope.Catalog.Entries.Any(entry =>
                        entry.Id == combatant.oid && entry.Type == (int)LF2ObjectType.Character));
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
                    bool transitionFrozeCombat =
                        scenario.schema == Q08SelectedArmorResultMilestonesScenarioSchema &&
                        world.Runtime.Results.NativeTransitionState != 0;
                    int expectedExactCharacterCount =
                        q08NarutoCloneStageGate && tick >= 2
                            ? exactCharacterCountExpected + 1
                            : exactCharacterCountExpected;
                    if (exactCharacterDelta !=
                        (transitionFrozeCombat ? 0 : expectedExactCharacterCount))
                    {
                        throw new InvalidOperationException(
                            $"Unity tick {tick} did not reach the exact character " +
                            $"frame-tick boundary: expected {expectedExactCharacterCount}, " +
                            $"actual {exactCharacterDelta}.");
                    }

                    writer.WriteLine(
                        NTSD28UnityEntityRawCapture.CaptureTickJson(world, tick));
                    writer.Flush();
                    if (cloneStageSourceWriter != null)
                    {
                        LF2Entity clone = world.FindEntityByRuntimeSlotForQuery(50);
                        if ((tick == 1 && clone != null) ||
                            (tick >= 2 && (clone == null || clone.ObjectId != 33 ||
                                           clone.Runtime.SlotIndex != 50)))
                            throw new InvalidOperationException(
                                $"Unity tick {tick} clone birth/identity differs from the exact fixture.");
                        cloneStageSourceWriter.WriteLine(BattleCanonicalJson.Serialize(DictionaryOf(
                            ("kind", "tick"),
                            ("schema", Q08NarutoCloneStageGateScenarioSchema),
                            ("completedTick", tick),
                            ("clonePresent", clone != null),
                            ("cloneSlot", clone?.Runtime.SlotIndex ?? -1),
                            ("cloneOid", clone?.ObjectId ?? -1),
                            ("cloneSourceRuleInitialized",
                                clone?.Runtime.SourceRulePositionInitialized ?? false),
                            ("cloneSourceRuleX", clone?.Runtime.SourceRuleX ?? 0.0),
                            ("cloneSourceRuleXInt", clone?.Runtime.SourceRuleXInt ?? 0),
                            ("clonePhysicalX", clone?.Runtime.X ?? 0.0))));
                        cloneStageSourceWriter.Flush();
                    }
                    if (knockoutWriter != null)
                    {
                        knockoutWriter.WriteLine(BuildKnockoutTickJson(world, tick));
                        knockoutWriter.Flush();
                    }
                    if (resultFlowWriter != null)
                    {
                        resultFlowWriter.WriteLine(BuildResultFlowTickJson(world, tick));
                        resultFlowWriter.Flush();
                    }
                    if (terminalFreezeWriter != null && tick >= 365)
                    {
                        BattleParityFrameSnapshot snapshot =
                            world.CaptureParityFrameSnapshot(365);
                        BattleResultsRuntimeState results = world.Runtime.Results;
                        NTSD28NativeRandomScalarState nativeRandom =
                            world.NativeRandom.CaptureScalarState();
                        terminalFreezeWriter.WriteLine(
                            BattleCanonicalJson.Serialize(DictionaryOf(
                                ("kind", "hostCall"),
                                ("schema", Q08TerminalHostFreezeCaptureSchema),
                                ("completedCall", tick),
                                ("driverTick", driver.CurrentTickIndex),
                                ("snapshotTickArgument", snapshot.Tick),
                                ("objectCount", snapshot.ObjectCount),
                                ("claimedSlots", world.ClaimedRuntimeSlotCountForDiagnostics),
                                ("slotsHash", snapshot.Hashes.Slots),
                                ("rngHash", snapshot.Hashes.Rng),
                                ("nativeRandom", (object)DictionaryOf(
                                    ("crtState", nativeRandom.CrtState),
                                    ("crtCalls", nativeRandom.CrtCalls),
                                    ("tableSeed", nativeRandom.TableSeed),
                                    ("synchronizedCounter", nativeRandom.SynchronizedCounter),
                                    ("synchronizedIndex", nativeRandom.SynchronizedIndex),
                                    ("synchronizedCalls", nativeRandom.SynchronizedCalls),
                                    ("lastSynchronizedCallSite",
                                        nativeRandom.LastSynchronizedCallSite),
                                    ("synchronizedTableHash",
                                        nativeRandom.SynchronizedTableHash.ToString("X16")),
                                    ("synchronizedGeneration",
                                        nativeRandom.SynchronizedGeneration))),
                                ("worldHash", snapshot.Hashes.World),
                                ("overallHash", snapshot.OverallChecksum),
                                ("resultOutputTimer", results.NativeResultOutputTimer),
                                ("resultStoredTimer", results.NativeResultTimer),
                                ("resultPhase", results.NativeResultPhase),
                                ("transitionState", results.NativeTransitionState))));
                        terminalFreezeWriter.Flush();
                    }
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
                            currentOccupants,
                            domainVersion));
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
                knockoutWriter?.Dispose();
                resultFlowWriter?.Dispose();
                terminalFreezeWriter?.Dispose();
                cloneStageSourceWriter?.Dispose();
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
            bool q07DdjScenario = string.Equals(
                scenario.schema, Q07DdjScenarioSchema, StringComparison.Ordinal);
            bool q07SasukeNeedleScenario = string.Equals(
                scenario.schema, Q07SasukeNeedleScenarioSchema, StringComparison.Ordinal);
            bool q07SasukeNeedleTargetHitScenario = string.Equals(
                scenario.schema, Q07SasukeNeedleTargetHitScenarioSchema, StringComparison.Ordinal);
            bool q07SasukeArmorTargetScenario = string.Equals(
                scenario.schema, Q07SasukeArmorTargetScenarioSchema, StringComparison.Ordinal);
            bool q08SelectedArmorLethalScenario = string.Equals(
                scenario.schema, Q08SelectedArmorLethalScenarioSchema, StringComparison.Ordinal);
            bool r06NaturalRevivalScenario = string.Equals(
                scenario.schema, R06NaturalRevivalScenarioSchema, StringComparison.Ordinal);
            bool q08SelectedArmorResultMilestonesScenario = string.Equals(
                scenario.schema, Q08SelectedArmorResultMilestonesScenarioSchema,
                StringComparison.Ordinal);
            bool q08NarutoCloneStageGateScenario = string.Equals(
                scenario.schema, Q08NarutoCloneStageGateScenarioSchema,
                StringComparison.Ordinal);
            bool q08SelectedArmorFixture = q08SelectedArmorLethalScenario ||
                q08SelectedArmorResultMilestonesScenario;
            bool q07HidanCatchScenario = string.Equals(
                scenario.schema, Q07HidanCatchScenarioSchema, StringComparison.Ordinal);
            bool q07HidanNaturalCatchScenario = string.Equals(
                scenario.schema, Q07HidanNaturalCatchScenarioSchema, StringComparison.Ordinal);
            bool q07C050Bdy50Scenario = string.Equals(
                scenario.schema, Q07C050Bdy50ScenarioSchema, StringComparison.Ordinal);
            bool q07C051Effect23Scenario = string.Equals(
                scenario.schema, Q07C051Effect23ScenarioSchema, StringComparison.Ordinal);
            bool q09HidanFrame430Scenario = string.Equals(
                scenario.schema, Q09HidanFrame430ScenarioSchema, StringComparison.Ordinal);
            bool q07LeeJlChildScenario = string.Equals(
                scenario.schema, Q07LeeJlChildScenarioSchema, StringComparison.Ordinal);
            bool q07NarutoPunchScenario = string.Equals(
                scenario.schema, Q07NarutoPunchScenarioSchema, StringComparison.Ordinal);
            bool q07HeldWeaponMotionScenario = string.Equals(
                scenario.schema, Q07HeldWeaponMotionScenarioSchema, StringComparison.Ordinal);
            bool q07N30LateInputScenario = string.Equals(
                scenario.schema, Q07N30LateInputScenarioSchema, StringComparison.Ordinal);
            bool q07HitFa5FullDriverScenario = string.Equals(
                scenario.schema, Q07HitFa5FullDriverScenarioSchema, StringComparison.Ordinal);
            bool q07FusionFullDriverScenario = string.Equals(
                scenario.schema, Q07FusionFullDriverScenarioSchema, StringComparison.Ordinal);
            bool q07RevivalPeerFullDriverScenario = string.Equals(
                scenario.schema, Q07RevivalPeerFullDriverScenarioSchema,
                StringComparison.Ordinal);
            bool q08State12KoFullTickScenario = string.Equals(
                scenario.schema, Q08State12KoFullTickScenarioSchema,
                StringComparison.Ordinal);
            bool q08NegativeEnvironmentKoFullTickScenario = string.Equals(
                scenario.schema, Q08NegativeEnvironmentKoFullTickScenarioSchema,
                StringComparison.Ordinal);
            bool q08HeldCpointKoFullTickScenario = string.Equals(
                scenario.schema, Q08HeldCpointKoFullTickScenarioSchema,
                StringComparison.Ordinal);
            bool formalReleaseScenario = q07DdjScenario || q07SasukeNeedleScenario ||
                                     q07SasukeNeedleTargetHitScenario ||
                                     q07SasukeArmorTargetScenario ||
                                     q08SelectedArmorFixture ||
                                     r06NaturalRevivalScenario ||
                                      q07HidanCatchScenario || q07HidanNaturalCatchScenario ||
                                      q07C050Bdy50Scenario ||
                                      q07C051Effect23Scenario ||
                                      q09HidanFrame430Scenario ||
                                     q07LeeJlChildScenario ||
                                     q07NarutoPunchScenario || q07HeldWeaponMotionScenario ||
                                     q07N30LateInputScenario ||
                                      q07HitFa5FullDriverScenario || q07FusionFullDriverScenario ||
                                       q07RevivalPeerFullDriverScenario ||
                                       q08NarutoCloneStageGateScenario ||
                        q08State12KoFullTickScenario ||
                                      q08NegativeEnvironmentKoFullTickScenario ||
                                      q08HeldCpointKoFullTickScenario;
            if (!formalReleaseScenario && !string.Equals(
                    scenario.schema, "ntsd28-scenario/1.0", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Scenario schema mismatch.");
            }
            if (!string.Equals(
                    scenario.referenceExeSha256,
                    q07C050Bdy50Scenario || q07C051Effect23Scenario
                        ? "336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3"
                        : formalReleaseScenario ? FormalAuthorityExeSha256 : LegacyScenarioReferenceSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Scenario reference EXE identity mismatch.");
            }
            if (!IsSha256(scenario.dataSha256))
                throw new InvalidDataException("Scenario data SHA-256 is invalid.");
            if (!string.Equals(scenario.mode, "versus", StringComparison.Ordinal))
                throw new InvalidDataException("Only the neutral versus baseline is supported.");
            if (scenario.ticks != (q07LeeJlChildScenario ? 45 :
                    q07NarutoPunchScenario || q09HidanFrame430Scenario ? 30 :
                    q07HeldWeaponMotionScenario ? 24 : q07HidanCatchScenario ? 24 :
                    q07HidanNaturalCatchScenario ? 40 :
                    q07C050Bdy50Scenario ? 3 :
                    q07C051Effect23Scenario ? 12 :
                    q07N30LateInputScenario ? 20 :
                     q07HitFa5FullDriverScenario ? 8 :
                     q07FusionFullDriverScenario ? 3 :
                      q07RevivalPeerFullDriverScenario ? 3 :
                      r06NaturalRevivalScenario ? 70 :
                       q08NarutoCloneStageGateScenario ? 3 :
                       q08State12KoFullTickScenario ? 3 :
                      q08NegativeEnvironmentKoFullTickScenario ? 13 :
                      q08HeldCpointKoFullTickScenario ? 3 :
                    q08SelectedArmorResultMilestonesScenario ? 366 :
                    formalReleaseScenario ? 26 : 3) ||
                scenario.emitInitial ||
                scenario.battleMode != 0 || scenario.stageId != 23 ||
                scenario.difficultyLevel4A0C30 !=
                    (q07LeeJlChildScenario || q07NarutoPunchScenario ||
                     q07HeldWeaponMotionScenario ||
                      q07HidanCatchScenario || q07HidanNaturalCatchScenario ||
                      q07C050Bdy50Scenario ||
                      q07C051Effect23Scenario ||
                      q09HidanFrame430Scenario ||
                     q07N30LateInputScenario || q07HitFa5FullDriverScenario ||
                      q07FusionFullDriverScenario ||
                       q07RevivalPeerFullDriverScenario ||
                        q08State12KoFullTickScenario ||
                       q08NegativeEnvironmentKoFullTickScenario ||
                       q08HeldCpointKoFullTickScenario ? 0 : 1) ||
                ((q07LeeJlChildScenario || q07NarutoPunchScenario ||
                  q08NarutoCloneStageGateScenario) &&
                    scenario.seed != Q07LeeJlChildSeed) ||
                (formalReleaseScenario && !q07C050Bdy50Scenario &&
                    !q07C051Effect23Scenario &&
                    !q07LeeJlChildScenario &&
                    !q07NarutoPunchScenario && !q08NarutoCloneStageGateScenario &&
                    scenario.seed != Q07DdjSeed) ||
                ((q07C050Bdy50Scenario || q07C051Effect23Scenario) &&
                    scenario.seed != 682973786))
            {
                throw new InvalidDataException(
                    q07LeeJlChildScenario
                        ? "Q07 Lee J,L scenario must preserve the 45-tick formal fixture."
                        : q07NarutoPunchScenario
                        ? "Q07 Naruto punch scenario must preserve the 30-tick formal fixture."
                        : formalReleaseScenario
                        ? "Q07 formal scenario must preserve its fixed Stage 23 fixture."
                        : "Scenario must preserve the frozen 3-tick Stage 23 baseline.");
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
                    combatant.facing > 1 || combatant.action < 0 ||
                    combatant.action >= LF2FrameCache.MaxFrameIdExclusive ||
                    (!formalReleaseScenario && combatant.z < Stage23ZMin) ||
                    combatant.z > Stage23ZMax)
                {
                    throw new InvalidDataException(
                        $"Combatant slot {combatant.slot} is outside the frozen baseline contract.");
                }
            }

            if (q07DdjScenario)
            {
                UnityRawCombatant naruto = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(combatant => combatant.slot == 1);
                if (naruto.oid != 2 || naruto.team != 1 || naruto.x != 500 || naruto.y != 0 ||
                    naruto.z != 350 || naruto.hp != 500 || naruto.baseHp != 500 ||
                    naruto.mp != 500 || naruto.facing != 0 || naruto.action != 110 ||
                    opponent.oid != 7 || opponent.team != 2 || opponent.x != 1200 ||
                    opponent.y != 0 || opponent.z != 350 || opponent.hp != 500 ||
                    opponent.baseHp != 500 || opponent.mp != 500 || opponent.facing != 0 ||
                    opponent.action != 0 || naruto.nativeAi || opponent.nativeAi)
                    throw new InvalidDataException("Q07 DDJ participant state differs from the formal fixture.");
            }

            if (q07SasukeNeedleScenario)
            {
                UnityRawCombatant sasuke = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(combatant => combatant.slot == 1);
                if (sasuke.oid != 11 || sasuke.team != 1 || sasuke.x != 500 || sasuke.y != 0 ||
                    sasuke.z != 350 || sasuke.hp != 500 || sasuke.baseHp != 500 ||
                    sasuke.mp != 500 || sasuke.facing != 0 || sasuke.action != 110 ||
                    opponent.oid != 7 || opponent.team != 2 || opponent.x != 1200 ||
                    opponent.y != 0 || opponent.z != 350 || opponent.hp != 500 ||
                    opponent.baseHp != 500 || opponent.mp != 500 || opponent.facing != 0 ||
                    opponent.action != 0 || sasuke.nativeAi || opponent.nativeAi)
                    throw new InvalidDataException("Q07 Sasuke participant state differs from the formal fixture.");
            }

            if (q07SasukeNeedleTargetHitScenario)
            {
                UnityRawCombatant sasuke = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(combatant => combatant.slot == 1);
                if (sasuke.oid != 11 || sasuke.team != 1 || sasuke.x != 500 || sasuke.y != 0 ||
                    sasuke.z != 350 || sasuke.hp != 500 || sasuke.baseHp != 500 ||
                    sasuke.mp != 500 || sasuke.facing != 0 || sasuke.action != 110 ||
                    opponent.oid != 7 || opponent.team != 2 ||
                    (opponent.x != 550 && opponent.x != 1200) || opponent.y != 0 ||
                    opponent.z != 350 || opponent.hp != 500 || opponent.baseHp != 500 ||
                    opponent.mp != 500 || opponent.facing != 0 ||
                    opponent.action != 0 || sasuke.nativeAi || opponent.nativeAi)
                    throw new InvalidDataException("Q07 Sasuke target-hit participants differ from the formal fixture.");
            }

            if (q07SasukeArmorTargetScenario)
            {
                UnityRawCombatant sasuke = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(combatant => combatant.slot == 1);
                if (sasuke.oid != 11 || sasuke.team != 1 || sasuke.x != 500 || sasuke.y != 0 ||
                    sasuke.z != 350 || sasuke.hp != 500 || sasuke.baseHp != 500 ||
                    sasuke.mp != 500 || sasuke.facing != 0 || sasuke.action != 110 ||
                    opponent.oid != 87 || opponent.team != 2 ||
                    (opponent.x != 550 && opponent.x != 1200) || opponent.y != 0 ||
                    opponent.z != 350 || opponent.hp != 500 || opponent.baseHp != 500 ||
                    opponent.mp != 300 || opponent.facing != 0 ||
                    opponent.action != 0 || sasuke.nativeAi || opponent.nativeAi)
                    throw new InvalidDataException("Q07 Sasuke armor-target participants differ from the formal fixture.");
            }

            if (q08SelectedArmorFixture)
            {
                UnityRawCombatant sasuke = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(combatant => combatant.slot == 1);
                if (sasuke.oid != 11 || sasuke.team != 1 || sasuke.x != 500 || sasuke.y != 0 ||
                    sasuke.z != 350 || sasuke.hp != 500 || sasuke.baseHp != 500 ||
                    sasuke.mp != 500 || sasuke.facing != 0 || sasuke.action != 110 ||
                    opponent.oid != 87 || opponent.team != 2 || opponent.x != 550 ||
                    opponent.y != 0 || opponent.z != 350 || opponent.hp != 3 ||
                    opponent.baseHp != 3 || opponent.mp != 300 || opponent.facing != 0 ||
                    opponent.action != 0 || sasuke.nativeAi || opponent.nativeAi ||
                    sasuke.reviveLives30c != 1 || opponent.reviveLives30c != 1 ||
                    sasuke.reviveNextLives310 != 0 || opponent.reviveNextLives310 != 0 ||
                    sasuke.reviveNextHp314 != 0 || opponent.reviveNextHp314 != 0)
                    throw new InvalidDataException("Q08 selected-armor lethal participants differ from the formal fixture.");
            }

            if (r06NaturalRevivalScenario)
            {
                UnityRawCombatant sasuke = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant target = scenario.combatants.Single(combatant => combatant.slot == 1);
                if (sasuke.oid != 11 || sasuke.team != 1 || sasuke.x != 500 ||
                    sasuke.y != 0 || sasuke.z != 350 || sasuke.hp != 500 ||
                    sasuke.baseHp != 500 || sasuke.mp != 500 || sasuke.facing != 0 ||
                    sasuke.action != 110 || sasuke.reviveLives30c != 1 ||
                    target.oid != 87 || target.team != 2 || target.x != 550 ||
                    target.y != 0 || target.z != 350 || target.hp != 3 ||
                    target.baseHp != 3 || target.mp != 300 || target.facing != 0 ||
                    target.action != 0 || target.reviveLives30c != 2 ||
                    sasuke.reviveNextLives310 != 0 || target.reviveNextLives310 != 0 ||
                    sasuke.reviveNextHp314 != 0 || target.reviveNextHp314 != 0 ||
                    sasuke.nativeAi || target.nativeAi)
                    throw new InvalidDataException(
                        "R06 natural revival participants differ from the paired formal source fixture.");
            }

            if (q07HidanCatchScenario)
            {
                UnityRawCombatant actor = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant target = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool HasHidanState(UnityRawCombatant combatant, int team, int x, int action) =>
                    combatant.oid == 24 && combatant.team == team &&
                    combatant.x == x && combatant.y == 0 && combatant.z == 350 &&
                    combatant.hp == 500 && combatant.baseHp == 500 &&
                    combatant.mp == 300 && combatant.facing == 0 &&
                    combatant.action == action && combatant.reviveLives30c == 1 &&
                    combatant.reviveNextLives310 == 0 &&
                    combatant.reviveNextHp314 == 0 && combatant.renderPhase008 == 0 &&
                    !combatant.nativeAi && combatant.nativeComputerState1b8 == 0;
                if (!HasHidanState(actor, 1, 500, 236) ||
                    (!HasHidanState(target, 2, 520, 0) &&
                     !HasHidanState(target, 2, 1200, 0)) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                {
                    throw new InvalidDataException(
                        "Q07 Hidan catch participants differ from the controlled formal fixture.");
                }
            }
            if (q07C050Bdy50Scenario)
            {
                UnityRawCombatant actor = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant target = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool HasState(UnityRawCombatant combatant, int oid, int team, int x, int action) =>
                    combatant.oid == oid && combatant.team == team &&
                    combatant.x == x && combatant.y == 0 && combatant.z == 400 &&
                    combatant.hp == 500 && combatant.baseHp == 500 &&
                    combatant.mp == 500 && combatant.facing == 0 &&
                    combatant.action == action && combatant.reviveLives30c == 1 &&
                    !combatant.nativeAi;
                if (!HasState(actor, 24, 1, 500, 37) ||
                    (!HasState(target, 56, 2, 520, 259) &&
                     !HasState(target, 56, 2, 1200, 259)) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                {
                    throw new InvalidDataException(
                        "C050 Hidan BDY50 participants differ from the 336B44 fixture.");
                }
            }

            if (q07C051Effect23Scenario)
            {
                UnityRawCombatant actor = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant target = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool HasState(UnityRawCombatant combatant, int oid, int team, int x,
                    int action, int facing) =>
                    combatant.oid == oid && combatant.team == team &&
                    combatant.x == x && combatant.y == 0 && combatant.z == 400 &&
                    combatant.hp == 500 && combatant.baseHp == 500 &&
                    combatant.mp == 500 && combatant.facing == facing &&
                    combatant.action == action && combatant.reviveLives30c == 1 &&
                    !combatant.nativeAi;
                bool right = HasState(actor, 20, 1, 500, 288, 0) &&
                    HasState(target, 2, 2, 550, 0, 0);
                bool left = HasState(actor, 20, 1, 500, 288, 1) &&
                    HasState(target, 2, 2, 350, 0, 1);
                bool armor = HasState(actor, 78, 1, 500, 466, 0) &&
                    HasState(target, 97, 2, 550, 0, 1);
                bool armorControl = HasState(actor, 78, 1, 500, 466, 0) &&
                    HasState(target, 2, 2, 550, 0, 1);
                if ((!right && !left && !armor && !armorControl) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                {
                    throw new InvalidDataException(
                        "C051 effect23 participants differ from the 336B44 fixture.");
                }
            }

            if (q07HidanNaturalCatchScenario || q09HidanFrame430Scenario)
            {
                UnityRawCombatant actor = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant target = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool HasHidanNaturalState(UnityRawCombatant combatant, int team, int x) =>
                    combatant.oid == 24 && combatant.team == team &&
                    combatant.x == x && combatant.y == 0 && combatant.z == 350 &&
                    combatant.hp == 500 && combatant.baseHp == 500 &&
                    combatant.mp == 300 && combatant.facing == 0 &&
                    combatant.action == 0 && combatant.reviveLives30c == 1 &&
                    combatant.reviveNextLives310 == 0 &&
                    combatant.reviveNextHp314 == 0 && combatant.renderPhase008 == 0 &&
                    !combatant.nativeAi && combatant.nativeComputerState1b8 == 0;
                if (!HasHidanNaturalState(actor, 1, 500) ||
                    (!HasHidanNaturalState(target, 2, 580) &&
                     !HasHidanNaturalState(target, 2, 1200)) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C)
                {
                    throw new InvalidDataException(
                        "Q07 Hidan natural catch participants differ from the formal fixture.");
                }
            }

            if (q07N30LateInputScenario)
            {
                UnityRawCombatant actor = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant target = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool HasN30State(UnityRawCombatant combatant, int team, int x) =>
                    combatant.oid == 2 && combatant.team == team &&
                    combatant.x == x && combatant.y == 0 && combatant.z == 350 &&
                    combatant.hp == 500 && combatant.baseHp == 500 &&
                    combatant.mp == 300 && combatant.facing == 0 &&
                    combatant.action == 0 && combatant.reviveLives30c == 1 &&
                    combatant.reviveNextLives310 == 0 &&
                    combatant.reviveNextHp314 == 0 &&
                    combatant.renderPhase008 == 0 && !combatant.nativeAi &&
                    combatant.nativeComputerState1b8 == 0;
                if (!HasN30State(actor, 1, 500) ||
                    !HasN30State(target, 2, 1200) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C)
                {
                    throw new InvalidDataException(
                        "Q07 N30 late-input participants differ from the formal fixture.");
                }
            }
            if (q07HitFa5FullDriverScenario)
            {
                UnityRawCombatant ally = scenario.combatants.Single(value => value.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(value => value.slot == 1);
                bool HasState(UnityRawCombatant value, int team, int x) =>
                    value.oid == 2 && value.team == team && value.x == x &&
                    value.y == 0 && value.z == 350 && value.hp == 500 &&
                    value.baseHp == 500 && value.mp == 300 &&
                    value.facing == 0 && value.action == 0 &&
                    !value.nativeAi && value.nativeComputerState1b8 == 0;
                if (!HasState(ally, 1, 251) || !HasState(opponent, 2, 1200) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                    throw new InvalidDataException(
                        "Q07 hit_Fa5 participants differ from the formal source fixture.");
            }

            if (q07FusionFullDriverScenario)
            {
                UnityRawCombatant primary = scenario.combatants.Single(value => value.slot == 0);
                UnityRawCombatant partner = scenario.combatants.Single(value => value.slot == 1);
                bool HasState(UnityRawCombatant value, int oid, int x) =>
                    value.oid == oid && value.team == 1 && value.x == x &&
                    value.y == 0 && value.z == 600 && value.hp == 100 &&
                    value.baseHp == 500 && value.mp == 500 &&
                    value.facing == (oid == 8 || oid == 11 ? 1 : 0) && value.action == 9 &&
                    value.reviveLives30c == 1 &&
                    value.reviveNextLives310 == 0 &&
                    value.reviveNextHp314 == 0 && value.renderPhase008 == 0 &&
                    !value.nativeAi && value.nativeComputerState1b8 == 0;
                bool row0 = (HasState(primary, 7, 304) || HasState(primary, 7, 315)) &&
                    HasState(partner, 8, 300);
                bool row1 = HasState(primary, 10, 304) && HasState(partner, 11, 300);
                if ((!row0 && !row1) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                    throw new InvalidDataException(
                        "Q07 fusion participants differ from the declared formal fixture.");
            }

            if (q07RevivalPeerFullDriverScenario)
            {
                UnityRawCombatant dead = scenario.combatants.Single(value => value.slot == 0);
                UnityRawCombatant peer = scenario.combatants.Single(value => value.slot == 1);
                bool HasCommonState(UnityRawCombatant value) =>
                    value.oid == 7 && value.team == 1 && value.y == 0 &&
                    value.z == 600 && value.baseHp == 500 && value.facing == 0 &&
                    value.reviveNextLives310 == 0 && value.reviveNextHp314 == 0 &&
                    !value.nativeAi && value.nativeComputerState1b8 == 0;
                if (!HasCommonState(dead) || dead.x != 50 || dead.hp != 1 ||
                    dead.mp != 77 || dead.action != 230 ||
                    dead.reviveLives30c != 2 || dead.renderPhase008 != 2 ||
                    !HasCommonState(peer) || (peer.x != 100 && peer.x != 0) ||
                    peer.hp != 500 || peer.mp != 500 || peer.action != 0 ||
                    peer.reviveLives30c != 1 || peer.renderPhase008 != 0 ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                    throw new InvalidDataException(
                        "Q07 revival peer participants differ from the formal fixture.");
            }

            if (q07LeeJlChildScenario)
            {
                UnityRawCombatant lee = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool LeeState(UnityRawCombatant c, int oid, int team, int x) =>
                    c.oid == oid && c.team == team && c.x == x && c.y == 0 &&
                    c.z == 650 && c.hp == 500 && c.baseHp == 500 && c.mp == 500 &&
                    c.facing == 0 && c.action == 0 && c.reviveLives30c == 1 &&
                    !c.nativeAi && c.nativeComputerState1b8 == 0;
                if (!LeeState(lee, 7, 1, 500) ||
                    !LeeState(opponent, 2, 2, 1200) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C)
                    throw new InvalidDataException("Q07 Lee participants differ from the formal fixture.");
            }

            if (q07NarutoPunchScenario)
            {
                UnityRawCombatant attacker = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant target = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool HasState(UnityRawCombatant c, int team, int x, int hp) =>
                    c.oid == 2 && c.team == team && c.x == x && c.y == 0 &&
                    c.z == 650 && c.hp == hp && c.baseHp == hp && c.mp == 500 &&
                    c.facing == 0 && c.action == 0 && c.reviveLives30c == 1 &&
                    c.reviveNextLives310 == 0 && c.reviveNextHp314 == 0 &&
                    c.renderPhase008 == 0 && !c.nativeAi &&
                    c.nativeComputerState1b8 == 0;
                if (!HasState(attacker, 1, 500, 500) ||
                    !HasState(target, 2, 525, 10) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C)
                    throw new InvalidDataException("Q07 Naruto punch participants differ from the formal fixture.");
            }
            if (q08NarutoCloneStageGateScenario)
            {
                UnityRawCombatant actor = scenario.combatants.Single(value => value.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(value => value.slot == 1);
                bool HasState(UnityRawCombatant value, int oid, int team,
                    int x, int facing, int action) =>
                    value.oid == oid && value.team == team && value.x == x &&
                    value.y == 0 && value.z == 650 && value.hp == 500 &&
                    value.baseHp == 500 && value.mp == 500 &&
                    value.facing == facing && value.action == action &&
                    value.reviveLives30c == 1 && value.reviveNextLives310 == 0 &&
                    value.reviveNextHp314 == 0 && value.renderPhase008 == 0 &&
                    !value.nativeAi && value.nativeComputerState1b8 == 0;
                if (!HasState(actor, 2, 1, 0, 0, 121) ||
                    !HasState(opponent, 7, 2, 1200, 1, 0) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                    throw new InvalidDataException(
                        "Q08 Naruto clone stage-gate participants differ from the paired source fixture.");
            }

            if (q07HeldWeaponMotionScenario)
            {
                UnityRawCombatant actor = scenario.combatants.Single(combatant => combatant.slot == 0);
                UnityRawCombatant opponent = scenario.combatants.Single(combatant => combatant.slot == 1);
                bool HasState(UnityRawCombatant c, int oid, int team, int x) =>
                    c.oid == oid && c.team == team && c.x == x && c.y == 0 &&
                    c.z == 542 && c.hp == 500 && c.baseHp == 500 && c.mp == 500 &&
                    c.facing == 0 && c.action == 0 && c.reviveLives30c == 1 &&
                    c.reviveNextLives310 == 0 && c.reviveNextHp314 == 0 &&
                    c.renderPhase008 == 0 && !c.nativeAi &&
                    c.nativeComputerState1b8 == 0;
                if (!HasState(actor, 2, 1, 200) ||
                    !HasState(opponent, 7, 2, 1200) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C)
                    throw new InvalidDataException(
                        "Q07 held-weapon participants differ from the formal fixture.");
            }

            if (q08State12KoFullTickScenario)
            {
                UnityRawCombatant victim = scenario.combatants.Single(value => value.slot == 0);
                UnityRawCombatant credit = scenario.combatants.Single(value => value.slot == 1);
                bool HasState(UnityRawCombatant value, int team, int x, int hp, int action) =>
                    value.oid == 2 && value.team == team && value.x == x &&
                    value.y == 0 && value.z == 542 && value.hp == hp &&
                    value.baseHp == hp && value.mp == 300 &&
                    value.facing == 0 && value.action == action &&
                    value.reviveLives30c == 1 && !value.nativeAi &&
                    value.nativeComputerState1b8 == 0;
                if ((!HasState(victim, 1, 200, 5, 180) &&
                     !HasState(victim, 1, 200, 50, 180)) ||
                    !HasState(credit, 2, 1200, 500, 0) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                    throw new InvalidDataException(
                        "Q08 state12 KO participants differ from the formal full-tick fixture.");
            }

            if (q08NegativeEnvironmentKoFullTickScenario)
            {
                UnityRawCombatant victim = scenario.combatants.Single(value => value.slot == 0);
                UnityRawCombatant credit = scenario.combatants.Single(value => value.slot == 1);
                bool HasState(UnityRawCombatant value, int team, int x, int hp) =>
                    value.oid == 2 && value.team == team && value.x == x &&
                    value.y == 0 && value.z == 542 && value.hp == hp &&
                    value.baseHp == hp && value.mp == 300 &&
                    value.facing == 0 && value.action == 0 &&
                    value.reviveLives30c == 1 && !value.nativeAi &&
                    value.nativeComputerState1b8 == 0;
                if ((!HasState(victim, 1, 200, 5) &&
                     !HasState(victim, 1, 200, 50)) ||
                    !HasState(credit, 2, 1200, 500) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                    throw new InvalidDataException(
                        "Q08 negative-environment participants differ from the formal full-tick fixture.");
            }

            if (q08HeldCpointKoFullTickScenario)
            {
                UnityRawCombatant catcher = scenario.combatants.Single(value => value.slot == 0);
                UnityRawCombatant caught = scenario.combatants.Single(value => value.slot == 1);
                bool HasState(UnityRawCombatant value, int team, int x, int hp, int action) =>
                    value.oid == 2 && value.team == team && value.x == x &&
                    value.y == 0 && value.z == 542 && value.hp == hp &&
                    value.baseHp == hp && value.mp == 300 &&
                    value.facing == 0 && value.action == action &&
                    value.reviveLives30c == 1 && !value.nativeAi &&
                    value.nativeComputerState1b8 == 0;
                if (!HasState(catcher, 1, 200, 500, 260) ||
                    (!HasState(caught, 2, 220, 30, 132) &&
                     !HasState(caught, 2, 220, 100, 132)) ||
                    scenario.fusionFirstFeatureGate4A8428 ||
                    scenario.fusionSecondFeatureGate4A842C ||
                    (scenario.inputs != null && scenario.inputs.Length != 0))
                    throw new InvalidDataException(
                        "Q08 held-CPoint participants differ from the formal full-tick fixture.");
            }

            ValidateInputs(scenario, slots);
            if (q08SelectedArmorFixture || r06NaturalRevivalScenario)
            {
                UnityRawInput[] inputs = scenario.inputs ?? Array.Empty<UnityRawInput>();
                string[] expectedKeys = { "L", "L", "D", "D", "J", "J" };
                if (inputs.Length != expectedKeys.Length ||
                    Enumerable.Range(0, expectedKeys.Length).Any(index =>
                        inputs[index].tick != index + 1 || inputs[index].slot != 0 ||
                        inputs[index].keys == null || inputs[index].keys.Length != 1 ||
                        inputs[index].keys[0] != expectedKeys[index]))
                    throw new InvalidDataException("Q08 selected-armor lethal input differs from the formal six-key fixture.");
            }
            if (q07N30LateInputScenario)
            {
                UnityRawInput[] inputs = scenario.inputs ?? Array.Empty<UnityRawInput>();
                bool HasSingleKey(int tick, string key) => inputs.Any(input =>
                    input.tick == tick && input.slot == 0 &&
                    input.keys != null && input.keys.Length == 1 &&
                    input.keys[0] == key);
                bool formalPhysicalKeys = HasSingleKey(1, "L") &&
                    HasSingleKey(3, "K") && HasSingleKey(5, "L") &&
                    HasSingleKey(7, "K");
                bool unityHistoryKeys = HasSingleKey(1, "J") &&
                    HasSingleKey(3, "L") && HasSingleKey(5, "J") &&
                    HasSingleKey(7, "L");
                if (inputs.Length != 4 ||
                    (!formalPhysicalKeys && !unityHistoryKeys))
                {
                    throw new InvalidDataException(
                        "Q07 N30 input must be L-K-L-K or J-L-J-L at ticks 1,3,5,7.");
                }
            }
            if (q07HidanNaturalCatchScenario)
            {
                UnityRawInput[] inputs = scenario.inputs ?? Array.Empty<UnityRawInput>();
                bool HasSingleKey(int tick, string key) => inputs.Any(input =>
                    input.tick == tick && input.slot == 0 &&
                    input.keys != null && input.keys.Length == 1 &&
                    input.keys[0] == key);
                if (inputs.Length != 4 || !HasSingleKey(0, "J") ||
                    !HasSingleKey(1, "J") || !HasSingleKey(2, "K") ||
                    !HasSingleKey(3, "K"))
                {
                    throw new InvalidDataException(
                        "Q07 Hidan natural catch input differs from attack ticks1-2 then jump ticks3-4.");
                }
            }
            if (q09HidanFrame430Scenario)
            {
                UnityRawInput[] inputs = scenario.inputs ?? Array.Empty<UnityRawInput>();
                bool HasKeys(int tick, params string[] keys) => inputs.Any(input =>
                    input.tick == tick && input.slot == 0 &&
                    input.keys != null && input.keys.SequenceEqual(keys));
                if (inputs.Length != 6 || !HasKeys(0, "J") || !HasKeys(1, "J") ||
                    !HasKeys(8, "K") || !HasKeys(9, "K") ||
                    !HasKeys(12, "L", "D", "J") ||
                    !HasKeys(13, "L", "D", "J"))
                {
                    throw new InvalidDataException(
                        "Q09 Hidan frame430 input differs from the formal 30-tick fixture.");
                }
            }
            if (q07LeeJlChildScenario)
            {
                UnityRawInput[] inputs = scenario.inputs ?? Array.Empty<UnityRawInput>();
                bool HasSingleKey(int tick, string key) => inputs.Any(input =>
                    input.tick == tick && input.slot == 0 &&
                    input.keys != null && input.keys.Length == 1 &&
                    input.keys[0] == key);
                if (inputs.Length != 3 || !HasSingleKey(1, "J") ||
                    !HasSingleKey(2, "L") || !HasSingleKey(3, "L"))
                    throw new InvalidDataException("Q07 Lee input schedule differs from the formal J,L fixture.");
            }
            if (q07NarutoPunchScenario)
            {
                UnityRawInput[] inputs = scenario.inputs ?? Array.Empty<UnityRawInput>();
                if (inputs.Length != 1 || inputs[0].tick != 1 ||
                    inputs[0].slot != 0 || inputs[0].keys == null ||
                    inputs[0].keys.Length != 1 || inputs[0].keys[0] != "J")
                    throw new InvalidDataException("Q07 Naruto punch input differs from J on completed tick 2.");
            }
            if (q07HeldWeaponMotionScenario)
            {
                UnityRawInput[] inputs = scenario.inputs ?? Array.Empty<UnityRawInput>();
                bool HasSingleKey(int tick, string key) => inputs.Any(input =>
                    input.tick == tick && input.slot == 0 &&
                    input.keys != null && input.keys.Length == 1 &&
                    input.keys[0] == key);
                if (inputs.Length != 16 || !HasSingleKey(0, "J") ||
                    !HasSingleKey(1, "J") ||
                    !Enumerable.Range(7, 7).All(tick => HasSingleKey(tick, "D")) ||
                    !Enumerable.Range(14, 7).All(tick => HasSingleKey(tick, "S")))
                    throw new InvalidDataException(
                        "Q07 held-weapon input differs from attack 1-2, right 8-14, down 15-21.");
            }
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
            bool loganContent,
            LoganObjectCatalog catalog)
        {
            world.ResetRuntimeState();
            if (world.AiExecutionProfile !=
                BattleAiExecutionProfile.DataOrientedCanonical)
            {
                world.ConfigureAiExecutionProfile(
                    BattleAiExecutionProfile.DataOrientedCanonical);
            }
            world.Rng.Seed(unchecked((uint)scenario.seed));
            if (scenario.schema == Q07LeeJlChildScenarioSchema ||
                scenario.schema == Q07HidanNaturalCatchScenarioSchema ||
                scenario.schema == Q08NarutoCloneStageGateScenarioSchema ||
                scenario.schema == Q07NarutoPunchScenarioSchema ||
                scenario.schema == Q07HeldWeaponMotionScenarioSchema ||
                scenario.schema == Q07HitFa5FullDriverScenarioSchema ||
                scenario.schema == Q07FusionFullDriverScenarioSchema ||
                scenario.schema == Q07RevivalPeerFullDriverScenarioSchema ||
                scenario.schema == Q08State12KoFullTickScenarioSchema ||
                scenario.schema == Q08NegativeEnvironmentKoFullTickScenarioSchema ||
                scenario.schema == Q08HeldCpointKoFullTickScenarioSchema)
                world.NativeRandom.ResetFromSeed(unchecked((uint)scenario.seed));
            else
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
                int objectType = catalog?.Entries.FirstOrDefault(entry =>
                    entry.Id == source.oid)?.Type ?? (int)LF2ObjectType.Character;
                LF2Entity entity = objectType switch
                {
                    (int)LF2ObjectType.Character => CreateCharacter(
                        world, configs[source.oid], source, loganContent),
                    (int)LF2ObjectType.SpecialAttack => CreateSpecialAttack(
                        world, configs[source.oid], source),
                    _ => throw new InvalidDataException(
                        $"Scenario oid {source.oid} has unsupported catalog type {objectType}."),
                };
                if (scenario.schema == Q08NarutoCloneStageGateScenarioSchema)
                {
                    entity.Runtime.SetSourceRulePosition(source.x, source.z);
                    entity.Runtime.SyncSourceRuleIntegerPosition();
                }
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
                rosterSlot.RuntimeSlotIndex = entity.Runtime.SlotIndex;
                rosterSlot.StableId = entity.Runtime.StableId;
                runtime.Roster.ActiveSlotCount++;
            }
            world.ConfigureFusionFeatureGates(scenario.fusionFirstFeatureGate4A8428,
                scenario.fusionSecondFeatureGate4A842C);
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
            if (source.action != 0)
                ApplyInitialAction(character, source);
            character.RefreshRuntimeSnapshot();
            return character;
        }

        private static void ApplyInitialAction(LF2Entity entity,
            UnityRawCombatant source)
        {
            if (!entity.FrameCache.HasFrame(source.action))
                throw new InvalidDataException(
                    $"Scenario oid {source.oid} has no authored action {source.action}.");

            entity.ImmediateFrame(source.action);
            entity.Frame.PN = source.action;
            entity.Frame.Prev = source.action;
            entity.Frame.Prev2 = source.action;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = source.action;
            entity.Trans.SyncWaitCounterFrame(source.action);
        }

        private static LF2SpecialAttack CreateSpecialAttack(
            SimulationWorld world,
            LF2CharacterDataWrapper wrapper,
            UnityRawCombatant source)
        {
            var entity = new LF2SpecialAttack();
            entity.ObjectId = source.oid;
            entity.FrameCache.Load(wrapper);
            ApplyInitialAction(entity, source);
            entity.InitializeNativeDefinitionIdentityForSpawn();
            entity.InitializeNativeArmorRuntimeFromCurrentDefinitionForSpawn();
            entity.Runtime.HP = source.hp;
            entity.Runtime.HPBound = source.hp;
            entity.Runtime.HP3 = source.baseHp;
            entity.Runtime.MP = source.mp;
            entity.Runtime.MPMax = wrapper.characterData.NativeMetadata.Stats
                .Int32OrDefault("max_mp", source.mp);
            entity.Runtime.PP = source.mp;
            entity.Team = source.team;
            entity.RelationTeam = source.team;
            entity.OwnerEntityIndex = source.slot;
            entity.HP2Orig = source.reviveLives30c;
            entity.HPOrig = source.reviveNextLives310;
            entity.RespawnCount = source.reviveNextHp314;
            entity.HitStun = source.renderPhase008;
            entity.AiControlled = source.nativeAi || source.nativeComputerState1b8 > 0;
            entity.SwitchDir(source.facing == 1 ? "left" : "right");
            WritePosition(entity.Runtime, source.x, source.y, source.z);
            entity.Runtime.Vx = 0.0;
            entity.Runtime.Vy = 0.0;
            entity.Runtime.Vz = 0.0;
            entity.SetRequiredRuntimeSlot(source.slot);
            world.Register(entity);
            if (!ReferenceEquals(entity.RegisteredWorldForSimulation, world) ||
                entity.Runtime.SlotIndex != source.slot)
                throw new InvalidOperationException(
                    $"Scenario oid {source.oid} failed to register at physical slot {source.slot}.");
            entity.RefreshRuntimeSnapshot();
            return entity;
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
                ("formalAuthorityExeSha256", scenario.schema == Q07C050Bdy50ScenarioSchema ||
                    scenario.schema == Q07C051Effect23ScenarioSchema
                    ? scenario.referenceExeSha256.ToUpperInvariant()
                    : FormalAuthorityExeSha256),
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

        private static string BuildKnockoutTickJson(
            SimulationWorld world,
            int completedTick)
        {
            object[] events = world.NativeKnockoutEvents
                .Select(value => (object)DictionaryOf(
                    ("battleTimeTick", value.BattleTimeTick),
                    ("sourceObjectType", value.SourceObjectType),
                    ("fourOwnerSlot", value.FourOwnerSlot),
                    ("victimSlot", value.VictimSlot),
                    ("sourceSlot", value.SourceSlot),
                    ("creditSlot", value.CreditSlot)))
                .ToArray();
            return BattleCanonicalJson.Serialize(DictionaryOf(
                ("kind", "tick"),
                ("schema", Q07KnockoutEventCaptureSchema),
                ("completedTick", completedTick),
                ("eventCount", events.Length),
                ("events", (object)events)));
        }

        private static string BuildResultFlowTickJson(
            SimulationWorld world,
            int completedTick)
        {
            BattleResultsRuntimeState results = world.Runtime?.Results ??
                throw new InvalidOperationException("Q08 result-flow capture has no battle results state.");
            return BattleCanonicalJson.Serialize(DictionaryOf(
                ("kind", "tick"),
                ("schema", Q08ResultFlowCaptureSchema),
                ("completedTick", completedTick),
                ("nativeResultTimer", results.NativeResultTimer),
                ("nativeResultOutputTimer", results.NativeResultOutputTimer),
                ("nativeResultPhase", results.NativeResultPhase),
                ("nativeTransitionState", results.NativeTransitionState),
                ("nativeLivingGroupMask", results.NativeLivingGroupMask)));
        }

        private static string BuildDomainHeaderJson(
            string scenarioPath,
            UnityRawScenario scenario,
            SimulationWorld world,
            IReadOnlyList<DomainOccupant> initialOccupants,
            string domainVersion)
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
                ("schema", domainVersion == "v2"
                    ? "ntsd28-logan-b0-domain-raw-v2" : DomainCaptureSchema),
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
            IReadOnlyList<DomainOccupant> currentOccupants,
            string domainVersion)
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
                        currentOccupants,
                        domainVersion)),
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

        internal static object[] DeriveLifecycleEvents(
            IReadOnlyList<DomainOccupant> previous,
            IReadOnlyList<DomainOccupant> current,
            string domainVersion)
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
                        if (domainVersion != "v2" || oldValue.AllocationEpoch == 0)
                        {
                            throw new InvalidOperationException(
                                "Domain occupant changed without allocation epoch.");
                        }
                        result.Add(ProjectLifecycleEvent(
                            "object-id-change", slot, oldValue.AllocationEpoch,
                            newValue.AllocationEpoch, oldValue.ObjectId, newValue.ObjectId));
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

        private sealed class TemporaryObjectPoolScope : IDisposable
        {
            private static readonly FieldInfo InstanceField =
                typeof(MoreMountains.Tools.MMSingleton<LF2ObjectPool>).GetField(
                    "_instance", BindingFlags.Static | BindingFlags.NonPublic);

            private readonly object previousInstance;
            private readonly GameObject host;

            public TemporaryObjectPoolScope()
            {
                previousInstance = InstanceField.GetValue(null);
                InstanceField.SetValue(null, null);
                try
                {
                    host = new GameObject("__NTSD28_UnityRawCapturePool")
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    LF2ObjectPool pool = host.AddComponent<LF2ObjectPool>();
                    if (!pool.IsRuntimeStateValidForAcceptance)
                        typeof(LF2ObjectPool).GetMethod("Awake",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(pool, null);
                    if (!pool.IsRuntimeStateValidForAcceptance)
                        throw new InvalidOperationException(
                            "The replay fixture pool did not initialize its runtime state.");
                    InstanceField.SetValue(null, pool);
                }
                catch
                {
                    if (host != null)
                        UnityEngine.Object.DestroyImmediate(host);
                    InstanceField.SetValue(null, previousInstance);
                    throw;
                }
            }

            public void Dispose()
            {
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
                InstanceField.SetValue(null, previousInstance);
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
            public BattleRuntimeShutdownReport LastShutdownReport { get; private set; }

            public void Dispose()
            {
                if (Driver != null)
                {
                    BattleRuntimeShutdownReport report =
                        Driver.ShutdownBattleRuntime();
                    LastShutdownReport = report;
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
            private readonly FieldInfo publishedCandidateField;
            private readonly object originalCachedConfig;
            private readonly object originalObjectLookup;
            private readonly object originalFrameConfig;
            private readonly object originalPublishedCandidate;
            private readonly List<DictionaryEntry> originalObjectsByType;
            private readonly List<DictionaryEntry> originalBackgroundLookup;

            private readonly LoganObjectCatalog loganCatalog;
            private readonly LoganVisualContentCandidate diagnosticCandidate;
            private readonly NTSD.App.ProjectBattleModeConfig.Snapshot projectModeSnapshot;
            private readonly FieldInfo registryField;
            private readonly object originalRegistry;
            private readonly string originalPublishedKey;
            private readonly LoganContentIdentity originalPublishedIdentity;

            public UnityCurrentDatScope(IReadOnlyCollection<int> requestedObjectIds,
                string loganRuntimeRoot, bool useProjectMode = false)
            {
                // Prepare source values before any singleton publication is changed.
                if (!string.IsNullOrWhiteSpace(loganRuntimeRoot))
                {
                    BattleContentSource source = BattleContentSource.ForLoganRuntime(loganRuntimeRoot);
                    projectModeSnapshot = useProjectMode
                        ? NTSD.App.ProjectBattleModeConfig.LoadDefault().Capture()
                        : null;
                    LoganObjectCatalog selected = LoganObjectCatalog.Read(source, projectModeSnapshot);
                    if (useProjectMode || selected.Entries.Any(entry =>
                        requestedObjectIds.Contains(entry.Id) &&
                        entry.Type == (int)LF2ObjectType.SpecialAttack))
                        diagnosticCandidate = LoganVisualContentCandidate.Capture(
                            source, projectModeSnapshot);
                    loganCatalog = diagnosticCandidate?.Catalog ?? selected;
                    Configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(loganCatalog);
                    Content = NTSD28TraceContentIdentity.FromLoganCatalog(loganCatalog);
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
                publishedCandidateField = RequireField(
                    typeof(CharacterAnimtorManager), "publishedLoganCandidate", flags);

                originalCachedConfig = cachedConfigField.GetValue(dataManager);
                originalObjectLookup = objectLookupField.GetValue(dataManager);
                originalFrameConfig = frameConfigField.GetValue(animationManager);
                originalPublishedCandidate = publishedCandidateField.GetValue(animationManager);
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
                    if (diagnosticCandidate != null)
                        publishedCandidateField.SetValue(animationManager, diagnosticCandidate);
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
                loganCatalog?.FusionInput.AssertInputsCurrent();
                Dictionary<string, object> current = loganCatalog == null
                    ? NTSD28TraceContentIdentity.CaptureLegacy(ProjectPath(ProductionConfigRoot), ProjectPath(ProductionDataIndex))
                    : NTSD28TraceContentIdentity.FromLoganCatalog(
                        LoganObjectCatalog.Read(loganCatalog.Source, projectModeSnapshot));
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
                        publishedCandidateField.SetValue(animationManager, originalPublishedCandidate);
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
            public string domainVersion;
            public string inputRngOutputPath;
            public string knockoutOutputPath;
            public string resultFlowOutputPath;
            public string terminalFreezeOutputPath;
            public string loganRuntimeRoot;
            public string resultPath;
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
            public bool hasSelectedStageGateOverride;
            public int selectedStageGateOverride;
            public bool fusionFirstFeatureGate4A8428;
            public bool fusionSecondFeatureGate4A842C;
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
            public int action;
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

        internal sealed class DomainOccupant
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
