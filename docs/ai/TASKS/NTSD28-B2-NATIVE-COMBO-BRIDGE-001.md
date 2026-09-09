# Task Contract — NTSD28-B2-NATIVE-COMBO-BRIDGE-001

> 状态：`FOCUSED_TEST_PASS / COMBO10_CORE_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

逐分支移植NTSD 2.8 `process_sampled_inputs/advance_combos/clear_combo_attempt`的十状态组合键核心，
固定exact block的逻辑键、物理edge byte与Unity历史交叉命名映射，并提供只覆盖已证明字段的legacy
action-consumer projection。本包建立可验证核心和桥，不接production调用点。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeComboStateMachine.cs`（新增）及meta
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboStateMachineEditorTests.cs`（新增）及meta
- 本 Task/Change/Ledger/STATE/handoff/总表

## 不变量

- exact logical current/previous顺序固定`W,S,A,D,J,K,L`；edge物理顺序固定`J,K,L,D,A,W,S`。
- rising-edge处理顺序固定`D,A,W,S,L,K,J`；history code固定`J=5,K=0,L=9,S=2,A=4,D=6,W=8`。
- first six directional combo、J-K/J-L/L-K-J/K-L分支优先级、same-sample fallthrough、early terminal保留、
  proxy tail清理与clear-attempt保留S edge必须逐项一致。
- legacy projection只映射`hit_Fa/Fj/Ua/Uj/Da/Dj/ja`已存在consumer；不得把native `hit_aj/ad/jd`
  冒充`ComboDja`或其他旧字段。
- 本包不改AI producer、human poll、input proxy、frame action resolver、DAT parser或内容。

## 验收

1. test-first只因新类型缺失而红；
2. 全键同sample的mask、edge物理槽、history顺序与衰减精确；
3. directional六状态机、四个history combo和branch priority精确；
4. clear-attempt与proxy tail边界精确；
5. legacy current/previous/edge/combo projection只覆盖明确映射；
6. warm process/projection零托管分配，focused、compile、相关proxy/carrier回归、SelfCheck、Console、Ledger、diff通过。

## 回滚

删除两个新增脚本及meta并移除本包治理记录；保留carrier和既有legacy输入路径。

## 实施与验证结果

- test-first：Unity仅产生10个预期`CS0103`，均指向缺失的新state-machine类型。
- 实施：逐分支移植edge decay、defend re-entry decay、七键native rising order、五项history、
  六个directional combo、四个history combo、ordered early-return、proxy-tail miss clear和
  clear-attempt边界；提供exact→legacy current/previous/edge及已证明combo字段projection。
- 明确未映射：native `hit_aj`、`hit_ad`、`hit_jd`不会写入任何旧`ComboD*`；只将native
  `L,K,J/hit_ja`投影到旧`ComboDja`。
- 首轮focused job `fe9d44957ce547fd98b55073b7ad4ed5`：19/19。
- 补齐jump-edge suppression与三项禁止别名断言后，final combo/proxy/carrier job
  `aaabe5a774ef496e97fda7f1e0d56c0a`：31/31。
- 完整`BattleRuntimeSelfCheck`：2026-09-03 06:50:53 `PASS`；清除预期负向夹具后Console Error 0。
- 未验收：production human/AI sampling、proxy copy、native route field消费与combo-attempt回写清理。
