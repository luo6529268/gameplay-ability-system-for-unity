# Handoff — R1-SOURCE-002 输入、组合键、AI 与逻辑帧边界源码合同

> 完成日期：2026-08-21  
> 状态：COMPLETED（静态 source contract）；不是 Unity gameplay 修复或 runtime 验收。  
> C++ authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 本次未启动 C++ executable、未运行 Unity/Play Mode/trace、未修改 Unity/C++ gameplay。

## 已完成

- 从 `game_tick.cpp` 的 callback gate 闭合到 `main.cpp` 实际 lambda：C++ 的
  post-cooldown input callback 先轮询 P1/P2，再在 `world.game_tick > 1` 时按升序对每个
  active character DAT 执行 AI prepare（如需）和 `apply_input`，之后才回到 T03 OID
  特殊维护。
- 读取并登记 C++ human key/prev/cooldown/history、AI target scan/tie、combo/direct
  sequence、PP/HP frame jump、F1/F2 gate 的 source contract。
- 建立 Unity `FrameInputSet` / `SimInputBuffer` / HumanInput / CharacterInput / AI / ECS
  writer / slot traversal 的 crosswalk。
- 将 `D-SCHED-005` 和 `D-SCHED-010` 从 UNKNOWN 升级为“待处理（静态差异已确认）”。
- 新建 `D-INP-001`～`D-INP-006`；其中 negative link gate 与 dead-AI prefilter 是已确认
  的静态分支差异，packet edge、physical binding、optimized AI 路径保持待测试或 UNKNOWN。

## 新增交付物

- `docs/ai/RESEARCH/R1-SOURCE-002-input-contract.md`
- `docs/ai/RESEARCH/R1-SOURCE-002-unity-input-crosswalk-and-diff.md`
- `docs/ai/TASKS/R1-SOURCE-002-input-contract.md`（状态更新）
- `docs/ai/RESEARCH/R1-SOURCE-001-unity-crosswalk-and-diff-inventory.md`（D-SCHED-005/010）

## 关键规则（供后续 Source / R3 使用）

1. 不能把 `NeedClearInput` 当成 C++ F1/F2 slow gate；两者早退位置和副作用不同。
2. 不能将 Unity human polling 与 `CharacterInputAll` 之间插入的 OID maintenance 当作
   无害拆分；C++ callback 内完整 input 在 T03 之前结束。
3. 不能先移除 `LinkState < 0` / HP<=0 input gate；先闭合 held/death/lifecycle source
   contract 并定义 tick fixture。
4. `FrameInputSet`、worker、SoA/ECS、CentralOnly、Texture2DArray、MobileExtended 与
   DesktopExtended 均是不可回退 Unity 实现边界；R3 只可做最小 adapter 修复。
5. 任一未来脚本改动必须先创建独立 Change Record 并执行 validator；当前没有 active
   gameplay Change ID。

## 推荐下一步

继续 **R1-SOURCE-003**：只读闭合 C++ `frame_advance.cpp`、`physics.cpp`、state/frame
dispatch、移动、落地、double/int 写回和 death/respawn 边界，并在 Unity 中定位对应
`FrameLogicBeforeAdvance`、`FrameAdvance`、`DeathCleanup`、两次 Z clamp 和 late update。
不要进入 R2/R3 gameplay 修改。
