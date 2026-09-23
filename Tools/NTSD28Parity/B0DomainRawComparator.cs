using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class B0DomainRawComparator
{
    internal const string ComparisonSchema =
        "ntsd28-logan-b0-domain-raw-comparison-v1";

    internal static B0DomainRawComparisonReport CompareFiles(
        string authorityPath,
        string unityPath)
    {
        B0DomainRawValidationReport authorityValidation =
            B0DomainRawContract.ValidateFile(authorityPath);
        B0DomainRawValidationReport unityValidation =
            B0DomainRawContract.ValidateFile(unityPath);
        if (!authorityValidation.Valid)
        {
            throw new InvalidDataException(
                $"authority-capture-invalid:{authorityValidation.Reason}");
        }
        if (!unityValidation.Valid)
        {
            throw new InvalidDataException(
                $"unity-capture-invalid:{unityValidation.Reason}");
        }

        return CompareCaptures(
            LoadCapture(authorityPath),
            LoadCapture(unityPath));
    }

    internal static B0DomainRawComparisonReport CompareTextForTest(
        string authorityText,
        string unityText)
    {
        B0DomainRawValidationReport authorityValidation =
            B0DomainRawContract.ValidateTextForTest(authorityText);
        B0DomainRawValidationReport unityValidation =
            B0DomainRawContract.ValidateTextForTest(unityText);
        if (!authorityValidation.Valid || !unityValidation.Valid)
            throw new InvalidDataException("capture-invalid-before-compare");
        return CompareCaptures(
            LoadCaptureFromLines(SplitLines(authorityText)),
            LoadCaptureFromLines(SplitLines(unityText)));
    }

    private static B0DomainRawComparisonReport CompareCaptures(
        Capture authority,
        Capture unity)
    {
        RequireProducer(
            authority.Header,
            B0DomainRawContract.AuthorityProducer,
            "authority");
        RequireProducer(
            unity.Header,
            B0DomainRawContract.UnityProducer,
            "unity");
        if (!string.Equals(
                RequireString(authority.Header, "schema"),
                RequireString(unity.Header, "schema"),
                StringComparison.Ordinal))
        {
            throw new InvalidDataException("raw-schema-mismatch");
        }
        string authorityScenario = RequireString(authority.Header, "scenarioId");
        string unityScenario = RequireString(unity.Header, "scenarioId");
        if (!string.Equals(
                authorityScenario,
                unityScenario,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException("scenario-id-mismatch");
        }
        if (authority.Ticks.Count != unity.Ticks.Count)
            throw new InvalidDataException("tick-count-mismatch");

        int authorityCapacity = RequireInt32(authority.Header, "slotCapacity");
        int unityCapacity = RequireInt32(unity.Header, "slotCapacity");
        var report = new B0DomainRawComparisonReport
        {
            Schema = ComparisonSchema,
            ScenarioId = authorityScenario,
            TicksCompared = authority.Ticks.Count,
            AuthoritySlotCapacity = authorityCapacity,
            UnitySlotCapacity = unityCapacity,
            SlotCapacityExceptionApplied = authorityCapacity != unityCapacity,
            CertificateEligible = false,
            AuthorityRng = BuildRngSummaries(authority),
            UnityRng = BuildRngSummaries(unity),
        };

        CompareTickDomain(
            authority,
            unity,
            "input",
            "input",
            "VALUE_DIFFERENCE",
            report,
            difference => report.InputEqual = !difference);

        string[] authorityStreams = AvailableStreams(authority.Header);
        string[] unityStreams = AvailableStreams(unity.Header);
        report.RngTopologyEqual = authorityStreams.SequenceEqual(
            unityStreams,
            StringComparer.Ordinal);
        if (!report.RngTopologyEqual)
        {
            report.Differences.Add(new B0DomainDifference
            {
                Domain = "rng",
                Path = "rng.streamAvailability",
                Classification = "STREAM_TOPOLOGY_DIFFERENCE",
                AuthorityValue = string.Join(",", authorityStreams),
                UnityValue = string.Join(",", unityStreams),
            });
        }

        CompareTickProjection(
            authority,
            unity,
            tick => tick["slots"]!["occupants"]!,
            "slots",
            "slots.occupants",
            "VALUE_DIFFERENCE",
            report,
            difference => report.SlotOccupantsEqual = !difference);
        CompareTickProjection(
            authority,
            unity,
            tick => tick["lifecycleDelta"]!,
            "lifecycle",
            "lifecycleDelta",
            "VALUE_DIFFERENCE",
            report,
            difference => report.LifecycleEqual = !difference);

        report.FirstDifference = report.Differences.FirstOrDefault();
        report.Status = report.Differences.Count == 0
            ? "equal-shared-domains"
            : report.InputEqual && report.SlotOccupantsEqual &&
              report.LifecycleEqual && !report.RngTopologyEqual
                ? "shared-domains-equal-rng-topology-different"
                : "different";
        return report;
    }

    private static void CompareTickDomain(
        Capture authority,
        Capture unity,
        string property,
        string path,
        string classification,
        B0DomainRawComparisonReport report,
        Action<bool> setDifference)
    {
        CompareTickProjection(
            authority,
            unity,
            tick => tick[property]!,
            property,
            path,
            classification,
            report,
            setDifference);
    }

    private static void CompareTickProjection(
        Capture authority,
        Capture unity,
        Func<JsonObject, JsonNode> projection,
        string domain,
        string path,
        string classification,
        B0DomainRawComparisonReport report,
        Action<bool> setDifference)
    {
        for (int index = 0; index < authority.Ticks.Count; index++)
        {
            JsonNode authorityNode = projection(authority.Ticks[index]);
            JsonNode unityNode = projection(unity.Ticks[index]);
            if (string.Equals(
                    TraceContract.HashNode(authorityNode),
                    TraceContract.HashNode(unityNode),
                    StringComparison.Ordinal))
            {
                continue;
            }

            setDifference(true);
            report.Differences.Add(new B0DomainDifference
            {
                Domain = domain,
                Path = path,
                Classification = classification,
                CompletedTick = RequireInt64(
                    authority.Ticks[index],
                    "completedTick"),
                AuthorityValue = TraceContract.Canonicalize(authorityNode)
                    .ToJsonString(),
                UnityValue = TraceContract.Canonicalize(unityNode)
                    .ToJsonString(),
            });
            return;
        }

        setDifference(false);
    }

    private static List<B0DomainRngStreamSummary> BuildRngSummaries(
        Capture capture)
    {
        JsonObject initial = capture.Header["initialRngTotalCalls"]!.AsObject();
        string[] streams = AvailableStreams(capture.Header);
        var result = new List<B0DomainRngStreamSummary>(streams.Length);
        foreach (string stream in streams)
        {
            result.Add(new B0DomainRngStreamSummary
            {
                Stream = stream,
                InitialTotalCalls = RequireUInt64(initial, stream),
                TickCallCounts = capture.Ticks
                    .Select(tick => RequireUInt64(
                        tick["rng"]![stream]!.AsObject(),
                        "tickCallCount"))
                    .ToArray(),
                FinalTotalCalls = RequireUInt64(
                    capture.Ticks[^1]["rng"]![stream]!.AsObject(),
                    "totalCalls"),
            });
        }

        return result;
    }

    private static string[] AvailableStreams(JsonObject header)
    {
        JsonObject availability = header["streamAvailability"]!.AsObject();
        return B0DomainRawContract.StreamNames
            .Where(stream => string.Equals(
                RequireString(availability, stream),
                B0DomainRawContract.Available,
                StringComparison.Ordinal))
            .ToArray();
    }

    private static Capture LoadCapture(string path)
    {
        return LoadCaptureFromLines(File.ReadLines(path));
    }

    private static Capture LoadCaptureFromLines(IEnumerable<string> lines)
    {
        using IEnumerator<string> enumerator = lines.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidDataException("empty-capture");
        JsonObject header = JsonNode.Parse(enumerator.Current)!.AsObject();
        var ticks = new List<JsonObject>();
        while (enumerator.MoveNext())
            ticks.Add(JsonNode.Parse(enumerator.Current)!.AsObject());
        return new Capture(header, ticks);
    }

    private static string[] SplitLines(string text)
    {
        return text.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static void RequireProducer(
        JsonObject header,
        string expected,
        string role)
    {
        if (!string.Equals(
                RequireString(header, "producer"),
                expected,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException($"{role}-producer-mismatch");
        }
    }

    private static string RequireString(JsonObject value, string property)
    {
        return value[property]?.GetValue<string>() ??
            throw new InvalidDataException($"missing-string:{property}");
    }

    private static int RequireInt32(JsonObject value, string property)
    {
        return value[property]?.GetValue<int>() ??
            throw new InvalidDataException($"missing-int32:{property}");
    }

    private static long RequireInt64(JsonObject value, string property)
    {
        return value[property]?.GetValue<long>() ??
            throw new InvalidDataException($"missing-int64:{property}");
    }

    private static ulong RequireUInt64(JsonObject value, string property)
    {
        return value[property]?.GetValue<ulong>() ??
            throw new InvalidDataException($"missing-uint64:{property}");
    }

    private sealed record Capture(
        JsonObject Header,
        List<JsonObject> Ticks);
}

internal sealed class B0DomainRawComparisonReport
{
    public string Schema { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ScenarioId { get; set; } = string.Empty;
    public int TicksCompared { get; set; }
    public bool InputEqual { get; set; }
    public bool RngTopologyEqual { get; set; }
    public bool SlotOccupantsEqual { get; set; }
    public bool LifecycleEqual { get; set; }
    public int AuthoritySlotCapacity { get; set; }
    public int UnitySlotCapacity { get; set; }
    public bool SlotCapacityExceptionApplied { get; set; }
    public List<B0DomainRngStreamSummary> AuthorityRng { get; set; } = [];
    public List<B0DomainRngStreamSummary> UnityRng { get; set; } = [];
    public List<B0DomainDifference> Differences { get; set; } = [];
    public B0DomainDifference? FirstDifference { get; set; }
    public bool CertificateEligible { get; set; }
}

internal sealed class B0DomainRngStreamSummary
{
    public string Stream { get; set; } = string.Empty;
    public ulong InitialTotalCalls { get; set; }
    public ulong FinalTotalCalls { get; set; }
    public ulong[] TickCallCounts { get; set; } = [];
}

internal sealed class B0DomainDifference
{
    public string Domain { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public long? CompletedTick { get; set; }
    public string AuthorityValue { get; set; } = string.Empty;
    public string UnityValue { get; set; } = string.Empty;
}
