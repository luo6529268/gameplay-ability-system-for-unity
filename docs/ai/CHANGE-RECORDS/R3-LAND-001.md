# R3-LAND-001 — character landing raw-frame writer

<!-- CHANGE-RECORD
id: R3-LAND-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDamageStateResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\physics.cpp and src\entity\game_tick.cpp/frame_advance.cpp release live landing writer lifecycle
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-PREFLIGHT-VERIFIED / UNITY-COMPILE-PASS / FULL-SELF-CHECK-PASS / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / physics / landing / frame-history / test  
> 所属 Work Package：`R3-LAND-01`  
> 当前状态：`RUNTIME_PENDING` — 已完成最小脚本写入、existing Unity Editor compile和full self-check；仍缺 Play Mode/集成与 C++ runtime trace。

## 1. 目标与允许范围

只把 character-DAT landing 的 frame writer 从 Unity-only `ImmediateFrame` 副作用收缩为 C++ F04 的
raw-frame 语义。允许路径仅为：

- `LF2CharacterDamageStateResolver.cs`；
- `LF2Entity.cs` 的 `ApplySharedCharacterDatLandingIfNeeded`；
- `BattleRuntimeSelfCheck.cs` 的落地 writer fixture。

不改通用 frame API、scheduler、collision/hit/CPoint/held/link/opoint、non-character landing、render、
scene/DAT/input/AI/ECS layout/pool/worker，也不触碰 C++ authority。

## 2. 权威依据

- **C++ source VERIFIED**：
  - `physics.cpp:153-223` 在 character landing 直接写 `core.frame`；state12 low-speed和 ordinary
    landing 才显式清 `special.attacking`；state12 high-speed、state18、state13 high-speed不清；
  - `game_tick.cpp:1247-1276,1645-1655,577-587` 确认 F04 physics 早于 candidate，F07 frame_tick 很晚执行；
  - `frame_advance.cpp:847-855,995` 确认 F07 才根据 `frame != wait_counter` 清 attacking并更新
    `wait_counter`。
- **Unity source VERIFIED**：
  - exact character 和 shared-character-DAT 各有上述 landing writer；两者当前调用
    `ImmediateFrame`；
  - `ImmediateFrame` 写 PN/attacking/Sprite/transistor；raw helper保留 PN/attacking/wait-counter并同步
    target DAT wait/next。
- **C++ runtime trace BLOCKED**：R1-WP02 未恢复，绝不运行或写 C++ release runtime。

## 3. 预期修改矩阵

| 分支 | 帧写入 | attacking | PN / wait-counter |
|---|---|---|---|
| state13 high | raw 185 | 保留 | PN、wait-counter保留 |
| state12 low | raw 230/231 | 立即置0 | PN、wait-counter保留 |
| state12 high | raw 185/191 | 保留 | PN、wait-counter保留 |
| state18 | raw 185 | 保留 | PN、wait-counter保留 |
| ordinary | raw 94/215/219 | 立即置0 | PN、wait-counter保留 |

## 4. 实际改动与验证

### 实际脚本 diff

1. `LF2CharacterDamageStateResolver`：
   - state13 high-speed、state12 high-speed/state18 bounce改为 raw frame write，保留 attacking；
   - state12 low-speed与ordinary landing先 raw-write target frame，再按 C++ F04 branch清 attacking。
2. `LF2Entity.ApplySharedCharacterDatLandingIfNeeded`：对相同 character-DAT compatibility branch应用同一
   raw writer / attacking matrix；未触碰 `ApplyCurrentDatNonCharacterLanding`。
3. `BattleRuntimeSelfCheck`：新增 `CheckLandingRawFrameIntermediateState`，以 exact和shared两个路径各8个
   case覆盖 state12/13/18/ordinary target selection，并在 F04 后、F07 前断言 target `Frame.N/D`、preserved
   `Frame.PN`、branch-specific `AttackingCounter`、preserved wait counter及目标 DAT wait/next。

### 实际验证证据

| 层级 | 命令 / 场景 | 实际结果 | 状态 |
|---|---|---|---|
| source/static | C++ F04→candidate→F07 与 Unity exact/shared writer crosswalk。 | 分支和消费者闭合；目标 landing blocks不再调用 `ImmediateFrame`。 | `PASS` |
| first compile | UnityMCP `refresh_unity(force/scripts/compile)`。 | `CS0136`：内层 `landingFrame` 与外层局部变量重名；记录后改名为 `fallingLandingFrame`。 | `FAIL → fixed` |
| final compile | UnityMCP `refresh_unity(force/scripts/compile)`。 | 02:41:50 预期 domain reload/reconnect后 editor ready；随后 menu self-check实际运行。 | `PASS` |
| full self-check | `NTSD/验证/运行战斗运行时自检`。 | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 02:42:40 +08:00；新16 case均被此入口执行。 | `PASS` |
| ledger / diff | `Tools/Validate-ChangeLedger.ps1`、`git diff --check`。 | ledger PASS（12 / 11）；diff check exit 0，只有既有 line-ending warning。 | `PASS` |
| Play Mode | real landing / skill chain。 | 本包未运行。 | `RUNTIME_PENDING` |
| C++ authority trace | R1-WP02。 | 不运行 C++ executable。 | `BLOCKED` |

不运行 C++ executable、Unity Play Mode、完整 build、压力测试或 R4+。

## 5. 风险、回滚与停止条件

- 主要风险是错误地把 C++ F07 clear 提前到 F04，或错误地不保留 target DAT wait/next；本 Record 的 fixture
  只覆盖 F04 intermediate state，不能替代真实 landing visual / cross-module验收。
- 若 self-check 发现普通 writer/helper语义需要变更，停止本 Record并建立更小的新 Record；不扩大。
- 回滚只涉及本 Record三条代码路径及关联 docs；不得回退任何已有用户/历史改动。
- 提交 hash：未提交。

## 6. 交接

- 继续时先读本 Record、`TASKS/R3-LAND-01-landing-raw-frame-contract.md`、
  `RESEARCH/R1-SOURCE-003-*.md`、`RESEARCH/R1-SOURCE-004-unity-collision-crosswalk-and-diff.md:148-160`。
- 下一计划内工作包是 `R3-SYNC-RESP-01 / D-MOV-003` 的只读 preflight；它与本 Record无重叠，且必须先建立
  新 Task Contract / Change Record。
