# Task Contract — NTSD28-B2-AI-HELD-RNG-001

> 状态：`FOCUSED_TEST_PASS / HELD_SITES_28_35_READY / ALL_40_LIVE_IDS_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

闭合 NTSD 2.8-Logan `step_held_object` 的 synchronized candidate 路径与 `0x28..0x35`
全部 14 个 live 表达式，使 valid held 分支不再调用 legacy-only `Rand(int)`；同步模式同时修正
line obstruction 所有权、ordinary/150 weapon 阈值、weapon-run state17/render phase 与 combo index4/5。

## Authority

- `source/ntsd28_core/src/simulation/native_ai.cpp::NativeAi28::step_held_object(...)`
  line 2037..2309，进入正式 playable `step_main→step_ordinary_combat` 调用闭包。
- 初始 `0x28>0` 返回 native 0 并停止 outer AI；其他早停分支与最终 direct/combo 分支的返回值必须保留。
- line blocker 必须是 subject 自身 nonzero battle group 的前20 slot active entity；selected target无需同组。
- ordinary weapon predicted X `<115`、Z `<6`；150/151 weapon predicted X `<300`、Z `<6`。
- weapon-run locked table只含122/123；state17门读取 subject `render_phase_008`，Unity snapshot对应 `Y`，
  不是 `HitStop`。
- `0x35` 未走 direct attack 时，selected在右置 native combo index4/hit_Da，即 legacy `ComboDda`；
  selected在左/同位置 index5/hit_Dj，即 legacy `ComboDdj`。
- 正式 playable `global_direction_lock_0049f608`保持默认0；同步 held 右移分支不得使用 Unity `MoveMode`。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiHeldRandomEditorTests.cs` 及 `.meta`
- 本 Task、Change Record、Ledger、STATE、handoff、总表与 B2 AI RNG manifest

禁止修改 Config/DAT/Scene/Prefab/ProjectSettings、authority、legacy CRT held 行为、production cursor
capture/commit、input combo producer/router 或 B3/B5/B6 行为。

## 不变量

- invalid linked slot/entity 不进入 authority helper且不消费 `0x28`。
- `0x29`只在state2；`0x2B`只在linked oid124；`0x2D`只在150/151且无遮挡和精确范围。
- weapon-run各边界/近距/state2分支按 authority 早停；`0x34`只在这些分支均未命中后消费。
- `0x35`只在local_flag30==0、self oid2/34、MP>150且`0x34==0`后消费。
- legacy `KeyJump=attack`、`KeyDefend=jump`、`KeyAttack=defend`桥不变。

## 验收

- test-first 捕获缺 native held helper 的预期编译失败；
- focused 覆盖 invalid link、0x28 early-stop、enemy target + ally blocker、115/300阈值、
  2E/2F与30/31边界、32/33近距、34/35短路、combo index4/5、state17 Y gate和完整顺序；
- 4096 zero-allocation、全 AI 回归、compile0、SelfCheck、Console0、Ledger PASS；
- production cursor仍不连接，下一包进入 authoritative-only capture/commit。

## 回滚

移除同步 held helper/分派和专用测试，恢复 synchronized held 路径 fail closed；legacy production不变。

## 结果

- test-first：1 个预期 `ProcessNativeHeld` 缺口，无任务外编译错误；
- focused：19/19，job `feaa950af1a446d4b9a4008141eeb18a`；
- 全 AI sensing/decision/character/shadow：361/361，job
  `323e390959514d0dbe07153e64109f87`；
- full candidate 路径实际得到 `14→3C→1C→28`并按原生 early-return 落到`HeldDecision`；
- 14 个 held expression / `0x28..0x35`全部有条件、顺序和动作断言；
- 4096 warm evaluation+commit 为 0 managed allocation；
- SelfCheck 2026-09-03 10:56:19 PASS，清理预期负例后 Console 0 error。
- Change Ledger validator PASS：126 records / 75 governed code files。

所有40个可能 live AI synchronized call-site ID现已具备candidate实现；production capture/commit、
输入生产者迁移与同seed/input/tick joint trace仍未完成。

## 2026-09-05 current-Authority binding correction

后续`NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001`以Y/phase互异的双端raw trace证明：Authority `render_phase_008`唯一绑定Unity `NTSDEntityRuntime.HitStop`，不是Y。本文当时“Unity snapshot对应Y”的判断只保留为历史实现事实，不再具有绑定裁决权；AI中仍读取Y的对应native render-phase consumer必须由后续consumer integration包纠正。
