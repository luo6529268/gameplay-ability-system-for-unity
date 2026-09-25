<!-- CHANGE-RECORD
id: NTSD28-Q07-RASENGAN-AFTER254-PROBE-GUARD-001
status: VERIFIED
change-kind: Q07_RASENGAN_NEGATIVE_WINDOW_DIAGNOSTIC_GUARD
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable 2tu input/frame path
evidence: docs/ai/TASKS/NTSD28-Q07-RASENGAN-AFTER254-PROBE-GUARD-001.md
-->

# NTSD28-Q07-RASENGAN-AFTER254-PROBE-GUARD-001

Before: `NaturalProbe.ObserveNatural` applies `IsUninterruptedPostWindowFrame` from the first observed tick whenever `timing == 2`. The original Editor at tick1170/action110/state7 rejected a legitimate defense startup before skillTick or afterWindowTick existed. The saved FAIL is a diagnostic false negative, not a gameplay first difference. The second253 case uses `afterWindowTick >= 0` already and passed with the current source.

After intended: require `afterWindowTick >= 0` before checking the authored standing/normal tail for both negative timing modes, without changing the guard after the window, event scheduling, resource data or production logic. Only the exact Editor test file named in the metadata may change. Validation and rollback are specified in the Task. This record is created before the script edit; actual diff, compile, Play and protection results are pending.

Actual: one condition in `NaturalProbe.ObserveNatural` changed from the asymmetric `timing == 2 || (timing == 1 && afterWindowTick >= 0)` to a shared negative-mode condition with `afterWindowTick >= 0`. No production script, DAT, image, Scene or GameConfig was edited by this Change. The original Editor recompiled the updated Editor assembly with zero Console errors. The clean Battle Scene after bootstrap produced an after254 physical-device PASS; the already-obtained second253 physical-device PASS establishes the other negative mode. The first253 positive witness was unaffected by this condition. Both negatives retained PP350/consumed150 and did not enter action301, consistent with the stated formal source/release milestones. Play ended in idle Edit Mode with clean Battle Scene and stable Battle/Menu/GameConfig disk SHA. See `artifacts/diagnostics/NTSD28-Q07-RASENGAN-AFTER254-PROBE-GUARD-001/ACCEPTANCE-20260925.md` for files, hashes, ticks, false starts and limits. This is scoped diagnostic verification; parent Q07/R18 and formal same-world/pixel validation remain open. Governance validator and `git diff --check` are recorded below after execution.

Final governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0; full output is `Temp/NTSD28-Q07-Rasengan-Guard-ledger-validation-20260925.log` (historical unrelated record warnings remain). `git diff --check` exited 0 (line-ending notices only). The three restored progress documents had zero NUL bytes at this check. No wider Play matrix or release pixel capture was run for this one-line diagnostic correction.
