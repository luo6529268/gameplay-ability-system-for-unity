<!-- CHANGE-RECORD
id: NTSD28-Q07-HIDAN-CATCH-FULL-DRIVER-001
status: VERIFIED
change-kind: Q07_HIDAN_CONTROLLED_CATCH_FULL_DRIVER_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/hidan_catch_full_driver_lfr_probe.cpp
authority: formal root NTSD2.8-Logan.exe paired playable GameSession/SimulationTickDriver/BattleWorld and indexed Hidan OID24 hid.dat frame236 kind3 to frame120/239
evidence: existing source-world resource witness and original Editor 35/35 do not establish whole-session root EXE catch
-->

# NTSD28-Q07-HIDAN-CATCH-FULL-DRIVER-001

Pre-change: real Hidan `hid.dat` frame236 kind3 ITR names catching action120/caught action130, frame120 next239 and frame239 held injury30. Source World direct invocation gives resource result, but no complete `GameSession28::step` or root formal EXE trace has yet demonstrated that relation. The candidate initializes OID24 at action236 in a controlled source-authored session, with overlap/miss positions; this is not normal input reachability.

Declared code path: new `Tools/NTSD28Q07Diagnostics/hidan_catch_full_driver_lfr_probe.cpp` only. It may write LFR/CSV to new diagnostics folders and uses read-only formal source/DAT. No Unity production/test, scene, config, image or nonbattle script edit. Acceptance, rollback and evidence limitation are in Task Contract. Status `IN_PROGRESS`; build/source/root EXE outcomes pending.

Actual script: only the declared new probe was written. It initializes two indexed Hidan OID24/type0 participants with actor action236, seed `0x28A55A5A`, X500 and X520 or X1200, mode0/BGM2, records 24 complete GameSession ticks, reciprocal relation/action/current PP/HP, and source-authored LFR. No source DAT or production code edit. Status `CODE_WRITTEN`; build/run pending.

Final scoped status: `VERIFIED`, controlled diagnostic only. Paired playable source compiled to `Temp/NTSD28Q07HidanCatch/build/hidan_catch_full_driver_lfr_probe.exe`; compiler exit 0. The shell wrapper returned 1 solely because `Get-Content compile.log` was attempted after a silent successful compile had not created that optional log. Initial source invocation with the decoded-DAT subfolder as catalog root failed `unable to open catalog.csv`; corrected to `resources/runtime`. X520 source ran 24 complete `GameSession28::step` ticks: reciprocal catch at tick 1, actor action 239 at tick 2, both PP 300→322 and target HP 500→470 at tick 3. X1200 ran 24 ticks with no relation or held injury. Source outputs are under the two `source-*` folders in the evidence directory.

Final repository gates after the Hidan diagnostic chain: `Tools/Validate-ChangeLedger.ps1` PASS, 877 records/11 governed code paths; `git diff --check` exit0. The original Menu/Battle Scene and GameConfig/ProjectBattleModeConfig SHA-256 stayed `785F828C...81E13`, `2EE465D8...8B77A`, `0527D737...C8EA7`, `88E10D43...F55C` respectively. These gates do not certify Battle Play.

First formal root EXE replay lacked the required `--lfr-slot0-action 236` override and failed with code 46; its report/trace are preserved. Corrected fresh formal root replays for X520 and X1200 both report `passed:true`, `failureCode:0`, 24 declared ticks and 25 trace snapshots including tick 0. Independent source CSV versus root trace comparison matched 24 ticks × seven fields = 168/168 each for actor/target action, actor/target PP, target HP and reciprocal catch slots. Root reports explicitly set `nativeParityClaim:false`, so selected field comparison, not LFR PASS alone, is the scoped parity evidence. No natural player-input reachability, original Unity full-driver/Play, pixels, or complete state parity is claimed. See `artifacts/diagnostics/NTSD28-Q07-HIDAN-CATCH-FULL-DRIVER-001/ACCEPTANCE.md`.
