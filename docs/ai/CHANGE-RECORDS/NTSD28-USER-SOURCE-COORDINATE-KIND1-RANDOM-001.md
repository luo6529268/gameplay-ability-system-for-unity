<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-KIND1-RANDOM-001
status: FOCUSED_TEST_PASS
change-kind: D024_KIND1_SOURCE_RULE_RANDOM_HISTORY
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeOpointBirthWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OpointMaterializerEditorTests.cs
authority: user D-024 ratio decision and paired formal playable BattleWorld28 kind1 synchronized random and integer-to-precise reprojection
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-KIND1-RANDOM-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-KIND1-RANDOM-001

2026-09-24 implementation: `BattleNativeOpointBirthWriter.InitializeBirth` now stores the existing X and Z `RandomDelta` return values once, preserves the X/Y/Z/action call sequence and physical battle formula, and adds those raw values to initialized source-rule integer X/Z before copying integers to source precise X/Z. Zero amplitude still performs integer-to-precise reprojection, while absent source history stays absent. Only the two declared script files changed under this package; no new RNG, source consumer activation, DAT or scene change.

Original Editor test-first job `08ea61459cf14604a34e6187faeeb68f` ran nine cases: three expected failures. Initialized nonzero source X expected105 but remained-42 in factor1 and configured view; zero-amplitude precise source X expected-14 but remained-12.75. After production change and original Editor compile/domain reload, focused job `9ba815d926e04554851867dbbb794782` passed14/14 in1.079s. Nine new cases cover initialized/absent, factor1/2048x1152, nonzero/zero X/Z amplitudes, explicit independent source integers, battle scaling and integer mirrors. The random observer confirms call sites 0x0044D3AB/0x0044D3B5 twice in X then Z order with bounds600/50/80/50 and four extra synchronized calls; the unchanged existing random-ratio control and first formal materializer World/component fixtures also pass. DAT variation exists only in memory.

Validation: `Tools/Validate-ChangeLedger.ps1` PASS746 Records (`Temp/NTSD28-USER-SOURCE-COORDINATE-KIND1-RANDOM-001-ledger.log`); scoped `git diff --check` PASS; Battle Scene SHA `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0` unchanged; Content/Config/Scene/ProjectSettings scoped status clear. No full suite or Play rerun was needed because no lifecycle discrepancy appeared in this shared writer increment. State `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`; remaining births, motion, collision/attachment histories and OID219/fusion reader acceptance remain open. Rollback only this writer's raw-delta reuse/source assignment and these new focused tests, preserving earlier work. The following pre-script paragraph is retained as history.

Pre-script record. Unity currently scales the already drawn kind-1 random X/Z deltas into physical battle coordinates, but leaves the new independent source-rule coordinate history at its pre-random birth. This is a confirmed writer omission from source inspection, with focused first-difference/runtime evidence pending. The writer must reuse the same raw deltas and update initialized source integer and precise positions in the formal post-spawn order. It must not add RNG calls, change physical motion, edit DAT data, or activate gameplay source-rule readers. Expected side effects are limited to versioned/checksummed source-rule fields. Acceptance and rollback are defined in the Task. Status remains PLANNED until test-first and production evidence are recorded.
