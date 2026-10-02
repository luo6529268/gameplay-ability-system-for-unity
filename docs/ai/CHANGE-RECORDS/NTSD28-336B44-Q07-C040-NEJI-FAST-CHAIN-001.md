<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-NEJI-FAST-CHAIN-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/neji_bee_c040_natural_probe.cpp
authority: 336B44 playable C040 natural reachability and formal OID18/OID75 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-NEJI-FAST-CHAIN-001.md
-->

# Q07/C040 宁次快速抓取连段受控自然筛选

Created before the new diagnostic code. Current formal `settle_catch_relations` retains a caught kind-2 action only if the hurtable/hold gate permits it, then uses the victim's current frame center with the catcher's vaction-frame CPOINT. The earlier Hinata/Bee sweep has 480 near-gate rows with entry hold -1, which reaches zero before settlement. Formal Neji DAT action286 has a kind-3 catch route, and later catch frames have injury followed by a shorter wait and changed vaction. This is a testable candidate, not a claimed reachability result.

First build passed, and initial four runs produced an apparent post-tick split in three near positions. Phase audit then found the probe's `c040_candidate` compared target state after `GameSession28::step` with the catcher's **post-frame** action; `SimulationTickDriver28::step` calls `settle_catch_relations` before `step_frame_slot`, so this mixed phases. At tick11 the catcher enters settlement in action126/vaction132, not end-of-tick action127/vaction131; victim action132 is therefore not a positive split. The initial raw files and one positive/negative repeat are preserved as `phase-mixed-v1` evidence. Before changing the test script, the corrected predicate is declared: compare the pre-frame catcher cpoint/vaction and pose with the post-settlement victim/hold/relationship only when catch relation input/throw/release did not transition the catcher; name the final frame fields `end_tick_*` for diagnostic context, never settlement fields. Rebuild and rerun to obtain the actual bounded result. No production or DAT adjustment follows from the discarded initial marker.

Only the new source diagnostic path named in metadata may be written. Pre-change Unity battle logic and all DAT/Scene/resource bytes remain unchanged. The diagnostic must use complete playable `GameSession28::step`, current formal content, a bounded four-position neutral-input matrix, unique raw outputs and a post-settlement positive predicate. A scoped negative must be recorded honestly; a positive requires separate root EXE and Unity Scene work. Rollback is a forward correction of the new diagnostic, preserving all existing files and failed runs. Validation: source build, bounded runs/determinism, Change Ledger validator and diff check.

Actual code: added only `Tools/NTSD28Q07Diagnostics/neji_bee_c040_natural_probe.cpp`, adapted from the existing bounded source probe for OID18/action286 and OID75. The first 0-diagnostic compile's candidate predicate mixed phases; after reviewing `SimulationTickDriver28::step` order, revised the probe to use pre-frame catcher vaction, guard relation input/throw/release transitions, and rename post-frame fields `end_tick_*`. The corrected compile again returned exit0/zero diagnostics. Initial v1 artifacts and independent repeats were preserved, not overwritten.

Focused result: four current-formal-content, 120-tick full GameSession runs at target X550/580/670/1200 returned zero corrected C040 positives. Near three positions did naturally grab and apply hold injury; 15 near-gate entries per position reached the changed vaction only with entry target hold -1, which physics advances to zero before settlement. X1200 had no grab. Two v2 repeats (X550 grab / X1200 no grab) matched CSV, RNG and LFR SHA; v1/v2 LFR/RNG also stayed identical. This is a bounded natural screen, not an all-content impossibility or parent C040 closure. No production, DAT, resource, Scene or non-battle file was touched by this package. Validator/diff check results are appended to the report.

Final checks: `git diff --check` exit0. Whole-worktree Change Ledger validation exit1 solely for a concurrently modified, unrecorded `Assets/NTSD/Scripts/UI/UIButton.cs` outside this Task; do not edit or fabricate a record for that file. A scoped `-StagedOnly -SimulateChangedPath` check exited0 and covered this exact new C++ path by this Change ID. This limits the audit claim to this package; the global worktree condition is documented in the report.
