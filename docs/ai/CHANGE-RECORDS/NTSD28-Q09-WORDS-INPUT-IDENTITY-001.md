# NTSD28-Q09-WORDS-INPUT-IDENTITY-001

Status: `PLANNED` (2026-09-22). Parent: BATCH-05/Q09, R17. Pre-change Task Contract: `artifacts/diagnostics/NTSD28-Q09-WORDS-INPUT-IDENTITY-001/TASK-CONTRACT.md`.

Authority and original state: formal playable `GameSession28::initialize_resources` resolves resource indices 16..21 for in-battle WORDS glyphs. Current `LoganVisualContentCandidate.Capture` hashes only actor `files/head/small`; `VisualFingerprint` uses `LOGAN_VISUAL_INPUTS_V1`, so six staged global PNGs and their selecting `resource.dat` are absent from publication freshness. `CharacterAnimtorManager` currently does not publish WORDS; that is a later behavior package.

Declared exact script paths/symbols: `Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs` (`Capture`, visual fingerprint and `AssertInputsCurrent`, new immutable WORDS input projection); `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09WordsInputIdentityEditorTests.cs` and its `.meta` (focused input/identity cases). No other production scripts. Expected side effect: formal-root visual cache key changes to a new V2 preimage when `resource.dat`/six selected PNGs are present; absent-resource fixtures keep V1. Battle-rule identity, World, tick, menu, legacy roots, existing owner resources and published visuals do not change. A missing/corrupt selected input fails before publication.

Acceptance: exact index selection, formal/staged fingerprint match, absent V1 stability, changed/appeared/removed input rejection, path containment, independent current-source compile, original Editor focused tests after fresh compile. Risks: cache invalidation and fixture assumptions; inspect all callers and run focused cases. This Record cannot be VERIFIED from external compilation alone. Rollback: review and reverse only its exact two scripts and new test meta; retain unrelated dirty work and all staged formal resources. Do not delete files or reset Git without explicit authorization.
## Actual change and evidence (2026-09-22)

Status: `CODE_WRITTEN / OFFLINE_COMPILE_PASS / ORIGINAL_UNITY_PENDING`. `LoganVisualContentCandidate.cs` now has a nested immutable `NativeWordsInput` that selects the first native `resource.dat` table indices 16..21, resolves within the VFS, hashes raw DAT plus selected PNG bytes, and exposes six selected image inputs. `Capture` adds its fingerprint to a `LOGAN_VISUAL_INPUTS_V2` preimage only when `resource.dat` exists; absent input keeps the exact prior V1 branch and 906 actor-image list. `AssertInputsCurrent` recaptures and compares optional WORDS input before publication. The only new test is `NTSD28Q09WordsInputIdentityEditorTests.cs` plus its `.meta`; no existing test/script outside the declared scope changed by this Record.

Validation: `dotnet msbuild Assembly-CSharp-Editor.csproj -t:Build` with exact missing Compile items in Temp-only targets exited 0 after the final edit; evaluated new Editor test included. Reflection against that compiled actual runtime DLL selected six formal/staged images and identical fingerprint `0E963A3DE1F56B71F9DB276EFB79D7010B1CD3B08B84E83D0DD9009B894C5B24`; independent Temp fixtures passed absent, image byte change, resource DAT byte change and path traversal checks. See `artifacts/diagnostics/NTSD28-Q09-WORDS-INPUT-IDENTITY-001/ACCEPTANCE-PENDING.md`. Original Unity import/NUnit/Play, visual publication and formal pixels remain unverified. No second Editor or computer-use. Rollback remains exact diff review only.
<!-- CHANGE-RECORD
id: NTSD28-Q09-WORDS-INPUT-IDENTITY-001
status: COMPILE_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09WordsInputIdentityEditorTests.cs
authority: formal NTSD2.8-Logan playable game_session.cpp resource indices 16..21 and render_snapshot.cpp battle glyph selection
evidence: artifacts/diagnostics/NTSD28-Q09-WORDS-INPUT-IDENTITY-001/ACCEPTANCE-PENDING.md
-->
Governance check after the code/test diff: `Tools/Validate-ChangeLedger.ps1` exited 0, `Change ledger validation PASSED`, 685 Records and 22 governed code files in the current dirty diff. This Record covers the new Q09 test and the shared visual-candidate script; the other covered files belong to pre-existing changes. `git diff --check` exited 0. The original Editor has not refreshed its `Library/ScriptAssemblies`; these checks do not advance the Record beyond `CODE_WRITTEN`.
The separate offline attempt to invoke the **full** visual candidate stopped at unresolved UnityEngine/UniTask dependencies in PowerShell before reaching `Capture`; it is not a test result for full publication identity. The compiled nested WORDS input projection did execute. The original Editor test is still required.

2026-09-22 current-state correction: original Editor PID 33236 regenerated runtime/Editor assemblies at 07:58:15/19Z after the candidate and its focused test source timestamps and logged `Mono: successfully reloaded assembly`. The previous “original Editor has not refreshed” sentence above is a historical snapshot. Status advances to `COMPILE_PASS` for original-project Unity compilation; focused NUnit, production pixel and runtime freshness acceptance remain pending.
