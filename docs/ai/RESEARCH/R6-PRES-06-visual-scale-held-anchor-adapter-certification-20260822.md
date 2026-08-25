# R6-PRES-06 — 1.5× visual scale / held anchor adapter certification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（no-code adapter certification）  
> 对应登记：`A-RENDER-002`

## 1. Authority / approved boundary

- C++ release `src/entity/game_tick.cpp:1527-1550`、`1924-1946` 以 holder/current held frame 的
  center 与首个 wpoint 写 held object 的逻辑 `x_int/y_int`；
- C++ release `src/render/renderer.cpp:558-638` 在 1× source surface 上以 `x_int/y_int/z_int`、
  center、facing 和 frame-delay phase 构造 body destination；
- 用户明确要求 Unity 保留 `BattleVisualScale=1.5f`。因此本包不是把 Unity scale 回退为 1，而是证明
  1.5× 仅改变表现尺寸，同时补偿 held wpoint 的相对视觉锚点。

## 2. Unity source mapping

| 合同 | Unity source | 结论 |
|---|---|---|
| 1.5 常量 | `NTSDRenderSpace.cs:117-122` | `BattleVisualScale=1.5f`；逻辑 pixel/world 单位仍由 `UnitsPerPixelX/Y` 决定。 |
| body pivot | `LF2ObjectRenderer.cs:545-567` | 基础 `screenX/screenY`仍使用逻辑整数位置；scale只乘 sprite center-to-pivot delta。 |
| held补偿 | `LF2ObjectRenderer.cs:582-638` | 仅在有效holder/target/link/wpoint关系下，使用`(scale-1)*(holder delta-held delta)`；left只翻X。 |
| Central path | `BattlePresentationShadowBuild.cs:2093-2097`、`2596-2611` | immutable snapshot捕获同一补偿，body command使用同一pivot；没有第二套中央公式。 |
| Legacy diagnostic | `LF2ObjectRenderer.cs:517-539` | legacy comparison renderer复用同一helper，不可作为独立authority，但可作central命令对照。 |

代数上，C++/Unity逻辑writer已经把held object放到1× wpoint重合位置；body放大到`s`后，holder与held
视觉wpoint会各自多出`(s-1)*delta`。现有补偿正好给held object加两者差值，因此不改变逻辑坐标也能
在`s=1.5`时保持wpoint重合。

## 3. Existing acceptance evidence

- `BattleRuntimeSelfCheck.CheckEntityAndShadowRenderPositionFormula`：覆盖right/left、奇数宽quarter-pixel、
  frame-delay jitter、type3 display Z、oid120/124 scale样例；
- `CheckHeldPresentationGeometryContracts`：覆盖right/left offset、invalid relation zero、central immutable
  snapshot、central-vs-legacy position、slot generation reuse与dormant holder；
- `CheckNarutoRasenganHeldPoseAndVisualAttachment`：覆盖right/left逻辑held pose与最终视觉wpoint重合；
- `CheckRenderSpaceHorizontalOriginContracts`：明确断言`BattleVisualScale`不参与逻辑pixel/world换算；
- 这些检查都由`RunAllChecksStatic`调用。fresh Assembly-CSharp为2026-08-22 19:41:38，request full
  self-check于19:49:12返回`PASS`，Editor.log当前`error CS=0`。

## 4. Decision

`A-RENDER-002`在source + fresh full self-check层认证为必要且当前一致的Unity adapter；本包不修改脚本。
它不构成真实角色/武器像素视觉验收，Play Mode中仍须检查body、held weapon、shadow在实际DAT/atlas上的
相对锚点，故最高状态为`RUNTIME_PENDING`。

## 5. Stop / reopen conditions

- `BattleVisualScale`、pixel/world换算、sprite pivot或held wpoint公式被修改；
- Central path不再复用同一helper；
- 真实Play Mode发现body/weapon/shadow相对锚点偏移；
- 任何修复试图把逻辑位置、碰撞距离或移动速度乘1.5。

