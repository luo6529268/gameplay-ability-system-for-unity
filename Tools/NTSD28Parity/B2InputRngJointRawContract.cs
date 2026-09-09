using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace NTSD28Parity;

internal static class B2InputRngJointRawContract
{
    internal const string Schema =
        "ntsd28-logan-b2-input-rng-joint-raw-v3";
    internal const string AuthorityProducer = "authority-source-model";
    internal const string UnityProducer = "unity-diagnostic";
    internal const string FormalExeSha256 =
        "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
    internal static readonly string[] KeyOrder =
        { "W", "S", "A", "D", "J", "K", "L" };
    internal static readonly string[] EdgeNames =
        { "attack", "jump", "defend", "right", "left", "up", "down" };

    private static readonly Regex Hash64Pattern = new(
        "^[0-9A-F]{16}$",
        RegexOptions.CultureInvariant);

    internal static B2InputRngJointRawValidationReport ValidateFile(
        string path)
    {
        try
        {
            return ValidateCapture(ParseLines(File.ReadLines(path)));
        }
        catch (Exception exception)
        {
            return Invalid(exception.Message);
        }
    }

    internal static B2InputRngJointRawValidationReport ValidateTextForTest(
        string text)
    {
        try
        {
            return ValidateCapture(ParseLines(SplitLines(text)));
        }
        catch (Exception exception)
        {
            return Invalid(exception.Message);
        }
    }

    internal static B2InputRngJointRawCapture LoadFile(string path)
    {
        return ParseLines(File.ReadLines(path));
    }

    internal static B2InputRngJointRawCapture LoadTextForTest(string text)
    {
        return ParseLines(SplitLines(text));
    }

    private static B2InputRngJointRawValidationReport ValidateCapture(
        B2InputRngJointRawCapture capture)
    {
        JsonObject header = capture.Header;
        RequireString(header, "kind", "header");
        RequireString(header, "schema", Schema);
        string producer = RequireString(header, "producer");
        if (producer is not AuthorityProducer and not UnityProducer)
            throw new InvalidDataException("unsupported-producer");
        RequireString(header, "formalExeSha256", FormalExeSha256);
        if (RequireBoolean(header, "certificateEligible"))
            throw new InvalidDataException("certificate-must-be-false");
        string scenarioId = RequireString(header, "scenarioId");
        if (string.IsNullOrWhiteSpace(scenarioId))
            throw new InvalidDataException("scenario-id-empty");
        if (RequireInt32(header, "firstCompletedTick") != 1)
            throw new InvalidDataException("first-completed-tick-must-be-one");
        int expectedTicks = RequireInt32(header, "expectedTickCount");
        if (expectedTicks < 1 || expectedTicks != capture.Ticks.Count)
            throw new InvalidDataException("expected-tick-count-mismatch");
        RequireStringArray(header, "keyOrder", KeyOrder);
        JsonObject perCall = RequireObject(
            header,
            "perCallTraceAvailability");
        RequireString(perCall, "crt", "available-completed-ticks");
        RequireString(
            perCall,
            "synchronized",
            "available-completed-ticks");
        JsonObject perCallScope = RequireObject(header, "perCallTraceScope");
        if (RequireBoolean(perCallScope, "aiCursorExcluded") ||
            RequireBoolean(perCallScope, "humanScenarioRequired") ||
            !RequireBoolean(perCallScope, "initializationExcluded"))
        {
            throw new InvalidDataException("per-call-trace-scope-invalid");
        }
        ValidateInputPhase(header, "initialInputPhase");

        JsonObject initialRng = RequireObject(header, "initialRng");
        uint previousCrtState = ValidateInitialCrt(
            RequireObject(initialRng, "crt"),
            out ulong previousCrtCalls);
        ValidateInitialSynchronized(
            RequireObject(initialRng, "synchronized"),
            out int previousSynchronizedCounter,
            out int previousSynchronizedIndex,
            out ulong previousSynchronizedCalls);

        int entitySnapshots = 0;
        for (int index = 0; index < capture.Ticks.Count; index++)
        {
            JsonObject tick = capture.Ticks[index];
            RequireString(tick, "kind", "tick");
            long completedTick = RequireInt64(tick, "completedTick");
            if (completedTick != index + 1)
                throw new InvalidDataException("completed-tick-sequence-invalid");
            ValidateInputPhase(tick, "inputPhase");

            JsonObject rng = RequireObject(tick, "rng");
            ValidateTickCrt(
                RequireObject(rng, "crt"),
                previousCrtState,
                previousCrtCalls,
                out previousCrtState,
                out previousCrtCalls);
            ValidateTickSynchronized(
                RequireObject(rng, "synchronized"),
                previousSynchronizedCounter,
                previousSynchronizedIndex,
                previousSynchronizedCalls,
                out previousSynchronizedCounter,
                out previousSynchronizedIndex,
                out previousSynchronizedCalls);

            JsonArray entities = RequireArray(tick, "entities");
            int previousSlot = -1;
            foreach (JsonNode? node in entities)
            {
                JsonObject entity = node?.AsObject() ??
                    throw new InvalidDataException("entity-must-be-object");
                int slot = RequireInt32(entity, "slot");
                if (slot < 0 || slot <= previousSlot)
                    throw new InvalidDataException("entity-slots-not-strictly-ordered");
                previousSlot = slot;
                if (RequireUInt64(entity, "allocationEpoch") == 0)
                    throw new InvalidDataException("allocation-epoch-must-be-positive");
                _ = RequireInt32(entity, "objectId");
                ValidateInput(RequireObject(entity, "input"));
                entitySnapshots++;
            }
        }

        return new B2InputRngJointRawValidationReport
        {
            Valid = true,
            Status = "valid-b2-input-rng-joint-raw",
            Producer = producer,
            ScenarioId = scenarioId,
            ValidatedTicks = capture.Ticks.Count,
            EntitySnapshots = entitySnapshots,
        };
    }

    private static uint ValidateInitialCrt(
        JsonObject value,
        out ulong calls)
    {
        uint state = RequireUInt32(value, "state");
        calls = RequireUInt64(value, "totalCalls");
        return state;
    }

    private static void ValidateInitialSynchronized(
        JsonObject value,
        out int counter,
        out int index,
        out ulong calls)
    {
        ValidateSynchronizedState(value);
        counter = RequireInt32(value, "counter");
        index = RequireInt32(value, "index");
        calls = RequireUInt64(value, "totalCalls");
    }

    private static void ValidateTickCrt(
        JsonObject value,
        uint previousState,
        ulong previousCalls,
        out uint currentState,
        out ulong currentCalls)
    {
        currentState = RequireUInt32(value, "state");
        currentCalls = RequireUInt64(value, "totalCalls");
        ValidateCallDelta(value, previousCalls, currentCalls);
        JsonArray calls = RequireArray(value, "calls");
        if ((ulong)calls.Count != currentCalls - previousCalls)
            throw new InvalidDataException("crt-call-array-count-mismatch");

        uint state = previousState;
        for (int index = 0; index < calls.Count; index++)
        {
            JsonObject call = calls[index]?.AsObject() ??
                throw new InvalidDataException("crt-call-must-be-object");
            uint result = RequireUInt32(call, "result");
            if (result > 0x7FFFu)
                throw new InvalidDataException("crt-call-result-out-of-range");
            state = RequireUInt32(call, "stateAfter");
            ulong ordinal = RequireUInt64(call, "totalCalls");
            if (ordinal != previousCalls + (ulong)index + 1UL)
                throw new InvalidDataException("crt-call-ordinal-invalid");
        }
        if (state != currentState)
            throw new InvalidDataException("crt-call-final-state-mismatch");
    }

    private static void ValidateTickSynchronized(
        JsonObject value,
        int previousCounter,
        int previousIndex,
        ulong previousCalls,
        out int currentCounter,
        out int currentIndex,
        out ulong currentCalls)
    {
        JsonObject state = RequireObject(value, "state");
        ValidateSynchronizedState(state);
        currentCounter = RequireInt32(state, "counter");
        currentIndex = RequireInt32(state, "index");
        currentCalls = RequireUInt64(value, "totalCalls");
        ValidateCallDelta(value, previousCalls, currentCalls);
        JsonArray calls = RequireArray(value, "calls");
        if ((ulong)calls.Count != currentCalls - previousCalls)
            throw new InvalidDataException(
                "synchronized-call-array-count-mismatch");

        int counter = previousCounter;
        int tableIndex = previousIndex;
        uint lastCallSite = RequireUInt32(state, "lastCallSite");
        for (int index = 0; index < calls.Count; index++)
        {
            JsonObject call = calls[index]?.AsObject() ??
                throw new InvalidDataException(
                    "synchronized-call-must-be-object");
            uint callSite = RequireUInt32(call, "callSite");
            int upperBound = RequireInt32(call, "upperBound");
            int result = RequireInt32(call, "result");
            if (upperBound < 1 || result < 0 || result >= upperBound)
                throw new InvalidDataException(
                    "synchronized-call-result-out-of-range");
            counter = (counter + 1) % 1234;
            tableIndex = (tableIndex + 1) % 3000;
            if (RequireInt32(call, "counterAfter") != counter ||
                RequireInt32(call, "indexAfter") != tableIndex)
            {
                throw new InvalidDataException(
                    "synchronized-call-after-state-invalid");
            }
            ulong ordinal = RequireUInt64(call, "totalCalls");
            if (ordinal != previousCalls + (ulong)index + 1UL)
                throw new InvalidDataException(
                    "synchronized-call-ordinal-invalid");
            if (index == calls.Count - 1 && callSite != lastCallSite)
                throw new InvalidDataException(
                    "synchronized-call-last-site-mismatch");
        }
        if (counter != currentCounter || tableIndex != currentIndex)
            throw new InvalidDataException(
                "synchronized-call-final-state-mismatch");
    }

    private static void ValidateSynchronizedState(JsonObject value)
    {
        int counter = RequireInt32(value, "counter");
        int index = RequireInt32(value, "index");
        if (counter < 0 || counter >= 1234)
            throw new InvalidDataException("synchronized-counter-out-of-range");
        if (index < 0 || index >= 3000)
            throw new InvalidDataException("synchronized-index-out-of-range");
        string hash = RequireString(value, "tableHash64");
        if (!Hash64Pattern.IsMatch(hash))
            throw new InvalidDataException("synchronized-table-hash-invalid");
        _ = RequireUInt32(value, "lastCallSite");
    }

    private static void ValidateCallDelta(
        JsonObject value,
        ulong previousCalls,
        ulong totalCalls)
    {
        if (totalCalls < previousCalls)
            throw new InvalidDataException("rng-total-calls-moved-backwards");
        ulong delta = RequireUInt64(value, "tickCallCount");
        if (delta != totalCalls - previousCalls)
            throw new InvalidDataException("rng-tick-call-count-mismatch");
    }

    private static void ValidateInput(JsonObject input)
    {
        ValidateMask(input, "currentMask");
        ValidateMask(input, "previousMask");
        JsonObject edgeWindow = RequireObject(input, "edgeWindow");
        foreach (string edgeName in EdgeNames)
        {
            if (RequireInt32(edgeWindow, edgeName) < 0)
                throw new InvalidDataException("edge-window-negative");
        }
        if (RequireInt32(input, "defendReentryCooldown") < 0)
            throw new InvalidDataException("defend-reentry-negative");
        ValidateByteArray(input, "comboState", 10);
        int proxyTail = RequireInt32(input, "proxyTail");
        if (proxyTail is < 0 or > 255)
            throw new InvalidDataException("proxy-tail-out-of-range");
        ValidateIntArray(input, "keyHistory", 5);
        _ = RequireInt32(input, "runAccumulator");
        _ = RequireInt32(input, "lastAction");
        _ = RequireInt32(input, "remapState");
        ValidateByteArray(input, "remapIndices", 7);
        _ = RequireInt32(input, "boundState");
        _ = RequireInt32(input, "globalRecordState");
    }

    private static void ValidateInputPhase(JsonObject value, string property)
    {
        int phase = RequireInt32(value, property);
        if (phase is not 0 and not 1)
            throw new InvalidDataException("input-phase-out-of-range");
    }

    private static void ValidateMask(JsonObject value, string property)
    {
        int mask = RequireInt32(value, property);
        if (mask is < 0 or > 127)
            throw new InvalidDataException("input-mask-out-of-range");
    }

    private static void ValidateByteArray(
        JsonObject value,
        string property,
        int count)
    {
        JsonArray array = RequireArray(value, property);
        if (array.Count != count)
            throw new InvalidDataException($"{property}-length-invalid");
        foreach (JsonNode? node in array)
        {
            int item = RequireValue<int>(node, property);
            if (item is < 0 or > 255)
                throw new InvalidDataException($"{property}-item-out-of-range");
        }
    }

    private static void ValidateIntArray(
        JsonObject value,
        string property,
        int count)
    {
        JsonArray array = RequireArray(value, property);
        if (array.Count != count)
            throw new InvalidDataException($"{property}-length-invalid");
        foreach (JsonNode? node in array)
            _ = RequireValue<int>(node, property);
    }

    private static B2InputRngJointRawCapture ParseLines(
        IEnumerable<string> lines)
    {
        using IEnumerator<string> enumerator = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidDataException("empty-capture");
        JsonObject header = ParseObject(enumerator.Current, "header");
        var ticks = new List<JsonObject>();
        while (enumerator.MoveNext())
            ticks.Add(ParseObject(enumerator.Current, "tick"));
        return new B2InputRngJointRawCapture(header, ticks);
    }

    private static JsonObject ParseObject(string json, string role)
    {
        return JsonNode.Parse(json)?.AsObject() ??
            throw new InvalidDataException($"{role}-must-be-object");
    }

    private static string[] SplitLines(string text)
    {
        return text.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static B2InputRngJointRawValidationReport Invalid(string reason)
    {
        return new B2InputRngJointRawValidationReport
        {
            Valid = false,
            Status = "invalid-b2-input-rng-joint-raw",
            Reason = reason,
        };
    }

    internal static JsonObject RequireObject(JsonObject value, string property)
    {
        return value[property]?.AsObject() ??
            throw new InvalidDataException($"{property}-must-be-object");
    }

    internal static JsonArray RequireArray(JsonObject value, string property)
    {
        return value[property]?.AsArray() ??
            throw new InvalidDataException($"{property}-must-be-array");
    }

    internal static string RequireString(JsonObject value, string property)
    {
        return RequireValue<string>(value[property], property);
    }

    private static void RequireString(
        JsonObject value,
        string property,
        string expected)
    {
        if (!string.Equals(
                RequireString(value, property),
                expected,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException($"{property}-mismatch");
        }
    }

    internal static int RequireInt32(JsonObject value, string property)
    {
        return RequireValue<int>(value[property], property);
    }

    internal static long RequireInt64(JsonObject value, string property)
    {
        return RequireValue<long>(value[property], property);
    }

    internal static uint RequireUInt32(JsonObject value, string property)
    {
        return RequireValue<uint>(value[property], property);
    }

    internal static ulong RequireUInt64(JsonObject value, string property)
    {
        return RequireValue<ulong>(value[property], property);
    }

    private static bool RequireBoolean(JsonObject value, string property)
    {
        return RequireValue<bool>(value[property], property);
    }

    private static T RequireValue<T>(JsonNode? node, string property)
    {
        if (node is JsonValue jsonValue && jsonValue.TryGetValue(out T? value))
            return value!;
        throw new InvalidDataException($"{property}-type-invalid");
    }

    private static void RequireStringArray(
        JsonObject value,
        string property,
        IReadOnlyList<string> expected)
    {
        JsonArray array = RequireArray(value, property);
        if (array.Count != expected.Count)
            throw new InvalidDataException($"{property}-length-invalid");
        for (int index = 0; index < expected.Count; index++)
        {
            string actual = RequireValue<string>(array[index], property);
            if (!string.Equals(actual, expected[index], StringComparison.Ordinal))
                throw new InvalidDataException($"{property}-order-invalid");
        }
    }
}

internal sealed record B2InputRngJointRawCapture(
    JsonObject Header,
    List<JsonObject> Ticks);

internal sealed class B2InputRngJointRawValidationReport
{
    public bool Valid { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Producer { get; set; }
    public string? ScenarioId { get; set; }
    public int ValidatedTicks { get; set; }
    public int EntitySnapshots { get; set; }
    public string? Reason { get; set; }
}
