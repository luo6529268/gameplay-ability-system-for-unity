using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class B0DomainRawContractSelfTest
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
    };

    internal static B0DomainRawSelfTestReport Run()
    {
        var report = new B0DomainRawSelfTestReport
        {
            Schema = "ntsd28-logan-b0-domain-raw-self-test-v1",
        };

        RunCase(report, "descriptor", "descriptor-valid", () =>
            ValidateDescriptor() ? "descriptor-valid" : "descriptor-invalid");

        string authority = BuildCapture(B0DomainRawContract.AuthorityProducer);
        string unity = BuildCapture(B0DomainRawContract.UnityProducer);
        RunValidationCase(report, "authority-valid", "valid-b0-domain-raw", authority);
        RunValidationCase(report, "unity-valid", "valid-b0-domain-raw", unity);
        RunValidationCase(report, "unity-ai-diagnostic-valid", "valid-b0-domain-raw",
            MutateTick(unity, 1, tick => tick["aiAcceptedTrace"] = new JsonObject
            {
                ["committed"] = 0,
                ["eligible"] = 0,
                ["fallback"] = 0,
                ["firstFallbackReason"] = "None",
                ["oracleMismatch"] = 0,
            }));
        RunValidationCase(report, "authority-ai-diagnostic-rejected", "invalid",
            MutateTick(authority, 1, tick => tick["aiAcceptedTrace"] = new JsonObject
            {
                ["committed"] = 0,
                ["eligible"] = 0,
                ["fallback"] = 0,
                ["firstFallbackReason"] = "None",
                ["oracleMismatch"] = 0,
            }));
        RunValidationCase(report, "unity-ai-diagnostic-malformed-rejected", "invalid",
            MutateTick(unity, 1, tick => tick["aiAcceptedTrace"] = new JsonObject
            {
                ["committed"] = -1,
                ["eligible"] = 0,
                ["fallback"] = 0,
                ["firstFallbackReason"] = "None",
                ["oracleMismatch"] = 0,
            }));
        RunValidationCase(report, "unity-ai-diagnostic-extra-field-rejected", "invalid",
            MutateTick(unity, 1, tick => tick["aiAcceptedTrace"] = new JsonObject
            {
                ["committed"] = 0,
                ["eligible"] = 0,
                ["fallback"] = 0,
                ["firstFallbackReason"] = "None",
                ["oracleMismatch"] = 0,
                ["unexpected"] = 1,
            }));
        RunValidationCase(
            report,
            "truncated",
            "invalid",
            JoinLines(SplitLines(authority)[..^1]));
        RunValidationCase(
            report,
            "extra-tick",
            "invalid",
            authority + Serialize(BuildTick(
                B0DomainRawContract.AuthorityProducer,
                completedTick: 3,
                crtTotal: 7,
                synchronizedTotal: 10,
                unityTotal: 0,
                occupants: Occupants((0, 2UL, 9)),
                events: Events())) + Environment.NewLine);
        RunValidationCase(
            report,
            "forged-stream-availability",
            "invalid",
            MutateHeader(unity, header =>
                header["streamAvailability"]!["authorityCrt"] =
                    B0DomainRawContract.Available));
        RunValidationCase(
            report,
            "input-mask-outside-seven-actions",
            "invalid",
            MutateTick(authority, 1, tick =>
                tick["input"]!["players"]![0]!["heldMask"] = 128));
        RunValidationCase(
            report,
            "rng-call-delta-forged",
            "invalid",
            MutateTick(authority, 2, tick =>
                tick["rng"]!["authorityCrt"]!["tickCallCount"] = 99));
        RunValidationCase(
            report,
            "occupants-not-ascending",
            "invalid",
            MutateTick(authority, 1, tick =>
            {
                JsonArray occupants = tick["slots"]!["occupants"]!.AsArray();
                tick["slots"]!["occupants"] = new JsonArray
                {
                    occupants[1]!.DeepClone(),
                    occupants[0]!.DeepClone(),
                };
            }));
        RunValidationCase(
            report,
            "forged-lifecycle-delta",
            "invalid",
            MutateTick(authority, 1, tick =>
                tick["lifecycleDelta"]!["events"]!.AsArray().Clear()));
        RunValidationCase(
            report,
            "occupant-changed-without-epoch",
            "invalid",
            MutateTick(authority, 2, tick =>
            {
                JsonObject occupant = tick["slots"]!["occupants"]![0]!.AsObject();
                occupant["allocationEpoch"] = 1;
                occupant["objectId"] = 77;
            }));
        RunValidationCase(
            report,
            "available-stream-null-total",
            "invalid",
            MutateTick(authority, 1, tick =>
                tick["rng"]!["authorityCrt"]!["totalCalls"] = null));

        const string v2Schema = "ntsd28-logan-b0-domain-raw-v2";
        RunCase(report, "v2-descriptor", "descriptor-valid", () =>
        {
            JsonObject descriptor = JsonSerializer.SerializeToNode(
                B0DomainRawContract.CreateDescriptor(v2Schema), CompactOptions)!.AsObject();
            return descriptor["schema"]!.GetValue<string>() == B0DomainRawContract.DescriptorSchemaV2 &&
                   descriptor["rawSchema"]!.GetValue<string>() == v2Schema &&
                   !descriptor["certificateEligible"]!.GetValue<bool>() &&
                   descriptor["lifecycleDelta"]!["provenance"]!.GetValue<string>() == "snapshot-derived" &&
                   TraceContract.SequenceEqual(descriptor["lifecycleDelta"]!["eventKinds"]!.AsArray(),
                       new[] { "birth", "death", "reuse", "object-id-change" })
                ? "descriptor-valid" : "descriptor-invalid";
        });
        string v2 = MutateHeader(authority, header => header["schema"] = v2Schema);
        RunValidationCase(report, "v2-existing-lifecycle-valid", "valid-b0-domain-raw", v2);
        string changedId = MutateTick(v2, 2, tick =>
        {
            tick["slots"]!["occupants"]![0]!["allocationEpoch"] = 1;
            tick["lifecycleDelta"]!["events"]![0] =
                Event("object-id-change", 0, 1, 1, 2, 9);
        });
        RunValidationCase(report, "v2-object-id-change-valid", "valid-b0-domain-raw", changedId);
        RunValidationCase(report, "v1-object-id-change-still-rejected", "invalid",
            MutateHeader(changedId, header => header["schema"] = B0DomainRawContract.Schema));
        RunValidationCase(report, "v2-object-id-change-missing", "invalid",
            MutateTick(changedId, 2, tick => tick["lifecycleDelta"]!["events"]!.AsArray().RemoveAt(0)));
        RunValidationCase(report, "v2-object-id-change-wrong-kind", "invalid",
            MutateTick(changedId, 2, tick => tick["lifecycleDelta"]!["events"]![0]!["kind"] = "reuse"));
        RunValidationCase(report, "v2-object-id-change-wrong-epoch", "invalid",
            MutateTick(changedId, 2, tick => tick["lifecycleDelta"]!["events"]![0]!["currentAllocationEpoch"] = 2));
        RunValidationCase(report, "v2-object-id-change-wrong-id", "invalid",
            MutateTick(changedId, 2, tick => tick["lifecycleDelta"]!["events"]![0]!["previousObjectId"] = 77));
        RunValidationCase(report, "v2-object-id-change-extra-field", "invalid",
            MutateTick(changedId, 2, tick => tick["lifecycleDelta"]!["events"]![0]!["unexpected"] = 1));
        RunValidationCase(report, "v2-object-id-change-extra-event", "invalid",
            MutateTick(changedId, 2, tick => tick["lifecycleDelta"]!["events"]!.AsArray().Add(
                Event("object-id-change", 0, 1, 1, 2, 9))));
        RunValidationCase(report, "v2-lifecycle-order-rejected", "invalid",
            MutateTick(changedId, 2, tick =>
            {
                JsonArray events = tick["lifecycleDelta"]!["events"]!.AsArray();
                tick["lifecycleDelta"]!["events"] = new JsonArray(events[1]!.DeepClone(), events[0]!.DeepClone());
            }));
        RunValidationCase(report, "v2-unchanged-id-false-event-rejected", "invalid",
            MutateTick(changedId, 2, tick => tick["slots"]!["occupants"]![0]!["objectId"] = 2));
        RunValidationCase(report, "v2-reuse-mislabeled-as-id-change-rejected", "invalid",
            MutateTick(changedId, 2, tick => tick["slots"]!["occupants"]![0]!["allocationEpoch"] = 2));
        foreach (string producerCapture in new[] { authority, unity })
        {
            bool isUnity = producerCapture == unity;
            string diagnosticV2 = MutateTick(
                MutateHeader(producerCapture, header => header["schema"] = v2Schema), 1,
                tick => tick["aiAcceptedTrace"] = new JsonObject
                {
                    ["committed"] = 0, ["eligible"] = 0, ["fallback"] = 0,
                    ["firstFallbackReason"] = "None", ["oracleMismatch"] = 0,
                });
            RunValidationCase(report, isUnity ? "v2-unity-ai-diagnostic-valid" : "v2-authority-ai-diagnostic-rejected",
                isUnity ? "valid-b0-domain-raw" : "invalid", diagnosticV2);
        }
        RunValidationCase(report, "v2-zero-epoch-rejected", "invalid",
            MutateTick(changedId, 2, tick => tick["slots"]!["occupants"]![0]!["allocationEpoch"] = 0));
        RunValidationCase(report, "v2-decreased-epoch-rejected", "invalid",
            MutateHeader(changedId, header => header["initialOccupants"]![0]!["allocationEpoch"] = 2));

        report.Passed = report.Cases.All(test => test.Passed);
        return report;
    }

    private static bool ValidateDescriptor()
    {
        JsonObject descriptor = JsonSerializer.SerializeToNode(
            B0DomainRawContract.CreateDescriptor(),
            CompactOptions)!.AsObject();
        return string.Equals(
                   descriptor["schema"]!.GetValue<string>(),
                   B0DomainRawContract.DescriptorSchema,
                   StringComparison.Ordinal) &&
               descriptor["certificateEligible"]!.GetValue<bool>() == false &&
               descriptor["input"]!["validMask"]!.GetValue<int>() == 0x7F &&
               TraceContract.SequenceEqual(
                   descriptor["rng"]!["sourceNativeStreams"]!.AsArray(),
                   B0DomainRawContract.StreamNames);
    }

    private static void RunValidationCase(
        B0DomainRawSelfTestReport report,
        string name,
        string expectedStatus,
        string capture)
    {
        RunCase(report, name, expectedStatus, () =>
            B0DomainRawContract.ValidateTextForTest(capture).Status);
    }

    private static void RunCase(
        B0DomainRawSelfTestReport report,
        string name,
        string expectedStatus,
        Func<string> action)
    {
        string actualStatus;
        string? reason = null;
        try
        {
            actualStatus = action();
        }
        catch (Exception exception)
        {
            actualStatus = "exception";
            reason = exception.ToString();
        }

        report.Cases.Add(new B0DomainRawSelfTestCase
        {
            Name = name,
            ExpectedStatus = expectedStatus,
            ActualStatus = actualStatus,
            Reason = reason,
            Passed = string.Equals(
                expectedStatus,
                actualStatus,
                StringComparison.Ordinal),
        });
    }

    private static string BuildCapture(string producer)
    {
        JsonObject header = BuildHeader(producer, Occupants((0, 1UL, 2)));
        JsonObject first = BuildTick(
            producer,
            completedTick: 1,
            crtTotal: 3,
            synchronizedTotal: 6,
            unityTotal: 4,
            occupants: Occupants((0, 1UL, 2), (1, 1UL, 7)),
            events: Events(Birth(1, 1, 7)));
        JsonObject second = BuildTick(
            producer,
            completedTick: 2,
            crtTotal: 5,
            synchronizedTotal: 8,
            unityTotal: 7,
            occupants: Occupants((0, 2UL, 9)),
            events: Events(Reuse(0, 1, 2, 2, 9), Death(1, 1, 7)));
        return JoinLines(new[]
        {
            Serialize(header),
            Serialize(first),
            Serialize(second),
        });
    }

    private static JsonObject BuildHeader(string producer, JsonArray initialOccupants)
    {
        bool authority = producer == B0DomainRawContract.AuthorityProducer;
        return new JsonObject
        {
            ["kind"] = "header",
            ["schema"] = B0DomainRawContract.Schema,
            ["producer"] = producer,
            ["evidenceClass"] = authority
                ? "SOURCE_MODEL_DIAGNOSTIC_ONLY"
                : "UNITY_DIAGNOSTIC_ONLY",
            ["certificateEligible"] = false,
            ["formalExeSha256"] = TraceContract.AuthorityExecutableSha256,
            ["scenarioId"] = "b0-domain-self-test",
            ["firstCompletedTick"] = 1,
            ["expectedTickCount"] = 2,
            ["slotCapacity"] = authority ? 1000 : 1050,
            ["streamAvailability"] = new JsonObject
            {
                ["authorityCrt"] = authority
                    ? B0DomainRawContract.Available
                    : B0DomainRawContract.Missing,
                ["authoritySynchronized"] = authority
                    ? B0DomainRawContract.Available
                    : B0DomainRawContract.Missing,
                ["unityDeterministic"] = authority
                    ? B0DomainRawContract.Missing
                    : B0DomainRawContract.Available,
            },
            ["perCallTraceAvailability"] = new JsonObject
            {
                ["authorityCrt"] = B0DomainRawContract.Missing,
                ["authoritySynchronized"] = B0DomainRawContract.Missing,
                ["unityDeterministic"] = B0DomainRawContract.Missing,
            },
            ["initialRngTotalCalls"] = new JsonObject
            {
                ["authorityCrt"] = authority ? 1 : null,
                ["authoritySynchronized"] = authority ? 2 : null,
                ["unityDeterministic"] = authority ? null : 1,
            },
            ["initialOccupants"] = initialOccupants,
            ["lifecycleDeltaProvenance"] = B0DomainRawContract.SnapshotDerived,
        };
    }

    private static JsonObject BuildTick(
        string producer,
        long completedTick,
        ulong crtTotal,
        ulong synchronizedTotal,
        ulong unityTotal,
        JsonArray occupants,
        JsonArray events)
    {
        bool authority = producer == B0DomainRawContract.AuthorityProducer;
        ulong previousCrt = completedTick == 1 ? 1UL : 3UL;
        ulong previousSynchronized = completedTick == 1 ? 2UL : 6UL;
        ulong previousUnity = completedTick == 1 ? 1UL : 4UL;
        return new JsonObject
        {
            ["kind"] = "tick",
            ["completedTick"] = completedTick,
            ["input"] = new JsonObject
            {
                ["captureBoundary"] = B0DomainRawContract.AppliedToTick,
                ["players"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["playerSlot"] = 0,
                        ["heldMask"] = completedTick == 1 ? 0x11 : 0x02,
                    },
                },
            },
            ["rng"] = new JsonObject
            {
                ["authorityCrt"] = RngStream(
                    authority,
                    JsonValue.Create((ulong)(100 + completedTick)),
                    crtTotal,
                    crtTotal - previousCrt),
                ["authoritySynchronized"] = RngStream(
                    authority,
                    new JsonObject
                    {
                        ["counter"] = (int)completedTick,
                        ["index"] = (int)completedTick + 10,
                        ["tableHash64"] = new string('A', 16),
                        ["lastCallSite"] = 42,
                    },
                    synchronizedTotal,
                    synchronizedTotal - previousSynchronized),
                ["unityDeterministic"] = RngStream(
                    !authority,
                    JsonValue.Create((ulong)(200 + completedTick)),
                    unityTotal,
                    unityTotal - previousUnity),
            },
            ["slots"] = new JsonObject
            {
                ["capacity"] = authority ? 1000 : 1050,
                ["occupants"] = occupants,
            },
            ["lifecycleDelta"] = new JsonObject
            {
                ["provenance"] = B0DomainRawContract.SnapshotDerived,
                ["events"] = events,
            },
        };
    }

    private static JsonObject RngStream(
        bool available,
        JsonNode state,
        ulong totalCalls,
        ulong tickCallCount)
    {
        return new JsonObject
        {
            ["availability"] = available
                ? B0DomainRawContract.Available
                : B0DomainRawContract.Missing,
            ["state"] = available ? state : null,
            ["totalCalls"] = available ? totalCalls : null,
            ["tickCallCount"] = available ? tickCallCount : null,
            ["calls"] = null,
        };
    }

    private static JsonArray Occupants(params (int Slot, ulong Epoch, int Oid)[] values)
    {
        var result = new JsonArray();
        foreach ((int slot, ulong epoch, int oid) in values)
        {
            result.Add(new JsonObject
            {
                ["slot"] = slot,
                ["allocationEpoch"] = epoch,
                ["objectId"] = oid,
            });
        }

        return result;
    }

    private static JsonArray Events(params JsonObject[] values)
    {
        var result = new JsonArray();
        foreach (JsonObject value in values)
        {
            result.Add(value);
        }

        return result;
    }

    private static JsonObject Birth(int slot, ulong epoch, int oid)
    {
        return Event("birth", slot, null, epoch, null, oid);
    }

    private static JsonObject Death(int slot, ulong epoch, int oid)
    {
        return Event("death", slot, epoch, null, oid, null);
    }

    private static JsonObject Reuse(
        int slot,
        ulong previousEpoch,
        ulong currentEpoch,
        int previousOid,
        int currentOid)
    {
        return Event(
            "reuse", slot, previousEpoch, currentEpoch, previousOid, currentOid);
    }

    private static JsonObject Event(
        string kind,
        int slot,
        ulong? previousEpoch,
        ulong? currentEpoch,
        int? previousOid,
        int? currentOid)
    {
        return new JsonObject
        {
            ["kind"] = kind,
            ["slot"] = slot,
            ["previousAllocationEpoch"] = previousEpoch,
            ["currentAllocationEpoch"] = currentEpoch,
            ["previousObjectId"] = previousOid,
            ["currentObjectId"] = currentOid,
        };
    }

    private static string MutateHeader(string source, Action<JsonObject> mutation)
    {
        string[] lines = SplitLines(source);
        JsonObject header = JsonNode.Parse(lines[0])!.AsObject();
        mutation(header);
        lines[0] = Serialize(header);
        return JoinLines(lines);
    }

    private static string MutateTick(
        string source,
        int completedTick,
        Action<JsonObject> mutation)
    {
        string[] lines = SplitLines(source);
        JsonObject tick = JsonNode.Parse(lines[completedTick])!.AsObject();
        mutation(tick);
        lines[completedTick] = Serialize(tick);
        return JoinLines(lines);
    }

    private static string Serialize(JsonNode value)
    {
        return TraceContract.Canonicalize(value).ToJsonString(CompactOptions);
    }

    private static string[] SplitLines(string source)
    {
        return source.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static string JoinLines(IEnumerable<string> lines)
    {
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}

internal sealed class B0DomainRawSelfTestReport
{
    public string Schema { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public List<B0DomainRawSelfTestCase> Cases { get; set; } = [];
}

internal sealed class B0DomainRawSelfTestCase
{
    public string Name { get; set; } = string.Empty;
    public string ExpectedStatus { get; set; } = string.Empty;
    public string ActualStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool Passed { get; set; }
}
