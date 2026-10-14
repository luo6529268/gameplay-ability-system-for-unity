using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;

internal static class Program
{
    private const string TaskId = "NTSD-M12-MATCH-IMAGE-RESIDENCY-20261013";
    private const string FullTaskId = "NTSD-M12-FULL-IMAGE-COVERAGE-20261014";
    private const string FullCatalogWorkerRelativePath =
        "artifacts/diagnostics/M12-full-image-coverage-20261014/selector-worker";
    private const string LoganRuntimeRelativePath = "Assets/NTSD/Content/LoganRuntime";
    private const string SelectorWorkerRootName = "selector-worker";
    private const string FixedSelectionRelativePath =
        "artifacts/diagnostics/M12-astc-eight-role-20261013/eight-role-audit-worker/result-01/selection.json";
    private const string CaptureRelativePath =
        "artifacts/diagnostics/M12-astc-eight-role-20261013/eight-role-audit-worker/capture/role-resource-capture.json";
    private const string OldResultRelativePath =
        "artifacts/diagnostics/M12-astc-eight-role-20261013/result-10-final/result.json";

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null
    };

    private sealed class CatalogEntry
    {
        public int RegistryIndex;
        public int Id;
        public int Type;
        public string DatPath;
        public string SourcePath;
        public string PublishedFolder;
        public string Name;
        public int Hidden;
        public int CatalogLine;
    }

    private sealed class SourceDeclaration
    {
        public string SourcePath;
        public string ImagePath;
        public int DeclaredStart;
        public int DeclaredEnd;
        public SpriteFileInfo RuntimeFile;
        public long ImageBytes;
        public string ImageSha256;
        public bool ImageExists;
    }

    private sealed class ParsedObject
    {
        public int Id;
        public int Type;
        public LF2CharacterDataWrapper Wrapper;
        public string HeadPath;
        public string SmallPath;
        public List<SourceDeclaration> Files = new List<SourceDeclaration>();
        public int RegistryIndex;
        public string CatalogName;
        public string SourcePath;
        public string PublishedFolder;
        public int CatalogLine;
        public string DatPath;
        public long DatBytes;
        public string DatSha256;
        public bool PublishedConfig;
        public bool PublishedFolderExists;
        public int NativeHidden;
    }

    private sealed class ActualRun
    {
        public string RepositoryRoot;
        public string RuntimeRoot;
        public string DecodedDatRoot;
        public string CapturePath;
        public string SelectionPath;
        public string OldResultPath;
        public string FusionPath;
        public string KindPath;
        public string CaptureSha256;
        public string SelectionSha256;
        public string OldResultSha256;
        public string FusionSha256;
        public string KindSha256;
        public int[] SelectedRoots;
        public Dictionary<int, LF2CharacterDataWrapper> Configs =
            new Dictionary<int, LF2CharacterDataWrapper>();
        public Dictionary<int, int> Types = new Dictionary<int, int>();
        public Dictionary<int, ParsedObject> Objects = new Dictionary<int, ParsedObject>();
        public LoganFusionCatalog Fusion;
        public LoganKindCatalog Kind;
        public List<object> ParseFailures = new List<object>();
        public string CatalogPath;
        public string CatalogSha256;
        public int CatalogObjectRows;
        public int CatalogSelectableCount;
        public List<CatalogEntry> CatalogEntries = new List<CatalogEntry>();
        public int[] SelectableRoots = Array.Empty<int>();
        public int[] AllCatalogRoots = Array.Empty<int>();
        public int[] StageAllWaveRoots = Array.Empty<int>();
        public string StageRootsSource;
        public string StageInputScope;
        public string StageConfigValue;
        public string StageResolvedPath;
        public bool StageSelectedFileExists;
        public string StageParseStatus;
        public bool StageUnverified;
        public Dictionary<string, string> StageSourceSha256 = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private sealed class NamedPlanCase
    {
        public string Name;
        public int[] Roots;
        public int[] AdditionalRoots;
    }

    private static int Main(string[] args)
    {
        string repositoryRoot = null;
        string requestedOutputRoot = null;
        string outputRoot = null;

        try
        {
            ParseArguments(
                args,
                out repositoryRoot,
                out requestedOutputRoot,
                out string budgetInput,
                out bool fullCatalog,
                out string plansInput);
            repositoryRoot = Path.GetFullPath(repositoryRoot ?? FindRepositoryRoot());
            if (fullCatalog)
                return RunFullCatalog(repositoryRoot, requestedOutputRoot, plansInput);

            string legacySelectorWorkerRoot = Path.Combine(
                repositoryRoot,
                "artifacts",
                "diagnostics",
                "M12-match-image-residency-20261013",
                SelectorWorkerRootName);
            string selectorWorkerRoot = legacySelectorWorkerRoot;
            string budgetTaskId = null;
            string fullCatalogSelectorWorkerRoot = Path.Combine(repositoryRoot, FullCatalogWorkerRelativePath);
            if (budgetInput != null && requestedOutputRoot != null &&
                IsPathUnderRoot(requestedOutputRoot, fullCatalogSelectorWorkerRoot))
            {
                selectorWorkerRoot = fullCatalogSelectorWorkerRoot;
                budgetTaskId = FullTaskId;
            }
            outputRoot = CreateFreshOutputRoot(
                requestedOutputRoot == null
                    ? Path.Combine(selectorWorkerRoot, "results")
                    : requestedOutputRoot,
                selectorWorkerRoot);

            if (budgetInput != null) return VerifyBudgets(budgetInput, outputRoot, budgetTaskId);

            var synthetic = RunSyntheticTests();
            WriteJson(outputRoot, "synthetic-tests.json", synthetic);

            ActualRun actual = LoadActualRun(repositoryRoot);
            WriteJson(outputRoot, "inputs.json", BuildInputProjection(actual));

            BattleImageLoadPlan roleOnly = BattleVisualDependencySelector.Build(
                actual.Configs,
                actual.Types,
                actual.SelectedRoots,
                actual.Fusion,
                actual.Kind,
                includeScenario: false,
                includeCompatibility: false);
            BattleImageLoadPlan match = BattleVisualDependencySelector.Build(
                actual.Configs,
                actual.Types,
                actual.SelectedRoots,
                actual.Fusion,
                actual.Kind,
                includeScenario: true,
                includeCompatibility: true);

            WriteJson(
                outputRoot,
                "role-only.json",
                ProjectPlan("role-only", roleOnly, actual));
            WriteJson(
                outputRoot,
                "match.json",
                ProjectPlan("match", match, actual));

            var individualPlans = new List<object>();
            foreach (int root in actual.SelectedRoots)
            {
                BattleImageLoadPlan individual = BattleVisualDependencySelector.Build(
                    actual.Configs,
                    actual.Types,
                    new[] { root },
                    actual.Fusion,
                    actual.Kind,
                    includeScenario: false,
                    includeCompatibility: false);
                individualPlans.Add(ProjectPlan("individual-" + root, individual, actual));
                WriteJson(
                    outputRoot,
                    "individual-" + root + ".json",
                    ProjectPlan("individual-" + root, individual, actual));
            }

            WriteJson(outputRoot, "individual-plans.json", individualPlans);
            WriteJson(
                outputRoot,
                "comparison.json",
                CompareWithOldResult(actual, roleOnly));

            int syntheticPassCount = CountSynthetic(synthetic, "PASS");
            var summary = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["taskId"] = TaskId,
                ["status"] = syntheticPassCount == synthetic.Count ? "PASS" : "FAIL",
                ["repositoryRoot"] = repositoryRoot,
                ["outputRoot"] = outputRoot,
                ["fixedSelectedRoots"] = actual.SelectedRoots,
                ["catalogObjectRows"] = actual.Objects.Count,
                ["parsedConfigs"] = actual.Configs.Count,
                ["parseFailures"] = actual.ParseFailures,
                ["roleOnly"] = PlanSummary(roleOnly, actual),
                ["match"] = PlanSummary(match, actual),
                ["syntheticPassCount"] = syntheticPassCount,
                ["syntheticCaseCount"] = synthetic.Count
            };
            WriteJson(outputRoot, "summary.json", summary);

            Console.WriteLine("status=PASS");
            Console.WriteLine("catalogObjects=" + actual.Objects.Count);
            Console.WriteLine("roleOnlyDefinitions=" + roleOnly.DefinitionIds.Count);
            Console.WriteLine("matchDefinitions=" + match.DefinitionIds.Count);
            Console.WriteLine("outputRoot=" + outputRoot);
            return syntheticPassCount == synthetic.Count ? 0 : 2;
        }
        catch (Exception error)
        {
            if (!string.IsNullOrWhiteSpace(outputRoot))
            {
                try
                {
                    WriteJson(
                        outputRoot,
                        "failure.json",
                        new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            ["taskId"] = TaskId,
                            ["status"] = "FAIL",
                            ["commandLine"] = Environment.CommandLine,
                            ["errorType"] = error.GetType().FullName,
                            ["error"] = error.Message,
                            ["stackTrace"] = error.ToString()
                        });
                }
                catch (Exception writeError)
                {
                    Console.Error.WriteLine("failure-artifact-error: " + writeError.Message);
                }
            }

            Console.Error.WriteLine("status=FAIL");
            Console.Error.WriteLine(error);
            if (!string.IsNullOrWhiteSpace(outputRoot))
                Console.Error.WriteLine("outputRoot=" + outputRoot);
            return 1;
        }
    }

    private static void TestMissingType()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper> { [1] = Wrapper(1) };
        bool refused = false;
        try { BattleVisualDependencySelector.Build(configs, new Dictionary<int, int>(), new[] { 1 },
            null, null, includeScenario: false, includeCompatibility: false); }
        catch (ArgumentException) { refused = true; }
        Expect(refused, "Incomplete type metadata must not silently omit type-specific dependencies.");
    }

    private static void TestStageRootFusion()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [30] = Wrapper(30), [31] = Wrapper(31), [32] = Wrapper(32, FrameWithPoints(1, Point(1, 33))),
            [33] = Wrapper(33)
        };
        var types = configs.Keys.ToDictionary(id => id, _ => 1);
        var plan = BattleVisualDependencySelector.Build(configs, types, new[] { 30 },
            Fusion(FusionRecord(30, 31, 32, 1)), EmptyKind(), new[] { 31 },
            includeScenario: false, includeCompatibility: false);
        Expect(plan.DefinitionIds.SequenceEqual(new[] { 30, 31, 32, 33 }), "Stage roots must feed fusion and recursive OPoint closure.");
    }

    private static int VerifyBudgets(string inputPath, string outputRoot, string taskId = null)
    {
        using var document = JsonDocument.Parse(File.OpenRead(inputPath));
        var results = new List<object>();
        foreach (var item in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            var budget = new BattleImageMemoryBudget(item.GetProperty("oldPublicationBytes").GetInt64(),
                long.MaxValue, long.MaxValue);
            foreach (var image in item.GetProperty("images").EnumerateArray())
            {
                int width = image.GetProperty("width").GetInt32(), height = image.GetProperty("height").GetInt32();
                long bytes = image.GetProperty("fileBytes").GetInt64();
                if (image.GetProperty("mode").GetString() == "astc") budget.AddCompressedImage(width, height, bytes);
                else budget.AddImage(new BMPLoader.VerifiedImageLayout("diagnostic", "", bytes, width, height, true),
                    image.GetProperty("retainPixels").GetBoolean(), image.GetProperty("readableTexture").GetBoolean());
            }
            foreach (var cell in item.GetProperty("clampedCells").EnumerateArray())
                budget.AddClampedCell(cell.GetProperty("width").GetInt32(), cell.GetProperty("height").GetInt32(),
                    cell.GetProperty("retainPixels").GetBoolean());
            budget.ReserveAtlas(item.GetProperty("atlasBytes").GetInt64());
            var actual = new Dictionary<string, long>
            {
                ["residentBytes"] = budget.ResidentBytes, ["loadingBytes"] = budget.LoadingBytes,
                ["sourceGpuBytes"] = budget.SourceTextureBytes, ["readableCpuBytes"] = budget.ReadableTextureBytes,
                ["retainedRgbaBytes"] = budget.RetainedPixelBytes, ["oldPublicationBytes"] = budget.OldPublicationBytes,
                ["atlasReservationBytes"] = budget.ReservedAtlasBytes
            };
            foreach (var expected in item.GetProperty("expected").EnumerateObject())
                Expect(actual[expected.Name] == expected.Value.GetInt64(), item.GetProperty("name").GetString() +
                    ": budget mismatch for " + expected.Name + " " + actual[expected.Name] + " != " + expected.Value.GetInt64());
            results.Add(new { name = item.GetProperty("name").GetString(), status = "PASS", actual });
        }
        WriteJson(outputRoot, "production-budget-verification.json", new
        {
            status = "PASS", inputPath = Path.GetFullPath(inputPath), inputSha256 = HashFile(inputPath),
            caseCount = results.Count, results,
            boundary = "Production budget arithmetic only; BMPLoader layout shape and PageSize are diagnostic stubs, no Unity upload or device allocation."
        });
        if (taskId != null)
        {
            WriteJson(outputRoot, "budget-input-identity.json", new
            {
                taskId,
                mode = "budget-verification",
                inputPath = Path.GetFullPath(inputPath),
                inputSha256 = HashFile(inputPath),
                outputScope = "New full-coverage selector-worker only; legacy fixed8 output restriction remains unchanged."
            });
        }
        Console.WriteLine("budgetCases=" + results.Count + "; status=PASS; outputRoot=" + outputRoot);
        return 0;
    }

    private static int RunFullCatalog(string repositoryRoot, string requestedOutputRoot, string plansInput)
    {
        string selectorWorkerRoot = Path.Combine(repositoryRoot, FullCatalogWorkerRelativePath);
        string outputRoot = CreateFreshOutputRoot(
            requestedOutputRoot == null
                ? Path.Combine(selectorWorkerRoot, "results-01")
                : requestedOutputRoot,
            selectorWorkerRoot);

        try
        {
            List<object> synthetic = RunSyntheticTests();
            WriteJson(outputRoot, "synthetic-tests.json", synthetic);

            ActualRun actual = LoadFullCatalogRun(repositoryRoot);
            WriteJson(outputRoot, "inputs.json", BuildFullInputProjection(actual));

            var roleObjects = new List<object>();
            foreach (CatalogEntry entry in actual.CatalogEntries
                .Where(value => actual.SelectableRoots.Contains(value.Id))
                .OrderBy(value => value.RegistryIndex))
            {
                roleObjects.Add(ProjectRole(entry, actual.Objects[entry.Id]));
            }
            WriteJson(outputRoot, "roles.json", roleObjects);

            int[] stageRoots = actual.StageAllWaveRoots ?? Array.Empty<int>();
            foreach (int root in actual.SelectableRoots.OrderBy(value => value))
            {
                BattleImageLoadPlan only = BattleVisualDependencySelector.Build(
                    actual.Configs,
                    actual.Types,
                    new[] { root },
                    actual.Fusion,
                    actual.Kind,
                    includeScenario: false,
                    includeCompatibility: false);
                WriteJson(
                    outputRoot,
                    "role-" + root + "-only.json",
                    ProjectPlan("role-" + root + "-only", only, actual, fullIdentity: true));

                BattleImageLoadPlan match = BattleVisualDependencySelector.Build(
                    actual.Configs,
                    actual.Types,
                    new[] { root },
                    actual.Fusion,
                    actual.Kind,
                    additionalRoots: stageRoots,
                    includeScenario: true,
                    includeCompatibility: true);
                WriteJson(
                    outputRoot,
                    "role-" + root + "-match.json",
                    ProjectPlan("role-" + root + "-match", match, actual, fullIdentity: true));
            }

            BattleImageLoadPlan allSelectable = BattleVisualDependencySelector.Build(
                actual.Configs,
                actual.Types,
                actual.SelectableRoots,
                actual.Fusion,
                actual.Kind,
                additionalRoots: stageRoots,
                includeScenario: true,
                includeCompatibility: true);
            WriteJson(
                outputRoot,
                "all-selectable-match.json",
                ProjectPlan("all-selectable-match", allSelectable, actual, fullIdentity: true));

            BattleImageLoadPlan fullCatalog = BattleVisualDependencySelector.Build(
                actual.Configs,
                actual.Types,
                actual.AllCatalogRoots,
                actual.Fusion,
                actual.Kind,
                includeScenario: true,
                includeCompatibility: true);
            WriteJson(
                outputRoot,
                "full-catalog.json",
                ProjectPlan("full-catalog", fullCatalog, actual, fullIdentity: true));

            var namedCases = new List<object>();
            if (plansInput != null)
            {
                foreach (NamedPlanCase namedCase in ReadNamedPlanCases(plansInput))
                {
                    int[] additionalRoots = UnionRoots(stageRoots, namedCase.AdditionalRoots);
                    BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
                        actual.Configs,
                        actual.Types,
                        namedCase.Roots,
                        actual.Fusion,
                        actual.Kind,
                        additionalRoots: additionalRoots,
                        includeScenario: true,
                        includeCompatibility: true);
                    WriteJson(
                        outputRoot,
                        "case-" + namedCase.Name + ".json",
                        ProjectPlan("case-" + namedCase.Name, plan, actual, fullIdentity: true));
                    namedCases.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["name"] = namedCase.Name,
                        ["rootIds"] = namedCase.Roots,
                        ["additionalRoots"] = additionalRoots,
                        ["summary"] = PlanSummary(plan, actual)
                    });
                }
            }

            int syntheticPassCount = CountSynthetic(synthetic, "PASS");
            bool coveragePass = syntheticPassCount == synthetic.Count &&
                actual.CatalogObjectRows == 330 &&
                actual.Objects.Count == 330 &&
                actual.SelectableRoots.Length == 50 &&
                !actual.StageUnverified;
            var summary = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["taskId"] = FullTaskId,
                ["status"] = coveragePass ? "PASS" : "FAIL",
                ["repositoryRoot"] = repositoryRoot,
                ["outputRoot"] = outputRoot,
                ["catalogPath"] = actual.CatalogPath,
                ["catalogObjectRows"] = actual.CatalogObjectRows,
                ["parsedConfigs"] = actual.Configs.Count,
                ["catalogSelectableCount"] = actual.CatalogSelectableCount,
                ["expectedSelectableCount"] = 50,
                ["selectableRoots"] = actual.SelectableRoots,
                ["allCatalogRootsCount"] = actual.AllCatalogRoots.Length,
                ["stageAllWaveRoots"] = actual.StageAllWaveRoots,
                ["stageRootsSource"] = actual.StageRootsSource,
                ["stageInputScope"] = actual.StageInputScope,
                ["stageConfigValue"] = actual.StageConfigValue,
                ["stageResolvedPath"] = actual.StageResolvedPath,
                ["stageSelectedFileExists"] = actual.StageSelectedFileExists,
                ["stageParseStatus"] = actual.StageParseStatus,
                ["stageUnverified"] = actual.StageUnverified,
                ["stageSourceSha256"] = actual.StageSourceSha256,
                ["parseFailures"] = actual.ParseFailures,
                ["allSelectableMatch"] = PlanSummary(allSelectable, actual),
                ["fullCatalogMatch"] = PlanSummary(fullCatalog, actual),
                ["namedCases"] = namedCases,
                ["pairwiseExports"] = false,
                ["combinationExportPolicy"] = "Named --plans-input cases only; no 1225 pair expansion.",
                ["syntheticPassCount"] = syntheticPassCount,
                ["syntheticCaseCount"] = synthetic.Count,
                ["boundary"] = "Static production parser/selector proof only; no Unity, mobile, global combo-max, or runtime claim."
            };
            WriteJson(outputRoot, "summary.json", summary);

            Console.WriteLine("status=" + (coveragePass ? "PASS" : "FAIL"));
            Console.WriteLine("catalogObjects=" + actual.CatalogObjectRows);
            Console.WriteLine("selectableRoles=" + actual.SelectableRoots.Length);
            Console.WriteLine("fullCatalogDefinitions=" + fullCatalog.DefinitionIds.Count);
            Console.WriteLine("outputRoot=" + outputRoot);
            return coveragePass ? 0 : 2;
        }
        catch (Exception error)
        {
            try
            {
                WriteJson(
                    outputRoot,
                    "failure.json",
                    new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["taskId"] = FullTaskId,
                        ["status"] = "FAIL",
                        ["mode"] = "full-catalog",
                        ["errorType"] = error.GetType().FullName,
                        ["error"] = error.Message,
                        ["stackTrace"] = error.ToString()
                    });
            }
            catch (Exception writeError)
            {
                Console.Error.WriteLine("failure-artifact-error: " + writeError.Message);
            }

            Console.Error.WriteLine("status=FAIL");
            Console.Error.WriteLine(error);
            Console.Error.WriteLine("outputRoot=" + outputRoot);
            return 1;
        }
    }

    private static ActualRun LoadFullCatalogRun(string repositoryRoot)
    {
        string runtimeRoot = Path.GetFullPath(Path.Combine(repositoryRoot, LoganRuntimeRelativePath));
        string catalogPath = Path.Combine(runtimeRoot, "catalog.csv");
        EnsureFile(catalogPath);
        BattleContentSource source = BattleContentSource.ForLoganRuntime(runtimeRoot);
        List<CatalogEntry> entries = ReadRuntimeCatalogEntries(catalogPath, source);
        if (entries.Count != 330)
            throw new InvalidDataException("Expected 330 current catalog object rows, got " + entries.Count + ".");

        var actual = new ActualRun
        {
            RepositoryRoot = repositoryRoot,
            RuntimeRoot = runtimeRoot,
            DecodedDatRoot = source.DatRoot,
            CatalogPath = catalogPath,
            CatalogSha256 = HashFile(catalogPath),
            CatalogObjectRows = entries.Count,
            CatalogEntries = entries
        };

        LoganFusionCatalogInput fusionInput = LoganFusionCatalogInput.Capture(source.DatRoot, runtimeRoot);
        actual.Fusion = fusionInput.Catalog;
        actual.FusionPath = fusionInput.SelectedPath;
        actual.FusionSha256 = fusionInput.SelectedPath == null ? null : HashFile(fusionInput.SelectedPath);
        if (actual.Fusion == null || !actual.Fusion.IsValid)
            throw new InvalidDataException("The current fusion catalog is invalid.");

        string kindPath = FindFirstExistingFile(
            Path.Combine(source.DatRoot, "data", "kind.dat"),
            Path.Combine(runtimeRoot, "data", "kind.dat"),
            Path.Combine(runtimeRoot, "dat", "data", "kind.dat"),
            Path.Combine(runtimeRoot, "assets", "data", "kind.dat"),
            Path.Combine(runtimeRoot, "NTSD2.8", "data", "kind.dat"));
        EnsureFile(kindPath);
        actual.KindPath = kindPath;
        actual.KindSha256 = HashFile(kindPath);
        actual.Kind = LoganKindCatalogParser.ParseText(File.ReadAllText(kindPath, Encoding.UTF8));
        if (actual.Kind == null || !actual.Kind.IsValid)
            throw new InvalidDataException("The current kind catalog is invalid.");

        var parser = new Lf2DatParserV2();
        foreach (CatalogEntry entry in entries.OrderBy(value => value.RegistryIndex))
        {
            try
            {
                entry.DatPath = ResolveCatalogDatPath(entry, source);
                EnsureFile(entry.DatPath);
                string text = File.ReadAllText(entry.DatPath, Encoding.UTF8);
                Lf2DatFile dat = parser.ParseLoganContent(text, entry.DatPath);
                ParsedObject parsed = BuildParsedObject(entry, dat, source);
                parsed.RegistryIndex = entry.RegistryIndex;
                parsed.CatalogName = entry.Name;
                parsed.SourcePath = entry.SourcePath;
                parsed.PublishedFolder = entry.PublishedFolder;
                parsed.CatalogLine = entry.CatalogLine;
                parsed.DatPath = entry.DatPath;
                parsed.DatBytes = new FileInfo(entry.DatPath).Length;
                parsed.DatSha256 = HashFile(entry.DatPath);
                parsed.PublishedConfig = !string.IsNullOrWhiteSpace(entry.PublishedFolder);
                parsed.PublishedFolderExists = Directory.Exists(
                    source.ResolvePublishedFolderPath(entry.PublishedFolder));
                parsed.NativeHidden = parsed.Wrapper.characterData.NativeMetadata.Bmp.Int32OrDefault("hidden", 0);
                foreach (SourceDeclaration declaration in parsed.Files)
                    CaptureImageIdentity(declaration);

                if (!actual.Objects.TryAdd(entry.Id, parsed) ||
                    !actual.Configs.TryAdd(entry.Id, parsed.Wrapper) ||
                    !actual.Types.TryAdd(entry.Id, entry.Type))
                {
                    throw new InvalidDataException("Duplicate current catalog OID " + entry.Id + ".");
                }
            }
            catch (Exception error)
            {
                actual.ParseFailures.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["registryIndex"] = entry.RegistryIndex,
                    ["catalogLine"] = entry.CatalogLine,
                    ["id"] = entry.Id,
                    ["type"] = entry.Type,
                    ["sourcePath"] = entry.SourcePath,
                    ["publishedFolder"] = entry.PublishedFolder,
                    ["datPath"] = entry.DatPath,
                    ["errorType"] = error.GetType().FullName,
                    ["error"] = error.Message
                });
            }
        }

        if (actual.ParseFailures.Count != 0)
            throw new InvalidDataException("At least one current catalog DAT failed parser/converter reconstruction.");

        actual.AllCatalogRoots = entries.OrderBy(value => value.RegistryIndex).Select(value => value.Id).ToArray();
        actual.SelectableRoots = entries
            .Where(value => actual.Objects.ContainsKey(value.Id) &&
                value.Type == 0 &&
                actual.Objects[value.Id].NativeHidden == 0 &&
                actual.Objects[value.Id].PublishedConfig)
            .OrderBy(value => value.RegistryIndex)
            .Select(value => value.Id)
            .ToArray();
        actual.CatalogSelectableCount = actual.SelectableRoots.Length;
        if (actual.CatalogSelectableCount != 50)
            throw new InvalidDataException("Expected 50 selectable catalog roles, got " + actual.CatalogSelectableCount + ".");

        StageResolutionEvidence stage = ReadConfiguredStageAllWaveRoots(repositoryRoot);
        actual.StageAllWaveRoots = stage.Roots;
        actual.StageRootsSource = stage.Status;
        actual.StageInputScope = stage.InputScope;
        actual.StageConfigValue = stage.ConfigValue;
        actual.StageResolvedPath = stage.ResolvedPath;
        actual.StageSelectedFileExists = stage.SelectedFileExists;
        actual.StageParseStatus = stage.ParseStatus;
        actual.StageUnverified = stage.Unverified;
        actual.StageSourceSha256 = stage.SourceSha256;
        return actual;
    }

    private static string ResolveCatalogDatPath(CatalogEntry entry, BattleContentSource source)
    {
        if (!string.IsNullOrWhiteSpace(entry.PublishedFolder))
        {
            string publishedPath = source.ResolvePublishedFolderPath(entry.PublishedFolder);
            if (Directory.Exists(publishedPath))
            {
                string[] files = Directory.EnumerateFiles(publishedPath)
                    .Where(path => string.Equals(Path.GetExtension(path), ".dat", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (files.Length != 1)
                {
                    throw new InvalidDataException(
                        "Published object folder must contain exactly one top-level DAT: " + publishedPath);
                }

                return Path.GetFullPath(files[0]);
            }
        }

        return source.ResolveDatPath(entry.SourcePath);
    }

    private static List<CatalogEntry> ReadRuntimeCatalogEntries(string path, BattleContentSource source)
    {
        using var reader = new StringReader(File.ReadAllText(path, Encoding.UTF8));
        string headerLine = reader.ReadLine();
        if (headerLine == null)
            throw new InvalidDataException("Current catalog.csv is empty.");

        List<string> header = ParseCsvRow(headerLine);
        var columns = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int index = 0; index < header.Count; index++)
            columns[header[index]] = index;
        foreach (string required in new[]
        {
            "registry_section", "registry_index", "id", "type", "source_path", "published_folder", "name", "hidden"
        })
        {
            if (!columns.ContainsKey(required))
                throw new InvalidDataException("Current catalog.csv is missing column: " + required);
        }

        var entries = new List<CatalogEntry>();
        var seenIds = new HashSet<int>();
        var seenIndices = new HashSet<int>();
        int lineNumber = 1;
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;
            List<string> row = ParseCsvRow(line);
            string Field(string name)
            {
                int column = columns[name];
                return column < row.Count ? row[column] : null;
            }

            if (!string.Equals(Field("registry_section"), "object", StringComparison.Ordinal))
                continue;
            int hidden = -1;
            string hiddenText = Field("hidden");
            if (!string.IsNullOrWhiteSpace(hiddenText) && !TryNativeInt(hiddenText, out hidden))
            {
                throw new InvalidDataException("Line " + lineNumber + ": current catalog hidden value is invalid.");
            }
            if (!TryNativeInt(Field("registry_index"), out int registryIndex) || registryIndex < 0 ||
                !TryNativeInt(Field("id"), out int id) ||
                !TryNativeInt(Field("type"), out int type) ||
                string.IsNullOrWhiteSpace(Field("source_path")) ||
                string.IsNullOrWhiteSpace(Field("published_folder")))
            {
                throw new InvalidDataException("Line " + lineNumber + ": current catalog object row is invalid.");
            }
            if (!seenIds.Add(id))
                throw new InvalidDataException("Line " + lineNumber + ": duplicate current catalog OID " + id + ".");
            if (!seenIndices.Add(registryIndex))
                throw new InvalidDataException("Line " + lineNumber + ": duplicate current catalog registry index " + registryIndex + ".");

            entries.Add(new CatalogEntry
            {
                RegistryIndex = registryIndex,
                Id = id,
                Type = type,
                SourcePath = Field("source_path"),
                PublishedFolder = Field("published_folder"),
                Name = Field("name") ?? string.Empty,
                Hidden = hidden,
                CatalogLine = lineNumber,
                DatPath = null
            });
        }

        return entries;
    }

    private static List<string> ParseCsvRow(string line)
    {
        var columns = new List<string>();
        var value = new StringBuilder();
        bool quoted = false;
        for (int index = 0; index < line.Length; index++)
        {
            char character = line[index];
            if (quoted)
            {
                if (character == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        value.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else
                {
                    value.Append(character);
                }
            }
            else if (character == '"')
            {
                quoted = true;
            }
            else if (character == ',')
            {
                columns.Add(value.ToString());
                value.Clear();
            }
            else
            {
                value.Append(character);
            }
        }

        columns.Add(value.ToString());
        return columns;
    }

    private static bool TryNativeInt(string value, out int result)
    {
        result = 0;
        if (string.IsNullOrEmpty(value))
            return false;
        int first = value[0] == '-' ? 1 : 0;
        if (first == value.Length)
            return false;
        for (int index = first; index < value.Length; index++)
        {
            if (value[index] < '0' || value[index] > '9')
                return false;
        }

        return int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);
    }

    private static string FindFirstExistingFile(params string[] candidates)
    {
        foreach (string candidate in candidates ?? Array.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        return null;
    }

    private sealed class StageResolutionEvidence
    {
        public int[] Roots = Array.Empty<int>();
        public string Status;
        public string InputScope;
        public string ConfigValue;
        public string ResolvedPath;
        public bool SelectedFileExists;
        public string ParseStatus;
        public bool Unverified;
        public Dictionary<string, string> SourceSha256 = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static StageResolutionEvidence ReadConfiguredStageAllWaveRoots(string repositoryRoot)
    {
        string matchConfigPath = Path.Combine(repositoryRoot, "Assets", "NTSD", "Scripts", "App", "MatchConfig.cs");
        string simulationTickDriverPath = Path.Combine(
            repositoryRoot, "Assets", "NTSD", "Scripts", "Simulation", "Host", "SimulationTickDriver.cs");
        string stageLoaderPath = Path.Combine(
            repositoryRoot, "Assets", "NTSD", "Scripts", "Simulation", "Stage", "BattleStageCampaignLoader.cs");
        EnsureFile(matchConfigPath);
        EnsureFile(simulationTickDriverPath);
        EnsureFile(stageLoaderPath);

        string matchConfigText = File.ReadAllText(matchConfigPath, Encoding.UTF8);
        string simulationTickDriverText = File.ReadAllText(simulationTickDriverPath, Encoding.UTF8);
        string stageLoaderText = File.ReadAllText(stageLoaderPath, Encoding.UTF8);
        if (!matchConfigText.Contains("public string stageCampaignFilePath = string.Empty;", StringComparison.Ordinal) ||
            !simulationTickDriverText.Contains("BattleStageCampaignLoader.LoadFromFile(", StringComparison.Ordinal) ||
            !simulationTickDriverText.Contains("config?.stageCampaignFilePath", StringComparison.Ordinal) ||
            !stageLoaderText.Contains("Path.Combine(Application.streamingAssetsPath, \"NTSD\", \"data\", \"stage.dat\")", StringComparison.Ordinal) ||
            !stageLoaderText.Contains("string.IsNullOrWhiteSpace(filePath) ? DefaultStageDatPath : filePath", StringComparison.Ordinal) ||
            !stageLoaderText.Contains("Directory.GetParent(Application.dataPath)", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Stage source contract changed; refusing to infer stage roots.");
        }

        var sourceHashes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Assets/NTSD/Scripts/App/MatchConfig.cs"] = HashFile(matchConfigPath),
            ["Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs"] = HashFile(simulationTickDriverPath),
            ["Assets/NTSD/Scripts/Simulation/Stage/BattleStageCampaignLoader.cs"] = HashFile(stageLoaderPath)
        };
        string defaultResolvedPath = Path.GetFullPath(Path.Combine(
            repositoryRoot, "Assets", "StreamingAssets", "NTSD", "data", "stage.dat"));
        bool selectedFileExists = File.Exists(defaultResolvedPath);
        if (selectedFileExists)
        {
            return new StageResolutionEvidence
            {
                Roots = Array.Empty<int>(),
                Status = "UNVERIFIED_STAGE_FILE_PRESENT",
                InputScope = "Default MatchConfig only; dynamic custom caller stage paths are not auto-consumed by this worker.",
                ConfigValue = string.Empty,
                ResolvedPath = defaultResolvedPath,
                SelectedFileExists = true,
                ParseStatus = "UNVERIFIED: default stage file exists but this standalone worker does not compile the Unity stage loader/decryptor path.",
                Unverified = true,
                SourceSha256 = sourceHashes
            };
        }

        return new StageResolutionEvidence
        {
            Roots = Array.Empty<int>(),
            Status = "VERIFIED_EMPTY_DEFAULT_MATCH",
            InputScope = "Default MatchConfig only; dynamic custom caller stage paths are not auto-consumed by this worker.",
            ConfigValue = string.Empty,
            ResolvedPath = defaultResolvedPath,
            SelectedFileExists = false,
            ParseStatus = "PASS: MatchConfig.stageCampaignFilePath defaults empty; BattleStageCampaignLoader resolved the default project candidate, which is absent, so the production loader supplies no campaigns.",
            Unverified = false,
            SourceSha256 = sourceHashes
        };
    }

    private static void CaptureImageIdentity(SourceDeclaration declaration)
    {
        declaration.ImageExists = !string.IsNullOrWhiteSpace(declaration.ImagePath) &&
            File.Exists(declaration.ImagePath);
        if (!declaration.ImageExists)
            return;
        declaration.ImageBytes = new FileInfo(declaration.ImagePath).Length;
        declaration.ImageSha256 = HashFile(declaration.ImagePath);
    }

    private static void ParseArguments(
        string[] args,
        out string repositoryRoot,
        out string outputRoot,
        out string budgetInput,
        out bool fullCatalog,
        out string plansInput)
    {
        repositoryRoot = null;
        outputRoot = null;
        budgetInput = null;
        fullCatalog = false;
        plansInput = null;
        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];
            if (string.Equals(argument, "--full-catalog", StringComparison.OrdinalIgnoreCase))
            {
                fullCatalog = true;
                continue;
            }
            if (index + 1 >= args.Length)
                throw new ArgumentException("An argument value is required: " + argument);

            string value = args[++index];
            if (string.Equals(argument, "--repository-root", StringComparison.OrdinalIgnoreCase))
                repositoryRoot = value;
            else if (string.Equals(argument, "--output-root", StringComparison.OrdinalIgnoreCase))
                outputRoot = value;
            else if (string.Equals(argument, "--budget-input", StringComparison.OrdinalIgnoreCase))
                budgetInput = value;
            else if (string.Equals(argument, "--plans-input", StringComparison.OrdinalIgnoreCase))
                plansInput = value;
            else
                throw new ArgumentException("Unknown argument: " + argument);
        }

        if (plansInput != null && !fullCatalog)
            throw new ArgumentException("--plans-input is only valid with --full-catalog.");
        if (budgetInput != null && fullCatalog)
            throw new ArgumentException("--budget-input cannot be combined with --full-catalog.");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo current = new DirectoryInfo(Environment.CurrentDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md")) &&
                Directory.Exists(Path.Combine(current.FullName, "Assets", "NTSD")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find the Unity_GAS repository root from the current directory.");
    }

    private static string CreateFreshOutputRoot(string requestedPath, string selectorWorkerRoot)
    {
        string fullRequested = Path.GetFullPath(requestedPath);
        string fullWorkerRoot = Path.GetFullPath(selectorWorkerRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string prefix = fullWorkerRoot + Path.DirectorySeparatorChar;
        if (!fullRequested.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Diagnostic output must remain under the selector-worker directory.",
                nameof(requestedPath));
        }

        string parent = Path.GetDirectoryName(fullRequested);
        Directory.CreateDirectory(parent);
        string candidate = fullRequested;
        int suffix = 0;
        while (Directory.Exists(candidate) || File.Exists(candidate))
        {
            suffix++;
            candidate = fullRequested + "-run-" + suffix.ToString("D2");
        }

        Directory.CreateDirectory(candidate);
        return candidate;
    }

    private static bool IsPathUnderRoot(string requestedPath, string root)
    {
        string fullRequested = Path.GetFullPath(requestedPath);
        string fullRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string prefix = fullRoot + Path.DirectorySeparatorChar;
        return fullRequested.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static ActualRun LoadActualRun(string repositoryRoot)
    {
        string capturePath = Path.GetFullPath(Path.Combine(repositoryRoot, CaptureRelativePath));
        string selectionPath = Path.GetFullPath(Path.Combine(repositoryRoot, FixedSelectionRelativePath));
        string oldResultPath = Path.GetFullPath(Path.Combine(repositoryRoot, OldResultRelativePath));
        EnsureFile(capturePath);
        EnsureFile(selectionPath);
        EnsureFile(oldResultPath);

        var actual = new ActualRun
        {
            RepositoryRoot = repositoryRoot,
            CapturePath = capturePath,
            SelectionPath = selectionPath,
            OldResultPath = oldResultPath,
            CaptureSha256 = HashFile(capturePath),
            SelectionSha256 = HashFile(selectionPath),
            OldResultSha256 = HashFile(oldResultPath)
        };

        List<CatalogEntry> entries;
        using (JsonDocument capture = JsonDocument.Parse(File.OpenRead(capturePath)))
        {
            JsonElement root = capture.RootElement;
            actual.RuntimeRoot = root.GetProperty("runtimeRoot").GetString();
            actual.DecodedDatRoot = root.GetProperty("decodedDatRoot").GetString();
            entries = ReadCatalogEntries(root.GetProperty("objects"));
        }

        if (entries.Count != 330)
            throw new InvalidDataException("Expected 330 capture objects, got " + entries.Count + ".");

        actual.SelectedRoots = ReadFixedSelection(selectionPath);
        if (actual.SelectedRoots.Length != 8 ||
            !actual.SelectedRoots.SequenceEqual(new[] { 28, 18, 65, 23, 8, 10, 66, 27 }))
        {
            throw new InvalidDataException(
                "The fixed eight-role selection changed; this worker never rerolls it.");
        }

        string fusionPath;
        string kindPath;
        using (JsonDocument capture = JsonDocument.Parse(File.OpenRead(capturePath)))
        {
            fusionPath = capture.RootElement.GetProperty("fusion").GetProperty("selectedPath").GetString();
            kindPath = capture.RootElement.GetProperty("kind").GetProperty("selectedPath").GetString();
        }

        actual.FusionPath = fusionPath;
        actual.KindPath = kindPath;
        EnsureFile(fusionPath);
        EnsureFile(kindPath);
        actual.FusionSha256 = HashFile(fusionPath);
        actual.KindSha256 = HashFile(kindPath);
        actual.Fusion = LoganFusionCatalogParser.ParseText(
            File.ReadAllText(fusionPath, Encoding.UTF8));
        actual.Kind = LoganKindCatalogParser.ParseText(
            File.ReadAllText(kindPath, Encoding.UTF8));
        if (!actual.Fusion.IsValid)
            throw new InvalidDataException("The actual fusion catalog is invalid.");
        if (!actual.Kind.IsValid)
            throw new InvalidDataException("The actual kind catalog is invalid.");

        var source = BattleContentSource.ForLoganRuntime(actual.RuntimeRoot);
        var parser = new Lf2DatParserV2();
        foreach (CatalogEntry entry in entries.OrderBy(value => value.RegistryIndex))
        {
            try
            {
                EnsureFile(entry.DatPath);
                string text = File.ReadAllText(entry.DatPath, Encoding.UTF8);
                Lf2DatFile dat = parser.ParseLoganContent(text, entry.DatPath);
                ParsedObject parsed = BuildParsedObject(entry, dat, source);
                actual.Objects.Add(entry.Id, parsed);
                actual.Configs.Add(entry.Id, parsed.Wrapper);
                actual.Types.Add(entry.Id, entry.Type);
            }
            catch (Exception error)
            {
                actual.ParseFailures.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["registryIndex"] = entry.RegistryIndex,
                    ["id"] = entry.Id,
                    ["type"] = entry.Type,
                    ["datPath"] = entry.DatPath,
                    ["errorType"] = error.GetType().FullName,
                    ["error"] = error.Message
                });
            }
        }

        if (actual.ParseFailures.Count != 0)
        {
            throw new InvalidDataException(
                "At least one current catalog DAT failed parser/converter reconstruction.");
        }

        return actual;
    }

    private static List<CatalogEntry> ReadCatalogEntries(JsonElement objects)
    {
        var result = new List<CatalogEntry>();
        var seenIds = new HashSet<int>();
        foreach (JsonElement item in objects.EnumerateArray())
        {
            var entry = new CatalogEntry
            {
                RegistryIndex = item.GetProperty("registryIndex").GetInt32(),
                Id = item.GetProperty("id").GetInt32(),
                Type = item.GetProperty("type").GetInt32(),
                DatPath = item.GetProperty("datPath").GetString()
            };
            if (!seenIds.Add(entry.Id))
                throw new InvalidDataException("Capture contains duplicate OID " + entry.Id + ".");
            result.Add(entry);
        }

        return result;
    }

    private static int[] ReadFixedSelection(string path)
    {
        using (JsonDocument selection = JsonDocument.Parse(File.OpenRead(path)))
        {
            return selection.RootElement
                .GetProperty("selectedOids")
                .EnumerateArray()
                .Select(value => value.GetInt32())
                .ToArray();
        }
    }

    private static ParsedObject BuildParsedObject(
        CatalogEntry entry,
        Lf2DatFile dat,
        BattleContentSource source)
    {
        if (dat == null)
            throw new InvalidDataException("Parser returned null.");
        if (dat.LoganStats == null)
            throw new InvalidDataException("Manager.ParseDatFile requires Logan stats metadata.");
        if (dat.LoganArmors == null)
            throw new InvalidDataException("Manager.ParseDatFile requires Logan armor metadata.");

        var data = new LF2CharacterData
        {
            NativeMetadata = new LoganDefinitionMetadata(
                LoganDefinitionMetadata.CopyFields(dat.Bmp?.Properties),
                LoganDefinitionMetadata.CopyFields(dat.LoganStats.Properties),
                dat.LoganArmors.Select(armor => LoganDefinitionMetadata.CopyFields(armor.Properties)),
                dat.LoganWeaponPiece == null
                    ? null
                    : new LoganWeaponPieceDefinition(dat.LoganWeaponPiece)),
            frames = new List<LF2FrameData>()
        };

        foreach (Lf2FrameBlock frameBlock in dat.Frames)
        {
            LF2FrameData frame = Lf2DatConverter.ConvertLoganFrameData(frameBlock);
            if (frame != null)
                data.frames.Add(frame);
        }

        string datDirectory = Path.GetDirectoryName(Path.GetFullPath(entry.DatPath));
        var parsed = new ParsedObject
        {
            Id = entry.Id,
            Type = entry.Type,
            HeadPath = string.Empty,
            SmallPath = string.Empty
        };

        if (dat.Bmp != null)
        {
            data.name = dat.Bmp.Name ?? "Unknown";
            data.head = source.ResolveImagePath(dat.Bmp.Head, datDirectory);
            data.small = source.ResolveImagePath(dat.Bmp.Small, datDirectory);
            parsed.HeadPath = data.head;
            parsed.SmallPath = data.small;

            long nextPic = 0;
            foreach (Lf2SpriteFileDef fileDef in dat.Bmp.Files)
            {
                long product = (long)fileDef.Row * fileDef.Col;
                long capacity = Math.Max(product, 0L);
                int start = checked((int)nextPic);
                nextPic = checked(nextPic + capacity);
                int end = checked((int)(nextPic - 1));
                string imagePath = source.ResolveImagePath(fileDef.Path, datDirectory);
                var runtimeFile = new SpriteFileInfo(
                    imagePath,
                    start,
                    end,
                    fileDef.Width,
                    fileDef.Height,
                    fileDef.Row,
                    fileDef.Col);
                data.files.Add(runtimeFile);
                parsed.Files.Add(new SourceDeclaration
                {
                    SourcePath = fileDef.Path.Replace('\\', '/'),
                    ImagePath = imagePath,
                    DeclaredStart = fileDef.StartIndex,
                    DeclaredEnd = fileDef.EndIndex,
                    RuntimeFile = runtimeFile
                });
            }
        }

        if (data.type_sub == 0)
            data.type_sub = entry.Id;

        Lf2DatConverter.ApplyNativeInputDefinitionData(dat, data, loganContent: true);
        Lf2DatConverter.ApplyNativeArmorDefinitionData(dat, data, loganContent: true);
        parsed.Wrapper = new LF2CharacterDataWrapper(entry.Id, data);
        return parsed;
    }

    private static List<object> RunSyntheticTests()
    {
        var results = new List<object>
        {
            RunSynthetic("multiopoint-cycle-and-illegal-kind", TestMultiOpointCycle),
            RunSynthetic("missing-downstream-and-illegal-oid", TestMissingDownstream),
            RunSynthetic("type-and-hit-fa-rules", TestTypeAndHitFaRules),
            RunSynthetic("weapon-piece-all-variants-and-default", TestWeaponPieceRules),
            RunSynthetic("fusion-both-inputs-fixed-point-recursive", TestFusionFixedPoint),
            RunSynthetic("respond-is-context-not-spawn", TestRespondContext),
            RunSynthetic("cpoint-is-copresent-context", TestCpointContext),
            RunSynthetic("scenario-and-compatibility-branches", TestScenarioAndCompatibility),
            RunSynthetic("immutable-selection-and-stable-fingerprint", TestImmutablePlan),
            RunSynthetic("missing-type-refuses-incomplete-plan", TestMissingType),
            RunSynthetic("stage-root-and-fusion-closure", TestStageRootFusion)
        };
        return results;
    }

    private static object RunSynthetic(string name, Action test)
    {
        try
        {
            test();
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["name"] = name,
                ["status"] = "PASS"
            };
        }
        catch (Exception error)
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["name"] = name,
                ["status"] = "FAIL",
                ["errorType"] = error.GetType().FullName,
                ["error"] = error.Message
            };
        }
    }

    private static void TestMultiOpointCycle()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [1] = Wrapper(1, FrameWithPoints(
                10,
                Point(1, 2),
                Point(2, 3),
                Point(3, 4),
                Point(1, -1))),
            [2] = Wrapper(2, FrameWithPoints(20, Point(1, 1))),
            [3] = Wrapper(3),
            [4] = Wrapper(4)
        };
        var types = new Dictionary<int, int> { [1] = 0, [2] = 1, [3] = 1, [4] = 1 };
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 1 },
            EmptyFusion(),
            EmptyKind(),
            includeScenario: false,
            includeCompatibility: false);

        Expect(plan.DefinitionIds.SequenceEqual(new[] { 1, 2, 3 }), "illegal OPoint kind or OID entered closure");
        Expect(plan.Edges.Count(edge => edge.ToId == 2) == 1, "duplicate primary/list OPoint edge was emitted");
        Expect(plan.Edges.Any(edge => edge.ToId == 1), "cycle edge was not retained");
        Expect(!plan.IncludesDefinition(4), "illegal OPoint kind3 entered closure");
    }

    private static void TestMissingDownstream()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [5] = Wrapper(5, FrameWithPoints(1, Point(1, 777), Point(0, 778)))
        };
        var types = new Dictionary<int, int> { [5] = 0 };
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 5 },
            EmptyFusion(),
            EmptyKind(),
            includeScenario: false,
            includeCompatibility: false);
        Expect(plan.DefinitionIds.SequenceEqual(new[] { 5 }), "missing downstream was promoted into definitions");
        Expect(plan.MissingDependencies.SequenceEqual(new[] { 777 }), "missing downstream was not retained");
        Expect(plan.Edges.Any(edge => edge.ToId == 777), "missing edge was not retained");
    }

    private static void TestTypeAndHitFaRules()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [10] = Wrapper(10, FrameWithState(1, 9996)),
            [11] = Wrapper(11, FrameWithHitFa(2, 9)),
            [12] = Wrapper(12, FrameWithHitFa(3, 11)),
            [13] = Wrapper(13, FrameWithHitFa(4, 5)),
            [217] = Wrapper(217), [218] = Wrapper(218),
            [221] = Wrapper(221), [222] = Wrapper(222),
            [211] = Wrapper(211), [212] = Wrapper(212), [219] = Wrapper(219)
        };
        var types = new Dictionary<int, int>
        {
            [10] = 0, [11] = 1, [12] = 11, [13] = 1,
            [217] = 1, [218] = 1, [221] = 1, [222] = 1,
            [211] = 1, [212] = 1, [219] = 1
        };
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 10, 11, 12, 13 },
            EmptyFusion(),
            EmptyKind(),
            includeScenario: false,
            includeCompatibility: false);
        Expect(plan.IncludesDefinition(217) && plan.IncludesDefinition(218), "state9996 children missing");
        Expect(plan.IncludesDefinition(221) && plan.IncludesDefinition(222), "hit_Fa9 targets missing");
        Expect(plan.IncludesDefinition(211) && plan.IncludesDefinition(212), "hit_Fa11 targets missing");
        Expect(plan.IncludesDefinition(219), "hit_Fa5 target missing");
        Expect(!plan.MissingDependencies.Contains(220), "unexpected hit_Fa target missing marker");
    }

    private static void TestWeaponPieceRules()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [20] = WeaponMetadata(20, 42),
            [42] = Wrapper(42),
            [999] = Wrapper(999)
        };
        var types = new Dictionary<int, int> { [20] = 1, [42] = 1, [999] = 1 };
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 20 },
            EmptyFusion(),
            EmptyKind(),
            includeScenario: false,
            includeCompatibility: false);
        Expect(plan.IncludesDefinition(42), "authored weapon piece variant missing");
        Expect(plan.IncludesDefinition(999), "default weapon piece variant did not map to OID999");
    }

    private static void TestFusionFixedPoint()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>();
        foreach (int id in new[] { 30, 31, 32, 33, 34, 35 })
            configs[id] = Wrapper(id);
        var types = configs.Keys.ToDictionary(id => id, _ => 1);
        LoganFusionCatalog fusion = Fusion(
            FusionRecord(30, 31, 32, 1),
            FusionRecord(32, 33, 34, 2),
            FusionRecord(30, 999, 35, 3));
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 30, 31, 33 },
            fusion,
            EmptyKind(),
            includeScenario: false,
            includeCompatibility: false);
        Expect(plan.DefinitionIds.SequenceEqual(new[] { 30, 31, 32, 33, 34 }), "fusion fixed point did not recurse");
        Expect(!plan.IncludesDefinition(35), "one-sided fusion input was incorrectly expanded");
    }

    private static void TestRespondContext()
    {
        var attacker = Wrapper(40, FrameWithPoints(1, Point(1, 41)));
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [40] = attacker,
            [41] = Wrapper(41),
            [999] = Wrapper(999)
        };
        var types = new Dictionary<int, int> { [40] = 3, [41] = 0, [999] = 0 };
        LoganKindCatalog kind = Kind(new LoganKindRecord(7, 8, new[] { 40 }, new[] { 41, 999 }, 10));
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 40 },
            EmptyFusion(),
            kind,
            includeScenario: false,
            includeCompatibility: false);
        Expect(plan.IncludesDefinition(41), "legal OPoint partner missing");
        Expect(!plan.IncludesDefinition(999), "kind respond ID was treated as an automatic summon");
        Expect(plan.Edges.Any(edge => edge.FromId == 40 && edge.ToId == 41 && edge.Rule.StartsWith("kind context", StringComparison.Ordinal)), "kind context edge missing");
    }

    private static void TestCpointContext()
    {
        var source = Wrapper(50, new LF2FrameData { frameId = 1 });
        source.characterData.frames[0].cpoint = new CatchPoint { throwinjury = -1 };
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [50] = source,
            [51] = Wrapper(51),
            [52] = Wrapper(52)
        };
        var types = new Dictionary<int, int> { [50] = 0, [51] = 0, [52] = 0 };
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 50, 51 },
            EmptyFusion(),
            EmptyKind(),
            includeScenario: false,
            includeCompatibility: false);
        Expect(plan.DefinitionIds.SequenceEqual(new[] { 50, 51 }), "cpoint context added a non-copresent object");
        Expect(plan.Edges.Any(edge => edge.FromId == 50 && edge.ToId == 51 && edge.Rule.StartsWith("cpoint context", StringComparison.Ordinal)), "cpoint context edge missing");
        Expect(!plan.Edges.Any(edge => edge.ToId == 52), "non-copresent cpoint target was recorded");
    }

    private static void TestScenarioAndCompatibility()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [60] = Wrapper(60, FrameWithState(1, 9995), FrameWithState(2, 4001)),
            [1] = Wrapper(1), [50] = Wrapper(50), [100] = Wrapper(100),
            [150] = Wrapper(150), [199] = Wrapper(199), [998] = Wrapper(998), [209] = Wrapper(209),
            [200] = Wrapper(200)
        };
        var types = configs.Keys.ToDictionary(id => id, id => id == 60 || id == 100 ? 0 : 1);
        BattleImageLoadPlan plan = BattleVisualDependencySelector.Build(
            configs,
            types,
            new[] { 60 },
            EmptyFusion(),
            EmptyKind(),
            includeScenario: true,
            includeCompatibility: true);
        Expect(plan.IncludesDefinition(998), "scenario OID998 missing");
        Expect(plan.IncludesDefinition(100) && plan.IncludesDefinition(150) && plan.IncludesDefinition(199), "scenario loaded range missing");
        Expect(plan.IncludesDefinition(50) && plan.IncludesDefinition(1), "compatibility state transforms missing");
        Expect(plan.IncludesDefinition(209), "optional loaded OID209 missing");
        Expect(!plan.IncludesDefinition(200), "scenario expanded outside [100,199]");
    }

    private static void TestImmutablePlan()
    {
        var configs = new Dictionary<int, LF2CharacterDataWrapper>
        {
            [1] = Wrapper(1), [2] = Wrapper(2)
        };
        var types = new Dictionary<int, int> { [1] = 0, [2] = 0 };
        var roots = new List<int> { 2, 1, 2 };
        BattleImageLoadPlan first = BattleVisualDependencySelector.Build(
            configs, types, roots, EmptyFusion(), EmptyKind(), includeScenario: false, includeCompatibility: false);
        string fingerprint = first.Fingerprint;
        roots.Add(99);
        BattleImageLoadPlan second = BattleVisualDependencySelector.Build(
            configs, types, new[] { 1, 2 }, EmptyFusion(), EmptyKind(), includeScenario: false, includeCompatibility: false);
        Expect(first.Fingerprint == fingerprint && first.Fingerprint == second.Fingerprint, "fingerprint changed after input mutation/order change");
        Expect(first.RootIds.SequenceEqual(new[] { 1, 2 }), "roots were not normalized");
        Expect(first.SelectBodyConfigs(configs).Select(wrapper => wrapper.characterId).SequenceEqual(new[] { 1, 2 }), "SelectBodyConfigs order changed");
        bool threw = false;
        try
        {
            first.SelectBodyConfigs(new Dictionary<int, LF2CharacterDataWrapper> { [1] = configs[1] }).ToList();
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }
        Expect(threw, "SelectBodyConfigs did not reject a missing declared config");
        BattleImageLoadPlan menu = BattleImageLoadPlan.Menu();
        Expect(!menu.SelectBodyConfigs(null).Any(), "Menu plan did not remain body-empty");
        BattleImageLoadPlan full = BattleImageLoadPlan.FullCatalogue(new[] { 2, 1, 2 }, "test fallback");
        Expect(full.DefinitionIds.SequenceEqual(new[] { 1, 2 }) && full.FallbackReason == "test fallback", "full catalogue plan normalization failed");
    }

    private static LF2CharacterDataWrapper Wrapper(int id, params LF2FrameData[] frames)
    {
        var data = new LF2CharacterData
        {
            frames = frames == null ? new List<LF2FrameData>() : frames.ToList(),
            files = new List<SpriteFileInfo>()
        };
        data.NativeMetadata = EmptyMetadata();
        return new LF2CharacterDataWrapper(id, data);
    }

    private static LF2CharacterDataWrapper Wrapper(int id, LF2CharacterData data)
    {
        if (data.NativeMetadata == null)
            data.NativeMetadata = EmptyMetadata();
        return new LF2CharacterDataWrapper(id, data);
    }

    private static LF2FrameData FrameWithPoints(int frameId, params BattleObjectPointValue[] points)
    {
        var frame = new LF2FrameData { frameId = frameId };
        foreach (BattleObjectPointValue point in points ?? Array.Empty<BattleObjectPointValue>())
            frame.opoints.Add(point);
        if (frame.opoints.Count > 0)
            frame.opoint = frame.opoints[0];
        return frame;
    }

    private static LF2FrameData FrameWithState(int frameId, int state)
    {
        return new LF2FrameData { frameId = frameId, state = state };
    }

    private static LF2FrameData FrameWithHitFa(int frameId, int hitFa)
    {
        return new LF2FrameData { frameId = frameId, hit_Fa = hitFa };
    }

    private static BattleObjectPointValue Point(int kind, int oid)
    {
        return new BattleObjectPointValue(kind, 0, 0, 0, 0, 0, oid, 1);
    }

    private static LF2CharacterDataWrapper WeaponMetadata(int id, int variantOid)
    {
        var block = new Lf2WeaponPieceBlock();
        var group = new Lf2WeaponPieceGroup { Piece = 1 };
        group.Properties.Add(new Lf2DatProperty("amount", "1"));
        var defaultVariant = new Lf2WeaponPieceVariant { Piece = 1 };
        defaultVariant.Properties.Add(new Lf2DatProperty("oid", "-1"));
        var explicitVariant = new Lf2WeaponPieceVariant { Piece = 1 };
        explicitVariant.Properties.Add(new Lf2DatProperty("oid", variantOid.ToString()));
        group.Variants.Add(defaultVariant);
        group.Variants.Add(explicitVariant);
        block.Groups.Add(group);
        var data = new LF2CharacterData
        {
            NativeMetadata = new LoganDefinitionMetadata(
                EmptyFields(),
                EmptyFields(),
                null,
                new LoganWeaponPieceDefinition(block)),
            frames = new List<LF2FrameData>(),
            files = new List<SpriteFileInfo>()
        };
        return Wrapper(id, data);
    }

    private static LoganDefinitionMetadata EmptyMetadata()
    {
        return new LoganDefinitionMetadata(EmptyFields(), EmptyFields(), null, null);
    }

    private static LoganDefinitionFieldSet EmptyFields()
    {
        return new LoganDefinitionFieldSet(new KeyValuePair<string, string>[0]);
    }

    private static LoganFusionCatalog EmptyFusion()
    {
        return new LoganFusionCatalog(
            false,
            Array.Empty<LoganFusionRecord>(),
            Array.Empty<LoganFusionDiagnostic>());
    }

    private static LoganFusionCatalog Fusion(params LoganFusionRecord[] records)
    {
        return new LoganFusionCatalog(true, records, Array.Empty<LoganFusionDiagnostic>());
    }

    private static LoganFusionRecord FusionRecord(int first, int second, int result, int line)
    {
        return new LoganFusionRecord(
            first, second, result,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, line);
    }

    private static LoganKindCatalog EmptyKind()
    {
        return new LoganKindCatalog(
            false,
            Array.Empty<LoganKindRecord>(),
            Array.Empty<LoganKindDiagnostic>());
    }

    private static LoganKindCatalog Kind(params LoganKindRecord[] records)
    {
        return new LoganKindCatalog(true, records, Array.Empty<LoganKindDiagnostic>());
    }

    private static List<NamedPlanCase> ReadNamedPlanCases(string path)
    {
        EnsureFile(path);
        using JsonDocument document = JsonDocument.Parse(File.OpenRead(path));
        if (!document.RootElement.TryGetProperty("cases", out JsonElement cases) ||
            cases.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("--plans-input must contain a cases array.");
        }

        var result = new List<NamedPlanCase>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonElement item in cases.EnumerateArray())
        {
            string name = item.GetProperty("name").GetString();
            if (!IsSafeCaseName(name) || !names.Add(name))
                throw new InvalidDataException("Named plan case names must be unique and path-safe: " + name);
            int[] roots = ReadIntArray(item, "roots");
            if (roots.Length == 0)
                throw new InvalidDataException("Named plan case requires at least one root: " + name);
            int[] additionalRoots = item.TryGetProperty("additionalRoots", out JsonElement additional)
                ? ReadIntArray(additional, "additionalRoots")
                : Array.Empty<int>();
            result.Add(new NamedPlanCase
            {
                Name = name,
                Roots = roots,
                AdditionalRoots = additionalRoots
            });
        }

        return result;
    }

    private static int[] ReadIntArray(JsonElement parent, string propertyName)
    {
        JsonElement values = parent.ValueKind == JsonValueKind.Array
            ? parent
            : parent.GetProperty(propertyName);
        if (values.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException(propertyName + " must be an array.");
        return values.EnumerateArray().Select(value => value.GetInt32()).ToArray();
    }

    private static bool IsSafeCaseName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name == "." || name == "..")
            return false;
        foreach (char character in name)
        {
            if (!(character == '-' || character == '_' || character == '.' ||
                character >= '0' && character <= '9' ||
                character >= 'A' && character <= 'Z' ||
                character >= 'a' && character <= 'z'))
            {
                return false;
            }
        }

        return true;
    }

    private static int[] UnionRoots(IEnumerable<int> first, IEnumerable<int> second)
    {
        return (first ?? Array.Empty<int>())
            .Concat(second ?? Array.Empty<int>())
            .Distinct()
            .OrderBy(value => value)
            .ToArray();
    }

    private static object ProjectRole(CatalogEntry entry, ParsedObject parsed)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["oid"] = entry.Id,
            ["name"] = entry.Name,
            ["registryIndex"] = entry.RegistryIndex,
            ["catalogLine"] = entry.CatalogLine,
            ["type"] = entry.Type,
            ["catalogHidden"] = entry.Hidden,
            ["nativeHidden"] = parsed.NativeHidden,
            ["sourcePath"] = entry.SourcePath,
            ["publishedFolder"] = entry.PublishedFolder,
            ["publishedConfig"] = parsed.PublishedConfig,
            ["publishedFolderExists"] = parsed.PublishedFolderExists,
            ["datPath"] = parsed.DatPath,
            ["datBytes"] = parsed.DatBytes,
            ["datSha256"] = parsed.DatSha256,
            ["head"] = ProjectFileIdentity(parsed.HeadPath),
            ["small"] = ProjectFileIdentity(parsed.SmallPath),
            ["sourceDeclarationCount"] = parsed.Files.Count,
            ["sourceDeclarations"] = parsed.Files.Select(value => ProjectSourceDeclaration(value, fullIdentity: true)).ToList()
        };
    }

    private static object ProjectPlan(string name, BattleImageLoadPlan plan, ActualRun actual)
    {
        return ProjectPlan(name, plan, actual, fullIdentity: false);
    }

    private static object ProjectPlan(
        string name,
        BattleImageLoadPlan plan,
        ActualRun actual,
        bool fullIdentity)
    {
        var bodyConfigs = plan.SelectBodyConfigs(actual.Configs).ToList();
        var bodySources = new List<object>();
        foreach (LF2CharacterDataWrapper wrapper in bodyConfigs)
        {
            ParsedObject parsed = actual.Objects[wrapper.characterId];
            var source = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["definitionId"] = wrapper.characterId,
                ["headPath"] = parsed.HeadPath,
                ["smallPath"] = parsed.SmallPath,
                ["files"] = parsed.Files.Select(value => ProjectSourceDeclaration(value, fullIdentity)).ToList()
            };
            if (fullIdentity)
            {
                source["registryIndex"] = parsed.RegistryIndex;
                source["catalogName"] = parsed.CatalogName;
                source["catalogLine"] = parsed.CatalogLine;
                source["type"] = parsed.Type;
                source["sourcePath"] = parsed.SourcePath;
                source["publishedFolder"] = parsed.PublishedFolder;
                source["publishedConfig"] = parsed.PublishedConfig;
                source["publishedFolderExists"] = parsed.PublishedFolderExists;
                source["nativeHidden"] = parsed.NativeHidden;
                source["datPath"] = parsed.DatPath;
                source["datBytes"] = parsed.DatBytes;
                source["datSha256"] = parsed.DatSha256;
                source["head"] = ProjectFileIdentity(parsed.HeadPath);
                source["small"] = ProjectFileIdentity(parsed.SmallPath);
            }
            bodySources.Add(source);
        }

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["name"] = name,
            ["phase"] = plan.Phase.ToString(),
            ["definitionIds"] = plan.DefinitionIds,
            ["rootIds"] = plan.RootIds,
            ["missingDependencies"] = plan.MissingDependencies,
            ["edges"] = plan.Edges.Select(ProjectEdge).ToList(),
            ["fingerprint"] = plan.Fingerprint,
            ["fallbackReason"] = plan.FallbackReason,
            ["selectedWrapperIds"] = bodyConfigs.Select(wrapper => wrapper.characterId).ToList(),
            ["bodySourceDeclarations"] = bodySources,
            ["bodySourceDeclarationCount"] = parsedFileCount(bodyConfigs, actual)
        };
    }

    private static int parsedFileCount(
        IReadOnlyList<LF2CharacterDataWrapper> wrappers,
        ActualRun actual)
    {
        int count = 0;
        foreach (LF2CharacterDataWrapper wrapper in wrappers)
            count += actual.Objects[wrapper.characterId].Files.Count;
        return count;
    }

    private static object ProjectSourceDeclaration(SourceDeclaration declaration)
    {
        return ProjectSourceDeclaration(declaration, fullIdentity: false);
    }

    private static object ProjectSourceDeclaration(SourceDeclaration declaration, bool fullIdentity)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["sourcePath"] = declaration.SourcePath,
            ["imagePath"] = declaration.ImagePath,
            ["declaredStart"] = declaration.DeclaredStart,
            ["declaredEnd"] = declaration.DeclaredEnd,
            ["startFrame"] = declaration.RuntimeFile.startFrame,
            ["endFrame"] = declaration.RuntimeFile.endFrame,
            ["width"] = declaration.RuntimeFile.width,
            ["height"] = declaration.RuntimeFile.height,
            ["row"] = declaration.RuntimeFile.row,
            ["col"] = declaration.RuntimeFile.col
        };
        if (fullIdentity)
        {
            result["imageExists"] = declaration.ImageExists;
            result["imageBytes"] = declaration.ImageBytes;
            result["imageSha256"] = declaration.ImageSha256;
        }

        return result;
    }

    private static object ProjectFileIdentity(string path)
    {
        bool exists = !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["path"] = path,
            ["exists"] = exists,
            ["bytes"] = exists ? new FileInfo(path).Length : 0L,
            ["sha256"] = exists ? HashFile(path) : null
        };
    }

    private static object ProjectEdge(BattleVisualDependencyEdge edge)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["fromId"] = edge.FromId,
            ["toId"] = edge.ToId,
            ["frameId"] = edge.FrameId,
            ["rule"] = edge.Rule
        };
    }

    private static object PlanSummary(BattleImageLoadPlan plan, ActualRun actual)
    {
        var wrappers = plan.SelectBodyConfigs(actual.Configs).ToList();
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["phase"] = plan.Phase.ToString(),
            ["definitionCount"] = plan.DefinitionIds.Count,
            ["rootCount"] = plan.RootIds.Count,
            ["missingCount"] = plan.MissingDependencies.Count,
            ["edgeCount"] = plan.Edges.Count,
            ["fingerprint"] = plan.Fingerprint,
            ["selectedWrapperCount"] = wrappers.Count,
            ["bodySourceDeclarationCount"] = wrappers.Sum(wrapper => actual.Objects[wrapper.characterId].Files.Count)
        };
    }

    private static object BuildInputProjection(ActualRun actual)
    {
        var sourceFiles = new[]
        {
            "Assets/NTSD/Scripts/Animation/Runtime/BattleImageLoadPlan.cs",
            "Assets/NTSD/Scripts/Animation/Runtime/BattleVisualDependencySelector.cs",
            "Assets/NTSD/Scripts/Animation/BattleContentSource.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganFrames.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs",
            "Assets/NTSD/Scripts/Animation/LF2CharacterData.cs",
            "Assets/NTSD/Scripts/Animation/LF2FrameData.cs",
            "Assets/NTSD/Scripts/Animation/LoganDefinitionMetadata.cs",
            "Assets/NTSD/Scripts/Animation/LoganWeaponPieceDefinition.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganFusionCatalog.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganFusionCatalogParser.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganKindCatalogParser.cs"
        };
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["capture"] = actual.CaptureSha256,
            ["fixedSelection"] = actual.SelectionSha256,
            ["oldResult10Final"] = actual.OldResultSha256,
            ["fusionCatalog"] = actual.FusionSha256,
            ["kindCatalog"] = actual.KindSha256
        };
        foreach (string relativePath in sourceFiles)
            hashes[relativePath] = HashFile(Path.Combine(actual.RepositoryRoot, relativePath));

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["taskId"] = TaskId,
            ["commandLine"] = Environment.CommandLine,
            ["commandArgs"] = Environment.GetCommandLineArgs(),
            ["workingDirectory"] = Environment.CurrentDirectory,
            ["repositoryRoot"] = actual.RepositoryRoot,
            ["runtimeRoot"] = actual.RuntimeRoot,
            ["decodedDatRoot"] = actual.DecodedDatRoot,
            ["capturePath"] = actual.CapturePath,
            ["fixedSelectionPath"] = actual.SelectionPath,
            ["oldResult10FinalPath"] = actual.OldResultPath,
            ["fusionPath"] = actual.FusionPath,
            ["kindPath"] = actual.KindPath,
            ["inputSha256"] = hashes,
            ["catalogObjectRows"] = actual.Objects.Count,
            ["parseFailures"] = actual.ParseFailures
        };
    }

    private static object BuildFullInputProjection(ActualRun actual)
    {
        var sourceFiles = new[]
        {
            "Assets/NTSD/Scripts/Animation/Runtime/BattleImageLoadPlan.cs",
            "Assets/NTSD/Scripts/Animation/Runtime/BattleVisualDependencySelector.cs",
            "Assets/NTSD/Scripts/Animation/BattleContentSource.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganFrames.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs",
            "Assets/NTSD/Scripts/Animation/LF2CharacterData.cs",
            "Assets/NTSD/Scripts/Animation/LF2FrameData.cs",
            "Assets/NTSD/Scripts/Animation/LoganDefinitionMetadata.cs",
            "Assets/NTSD/Scripts/Animation/LoganWeaponPieceDefinition.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganFusionCatalog.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganFusionCatalogParser.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs",
            "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganKindCatalogParser.cs"
        };
        var sourceHashes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["catalog"] = actual.CatalogSha256,
            ["fusionCatalog"] = actual.FusionSha256,
            ["kindCatalog"] = actual.KindSha256
        };
        foreach (string relativePath in sourceFiles)
            sourceHashes[relativePath] = HashFile(Path.Combine(actual.RepositoryRoot, relativePath));

        var datIdentities = new List<object>();
        foreach (CatalogEntry entry in actual.CatalogEntries.OrderBy(value => value.RegistryIndex))
        {
            ParsedObject parsed = actual.Objects[entry.Id];
            datIdentities.Add(new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["oid"] = entry.Id,
                ["registryIndex"] = entry.RegistryIndex,
                ["catalogLine"] = entry.CatalogLine,
                ["type"] = entry.Type,
                ["sourcePath"] = entry.SourcePath,
                ["publishedFolder"] = entry.PublishedFolder,
                ["datPath"] = parsed.DatPath,
                ["bytes"] = parsed.DatBytes,
                ["sha256"] = parsed.DatSha256,
                ["parseStatus"] = "PASS"
            });
        }

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["taskId"] = FullTaskId,
            ["mode"] = "full-catalog",
            ["repositoryRoot"] = actual.RepositoryRoot,
            ["runtimeRoot"] = actual.RuntimeRoot,
            ["decodedDatRoot"] = actual.DecodedDatRoot,
            ["catalogPath"] = actual.CatalogPath,
            ["catalogObjectRows"] = actual.CatalogObjectRows,
            ["catalogSelectableCount"] = actual.CatalogSelectableCount,
            ["expectedSelectableCount"] = 50,
            ["selectableRoots"] = actual.SelectableRoots,
            ["allCatalogRootsCount"] = actual.AllCatalogRoots.Length,
            ["fusionPath"] = actual.FusionPath,
            ["fusionSha256"] = actual.FusionSha256,
            ["kindPath"] = actual.KindPath,
            ["kindSha256"] = actual.KindSha256,
            ["stageAllWaveRoots"] = actual.StageAllWaveRoots,
            ["stageRootsSource"] = actual.StageRootsSource,
            ["stageInputScope"] = actual.StageInputScope,
            ["stageConfigValue"] = actual.StageConfigValue,
            ["stageResolvedPath"] = actual.StageResolvedPath,
            ["stageSelectedFileExists"] = actual.StageSelectedFileExists,
            ["stageParseStatus"] = actual.StageParseStatus,
            ["stageUnverified"] = actual.StageUnverified,
            ["stageSourceSha256"] = actual.StageSourceSha256,
            ["sourceSha256"] = sourceHashes,
            ["datIdentities"] = datIdentities,
            ["parseFailures"] = actual.ParseFailures,
            ["boundary"] = "Fresh current catalog/DAT parse plus production selector proof; no Unity, mobile, global combo-max, or runtime claim."
        };
    }

    private static object CompareWithOldResult(ActualRun actual, BattleImageLoadPlan current)
    {
        using JsonDocument oldResult = JsonDocument.Parse(File.OpenRead(actual.OldResultPath));
        JsonElement set = oldResult.RootElement
            .GetProperty("sets")
            .GetProperty("role_only_static_superset");
        int[] oldIds = set.GetProperty("oids").EnumerateArray().Select(value => value.GetInt32()).ToArray();
        var oldSet = new HashSet<int>(oldIds);
        var currentSet = new HashSet<int>(current.DefinitionIds);
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["oldResultPath"] = actual.OldResultPath,
            ["oldResultSha256"] = actual.OldResultSha256,
            ["oldSet"] = "role_only_static_superset",
            ["oldRootOids"] = set.GetProperty("rootOids").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
            ["oldDefinitionIds"] = oldIds,
            ["currentDefinitionIds"] = current.DefinitionIds,
            ["oldOnlyDefinitionIds"] = oldIds.Where(id => !currentSet.Contains(id)).ToArray(),
            ["currentOnlyDefinitionIds"] = current.DefinitionIds.Where(id => !oldSet.Contains(id)).ToArray(),
            ["oldEdgeCount"] = set.GetProperty("edges").GetArrayLength(),
            ["currentEdgeCount"] = current.Edges.Count,
            ["oldUnknownCount"] = set.GetProperty("unknowns").GetArrayLength(),
            ["currentMissingDependencyCount"] = current.MissingDependencies.Count,
            ["interpretation"] = "Independent comparison only; the old audit edge schema and OPoint admission rules remain historical evidence."
        };
    }

    private static int CountSynthetic(IReadOnlyList<object> results, string status)
    {
        int count = 0;
        foreach (object item in results)
        {
            if (item is Dictionary<string, object> dictionary &&
                string.Equals(dictionary["status"]?.ToString(), status, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void EnsureFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            throw new FileNotFoundException("Required input file is missing.", path);
    }

    private static string HashFile(string path)
    {
        EnsureFile(path);
        using FileStream stream = File.OpenRead(path);
        using SHA256 algorithm = SHA256.Create();
        return Convert.ToHexString(algorithm.ComputeHash(stream));
    }

    private static void WriteJson(string outputRoot, string name, object value)
    {
        string path = Path.Combine(outputRoot, name);
        using FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, value, JsonOptions);
        stream.Flush(true);
    }
}

namespace NTSD.Animation
{
    // Data-only diagnostics: production memory arithmetic receives the verified file facts from Python.
    internal static class BattleAtlasLayoutPlanner { internal const int PageSize = 2048; }
    internal static class BMPLoader
    {
        internal readonly struct VerifiedImageLayout
        {
            internal readonly long FileBytes;
            internal readonly int Width, Height;
            internal readonly bool IsPng;
            internal VerifiedImageLayout(string path, string hash, long bytes, int width, int height, bool png)
            { FileBytes = bytes; Width = width; Height = height; IsPng = png; }
        }
    }
}
