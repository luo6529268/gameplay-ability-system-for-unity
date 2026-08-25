# R8-WP01D-06 — Game/Scene central submission first-difference diagnostic

> 日期：2026-08-23  
> 状态：`VERIFIED / EDITOR DIAGNOSTIC ONLY`  
> Change ID：`R8-SPRITEMAP-006`

## Goal

在全catalog descriptor与GPU像素均已通过后，定位真实Game画面仍没有战斗实体的第一个通用断点，
沿`published snapshot → entity/shadow command → resolver → segment/chunk → URP submission → camera`
逐层记录，不按角色、技能、OID、frame或文件名处理。

## Scope

- 新增Editor-only显式Play diagnostic；
- 枚举当前world全部claimed runtime slots；
- 对每个slot记录Entity与Shadow的reason、snapshot、command、resource、segment、submitted、position、sort；
- 记录`BattleRenderingDiagnosticReport`的command/build/submission/draw/mode/tick/refusal数据；
- 记录主相机、culling mask、正交参数与中央feature material可用性；
- 输出结构化JSON，production脚本0改动。

## Authority / Evidence

- C++ release `renderer.cpp`在live render handoff中消费已选frame/source rect，并按实体位置/层级提交；
- Unity all-DAT descriptor与全catalog GPU绑定像素已0差异，说明图片内容链不是当前Game空画面的首差；
- fresh Game截图`Temp/R8-WP01D-06/R8-WP01D-06-game.png`显示HUD/背景正常但实体不可见；
- Scene截图受当前180×936窄Scene viewport限制，只作环境证据，不裁决表现。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleCentralGameVisibilityPlayModeProbeEditor.cs`（新增）
- 对应`.meta`
- 本Task、Change Record、Ledger、STATE、register、matrix与handoff

## Deliverables

1. all-active-slot Entity/Shadow diagnostic；
2. central build/submission/draw report；
3. camera/URP material环境报告；
4. cleanup/pause恢复；
5. first-difference结论；若需production修复，拆新Change Record，不在本包直接修改。

## Verification

1. fresh compile 0 error；
2. Play Mode报告覆盖全部claimed slots；
3. JSON能明确断点位于command前、resolver、backend、submission或camera；
4. world object/claimed与pause状态恢复；
5. validator通过。

## Stop conditions

- first difference需要修改production/URP asset/scene；
- 需要角色/技能/OID/frame/file特判；
- 需要改变approved adapters或C++ authority；
- 需要人工Game/Scene输入才能继续。

## Out of scope

- 本Task不修production；
- 不修改DAT/BMP/scene/URP asset；
- 不恢复Legacy owner；
- 不处理state8000缺样本、C++ full trace、T8、Android、1000 AI、Player或服务器。

## Result

- 第一次在battle刚启动的tick 1采样得到`NO_SNAPSHOT_ENTITIES`，该结果在加入worker、pending publication、
  immutable plan字段并延后采样后被证明是过早采样，不再作为production首差；
- final报告`Temp/NTSD_R8_WP01D_06_GameVisibility.result.json`在tick 257覆盖3个claimed slots：
  published frame含3个entity snapshot，materialized plan含6个source/resolved commands、1 chunk、1 segment、
  1 draw；`currentPlanValid=true`、simulation/display tick均257、`stale=false`、无refusal/worker failure；
- final first difference为`NO_DIAGNOSTIC_DIFFERENCE`，pause与world计数恢复，`cleanupRestored=true`；
- fresh Game截图`Temp/R8-WP01D-06/R8-WP01D-06-game-current.png`实际显示角色、武器和阴影；
- 当前Scene窗口只有180×936，且logic-only中央实体的GameObject Transform不是表现逻辑位置，现有
  Scene截图不能裁决Scene View最终可观察性；该证据缺口不属于本diagnostic完成条件；
- production、scene、URP asset、DAT/BMP和Legacy owner均未修改，没有角色/技能/OID/frame/file分支；
- source 11:47:36早于`Assembly-CSharp-Editor.dll` 11:47:58，C# compiler error为0；
  11:53:16 full `BattleRuntimeSelfCheck=PASS`；ledger validator为66 records / 65 governed files PASS。
