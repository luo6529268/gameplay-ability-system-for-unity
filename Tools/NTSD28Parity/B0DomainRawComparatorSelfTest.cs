using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class B0DomainRawComparatorSelfTest
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
    };

    internal static B0DomainComparatorSelfTestReport Run()
    {
        var report = new B0DomainComparatorSelfTestReport
        {
            Schema = "ntsd28-logan-b0-domain-comparator-self-test-v1",
        };

        string authority = BuildCapture(
            B0DomainRawContract.AuthorityProducer,
            heldMask: 17,
            objectId: 2,
            removeAtTick: false);
        string unity = BuildCapture(
            B0DomainRawContract.UnityProducer,
            heldMask: 17,
            objectId: 2,
            removeAtTick: false);
        RunCase(report, "shared-domains-equal", true, () =>
        {
            B0DomainRawComparisonReport comparison =
                B0DomainRawComparator.CompareTextForTest(authority, unity);
            return comparison.Status ==
                       "shared-domains-equal-rng-topology-different" &&
                   comparison.InputEqual &&
                   !comparison.RngTopologyEqual &&
                   comparison.SlotOccupantsEqual &&
                   comparison.LifecycleEqual &&
                   comparison.SlotCapacityExceptionApplied &&
                   comparison.FirstDifference?.Classification ==
                       "STREAM_TOPOLOGY_DIFFERENCE";
        });

        RunCase(report, "input-difference", true, () =>
        {
            string changed = BuildCapture(
                B0DomainRawContract.UnityProducer,
                heldMask: 1,
                objectId: 2,
                removeAtTick: false);
            B0DomainRawComparisonReport comparison =
                B0DomainRawComparator.CompareTextForTest(authority, changed);
            return !comparison.InputEqual &&
                   comparison.FirstDifference?.Domain == "input" &&
                   comparison.FirstDifference.CompletedTick == 1;
        });

        RunCase(report, "slot-difference-not-hidden-by-capacity", true, () =>
        {
            string changed = BuildCapture(
                B0DomainRawContract.UnityProducer,
                heldMask: 17,
                objectId: 7,
                removeAtTick: false);
            B0DomainRawComparisonReport comparison =
                B0DomainRawComparator.CompareTextForTest(authority, changed);
            return !comparison.SlotOccupantsEqual &&
                   comparison.Differences.Any(difference =>
                       difference.Path == "slots.occupants");
        });

        RunCase(report, "lifecycle-difference", true, () =>
        {
            string removed = BuildCapture(
                B0DomainRawContract.AuthorityProducer,
                heldMask: 17,
                objectId: 2,
                removeAtTick: true);
            B0DomainRawComparisonReport comparison =
                B0DomainRawComparator.CompareTextForTest(removed, unity);
            return !comparison.LifecycleEqual &&
                   comparison.Differences.Any(difference =>
                       difference.Path == "lifecycleDelta");
        });

        RunCase(report, "invalid-capture-rejected", true, () =>
        {
            string invalid = Serialize(BuildHeader(
                B0DomainRawContract.AuthorityProducer,
                objectId: 2)) + Environment.NewLine;
            try
            {
                _ = B0DomainRawComparator.CompareTextForTest(invalid, unity);
                return false;
            }
            catch (InvalidDataException)
            {
                return true;
            }
        });

        RunCase(report, "producer-order-rejected", true, () =>
        {
            try
            {
                _ = B0DomainRawComparator.CompareTextForTest(unity, authority);
                return false;
            }
            catch (InvalidDataException)
            {
                return true;
            }
        });

        report.Passed = report.Cases.All(test => test.Passed);
        return report;
    }

    private static void RunCase(
        B0DomainComparatorSelfTestReport report,
        string name,
        bool expected,
        Func<bool> action)
    {
        bool actual = false;
        string? reason = null;
        try
        {
            actual = action();
        }
        catch (Exception exception)
        {
            reason = exception.ToString();
        }

        report.Cases.Add(new B0DomainComparatorSelfTestCase
        {
            Name = name,
            Expected = expected,
            Actual = actual,
            Reason = reason,
            Passed = expected == actual,
        });
    }

    private static string BuildCapture(
        string producer,
        int heldMask,
        int objectId,
        bool removeAtTick)
    {
        JsonObject header = BuildHeader(producer, objectId);
        JsonObject tick = BuildTick(
            producer,
            heldMask,
            objectId,
            removeAtTick);
        return Serialize(header) + Environment.NewLine +
               Serialize(tick) + Environment.NewLine;
    }

    private static JsonObject BuildHeader(string producer, int objectId)
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
            ["scenarioId"] = "domain-comparator-self-test",
            ["firstCompletedTick"] = 1,
            ["expectedTickCount"] = 1,
            ["slotCapacity"] = authority ? 1000 : 400,
            ["streamAvailability"] = new JsonObject
            {
                ["authorityCrt"] = authority ? "available" : "missing",
                ["authoritySynchronized"] = authority ? "available" : "missing",
                ["unityDeterministic"] = authority ? "missing" : "available",
            },
            ["perCallTraceAvailability"] = new JsonObject
            {
                ["authorityCrt"] = "missing",
                ["authoritySynchronized"] = "missing",
                ["unityDeterministic"] = "missing",
            },
            ["initialRngTotalCalls"] = new JsonObject
            {
                ["authorityCrt"] = authority ? 0 : null,
                ["authoritySynchronized"] = authority ? 0 : null,
                ["unityDeterministic"] = authority ? null : 0,
            },
            ["initialOccupants"] = new JsonArray
            {
                Occupant(objectId),
            },
            ["lifecycleDeltaProvenance"] = "snapshot-derived",
        };
    }

    private static JsonObject BuildTick(
        string producer,
        int heldMask,
        int objectId,
        bool removeAtTick)
    {
        bool authority = producer == B0DomainRawContract.AuthorityProducer;
        JsonArray occupants = removeAtTick
            ? new JsonArray()
            : new JsonArray { Occupant(objectId) };
        JsonArray events = removeAtTick
            ? new JsonArray
            {
                new JsonObject
                {
                    ["kind"] = "death",
                    ["slot"] = 0,
                    ["previousAllocationEpoch"] = 1,
                    ["currentAllocationEpoch"] = null,
                    ["previousObjectId"] = objectId,
                    ["currentObjectId"] = null,
                },
            }
            : new JsonArray();
        return new JsonObject
        {
            ["kind"] = "tick",
            ["completedTick"] = 1,
            ["input"] = new JsonObject
            {
                ["captureBoundary"] = "applied-to-tick",
                ["players"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["playerSlot"] = 0,
                        ["heldMask"] = heldMask,
                    },
                },
            },
            ["rng"] = new JsonObject
            {
                ["authorityCrt"] = RngStream(
                    authority,
                    JsonValue.Create(1UL),
                    totalCalls: 0,
                    tickCallCount: 0),
                ["authoritySynchronized"] = RngStream(
                    authority,
                    new JsonObject
                    {
                        ["counter"] = 0,
                        ["index"] = 0,
                        ["tableHash64"] = "AAAAAAAAAAAAAAAA",
                        ["lastCallSite"] = 0,
                    },
                    totalCalls: 0,
                    tickCallCount: 0),
                ["unityDeterministic"] = RngStream(
                    !authority,
                    JsonValue.Create(2UL),
                    totalCalls: 1,
                    tickCallCount: 1),
            },
            ["slots"] = new JsonObject
            {
                ["capacity"] = authority ? 1000 : 400,
                ["occupants"] = occupants,
            },
            ["lifecycleDelta"] = new JsonObject
            {
                ["provenance"] = "snapshot-derived",
                ["events"] = events,
            },
        };
    }

    private static JsonObject Occupant(int objectId)
    {
        return new JsonObject
        {
            ["slot"] = 0,
            ["allocationEpoch"] = 1,
            ["objectId"] = objectId,
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
            ["availability"] = available ? "available" : "missing",
            ["state"] = available ? state : null,
            ["totalCalls"] = available ? totalCalls : null,
            ["tickCallCount"] = available ? tickCallCount : null,
            ["calls"] = null,
        };
    }

    private static string Serialize(JsonNode value)
    {
        return TraceContract.Canonicalize(value).ToJsonString(CompactOptions);
    }
}

internal sealed class B0DomainComparatorSelfTestReport
{
    public string Schema { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public List<B0DomainComparatorSelfTestCase> Cases { get; set; } = [];
}

internal sealed class B0DomainComparatorSelfTestCase
{
    public string Name { get; set; } = string.Empty;
    public bool Expected { get; set; }
    public bool Actual { get; set; }
    public string? Reason { get; set; }
    public bool Passed { get; set; }
}
