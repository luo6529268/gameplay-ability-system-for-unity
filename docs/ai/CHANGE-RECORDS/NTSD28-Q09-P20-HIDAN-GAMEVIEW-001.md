<!-- CHANGE-RECORD
id: NTSD28-Q09-P20-HIDAN-GAMEVIEW-001
status: VERIFIED
change-kind: Q09_P20_HIDAN_NATURAL_FULL_GAMEVIEW_WITNESS
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HidanPhysicalBattlePlayProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033, matching playable frame430 path, original Battle Scene physical-input evidence
evidence: one original Battle natural physical-input Play captured 1920x1080 composed Game View, tick20 checksum stable, ordered shutdown zero residue, protected hashes unchanged; scoped acceptance in artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-GAMEVIEW-001/ACCEPTANCE.md
-->

# NTSD28-Q09-P20-HIDAN-GAMEVIEW-001

Pre-script status `PLANNED`. [Task](../TASKS/NTSD28-Q09-P20-HIDAN-GAMEVIEW-001.md).

Current state: original Battle physical-input probe already reaches Hidan action430/pic119 with valid Logan hid6 binding; opt-in isolated World Camera capture proved the actor body visible, but did not capture the composed Game View. This file is already dirty with prior packages and is protected from broad cleanup.

Declared edit: add a separate opt-in composed Game View screenshot request to that existing Editor-only probe. Wait for the asynchronous PNG while the Driver remains paused on the same completed frame430; record image dimensions and before/after World tick/checksum; retain existing input and ordered-close behavior for all other branches. New results use a unique no-overwrite path under `artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-GAMEVIEW-001/`. Risk: Game View capture may occur on a different rendered frame or omit background/HUD; retain the raw PNG and report actual contents without upgrading to formal pixel parity. No production, DAT, image, Scene, asset, ProjectSettings, formal source, or nonbattle edit.

Validation pending: generated Editor compile; original Editor refresh, one natural physical Play screenshot and independent image inspection; ordered shutdown, unchanged protected hashes, Ledger validator, diff check. Rollback only this package's opt-in hunks after reviewing the dirty file, subject to deletion rules.

Code written: new `captureComposedGameView` request/report flag and unique result root; first natural frame430 pauses the complete Driver, requests Unity `ScreenCapture`, then waits for a valid PNG while checking World tick/checksum before `FinishFrame430` and existing ordered exit. The 30-tick frame430 command path, isolated World Camera path and default Q07 path retain their previous branches. `RestoreSession` fails closed if a domain reload interrupts screenshot wait. Generated `Assembly-CSharp-Editor.csproj` build exited0 with 0 errors and 213 warnings. Original Editor reload/Play and screenshot interpretation pending.

Final scoped validation: original Editor PID11944 refreshed in place, returned to Edit/idle with clean Battle Scene, and consumed one opt-in physical request. The [raw report](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-GAMEVIEW-001/hidan-gameview-20260929-01.json) recorded relative tick15/absolute20 Hidan action430/pic119, valid hid6 body command, actual 1920x1080 Game View PNG, World tick/checksum unchanged during asynchronous screenshot, and ordered shutdown Completed/Stopped with World detached and objects/slots/borrowers0. Visual inspection showed project background, HUD, actor and target; this is only original Unity composed-view evidence, not formal root GUI parity. Battle/Menu/ProjectBattleModeConfig/BuildSettings hashes match the protected baselines. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-GAMEVIEW-001/ACCEPTANCE.md). P-20/Q09/BATCH-05 and Q07 remain open. Only Editor diagnostic code and records changed; no production, DAT, image resource, Scene or nonbattle edits.

Final governance: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity` exited0/PASSED with 1008 Records and 35 governed diff code files covered. The first `powershell -File` call without an explicit `-RepositoryRoot` exited1 because this shell passed empty `$PSScriptRoot` into the script's default parameter; the explicit-root call is the valid result. `git -c core.safecrlf=false diff --check` exited0. The original Editor is back in Edit/idle with no compile/reload/test activity; no broader suite was run for this Editor-only opt-in screenshot branch.
