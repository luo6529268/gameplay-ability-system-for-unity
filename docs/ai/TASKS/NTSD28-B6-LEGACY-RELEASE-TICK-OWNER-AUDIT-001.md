# Task Contract — NTSD28-B6-LEGACY-RELEASE-TICK-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_FIELD_OR_READER / TWO_PACKAGE_SPLIT / CURRENT_RELEASE_WITNESS / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-OWNER-AUDIT-001 / VERIFIED`

## 目标

审计Unity `NTSDEntityRuntime.ReleaseTick`的当前2.8 Authority对应物、全部生产writer/reader、checksum与
snapshot影响；将立即可做的动态producer退休与需要兼容方向的carrier/schema disposition拆开。

## Authority 与来源

- 当前 playable closure 的`EntityState28`没有release-tick字段；`settle_held_refill_objects()`的refill
  exhaustion、DVX与kind3 release都不写world tick或等价值。
- Authority relation release仅写action/motion/relation及分支专属字段；没有同tick suppression reader。
- 全closure检索没有release-tick/throw-tick token或通过等价字段读取“本tick刚释放”的逻辑。
- Unity字段由历史commit `f03773f3`在旧“C# authority parity tooling”阶段引入；该历史不能裁决当前规则。

## Unity production closure

### Writers

- `LF2WeaponHeldStateResolver.ThrowHeldWeapon()`通过
  `ReleaseHeldWeaponRuntimeInternal(..., stampReleaseTick:true)`写当前world tick；
- `BattleHeldObjectWriter.ThrowHeldObject()`及real/generic `DropRandomly()`写当前tick；
- `LF2WeaponReleaseFlowResolver.ReleaseHeldWeaponForConsume()`在OID122/123 exhaustion无条件写当前tick。

### Readers / propagation

- gameplay、AI、collision、hit、frame、lifecycle与presentation生产代码对`ReleaseTick`的读取为0；
- `NTSDEntityRuntime.TryCopyCanonicalStateTo()`复制它，`Reset()`恢复-1；full entity snapshot通过canonical copy
  隐式携带；
- `BattleRuntimeFingerprint.Compute()`、`BattleLockstepChecksumModule`与`BattleParitySnapshot`把动态值纳入
  fingerprint/checksum/JSON；因此即使不改gameplay，它也会制造同seed/input/tick首差；
- `ThrowFrameGuard`是另一个无正值生产者的legacy carrier，不属于本包，禁止顺手删除。

## Current reachability

在完整41-source/668-edge union并先应用terminal/missing-action正确优先级后：

- target action有效的non-kind3 DVX release有2,125 holder-frame/edge rows（仅target type1/2/4/6）；
- target action有效的kind3 release有11,116 rows，其中Sasori OID51→type3 OID213 actions396/399两条
  证明generic kind3 production也可达；
- OPoint-held OID122/123 exhaustion覆盖3+35个distinct source-target edges；
- 当前实现还会在missing-action错误路径stamp，但该部分由前一owner先截断，不能作为保留字段的理由。

这些是formal current-content witnesses；动态`ReleaseTick=tick`会立刻改变Unity checksum/parity。真实输入
Play仍待验。

## 两个后继包

### 1. Producer retirement

`NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-001`

- 删除real/generic/consume的所有当前tick写入与`stampReleaseTick`参数；
- 暂时保留`ReleaseTick`字段、canonical copy与schema位置为恒定legacy reserved `-1`，避免本包改变
  snapshot/recovery格式；
- 更正旧`BATTLE-AUDIT7/AUDIT9`测试：DVX、kind3、consume、damaged-drop和generic都必须保持-1；
- 不改relation、RNG、action、motion、terminal、missing-action或release调用次数。

### 2. Carrier/schema disposition

`NTSD28-B6-LEGACY-RELEASE-TICK-CARRIER-DISPOSITION-001 / USER_DIRECTION_REQUIRED`

- 选项A：删除字段/copy/reset/fingerprint/checksum/parity key，并按现有append-only规则升级entity snapshot
  `12→13`、full snapshot `19→20`、checksum `22→23`及全部兼容测试；
- 选项B：明确保留reserved `-1`字段以维持snapshot形状，但从authority-facing parity中排除并记录兼容期限；
- 该选择改变snapshot/recovery/checksum schema policy，按standing Client authorization边界必须取得用户方向，
  不能由普通B6 bugfix代替决定。
- 后继owner audit又确认`WeaponState`、`TrackerFlag/TrackerParent`、`GrabbedBy`和`HolderCopySlot`同属待退休
  legacy carrier。若用户选择删除，必须以一个联合迁移一次完成entity `12→13`、full `19→20`和
  checksum `22→23`；`GrabbedBy`本身未进checksum，22→23来自本字段与`WeaponState`的已有checksum语义删除。

## 验收矩阵

- producer包：real DVX type1/2/4/6、kind3 real/generic、OID122/123 exhaustion、damaged-frame/no-release；
- 每个case以非默认sentinel初始化，证明writer不再改值；同时relation/action/motion/RNG输出与对应Authority合同一致；
- source guard证明除reserved field/copy/checksum位置外生产write为0，gameplay read仍0；
- checksum A/B：只改变旧dynamic release tick时，producer retirement后同一正式状态不得分叉；
- carrier包若获批，覆盖snapshot capture/restore/schema mismatch、history/session checksum、parity JSON key与4096
  warmed copy/hash 0 B；
- compile、focused、B6 regression、SelfCheck、Sakura/Rock Lee/Sasori/OID122/123 Play和joint trace。缺运行证据
  时最多`RUNTIME_PENDING`。

## 不变量 / 阻塞

- 不修改Authority、Config/Scene/Prefab/resource/importer、pass placement、RNG、relation字段或shutdown顺序。
- producer retirement不升级schema、不删除field；carrier disposition未获用户方向前不实施。
- 当前Unity runtime栈未清，不叠加producer代码；本轮只做owner审计。

## 回滚

仅移除治理记录；没有脚本、content、Scene或Authority回滚。
