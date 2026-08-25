# R8-PLAY-001 — hitbox gizmo selection binding repair

<!-- CHANGE-RECORD
id: R8-PLAY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Tools/NTSDHitboxGizmos.cs
authority: R8-WP01 Play Mode diagnostic requirement; observed Unity ArgumentException; LF2ObjectRenderer.LogicObject binding
evidence: 2026-08-23 MCP read_console repeated exception at NTSDHitboxGizmos.cs:47
-->

## 1. Authority / requirement source

本包不是 C++ gameplay 对齐改动。依据是 R8 真实 Play Mode 验收必须在无诊断脚本异常污染的环境中执行，
以及 Unity 当前 renderer→logic 绑定合同。C++ authority 保持只读且不参与该 Editor 工具修复。

## 2. Unity original state / first difference

`showOnlySelected` 分支调用 `selectedObject.GetComponentInParent<LF2Entity>()`。当前 `LF2Entity` 是纯 C#
逻辑对象，不继承 `Component`，Unity 因此在 Scene Gizmo 重绘时持续抛 `ArgumentException`。非 selected
分支已经通过 `SimulationWorld.GetAllEntities` 获得逻辑对象，没有同类错误。

## 3. Planned paths and symbols

- `NTSDHitboxGizmos.OnDrawGizmos`
- 如有必要，新增仅负责从选中 GameObject 读取现有 `LF2ObjectRenderer.LogicObject` 的私有 helper

## 4. Intended behavior / side effects

- 选中 renderer 自身/其后代时向上解析 `LF2ObjectRenderer`；
- 选中实体根节点时向下解析 renderer；
- 只在逻辑绑定是 `LF2Entity` 时绘制该实体；
- 不分配/注册/修改实体，不写回逻辑或表现状态。

## 5. Non-regression boundaries

- 不修改 `LF2Entity`、`LF2ObjectRenderer` 或中央渲染；
- 不改变所有实体绘制路径、碰撞盒数学或战斗时序；
- 保留所有既有用户改动和脏工作树；
- 不修改场景、资源、C++ authority、T8 或 Android。

## 6. Acceptance criteria

- Unity 编译 0 error；
- fresh self-check PASS；
- Play Mode 中该异常不再出现；
- TestPlayer 与 CentralOnly 的后续检查可继续；
- ledger validator PASS。

## 7. Rollback

若验证失败，只回退本 Change ID 在 `NTSDHitboxGizmos.cs` 的局部 selection-binding 增量，保留记录并
标记 `BLOCKED` 或 `ROLLED_BACK`；不得回退用户/R2～R8其他改动。

## 8. Actual change / verification

- `OnDrawGizmos` 的 selected 分支不再对 `LF2Entity` 调用 Unity component query；
- 新增 `ResolveSelectedEntity`：先向父级查找现有 `LF2ObjectRenderer`，再向子级查找，以同时覆盖
  EntityModel/后代选择与实体根节点选择；只读取 `renderer.LogicObject as LF2Entity`；
- 非 selected 的 world snapshot 路径、碰撞盒数学与 gameplay 未改；
- 代码写入阶段曾为 `CODE_WRITTEN`；其后的fresh证据见下一节。

## 9. Verification result

- Unity force scripts refresh完成domain reload；reload后MCP恢复，Console `error/warning = 0`；
- `Temp/NTSD_BattleRuntimeSelfCheck.result` 于2026-08-23 07:40:57写入`PASS`；
- 清空Console后进入`NTSD_Battle` Play Mode等待15秒，原异常为0，全部error/warning也为0；
- hierarchy确认两个active `EntityObject(Clone)`及其`EntityModel/LF2ObjectRenderer` binding存在；
- `Tools/Validate-ChangeLedger.ps1`：PASS，57 Records / 56 governed code files；
- scoped `git diff --check`无错误，仅报告工作树LF→CRLF提示。

因此本Editor diagnostic-only修复记为`VERIFIED`。这不验证物理输入、组合技、CentralOnly实际可见性或
C++ gameplay对齐；用户随后报告的组合键失败属于独立`D-INP-006 / R8-WP01B`问题。
