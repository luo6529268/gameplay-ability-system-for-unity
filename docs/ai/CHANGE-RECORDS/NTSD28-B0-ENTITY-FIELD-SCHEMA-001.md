# NTSD28-B0-ENTITY-FIELD-SCHEMA-001 — 核心实体字段 trace schema

<!-- CHANGE-RECORD
id: NTSD28-B0-ENTITY-FIELD-SCHEMA-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Tools/NTSD28Parity/TraceContract.cs
code-path: Tools/NTSD28Parity/TraceComparator.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
authority: User-approved NTSD28-UNITY-BATTLE-REALIGNMENT-001 B0; fixed-SHA NTSD 2.8-Logan playable source EntityState28/FrameCursor28/Position28/Motion28; Unity NTSDEntityRuntime and ECS stores.
evidence: TRACE-SCHEMA-V2 / ENTITY-FIELDS-47 / BINDINGS-VERIFIED-21-CANDIDATE-12-MISSING-14 / RELEASE-BUILD-0-WARN-0-ERROR / SELF-TEST-17-OF-17-PASS / CONTRACT-SHA256-F5E0154AC9EA68576CE4CF2515CA7A271AA71BE8537A3569822C2842DCD6900D-STABLE / DOTNET-FORMAT-PASS / GLOBAL-LEDGER-PASS / NO-RUNTIME-OR-RESOURCE-WRITE
-->

> 状态：`FOCUSED_TEST_PASS / TRACE-SCHEMA-V2 / EXPORTERS-NOT-STARTED`

## 1. 目的与边界

本包扩展独立 `Tools/NTSD28Parity` 的实体核心字段消费者合同，显式区分已确认绑定、候选绑定和
Unity 缺失绑定；不实现 exporter，不修改任何战斗 runtime，也不把 schema self-test 视为行为对齐。

## 2. Authority / Unity 原状

- Authority `EntityState28` 聚合 frame、position、motion、vital、combat、lifecycle 和大量输入、
  关系、rest、event/presentation producer state。
- v1 schema 只要求 `slot/allocationEpoch/active/objectId/objectType`，不足以定位实体状态首差。
- Unity `NTSDEntityRuntime` 有大量历史字段，但名称相似不等于语义已确认；ECS store 也是镜像/消费
  层，不能反向定义 authority。

## 3. 计划改动

- 新增 data-driven `EntityFieldContract`，冻结 JSON path/type/authority source/Unity binding/
  binding status/compare policy。
- schema 升级并把字段表纳入 canonical descriptor。
- comparator 对每个 entity group 执行 exact-property 与类型校验。
- self-test 增加缺字段、额外字段、错误类型和 descriptor binding inventory 断言。

## 4. 不可回退边界

- Slot capacity 继续按用户决定不比较，但每个实体 slot 必须落在自身 producer capacity。
- Unity-native presentation identity 只能归一成 allocation epoch，不得写回 gameplay。
- `MISSING/CANDIDATE` 不能被 comparator 自动忽略或默认填零。
- 不修改旧 2.4 parity、Unity、authority、Scene、DAT 或资源。

## 5. 实际改动与验证

- 新增 `EntityFieldContract.cs`：冻结 47 个 typed leaves，分为 root identity、identity、frame、
  position、motion、vitals、combat 和 lifecycle；所有 comparison policy 均为 `STRICT`。
- binding maturity 实测：`VERIFIED=21 / CANDIDATE=12 / MISSING=14`。候选/缺失不会被 comparator
  忽略；它们是后续 exporter/runtime 包必须关闭或暴露首差的 backlog。
- trace/descriptor/comparison/validation/self-test schema 已升级为 v2；v1 保留为首包历史证据。
- comparator 对 root 和七个 group 执行 exact-property，校验 int32/int64/boolean/finite float64，
  并拒绝 inactive entity 进入 active array。
- Release build：`0 warning / 0 error`。
- self-test：`17/17 passed`，新增 binding inventory、缺字段、额外字段、错误类型四类断言。
- contract 连续两次输出 SHA-256 均为
  `F5E0154AC9EA68576CE4CF2515CA7A271AA71BE8537A3569822C2842DCD6900D`。
- `dotnet format --verify-no-changes`、`git diff --check`、全局 Ledger validator 均通过；换行符
  仅有既有 LF→CRLF warning。
- 未实现 exporter，未修改 Unity/C++ runtime、Scene、DAT、资源或旧 parity 工具；不宣称 runtime
  parity。

## 6. 2026-09-05 current MP binding correction

`NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001`以MP200/PP173互异测试证明本记录初始
`current_mp -> Runtime.MP`候选后来被错误晋级。当前有效合同已supersede为
`EntityState28::current_mp -> NTSDEntityRuntime::PP / LF2Health::PP`；旧contract SHA仅是历史证据，
新contract SHA为`1AE87A06DD3F1C8F24A089555BBDC3151F60465E8EA67DC0C56768F54CD7EDF4`。
