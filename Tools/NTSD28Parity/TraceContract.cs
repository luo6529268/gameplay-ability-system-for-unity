using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace NTSD28Parity;

internal static class TraceContract
{
    internal const string Schema = "ntsd28-logan-battle-trace-v3";
    internal const string DescriptorSchema = "ntsd28-logan-trace-contract-v3";
    internal const string ComparisonSchema = "ntsd28-logan-trace-comparison-v3";
    internal const string ValidationSchema = "ntsd28-logan-trace-validation-v3";
    internal const string SelfTestSchema = "ntsd28-logan-trace-self-test-v3";
    internal const string AuthorityExecutableSha256 =
        "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
    internal const string ContentPolicy = TraceContentIdentity.Policy;
    internal const int NormalLogicIntervalMilliseconds = 33;
    internal const int FastLogicIntervalMilliseconds = 3;

    internal static readonly string[] DomainOrder =
    [
        "input",
        "rng",
        "world",
        "entities",
        "relations",
        "rests",
        "events",
        "presentation",
    ];

    internal static readonly string[] ApprovedExceptions =
    [
        "slot-capacity-unity",
        "overhead-health-bars",
        "footself-marker",
        "mobile-bottom-gap-framing",
        "polygon-stage-boundary",
        "unity-random-weapon-drop",
        "fixed-world-camera",
    ];

    internal static readonly string[] ExcludedFeatures =
    [
        "native-battle-hud",
        "native-results-presentation",
        "background-layers-cycle",
        "native-selection-flow",
    ];

    internal static readonly string[] HeaderProperties =
    [
        "kind",
        "schema",
        "producer",
        "authority",
        "scenario",
        "timing",
        "slotCapacity",
        "content",
        "approvedExceptions",
        "excludedFeatures",
        "domainOrder",
    ];

    internal static readonly string[] TickProperties =
    [
        "kind",
        "completedTick",
        "hashes",
        "input",
        "rng",
        "world",
        "entities",
        "relations",
        "rests",
        "events",
        "presentation",
    ];

    private static readonly Regex Sha256Pattern = new(
        "^[0-9A-Fa-f]{64}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
    };

    internal static object CreateDescriptor()
    {
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["schema"] = DescriptorSchema,
            ["traceSchema"] = Schema,
            ["authorityExecutableSha256"] = AuthorityExecutableSha256,
            ["completedTickBoundary"] =
                "SimulationTickDriver28::step completed state; host/render frames excluded",
            ["normalLogicIntervalMilliseconds"] = NormalLogicIntervalMilliseconds,
            ["fastLogicIntervalMilliseconds"] = FastLogicIntervalMilliseconds,
            ["contentPolicy"] = ContentPolicy,
            ["slotCapacityEqualityRequired"] = false,
            ["approvedExceptionsAutomaticallyNormalized"] = false,
            ["certificateEligible"] = false,
            ["domainOrder"] = DomainOrder,
            ["approvedExceptions"] = ApprovedExceptions,
            ["excludedFeatures"] = ExcludedFeatures,
            ["requiredHeaderProperties"] = HeaderProperties,
            ["requiredTickProperties"] = TickProperties,
            ["rng"] = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["streams"] = new[] { "crt", "synchronized" },
                ["crtTickCallFields"] = new[] { "ordinal", "bound", "raw", "value" },
                ["synchronizedTickCallFields"] =
                    new[] { "ordinal", "callSite", "bound", "value" },
            },
            ["entity"] = EntityFieldContract.CreateDescriptor(),
            ["firstDifferenceOrder"] = DomainOrder.Append("overall").ToArray(),
        };
    }

    internal static bool IsSha256(string? value)
    {
        return value is not null && Sha256Pattern.IsMatch(value);
    }

    internal static string HashNode(JsonNode node)
    {
        string canonical = Canonicalize(node).ToJsonString(CompactOptions);
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(digest);
    }

    internal static string ComputeOverallHash(JsonObject hashes)
    {
        var committed = new JsonObject();
        foreach (string domain in DomainOrder)
        {
            committed[domain] =
                (hashes[domain]?.GetValue<string>() ?? string.Empty)
                .ToUpperInvariant();
        }

        return HashNode(committed);
    }

    internal static JsonNode Canonicalize(JsonNode? node)
    {
        if (node is null)
        {
            return JsonValue.Create((string?)null)!;
        }

        if (node is JsonObject sourceObject)
        {
            var result = new JsonObject();
            foreach ((string key, JsonNode? value) in sourceObject
                         .OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                result[key] = Canonicalize(value);
            }

            return result;
        }

        if (node is JsonArray sourceArray)
        {
            var result = new JsonArray();
            foreach (JsonNode? value in sourceArray)
            {
                result.Add(Canonicalize(value));
            }

            return result;
        }

        return node.DeepClone();
    }

    internal static bool SequenceEqual(JsonArray array, IReadOnlyList<string> expected)
    {
        if (array.Count != expected.Count)
        {
            return false;
        }

        for (int index = 0; index < expected.Count; index++)
        {
            if (array[index] is not JsonValue value ||
                !value.TryGetValue(out string? actual) ||
                !string.Equals(actual, expected[index], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool HasExactProperties(
        JsonObject value,
        IReadOnlyCollection<string> expected)
    {
        if (value.Count != expected.Count)
        {
            return false;
        }

        foreach (string property in expected)
        {
            if (!value.ContainsKey(property))
            {
                return false;
            }
        }

        return true;
    }
}
