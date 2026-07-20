using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NtsdReleaseCSharp.BattleCore.Simulation;
using NtsdReleaseCSharp.Data;

namespace NTSDParity;

internal static class DataAuditCommand
{
    private const string DefaultAuthorityRoot = @"J:\QQFile\NTSD2.4";

    private static readonly HashSet<string> ProjectionExclusions = new(StringComparer.Ordinal)
    {
        "CharData.Frames",
        "CharData.FrameIndex",
    };

    private static readonly HashSet<string> BattleLogicExclusions = new(ProjectionExclusions, StringComparer.Ordinal)
    {
        "CharData.Name",
        "CharData.HeadFile",
        "CharData.SmallFile",
        "CharData.SpriteFile",
        "CharData.SpriteW",
        "CharData.SpriteH",
        "CharData.SpriteRow",
        "CharData.SpriteCol",
        "CharData.SpriteRanges",
        "CharData.WeaponHitSound",
        "CharData.WeaponDropSound",
        "CharData.WeaponBrokenSound",
        "FrameData.Pic",
        "FrameData.Sound",
    };

    private static readonly HashSet<string> NormalizedPathMembers = new(StringComparer.Ordinal)
    {
        "CharData.HeadFile",
        "CharData.SmallFile",
        "CharData.SpriteFile",
        "CharData.WeaponHitSound",
        "CharData.WeaponDropSound",
        "CharData.WeaponBrokenSound",
        "SpriteRange.File",
        "FrameData.Sound",
    };

    public static int Run(string[] args)
    {
        CommandLine cli = CommandLine.Parse(args);
        string authorityRoot = Path.GetFullPath(cli.Get("--authority-root") ?? DefaultAuthorityRoot);
        string unityRoot = RepositoryPaths.FindUnityRoot(cli.Get("--unity-root"));
        string authorityIndex = Path.GetFullPath(cli.Get("--authority-index") ?? Path.Combine(authorityRoot, "data", "data.txt"));
        string unityIndex = Path.GetFullPath(cli.Get("--unity-index") ?? Path.Combine(unityRoot, "Assets", "NTSD", "Config", "data.txt"));
        string output = RepositoryPaths.ResolveOutput(cli.Get("--output") ?? "data-audit-report.json");

        HashSet<int> requestedOids = ParseRequestedOids(cli.GetAll("--oid"));
        List<OidEntry> authorityEntries = DatLoader.ParseDataTxt(authorityIndex);
        List<OidEntry> unityEntries = DatLoader.ParseDataTxt(unityIndex);
        Dictionary<int, OidEntry> authorityByOid = FirstByOid(authorityEntries);
        Dictionary<int, OidEntry> unityByOid = FirstByOid(unityEntries);
        int[] oids = authorityByOid.Keys
            .Union(unityByOid.Keys)
            .Where(oid => requestedOids.Count == 0 || requestedOids.Contains(oid))
            .OrderBy(oid => oid)
            .ToArray();

        ManifestSet authorityManifest = BuildManifest(
            authorityByOid.Values.Where(entry => requestedOids.Count == 0 || requestedOids.Contains(entry.Oid)),
            entry => DataPaths.ToAbsolutePath(authorityRoot, entry.File),
            forceDecrypt: true);
        ManifestSet unityManifest = BuildManifest(
            unityByOid.Values.Where(entry => requestedOids.Count == 0 || requestedOids.Contains(entry.Oid)),
            entry => ResolveUnityPath(unityRoot, entry.File),
            forceDecrypt: false);
        List<OidAuditResult> results = [];
        foreach (int oid in oids)
        {
            authorityByOid.TryGetValue(oid, out OidEntry? authorityEntry);
            unityByOid.TryGetValue(oid, out OidEntry? unityEntry);
            results.Add(AuditOid(
                oid,
                authorityRoot,
                unityRoot,
                authorityEntry,
                unityEntry));
        }

        ManifestSummary manifest = new()
        {
            Schema = "ntsd-resolved-dat-manifest-v1",
            AuthoritySha256 = CanonicalJson.Sha256(authorityManifest.Full),
            UnitySha256 = CanonicalJson.Sha256(unityManifest.Full),
            AuthorityBattleLogicSha256 = CanonicalJson.Sha256(authorityManifest.BattleLogic),
            UnityBattleLogicSha256 = CanonicalJson.Sha256(unityManifest.BattleLogic),
            AuthorityPresentationSha256 = CanonicalJson.Sha256(authorityManifest.Presentation),
            UnityPresentationSha256 = CanonicalJson.Sha256(unityManifest.Presentation),
            AuthorityEntries = BuildManifestDigests(authorityManifest.Full),
            UnityEntries = BuildManifestDigests(unityManifest.Full),
        };
        manifest.Equal = string.Equals(manifest.AuthoritySha256, manifest.UnitySha256, StringComparison.Ordinal);
        manifest.BattleLogicEqual = string.Equals(
            manifest.AuthorityBattleLogicSha256,
            manifest.UnityBattleLogicSha256,
            StringComparison.Ordinal);
        manifest.PresentationEqual = string.Equals(
            manifest.AuthorityPresentationSha256,
            manifest.UnityPresentationSha256,
            StringComparison.Ordinal);

        DataAuditReport report = new()
        {
            Schema = "ntsd-data-audit-v2",
            RequestedOids = requestedOids.OrderBy(value => value).ToArray(),
            NormalizedPathMembers = NormalizedPathMembers.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            DuplicateAuthorityOids = DuplicateOids(authorityEntries),
            DuplicateUnityOids = DuplicateOids(unityEntries),
            Manifest = manifest,
            Results = results,
        };
        report.Summary = BuildSummary(results);
        report.DifferenceCategories = BuildDifferenceCategories(results);

        File.WriteAllText(output, JsonSerializer.Serialize(report, JsonProjection.SerializerOptions), new UTF8Encoding(false));
        Console.WriteLine(output);
        Console.WriteLine(
            $"audited={report.Summary.Audited} equal={report.Summary.Equal} different={report.Summary.Different} " +
            $"missingAuthority={report.Summary.MissingAuthority} missingUnity={report.Summary.MissingUnity} errors={report.Summary.Errors}");
        Console.WriteLine($"authorityManifest={manifest.AuthoritySha256} unityManifest={manifest.UnitySha256}");
        Console.WriteLine(
            $"authorityBattleLogicManifest={manifest.AuthorityBattleLogicSha256} " +
            $"unityBattleLogicManifest={manifest.UnityBattleLogicSha256}");
        if (report.Summary.Errors != 0)
            return 1;
        if (cli.Has("--require-equal") &&
            (report.Summary.Different != 0 || report.Summary.MissingAuthority != 0 ||
             report.Summary.MissingUnity != 0 || !report.Manifest.Equal))
        {
            Console.Error.WriteLine("data-audit equality certificate failed");
            return 3;
        }
        return 0;
    }

    public static object ProjectResolvedCharData(CharData data)
        => ProjectResolvedCharData(data, ProjectionExclusions);

    public static object ProjectBattleLogicCharData(CharData data)
        => ProjectResolvedCharData(data, BattleLogicExclusions);

    private static object ProjectResolvedCharData(CharData data, ISet<string> exclusions)
    {
        object? baseProjection = JsonProjection.Project(data, exclusions, normalizedPathMembers: NormalizedPathMembers);
        if (baseProjection is not SortedDictionary<string, object?> projected)
            throw new InvalidOperationException("CharData projection did not produce an object.");

        SortedDictionary<string, object?> resolvedFrames = new(StringComparer.Ordinal);
        for (int frameId = 0; frameId < CharData.MaxFrameId; frameId++)
        {
            if (!data.HasFrame(frameId))
                continue;

            FrameData frame = data.GetFrameOrNull(frameId)
                ?? throw new InvalidOperationException($"Resolved frame {frameId} unexpectedly returned null.");
            resolvedFrames[frameId.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                JsonProjection.Project(frame, exclusions, normalizedPathMembers: NormalizedPathMembers);
        }

        projected["ResolvedFrameCount"] = resolvedFrames.Count;
        projected["ResolvedFramesById"] = resolvedFrames;
        return projected;
    }

    public static string ComputeLoadedBattleLogicManifestSha256(GameWorld world)
    {
        SortedDictionary<string, object?> entries = new(StringComparer.Ordinal);
        foreach (CharData data in world.CharData.Where(value => value is not null).Cast<CharData>().OrderBy(value => value.Oid))
            entries[FormatOid(data.Oid)] = BuildManifestEntry(data.ObjType, ProjectBattleLogicCharData(data));
        return CanonicalJson.Sha256(entries);
    }

    internal static byte[] LoadDatPayload(string path, bool forceDecrypt)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("DAT file not found.", path);

        byte[] bytes = File.ReadAllBytes(path);
        bool encrypted = forceDecrypt || LooksEncrypted(bytes);
        byte[] payload = encrypted ? DatLoader.DatDecrypt(path) : bytes;
        if (payload.Length == 0)
            throw new InvalidDataException("DAT payload is empty after loading/decryption.");
        return payload;
    }

    private static OidAuditResult AuditOid(
        int oid,
        string authorityRoot,
        string unityRoot,
        OidEntry? authorityEntry,
        OidEntry? unityEntry)
    {
        OidAuditResult result = new() { Oid = oid };
        if (authorityEntry is null)
        {
            result.Status = "missing-authority-index";
            result.UnityPath = unityEntry is null ? null : NormalizeReportPath(unityEntry.File);
            return result;
        }
        if (unityEntry is null)
        {
            result.Status = "missing-unity-index";
            result.AuthorityPath = NormalizeReportPath(authorityEntry.File);
            return result;
        }

        string authorityPath = DataPaths.ToAbsolutePath(authorityRoot, authorityEntry.File);
        string unityPath = ResolveUnityPath(unityRoot, unityEntry.File);
        result.AuthorityPath = NormalizeReportPath(authorityEntry.File);
        result.UnityPath = NormalizeReportPath(unityEntry.File);
        result.AuthorityType = authorityEntry.Type;
        result.UnityType = unityEntry.Type;

        if (!File.Exists(authorityPath))
        {
            result.Status = "missing-authority-file";
            return result;
        }
        if (!File.Exists(unityPath))
        {
            result.Status = "missing-unity-file";
            return result;
        }

        try
        {
            CharData authorityData = ParseDat(authorityPath, authorityEntry, forceDecrypt: true, out string authorityInputKind);
            CharData unityData = ParseDat(unityPath, unityEntry, forceDecrypt: false, out string unityInputKind);
            result.AuthorityInputKind = authorityInputKind;
            result.UnityInputKind = unityInputKind;
            result.DuplicateAuthorityFrameIds = DuplicateFrameIds(authorityData);
            result.DuplicateUnityFrameIds = DuplicateFrameIds(unityData);

            object authorityProjected = ProjectResolvedCharData(authorityData);
            object unityProjected = ProjectResolvedCharData(unityData);
            CanonicalJson.CompareNodes(
                JsonProjection.ToNode(authorityProjected),
                JsonProjection.ToNode(unityProjected),
                "$",
                result.Differences);
            if (authorityEntry.Type != unityEntry.Type)
            {
                result.Differences.Insert(0, new FieldDifference
                {
                    Path = "$.ObjType",
                    Authority = authorityEntry.Type.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Unity = unityEntry.Type.ToString(System.Globalization.CultureInfo.InvariantCulture),
                });
            }
            result.Status = result.Differences.Count == 0 ? "equal" : "different";
        }
        catch (Exception ex)
        {
            result.Status = "error";
            result.Error = ex.Message;
        }

        return result;
    }

    private static CharData ParseDat(string path, OidEntry entry, bool forceDecrypt, out string inputKind)
    {
        byte[] original = File.ReadAllBytes(path);
        bool encrypted = forceDecrypt || LooksEncrypted(original);
        byte[] payload = encrypted ? DatLoader.DatDecrypt(path) : original;
        inputKind = encrypted ? "encrypted" : "plaintext";
        if (payload.Length == 0)
            throw new InvalidDataException("DAT payload is empty after loading/decryption.");

        CharData data = new();
        if (!DatLoader.ParseCharData(payload, data))
            throw new InvalidDataException("Authority DatLoader.ParseCharData returned false.");
        data.Oid = entry.Oid;
        data.ObjType = entry.Type;
        return data;
    }

    private static bool LooksEncrypted(byte[] bytes)
    {
        if (bytes.Length <= 123)
            return false;
        int zeroCount = 0;
        for (int i = 0; i < 123; i++)
        {
            if (bytes[i] == 0)
                zeroCount++;
        }
        return zeroCount >= 120;
    }

    private static string ResolveUnityPath(string unityRoot, string indexedPath)
    {
        string normalized = indexedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        if (normalized.StartsWith("Assets" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return Path.GetFullPath(Path.Combine(unityRoot, normalized));
        return Path.GetFullPath(Path.Combine(unityRoot, "Assets", "NTSD", "Config", normalized));
    }

    private static string NormalizeReportPath(string value) => CanonicalJson.NormalizePath(value);

    private static string FormatOid(int oid)
        => oid.ToString("D6", System.Globalization.CultureInfo.InvariantCulture);

    private static ManifestSet BuildManifest(
        IEnumerable<OidEntry> entries,
        Func<OidEntry, string> resolvePath,
        bool forceDecrypt)
    {
        ManifestSet manifest = new();
        foreach (OidEntry entry in entries.OrderBy(value => value.Oid))
        {
            string path = resolvePath(entry);
            if (!File.Exists(path))
                continue;
            CharData data = ParseDat(path, entry, forceDecrypt, out _);
            string key = FormatOid(entry.Oid);
            manifest.Full[key] = BuildManifestEntry(entry.Type, ProjectResolvedCharData(data));
            manifest.BattleLogic[key] = BuildManifestEntry(entry.Type, ProjectBattleLogicCharData(data));
            manifest.Presentation[key] = BuildManifestEntry(entry.Type, ProjectPresentationCharData(data));
        }
        return manifest;
    }

    private static object ProjectPresentationCharData(CharData data)
    {
        SortedDictionary<string, object?> frames = new(StringComparer.Ordinal);
        for (int frameId = 0; frameId < CharData.MaxFrameId; frameId++)
        {
            if (!data.HasFrame(frameId))
                continue;
            FrameData frame = data.GetFrameOrNull(frameId)!;
            frames[frameId.ToString(System.Globalization.CultureInfo.InvariantCulture)] =
                new SortedDictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["pic"] = frame.Pic,
                    ["centerX"] = frame.CenterX,
                    ["centerY"] = frame.CenterY,
                    ["sound"] = CanonicalJson.NormalizePureAssetPath(frame.Sound),
                };
        }

        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["name"] = data.Name,
            ["headFile"] = CanonicalJson.NormalizePureAssetPath(data.HeadFile),
            ["smallFile"] = CanonicalJson.NormalizePureAssetPath(data.SmallFile),
            ["spriteFile"] = CanonicalJson.NormalizePureAssetPath(data.SpriteFile),
            ["spriteW"] = data.SpriteW,
            ["spriteH"] = data.SpriteH,
            ["spriteRow"] = data.SpriteRow,
            ["spriteCol"] = data.SpriteCol,
            ["spriteRanges"] = JsonProjection.Project(data.SpriteRanges, normalizedPathMembers: NormalizedPathMembers),
            ["weaponHitSound"] = CanonicalJson.NormalizePureAssetPath(data.WeaponHitSound),
            ["weaponDropSound"] = CanonicalJson.NormalizePureAssetPath(data.WeaponDropSound),
            ["weaponBrokenSound"] = CanonicalJson.NormalizePureAssetPath(data.WeaponBrokenSound),
            ["resolvedFramesById"] = frames,
        };
    }

    private static object BuildManifestEntry(int type, object data)
        => new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["type"] = type,
            ["data"] = data,
        };

    private static ManifestEntryDigest[] BuildManifestDigests(SortedDictionary<string, object?> manifest)
        => manifest.Select(pair => new ManifestEntryDigest
        {
            Oid = int.Parse(pair.Key, System.Globalization.CultureInfo.InvariantCulture),
            Sha256 = CanonicalJson.Sha256(pair.Value),
        }).ToArray();

    private static HashSet<int> ParseRequestedOids(IReadOnlyList<string> values)
    {
        HashSet<int> result = [];
        foreach (string value in values)
        {
            if (!int.TryParse(value, out int oid))
                throw new ArgumentException($"Invalid --oid value '{value}'.");
            result.Add(oid);
        }
        return result;
    }

    private static Dictionary<int, OidEntry> FirstByOid(IEnumerable<OidEntry> entries)
        => entries.GroupBy(entry => entry.Oid).ToDictionary(group => group.Key, group => group.First());

    private static int[] DuplicateOids(IEnumerable<OidEntry> entries)
        => entries.GroupBy(entry => entry.Oid).Where(group => group.Count() > 1).Select(group => group.Key).OrderBy(value => value).ToArray();

    private static int[] DuplicateFrameIds(CharData data)
        => data.Frames
            .GroupBy(frame => frame.FrameId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(value => value)
            .ToArray();

    private static DataAuditSummary BuildSummary(IEnumerable<OidAuditResult> results)
    {
        OidAuditResult[] array = results.ToArray();
        return new DataAuditSummary
        {
            Audited = array.Length,
            Equal = array.Count(result => result.Status == "equal"),
            Different = array.Count(result => result.Status == "different"),
            MissingAuthority = array.Count(result => result.Status.StartsWith("missing-authority", StringComparison.Ordinal)),
            MissingUnity = array.Count(result => result.Status.StartsWith("missing-unity", StringComparison.Ordinal)),
            Errors = array.Count(result => result.Status == "error"),
        };
    }

    private static DifferenceCategorySummary[] BuildDifferenceCategories(IEnumerable<OidAuditResult> results)
    {
        Dictionary<string, DifferenceCategorySummary> grouped = results
            .Where(result => result.Status == "different")
            .SelectMany(result => result.Differences.Select(difference => new
            {
                result.Oid,
                Category = ClassifyDifference(difference.Path),
            }))
            .GroupBy(value => value.Category, StringComparer.Ordinal)
            .OrderBy(group => CategoryOrder(group.Key))
            .Select(group => new DifferenceCategorySummary
            {
                Category = group.Key,
                DifferenceCount = group.Count(),
                OidCount = group.Select(value => value.Oid).Distinct().Count(),
                Oids = group.Select(value => value.Oid).Distinct().OrderBy(value => value).ToArray(),
            })
            .ToDictionary(value => value.Category, StringComparer.Ordinal);

        string[] categories = ["logic", "frame", "geometry", "sprite-dimension", "sound", "path-only"];
        return categories.Select(category => grouped.TryGetValue(category, out DifferenceCategorySummary? summary)
            ? summary
            : new DifferenceCategorySummary { Category = category }).ToArray();
    }

    private static string ClassifyDifference(string path)
    {
        if (path is "$.headFile" or "$.smallFile" or "$.spriteFile" ||
            (path.StartsWith("$.spriteRanges[", StringComparison.Ordinal) && path.EndsWith(".file", StringComparison.Ordinal)))
        {
            return "path-only";
        }
        if (path.EndsWith(".sound", StringComparison.Ordinal) ||
            path is "$.weaponHitSound" or "$.weaponDropSound" or "$.weaponBrokenSound")
        {
            return "sound";
        }
        if (path is "$.spriteW" or "$.spriteH" or "$.spriteRow" or "$.spriteCol" ||
            path.StartsWith("$.spriteRanges[", StringComparison.Ordinal))
        {
            return "sprite-dimension";
        }
        string member = path[(path.LastIndexOf('.') + 1)..];
        if (member is "x" or "y" or "w" or "h" or "zwidth" or
            "dvx" or "dvy" or "dvz" or "throwVx" or "throwVy" or "throwVz" or
            "centerX" or "centerY")
        {
            return "geometry";
        }
        if (path == "$.resolvedFrameCount" || path.StartsWith("$.resolvedFramesById.", StringComparison.Ordinal))
            return "frame";
        return "logic";
    }

    private static int CategoryOrder(string category)
        => category switch
        {
            "logic" => 0,
            "frame" => 1,
            "geometry" => 2,
            "sprite-dimension" => 3,
            "sound" => 4,
            "path-only" => 5,
            _ => 6,
        };

    private sealed class DataAuditReport
    {
        public string Schema { get; set; } = string.Empty;
        public int[] RequestedOids { get; set; } = [];
        public string[] NormalizedPathMembers { get; set; } = [];
        public int[] DuplicateAuthorityOids { get; set; } = [];
        public int[] DuplicateUnityOids { get; set; } = [];
        public ManifestSummary Manifest { get; set; } = new();
        public DataAuditSummary Summary { get; set; } = new();
        public DifferenceCategorySummary[] DifferenceCategories { get; set; } = [];
        public List<OidAuditResult> Results { get; set; } = [];
    }

    private sealed class ManifestSummary
    {
        public string Schema { get; set; } = string.Empty;
        public string AuthoritySha256 { get; set; } = string.Empty;
        public string UnitySha256 { get; set; } = string.Empty;
        public string AuthorityBattleLogicSha256 { get; set; } = string.Empty;
        public string UnityBattleLogicSha256 { get; set; } = string.Empty;
        public string AuthorityPresentationSha256 { get; set; } = string.Empty;
        public string UnityPresentationSha256 { get; set; } = string.Empty;
        public bool Equal { get; set; }
        public bool BattleLogicEqual { get; set; }
        public bool PresentationEqual { get; set; }
        public ManifestEntryDigest[] AuthorityEntries { get; set; } = [];
        public ManifestEntryDigest[] UnityEntries { get; set; } = [];
    }

    private sealed class ManifestEntryDigest
    {
        public int Oid { get; set; }
        public string Sha256 { get; set; } = string.Empty;
    }

    private sealed class DataAuditSummary
    {
        public int Audited { get; set; }
        public int Equal { get; set; }
        public int Different { get; set; }
        public int MissingAuthority { get; set; }
        public int MissingUnity { get; set; }
        public int Errors { get; set; }
    }

    private sealed class DifferenceCategorySummary
    {
        public string Category { get; set; } = string.Empty;
        public int DifferenceCount { get; set; }
        public int OidCount { get; set; }
        public int[] Oids { get; set; } = [];
    }

    private sealed class ManifestSet
    {
        public SortedDictionary<string, object?> Full { get; } = new(StringComparer.Ordinal);
        public SortedDictionary<string, object?> BattleLogic { get; } = new(StringComparer.Ordinal);
        public SortedDictionary<string, object?> Presentation { get; } = new(StringComparer.Ordinal);
    }

    private sealed class OidAuditResult
    {
        public int Oid { get; set; }
        public int AuthorityType { get; set; }
        public int UnityType { get; set; }
        public string? AuthorityPath { get; set; }
        public string? UnityPath { get; set; }
        public string? AuthorityInputKind { get; set; }
        public string? UnityInputKind { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
        public int[] DuplicateAuthorityFrameIds { get; set; } = [];
        public int[] DuplicateUnityFrameIds { get; set; } = [];
        public List<FieldDifference> Differences { get; set; } = [];
    }
}
