# HANDOFF — R3-SYNC-RESP-01 physics-tail integer sync before respawn

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R3-SYNC-RESP-001`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、修改或写入 C++ authority。

## 已完成

- 完成 `D-MOV-003` 的 C++→Unity writer/reader contract：
  - C++ `frame_advance.cpp:25-48` 的 delay/link/kind2 F03 early return；
  - C++ `physics.cpp:326-342` 的 success-physics-tail integer sync；
  - C++ `game_tick.cpp:1280-1421` 的 F05 no-count respawn reader和先后关系；
  - Unity exact / shared character-DAT early-return path、global respawn sync和final respawn writer crosswalk。
- 先建立 Task Contract与 Change Record，再只删除
  `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 的 respawn-gate 前 global `SyncIntegerPosition()` loop。
- 未改变 `ApplyRespawnWithoutStoredCount` 的 eligibility、average、RNG、position writer或其 final sync；未改变
  per-physics tail sync、CPoint/link/held、weapon、stage、render、pool或任何 C++文件。
- 新增 full self-check joint fixture：exact delay/link/kind2 + shared character-DAT delay四个 stale-int readers，
  并断言 no-count respawn使用 `(40,50)` stale average和 `DeterministicRng(0x4E545344u)` offsets，而非 live doubles。

## 验证

| 检查 | 结果 |
|---|---|
| static target guard | `PostFrameAdvanceDeathCleanupAll` respawn前 global sync=false；respawn entity final sync=true。 |
| existing Unity Editor refresh/compile | UnityMCP port 6401，03:00:48 +08:00 expected domain reload/reconnect后 ready。 |
| full `BattleRuntimeSelfCheck` | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，2026-08-22 03:01:32 +08:00。 |
| Change Ledger | `Tools/Validate-ChangeLedger.ps1` PASS（13 records / 11 governed code files）。 |
| diff hygiene | `git diff --check` exit 0；仅有既有 LF/CRLF warning。 |

## 已记录的工具失败

第一次本地 MCP client错误使用已经不存在的
`I:\GitHub\Unity_GAS\unity-mcp\Server\src\unity_mcp_server\server.py`，因此 client未连接 Unity。已仅把
启动入口改为实际存在的 `Server\src\main.py` 后重试成功。该失败没有触发 Unity test、没有写 C++ authority，
也没有扩大脚本范围。

## 未关闭 / 不得夸大

- 未执行 real respawn Play Mode、输入组合 / stage asset / renderer验证。
- `R1-WP02` 的 C++ runtime trace仍为 `BLOCKED`；不运行 C++ executable。
- 未审计所有 cross-module direct-position writer；本包不以 global sync充当它们的补偿。
- 本 handoff不代表完整 R3 或完整 battle alignment。

## 下一步（按 D-009 连续推进）

开始 `R3-FRAME-02 / D-MOV-004/005` 的只读 reachability preflight：

1. 确认 Unity `ThrowFrameGuard` 是否存在 normal production nonnegative writer及其 runtime reachability；
2. 确认 C++ `state2000` facing contract与 Unity exact data-oriented FrameTick 的缺口，并审计现有 DAT/fixture
   是否能安全到达该 state；
3. 只在结论闭合后建立新的 Task Contract / Change Record；在此之前不改脚本。
