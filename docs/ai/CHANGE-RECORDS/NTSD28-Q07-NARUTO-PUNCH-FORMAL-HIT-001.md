<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-PUNCH-FORMAL-HIT-001
status: VERIFIED
change-kind: Q07_NARUTO_NATURAL_PUNCH_FORMAL_HIT_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/naruto_punch_formal_hit_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable ordinary hit chain
evidence: docs/ai/TASKS/NTSD28-Q07-NARUTO-PUNCH-FORMAL-HIT-001.md; artifacts/diagnostics/NTSD28-Q07-NARUTO-PUNCH-FORMAL-HIT-001/REPORT.md
-->

# NTSD28-Q07-NARUTO-PUNCH-FORMAL-HIT-001

Pre-change: Q08 original-Editor Menu Play has physical J→formal Naruto frame513→attributed KO, but its saved JSON lacks complete initial world/seed/source position and cannot be compared to a newly invented source scenario. The prior Q07 natural Rasengan OID434/action35 target scan proved geometric candidates that native kind5 consumer rejects, so it is not a hit witness. Formal Naruto frame513 has authored kind0 injury20 ITR and is an appropriate ordinary consumer candidate.

Declared code change: add only the named C++ diagnostic, compile the paired playable 28-core source list plus GameSession/LFR/selection files, scan bounded input/target positions without changing authored frames or target mid-tick, capture actual hit/KO and a first valid LFR. Do not edit authority source, Unity scripts, DAT, image, Scene, ProjectSettings or nonbattle code. Side effects are bounded CPU time and new non-overwriting diagnostic artifacts. Risks: sampled 2tu phase, normal attack route choice, collision geometry, target HP10 result timing, and misattributed damage; require exact attacker/target and frame evidence before source success. Task holds acceptance and rollback. After edit record actual file/symbol, builds/runs/failures, root replay if qualified and governance checks.

After edit: only `Tools/NTSD28Q07Diagnostics/naruto_punch_formal_hit_probe.cpp` was added. `run_case` uses unmodified paired `GameSession28::set_input/step` and `GameSessionLfr28`, writes per-tick action/HP/phase/KO attribution and emits the first qualified LFR. The paired closure compiled with 0 errors. The 36 bounded cases each reached an authored punch frame and attributable KO; first X525/tick2x1 witness reached frame513 and HP10→-10 at tick8, event source/victim/credit/four-owner slots 0/1/0/0. Unchanged root EXE SHA was verified and its headless LFR report passed; tick1–30 actor action, target HP/action and input phase matched source scan 30/30. Formal and staged Naruto DAT hashes matched. See REPORT for precise initial config, hashes, trace and command boundary.

Verification scope: `VERIFIED` applies to the new formal source/release diagnostic only. No Unity same-initial-state trace, original-Editor Play, EXE GUI/audio/visual check, full-state comparison or Q07 aggregate exit occurred. The pre-existing Q08 physical-J Unity witness remains separate due to incomplete initial-state capture. No production, DAT, image, Scene, ProjectSettings or nonbattle files changed. Governance validator and diff checks are recorded after the documentation update.

Governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 with 851 Records and seven governed current code files covered (the shared Q09 dirty work was preserved); historical non-diff Record warnings did not fail validation. `git diff --check` exited 0. The three live progress documents were rechecked with zero NUL bytes. The project Unity Editor, Unity compilation, SelfCheck and Play were not run because this package changed only a standalone source probe and documents.
