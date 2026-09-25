<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-HELD-AIR-ACTION-001
status: VERIFIED
change-kind: EDITOR_BATTLE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoHeldAirActionProbeEditor.cs
authority: formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable state4 linked attack, formal OID2 and OID120 content
evidence: original Editor compile and scoped Battle Scene Play final request PASS action30/pic97 tick20, cleanup and Scene SHA stable
-->

# NTSD28-Q07-NARUTO-HELD-AIR-ACTION-001

Before edit: Q07 has a controlled OID30/frame31 pixel witness, but no actual held airborne action/pic from a selectable character under formal content. Existing pickup/throw Play and synthetic action-selector tests do not capture the combined sequence. Formal OID2 Naruto frame30/pic97 is `jump_weapon_atck`, while frame31 is `sexy`; OID120 weapon stats have no `jump_attack` override.

Declared edit: add only a fixed-target, opt-in Editor diagnostic script/meta named in Task. It will use original Editor Battle Scene formal runtime, a temporary OID2/OID120 fixture, production kind2 pickup, roster-bound discrete Jump/Attack inputs through the complete Driver, and per-tick result JSON. No production behavior, DAT, PNG, Scene, Prefab, camera, game setting or nonbattle code changes. Expected transient effects are two temporary registered battle entities and a temporary roster binding; cleanup must restore all and report counts. This is **not** a physical device or same-world formal EXE certificate. Rollback only exact new diagnostic files after review; preserve all pre-existing dirty work.

Actual edit: added only the declared Editor probe and meta. It creates a fixed OID2/OID120 fixture, uses authored frame60 kind2 through production pickup, binds a temporary human roster slot, steps the complete production Driver with discrete packets, records actual action/state/pic/Y/link per tick, then unregisters both objects and restores the roster. The first run exposed an overly strict expected link1; actual formal-content production relation was 101/-1 with correct references/slots. A second diagnostic added those fields; a third found `FrameInputSet.Jump` enters held action20 because of the existing legacy carrier mapping. The final probe uses the existing native-bank carrier mapping, without changing production input code.

Validation: original Editor Tundra build success after script edits and no current C# Console compile error. Final saved-Battle-Scene request `naruto-held-air-20260925-04` PASS: pickup link101/-1; jump action210 tick12; airborne action212 Y=-16 tick18; attack action30/state15/pic97 tick20; holder link101 retained. The three earlier FAIL JSON reports remain. Exit restored World4→4, slots2→2, pool2→2 and roster; Editor returned idle/non-Play. Battle/Menu disk SHA stayed `2EE465D8...B77A` / `785F828C...E13`. MCP emitted two disposed-client errors during exit then reconnected; these are tooling errors, not current compile errors. Report and full hashes: `artifacts/diagnostics/NTSD28-Q07-NARUTO-HELD-AIR-ACTION-001/ACCEPTANCE-20260925.md`. Focused NUnit, full SelfCheck, physical keyboard, natural collision pickup, formal root-EXE same-world trace and GPU pixels were not run/proven. Rollback only this probe/meta after protected-worktree review. Q07/R17/total goal remain open.

Governance exit: `Tools/Validate-ChangeLedger.ps1` exit0 (837 Records/32 governed code files), `git diff --check` exit0, three progress docs NUL-free and new meta GUID unique. This does not widen the scoped runtime claim.
