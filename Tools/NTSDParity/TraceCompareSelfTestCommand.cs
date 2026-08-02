using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NtsdReleaseCSharp.BattleCore.Lockstep;

namespace NTSDParity;

internal static class TraceCompareSelfTestCommand
{
    public static int Run(string[] args)
    {
        CommandLine cli = CommandLine.Parse(args);
        string output = RepositoryPaths.ResolveOutput(cli.Get("--output") ?? "Temp/NTSDParity/trace-compare-self-test.json");
        string[] validLines = BuildValidTrace();
        string[] structuralLines = BuildValidStructuralTrace();
        string[] positiveLinkLines = BuildValidPositiveLinkTrace();
        List<SelfTestCase> cases =
        [
            TestDenseScenarioInputs(),
            TestScenarioValidationRejectsDuplicatePlayer(),
            TestScenarioValidationRejectsUnknownButtonBits(),
            TestAuthorityStructuralFixtureW03(),
            TestAuthorityStructuralFixtureW04(),
            TestAuthorityStructuralFixtureW07(),
            Expect("valid-compact", Join(validLines), Join(validLines), "equal-commitments"),
            Expect("empty-trace", string.Empty, string.Empty, "different"),
            Expect("header-only", Join(validLines[..1]), Join(validLines[..1]), "different"),
            Expect("skip-tick", Join(validLines), Join([validLines[0], validLines[2]]), "different"),
            Expect("fewer-ticks", Join(validLines), Join(validLines[..2]), "different"),
            Expect("extra-tick", Join(validLines), Join(validLines.Append(Serialize(BuildTick(3)))), "different"),
            Expect("body-changed-hash-stale", Join(validLines), Join(MutateBody(validLines)), "different"),
            Expect("hash-changed-body-stale", Join(validLines), Join(MutateHash(validLines)), "different"),
            Expect("slot-body-commitment-stale", Join(validLines), Join(MutateSlotBody(validLines)), "different"),
            Expect("slot-commitment-domain-hash-stale", Join(validLines), Join(MutateSlotCommitment(validLines)), "different"),
            Expect("manifest-mismatch", Join(validLines), Join(MutateManifest(validLines)), "different"),
            Expect(
                "manifest-mismatch-camera-profile",
                Join(validLines),
                Join(MutateManifest(validLines)),
                "different",
                TraceCompareCommand.FixedWorldCameraProfile),
            Expect("diagnostic-without-opt-in", Join(validLines), Join(MutateDataFixture(validLines)), "different"),
            Expect(
                "diagnostic-explicit-never-certifies",
                Join(validLines),
                Join(MutateDataFixture(validLines)),
                "equal-diagnostic",
                TraceCompareCommand.StrictProfile,
                allowDiagnostic: true,
                expectedCertificate: false),
            Expect("camera-strict", Join(validLines), Join(MutateCamera(validLines)), "different"),
            Expect(
                "camera-fixed-world-profile",
                Join(validLines),
                Join(MutateCamera(validLines)),
                "equal-commitments",
                TraceCompareCommand.FixedWorldCameraProfile),
            Expect(
                "non-camera-world-change-stays-strict",
                Join(validLines),
                Join(MutateNonCameraWorld(validLines)),
                "different",
                TraceCompareCommand.FixedWorldCameraProfile),
            Expect(
                "valid-v4-structural",
                Join(structuralLines),
                Join(structuralLines),
                "equal-commitments"),
            Expect(
                "v4-rejects-slot-400",
                Join(structuralLines),
                Join(MutateStructuralSlot(structuralLines, 400)),
                "different"),
            Expect(
                "valid-v4-positive-link",
                Join(positiveLinkLines),
                Join(positiveLinkLines),
                "equal-commitments"),
            Expect(
                "v4-rejects-deleted-required-field",
                Join(structuralLines),
                Join(RemoveStructuralField(structuralLines, "sourceKind")),
                "different"),
            Expect(
                "v4-detects-structural-field-change",
                Join(structuralLines),
                Join(MutateStructuralBefore(structuralLines, "changed")),
                "different"),
            Expect(
                "v4-detects-structural-event-deletion",
                Join(structuralLines),
                Join(DeleteStructuralEvent(structuralLines)),
                "different"),
            Expect(
                "v4-detects-structural-event-reorder",
                Join(structuralLines),
                Join(ReverseStructuralEvents(structuralLines)),
                "different"),
        ];

        SelfTestReport report = new()
        {
            Schema = "ntsd-trace-compare-self-test-v1",
            Passed = cases.All(value => value.Passed),
            Cases = cases,
        };
        File.WriteAllText(output, JsonSerializer.Serialize(report, JsonProjection.SerializerOptions), new UTF8Encoding(false));
        Console.WriteLine(output);
        foreach (SelfTestCase item in cases)
            Console.WriteLine($"{item.Name}: expected={item.Expected} actual={item.Actual} passed={item.Passed} reason={item.Reason}");
        return report.Passed ? 0 : 1;
    }

    private static SelfTestCase TestDenseScenarioInputs()
    {
        AuthorityScenario scenario = new()
        {
            GameRoot = ".",
            Ticks = 4,
            Slots =
            [
                new ScenarioSlot { PlayerSlot = 2, Active = true, Ai = true },
                new ScenarioSlot { PlayerSlot = 1, Active = false, Ai = false },
                new ScenarioSlot { PlayerSlot = 0, Active = true, Ai = false },
            ],
            Inputs =
            [
                new ScenarioTickInput
                {
                    Tick = 2,
                    Players =
                    [
                        new ScenarioPlayerInput { PlayerSlot = 0, ButtonMask = (int)SimulationInputButtons.Left },
                        new ScenarioPlayerInput { PlayerSlot = 1, ButtonMask = (int)SimulationInputButtons.Attack },
                        new ScenarioPlayerInput { PlayerSlot = 2, ButtonMask = (int)SimulationInputButtons.Defend },
                    ],
                },
                new ScenarioTickInput
                {
                    Tick = 3,
                    Players = [new ScenarioPlayerInput { PlayerSlot = 0, ButtonMask = 0 }],
                },
            ],
        };
        AuthorityTraceCommand.ValidateScenario(scenario);
        AuthorityTraceCommand.ScenarioInputProvider provider = new(scenario);
        SimulationFrameInput tick1 = provider.GetFrameInput(1);
        SimulationFrameInput tick2 = provider.GetFrameInput(2);
        SimulationFrameInput tick3 = provider.GetFrameInput(3);
        SimulationFrameInput tick4 = provider.GetFrameInput(4);
        bool passed = tick1.Players.Count == 1 &&
                      tick1.Players[0].PlayerSlot == 0 &&
                      tick1.Players[0].Buttons == SimulationInputButtons.None &&
                      tick2.Players.Count == 1 &&
                      tick2.Players[0].Buttons == SimulationInputButtons.Left &&
                      tick3.Players[0].Buttons == SimulationInputButtons.None &&
                      tick4.Players[0].Buttons == SimulationInputButtons.None;
        return BooleanCase("dense-scenario-input-timeline", passed);
    }

    private static SelfTestCase TestScenarioValidationRejectsDuplicatePlayer()
    {
        AuthorityScenario scenario = MinimalScenario();
        scenario.Inputs =
        [
            new ScenarioTickInput
            {
                Tick = 1,
                Players =
                [
                    new ScenarioPlayerInput { PlayerSlot = 0, ButtonMask = 1 },
                    new ScenarioPlayerInput { PlayerSlot = 0, ButtonMask = 2 },
                ],
            },
        ];
        return ValidationFailureCase("scenario-rejects-duplicate-player-tick", scenario);
    }

    private static SelfTestCase TestScenarioValidationRejectsUnknownButtonBits()
    {
        AuthorityScenario scenario = MinimalScenario();
        scenario.Inputs =
        [
            new ScenarioTickInput
            {
                Tick = 1,
                Players = [new ScenarioPlayerInput { PlayerSlot = 0, ButtonMask = 0x80 }],
            },
        ];
        return ValidationFailureCase("scenario-rejects-unknown-button-bits", scenario);
    }

    private static SelfTestCase TestAuthorityStructuralFixtureW03()
    {
        StructuralWitnessEventBuffer fixture = StructuralWitnessFixture.Run("W03")!;
        string tick1 = Serialize(fixture.CaptureTick(1));
        string tick2 = Serialize(fixture.CaptureTick(2));
        bool passed = tick1.Contains("\"action\":\"unregister-deferred\"", StringComparison.Ordinal) &&
                      tick1.Contains("\"action\":\"unregister-flush\"", StringComparison.Ordinal) &&
                      tick1.Contains("\"slot\":3", StringComparison.Ordinal) &&
                      tick2.Contains("\"lifecycleEpoch\":2", StringComparison.Ordinal) &&
                      tick2.Contains("\"slot\":0", StringComparison.Ordinal);
        return BooleanCase("authority-structural-fixture-w03-hits-events", passed);
    }

    private static SelfTestCase TestAuthorityStructuralFixtureW04()
    {
        StructuralWitnessEventBuffer fixture = StructuralWitnessFixture.Run("W04")!;
        string tick1 = Serialize(fixture.CaptureTick(1));
        bool passed = tick1.Contains("\"searchStart\":0", StringComparison.Ordinal) &&
                      tick1.Contains("\"searchStart\":20", StringComparison.Ordinal) &&
                      tick1.Contains("\"searchStart\":50", StringComparison.Ordinal) &&
                      tick1.Contains("\"slot\":399", StringComparison.Ordinal) &&
                      !tick1.Contains("\"slot\":400", StringComparison.Ordinal);
        return BooleanCase("authority-structural-fixture-w04-hits-bands", passed);
    }

    private static SelfTestCase TestAuthorityStructuralFixtureW07()
    {
        StructuralWitnessEventBuffer fixture = StructuralWitnessFixture.Run("W07")!;
        string tick1 = Serialize(fixture.CaptureTick(1));
        string tick2 = Serialize(fixture.CaptureTick(2));
        string tick3 = Serialize(fixture.CaptureTick(3));
        bool passed = tick1 == "[]" &&
                      tick2.Contains("\"outcome\":\"kept\"", StringComparison.Ordinal) &&
                      tick2.Contains("\"observedHolderSlot\":0", StringComparison.Ordinal) &&
                      tick3.Contains("\"outcome\":\"cleared\"", StringComparison.Ordinal) &&
                      tick3.Contains("\"reason\":\"holder-mismatch\"", StringComparison.Ordinal) &&
                      tick3.Contains("\"after\":\"0/-1/-1\"", StringComparison.Ordinal) &&
                      tick3.Contains("\"targetAfterHolderSlot\":2", StringComparison.Ordinal);
        return BooleanCase("authority-structural-fixture-w07-runs-game-tick-link-validation", passed);
    }

    private static AuthorityScenario MinimalScenario()
    {
        return new AuthorityScenario
        {
            GameRoot = ".",
            Ticks = 1,
            Slots = [new ScenarioSlot { PlayerSlot = 0, Active = true, Ai = false }],
        };
    }

    private static SelfTestCase ValidationFailureCase(string name, AuthorityScenario scenario)
    {
        try
        {
            AuthorityTraceCommand.ValidateScenario(scenario);
            return BooleanCase(name, false);
        }
        catch (ArgumentException)
        {
            return BooleanCase(name, true);
        }
    }

    private static SelfTestCase BooleanCase(string name, bool passed)
    {
        return new SelfTestCase
        {
            Name = name,
            Expected = "passed",
            Actual = passed ? "passed" : "failed",
            Passed = passed,
        };
    }

    private static SelfTestCase Expect(
        string name,
        string authority,
        string unity,
        string expected,
        string comparisonProfile = TraceCompareCommand.StrictProfile,
        bool allowDiagnostic = false,
        bool expectedCertificate = false)
    {
        TraceCompareTestResult result = TraceCompareCommand.CompareTextForTest(
            authority,
            unity,
            comparisonProfile,
            allowDiagnostic);
        return new SelfTestCase
        {
            Name = name,
            Expected = expected,
            Actual = result.Status,
            Reason = result.Reason,
            Passed = string.Equals(expected, result.Status, StringComparison.Ordinal) &&
                     result.CertificateEligible == expectedCertificate,
        };
    }

    private static string[] BuildValidTrace()
    {
        string manifest = new('a', 64);
        SortedDictionary<string, int> buttonMask = new(StringComparer.Ordinal)
        {
            ["right"] = 1,
            ["left"] = 2,
            ["up"] = 4,
            ["down"] = 8,
            ["attack"] = 16,
            ["jump"] = 32,
            ["defend"] = 64,
        };
        object header = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["kind"] = "header",
            ["schema"] = TraceCompareCommand.TraceSchema,
            ["scenario"] = new SortedDictionary<string, object?>(StringComparer.Ordinal) { ["ticks"] = 2 },
            ["loadedChars"] = 1,
            ["maxRuntimeSlots"] = TraceCompareCommand.RuntimeSlotCount,
            ["expectedTicks"] = 2,
            ["manifest"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["schema"] = "ntsd-resolved-dat-manifest-v2",
                ["domain"] = "battle-logic",
                ["battleLogicSha256"] = manifest,
            },
            ["stageFixture"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["loaded"] = false,
                ["name"] = null,
                ["sha256"] = null,
                ["campaignCount"] = 0,
            },
            ["rngAfterBootstrap"] = new { seed = 1u, callCount = 0L },
            ["buttonMask"] = buttonMask,
            ["detail"] = "compact",
            ["dataFixture"] = "production",
        };
        return [Serialize(header), Serialize(BuildTick(1)), Serialize(BuildTick(2))];
    }

    private static string[] BuildValidStructuralTrace()
    {
        string[] result = BuildValidTrace();
        JsonObject header = JsonNode.Parse(result[0])!.AsObject();
        header["schema"] = TraceCompareCommand.StructuralTraceSchema;
        header["scenario"]!["structuralWitness"] = "W03";
        result[0] = header.ToJsonString(CanonicalJson.CompactOptions);

        for (int line = 1; line < result.Length; line++)
        {
            JsonObject tick = JsonNode.Parse(result[line])!.AsObject();
            int tickIndex = tick["tick"]!.GetValue<int>();
            tick["events"]!["structural"] = new JsonArray
            {
                JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["action"] = "scan",
                    ["actorSlot"] = 0,
                    ["after"] = "visited",
                    ["before"] = "active",
                    ["cursorSlot"] = 0,
                    ["lifecycleEpoch"] = 1,
                    ["pass"] = "late-entity-update",
                    ["searchEndExclusive"] = 400,
                    ["searchStart"] = 0,
                    ["slot"] = 0,
                    ["sourceKind"] = "general",
                    ["tick"] = tickIndex,
                }),
                JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["action"] = "scan",
                    ["actorSlot"] = 1,
                    ["after"] = "visited",
                    ["before"] = "active",
                    ["cursorSlot"] = 1,
                    ["lifecycleEpoch"] = 1,
                    ["pass"] = "late-entity-update",
                    ["searchEndExclusive"] = 400,
                    ["searchStart"] = 0,
                    ["slot"] = 1,
                    ["sourceKind"] = "general",
                    ["tick"] = tickIndex,
                }),
            };
            RehashEvents(tick);
            result[line] = tick.ToJsonString(CanonicalJson.CompactOptions);
        }
        return result;
    }

    private static string[] BuildValidPositiveLinkTrace()
    {
        string[] result = BuildValidStructuralTrace();
        JsonObject header = JsonNode.Parse(result[0])!.AsObject();
        header["scenario"]!["structuralWitness"] = "W07";
        result[0] = header.ToJsonString(CanonicalJson.CompactOptions);

        for (int line = 1; line < result.Length; line++)
        {
            JsonObject tick = JsonNode.Parse(result[line])!.AsObject();
            int tickIndex = tick["tick"]!.GetValue<int>();
            tick["events"]!["structural"] = new JsonArray
            {
                JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["action"] = "link-validation",
                    ["actorSlot"] = 0,
                    ["after"] = "1/1/1",
                    ["afterHeldWeaponSlot"] = 1,
                    ["afterLinkState"] = 1,
                    ["afterTargetSlot"] = 1,
                    ["before"] = "1/1/1",
                    ["beforeHeldWeaponSlot"] = 1,
                    ["beforeLinkState"] = 1,
                    ["beforeTargetSlot"] = 1,
                    ["cursorSlot"] = 0,
                    ["lifecycleEpoch"] = 1,
                    ["observedHolderSlot"] = 0,
                    ["outcome"] = "kept",
                    ["pass"] = "positive-link-validation",
                    ["reason"] = "reciprocal",
                    ["searchEndExclusive"] = -1,
                    ["searchStart"] = -1,
                    ["slot"] = 0,
                    ["sourceKind"] = "positive-link",
                    ["targetActive"] = true,
                    ["targetAfterHolderSlot"] = 0,
                    ["targetAfterLinkState"] = 0,
                    ["targetBeforeHolderSlot"] = 0,
                    ["targetBeforeLinkState"] = 0,
                    ["tick"] = tickIndex,
                }),
            };
            RehashEvents(tick);
            result[line] = tick.ToJsonString(CanonicalJson.CompactOptions);
        }
        return result;
    }

    private static string[] MutateStructuralSlot(string[] source, int slot)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["events"]!["structural"]![0]!["slot"] = slot;
        RehashEvents(tick);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] RemoveStructuralField(string[] source, string field)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["events"]!["structural"]![0]!.AsObject().Remove(field);
        RehashEvents(tick);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] MutateStructuralBefore(string[] source, string value)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["events"]!["structural"]![0]!["before"] = value;
        RehashEvents(tick);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] DeleteStructuralEvent(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["events"]!["structural"]!.AsArray().RemoveAt(0);
        RehashEvents(tick);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] ReverseStructuralEvents(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        JsonArray structural = tick["events"]!["structural"]!.AsArray();
        JsonNode? first = structural[0]?.DeepClone();
        JsonNode? second = structural[1]?.DeepClone();
        structural[0] = second;
        structural[1] = first;
        RehashEvents(tick);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static void RehashEvents(JsonObject tick)
    {
        JsonObject hashes = tick["hashes"]!.AsObject();
        hashes["events"] = CanonicalJson.Sha256(CanonicalJson.Canonicalize(tick["events"]));
        var overall = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (string domain in new[] { "input", "rng", "world", "slots", "aRest", "vRest", "stats", "events" })
            overall[domain] = hashes[domain]!.GetValue<string>();
        hashes["overall"] = CanonicalJson.Sha256(overall);
    }

    private static object BuildTick(int tick)
    {
        object slotBody = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["runtimeSlot"] = 0,
            ["currentDataOid"] = null,
            ["runtime"] = new SortedDictionary<string, object?>(StringComparer.Ordinal),
        };
        string[] commitments = Enumerable.Repeat(new string('b', 64), TraceCompareCommand.RuntimeSlotCount).ToArray();
        commitments[0] = CanonicalJson.Sha256(slotBody);
        object input = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["tickIndex"] = tick,
            ["players"] = Array.Empty<object>(),
        };
        object rng = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["seed"] = (uint)tick,
            ["callCount"] = (long)tick,
        };
        object world = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["cameraVel"] = 0,
            ["cameraX"] = 0,
            ["gameTick"] = tick,
            ["objectCount"] = 0,
            ["runtime"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["stage"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["cameraVel"] = 0,
                    ["cameraX"] = 0,
                },
            },
        };
        object slots = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["count"] = TraceCompareCommand.RuntimeSlotCount,
            ["commitments"] = commitments,
        };
        object aRest = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = TraceCompareCommand.RuntimeSlotCount,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = Array.Empty<object>(),
        };
        object vRest = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = TraceCompareCommand.RuntimeSlotCount,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = Array.Empty<object>(),
        };
        object stats = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["kill"] = new[] { 0, 0, 0 },
            ["damage"] = new[] { 0, 0, 0 },
        };
        object events = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["pendingSounds"] = Array.Empty<object>(),
        };

        SortedDictionary<string, string> hashes = new(StringComparer.Ordinal)
        {
            ["input"] = CanonicalJson.Sha256(input),
            ["rng"] = CanonicalJson.Sha256(rng),
            ["world"] = CanonicalJson.Sha256(world),
            ["slots"] = CanonicalJson.Sha256(slots),
            ["aRest"] = CanonicalJson.Sha256(aRest),
            ["vRest"] = CanonicalJson.Sha256(vRest),
            ["stats"] = CanonicalJson.Sha256(stats),
            ["events"] = CanonicalJson.Sha256(events),
        };
        hashes["overall"] = CanonicalJson.Sha256(hashes);

        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["kind"] = "tick",
            ["tick"] = tick,
            ["hashes"] = hashes,
            ["input"] = input,
            ["rng"] = rng,
            ["world"] = world,
            ["objectCount"] = 0,
            ["slots"] = new[] { slotBody },
            ["slotCommitments"] = commitments,
            ["aRest"] = aRest,
            ["vRest"] = vRest,
            ["stats"] = stats,
            ["events"] = events,
        };
    }

    private static string[] MutateBody(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["world"]!["gameTick"] = 999;
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] MutateHash(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["hashes"]!["world"] = new string('0', 64);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] MutateSlotBody(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["slots"]![0]!["currentDataOid"] = 123;
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] MutateSlotCommitment(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["slotCommitments"]![1] = new string('c', 64);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] MutateManifest(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject header = JsonNode.Parse(result[0])!.AsObject();
        header["manifest"]!["battleLogicSha256"] = new string('c', 64);
        result[0] = header.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] MutateDataFixture(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject header = JsonNode.Parse(result[0])!.AsObject();
        header["dataFixture"] = "authority-dat-diagnostic";
        result[0] = header.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static string[] MutateCamera(string[] source)
    {
        string[] result = source.ToArray();
        for (int line = 1; line < result.Length; line++)
        {
            JsonObject tick = JsonNode.Parse(result[line])!.AsObject();
            JsonObject world = tick["world"]!.AsObject();
            world["cameraX"] = 19;
            world["cameraVel"] = -3;
            world["runtime"]!["stage"]!["cameraX"] = 19;
            world["runtime"]!["stage"]!["cameraVel"] = -3;
            RehashTick(tick);
            result[line] = tick.ToJsonString(CanonicalJson.CompactOptions);
        }
        return result;
    }

    private static string[] MutateNonCameraWorld(string[] source)
    {
        string[] result = source.ToArray();
        JsonObject tick = JsonNode.Parse(result[1])!.AsObject();
        tick["world"]!["gameTick"] = 999;
        RehashTick(tick);
        result[1] = tick.ToJsonString(CanonicalJson.CompactOptions);
        return result;
    }

    private static void RehashTick(JsonObject tick)
    {
        JsonObject hashes = tick["hashes"]!.AsObject();
        hashes["world"] = CanonicalJson.Sha256(CanonicalJson.Canonicalize(tick["world"]));
        var overall = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["input"] = hashes["input"]!.GetValue<string>(),
            ["rng"] = hashes["rng"]!.GetValue<string>(),
            ["world"] = hashes["world"]!.GetValue<string>(),
            ["slots"] = hashes["slots"]!.GetValue<string>(),
            ["aRest"] = hashes["aRest"]!.GetValue<string>(),
            ["vRest"] = hashes["vRest"]!.GetValue<string>(),
            ["stats"] = hashes["stats"]!.GetValue<string>(),
            ["events"] = hashes["events"]!.GetValue<string>(),
        };
        hashes["overall"] = CanonicalJson.Sha256(overall);
    }

    private static string Serialize(object value)
        => JsonSerializer.Serialize(value, CanonicalJson.CompactOptions);

    private static string Join(IEnumerable<string> lines)
        => string.Join(Environment.NewLine, lines) + Environment.NewLine;

    private sealed class SelfTestReport
    {
        public string Schema { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public List<SelfTestCase> Cases { get; set; } = [];
    }

    private sealed class SelfTestCase
    {
        public string Name { get; set; } = string.Empty;
        public string Expected { get; set; } = string.Empty;
        public string Actual { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public bool Passed { get; set; }
    }
}
