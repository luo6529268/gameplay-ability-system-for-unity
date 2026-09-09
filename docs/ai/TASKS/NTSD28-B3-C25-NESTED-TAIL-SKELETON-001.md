# Task Contract — NTSD28-B3-C25-NESTED-TAIL-SKELETON-001

> 状态：`VERIFIED / C25-SINGLE-PRODUCTION-ENTRY / COMPLETED-TICK-RENDER / TARGETED-PLAY-PASS / C25A-P-BEHAVIOR-PENDING`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25`
> 依赖：`NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001 / VERIFIED`

## 目标

把现有动态升序 late-slot loop 建立为 Authority C25 的唯一生产入口：正常完整 tick 中必须紧接 C24 执行；legacy serial remainder 明确后置且不再冒充 C25；Stage/Session tail/Results 完成后才发布 Render snapshot。

## 本包只做

- 调整 `NTSDBattleTickSystem` 的 production caller placement，不改 late loop 内部算法。
- 保留现有 `BattleLateEntityLifecycleModule` 的动态 `runtimeSlot++` / current occupant 查询和 deferred mutation实现；C25o exact逐槽销毁留 K-P 包。
- 正常完整 tick 顺序锁定为 C23→C24→C25 skeleton→legacy serial remainder→Stage→random weapon tail→entity post-tail→Results→Render。
- step-wait 路径保持原有“执行 legacy serial、Stage、Render后返回，不执行 late/post/results”的行为，本包不重判 Host/step语义。
- 新增 focused phase test与真实 Play probe；更新只因本 placement 改变而过时的 phase断言。

## 明确不做

- 不实现或宣称 C25a～p具体行为完成。
- 不改 `SerialTickAll` body、type3旧逻辑、state9998 cleanup、C15用户随机掉武器算法、Stage/Results算法。
- 不改资源、Scene、Prefab、ProjectSettings、shared Server package或 Authority目录。
- 不安装 hook，不提交/推送。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C25NestedTailSkeletonEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C25NestedTailSkeletonPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomeEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅新phase contract）
- 本Task/Record、Ledger、STATE、handoff、总表及B3/C25 manifests。

## 验收

- test-first 的新 focused test 在旧顺序失败，实施后通过。
- full tick仍34 occurrences；index25/26为C23/C24，27为`LateEntityUpdate` C25 skeleton，28为legacy `FrameAdvance`，33为`RenderDispatch`。
- input-clear partial仍4 occurrences；step-wait旧行为有独立断言且未被意外改变。
- 既有 W05 high/low live-slot、worker/snapshot相关 focused测试通过。
- Unity compile 0 error、SelfCheck、真实 Play phase probe通过；Play退出、Scene SHA/mtime不变、Console 0 error。

## 回滚

恢复 `NTSDBattleTickSystem` 中三个 caller 的原位置并删除新增 focused/probe；不得回退 C01～C24。

## 完成证据

- test-first job `69abe6d0d3ff42648a010562ba7b5908`：2项中1项按预期失败，旧index27为`FrameAdvance`而不是C25；step-wait保留项通过。
- production正常路径现为index25/26 C23/C24、27 C25 `LateEntityUpdate`、28 legacy serial、29 Stage、30 random tail、31 entity post-tail、32 Results、33 Render；总数仍34。
- focused `a1d8242b270b47ea848dff075002ca30` 2/2，最终复核`cc2a8c85af744b2783edc89dc8dd671e` 2/2。
- placement/W05/worker job `f634d9b90b5249c8b2a552d19b7e305e` 51/51；presentation/results/OID相关job `39c7c76d7689422c8eefbe6a7eeaf483` 14/14；NTSD28 broad job `f56031d148764c5bb5fe42bbf32945c9` 123/123。
- SelfCheck 2026-09-05 06:38:37 +08 `PASS`；Unity compile/Console 0 error。
- 真实Play tick6为完整34项且publishedTick=6；artifact SHA-256 `8AE8AB883FEF85617ABD43854B98D46A5EFC17F2C7D81D78CE2286A90430D2F5`。
- Play已退出；Scene SHA `0D74E174...23D77`与mtime不变；Authority目录零写入。
- 本包只关闭C25入口/placement与completed-tick Render边界；下一包为C25a-b definition/special clone，C25c-p仍未完成。
