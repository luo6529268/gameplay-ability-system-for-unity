# NTSD28-Q09-COM-LABEL-TEST-CONSTRUCTOR-001

Status: FOCUSED_TEST_PASS. Parent: Q09 battle presentation regression.

Authority and trigger: `BattlePresentationCommandWriterEditorTests.CentralOnly_GenericComUsesRelationSheetComposite_LegacyKeepsThreeGlyphs` stops before command assertions because its reflection fixture requests a six-argument `BattleCommonVisualCatalog` com-label constructor. The current project constructor in `BattleSpriteCatalog.cs` takes seven arguments, adding optional `bool nativeSpark`. This is a test-fixture signature drift; it is not evidence of a battle render defect.

Scope: change only the com-label constructor signature and invocation in `Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs`. Pass `false` for the optional flag to retain the previous fixture's intended non-native spark behavior. Preserve the existing uncommitted platform-shadow test in this file. Do not change production, DAT, pictures, mode Asset, Scene, or nonbattle behavior.

Acceptance: pre-register this Task, Change Record, Ledger, STATE, handoff and alignment before editing the test script. Then compile in the original Editor and run this single previously failing test, followed by this seven-case test class only if the single case passes. Report any remaining assertion failure as a separate first difference. Verify protected Scene/Asset hashes, `git diff --check` and `Tools/Validate-ChangeLedger.ps1`.

Risk and rollback: reflection tests can become stale when private constructors change. A focused seven-argument update is reversible by removing only these two new lines; preserve all pre-existing hunks and user files.

Result: the original Editor compiled the two-line test fixture change with zero current Console errors. The formerly blocked test passed 1/1, then the eight exact tests discovered in this class passed 8/8. This closes only the fixture regression. It does not prove platform-shadow GPU ownership or formal EXE visual parity. See the package acceptance report.
