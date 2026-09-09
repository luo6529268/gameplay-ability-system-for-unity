using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class B2InputRngJointRawComparator
{
    internal const string ComparisonSchema =
        "ntsd28-logan-b2-input-rng-joint-raw-comparison-v3";

    internal static B2InputRngJointRawComparisonReport CompareFiles(
        string authorityPath,
        string unityPath)
    {
        B2InputRngJointRawValidationReport authorityValidation =
            B2InputRngJointRawContract.ValidateFile(authorityPath);
        B2InputRngJointRawValidationReport unityValidation =
            B2InputRngJointRawContract.ValidateFile(unityPath);
        RequireValid(authorityValidation, "authority");
        RequireValid(unityValidation, "unity");
        return CompareCaptures(
            B2InputRngJointRawContract.LoadFile(authorityPath),
            B2InputRngJointRawContract.LoadFile(unityPath));
    }

    internal static B2InputRngJointRawComparisonReport CompareTextForTest(
        string authorityText,
        string unityText)
    {
        B2InputRngJointRawValidationReport authorityValidation =
            B2InputRngJointRawContract.ValidateTextForTest(authorityText);
        B2InputRngJointRawValidationReport unityValidation =
            B2InputRngJointRawContract.ValidateTextForTest(unityText);
        RequireValid(authorityValidation, "authority");
        RequireValid(unityValidation, "unity");
        return CompareCaptures(
            B2InputRngJointRawContract.LoadTextForTest(authorityText),
            B2InputRngJointRawContract.LoadTextForTest(unityText));
    }

    private static B2InputRngJointRawComparisonReport CompareCaptures(
        B2InputRngJointRawCapture authority,
        B2InputRngJointRawCapture unity)
    {
        RequireProducer(
            authority.Header,
            B2InputRngJointRawContract.AuthorityProducer,
            "authority");
        RequireProducer(
            unity.Header,
            B2InputRngJointRawContract.UnityProducer,
            "unity");
        string scenarioId = B2InputRngJointRawContract.RequireString(
            authority.Header,
            "scenarioId");
        if (!string.Equals(
                scenarioId,
                B2InputRngJointRawContract.RequireString(
                    unity.Header,
                    "scenarioId"),
                StringComparison.Ordinal))
        {
            throw new InvalidDataException("scenario-id-mismatch");
        }
        if (authority.Ticks.Count != unity.Ticks.Count)
            throw new InvalidDataException("tick-count-mismatch");

        var report = new B2InputRngJointRawComparisonReport
        {
            Schema = ComparisonSchema,
            ScenarioId = scenarioId,
            TicksCompared = authority.Ticks.Count,
            CertificateEligible = false,
        };

        if (CompareProperty(
                authority.Header,
                unity.Header,
                "initialInputPhase",
                "initialInputPhase",
                report))
        {
            return Finish(report);
        }
        if (CompareInitialRng(authority.Header, unity.Header, report))
            return Finish(report);

        for (int tickIndex = 0; tickIndex < authority.Ticks.Count; tickIndex++)
        {
            JsonObject authorityTick = authority.Ticks[tickIndex];
            JsonObject unityTick = unity.Ticks[tickIndex];
            long completedTick = B2InputRngJointRawContract.RequireInt64(
                authorityTick,
                "completedTick");
            if (CompareProperty(
                    authorityTick,
                    unityTick,
                    "inputPhase",
                    $"ticks[{completedTick}].inputPhase",
                    report,
                    completedTick))
            {
                return Finish(report);
            }
            if (CompareTickRng(
                    authorityTick,
                    unityTick,
                    completedTick,
                    report))
            {
                return Finish(report);
            }
            if (CompareEntities(
                    authorityTick,
                    unityTick,
                    completedTick,
                    report))
            {
                return Finish(report);
            }
        }

        return Finish(report);
    }

    private static bool CompareInitialRng(
        JsonObject authorityHeader,
        JsonObject unityHeader,
        B2InputRngJointRawComparisonReport report)
    {
        JsonObject authority = B2InputRngJointRawContract.RequireObject(
            authorityHeader,
            "initialRng");
        JsonObject unity = B2InputRngJointRawContract.RequireObject(
            unityHeader,
            "initialRng");
        JsonObject authorityCrt = B2InputRngJointRawContract.RequireObject(
            authority,
            "crt");
        JsonObject unityCrt = B2InputRngJointRawContract.RequireObject(
            unity,
            "crt");
        foreach (string property in new[] { "state", "totalCalls" })
        {
            if (CompareProperty(
                    authorityCrt,
                    unityCrt,
                    property,
                    $"initialRng.crt.{property}",
                    report))
            {
                return true;
            }
        }

        JsonObject authoritySynchronized =
            B2InputRngJointRawContract.RequireObject(
                authority,
                "synchronized");
        JsonObject unitySynchronized = B2InputRngJointRawContract.RequireObject(
            unity,
            "synchronized");
        foreach (string property in new[]
                 {
                     "counter", "index", "tableHash64", "lastCallSite",
                     "totalCalls",
                 })
        {
            if (CompareProperty(
                    authoritySynchronized,
                    unitySynchronized,
                    property,
                    $"initialRng.synchronized.{property}",
                    report))
            {
                return true;
            }
        }
        return false;
    }

    private static bool CompareTickRng(
        JsonObject authorityTick,
        JsonObject unityTick,
        long completedTick,
        B2InputRngJointRawComparisonReport report)
    {
        JsonObject authority = B2InputRngJointRawContract.RequireObject(
            authorityTick,
            "rng");
        JsonObject unity = B2InputRngJointRawContract.RequireObject(
            unityTick,
            "rng");
        JsonObject authorityCrt = B2InputRngJointRawContract.RequireObject(
            authority,
            "crt");
        JsonObject unityCrt = B2InputRngJointRawContract.RequireObject(
            unity,
            "crt");
        foreach (string property in new[]
                 {
                     "state", "totalCalls", "tickCallCount", "calls",
                 })
        {
            if (CompareProperty(
                    authorityCrt,
                    unityCrt,
                    property,
                    $"ticks[{completedTick}].rng.crt.{property}",
                    report,
                    completedTick))
            {
                return true;
            }
        }

        JsonObject authoritySynchronized =
            B2InputRngJointRawContract.RequireObject(
                authority,
                "synchronized");
        JsonObject unitySynchronized = B2InputRngJointRawContract.RequireObject(
            unity,
            "synchronized");
        JsonObject authorityState = B2InputRngJointRawContract.RequireObject(
            authoritySynchronized,
            "state");
        JsonObject unityState = B2InputRngJointRawContract.RequireObject(
            unitySynchronized,
            "state");
        foreach (string property in new[]
                 {
                     "counter", "index", "tableHash64", "lastCallSite",
                 })
        {
            if (CompareProperty(
                    authorityState,
                    unityState,
                    property,
                    $"ticks[{completedTick}].rng.synchronized.state.{property}",
                    report,
                    completedTick))
            {
                return true;
            }
        }
        foreach (string property in new[]
                 {
                     "totalCalls", "tickCallCount", "calls",
                 })
        {
            if (CompareProperty(
                    authoritySynchronized,
                    unitySynchronized,
                    property,
                    $"ticks[{completedTick}].rng.synchronized.{property}",
                    report,
                    completedTick))
            {
                return true;
            }
        }
        return false;
    }

    private static bool CompareEntities(
        JsonObject authorityTick,
        JsonObject unityTick,
        long completedTick,
        B2InputRngJointRawComparisonReport report)
    {
        JsonArray authorityEntities = B2InputRngJointRawContract.RequireArray(
            authorityTick,
            "entities");
        JsonArray unityEntities = B2InputRngJointRawContract.RequireArray(
            unityTick,
            "entities");
        if (authorityEntities.Count != unityEntities.Count)
        {
            SetDifference(
                report,
                $"ticks[{completedTick}].entities.count",
                completedTick,
                null,
                authorityEntities.Count.ToString(),
                unityEntities.Count.ToString());
            return true;
        }

        for (int index = 0; index < authorityEntities.Count; index++)
        {
            JsonObject authority = authorityEntities[index]!.AsObject();
            JsonObject unity = unityEntities[index]!.AsObject();
            int slot = B2InputRngJointRawContract.RequireInt32(
                authority,
                "slot");
            foreach (string property in new[]
                     {
                         "slot", "allocationEpoch", "objectId",
                     })
            {
                if (CompareProperty(
                        authority,
                        unity,
                        property,
                        $"entities[slot={slot}].{property}",
                        report,
                        completedTick,
                        slot))
                {
                    return true;
                }
            }

            JsonObject authorityInput =
                B2InputRngJointRawContract.RequireObject(authority, "input");
            JsonObject unityInput = B2InputRngJointRawContract.RequireObject(
                unity,
                "input");
            foreach (string property in new[]
                     {
                         "currentMask", "previousMask",
                     })
            {
                if (CompareProperty(
                        authorityInput,
                        unityInput,
                        property,
                        $"entities[slot={slot}].input.{property}",
                        report,
                        completedTick,
                        slot))
                {
                    return true;
                }
            }

            JsonObject authorityEdges =
                B2InputRngJointRawContract.RequireObject(
                    authorityInput,
                    "edgeWindow");
            JsonObject unityEdges = B2InputRngJointRawContract.RequireObject(
                unityInput,
                "edgeWindow");
            foreach (string edgeName in B2InputRngJointRawContract.EdgeNames)
            {
                if (CompareProperty(
                        authorityEdges,
                        unityEdges,
                        edgeName,
                        $"entities[slot={slot}].input.edgeWindow.{edgeName}",
                        report,
                        completedTick,
                        slot))
                {
                    return true;
                }
            }

            if (CompareProperty(
                    authorityInput,
                    unityInput,
                    "defendReentryCooldown",
                    $"entities[slot={slot}].input.defendReentryCooldown",
                    report,
                    completedTick,
                    slot))
            {
                return true;
            }
            foreach (string arrayName in new[]
                     {
                         "comboState", "keyHistory", "remapIndices",
                     })
            {
                JsonArray authorityArray =
                    B2InputRngJointRawContract.RequireArray(
                        authorityInput,
                        arrayName);
                JsonArray unityArray = B2InputRngJointRawContract.RequireArray(
                    unityInput,
                    arrayName);
                for (int item = 0; item < authorityArray.Count; item++)
                {
                    if (CompareNodes(
                            authorityArray[item],
                            unityArray[item],
                            $"entities[slot={slot}].input.{arrayName}[{item}]",
                            report,
                            completedTick,
                            slot))
                    {
                        return true;
                    }
                }
            }
            foreach (string property in new[]
                     {
                         "proxyTail", "runAccumulator", "lastAction",
                         "remapState", "boundState", "globalRecordState",
                     })
            {
                if (CompareProperty(
                        authorityInput,
                        unityInput,
                        property,
                        $"entities[slot={slot}].input.{property}",
                        report,
                        completedTick,
                        slot))
                {
                    return true;
                }
            }
            report.EntityPairsCompared++;
        }
        return false;
    }

    private static bool CompareProperty(
        JsonObject authority,
        JsonObject unity,
        string property,
        string path,
        B2InputRngJointRawComparisonReport report,
        long? completedTick = null,
        int? slot = null)
    {
        return CompareNodes(
            authority[property],
            unity[property],
            path,
            report,
            completedTick,
            slot);
    }

    private static bool CompareNodes(
        JsonNode? authority,
        JsonNode? unity,
        string path,
        B2InputRngJointRawComparisonReport report,
        long? completedTick,
        int? slot)
    {
        if (JsonNode.DeepEquals(authority, unity))
            return false;
        SetDifference(
            report,
            path,
            completedTick,
            slot,
            NodeText(authority),
            NodeText(unity));
        return true;
    }

    private static void SetDifference(
        B2InputRngJointRawComparisonReport report,
        string path,
        long? completedTick,
        int? slot,
        string authorityValue,
        string unityValue)
    {
        report.FirstDifference = new B2InputRngJointRawDifference
        {
            Path = path,
            CompletedTick = completedTick,
            Slot = slot,
            AuthorityValue = authorityValue,
            UnityValue = unityValue,
        };
    }

    private static string NodeText(JsonNode? value)
    {
        return value?.ToJsonString() ?? "null";
    }

    private static B2InputRngJointRawComparisonReport Finish(
        B2InputRngJointRawComparisonReport report)
    {
        report.Equal = report.FirstDifference == null;
        report.Status = report.Equal ? "equal-input-rng-joint-raw" : "different";
        return report;
    }

    private static void RequireValid(
        B2InputRngJointRawValidationReport report,
        string role)
    {
        if (!report.Valid)
            throw new InvalidDataException($"{role}-capture-invalid:{report.Reason}");
    }

    private static void RequireProducer(
        JsonObject header,
        string expected,
        string role)
    {
        if (!string.Equals(
                B2InputRngJointRawContract.RequireString(header, "producer"),
                expected,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException($"{role}-producer-mismatch");
        }
    }
}

internal sealed class B2InputRngJointRawComparisonReport
{
    public string Schema { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ScenarioId { get; set; } = string.Empty;
    public bool Equal { get; set; }
    public bool CertificateEligible { get; set; }
    public int TicksCompared { get; set; }
    public int EntityPairsCompared { get; set; }
    public B2InputRngJointRawDifference? FirstDifference { get; set; }
}

internal sealed class B2InputRngJointRawDifference
{
    public string Path { get; set; } = string.Empty;
    public long? CompletedTick { get; set; }
    public int? Slot { get; set; }
    public string AuthorityValue { get; set; } = string.Empty;
    public string UnityValue { get; set; } = string.Empty;
}
