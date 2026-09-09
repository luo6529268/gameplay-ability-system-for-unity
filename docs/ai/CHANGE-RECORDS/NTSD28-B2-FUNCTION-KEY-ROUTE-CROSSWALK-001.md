# NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001 — F1～F12 路由与效果归属交叉

<!-- CHANGE-RECORD
id: NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan playable live native function-key route, host transition and GameSession command closure.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-HEADER-MAIN-GAMESESSION-LIVE-CLOSURE / FULL-F1-F12-MATRIX / NINE-STAGE-ROUTE-PRIORITY / REPEAT-AND-MAINTENANCE-ORDER / CONTINUOUS-F11-F12 / SESSION-MASK-0XF4 / FIXED-F3-F6-F7-F8-F9-DISPATCH / SAME-WINDOW-F9-WINS / F7-MP-ONLY-DIFFERENCE / UNITY-SPLIT-HOST-AND-LEGACY-F789 / IMPLEMENTATION-SPLIT-DEFINED / NEXT-ROUTE-CONTRACT / LEDGER-VALIDATOR-153-104-PASS / AUTHORITY-READ-ONLY / NO-PRODUCTION-CHANGE
-->

> 状态：`VERIFIED / FULL-F1-F12-CROSSWALK / IMPLEMENTATION-SPLIT-DEFINED / NEXT-ROUTE-CONTRACT`

## 计划

- 从纯 route 层向外追到 Win32 physical edge、Host transition、GameSession queue/session acceptance 与 tick 尾部消费。
- 按 F1～F12 逐键对照 Unity，分离 route/carrier、已存在 B1 Host owner 与 B3/B8/B10/B11 下游效果。
- 冻结 exact priority、event mask/order、reset 与 snapshot/checksum 要求，然后输出最小实施拆包。

## 当前事实

- Authority 的 F1～F12 不是一套同质事件：F1/F2/F4/F5 为 one-shot Host，F3/F6～F9 为 Session，F10 no-op，
  F11/F12 为持续 Host；Ctrl+F9/F10 是 recording maintenance。
- Unity 当前 F1/F2/F5 已由 B1 Host latch 生产接线；旧 FunctionKey latch 仅处理 allow-listed F7/F8/F9，且 F7/F8/F9
  语义与当前 Authority 不完全相同。
- 本包只产出证据与实施路由，不修改 production。

## 审计结果

- F1～F12的disposition/command、repeat/context priority、Session二次gate、mask `0xF4`、固定
  F3→F6→F7→F8→F9 dispatch与post-tick tail均已逐项闭合到manifest。
- 当前formal main对main-state/global-delay route传常量true，Session会动态重检BattleFlow/mode但global-delay仍传true；
  因而保留pure gate，不虚报dynamic delay rejection为正式可观察事实。
- Unity旧F7是HP3/HPBound/HP/PP全500且清exit countdown；Authority F7是simulation tick后只写active current MP500。
  该可观察差异必须移除旧owner后才能验收。
- B1 F1/F2/F5只需统一route adapter；F3/F6～F9需要B2 carrier；F4/F7～F9效果归B8，F6 consumer归B5，
  F11/F12归B10。下一包只实现pure route contract。

## Git / 交接

- production 代码：零修改。
- Authority：只读。
- validator：`PASSED / Records 153 / governed code files 104`；既有历史warning不影响PASS。
