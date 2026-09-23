<!-- CHANGE-RECORD
id: NTSD28-R15-B0-UNITY-V2-PRODUCER-001
status: FOCUSED_TEST_PASS
change-kind: TRACE_DIAGNOSTIC_UNITY_PRODUCER_V2
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: formal kind source-model same-epoch ID mutation and strict B0 v2 diagnostic contract NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001
evidence: Unity raw capture writer still emits B0 v1 and rejects same-slot/same-epoch object-ID change
-->

# NTSD28-R15-B0-UNITY-V2-PRODUCER-001

Before: Unity Editor diagnostic capture defaults to B0 raw v1, with an `aiAcceptedTrace` Unity-only field that the parity tool already validates strictly. Existing requests, neutral captures and previous evidence are user-owned and must remain valid.

After intended: explicit B0 v2 emits the same strict six-field `object-id-change` observation as the source-model producer, preserving v1 default behavior and every gameplay/runtime field. No type3/action initial-state extension is included.

Scope, invariants, focused acceptance, rollback and remaining gates are in the matching Task. Actual changed symbols and validation outcomes will be appended after implementation.

Actual implementation: only the two declared Editor diagnostic files changed. An optional request/test `domainVersion` accepts exact `v1` or `v2` only when a domain output is supplied. Omitted version keeps v1. The version passes through the header/tick writer; v2 uses raw schema v2 and derives the six-field `object-id-change` when a positive allocation epoch is unchanged but ID differs. V1 still rejects the transition. `DomainOccupant` and the derivation helper were made `internal` solely for focused synthetic tests; no runtime or production gameplay type changed. Existing Unity-only `aiAcceptedTrace`, main/B2 streams and saved Scene behavior were preserved.

Validation: two preliminary offline Editor MSBuild attempts failed because the generated project referenced unavailable `Temp/bin/Debug` DLLs; those are harness failures and do not count as test RED. A Temp-only targets override using existing original-project `Library/ScriptAssemblies` references produced offline Editor assembly compile exit0 (`Temp/r15-unity-v2-green3.log`). The original project Editor PID173216, version 2022.3.62f3, then accepted a scripts compilation request and completed domain reload. Its focused EditMode TestRunner job `1da70265be714720a9316334ce93c7e7` passed 7/7: v2 neutral capture, four invalid-version/missing-domain cases, exact same-epoch event/v1 rejection, and existing default-v1 input capture. The generated v2 neutral B0 file passed strict `validate-b0-domain-raw`: 3 ticks, `unity-diagnostic`, valid=true (`Temp/diagnostics/NTSD28-R15-B0-UNITY-V2-PRODUCER-001-domain-validation.json`). Two-file `git diff --check` passed. No broad battle suite was run.

Remaining: actual kind-dependent Unity capture still needs a correct formal type3/action initial-state carrier and then main/B2/B0 same-scenario comparison. v2 neutral capture and synthetic event test do not prove that Unity gameplay transforms OID206 into 213. Formal EXE observable parity, physical input and presentation remain open; Q07/R15/total goal open.
