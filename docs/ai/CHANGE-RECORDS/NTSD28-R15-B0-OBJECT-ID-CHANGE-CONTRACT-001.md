<!-- CHANGE-RECORD
id: NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001
status: FOCUSED_TEST_PASS
change-kind: TRACE_DIAGNOSTIC_CONTRACT_VERSION
code-path: Tools/NTSD28Parity/B0DomainRawContract.cs
code-path: Tools/NTSD28Parity/B0DomainRawContractSelfTest.cs
code-path: Tools/NTSD28Parity/B0DomainRawComparator.cs
code-path: Tools/NTSD28Parity/B0DomainRawComparatorSelfTest.cs
authority: formal playable BattleWorld28 kind transform and NTSD28-R15-KIND-DEPENDENT-SOURCE-PREFLIGHT-001
evidence: source-model OID206 to OID213 same-slot same-epoch tick 1; B0 v1 rejects occupant-changed-without-allocation-epoch
-->

# NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001

Before: B0 raw v1 permits birth, death and reuse; same-epoch object-ID change is rejected. Existing v1 and Unity-only AI diagnostic extension are user-owned working-tree changes and must be preserved.

After intended: a distinct opt-in v2 B0 raw contract admits a strict snapshot-derived `object-id-change` event for this observation while v1 behavior and captures remain unchanged. Comparator rejects mixed versions. This is not a battle-rule or producer change.

Invariants: six lifecycle event fields remain exact; event order is ascending slot; epochs remain positive and nondecreasing; shared input/RNG/slot contracts are unchanged; `certificateEligible=false`; formal EXE parity remains unclaimed. No producer can emit v2 in this package.

Validation and rollback are specified in the matching Task. Actual files, tests, first differences, and remaining producer/runtime gates will be appended after implementation.

Actual change: only the four declared `Tools/NTSD28Parity` files changed for this package. `B0DomainRawContract` adds explicit raw v2 and descriptor v2, carries schema through validation, and derives the exact six-field `object-id-change` event only for same positive epoch/different ID in v2. V1 still rejects that state. `B0DomainRawComparator` rejects mixed raw schemas before comparing. Focused synthetic cases cover valid and malformed v2 transitions, v1 compatibility, and mixed-version rejection. The pre-existing Unity-only `aiAcceptedTrace` extension in the two already-dirty contract/test files was preserved.

Actual validation: test-first RED outputs `Temp/b0-v2-contract-red.json` (28 cases, 2 expected failures) and `Temp/b0-v2-comparator-red.json` (9 cases, 3 expected failures). `dotnet build Tools/NTSD28Parity/NTSD28Parity.csproj --no-restore` passed with 0 warnings/0 errors. Final `self-test-b0-domain-raw` passed 33/33 and `self-test-b0-domain-comparator` passed 11/11 (`Temp/b0-v2-contract-green.json`, `Temp/b0-v2-comparator-green.json`). Existing formal-source and Unity v1 three-tick captures from `NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001` each validated. Four-file `git diff --check` passed. One first attempt to validate a historical capture used unsupported `--input`; it reported `Missing required option --capture`, then the correct `--capture` command succeeded.

Remaining: source and Unity producers still emit v1; the formal kind-dependent three-tick B0 capture, Unity type3/action scenario, cross-side comparison, original Unity runtime, and formal EXE observable parity remain pending. This package cannot promote R15/Q07 or the total goal to aligned. Certificate remains false.
