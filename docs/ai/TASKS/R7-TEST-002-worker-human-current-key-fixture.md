# R7-TEST-002 — worker human current-key fixture correction

> 日期：2026-08-23
> 状态：`VERIFIED / TEST-ONLY`

## Goal

只修正`DedicatedWorkerFullTickConsumesCanonicalHumanInput`对首次Left+Attack完整tick后的current-key过时预期，
使其符合C++ release `InputHandler::poll`和当前Unity production input lifetime。

## Scope

- `KeyLeft`与`KeyAttack`期望由0改为1；
- 注释改为“current key本tick保持，旧key进入Prev”；
- 保留Prev、cooldown、history、worker publication/ack与cleanup全部断言；
- 不修改任何production input、worker、driver、render或frame-advance代码。

## Authority / Evidence

- `J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp:1555-1620`：先把旧key写Prev，
  再从当前held state写`key_*`，新按下才写cooldown/history；
- 同文件由release Makefile列入构建；
- `D-TEST-002`既有fresh-domain first difference：expected0/actual1，production actual与C++ source一致。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs`
- 本Task/Change Record/Ledger/STATE/diff register/handoff与主计划。

## Deliverables

1. 两条current-key断言与一段注释修正；
2. exact test、worker boundary class、compile与fresh self-check证据；
3. `D-TEST-002`更新为test-contract closed。

## Verification

- exact test PASS且`KeyLeft=1`、`KeyAttack=1`、`PrevLeft=0`、`PrevAttack=0`；
- cooldown仍为5，history tail仍为4/9；
- worker boundary class无新失败；
- Unity compile 0 error、fresh-domain full self-check PASS、ledger/scoped diff PASS。

## Stop conditions

- production实际current key不是1；
- 修正需要改变input polling、frame lifetime、worker/driver或pass order；
- 发现C++ source与当前fixture输入含义不一致。

## Out of scope

- D-TEST-003 worker/central/ack联合夹具；
- D-TEST-001 static pollution；
- worker pipeline、catch-up、render或服务器逻辑。

## Result

- 只修改了两条current-key断言（0→1）与对应注释；
- exact job `86e6bddd257f4e18bb37433941f1a916`：1/1 PASS；
- worker boundary job `41f7b4803c754635b0d7c16abaf73754`：17/17 PASS；
- dotnet/Unity compile 0 error，2026-08-23 02:14:36 fresh-domain full self-check PASS；
- production脚本未修改；本`VERIFIED`只关闭stale test contract。
