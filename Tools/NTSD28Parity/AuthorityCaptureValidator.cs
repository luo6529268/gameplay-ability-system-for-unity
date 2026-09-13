using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class AuthorityCaptureValidator
{
    internal const string CaptureSchema = "ntsd28-authority-source-capture-v2";
    internal const string ReportSchema =
        "ntsd28-authority-source-capture-validation-v2";
    internal const string EvidenceClass = "SOURCE_MODEL_DIAGNOSTIC_ONLY";
    internal const string LegacyScenarioReferenceSha256 =
        "5EDA51440039099069D041E5BD13FDE8BE9FD8C7B20983985B1712A59719D86B";
    internal const int AuthoritySlotCapacity = 1000;

    private static readonly string[] HeaderProperties =
    [
        "kind", "schema", "certificateEligible", "evidenceClass",
        "formalExeSha256", "authoritySourceManifestSha256",
        "captureRunnerSourceSha256", "captureBinarySha256",
        "scenarioReferenceExeSha256",
        "scenarioDataSha256", "scenarioId", "firstCompletedTick",
        "expectedTickCount", "slotCapacity", "content",
    ];

    private static readonly string[] TickProperties =
    [
        "kind", "completedTick", "entities",
    ];

    internal static AuthorityCaptureValidationReport ValidateFile(string path)
    {
        using var reader = new StreamReader(
            path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return Validate(reader, Path.GetFileName(path));
    }

    internal static AuthorityCaptureValidationReport ValidateTextForTest(
        string capture)
    {
        using var reader = new StringReader(capture);
        return Validate(reader, "synthetic-authority-capture");
    }

    private static AuthorityCaptureValidationReport Validate(
        TextReader reader,
        string source)
    {
        var report = new AuthorityCaptureValidationReport
        {
            Schema = ReportSchema,
            Source = source,
            EvidenceClass = EvidenceClass,
            CertificateEligible = false,
        };

        try
        {
            JsonObject header = ParseObject(
                ReadRequiredLine(reader, "missing-header"), "header");
            if (!TraceContract.HasExactProperties(header, HeaderProperties))
                throw new InvalidDataException("header-property-set-mismatch");

            RequireString(header, "kind", "header");
            RequireString(header, "schema", CaptureSchema);
            report.ContentIdentityKey = TraceContentIdentity.Validate(
                header["content"] as JsonObject ?? throw new InvalidDataException("content-object-required"), true);
            if (RequireBoolean(header, "certificateEligible"))
            {
                throw new InvalidDataException(
                    "source-model-capture-cannot-be-certificate-eligible");
            }
            RequireString(header, "evidenceClass", EvidenceClass);
            RequireSha256(
                header, "formalExeSha256",
                TraceContract.AuthorityExecutableSha256);
            report.AuthoritySourceManifestSha256 = RequireSha256(
                header, "authoritySourceManifestSha256");
            report.CaptureRunnerSourceSha256 = RequireSha256(
                header, "captureRunnerSourceSha256");
            report.CaptureBinarySha256 = RequireSha256(
                header, "captureBinarySha256");
            RequireSha256(
                header, "scenarioReferenceExeSha256",
                LegacyScenarioReferenceSha256);
            report.ScenarioDataSha256 = RequireSha256(
                header, "scenarioDataSha256");
            report.ScenarioId = RequireNonEmptyString(header, "scenarioId");
            long firstCompletedTick = RequirePositiveInt64(
                header, "firstCompletedTick");
            if (firstCompletedTick != 1)
            {
                throw new InvalidDataException(
                    "source-capture-first-completed-tick-must-be-one");
            }
            int expectedTickCount = RequirePositiveInt32(
                header, "expectedTickCount");
            int slotCapacity = RequirePositiveInt32(header, "slotCapacity");
            if (slotCapacity != AuthoritySlotCapacity)
            {
                throw new InvalidDataException(
                    "authority-source-slot-capacity-mismatch");
            }

            report.ExpectedTicks = expectedTickCount;
            for (int index = 0; index < expectedTickCount; index++)
            {
                long expectedTick = checked(firstCompletedTick + index);
                JsonObject tick = ParseObject(
                    ReadRequiredLine(
                        reader, $"missing-required-tick:{expectedTick}"),
                    $"tick:{expectedTick}");
                if (!TraceContract.HasExactProperties(tick, TickProperties))
                    throw new InvalidDataException("tick-property-set-mismatch");
                RequireString(tick, "kind", "tick");
                long completedTick = RequirePositiveInt64(
                    tick, "completedTick");
                if (completedTick != expectedTick)
                {
                    throw new InvalidDataException(
                        $"non-contiguous-completed-tick: expected {expectedTick}, got {completedTick}");
                }
                if (tick["entities"] is not JsonArray entities)
                    throw new InvalidDataException("entities-must-be-array");
                TraceComparator.ValidateEntityArrayForContract(
                    entities, slotCapacity);
                report.ValidatedTicks++;
                report.EntitySnapshots += entities.Count;
            }

            if (ReadNextLine(reader) is not null)
                throw new InvalidDataException("extra-tick-after-declared-range");

            report.Valid = true;
            report.Status = "valid-source-model-capture";
            return report;
        }
        catch (Exception exception) when (TraceComparator.IsContractFailure(exception))
        {
            report.Valid = false;
            report.Status = "invalid";
            report.Reason = exception.Message;
            return report;
        }
    }

    private static string ReadRequiredLine(TextReader reader, string reason) =>
        ReadNextLine(reader) ?? throw new InvalidDataException(reason);

    private static string? ReadNextLine(TextReader reader)
    {
        string? line;
        do
        {
            line = reader.ReadLine();
        }
        while (line is not null && string.IsNullOrWhiteSpace(line));
        return line;
    }

    private static JsonObject ParseObject(string text, string context)
    {
        JsonNode? node = JsonNode.Parse(text);
        return node as JsonObject ??
               throw new InvalidDataException($"{context}-must-be-object");
    }

    private static string RequireString(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out string? result) || result is null)
            throw new InvalidDataException($"{property}-must-be-string");
        return result;
    }

    private static string RequireNonEmptyString(
        JsonObject owner, string property)
    {
        string result = RequireString(owner, property);
        if (string.IsNullOrWhiteSpace(result))
            throw new InvalidDataException($"{property}-must-be-nonempty");
        return result;
    }

    private static void RequireString(
        JsonObject owner, string property, string expected)
    {
        string actual = RequireString(owner, property);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"{property}-mismatch: expected {expected}, got {actual}");
        }
    }

    private static bool RequireBoolean(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out bool result))
            throw new InvalidDataException($"{property}-must-be-boolean");
        return result;
    }

    private static long RequirePositiveInt64(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out long result))
            throw new InvalidDataException($"{property}-must-be-int64");
        if (result <= 0)
            throw new InvalidDataException($"{property}-must-be-positive");
        return result;
    }

    private static int RequirePositiveInt32(JsonObject owner, string property)
    {
        long result = RequirePositiveInt64(owner, property);
        if (result > int.MaxValue)
            throw new InvalidDataException($"{property}-must-fit-int32");
        return (int)result;
    }

    private static string RequireSha256(JsonObject owner, string property)
    {
        string result = RequireString(owner, property);
        if (!TraceContract.IsSha256(result))
            throw new InvalidDataException($"{property}-must-be-sha256");
        return result.ToUpperInvariant();
    }

    private static void RequireSha256(
        JsonObject owner, string property, string expected)
    {
        string actual = RequireSha256(owner, property);
        if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"{property}-mismatch");
    }
}

internal sealed class AuthorityCaptureValidationReport
{
    public string Schema { get; set; } = string.Empty;
    public string ContentIdentityKey { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Valid { get; set; }
    public bool CertificateEligible { get; set; }
    public string EvidenceClass { get; set; } = string.Empty;
    public string ScenarioId { get; set; } = string.Empty;
    public string AuthoritySourceManifestSha256 { get; set; } = string.Empty;
    public string CaptureRunnerSourceSha256 { get; set; } = string.Empty;
    public string CaptureBinarySha256 { get; set; } = string.Empty;
    public string ScenarioDataSha256 { get; set; } = string.Empty;
    public int ExpectedTicks { get; set; }
    public int ValidatedTicks { get; set; }
    public int EntitySnapshots { get; set; }
    public string? Reason { get; set; }
}
