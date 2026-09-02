# GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001

<!-- CHANGE-RECORD
id: GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001
status: BLOCKED
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FormalContentClosureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/UnityPresentContentAuthorityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: User Direction B; Unity current DAT values are content authority while C++ release remains battle-rule/order authority.
evidence: IN_PROGRESS / PRE_CHANGE_SOURCE_DIFF_CLEAN / FIXTURE_SHA_FROZEN / CONFIG_RESTORE_EXPLICITLY_AUTHORIZED / S0_NOT_VERIFIED
-->

> Status: `BLOCKED / DIRECTION_B_GOVERNANCE_WRITTEN / CONFIG_HEAD_EQUIVALENCE_PROVEN / RAW_MANIFEST_FROZEN / PRODUCTION_ROLLBACK_CODE_WRITTEN / FIXTURE_CLEANUP_COMPLETE / AUTHORITY_CAPTURE_FIXTURE_WRITTEN / UNITY_COMPILE_BLOCKED_EXTERNAL_SIMULATION_REORGANIZATION`

The Server same-ID Task/Change owns the complete decision, exact source
rollback, fixture SHA table, invariants, validation and rollback. This Client
mirror exists before script/test changes as required by `AGENTS.md`.

Observed facts before mutation:

- Config, ParserV2, Converter and BattleRuntimeSelfCheck have no uncommitted diff;
- only three package TSVs exist: Bdy, OPoint and WPoint;
- CAP-only parser behavior can be removed without reverting independent Itr
  multivalue or immutable value seams;
- no CAP-only BattleRuntimeSelfCheck contract was actually added, so the
  expected SelfCheck source diff is zero.

The exact authorized Config `git restore` command was attempted and denied
before worktree write because `.git/index.lock` is protected. No bypass or
retry occurred. Read-only proof establishes worktree/index clean, zero
untracked Config files, 138 DATs and raw manifest SHA-256
`d32b49d32b57c21e5743a4103fee8152a58bba0add01c2446d2d76a52d1a0346`.

Actual source/cleanup evidence and the external Simulation-reorganization
compile blocker are recorded in the Server same-ID Change. No SelfCheck or
focused result exists yet.

Scoped Direction-B `git diff --check` passes. The Client-wide Ledger still
fails only on the pre-existing unrelated
`CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001` missing `code-path` metadata.
