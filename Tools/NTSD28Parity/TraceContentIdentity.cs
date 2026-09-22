using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class TraceContentIdentity
{
    internal const string Policy = "logan-dat-character-images";
    internal const string LoganTag = "NTSD28_LOGAN_DAT_SEMANTICS_V3";
    internal const string LegacyTag = "NTSD28_UNITY_LEGACY_DAT_SEMANTICS_V1";
    internal const string BattleInputV1 = "NTSD28_LOGAN_BATTLE_INPUTS_V1";
    internal const string BattleInputV2 = "NTSD28_LOGAN_BATTLE_INPUTS_V2";
    private static readonly string[] Properties =
    [
        "policy", "scope", "profile", "rawDefinitionSha256", "decodeContract",
        "semanticSha256", "catalogFingerprint64", "schemas",
    ];
    private static readonly string[] LoganProperties = Properties.Concat(new[]
    {
        "objectDefinitionSha256", "fusionInputSha256", "fusionSemanticSha256",
    }).ToArray();
    private static readonly string[] LoganV2Properties = LoganProperties.Concat(new[]
    {
        "battleInputContract", "modeInputSha256", "modeSemanticSha256",
    }).ToArray();
    private static readonly IReadOnlyDictionary<string, int> Schemas = new Dictionary<string, int>
    {
        ["entityRuntime"] = 17, ["aggregate"] = 25, ["checksum"] = 28,
        ["characterShell"] = 2, ["entityBaseShell"] = 2,
    };

    internal static JsonObject Create(string profile, string rawDefinition)
    {
        if (profile != "unity-legacy")
            throw new InvalidDataException("logan-content-requires-three-components");
        return Build(profile, rawDefinition);
    }

    internal static JsonObject CreateLogan(string objectDefinition, string fusionInput, string fusionSemantic)
    {
        byte[] prefix = Encoding.ASCII.GetBytes(BattleInputV1 + "\0");
        byte[] inputs = prefix.Concat(DecodeSha(objectDefinition)).Concat(DecodeSha(fusionInput))
            .Concat(DecodeSha(fusionSemantic)).ToArray();
        JsonObject content = Build("logan-runtime", Convert.ToHexString(SHA256.HashData(inputs)));
        content["objectDefinitionSha256"] = objectDefinition.ToUpperInvariant();
        content["fusionInputSha256"] = fusionInput.ToUpperInvariant();
        content["fusionSemanticSha256"] = fusionSemantic.ToUpperInvariant();
        return content;
    }

    internal static JsonObject CreateLogan(string objectDefinition, string fusionInput, string fusionSemantic,
        string modeInput, string modeSemantic)
    {
        byte[] inputs = Encoding.ASCII.GetBytes(BattleInputV2 + "\0")
            .Concat(DecodeSha(objectDefinition)).Concat(DecodeSha(fusionInput))
            .Concat(DecodeSha(fusionSemantic)).Concat(DecodeSha(modeInput)).Concat(DecodeSha(modeSemantic)).ToArray();
        JsonObject content = Build("logan-runtime", Convert.ToHexString(SHA256.HashData(inputs)));
        content["scope"] = "catalog-object-fusion-mode-definitions";
        content["battleInputContract"] = BattleInputV2;
        content["objectDefinitionSha256"] = objectDefinition.ToUpperInvariant();
        content["fusionInputSha256"] = fusionInput.ToUpperInvariant();
        content["fusionSemanticSha256"] = fusionSemantic.ToUpperInvariant();
        content["modeInputSha256"] = modeInput.ToUpperInvariant();
        content["modeSemanticSha256"] = modeSemantic.ToUpperInvariant();
        content["schemas"]!["aggregate"] = 26;
        content["schemas"]!["checksum"] = 29;
        return content;
    }

    private static JsonObject Build(string profile, string rawDefinition)
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
        string profile = Text(content, "profile");
        bool version2 = profile == "logan-runtime" && content.ContainsKey("battleInputContract");
        if (version2 && Text(content, "battleInputContract") != BattleInputV2)
            throw new InvalidDataException("unknown-content-battle-input-contract");
        if (!TraceContract.HasExactProperties(content,
            profile == "logan-runtime" ? (version2 ? LoganV2Properties : LoganProperties) : Properties))
            throw new InvalidDataException("content-property-set-mismatch");
        if (requireLogan && profile != "logan-runtime")
            throw new InvalidDataException("authority-content-profile-mismatch");
        string raw = Text(content, "rawDefinitionSha256");
        DecodeSha(raw);
        JsonObject expected = profile == "logan-runtime"
            ? version2
                ? CreateLogan(Text(content, "objectDefinitionSha256"), Text(content, "fusionInputSha256"),
                    Text(content, "fusionSemanticSha256"), Text(content, "modeInputSha256"), Text(content, "modeSemanticSha256"))
                : CreateLogan(Text(content, "objectDefinitionSha256"), Text(content, "fusionInputSha256"),
                Text(content, "fusionSemanticSha256"))
            : Create(profile, raw);
        foreach (string key in new[] { "policy", "scope", "decodeContract" })
            if (!string.Equals(Text(content, key), Text(expected, key), StringComparison.Ordinal))
                throw new InvalidDataException("content-" + key + "-mismatch");
        foreach (string key in new[] { "rawDefinitionSha256", "semanticSha256", "catalogFingerprint64" })
            if (!string.Equals(Text(content, key), Text(expected, key), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("content-" + key + "-mismatch");
        if (content["schemas"] is not JsonObject schemas ||
            !TraceContract.HasExactProperties(schemas, Schemas.Keys.ToArray()))
            throw new InvalidDataException("content-schema-property-set-mismatch");
        var actualSchemas = new Dictionary<string, int>();
        foreach (var pair in Schemas)
        {
            if (schemas[pair.Key] is not JsonValue value || !value.TryGetValue<int>(out int actual))
                throw new InvalidDataException("content-schema-" + pair.Key + "-mismatch");
            actualSchemas[pair.Key] = actual;
        }
        bool historical = Schemas.All(pair => actualSchemas[pair.Key] == pair.Value);
        bool current = Schemas.All(pair => actualSchemas[pair.Key] ==
            (pair.Key == "aggregate" ? 26 : pair.Key == "checksum" ? 29 : pair.Value));
        if (!current && (version2 || !historical))
            throw new InvalidDataException("content-schema-tuple-mismatch");
        string contract = profile == "logan-runtime" ? (version2 ? BattleInputV2 : BattleInputV1) : LegacyTag;
        return profile + "|" + contract + "|" + Text(expected, "rawDefinitionSha256") + "|" +
            Text(expected, "semanticSha256") + "|" + string.Join("|", Schemas.Keys.Select(key => key + "=" + actualSchemas[key]));
    }

    private static (string Scope, string Tag) Profile(string profile)
    {
        return profile switch
        {
            "logan-runtime" => ("catalog-object-fusion-definitions", LoganTag),
            "unity-legacy" => ("unity-legacy-dat-files", LegacyTag),
            _ => throw new InvalidDataException("unknown-content-profile"),
        };
    }

    private static byte[] DecodeSha(string value)
    {
        if (value == null || value.Length != 64 || !value.All(Uri.IsHexDigit))
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
