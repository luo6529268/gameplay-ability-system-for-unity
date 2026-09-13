using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class TraceComparator
{
    internal const string EqualStructureStatus = "equal-structure";
    internal const string ContentStrategyPendingStatus = "content-strategy-pending";

    internal static TraceValidationReport ValidateFile(string tracePath)
    {
        using var reader = new StreamReader(
            tracePath,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);
        return Validate(reader, Path.GetFileName(tracePath));
    }

    internal static TraceComparisonReport CompareFiles(
        string authorityPath,
        string unityPath)
    {
        using var authority = new StreamReader(
            authorityPath,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);
        using var unity = new StreamReader(
            unityPath,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);
        return Compare(
            authority,
            unity,
            Path.GetFileName(authorityPath),
            Path.GetFileName(unityPath));
    }

    internal static TraceValidationReport ValidateTextForTest(string trace)
    {
        using var reader = new StringReader(trace);
        return Validate(reader, "synthetic");
    }

    internal static TraceComparisonReport CompareTextForTest(
        string authority,
        string unity)
    {
        using var authorityReader = new StringReader(authority);
        using var unityReader = new StringReader(unity);
        return Compare(authorityReader, unityReader, "authority", "unity");
    }

    private static TraceValidationReport Validate(TextReader reader, string source)
    {
        var report = new TraceValidationReport
        {
            Schema = TraceContract.ValidationSchema,
            Source = source,
            CertificateEligible = false,
        };

        try
        {
            string headerLine = ReadRequiredLine(reader, "missing-header");
            HeaderInfo header = ValidateHeader(headerLine, expectedProducer: null);
            report.Producer = header.Producer;
            report.ExpectedTicks = header.ExpectedTickCount;
            report.ContentManifestSha256 = header.ContentManifestSha256;

            for (int index = 0; index < header.ExpectedTickCount; index++)
            {
                long expectedTick = checked(header.FirstCompletedTick + index);
                string tickLine = ReadRequiredLine(
                    reader,
                    $"missing-required-tick:{expectedTick}");
                _ = ValidateTick(tickLine, expectedTick, header.SlotCapacity);
                report.ValidatedTicks++;
            }

            if (ReadNextLine(reader) is not null)
            {
                throw new TraceContractException("extra-tick-after-declared-range");
            }

            report.Valid = true;
            report.Status = "valid-structure";
            return report;
        }
        catch (Exception exception) when (
            exception is TraceContractException or
            JsonException or
            FormatException or
            OverflowException)
        {
            report.Valid = false;
            report.Status = "invalid";
            report.Reason = exception.Message;
            return report;
        }
    }

    private static TraceComparisonReport Compare(
        TextReader authorityReader,
        TextReader unityReader,
        string authorityName,
        string unityName)
    {
        var report = new TraceComparisonReport
        {
            Schema = TraceContract.ComparisonSchema,
            Authority = authorityName,
            Unity = unityName,
            CertificateEligible = false,
        };

        HeaderInfo authorityHeader;
        HeaderInfo unityHeader;
        try
        {
            authorityHeader = ValidateHeader(
                ReadRequiredLine(authorityReader, "missing-authority-header"),
                "authority");
        }
        catch (Exception exception) when (IsContractFailure(exception))
        {
            return Fail(report, "header", 0, "invalid-authority: " + exception.Message);
        }

        try
        {
            unityHeader = ValidateHeader(
                ReadRequiredLine(unityReader, "missing-unity-header"),
                "unity");
        }
        catch (Exception exception) when (IsContractFailure(exception))
        {
            return Fail(report, "header", 0, "invalid-unity: " + exception.Message);
        }

        string? headerDifference = CompareHeaders(authorityHeader, unityHeader);
        if (headerDifference is not null)
        {
            return Fail(report, "header", 0, headerDifference);
        }

        report.ExpectedTicks = authorityHeader.ExpectedTickCount;
        report.AuthoritySlotCapacity = authorityHeader.SlotCapacity;
        report.UnitySlotCapacity = unityHeader.SlotCapacity;
        report.AuthorityContentManifestSha256 =
            authorityHeader.ContentManifestSha256;
        report.UnityContentManifestSha256 = unityHeader.ContentManifestSha256;
        bool contentMismatch = !string.Equals(
            authorityHeader.ContentManifestSha256,
            unityHeader.ContentManifestSha256,
            StringComparison.OrdinalIgnoreCase);

        for (int index = 0; index < authorityHeader.ExpectedTickCount; index++)
        {
            long expectedTick = checked(authorityHeader.FirstCompletedTick + index);
            ValidatedTick authorityTick;
            ValidatedTick unityTick;
            try
            {
                authorityTick = ValidateTick(
                    ReadRequiredLine(
                        authorityReader,
                        $"missing-authority-tick:{expectedTick}"),
                    expectedTick,
                    authorityHeader.SlotCapacity);
            }
            catch (Exception exception) when (IsContractFailure(exception))
            {
                return Fail(
                    report,
                    "authority",
                    expectedTick,
                    "invalid-authority-tick: " + exception.Message);
            }

            try
            {
                unityTick = ValidateTick(
                    ReadRequiredLine(
                        unityReader,
                        $"missing-unity-tick:{expectedTick}"),
                    expectedTick,
                    unityHeader.SlotCapacity);
            }
            catch (Exception exception) when (IsContractFailure(exception))
            {
                return Fail(
                    report,
                    "unity",
                    expectedTick,
                    "invalid-unity-tick: " + exception.Message);
            }

            foreach (string domain in TraceContract.DomainOrder)
            {
                if (!string.Equals(
                        authorityTick.Hashes[domain],
                        unityTick.Hashes[domain],
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Fail(
                        report,
                        domain,
                        expectedTick,
                        "domain-hash-mismatch",
                        authorityTick.Hashes[domain],
                        unityTick.Hashes[domain]);
                }

                if (!JsonNode.DeepEquals(
                        authorityTick.Domains[domain],
                        unityTick.Domains[domain]))
                {
                    return Fail(
                        report,
                        domain,
                        expectedTick,
                        "domain-body-mismatch");
                }
            }

            if (!string.Equals(
                    authorityTick.Hashes["overall"],
                    unityTick.Hashes["overall"],
                    StringComparison.OrdinalIgnoreCase))
            {
                return Fail(
                    report,
                    "overall",
                    expectedTick,
                    "overall-hash-mismatch",
                    authorityTick.Hashes["overall"],
                    unityTick.Hashes["overall"]);
            }

            report.TicksCompared++;
        }

        if (ReadNextLine(authorityReader) is not null)
        {
            return Fail(
                report,
                "stream",
                checked(authorityHeader.FirstCompletedTick + authorityHeader.ExpectedTickCount),
                "extra-authority-tick-after-declared-range");
        }

        if (ReadNextLine(unityReader) is not null)
        {
            return Fail(
                report,
                "stream",
                checked(unityHeader.FirstCompletedTick + unityHeader.ExpectedTickCount),
                "extra-unity-tick-after-declared-range");
        }

        report.Status = contentMismatch
            ? ContentStrategyPendingStatus
            : EqualStructureStatus;
        report.ContentStrategyPending = contentMismatch;
        return report;
    }

    private static HeaderInfo ValidateHeader(
        string line,
        string? expectedProducer)
    {
        JsonObject header = ParseObject(line, "header");
        if (!TraceContract.HasExactProperties(
                header,
                TraceContract.HeaderProperties))
        {
            throw new TraceContractException("header-property-set-mismatch");
        }

        RequireString(header, "kind", "header");
        RequireString(header, "schema", TraceContract.Schema);
        string producer = RequireString(header, "producer");
        if (producer is not ("authority" or "unity"))
        {
            throw new TraceContractException("invalid-producer");
        }

        if (expectedProducer is not null &&
            !string.Equals(producer, expectedProducer, StringComparison.Ordinal))
        {
            throw new TraceContractException(
                $"producer-mismatch: expected {expectedProducer}, got {producer}");
        }

        JsonObject authority = RequireObject(header, "authority");
        RequireExactProperties(authority, "authority", "exeSha256");
        string executableSha = RequireSha256(authority, "exeSha256");
        if (!string.Equals(
                executableSha,
                TraceContract.AuthorityExecutableSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new TraceContractException("authority-executable-sha256-mismatch");
        }

        JsonObject scenario = RequireObject(header, "scenario");
        RequireExactProperties(
            scenario,
            "scenario",
            "id",
            "version",
            "firstCompletedTick",
            "expectedTickCount");
        string scenarioId = RequireNonEmptyString(scenario, "id");
        int scenarioVersion = RequirePositiveInt32(scenario, "version");
        long firstCompletedTick = RequireNonNegativeInt64(
            scenario,
            "firstCompletedTick");
        int expectedTickCount = RequirePositiveInt32(
            scenario,
            "expectedTickCount");

        JsonObject timing = RequireObject(header, "timing");
        RequireExactProperties(
            timing,
            "timing",
            "normalIntervalMs",
            "fastIntervalMs");
        if (RequireInt32(timing, "normalIntervalMs") !=
                TraceContract.NormalLogicIntervalMilliseconds ||
            RequireInt32(timing, "fastIntervalMs") !=
                TraceContract.FastLogicIntervalMilliseconds)
        {
            throw new TraceContractException("timing-contract-mismatch");
        }

        int slotCapacity = RequirePositiveInt32(header, "slotCapacity");

        JsonObject content = RequireObject(header, "content");
        RequireExactProperties(content, "content", "policy", "manifestSha256");
        RequireString(content, "policy", TraceContract.ContentPolicy);
        string contentManifest = RequireSha256(content, "manifestSha256");

        JsonArray approvedExceptions = RequireArray(
            header,
            "approvedExceptions");
        if (!TraceContract.SequenceEqual(
                approvedExceptions,
                TraceContract.ApprovedExceptions))
        {
            throw new TraceContractException("approved-exceptions-mismatch");
        }

        JsonArray excludedFeatures = RequireArray(header, "excludedFeatures");
        if (!TraceContract.SequenceEqual(
                excludedFeatures,
                TraceContract.ExcludedFeatures))
        {
            throw new TraceContractException("excluded-features-mismatch");
        }

        JsonArray domainOrder = RequireArray(header, "domainOrder");
        if (!TraceContract.SequenceEqual(domainOrder, TraceContract.DomainOrder))
        {
            throw new TraceContractException("domain-order-mismatch");
        }

        return new HeaderInfo(
            producer,
            scenarioId,
            scenarioVersion,
            firstCompletedTick,
            expectedTickCount,
            slotCapacity,
            contentManifest);
    }

    private static string? CompareHeaders(HeaderInfo authority, HeaderInfo unity)
    {
        if (!string.Equals(
                authority.ScenarioId,
                unity.ScenarioId,
                StringComparison.Ordinal))
        {
            return "scenario-id-mismatch";
        }

        if (authority.ScenarioVersion != unity.ScenarioVersion)
        {
            return "scenario-version-mismatch";
        }

        if (authority.FirstCompletedTick != unity.FirstCompletedTick)
        {
            return "first-completed-tick-mismatch";
        }

        if (authority.ExpectedTickCount != unity.ExpectedTickCount)
        {
            return "expected-tick-count-mismatch";
        }

        return null;
    }

    private static ValidatedTick ValidateTick(
        string line,
        long expectedTick,
        int slotCapacity)
    {
        JsonObject tick = ParseObject(line, $"tick:{expectedTick}");
        if (!TraceContract.HasExactProperties(tick, TraceContract.TickProperties))
        {
            throw new TraceContractException("tick-property-set-mismatch");
        }

        RequireString(tick, "kind", "tick");
        long completedTick = RequireNonNegativeInt64(tick, "completedTick");
        if (completedTick != expectedTick)
        {
            throw new TraceContractException(
                $"non-contiguous-completed-tick: expected {expectedTick}, got {completedTick}");
        }

        JsonObject input = RequireObject(tick, "input");
        JsonObject rng = RequireObject(tick, "rng");
        JsonObject world = RequireObject(tick, "world");
        JsonArray entities = RequireArray(tick, "entities");
        JsonArray relations = RequireArray(tick, "relations");
        JsonObject rests = RequireObject(tick, "rests");
        JsonArray events = RequireArray(tick, "events");
        JsonArray presentation = RequireArray(tick, "presentation");
        _ = input;
        _ = world;
        _ = relations;
        _ = rests;
        _ = events;
        _ = presentation;

        ValidateRng(rng);
        ValidateEntityArrayForContract(entities, slotCapacity);

        JsonObject hashes = RequireObject(tick, "hashes");
        string[] expectedHashProperties =
            TraceContract.DomainOrder.Append("overall").ToArray();
        if (!TraceContract.HasExactProperties(hashes, expectedHashProperties))
        {
            throw new TraceContractException("hash-property-set-mismatch");
        }

        var committedHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        var domains = new Dictionary<string, JsonNode>(StringComparer.Ordinal);
        foreach (string domain in TraceContract.DomainOrder)
        {
            JsonNode body = tick[domain] ??
                throw new TraceContractException($"missing-domain:{domain}");
            string committedHash = RequireSha256(hashes, domain);
            string actualHash = TraceContract.HashNode(body);
            if (!string.Equals(
                    committedHash,
                    actualHash,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new TraceContractException($"forged-domain-hash:{domain}");
            }

            committedHashes[domain] = committedHash.ToUpperInvariant();
            domains[domain] = body;
        }

        string overall = RequireSha256(hashes, "overall");
        string actualOverall = TraceContract.ComputeOverallHash(hashes);
        if (!string.Equals(overall, actualOverall, StringComparison.OrdinalIgnoreCase))
        {
            throw new TraceContractException("forged-overall-hash");
        }

        committedHashes["overall"] = overall.ToUpperInvariant();
        return new ValidatedTick(completedTick, committedHashes, domains);
    }

    private static void ValidateRng(JsonObject rng)
    {
        RequireExactProperties(rng, "rng", "crt", "synchronized");
        JsonObject crt = RequireObject(rng, "crt");
        RequireExactProperties(crt, "rng.crt", "state", "totalCalls", "tickCalls");
        long crtState = RequireNonNegativeInt64(crt, "state");
        if (crtState > uint.MaxValue)
        {
            throw new TraceContractException("rng.crt.state-out-of-range");
        }

        long crtTotalCalls = RequireNonNegativeInt64(crt, "totalCalls");
        JsonArray crtCalls = RequireArray(crt, "tickCalls");
        ValidateCrtCalls(crtCalls);
        if (crtTotalCalls < crtCalls.Count)
        {
            throw new TraceContractException("rng.crt.totalCalls-less-than-tickCalls");
        }

        JsonObject synchronized = RequireObject(rng, "synchronized");
        RequireExactProperties(
            synchronized,
            "rng.synchronized",
            "tableSha256",
            "totalCalls",
            "tickCalls");
        _ = RequireSha256(synchronized, "tableSha256");
        long synchronizedTotalCalls = RequireNonNegativeInt64(
            synchronized,
            "totalCalls");
        JsonArray synchronizedCalls = RequireArray(synchronized, "tickCalls");
        ValidateSynchronizedCalls(synchronizedCalls);
        if (synchronizedTotalCalls < synchronizedCalls.Count)
        {
            throw new TraceContractException(
                "rng.synchronized.totalCalls-less-than-tickCalls");
        }
    }

    private static void ValidateCrtCalls(JsonArray calls)
    {
        for (int index = 0; index < calls.Count; index++)
        {
            JsonObject call = RequireArrayObject(calls, index, "rng.crt.tickCalls");
            RequireExactProperties(
                call,
                "rng.crt.tickCall",
                "ordinal",
                "bound",
                "raw",
                "value");
            RequireOrdinal(call, index);
            int bound = RequirePositiveInt32(call, "bound");
            int raw = RequireInt32(call, "raw");
            int value = RequireInt32(call, "value");
            if (raw < 0 || raw > 0x7fff)
            {
                throw new TraceContractException("rng.crt.raw-out-of-range");
            }

            if (value < 0 || value >= bound)
            {
                throw new TraceContractException("rng.crt.value-out-of-range");
            }
        }
    }

    private static void ValidateSynchronizedCalls(JsonArray calls)
    {
        for (int index = 0; index < calls.Count; index++)
        {
            JsonObject call = RequireArrayObject(
                calls,
                index,
                "rng.synchronized.tickCalls");
            RequireExactProperties(
                call,
                "rng.synchronized.tickCall",
                "ordinal",
                "callSite",
                "bound",
                "value");
            RequireOrdinal(call, index);
            int callSite = RequireInt32(call, "callSite");
            int bound = RequirePositiveInt32(call, "bound");
            int value = RequireInt32(call, "value");
            if (callSite < 0 || callSite >= 3000)
            {
                throw new TraceContractException(
                    "rng.synchronized.callSite-out-of-range");
            }

            if (value < 0 || value >= bound)
            {
                throw new TraceContractException(
                    "rng.synchronized.value-out-of-range");
            }
        }
    }

    internal static void ValidateEntityArrayForContract(
        JsonArray entities,
        int slotCapacity)
    {
        int previousSlot = -1;
        for (int index = 0; index < entities.Count; index++)
        {
            JsonObject entity = RequireArrayObject(entities, index, "entities");
            if (!TraceContract.HasExactProperties(
                    entity,
                    EntityFieldContract.RootProperties))
            {
                throw new TraceContractException(
                    "entity-root-property-set-mismatch");
            }

            int slot = RequireNonNegativeInt32(entity, "slot");
            if (slot >= slotCapacity)
            {
                throw new TraceContractException(
                    "entity-slot-outside-producer-capacity");
            }

            if (slot <= previousSlot)
            {
                throw new TraceContractException(
                    "entities-not-strictly-ascending-by-slot");
            }

            previousSlot = slot;
            _ = RequirePositiveInt64(entity, "allocationEpoch");
            if (!RequireBoolean(entity, "active"))
            {
                throw new TraceContractException("inactive-entity-in-active-array");
            }

            foreach ((string groupName, string[] groupProperties) in
                     EntityFieldContract.GroupProperties)
            {
                JsonObject group = RequireObject(entity, groupName);
                if (!TraceContract.HasExactProperties(group, groupProperties))
                {
                    throw new TraceContractException(
                        $"entity-{groupName}-property-set-mismatch");
                }

                foreach (string property in groupProperties)
                {
                    string path = $"{groupName}.{property}";
                    ValidateEntityFieldType(
                        group,
                        property,
                        EntityFieldContract.FieldAtPath(path));
                }
            }
        }
    }

    private static void ValidateEntityFieldType(
        JsonObject group,
        string property,
        EntityFieldDescriptor descriptor)
    {
        switch (descriptor.JsonType)
        {
            case "int32":
                _ = RequireInt32(group, property);
                return;
            case "int64":
                _ = RequireInt64(group, property);
                return;
            case "boolean":
                _ = RequireBoolean(group, property);
                return;
            case "float64":
                double value = RequireDouble(group, property);
                if (!double.IsFinite(value))
                {
                    throw new TraceContractException(
                        $"entity-field-not-finite:{descriptor.Path}");
                }

                return;
            default:
                throw new TraceContractException(
                    $"unsupported-entity-field-type:{descriptor.JsonType}");
        }
    }

    private static void RequireOrdinal(JsonObject call, int expected)
    {
        int ordinal = RequireNonNegativeInt32(call, "ordinal");
        if (ordinal != expected)
        {
            throw new TraceContractException(
                $"rng-call-ordinal-mismatch: expected {expected}, got {ordinal}");
        }
    }

    private static TraceComparisonReport Fail(
        TraceComparisonReport report,
        string domain,
        long completedTick,
        string reason,
        string? authorityValue = null,
        string? unityValue = null)
    {
        report.Status = "different";
        report.CertificateEligible = false;
        report.FirstDifference = new TraceDifference
        {
            Domain = domain,
            CompletedTick = completedTick,
            Reason = reason,
            AuthorityValue = authorityValue,
            UnityValue = unityValue,
        };
        return report;
    }

    private static bool IsContractFailure(Exception exception)
    {
        return exception is TraceContractException or
            JsonException or
            FormatException or
            OverflowException;
    }

    private static string ReadRequiredLine(TextReader reader, string reason)
    {
        return ReadNextLine(reader) ?? throw new TraceContractException(reason);
    }

    private static string? ReadNextLine(TextReader reader)
    {
        while (true)
        {
            string? line = reader.ReadLine();
            if (line is null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                return line;
            }
        }
    }

    private static JsonObject ParseObject(string line, string context)
    {
        JsonNode? node = JsonNode.Parse(line);
        return node as JsonObject ??
            throw new TraceContractException($"{context}-must-be-json-object");
    }

    private static JsonObject RequireObject(JsonObject owner, string property)
    {
        return owner[property] as JsonObject ??
            throw new TraceContractException($"{property}-must-be-object");
    }

    private static JsonArray RequireArray(JsonObject owner, string property)
    {
        return owner[property] as JsonArray ??
            throw new TraceContractException($"{property}-must-be-array");
    }

    private static JsonObject RequireArrayObject(
        JsonArray owner,
        int index,
        string context)
    {
        return owner[index] as JsonObject ??
            throw new TraceContractException($"{context}[{index}]-must-be-object");
    }

    private static void RequireExactProperties(
        JsonObject owner,
        string context,
        params string[] expected)
    {
        if (!TraceContract.HasExactProperties(owner, expected))
        {
            throw new TraceContractException($"{context}-property-set-mismatch");
        }
    }

    private static string RequireString(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out string? result) ||
            result is null)
        {
            throw new TraceContractException($"{property}-must-be-string");
        }

        return result;
    }

    private static string RequireNonEmptyString(JsonObject owner, string property)
    {
        string result = RequireString(owner, property);
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new TraceContractException($"{property}-must-not-be-empty");
        }

        return result;
    }

    private static void RequireString(
        JsonObject owner,
        string property,
        string expected)
    {
        string actual = RequireString(owner, property);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new TraceContractException(
                $"{property}-mismatch: expected {expected}, got {actual}");
        }
    }

    private static string RequireSha256(JsonObject owner, string property)
    {
        string value = RequireString(owner, property);
        if (!TraceContract.IsSha256(value))
        {
            throw new TraceContractException($"{property}-must-be-sha256");
        }

        return value.ToUpperInvariant();
    }

    private static bool RequireBoolean(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out bool result))
        {
            throw new TraceContractException($"{property}-must-be-boolean");
        }

        return result;
    }

    private static int RequireInt32(JsonObject owner, string property)
    {
        long value = RequireInt64(owner, property);
        if (value < int.MinValue || value > int.MaxValue)
        {
            throw new TraceContractException($"{property}-must-fit-int32");
        }

        return (int)value;
    }

    private static long RequireInt64(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out long result))
        {
            throw new TraceContractException($"{property}-must-be-int64");
        }

        return result;
    }

    private static double RequireDouble(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out double result))
        {
            throw new TraceContractException($"{property}-must-be-float64");
        }

        return result;
    }

    private static int RequirePositiveInt32(JsonObject owner, string property)
    {
        int value = RequireInt32(owner, property);
        if (value <= 0)
        {
            throw new TraceContractException($"{property}-must-be-positive");
        }

        return value;
    }

    private static int RequireNonNegativeInt32(JsonObject owner, string property)
    {
        int value = RequireInt32(owner, property);
        if (value < 0)
        {
            throw new TraceContractException($"{property}-must-be-nonnegative");
        }

        return value;
    }

    private static long RequirePositiveInt64(JsonObject owner, string property)
    {
        long value = RequireInt64(owner, property);
        if (value <= 0)
        {
            throw new TraceContractException($"{property}-must-be-positive");
        }

        return value;
    }

    private static long RequireNonNegativeInt64(JsonObject owner, string property)
    {
        long value = RequireInt64(owner, property);
        if (value < 0)
        {
            throw new TraceContractException($"{property}-must-be-nonnegative");
        }

        return value;
    }

    private sealed record HeaderInfo(
        string Producer,
        string ScenarioId,
        int ScenarioVersion,
        long FirstCompletedTick,
        int ExpectedTickCount,
        int SlotCapacity,
        string ContentManifestSha256);

    private sealed record ValidatedTick(
        long CompletedTick,
        IReadOnlyDictionary<string, string> Hashes,
        IReadOnlyDictionary<string, JsonNode> Domains);

    private sealed class TraceContractException : Exception
    {
        internal TraceContractException(string message)
            : base(message)
        {
        }
    }
}

internal sealed class TraceValidationReport
{
    public string Schema { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Valid { get; set; }
    public bool CertificateEligible { get; set; }
    public string Producer { get; set; } = string.Empty;
    public int ExpectedTicks { get; set; }
    public int ValidatedTicks { get; set; }
    public string ContentManifestSha256 { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

internal sealed class TraceComparisonReport
{
    public string Schema { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string Unity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool CertificateEligible { get; set; }
    public bool ContentStrategyPending { get; set; }
    public int ExpectedTicks { get; set; }
    public int TicksCompared { get; set; }
    public int AuthoritySlotCapacity { get; set; }
    public int UnitySlotCapacity { get; set; }
    public string AuthorityContentManifestSha256 { get; set; } = string.Empty;
    public string UnityContentManifestSha256 { get; set; } = string.Empty;
    public TraceDifference? FirstDifference { get; set; }
}

internal sealed class TraceDifference
{
    public string Domain { get; set; } = string.Empty;
    public long CompletedTick { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AuthorityValue { get; set; }
    public string? UnityValue { get; set; }
}
