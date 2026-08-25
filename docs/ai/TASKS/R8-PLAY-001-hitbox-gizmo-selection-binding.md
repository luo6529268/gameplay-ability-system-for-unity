# R8-PLAY-001 — hitbox gizmo selection binding repair

> 日期：2026-08-23
> Change ID：`R8-PLAY-001`
> 状态：`VERIFIED / EDITOR-DIAGNOSTIC-ONLY`

## Goal

修复 `NTSDHitboxGizmos.OnDrawGizmos` 在 Play Mode 中把纯 C# `LF2Entity` 当成 Unity
`Component` 查询而持续抛出 `ArgumentException` 的问题，使 R8-WP01B～D 的真实场景验收拥有
无异常污染的 Scene/Game 诊断环境。

## Scope

- 只修改 `Assets/NTSD/Scripts/Tools/NTSDHitboxGizmos.cs` 的选中对象解析；
- 从选中节点或其父/子节点取得 `LF2ObjectRenderer`，再读取其 `LogicObject` 绑定；
- 保留未选中时从 `SimulationWorld.GetAllEntities` 绘制全部实体的既有路径；
- 不修改 gameplay、输入、碰撞、命中、对象生命周期、中央渲染或场景资产。

## Authority / evidence

- R8-WP01 的 Unity Play Mode 诊断要求；
- 2026-08-23 Play Mode Console 重复异常：`GetComponent requires ... LF2Entity derives from Component`，
  定位到 `NTSDHitboxGizmos.cs:47`；
- 当前 Unity 数据契约：`LF2Entity` 是纯 C# 逻辑对象，`LF2ObjectRenderer.LogicObject` 是 Unity
  表现节点到逻辑对象的现有绑定。

## Files likely involved

- `Assets/NTSD/Scripts/Tools/NTSDHitboxGizmos.cs`
- `docs/ai/CHANGE-RECORDS/R8-PLAY-001.md`
- `docs/ai/CHANGE-LEDGER.md`
- `docs/ai/STATE.md`
- `docs/ai/HANDOFFS/HANDOFF-R8-PLAY-001-hitbox-gizmo-selection-binding.md`

## Deliverables

1. 不再调用 `GetComponentInParent<LF2Entity>()`；
2. 选中根节点、EntityModel 子节点或其后代时都能解析现有 renderer→logic binding；
3. Play Mode Console 不再产生该异常；
4. 重新检查 TestPlayer_0/TestPlayer_1 与 CentralOnly 可见性。

## Verification

- Unity scripts refresh/compile 0 error；
- `BattleRuntimeSelfCheck` fresh PASS；
- 进入 `NTSD_Battle` Play Mode，清空 Console 后等待并确认该异常为 0；
- 分别选中根/子节点时检查 OnDrawGizmos 不抛异常；
- 运行 `Tools/Validate-ChangeLedger.ps1`。

## Stop conditions

- 修复需要改变 `LF2Entity` 的继承结构或 gameplay 绑定；
- 发现 `LF2ObjectRenderer.LogicObject` 不是当前 production binding；
- 编译、自检或 Play Mode 出现新的 scope 外失败。

## Out of scope

- C++ authority 修改或运行；
- 改变 hitbox 数值、碰撞规则或 gizmo 颜色/形状；
- 修复 CentralOnly 本体、输入或技能；
- T8 默认 stage.dat 与 Android。
