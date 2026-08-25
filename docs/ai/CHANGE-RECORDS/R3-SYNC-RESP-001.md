# R3-SYNC-RESP-001 — physics-tail integer sync before respawn

<!-- CHANGE-RECORD
id: R3-SYNC-RESP-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\frame_advance.cpp/physics.cpp and src\entity\game_tick.cpp release live physics-tail integer sync and respawn reader order
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-PREFLIGHT-VERIFIED / UNITY-COMPILE-PASS / FULL-SELF-CHECK-PASS / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / frame-advance / physics / respawn / integer-position / test  
> 所属 Work Package：`R3-SYNC-RESP-01`  
> 当前状态：`RUNTIME_PENDING` — 最小脚本写入、existing Unity Editor compile和full self-check 已通过；仍缺 Play Mode 与 C++ runtime trace。

## 1. 目标与允许范围

只移除 `PostFrameAdvanceDeathCleanupAll` 在 respawn gate前对所有 active entity执行的 global
`SyncIntegerPosition()`。允许脚本路径仅：

- `SimulationWorld.Passes.partial.cs`；
- `BattleRuntimeSelfCheck.cs` 的 early-return stale-int / respawn-average fixture。

成功 physics路径已有同步、respawn自身 position writer及其 final sync必须保持；不得修改 `NTSDEntityRuntime`、
任何 physics/weapon/CPoint/held/stage/renderer/AI/input/pool代码。

## 2. Authority / 差异依据

- **C++ VERIFIED**：`frame_advance.cpp:35-48` 的 delay/link/kind2 early return在
  `physics_update` 前；`physics.cpp:326-342` 只在 physics尾同步整数；`game_tick.cpp:1319-1340` 的
  no-count respawn平均读取 active same relation character-DAT 的 integer x/z，且发生在 first Z clamp前。
- **Unity VERIFIED**：exact/shared success dynamics都各自sync，但
  `SimulationWorld.Passes.partial.cs:656-661` 又将所有 active integer重算，使 early-return participant成为
  respawn average的错误 live-double值。
- **C++ runtime trace BLOCKED**：R1-WP02未解除；本包绝不运行C++ executable。

## 3. 实际修改 / 验收

1. 删除上述全体 sync loop；未动 `ApplyRespawnWithoutStoredCount` 的 field formula、RNG或 respawn writer final sync。
2. 新增 `CheckRespawnReadsPhysicsTailIntegerCoordinates`：一个 no-count respawn entity，四个同 relation
   character-DAT allies，覆盖 exact delay/link/kind2和shared delay。每个 ally 都先写 stale integer position，再写
   不同步的 live double position；`SerialTickAll(1)`后验证 early-return保留 stale ints，respawn再验证只按
   stale average与同一 `DeterministicRng(0x4E545344u)` offset写位置。
3. `PostFrameAdvanceDeathCleanupAll` 在 respawn gate前已无 global sync；respawn entity自身 final sync保留。

## 4. 实际验证证据

| 层级 | 命令 / 场景 | 实际结果 | 状态 |
|---|---|---|---|
| source/static | C++ F03→F04 tail sync→F05 respawn reader与Unity writer/reader crosswalk。 | four early-return writer contract、success physics tail以及 respawn reader均闭合；只确认本包所涉时点。 | `PASS` |
| first editor tool attempt | UnityMCP client以已废弃 `src\\unity_mcp_server\\server.py` 启动。 | 本地 client path不存在，未到达 Unity、未运行测试、未修改任何 Unity/C++内容；立即改为实际 `Server\\src\\main.py`。 | `TOOLING_FAIL → fixed` |
| final compile | UnityMCP `refresh_unity(force/scripts/compile)`。 | 03:00:48 +08:00 预期 domain reload/reconnect后 editor ready。 | `PASS` |
| full self-check | `NTSD/验证/运行战斗运行时自检`。 | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 03:01:32 +08:00；新 stale-int fixture由此入口执行。 | `PASS` |
| ledger / diff | `Tools/Validate-ChangeLedger.ps1`、`git diff --check`。 | ledger PASS（13 / 11）；diff check exit 0，只有既有 line-ending warning。 | `PASS` |
| Play Mode | real delayed/linked/caught respawn sequence。 | 本包未运行。 | `RUNTIME_PENDING` |
| C++ authority trace | R1-WP02。 | 不运行 C++ executable。 | `BLOCKED` |

## 5. 风险、停止与回滚

- 若任一 production character-DAT成功物理路径缺失自身 sync，停止并不以global sync补偿；另建更小的 writer Record。
- 若需要改 respawn formula、relation eligibility、cpoint/link或 stage，停止并拆包。
- 回滚只涉及本 Record两条代码路径与关联 docs；提交 hash：未提交。

## 6. 交接

- 继续时先读本 Record、`TASKS/R3-SYNC-RESP-01-integer-sync-respawn-contract.md`、
  `RESEARCH/R1-SOURCE-003-unity-crosswalk-and-diff.md` 与新 handoff。
- `D-MOV-003` 现在是 `RUNTIME_PENDING`；不能由 Unity fixture提升为 C++ runtime equality，且不得以本包为由
  处理 all direct-position writer。
- 依 `D-009` 连续进入 `R3-FRAME-02` 的**只读** reachability preflight（`D-MOV-004/005`）。必须先确认
  `ThrowFrameGuard` nonnegative production writer与 state2000 DAT reachability；在新 Task Contract / Change Record
  建立前不改脚本。
