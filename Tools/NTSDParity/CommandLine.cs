namespace NTSDParity;

internal sealed class CommandLine
{
    private readonly Dictionary<string, List<string>> values = new(StringComparer.Ordinal);
    private readonly HashSet<string> flags = new(StringComparer.Ordinal);

    public static CommandLine Parse(string[] args)
    {
        CommandLine parsed = new();
        for (int i = 0; i < args.Length; i++)
        {
            string key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Expected an option, got '{key}'.");
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                parsed.flags.Add(key);
                continue;
            }

            if (!parsed.values.TryGetValue(key, out List<string>? bucket))
            {
                bucket = [];
                parsed.values.Add(key, bucket);
            }
            bucket.Add(args[++i]);
        }
        return parsed;
    }

    public string? Get(string key)
        => values.TryGetValue(key, out List<string>? bucket) ? bucket[^1] : null;

    public IReadOnlyList<string> GetAll(string key)
        => values.TryGetValue(key, out List<string>? bucket) ? bucket : Array.Empty<string>();

    public string Require(string key)
        => Get(key) ?? throw new ArgumentException($"Missing required option '{key}'.");

    public bool Has(string key)
        => flags.Contains(key) || values.ContainsKey(key);
}
