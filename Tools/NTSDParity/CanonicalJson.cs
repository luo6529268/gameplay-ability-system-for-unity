using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSDParity;

internal static class CanonicalJson
{
    public static readonly JsonSerializerOptions CompactOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public static string Sha256(object? value)
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(value, CompactOptions);
        return Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
    }

    public static string Sha256Bytes(ReadOnlySpan<byte> value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    public static JsonNode? Canonicalize(JsonNode? value)
    {
        if (value is JsonObject sourceObject)
        {
            JsonObject result = new();
            foreach ((string key, JsonNode? child) in sourceObject.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                result[key] = Canonicalize(child);
            return result;
        }
        if (value is JsonArray sourceArray)
        {
            JsonArray result = new();
            foreach (JsonNode? child in sourceArray)
                result.Add(Canonicalize(child));
            return result;
        }
        return value?.DeepClone();
    }

    public static string NormalizePath(string value)
    {
        string normalized = value.Trim().Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];
        while (normalized.Contains("//", StringComparison.Ordinal))
            normalized = normalized.Replace("//", "/", StringComparison.Ordinal);
        return normalized.ToLowerInvariant();
    }

    public static string NormalizePureAssetPath(string value)
    {
        string normalized = NormalizePath(value);
        int separator = normalized.LastIndexOf('/');
        string identifier = separator >= 0 ? normalized[(separator + 1)..] : normalized;
        return identifier.StartsWith("snddata_", StringComparison.Ordinal)
            ? identifier["snddata_".Length..]
            : identifier;
    }

    public static void CompareNodes(
        JsonNode? authority,
        JsonNode? unity,
        string path,
        List<FieldDifference> differences,
        int limit = int.MaxValue)
    {
        if (differences.Count >= limit)
            return;

        if (authority is JsonObject authorityObject && unity is JsonObject unityObject)
        {
            IEnumerable<string> keys = authorityObject.Select(item => item.Key)
                .Union(unityObject.Select(item => item.Key))
                .OrderBy(value => value, StringComparer.Ordinal);
            foreach (string key in keys)
                CompareNodes(authorityObject[key], unityObject[key], path + "." + key, differences, limit);
            return;
        }

        if (authority is JsonArray authorityArray && unity is JsonArray unityArray)
        {
            int count = Math.Max(authorityArray.Count, unityArray.Count);
            for (int i = 0; i < count; i++)
            {
                JsonNode? authorityItem = i < authorityArray.Count ? authorityArray[i] : null;
                JsonNode? unityItem = i < unityArray.Count ? unityArray[i] : null;
                CompareNodes(authorityItem, unityItem, path + "[" + i + "]", differences, limit);
            }
            return;
        }

        string authorityJson = authority?.ToJsonString() ?? "null";
        string unityJson = unity?.ToJsonString() ?? "null";
        if (!string.Equals(authorityJson, unityJson, StringComparison.Ordinal))
        {
            differences.Add(new FieldDifference
            {
                Path = path,
                Authority = authorityJson,
                Unity = unityJson,
            });
        }
    }
}

internal sealed class FieldDifference
{
    public string Path { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string Unity { get; set; } = string.Empty;
}
