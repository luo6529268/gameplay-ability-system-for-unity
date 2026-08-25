# R8-WP01G-R07A — render pass / hit-record writeback joint runtime certification

> 建立日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE`  
> D-ID：`D-SCHED-009`、`D-RENDER-002`

## Goal

以C++ Release live source为唯一规则，在真实Unity production tick中证明PreFrame/Stage→RenderDispatch→
FramePostProcess→LateEntityUpdate的可观察顺序，并证明由正式collision/hit路径产生的hit record在本次
RenderDispatch中按冻结前age绘制、live owner恰好推进一次，使下一tick capacity/RNG gate与C++一致。

## Scope

### 允许

1. 只读复核C++ release `game_tick.cpp`、`renderer.cpp`与kind0 hit-record writer；
2. 复用WP01C-04正式collision/hit Play路径产生真实hit record；
3. 运行现有hit-record、worker publication、central submission、capacity/RNG focused tests与self-check；
4. 若现有诊断无法形成联合证据，批准后先建立test-only Change Record，再新增一个Editor-only Play probe；
5. 记录tick/pass、published sample age/count、RenderDispatch后live age/count、postprocess/late状态、
   stable id/slot/generation、RNG before/after/call count与cleanup；
6. 分别覆盖non-worker publication、worker publication和no-publication exact自动矩阵；
7. fresh compile、focused tests、full self-check、Play报告、Console0与ledger validator。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不直接写candidate、hit结果、hit-record live结果、RNG state或调用finalizer/advance制造PASS；
- 不移动battle pass，不改变worker acknowledgement、checksum、FrameInputSet或30Hz；
- 不修改CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× scale、fixed camera、capacity或0GC边界；
- 不处理R07B liveness/identity/visibility、R07C fail-closed ownership、P1/P2、AI、T8、IL2CPP、Android或服务器。

## Authority / Evidence

- C++ `src/entity/game_tick.cpp:2061-2083`；
- C++ `src/render/renderer.cpp:687-758,1300-1438`；
- Unity `NTSDBattleTickSystem.RunPresentationAndCleanupPhase/RenderDispatch`；
- Unity `BattlePresentationShadowBuild.FinalizePublishedHitRecordCycle`、
  `AdvanceHitRecordsWithoutPublication`；
- existing `BattleRuntimeSelfCheck` hit-record lifecycle/writeback matrix；
- `BattleHitExecutionPlanEditorTests.FullKind0HitRecordCapacity_DoesNotAdvanceBattleRng`；
- WP01C-04 collision/hit Play evidence；WP01D-06/07 central submission/pixel evidence。

## Acceptance

1. actual collision/hit在production world产生hit record，不通过probe直接造live结果；
2. published snapshot/command保存advance前age，符合C++本次blit输入；
3. 同tick RenderDispatch返回前live age/tail count完成一次writeback；
4. FramePostProcess与LateEntityUpdate不得重复推进；Late/worker fallback保持幂等；
5. no-publication路径按available lifecycle catalog推进，unavailable时保持不变；
6. valid、invalid non-tail、invalid tail、10-slot full gate与RNG call count exact matrix全部PASS；
7. worker/non-worker的最终live state、tick与hash一致；
8. cleanup恢复world、slot、pool、driver/presentation状态；warmed path 0 B；
9. fresh compile0、focused PASS、Play PASS、self-check PASS、Console0、ledger PASS。

## Files likely involved

- 现有测试与Play probe（优先复用）；
- 如确有必要：一个新的`Assets/NTSD/Scripts/Test/Editor/`联合Play probe及`.meta`；
- Task/Research/Change Record/Ledger/STATE/register/main plan/handoff；
- production gameplay默认不应修改。

## Verification

1. source crosswalk逐点复核；
2. hit-record lifecycle/writeback与capacity/RNG focused matrix；
3. worker publication/ack focused matrix；
4. actual collision→hit-record→central publication Play；
5. full `BattleRuntimeSelfCheck`；
6. `Tools/Validate-ChangeLedger.ps1`；
7. 结构化Temp报告记录first-difference或PASS。

## Stop conditions

- 发现production first difference；
- 需要修改production但尚未建立独立修复Task/Change Record；
- 需要改变pass order、RNG、worker、checksum或受保护adapter；
- current scene/DAT不能产生真实hit record；
- 观察点必须直接写live结果才能获得；
- 只剩C++ executable/full trace证据而R1-WP02仍BLOCKED。

## Out of scope

R07B、R07C、P1/P2、AI、debug keys、T8、IL2CPP、Android、服务器、C++ executable/full trace。

## Authorization

用户于2026-08-23明确批准：`批准执行 R8-WP01G-R07A，恢复目标`。本包已完成source/crosswalk复核，
existing exact baseline job `7ec88f1aa50f4f93af44990ad9a08dd6`为2/2 PASS；在脚本写入前已建立
`R8-HITWRITEBACK-001`并只新增一个Editor-only联合Play探针。该Change现已按下方Completion推进为
`VERIFIED`。

## Completion

`R8-HITWRITEBACK-001 / VERIFIED`只新增Editor-only probe，production未改。worker Play tick843～846
通过actual kind0 producer、3个published cycle、1个CentralOnly no-publication cycle、每tick exact 2 RNG、
frozen/live age、central command、Late幂等、0 allocation violation delta与完整cleanup。fresh compile0；
worker18/18、hit178/178、central13/13、full self-check PASS；final Console0；ledger PASS。

最高结论：`D-SCHED-009`与`D-RENDER-002 = UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。R07B、
R07C与R08不在本包内，未开始。
