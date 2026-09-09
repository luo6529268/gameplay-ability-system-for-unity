# Task Contract — NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_PARALLEL_STATE / BEHAVIOR_RETIREMENT_DEFINED / CURRENT_OID124_WITNESS / CARRIER_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-LEGACY-RELEASE-TICK-OWNER-AUDIT-001 / VERIFIED`

## 目标

审计 Unity `NTSDEntityRuntime.WeaponState` 的当前 2.8 Authority 对应物、全部生产 writer/reader、
实际战斗副作用、Direction-B 可达语料及 snapshot/checksum 影响；把可以按 Authority 退休的旧行为与需要
兼容策略方向的 carrier/schema disposition 分开。

本 Task 只修改治理记录，不修改 C#、Config、Scene、Prefab、资源、ProjectSettings、Server 或 Authority。

## Authority 合同

- 当前 playable closure 的 `EntityState28` 没有与 Unity `WeaponState` 等价的独立可变武器状态。
  `runtime_state_code` 属于 mode/story runtime，`frame_state_code` 是诊断镜像；两者均不是第二套武器 frame state。
- `BattleWorld28::settle_held_refill_objects()` 的 held follow/release 只写 child `frame.action`、motion、relation
  及各分支明确字段；type1/4/6 DVX 写 action40，type2 写同步 RNG 选出的 action0..5。它不另写 1001/1002
  武器状态。
- `NativeAi28::step_non_character_hit_fa()` 每次从 `definition->frame(frame.action)` 读取 `hit_Fa`，所有 state
  判断也通过同一 current frame 读取。当前 C02 不存在 `1002→2000→3000` 的平行状态迁移，也不存在每 tick
  把 Vx 除以二的前置分支。
- `PhysicsIntegrator28`、hit/landing 与 pickup 同样读取实际 current frame state；没有 reader 把独立 runtime
  state 覆盖到 frame state。
- Authority closure 没有 Unity prelude 的另一条规则：type4/6、state1000、`|Vx|>9` 时预先切 action40。
  当前语料下它因 C02 只调度 positive `hit_Fa` frame 而 dormant，但仍不是可保留的当前规则。

## Unity production closure

### 来源

- `WeaponState` 由历史 commit `8101df55` 在旧 battle runtime 移植中作为 `GetState()` 镜像加入；
- commit `9612bf2f` 将它改成 held/throw 独立 writer，commit `12949da0` 又新增独立 pre-frame 状态机；
- 这些提交早于当前 NTSD 2.8-Logan authority migration，不能裁决当前规则。

### Writers

- `LF2WeaponHeldStateResolver.Drop()` 写 0 两次；
- `Act()` 的 held follow 写 1001，`ThrowHeldWeapon()` 写 1002，damaged-drop 写 0；
- `LF2WeaponFrameLogicResolver.RunWeaponFrameLogicBeforeAdvance()` 写 2000，随后在衰减阈值内写 3000；
- `LF2WeaponBase.Reset()` / `NTSDEntityRuntime.Reset()` 写默认 0，canonical copy复制字段。

generic held writer 不写该字段，因此同一 DAT 行为还会因 CLR shell 类型不同而出现额外差异。

### Readers 与传播

- 唯一 production gameplay reader 是
  `LF2WeaponBase.ResolveRuntimeWeaponState() -> GetRuntimeWeaponState()`，且唯一调用者是
  `LF2WeaponFrameLogicResolver`；pickup 的 `GetResolvedWeaponStateForExternalUse()` 已独立读取 current frame，
  不读 `WeaponState`。
- prelude 在 current hit_Fa 行为之前执行：state1002 先把 carrier 写2000但本 tick 不衰减；后续 carrier2000
  tick先把Vx乘0.5，绝对值小于0.5时再写3000，然后才执行 hit_Fa 追踪。
- 字段进入 `NTSDEntityRuntime.TryCopyCanonicalStateTo()`、隐式 full entity snapshot、
  `BattleRuntimeFingerprint`、`BattleEcsWorld` fingerprint、lockstep checksum与parity JSON；动态值即使尚未改变
  motion，也会制造同 seed/input/tick 的 checksum/parity 首差。
- self-check 中 `FL-WEAPON-STATE` 明确断言旧 `1002→2000→3000` 和 Vx halving；held throw 测试也断言1002。
  这些是旧实现锁定测试，不是当前Authority证明。

## Current / release 可达证据

### frozen Direction-B projection

在当前 `data.txt` indexed 的 type1/2/4/6 definitions 中：

- positive `hit_Fa` 只有 OID124 `chars/weapon9.dat`；
- action40..55 共16帧，全部为 `state=1002 / hit_Fa=12`，并以55→40无限循环；
- OID124 的 ground candidate actions 60/62/63/64/70/72 为state1004，因此属于当前16个kind2 pickup
  target definitions之一；current有117条kind2 ITR / 39个holder definitions可建立关系；
- Naruto clone actions47/51/54的primary WPoint分别为
  `(weaponact35,dvx35)`、`(weaponact10,dvx18)`、`(weaponact23,dvx23)`；OID124声明35/10/23，
  所以三条均可先follow再按type4 DVX切入action40；
- 此外 Tenten action248直接以OPoint kind1/action40/dvx30生成OID124，Criminal2 action131直接以
  kind1/action40/dvx0生成OID124，不依赖held relation或其待修lifecycle。

当前正式 release runtime 的 `w/9.dat` 也具有同一16帧1002/hit_Fa12循环，Tenten release DAT同样直接生成
OID124 action40/dvx30；因此不是只存在于Unity旧内容的synthetic路径。

### 最早差异

以目标在右侧、OID124 action40、初始 `Vx=30` 为确定性见证：

1. 第一个 C02 tick：Authority执行 `+0.7` 后clamp为14；Unity先写 `WeaponState=2000`，再执行同一追踪并
   clamp为14。motion可能暂时相同，但Unity checksum/parity已因额外字段变化而首差。
2. 第二个 C02 tick：Authority从14加0.7并仍clamp为14；Unity先把14减半为7，再加0.7，得到约7.7。
   actual motion、随后位置、碰撞与表现开始分叉。

以 Criminal2 的 `Vx=0` 见证，右侧目标下前两次追踪Authority为0.7→1.4，Unity为0.7→1.05，
同样在第二次C02调用出现motion首差。实际frame始终是state1002；Authority不会把它变成2000/3000。

## 两个后继包

### 1. Behavior/producer retirement

`NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-001`

- 从 `LF2WeaponFrameLogicResolver` 删除 state1000 fast-to-40、1002→2000、2000 Vx-halving→3000 三条旧prelude；
- 删除 `ResolveRuntimeWeaponState()` / `GetRuntimeWeaponState()`，hit_Fa 逻辑继续只读 actual current frame；
- 删除held/drop/throw对`WeaponState`的动态写入；暂留字段、canonical copy、reset、fingerprint、checksum与
  parity位置为恒定legacy reserved 0，避免本包自行改变snapshot/recovery schema；
- 若 `WeaponBoomerangVxMin/Max` 在移除prelude后无生产引用，同包删除这两个旧常量和对应旧测试；
- 更正`FL-WEAPON-STATE`与held throw旧断言，新增OID124 formal两tick actual/field/checksum见证；
- 不改hit_Fa12目标选择/追踪常量、frame advance、physics、held relation、RNG、action40 release或content。

当前多个B6 production包和Unity实例仍占用runtime验证栈；本包在栈清理前保持held，不叠加脚本修改。

### 2. Carrier/schema disposition

`NTSD28-B6-LEGACY-WEAPON-STATE-CARRIER-DISPOSITION-001 / USER_DIRECTION_REQUIRED`

- 选项A：删除字段/copy/reset/fingerprint/checksum/parity key，并按append-only规则升级entity snapshot
  `12→13`、full snapshot `19→20`、checksum `22→23`及全部compatibility tests；
- 选项B：显式保留reserved 0位置和旧snapshot形状，定义旧snapshot中非零值的拒绝/归一化策略、
  authority-facing parity排除方式与兼容期限；
- `ReleaseTick`、`TrackerFlag/TrackerParent`、`GrabbedBy`与`HolderCopySlot` carrier disposition正请求同一组
  13/20/23迁移。若用户选择删除，这些字段应由一个具名联合schema migration一次完成，
  不能各自重复bump或产生中间伪兼容版本；`GrabbedBy`本身未进checksum，不单独触发22→23。
- snapshot/recovery/checksum policy属于standing authorization明确要求的新方向，本owner audit不替用户选择。

## 验收矩阵

- source guard：除reserved reset/copy/checksum/parity位置外，production `WeaponState` writer=0、reader=0；
- OID124 action40..55、目标左/右、初始Vx 30/0/负值：连续C02至少3 tick，frame state始终1002，
  无额外halving，motion与Authority逐tick一致；
- Tenten action248与Criminal2 action131真实OPoint生成；Naruto clone kind2 pickup后actions47/51/54三种DVX；
- synthetic state1000+positive-hitFa、state2000+positive-hitFa证明旧fast-to-40与halving均已退休；
- type1/2/4/6 held follow、DVX、kind3、damaged drop与refill不再写carrier，同时原action/motion/relation/RNG
  合同不变；
- carrier包若获批，覆盖snapshot capture/restore/schema mismatch、history/session checksum、parity JSON与
  4096 warmed copy/hash 0 B；
- compile、focused、B6/NTSD28 regression、SelfCheck、Tenten OID124 Play及same-seed joint trace。没有运行证据时
  最多`RUNTIME_PENDING`。

## 不变量 / 回滚

- 不修改Authority、Config/Scene/Prefab/resource/importer、pass placement、hit_Fa12本体、physics、RNG、
  relation/lifecycle或shutdown顺序。
- behavior包不删除carrier、不升级schema；carrier disposition未获用户方向前不实施。
- 本治理审计回滚仅删除新增记录与摘要；没有脚本、content、Scene或Authority回滚。
