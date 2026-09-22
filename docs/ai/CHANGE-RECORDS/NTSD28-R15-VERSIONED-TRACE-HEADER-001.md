<!-- CHANGE-RECORD
id: NTSD28-R15-VERSIONED-TRACE-HEADER-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28TraceContentIdentity.cs
code-path: Tools/NTSD28Parity/TraceContentIdentity.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionCompositeContentIdentityEditorTests.cs
authority: formal Logan playable selected mode load, Q07 production V2 identity, and R15 trace contract
evidence: artifacts/diagnostics/NTSD28-Q07-MODE-COMBO-R15-IDENTITY-IMPACT-001/REPORT.md
-->

# NTSD28-R15-VERSIONED-TRACE-HEADER-001

Created before script modification. Current Unity trace output may carry the production V2 composite digest under a V1-only object/fusion header. The parity validator accepts V1 with hard-coded schema 17/25/28/2/2, whereas current Unity runtime emits 17/26/29/2/2. This is a trace contract defect and validation blocker; it is not evidence of a gameplay first difference. The linked Task Contract fixes exact paths, authority, invariants, acceptance and rollback.

Expected change: produce and validate strict V1 and V2 headers; retain historical V1 bytes; reject unknown/mixed contracts, invalid digest/projection, and mismatched schema before tick comparison. Current V2 mode is expected only with schema 17/26/29/2/2. Update the two directly affected formal-root Editor assertions, preserving absent-mode V1 fixture assertions. No game rule, scene, resource, native formal source, UI, producer, runtime owner or eleven-stage shutdown change. Historical native V1 captures remain diagnostic V1 and may not be upgraded by relabeling.

Validation plan: parity build and focused self-tests, exact V1/V2 vectors, V2 malformed/cross-version/schema rejection, then original-project Unity EditMode tests when newly compiled. The Q07 published-activation code, native V2 diagnostic capture, fixed Play probes and same-seed/input formal comparison are separate dependencies. Until fresh original-project Unity evidence exists, report only code/build/tool test status. Rollback by reviewing and reversing only the five declared code-file changes; never reset or clean unrelated dirty work.

Actual code: the Unity emitter now chooses V1's original scope/property set when mode is absent and V2's explicit input contract, mode hashes and mode scope when present; it rejects missing/mixed mode components. The parity validator retains V1's exact historical header and adds strict five-component V2 reconstruction, exact old/current schema tuple validation, and version/schema-bearing comparison keys. Self-tests cover V1/current, V2/current, both cross-version directions, mismatched schema, same-version V2 first tick difference, and malformed components/properties. The two formal-root Editor assertions now expect the independently rechecked V2 raw `33B0341C58D2A708B208F2CA4A0C4DA740E942D49A8A9D82E106F1D1D9C419BE`, semantic `FF1218FF3FEB409FF6B2F8EDB1090591612B3D82D7FA91601E596D29CDF13DFB` and projection `9F40EB3FFF1812FF`, while their synthetic no-mode V1 fixture stays intact.

Actual validation: `artifacts/diagnostics/NTSD28-Q07-MODE-COMBO-PUBLISHED-ACTIVATION-001/pure-check.ps1` reran exit 0 and independently rechecked the formal/staged V2 vector. `dotnet build Tools/NTSD28Parity/NTSD28Parity.csproj -c Release` passed with zero warnings/errors; tool `self-test` passed 157/157, archived at `artifacts/diagnostics/NTSD28-R15-VERSIONED-TRACE-HEADER-001/parity-self-test.json`. These prove the parity tool contract on synthetic traces, not native V2 parity or Unity compilation. Original Unity Editor assemblies still predate the new Q07 code; the emitter and two EditMode tests have not run there. Other fixed V1 formal-root probes are separately identified and not yet migrated. Native source-model runner remains historical V1/25/28, and same-seed/input first difference remains untested. Status `CODE_WRITTEN / TOOL_BUILD_AND_SELF_TEST_PASS / UNITY_COMPILE_PENDING / R15_RUNTIME_PENDING`.

Independent read-only review found no confirmed P1/P2 emitter or validator defect. It found one P2 regression in the migrated formal fusion test: comparing V2 formal identity with a three-argument V1 fallback would make the expected inequality trivially true. That test now asserts the fallback input hash differs and compares two five-component V2 identities with the same mode hashes, varying only fusion input. This correction has passed diff checking but remains uncompiled/unrun in the original Unity Editor.
