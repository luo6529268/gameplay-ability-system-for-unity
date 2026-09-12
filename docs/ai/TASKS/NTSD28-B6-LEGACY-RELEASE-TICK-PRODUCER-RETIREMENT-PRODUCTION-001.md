# NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-PRODUCTION-001

Goal20 R4；`IN_PROGRESS / TEST_FIRST`。本包只退休 Unity 生产路径中把当前 world tick 写入 `NTSDEntityRuntime.ReleaseTick` 的 legacy producer，保留 reserved carrier `-1` 以及现有 canonical copy、snapshot、ECS fingerprint、lockstep checksum 和 parity 结构。

## Authority 与现状

- Authority 依据：`NTSD28-B6-LEGACY-RELEASE-TICK-OWNER-AUDIT-001 / VERIFIED`；正式 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`，source closure 由 `source/README_SOURCE.md` 声明的 `source/ntsd28_playable/scripts/build.ps1 -Target playable` 定义。
- Authority `EntityState28` 与 `BattleWorld28::settle_held_refill_objects()` 没有 release-tick 字段、writer 或同 tick suppression reader；Unity 生产 gameplay reader 仍为 0。`NTSDEntityRuntime.ReleaseTick`、canonical copy/reset、ECS fingerprint、checksum 和 parity/snapshot 位置是本包必须保留的 carrier/schema。
- 审计冻结的 current-content witness：有效 target-action 的 non-kind3 DVX release 为 2,125 holder-frame/edge rows；有效 kind3 release 为 11,116 rows，其中包含 Sasori OID51→generic type3 OID213 actions396/399 两条；OPoint-held OID122/123 exhaustion 覆盖 3+35 个 distinct source-target edges。它们是测试可达性合同，不是本包新增内容数值。

## Fresh callgraph 基线

在 R2 已完成两个共享生产文件的 `GrabbedBy` 改动后重新扫描 `Assets/NTSD/Scripts/`：

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs`：`ReleaseHeldWeaponRuntime` 中 1 个 `Runtime.ReleaseTick = Match.CurrentTickIndex` 条件写入；`ReleaseHeldWeaponForConsume` 中 1 个无条件当前 tick 写入。
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs`：`ClearLinks` 中 1 个 `Runtime.ReleaseTick = held.Match/holder.Match.CurrentTickIndex` 条件写入；`ThrowHeldObject`、`DropRandomly` 共 3 个 `stampReleaseTick:true` 路由调用。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs` 仍有 1 个向 `ReleaseHeldWeaponRuntimeInternal` 传入 `stampReleaseTick:true` 的调用，`LF2WeaponBase.cs` 仍有 1 个兼容转发参数；二者不在本包授权生产路径内，不能顺手改动。删除两个允许文件中的实际赋值后，这些参数只保留为无效兼容 plumbing，不再形成 ReleaseTick producer。
- 其余 `ReleaseTick` 命中属于 carrier declaration/copy/reset、checksum/parity/snapshot propagation 或 tests/fixtures；不得删除、改名或改 schema。

## 精确范围

允许修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs`：删除两个当前 tick 写入语句，保持 held relation teardown、consume target reset、holder slot 与 action/motion 顺序不变。
2. `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs`：删除 `ClearLinks` 的当前 tick 写入语句，保持 DVX throw、kind3 real/generic drop、damaged drop、RNG、relation clear 与 release 调用次数不变。
3. 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyReleaseTickRetirementEditorTests.cs` 及 `.meta`；测试必须使用现有 current DVX/kind3/consume fixtures 或实际 shared writer 调用，不能只用合成直接 helper 证明可达性。
4. 新增 `Temp/Goal20_R4_*` callgraph、RED、focused 与 source-guard 证据。

禁止修改 schema、snapshot/checksum/parity/ECS fingerprint 结构、`NTSDSpec.cs`、`Gen/`、`Plugins/`、content、Prefab、Scene、`+0x2F8`，以及其他生产脚本；不修改 Ledger/STATE/handoff/对齐总表。不得 git add/commit/push。

## Test-first 与验收

1. 生产修改前，以当前可达 fixture 先证明 DVX/kind3/consume 路径能够把非默认 sentinel 改成动态 tick，并保存 `Temp/Goal20_R4_RED_*`；RED 必须覆盖 real DVX、kind3 real/generic、OID122/123 exhaustion 和 damaged/no-release，且记录 relation/action/motion/RNG 观察值。
2. 生产修改后，focused 测试必须证明上述 current witness 路径的 `ReleaseTick` 始终为 `-1`，同时 `LinkState`、`HolderStableId`/`TargetSlotIndex`、action/frame、velocity、RNG 消费和 cleanup 与既有合同一致；carrier copy/fingerprint/checksum/parity/snapshot 结构可继续读取 `-1`。
3. source guard 必须确认两个允许生产文件无 `Runtime.ReleaseTick =` 动态写入；允许的 declaration/copy/reset 及 checksum/parity/snapshot 位置不受影响，并明确列出范围外惰性 `stampReleaseTick` plumbing。
4. 该子包只执行 focused/source guard 与必要的静态检查。Unity refresh、编译、Play、shared B6/NTSD28 regression、full SelfCheck、双 build、validator、Console/MinMaxAABB/Overlap 和 Scene SHA 由主代理串行执行；没有这些证据时本包最多 `RUNTIME_PENDING`。

## 不变量、硬停与回滚

- `ReleaseTick` reserved 默认固定为 `-1`；不删除 carrier，不改变 copy/reset/schema/checksum/parity key 和字段顺序。
- 不改变 Authority relation 本体、pass 顺序、RNG、frame/action、physics、DVX/kind3/consume release 调用次数、damage/drop 生命周期或 shutdown 顺序。
- 若 fresh callgraph 发现其他动态 writer、semantic reader、超出授权清单的调用点，或需要修改禁改结构文件，立即硬停并报告。
- 若 focused 发现 current gameplay 变化且无法由 Authority 解释，立即硬停。既有测试只有在本包退休语义范围内才可修订；类外冲突交主代理处理。
- 回滚须经明确批准，仅按本包变更块恢复；不使用 `git restore/reset/clean/rm`，不覆盖其他代理工作。

当前状态：`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED`。Task 建立在生产改动前；fresh callgraph 已保存，RED/focused/Unity runtime/shared gates 待执行。

Final status VERIFIED within declared behavior scope. RED 9FAIL/4PASS; focused13/13; current Play4/4 only retired carrier differs. Closure/limitations and exact evidence: corresponding Change Record and Temp/Goal20_FinalSummary.json. Earlier NOT_RUN progress is superseded; original broad failure retained and reconciled with24-case focused recheck.
