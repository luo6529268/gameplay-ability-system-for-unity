# HANDOFF — R3-LAND-01 character landing raw-frame writer

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R3-LAND-001`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、修改或写入 C++ authority。

## 已完成

- 完成 `D-MOV-002` 的只读 preflight：
  - C++ `physics.cpp:153-223` 的 F04 landing direct-frame matrix；
  - C++ `game_tick.cpp:1247-1276,1645-1655,577-587` 的 F04→candidate→F07 order；
  - C++ `frame_advance.cpp:847-855,995` 的 late attacking / wait-counter tail；
  - Unity exact character 与 shared-character-DAT writer/consumer crosswalk。
- 建立 Task Contract、`R3-LAND-001` Change Record、ledger entry和本 handoff，后再改脚本。
- exact `LF2Character` 和 shared-character-DAT landing paths均改为 raw target-frame write：
  - state13 high、state12 high、state18不在 F04 清 attacking；
  - state12 low和ordinary landing在 F04清 attacking；
  - target DAT wait/next同步，但 PN及wait counter保持。
- 新增 full self-check中的16-case F04-only fixture：exact/shared各覆盖 state12 low front/back、state12 high、
  state18、state13 high、ordinary state100、ordinary frame212和ordinary default。

## 首次失败及处置

首次 Unity scripts refresh报 `LF2Entity.cs(5569,25) CS0136`：内层 `landingFrame` 与同方法外层局部变量
重名。仅改名为 `fallingLandingFrame`，没有扩大脚本范围、改变算法或回退任何已有改动。

## 验证

| 检查 | 结果 |
|---|---|
| static target blocks | `ImmediateFrame=false`；resolver falling-ground raw count=3（另有 frozen raw=1），shared raw count=4。 |
| existing Unity Editor refresh/compile | 02:41:50 expected domain reload/reconnect后 ready。 |
| full `BattleRuntimeSelfCheck` | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，02:42:40 +08:00。 |
| Change Ledger | `Tools/Validate-ChangeLedger.ps1` PASS（12 records / 11 governed code files）。 |
| diff hygiene | `git diff --check` exit 0；仅有已存在 LF/CRLF warning。 |

## 未关闭 / 不得夸大

- 本包未做 physical Play Mode、真实技能落地/表现、C++ executable trace或 legacy non-character landing fallback。
- R1-WP02 full C++ trace继续 `BLOCKED`。
- 这不是完整 R3 或完整 battle alignment证书。

## 下一步（按 D-009 连续推进）

进入 `R3-SYNC-RESP-01 / D-MOV-003` 的只读 preflight：只审计 C++ `physics.cpp` 成功 physics tail 的 integer
sync、Unity `PostFrameAdvanceDeathCleanupAll` 的全体 sync，以及 respawn coordinate readers。先闭合 writer/consumer
和最小 fixture；在新 Task Contract / Change Record建立前不修改代码。
