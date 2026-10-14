using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;

internal static class RoleResourceCapture
{
    private const string AuditId = "NTSD-M12-EIGHT-ROLE-MEMORY-AUDIT-001";
    private const string ProjectDirectoryName = "gameplay-ability-system-for-unity";

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
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
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganHeaders.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganDefinitions.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatDecryptor.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2LoganDefinitionBlocks.cs",
        "Assets/NTSD/Scripts/Animation/LF2FrameData.cs",
        "Assets/NTSD/Scripts/Animation/LF2CharacterData.cs",
        "Assets/NTSD/Scripts/Animation/LF2ArmorData.cs",
        "Assets/NTSD/Scripts/Animation/LoganDefinitionMetadata.cs",
        "Assets/NTSD/Scripts/Animation/LoganWeaponPieceDefinition.cs",
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
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganFusionCatalog.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganFusionCatalogParser.cs",
        "Assets/NTSD/Scripts/Animation/LoganFusionCatalogInput.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs",
        "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganKindCatalogParser.cs",
    };

    private sealed class CatalogRecord
    {
        public int RegistryIndex;
        public int Id;
        public int Type;
        public string SourcePath;
        public string PublishedFolder;
        public string CatalogName;
        public int SourceLine;
    }

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
            string runtimeRoot = Path.GetFullPath(arguments.RuntimeRoot);
            string outputPath = Path.GetFullPath(arguments.Output);
            ValidateRuntimeRoot(runtimeRoot);
            ValidateOutputPath(repositoryRoot, runtimeRoot, outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            string catalogPath = Path.Combine(runtimeRoot, "catalog.csv");
            string decodedRoot = Path.Combine(runtimeRoot, "decoded_dat");
            string vfsRoot = Path.Combine(runtimeRoot, "vfs");
            if (!File.Exists(catalogPath) || !Directory.Exists(decodedRoot) || !Directory.Exists(vfsRoot))
                throw new InvalidDataException("runtime root must contain catalog.csv, decoded_dat and vfs.");

            Dictionary<string, string> sourceHashes = HashProductionSources(repositoryRoot);
            byte[] catalogBytes = File.ReadAllBytes(catalogPath);
            List<CatalogRecord> records = ReadObjectCatalog(runtimeRoot, catalogBytes);
            if (records.Count != 330)
                throw new InvalidDataException($"Expected 330 object catalog rows, got {records.Count}.");

            var result = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["schemaVersion"] = 1,
                ["auditId"] = AuditId,
                ["captureKind"] = "production_parser_logan_content_all_object_catalog",
                ["capturedAtUtc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                ["repositoryRoot"] = repositoryRoot,
                ["runtimeRoot"] = runtimeRoot,
                ["decodedDatRoot"] = decodedRoot,
                ["vfsRoot"] = vfsRoot,
                ["catalogPath"] = catalogPath,
                ["catalogIdentity"] = FileIdentity(catalogPath),
                ["productionSourceInputs"] = sourceHashes
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => (object)new Dictionary<string, object>
                    {
                        ["path"] = pair.Key,
                        ["sha256"] = pair.Value,
                    }).ToList(),
                ["objectCountExpected"] = 330,
                ["objects"] = new List<object>(),
                ["commonInputs"] = CaptureCommonInputs(decodedRoot, vfsRoot),
                ["fusion"] = CaptureFusion(decodedRoot, runtimeRoot),
                ["kind"] = CaptureKind(decodedRoot),
            };

            var objects = (List<object>)result["objects"];
            var parser = new Lf2DatParserV2();
            int parseSuccess = 0;
            int parseFailure = 0;
            int converterFailureFrames = 0;
            foreach (CatalogRecord record in records.OrderBy(item => item.RegistryIndex))
            {
                Dictionary<string, object> capture = CaptureObject(
                    runtimeRoot, decodedRoot, vfsRoot, record, parser,
                    out bool parsed, out int conversionFailures);
                objects.Add(capture);
                if (parsed) parseSuccess++;
                else parseFailure++;
                converterFailureFrames += conversionFailures;
            }

            result["parseSummary"] = new Dictionary<string, object>
            {
                ["catalogObjectRows"] = records.Count,
                ["parseSuccess"] = parseSuccess,
                ["parseFailure"] = parseFailure,
                ["converterFailureFrames"] = converterFailureFrames,
                ["allObjectsCaptured"] = records.Count == objects.Count,
            };

            File.WriteAllText(outputPath, JsonSerializer.Serialize(result, JsonOptions),
                new UTF8Encoding(false));
            Console.WriteLine($"objects={objects.Count} parseSuccess={parseSuccess} parseFailure={parseFailure} converterFailureFrames={converterFailureFrames}");
            Console.WriteLine($"output={outputPath}");
            return parseFailure == 0 ? 0 : 2;
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
        public string RuntimeRoot;
        public string Output;
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
                if (!arg.StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length)
                    throw new ArgumentException("Expected --runtime-root and --output.");
                string value = args[++index];
                switch (arg.ToLowerInvariant())
                {
                    case "--runtime-root": result.RuntimeRoot = value; break;
                    case "--output": result.Output = value; break;
                    default: throw new ArgumentException("Unknown argument: " + arg);
                }
            }
            if (result.SelfTest) return result;
            if (string.IsNullOrWhiteSpace(result.RuntimeRoot) || string.IsNullOrWhiteSpace(result.Output))
                throw new ArgumentException("Requires --runtime-root and --output.");
            return result;
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo current = new DirectoryInfo(Environment.CurrentDirectory);
        while (current != null)
        {
            if (string.Equals(current.Name, ProjectDirectoryName, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(Path.Combine(current.FullName, "ProjectSettings", "ProjectVersion.txt")))
                return current.FullName;
            current = current.Parent;
        }
        throw new InvalidOperationException("Could not locate the Unity repository root.");
    }

    private static void ValidateRuntimeRoot(string runtimeRoot)
    {
        if (!Directory.Exists(runtimeRoot))
            throw new ArgumentException("Runtime root does not exist: " + runtimeRoot);
    }

    private static void ValidateOutputPath(string repositoryRoot, string runtimeRoot, string outputPath)
    {
        if (IsWithin(runtimeRoot, outputPath))
            throw new ArgumentException("Capture output cannot be inside the read-only runtime root.");
        string allowed = Path.GetFullPath(Path.Combine(repositoryRoot,
            "artifacts/diagnostics/M12-astc-eight-role-20261013/eight-role-audit-worker"));
        if (!IsWithin(allowed, outputPath))
            throw new ArgumentException("Capture output must be inside the worker evidence directory.");
    }

    private static bool IsWithin(string root, string candidate)
    {
        string rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string candidateFull = Path.GetFullPath(candidate);
        return candidateFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(candidateFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                rootFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string> HashProductionSources(string repositoryRoot)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string relativePath in ProductionSourceRelativePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string fullPath = Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath)) throw new FileNotFoundException("Production source missing: " + relativePath, fullPath);
            result[relativePath] = Sha256File(fullPath);
        }
        return result;
    }

    private static List<CatalogRecord> ReadObjectCatalog(string runtimeRoot, byte[] catalogBytes)
    {
        string catalogText = DecodeUtf8(catalogBytes);
        using var reader = new StringReader(catalogText);
        string headerLine = reader.ReadLine();
        if (headerLine == null) throw new InvalidDataException("catalog.csv is empty.");
        List<string> header = ParseCsvRow(headerLine);
        var columns = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int index = 0; index < header.Count; index++) columns[header[index]] = index;
        foreach (string required in new[] { "registry_section", "registry_index", "id", "type", "source_path", "published_folder" })
            if (!columns.ContainsKey(required)) throw new InvalidDataException("catalog.csv is missing " + required);

        var result = new List<CatalogRecord>();
        var ids = new HashSet<int>();
        var indices = new HashSet<int>();
        string line;
        int lineNumber = 1;
        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;
            List<string> row = ParseCsvRow(line);
            string Field(string key) => columns[key] < row.Count ? row[columns[key]] : null;
            if (!string.Equals(Field("registry_section"), "object", StringComparison.Ordinal)) continue;
            if (!TryInt(Field("registry_index"), out int registryIndex) || registryIndex < 0 ||
                !TryInt(Field("id"), out int id) || !TryInt(Field("type"), out int type) ||
                Field("source_path") == null || string.IsNullOrEmpty(Field("published_folder")))
                throw new InvalidDataException($"catalog line {lineNumber} has invalid object fields.");
            if (!ids.Add(id) || !indices.Add(registryIndex))
                throw new InvalidDataException($"catalog line {lineNumber} has duplicate id or registry index.");
            result.Add(new CatalogRecord
            {
                RegistryIndex = registryIndex,
                Id = id,
                Type = type,
                SourcePath = Field("source_path"),
                PublishedFolder = Field("published_folder"),
                CatalogName = Field("name"),
                SourceLine = lineNumber,
            });
        }
        return result.OrderBy(item => item.RegistryIndex).ToList();
    }

    private static Dictionary<string, object> CaptureObject(
        string runtimeRoot,
        string decodedRoot,
        string vfsRoot,
        CatalogRecord record,
        Lf2DatParserV2 parser,
        out bool parsed,
        out int conversionFailures)
    {
        parsed = false;
        conversionFailures = 0;
        string normalizedSource = NormalizePath(record.SourcePath);
        string publishedPath = Path.GetFullPath(Path.Combine(runtimeRoot,
            record.PublishedFolder.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)));
        string datPath;
        string resolution;
        if (Directory.Exists(publishedPath))
        {
            string[] candidates = Directory.EnumerateFiles(publishedPath, "*.dat", SearchOption.TopDirectoryOnly)
                .OrderBy(path => NormalizePath(path), StringComparer.OrdinalIgnoreCase).ToArray();
            if (candidates.Length != 1)
                throw new InvalidDataException($"published_folder must contain exactly one top-level DAT: {publishedPath}");
            datPath = Path.GetFullPath(candidates[0]);
            resolution = "published_folder";
        }
        else
        {
            datPath = Path.GetFullPath(Path.Combine(decodedRoot,
                normalizedSource.Replace('/', Path.DirectorySeparatorChar)));
            resolution = "decoded_dat/source_path";
        }
        if (!File.Exists(datPath))
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["registryIndex"] = record.RegistryIndex,
                ["id"] = record.Id,
                ["type"] = record.Type,
                ["sourcePath"] = normalizedSource,
                ["publishedFolder"] = NormalizePath(record.PublishedFolder),
                ["catalogName"] = record.CatalogName,
                ["catalogSourceLine"] = record.SourceLine,
                ["datResolution"] = resolution,
                ["datPath"] = datPath,
                ["parseSuccess"] = false,
                ["parseError"] = "DAT file missing",
                ["frames"] = new List<object>(),
                ["bmp"] = null,
                ["weaponPiece"] = null,
            };
        }

        var result = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["registryIndex"] = record.RegistryIndex,
            ["id"] = record.Id,
            ["type"] = record.Type,
            ["sourcePath"] = normalizedSource,
            ["publishedFolder"] = NormalizePath(record.PublishedFolder),
            ["catalogName"] = record.CatalogName,
            ["catalogSourceLine"] = record.SourceLine,
            ["datResolution"] = resolution,
            ["datPath"] = datPath,
            ["datIdentity"] = FileIdentity(datPath),
            ["parseSuccess"] = false,
            ["parseError"] = null,
            ["bmp"] = null,
            ["frames"] = new List<object>(),
            ["weaponPiece"] = null,
            ["parserDiagnostics"] = new List<object>(),
        };
        try
        {
            UnityEngine.Debug.ClearAuditSink();
            string text = File.ReadAllText(datPath, Encoding.UTF8);
            Lf2DatFile dat = parser.ParseLoganContent(text, datPath);
            parsed = true;
            result["parseSuccess"] = true;
            result["fileType"] = dat.FileType.ToString();
            result["bmp"] = ProjectBmp(dat.Bmp, vfsRoot);
            result["weaponPiece"] = ProjectWeaponPiece(dat.LoganWeaponPiece);
            var frames = (List<object>)result["frames"];
            foreach (Lf2FrameBlock frame in dat.Frames)
            {
                frames.Add(ProjectFrame(frame, out bool conversionFailed));
                if (conversionFailed) conversionFailures++;
            }
            result["frameCount"] = frames.Count;
            result["parserDiagnostics"] = ProjectDiagnostics();
        }
        catch (Exception error)
        {
            result["parseError"] = ErrorText(error);
            result["parserDiagnostics"] = ProjectDiagnostics();
        }
        return result;
    }

    private static Dictionary<string, object> ProjectBmp(Lf2BmpSection bmp, string vfsRoot)
    {
        if (bmp == null) return null;
        var files = new List<object>();
        long nextPic = 0;
        foreach (Lf2SpriteFileDef file in bmp.Files)
        {
            long capacity = Math.Max((long)file.Row * file.Col, 0L);
            int effectiveFirst = checked((int)nextPic);
            nextPic = checked(nextPic + capacity);
            int effectiveLast = checked((int)(nextPic - 1));
            string virtualPath = NormalizePath(file.Path);
            string imagePath = ResolveVfsPath(vfsRoot, virtualPath);
            var entry = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["path"] = virtualPath,
                ["imagePath"] = imagePath,
                ["declaredFirst"] = file.StartIndex,
                ["declaredLast"] = file.EndIndex,
                ["effectiveFirst"] = effectiveFirst,
                ["effectiveLast"] = effectiveLast,
                ["width"] = file.Width,
                ["height"] = file.Height,
                ["row"] = file.Row,
                ["col"] = file.Col,
                ["imageIdentity"] = FileIdentityOrNull(imagePath),
            };
            files.Add(entry);
        }
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["name"] = bmp.Name,
            ["head"] = ProjectUiPath(bmp.Head, vfsRoot),
            ["small"] = ProjectUiPath(bmp.Small, vfsRoot),
            ["hidden"] = GetLastInt(bmp.Properties, "hidden", 0),
            ["properties"] = ProjectProperties(bmp.Properties),
            ["frameSequences"] = bmp.FrameSequences.Select(sequence => (object)new Dictionary<string, object>
            {
                ["name"] = sequence.Name,
                ["actions"] = sequence.Actions,
            }).ToList(),
            ["files"] = files,
        };
    }

    private static object ProjectUiPath(string path, string vfsRoot)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        string virtualPath = NormalizePath(path);
        string imagePath = ResolveVfsPath(vfsRoot, virtualPath);
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["path"] = virtualPath,
            ["imagePath"] = imagePath,
            ["imageIdentity"] = FileIdentityOrNull(imagePath),
        };
    }

    private static Dictionary<string, object> ProjectFrame(Lf2FrameBlock source, out bool conversionFailed)
    {
        conversionFailed = false;
        var result = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["frameId"] = source.FrameIndex,
            ["frameName"] = source.FrameName,
            ["rawProperties"] = ProjectProperties(source.Properties),
            ["subBlocks"] = source.SubBlocks.Select(ProjectSubBlock).ToList(),
            ["converted"] = null,
            ["conversionError"] = null,
        };
        try
        {
            LF2FrameData frame = Lf2DatConverter.ConvertLoganFrameData(source);
            result["converted"] = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["frameId"] = frame.frameId,
                ["pic"] = frame.pic,
                ["state"] = frame.state,
                ["hit_Fa"] = frame.hit_Fa,
                ["next"] = frame.next,
                ["wait"] = frame.wait,
                ["opoints"] = frame.opoints.Select(ProjectObjectPoint).ToList(),
                ["cpoints"] = frame.CatchPoints.Select(ProjectCatchPoint).ToList(),
                ["wpoints"] = frame.FormalWeaponPoints.Select(ProjectWeaponPoint).ToList(),
            };
        }
        catch (Exception error)
        {
            conversionFailed = true;
            result["conversionError"] = ErrorText(error);
            result["converted"] = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["frameId"] = source.FrameIndex,
                ["pic"] = GetLastInt(source.Properties, "pic", 0),
                ["state"] = GetLastInt(source.Properties, "state", 0),
                ["hit_Fa"] = GetLastInt(source.Properties, "hit_Fa", 0),
                ["next"] = GetLastInt(source.Properties, "next", 0),
                ["wait"] = GetLastInt(source.Properties, "wait", 0),
                ["opoints"] = new List<object>(),
                ["cpoints"] = new List<object>(),
                ["wpoints"] = new List<object>(),
            };
        }
        return result;
    }

    private static Dictionary<string, object> ProjectSubBlock(Lf2DatSubBlock block)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = block.Name,
            ["fields"] = ProjectProperties(block.Properties),
        };
    }

    private static Dictionary<string, object> ProjectObjectPoint(BattleObjectPointValue point)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = point.Kind, ["x"] = point.X, ["y"] = point.Y, ["action"] = point.Action,
            ["dvx"] = point.Dvx, ["dvy"] = point.Dvy, ["oid"] = point.Oid, ["facing"] = point.Facing,
            ["z"] = point.Z, ["dvz"] = point.Dvz, ["hp"] = point.Hp, ["mp"] = point.Mp,
            ["team"] = point.Team, ["reserve"] = point.Reserve, ["effect"] = point.Effect,
            ["pic"] = point.Pic, ["centerX"] = point.CenterX, ["centerY"] = point.CenterY,
            ["centerZ"] = point.CenterZ, ["frameA"] = point.FrameA, ["attacking"] = point.Attacking,
            ["join"] = point.Join, ["joinReserve"] = point.JoinReserve, ["joinPic"] = point.JoinPic,
        };
    }

    private static Dictionary<string, object> ProjectCatchPoint(BattleCatchPointValue point)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = point.Kind, ["x"] = point.X, ["y"] = point.Y, ["injury"] = point.Injury,
            ["cover"] = point.Cover, ["vaction"] = point.Vaction, ["aaction"] = point.Aaction,
            ["jaction"] = point.Jaction, ["daction"] = point.Daction, ["throwVx"] = point.ThrowVx,
            ["throwVy"] = point.ThrowVy, ["hurtable"] = point.Hurtable, ["decrease"] = point.Decrease,
            ["dirControl"] = point.DirControl, ["taction"] = point.Taction, ["throwInjury"] = point.ThrowInjury,
            ["throwVz"] = point.ThrowVz, ["frontHurtAct"] = point.FrontHurtAct,
            ["backHurtAct"] = point.BackHurtAct, ["faction"] = point.Faction, ["baction"] = point.Baction,
            ["uzaction"] = point.Uzaction, ["dzaction"] = point.Dzaction, ["z"] = point.Z,
            ["recover"] = point.Recover, ["drain"] = point.Drain, ["gain"] = point.Gain,
        };
    }

    private static Dictionary<string, object> ProjectWeaponPoint(BattleWeaponPointValue point)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["kind"] = point.Kind, ["x"] = point.X, ["y"] = point.Y, ["attacking"] = point.Attacking,
            ["cover"] = point.Cover, ["weaponact"] = point.WeaponAct, ["dvx"] = point.Dvx,
            ["dvy"] = point.Dvy, ["dvz"] = point.Dvz,
        };
    }

    private static object ProjectWeaponPiece(Lf2WeaponPieceBlock block)
    {
        if (block == null) return null;
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["fields"] = ProjectProperties(block.Properties),
            ["groups"] = block.Groups.Select(group => (object)new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["piece"] = group.Piece,
                ["fields"] = ProjectProperties(group.Properties),
                ["variants"] = group.Variants.Select(variant => (object)new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["piece"] = variant.Piece,
                    ["fields"] = ProjectProperties(variant.Properties),
                }).ToList(),
            }).ToList(),
        };
    }

    private static List<object> ProjectProperties(IEnumerable<Lf2DatProperty> properties)
    {
        var result = new List<object>();
        if (properties == null) return result;
        foreach (Lf2DatProperty property in properties)
            result.Add(new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["key"] = property?.Key,
                ["value"] = property?.Value,
            });
        return result;
    }

    private static Dictionary<string, object> CaptureCommonInputs(string decodedRoot, string vfsRoot)
    {
        string dataRoot = Path.Combine(decodedRoot, "data");
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (string name in new[] { "resource.dat", "system.dat", "mode.dat" })
        {
            string path = Path.Combine(dataRoot, name);
            result[name] = FileIdentityOrNull(path);
        }
        result["resourceVirtualPaths"] = CaptureResourceVirtualPaths(Path.Combine(dataRoot, "resource.dat"), vfsRoot);
        result["systemSparkFields"] = CaptureSystemSparkFields(Path.Combine(dataRoot, "system.dat"));
        return result;
    }

    private static List<object> CaptureResourceVirtualPaths(string path, string vfsRoot)
    {
        var result = new List<object>();
        if (!File.Exists(path)) return result;
        string[] tokens = File.ReadAllText(path, Encoding.UTF8)
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        bool inBmp = false;
        int index = 0;
        for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
        {
            if (tokens[tokenIndex] == "<bmp_begin>") { inBmp = true; continue; }
            if (tokens[tokenIndex] == "<bmp_end>") { inBmp = false; continue; }
            if (!inBmp || tokens[tokenIndex] != "pic:" || tokenIndex + 1 >= tokens.Length) continue;
            string virtualPath = NormalizePath(tokens[++tokenIndex]);
            result.Add(new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["index"] = index++,
                ["path"] = virtualPath,
                ["imagePath"] = ResolveVfsPath(vfsRoot, virtualPath),
                ["imageIdentity"] = FileIdentityOrNull(ResolveVfsPath(vfsRoot, virtualPath)),
            });
        }
        return result;
    }

    private static Dictionary<string, object> CaptureSystemSparkFields(string path)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["spark_w"] = null,
            ["spark_h"] = null,
        };
        if (!File.Exists(path)) return result;
        foreach (string line in File.ReadLines(path, Encoding.UTF8))
        {
            string trimmed = line.Trim();
            string[] parts = trimmed.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            if (parts[0] == "spark_w:") result["spark_w"] = TryInt(parts[1], out int width) ? width : null;
            if (parts[0] == "spark_h:") result["spark_h"] = TryInt(parts[1], out int height) ? height : null;
        }
        return result;
    }

    private static object CaptureFusion(string decodedRoot, string runtimeRoot)
    {
        LoganFusionCatalogInput input = LoganFusionCatalogInput.Capture(decodedRoot, runtimeRoot);
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["selectedPath"] = input.SelectedPath,
            ["usesLockedFallback"] = input.UsesLockedFallback,
            ["inputFingerprint"] = input.InputFingerprint,
            ["semanticFingerprint"] = input.SemanticFingerprint,
            ["records"] = input.Catalog.Records.Select(record => (object)new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["id1"] = record.Id1, ["id2"] = record.Id2, ["id3"] = record.Id3,
                ["hp"] = record.Hp, ["mp"] = record.Mp, ["respond"] = record.Respond,
                ["decrease"] = record.Decrease, ["wait"] = record.Wait, ["state"] = record.State,
                ["action"] = record.Action, ["frame"] = record.Frame, ["chp"] = record.Chp,
                ["hitJa"] = record.HitJa, ["cover"] = record.Cover,
                ["frontHurtAction"] = record.FrontHurtAction, ["backHurtAction"] = record.BackHurtAction,
                ["sourceLine"] = record.SourceLine,
            }).ToList(),
            ["diagnostics"] = input.Catalog.Diagnostics.Select(diagnostic => (object)new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["severity"] = diagnostic.Severity.ToString(), ["line"] = diagnostic.Line, ["message"] = diagnostic.Message,
            }).ToList(),
        };
    }

    private static object CaptureKind(string decodedRoot)
    {
        string path = Path.Combine(decodedRoot, "data", "kind.dat");
        if (!File.Exists(path)) return new Dictionary<string, object> { ["selectedPath"] = null, ["records"] = new List<object>() };
        LoganKindCatalog catalog = LoganKindCatalogParser.ParseText(File.ReadAllText(path, Encoding.UTF8));
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["selectedPath"] = path,
            ["identity"] = FileIdentity(path),
            ["isValid"] = catalog.IsValid,
            ["records"] = catalog.Records.Select(record => (object)new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["effect"] = record.Effect, ["frame"] = record.Frame, ["sourceLine"] = record.SourceLine,
                ["boundIds"] = record.BoundIds, ["respondIds"] = record.RespondIds,
            }).ToList(),
            ["diagnostics"] = catalog.Diagnostics.Select(diagnostic => (object)new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["severity"] = diagnostic.Severity.ToString(), ["line"] = diagnostic.Line, ["message"] = diagnostic.Message,
            }).ToList(),
        };
    }

    private static Dictionary<string, object> FileIdentity(string path)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["path"] = path,
            ["exists"] = File.Exists(path),
            ["bytes"] = File.Exists(path) ? new FileInfo(path).Length : 0L,
            ["sha256"] = File.Exists(path) ? Sha256File(path) : null,
        };
    }

    private static object FileIdentityOrNull(string path)
    {
        return File.Exists(path) ? FileIdentity(path) : new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["path"] = path,
            ["exists"] = false,
            ["bytes"] = 0L,
            ["sha256"] = null,
        };
    }

    private static string ResolveVfsPath(string vfsRoot, string virtualPath)
    {
        string normalized = NormalizePath(virtualPath);
        return Path.GetFullPath(Path.Combine(vfsRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static string Sha256File(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string DecodeUtf8(byte[] bytes)
    {
        int offset = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf ? 3 : 0;
        return Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
    }

    private static string NormalizePath(string path) => (path ?? string.Empty).Replace('\\', '/');

    private static bool TryInt(string value, out int result)
    {
        return int.TryParse(value?.Trim(), NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out result);
    }

    private static int GetLastInt(IEnumerable<Lf2DatProperty> properties, string key, int fallback)
    {
        int result = fallback;
        if (properties == null) return result;
        foreach (Lf2DatProperty property in properties)
            if (string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase) && TryInt(property.Value, out int value))
                result = value;
        return result;
    }

    private static List<object> ProjectDiagnostics()
    {
        return UnityEngine.Debug.Entries
            .Where(entry => !string.Equals(entry.Level, "Log", StringComparison.Ordinal))
            .Select(entry => (object)new Dictionary<string, object>
            {
                ["level"] = entry.Level,
                ["message"] = entry.Message,
            }).ToList();
    }

    private static string ErrorText(Exception error)
    {
        return error.GetType().FullName + ": " + error.Message;
    }

    private static List<string> ParseCsvRow(string line)
    {
        var columns = new List<string>();
        var value = new StringBuilder();
        bool quoted = false;
        for (int index = 0; index < line.Length; index++)
        {
            char ch = line[index];
            if (quoted)
            {
                if (ch == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        value.Append('"');
                        index++;
                    }
                    else quoted = false;
                }
                else value.Append(ch);
            }
            else if (ch == '"' && value.Length == 0) quoted = true;
            else if (ch == ',')
            {
                columns.Add(value.ToString());
                value.Clear();
            }
            else if (ch != '\r') value.Append(ch);
        }
        columns.Add(value.ToString());
        return columns;
    }

    private static void RunSelfTest()
    {
        const string fixture = "<bmp_begin>\nname: fixture\nfile(0-1): sprite\\frame\\fixture.png w: 16 h: 16 row: 1 col: 2\n<bmp_end>\n<frame> 0 standing\npic: 0\nstate: 0\nhit_Fa: 5\nopoint:\noid: 7\nopoint_end:\n<frame_end>";
        Lf2DatFile dat = new Lf2DatParserV2().ParseLoganContent(fixture, "fixture.dat");
        if (dat.Frames.Count != 1 || dat.Bmp == null || dat.Bmp.Files.Count != 1)
            throw new InvalidOperationException("production Logan parser fixture failed");
        LF2FrameData frame = Lf2DatConverter.ConvertLoganFrameData(dat.Frames[0]);
        if (frame.state != 0 || frame.hit_Fa != 5 || frame.opoints.Count != 1 || frame.opoints[0].Oid != 7)
            throw new InvalidOperationException("production Logan converter fixture failed");
    }
}
