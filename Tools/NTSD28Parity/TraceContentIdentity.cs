using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class TraceContentIdentity
{
    internal const string Policy = "logan-dat-character-images";
    internal const string LoganTag = "NTSD28_LOGAN_DAT_SEMANTICS_V2";
    internal const string LegacyTag = "NTSD28_UNITY_LEGACY_DAT_SEMANTICS_V1";
    private static readonly string[] Properties =
    [
        "policy", "scope", "profile", "rawDefinitionSha256", "decodeContract",
        "semanticSha256", "catalogFingerprint64", "schemas",
    ];
    private static readonly IReadOnlyDictionary<string, int> Schemas = new Dictionary<string, int>
    {
        ["entityRuntime"] = 13, ["aggregate"] = 21, ["checksum"] = 24,
        ["characterShell"] = 2, ["entityBaseShell"] = 2,
    };

    internal static JsonObject Create(string profile, string rawDefinition)
    {
        (string scope, string tag) = Profile(profile);
        byte[] raw = DecodeSha(rawDefinition);
        byte[] prefix = Encoding.ASCII.GetBytes(tag + "\0");
        byte[] input = new byte[prefix.Length + raw.Length];
        prefix.CopyTo(input, 0);
        raw.CopyTo(input, prefix.Length);
        byte[] semantic = SHA256.HashData(input);
        ulong projection = BinaryPrimitives.ReadUInt64LittleEndian(semantic);
        if (projection == 0) projection = 1;
        var schemas = new JsonObject();
        foreach (var pair in Schemas) schemas[pair.Key] = pair.Value;
        return new JsonObject
        {
            ["policy"] = Policy, ["scope"] = scope, ["profile"] = profile,
            ["rawDefinitionSha256"] = Convert.ToHexString(raw), ["decodeContract"] = tag,
            ["semanticSha256"] = Convert.ToHexString(semantic),
            ["catalogFingerprint64"] = projection.ToString("X16"), ["schemas"] = schemas,
        };
    }

    internal static string Validate(JsonObject content, bool requireLogan = false)
    {
        if (!TraceContract.HasExactProperties(content, Properties))
            throw new InvalidDataException("content-property-set-mismatch");
        string profile = Text(content, "profile");
        if (requireLogan && profile != "logan-runtime")
            throw new InvalidDataException("authority-content-profile-mismatch");
        string raw = Text(content, "rawDefinitionSha256");
        JsonObject expected = Create(profile, raw);
        foreach (string key in new[] { "policy", "scope", "decodeContract" })
            if (!string.Equals(Text(content, key), Text(expected, key), StringComparison.Ordinal))
                throw new InvalidDataException("content-" + key + "-mismatch");
        foreach (string key in new[] { "semanticSha256", "catalogFingerprint64" })
            if (!string.Equals(Text(content, key), Text(expected, key), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("content-" + key + "-mismatch");
        if (content["schemas"] is not JsonObject schemas ||
            !TraceContract.HasExactProperties(schemas, Schemas.Keys.ToArray()))
            throw new InvalidDataException("content-schema-property-set-mismatch");
        foreach (var pair in Schemas)
            if (schemas[pair.Key] is not JsonValue value || !value.TryGetValue<int>(out int actual) || actual != pair.Value)
                throw new InvalidDataException("content-schema-" + pair.Key + "-mismatch");
        return profile + "|" + Text(expected, "rawDefinitionSha256") + "|" + Text(expected, "semanticSha256");
    }

    private static (string Scope, string Tag) Profile(string profile)
    {
        return profile switch
        {
            "logan-runtime" => ("catalog-object-definitions", LoganTag),
            "unity-legacy" => ("unity-legacy-dat-files", LegacyTag),
            _ => throw new InvalidDataException("unknown-content-profile"),
        };
    }

    private static byte[] DecodeSha(string value)
    {
        if (value.Length != 64 || !value.All(Uri.IsHexDigit))
            throw new InvalidDataException("invalid-content-raw-definition-sha256");
        return Convert.FromHexString(value);
    }

    private static string Text(JsonObject value, string key)
    {
        if (value[key] is not JsonValue node || !node.TryGetValue<string>(out string? result) || result == null)
            throw new InvalidDataException("invalid-content-" + key);
        return result;
    }
}
