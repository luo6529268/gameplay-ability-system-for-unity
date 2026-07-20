---
provider: "codex"
agent_role: "architect"
model: "gpt-5.6-sol"
files:
  - "Tools/NTSDParity/README.md"
  - "Tools/NTSDParity/AuthorityTraceCommand.cs"
  - "Tools/NTSDParity/TraceCompareCommand.cs"
  - "Tools/NTSDParity/DataAuditCommand.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs"
  - "Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs"
  - "Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs"
  - "Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs"
  - "Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md"
timestamp: "2026-07-17T03:14:35.320Z"
---

--- File: Tools/NTSDParity/README.md ---
# NTSD C# / Unity battle parity tools

`NTSDParity` references the formal C# authority project at
`J:\QQFile\NTSD2.4\ntsd_release_C#`. It reads that project through its public
runtime and DAT APIs; it never modifies authority source files.

Build:

```powershell
dotnet build Tools/NTSDParity/NTSDParity.csproj
```

## Resolved DAT audit

Both sides are parsed by the authority `DatLoader`. The comparison includes
all parsed combat fields, sprite dimensions/ranges, sound cues, and the
resolved frame table. Duplicate frame ids follow the actual runtime rule:
the last parsed frame wins. Pure sprite asset paths are normalized across the
two repository layouts; sound cues stay in the comparison under their logical
basename (`snddata_1877.wav` and `1877.wav` are the same cue).
Per-OID duplicate frame-id lists remain in the report for diagnosis even
though only each side's last resolved definition participates in comparison.

```powershell
dotnet run --project Tools/NTSDParity/NTSDParity.csproj -- data-audit `
  --output Temp/NTSDParity/data-audit-full.json
```

Use repeatable `--oid <id>` options for a focused audit. The report records
`missing-unity-file` separately from an index mismatch or parser error, and
contains stable authority and Unity SHA-256 normalized manifest hashes. Full,
battle-logic, and presentation manifests are reported separately.

## Deterministic authority trace

```powershell
dotnet run --project Tools/NTSDParity/NTSDParity.csproj -- trace-authority `
  --scenario Tools/NTSDParity/scenario.sample.json `
  --output Temp/NTSDParity/authority-trace.jsonl
```

The header embeds only the normalized battle-logic DAT manifest hash, so asset
deployment paths and sound naming cannot block runtime comparison. Each tick has independent
hashes for input, RNG, world state, all fixed runtime slots, ARest, VRest,
stats, and events, plus an overall hash. Compact output writes only active or
non-default slots; hashes still cover every fixed slot. Use `--detail full`
to emit every slot and full rest matrices for field-level diagnosis.

`stage.dat` is not loaded by default. A scenario may opt in with an explicit
`stageFixture` path; its payload hash is then written to the trace header.

Button masks are `Right=1`, `Left=2`, `Up=4`, `Down=8`, `Attack=16`,
`Jump=32`, and `Defend=64`.

## Streaming trace comparison

```powershell
dotnet run --project Tools/NTSDParity/NTSDParity.csproj -- compare `
  --authority Temp/NTSDParity/authority-trace.jsonl `
  --unity Temp/NTSDParity/unity-trace.jsonl `
  --output Temp/NTSDParity/first-difference.json
```

The comparator reads one line from each trace at a time. It first validates
the schema, manifest, scenario, and explicit stage fixture, then compares
every tick domain hash including RNG and events. `--detail full` includes up
to 512 field-level differences from the first divergent line. Generate both
input traces in full detail when a complete fixed-slot field diff is needed.


--- File: Tools/NTSDParity/AuthorityTraceCommand.cs ---
using System.Reflection;
using System.Text;
using System.Text.Json;
using NtsdReleaseCSharp.App;
using NtsdReleaseCSharp.BattleCore.Common;
using NtsdReleaseCSharp.BattleCore.Entities;
using NtsdReleaseCSharp.BattleCore.Lockstep;
using NtsdReleaseCSharp.BattleCore.Runtime;
using NtsdReleaseCSharp.BattleCore.Simulation;
using NtsdReleaseCSharp.Data;

namespace NTSDParity;

internal static class AuthorityTraceCommand
{
    private static readonly HashSet<string> WorldDomainExclusions = new(StringComparer.Ordinal)
    {
        "Objects",
        "VRest",
        "ARest",
        "CharData",
        "LoadedOidOrder",
        "CharCount",
        "RuntimeStageCount",
        "StageCampaigns",
        "BattleSlotLabels",
        "BattleSlotLabelState",
        "Bg",
        "KillStats",
        "DamageStats",
        "PendingSounds",
        "Results",
    };

    private static readonly HashSet<string> TracePathMembers = new(StringComparer.Ordinal)
    {
        "BgData.ShadowPath",
        "BgLayer.BmpPath",
        "PendingSoundEvent.Cue",
    };

    public static int Run(string[] args)
    {
        CommandLine cli = CommandLine.Parse(args);
        string scenarioPath = Path.GetFullPath(cli.Require("--scenario"));
        string outputPath = RepositoryPaths.ResolveOutput(cli.Require("--output"));
        string detail = cli.Get("--detail") ?? "compact";
        if (detail is not ("compact" or "full"))
            throw new ArgumentException("--detail must be 'compact' or 'full'.");

        AuthorityScenario scenario = LoadScenario(scenarioPath);
        ValidateScenario(scenario);

        NtsdRng.Srand(scenario.Seed);
        GameWorld world = new();
        RuntimeBootstrap bootstrap = new(scenario.GameRoot);
        int loadedChars = bootstrap.LoadAllChars(world);
        if (loadedChars <= 0)
            throw new InvalidOperationException($"No DAT objects loaded from '{scenario.GameRoot}'.");

        world.RuntimeStageCount = bootstrap.LoadBackgroundIndex().Count;
        StageFixtureInfo stageFixture = LoadExplicitStageFixture(world, scenarioPath, scenario.StageFixture);
        if (!bootstrap.LoadStageBgByIndex(world, scenario.Stage))
        {
            world.Bg.Width = 800;
            world.Bg.ZBoundaryMin = 180;
            world.Bg.ZBoundaryMax = 350;
        }

        BattleBootstrapConfig bootstrapConfig = BuildBootstrapConfig(scenario);
        DirectBattleBootstrap.InitializeFromConfig(world, bootstrapConfig);
        CharacterSync.SyncRuntimeFromLegacy(world);

        ScenarioInputProvider inputProvider = new(scenario.Inputs);
        LockstepSimulationSettings settings = new()
        {
            DriveMode = SimulationDriveMode.Manual,
            EnableFrameChecksum = true,
        };
        SimulationTickDriver driver = new(new BattleTickScheduler(), settings);
        driver.SetFrameInputProvider(inputProvider);

        string manifestSha256 = DataAuditCommand.ComputeLoadedBattleLogicManifestSha256(world);
        object?[] defaultSlots = BuildDefaultSlots(world.Objects.Length);
        using StreamWriter writer = new(outputPath, false, new UTF8Encoding(false));
        WriteJsonLine(
            writer,
            BuildHeader(scenarioPath, scenario, world, loadedChars, manifestSha256, stageFixture, detail));

        for (int tick = 1; tick <= scenario.Ticks; tick++)
        {
            int previousTick = world.GameTick;
            driver.StepOneTick(world);
            if (world.GameTick != previousTick + 1)
                throw new InvalidOperationException($"Authority simulation did not advance tick {tick}.");

            CharacterSync.SyncRuntimeFromLegacy(world);
            WriteJsonLine(writer, BuildTick(world, inputProvider.GetFrameInput(tick), defaultSlots, detail));
        }

        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"ticks={scenario.Ticks} loadedChars={loadedChars} manifest={manifestSha256} " +
            $"finalRngSeed={NtsdRng.Seed} rngCalls={NtsdRng.CallCount}");
        return 0;
    }

    private static AuthorityScenario LoadScenario(string path)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        AuthorityScenario? scenario = JsonSerializer.Deserialize<AuthorityScenario>(File.ReadAllText(path), options);
        return scenario ?? throw new InvalidDataException("Scenario JSON deserialized to null.");
    }

    private static void ValidateScenario(AuthorityScenario scenario)
    {
        if (scenario.Ticks <= 0)
            throw new ArgumentException("Scenario ticks must be positive.");
        if (string.IsNullOrWhiteSpace(scenario.GameRoot))
            throw new ArgumentException("Scenario gameRoot is required.");
        foreach (ScenarioSlot slot in scenario.Slots)
        {
            if (slot.PlayerSlot is < 0 or >= 8)
                throw new ArgumentException($"Scenario player slot {slot.PlayerSlot} is outside 0..7.");
        }
        foreach (ScenarioTickInput input in scenario.Inputs)
        {
            if (input.Tick <= 0 || input.Tick > scenario.Ticks)
                throw new ArgumentException($"Scenario input tick {input.Tick} is outside 1..{scenario.Ticks}.");
        }
    }

    private static StageFixtureInfo LoadExplicitStageFixture(
        GameWorld world,
        string scenarioPath,
        string? configuredPath)
    {
        world.StageCampaigns.Clear();
        if (string.IsNullOrWhiteSpace(configuredPath))
            return new StageFixtureInfo { Loaded = false };

        string path = Path.IsPathRooted(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scenarioPath)!, configuredPath));
        byte[] payload = DataAuditCommand.LoadDatPayload(path, forceDecrypt: false);
        List<StageCampaignData> campaigns = DatLoader.ParseStageCampaign(payload);
        world.StageCampaigns.AddRange(campaigns);
        return new StageFixtureInfo
        {
            Loaded = true,
            Name = Path.GetFileName(path),
            Sha256 = CanonicalJson.Sha256Bytes(payload),
            CampaignCount = campaigns.Count,
        };
    }

    private static BattleBootstrapConfig BuildBootstrapConfig(AuthorityScenario scenario)
    {
        BattleBootstrapConfig config = new()
        {
            GameMode = scenario.Mode,
            Difficulty = scenario.Difficulty,
            StageIdx = scenario.Stage,
            RandomStage = scenario.RandomStage,
        };
        foreach (ScenarioSlot source in scenario.Slots)
        {
            BattleSlotConfig target = config.Slots[source.PlayerSlot];
            target.Oid = source.Oid;
            target.Team = source.Team;
            target.Active = source.Active;
            target.Ai = source.Ai;
        }
        return config;
    }

    private static object BuildHeader(
        string scenarioPath,
        AuthorityScenario scenario,
        GameWorld world,
        int loadedChars,
        string manifestSha256,
        StageFixtureInfo stageFixture,
        string detail)
    {
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["kind"] = "header",
            ["schema"] = "ntsd-battle-trace-v2",
            ["scenarioName"] = Path.GetFileName(scenarioPath),
            ["scenario"] = ProjectScenario(scenario),
            ["loadedChars"] = loadedChars,
            ["maxRuntimeSlots"] = world.Objects.Length,
            ["manifest"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["schema"] = "ntsd-resolved-dat-manifest-v2",
                ["domain"] = "battle-logic",
                ["battleLogicSha256"] = manifestSha256,
            },
            ["stageFixture"] = stageFixture,
            ["rngAfterBootstrap"] = new { seed = NtsdRng.Seed, callCount = NtsdRng.CallCount },
            ["buttonMask"] = new SortedDictionary<string, int>(StringComparer.Ordinal)
            {
                ["right"] = (int)SimulationInputButtons.Right,
                ["left"] = (int)SimulationInputButtons.Left,
                ["up"] = (int)SimulationInputButtons.Up,
                ["down"] = (int)SimulationInputButtons.Down,
                ["attack"] = (int)SimulationInputButtons.Attack,
                ["jump"] = (int)SimulationInputButtons.Jump,
                ["defend"] = (int)SimulationInputButtons.Defend,
            },
            ["detail"] = detail,
        };
    }

    private static object ProjectScenario(AuthorityScenario scenario)
        => new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["seed"] = scenario.Seed,
            ["mode"] = scenario.Mode,
            ["difficulty"] = scenario.Difficulty,
            ["stage"] = scenario.Stage,
            ["randomStage"] = scenario.RandomStage,
            ["ticks"] = scenario.Ticks,
            ["slots"] = JsonProjection.Project(scenario.Slots),
            ["inputs"] = JsonProjection.Project(scenario.Inputs),
        };

    private static object BuildTick(
        GameWorld world,
        SimulationFrameInput input,
        object?[] defaultSlots,
        string detail)
    {
        object inputDomain = JsonProjection.Project(input)!;
        object rngDomain = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["seed"] = NtsdRng.Seed,
            ["callCount"] = NtsdRng.CallCount,
        };
        object worldDomain = ProjectWorldDomain(world);
        object?[] allSlots = ProjectAllSlots(world);
        object aRestDomain = ProjectARest(world, full: false);
        object vRestDomain = ProjectVRest(world, full: false);
        object statsDomain = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["kill"] = world.KillStats.ToArray(),
            ["damage"] = world.DamageStats.ToArray(),
        };
        object eventsDomain = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["pendingSounds"] = JsonProjection.Project(world.PendingSounds, normalizedPathMembers: TracePathMembers),
        };

        SortedDictionary<string, string> hashes = new(StringComparer.Ordinal)
        {
            ["input"] = CanonicalJson.Sha256(inputDomain),
            ["rng"] = CanonicalJson.Sha256(rngDomain),
            ["world"] = CanonicalJson.Sha256(worldDomain),
            ["slots"] = CanonicalJson.Sha256(allSlots),
            ["aRest"] = CanonicalJson.Sha256(aRestDomain),
            ["vRest"] = CanonicalJson.Sha256(vRestDomain),
            ["stats"] = CanonicalJson.Sha256(statsDomain),
            ["events"] = CanonicalJson.Sha256(eventsDomain),
        };
        hashes["overall"] = CanonicalJson.Sha256(hashes);

        object?[] outputSlots = detail == "full"
            ? allSlots
            : allSlots.Where((slot, index) => !CanonicalEquivalent(slot, defaultSlots[index])).ToArray();

        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["kind"] = "tick",
            ["tick"] = world.GameTick,
            ["hashes"] = hashes,
            ["input"] = inputDomain,
            ["rng"] = rngDomain,
            ["world"] = worldDomain,
            ["objectCount"] = world.ObjectCount,
            ["slots"] = outputSlots,
            ["aRest"] = detail == "full" ? ProjectARest(world, full: true) : aRestDomain,
            ["vRest"] = detail == "full" ? ProjectVRest(world, full: true) : vRestDomain,
            ["stats"] = statsDomain,
            ["events"] = eventsDomain,
        };
    }

    private static object ProjectWorldDomain(GameWorld world)
    {
        SortedDictionary<string, object?> result = new(StringComparer.Ordinal);
        Type type = world.GetType();
        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(value => value.Name, StringComparer.Ordinal))
        {
            if (!WorldDomainExclusions.Contains(field.Name))
                result[field.Name] = JsonProjection.Project(field.GetValue(world), normalizedPathMembers: TracePathMembers);
        }
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(value => value.Name, StringComparer.Ordinal))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0 ||
                WorldDomainExclusions.Contains(property.Name) || result.ContainsKey(property.Name))
            {
                continue;
            }
            result[property.Name] = JsonProjection.Project(property.GetValue(world), normalizedPathMembers: TracePathMembers);
        }
        result["Results"] = ProjectBattleResults(world.Results);
        return result;
    }

    private static object ProjectBattleResults(ResultsState results)
        => new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["phase"] = results.Phase,
            ["timer"] = results.Timer,
            ["winner"] = results.Winner,
            ["hadBoth"] = results.HadBoth,
            ["battleEndPhase"] = results.BattleEndPhase,
            ["pendingWinner"] = results.PendingWinner,
            ["teamCount"] = results.TeamCount,
            ["teamIds"] = results.TeamIds.ToArray(),
            ["pendingHostAction"] = results.PendingHostAction,
        };

    private static object?[] ProjectAllSlots(GameWorld world)
    {
        object?[] result = new object?[world.Objects.Length];
        for (int i = 0; i < world.Objects.Length; i++)
            result[i] = ProjectSlot(world.Objects[i], i);
        return result;
    }

    private static object?[] BuildDefaultSlots(int count)
    {
        object?[] result = new object?[count];
        for (int i = 0; i < count; i++)
        {
            Entity entity = new();
            entity.Reset();
            entity.Slot = i;
            entity.Runtime.CopyFrom(entity);
            result[i] = ProjectSlot(entity, i);
        }
        return result;
    }

    private static object ProjectSlot(Entity entity, int slot)
        => new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["runtimeSlot"] = slot,
            ["currentDataOid"] = entity.CharData?.Oid,
            ["runtime"] = JsonProjection.Project(entity.Runtime),
        };

    private static bool CanonicalEquivalent(object? left, object? right)
        => string.Equals(CanonicalJson.Sha256(left), CanonicalJson.Sha256(right), StringComparison.Ordinal);

    private static object ProjectARest(GameWorld world, bool full)
    {
        if (full)
        {
            return new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["dimension"] = world.Objects.Length,
                ["encoding"] = "full",
                ["values"] = world.ARest.ToArray(),
            };
        }

        List<object> entries = [];
        for (int slot = 0; slot < world.ARest.Length; slot++)
        {
            if (world.ARest[slot] != 0)
                entries.Add(new { slot, value = world.ARest[slot] });
        }
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = world.Objects.Length,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = entries,
        };
    }

    private static object ProjectVRest(GameWorld world, bool full)
    {
        if (full)
        {
            int[][] rows = new int[world.Objects.Length][];
            for (int attacker = 0; attacker < world.Objects.Length; attacker++)
            {
                rows[attacker] = new int[world.Objects.Length];
                for (int victim = 0; victim < world.Objects.Length; victim++)
                    rows[attacker][victim] = world.VRest[attacker, victim];
            }
            return new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["dimension"] = world.Objects.Length,
                ["encoding"] = "full-row-major",
                ["values"] = rows,
            };
        }

        List<object> entries = [];
        for (int attacker = 0; attacker < world.Objects.Length; attacker++)
        {
            for (int victim = 0; victim < world.Objects.Length; victim++)
            {
                int value = world.VRest[attacker, victim];
                if (value != 0)
                    entries.Add(new { attackerSlot = attacker, victimSlot = victim, value });
            }
        }
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = world.Objects.Length,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = entries,
        };
    }

    private static void WriteJsonLine(StreamWriter writer, object value)
    {
        writer.WriteLine(JsonSerializer.Serialize(value, CanonicalJson.CompactOptions));
        writer.Flush();
    }

    private sealed class ScenarioInputProvider : ISimulationFrameInputProvider
    {
        private readonly Dictionary<int, SimulationFrameInput> inputs;

        public ScenarioInputProvider(IEnumerable<ScenarioTickInput> source)
        {
            inputs = source
                .GroupBy(item => item.Tick)
                .ToDictionary(
                    group => group.Key,
                    group => new SimulationFrameInput
                    {
                        TickIndex = group.Key,
                        Players = group.SelectMany(item => item.Players)
                            .Select(player => new SimulationPlayerInput(
                                player.PlayerSlot,
                                (SimulationInputButtons)player.ButtonMask))
                            .ToArray(),
                    });
        }

        public bool IsFrameInputReady(int tickIndex) => true;

        public SimulationFrameInput GetFrameInput(int tickIndex)
            => inputs.TryGetValue(tickIndex, out SimulationFrameInput? input)
                ? input
                : SimulationFrameInput.Empty(tickIndex);
    }

    private sealed class StageFixtureInfo
    {
        public bool Loaded { get; set; }
        public string? Name { get; set; }
        public string? Sha256 { get; set; }
        public int CampaignCount { get; set; }
    }
}

internal sealed class AuthorityScenario
{
    public uint Seed { get; set; }
    public string GameRoot { get; set; } = @"J:\QQFile\NTSD2.4";
    public int Mode { get; set; } = 1;
    public int Difficulty { get; set; } = 1;
    public int Stage { get; set; }
    public int RandomStage { get; set; }
    public string? StageFixture { get; set; }
    public int Ticks { get; set; }
    public List<ScenarioSlot> Slots { get; set; } = [];
    public List<ScenarioTickInput> Inputs { get; set; } = [];
}

internal sealed class ScenarioSlot
{
    public int PlayerSlot { get; set; }
    public int Oid { get; set; }
    public int Team { get; set; } = 1;
    public bool Active { get; set; } = true;
    public bool Ai { get; set; }
}

internal sealed class ScenarioTickInput
{
    public int Tick { get; set; }
    public List<ScenarioPlayerInput> Players { get; set; } = [];
}

internal sealed class ScenarioPlayerInput
{
    public int PlayerSlot { get; set; }
    public int ButtonMask { get; set; }
}


--- File: Tools/NTSDParity/TraceCompareCommand.cs ---
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSDParity;

internal static class TraceCompareCommand
{
    private static readonly string[] RequiredHashDomains =
    [
        "overall",
        "input",
        "rng",
        "world",
        "slots",
        "aRest",
        "vRest",
        "stats",
        "events",
    ];

    public static int Run(string[] args)
    {
        CommandLine cli = CommandLine.Parse(args);
        string authorityPath = Path.GetFullPath(cli.Require("--authority"));
        string unityPath = Path.GetFullPath(cli.Require("--unity"));
        string outputPath = RepositoryPaths.ResolveOutput(cli.Require("--output"));
        string detail = cli.Get("--detail") ?? "hashes";
        if (detail is not ("hashes" or "full"))
            throw new ArgumentException("--detail must be 'hashes' or 'full'.");

        using StreamReader authority = new(authorityPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using StreamReader unity = new(unityPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        TraceCompareReport report = Compare(
            authority,
            unity,
            Path.GetFileName(authorityPath),
            Path.GetFileName(unityPath),
            detail == "full");
        File.WriteAllText(outputPath, JsonSerializer.Serialize(report, JsonProjection.SerializerOptions), new UTF8Encoding(false));
        Console.WriteLine(outputPath);
        Console.WriteLine($"status={report.Status} ticksCompared={report.TicksCompared} firstDifferenceTick={report.FirstDifference?.Tick}");
        return report.Status == "equal" ? 0 : 1;
    }

    private static TraceCompareReport Compare(
        StreamReader authorityReader,
        StreamReader unityReader,
        string authorityName,
        string unityName,
        bool fullDetail)
    {
        TraceCompareReport report = new()
        {
            Schema = "ntsd-streaming-trace-compare-v1",
            Authority = authorityName,
            Unity = unityName,
        };

        string? authorityHeaderLine = ReadNextLine(authorityReader);
        string? unityHeaderLine = ReadNextLine(unityReader);
        if (authorityHeaderLine is null || unityHeaderLine is null)
            return Fail(report, "header", 0, "missing-header", authorityHeaderLine, unityHeaderLine, fullDetail);

        using (JsonDocument authorityHeader = JsonDocument.Parse(authorityHeaderLine))
        using (JsonDocument unityHeader = JsonDocument.Parse(unityHeaderLine))
        {
            string? authoritySchema = ReadString(authorityHeader.RootElement, "schema");
            string? unitySchema = ReadString(unityHeader.RootElement, "schema");
            if (!string.Equals(authoritySchema, unitySchema, StringComparison.Ordinal))
                return Fail(report, "header", 0, "schema", authorityHeaderLine, unityHeaderLine, fullDetail);

            string? authorityManifest = ReadNestedString(authorityHeader.RootElement, "manifest", "battleLogicSha256");
            string? unityManifest = ReadNestedString(unityHeader.RootElement, "manifest", "battleLogicSha256");
            report.AuthorityManifestSha256 = authorityManifest;
            report.UnityManifestSha256 = unityManifest;
            if (string.IsNullOrEmpty(authorityManifest) ||
                !string.Equals(authorityManifest, unityManifest, StringComparison.Ordinal))
            {
                return Fail(report, "header", 0, "manifest", authorityHeaderLine, unityHeaderLine, fullDetail);
            }

            string authorityScenarioHash = CanonicalJson.Sha256(ReadNode(authorityHeader.RootElement, "scenario"));
            string unityScenarioHash = CanonicalJson.Sha256(ReadNode(unityHeader.RootElement, "scenario"));
            if (!string.Equals(authorityScenarioHash, unityScenarioHash, StringComparison.Ordinal))
                return Fail(report, "header", 0, "scenario", authorityHeaderLine, unityHeaderLine, fullDetail);

            string authorityFixtureHash = CanonicalJson.Sha256(ReadNode(authorityHeader.RootElement, "stageFixture"));
            string unityFixtureHash = CanonicalJson.Sha256(ReadNode(unityHeader.RootElement, "stageFixture"));
            if (!string.Equals(authorityFixtureHash, unityFixtureHash, StringComparison.Ordinal))
                return Fail(report, "header", 0, "stageFixture", authorityHeaderLine, unityHeaderLine, fullDetail);
        }

        while (true)
        {
            string? authorityLine = ReadNextLine(authorityReader);
            string? unityLine = ReadNextLine(unityReader);
            if (authorityLine is null || unityLine is null)
            {
                if (authorityLine is null && unityLine is null)
                {
                    report.Status = "equal";
                    return report;
                }
                return Fail(report, "stream", report.TicksCompared + 1, "trace-length", authorityLine, unityLine, fullDetail);
            }

            using JsonDocument authorityTick = JsonDocument.Parse(authorityLine);
            using JsonDocument unityTick = JsonDocument.Parse(unityLine);
            int authorityTickIndex = ReadInt(authorityTick.RootElement, "tick");
            int unityTickIndex = ReadInt(unityTick.RootElement, "tick");
            if (authorityTickIndex != unityTickIndex)
                return Fail(report, "tick", Math.Min(authorityTickIndex, unityTickIndex), "tick-index", authorityLine, unityLine, fullDetail);

            foreach (string domain in RequiredHashDomains)
            {
                string? authorityHash = ReadNestedString(authorityTick.RootElement, "hashes", domain);
                string? unityHash = ReadNestedString(unityTick.RootElement, "hashes", domain);
                if (string.IsNullOrEmpty(authorityHash) ||
                    !string.Equals(authorityHash, unityHash, StringComparison.Ordinal))
                {
                    return Fail(report, domain, authorityTickIndex, "domain-hash", authorityLine, unityLine, fullDetail);
                }
            }
            report.TicksCompared++;
        }
    }

    private static TraceCompareReport Fail(
        TraceCompareReport report,
        string domain,
        int tick,
        string reason,
        string? authorityLine,
        string? unityLine,
        bool fullDetail)
    {
        report.Status = "different";
        report.FirstDifference = new TraceDifference
        {
            Tick = tick,
            Domain = domain,
            Reason = reason,
        };

        if (fullDetail && authorityLine is not null && unityLine is not null)
        {
            JsonNode? authorityNode = JsonNode.Parse(authorityLine);
            JsonNode? unityNode = JsonNode.Parse(unityLine);
            CanonicalJson.CompareNodes(authorityNode, unityNode, "$", report.FirstDifference.Fields, limit: 512);
            report.FirstDifference.FieldDiffTruncated = report.FirstDifference.Fields.Count >= 512;
        }
        return report;
    }

    private static string? ReadNextLine(StreamReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }
        return null;
    }

    private static string? ReadString(JsonElement element, string property)
        => element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? ReadNestedString(JsonElement element, string parent, string property)
        => element.TryGetProperty(parent, out JsonElement nested) ? ReadString(nested, property) : null;

    private static int ReadInt(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out JsonElement value) || !value.TryGetInt32(out int result))
            throw new InvalidDataException($"Trace line is missing integer property '{property}'.");
        return result;
    }

    private static JsonNode? ReadNode(JsonElement element, string property)
        => element.TryGetProperty(property, out JsonElement value) ? JsonNode.Parse(value.GetRawText()) : null;

    private sealed class TraceCompareReport
    {
        public string Schema { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string Unity { get; set; } = string.Empty;
        public string? AuthorityManifestSha256 { get; set; }
        public string? UnityManifestSha256 { get; set; }
        public int TicksCompared { get; set; }
        public TraceDifference? FirstDifference { get; set; }
    }

    private sealed class TraceDifference
    {
        public int Tick { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool FieldDiffTruncated { get; set; }
        public List<FieldDifference> Fields { get; set; } = [];
    }
}


--- File: Tools/NTSDParity/DataAuditCommand.cs ---
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NtsdReleaseCSharp.BattleCore.Simulation;
using NtsdReleaseCSharp.Data;

namespace NTSDParity;

internal static class DataAuditCommand
{
    private const string DefaultAuthorityRoot = @"J:\QQFile\NTSD2.4";

    private static readonly HashSet<string> ProjectionExclusions = new(StringComparer.Ordinal)
    {
        "CharData.Frames",
        "CharData.FrameIndex",
    };

    private static readonly HashSet<string> BattleLogicExclusions = new(ProjectionExclusions, StringComparer.Ordinal)
    {
        "CharData.Name",
        "CharData.HeadFile",
        "CharData.SmallFile",
        "CharData.SpriteFile",
        "CharData.SpriteW",
        "CharData.SpriteH",
        "CharData.SpriteRow",
        "CharData.SpriteCol",
        "CharData.SpriteRanges",
        "CharData.WeaponHitSound",
        "CharData.WeaponDropSound",
        "CharData.WeaponBrokenSound",
        "FrameData.Pic",
        "FrameData.CenterX",
        "FrameData.CenterY",
        "FrameData.Sound",
    };

    private static readonly HashSet<string> NormalizedPathMembers = new(StringComparer.Ordinal)
    {
        "CharData.HeadFile",
        "CharData.SmallFile",
        "CharData.SpriteFile",
        "CharData.WeaponHitSound",
        "CharData.WeaponDropSound",
        "CharData.WeaponBrokenSound",
        "SpriteRange.File",
        "FrameData.Sound",
    };

    public static int Run(string[] args)
    {
        CommandLine cli = CommandLine.Parse(args);
        string authorityRoot = Path.GetFullPath(cli.Get("--authority-root") ?? DefaultAuthorityRoot);
        string unityRoot = RepositoryPaths.FindUnityRoot(cli.Get("--unity-root"));
        string authorityIndex = Path.GetFullPath(cli.Get("--authority-index") ?? Path.Combine(authorityRoot, "data", "data.txt"));
        string unityIndex = Path.GetFullPath(cli.Get("--unity-index") ?? Path.Combine(unityRoot, "Assets", "NTSD", "Config", "data.txt"));
        string output = RepositoryPaths.ResolveOutput(cli.Get("--output") ?? "data-audit-report.json");

        HashSet<int> requestedOids = ParseRequestedOids(cli.GetAll("--oid"));
        List<OidEntry> authorityEntries = DatLoader.ParseDataTxt(authorityIndex);
        List<OidEntry> unityEntries = DatLoader.ParseDataTxt(unityIndex);
        Dictionary<int, OidEntry> authorityByOid = FirstByOid(authorityEntries);
        Dictionary<int, OidEntry> unityByOid = FirstByOid(unityEntries);
        int[] oids = authorityByOid.Keys
            .Union(unityByOid.Keys)
            .Where(oid => requestedOids.Count == 0 || requestedOids.Contains(oid))
            .OrderBy(oid => oid)
            .ToArray();

        ManifestSet authorityManifest = BuildManifest(
            authorityByOid.Values.Where(entry => requestedOids.Count == 0 || requestedOids.Contains(entry.Oid)),
            entry => DataPaths.ToAbsolutePath(authorityRoot, entry.File),
            forceDecrypt: true);
        ManifestSet unityManifest = BuildManifest(
            unityByOid.Values.Where(entry => requestedOids.Count == 0 || requestedOids.Contains(entry.Oid)),
            entry => ResolveUnityPath(unityRoot, entry.File),
            forceDecrypt: false);
        List<OidAuditResult> results = [];
        foreach (int oid in oids)
        {
            authorityByOid.TryGetValue(oid, out OidEntry? authorityEntry);
            unityByOid.TryGetValue(oid, out OidEntry? unityEntry);
            results.Add(AuditOid(
                oid,
                authorityRoot,
                unityRoot,
                authorityEntry,
                unityEntry));
        }

        ManifestSummary manifest = new()
        {
            Schema = "ntsd-resolved-dat-manifest-v1",
            AuthoritySha256 = CanonicalJson.Sha256(authorityManifest.Full),
            UnitySha256 = CanonicalJson.Sha256(unityManifest.Full),
            AuthorityBattleLogicSha256 = CanonicalJson.Sha256(authorityManifest.BattleLogic),
            UnityBattleLogicSha256 = CanonicalJson.Sha256(unityManifest.BattleLogic),
            AuthorityPresentationSha256 = CanonicalJson.Sha256(authorityManifest.Presentation),
            UnityPresentationSha256 = CanonicalJson.Sha256(unityManifest.Presentation),
            AuthorityEntries = BuildManifestDigests(authorityManifest.Full),
            UnityEntries = BuildManifestDigests(unityManifest.Full),
        };
        manifest.Equal = string.Equals(manifest.AuthoritySha256, manifest.UnitySha256, StringComparison.Ordinal);
        manifest.BattleLogicEqual = string.Equals(
            manifest.AuthorityBattleLogicSha256,
            manifest.UnityBattleLogicSha256,
            StringComparison.Ordinal);
        manifest.PresentationEqual = string.Equals(
            manifest.AuthorityPresentationSha256,
            manifest.UnityPresentationSha256,
            StringComparison.Ordinal);

        DataAuditReport report = new()
        {
            Schema = "ntsd-data-audit-v2",
            RequestedOids = requestedOids.OrderBy(value => value).ToArray(),
            NormalizedPathMembers = NormalizedPathMembers.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            DuplicateAuthorityOids = DuplicateOids(authorityEntries),
            DuplicateUnityOids = DuplicateOids(unityEntries),
            Manifest = manifest,
            Results = results,
        };
        report.Summary = BuildSummary(results);
        report.DifferenceCategories = BuildDifferenceCategories(results);

        File.WriteAllText(output, JsonSerializer.Serialize(report, JsonProjection.SerializerOptions), new UTF8Encoding(false));
        Console.WriteLine(output);
        Console.WriteLine(
            $"audited={report.Summary.Audited} equal={report.Summary.Equal} different={report.Summary.Different} " +
            $"missingAuthority={report.Summary.MissingAuthority} missingUnity={report.Summary.MissingUnity} errors={report.Summary.Errors}");
        Console.WriteLine($"authorityManifest={manifest.AuthoritySha256} unityManifest={manifest.UnitySha256}");
        Console.WriteLine(
            $"authorityBattleLogicManifest={manifest.AuthorityBattleLogicSha256} " +
            $"unityBattleLogicManifest={manifest.UnityBattleLogicSha256}");
        return report.Summary.Errors == 0 ? 0 : 1;
    }

    public static object ProjectResolvedCharData(CharData data)
        => ProjectResolvedCharData(data, ProjectionExclusions);

    public static object ProjectBattleLogicCharData(CharData data)
        => ProjectResolvedCharData(data, BattleLogicExclusions);

    private static object ProjectResolvedCharData(CharData data, ISet<string> exclusions)
    {
        object? baseProjection = JsonProjection.Project(data, exclusions, normalizedPathMembers: NormalizedPathMembers);
        if (baseProjection is not SortedDictionary<string, object?> projected)
            throw new InvalidOperationException("CharData projection did not produce an object.");

        SortedDictionary<string, object?> resolvedFrames = new(StringComparer.Ordinal);
        for (int frameId = 0; frameId < CharData.MaxFrameId; frameId++)
        {
            if (!data.HasFrame(frameId))
                continue;

            FrameData frame = data.GetFrameOrNull(frameId)
                ?? throw new InvalidOperationException($"Resolved frame {frameId} unexpectedly returned null.");
            resolvedFrames[frameId.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                JsonProjection.Project(frame, exclusions, normalizedPathMembers: NormalizedPathMembers);
        }

        projected["ResolvedFrameCount"] = resolvedFrames.Count;
        projected["ResolvedFramesById"] = resolvedFrames;
        return projected;
    }

    public static string ComputeLoadedBattleLogicManifestSha256(GameWorld world)
    {
        SortedDictionary<string, object?> entries = new(StringComparer.Ordinal);
        foreach (CharData data in world.CharData.Where(value => value is not null).Cast<CharData>().OrderBy(value => value.Oid))
            entries[FormatOid(data.Oid)] = BuildManifestEntry(data.ObjType, ProjectBattleLogicCharData(data));
        return CanonicalJson.Sha256(entries);
    }

    internal static byte[] LoadDatPayload(string path, bool forceDecrypt)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("DAT file not found.", path);

        byte[] bytes = File.ReadAllBytes(path);
        bool encrypted = forceDecrypt || LooksEncrypted(bytes);
        byte[] payload = encrypted ? DatLoader.DatDecrypt(path) : bytes;
        if (payload.Length == 0)
            throw new InvalidDataException("DAT payload is empty after loading/decryption.");
        return payload;
    }

    private static OidAuditResult AuditOid(
        int oid,
        string authorityRoot,
        string unityRoot,
        OidEntry? authorityEntry,
        OidEntry? unityEntry)
    {
        OidAuditResult result = new() { Oid = oid };
        if (authorityEntry is null)
        {
            result.Status = "missing-authority-index";
            result.UnityPath = unityEntry is null ? null : NormalizeReportPath(unityEntry.File);
            return result;
        }
        if (unityEntry is null)
        {
            result.Status = "missing-unity-index";
            result.AuthorityPath = NormalizeReportPath(authorityEntry.File);
            return result;
        }

        string authorityPath = DataPaths.ToAbsolutePath(authorityRoot, authorityEntry.File);
        string unityPath = ResolveUnityPath(unityRoot, unityEntry.File);
        result.AuthorityPath = NormalizeReportPath(authorityEntry.File);
        result.UnityPath = NormalizeReportPath(unityEntry.File);
        result.AuthorityType = authorityEntry.Type;
        result.UnityType = unityEntry.Type;

        if (!File.Exists(authorityPath))
        {
            result.Status = "missing-authority-file";
            return result;
        }
        if (!File.Exists(unityPath))
        {
            result.Status = "missing-unity-file";
            return result;
        }

        try
        {
            CharData authorityData = ParseDat(authorityPath, authorityEntry, forceDecrypt: true, out string authorityInputKind);
            CharData unityData = ParseDat(unityPath, unityEntry, forceDecrypt: false, out string unityInputKind);
            result.AuthorityInputKind = authorityInputKind;
            result.UnityInputKind = unityInputKind;
            result.DuplicateAuthorityFrameIds = DuplicateFrameIds(authorityData);
            result.DuplicateUnityFrameIds = DuplicateFrameIds(unityData);

            object authorityProjected = ProjectResolvedCharData(authorityData);
            object unityProjected = ProjectResolvedCharData(unityData);
            CanonicalJson.CompareNodes(
                JsonProjection.ToNode(authorityProjected),
                JsonProjection.ToNode(unityProjected),
                "$",
                result.Differences);
            if (authorityEntry.Type != unityEntry.Type)
            {
                result.Differences.Insert(0, new FieldDifference
                {
                    Path = "$.ObjType",
                    Authority = authorityEntry.Type.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Unity = unityEntry.Type.ToString(System.Globalization.CultureInfo.InvariantCulture),
                });
            }
            result.Status = result.Differences.Count == 0 ? "equal" : "different";
        }
        catch (Exception ex)
        {
            result.Status = "error";
            result.Error = ex.Message;
        }

        return result;
    }

    private static CharData ParseDat(string path, OidEntry entry, bool forceDecrypt, out string inputKind)
    {
        byte[] original = File.ReadAllBytes(path);
        bool encrypted = forceDecrypt || LooksEncrypted(original);
        byte[] payload = encrypted ? DatLoader.DatDecrypt(path) : original;
        inputKind = encrypted ? "encrypted" : "plaintext";
        if (payload.Length == 0)
            throw new InvalidDataException("DAT payload is empty after loading/decryption.");

        CharData data = new();
        if (!DatLoader.ParseCharData(payload, data))
            throw new InvalidDataException("Authority DatLoader.ParseCharData returned false.");
        data.Oid = entry.Oid;
        data.ObjType = entry.Type;
        return data;
    }

    private static bool LooksEncrypted(byte[] bytes)
    {
        if (bytes.Length <= 123)
            return false;
        int zeroCount = 0;
        for (int i = 0; i < 123; i++)
        {
            if (bytes[i] == 0)
                zeroCount++;
        }
        return zeroCount >= 120;
    }

    private static string ResolveUnityPath(string unityRoot, string indexedPath)
    {
        string normalized = indexedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        if (normalized.StartsWith("Assets" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return Path.GetFullPath(Path.Combine(unityRoot, normalized));
        return Path.GetFullPath(Path.Combine(unityRoot, "Assets", "NTSD", "Config", normalized));
    }

    private static string NormalizeReportPath(string value) => CanonicalJson.NormalizePath(value);

    private static string FormatOid(int oid)
        => oid.ToString("D6", System.Globalization.CultureInfo.InvariantCulture);

    private static ManifestSet BuildManifest(
        IEnumerable<OidEntry> entries,
        Func<OidEntry, string> resolvePath,
        bool forceDecrypt)
    {
        ManifestSet manifest = new();
        foreach (OidEntry entry in entries.OrderBy(value => value.Oid))
        {
            string path = resolvePath(entry);
            if (!File.Exists(path))
                continue;
            CharData data = ParseDat(path, entry, forceDecrypt, out _);
            string key = FormatOid(entry.Oid);
            manifest.Full[key] = BuildManifestEntry(entry.Type, ProjectResolvedCharData(data));
            manifest.BattleLogic[key] = BuildManifestEntry(entry.Type, ProjectBattleLogicCharData(data));
            manifest.Presentation[key] = BuildManifestEntry(entry.Type, ProjectPresentationCharData(data));
        }
        return manifest;
    }

    private static object ProjectPresentationCharData(CharData data)
    {
        SortedDictionary<string, object?> frames = new(StringComparer.Ordinal);
        for (int frameId = 0; frameId < CharData.MaxFrameId; frameId++)
        {
            if (!data.HasFrame(frameId))
                continue;
            FrameData frame = data.GetFrameOrNull(frameId)!;
            frames[frameId.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                new SortedDictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["pic"] = frame.Pic,
                    ["centerX"] = frame.CenterX,
                    ["centerY"] = frame.CenterY,
                    ["sound"] = CanonicalJson.NormalizePureAssetPath(frame.Sound),
                };
        }

        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["name"] = data.Name,
            ["headFile"] = CanonicalJson.NormalizePureAssetPath(data.HeadFile),
            ["smallFile"] = CanonicalJson.NormalizePureAssetPath(data.SmallFile),
            ["spriteFile"] = CanonicalJson.NormalizePureAssetPath(data.SpriteFile),
            ["spriteW"] = data.SpriteW,
            ["spriteH"] = data.SpriteH,
            ["spriteRow"] = data.SpriteRow,
            ["spriteCol"] = data.SpriteCol,
            ["spriteRanges"] = JsonProjection.Project(data.SpriteRanges, normalizedPathMembers: NormalizedPathMembers),
            ["weaponHitSound"] = CanonicalJson.NormalizePureAssetPath(data.WeaponHitSound),
            ["weaponDropSound"] = CanonicalJson.NormalizePureAssetPath(data.WeaponDropSound),
            ["weaponBrokenSound"] = CanonicalJson.NormalizePureAssetPath(data.WeaponBrokenSound),
            ["resolvedFramesById"] = frames,
        };
    }

    private static object BuildManifestEntry(int type, object data)
        => new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["type"] = type,
            ["data"] = data,
        };

    private static ManifestEntryDigest[] BuildManifestDigests(SortedDictionary<string, object?> manifest)
        => manifest.Select(pair => new ManifestEntryDigest
        {
            Oid = int.Parse(pair.Key, System.Globalization.CultureInfo.InvariantCulture),
            Sha256 = CanonicalJson.Sha256(pair.Value),
        }).ToArray();

    private static HashSet<int> ParseRequestedOids(IReadOnlyList<string> values)
    {
        HashSet<int> result = [];
        foreach (string value in values)
        {
            if (!int.TryParse(value, out int oid))
                throw new ArgumentException($"Invalid --oid value '{value}'.");
            result.Add(oid);
        }
        return result;
    }

    private static Dictionary<int, OidEntry> FirstByOid(IEnumerable<OidEntry> entries)
        => entries.GroupBy(entry => entry.Oid).ToDictionary(group => group.Key, group => group.First());

    private static int[] DuplicateOids(IEnumerable<OidEntry> entries)
        => entries.GroupBy(entry => entry.Oid).Where(group => group.Count() > 1).Select(group => group.Key).OrderBy(value => value).ToArray();

    private static int[] DuplicateFrameIds(CharData data)
        => data.Frames
            .GroupBy(frame => frame.FrameId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(value => value)
            .ToArray();

    private static DataAuditSummary BuildSummary(IEnumerable<OidAuditResult> results)
    {
        OidAuditResult[] array = results.ToArray();
        return new DataAuditSummary
        {
            Audited = array.Length,
            Equal = array.Count(result => result.Status == "equal"),
            Different = array.Count(result => result.Status == "different"),
            MissingAuthority = array.Count(result => result.Status.StartsWith("missing-authority", StringComparison.Ordinal)),
            MissingUnity = array.Count(result => result.Status.StartsWith("missing-unity", StringComparison.Ordinal)),
            Errors = array.Count(result => result.Status == "error"),
        };
    }

    private static DifferenceCategorySummary[] BuildDifferenceCategories(IEnumerable<OidAuditResult> results)
    {
        Dictionary<string, DifferenceCategorySummary> grouped = results
            .Where(result => result.Status == "different")
            .SelectMany(result => result.Differences.Select(difference => new
            {
                result.Oid,
                Category = ClassifyDifference(difference.Path),
            }))
            .GroupBy(value => value.Category, StringComparer.Ordinal)
            .OrderBy(group => CategoryOrder(group.Key))
            .Select(group => new DifferenceCategorySummary
            {
                Category = group.Key,
                DifferenceCount = group.Count(),
                OidCount = group.Select(value => value.Oid).Distinct().Count(),
                Oids = group.Select(value => value.Oid).Distinct().OrderBy(value => value).ToArray(),
            })
            .ToDictionary(value => value.Category, StringComparer.Ordinal);

        string[] categories = ["logic", "frame", "geometry", "sprite-dimension", "sound", "path-only"];
        return categories.Select(category => grouped.TryGetValue(category, out DifferenceCategorySummary? summary)
            ? summary
            : new DifferenceCategorySummary { Category = category }).ToArray();
    }

    private static string ClassifyDifference(string path)
    {
        if (path is "$.headFile" or "$.smallFile" or "$.spriteFile" ||
            (path.StartsWith("$.spriteRanges[", StringComparison.Ordinal) && path.EndsWith(".file", StringComparison.Ordinal)))
        {
            return "path-only";
        }
        if (path.EndsWith(".sound", StringComparison.Ordinal) ||
            path is "$.weaponHitSound" or "$.weaponDropSound" or "$.weaponBrokenSound")
        {
            return "sound";
        }
        if (path is "$.spriteW" or "$.spriteH" or "$.spriteRow" or "$.spriteCol" ||
            path.StartsWith("$.spriteRanges[", StringComparison.Ordinal))
        {
            return "sprite-dimension";
        }
        string member = path[(path.LastIndexOf('.') + 1)..];
        if (member is "x" or "y" or "w" or "h" or "zwidth" or
            "dvx" or "dvy" or "dvz" or "throwVx" or "throwVy" or "throwVz" or
            "centerX" or "centerY")
        {
            return "geometry";
        }
        if (path == "$.resolvedFrameCount" || path.StartsWith("$.resolvedFramesById.", StringComparison.Ordinal))
            return "frame";
        return "logic";
    }

    private static int CategoryOrder(string category)
        => category switch
        {
            "logic" => 0,
            "frame" => 1,
            "geometry" => 2,
            "sprite-dimension" => 3,
            "sound" => 4,
            "path-only" => 5,
            _ => 6,
        };

    private sealed class DataAuditReport
    {
        public string Schema { get; set; } = string.Empty;
        public int[] RequestedOids { get; set; } = [];
        public string[] NormalizedPathMembers { get; set; } = [];
        public int[] DuplicateAuthorityOids { get; set; } = [];
        public int[] DuplicateUnityOids { get; set; } = [];
        public ManifestSummary Manifest { get; set; } = new();
        public DataAuditSummary Summary { get; set; } = new();
        public DifferenceCategorySummary[] DifferenceCategories { get; set; } = [];
        public List<OidAuditResult> Results { get; set; } = [];
    }

    private sealed class ManifestSummary
    {
        public string Schema { get; set; } = string.Empty;
        public string AuthoritySha256 { get; set; } = string.Empty;
        public string UnitySha256 { get; set; } = string.Empty;
        public string AuthorityBattleLogicSha256 { get; set; } = string.Empty;
        public string UnityBattleLogicSha256 { get; set; } = string.Empty;
        public string AuthorityPresentationSha256 { get; set; } = string.Empty;
        public string UnityPresentationSha256 { get; set; } = string.Empty;
        public bool Equal { get; set; }
        public bool BattleLogicEqual { get; set; }
        public bool PresentationEqual { get; set; }
        public ManifestEntryDigest[] AuthorityEntries { get; set; } = [];
        public ManifestEntryDigest[] UnityEntries { get; set; } = [];
    }

    private sealed class ManifestEntryDigest
    {
        public int Oid { get; set; }
        public string Sha256 { get; set; } = string.Empty;
    }

    private sealed class DataAuditSummary
    {
        public int Audited { get; set; }
        public int Equal { get; set; }
        public int Different { get; set; }
        public int MissingAuthority { get; set; }
        public int MissingUnity { get; set; }
        public int Errors { get; set; }
    }

    private sealed class DifferenceCategorySummary
    {
        public string Category { get; set; } = string.Empty;
        public int DifferenceCount { get; set; }
        public int OidCount { get; set; }
        public int[] Oids { get; set; } = [];
    }

    private sealed class ManifestSet
    {
        public SortedDictionary<string, object?> Full { get; } = new(StringComparer.Ordinal);
        public SortedDictionary<string, object?> BattleLogic { get; } = new(StringComparer.Ordinal);
        public SortedDictionary<string, object?> Presentation { get; } = new(StringComparer.Ordinal);
    }

    private sealed class OidAuditResult
    {
        public int Oid { get; set; }
        public int AuthorityType { get; set; }
        public int UnityType { get; set; }
        public string? AuthorityPath { get; set; }
        public string? UnityPath { get; set; }
        public string? AuthorityInputKind { get; set; }
        public string? UnityInputKind { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
        public int[] DuplicateAuthorityFrameIds { get; set; } = [];
        public int[] DuplicateUnityFrameIds { get; set; } = [];
        public List<FieldDifference> Differences { get; set; } = [];
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs ---
﻿using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Simulation
{
    public enum SimulationDriveMode
    {
        LocalFreeRun,
        LockstepBuffered,
        Manual
    }

    /// <summary>
    /// 战斗逻辑帧配置。
    /// 逻辑帧长度固定使用 SimulationConstants.SIM_DT；这里的配置只决定外层驱动、追帧和联机预留策略。
    /// </summary>
    [System.Serializable]
    public sealed class LockstepSimulationSettings
    {
        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("单个 Unity 渲染帧最多追多少个逻辑帧。正式 NTSD 以 30Hz 逐帧呈现，默认不在一个渲染帧内连续追多个逻辑帧。")]
        public int maxCatchUpTicksPerFrame = 1;

        [Tooltip("最多保留多少个逻辑帧的时间积压，超过后丢弃外层积压但不改变单个逻辑帧步长。")]
        public int maxBacklogTicks = 8;

        [Tooltip("联机帧同步预留：本地输入写入未来第 N 帧。当前单机可保持 0。")]
        public int inputDelayTicks = 0;

        [Tooltip("联机帧同步预留：推进前是否要求该逻辑帧的输入已经准备好。")]
        public bool requireInputFrameReady = false;

        [Tooltip("预留世界校验点；当前只保留调用位置，后续可接入 checksum/hash。")]
        public bool enableFrameChecksum = false;

        public void Normalize()
        {
            if (maxCatchUpTicksPerFrame < 1) maxCatchUpTicksPerFrame = 1;
            if (maxBacklogTicks < maxCatchUpTicksPerFrame) maxBacklogTicks = maxCatchUpTicksPerFrame;
            if (inputDelayTicks < 0) inputDelayTicks = 0;
        }
    }

    /// <summary>
    /// 逻辑帧输入源预留接口。
    /// 当前单机输入仍由角色自己的 SimInputBuffer 消费；后续联机可在这里接入输入收齐、预测、回滚和重放。
    /// </summary>
    public interface ISimulationFrameInputProvider
    {
        bool IsFrameInputReady(int tickIndex);
        FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
        void BeforeSimTick(int tickIndex) { }
        void AfterSimTick(int tickIndex) { }
        void Reset() { }
    }

    public sealed class LocalSimulationFrameInputProvider : ISimulationFrameInputProvider
    {
        public bool IsFrameInputReady(int tickIndex) => true;
        public FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
    }

    /// <summary>
    /// 战斗场景模拟时钟。
    /// 负责固定 30Hz 逻辑 tick，并把 C++ release 风格的 pass 顺序交给 NTSDBattleTickSystem。
    /// Unity 的 Update/LateUpdate 只作为外层驱动和表现刷新；战斗逻辑内部不能依赖 deltaTime。
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        [Tooltip("记录每个模拟 tick 的开始和结束。")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("启动时暂停，直到 BattleBootstrap 恢复模拟。")]
        [SerializeField] private bool startPaused = true;

        [Header("帧同步时钟")]
        [SerializeField] private LockstepSimulationSettings lockstepSettings = new LockstepSimulationSettings();

        [Header("调试信息（只读）")]
        [SerializeField][MMReadOnly] private int currentTickIndex = 0;
        [SerializeField][MMReadOnly] private float timeAccumulator = 0f;
        [SerializeField][MMReadOnly] private int objectCount = 0;
        [SerializeField][MMReadOnly] private bool paused = true;
        [SerializeField][MMReadOnly] private float renderAlpha = 0f;
        [SerializeField][MMReadOnly] private int backlogTickCount = 0;
        [SerializeField][MMReadOnly] private string lastFrameChecksum = string.Empty;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;

        private int _sparkRenderFrame = 0;
        private ISimulationFrameInputProvider _frameInputProvider = new LocalSimulationFrameInputProvider();
        private FrameInputSet _lastAppliedFrameInput = FrameInputSet.Empty(0);
        private BattleParityFrameSnapshot _lastFrameSnapshot;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;
            lockstepSettings ??= new LockstepSimulationSettings();
            lockstepSettings.Normalize();

            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);

            Log.Info($"[SimulationTickDriver] Awake. paused={paused}, World created");
        }

        private void Update()
        {
            if (paused || _world == null || lockstepSettings.driveMode == SimulationDriveMode.Manual)
            {
                RefreshInspectorState();
                return;
            }

            float delta = lockstepSettings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timeAccumulator += delta;

            int maxBacklogTicks = Mathf.Max(lockstepSettings.maxBacklogTicks, lockstepSettings.maxCatchUpTicksPerFrame);
            float maxAccumulator = SimulationConstants.SIM_DT * maxBacklogTicks;
            if (_timeAccumulator > maxAccumulator)
                _timeAccumulator = maxAccumulator;

            int catchUpTicks = 0;
            while (_timeAccumulator >= SimulationConstants.SIM_DT &&
                   catchUpTicks < lockstepSettings.maxCatchUpTicksPerFrame)
            {
                int nextTickIndex = _tickIndex + 1;
                if (!CanAdvanceTick(nextTickIndex))
                    break;

                _timeAccumulator -= SimulationConstants.SIM_DT;
                StepOneTickInternal(nextTickIndex);
                catchUpTicks++;
            }

            RefreshInspectorState();
        }

        private void FixedUpdate()
        {
            // 帧同步逻辑不依赖 Unity FixedUpdate。Unity 物理循环只作为引擎外层回调存在。
        }

        private void LateUpdate()
        {
            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                    _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
            }

            _sparkRenderer.RenderAll(_world);
        }

        private bool CanAdvanceTick(int tickIndex)
        {
            if (lockstepSettings.driveMode != SimulationDriveMode.LockstepBuffered &&
                !lockstepSettings.requireInputFrameReady)
            {
                return true;
            }

            return _frameInputProvider == null || _frameInputProvider.IsFrameInputReady(tickIndex);
        }

        private bool StepOneTickInternal(int tickIndex)
        {
            if (_world == null || !CanAdvanceTick(tickIndex))
                return false;

            _tickIndex = tickIndex;
            _sparkRenderFrame = tickIndex;
            if (_world.Runtime?.Flow != null)
            {
                _world.Runtime.Flow.SparkRenderFrame = _sparkRenderFrame;
            }

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");

            _frameInputProvider?.BeforeSimTick(tickIndex);
            FrameInputSet frameInput = _frameInputProvider?.GetFrameInput(tickIndex) ??
                                       FrameInputSet.Empty(tickIndex);
            if (frameInput.TickIndex != tickIndex)
                frameInput = FrameInputSet.Empty(tickIndex);

            _lastAppliedFrameInput = frameInput;
            _world.ApplyFrameInputSet(frameInput);
            _battleTickSystem?.RunReleaseTick(tickIndex);
            CaptureFrameChecksumIfNeeded(tickIndex, frameInput);
            _frameInputProvider?.AfterSimTick(tickIndex);

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");

            return true;
        }

        private void CaptureFrameChecksumIfNeeded(int tickIndex, FrameInputSet frameInput)
        {
            if (!lockstepSettings.enableFrameChecksum)
            {
                _lastFrameSnapshot = null;
                lastFrameChecksum = string.Empty;
                return;
            }

            _lastFrameSnapshot = _world.CaptureParityFrameSnapshot(tickIndex, frameInput);
            lastFrameChecksum = _lastFrameSnapshot?.Hashes?.Overall ?? string.Empty;
        }

        private void RefreshInspectorState()
        {
            currentTickIndex = _tickIndex;
            timeAccumulator = _timeAccumulator;
            objectCount = _world?.ObjectCount ?? 0;
            renderAlpha = Mathf.Clamp01(_timeAccumulator / SimulationConstants.SIM_DT);
            backlogTickCount = Mathf.FloorToInt(_timeAccumulator / SimulationConstants.SIM_DT);
        }

        public SimulationWorld World => _world;
        public int SparkRenderFrame => _sparkRenderFrame;
        public int CurrentTickIndex => _tickIndex;
        public FrameInputSet LastAppliedFrameInput => _lastAppliedFrameInput;
        public BattleParityFrameSnapshot LastFrameSnapshot => _lastFrameSnapshot;
        public bool HasFrameChecksum => _lastFrameSnapshot != null;
        public string LastFrameChecksum => lastFrameChecksum;

        public float RemainingAccumulatorTime => _timeAccumulator;
        public float RenderAlpha => renderAlpha;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        public void ApplySettings(LockstepSimulationSettings settings)
        {
            if (settings == null)
                return;

            lockstepSettings = settings;
            lockstepSettings.Normalize();
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            if (_world == null)
                return;

            _world.ResetRuntimeState();

            BattleMatchRuntimeState matchState = _world.Runtime?.Match;
            if (matchState != null)
            {
                matchState.LocalGameModeId = config?.gameMode?.gameModeId ?? 0;
                matchState.BattleGameModeId = config?.gameMode?.battleGameModeId ?? 1;
                matchState.BackgroundId = config?.backgroundId ?? -1;
                matchState.Difficulty = config?.difficulty ?? 2;
                matchState.Seed = config?.seed ?? 0;
            }

            _world.Rng?.Seed((uint)(config?.seed ?? 0));
            _world.Runtime?.Roster?.ApplyMatchConfig(config);
            _world.RefreshStageRuntimeSnapshotFromScene();

            List<BattleStageCampaignData> stageCampaigns = BattleStageCampaignLoader.LoadFromFile(
                config?.stageCampaignFilePath);
            _world.ConfigureStageCampaigns(stageCampaigns, config?.stageSeriesId ?? 0, -1);
            if (matchState != null &&
                (matchState.BattleGameModeId == 1 || matchState.BattleGameModeId == 2))
            {
                _world.StartInitialStageWave();
            }

            _world.SetAiPhaseGate(matchState != null && matchState.BattleGameModeId == 2 ? 1 : 0);
        }

        public void SetFrameInputProvider(ISimulationFrameInputProvider provider)
        {
            _frameInputProvider = provider ?? new LocalSimulationFrameInputProvider();
            _frameInputProvider.Reset();
            _lastAppliedFrameInput = FrameInputSet.Empty(_tickIndex);
        }

        public bool StepOneTick(bool ignorePaused = false)
        {
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(_tickIndex + 1);
            RefreshInspectorState();
            return stepped;
        }

        public void UnbindWorld()
        {
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);
            _tickIndex = 0;
            _timeAccumulator = 0f;
            _sparkRenderFrame = 0;
            _lastAppliedFrameInput = FrameInputSet.Empty(0);
            _lastFrameSnapshot = null;
            lastFrameChecksum = string.Empty;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

        protected override void OnSingletonDestroyed()
        {
            _world = null;
            _battleTickSystem = null;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs ---
using NTSD.Animation.LF2Objects;
using NTSD.Input;

namespace NTSD.Simulation
{
    public partial class SimulationWorld
    {
        private static readonly (SimulationInputButtons button, FuncKeyMask key)[] FrameInputKeys =
        {
            (SimulationInputButtons.Right, FuncKeyMask.right),
            (SimulationInputButtons.Left, FuncKeyMask.left),
            (SimulationInputButtons.Up, FuncKeyMask.up),
            (SimulationInputButtons.Down, FuncKeyMask.down),
            (SimulationInputButtons.Attack, FuncKeyMask.att),
            (SimulationInputButtons.Jump, FuncKeyMask.jump),
            (SimulationInputButtons.Defend, FuncKeyMask.def),
        };

        public void ApplyFrameInputSet(FrameInputSet frameInput)
        {
            if (frameInput?.Players == null || frameInput.Players.Count == 0)
                return;

            for (int i = 0; i < frameInput.Players.Count; i++)
            {
                SimulationPlayerInput playerInput = frameInput.Players[i];
                if (!TryResolveRosterInputEntity(playerInput.PlayerSlot, out LF2Entity entity) ||
                    entity.AiControlled ||
                    !entity.TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
                {
                    continue;
                }

                // The frame packet is a complete held-state snapshot. Queue every key so an
                // authoritative replay packet is applied after any local callback queued for
                // the same tick; NTSDInputStateModule derives the press/release edges once.
                for (int keyIndex = 0; keyIndex < FrameInputKeys.Length; keyIndex++)
                {
                    (SimulationInputButtons button, FuncKeyMask key) mapping = FrameInputKeys[keyIndex];
                    bool down = (playerInput.Buttons & mapping.button) != 0;
                    controller.InputBuffer.EnqueueForTick(frameInput.TickIndex, mapping.key, down);
                }
            }
        }

        internal bool TryResolveRosterInputEntity(int playerSlot, out LF2Entity entity)
        {
            return TryResolveRosterEntity(playerSlot, requireHuman: true, out entity);
        }

        internal bool TryResolveRosterEntity(int playerSlot, bool requireHuman, out LF2Entity entity)
        {
            entity = null;
            BattleRosterRuntimeState roster = Runtime?.Roster;
            if (roster?.Slots == null || playerSlot < 0 || playerSlot >= roster.Slots.Length)
                return false;

            BattleSlotRuntimeState rosterSlot = roster.Slots[playerSlot];
            if (rosterSlot == null || !rosterSlot.Active || (requireHuman && !rosterSlot.IsHuman))
                return false;

            entity = ResolveRosterSlotEntity(rosterSlot.RuntimeSlotIndex, rosterSlot);
            if (entity == null && rosterSlot.StableId >= 0)
                entity = FindRosterEntityByStableId(rosterSlot.StableId, rosterSlot);

            if (entity == null)
                entity = ResolveRosterSlotEntity(playerSlot, rosterSlot);

            if (entity == null)
            {
                for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
                {
                    LF2Entity candidate = ResolveRosterSlotEntity(runtimeSlot, rosterSlot);
                    if (candidate == null || IsRuntimeSlotBoundToOtherRosterPlayer(runtimeSlot, playerSlot))
                        continue;

                    entity = candidate;
                    break;
                }
            }

            if (entity == null)
                return false;

            rosterSlot.RuntimeSlotIndex = entity.Runtime.SlotIndex;
            rosterSlot.StableId = entity.Runtime.StableId;
            return true;
        }

        private LF2Entity ResolveRosterSlotEntity(int runtimeSlot, BattleSlotRuntimeState rosterSlot)
        {
            if (runtimeSlot < 0 || runtimeSlot >= MaxRuntimeSlots)
                return null;

            LF2Entity candidate = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
            return RosterEntityMatches(candidate, rosterSlot) ? candidate : null;
        }

        private LF2Entity FindRosterEntityByStableId(int stableId, BattleSlotRuntimeState rosterSlot)
        {
            for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
            {
                LF2Entity candidate = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (candidate?.Runtime?.StableId == stableId && RosterEntityMatches(candidate, rosterSlot))
                    return candidate;
            }

            return null;
        }

        private bool IsRuntimeSlotBoundToOtherRosterPlayer(int runtimeSlot, int playerSlot)
        {
            BattleSlotRuntimeState[] rosterSlots = Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return false;

            for (int i = 0; i < rosterSlots.Length; i++)
            {
                if (i != playerSlot && rosterSlots[i]?.Active == true &&
                    rosterSlots[i].RuntimeSlotIndex == runtimeSlot)
                {
                    return true;
                }
            }

            return false;
        }

        private bool RosterEntityMatches(LF2Entity candidate, BattleSlotRuntimeState rosterSlot)
        {
            if (candidate?.Runtime == null || !IsActiveForCurrentPass(candidate) ||
                candidate.AiControlled == rosterSlot.IsHuman)
                return false;
            if (candidate.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return false;
            if (rosterSlot.CharacterId >= 0 && candidate.ObjectId != rosterSlot.CharacterId)
                return false;
            return candidate.Team == rosterSlot.Team;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs ---
using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    [Flags]
    public enum SimulationInputButtons : byte
    {
        None = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
        Attack = 1 << 4,
        Jump = 1 << 5,
        Defend = 1 << 6,
    }

    [Serializable]
    public readonly struct SimulationPlayerInput
    {
        public SimulationPlayerInput(int playerSlot, SimulationInputButtons buttons)
        {
            PlayerSlot = playerSlot;
            Buttons = buttons;
        }

        public int PlayerSlot { get; }
        public SimulationInputButtons Buttons { get; }
    }

    [Serializable]
    public sealed class FrameInputSet
    {
        private static readonly IReadOnlyList<SimulationPlayerInput> NoPlayers =
            Array.Empty<SimulationPlayerInput>();

        public FrameInputSet(int tickIndex, IReadOnlyList<SimulationPlayerInput> players = null)
        {
            TickIndex = tickIndex;
            Players = players ?? NoPlayers;
        }

        public int TickIndex { get; }
        public IReadOnlyList<SimulationPlayerInput> Players { get; }

        public static FrameInputSet Empty(int tickIndex)
        {
            return new FrameInputSet(tickIndex);
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs ---
using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// 所有战斗实体共享的运行时字段。
    /// 这里按语义镜像 C++ release 实体布局；Unity 的渲染、对象池和组件引用不写入战斗真相状态。
    /// </summary>
    [Serializable]
    public sealed class NTSDEntityRuntime
    {
        public int SlotIndex = -1;
        public int StableId;
        public int ObjectId;

        public int ObjType;
        public int EntityType;
        public int TransformOriginalObjectId = -1;
        public int TransformTargetObjectId = -1;

        public int Team;
        public int RelationTeam;
        public int OwnerSlotIndex = -1;
        public int OwnerStableId = -1;
        public int RelationOwnerSlotIndex = -1;
        public int SpawnerSlotIndex = -1;
        public int GrabbedBy;
        public int LinkState;
        public int TargetSlotIndex = -1;
        public int CaughtSlotIndex = -1;
        public int CatcherSlotIndex = -1;
        public int HeldWeaponStableId = -1;
        public int ThrowFrameGuard = -1;
        public int CaughtDuration;
        public int CaughtFrontFlag = 1;
        public int CatchingStateTU;
        public int JumpAttackLock;
        public int AnimCounter;
        public int AnimSub;
        public int LateSpecialTargetX;
        public int LateSpecialTargetZ;
        public int[] InputHistory = new int[6];
        public byte CdAttack;
        public byte CdJump;
        public byte CdDefend;
        public byte CdDefendLock;
        public byte CdRight;
        public byte CdLeft;
        public byte CdUp;
        public byte CdDown;
        public byte ComboDra;
        public byte ComboDla;
        public byte ComboDua;
        public byte ComboDda;
        public byte ComboDrj;
        public byte ComboDlj;
        public byte ComboDuj;
        public byte ComboDdj;
        public byte ComboDja;
        public byte PrevUp;
        public byte PrevDown;
        public byte PrevLeft;
        public byte PrevRight;
        public byte PrevJump;
        public byte PrevDefend;
        public byte PrevAttack;
        public byte KeyUp;
        public byte KeyDown;
        public byte KeyLeft;
        public byte KeyRight;
        public byte KeyAttack;
        public byte KeyJump;
        public byte KeyDefend;
        public int HolderStableId = -1;
        public int HolderCopySlotIndex = -1;
        public int PickerStableId = -1;
        public int TrackerFlag;
        public bool AiControlled;

        public double X;
        public double Y;
        public double Z;
        public int XInt;
        public int YInt;
        public int ZInt;
        public double Vx;
        public double Vy;
        public double Vz;
        public float SpriteX;
        public float SpriteY;
        public float SpriteZ;
        public double Type3VisualZOffset;
        public float RenderOffsetX;
        public string Dir = "right";
        public float Zz;
        public bool XBoundPositive;
        public bool XBoundNegative;
        public bool ZBoundPositive;
        public bool ZBoundNegative;

        public int Frame;
        public int PrevFrame2;
        public int FirstPresentationTick;
        public int SpawnSemantic;
        public int SuppressFrameTickUntilTick;
        public int SuppressLateFrameTickUntilTick;
        public int SuppressPostInteractionUntilTick;
        public int SuppressObjectInteractionUntilTick;
        public int SuppressPreInteractionUntilTick;
        public int SuppressCollisionCandidateUntilTick;
        public int RenderPicOffset;
        public int WaitCounter;
        public int NextFrame;
        public int AttackingCounter;
        public int FrameDelay;
        public int HitStop;
        public double KnockbackVx;
        public double KnockbackVy;
        public double KnockbackVz;
        public int ShakeTimer;
        public int AttackExempt;
        public int HitStateCount;
        public int Fall;
        public int Bdefend;
        public int HitCount;
        public int HitConfirmEa;
        public int HitConfirm2;
        public int HealTimer;
        public int CatchTimer;
        public int KillCount = -1;
        public int ComboCountVic;
        public int ComboCountAtk;
        public int KillStat;
        public int Unk328 = -1;
        public int Unk32C = -1;
        public int Unk330;
        public int Unk334;
        public int Unk338;
        public int Unk344;
        public int Unk360 = -1;
        public int Unk3FC = -1000;
        public int Unk400 = -1000;
        public int ShotCount;
        public int WeaponCount;
        public int FallDamageDiv;
        public int WeaponFlightCounter;
        public int WeaponDropHurt;
        public int WeaponState;
        public int Blink;
        public int HitCandidateCount;
        public int HitCandidateNearestDistance = 1000;
        public int HitCandidateKind1Distance = 1000;
        public int HitCandidateExtraDistance = 1000;
        public bool OidMergeDormant;
        public bool PendingFlushDestroy;

        public int HP = 500;
        public int HPBound = 500;
        public int HP3 = 500;
        public int HPOrig;
        public int HP2Orig;
        public int RespawnCount;
        public int HPLost;
        public int MP = 500;
        public int MPMax = 500;
        public int PP = 500;
        public int PPMax = 500;
        public int PPBound = 500;
        public int PpDisplay;

        public void SetPosition(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void SetVelocity(double vx, double vy, double vz)
        {
            Vx = vx;
            Vy = vy;
            Vz = vz;
        }

        public void SyncIntegerPosition()
        {
            XInt = (int)X;
            YInt = (int)Y;
            ZInt = (int)Z;
        }

        public void UpdateSpriteOrigin(int centerx, int centery, float spriteWidthPx)
        {
            SpriteX = (float)(Dir == "right"
                ? X - centerx
                : X + centerx - spriteWidthPx);
            SpriteY = (float)(Y + Z - centery);
            SpriteZ = (float)Z;
        }

        public void ClearBounds()
        {
            XBoundPositive = false;
            XBoundNegative = false;
            ZBoundPositive = false;
            ZBoundNegative = false;
        }

        public int ResolveActiveHeldSlotIndex()
        {
            return LinkState > 0 ? TargetSlotIndex : -1;
        }

        public int ResolveActiveHolderSlotIndex()
        {
            return LinkState < 0 ? HolderStableId : -1;
        }

        public bool IsActivelyHeldBySlot(int holderSlotIndex)
        {
            return LinkState < 0 && HolderStableId == holderSlotIndex;
        }

        public void RollInputFromCurrent()
        {
            PrevUp = KeyUp;
            PrevDown = KeyDown;
            PrevLeft = KeyLeft;
            PrevRight = KeyRight;
            PrevJump = KeyJump;
            PrevDefend = KeyDefend;
            PrevAttack = KeyAttack;
        }

        public bool HasInputHistoryGate()
        {
            EnsureInputHistory();
            return InputHistory[0] != 0;
        }

        public void ClearDirectionalInputKeys()
        {
            KeyUp = KeyDown = KeyLeft = KeyRight = 0;
        }

        public void ClearActionInputKeys()
        {
            KeyAttack = KeyJump = KeyDefend = 0;
        }

        public void ApplyInputEdges()
        {
            if (PrevRight == 0 && KeyRight == 1) { CdRight = 5; PushInputHistory(6); }
            if (PrevLeft == 0 && KeyLeft == 1) { CdLeft = 5; PushInputHistory(4); }
            if (PrevUp == 0 && KeyUp == 1) { CdUp = 5; PushInputHistory(8); }
            if (PrevDown == 0 && KeyDown == 1) { CdDown = 5; PushInputHistory(2); }
            if (PrevAttack == 0 && KeyAttack == 1) { CdDefend = 5; PushInputHistory(9); }
            if (PrevDefend == 0 && KeyDefend == 1) { CdJump = 5; PushInputHistory(0); }
            if (PrevJump == 0 && KeyJump == 1) { CdAttack = 5; PushInputHistory(5); }
        }

        public void PushInputHistory(int keyNum)
        {
            EnsureInputHistory();
            InputHistory[1] = InputHistory[2];
            InputHistory[2] = InputHistory[3];
            InputHistory[3] = InputHistory[4];
            InputHistory[4] = InputHistory[5];
            InputHistory[5] = keyNum;
        }

        public void SetInputHistoryGate(bool enabled)
        {
            EnsureInputHistory();
            InputHistory[0] = enabled ? 1 : 0;
        }

        public void ClearInputHistoryTail()
        {
            EnsureInputHistory();
            Array.Clear(InputHistory, 1, InputHistory.Length - 1);
        }

        public void TickInputCooldowns()
        {
            if (CdRight > 0) CdRight--;
            if (CdLeft > 0) CdLeft--;
            if (CdUp > 0) CdUp--;
            if (CdDown > 0) CdDown--;
            if (CdJump > 0) CdJump--;
            if (CdAttack > 0) CdAttack--;
            if (CdDefend > 0) CdDefend--;
        }

        private void EnsureInputHistory()
        {
            if (InputHistory == null || InputHistory.Length != 6)
                InputHistory = new int[6];
        }

        internal void TickDefendLockCooldown()
        {
            if (CdDefendLock > 0)
                CdDefendLock--;
        }

        public void Reset()
        {
            SlotIndex = -1;
            StableId = 0;
            ObjectId = 0;
            ObjType = 0;
            EntityType = 0;
            TransformOriginalObjectId = -1;
            TransformTargetObjectId = -1;
            Team = 0;
            RelationTeam = 0;
            OwnerSlotIndex = -1;
            OwnerStableId = -1;
            RelationOwnerSlotIndex = -1;
            SpawnerSlotIndex = -1;
            GrabbedBy = 0;
            LinkState = 0;
            TargetSlotIndex = -1;
            CaughtSlotIndex = -1;
            CatcherSlotIndex = -1;
            HeldWeaponStableId = -1;
            ThrowFrameGuard = -1;
            CaughtDuration = 0;
            CaughtFrontFlag = 1;
            CatchingStateTU = 0;
            JumpAttackLock = 0;
            AnimCounter = 0;
            AnimSub = 0;
            LateSpecialTargetX = 0;
            LateSpecialTargetZ = 0;
            EnsureInputHistory();
            Array.Clear(InputHistory, 0, InputHistory.Length);
            CdAttack = 0;
            CdJump = 0;
            CdDefend = 0;
            CdDefendLock = 0;
            CdRight = 0;
            CdLeft = 0;
            CdUp = 0;
            CdDown = 0;
            ComboDra = 0;
            ComboDla = 0;
            ComboDua = 0;
            ComboDda = 0;
            ComboDrj = 0;
            ComboDlj = 0;
            ComboDuj = 0;
            ComboDdj = 0;
            ComboDja = 0;
            PrevUp = 0;
            PrevDown = 0;
            PrevLeft = 0;
            PrevRight = 0;
            PrevJump = 0;
            PrevDefend = 0;
            PrevAttack = 0;
            KeyUp = 0;
            KeyDown = 0;
            KeyLeft = 0;
            KeyRight = 0;
            KeyAttack = 0;
            KeyJump = 0;
            KeyDefend = 0;
            HolderStableId = -1;
            HolderCopySlotIndex = -1;
            PickerStableId = -1;
            TrackerFlag = 0;
            AiControlled = false;
            X = 0f;
            Y = 0f;
            Z = 0f;
            XInt = 0;
            YInt = 0;
            ZInt = 0;
            Vx = 0f;
            Vy = 0f;
            Vz = 0f;
            SpriteX = 0f;
            SpriteY = 0f;
            SpriteZ = 0f;
            Type3VisualZOffset = 0.0;
            RenderOffsetX = 0f;
            Dir = "right";
            Zz = 0f;
            ClearBounds();
            Frame = 0;
            PrevFrame2 = 0;
            FirstPresentationTick = 0;
            SpawnSemantic = 0;
            SuppressFrameTickUntilTick = 0;
            SuppressLateFrameTickUntilTick = 0;
            SuppressPostInteractionUntilTick = 0;
            SuppressObjectInteractionUntilTick = 0;
            SuppressPreInteractionUntilTick = 0;
            SuppressCollisionCandidateUntilTick = 0;
            RenderPicOffset = 0;
            WaitCounter = 0;
            NextFrame = 0;
            AttackingCounter = 0;
            FrameDelay = 0;
            HitStop = 0;
            KnockbackVx = 0.0;
            KnockbackVy = 0.0;
            KnockbackVz = 0.0;
            ShakeTimer = 0;
            AttackExempt = 0;
            HitStateCount = 0;
            Fall = 0;
            Bdefend = 0;
            HitCount = 0;
            HitConfirmEa = 0;
            HitConfirm2 = 0;
            HealTimer = 0;
            CatchTimer = 0;
            KillCount = -1;
            ComboCountVic = 0;
            ComboCountAtk = 0;
            KillStat = 0;
            Unk328 = -1;
            Unk32C = -1;
            Unk330 = 0;
            Unk334 = 0;
            Unk338 = 0;
            Unk344 = 0;
            Unk360 = -1;
            Unk3FC = -1000;
            Unk400 = -1000;
            ShotCount = 0;
            WeaponCount = 0;
            FallDamageDiv = 0;
            WeaponFlightCounter = 0;
            WeaponDropHurt = 0;
            WeaponState = 0;
            Blink = 0;
            HitCandidateCount = 0;
            HitCandidateNearestDistance = 1000;
            HitCandidateKind1Distance = 1000;
            HitCandidateExtraDistance = 1000;
            OidMergeDormant = false;
            PendingFlushDestroy = false;
            HP = 500;
            HPBound = 500;
            HP3 = 500;
            HPOrig = 0;
            HP2Orig = 0;
            RespawnCount = 0;
            HPLost = 0;
            MP = 500;
            MPMax = 500;
            PP = 500;
            PPMax = 500;
            PPBound = 500;
            PpDisplay = 0;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs ---
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public sealed class BattleParityHashes
    {
        public string ARest;
        public string Events;
        public string Input;
        public string Overall;
        public string Rng;
        public string Slots;
        public string Stats;
        public string VRest;
        public string World;

        internal SortedDictionary<string, object> ToCanonicalObject(bool includeOverall)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARest,
                ["events"] = Events,
                ["input"] = Input,
                ["rng"] = Rng,
                ["slots"] = Slots,
                ["stats"] = Stats,
                ["vRest"] = VRest,
                ["world"] = World,
            };
            if (includeOverall)
                result["overall"] = Overall;
            return result;
        }
    }

    public sealed class BattleParityFrameSnapshot
    {
        internal object InputDomain;
        internal object RngDomain;
        internal object WorldDomain;
        internal object[] AllSlotsDomain;
        internal object[] CompactSlotsDomain;
        internal object ARestDomain;
        internal object VRestDomain;
        internal object StatsDomain;
        internal object EventsDomain;

        public int Tick { get; internal set; }
        public int ObjectCount { get; internal set; }
        public BattleParityHashes Hashes { get; internal set; }

        public string ToJson()
        {
            var tick = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARestDomain,
                ["events"] = EventsDomain,
                ["hashes"] = Hashes.ToCanonicalObject(includeOverall: true),
                ["input"] = InputDomain,
                ["kind"] = "tick",
                ["objectCount"] = ObjectCount,
                ["rng"] = RngDomain,
                ["slots"] = CompactSlotsDomain,
                ["stats"] = StatsDomain,
                ["tick"] = Tick,
                ["vRest"] = VRestDomain,
                ["world"] = WorldDomain,
            };
            return BattleCanonicalJson.Serialize(tick);
        }
    }

    public static class BattleCanonicalJson
    {
        public static string Serialize(object value)
        {
            var builder = new StringBuilder(4096);
            WriteValue(builder, value);
            return builder.ToString();
        }

        public static string Sha256(object value)
        {
            byte[] payload = Encoding.UTF8.GetBytes(Serialize(value));
            using SHA256 sha = SHA256.Create();
            byte[] digest = sha.ComputeHash(payload);
            var builder = new StringBuilder(digest.Length * 2);
            for (int i = 0; i < digest.Length; i++)
                builder.Append(digest[i].ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static void WriteValue(StringBuilder builder, object value)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            switch (value)
            {
                case string text:
                    WriteString(builder, text);
                    return;
                case char character:
                    WriteString(builder, character.ToString());
                    return;
                case bool boolean:
                    builder.Append(boolean ? "true" : "false");
                    return;
                case byte byteValue:
                    builder.Append(byteValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case sbyte signedByteValue:
                    builder.Append(signedByteValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case short shortValue:
                    builder.Append(shortValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case ushort unsignedShortValue:
                    builder.Append(unsignedShortValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case int intValue:
                    builder.Append(intValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case uint unsignedIntValue:
                    builder.Append(unsignedIntValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case long longValue:
                    builder.Append(longValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case ulong unsignedLongValue:
                    builder.Append(unsignedLongValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case float floatValue:
                    WriteFloatingPoint(builder, floatValue);
                    return;
                case double doubleValue:
                    WriteFloatingPoint(builder, doubleValue);
                    return;
                case decimal decimalValue:
                    builder.Append(decimalValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case IDictionary dictionary:
                    WriteDictionary(builder, dictionary);
                    return;
                case IEnumerable enumerable:
                    WriteArray(builder, enumerable);
                    return;
            }

            if (value.GetType().IsEnum)
            {
                builder.Append(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                return;
            }

            throw new InvalidOperationException(
                $"Unsupported canonical JSON value type: {value.GetType().FullName}");
        }

        private static void WriteDictionary(StringBuilder builder, IDictionary dictionary)
        {
            var keys = new List<string>(dictionary.Count);
            foreach (object key in dictionary.Keys)
                keys.Add(Convert.ToString(key, CultureInfo.InvariantCulture));
            keys.Sort(StringComparer.Ordinal);

            builder.Append('{');
            for (int i = 0; i < keys.Count; i++)
            {
                if (i > 0)
                    builder.Append(',');
                string key = keys[i];
                WriteString(builder, key);
                builder.Append(':');
                WriteValue(builder, dictionary[key]);
            }
            builder.Append('}');
        }

        private static void WriteArray(StringBuilder builder, IEnumerable values)
        {
            builder.Append('[');
            bool first = true;
            foreach (object value in values)
            {
                if (!first)
                    builder.Append(',');
                first = false;
                WriteValue(builder, value);
            }
            builder.Append(']');
        }

        private static void WriteFloatingPoint(StringBuilder builder, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new InvalidOperationException("Canonical battle snapshots cannot contain NaN or Infinity.");
            builder.Append(value.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void WriteString(StringBuilder builder, string value)
        {
            builder.Append('"');
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (c < 0x20 || c > 0x7E)
                            builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            builder.Append(c);
                        break;
                }
            }
            builder.Append('"');
        }
    }

    public partial class SimulationWorld
    {
        public BattleParityFrameSnapshot CaptureParityFrameSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null)
        {
            object inputDomain = ProjectFrameInput(frameInput ?? FrameInputSet.Empty(tickIndex));
            object rngDomain = DictionaryOf(
                ("callCount", (object)(Rng?.CallCount ?? 0UL)),
                ("seed", Rng?.State ?? 0U));
            object worldDomain = ProjectWorldDomain();
            object[] allSlots = ProjectAllRuntimeSlots();
            object aRestDomain = ProjectARestDomain();
            object vRestDomain = ProjectVRestDomain();
            object statsDomain = DictionaryOf(
                ("damage", CloneArray(DamageStats)),
                ("kill", CloneArray(KillStats)));
            object eventsDomain = ProjectEventsDomain();

            var hashes = new BattleParityHashes
            {
                ARest = BattleCanonicalJson.Sha256(aRestDomain),
                Events = BattleCanonicalJson.Sha256(eventsDomain),
                Input = BattleCanonicalJson.Sha256(inputDomain),
                Rng = BattleCanonicalJson.Sha256(rngDomain),
                Slots = BattleCanonicalJson.Sha256(allSlots),
                Stats = BattleCanonicalJson.Sha256(statsDomain),
                VRest = BattleCanonicalJson.Sha256(vRestDomain),
                World = BattleCanonicalJson.Sha256(worldDomain),
            };
            hashes.Overall = BattleCanonicalJson.Sha256(hashes.ToCanonicalObject(includeOverall: false));

            var compactSlots = new List<object>();
            for (int slot = 0; slot < allSlots.Length; slot++)
            {
                object baseline = ProjectDefaultRuntimeSlot(slot);
                if (!string.Equals(
                        BattleCanonicalJson.Sha256(allSlots[slot]),
                        BattleCanonicalJson.Sha256(baseline),
                        StringComparison.Ordinal))
                {
                    compactSlots.Add(allSlots[slot]);
                }
            }

            return new BattleParityFrameSnapshot
            {
                Tick = tickIndex,
                ObjectCount = ObjectCount,
                Hashes = hashes,
                InputDomain = inputDomain,
                RngDomain = rngDomain,
                WorldDomain = worldDomain,
                AllSlotsDomain = allSlots,
                CompactSlotsDomain = compactSlots.ToArray(),
                ARestDomain = aRestDomain,
                VRestDomain = vRestDomain,
                StatsDomain = statsDomain,
                EventsDomain = eventsDomain,
            };
        }

        private object ProjectFrameInput(FrameInputSet frameInput)
        {
            var players = new object[frameInput.Players?.Count ?? 0];
            for (int i = 0; i < players.Length; i++)
            {
                SimulationPlayerInput player = frameInput.Players[i];
                players[i] = DictionaryOf(
                    ("buttons", (object)(byte)player.Buttons),
                    ("playerSlot", player.PlayerSlot));
            }
            return DictionaryOf(("players", (object)players), ("tickIndex", frameInput.TickIndex));
        }

        private object[] ProjectAllRuntimeSlots()
        {
            var result = new object[MaxRuntimeSlots];
            for (int runtimeSlot = 0; runtimeSlot < result.Length; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                result[runtimeSlot] = entity == null
                    ? ProjectDefaultRuntimeSlot(runtimeSlot)
                    : ProjectRuntimeSlot(entity, runtimeSlot);
            }
            return result;
        }

        private object ProjectDefaultRuntimeSlot(int runtimeSlot)
        {
            return DictionaryOf(
                ("currentDataOid", null),
                ("runtime", ProjectEntityRuntime(null, runtimeSlot, false)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectRuntimeSlot(LF2Entity entity, int runtimeSlot)
        {
            bool active = IsActiveForCurrentPass(entity);
            int? currentDataOid = entity.FrameCache?.Wrapper != null
                ? entity.FrameCache.Wrapper.characterId
                : entity.ObjectId;
            return DictionaryOf(
                ("currentDataOid", (object)currentDataOid),
                ("runtime", ProjectEntityRuntime(entity, runtimeSlot, active)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectEntityRuntime(LF2Entity entity, int runtimeSlot, bool active)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            bool isDefault = runtime == null;
            int[] hitRecordDamage = new int[LF2Entity.MaxHitRecordSlots];
            int[] hitRecordX = new int[LF2Entity.MaxHitRecordSlots];
            int[] hitRecordZ = new int[LF2Entity.MaxHitRecordSlots];
            if (entity != null)
            {
                for (int i = 0; i < hitRecordDamage.Length; i++)
                {
                    hitRecordDamage[i] = entity.GetHitRecordAge(i);
                    hitRecordX[i] = entity.GetHitRecordX(i);
                    hitRecordZ[i] = entity.GetHitRecordZ(i);
                }
            }

            int currentDataType = entity?.GetCurrentDataObjectTypeForSimulation() ?? -1;
            int category = ResolveTraceCategory(currentDataType);
            object identity = DictionaryOf(
                ("active", active),
                ("aiControlled", runtime?.AiControlled ?? false),
                ("category", isDefault ? 3 : category),
                ("charId", isDefault ? -1 : runtime.ObjectId),
                ("entityType", isDefault ? 0 : runtime.EntityType),
                ("objType", isDefault ? 0 : runtime.ObjType),
                ("ownerId", isDefault ? -1 : runtime.OwnerSlotIndex),
                ("slot", runtimeSlot),
                ("team", isDefault ? 0 : runtime.Team),
                ("unk364", isDefault ? 0 : (runtime.RelationTeam != 0 ? runtime.RelationTeam : runtime.Team)));

            object transform = DictionaryOf(
                ("facing", (object)(isDefault || runtime.Dir == "right" ? 0 : 1)),
                ("renderOffsetX", isDefault ? 0 : (int)runtime.RenderOffsetX),
                ("type3VisualZOffset", isDefault ? 0.0 : runtime.Type3VisualZOffset),
                ("x", isDefault ? 0.0 : runtime.X),
                ("xInt", isDefault ? 0 : runtime.XInt),
                ("y", isDefault ? 0.0 : runtime.Y),
                ("yInt", isDefault ? 0 : runtime.YInt),
                ("z", isDefault ? 0.0 : runtime.Z),
                ("zInt", isDefault ? 0 : runtime.ZInt));

            object motion = DictionaryOf(
                ("fall", isDefault ? 0 : runtime.Fall),
                ("hitCount", isDefault ? 0 : runtime.HitCount),
                ("knockbackVx", isDefault ? 0.1 : runtime.KnockbackVx),
                ("knockbackVy", isDefault ? 0.1 : runtime.KnockbackVy),
                ("knockbackVz", isDefault ? 0.1 : runtime.KnockbackVz),
                ("vx", isDefault ? 0.0 : runtime.Vx),
                ("vy", isDefault ? 0.0 : runtime.Vy),
                ("vz", isDefault ? 0.0 : runtime.Vz));

            object frame = DictionaryOf(
                ("animCounter", isDefault ? 0 : runtime.AnimCounter),
                ("animSub", isDefault ? 0 : runtime.AnimSub),
                ("attacking", isDefault ? 0 : runtime.AttackingCounter),
                ("frame", isDefault ? 0 : runtime.Frame),
                ("frameDelay", isDefault ? 0 : runtime.FrameDelay),
                ("frameWaitCounter", isDefault ? 0 : runtime.WaitCounter),
                ("hitStateCount", isDefault ? 0 : runtime.HitStateCount),
                ("hitStop", isDefault ? 0 : runtime.HitStop),
                ("jumpInitPending", false),
                ("prevFrame", isDefault ? 0 : entity.Frame?.Prev ?? 0),
                ("prevFrame2", isDefault ? 0 : runtime.PrevFrame2),
                ("suppressJumpInit", false),
                ("waitCounter", isDefault ? 0 : runtime.WaitCounter));

            object links = DictionaryOf(
                ("catcherIdx", isDefault ? -1 : runtime.CatcherSlotIndex),
                ("caughtDuration", isDefault ? 0 : runtime.CaughtDuration),
                ("caughtIdx", isDefault ? -1 : runtime.CaughtSlotIndex),
                ("escapeCounter", isDefault ? 0 : runtime.CatchingStateTU),
                ("grabbedTimer", isDefault ? 0 : runtime.GrabbedBy),
                ("heldWeaponSlot", isDefault ? -1 : runtime.HeldWeaponStableId),
                ("holderCopy", isDefault ? 99 : runtime.HolderCopySlotIndex),
                ("holderIdx", isDefault ? -1 : runtime.HolderStableId),
                ("linkState", isDefault ? 0 : runtime.LinkState),
                ("pickerIdx", isDefault ? -1 : runtime.PickerStableId),
                ("pickupCount", 0),
                ("releaseTick", -1),
                ("stuckVictimSlot", -1),
                ("targetIdx", isDefault ? -1 : runtime.TargetSlotIndex),
                ("throwFrameGuard", isDefault ? -1 : runtime.ThrowFrameGuard));

            object transient = DictionaryOf(
                ("hitCandidateItrIndices", (object)new sbyte[20]),
                ("hitCandidateSlots", new int[20]),
                ("mp", isDefault ? 0 : runtime.MP),
                ("mp2", 1000),
                ("mp3", 1000),
                ("mp4", 1000));

            object stats = DictionaryOf(
                ("comboCountAtk", isDefault ? 0 : runtime.ComboCountAtk),
                ("comboCountVic", isDefault ? 0 : runtime.ComboCountVic),
                ("fallDamageDiv", isDefault ? 0 : runtime.FallDamageDiv),
                ("hp", isDefault ? 500 : runtime.HP),
                ("hp3", isDefault ? 500 : runtime.HP3),
                ("hpMax", isDefault ? 500 : runtime.HPBound),
                ("killCount", isDefault ? -1 : runtime.KillCount),
                ("killStat", isDefault ? 0 : runtime.KillStat),
                ("pp", isDefault ? 500 : runtime.PP),
                ("respawnCount", isDefault ? 0 : runtime.RespawnCount),
                ("spawnerSlot", isDefault ? -1 : runtime.SpawnerSlotIndex),
                ("unk344", isDefault ? 0 : runtime.Unk344),
                ("weaponCount", isDefault ? 0 : runtime.WeaponCount));

            object input = DictionaryOf(
                ("cdAttack", (object)(runtime?.CdAttack ?? 0)),
                ("cdDefend", runtime?.CdDefend ?? 0),
                ("cdDefendLock", runtime?.CdDefendLock ?? 0),
                ("cdDown", runtime?.CdDown ?? 0),
                ("cdJump", runtime?.CdJump ?? 0),
                ("cdLeft", runtime?.CdLeft ?? 0),
                ("cdRight", runtime?.CdRight ?? 0),
                ("cdUp", runtime?.CdUp ?? 0),
                ("comboDda", runtime?.ComboDda ?? 0),
                ("comboDdj", runtime?.ComboDdj ?? 0),
                ("comboDja", runtime?.ComboDja ?? 0),
                ("comboDla", runtime?.ComboDla ?? 0),
                ("comboDlj", runtime?.ComboDlj ?? 0),
                ("comboDra", runtime?.ComboDra ?? 0),
                ("comboDrj", runtime?.ComboDrj ?? 0),
                ("comboDua", runtime?.ComboDua ?? 0),
                ("comboDuj", runtime?.ComboDuj ?? 0),
                ("inputHistory", isDefault ? new int[6] : CloneArray(runtime.InputHistory)),
                ("keyAttack", runtime?.KeyAttack ?? 0),
                ("keyDefend", runtime?.KeyDefend ?? 0),
                ("keyDown", runtime?.KeyDown ?? 0),
                ("keyJump", runtime?.KeyJump ?? 0),
                ("keyLeft", runtime?.KeyLeft ?? 0),
                ("keyRight", runtime?.KeyRight ?? 0),
                ("keyUp", runtime?.KeyUp ?? 0),
                ("prevAttack", runtime?.PrevAttack ?? 0),
                ("prevDefend", runtime?.PrevDefend ?? 0),
                ("prevDown", runtime?.PrevDown ?? 0),
                ("prevJump", runtime?.PrevJump ?? 0),
                ("prevLeft", runtime?.PrevLeft ?? 0),
                ("prevRight", runtime?.PrevRight ?? 0),
                ("prevUp", runtime?.PrevUp ?? 0));

            object presentation = DictionaryOf(
                ("blink", isDefault ? 0 : runtime.Blink),
                ("hitRecordCount", entity?.HitRecordCount ?? 0),
                ("hitRecordDamage", hitRecordDamage),
                ("hitRecordX", hitRecordX),
                ("hitRecordZ", hitRecordZ),
                ("hp2Orig", isDefault ? 0 : runtime.HP2Orig),
                ("hpOrig", isDefault ? 0 : runtime.HPOrig),
                ("ppDisplay", isDefault ? 0 : runtime.PpDisplay));

            object residual = DictionaryOf(
                ("abortRemainingHitPairs", false),
                ("attackExempt", isDefault ? 0 : runtime.AttackExempt),
                ("blockBackZ", 0),
                ("blockFwdZ", 0),
                ("blockLeft", 0),
                ("blockRight", 0),
                ("catchTimer", isDefault ? 0 : runtime.CatchTimer),
                ("healTimer", isDefault ? 0 : runtime.HealTimer),
                ("hitConfirm", isDefault ? 0 : runtime.HitConfirmEa),
                ("hitConfirm2", isDefault ? 0 : runtime.HitConfirm2),
                ("unk318", 0),
                ("unk31C", 0),
                ("unk324", -1),
                ("unk328", isDefault ? -1 : runtime.Unk328),
                ("unk32C", isDefault ? -1 : runtime.Unk32C),
                ("unk330", isDefault ? 0 : runtime.Unk330),
                ("unk334", isDefault ? 0 : runtime.Unk334),
                ("unk338", isDefault ? 0 : runtime.Unk338),
                ("unk33C", -1),
                ("unk360", isDefault ? -1 : runtime.Unk360),
                ("unk3FC", isDefault ? -1000 : runtime.Unk3FC),
                ("unk400", isDefault ? -1000 : runtime.Unk400),
                ("weaponState", isDefault ? 0 : runtime.WeaponState));

            return DictionaryOf(
                ("frame", frame),
                ("identity", identity),
                ("input", input),
                ("links", links),
                ("motion", motion),
                ("presentation", presentation),
                ("residual", residual),
                ("stats", stats),
                ("transform", transform),
                ("transient", transient));
        }

        private object ProjectWorldDomain()
        {
            BattleRuntimeState battle = Runtime ?? new BattleRuntimeState();
            BattleMatchRuntimeState match = battle.Match ?? new BattleMatchRuntimeState();
            BattleStageRuntimeState stage = battle.Stage ?? new BattleStageRuntimeState();
            BattleFlowRuntimeState flow = battle.Flow ?? new BattleFlowRuntimeState();
            BattleRosterRuntimeState roster = battle.Roster ?? new BattleRosterRuntimeState();
            BattleStageProgressionState progression = battle.StageProgression ?? new BattleStageProgressionState();

            int slotCount = roster.Slots?.Length ?? 0;
            var battleSlotEntity = FilledArray(8, -1);
            var battleSlotOid = FilledArray(8, -1);
            var battleSlotState = new int[8];
            var battleSlotTeam = FilledArray(8, 1);
            var rosterSlots = new object[8];
            for (int i = 0; i < rosterSlots.Length; i++)
            {
                BattleSlotRuntimeState slot = i < slotCount ? roster.Slots[i] : null;
                bool active = slot?.Active ?? false;
                if (active)
                    TryResolveRosterEntity(i, requireHuman: false, out _);
                int oid = active ? slot.CharacterId : -1;
                int entitySlot = active ? slot.RuntimeSlotIndex : -1;
                int team = active ? slot.Team : 1;
                battleSlotEntity[i] = entitySlot;
                battleSlotOid[i] = oid;
                battleSlotState[i] = active ? 3 : 0;
                battleSlotTeam[i] = team;
                rosterSlots[i] = DictionaryOf(
                    ("active", (object)active),
                    ("ai", active && !slot.IsHuman),
                    ("entitySlot", entitySlot),
                    ("oid", oid),
                    ("state", battleSlotState[i]),
                    ("team", team));
            }

            object runtimeDomain = DictionaryOf(
                ("flow", DictionaryOf(
                    ("aiPhaseGate", (object)flow.AiPhaseGate),
                    ("battlePauseOverlay", 0),
                    ("battleStepEarlyReturned", 0),
                    ("battleStepFlag", 0),
                    ("battleStepGate", flow.BattleStepGate),
                    ("battleStepMode", flow.BattleStepMode),
                    ("frameMod12", flow.FrameMod12),
                    ("frameToggle", flow.FrameToggle),
                    ("gameTick", flow.CurrentTickIndex),
                    ("inputPhase", flow.InputPhase),
                    ("needClearInput", false),
                    ("paused", false))),
                ("match", DictionaryOf(
                    ("difficulty", (object)match.Difficulty),
                    ("gameMode", match.BattleGameModeId),
                    ("randomStage", match.BackgroundId),
                    ("seed", match.Seed),
                    ("stageIdx", progression.StageSeriesIdx))),
                ("roster", DictionaryOf(
                    ("activeSlotCount", (object)roster.ActiveSlotCount),
                    ("slots", rosterSlots))),
                ("stage", DictionaryOf(
                    ("boundLeft", (object)0),
                    ("boundRight", stage.StageWidthPx),
                    ("cameraMaxOverride", stage.CameraMaxOverride),
                    ("cameraVel", _cameraVel),
                    ("cameraX", _cameraX),
                    ("width", stage.StageWidthPx),
                    ("xMaxOverride", stage.XMaxOverride),
                    ("zMax", stage.ZMax),
                    ("zMin", stage.ZMin))));

            return DictionaryOf(
                ("aiDifficulty", (object)flow.AiDifficulty),
                ("aiMoveMode", flow.AiMoveMode),
                ("aiPhaseGate", flow.AiPhaseGate),
                ("aiRand15", flow.AiRand15),
                ("aiRand20", flow.AiRand20),
                ("aiRand3", flow.AiRand3),
                ("aiRand5", flow.AiRand5),
                ("aiStageTargetX", flow.AiStageTargetX),
                ("battlePauseOverlay", 0),
                ("battleSlotCount", roster.ActiveSlotCount),
                ("battleSlotEntity", battleSlotEntity),
                ("battleSlotOid", battleSlotOid),
                ("battleSlotState", battleSlotState),
                ("battleSlotTeam", battleSlotTeam),
                ("battleStepEarlyReturned", 0),
                ("battleStepFlag449048", 0),
                ("battleStepGate44905C", flow.BattleStepGate),
                ("battleStepMode", flow.BattleStepMode),
                ("boundLeft", 0),
                ("boundRight", stage.StageWidthPx),
                ("cameraMaxOverride", stage.CameraMaxOverride),
                ("cameraVel", _cameraVel),
                ("cameraX", _cameraX),
                ("difficulty", match.Difficulty),
                ("djaGuardGlobal44F224", flow.DjaGuardGlobal44F224),
                ("f8Pressed", false),
                ("frameMod12", flow.FrameMod12),
                ("frameToggle", flow.FrameToggle),
                ("gameMode", match.BattleGameModeId),
                ("gameMode2", match.LocalGameModeId),
                ("gameTick", flow.CurrentTickIndex),
                ("humanInputPolledExternally", false),
                ("initStats", 0),
                ("inputPhase", flow.InputPhase),
                ("needClearInput", false),
                ("objectCount", ObjectCount),
                ("paused", false),
                ("ppMode", PpMode),
                ("randomStage", match.BackgroundId),
                ("reserveCommittedHp", ZeroMatrix(2, 11)),
                ("reserveCommittedTotal", ZeroMatrix(2, 11)),
                ("reserveLiveCount", ZeroMatrix(2, 11)),
                ("reserveMissingCount", ZeroMatrix(2, 11)),
                ("reserveOidTable", new[] { 30, 31, 33, 34, 39, 32, 35, 36, 37, 122, 123 }),
                ("reserveOwnerValid", false),
                ("results", DictionaryOf(
                    ("battleEndPhase", (object)0),
                    ("hadBoth", false),
                    ("pendingHostAction", 0),
                    ("pendingWinner", -2),
                    ("phase", 0),
                    ("teamCount", 0),
                    ("teamIds", new[] { -1, -1 }),
                    ("timer", 0),
                    ("winner", -1))),
                ("runtime", runtimeDomain),
                ("stageAiInputCarrier", 0),
                ("stageIdx", progression.StageSeriesIdx),
                ("stageProgression", DictionaryOf(
                    ("round", (object)progression.Round),
                    ("roundMax", progression.RoundMax),
                    ("stageSeriesIdx", progression.StageSeriesIdx),
                    ("waveIdx", progression.WaveIdx))),
                ("stageProgressionValid", battle.StageProgressionValid),
                ("stageSpawnRuntimeEntryCount", CloneList(battle.StageSpawnRuntimeEntryCount)),
                ("stageSpawnRuntimeSlots", CloneNestedList(battle.StageSpawnRuntimeSlots)),
                ("stageSpawnRuntimeSpawnedTotal", CloneList(battle.StageSpawnRuntimeSpawnedTotal)),
                ("stageSpawnRuntimeTargetTotal", CloneList(battle.StageSpawnRuntimeTargetTotal)),
                ("stageSpawnRuntimeWave", battle.StageSpawnRuntimeWave),
                ("stageSpawnWaveApplied", battle.StageSpawnWaveApplied),
                ("stageSpawnWaveDeferredEntryApplied", battle.StageSpawnWaveDeferredEntryApplied),
                ("xMaxOverride", stage.XMaxOverride));
        }

        private object ProjectARestDomain()
        {
            var entries = new List<object>();
            for (int slot = 0; slot < MaxRuntimeSlots; slot++)
            {
                int value = FindEntityByRuntimeSlotIncludingDormant(slot)?.ItrRest?.Arest ?? 0;
                if (value != 0)
                    entries.Add(DictionaryOf(("slot", (object)slot), ("value", value)));
            }
            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectVRestDomain()
        {
            var entries = new List<object>();
            var victims = new LF2Entity[MaxRuntimeSlots];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int attacker = 0; attacker < MaxRuntimeSlots; attacker++)
            {
                for (int victim = 0; victim < MaxRuntimeSlots; victim++)
                {
                    int value = victims[victim]?.ItrRest?.GetVrest(attacker) ?? 0;
                    if (value == 0)
                        continue;
                    entries.Add(DictionaryOf(
                        ("attackerSlot", (object)attacker),
                        ("value", value),
                        ("victimSlot", victim)));
                }
            }
            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectEventsDomain()
        {
            var sounds = new object[PendingSounds?.Count ?? 0];
            for (int i = 0; i < sounds.Length; i++)
            {
                PendingSoundEvent sound = PendingSounds[i];
                sounds[i] = DictionaryOf(
                    ("cue", (object)(sound?.Cue ?? string.Empty)),
                    ("tick", sound?.Tick ?? 0),
                    ("worldX", sound?.WorldX ?? 0));
            }
            return DictionaryOf(("pendingSounds", (object)sounds));
        }

        private static int ResolveTraceCategory(int dataType)
        {
            return dataType switch
            {
                0 => 0,
                1 or 2 or 4 or 6 => 1,
                3 => 2,
                _ => 3,
            };
        }

        private static int[] CloneArray(int[] values)
        {
            return values == null ? Array.Empty<int>() : (int[])values.Clone();
        }

        private static int[] FilledArray(int count, int value)
        {
            var result = new int[count];
            if (value != 0)
            {
                for (int i = 0; i < result.Length; i++)
                    result[i] = value;
            }
            return result;
        }

        private static object[] ZeroMatrix(int rows, int columns)
        {
            var result = new object[rows];
            for (int i = 0; i < rows; i++)
                result[i] = new int[columns];
            return result;
        }

        private static int[] CloneList(List<int> values)
        {
            return values == null ? Array.Empty<int>() : values.ToArray();
        }

        private static object[] CloneNestedList(List<int[]> values)
        {
            if (values == null)
                return Array.Empty<object>();
            var result = new object[values.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = CloneArray(values[i]);
            return result;
        }

        private static SortedDictionary<string, object> DictionaryOf(
            params (string key, object value)[] values)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
            for (int i = 0; i < values.Length; i++)
                result[values[i].key] = values[i].value;
            return result;
        }
    }
}


--- File: Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md ---
# NTSD C# 工程 vs Unity 工程 — 战斗逻辑差异与对齐清单

> 创建日期：2026-07-12
>
> **唯一 gameplay authority**：`J:\QQFile\NTSD2.4\ntsd_release_C#`。战斗规则、pass 顺序、字段副作用和可观察行为只能以该 C# 工程为准。
>
> **核心入口**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore`。旧工程、反汇编和旧对齐结论只保留为历史记录，不得作为当前实现或验收依据。
>
> **历史表说明**：下文历史表中仍可能出现旧来源坐标；这些坐标只说明当时的追踪过程。若与唯一权威 C# 冲突，必须重新按 C# 审计并更新结论。
>
> **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
>
> 说明：
> - 本文只覆盖**战斗相关逻辑**：固定 tick/pass 顺序、输入与 AI、帧推进/状态、实体位移与逻辑 X 边界、碰撞/命中、武器/cpoint/opoint、死亡复活、波次和实体生命周期。菜单、选人、加载、HUD/结算、相机、背景/纯渲染、音频播放系统、网络、回放/回滚基础设施不在本清单内。
> - bg.dat 的 Z 可活动范围与相机/背景表现不对齐，Unity 保留自己的 BoundaryWall + ProCamera2D；但 `ApplyPreframeBounds` 中会改变实体存亡或 X 坐标的逻辑分支仍属于战斗逻辑，不能随表现层一起排除。
> - "冗余脚本可删除"的判定必须严格：**只有在 C# 无对应分支、且 Unity 自身也不引用时才可删**；若只是 Unity 换了一种架构实现同一件事（组合/resolver/partial），**不算冗余，不得删除**。
> - **最终表现效果一致原则（重要）**：对于因 Unity 框架/架构限制而**无法做到逻辑层完全对齐**的项，退而求其次的底线是——**运行时最终表现效果必须与 C# 工程完全一致**（位置、帧号、速度、判定结果、伤害数值、时序等对外可观测行为逐帧等价）。即"实现方式可不同，但结果必须等价"。凡标 🔷 的项，验收标准就是这条：不比对代码是否同构，而比对运行结果是否逐帧一致。
> - 标记含义：✅ 已对齐 / ⚠️ 部分对齐或存疑 / ❌ 缺失或明显偏差 / 🔷 架构不同但结果需等价 / 🗑️ 疑似可删（需二次确认）
> - **当前执行口径（Audit4）**：BATTLE-AUDIT4-01..16 的生产修复均已落地并经 Architect 最终复核 **PASS**。Unity Editor PID `11540` fresh script compile 为 **0 C# error**；`BattleRuntimeSelfCheck.cs` source/test `2026-07-17 01:39:46` < `Assembly-CSharp.dll` `09:26:23` < result `09:26:55`，fresh full self-check **PASS**。Naruto 防前跳螺旋丸、奔跑防跳完整后续招、投掷武器单次命中/Arest 三项目标 Play Mode 均已通过。上述证据关闭本批确认差异，但完整对局逐帧对拍与 RISK-4 仍未关闭，因此不能宣称全部战斗逻辑已经完全对齐。

---

## 0. 权威工程 BattleCore 结构 → Unity 映射总表

| C# BattleCore 文件 | 职责 | Unity 对应 | 映射类型 |
|--------------------|------|-----------|---------|
| `Simulation/GameTick.cs` | 单 tick 总调度（顺序主干） | `Simulation/NTSDBattleTickSystem.cs` + `SimulationWorld.Passes.partial.cs` | 🔷 pass 拆分 |
| `Simulation/NtsdBattleTickSystem.cs` | tick 外层入口 | `Simulation/NTSDBattleTickSystem.cs` | ✅ |
| `Simulation/SimulationWorld.cs` | 世界容器/对象池 | `Simulation/SimulationWorld*.cs` | 🔷 固定槽 vs 动态槽 |
| `Frame/FrameTick.cs` | frame_tick 帧推进 | `Character/FrameTransistor.cs` + `LF2Entity.RunCommonFrameTick` | 🔷 |
| `Frame/FrameAdvance.cs` / `Physics.cs` | 帧推进物理 | `Character/CharacterMechanics.cs` + `PhysicsState` | 🔷 |
| `Interaction/HitResolve.cs` | 命中结算（kind 0~16） | `LF2CharacterHitResolver.cs` + `LF2Weapon.ApplyHitEffects` + `LF2CharacterDatHitResolver` | 🔷 分散到多类 |
| `Interaction/CollisionCollect.cs` | 候选收集 | `Character/BruteForceSceneQuery.cs` | 🔷 |
| `Interaction/CPointRuntime.cs` | 抓取 cpoint | `LF2CharacterCatchResolver.cs` + `PreInteractionTickAll` | 🔷 |
| `Interaction/WeaponRuntime.cs` | 持武器同步/投掷/掉落 | `LF2WeaponHeldStateResolver.cs` + `LF2WeaponReleaseFlowResolver.cs` | 🔷 |
| `Interaction/ObjectPointFactory.cs` (`FrameTick.SpawnFromOpoint`) | opoint 生成 | `Character/LF2ObjectPointFactory.cs` | ✅ Naruto DDJ 生命周期差异修复后已验证 |
| `Input/InputRuntime.cs` | 输入消费 + AI | `Input/CharacterInputModule.cs` + `LF2Entity` shared-DAT 桥 | 🔷 |
| `Entity/Entity.cs` (大字段实体) | 实体真值 | `NTSDEntityRuntime.cs` + `LF2Entity` | 🔷 字段化 |
| `Entity/NtsdCharacter/NtsdWeapon/...` | 实体类别 | `LF2Character/LF2Weapon/LF2SpecialAttack/LF2OtherObject` | 🔷 |

---

## 1. Tick 主循环顺序（C# authority vs Unity pass）

C# `GameTick.Run` 是唯一正式顺序。Unity 拆成 `NTSDBattleTickSystem` 调度多个 `SimulationWorld` pass，两侧顺序必须逐段等价。

| # | C# 正式顺序 | Unity pass | 状态 |
|---|------------------------|-----------|------|
| 1 | `GameTick++` / `InputPhase` / `FrameMod12` / `FrameToggle` | `NTSDBattleTickSystem` + `BattleRuntimeState.Flow` | ✅ `AdvanceBattleFlowTick` 在 tick 头统一推进四项；state 400/401 读取持久化 `FrameToggle` |
| 2 | 清瞬时状态 `PendingSounds.Clear()` 等 | 战斗候选载体在 `EntityPostFrameTailAll` 清理 | 🔷 音频/overlay 瞬时状态排除；战斗候选清理已存在，仍随碰撞快照专项验收 |
| 3 | `RunCooldownsTick`（arest-- + attack_exempt 清理） | `VrestTickAll` + `ClearAttackExemptIfCurrentFrameCannotHit` | 🔷 |
| 4 | `GameTick.Run:61-62` `postCooldownInput` callback | `PostCooldownHumanInputAll` → `AiInputAndComboAll` | ⚠️ 历史自检曾通过；当前必须以 C# callback 契约重新核验 |
| 5 | `GameTick.Run:63-64` `RunOid5152RuntimeMaintenance`（实现见 `:1093-1263`） | `Oid5152RuntimeMaintenanceAll` + `TryMergeOid7Or8Into51` / `TrySplitOid51BackToPair` | ⚠️ 历史实现/self-check 保留；需按唯一 C# 权威重新审计 |
| 6 | `GameTick.Run:75-78` `ApplyCharacterInputPass` | Unity 输入 pass 拆分 | 🔷 以 C# 正式调用顺序判定等价性 |
| 7 | `RunEarlyStatePasses`（400/401/500/501） | `EarlyFrameAdvanceSpecialsAll` | ✅ 含 BMD-023 修复 |
| 8 | `FrameRuntimePasses.RunFrameLogic`（hit_fa>0 非角色） | `FrameLogicBeforeAdvanceAll` | ✅ |
| 9 | `RunFrameAdvance`（所有 active，清方向键 + 帧推进） | `SerialTickAll`（SimTransit+SimTU） | 🔷 |
| 10 | `RunPostFrameAdvanceStatePasses`（9998 清理 + 复活） | `CleanupState9998Entities` + `PostFrameAdvanceDeathCleanupAll` + `RunReleaseEntityCleanupTail` | ✅ 复活由 T5 完成并通过运行时自检 |
| 11 | `ClampCharactersToStageZ` | (Z 边界，属可活动范围) | 🚫 不对齐 |
| 12 | `RunCPoint` | `PreInteractionTickAll`→`RunCpointCheckStep10` | ✅ |
| 13 | `SyncHeldWeapons` | `RunWeaponSyncHeldStep10` | ✅ |
| 14 | `ValidatePositiveLinks` | `ValidateHeldLinksAll` | ✅ 全局扫描 active slot `0..399`；invalid 只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot`，不清 target 反向字段 |
| 15 | `RunHeldWeaponStep12` | `PreInteractionTickAll` 内 | ✅ |
| 16 | `SnapshotPrevFrame2` | `CaptureCollisionFrameSnapshotsAll` | ✅ |
| 17 | `CollectCandidates` | `CollectCollisionCandidatesAll` | ✅ |
| 18 | `ResolveCharacterHits` | `PostInteractionTickAll`（角色候选消费） | 🔷 |
| 19 | `RunNaturalRandomWeaponDrop` | `RandomWeaponDropTickAll` | ✅ |
| 20 | `RunF8WeaponDrop` | **未找到 F8 路径** | 🗑️? 调试功能，见 §7 |
| 21 | `ResolveObjectHits` | `ObjectInteractionTickAll` | 🔷 |
| 22 | `ApplyPreframeBounds`（含相机/bg） | `ApplyPreFrameBoundsAll`（只做逻辑边界） | 🔷 相机部分不对齐 |
| 23 | `ApplyCurrentWavePhaseAdvance` / `StageSpawns` | `CurrentWaveStageTickAll`（`SimulationWorld.StageWave.partial.cs`） | ✅ 已完成并通过 fresh Unity 运行时验收 |
| 24 | `ApplyFramePostProcess`（HitCount→Vx 平均） | `FramePostProcessAll` | ✅ |
| 25 | `RunLatePerEntityUpdatePass` | `LateEntityUpdateAll` | ✅ 主对齐点 |
| 26 | `RunMode2RandomWeaponDrop` | `Mode2RandomWeaponDropTailAll` | 🚫 C# baseline 的 F7-F9/debug 控制路径，不作为正式战斗对齐项 |
| 27 | `RunEntityPostframeTail`（heal/catch timer） | `EntityPostFrameTailAll` | ✅ heal/catch timer 与战斗候选载体清理已落地；`InitStats`/mode2 debug 分支排除 |
| 28 | `UpdateBattleResultsFlow` | (结算流程) | 🚫 非战斗运行时范围 |

**关键差异**：
- C# 是**固定 400 槽 `Objects[]` 线性遍历**；Unity 是**动态 runtime slot + SortedDictionary bucket**。这是 🔷 架构差异，结果需等价，遍历顺序必须仍是 slot 升序。
- C# `RunLateEntityUpdate` 单函数内顺序：`RunStateSpecialPreCollision → RegeneratePreCollisionStats → FrameTickRuntime.Tick → 帧组1100/1200 → 死亡掉武器/弹地 → ProcessOpointSpawn → 破武器回收 → RunN30InputTrigger → SpawnStateTransitionEffects → PrevFrame 镜像`。Unity `LateEntityUpdateAll` 已按同序拆分（✅），但 **`RegeneratePreCollisionStats`（HP/PP 自然恢复）** 的位置需核对（见 §5）。

---

## 2. 受击/命中结算（`HitResolve.cs` vs `LF2CharacterHitResolver` + `LF2Weapon`）

C# 把**所有对象**的命中都集中在 `HitResolve.ApplyCandidate`（一个 switch(kind)）。Unity 拆成三条独立路径：
- 角色被击 → `LF2CharacterHitResolver.ResolveHit`
- 武器被击 → `LF2Weapon.Hit` / `ApplyHitEffects`
- 非角色 DAT 实体 → `LF2CharacterDatHitResolver`

这是 🔷 架构差异（合法）。以下逐 kind 核对行为是否等价。

| kind | C# `HitResolve` 分支 | Unity 分支 | 状态 |
|------|---------------------|-----------|------|
| 0/4，以及预处理后的 9→0 → 伤害 | `ApplyDamageCandidate` | `ResolveHit` 普通伤害入口；raw kind9 先由 `BruteForceSceneQuery` 转为 kind0 | ✅ alternate 路径已补齐并运行验证，见下方逐点 |
| 6 | `victim.HitConfirm=3` | `HitConfirmEa=3` return | ✅ |
| 8 | `ApplyKind8`（heal_timer/传送） | `ResolveHit` kind 8 | ✅ |
| 10/11 | `ApplyKind10Or11`（笛子）：kind==11 && weaponCount>=0 return false；WeaponCount=FluteForce 值；Falling 双倍伤害 | `LF2CharacterHitResolver.cs:357-369`（✅）+ `LF2Weapon.cs:481-501`（✅） | ✅ |
| 14 | `ApplyKind14`（方向阻挡） | `ResolveHit` kind 14 + `ApplyKind14DirectionalBlockFrom` | ✅ |
| 15 | `ApplyKind15Movement`（KnockbackVx/Vx/Vz/YInt=-2，按对象类型分 vyStep=3.0/2.3） | `LF2CharacterHitResolver.cs:373-380` 简化实现；武器侧 `LF2Weapon.cs:503-506` `WhirlwindForce` | ⚠️ 形式不同（C# 走 KnockbackVx+真实 Vx/Vz+设 YInt=-2 三段；Unity 走 PS.vx/vz 增量；C# 按对象类型分 3.0/2.3 vyStep，Unity 未区分） |
| 16 | `ApplyKind15Or16` kind=16 路径：Hp-、KillStat++、ComboCountAtk、SFX_065、frame=200、vrest 写入、LinkState 断开 | `LF2CharacterHitResolver.cs:383-390`：`ImmediateFrame(MpDrain=200)` ✅ + MaxMP 缩放伤害 ✅；**缺** KillStat++、ComboCountAtk、SFX_065 音效、vrest 写入、LinkState 断开处理 | ⚠️ |
| 1/3 | `ApplyKind1Grab`/`ApplyKind3Grab` | 走 pre-interaction（`LF2CharacterInteractionResolver`） | 🔷 时序不同，见 §4 |
| 2/7 | `ApplyPickupCandidate` | pre-interaction | 🔷 见 §4 |
| kind 4+WeaponCount>0→0 + dvx 翻转 | `PreprocessCandidate` 154-172 | `BruteForceSceneQuery.cs:602-615` 完整实现（kind 翻转 + dvx 翻转按 PS.dir） | ✅ |
| kind 5 委托攻击 | `PreprocessCandidate`（holder wpoint 替换） | `ResolveHit` kind 5（TrackerParent） | ✅ |
| oid 300 特判 | `ApplyOid300SpecialHit` | `ResolveHit` `ObjectId==300` 分支（`LF2CharacterHitResolver.cs:279`） | ✅ |

### 2.1 kind 0/4/9 伤害主流程逐点核对

C# `ApplyDamageCandidate`（character victim）关键顺序：

1. `itrArest = (itr.Arest < 4 && itr.Vrest == 0) ? 4 : itr.Arest`（`HitResolve.cs:268`） — ✅ **C# 用 Arest 判定 + 取值**
   Unity 已由 `LF2Entity.ResolveArestCooldown` 统一实现同一公式，并供普通角色命中路径复用；`CheckArestCooldownRule` 已在 Unity batchmode 中通过。
2. IronBall victim → dvx/dvy 减半（`PreprocessCandidate`）— Unity 在 `LF2Weapon` 侧，角色路径无此（正确，角色不是 IronBall）
3. alternate 受击路径 — ✅ **已完整落地并通过 Unity 运行时自检**：
   - C# `ShouldUseAlternateHurt`（629-680）→ `ApplyAlternateDamage`（实际逻辑延续到约 line 827）。Unity 以共享 `LF2AlternateDamageResolver` 承载，真实 `LF2Character.Hit` 由 `LF2CharacterHitResolver` 接入，当前 DAT 为角色但 CLR shell 非角色的对象由 `LF2CharacterDatHitResolver.TryResolveHit` 接入；两条入口调用同一 `ShouldUseAlternateHurt` / `ApplyAlternateDamage`，并各自只记录一次 `RecordKind0Hit`。
   - `ShouldUseAlternateHurt` 已覆盖 oid 37/6/52 的 `HitStateCount`/frame 窗口、heavy effect、attacker oid 214/208，以及 `PrevFrame2` state 7 的 HP、`bdefend`、朝向、负 `dvx` 和特殊攻击者判定。
   - 伤害契约为 `FallDamageDiv` 整数换算后 `reducedInjury = injury / 10`；扣 `HP`，`HPBound -= reducedInjury / 3`（整数除法），不累计 `HPLost`。致死与统计副作用使用 holder-copy 的 `KillStat`/`ComboCountAtk`、victim `ComboCountVic`，并以 `Unk344` 索引稳定 3 槽 `KillStats`/`DamageStats`；世界 reset 保持数组 identity 并清零内容。
   - 其余已覆盖 `Fall=80`、hit/attacking 计数、attacker/victim/negative-link holder 的 FrameDelay、attacker-only AttackExempt、vrest clamp、frame 111/112 保留 wait counter、ground/air knockback、state 1002/2000/3000 尾分支。state1002 随机切帧只改 frame/速度，不额外写 `Runtime.WeaponState`；状态判断继续以当前 `Frame.D.state` 为准。
   - heavy weapon 普通伤害的减半发生在 alternate 判断之后，因此 alternate 始终消费原始 itr，不会错误变成 `injury/20`。`ApplyAlternateDamage` 本身也保留 character DAT/type guard，不能被非角色 victim 直接调用。
   - **raw kind9 不直接触发 alternate**：真实角色与 shared-character-DAT 两个 caller 都以 `itr.kind != 9` 为门；raw kind9 必须先由 `BruteForceSceneQuery.ResolveRuntimeItrForPair` 转换为 kind0，才会在非 kind9 普通伤害入口判断 alternate。`LF2SpecialAttack` 也统一在 object interaction pass 使用这条预处理，覆盖 kind4 的 `WeaponCount`/反向 `dvx`（读取逻辑真值 `Dirh()`/`Runtime.Vx`）和 kind9 的 kind0 转换/攻击者 HP 清零。
   - alternate 已写入的 clamp 后 vrest 不会再被角色 DAT、武器或技能对象外层 generic rest 更新覆盖。type3（`Consumable3`/Unity `SpecialAttack`）lead sound 条件已按权威修正；该声音分支属于代码权威对齐，headless 自检无法直接观测音频播放。
   - 针对性自检：`CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess`；均包含在 2026-07-14 02:54:22 的 fresh Unity batchmode PASS 中。
4. fall 累积档位（Light/Medium/Heavy/Fall 阈值 → frame 220/222/224/226/180/186）— Unity `HitFall`/`HitFallDown` ✅ 已对齐（注意 5f/7f→0.714 修复已在）
5. `victim.HitStateCount = 45` → Unity `SetHitStateCount(45)` ✅
6. `attacker.FrameDelay=3 / victim.FrameDelay=-3` 普通路径 — Unity 多处 `-3` ✅；alternate 路径独立写 `victim.FrameDelay=-5`，并传播 negative-link holder delay ✅。
7. 攻击方攻击豁免写入 — Unity `attackerLiving?.HitCounters?.SetAttackExempt(exemptVal)` ✅（公式按点 1 修正）

### 2.2 武器被击（`LF2Weapon.ApplyHitEffects` vs `HitResolve.ApplyObjectHurtTail`）

Unity `LF2Weapon.ApplyHitEffects` 已注明"C# baseline: ApplyObjectHurtTail + ApplyStandardDamageKnockbackX"，逐段抄写。核对：
- `FallCounter += fall!=0?fall:20` ✅
- `lightThrow||heavyLike||specialLike → FallCounter=80` ✅
- ApplyStandardDamageKnockbackX 五分支（固定5 / state2000+dvx / FlyingA/B scaled / effect22/23 / 常规）✅
- knockback 帧 180/186 + KnockbackVy ✅
- 攻击者 state 1002 反弹 / state 2000 减速 / state 3000 归 frame 10 — Unity `ApplyAttackerResponse` ✅

**✅ `RecordKind0Hit` 已统一**：`LF2Entity.RecordKind0Hit` 承载 C# timer、owner、随机坐标和 10 槽上限语义，角色与 `LF2Weapon.ApplyHitEffects` 的 kind0 路径均接入；`CheckKind0HitRecords` 已在 Unity batchmode 中通过。

---

## 3. 帧推进（`FrameTick.cs` vs `FrameTransistor` + `RunCommonFrameTick`）

C# `FrameTick.Tick` 是单函数，Unity 拆成 `FrameTransistor.Trans()`（wait/next 推进）+ `LF2Entity.RunCommonFrameTick`（前置门控 + 倒计时）+ hook（`OnFrameTickBeforeWaitAdvance` / `OnFrameTickAfterWaitAdvance`）。

| C# `FrameTick.Tick` 步骤 | Unity | 状态 |
|--------------------------|-------|------|
| `ThrowFrameGuard==Frame` early return | `RunCommonFrameTick` 门控 | ⚠️ 需确认 |
| `FrameDelay!=0 && !Consumable3` return | ✅ | ✅ |
| `AttackExempt--` | ✅ | ✅ |
| `LinkState<0` return | ✅ | ✅ |
| cpoint kind==2 return | ✅ | ✅ |
| Consumable3 + hitA>0 → HP-=hitA, HP<=0 跳 hitD | `LF2Entity.RunCommonFrameTick` type3 分支 | ✅ |
| HitStop/Fall/HitStateCount/HitConfirm 倒计时 | `RunCommonFrameTick` | ✅ |
| frame!=waitCounter → 音效+attacking=0 | `FrameTransistor.Trans` frame 变化清 attacking | ✅ |
| `attacking++` | `Trans.AttackingCounter++` | ✅ |
| state 0 + YInt<0 → frame 212 + SuppressJumpInit | `OnFrameTickBeforeWaitAdvance` | ✅ BMD-023 相关 |
| IronBall state 2000 静止 return | `LF2Weapon.ApplyObjectSpecificFrameTickBeforeWaitAdvance` | ✅ |
| state 14 HP<=0 → HitStop=30 | `RunCommonFrameTick` | ✅ |
| state 2000 facing=vx | ✅ | ✅ |
| `attacking>wait` → next 换帧 | `Trans` attacking>wait | ✅ |
| next=999 → 212/0（空中角色） | `ResolveFrameTickNext999Target` | ✅ |
| next<0 翻面 | `Trans` switchDir | ✅ |
| 上一帧 state14→非13 的 HitStop=15 逻辑 | `OnFrameTickAfterWaitAdvance` | ✅ 含 oid/5==3 skip + difficulty 分支 |
| frame 212 + JumpInitPending → 跳跃初速 | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame mp<0 PP 扣费 + hitD turn | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame 110/114 → CdDefendLock=3 | `RunCommonFrameTick` 尾 | ✅，`CheckFrameTickDefendLockTail` 运行通过 |
| frame 202 → HitStop=20 | ✅ | ✅ |

**结论**：帧推进主干及上述 state14、frame mp、110/114、202 尾部特判均已核实对齐（🔷 hook 拆分合法）。

**逐点核实结果（§3 全部）：**
- §3-1 state14 入口 HitStun=30 + AttackingCounter=0（KillCount>=0 OR Unk364==5 OR slot>=20）— Unity `LF2Character.cs:2205-2211` ✅ **完整对齐**
- §3-2 state14→非13 复活 HitStun=15 分支（aiControlled 检查 + Difficulty!=2 + oid/5==3 + GameMode==1/4 + Oid!=38）— Unity `LF2Character.cs:2134-2163` `ApplyCommonCaughtExitHitStop` ✅ **完整对齐**
- §3-3 frame mp turn-around（C# `HitResolve.cs:178-203`）— Unity `LF2Entity.cs:3284-3321` `ApplyCommonFrameTickPpDisplayPostAdvance` ✅ **完整对齐**（含 PP 扣费、frame.hitD turn、Dual KeyLeft/Right + Facing + YInt==0 条件）
- §3-4 frame==202 → HitStun=20 — Unity `LF2Entity.cs:3634-3635` ✅
- §3-5 frame==110 || frame==114 → `CdDefendLock=3` — Unity `LF2Entity.RunCommonFrameTick` 尾部已实现，runtime Reset/cooldown 衰减已承载；`CheckFrameTickDefendLockTail` 已运行通过 ✅

---

## 4. 交互（pre/post-interaction, cpoint 抓取, opoint）

### 4.1 命中候选消费时序差异（重要）

C# 在 `HitResolve.ApplyCandidate` 里**同一个 switch** 同时处理攻击(0/4/9)、抓取(1/3)、拾取(2/7)。Unity 分成两个阶段：
- `PostInteractionTickAll` → 角色候选消费（攻击 + pre-interaction 混合，`LF2CharacterInteractionResolver.TryConsumeUnifiedStep7CandidateSequence`）
- `ObjectInteractionTickAll` → 武器/技能候选消费

🔷 这是合法架构差异，但 **候选序列消费顺序必须与 C# 一致**（按 step6 收集顺序）。Unity 已用 `TryGetCollisionCandidateSequence` 保序 ✅。

### 4.2 抓取 cpoint

| C# | Unity | 状态 |
|----|-------|------|
| `ApplyKind1Grab`/`ApplyKind3Grab`（命中即建立） | `HandlePreInteractionKind`（pre-interaction 建立） | 🔷 时序不同 |
| `AlignGrabPair`（对位公式 centerx/wact/lerp） | `ApplyImmediateCatchPairState`（同公式） | ✅ 公式一致 |
| `CPointRuntime.Run`（step10 维护） | `RunCpointCheckStep10` + `RunCpointMismatchTailStep10` | ✅ |
| cpoint kind==2 受击 fronthurtact/backhurtact | `ApplyCaughtVictimHurtFrame` / `TryCaughtA` | ✅ |
| throwvx/vy/vz 投掷 + throwinjury | `LF2CharacterCatchResolver`（自检覆盖） | ✅ 有 BattleRuntimeSelfCheck |

### 4.3 opoint 生成 — ✅ 已在 `skill_release_flow_comparison.md` 验证一致

`FrameTick.ProcessOpointSpawn` / `SpawnFromOpoint` vs `LF2ObjectPointFactory.ProcessOpointSpawn` / `ProcessOneLateOpoint`：条件（kind>0 && oid>0 && attacking==0 && (角色→FrameDelay==0)）、facing 展开（>10 → count/facing）、多发 AttackExempt+VRest 扩散、state 3003 linked slot vrest — 均已对齐。

**2026-07-16 Naruto DDJ 完整链专项回归：** 既有 combo wrapper 测试只能证明输入可跳到技能起始帧，不能证明递归 opoint 和对象池生命周期正确。本次按真实 DAT/authority 链新增端到端断言：

- 同 tick held chord 内部输入 `att + down + def` 命中 Naruto frame `271`；随后 frame `272` 生成 oid205/action98，辅助链继续经过 99/325/341；frame `273` 生成 oid204/action130，并展开六个分支，最终各自到 frame `147` 生成 `6 x oid33/action307`。clone 从 307 后落地进入 frame `219` 是 authority 行为，不应把 219 误判为生成失败。
- 新确认差异 1：`LF2ReferencePool.Release` 无条件接收外部 synthetic 实例，造成逻辑池类型污染。
- 新确认差异 2：factory 角色 opoint 在 `ModuleBind` 注册前用 `slot < 0` 过早拒绝，合法生成会被提前丢弃。
- 新确认差异 3：tick 中 pending-unregister 对象同 tick 归池复用时，旧生命周期仍留在 registry bucket；后续 `Register` 被旧 bucket 的 `Contains` 拒绝，递归六分支只生成 3 个 clone。
- 新确认差异 4：池化 `LF2Character.Init` 没有重新分配 `StableId`，复用角色无法保持独立生命周期身份。
- 新确认差异 5：`SpawnFromOpoint` 缺少 `RelationTeam`、`Unk364` 与 holder-copy 继承，生成角色的关系字段与 authority 不完整。
- 已修复契约：`Release` 只把 active 实例归池；`Register` 先 finalize 旧 pending lifecycle；`slot < 0` guard 移到 `ModuleBind + Initialize` 之后；character `Init` 重新 `AllocateStableId`；`PostInitLiving` 继承 `Team`、`RelationTeam` 与 holder-copy（含 `Unk364`）。
- 回归结果：PP `500 -> 295`，所有生成对象使用 dynamic slot，6 个 clone 拥有 6 个唯一 `StableId`，均实际到达 action/frame `307`，且 6 个 renderer 均可见。

**真实 Unity Play Mode 生产输入链验收：** 在 `NTSD_Battle` Play Mode 中等待 slot0 的 `CharacterInputModule`/`ActionMap` 就绪，再通过 UnityMCP 临时 `InputSystem.Keyboard` 事件按默认物理绑定依次注入 `L (Defend) -> S (Down) -> K (Jump)`。事件真实经过 `InputActionMap -> CharacterInputModule -> SimInputBuffer`，没有直接调用技能、写帧或调用 opoint。观测日志为 `INPUT focused=True buffered=1, attackAction=0, jumpAction=1, defendAction=1, moveY=-1`；这里的 crossed internal mapping 是项目/C# baseline 的预期映射，不是错误。运行结果：

- `frame271=True`，`max204=11`，`max205=3`，`maxClones=6`，`maxSpriteReady=6`，`maxVisible=6`。
- clone 数量时间线：`t=0.446: 3`、`t=0.473: 4`、`t=0.509: 5`、`t=0.541: 6`；测试窗口无异常。
- 峰值截图：`Temp/naruto-ddj-unitymcp-peak.png`。
- 验收限制：Win32 `keybd_event` 不被 Unity RawInput 接收，因此本次不是物理硬件键盘证明；它证明的是 UnityMCP `InputSystem.Keyboard` 事件通过完整生产输入链可以稳定释放真实六分身技能。

---

## 5. HP/PP 自然恢复 + heal/catch timer

**✅ HP/PP 自然恢复语义对齐**（逐字段核实）：
- C# `RegeneratePreCollisionStats`（`GameTick.cs:1474-1519`） vs Unity `LF2Character.cs:2534-2584`：
  - HP `Hp < HpMax`（HP < HPBound）每12tick+1 ✅
  - `hpForRate = Hp; >500 → 500; oid 51/52 /=2; PP += (500-hpForRate)/100+1` ✅
  - `WeaponCount<0` 每12tick 扣血（injury=900/FallDamageDiv）✅，HP -= injury、HPBound -= injury/3、ComboCountVic += 9 ✅
- 字段映射：`HpMax`↔`HPBound`、`Pp`↔`PP`，通过 `Runtime.HpMax` / `Health.HPBound` / `Runtime.Pp` / `Health.PP` 字段映射。
- 调用入口：Unity `RunPreCollisionRecoveryPhase` 虚函数（`LF2Entity.cs:972` + `LF2Character.cs:2619-2622`），由 `SimulationWorld.Passes.partial.cs:264` 调用。✅

**heal/catch timer（C# `RunEntityPostframeTail`）**：Unity `EntityPostFrameTailAll` 覆盖 HealTimer/CatchTimer/state1700 ✅（之前已确认）。

---

## 6. 输入 + AI

### 6.1 玩家输入消费（`InputRuntime.ApplyCharacterInput` vs `CharacterInputModule` + `LF2CharacterActionResolver`）

C# `ApplyCharacterInput` 单函数：combo wrapper → hitA/hitD/hitJ frame jump → frame110 facing → state 301/19 lane → LinkState2 heavy → frame215 landing → frame182/188 recovery → state 0/1/2/4/5 分发 → ApplyFrameVelocityTail。

Unity 有两套：
- `LF2Character` → `LF2CharacterActionResolver`（完整角色输入）
- `LF2Entity` shared-DAT 桥（`RunSharedCharacterDatStandingActionInputPhase` 等，用于"当前 DAT 是角色但 CLR 实例不是 LF2Character"的 transform 后对象）

🔷 合法架构分层。**注意**：shared-DAT 桥自称"最小实现"，只覆盖 standing/walking/running/dash/jump 基础，**不覆盖 combo/catching/held-weapon 全动作**。这不是冗余 —— 它服务 transform（state 501/4000/8000）后仍挂在 wrong shell 的角色。

关键值对齐（已修复）：
- walk 斜向 `Vx *= 5.0/7.0` = 0.7142857142857143 ✅（两侧都是）
- heavy run 斜向 `Vx *= 5f/6f` / `0.8333...` ✅

**✅ combo wrapper（DJA 等 9 组方向+攻击/跳连招）已落地并补 fresh 运行时验证**：Unity 现已由 `NTSDInputStateModule` 承载 9 组 wrapper 与 oid6（Sasuke）DjaGuard 特判，真实输入消费路径是 `LF2Character.RunPostCooldownInputPhase -> UpdateLocalInputStateFromControllerBuffer -> ComboUpdate -> NTSDInputStateModule.ApplyFrameInput`。本轮新增 `BattleRuntimeSelfCheck` 覆盖 9 组连招帧跳与 oid6 guard hold/release，`Temp/NTSD_BattleRuntimeSelfCheck.result` fresh 返回 `PASS`。

### 6.2 AI（`InputRuntime.PrepareAiInputBasic`）

**✅ AI 输入生成器已完整落地并通过 fresh Unity 运行时验证**：
- C# `InputRuntime.cs:16` `PrepareAiInputBasic`（~600 行巨型函数，oid 专属 combo 决策、C8 威胁扫描、7A/7B 守卫、队友守卫、held weapon 决策、历史闸门、oid1/4/5/33/52 多种 oid 专属 combo）。
- 实际包含 14 个辅助函数（已 grep 确认）：
  - `AiBetweenX`、`AiPostCacheCoordinateAllowsSpecial`、`AiPreUpdateTarget3000SideEffect`
  - `AiUpdateOid33_19_16PredictedDuaDecision`、`AiUpdateOid52_1_2_21PreLabel591Decision`
  - `AiUpdateLabel591Oid51_2_18_7Decision`、`AiUpdateFirstDecision`、`AiUpdateTeammateGuardDecision`
  - `AiUpdateOid1ComboDecision`、`AiUpdateCloseOid1Decision`、`AiUpdateOid4ComboDecision`、`AiUpdateOid5ComboDecision`
  - `AiProcessSubOidGroup`、`AiSpecialOidForSubGate`、`AiProcessHelper`
- Unity `SimulationWorld.AiInput.partial.cs` 已覆盖主入口及文档原先漏列的 target/team/move-mode/no-target/三个 `AiProcessSub*` 等完整直接/间接 helper 闭包。
- 输入 pass、runtime 字段、deterministic RNG、runtime-slot 顺序、shared-DAT shell 与 roster/opoint bootstrap 均已接通；fresh build 0 errors，fresh Unity batch 自检通过。

---

## 7. C# 有、Unity 未确认/缺失的战斗逻辑（重点排查项）

| 编号 | C# 逻辑 | 位置 | Unity 状态 | 判定 |
|------|---------|------|-----------|------|
| M-1 | **oid 7/8 → 51 合体 / 51 拆分**；唯一权威为 C# `GameTick.cs:1093-1263` | `GameTick.Run:61-64` 的 `postCooldownInput` 后、early specials 前 | `NTSDBattleTickSystem` / `SimulationWorld.Passes` / `NTSDEntityRuntime` / `BattleRuntimeSelfCheck` 有历史实现与自检 | **⚠️ 历史验证保留；需按 C# 重新审计（T4）** |
| M-2 | **复活 pass**（`RunRespawnPass` `GameTick.cs:839-934`：state14+HP<=0 + HitStop 窗口 + 两分支[Hp2Overlay/RespawnCount] + 队友位置平均 + Pp=500/HpMax=Hp3 + Frame=212/YInt=-300 + 生成 oid998 复活特效） | GameTick step10 | ✅ `SimulationWorld.Passes` / `BattleRuntimeSelfCheck` 主逻辑与样例已落地；已补 no-renderer 销毁注销链与 reference-pool 惰性初始化 | **✅ 已完成 / Unity 运行时已验证（T5）** |
| M-3 | **N30 输入触发**（`RunN30InputTrigger`：input history 9/0/9/0→触发码 100/102/104 生成 998 + history gate 广播） | LateEntityUpdate | ✅ `RunLateCharacterDatInputTrigger`（LF2Entity） | ✅ 已移植 |
| M-4 | **状态转换特效**（`SpawnStateTransitionEffects`：state13/frame200 退出 + state18/19 燃烧特效） | LateEntityUpdate | ✅ `SpawnLateTransitionEffects` | ✅ |
| M-5 | **死亡弹地帧**（`ApplyDeathBounceFrame`：frame186 + Vy=-3） | LateEntityUpdate | ✅ `RunLateDeathOpointPreCleanupPhase` 已对齐并由 `CheckLateDeathBounceFrame` 覆盖 | **✅ 已完成 / Unity 运行时已验证（提交 `995c860b`）** |
| M-6 | **F8 强制掉武器**（`RunF8WeaponDrop`） | GameTick | ❌ grep `F8/force drop` 0 命中 | 🗑️ **确认是调试功能，可不移植** |
| M-7 | **kind 4 + WeaponCount>0 → kind 0 + dvx 翻转**（`PreprocessCandidate` 154-172） | HitResolve | ✅ `BruteForceSceneQuery.cs:602-615` 完整实现 | ✅ 已对齐 |
| M-8 | **ShouldUseAlternateHurt / ApplyAlternateDamage**（injury/10 减伤 + KnockbackVx 特殊累积 + FrameDelay=-5） | HitResolve 629-约827 | ✅ 共享 `LF2AlternateDamageResolver`；`LF2Character.Hit` 与 shared-character-DAT resolver 两入口均接入；runtime/stat/运动尾契约均有自检 | **✅ 已完成 / Unity 运行时已验证（T1）** |
| M-9 | **RecordKind0Hit**（命中记录锚点 + spark，武器命中也调用） | HitResolve 1150 | ✅ `LF2Entity.RecordKind0Hit` 统一角色/武器 kind0 记录 | **✅ 已完成 / Unity 运行时已验证（T2）** |
| M-10 | **oid300 特殊命中**（bdy.x>1000→帧号） | HitResolve | ✅ `ResolveHit` ObjectId==300（`LF2CharacterHitResolver.cs:279`） | ✅ |
| M-11 | **state 400/401 传送**（最近敌/最远友） | GameTick early | ✅ `RunEarlyTeleportSpecialsPhase` | ✅ |
| M-12 | **state 500/501 变身 transform** | GameTick early | ✅ `RunEarlyState500/501Specials`（BMD-023） | ✅ |
| M-13 | **stage 波次生成**（`ApplyCurrentWavePhaseAdvance` `GameTick.cs:2317` + `ApplyCurrentWaveImmediateStageSpawns` :2350 + `RefillCurrentWavePositiveStageSpawns` :2226，StageProgression/StageSpawnRuntime 一整套） | GameTick step 23 | ✅ `BattleStageCampaignLoader` / `ApplyMatchConfig` 生产接线 + progression + spawn/refill/advance/bound + identity/dynamic-slot 契约已落地 | **✅ 逻辑与接线已完成 / Unity 运行时已验证；默认 `stage.dat` 部署由用户明确暂缓，不进入当前 backlog（T8）** |
| M-14 | **frame 110/114 → CdDefendLock=3**（`FrameTick.cs:208-209`） | FrameTick 尾 | ✅ `LF2Entity.RunCommonFrameTick` 尾部 + runtime Reset/cooldown | **✅ 已完成 / Unity 运行时已验证（T3）** |
| M-15 | **kind 16 完整结算**（`ApplyKind15Or16` kind=16：KillStat++/ComboCountAtk/SFX_065/vrest/LinkState 断开） | HitResolve 1640-1704 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已补齐 FallDamageDiv 缩放、KillStat/ComboCount、frame200、vrest、2/-2 持有断开与 SFX_065 | **✅ 已完成 / Unity 运行时已验证（T6）** |
| M-16 | **kind 15 完整位移**（`ApplyKind15Movement`：KnockbackVx+真实 Vx/Vz+YInt=-2，按对象类型分 vyStep 3.0/2.3） | HitResolve 1737 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已改为 authority 的 KnockbackVx/Vz + YInt/Vy 语义；武器/铁球侧原 `WhirlwindForce` 保持 3.0/2.3 分支 | **✅ 已完成 / Unity 运行时已验证（T6）** |

> **判定原则提醒**：当前仍标 ❌/⚠️ 的项目都**不能直接删对应 Unity 脚本**；它们是"C# 有 Unity 缺/结果仍需验证"。M-1/M-2/M-7/M-8/M-9/M-10/M-11/M-12/M-13/M-14/M-15/M-16 已确认对齐或完成并运行验证。只有 M-6（F8 调试）确认是调试功能后可不移植。

---

## 8. 判定为"架构不同但等价"的项（🔷 — 不得当冗余删除）

以下 Unity 代码看似"多出来"，实为 Unity 框架下实现 C# 同一逻辑的必要产物，**严禁因为 C# 没有同名文件就删除**：

| Unity 脚本/机制 | 对应 C# 逻辑 | 说明 |
|-----------------|-------------|------|
| `LF2Character*Resolver.cs`（Hit/Catch/DamageState/Action/Interaction/State/WeaponLink） | `NtsdCharacter` + `HitResolve`/`CPointRuntime`/`InputRuntime` 各段 | 组合模式拆分，逻辑等价 |
| `LF2AlternateDamageResolver` + `LF2CharacterDatHitResolver` | `HitResolve.ShouldUseAlternateHurt` / `ApplyAlternateDamage` | alternate 真值集中一次实现，由真实 `LF2Character.Hit` 与 shared-character-DAT 两入口复用 |
| `LF2Weapon*Resolver.cs`（Interaction/HeldState/ReleaseFlow/FrameLogic） | `WeaponRuntime` 各段 | 同上 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 中"当前 DAT 是角色"的分发 | 服务 transform 后 wrong-shell 角色，C# 因为是纯数据 Entity 不需要 shell 概念 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | Unity 运行时化，字段一一对应 |
| `FrameTransistor` hook（OnFrameTickBeforeWaitAdvance 等） | `FrameTick.Tick` 内联步骤 | 拆成 hook 供子类覆写 |
| `SimulationWorld` 动态 runtime slot | `Objects[400]` 固定槽 | Unity 用对象池，遍历顺序需保持 slot 升序 |
| `RefreshRuntimeSnapshot` 调用 | `CharacterSync.SyncRuntimeFromLegacy` | Unity 每 pass 后刷快照 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023：区别于 `ImmediateFrame`（会清 attacking） |

---

## 9. 不需要对齐的部分（明确排除）

| 项 | C# 位置 | 原因 |
|----|---------|------|
| 可活动范围 / Z 边界钳制 | `ApplyPreframeBounds` Z 段、`ClampCharactersToStageZ`、`Bg.ZBoundary*` | 用户明确：bg.dat 可活动范围不对齐，Unity 用 BoundaryWall |
| 相机 | `UpdateCameraAndBgAnimation`、`CameraX`/`CameraVel` | 用户明确：相机不对齐，Unity 用 ProCamera2D |
| bg 层动画 | `layer.AnimCounter` | 背景表现 |
| 结算界面 | `RunResultsTick`、`UpdateBattleResultsFlow` | 非战斗运行时（菜单/结算） |
| SDL/Host/音频桥 | `src/Host/*` | C# EXE 适配层 |
| 数据加载 | `src/Data/*` | Unity 用自己的 DatParser |

---

## 10. 对齐优先级清单（已全部逐行核实，✅=已核实定性）

### P0 — 已修复并完成 Unity 运行时验证
- [x] **§2.1-1 / T0** `exemptVal` 公式 — **已修复并通过 Unity 运行时自检**：`LF2Entity.ResolveArestCooldown` 与 `LF2CharacterHitResolver` 已按 arest/vrest 权威公式处理
- [x] **§2.1-3 / M-8 / T1** ApplyAlternateDamage — **已完成并通过 Unity 运行时自检**：共享 `LF2AlternateDamageResolver` 覆盖约 line 827 的完整权威契约；真实 `LF2Character.Hit` 与 shared-character-DAT resolver 两入口、`Unk344`/统计数组/`HPBound`、heavy/rest/preprocess/state tail 均有针对性检查

### P1 — 已补齐并完成 fresh Unity 运行时验证
- [x] **M-1 / T4** oid 7/8→51 合体拆分 — 历史实现和 self-check 覆盖 gate matrix、oid8 镜像、identity/presentation、human+AI DJA full-tick、split reset 与外部 `ItrRest`；**当前需按唯一 C# 权威 `GameTick.cs:1093-1263` 重新审计，历史来源不能作为完成依据**
- [x] **M-2 / T5** 复活 pass（`RunRespawnPass` 完整逻辑）— **已完成并通过 fresh Unity 运行时自检**
- [x] **M-13 / T8** stage 波次生成（`ApplyCurrentWaveXxx` 整套）— **逻辑与生产接线已完成并通过 fresh Unity 运行时自检；默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进**
- [x] **P1 / BOUNDS-X** PreFrame 实体 X clamp/free — **已完成并通过 physical worktree fresh Unity 运行时自检**：base `bg.width` 与 phase override 分离、current-DAT 分派、`RelationTeam`/`HitStop`/`Unk344`/`YInt`/严格边界与 `XInt` 契约均有矩阵覆盖

### P1 — 已确认缺失战斗逻辑（需新增）
- [x] **§6.2 AI / T9** `PrepareAiInputBasic` 完整调用闭包 — **已完成并通过 fresh Unity 运行时自检**

### P1 — 已确认对齐（无需动作）
- [x] **M-7** kind4+WeaponCount>0→0 dvx 翻转 — ✅ `BruteForceSceneQuery.cs:602-615`
- [x] **M-9 / T2** 武器命中 spark（`RecordKind0Hit`）— **已完成并通过 Unity 运行时自检**（角色与武器 kind0 路径统一记录）
- [x] **§5** HP/PP 自然恢复 + HpMax/HPBound — ✅ 逐字段对齐
- [x] **kind 10/11 笛子** ✅、**kind 14 方向阻挡** ✅、**oid300** ✅、**kind 5 委托** ✅

### P2 — 帧推进尾部特判（已核实）
- [x] **§3-1/§3-2** state14 复活 HitStop（oid/5==3 + difficulty 分支）— ✅ 完整对齐（`LF2Character.cs:2134-2163 / 2205-2211`）
- [x] **§3-3** frame mp turn-around — ✅ 完整对齐（`LF2Entity.cs:3284-3321`）
- [x] **§3-4** frame 202 HitStun=20 — ✅（`LF2Entity.cs:3634`）
- [x] **M-14 / T3** frame 110/114 CdDefendLock=3 — **已完成并通过 Unity 运行时自检**

### P2 — 已补齐并完成 Unity 运行时验证
- [x] **M-15 / M-16 / T6** kind 15/16 完整位移与副作用 — **已完成并通过 Unity 运行时自检**

### P3 — 确认可不移植
- [x] **M-6** F8 强制掉武器 — ✅ 确认是调试功能，Unity 不需实现（非冗余，是未移植的调试项）

### 二次审计战斗差异收口（2026-07-15）

> 本表只列会改变战斗模拟结果的项目。UI/HUD、camera/background/render、audio playback、network、replay，以及 F7-F9/debug 路径均不进入 backlog。`stage.dat` 默认资产部署由用户明确暂缓，也不进入本轮推进。
>
> 计数单位是“差异簇”而不是原子代码点；例如 INPUT-8 同时包含 shared-DAT running 的提前返回和缺 defend 分支。下表是 **Audit2 历史记录**；其中旧来源坐标不得用于当前实现，任何后续复查都必须回到 `ntsd_release_C#`。当前执行状态以 Audit4 为准。

#### 已确认差异簇（14/14 已修复并通过新增自检）

| 编号 | 差异 | Unity 证据 | Authority 证据 |
|---|---|---|---|
| INPUT-1 | state 7 `Defending` 被加入正式输入 state switch；authority switch 只分发 0/1/2/4/5 | `LF2CharacterActionResolver.cs:54-81` | C++ `input_handler.cpp:3018-3043`；C# `InputRuntime.cs:718-735` |
| INPUT-2 | jump 输入门槛读取 `PS.y`/浮点 Y，authority 使用 `YInt`；real character 与 shared-DAT 路径均需统一 | `LF2CharacterActionResolver.cs:61-68`；`LF2Entity.cs:1529` | C++ `input_handler.cpp:3032-3035`；C# `InputRuntime.cs:728-730` |
| INPUT-3 | state 301/19 的纵向移动门槛读取 `PS.y`，authority 使用整数 Y 门槛 | `LF2CharacterActionResolver.cs:503-516` | C++ `input_handler.cpp:2895-2903`；C# `InputRuntime.cs:680-685` |
| INPUT-4 | 正式 battle input pass 调用 `RunPostCooldownInputPhase` 后没有执行当前帧 `dvx/dvy/dvz` tail；唯一 tail 留在当前无生产调用者的 `RunCharacterInputPhase` | `SimulationWorld.Passes.partial.cs:54-63`；`LF2Character.cs:750-779` | C++ `input_handler.cpp:3045-3090`；C# `InputRuntime.cs:737,1463-1510` |
| INPUT-5 | `CdDefendLock` 同时由 Runtime 与 `NTSDInputStateModule` 持有/衰减/回写，存在双状态源不同步 | `SimulationWorld.Passes.partial.cs:920-928`；`NTSDInputStateModule.cs:75-111,165-174,408-436`；`LF2Entity.cs:1188-1196` | authority 仅有实体 input runtime 单一字段 |
| INPUT-6 | Super Punch 分支提前清零 `HitConfirmEa`；authority 在这里只读取命中确认并切帧 | `LF2CharacterActionResolver.cs:92-104`；shared-DAT `LF2Entity.cs:1269-1281` | C++ `input_handler.cpp:2371-2379`；C# `InputRuntime.cs:942-953` |
| INPUT-7 | `ImmediateFrame` 统一清零 `AttackingCounter`，把 authority 的 raw frame write 和计数副作用合并，影响多个输入动作跳帧 | `LF2LivingObject.cs:480-497` | C++ `input_handler.cpp` state-jumping 2610+、state-dash 2660+、frame215 2966+ |
| INPUT-8 | transformed/shared-DAT running 路径存在提前返回，并缺少 authority 的 running defend 分支（一个关联差异簇） | `LF2Entity.cs:1578-1636` | C++ `input_handler.cpp:2536-2604`；C# `InputRuntime.cs:1131-1205` |
| INPUT-9 | transformed/shared-DAT frame 215 额外接受 attack 分支，authority 只处理其正式输入条件 | `LF2Entity.cs:1774-1810` | C++ `input_handler.cpp:2966-2997`；C# `InputRuntime.cs:1405-1438` |
| INTERACT-1 | `LF2SpecialAttack` 没有声明使用 dynamic runtime slot，opoint 技能实体不能稳定遵循 `50..399` 槽区契约 | `LF2SpecialAttack.cs:68`；`LF2Entity.cs:1014` | C++ `collision.cpp:1280-1283` |
| INTERACT-2 | dynamic slot `50..399` 满后 Unity 回退分配 `0..49`；authority 应直接生成失败 | `SimulationWorld.Registry.partial.cs:359-369` | C++ `collision.cpp:1280-1283` |
| INTERACT-3 | vrest key 混用 `StableId` 与 runtime slot，可能导致互斥命中对象身份与固定槽 authority 不一致 | `LF2WeaponBase.cs:672,718`；`LF2ObjectPointFactory.cs:260-261`；`LF2SpecialAttack.cs:1001`；对照 `LF2SpecialAttack.cs:995-996` | production collision/vrest 路径以 `Runtime.SlotIndex` 为对象身份 |
| INTERACT-4 | state 3003 opoint 的双向 vrest 参与对象/身份写入与 authority 不一致 | `LF2ObjectPointFactory.cs:213-216,533-537` | C++ `frame_advance.cpp:138-143`；C# `FrameTick.cs:280-287` |
| INTERACT-5 | 非角色 parent 的 kind 2 链接把 `StableId` 写入 `TargetSlotIndex`/`HeldWeaponStableId`/`HolderStableId` 等 slot 字段 | `LF2ObjectPointFactory.cs:540-555`；消费端 `SimulationWorld.QueryAndLinks.partial.cs:119-133` | C++ `collision.cpp:1343-1351` |

当前收口状态：

- **INPUT-1~9：全部已修复并运行时验证。** `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖 state switch、`YInt` 门、frame velocity tail、单一 defend-lock 真值、Super Punch、raw frame write、running 顺序/defend/反向停跑和 frame215。
- **INTERACT-1~5：全部已修复并运行时验证。** `CheckInteractionRuntimeSlotContracts` 覆盖 dynamic slot `50..399`、满槽直接拒绝、runtime-slot vrest、state3003 双向 vrest 和 non-character kind2 链接；满槽拒绝同时断言不遗留空 registry bucket、renderer pool 或 reference/logic pool 生命周期残留。
- **NARUTO-DDJ / OPOINT-LIFECYCLE：已修复并运行时验证。** 真实 frame271→272/273→oid205/204→六分支→`6 x oid33/action307` 回归覆盖 reference pool 类型安全、pending lifecycle finalize、factory 注册时机、池化角色 `StableId` 重分配和 opoint 关系字段继承；详细链路见 §4.3。

#### 审计风险收口（RISK-1/2/3/5 已关闭；RISK-4 保留）

| 编号 | 状态 | 审计结论 / 验证 |
|---|---|---|
| RISK-1 | ✅ 已修复 / Unity 运行时已验证 | late frame rollover 不再通过 `FrameEvent` 二次推进 walking/running locomotion；新增矩阵验证同 tick `AnimCounter` 只推进一次并保留 state-entry 副作用 |
| RISK-2 | ✅ 已修复 / Unity 运行时已验证 | input/move raw frame write 均保持 `PrevFrame/PN`、wait counter 和非显式清零的 attacking；新增 raw move write 矩阵通过 |
| RISK-3 | ✅ 已修复 / Unity 运行时已验证 | held/`TrackerParent` 行为引用改由 runtime slot 和反向关系校验；注销、同槽复用、异槽复用均清理失效缓存，`CheckHeldReferenceSlotReuseContracts` 通过 |
| RISK-4 | ⚠️ 保留 1 项待审计风险 | Unity candidate carrier 缓存对象引用，C# 缓存 slot；审计未找到正式主循环可达的“collect 后释放目标 + 同槽复用 + consume”边界，故不计入确认差异。未来出现此类 producer 时必须升级并补专项测试 |
| RISK-5 | ✅ 已修复 / Unity 运行时已验证 | step7/step9 capability 与入口按 current DAT `obj_type` 中央分派；character shell→non-character 和 special/non-character shell→character 双向矩阵验证不会漏跑或重复跑 interaction pass |

#### 已关闭的历史 backlog / 验收矩阵

下表保留既有工作的**历史来源坐标与 self-check 证据**；旧工程/EXE 坐标不具当前权威性，不得用于当前实现或验收，也不能覆盖或冲销 Audit4。

| 优先级 / 编号 | 状态 | Authority | Unity 现状 | 明确缺口 | 验收标准 |
|---|---|---|---|---|---|
| P0 / CP-NV1 action selection | ✅ 已完成 / Unity 运行时已验证 | C++ release `cpoint.cpp:81-124`，EXE `Collision_Check1` 0x41B740 | real character 与 shared-DAT 两入口均由 signed attacker action + raw victim vaction helper 承载；双方 attacking 清零且 wait 保持 | 已关闭 | `CheckCpointNegativeActionMatrix` 覆盖负 a/t/jaction、victim raw negative frame、facing/wait/attacking/Prev2，fresh Unity batch PASS |
| P0 / CP-NV2 throw raw | ✅ 已完成 / Unity 运行时已验证 | C++ release `cpoint.cpp:126-180`，EXE `Collision_Check1` 0x41B740 | attacker next 与 victim vaction 均 raw 写 `Frame`/`PrevFrame2`；负 frame 允许 `D=null`，不翻面、不改 wait；无/双方向保留旧 Vz | 已关闭 | `CheckCpointThrowRawAndTransformMatrix` 覆盖 real/shared-DAT、raw negative、Prev2/facing/wait/attacking/Vz，fresh Unity batch PASS |
| P0 / CP-NV3 held sync | ✅ 已完成 / Unity 运行时已验证 | C++ release `weapon.cpp:22-107`，EXE `Collision_Check2` 0x41B2C0 | hurtable gate 下 `vaction==0` 也写；负值 raw 后只翻一次并 abs；wait 保持；位置 cpoint 取原始 signed vaction、center 取 resolved current frame | 已关闭 | `CheckCpointHeldSyncVactionMatrix` 覆盖 `vaction<0/==0/>0` 的 real/shared-DAT frame/facing/wait/位置/attacking，fresh Unity batch PASS |
| P1 / FLOW-1 FrameToggle | ✅ 已完成 / Unity 运行时已验证 | C++ release tick-head `g_frame_toggle` 与 state 400/401 early pass；C# `GameTick.Run:32-36` | Flow 新增 `FrameMod12`/`FrameToggle` 并由 `AdvanceBattleFlowTick` 与 CurrentTick/InputPhase 同步推进；early teleport 读取 toggle，source 无 Character gate、401 可选 self、target 保留 Character 过滤 | 已关闭 | `CheckBattleFlowToggleAndTeleportMatrix` 覆盖 tick 1-4/11-13、reset、401 self、non-character source、target 选择/no-target，Unity self-check PASS |
| P1 / LINK-1 positive link validation | ✅ 已完成 / Unity 运行时已验证 | C++ release `game_tick.cpp` step11；C# `GameTick.ValidatePositiveLinks` 仅作映射参考 | `ValidateHeldLinksAll` 按 runtime slot `0..399` 覆盖所有 active `LF2Entity`；valid 仅 target range/active/反向 holder；invalid 只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot`，不清 inactive/mismatch target 的反向字段 | 已关闭 | `CheckValidatePositiveLinksMatrix` 覆盖 valid character/non-character、slot0/399、target -1/400、inactive/mismatch、link<=0、target link 状态和多 holder slot 顺序，Unity self-check PASS |
| P1 / BOUNDS-X | ✅ 已完成 / Unity 运行时已验证 | C++ release `game_tick.cpp:77-130`；C# `GameTick.ApplyPreframeBounds` 仅作映射参考 | `LF2Entity.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride)` 按 current DAT type/OID 中央分派；实体 pass 显式使用 `BaseStageWidthPx`，不改变 stage spawn/AI/camera 的既有 `StageWidthPx` 消费 | 已关闭；oid122/123 条件使用独立 `Unk344>0`，不再误用 `WeaponFlightCounter` | `CheckPreFrameXBoundsMatrix` 覆盖 slot/team/hit-stop/override、strict edges、type3/free、oid122/123、`YInt`、current-DAT/CLR 交叉、base/active width 分离、`XInt` 与 world lifecycle；physical worktree fresh Unity 日志双 PASS |
| P1 / TRANSFORM-SHELL | ✅ 已完成 / Unity 运行时已验证 | C++ release state `4000..4999` / `8000..8999` 替换 `char_data` 及 `obj_type`；`init_runtime_identity` 将 DAT `weapon_hp` 写入 `unk_31C` | frame/physics/landing 及 step7/step9 interaction capability/entry 已按 current DAT 中央分派；transform 后以目标 `weapon_hp` 刷新 `WeaponFlightCounter`，不改 `WeaponCount`；state8000 再设 hit-stop 140 | 验收过程修复 transformed pending-destroy、cross-SimOrder 注销遗留，以及 CLR shell 固定 override 导致的 interaction pass 漏跑/重复跑 | `CheckStateTransformLandingMatrix` + `CheckStateTransformInteractionPhaseRouting` 覆盖落地、单次销毁和 character↔non-character shell 双向 pass；2026-07-15 fresh PASS |
| P1 / STEP10 | ✅ 已完成 / Unity 运行时已验证 | C++ release Step10/10.5 的 cpoint duration/mismatch、throw、dircontrol、injury/stat | 已补 duration/mismatch 后 throw/dir；action/dir/injury 不限于 Character-DAT shell；正 injury 写 KillStats/DamageStats 且不误增 `HPLost` | 已关闭 | `CheckCpointEscapeAndMismatchStillRunTail` 与 `CheckSharedDatCpointStep10StatsAndInputOrder` 覆盖 non-character/shared-DAT、tail、input priority 和 stats；2026-07-15 fresh PASS |
| P1 / OPOINT-VIS | ✅ 已完成 / Unity 运行时已验证 | C++ release direct spawn visibility；`frame_logic`、natural drop 与逐实体 late opoint 的不同可见边界 | 已恢复 pre-advance frame_logic、natural drop、逐实体 late producer 三个发布边界；late pass 保持动态 slot 扫描 | 验收过程修复 pending destroy 实体被 active-only 采集过滤，确保 fragment/transition 发布后只回收一次 | `CheckQueuedObjectPointPassBoundaries`、`CheckSimulationWorldLateMutation` 覆盖 real factory queue、三边界、父回收、高/low slot 可见性；2026-07-15 fresh PASS |
| P2 / FRAME-ADV | ✅ 已完成 / Unity 运行时已验证 | C++ release `frame_advance.cpp:25-97` + `physics.cpp`；按 current `char_data->obj_type` 分支 | `SerialTickAll` 按 runtime slot 交错执行 Transit/TU；per-class 路由已收到 current DAT；SpecialAttack 不再提前运行 wait/next | 验收过程修复 character/weapon 壳的 `PS.BindRuntime`，防止物理仍读写脱离 runtime 的状态 | `CheckSerialTickInterleaveAndFrameEdgeMatrix` 覆盖逐 slot 顺序、SpecialAttack 单次 physics、weapon shell 的 type3/other DAT、negative next；落地矩阵同时通过 |
| P2 / FRAME-TICK | ✅ 已完成 / Unity 运行时已验证 | C++ release `frame_advance.cpp:802-996`（release 无独立 `frame_tick.cpp`） | current-DAT 公共主干已集中到 `RunCommonFrameTick`；type3 `hit_a`、state14、iron-ball frame20、wait/next/999/negative、frame mp/`PpDisplay` 与 tail 统一执行 | release 无 oid9 Amaterasu 专属 drain，oid9 只走 DAT 正式路径；SpecialAttack 的旧重复 drain/counter 已移除 | `CheckSpecialAttackStep4AndLateFrameTick`、`CheckFrameTickPpDisplayAndCurrentDatMatrix`、`CheckStateTransformLandingMatrix` 覆盖单次 drain、PpDisplay、gate/999、state14、iron-ball 和 current-DAT landing；2026-07-15 fresh PASS |
| P3 / COLLISION-SNAPSHOT | 🔷 权威审计未发现生产差异 / 保留回归风险 | C++ release step6/7/9/10 的 slot 顺序、`prev_frame2`、双向 pair、itr 顺序与 20 candidate cap | `CaptureCollisionFrameSnapshotsAll` + `BruteForceSceneQuery` 的普通生产路径与权威一致，当前没有实锤修复项 | Unity carrier 缓存对象引用，C++ 缓存 slot；若未来在 snapshot 消费期间引入同 slot 即时复用 producer，语义可能分叉 | 保留多候选/20+、同距、Prev2、cache 隔离回归；未来新增 pass 内 slot reuse 时必须补专项测试 |

---

## 附：核对方法

1. 本文所有 ⚠️/❓ 项都需**打开对应 C# 源码段 + Unity 源码段逐行比对**后才能定性。
2. 定性为"Unity 用别的方式实现了" → 标 🔷 并记录对应关系，**不删**。
3. 定性为"C# 有 Unity 真没有，且是正式战斗逻辑" → 标 ❌ 进 P1 待补。
4. 定性为"C# 是调试/表现/菜单，非战斗运行时" → 标 🚫 排除。
5. 每完成一项核对，更新对应行状态并在 §10 勾选。

---

## 附二：核实总账（更新至 2026-07-16）

**✅ 二次审计确认差异已收口（14/14）：**

输入/动作 9 项（INPUT-1~9）与交互/opoint/vrest 5 项（INTERACT-1~5）均已修复并通过新增自检。RISK-1/2/3/5 经审计实锤后也已修复并运行时验证；只剩 RISK-4 一项未找到正式主循环可达触发边界的待审计风险，不计入确认差异。

**✅ Naruto DDJ 新确认差异已收口（1 个关联差异簇 / 5 个根因）：**

真实 Naruto 防下跳链暴露的 reference pool 污染、factory 注册时机、pending lifecycle 同槽复用、池化角色 StableId 和 opoint 关系字段继承问题均已修复；完整链回归确认 6 个 clone 到达 action307 且 renderer 可见。

**✅ 已修复真 bug（共 1 项）：**

| 项 | 内容 |
|----|------|
| §2.1-1 / T0 | `exemptVal` 已改用权威 arest/vrest 公式，并通过 Unity 运行时自检 |

**✅ 原缺失项已完成并通过 Unity 运行时自检（主要项）：**

| 项 | 内容 |
|----|------|
| M-1 / T4 | oid 7/8→51 合体拆分；C++ gate/oid8 镜像/身份表现/DJA human+AI full-tick/split reset 与 `ItrRest` 契约均已覆盖 |
| M-2 / T5 | 复活 pass（含 free-entity gate、队友平均落点、stored-count 分支与 oid998 特效） |
| M-8 / T1 | 共享 ApplyAlternateDamage 完整契约、真实角色/shared-DAT 两入口及 object-pass 预处理 |
| M-9 / T2 | 角色/武器统一 `RecordKind0Hit` |
| M-14 / T3 | frame 110/114 写 `CdDefendLock=3` 及 cooldown 生命周期 |
| M-15 / M-16 / T6 | kind15 authority 位移 + kind16 完整结算、副作用与持有断开 |
| combo / T7 | RunComboWrappers 9 组连招 + oid6 DjaGuard |
| Naruto DDJ / OPOINT-LIFECYCLE | frame271 起始、oid205/204 递归链、6 x oid33/action307、对象池/slot/StableId/关系字段完整契约 |
| M-13 / T8 | stage immediate spawn、positive refill、清场推进与 phase bound |

**历史快照（Audit4 前）：** 当时只保留 RISK-4 与完整对局逐帧对拍缺口；该结论已被 Audit4-01..16 取代，不代表当前无待实现差异。

**✅ 已确认对齐或已完成并验证（主要项）：**
tick 主循环主干、kind 0/4/9 主流程（含 raw kind9→kind0 预处理与 alternate）、kind 6/8/10/11/14 命中、oid300、kind5 委托、kind4+WeaponCount 翻转（M-7）、HP/PP 自然恢复（§5）、heal/catch timer、帧推进主干 + state14 复活 HitStop（§3-1~§3-5）、frame mp turn-around、opoint 生成、cpoint 抓取、state 400/401/500/501、N30 触发、状态转换特效。

**🔷 架构不同但等价（严禁删，见 §8）：** resolver / shared-DAT 桥 / 字段化 runtime / hook 拆分 / 动态槽 / DirectWriteFramePreserveWaitCounter 等。

**🚫 不需对齐（见 §9）：** UI/HUD、camera/background/render、audio playback、network/replay、Host 和 F7-F9/debug 控制路径。**🗑️ 确认可不移植：** M-6 F8 调试掉武器。

**⏸️ 用户明确暂缓：** T8 默认 `stage.dat` 资产部署。T8 逻辑/接线和 self-check 状态不变，但该资产工作不进入当前推进。

---

### Audit4 前历史总结（已失效）

**本段只记录 Audit4 前的历史验收快照，不是当前执行口径。** BATTLE-AUDIT4-01..14 的生产修复和已有断言现已通过 fresh full self-check，但 3 项定向 Play Mode 尚未完成；T8 默认 `stage.dat` 资产部署仍由用户明确暂缓并排除在当前 backlog 之外。

## 第三次实战/静态审计（2026-07-16，最高优先级）

旧版“当前无确认差异”结论已失效。以下 BATTLE-AUDIT3-01..17 均为已静态确认的战斗逻辑差异，17 项生产修复现已全部落地。最新 fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` 源码时间 `2026-07-16 18:24:04`，Unity `Assembly-CSharp.dll` 时间 `18:31:52`，`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `18:33:00` fresh 返回 **PASS**，满足 source < DLL < result。该结果包含本轮 M-1/T4 完整矩阵。此前生产 diff 的 Architect 复核结论保留；新增自检覆盖由本次 fresh build/PASS 证明。上述证据只关闭编译、静态复核和针对性 self-check 门槛；本轮变更后的真实 `NTSD_Battle` Naruto 防前跳螺旋丸、奔跑防跳命中及防下跳六分身仍待 Play Mode 验收，因此不得把 17 项标成 Play Mode 全完成，也不得宣称战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。

| 编号 | 双方证据 | 影响 | 状态 |
|---|---|---|---|
| BATTLE-AUDIT3-01 | Unity `BattleTestBootstrap.cs:203` 只写 Team；C#/正式入口 `AppManager.cs:206-207` 写 Team+RelationTeam；`LF2WeaponInteractionResolver.cs:20-23` 对 RelationTeam=0 退出 | oid434 action396 kind3 消费被阻断，Naruto frame256 链不成立 | 生产修复和针对性 self-check 已通过；`RelationTeam` 已补，仍待真实 bootstrap 与 Naruto 螺旋丸 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-02 | C# `WeaponRuntime.cs:140-149` cover=0 为 z+1/y-1；Unity `LF2CharacterWeaponLinkResolver.cs:265-277` 相反；renderer `LF2ObjectRenderer.cs:219-220` 另加 zz | held 武器 Y/Z 与排序偏移，renderer 仅部分抵消 | 生产修复和针对性 self-check 已通过；held 层级、位置与跟手仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-03 | Unity `BruteForceSceneQuery.cs:1630-1643` coarse union 排除 kind5，消费侧 `:529-660` 才替换；C# `CollisionCollect.cs:431-451` union 纳入全部 itr | kind5-only 命中在粗筛阶段消失 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-04 | Unity `BruteForceSceneQuery.cs:1614-1625,1658-1668` 过滤大坐标；C# `CollisionCollect.cs:431-478` 保留原始几何；DAT 有 Naruto y=80000 kind3 | 高层碰撞候选无法进入 Unity | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-05 | Unity `LF2WeaponHeldStateResolver.cs:75-78`、`LF2Weapon.cs:675-730` 有 ordinary weapon_strength held 旁路；C# `WeaponRuntime.cs:71-213` 无此旁路 | 普通武器 held 动作/伤害路径偏离 | 生产修复和针对性 self-check 已通过；螺旋丸按攻击键的真实 weapon 路径仍待 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-06 | Unity `NTSDBattleTickSystem.cs:37,50,56` 每 tick 三次 HeldObjectProcessAll；C# `GameTick.cs:99-103` 一次 Step12、一次 SyncHeld | 重复同步/释放/消耗 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-07 | Unity `NTSDBattleTickSystem.cs:38-50` candidate/hit 后才 PreInteraction；C# `GameTick.cs:99-106` 先 cpoint/link 再 collect | 本 tick cpoint/held 状态不能影响候选 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-08 | C# `GameTick.cs:95-106` candidate 前 clamp Z；Unity `NTSDBattleTickSystem.cs:37-39,55-56` clamp 在交互后 | 候选读取未 clamp 的角色 Z | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-09 | Unity 原实现对 invalid positive link 只清 `LinkState`；C# `ValidatePositiveLinks` 对无效链接只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot` | holder 残留 target/held slot 污染后续 held；inactive/mismatch target 的反向字段不在此处清理 | 已按 C# 契约只清 holder 三字段，针对性 self-check 已通过；不清 target 反向字段，仍待真实 Play Mode |
| BATTLE-AUDIT3-10 | Unity `SimulationWorld.Passes.partial.cs:635-648` 依赖 Supports；基类 `LF2Entity.cs:1034-1036` 默认 false，Special/Other 无 override；C# `GameTick.cs:83-90,165-170` 统一分派非角色 DAT hit_Fa | Special/Other hit_Fa 时机/执行路径错误 | 生产重构和 fresh self-check 已通过：`hit_Fa1..14` 唯一下沉 `LF2Entity`，Special/Other/current-DAT shell 共用；新增覆盖 3/4/10/14，3/14 对 Other、current-DAT Character、Special 三壳连续两 tick 验证副作用仅一次，4 覆盖 catch frame/速度/`CatchTimer`，10 覆盖原路径与落地摩擦防重复；仍待真实 Play Mode 场景验收 |
| BATTLE-AUDIT3-11 | Unity `LF2ObjectPointFactory.cs:221-229` logicalY+PS.z；C# `FrameTick.cs:381-394` spawnY 不加 Z；Character/Weapon/Other 初始化直接用 task.pos.y，renderer `LF2ObjectRenderer.cs:278-280` 再加 displayZ | non-special opoint 出生高度可能双加 Z；SpecialAttack `LF2SpecialAttack.cs:1383-1387` 会减回 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-12 | Unity `SimulationWorld.QueryAndLinks.partial.cs:77-83` 强制 LF2Character holder；C# `WeaponRuntime.cs:86-94` 接受任意带 CharData Entity | shared-DAT/非 Character holder 断链 | generic holder、damaged 后继续 dvx/kind3 与 IronBall `FrameDelay=1` 已落地；新增 `CheckWorldLevelRealWeaponStep12Contracts` 经 `SimulationWorld.HeldObjectProcessAll`、generic `LF2Entity` holder、真实 `LF2Weapon` 覆盖 damaged→dvx、damaged→kind3、IronBall `FrameDelay=1` 并 fresh PASS；仍待真实 Play Mode 场景验收 |
| BATTLE-AUDIT3-13 | Unity `BruteForceSceneQuery.cs:1603-1627,1646-1677` 过滤 body kind、x/y、w/h/zwidth；C# `CollisionCollect.cs:431-478` 不过滤；full-height 识别两边均有 | 正式大范围技能/特殊几何被 Unity 粗筛排除 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-14 | Unity `BruteForceSceneQuery.cs:446-526,1277-1304` nearest/bodyX gate 依赖 modeArg==1；C# `CollisionCollect.cs:181-240` 无 mode gate | 默认模式目标选择/候选数不同 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-15 | C# `CollisionCollect.cs:144-158` 有 oid205→oid9 frame301、hit_a/d/j=999、同非零 Unk364 pair gate；Unity 仅有 oid→209 kind9 gate `BruteForceSceneQuery.cs:1064-1075` | Naruto 相关同关系对象错误进入候选 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待 Naruto 真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-16 | C# same-team 例外 `CollisionCollect.cs:304-355` 读 attacker Prev2/collision；Unity `BruteForceSceneQuery.cs:988-1007,1034-1037` 读 current | 帧边界放行/拒绝相反 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-17 | C# kind8/state3005 lead-in `CollisionCollect.cs:99-101` 读 current；Unity `BruteForceSceneQuery.cs:990-1002` 传 Prev2 collision | kind8 延迟命中时机偏移 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |

**本轮验收状态：**fresh `/m:1` build 已为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full self-check 返回 **PASS**。除 Audit3-10 的 3/4/10/14 扩展矩阵和 Audit3-12 的 world-level generic holder/真实 weapon Step12 矩阵外，本结果也覆盖 M-1/T4 的完整运行时矩阵。下一步仍须在真实 `NTSD_Battle` 回归 Naruto 防前跳螺旋丸的层级/位置/跟手/攻击路径、奔跑防跳命中，以及防下跳六分身。因此 17 项只能称为“生产修复已落地、针对性 self-check 已通过、Play Mode 未全部验收”。T8 默认 `stage.dat` 部署继续暂缓。

## 实施进度（2026-07-16）

> §10 的 `[x]` 仅表示“已核实定性”，不表示已经实现；实际完成状态以本表为准。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState`。type3 lead sound 已按代码权威对齐，headless 未直接观测音频 | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **历史实现/self-check 已通过；待 C# 重审** | 唯一权威为 C# `GameTick.cs:1093-1263`；旧实现的 pass 顺序、merge/split 与身份链需据此重新核验 | 既有 7 项检查仅保留为回归基线，不能代替 C# 权威重审 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |
| T7（§6.1 / combo） | **已完成 / Unity 运行时已验证** | `NTSDInputStateModule` 已承载 9 组 combo wrapper 与 oid6 DjaGuard；角色真实输入路径经 `RunPostCooldownInputPhase` 消费并落到 `ApplyFrameInput` | `CheckComboWrappersCharacterFrameJumps`、`CheckOid6DjaGuardComboHold` 已覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release 并通过 |
| T8（M-13 / stage） | **逻辑与接线已完成 / Unity 运行时已验证；默认资产部署暂缓** | `BattleStageCampaignLoader`、`ApplyMatchConfig` 生产接线；stage progression/runtime；立即刷敌、positive refill、清场推进、phase bound、精确身份字段与 dynamic slot 50+ | 三项 stage self-check 均通过；默认 `stage.dat` 部署由用户明确暂缓，不进入当前 backlog |
| T9（AI） | **已完成 / Unity 运行时已验证** | `SimulationWorld.AiInput.partial.cs` 完整 AI 闭包；human/AI 输入 pass 分段；runtime 字段与 roster/opoint bootstrap；shared-DAT shell | `CheckAiTargetCacheCoordinateAndDeterminism`、`CheckAiHumanInputIsolation` 通过，并回归 T0-T8 |
| 二次审计 INPUT-1~9 | **全部已修复 / Unity 运行时已验证** | real/shared-DAT input state、raw frame、velocity tail、running/frame215 等契约已按 authority 收口 | `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵通过 |
| 二次审计 INTERACT-1~5 | **全部已修复 / Unity 运行时已验证** | dynamic slot、满槽拒绝、runtime-slot vrest、state3003、non-character kind2 已收口；拒绝路径清理空 bucket/pool/reference 生命周期 | `CheckInteractionRuntimeSlotContracts` 通过 |
| Naruto DDJ / OPOINT-LIFECYCLE | **已修复 / 当前版本真实 Play Mode 已通过** | active-only reference release；register finalize pending old lifecycle；factory slot guard 后移；pooled character 重分配 StableId；`PostInitLiving` 补 Team/RelationTeam/HolderCopy 继承 | 真实生产输入链 `L -> L+S -> L+S+K` 通过；6 个 unique clone 均到 action307，6 个 renderer 同时可见 |
| 二次审计 RISK | **RISK-1/2/3/5 已修复；RISK-4 保留** | locomotion 单次推进、raw move frame、held/Tracker slot 生命周期、current-DAT interaction phase 已收口 | 对应新增矩阵及 `CheckHeldReferenceSlotReuseContracts`、`CheckStateTransformInteractionPhaseRouting` 通过 |

Audit3 历史验证（2026-07-16）：fresh `/m:1` build 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。M-1/T4 的 gate、oid8 镜像、identity/presentation、human+AI DJA full-tick、split formal reset 与 `ItrRest` 保留矩阵，以及 Audit3-10/12 的扩展矩阵均包含在该结果中。该结果是针对性断言证据，不是完整 Play Mode 或逐帧等价证明；M-1 已完成 runtime self-check，不能据此扩大为全部战斗逻辑完全对齐。RISK-4 与完整对局逐帧对拍仍是验证缺口；T8 默认 `stage.dat` 部署继续由用户明确暂缓。

当前版本已在真实 `NTSD_Battle` Play Mode 重新验证 Naruto 防下跳六分身：生产输入按同一逻辑帧渐进注入 `L -> L+S -> L+S+K`，经 `InputActionMap -> CharacterInputModule -> SimInputBuffer`；tick1 到 frame271，tick12 到 frame272 且 PP `500 -> 295`、生成 oid205，tick15 到 frame273 并开始展开 oid204，tick29-32 出现 6 个 unique oid33/action307，tick38 共有 6 个 renderer 同时可见。峰值为 `max204=11`、`max205=3`、`uniqueClones=6`、`action307=6`、`maxVisible=6`，因此该项定向 Play Mode **PASS**。Audit4 后续三项定向 Play 也已全部通过，证据见本节末。T8 默认 `stage.dat` 资产部署继续暂缓。

## 第四次战斗命中/技能链审计实施进度（BATTLE-AUDIT4，2026-07-17 最终状态）

> 唯一权威为 `J:\QQFile\NTSD2.4\ntsd_release_C#`；表内所有 C# 坐标均指向该工程。以下 16 项的生产修复已经落地，fresh full `BattleRuntimeSelfCheck` 与 3 项真实角色/输入/对象链 Play Mode 均已通过，Architect 最终复核为 **PASS**。这只关闭本批已确认差异，不等于完整对局逐帧等价；RISK-4 和完整对局逐帧对拍仍保留。T8 默认 `stage.dat` 资产部署继续按用户要求暂缓，不进入本批任务。

| 编号 | C# 权威（文件 / 方法 / 行） | Unity 差异（文件 / 方法 / 行） | 影响 | 当前状态 |
|---|---|---|---|---|
| BATTLE-AUDIT4-01 | `Simulation/GameTick.cs:1265-1297` `RunCooldownsTick`：以当前 frame 的 `Itrs` 判定是否清 `AttackExempt`，并处理 state1001 holder/wpoint/attacking 分支 | `SimulationWorld.Passes.partial.cs:943-958` `ClearAttackExemptIfCurrentFrameCannotHit` 错查 `opoints/opoint`，且没有 holder 分支 | 攻击豁免可能在仍有 itr 时被清除，或在无 itr 时残留，导致技能/武器重复命中或错误漏命中 | **生产修复已落地；Audit4 针对性矩阵 fresh PASS** |
| BATTLE-AUDIT4-02 | `Interaction/HitResolve.cs:262-510` `ApplyDamageCandidate` 是真实角色、shared-DAT 和对象的统一标准命中结算；其中 `:447-485` 统一写 `FrameDelay/AttackExempt`，state1002 随机帧与 `Vx/Vy=-4`，并处理 FlyingA 对撞 | 真实角色 `LF2CharacterHitResolver.cs:360-420` 只向 `LF2LivingObject.HitCounters` 写豁免、普通受击写 `FrameDelay=-5`，state1002 不换帧且读取 victim `PS.vx`/写 `Vy=-3.5`；shared-DAT `LF2CharacterDatHitResolver.cs:681-744` 又采用另一套行为并额外写 `WeaponState`/ProjectileFlying frame10 | 同一 C# 命中规则在两条 Unity 路径漂移；投掷物反弹、首击结束时机、飞行物互撞和不同实体壳表现不一致 | **生产修复已落地；标准命中矩阵 fresh PASS；投掷武器 Play 09:45:21 PASS** |
| BATTLE-AUDIT4-03 | `Interaction/HitResolve.cs:26-65` `ResolveCandidates` 除显式 `AbortRemainingHitPairs` 外继续消费同 tick 后续候选 | `LF2WeaponInteractionResolver.cs:38-100` 成功命中后无条件在 `:99` `break` | 武器同 tick 多目标/多候选只处理首个成功对象，与 C# 候选消费数量和顺序不一致 | **生产修复已落地；连续候选/显式 abort 矩阵 fresh PASS** |
| BATTLE-AUDIT4-04 | `Interaction/HitResolve.cs:26-65,447-485` 使用 world `ARest/VRest` 契约；`Interaction/WeaponRuntime.cs:99-215` 的 held/throw/drop 路径没有额外清零双方 arest | `LF2WeaponInteractionResolver.cs:91-99` 额外调用 `ItrArestUpdate`；`LF2WeaponHeldStateResolver.cs:92-95,108-111` 在投掷/受伤掉落时清零 weapon 与 holder 的 `ItrRest.Arest` | Unity 的第二套 arest 状态会暂时挡住命中，冷却结束后又重新命中；投掷/掉落还会改变下一次可命中时机 | **生产修复已落地；held/Arest 断言 fresh PASS；投掷武器 Play 09:45:21 PASS** |
| BATTLE-AUDIT4-05 | `Interaction/CollisionCollect.cs:14-240` 在 collect 阶段完成 pair/geometry/team 等筛选；`HitResolve.cs:26-65` consume 只校验 slot、itr index、active/CharData 与 VRest | `LF2CharacterInteractionResolver.cs:45-139` 和 `LF2WeaponInteractionResolver.cs:43-99` consume 时再次计算 allow gate、runtime itr、target/team/type/geometry/arest 等条件 | collect 后到 consume 前状态变化会让已收集候选被 Unity 二次拒绝，技能命中窗口和候选顺序偏离 C# | **生产修复已落地；SpecialAttack 已删除 live Team gate；collect 后 attacker `Team=0` 仍消费两个冻结候选并 fresh PASS** |
| BATTLE-AUDIT4-06 | `Interaction/HitResolve.cs:563-617` `ApplyKind3Grab/AlignGrabPair`：raw 写双方 frame、按整数坐标快照对位、建立 slot 关系，不附带丢武器 | `LF2CharacterInteractionResolver.cs:265-350,419-450`：限制目标必须是真实 `LF2Character`，使用 `ImmediateFrame`，坐标/计数副作用不同，并在 `:446-447` 额外 `DropWeapon` | Naruto 奔跑 `L -> K` 的 `102 -> 295/296 -> kind3 -> 297 -> 298 -> 299 -> 275...` 后续链可能在抓取帧、对位或目标壳 gate 中断，导致命中后缺少下一招 | **生产修复已落地；kind3 real/shared-DAT 矩阵 fresh PASS；Naruto 奔跑防跳 Play 09:34:36 PASS** |
| BATTLE-AUDIT4-07 | `Interaction/HitResolve.cs:1318-1529` `ApplyKind0Type3Tail` 完整覆盖 state3000/3005/3006 的关系继承、双方速度/帧/延迟、effect 尾和声音 | `LF2SpecialAttack.cs:456-519` `Hit/ApplyPostHitSelfDestruct` 只覆盖部分 3000/3006 分支，且 oid201/214 的 `DieEvent`/HP 清零后处理按 Unity CLR attacker 类型分流 | 技能对象互撞、扩张/飞行态转换、关系字段及 oid201/214 自毁方向/时机与 C# 不一致 | **生产修复已落地；type3/oid201/214 针对性矩阵 fresh PASS** |
| BATTLE-AUDIT4-08 | `Simulation/GameTick.cs:1773-1870` `SpawnStateTransitionEffects` 规定 branch 判定及每个碎片的 RNG 调用顺序（Y、X、Vy、Vx 等） | `LF2Entity.cs:3501-3564` `SpawnLateTransitionEffects/SpawnTransitionEffectBranch1/2` 的随机取值顺序和次数不同 | 即使单个特效范围相同，也会推进不同的全局 RNG 状态，继而改变后续战斗随机结果 | **生产修复已落地；现有 transition/RNG 断言随 full self-check fresh PASS** |
| BATTLE-AUDIT4-09 | `Interaction/WeaponRuntime.cs:99-155` `RunHeldObjectStep12ForPair` 每 tick raw 写 `held.Frame/Facing/FrameDelay`，朝向直接跟 holder | `LF2CharacterWeaponLinkResolver.cs:251-292` 与 `LF2WeaponHeldStateResolver.cs:32-41,139-175` 每 tick `ImmediateFrame`，并按 cover 十位再执行额外 flip | held 对象的 attacking/wait 等计数被重复重置，朝向和挂点帧可能抖动或滞后，影响螺旋丸跟手、层级与按攻击键后的动作 | **生产修复已落地；raw frame/wait/facing 矩阵 fresh PASS；Naruto 螺旋丸 Play 01:10:34 PASS** |
| BATTLE-AUDIT4-10 | `Interaction/HitResolve.cs:382-406,889-906` 受击帧按 attacker/victim 的 `Facing` 关系选择 | `LF2CharacterHitResolver.cs:581-596,673-680` 与 `LF2CharacterDatHitResolver.cs:954-968,1011-1016` 通过 attacker 相对 X 推断方向 | 交叉、瞬移、同 X 或攻击者背向出招时会进入错误的正面/背面受击帧 | **生产修复已落地；real/shared-DAT facing 矩阵 fresh PASS** |
| BATTLE-AUDIT4-11 | `Frame/FrameTick.cs:242-252` 要求 first op 同时满足 `Kind>0 && Oid>0`；`:414-419` 为 oid5/52 初始化 `Hp/HpMax/Hp3/Pp=10/10/10/5` | `LF2ObjectPointFactory.cs:139-145` first-op 总闸门漏 `oid>0`；`:536-547` 的 oid5/52 初始化字段不完整 | 无效 first-op 可能错误放行后续生成；oid5/52 技能实体初始生命/PP 契约错误 | **生产修复已落地；first-op 与 oid5/52 初始化矩阵 fresh PASS** |
| BATTLE-AUDIT4-12 | `Interaction/HitResolve.cs:1084-1147` `RecordDamageEffectSound/RecordStandardHurtSounds/RecordAlternateHurtLeadSound` 覆盖 effect cue、effect1 附加声、attacker/victim 武器声音及 oid 条件 | `LF2CharacterHitResolver.cs:439-446` 与 `LF2CharacterDatHitResolver.cs:762-767` 主要只播通用 `SFX_001/006`；shared 路径部分判断还使用 `type_sub` 代替 oid（`:276-282`） | 命中确认的声音组合、声源位置和特定技能反馈与 C# 不一致 | **生产修复已落地；声音记录随 Audit4 full self-check fresh PASS** |
| BATTLE-AUDIT4-13 | `Frame/FrameTick.cs:13-216,218-230` 在规定 frame_tick 边界统一 `QueueFrameSound`；`SpawnFromOpoint` 仍按正常实体生命周期生成对象 | `LF2SpecialAttack.cs:96-98,230-231` 存在类内独立 frame sound；`LF2ObjectPointFactory.cs:331-340,467-477` 对 `pic=999,wait=0,next=1000` 直接播放并立即回收 | 同一声音可能在不同 pass 播放、重复或丢失；pic999 对象不再经历 C# 的注册、frame tick 和回收边界 | **生产修复已落地；living/weapon/SpecialAttack `PendingSounds` 单次精确断言与 tick/reset 清理 fresh PASS** |
| BATTLE-AUDIT4-14 | `Interaction/HitResolve.cs:503-507,1150-1195` 对成功 kind0 统一 `RecordKind0Hit`，不以 effect6/23 排除 spark 记录 | shared-DAT `LF2CharacterDatHitResolver.cs:770-773` 显式跳过 effect6/23 的 `SpawnSpark`，真实角色路径又在 `LF2CharacterHitResolver.cs:449-450` 无该排除 | 同一命中在真实角色与 shared-DAT 壳的 spark 记录数量/随机数消费不同 | **生产修复已落地；effect6/23 统一 spark 断言 fresh PASS** |
| BATTLE-AUDIT4-15 | `Simulation/GameTick.cs:142-147` 在交互后的 late update 推进 holder frame；`Interaction/WeaponRuntime.cs:99-155` 定义 held frame/挂点/整数位置契约。Unity 必须在 late holder 切帧后刷新该契约的表现结果 | `HeldObjectProcessAll` 早于 late `SimFrameTick`，holder 首 tick 切帧后 held 仍使用旧挂点；renderer 刷新也没有保证 holder 后于 held 的同 tick 可见顺序 | 螺旋丸已生成但首 tick 位置滞后、移动不跟手或层级/攻击表现落后一拍 | **生产修复已落地：late frame 变化后只调用纯 `SyncHeldPose`，不重复 step12，并按 holder→held 刷新 renderer；focused self-check 01:07:01 PASS；Rasengan Play 01:10:34 PASS** |
| BATTLE-AUDIT4-16 | `Interaction/CPointRuntime.cs:58-85` 按 `PrevFrame2` 与持久 `CaughtIdx/CatcherIdx` 维持抓取链；`Runtime/NtsdEntityRuntime.cs:178-190` 只在完整实体 reset 时清关系字段 | `LF2CharacterCatchResolver` 的普通 `state_exit` 与 `LF2Character.ResetStateRuntime` 提前清 `CaughtSlotIndex/CatcherSlotIndex`；`276 -> 277` 后下一 tick 的 cpoint 仍读 `PrevFrame2=276`，却因关系已清而强制 frame0 | Naruto 奔跑防跳抓取链在 276 后中断，缺失 277/278/279 与 86/87/88 后续招 | **生产修复已落地：普通 state transition 保留 catch link，完整实体 Reset 仍清；fresh full self-check 09:26:55 PASS；Running Play 09:34:36 PASS** |

### Audit4 fresh 验证证据（2026-07-17）

- 当前 Unity Editor PID `11540` 完成 fresh script compile，Console 为 **0 C# error**。
- 最终 freshness 链：`BattleRuntimeSelfCheck.cs` source/test `01:39:46` < `Library/ScriptAssemblies/Assembly-CSharp.dll` `09:26:23` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `09:26:55`；fresh full self-check **PASS**。
- 早一轮 held late pose focused freshness 链为 source `01:05:07` < DLL `01:06:22` < result `01:07:01`，结果 **PASS**；最终 full PASS 已再次覆盖该回归。
- Architect 复核后新增的 SpecialAttack 候选矩阵已进入本次 PASS：生产 consume 删除 live `Team` gate；候选在 collect 后把 attacker `Team` 改为 `0`，仍按冻结的 geometry/team 连续消费两个目标；显式 oid300 abort 仍会停止后续候选。
- SpecialAttack frame sound 断言精确要求 `PendingSounds.Count == 1`，且 Cue、WorldX、Tick 均匹配；living/weapon 分支、下一逻辑 tick 清空及 `ResetRuntimeState` 清空也在同次 PASS 中。
- Naruto 防前跳螺旋丸 Play `01:10:34` **PASS**：frame240 / oid434 / link 均成立；change runtime/holderVisual/heldVisual=`5/5/5/5`，move=`9/9/9/9`，sorting `526 -> 527`；攻击链 `20 -> 257 -> 258 -> 259`，oid434 `396 -> 397`。
- Naruto 奔跑防跳 Play `09:34:36` **PASS**：完整链为 `9 -> 102 -> 295(prev2)/297(pn) -> 298 -> 299 -> 275 -> 276 -> 277 -> 278 -> 279 -> 86 -> 87 -> 88`，victim 保持 frame130/catch；oid33 `current311/pn310` 是 wait0 的正确观测口径。
- 投掷武器 Play `09:45:21` **PASS**：使用生产 oid120 / hold / double-D / D+J；HP 只在 tick17 从 `500 -> 489` 下降一次；weapon state1002/frame41 后同 tick 切到 frame7/state1000，`AttackExempt=4`；跨 35 tick 冷却归零并落地，HP 无二次下降。
- 当前 Unity 自动生成的 dotnet `.csproj` 仍包含 35 个已删除历史源文件，最终 `dotnet build` 被 `CS2001` 阻塞。不得把此前的 dotnet 0 error 冒充为 Audit4-16 后的最终证据；最终有效编译证据是上述 Unity fresh script compile 0 C# error。

### Audit4 实施顺序与剩余边界

- **已完成的串行核心链**：`01 -> 02 -> 03/04 -> 05` 已按依赖顺序收口，cooldown、标准命中和 candidate 消费矩阵已进入 fresh PASS。
- **已完成的独立轨**：`07`（SpecialAttack type3 tail）、`08`（状态转换 RNG）、`09`（held 同步）生产修复已合并并通过已有断言。
- **已完成的第二阶段**：`06/10/12/14` 的命中尾与 `11/13` 的 opoint/声音生命周期生产修复已落地并通过已有断言。
- **Play 抓出的后续修复**：`15` 收口 late holder 切帧后的 held pose/renderer 同 tick 刷新；`16` 收口普通 state transition 错清 catch link。两项均已进入最终 full self-check，并由对应 Play 场景验证。
- **目标 Play Mode**：Naruto 奔跑 `L -> K` 后续招、Naruto 防前跳螺旋丸 held/层级/跟手/攻击链、投掷武器首击后的单次命中/Arest 时间线均已 **PASS**。
- **仍保留的审计/验证边界**：完整对局逐帧对拍尚未完成，RISK-4 仍是待审计风险，因此不能将 Audit4 本批验收扩大成“全部战斗逻辑完全对齐”。
- **非行为性清理债**：`WeaponSpawner` 仍有旧 Python/C++ 注释，F9 debug 说明也存在与当前 C# 唯一权威措辞冲突的历史文字；F7-F9/debug 已按 `AGENTS.md` 排除正式战斗 backlog，不计为生产逻辑差异。


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# NTSD full battle parity certificate architecture review

Act as the independent architect reviewer. The only behavioral authority is
`J:\QQFile\NTSD2.4\ntsd_release_C#`. Do not read, cite, or infer behavior from
any C++ project, binary disassembly, pseudocode, or legacy implementation.

Review the current full battle parity strategy and implementation skeleton:

- `Tools/NTSDParity/README.md`
- `Tools/NTSDParity/AuthorityTraceCommand.cs`
- `Tools/NTSDParity/TraceCompareCommand.cs`
- `Tools/NTSDParity/DataAuditCommand.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs`
- `Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs`
- `Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs`
- `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`

User goal: same input, same seed, compare every 30 Hz logic tick, and prove all
battle behavior observable in Unity matches the formal C# project even when the
framework implementation differs. T8 default `stage.dat` deployment remains
explicitly deferred and must not block the non-stage certificate.

Identify concrete schema/runner/certification gaps which could yield a false
positive. Check at least: fixed 400-slot identity/lifecycle, full entity runtime
state, DAT manifest separation, input edge/history, RNG state and call count,
arest/vrest, ownership/link/target/holder, hit candidates and pass boundaries,
stats, queued spawns/destroys, sound/event timing where behaviorally relevant,
world flow/toggles/bounds, render-observable attachment/sorting/visibility, and
scenario coverage. Distinguish required deterministic logic fields from Unity-
native presentation fields that need separate Play Mode evidence.

Do not edit source. Write a severity-ordered, repo-grounded report to the output
file. Conclude with a minimal but sufficient certification gate and state whether
the current implementation is ready to claim full parity (it is expected not to
be ready yet unless evidence truly proves otherwise).
