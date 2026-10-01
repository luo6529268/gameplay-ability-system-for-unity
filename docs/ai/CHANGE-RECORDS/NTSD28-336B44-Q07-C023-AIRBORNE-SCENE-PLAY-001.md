<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07AirborneIdleBattlePlayProbeEditor.cs
authority: selected336B44 formalGuren388 natural619 to85 airborne state0 postcounter frame gate
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001.md
-->

# NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001

Created before code. Exact natural source/root case, current ordinary initial-state boundary, comparison/shutdown/risks/acceptance/rollback in Task. Only Editor diagnostic; production and serialized resources unchanged.


Actual code written: reused original Editor request/lifecycle and32tick scalar capture; ordinary roster84/2/action388/0 starts on ground. Natural619/85 indexed entities captured with no direct spawn/Y/roll/action mutation. Probe birth/next-tick predicates from measured source, independent offline14fields+sixRNG comparison still required. Compile/Play pending. No production changes.

Original Editor compile0 errors: Tundra4.07s, Editor DLL21:39:14; domain reload completed andMCPconfirmedidle/nonPlay. Submitonlyunique airborne-idle-scene-01; wait same32tick/compare/normalexit, no restart onobservationtimeout.


2026-09-30 Scene01 preserved: local birth/next predicates PASS and exit/clean, but offline1690fields has62differences. Earliest RNGdifference tick24, entitydifference tick28/slot51/action213 versus215. Before any production correction, source BattleConfig difficulty_level_4a0c30 defaults0 whereas Unity world Difficulty default2; this candidate fixture mismatch must be eliminated. Pre-edit declared diagnostic amendment: capture original runtime difficulty, battle/local modes and AI phase; set only transient World.Match.Difficulty=0 matching source config, capture effective difficulty; retain production mode/map/input/AI untouched, no Assets serialization. Repeat unique Scene02 only after originalEditoridle. Capture full32ticks as before and compare; Scene01 evidence never overwritten. Actual initial/effective values determine whether mismatch was confirmed. If divergence persists, locate writer before further edits.


Actual diagnostic amendment written at declared path: Report captures difficultyBefore/effective andBattle/LocalMode/AiPhaseGate; WaitForRoster sets only temporary Match.Difficulty0 before sample. OriginalEditorTundra9.30s0errors, DLL14:04:06UTC, reloadready14:07:10UTC; uniqueScene02 started14:08:07UTC, pending. Saved probe-v2 snapshot/compile identity. Scene01 birth assertions passed but complete1690 comparisonFAIL62; earliestRNGtick24, entitytick28 preserved. No production code edits. Startupledger1063records/0governedtrackedcode diffPASS (probe was already tracked at latest baseline; do not invent prior23).


2026-09-30 C023 限定关闭（覆盖此前自然/Scene待验）：Guren84从地面受控初始388按正式DAT自然生成619/Y-40，tick21由267→268生成85/action0/Y-22，同tick按计数后转212/counter1/Vy0；tick22 counter0/Vy1.7，后续自然重力/落地/AI均纳入32tick。源/336B44正式根1982声明字段相同/exit0/PASS；原Battle Scene唯一airborne-idle-scene-02 PASS/DONE，107活跃实体行×14字段和32tick六RNG标量共1690/1690、首差0/速度差0。原EditorTundra9.30s0错，idle/nonPlay/noncompiling；正常exit/Scene clean/四保护SHA不变。Scene01 local出生PASS但完整1690字段FAIL62保留：先RNGtick24，再实体tick28。Scene02实测World初始difficulty2，与正式root0不同；只在临时诊断World统一0后整段通过，生产AI未修、默认配置未改。这是fixture口径修正，非新增生产缺陷。既有完整tick5/5、相邻C0223/3/C25G4/4和type3控制复用，不重跑无关测试。关闭共享空中state0后计数转212门；不是物理键选388/所有类型自然入口/全World/checksum/画面或整场证书。Q07与总目标继续ACTIVE，下一G1为F02自然高速武器进入state1000的根/原Scene出口。


Final validation: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot current checkout -> PASS1063records,1governedcodefile covered by AIRBORNE-SCENE-PLAY-001; full output ledger-check-final.txt. git -c core.safecrlf=false diff --check -> exit0. OriginalMCPfresh editor-final-state.json confirms originalBattleactive/idle/nonPlay/noncompiling; scene02-protected-hashes.json four SHA equal. Two PNGmeta importer changes observed during Play remain preserved/unattributed; no manual importer edit or rollback, not included in four-protected-asset claim. Existing unrelated untrackedB11files remain untouched. No new production, DAT, Scene or configAsset changes; no unrelated tests rerun.
