# Task Contract — NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001

> 状态：`VERIFIED / PRODUCTION-C01-SINGLE-WRITER / PRESENTATION-READ-ONLY / REAL-PLAY-PASS`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C01`  
> 依赖：`NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001 / FOCUSED_TEST_PASS`  
> 建立日期：2026-09-04

## 目标

把精确native spark lifecycle接到每个实际Unity battle core tick的C01：input phase advance之后、当前
Cooldown之前。同步、dedicated-worker与`buildPresentation=false`必须共享同一调用；RenderDispatch U25
停止对logical hit records执行production finalize/no-publication writeback，使presentation只冻结/发布。

保留旧`FinalizePublishedHitRecordCycle`与`AdvanceHitRecordsWithoutPublication`方法作为历史测试/兼容入口，
但正式`NTSDBattleTickSystem`、同步Host、dedicated worker消费回调与production stress snapshot不得再调用
logical writeback。presentation publication binding改由只读acknowledgement释放。本包不修改spark resource
mapping、renderer或hit初始ID producer。

实施后的扩大调用点审计发现：`SimulationTickDriver.LateUpdate()`、worker
`BattleWorldSimulationTickExecutor.OnPresentationConsumed()`和stress final snapshot仍调用旧finalize；仅移除
TickSystem U25调用会让正式路径在presentation之后继续二次推进。因此在修改这些新增脚本前扩展本合同范围，
并把既有R8 writeback Play probe迁移为B3 C01只读presentation验收。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs`：只新增不改logical state的publication acknowledgement；不改旧compatibility API语义。
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`：同步LateUpdate与worker停止尾部只迁移到只读acknowledgement。
- `Assets/NTSD/Scripts/Simulation/Host/BattleSimulationWorkerBoundary.cs`：presentation consumed只迁移到只读acknowledgement。
- `Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs`：final parity snapshot只迁移到只读acknowledgement。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeSparkC01IntegrationEditorTests.cs`（新增）及`.meta`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs`：迁移为B3 C01 production Play witness，保留fixture/cleanup骨架。
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`：只迁移`R6-PRES-005`旧RenderDispatch写回断言为B3 C01逻辑生命周期断言；其他legacy compatibility覆盖保留。
- `Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs`：只更新phase name/count测试。
- 本Task/Record、Ledger、STATE、handoff、总表和B3 manifest。

禁止修改`BattlePresentationShadowBuild`旧API语义、LF2Entity primitive/hit producer、render catalog/materializer、
Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。

## 强制合同

- 新`NativeSparkAdvance` diagnostics phase追加enum末尾以保持既有phase数值稳定，但实际occurrence必须在
  `BattleFlow`之后、`Cooldown`之前。
- logical advance完全不读取presentation/resource catalog；空/不可用catalog也照常推进或tail-pop。
- 本tick在SimTU/hit阶段新增的record必须保持base ID到tick结束，不被U25推进。
- 现有record在C01恰好推进一次；RenderDispatch不得二次推进。
- worker/sync、buildPresentation true/false一致；presentation capture只读C01后及本tick新增记录的状态。
- 默认diagnostics disabled；不引入每tick分配。
- 旧manual finalize API暂不删除，避免在B3顺手重写B9测试面；必须明确不再是production caller。
- production acknowledgement只允许claim当前publication cycle并释放resource binding；不得读取/写入entity
  HitRecord。同步、worker consumed、worker stop与stress final snapshot均必须走该只读入口。

## Test-first验收

1. 新测试先要求phase/world C01 seam及new-hit same-tick base，取得预期red。
2. 更新实际sequence基线：full 31 occurrences、input-clear partial 6，首个旧差异关闭；下一首差为
   `expected CoreProducerSampleScan / actual Cooldown`（Cooldown仍属于尾部，后续B3处理）。
3. compile0；focused及presentation/snapshot/worker/phase相关测试通过。
4. full SelfCheck PASS、known negative复核、Console0。
5. 迁移后的actual kind0 collision Play probe覆盖published/no-publication、同步或worker正式入口、C01后新hit
   base ID、下一tick单次推进、presentation/late只读、零新增warm allocation violation和clean cleanup。
6. validator与`git diff --check`通过。

首轮SelfCheck必须保留为red证据：旧`R6-PRES-005`要求no-publication RenderDispatch按resource catalog
推进，和当前C01权威合同直接冲突。该red出现后、本Task在修改SelfCheck脚本前已把精确方法加入允许范围。

## 回滚

移除C01 world/tick caller与只读acknowledgement，恢复Host/worker/stress的compatibility finalize调用、
RenderDispatch production writeback与phase enum/tests；旧primitive仍可保留为上游focused core，不影响production。

## 完成裁决

上述6项验收全部完成。最终focused `38/38`、相关`98/98`、22:14:51 SelfCheck、真实kind0
Play tick3～6、Console0、零warm allocation增量与Scene pre/post SHA一致均有Record证据。下一B3首差为
`CoreProducerSampleScan`相对Unity `Cooldown`的位置，不在本Task继续扩大。
