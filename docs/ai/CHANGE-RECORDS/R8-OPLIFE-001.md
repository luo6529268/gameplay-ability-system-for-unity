# R8-OPLIFE-001 — production opoint birth and lifecycle Play probe

<!-- CHANGE-RECORD
id: R8-OPLIFE-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOpointLifecyclePlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\frame_advance.cpp:100-140; src/entity/game_tick.cpp:605-638,2190+; Makefile release path; user approval 2026-08-23
evidence: Existing W05 EditMode coverage cannot provide R8 S4 live NTSD_Battle producer-to-consumer evidence
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / battle certification

## 1. 状态与范围

- 当前状态：`VERIFIED`
- 所属 Work Package：`R8-WP01C-01`
- 不属于本次范围：任何 production gameplay/factory/pool/pass/DAT/scene 修改，以及WP01C-02～07
- 关联 Change ID：`R7-LATE-001`、历史 `W05OpointLifecycleEditorTests`

## 2. Authority / 需求依据

- C++ release：`frame_advance.cpp::process_opoint_spawn/spawn_from_opoint` 和
  `game_tick.cpp::game_tick/free_entity`；只读 source contract；
- 用户明确批准：2026-08-23“批准执行 R8-WP01C-01，恢复目标”；
- Evidence等级：C++ source `VERIFIED`，S4运行结果当前`PENDING`。

## 3. Unity 原状与已确认差异

- `W05OpointLifecycleEditorTests` 已验证最低空闲slot、high/low scan cursor、generation/reuse和0 B，但运行在
  隔离EditMode world；
- `BattleComboPlayModeProbeEditor` 可进入真实场景，但只记录特定技能对象数量，不能标识四类对象、handle或reuse；
- 当前不是已确认 gameplay 差异，而是 R8 S4 证据缺口；
- 当前Unity Editor进程未运行，属于运行前置，不是production失败。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleOpointLifecyclePlayModeProbeEditor.cs` | Editor-only explicit probe | 不存在 | 在live production world执行四类opoint与scan/reuse认证，输出JSON并best-effort cleanup |

## 5. 不可回退边界

- 不修改CentralOnly/Texture2DArray/dynamic Mesh/URP或1.5 scale；
- 不修改Authority400/MobileExtended/DesktopExtended容量合同；
- 不修改30 Hz、FrameInputSet、slot/generation算法、SoA/ECS、对象池、worker或0 GC production路径；
- 不修改C++、DAT、scene、T8、Android及已关闭Change ID。

## 6. 实际改动

已新增 `#if UNITY_EDITOR`、仅显式菜单触发的 `BattleOpointLifecyclePlayModeProbeEditor`：

- 在live world idle边界用正式factory/catalog/structural writer执行OID33/120/203/999四类birth；
- 记录CLR/type、frame/runtime frame、Prev2、slot/generation、SpawnSemantic和render/logic pool delta；
- 每次release验证旧handle失效及object/pool baseline恢复；下一类复用最低slot并验证generation前进；
- dedicated worker存在时暂停自动推进并通过既有paused diagnostic tick接口获取high/low scan witness，
  不停止或替换worker；没有worker时才使用同步`StepOneTick(ignorePaused:true)`；
- 统一best-effort cleanup并恢复driver原paused状态，结果写入
  `Temp/NTSD_R8_WP01C_01_OpointLifecycle.result.json`；
- 新增对应meta；未改production脚本、scene、DAT或资源。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity 2022.3.62f3 force all refresh/compile | 新脚本导入；Assembly-CSharp-Editor.dll 09:01:25晚于源码；Tundra success；Console 0 error | `PASS` |
| focused test / self-check | W05 class + full BattleRuntimeSelfCheck | job `3b8e08105d0946bca58d88e5ed6ef990` 8/8 PASS；09:06:51 full self-check PASS | `PASS` |
| Play Mode / 集成 | `NTSD_Battle` explicit opoint lifecycle probe | result 09:05:09 PASS；四类birth、high/low cursor、release/reuse、cleanup通过 | `PASS` |
| C++ authority 对照 | source contract | 入口与行为合同已读 | `VERIFIED` |
| 可选 full trace | R1-WP02 | 观察通道未闭合 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 风险：live world 中的fixture producer或spawn若清理失败会污染后续场景；结果证明本次cleanup完整；
- 未关闭项：C++ full trace；extended >399 real Play cursor；WP01C-02～07；
- 回滚方式：仅删除本Change新增probe及meta，并保留结果/失败证据；不回退其他工作树内容；
- production first-difference出现时建立新D-ID/repair WP，本Record不修复。

## 9. Git / 交接

- 修改前工作树为脏工作树，包含用户、R2～R8和资源/场景改动；不得覆盖或清理；
- 实际diff范围：新增唯一Editor probe及meta，并同步治理文档；
- 提交hash：无；
- validator：final PASS，59 records / 59 governed code files；scoped diff check PASS；
- 交接优先阅读：本Record、`R8-WP01C-01-opoint-birth-lifecycle-execution.md`、R8-WP01C handoff。

最终运行证据：

- production Play：tick356→359，worker active；四类slot53 generation 1/3/5/7；high 52→53 same-pass，
  low 53→52 next-pass；baseline/final object6、claimed4、object-pool2、logic-pool4；
- persistent summary：`RESEARCH/R8-WP01C-01-opoint-lifecycle-runtime-evidence-20260823.md`；
- Play后Console 0 error/warning；production代码无改动。

`VERIFIED`只裁决本Editor probe及`R8-WP01C-01`取得了与范围相称的Unity S4证据；不把该状态
扩大为C++ full-trace、extended >399 Play、整个WP01C或完整战斗对齐。
