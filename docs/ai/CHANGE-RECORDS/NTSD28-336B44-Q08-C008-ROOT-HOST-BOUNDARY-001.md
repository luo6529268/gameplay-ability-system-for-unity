<!-- CHANGE-RECORD
id: NTSD28-336B44-Q08-C008-ROOT-HOST-BOUNDARY-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/result_continue_host_lfr_probe.cpp
authority: selected 336B44 formal root and playable BattleFlow28/GameSession28; C008 G1 root evidence gate
evidence: docs/ai/TASKS/NTSD28-336B44-Q08-C008-ROOT-HOST-BOUNDARY-001.md
-->

# NTSD28-336B44-Q08-C008-ROOT-HOST-BOUNDARY-001

Created before diagnostic script edits. This carrier runs unchanged formal DAT through selected-source GameSession, generates a natural Naruto KO, records held and natural result boundaries and encodes LFR for frozen root replay. It cannot change battle rules or formal inputs. Expected side effect is new diagnostic files only. Root terminal replay can reject the correctly frozen World after emitting its trace; this must remain an explicit carrier limitation. [Task](../TASKS/NTSD28-336B44-Q08-C008-ROOT-HOST-BOUNDARY-001.md) defines bounded input, acceptance and rollback.

Actual code written/compiled: one new `result_continue_host_lfr_probe.cpp`, selected playable source closure g++ exit0. Source pulse control and natural350 cases both naturally KO at host8; root source fields compare 1683/1683 and 3938/3938, with root terminal code46 at the correct frozen boundary. Before revising the probe, Task amended the pulse-versus-held distinction: preserve the first pulse control and latch Attack from prior timer142 for the final held-v2 evidence. No Unity or formal file is changed.

Capture fidelity correction before v3 edit: v2 source requests Attack at phase1/current143, but formal LFR captures sampled `input.current`, retained neutral until phase0/current144. Root v2 thus does not establish an already-held button before144. Task now requires hold from phase0/current142 and an explicit sampled-attack CSV field. This is diagnostic capture correction only; no production input behavior is changed.

Final scoped evidence: v3 source compile/run exit0; source requested/sampled Attack and root current mask16 remain active on host150–152 (timer142/143/held350). Root host152 writes350/transition0; host153 emitstransition2 with unchangedWorld152 and unchangedRNG, only pending cleared16→0. Source/root12 fields1836/1836; natural350 control11 fields3938/3938; no first differences. Root process/replay reports remain terminal46/false due to the recorder's strict tick-advance carrier gate, as planned, not battle failure. Direct root trace is the authoritative observable evidence. No Unity production/test/assets or formal files changed. Root upper1 and full parity are unverified. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C008-ROOT-HOST-BOUNDARY-001/REPORT.md). Rollback remains only this owned new probe after worktree review.

Final checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` PASS (1052 records,12 governed code files in shared diff); `git -c core.safecrlf=false diff --check` PASS. Root EXE and Battle/Menu Scene, GameConfig and ProjectBattleModeConfig SHA values match protected baseline. Formal/staged Naruto DAT bytes are identical and current probe bytes equal the compiled-v3 source snapshot; [hash manifest](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C008-ROOT-HOST-BOUNDARY-001/artifact-hashes.json). No Unity test/Play was repeated for this native-only evidence package.
