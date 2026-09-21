using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
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

        RunCase(report, "Q05-current-trace-version", "v3", null,
            () => (TraceContract.Schema == "ntsd28-logan-battle-trace-v3" ? "v3" : "old", (string?)null, (string?)null));
        RunCase(report, "Q05-independent-2f8-binding", "bound", null,
            () => (EntityFieldContract.Fields.Length == 50 && EntityFieldContract.Fields.Any(
                field => field.Path == "combat.objectAiExcludedGroupSourceSlot") ? "bound" : "missing", (string?)null, (string?)null));
        RunCase(report, "Q05-semantic-header-required", "invalid", null, () =>
        {
            string[] lines = SplitLines(BuildTrace("authority", 1000, new string('A', 64)));
            JsonObject header = JsonNode.Parse(lines[0])!.AsObject();
            header["content"]!.AsObject().Remove("semanticSha256");
            lines[0] = Serialize(header);
            var validation = TraceComparator.ValidateTextForTest(JoinLines(lines));
            return (validation.Status, validation.Reason, (string?)null);
        });
        RunCase(report, "Q05-old-trace-version-rejected", "invalid", null, () =>
        {
            string[] lines = SplitLines(BuildTrace("authority", 1000, new string('A', 64)));
            JsonObject header = JsonNode.Parse(lines[0])!.AsObject();
            header["schema"] = "ntsd28-logan-battle-trace-v2";
            lines[0] = Serialize(header);
            var validation = TraceComparator.ValidateTextForTest(JoinLines(lines));
            return (validation.Status, validation.Reason, (string?)null);
        });

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
            "content-identity-mismatch",
            "different",
            "header",
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

        var mutations = new Dictionary<string, Action<JsonObject>>
        {
            ["raw-without-rehash"] = content => content["rawDefinitionSha256"] = new string('B', 64),
            ["invalid-raw"] = content => content["rawDefinitionSha256"] = "not-a-sha",
            ["false-semantic"] = content => content["semanticSha256"] = new string('0', 64),
            ["wrong-projection"] = content => content["catalogFingerprint64"] = "0000000000000000",
            ["wrong-decode"] = content => content["decodeContract"] = "NTSD28_LOGAN_DAT_SEMANTICS_V1",
            ["retired-policy"] = content => content["policy"] = "strategy-pending",
            ["wrong-scope"] = content => content["scope"] = "all-assets",
            ["unknown-profile"] = content => content["profile"] = "unknown",
            ["extra-content-field"] = content => content["extra"] = 1,
            ["wrong-schema-type"] = content => content["schemas"] = "13/21/24/2/2",
            ["extra-schema"] = content => content["schemas"]!["extra"] = 1,
        };
        foreach (string property in new[] { "policy", "scope", "profile", "rawDefinitionSha256", "decodeContract", "semanticSha256", "catalogFingerprint64", "schemas", "objectDefinitionSha256", "fusionInputSha256", "fusionSemanticSha256" })
            mutations["missing-" + property] = content => content.Remove(property);
        foreach (string schema in new[] { "entityRuntime", "aggregate", "checksum", "characterShell", "entityBaseShell" })
            mutations["old-" + schema] = content => content["schemas"]![schema] = 0;
        foreach (var retiredSchema in new Dictionary<string, int>
        {
            ["entityRuntime"] = 15, ["aggregate"] = 23, ["checksum"] = 26,
        })
        {
            mutations["retired-fusion-carrier-schema-" + retiredSchema.Key] =
                content => content["schemas"]![retiredSchema.Key] = retiredSchema.Value;
        }
        foreach (var retiredSchema in new Dictionary<string, int>
        {
            ["entityRuntime"] = 16, ["aggregate"] = 24, ["checksum"] = 27,
        })
        {
            mutations["retired-platform-carrier-schema-" + retiredSchema.Key] =
                content => content["schemas"]![retiredSchema.Key] = retiredSchema.Value;
        }
        foreach (var mutation in mutations)
        {
            RunCase(report, "Q05-" + mutation.Key, "invalid", null, () =>
            {
                string[] lines = SplitLines(authority);
                JsonObject header = JsonNode.Parse(lines[0])!.AsObject();
                mutation.Value(header["content"]!.AsObject());
                lines[0] = Serialize(header);
                var validation = TraceComparator.ValidateTextForTest(JoinLines(lines));
                return (validation.Status, validation.Reason, (string?)null);
            });
        }
        RunCase(report, "Q05-frozen-semantic-vector", "match", null, () =>
        {
            // Historical V2 vector remains evidence of the retired object-only math.
            byte[] input = Encoding.ASCII.GetBytes("NTSD28_LOGAN_DAT_SEMANTICS_V2\0")
                .Concat(Convert.FromHexString("4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C")).ToArray();
            byte[] semantic = SHA256.HashData(input);
            bool matches = Convert.ToHexString(semantic) == "DB579550BCEC0039383BB421B0F62FB9C741FA2BFD30212059F329B8CADA4407" &&
                BinaryPrimitives.ReadUInt64LittleEndian(semantic).ToString("X16") == "3900ECBC509557DB";
            return (matches ? "match" : "mismatch", (string?)null, (string?)null);
        });
        RunCase(report, "Q06-composite-formal-independent-vector", "match", null, () =>
        {
            var content = TraceContentIdentity.CreateLogan(
                "4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C",
                "28E1809EDE9C18E49629E6CBEC20CBD11FCB6E0175DF9DEE6ED90CE542886D5F",
                "81CA495386950C3F8D5F00B43A62934410D4F6F738CD7B720610241E88F8D369");
            TraceContentIdentity.Validate(content, true);
            bool matches = content["rawDefinitionSha256"]!.GetValue<string>() == "3A7FF5A15521B9766FC35BBF04B8FA9D3F4BDEA5C5D0045A578BA8B523C37CC4" &&
                content["semanticSha256"]!.GetValue<string>() == "FD18D668B9D4EF0FAD4EE3D8056F98754049B3F25FB6927EC562C3F60B008147" &&
                content["catalogFingerprint64"]!.GetValue<string>() == "0FEFD4B968D618FD";
            return (matches ? "match" : "mismatch", (string?)null, (string?)null);
        });
        RunCase(report, "Q06-composite-fixed-byte-independent-vector", "match", null, () =>
        {
            var content = TraceContentIdentity.CreateLogan(
                "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F",
                "202122232425262728292A2B2C2D2E2F303132333435363738393A3B3C3D3E3F",
                "404142434445464748494A4B4C4D4E4F505152535455565758595A5B5C5D5E5F");
            TraceContentIdentity.Validate(content, true);
            bool matches = content["rawDefinitionSha256"]!.GetValue<string>() == "C32F2109ABE09F390E5D13936B32E174AD704D64B1B56A9516D573BCF3BE55EF" &&
                content["semanticSha256"]!.GetValue<string>() == "68EE16B9C9BCAB61B44C040C057C7686E642662F25DA2B0999FD7359729D0111" &&
                content["catalogFingerprint64"]!.GetValue<string>() == "61ABBCC9B916EE68";
            return (matches ? "match" : "mismatch", (string?)null, (string?)null);
        });
        var compositeMutations = new Dictionary<string, Action<JsonObject>>
        {
            ["old-v2-tag"] = content => content["decodeContract"] = "NTSD28_LOGAN_DAT_SEMANTICS_V2",
            ["old-v2-property-set"] = content =>
            {
                content.Remove("objectDefinitionSha256"); content.Remove("fusionInputSha256"); content.Remove("fusionSemanticSha256");
                content["decodeContract"] = "NTSD28_LOGAN_DAT_SEMANTICS_V2";
                content["scope"] = "catalog-object-definitions";
            },
        };
        foreach (string component in new[] { "objectDefinitionSha256", "fusionInputSha256", "fusionSemanticSha256" })
        {
            compositeMutations[component + "-tampered"] = content => content[component] = new string('E', 64);
            compositeMutations[component + "-invalid"] = content => content[component] = "not-a-sha";
            compositeMutations[component + "-null"] = content => content[component] = null;
        }
        foreach (var mutation in compositeMutations)
        {
            RunCase(report, "Q06-composite-" + mutation.Key, "invalid", null, () =>
            {
                string[] lines = SplitLines(authority);
                JsonObject header = JsonNode.Parse(lines[0])!.AsObject();
                mutation.Value(header["content"]!.AsObject());
                lines[0] = Serialize(header);
                var validation = TraceComparator.ValidateTextForTest(JoinLines(lines));
                return (validation.Status, validation.Reason, (string?)null);
            });
        }
        RunCase(report, "Q06-one-component-logan-factory-rejected", "rejected", null, () =>
        {
            try { TraceContentIdentity.Create("logan-runtime", new string('A', 64)); }
            catch (InvalidDataException) { return ("rejected", (string?)null, (string?)null); }
            return ("accepted", (string?)null, (string?)null);
        });
        foreach (int componentIndex in new[] { 0, 1, 2 })
        {
            foreach (string? invalid in new string?[] { null, "", new string('G', 64), new string('A', 63) })
            {
                RunCase(report, "Q06-invalid-factory-component-" + componentIndex + "-" + (invalid ?? "null"), "rejected", null, () =>
                {
                    string[] components = { new string('A', 64), new string('C', 64), new string('D', 64) };
                    components[componentIndex] = invalid!;
                    try { TraceContentIdentity.CreateLogan(components[0], components[1], components[2]); }
                    catch (InvalidDataException) { return ("rejected", (string?)null, (string?)null); }
                    return ("accepted", (string?)null, (string?)null);
                });
            }
        }
        RunCase(report, "Q06-legacy-profile-unchanged", "match", null, () =>
        {
            var content = TraceContentIdentity.Create("unity-legacy", new string('A', 64));
            TraceContentIdentity.Validate(content);
            byte[] semantic = SHA256.HashData(Encoding.ASCII.GetBytes("NTSD28_UNITY_LEGACY_DAT_SEMANTICS_V1\0")
                .Concat(Convert.FromHexString(new string('A', 64))).ToArray());
            bool matches = content.Count == 8 && content["scope"]!.GetValue<string>() == "unity-legacy-dat-files" &&
                content["semanticSha256"]!.GetValue<string>() == Convert.ToHexString(semantic);
            return (matches ? "match" : "mismatch", (string?)null, (string?)null);
        });
        RunCase(report, "Q05-source-missing-2f8-rejected", "invalid", null, () =>
        {
            string[] lines = SplitLines(authorityCapture);
            JsonObject tick = JsonNode.Parse(lines[1])!.AsObject();
            tick["entities"]![0]!["combat"]!.AsObject().Remove("objectAiExcludedGroupSourceSlot");
            lines[1] = Serialize(tick);
            var validation = AuthorityCaptureValidator.ValidateTextForTest(JoinLines(lines));
            return (validation.Status, validation.Reason, (string?)null);
        });
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

        return EntityFieldContract.Fields.Length == 50 &&
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
            ["content"] = TraceContentIdentity.CreateLogan(contentManifest, new string('C', 64), new string('D', 64)),
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
                ["objectAiExcludedGroupSourceSlot"] = -1,
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
            ["content"] = TraceContentIdentity.CreateLogan(new string('A', 64), new string('C', 64), new string('D', 64)),
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
