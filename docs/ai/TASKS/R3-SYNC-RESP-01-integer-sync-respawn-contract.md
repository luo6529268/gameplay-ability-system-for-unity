# R3-SYNC-RESP-01 — physics-tail integer position sync before respawn

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、static、existing Unity Editor scripts refresh/compile 和 full self-check 已通过；仍缺 C++ runtime trace 与 Play Mode。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 对应差异：`D-MOV-003`。

## Goal

只关闭 respawn 前 integer position 的时点差异：C++ 只在成功进入 `physics_update(...)` 的尾部把
`x/y/z` 写入 `x_int/y_int/z_int`；frame-delay、negative link、cpoint-kind2 的 F03 early return 不执行该写。
Unity `PostFrameAdvanceDeathCleanupAll` 却在 respawn scan 前对所有 active entity 无条件
`Runtime.SyncIntegerPosition()`，使 C++ 应读取的旧整数坐标被提前覆盖。

本包只移除该 Unity-only global sync；成功物理路径各自已有的 tail sync、respawn writer的显式坐标写入和
respawn 完成后的最终 sync 都必须保留。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`
   - `PostFrameAdvanceDeathCleanupAll` 起始的全体 active `SyncIntegerPosition()` loop。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 新增一个最小 joint fixture，验证 exact character / shared-character-DAT 的 F03 early return 不会被
     respawn前的global sync改写，并验证 no-stored-count respawn仍按同 relation、current character-DAT 的
     **stale integer** x/z 求平均和消耗 RNG。

禁止：

- 修改任何 per-physics `SyncIntegerPosition`、`CharacterMechanics`、weapon/native physics、stage clamp、
  `ApplyRespawnWithoutStoredCount` / `ApplyRespawnFromStoredCount` 字段公式、RNG、CPoint/held/link writer、
  object pool、render、scene/DAT；
- 处理 `D-MOV-004/005`、R4+ 或把此 package 扩为一般 position refactor；
- 修改、构建、运行或写入 C++ authority runtime。

## Authority / Evidence

### VERIFIED — C++ release source

- `src/entity/frame_advance.cpp:25-48`：`frame_delay` 向0递进并 early return；随后 `link_state < 0` 和
  current frame `cpoint.kind == 2` 也 return；只有所有 gate 通过后才在 line87调用 `physics_update`。
- `src/entity/physics.cpp:326-342`：`x_int/y_int/z_int` 仅在 `physics_update` 尾部写两次；该函数没有为
  F03 early-return caller提供补偿 sync。
- `src/entity/game_tick.cpp:1280-1421`：state9998 cleanup 后立即 respawn scan；`respawn_count <= 0`
  branch按 all active、same relation、current character-DAT 的 `x_int/z_int` 计算平均，再写 respawn entity。
  first Z clamp在该 scan之后（1423-1438），不能补偿其读值。

### VERIFIED — Unity source crosswalk

- exact character：`BattleEcsCharacterFrameAdvancePass.TryExecute` 的 frame delay/link/cpoint early return在
  `ExecuteCharacterDynamics`（唯一 exact sync call）之前；成功 dynamics才在 line135 sync。
- fallback / shared character DAT：`RunReleaseFrameAdvance`、`RunSharedCharacterDatFrameAdvanceAsCharacter`、
  `RunSharedNonCharacterDatFrameAdvance` 都先经过同类 gate，成功 physics后才各自 sync。
- Unity唯一已确认的 broad deviation是
  `SimulationWorld.Passes.partial.cs:654-661`：respawn gate之前全体 active `Runtime.SyncIntegerPosition()`。
- Unity respawn average `ApplyRespawnWithoutStoredCount` 读取 `other.Runtime.XInt/ZInt`，与 C++ reader相同；
  它自身在写入 respawn x/y/z之后的 final `SyncIntegerPosition()` 是必要的实体自身写回，不在本包移除范围。

### UNKNOWN / boundary

- 其他跨模块 direct-position writer是否都在 C++ 对应写点显式更新 integers，归 R4/R5各自的 writer contract；
  本包不会用 global sync 兜底。
- C++ executable trace、actual respawn Play Mode和stage asset均未验证。

## Planned behavior contract

| checkpoint | C++ contract | Unity required behavior |
|---|---|---|
| F03 frame-delay early return | decrement delay then no physics/tail int sync | exact/shared runtime `XInt/ZInt` 保留旧值 |
| F03 negative-link early return | no physics/tail int sync | `XInt/ZInt` 保留旧值 |
| F03 cpoint-kind2 early return | no physics/tail int sync | `XInt/ZInt` 保留旧值 |
| F04 success | physics tail sync | 保持现有 per-physics sync，不由本包改动 |
| F05 respawn no-count | sum same-relation active character-DAT `x_int/z_int` | 读取上述各路径的真实当前 integer 值，不先全体重算 |
| respawn writer completion | write respawn position + its own int fields | 保留实体自身 final sync |

## Acceptance

1. **S0 source/static**：`PostFrameAdvanceDeathCleanupAll` 在 `PassesRespawnGate` 前不再包含全体 active
   integer sync；保留后续 respawn entity自身 sync。
2. **S1 focused fixture**：一个 respawn entity与四个同 relation allies：exact frame-delay、exact negative-link、
   exact cpoint-kind2、shared-character-DAT frame-delay。它们的 double坐标和预先存在的 integer坐标故意不同。
   `SerialTickAll` 后四者仍保留 stale integers，respawn使用 stale averages加 C++ RNG offset，而不是 live doubles。
3. **S2 regression**：existing Unity Editor scripts compile、full `BattleRuntimeSelfCheck`、ledger validator与
   `git diff --check`均通过。
4. **S3 evidence boundary**：最多 `RUNTIME_PENDING`；不声称 real respawn Play Mode、C++ trace或所有 direct-position
   writers已验证。

## Stop conditions

- 删除 loop 后揭示任何成功 physics path遗漏自己的 tail sync；
- fixture需要修改 stage / CPoint / held / respawn公式、RNG或其他 package的 writer；
- C++ source无法闭合 reader/writer时点，或必须运行/修改 C++ runtime；
- 需要改变 protected capacity / central renderer / 30Hz / FrameInputSet / SoA/pool边界。

## Out of scope

`D-MOV-001/002/004/005`、R3-PHY-01、R4～R8、R1-WP02、T8 default `stage.dat`、服务器、Android。

## 实际验证结果（2026-08-22）

- **最小脚本写入**：仅移除 `PostFrameAdvanceDeathCleanupAll` 在 respawn gate 前对所有 active entity 的
  `Runtime.SyncIntegerPosition()` loop；没有修改成功 physics path 的 tail sync、任何 respawn formula/RNG 或
  respawn entity 自身完成写入后的 final sync。
- **joint fixture**：新增 `CheckRespawnReadsPhysicsTailIntegerCoordinates`。它将 four same-relation
  character-DAT participants 的 live double position 与预先存在的 integer position 故意分离，并分别覆盖 exact
  `frame_delay`、exact `link_state < 0`、exact `cpoint.kind == 2` 以及 shared-character-DAT `frame_delay`。
  `SerialTickAll(1)` 后断言早退路径保留 stale ints；随后 no-count respawn 按四份 stale integer 的
  `(avgX, avgZ) = (40, 50)` 与同一 deterministic RNG offsets生成位置。旧 Unity global sync会先将它们覆写为
  live values，因而无法通过本夹具。
- **static**：`PostFrameAdvanceDeathCleanupAll` 的 respawn-gate 前 segment不再包含
  `SyncIntegerPosition()`；`ApplyRespawnWithoutStoredCount` 的 entity final sync仍存在。
- **Unity compile / self-check**：现有 Unity Editor（MCP port 6401）在 03:00:48 +08:00 force
  scripts refresh/compile 后 ready；菜单 `NTSD/验证/运行战斗运行时自检` 实际执行，
  `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 03:01:32 +08:00。
- **治理检查**：`Tools/Validate-ChangeLedger.ps1` PASS（13 records / 11 governed code files）；
  `git diff --check` exit 0，只有既有 LF/CRLF warning。
- **边界**：未运行或写入 C++ runtime，`R1-WP02` trace仍 `BLOCKED`；未进行 respawn Play Mode、stage asset
  或其他 direct-position writer audit。因此本包为 `RUNTIME_PENDING`，不是完整 movement/respawn 对齐结论。
