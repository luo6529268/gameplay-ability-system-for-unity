<!-- CHANGE-RECORD
id: NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001
status: FOCUSED_TEST_PASS
change-kind: DIAGNOSTIC_AUTHORITY_SOURCE_MODEL_KIND_V3
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
authority: formal playable game_session.cpp kind selection and KindCatalog28; Unity V3 trace identity contract; D-023 formal kind.dat
evidence: fresh formal source-model V3 capture twice byte-identical, source validators main/domain/B2 pass, original Unity same-seed 3tick raw 300/300 equal and B2 equal; B0 Unity schema mismatch remains
-->

# NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001

Before/after, exact paths, expected side effects, invariants, validation and rollback are in the matching Task. The only intended behavior change is diagnostic V3 content identity/freshness for newly generated source-model traces. Historical V1/V2 files, native formal source, Unity production, saved Scenes and nonbattle behavior stay untouched. Formal EXE observable parity, B0 domain comparison and kind-dependent runtime scenarios remain separate gates.

Implemented: the declared runner now resolves selected kind DAT or formal locked fallback, parses with formal KindCatalog28, computes Unity-compatible kind input/semantic digests, composes V3 or V3_KIND_ONLY, emits exact content fields and current 17/28/31/2/2 compatibility tuple, and rechecks kind path/priority/digests at both freshness gates. The old JSONL envelope name v2 is unchanged; its content identity is versioned V3. No formal source or Unity production script was edited.

Validation: fresh build from formal source with runner SHA 7E2E023D4F0A69D12A09ABAF77BB282053A9E1F3022053C1E4C104E31391D8BA, binary SHA 017C9528A680AD3D384A491D047CE869F4019D8ECE46FFDB30AFB0A7CF6D72C0; manifest SHA values rechecked against files. Formal source-model three-tick main capture ran twice byte-identically, semantic B8B13894088DDE96D71771C9110FBD84E2C03AE712FE32222B99C5DFE8155A45 / projection 96DE8D089438B1B8 matches Unity; source main/domain/B2 validators passed. No-mode/locked-kind V3_KIND_ONLY fixture validated. Invalid selected kind failed closed exit 91; high-priority kind appearance after visible header failed exit 91 and invalidated all three streams. Same-path byte mutation not directly tested.

Original Unity Editor was idle outside Play; existing request-file capture returned PASS for the same formal root/scenario/seed/ticks. Main raw comparison: 3 ticks, 6 pairs, 300/300 field occurrences equal, no first difference. B2 joint input/RNG comparison: equal for 3 ticks/6 pairs. B0 domain comparison was stopped by Unity tick field aiAcceptedTrace not declared in current B0DomainRawContract TickProperties; Unity B0 validator returned invalid/tick-properties. It is not a domain equality result. This and kind-dependent real battle Scene behavior, Player cold start and formal EXE observable comparison remain open. The diagnostic REPORT and raw JSON/JSONL artifacts preserve exact evidence. The initial parallel dotnet build file-collision was resolved by serial --no-build invocations; only latter validations count. Scene SHA unchanged.
