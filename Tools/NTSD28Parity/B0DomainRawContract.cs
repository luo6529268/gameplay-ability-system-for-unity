using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace NTSD28Parity;

internal static class B0DomainRawContract
{
    internal const string Schema = "ntsd28-logan-b0-domain-raw-v1";
    internal const string DescriptorSchema = "ntsd28-logan-b0-domain-raw-contract-v1";
    internal const string ValidationSchema = "ntsd28-logan-b0-domain-raw-validation-v1";
    internal const string AuthorityProducer = "authority-source-model";
    internal const string UnityProducer = "unity-diagnostic";
    internal const string Available = "available";
    internal const string Missing = "missing";
    internal const string SnapshotDerived = "snapshot-derived";
    internal const string AppliedToTick = "applied-to-tick";
    internal const int ValidInputMask = 0x7F;

    internal static readonly string[] StreamNames =
    [
        "authorityCrt",
        "authoritySynchronized",
        "unityDeterministic",
    ];

    private static readonly string[] HeaderProperties =
    [
        "kind", "schema", "producer", "evidenceClass", "certificateEligible",
        "formalExeSha256", "scenarioId", "firstCompletedTick", "expectedTickCount",
        "slotCapacity", "streamAvailability", "perCallTraceAvailability",
        "initialRngTotalCalls", "initialOccupants", "lifecycleDeltaProvenance",
    ];

    private static readonly string[] TickProperties =
    [
        "kind", "completedTick", "input", "rng", "slots", "lifecycleDelta",
    ];

    private static readonly string[] InputProperties =
    [
        "captureBoundary", "players",
    ];

    private static readonly string[] PlayerProperties =
    [
        "playerSlot", "heldMask",
    ];

    private static readonly string[] RngStreamProperties =
    [
        "availability", "state", "totalCalls", "tickCallCount", "calls",
    ];

    private static readonly string[] SynchronizedStateProperties =
    [
        "counter", "index", "tableHash64", "lastCallSite",
    ];

    private static readonly Regex Hash64Pattern = new(
        "^[0-9A-Fa-f]{16}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly string[] SlotsProperties =
    [
        "capacity", "occupants",
    ];

    private static readonly string[] OccupantProperties =
    [
        "slot", "allocationEpoch", "objectId",
    ];

    private static readonly string[] LifecycleDeltaProperties =
    [
        "provenance", "events",
    ];

    private static readonly string[] LifecycleEventProperties =
    [
        "kind", "slot", "previousAllocationEpoch", "currentAllocationEpoch",
        "previousObjectId", "currentObjectId",
    ];

    internal static object CreateDescriptor()
    {
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["schema"] = DescriptorSchema,
            ["rawSchema"] = Schema,
            ["completedTickBoundary"] =
                "state after one simulation step; host/render frames excluded",
            ["input"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["captureBoundary"] = AppliedToTick,
                ["heldMaskBits"] = new[]
                {
                    "right", "left", "up", "down", "attack", "jump", "defend",
                },
                ["validMask"] = ValidInputMask,
            },
            ["rng"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["sourceNativeStreams"] = StreamNames,
                ["authorityAvailability"] = new[] { Available, Available, Missing },
                ["unityAvailability"] = new[] { Missing, Missing, Available },
                ["perCallTraceAvailability"] = new[] { Missing, Missing, Missing },
                ["mappingPolicy"] =
                    "no cross-stream equivalence is asserted by this B0 contract",
            },
            ["slots"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["capacityEqualityRequired"] = false,
                ["occupantOrder"] = "strictly ascending physical/runtime slot",
                ["allocationEpoch"] = "positive per-slot lifetime epoch",
            },
            ["lifecycleDelta"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["provenance"] = SnapshotDerived,
                ["eventKinds"] = new[] { "birth", "death", "reuse" },
                ["initialSnapshotRequired"] = true,
            },
            ["certificateEligible"] = false,
        };
    }

    internal static B0DomainRawValidationReport ValidateFile(string path)
    {
        return ValidateLines(File.ReadLines(path));
    }

    internal static B0DomainRawValidationReport ValidateTextForTest(string text)
    {
        return ValidateLines(text.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries));
    }

    private static B0DomainRawValidationReport ValidateLines(IEnumerable<string> lines)
    {
        var report = new B0DomainRawValidationReport
        {
            Schema = ValidationSchema,
        };

        try
        {
            using IEnumerator<string> enumerator = lines.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new InvalidDataException("empty-capture");
            }

            JsonObject header = ParseObject(enumerator.Current, "header");
            HeaderContext context = ValidateHeader(header);
            SortedDictionary<int, Occupant> previous = context.InitialOccupants;
            Dictionary<string, ulong?> previousTotals = context.InitialRngTotals;

            int tickCount = 0;
            while (enumerator.MoveNext())
            {
                JsonObject tick = ParseObject(enumerator.Current, "tick");
                long expectedTick = context.FirstCompletedTick + tickCount;
                (SortedDictionary<int, Occupant> occupants,
                    Dictionary<string, ulong?> totals) = ValidateTick(
                    tick,
                    expectedTick,
                    context,
                    previous,
                    previousTotals);
                previous = occupants;
                previousTotals = totals;
                tickCount++;
            }

            if (tickCount != context.ExpectedTickCount)
            {
                throw new InvalidDataException("tick-count-mismatch");
            }

            report.Valid = true;
            report.Status = "valid-b0-domain-raw";
            report.Producer = context.Producer;
            report.ValidatedTicks = tickCount;
            report.CertificateEligible = false;
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidDataException or
            InvalidOperationException or FormatException or OverflowException)
        {
            report.Valid = false;
            report.Status = "invalid";
            report.Reason = exception.Message;
            report.CertificateEligible = false;
        }

        return report;
    }

    private static HeaderContext ValidateHeader(JsonObject header)
    {
        RequireExactProperties(header, HeaderProperties, "header-properties");
        RequireString(header, "kind", "header");
        RequireString(header, "schema", Schema);
        string producer = RequireString(header, "producer");
        if (producer is not AuthorityProducer and not UnityProducer)
        {
            throw new InvalidDataException("unsupported-producer");
        }

        RequireString(
            header,
            "evidenceClass",
            producer == AuthorityProducer
                ? "SOURCE_MODEL_DIAGNOSTIC_ONLY"
                : "UNITY_DIAGNOSTIC_ONLY");
        if (RequireBoolean(header, "certificateEligible"))
        {
            throw new InvalidDataException("certificate-must-be-false");
        }

        RequireString(
            header,
            "formalExeSha256",
            TraceContract.AuthorityExecutableSha256);
        string scenarioId = RequireString(header, "scenarioId");
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            throw new InvalidDataException("scenario-id-empty");
        }

        long firstCompletedTick = RequireNonNegativeInt64(header, "firstCompletedTick");
        int expectedTickCount = RequirePositiveInt32(header, "expectedTickCount");
        int slotCapacity = RequirePositiveInt32(header, "slotCapacity");
        JsonObject availability = RequireObject(header, "streamAvailability");
        JsonObject perCall = RequireObject(header, "perCallTraceAvailability");
        JsonObject initialTotalsNode = RequireObject(header, "initialRngTotalCalls");
        RequireStreamKeySet(availability, "stream-availability-properties");
        RequireStreamKeySet(perCall, "per-call-availability-properties");
        RequireStreamKeySet(initialTotalsNode, "initial-rng-total-properties");

        var streamAvailability = new Dictionary<string, string>(StringComparer.Ordinal);
        var initialTotals = new Dictionary<string, ulong?>(StringComparer.Ordinal);
        foreach (string stream in StreamNames)
        {
            string expectedAvailability = ExpectedAvailability(producer, stream);
            string actualAvailability = RequireString(availability, stream);
            if (!string.Equals(
                    expectedAvailability,
                    actualAvailability,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException($"stream-availability-mismatch:{stream}");
            }

            RequireString(perCall, stream, Missing);
            streamAvailability.Add(stream, actualAvailability);
            if (actualAvailability == Available)
            {
                initialTotals.Add(
                    stream,
                    RequireNonNegativeUInt64(initialTotalsNode, stream));
            }
            else
            {
                RequireNull(initialTotalsNode, stream);
                initialTotals.Add(stream, null);
            }
        }

        SortedDictionary<int, Occupant> initialOccupants = ValidateOccupants(
            RequireArray(header, "initialOccupants"),
            slotCapacity);
        RequireString(header, "lifecycleDeltaProvenance", SnapshotDerived);
        return new HeaderContext(
            producer,
            firstCompletedTick,
            expectedTickCount,
            slotCapacity,
            streamAvailability,
            initialTotals,
            initialOccupants);
    }

    private static (SortedDictionary<int, Occupant> Occupants,
        Dictionary<string, ulong?> Totals) ValidateTick(
        JsonObject tick,
        long expectedTick,
        HeaderContext context,
        SortedDictionary<int, Occupant> previousOccupants,
        Dictionary<string, ulong?> previousTotals)
    {
        RequireExactProperties(tick, TickProperties, "tick-properties");
        RequireString(tick, "kind", "tick");
        if (RequireNonNegativeInt64(tick, "completedTick") != expectedTick)
        {
            throw new InvalidDataException("completed-tick-sequence-mismatch");
        }

        ValidateInput(RequireObject(tick, "input"), context.SlotCapacity);
        Dictionary<string, ulong?> totals = ValidateRng(
            RequireObject(tick, "rng"),
            context,
            previousTotals);
        JsonObject slots = RequireObject(tick, "slots");
        RequireExactProperties(slots, SlotsProperties, "slots-properties");
        if (RequirePositiveInt32(slots, "capacity") != context.SlotCapacity)
        {
            throw new InvalidDataException("tick-slot-capacity-mismatch");
        }

        SortedDictionary<int, Occupant> occupants = ValidateOccupants(
            RequireArray(slots, "occupants"),
            context.SlotCapacity);
        ValidateLifecycleDelta(
            RequireObject(tick, "lifecycleDelta"),
            previousOccupants,
            occupants);
        return (occupants, totals);
    }

    private static void ValidateInput(JsonObject input, int slotCapacity)
    {
        RequireExactProperties(input, InputProperties, "input-properties");
        RequireString(input, "captureBoundary", AppliedToTick);
        JsonArray players = RequireArray(input, "players");
        int previousSlot = -1;
        foreach (JsonNode? node in players)
        {
            JsonObject player = RequireObject(node, "player");
            RequireExactProperties(player, PlayerProperties, "player-properties");
            int playerSlot = RequireNonNegativeInt32(player, "playerSlot");
            if (playerSlot >= slotCapacity || playerSlot <= previousSlot)
            {
                throw new InvalidDataException("player-slot-order-or-range");
            }

            int heldMask = RequireNonNegativeInt32(player, "heldMask");
            if ((heldMask & ~ValidInputMask) != 0)
            {
                throw new InvalidDataException("held-mask-outside-seven-actions");
            }

            previousSlot = playerSlot;
        }
    }

    private static Dictionary<string, ulong?> ValidateRng(
        JsonObject rng,
        HeaderContext context,
        Dictionary<string, ulong?> previousTotals)
    {
        RequireStreamKeySet(rng, "rng-properties");
        var totals = new Dictionary<string, ulong?>(StringComparer.Ordinal);
        foreach (string stream in StreamNames)
        {
            JsonObject value = RequireObject(rng, stream);
            RequireExactProperties(
                value,
                RngStreamProperties,
                $"rng-stream-properties:{stream}");
            string availability = RequireString(value, "availability");
            if (!string.Equals(
                    availability,
                    context.StreamAvailability[stream],
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"tick-stream-availability-mismatch:{stream}");
            }

            if (availability == Missing)
            {
                RequireNull(value, "state");
                RequireNull(value, "totalCalls");
                RequireNull(value, "tickCallCount");
                RequireNull(value, "calls");
                totals.Add(stream, null);
                continue;
            }

            ValidateRngState(stream, value["state"]);
            ulong totalCalls = RequireNonNegativeUInt64(value, "totalCalls");
            ulong tickCallCount = RequireNonNegativeUInt64(value, "tickCallCount");
            RequireNull(value, "calls");
            ulong previous = previousTotals[stream] ??
                throw new InvalidDataException($"missing-previous-total:{stream}");
            if (totalCalls < previous || totalCalls - previous != tickCallCount)
            {
                throw new InvalidDataException($"rng-call-delta-mismatch:{stream}");
            }

            totals.Add(stream, totalCalls);
        }

        return totals;
    }

    private static void ValidateRngState(string stream, JsonNode? node)
    {
        if (stream is "authorityCrt" or "unityDeterministic")
        {
            _ = RequireNonNegativeUInt64(node, $"rng-state:{stream}");
            return;
        }

        JsonObject state = RequireObject(node, "synchronized-state");
        RequireExactProperties(
            state,
            SynchronizedStateProperties,
            "synchronized-state-properties");
        int counter = RequireNonNegativeInt32(state, "counter");
        int index = RequireNonNegativeInt32(state, "index");
        if (counter >= 1234 || index >= 3000)
        {
            throw new InvalidDataException("synchronized-state-range");
        }

        if (!Hash64Pattern.IsMatch(RequireString(state, "tableHash64")))
        {
            throw new InvalidDataException("synchronized-table-hash64-invalid");
        }

        _ = RequireNonNegativeUInt64(state, "lastCallSite");
    }

    private static SortedDictionary<int, Occupant> ValidateOccupants(
        JsonArray array,
        int slotCapacity)
    {
        var result = new SortedDictionary<int, Occupant>();
        int previousSlot = -1;
        foreach (JsonNode? node in array)
        {
            JsonObject occupant = RequireObject(node, "occupant");
            RequireExactProperties(
                occupant,
                OccupantProperties,
                "occupant-properties");
            int slot = RequireNonNegativeInt32(occupant, "slot");
            if (slot >= slotCapacity || slot <= previousSlot)
            {
                throw new InvalidDataException("occupant-slot-order-or-range");
            }

            ulong allocationEpoch = RequireNonNegativeUInt64(
                occupant,
                "allocationEpoch");
            if (allocationEpoch == 0)
            {
                throw new InvalidDataException("allocation-epoch-must-be-positive");
            }

            int objectId = RequireNonNegativeInt32(occupant, "objectId");
            result.Add(slot, new Occupant(slot, allocationEpoch, objectId));
            previousSlot = slot;
        }

        return result;
    }

    private static void ValidateLifecycleDelta(
        JsonObject lifecycleDelta,
        SortedDictionary<int, Occupant> previous,
        SortedDictionary<int, Occupant> current)
    {
        RequireExactProperties(
            lifecycleDelta,
            LifecycleDeltaProperties,
            "lifecycle-delta-properties");
        RequireString(lifecycleDelta, "provenance", SnapshotDerived);
        JsonArray events = RequireArray(lifecycleDelta, "events");
        List<LifecycleEvent> expected = DeriveEvents(previous, current);
        if (events.Count != expected.Count)
        {
            throw new InvalidDataException("lifecycle-event-count-mismatch");
        }

        for (int index = 0; index < expected.Count; index++)
        {
            JsonObject actual = RequireObject(events[index], "lifecycle-event");
            RequireExactProperties(
                actual,
                LifecycleEventProperties,
                "lifecycle-event-properties");
            LifecycleEvent expectedEvent = expected[index];
            RequireString(actual, "kind", expectedEvent.Kind);
            if (RequireNonNegativeInt32(actual, "slot") != expectedEvent.Slot)
            {
                throw new InvalidDataException("lifecycle-event-slot-mismatch");
            }

            RequireNullableUInt64(
                actual,
                "previousAllocationEpoch",
                expectedEvent.PreviousAllocationEpoch);
            RequireNullableUInt64(
                actual,
                "currentAllocationEpoch",
                expectedEvent.CurrentAllocationEpoch);
            RequireNullableInt32(actual, "previousObjectId", expectedEvent.PreviousObjectId);
            RequireNullableInt32(actual, "currentObjectId", expectedEvent.CurrentObjectId);
        }
    }

    private static List<LifecycleEvent> DeriveEvents(
        SortedDictionary<int, Occupant> previous,
        SortedDictionary<int, Occupant> current)
    {
        var slots = new SortedSet<int>(previous.Keys);
        slots.UnionWith(current.Keys);
        var result = new List<LifecycleEvent>();
        foreach (int slot in slots)
        {
            bool hadPrevious = previous.TryGetValue(slot, out Occupant oldValue);
            bool hasCurrent = current.TryGetValue(slot, out Occupant newValue);
            if (!hadPrevious)
            {
                result.Add(new LifecycleEvent(
                    "birth", slot, null, newValue.AllocationEpoch, null, newValue.ObjectId));
                continue;
            }

            if (!hasCurrent)
            {
                result.Add(new LifecycleEvent(
                    "death", slot, oldValue.AllocationEpoch, null, oldValue.ObjectId, null));
                continue;
            }

            if (oldValue.AllocationEpoch == newValue.AllocationEpoch)
            {
                if (oldValue.ObjectId != newValue.ObjectId)
                {
                    throw new InvalidDataException(
                        "occupant-changed-without-allocation-epoch");
                }

                continue;
            }

            if (newValue.AllocationEpoch <= oldValue.AllocationEpoch)
            {
                throw new InvalidDataException("allocation-epoch-not-monotonic");
            }

            result.Add(new LifecycleEvent(
                "reuse",
                slot,
                oldValue.AllocationEpoch,
                newValue.AllocationEpoch,
                oldValue.ObjectId,
                newValue.ObjectId));
        }

        return result;
    }

    private static string ExpectedAvailability(string producer, string stream)
    {
        return producer switch
        {
            AuthorityProducer when stream is "authorityCrt" or
                "authoritySynchronized" => Available,
            UnityProducer when stream == "unityDeterministic" => Available,
            _ => Missing,
        };
    }

    private static JsonObject ParseObject(string json, string context)
    {
        return RequireObject(JsonNode.Parse(json), context);
    }

    private static void RequireStreamKeySet(JsonObject value, string reason)
    {
        RequireExactProperties(value, StreamNames, reason);
    }

    private static void RequireExactProperties(
        JsonObject value,
        IReadOnlyCollection<string> expected,
        string reason)
    {
        if (!TraceContract.HasExactProperties(value, expected))
        {
            throw new InvalidDataException(reason);
        }
    }

    private static JsonObject RequireObject(JsonObject parent, string property)
    {
        return RequireObject(parent[property], property);
    }

    private static JsonObject RequireObject(JsonNode? node, string context)
    {
        return node as JsonObject ??
            throw new InvalidDataException($"expected-object:{context}");
    }

    private static JsonArray RequireArray(JsonObject parent, string property)
    {
        return parent[property] as JsonArray ??
            throw new InvalidDataException($"expected-array:{property}");
    }

    private static string RequireString(JsonObject parent, string property)
    {
        if (parent[property] is not JsonValue value ||
            !value.TryGetValue(out string? result) || result is null)
        {
            throw new InvalidDataException($"expected-string:{property}");
        }

        return result;
    }

    private static void RequireString(
        JsonObject parent,
        string property,
        string expected)
    {
        if (!string.Equals(
                RequireString(parent, property),
                expected,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException($"string-mismatch:{property}");
        }
    }

    private static bool RequireBoolean(JsonObject parent, string property)
    {
        if (parent[property] is not JsonValue value ||
            !value.TryGetValue(out bool result))
        {
            throw new InvalidDataException($"expected-boolean:{property}");
        }

        return result;
    }

    private static int RequirePositiveInt32(JsonObject parent, string property)
    {
        int result = RequireNonNegativeInt32(parent, property);
        if (result == 0)
        {
            throw new InvalidDataException($"expected-positive-int32:{property}");
        }

        return result;
    }

    private static int RequireNonNegativeInt32(JsonObject parent, string property)
    {
        long result = RequireNonNegativeInt64(parent, property);
        if (result > int.MaxValue)
        {
            throw new InvalidDataException($"int32-overflow:{property}");
        }

        return (int)result;
    }

    private static long RequireNonNegativeInt64(JsonObject parent, string property)
    {
        if (parent[property] is not JsonValue value ||
            !value.TryGetValue(out long result) || result < 0)
        {
            throw new InvalidDataException($"expected-nonnegative-int64:{property}");
        }

        return result;
    }

    private static ulong RequireNonNegativeUInt64(JsonObject parent, string property)
    {
        return RequireNonNegativeUInt64(parent[property], property);
    }

    private static ulong RequireNonNegativeUInt64(JsonNode? node, string context)
    {
        if (node is not JsonValue value || !value.TryGetValue(out ulong result))
        {
            throw new InvalidDataException($"expected-nonnegative-uint64:{context}");
        }

        return result;
    }

    private static void RequireNull(JsonObject parent, string property)
    {
        if (parent[property] is not null)
        {
            throw new InvalidDataException($"expected-null:{property}");
        }
    }

    private static void RequireNullableUInt64(
        JsonObject parent,
        string property,
        ulong? expected)
    {
        if (!expected.HasValue)
        {
            RequireNull(parent, property);
            return;
        }

        if (RequireNonNegativeUInt64(parent, property) != expected.Value)
        {
            throw new InvalidDataException($"uint64-mismatch:{property}");
        }
    }

    private static void RequireNullableInt32(
        JsonObject parent,
        string property,
        int? expected)
    {
        if (!expected.HasValue)
        {
            RequireNull(parent, property);
            return;
        }

        if (RequireNonNegativeInt32(parent, property) != expected.Value)
        {
            throw new InvalidDataException($"int32-mismatch:{property}");
        }
    }

    private sealed record HeaderContext(
        string Producer,
        long FirstCompletedTick,
        int ExpectedTickCount,
        int SlotCapacity,
        Dictionary<string, string> StreamAvailability,
        Dictionary<string, ulong?> InitialRngTotals,
        SortedDictionary<int, Occupant> InitialOccupants);

    private readonly record struct Occupant(int Slot, ulong AllocationEpoch, int ObjectId);

    private readonly record struct LifecycleEvent(
        string Kind,
        int Slot,
        ulong? PreviousAllocationEpoch,
        ulong? CurrentAllocationEpoch,
        int? PreviousObjectId,
        int? CurrentObjectId);
}

internal sealed class B0DomainRawValidationReport
{
    public string Schema { get; set; } = string.Empty;
    public bool Valid { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Producer { get; set; }
    public int ValidatedTicks { get; set; }
    public bool CertificateEligible { get; set; }
}
