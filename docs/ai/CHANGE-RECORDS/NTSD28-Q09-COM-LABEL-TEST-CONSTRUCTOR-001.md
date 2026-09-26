<!-- CHANGE-RECORD
id: NTSD28-Q09-COM-LABEL-TEST-CONSTRUCTOR-001
status: FOCUSED_TEST_PASS
change-kind: Q09_EDITOR_TEST_FIXTURE_SIGNATURE
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
authority: current BattleCommonVisualCatalog private constructor and Q09 adjacent focused test failure
evidence: docs/ai/TASKS/NTSD28-Q09-COM-LABEL-TEST-CONSTRUCTOR-001.md; artifacts/diagnostics/NTSD28-Q09-PLATFORM-SHADOW-OFFSET-PUBLICATION-001/ACCEPTANCE-20260926.md
-->

# NTSD28-Q09-COM-LABEL-TEST-CONSTRUCTOR-001

Pre-change: the com-label fixture's reflection lookup and `Invoke` use six arguments. Current `BattleCommonVisualCatalog` constructor has seven including optional `nativeSpark` bool; reflection does not apply the C# optional default. The test aborts at its constructor-not-null assertion before drawing. The same test file has a pre-existing uncommitted platform-shadow test hunk; preserve it.

Declared edit: add `typeof(bool)` to this one reflection signature and `false` to its matching invocation. No production or resources. The previous test fixture semantics are `nativeSpark=false`.

Expected side effects: the test can now reach its intended CentralOnly/Legacy com-label assertions; an assertion failure there is not pre-authorized to weaken expectations. Acceptance, protected files and rollback are in the Task Contract.

Code written: added `typeof(bool)` to the com-label reflection signature and `false` to its matching `Invoke` argument list. The pre-existing platform-shadow test hunk in the same file remains unchanged.

Validation: original Editor PID11944, project path and MCP port6402 verified before use. After `Assets/Refresh`, Editor assembly timestamp was newer than the final test script; `read_console` Error filter returned zero. Exact formerly blocked test job `fe0daaae3ce94054b2e5760c21b5f855` passed 1/1. Exact eight-test class job `81707241ca2a4013948bf66ec36b116d` passed 8/8, including both existing platform-shadow assertions. Menu/Battle/GameConfig/ProjectBattleModeConfig disk SHA-256 values remained at their protected baselines; Scene Git status was clean. `git diff --check` exited zero. The class now has eight tests, so the Task's earlier “seven-case” count was a pre-run estimate, corrected by `get_tests` exact discovery. No production/Asset/DAT/Scene change was made by this package. Pixel ownership and formal EXE parity remain unverified.

Final governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <project>` exited zero (`PASSED`, 858 Records and 16 governed code files in the current dirty diff). The three live progress documents each contain zero NUL bytes. No aggregate SelfCheck or Play run was needed for this test-only signature repair; previous Play platform-shadow pixel status remains `PIXEL_OWNERSHIP_UNPROVEN`.
