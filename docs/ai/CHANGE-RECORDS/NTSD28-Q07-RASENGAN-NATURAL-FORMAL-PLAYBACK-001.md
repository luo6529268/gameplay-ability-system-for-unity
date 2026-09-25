<!-- CHANGE-RECORD
id: NTSD28-Q07-RASENGAN-NATURAL-FORMAL-PLAYBACK-001
status: VERIFIED
change-kind: Q07_NARUTO_NATURAL_COMBO_FORMAL_RELEASE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/rasengan_natural_lfr_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and declared playable input/frame closure
evidence: artifacts/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-FORMAL-PLAYBACK-001/REPORT.md; GCC compile and three formal EXE replay reports passed
-->

# NTSD28-Q07-RASENGAN-NATURAL-FORMAL-PLAYBACK-001

Before edit: original Editor synthetic physical-device probe established Naruto L/D/K/J on a live standing actor with exact 2tu phases; the formal phase-1 second-253 miss is supported by playable source, but has not yet been exercised in the root formal EXE under the same discrete input schedule. The existing `rasengan_miss_lfr_probe.cpp` starts directly at frame241 and does not cover natural combo entry. This new diagnostic file will use only the formal playable/core compilation closure and record first/second/post254 cases; no production or formal authority file is modified. Expected side effects are new C++ diagnostic binary/LFR/trace/report files under a unique artifacts directory. Acceptance, risk and rollback are in the same-ID Task. No Unity source, DAT value, Scene or nonbattle change is authorized here.

After edit: added only `Tools/NTSD28Q07Diagnostics/rasengan_natural_lfr_probe.cpp`, using current playable `GameSession28::set_input/step` and `GameSessionLfr28` to make three non-overwriting 55tick recordings from a stated standing/2tu initial state. GCC 15.1.0 compiled the new script with the 28 declared core files and four playable source files (exit0, empty compile log). Root formal EXE unchanged SHA, three headless replays exit0/report PASS/failureCode0; source/release six relevant fields across 165 ticks are 990/990 equal. Two raw `pending` differences are due to LFR sampled-input projection and are excluded from parity count; second253 and post254 LFR bytes are equal. The existing Unity natural probe's `Runtime.MP` was discovered to be the wrong resource field for comparison with native `current_mp`; `Health.PP` remains unmeasured there. Report, exact limits and output SHA in `artifacts/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-FORMAL-PLAYBACK-001/REPORT.md`. No authority/DAT/Unity production/Scene/nonbattle write. Record `VERIFIED` is this controlled diagnostic only, not Q07/R18. Rollback concerns only the new diagnostic script and this package's append-only records under repo approval rules.
