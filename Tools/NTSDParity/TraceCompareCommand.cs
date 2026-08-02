using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSDParity;

internal static class TraceCompareCommand
{
    internal const string TraceSchema = "ntsd-battle-trace-v3";
    internal const string StructuralTraceSchema = "ntsd-battle-trace-v4";
    internal const int RuntimeSlotCount = 400;
    internal const string StrictProfile = "strict";
    internal const string FixedWorldCameraProfile = "fixed-world-camera";
    private const string ProductionDataFixture = "production";
    private const string AuthorityDiagnosticDataFixture = "authority-dat-diagnostic";

    private static readonly string[] DomainNames =
    [
        "input",
        "rng",
        "world",
        "slots",
        "aRest",
        "vRest",
        "stats",
        "events",
    ];

    private static readonly SortedDictionary<string, int> ExpectedButtonMask = new(StringComparer.Ordinal)
    {
        ["right"] = 1,
        ["left"] = 2,
        ["up"] = 4,
        ["down"] = 8,
        ["attack"] = 16,
        ["jump"] = 32,
        ["defend"] = 64,
    };

    public static int Run(string[] args)
    {
        CommandLine cli = CommandLine.Parse(args);
        string authorityPath = Path.GetFullPath(cli.Require("--authority"));
        string unityPath = Path.GetFullPath(cli.Require("--unity"));
        string outputPath = RepositoryPaths.ResolveOutput(cli.Require("--output"));
        bool fullFieldDiff = string.Equals(cli.Get("--detail") ?? "hashes", "full", StringComparison.Ordinal);
        if ((cli.Get("--detail") ?? "hashes") is not ("hashes" or "full"))
            throw new ArgumentException("--detail must be 'hashes' or 'full'.");
        string comparisonProfile = cli.Get("--profile") ?? StrictProfile;
        ValidateComparisonProfile(comparisonProfile);
        bool allowDiagnostic = cli.Has("--allow-diagnostic");
        bool requireCertificate = cli.Has("--require-certificate");

        using StreamReader authority = new(authorityPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using StreamReader unity = new(unityPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        TraceCompareReport report = Compare(
            authority,
            unity,
            Path.GetFileName(authorityPath),
            Path.GetFileName(unityPath),
            fullFieldDiff,
            comparisonProfile,
            allowDiagnostic);

        File.WriteAllText(outputPath, JsonSerializer.Serialize(report, JsonProjection.SerializerOptions), new UTF8Encoding(false));
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} certificate={report.CertificateEligible} " +
            $"ticksCompared={report.TicksCompared} firstDifferenceTick={report.FirstDifference?.Tick}");
        return requireCertificate
            ? report.CertificateEligible ? 0 : 1
            : report.Status.StartsWith("equal", StringComparison.Ordinal) ? 0 : 1;
    }

    internal static TraceCompareTestResult CompareTextForTest(
        string authority,
        string unity,
        string comparisonProfile = StrictProfile,
        bool allowDiagnostic = false)
    {
        ValidateComparisonProfile(comparisonProfile);
        using StringReader authorityReader = new(authority);
        using StringReader unityReader = new(unity);
        TraceCompareReport report = Compare(
            authorityReader,
            unityReader,
            "authority",
            "unity",
            fullFieldDiff: true,
            comparisonProfile,
            allowDiagnostic);
        return new TraceCompareTestResult(report.Status, report.FirstDifference?.Reason, report.CertificateEligible);
    }

    private static TraceCompareReport Compare(
        TextReader authorityReader,
        TextReader unityReader,
        string authorityName,
        string unityName,
        bool fullFieldDiff,
        string comparisonProfile,
        bool allowDiagnostic)
    {
        TraceCompareReport report = new()
        {
            Schema = "ntsd-streaming-trace-compare-v2",
            Authority = authorityName,
            Unity = unityName,
            ComparisonProfile = comparisonProfile,
        };

        string? authorityHeaderLine = ReadNextLine(authorityReader);
        string? unityHeaderLine = ReadNextLine(unityReader);
        if (authorityHeaderLine is null || unityHeaderLine is null)
            return Fail(report, "header", 0, "missing-header", authorityHeaderLine, unityHeaderLine, fullFieldDiff);

        HeaderContract authorityHeader;
        HeaderContract unityHeader;
        try
        {
            authorityHeader = ValidateHeader(authorityHeaderLine, "authority");
            unityHeader = ValidateHeader(unityHeaderLine, "unity");
        }
        catch (Exception ex)
        {
            return Fail(report, "header", 0, "invalid-header: " + ex.Message, authorityHeaderLine, unityHeaderLine, fullFieldDiff);
        }

        report.AuthorityManifestSha256 = authorityHeader.Manifest;
        report.UnityManifestSha256 = unityHeader.Manifest;
        report.AuthorityDetail = authorityHeader.Detail;
        report.UnityDetail = unityHeader.Detail;
        report.AuthorityDataFixture = authorityHeader.DataFixture;
        report.UnityDataFixture = unityHeader.DataFixture;
        report.ExpectedTicks = authorityHeader.ExpectedTicks;

        bool diagnosticComparison =
            authorityHeader.DataFixture != ProductionDataFixture ||
            unityHeader.DataFixture != ProductionDataFixture;
        report.DiagnosticComparison = diagnosticComparison;
        string? headerMismatch = CompareHeaders(authorityHeader, unityHeader, allowDiagnostic);
        if (headerMismatch is not null)
            return Fail(report, "header", 0, headerMismatch, authorityHeaderLine, unityHeaderLine, fullFieldDiff);

        for (int expectedTick = 1; expectedTick <= authorityHeader.ExpectedTicks; expectedTick++)
        {
            string? authorityLine = ReadNextLine(authorityReader);
            string? unityLine = ReadNextLine(unityReader);
            if (authorityLine is null || unityLine is null)
                return Fail(report, "stream", expectedTick, "missing-required-tick", authorityLine, unityLine, fullFieldDiff);

            ValidatedTick authorityTick;
            ValidatedTick unityTick;
            try
            {
                authorityTick = ValidateTick(authorityLine, authorityHeader, expectedTick, "authority", comparisonProfile);
            }
            catch (Exception ex)
            {
                return Fail(report, "authority", expectedTick, "invalid-tick: " + ex.Message, authorityLine, unityLine, fullFieldDiff);
            }
            try
            {
                unityTick = ValidateTick(unityLine, unityHeader, expectedTick, "unity", comparisonProfile);
            }
            catch (Exception ex)
            {
                return Fail(report, "unity", expectedTick, "invalid-tick: " + ex.Message, authorityLine, unityLine, fullFieldDiff);
            }

            foreach (string domain in DomainNames.Append("overall"))
            {
                string authorityHash = authorityTick.ComparisonHashes[domain];
                string unityHash = unityTick.ComparisonHashes[domain];
                if (!string.Equals(authorityHash, unityHash, StringComparison.Ordinal))
                    return Fail(report, domain, expectedTick, "domain-mismatch", authorityLine, unityLine, fullFieldDiff);
            }

            foreach (string domain in DomainNames.Where(value => value != "slots"))
            {
                if (!JsonNode.DeepEquals(authorityTick.ComparisonDomains[domain], unityTick.ComparisonDomains[domain]))
                    return Fail(report, domain, expectedTick, "domain-body-mismatch", authorityLine, unityLine, fullFieldDiff);
            }
            if (authorityHeader.Detail == "full" && unityHeader.Detail == "full" &&
                !JsonNode.DeepEquals(authorityTick.OpenedSlotBodies, unityTick.OpenedSlotBodies))
            {
                return Fail(report, "slots", expectedTick, "slot-body-mismatch", authorityLine, unityLine, fullFieldDiff);
            }

            report.TicksCompared++;
        }

        string? authorityExtra = ReadNextLine(authorityReader);
        string? unityExtra = ReadNextLine(unityReader);
        if (authorityExtra is not null || unityExtra is not null)
            return Fail(report, "stream", authorityHeader.ExpectedTicks + 1, "unexpected-extra-tick", authorityExtra, unityExtra, fullFieldDiff);

        report.CertificateEligible = !diagnosticComparison &&
                                     authorityHeader.Schema == TraceSchema &&
                                     authorityHeader.Detail == "full" &&
                                     unityHeader.Detail == "full";
        report.CertificateClass = report.CertificateEligible
            ? comparisonProfile == StrictProfile
                ? "strict-production-v1"
                : "profiled-fixed-world-camera-v1"
            : "none";
        report.Status = diagnosticComparison
            ? "equal-diagnostic"
            : report.CertificateEligible ? "equal" : "equal-commitments";
        return report;
    }

    private static HeaderContract ValidateHeader(string line, string producer)
    {
        JsonObject header = ParseObject(line, producer + " header");
        RequireString(header, "kind", "header");
        string schema = RequireString(header, "schema");
        if (schema is not (TraceSchema or StructuralTraceSchema))
            throw new InvalidDataException("schema must be ntsd-battle-trace-v3 or ntsd-battle-trace-v4");

        int expectedTicks = RequireInt(header, "expectedTicks");
        if (expectedTicks <= 0)
            throw new InvalidDataException("expectedTicks must be positive");
        int maxRuntimeSlots = RequireInt(header, "maxRuntimeSlots");
        if (maxRuntimeSlots != RuntimeSlotCount)
            throw new InvalidDataException($"maxRuntimeSlots must be {RuntimeSlotCount}");
        int loadedChars = RequireInt(header, "loadedChars");
        if (loadedChars <= 0)
            throw new InvalidDataException("loadedChars must be positive");

        string detail = RequireString(header, "detail");
        if (detail is not ("compact" or "full"))
            throw new InvalidDataException("detail must be compact or full");
        string dataFixture = RequireString(header, "dataFixture");
        if (dataFixture is not (ProductionDataFixture or AuthorityDiagnosticDataFixture))
            throw new InvalidDataException("dataFixture must be production or authority-dat-diagnostic");

        JsonObject scenario = RequireObject(header, "scenario");
        if (RequireInt(scenario, "ticks") != expectedTicks)
            throw new InvalidDataException("scenario.ticks does not match expectedTicks");

        JsonObject manifest = RequireObject(header, "manifest");
        RequireString(manifest, "schema", "ntsd-resolved-dat-manifest-v2");
        RequireString(manifest, "domain", "battle-logic");
        string manifestHash = RequireHash(manifest, "battleLogicSha256");

        JsonObject buttonMask = RequireObject(header, "buttonMask");
        if (!JsonNode.DeepEquals(CanonicalJson.Canonicalize(buttonMask), CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(ExpectedButtonMask))))
            throw new InvalidDataException("buttonMask does not match the v3 contract");

        JsonObject rng = RequireObject(header, "rngAfterBootstrap");
        _ = RequireUInt(rng, "seed");
        if (RequireLong(rng, "callCount") < 0)
            throw new InvalidDataException("rngAfterBootstrap.callCount must be nonnegative");

        JsonObject stageFixture = RequireObject(header, "stageFixture");
        bool fixtureLoaded = RequireBool(stageFixture, "loaded");
        int fixtureCampaignCount = RequireInt(stageFixture, "campaignCount");
        if (fixtureCampaignCount < 0)
            throw new InvalidDataException("stageFixture.campaignCount must be nonnegative");
        if (fixtureLoaded)
            _ = RequireHash(stageFixture, "sha256");

        return new HeaderContract
        {
            Schema = schema,
            ExpectedTicks = expectedTicks,
            Detail = detail,
            DataFixture = dataFixture,
            LoadedChars = loadedChars,
            Manifest = manifestHash,
            ScenarioHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(scenario)),
            StageFixtureHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(stageFixture)),
            ButtonMaskHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(buttonMask)),
            BootstrapRngHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(rng)),
        };
    }

    private static string? CompareHeaders(
        HeaderContract authority,
        HeaderContract unity,
        bool allowDiagnostic)
    {
        if (authority.ExpectedTicks != unity.ExpectedTicks)
            return "expectedTicks";
        if (!string.Equals(authority.Schema, unity.Schema, StringComparison.Ordinal))
            return "schema";
        if (authority.LoadedChars != unity.LoadedChars)
            return "loadedChars";
        if (!string.Equals(authority.Manifest, unity.Manifest, StringComparison.Ordinal))
            return "manifest";
        bool diagnostic = authority.DataFixture != ProductionDataFixture ||
                          unity.DataFixture != ProductionDataFixture;
        if (diagnostic && !allowDiagnostic)
            return "diagnostic-data-fixture-requires-explicit-opt-in";
        if (!string.Equals(authority.ScenarioHash, unity.ScenarioHash, StringComparison.Ordinal))
            return "scenario";
        if (!string.Equals(authority.StageFixtureHash, unity.StageFixtureHash, StringComparison.Ordinal))
            return "stageFixture";
        if (!string.Equals(authority.ButtonMaskHash, unity.ButtonMaskHash, StringComparison.Ordinal))
            return "buttonMask";
        if (!string.Equals(authority.BootstrapRngHash, unity.BootstrapRngHash, StringComparison.Ordinal))
            return "rngAfterBootstrap";
        return null;
    }

    private static ValidatedTick ValidateTick(
        string line,
        HeaderContract header,
        int expectedTick,
        string producer,
        string comparisonProfile)
    {
        JsonObject tick = ParseObject(line, producer + " tick");
        RequireString(tick, "kind", "tick");
        int actualTick = RequireInt(tick, "tick");
        if (actualTick != expectedTick)
            throw new InvalidDataException($"tick index {actualTick} is not expected contiguous tick {expectedTick}");

        JsonObject worldBody = RequireObject(tick, "world");
        int topLevelObjectCount = RequireInt(tick, "objectCount");
        if (RequireInt(worldBody, "objectCount") != topLevelObjectCount)
            throw new InvalidDataException("top-level objectCount does not match world.objectCount");

        JsonObject eventsBody = RequireObject(tick, "events");
        if (header.Schema == StructuralTraceSchema)
            ValidateStructuralEvents(eventsBody, expectedTick);

        Dictionary<string, JsonNode?> domains = new(StringComparer.Ordinal)
        {
            ["input"] = CanonicalJson.Canonicalize(RequireNode(tick, "input")),
            ["rng"] = CanonicalJson.Canonicalize(RequireNode(tick, "rng")),
            ["world"] = CanonicalJson.Canonicalize(worldBody),
            ["aRest"] = NormalizeARest(RequireObject(tick, "aRest")),
            ["vRest"] = NormalizeVRest(RequireObject(tick, "vRest")),
            ["stats"] = CanonicalJson.Canonicalize(RequireNode(tick, "stats")),
            ["events"] = CanonicalJson.Canonicalize(eventsBody),
        };
        SlotValidation slots = ValidateSlots(tick, header.Detail);
        domains["slots"] = slots.CommitmentDomain;

        SortedDictionary<string, string> computed = new(StringComparer.Ordinal);
        foreach (string domain in DomainNames)
            computed[domain] = CanonicalJson.Sha256(domains[domain]);
        computed["overall"] = CanonicalJson.Sha256(computed);

        JsonObject reported = RequireObject(tick, "hashes");
        foreach (string domain in DomainNames.Append("overall"))
        {
            string reportedHash = RequireHash(reported, domain);
            if (!string.Equals(reportedHash, computed[domain], StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"{domain} body hash mismatch (reported {reportedHash}, computed {computed[domain]})");
            }
        }

        Dictionary<string, JsonNode?> comparisonDomains = BuildComparisonDomains(domains, comparisonProfile);
        SortedDictionary<string, string> comparisonHashes = new(StringComparer.Ordinal);
        foreach (string domain in DomainNames)
            comparisonHashes[domain] = CanonicalJson.Sha256(comparisonDomains[domain]);
        comparisonHashes["overall"] = CanonicalJson.Sha256(comparisonHashes);

        return new ValidatedTick(computed, domains, comparisonHashes, comparisonDomains, slots.OpenedBodies);
    }

    private static Dictionary<string, JsonNode?> BuildComparisonDomains(
        Dictionary<string, JsonNode?> validatedDomains,
        string comparisonProfile)
    {
        var result = new Dictionary<string, JsonNode?>(validatedDomains, StringComparer.Ordinal);
        if (comparisonProfile == StrictProfile)
            return result;

        JsonObject world = validatedDomains["world"]?.DeepClone() as JsonObject
            ?? throw new InvalidDataException("world domain must be an object");
        _ = RequireInt(world, "cameraX");
        _ = RequireInt(world, "cameraVel");
        world["cameraX"] = 0;
        world["cameraVel"] = 0;
        JsonObject runtime = RequireObject(world, "runtime");
        JsonObject stage = RequireObject(runtime, "stage");
        _ = RequireInt(stage, "cameraX");
        _ = RequireInt(stage, "cameraVel");
        stage["cameraX"] = 0;
        stage["cameraVel"] = 0;
        result["world"] = CanonicalJson.Canonicalize(world);
        return result;
    }

    private static void ValidateComparisonProfile(string comparisonProfile)
    {
        if (comparisonProfile is not (StrictProfile or FixedWorldCameraProfile))
        {
            throw new ArgumentException(
                $"--profile must be '{StrictProfile}' or '{FixedWorldCameraProfile}'.");
        }
    }

    private static void ValidateStructuralEvents(JsonObject events, int expectedTick)
    {
        JsonArray structural = RequireArray(events, "structural");
        foreach (JsonNode? node in structural)
        {
            JsonObject item = node as JsonObject
                ?? throw new InvalidDataException("structural event must be an object");
            if (RequireInt(item, "tick") != expectedTick)
                throw new InvalidDataException("structural event tick must match its containing tick");
            _ = RequireString(item, "pass");
            _ = RequireString(item, "action");
            _ = RequireString(item, "before");
            _ = RequireString(item, "after");
            _ = RequireString(item, "sourceKind");
            int cursorSlot = RequireInt(item, "cursorSlot");
            int actorSlot = RequireInt(item, "actorSlot");
            int slot = RequireInt(item, "slot");
            int searchStart = RequireInt(item, "searchStart");
            int searchEndExclusive = RequireInt(item, "searchEndExclusive");
            int lifecycleEpoch = RequireInt(item, "lifecycleEpoch");
            if (cursorSlot is < -1 or >= RuntimeSlotCount ||
                actorSlot is < -1 or >= RuntimeSlotCount ||
                slot is < -1 or >= RuntimeSlotCount)
            {
                throw new InvalidDataException("structural event slot fields must stay within -1..399");
            }
            if (searchStart is < -1 or > RuntimeSlotCount ||
                searchEndExclusive is < -1 or > RuntimeSlotCount ||
                (searchStart >= 0 && searchEndExclusive <= searchStart))
            {
                throw new InvalidDataException("structural event search range is invalid");
            }
            if (lifecycleEpoch < 0)
                throw new InvalidDataException("structural lifecycleEpoch must be nonnegative");
            if (string.Equals(RequireString(item, "action"), "link-validation", StringComparison.Ordinal))
                ValidatePositiveLinkStructuralEvent(item, actorSlot, cursorSlot, slot);
        }
    }

    private static void ValidatePositiveLinkStructuralEvent(
        JsonObject item,
        int actorSlot,
        int cursorSlot,
        int slot)
    {
        int beforeLinkState = RequireInt(item, "beforeLinkState");
        int beforeTargetSlot = RequireInt(item, "beforeTargetSlot");
        int beforeHeldWeaponSlot = RequireInt(item, "beforeHeldWeaponSlot");
        int afterLinkState = RequireInt(item, "afterLinkState");
        int afterTargetSlot = RequireInt(item, "afterTargetSlot");
        int afterHeldWeaponSlot = RequireInt(item, "afterHeldWeaponSlot");
        bool targetActive = RequireBool(item, "targetActive");
        int observedHolderSlot = RequireInt(item, "observedHolderSlot");
        string outcome = RequireString(item, "outcome");
        string reason = RequireString(item, "reason");
        int targetBeforeHolderSlot = RequireInt(item, "targetBeforeHolderSlot");
        int targetBeforeLinkState = RequireInt(item, "targetBeforeLinkState");
        int targetAfterHolderSlot = RequireInt(item, "targetAfterHolderSlot");
        int targetAfterLinkState = RequireInt(item, "targetAfterLinkState");

        int[] linkSlots =
        [
            beforeTargetSlot,
            beforeHeldWeaponSlot,
            afterTargetSlot,
            afterHeldWeaponSlot,
            observedHolderSlot,
            targetBeforeHolderSlot,
            targetAfterHolderSlot,
        ];
        if (linkSlots.Any(value => value is < -1 or >= RuntimeSlotCount))
            throw new InvalidDataException("positive-link event slot fields must stay within -1..399");
        if (actorSlot != slot || cursorSlot != slot)
            throw new InvalidDataException("positive-link event actor/cursor/slot must identify the holder slot");
        if (RequireString(item, "before") !=
            $"{beforeLinkState}/{beforeTargetSlot}/{beforeHeldWeaponSlot}" ||
            RequireString(item, "after") !=
            $"{afterLinkState}/{afterTargetSlot}/{afterHeldWeaponSlot}")
        {
            throw new InvalidDataException("positive-link before/after must match the canonical forward fields");
        }
        if (targetBeforeHolderSlot != observedHolderSlot)
            throw new InvalidDataException("positive-link observed holder must match target reverse before state");
        if (targetBeforeHolderSlot != targetAfterHolderSlot ||
            targetBeforeLinkState != targetAfterLinkState)
        {
            throw new InvalidDataException("positive-link validation must not mutate target reverse fields");
        }

        if (outcome == "kept")
        {
            if (reason != "reciprocal" || !targetActive || observedHolderSlot != actorSlot ||
                beforeLinkState <= 0 || afterLinkState != beforeLinkState ||
                afterTargetSlot != beforeTargetSlot || afterHeldWeaponSlot != beforeHeldWeaponSlot)
            {
                throw new InvalidDataException("positive-link kept event is not reciprocal and unchanged");
            }
            return;
        }

        if (outcome != "cleared" || reason is not ("holder-mismatch" or "target-inactive") ||
            beforeLinkState <= 0 || afterLinkState != 0 ||
            afterTargetSlot != -1 || afterHeldWeaponSlot != -1)
        {
            throw new InvalidDataException("positive-link cleared event has invalid outcome fields");
        }
        if (reason == "holder-mismatch" && (!targetActive || observedHolderSlot == actorSlot))
            throw new InvalidDataException("positive-link holder-mismatch event must observe a different active holder");
        if (reason == "target-inactive" && targetActive)
            throw new InvalidDataException("positive-link target-inactive event must observe an inactive target");
    }

    private static SlotValidation ValidateSlots(JsonObject tick, string detail)
    {
        JsonArray commitmentsNode = RequireArray(tick, "slotCommitments");
        if (commitmentsNode.Count != RuntimeSlotCount)
            throw new InvalidDataException($"slotCommitments must contain {RuntimeSlotCount} entries");

        string[] commitments = new string[RuntimeSlotCount];
        for (int slot = 0; slot < RuntimeSlotCount; slot++)
        {
            commitments[slot] = commitmentsNode[slot]?.GetValue<string>()
                ?? throw new InvalidDataException($"slot commitment {slot} is not a string");
            ValidateHash(commitments[slot], $"slot commitment {slot}");
        }

        JsonArray slots = RequireArray(tick, "slots");
        bool[] opened = new bool[RuntimeSlotCount];
        JsonNode?[] openedBodies = new JsonNode?[RuntimeSlotCount];
        foreach (JsonNode? slotNode in slots)
        {
            if (slotNode is not JsonObject slotObject)
                throw new InvalidDataException("slot body must be an object");
            int slot = RequireInt(slotObject, "runtimeSlot");
            if (slot < 0 || slot >= RuntimeSlotCount || opened[slot])
                throw new InvalidDataException($"invalid or duplicate runtime slot {slot}");
            opened[slot] = true;
            JsonNode? canonicalBody = CanonicalJson.Canonicalize(slotObject);
            openedBodies[slot] = canonicalBody;
            string bodyHash = CanonicalJson.Sha256(canonicalBody);
            if (!string.Equals(bodyHash, commitments[slot], StringComparison.Ordinal))
                throw new InvalidDataException($"slot {slot} body does not match its commitment");
        }

        if (detail == "full" && (slots.Count != RuntimeSlotCount || opened.Any(value => !value)))
            throw new InvalidDataException("full trace must open all 400 runtime slot commitments");

        JsonNode commitmentDomain = CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["count"] = RuntimeSlotCount,
            ["commitments"] = commitments,
        }))!;
        JsonNode openedDomain = CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(openedBodies))!;
        return new SlotValidation(commitmentDomain, openedDomain);
    }

    private static JsonNode NormalizeARest(JsonObject source)
    {
        if (RequireInt(source, "dimension") != RuntimeSlotCount)
            throw new InvalidDataException("aRest dimension must be 400");
        string encoding = RequireString(source, "encoding");
        SortedDictionary<int, int> values = new();
        if (encoding == "sparse-nonzero")
        {
            foreach (JsonNode? node in RequireArray(source, "entries"))
            {
                JsonObject entry = node as JsonObject ?? throw new InvalidDataException("aRest entry must be an object");
                int slot = RequireInt(entry, "slot");
                int value = RequireInt(entry, "value");
                if (slot < 0 || slot >= RuntimeSlotCount || value == 0 || !values.TryAdd(slot, value))
                    throw new InvalidDataException("invalid aRest sparse entry");
            }
        }
        else if (encoding == "full")
        {
            JsonArray full = RequireArray(source, "values");
            if (full.Count != RuntimeSlotCount)
                throw new InvalidDataException("aRest full values must contain 400 entries");
            for (int slot = 0; slot < RuntimeSlotCount; slot++)
            {
                int value = full[slot]?.GetValue<int>() ?? throw new InvalidDataException("invalid aRest value");
                if (value != 0)
                    values.Add(slot, value);
            }
        }
        else
        {
            throw new InvalidDataException("unsupported aRest encoding");
        }

        return CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = RuntimeSlotCount,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = values.Select(pair => new { slot = pair.Key, value = pair.Value }).ToArray(),
        }))!;
    }

    private static JsonNode NormalizeVRest(JsonObject source)
    {
        if (RequireInt(source, "dimension") != RuntimeSlotCount)
            throw new InvalidDataException("vRest dimension must be 400");
        string encoding = RequireString(source, "encoding");
        SortedDictionary<(int First, int Second), int> values = new();
        if (encoding == "sparse-nonzero")
        {
            foreach (JsonNode? node in RequireArray(source, "entries"))
            {
                JsonObject entry = node as JsonObject ?? throw new InvalidDataException("vRest entry must be an object");
                int first = RequireInt(entry, "attackerSlot");
                int second = RequireInt(entry, "victimSlot");
                int value = RequireInt(entry, "value");
                if (first < 0 || first >= RuntimeSlotCount || second < 0 || second >= RuntimeSlotCount ||
                    value == 0 || !values.TryAdd((first, second), value))
                {
                    throw new InvalidDataException("invalid vRest sparse entry");
                }
            }
        }
        else if (encoding == "full-row-major")
        {
            JsonArray rows = RequireArray(source, "values");
            if (rows.Count != RuntimeSlotCount)
                throw new InvalidDataException("vRest full matrix must contain 400 rows");
            for (int first = 0; first < RuntimeSlotCount; first++)
            {
                JsonArray row = rows[first] as JsonArray ?? throw new InvalidDataException("vRest row must be an array");
                if (row.Count != RuntimeSlotCount)
                    throw new InvalidDataException("vRest full row must contain 400 entries");
                for (int second = 0; second < RuntimeSlotCount; second++)
                {
                    int value = row[second]?.GetValue<int>() ?? throw new InvalidDataException("invalid vRest value");
                    if (value != 0)
                        values.Add((first, second), value);
                }
            }
        }
        else
        {
            throw new InvalidDataException("unsupported vRest encoding");
        }

        return CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = RuntimeSlotCount,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = values.Select(pair => new
            {
                attackerSlot = pair.Key.First,
                victimSlot = pair.Key.Second,
                value = pair.Value,
            }).ToArray(),
        }))!;
    }

    private static TraceCompareReport Fail(
        TraceCompareReport report,
        string domain,
        int tick,
        string reason,
        string? authorityLine,
        string? unityLine,
        bool fullFieldDiff)
    {
        report.Status = "different";
        report.CertificateEligible = false;
        report.FirstDifference = new TraceDifference { Tick = tick, Domain = domain, Reason = reason };
        if (fullFieldDiff && authorityLine is not null && unityLine is not null)
        {
            try
            {
                CanonicalJson.CompareNodes(JsonNode.Parse(authorityLine), JsonNode.Parse(unityLine), "$", report.FirstDifference.Fields, 512);
                report.FirstDifference.FieldDiffTruncated = report.FirstDifference.Fields.Count >= 512;
            }
            catch (JsonException)
            {
                // The contract error already identifies malformed JSON.
            }
        }
        return report;
    }

    private static JsonObject ParseObject(string json, string description)
        => JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException(description + " must be a JSON object");

    private static JsonNode RequireNode(JsonObject owner, string property)
        => owner[property] ?? throw new InvalidDataException($"missing property '{property}'");

    private static JsonObject RequireObject(JsonObject owner, string property)
        => owner[property] as JsonObject ?? throw new InvalidDataException($"property '{property}' must be an object");

    private static JsonArray RequireArray(JsonObject owner, string property)
        => owner[property] as JsonArray ?? throw new InvalidDataException($"property '{property}' must be an array");

    private static string RequireString(JsonObject owner, string property)
        => owner[property]?.GetValue<string>() ?? throw new InvalidDataException($"property '{property}' must be a string");

    private static string RequireString(JsonObject owner, string property, string expected)
    {
        string actual = RequireString(owner, property);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidDataException($"property '{property}' must be '{expected}'");
        return actual;
    }

    private static int RequireInt(JsonObject owner, string property)
        => owner[property]?.GetValue<int>() ?? throw new InvalidDataException($"property '{property}' must be an integer");

    private static uint RequireUInt(JsonObject owner, string property)
        => owner[property]?.GetValue<uint>() ?? throw new InvalidDataException($"property '{property}' must be an unsigned integer");

    private static long RequireLong(JsonObject owner, string property)
        => owner[property]?.GetValue<long>() ?? throw new InvalidDataException($"property '{property}' must be an integer");

    private static bool RequireBool(JsonObject owner, string property)
        => owner[property]?.GetValue<bool>() ?? throw new InvalidDataException($"property '{property}' must be a boolean");

    private static string RequireHash(JsonObject owner, string property)
    {
        string hash = RequireString(owner, property);
        ValidateHash(hash, property);
        return hash;
    }

    private static void ValidateHash(string hash, string description)
    {
        if (hash.Length != 64 || hash.Any(value => !char.IsAsciiHexDigit(value)) ||
            !string.Equals(hash, hash.ToLowerInvariant(), StringComparison.Ordinal))
        {
            throw new InvalidDataException(description + " must be a lowercase SHA-256 hex digest");
        }
    }

    private static string? ReadNextLine(TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }
        return null;
    }

    private sealed class HeaderContract
    {
        public string Schema { get; set; } = string.Empty;
        public int ExpectedTicks { get; set; }
        public int LoadedChars { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string DataFixture { get; set; } = string.Empty;
        public string Manifest { get; set; } = string.Empty;
        public string ScenarioHash { get; set; } = string.Empty;
        public string StageFixtureHash { get; set; } = string.Empty;
        public string ButtonMaskHash { get; set; } = string.Empty;
        public string BootstrapRngHash { get; set; } = string.Empty;
    }

    private sealed record ValidatedTick(
        SortedDictionary<string, string> Hashes,
        Dictionary<string, JsonNode?> CanonicalDomains,
        SortedDictionary<string, string> ComparisonHashes,
        Dictionary<string, JsonNode?> ComparisonDomains,
        JsonNode? OpenedSlotBodies);

    private sealed record SlotValidation(JsonNode CommitmentDomain, JsonNode OpenedBodies);

    private sealed class TraceCompareReport
    {
        public string Schema { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool CertificateEligible { get; set; }
        public string Authority { get; set; } = string.Empty;
        public string Unity { get; set; } = string.Empty;
        public string ComparisonProfile { get; set; } = string.Empty;
        public string CertificateClass { get; set; } = "none";
        public bool DiagnosticComparison { get; set; }
        public string? AuthorityManifestSha256 { get; set; }
        public string? UnityManifestSha256 { get; set; }
        public string? AuthorityDetail { get; set; }
        public string? UnityDetail { get; set; }
        public string? AuthorityDataFixture { get; set; }
        public string? UnityDataFixture { get; set; }
        public int ExpectedTicks { get; set; }
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

internal sealed record TraceCompareTestResult(string Status, string? Reason, bool CertificateEligible);
