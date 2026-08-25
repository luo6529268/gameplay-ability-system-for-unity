# R8-WP01G-R07A — render / HitRecord writeback joint runtime evidence

> 日期：2026-08-23  
> 结论：`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`  
> D-ID：`D-SCHED-009`、`D-RENDER-002`  
> Change：`R8-HITWRITEBACK-001 / VERIFIED`

## 1. Evidence ceiling

本报告证明Unity当前production worker tick、CentralOnly publication/materialization与HitRecord lifecycle在
目标联合夹具中通过。C++ Release authority仍只以live source裁决；R1-WP02 full C++ runtime trace继续
BLOCKED，因此本报告不能升级为C++ runtime逐tick完整VERIFIED，也不能扩大为整个render或battle已对齐。

C++ authority保持只读，未运行、构建、复制、插桩或写入。

## 2. Authority / Unity crosswalk

- C++ `src/entity/game_tick.cpp:2061-2083`：PreFrame/Stage后进入render，随后FramePostProcess与Late；
- C++ `src/render/renderer.cpp:687-758,1300-1438`：本render cycle读取HitRecord age，valid record推进一次，
  invalid tail每cycle最多移除一个；
- Unity `NTSDBattleTickSystem.RunPresentationAndCleanupPhase/RenderDispatch`：worker publication或
  no-publication lifecycle发生在FramePostProcess/Late之前；
- Unity `BattlePresentationShadowBuild.FinalizePublishedHitRecordCycle`与
  `AdvanceHitRecordsWithoutPublication`：按冻结owner handle/count/sample推进live age/tail；
- worker publication保持纯冻结且commands未物化；正式中央宿主随后复制到
  `CurrentPixelFramePlan.CapturedFrame`并物化commands，不反写worker frozen frame。

## 3. Test-only witness design

新增：

- `Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs`

探针只在Play Mode安全边界预建4组独立attacker/victim fixture。每tick只把一组放入碰撞位置，由正式
collision/candidate/hit writer与完整production tick产生一个kind0 record；旧victim保留在world继续参加后续
publication/no-publication lifecycle。探针没有调用`RecordKind0Hit`、`AddHitRecord`、
`FinalizePublishedHitRecordCycle`或`AdvanceHitRecordsWithoutPublication`，没有清rest、重置受击状态或直接写
record/RNG/candidate/hit结果。

前几轮失败均为probe假设错误并被如实留痕：

1. scripts-only refresh没有导入无`.meta`新文件，原0-error证据撤销；
2. test-only CS0102、CS0165、CS0266均在fresh import/compile中发现并修复；
3. 删除了不属于C++合同的Unity-only `LastAdvanceTick`断言；
4. worker frozen publication本来就不materialize commands，检查改为等待正式central captured frame；
5. 同pair及共享victim的下一tick命中会被正式rest/受击资格抑制，最终用独立pair而非清字段获得重复producer。

## 4. Production Play result

结构化报告：`Temp/NTSD_R8_WP01G_R07A_HitRecordWriteback.result.json`

- status：`PASS`；
- worker path：`true`；
- report start/end：841/846；安全边界后实际证据tick为843～846；
- baseline/final：ObjectCount 8→8、claimed slots 6→6、object pool active 2→2、logic pool active 6→6；
- RNG、stats、sounds、presentation owner与pause全部恢复；cleanup completed且无cleanup error；
- warmed tick/presentation allocation violation delta均为0；
- Play结束前Unity Console error为0。

| tick | publication | RNG calls | frozen ages | live ages | owners / records / commands | Late幂等 |
|---:|---|---:|---|---|---|---|
| 843 | yes | 2 | `[0]` | `[1]` | 1 / 1 / 1 | PASS |
| 844 | yes | 2 | `[1,0]` | `[2,1]` | 2 / 2 / 2 | PASS |
| 845 | yes | 2 | `[2,1,0]` | `[3,2,1]` | 3 / 3 / 3 | PASS |
| 846 | no | 2 | frozen cycle保持tick845 | `[4,3,2,1]` | frozen 3 / 3 / 0 new commands | PASS |

每个published owner均按stable id、runtime slot、generation和count核对；central command只接受对应victim
stable id与有效spark age/pic。no-publication tick没有替换cycle，但通过runtime lifecycle catalog推进全部live
records；随后Unity Late/worker fallback没有二次推进。

## 5. Regression evidence

- fresh Unity scripts compile：0 error；
- `BattleSimulationWorkerBoundaryEditorTests`：job
  `f6cfd5b83f264dc0ae3e9bcf612bbc30`，18/18 PASS；
- `BattleHitExecutionPlanEditorTests`：job
  `789f5e5544ba4fa7a9c8c43264a75844`，178/178 PASS；
- `BattleCentralLatestFrameMaterializationEditorTests`：job
  `4bb3792ae52d451d844cd8cfb6d695a1`，13/13 PASS；
- full `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-08-23 20:25:11 PASS；
- self-check故意触发的两条negative-path runtime-rest error已识别；清理后final Console error为0；
- `Tools/Validate-ChangeLedger.ps1`：PASS，82 records / 97 governed code files；
- scoped `git diff --check`：PASS；
- probe forbidden direct-call scan：0。

MCP stdio在Unity结果返回后偶发`BrokenResourceError`/disposed-client关闭噪声；各Unity test job均已返回
`status=succeeded`，该工具关闭噪声不计为测试失败。

## 6. Final classification

- `D-SCHED-009`：`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；
- `D-RENDER-002`：`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；
- 没有发现需要修改production gameplay/render的first-difference；
- `R8-HITWRITEBACK-001`只验证Editor-only witness本身，状态可标`VERIFIED`；
- R07B、R07C、R08仍未由本包执行或关闭。
