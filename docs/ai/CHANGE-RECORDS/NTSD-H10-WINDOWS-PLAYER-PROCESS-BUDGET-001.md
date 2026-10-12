<!-- CHANGE-RECORD
id: NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-001
status: ABANDONED
code-path: Assets/NTSD/Scripts/Test/Editor/H10WindowsPlayerBudgetBuild.cs
code-path: Assets/NTSD/Scripts/Test/H10WindowsPlayerBudgetProbe.cs
code-path: Tools/Diagnostics/H10WindowsPrivateBytesSampler.ps1
authority: Human scope decision on 2026-10-11 closes H10 current optimization and cancels the unimplemented Player route; no fourth processor or runtime approval
evidence: artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/closeout-user-scope-01/REPORT.md
-->

> **2026-10-11 H10 CLOSED：按用户决定结束当前优化范围。** 用户取消新增Windows Player出包验收路线，未实施Task NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011与Change NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-001均ABANDONED；原因是范围取消，不是运行失败。第四processor待批事项撤销，三个新脚本/两个meta不创建，兼容提案不实施。PCM有界/逐局formal owned、同输入320MiB对象门及resume03隔离Editor三轮音频门按原scope保留；Player、整进程2/3GiB、目标8GB型号、完整千人预算、人工听感仍未验证，但不再是本次H10 active待办或结项前置。旧compat13/14、成本尖峰、失败/事件不追溯改PASS。决定：docs/ai/DECISIONS/NTSD-H10-USER-SCOPE-CLOSEOUT-20261011.md；结项：artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/closeout-user-scope-01/REPORT.md。当前H10已结束，下一无自动启动项；H08/H07/M03/producer/H06等hold、33ms/十一阶段及全部原边界保持。下方旧OPEN/等待第四域/继续出包和旧统计只保留为被本决定替代的历史。


# NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-001

PLANNED only. All three code paths and two C# meta files are confirmed absent; they are proposed ownership, not implementation facts. Current work only read-only preflight and governance. Source/settings/build/Player/sidecar executions=0. No source is deliverable yet.

Concrete before-state: current serialized Standalone IL2CPP, safe original idle Editor; stress runner requires emptyWorld/pools, changes to MobileExtended/Manual and cleanup RecreateWorld closes audio before later true App map shutdown. Old single-cycle probes cannot supply a three-cycle process budget. Unity internal PrivateMemorySize64=0 remains unavailable. Do not call old R8 builder or modify it.

Only proposed new symbols: H10WindowsPlayerBudgetBuild.BuildOnceFromRequest/ValidatePreflight/RestoreOwnedBackendChange; H10WindowsPlayerBudgetProbe opt-in Install/RunOrdinaryThreeSessions/RunCombatThreeSessions/CaptureOwners/CloseThroughApp/EmitPhase; H10WindowsPrivateBytesSampler exact identity/100ms sample/export. No production/callback/old recorder modification. No script written.

Ownership/capacities: one Editor callback owner, one opt-in Player probe host, <=256 phase rows, <=2048 owned references per round/6144 retained across3rounds, external one-PID sampler. Full stage1 admission stop, public stage5 audio owner, existing stage6-10, actual App stage11 map proof, tail/deferred destruction and own-event unsubscribe in PLAN. No new production cache/worker/manager. Risks: bootstrap suppression/readiness, true map ownership, cleanup resetting audio, UI reentry, raw-file deployment, sampling gaps, concurrent Settings changes; R1-R4 reads required before final implementation specification.

Minimum candidate verification (not run): exact new-script compile/narrow qualification; one Windows Mono Development build with fresh backend true-byte backup and finally verification; exact sidecar manifest; proposed twoPlayer x threeRound (smoke120+180 then twoformal120+1800); original GC/capacity/correctness gates retained; exact public shutdown/borrowed/owned/no accumulation; 320MiB object,2GiBsteady/3GiBsampledtransition; protected evidence and original ChangeLedger. The executable quota is not self-approved by this plan. See Task and preflight PLAN for finite timeouts/known scope differences.

Rollback: retain all current dirty/untracked and old resume03/source/artifacts; only current new diagnostic additions and attributable backend field may later be handled via fresh Operation true-preimage guard, never HEAD/global cleanup. New files/failedbuild not automatically deleted. Current preflight only appends governance and CreateNew artifacts, Operation OP-NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-PREFLIGHT01-20261011. H10 parent OPEN; no H07/Player/device success claim, no Goal/next point.

Preflight coverage correction before implementation: see preflight-01/H10-COVERAGE.md and PLAN section12. Single-OID1000/ordinaryNaruto proposal does not close multi-role/skill worst-set or allproducer dynamic coverage. CASE-MATRIX and exact finite execution table must be independently locked within this Task before build authorization; no implicit third process or new code/body domain. Status stays PLANNED, no scripts/build/runtime run.


## PRE-FLIGHT-01 addendum-01（当前补充，原文保留）

状态仍PLANNED，未执行代码/Settings/编译/测试/build/Player/sidecar。R1-R3已由主会话具名供证，精确采取方案及来源见 `artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/preflight-01/PREFLIGHT-ADDENDUM-01.md`。新的具体用例为2/16/507三角色三轮，见同目录`CASE-MATRIX-CANDIDATE.md`；四cue当前SHA与旧inventory相等，见`case-evidence-inputs.json`。并非所有producer/角色技能动态穷举；原“全部动态覆盖”措辞按本补充纠正，不降低全catalog静态＋实际运行请求覆盖门。

仍需独立审查：`NEW-READ-DEPENDENCIES-01.md`的R4A/B、R5A/B，`OBSERVATION-SEAM-REQUEST.md`的只读事件转发入口；`EMPTY-WORLD-ENTRY-ORDER.md`说明真实MatchConfirmed后仍自动Set/Load原cfg，因此原空players回调方案不闭合，不能直接实现。不得以反射写原cfg、清场或重复Load补救。候选跨进程模板/实际UI模式引用方案及100ms/250ms/99%采样质量/无自动kill处置均尚待主审冻结。

保存的原校验：ChangeLedger exit0/PASSED1423 records/4534 warnings，3条属于本PLANNED未创建脚本，不称本Change0warning；protection-audit的4094保护/406旧H10/18源程序集Settings无漂移。最终补充审计另存，不覆盖原件。没有恢复旧目标或跳其它优化；H10父项保持OPEN且继续同Task。


## PRE-FLIGHT-01 review-02 scope refinement (PLANNED only)

Current source2334/80126190B vs old1373/46927691 has extra978/missing17/overlap1356 identical. New exact CSV and SHA evidence in `artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/preflight-01/postprocessor-compatibility-preflight.json`. Main independently verified2334 and allknown2752/96956488B; not deployment/runtime/rawUI closure. Compatibility proposal chooses immutable exact one-H10-build manifest inside unchanged callback gates; proposed additional existing processor path has NOT received direct-human authorization, NOT added to active code-path metadata, NOT edited. Proposal and fixed constraints: POSTPROCESSOR-COMPATIBILITY-PROPOSAL.md / REVIEW-02-CONTRACT-CLARIFICATIONS.md. Even fourth-path approval will not by itself authorize implementation/build until complete exact dispatch.

Finite cases/quality/owner/overflow/restore are now CASE-SAMPLING-RESTORE-V2.md, corrected by TEAM-ENTRY-CORRECTION.md: no same-team assumption, no allproducer dynamic enumeration, no exact two-consumed-tick promise. Observation failures must not throw through Driver after forwarding; record status, let original count/list cleanup finish, then stop at cold controller. Unknown original sink owner refuses installation. Driver277 cold readonly reference capture approved as plan; no reflection setter or _battleSoundPlayer write. Precise remaining rawUI/mode/input-data/cold-array/read scopes are in review-02 clarification. Three original new scripts/two meta/build-01 still absent; no code/Settings/Unity command/build/Player/sidecar/restart. SameTask continues, no Goal/queue/otherpoint.


## PRE-FLIGHT-01 review-03 source proof and diagnostic capacity clarification

Same Task/Change PLANNED; no implementation permission inferred. `preflight-01/REVIEW-03-CASE-CAPACITY-CLARIFICATIONS.md` now records main-supplied public team mapping (index0/1/2 -> team1/2/3), pure packet four-value copies, exact cold owner fields, and locally verified Menu serialized mode0/1 plus script GUID. Runtime Confirmed/mode/teams/full Running window still unproved. No Results writer body read. FrameInputSet/Players references are not historical records; fixed8192 tick observations each up to8 actual slots preserve four value fields and packet tick, missing ticks UNKNOWN.

Old metadata1000 raw includes19builtins/2mode; 1050 is old distinct clip count, not keys. `RAW-KEY-RESOLUTION-V4-ORDINAL.json` preserves exact case and raw identities, adds968 absent unique slash aliases, total1968/2048;14 colliding keys are original raw and win. V3 lowercased mapping stays historical only. New Player prepared set, natural coverage and tee0GC still require qualification/runtime. No production alias registration or sealed rule changes.

4096 rows are a diagnostic bound, not inferred1000-AI maximum. Ordinary target full rows and proposed combat full-stream counters/membership/order digest with explicitly fixed first-nonempty-batch sample are distinguished in new clarification; combat sample requires main review, does not silently change acceptance. Diagnostic overflow/unknown is evidence failure, actual production rejection/drop separate; still forward entire original list once, observation never throws into Driver, no expanding/retrying. Fixed buffers owned by probe; existing stage1 stops gameplay, ordinary restores tee before App close and combat restores only after original harness terminal if exact identity permits, followed by actual App map11; no new shutdown hook/order.

Remaining precise read request is `RAW-UI-NEXT-READ-REQUEST.md`: only Candidate ImageInput/Words/Spark/Kill inputs, Capture body/head/small paths, config-copy/hash binding. Formal/static known-domain remains unproved full raw UI closure. Fourth processor domain still awaits direct human and full implementation dispatch. No new scripts/meta/build, no Unity commands, tests, Player/sampler, process restart, Goal or next optimization. Existing scoped evidence reused, H10 parent OPEN. Operation before/after and current validator/protection are `review-03-before-manifest.json`, `review-03-operation-after.json`, `validator-review-03.json`, `review-03-protection.json`; result `REVIEW-03-RESULT.md` under same preflight root.


## PRE-FLIGHT current convergence (PLANNED only)

Main supplied exact Candidate image-input contracts and froze review03 evidence granularity/static VS source. Canonical current contract is now the appended convergence section of existing preflight-01/PLAN.md and PRE-FLIGHT-01.md, not another execution branch. It records body703/703 source/manifest equality, resource/system and both SPARK identities, optional Words six missing candidate files and KillIcon three missing candidate paths with distinct existing null/skip behavior. No resources added/copied, no full head/small proof invented. `resolved-input-deployment-comparison.json` / `kill-icon-input-data-check.json` preserve read-only raw facts. Remaining root/data bindings are only FINAL-PATH-BINDING-REQUEST.md; no broad parser/producer/Q06 read.

Fixed table stays1build/2process×3round (120+180,120+1800,120+1800), P1 three real public human slots/teams1/2/3/VS0/1, P2 approved emptyplayers App path and original1000 Manual harness; all implementation/build/runtime remain NOT AUTHORIZED by preflight. Exact new qualification names to be declared with implementation, no same-version retry. Safety covers freshEditor state, context-once/manifest, backendtruebytes/no unknown overwrite, known ownertee restore, actualApp map11/ownedtail, 100ms/250ms/99percent sampler and original320MiB/2GiB/3GiB gates, no automatickill/cleanup/extra quota. Fourthprocessor human confirmation and complete dispatch still pending with main, not asked again. Twelve true preimages and sameOperation; after/protection/validator in convergence-*; final local result CONVERGENCE-RESULT.md. H10parentOPEN; old successes/failures remain scoped and reused, no otherpoint/Goal.


## Final integrated cold qualification and hard-gate map (PLANNED)

Main supplied exact root/Manager/Catalog/explicit ProjectBattleModeConfig.Capture evidence and accepted the Editor-only static bridge in the originally proposed new H10WindowsPlayerBudgetProbe.cs (CaptureBuildInputEvidenceForEditor). Same-runtime-assembly existing internal CopyCharacterConfigs may be used after public Candidate.Capture, without production visibility/API/asmdef/reflection callback changes or host creation. Cold reads/parsing/managed allocations and second config parse are explicit, not hot0GC. Fixed4096 unique cold input rows are owned by the invocation and released on failure/export; no runtime manager/cache or shutdown integration.

Existing PLAN/PRE-FLIGHT now contain the single complete finite table plus final cold qualification/gate map. Before backend/BuildPipeline, capture exact original publication/config/manager/cache/player/singleton/Scene/serialized/source/settings evidence; inability to safely capture rejects before Candidate. Record actual body/head/small/common path/role/allowMissing/availability/bytes/hash/manifest and auxiliary fingerprints. Any unexpected state change/publication/decode/play/host, missing-required/undeclared path/hash drift stops before build, no alternate loader/repair/retry. Root/CASE/Ordinal/recording granularity/budgets are reviewed, not repeatedly undecided; runtime gates remain NOT_RUN. Fourthprocessor direct-human confirmation and full dispatch still pending, no code/Settings/Unity/build/Player/sampler permission inferred. No Goal/otherpoint; H10parentOPEN.

Twelve current true preimages and sameOperation in cold-qualification-before-manifest.json; actual record cold-qualification-operation-after.json, final protection/validator and CONVERGENCE-RESULT.md. A tool-input SyntaxError occurred before execution of the first final-append attempt; no shell or file mutation ran from that rejected input. Subsequent same-scope command uses these unchanged current-SHA guards.


## 2026-10-11 用户范围结项（当前终态）

H10 CLOSED；未实施Player路线ABANDONED。用户取消新增出包验收条件，第四processor待批事项撤销，不再等答复或完整实施派发。三个新脚本/两个meta不创建，全部preflight/proposal/FAIL保留。已实施PCM/逐局formal owned、320MiB同输入对象门/resume03限定音频门按原scope保留；Player/2与3GiB进程/8GB型号/完整千人/人工听感仍未验证，不列本次active后继。旧compat13/14/成本尖峰/失败不改PASS。决定见docs/ai/DECISIONS/NTSD-H10-USER-SCOPE-CLOSEOUT-20261011.md，报告见既有Player输出根closeout-user-scope-01/REPORT.md。仅治理检查，无源码/资源/Settings/Unity/编译/测试/测量/build/Player/sampler；无下一项/Goal，其他hold与所有原边界不变。此前PLANNED/OPEN/等待第四域/继续出包仅为被本决定替代的历史，不再授执行权限。

### 结项前字段快照（被本决定替代，仅历史）

取消前机器元数据作为历史文本：
历史元数据起
id: NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-001
status: PLANNED
code-path: Assets/NTSD/Scripts/Test/Editor/H10WindowsPlayerBudgetBuild.cs
code-path: Assets/NTSD/Scripts/Test/H10WindowsPlayerBudgetProbe.cs
code-path: Tools/Diagnostics/H10WindowsPrivateBytesSampler.ps1
authority: Human approved H10 Windows Mono independent diagnostic direction; current delegated PRE-FLIGHT-01 permits planning only before independent implementation review
evidence: artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/preflight-01/PRE-FLIGHT-01.md
历史元数据止
