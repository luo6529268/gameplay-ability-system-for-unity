# R06 formal story queued reachability — scoped acceptance

Authority: root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; paired playable `GameSession28` and formal `resources/runtime/decoded_dat/data/stage.dat` → mission1 child5 phase0 `s/1/stage1.dat` row OID8/HP350/Join500. This run uses the paired source's configurable story scenario; it does not prove the unchanged root EXE's ordinary menu can select it.

The new `formal_story_queued_reachability_probe.cpp` compiled against the paired playable/core source list with an empty `compile.log`. The first invocation passed the wrong extracted root and failed before simulation (`run.log`: `initialize: unable to open catalog.csv`, exit 4); no trace was written. Correcting both roots to the formal `resources/runtime` produced `story_queued_300ticks.csv`, `run-corrected.log`, exit 0 and `rows=301 join_birth=1 natural_lethal=0 queued_continuation=0`.

Initial tick 0 already contains naturally spawned OID8 Chiyo in slot23, HP350, action360, lives0, queued next HP500, group5 and owner23. The 301 CSV rows cover initial state and complete ticks 1–300. Slot23, HP350, next HP500 and phase0 remain constant; no revival event is emitted. The neutral scenario therefore proves source-side stage birth and queued field setup, but does not reach lethal damage or queued continuation. It does not validate Unity's queued consumer, original-EXE story-menu reachability, physical input or picture output. Unity default stage.dat deployment remains on user hold; Q07/R06/BATCH-04 remain open.

Evidence: `compile-args.txt`, `compile.log`, `run.log`, `run-corrected.log`, `story_queued_300ticks.csv`. No DAT, scene, Unity production code or formal authority file was changed.

Repository checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <workspace>` exited 0 (`change-ledger-validation.log`; pre-existing unmatched-record warnings remain). `git -c core.safecrlf=false diff --check` exited 0. The formal EXE SHA and the four protected Battle/Menu/GameConfig/ProjectBattleModeConfig asset SHA values match their recorded baselines.
