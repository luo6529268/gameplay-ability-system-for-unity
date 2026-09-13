using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSD28Parity;

internal static class TraceContractSelfTest
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
    };

    internal static TraceSelfTestReport Run()
    {
        var report = new TraceSelfTestReport
        {
            Schema = TraceContract.SelfTestSchema,
        };

        RunCase(
            report,
            "contract-descriptor",
            "descriptor-valid",
            null,
            () => ValidateDescriptor() ?
                ("descriptor-valid", (string?)null, (string?)null) :
                ("descriptor-invalid", null, null));

        RunCase(
            report,
            "entity-binding-inventory",
            "binding-inventory-valid",
            null,
            () => ValidateEntityBindingInventory() ?
                ("binding-inventory-valid", (string?)null, (string?)null) :
                ("binding-inventory-invalid", null, null));

        string authorityCapture = BuildAuthorityCapture();
        RunCase(
            report,
            "authority-source-capture-valid",
            "valid-source-model-capture",
            null,
            () =>
            {
                AuthorityCaptureValidationReport validation =
                    AuthorityCaptureValidator.ValidateTextForTest(
                        authorityCapture);
                return (validation.Status, validation.Reason, (string?)null);
            });

        RunCase(
            report,
            "authority-source-capture-truncated",
            "invalid",
            null,
            () =>
            {
                string[] lines = SplitLines(authorityCapture);
                AuthorityCaptureValidationReport validation =
                    AuthorityCaptureValidator.ValidateTextForTest(
                        JoinLines(lines[..^1]));
                return (validation.Status, validation.Reason, (string?)null);
            });

        RunCase(
            report,
            "authority-source-capture-extra-tick",
            "invalid",
            null,
            () =>
            {
                string extra = authorityCapture +
                    Serialize(BuildAuthorityCaptureTick(3)) +
                    Environment.NewLine;
                AuthorityCaptureValidationReport validation =
                    AuthorityCaptureValidator.ValidateTextForTest(extra);
                return (validation.Status, validation.Reason, (string?)null);
            });

        RunCase(
            report,
            "authority-source-capture-formal-sha-drift",
            "invalid",
            null,
            () =>
            {
                string[] lines = SplitLines(authorityCapture);
                JsonObject header = JsonNode.Parse(lines[0])!.AsObject();
                header["formalExeSha256"] = new string('0', 64);
                lines[0] = Serialize(header);
                AuthorityCaptureValidationReport validation =
                    AuthorityCaptureValidator.ValidateTextForTest(
                        JoinLines(lines));
                return (validation.Status, validation.Reason, (string?)null);
            });

        string authority = BuildTrace(
            "authority",
            slotCapacity: 1000,
            contentManifest: new string('A', 64));
        string unity = BuildTrace(
            "unity",
            slotCapacity: 1050,
            contentManifest: new string('A', 64));

        RunCase(
            report,
            "valid-trace",
            "valid-structure",
            null,
            () =>
            {
                TraceValidationReport validation =
                    TraceComparator.ValidateTextForTest(authority);
                return (validation.Status, validation.Reason, (string?)null);
            });

        RunComparisonCase(
            report,
            "equal-with-different-slot-capacity",
            TraceComparator.EqualStructureStatus,
            null,
            authority,
            unity);

        RunComparisonCase(
            report,
            "input-first-difference",
            "different",
            "input",
            authority,
            MutateTickDomain(unity, lineIndex: 1, "input", input =>
            {
                input["phase"] = 1;
            }));

        RunComparisonCase(
            report,
            "rng-call-site-first-difference",
            "different",
            "rng",
            authority,
            MutateTickDomain(unity, lineIndex: 1, "rng", rng =>
            {
                JsonObject synchronized = rng["synchronized"]!.AsObject();
                synchronized["totalCalls"] = 1;
                synchronized["tickCalls"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["ordinal"] = 0,
                        ["callSite"] = 0x92,
                        ["bound"] = 200,
                        ["value"] = 1,
                    },
                };
            }));

        RunComparisonCase(
            report,
            "skipped-completed-tick",
            "different",
            "authority",
            MutateTickNumber(authority, lineIndex: 2, completedTick: 3),
            unity);

        RunComparisonCase(
            report,
            "forged-domain-hash",
            "different",
            "authority",
            MutateWithoutRehash(authority, lineIndex: 1, "world", world =>
            {
                world["sequence"] = 999;
            }),
            unity);

        RunComparisonCase(
            report,
            "entity-slot-outside-producer-capacity",
            "different",
            "authority",
            MutateTickArrayDomain(
                authority,
                lineIndex: 1,
                "entities",
                entities =>
                {
                    entities[0]!["slot"] = 1000;
                }),
            unity);

        RunComparisonCase(
            report,
            "entity-required-field-missing",
            "different",
            "unity",
            authority,
            MutateTickArrayDomain(
                unity,
                lineIndex: 1,
                "entities",
                entities =>
                {
                    entities[0]!["frame"]!.AsObject().Remove("actionLatch");
                }));

        RunComparisonCase(
            report,
            "entity-extra-field-rejected",
            "different",
            "unity",
            authority,
            MutateTickArrayDomain(
                unity,
                lineIndex: 1,
                "entities",
                entities =>
                {
                    entities[0]!["combat"]!["invented"] = 1;
                }));

        RunComparisonCase(
            report,
            "entity-field-type-rejected",
            "different",
            "unity",
            authority,
            MutateTickArrayDomain(
                unity,
                lineIndex: 1,
                "entities",
                entities =>
                {
                    entities[0]!["position"]!["preciseX"] = "0.0";
                }));

        RunComparisonCase(
            report,
            "content-strategy-pending",
            TraceComparator.ContentStrategyPendingStatus,
            null,
            authority,
            BuildTrace(
                "unity",
                slotCapacity: 1050,
                contentManifest: new string('C', 64)));

        RunComparisonCase(
            report,
            "approved-exception-drift",
            "different",
            "header",
            authority,
            MutateHeaderArray(unity, "approvedExceptions"));

        RunComparisonCase(
            report,
            "excluded-feature-drift",
            "different",
            "header",
            authority,
            MutateHeaderArray(unity, "excludedFeatures"));

        RunComparisonCase(
            report,
            "extra-authority-tick",
            "different",
            "stream",
            AppendExtraTick(authority, completedTick: 3),
            unity);

        RunComparisonCase(
            report,
            "malformed-unity-json",
            "different",
            "unity",
            authority,
            ReplaceLine(unity, lineIndex: 2, "{not-json}"));

        report.Passed = report.Cases.All(test => test.Passed);
        return report;
    }

    private static void RunComparisonCase(
        TraceSelfTestReport report,
        string name,
        string expectedStatus,
        string? expectedDomain,
        string authority,
        string unity)
    {
        RunCase(
            report,
            name,
            expectedStatus,
            expectedDomain,
            () =>
            {
                TraceComparisonReport comparison =
                    TraceComparator.CompareTextForTest(authority, unity);
                return (
                    comparison.Status,
                    comparison.FirstDifference?.Reason,
                    comparison.FirstDifference?.Domain);
            });
    }

    private static void RunCase(
        TraceSelfTestReport report,
        string name,
        string expectedStatus,
        string? expectedDomain,
        Func<(string Status, string? Reason, string? Domain)> action)
    {
        string actualStatus;
        string? actualReason;
        string? actualDomain;
        try
        {
            (actualStatus, actualReason, actualDomain) = action();
        }
        catch (Exception exception)
        {
            actualStatus = "exception";
            actualReason = exception.ToString();
            actualDomain = null;
        }

        bool passed = string.Equals(
                          actualStatus,
                          expectedStatus,
                          StringComparison.Ordinal) &&
                      string.Equals(
                          actualDomain,
                          expectedDomain,
                          StringComparison.Ordinal);
        report.Cases.Add(new TraceSelfTestCase
        {
            Name = name,
            ExpectedStatus = expectedStatus,
            ActualStatus = actualStatus,
            ExpectedDomain = expectedDomain,
            ActualDomain = actualDomain,
            Reason = actualReason,
            Passed = passed,
        });
    }

    private static bool ValidateDescriptor()
    {
        string json = JsonSerializer.Serialize(
            TraceContract.CreateDescriptor(),
            CompactOptions);
        JsonObject descriptor = JsonNode.Parse(json)!.AsObject();
        return string.Equals(
                   descriptor["schema"]!.GetValue<string>(),
                   TraceContract.DescriptorSchema,
                   StringComparison.Ordinal) &&
               string.Equals(
                   descriptor["authorityExecutableSha256"]!.GetValue<string>(),
                   TraceContract.AuthorityExecutableSha256,
                   StringComparison.Ordinal) &&
               descriptor["certificateEligible"]!.GetValue<bool>() == false &&
               TraceContract.SequenceEqual(
                   descriptor["domainOrder"]!.AsArray(),
                   TraceContract.DomainOrder) &&
               TraceContract.SequenceEqual(
                   descriptor["approvedExceptions"]!.AsArray(),
                   TraceContract.ApprovedExceptions) &&
               TraceContract.SequenceEqual(
                   descriptor["excludedFeatures"]!.AsArray(),
                   TraceContract.ExcludedFeatures) &&
               descriptor["entity"]!["fieldCount"]!.GetValue<int>() ==
                   EntityFieldContract.Fields.Length &&
               descriptor["entity"]!["allComparisonPoliciesStrict"]!
                   .GetValue<bool>();
    }

    private static bool ValidateEntityBindingInventory()
    {
        var expectedPaths = new HashSet<string>(StringComparer.Ordinal)
        {
            "slot",
            "allocationEpoch",
            "active",
        };
        foreach ((string group, string[] properties) in
                 EntityFieldContract.GroupProperties)
        {
            foreach (string property in properties)
            {
                expectedPaths.Add($"{group}.{property}");
            }
        }

        return EntityFieldContract.Fields.Length == 49 &&
               EntityFieldContract.Fields.Select(field => field.Path)
                   .Distinct(StringComparer.Ordinal)
                   .Count() == EntityFieldContract.Fields.Length &&
               expectedPaths.SetEquals(
                   EntityFieldContract.Fields.Select(field => field.Path)) &&
               EntityFieldContract.Fields.All(field =>
                   field.ComparisonPolicy == EntityFieldContract.StrictComparison) &&
               EntityFieldContract.Fields.All(field =>
                   field.BindingStatus is EntityFieldContract.VerifiedBinding or
                       EntityFieldContract.CandidateBinding or
                       EntityFieldContract.MissingBinding) &&
               EntityFieldContract.Fields.Any(field =>
                   field.BindingStatus == EntityFieldContract.MissingBinding);
    }

    private static string BuildTrace(
        string producer,
        int slotCapacity,
        string contentManifest)
    {
        JsonObject header = BuildHeader(
            producer,
            slotCapacity,
            contentManifest,
            expectedTickCount: 2);
        JsonObject first = BuildTick(1);
        JsonObject second = BuildTick(2);
        return string.Join(
                   Environment.NewLine,
                   Serialize(header),
                   Serialize(first),
                   Serialize(second)) +
               Environment.NewLine;
    }

    private static JsonObject BuildHeader(
        string producer,
        int slotCapacity,
        string contentManifest,
        int expectedTickCount)
    {
        return new JsonObject
        {
            ["kind"] = "header",
            ["schema"] = TraceContract.Schema,
            ["producer"] = producer,
            ["authority"] = new JsonObject
            {
                ["exeSha256"] = TraceContract.AuthorityExecutableSha256,
            },
            ["scenario"] = new JsonObject
            {
                ["id"] = "self-test",
                ["version"] = 1,
                ["firstCompletedTick"] = 1,
                ["expectedTickCount"] = expectedTickCount,
            },
            ["timing"] = new JsonObject
            {
                ["normalIntervalMs"] =
                    TraceContract.NormalLogicIntervalMilliseconds,
                ["fastIntervalMs"] =
                    TraceContract.FastLogicIntervalMilliseconds,
            },
            ["slotCapacity"] = slotCapacity,
            ["content"] = new JsonObject
            {
                ["policy"] = TraceContract.ContentPolicy,
                ["manifestSha256"] = contentManifest,
            },
            ["approvedExceptions"] = ToJsonArray(
                TraceContract.ApprovedExceptions),
            ["excludedFeatures"] = ToJsonArray(
                TraceContract.ExcludedFeatures),
            ["domainOrder"] = ToJsonArray(TraceContract.DomainOrder),
        };
    }

    private static JsonObject BuildTick(long completedTick)
    {
        var tick = new JsonObject
        {
            ["kind"] = "tick",
            ["completedTick"] = completedTick,
            ["input"] = new JsonObject
            {
                ["phase"] = 0,
                ["players"] = new JsonArray(),
            },
            ["rng"] = new JsonObject
            {
                ["crt"] = new JsonObject
                {
                    ["state"] = completedTick,
                    ["totalCalls"] = 0,
                    ["tickCalls"] = new JsonArray(),
                },
                ["synchronized"] = new JsonObject
                {
                    ["tableSha256"] = new string('B', 64),
                    ["totalCalls"] = 0,
                    ["tickCalls"] = new JsonArray(),
                },
            },
            ["world"] = new JsonObject
            {
                ["sequence"] = completedTick,
                ["battleMode"] = 0,
            },
            ["entities"] = new JsonArray
            {
                BuildEntity(),
            },
            ["relations"] = new JsonArray(),
            ["rests"] = new JsonObject
            {
                ["a"] = new JsonArray(),
                ["v"] = new JsonArray(),
            },
            ["events"] = new JsonArray(),
            ["presentation"] = new JsonArray(),
            ["hashes"] = new JsonObject(),
        };
        RehashTick(tick);
        return tick;
    }

    private static JsonObject BuildEntity()
    {
        return new JsonObject
        {
            ["slot"] = 0,
            ["allocationEpoch"] = 1,
            ["active"] = true,
            ["identity"] = new JsonObject
            {
                ["objectId"] = 1,
                ["objectType"] = 0,
                ["controlSlot"] = 0,
                ["ownerSlot"] = -1,
                ["battleGroup"] = 1,
                ["participantClass"] = 0,
            },
            ["frame"] = new JsonObject
            {
                ["action"] = 0,
                ["actionLatch"] = 0,
                ["previousAction"] = 0,
                ["tickActionSnapshot"] = 0,
                ["frameCounter"] = 0,
                ["frameState"] = 0,
                ["facingLeft"] = false,
            },
            ["position"] = new JsonObject
            {
                ["x"] = 0,
                ["y"] = 0,
                ["z"] = 0,
                ["preciseX"] = 0.0,
                ["preciseY"] = 0.0,
                ["preciseZ"] = 0.0,
            },
            ["motion"] = new JsonObject
            {
                ["x"] = 0.0,
                ["y"] = 0.0,
                ["z"] = 0.0,
            },
            ["vitals"] = new JsonObject
            {
                ["currentHp"] = 500,
                ["effectiveMaxHp"] = 500,
                ["baseMaxHp"] = 500,
                ["currentMp"] = 500,
                ["baseMaxMp"] = 500,
                ["reviveLives"] = 1,
                ["reviveNextLives"] = 0,
                ["reviveNextHp"] = 0,
            },
            ["combat"] = new JsonObject
            {
                ["runtimeStateCode"] = 0,
                ["renderPhase"] = 0,
                ["attackerRest"] = 0,
                ["collisionYReference"] = 0,
                ["platformSourceSlot"] = 0,
                ["hitReactionTimer"] = 0,
                ["bdefendAccumulator"] = 0,
                ["runtimeArmorHp"] = 0,
                ["armorRecoveryTimer"] = -1,
                ["motionHoldTimer"] = 0,
                ["weaponHp"] = 0,
                ["specialHitLatch0eb"] = false,
                ["environmentState"] = 0,
                ["environmentSourceSlot"] = -1,
            },
            ["lifecycle"] = new JsonObject
            {
                ["resolutionPending"] = false,
                ["code"] = 0,
            },
        };
    }

    private static string BuildAuthorityCapture()
    {
        var header = new JsonObject
        {
            ["kind"] = "header",
            ["schema"] = AuthorityCaptureValidator.CaptureSchema,
            ["certificateEligible"] = false,
            ["evidenceClass"] = AuthorityCaptureValidator.EvidenceClass,
            ["formalExeSha256"] = TraceContract.AuthorityExecutableSha256,
            ["authoritySourceManifestSha256"] = new string('C', 64),
            ["captureRunnerSourceSha256"] = new string('D', 64),
            ["captureBinarySha256"] = new string('F', 64),
            ["scenarioReferenceExeSha256"] =
                AuthorityCaptureValidator.LegacyScenarioReferenceSha256,
            ["scenarioDataSha256"] = new string('E', 64),
            ["scenarioId"] = "self-test-source-capture",
            ["firstCompletedTick"] = 1,
            ["expectedTickCount"] = 2,
            ["slotCapacity"] = AuthorityCaptureValidator.AuthoritySlotCapacity,
        };
        return string.Join(
                   Environment.NewLine,
                   Serialize(header),
                   Serialize(BuildAuthorityCaptureTick(1)),
                   Serialize(BuildAuthorityCaptureTick(2))) +
               Environment.NewLine;
    }

    private static JsonObject BuildAuthorityCaptureTick(long completedTick)
    {
        return new JsonObject
        {
            ["kind"] = "tick",
            ["completedTick"] = completedTick,
            ["entities"] = new JsonArray
            {
                BuildEntity(),
            },
        };
    }

    private static string MutateTickDomain(
        string source,
        int lineIndex,
        string domain,
        Action<JsonObject> mutation)
    {
        string[] lines = SplitLines(source);
        JsonObject tick = JsonNode.Parse(lines[lineIndex])!.AsObject();
        mutation(tick[domain]!.AsObject());
        RehashTick(tick);
        lines[lineIndex] = Serialize(tick);
        return JoinLines(lines);
    }

    private static string MutateWithoutRehash(
        string source,
        int lineIndex,
        string domain,
        Action<JsonObject> mutation)
    {
        string[] lines = SplitLines(source);
        JsonObject tick = JsonNode.Parse(lines[lineIndex])!.AsObject();
        mutation(tick[domain]!.AsObject());
        lines[lineIndex] = Serialize(tick);
        return JoinLines(lines);
    }

    private static string MutateTickArrayDomain(
        string source,
        int lineIndex,
        string domain,
        Action<JsonArray> mutation)
    {
        string[] lines = SplitLines(source);
        JsonObject tick = JsonNode.Parse(lines[lineIndex])!.AsObject();
        mutation(tick[domain]!.AsArray());
        RehashTick(tick);
        lines[lineIndex] = Serialize(tick);
        return JoinLines(lines);
    }

    private static string MutateTickNumber(
        string source,
        int lineIndex,
        long completedTick)
    {
        string[] lines = SplitLines(source);
        JsonObject tick = JsonNode.Parse(lines[lineIndex])!.AsObject();
        tick["completedTick"] = completedTick;
        lines[lineIndex] = Serialize(tick);
        return JoinLines(lines);
    }

    private static string MutateHeaderArray(string source, string property)
    {
        string[] lines = SplitLines(source);
        JsonObject header = JsonNode.Parse(lines[0])!.AsObject();
        header[property]!.AsArray().RemoveAt(0);
        lines[0] = Serialize(header);
        return JoinLines(lines);
    }

    private static string AppendExtraTick(string source, long completedTick)
    {
        return source + Serialize(BuildTick(completedTick)) + Environment.NewLine;
    }

    private static string ReplaceLine(
        string source,
        int lineIndex,
        string replacement)
    {
        string[] lines = SplitLines(source);
        lines[lineIndex] = replacement;
        return JoinLines(lines);
    }

    private static void RehashTick(JsonObject tick)
    {
        JsonObject hashes = tick["hashes"]!.AsObject();
        foreach (string domain in TraceContract.DomainOrder)
        {
            hashes[domain] = TraceContract.HashNode(tick[domain]!);
        }

        hashes["overall"] = TraceContract.ComputeOverallHash(hashes);
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values)
    {
        var result = new JsonArray();
        foreach (string value in values)
        {
            result.Add(value);
        }

        return result;
    }

    private static string Serialize(JsonNode value)
    {
        return TraceContract.Canonicalize(value).ToJsonString(CompactOptions);
    }

    private static string[] SplitLines(string source)
    {
        return source.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static string JoinLines(IEnumerable<string> lines)
    {
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}

internal sealed class TraceSelfTestReport
{
    public string Schema { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public List<TraceSelfTestCase> Cases { get; set; } = [];
}

internal sealed class TraceSelfTestCase
{
    public string Name { get; set; } = string.Empty;
    public string ExpectedStatus { get; set; } = string.Empty;
    public string ActualStatus { get; set; } = string.Empty;
    public string? ExpectedDomain { get; set; }
    public string? ActualDomain { get; set; }
    public string? Reason { get; set; }
    public bool Passed { get; set; }
}
