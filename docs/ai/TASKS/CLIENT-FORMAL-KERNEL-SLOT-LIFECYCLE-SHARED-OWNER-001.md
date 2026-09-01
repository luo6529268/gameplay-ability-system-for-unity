# Task Contract — CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001

> 状态：`FOCUSED_TEST_PASS / SHARED_SLOT_LIFECYCLE_OWNER_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`
> 创建：2026-08-30

## 1. 目标

把已经dependency-closed、平台无关的`RuntimeSlotAllocator`、`RuntimeEntityHandle`和`RuntimeSlotLifecycleState`唯一production source及原GUID从Client移动到Server-owned `packages/com.ntsd.battle-kernel/Runtime/Core`。Unity与.NET必须编译同一物理源码，同时保持现有namespace/API、Authority400 allocation、本地Generation lease、canonical allocationEpoch和Client adapter行为。

本包只关闭Cut C shared source ownership，不实现完整formal BattleKernel，不修改Client adapter/Registry/Rest、snapshot/recovery、formal AI或marker，也不晋升S0/S5。

## 2. 精确允许范围

- 移动并保留GUID：
  - `RuntimeSlotAllocator.cs`：`3f48ed724a794e0a99073634df7ae654`；
  - `RuntimeEntityHandle.cs`：`35a1af57f8e14b27a120ff236130d1ab`；
  - `RuntimeSlotLifecycleState.cs`：`92265319af00462f90e1a367b8ec3584`。
- 更新Server package Core SDK project、`0.3.0`统一release tuple、direct/locked artifact consumers和package README。
- 增加同一48-line golden journal的Unity package/.NET consumers。
- API可见性变化只限 unchanged Client adapter在新assembly边界上已经调用的成员。Preflight识别`SetOccupancyEpochForSelfCheck`；首次Unity compile又精确证明`InvalidateLocalLeaseForWorldReset`、`BeginTopologyRestore`、`SetLocalGenerationForTopologyRestore`、`TryRestoreCommittedClaim`、`CompleteTopologyRestore`五个现有调用需要公开。六个方法只能由`internal`改为`public`，签名、方法体、调用顺序和snapshot/lease语义不变。
- 更新双仓库治理文档。

## 3. 冻结文件和语义

禁止修改`RuntimeSlotTable.cs`、`SimulationWorld.Registry.partial.cs`、`SimulationQueryAndLinkModule.cs`、`RuntimeRestStore.cs`、`BattleParitySnapshot.cs`以及所有既有Client runtime/test caller。Client manifest/lock/asmdef仅在Unity出现精确assembly metadata失败时才可先记录后修改；预期无需修改。

## 4. 必须保持

- 每个类型只有一个Server-owned production source并保留原GUID。
- `NTSD.Simulation` type identity不变；`0/20/50`、ascending first-free、Generation、occupancy epoch、commit-only allocationEpoch语义不变。
- 48-line payload与SHA-256 `22F25272BCD5E4616AFB92B50A6E080E546B6AA53A11DAB96647387F1C4381B7`原样由Unity/.NET消费。
- Core保持BCL-only/no-engine；不得引用LF2/Unity/Protocol/transport/recovery/formal AI。
- UPM/Core/Abstractions/artifact consumer统一为`0.3.0`；formal marker仍false且无diff。

## 5. 验证

先取得pre-move失败证据，再移动source。随后运行single-source/GUID/purity/version审计、.NET Debug/Release及exact locked artifact、Unity compile、package slot lifecycle、既有slot/lifecycle focused tests、BattleRuntimeSelfCheck、S0 8/8、lockstep 9/9、Server Debug/Release/full/no-network及双Ledger/workflow/matrix/diff检查。

## 6. 禁止

禁止修改RuntimeSlotTable adapter、Registry、Rest、battle rules、30 Hz、Scene、资源、Input Actions、TargetTick/InputDelayFrames、transport、Socket、数据库、公网、snapshot/recovery、formal AI、S1 wire和formal marker。禁止duplicate/generated source和S0/S5 VERIFIED claim。

## 7. 回滚

将同三份物理源码和GUID移回原Client路径；恢复Core project和release tuple为`0.2.0`；只移除本包vector/consumer。保留RNG Cut A、FrameInput Cut B、StageSpawn correction、closed seam和全部无关用户改动。

## 8. 完成结果

- 三个类型和GUID均只有一个Server-owned `Runtime/Core` production owner，原Client路径已删除。
- shared source只把六个既有lifecycle方法由`internal`提升为`public`以跨assembly消费；方法体、签名、调用顺序和语义未变，冻结的Client adapter/caller未改。
- `0.3.0` direct Debug/Release与exact locked artifact consumers、48-line journal、Unity compile/package/slot regressions、S0 witness、lockstep、fresh SelfCheck和Server全回归均通过。
- 本包只关闭Cut C shared ownership；不证明完整formal Kernel、S0/S5 `VERIFIED`、snapshot/recovery、formal AI或marker promotion。
