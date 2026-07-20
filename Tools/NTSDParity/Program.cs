namespace NTSDParity;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintUsage();
            return args.Length == 0 ? 2 : 0;
        }

        try
        {
            return args[0] switch
            {
                "data-audit" => DataAuditCommand.Run(args[1..]),
                "trace-authority" => AuthorityTraceCommand.Run(args[1..]),
                "compare" => TraceCompareCommand.Run(args[1..]),
                "self-test" => TraceCompareSelfTestCommand.Run(args[1..]),
                _ => UnknownCommand(args[0]),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static bool IsHelp(string value)
        => value is "-h" or "--help" or "help";

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 2;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("NTSD C# authority parity tools");
        Console.WriteLine();
        Console.WriteLine("data-audit options:");
        Console.WriteLine("  --authority-root <path>  Game data root (default J:\\QQFile\\NTSD2.4)");
        Console.WriteLine("  --unity-root <path>      Unity repository root (auto-detected by default)");
        Console.WriteLine("  --authority-index <path> Authority data.txt override");
        Console.WriteLine("  --unity-index <path>     Unity data.txt override");
        Console.WriteLine("  --oid <id>               Audit one oid (repeatable)");
        Console.WriteLine("  --output <path>          JSON report path");
        Console.WriteLine("  --require-equal          Exit nonzero for any missing/different data");
        Console.WriteLine();
        Console.WriteLine("trace-authority options:");
        Console.WriteLine("  --scenario <path>        Shared JSON scenario");
        Console.WriteLine("  --output <path>          JSONL trace path");
        Console.WriteLine("  --detail compact|full    Compact non-default slots or full state");
        Console.WriteLine();
        Console.WriteLine("compare options:");
        Console.WriteLine("  --authority <path>       Authority JSONL trace");
        Console.WriteLine("  --unity <path>           Unity JSONL trace");
        Console.WriteLine("  --output <path>          First-difference JSON report");
        Console.WriteLine("  --detail hashes|full     Hash-only or field-level first difference");
        Console.WriteLine("  --profile strict|fixed-world-camera  Explicit comparison normalization");
        Console.WriteLine("  --allow-diagnostic       Permit diagnostic fixture comparison (never certifies)");
        Console.WriteLine("  --require-certificate    Exit zero only for an eligible production certificate");
        Console.WriteLine();
        Console.WriteLine("self-test options:");
        Console.WriteLine("  --output <path>          Malicious trace regression report");
    }
}
