# R3-INP-03A — canonical full-held FrameInputSet packet contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、static、Unity scripts compile 和 focused self-check 均已通过；
> 仍缺 C++ trace，physical binding 另属 R3-PHY-01。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 执行方式：按 `D-009` 连续推进；脚本修改前已建立 test-only `R3-INP-003A-001` Change Record。

## Goal

只关闭 `D-INP-003` 的 adapter contract：定义 Unity 的 canonical input packet 为“每个 active human
player、每个 logical tick、一个完整 `Buttons` held-state”，并验证它按 C++ `InputHandler::poll` 的顺序
得到相同的 `key`、`prev`、cooldown 和 `input_history` 可观察字段。

这不是把 Unity InputAction 边沿或 `PressedButtons` / `ReleasedButtons` 当成新的战斗真相；C++ authority
只在 tick 读取当前 `is_down(...)` state，再由前一个 key state 推导 edge。

## Scope

本包允许的最小步骤：

1. 只读核对 C++ `InputHandler::poll`、P1/P2 caller 与 Unity packet/state adapter；
2. 只在 `BattleRuntimeSelfCheck.cs` 增加一个 test-only fixture，覆盖一个 human roster slot 的：
   - press：多个 logical key 同 tick 按下；
   - hold：同一完整 held packet 重复一 tick；
   - release：下一 tick 所有 `Buttons=None`；
   - edge / cooldown / history 的固定顺序与 no-replay 行为；
3. 验证 fixture 后，只更新 Task / Record / ledger / STATE / diff register / handoff。

禁止：

- 改动 `SimulationFrameInputModule`、`FrameInputSet`、`SimInputBuffer`、`NTSDInputStateModule`、
  `SimulationTickDriver`、lockstep protocol、worker、roster capacity、AI target、InputAction / Inspector asset；
- 把 `PressedButtons` / `ReleasedButtons` 的 checksum/diagnostic metadata 强行改成 gameplay source；
- 改 physical W/S/A/D/J/K/L binding、C++、DAT、scene、renderer、30 Hz、FrameInputSet/SoA/pool/0-GC
  已批准边界。

若 fixture 指出 actual adapter 不等价，停止该 test-only Record；先在 source 中闭合差异并建立一个新的
implementation Change Record，不能顺手改 writer。

## Authority / Evidence

### C++ release live source — VERIFIED(source)

- `src/core/main.cpp:4607-4608`：active P1、P2 每 tick 分别调用 `input.poll(*p1, DEFAULT_P1)` 与
  `input.poll(*p2, DEFAULT_P2)`；
- `src/input/input_handler.cpp:1555-1613`：`poll` 的固定顺序是：
  1. `prev_* = key_*`；
  2. seven logical keys 从 current held `is_down` 覆盖；
  3. existing cooldown decrement；
  4. new press 依 `right,left,up,down,attack,defend,jump` 顺序写 cooldown 与 history；
  5. combo/action 进入后续 `apply_input`，不属于本包。

### Unity current source — VERIFIED(source)

- `SimulationTickDriver.cs:98-143`：本地 provider 每 tick 捕获 current held `Buttons`，并把
  pressed/released 生成为 metadata；
- `SimulationFrameInputModule.cs:42-69`：每位 roster-bound non-AI human 将 seven logical `Buttons`
  写入一个 complete packet；
- `NTSDInputStateModule.cs:74-115`：complete packet 先从 runtime 取上 tick held state、写 prev、应用
  full held packet、decrement cooldown、按固定 logical order 生成 press edge / history；
- `BattleRuntimeSelfCheck.cs::CheckAudit6InputPhaseOrder` 与
  `Test/Editor/LocalFrameInputProviderEditorTests.cs` 已覆盖部分 input state / packet behavior，但没有
  一份直接以 C++ poll contract 描述 press→hold→release→multi-key 顺序的 focused fixture。

### Evidence limits

- `R1-WP02` C++ full trace 仍 BLOCKED；source/fixture 不可写成 C++ executable runtime equality；
- physical `InputAction` asset / W-S-A-D-J-K-L binding 属于 `R3-PHY-01`；
- canonical frame 缺 player、3+ human extension、AI target behavior 分别属于 `R3-INP-04` / `R3-AI-TGT-01`；
- C++ P1/P2 physical configuration与 Unity local player capacity不是同一实现边界。

## Proposed contract

```text
C++ tick poll:
  previous key fields <- last tick current keys
  current key fields  <- current held physical state
  cooldown decrement
  new edges -> cooldown/history in logical C++ order

Unity canonical packet:
  FrameInputSet[player].Buttons = full current held logical state
  metadata PressedButtons/ReleasedButtons is observational, never overrides Buttons
  complete packet -> same prev/current -> cooldown -> history order
```

## Files likely involved

| 类别 | 文件 | 允许职责 |
|---|---|---|
| test only | `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 添加一个 C++ poll-contract focused fixture。 |
| records | `docs/ai/...` | 记录 source、结果和未关闭项。 |

## Acceptance / verification

| 层级 | 最小验收 | 初始状态 |
|---|---|---|
| S0 source | C++ poll order 与 Unity complete-packet projection 有明确字段 mapping。 | `PASS` |
| S1 fixture press | multi-key press 写正确 current/prev/cooldown/history。 | `PASS` |
| S2 fixture hold | same held packet 不重复 history，只有 cooldown tick。 | `PASS` |
| S3 fixture release | full `Buttons=None` 写 prev=1/current=0，不重放 edge。 | `PASS` |
| S4 compile/self-check | static、ledger、Unity compile、`error CS`、full self-check。 | `PASS` |
| S5 Play Mode | physical keyboard binding。 | `OUT_OF_SCOPE / R3-PHY-01` |
| S6 C++ trace | same packet trace。 | `BLOCKED / R1-WP02` |

## Stop conditions

停止并记录，若：

1. actual fixture 需要改 `FrameInputSet` / packet writer / InputAction / roster / worker 才能成立；
2. 需要让 pressed/released metadata 取代 C++ current-held poll truth；
3. 需要改 C++、运行 C++ executable，或进入 AI target / physical binding / capacity scope；
4. fixture 依赖 DAT、scene、render、movement、held/CPoint/collision。

## Out of scope

`D-INP-004`（R3-INP-04）、`D-INP-005`（R3-AI-TGT-01）、`D-INP-006`（R3-PHY-01）、所有 D-MOV、
R4～R8、R1-WP02 trace、T8 default `stage.dat`、服务器和 Android 验收。

## 实际验证结果（2026-08-22）

- 新 fixture 以 `Right|Attack|Jump|Defend` 完整 packet 检查 C++ logical edge order：press 时 history
  末四位为 `6,9,0,5`，同 held tick 不重复写 history、只将四个 cooldown `5→4`，release tick 写
  `prev=1/current=0` 并将 cooldown `4→3`；
- static guard 同时确认 C++ `attack → defend → jump` anchor 与 Unity `ApplyNewPressEdges` 顺序，且
  `SimulationFrameInputModule` application 不读取 `PressedButtons` / `ReleasedButtons` 为 gameplay truth；
- `Tools/Validate-ChangeLedger.ps1`：PASS（8 records、10 governed code files）；
- `git diff --check`：exit 0（仅现有 LF/CRLF warning）；
- 现有 Unity Editor 的 UnityMCP force scripts refresh / compile：成功，filtered `error CS` 为 0；
- `NTSD/验证/运行战斗运行时自检`：`Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`，
  文件最后写入为 `2026-08-22 01:20:32 +08:00`。

这是 canonical full-held packet adapter 的 code-level contract，不是实际键盘资产、2-human scene、C++
executable trace 或所有 combo behavior 的对齐证书。
