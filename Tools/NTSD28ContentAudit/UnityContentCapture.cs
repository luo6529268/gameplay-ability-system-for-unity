using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;

internal static class UnityContentCapture
{
    private const string AuditId = "NTSD28-B11-CONTENT-ENTRY-INVENTORY-001";
    private const string ProjectDirectoryName = "gameplay-ability-system-for-unity";
    private const string UnityOutputRelative = "Temp/NTSD28ContentAudit/unity";
    private const string ArtifactOutputRelative =
        "artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/unity";
    private const string DecodedOutputRelative = "Temp/NTSD28ContentAudit/decoded-current";
    private const string UnityDatPassword =
        "odBearBecauseHeIsVeryGoodSiuHungIsAGo";

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
    };

    private static readonly string[] ProductionSourceRelativePaths =
    {
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatProperty.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatBlock.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatSubBlock.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatFile.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2WeaponStrengthRow.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganStrength.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganNumericDecoder.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatEnums.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2BmpSection.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2FrameBlock.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2SpriteFileDef.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatTokenizer.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganFrames.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatDecryptor.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs",
        "Assets/NTSD/Scripts/Animation/LF2FrameData.cs",
        "Assets/NTSD/Scripts/Animation/LF2CharacterData.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganHeaders.cs",
        "Assets/NTSD/Scripts/Animation/LoganDefinitionMetadata.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2LoganDefinitionBlocks.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganDefinitions.cs",
        "Assets/NTSD/Scripts/Animation/LoganWeaponPieceDefinition.cs",


        "Assets/NTSD/Scripts/Animation/LF2ArmorData.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/BloodPoint/BattleBloodPointValue.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/BloodPoint/BattleBloodPointCatalog.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointValue.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointCatalog.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointValueAdapter.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/BodyBox/BattleBodyBoxValue.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/BodyBox/BattleBodyBoxValueAdapter.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/ObjectPoint/BattleObjectPointValue.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/ObjectPoint/BattleObjectPointValueAdapter.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/WeaponPoint/BattleWeaponPointValue.cs",
        "Assets/NTSD/Scripts/Simulation/DataContracts/WeaponPoint/BattleWeaponPointValueAdapter.cs",
    };

    private static int Main(string[] args)
    {
        try
        {
            Arguments arguments = Arguments.Parse(args);
            if (arguments.SelfTest)
            {
                RunSelfTest();
                Console.WriteLine("self-test=PASS");
                return 0;
            }

            string repositoryRoot = FindRepositoryRoot();
            string inputRoot = FullPath(arguments.InputRoot);
            string outputPath = FullPath(arguments.Output);
            ValidateInputRoot(inputRoot);
            ValidateOutputPath(repositoryRoot, inputRoot, outputPath);

            string decodedOutputRoot = null;
            if (!string.IsNullOrWhiteSpace(arguments.DecodedOutputRoot))
            {
                decodedOutputRoot = FullPath(arguments.DecodedOutputRoot);
                ValidateDecodedOutputRoot(repositoryRoot, inputRoot, decodedOutputRoot);
                Directory.CreateDirectory(decodedOutputRoot);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            Dictionary<string, string> sourceHashes =
                HashProductionSources(repositoryRoot);
            List<string> files = Directory
                .EnumerateFiles(inputRoot, "*.dat", SearchOption.AllDirectories)
                .OrderBy(path => NormalizePath(Path.GetRelativePath(inputRoot, path)),
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => NormalizePath(Path.GetRelativePath(inputRoot, path)),
                    StringComparer.Ordinal)
                .ToList();

            int parseSuccessCount = 0;
            int parseFailureCount = 0;
            int converterFailureFileCount = 0;
            var lines = new List<string>(files.Count);
            foreach (string filePath in files)
            {
                Dictionary<string, object> record = CaptureFile(
                    repositoryRoot,
                    inputRoot,
                    filePath,
                    arguments.InputMode,
                    decodedOutputRoot,
                    sourceHashes);
                if (GetBool(record, "parseSuccess"))
                    parseSuccessCount++;
                else
                    parseFailureCount++;

                if (GetString(record, "fullConversionError") != null)
                    converterFailureFileCount++;

                record.Remove("fullConversionError");
                lines.Add(JsonSerializer.Serialize(record, JsonOptions));
            }

            File.WriteAllText(outputPath, string.Join(Environment.NewLine, lines) +
                (lines.Count == 0 ? string.Empty : Environment.NewLine),
                new UTF8Encoding(false));
            Console.WriteLine(
                $"files={files.Count} parseSuccess={parseSuccessCount} " +
                $"parseFailure={parseFailureCount} " +
                $"converterFailureFiles={converterFailureFileCount}");
            Console.WriteLine($"output={outputPath}");
            return parseFailureCount == 0 ? 0 : 2;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"argument-error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"fatal: {ex.GetType().FullName}: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private sealed class Arguments
    {
        public string InputRoot;
        public string InputMode;
        public string Output;
        public string DecodedOutputRoot;
        public bool SelfTest;

        public static Arguments Parse(string[] args)
        {
            var result = new Arguments();
            for (int index = 0; index < args.Length; index++)
            {
                string arg = args[index];
                if (string.Equals(arg, "--self-test", StringComparison.OrdinalIgnoreCase))
                {
                    result.SelfTest = true;
                    continue;
                }

                if (!arg.StartsWith("--", StringComparison.Ordinal) ||
                    index + 1 >= args.Length)
                {
                    throw new ArgumentException($"无法识别参数: {arg}");
                }

                string value = args[++index];
                switch (arg.ToLowerInvariant())
                {
                    case "--input-root":
                        result.InputRoot = value;
                        break;
                    case "--input-mode":
                        result.InputMode = value;
                        break;
                    case "--output":
                        result.Output = value;
                        break;
                    case "--decoded-output-root":
                        result.DecodedOutputRoot = value;
                        break;
                    default:
                        throw new ArgumentException($"无法识别参数: {arg}");
                }
            }

            if (result.SelfTest)
                return result;
            if (string.IsNullOrWhiteSpace(result.InputRoot) ||
                string.IsNullOrWhiteSpace(result.InputMode) ||
                string.IsNullOrWhiteSpace(result.Output))
            {
                throw new ArgumentException(
                    "需要 --input-root、--input-mode plaintext|unity 和 --output。");
            }

            result.InputMode = result.InputMode.Trim().ToLowerInvariant();
            if (result.InputMode != "plaintext" && result.InputMode != "unity")
                throw new ArgumentException(
                    $"--input-mode 必须是 plaintext 或 unity，实际为 {result.InputMode}。");
            return result;
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo current = new DirectoryInfo(Environment.CurrentDirectory);
        while (current != null)
        {
            string marker = Path.Combine(current.FullName, "ProjectSettings",
                "ProjectVersion.txt");
            if (string.Equals(current.Name, ProjectDirectoryName,
                    StringComparison.OrdinalIgnoreCase) &&
                File.Exists(marker))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new InvalidOperationException(
            "无法从当前目录定位 Unity 项目根目录。");
    }

    private static string FullPath(string path)
    {
        return Path.GetFullPath(path ?? string.Empty);
    }

    private static void ValidateInputRoot(string inputRoot)
    {
        if (!Directory.Exists(inputRoot))
            throw new ArgumentException($"输入目录不存在: {inputRoot}");
    }

    private static void ValidateOutputPath(
        string repositoryRoot,
        string inputRoot,
        string outputPath)
    {
        if (IsWithin(inputRoot, outputPath))
            throw new ArgumentException("输出文件不能位于输入目录内。");

        string unityRoot = FullPath(Path.Combine(repositoryRoot, UnityOutputRelative));
        string artifactRoot =
            FullPath(Path.Combine(repositoryRoot, ArtifactOutputRelative));
        if (!IsWithin(unityRoot, outputPath) &&
            !IsWithin(artifactRoot, outputPath))
        {
            throw new ArgumentException(
                "输出文件必须位于 Temp/NTSD28ContentAudit/unity 或 " +
                "artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/unity。");
        }
    }

    private static void ValidateDecodedOutputRoot(
        string repositoryRoot,
        string inputRoot,
        string decodedOutputRoot)
    {
        if (IsWithin(inputRoot, decodedOutputRoot))
            throw new ArgumentException("decoded 输出目录不能位于输入目录内。");

        string permittedRoot =
            FullPath(Path.Combine(repositoryRoot, DecodedOutputRelative));
        if (!IsWithin(permittedRoot, decodedOutputRoot))
        {
            throw new ArgumentException(
                "decoded 输出目录必须位于 Temp/NTSD28ContentAudit/decoded-current。");
        }
    }

    private static bool IsWithin(string root, string candidate)
    {
        string rootWithSeparator = EnsureTrailingSeparator(FullPath(root));
        string fullCandidate = FullPath(candidate);
        return fullCandidate.StartsWith(rootWithSeparator,
            StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                fullCandidate.TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                rootWithSeparator.TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(),
                StringComparison.Ordinal) ||
            path.EndsWith(Path.AltDirectorySeparatorChar.ToString(),
                StringComparison.Ordinal))
            return path;
        return path + Path.DirectorySeparatorChar;
    }

    private static Dictionary<string, string> HashProductionSources(
        string repositoryRoot)
    {
        var result = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        foreach (string relativePath in ProductionSourceRelativePaths)
        {
            string fullPath = Path.Combine(repositoryRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
                throw new FileNotFoundException(
                    $"链接的正式源文件不存在: {relativePath}", fullPath);

            using FileStream stream = File.OpenRead(fullPath);
            byte[] hash = SHA256.HashData(stream);
            result[NormalizePath(relativePath)] =
                Convert.ToHexString(hash).ToLowerInvariant();
        }
        return result;
    }

    private static Dictionary<string, object> CaptureFile(
        string repositoryRoot,
        string inputRoot,
        string filePath,
        string inputMode,
        string decodedOutputRoot,
        Dictionary<string, string> sourceHashes)
    {
        UnityEngine.Debug.ClearAuditSink();
        string relativePath = NormalizePath(Path.GetRelativePath(inputRoot, filePath));
        var result = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["auditId"] = AuditId,
            ["path"] = relativePath,
            ["inputMode"] = inputMode,
            ["inputIdentity"] = new Dictionary<string, object>
            {
                ["absoluteRoot"] = inputRoot,
                ["sha256"] = Sha256File(filePath),
            },
            ["parseSuccess"] = false,
            ["parserReturned"] = false,
            ["parserDiagnosticErrors"] = null,
            ["fullConverterSuccess"] = null,
            ["isolatedSubblockFailures"] = null,
            ["diagnostics"] = new List<string>(),
            ["top"] = new List<object>(),
            ["bmp"] = new List<object>(),
            ["stats"] = new List<object>(),
            ["auditStats"] = new List<object>(),
            ["menuFace"] = new List<object>(),
            ["menuFaceClassification"] = null,
            ["menuFaceCaptureStatus"] = null,
            ["sprites"] = new List<object>(),
            ["frames"] = new List<object>(),
            ["blocks"] = new List<object>(),
            ["source"] = BuildSourceMetadata(repositoryRoot, sourceHashes),
        };
        var diagnostics = (List<string>)result["diagnostics"];
        var stats = (List<object>)result["auditStats"];
        long bytes = new FileInfo(filePath).Length;
        AddStat(stats, "inputBytes", bytes);

        string text;
        if (inputMode == "unity")
        {
            result["decryption"] = new Dictionary<string, object>
            {
                ["attempted"] = true,
                ["method"] = "NTSD.DatParser.Lf2DatDecryptor.DecryptFile",
                ["keyId"] = "odBearBecauseHeIsVeryGoodSiuHungIsAGo",
                ["success"] = false,
            };
            try
            {
                text = Lf2DatDecryptor.DecryptFile(filePath, UnityDatPassword);
            }
            catch (Exception ex)
            {
                diagnostics.Add($"解密调用异常: {ErrorText(ex)}");
                AddStat(stats, "decryptionSuccess", false);
                AppendUnityDiagnostics(diagnostics);
                return result;
            }

            bool recognized = LooksLikeDatText(text);
            ((Dictionary<string, object>)result["decryption"])["success"] = recognized;
            AddStat(stats, "decryptionSuccess", recognized);
            if (!recognized)
            {
                diagnostics.Add(
                    "真实 Decryptor 未产生可识别 DAT 根标签；未把原始字节当作成功解析。");
                AppendUnityDiagnostics(diagnostics);
                return result;
            }

            if (!string.IsNullOrWhiteSpace(decodedOutputRoot))
            {
                string decodedPath = SafeDecodedPath(
                    decodedOutputRoot, relativePath, inputRoot);
                Directory.CreateDirectory(Path.GetDirectoryName(decodedPath));
                File.WriteAllText(decodedPath, text, new UTF8Encoding(false));
                ((Dictionary<string, object>)result["decryption"])["decodedPath"] =
                    NormalizePath(Path.GetRelativePath(repositoryRoot, decodedPath));
            }
        }
        else
        {
            try
            {
                text = File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                diagnostics.Add($"读取明文异常: {ErrorText(ex)}");
                AppendUnityDiagnostics(diagnostics);
                return result;
            }
            result["decryption"] = new Dictionary<string, object>
            {
                ["attempted"] = false,
                ["success"] = true,
                ["method"] = "direct UTF-8 read",
            };
            AddStat(stats, "decryptionSuccess", true);
        }

        Lf2DatFile dat;
        try
        {
            dat = new Lf2DatParserV2().Parse(text, filePath);
        }
        catch (Exception ex)
        {
            diagnostics.Add($"ParserV2 异常: {ErrorText(ex)}");
            AppendUnityDiagnostics(diagnostics);
            return result;
        }

        result["parseSuccess"] = true;
        result["parserReturned"] = true;
        result["parserDiagnosticErrors"] =
            CountUnityDiagnostics("Error");
        result["fileType"] = dat.FileType.ToString();
        List<object> menuFace = ProjectMenuFace(
            dat,
            out bool hasMenuFace,
            out string menuFaceCaptureStatus);
        result["menuFace"] = menuFace;
        result["menuFaceCaptureStatus"] = menuFaceCaptureStatus;
        if (hasMenuFace)
        {
            result["menuFaceClassification"] = "NON_BATTLE_MENU_REVIEW";
        }
        ProjectDat(dat, result, diagnostics, stats);
        AppendUnityDiagnostics(diagnostics);
        return result;
    }

    private static void AppendUnityDiagnostics(List<string> diagnostics)
    {
        foreach (UnityEngine.DiagnosticEntry entry in UnityEngine.Debug.Entries)
        {
            diagnostics.Add($"UnityEngine.Debug.{entry.Level}: {entry.Message}");
        }
    }

    private static int CountUnityDiagnostics(string level)
    {
        int count = 0;
        foreach (UnityEngine.DiagnosticEntry entry in UnityEngine.Debug.Entries)
        {
            if (string.Equals(entry.Level, level,
                    StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }
        return count;
    }

    private static string SafeDecodedPath(
        string decodedOutputRoot,
        string relativePath,
        string inputRoot)
    {
        string output = FullPath(Path.Combine(
            decodedOutputRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (IsWithin(inputRoot, output) ||
            !IsWithin(decodedOutputRoot, output))
        {
            throw new InvalidOperationException(
                $"拒绝不安全的 decoded 输出路径: {output}");
        }
        return output;
    }

    private static Dictionary<string, object> BuildSourceMetadata(
        string repositoryRoot,
        Dictionary<string, string> sourceHashes)
    {
        var linkedSources = new Dictionary<string, object>(
            StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> pair in sourceHashes)
            linkedSources[pair.Key] = pair.Value;

        return new Dictionary<string, object>
        {
            ["loadsLibraryScriptAssemblies"] = false,
            ["linkedProductionSourceSha256"] = linkedSources,
            ["stubBoundary"] = new Dictionary<string, object>
            {
                ["file"] = "Tools/NTSD28ContentAudit/UnityDiagnosticStubs.cs",
                ["sha256"] = Sha256File(Path.Combine(
                    repositoryRoot,
                    "Tools/NTSD28ContentAudit/UnityDiagnosticStubs.cs")),
                ["allowedTypes"] = new[]
                {
                    "UnityEngine.HeaderAttribute",
                    "UnityEngine.CreateAssetMenuAttribute",
                    "UnityEngine.ScriptableObject",
                    "UnityEngine.Debug",
                    "UnityEngine.JsonUtility",
                },
                ["boundary"] =
                    "Only compile-time/runtime no-op stubs for linked parser/model code; " +
                    "no Unity scene, asset database, renderer, or Library DLL access.",
            },
        };
    }

    private static string Sha256File(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static bool LooksLikeDatText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
        string[] roots =
        {
            "<bmp_begin>",
            "<frame>",
            "<stage>",
            "<object>",
            "<background>",
        };
        foreach (string root in roots)
        {
            if (text.IndexOf(root, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    private static bool GetBool(
        Dictionary<string, object> record,
        string key)
    {
        return record.TryGetValue(key, out object value) &&
            value is bool flag && flag;
    }

    private static string GetString(
        Dictionary<string, object> record,
        string key)
    {
        return record.TryGetValue(key, out object value) ? value as string : null;
    }

    private static string NormalizePath(string path)
    {
        return (path ?? string.Empty).Replace('\\', '/');
    }

    private static string ErrorText(Exception ex)
    {
        return $"{ex.GetType().FullName}: {ex.Message}";
    }

    private static void AddStat(
        List<object> stats,
        string key,
        object value)
    {
        stats.Add(new Dictionary<string, object>
        {
            ["key"] = key,
            ["value"] = value,
        });
    }

    private static void ProjectDat(
        Lf2DatFile dat,
        Dictionary<string, object> result,
        List<string> diagnostics,
        List<object> stats)
    {
        result["top"] = ProjectProperties(dat.Properties);
        result["stats"] = ProjectStatsProperties(dat.Blocks);
        AddStat(stats, "fileType", dat.FileType.ToString());
        AddStat(stats, "topPropertyCount", dat.Properties?.Count ?? 0);
        AddStat(stats, "frameCount", dat.Frames?.Count ?? 0);
        AddStat(stats, "blockCount", dat.Blocks?.Count ?? 0);

        if (dat.Bmp != null)
        {
            List<object> sprites = ProjectSprites(dat.Bmp.Files);
            result["bmp"] = ProjectProperties(dat.Bmp.Properties);
            result["bmpHeader"] = new Dictionary<string, object>
            {
                ["name"] = dat.Bmp.Name,
                ["head"] = dat.Bmp.Head,
                ["small"] = dat.Bmp.Small,
            };
            result["bmpFrameSequences"] =
                ProjectFrameSequences(dat.Bmp.FrameSequences);
            result["sprites"] = sprites;
            AddStat(stats, "bmpFileCount", dat.Bmp.Files?.Count ?? 0);
            AddStat(stats, "spriteEffectiveRangeSource",
                "Lf2DatParserV2 AST declaration; no Unity texture importer executed");
        }
        else
        {
            AddStat(stats, "bmpFileCount", 0);
        }

        var frameRecords = (List<object>)result["frames"];
        int fullConversionSuccesses = 0;
        int fullConversionFailures = 0;
        int isolatedConversionSuccesses = 0;
        int isolatedConversionFailures = 0;
        int subblockCount = 0;

        if (dat.Frames != null)
        {
            foreach (Lf2FrameBlock frame in dat.Frames)
            {
                var frameRecord = new Dictionary<string, object>
                {
                    ["id"] = frame.FrameIndex,
                    ["name"] = frame.FrameName,
                    ["fields"] = ProjectProperties(frame.Properties),
                    ["subblocks"] = new List<object>(),
                    ["conversionError"] = null,
                };
                string fullConversionError = null;
                try
                {
                    Lf2DatConverter.ConvertToFrameData(frame);
                    fullConversionSuccesses++;
                }
                catch (Exception ex)
                {
                    fullConversionFailures++;
                    fullConversionError = ErrorText(ex);
                    frameRecord["conversionError"] = fullConversionError;
                    diagnostics.Add(
                        $"frame {frame.FrameIndex} full Converter 异常: {fullConversionError}");
                }

                var projectedSubblocks = (List<object>)frameRecord["subblocks"];
                if (frame.SubBlocks != null)
                {
                    foreach (Lf2DatSubBlock subBlock in frame.SubBlocks)
                    {
                        subblockCount++;
                        Dictionary<string, object> projected = ProjectSubblock(
                            frame.Properties, subBlock, out bool converted);
                        projectedSubblocks.Add(projected);
                        if (converted)
                            isolatedConversionSuccesses++;
                        else
                            isolatedConversionFailures++;
                    }
                }

                if (fullConversionError != null)
                    result["fullConversionError"] = fullConversionError;
                frameRecords.Add(frameRecord);
            }
        }

        var blockRecords = (List<object>)result["blocks"];
        if (dat.Blocks != null)
        {
            foreach (Lf2DatBlock block in dat.Blocks)
            {
                var blockRecord = new Dictionary<string, object>
                {
                    ["kind"] = block.Name,
                    ["fields"] = ProjectProperties(block.Properties),
                    ["subblocks"] = new List<object>(),
                };
                var projectedSubblocks = (List<object>)blockRecord["subblocks"];
                if (block.SubBlocks != null)
                {
                    foreach (Lf2DatSubBlock subBlock in block.SubBlocks)
                    {
                        subblockCount++;
                        Dictionary<string, object> projected = ProjectSubblock(
                            block.Properties, subBlock, out bool converted);
                        projectedSubblocks.Add(projected);
                        if (converted)
                            isolatedConversionSuccesses++;
                        else
                            isolatedConversionFailures++;
                    }
                }
                blockRecords.Add(blockRecord);
            }
        }

        AddStat(stats, "subblockCount", subblockCount);
        AddStat(stats, "fullFrameConversionSuccesses", fullConversionSuccesses);
        AddStat(stats, "fullFrameConversionFailures", fullConversionFailures);
        AddStat(stats, "isolatedSubblockConversionSuccesses",
            isolatedConversionSuccesses);
        AddStat(stats, "isolatedSubblockConversionFailures",
            isolatedConversionFailures);
        result["fullConverterSuccess"] = fullConversionFailures == 0;
        result["isolatedSubblockFailures"] = isolatedConversionFailures;

        var characterData = new LF2CharacterData();
        try
        {
            Lf2DatConverter.ApplyNativeInputDefinitionData(dat, characterData);
            AddStat(stats, "nativeInputDefinitionConversionSuccess", true);
        }
        catch (Exception ex)
        {
            AddStat(stats, "nativeInputDefinitionConversionSuccess", false);
            diagnostics.Add($"Native input definition Converter 异常: {ErrorText(ex)}");
        }

        try
        {
            Lf2DatConverter.ApplyNativeArmorDefinitionData(dat, characterData);
            AddStat(stats, "nativeArmorDefinitionConversionSuccess", true);
        }
        catch (Exception ex)
        {
            AddStat(stats, "nativeArmorDefinitionConversionSuccess", false);
            diagnostics.Add($"Native armor definition Converter 异常: {ErrorText(ex)}");
        }
    }

    private static List<object> ProjectStatsProperties(
        IEnumerable<Lf2DatBlock> blocks)
    {
        var result = new List<object>();
        if (blocks == null)
            return result;

        foreach (Lf2DatBlock block in blocks)
        {
            if (block != null &&
                string.Equals(block.Name, "stats",
                    StringComparison.OrdinalIgnoreCase))
            {
                result.AddRange(ProjectProperties(block.Properties));
            }
        }
        return result;
    }

    private static List<object> ProjectMenuFace(
        Lf2DatFile dat,
        out bool present,
        out string captureStatus)
    {
        var result = new List<object>();
        present = false;
        captureStatus = null;
        if (dat?.Blocks == null)
            return result;

        foreach (Lf2DatBlock block in dat.Blocks)
        {
            if (!string.Equals(block?.Name, "menu_face",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            present = true;
            if (block.SubBlocks == null)
                continue;

            foreach (Lf2DatSubBlock layer in block.SubBlocks)
            {
                if (!string.Equals(layer?.Name, "layer",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(new Dictionary<string, object>
                {
                    ["fields"] = ProjectProperties(layer.Properties),
                });
            }
        }

        captureStatus = result.Count > 0
            ? "AVAILABLE_IN_PRODUCTION_AST"
            : present
                ? "UNAVAILABLE_IN_PRODUCTION_AST"
                : null;
        return result;
    }

    private static List<object> ProjectProperties(
        IEnumerable<Lf2DatProperty> properties)
    {
        var result = new List<object>();
        if (properties == null)
            return result;

        foreach (Lf2DatProperty property in properties)
        {
            result.Add(new Dictionary<string, object>
            {
                ["key"] = property?.Key,
                ["value"] = property?.Value,
            });
        }
        return result;
    }

    private static List<object> ProjectFrameSequences(
        IEnumerable<Lf2BmpFrameSequence> sequences)
    {
        var result = new List<object>();
        if (sequences == null)
            return result;

        foreach (Lf2BmpFrameSequence sequence in sequences)
        {
            result.Add(new Dictionary<string, object>
            {
                ["name"] = sequence?.Name,
                ["actions"] = sequence?.Actions ?? new List<int>(),
            });
        }
        return result;
    }

    private static List<object> ProjectSprites(
        IEnumerable<Lf2SpriteFileDef> files)
    {
        var result = new List<object>();
        if (files == null)
            return result;

        foreach (Lf2SpriteFileDef file in files)
        {
            if (file == null)
                continue;
            result.Add(new Dictionary<string, object>
            {
                ["path"] = NormalizePath(file.Path),
                ["width"] = file.Width,
                ["height"] = file.Height,
                ["row"] = file.Row,
                ["col"] = file.Col,
                ["declaredFirst"] = file.StartIndex,
                ["declaredLast"] = file.EndIndex,
                ["effectiveFirst"] = file.StartIndex,
                ["effectiveLast"] = file.EndIndex,
            });
        }
        return result;
    }

    private static Dictionary<string, object> ProjectSubblock(
        IEnumerable<Lf2DatProperty> frameProperties,
        Lf2DatSubBlock subBlock,
        out bool converted)
    {
        var result = new Dictionary<string, object>
        {
            ["kind"] = subBlock?.Name,
            ["fields"] = ProjectProperties(subBlock?.Properties),
            ["normalized"] = null,
            ["conversionError"] = null,
        };
        converted = false;
        if (subBlock == null)
        {
            result["conversionError"] = "null subblock";
            return result;
        }

        try
        {
            Lf2FrameBlock isolated = BuildIsolatedFrame(
                frameProperties, subBlock);
            LF2FrameData convertedFrame =
                Lf2DatConverter.ConvertToFrameData(isolated);
            result["normalized"] = ProjectConvertedSubblock(
                subBlock, convertedFrame);
            converted = true;
        }
        catch (Exception ex)
        {
            result["conversionError"] = ErrorText(ex);
        }
        return result;
    }

    private static Dictionary<string, object> ProjectConvertedSubblock(
        Lf2DatSubBlock subBlock,
        LF2FrameData convertedFrame)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        string kind = subBlock?.Name?.Trim().ToLowerInvariant();
        if (convertedFrame == null || string.IsNullOrWhiteSpace(kind))
            return result;

        Dictionary<string, object> actual;
        switch (kind)
        {
            case "opoint":
                actual = convertedFrame.opoints.Count == 0
                    ? new Dictionary<string, object>()
                    : ObjectPointValues(convertedFrame.opoints[0]);
                break;
            case "cpoint":
                actual = convertedFrame.CatchPoints.Count == 0
                    ? new Dictionary<string, object>()
                    : CatchPointValues(convertedFrame.CatchPoints[0]);
                break;
            case "wpoint":
                actual = convertedFrame.FormalWeaponPoints.Count == 0
                    ? new Dictionary<string, object>()
                    : WeaponPointValues(convertedFrame.FormalWeaponPoints[0]);
                break;
            case "bdy":
                actual = convertedFrame.bodies.Count == 0
                    ? new Dictionary<string, object>()
                    : BodyBoxValues(
                        convertedFrame.bodies[0],
                        convertedFrame.PrimaryBodyKind,
                        convertedFrame.PrimaryBodyRespond);
                break;
            case "bpoint":
                actual = convertedFrame.BloodPoints.Count == 0
                    ? new Dictionary<string, object>()
                    : BloodPointValues(convertedFrame.BloodPoints[0]);
                break;
            case "itr":
                actual = convertedFrame.itrs.Count == 0
                    ? new Dictionary<string, object>()
                    : InteractionValues(convertedFrame.itrs[0]);
                break;
            default:
                return result;
        }

        return actual;
    }

    private static Dictionary<string, object> ObjectPointValues(
        BattleObjectPointValue point)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = point.Kind,
            ["x"] = point.X,
            ["y"] = point.Y,
            ["action"] = point.Action,
            ["dvx"] = point.Dvx,
            ["dvy"] = point.Dvy,
            ["oid"] = point.Oid,
            ["facing"] = point.Facing,
        };
    }

    private static Dictionary<string, object> CatchPointValues(
        BattleCatchPointValue point)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = point.Kind,
            ["x"] = point.X,
            ["y"] = point.Y,
            ["injury"] = point.Injury,
            ["cover"] = point.Cover,
            ["vaction"] = point.Vaction,
            ["aaction"] = point.Aaction,
            ["jaction"] = point.Jaction,
            ["daction"] = point.Daction,
            ["throwvx"] = point.ThrowVx,
            ["throwvy"] = point.ThrowVy,
            ["hurtable"] = point.Hurtable,
            ["decrease"] = point.Decrease,
            ["dircontrol"] = point.DirControl,
            ["taction"] = point.Taction,
            ["throwinjury"] = point.ThrowInjury,
            ["throwvz"] = point.ThrowVz,
            ["fronthurtact"] = point.FrontHurtAct,
            ["backhurtact"] = point.BackHurtAct,
        };
    }

    private static Dictionary<string, object> WeaponPointValues(
        BattleWeaponPointValue point)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = point.Kind,
            ["x"] = point.X,
            ["y"] = point.Y,
            ["attacking"] = point.Attacking,
            ["cover"] = point.Cover,
            ["weaponact"] = point.WeaponAct,
            ["dvx"] = point.Dvx,
            ["dvy"] = point.Dvy,
            ["dvz"] = point.Dvz,
        };
    }

    private static Dictionary<string, object> BodyBoxValues(
        BattleBodyBoxValue box,
        int kind,
        int respond)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = kind,
            ["x"] = box.X,
            ["y"] = box.Y,
            ["w"] = box.W,
            ["h"] = box.H,
            ["respond"] = respond,
        };
    }

    private static Dictionary<string, object> BloodPointValues(
        BattleBloodPointValue point)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["x"] = point.X,
            ["y"] = point.Y,
        };
    }

    private static Dictionary<string, object> InteractionValues(
        InteractionArea area)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = area.kind,
            ["x"] = area.x,
            ["y"] = area.y,
            ["w"] = area.w,
            ["h"] = area.h,
            ["zwidth"] = area.zwidth,
            ["dvx"] = area.dvx,
            ["dvy"] = area.dvy,
            ["dvz"] = area.dvz,
            ["injury"] = area.injury,
            ["fall"] = area.fall,
            ["vaction"] = area.vaction,
            ["arest"] = area.arest,
            ["vrest"] = area.vrest,
            ["effect"] = area.effect,
            ["spark"] = area.spark,
            ["recover"] = area.recover,
            ["dbdefend"] = area.dbdefend,
            ["kill"] = area.kill,
            ["bdefend"] = area.bdefend,
            ["attacking"] = area.attacking,
            ["respond"] = area.respond,
            ["pickingact"] = area.pickingact,
            ["pickedact"] = area.pickedact,
            ["delay"] = area.delay,
            ["poison"] = area.poison,
            ["confus"] = area.confus,
            ["weak"] = area.weak,
            ["manacle"] = area.manacle,
            ["join"] = area.join,
            ["mimic"] = area.mimic,
            ["bound"] = area.bound,
            ["facing"] = area.facing,
            ["dx"] = area.dx,
            ["dy"] = area.dy,
            ["dz"] = area.dz,
            ["gain"] = area.gain,
            ["throwvx"] = area.throwvx,
            ["throwvy"] = area.throwvy,
            ["throwinjury"] = area.throwinjury,
            ["throwvz"] = area.throwvz,
            ["catchingact"] = area.catchingact,
            ["catchingact2"] = area.catchingact2,
            ["caughtact"] = area.caughtact,
            ["caughtact2"] = area.caughtact2,
        };
    }

    private static Lf2FrameBlock BuildIsolatedFrame(
        IEnumerable<Lf2DatProperty> frameProperties,
        Lf2DatSubBlock subBlock)
    {
        var frame = new Lf2FrameBlock
        {
            FrameIndex = -1,
            FrameName = "isolated_subblock",
        };
        if (frameProperties != null)
        {
            foreach (Lf2DatProperty property in frameProperties)
            {
                frame.AddProperty(new Lf2DatProperty(
                    property?.Key, property?.Value));
            }
        }
        frame.SubBlocks.Add(subBlock);
        return frame;
    }

    private static void RunSelfTest()
    {
        string fixture = string.Join("\n", new[]
        {
            "<bmp_begin>",
            "name: fixture",
            "head: fixture.bmp",
            "small: 0",
            "file(0-1): fixture.bmp w: 16 h: 16 row: 1 col: 2",
            "<bmp_end>",
            "<frame> 0 standing",
            "pic: 0",
            "cpoint:",
            "fronthurtact: 100",
            "cpoint_end:",
            "opoint:",
            "kind: 1",
            "x: 5",
            "y: 6",
            "oid: 7",
            "unknown_field: 999",
            "opoint_end:",
            "itr:",
            "kind: 0",
            "x: 1",
            "y: 2",
            "w: 3",
            "h: 4",
            "injury: 5",
            "itr_end:",
            "cpoint:",
            "kind: 1",
            "drain: 5",
            "cpoint_end:",
            "<frame_end>",
        });
        Lf2DatFile dat = new Lf2DatParserV2().Parse(fixture, "fixture.dat");
        if (dat.Frames.Count != 1 || dat.Frames[0].SubBlocks.Count != 4)
        {
            throw new InvalidOperationException(
                "fixture parser did not preserve all four multiline subblocks");
        }
        var menuDat = new Lf2DatFile();
        var menuBlock = new Lf2DatBlock { Name = "menu_face" };
        var menuLayer = new Lf2DatSubBlock { Name = "layer" };
        menuLayer.AddProperty(new Lf2DatProperty("pic", "c\\0\\M.png"));
        menuLayer.AddProperty(new Lf2DatProperty("x", "-12"));
        menuBlock.SubBlocks.Add(menuLayer);
        menuDat.Blocks.Add(menuBlock);
        List<object> menuFixture = ProjectMenuFace(
            menuDat,
            out bool menuPresent,
            out string menuStatus);
        if (!menuPresent || menuStatus != "AVAILABLE_IN_PRODUCTION_AST" ||
            menuFixture.Count != 1 ||
            !((Dictionary<string, object>)menuFixture[0])
                .ContainsKey("fields") ||
            ((List<object>)((Dictionary<string, object>)menuFixture[0])
                ["fields"]).Count != 2)
        {
            throw new InvalidOperationException(
                "menu_face AST layer projection fixture failed");
        }

        var unavailableMenuDat = new Lf2DatFile();
        unavailableMenuDat.Blocks.Add(
            new Lf2DatBlock { Name = "menu_face" });
        ProjectMenuFace(
            unavailableMenuDat,
            out bool unavailablePresent,
            out string unavailableStatus);
        if (!unavailablePresent ||
            unavailableStatus != "UNAVAILABLE_IN_PRODUCTION_AST")
        {
            throw new InvalidOperationException(
                "menu_face unavailable AST status fixture failed");
        }

        Lf2FrameBlock frame = dat.Frames[0];
        Lf2DatSubBlock aliasCpoint = frame.SubBlocks[0];
        Lf2DatSubBlock opoint = frame.SubBlocks[1];
        Lf2DatSubBlock itr = frame.SubBlocks[2];
        Lf2DatSubBlock rejectedCpoint = frame.SubBlocks[3];

        Dictionary<string, object> aliasProjection = ProjectSubblock(
            frame.Properties, aliasCpoint, out bool aliasConverted);
        if (!aliasConverted ||
            !((Dictionary<string, object>)aliasProjection["normalized"])
                .ContainsKey("fronthurtact") ||
            !((Dictionary<string, object>)aliasProjection["normalized"])
                .ContainsKey("backhurtact") ||
            !((Dictionary<string, object>)aliasProjection["normalized"])
                .TryGetValue("injury", out object aliasInjury) ||
            Convert.ToInt32(aliasInjury) != 100 ||
            !((Dictionary<string, object>)aliasProjection["normalized"])
                .TryGetValue("cover", out object defaultCover) ||
            Convert.ToInt32(defaultCover) != 0 ||
            ((Dictionary<string, object>)aliasProjection["normalized"]).Count != 19)
        {
            throw new InvalidOperationException(
                "CPoint alias/default typed projection fixture failed");
        }

        Dictionary<string, object> opointProjection = ProjectSubblock(
            frame.Properties, opoint, out bool opointConverted);
        var normalized = (Dictionary<string, object>)opointProjection["normalized"];
        var opointFields = (List<object>)opointProjection["fields"];
        if (!opointConverted || normalized.ContainsKey("unknown_field") ||
            !opointFields.Any(field =>
                field is Dictionary<string, object> property &&
                string.Equals(property["key"] as string, "unknown_field",
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "OPoint unknown AST field was not retained only in raw fields");
        }

        LF2FrameData opointData = Lf2DatConverter.ConvertToFrameData(
            BuildIsolatedFrame(frame.Properties, opoint));
        if (opointData.opoints.Count != 1 ||
            opointData.opoints[0].Oid != 7 ||
            opointData.opoints[0].GetType().GetMembers()
                .Any(member => string.Equals(member.Name, "unknown_field",
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "OPoint unknown field entered the formal DTO or known values failed");
        }

        Dictionary<string, object> itrProjection = ProjectSubblock(
            frame.Properties, itr, out bool itrConverted);
        if (!itrConverted)
            throw new InvalidOperationException("ITR isolated conversion failed");

        Dictionary<string, object> rejectedProjection = ProjectSubblock(
            frame.Properties, rejectedCpoint, out bool rejectedConverted);
        string rejectedError = rejectedProjection["conversionError"] as string;
        if (rejectedConverted || string.IsNullOrWhiteSpace(rejectedError) ||
            rejectedError.IndexOf("drain", StringComparison.OrdinalIgnoreCase) < 0)
        {
            throw new InvalidOperationException(
                "CPoint drain rejection was not retained");
        }

        try
        {
            Lf2DatConverter.ConvertToFrameData(frame);
            throw new InvalidOperationException(
                "full frame conversion unexpectedly accepted rejected CPoint");
        }
        catch (InvalidOperationException ex) when (
            ex.Message.IndexOf("drain", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            // Expected full-frame failure. Isolated records above retain later blocks.
        }
    }
}
