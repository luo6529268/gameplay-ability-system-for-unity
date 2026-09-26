<!-- CHANGE-RECORD
id: NTSD28-Q07-N30-LATE-INPUT-RELEASE-WITNESS-001
status: VERIFIED
change-kind: Q07_N30_LATE_INPUT_RELEASE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/n30_late_input_release_probe.cpp
authority: formal root NTSD2.8-Logan.exe and paired playable GameSession28::step/input_routing.cpp
evidence: Q07 exit reconciliation names reachable Unity N30 OID998 birth with missing formal live-path witness
-->

# NTSD28-Q07-N30-LATE-INPUT-RELEASE-WITNESS-001

Pre-change: `LF2Entity.RunLateCharacterDatInputTrigger` has reachable production late-tail input patterns and creates OID998 action100/102/104; `ConfigureLateN30SpawnTask` does not carry source-rule coordinates. Formal paired source currently has no identified equivalent OID998 late-input writer. Key-history numeric codes differ between the formal source and Unity, so neither a static source search nor a mismatched physical-key schedule can decide parity.

Declared script and symbols: new `Tools/NTSD28Q07Diagnostics/n30_late_input_release_probe.cpp`, `wmain`, bounded `GameSession28::step` input schedules, `GameSessionLfr28` recording, per-tick history/actor/entity audit. No existing source or resource may be overwritten. Side effects are new diagnostic binary/JSONL/LFR/replay artifacts only. No production runtime, DAT values, Scene, config, sprite, audio or nonbattle change.

Acceptance, rollback and risk: see Task Contract. Source-generated LFR replay is a scoped formal root EXE witness, not a complete native parity claim; unchanged input history or a missing entity in one fixture cannot prove global absence. Keep Unity production unchanged until the same-state complete-Driver result and authority conclusion. Rollback requires separate approval for deleting new files; do not clean the dirty worktree.

Actual new script: only `Tools/NTSD28Q07Diagnostics/n30_late_input_release_probe.cpp`. It records four 20-tick source schedules and LFRs, actor action, input phase, five-entry history, active count and OID998 slot/action. New binary and all outputs are under the declared artifact directory. First C++17 link omitted `-municode` and failed with `undefined reference to WinMain`; corrected build with 28 core/four playable sources, `-municode`, `-lz` exited0. Four source schedules executed. The Defend–Jump–Defend–Jump schedule reached formal history `[-1,9,0,9,0]` at tick8 and retained it to tick20 with no OID998 birth and active count2. The other three schedules did not retain four input edges, so their no-birth observations do not independently establish the N30 question.

Formal root EXE SHA-256 was rechecked. Only the exact-history source LFR was replayed; root report `passed:true`, failure0, 20 trace rows, no initial overrides, `nativeParityClaim:false`. Independent source/root comparison found 0 differences in 20 tick rows for tick/action/five history values/active count/OID998 count. Exact evidence and limitations are in `artifacts/diagnostics/NTSD28-Q07-N30-LATE-INPUT-RELEASE-WITNESS-001/ACCEPTANCE.md`. This closes the scoped formal witness only. Unity same-state complete-Driver and natural N30 reachability, if any, remain pending; Q07/D-024 stay open. No production, DAT, Scene, config, image/audio or nonbattle edit.

Post-change checks: `git diff --check` exit0; Change Ledger validator PASS with explicit repository root under PowerShell 7, 881 records/16 governed code files in the current dirty diff. The initial Windows PowerShell invocation failed on its empty default `$PSScriptRoot` before validation and was superseded by the successful explicit-root command. Original Unity Editor is running, but this Unity 2022.3 project has no `com.unity.pipeline` connection for `unity status`; existing raw-capture request schemas reject arbitrary N30 schedules. No Unity request or second Editor was launched, so Unity runtime validation remains pending.
