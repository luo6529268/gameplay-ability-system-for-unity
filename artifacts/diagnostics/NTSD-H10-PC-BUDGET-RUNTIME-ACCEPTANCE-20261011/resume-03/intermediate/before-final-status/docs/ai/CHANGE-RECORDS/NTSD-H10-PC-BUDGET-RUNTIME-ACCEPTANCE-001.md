<!-- CHANGE-RECORD
id: NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001
status: IN_PROGRESS
code-path: Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs
authority: Human approved specific PC engineering targets and bounded same-H10 runtime diagnosis in discussion thread 01a11fe2-bfaa-72f3-a9ed-c8260f63b193
evidence: artifacts/diagnostics/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011/resume-02/REPORT.md
-->
# NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001

Pre-change record, 2026-10-11. Sole code path and new symbols: H10ResidentClosureDiagnostics.PcBudgetRuntimeQualification, PcBudgetRuntimeThreeSessionsPlay, RestorePcBudgetRuntimeScene, nested PcBudget fixture/evidence helpers. Existing methods and production behavior preserved. Previous evidence includes two-session full catalog object ledger and smaller lifecycle/static coverage; it does not cover this combined live Play/dispatch/three-cycle task.

Task contract declares approval, exact read/write domains, first-difference stop, minimum tests, state owner/capacity/stage1/stage5/drain/deferredDestroy lifecycle and rollback. Operation OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 contains ten actual preimages/SHA. No runtime state/cache/threshold added to production. Diagnostic state is bounded to one test fixture; no Q06/body or rule changes.

Risks and side effects: original Editor scripts import, a single isolated Play session and strictly owned audio fixture lifecycle; scene setup restore and exact cache/voice/config/manager preservation are mandatory. No irreversible production change; unknown ownership or unsafe Editor blocks execution instead of cleanup. One bounded group only after compile and qualification; no same-version full rerun. Process bytes observed in Editor are diagnostic, not game-process/8GB certification. AudioClip budget may reuse old measured envelope only after current input fingerprint correspondence.

Validation planned, not yet run: compile; PcBudgetRuntimeQualification; PcBudgetRuntimeThreeSessionsPlay; source/resource/Git preservation; Tools/Validate-ChangeLedger.ps1. Code not yet changed at this record creation. Player plan only read, no build. Rollback only precise task additions from audited actual preimages, preserving later user edits and old evidence.

Actual first implementation appended PcBudget* diagnostic members and separate outputs only. Existing method bodies retained. Static review before compile: protect construction failures, capture owned references before first public shutdown, preserve first hot failure samples. No Unity run yet.

Original Editor compile accepted after two new-fixture type corrections (CS0246/CS0534, then CS1061); both failure logs retained. Third compile/domain reload completed, no compiler errors in compile-console-03.json. No Play yet. EditMode runner chosen so plugin does not adjust PlayMode/domainreload options; all-scenes-clean checked before its auto-save guard.

Final scoped status: PC_TARGETS_ADOPTED / RUNTIME_CALIBRATION_FAILED. Current1682 inputs match; old228918567B reused below320MiB. Compile accepted; qualification1/1PASS; unique Play0/1FAIL at byte[97] positive control reporting0. No valid hot samples or three-session/playback/cancellation certification. First prepared22owned objects; real public Driver stage10, empty-map stage11, drain/next-frame release; exact generic016 removed, fixture/state/cleanMenu restored. Source remains exactly measured failed version; no rerun or production patch. PrivateMemorySize64 returned0/unavailable, not process budget PASS. Minimal same-file recorder/generic fixture repair and four-source Player review in artifacts; H10 parent remains OPEN/in progress, no auto-pause/Goal/other point.

Final governance validation: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` PASS / exit0, 1422 records and one governed changed script covered, zero warnings for this Change. Other-record4531 declared-path-not-in-diff warnings retained. Prior two WindowsPowerShell invocation failures (default root evaluation; native Git LF/CRLF warning handling) retained; validator/checks/Git configuration unchanged. Protection audit4094 named fingerprint rows no drift; eight source/assembly fingerprints unchanged since Play; all ten real initial backups valid; old diagnostic body preserved; HEAD/staged unchanged and all14 initial status untracked entries retained (12 historical plus2 current-task directories). `git diff --check` exit0. Exact evidence: task artifacts protection-audit.json, validator-pwsh7-result.json, change-ledger-pwsh7.log, final-audit.json. This evidence-only appendix does not alter the validated Change metadata or script; no test rerun.


## resume-01 pre-change registration

> **2026-10-11 H10 resume-01 IN_PROGRESS。** 独立审阅后恢复Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001。仅PcBudget诊断计量/负控/保护修正，先无解码资格，全部通过后仅一次新版本三轮Play组；首差停止。旧分配校准FAIL原件保留，生产/Scene/资源/Settings冻结，预算已批准/对象门复用保持，H10父项OPEN，无Player/其他点/Goal。计划 artifacts/diagnostics/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011/resume-01/PLAN.md；Operation OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-RESUME01-20261011。下方为此前时点。

Exact new symbols: PcBudgetAllocationMeasurementQualification, PcBudgetAllocationBoundary, PcBudget component snapshot/restore evidence, revised queue/sink/pipeline measurement and real generic016 guard. Earlier non-PcBudget methods remain unchanged. See resume-01/PLAN.md for ownership/capacity/finally/shutdown, minimum compile/qualification/single-Play gates and rollback. Current failed source and all eleven dirty files backed up before this update; old root evidence unchanged. No tests run yet for this version.

resume-01 CODE_WRITTEN (before compile): only PcBudget members changed; source-version-01.cs and static-review.json preserve exact implementation. Three independent recorder instances, outer sampling disables nested sink; per-range before/after positive+empty calibration and try/finally End/Dispose/output. Real generic016 cold identity assertions; stable Menu serialized-object identity and fixture config/native snapshots; actual App.BattleBootstrap map-free guard; pending/naturalDestroy identity evidence. Existing earlier methods byte-equivalent after newline normalization; no production/recorder changes. Stages list fixed capacity24 for20 expected rows (corrects planned16 before execution); no hot growth. All protected inputs matched. Minimum compile and two no-decode qualifications remain pending; Play only after both pass.


## resume-01 actual final result

> **2026-10-11 H10 resume-01：MEASUREMENT_QUALIFIED_FIXTURE_CUE_ID_MISMATCH。** Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001 BLOCKED（本轮夹具）。首次编译PASS，无解码资格2/2PASS：三个独立GC.Alloc recorder前后正控有效/empty0，Unit原样TimeNanoseconds；不是生产热0GC。唯一新Play0/1 Error，冷generic查询data/016与实际注册data\016 raw key不一致，wrapper null，未进热循环；归属执行者夹具错误，生产未改，不追加组。22owned失败清理公开stage10/空地图stage11及下一帧释放通过，generic016精确清理；当前Menu clean8/组件快照/config/MM settings相同。4094保护/旧105artifact无漂移，旧其他方法体保持。预算已批准与218.313758MiB对象门复用保持；三轮/播放/取消/Player/进程门未过，H10父项OPEN。报告 artifacts/diagnostics/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011/resume-01/REPORT.md；下一仅同H10精确raw-key/guard修正独立审阅，不自动运行，无新Goal/队列/Player。下方保留此前时点。

Actual commands/results and no-source-change-after-measured-version: resume-01/commands.jsonl, unique-play-tests.xml, RESULT.json, protection-audit.json, resume-only-source.diff. Qualification job9da6e4f63e3445a3818a842540832e87 total2pass; unique Play jobfde8ef10276c4f4b85bcb4322f479124 actual XML0/1Error15.118494s despite stale bridge running. Null wrapper arose before ExerciseHot, no hot samples; runtime fixture omission of pre-read nonnull assertion documented. No production defect conclusion, no runtime certification. Existing old failure and root evidence preserved. Validator command is pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity; actual original log/result saved under resume-01/change-ledger.log and validator-result.json.

resume-01 validator actual: PASS exit0 under existing PowerShell7;1422Records/one governed changed script covered;zero own warnings,4531other historical warnings retained. git diff --check exit0 with LF/CRLF notices only. Original log/result in resume-01. This appendix changes neither metadata nor measured code; failed Play remains BLOCKED, not certification.


## resume-02 pre-change registration

> **2026-10-11 H10 resume-02 IN_PROGRESS。** 同Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001，经独立审阅仅修PcBudget真实raw-key、无解码成员资格、cold wrapper guard及关闭证据字段；三个已通过recorder算法不改。先编译/本版资格2项，全过后仅一次新版Play，首差停止。原两失败/生产/预算及对象门复用保持，H10父项OPEN。计划 artifacts/diagnostics/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011/resume-02/PLAN.md；Operation OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-RESUME02-20261011；无Player/Settings/其他点/Goal。下方保留旧时点。

Exact symbols/scope and owner/capacity/shutdown/minimum validation/rollback are in resume-02/PLAN.md. Only PcBudgetGenericCue, output/restore key, RuntimeQualification raw-key membership, RunThreeSessions cold wrapper/clip/cache checks, CloseDriver evidence label. Three recorder implementation and old non-PcBudget methods unchanged. Current dirty11backups before source edit; old196artifacts and9source/DLL/recorder fingerprints preserved. No new test yet.

resume-02 CODE_WRITTEN before compile: exact four repairs plus output/session key. Raw key data\016.wav uses readonly BuiltInBattleSoundIds Ordinal membership, distinct disk path/cache evidence; cold raw-key/nonnull/same-wrapper and public clips-array identity checks before use; closure field now explicitly whole public-shutdown return(stage10). No recorder/ExerciseHot or old non-PcBudget byte changes. No new state/cache. Exact version in resume-02/source-version-01.cs, static-review.json, resume-only-source.diff.4094protected hashes match. Compile/new qualification/new single Play pending.


## resume-02 actual result

> **2026-10-11 H10 resume-02：SCOPED_HOT_0EVENTS_PASS / SECOND_CYCLE_GENERIC_PLAY_OBSERVATION_FAILED。** Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001 BLOCKED。首次编译及新资格2/2PASS，raw-key/两轮cold wrapper与generic同引用通过；两轮各7tick×3范围共42有效校准scope0事件，无热load/build/reject增量，非整Driver/千人实体。第一轮实际播放/非零输出及公开关闭重入完成；唯一Play整体0/1FAIL于第二轮genericVoice在播断言，首个同clip voice选择器有观察风险，实际原因未证明，未改生产/追加组。两次关闭/owned后帧归零/精确generic清理及当前Menu组件/config恢复通过。4094保护/9指纹/196旧artifact保持。三轮/pending/natural/Player/进程门未完成，H10父项OPEN；预算与对象门复用不变。报告 artifacts/diagnostics/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011/resume-02/REPORT.md，下一仅同H10精确播放观察独立审阅。无新Goal/队列/其他点，以下是旧时点。

Actual tests: new no-decode qualification2/2PASS job4e7eea342d314c3cb3964a5971ab1a27; unique Play jobfcfe226f650b448cbf6be15f1090b02a actual XML0/1FAIL24.919952s.42real scopes0events after valid before/after controls, first normal cycle complete, cycle2generic observationFAIL at4125; no hot allocation or production playback defect proved by failure. Legacy helper first-match behavior is only a diagnostic risk, not established root cause. Scope evidence/cleanup/source fingerprints in resume-02, source a833e450dbeb17fa3b9a1455dba1a81d6c268a525f8ba3dcb2fc166fbd9efe5b frozen. Planned final validator command: pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity; actual log/result in resume-02/change-ledger.log and validator-result.json. No new execution after first failure.

resume-02 actual final validator PASS exit0:1422Records/one governed changed script covered/zero own warnings/4531other historical warnings retained. See resume-02/validator-result.json and original change-ledger.log. Evidence-only appendix, validated metadata and measured source unchanged; overall runtime remains BLOCKED at second generic observation.


## resume-03 pre-change registration

> **2026-10-11 H10 resume-03 IN_PROGRESS。** 同Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001。仅PcBudget冷播放观察：复制voice/序号前后值识别本次真正启动源，完整证据先于断言，公开关闭前后精确借用观察。旧helper/三recorder/ExerciseHot/生产冻结。两项本版资格通过后仅一次三轮Play组，首差停止。旧失败保留；H10父项OPEN、预算/对象门复用不变，无Player/Goal/其它点。计划 artifacts/diagnostics/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011/resume-03/PLAN.md；Operation OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-RESUME03-20261011。下方为此前时点。

# H10 resume-03：冷播放观察身份修正（脚本修改前）

Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001。授权来源：人类继续H10及已批准PC/320MiB/2GiB/3GiB预算；讨论会话01a11fe2-bfaa-72f3-a9ed-c8260f63b193独立审阅后的resume-03准确派发（执行turn01a12921-3e24-7370-9258-494349611e63）。此前byte97校准、raw-key及第二轮generic播放观察失败完整保留。

唯一脚本为Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs；只更改PcBudgetOutput/RestoreKey、本Task新增PcBudget冷快照/选择helper、RunThreeSessions的formal/generic播放观察、CloseDriver的关闭前后观察。旧FindVoiceWithClip及非PcBudget前缀，三recorder/资格算法、sink/emitter、ExerciseHot逐字冻结。所有生产/其它测试/Scene/资源/Settings/Plugins冻结，不读Q06正文。

复制调用前后AudioSource值（包括inactive、ID、clip、parent/host归属、enabled/active/isPlaying/loop/pitch/timeSamples/dsp），复制oneShotVoices槽位和long启动序号。仅留原数组引用用于ReferenceEquals身份验证，不把可变数组内容当before；保存旧selector、全部matching/playing及序号推进映射。直接只读原生产播放/loop/load/build/failure/rejection计数，不写hook、不调用任意getter。

每次单formal或generic调用必须总播放计数增1、无load/build/failure/rejection增量、原数组及逐槽source身份不变，恰一启动序号推进且新声源clip为期望同引用，逐项断言非null/enabled/active/isPlaying。完整冷证据CreateNew落盘先于播放断言。8帧波形也先落盘再generic，非零输出与人工听感false分列。

CloseDriver传本次真实generic源；在公开ShutdownBattleRuntime前后立即采其身份/clip/启用/在播/timeSamples/dsp并先落盘，保留原借用不可停断言、stage10返回、空地图stage11、tail/drain/后帧owned归零。自然结束与Stop无法区分时FAIL/UNKNOWN，不重播/loop/延时/改pitch制造通过。

新状态均属单个PcBudget测试调用的冷诊断快照，容量由既有64voice池和自有audioHost实际source数量一次确定，逐槽复制数组，无生产缓存/热增长；只在热scope外创建。证据输出后快照由方法释放，不参与生产阶段；fixture仍stage1停止请求、stage5原声音owner回收，stage10/11公开关闭、await drain和后帧Destroy。无新增runtime服务或关闭顺序。

最小验证：当前dirty11份真实备份、源码/程序集/recorder9指纹、4094保护与旧全部artifact；原Editor新鲜idle/nonPlay/noncompile/update/nontest/Menu clean8。使用既有MCP（已知CLI Pipeline不可用，不安装/改工具）最窄编译；本版PcBudgetAllocationMeasurementQualification和PcBudgetRuntimeQualification两项。全部通过才一次PcBudgetRuntimeThreeSessionsPlay隔离EditMode Enter/ExitPlayMode组，三轮原负载/真实play/public close/reentry后同组pending取消和自然UnityDestroy。任一硬门首差立即停止剩余步骤，finally仅安全清理，源保持实际受测版本，不重跑找PASS。

三独立current-thread GC.Alloc/Internal/cap256及前后1MiB正控/empty保持，Unit TimeNanoseconds不是字节；1000声音请求不是1000实体，非wholeDriver/CPU收益。旧218.313758MiB对象账本仅当前指纹匹配后复用，不重解码全catalog；进程/Player/目标PC/Android门不声称通过。

风险：旧selector选中已停的同clip声音、快照可变引用污染、证据观察耗时使短clip自然结束、关闭误停借用、测试失败后现场恢复。所有观察在热外且不跨await；未知字段null/UNKNOWN，关键缺失fail-closed。generic三轮同引用且最终精确key/clip/borrower检查后才RemoveCache/去重Destroy；不清旧资源、不GC.Collect、不UnloadUnusedAssets/Editor重启。

回滚仅从本次真实dirty备份审计撤本诊断增量，不能HEAD覆盖或撤生产。新原件仅resume-03 CreateNew；旧root/resume01/02冻结；Temp/Goal18_LastTestResults.xml每次callback覆盖前备份真实前件。治理限Task/Change/Ledger/STATE/CURRENT-AUTHORITY/handoff/H10卡/进度/对齐表H10状态与Operation/INDEX追加，预算决定不改。进程字段仅PID/创建时间，禁止CommandLine/环境/凭据及全字段输出；旧脱敏事件保留不复制原敏感数据。H10父项仍OPEN，无Goal/34队列/其它点。


resume-03 CODE_WRITTEN before compile: copied cold PcBudgetVoiceObservation/PcBudgetColdVoices with read-only known fields; PcBudgetObservePlayback persists before assertions and identifies exactly one advanced sequence with preserved array/index/source identity. RunThreeSessions formal/generic now uses actual started source, waveform persisted before generic. CloseDriver snapshots exact borrowed source before/after public return before unchanged lifecycle assertions. Old non-PcBudget 195422bytes, three recorders/qual/sink/emitter and ExerciseHot byte-identical. No production mutation or test run. Exact source-version-01.cs/static-review.json/resume-only-source.diff in resume-03.

resume-03 COMPILE_PASS / QUALIFICATION 2/2 PASS: original Editor refresh scripts + completed domain reload, no C# error. Qualification job7fc9ccdf217d4a1a88a1f5f2992177a1, measurement0.146295s/runtime0.479762s. Exact XML saved; all1682 input fingerprints fresh-match. Unique authorized Play job8643d09589eb42699199c1dbcfc2ed5b started only after fresh idle/cleanMenu8; not yet runtime acceptance. Source d36cd18e6bffc68090891dea9874ea8c5d2ff56ae13d826dcef792e2edfd8b4e, EditorDLL1ac32f7bad57dcc6a28a8a6dc552f28b12c6bdb42fa9d07ab5fcd236f6d5f9c3, runtimeDLL/production/recorder unchanged.
