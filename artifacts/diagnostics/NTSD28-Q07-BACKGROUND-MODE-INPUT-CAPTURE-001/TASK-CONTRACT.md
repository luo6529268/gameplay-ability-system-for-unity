# NTSD28-Q07-BACKGROUND-MODE-INPUT-CAPTURE-001

Status: `PLANNED` (2026-09-24). Parent BATCH-04/Q07. This package builds the immutable formal background-mode input reader needed before any World projection.

## Authority and first difference

Formal playable `game_session.cpp` loads `data/bg_mode.dat` groups, each referenced `data/bg/*.dat` child and its ordered `<mode>` records, then `apply_native_background_mode_record` projects the selected record before battle. The locked parent contains **32 group descriptors** that reference **25 unique child DATs**; each child has three physical records, so there are 75 physical records and 96 group-indexed record positions. The children share rule-token signatures per record index today, but production must resolve the selected group and record generically. Unity's current `LoganObjectCatalog` has fusion/mode-combo/kind inputs but no background-mode reader; `MatchConfig` has backgroundId and no background-mode index, so staged files do not yet affect battle behavior.

## Exact owned paths and boundaries

- Add `Assets/NTSD/Scripts/Animation/LoganBackgroundModeInput.cs` and generated `.meta`: parse parent descriptors and all child records using the existing DAT tokenizer and `BattleContentSource.ResolveDatPath`; retain formal integer-zero defaults, ordered repeated `id:`, ID/record lookup and parent/child raw freshness hashes. Return null when the parent is absent for legacy/minimal test roots; fail closed when a present parent has invalid or missing children.
- Add focused `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07BackgroundModeInputEditorTests.cs` and generated `.meta`: formal/staged corpus 32 descriptors/25 unique children/75 physical records, three distinct mode rules, exact selected ID/index, malformed/missing child and changed-input checks.
- No other production script, identity/schema, World, Input, Menu, Scene, DAT, PNG or old resource edit. This step is parser-only; it cannot be reported as an active battle-rule fix.

## Verification and rollback

Refresh the existing original Unity Editor, run only the focused new test class, inspect compile/errors and generated GUID uniqueness, verify Scene hashes unchanged, run `Tools/Validate-ChangeLedger.ps1` and diff check. Later Q07 Task/Change must atomically include the captured background-mode input in content identity/publication and select/apply the full formal battle-rule record before tick 1 with snapshot/checksum coverage and approved exceptions. Rollback requires the repository's explicit deletion approval for newly added files; do not auto-delete.
