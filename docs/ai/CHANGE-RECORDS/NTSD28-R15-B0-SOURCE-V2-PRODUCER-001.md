<!-- CHANGE-RECORD
id: NTSD28-R15-B0-SOURCE-V2-PRODUCER-001
status: FOCUSED_TEST_PASS
change-kind: TRACE_DIAGNOSTIC_SOURCE_PRODUCER_V2
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
authority: formal playable BattleWorld28 kind transform and B0 strict v2 contract NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001
evidence: NTSD28-R15-KIND-DEPENDENT-SOURCE-PREFLIGHT-001 source-model slot1 OID206 to 213 at epoch1 while B0 v1 rejects the observation
-->

# NTSD28-R15-B0-SOURCE-V2-PRODUCER-001

Before: the current diagnostic runner emits only B0 raw v1; its same-epoch object-ID check throws on the kind-dependent formal-source scenario. The file already has user-owned V3 kind content-identity changes. Keep them intact.

After intended: default v1 remains unchanged; explicit v2 output writes the strict snapshot-derived identity mutation event and correct v2 header, preserving the existing six event fields and all other streams. This is a diagnostic-format extension, not a battle rule, allocation epoch, or formal source change.

Invariants, exact file boundary, negative cases, validation and rollback are in the matching Task. Actual code diff, test results, evidence paths and remaining Unity/formal-EXE gates will be appended after implementation.

Actual implementation: only the declared diagnostic runner file changed for this package. `Options28`/`parse_options` accept explicit `--domain-version 1|2` only alongside `--domain-output`, with default v1 and duplicate/unknown option rejection. `write_domain_header` emits the selected raw schema; `write_domain_events` emits a six-field `object-id-change` for same-slot/same-epoch/different-ID only under v2. Existing V3 kind content-identity changes in the same dirty file were preserved. No allocation epoch, World, gameplay or formal J: source change.

Actual validation: `Build-AuthoritySourceCapture.ps1` completed exit0 against formal EXE SHA `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, source manifest `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`. The kind scenario produced complete main/B0/B2 three-tick bundles twice, each stream byte-identical across runs. All six validators passed; tick1 B0 has slot0 death and slot1 ID206->213 with both epochs1. Main tick rows and entire B2 match the prior B0-disabled source capture; main header changed only runner/binary provenance SHA. Default neutral three-tick B0 remains v1, and its three validators passed. Explicit v1 on the kind scenario still exits91 and marks all three outputs invalid. Unknown version3, version2 without domain output, duplicate version and unknown option all exit2. `git diff --check` passed. Machine-readable evidence is `Temp/diagnostics/NTSD28-R15-B0-SOURCE-V2-PRODUCER-001/focused-evidence.json`; report under the matching artifacts directory.

Remaining: Unity diagnostic writer and type3/action initial scenario do not yet produce the same v2 comparison. This remains source-model-only evidence, `certificateEligible=false`; Q07/R15 and total goal stay open.
