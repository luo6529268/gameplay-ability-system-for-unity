# NTSD28-Q09-TRUSTED-COMMAND-TEST-CONSTRUCTOR-001

Status: `FOCUSED_TEST_PASS`. Parent Q09 presentation regression gate, discovered while validating `NTSD28-Q09-BPOINT-BLEED-CENTRAL-001`.

Observed original-project Editor evidence: exact P-08 command/resolver tests 2/2 PASS, then selected adjacent two classes 28/32 PASS. Four old resolver tests fail before resource assertions at `CreateTrustedCommandWithIdentity`: its reflection signature ends after `HasStableHealthAnchor`, while the current internal `BattleRenderCommand` constructor also takes `StableFootAnchorWorld`, `HasStableFootAnchor`, `ShowSelfFootMarker`, `FootMarkerScale`. The four failures share `Expected: not null; But was: null` at the constructor lookup. They are an old test fixture signature gap, not a demonstrated central render regression.

Declared script scope: only `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs`, only the reflection signature and invocation in `CreateTrustedCommandWithIdentity`. Preserve the P-08 test added nearby and all production code. No DAT/PNG/Scene/Prefab/ProjectSettings/nonbattle change; no new owner/shutdown phase.

Acceptance: exact four previously failing tests and the 23-test resolver class PASS in the original Editor after compile0; selected 9-test command-writer class remains PASS. Preserve the initial failed 28/32 job as evidence. Run validator and diff check. This fixes only the fixture and does not prove P-08 GPU/Play/EXE parity. Rollback reverses only four reflected type/value additions after review, preserving all other dirty work.

Exit: the original failed selected-class job remains `10e3fd596ba946b282e057637dd12dcf` (28/32); after the four reflected type/value additions, original-Editor job `13260ab2c3614e79a4107c92dc026c78` passed 32/32 with zero current compile errors. Ledger 860/20 and diff check passed. Production constructor unchanged.
