# R8-SPRITEMAP-006 — Game/Scene central submission first-difference diagnostic

<!-- CHANGE-RECORD
id: R8-SPRITEMAP-006
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralGameVisibilityPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\render\renderer.cpp:581-624; src\entity\game_tick.cpp; Makefile
evidence: final tick257 report has 3 snapshots, 6 resolved commands and 1 submitted draw; fresh Game screenshot shows entities
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / live central submission diagnostic

## 1. 状态与范围

- 当前状态：`VERIFIED`（仅Editor diagnostic / Game submission证据范围）
- Work Package：`R8-WP01D-06`
- 唯一允许脚本：新增`BattleCentralGameVisibilityPlayModeProbeEditor.cs`及meta；
- production、scene、URP asset、DAT/BMP、Legacy owner和专项分支全部禁止。

## 2. 修改前证据

- `R8-SPRITEMAP-002/004`：5537 descriptor entries、0 differences；
- `R8-SPRITEMAP-005`：84,327,319 source→central pixels与dynamic Mesh witness均PASS；
- `Temp/R8-WP01D-06/R8-WP01D-06-game.png`：HUD/背景可见，实体不可见；
- 因此下一步只诊断正式frame/submission/camera，不再改图片mapping。

## 3. 计划改动与验收

- 遍历全部runtime slots并调用现有`CaptureEntityDiagnosticBySlot`；
- 投影`CaptureDiagnosticReport`、main camera与feature materials；
- 输出JSON及reason summary，恢复pause；
- compile/Play/cleanup/validator后根据first difference决定是否新建production Change Record。

## 4. 当前验证

| 层级 | 结果 | 状态 |
|---|---|---|
| Task/Record/Ledger/STATE | 已建立 | `PASS` |
| compile | source 11:47:36 < Editor DLL 11:47:58；C# compiler error 0 | `PASS` |
| first live diagnostic | tick 1为`NO_SNAPSHOT_ENTITIES`；扩展字段后认定为过早采样 | `SUPERSEDED EVIDENCE` |
| final live diagnostic | tick 257；3 snapshots、6 source/resolved commands、1 chunk/segment/draw；plan current；无worker/refusal；cleanup PASS | `PASS` |
| Game visual | `R8-WP01D-06-game-current.png`可见角色、武器和阴影 | `PASS` |
| Scene visual | 180×936窄viewport且Transform不代表central逻辑位置，不能裁决 | `PENDING / SEPARATE` |
| full self-check | 2026-08-23 11:53:16 PASS | `PASS` |
| ledger validator | 66 records / 65 governed code files | `PASS` |
| production fix | 本Record禁止 | `OUT OF SCOPE` |

final结构化报告为`Temp/NTSD_R8_WP01D_06_GameVisibility.result.json`，其
`firstDifference=NO_DIAGNOSTIC_DIFFERENCE`、`currentPlanValid=true`、simulation/display tick均257、
`stale=false`、`submissionBuildCurrent=true`、`currentPlanHasSubmission=true`。因此没有证据支持修改
worker publication、command materialization、resolver、URP feature或camera；006没有新建production repair。

## 5. 回滚与交接

- 回滚只删除新增Editor probe/meta并标记`ROLLED_BACK`；
- 未提交；handoff为`HANDOFF-R8-WP01D-generic-sprite-mapping.md`。
- 006完成不关闭整个`D-RENDER-006`：authored state8000 live witness、Scene View可观察性与C++ full trace
  仍是独立证据缺口。
