<!-- CHANGE-RECORD
id: NTSD28-Q07-RASENGAN-ACTION35-TARGET-SOURCE-001
status: VERIFIED
change-kind: Q07_RASENGAN_ACTION35_TARGET_FORMAL_SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/rasengan_action35_target_source_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28/native collision path
evidence: docs/ai/TASKS/NTSD28-Q07-RASENGAN-ACTION35-TARGET-SOURCE-001.md; artifacts/diagnostics/NTSD28-Q07-RASENGAN-ACTION35-TARGET-SOURCE-001/REPORT.md
-->

# NTSD28-Q07-RASENGAN-ACTION35-TARGET-SOURCE-001

Before edit: existing natural formal LFR with a distant target reaches OID434/action35 at ticks35–37, but no target hit; Lee J→L OID204/action20–23 was rejected as an outgoing-hit witness because those actions have no ITR. Formal Naruto frame254 OPoint kind2 creates OID434/action35, whose formal frame authors a kind5 ITR. No existing target-present formal GameSession scan for this ordinary input route has been observed.

Declared edit: add only the named C++ diagnostic file, preserving all previous probes. Scan bounded stationary opponent coordinates using unchanged normal input and content; collect child candidate-to-target and actual target state per tick. Emit one LFR only for a first attributable target case. Do not change source-authority files, Unity production, DAT values, images, Scene, ProjectSettings or nonbattle modules. Side effects are only new diagnostic artifacts and bounded CPU time. Risks: close target may alter the pre-spawn Naruto route, action35 may be held/converted before collision, and a target HP change may come from a different attacker. Require action35 candidate attribution and record any early disturbance separately.

Acceptance and rollback are in the Task. After implementation record actual file/symbol, compile command/result, source scan, release replay if applicable, unresolved evidence, and governance validation. No previous failure will be overwritten.

After edit: only `rasengan_action35_target_source_probe.cpp` was added. Its `run_case` uses unchanged `GameSession28::set_input/step`, records OID434 action, child-to-slot1 candidate count, read-only `classify_ordinary_hit_eligibility` status/message and target HP/action, with non-overwriting CSV/LFR output. First compile missed `-municode -lz` and failed at link; second compile succeeded with exactly the paired 28 core source list plus four playable sources. The first run passed the wrong constructor root and failed at catalog loading; correct roots produced two initial 11-case scans, then an eligibility-strengthened final build and two independent final scans. All failures/output remain preserved. Final source and EXE hashes, scan identity and source branch are in REPORT.

Verification: 11 cases ×55 ticks =605; action35 present in all; five positions yielded 10 candidate rows, all classifier `rejected` with the same kind5 linked-holder/wpoint-strength reason, target HP500 in every row. Source `BattleWorld28::resolve_standard_damage_interaction` requires holder first wpoint `attacking` 1..9 for kind5→kind0; formal Naruto frame254 has `attacking:0`. No candidate-plus-effect case occurred, so no LFR was emitted and root formal EXE was only rehashed, not target-replayed. No Unity runtime, DAT, image, Scene, ProjectSettings, production or nonbattle file was changed. `VERIFIED` is only this bounded negative source qualification, not Q07/R09/R12/R18 closure.

Governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 with 850 Records and all six governed current code files covered, including this new diagnostic; its historical non-diff Record warnings are not failures. `git diff --check` exited 0. The three live progress documents have zero NUL bytes after the update. No Unity compile or Play was run because no Unity script or runtime was changed.

Final diagnostic wording correction: `saw_target_effect` described only HP, so the same code now calls it `saw_target_hp_change` and reports no candidate-plus-HP-change. The rebuilt v3 EXE compiled 0 errors; final `scan6`/`scan7` each ran all 11 cases and produced byte-identical rows and summary. The 10 ordinary-hit classification rejections and HP500 observations are unchanged. REPORT lists final source/EXE/CSV SHA values; no older artifact was overwritten. This does not broaden the claim to PP, hitstop, relation or every other target field.
