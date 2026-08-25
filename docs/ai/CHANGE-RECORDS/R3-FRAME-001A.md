# R3-FRAME-001A — current-key lifetime through frame advance

<!-- CHANGE-RECORD
id: R3-FRAME-001A
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp and src\entity\game_tick.cpp/frame_advance.cpp release live current-key lifecycle
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-COMPILE-PASS / FULL-SELF-CHECK-PASS / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / input / frame / test  
> 所属 Work Package：`R3-FRAME-01A`

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`；source/static、Unity scripts compile和full self-check均通过；仍缺
  physical Play Mode/集成与 C++ runtime trace。
- 目标：只关闭 `D-MOV-001`：撤销 Unity 在 frame advance 前的 generic current-key clear，使本 tick key
  保留到 C++ F03/F09 consumer。
- 允许脚本路径：
  - `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`；
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`（仅更正旧语义注释）；
  - `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。
- 不属于本次范围：`D-MOV-002/003/004/005`、human/AI producer算法、InputAction asset、scene/DAT、
  held/link/CPoint/collision/hit/opoint/render、worker/SoA/profile、pool/capacity、C++ runtime。
- 关联 Change ID：`R3-INP-003A-001`、`R3-HOLD-INP-001`、`R3-AI-LIFE-001`、`R3-AI-TGT-001`只提供
  相邻输入契约，不授权扩大它们的范围。

## 2. Authority / 需求依据

- **C++ release live path**：
  - `src/entity/game_tick.cpp:1002-1005,1247-1276`：post-cooldown input在 frame advance 前；
  - `src/input/input_handler.cpp:1555-1613`：human poll滚动 prev、写 current held、更新 edge/history；
  - `src/input/input_handler.cpp:1615-1624`：AI仅在自身 producer内 roll/clear后重写；
  - `src/entity/frame_advance.cpp:80-83,941-951,977-980`：F03/F09仍读 current key。
- **Unity 原状**：`SimulationWorld.Passes.partial.cs:599-612` 在 input pass完成之后、F03之前统一调用
  `BattleCharacterInputWriter.ClearCurrentKeys`，清空 input-store及Runtime mirror。
- Evidence 等级：C++ release source contract `VERIFIED`；Unity source crosswalk `VERIFIED`；C++ executable
  trace `BLOCKED`；实际 Unity script验证 `PENDING`。

## 3. Unity 原状与已确认差异

1. `NTSDBattleTickSystem.RunTick` 当前执行 human input → CharacterInput → OID maintenance → early/frame logic
   → `SerialTickAll`；所以 `SerialTickAll` 的 clear 删除的是刚由本 tick producer写入的状态。
2. human `InputHandler::poll` 的等价 Unity path由 `RunHumanInputPollPhase` 和 `NTSDInputStateModule`保留
   held state；AI path已经在 `PrepareAiInputBasic*` 内自己的 `RollAndClearAiKeys` 做 producer-local roll/clear。
3. 旧 `BattleRuntimeSelfCheck` 中 GT-02/GT-03以及 complete-held fixture错误地将历史 C# clear行为写成
   authority。这些断言本身必须随着 C++ authority迁移而改为 C++ source contract，不能把旧 test当作裁决。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `SimulationWorld.Passes.partial.cs` | `SerialTickAll` | 每实体 F03前清 current key。 | 不触碰 current key；F03/F09读取本 tick producer输出。 |
| `LF2Character.cs` | `UpdateLocalInputStateFromControllerBuffer` 注释 | 声称 frame advance clear key符合 authority。 | 说明 held key保留到本 tick consumer，下 tick由human poll或AI producer管理滚动。 |
| `BattleRuntimeSelfCheck.cs` | full-held / GT-02 / GT-03 fixtures | 证明旧 C# clear语义。 | 证明C++ current-key lifetime、edge/history非重复、212 direction override和entry-clear保持。 |

## 5. 不可回退边界

- 中央表现 / `CentralOnly` / Texture2DArray / 动态 Mesh / URP：不触碰；
- `Authority400`、`MobileExtended`、`DesktopExtended`：不触碰；
- 30 Hz、`FrameInputSet`、slot/generation、SoA/ECS、对象池、worker、0 GC：不改变；
- `NeedClearInput` 的 battle-entry clear仍然是独立 branch，不被删除或扩展到非-character；
- AI的 `RollAndClearAiKeys` 保持 producer-local，不能用本 Record重写AI policy或cache。

## 6. 实际改动

已写入的最小改动：

- `SimulationWorld.SerialTickAll` 不再在 F03 前调用 generic `ClearCurrentKeys`；
- `LF2Character` 的 local-input 注释改为 C++ current-key lifecycle；
- `BattleRuntimeSelfCheck` 更新 complete-held、GT-02、GT-03 和 AUDIT6 local-held断言，并新增 retained-key
  helper / probe字段命名，使旧 C# clear expectation不能继续反向裁决 C++ contract。

首次 self-check（02:18:33）发现并记录一处遗留旧断言：`CheckAudit6InputPhaseOrder` 仍要求 serial
frame advance清 `localHeld.Runtime.KeyLeft`。这与本包 C++ source contract矛盾，已在同一 test-only
fixture中更正。后续完整 self-check于 02:22:20 PASS；此处不是 production first difference。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| source/static | C++ input/game_tick/frame_advance 与 Unity scheduler/input writer crosswalk。 | 已完成 preflight。 | `PASS` |
| focused self-check | held input、GT-02 transit、GT-03 frame212、NeedClearInput fixture。 | 02:18:33 first-difference只命中旧 AUDIT6 clear assertion；更正后由 full self-check覆盖。 | `PASS` |
| Unity compile | existing Editor UnityMCP scripts refresh / filtered `error CS`。 | 02:21 force scripts refresh成功（预期domain reload/reconnect后ready）；filtered `error CS`=0。 | `PASS` |
| full self-check | `NTSD/验证/运行战斗运行时自检`。 | 02:22:20 `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`。 | `PASS` |
| Play Mode | actual W/S/A/D/J/K/L、walk/run/jump/turn。 | 用户资产/Play Mode边界，R3-PHY-01。 | `RUNTIME_PENDING` |
| C++ authority trace | R1-WP02。 | 不运行C++ executable。 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 已知风险：旧 Unity self-check和local input comment曾把清 key视为正确；本包已更新已发现的四类
  fixture，但未来新增的历史夹具必须继续以 C++ source contract 而非旧 C# expectation审核。
- 已知风险：没有 generic clear 后，controllerless/test-only entity保留 current key是符合 C++当前字段
  生命周期的；不能因其与历史Unity fixture不同而重新加入全局清除。
- 未关闭项：physical binding、真实场景walk/run/jump、AI/held/collision joint path和C++ runtime trace。
- 回滚方式：只恢复本 Record的三处脚本 diff和对应自检断言；不得回退任何已有输入、AI或scheduler Record。

## 9. Git / 交接

- 修改前工作树基线：存在用户/历史的未提交 scene、settings、resource、docs和脚本改动；不得覆盖、回退或
  归属给本 Record。
- 实际脚本 diff 范围：仅本 Record第1节的三条路径。
- 提交 hash：未提交。
- `Tools/Validate-ChangeLedger.ps1`：2026-08-22 final PASS（11 records / 10 governed code files）。
- 交接需优先阅读：本 Record、`TASKS/R3-FRAME-01A-current-key-lifetime-contract.md`、
  `RESEARCH/R1-SOURCE-003-*.md`、`D-012`。
