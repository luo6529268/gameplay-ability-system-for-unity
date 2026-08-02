using System.Reflection;
using System.Text;
using System.Text.Json;
using NtsdReleaseCSharp.App;
using NtsdReleaseCSharp.BattleCore.Common;
using NtsdReleaseCSharp.BattleCore.Entities;
using NtsdReleaseCSharp.BattleCore.Frame;
using NtsdReleaseCSharp.BattleCore.Lockstep;
using NtsdReleaseCSharp.BattleCore.Runtime;
using NtsdReleaseCSharp.BattleCore.Simulation;
using NtsdReleaseCSharp.Data;

namespace NTSDParity;

internal static class AuthorityTraceCommand
{
    private const int KnownButtonMask = 0x7F;

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

        // Diagnostic fixtures are detached from the exported battle world. Run them
        // before seeding the production trace so their real call chains cannot alter
        // the trace RNG state or call count.
        StructuralWitnessEventBuffer? structuralEvents =
            StructuralWitnessFixture.Run(scenario.StructuralWitness);
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

        ScenarioInputProvider inputProvider = new(scenario);
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
            driver.StepOneTick(
                world,
                postCooldownInput: () => world.HumanInputPolledExternally = true);
            if (world.GameTick != previousTick + 1)
                throw new InvalidOperationException($"Authority simulation did not advance tick {tick}.");

            CharacterSync.SyncRuntimeFromLegacy(world);
            ApplyFrameCounterProbe(world, scenario.FrameCounterProbe, tick);
            WriteJsonLine(
                writer,
                BuildTick(
                    world,
                    inputProvider.GetFrameInput(tick),
                    defaultSlots,
                    detail,
                    structuralEvents?.CaptureTick(tick)));
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

    internal static void ValidateScenario(AuthorityScenario scenario)
    {
        if (scenario.Ticks <= 0)
            throw new ArgumentException("Scenario ticks must be positive.");
        if (string.IsNullOrWhiteSpace(scenario.GameRoot))
            throw new ArgumentException("Scenario gameRoot is required.");
        HashSet<int> rosterSlots = [];
        foreach (ScenarioSlot slot in scenario.Slots)
        {
            if (slot.PlayerSlot is < 0 or >= 8)
                throw new ArgumentException($"Scenario player slot {slot.PlayerSlot} is outside 0..7.");
            if (!rosterSlots.Add(slot.PlayerSlot))
                throw new ArgumentException($"Scenario player slot {slot.PlayerSlot} is duplicated.");
        }

        HashSet<(int Tick, int PlayerSlot)> inputPlayers = [];
        foreach (ScenarioTickInput input in scenario.Inputs)
        {
            if (input.Tick <= 0 || input.Tick > scenario.Ticks)
                throw new ArgumentException($"Scenario input tick {input.Tick} is outside 1..{scenario.Ticks}.");
            foreach (ScenarioPlayerInput player in input.Players)
            {
                if (player.PlayerSlot is < 0 or >= 8)
                    throw new ArgumentException($"Scenario input player slot {player.PlayerSlot} is outside 0..7.");
                if ((player.ButtonMask & ~KnownButtonMask) != 0)
                    throw new ArgumentException(
                        $"Scenario input tick {input.Tick} player {player.PlayerSlot} has unknown button bits 0x{player.ButtonMask:X}.");
                if (!inputPlayers.Add((input.Tick, player.PlayerSlot)))
                    throw new ArgumentException(
                        $"Scenario input tick {input.Tick} duplicates player slot {player.PlayerSlot}.");
            }
        }

        FrameCounterProbe? probe = scenario.FrameCounterProbe;
        if (probe is not null)
        {
            if (probe.RuntimeSlot is < 0 or >= 400)
                throw new ArgumentException("frameCounterProbe.runtimeSlot must be 0..399.");
            if (probe.PrepareTick <= 0 || probe.ImmediateTick != probe.PrepareTick + 1 ||
                probe.ImmediateTick > scenario.Ticks)
            {
                throw new ArgumentException(
                    "frameCounterProbe must use consecutive prepare/immediate ticks inside the scenario.");
            }
            if (probe.WaitCounter == 0 || probe.FrameWaitCounter == 0 ||
                probe.FrameWaitCounter == probe.WaitCounter)
                throw new ArgumentException("frameCounterProbe counters must be nonzero and distinguishable.");
            if (probe.TargetFrame is < 0 or >= 400)
                throw new ArgumentException("frameCounterProbe.targetFrame must be 0..399.");
        }

        if (scenario.StructuralWitness is not null and not ("W03" or "W04" or "W07"))
            throw new ArgumentException("structuralWitness must be W03, W04, or W07 when present.");
    }

    private static void ApplyFrameCounterProbe(GameWorld world, FrameCounterProbe? probe, int tick)
    {
        if (probe is null || (tick != probe.PrepareTick && tick != probe.ImmediateTick))
            return;

        Entity entity = world.Objects[probe.RuntimeSlot];
        if (!entity.Active || entity.CharData is null)
            throw new InvalidOperationException($"frameCounterProbe slot {probe.RuntimeSlot} is not active.");
        if (entity.CharData.GetFrameOrNull(probe.TargetFrame) is null)
        {
            throw new InvalidOperationException(
                $"frameCounterProbe target frame {probe.TargetFrame} is unavailable at slot {probe.RuntimeSlot}.");
        }

        entity.WaitCounter = probe.WaitCounter;
        entity.FrameWaitCounter = probe.FrameWaitCounter;
        int expectedFrame = entity.Frame;
        if (tick == probe.ImmediateTick)
        {
            FrameRuntime.SetFrameImmediate(entity, probe.TargetFrame);
            expectedFrame = probe.TargetFrame;
        }

        int expectedFrameWaitCounter = tick == probe.ImmediateTick ? 0 : probe.FrameWaitCounter;
        if (entity.Frame != expectedFrame ||
            entity.WaitCounter != probe.WaitCounter ||
            entity.FrameWaitCounter != expectedFrameWaitCounter)
        {
            throw new InvalidOperationException(
                $"Authority frameCounterProbe contract failed at tick {tick}: " +
                $"frame={entity.Frame}, frameWaitCounter={entity.FrameWaitCounter}, " +
                $"waitCounter={entity.WaitCounter}.");
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
            ["schema"] = string.IsNullOrWhiteSpace(scenario.StructuralWitness)
                ? "ntsd-battle-trace-v3"
                : "ntsd-battle-trace-v4",
            ["scenarioName"] = Path.GetFileName(scenarioPath),
            ["scenario"] = ProjectScenario(scenario),
            ["loadedChars"] = loadedChars,
            ["maxRuntimeSlots"] = world.Objects.Length,
            ["expectedTicks"] = scenario.Ticks,
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
            ["dataFixture"] = "production",
        };
    }

    private static object ProjectScenario(AuthorityScenario scenario)
    {
        SortedDictionary<string, object?> result = new(StringComparer.Ordinal)
        {
            ["seed"] = scenario.Seed,
            ["mode"] = scenario.Mode,
            ["difficulty"] = scenario.Difficulty,
            ["stage"] = scenario.Stage,
            ["randomStage"] = scenario.RandomStage,
            ["ticks"] = scenario.Ticks,
            ["slots"] = JsonProjection.Project(scenario.Slots),
            ["inputs"] = JsonProjection.Project(scenario.Inputs),
            ["frameCounterProbe"] = ProjectFrameCounterProbe(scenario.FrameCounterProbe),
        };
        if (!string.IsNullOrWhiteSpace(scenario.StructuralWitness))
            result["structuralWitness"] = scenario.StructuralWitness;
        return result;
    }

    private static object? ProjectFrameCounterProbe(FrameCounterProbe? probe)
        => probe is null
            ? null
            : new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["runtimeSlot"] = probe.RuntimeSlot,
                ["prepareTick"] = probe.PrepareTick,
                ["immediateTick"] = probe.ImmediateTick,
                ["waitCounter"] = probe.WaitCounter,
                ["frameWaitCounter"] = probe.FrameWaitCounter,
                ["targetFrame"] = probe.TargetFrame,
            };

    private static object BuildTick(
        GameWorld world,
        SimulationFrameInput input,
        object?[] defaultSlots,
        string detail,
        object[]? structuralEvents)
    {
        object inputDomain = JsonProjection.Project(input)!;
        object rngDomain = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["seed"] = NtsdRng.Seed,
            ["callCount"] = NtsdRng.CallCount,
        };
        object worldDomain = ProjectWorldDomain(world);
        object?[] allSlots = ProjectAllSlots(world);
        string[] slotCommitments = allSlots.Select(CanonicalJson.Sha256).ToArray();
        object slotCommitmentDomain = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["count"] = allSlots.Length,
            ["commitments"] = slotCommitments,
        };
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
        if (structuralEvents is not null)
            ((SortedDictionary<string, object?>)eventsDomain)["structural"] = structuralEvents;

        SortedDictionary<string, string> hashes = new(StringComparer.Ordinal)
        {
            ["input"] = CanonicalJson.Sha256(inputDomain),
            ["rng"] = CanonicalJson.Sha256(rngDomain),
            ["world"] = CanonicalJson.Sha256(worldDomain),
            ["slots"] = CanonicalJson.Sha256(slotCommitmentDomain),
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
            ["slotCommitments"] = slotCommitments,
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

    internal sealed class ScenarioInputProvider : ISimulationFrameInputProvider
    {
        private readonly Dictionary<int, SimulationFrameInput> inputs;

        public ScenarioInputProvider(AuthorityScenario scenario)
        {
            int[] activeHumanSlots = scenario.Slots
                .Where(slot => slot.Active && !slot.Ai)
                .Select(slot => slot.PlayerSlot)
                .Distinct()
                .OrderBy(playerSlot => playerSlot)
                .ToArray();
            var heldButtons = activeHumanSlots.ToDictionary(
                playerSlot => playerSlot,
                _ => SimulationInputButtons.None);
            Dictionary<int, ScenarioPlayerInput[]> updatesByTick = scenario.Inputs
                .GroupBy(item => item.Tick)
                .ToDictionary(
                    group => group.Key,
                    group => group.SelectMany(item => item.Players).ToArray());

            inputs = new Dictionary<int, SimulationFrameInput>(scenario.Ticks);
            for (int tick = 1; tick <= scenario.Ticks; tick++)
            {
                if (updatesByTick.TryGetValue(tick, out ScenarioPlayerInput[]? updates))
                {
                    foreach (ScenarioPlayerInput update in updates)
                    {
                        if (heldButtons.ContainsKey(update.PlayerSlot))
                            heldButtons[update.PlayerSlot] = (SimulationInputButtons)update.ButtonMask;
                    }
                }

                inputs[tick] = new SimulationFrameInput
                {
                    TickIndex = tick,
                    Players = activeHumanSlots
                        .Select(playerSlot => new SimulationPlayerInput(playerSlot, heldButtons[playerSlot]))
                        .ToArray(),
                };
            }
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
    public FrameCounterProbe? FrameCounterProbe { get; set; }
    public string? StructuralWitness { get; set; }
}

internal sealed class FrameCounterProbe
{
    public int RuntimeSlot { get; set; }
    public int PrepareTick { get; set; }
    public int ImmediateTick { get; set; }
    public int WaitCounter { get; set; }
    public int FrameWaitCounter { get; set; }
    public int TargetFrame { get; set; }
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
