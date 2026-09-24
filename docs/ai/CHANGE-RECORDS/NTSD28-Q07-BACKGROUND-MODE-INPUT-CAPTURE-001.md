<!-- CHANGE-RECORD
id: NTSD28-Q07-BACKGROUND-MODE-INPUT-CAPTURE-001
status: ABANDONED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganBackgroundModeInput.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07BackgroundModeInputEditorTests.cs
authority: locked formal NTSD2.8-Logan playable game_session.cpp background-mode loader and selected record projection; user D-023 formal DAT bytes
evidence: artifacts/diagnostics/NTSD28-Q07-BACKGROUND-MODE-INPUT-CAPTURE-001/TASK-CONTRACT.md
-->

# NTSD28-Q07-BACKGROUND-MODE-INPUT-CAPTURE-001

Before code: Unity has no generic `data/bg_mode.dat` / `data/bg/*.dat` capture path, despite the exact 26 formal DATs now being staged. This change adds only a pure immutable input reader and focused tests; it must not publish the input, alter content identity, select a menu option, write World state or change battle behavior. Owned symbols are `LoganBackgroundModeInput.Capture`, `TryGetRecord`, `AssertInputsCurrent` and the focused test class. Expected side effect is only new source/test files and Unity-generated metas. Rollback is removal of these package-owned additions only after the repository's required deletion approval; no current user or legacy files may be overwritten. Acceptance is original Editor compile, focused corpus and malformed/freshness tests, unique GUIDs, stable Scene hashes, ledger/diff validation. The subsequent identity/runtime projection requires a separate Task/Change with the complete field-reader and snapshot contract.

Actual diff: added only `LoganBackgroundModeInput.cs` and `NTSD28Q07BackgroundModeInputEditorTests.cs`. `Capture` resolves the formal parent and all ordered group descriptors through the existing content source root, parses each child with the existing DAT tokenizer, retains C++ zero-initialized missing integer defaults and ordered repeated drop IDs, and exposes group/record lookup plus raw-input and projected-semantic SHA-256. It returns null only when the parent is absent, and rejects a present parent with missing/invalid child data. `AssertInputsCurrent` checks the captured bytes before later publication. The test class covers current formal/staged corpus, mode indices 0/1/2, random/story descriptors, missing/duplicate/escaping paths, zero defaults and byte freshness. Content identity and World remain unchanged.

2026-09-24 correction: user explicitly clarified that this project does not use original NTSD background or background-mode DAT. The D-023 authority inference for these two classes was wrong. This package is `ABANDONED`; the four new script/meta files remain on disk pending the repository's required explicit deletion approval, and no production reader was connected. The original Editor was refreshed and compiled, but the test bridge ignored the requested filter and started an unrelated full EditMode suite. That suite is not focused acceptance; cancellation/status is being checked separately. No behavior or parity claim is made.
