# HANDOFF — R3-FRAME-01A current-key lifetime

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change Record：`R3-FRAME-001A / RUNTIME_PENDING`

## 已完成的最小闭环

- C++ release `InputHandler::poll` / `prepare_ai_input`、`game_tick(...)` input→frame-advance顺序和
  `frame_advance.cpp` F03/F09 current-key consumers已闭合；C++ source明确要求本 tick current key在
  non-character dvz、frame212 jump-init和MP turn consumer时仍可读。
- Unity `SimulationWorld.SerialTickAll` 的 generic pre-F03 `ClearCurrentKeys` 已移除。human poll、AI
  producer-local `RollAndClearAiKeys` 和 `NeedClearInput` battle-entry clear均保留原职责。
- `LF2Character` 旧的“frame advance clear key符合 authority”注释已更正。
- `BattleRuntimeSelfCheck` 已覆盖并通过：
  - complete held-left只生成一次edge/history、current key保持、cooldown自然递减；
  - GT-02 transit看见完整current key和preserved prev；
  - GT-03 frame212读取right/up并写入`jump_distance/-jump_distancez`，无方向时保留已有Vx/Vz；
  - GT-01 `NeedClearInput`仍只清character input并early-return。

## 实际验证证据

- UnityMCP existing Editor：`refresh_unity(force/scripts/compile)` 于约 02:21成功完成预期domain reload/reconnect；
  `read_console(error CS)` 返回 0 条。
- full self-check：`NTSD/验证/运行战斗运行时自检` 后，
  `Temp/NTSD_BattleRuntimeSelfCheck.result` 在 **2026-08-22 02:22:20 +08:00** 为 `PASS`。
- `Tools/Validate-ChangeLedger.ps1`：PASS（11 records / 10 governed code files）。
- `git diff --check`：exit 0；仅有工作树既有 LF/CRLF warning。

## 必须保留的 first-difference 历史

第一次 full self-check（02:18:33）失败于：

`AUDIT6-01: C# authority frame advance must clear the current local input key`

根因是 `CheckAudit6InputPhaseOrder` 仍保留旧 C# baseline assertion；它在本包的 source contract下是
错误的 test expectation，而不是生产 input/movement异常。只在同一 test fixture把期望改为
`KeyLeft=1 / PrevLeft=0 / CdLeft=5` 后重跑即 PASS。不要把这次失败记为“已发现并修复新的生产 bug”。

## 未关闭项 / 不可扩大结论

- 本条是 `RUNTIME_PENDING`，不是完整 C++ battle alignment。
- `R3-PHY-01` 的实际 InputAction / Inspector / W-S-A-D-J-K-L Play Mode仍由用户场景证据关闭；没有改动
  physical binding。
- 真实 walk/run/jump/MP-turn、AI/held/collision joint Play Mode仍未由本包执行。
- R1-WP02 C++ full trace仍 BLOCKED；本包没有运行、修改、构建或写入 C++ authority。
- D-MOV-002 landing raw frame writer、D-MOV-003 respawn integer sync、D-MOV-004/005均未修改。

## 连续下一步

按 D-012，下一项先进入 `R3-LAND-01 / D-MOV-002` 的只读 writer/consumer preflight：将 C++ physics landing
branch按object/type划分，逐一对照 Unity frame、PN、Attacking、FrameWaitCounter、Transistor和后续R4/R5 consumer。
未建立新的 Task Contract和Change Record前，不改任何 landing或raw-frame脚本。之后才决定其能否独立实施，或必须
等待R4/R5的字段消费者合同。
