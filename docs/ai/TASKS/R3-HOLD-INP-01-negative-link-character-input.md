# R3-HOLD-INP-01 — negative-link character input eligibility

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（最小脚本改动、静态检查、Unity compile 与 focused self-check 已通过；仍缺 relation joint fixture / Play Mode / C++ trace）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 执行方式：按 `D-009` 连续推进；脚本修改前已建立独立 `R3-HOLD-INP-001` Change Record。

## Goal

只关闭 `D-INP-001` 的 **character input eligibility** 差异：当一个 active、current DAT type 为
character 的 Unity 实体具有 `Runtime.LinkState < 0` 时，不能因为 Unity 的总体前置 return 而跳过
本 tick 的 character input 调用链。

该目标只恢复 C++ `InputHandler::apply_input` 的调用资格。它不主张 negative-link 的角色可摆脱
held/caught/cpoint 约束，也不改写这些约束的 frame advance、held、collision 或生命周期行为。

## Scope

允许在本 Task 的 Change Record 范围内，仅做以下最小变动：

1. 在 `LF2Entity` 的 character-DAT compatibility 输入入口中，保留 null / DAT type gate，移除
   `Runtime.LinkState < 0` 的整体跳过；
2. 在 `LF2Character` 的 production character 输入入口中作同样处理；
3. 在 data-oriented exact-character AI input pass 中作同样处理，使 AI 不会在 global eligibility
   gate 被跳过；
4. 在 `BattleRuntimeSelfCheck` 新增一个有效 negative relation、current DAT type character 的
   focused fixture，证明 direct input / input tail 到达既有 resolver，而不是在入口被整体返回；
5. 更新 Change Record、ledger、STATE、全量差异登记和 handoff。

禁止：

- 修改 `IsBlockedByReleaseLinkOrCaughtCpoint()`、frame advance、held pass、CPoint、positive/negative
  link cleanup、opoint、collision/hit、death/respawn、input poll / packet / cooldown writer；
- 修改 action resolver 中已有的具体 `LinkState == 2` heavy/held 分支；
- 修改 `NeedClearInput`、F1/F2 step gate、AI target strategy、FrameInputSet、physical input binding；
- 修改 render、CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× scale、fixed-world camera、capacity、
  30 Hz、SoA/ECS、pool、worker、0-GC、scene、DAT 或 C++。

## Authority / Evidence

### C++ release live source — VERIFIED(source)

- `src/entity/game_tick.cpp:994-1005`：post-cooldown callback 在非 step-wait tick 被调用；
- `src/core/main.cpp:5505-5522`：callback 在 `game_tick > 1` 时按 `0..MAX_OBJECTS-1` 升序处理每个
  `active && char_data && char_data->obj_type == 0` 的实体；AI 先 `prepare_ai_input`，之后所有该类
  实体调用 `input.apply_input(...)`。该 caller 没有 `hp` 或 `link_state < 0` 前置过滤；
- `src/input/input_handler.cpp:2742-3096`：`apply_input` 只先检查 `char_data` / current frame；其内部
  顺序是 combo → direct `hit_a/hit_d/hit_j` → state-specific input → resulting-frame `dvx/dvy/dvz` tail，
  没有 negative-link 的函数级 return；
- 同文件的 relation 判断是局部的，例如 combo / heavy walk 的 `link_state == 2`，以及站立、跑步、
  跳跃、dash 的具体 held action 分支。因此不能用 Unity 的 `LinkState < 0` 总体 early return 代替。

### Unity current source — VERIFIED(source)

- `LF2Entity.cs:2692-2717` 与 `LF2Character.cs:819-866` 在 character input entry / known-character
  entry 中都将 `Runtime.LinkState < 0` 作为总体 return；
- `SimulationWorld.Passes.partial.cs:232-329` 已按 runtime slot 遍历 current character DAT；
- `BattleEcsCharacterInputPass.cs:81-137` 的 exact AI path 也以 `runtime.LinkState < 0` 返回成功但不做
  AI / resolver / velocity tail；
- `BattleCharacterInputActionResolver.cs` 已在局部 combo / state-action 路径中使用
  `LinkState == 2` 与 existing held predicates；本包不扩大或重写这些判断；
- `LF2Entity.IsBlockedByReleaseLinkOrCaughtCpoint()` 仍在 frame advance 使用，明确不属于 input eligibility
  改动范围。

### Evidence limits

- 以上是 source-contract evidence；`R1-WP02` 的 C++ full trace 仍 `BLOCKED`，本包不能写成 C++ runtime
  equivalence；
- source 能证明 C++ caller 不做 global negative-link skip，但当前没有可重复 C++ runtime fixture 证明
  每一种 caught / held subtype 的最终动画；这些情况必须保留到 R5 的 relation joint fixtures；
- Unity `LinkState < 0` 可用于多类 held child；只有 current DAT type character 才落入本 Task 的 input
  eligibility。非 character object 仍不会进入 character input pass。

## Proposed minimal implementation contract

```text
C++ source callback eligibility:
active + current DAT type character
  → [AI: prepare_ai_input]
  → apply_input
  → apply_input 内的局部 relation / state gate

Unity target eligibility:
active + current DAT type character + runtime exists
  → [AI: existing prepare path]
  → existing combo/direct/release resolver
  → existing frame-velocity tail
  → existing local relation / state gate
```

不能把 negative-link 输入调用资格与 negative-link frame advance 或 held attachment 处理合并。若必须改动
上述禁止模块才能使 fixture 通过，停止并升级为新的 Contract，而不是扩大本包。

## Files likely involved

| 类别 | 文件 | 允许职责 |
|---|---|---|
| compatibility input entry | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` | 仅移除 overall negative-link eligibility guard。 |
| production character entry | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` | 仅移除 overall negative-link eligibility guard。 |
| exact AI input | `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterInputPass.cs` | 仅移除 overall negative-link eligibility guard。 |
| focused test | `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 仅新增 negative-link input-entry fixture。 |
| records | `docs/ai/...` | 按 D-008 留痕。 |

## Unknowns

1. 哪些 production DAT / CPoint chain 会让 character DAT 持久处于 negative link，尚无 C++ runtime trace；
2. Unity existing action resolver 的所有 state-specific relation branch 与 C++ 的逐 case 等价性，仍由
   R3/R5 后续合同与 joint fixture 覆盖；
3. physical input binding、packet edge / replay、dead/respawn AI 与 full trace 都不在本包中。

## Deliverables

1. `R3-HOLD-INP-001` Change Record，先于所有脚本改动建立；
2. 最小 Unity input eligibility patch 与 focused self-check；
3. 更新 ledger、STATE、D-INP-001 差异状态、handoff；
4. 静态检查、Unity scripts compile / `error CS` 查询、request self-check 的实际结果。

## Verification

| 层级 | 最小验收 | 初始状态 |
|---|---|---|
| S0 source | 上述 C++ caller / `apply_input` 与 Unity global guards 坐标闭合。 | `PASS` |
| S1 focused relation fixture | valid negative relation 的 character direct input 不被入口跳过；existing resolver / tail 执行。 | `PASS` |
| S2 local static | 四个 global character-input guard 被移除，type/null gate 与 frame-advance link gate 保留。 | `PASS` |
| S3 compile / self-check | 现有 Editor scripts compile、filtered `error CS`、BattleRuntimeSelfCheck request。 | `PASS` |
| S4 joint fixture / Play Mode | caught/held input、release、frame/lifecycle 的联合行为。 | `PENDING / R5` |
| S5 C++ trace | same fixture C++ trace。 | `BLOCKED (R1-WP02)` |

## Stop conditions

立即停止并记录 blocker，若：

1. 需要改变 frame advance、held / CPoint / link cleanup、collision、opoint、DAT、scene 或 render 才能让
   input-entry fixture成立；
2. 现有 action resolver 对 negative link 缺少 C++ source 可确认的局部条件，且不能在本包的现有
   `LinkState == 2` / held guard 下闭合；
3. 需要修改 C++ source/build/executable/config，或运行 C++ executable；
4. fixture 指向 physical binding、AI target、dead/respawn、packet journal 或长期 Unity adapter 问题。

## Out of scope

`D-INP-002`～`006`、`D-SCHED-004`、所有 `D-LINK-*`、`D-HOLD-*`、`D-CPT-*`、`D-OP-*`、所有
`D-MOV-*`、R4～R8、R1-WP02 trace、T8 default `stage.dat`、服务器与 Android 验收。

## Actual result (2026-08-22)

`R3-HOLD-INP-001` 已完成允许范围内的最小 patch：`LF2Entity`、`LF2Character`、
`BattleEcsCharacterInputPass` 的 character-input entry 不再以 negative link 全体跳过。新增 fixture
已从 valid negative relation 的 human world path 与 shared character-DAT compatibility path 验证
direct input → frame 10 → `dvx=7` tail。UnityMCP scripts compile、filtered `error CS`=0、完整
`BattleRuntimeSelfCheck` result `PASS` 均已实际获得。

仍没有 C++ runtime trace、negative-link held/caught Play Mode / lifecycle joint fixture 或物理输入验收，
故状态保持 `RUNTIME_PENDING`，不得据此宣称 held/caught 逻辑已完整对齐。
