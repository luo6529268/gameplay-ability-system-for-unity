# R3-INP-02 — F1/F2 step gate 与 battle-entry clear 分离

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（`R3-INP-002` 已通过 static、Unity compile 与 focused self-check；仍缺 Play Mode / physical binding / C++ trace）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 执行方式：按 `D-009` 连续推进；脚本修改前仍必须先建立独立 Change Record。

## Goal

只关闭 `D-SCHED-010` 的 **默认 F1/F2 step gate**：让 Unity 使用既有的
`BattleFlowRuntimeState.BattleStepMode` / `BattleStepGate` 表达 C++ 的以下 tick 行为，并与 Unity
battle bootstrap 的 `NeedClearInput` 完全分离：

```text
C++ default F1 wait:
cooldown → gate reset → skip complete input callback → T03..pre-postprocess render → return

C++ F2 one-step:
mode=2 → gate=1 + mode=1 → complete input callback → normal downstream tick

Unity entry clear:
bootstrap marker → existing maintenance → clear character-DAT input → return
```

本包的完成不是“F1/F2 物理按键已接入”。物理 Input Action / Inspector 映射仍是
`D-INP-006 / R3-INP-03` 的 Play Mode 范围。

## Scope

允许在建立 Change Record 后仅处理：

1. `NTSDBattleTickSystem` 中 C++ step gate 的每 tick reset、mode 2 → one-step gate / mode 1 转换；
2. wait tick 对完整 human/AI CharacterInput callback 的跳过；
3. wait tick 继续经过 OID、frame / interaction、preframe、stage、RenderDispatch，并在 render 后跳过
   FramePostProcess、late entity、mode2 / entity tail 与 results；
4. 保留 `NeedClearInput` 的 bootstrap 清输入和现有 early return；
5. `BattleRuntimeSelfCheck` 中新增 F1-wait、F2-one-step、entry-clear 三组 scheduler fixture。

本包禁止处理：

- `Runtime.LinkState < 0` input gate（`D-INP-001`，后续 `R3-HOLD-INP-01`）；
- `HP <= 0` AI prefilter（`D-INP-002`，后续 `R3-AI-LIFE-01`）；
- FrameInputSet edge journal、P1/P2/8-player adapter、AI target optimizer、物理 F1/F2/W/S/A/D/J/K/L binding；
- C++ `g_dword_449048` A-B-C debug-unlock 非零分支；
- OID formula、CPoint / held / link / collision / opoint、frame / physics、render implementation、DAT、scene、资源；
- CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× scale、fixed-world camera、capacity、30 Hz、
  FrameInputSet、SoA/ECS、pool、worker 或 0-GC 边界。

## Authority / Evidence

### C++ release live source — VERIFIED(source)

- `src/core/main.cpp:159-166`：F1 将 `g_battle_step_mode` 在 0/1 间切换；F2 写 mode 2；
- `src/entity/game_tick.cpp:994-1000`：每 tick 先清 `g_dword_44905C`，mode 2 时写 gate 1 并改回
  mode 1；
- `game_tick.cpp:1002-1005`：mode 1 且 gate != 1 时，跳过完整 `post_cooldown_input` callback；
- `src/core/main.cpp:4607-4608, 5505-5522`：callback 包含 P1/P2 poll、active character DAT 的
  AI prepare 与 `apply_input`；
- `game_tick.cpp:145-156, 1466, 1878`：wait gate 仍由后续恢复/held 分支读取；
- `game_tick.cpp:2066-2077`：wait 条件下仍走 preframe/stage/render，render 后再 early return；
  默认分支要求 `g_dword_449048 == 0`。

### Unity current source — VERIFIED(source)

- `BattleRuntimeState.cs:305-306` 已有 `BattleStepMode` / `BattleStepGate`，其 reset、checksum、
  scalar snapshot 和 restore 均已存在；
- `LF2Entity.cs:2631-2643`、`BattleEcsCharacterRecoveryPass.cs:120-129`、
  `LF2CharacterDatHitResolver.cs:33-37` 已读取 step-wait predicate；
- `NTSDBattleTickSystem.cs` 尚未在每 tick 生产 gate，也没有在 RenderDispatch 后实现 step wait
  return；
- `SimulationTickDriver.cs:1213` 将 `NeedClearInput` 作为 bootstrap marker；
  `NTSDBattleTickSystem` 的 clear branch 清输入后在 frame/interaction/render 前返回；
- `BattleRuntimeSelfCheck.CheckGameTickInputClearBoundaries` 已锁定现有 entry-clear 契约。

### Evidence limits

- 以上是 C++ source contract，不是 C++ runtime trace；`R1-WP02` 仍 `BLOCKED`；
- `g_dword_449048 != 0`、A-B-C physical debug-unlock 与 Unity physical F1/F2 binding 当前均为
  `UNKNOWN` / out of scope；默认 `0` 分支才是本包可验证对象；
- `NeedClearInput && step wait` 的同时出现没有 C++ 同名对应物。本包必须保持现有 entry-clear
  行为优先，不得将该 Unity bootstrap 组合伪装成 C++ F1/F2 source fact。

## Proposed minimal implementation contract

在 Change Record 建立后，目标调度形态为：

```text
Tick start
  → cooldown
  → BattleStepGate = 0
  → if BattleStepMode == 2: BattleStepGate = 1; BattleStepMode = 1
  → if NeedClearInput: 保持既有 human-poll / M1 / clear / return 边界
  → else if step wait: skip human poll + CharacterInput
  → else: PostCooldownHumanInput + CharacterInput
  → OID / frame / interaction / preframe / stage / RenderDispatch
  → if step wait: return after RenderDispatch
  → FramePostProcess / late entity / tails / results
```

实现只能复用已有 `BattleFlowRuntimeState` 字段，不新增持久分配、逐帧容器、renderer fallback 或
外部输入资产。若发现真实 source order 需要重排 CPoint、held、collision、frame physics 或 `NeedClearInput`
本身，立即停止，另开 Task / Change Record。

## Acceptance / Verification

| 层级 | 最小验收 | 状态 |
|---|---|---|
| S0 source | 以上 C++ / Unity 坐标闭合；默认与非零 debug-unlock 分支分开标记。 | `PASS (source)` |
| S1 F1 fixture | mode 1/gate 0：输入 callback 不运行；OID/frame/RenderDispatch 仍运行；FramePost/late/tail/results 不运行；gate 保持 0。 | `PENDING` |
| S2 F2 fixture | mode 2：同 tick 写 gate 1、mode 1，完整 input 与 normal tail 运行；下一 tick 无 F2 时回到 wait。 | `PENDING` |
| S3 entry-clear fixture | `NeedClearInput` 继续执行既有 M1→clear→return，不被 step-gate 重命名或破坏。 | `PENDING` |
| S4 compile/self-check | 现有 Editor scripts compile、filtered `error CS`、BattleRuntimeSelfCheck request。 | `PENDING` |
| S5 Play Mode / input asset | physical F1/F2 映射与视觉 pause overlay。 | `PENDING / R3-INP-03 / user` |
| S6 C++ trace | same fixture C++ trace。 | `BLOCKED (R1-WP02)` |

## Files likely involved

| 类别 | 预计文件 | 说明 |
|---|---|---|
| scheduler | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` | 唯一的 normal / step-wait 调度适配点。 |
| focused test | `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 仅增加三组 scheduler fixture。 |
| docs | Change Record、ledger、STATE、diff register、handoff | 按 D-008 留痕。 |

`BattleRuntimeState.cs`、lockstep snapshot、renderer、Input Action asset、`SimulationWorld.Passes.partial.cs`
默认不在本包可写集合，因为 Flow 字段已存在；若实施证明必须触及它们，停止并新建/修订 Contract。

## Stop conditions

立即停止并记录 blocker，若：

1. 必须新增 `g_dword_449048` / A-B-C debug-unlock、F1/F2 physical binding 或 UI overlay 才能让默认
   core gate 正确；
2. `NeedClearInput` 与 step wait 同 tick 的现有 Unity bootstrap 行为无法保持且 C++ source 无可引用合同；
3. 必须改 CPoint / held / link / collision / opoint / frame physics、AI decision、FrameInputSet 或
   input asset；
4. 需要改 C++ source/build/executable/config/resource，或运行 C++ executable；
5. focused fixture 指向 scope 外的 lifecycle/renderer/scene/DAT 问题。

## Out of scope

`D-INP-001`、`D-INP-002`、`D-INP-003`～`006`、所有 `D-MOV-*`、R4～R8、R1-WP02 trace、
T8 default `stage.dat`、服务器和 Android 验收。
