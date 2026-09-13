using System.Text;
using System.Text.Json;

namespace NTSD28Parity;

internal static class Program
{
    private static readonly JsonSerializerOptions OutputOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

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
                "contract" => RunContract(args[1..]),
                "b0-domain-contract" => RunB0DomainContract(args[1..]),
                "validate" => RunValidate(args[1..]),
                "validate-b0-domain-raw" =>
                    RunValidateB0DomainRaw(args[1..]),
                "compare-b0-domain-raw" =>
                    RunCompareB0DomainRaw(args[1..]),
                "validate-b2-input-rng-raw" =>
                    RunValidateB2InputRngRaw(args[1..]),
                "compare-b2-input-rng-raw" =>
                    RunCompareB2InputRngRaw(args[1..]),
                "self-test-b2-input-rng-raw" =>
                    RunSelfTestB2InputRngRaw(args[1..]),
                "validate-authority-capture" =>
                    RunValidateAuthorityCapture(args[1..]),
                "compare-raw-entities" =>
                    RunCompareRawEntities(args[1..]),
                "self-test-raw-entities" =>
                    RunSelfTestRawEntities(args[1..]),
                "compare" => RunCompare(args[1..]),
                "self-test" => RunSelfTest(args[1..]),
                "self-test-b0-domain-raw" =>
                    RunSelfTestB0DomainRaw(args[1..]),
                "self-test-b0-domain-comparator" =>
                    RunSelfTestB0DomainComparator(args[1..]),
                _ => UnknownCommand(args[0]),
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static int RunContract(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        WriteJson(outputPath, TraceContract.CreateDescriptor());
        Console.WriteLine(outputPath);
        return 0;
    }

    private static int RunB0DomainContract(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        WriteJson(outputPath, B0DomainRawContract.CreateDescriptor());
        Console.WriteLine(outputPath);
        return 0;
    }

    private static int RunValidateB0DomainRaw(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string capturePath = Path.GetFullPath(commandLine.Require("--capture"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        B0DomainRawValidationReport report =
            B0DomainRawContract.ValidateFile(capturePath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} valid={report.Valid} " +
            $"producer={report.Producer} ticks={report.ValidatedTicks}");
        return report.Valid ? 0 : 1;
    }

    private static int RunCompareB0DomainRaw(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string authorityPath = Path.GetFullPath(
            commandLine.Require("--authority"));
        string unityPath = Path.GetFullPath(commandLine.Require("--unity"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        B0DomainRawComparisonReport report =
            B0DomainRawComparator.CompareFiles(authorityPath, unityPath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} ticks={report.TicksCompared} " +
            $"inputEqual={report.InputEqual} " +
            $"rngTopologyEqual={report.RngTopologyEqual} " +
            $"slotsEqual={report.SlotOccupantsEqual} " +
            $"lifecycleEqual={report.LifecycleEqual} " +
            $"firstDifference={report.FirstDifference?.Path}");
        return report.Differences.Count == 0 ? 0 : 1;
    }

    private static int RunValidate(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string tracePath = Path.GetFullPath(commandLine.Require("--trace"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        TraceValidationReport report = TraceComparator.ValidateFile(tracePath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} valid={report.Valid} ticks={report.ValidatedTicks}");
        return report.Valid ? 0 : 1;
    }

    private static int RunValidateB2InputRngRaw(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string capturePath = Path.GetFullPath(commandLine.Require("--capture"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        B2InputRngJointRawValidationReport report =
            B2InputRngJointRawContract.ValidateFile(capturePath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} valid={report.Valid} " +
            $"producer={report.Producer} ticks={report.ValidatedTicks} " +
            $"entities={report.EntitySnapshots}");
        return report.Valid ? 0 : 1;
    }

    private static int RunCompareB2InputRngRaw(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string authorityPath = Path.GetFullPath(
            commandLine.Require("--authority"));
        string unityPath = Path.GetFullPath(commandLine.Require("--unity"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        B2InputRngJointRawComparisonReport report =
            B2InputRngJointRawComparator.CompareFiles(
                authorityPath,
                unityPath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} equal={report.Equal} " +
            $"ticks={report.TicksCompared} pairs={report.EntityPairsCompared} " +
            $"firstDifference={report.FirstDifference?.Path}");
        return report.Equal ? 0 : 1;
    }

    private static int RunSelfTestB2InputRngRaw(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        B2InputRngJointRawSelfTestReport report =
            B2InputRngJointRawSelfTest.Run();
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"passed={report.Passed} cases={report.Cases.Count} " +
            $"failed={report.Cases.Count(test => !test.Passed)}");
        return report.Passed ? 0 : 1;
    }

    private static int RunCompare(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string authorityPath = Path.GetFullPath(commandLine.Require("--authority"));
        string unityPath = Path.GetFullPath(commandLine.Require("--unity"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        TraceComparisonReport report = TraceComparator.CompareFiles(
            authorityPath,
            unityPath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} certificate={report.CertificateEligible} " +
            $"ticks={report.TicksCompared} firstDifference={report.FirstDifference?.Domain}");
        return report.Status == TraceComparator.EqualStructureStatus ? 0 : 1;
    }

    private static int RunValidateAuthorityCapture(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string capturePath = Path.GetFullPath(
            commandLine.Require("--capture"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        AuthorityCaptureValidationReport report =
            AuthorityCaptureValidator.ValidateFile(capturePath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} valid={report.Valid} " +
            $"ticks={report.ValidatedTicks} entities={report.EntitySnapshots} " +
            $"certificate={report.CertificateEligible}");
        return report.Valid ? 0 : 1;
    }

    private static int RunCompareRawEntities(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string authorityPath = Path.GetFullPath(
            commandLine.Require("--authority"));
        string unityPath = Path.GetFullPath(commandLine.Require("--unity"));
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        RawEntityComparisonReport report =
            RawEntityCaptureComparator.CompareFiles(authorityPath, unityPath);
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} ticks={report.TicksCompared} " +
            $"pairs={report.EntityPairsCompared} " +
            $"fieldOccurrences={report.FieldOccurrencesCompared} " +
            $"differenceOccurrences={report.DifferenceOccurrences} " +
            $"uniqueDifferences={report.UniqueDifferenceFields} " +
            $"uniqueEqual={report.UniqueEqualFields} " +
            $"firstDifference={report.FirstDifference?.Path}");
        return report.Status == "equal-raw" ? 0 : 1;
    }

    private static int RunSelfTestRawEntities(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        RawEntityComparisonSelfTestReport report =
            RawEntityCaptureComparator.RunSelfTest();
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"passed={report.Passed} cases={report.Cases.Count} " +
            $"failed={report.Cases.Count(test => !test.Passed)}");
        return report.Passed ? 0 : 1;
    }

    private static int RunSelfTest(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        TraceSelfTestReport report = TraceContractSelfTest.Run();
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"passed={report.Passed} cases={report.Cases.Count} " +
            $"failed={report.Cases.Count(test => !test.Passed)}");
        return report.Passed ? 0 : 1;
    }

    private static int RunSelfTestB0DomainRaw(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        B0DomainRawSelfTestReport report = B0DomainRawContractSelfTest.Run();
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"passed={report.Passed} cases={report.Cases.Count} " +
            $"failed={report.Cases.Count(test => !test.Passed)}");
        return report.Passed ? 0 : 1;
    }

    private static int RunSelfTestB0DomainComparator(string[] args)
    {
        CommandLine commandLine = CommandLine.Parse(args);
        string outputPath = ResolveOutput(commandLine.Require("--output"));
        B0DomainComparatorSelfTestReport report =
            B0DomainRawComparatorSelfTest.Run();
        WriteJson(outputPath, report);
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"passed={report.Passed} cases={report.Cases.Count} " +
            $"failed={report.Cases.Count(test => !test.Passed)}");
        return report.Passed ? 0 : 1;
    }

    private static bool IsHelp(string value)
    {
        return value is "-h" or "--help" or "help";
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 2;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("NTSD 2.8-Logan trace contract and first-difference tool");
        Console.WriteLine();
        Console.WriteLine("contract --output <contract.json>");
        Console.WriteLine("b0-domain-contract --output <contract.json>");
        Console.WriteLine("validate --trace <trace.jsonl> --output <report.json>");
        Console.WriteLine(
            "validate-b0-domain-raw --capture <raw.jsonl> --output <report.json>");
        Console.WriteLine(
            "compare-b0-domain-raw --authority <raw.jsonl> " +
            "--unity <raw.jsonl> --output <report.json>");
        Console.WriteLine(
            "validate-b2-input-rng-raw --capture <raw.jsonl> " +
            "--output <report.json>");
        Console.WriteLine(
            "compare-b2-input-rng-raw --authority <raw.jsonl> " +
            "--unity <raw.jsonl> --output <report.json>");
        Console.WriteLine(
            "self-test-b2-input-rng-raw --output <report.json>");
        Console.WriteLine(
            "validate-authority-capture --capture <raw.jsonl> " +
            "--output <report.json>");
        Console.WriteLine(
            "compare-raw-entities --authority <authority.raw.jsonl> " +
            "--unity <unity.raw.jsonl> --output <report.json>");
        Console.WriteLine(
            "self-test-raw-entities --output <report.json>");
        Console.WriteLine(
            "compare --authority <authority.jsonl> --unity <unity.jsonl> " +
            "--output <report.json>");
        Console.WriteLine("self-test --output <report.json>");
        Console.WriteLine("self-test-b0-domain-raw --output <report.json>");
        Console.WriteLine(
            "self-test-b0-domain-comparator --output <report.json>");
    }

    private static string ResolveOutput(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return fullPath;
    }

    private static void WriteJson<T>(string path, T value)
    {
        string json = JsonSerializer.Serialize(value, OutputOptions);
        File.WriteAllText(path, json + Environment.NewLine, new UTF8Encoding(false));
    }

    private sealed class CommandLine
    {
        private readonly Dictionary<string, string> values;

        private CommandLine(Dictionary<string, string> values)
        {
            this.values = values;
        }

        public static CommandLine Parse(string[] args)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < args.Length; index++)
            {
                string key = args[index];
                if (!key.StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Unexpected argument: {key}");
                }

                if (index + 1 >= args.Length ||
                    args[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Missing value for {key}.");
                }

                if (!values.TryAdd(key, args[++index]))
                {
                    throw new ArgumentException($"Duplicate option: {key}");
                }
            }

            return new CommandLine(values);
        }

        public string Require(string key)
        {
            if (!values.TryGetValue(key, out string? value) ||
                string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Missing required option {key}.");
            }

            return value;
        }
    }
}
