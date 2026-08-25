# R8-JOINTINPUT-PROBE-002 — bounded synthetic physical-input pulse delivery

<!-- CHANGE-RECORD
id: R8-JOINTINPUT-PROBE-002
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePhysicalMovementPlayModeProbeEditor.cs
authority: USER-APPROVED-R8-WP01G-R03 / test-only evidence repeatability
evidence: CURRENT-TEMP-DDJ-FAIL-20260823-172233 / CURRENT-TEMP-F2-FAIL-20260823-172344 / FRESH-F2-DDJ-DRA-PASS / EDITMODE-257-257 / SELF-CHECK-PASS-173315 / LEDGER-PASS
-->

> 创建日期：2026-08-23  
> 当前状态：`VERIFIED / TEST-ONLY`

## Requirement

R03曾取得DDJ/DRA/F2 PASS，但fresh Play重跑中一次性合成物理键没有进入canonical `FrameInputSet`，
当前Temp报告被覆盖为FAIL。两个不同首键L/D均失败，而production脚本未变化，说明证据探针的一次性事件
与Editor/InputSystem采样边界不具备可重复性；不能保留互相矛盾的最终证据。

## Planned change

- 两个Editor-only probe在等待canonical edge时使用最多8次的release→press物理状态脉冲；
- 每个阶段确认edge后立即停止重试；
- 输出attempt计数；
- 不改production，不手工调用InputSystem update，不直接写runtime/buffer/frame/motion。

## Protected boundaries

C++只读；不改InputAction asset、30Hz、FrameInputSet producer、worker、DAT、render、capacity、T8、
IL2CPP、Android或服务器。

## Acceptance

见Task合同。DDJ、DRA、D/K三份fresh当前报告必须同时PASS，随后compile/focused/self-check/validator收口。

## Rollback

只回退本Record在两个Editor probe中增加的有限脉冲与attempt字段；保留历史失败/通过证据，不触碰production。

## Actual changes / verification

- 两个probe均增加每阶段最多8次press attempt；
- 未见canonical edge时按逻辑tick交替queue release state与相同physical press state；
- combo probe覆盖L、第二方向、第三动作三段；movement probe覆盖D与D+K两段；
- edge一旦进入FrameInputSet/runtime即停止该阶段重试；
- 报告新增每阶段attempt计数；未调用InputSystem.Update，未写runtime/buffer/frame/motion；
- force all refresh/domain reload后fresh compile为0 error；Console仅1条MCP disposed-object工具噪声，无project error；
- fresh Play当前三份报告均PASS：
  - D/K movement：Right attempt2、K attempt1，tick1049/1053/1057/1077完成right/jump/air/land；
  - DDJ：L/S/K attempt均1，tick1603/1604/1616到frame271，object8→peak20；
  - DRA：L/D/J attempt均1，tick2225/2226/2238到frame263，object9→peak10；
- 当前Temp报告已全部由fresh PASS覆盖，不再与R03文档矛盾；
- 首次8类focused job `8bb8691298a04390b011410985761424`为256/257，唯一W05B generation断言失败；
  本Change未触碰W05/slot/pool。W05隔离job `b65c95e2443a42988f4aeb5fd0dd8ce4`为8/8 PASS，
  随后同8类fresh job `bf16f84db0b346809407bfe7a01dbc83`为257/257 PASS，确认瞬时测试隔离残留；
- full `BattleRuntimeSelfCheck`：2026-08-23 17:33:15 PASS；
- 清理self-check预期负路径诊断后Console error=0；
- `Tools/Validate-ChangeLedger.ps1` PASS：79 records / 93 governed code files，两个脚本均由本Record覆盖；
- scoped code diff/whitespace PASS；静态搜索确认未调用InputSystem.Update、未写runtime/input buffer。
