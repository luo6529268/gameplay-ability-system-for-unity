using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class RawEntityCaptureComparator
{
    internal const string UnityCaptureSchema =
        "ntsd28-unity-raw-capture-v2";
    internal const string UnityTickSchema =
        "ntsd28-unity-entity-raw-capture-v2";
    internal const string UnityEvidenceClass =
        "UNITY_CURRENT_RUNTIME_DIAGNOSTIC_ONLY";
    internal const string UnityTickEvidenceClass =
        "UNITY_RAW_BINDING_DIAGNOSTIC_ONLY";
    internal const string ReportSchema =
        "ntsd28-raw-entity-comparison-v2";
    internal const string SelfTestSchema =
        "ntsd28-raw-entity-comparison-self-test-v2";

    private static readonly string[] UnityHeaderProperties =
    [
        "bindingStatus", "certificateEligible", "evidenceClass",
        "expectedTickCount", "exporterSourceSha256", "firstCompletedTick",
        "formalAuthorityExeSha256", "kind", "scenarioDataSha256",
        "scenarioFileSha256", "scenarioId", "scenarioReferenceExeSha256",
        "schema", "slotCapacity", "unityAssemblySha256",
        "content", "runtimeAssemblySha256",
    ];

    private static readonly string[] UnityTickProperties =
    [
        "bindingStatus", "certificateEligible", "completedTick", "entities",
        "evidenceClass", "kind", "schema", "slotCapacity",
    ];

    private static readonly string[] BindingProperties =
    [
        "candidate", "candidateCount", "fieldCount", "missing",
        "missingCount", "verifiedCount",
    ];

    private static readonly string[] CandidatePaths = EntityFieldContract.Fields
        .Where(field => field.BindingStatus == EntityFieldContract.CandidateBinding)
        .Select(field => field.Path)
        .ToArray();

    private static readonly string[] MissingPaths = EntityFieldContract.Fields
        .Where(field => field.BindingStatus == EntityFieldContract.MissingBinding)
        .Select(field => field.Path)
        .ToArray();

    internal static RawEntityComparisonReport CompareFiles(
        string authorityPath,
        string unityPath)
    {
        return Compare(
            File.ReadAllText(authorityPath, Encoding.UTF8),
            Path.GetFileName(authorityPath),
            File.ReadAllText(unityPath, Encoding.UTF8),
            Path.GetFileName(unityPath));
    }

    internal static RawEntityComparisonReport CompareTextForTest(
        string authority,
        string unity)
    {
        return Compare(
            authority,
            "synthetic-authority.raw.jsonl",
            unity,
            "synthetic-unity.raw.jsonl");
    }

    private static RawEntityComparisonReport Compare(
        string authorityText,
        string authoritySource,
        string unityText,
        string unitySource)
    {
        var report = new RawEntityComparisonReport
        {
            Schema = ReportSchema,
            Authority = authoritySource,
            Unity = unitySource,
            CertificateEligible = false,
        };

        try
        {
            AuthorityCaptureValidationReport authorityValidation =
                AuthorityCaptureValidator.ValidateTextForTest(authorityText);
            if (!authorityValidation.Valid)
            {
                throw new InvalidDataException(
                    "authority-capture-invalid:" + authorityValidation.Reason);
            }

            ParsedRawCapture authority = ParseAuthority(
                authorityText,
                authorityValidation);
            ParsedRawCapture unity = ParseUnity(unityText);
            if (authority.ContentIdentityKey != unity.ContentIdentityKey)
                throw new InvalidDataException("content-identity-mismatch");
            if (!string.Equals(
                    authority.ScenarioId,
                    unity.ScenarioId,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException("scenario-id-mismatch");
            }
            if (!string.Equals(
                    authority.ScenarioDataSha256,
                    unity.ScenarioDataSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("scenario-data-sha256-mismatch");
            }
            if (authority.Ticks.Count != unity.Ticks.Count)
                throw new InvalidDataException("tick-count-mismatch");

            var differences = new Dictionary<string, RawEntityFieldDifference>(
                StringComparer.Ordinal);
            var differentPaths = new HashSet<string>(StringComparer.Ordinal);
            for (int tickIndex = 0; tickIndex < authority.Ticks.Count; tickIndex++)
            {
                ParsedRawTick authorityTick = authority.Ticks[tickIndex];
                ParsedRawTick unityTick = unity.Ticks[tickIndex];
                if (authorityTick.CompletedTick != unityTick.CompletedTick)
                    throw new InvalidDataException("completed-tick-mismatch");
                if (!authorityTick.Entities.Keys.SequenceEqual(
                        unityTick.Entities.Keys))
                {
                    throw new InvalidDataException("entity-slot-set-mismatch");
                }

                foreach (int slot in authorityTick.Entities.Keys)
                {
                    JsonObject authorityEntity = authorityTick.Entities[slot];
                    JsonObject unityEntity = unityTick.Entities[slot];
                    report.EntityPairsCompared++;
                    foreach (EntityFieldDescriptor field in
                             EntityFieldContract.Fields)
                    {
                        report.FieldOccurrencesCompared++;
                        JsonNode? authorityValue = GetField(
                            authorityEntity,
                            field.Path);
                        JsonNode? unityValue = GetField(unityEntity, field.Path);
                        if (FieldValuesEqual(
                                authorityValue,
                                unityValue,
                                field.JsonType))
                        {
                            continue;
                        }

                        report.DifferenceOccurrences++;
                        differentPaths.Add(field.Path);
                        if (!differences.TryGetValue(
                                field.Path,
                                out RawEntityFieldDifference? difference))
                        {
                            difference = new RawEntityFieldDifference
                            {
                                Path = field.Path,
                                BindingStatus = field.BindingStatus,
                                Classification = unityValue is null &&
                                    field.BindingStatus ==
                                        EntityFieldContract.MissingBinding
                                    ? "UNITY_BINDING_MISSING"
                                    : "VALUE_DIFFERENCE",
                                FirstCompletedTick = authorityTick.CompletedTick,
                                FirstSlot = slot,
                                FirstAuthorityValue = Display(authorityValue),
                                FirstUnityValue = Display(unityValue),
                            };
                            differences.Add(field.Path, difference);
                        }
                        difference.OccurrenceCount++;
                    }
                }
                report.TicksCompared++;
            }

            report.Differences = EntityFieldContract.Fields
                .Where(field => differences.ContainsKey(field.Path))
                .Select(field => differences[field.Path])
                .ToList();
            report.EqualFields = EntityFieldContract.Fields
                .Where(field => !differentPaths.Contains(field.Path))
                .Select(field => field.Path)
                .ToList();
            report.UniqueDifferenceFields = report.Differences.Count;
            report.UniqueEqualFields = report.EqualFields.Count;
            report.Status = report.UniqueDifferenceFields == 0
                ? "equal-raw"
                : "different";
            report.FirstDifference = report.Differences
                .OrderBy(difference => difference.FirstCompletedTick)
                .ThenBy(difference => difference.FirstSlot)
                .FirstOrDefault();
            return report;
        }
        catch (Exception exception) when (TraceComparator.IsContractFailure(exception))
        {
            report.Status = "invalid-capture";
            report.Reason = exception.Message;
            return report;
        }
    }

    private static ParsedRawCapture ParseAuthority(
        string text,
        AuthorityCaptureValidationReport validation)
    {
        string[] lines = SplitLines(text);
        var ticks = new List<ParsedRawTick>(validation.ValidatedTicks);
        for (int index = 1; index < lines.Length; index++)
        {
            JsonObject tick = ParseObject(lines[index], "authority-tick");
            long completedTick = RequirePositiveInt64(tick, "completedTick");
            JsonArray entities = RequireArray(tick, "entities");
            ticks.Add(new ParsedRawTick(
                completedTick,
                IndexAuthorityEntities(entities)));
        }

        return new ParsedRawCapture(
            validation.ScenarioId,
            validation.ScenarioDataSha256,
            validation.ContentIdentityKey,
            ticks);
    }

    private static ParsedRawCapture ParseUnity(string text)
    {
        string[] lines = SplitLines(text);
        if (lines.Length == 0)
            throw new InvalidDataException("unity-header-missing");
        JsonObject header = ParseObject(lines[0], "unity-header");
        RequireExactProperties(header, "unity-header", UnityHeaderProperties);
        RequireString(header, "kind", "header");
        RequireString(header, "schema", UnityCaptureSchema);
        if (RequireBoolean(header, "certificateEligible"))
        {
            throw new InvalidDataException(
                "unity-raw-capture-cannot-be-certificate-eligible");
        }
        RequireString(header, "evidenceClass", UnityEvidenceClass);
        RequireSha256(
            header,
            "formalAuthorityExeSha256",
            TraceContract.AuthorityExecutableSha256);
        string scenarioReferenceSha = RequireSha256(
            header,
            "scenarioReferenceExeSha256");
        if (!string.Equals(
                scenarioReferenceSha,
                AuthorityCaptureValidator.LegacyScenarioReferenceSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "unity-scenario-reference-sha256-mismatch");
        }
        string scenarioDataSha256 = RequireSha256(
            header,
            "scenarioDataSha256");
        _ = RequireSha256(header, "scenarioFileSha256");
        _ = RequireSha256(header, "exporterSourceSha256");
        _ = RequireSha256(header, "unityAssemblySha256");
        _ = RequireSha256(header, "runtimeAssemblySha256");
        string contentIdentityKey = TraceContentIdentity.Validate(
            header["content"] as JsonObject ?? throw new InvalidDataException("content-object-required"));
        string scenarioId = RequireNonEmptyString(header, "scenarioId");
        long firstCompletedTick = RequirePositiveInt64(
            header,
            "firstCompletedTick");
        if (firstCompletedTick != 1)
            throw new InvalidDataException("unity-first-completed-tick-must-be-one");
        int expectedTickCount = RequirePositiveInt32(
            header,
            "expectedTickCount");
        int slotCapacity = RequirePositiveInt32(header, "slotCapacity");
        ValidateBindingStatus(RequireObject(header, "bindingStatus"));

        if (lines.Length != expectedTickCount + 1)
            throw new InvalidDataException("unity-tick-count-mismatch");
        var ticks = new List<ParsedRawTick>(expectedTickCount);
        for (int index = 0; index < expectedTickCount; index++)
        {
            JsonObject tick = ParseObject(lines[index + 1], "unity-tick");
            RequireExactProperties(tick, "unity-tick", UnityTickProperties);
            RequireString(tick, "kind", "tick");
            RequireString(tick, "schema", UnityTickSchema);
            RequireString(tick, "evidenceClass", UnityTickEvidenceClass);
            if (RequireBoolean(tick, "certificateEligible"))
            {
                throw new InvalidDataException(
                    "unity-raw-tick-cannot-be-certificate-eligible");
            }
            int tickCapacity = RequirePositiveInt32(tick, "slotCapacity");
            if (tickCapacity != slotCapacity)
                throw new InvalidDataException("unity-tick-slot-capacity-mismatch");
            ValidateBindingStatus(RequireObject(tick, "bindingStatus"));
            long completedTick = RequirePositiveInt64(tick, "completedTick");
            long expectedTick = firstCompletedTick + index;
            if (completedTick != expectedTick)
                throw new InvalidDataException("unity-non-contiguous-completed-tick");
            JsonArray entities = RequireArray(tick, "entities");
            ticks.Add(new ParsedRawTick(
                completedTick,
                ValidateAndIndexUnityEntities(entities, slotCapacity)));
        }

        return new ParsedRawCapture(scenarioId, scenarioDataSha256, contentIdentityKey, ticks);
    }

    private static SortedDictionary<int, JsonObject> IndexAuthorityEntities(
        JsonArray entities)
    {
        var result = new SortedDictionary<int, JsonObject>();
        for (int index = 0; index < entities.Count; index++)
        {
            JsonObject entity = RequireArrayObject(
                entities,
                index,
                "authority-entities");
            int slot = RequireNonNegativeInt32(entity, "slot");
            result.Add(slot, entity);
        }
        return result;
    }

    private static SortedDictionary<int, JsonObject>
        ValidateAndIndexUnityEntities(JsonArray entities, int slotCapacity)
    {
        var result = new SortedDictionary<int, JsonObject>();
        int previousSlot = -1;
        for (int index = 0; index < entities.Count; index++)
        {
            JsonObject entity = RequireArrayObject(
                entities,
                index,
                "unity-entities");
            RequireExactProperties(
                entity,
                "unity-entity",
                EntityFieldContract.RootProperties);
            int slot = RequireNonNegativeInt32(entity, "slot");
            if (slot >= slotCapacity || slot <= previousSlot)
                throw new InvalidDataException("unity-entity-slot-order-or-range");
            previousSlot = slot;
            _ = RequirePositiveInt64(entity, "allocationEpoch");
            if (!RequireBoolean(entity, "active"))
                throw new InvalidDataException("unity-inactive-entity-in-array");

            foreach ((string groupName, string[] properties) in
                     EntityFieldContract.GroupProperties)
            {
                JsonObject group = RequireObject(entity, groupName);
                RequireExactProperties(
                    group,
                    "unity-entity-" + groupName,
                    properties);
                foreach (string property in properties)
                {
                    EntityFieldDescriptor field =
                        EntityFieldContract.FieldAtPath(
                            groupName + "." + property);
                    JsonNode? value = group[property];
                    if (value is null)
                    {
                        if (field.BindingStatus !=
                            EntityFieldContract.MissingBinding)
                        {
                            throw new InvalidDataException(
                                "unity-null-nonmissing-field:" + field.Path);
                        }
                        continue;
                    }
                    ValidateValueType(value, field);
                }
            }
            result.Add(slot, entity);
        }
        return result;
    }

    private static void ValidateBindingStatus(JsonObject bindingStatus)
    {
        RequireExactProperties(
            bindingStatus,
            "bindingStatus",
            BindingProperties);
        JsonArray candidate = RequireArray(bindingStatus, "candidate");
        JsonArray missing = RequireArray(bindingStatus, "missing");
        if (!TraceContract.SequenceEqual(candidate, CandidatePaths) ||
            !TraceContract.SequenceEqual(missing, MissingPaths) ||
            RequireInt32(bindingStatus, "candidateCount") !=
                CandidatePaths.Length ||
            RequireInt32(bindingStatus, "missingCount") !=
                MissingPaths.Length ||
            RequireInt32(bindingStatus, "verifiedCount") !=
                EntityFieldContract.Fields.Count(field =>
                    field.BindingStatus == EntityFieldContract.VerifiedBinding) ||
            RequireInt32(bindingStatus, "fieldCount") !=
                EntityFieldContract.Fields.Length)
        {
            throw new InvalidDataException("unity-binding-status-mismatch");
        }
    }

    private static JsonNode? GetField(JsonObject entity, string path)
    {
        int separator = path.IndexOf('.');
        if (separator < 0)
            return entity[path];
        string group = path[..separator];
        string property = path[(separator + 1)..];
        return entity[group]![property];
    }

    private static bool FieldValuesEqual(
        JsonNode? authority,
        JsonNode? unity,
        string jsonType)
    {
        if (authority is null || unity is null)
            return authority is null && unity is null;
        return jsonType switch
        {
            "int32" or "int64" => ReadInt64(authority) == ReadInt64(unity),
            "boolean" => ReadBoolean(authority) == ReadBoolean(unity),
            "float64" => ReadDouble(authority).Equals(ReadDouble(unity)),
            _ => throw new InvalidDataException(
                "unsupported-entity-field-type:" + jsonType),
        };
    }

    private static void ValidateValueType(
        JsonNode value,
        EntityFieldDescriptor field)
    {
        switch (field.JsonType)
        {
            case "int32":
                long int32 = ReadInt64(value);
                if (int32 < int.MinValue || int32 > int.MaxValue)
                    throw new InvalidDataException("field-must-fit-int32:" + field.Path);
                return;
            case "int64":
                _ = ReadInt64(value);
                return;
            case "boolean":
                _ = ReadBoolean(value);
                return;
            case "float64":
                double number = ReadDouble(value);
                if (!double.IsFinite(number))
                    throw new InvalidDataException("field-must-be-finite:" + field.Path);
                return;
            default:
                throw new InvalidDataException(
                    "unsupported-entity-field-type:" + field.JsonType);
        }
    }

    private static long ReadInt64(JsonNode node)
    {
        return node is JsonValue value && value.TryGetValue(out long result)
            ? result
            : throw new InvalidDataException("value-must-be-int64");
    }

    private static double ReadDouble(JsonNode node)
    {
        return node is JsonValue value && value.TryGetValue(out double result)
            ? result
            : throw new InvalidDataException("value-must-be-float64");
    }

    private static bool ReadBoolean(JsonNode node)
    {
        return node is JsonValue value && value.TryGetValue(out bool result)
            ? result
            : throw new InvalidDataException("value-must-be-boolean");
    }

    private static string Display(JsonNode? value)
    {
        return value is null
            ? "null"
            : TraceContract.Canonicalize(value).ToJsonString();
    }

    internal static RawEntityComparisonSelfTestReport RunSelfTest()
    {
        var report = new RawEntityComparisonSelfTestReport
        {
            Schema = SelfTestSchema,
        };
        string authority = BuildSyntheticAuthorityCapture();
        string equalUnity = BuildSyntheticUnityCapture(nullMissing: false);
        AddSelfTest(
            report,
            "equal-raw",
            "equal-raw",
            0,
            CompareTextForTest(authority, equalUnity));

        string missingUnity = BuildSyntheticUnityCapture(nullMissing: true);
        AddSelfTest(
            report,
            "missing-bindings",
            "different",
            MissingPaths.Length,
            CompareTextForTest(authority, missingUnity));

        string invalidNull = MutateUnityField(
            equalUnity,
            "frame",
            "previousAction",
            null);
        AddSelfTest(
            report,
            "candidate-null-rejected",
            "invalid-capture",
            0,
            CompareTextForTest(authority, invalidNull));

        string nonContiguous = MutateUnityTick(equalUnity, 2);
        AddSelfTest(
            report,
            "non-contiguous-tick-rejected",
            "invalid-capture",
            0,
            CompareTextForTest(authority, nonContiguous));

        string valueDifference = MutateUnityField(
            equalUnity,
            "vitals",
            "baseMaxMp",
            JsonValue.Create(499));
        AddSelfTest(
            report,
            "value-difference",
            "different",
            1,
            CompareTextForTest(authority, valueDifference));

        var headerMutations = new Dictionary<string, Action<JsonObject>>
        {
            ["old-raw-version"] = header => header["schema"] = "ntsd28-unity-raw-capture-v1",
            ["missing-content"] = header => header.Remove("content"),
            ["forged-semantic"] = header => header["content"]!["semanticSha256"] = new string('0', 64),
            ["different-valid-content"] = header => header["content"] = TraceContentIdentity.Create("logan-runtime", new string('B', 64)),
            ["legacy-content-not-logan"] = header => header["content"] = TraceContentIdentity.Create("unity-legacy", new string('A', 64)),
            ["wrong-joint-schema"] = header => header["content"]!["schemas"]!["entityRuntime"] = 12,
            ["missing-runtime-provenance"] = header => header.Remove("runtimeAssemblySha256"),
        };
        foreach (var mutation in headerMutations)
        {
            string[] lines = SplitLines(equalUnity);
            JsonObject header = ParseObject(lines[0], "synthetic-header");
            mutation.Value(header);
            lines[0] = header.ToJsonString();
            AddSelfTest(report, "Q05-" + mutation.Key, "invalid-capture", 0,
                CompareTextForTest(authority, string.Join(Environment.NewLine, lines) + Environment.NewLine));
        }
        AddSelfTest(report, "Q05-nondefault-2f8-difference", "different", 1,
            CompareTextForTest(authority, MutateUnityField(equalUnity, "combat", "objectAiExcludedGroupSourceSlot", JsonValue.Create(37))));
        static string TwoTicks(string capture, bool different)
        {
            string[] lines = SplitLines(capture);
            JsonObject header = ParseObject(lines[0], "synthetic-header");
            header["expectedTickCount"] = 2;
            JsonObject first = ParseObject(lines[1], "synthetic-first");
            JsonObject second = first.DeepClone().AsObject();
            second["completedTick"] = 2;
            if (different)
            {
                first["entities"]![0]!["vitals"]!["baseMaxMp"] = 499;
                second["entities"]![0]!["vitals"]!["currentMp"] = 99999;
            }
            return SerializeLines(header, first, second);
        }
        var chronological = CompareTextForTest(TwoTicks(authority, false), TwoTicks(equalUnity, true));
        AddSelfTest(report, "Q05-earliest-tick-first-difference", "different", 2, chronological);
        report.Cases[^1].Passed &= chronological.FirstDifference?.Path == "vitals.baseMaxMp" &&
            chronological.FirstDifference.FirstCompletedTick == 1;
        if (!report.Cases[^1].Passed)
            report.Cases[^1].Reason = "Expected earliest tick 1/baseMaxMp, observed " + chronological.FirstDifference?.Path;
        report.Passed = report.Cases.All(test => test.Passed);
        return report;
    }

    private static void AddSelfTest(
        RawEntityComparisonSelfTestReport report,
        string name,
        string expectedStatus,
        int expectedUniqueDifferences,
        RawEntityComparisonReport actual)
    {
        report.Cases.Add(new RawEntityComparisonSelfTestCase
        {
            Name = name,
            ExpectedStatus = expectedStatus,
            ActualStatus = actual.Status,
            ExpectedUniqueDifferences = expectedUniqueDifferences,
            ActualUniqueDifferences = actual.UniqueDifferenceFields,
            Reason = actual.Reason,
            Passed = string.Equals(
                         actual.Status,
                         expectedStatus,
                         StringComparison.Ordinal) &&
                     actual.UniqueDifferenceFields == expectedUniqueDifferences,
        });
    }

    private static string BuildSyntheticAuthorityCapture()
    {
        JsonObject header = new()
        {
            ["kind"] = "header",
            ["schema"] = AuthorityCaptureValidator.CaptureSchema,
            ["content"] = TraceContentIdentity.Create("logan-runtime", new string('A', 64)),
            ["certificateEligible"] = false,
            ["evidenceClass"] = AuthorityCaptureValidator.EvidenceClass,
            ["formalExeSha256"] = TraceContract.AuthorityExecutableSha256,
            ["authoritySourceManifestSha256"] = new string('A', 64),
            ["captureRunnerSourceSha256"] = new string('B', 64),
            ["captureBinarySha256"] = new string('C', 64),
            ["scenarioReferenceExeSha256"] =
                AuthorityCaptureValidator.LegacyScenarioReferenceSha256,
            ["scenarioDataSha256"] = new string('D', 64),
            ["scenarioId"] = "raw-self-test",
            ["firstCompletedTick"] = 1,
            ["expectedTickCount"] = 1,
            ["slotCapacity"] = AuthorityCaptureValidator.AuthoritySlotCapacity,
        };
        JsonObject tick = new()
        {
            ["kind"] = "tick",
            ["completedTick"] = 1,
            ["entities"] = new JsonArray(BuildSyntheticEntity(false)),
        };
        return SerializeLines(header, tick);
    }

    private static string BuildSyntheticUnityCapture(bool nullMissing)
    {
        JsonObject header = new()
        {
            ["bindingStatus"] = BuildBindingStatus(),
            ["certificateEligible"] = false,
            ["evidenceClass"] = UnityEvidenceClass,
            ["expectedTickCount"] = 1,
            ["exporterSourceSha256"] = new string('E', 64),
            ["firstCompletedTick"] = 1,
            ["formalAuthorityExeSha256"] = TraceContract.AuthorityExecutableSha256,
            ["kind"] = "header",
            ["scenarioDataSha256"] = new string('D', 64),
            ["scenarioFileSha256"] = new string('F', 64),
            ["scenarioId"] = "raw-self-test",
            ["scenarioReferenceExeSha256"] =
                AuthorityCaptureValidator.LegacyScenarioReferenceSha256,
            ["schema"] = UnityCaptureSchema,
            ["slotCapacity"] = 400,
            ["unityAssemblySha256"] = new string('1', 64),
            ["runtimeAssemblySha256"] = new string('2', 64),
            ["content"] = TraceContentIdentity.Create("logan-runtime", new string('A', 64)),
        };
        JsonObject tick = new()
        {
            ["bindingStatus"] = BuildBindingStatus(),
            ["certificateEligible"] = false,
            ["completedTick"] = 1,
            ["entities"] = new JsonArray(BuildSyntheticEntity(nullMissing)),
            ["evidenceClass"] = UnityTickEvidenceClass,
            ["kind"] = "tick",
            ["schema"] = UnityTickSchema,
            ["slotCapacity"] = 400,
        };
        return SerializeLines(header, tick);
    }

    private static JsonObject BuildSyntheticEntity(bool nullMissing)
    {
        JsonObject entity = new()
        {
            ["slot"] = 0,
            ["allocationEpoch"] = 1,
            ["active"] = true,
        };
        foreach ((string group, string[] properties) in
                 EntityFieldContract.GroupProperties)
        {
            var groupValue = new JsonObject();
            foreach (string property in properties)
            {
                EntityFieldDescriptor field =
                    EntityFieldContract.FieldAtPath(group + "." + property);
                groupValue[property] = nullMissing &&
                    field.BindingStatus == EntityFieldContract.MissingBinding
                    ? null
                    : BuildDefaultValue(field.JsonType);
            }
            entity[group] = groupValue;
        }
        return entity;
    }

    private static JsonNode BuildDefaultValue(string jsonType)
    {
        JsonNode? value = jsonType switch
        {
            "int32" => JsonValue.Create(0),
            "int64" => JsonValue.Create(1L),
            "boolean" => JsonValue.Create(false),
            "float64" => JsonValue.Create(0.0),
            _ => throw new InvalidDataException(
                "unsupported-entity-field-type:" + jsonType),
        };
        return value ?? throw new InvalidDataException(
            "default-entity-field-value-was-null:" + jsonType);
    }

    private static JsonObject BuildBindingStatus()
    {
        return new JsonObject
        {
            ["candidate"] = ToJsonArray(CandidatePaths),
            ["candidateCount"] = CandidatePaths.Length,
            ["fieldCount"] = EntityFieldContract.Fields.Length,
            ["missing"] = ToJsonArray(MissingPaths),
            ["missingCount"] = MissingPaths.Length,
            ["verifiedCount"] = EntityFieldContract.Fields.Count(field =>
                field.BindingStatus == EntityFieldContract.VerifiedBinding),
        };
    }

    private static string MutateUnityField(
        string unity,
        string group,
        string property,
        JsonNode? value)
    {
        string[] lines = SplitLines(unity);
        JsonObject tick = ParseObject(lines[1], "synthetic-unity-tick");
        tick["entities"]![0]![group]![property] = value;
        lines[1] = Serialize(tick);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string MutateUnityTick(string unity, long completedTick)
    {
        string[] lines = SplitLines(unity);
        JsonObject tick = ParseObject(lines[1], "synthetic-unity-tick");
        tick["completedTick"] = completedTick;
        lines[1] = Serialize(tick);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values)
    {
        var result = new JsonArray();
        foreach (string value in values)
            result.Add(value);
        return result;
    }

    private static string SerializeLines(params JsonObject[] lines)
    {
        return string.Join(Environment.NewLine, lines.Select(Serialize)) +
               Environment.NewLine;
    }

    private static string Serialize(JsonNode node)
    {
        return TraceContract.Canonicalize(node).ToJsonString(
            new JsonSerializerOptions { WriteIndented = false });
    }

    private static string[] SplitLines(string source)
    {
        return source.Split(
            ["\r\n", "\n"],
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static JsonObject ParseObject(string text, string context)
    {
        return JsonNode.Parse(text) as JsonObject ??
            throw new InvalidDataException(context + "-must-be-object");
    }

    private static JsonObject RequireObject(JsonObject owner, string property)
    {
        return owner[property] as JsonObject ??
            throw new InvalidDataException(property + "-must-be-object");
    }

    private static JsonArray RequireArray(JsonObject owner, string property)
    {
        return owner[property] as JsonArray ??
            throw new InvalidDataException(property + "-must-be-array");
    }

    private static JsonObject RequireArrayObject(
        JsonArray owner,
        int index,
        string context)
    {
        return owner[index] as JsonObject ??
            throw new InvalidDataException($"{context}[{index}]-must-be-object");
    }

    private static void RequireExactProperties(
        JsonObject owner,
        string context,
        IReadOnlyCollection<string> expected)
    {
        if (!TraceContract.HasExactProperties(owner, expected))
            throw new InvalidDataException(context + "-property-set-mismatch");
    }

    private static string RequireString(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out string? result) || result is null)
        {
            throw new InvalidDataException(property + "-must-be-string");
        }
        return result;
    }

    private static string RequireNonEmptyString(
        JsonObject owner,
        string property)
    {
        string value = RequireString(owner, property);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDataException(property + "-must-be-nonempty");
        return value;
    }

    private static void RequireString(
        JsonObject owner,
        string property,
        string expected)
    {
        string actual = RequireString(owner, property);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidDataException(property + "-mismatch");
    }

    private static string RequireSha256(JsonObject owner, string property)
    {
        string value = RequireString(owner, property);
        if (!TraceContract.IsSha256(value))
            throw new InvalidDataException(property + "-must-be-sha256");
        return value.ToUpperInvariant();
    }

    private static void RequireSha256(
        JsonObject owner,
        string property,
        string expected)
    {
        string actual = RequireSha256(owner, property);
        if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(property + "-mismatch");
    }

    private static bool RequireBoolean(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out bool result))
        {
            throw new InvalidDataException(property + "-must-be-boolean");
        }
        return result;
    }

    private static long RequireInt64(JsonObject owner, string property)
    {
        if (owner[property] is not JsonValue value ||
            !value.TryGetValue(out long result))
        {
            throw new InvalidDataException(property + "-must-be-int64");
        }
        return result;
    }

    private static int RequireInt32(JsonObject owner, string property)
    {
        long value = RequireInt64(owner, property);
        if (value < int.MinValue || value > int.MaxValue)
            throw new InvalidDataException(property + "-must-fit-int32");
        return (int)value;
    }

    private static int RequirePositiveInt32(JsonObject owner, string property)
    {
        int value = RequireInt32(owner, property);
        if (value <= 0)
            throw new InvalidDataException(property + "-must-be-positive");
        return value;
    }

    private static int RequireNonNegativeInt32(
        JsonObject owner,
        string property)
    {
        int value = RequireInt32(owner, property);
        if (value < 0)
            throw new InvalidDataException(property + "-must-be-nonnegative");
        return value;
    }

    private static long RequirePositiveInt64(
        JsonObject owner,
        string property)
    {
        long value = RequireInt64(owner, property);
        if (value <= 0)
            throw new InvalidDataException(property + "-must-be-positive");
        return value;
    }

    private sealed record ParsedRawCapture(
        string ScenarioId,
        string ScenarioDataSha256,
        string ContentIdentityKey,
        List<ParsedRawTick> Ticks);

    private sealed record ParsedRawTick(
        long CompletedTick,
        SortedDictionary<int, JsonObject> Entities);
}

internal sealed class RawEntityComparisonReport
{
    public string Schema { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string Unity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool CertificateEligible { get; set; }
    public int TicksCompared { get; set; }
    public int EntityPairsCompared { get; set; }
    public int FieldOccurrencesCompared { get; set; }
    public int DifferenceOccurrences { get; set; }
    public int UniqueDifferenceFields { get; set; }
    public int UniqueEqualFields { get; set; }
    public RawEntityFieldDifference? FirstDifference { get; set; }
    public List<RawEntityFieldDifference> Differences { get; set; } = [];
    public List<string> EqualFields { get; set; } = [];
    public string? Reason { get; set; }
}

internal sealed class RawEntityFieldDifference
{
    public string Path { get; set; } = string.Empty;
    public string BindingStatus { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; }
    public long FirstCompletedTick { get; set; }
    public int FirstSlot { get; set; }
    public string FirstAuthorityValue { get; set; } = string.Empty;
    public string FirstUnityValue { get; set; } = string.Empty;
}

internal sealed class RawEntityComparisonSelfTestReport
{
    public string Schema { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public List<RawEntityComparisonSelfTestCase> Cases { get; set; } = [];
}

internal sealed class RawEntityComparisonSelfTestCase
{
    public string Name { get; set; } = string.Empty;
    public string ExpectedStatus { get; set; } = string.Empty;
    public string ActualStatus { get; set; } = string.Empty;
    public int ExpectedUniqueDifferences { get; set; }
    public int ActualUniqueDifferences { get; set; }
    public string? Reason { get; set; }
    public bool Passed { get; set; }
}
