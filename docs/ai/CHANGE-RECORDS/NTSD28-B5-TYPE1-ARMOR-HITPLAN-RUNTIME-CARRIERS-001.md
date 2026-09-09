# NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001
status: VERIFIED
change-kind: TEST_FIRST_DIAGNOSTIC_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorHitPlanRuntimeCarriersEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28 type1 armor activation writes runtime armor HP and input HP/MP consumption before reduced-hit/fallback; EXE B1E13AE1, closure 39DDDA15.
evidence: red bad15cd869c44364b063b462192132a8 4/4 expected failures; focused e4a622735c0847abb8324c79c33067fe 4/4 pass; B5 caf23965d9c14de0a6c46c465c5490f4 343/343 pass; NTSD28 broad 8a61484a2e6e4be793969b92be3dc2ac 808/808 pass; BattleRuntimeSelfCheck PASS 2026-09-06T01:57:16Z; Console only 7 intentional rest-binding negative-path errors; scene baseline D4266C6D...583B unchanged; ledger PASS.
-->

> 状态：`VERIFIED / HITPLAN_CARRIERS_READY / ARMOR_TRANSACTION_UNCONNECTED`

原子生产审计确认HitPlan目前可观察target HP/PP/bdefend，但缺runtime armor HP及input HP/MP消费累计。
本包只补capture/compare载体，不改变任何命中结果。回滚删除三个snapshot field/capture/compare条件及测试/meta。

## 实际修改

- `WriterEffectSnapshot`新增`TargetRuntimeArmorHp`、`TargetInputHpConsumedTotal`、
  `TargetInputMpConsumedTotal`。
- `CaptureWriterEffectSnapshot(...)`从target逻辑`Runtime`读取三个值；null target保持`int.MinValue`。
- `DifferenceMask(...)`把三个字段归入既有target vitals/resource bit 53；任一字段不等即fail closed。
- focused test以反射同时验证私有snapshot schema、capture值、null sentinel与逐字段difference。

## 验证结果

- test-first red：`bad15cd869c44364b063b462192132a8`，4/4按预期失败（字段缺失）。
- focused：`e4a622735c0847abb8324c79c33067fe`，4/4通过。
- B5：`caf23965d9c14de0a6c46c465c5490f4`，343/343通过。
- NTSD28 broad：`8a61484a2e6e4be793969b92be3dc2ac`，808/808通过。
- `BattleRuntimeSelfCheck`：`PASS`，结果时间`2026-09-06T01:57:16Z`。
- Console：仅7条self-check故意触发的rest-binding负路径错误，无编译错误。
- `NTSD_Battle.unity`：SHA-256仍为`D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`，
  长度205625、mtime `2026-09-05T15:44:52.7794120Z`均未变。
- Ledger：PASS，Records 295、governed code diff 258。

## 验证异常说明

一次错误过滤的job `7575d374b62e48b2bc63ab07e9825f15`误跑2504个全项目测试并暴露既存/任务外失败；
它既未发现本新增测试，也未作为本包通过或失败证据。随后强制refresh并用4个exact `testNames`取得上述有效red/green。

## 剩余边界

本包没有对projection写这三个值，因而没有接入护甲事务；下一独立包必须原子完成
ordinary defense→type1 match→activation→reduced/unarmored fallback→break，并同步actual与HitPlan projection。
