# NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_STRUCTURAL_RELEASE_ATOMIC_CLEANUP
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleEntityLinkLifecycleWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6EntityLinkLifecycleCleanupProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::despawn -> clear_entity_links before slot reset/reuse; battle_world.cpp EB37E8EC; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-FIXTURE-WRITTEN-RED-NOT-EXECUTED / FOCUSED-7-OF-7 / B6-CATEGORY-15-OF-15 / RELATED-91-OF-91 / TARGETED-PLAY-7-CASES / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-HELD-ACCOUNTING / CONSOLE-0-ERROR / SCENE-BASELINE-RESTORED / ATOMIC-RELEASE-CLEANUP
-->

> 状态：`VERIFIED / FOCUSED_7_OF_7 / B6_CATEGORY_15_OF_15 / RELATED_91_OF_91 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / ATOMIC_RELEASE_CLEANUP / RED_NOT_EXECUTED`

## 实施结果

- 新增无状态`BattleEntityLinkLifecycleWriter.ClearReferencesToReleasedSlot()`，按physical slot升序扫描当前
  active occupant。它精确清理removed slot对应的child/parent Link、catch target/source及既有
  `HeldWeaponStableId`、`ThrowFrameGuard`、held/Catching reference和plain `CatcherSlotIndex`兼容镜像。
- `SimulationRegistryModule.ReleaseRuntimeSlot()`只在occupant/rest preflight与`RuntimeSlots.Release()`均成功后
  调用writer，调用位于Identity/Input/Frame/Relation/Vital generation release之前；失败release保持零关系
  副作用，同槽allocator复用前没有可观察间隙。
- `LinkState`、`TargetSlotIndex`通过现有property setter同步relation SoA/unified row。顺序保留Authority的
  child-first语义；如果同一实体先命中child分支并把Link清0，后续parent分支不会额外清Holder。
- `HolderCopySlot`、`Kind4SourceCount92`、owner、Spawner、action/motion与RNG均保持。C09/C20 invalid handler、
  positive-link pass和catch producer/settlement没有并入。
- `BattleRuntimeSelfCheck`旧P7复用夹具已纠正：释放后先断言旧holder relation被清，只有显式写入新
  `LinkState/HolderStableId`后才允许新generation holder恢复表现关系。

## 验证记录

- test-first fixture在production之前已落盘，但用户当时暂禁Unity控制；恢复其他工具授权后本轮直接完成
  production并运行，未取得可引用的behavioral RED。因此只记录`RED_NOT_EXECUTED`，不虚构失败数。
- focused job `cbf157a3d82b47ddb207ae216a73dd0d`：`7/7`。覆盖slot0/399、immediate、deferred、
  pending-destroy admission、rejected release、same-slot reuse、exact/compat/excluded sentinel、relation row与
  4096-slot warmed 0-byte allocation。
- B6 category最终job `8a33b611edd24a298075b08018bbfe15`：`15/15`；首次category job
  `59d510d7f9724337892f60ffec67c1d7`亦为`15/15`。
- runtime-slot/pending-destroy/relation/positive-link/snapshot/read-only shadow/pooled reuse七类相邻回归job
  `707bfbf97319460dabc3b73fe85a2797`：`91/91`。
- 真实`NTSD_Battle` Play runner：`7` cases通过，结果明确为
  `immediateDeferredPendingReuse=exact`、`releaseFailureSideEffects=none`、
  `warmed4096AllocationBytes=0`；退出Play后Console error为0。
- fresh `dotnet build Assembly-CSharp.csproj --no-restore`与Editor对应命令均0 error；最终完整输出的既有
  warning分别为47/129。Unity 2022.3.62f3程序集刷新后Console无编译错误。
- full `BattleRuntimeSelfCheck`首次到达旧P7 same-slot rebind断言；夹具按本Authority纠正后已跨过P7，下一
  首差恢复为独立held-CPoint injury legacy accounting断言
  `BattleRuntimeSelfCheck.cs:11403`。该后继差异不属于本cleanup。
- 验证中SelfCheck令既有用户Scene对象`HUDBg`从active1自动保存为0；已仅反向恢复该单值，并以测试前
  SHA-256 `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`、长度216762
  精确确认后重新加载。Unity最终`isDirty=false`、rootCount16；本Change不认领或改变其他Scene内容。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（422 records / 354 governed code files）。

## 未关闭项

按既定严格顺序，下一包是`NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001`；之后才处理
catch relation producer、mixed advance fences、settlement vaction preflight与held accounting。positive-link
retirement、carrier/schema、content和完整resource仍保持各自后置。

## 回滚

反向删除writer/调用与本Change测试改动；无数据迁移。不得用Git restore/reset覆盖用户工作树。
