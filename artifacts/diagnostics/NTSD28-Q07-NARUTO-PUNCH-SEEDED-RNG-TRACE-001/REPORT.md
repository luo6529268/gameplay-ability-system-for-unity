# Q07 Naruto punch paired-source seeded RNG trace

Status: `VERIFIED / SCOPED_SEEDED_SOURCE_UNITY_RNG_STATE_PARITY`. Parent Q07/R09/R10/R18 and the full battle-alignment goal remain open.

The formal root EXE SHA-256 remains `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. This package uses its paired playable `GameSession28` and `NativeRandom28` source, the same formal runtime root and the unchanged Naruto DAT. It adds seven **diagnostic observation** columns to the existing 36-case ordinary-punch probe: CRT state and calls; synchronized counter, index, calls, last site and table hash. No battle input, frame, DAT or production rule changed.

The paired closure's 28 core files plus `game_session.cpp`, `selection_flow.cpp`, `game_session_lfr.cpp` and `lfr_recorder.cpp` compiled with g++ 15.1.0, C++17 `-O2 -Wall -Wextra -Wpedantic -municode -lz`: second build exit0 and empty warning/error log. The first standalone link failed because its invocation omitted the existing `lfr_recorder.cpp` dependency; `build.log` retains that failure. First execution stopped before tick 1 because the complete VFS constructor argument pointed at `<runtime>/vfs` rather than the root containing `decoded_dat` and `vfs`; `source-run1.log` and its partial output remain. The corrected second execution returned exit0 into a new `source-run2` directory.

The new scan has 36 cases × 30 completed ticks = 1,080 rows. All 36 still recorded input, authored punch and attributed KO. Every one of the previous scan's 15 columns matches the preserved run1 row-for-row; the 36-case summary and 10,002-byte witness LFR are **byte-identical** to run1. That LFR was already replayed successfully by the unchanged root EXE, so this package does not substitute a new candidate EXE or infer a different release outcome.

For the first exact case (two Naruto OID2 at X500/X525, Z650, HP500/10, seed682973786, explicit BGM2, one J on completed tick2), the paired source's seven after-tick RNG fields match the original project's preserved Unity `original-editor-run2.input-rng.jsonl` on all 30 completed ticks: **210/210 field comparisons, no first difference**. Both streams start at CRT state `1758127634` after 3000 calls; at tick8 both reach state `2524509468` after 3002 calls and retain it through tick30. Synchronized RNG starts at counter/index/calls 0/0/0; one call at site130 on tick2 raises them to 1/1/1, with the same table hash `A1BA1B90EA55796D` throughout. This compares the paired source and Unity under the **same declared seed**; the formal root EXE's LFR playback still starts its nonserialized CRT stream from default seed0. Do not call that playback CRT state equal or claim CRT per-call consumer-site equality from after-tick states alone.

Artifact SHA-256:

| File | SHA-256 |
|---|---|
| `Tools/NTSD28Q07Diagnostics/naruto_punch_formal_hit_probe.cpp` | `C219E5C2FE07348DCB579DE42656701580DD393152E93DD4CE92D78D4E7416C6` |
| `naruto_punch_seeded_rng_probe.exe` | `696E15F4DCBD7B59F078186F393B1708DCC26CBCC78B26C8E873E74DA1B9B42F` |
| `source-run2/punch-hit-scan.csv` | `6218CBA1AB164D7256B174B8F1F15F8E81621A1FD1309237D76AF5025D637E62` |
| `source-run2/punch-hit-summary.csv` | `3F3AF7DD4C83DFD367B65E34534E70B63BA06CA940A46870FAEB84245E3267A2` |
| `source-run2/first-attributed-punch-hit.lfr` | `B84CBAA10C0C115AA66B1426E0BF6EB3ACBF0052D02B7478F5BCE0D4740632FA` |

The source-vs-Unity comparison is a bounded 30-tick EditMode fixture and the root EXE comparison is bounded to the already verified LFR-selected outputs. This does not certify all Naruto skills, other seeds/positions, physical keyboard wall-clock play, GPU pixels, audio, complete EXE RNG state or Q07 aggregate exit. The existing Q08 Menu Play separately proves a real Unity physical-J authored punch, native KO attribution and result return for a different controlled initial state; it is not repeated here.

Final governance: `Tools/Validate-ChangeLedger.ps1` exited 0 (`Change ledger validation PASSED`, 853 Records, seven current governed code files covered); `git diff --check` exited 0. The unchanged saved Menu/Battle Scene SHA-256 values are `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. The three live progress documents remain NUL-free. This standalone C++ diagnostic did not require Unity import, SelfCheck or Play and did not start another Editor.
