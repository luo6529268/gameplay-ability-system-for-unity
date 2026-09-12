# NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001

Goal20 R5；`IN_PROGRESS / TEST_FIRST`。本包只退休 `HolderCopySlot` 的残余生产行为 writer 与 semantic reader，保留 runtime/task carrier、canonical copy、snapshot、ECS Links/fingerprint、lockstep checksum 和 parity 结构。R3 所有 `LF2WeaponBase.cs` 生产 hunk 与 R4 所有 `LF2WeaponReleaseFlowResolver.cs` 生产 hunk 由主代理在对应包结束后集成；本包不直接改动这两个共享文件。

## Authority 与 fresh inventory

- Authority：`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001 / VERIFIED`，正式 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`，对应 playable closure `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- Audit 的 Authority 结论是 HolderCopy 没有单一对应字段；linked parent、owner、battle group、control 和 physical slot 分别由 `HolderStableId`/`LinkState`、`OwnerSlotIndex`、`RelationTeam`、`AnimCounter` 和 `SlotIndex` 表达。
- R1 之后对 `Assets/NTSD/Scripts/` 做不区分大小写 fresh exact scan：production 为 52 行/22 文件，test 为 132 行/20 文件。现有审计记录的历史基线为 63 production/26 文件；差额记录在 `Temp/Goal20_R5_Callgraph.json`，不能用历史计数反推当前调用点。
- R1 已将 kind5/frozen pair/nearest/negative-link 的旧 raw reader 改为 `Runtime.HolderStableId`/implicit-zero；当前这些调用链直接读取 HolderCopy 的数量为 0。B5 kind5、B5 type3 和 Goal16 P3 kind7 三条既有路线作为已完成前置，不在 R5 重做。

## 精确残余范围

当前 production exact callgraph 与分类详见 `Temp/Goal20_R5_Callgraph.json`。R5 的行为退休集合为：

1. `LF2Character.InitializeFromOpoint` 的三处 task/parent HolderCopy propagation；`LF2ObjectPointFactory.PostInitLiving` 与 `BattleLogicEntityFactory.PostInitLiving` 的 parent propagation。
2. `LF2Entity.FillHitFa8SpawnTask`、`FillHitFa13SpawnTask` 的 task propagation，以及 stage self-slot、state9996、OID5152、death/release 的运行期 HolderCopy producers。
3. `LF2CharacterWeaponLinkResolver.HoldWeapon` / `AttachOpointHeldObject` 的 held mirror；`LF2WeaponBase.Pick`、`InitializeParent` 与 `LF2WeaponReleaseFlowResolver.ClearWeaponHolderRuntime` 的 hunk 由 R3/R4 完成后集成。
4. `LF2HitResolveRuntimeData.ResolveHolderCopyEntity` 及 `BattleDamageWriter` 四个调用点。HolderCopy 只导向 `holder.KillStat++` 和 `holder.ComboCountAtk += injury/reducedInjury`；相邻 native HP/PP/InputHpConsumedTotal/ComboCountVic、`ApplyNativeStandardHitKnockout`、`ApplyNativeStandardHitCreditAndConsume` 与独立 world/victim 统计保持原状，不猜测新的 owner。
5. `LF2CharacterHitResolver.ResolveHolderCopyEntity` 私有死 helper，以及 `BattleEcsHitExecutionPlan` 中用 HolderCopy 构造 legacy `HolderHandle`/holder-stat projection 的读取。HitPlan 的 `TargetHolderCopySlot` 字段、capture 和 comparison mask 保留为 diagnostic carrier。

真正的 declaration、task initialization 和 Reset 默认写保留：runtime/character 为 99，task/weapon/other 初始化为 -1。数值虽为默认但属于运行期行为的 state9996/OID5152/death/release writers 仍属于退休集合，不能泛化为保留项。

## 允许修改与明确排除

允许的生产 hunk 限定为上述 exact callgraph 中的 HolderCopy 行；保留同方法的 relation、owner、group、control、action、motion、RNG、HP/PP、生命周期和顺序。新 focused test 为：

`Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyHolderCopyResidualRetirementEditorTests.cs`

及其 `.meta`。证据只写入 `Temp/Goal20_R5_*`。

禁止修改：

- `NTSDEntityRuntime`、`OPointCreateTask`、`LF2Entity.HolderCopySlot` property 的结构与默认；
- `BattleEcsWorld` 的 HolderCopy array/copy/reset/consistency/fingerprint 结构；
- `BattleParitySnapshot`、`BattleLockstepChecksumModule` 的 carrier/schema 位置；
- HitPlan `TargetHolderCopySlot` snapshot shape；
- schema 目录、`NTSDSpec.cs`、`Gen/`、`Plugins/`、content、Prefab、Scene、`+0x2F8`；
- relation 本体（`LinkState`、`TargetSlotIndex`、`HeldWeaponStableId`、`HolderStableId`、`OwnerSlotIndex`、`RelationTeam`）及 R3/R4 尚未释放的共享 hunk。

不修改 Ledger、STATE、handoff 或对齐总表；由主代理在包集成时登记。不得执行 `git add`、`commit`、`push` 或破坏性回退。

## Test-first 与验收

1. 新 focused 先以 source guard 锁定残余动态 writer/semantic reader，并用 `HoldWeapon` 与 `AttachOpointHeldObject` 的 canonical relation fixture 证明 HolderCopy writer 可达；当前应为 RED。
2. 生产退休后，focused 必须证明所有 runtime producer 只留下现有 lifecycle default，四个 damage caller 不再解析 HolderCopy，HitPlan legacy holder projection 不再读取 HolderCopy；canonical relation、native damage/vital、world/victim stats、RNG、action/motion 和 object lifecycle 不变。
3. carrier test 必须确认 runtime default、task field/Clear、canonical copy、ECS Links/fingerprint、checksum/parity 和 HitPlan `TargetHolderCopySlot` 结构仍存在且可传递；不修改其 layout。
4. 主代理随后执行 R1-R5 批末共享 focused、B6/前置 focused、refill9、SelfCheck、双 build、validator、Console/Scene SHA。没有这些结果，R5 只能报告 `RUNTIME_PENDING`。

## 风险、硬停与回滚

- 四个 damage caller 的 HolderCopy 依赖只产生旧 holder `KillStat`/`ComboCountAtk` 额外写；不得删除相邻 native/world/victim 统计，也不得自行抽取替代 owner。
- 如果 fresh scan 发现 `Temp/Goal20_R5_Callgraph.json` 之外的 semantic reader/writer、需要改 carrier/schema/checksum/ECS structure、或出现无法由 Authority 解释的 current gameplay 变化，立即硬停。
- R3/R4 共享文件的 hunk 只能在其现有包结束后由主代理集成；本包不覆盖其未提交修改。
- 回滚须经明确批准，只恢复本包对应 hunk 与 focused evidence，不使用 `git restore`、`reset`、`clean` 或 `rm`。

当前状态：`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED_FOR_R5`；Task/Record 与 fresh callgraph 已建立，RED focused 待运行，Unity/build/Play 由主代理执行。

Final status VERIFIED within declared behavior scope. RED 7FAIL/1PASS; focused157/157; fresh production references52/22files to29/13files, only defaults/carrier/diagnostics remain. Closure/limitations and exact evidence: corresponding Change Record and Temp/Goal20_FinalSummary.json. Earlier NOT_RUN progress is superseded; original broad failure retained and reconciled with24-case focused recheck.
